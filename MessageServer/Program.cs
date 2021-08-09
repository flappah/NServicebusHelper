using NServiceBus;
using PostSharp.Patterns.Diagnostics;
using PostSharp.Patterns.Diagnostics.Backends.Console;
using SharedMessages;
using System;
using System.Threading.Tasks;

namespace MessageServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var backend = new ConsoleLoggingBackend();
            backend.Options.Theme = ConsoleThemes.Dark;
            backend.Options.IncludeTimestamp = true;
            backend.Options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.ffff";
            LoggingServices.DefaultBackend = backend;

            var endpointConfiguration = new EndpointConfiguration("Demo.Message.Server");
            endpointConfiguration.UsePersistence<LearningPersistence>();
            endpointConfiguration.UseTransport<LearningTransport>();
            endpointConfiguration.PurgeOnStartup(true);

            IEndpointInstance endPointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            Console.WriteLine("Waiting for messages. Press any key to terminate server ..");

            Console.ReadKey();

            await endPointInstance.Stop().ConfigureAwait(false);
        }
    }

    public class MessageHandler : IHandleMessages<SharedMessages.Message>
    {
        public async Task Handle(SharedMessages.Message message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Received message with content '{message.Content}' ..");

            Guid correlationId = Guid.Empty;
            if (context.MessageHeaders.ContainsKey("CorrelationId"))
            {
                _ = Guid.TryParse(context.MessageHeaders["CorrelationId"], out correlationId);
            }

            var replyOptions = new ReplyOptions();
            replyOptions.SetHeader("CorrelationId", correlationId.ToString());
            await context.Reply<MessageReply>(m => m.Result = $"Reply to message with content '{message.Content}'", replyOptions).ConfigureAwait(false);
        }
    }

}
