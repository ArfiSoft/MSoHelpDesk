using Helpdesk_V0._1.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;

namespace Helpdesk_V0._1.Controllers
{
    public class HomeController : Controller
    {
        static ApplicationDbContext db = ApplicationDbContext.Create();

        private Helpers.KaseyaWSClient KasClient = new Helpers.KaseyaWSClient(db.Settings.FirstOrDefault(s => s.Key == "KaseyaURI").Value);
        private Helpers.ServiceDeskWSClient sDesk = new Helpers.ServiceDeskWSClient(db.Settings.FirstOrDefault(s => s.Key == "ServiceDeskURI").Value, db.Settings.FirstOrDefault(s => s.Key == "KaseyaURI").Value);

        private List<string> ImportUsers()
        {
            GetOrgsByScopeIDResponse req = new GetOrgsByScopeIDResponse();
            XmlSerializer serializer = new XmlSerializer(typeof(GetOrgsByScopeIDResponse));

            string xml = KasClient.ProcessRequest(req.Get);

            req = (GetOrgsByScopeIDResponse)serializer.Deserialize(new StringReader(xml));

            return KasClient.ImportUsers(req.Orgs);
        }

        [Authorize]
        public JsonResult GetTickets(IncidentListRequestModel param)//string searchText, string[] options)
        {
            //IncidentListRequestModel param = new IncidentListRequestModel();

            SDWS.GetIncidentListRequest req = new SDWS.GetIncidentListRequest();
            SDWS.IncidentListFilter filter = new SDWS.IncidentListFilter();

            //param.searchText = searchText;
            //param.options = options.ToList();

            //filter.IncidentCount = 30;
            filter.IncidentCountSpecified = false;
            //filter.SortField = x;
            //filter.SummarySearch = x;
            filter.Status = param.options.ToArray();
            //filter.StartingIncident = x;
            //filter.Stage = x;
            filter.ServiceDeskName = db.Settings.FirstOrDefault(s => s.Key == "ServiceDeskName").Value;
            //filter.Priority=x;
            //filter.OrganizationStaff =x;
            if (User.Identity.Name != "admin")
            {
                filter.Organization = Helpers.UserTools.UserToFilter(User.Identity.Name);
            }
            //filter.MachineGroup = x;
            //filter.Machine = x;
            //filter.Assignee = x;

            req.IncidentListRequest = filter;
            SDWS.GetIncidentListResponse items = sDesk.ProcessRequest(req);

            GetTicketsModel result = new GetTicketsModel();

            try
            {
                AutoMapper.Mapper.AssertConfigurationIsValid();
                result.ErrorLocation = items.ErrorLocation;
                result.ErrorMessage = items.ErrorMessage;
                IncidentListModel incidentList = new IncidentListModel();

                incidentList.nextStartingIncident = items.IncidentList.nextStartingIncident;
                incidentList.totalIncidents = items.IncidentList.totalIncidents;

                List<IncidentSummaryGridModel> list = new List<IncidentSummaryGridModel>();
                SDWS.IncidentSummary[] incList = items.IncidentList.Incident;

                if (param.searchText != null)
                {
                    List<SDWS.IncidentSummary> temp = new List<SDWS.IncidentSummary>();

                    foreach (var item in incList)
                    {
                        if (item.IncidentNumber.ToLower().Contains(param.searchText.ToLower()) ||
                            (item.Description ?? "").ToLower().Contains(param.searchText.ToLower()) ||
                            (item.OrganizationName ?? "").ToLower().Contains(param.searchText.ToLower()) ||
                            (item.SubmitterEmail ?? "").ToLower().Contains(param.searchText.ToLower()) ||
                            (item.Submitter ?? "").ToLower().Contains(param.searchText.ToLower()) ||
                            (item.Summary ?? "").ToLower().Contains(param.searchText.ToLower()) ||
                            (item.Resolution ?? "").ToLower().Contains(param.searchText.ToLower()) ||
                            (item.SubCategory ?? "").ToLower().Contains(param.searchText.ToLower())
                            )
                        {
                            temp.Add(item);
                        }
                    }

                    incList = temp.ToArray();

                }
                foreach (var incidentSummary in incList)
                {
                    IncidentSummaryGridModel model = new IncidentSummaryGridModel();
                    model = AutoMapper.Mapper.Map<IncidentSummaryGridModel>(incidentSummary);
                    list.Add(model);
                }
                incidentList.Incident = list.ToArray();
                result.IncidentList = incidentList;

            }
            catch (Exception ex)
            {

                result.ErrorLocation = ex.StackTrace;
                result.ErrorMessage = ex.Message;
            }

            //var status = result.IncidentList.Incident.AsQueryable().Select(i => i.Status).Distinct();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Add()
        {
            //IncidentListModel model;
            SDWS.AddIncidentRequest ticket = new SDWS.AddIncidentRequest();
            SDWS.AddIncidentResponse responce = new SDWS.AddIncidentResponse();


            return Json(responce.AddSDIncidentResponse.IncidentNumber);
        }

        //[Authorize]
        public PartialViewResult _Ticket(string Id)
        {
            IncidentModel model;
            SDWS.GetIncidentReq req = new SDWS.GetIncidentReq();
            SDWS.GetIncidentRequest filter = new SDWS.GetIncidentRequest();
            filter.IncidentNumber = Id;
            filter.IncidentIdSpecified = false;
            filter.IncludeAttachments = true;
            filter.IncludeAttachmentsSpecified = true;
            filter.IncludeDefinition = true;
            filter.IncludeDefinitionSpecified = true;
            filter.IncludeNotes = true;
            filter.IncludeNotesSpecified = true;
            req.IncidentRequest = filter;
            SDWS.GetIncidentResponse responce = sDesk.ProcessRequest(req);

            model = AutoMapper.Mapper.Map<IncidentModel>(responce.IncidentResponse);
            jqxDateTime resDate = new jqxDateTime();
            resDate.Day = model.CloseDateTime.Day;
            resDate.Month = model.CloseDateTime.Month - 1;
            resDate.Year = model.CloseDateTime.Year;

            ViewBag.resDate = resDate;

            return PartialView(model);
        }

        [Authorize]
        public ActionResult Index()
        {
            ViewBag.Error = new List<string>();
            try
            {

                foreach (string er in ImportUsers())
                {
                    ViewBag.Error.Add(er);
                }
            }
            catch (Exception er)
            {
                string tmp = string.Empty;
                if (er.InnerException != null)
                {
                    tmp = string.Format(" InnerError: {0}, Source: {1}", er.InnerException.Message, er.InnerException.Source);
                }
                ViewBag.Error.Add(string.Format("Error: {0}, Source: {1}.{2}", er.Message, er.Source, tmp));
            }


            return View();
        }

    }
}