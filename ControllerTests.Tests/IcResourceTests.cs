using System.Linq;
using System.Net;
using Autofac;
using ControllerTests.MigrateDb;
using ControllerTests.Web;
using ControllerTests.Web.Models;
using NHibernate;
using NHibernate.Linq;
using Shouldly;
using Xunit;

namespace ControllerTests.Tests
{
    public class IcResourceTests : ApiControllerTestBase<ISession>
    {
        static IcResourceTests()
        {
            Program.Main(new[] { Config.DatabaseConnectionString });
        }

        public IcResourceTests()
            : base(new ApiTestSetup<ISession>(
                ContainerConfig.BuildContainer(),
                WebApiConfig.Register,
                builder =>
                {
                    // changing the ISession to a singleton so that the two ISession Resolve() calls
                    // produce the same instance such that the transaction includes all test activity.
                    builder.Register(context => NhibernateConfig.CreateSessionFactory().OpenSession())
                        .As<ISession>()
                        .SingleInstance();
                },
                session => session.BeginTransaction(),
                session => session.Transaction.Dispose(),    // tear down transaction to release locks
                session =>
                {
                    NhibernateConfig.CompleteRequest(session);
                    session.Clear();    // this is to ensure we don't get ghost results from the NHibernate cache                    
                }
                ))
        { }

        [Fact]
        public void GivenNoIcsInStore_WhenPost_ThenAnIcIsPresentInStoreAndTheIdIsReturned()
        {
            Session.Query<IntegratedCircuit>().Count().ShouldBe(0);

            var response = Post("/api/ics", new { code = "7805", description = "5v linear regulator" });

            response.StatusCode.ShouldBe(HttpStatusCode.Created);
            response.BodyAs<int>().ShouldBeGreaterThan(0);
        }
    }
}
