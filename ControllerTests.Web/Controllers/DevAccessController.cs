using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ControllerTests.Web.Helpers;

namespace ControllerTests.Web.Controllers
{
    public class DevAccessController : ApiController
    {
        private readonly IDevAccessChecker _decDevAccessChecker;

        public DevAccessController(IDevAccessChecker decDevAccessChecker)
        {
            _decDevAccessChecker = decDevAccessChecker;
        }

        public HttpResponseMessage Get()
        {
            if (_decDevAccessChecker.UserHasDevAccess())
                return Request.CreateResponse(HttpStatusCode.OK);

            return new HttpResponseMessage(HttpStatusCode.Forbidden);
        }
    }
}