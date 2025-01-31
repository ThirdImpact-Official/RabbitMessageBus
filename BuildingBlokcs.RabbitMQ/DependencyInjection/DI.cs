using BuildingBlokcs.RabbitMQ.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;


namespace BuildingBlokcs.RabbitMQ.DependencyInjection
{
    public static class DI
    {
        public static IServiceCollection AddBuildingBlocksRabbitMQ(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetSection("EventBus");
            services.Configure<RabbitMqOption>(sp =>
            {
                sp.HostName = options["HostName"];
                sp.UserName = options["UserName"];
                sp.Password = options["Password"];
            });

            services.AddSingleton<IEventBus, RabbitMQBus>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<RabbitMQBus>>();
                var options = sp.GetRequiredService<IOptions<RabbitMqOption>>().Value;
                return new RabbitMQBus("my_exchange", logger, options);
            });

            return services;
        }
    }
}
