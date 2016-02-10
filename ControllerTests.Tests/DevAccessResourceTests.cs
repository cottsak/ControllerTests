using System;
using System.Net;
using ControllerTests.Web;
using NUnit.Framework;
using Shouldly;

namespace ControllerTests.Tests
{
    public class DevAccessResourceTests : ApiControllerTestBase
    {
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
        public void WhenDevAccessIsNotPresent_WhenGet_ThenResultIs404()
        {
            Get("/api/devaccess").StatusCode.ShouldBe((HttpStatusCode)404);
        }

        [Test]
        public void WhenISendTheCorrectDevAccessHeader_WhenGet_ThenResultIs200()
        {
            var headers = Tuple.Create("dev", "1234");

            Get("/api/devaccess", headers).StatusCode.ShouldBe((HttpStatusCode)200);
        }
    }
}
