using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using Autofac.Core.Lifetime;
using MVCControllerTestsWithLocalDb.Web;
using MVCControllerTestsWithLocalDb.Web.Controllers;
using MVCControllerTestsWithLocalDb.Web.Models;
using NHibernate;
using NHibernate.Linq;
using NUnit.Framework;
using Shouldly;
using Subtext.TestLibrary;

namespace MVCControllerTestsWithLocalDb.Tests
{
    class HomeControllerTests : MVCControllerTest<HomeController>
    {
        [Test]
        public void Given3ICs_WhenGetIndex_ThenAll3ICsAreReturned()
        {
            // todo: remove this once using LocalDB as there should be no initial state in this table
            PurgeICTable();

            new[]
            {
                new IntegratedCircuit {Code = "1", Description = "Test1"},
                new IntegratedCircuit {Code = "2", Description = "Test2"},
                new IntegratedCircuit {Code = "3", Description = "Test3"},
            }.ForEach(ic => Session.Save(ic));
            Session.Flush();

            var model = (IEnumerable<IntegratedCircuit>)((ViewResult)Controller.Index()).Model;

            model.Count().ShouldBe(3);
            model.Last().Description.ShouldBe("Test3");
        }

        private void PurgeICTable()
        {
            var existingIcs = Session.Query<IntegratedCircuit>().ToList();
            existingIcs.ForEach(ic => Session.Delete(ic));
        }
    }

    internal class MVCControllerTest<TController> where TController : Controller
    {
        private HttpSimulator _httpRequest;

        [SetUp]
        public void Setup()
        {
            var container = ContainerConfig.BuildContainer();
            LifetimeScope = container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

            _httpRequest = new HttpSimulator().SimulateRequest();

            //var routes = new RouteCollection();
            //RouteConfig.RegisterRoutes(routes);
            Controller = LifetimeScope.Resolve<TController>();
            //Controller.ControllerContext = new ControllerContext(new HttpContextWrapper(HttpContext.Current), new RouteData(), controller);
            //Controller.Url = new UrlHelper(Controller.Request.RequestContext, routes);

            Session = LifetimeScope.Resolve<ISession>();
            Session.BeginTransaction();
        }

        protected TController Controller { get; private set; }
        protected ILifetimeScope LifetimeScope { get; private set; }
        protected ISession Session { get; private set; }

        [TearDown]
        public void TearDown()
        {
            LifetimeScope.Dispose();    // tear down transaction to release locks
            _httpRequest.Dispose();
        }
    }
}
