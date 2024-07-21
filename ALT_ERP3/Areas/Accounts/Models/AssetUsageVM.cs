using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class AssetUsageVM
    {
        // iX9: Field Structure of AssetUsage
        public int AssetUsage_RECORDKEY { get; set; }
        public string AssetUsage_AssetID { get; set; }
        public string AssetUsage_Branch { get; set; }
        public string AssetUsage_CompCode { get; set; }
        public System.DateTime AssetUsage_FromDate { get; set; }
        public string FromDateVM { get; set; }
        public int AssetUsage_LocationCode { get; set; }
        public int? AssetUsage_Shift { get; set; }
        public System.DateTime AssetUsage_ToDate { get; set; }
        public string ToDateVM { get; set; }
        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public string AssetIDName { get; set; }
        public string ShiftName { get; set; }

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