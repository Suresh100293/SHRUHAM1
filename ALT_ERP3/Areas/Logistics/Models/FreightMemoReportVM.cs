using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class FreightMemoReportVM
    {
        public bool GetReportParameter { get; set; }
        public string ReportName { get; set; }
        public List<SelectListItem> SaveReportList { get; set; }
        public string HideColumnList { get; set; }

        public string FromDate { get; set; }
        public string ToDate { get; set; }

        public string FreightMemoNo { get; set; }

        public List<SelectListItem> Branches { get; set; }
        public List<SelectListItem> BranchesOnly { get; set; }

        public List<SelectListItem> ReportsType { get; set; }
        public string ReportTypeL { get; set; }

        public string Branch { get; set; }
        public string BranchL { get; set; }

        public string LoadFrom { get; set; }
        public string LoadFromL { get; set; }

        public string SendTo { get; set; }
        public string SendToL { get; set; }

        public List<SelectListItem> Brokers { get; set; }
        public string Broker { get; set; }
        public string BrokerL { get; set; }

        public string PAyableAt { get; set; }
        public string PAyableAtL { get; set; }

        public List<SelectListItem> TruckNos { get; set; }
        public string TruckNo { get; set; }
        public string TruckNoL { get; set; }

        public List<SelectListItem> Drivers { get; set; }
        public string Driver { get; set; }
        public string DriverL { get; set; }

        public int page { get; set; }
        public int rows { get; set; }
        public string searchField { get; set; }
        public string searchOper { get; set; }
        public string searchString { get; set; }
        public string sidx { get; set; }
        public string sord { get; set; }
        public string mWhat { get; set; }

        public bool PaymentDetails { get; set; }
        

        public bool Advance { get; set; }
        public bool Balance { get; set; }

        public string VehicleType { get; set; }

        public bool FMExpensesDetails { get; set; }
        public bool LRExpensesDetails { get; set; }
        public bool DispatchDetails { get; set; }
        public bool LorryReceiptDetails { get; set; }
        public bool TripDetails { get; set; }
        public bool PLAccount { get; set; }


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
        public dynamic Controller { get;   set; }
        public string Date { get;   set; }
        public string ViewCode { get;   set; }
        public string Mode { get;   set; }
    }
}