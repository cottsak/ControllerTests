using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using Autofac.Core.Lifetime;
using MVCControllerTestsWithLocalDb.Web;
using MVCControllerTestsWithLocalDb.Web.Controllers;
using MVCControllerTestsWithLocalDb.Web.Models;
using NCrunch.Framework;
using NHibernate;
using NHibernate.Linq;
using Shouldly;
using Subtext.TestLibrary;
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

    [ExclusivelyUses("db-transaction")]     // don't run these transaction db tests in parallel else deadlocks
    public abstract class MVCControllerTest<TController> : IDisposable where TController : Controller
    {
        private bool _disposed;
        private readonly HttpSimulator _httpRequest;

        static MVCControllerTest()
        {
            Db.Program.Main(new[] { Config.DatabaseConnectionString });
        }

        public MVCControllerTest()
        {
            var container = ContainerConfig.BuildContainer();
            var lts = container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

            _httpRequest = new HttpSimulator().SimulateRequest();

            Controller = lts.Resolve<TController>();
            Session = lts.Resolve<ISession>();
            Session.BeginTransaction();
        }

        protected TController Controller { get; private set; }
        protected ISession Session { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MVCControllerTest()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Session.Transaction.Dispose();  // tear down transaction to release locks
                _httpRequest.Dispose();
            }

            _disposed = true;
        }
    }
}
