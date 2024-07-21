using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public partial class OutstandingVM
    {
        public int RECORDKEY { get; set; }
        public string aMaintype { get; set; }
        public decimal Amount { get; set; }
        public string aPrefix { get; set; }
        public string aSrl { get; set; }
        public int aSno { get; set; }
        public string aSubType { get; set; }
        public string aType { get; set; }
        public string AUTHIDS { get; set; }
        public string AUTHORISE { get; set; }
        public System.DateTime BillDate { get; set; }
        public string BillNumber { get; set; }
        public string Branch { get; set; }
        public string Broker { get; set; }
        public decimal Brokerage { get; set; }
        public decimal BrokerAmt { get; set; }
        public decimal BrokerOn { get; set; }
        public System.DateTime ChlnDate { get; set; }
        public string ChlnNumber { get; set; }
        public string Code { get; set; }
        public int CrPeriod { get; set; }
        public string CurrName { get; set; }
        public decimal CurrRate { get; set; }
        public System.DateTime DocDate { get; set; }
        public string Flag { get; set; }
        //public string HWSerial { get; set; }
        //public string HWSerial2 { get; set; }
        public string MainType { get; set; }
        public string Narr { get; set; }
        public string OpFlag { get; set; }
        public decimal OpnPending { get; set; }
        public System.DateTime OrdDate { get; set; }
        public string OrdNumber { get; set; }
        public decimal Pending { get; set; }
        public decimal Pending2 { get; set; }
        public string Prefix { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectStage { get; set; }
        public string ProjectUnit { get; set; }
        public string RefParty { get; set; }
        public decimal SalemanAmt { get; set; }
        public decimal SalemanOn { get; set; }
        public decimal SalemanPer { get; set; }
        public string Salesman { get; set; }
        public int Sno { get; set; }
        public string Srl { get; set; }
        public string SubType { get; set; }
        public decimal TdsAmt { get; set; }
        public string Type { get; set; }
        //public decimal TOUCHVALUE { get; set; }
        public string EnteredBy { get; set; }
        public System.DateTime LastUpdateDate { get; set; }
        public decimal CurrAmount { get; set; }
        public System.DateTime ValueDate { get; set; }
        public string LocationCode { get; set; }
        public double CurrRateI { get; set; }



        public int SrNo { get; set; }
        public decimal Amt { get; set; }

    }
}