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
    
    public partial class EmployeeLeave
    {
        public int RECORDKEY { get; set; }
        public Nullable<int> Balance { get; set; }
        public string Branch { get; set; }
        public string EmpID { get; set; }
        public Nullable<System.DateTime> FromDate { get; set; }
        public string Leave { get; set; }
        public Nullable<decimal> Rate { get; set; }
        public Nullable<int> Remaining { get; set; }
        public string SalType { get; set; }
        public Nullable<System.DateTime> ToDate { get; set; }
        public Nullable<int> Total { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
    }
}
