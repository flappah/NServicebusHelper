using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MessageCient
{
    public class NServiceBusHelper<Message, Reply> : IHandleMessages<Reply> where Message : IMessage where Reply : IMessage
    {
        public IMessageHandlerContext Context { get; set; }
        public Guid CorrelationId { get; set; }

        private static Dictionary<Guid, NServiceBusHelperItem> _itemList;

        /// <summary>
        ///     Sends a message and waits for the reply
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="message"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<Reply> GetMessage(IEndpointInstance endPoint, Message message, TimeSpan timeout)
        {
            if (_itemList == null)
            {
                _itemList = new Dictionary<Guid, NServiceBusHelperItem>();
            }

            Guid correlationId = Guid.NewGuid();
            if (CorrelationId != Guid.Empty)
            {
                correlationId = CorrelationId;
            }

            _itemList.Add(correlationId, new NServiceBusHelperItem { ResetEvent = new ManualResetEvent(false) });

            var sendOptions = new SendOptions();
            sendOptions.SetHeader("CorrelationId", correlationId.ToString());

            await endPoint.Send(message).ConfigureAwait(false);

            if (_itemList[correlationId].ResetEvent.WaitOne(timeout))
            {
                Reply reply = _itemList[correlationId].Reply;
                Context = _itemList[correlationId].Context;
                CorrelationId = correlationId;
                _itemList.Remove(correlationId);

                return reply;
            }

            return default(Reply);
        }

        /// <summary>
        ///     Sends a message and waits for the reply
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="endPoint"></param>
        /// <param name="message"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<Reply> GetMessage(string destination, IEndpointInstance endPoint, Message message, TimeSpan timeout)
        {
            if (_itemList == null)
            {
                _itemList = new Dictionary<Guid, NServiceBusHelperItem>();
            }

            Guid correlationId = Guid.NewGuid();
            if (CorrelationId != Guid.Empty)
            {
                correlationId = CorrelationId;
            }

            _itemList.Add(correlationId, new NServiceBusHelperItem { ResetEvent = new ManualResetEvent(false) });

            var sendOptions = new SendOptions();
            sendOptions.SetHeader("CorrelationId", correlationId.ToString());
            sendOptions.SetDestination(destination);

            await endPoint.Send(message, sendOptions).ConfigureAwait(false);

            if (_itemList[correlationId].ResetEvent.WaitOne(timeout))
            {
                Reply reply = _itemList[correlationId].Reply;
                Context = _itemList[correlationId].Context;
                CorrelationId = correlationId;
                _itemList.Remove(correlationId);

                return reply;
            }

            return default(Reply);
        }

        /// <summary>
        ///     Sends a message and waits for the reply
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="message"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<Reply> GetMessage(IMessageHandlerContext endPoint, Message message, TimeSpan timeout)
        {
            if (_itemList == null)
            {
                _itemList = new Dictionary<Guid, NServiceBusHelperItem>();
            }

            Guid correlationId = Guid.NewGuid();
            if (CorrelationId != Guid.Empty)
            {
                correlationId = CorrelationId;
            }

            _itemList.Add(correlationId, new NServiceBusHelperItem { ResetEvent = new ManualResetEvent(false) });

            var sendOptions = new SendOptions();
            sendOptions.SetHeader("CorrelationId", correlationId.ToString());

            await endPoint.Send(message, sendOptions).ConfigureAwait(false);

            if (_itemList[correlationId].ResetEvent.WaitOne(timeout))
            {
                Reply reply = _itemList[correlationId].Reply;
                Context = _itemList[correlationId].Context;
                CorrelationId = correlationId;
                _itemList.Remove(correlationId);

                return reply;
            }

            return default(Reply);
        }

        /// <summary>
        ///     Sends a message and waits for the reply
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="endPoint"></param>
        /// <param name="message"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<Reply> GetMessage(string destination, IMessageHandlerContext endPoint, Message message, TimeSpan timeout)
        {
            if (_itemList == null)
            {
                _itemList = new Dictionary<Guid, NServiceBusHelperItem>();
            }

            Guid correlationId = Guid.NewGuid();
            if (CorrelationId != Guid.Empty)
            {
                correlationId = CorrelationId;
            }

            _itemList.Add(correlationId, new NServiceBusHelperItem { ResetEvent = new ManualResetEvent(false) });

            var sendOptions = new SendOptions();
            sendOptions.SetHeader("CorrelationId", correlationId.ToString());
            sendOptions.SetDestination(destination);

            await endPoint.Send(message, sendOptions).ConfigureAwait(false);

            if (_itemList[correlationId].ResetEvent.WaitOne(timeout))
            {
                Reply reply = _itemList[correlationId].Reply;
                Context = _itemList[correlationId].Context;
                CorrelationId = correlationId;
                _itemList.Remove(correlationId);

                return reply;
            }

            return default(Reply);
        }

        /// <summary>
        ///     Generic message handler. Make sure to configure the endpoint to ensure that this handler
        ///     is called by using the ExecuteTheseHandlersFirst property! NServiceBus can't handle 
        ///     generic messages so it has to be told how to route the message. 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task Handle(Reply message, IMessageHandlerContext context)  
        {
            if(_itemList == null || _itemList.Count == 0)
            {
                return Task.CompletedTask;
            }

            // item matching is based on the supplied CorrelationId. If no correlationId can be determined the last item in the queue
            // is used to match on. There's a guaranteed reply delivery but if there is no correlationId can be determined no 
            // exact delivery can be guaranteed!
            Guid instanceId = Guid.Empty;
            if (context.MessageHeaders.ContainsKey("CorrelationId"))
            {
                _ = Guid.TryParse(context.MessageHeaders["CorrelationId"], out instanceId);
            }

            if (instanceId == Guid.Empty)
            {
                instanceId = _itemList.Last().Key;
            }

            _itemList[instanceId].Reply = message;
            _itemList[instanceId].Context = context;
            _itemList[instanceId].ResetEvent.Set();

            return Task.CompletedTask;
        }

        private class NServiceBusHelperItem
        { 
            public Reply Reply { get; set; }
            public IMessageHandlerContext Context { get; set; }
            public ManualResetEvent ResetEvent { get; set; }
        }
    }
}
