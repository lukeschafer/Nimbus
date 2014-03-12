using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.HandlerFactories;
using Nimbus.Hooks;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.Events
{
    public class CompetingEventMessageDispatcher : IMessageDispatcher
    {
        private readonly ICompetingEventHandlerFactory _competingEventHandlerFactory;
        private readonly Type _eventType;
        private readonly IHookProvider _hookProvider;

        public CompetingEventMessageDispatcher(ICompetingEventHandlerFactory competingEventHandlerFactory, Type eventType, IHookProvider hookProvider)
        {
            _competingEventHandlerFactory = competingEventHandlerFactory;
            _eventType = eventType;
            _hookProvider = hookProvider;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var busEvent = message.GetBody(_eventType);
            busEvent = _hookProvider.Filters.ApplyToIncoming(message, busEvent);
            await Dispatch((dynamic) busEvent, message);
        }

        private async Task Dispatch<TBusEvent>(TBusEvent busEvent, BrokeredMessage message) where TBusEvent : IBusEvent
        {
            using (var handlers = _competingEventHandlerFactory.GetHandlers<TBusEvent>())
            {
                await Task.WhenAll(handlers.Component.Select(h => h.Handle(busEvent)));
            }
        }
    }
}