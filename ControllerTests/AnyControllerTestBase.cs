using System;
using Autofac;
using Autofac.Core.Lifetime;
using NSubstitute;

namespace ControllerTests
{
    public abstract class AnyControllerTestBase<TController, TSession> : IDisposable
    {
        private bool _disposed;
        private readonly Lazy<TController> _controller;
        private TSession _session;
        private readonly TestSetup<TSession> _setup;

        protected AnyControllerTestBase(TestSetup<TSession> setup)
        {
            if (setup == null)
                throw new ArgumentException("Please initialise the test class by creating a constructor and passing the setup argument to base()", "setup");
            _setup = setup;

            _controller = new Lazy<TController>(() =>
            {
                var container = setup.Container;
                // todo: remove MatchingScopeLifetimeTags.RequestLifetimeScopeTag? we don't know what framework will create a LTS and how it will do it
                var rootScope = container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag, setup.AdditionalConfig);

                var controller = rootScope.Resolve<TController>();
                _session = rootScope.Resolve<TSession>();
                if (setup.SessionSetup != null)
                    setup.SessionSetup(_session);

                return controller;
            });
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

        protected void ActAction(Action<TController> action)
        {
            action(_controller.Value);

            if (_setup.PostControllerAction != null)
                _setup.PostControllerAction(Session);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AnyControllerTestBase()
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
            }

            _disposed = true;
        }
    }

    public class TestSetup<TSession>
    {
        public TestSetup(IContainer container,
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