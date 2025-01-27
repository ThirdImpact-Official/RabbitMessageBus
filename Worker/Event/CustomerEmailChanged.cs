using HandleEvent.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Worker.Event
{
    public class CustomerEmailChanged : IEvent
    {
        public CustomerEmailChanged(int customerId, string newEmail)
        {
            CustomerId = customerId;
            NewEmail = newEmail;
        }

        public int CustomerId { get; set; }
        public string NewEmail { get; set; }
    }
}
