using System;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using MVCControllerTestsWithLocalDb.Db;
using MVCControllerTestsWithLocalDb.Web;
using MVCControllerTestsWithLocalDb.Web.Models;
using NCrunch.Framework;
using Newtonsoft.Json;
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

    [ExclusivelyUses(NCrunchSingleThreadForDb)]     // don't run these transaction db tests in parallel else deadlocks
    public abstract class ApiControllerTest : IDisposable
    {
        public const string NCrunchSingleThreadForDb = "db-transaction";

        private const string MediaType = "application/json";

        private bool _disposed;
        private readonly HttpConfiguration _config;
        private readonly HttpServer _httpServer;
        private readonly Uri _baseUri = new Uri("http://localhost");
        protected IDbConnection _conn;
        private IDbTransaction _tx;

        static ApiControllerTest()
        {
            Program.Main(new[] { Config.DatabaseConnectionString });
        }

        protected ApiControllerTest()
        {
            _conn = new SqlConnection(@"Server=.\sqlexpress; Database=MVCControllerTestsWithLocalDb; Trusted_connection=true");
            _conn.Open();
            //var cmd=_conn.CreateCommand();
            _tx = _conn.BeginTransaction();

            var container = ContainerConfig.BuildContainer();
            var rootScope = container.BeginLifetimeScope(builder =>
                {
                    // this re-registration is to allow the injection of a common transaction for both the
                    // ISession in the test Arrange and Assert as well as the Controller (Act)
                    builder.Register(context =>
                    {
                        var newSession = NhibernateConfig.CreateSessionFactory().OpenSession(_conn);
                        return newSession;
                    })
                        .As<ISession>()
                        .InstancePerRequest()
                        .OnRelease(NhibernateConfig.FlushSession);
                });

            _config = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(rootScope) };
            WebApiConfig.Register(_config);
            _config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            _httpServer = new HttpServer(_config);

            Session = _config.DependencyResolver.BeginScope().GetRequestLifetimeScope().Resolve<ISession>();
            //Session.BeginTransaction();
        }

        protected ISession Session { get; private set; }

        protected HttpResponseMessage Post(string relativeUrl, object content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_baseUri, relativeUrl));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaType));
            request.Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, MediaType);

            var response = new HttpClient(_httpServer).SendAsync(request).Result;

            // for debugging
            if (response.StatusCode == HttpStatusCode.InternalServerError)
                Console.WriteLine("response.StatusCode == 500\r\nDetails:\r\n{0}\r\n", response.Content.ReadAsStringAsync().Result);

            request.GetDependencyScope().Dispose(); // force the disposal of the request lifetimescope for .Flush()
            return response;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ApiControllerTest()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _tx.Rollback();
                _conn.Dispose();
                //Session.Transaction.Dispose();  // tear down transaction to release locks
                _httpServer.Dispose();
                _config.Dispose();
            }

            _disposed = true;
        }
    }
}
