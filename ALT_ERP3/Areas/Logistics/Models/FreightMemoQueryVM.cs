using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class FreightMemoQueryVM
    {
        public List<RouteDetails> ViewSchedule { get; set; }
        public List<FreightMemoQueryVM> ValueList { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }

        public bool FindOpening { get; set; }
        public string NarrStr { get; set; }
        public int AttachCount { get; set; }
        public int AlertCount { get; set; }
        public string TrackId { get; set; }
        public bool TrackButtonReq { get; set; }
        public string TrackErrorMsg { get; set; }

        public string FreightMemoKey { get; set; }
        public string FreightMemoParentkey { get; set; }
        public string FMno { get; set; }
        public string FMBranch { get; set; }
        public DateTime FMDate { get; set; }
        public string FMTime { get; set; }
        public DateTime FMCreateDate { get; set; }
        public string FMEnterdby { get; set; }
        public DateTime FMLastUpdateDate { get; set; }
        public string VehicleType { get; set; }
        public string VehicleNo { get; set; }
        public string Broker { get; set; }
        public string KM { get; set; }
        public string FFrom { get; set; }
        public string Via { get; set; }
        public string Route { get; set; }
        public string FTo { get; set; }
        public string Driver { get; set; }
        public string Freight { get; set; }
        public string Advance { get; set; }
        public string Balance { get; set; }
        public string PayableAt { get; set; }
        public string Remark { get; set; }
        public string TotalQtyLoad { get; set; }
        public string TotalQtyWeight { get; set; }
        public string Payload { get; set; }

        public List<FMRelatedDispatchDetailsVM> LorryChallans { get; set; }
        public List<FMRelatedDirectLorryReceiptDetailsVM> DirectLoadingDetails { get; set; }
        public List<FMRelatedPaymentDetailsVM> PaymentDetails { get; set; }
        public List<FMRelatedExpensesDetailsVM> ExpensesDetails { get; set; }

        public bool FMD { get; set; }
        public bool DS { get; set; }
        public bool PD { get; set; }
        public bool ED { get; set; }
        public bool NotShowDetails { get; set; }




        // iX9: Common default Fields
        public string Prefix { get; set; }
        public string Document { get; set; }
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
        public string Code { get; set; }

        public bool Shortcut { get; set; }
    }
    public class FMRelatedDispatchDetailsVM
    {
        public string LCno { get; set; }
        public string LCBranch { get; set; }
        public string LCFrom { get; set; }
        public string LCTo { get; set; }
        public string LCQty { get; set; }
        public DateTime LCDate { get; set; }
        public string LCTime { get; set; }
        public DateTime LCCreateDate { get; set; }
        public string LCEnteredBy { get; set; }
        public DateTime LCLastUpdateDate { get; set; }
        public string LCRemark { get; set; }

        public List<FMRelatedLorryReceiptDetailsVM> LorryReceipts { get; set; }

        public string TotalQtyLoad { get; set; }
        public string TotalQtyWeight { get; set; }
    }
    public class FMRelatedLorryReceiptDetailsVM
    {
        public int Serial { get; set; }
        public string Lrno { get; set; }
        public DateTime BookDate { get; set; }
        public string LRBranch { get; set; }
        public string LRType { get; set; }
        public string Consignor { get; set; }
        public string Consignee { get; set; }
        public string LRFrom { get; set; }
        public string LRTo { get; set; }
        public string LRQty { get; set; }
        public string Unit { get; set; }
        public string ActWt { get; set; }
        public string ChrgWt { get; set; }
        public string ChrgType { get; set; }
    }
    public class FMRelatedDirectLorryReceiptDetailsVM
    {
        public int Serial { get; set; }
        public DateTime LodeDate { get; set; }
        public string LoadingIn { get; set; }
        public string LoadingFor { get; set; }
        public string LoadQty { get; set; }
        public string LodedBy { get; set; }
        public string Lrno { get; set; }
        public DateTime BookDate { get; set; }
        public string LRBranch { get; set; }
        public string LRType { get; set; }
        public string Consignor { get; set; }
        public string Consignee { get; set; }
        public string LRFrom { get; set; }
        public string LRTo { get; set; }
        public string LRQty { get; set; }
        public string Unit { get; set; }
        public string ActWt { get; set; }
        public string ChrgWt { get; set; }
        public string ChrgType { get; set; }
    }
    public class FMRelatedPaymentDetailsVM
    {
        public string VouNO { get; set; }
        public DateTime VouNODate { get; set; }
        public DateTime VouNOCreateDate { get; set; }
        public string VouNOBranch { get; set; }

        public string VouNOBoker { get; set; }
        public string VouNOBank { get; set; }
        public string Amount { get; set; }
        public string AB { get; set; }

        public string VouNOEnteredBy { get; set; }
        public DateTime VouNOLastUpdateDate { get; set; }
        public string VouNORemark { get; set; }
        public List<AdvancePayModel> Charges { get; set; }
    }
    public class FMRelatedExpensesDetailsVM
    {
        public string VouNO { get; set; }
        public DateTime VouNODate { get; set; }
        public DateTime VouNOCreateDate { get; set; }
        public string VouNOBranch { get; set; }
        public string VouNOEnteredBy { get; set; }
        public DateTime VouNOLastUpdateDate { get; set; }
        public string VouNORemark { get; set; }
        public string VouNOTrnType { get; set; }
        public string VouNOTrnTypeCode { get; set; }
        public string VouNOTrnIncAmt { get; set; }
        public string VouNOTrnExpAmt { get; set; }
        public string VouNOTrnChargeName { get; set; }
    }
}