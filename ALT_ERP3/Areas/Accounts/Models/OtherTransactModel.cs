using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class OtherTransactModel
    {
        public List<GridOption> PrintGridList { get; set; }

        public bool LedgerThrough { get; set; }

        public bool LockAdjustTrip { get; set; }
        public string LockAdjustTripMessage { get; set; }

        public bool LockAuthorise { get; set; }
        public bool PeriodLock { get; set; }
        public int RECORDKEY { get; set; }
        public string Branch { get; set; }
        public string BTNNO { get; set; }
        public string BTNNOCombo { get; set; }
        public string BTNNOComboN { get; set; }
        public decimal BTNTotalAmt { get; set; }
        public decimal BTNBalAmt { get; set; }
        public string xBranch { get; set; }
        public string Allocation { get; set; }
        public string Document { get; set; }
        public string AltCode { get; set; }
        public bool Audited { get; set; }
        public bool CutTDS { get; set; }
        public string AuthIds { get; set; }
        public string Authorise { get; set; }
        public string BankCode1 { get; set; }
        public DateTime BillDate { get; set; }
        public string BillNumber { get; set; }
        public string CompCode { get; set; }
        public string Cheque { get; set; }
        public DateTime ChequeDate { get; set; }
        public bool ChequeReturn { get; set; }
        public DateTime ClearDate { get; set; }
        public string Code { get; set; }
        public string DebitCode { get; set; }
        public string CreditCode { get; set; }
        public int CostCentre { get; set; }
        public decimal Credit { get; set; }
        public decimal CreditAmt { get; set; }
        public int CrPeriod { get; set; }
        public string CurrName { get; set; }
        public decimal CurrRate { get; set; }
        public decimal Debit { get; set; }
        public decimal DebitAmt { get; set; }
        public bool Discounted { get; set; }
        public DateTime DocDate { get; set; }
        public string MainType { get; set; }
        public string Narr { get; set; }
        public string Party { get; set; }
        public string Prefix { get; set; }
        public int ProjectCode { get; set; }
        public int ProjectStage { get; set; }
        public int ProjectUnit { get; set; }
        public string RecoFlag { get; set; }
        public string RefDoc { get; set; }
        public int Sno { get; set; }
        public string Srl { get; set; }
        public string STCode { get; set; }
        public bool STFlag { get; set; }
        public string SubType { get; set; }
        public string TDSChallanNumber { get; set; }
        public string TDSCode { get; set; }
        public bool TDSFlag { get; set; }
        public string Type { get; set; }
        public string EnteredBy { get; set; }
        public DateTime LASTUPDATEDATE { get; set; }
        public DateTime DueDate { get; set; }
        public bool Reminder { get; set; }
        public int TaskID { get; set; }
        public int ChqCategory { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        //public string Date { get; set; }
        public decimal CurrAmount { get; set; }
        public string RevDoc { get; set; }
        public int LocationCode { get; set; }
        public double CurrRateI { get; set; }
        public string SLCode { get; set; }
        public List<dynamic> headerName = new List<dynamic>();
        public List<OtherTransactModel> ValueList { get; set; }
        public int SrNo { get; set; }
        public int tempId { get; set; }
        public bool tempIsDeleted { get; set; }
        public bool tempIsAdded { get; set; }
        public bool ExistData { get; set; }
        public string AccountName { get; set; }
        public IList<OtherTransactModel> Selectedleger { get; set; }
        public string BankCashCode { get; set; }
        public decimal SumDebit { get; set; }
        public decimal SumCredit { get; set; }
        public string Mode { get; set; }
        public string id { get; set; }
        public string TempDOCD { get; set; }

        public HttpPostedFileBase[] files { get; set; }

        public List<OtherTransactModel> LRDetailList { get; set; }
        public IList<OtherTransactModel> OSAdjList2 { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public string Path { get; set; }
        public string TableKey { get; set; }
        public string Controller { get; set; }

        public string HSNCODE { get; set; }
        public string HSNCODEName { get; set; }

        public string Item { get; set; }
        public string ItemName { get; set; }


        public decimal BillAmt { get; set; }
        public decimal CurrAmt { get; set; }
        public decimal BalanceAmt { get; set; }
        public decimal AdjustAmt { get; set; }
        public decimal TotalPending { get; set; }

        public int count { get; set; }

        public IList<OtherTransactModel> costgroup1 { get; set; }
        public IList<OtherTransactModel> costcentre1 { get; set; }

        public IList<OtherTransactModel> CCList { get; set; }
        public decimal CCAmt { get; set; }
        public int CostCode { get; set; }

        public string PaymentMode { get; set; }
        public string CurrCode { get; set; }
        public string TempChequeDate { get; set; }
        public string BankCashName { get; set; }

        public string FStartDate { get; set; }
        public string FEndDate { get; set; }

        public string CommonSeries { get; set; }
        public string vAuto { get; set; }
        public string DocWidth { get; set; }
        public string AddZero { get; set; }
        public string LastSerial { get; set; }
        public string LimitFrom { get; set; }
        public string LimitTo { get; set; }
        public string PrefixConst { get; set; }
        public string VMode { get; set; }

        public string DocuDate { get; set; }
        public string StrChqDate { get; set; }
        public string BranchName { get; set; }
        public double ODLimit { get; set; }
        public double Usable { get; set; }
        public string DraweeCode { get; set; }
        public string DraweeName { get; set; }
        public string RefParty { get; set; }
        public decimal TDSAmt { get; set; }
        public double TDSCess { get; set; }
        public double TDSSurch { get; set; }
        public double SHECess { get; set; }
        public double TDSDr { get; set; }
        public double TDSCr { get; set; }
        public double TDSOn { get; set; }
        public double TDSAdj { get; set; }

        public string GSTType { get; set; }
        public bool GSTNoItc { get; set; }
        public decimal IGSTRate { get; set; }
        public decimal CGSTRate { get; set; }
        public decimal SGSTRate { get; set; }
        public decimal IGSTAmt { get; set; }
        public decimal CGSTAmt { get; set; }
        public decimal SGSTAmt { get; set; }
        public decimal Taxable { get; set; }
        public bool GSTFlag { get; set; }
        public decimal InvoiceAmt { get; set; }

        public string TransactionType { get; set; }
        public string StrDueDate { get; set; }

        public string AddOnLstVal { get; set; }
        public List<AddOns> AddOnList { get; set; }
        public List<AddOns> ItemList { get; set; }
        public List<OtherTransactModel> NewReferenceList { get; set; }
        public List<OtherTransactModel> ReferencePickUpList { get; set; }
        public string InnerCurrency { get; set; }
        public string StrBillDate { get; set; }
        public string ParentKey { get; set; }

        public decimal PartyAdvances { get; set; }
        public decimal PartyNetOutstanding { get; set; }
        public decimal PartyConsOutstanding { get; set; }
        public decimal TDSRate { get; set; }
        public string RefPartyName { get; set; }
        public string TDSCodeName { get; set; }
        public string PaymentModeName { get; set; }
        public string TransactionTypeName { get; set; }
        public string GSTTypeName { get; set; }
        public bool OSAdjFlag { get; set; }
        public string ChangeLog { get; set; }
        public string SessionFlag { get; set; }
        public int ConstantMode { get; set; }
        public decimal Amt { get; set; }
        public decimal Amount { get; set; }
        public string AmtType { get; set; }
        public string hdnAddonSaved { get; set; }
        
        public string Controller2 { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
        public List<AddOns> AddonValueLast { get; set; }
        public string Fld { get; set; }

        public List<OtherTransactModel> DocumentList { get; set; }
        public string ImageStr { get; set; }
        public string FileName { get; set; }
        public string AllFileStr { get; set; }
        public string FileContStr { get; set; }
        public string FileNameStr { get; set; }
        public byte[] ImageData { get; set; }
        public string FileContent { get; set; }
        public string ContentType { get; set; }
        public decimal BankCharges { get; set; }
        public DateTime BankDate { get; set; }
        public string StrBankDate { get; set; }
        public decimal PartyCharges { get; set; }
        public string BankChgAcc { get; set; }
        public string BankChgAccName { get; set; }
        public bool AuthLock { get; set; }
        public bool AuthNoPrint { get; set; }
        public bool AuthReq { get; set; }
        public bool AuthAgain { get; set; }
        public bool LockPosting { get; set; }
        public string GSTCode { get; set; }
        public string GSTCodeName { get; set; }
        public string NarrStr { get; set; }
        public string LRNumber { get; set; }
        public string ConsignmentKey { get; set; }

        public string RelatedTo { get; set; }
        public string RelatedToN { get; set; }

        public string RelatedToE { get; set; }
        public string RelatedToNE { get; set; }
        public bool ExtraCostUse { get; set; }
   
        public string ApplCode { get; set; }

        public string RelatedChoice { get; set; }
        public string RelatedChoiceN { get; set; }

        public decimal TotDebit { get; set; }

        public decimal TotCredit { get; set; }

        public List<OtherTransactModel> LedgerPostList { get; set; }
        public string OptionType { get; set; }
        public string ShowLedgerPost { get; set; }
        public string FilBranch { get; set; }
        public string OTGenerate { get; set; }
        public bool BillBoth { get; set; }

        public bool BillAuto { get; set; }
        public bool CCAmtMatch { get; set; }

        public int BackDays { get; set; }

        public bool BackDated { get; set; }
        public bool DuplExpLRFMConfirm { get; set; }

        public bool NoDuplExpLRFM { get; set; }

        public string Status { get; set; }
        public string Message { get; set; }

        public string PartialDivName { get; set; }

        public decimal Qty { get; set; }

        public double ActWt { get; set; }
        public double ChgWt { get; set; }
        public string LRFrom { get; set; }
        public string LRTo { get; set; }
        public string LRConginer { get; set; }
        public string LRConsignee { get; set; }

        public string FMNumber { get; set; }
        public string FreightMemoKey { get; set; }

        public List<OtherTransactModel> FMDetailList { get; set; }
        public bool ForwardDated { get; set; }
        public int ForwardDays { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public decimal LRAmt { get; set; }
        public decimal OSAmt { get; set; }

        public decimal FMAmt { get; set; }
        public bool ShowDocSerial { get; set; }
        public bool AllowZeroAmt { get; set; }
        public bool AllowAutoRemark { get; set; }

        public List<OtherTransactModel> TyreStockList { get; set; }
        public List<OtherTransactModel> ConsignmentExplist { get; set; }
        public string StepneeNo { get; set; }
        public string TyreNo { get; set; }
        public string VehicleNo { get; set; }
        public bool NoDuplExpDt { get; set; }

        public bool DuplExpDtConfirm { get; set; }
        public string ConfirmModelMessage { get; set; }
        public decimal Cost { get; set; }

        public double KM { get; set; }

        public decimal ExpDays { get; set; }

        public string TyreType { get; set; }

        public string StockAt { get; set; }

        public bool IsActive { get; set; }

        public string FromType { get; set; }
        public string ProductGroupType { get; set; }
        public bool ReferAccReq { get;  set; }
        public bool SaveCostCenter { get;  set; }
        public bool CostCenterTally { get; set; }

        //public bool AccReqRelated { get;  set; }
        //public bool AccReqRelated { get;  set; }
        //public bool SetupReqRelatedAc { get;  set; }


        public bool ShowDriverOrVehicle { get; set; }
        public string ShowDriverOrVehicleLabel { get; set; }


    }

}