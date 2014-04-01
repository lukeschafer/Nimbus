using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal class NimbusSubscriptionMessageReceiver : INimbusMessageReceiver
    {
        private readonly IQueueManager _queueManager;
        private readonly string _topicPath;
        private readonly string _subscriptionName;
        private readonly ILogger _logger;

        private SubscriptionClient _subscriptionClient;
        private readonly object _mutex = new object();
        private CancellationTokenSource _messagePumpCancellation;

        public NimbusSubscriptionMessageReceiver(IQueueManager queueManager, string topicPath, string subscriptionName, ILogger logger)
        {
            _queueManager = queueManager;
            _topicPath = topicPath;
            _subscriptionName = subscriptionName;
            _logger = logger;
        }

        public void Start(Func<BrokeredMessage, Task> callback)
        {
            lock (_mutex)
            {
                if (_subscriptionClient != null) throw new InvalidOperationException("Already started!");
                _subscriptionClient = _queueManager.CreateSubscriptionReceiver(_topicPath, _subscriptionName);
                _messagePumpCancellation = _subscriptionClient.CreateMessagePumps(Environment.ProcessorCount, callback, _logger);
            }
        }


        public void Stop()
        {
            lock (_mutex)
            {
                var subscriptionClient = _subscriptionClient;
                if (subscriptionClient == null) return;

                subscriptionClient.Close();
                _messagePumpCancellation.Cancel();
                _subscriptionClient = null;
            }
        }

        public override string ToString()
        {
            return "{0}/{1}".FormatWith(_topicPath, _subscriptionName);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}