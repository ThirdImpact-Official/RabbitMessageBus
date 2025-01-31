using BuildingBlocks.Eventbus.EventBus;

namespace Publisher.ExtensionMethods
{
    public static class StartEventBus
    {
        public static async Task StartBus(this WebApplication  app)
        {
            var eventBus = app.Services.GetRequiredService<IEventBus>();
            await eventBus.StartAsync();

            //registered event if needed

            await Task.CompletedTask;
        }
    }
}
