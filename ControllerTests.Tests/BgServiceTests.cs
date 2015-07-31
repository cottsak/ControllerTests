using System;
using ControllerTests.MigrateDb;
using ControllerTests.Web;
using ControllerTests.Web.Controllers;
using NHibernate;
using Shouldly;
using Xunit;

namespace ControllerTests.Tests
{
    public class BgServiceTests : AnyControllerTestBase<IBackgroundService, ISession>
    {
        static BgServiceTests()
        {
            Program.Main(new[] { Config.DatabaseConnectionString });
        }

        public BgServiceTests()
            : base(new AnyTestSetup<ISession>(
                ContainerConfig.BuildContainer(),
                sessionSetup: session => session.BeginTransaction(),
                sessionTeardown: session => session.Transaction.Dispose(), // tear down transaction to release locks
                postControllerAction: session =>
                {
                    NhibernateConfig.CompleteRequest(session);
                    session.Clear(); // this is to ensure we don't get ghost results from the NHibernate cache
                }
                ))
        { }

        [Fact]
        public void WhenInvokeRun_ThenDateFlagShouldBeWithin5sOfUtcNow()
        {
            ActAction(s => s.InvokeRun());

            Session.Get<BgServiceFlag>(BgServiceFlag.LookupId)
                .LastStarted.Value.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow);
        }
    }
}
