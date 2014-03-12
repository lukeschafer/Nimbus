﻿using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Hooks;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Commands;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.MessageContracts;
using Nimbus.UnitTests.BatchSendingTests.MessageContracts;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.BatchSendingTests
{
    [TestFixture]
    internal class WhenSendingACollectionOfCommandsViaTheCommandSender : SpecificationForAsync<BusCommandSender>
    {
        private INimbusMessageSender _nimbusMessageSender;

        public override Task<BusCommandSender> Given()
        {
            _nimbusMessageSender = Substitute.For<INimbusMessageSender>();

            var hookProvider = Substitute.For<IHookProvider>();
            var messagingFactory = Substitute.For<INimbusMessagingFactory>();
            messagingFactory.GetQueueSender(Arg.Any<string>()).Returns(ci => _nimbusMessageSender);

            var clock = new SystemClock();
            var validCommandTypes = new CommandTypesSetting {Value = new[] {typeof (FooCommand), typeof (BarCommand), typeof (BazCommand)}};

            var busCommandSender = new BusCommandSender(messagingFactory, clock, validCommandTypes, hookProvider);
            return Task.FromResult(busCommandSender);
        }

        public override async Task When()
        {
            var commands = new IBusCommand[] {new FooCommand(), new BarCommand(), new BazCommand()};

            foreach (var command in commands)
            {
                await Subject.Send(command);
            }
        }

        [Test]
        public void TheCommandSenderShouldHaveReceivedThreeCalls()
        {
            _nimbusMessageSender.ReceivedCalls().Count().ShouldBe(3);
        }
    }
}