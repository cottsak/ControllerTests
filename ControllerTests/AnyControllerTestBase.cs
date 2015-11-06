using System;
using Autofac;
using Autofac.Core.Lifetime;
using NSubstitute;

namespace ControllerTests
{
    /// <summary>
    /// This base type allows you to write simple tests which encapsulate the whole lifecycle of the 
    /// System Under Test (SUT/TSystem)
    /// </summary>
    /// <typeparam name="TSystem">The System Under Test (SUT): a controller, service or other contract that will be used as an entry point.</typeparam>
    /// <typeparam name="TSession">The contract which allows you to set up and assert state changes. eg. database connection, ORM session abstraction.</typeparam>
    public abstract class AnyControllerTestBase<TSystem, TSession> : IDisposable
    {
        private bool _disposed;
        private readonly Lazy<TSystem> _system;
        private TSession _session;
        private readonly TestSetup<TSession> _setup;

        protected AnyControllerTestBase(TestSetup<TSession> setup)
        {
            if (setup == null)
                throw new ArgumentException("Please initialise the test class by creating a constructor and passing the setup argument to base()", "setup");
            _setup = setup;

            _system = new Lazy<TSystem>(() =>
            {
                var container = setup.Container;
                // todo: remove MatchingScopeLifetimeTags.RequestLifetimeScopeTag? we don't know what framework will create a LTS and how it will do it
                var rootScope = container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag, setup.AdditionalConfig);

                var system = rootScope.Resolve<TSystem>();
                _session = rootScope.Resolve<TSession>();
                if (setup.SessionSetup != null)
                    setup.SessionSetup(_session);

                return system;
            });
        }

        protected void ConfigureServices(Action<ContainerBuilder> config)
        {
            if (_system.IsValueCreated)
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
                if (!_system.IsValueCreated)
                {
                    var initSystem = _system.Value;
                }

                return _session;
            }
            set { _session = value; }
        }

        protected void ActAction(Action<TSystem> action)
        {
            action(_system.Value);

            if (_setup.AfterActAction != null)
                _setup.AfterActAction(Session);
        }

        protected TResult ActAction<TResult>(Func<TSystem, TResult> action)
        {
            var result = action(_system.Value);

            if (_setup.AfterActAction != null)
                _setup.AfterActAction(Session);

            return result;
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
            Action<TSession> afterActAction = null)
        {
            if (container == null)
                throw new ArgumentNullException("container", "A real container must be supplied to setup tests");
            Container = container;

            AdditionalConfig = additionalConfig ?? (builder => { });
            SessionSetup = sessionSetup;
            SessionTeardown = sessionTeardown;
            AfterActAction = afterActAction;
        }

        internal IContainer Container { get; private set; }
        internal Action<ContainerBuilder> AdditionalConfig { get; set; }
        internal Action<TSession> SessionSetup { get; private set; }
        internal Action<TSession> SessionTeardown { get; private set; }
        internal Action<TSession> AfterActAction { get; private set; }
    }
}