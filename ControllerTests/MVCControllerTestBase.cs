using System;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Core.Lifetime;
using NSubstitute;
using Subtext.TestLibrary;

namespace ControllerTests
{
    public abstract class MvcControllerTestBase<TController, TSession> : IDisposable where TController : Controller
    {
        private bool _disposed;
        private HttpSimulator _httpRequest;
        private readonly Lazy<TController> _controller;
        private TSession _session;
        private readonly TestSetup<TSession> _setup;

        protected MvcControllerTestBase(TestSetup<TSession> setup)
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
                SetupRequestMocking(requestScope, controller);
                _session = requestScope.Resolve<TSession>();
                if (setup.SessionSetup != null)
                    setup.SessionSetup(_session);

                return controller;
            }, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        protected void ConfigureServices(Action<ContainerBuilder> config)
        {
            if (_controller.IsValueCreated)
                throw Constants.BadBuilderConfigOrderException();

            _setup.AdditionalConfig += config;
        }

        protected T SubstituteAndConfigure<T>() where T : class
        {
            var sub = Substitute.For<T>();
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

        /// <summary>
        /// This should be used for the 'Act' part of your test (in Arrange/Act/Assert terms). Please don't use this as a 
        /// generic method to access the controller instance. Make assertions using the Controller 
        /// instance with <see cref="AssertWithController"/> instead
        /// </summary>
        /// <param name="action">a lambda which invokes the controller action under test. eg ActAction(c => c.ChangePassword(vm));</param>
        /// <returns>the result of the Action being invoked</returns>
        protected ActionResult ActAction(Func<TController, ActionResult> action)
        {
            var result = action(_controller.Value);

            if (_setup.AfterActAction != null)
                _setup.AfterActAction(Session);

            return result;
        }

        /// <summary>
        /// Use this optional helper to make assertions in your test that require the Controller instance.
        /// </summary>
        protected void AssertWithController(Action<TController> controllerAction)
        { controllerAction(_controller.Value); }

        private static void SetupRequestMocking(ILifetimeScope requestScope, TController controller)
        {
            var context = Substitute.For<HttpContextBase>();
            context.Request.Returns(requestScope.Resolve<HttpRequestBase>());
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);
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
                if (_httpRequest != null)
                    _httpRequest.Dispose();
            }

            _disposed = true;
        }
    }
}
