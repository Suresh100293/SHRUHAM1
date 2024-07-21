using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class NotificationVM
    {
        public string Srl { get; set; }
        public System.DateTime CreateOn { get; set; }
        public bool Clear { get; set; }
        public bool Priority { get; set; }
        public string DocType { get; set; }
        public string DocNo { get; set; }
        public string Parentkey { get; set; }
        public string Tablekey { get; set; }
        public string Describe { get; set; }
        public string Msg { get; set; }
        public string HtmlString { get; set; }
        public string ToUser { get; set; }
        public string ToUserName { get; set; }


        // iX9: Common default Fields
        public int Document { get; set; }
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
        public bool CloseTab { get; set; }

    }
}