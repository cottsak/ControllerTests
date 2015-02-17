using System.Net;
using ControllerTests.MigrateDb;
using ControllerTests.Web;
using ControllerTests.Web.Helpers;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ControllerTests.Tests
{
    public class DevAccessResourceTests : ApiControllerTestBase
    {
        static DevAccessResourceTests()
        {
            Program.Main(new[] { Config.DatabaseConnectionString });
        }

        public DevAccessResourceTests()
            : base(new ApiTestSetup(ContainerConfig.BuildContainer(), WebApiConfig.Register))
        { }

        [Fact]
        public void WhenDevAccessIsPresent_WhenGet_ThenResultIs200()
        {
            ConfigureService<IDevAccessChecker>().UserHasDevAccess().Returns(true);

            Get("/api/devaccess").StatusCode.ShouldBe((HttpStatusCode)200);
        }

        [Fact]
        public void WhenDevAccessIsNotPresent_WhenGet_ThenResultIs403()
        {
            ConfigureService<IDevAccessChecker>().UserHasDevAccess().Returns(false);

            Get("/api/devaccess").StatusCode.ShouldBe((HttpStatusCode)403);
        }
    }
}
