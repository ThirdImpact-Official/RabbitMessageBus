using Event.MesssageHandling;
using HandleEvent.Interface;


namespace Event
{
    public delegate void MessageReceived(MessageReceivedEventArgs handler);
    
    public interface IRabbitMQAdapter
    {
        event MessageReceived MessageReceived;
        void TryConnect();
        Task BasicPublish(ICommand command, string destination);
        Task BasicPublish(IEvent @event);
        Task StartConsuming();
    }
}
