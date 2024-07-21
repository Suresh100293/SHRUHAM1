﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EntitiModel;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class OpeningLRVM
    {
        public string GST_Tickler_DecalreValue { get; set; }
        public string EWB_Tickler_DecalreValue { get; set; }

        public string LrContactPerson { get; set; }
        public string LrContactPersonNo { get; set; }
        public string LrContactPersonEmailId { get; set; }

        public decimal DieselAmt { get; set; }
        public string DieselLtr { get; set; }
        public string Driver { get; set; }
        public string DriverN { get; set; }

        public string Sno { get; set; }
        public string ChequeNo { get; set; }
        public string CurrName { get; set; }
        public string BillNumber { get; set; }
        public List<SelectListItem> RpoertViewData { get; set; }
        public string EmptyViewDataId { get; set; }

        #region Attachment

        public bool PeriodLock { get; set; }
        public AttachmentVM AttachmentVM { get; set; }
        

        public LRAttachment Attachment { get; set; }
        public List<LRAttachment> attachments { get; set; }
        #endregion
        #region LrMaster

        public string Branch { get; set; }
        public string LRMode { get; set; }
        public string Branch_Name { get; set; }
        public string BookDate { get; set; }
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
        public string VehicleNo { get; set; }
        public double ActWt { get; set; }
        public double ChgWt { get; set; }
        public decimal Amt { get; set; }
        public string Val1 { get; set; }
        public string FldValue { get; set; }
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

        #region Calculation Part
        public string[] ChargeName { get; set; }
        public string[] ChargeValue { get; set; }
        public string[] Fld { get; set; }
        public List<PurchaseVM> Charges { get; set; }
        #endregion

        //Extra Parameters
        public string LrGenerate { get; set; }
        public LRSetup LRSetup { get; set; }
        public bool BillGenerate { get; set; }
        public string BillDetails { get; set; }
        public bool DispatchLR { get; set; }
        public bool DeliveryLR { get; set; }
        public bool LockAuthorise { get; set; }


        public string RecGST { get; set; }
        public string SendGST { get; set; }
        public string BillGST { get; set; }
        public string TrnMode { get; set; }
        public string HSNCODE { get; set; }


        public bool LockLR { get; set; }
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
        public string VehicleNoName { get; set; }
    }
   
}