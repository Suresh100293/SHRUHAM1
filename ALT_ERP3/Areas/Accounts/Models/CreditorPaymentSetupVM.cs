using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class CreditorPaymentSetupVM
    {
        #region Classification Setup

        public bool Class_CurrDatetOnlyreq { get; set; }
        public bool Class_BackDateAllow { get; set; }
        public int Class_BackDaysUpto { get; set; }
        public bool Class_ForwardDateAllow { get; set; }
        public int Class_ForwardDaysUpto { get; set; }
        public bool Class_BranchwiseSrlReq { get; set; }
        public bool Class_YearwiseSrlReq { get; set; }
        public bool Class_CetralisedSrlReq { get; set; }
        public string Class_Srl { get; set; }
        public string Class_Width { get; set; }
        #endregion

        public List<SelectListItem> PrintFormats { get; set; }
        public string PrintFormat { get; set; }
        public string PrintFormatsL { get; set; }

        public bool BillBoth { get; set; }
        public bool BillManual { get; set; }
        public bool BillAuto { get; set; }

        public bool CutTDS { get; set; }
        public bool CutBillwiseTDS { get; set; }
        public int Roundoff { get; set; }

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