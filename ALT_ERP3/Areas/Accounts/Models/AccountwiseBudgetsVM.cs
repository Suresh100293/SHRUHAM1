using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class AccountwiseBudgetsVM
    {
        // iX9: Field Structure of Budgets
        public int Budgets_RECORDKEY { get; set; }
        public decimal? Budgets_Annual { get; set; }
        public double? Budgets_AnnualQty { get; set; }
        public int? Budgets_Area { get; set; }
        public string Budgets_Branch { get; set; }
        public string Budgets_BudgetType { get; set; }
        public string Budgets_Code { get; set; }
        public string Budgets_CompCode { get; set; }
        public bool Budgets_ControlBP { get; set; }
        public bool Budgets_ControlBS { get; set; }
        public bool Budgets_ControlBY { get; set; }
        public bool Budgets_ControlMCP { get; set; }
        public bool Budgets_ControlMCS { get; set; }
        public bool Budgets_ControlMCY { get; set; }
        public decimal? Budgets_ControlToleP { get; set; }
        public decimal? Budgets_ControlToleS { get; set; }
        public decimal? Budgets_ControlToleY { get; set; }
        public bool Budgets_ControlTypeP { get; set; }
        public bool Budgets_ControlTypeS { get; set; }
        public bool Budgets_ControlTypeY { get; set; }
        public int? Budgets_CostCentre { get; set; }
        public double? Budgets_Factor { get; set; }
        public string Budgets_Flag { get; set; }
        public int? Budgets_ItemCategory { get; set; }
        public string Budgets_ItemGroup { get; set; }
        public int Budgets_LocationCode { get; set; }
        public string Budgets_Party { get; set; }
        public string Budgets_Prefix { get; set; }
        public string Budgets_Product { get; set; }
        public double? Budgets_Qty1 { get; set; }
        public double? Budgets_Qty10 { get; set; }
        public double? Budgets_Qty11 { get; set; }
        public double? Budgets_Qty12 { get; set; }
        public double? Budgets_Qty3 { get; set; }
        public double? Budgets_Qty4 { get; set; }
        public double? Budgets_Qty5 { get; set; }
        public double? Budgets_Qty6 { get; set; }
        public double? Budgets_Qty7 { get; set; }
        public double? Budgets_Qty8 { get; set; }
        public double? Budgets_Qty9 { get; set; }
        public decimal? Budgets_Ratio1 { get; set; }
        public decimal? Budgets_Ratio10 { get; set; }
        public decimal? Budgets_Ratio11 { get; set; }
        public decimal? Budgets_Ratio12 { get; set; }
        public decimal? Budgets_Ratio2 { get; set; }
        public decimal? Budgets_Ratio3 { get; set; }
        public decimal? Budgets_Ratio4 { get; set; }
        public decimal? Budgets_Ratio5 { get; set; }
        public decimal? Budgets_Ratio6 { get; set; }
        public decimal? Budgets_Ratio7 { get; set; }
        public decimal? Budgets_Ratio8 { get; set; }
        public decimal? Budgets_Ratio9 { get; set; }
        public int? Budgets_Salesman { get; set; }
        public decimal? Budgets_Sanctioned { get; set; }
        public string Budgets_SelectFlags { get; set; }
        public string Budgets_Type { get; set; }
        public decimal? Budgets_Val1 { get; set; }
        public decimal? Budgets_Val10 { get; set; }
        public decimal? Budgets_Val11 { get; set; }
        public decimal? Budgets_Val12 { get; set; }
        public decimal? Budgets_Val2 { get; set; }
        public decimal? Budgets_Val3 { get; set; }
        public decimal? Budgets_Val4 { get; set; }
        public decimal? Budgets_Val5 { get; set; }
        public decimal? Budgets_Val6 { get; set; }
        public decimal? Budgets_Val7 { get; set; }
        public decimal? Budgets_Val8 { get; set; }
        public decimal? Budgets_Val9 { get; set; }
        public string Budgets_xValue1 { get; set; }
        public string Budgets_xValue2 { get; set; }

        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public List<SelectListItem> PrefixList { get; set; }
        public string CodeName { get; set; }

        public bool CheckMode { get; set; }
        public string Message { get; set; }

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