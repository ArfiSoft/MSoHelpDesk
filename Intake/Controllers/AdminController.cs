using Intake.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Intake.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        static SettingsContext db = SettingsContext.Create();

        private Helpers.Kaseya.ServiceDeskWSClient sDesk = new Helpers.Kaseya.ServiceDeskWSClient(db.Settings.FirstOrDefault(s => s.Key == "ServiceDeskURI").Value.ToString(), db.Settings.FirstOrDefault(s => s.Key == "KaseyaURI").Value.ToString());
        //
        // GET: /Admin/

        public ActionResult Index()
        {
            Models.AdminSettings result = new Models.AdminSettings();
            
            result.password = db.Settings.FirstOrDefault(s => s.Key == "KaseyaPassword").Value.ToString();
            result.userName = db.Settings.FirstOrDefault(s => s.Key == "KaseyaUser").Value.ToString();
            //result.servicedeskId = Convert.ToInt32(db.Settings.FirstOrDefault(s => s.Key == "ServiceDeskId").Value);
            //result.servicedeskName = db.Settings.FirstOrDefault(s => s.Key == "ServiceDeskName").Value.ToString();
            result.server = ((Uri)new Uri(db.Settings.FirstOrDefault(s => s.Key == "ServiceDeskURI").Value.ToString())).GetLeftPart(System.UriPartial.Authority);
            result.DefaultScope = db.Settings.FirstOrDefault(s => s.Key == "DefaultScope").Value.ToString();
            result.connected = (sDesk.kWS.SessionID !=0);
            
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

        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult SaveSettings(Models.AdminSettings settings)
        {
            db.Settings.First(s => s.Key == "KaseyaPassword").Value = settings.password;
            db.Settings.First(s => s.Key == "KaseyaUser").Value = settings.userName;
            //db.Settings.First(s => s.Key == "ServiceDeskId").Value = settings.servicedeskId.ToString();
            //db.Settings.First(s => s.Key == "ServiceDeskName").Value = settings.servicedeskName;
            db.Settings.First(s => s.Key == "ServiceDeskURI").Value = settings.server;
            db.Settings.First(s => s.Key == "DefaulScope").Value = settings.DefaultScope;

            db.SaveChanges();

            return Index();
        }
    }
}