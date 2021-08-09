using System;
using System.Collections.Generic;
using System.Text;
using NServiceBus;

namespace Shared
{
    public class Message : ICommand, IMessage
    {
        public string Content { get; set; }
    }
}
