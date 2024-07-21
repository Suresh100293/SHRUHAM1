using ALT_ERP3.Areas.Logistics.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ALT_ERP3
{
    public partial class GridOption
    {



        public bool GetReportParameter { get; set; }
        public string ReportName { get; set; }
        public List<SelectListItem> SaveReportList { get; set; }
        public string HideColumnList { get; set; }
        public string ReportTypeL { get; set; }

        public bool BillDetails { get; set; }
        public bool CustomerF { get; set; }
        public string Age1 { get; set; }
        public string Age2 { get; set; }
        public string Age3 { get; set; }
        public string Age4 { get; set; }
        public string Age5 { get; set; }
        public string Age6 { get; set; }



        public List<GridOption> PrintGridList { get; set; }

        
        public string TableKey { get; set; }
        public string mWhat { get; set; }
        public string Para { get; set; }
        public string ParaReport { get; set; }
        public string Document { get; set; }
        public string Date { get; set; }
        public string sidx { get; set; }
        public string sord { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public int rows { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Type { get; set; }
        public string Area { get; set; }
        public string AcType { get; set; }
        public string query { get; set; }
        public string query3 { get; set; }
        public string query4 { get; set; }
        public string HeaderContent { get; set; }
        public string SelectContent { get; set; }
        public string searchtype { get; set; }
        public string searchtext { get; set; }
        public bool search { get; set; }
        public string Code { get; set; }
        public string queryforcount { get; set; }
        public string AccountName { get; set; }
        public string FromDate1 { get; set; }
        public string ToDate1 { get; set; }
        public bool _search { get; set; }
        public bool Supress { get; set; }
        public string searchField { get; set; }
        public string searchOper { get; set; }
        public string searchString { get; set; }
        public string MainType { get; set; }
        public string SubType { get; set; }
        public string ViewDataId { get; set; }
        public string id { get; set; }
        public string Header { get; set; }
        public string Mode { get; set; }
        public string TableName { get; set; }
        public string THead { get; set; }
        public string MenuName { get; set; }
        //public string optioncode {get; set;}
        public string Head { get; set; }
        public string PurchType { get; set; }
        public string Item { get; set; }
        public string IndentNumber { get; set; }
        public string Store { get; set; }
        public string Percentage { get; set; }
        public string Season { get; set; }
        public string FSN { get; set; }
        public string PurchCode { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string Value3 { get; set; }
        public string Salesman { get; set; }
        public string Party { get; set; }
        public string Branch { get; set; }
        public string Process { get; set; }
        public string Warehouse { get; set; }
        public string productCat { get; set; }
        public string Broker { get; set; }
        public string ProductGrp { get; set; }
        public string Order { get; set; }
        public string Name { get; set; }
        public string BankName { get; set; }
        public IList<GridOption> list { get; set; }
        public IList<GridOption> list1 { get; set; }
        //public SelectListItem[] types { get; set; }
        public string PurSaleType { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Docdate { get; set; }
        public string ViewCode { get; set; }
        public string Month { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Opening { get; set; }
        public decimal Closing { get; set; }
        public decimal Balance { get; set; }
        public string MonthName { get; set; }
        public string MonthName1 { get; set; }
        public decimal SumDebit { get; set; }
        public decimal SumCredit { get; set; }
        public decimal SumClosing { get; set; }
        public decimal Balance1 { get; set; }
        public decimal Highest { get; set; }
        public decimal lowest { get; set; }
        public string BaseGr { get; set; }
        public string Grp { get; set; }
        public string BaseGr1 { get; set; }
        public decimal Receipts { get; set; }
        public decimal Payments { get; set; }
        public string AccountDescription { get; set; }
        public string CostCentre { get; set; }
        public string Serial { get; set; }
        public int LocationCode { get; set; }
        public string Srl { get; set; }
        public string BillNumber { get; set; }
        public string ColField { get; set; }
        //public string Grp { get; set; }
        public string Flag { get; set; }
        public string Level { get; set; }
        public IList<GridOption> Grouplist { get; set; }
        public string Prefix { get; set; }
        public decimal Amount { get; set; }
        public decimal Pending { get; set; }
        public string DueDate { get; set; }
        public decimal Bal { get; set; }
        public decimal CurrAmount { get; set; }
        public string CurrName { get; set; }
        public string Narr { get; set; }
        public string Sales { get; set; }
        public string Purchase { get; set; }
        public IList<GridOption> Reclist1 { get; set; }
        public IList<GridOption> Reclist2 { get; set; }
        public IList<GridOption> RecClist1 { get; set; }
        public IList<GridOption> RecDList1 { get; set; }
        public string Sno { get; set; }
        public string ClearDate { get; set; }
        public string ChequeNo { get; set; }
        public bool AutoClose { get; set; }
        public string Controller { get; set; }
        public string Controller2 { get; set; }
        public string Module { get; set; }
        public bool ComController { get; set; }
        public string OptionType { get; set; }
        public string OptionCode { get; set; }
        public string CurrConv { get; set; }
        public string Format { get; set; }
        public bool IsFormatSelected { get; set; }
        public string StoreProcedure { get; set; }
        public List<string> AddOnParaList { get; set; }
        public bool xAdd { get; set; }
        public bool xEdit { get; set; }
        public bool xDelete { get; set; }
        public bool xView { get; set; }
        public bool xPrint { get; set; }
        public bool IsHidden { get; set; }
        public string mParaString { get; set; }
        //public List<StockBatchVM> ItemBatchListPS { get; set; }
        public string BranchName { get; set; }
        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }
        public string mVar1 { get; set; }
        public string mVar2 { get; set; }
        public string mVar3 { get; set; }
        public string mVar4 { get; set; }
        public string mVar5 { get; set; }
        public string ViewName { get; set; }
        public string Filter { get; set; }
        public string TempMainType { get; set; }
        public string TempMainTypeN { get; set; }
        public string TempSubType { get; set; }
        public string TempSubTypeN { get; set; }
        public string TempType { get; set; }
        public string TempTypeN { get; set; }

        public bool ARAPReqOnly { get; set; }
        public bool ScheduleFlow { get; set; }
        public EnumVehicleStatus vehicleStatus { get; set; }
        public bool LastBranchForFm { get; set; }
        public bool Customer { get; set; }
        public bool Rework { get; set; }

        public List<SelectListItem> Customers { get; set; }
        public List<SelectListItem> Accounts { get; set; }
        public string CustomerL { get; set; }
        public string CustomerCodes { get; set; }

        public List<SelectListItem> Branches { get; set; }
        public string BranchL { get; set; }

        public List<SelectListItem> Users { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }

        public List<SelectListItem> DocTypes { get; set; }
        public string DocType { get; set; }
        public string DocTypeL { get; set; }

        public List<SelectListItem> OptionModes { get; set; }
        public string OptionMode { get; set; }
        public string OptionModeL { get; set; }

        public List<SelectListItem> CashBankList { get; set; }
        public string CashBank { get; set; }
        public string CashBankL { get; set; }

        public string TemplateCode { get; set; }
        public string TemplateName { get; set; }

        public string SMSUser { get; set; }
        public string SMSCustomer { get; set; }

        public string MobileNO { get; set; }
        public string EmailId { get; set; }


        public string SundryCreditorsFilterGroups { get; set; }

    }
}