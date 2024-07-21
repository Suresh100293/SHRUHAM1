using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class AccountOpeningVM
    {// iX9: Field Structure of Ledger
        public int RECORDKEY { get; set; }
        public string AltCode { get; set; }
        public bool Audited { get; set; }
        public int BankCode { get; set; }
        public System.DateTime BillDate { get; set; }
        public string BillDateVM { get; set; }
        public string BillNumber { get; set; }
        public decimal CGSTAmt { get; set; }
        public decimal CGSTRate { get; set; }
        public string Cheque { get; set; }
        public System.DateTime ChequeDate { get; set; }
        public string ChequeDateVM { get; set; }
        public bool ChequeReturn { get; set; }
        public int ChqCategory { get; set; }
        public System.DateTime ClearDate { get; set; }

        public string Code { get; set; }
        public string CompCode { get; set; }
        public decimal Credit { get; set; }
        public int CrPeriod { get; set; }
        public decimal CurrAmount { get; set; }
        public string CurrName { get; set; }
        public decimal CurrRate { get; set; }
        public decimal Debit { get; set; }
        public bool Discounted { get; set; }
        public System.DateTime DocDate { get; set; }
        public System.DateTime DueDate { get; set; }
        public string DueDateVM { get; set; }
        public int GSTType { get; set; }
        public string HSNCode { get; set; }
        public decimal IGSTAmt { get; set; }
        public decimal IGSTRate { get; set; }
        public int LocationCode { get; set; }
        public string MainType { get; set; }
        public string Narr { get; set; }
        public string ParentKey { get; set; }
        public string Party { get; set; }
        public int PCCode { get; set; }
        public string Prefix { get; set; }
        public int ProjectCode { get; set; }
        public int ProjectStage { get; set; }
        public int ProjectUnit { get; set; }
        public string RecoFlag { get; set; }
        public string RefDoc { get; set; }
        public bool Reminder { get; set; }
        public decimal SGSTAmt { get; set; }
        public decimal SGSTRate { get; set; }
        public int Sno { get; set; }
        public string Srl { get; set; }
        public string SubType { get; set; }
        public string TableKey { get; set; }
        public int TaskID { get; set; }
        public string TDSChallanNumber { get; set; }
        public int TDSCode { get; set; }
        public bool TDSFlag { get; set; }
        public string OrderNumber { get; set; }
        public string ChlnNumber { get; set; }
        // iX9: Common default Fields
        public string Document { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Mode { get; set; }
        
        // iX9: used while passing parameters from MasterGrid to Option and Back 
        public int tEmpID { get; set; }
        public string Controller2 { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
        public decimal Amount { get; set; }
        public string AmtType { get; set; }
        public List<AccountOpeningVM> BreakUpList { get; set; }
        public string SessionFlag { get; set; }
        public string InvNumber { get; set; }
        public string addDocuDate { get; set; }
        public decimal addAmount { get; set; }
        public string addAmtType { get; set; }
        public decimal addOrgAmount { get; set; }
        public int CreditDays { get; set; }
        public string addOrdDate { get; set; }
        public string ChlnDate { get; set; }
        public string SalesMan { get; set; }
        public decimal SalesManCommOn { get; set; }
        public decimal SalesManCommOnPer { get; set; }
        public decimal SalesManCommOnAmt { get; set; }
        public string Broker { get; set; }
        public decimal BrokerCommOn { get; set; }
        public decimal BrokerCommOnPer { get; set; }
        public decimal BrokerCommOnAmt { get; set; }
        public string SalesManName { get; set; }
        public int SalesManCode { get; set; }
        public int BrokerCode { get; set; }
        public string BrokerName { get; set; }
        public int CurrencyCode { get; set; }
        public string TypeName { get; set; }
        public string CurrencyName { get; set; }
        public bool AuthLock { get; set; }
        public bool AuthNoPrint { get; set; }
        public bool AuthReq { get; set; }
        public bool AuthAgain { get; set; }
        public string CostCentre { get; set; }
        public string CostCentreName { get; set; }
        public List<AccountOpeningVM> CostCentreList { get; set; }
        public string BaseGr { get; set; }
        public bool HasRecord { get; set; }
        public decimal addBillAmt { get; set; }

        public string Account { get; set; }
        public string AccountN { get; set; }

        public string ACGroup { get; set; }
        public string ACGroupN { get; set; }

        public string Branch { get; set; }
        public string BranchN { get; set; }

        public string Type { get; set; }
        public string TypeN { get; set; }

        public string CustChild { get; set; }
        public string CustChildN { get; set; }

        public string CurrCode { get; set; }

        public string ChildSummary { get; set; }

        public string CostBranch { get; set; }
        public string CostBranchN { get; set; }

        public bool AdjustBill { get; set; }
        public string Message { get; set; }
    }
}