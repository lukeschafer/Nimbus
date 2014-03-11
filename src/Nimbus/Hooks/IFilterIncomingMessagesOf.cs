using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Hooks
{
    public interface IFilterIncomingMessagesOf<T>
    {
        T FilterIn(BrokeredMessage brokeredMessage, T originalMessage);
    }
}
