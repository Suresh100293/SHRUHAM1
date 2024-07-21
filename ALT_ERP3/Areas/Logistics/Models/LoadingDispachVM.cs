using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Vehicles.Models;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class LoadingDispachVM
    {

        #region Attachment

        public LoadingDispachAttachment Attachment { get; set; }
        public List<LoadingDispachAttachment> attachments { get; set; }
        #endregion

        #region Lock System Properties
        public bool Dispach { get; set; }
        public bool Delivery { get; set; }
        public bool Billing { get; set; }
        public bool Payment { get; set; }
        public bool All { get; set; }
        public string LockRemark { get; set; }
        public string Note { get; set; }

        #endregion

        #region Temporary Variable

        public List<LR_LC_Combine_VM> AllDest { get; set; }
        public string DestCombo { get; set; }
        public string DestCombo_Name { get; set; }
        public List<LCModal> LClist { get; set; }
        public string PayLoad { get; set; }
        public string Loaded { get; set; }
        public string AvailablePayload { get; set; }
        public string CurrentLoad { get; set; }
        public string OverLoadLoad { get; set; }
        public string DispatchDate { get; set; }
        public string DispatchTime { get; set; }
        public string DispachKM { get; set; }
        public string DispachRemark { get; set; }
        public double TotalQtyForUnloading { get; set; }
        public double TotalWeightForUnloading { get; set; }


        public string ScriptDeliverylist { get; set; }
        public string ScriptLRlist { get; set; }
        public string ScriptQtylist { get; set; }
        public string AllocatedWeight { get; set; }
        public string WhatWeDo { get; set; }

        //Clear Flag
        public bool Clear { get; set; }
        public bool UpdateFmStatus { get; set; }
        public VehicleReportingst vehicleStatus { get; set; }

        public string PayLoadL { get; set; }
        public string AppBranch { get; set; }
        public List<SelectListItem> Branches { get; set; }
        #endregion

        #region use Variable

        public string FMNO { get; set; }
        public string LcNo { get; set; }
        public string FM_Date { get; set; }
        public string FM_Time { get; set; }
        //public TruckStatus TruckStatus { get; set; }
        public string VehicleCategory { get; set; }
        public string VehicleCategory_Name { get; set; }
        public string VehicleGroup { get; set; }
        public string VehicleGroup_Name { get; set; }
        //public string PayLoad { get; set; }
        public int ReceiptNo { get; set; }
        public string VehicleNo { get; set; }
        public string VehicleNo_Name { get; set; }
        public decimal KM { get; set; }
        public string From { get; set; }
        public string From_Name { get; set; }
        public string To { get; set; }
        public string To_Name { get; set; }
        public string AddDestination { get; set; }
        public string AddDestination_Name { get; set; }
        public string DriverName { get; set; }
        public string LicenceNo { get; set; }
        public string LicenceExpDate { get; set; }
        public string Owner { get; set; }
        public int ChallanNo { get; set; }
        public string Broker { get; set; }
        public string Broker_Name { get; set; }
        public int ContactNo { get; set; }
        public decimal Freight { get; set; }
        public decimal Advance { get; set; }
        public string PayableAt { get; set; }
        public string PayableAt_Name { get; set; }

        public string SC_Date { get; set; }
        public string SC_Time { get; set; }
        public string SC_KM { get; set; }

        public decimal Balance { get; set; }
        public decimal PaidAdvance { get; set; }

        public string Remark { get; set; }

        public string LC_Date { get; set; }
        public string LC_Time { get; set; }
        public string LC_From { get; set; }
        public string LC_To { get; set; }



        public string ArrivalDate { get; set; }
        public string ArrivalTime { get; set; }
        public string ArrivalKM { get; set; }
        public string ArrivalRemark { get; set; }

        #endregion

        // iX9: Common default Fields
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


        public List<LCDetail> lCDetails { get; set; }
        public List<LRModal> LRLIst { get; set; }
        public string DeleteUnloadLR { get; set; }

        public List<SelectListItem> DelStatuss { get; set; }
        public List<VehicleDelivery> vehicleDeliveries { get; set; }

    }
    public class LRModal
    {
        public string OTHBranch { get; set; }
        public string OTHBranchName { get; set; }

        public string recordkey { get; set; }
        public string Lcno { get; set; }
        public string LcFrom { get; set; }
        public string LCTo { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public int    Lrno { get; set; }
        public string From { get; set; }
        public string From_Name { get; set; }
        public string To { get; set; }
        public string To_Name { get; set; }
        public string Consignor { get; set; }
        public string Consignee { get; set; }
        public string ChargeType { get; set; }
        public string PorductType { get; set; }
        public string Delivery { get; set; }
        public string Godown { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }

        public int Qty { get; set; }
        public double Weight { get; set; }

        public int AllowQty { get; set; }

        public int unloadGQty { get; set; }
        public int unloadDQty { get; set; }
        public int loadGQty { get; set; }
        public int loadDQty { get; set; }
        public double UnWeight { get; set; }
        public bool UnOtherBranch { get; set; }
        //public string UnloadLRDate { get; set; }
        //public string UnloadLRTime { get; set; }
        //public string LoadDate { get; set; }
        //public string LoadTime { get; set; }
    }
    public class LCModal
    {
        public string Date { get; set; }
        public string Branch { get; set; }
        public string Time { get; set; }
        public string lcno { get; set; }
        public double TotalQty { get; set; }
        public double LoadQty { get; set; }
        public string From { get; set; }
        public string From_Name { get; set; }
        public string To { get; set; }
        public string To_Name { get; set; }
        public double Weight { get; set; }
        public string Remark { get; set; }
        public string LoadDate { get; set; }
        public string LoadTime { get; set; }
        public string UnLoadDate { get; set; }
        public string UnLoadTime { get; set; }
        public bool EnableDeleteOrNot { get; set; }
        public bool IMG { get; set; }
        public bool LRAlertNote { get; set; }
        public string Authorise { get; set; }
        public string Tablekey { get; set; }
        public List<LRModal> LrListOfLC { get; set; }
    }
    public class LoadConsignment
    {
        public string Parentkey { get; set; }
        public string LRNO { get; set; }
        public string BookDate { get; set; }
        public string Type { get; set; }
        public decimal ActWT { get; set; }
        public decimal ChgWT { get; set; }
        public string Unit { get; set; }
        public string ChargeType { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Delivery { get; set; }
        public string Collection { get; set; }
        public string Consignor { get; set; }
        public string Consignee { get; set; }
        public string LRType { get; set; }
        public string LRMode { get; set; }

        public int AvailableQty { get; set; }
        public decimal AvailableWeight { get; set; }
        public int LoadQty { get; set; }
        public decimal LoadWeight { get; set; }

        public string ConsignmentDest { get; set; }
    }
    public class LoadingDispachAttachment
    {
        public HttpPostedFileBase UploadFile { get; set; }
        public byte[] Image { get; set; }
        public string FileName { get; set; }
        public string DocumentString { get; set; }
        public string ContentType { get; set; }
        public string AttachFMNo { get; set; }
        public string typeofattachment { get; set; }
    }

    public class VehicleDelivery
    {
        //Lorry Receipt Fields
        public string TableKey { get; set; }
        public int Lrno { get; set; }
        public string LRDate { get; set; }
        public int Qty { get; set; }
        public double Weight { get; set; }
        public string Consignor { get; set; }
        public string Consignee { get; set; }
        public string From { get; set; }
        public string To { get; set; }

        //Lorry Challan Fields
        public string Lcno { get; set; }
        public string LcFrom { get; set; }
        public string LCTo { get; set; }

        //Delivery Field
        public string DeliveryNo { get; set; }
        public string DelTableyKey { get; set; }
        public string DelDate { get; set; }
        public int ShortQty { get; set; }
        public int DeliveredQty { get; set; }
        public string DelVehicleNo { get; set; }
        public string DelStatus { get; set; }
        public string DelNarr { get; set; }

    }

}