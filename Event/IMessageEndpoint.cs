using Event.MesssageHandling;
using HandleEvent.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event
{
    public interface IMessageEndpoint
    {
        void Start();
        void HandleMessage(MessageReceivedEventArgs args);
        void Send(ICommand command, string destination);
        void Publish(IEvent @event);
    }
}
