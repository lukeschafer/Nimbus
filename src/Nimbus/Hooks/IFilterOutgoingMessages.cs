using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Hooks
{
    public interface IFilterOutgoingMessages
    {
        T FilterOut<T>(T message);
        BrokeredMessage FilterOut<T>(BrokeredMessage brokeredMessage, T originalMessage);
    }
}
