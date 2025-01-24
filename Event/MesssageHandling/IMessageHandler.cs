using HandleEvent.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.MesssageHandling
{
    public interface IMessageHandler<T> where T : IMessage
    {
        Task Handle(T message);
    }
}
