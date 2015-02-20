using System.Net;
using ControllerTests.MigrateDb;
using ControllerTests.Web;
using ControllerTests.Web.Helpers;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace ControllerTests.Tests
{
    public class DevAccessResourceTests : ApiControllerTestBase
    {
        static DevAccessResourceTests()
        {
            Program.Main(new[] { Config.DatabaseConnectionString });
        }

        #region NUnit overhead

        private static readonly ApiTestSetup<NoSession> DevAccessResourceTestsSetup =
            new ApiTestSetup<NoSession>(ContainerConfig.BuildContainer(), WebApiConfig.Register);

        public DevAccessResourceTests() : base(DevAccessResourceTestsSetup) { }

        [SetUp]
        public void Setup()
        {
            this.Init(DevAccessResourceTestsSetup);
        }

        [TearDown]
        public void TearDown()
        {
            this.Dispose();
        }

        #endregion

        [Test]
        public void WhenDevAccessIsPresent_WhenGet_ThenResultIs200()
        {
            SubstituteAndConfigure<IDevAccessChecker>().UserHasDevAccess().Returns(true);

            Get("/api/devaccess").StatusCode.ShouldBe((HttpStatusCode)200);
        }

        [Test]
        public void WhenDevAccessIsNotPresent_WhenGet_ThenResultIs403()
        {
            SubstituteAndConfigure<IDevAccessChecker>().UserHasDevAccess().Returns(false);

            Get("/api/devaccess").StatusCode.ShouldBe((HttpStatusCode)403);
        }
    }
}
