using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class AssetsCategoryVM
    {
        // iX9: Field Structure of AssetCategory
        public int AssetCategory_RECORDKEY { get; set; }
        public string AssetCategory_Account { get; set; }
        public int AssetCategory_Code { get; set; }
        public string AssetCategory_DeprAccount { get; set; }
        public bool AssetCategory_Depreciable { get; set; }
        public string AssetCategory_DeprMethod { get; set; }
        public decimal? AssetCategory_DeprRate { get; set; }
        public decimal? AssetCategory_DeprRateIT { get; set; }
        public string AssetCategory_Name { get; set; }
        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public List<SelectListItem> DeprMethodList { get; set; }

        // iX9: Common default Fields
        public int Document { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Mode { get; set; }

        // iX9: used while passing parameters from MasterGrid to Option and Back 
        public string Controller2 { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
    }
}