using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3
{
    public partial class StockVM
    {
        public int RECORDKEY { get; set; }
        public decimal AddTax { get; set; }
        public decimal Amt { get; set; }
        public decimal aSurcharge { get; set; }
        public bool Audited { get; set; }
        public string AuthIds { get; set; }
        public string Authorise { get; set; }
        public decimal BCD { get; set; }
        public decimal BCDAmt { get; set; }
        public System.DateTime BillDate { get; set; }
        public string BillNumber { get; set; }
        public int BINNumber { get; set; }
        public string BOMSrl { get; set; }
        public string Branch { get; set; }
        public decimal Cess { get; set; }
        public System.DateTime ChlnDate { get; set; }
        public string ChlnNumber { get; set; }
        public string Code { get; set; }
        public decimal cRate { get; set; }
        public string CurrName { get; set; }
        public double CurrRate { get; set; }
        public decimal Disc { get; set; }
        public decimal DiscAmt { get; set; }
        public decimal Discount { get; set; }
        public System.DateTime DocDate { get; set; }
        public bool Excisable { get; set; }
        public decimal ExciseAs { get; set; }
        public string ExcUnits { get; set; }
        public double Factor { get; set; }
        public bool FirstSno { get; set; }
        public bool FreeQty { get; set; }
        public bool Fvalue { get; set; }
        public string GINNumber { get; set; }
        public string IndNumber { get; set; }
        public bool IsChargeable { get; set; }
        public bool IsReturnable { get; set; }
        public string IssueNo { get; set; }
        public string ItemSerial { get; set; }
        public int LocationCode { get; set; }
        public string MainType { get; set; }
        public string MenuCode { get; set; }
        public string Narr { get; set; }
        public decimal NewRate { get; set; }
        public decimal NewRateEntry { get; set; }
        public decimal NewRateLink { get; set; }
        public bool NotInStock { get; set; }
        public System.DateTime OrdDate { get; set; }
        public string OrdNumber { get; set; }
        public string Party { get; set; }
        public decimal Pending { get; set; }
        public decimal Pending2 { get; set; }
        public string PerUnit { get; set; }
        public decimal pQty { get; set; }
        public string Prefix { get; set; }
        public string ProcessCode { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectStage { get; set; }
        public string ProjectUnit { get; set; }
        public bool QCDone { get; set; }
        public bool QCIssued { get; set; }
        public decimal QCQty { get; set; }
        public bool QCRequire { get; set; }
        public double Qty { get; set; }
        public double Qty2 { get; set; }
        public double Rate { get; set; }
        public decimal RateOn { get; set; }
        public decimal RatePer { get; set; }
        public int ReasonCode { get; set; }
        public string RouteCode { get; set; }
        public decimal SHECess { get; set; }
        public decimal SHECessAmt { get; set; }
        public string Sno { get; set; }
        public string Srl { get; set; }
        public long Stage { get; set; }
        public decimal STAmt { get; set; }
        public decimal Staxable { get; set; }
        public decimal STCess { get; set; }
        public string STCode { get; set; }
        public int Store { get; set; }
        public decimal STSheCess { get; set; }
        public string SubType { get; set; }
        public bool SuppInvoice { get; set; }
        public decimal SurCharge { get; set; }
        public decimal Taxable { get; set; }
        public decimal TaxAmt { get; set; }
        public string TaxCode { get; set; }
        public string Type { get; set; }
        public string Unit { get; set; }
        public string Unit2 { get; set; }
        public string WasteFlag { get; set; }
        public decimal Weightage { get; set; }
        public string WONumber { get; set; }
        public string WorkOrder { get; set; }
        public string EnteredBy { get; set; }
        public System.DateTime LastUpdateDate { get; set; }
        public string ExciseSeries { get; set; }
        public decimal CurrAmount { get; set; }
        public double CurrRateI { get; set; }
        public string Part1 { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string AccountName { get; set; }
        public string Date { get; set; }
        public decimal Balance { get; set; }
        public decimal CloseQuantity { get; set; }
        public List<dynamic> headerName = new List<dynamic>();
        public List<StockVM> ValueList = new List<StockVM>();
        public string Name { get; set; }

        public double AvailStock { get; set; }
        public string ProductName { get; set; }
        public int FromStore { get; set; }
        public int ToStore { get; set; }
        public double Value { get; set; }
        public int SrNo { get; set; }
        public int tEmpID { get; set; }
        public bool tempIsDeleted { get; set; }
        public bool tempIsAdded { get; set; }
        public bool ExistData { get; set; }
        public int LocationCode2 { get; set; }
        public int BINNumber2 { get; set; }
        public string ChangeLog { get; set; }

        public int count { get; set; }
        public int Snocount { get; set; }
        public string Document { get; set; }
        public bool IsItemBatchYes { get; set; }

        public string id { get; set; }
        public string Mode { get; set; }
        //public List<WorkOrderRouteVM> List = new List<WorkOrderRouteVM>();
        public string Tool { get; set; }
        public string PMachine { get; set; }
        public string RStore { get; set; }
        public string BOMType { get; set; }
        public string WorkShift { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string ShiftTiming { get; set; }
        public string TotalMCHour { get; set; }
        public string MCRate { get; set; }
        public string MCCharge { get; set; }
        public string TotalManHour { get; set; }
        public string ManRate { get; set; }
        public string ManCharge { get; set; }
    }
}