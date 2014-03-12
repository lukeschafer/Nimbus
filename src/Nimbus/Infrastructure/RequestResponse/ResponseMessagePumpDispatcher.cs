using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.Hooks;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class ResponseMessagePumpDispatcher : IMessageDispatcher
    {
        private readonly RequestResponseCorrelator _requestResponseCorrelator;
        private readonly IHookProvider _hookProvider;

        public ResponseMessagePumpDispatcher(RequestResponseCorrelator requestResponseCorrelator, IHookProvider hookProvider)
        {
            _requestResponseCorrelator = requestResponseCorrelator;
            _hookProvider = hookProvider;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var correlationId = Guid.Parse(message.CorrelationId);
            var responseCorrelationWrapper = _requestResponseCorrelator.TryGetWrapper(correlationId);
            if (responseCorrelationWrapper == null) return;

            var success = (bool) message.Properties[MessagePropertyKeys.RequestSuccessful];
            if (success)
            {
                var responseType = responseCorrelationWrapper.ResponseType;
                var response = message.GetBody(responseType);
                response = _hookProvider.Filters.ApplyToIncoming(message, response);
                responseCorrelationWrapper.Reply(response);
            }
            else
            {
                var exceptionMessage = (string) message.Properties[MessagePropertyKeys.ExceptionMessage];
                var exceptionStackTrace = (string) message.Properties[MessagePropertyKeys.ExceptionStackTrace];
                responseCorrelationWrapper.Throw(exceptionMessage, exceptionStackTrace);
            }
        }
    }
}