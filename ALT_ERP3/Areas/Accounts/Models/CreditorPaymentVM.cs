using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class CreditorPaymentVM
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public bool LedgerThrough { get; set; }

        public List<GridOption> PrintGridList { get; set; }

        public bool LockAuthorise { get; set; }
        public bool PeriodLock { get; set; }
        public string Srl { get; set; }

        public DateTime DocDate { get; set; }
        public string DocuDate { get; set; }
        public string AltCode { get; set; }

        public string Account { get; set; }
        public string AccountName { get; set; }
        public string Type { get; set; }
        public string AdvType { get; set; }
        public string Bank { get; set; }
        public string BankName { get; set; }
        public decimal Amt { get; set; }
        public string ChequeNo { get; set; }

        public string FMNo { get; set; }
        public string FMDateStr { get; set; }
        public DateTime FMDate { get; set; }

        public string Branch { get; set; }
        public decimal AdvPending { get; set; }
        public decimal BalPending { get; set; }
        public int tempid { get; set; }
        public string AddLess { get; set; }
        public string CalcOn { get; set; }
        public string Header { get; set; }
        public List<CreditorPaymentVM> SelectedLedger { get; set; }
        public string id { get; set; }
        public string ChangeLog { get; set; }
        public string Mode { get; set; }
        public string Document { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }

        // iX9: used while passing parameters from MasterGrid to Option and Back 
        public string MainType { get; set; }
        public string Controller2 { get; set; }

        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }

        public string SubType { get; set; }

        public string SessionFlag { get; set; }

        public List<CreditorPaymentVM> ChargesList { get; set; }
        public string Fld { get; set; }
        public string PostCode { get; set; }
        public string ColVal { get; set; }
        public string ValueLast { get; set; }
        public string Code { get; set; }
        public string Remark { get; set; }
        public decimal NetAmt { get; set; }
        public decimal AdvAmt { get; set; }
        public decimal BalAmt { get; set; }
        public decimal Freight { get; set; }

        public List<CreditorPaymentVM> PickupList { get; set; }
        public int RecordKey { get; set; }
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

        public List<string> HeaderList { get; set; }
        public List<string> EquationList { get; set; }
        public List<string> FldList { get; set; }

        public string LedgerPrefix { get; set; }

        public string ParentKey { get; set; }

        public string TableKey { get; set; }

        public string LedgerSrl { get; set; }

        public string OthPostType { get; set; }

        public string LedgerType { get; set; }
        public string LedgerMainType { get; set; }
        public string LedgerSubType { get; set; }
        public string LedgerParentKey { get; set; }
        public string LedgerTableKey { get; set; }

        public int Sno { get; set; }

        public int aSno { get; set; }

        public string RefTableKey { get; set; }
        public string Prefix { get; set; }
        public List<decimal> ChgPickupList { get; set; }

        public decimal TotalQty { get; set; }

        public List<decimal> TotalChgPickupList { get; set; }

        public decimal TotalTaxable { get; set; }

        public List<CreditorPaymentVM> LedgerPostList { get; set; }

        public decimal Debit { get; set; }

        public decimal Credit { get; set; }

        public decimal TotDebit { get; set; }

        public decimal TotCredit { get; set; }

        public decimal AdjustedAmt { get; set; }

        public decimal PrevAmt { get; set; }

        public string BranchName { get; set; }
        public string RelatedTo { get; set; }
        public string Party { get; set; }

        public string PartyCode { get; set; }
        public string RelatedToCode { get; set; }

        public string FilBranch { get; set; }
        public string BillNumber { get; set; }
        public string StrBillDate { get; set; }
        public string CurrName { get; set; }
        public decimal BillAmt { get; set; }
        public bool OSAdjFlag { get; set; }
        public bool AutoAdjInterface { get; set; }
        public decimal BillTDS { get; set; }
        public decimal TotalBillTds { get; set; }

        public decimal AdjustAmt { get; set; }

        public decimal PartyAdvances { get; set; }

        public decimal PartyNetOutstanding { get; set; }

        public decimal PartyConsOutstanding { get; set; }

        public decimal Val1 { get; set; }

        public decimal AccAmt { get; set; }

        public List<decimal> AdjAmountList { get; set; }
        public DateTime BillDate { get; set; }
        public int LedgerSno { get; set; }

        public string CustBalance { get; set; }

        public string CashBalance { get; set; }

        public decimal ExcessAmt { get; set; }
        public decimal OnlyAdjAmount { get; set; }
        public decimal UnAdjustedAmt { get; set; }


        //TDS
        public string TDSCode { get; set; }
        public string TDSCodeName { get; set; }
        public bool TDSFlag { get; set; }
        public decimal TDSRate { get; set; }
        public double TDSAmt { get; set; }

        public bool TDSBillWiseCut { get; set; }
        public int RoundOff { get; set; }



        //GST
        public bool GSTFlag { get; set; }
        public string GSTCode { get; set; }
        public string GSTCodeName { get; set; }
        public decimal Taxable { get; set; }
        public decimal InvoiceAmt { get; set; }
        public decimal IGSTRate { get; set; }
        public double IGSTAmt { get; set; }
        public decimal CGSTRate { get; set; }
        public double CGSTAmt { get; set; }
        public decimal SGSTRate { get; set; }
        public double SGSTAmt { get; set; }
    }
}