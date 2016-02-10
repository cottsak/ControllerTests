using System.Linq;
using System.Web.Mvc;
using Autofac;
using ControllerTests.MigrateDb;
using ControllerTests.Web;
using ControllerTests.Web.Controllers;
using ControllerTests.Web.Helpers;
using ControllerTests.Web.Models;
using NHibernate;
using NHibernate.Linq;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ControllerTests.Tests
{
    public class HomeControllerTests : MvcControllerTestBase<HomeController, ISession>
    {
        internal static TestSetup<ISession> MssqlTestSetup = new TestSetup<ISession>(
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
            });

        public HomeControllerTests() : base(MssqlTestSetup) { }

        [Fact]
        public void Given3ICs_WhenGetIndex_ThenAll3ICsAreReturned()
        {
            new[]
            {
                new IntegratedCircuit {Code = "1", Description = "Test1"},
                new IntegratedCircuit {Code = "2", Description = "Test2"},
                new IntegratedCircuit {Code = "3", Description = "Test3"},
            }.ForEach(ic => Session.Save(ic));
            Session.Flush();

            var model = ((HomeController.IndexVM)((ViewResult)ActAction(c => c.Index())).Model).ICs;

            model.Count().ShouldBe(3);
            model.Last().Description.ShouldBe("Test3");
        }

        [Fact]
        public void GivenNoICs_WhenPostCreateIC_ThenStoreContainsNewIC()
        {
            const string newIcCode = "556";
            const string newIcDescription = "dual timer";

            ActAction(c => c.CreateIC(newIcCode, newIcDescription));

            var newIc = Session.Query<IntegratedCircuit>().Single();
            newIc.Code.ShouldBe(newIcCode);
            newIc.Description.ShouldBe(newIcDescription);
        }

        [Fact]
        public void Given3ICs_WhenDeleteICsWithAll3Ids_ThenTheStoreIsEmpty()
        {
            SubstituteAndConfigure<IDevAccessChecker>().UserHasDevAccess().Returns(true);

            var newICs = new[]
            {
                new IntegratedCircuit {Code = "1", Description = "Test1"},
                new IntegratedCircuit {Code = "2", Description = "Test2"},
                new IntegratedCircuit {Code = "3", Description = "Test3"},
            };
            newICs.ForEach(ic => Session.Save(ic));
            Session.Flush();

            ActAction(c => c.DeleteICs(newICs.Select(i => i.Id).ToArray()));

            Session.Query<IntegratedCircuit>().Count().ShouldBe(0);
        }
    }
}
