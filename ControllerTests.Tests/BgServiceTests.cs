using System;
using ControllerTests.Web.Controllers;
using NHibernate;
using Shouldly;
using Xunit;

namespace ControllerTests.Tests
{
    public class BgServiceTests : AnyControllerTestBase<IBackgroundService, ISession>
    {
        public BgServiceTests() : base(HomeControllerTests.MssqlTestSetup) { }

        [Fact]
        public void WhenInvokeRun_ThenDateFlagShouldBeWithin5sOfUtcNow()
        {
            ActAction(s => s.InvokeRun());

            Session.Get<BgServiceFlag>(BgServiceFlag.LookupId)
                .LastStarted.Value.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow);
        }
    }
}
