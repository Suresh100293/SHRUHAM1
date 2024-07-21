using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class AssetsTransferVM
    {
        // iX9: Field Structure of AssetTracking
        public int AssetTracking_RECORDKEY { get; set; }
        public string AssetTracking_AssetID { get; set; }
        public System.DateTime? AssetTracking_EndDate { get; set; }
        public string EndDateVM { get; set; }
        public System.DateTime? AssetTracking_EndTime { get; set; }
        public string EndTimeVM { get; set; }
        public string AssetTracking_fBranch { get; set; }
        public int AssetTracking_fLocation { get; set; }
        public string AssetTracking_Narr { get; set; }
        public string AssetTracking_operators { get; set; }
        public string AssetTracking_Prefix { get; set; }
        public double AssetTracking_Qty { get; set; }
        public double AssetTracking_Qty2 { get; set; }
        public string AssetTracking_Srl { get; set; }
        public System.DateTime? AssetTracking_STDate { get; set; }
        public string STDateVM { get; set; }
        public System.DateTime? AssetTracking_stTime { get; set; }
        public string stTimeVM { get; set; }
        public string AssetTracking_TagNo { get; set; }
        public string AssetTracking_tBranch { get; set; }
        public int AssetTracking_tLocation { get; set; }
        public string AssetTracking_Type { get; set; }
        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public string fBranchName { get; set; }
        public string fLocationName { get; set; }
        public string tBranchName { get; set; }
        public string tLocationName { get; set; }
        public string AssetIDName { get; set; }

        // iX9: Common default Fields
        public string Document { get; set; }
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