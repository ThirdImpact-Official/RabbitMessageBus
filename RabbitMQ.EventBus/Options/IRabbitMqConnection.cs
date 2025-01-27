﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.EventBus.Options
{
    /// <summary>
    /// object parameter for Rabbit Mq connection
    /// </summary>
    public class IRabbitMqConnection
    {
        
        public string HostName { get; set; } = string.Empty;

        public string BrokerName = string.Empty;
        public string QueueName { get; set; } = string.Empty;
        public int Port { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } =string.Empty;

        public string VirtualHost { get; set; } = string.Empty;
        //
        public string RetryCount { get; set; } = string.Empty;
    }
}
