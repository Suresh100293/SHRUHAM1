﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Vehicles.Models
{
    public class TyreStockReportVM
    {
        public bool GetReportParameter { get; set; }
        public string ReportName { get; set; }
        public string ReportTypeL { get; set; }
        public List<SelectListItem> SaveReportList { get; set; }
        public string HideColumnList { get; set; }

        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Date { get; set; }

        public List<SelectListItem> StockList { get; set; }
        public string StockAt { get; set; }
        public string StockAtL { get; set; }


        public List<SelectListItem> StockSerialList { get; set; }
        public string StockSerial { get; set; }
        public string StockSerialL { get; set; }

        public List<SelectListItem> VehicleList { get; set; }
        public string Vehicle { get; set; }
        public string VehicleL { get; set; }

        public bool HistoryVehicleWise { get; set; }
        public bool History { get; set; }

        public int page { get; set; }
        public int rows { get; set; }
        public string searchField { get; set; }
        public string searchOper { get; set; }
        public string searchString { get; set; }
        public string sidx { get; set; }
        public string sord { get; set; }
        public string mWhat { get; set; }


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
        public string ViewCode { get; set; }
        public string Mode { get; set; }
    }
}