using System;
using System.Collections.Generic;
using System.Text;
using NServiceBus;

namespace SharedMessages
{
    public class MessageReply2 : IMessage
    {
        public string Result { get; set; }
    }
}
