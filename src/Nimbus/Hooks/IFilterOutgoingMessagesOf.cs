using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Hooks
{
    public interface IFilterOutgoingMessagesOf<T>
    {
        T FilterOut(T message);
        BrokeredMessage FilterOut(BrokeredMessage brokeredMessage, T originalMessage);
    }
}
