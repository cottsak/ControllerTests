using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MVCControllerTestsWithLocalDb.Tests.Helpers;
using MVCControllerTestsWithLocalDb.Web.Controllers;
using MVCControllerTestsWithLocalDb.Web.Models;
using NHibernate.Linq;
using Shouldly;
using Xunit;

namespace MVCControllerTestsWithLocalDb.Tests
{
    public class HomeControllerTests : MVCControllerTest<HomeController>
    {
        [Fact]
        public void Given3ICs_WhenGetIndex_ThenAll3ICsAreReturned()
        {
            new[]
            {
                new IntegratedCircuit {Code = "1", Description = "Test1"},
                new IntegratedCircuit {Code = "2", Description = "Test2"},
                new IntegratedCircuit {Code = "3", Description = "Test3"},
            }.ForEach(ic => Session.Save(ic));
            Session.Flush();

            var model = (IEnumerable<IntegratedCircuit>)((ViewResult)Controller.Index()).Model;

            model.Count().ShouldBe(3);
            model.Last().Description.ShouldBe("Test3");
        }

        [Fact]
        public void GivenNoICs_WhenPostCreateIC_ThenStoreContainsNewIC()
        {
            const string newIcCode = "556";
            const string newIcDescription = "dual timer";

            Controller.CreateIC(newIcCode, newIcDescription);

            var newIc = Session.Query<IntegratedCircuit>().Single();
            newIc.Code.ShouldBe(newIcCode);
            newIc.Description.ShouldBe(newIcDescription);
        }
    }
}
