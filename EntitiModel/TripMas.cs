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
    
    public partial class TripMas
    {
        public int RECORDKEY { get; set; }
        public string Code { get; set; }
        public Nullable<decimal> Diesel { get; set; }
        public Nullable<System.DateTime> FromDt { get; set; }
        public Nullable<decimal> Rate { get; set; }
        public string Remark { get; set; }
        public Nullable<System.DateTime> ToDt { get; set; }
        public bool TripType { get; set; }
        public string Vcode { get; set; }
        public string VType { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
    }
}
