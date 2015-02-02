using System.Web.Mvc;
using ControllerTests;
using MVCControllerTestsWithLocalDb.Db;
using MVCControllerTestsWithLocalDb.Web;
using NHibernate;

namespace MVCControllerTestsWithLocalDb.Tests.Helpers
{
    public class MvcControllerTest<TController> : MvcControllerTestBase<TController, ISession> where TController : Controller
    {
        static MvcControllerTest()
        {
            Program.Main(new[] { Config.DatabaseConnectionString });
        }

        public MvcControllerTest()
            : base(new MvcTestSetup<ISession>(
                ContainerConfig.BuildContainer(),
                session => session.BeginTransaction(),
                session => session.Transaction.Dispose(),    // tear down transaction to release locks
                session =>
                {
                    NhibernateConfig.CompleteRequest(session);
                    session.Clear();    // this is to ensure we don't get ghost results from the NHibernate cache
                }
                ))
        { }
    }
}
