using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public partial class StockBatchVM
    {
        public int RecordKey { get; set; }
        public string AUTHIDS { get; set; }
        public string AUTHORISE { get; set; }
        public string Batch { get; set; }
        public string BatchParent { get; set; }
        public string Branch { get; set; }
        public string Code { get; set; }
        public System.DateTime DocDate { get; set; }
        public System.DateTime? ExpDate { get; set; }
        public double Factor { get; set; }
        public string HWSerial { get; set; }
        public string HWSerial2 { get; set; }
        public string ItemCategory { get; set; }
        public string MainType { get; set; }
        public System.DateTime? MfgDate { get; set; }
        public double? MRP { get; set; }
        public string Narr { get; set; }
        public decimal? NewRate { get; set; }
        public bool NoExpiry { get; set; }
        public bool NotInStock { get; set; }
        public string Party { get; set; }
        public string Prefix { get; set; }
        public double Qty { get; set; }
        public double Qty2 { get; set; }
        public double? Rate { get; set; }
        public string SerialNumber { get; set; }
        public string Sno { get; set; }
        public string Srl { get; set; }
        public int? Store { get; set; }
        public string SubType { get; set; }
        public string Type { get; set; }
        public string EnteredBy { get; set; }
        public System.DateTime LastUpdateDate { get; set; }
        public decimal GrWt { get; set; }
        public string LocationCode { get; set; }
        public decimal NettWt { get; set; }
        public decimal Potency { get; set; }
        public string Shade { get; set; }

        public double Balance { get; set; }
        public double UseQty { get; set; }

        public int SrNo { get; set; }
        public int tEmpID { get; set; }
        public bool tempIsDeleted { get; set; }
        public bool tempIsAdded { get; set; }
        public bool ExistData { get; set; }
        public string StrExpDate { get; set; }
        public string StrMfgDate { get; set; }
    }
}