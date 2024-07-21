using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class PODReportVM
    {
        public bool GetReportParameter { get; set; }
        public string ReportName { get; set; }

        public string FromDate { get; set; }
        public string ToDate { get; set; }

        public List<SelectListItem> Branches { get; set; }
        public string EntryBranch { get; set; }
        public string EntryBranchL { get; set; }
        public string FromBranch { get; set; }
        public string FromBranchL { get; set; }
        public string ToBranch { get; set; }
        public string ToBranchL { get; set; }

        public List<SelectListItem> ReportsType { get; set; }
        public string ReportType { get; set; }
        public string ReportTypeL { get; set; }

        public List<SelectListItem> Customers { get; set; }
        public string Customer { get; set; }
        public string CustomerL { get; set; }

        public string PODReceivedBranch_Direct { get; set; }
        public string PODSendCustomer_Branch_File { get; set; }
        public string PODStatusActive_Closed_All { get; set; }



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
        public dynamic Controller { get; set; }
        public string Date { get; set; }
        public string ViewCode { get; set; }
        public string Mode { get; set; }

        public int page { get; set; }
        public int rows { get; set; }
        public string searchField { get; set; }
        public string searchOper { get; set; }
        public string searchString { get; set; }
        public string sidx { get; set; }
        public string sord { get; set; }
        public string mWhat { get; set; }
    }
}