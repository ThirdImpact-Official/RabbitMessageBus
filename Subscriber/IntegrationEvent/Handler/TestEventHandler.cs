using BuildingBlocks.Eventbus.Event;
using BuildingBlocks.Eventbus.EventBus;
using Subscriber.IntegrationEvents;



namespace Subscriber.Event.EventHandler
{
    public class TestEventHandler : IEventHandler<TestEvent>
    {
        private readonly IEventBus _eventBus;
        private readonly ILogger<TestEventHandler> _logger;
        public TestEventHandler(IEventBus eventBus, ILogger<TestEventHandler> logger)
        {
            _eventBus = eventBus;
            _logger = logger;
        }

        public Task Handle(TestEvent @event)
        {
            try
            {
                Console.WriteLine("Test event handled");
                Console.WriteLine(@event.Message);
                Console.WriteLine(@event.Id);
                Console.WriteLine(" congratulation you have succeeded");
                return Task.CompletedTask;

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
