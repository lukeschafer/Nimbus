using System;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.Hooks;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.HookTests.Filters
{
    [TestFixture]
    public class WhenApplyingMultipleIncomingFilters : SpecificationFor<IFilterHooks>
    {
        private object _originalMessage;
        private BrokeredMessage _originalBrokeredMessage;
        private object _interimMessage;
        private object _outputMessage;

        private object _filteredMessage;

        private IFilterIncomingMessages _filter1;
        private IFilterIncomingMessagesOf<object> _filter2;
        private IFilterIncomingMessagesOf<string> _filter3;

        public override IFilterHooks Given()
        {
            _originalMessage = new object();
            _originalBrokeredMessage = new BrokeredMessage();
            _interimMessage = new object();
            _outputMessage = new object();

            _filter1 = Substitute.For<IFilterIncomingMessages>();
            _filter1.Order.Returns(2);
            _filter1.FilterIn(_originalBrokeredMessage, _interimMessage).Returns(_outputMessage);

            _filter2 = Substitute.For<IFilterIncomingMessagesOf<object>>();
            _filter2.Order.Returns(1);
            _filter2.FilterIn(_originalBrokeredMessage, _originalMessage).Returns(_interimMessage);

            _filter3 = Substitute.For<IFilterIncomingMessagesOf<string>>();
            _filter3.Order.Returns(3);
            _filter3.FilterIn(Arg.Any<BrokeredMessage>(), Arg.Any<string>()).Returns("test");

            return new FilterHooks()
                .WithIncomingMessageFilter(_filter3)
                .WithIncomingMessageFilter(_filter2)
                .WithIncomingMessageFilter(_filter1);
        }

        public override void When()
        {
            _filteredMessage = Subject.ApplyToIncoming(_originalBrokeredMessage, _originalMessage);
        }

        [Test]
        public void ShouldHaveCalledBothFiltersInOrder()
        {
            _filter1.Received().FilterIn(_originalBrokeredMessage, _interimMessage);
            _filter2.Received().FilterIn(_originalBrokeredMessage, _originalMessage);

            _filteredMessage.ShouldBeSameAs(_outputMessage);
        }

        [Test]
        public void ShouldIgnoreFiltersForDifferentTypes()
        {
            _filter3.DidNotReceive().FilterIn(Arg.Any<BrokeredMessage>(), Arg.Any<string>());

            _filteredMessage.ShouldBeSameAs(_outputMessage);
        }
    }
}
