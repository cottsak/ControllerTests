using System.Web.Http;

namespace MVCControllerTestsWithLocalDb.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // we don't like XML
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}