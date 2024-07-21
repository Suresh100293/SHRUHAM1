using EntitiModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class LoadingToDispatchVM
    {
        
        public int NarrSno { get; set; }
        public string NarrStr { get; set; }
        public string FMNO { get; set; }
        public string FM_Date { get; set; }
        public string FM_Time { get; set; }
        public string VehicleGroup { get; set; }
        public string VehicleGroup_Name { get; set; }
        public string VehicleNo { get; set; }
        public string VehicleNoName { get; set; }

        public string Broker { get; set; }
        public string Broker_Name { get; set; }
        public decimal KM { get; set; }
        public string From { get; set; }
        public string From_Name { get; set; }
        public string To { get; set; }
        public string To_Name { get; set; }
        public string DestCombo { get; set; }
        public string DestCombo_Name { get; set; }
        public string VehicleCategory { get; set; }
        public string VehicleCategory_Name { get; set; }
        public string ReceiptNo { get; set; }
        public string DriverName { get; set; }
        public string LicenceNo { get; set; }
        public string LicenceExpDate { get; set; }
        public string Owner { get; set; }
        public string ChallanNo { get; set; }
        public string ContactNo { get; set; }
        public decimal Freight { get; set; }
        public decimal Advance { get; set; }
        public string PayableAt { get; set; }
        public string PayableAt_Name { get; set; }
        public string Remark { get; set; }
        public string PayLoad { get; set; }
        public string Loaded { get; set; }
        public string AvailablePayload { get; set; }
        public string OverLoadLoad { get; set; }
        public decimal Balance { get; set; }

        public EnumVehicleStatus vehicleStatus { get; set; }

        public LoadingDispachAttachment Attachment { get; set; }
        public List<FMAttachment> attachments { get; set; }
        public List<LR_LC_Combine_VM> AllDestList { get; set; }
        public List<RouteDetails> ViewSchedule { get; set; }
        public LogisticsFlow LogisticsFlow { get; set; }
        public UnLoadSetup UnLoadSetup { get; set; }

        public string PayLoadL { get; set; }
        public string AppBranch { get; set; }
        public List<SelectListItem> Branches { get; set; }

        public bool MaterialAvailableForLoading { get; set; }
        public bool MaterialAvailableForUnLoading { get; set; }
        public bool LastRoute { get; set; }

        public List<VehicleDelivery> vehicleDeliveries { get; set; }

        #region Arrival-Load-Unload-Dispatch
        public List<ArrivalVM> ArrivalList { get; set; }


        public List<LCModal> ExistLClist { get; set; }
        public List<LCModal> NewLCList { get; set; }
        public List<LRModal> OTHUnloadLrList { get; set; }
        public List<LRModal> UnloadLrList { get; set; }
        public List<LoadConsignment> loadConsignmentList { get; set; }

        public List<DispatchVM> DispatchList { get; set; }
        public List<ConsignmentTracking> consignmentTrackings { get; set; }

        public List<SelectListItem> UnloadBranches { get; set; }
        public string UnloadBranch { get; set; }
        public string UnloadBranchL { get; set; }



        #endregion

        public string Parentkey { get; set; }
        public string Document { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Mode { get; set; }
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

    public class ConsignmentTracking
    {
        
        public string LRRefTablekey { get; set; }
        public string LCRefTablekey { get; set; }
        public string FMRefTablekey { get; set; }
        public string ConsignNo { get; set; }
        public string ConsignDate { get; set; }
        public double ConsignNoQty { get; set; }
        public double ConsignNoWeight { get; set; }
        public string ConsignNoFrom { get; set; }
        public string ConsignNoTo{ get; set; }
        public string ConsignLoadBranch { get; set; }
        public string ConsignLoadDate { get; set; }
        public List<ConsignmentTrackingUnload> trackingUnloads { get; set; }
        public List<ConsignmentTrackingDelivery> trackingDeliveries { get; set; }
        public double ConsignNoBalQty { get; set; }
        public double ConsignNoBalWeight { get; set; }
    }

    public class ConsignmentTrackingUnload
    {
        public string UnloadBranch { get; set; }
        public string UnloadDate { get; set; }
        public double UnloadQty { get; set; }
        public double UnloadWeight { get; set; }
    }

    public class ConsignmentTrackingDelivery
    {
        public string DeliveryBranch { get; set; }
        public string DeliveryDate { get; set; }
        public double DeliveryQty { get; set; }
        public double DeliveryWeight { get; set; }
    }


    public class ArrivalVM
    {
        public string RecordKey { get; set; }
        public string AreaNameOf_A_D { get; set; }
        public string ArrivalDate { get; set; }
        public string ArrivalTime { get; set; }
        public string ArrivalKM { get; set; }
        public string ArrivalRemark { get; set; }
    }
    public class DispatchVM
    {
        public string RecordKey { get; set; }
        public string AreaNameOf_A_D { get; set; }
        public string DispatchDate { get; set; }
        public string DispatchTime { get; set; }
        public string DispachKM { get; set; }
        public string DispachRemark { get; set; }
    }


    public enum EnumVehicleStatus
    {
        [Display(Name = "Ready For FM")]
        Ready = 0,
        [Display(Name = "Under The Maintaince")]
        Maintaince = 1,
        [Display(Name = "Driver Not Available")]
        NODriver = 2,
        [Display(Name = "On The Road")]
        Transit = 3,
    }
}