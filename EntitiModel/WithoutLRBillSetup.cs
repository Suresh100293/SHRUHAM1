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
    
    public partial class WithoutLRBillSetup
    {
        public int RECORDKEY { get; set; }
        public bool BillBoth { get; set; }
        public bool BillAuto { get; set; }
        public bool ShowLedgerPost { get; set; }
        public string AUTHIDS { get; set; }
        public string AUTHORISE { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public bool CurrDatetOnlyreq { get; set; }
        public bool BackDateAllow { get; set; }
        public int BackDaysUpto { get; set; }
        public bool ForwardDateAllow { get; set; }
        public int ForwardDaysUpto { get; set; }
        public bool BranchwiseSrlReq { get; set; }
        public bool YearwiseSrlReq { get; set; }
        public bool CetralisedSrlReq { get; set; }
        public string Srl { get; set; }
        public bool CutTDS { get; set; }
        public string DefaultPrint { get; set; }
        public string Width { get; set; }
        public bool follow_GST_HSN_Ledgerwise { get; set; }
        public bool MergeSerial { get; set; }
    }
}
