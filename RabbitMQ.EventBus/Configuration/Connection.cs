using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.EventBus.Interface;
using RabbitMQ.EventBus.Options;
using RabbitMQ.EventBus.Rabbit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.EventBus.Configuration
{
    public static class Connection
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMqEventBus(this IServiceCollection services, IRabbitMqConnection options)
        {
            //Connection
            services.AddSingleton<IRabbitMqPersistenceConnection>(sp =>
            {
                var retryCount = 5;
                var factory = new ConnectionFactory()
                {
                    HostName = options.HostName,
                
                };

                if (!string.IsNullOrEmpty(options.UserName))
                {
                    factory.UserName = options.UserName;
                }

                if (!string.IsNullOrEmpty(options.Password))
                {
                    factory.Password =options.Password;
                }
                if (!string.IsNullOrEmpty(options.VirtualHost))
                {
                    factory.VirtualHost = options.VirtualHost;
                }
                if (!string.IsNullOrEmpty(options.RetryCount))
                {
                    retryCount= int.Parse(options.RetryCount);
                }
                return  new DefaultRabbitMqPersistentConnection(factory, retryCount);
            });

            return services;
        }
    }
}
