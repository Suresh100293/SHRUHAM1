using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class StockSerialVM
    {
        public int RECORDKEY { get; set; }
        public string AUTHIDS { get; set; }
        public string AUTHORISE { get; set; }
        public System.DateTime BillDate { get; set; }
        public string BillNumber { get; set; }
        public string Branch { get; set; }
        public string Code { get; set; }
        public System.DateTime DocDate { get; set; }
        public decimal? GrossWt { get; set; }
        public string ItemCategory { get; set; }
        public string MainType { get; set; }
        public decimal? NettWt { get; set; }
        public decimal NewRate { get; set; }
        public bool NotInStock { get; set; }
        public string PartyCode { get; set; }
        public string Prefix { get; set; }
        public decimal Qty { get; set; }
        public double Rate { get; set; }
        public string Reference { get; set; }
        public string RevEntry { get; set; }
        public string SerialNumber { get; set; }
        public string Sno { get; set; }
        public decimal sQty { get; set; }
        public string Srl { get; set; }
        public int Store { get; set; }
        public string SubType { get; set; }
        public bool TagPrepared { get; set; }
        public string Type { get; set; }
        public string EnteredBy { get; set; }
        public System.DateTime LastUpdateDate { get; set; }
        public string LocationCode { get; set; }
        public int tEmpID { get; set; }
        public int SrNo { get; set; }
        public int Index { get; set; }
        public bool ExistData { get; set; }
    }
}