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
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(description))
            {
                AddPageAlert("IC was not added: please fill the whole form.");
                return RedirectToAction("Index");
            }

            _session.Save(new IntegratedCircuit { Code = code, Description = description });
            AddPageAlert("IC added successfully.", AlertType.Success);
            return RedirectToAction("Index");
        }

        protected void AddPageAlert(string message, AlertType type = AlertType.Warning)
        {
            TempData[PageAlertHelper.PageAlertTempDataKey] = Tuple.Create(message, type);
        }

        
        // todo: now make this a webapi call
        [HttpPost]
        public ActionResult DeleteIC(int id)
        {
            throw new NotImplementedException();
        }
    }
}