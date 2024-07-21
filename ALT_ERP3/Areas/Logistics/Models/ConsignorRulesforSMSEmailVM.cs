using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class ConsignorRulesforSMSEmailVM
    {
        public int tEmpID { get; set; }

        public string Type { get; set; }
        public string TypeName { get; set; }

        public string Code { get; set; }
        public string CodeName { get; set; }

        public string CodeL { get; set; }
        public List<SelectListItem> Consignors { get; set; }

        public string AppBranchL { get; set; }
        public List<SelectListItem> Branches { get; set; }
        public string AppBranch { get; set; }

        public bool SendEmail { get; set; }
        public string EmailFormatL { get; set; }
        public List<SelectListItem> EmailFormats { get; set; }
        public string EmailFormat { get; set; }

        public bool SendSMS { get; set; }
        public string SMSTemplate { get; set; }
        public string SMSTemplateName { get; set; }
        public List<SelectListItem> SMSTemplates { get; set; }

        public IList<ConsignorRulesforSMSEmailVM> GridDataVM { get; set; }
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