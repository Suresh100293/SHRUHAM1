using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Models;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3
{
    public class PurchaseVM
    {
        public int RECORDKEY { get; set; }
        public decimal AccAmt { get; set; }
        public string Account { get; set; }
        public int AltAddress { get; set; }
        public decimal Amt { get; set; }
        public decimal Amt1 { get; set; }
        public string AuthIds { get; set; }
        public string AUTHORISE { get; set; }
        public System.DateTime BillDate { get; set; }
        public string BillNumber { get; set; }
        public string Branch { get; set; }
        public string BatchNoDupl { get; set; }
      
        public decimal Cess { get; set; }
        public string Code { get; set; }
        public int CrPeriod { get; set; }
        public int CurrName { get; set; }
        public decimal CurrRate { get; set; }
        public string DeliverBy { get; set; }
        public int DelyAltAdd { get; set; }
        public string DelyCode { get; set; }
        public decimal Disc { get; set; }
        public System.DateTime DocDate { get; set; }
        public bool LockAddon { get;  set; }
        public bool ForceOrderS { get; set; }
      
        public string MainType { get; set; }
        public string Narr { get; set; }
        public string PDocNo { get; set; }
        public string Prefix { get; set; }
    
        public double Qty { get; set; }
        public string ReceiveBy { get; set; }
        public bool IsItemClass { get;  set; }
        public string RefBy { get; set; }
        public string RefDoc { get; set; }
        public string Reference { get; set; }
        public int RefSno { get; set; }
        public decimal RoundOff { get; set; }
        public string Srl { get; set; }
        public int Stage { get; set; }
        public string SubType { get; set; }
       
        public decimal Taxable { get; set; }
        public decimal TaxAmt { get; set; }
        public string TaxCode { get; set; }
        public decimal TDSAble { get; set; }
        public decimal TDSAmt { get; set; }
        public decimal TDSCess { get; set; }
        public int TDSCode { get; set; }
        public bool TDSFlag { get; set; }
        public string TDSReason { get; set; }
        public decimal TDSSchg { get; set; }
        public decimal TDSTax { get; set; }
        public string Type { get; set; }
        public decimal Val1 { get; set; }
        public string WONumber { get; set; }
        public string EnteredBy { get; set; }
      
        public int LocationCode { get; set; }
        public double CurrRateI { get; set; }
      
        public decimal CGSTAmt { get; set; }
        public decimal IGSTAmt { get; set; }
        public string RichNote { get; set; }
        public decimal SGSTAmt { get; set; }
        public decimal CGSTRate { get; set; }
        public decimal IGSTRate { get; set; }
        public decimal SGSTRate { get; set; }
        public string Unit { get; set; }
        public double Factor { get; set; }
        public decimal DiscAmt { get; set; }
     
        public string AddLess { get; set; }
        public string MSTPer { get; set; }
        public string PayStage { get; set; }
        public string PayPercent { get; set; }
       
        public string Remark { get; set; }
      
        public string LRName { get; set; }
        public List<PurchaseVM> NewItemList { get; set; }
        public List<PurchaseVM> PickUpList { get; set; }
        public List<PurchaseVM> EditItemList { get; set; }
        public List<PurchaseVM> LedgerPostList { get; set; }
        public double Qty2 { get; set; }
        public string ProductAddOn { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public int SrNo { get; set; }
        public int tempId { get; set; }
        public int PtempId { get; set; }
        public bool tempIsDeleted { get; set; }
        public string id { get; set; }
     
        public string Document { get; set; }
        public string ChangeLog { get; set; }
        public string AccountName { get; set; }
        public string DelyName { get; set; }
      
        public int Source { get; set; }
        public int Store { get; set; }
        public string Mode { get; set; }
        public double Balance { get; set; }
     
        public string HSN { get; set; }
        public string HSNCode { get; set; }
        public string GSTCode { get; set; }
        public string HSNName { get; set; }
        public string GSTCodeName { get; set; }
        public string GSTType { get; set; }
        public List<PurchaseVM> DelyScheList { get; set; }
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
        public List<PurchaseVM> Charges { get; set; }
        public string ColVal { get; set; }
        public List<AddOns> AddOnList { get; set; }
        public List<AddOns> PAddOnList { get; set; }
        public List<AddOns> PEdtAddOnList { get; set; }
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
        public string SendEmail { get; set; }
        public string ITFKey { get; set; }
        public string InterSrl { get; set; }
        public string BarCode { get; set; }
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
     
        public List<PurchaseVM> DocumentList { get; set; }
        public string ImageStr { get; set; }
        public string FileName { get; set; }
        public string AllFileStr { get; set; }
      
        public string FileNameStr { get; set; }
        public byte[] ImageData { get; set; }
        public string FileContent { get; set; }
        public string ContentType { get; set; }
        public string Path { get; set; }
        public string hdnDelySchSaved { get; set; }
       
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
        public string TermId { get; set; }
        public string TermName { get; set; }
        public List<PurchaseVM> TermList { get; set; }
        public string TermTemplate { get; set; }
        public int TermTemplateId { get; set; }
        public decimal CrLimit { get; set; }
        public bool Inclusive { get; set; }
        public bool CheckMode { get; set; }
     
        public string IsPrint { get; set; }
        public bool AuthLock { get; set; }
        public bool AuthNoPrint { get; set; }
        public bool AuthReq { get; set; }
        public bool AuthAgain { get; set; }
        public bool IsManual { get; set; }
        public decimal DiscPerAmt { get; set; }
        public string CurrConv { get; set; }
        public bool AllowCurr { get; set; }
        public double QtyTolePlus { get; set; }
        public double MLSAmount { get; set; }
        public string Status { get; set; }
        public bool EnableParty { get; set; }
        public bool BinConcept { get; set; }
        public bool MultiUnit { get; set; }
       
        public List<GridOption> PrintGridList { get; set; }
        public string StageName { get; set; }
        public string ItemClass1 { get; set; }
        public string ItemClass2 { get; set; }
       
        public int DueDays { get; set; }
       
        public string IsSaveAs { get; set; }
        
        public double MinSaleRate { get; set; }
        public bool NegStock { get; set; }
        public string SessionFlag { get; set; }
     
        public bool CutTDS { get; set; }
        public decimal TDSSHECess { get; set; }
        public decimal PrevInvAmt { get; set; }
        public bool NonStock { get; set; }
      
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
        public decimal DiscCharge { get; set; }
        public decimal DiscChargeAmt { get; set; }
        public string hdnAddDisChgSvd { get; set; }
        public decimal DiscCharge1 { get; set; }
        public decimal DiscChargeAmt1 { get; set; }
        public decimal DiscCharge2 { get; set; }
        public decimal DiscChargeAmt2 { get; set; }
        public decimal DiscCharge3 { get; set; }
        public decimal DiscChargeAmt3 { get; set; }
        public decimal DiscCharge4 { get; set; }
        public decimal DiscChargeAmt4 { get; set; }
        public decimal DiscCharge5 { get; set; }
        public decimal DiscChargeAmt5 { get; set; }
        public decimal DiscCharge6 { get; set; }
        public decimal DiscChargeAmt6 { get; set; }
       
        public bool DiscNotAllowed { get; set; }
        public double PendingFactor { get; set; }
        public bool FIFOOrder { get; set; }
        public string PostAccount { get; set; }
        public string PostAccountName { get; set; }
       
        public string PIKKey { get; set; }
        public double TotalQty { get; set; }
      
        public bool FreeQty { get; set; }
       
        public List<LedgerVM> OSAdjList { get; set; }
        public string OriginalTablekey { get; set; }
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
       
        public string ChargeHtml { get; set; }
        public bool PerValue { get; set; }
        public string ShipFrom { get; set; }

        public string ShipFromName { get; set; }
        public bool IsSelf { get; set; }

        public bool IsGstDocType { get; set; }
      

        public bool CheckStock { get; set; }

      
        public string DocMoreQty { get; set; }

        public List<PriceListVM> DiscChargeList { get; set; }

        public DateTime LRDocDate { get; set; }

        public string LRDocuDate { get; set; }
        public string Consignor { get; set; }

        public string Consignee { get; set; }

        public string FromLocation { get; set; }
        public string ToLocation { get; set; }
        //public int tEmpID { get; set; }

        public string Party { get; set; }
        public string RelatedTo { get; set; }
        public string EmailTo { get;  set; }
        public string EmailCC { get;  set; }
        public string EmailBCC { get;  set; }
        public string EmailSubject { get;  set; }
        public string EmailMessage { get;  set; }
        public bool MileStoneReqd { get;  set; }
        public bool IsBarCodeScan { get;  set; }
        public bool InterBranch { get;  set; }
        public bool LockQty { get;  set; }
        public bool LockDiscCharges { get;  set; }
        public bool LockStore { get;  set; }
        public bool LockFactor { get;  set; }
        public bool HideAddlCharges { get;  set; }
        public bool LockTax { get;  set; }
        public bool LockHSN { get;  set; }
        public bool LockItem { get;  set; }
        public bool LockWarehouse { get;  set; }
        public bool ScanAndEdit { get;  set; }
        public bool LockRate { get;  set; }
        public bool LockDiscount { get;  set; }
        public DateTime EWBDate { get;  set; }
        public DateTime NoteDate { get;  set; }
        public DateTime StrEWBDate { get;  set; }
        public int Broker { get;  set; }
        public string BrokerName { get;  set; }
        public string InsuranceNo { get;  set; }
        public string SalesmanCode { get;  set; }
        public decimal BrokerOn { get;  set; }
        public decimal Brokerage { get;  set; }
        public int PayTerms { get;  set; }
        public string LCNo { get;  set; }
        public decimal SCommOn { get;  set; }
        public string IncoPlace { get;  set; }
        public decimal BrokerAmt { get;  set; }
        public int IncoTerms { get;  set; }
        public string AdvLicence { get;  set; }
        public decimal SCommission { get;  set; }
        public string ProjCode { get;  set; }
        public string ItemName { get;  set; }
        public decimal CVDAmt { get;  set; }
        public decimal CVDCessAmt { get;  set; }
        public decimal CVDExtra { get;  set; }
        public decimal SAmt { get;  set; }
        public decimal CVDSCessAmt { get;  set; }
        public string PKSKey { get;  set; }
        public string RateType { get;  set; }
        public string RateCalcType { get;  set; }
        public string ClassValues1 { get;  set; }
        public string ClassValues2 { get;  set; }
        public string PriceRateCode { get;  set; }
        public string ItemSchemeCode { get;  set; }
        public int? ProjectStage { get;  set; }
        public int? ProjectUnit { get;  set; }
        public bool QCReqd { get;  set; }
        public bool QCDone { get;  set; }
        public object RateTypeTaxable { get;  set; }
        public double MinQty { get;  set; }
        public double MaxQty { get;  set; }
        public bool SerialReq { get;  set; }
        public decimal PackSize { get;  set; }
        public int ReOrderLevel { get;  set; }
        public bool ChkReOrderLevel { get;  set; }
        public object Transporter { get;  set; }
        public string ContactPerson { get;  set; }
        public string NoPkg { get;  set; }
        public string NoteNo { get;  set; }
        public string TransporterN { get;  set; }
        public string TransMode { get;  set; }
        public string VehicleNo { get;  set; }
        public byte DeliveryType { get;  set; }
        public byte FreightType { get;  set; }
        public string EwBillNo { get;  set; }
        public int TrxWeight { get;  set; }
        public short TransportType { get;  set; }
        public bool IsStockSerial { get;  set; }
        public bool IsBatchReqd { get;  set; }
        public bool ChkMinQtyLevel { get;  set; }
        public bool RevBool { get;  set; }
        public string ITFNumber { get;  set; }
        public int? TaskID { get;  set; }
        public int CessAmt { get;  set; }
        public decimal MLSPercent { get;  set; }
    }
}