using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure
{
    public static class AzureExtensions
    {
        public static CancellationTokenSource CreateMessagePumps(this SubscriptionClient client, int count, Func<BrokeredMessage, Task> doAction)
        {
            var cancellation = new CancellationTokenSource();
            var taskFactory = new TaskFactory();
            for (var i = 0; i < count; i++)
                taskFactory.StartNew(() =>
                {
                    while (Thread.CurrentThread.IsAlive)
                    {
                        var msg = client.Receive(TimeSpan.FromSeconds(10));
                        if (msg != null) doAction(msg).Wait(cancellation.Token);
                    }
                }, cancellation.Token);
            return cancellation;
        }

        public static CancellationTokenSource CreateMessagePumps(this QueueClient client, int count, Func<BrokeredMessage, Task> doAction)
        {
            var cancellation = new CancellationTokenSource();
            var taskFactory = new TaskFactory();
            for (var i = 0; i < count; i++)
                taskFactory.StartNew(() =>
                {
                    while (Thread.CurrentThread.IsAlive)
                    {
                        var msg = client.Receive(TimeSpan.FromSeconds(10));
                        if (msg != null) doAction(msg).Wait(cancellation.Token);
                    }
                }, cancellation.Token);
            return cancellation;
        }
    }
}
