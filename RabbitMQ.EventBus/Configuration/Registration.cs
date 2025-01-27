using EventBus.Base.Standard;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.EventBus.Interface;
using RabbitMQ.EventBus.Options;
using RabbitMQ.Rabbit;

namespace RabbitMQ.EventBus.Configuration
{
    public static class Registration
    {
        /// <summary>
        /// adds configure RabbitMQ Event Bus  services  to the service collection
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMqRegistration(this IServiceCollection services, IRabbitMqConnection  options)
        {
            //Connection
            services.AddSingleton<IEventBus, EventBusRabbitMq>(sp =>
            {
                //Connection
                var rabbitMqPersistenceConnection = sp.GetRequiredService<IRabbitMqPersistenceConnection>();
                var lifetimeScope = sp.GetRequiredService<IServiceScopeFactory>();
                var eventBusSubcriber = sp.GetRequiredService<IEventBusSubscriptionManager>();

                var broken= options.BrokerName;
                var queueName = options.QueueName;
                var retryCount =  5;

                if (!string.IsNullOrEmpty(options.RetryCount))
                {
                    retryCount = int.Parse(options.RetryCount);
                }

                return new EventBusRabbitMq(rabbitMqPersistenceConnection, 
                                            lifetimeScope,
                                            eventBusSubcriber,
                                            queueName,
                                            broken,
                                            retryCount);
            });
            services.AddSingleton<IEventBusSubscriptionManager, InMemoryEventBusSubscriptionManager>();
            return services;
        }
    }
}
