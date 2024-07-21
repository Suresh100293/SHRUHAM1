using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class TDSMasterVM
    {
        // iX9: Field Structure of TDSMaster
        public int TDSMaster_RECORDKEY { get; set; }
        public int? TDSMaster_CertAfter { get; set; }
        public bool TDSMaster_CertAuto { get; set; }
        public int? TDSMaster_CertBefore { get; set; }
        public int? TDSMaster_Code { get; set; }
        public int? TDSMaster_DaysCert { get; set; }
        public int? TDSMaster_DaysDeposit { get; set; }
        public int? TDSMaster_DepositAfter { get; set; }
        public int? TDSMaster_DepositBefore { get; set; }
        public bool TDSMaster_Differ { get; set; }
        public int? TDSMaster_FileAfter { get; set; }
        public int? TDSMaster_FileBefore { get; set; }
        public System.DateTime? TDSMaster_FileDate { get; set; }
        public string FileDateVM { get; set; }
        public string TDSMaster_Form { get; set; }
        public bool TDSMaster_Form15H { get; set; }
        public int? TDSMaster_FormType { get; set; }
        public string TDSMaster_Name { get; set; }
        public string TDSMaster_PostCode { get; set; }
        public string TDSMaster_Prefix { get; set; }
        public string TDSMaster_Sections { get; set; }
        // iX9: Field Structure of TDSRates
        public int TDSRates_RECORDKEY { get; set; }
        public decimal? TDSRates_Cess { get; set; }
        public int TDSRates_Code { get; set; }
        public System.DateTime? TDSRates_EffDate { get; set; }
        public string EffDateVM { get; set; }
        public decimal? TDSRates_LimitFrom { get; set; }
        public decimal? TDSRates_LimitTo { get; set; }
        public decimal? TDSRates_SHECess { get; set; }
        public int? TDSRates_Sno { get; set; }
        public decimal? TDSRates_SurCharge { get; set; }
        public decimal? TDSRates_Tax { get; set; }
        public decimal? TDSRates_TDSRate { get; set; }
        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public string PostCodeName { get; set; }

        // iX9: Fields for GridView
        public bool tempIsDeleted { get; set; }
        public int tEmpID { get; set; }
        public IList<TDSMasterVM> GridDataVM { get; set; }

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