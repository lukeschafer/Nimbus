using System;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.Hooks;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.HookTests.Filters
{
    [TestFixture]
    public class WhenApplyingOutgoingFiltersAfterConversionWithNoFiltersRegistered : SpecificationFor<IFilterHooks>
    {
        private BrokeredMessage _originalMessage;
        private BrokeredMessage _filteredMessage;

        public override IFilterHooks Given()
        {
            _originalMessage = new BrokeredMessage();
            return new FilterHooks();
        }

        public override void When()
        {
            _filteredMessage = Subject.ApplyToOutgoingAfterConversion(_originalMessage, new Object());
        }

        [Test]
        public void ShouldReturnOriginalWithoutError()
        {
            _filteredMessage.ShouldBeSameAs(_originalMessage);
        }
    }
}
