using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using NCrunch.Framework;
using Newtonsoft.Json;

namespace ControllerTests
{
    [ExclusivelyUses(NCrunchConstants.SingleThreadForDb)]   // don't run these transaction db tests in parallel else deadlocks
    public abstract class ApiControllerTestBase<TSession> : IDisposable
    {
        private bool _disposed;
        private readonly HttpServer _httpServer;
        private const string MediaType = "application/json";
        private readonly Uri _baseUri = new Uri("http://localhost");
        protected readonly TSession Session;
        private readonly ApiTestSetup<TSession> _setup;

        protected ApiControllerTestBase(ApiTestSetup<TSession> setup)
        {
            if (setup == null)
                throw new ArgumentException("Please initialise the test class by creating a constructor and passing the setup argument to base()", "setup");
            _setup = setup;

            var rootScope = setup.Container.BeginLifetimeScope(setup.AdditionalConfig);

            var config = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(rootScope) };
            setup.RegisterWebApiConfig(config);
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            _httpServer = new HttpServer(config);

            Session = rootScope.Resolve<TSession>();
            if (setup.SessionSetup != null)
                setup.SessionSetup(Session);
        }

        protected HttpResponseMessage Post(string relativeUrl, object content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_baseUri, relativeUrl));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaType));
            request.Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, MediaType);

            var response = new HttpClient(_httpServer).SendAsync(request).Result;

            // for debugging
            if (response.StatusCode == HttpStatusCode.InternalServerError)
                Console.WriteLine("response.StatusCode == 500\r\nDetails:\r\n{0}\r\n", response.Content.ReadAsStringAsync().Result);

            if (_setup.PostControllerAction != null)
                _setup.PostControllerAction(Session);

            return response;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ApiControllerTestBase()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_setup.SessionTeardown != null)
                    _setup.SessionTeardown(Session);
                _httpServer.Dispose();
            }

            _disposed = true;
        }
    }

    public class ApiTestSetup<TSession>
    {
        public ApiTestSetup(IContainer container,
            Action<HttpConfiguration> registerWebApiConfig,
            Action<ContainerBuilder> additionalConfig = null,
            Action<TSession> sessionSetup = null,
            Action<TSession> sessionTeardown = null,
            Action<TSession> postControllerAction = null)
        {
            if (container == null)
                throw new ArgumentNullException("container", "A real container must be supplied to setup tests");
            Container = container;

            if (registerWebApiConfig == null)
                throw new ArgumentNullException("registerWebApiConfig", "WebApi needs to configure itself before running tests.");
            RegisterWebApiConfig = registerWebApiConfig;

            AdditionalConfig = additionalConfig ?? (builder => { });
            SessionSetup = sessionSetup;
            SessionTeardown = sessionTeardown;
            PostControllerAction = postControllerAction;
        }

        public IContainer Container { get; private set; }
        public Action<HttpConfiguration> RegisterWebApiConfig { get; private set; }
        public Action<ContainerBuilder> AdditionalConfig { get; private set; }
        public Action<TSession> SessionSetup { get; private set; }
        public Action<TSession> SessionTeardown { get; private set; }
        public Action<TSession> PostControllerAction { get; private set; }
    }
}
