using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Exceptions
{
    public class MessageHandlerNotFoundException :Exception
    {
        public MessageHandlerNotFoundException( string messageType) :
            base(string.Format("Message handler not found for the messate type : {0}",messageType))
        { }
    }
}
