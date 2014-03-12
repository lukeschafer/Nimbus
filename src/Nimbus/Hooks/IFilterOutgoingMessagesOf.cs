using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Hooks
{
    public interface IFilterOutgoingMessagesOf<T> : IFilter
    {
        T PreFilterOut(T message);
        BrokeredMessage FilterOut(BrokeredMessage brokeredMessage, T originalMessage);
    }
}
