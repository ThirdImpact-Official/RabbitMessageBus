using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Eventbus.Event
{
    public interface IEventHandler<in TEvent> where TEvent: IntegrationEvent
    {
        Task Handle(TEvent @event);
    }
}
