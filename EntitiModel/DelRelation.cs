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
    
    public partial class DelRelation
    {
        public int RECORDKEY { get; set; }
        public int DeliveryNo { get; set; }
        public string Branch { get; set; }
        public string Type { get; set; }
        public string ParentKey { get; set; }
        public int DelQty { get; set; }
        public Nullable<double> DelWeight { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Prefix { get; set; }
    }
}
