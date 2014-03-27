using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal class NimbusQueueMessageReceiver : INimbusMessageReceiver
    {
        private readonly IQueueManager _queueManager;
        private readonly string _queuePath;

        private readonly object _mutex = new object();
        private QueueClient _queueClient;
        private CancellationTokenSource _messagePumpCancellation;

        public NimbusQueueMessageReceiver(IQueueManager queueManager, string queuePath)
        {
            _queueManager = queueManager;
            _queuePath = queuePath;
        }

        public void Start(Func<BrokeredMessage, Task> callback)
        {
            lock (_mutex)
            {
                if (_queueClient != null) throw new InvalidOperationException("Already started!");

                _queueClient = _queueManager.CreateQueueClient(_queuePath);
                _messagePumpCancellation = _queueClient.CreateMessagePumps(Environment.ProcessorCount, callback);
            }
        }

        public void Stop()
        {
            lock (_mutex)
            {
                var queueClient = _queueClient;
                if (queueClient == null) return;

                queueClient.Close();
                _messagePumpCancellation.Cancel();
                _queueClient = null;
            }
        }

        public override string ToString()
        {
            return _queuePath;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            Stop();
        }
    }
}