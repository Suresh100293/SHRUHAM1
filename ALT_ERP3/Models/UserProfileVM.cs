using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Models
{
    public class UserProfileVM
    {
        // iX9: Field Structure of TfatPass
        public List<SelectListItem> UserList { get; set; }
        public string ChildUsers { get; set; }
        public string Users { get; set; }



        public int TfatPass_RECORDKEY { get; set; }
        public string TfatPass_AppBranch { get; set; }
        public string TfatPass_Assistant { get; set; }
        public string TfatPass_BCCTo { get; set; }
        public string TfatPass_CCTo { get; set; }
        public string TfatPass_Code { get; set; }
        public int TfatPass_ColScheme { get; set; }
        public string TfatPass_CorpID { get; set; }
        public bool TfatPass_DailyPassword { get; set; }
        public bool TfatPass_DashboardActive { get; set; }
        public int TfatPass_Dept { get; set; }
        public bool TfatPass_DontChangePassword { get; set; }
        public string TfatPass_Email { get; set; }
        public string TfatPass_EmailClient { get; set; }
        public System.DateTime TfatPass_Expiry { get; set; }
        public string TfatPass_ExpiryVM { get; set; }
        public bool TfatPass_Fri { get; set; }
        public int TfatPass_GridRows { get; set; }
        public bool TfatPass_Hide { get; set; }
        public bool TfatPass_IsHead { get; set; }
        public int TfatPass_Language { get; set; }
        public string TfatPass_LastBranch { get; set; }
        public string TfatPass_LastCompany { get; set; }
        public System.DateTime TfatPass_LastLogin { get; set; }
        public string TfatPass_LastLoginVM { get; set; }
        public System.DateTime TfatPass_LastStartDt { get; set; }
        public string TfatPass_LastStartDtVM { get; set; }
        public string TfatPass_LicenseID { get; set; }
        public bool TfatPass_Locked { get; set; }
        public string TfatPass_LogIns { get; set; }
        public int TfatPass_MinAcc { get; set; }
        public int TfatPass_MinItem { get; set; }
        public int TfatPass_MinOthers { get; set; }
        public string TfatPass_Mobile { get; set; }
        public bool TfatPass_Mon { get; set; }
        public int TfatPass_MsgDays { get; set; }
        public string TfatPass_Name { get; set; }
        public string TfatPass_PassRead { get; set; }
        public string TfatPass_PassWords { get; set; }
        public string TfatPass_Photograph { get; set; }
        public string TfatPass_POPServer { get; set; }
        public bool TfatPass_PrintControl { get; set; }
        public bool TfatPass_RecMsg { get; set; }
        public bool TfatPass_RecSMS { get; set; }
        public bool TfatPass_Remember { get; set; }
        public bool TfatPass_Sat { get; set; }
        public string TfatPass_Signature { get; set; }
        public string TfatPass_SMTPPassword { get; set; }
        public int TfatPass_SMTPPort { get; set; }
        public bool TfatPass_SMTPRoute { get; set; }
        public string TfatPass_SMTPServer { get; set; }
        public string TfatPass_SMTPUser { get; set; }
        public string TfatPass_StrictIP { get; set; }
        public bool TfatPass_Sun { get; set; }
        public string TfatPass_Telephone { get; set; }
        public bool TfatPass_Thu { get; set; }
        public bool TfatPass_Tue { get; set; }
        public string TfatPass_UserRole { get; set; }
        public bool TfatPass_Wed { get; set; }
        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public List<SelectListItem> GridRowsList { get; set; }
        public string UserRoleName { get; set; }
        public string DeptName { get; set; }
        public string LanguageName { get; set; }
        public string AssistantName { get; set; }
        public List<SelectListItem> AppBranchMultiX { get; set; }
        public string AppBranchItemX { get; set; }

        // iX9: Common default Fields
        public string Document { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Mode { get; set; }

        public string DefaultLoginBranch { get; set; }
        public string DefaultLoginBranchN { get; set; }

        // iX9: used while passing parameters from MasterGrid to Option and Back 
        public string MainType { get; set; }
        public string Controller2 { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }

        public bool ConsignmenyQueryBranch { get; set; }
        public bool FreightMemoQueryBranch { get; set; }
        public List<ConsignmentQueryPanel> ConsignmentPanels { get; set; }
        public List<FreightMemoQueryPanel> FreightMemoPanels { get; set; }
    }

    public class ConsignmentQueryPanel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Check { get; set; }
    }
    public class FreightMemoQueryPanel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Check { get; set; }
    }
}