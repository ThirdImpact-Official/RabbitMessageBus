using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.EventBus.Configuration;
using RabbitMQ.EventBus.Options;

public class Program
{
    public static async void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        await host.RunAsync();
    }
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                IConfiguration Configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                                     .AddEnvironmentVariables()
                                                                     .AddJsonFile("appsettings.json")
                                                                     .Build();


                var options = Configuration.GetSection("RabbitMqConnection") as IRabbitMqConnection;
                services.AddRabbitMqEventBus(options);
                services.AddRabbitMqRegistration(options);
            });

}