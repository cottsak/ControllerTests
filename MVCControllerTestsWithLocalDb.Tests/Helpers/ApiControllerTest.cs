using Autofac;
using ControllerTests;
using MVCControllerTestsWithLocalDb.Db;
using MVCControllerTestsWithLocalDb.Web;
using NHibernate;

namespace MVCControllerTestsWithLocalDb.Tests.Helpers
{
    public class ApiControllerTest : ApiControllerTestBase<ISession>
    {
        static ApiControllerTest()
        {
            Program.Main(new[] { Config.DatabaseConnectionString });
        }

        public ApiControllerTest()
            : base(new ApiTestSetup<ISession>(
                ContainerConfig.BuildContainer(),
                WebApiConfig.Register,
                builder =>
                {
                    // changing the ISession to a singleton so that the two ISession Resolve() calls
                    // produce the same instance such that the transaction includes all test activity.
                    builder.Register(context => NhibernateConfig.CreateSessionFactory().OpenSession())
                        .As<ISession>()
                        .SingleInstance();
                },
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
