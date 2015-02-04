using System.Linq;
using System.Net;
using Autofac;
using ControllerTests.MigrateDb;
using ControllerTests.Web;
using ControllerTests.Web.Models;
using NHibernate;
using NHibernate.Linq;
using Shouldly;
using Xunit;

namespace ControllerTests.Tests
{
    public class DeleteIcControllerTests : ApiControllerTestBase<ISession>
    {
        static DeleteIcControllerTests()
        {
            Program.Main(new[] { Config.DatabaseConnectionString });
        }

        public DeleteIcControllerTests()
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
