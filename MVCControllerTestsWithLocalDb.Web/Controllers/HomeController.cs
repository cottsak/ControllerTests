using System.Linq;
using System.Web.Mvc;
using MVCControllerTestsWithLocalDb.Web.Models;
using NHibernate;
using NHibernate.Linq;

namespace MVCControllerTestsWithLocalDb.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISession _session;

        public HomeController(ISession session)
        {
            _session = session;
        }

        public ActionResult Index()
        {
            var ics = _session.Query<IntegratedCircuit>().ToList();
            return View(ics);
        }
    }
}