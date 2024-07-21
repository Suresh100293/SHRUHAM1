using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class LCSetupVM
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
        public int Class_Srl { get; set; }

        public bool YearwiseManualSrlReq { get; set; }
        public bool CetralisedManualSrlReq { get; set; }
        #endregion

        public List<SelectListItem> PrintFormats { get; set; }
        public string PrintFormat { get; set; }
        public string PrintFormatsL { get; set; }


        public string FooterDetails1 { get; set; }
        public string FooterDetails2 { get; set; }
        public string FooterDetails3 { get; set; }
        public string FooterDetails4 { get; set; }

        
        public int Days { get; set; }

        public bool LcAutomatic { get; set; }
        public bool LcManual { get; set; }
        public bool Both { get; set; }

        public bool FillFromCurr { get; set; }


        public bool CheckLrDate { get; set; }

        

        public bool EditReq { get; set; }
        public bool DeleteReq { get; set; }
        public int EditHours { get; set; }
        public int DeleteHours { get; set; }


        public bool CheckManualLC { get; set; }

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
    public enum AscDescLC
    {
        DocumentDate, LCNo
    }
}