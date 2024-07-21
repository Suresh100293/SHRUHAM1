using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Accounts.Models;
using EntitiModel;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class FMVM
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public decimal DieselAmt { get; set; }
        public string DieselLtr { get; set; }

        public int Tempid { get; set; }
        public bool LedgerThrough { get; set; }
        public bool AllowToChangeVehicleCharges { get; set; }
        public bool AllowToChangeDriverCharges { get; set; }

        public List<GridOption> PrintGridList { get; set; }

        public string Sno { get; set; }
        public string ChequeNo { get; set; }
        public string RouteBranch { get; set; }
        public string CurrName { get; set; }
        public string BillNumber { get; set; }
        public bool BlockFM { get; set; }
        public bool LockAuthorise { get; set; }
        public string BlockFMMessage { get; set; }


        public bool PeriodLock { get; set; }

        public List<FmCatchChargesInfo> fmCatchCharges { get; set; }
        public List<FMAttachment> attachments { get; set; }
        public FMAttachment Attachment { get; set; }
        public DifferenceFmParameters differenceFm { get; set; }
        public List<PurchaseVM> Charges { get; set; }
        //public List<PurchaseVM> LedgerPosting { get; set; }

        public bool getRecentFM { get; set; }
        public string FMGenerate { get; set; }
        public string Document { get; set; }
        public string Branch { get; set; }
        public string BranchN { get; set; }
        public string FMNO { get; set; }
        public string VehicleCategory { get; set; }
        public string VehicleNo { get; set; }
        public string VehicleNoName { get; set; }
        public string FM_Date { get; set; }
        public string FM_Time { get; set; }
        public double KM { get; set; }
        public DateTime DocDate { get; set; }
        public List<SelectListItem> Branches { get; set; }
        public string VehicleCategory_Name { get; set; }
        public string VehicleGroup { get; set; }
        public string VehicleGroup_Name { get; set; }
        public string Broker { get; set; }
        public string Broker_Name { get; set; }
        public string From { get; set; }
        public string From_Name { get; set; }
        public string To { get; set; }
        public string To_Name { get; set; }
        public string PayLoad { get; set; }
        public string ReceiptNo { get; set; }
        
        public string LicenceNo { get; set; }
        public string LicenceExpDate { get; set; }
        public string Owner { get; set; }
        public string ChallanNo { get; set; }
        public LogisticsFlow LogisticsFlow { get; set; }
        public bool VehicleRestrict { get; set; }
        public FMSetup FMSetup { get; set; }
        //public HireFMSetup HireFMSetup { get; set; }
        public string Mode { get; set; }
        public string Header { get; set; }
        public string ContactNo { get; set; }
        public decimal Freight { get; set; }
        public decimal Advance { get; set; }
        public decimal TripFreight { get; set; }
        public decimal TripAdvance { get; set; }
        public string PayableAt { get; set; }
        public string PayableAt_Name { get; set; }
        public string Remark { get; set; }
        public decimal Balance { get; set; }
        public string AppBranch { get; set; }
        public List<RouteDetails> ViewSchedule { get; set; }
        public List<LR_LC_Combine_VM> AllDest { get; set; }

        public bool ScheduleRequired { get; set; }
        public string ArrivalDate { get; set; }
        public string ArrivalTime { get; set; }
        public string ArrivalKM { get; set; }
        public string DispatchDate { get; set; }
        public string DispatchTime { get; set; }
        public string DispatchKM { get; set; }
        public string ArrivalRemark { get; set; }


        public List<LCModal> LClist { get; set; }
        public List<LCModal> UnLClist { get; set; }
        public string DestCombo_Sequence { get; set; }
        public string DestCombo { get; set; }
        public string DestCombo_Name { get; set; }
        public string Draft_Name { get; set; }
        public bool Payment { get; set; }
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
        public List<RouteDetails> Allroutelist { get; set; }
        public List<RouteDetails> AllroutelistChange { get; set; }

        public string Loaded { get; set; }
        public string AvailablePayload { get; set; }
        public string OverLoadLoad { get; set; }
        public string Note { get; set; }
        public bool Draft { get; set; }
        public string PayLoadL { get; set; }

        public string DriverName { get; set; }
        public string DriverCode { get; set; }
        public string DriverNCombo { get; set; }
        public bool CheckMode { get; set; }
        public string Message { get; set; }


        public bool MaintainDriAc { get; set; }
        public bool CatchFreight { get; set; }
        public bool FMVouRelReq { get; set; }
        public bool FMPosting { get; set; }



        public decimal TotAdvExp { get; set; }
        public decimal TotExp { get; set; }
        public decimal TDSAmt { get; set; }
        public decimal NewBal { get; set; }



        public List<PurchaseVM> LedgerPostList { get; set; }

        public decimal TotDebit { get; set; }
        public decimal TotCredit { get; set; }
        public bool CutTDS { get; set; }
        public decimal TDSRate { get; set; }
        public int TDSCode { get; set; }
        public bool AllowToChangeTDS { get; set; }

        public string GrandActivity { get; set; }
        public int GrandKM { get; set; }
        public string GrandTime { get; set; }
        public List<ScheduleCalculator> scheduleCalculators { get; set; }

        //public bool HireSpclFlag { get; set; }
        //public bool HireBlackListFlag { get; set; }
        //public string HireSpclRemark { get; set; }
        //public string HireBlackListRemark { get; set; }

        //public string SC_Date { get; set; }
        //public string SC_Time { get; set; }
        //public string SC_KM { get; set; }


        //public LoadingDispachVM LoadingDispachVM { get; set; }

    }


    public class FMAttachment
    {
        public HttpPostedFileBase UploadFile { get; set; }
        public byte[] Image { get; set; }
        public string FileName { get; set; }
        public string DocumentString { get; set; }
        public string ContentType { get; set; }
        public string AttachFMNo { get; set; }
        public string TypeOFAttachment { get; set; }
    }
    public class LR_LC_Combine_VM
    {
        public string Branch { get; set; }
        public string LCNo { get; set; }
        public string LRNo { get; set; }
        public string Quantity { get; set; }
        public string Box_Bag { get; set; }
        public decimal ActWeight { get; set; }
        public string ChgWeight { get; set; }
        public string Date { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Consigner { get; set; }
        public string Consignee { get; set; }
        public string Remark { get; set; }
        public bool Flag { get; set; }
        public List<LR_LC_Combine_VM> LoadInfo { get; set; }
        public List<LR_LC_Combine_VM> UnloadInfo { get; set; }
    }

    public class RouteDetails
    {
        public int Tempid { get; set; }
        public string Branch { get; set; }
        public string BranchN { get; set; }
        public string AllowToChange { get; set; }
        public int SequenceRoute { get; set; }

        public string ArrivalSchDate { get; set; }
        public string ArrivalSchTime { get; set; }
        public string ArrivalSchKm { get; set; }
        public string ArrivalReSchDate { get; set; }
        public string ArrivalReSchTime { get; set; }
        public string ArrivalReSchKm { get; set; }


        public string ArrivalDate { get; set; }
        public string ArrivalTime { get; set; }
        public string ArrivalKM { get; set; }

        public string ArrivalLateTime { get; set; }
        public string LateKM { get; set; }


        public string DispatchSchDate { get; set; }
        public string DispatchSchTime { get; set; }
        public string DispatchReSchDate { get; set; }
        public string DispatchReSchTime { get; set; }

        public string DispatchLateTime { get; set; }

        public string DispatchDate { get; set; }
        public string DispatchTime { get; set; }
        public string DispatchKM { get; set; }
        public string ActivityTime { get; set; }
        
        

        public List<LCModal> lCModals { get; set; }
        
    }

    public class DifferenceFmParameters
    {
        public bool OldFmPosting { get; set; }
        public bool NewFmPosting { get; set; }
        public bool OldMaintainDriverAc { get; set; }
        public bool NewMaintainDriverAc { get; set; }
        public bool OldMaintainCreditorPayRecord { get; set; }
        public bool NewMaintainCreditorPayRecord { get; set; }
        public string OldVehiclePosting { get; set; }
        public string OldVehiclePostingN { get; set; }
        public string NewVehiclePosting { get; set; }
        public string NEwVehiclePostingN { get; set; }
        public string OldDriverPosting { get; set; }
        public string OldDriverPostingN { get; set; }
        public string NewDriverPosting { get; set; }
        public string NEwDriverPostingN { get; set; }

    }
    
    public class ScheduleCalculator
    {
        
        public string FromName { get; set; }
        public string ToName { get; set; }
        public string ActivityTime { get; set; }
        public int RunningKM { get; set; }
        public string RunningTime { get; set; }
        

    }
    

}