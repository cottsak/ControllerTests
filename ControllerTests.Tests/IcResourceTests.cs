using System.Linq;
using System.Net;
using ControllerTests.Web;
using ControllerTests.Web.Helpers;
using ControllerTests.Web.Models;
using NHibernate;
using NHibernate.Linq;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ControllerTests.Tests
{
    public class IcResourceTests : ApiControllerTestBase<ISession>
    {
        public IcResourceTests() : base(new ApiTestSetup<ISession>(HomeControllerTests.MssqlTestSetup, WebApiConfig.Register)) { }

        [Fact]
        public void GivenDevAccessAndNoIcsInStore_WhenPost_ThenIcIsCreatedInStoreAndURIAndRepresentationAreReturned()
        {
            SubstituteAndConfigure<IDevAccessChecker>().UserHasDevAccess().Returns(true);
            const string code = "7805";
            const string description = "5v linear regulator";
            Session.Query<IntegratedCircuit>().Count().ShouldBe(0);

            var response = Post("/api/ics", new { code, description });

            response.StatusCode.ShouldBe(HttpStatusCode.Created);
            var ic = Session.Query<IntegratedCircuit>().First();
            ic.Code.ShouldBe(code);
            ic.Description.ShouldBe(description);

            response.Headers.Location.PathAndQuery.ShouldBe(string.Format("/api/ics/{0}", ic.Id));

            response.BodyAs<string>().ShouldContain(ic.Id.ToString());
            response.BodyAs<string>().ShouldContain(code);
            response.BodyAs<string>().ShouldContain(description);
        }
    }
}
