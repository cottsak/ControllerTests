using System.Linq;
using System.Net;
using Autofac;
using ControllerTests.MigrateDb;
using ControllerTests.Web;
using ControllerTests.Web.Helpers;
using ControllerTests.Web.Models;
using NHibernate;
using NHibernate.Linq;
using NSubstitute;
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
        public void GivenDevAccessAndNoIcsInStore_WhenPost_ThenIcIsCreatedInStoreAndURIAndRepresentationAreReturned()
        {
            SubstituteAndConfigure<IDevAccessChecker>().UserHasDevAccess().Returns(true);
            const string code = "7805";
            const string description = "5v linear regulator";
            Session.Query<IntegratedCircuit>().Count().ShouldBe(0);

            var response = Post("/api/ics", new { code, description });

            response.StatusCode.ShouldBe(HttpStatusCode.Created);
            var ic = Session.Query<IntegratedCircuit>().First();
            ic.Code.ShouldBe(code);
            ic.Description.ShouldBe(description);

            response.Headers.Location.PathAndQuery.ShouldBe(string.Format("/api/ics/{0}", ic.Id));

            response.BodyAs<string>().ShouldContain(ic.Id.ToString());
            response.BodyAs<string>().ShouldContain(code);
            response.BodyAs<string>().ShouldContain(description);
        }
    }
}
