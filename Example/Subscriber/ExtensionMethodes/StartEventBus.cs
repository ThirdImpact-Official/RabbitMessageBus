using BuildingBlocks.Eventbus.EventBus;
using Subscriber.Event.EventHandler;
using Subscriber.IntegrationEvents;

namespace Subscriber.ExtensionMethodes
{
    public static class StartEventBus
    {
        public static async Task StartBus(this WebApplication app)
        {
            //add the vent and the suvscription 
            var eventBus = app.Services.GetRequiredService<IEventBus>(); //b
                                                                         //start the connection to the bus 
            await eventBus.StartAsync();
            //subscription to the event bus
            await eventBus.SubscribeAsync<TestEvent, TestEventHandler>();

        }
    }
}
