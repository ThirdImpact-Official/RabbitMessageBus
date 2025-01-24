using HandleEvent.Interface;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using System;
using Event.Extension;
using Event.MesssageHandling;

namespace Event
{
    public class RabbitMQAdapter : IRabbitMQAdapter
    {
        public int retryCount = 5;
        public IConnection _connection;
        public bool IsConnected = false ;
        private IChannel consummerChannel;
        private const string exchangeName = "event_exchange";
        private const string CommandExcahnge = "command_exchange";
        public string queueName { get; set; }

        public event MessageReceived MessageReceived;
        ConnectionFactory ConnectionFactory = new ConnectionFactory() { HostName = "localhost" };
        
        //Constructor
        public RabbitMQAdapter(string endpointConnection)
        {
            queueName = endpointConnection;
        }

        private IConnection connection
        {
            get
            {
                if (!IsConnected)
                {
                    TryConnect();
                }
                return _connection;
            }
        }

        /// <summary>
        /// Send a command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="destination"></param>
        /// <exception cref="NotImplementedException"></exception>
        public  async Task BasicPublish(ICommand command, string destination)
        {
            await using (var channel = await connection.CreateChannelAsync())
            {
                // Correction : Utilisation de CommandExcahnge (votre typo originale)
                await channel.ExchangeDeclareAsync(CommandExcahnge, ExchangeType.Direct);

                await channel.BasicPublishAsync(
                    exchange: CommandExcahnge,
                    routingKey: destination,
                    mandatory: true,
                    basicProperties: new BasicProperties(),
                    body: command.ToJson().ToByteArray()
                );
            }
        }
        /// <summary>
        /// Send an event
        /// </summary>
        /// <param name="event"></param>
        /// <exception cref="NotImplementedException"></exception>
        public async Task BasicPublish(IEvent @event)
        {
            // Correction : Suppression du 's' après susing (probablement une faute de frappe)
            await using (var channel = await connection.CreateChannelAsync())
            {
                await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout);
                await channel.BasicPublishAsync(
                    exchange: exchangeName,
                    routingKey: "",
                    mandatory: true,
                    basicProperties: new BasicProperties(),
                    body: @event.ToJson().ToByteArray()
                );
            }
        }
        /// <summary>
        /// Start consuming
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public async Task StartConsuming()
        {
            consummerChannel = await connection.CreateChannelAsync();

            await consummerChannel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout);
            await consummerChannel.ExchangeDeclareAsync(CommandExcahnge, ExchangeType.Direct);

            // Correction : Utilisation de await au lieu de .Result
            var queue = await consummerChannel.QueueDeclareAsync(queueName);

            // Correction : Utilisation de await au lieu de .Wait()
            await consummerChannel.QueueBindAsync(queue, CommandExcahnge, queueName);
            await consummerChannel.QueueBindAsync(queue, exchangeName, queueName);

            var consumer = new AsyncEventingBasicConsumer(consummerChannel);

            await consummerChannel.BasicConsumeAsync(
                queue: queueName,
                autoAck: true,
                consumer: consumer
            );

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var messageBody = ea.Body.ToArray().GetPayload();
                var args = JsonConvert.DeserializeObject<MessageReceivedEventArgs>(messageBody);
                
                // Correction : Null-conditional operator pour éviter les exceptions
                MessageReceived?.Invoke(args);

            };
        }
        /// <summary>
        /// Try to connect
        /// </summary>
        public void TryConnect()
        {
           var policy = RetryPolicy.Handle<SocketException>().Or<BrokerUnreachableException>()
                                   .WaitAndRetry(retryCount, op=> TimeSpan.FromSeconds(Math.Pow(2,op)),(ex,time) =>
                                   { 
                                       Console.WriteLine($"Could not connect to RabbitMQ");
                                   });

            policy.Execute(() => 
            {
                _connection = ConnectionFactory.CreateConnectionAsync().Result;
                IsConnected = true;
                Console.WriteLine("Connected to RabbitMQ");
            });
        }
    }
}
