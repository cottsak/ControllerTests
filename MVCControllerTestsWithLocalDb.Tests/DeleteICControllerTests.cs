using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MVCControllerTestsWithLocalDb.Web.Models;
using NHibernate;
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
    }

    public abstract class ApiControllerTest : IDisposable
    {
        protected ISession Session { get; private set; }

        protected HttpResponseMessage Post(string relativeUrl, object content)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
