using HandleEvent.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sender.Command
{
    public class CreateOrderCommand :ICommand
    {
        public CreateOrderCommand(int OrderId, string Name)
        {
            this.OrderId = OrderId;
            this.Name = Name;
        }

        public int OrderId { get; set; }
        public string Name { get; set; }=string.Empty;
    }
}
