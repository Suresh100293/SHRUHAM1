//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EntitiModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class CreditPurchaseSetup
    {
        public int RECORDKEY { get; set; }
        public bool BillBoth { get; set; }
        public bool BillAuto { get; set; }
        public bool ShowLedgerPost { get; set; }
        public bool CutTDS { get; set; }
        public decimal BackDays { get; set; }
        public bool BackDated { get; set; }
        public bool DuplExpLRFMConfirm { get; set; }
        public bool NoDuplExpLRFM { get; set; }
        public bool ForwardDated { get; set; }
        public decimal ForwardDays { get; set; }
        public bool AllowZeroAmt { get; set; }
        public bool ShowDocSerial { get; set; }
        public bool AutoRemark { get; set; }
        public bool NoDuplExpDt { get; set; }
        public bool DuplExpDtConfirm { get; set; }
        public bool TyreStockDelete { get; set; }
        public string AUTHIDS { get; set; }
        public string AUTHORISE { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public bool SplitPosting { get; set; }
        public bool RestrictLrDateExp { get; set; }
        public string RestrictLrExpDays { get; set; }
        public bool CurrDatetOnlyreq { get; set; }
        public bool BranchwiseSrlReq { get; set; }
        public bool YearwiseSrlReq { get; set; }
        public bool CetralisedSrlReq { get; set; }
        public string Srl { get; set; }
        public string Width { get; set; }
        public bool ShowConsignmentExp { get; set; }
    }
}
