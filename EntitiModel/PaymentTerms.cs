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
    
    public partial class PaymentTerms
    {
        public int RECORDKEY { get; set; }
        public int Code { get; set; }
        public Nullable<int> CrPeriod { get; set; }
        public Nullable<decimal> EarlyDay1 { get; set; }
        public Nullable<decimal> EarlyDay2 { get; set; }
        public Nullable<decimal> EarlyDay3 { get; set; }
        public Nullable<decimal> EarlyDay4 { get; set; }
        public Nullable<decimal> EarlyDay5 { get; set; }
        public Nullable<decimal> EarlyPerc1 { get; set; }
        public Nullable<decimal> EarlyPerc2 { get; set; }
        public Nullable<decimal> EarlyPerc3 { get; set; }
        public Nullable<decimal> EarlyPerc4 { get; set; }
        public Nullable<decimal> EarlyPerc5 { get; set; }
        public Nullable<decimal> LateDay1 { get; set; }
        public Nullable<decimal> LateDay2 { get; set; }
        public Nullable<decimal> LateDay3 { get; set; }
        public Nullable<decimal> LateDay4 { get; set; }
        public Nullable<decimal> LateDay5 { get; set; }
        public Nullable<decimal> LatePerc1 { get; set; }
        public Nullable<decimal> LatePerc2 { get; set; }
        public Nullable<decimal> LatePerc3 { get; set; }
        public Nullable<decimal> LatePerc4 { get; set; }
        public Nullable<decimal> LatePerc5 { get; set; }
        public string Name { get; set; }
        public string Narr { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
    }
}
