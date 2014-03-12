using System;
using System.Collections.Generic;

namespace Nimbus.Hooks
{
    public interface IFilterHooks
    {
        IList<Func<IFilterIncomingMessages>> IncomingGlobal { get; }
        IList<Func<IFilterOutgoingMessages>> OutgoingGlobal { get; }
        IDictionary<Type, IList<Func<IFilter>>> IncomingFor { get; }
        IDictionary<Type, IList<Func<IFilter>>> OutgoingFor { get; }

        IFilterHooks WithOutgoingMessageFilter(IFilterOutgoingMessages filter);
        IFilterHooks WithOutgoingMessageFilter(Func<IFilterOutgoingMessages> filterFactory);
        IFilterHooks WithIncomingMessageFilter(IFilterIncomingMessages filter);
        IFilterHooks WithIncomingMessageFilter(Func<IFilterIncomingMessages> filterFactory);

        IFilterHooks WithOutgoingMessageFilter<T>(IFilterOutgoingMessagesOf<T> filter);
        IFilterHooks WithOutgoingMessageFilter<T>(Func<IFilterOutgoingMessagesOf<T>> filterFactory);
        IFilterHooks WithIncomingMessageFilter<T>(IFilterIncomingMessagesOf<T> filter);
        IFilterHooks WithIncomingMessageFilter<T>(Func<IFilterIncomingMessagesOf<T>> filterFactory);

    }
    public class FilterHooks : IFilterHooks
    {
        public IList<Func<IFilterIncomingMessages>> IncomingGlobal { get; private set; }
        public IList<Func<IFilterOutgoingMessages>> OutgoingGlobal { get; private set; }
        public IDictionary<Type, IList<Func<IFilter>>> IncomingFor { get; private set; }
        public IDictionary<Type, IList<Func<IFilter>>> OutgoingFor { get; private set; }

        public FilterHooks()
        {
            IncomingGlobal = new List<Func<IFilterIncomingMessages>>();
            OutgoingGlobal = new List<Func<IFilterOutgoingMessages>>();
            IncomingFor = new Dictionary<Type, IList<Func<IFilter>>>();
            OutgoingFor = new Dictionary<Type, IList<Func<IFilter>>>();
        }

        public IFilterHooks WithOutgoingMessageFilter(IFilterOutgoingMessages filter)
        {
            OutgoingGlobal.Add(() => filter);
            return this;
        }

        public IFilterHooks WithOutgoingMessageFilter(Func<IFilterOutgoingMessages> filterFactory)
        {
            OutgoingGlobal.Add(filterFactory);
            return this;
        }

        public IFilterHooks WithIncomingMessageFilter(IFilterIncomingMessages filter)
        {
            IncomingGlobal.Add(() => filter);
            return this;
        }

        public IFilterHooks WithIncomingMessageFilter(Func<IFilterIncomingMessages> filterFactory)
        {
            IncomingGlobal.Add(filterFactory);
            return this;
        }

        public IFilterHooks WithOutgoingMessageFilter<T>(IFilterOutgoingMessagesOf<T> filter)
        {
            var type = typeof(T);
            if (!OutgoingFor.ContainsKey(type)) OutgoingFor[type] = new List<Func<IFilter>>();

            OutgoingFor[type].Add(() => filter);

            return this;
        }

        public IFilterHooks WithOutgoingMessageFilter<T>(Func<IFilterOutgoingMessagesOf<T>> filterFactory)
        {
            var type = typeof(T);
            if (!OutgoingFor.ContainsKey(type)) OutgoingFor[type] = new List<Func<IFilter>>();

            OutgoingFor[type].Add(filterFactory);

            return this;
        }

        public IFilterHooks WithIncomingMessageFilter<T>(IFilterIncomingMessagesOf<T> filter)
        {
            var type = typeof(T);
            if (!IncomingFor.ContainsKey(type)) IncomingFor[type] = new List<Func<IFilter>>();

            IncomingFor[type].Add(() => filter);

            return this;
        }

        public IFilterHooks WithIncomingMessageFilter<T>(Func<IFilterIncomingMessagesOf<T>> filterFactory)
        {
            var type = typeof(T);
            if (!IncomingFor.ContainsKey(type)) IncomingFor[type] = new List<Func<IFilter>>();

            IncomingFor[type].Add(filterFactory);

            return this;
        }
    }
}
