using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.HandlerFactories;
using Nimbus.Hooks;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.Commands
{
    internal class CommandMessageDispatcher : IMessageDispatcher
    {
        private readonly ICommandHandlerFactory _commandHandlerFactory;
        private readonly Type _commandType;
        private readonly IHookProvider _hookProvider;

        public CommandMessageDispatcher(ICommandHandlerFactory commandHandlerFactory, Type commandType, IHookProvider hookProvider)
        {
            _commandHandlerFactory = commandHandlerFactory;
            _commandType = commandType;
            _hookProvider = hookProvider;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var busCommand = message.GetBody(_commandType);
            busCommand = _hookProvider.Filters.ApplyToIncoming(message, busCommand);
            await Dispatch((dynamic) busCommand, message);
        }

        private async Task Dispatch<TBusCommand>(TBusCommand busCommand, BrokeredMessage message) where TBusCommand : IBusCommand
        {
            using (var handler = _commandHandlerFactory.GetHandler<TBusCommand>())
            {
                await handler.Component.Handle(busCommand);
            }
        }
    }
}