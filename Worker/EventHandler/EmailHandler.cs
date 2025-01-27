using Event.MesssageHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Worker.Event;

namespace Worker.EventHandler
{
    public class EmailHandler :IMessageHandler<CustomerEmailChanged>
    {
      
        public EmailHandler()
        {
            
        }

        public Task Handle(CustomerEmailChanged message)
        {
            Console.WriteLine($"Email changed : Id {message.CustomerId} and Email  {message.NewEmail}");
            return Task.CompletedTask;
        }
    }
}
