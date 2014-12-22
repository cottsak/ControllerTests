using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MVCControllerTestsWithLocalDb.Web.Models;
using NHibernate;

namespace MVCControllerTestsWithLocalDb.Web.Controllers
{
    public class DeleteICController : ApiController
    {
        private readonly ISession _session;

        public DeleteICController(ISession session)
        {
            _session = session;
        }

        public HttpResponseMessage Post(int id)
        {
            var ic = _session.Load<IntegratedCircuit>(id);
            _session.Delete(ic);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
