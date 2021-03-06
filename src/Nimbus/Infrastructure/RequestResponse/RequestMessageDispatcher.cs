﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.Interceptors.Inbound;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class RequestMessageDispatcher : IMessageDispatcher
    {
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IClock _clock;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly Type _handlerType;
        private readonly IInboundInterceptorFactory _inboundInterceptorFactory;
        private readonly ILogger _logger;
        private readonly Type _messageType;
        private readonly INimbusMessagingFactory _messagingFactory;

        public RequestMessageDispatcher(
            INimbusMessagingFactory messagingFactory,
            IBrokeredMessageFactory brokeredMessageFactory,
            IInboundInterceptorFactory inboundInterceptorFactory,
            Type messageType,
            IClock clock,
            ILogger logger,
            IDependencyResolver dependencyResolver,
            Type handlerType)
        {
            _messagingFactory = messagingFactory;
            _brokeredMessageFactory = brokeredMessageFactory;
            _inboundInterceptorFactory = inboundInterceptorFactory;
            _messageType = messageType;
            _clock = clock;
            _logger = logger;
            _dependencyResolver = dependencyResolver;
            _handlerType = handlerType;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var request = await _brokeredMessageFactory.GetBody(message, _messageType);
            var dispatchMethod = GetGenericDispatchMethodFor(request);
            await (Task) dispatchMethod.Invoke(this, new[] {request, message});
        }

        // ReSharper disable UnusedMember.Local
        private async Task Dispatch<TBusRequest, TBusResponse>(TBusRequest busRequest, BrokeredMessage message)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
            var replyQueueName = message.ReplyTo;
            var replyQueueClient = _messagingFactory.GetQueueSender(replyQueueName);

            Exception exception = null;
            using (var scope = _dependencyResolver.CreateChildScope())
            {
                var handler = scope.Resolve<IHandleRequest<TBusRequest, TBusResponse>>(_handlerType.FullName);
                var interceptors = _inboundInterceptorFactory.CreateInterceptors(scope, handler,
                    busRequest);

                foreach (var interceptor in interceptors)
                {
                    _logger.Debug("Executing OnRequestHandlerExecuting on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]", 
                        interceptor.GetType().FullName, 
                        message.SafelyGetBodyTypeNameOrDefault(), 
                        message.MessageId, 
                        message.CorrelationId);
                    await interceptor.OnRequestHandlerExecuting(busRequest, message);
                    _logger.Debug("Executed OnRequestHandlerExecuting on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                        interceptor.GetType().FullName,
                        message.SafelyGetBodyTypeNameOrDefault(),
                        message.MessageId,
                        message.CorrelationId);
                }

                try
                {
                    var handlerTask = handler.Handle(busRequest);
                    var wrapperTask = new LongLivedTaskWrapper<TBusResponse>(handlerTask, handler as ILongRunningTask,
                        message, _clock);
                    var response = await wrapperTask.AwaitCompletion();

                    var responseMessage =
                        await _brokeredMessageFactory.CreateSuccessfulResponse(response, message);

                    _logger.Debug("Sending successful response message {0} to {1} [MessageId:{2}, CorrelationId:{3}]",
                        responseMessage.SafelyGetBodyTypeNameOrDefault(),
                        replyQueueName,
                        message.MessageId,
                        message.CorrelationId);
                    await replyQueueClient.Send(responseMessage);
                    _logger.Info("Sent successful response message {0} to {1} [MessageId:{2}, CorrelationId:{3}]",
                        message.SafelyGetBodyTypeNameOrDefault(),
                        replyQueueName,
                        message.MessageId,
                        message.CorrelationId);
                    
                }
                catch (Exception exc)
                {
                    // Capture any exception so we can send a failed response outside the catch block
                    exception = exc;
                }
                if (exception == null)
                {
                    foreach (var interceptor in interceptors.Reverse())
                    {
                        _logger.Debug("Executing OnRequestHandlerSuccess on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                        interceptor.GetType().FullName,
                        message.SafelyGetBodyTypeNameOrDefault(),
                        message.MessageId,
                        message.CorrelationId);

                        await interceptor.OnRequestHandlerSuccess(busRequest, message);
                        
                        _logger.Debug("Executed OnRequestHandlerSuccess on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                        interceptor.GetType().FullName,
                        message.SafelyGetBodyTypeNameOrDefault(),
                        message.MessageId,
                        message.CorrelationId);
                    }
                }
                else
                {
                    foreach (var interceptor in interceptors.Reverse())
                    {
                        _logger.Debug("Executing OnRequestHandlerError on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                        interceptor.GetType().FullName,
                        message.SafelyGetBodyTypeNameOrDefault(),
                        message.MessageId,
                        message.CorrelationId);

                        await interceptor.OnRequestHandlerError(busRequest, message, exception);

                        _logger.Debug("Executed OnRequestHandlerError on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                        interceptor.GetType().FullName,
                        message.SafelyGetBodyTypeNameOrDefault(),
                        message.MessageId,
                        message.CorrelationId);

                    }

                    var failedResponseMessage =
                        await _brokeredMessageFactory.CreateFailedResponse(message, exception);

                    _logger.Warn("Sending failed response message to {0} [MessageId:{1}, CorrelationId:{2}]",
                        replyQueueName,
                        exception.Message,
                        message.MessageId,
                        message.CorrelationId);
                    await replyQueueClient.Send(failedResponseMessage);
                    _logger.Info("Sent failed response message to {0} [MessageId:{1}, CorrelationId:{2}]",
                        replyQueueName,
                        message.MessageId,
                        message.CorrelationId);
                }
            }
        }

        // ReSharper restore UnusedMember.Local

        internal static MethodInfo GetGenericDispatchMethodFor(object request)
        {
            var closedGenericTypeOfIBusRequest = request.GetType()
                .GetInterfaces()
                .Where(t => t.IsClosedTypeOf(typeof (IBusRequest<,>)))
                .Single();

            var genericArguments = closedGenericTypeOfIBusRequest.GetGenericArguments();
            var requestType = genericArguments[0];
            var responseType = genericArguments[1];

            var openGenericMethod = typeof (RequestMessageDispatcher).GetMethod("Dispatch",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var closedGenericMethod = openGenericMethod.MakeGenericMethod(new[] {requestType, responseType});
            return closedGenericMethod;
        }
    }
}