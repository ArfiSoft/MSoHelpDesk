using Intake.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Intake.Controllers
{
    public class TestAutoCompleteController : Controller
    {
        // GET: TestAutoComplete
        public ActionResult Index()
        {
            return View();
        }
        // GET: /AutoComplete/
        [Authorize]
        public ActionResult GetCompagny(string searchHint)
        {
            return Json(CompagnyNames.GetCompagnyLikeName(searchHint), JsonRequestBehavior.AllowGet);
        }

        // GET: /AutoComplete/
        [Authorize]
        public ActionResult GetServiceDeskName(string searchHint)
        {
            return Json(ServiceDeskNames.GetServiceDeskLikeName(searchHint), JsonRequestBehavior.AllowGet);
        }

        // GET: /AutoComplete/
        [Authorize]
        public ActionResult GetTicket(string text, string ServiceDesk, string Org)
        {
            return Json(TicketNumbers.GetTicketLikeName(text,ServiceDesk,Org), JsonRequestBehavior.AllowGet);
        }
    }
}