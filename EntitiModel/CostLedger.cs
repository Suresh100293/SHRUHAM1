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
    
    public partial class CostLedger
    {
        public int RECORDKEY { get; set; }
        public string Branch { get; set; }
        public string Code { get; set; }
        public string CompCode { get; set; }
        public int CostCode { get; set; }
        public Nullable<int> CostGrp { get; set; }
        public Nullable<decimal> Credit { get; set; }
        public Nullable<decimal> Debit { get; set; }
        public Nullable<decimal> DocAmount { get; set; }
        public Nullable<System.DateTime> DocDate { get; set; }
        public int LocationCode { get; set; }
        public string MainType { get; set; }
        public string ParentKey { get; set; }
        public string Prefix { get; set; }
        public int Sno { get; set; }
        public string Srl { get; set; }
        public int SrNo { get; set; }
        public string SubType { get; set; }
        public string TableKey { get; set; }
        public string Type { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
    }
}
