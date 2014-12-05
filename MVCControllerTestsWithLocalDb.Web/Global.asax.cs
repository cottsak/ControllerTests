using System.Web.Mvc;
using System.Web.Routing;

namespace MVCControllerTestsWithLocalDb.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ContainerConfig.SetupDependencyInjection();
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
