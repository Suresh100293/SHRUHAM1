using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class LorryReceiptReportsVM
    {

        public bool GetReportParameter { get; set; }
        public string ReportName { get; set; }
        public List<SelectListItem> SaveReportList { get; set; }
        public string HideColumnList { get; set; }

        public List<LRMaster> LRMasters { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        
        public List<SelectListItem> Branches { get; set; }
        public List<SelectListItem> FromBranches { get; set; }
        public List<SelectListItem> BranchesOnly { get; set; }
        public List<SelectListItem> FromBranchesOnly { get; set; }
        public List<SelectListItem> StockBranchesOnly { get; set; }
        public List<SelectListItem> StockTypes { get; set; }

        public string StockType { get; set; }
        public string StockTypeL { get; set; }

        public string StockBranch { get; set; }
        public string StockBranchL { get; set; }

        public string FromBranch { get; set; }
        public string FromBranchL { get; set; }
        public string ToBranch { get; set; }
        public string ToBranchL { get; set; }
        public string FromDestination { get; set; }
        public string FromDestinationL { get; set; }
        public string ToDestination { get; set; }
        public string ViewCode { get;  set; }
        public string ToDestinationL { get; set; }
        public string BillingBranch { get; set; }
        public string BillingBranchL { get; set; }

        public List<SelectListItem> LrTypes { get; set; }
        public string LrType { get; set; }
        public string LrTypeL { get; set; }

        public List<SelectListItem> Consignors { get; set; }
        public string Consignor { get; set; }
        public string ConsignorL { get; set; }
        public string Consignee { get; set; }
        public string ConsigneeL { get; set; }

        public List<SelectListItem> BillingPartys { get; set; }
        public string BillingParty { get; set; }
        public string BillingPartyL { get; set; }

        public List<SelectListItem> ChargeTypes { get; set; }
        public string ChargeType { get; set; }
        public string ChargeTypeL { get; set; }

        public List<SelectListItem> Particulars { get; set; }
        public string Particular { get; set; }
        public string ParticularL { get; set; }

        public List<SelectListItem> Units { get; set; }
        public string Unit { get; set; }
        public string UnitL { get; set; }

        public List<SelectListItem> ReportsType { get; set; }
        public string ReportTypeL { get; set; }

        public List<SelectListItem> Collections { get; set; }
        public string Collection { get; set; }
        public string CollectionL { get; set; }

        public List<SelectListItem> Delivereries { get; set; }
        public string Delivery { get; set; }
        public string DeliveryL { get; set; }

        public List<SelectListItem> DateRanges { get; set; }
        public string DateType { get; set; }

        public string LrGenetate { get; set; }
        public string LrMode { get; set; }

        public string ConsignmentNo { get; set; }
        public bool OnlySummary { get; set; }
        public bool Summary { get; set; }
        public bool MonthlyReport { get; set; }
        public bool NoOfLR { get; set; }
        public bool ChargeShow { get; set; }
        public bool BillChargeShow { get; set; }
        public bool BillRelationDetails { get; set; }
        public bool DispatchDetails { get; set; }
        public bool DeliveryDetails { get; set; }
        public bool ExpensesDetails { get; set; }
        public bool TripDetails { get; set; }
        public bool AdvBalDetails { get; set; }
        public bool PLAccount { get; set; }
        public bool SkipDuplicateFM { get; set; }

        public string DeliverdConsignmentStatus { get; set; }


        public int page { get; set; }
        public int rows { get; set; }
        public string searchField { get; set; }
        public string searchOper { get; set; }
        public string searchString { get; set; }
        public string sidx { get; set; }
        public string sord { get; set; }
        public string mWhat { get; set; }

        public List<string> AllHeaderList { get; set; }
        public List<string> AllFMPHeaderList { get; set; }
        public List<string> AllFMHeaderList { get; set; }


        // iX9: used while passing parameters from MasterGrid to Option and Back 
        public string MainType { get; set; }
        public string Controller2 { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string Mode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
        public string Document { get;  set; }
        public dynamic Controller { get; internal set; }
        public string Date { get; internal set; }
    }
}