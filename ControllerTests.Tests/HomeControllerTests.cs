using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ControllerTests.MigrateDb;
using ControllerTests.Web;
using ControllerTests.Web.Controllers;
using ControllerTests.Web.Helpers;
using ControllerTests.Web.Models;
using NCrunch.Framework;
using NHibernate;
using NHibernate.Linq;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ControllerTests.Tests
{
    [ExclusivelyUses(SingleThreadForDb)]
    public class HomeControllerTests : MvcControllerTestBase<HomeController, ISession>
    {
        public const string SingleThreadForDb = "db-transaction";

        static HomeControllerTests()
        {
            Program.Main(new[] { Config.DatabaseConnectionString });
        }

        public HomeControllerTests()
            : base(new MvcTestSetup<ISession>(
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
        public void Given3ICs_WhenGetIndex_ThenAll3ICsAreReturned()
        {
            new[]
            {
                new IntegratedCircuit {Code = "1", Description = "Test1"},
                new IntegratedCircuit {Code = "2", Description = "Test2"},
                new IntegratedCircuit {Code = "3", Description = "Test3"},
            }.ForEach(ic => Session.Save(ic));
            Session.Flush();

            var model = (IEnumerable<IntegratedCircuit>)((ViewResult)ActAction(c => c.Index())).Model;

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
