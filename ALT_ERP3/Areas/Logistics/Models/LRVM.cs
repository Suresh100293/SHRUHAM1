using ALT_ERP3.Areas.Accounts.Models;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class LRVM
    {
        public bool ExtraInfoTab { get; set; }

        public List<LRVM> LRDetailList { get; set; }

        public decimal ActualExp { get; set; }
        public string ApprovedDiesel { get; set; }
        public string PendingDiesel { get; set; }
        public string AdvanceDiesel { get; set; }
        public string TripNarr { get; set; }
        public string TripNo { get; set; }

        public string Status { get; set; }
        public string Message { get; set; }

        public string ExpAcount { get; set; }
        public string ExpAcountName { get; set; }
        public string SubHeadAcc { get; set; }
        public decimal ExpAmount { get; set; }
        public int tempId { get; set; }



        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public string GST_Tickler_DecalreValue { get; set; }
        public string EWB_Tickler_DecalreValue { get; set; }


        public string RecGST { get; set; }
        public string SendGST { get; set; }
        public string BillGST { get; set; }
        public string TrnMode { get; set; }
        public string HSNCODE { get; set; }


        public List<GridOption> PrintGridList { get; set; }
        public List<SelectListItem> ChargeTypeList { get; set; }
        public List<SelectListItem> ChargeOnWeightList { get; set; }


        public string LrContactPerson { get; set; }
        public string LrContactPersonNo { get; set; }
        public string LrContactPersonEmailId { get; set; }

        public decimal DieselAmt { get; set; }
        public string DieselLtr { get; set; }
        public string Driver { get; set; }
        public string DriverN { get; set; }

        public string RateChrgOn { get; set; }
        public Nullable<decimal> Rate { get; set; }
        public string RateType { get; set; }
        public Nullable<decimal> DriverTripExp { get; set; }


        public string HitContractType { get; set; }


        public string Sno { get; set; }
        public string ChequeNo { get; set; }
        public string CurrName { get; set; }
        public string BillNumber { get; set; }
        public List<SelectListItem> RpoertViewData { get; set; }
        public string EmptyViewDataId { get; set; }

        #region Attachment

        public bool PeriodLock { get; set; }
        public AttachmentVM AttachmentVM { get; set; }
        public BroadCastMaster BroadCastMaster { get; set; }

        public LRAttachment Attachment { get; set; }
        public List<LRAttachment> attachments { get; set; }
        #endregion

        #region LrMaster

        public string ConsignerAddNo { get; set; }
        public string ConsigneeAddNo { get; set; }
        public string BillPartyAddNo { get; set; }
        public string ConsignerAddNoName { get; set; }
        public string ConsigneeAddNoName { get; set; }
        public string BillPartyAddNoName { get; set; }
        public string Branch { get; set; }
        public string StockAt { get; set; }
        public string LRMode { get; set; }
        public string Branch_Name { get; set; }
        public string BookDate { get; set; }
        public string OrderReceivedDate { get; set; }
        public string DateOfOrder { get; set; }
        public string ScheduleDate { get; set; }
        public string Time { get; set; }
        public string DocNo { get; set; }
        public int LrNo { get; set; }
        public int TotQty { get; set; }
        public string BillBran { get; set; }
        public string BillBran_Name { get; set; }
        public string BillParty { get; set; }
        public string BillParty_Name { get; set; }
        public string LRtype { get; set; }
        public string LRtype_Name { get; set; }
        public string ServiceType { get; set; }
        public string ServiceType_Name { get; set; }
        public string RecCode { get; set; }
        public string RecCode_Name { get; set; }
        public string SendCode { get; set; }
        public string SendCode_Name { get; set; }
        public string Source { get; set; }
        public string Source_Name { get; set; }
        public string Dest { get; set; }
        public string Dest_Name { get; set; }
        public string PartyRef { get; set; }
        public string PartyInvoice { get; set; }
        public string GSTNO { get; set; }
        public string EwayBill { get; set; }
        public string EwayBillValid { get; set; }
        public string VehicleNo { get; set; }
        public string VehicleNoName { get; set; }
        public double ActWt { get; set; }
        public double ChgWt { get; set; }
        public decimal Amt { get; set; }
        public string Val1 { get; set; }
        public string FldValue { get; set; }
        public string LRRefTableKey { get; set; }

        //public string Srl { get; set; }

        public decimal? DecVal { get; set; }
        public string DescrType { get; set; }
        public string DescrType_Name { get; set; }
        public string DescrTxt { get; set; }
        public string ChgType { get; set; }
        public string ChgType_Name { get; set; }
        public string UnitCode { get; set; }
        public string UnitCode_Name { get; set; }
        public string Colln { get; set; }
        public string CollNArea { get; set; }
        public string CollNArea_Name { get; set; }
        public string CollNArea1 { get; set; }
        public string Delivery { get; set; }
        public string DeliveryAt { get; set; }
        public string DeliveryAt_Name { get; set; }
        public string DeliveryTxt { get; set; }
        public string FormNo { get; set; }
        public string TransactionAt { get; set; }
        public string GstLiable { get; set; }
        public string Narr { get; set; }
        public string Prefix { get; set; }
        public string TrType { get; set; }
        public DateTime DocDate { get; set; }
        public Nullable<System.DateTime> MaxDate { get; set; }
        public Nullable<System.DateTime> MinDate { get; set; }
        public string ConsignerEXTRAInfo { get; set; }
        public string ConsigneeEXTRAInfo { get; set; }
        public string BillingPartyEXTRAInfo { get; set; }
        public int DispachLC { get; set; }
        public int DispachFM { get; set; }
        public bool Crossing { get; set; }
        public string PONumber { get; set; }
        public string BENumber { get; set; }
        public string UnbillQty { get; set; }
        public int StockQty { get; set; }
        #endregion

        #region Lock System Properties

        //public LR_ExtraInformation lR_ExtraInformation { get; set; }
        public bool MDispach { get; set; }
        public bool MDelivery { get; set; }
        public bool MBilling { get; set; }
        public bool MAll { get; set; }
        public string MRemark { get; set; }
        public string MNote { get; set; }

        #endregion

        #region Calculation Part
        public string[] ChargeName { get; set; }
        public string[] ChargeValue { get; set; }
        public string[] Fld { get; set; }
        public List<PurchaseVM> Charges { get; set; }
        #endregion

        #region LrSetup

        public LRSetup LRSetup { get; set; }
        public Master Master { get; set; }

        #endregion

        #region PickOrder

        public List<OrderRequestVM> pickOrders { get; set; }
        //public PickOrderSetup PickOrderSetup { get; set; }
        //public PickOrder PickOrder { get; set; }
        public int PickOrderBalQty { get; set; }
        #endregion

        #region Draft
        //public List<DraftVM> DraftVM { get; set; }
        public bool Draft { get; set; }
        public bool PickDraft { get; set; }
        public string Draft_Name { get; set; }
        public string OrderNo { get; set; }
        // iX9: Common default Fields
        #endregion

        public bool DeliveryLR { get; set; }
        public bool DispatchLR { get; set; }
        public bool BillGenerate { get; set; }
        public bool TripLock { get; set; }
        public string TripMsg { get; set; }
        public string BillDetails { get; set; }

        public bool LockAuthorise { get; set; }
        public bool LedgerThrough { get; set; }

        public bool getRecentLR { get; set; }
        public bool ConsignorRestrict { get; set; }
        public bool DescriptionRestrict { get; set; }
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
        public string GridHtml { get; set; }
        public string LrGenerate { get; set; }
    }
    public class LRAttachment
    {
        public HttpPostedFileBase UploadFile { get; set; }
        public byte[] Image { get; set; }
        public string FileName { get; set; }
        public string DocumentString { get; set; }
        public string ContentType { get; set; }
        public string AttachLrNo { get; set; }
    }
}