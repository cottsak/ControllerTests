using System;
using Autofac;
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
        public BgServiceTests()
            : base(new TestSetup<ISession>(
                ContainerConfig.BuildContainer(),
                builder =>
                {
                    var conn = new LocalDb().OpenConnection();
                    // migrate empty db
                    Program.Main(new[] { conn.ConnectionString });

                    // changing the ISession to a singleton so that the two ISession Resolve() calls
                    // produce the same instance such that the transaction includes all test activity.
                    builder.Register(context => NhibernateConfig.CreateSessionFactory(conn.ConnectionString).OpenSession())
                        .As<ISession>()
                        .SingleInstance();
                },
                session => session.BeginTransaction(),
                session => session.Transaction.Dispose(), // tear down transaction to release locks
                session =>
                {
                    NhibernateConfig.CompleteRequest(session);
                    session.Clear(); // this is to ensure we don't get ghost results from the NHibernate cache
                }))
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
