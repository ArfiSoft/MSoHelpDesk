using Intake.Models;
using Intake.SDWS;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Intake.Controllers
{
    public class HomeController : Controller
    {
        private static SettingsContext db = SettingsContext.Create();
        private static string MessageDisplay = string.Empty;
        private static bool MessageSucces = true;
        private static string MessageTitle = string.Empty;
        private static Helpers.Kaseya.KaseyaWSClient KasClient = new Helpers.Kaseya.KaseyaWSClient(db.Settings.FirstOrDefault(s => s.Key == "KaseyaURI").Value);
        private static Helpers.Kaseya.ServiceDeskWSClient sDesk = new Helpers.Kaseya.ServiceDeskWSClient(db.Settings.FirstOrDefault(s => s.Key == "ServiceDeskURI").Value, db.Settings.FirstOrDefault(s => s.Key == "KaseyaURI").Value);


        private void WriteCoockie(string Name, string Value)
        {
            HttpCookie cookie = new HttpCookie(Name,Value);
            cookie.Expires = DateTime.Now.AddDays(30);
            this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
        }

        private void RemoveCookie(string Name)
        {
            if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(Name))
            {
                //HttpCookie cookie = this.ControllerContext.HttpContext.Request.Cookies[Name];
                //cookie.Expires = DateTime.Now.AddDays(-1);
                this.ControllerContext.HttpContext.Response.Cookies.Remove(Name);
            }
        }

        private string ReadCookie(string Name)
        {
            if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(Name))
            {
                HttpCookie cookie = this.ControllerContext.HttpContext.Request.Cookies[Name];
                return cookie.Value;
            }
            else
            {
                return "";
            }
        }

        private IntakeFormModel initModel()
        {
            
            IntakeFormModel model = new Models.IntakeFormModel();
            
            if (ReadCookie("LastRequestType") == "")
            {
                WriteCoockie("LastRequestType", "1");
            }
            if (ReadCookie("ServiceDeskName") == "")
            {
                WriteCoockie("ServiceDeskName", "");
            }
            model.VerzoekType = Convert.ToInt32(ReadCookie("LastRequestType"));
            model.Ticket = string.Empty;
            model.Message = string.Empty;
            model.Compagny_AutoComplete = string.Empty;
            model.ContactName = string.Empty;
            model.ContactPhone = string.Empty;
            model.ServiceDesk_AutoComplete = ReadCookie("ServiceDeskName");
            model.ServiceDesk = ReadCookie("ServiceDeskName");

            return model;
        }

        private KWS.Org FindCustomer(string Name)
        {
            KWS.GetOrgsRequest OrgReq = new KWS.GetOrgsRequest();
            var orgs = KasClient.ProcessRequest(OrgReq);
            var result = orgs.Orgs.FirstOrDefault(o => o.OrgName.Contains(Name));
            if (result == null)
            {
                result = new KWS.Org();
                result.OrgName = Name;
            }
            return result;
        }

        [Authorize]
        public JsonResult _Orgs()
        {
            KWS.GetOrgsRequest OrgReq = new KWS.GetOrgsRequest();
            var orgs = KasClient.ProcessRequest(OrgReq);
            List<string> list = new List<string>();
            foreach (var o in orgs.Orgs.OrderBy(o=>o.OrgName))
            {
                string n = o.OrgName;
                list.Add(n);
            }
            return Json(list.ToArray(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult SaveTicket(IntakeFormModel model)
        {
            AddIncidentRequest req = new AddIncidentRequest();
            Incident ticket = new Incident();

            string sum;
            string desc;

            if (model !=null)
            {
                //Default Ticket Config           
                ticket.AssigneeType = Intake.SDWS.AssigneeType.POOL;
                ticket.Assignee = "Eerste lijn";
                ticket.AssigneeEmail = ""; //ToDo: somthing to find the correct name and email.

                if (model.VerzoekType == 1)
                {
                    //Terugbelverzoek
                    try {
                        desc = string.Format("Dhr/mevr {0} van {1} heeft gebeld met het verzoek om teruggebeld te worden op {2}.<br />Bericht:<br />{3}", model.ContactName, model.Compagny_AutoComplete, model.ContactPhone, model.Message);
                        sum = string.Format("Dhr/mevr {0} heeft gebeld met het verzoek om teruggebeld te worden.", model.ContactName);
                        ticket.Description = desc;
                        ticket.IsUnread = true;
                        ticket.Organization = model.Compagny_AutoComplete;
                        ticket.OrganizationName = model.Compagny_AutoComplete;
                        ticket.OrganizationStaffName = model.ContactName;
                        ticket.ServiceDeskName = model.ServiceDesk;
                        ticket.Submitter = model.ContactName;
                        ticket.SubmitterPhone = model.ContactPhone;
                        ticket.Summary = sum;
                        SDWS.AddIncidentRequest newTicket = new AddIncidentRequest();
                        newTicket.AddSDIncident = ticket;
                        SDWS.AddIncidentResponse responce1 = sDesk.ProcessRequest(newTicket);
                        if (responce1.ErrorMessage.Length > 0)
                        {
                            MessageTitle = "Fout bij het aanmaken van het terugbel verzoek!!";
                            MessageSucces = false;
                            MessageDisplay = responce1.ErrorMessage;
                        }
                        else
                        {
                            MessageTitle = "Terugbel verzoek aangemaakt";
                            MessageDisplay = string.Format("Voor {0} is er een terugbelverzoek aangemaakt onder ticket {1}.", model.ContactName, responce1.AddSDIncidentResponse.IncidentNumber);
                        }
                    } catch (Exception e)
                    {
                        MessageTitle = "Fout bij het aanmaken van het terugbel verzoek!!";
                        MessageSucces = false;
                        MessageDisplay = e.Message;
                    }
                }
                else if (model.VerzoekType == 2)
                {
                    //Terugbelverzoek inzake bestaande ticket
                    try
                    {
                        SDWS.GetIncidentReq r = new GetIncidentReq();
                        SDWS.GetIncidentRequest filter = new GetIncidentRequest();
                        SDWS.Note n = NewNote();
                        List<SDWS.Note> NotesList = new List<Note>();

                        filter.IncidentNumber = model.Ticket;
                        r.IncidentRequest = filter;

                        var responce = sDesk.ProcessRequest(r);
                        //NotesList = responce.IncidentResponse.Notes.ToList<SDWS.Note>();

                        n.Text = string.Format("Dhr/mevr {0} van {1} heeft gebeld met het verzoek om teruggebeld te worden op {2}.<br />Bericht:<br />{3}", model.ContactName, model.Compagny_AutoComplete, model.ContactPhone, model.Message);
                        NotesList.Add(n);

                        ticket.id = responce.IncidentResponse.id;
                        ticket.IncidentNumber = responce.IncidentResponse.IncidentNumber;
                        ticket.IsUnread = true;
                        ticket.Notes = NotesList.ToArray();

                        SDWS.UpdateIncidentRequest update = new UpdateIncidentRequest();
                        update.UpdateSDIncident = ticket;

                        SDWS.UpdateIncidentResponse responce2 = sDesk.ProcessRequest(update);
                        if (responce2.ErrorMessage.Length > 0)
                        {
                            MessageTitle = "Fout bij het aanmaken van het terugbel verzoek!!";
                            MessageSucces = false;
                            MessageDisplay = responce2.ErrorMessage;
                        }
                        else
                        {
                            MessageTitle = string.Format("Terugbel verzoek voor ticket {0} aangemaakt", ticket.IncidentNumber);
                            MessageDisplay = string.Format("Voor {0} is er een terugbelverzoek aangemaakt onder ticket {1}.", model.ContactName, responce.IncidentResponse.IncidentNumber);
                        }
                    } catch (Exception e)
                    {
                        MessageTitle = "Fout bij het aanmaken van het terugbel verzoek!!";
                        MessageSucces = false;
                        MessageDisplay = e.Message;
                    }
                }
                else if (model.VerzoekType==3)
                {
                    //Nieuwe incident
                    try
                    {
                        sum = string.Format("Ticket aangemaakt voor {0} van {1}", model.ContactName, model.Compagny_AutoComplete);
                        desc = string.Format("{0} heeft het volgende gemeld.<br />Omschrijving:<br />{1}<br /><br />{0} is bereikbaar op {2}"
                            , model.ContactName, model.Message, model.ContactPhone);
                        ticket.Description = desc;
                        ticket.IsUnread = true;
                        ticket.Organization = model.Compagny_AutoComplete;
                        ticket.OrganizationName = model.Compagny_AutoComplete;
                        ticket.OrganizationStaffName = model.ContactName;
                        ticket.ServiceDeskName = model.ServiceDesk_AutoComplete;
                        ticket.Submitter = model.ContactName;
                        ticket.SubmitterPhone = model.ContactPhone;
                        ticket.Summary = sum;
                        
                        SDWS.AddIncidentRequest newTicket = new AddIncidentRequest();
                        newTicket.AddSDIncident = ticket;
                        SDWS.AddIncidentResponse responce3 = sDesk.ProcessRequest(newTicket);
                        if (responce3.ErrorMessage.Length > 0)
                        {
                            MessageTitle = "Fout bij het aanmaken van een nieuw ticket!!";
                            MessageSucces = false;
                            MessageDisplay = responce3.ErrorMessage;
                        }
                        else
                        {
                            MessageTitle = string.Format("Ticket {0} aangemaakt",responce3.AddSDIncidentResponse.IncidentNumber);
                            MessageDisplay = string.Format("Voor {0} is er een nieuw verzoek aangemaakt onder ticket {1}.", model.ContactName, responce3.AddSDIncidentResponse.IncidentNumber);
                        }
                    } catch (Exception e)
                    {
                        MessageTitle = "Fout bij het aanmaken van een nieuw ticket!!";
                        MessageSucces = false;
                        MessageDisplay = e.Message;
                    }
                }
                else
                {
                    //Update van bestaande ticket
                    try
                    {
                        SDWS.GetIncidentReq r = new GetIncidentReq();
                        SDWS.GetIncidentRequest filter = new GetIncidentRequest();
                        SDWS.Note n = NewNote();
                        List<SDWS.Note> NotesList = new List<Note>();

                        filter.IncidentNumber = model.Ticket_AutoComplete;
                        r.IncidentRequest = filter;

                        var responce = sDesk.ProcessRequest(r);
                        //NotesList = responce.IncidentResponse.Notes.ToList<SDWS.Note>();

                        n.Text = string.Format("Dhr/mevr {0} van {1} heeft een aanvulling gedaan op ticket {2}.<br />Bericht:<br />{3}<br /><br />{0} is bereikbaar op {4}.", model.ContactName, model.Compagny_AutoComplete, responce.IncidentResponse.IncidentNumber, model.ContactPhone, model.Message);
                        NotesList.Add(n);

                        ticket.id = responce.IncidentResponse.id;
                        ticket.IncidentNumber = responce.IncidentResponse.IncidentNumber;
                        ticket.IsUnread = true;
                        ticket.Notes = NotesList.ToArray();

                        SDWS.UpdateIncidentRequest update = new UpdateIncidentRequest();
                        update.UpdateSDIncident = ticket;

                        SDWS.UpdateIncidentResponse responce2 = sDesk.ProcessRequest(update);
                        if (responce2.ErrorMessage.Length > 0)
                        {
                            MessageTitle = string.Format("Fout bij het het aanvullen van ticket {0}!!",model.Ticket);
                            MessageSucces = false;
                            MessageDisplay = responce2.ErrorMessage;
                        }
                        else
                        {
                            MessageTitle = string.Format("Ticket {0} aangevuld", model.Ticket);
                            MessageDisplay = string.Format("Voor {0} is er een terugbelverzoek aangemaakt onder ticket {1}.", model.ContactName, responce.IncidentResponse.IncidentNumber);
                        }
                    }
                    catch (Exception e)
                    {
                        MessageTitle = string.Format("Fout bij het het aanvullen van ticket {0}!!", model.Ticket_AutoComplete);
                        MessageSucces = false;
                        MessageDisplay = e.Message;
                    }
                }                
            }

            
            return RedirectToAction("Index");
        }

        private SDWS.Note NewNote()
        {
            SDWS.Note note = new SDWS.Note();

            note.Billable = false;
            note.DateStartedSpecified = false;
            note.delete = false;
            note.HoursWorked = 0;
            note.SuppressNotify = false;
            note.Text = "";
            note.Timestamp = DateTime.Now;
            note.User = "System-Intake";
            note.Hidden = false;
            return note;
        }

        public ActionResult Index()
        {
            var model = initModel();
            if (MessageTitle.Length>0)
            {
                ViewBag.MessageTitle = MessageTitle;
                ViewBag.MessageSucces = MessageSucces;
                ViewBag.MessageDisplay = MessageDisplay;
            }
            MessageTitle = string.Empty;
            MessageSucces = true;
            MessageDisplay = string.Empty;

            return View(model);
        }
    }
}