using Intake.Models;
using Intake.SDWS;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.DirectoryServices;
using System.Linq;
using System.Security.Principal;
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

        #region Cookie
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
        #endregion

        private string uEmail(string uid)
        {
            DirectorySearcher dirSearcher = new DirectorySearcher();
            DirectoryEntry entry = new DirectoryEntry(dirSearcher.SearchRoot.Path);
            IPrincipal userPrincipal = HttpContext.User;
            WindowsIdentity windowsId = userPrincipal.Identity as WindowsIdentity;
            
            //dirSearcher.Filter = "(&(objectClass=user)(objectcategory=person)(LDAP://<SID=" + windowsId.User.Value + ">))";
            using (DirectoryEntry userDirectoryEntry = new DirectoryEntry("LDAP://<SID=" + windowsId.User.Value + ">"))
            {
                try
                {
                    return userDirectoryEntry.Properties["mail"].ToString();
                }
                catch
                {
                    return "";
                }
            }
        }

        private class UserInfo
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
        }
        private UserInfo GetUserInfo(string uid)
        {
            UserInfo result = new UserInfo();

            //DirectorySearcher dirSearcher = new DirectorySearcher();
            //DirectoryEntry entry = new DirectoryEntry(dirSearcher.SearchRoot.Path);
            IPrincipal userPrincipal = HttpContext.User;
            WindowsIdentity windowsId = userPrincipal.Identity as WindowsIdentity;

            //dirSearcher.Filter = "(&(objectClass=user)(objectcategory=person)(LDAP://<SID=" + windowsId.User.Value + ">))";
            using (DirectoryEntry userDirectoryEntry = new DirectoryEntry("LDAP://<SID=" + windowsId.User.Value + ">"))
            {
                try
                {
                    result.Email = userDirectoryEntry.Properties["mail"].Value.ToString();
                    result.FullName = userDirectoryEntry.Properties["cn"].Value.ToString();
                    result.Phone = userDirectoryEntry.Properties["telephoneNumber"].Value.ToString();
                }
                catch(Exception ex)
                {
                    
                }
            }

            return result;
        }
        private string uFullName(string uid)
        {
            DirectorySearcher dirSearcher = new DirectorySearcher();
            DirectoryEntry entry = new DirectoryEntry(dirSearcher.SearchRoot.Path);
            IPrincipal userPrincipal = HttpContext.User;
            WindowsIdentity windowsId = userPrincipal.Identity as WindowsIdentity;

            //dirSearcher.Filter = "(&(objectClass=user)(objectcategory=person)(LDAP://<SID=" + windowsId.User.Value + ">))";
            using (DirectoryEntry userDirectoryEntry = new DirectoryEntry("LDAP://<SID=" + windowsId.User.Value + ">"))
            {
                try
                {
                    return userDirectoryEntry.Properties["cn"].ToString();
                }
                catch
                {
                    return "";
                }
            }
        }

        private string GetAssignee(ServiceDeskDefinition ServiceDeskDef)
        {
            var assignee = ServiceDeskDef.Participants.FirstOrDefault(p => p.@ref.Contains(ServiceDeskDef.Name) && p.@ref.Contains("Eerstelijn"));

            return assignee.@ref;
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

        private Incident CreateTicket(string ServiceDesk)
        {
            Incident ticket = new Incident();
            string InitString;

            if (ServiceDesk == "MSO_Servicedesk")
            {
                InitString = "MSO_Eerstelijn";
            }
            else
            {
                InitString = "VDT_Eerstelijn";
            }

            GetServiceDeskRequest sdReq = new GetServiceDeskRequest();
            ServiceDeskDefinitionRequest sdFilter = new ServiceDeskDefinitionRequest();
            sdFilter.ServiceDeskName = ServiceDesk;
            sdReq.ServiceDeskDefinitionRequest = sdFilter;

            var sdRes = sDesk.ProcessRequest(sdReq);

            //Default Ticket Config           
            ticket.AssigneeType = Intake.SDWS.AssigneeType.POOL;
            ticket.Assignee = GetAssignee(sdRes.ServiceDeskDefinitionResponse);
            ticket.AssigneeTypeSpecified = true;
            //ticket.AssigneeEmail = ""; //ToDo: somthing to find the correct name and email.
            ticket.EditingTemplate = sdRes.ServiceDeskDefinitionResponse.EditingTemplate;
            ticket.IsUnread = true;
            ticket.IsUnreadSpecified = true;
            ticket.LastEditDateTime = DateTime.Now;
            ticket.LastEditDateTimeSpecified = true;
            ticket.Policy = sdRes.ServiceDeskDefinitionResponse.DefaultPolicy;
            ticket.Priority = sdRes.ServiceDeskDefinitionResponse.Priority[0].@ref;
            ticket.ServiceDeskDefinition = sdRes.ServiceDeskDefinitionResponse;
            ticket.ServiceDeskName = sdRes.ServiceDeskDefinitionResponse.Name;
            ticket.Severity = sdRes.ServiceDeskDefinitionResponse.Severity.FirstOrDefault(s => s.id == sdRes.ServiceDeskDefinitionResponse.DefaultSeverity).@ref;
            ticket.SourceType = SourceType.Other;
            ticket.SourceTypeSpecified = true;
            ticket.Stage = sdRes.ServiceDeskDefinitionResponse.Stages.FirstOrDefault(s => s.Initialization == InitString).Item.@ref;
            ticket.Status = sdRes.ServiceDeskDefinitionResponse.Status.FirstOrDefault(s => s.Value == "Nieuw").@ref;
            var user = GetUserInfo(User.Identity.Name);
            ticket.Submitter = user.FullName;
            ticket.SubmitterEmail = user.Email;
            ticket.SubmitterPhone = user.Phone;
            var extraFields = sdRes.ServiceDeskDefinitionResponse.CustomFields;
            List<SDWS.CustomField> list = new List<SDWS.CustomField>();
            foreach (var item in extraFields)
            {
                CustomField cf = new CustomField();

                cf.fieldName = item.FieldName;
                if (cf.fieldName == "Urgentie")
                {
                    cf.Value = "1 - Laag";
                }
                else if (cf.fieldName == "Afkomst")
                {
                    cf.Value = "Telefoon";
                }
                else if (cf.fieldName == "Afkomst")
                {
                    cf.Value = "Telefoon";
                }
                {
                    cf.Value = item.DefaultValue;
                }
                list.Add(cf);
            }
            ticket.CustomFields = list.ToArray();

            return ticket;
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult SaveTicket(IntakeFormModel model)
        {
            AddIncidentRequest req = new AddIncidentRequest();
            
            string sum;
            string desc;
            
            if (model !=null)
            {
                if (Request.Params[11] == "Cancel")
                {
                    //clear screen
                    return RedirectToAction("Index");
                }

                //Write last selected in cookie
                WriteCoockie("ServiceDeskName", model.ServiceDesk_AutoComplete);
                //WriteCoockie("LastRequestType", model.VerzoekType.ToString());


                if (model.VerzoekType == 1)
                {
                    //Terugbelverzoek
                    try {
                        Incident ticket = CreateTicket(model.ServiceDesk_AutoComplete);
                        desc = string.Format("Dhr/mevr {0} van {1} heeft gebeld met het verzoek om teruggebeld te worden op {2}.<br />Bericht:<br />{3}", model.ContactName, model.Compagny_AutoComplete, model.ContactPhone, model.Message);
                        sum = string.Format("Dhr/mevr {0} heeft gebeld met het verzoek om teruggebeld te worden.", model.ContactName);
                        ticket.Description = desc;
                        ticket.Organization = model.Compagny;
                        ticket.OrganizationName = model.Compagny_AutoComplete;
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
                        var updated = responce.IncidentResponse;
                        updated.LastEditDateTime = DateTime.Now;
                        updated.LastEditDateTimeSpecified = true;
                        if (updated.Notes != null)
                        {
                            foreach (var note in updated.Notes)
                            {
                                NotesList.Add(note);
                            }
                        }
                        n.Text = string.Format("Dhr/mevr {0} van {1} heeft gebeld met het verzoek om teruggebeld te worden op {2}.<br />Bericht:<br />{3}", model.ContactName, model.Compagny_AutoComplete, model.ContactPhone, model.Message);
                        NotesList.Add(n);
                        updated.Notes = NotesList.ToArray();
                        
                        SDWS.UpdateIncidentRequest update = new UpdateIncidentRequest();
                        update.UpdateSDIncident = updated;

                        SDWS.UpdateIncidentResponse responce2 = sDesk.ProcessRequest(update);
                        if (responce2.ErrorMessage.Length > 0)
                        {
                            MessageTitle = "Fout bij het aanmaken van het terugbel verzoek!!";
                            MessageSucces = false;
                            MessageDisplay = responce2.ErrorMessage;
                        }
                        else
                        {
                            MessageTitle = string.Format("Terugbel verzoek voor ticket {0} aangemaakt", updated.IncidentNumber);
                            MessageDisplay = string.Format("Voor {0} is er een terugbelverzoek aangemaakt onder ticket {1}.", model.ContactName, updated.IncidentNumber);
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
                        Incident ticket = CreateTicket(model.ServiceDesk_AutoComplete);
                        sum = string.Format("Ticket aangemaakt voor {0} van {1}", model.ContactName, model.Compagny_AutoComplete);
                        desc = string.Format("{0} heeft het volgende gemeld.<br />Omschrijving:<br />{1}<br /><br />{0} is bereikbaar op {2}"
                            , model.ContactName, model.Message, model.ContactPhone);
                        ticket.Description = desc;
                        ticket.Organization = model.Compagny;
                        ticket.OrganizationName = model.Compagny_AutoComplete;
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

                        filter.IncidentNumber = model.Ticket;
                        r.IncidentRequest = filter;
                        r.IncidentRequest.IncludeNotes = true;
                        
                        var responce = sDesk.ProcessRequest(r);

                        var updated = responce.IncidentResponse;
                        updated.LastEditDateTime = DateTime.Now;
                        updated.LastEditDateTimeSpecified = true;
                        if (updated.Notes != null)
                        {
                            foreach (var note in updated.Notes)
                            {
                                NotesList.Add(note);
                            }
                        }
                        n.Text = string.Format("Dhr/mevr {0} van {1} heeft een aanvulling gedaan op ticket {2}.<br />Bericht:<br />{3}<br /><br />{0} is bereikbaar op {4}.", model.ContactName, model.Compagny_AutoComplete, responce.IncidentResponse.IncidentNumber, model.ContactPhone, model.Message);
                        NotesList.Add(n);
                        updated.Notes = NotesList.ToArray();
                                                
                        SDWS.UpdateIncidentRequest update = new UpdateIncidentRequest();
                        update.UpdateSDIncident = updated;
                        
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

        private Note NewNote()
        {
            SDWS.Note note = new SDWS.Note();

            note.Billable = false;
            note.DateStartedSpecified = false;
            note.delete = false;
            note.HoursWorked = 0;
            note.SuppressNotify = false;
            note.Text = "";
            note.Timestamp = DateTime.Now;
            note.User = "kaseyasupport";
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