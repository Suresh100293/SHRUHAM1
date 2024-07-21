using ALT_ERP3.Areas.Accounts.Models;

using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3
{
    public class LRInvoiceVM
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public List<LRInvoiceVM> ValueList { get; set; }
        public bool LedgerThrough { get; set; }

        public string ChequeNo { get; set; }
        public string CurrName { get; set; }
        public List<SelectListItem> RpoertViewData { get; set; }
        public string EmptyViewDataId { get; set; }


        public string HitContractType { get; set; }



        public int RECORDKEY { get; set; }
        public decimal AccAmt { get; set; }
        public string Account { get; set; }
        public int AltAddress { get; set; }
        public decimal Amt { get; set; }
        public decimal Amt1 { get; set; }
        public string AuthIds { get; set; }
        public string Authorise { get; set; }
        public System.DateTime BillDate { get; set; }
        public string BillNumber { get; set; }
        public string Branch { get; set; }
      
        public decimal Cess { get; set; }
        public string Code { get; set; }
        public int CrPeriod { get; set; }
      
        public string DeliverBy { get; set; }
        public int DelyAltAdd { get; set; }
        public string DelyCode { get; set; }
        public decimal Disc { get; set; }
        public System.DateTime DocDate { get; set; }
        public bool ForceOrderS { get; set; }
      
        public string MainType { get; set; }
        public string Narr { get; set; }
        public string PDocNo { get; set; }
        public string Prefix { get; set; }
    
        public double Qty { get; set; }
        public string ReceiveBy { get; set; }
        public string RefBy { get; set; }
        public string RefDoc { get; set; }
        public string Reference { get; set; }
        public int RefSno { get; set; }
        public decimal RoundOff { get; set; }
        public string Srl { get; set; }
        public int Stage { get; set; }
        public string SubType { get; set; }
       
        public decimal TaxAmt { get; set; }
        public string TaxCode { get; set; }
        public decimal TDSAble { get; set; }
        public decimal TDSCess { get; set; }
        public string TDSReason { get; set; }
        public decimal TDSSchg { get; set; }
        public decimal TDSTax { get; set; }
        public string Type { get; set; }
        public decimal Val1 { get; set; }
        public string WONumber { get; set; }
        public string EnteredBy { get; set; }
      
        public int LocationCode { get; set; }
        public double CurrRateI { get; set; }
      
        public string RichNote { get; set; }
        public string Unit { get; set; }
        public double Factor { get; set; }
        public decimal DiscAmt { get; set; }
     
        public string AddLess { get; set; }
       
        public string Remark { get; set; }
      
        public string LRName { get; set; }
        public List<LRInvoiceVM> NewItemList { get; set; }
        public List<LRInvoiceVM> PickUpList { get; set; }
      
        public List<LRInvoiceVM> LedgerPostList { get; set; }
        public double Qty2 { get; set; }
        public string ProductAddOn { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public int SrNo { get; set; }
        public int tempId { get; set; }
        public int PtempId { get; set; }
        public bool tempIsDeleted { get; set; }
        public string id { get; set; }
     
        public string LRPartyInvoice { get; set; }
        public string LRPONumber { get; set; }
        public string Document { get; set; }
        public string ChangeLog { get; set; }
        public string AccountName { get; set; }
        public string DelyName { get; set; }
      
        public int Source { get; set; }
        public int Store { get; set; }
        public string Mode { get; set; }
        public double Balance { get; set; }
     
        
        public string GSTType { get; set; }
        public List<LRInvoiceVM> DelyScheList { get; set; }
        public double ExecutedQty { get; set; }
        public string StoreName { get; set; }
        public decimal Days { get; set; }
        public double Pending { get; set; }
       
        public double Qty1 { get; set; }
        public double MRP { get; set; }
        public double Rate { get; set; }
        public string ViewDataId { get; set; }
        public string AddressFrom { get; set; }
        public string AddressTo { get; set; }
        public string GstTypeName { get; set; }
        public string Module { get; set; }
        public string SourceDoc { get; set; }
        public string DocuDate { get; set; }
        public string OrdDate { get; set; }
        public string NotuDate { get; set; }
        public List<LRInvoiceVM> Charges { get; set; }
        public string ColVal { get; set; }
        public List<AddOns> AddOnList { get; set; }
        public string Fld { get; set; }
        public string Equation { get; set; }
        public string ChgPostCode { get; set; }
        public string Head { get; set; }
        public string ValueLast { get; set; }
      
        public double InvoiceQty { get; set; }
       
        public string Reason { get; set; }
        public int ReasonCode { get; set; }
      
        public string SalePurchNumber { get; set; }
        public string ParentKey { get; set; }
        public List<SelectListItem> HsnIds { get; set; }
        public string optiontype { get; set; }
        public string Controller2 { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string TableName { get; set; }
        public int Sno { get; set; }
      
        public string hdnProductAddonSaved { get; set; }
        public string hdnAddonSaved { get; set; }
        
        public string TableKey { get; set; }
      
        public string StrDlyDate { get; set; }
        public string Unit2 { get; set; }
        public bool RateOn2 { get; set; }
    
        public bool IsPickUp { get; set; }
        public string IndKey { get; set; }
        public string OrdKey { get; set; }
        public string ChlnKey { get; set; }
        public string QtnKey { get; set; }
        public string EnqKey { get; set; }
        public double PIgst { get; set; }
        public double PCgst { get; set; }
        public double PSgst { get; set; }
        public decimal TotDebit { get; set; }
        public decimal TotCredit { get; set; }
   
        public decimal InvoiceAmt { get; set; }
        public int BillContact { get; set; }
        public int DelyContact { get; set; }
        public string DisplayGSTId { get; set; }
        public string DisplayGSTId2 { get; set; }
        public string ShiptoParty { get; set; }
     
        public List<LRInvoiceVM> DocumentList { get; set; }
        public string ImageStr { get; set; }
        public string FileName { get; set; }
        public string AllFileStr { get; set; }
      
        public string FileNameStr { get; set; }
        public byte[] ImageData { get; set; }
        public string FileContent { get; set; }
        public string ContentType { get; set; }
        public string Path { get; set; }
       
        public double ProductQty { get; set; }
        public double ProductQty2 { get; set; }
        public bool ProductRateOn2 { get; set; }
        public double Stock { get; set; }
        public double Stock2 { get; set; }
        public int BinNumber { get; set; }
        public string InvKey { get; set; }
        public bool IsRoundOff { get; set; }
        public List<AddOns> AddonValueLast { get; set; }
        public bool IsBlanketOrder { get; set; }
        public decimal CrLimit { get; set; }
        public bool Inclusive { get; set; }
        public bool CheckMode { get; set; }
        public bool CheckIRNGENMode { get; set; }
     
        public string IsPrint { get; set; }
        public bool AuthLock { get; set; }
        public bool AuthNoPrint { get; set; }
        public bool AuthReq { get; set; }
        public bool AuthAgain { get; set; }
        public bool IsManual { get; set; }
        public string Status { get; set; }
        public bool EnableParty { get; set; }
       
        public List<GridOption> PrintGridList { get; set; }
       
        public int DueDays { get; set; }
       
        public string IsSaveAs { get; set; }
        
        public double MinSaleRate { get; set; }
        public string SessionFlag { get; set; }
     
        public bool CutTDS { get; set; }
        public decimal TDSSHECess { get; set; }
        public decimal PrevInvAmt { get; set; }
      
        public string Message { get; set; }
       
        public string VATGSTApp { get; set; }
      
        public string Grp { get; set; }
       
        public decimal NewRate { get; set; }
        public string EqCost { get; set; }
        public bool ForceChln { get; set; }
        public string SkipStock { get; set; }
        public decimal Weightage { get; set; }
        public string ItemType { get; set; }
        public string SaveAsDraft { get; set; }
        public bool AllowDraftSave { get; set; }
        public bool IsDraftSave { get; set; }
        public double Pending2 { get; set; }
        public double PendingFactor { get; set; }
        public string PostAccount { get; set; }
        public string PostAccountName { get; set; }
        public double TotalQty { get; set; }
      
        public bool FreeQty { get; set; }
       
        public List<LedgerVM> OSAdjList { get; set; }
        public string PriceDiscCode { get; set; }
        public bool GSTNoITC { get; set; }
        public string EmailTemplate { get; set; }
      
        public bool IsPickUpByOther { get; set; }
        public string PickedUpIn { get; set; }
        public string PlaceOfSupply { get; set; }
     
        public bool DiscOnTaxable { get; set; }
        public string IssueKey { get; set; }

        public string LoadKey { get; set; }
        public string TaxAmtStr { get; set; }
       
        public string ShipFrom { get; set; }

        public string ShipFromName { get; set; }

        public bool IsGstDocType { get; set; }

        public bool CheckStock { get; set; }

        public List<LRInvoiceVM> LRChargeList { get; set; }

        public DateTime LRDocDate { get; set; }

        public string LRDocuDate { get; set; }
        public string Consignor { get; set; }

        public string Consignee { get; set; }

        public string FromLocation { get; set; }
        public string ToLocation { get; set; }

        public string BookNarr { get; set; }

        public string BillNarr { get; set; }
        public decimal Freight { get; set; }
        public decimal FLD01 { get; set; }
        public decimal FLD02 { get; set; }
        public decimal FLD03 { get; set; }
        public decimal FLD04 { get; set; }
        public decimal FLD05 { get; set; }
        public decimal FLD06 { get; set; }
        public decimal FLD07 { get; set; }
        public decimal FLD08 { get; set; }
        public decimal FLD09 { get; set; }
        public decimal FLD10 { get; set; }

        public decimal NetFLD01 { get; set; }
        public decimal NetFLD02 { get; set; }
        public decimal NetFLD03 { get; set; }
        public decimal NetFLD04 { get; set; }
        public decimal NetFLD05 { get; set; }

        public string HeadF001 { get; set; }
        public string HeadF002 { get; set; }
        public string HeadF003 { get; set; }
        public string HeadF004 { get; set; }
        public string HeadF005 { get; set; }
        public List<decimal> ChgPickupList { get; set; }
        public List<string> HeaderList { get; set; }
        public List<string> AllHeaderList { get; set; }
        public string ChargeType { get; set; }
        public double ChgWt { get; set; }
        public double ActWt { get; set; }

        public List<decimal> TotalChgPickupList { get; set; }

        public bool POD { get; set; }

        public string AccParentGrp { get; set; }
        public string AccParentGrpName { get; set; }

        public string Customer { get; set; }
        public string CustomerName { get; set; }



        public bool BillBoth { get; set; }
        public bool BillAuto { get; set; }

        public bool OthLRShow { get; set; }
        public bool LRCutTDS { get; set; }
        public bool ShowLedgerPost { get; set; }

        public string LRGenerate { get; set; }

        public bool LRReqd { get; set; }
        public string FilCustomer { get; set; }

        public string FilBranch { get; set; }

        public string CPerson { get; set; }
        public DateTime HoldInvoiceDt { get; set; }
        public bool HoldInvoice { get; set; }
        public bool AlertHoldInvoice { get; set; }
        public string HoldNarr { get; set; }
        public string Tickler { get; set; }

        public bool LockQty { get; set; }

        public bool PeriodLock { get; set; }
        public bool LockAuthorise { get; set; }


        //TDS
        public string TDSCode { get; set; }
        public string TDSCodeName { get; set; }
        public bool TDSFlag { get; set; }
        public decimal TDSRate { get; set; }
        public decimal TDSAmt { get; set; }


        //GST
        public bool GSTFlag { get; set; }
        public string GSTCode { get; set; }
        public string GSTCodeName { get; set; }
        public decimal Taxable { get; set; }
        public decimal IGSTRate { get; set; }
        public decimal IGSTAmt { get; set; }
        public decimal CGSTRate { get; set; }
        public decimal CGSTAmt { get; set; }
        public decimal SGSTRate { get; set; }
        public decimal SGSTAmt { get; set; }
        public string LRRefTableKey { get; set; }

        public decimal RoundOffAmt { get; set; }

        //public int IntegerValue { get; set; }
        //public int SelectedIntegerValue { get; set; }
    }
}