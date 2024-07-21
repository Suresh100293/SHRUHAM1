using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class TripSheetModel
    {
        public string Srl { get; set; }

        public DateTime DocDate { get; set; }
        public string DocuDate { get; set; }

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
        public List<TripSheetModel> SelectedLedger { get; set; }
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

        public List<TripSheetModel> ChargesList { get; set; }
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
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }

        public string RelateTo { get; set; }

        public DateTime StartDate { get; set; }

        public string StartDateStr { get; set; }

        public DateTime EndDate { get; set; }

        public string EndDateStr { get; set; }
        public decimal StartKM { get; set; }
        public decimal EndKM { get; set; }
        public decimal ChargeKM { get; set; }
        public decimal Rate { get; set; }
        public decimal ChargeTrip { get; set; }

        public int FromLocation { get; set; }

        public int ToLocation { get; set; }

        public string ChargesAcc { get; set; }

        public string Descr { get; set; }

        public string LCDate { get; set; }
        public string LCNo { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string Charges { get; set; }

        public string Amount { get; set; }

        public List<TripSheetModel> AdvanceList { get; set; }

        public string ChargesAccName { get; set; }

        public List<TripSheetModel> LCDetailList { get; set; }

        public string FromName { get; set; }

        public string ToName { get; set; }

        public string AddNarr { get; set; }

        public string LessNarr { get; set; }

        public decimal AddAmt { get; set; }

        public decimal LessAmt { get; set; }

        public List<TripSheetModel> LessNarrList { get; set; }

        public List<TripSheetModel> AddNarrList { get; set; }

        public string hdnTripDetsSaved { get; set; }
        public decimal TripFinalAmt { get; set; }
        public decimal LCAmt { get; set; }
    }
}