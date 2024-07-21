using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class PaymentTermsVM
    {
        // iX9: Field Structure of PaymentTerms
        public int PaymentTerms_RECORDKEY { get; set; }
        public int PaymentTerms_Code { get; set; }
        public int? PaymentTerms_CrPeriod { get; set; }
        public int? PaymentTerms_EarlyDay1 { get; set; }
        public int? PaymentTerms_EarlyDay2 { get; set; }
        public int? PaymentTerms_EarlyDay3 { get; set; }
        public int? PaymentTerms_EarlyDay4 { get; set; }
        public int? PaymentTerms_EarlyDay5 { get; set; }
        public decimal? PaymentTerms_EarlyPerc1 { get; set; }
        public decimal? PaymentTerms_EarlyPerc2 { get; set; }
        public decimal? PaymentTerms_EarlyPerc3 { get; set; }
        public decimal? PaymentTerms_EarlyPerc4 { get; set; }
        public decimal? PaymentTerms_EarlyPerc5 { get; set; }
        public int? PaymentTerms_LateDay1 { get; set; }
        public int? PaymentTerms_LateDay2 { get; set; }
        public int? PaymentTerms_LateDay3 { get; set; }
        public int? PaymentTerms_LateDay4 { get; set; }
        public int? PaymentTerms_LateDay5 { get; set; }
        public decimal? PaymentTerms_LatePerc1 { get; set; }
        public decimal? PaymentTerms_LatePerc2 { get; set; }
        public decimal? PaymentTerms_LatePerc3 { get; set; }
        public decimal? PaymentTerms_LatePerc4 { get; set; }
        public decimal? PaymentTerms_LatePerc5 { get; set; }
        public string PaymentTerms_Name { get; set; }
        public string PaymentTerms_Narr { get; set; }


        public bool CheckMode { get; set; }
        public string Message { get; set; }

        // iX9: Common default Fields
        public List<GridOption> PrintGridList { get; set; }
        public int Document { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Mode { get; set; }
        public bool AuthLock { get; set; }
        public bool AuthNoPrint { get; set; }
        public bool AuthReq { get; set; }
        public bool AuthAgain { get; set; }
        public string RichNote { get; set; }

        // iX9: used while passing parameters from MasterGrid to Option and Back 
        public string MainType { get; set; }
        public string Controller2 { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
    }
}