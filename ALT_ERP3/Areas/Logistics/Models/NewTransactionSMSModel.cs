using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class NewTransactionSMSModel
    {
        public int RECORDKEY { get; set; }
        public string Branch { get; set; }
        public string Code { get; set; }
        public string CompCode { get; set; }
        public int LocationCode { get; set; }
        public int Sno { get; set; }
        public string Type { get; set; }

        public decimal LimitAmount { get; set; }
        public bool SendEmail { get; set; }
        public bool SendSMS { get; set; }
        public bool SendMSG { get; set; }
        public bool SendBroker { get; set; }
        public bool SendSalesMan { get; set; }
        public bool SendUser { get; set; }
        public bool SendCheckBlank { get; set; }

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

        public List<NewTransactionSMSModel> mLeftList { get; set; }
        public List<NewTransactionSMSModel> mRightList { get; set; }


        public List<MessageRulesVM> mRightUList { get; set; }




        public string SearchBy { get; set; }
        public string SearchContent { get; set; }

        public string PartyId { get; set; }

        public string PartyName { get; set; }
        public string Users { get; set; }

        public string SMSTemplate { get; set; }

        public string EmailTemplate { get; set; }

        public string AllUsers { get; set; }
        public List<SelectListItem> UserList { get; set; }
    }
}