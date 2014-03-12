using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Hooks
{
    public interface IFilterOutgoingMessages : IFilter
    {
        T PreFilterOut<T>(T message);
        BrokeredMessage FilterOut<T>(BrokeredMessage brokeredMessage, T originalMessage);
    }
}
