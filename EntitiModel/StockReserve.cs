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
    
    public partial class StockReserve
    {
        public int RECORDKEY { get; set; }
        public string Branch { get; set; }
        public bool CancFlag { get; set; }
        public string Code { get; set; }
        public string CompCode { get; set; }
        public System.DateTime DocDate { get; set; }
        public int LocationCode { get; set; }
        public string ParentDoc { get; set; }
        public string ParentKey { get; set; }
        public string Prefix { get; set; }
        public string RefDocument { get; set; }
        public Nullable<double> ReservedQty { get; set; }
        public int Sno { get; set; }
        public string Srl { get; set; }
        public int Store { get; set; }
        public string TableKey { get; set; }
        public string Type { get; set; }
        public string xValue1 { get; set; }
        public string xValue2 { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
    }
}
