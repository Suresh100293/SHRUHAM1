using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class MonthlyBookingUnBillLRVM
    {
        public string ReportName { get; set; }
        public List<SelectListItem> SaveReportList { get; set; }
        public string HideColumnList { get; set; }
        public string ReportTypeL { get; set; }
        public string StockType { get; set; }

        public bool NoOfLR { get; set; }
        public bool TotalQty { get; set; }
        public bool TotalWT { get; set; }
        public bool TotalAmt { get; set; }


        public int page { get; set; }
        public int rows { get; set; }
        public string searchField { get; set; }
        public string searchOper { get; set; }
        public string searchString { get; set; }
        public string sidx { get; set; }
        public string sord { get; set; }
        public string mWhat { get; set; }
        public string ViewCode { get; set; }


        public List<SelectListItem> Branches { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }

        // iX9: used while passing parameters from MasterGrid to Option and Back 
        public string MainType { get; set; }
        public string Controller2 { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
        public string Document { get; set; }
        public string Controller { get; set; }
        public string Date { get; set; }
        public string Mode { get; set; }
        public string SelectContent { get; set; }
        
    }
}