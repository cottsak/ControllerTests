using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using Autofac.Core.Lifetime;
using MVCControllerTestsWithLocalDb.Web;
using MVCControllerTestsWithLocalDb.Web.Controllers;
using MVCControllerTestsWithLocalDb.Web.Models;
using NUnit.Framework;
using Shouldly;
using Subtext.TestLibrary;

namespace MVCControllerTestsWithLocalDb.Tests
{
    class HomeControllerTests : MVCControllerTest<HomeController>
    {
        [Test]
        public void WhenGetIndex_ThenAViewIsReturned()
        {
            var view = Controller.Index() as ViewResult;

            view.ShouldNotBe(null);
            ((IEnumerable<IntegratedCircuit>)view.Model).Count().ShouldBeGreaterThan(1);
        }
    }

    internal class MVCControllerTest<TController> where TController : Controller
    {
        private readonly HttpSimulator _httpRequest;

        internal MVCControllerTest()
        {
            var container = ContainerConfig.BuildContainer();
            LifetimeScope = container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

            _httpRequest = new HttpSimulator().SimulateRequest();

            //var routes = new RouteCollection();
            //RouteConfig.RegisterRoutes(routes);
            Controller = LifetimeScope.Resolve<TController>();
            //Controller.ControllerContext = new ControllerContext(new HttpContextWrapper(HttpContext.Current), new RouteData(), controller);
            //Controller.Url = new UrlHelper(Controller.Request.RequestContext, routes);
        }

        protected TController Controller { get; private set; }
        protected ILifetimeScope LifetimeScope { get; private set; }

        [TearDown]
        void TearDown()
        {
            LifetimeScope.Dispose();
            _httpRequest.Dispose();
        }
    }
}
