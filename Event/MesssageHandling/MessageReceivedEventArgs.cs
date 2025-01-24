using System;

namespace Event.MesssageHandling
{
    public  class MessageReceivedEventArgs : EventArgs
    {
        public string Type { get; set; }
        public object Payload { get; private set; }
        public DateTime PublishDate { get; private set; } 

        public MessageReceivedEventArgs(string type, object payload , DateTime PublisDate)
        {
            Type = type;
            Payload = payload;
            PublishDate = DateTime.Now;
        }   
    }
}
