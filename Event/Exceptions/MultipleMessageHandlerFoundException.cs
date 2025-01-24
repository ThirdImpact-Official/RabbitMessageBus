using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Exceptions
{
    public class MultipleMessageHandlerFoundException : Exception
    {
        public MultipleMessageHandlerFoundException(string messageType) :
            base(string.Format("Multiple message handler found for the same message type",messageType))
        { }
    }
}
