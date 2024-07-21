using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class TransactionSMSModel
    {
        public int RECORDKEY { get; set; }
        //public string Branch { get; set; }
        //public string Code { get; set; }
        public string CompCode { get; set; }
        public int LocationCode { get; set; }
        //public int Sno { get; set; }
        public string Type { get; set; }
        public string ViewDataId { get; set; }
        public decimal LimitAmount { get; set; }
        public string OptionCode { get; set; }


        //Using Party And User Template
        public string SMSTemplate { get; set; }
        public List<SelectListItem> SMSTemplates { get; set; }

        //Using Party And User Template
        public string PartyPrintFormat { get; set; }
        public string PartyPrintFormatL { get; set; }
        public string UserPrintFormat { get; set; }
        public string UserPrintFormatL { get; set; }
        public List<SelectListItem> PrintFormats { get; set; }




        //Party Parameters
        public string PartyId { get; set; }
        public string PartyName { get; set; }
        public bool PartySendEmail { get; set; }
        public bool PartySendSMS { get; set; }

        public string PartyBranch { get; set; }
        public string PartyBranchL { get; set; }
        public List<SelectListItem> PartyBranchs { get; set; }


        //Users Parameters
        public string Users { get; set; }
        public string UsersL { get; set; }
        public List<SelectListItem> Userss { get; set; }

        public string UserBranch { get; set; }
        public string UserBranchL { get; set; }
        public List<SelectListItem> UserBranchs { get; set; }

        public bool UserSendEmail { get; set; }
        public bool UserSendSMS { get; set; }
        public bool UserSendMSG { get; set; }



        //public bool SendEmail { get; set; }
        // public bool SendSMS { get; set; }
        //public bool SendMSG { get; set; }
        //public bool SendBroker { get; set; }
        //public bool SendSalesMan { get; set; }
        // public bool SendUser { get; set; }
        //public bool SendCheckBlank { get; set; }

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

        public List<TransactionSMSModel> mLeftList { get; set; }
        public List<TransactionSMSModel> mRightList { get; set; }
        public string SearchBy { get; set; }
        public string SearchContent { get; set; }

        
        //public string Users { get; set; }


        //public string EmailTemplate { get; set; }

        //public string AllUsers { get; set; }



        
        

    }
}