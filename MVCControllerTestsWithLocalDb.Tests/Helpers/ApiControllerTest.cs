using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using MVCControllerTestsWithLocalDb.Db;
using MVCControllerTestsWithLocalDb.Web;
using NCrunch.Framework;
using Newtonsoft.Json;
using NHibernate;

namespace MVCControllerTestsWithLocalDb.Tests.Helpers
{
    [ExclusivelyUses(NCrunchSingleThreadForDb)]     // don't run these transaction db tests in parallel else deadlocks
    public abstract class ApiControllerTest : IDisposable
    {
        public const string NCrunchSingleThreadForDb = "db-transaction";

        private const string MediaType = "application/json";

        private bool _disposed;
        private readonly HttpServer _httpServer;
        private readonly Uri _baseUri = new Uri("http://localhost");

        static ApiControllerTest()
        {
            Program.Main(new[] { Config.DatabaseConnectionString });
        }

        protected ApiControllerTest()
        {
            var rootScope = ContainerConfig.BuildContainer().BeginLifetimeScope(builder =>
            {
                // changing the ISession to a singleton so that the two ISession Resolve() calls
                // produce the same instance such that the transaction includes all test activity.
                builder.Register(context => NhibernateConfig.CreateSessionFactory().OpenSession())
                    .As<ISession>()
                    .SingleInstance();
            });

            var config = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(rootScope) };
            WebApiConfig.Register(config);
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            _httpServer = new HttpServer(config);

            Session = rootScope.Resolve<ISession>();
            Session.BeginTransaction();
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

            NhibernateConfig.CompleteRequest(Session);
            Session.Clear();    // this is to ensure we don't get ghost results from the NHibernate cache

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
                Session.Transaction.Dispose();  // tear down transaction to release locks
                _httpServer.Dispose();
            }

            _disposed = true;
        }
    }
}