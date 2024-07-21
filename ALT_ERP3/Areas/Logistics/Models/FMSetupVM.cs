using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class FMSetupVM
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

        public bool FMAutomatic { get; set; }
        public bool FMManual { get; set; }
        public bool Both { get; set; }

        public bool EditReq { get; set; }
        public bool DeleteReq { get; set; }
        public int EditHours { get; set; }
        public int DeleteHours { get; set; }

        public bool FillFromCurr { get; set; }


        public string[] FM_Vehicle_Category_Name { get; set; }
        public string[] FM_Vehicle_Category_Code { get; set; }
        public bool[] FM_Vehicle_Category_Status_Maintain_Flag { get; set; }
        public bool FmGetVehicleBranchWise { get; set; }
        public bool FmGetVehicleReadyStatus { get; set; }
        public string Selected_FM_Vehicle_Category_Status_Maintain { get; set; }

        public bool KM { get; set; }
        public bool Payload { get; set; }
        public bool LicenceNo { get; set; }
        public bool LicenceExpDate { get; set; }
        public bool DriverName { get; set; }
        public bool ContactNo { get; set; }
        //public string PayableAt { get; set; }
        //public string PayableAt_Name { get; set; }

        public string FMType { get; set; }
        public string FMType_Name { get; set; }

        public bool CheckManualFM { get; set; }
        public bool ShowLedgerPost { get; set; }

        public bool AllowToChgTDS { get; set; }
        public bool GenerateSchedule { get; set; }
        public string FooterDetails1 { get; set; }
        public string FooterDetails2 { get; set; }
        public string FooterDetails3 { get; set; }
        public string FooterDetails4 { get; set; }

        public bool ExcludeHire { get; set; }

        //public bool OwnPostingReq { get; set; }
        //public string OwnDebitAcName { get; set; }
        //public string OwnDebitAcCode { get; set; }
        //public string OwnCreditAccount { get; set; }
        //public bool OwnMaintainCreditorRecord { get; set; }

        //public bool AttachPostingReq { get; set; }
        //public string AttachDebitAcName { get; set; }
        //public string AttachDebitAcCode { get; set; }
        //public string AttachCreditAccount { get; set; }
        //public bool AttachMaintainCreditorRecord { get; set; }


        //public bool HirePostingReq { get; set; }
        public string HireDebitAcName { get; set; }
        public string HireDebitAcCode { get; set; }
        //public string HireCreditAccount { get; set; }

        public bool ChangeBroker { get; set; }
        public bool ChangeDriver { get; set; }


        //Darshan
        public bool AttachTDSCut { get; set; }
        public bool HireTDSCut { get; set; }
        public bool OwnTDSCut { get; set; }
        //Darshan


        public bool FitnessExp { get; set; }
        public bool InsuranceExp { get; set; }
        public bool PUCExp { get; set; }
        public bool AIPExp { get; set; }
        public bool StateTaxExp { get; set; }
        public bool TPStateExp { get; set; }
        public bool GreenTaxExp { get; set; }
        public bool DriverExp { get; set; }

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