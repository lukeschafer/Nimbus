using Nimbus.Configuration;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.ConfigurationTests
{
    [TestFixture]
    public class WhenConfiguringMessageFilters : SpecificationFor<BusBuilderConfiguration>
    {
        public override BusBuilderConfiguration Given()
        {
            return new BusBuilderConfiguration();
        }

        public override void When()
        {
            //TODO
        }

        [Test]
        public void HookAndFilterProvidersShouldntBeNull()
        {
            Subject.Hooks.ShouldNotBe(null);
            Subject.Hooks.Filters.ShouldNotBe(null);
            Subject.Hooks.Filters.IncomingGlobal.ShouldNotBe(null);
            Subject.Hooks.Filters.OutgoingGlobal.ShouldNotBe(null);
            Subject.Hooks.Filters.IncomingFor.ShouldNotBe(null);
            Subject.Hooks.Filters.OutgoingFor.ShouldNotBe(null);
        }
    }
}
