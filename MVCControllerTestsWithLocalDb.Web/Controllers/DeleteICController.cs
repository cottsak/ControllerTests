using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MVCControllerTestsWithLocalDb.Web.Models;
using NHibernate;
using NHibernate.Linq;

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
            var ic = _session.Query<IntegratedCircuit>().SingleOrDefault(i => i.Id == id);

            if (ic == null)
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            _session.Delete(ic);
            _session.Flush();
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
