using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class TransactionAuthorisationRulesVM
    {
        // iX9: Field Structure of TfatUserAuditHeader
        public int TfatUserAuditHeader_RECORDKEY { get; set; }
        public bool TfatUserAuditHeader_AuthAgain { get; set; }
        public bool TfatUserAuditHeader_AuthCond { get; set; }
        public bool TfatUserAuditHeader_AuthEach { get; set; }
        public bool TfatUserAuditHeader_AuthLock { get; set; }
        public bool TfatUserAuditHeader_AuthLockDelete { get; set; }
        public bool TfatUserAuditHeader_AuthNewSerial { get; set; }
        public string TfatUserAuditHeader_AuthNewType { get; set; }
        public bool TfatUserAuditHeader_AuthNoPrint { get; set; }
        public bool TfatUserAuditHeader_AuthReq { get; set; }
        public int TfatUserAuditHeader_AuthRule { get; set; }
        public bool TfatUserAuditHeader_AuthSame { get; set; }
        public bool TfatUserAuditHeader_AuthTimeBound { get; set; }
        public int TfatUserAuditHeader_AuthTimeLimit { get; set; }
        public string TfatUserAuditHeader_Type { get; set; }

        public bool TfatUserAuditHeader_OptAuthLock { get; set; }
        public bool TfatUserAuditHeader_OptAuthLockDelete { get; set; }


        // iX9: Field Structure of TfatUserAudit
        public int TfatUserAudit_RECORDKEY { get; set; }
        public decimal TfatUserAudit_SancLimit { get; set; }
        public bool TfatUserAudit_SendEmail { get; set; }
        public bool TfatUserAudit_SendMSG { get; set; }
        public bool TfatUserAudit_SendSMS { get; set; }
        public int TfatUserAudit_Sno { get; set; }
        public string TfatUserAudit_Type { get; set; }
        public string TfatUserAudit_UserID { get; set; }
        public int TfatUserAudit_UserLevel { get; set; }
        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public string TypeName { get; set; }
        public string UserIDName { get; set; }
        public string AuthNewTypeName { get; set; }

        public string AppBranchL { get; set; }
        public List<SelectListItem> Branches { get; set; }
        public string AppBranch { get; set; }

        public string AuthenticateBranchList { get; set; }
        public string AuthenticateReqBranchList { get; set; }
        public List<SelectListItem> ReqBranches { get; set; }
        // iX9: Fields for GridView
        public bool tempIsDeleted { get; set; }
        public int tEmpID { get; set; }
        public IList<TransactionAuthorisationRulesVM> GridDataVM { get; set; }

        // iX9: Common default Fields
        public string Document { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Mode { get; set; }

        // iX9: used while passing parameters from MasterGrid to Option and Back 
        public string MainType { get; set; }
        public string Controller2 { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
    }
}