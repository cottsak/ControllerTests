using System.Web.Http;
using ControllerTests.Web.Models;
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

        public class PostModel
        {
            public int id { get; set; }
            public string code { get; set; }
            public string description { get; set; }
        }
        // todo: find a way to wire this into the UI. this will make a comprehensive demo.
        public IHttpActionResult Post(PostModel model)
        {
            if (string.IsNullOrWhiteSpace(model.code) || string.IsNullOrWhiteSpace(model.description))
                return BadRequest("IC was not added: please send all fields.");

            var newIc = new IntegratedCircuit { Code = model.code, Description = model.description };
            _session.Save(newIc);
            
            var location = Url.Link("DefaultApi", new { controller = "ics", id = newIc.Id });
            return Created(location, newIc);
        }
    }
}