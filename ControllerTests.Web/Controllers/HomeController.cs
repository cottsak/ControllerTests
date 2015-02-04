using System;
using System.Linq;
using System.Web.Mvc;
using ControllerTests.Web.Helpers;
using ControllerTests.Web.Models;
using NHibernate;
using NHibernate.Linq;

namespace ControllerTests.Web.Controllers
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

        public ActionResult DeleteICs()
        {
            var ics = _session.Query<IntegratedCircuit>().ToList();
            return View(ics);
        }

        [HttpPost]
        public ActionResult DeleteICs(int[] idsToDelete)
        {
            if (idsToDelete == null)
            {
                AddPageAlert("Please select at least one IC to delete");
                return DeleteICs();
            }

            var icsToDelete = _session.Query<IntegratedCircuit>().Where(i => idsToDelete.Contains(i.Id));
            icsToDelete.ForEach(ic => _session.Delete(ic));

            AddPageAlert(string.Format("{0} ICs deleted successfully.", idsToDelete.Count()), AlertType.Success);

            return RedirectToAction("Index");
        }
    }
}