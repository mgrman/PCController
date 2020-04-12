using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace CommandExecutor
{
    internal static class Program
    {
        private const string QueueName = "commandqueue";
        private const string ResponseQueueName = "responsequeue";
        private const string ServiceBusConnectionString = "Endpoint=sb://pccontroller.servicebus.windows.net/;SharedAccessKeyName=PCExecutorListenToCommands;SharedAccessKey=IDKuVvrFtHlhbH2l7sHYDAx/jc8vclZWF02Jj9aXZnw=";
        private static QueueClient queueClient;
        private static QueueClient responseClient;

        public static async Task Main(string[] args)
        {
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
            responseClient = new QueueClient(ServiceBusConnectionString, ResponseQueueName);

            Console.WriteLine("======================================================");
            Console.WriteLine("Press ENTER key to exit after receiving all the messages.");
            Console.WriteLine("======================================================");

            queueClient.RegisterMessageHandler(async (message, token) =>
            {
                var command = Encoding.UTF8.GetString(message.Body);
                Console.WriteLine($"Evaluating: {command} at {DateTime.Now}");
                if (command == "lock")
                {
                    System.Diagnostics.Process.Start(@"C:\WINDOWS\system32\rundll32.exe", "user32.dll,LockWorkStation");
                }
                else if (command == "sleep")
                {
                    System.Diagnostics.Process.Start(@"C:\WINDOWS\system32\rundll32.exe", "powrprof.dll,SetSuspendState 0,1,0");
                }
                else if (command == "isAwake")
                {
                    await responseClient.SendAsync(new Message() { Body = Encoding.UTF8.GetBytes("awake") });
                }
                else
                {
                    Console.WriteLine($"Unknown command: {command}");
                }
                await queueClient.CompleteAsync(message.SystemProperties.LockToken);
                return;
            }, new MessageHandlerOptions((ex) =>
            {
                return Task.CompletedTask;
            })
            {    // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                 // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                AutoComplete = true
            });

            Console.ReadKey();

            await queueClient.CloseAsync();
            await responseClient.CloseAsync();
        }
    }
}