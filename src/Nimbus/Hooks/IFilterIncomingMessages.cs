using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Hooks
{
    public interface IFilterIncomingMessages : IFilter
    {
        T FilterIn<T>(BrokeredMessage brokeredMessage, T originalMessage);
    }
}
