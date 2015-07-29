using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace Helpdesk_V0._1.Models
{
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
        public int servicedeskId { get; set; }
        public string servicedeskName { get; set; }
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

    #region GridModels
    public class NoteModel
    {
        public DateTime TimeStamp { get; set; }
        public string Text { get; set; }
        public string User { get; set; }
    }

    public class NotesGridModel
    {
        public NoteModel[] Notes { get; set; }
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

    public class IncidentSummaryListModel
    {
        SDWS.IncidentList items = new SDWS.IncidentList();
        public IncedentSummaryModel[] Incident { get; set; }
        public string NextStartingIncident { get; set; }
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

    #endregion
}