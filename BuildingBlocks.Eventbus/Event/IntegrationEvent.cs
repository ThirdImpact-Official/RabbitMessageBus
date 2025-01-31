using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Eventbus.Event
{
    public class IntegrationEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime CreateAt { get; } = DateTime.Now;
    }
}
