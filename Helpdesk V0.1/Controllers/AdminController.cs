using Helpdesk_V0._1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Helpdesk_V0._1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        static ApplicationDbContext db = ApplicationDbContext.Create();

        private Helpers.ServiceDeskWSClient sDesk = new Helpers.ServiceDeskWSClient(db.Settings.FirstOrDefault(s => s.Key == "ServiceDeskURI").Value.ToString(), db.Settings.FirstOrDefault(s => s.Key == "KaseyaURI").Value.ToString());
        //
        // GET: /Admin/

        public ActionResult Index()
        {
            Models.AdminSettings result = new Models.AdminSettings();
            
            result.password = db.Settings.FirstOrDefault(s => s.Key == "KaseyaPassword").Value.ToString();
            result.userName = db.Settings.FirstOrDefault(s => s.Key == "KaseyaUser").Value.ToString();
            result.servicedeskId = Convert.ToInt32(db.Settings.FirstOrDefault(s => s.Key == "ServiceDeskId").Value);
            result.servicedeskName = db.Settings.FirstOrDefault(s => s.Key == "ServiceDeskName").Value.ToString();
            result.server = ((Uri)new Uri(db.Settings.FirstOrDefault(s => s.Key == "ServiceDeskURI").Value.ToString())).GetLeftPart(System.UriPartial.Authority);

            return View(result);
        }

        public JsonResult GetServicedesks()
        {
            
            SDWS.GetServiceDesksRequest req = new SDWS.GetServiceDesksRequest();

            SDWS.GetServiceDesksResponse list = sDesk.ProcessRequest(req);
            List<ServiceDeskCBModel> desks = new List<ServiceDeskCBModel>();
            ServiceDeskCBModel defaultDesk = new ServiceDeskCBModel();

            defaultDesk.ServiceDeskID = -1;
            defaultDesk.ServiceDeskName = "-";
            desks.Add(defaultDesk);

            foreach (SDWS.ServiceDesk item in list.ServiceDesks)
            {
                ServiceDeskCBModel newItem = new ServiceDeskCBModel();
                newItem.ServiceDeskID = item.ServiceDeskID;
                newItem.ServiceDeskName = item.ServiceDeskName;
                desks.Add(newItem);
            }

            //list.ServiceDeskDefinitionResponse
            return Json(desks, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveSettings(Models.AdminSettings settings)
        {
            db.Settings.First(s => s.Key == "KaseyaPassword").Value = settings.password;
            db.Settings.First(s => s.Key == "KaseyaUser").Value = settings.userName;
            db.Settings.First(s => s.Key == "ServiceDeskId").Value = settings.servicedeskId.ToString();
            db.Settings.First(s => s.Key == "ServiceDeskName").Value = settings.servicedeskName;
            db.Settings.First(s => s.Key == "ServiceDeskURI").Value = settings.server;
            db.SaveChanges();

            return Index();
        }
    }
}