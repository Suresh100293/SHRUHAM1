using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class CountryMasterVM
    {
        // iX9: Field Structure of TfatCountry
        public int TfatCountry_RECORDKEY { get; set; }
        public int TfatCountry_Code { get; set; }
        public int TfatCountry_CountryCode { get; set; }
        public string TfatCountry_CurCode { get; set; }
        public string TfatCountry_CurDecName { get; set; }
        public int TfatCountry_CurrDec { get; set; }
        public int TfatCountry_CurrName { get; set; }
        public decimal TfatCountry_CurrRate { get; set; }
        public string TfatCountry_DialCode { get; set; }
        public int TfatCountry_Language { get; set; }
        public string TfatCountry_LockCode { get; set; }
        public string TfatCountry_Name { get; set; }
        public int TfatCountry_ResourceOffset { get; set; }
        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public string CurrNameName { get; set; }
        public string LanguageName { get; set; }
        public HttpPostedFileBase AttacheFile { get; set; }

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