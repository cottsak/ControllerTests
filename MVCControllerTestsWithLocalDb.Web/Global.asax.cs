using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace MVCControllerTestsWithLocalDb.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ContainerConfig.SetupDependencyInjection();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
