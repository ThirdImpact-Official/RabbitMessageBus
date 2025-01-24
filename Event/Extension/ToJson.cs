using HandleEvent.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Extension
{
    public static class ManageExtension
    {
        public static string ToJson(this IMessage message)
        {
            JObject OBject = new JObject();
            OBject.Add("type", message.GetType().Name);
            OBject.Add("publishDate", DateTime.Now);
            OBject.Add("payload",JsonConvert.SerializeObject(message));
            Console.WriteLine(OBject.ToString());
            return OBject.ToString();
        }
    }
}
