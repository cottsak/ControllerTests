using System;
using System.Linq;
using System.Web.Mvc;
using MVCControllerTestsWithLocalDb.Web.Helpers;
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

        [HttpPost]
        public ActionResult CreateIC(string code, string description)
        {
            _session.Save(new IntegratedCircuit { Code = code, Description = description });
            AddPageAlert("IC added successfully.", AlertType.Success);
            return RedirectToAction("Index");
        }

        protected void AddPageAlert(string message, AlertType type = AlertType.Alert)
        {
            TempData[PageAlertHelper.PageAlertTempDataKey] = Tuple.Create(message, type);
        }
    }
}