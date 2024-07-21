using EntitiModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class EwayBillVM
    {
        public bool GetDate { get; set; }

        public int TotalQty { get; set; }
        public decimal TaxableAmt { get; set; }
        public decimal IGSTAmt { get; set; }
        public decimal CGSTAmt { get; set; }
        public decimal SGSTAmt { get; set; }
        public decimal CessAmt { get; set; }
        public decimal CessAdvolAmt { get; set; }
        public decimal IGSTRate { get; set; }
        public decimal CGSTRate { get; set; }
        public decimal SGSTRate { get; set; }
        public decimal CessRate { get; set; }
        public decimal CessAdvolRate { get; set; }
        public decimal OtherAmt { get; set; }
        public decimal TotalInvAmt { get; set; }

        public string EWB_CGSTSGST { get; set; }
        public string EWB_IGST { get; set; }
        public string EWB_CESS { get; set; }
        public string EWB_CESSAdvol { get; set; }


        public string Document { get; set; }
        public string ConsignemtKey { get; set; }
        public string SearchEway { get; set; }
        public string EWBDocument { get; set; }
        public string EWB_EwayNo { get; set; }
        public string EWB_LastVehicleNo { get; set; }

        public string EWB_SupplyType { get; set; }
        public string EWB_SubSupplyType { get; set; }
        public string EWB_DocType { get; set; }
        public string EWB_TRNType { get; set; }
        public string EWB_TRNMode { get; set; }
        public string EWB_VehicleType { get; set; }
        public string EWB_VehicleNo { get; set; }
        public string EWB_VehicleNoTxt { get; set; }
        public string EWB_VehicleName { get; set; }
        public string EWB_ReasonCode { get; set; }
        public string EWB_ConsignmentCode { get; set; }
        public string EWB_ReasonRemark { get; set; }
        public string EWB_ExtendReasonCode { get; set; }
        public string EWB_ExtendReasonRemark { get; set; }
        public string EWB_CancelReasonCode { get; set; }
        public string EWB_CancelReasonRemark { get; set; }

        public string EWB_SubSupplyDesc { get; set; }
        public string EWB_hsnCode { get; set; }
        public string EWB_Distance { get; set; }
        public string EWB_productName { get; set; }


        //public string EWB_ConsignorCode { get; set; }
        public string EWB_ConsignorName { get; set; }
        public string EWB_ConsignorAddr1 { get; set; }
        public string EWB_ConsignorAddr2 { get; set; }
        public string EWB_ConsignorAddr3 { get; set; }//New Property Create Only Use In Extend Automatic
        public string EWB_ConsignorGST { get; set; }
        public string EWB_ConsignorState { get; set; }
        public string EWB_ConsignorStateName { get; set; }
        public string EWB_ConsignorPincode { get; set; }
        //public string EWB_ConsignerAddNo { get; set; }
        //public string EWB_ConsignerAddNoName { get; set; }

        //public string EWB_ConsigneeCode { get; set; }
        public string EWB_ConsigneeName { get; set; }
        public string EWB_ConsigneeAddr1 { get; set; }
        public string EWB_ConsigneeAddr2 { get; set; }
        public string EWB_ConsigneeGST { get; set; }
        public string EWB_ConsigneeState { get; set; }
        public string EWB_ConsigneeStateName { get; set; }
        public string EWB_ConsigneePincode { get; set; }
        //public string EWB_ConsigneeAddNo { get; set; }
        //public string EWB_ConsigneeAddNoName { get; set; }

        //Required Bpart Properties
        public string EWB_Branch { get; set; }
        public string EWB_BranchName { get; set; }


        public string EWB_FromPlace { get; set; }
        public string EWB_FromPlaceName { get; set; }
        public string EWB_ToPlace { get; set; }
        public string EWB_ToPlaceName { get; set; }

        public string EWB_TotalQty { get; set; }
        public string EWB_VehicleQty { get; set; }

        public List<EwayBillVM> ConsoleList { get; set; }
        public List<UnitList> UnitLists { get; set; }
        public List<VehiclListDetail> VehiclListHistoryDetails { get; set; }
        public List<tfatEWB> ExtendEwayBillList { get; set; }
        public tfatEwaySetup tfatEwaySetup { get; set; }

        //Console Field Below
        public string LREwayNo { get; set; }
        public string LREwayValid { get; set; }
        public string LRno { get; set; }
        public string LRDate { get; set; }
        public string Consingor { get; set; }
        public string Consignee { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string LRrefKey { get; set; }
        



        // iX9: Common default Fields
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Mode { get; set; }
        // iX9: used while passing parameters from MasterGrid to Option and Back 
        public string MainType { get; set; }
        public string Controller2 { get; set; }
        public string Controller { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }

        public string DocDate { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string SyncDate { get; set; }
        public string SyncToDate { get; set; }
        public string UnitCode { get; set; }
        public string UnitName { get; set; }
        public string GetEaybill { get; set; }


        public bool ZoomSideBar { get; set; }

    }


    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

    public class UnitList
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
    public class VehiclListDetail
    {
        public string updMode { get; set; }
        public string vehicleNo { get; set; }
        public string fromPlace { get; set; }
        public int fromState { get; set; }
        public int tripshtNo { get; set; }
        public string userGSTINTransin { get; set; }
        public string enteredDate { get; set; }
        public string transMode { get; set; }
        public string transDocNo { get; set; }
        public string transDocDate { get; set; }
        public string groupNo { get; set; }
    }

}