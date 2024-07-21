using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class HSNMasterVM
    {
        // iX9: Field Structure of HSNMaster
        public int HSNMaster_RECORDKEY { get; set; }
        public string HSNMaster_CessIn { get; set; }
        public string HSNMaster_CessOut { get; set; }
        public string HSNMaster_CGSTIn { get; set; }
        public string HSNMaster_CGSTOut { get; set; }
        public string HSNMaster_Code { get; set; }
        public string HSNMaster_Flag { get; set; }
        public int HSNMaster_Grp { get; set; }
        public string HSNMaster_IGSTIn { get; set; }
        public string HSNMaster_IGSTOut { get; set; }
        public string HSNMaster_Name { get; set; }
        public string HSNMaster_Narr { get; set; }
        public string HSNMaster_SGSTIn { get; set; }
        public string HSNMaster_SGSTOut { get; set; }
        public string HSNMaster_Unit { get; set; }
        // iX9: Field Structure of HSNRates
        public int HSNRates_RECORDKEY { get; set; }
        public decimal HSNRates_Abatement { get; set; }
        public decimal HSNRates_CessRate { get; set; }
        public decimal HSNRates_CGSTRate { get; set; }
        public string HSNRates_Code { get; set; }
        public bool HSNRates_DiscOnTxbl { get; set; }
        public System.DateTime? HSNRates_EffDate { get; set; }
        public string HSNRates_EffDateVM { get; set; }
        public decimal HSNRates_IGSTRate { get; set; }
        public decimal HSNRates_RateLimit { get; set; }
        public decimal HSNRates_SGSTRate { get; set; }
        public int? HSNRates_Sno { get; set; }
        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public string GrpName { get; set; }
        public string UnitName { get; set; }
        public string IGSTInName { get; set; }
        public string IGSTOutName { get; set; }
        public string SGSTInName { get; set; }
        public string SGSTOutName { get; set; }
        public string CGSTInName { get; set; }
        public string CGSTOutName { get; set; }
        public string CessInName { get; set; }
        public string CessOutName { get; set; }

        // iX9: Fields for GridView
        public bool tempIsDeleted { get; set; }
        public int tEmpID { get; set; }
        public IList<HSNMasterVM> GridDataVM { get; set; }

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
        public bool AutoClose { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
    }
}