using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace ControllerTests.Web.Controllers
{
    public class DevAccessController : ApiController
    {
        [DevAccessFilter]
        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }

    class DevAccessFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.Request.Headers.Contains("dev") &&
                actionContext.Request.Headers.GetValues("dev").Contains("1234"))
            {
                // all ok
                base.OnActionExecuting(actionContext);
            }
            else
                throw new HttpResponseException(HttpStatusCode.NotFound);
        }
    }
}