using System.Configuration;

namespace ControllerTests.Web
{
    public class Config
    {
        public static string DatabaseConnectionString
        { get { return ConfigurationManager.ConnectionStrings["store"].ConnectionString; } }
    }
}