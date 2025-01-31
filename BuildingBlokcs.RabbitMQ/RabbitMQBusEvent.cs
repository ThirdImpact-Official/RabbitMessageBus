using BuildIngBlocks.Event.EventBus;
using BuildingBlokcs.RabbitMQ.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace BuildingBlocks.RabbitMQ
{
    public class RabbitMQBus : IEventBus
    {
        private readonly ILogger<RabbitMQBus> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _exchangeName;
        private readonly SemaphoreSlim _semaphore;
        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly IConnectionFactory _connectionFactory;
        private IConnection? _connection;
        private IChannel? _channel;
        private bool _disposed;

        public RabbitMQBus(string exchangeName,
            IServiceProvider serviceProvider,
                           ILogger<RabbitMQBus> logger,
                           RabbitMqOption options)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _exchangeName = exchangeName;
            _semaphore = new SemaphoreSlim(1, 1);
            _handlers = new Dictionary<string, List<Type>>();
            _connectionFactory = new ConnectionFactory()
            {

                HostName = options.HostName,
                Password = options.Password,
                UserName = options.UserName,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };
        }
        /// <summary>
        /// Dispose
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            _disposed = true;

            if (_channel != null && _channel.IsOpen)
            {
                await _channel.CloseAsync();
                _channel.Dispose();
            }

            if (_connection != null && _connection.IsOpen)
            {
                await _connection.CloseAsync();
                _connection.Dispose();
            }

            _semaphore.Dispose();
        }
        /// <summary>
        /// Publish an event on the bus
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="event"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : IntegrationEvent
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RabbitMQBus));

            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                if (_channel == null || !_channel.IsOpen)
                {
                    throw new Exception("Channel is not open");
                }
                //TODO: Publish
                var eventName = @event.GetType().Name;
                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));
                // Create basic Properties
                var properties = new BasicProperties
                {
                    Persistent = true, // Ensure messages are persisted to disk
                    ContentType = "application/json",
                    ContentEncoding = "UTF-8"
                };

                //
                await _channel.BasicPublishAsync(exchange: _exchangeName,
                                                 routingKey: eventName,
                                                 mandatory: true,
                                                 basicProperties: properties,
                                                 body: body);

                _logger.LogInformation("Published event: {EventName}", eventName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish event: {EventName}", @event.GetType().Name);
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }
        /// <summary>
        /// Subscribe to an event 
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="THandler"></typeparam>
        /// <param name="handler"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public async Task SubscribeAsync<TEvent, THandler>(CancellationToken cancellationToken = default)
            where TEvent : IntegrationEvent
            where THandler : IEventHandler<TEvent>
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RabbitMQBus));

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                var eventName = typeof(TEvent).Name;

                if (!_handlers.ContainsKey(eventName))
                {
                    _handlers[eventName] = new List<Type>();
                }

                _handlers[eventName].Add(typeof(THandler));

                var queueName = $"{_exchangeName}.{eventName}";
                _ = await _channel.QueueDeclareAsync(queueName,
                                                     true,
                                                     false,
                                                     false,
                                                     null);
                await _channel.QueueBindAsync(queueName, _exchangeName, eventName);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var @event = JsonConvert.DeserializeObject<TEvent>(message);

                        if (@event != null)
                        {
                            using var scope = _serviceProvider.CreateScope();
                            foreach (var handlerType in _handlers[eventName])
                            {
                                var handler = scope.ServiceProvider.GetRequiredService(handlerType) as IEventHandler<TEvent>;
                                if (handler != null)
                                {
                                    await handler.Handle(@event);
                                }
                            }
                        }

                        await _channel.BasicAckAsync(ea.DeliveryTag, false); // Acknowledge the message
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process event: {EventName}", eventName);
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, true); // Reject and requeue the message
                    }
                };

                await _channel.BasicConsumeAsync(queueName, false, consumer); // Start consuming messages
                _logger.LogInformation("Subscribed to event: {EventName}", eventName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to subscribe to event: {EventName}", typeof(TEvent).Name);
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }
        /// <summary>
        /// Unsubscribe from an event
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="THandler"></typeparam>
        /// <param name="handler"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public async Task UnsubscribeAsync<TEvent, THandler>(CancellationToken cancellationToken = default)
            where TEvent : IntegrationEvent
            where THandler : IEventHandler<TEvent>
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RabbitMQBus));

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                var eventName = typeof(TEvent).Name;

                if (_handlers.ContainsKey(eventName))
                {
                    _handlers[eventName].Remove(typeof(THandler));

                    if (_handlers[eventName].Count == 0)
                    {
                        _handlers.Remove(eventName);

                        var queueName = $"{_exchangeName}.{eventName}";
                        await _channel.QueueUnbindAsync(queueName, _exchangeName, eventName, null);
                        await _channel.QueueDeleteAsync(queueName);

                        _logger.LogInformation("Unsubscribed from event: {EventName}", eventName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unsubscribe from event: {EventName}", typeof(TEvent).Name);
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _connection = await _connectionFactory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();
                await _channel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Direct, true);
                _logger.LogInformation("RabbitMQ bus started successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start RabbitMQ bus.");
                throw;
            }
        }
    }
}