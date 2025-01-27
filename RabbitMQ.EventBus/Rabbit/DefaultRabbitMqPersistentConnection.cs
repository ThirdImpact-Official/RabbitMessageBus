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
    public class DefaultRabbitMqPersistentConnection : IRabbitMqPersistenceConnection, IDisposable
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly int _retryCount;
        private readonly object _syncRoot = new object();

        private IConnection _connection;
        private bool _disposed;

        public DefaultRabbitMqPersistentConnection(
                            IConnectionFactory connectionFactory,
                            int retryCount = 5)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _retryCount = retryCount;
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
            if (_disposed)
            {
                return false;
            }

            lock (_syncRoot)
            {
                if (IsConnected)
                {
                    return true;
                }

                var policy = Policy.Handle<SocketException>()
                                   .Or<BrokerUnreachableException>()
                                   .WaitAndRetryAsync(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                   (ex, time) =>
                                   {
                                      
                                           Console.WriteLine($"RabbitMQ Client could not connect after {time} seconds ({ex.Message})");
                                       
                                   });
                try
                {
                    policy.ExecuteAsync(async () =>
                    {
                        _connection = await _connectionFactory.CreateConnectionAsync();

                    }).GetAwaiter().GetResult();

                    if (IsConnected)
                    {
                        _connection.ConnectionShutdownAsync += OnConnectionShutdownAsync;
                        _connection.CallbackExceptionAsync += OnCallbackExceptionAsync;
                        _connection.ConnectionBlockedAsync += OnConnectionBlocked;
                        return true;
                    }
                }
                catch (Exception)
                {

                   return false;
                }

                return false;
            }   
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private Task OnConnectionShutdownAsync(object sender, ShutdownEventArgs e)
        {
            if (!_disposed)
            {
                return Task.CompletedTask;
            }
            return TryConnect();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private Task OnCallbackExceptionAsync(object sender, CallbackExceptionEventArgs e)
        {
            if (!_disposed)
            {
                return Task.CompletedTask;
            }
            return TryConnect();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private Task OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (!_disposed)
            {
                return Task.CompletedTask;
            }
            return TryConnect();
        }

        public void Dispose()
        {
           if(_disposed) return;

            try
            {
                _connection?.Dispose();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
