using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace Intake.Models
{
    public class SettingsContext : DbContext
    {
        public SettingsContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<Setting> Settings { get; set; }

        public static SettingsContext Create()
        {
            return new SettingsContext();
        }
    }

    public class Setting
    {
        public int ID { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class IncidentModel
    {
        public string Assignee { get; set; }
        public SDWS.Attachment[] Attachments { get; set; }
        public string Category { get; set; }
        public DateTime CloseDateTime { get; set; }
        public bool CloseDateTimeSpecified { get; set; }
        public DateTime CreateDateTime { get; set; }
        public bool CreateDateTimeSpecified { get; set; }
        public string CurrentStage { get; set; }
        public SDWS.CustomField[] CustomFields { get; set; }
        public string Description { get; set; }
        public string EditingTemplate { get; set; }
        public string IncidentNumber { get; set; }
        public bool IsArchived { get; set; }
        public bool IsClosed { get; set; }
        public DateTime LastEditDateTime { get; set; }
        public string LockUser { get; set; }
        public SDWS.Note[] Notes { get; set; }
        public string OrganizationName { get; set; }
        public string Owner { get; set; }
        public string Resolution { get; set; }
        public string ResolutionNote { get; set; }
        public SDWS.ServiceDeskDefinition ServiceDeskDefinition { get; set; }
        public SDWS.SourceType SourceType { get; set; }
        public string Stage { get; set; }
        public string Status { get; set; }
        public string SubCategory { get; set; }
        public string Submitter { get; set; }
        public string SubmitterEmail { get; set; }
        public string Summary { get; set; }
    }

    public class jqxDateTime
    {
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
    }

    public class AdminSettings
    {
        public string server { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        //public int servicedeskId { get; set; }
        //public string servicedeskName { get; set; }
        public string DefaultScope { get; set; }
        public bool connected { get; set; }
    }

    public class ServiceDeskCBList
    {
        ServiceDeskCBModel[] List { get; set; }
    }

    public class ServiceDeskCBModel
    {
        public decimal ServiceDeskID { get; set; }
        public string ServiceDeskName { get; set; }
    }

    public class IncidentListRequestModel
    {
        public string searchText { get; set; }
        public List<string> options { get; set; }
    }

    public class GetTicketsModel
    {
        public IncidentListModel IncidentList { get; set; }
        public string ErrorLocation { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class IncidentListModel
    {
        public IncidentSummaryGridModel[] Incident { get; set; }
        public int totalIncidents { get; set; }
        public bool totalIncidentsSpecified { get; set; }
        public string nextStartingIncident { get; set; }

    }

    public class rootElements
    {
        public string Method { get; set; }
        public decimal TransactionID { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorLocation { get; set; }
    }

    [XmlRoot]
    public class GetOrgsResponse : rootElements
    {
        public string Get { get { return "<GetOrgsRequest></GetOrgsRequest>"; } }
        [XmlArrayAttribute]
        public Org[] Orgs { get; set; }
    }

    [XmlRoot]
    public class GetOrgsByScopeIDResponse : rootElements
    {
        public string Get { get { return "<GetOrgsByScopeIDRequest><ScopeID>Connect-IT</ScopeID></GetOrgsByScopeIDRequest>"; } }
        [XmlArrayAttribute]
        public Org[] Orgs { get; set; }
    }

    public class Org
    {
        public string OrgName { get; set; }
        public string OrgRef { get; set; }
        public string OrgId { get; set; }
        public string CustomerID { get; set; }
    }

    public class IncedentSummaryModel
    {
        public string ServiceDeskName { get; set; }
        public string IncidentNumber { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string Resolution { get; set; }
        public string Stage { get; set; }
        public string Severity { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Policy { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime LastEditDateTime { get; set; }
        public DateTime CloseDateTime { get; set; }
        public decimal OrgID { get; set; }
        public string OrganizationName { get; set; }
        public string Organization { get; set; }
        public string OrganizationStaffName { get; set; }
        public string OrganizationStaff { get; set; }
        public string OrganizationStaffEmail { get; set; }
        public string Machine { get; set; }
        public decimal MachineGuid { get; set; }
        public string MachineGroup { get; set; }
        public decimal MachineGroupGuid { get; set; }
        public string Submitter { get; set; }
        public string SubmitterEmail { get; set; }
        public string SubmitterPhone { get; set; }
        public string SubmitterType { get; set; }
        public bool IsUnread { get; set; }
    }

    #region ViewGridModels

    public class ScopesIds
    {
        private static List<ScopeIdModdel> ScopeIDList;
        private static DateTime UpdateTime = DateTime.Now.AddMinutes(15);
        private static SettingsContext db = SettingsContext.Create();
        private static Helpers.Kaseya.ServiceDeskWSClient sDesk = new Helpers.Kaseya.ServiceDeskWSClient(db.Settings.FirstOrDefault(s => s.Key == "ServiceDeskURI").Value, db.Settings.FirstOrDefault(s => s.Key == "KaseyaURI").Value);

        public static List<ScopeIdModdel> ListScopes
        {
            get
            {
                if (ScopeIDList == null || UpdateTime <= DateTime.Now)
                {
                    KWS.GetScopesRequest req = new KWS.GetScopesRequest();
                    var resp = sDesk.kWS.ProcessRequest(req);
                    List<ScopeIdModdel> list = new List<ScopeIdModdel>();

                    foreach (var sc in resp.Scopes)
                    {
                        ScopeIdModdel newScope = new ScopeIdModdel();
                        newScope.ID = list.Count;
                        newScope.ScopeID = sc.ScopeID;
                    }
                    ScopeIDList = list;
                }
                return ScopeIDList;
            }
        }
        public static List<ScopeIdModdel> GetScopesLike(string namestring)
        {
            var x = ScopeIDList.Where(d => d.ScopeID.ToLower().Contains(namestring.ToLower().Trim()));
            return x.ToList();

        }
    }
    
    public class ServiceDeskNames
    {
        private static List<ServiceDeskNameModel> ServiceDeskList;
        private static DateTime UpdateTime = DateTime.Now.AddMinutes(15);
        private static SettingsContext db = SettingsContext.Create();
        private static Helpers.Kaseya.ServiceDeskWSClient sDesk = new Helpers.Kaseya.ServiceDeskWSClient(db.Settings.FirstOrDefault(s => s.Key == "ServiceDeskURI").Value, db.Settings.FirstOrDefault(s => s.Key == "KaseyaURI").Value);

        public static List<ServiceDeskNameModel> ListServiceDesks
        {
            get
            {
                if (ServiceDeskList == null || UpdateTime <= DateTime.Now)
                {
                    SDWS.GetServiceDesksRequest sdReq = new SDWS.GetServiceDesksRequest();
                    var sdResponse = sDesk.ProcessRequest(sdReq);
                    
                    List<ServiceDeskNameModel> list = new List<ServiceDeskNameModel>();
                    
                    foreach (var sd in sdResponse.ServiceDesks.OrderBy(s => s.ServiceDeskName))
                    {
                        ServiceDeskNameModel i = new ServiceDeskNameModel();
                        i.ID = sd.ServiceDeskID;
                        i.ServiceDeskName = sd.ServiceDeskName;
                        if (!list.Contains(i)) { list.Add(i); }
                    }
                    ServiceDeskList = list;
                }
                return ServiceDeskList;
            }
        }
        public static List<ServiceDeskNameModel> GetServiceDeskLikeName(string namestring)
        {
            var x = ListServiceDesks.Where(d => d.ServiceDeskName.ToLower().Contains(namestring.ToLower().TrimStart()));
            return x.ToList();

        }
    }

    public class CompagnyNames
    {
        private static List<CompagnyNameModel> CompagnyList;
        private static DateTime UpdateTime = DateTime.Now.AddMinutes(15);
        private static SettingsContext db = SettingsContext.Create();
        private static Helpers.Kaseya.KaseyaWSClient KasClient = new Helpers.Kaseya.KaseyaWSClient(db.Settings.FirstOrDefault(s => s.Key == "KaseyaURI").Value);

        public static List<CompagnyNameModel> ListCompagys
        {
            get
            {
                if (CompagnyList == null || UpdateTime <= DateTime.Now)
                {
                    KWS.GetScopesRequest scReq = new KWS.GetScopesRequest();
                    var scResponse = KasClient.ProcessRequest(scReq);
                    //KWS.AssignScopeRequest asReq = new KWS.AssignScopeRequest();
                    //KWS.AddUserToScopeRequest rcs = new KWS.AddUserToScopeRequest();
                    //KWS.AddUserToScopeResponse asResponce = null;
                     
                    //rcs.UserName = db.Settings.FirstOrDefault(s => s.Key == "KaseyaUser").Value;

                    List<CompagnyNameModel> list = new List<CompagnyNameModel>();

                    //foreach (var scope in scResponse.Scopes)
                    //{
                        //rcs.ScopeID = scope.ScopeID;
                        //asResponce = KasClient.ProcessRequest(rcs);
                        //if (asResponce.ErrorMessage.Length > 0)
                        //{
                        //    throw new Exception(asResponce.ErrorMessage);
                        //}
                        KWS.GetOrgsRequest r = new KWS.GetOrgsRequest();
                        var response = KasClient.ProcessRequest(r);
                        
                        foreach (var sd in response.Orgs.OrderBy(s => s.OrgName))
                        {
                            CompagnyNameModel i = new CompagnyNameModel();
                            i.ID = sd.OrgRef;
                            i.CompagnyName = sd.OrgName;

                            if (!list.Contains(i)) { list.Add(i); }
                        }
                    //}
                    CompagnyList = list;
                    //rcs.ScopeID = db.Settings.FirstOrDefault(s => s.Key == "DefaultScope").Value;
                    //asResponce = KasClient.ProcessRequest(rcs);
                    //if (asResponce.ErrorMessage.Length > 0)
                    //{
                    //    throw new Exception(asResponce.ErrorMessage);
                    //}
                }
                return CompagnyList;
            }
        }
        public static List<CompagnyNameModel> GetCompagnyLikeName(string namestring)
        {
            var x = ListCompagys.Where(d => d.CompagnyName.ToLower().Contains(namestring.ToLower().TrimStart()));
            return x.ToList();
        }
    }

    public class TicketNumbers
    {
        private static List<TicketNumberModel> TicketList;
        private static DateTime UpdateTime = DateTime.Now.AddMinutes(2); //ToDo: iets grotere interval
        private static SettingsContext db = SettingsContext.Create();
        private static Helpers.Kaseya.ServiceDeskWSClient sDesk = new Helpers.Kaseya.ServiceDeskWSClient(db.Settings.FirstOrDefault(s => s.Key == "ServiceDeskURI").Value, db.Settings.FirstOrDefault(s => s.Key == "KaseyaURI").Value);

        public static List<TicketNumberModel> ListTickets
        {
            get
            {
                if (TicketList == null || UpdateTime <= DateTime.Now)
                {
                    SDWS.GetServiceDesksRequest r = new SDWS.GetServiceDesksRequest();
                    var response = sDesk.ProcessRequest(r);
                    List<TicketNumberModel> list = new List<TicketNumberModel>();
                    foreach (var sd in response.ServiceDesks.OrderBy(s => s.ServiceDeskName))
                    {
                        SDWS.GetIncidentListRequest req = new SDWS.GetIncidentListRequest();
                        SDWS.IncidentListFilter filter = new SDWS.IncidentListFilter();

                        filter.IncidentCountSpecified = false;
                        filter.ServiceDeskName = sd.ServiceDeskName;
                        req.IncidentListRequest = filter;
                        SDWS.GetIncidentListResponse items = sDesk.ProcessRequest(req);

                        foreach (var t in items.IncidentList.Incident)
                        {
                            TicketNumberModel i = new TicketNumberModel();
                            i.ID = t.IncidentNumber;
                            i.Ticket = string.Format("{0} - {1}",t.IncidentNumber,t.Summary);
                            i.Org = t.OrganizationName;
                            i.ServiceDesk = t.ServiceDeskName;

                            list.Add(i);
                        }
                    }
                    TicketList = list;
                }
                return TicketList;
            }
        }
        
        public static List<TicketNumber> GetTicketLikeName(string namestring, string ServiceDesk = null, string Org = null)
        {
            var x = ListTickets;
            if (ServiceDesk != null && ServiceDesk != "")
            {
                x = x.Where(t => t.ServiceDesk.ToLower() == ServiceDesk.ToLower()).ToList();
            }
            if (Org != null && Org != "")
            {
                x = x.Where(t => t.Org == Org).ToList();
            }
            x = x.Where(d => d.ID.ToLower().Contains(namestring.ToLower().TrimStart())).ToList();
            var l =  x.Select(t=> new { t.ID, t.Ticket } ).ToList();
            List<TicketNumber> list = new List<TicketNumber>();
            foreach (var item in l)
            {
                TicketNumber t = new TicketNumber();
                t.ID = item.ID;
                t.Ticket = item.Ticket;
                list.Add(t);
            }
            return list;
        }
    }

    public class ServiceDeskNameModel
    {
        public decimal ID { get; set; }
        public string ServiceDeskName { get; set; }
    }

    public class CompagnyNameModel
    {
        public string ID { get; set; }
        public string CompagnyName { get; set; }
    }

    public class TicketNumberModel: TicketNumber
    {
        public string ServiceDesk { get; set; }
        public string Org { get; set; }
    }

    public class ScopeIdModdel
    {
        public int ID { get; set; }
        public string ScopeID { get; set; }
    }

    public class TicketNumber
    {
        public string ID { get; set; }
        public string Ticket { get; set; }
    }

    public class IncidentSummaryGridModel
    {
        public string Category { get; set; }
        [Display(Name = "Ticket")]
        public string IncidentNumber { get; set; }
        public bool IsUnread { get; set; }
        [Display(Name = "Ingediend door")]
        public string Submitter { get; set; }
        [Display(Name = "Klant")]
        public string OrganizationName { get; set; }
        public string Stage { get; set; }
        [Display(Name = "Samenvatting")]
        public string Summary { get; set; }
        [Display(Name = "Omschrijving")]
        public string Description { get; set; }
        public string Status { get; set; }
        [Display(Name = "Oplossing")]
        public string Resolution { get; set; }
    }

    public class IntakeFormModel
    {
        [Required]
        [Display(Name = "Servicedesk")]
        public string ServiceDesk { get; set; }
        public string ServiceDesk_AutoComplete { get; set; }
        [Required]
        [MinLength(length: 2, ErrorMessage = "Klant opgeven")]
        [Display(Name = "Klant")]
        public string Compagny { get; set; }
        public string Compagny_AutoComplete { get; set; }
        [Required]
        [MinLength(length: 2, ErrorMessage = "Contact opgeven opgeven")]
        [Display(Name = "Contactpersoon")]
        public string ContactName { get; set; }
        [Required]
        [Display(Name = "Telefoonnummer")]
        public string ContactPhone { get; set; }
        [Required]
        [Display(Name = "Verzoek Tot")]
        public int VerzoekType { get; set; }
        [Display(Name = "Ticket nummer")]
        public string Ticket { get; set; }
        public string Ticket_AutoComplete { get; set; }
        [Display(Name = "Notitie")]
        public string Message { get; set; }
    }

    public class TicketFilter
    {
        public string text { get; set; }
        public int OrgId { get; set; }
        public string ServiceDesk { get; set; }
    }
    //public class NoteModel
    //{
    //    public DateTime TimeStamp { get; set; }
    //    public string Text { get; set; }
    //    public string User { get; set; }
    //}

    //public class NotesGridModel
    //{
    //    public NoteModel[] Notes { get; set; }
    //}

    //public class IncidentSummaryListModel
    //{
    //    SDWS.IncidentList items = new SDWS.IncidentList();
    //    public IncedentSummaryModel[] Incident { get; set; }
    //    public string NextStartingIncident { get; set; }
    //}

    //

    #endregion
}