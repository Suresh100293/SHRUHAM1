using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class LabourGradesVM
    {
        // iX9: Field Structure of Grade
        public int Grade_RECORDKEY { get; set; }
        public int Grade_Code { get; set; }
        public int Grade_CrewSize { get; set; }
        public string Grade_Name { get; set; }

        // iX9: Field Structure of EmpGradeRates
        public int EmpGradeRates_RECORDKEY { get; set; }
        public int EmpGradeRates_Code { get; set; }
        public System.DateTime EmpGradeRates_EffDate { get; set; }
        public string EmpGradeRates_EffDateVM { get; set; }
        public decimal? EmpGradeRates_OTRate { get; set; }
        public double? EmpGradeRates_Rate { get; set; }
        public int EmpGradeRates_Sno { get; set; }
        public string EmpGradeRates_Unit { get; set; }

        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public List<SelectListItem> UnitList { get; set; }

        public bool tempIsDeleted { get; set; }
        public List<LabourGradesVM> DocumentList { get; set; }
        // iX9: Fields for GridView
        public IList<LabourGradesVM> GridEmpGradeRatesVM { get; set; }
        public bool CheckMode { get; set; }
        public string Message { get; set; }

        public int tempId { get; set; }
        // iX9: Common default Fields
        public List<GridOption> PrintGridList { get; set; }
        public int Document { get; set; }
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