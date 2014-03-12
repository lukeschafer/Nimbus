using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Hooks
{
    public interface IFilterIncomingMessagesOf<T> : IFilter
    {
        T FilterIn(BrokeredMessage brokeredMessage, T originalMessage);
    }
}
