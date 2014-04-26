﻿using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Interceptors.Inbound;

namespace Nimbus.IntegrationTests.Tests.InterceptorTests.Interceptors
{
    public class SomeBaseMethodLevelInterceptorForBar : InboundInterceptor
    {
        public override async Task OnCommandHandlerExecuting<TBusCommand>(TBusCommand busCommand, BrokeredMessage brokeredMessage)
        {
            MethodCallCounter.RecordCall<SomeBaseMethodLevelInterceptorForBar>(h => h.OnCommandHandlerExecuting(busCommand, brokeredMessage));
        }

        public override async Task OnCommandHandlerSuccess<TBusCommand>(TBusCommand busCommand, BrokeredMessage brokeredMessage)
        {
            MethodCallCounter.RecordCall<SomeBaseMethodLevelInterceptorForBar>(h => h.OnCommandHandlerSuccess(busCommand, brokeredMessage));
        }

        public override async Task OnCommandHandlerError<TBusCommand>(TBusCommand busCommand, BrokeredMessage brokeredMessage, Exception exception)
        {
            MethodCallCounter.RecordCall<SomeBaseMethodLevelInterceptorForBar>(h => h.OnCommandHandlerError(busCommand, brokeredMessage, exception));
        }
    }
}