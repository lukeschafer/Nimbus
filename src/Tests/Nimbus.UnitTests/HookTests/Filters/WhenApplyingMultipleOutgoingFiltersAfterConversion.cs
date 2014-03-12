using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.Hooks;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.HookTests.Filters
{
    [TestFixture]
    public class WhenApplyingMultipleOutgoingFiltersAfterConversion : SpecificationFor<IFilterHooks>
    {
        private object _originalMessage;
        private BrokeredMessage _originalBrokeredMessage;
        private BrokeredMessage _interimBrokeredMessage;
        private BrokeredMessage _outputBrokeredMessage;

        private BrokeredMessage _filteredBrokeredMessage;

        private IFilterOutgoingMessages _filter1;
        private IFilterOutgoingMessagesOf<object> _filter2;
        private IFilterOutgoingMessagesOf<string> _filter3;

        public override IFilterHooks Given()
        {
            _originalMessage = new object();
            _originalBrokeredMessage = new BrokeredMessage();
            _interimBrokeredMessage = new BrokeredMessage();
            _outputBrokeredMessage = new BrokeredMessage();

            _filter1 = Substitute.For<IFilterOutgoingMessages>();
            _filter1.Order.Returns(2);
            _filter1.FilterOut(_interimBrokeredMessage, _originalMessage).Returns(_outputBrokeredMessage);

            _filter2 = Substitute.For<IFilterOutgoingMessagesOf<object>>();
            _filter2.Order.Returns(1);
            _filter2.FilterOut(_originalBrokeredMessage, _originalMessage).Returns(_interimBrokeredMessage);

            _filter3 = Substitute.For<IFilterOutgoingMessagesOf<string>>();
            _filter3.Order.Returns(3);
            _filter3.FilterOut(Arg.Any<BrokeredMessage>(), Arg.Any<string>()).Returns(new BrokeredMessage());

            return new FilterHooks()
                .WithOutgoingMessageFilter(_filter3)
                .WithOutgoingMessageFilter(_filter2)
                .WithOutgoingMessageFilter(_filter1);
        }

        public override void When()
        {
            _filteredBrokeredMessage = Subject.ApplyToOutgoingAfterConversion(_originalBrokeredMessage, _originalMessage);
        }

        [Test]
        public void ShouldHaveCalledBothFiltersInOrder()
        {
            _filter1.Received().FilterOut(_interimBrokeredMessage, _originalMessage);
            _filter2.Received().FilterOut(_originalBrokeredMessage, _originalMessage);

            _filteredBrokeredMessage.ShouldBeSameAs(_outputBrokeredMessage);
        }

        [Test]
        public void ShouldIgnoreFiltersForDifferentTypes()
        {
            _filter3.DidNotReceive().FilterOut(Arg.Any<BrokeredMessage>(), Arg.Any<string>());

            _filteredBrokeredMessage.ShouldBeSameAs(_outputBrokeredMessage);
        }
    }
}
