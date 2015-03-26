using System;
using System.Web.Mvc;
using Autofac;
using Autofac.Core.Lifetime;
using NCrunch.Framework;
using Subtext.TestLibrary;

namespace ControllerTests
{
    [ExclusivelyUses(NCrunchConstants.SingleThreadForDb)]   // don't run these transaction db tests in parallel else deadlocks
    public abstract class MvcControllerTestBase<TController, TSession> : IDisposable where TController : Controller
    {
        private bool _disposed;
        private HttpSimulator _httpRequest;
        private readonly Lazy<TController> _controller;
        private TSession _session;
        private readonly MvcTestSetup<TSession> _setup;

        protected MvcControllerTestBase(MvcTestSetup<TSession> setup)
        {
            if (setup == null)
                throw new ArgumentException("Please initialise the test class by creating a constructor and passing the setup argument to base()", "setup");
            _setup = setup;

            _controller = new Lazy<TController>(() =>
            {
                var container = setup.Container;
                var requestScope = container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag, setup.AdditionalConfig);

                _httpRequest = new HttpSimulator().SimulateRequest();

                var controller = requestScope.Resolve<TController>();
                _session = requestScope.Resolve<TSession>();
                if (setup.SessionSetup != null)
                    setup.SessionSetup(_session);

                return controller;
            });
        }

        protected void ConfigureServices(Action<ContainerBuilder> config)
        {
            if (_controller.IsValueCreated)
                throw new InvalidOperationException("Can not configure after the server has been initialised. Always ConfigureServices before using the Session member.");

            _setup.AdditionalConfig += config;
        }

        protected T SubstituteAndConfigure<T>() where T : class
        {
            var sub = NSubstitute.Substitute.For<T>();
            ConfigureServices(builder => builder.Register(context => sub).As<T>());
            return sub;
        }

        protected TSession Session
        {
            get
            {
                if (!_controller.IsValueCreated)
                {
                    var initController = _controller.Value;
                }

                return _session;
            }
            set { _session = value; }
        }

        protected ActionResult InvokeAction(Func<TController, ActionResult> action)
        {
            var result = action(_controller.Value);

            if (_setup.PostControllerAction != null)
                _setup.PostControllerAction(Session);

            return result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MvcControllerTestBase()
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
                _httpRequest.Dispose();
            }

            _disposed = true;
        }
    }

    public class MvcTestSetup<TSession>
    {
        public MvcTestSetup(IContainer container,
            Action<ContainerBuilder> additionalConfig = null,
            Action<TSession> sessionSetup = null,
            Action<TSession> sessionTeardown = null,
            Action<TSession> postControllerAction = null)
        {
            if (container == null)
                throw new ArgumentNullException("container", "A real container must be supplied to setup tests");
            Container = container;

            AdditionalConfig = additionalConfig ?? (builder => { });
            SessionSetup = sessionSetup;
            SessionTeardown = sessionTeardown;
            PostControllerAction = postControllerAction;
        }

        internal IContainer Container { get; private set; }
        internal Action<ContainerBuilder> AdditionalConfig { get; set; }
        internal Action<TSession> SessionSetup { get; private set; }
        internal Action<TSession> SessionTeardown { get; private set; }
        internal Action<TSession> PostControllerAction { get; private set; }
    }
}
