using System;
using System.Collections.Generic;
using System.Text;
using NServiceBus;

namespace SharedMessages
{
    public class Message2 : ICommand, IMessage
    {
        public string Content { get; set; }
    }
}
