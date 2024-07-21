using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class AltNotificationVM
    {

        public List<SelectListItem> Users { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }

        public List<SelectListItem> Branches { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }

        public string Type { get; set; }

        //Grid Purpose Property
        public long RECORDKEY { get; set; }
        public string SubType { get; set; }
        public string SubTypeText { get; set; }

        public List<SelectListItem> Dynamiclist { get; set; }
        public bool Param1List { get; set; }
        public string Param1Type { get; set; }
        public string Param1 { get; set; }
        public string Param1L { get; set; }
        public bool Visible_Param2 { get; set; }
        public decimal Param2 { get; set; }
        public string Param2L { get; set; }
        public bool NoParameter { get; set; }

        public string CreateOn { get; set; }
        public int SrNo { get; set; }
        public bool Notification { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool Self { get; set; }

        public List<AltNotificationVM> list { get; set; }

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