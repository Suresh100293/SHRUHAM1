using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class OTransSetup
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

        public string OTGenerate { get; set; }
        public bool BillBoth { get; set; }
        public bool BillManual { get; set; }
        public bool BillAuto { get; set; }

        
        public bool DuplExpLRFMConfirm { get; set; }

        public bool NoDuplExpLRFM { get; set; }
        public bool ShowLedgerPost { get; set; }
        public bool TyreStock { get; set; }

        public string Controller2 { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
        public bool CutTDS { get; set; }

        public string Mode { get; set; }

        public string OptionType { get; set; }

        
        public bool ShowDocSerial { get; set; }
        public bool AllowZeroAmt { get; set; }

        public bool AllowAutoRemark { get; set; }

        public bool NoDuplExpDt { get; set; }
        public bool DuplExpDtConfirm { get; set; }

        public string ExpDtType { get; set; }
    }
}