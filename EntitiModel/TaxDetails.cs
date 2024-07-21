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
    
    public partial class TaxDetails
    {
        public int RECORDKEY { get; set; }
        public string Branch { get; set; }
        public string Code { get; set; }
        public bool CutTCS { get; set; }
        public bool CutTDS { get; set; }
        public string Deductee { get; set; }
        public Nullable<decimal> DifferRate { get; set; }
        public string DifferRateCertNo { get; set; }
        public bool FBT { get; set; }
        public string FBTCode { get; set; }
        public Nullable<decimal> FBTRate { get; set; }
        public Nullable<decimal> FBTSlab { get; set; }
        public Nullable<System.DateTime> Form15HCITDate { get; set; }
        public Nullable<System.DateTime> Form15HDate { get; set; }
        public bool IsDifferRate { get; set; }
        public bool IsForm15H { get; set; }
        public Nullable<int> LocationCode { get; set; }
        public bool ServiceTax { get; set; }
        public string ServiceTaxCode { get; set; }
        public Nullable<int> TDSCode { get; set; }
        public Nullable<decimal> TDSRate { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
    }
}
