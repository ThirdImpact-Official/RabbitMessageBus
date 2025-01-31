using BuildingBlocks.Eventbus.Event;

namespace Publisher.IntegrationEvents
{
    public class TestEvent :IntegrationEvent
    {
        public string Message { get; set; }
        public int EventId { get; set; }
        public TestEvent(string message, int eventId)
        {
            Message = message;
            EventId = eventId;
        }
    }
}
