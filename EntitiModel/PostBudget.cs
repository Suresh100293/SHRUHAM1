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
    
    public partial class PostBudget
    {
        public int RECORDKEY { get; set; }
        public Nullable<double> AnnualHrs { get; set; }
        public string Branch { get; set; }
        public Nullable<decimal> BudAmt { get; set; }
        public string CompCode { get; set; }
        public Nullable<int> Dept { get; set; }
        public string Descr { get; set; }
        public System.DateTime FromDate { get; set; }
        public Nullable<double> FTE { get; set; }
        public Nullable<int> HeadCount { get; set; }
        public int LocationCode { get; set; }
        public int PostCode { get; set; }
        public int Srl { get; set; }
        public System.DateTime ToDt { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
    }
}
