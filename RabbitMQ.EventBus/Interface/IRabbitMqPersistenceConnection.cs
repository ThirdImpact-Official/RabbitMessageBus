using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.EventBus.Interface
{
    public interface IRabbitMqPersistenceConnection
    {

        //bool IsConnected { get; }
        bool IsConnected { get; }
        /// <summary>
        /// provide a way to try to connect
        /// </summary>
        /// <returns></returns>
        Task<bool> TryConnect();
        /// <summary>
        /// create a channel
        /// </summary>
        /// <returns></returns>
        Task<IChannel> CreateModel();
    }
}
