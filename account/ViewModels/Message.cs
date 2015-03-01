using System;

namespace account.ViewModels
{
    public class Message
    {

        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public Message() { }
        public Message(string to, string subject, string body)
        {
            To = to;
            Subject = subject;
            Body = body;
        }
    }
}
