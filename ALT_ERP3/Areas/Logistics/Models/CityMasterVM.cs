using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class CityMasterVM
    {
        // iX9: Field Structure of TfatCity
        public int TfatCity_RECORDKEY { get; set; }
        public int TfatCity_Code { get; set; }
        public string TfatCity_Name { get; set; }
        public string TfatCity_State { get; set; }

        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public string StateName { get; set; }

        public bool CheckMode { get; set; }
        public string Message { get; set; }

        // iX9: Common default Fields
        public List<GridOption> PrintGridList { get; set; }
        public string Document { get; set; }
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