using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Vehicles.Models
{
    public class TripSheetSetupVM
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


        public string DebitAc { get; set; }
        public string DebitAcName { get; set; }
        public string FetchFmFrom { get; set; }
        public bool ChangeAc { get; set; }
        public bool TDSDeduction { get; set; }
        public bool ChangeCharge { get; set; }
        public bool NoDocumentAllow { get; set; }
        public bool SplitPosting { get; set; }
        public bool AllowDateFilter { get; set; }

        public bool DriverTripDefault { get; set; }
        public bool TripFMDefaultDoc { get; set; }
        public bool Pick_Financial_Document { get; set; }
        public bool ShowSummary { get; set; }

        public bool ShowConsignmentExp { get; set; }
        public bool RestrictLrDateExp { get; set; }
        public string RestrictLrExpDays { get; set; }


        public bool FitnessExp { get; set; }
        public bool InsuranceExp { get; set; }
        public bool PUCExp { get; set; }
        public bool AIPExp { get; set; }
        public bool StateTaxExp { get; set; }
        public bool TPStateExp { get; set; }
        public bool GreenTaxExp { get; set; }
        public bool DriverExp { get; set; }

        public bool ConfirmDupDateOfTrip { get; set; }
        public bool RestrictDupDateOfTrip { get; set; }



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