using System;
using System.Net.Http;
using System.Web.Http;
using NHibernate;

namespace ControllerTests.Web.Controllers
{
    public class ICsController : ApiController
    {
        private readonly ISession _session;

        public ICsController(ISession session)
        {
            _session = session;
        }

        public HttpResponseMessage Post(string code, string description)
        {
            throw new NotImplementedException();
        }
    }
}