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
    
    public partial class MultiRates
    {
        public int RECORDKEY { get; set; }
        public bool AllowUser { get; set; }
        public string Branch { get; set; }
        public bool CanMfg { get; set; }
        public bool CanPurch { get; set; }
        public bool CanSale { get; set; }
        public string Code { get; set; }
        public string CompCode { get; set; }
        public System.DateTime EffectiveDate { get; set; }
        public Nullable<decimal> ListPrice { get; set; }
        public int LocationCode { get; set; }
        public Nullable<double> MRP { get; set; }
        public Nullable<decimal> PurchDisc { get; set; }
        public Nullable<decimal> PurchDiscAmt { get; set; }
        public Nullable<decimal> PurchPer { get; set; }
        public Nullable<double> PurchRate { get; set; }
        public string PurchTaxCode { get; set; }
        public Nullable<decimal> SalesDisc { get; set; }
        public Nullable<decimal> SalesDiscAmt { get; set; }
        public decimal SalesPer { get; set; }
        public Nullable<double> SalesRate { get; set; }
        public string SalesTaxCode { get; set; }
        public string xValue1 { get; set; }
        public string xValue2 { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
    }
}
