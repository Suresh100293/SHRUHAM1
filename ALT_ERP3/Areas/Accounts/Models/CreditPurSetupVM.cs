using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class CreditPurSetupVM
    {

        #region Classification Setup
        public bool Class_CurrDatetOnlyreq { get; set; }
        public bool BackDated { get; set; }
        public decimal BackDays { get; set; }
        public bool ForwardDated { get; set; }
        public decimal ForwardDays { get; set; }
        public bool Class_BranchwiseSrlReq { get; set; }
        public bool Class_YearwiseSrlReq { get; set; }
        public bool Class_CetralisedSrlReq { get; set; }
        public string Class_Srl { get; set; }
        public string Class_Width { get; set; }
        #endregion

        public bool ShowConsignmentExp { get; set; }
        public bool RestrictLrDateExp { get; set; }
        public string RestrictLrExpDays { get; set; }

        public bool Automatic { get; set; }
        public bool Manual { get; set; }
        public bool Both { get; set; }
        public bool CutTDS { get; set; }
        public bool SplitPosting { get; set; }
        public bool ShowLedgerPost { get; set; }
        public bool RelatedACReq { get; set; }
        public bool TyreStock { get; set; }


        public string OTGenerate { get; set; }
        public bool CCAmtMatch { get; set; }

        
        public bool DuplExpLRFMConfirm { get; set; }

        public bool NoDuplExpLRFM { get; set; }

        
        public bool ShowDocSerial { get; set; }
        public bool AllowZeroAmt { get; set; }

        public bool AllowAutoRemark { get; set; }

        public bool NoDuplExpDt { get; set; }
        public bool DuplExpDtConfirm { get; set; }
        public bool ActiveBranchTransfer { get; set; }

        public string ExpDtType { get; set; }



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