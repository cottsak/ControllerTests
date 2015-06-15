using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using ControllerTests.Web;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace ControllerTests.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var container = ContainerConfig.SetupDependencyInjection();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BackgroundJobs.Configure(app, container);
        }
    }
}
