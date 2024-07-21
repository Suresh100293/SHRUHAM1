using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class TransactionChargesSetupVM
    {
        // iX9: Field Structure of Charges
        public int Charges_RECORDKEY { get; set; }
        public bool Charges_AfterTax { get; set; }
        public string Charges_Code { get; set; }
        public string Charges_CompCode { get; set; }
        public bool Charges_DontUse { get; set; }
        public string Charges_EqAmt { get; set; }
        public string Charges_EqBro { get; set; }
        public string Charges_EqCost { get; set; }
        public string Charges_EqSale { get; set; }
        public string Charges_EqTax { get; set; }
        public string Charges_Equation { get; set; }
        public string Charges_Fld { get; set; }
        public string Charges_Head { get; set; }
        public string Charges_HSNCode { get; set; }
        public string Charges_MainType { get; set; }
        public bool Charges_Post { get; set; }
        public decimal Charges_PostNo { get; set; }
        public decimal Charges_RoundOff { get; set; }
        public string Charges_SubType { get; set; }
        public string Charges_TaxCode { get; set; }
        public bool Charges_ToPrint { get; set; }
        public string Charges_Type { get; set; }
        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public List<SelectListItem> EqAmtList { get; set; }
        public List<SelectListItem> EqCostList { get; set; }
        public List<SelectListItem> EqSaleList { get; set; }
        public List<SelectListItem> EqBroList { get; set; }
        public List<SelectListItem> EqTaxList { get; set; }
        public string CodeName { get; set; }
        public string HSNCodeName { get; set; }
        public string TaxCodeName { get; set; }

        // iX9: Common default Fields
        public int Document { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Mode { get; set; }

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