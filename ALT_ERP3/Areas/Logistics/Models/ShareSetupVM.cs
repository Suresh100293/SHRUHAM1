using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class ShareSetupVM
    {
        public bool LRExtra { get; set; }
        public bool LRAttachReq { get; set; }
        public string LRFormat { get; set; }
        public string LRFormatL { get; set; }
        public List<SelectListItem> LRPrintFormats { get; set; }

        public bool FmExtra { get; set; }
        public bool FMAttachReq { get; set; }
        public string FMFormat { get; set; }
        public string FMFormatL { get; set; }
        public List<SelectListItem> FMPrintFormats { get; set; }

        public bool BillExtra { get; set; }
        public bool BillAttachReq { get; set; }
        public string BillFormat { get; set; }
        public string BillFormatL { get; set; }
        public List<SelectListItem> BillPrintFormats { get; set; }

        public bool Payment { get; set; }
        public bool PaymentAttachReq { get; set; }
        public string CCEmail { get; set; }
        public string PaymentFormat { get; set; }
        public string PaymentFormatL { get; set; }
        public List<SelectListItem> PaymentPrintFormats { get; set; }

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
        public string Controller { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
        public string Code { get; set; }
    }
}