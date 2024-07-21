using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class SalesTaxVM
    {
        // iX9: Field Structure of TaxMaster
        public int TaxMaster_RECORDKEY { get; set; }
        public decimal TaxMaster_Cess { get; set; }
        public string TaxMaster_CessCode { get; set; }
        public decimal TaxMaster_CessRate { get; set; }
        public string TaxMaster_CGSTCode { get; set; }
        public decimal TaxMaster_CGSTRate { get; set; }
        public string TaxMaster_Code { get; set; }
        public bool TaxMaster_Composition { get; set; }
        public bool TaxMaster_DiscOnTxbl { get; set; }
        public bool TaxMaster_Exempted { get; set; }
        public string TaxMaster_Form { get; set; }
        public string TaxMaster_FormName { get; set; }
        public int TaxMaster_GSTType { get; set; }
        public string TaxMaster_IGSTCode { get; set; }
        public decimal TaxMaster_IGSTRate { get; set; }
        public bool TaxMaster_Inclusive { get; set; }
        public bool TaxMaster_Labour { get; set; }
        public bool TaxMaster_Locked { get; set; }
        public bool TaxMaster_MRPTax { get; set; }
        public string TaxMaster_Name { get; set; }
        public decimal TaxMaster_Pct { get; set; }
        public string TaxMaster_PostCode { get; set; }
        public string TaxMaster_Scope { get; set; }
        public decimal TaxMaster_SetOff { get; set; }
        public string TaxMaster_SGSTCode { get; set; }
        public decimal TaxMaster_SGSTRate { get; set; }
        public bool TaxMaster_Taxable { get; set; }
        public string TaxMaster_TaxableCode { get; set; }
        public bool TaxMaster_VATGST { get; set; }
        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public List<SelectListItem> ScopeList { get; set; }
        public List<SelectListItem> FormList { get; set; }
        public string PostCodeName { get; set; }
        public string SGSTCodeName { get; set; }
        public string CGSTCodeName { get; set; }
        public string IGSTCodeName { get; set; }
        public string CessCodeName { get; set; }

        // iX9: Common default Fields
        public string Document { get; set; }
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