using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Hooks;

namespace Nimbus.Extensions
{
    public static class HookExtensions
    {
        internal static T ApplyToIncoming<T>(this IFilterHooks me, BrokeredMessage brokeredMessage, T message)
        {
            if (me == null) return message;

            var messageType = message.GetType();

            //need to aggregate 2 lists of filters together by their defined order. No defined order = 0
            var filtersToApply = new List<KeyValuePair<int, Func<BrokeredMessage, object, object>>>();

            //get all global filters
            if (me.IncomingGlobal.Any())
                filtersToApply.AddRange(me.IncomingGlobal.Select(fac =>
                {
                    var filter = fac();
                    return new KeyValuePair<int, Func<BrokeredMessage, object, object>>(filter.Order.GetValueOrDefault(), filter.FilterIn);
                }));

            //get all typed filters
            if (me.IncomingFor.ContainsKey(messageType) && me.IncomingFor[messageType].Any())
                filtersToApply.AddRange(me.IncomingFor[messageType].Select(fac =>
                {
                    var filter = fac();
                    return new KeyValuePair<int, Func<BrokeredMessage, object, object>>(filter.Order.GetValueOrDefault(),
                        (brokered, msg) =>
                        {
                            var method = filter.GetType().GetMethod("FilterIn");//.MakeGenericMethod(messageType);
                            return method.Invoke(filter, new[] { brokered, msg });
                        });
                }));

            //apply in order
            return filtersToApply.OrderBy(f => f.Key).Aggregate(message, (current, filter) => (T)filter.Value(brokeredMessage, current));
        }

        internal static T ApplyToOutgoingBeforeConversion<T>(this IFilterHooks me, T message)
        {
            if (me == null) return message;

            var messageType = message.GetType();

            //need to aggregate 2 lists of filters together by their defined order. No defined order = 0
            var filtersToApply = new List<KeyValuePair<int, Func<object, object>>>();

            //get all global filters
            if (me.OutgoingGlobal.Any())
                filtersToApply.AddRange(me.OutgoingGlobal.Select(fac =>
                {
                    var filter = fac();
                    return new KeyValuePair<int, Func<object, object>>(filter.Order.GetValueOrDefault(), filter.PreFilterOut);
                }));

            //get all typed filters
            if (me.OutgoingFor.ContainsKey(messageType) && me.OutgoingFor[messageType].Any())
                filtersToApply.AddRange(me.OutgoingFor[messageType].Select(fac =>
                {
                    var filter = fac();
                    return new KeyValuePair<int, Func<object, object>>(filter.Order.GetValueOrDefault(),
                        msg =>
                        {
                            var method = filter.GetType().GetMethod("PreFilterOut");//.MakeGenericMethod(messageType);
                            return method.Invoke(filter, new[] { msg });
                        });
            }));

            //apply in order
            return filtersToApply.OrderBy(f => f.Key).Aggregate(message, (current, filter) => (T)filter.Value(current));
        }

        internal static BrokeredMessage ApplyToOutgoingAfterConversion(this IFilterHooks me, BrokeredMessage brokeredMessage, object originalMessage)
        {
            if (me == null) return brokeredMessage;

            var messageType = originalMessage.GetType();

            //need to aggregate 2 lists of filters together by their defined order. No defined order = 0
            var filtersToApply = new List<KeyValuePair<int, Func<BrokeredMessage, object, BrokeredMessage>>>();

            //get all global filters
            if (me.OutgoingGlobal.Any())
                filtersToApply.AddRange(me.OutgoingGlobal.Select(fac =>
                {
                    var filter = fac();
                    return new KeyValuePair<int, Func<BrokeredMessage, object, BrokeredMessage>>(filter.Order.GetValueOrDefault(), filter.FilterOut);
                }));

            //get all typed filters
            if (me.OutgoingFor.ContainsKey(messageType) && me.OutgoingFor[messageType].Any())
                filtersToApply.AddRange(me.OutgoingFor[messageType].Select(fac =>
                {
                    var filter = fac();
                    return new KeyValuePair<int, Func<BrokeredMessage, object, BrokeredMessage>>(filter.Order.GetValueOrDefault(),
                        (brokered, msg) =>
                        {
                            var method = filter.GetType().GetMethod("FilterOut");//.MakeGenericMethod(messageType);
                            return (BrokeredMessage)method.Invoke(filter, new[] { brokered, msg });
                        });
                }));

            //apply in order
            return filtersToApply.OrderBy(f => f.Key).Aggregate(brokeredMessage, (current, filter) => filter.Value(current, originalMessage));
        }
    }
}
