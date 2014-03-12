using Nimbus.Extensions;
using Nimbus.Hooks;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.HookTests.Filters
{
    [TestFixture]
    public class WhenApplyingMultipleOutgoingFiltersBeforeConversion : SpecificationFor<IFilterHooks>
    {
        private object _originalMessage;
        private object _interimMessage;
        private object _outputMessage;

        private object _filteredMessage;

        private IFilterOutgoingMessages _filter1;
        private IFilterOutgoingMessagesOf<object> _filter2;
        private IFilterOutgoingMessagesOf<string> _filter3;

        public override IFilterHooks Given()
        {
            _originalMessage = new object();
            _interimMessage = new object();
            _outputMessage = new object();

            _filter1 = Substitute.For<IFilterOutgoingMessages>();
            _filter1.Order.Returns(2);
            _filter1.PreFilterOut(_interimMessage).Returns(_outputMessage);

            _filter2 = Substitute.For<IFilterOutgoingMessagesOf<object>>();
            _filter2.Order.Returns(1);
            _filter2.PreFilterOut(_originalMessage).Returns(_interimMessage);

            _filter3 = Substitute.For<IFilterOutgoingMessagesOf<string>>();
            _filter3.Order.Returns(3);
            _filter3.PreFilterOut(Arg.Any<string>()).Returns("test string");

            return new FilterHooks()
                .WithOutgoingMessageFilter(_filter3)
                .WithOutgoingMessageFilter(_filter2)
                .WithOutgoingMessageFilter(_filter1);
        }

        public override void When()
        {
            _filteredMessage = Subject.ApplyToOutgoingBeforeConversion(_originalMessage);
        }

        [Test]
        public void ShouldHaveCalledBothFiltersInOrder()
        {
            _filter1.Received().PreFilterOut(_interimMessage);
            _filter2.Received().PreFilterOut(_originalMessage);

            _filteredMessage.ShouldBeSameAs(_outputMessage);
        }

        [Test]
        public void ShouldIgnoreFiltersForDifferentTypes()
        {
            _filter3.DidNotReceive().PreFilterOut(Arg.Any<string>());

            _filteredMessage.ShouldBeSameAs(_outputMessage);
        }
    }
}
