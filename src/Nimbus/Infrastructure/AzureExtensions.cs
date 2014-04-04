using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure
{
    public static class AzureExtensions
    {
        public static void CreateMessagePumps(this SubscriptionClient client, int count, Func<BrokeredMessage, Task> doAction, ILogger logger)
        {
            for (var i = 0; i < count; i++)
            {
                var t = new Task(() =>
                {
                    logger.Debug("Starting Subscription Message Pump Thread: {0}", Thread.CurrentThread.ManagedThreadId);
                    while (true)
                    {
                        try
                        {
                            var msg = client.Receive(TimeSpan.FromSeconds(10));
                            if (msg == null) continue;
                            logger.Debug("Got Subscription Message");
                            doAction(msg).Wait();
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, "Issue in Subscription Message Pump! Thread: {0}", Thread.CurrentThread.ManagedThreadId);
                        }
                    }
                });
                t.ContinueWith((thr, o) =>
                {
                    if (thr.Exception != null) logger.Error(thr.Exception, "Fatal Issue in Subscription Message Pump Thread: {0}", Thread.CurrentThread.ManagedThreadId);
                }, new object());
                t.Start();
            }
        }

        public static void CreateMessagePumps(this QueueClient client, int count, Func<BrokeredMessage, Task> doAction, ILogger logger)
        {
            for (var i = 0; i < count; i++)
            {
                var t = new Task(() =>
                {
                    logger.Debug("Starting Queue Message Pump Thread: {0}", Thread.CurrentThread.ManagedThreadId);
                    while (true)
                    {
                        try
                        {
                            var msg = client.Receive(TimeSpan.FromSeconds(10));
                            if (msg == null) continue;
                            logger.Debug("Got Queue Message");
                            doAction(msg).Wait();
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, "Issue in Queue Message Pump! Thread: {0}",
                                Thread.CurrentThread.ManagedThreadId);
                        }
                    }
                });

                t.ContinueWith((thr, o) =>
                {
                    if (thr.Exception != null) logger.Error(thr.Exception, "Fatal Issue in Queue Message Pump Thread: {0}", Thread.CurrentThread.ManagedThreadId);
                }, new object());
                t.Start();
            }
        }
    }
}
