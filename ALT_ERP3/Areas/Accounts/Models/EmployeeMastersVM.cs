using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class EmployeeMastersVM
    {
        // iX9: Field Structure of Employee
        public int Employee_RECORDKEY { get; set; }
        public string Employee_AadharNo { get; set; }
        public string Employee_AccNo { get; set; }
        public int Employee_AnnualBonus { get; set; }
        public string Employee_AppBranch { get; set; }
        public decimal Employee_Basic { get; set; }
        public string Employee_BasicType { get; set; }
        public string Employee_BkCode { get; set; }
        public int Employee_CateNo { get; set; }
        public string Employee_CitizenId { get; set; }
        public System.DateTime Employee_CPRExpiDt { get; set; }
        public string Employee_CPRExpiDtVM { get; set; }
        public System.DateTime Employee_CPRIssuDt { get; set; }
        public string Employee_CPRIssuDtVM { get; set; }
        public string Employee_CPRNo { get; set; }
        public int Employee_DaysPerPeriod { get; set; }
        public int Employee_Dept { get; set; }
        public System.DateTime Employee_Dob { get; set; }
        public string Employee_DobVM { get; set; }
        public System.DateTime Employee_Doc { get; set; }
        public string Employee_DocVM { get; set; }
        public System.DateTime Employee_Doi { get; set; }
        public string Employee_DoiVM { get; set; }
        public System.DateTime Employee_Doj { get; set; }
        public string Employee_DojVM { get; set; }
        public System.DateTime Employee_Dol { get; set; }
        public string Employee_DolVM { get; set; }
        public System.DateTime Employee_Dor { get; set; }
        public string Employee_DorVM { get; set; }
        public string Employee_EmpCompId { get; set; }
        public int Employee_EmpGrade { get; set; }
        public string Employee_EmpID { get; set; }
        public int Employee_EmpType { get; set; }
        public int Employee_ESICAppl { get; set; }
        public string Employee_ESICNo { get; set; }
        public int Employee_FPFAppl { get; set; }
        public string Employee_FPFNo { get; set; }
        public string Employee_GradeNo { get; set; }
        public int Employee_HolidayAppl { get; set; }
        public decimal Employee_HrsPerDay { get; set; }
        public string Employee_LeftReason { get; set; }
        public decimal Employee_LnOpen { get; set; }
        public decimal Employee_LoanOpen { get; set; }
        public int Employee_LWFappl { get; set; }
        public string Employee_LWFNo { get; set; }
        public decimal Employee_MaxLoan { get; set; }
        public bool Employee_Monthly { get; set; }
        public bool Employee_MonthlyBasic { get; set; }
        public string Employee_Name { get; set; }
        public bool Employee_NonPension { get; set; }
        public decimal Employee_OTRate { get; set; }
        public string Employee_OTUnit { get; set; }
        public string Employee_PAN { get; set; }
        public int Employee_PaymentMode { get; set; }
        public int Employee_PF { get; set; }
        public decimal Employee_PFAmt { get; set; }
        public int Employee_PfAppl { get; set; }
        public string Employee_PFCode { get; set; }
        public string Employee_PfNo { get; set; }
        public int Employee_PostNo { get; set; }
        public int Employee_PostNoJoin { get; set; }
        public int Employee_Prefix { get; set; }
        public decimal Employee_ProfAmt { get; set; }
        public int Employee_ProfAppl { get; set; }
        public string Employee_PTNo { get; set; }
        public decimal Employee_RateHour { get; set; }
        public int Employee_Shift { get; set; }
        public string Employee_Status { get; set; }
        public decimal Employee_SumPresent { get; set; }
        public int Employee_TDSAppl { get; set; }
        public decimal Employee_TotLoan { get; set; }
        public decimal Employee_UnBasic { get; set; }
        public System.DateTime Employee_UpToDate { get; set; }
        public string Employee_UpToDateVM { get; set; }
        public int Employee_WkDays { get; set; }
        public string Employee_WkHoli1 { get; set; }
        public string Employee_WkHoli2 { get; set; }
        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public List<SelectListItem> OTUnitList { get; set; }
        public string DeptName { get; set; }
        public string CateNoName { get; set; }
        public string EmpGradeName { get; set; }

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