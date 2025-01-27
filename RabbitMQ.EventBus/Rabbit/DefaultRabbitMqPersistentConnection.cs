using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.EventBus.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.EventBus.Rabbit
{
    public class DefaultRabbitMqPersistentConnection : IRabbitMqPersistenceConnection
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly int retryCount;
        private readonly object _syncRoot = new object();

        private IConnection _connection;
        private bool _disposed;

        public DefaultRabbitMqPersistentConnection(IConnectionFactory connectionFactory, int retryCount = 5)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            this.retryCount = retryCount;
        }

        public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<IChannel> CreateModel()
        {
            if (!IsConnected) {
                throw new InvalidOperationException("");
            }

            return await _connection.CreateChannelAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> TryConnect()
        {
            lock (_syncRoot)
            {
                var policy = Policy.Handle<SocketException>()
                                   .Or<BrokerUnreachableException>()
                                   .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                                    {
                      
                                    });
                policy.Execute(async () =>
                {
                    _connection = await _connectionFactory.CreateConnectionAsync();
                });

                if(!IsConnected)
                {
                    return false;
                }

                _connection.ConnectionShutdownAsync += OnConnectionShutdownAsync;
                _connection.CallbackExceptionAsync += OnCallbackExceptionAsync;
                _connection.ConnectionBlockedAsync +=  OnConnectionBlocked;

                return true;
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async Task OnConnectionShutdownAsync(object sender, ShutdownEventArgs e)
        {
            if(!_disposed)
            {
                return;
            }
            await TryConnect();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async Task OnCallbackExceptionAsync(object sender, CallbackExceptionEventArgs e)
        {
            if (!_disposed)
            {
                return;
            }
            await TryConnect();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async Task OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (!_disposed)
            {
                return;
            }
            await TryConnect();
        }
    }
}
