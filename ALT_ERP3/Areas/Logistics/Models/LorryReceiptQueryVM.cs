using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class LorryReceiptQueryVM
    {
        public int AttachCount { get; set; }
        public int AlertCount { get; set; }

        public List<LorryReceiptQueryVM> ValueList { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }


        public string ConsignmentKey { get; set; }
        public string Lrno { get; set; }
        public string LRBranch { get; set; }
        public DateTime LrDate { get; set; }
        public string LrTime { get; set; }
        public DateTime LrCreateDate { get; set; }
        public string LrEnterdby { get; set; }
        public DateTime LRLastUpdateDate { get; set; }
        public string LrFrom { get; set; }
        public string LrTo { get; set; }
        public string LrCollection { get; set; }
        public string LrDelivery { get; set; }
        public string LrMode { get; set; }
        public string LrServiceType { get; set; }
        public string LrConsignor { get; set; }
        public string LrConsignee { get; set; }
        public string LrType { get; set; }
        public string LrBillParty { get; set; }
        public string LrBillBranch { get; set; }
        public string LrQty { get; set; }
        public string LrUnit { get; set; }
        public string LrActWt { get; set; }
        public string LrChrgWt { get; set; }
        public string LrChargeType { get; set; }
        public string LrDescription { get; set; }
        public string LrPartyChallan { get; set; }
        public string LrPartyInvoice { get; set; }
        public string LrPONumber { get; set; }
        public string LrBENumber { get; set; }
        public string LrDeclareValue { get; set; }
        public string LrGST { get; set; }
        public string LrEwayBill { get; set; }
        public string LrRemark { get; set; }
        public string VehicleNo { get; set; }

        public List<LRRelatedStockDetailsVM> StockDetails { get; set; }
        public List<LRRelatedDispatchDetailsVM> DispatchDetails { get; set; }
        public List<LRRelatedDispatchDetailsVM> DirectLoadingDetails { get; set; }
        public List<LRRelatedDeliveryDetailsVM> DeliveryDetails { get; set; }
        public List<LRRelatedPODDetailsVM> PODDetails { get; set; }
        public List<LRRelatedInvoiceDetailsVM> BillDetails { get; set; }
        public List<LRRelatedInvoiceDetailsVM> MemoDetails { get; set; }
        public List<LRRelatedExpensesDetailsVM> ExpensesDetails { get; set; }

        public bool CD { get; set; }
        public bool CS { get; set; }
        public bool DS { get; set; }
        public bool DD { get; set; }
        public bool PD { get; set; }
        public bool BD { get; set; }
        public bool CSD { get; set; }
        public bool ED { get; set; }
        public bool NotShowDetails { get; set; }


        // iX9: Common default Fields
        public string Document { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Mode { get; set; }

        // iX9: used while passing parameters from MasterGrid to Option and Back 
        public string Prefix { get; set; }
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
    public class LRRelatedDispatchDetailsVM
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

        public string FMno { get; set; }
        public DateTime FMDate { get; set; }
        public string FMTime { get; set; }
        public string FMBranch { get; set; }
        public string FMFrom { get; set; }
        public string FMTo { get; set; }
        public string FMVia { get; set; }
        public DateTime FMCreateDate { get; set; }
        public string FMEnterdBy { get; set; }
        public DateTime FMLastUpdateDate { get; set; }
        public string FMType { get; set; }
        public string FMVehicleNO { get; set; }
        public string FMBroker { get; set; }
        public string FMFreight { get; set; }
        public string FMAdvance { get; set; }
        public string FMBalance { get; set; }
        public string FMPaybleAt { get; set; }
        public string FMRemark { get; set; }
        public string FMDriver { get; set; }
    }
    public class LRRelatedDeliveryDetailsVM
    {
        public string Delno { get; set; }
        public string DelBranch { get; set; }
        public DateTime DelDate { get; set; }
        public string DelTime { get; set; }
        public DateTime DelCreateDate { get; set; }
        public string DelEnteredBy { get; set; }
        public DateTime DelLastUpdateDate { get; set; }
        public string DelQty { get; set; }
        public string ShortQty { get; set; }
        public string DelStatus { get; set; }
        public string DelRemark { get; set; }
    }
    public class LRRelatedPODDetailsVM
    {
        public string PODno { get; set; }
        public DateTime PODDate { get; set; }
        public string PODTime { get; set; }
        public DateTime PODCreateDate { get; set; }
        public string PODMOduleName { get; set; }
        public string PODTask { get; set; }
        public string PODBranch { get; set; }
        public string PODSendToBranch { get; set; }
        public string PODReceiverdFromBranch { get; set; }
        public string PODEnterdeby { get; set; }
        public string PODSendParticular { get; set; }
        public DateTime PODLastUpdateDate { get; set; }
        public string PODRemark { get; set; }
    }
    public class LRRelatedInvoiceDetailsVM
    {
        public string BillNo { get; set; }
        public DateTime BillNoDate { get; set; }
        public DateTime BillNoCreateDate { get; set; }
        public string BillNoBranch { get; set; }
        public string BillNoEnteredBy { get; set; }
        public DateTime BillNoLastUpdateDate { get; set; }
        public string BillNoRemark { get; set; }
        public List<LRInvoiceVM> Charges { get; set; }
        public string BillTotal { get; set; }
        public string BillTablekey { get; set; }
        public string BillParty { get; set; }
    }
    public class LRRelatedExpensesDetailsVM
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
    public class LRRelatedStockDetailsVM
    {
        public string StkBranch { get; set; }
        public string FMNO { get; set; }
        public string Tablekey { get; set; }
        public string Type { get; set; }
        public string VehicleNo { get; set; }
        public string Qty { get; set; }

    }
}