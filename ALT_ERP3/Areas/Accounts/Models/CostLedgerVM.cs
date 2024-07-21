using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class CostLedgerVM
    {
        public int RECORDKEY { get; set; }
        public string AUTHIDS { get; set; }
        public string AUTHORISE { get; set; }
        public string Branch { get; set; }
        public string Code { get; set; }
        public int CostCode { get; set; }
        public string CostGrp { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
        public System.DateTime DocDate { get; set; }
        public string MainType { get; set; }
        public string Prefix { get; set; }
        public string Sno { get; set; }
        public string Srl { get; set; }
        public string SubType { get; set; }
        public string Type { get; set; }
        public string EnteredBy { get; set; }
        public System.DateTime LastUpdateDate { get; set; }
        public decimal DocAmount { get; set; }
        public string LocationCode { get; set; }

        public decimal Amount { get; set; }
        public int SrNo { get; set; }
    }
}