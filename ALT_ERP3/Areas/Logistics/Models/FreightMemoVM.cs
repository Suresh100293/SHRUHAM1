using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Accounts.Models;
using EntitiModel;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class FreightMemoVM
    {
        public bool getRecentFM { get;  set; }
        public string FMGenerate { get;  set; }
        public string Document { get;  set; }
        public string FMNO { get;  set; }
        public string VehicleCategory { get;  set; }
        public string VehicleNo { get;  set; }
        public string FM_Date { get;  set; }
        public string FM_Time { get;  set; }
        public double KM { get;  set; }
        public DateTime DocDate { get;  set; }
        public List<SelectListItem> Branches { get;  set; }
        public string VehicleCategory_Name { get;  set; }
        public string VehicleGroup { get;  set; }
        public string VehicleGroup_Name { get;  set; }
        public string Broker { get;  set; }
        public string Broker_Name { get;  set; }
        public string From { get;  set; }
        public string From_Name { get;  set; }
        public string To { get;  set; }
        public string To_Name { get;  set; }
        public string PayLoad { get;  set; }
        public string ReceiptNo { get;  set; }
        public string DriverName { get;  set; }
        public string LicenceNo { get;  set; }
        public string LicenceExpDate { get;  set; }
        public string Owner { get;  set; }
        public string ChallanNo { get;  set; }
        public LogisticsFlow LogisticsFlow { get;  set; }
        public bool VehicleRestrict { get;  set; }
        public FMSetup FMSetup { get;  set; }
        public string Mode { get;  set; }
        public string Header { get;  set; }
        public string ContactNo { get;  set; }
        public decimal Freight { get;  set; }
        public decimal Advance { get;  set; }
        public string PayableAt { get;  set; }
        public string PayableAt_Name { get;  set; }
        public string Remark { get;  set; }
        public decimal Balance { get;  set; }
        public string AppBranch { get;  set; }
        public List<RouteDetails> ViewSchedule { get;  set; }
        public List<LR_LC_Combine_VM> AllDest { get;  set; }
        public List<PurchaseVM> Charges { get;  set; }
        public bool ScheduleRequired { get;  set; }
        public string ArrivalDate { get;  set; }
        public string ArrivalTime { get;  set; }
        public string ArrivalKM { get;  set; }
        public string ArrivalRemark { get;  set; }
        public List<LCModal> LClist { get;  set; }
        public List<LCModal> UnLClist { get;  set; }
        public string DestCombo_Sequence { get;  set; }
        public string DestCombo { get;  set; }
        public string Draft_Name { get;  set; }
        public List<PurchaseVM> LedgerPosting { get;  set; }
        public bool Payment { get;  set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        // iX9: used while passing parameters from MasterGrid to Option and Back 
        public string MainType { get; set; }
        public string Controller2 { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
    }
}