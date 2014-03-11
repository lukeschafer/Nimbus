using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Hooks
{
    public interface IFilterIncomingMessages
    {
        T FilterIn<T>(BrokeredMessage brokeredMessage, T originalMessage);
    }
}
