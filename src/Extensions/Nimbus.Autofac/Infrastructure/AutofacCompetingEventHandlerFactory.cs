﻿using System.Collections.Generic;
using Autofac;
using Autofac.Features.OwnedInstances;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Autofac.Infrastructure
{
    public class AutofacCompetingEventHandlerFactory : ICompetingEventHandlerFactory
    {
        private readonly ILifetimeScope _lifetimeScope;

        public AutofacCompetingEventHandlerFactory(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public OwnedComponent<IEnumerable<IHandleCompetingEvent<TBusEvent>>> GetHandler<TBusEvent>() where TBusEvent : IBusEvent
        {
            var handlers = _lifetimeScope.Resolve<Owned<IEnumerable<IHandleCompetingEvent<TBusEvent>>>>();
            return new OwnedComponent<IEnumerable<IHandleCompetingEvent<TBusEvent>>>(handlers.Value, handlers);
        }
    }
}