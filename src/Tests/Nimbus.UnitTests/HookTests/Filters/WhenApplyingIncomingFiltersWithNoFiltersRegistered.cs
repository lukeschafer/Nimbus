using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.Hooks;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.HookTests.Filters
{
    [TestFixture]
    public class WhenApplyingIncomingFiltersWithNoFiltersRegistered : SpecificationFor<IFilterHooks>
    {
        private object _originalMessage;
        private object _filteredMessage;

        public override IFilterHooks Given()
        {
            _originalMessage = new object();
            return new FilterHooks();
        }

        public override void When()
        {
            _filteredMessage = Subject.ApplyToIncoming(new BrokeredMessage(), _originalMessage);
        }

        [Test]
        public void ShouldReturnOriginalWithoutError()
        {
            _filteredMessage.ShouldBeSameAs(_originalMessage);
        }
    }
}
