using System;
using System.Collections.Generic;
using System.Text;
using NServiceBus;

namespace Shared
{
    public class MessageReply : IMessage
    {
        public string Result { get; set; }
    }
}
