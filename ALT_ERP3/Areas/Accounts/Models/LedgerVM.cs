using EntitiModel;
using System;
using System.Collections.Generic;
using System.Web;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class LedgerVM
    {
        public bool LedgerThrough { get; set; }

        public int RECORDKEY { get; set; }
        public string Branch { get; set; }
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
        public string ProjCode { get; set; }
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
        public int TDSCode { get; set; }
        public bool TDSFlag { get; set; }
        public string Type { get; set; }
        public string EnteredBy { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool Reminder { get; set; }
        public int TaskID { get; set; }
        public int ChqCategory { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Date { get; set; }
        public decimal CurrAmount { get; set; }
        public string RevDoc { get; set; }
        public int LocationCode { get; set; }
        public double CurrRateI { get; set; }
        public int SLCode { get; set; }
        public List<dynamic> headerName = new List<dynamic>();
        public List<LedgerVM> ValueList = new List<LedgerVM>();
        public int SrNo { get; set; }
        public int tempId { get; set; }
        public bool tempIsDeleted { get; set; }
        public bool tempIsAdded { get; set; }
        public bool ExistData { get; set; }
        public string AccountName { get; set; }
        public IList<LedgerVM> Selectedleger { get; set; }
        public string BankCashCode { get; set; }
        public decimal SumDebit { get; set; }
        public decimal SumCredit { get; set; }
        public string Mode { get; set; }
        public string id { get; set; }
        public string TempDOCD { get; set; }
        public string TempDOCDAmt { get; set; }

        public HttpPostedFileBase[] files { get; set; }

        public IList<LedgerVM> fileUploader { get; set; }
        public List<LedgerVM> OSAdjList { get; set; }
        public IList<LedgerVM> OSAdjList2 { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public string Path { get; set; }
        public string TableKey { get; set; }
        public bool AutoClose { get; set; }
        public string Controller { get; set; }

        public decimal BillAmt { get; set; }
        public decimal CurrAmt { get; set; }
        public decimal BalanceAmt { get; set; }
        public decimal AdjustAmt { get; set; }
        public decimal TotalPending { get; set; }

        public int count { get; set; }

        public IList<LedgerVM> costgroup1 { get; set; }
        public IList<LedgerVM> costcentre1 { get; set; }

        public IList<LedgerVM> CCList { get; set; }
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
        public double TDSAmt { get; set; }
        public double TDSCess { get; set; }
        public double TDSSurch { get; set; }
        public double SHECess { get; set; }
        public double TDSDr { get; set; }
        public double TDSCr { get; set; }
        public double TDSOn { get; set; }
        public double TDSAdj { get; set; }

        public string GSTType { get; set; }
        public bool GSTNoItc { get; set; }
        public double IGSTRate { get; set; }
        public double CGSTRate { get; set; }
        public double SGSTRate { get; set; }
        public double IGSTAmt { get; set; }
        public double CGSTAmt { get; set; }
        public double SGSTAmt { get; set; }
        public double Taxable { get; set; }

        public string TransactionType { get; set; }
        public string StrDueDate { get; set; }

        public string AddOnLstVal { get; set; }
        public List<AddOns> AddOnList { get; set; }
        public List<LedgerVM> NewReferenceList { get; set; }
        public List<LedgerVM> ReferencePickUpList { get; set; }
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
        public string optiontype { get; set; }
        public string Controller2 { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
        public List<AddOns> AddonValueLast { get; set; }
        public string Fld { get; set; }

        public List<LedgerVM> DocumentList { get; set; }
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
        public List<GridOption> PrintGridList { get; set; }
        //public string SubLedgerCode { get; set; }
        public string Message { get; set; }
        public bool CheckMode { get; set; }
        public bool CCReqd { get; set; }
        public List<LedgerVM> SubLedgerList { get; set; }

    }
}