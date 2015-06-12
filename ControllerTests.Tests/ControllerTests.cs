using System;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Core.Lifetime;
using ControllerTests.Web;
using Xunit;

namespace ControllerTests.Tests
{
    public class ControllerTests    // this class is so meta!
    {
        private readonly Type[] _controllerTypes = { typeof(Controller), typeof(ApiController) };

        [Fact]
        public void GivenARealContainer_WhenInstantiatingControllers_ThenDependenciesAreInjected()
        {
            var typesToTest = typeof(ContainerConfig).Assembly.GetTypes()
                .Where(t => _controllerTypes.Any(tt => tt.IsAssignableFrom(t)));

            var container = ContainerConfig.BuildContainer();
            using (var requestScope = container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag))
                foreach (var type in typesToTest)
                    requestScope.Resolve(type);
        }
    }
}
