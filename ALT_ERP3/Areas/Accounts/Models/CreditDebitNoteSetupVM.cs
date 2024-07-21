using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class CreditDebitNoteSetupVM
    {
        public List<SelectListItem> PrintFormats { get; set; }
        public string PrintFormat { get; set; }
        public string PrintFormatsL { get; set; }

        public string FooterDetails1 { get; set; }
        public string FooterDetails2 { get; set; }
        public string FooterDetails3 { get; set; }
        public string FooterDetails4 { get; set; }
        public string FooterDetails5 { get; set; }
        public string FooterDetails6 { get; set; }
        public string FooterDetails7 { get; set; }
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