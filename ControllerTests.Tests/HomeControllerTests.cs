using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ControllerTests.MigrateDb;
using ControllerTests.Web;
using ControllerTests.Web.Controllers;
using ControllerTests.Web.Models;
using NHibernate;
using NHibernate.Linq;
using Shouldly;
using Xunit;

namespace ControllerTests.Tests
{
    public class HomeControllerTests : MvcControllerTestBase<HomeController, ISession>
    {
        static HomeControllerTests()
        {
            Program.Main(new[] { Config.DatabaseConnectionString });
        }

        public HomeControllerTests()
            : base(new MvcTestSetup<ISession>(
                ContainerConfig.BuildContainer(),
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
        public void Given3ICs_WhenGetIndex_ThenAll3ICsAreReturned()
        {
            new[]
            {
                new IntegratedCircuit {Code = "1", Description = "Test1"},
                new IntegratedCircuit {Code = "2", Description = "Test2"},
                new IntegratedCircuit {Code = "3", Description = "Test3"},
            }.ForEach(ic => Session.Save(ic));
            Session.Flush();

            var model = (IEnumerable<IntegratedCircuit>)((ViewResult)InvokeAction(c => c.Index())).Model;

            model.Count().ShouldBe(3);
            model.Last().Description.ShouldBe("Test3");
        }

        [Fact]
        public void GivenNoICs_WhenPostCreateIC_ThenStoreContainsNewIC()
        {
            const string newIcCode = "556";
            const string newIcDescription = "dual timer";

            InvokeAction(c => c.CreateIC(newIcCode, newIcDescription));

            var newIc = Session.Query<IntegratedCircuit>().Single();
            newIc.Code.ShouldBe(newIcCode);
            newIc.Description.ShouldBe(newIcDescription);
        }

        [Fact]
        public void Given3ICs_WhenDeleteICsWithAll3Ids_ThenTheStoreIsEmpty()
        {
            var newICs = new[]
            {
                new IntegratedCircuit {Code = "1", Description = "Test1"},
                new IntegratedCircuit {Code = "2", Description = "Test2"},
                new IntegratedCircuit {Code = "3", Description = "Test3"},
            };
            newICs.ForEach(ic => Session.Save(ic));
            Session.Flush();

            InvokeAction(c => c.DeleteICs(newICs.Select(i => i.Id).ToArray()));

            Session.Query<IntegratedCircuit>().Count().ShouldBe(0);
        }
    }
}
