using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class ChergesListWithHSN
    {
        public string Type { get; set; }
        public string TableKey { get; set; }
        public int Sno { get; set; }
        public string HSNCode { get; set; }
        public int Qty { get; set; }
        public bool FreeQty { get; set; }
        public string Unit { get; set; }
        public string PrdDesc { get; set; }
        public decimal Rate { get; set; }
        public decimal QtyRate { get; set; }
        public decimal Taxable { get; set; }
        public decimal GSTRate { get; set; }
        public decimal sGSTAmt { get; set; }
        public decimal iGSTAmt { get; set; }
        public decimal cGSTAmt { get; set; }
        public decimal Cess { get; set; }
        public decimal Amt { get; set; }
    }
}