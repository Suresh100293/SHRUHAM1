using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3
{
    public class ProductionScheduleVM
    {
        public int RECORDKEY { get; set; }
        public string Allocation { get; set; }
        public string AUTHIDS { get; set; }
        public string AUTHORISE { get; set; }
        public string Branch { get; set; }
        public string CancFlag { get; set; }
        public string Code { get; set; }
        public System.DateTime DelyDate { get; set; }
        public System.DateTime DocDate { get; set; }
        public double Factor { get; set; }
        public bool FirstSno { get; set; }
        //public string HWSerial { get; set; }
        //public string HWSerial2 { get; set; }
        public string ItemCategory { get; set; }
        public string MainType { get; set; }
        public string Narr { get; set; }
        public string OrdNumber { get; set; }
        public string Party { get; set; }
        public string PlanNumber { get; set; }
        public string Prefix { get; set; }
        public string ProcessCode { get; set; }
        public decimal Qty { get; set; }
        public decimal Qty2 { get; set; }
        public string QtyEqsn { get; set; }
        public decimal RateOn { get; set; }
        public string Sno { get; set; }
        public string Srl { get; set; }
        public int Stage { get; set; }
        public string Store { get; set; }
        public string SubType { get; set; }
        public string Type { get; set; }
        public string Unit { get; set; }
        public string Unit2 { get; set; }
        public string WONumber { get; set; }
        public string WRKNumber { get; set; }
        public string zQty { get; set; }
        //public decimal TOUCHVALUE { get; set; }
        public string EnteredBy { get; set; }
        public System.DateTime LastUpdateDate { get; set; }
        public string LocationCode { get; set; }
        public decimal Warp { get; set; }
        public decimal Weft { get; set; }
    }
}