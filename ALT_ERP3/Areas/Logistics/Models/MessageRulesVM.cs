using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class MessageRulesVM
    {
        // iX9: Field Structure of MessageRules
        public int MessageRules_RECORDKEY { get; set; }
        public string MessageRules_Code { get; set; }
        public string MessageRules_Type { get; set; }
        public string ViewDataId { get; set; }
        public string OptionCode { get; set; }



        //Sms Template
        public string SMSBranch { get; set; }
        public List<SelectListItem> SMSBranchs { get; set; }
        public string SMSBranchL { get; set; }

        public bool MessageRules_SendSMS { get; set; }

        public string EmailTemplate { get; set; }
        public string SMSTemplate { get; set; }
        public List<SelectListItem> SMSTemplates { get; set; }



        //Email 
        public string EmailBranch { get; set; }
        public List<SelectListItem> EmailBranchs { get; set; }
        public string EmailBranchL { get; set; }

        public bool EmailMessageRules_xAdd { get; set; }
        public bool EmailMessageRules_xEdit { get; set; }
        public bool EmailMessageRules_xDelete { get; set; }
        public bool EmailMessageRules_xPrint { get; set; }
        public bool EmailMessageRules_xBackDated { get; set; }

        //MSG 
        public string MSGBranch { get; set; }
        public List<SelectListItem> MSGBranchs { get; set; }
        public string MSGBranchL { get; set; }

        public bool MSGMessageRules_xAdd { get; set; }
        public bool MSGMessageRules_xEdit { get; set; }
        public bool MSGMessageRules_xDelete { get; set; }
        public bool MSGMessageRules_xPrint { get; set; }
        public bool MSGMessageRules_xBackDated { get; set; }


        
        

        


        //public string MessageRules_Code { get; set; }
        //public string MessageRules_CompCode { get; set; }
        //public int MessageRules_LocationCode { get; set; }
        //public int MessageRules_Sno { get; set; }
        //public string MessageRules_Type { get; set; }

        //public bool MessageRules_xAdd { get; set; }
        //public bool MessageRules_xEdit { get; set; }
        //public bool MessageRules_xDelete { get; set; }
        //public bool MessageRules_xPrint { get; set; }
        //public bool MessageRules_xBackDated { get; set; }

        
        //public bool MessageRules_SendEmail { get; set; }
        //public bool MessageRules_SendMSG { get; set; }

        public decimal MessageRules_LimitAmount { get; set; }

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
        public string SearchBy { get; set; }
        public string SearchContent { get; set; }
    }
}