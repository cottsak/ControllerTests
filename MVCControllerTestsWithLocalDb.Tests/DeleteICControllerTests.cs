using System.Linq;
using System.Net;
using MVCControllerTestsWithLocalDb.Tests.Helpers;
using MVCControllerTestsWithLocalDb.Web.Models;
using NHibernate.Linq;
using Shouldly;
using Xunit;

namespace MVCControllerTestsWithLocalDb.Tests
{
    public class DeleteICControllerTests : ApiControllerTest
    {
        [Fact]
        public void GivenOneIC_WhenRemoveOnlyIC_ThenStoreShouldBeEmpty()
        {
            var ic = new IntegratedCircuit { Code = "1", Description = "Test1" };
            Session.Save(ic);
            Session.Flush();

            var result = Post("/api/deleteic/" + ic.Id, new { });

            result.StatusCode.ShouldBe(HttpStatusCode.OK);
            Session.Query<IntegratedCircuit>().ShouldBeEmpty();
        }

        [Fact]
        public void GivenASetOfICs_WhenRemovingASpecificOne_ThenOnlyThatOneIsMissing()
        {
            new[]
            {
                new IntegratedCircuit {Code = "1", Description = "Test1"},
                new IntegratedCircuit {Code = "2", Description = "Test2"},
                new IntegratedCircuit {Code = "3", Description = "Test3"},
            }.ForEach(ic => Session.Save(ic));
            Session.Flush();
            var ic2 = Session.Query<IntegratedCircuit>().Single(i => i.Code == "2");

            var result = Post("/api/deleteic/" + ic2.Id, new { });

            result.StatusCode.ShouldBe(HttpStatusCode.OK);
            var allRemainingICs = Session.Query<IntegratedCircuit>().ToList();
            allRemainingICs.Count().ShouldBe(2);
            allRemainingICs[0].Description.ShouldBe("Test1");
            allRemainingICs[1].Description.ShouldBe("Test3");
        }
    }
}
