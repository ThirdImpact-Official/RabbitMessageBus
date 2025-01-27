using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using EventBus.Base.Standard;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.EventBus.Interface;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Polly;
using System.Net.Sockets;

namespace RabbitMQ.Rabbit
{
    public class EventBusRabbitMq: IEventBus, IDisposable
    {
        private readonly IRabbitMqPersistenceConnection _persistentConnection;
        private readonly IEventBusSubscriptionManager _subsManager;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly string _brokerName;
        private readonly int _retryCount;

        private IChannel _consumerChannel;
        private string _queueName;

        public EventBusRabbitMq(IRabbitMqPersistenceConnection persistentConnection,
                                IServiceScopeFactory serviceScopeFactory,
                                IEventBusSubscriptionManager subsManager,
                                string brokerName,
                                string queueName = null,
                                int retryCount = 5)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _subsManager = subsManager ?? new InMemoryEventBusSubscriptionManager();
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

            _brokerName = brokerName;
            _queueName = queueName;
            _retryCount = retryCount;
            _subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
            InitializeConsumerChannel().GetAwaiter().GetResult();
        }

        private async Task InitializeConsumerChannel()
        {
            _consumerChannel = await CreateConsumerChannel();
            await StartBasicConsume();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventName"></param>
        /// <returns></returns>
        private async void SubsManager_OnEventRemoved(object sender, string eventName)
        {

            await using (var channel = await _persistentConnection.CreateModel())
            {
                await channel.QueueUnbindAsync(_queueName, _brokerName, eventName);

                if (!_subsManager.IsEmpty)
                {
                    _queueName = string.Empty;
                  _consumerChannel.CloseAsync().Wait();
                  
                }
            }
        }

        public async void Publish(IntegrationEvent @event)
        {
            if (!_persistentConnection.IsConnected)
            {
              await  _persistentConnection.TryConnect();
            }
            
            var policy = Policy.Handle<BrokerUnreachableException>()
                               .Or<SocketException>()
                               .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                    (ex, time) => { });

            var eventName = @event.GetType().Name;

           await using (var channel = await _persistentConnection.CreateModel())
            {
                await channel.ExchangeDeclareAsync(_brokerName, "direct");

                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);

                await policy.Execute(async () =>
                {
                    var properties = new BasicProperties();
                    properties.MessageId = Guid.NewGuid().ToString();
                    properties.DeliveryMode =(DeliveryModes)2 ;

                   await channel.BasicPublishAsync(_brokerName, eventName, true, properties, body);
                });
            }
        }

        public async void SubscribeDynamic<TH>(string eventName) where TH : IDynamicIntegrationEventHandler
        {
            DoInternalSubscription(eventName);

            _subsManager.AddDynamicSubscription<TH>(eventName);

            await StartBasicConsume();
        }

        public void Subscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subsManager.GetEventKey<T>();

            DoInternalSubscription(eventName);

            _subsManager.AddSubscription<T, TH>();

            _= StartBasicConsume();
        }

        private async void DoInternalSubscription(string eventName)
        {
            var containsKey = _subsManager.HasSubscriptionsForEvent(eventName);

            if (containsKey)
            {
                return;
            }

            if (!_persistentConnection.IsConnected)
            {
              await  _persistentConnection.TryConnect();
            }

            await using (var channel = await _persistentConnection.CreateModel())
            {
               await channel.QueueBindAsync(_queueName, _brokerName, eventName);
            }
        }

        public void Unsubscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
        {
            _subsManager.RemoveSubscription<T, TH>();
        }

        public void UnsubscribeDynamic<TH>(string eventName) where TH : IDynamicIntegrationEventHandler
        {
            _subsManager.RemoveDynamicSubscription<TH>(eventName);
        }

        public void Dispose()
        {
            _consumerChannel?.Dispose();

            _subsManager.Clear();
        }

        private async Task StartBasicConsume()
        {
            if (_consumerChannel == null)
            {
                return;
            }

            var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

            consumer.ReceivedAsync += Consumer_Received;

            await _consumerChannel.BasicConsumeAsync(_queueName, false, consumer);
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
        {
            var eventName = eventArgs.RoutingKey;
            var message = Encoding.UTF8.GetString(eventArgs.Body.Span.ToArray());

            try
            {
                if (message.ToLowerInvariant().Contains("throw-fake-exception"))
                {
                    throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
                }

                await ProcessEvent(eventName, message);
            }
            catch (Exception)
            {
                throw new Exception($"Error processing message \"{message}\"");
            }

            await _consumerChannel.BasicAckAsync(eventArgs.DeliveryTag, false);
        }

        private async Task<IChannel> CreateConsumerChannel()
        {
            if (!_persistentConnection.IsConnected)
            {
               await _persistentConnection.TryConnect();
            }

            var channel = await _persistentConnection.CreateModel();

             await channel.ExchangeDeclareAsync(_brokerName, "direct");
             await channel.QueueDeclareAsync(_queueName, true, false, false, null);

             channel.CallbackExceptionAsync += async (sender, ea) =>
            {
                await _consumerChannel.DisposeAsync();
                _consumerChannel = await CreateConsumerChannel();

               await  StartBasicConsume();
            };

            return channel;
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (!_subsManager.HasSubscriptionsForEvent(eventName))
            {
                throw new Exception("Submanager does not have subscription for the event.");
            }


            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            
            var subscriptions = _subsManager.GetHandlersForEvent(eventName);
            foreach (var subscription in subscriptions)
            {
                var handler = scope.ServiceProvider.GetService(subscription.HandlerType);
                if (handler != null)
                {
                    continue;
                }

                if (subscription.IsDynamic)
                {
                    await ProcessEvent(eventName, message);
                }
                else 
                {
                    await ProcessTypeEvent(handler,eventName, message);
                }
                 
            }
                
            
        }
        #region submethods
        private async Task ProcessDynamicEvent(IDynamicIntegrationEventHandler handler , string message)
        {
            if(handler == null) return;
            dynamic Eventdata= JObject.Parse(message);
            await handler.Handle(Eventdata);
        }
        private async Task ProcessTypeEvent(object handler,string eventName, string message)
        {
            var eventType= _subsManager.GetEventTypeByName(eventName);
            var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
            var concretetype =  typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
            await (Task)concretetype.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
        }

        #endregion
    }
}
