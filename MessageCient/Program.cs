using NServiceBus;
using PostSharp.Patterns.Diagnostics;
using PostSharp.Patterns.Diagnostics.Backends.Console;
using SharedMessages;
using System;
using System.Threading.Tasks;

namespace MessageCient
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

            var endpointConfiguration = new EndpointConfiguration("Demo.Message.Client");
            endpointConfiguration.UsePersistence<LearningPersistence>();
            endpointConfiguration.UseTransport<LearningTransport>();
            endpointConfiguration.PurgeOnStartup(true);
            endpointConfiguration
                .ExecuteTheseHandlersFirst(typeof(NServiceBusHelper<,>).MakeGenericType(typeof(Message), typeof(MessageReply)),
                                           typeof(NServiceBusHelper<,>).MakeGenericType(typeof(Message2), typeof(MessageReply2)));

            IEndpointInstance endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            while (true)
            {
                Console.WriteLine("Any key to send message. Enter to end ..");

                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }

                var message = new Message { Content = "TestContent" };
                
                Console.WriteLine("Message sent. Waiting for reply ..");

                var helper = new NServiceBusHelper<Message, MessageReply>();
                MessageReply result = 
                    await helper.GetMessage("Demo.Message.Server", endpointInstance, message, TimeSpan.FromMinutes(1)).ConfigureAwait(false);

                Console.WriteLine($"Reply received. Reply is '{result.Result}' ..");
            }

            await endpointInstance.Stop().ConfigureAwait(false);
        }
    }
}
