using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class ChargesVM
    {
        public bool AfterExcise { get; set; }
        public bool AfterTax { get; set; }
        public string Branch { get; set; }
        public string Code { get; set; }
        public decimal DefaultValue { get; set; }
        public string DocType { get; set; }
        public bool DontUse { get; set; }
        public string EqAmt { get; set; }
        public string EqBro { get; set; }
        public string EqCost { get; set; }
        public string EqExc { get; set; }
        public string EqSale { get; set; }
        public string EqSTax { get; set; }
        public string EqTax { get; set; }
        public string Equation { get; set; }
        public string Fld { get; set; }
        public string Head { get; set; }
        public string LocationCode { get; set; }
        public string MainType { get; set; }
        public bool Post { get; set; }
        public decimal PostNo { get; set; }
        public string TotalValues { get; set; }
        public decimal RoundOff { get; set; }
        public string SubType { get; set; }
        public string TaxCode { get; set; }
        public bool ToPrint { get; set; }
        public bool CheckHigh { get; set; }
        public string HSNCode { get; set; }
        //        //public decimal TOUCHVALUE { get; set; }
        public int RECORDKEY { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        ////public string HWSerial { get; set; }
        ////public string HWSerial2 { get; set; }
        public List<ChargesVM> Charges { get; set; }
        public decimal Amt1 { get; set; }
        public decimal val1 { get; set; }
        public int tEmpID { get; set; }
        public bool tempIsDeleted1 { get; set; }
        public string Factor { get; set; }
        public string ValueLast { get; set; }
    }
}