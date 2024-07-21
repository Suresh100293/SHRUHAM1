using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class MessageRulesVM
    {
        // iX9: Field Structure of MessageRules
        public int MessageRules_RECORDKEY { get; set; }
        public string MessageRules_Branch { get; set; }
        public string MessageRules_Code { get; set; }
        public string MessageRules_CompCode { get; set; }
        public int MessageRules_LocationCode { get; set; }
        public int MessageRules_Sno { get; set; }
        public string MessageRules_Type { get; set; }
        public bool MessageRules_xAdd { get; set; }
        public bool MessageRules_xBackDated { get; set; }
        public bool MessageRules_xDelete { get; set; }
        public bool MessageRules_xEdit { get; set; }
        public bool MessageRules_xPrint { get; set; }
        public decimal MessageRules_LimitAmount { get; set; }
        public bool MessageRules_SendEmail { get; set; }
        public bool MessageRules_SendSMS { get; set; }
        public bool MessageRules_SendMSG { get; set; }

        // iX9: Common default Fields
        public int Document { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Mode { get; set; }

        // iX9: special lists used for Grid2Grid relation type of interface
        public string DocTypes_Code { get; set; }
        public string DocTypes_Name { get; set; }
        public string DocTypes_MainType { get; set; }
        public string DocTypes_SubType { get; set; }

        public List<MessageRulesVM> mLeftList { get; set; }
        public List<MessageRulesVM> mRightList { get; set; }
    }
}