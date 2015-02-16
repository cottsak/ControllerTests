﻿using System;
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
        private readonly Lazy<HttpServer> _httpServer;
        private const string MediaType = "application/json";
        private readonly Uri _baseUri = new Uri("http://localhost");
        private TSession _session;
        private readonly ApiTestSetup<TSession> _setup;

        protected ApiControllerTestBase(ApiTestSetup<TSession> setup)
        {
            if (setup == null)
                throw new ArgumentException("Please initialise the test class by creating a constructor and passing the setup argument to base()", "setup");
            _setup = setup;

            _httpServer = new Lazy<HttpServer>(() =>
            {
                var rootScope = setup.Container.BeginLifetimeScope(setup.AdditionalConfig);

                var config = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(rootScope) };
                setup.RegisterWebApiConfig(config);
                config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

                _session = rootScope.Resolve<TSession>();
                if (setup.SessionSetup != null)
                    setup.SessionSetup(_session);

                return new HttpServer(config);
            });

        }

        protected void ConfigureServices(Action<ContainerBuilder> config)
        {
            if (_httpServer.IsValueCreated)
                throw new InvalidOperationException("Can not configure after the server has been initialised. Always ConfigureServices before using the Session member.");

            _setup.AdditionalConfig += config;
        }

        protected T ConfigureService<T>() where T : class
        {
            var sub = NSubstitute.Substitute.For<T>();
            ConfigureServices(builder => builder.Register(context => sub).As<T>());
            return sub;
        }

        protected TSession Session
        {
            get
            {
                if (!_httpServer.IsValueCreated)
                {
                    var initServer = _httpServer.Value;
                }

                return _session;
            }
            set { _session = value; }
        }

        protected HttpResponseMessage Post(string relativeUrl, object content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_baseUri, relativeUrl));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaType));
            request.Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, MediaType);

            var response = new HttpClient(_httpServer.Value).SendAsync(request).Result;

            // for debugging
            if (response.StatusCode == HttpStatusCode.InternalServerError)
                Console.WriteLine("\r\nresponse.StatusCode == 500\r\nDetails:\r\n{0}\r\n", response.Content.ReadAsStringAsync().Result);

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
                _httpServer.Value.Dispose();
            }

            _disposed = true;
        }
    }

    public static class ApiControllerTestExtentions
    {
        public static T BodyAs<T>(this HttpResponseMessage response)
        {
            if (typeof(T) == typeof(string))
                return (T)(object)response.Content.ReadAsStringAsync().Result;
            return response.Content.ReadAsAsync<T>().Result;
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

        internal IContainer Container { get; private set; }
        internal Action<HttpConfiguration> RegisterWebApiConfig { get; private set; }
        internal Action<ContainerBuilder> AdditionalConfig { get; set; }
        internal Action<TSession> SessionSetup { get; private set; }
        internal Action<TSession> SessionTeardown { get; private set; }
        internal Action<TSession> PostControllerAction { get; private set; }
    }
}
