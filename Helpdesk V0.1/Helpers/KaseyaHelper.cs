using Helpdesk_V0._1.Controllers;
using Helpdesk_V0._1.KWS;
using Helpdesk_V0._1.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace Helpdesk_V0._1.Helpers
{
    public static class UserTools
    {
        public static string UserToFilter(string Name)
        {
            //var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var manager = new AccountController(); //IdentityManager.Store.Users.GetUser(Name);
            var user = manager.UserManager.FindByNameAsync(Name);
            
            return user.Result.Company;
        }
    }

    public static class Configuration
    {
        static ApplicationDbContext db = ApplicationDbContext.Create();

        public static SDWS.ServiceDeskDefinition GetServiceDeskConfiguration
        {
            get
            {
                if (HttpContext.Current.Application["defaultDefinition"] == null || DateTime.Now >= ((DateTime)(HttpContext.Current.Application["modifyDate"] ?? DateTime.MinValue)).AddHours(2))
                {
                    
                    
                    ServiceDeskWSClient sDesk = new ServiceDeskWSClient(db.Settings.FirstOrDefault(s => s.Key == "ServiceDeskURI").Value.ToString(), db.Settings.FirstOrDefault(s => s.Key == "KaseyaURI").Value.ToString());
                    SDWS.GetServiceDeskRequest req = new SDWS.GetServiceDeskRequest();
                    SDWS.ServiceDeskDefinitionRequest filter = new SDWS.ServiceDeskDefinitionRequest();
                    filter.ServiceDeskName = db.Settings.FirstOrDefault(s => s.Key == "ServiceDeskName").Value.ToString();
                    req.ServiceDeskDefinitionRequest = filter;
                    GetServiceDeskConfiguration = sDesk.ProcessRequest(req).ServiceDeskDefinitionResponse;
                }

                return HttpContext.Current.Application["defaultDefinition"] as SDWS.ServiceDeskDefinition;
            }
            set
            {
                lock (HttpContext.Current.Application)
                {
                    HttpContext.Current.Application["defaultDefinition"] = value;
                    HttpContext.Current.Application["modifyDate"] = DateTime.Now;
                }
            }
        }
    }
    public class KaseyaWSClient
    {
        #region Private Data Members
        static ApplicationDbContext db = ApplicationDbContext.Create();
        KWS.KaseyaWS clientWS;
        decimal _id;
        #endregion

        #region Public Data Members

        public decimal SessionID
        {
            get
            {
                if (_id == 0)
                {
                    AuthenticationResponse resp = null;

                    resp = Authenticate(db.Settings.FirstOrDefault(s => s.Key == "KaseyaUser").Value.ToString(), db.Settings.FirstOrDefault(s => s.Key == "KaseyaPassword").Value.ToString(), "SHA-256");
                    return resp.SessionID;
                }
                else
                {
                    return _id;
                }
            }
            set
            {
                _id = value;
            }
        }

        #endregion

        #region Constructors

        public KaseyaWSClient(string URL)
        {
            clientWS = new KWS.KaseyaWS();
            clientWS.Url = URL;
        }

        #endregion

        #region Public Methods
        public object RequestToModel(object req, string Get)
        {

            XmlSerializer serializer = new XmlSerializer(req.GetType());

            string xml = this.ProcessRequest(Get);

            return serializer.Deserialize(new StringReader(xml));
        }

        public List<string> ImportUsers(Models.Org[] Users)
        {
            List<string> result = new List<string>();
            string defaultPW = "welkom";
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            foreach (Models.Org O in Users)
            {
                try
                {
                    if (userManager.FindByName(CleanUsername(O.OrgRef)) == null)
                    {
                        var user = new ApplicationUser { UserName = CleanUsername(O.OrgRef), Email = "Helpdesk@connect-it.com", Company = O.OrgName, LockoutEnabled = true };
                        var r = userManager.Create(user, defaultPW);
                        if (r.Succeeded)
                        {
                            userManager.AddToRole(user.Id, "Customer");
                        }
                        else
                        {
                            foreach (var Message in r.Errors)
                            {
                                result.Add(string.Format("Voor klant {0} is geen inlog aangemaakt. De foutmelding is: {1}", O.OrgName, Message));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    result.Add(string.Format("Voor klant {0} is geen inlog aangemaakt. De foutmelding is: {1}", O.OrgName, e.Message));
                }
            }
            return result;
        }

        //Since the proxy objects created by our Web Reference
        // behave exactly like locally generated Business Entity Objects.
        //Note:: this is not a "Primative" method call.  It is the "Complex"
        //version, using object interface
        public KWS.AuthenticationResponse Authenticate(string UserName, string Password, string HashingAlgorithm)
        {

            KWS.AuthenticationRequest req = new KWS.AuthenticationRequest();

            //call hash.dll here to encrypt password - ALL CLIENT CODE MUST USE hash.dll
            hash Hash = new hash();
            //password goes in by ref and will be modified by hash
            string RandomNumber = Hash.HashToCoveredPassword(UserName, ref Password, HashingAlgorithm);
            //set the unmodified username
            req.UserName = UserName;
            //set the hash generated single use encryption cover (2nd SHA-n hash pass)
            req.RandomNumber = RandomNumber;
            //set the double SHA-n hash encrypted password
            req.CoveredPassword = Password;
            //if (String.IsNullOrEmpty(HashingAlgorithm))
            //    req.HashingAlgorithm = "SHA-1";
            //else
            //    req.HashingAlgorithm = HashingAlgorithm;


            AuthenticationResponse resp = clientWS.Authenticate(req);
            SessionID = resp.SessionID;

            return resp;

        }

        public string ProcessRequest(string XmlIn)
        {
            //we always set the current SessionID, otherwise KaseyaWS security will fail the transaction
            string s = SetSessionID(XmlIn);

            // logic ladder identifying the method the xml is meant for.
            // This is reliable since WebMethods interfaces follow this pattern::
            // WebMethodName+Response WebMethodName(WebMethodName+Request) 
            if (s.Contains("<AddMachGroupToScopeRequest>"))
            {
                // For the "primative" methods, the primative datatype 'string' expresses the data 
                // and serialization / deserialization occurs on the web server.
                // We use "primative" here since the test app's UI is XML
                return clientWS.PrimitiveAddMachGroupToScope(s);
            }
            else if (s.Contains("<AddOrgRequest>"))
            {
                return clientWS.PrimitiveAddOrg(s);
            }
            else if (s.Contains("<AddOrgDepartment>"))
            {
                return clientWS.PrimitiveAddOrgDepartment(s);
            }
            else if (s.Contains("<AddOrgDeptStaffRequest>"))
            {
                return clientWS.PrimitiveAddOrgDeptStaff(s);
            }
            else if (s.Contains("<AddOrgToScopeRequest>"))
            {
                return clientWS.PrimitiveAddOrgToScope(s);
            }
            else if (s.Contains("<AddScopeRequest>"))
            {
                return clientWS.PrimitiveAddScope(s);
            }
            else if (s.Contains("<AddScopeOrgRequest>"))
            {
                return clientWS.PrimitiveAddScopeOrg(s);
            }
            else if (s.Contains("<AddTicRequestRequest>"))
            {
                return clientWS.PrimitiveAddTicRequest(s);
            }
            else if (s.Contains("<AddUserToRoleRequest>"))
            {
                return clientWS.PrimitiveAddUserToRole(s);
            }
            else if (s.Contains("<AddUserToScopeRequest>"))
            {
                return clientWS.PrimitiveAddUserToScope(s);
            }
            else if (s.Contains("<CloseAlarmRequest>"))
            {
                return clientWS.PrimitiveCloseAlarm(s);
            }
            // missing CreateAdmin
            // missing CreateAgentInstallPackage
            else if (s.Contains("<CreateRoleRequest>"))
            {
                return clientWS.PrimitiveCreateRole(s);
            }
            // missing DeleteAdmin
            // missing DeleteAgent 
            //DeleteAgentInstallPackage
            else if (s.Contains("<DeleteMachineGroupRequest>"))
            {
                return clientWS.PrimitiveDeleteMachineGroup(s);
            }
            else if (s.Contains("<DeleteOrgRequest>"))
            {
                return clientWS.PrimitiveDeleteOrg(s);
            }
            else if (s.Contains("<DeleteScopeRequest>"))
            {
                return clientWS.PrimitiveDeleteScope(s);
            }
            //DisableAdmin
            else if (s.Contains("<EchoMtRequest>"))
            {
                return clientWS.PrimitiveEchoMt(s);
            }
            //EnableAdmin
            else if (s.Contains("<GetAlarmRequest>"))
            {
                return clientWS.PrimitiveGetAlarm(s);
            }
            else if (s.Contains("<GetAlarmListRequest>"))
            {
                return clientWS.PrimitiveGetAlarmList(s);
            }
            // GetGroupLicenseInfo
            else if (s.Contains("<GetLogEntryRequest>"))
            {
                return clientWS.PrimitiveGetLogEntry(s);
            }
            else if (s.Contains("<GetMachineRequest>"))
            {
                return clientWS.PrimitiveGetMachine(s);
            }
            /*
            else if (s.Contains("<GetMachineCollectionListRequest>"))
            {
                return clientWS.PrimitiveGetMachineCollectionList(s);
            }
            */
            else if (s.Contains("<GetMachineGroupListRequest>"))
            {
                return clientWS.PrimitiveGetMachineGroupList(s);
            }
            else if (s.Contains("<GetMachineListRequest>"))
            {
                return clientWS.PrimitiveGetMachineList(s);
            }
            else if (s.Contains("<GetMachineUptimeRequest>"))
            {
                return clientWS.PrimitiveGetMachineUptime(s);
            }
            else if (s.Contains("<GetNotesListRequest>"))
            {
                return clientWS.PrimitiveGetNotesList(s);
            }
            else if (s.Contains("<GetOrgLocationRequest>"))
            {
                return clientWS.PrimitiveGetOrgLocation(s);
            }
            else if (s.Contains("<GetOrgTypesRequest>"))
            {
                return clientWS.PrimitiveGetOrgTypes(s);
            }
            else if (s.Contains("<GetOrgsRequest>"))
            {
                return clientWS.PrimitiveGetOrgs(s);
            }
            else if (s.Contains("<GetOrgsByScopeIDRequest>"))
            {
                return clientWS.PrimitiveGetOrgsByScopeID(s);
            }
            // GetPackageURLs
            else if (s.Contains("<GetPartnerUserLocationRequest>"))
            {
                return clientWS.PrimitiveGetPartnerUserLocation(s);
            }
            else if (s.Contains("<GetRolesRequest>"))
            {
                return clientWS.PrimitiveGetRoles(s);
            }
            else if (s.Contains("<GetScopesRequest>"))
            {
                return clientWS.PrimitiveGetScopes(s);
            }
            else if (s.Contains("<GetTicRequestTicketRequest>"))
            {
                return clientWS.PrimitiveGetTicRequestTicket(s);
            }
            else if (s.Contains("<GetTicketRequest>"))
            {
                return clientWS.PrimitiveGetTicket(s);
            }
            else if (s.Contains("<GetTicketListRequest>"))
            {
                return clientWS.PrimitiveGetTicketList(s);
            }
            else if (s.Contains("<GetTicketNotes>"))
            {
                return clientWS.PrimitiveGetTicketNotes(s);
            }
            else if (s.Contains("<GetVerboseMachineGroupListRequest>"))
            {
                return clientWS.PrimitiveGetVerboseMachineGroupList(s);
            }
            // MergeAgent
            else if (s.Contains("<MoveMachineToAnotherGroupRequest>"))
            {
                return clientWS.PrimitiveMoveMachineToAnotherGroup(s);
            }
            else if (s.Contains("<RemoveUserFromRoleRequest>"))
            {
                return clientWS.PrimitiveRemoveUserFromRole(s);
            }
            else if (s.Contains("<RemoveUserFromScopeRequest>"))
            {
                return clientWS.PrimitiveRemoveUserFromScope(s);
            }
            //
            else if (s.Contains("<RenameMachineRequest>"))
            {
                return clientWS.PrimitiveRenameMachine(s);
            }
            else if (s.Contains("<ResetPasswordRequest>"))
            {
                return clientWS.PrimitiveResetPassword(s);
            }
            // SetAdminPassword
            else if (s.Contains("<SetLicenseByOrgRequest>"))
            {
                return clientWS.PrimitiveSetLicenseByOrg(s);
            }
            else if (s.Contains("<SetPartnerUserLocationRequest>"))
            {
                return clientWS.PrimitiveSetPartnerUserLocation(s);
            }
            else if (s.Contains("<UpdateOrgRequest>"))
            {
                return clientWS.PrimitiveUpdateOrg(s);
            }
            else if (s.Contains("<UpdateTicketRequest>"))
            {
                return clientWS.PrimitiveUpdateTicket(s);
            }
            else if (s.Contains("<UpdateUserRequest>"))
            {
                return clientWS.PrimitiveUpdateUser(s);
            }

            return "Unknown XML";

        }

        public string SetSessionID(string payload)
        {
            if (SessionID == 0)
            {
                AuthenticationResponse resp = null;

                resp = Authenticate(db.Settings.FirstOrDefault(s => s.Key == "KaseyaUser").Value.ToString(), db.Settings.FirstOrDefault(s => s.Key == "KaseyaPassword").Value.ToString(), "SHA-256");
            }

            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(payload);

            XmlNode xNode = xDoc.SelectSingleNode("//SessionID");

            if (xNode != null)
            {
                xNode.InnerXml = SessionID.ToString();
            }
            else
            {
                XmlNode newNode = xDoc.CreateElement("SessionID");
                newNode.InnerXml = SessionID.ToString();
                xDoc.DocumentElement.AppendChild(newNode);

            }

            return xDoc.OuterXml;

        }

        #endregion

        #region Private Methods

        private string CleanUsername(string name)
        {
            string result = name;
            string[] replace = new string[] { "-", "_", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "/", "=", "+", "~", "`",
                "\"", "'", ";", ":", "<", ">", "/", "\\", "[", "]", "|" };

            foreach (string r in replace)
            {
                result = result.Replace(r, "");
            }

            return result;
        }



        #endregion
    }

    public class ServiceDeskWSClient
    {
        #region Private Data Members

        SDWS.vsaServiceDeskWS clientWS;
        KaseyaWSClient authClient;
        #endregion

        #region Public Data Members

        public string SessionID;

        #endregion

        #region Constructors

        public ServiceDeskWSClient(string ServicedeskURL, string AuthURL)
        {
            clientWS = new SDWS.vsaServiceDeskWS();
            clientWS.Url = ServicedeskURL;
            authClient = new KaseyaWSClient(AuthURL);

        }

        #endregion

        #region Public Methods

        public SDWS.AddIncidentResponse ProcessRequest(SDWS.AddIncidentRequest req)
        {
            req.SessionID = authClient.SessionID;
            return clientWS.AddIncident(req);
        }

        public SDWS.AddServDeskToScopeResponse ProcessRequest(SDWS.AddServDeskToScopeRequest req)
        {
            req.SessionID = authClient.SessionID;
            return clientWS.AddServDeskToScope(req);
        }

        public SDWS.GetIncidentListResponse ProcessRequest(SDWS.GetIncidentListRequest req)
        {
            SDWS.GetIncidentListResponse result = new SDWS.GetIncidentListResponse();
            try
            {
                req.SessionID = authClient.SessionID;
                result = clientWS.GetIncidentList(req);
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                result.ErrorLocation = ex.StackTrace;
            }

            return result;

        }

        public SDWS.GetIncidentList2Response ProcessRequest(SDWS.GetIncidentList2Request req)
        {
            req.SessionID = authClient.SessionID;
            return clientWS.GetIncidentList2(req);
        }

        public SDWS.GetIncidentResponse ProcessRequest(SDWS.GetIncidentReq req)
        {
            req.SessionID = authClient.SessionID;
            return clientWS.GetIncident(req);
        }

        public SDWS.GetServiceDeskResponse ProcessRequest(SDWS.GetServiceDeskRequest req)
        {
            req.SessionID = authClient.SessionID;
            return clientWS.GetServiceDesk(req);
        }

        public SDWS.GetServiceDesksResponse ProcessRequest(SDWS.GetServiceDesksRequest req)
        {
            req.SessionID = authClient.SessionID;
            return clientWS.GetServiceDesks(req);
        }

        public SDWS.UpdateIncidentResponse ProcessRequest(SDWS.UpdateIncidentRequest req)
        {
            req.SessionID = authClient.SessionID;
            return clientWS.UpdateIncident(req);
        }

        #endregion

        #region Private Methods





        #endregion
    }

    public class hash
    {

        public string HashToCoveredPassword(string UserName, ref string Password, string HashingAlgorithm)
        {
            string Rand = GenerateRandomNumber(8);

            Password = HashValues(Password, UserName, HashingAlgorithm);
            Password = HashValues(Password, Rand, HashingAlgorithm);

            return Rand;
        }

        private string HashValues(string Value1, string Value2, string HashingAlgorithm)
        {
            string sHashingAlgorithm = "";
            if (String.IsNullOrEmpty(HashingAlgorithm))
                sHashingAlgorithm = "SHA-1";
            else
                sHashingAlgorithm = HashingAlgorithm;

            byte[] arrByte;

            if (sHashingAlgorithm == "SHA-1")
            {
                SHA1Managed hash = new SHA1Managed();
                arrByte = hash.ComputeHash(ASCIIEncoding.ASCII.GetBytes(Value1 + Value2));
            }
            else
            {
                SHA256Managed hash = new SHA256Managed();
                arrByte = hash.ComputeHash(ASCIIEncoding.ASCII.GetBytes(Value1 + Value2));
            }

            string s = "";
            for (int i = 0; i < arrByte.Length; i++)
            {
                s += arrByte[i].ToString("x2");
            }
            return s;
        }

        private string GenerateRandomNumber(int NumberOfDigits)
        {
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();

            byte[] numbers = new byte[NumberOfDigits * 2];
            rng.GetNonZeroBytes(numbers);

            string result = "";
            for (int i = 0; i < NumberOfDigits; i++)
            {
                result += numbers[i].ToString();
            }

            result = result.Replace("0", "");
            return result.Substring(1, NumberOfDigits);

        }
    }
}