using System.Configuration;

namespace MVCControllerTestsWithLocalDb.Web
{
    public class Config
    {
        public static string DatabaseConnectionString
        { get { return ConfigurationManager.ConnectionStrings["store"].ConnectionString; } }
    }
}