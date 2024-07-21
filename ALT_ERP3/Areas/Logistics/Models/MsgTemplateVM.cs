using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class MsgTemplateVM
    {
        // iX9: Field Structure of MsgTemplate
        public int MsgTemplate_RECORDKEY { get; set; }
        public string MsgTemplate_AttachPath { get; set; }
        public string MsgTemplate_Code { get; set; }
        public bool MsgTemplate_IsHTML { get; set; }
        public string MsgTemplate_MsgHTML { get; set; }
        public string MsgTemplate_MsgRTF { get; set; }
        public string MsgTemplate_MsgText { get; set; }
        public string MsgTemplate_Subject { get; set; }

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