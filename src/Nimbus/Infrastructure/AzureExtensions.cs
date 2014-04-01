using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure
{
    public static class AzureExtensions
    {
        public static CancellationTokenSource CreateMessagePumps(this SubscriptionClient client, int count, Func<BrokeredMessage, Task> doAction, ILogger logger)
        {
            var cancellation = new CancellationTokenSource();
            var taskFactory = new TaskFactory();
            for (var i = 0; i < count; i++)
                taskFactory.StartNew(() =>
                {
                    try
                    {
                        logger.Debug("Starting Subscription Message Pump Thread: {0}", Thread.CurrentThread.ManagedThreadId);
                        while (Thread.CurrentThread.IsAlive)
                        {
                            var msg = client.Receive(TimeSpan.FromSeconds(10));
                            if (msg != null)
                            {
                                logger.Debug("Got Subscription Message");
                                doAction(msg).Wait(cancellation.Token);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Fatal issue in Subscription Message Pump! This has caused at least one pump thread to die! Thread: {0}", Thread.CurrentThread.ManagedThreadId);
                    }
                }, cancellation.Token);
            return cancellation;
        }

        public static CancellationTokenSource CreateMessagePumps(this QueueClient client, int count, Func<BrokeredMessage, Task> doAction, ILogger logger)
        {
            var cancellation = new CancellationTokenSource();
            var taskFactory = new TaskFactory();
            for (var i = 0; i < count; i++)
                taskFactory.StartNew(() =>
                {
                    try
                    {
                        logger.Debug("Starting Queue Message Pump Thread: {0}", Thread.CurrentThread.ManagedThreadId);
                        while (Thread.CurrentThread.IsAlive)
                        {
                            var msg = client.Receive(TimeSpan.FromSeconds(10));
                            {
                                logger.Debug("Got Queue Message");
                                doAction(msg).Wait(cancellation.Token);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Fatal issue in Queue Message Pump! This has caused at least one pump thread to die! Thread: {0}", Thread.CurrentThread.ManagedThreadId);
                    }
                }, cancellation.Token);
            return cancellation;
        }
    }
}
