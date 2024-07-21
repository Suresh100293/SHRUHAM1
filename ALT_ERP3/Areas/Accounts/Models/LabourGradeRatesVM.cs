using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class LabourGradeRatesVM
    {
        // iX9: Field Structure of EmpGradeRates
        public int EmpGradeRates_RECORDKEY { get; set; }
        public int EmpGradeRates_Code { get; set; }
        public System.DateTime EmpGradeRates_DateFrom { get; set; }
        public string EmpGradeRates_DateFromVM { get; set; }
        public System.DateTime EmpGradeRates_DateTo { get; set; }
        public string EmpGradeRates_DateToVM { get; set; }
        public decimal EmpGradeRates_OTRate { get; set; }
        public double EmpGradeRates_Rate { get; set; }
        public string EmpGradeRates_Unit { get; set; }
        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public string CodeName { get; set; }
        public string UnitName { get; set; }

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