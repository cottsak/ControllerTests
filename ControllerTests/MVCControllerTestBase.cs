using System;
using System.Web.Mvc;
using Autofac;
using Autofac.Core.Lifetime;
using NCrunch.Framework;
using Subtext.TestLibrary;

namespace ControllerTests
{
    [ExclusivelyUses(NCrunchConstants.SingleThreadForDb)]     // don't run these transaction db tests in parallel else deadlocks
    public abstract class MvcControllerTestBase<TController, TSession> : IDisposable where TController : Controller
    {
        private bool _disposed;
        private readonly HttpSimulator _httpRequest;
        private readonly TController _controller;
        protected readonly TSession Session;
        private readonly MvcTestSetup<TSession> _setup;

        protected MvcControllerTestBase(MvcTestSetup<TSession> setup)
        {
            if (setup == null)
                throw new ArgumentException("Please initialise the test class by creating a constructor and passing the setup argument to base()", "setup");
            _setup = setup;

            var container = setup.Container;
            var lts = container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

            _httpRequest = new HttpSimulator().SimulateRequest();

            _controller = lts.Resolve<TController>();
            Session = lts.Resolve<TSession>();
            if (setup.SessionSetup != null)
                setup.SessionSetup(Session);
        }

        protected ActionResult InvokeAction(Func<TController, ActionResult> action)
        {
            var result = action(_controller);

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
            Action<TSession> sessionSetup = null,
            Action<TSession> sessionTeardown = null,
            Action<TSession> postControllerAction = null)
        {
            if (container == null)
                throw new ArgumentNullException("container", "A real container must be supplied to setup tests");
            Container = container;
            SessionSetup = sessionSetup;
            SessionTeardown = sessionTeardown;
            PostControllerAction = postControllerAction;
        }

        public IContainer Container { get; private set; }
        public Action<TSession> SessionSetup { get; private set; }
        public Action<TSession> SessionTeardown { get; private set; }
        public Action<TSession> PostControllerAction { get; private set; }
    }
}
