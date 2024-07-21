using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Models;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Vehicles.Models
{
    public class TripSheetVM
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool Pick_Financial_Document { get; set; }
        public bool ShowSummary { get; set; }

        //this is Handle Expenses Account Or Deduction Account LrDetails And Fmdetails Manipulate
        public bool ExpensesAcCurrent { get; set; }

        public List<TripSheetVM> ValueList { get; set; }
        public string FreightMemoKey { get; set; }
        public string ConsignmentKey { get; set; }
        public bool LedgerThrough { get; set; }

        public bool VehicleFlag { get; set; }

        public List<GridOption> PrintGridList { get; set; }

        public string FromDate { get; set; }
        public string TODate { get; set; }

        public bool FMOrNOt { get; set; }



        public string AdvFromDate { get; set; }
        public string AdvTODate { get; set; }
        public bool CutAdv { get; set; }
        public List<Ledger> Advledgers { get; set; }

        public string BalFromDate { get; set; }
        public string BalTODate { get; set; }
        public List<Ledger> Balledgers { get; set; }


        public string CCFromDate { get; set; }
        public string CCTODate { get; set; }
        public bool CutCC { get; set; }
        public List<Ledger> CCledgers { get; set; }

        public string SearchFMNo { get; set; }

        public string TripSheetNo { get; set; }
        public string TripSheetDate { get; set; }
        public string DriverCode { get; set; }
        public string DriveName { get; set; }
        public string Narr { get; set; }
        public decimal NetAmt { get; set; }
        public string ParentKey { get; set; }
        public string TableKey { get; set; }

        public string VehicleCode { get; set; }
        public string VehicleName { get; set; }

        public string FromKM { get; set; }
        public string ToKM { get; set; }
        public string RunningKM { get; set; }
        public decimal PerKMChrg { get; set; }
        public decimal TripChrgKMExp { get; set; }

        #region LR-Cost Center

        public bool DectionCostCenter { get; set; }

        public string PartialDivName { get; set; }
        public bool CostCenterTally { get; set; }

        public string LRNumber { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string SessionFlag { get; set; }
        public bool NoDuplExpLRFM { get; set; }
        public decimal LRAmt { get; set; }
        public int tempId { get; set; }
        public string Code { get; set; }
        public DateTime DocDate { get; set; }
        public decimal Qty { get; set; }
        public double ActWt { get; set; }
        public double ChgWt { get; set; }
        public string LRFrom { get; set; }
        public string LRTo { get; set; }
        public string LRConginer { get; set; }
        public string LRConsignee { get; set; }
        public bool DuplExpLRFMConfirm { get; set; }

        public string AccountName { get; set; }
        public decimal Amt { get; set; }

        public List<TripSheetVM> ConsignmentExplist { get; set; }

        public List<TripSheetVM> LRDetailList { get; set; }

        public string FMNumber { get; set; }
        public decimal FMAmt { get; set; }
        public string Name { get; set; }

        public List<TripSheetVM> FMDetailList { get; set; }

        public List<FmNarrBranchWise> FMNarrDetailList { get; set; }
        public List<RouteDetails> ScheduleDetails { get; set; }

        public decimal DriverOpening { get; set; }

        #endregion


        #region Setup

        public string TripDebitAc { get; set; }
        public string TripDebitAcName { get; set; }
        public bool ChangeAC { get; set; }
        public bool TDSDeduction { get; set; }
        public bool ChangeCharge { get; set; }
        public bool CostCenter { get; set; }
        public bool NoDocumentReq { get; set; }
        public bool ConfirmDupDateOfTrip { get; set; }
        public bool RestrictDupDateOfTrip { get; set; }

        #endregion

        #region Posting

        public List<PurchaseVM> LedgerPostList { get; set; }

        public decimal TotDebit { get; set; }
        public decimal TotCredit { get; set; }
        public bool CutTDS { get; set; }
        public decimal TDSRate { get; set; }
        //public int TDSCode { get; set; }
        public string TDSCode { get; set; }
        public string TDSCodeName { get; set; }
        public decimal TDSAmt { get; set; }


        #endregion


        public string OtherExpAc { get; set; }
        public string OtherExpAcName { get; set; }

        public string OtherDeductnAc { get; set; }
        public string OtherDeductnAcName { get; set; }


        public bool AdvCutFromSummary { get; set; }
        public bool CCCutFromSummary { get; set; }

        public bool LockAuthorise { get; set; }
        public bool PeriodLock { get; set; }
        public bool RefDocument { get; set; }



        public List<FMMasterTrip> fMMasters { get; set; }
        public OtherExpenses otherExpenses { get; set; }
        public List<OtherExpenses> expenseslist { get; set; }
        public List<OtherExpenses> deductionlist { get; set; }
        public LRMaster LRMaster { get; set; }
        public FMMaster FMMaster { get; set; }
        public Ledger ledger { get; set; }

        // iX9: Common default Fields
        public string Document { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Mode { get; set; }
        public string Prefix { get; set; }

        // iX9: used while passing parameters from MasterGrid to Option and Back 
        public string MainType { get; set; }
        public string Controller2 { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }
    }
    public class FMMasterTrip
    {
        public int Sno { get; set; }
        public string RefTablekey { get; set; }
        public string FMNo { get; set; }
        public DateTime Date { get; set; }
        public string VehicleNo { get; set; }
        public string Driver { get; set; }
        public string DriverC { get; set; }
        public string From { get; set; }
        public string FromC { get; set; }
        public string To { get; set; }
        public string ToC { get; set; }
        public string RouteVia { get; set; }
        public string RouteViaC { get; set; }
        public decimal Tripchages { get; set; }
        public decimal LocalCharges { get; set; }
        public decimal ViaCharges { get; set; }
        public decimal PaymentAmt { get; set; }
        public string DieselLtr { get; set; }
        //public decimal TripBal { get; set; }
        public decimal Total { get; set; }
        public bool NarrReq { get; set; }

        public decimal RunningKM { get; set; }
        public decimal StartKM { get; set; }
        public decimal EndKM { get; set; }
        public bool ShowSchedule { get; set; }


    }
    public class OtherExpenses
    {
        public string ExpensesAc { get; set; }
        public string ExpensesAcName { get; set; }
        public string DocNo { get; set; }
        public decimal Amount { get; set; }
        public string ChangeLog { get; set; }
        public int tempId { get; set; }
        public string RelatedTo { get; set; }
        public List<TripSheetVM> LRDetailList { get; set; }
        public List<TripSheetVM> FMDetailList { get; set; }
        public string PartialDivName { get; set; }
        public bool CostCenterTally { get; set; }
        public string Branch { get; set; }
        public string Tablekey { get; set; }
        public string Narr { get; set; }
    }
    public class FmNarrBranchWise
    {
        public string DocNo { get; set; }
        public string Narr { get; set; }
        public string Branch { get; set; }
        public string FMno { get; set; }
        public string Description { get; set; }
    }

}