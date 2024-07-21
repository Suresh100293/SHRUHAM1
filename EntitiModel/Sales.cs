//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EntitiModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class Sales
    {
        public int RECORDKEY { get; set; }
        public string AcHeadCode { get; set; }
        public Nullable<decimal> AcHeadPerc { get; set; }
        public Nullable<decimal> AddTax { get; set; }
        public Nullable<byte> AltAddress { get; set; }
        public Nullable<decimal> Amt { get; set; }
        public Nullable<decimal> Amt1 { get; set; }
        public Nullable<decimal> Amt10 { get; set; }
        public Nullable<decimal> Amt2 { get; set; }
        public Nullable<decimal> Amt3 { get; set; }
        public Nullable<decimal> Amt4 { get; set; }
        public Nullable<decimal> Amt5 { get; set; }
        public Nullable<decimal> Amt6 { get; set; }
        public Nullable<decimal> Amt7 { get; set; }
        public Nullable<decimal> Amt8 { get; set; }
        public Nullable<decimal> Amt9 { get; set; }
        public Nullable<decimal> AssAmt { get; set; }
        public Nullable<int> BillContact { get; set; }
        public Nullable<System.DateTime> BillDate { get; set; }
        public string BillNumber { get; set; }
        public bool BomUpdated { get; set; }
        public string Branch { get; set; }
        public string Broker { get; set; }
        public Nullable<decimal> Brokerage { get; set; }
        public Nullable<decimal> BrokerAmt { get; set; }
        public Nullable<decimal> BrokerOn { get; set; }
        public Nullable<System.DateTime> CancelDate { get; set; }
        public bool CancelFlag { get; set; }
        public bool CancelFlagGST { get; set; }
        public Nullable<byte> CancelID { get; set; }
        public string CancelReason { get; set; }
        public Nullable<decimal> CashAmt { get; set; }
        public Nullable<decimal> Cess { get; set; }
        public Nullable<decimal> CGSTAmt { get; set; }
        public Nullable<System.DateTime> ChlnDate { get; set; }
        public string ChlnNumber { get; set; }
        public Nullable<decimal> ChqAmt { get; set; }
        public string ChqNo { get; set; }
        public string Code { get; set; }
        public string CompCode { get; set; }
        public string CostCentre { get; set; }
        public bool Coupan { get; set; }
        public Nullable<decimal> CrCardAmt { get; set; }
        public string CrCardNo { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CrPeriod { get; set; }
        public decimal CurrAmount { get; set; }
        public string CurrName { get; set; }
        public Nullable<decimal> CurrRate { get; set; }
        public Nullable<int> DCCode { get; set; }
        public Nullable<byte> DelyAltAdd { get; set; }
        public string Delycode { get; set; }
        public Nullable<int> DelyContact { get; set; }
        public Nullable<System.DateTime> DelyDate { get; set; }
        public Nullable<decimal> Disc { get; set; }
        public Nullable<decimal> Discount { get; set; }
        public bool Discounted { get; set; }
        public Nullable<System.DateTime> DocDate { get; set; }
        public Nullable<System.DateTime> EWBDate { get; set; }
        public string EWBNo { get; set; }
        public Nullable<System.DateTime> EWBValidDate { get; set; }
        public Nullable<decimal> ExcAmt { get; set; }
        public string ExcInvNo { get; set; }
        public Nullable<decimal> ExciseAs { get; set; }
        public Nullable<decimal> ExcRate { get; set; }
        public bool ForceOrderP { get; set; }
        public Nullable<System.DateTime> GSTAckDate { get; set; }
        public string GSTAckNo { get; set; }
        public string GSTeInoivce { get; set; }
        public string GSTError { get; set; }
        public bool GSTEWayFlag { get; set; }
        public string GSTIRNNumber { get; set; }
        public Nullable<int> GSTNoITC { get; set; }
        public string GSTQRCode { get; set; }
        public Nullable<int> GSTType { get; set; }
        public Nullable<decimal> IGSTAmt { get; set; }
        public string InsuranceNo { get; set; }
        public Nullable<int> InsuranceSeq { get; set; }
        public string ITFNumber { get; set; }
        public Nullable<System.DateTime> LastUpdateOn { get; set; }
        public string LoadingKey { get; set; }
        public int LocationCode { get; set; }
        public string MainType { get; set; }
        public string Narr { get; set; }
        public string OrdNumber { get; set; }
        public Nullable<int> PCCode { get; set; }
        public string PlaceOfSupply { get; set; }
        public string Prefix { get; set; }
        public byte[] QRCodeImage { get; set; }
        public Nullable<decimal> Qty { get; set; }
        public double Qty2 { get; set; }
        public Nullable<int> ReasonCode { get; set; }
        public string RefParty { get; set; }
        public string RefSno { get; set; }
        public string Remark { get; set; }
        public Nullable<decimal> RoundOff { get; set; }
        public Nullable<decimal> RunBal { get; set; }
        public Nullable<decimal> SalemanAmt { get; set; }
        public Nullable<decimal> SalemanOn { get; set; }
        public Nullable<decimal> SalemanPer { get; set; }
        public string SalesMan { get; set; }
        public Nullable<decimal> SGSTAmt { get; set; }
        public string ShipFrom { get; set; }
        public string SourceDoc { get; set; }
        public string Srl { get; set; }
        public Nullable<decimal> STAmt { get; set; }
        public Nullable<decimal> Staxable { get; set; }
        public Nullable<decimal> STCess { get; set; }
        public string STCode { get; set; }
        public Nullable<decimal> STSheCess { get; set; }
        public string SubType { get; set; }
        public Nullable<decimal> SurCharge { get; set; }
        public string TableKey { get; set; }
        public Nullable<decimal> Taxable { get; set; }
        public Nullable<decimal> TaxAmt { get; set; }
        public string TaxCode { get; set; }
        public Nullable<decimal> TDSAble { get; set; }
        public Nullable<decimal> TDSAmt { get; set; }
        public Nullable<decimal> TDSCess { get; set; }
        public Nullable<int> TDSCode { get; set; }
        public bool TDSFlag { get; set; }
        public string TDSReason { get; set; }
        public Nullable<decimal> TDSSchg { get; set; }
        public Nullable<decimal> TDSTax { get; set; }
        public string Type { get; set; }
        public Nullable<decimal> Val1 { get; set; }
        public Nullable<decimal> Val10 { get; set; }
        public Nullable<decimal> Val2 { get; set; }
        public Nullable<decimal> Val3 { get; set; }
        public Nullable<decimal> Val4 { get; set; }
        public Nullable<decimal> Val5 { get; set; }
        public Nullable<decimal> Val6 { get; set; }
        public Nullable<decimal> Val7 { get; set; }
        public Nullable<decimal> Val8 { get; set; }
        public Nullable<decimal> Val9 { get; set; }
        public Nullable<decimal> Val11 { get; set; }
        public Nullable<decimal> Val12 { get; set; }
        public Nullable<decimal> Val13 { get; set; }
        public Nullable<decimal> Val14 { get; set; }
        public Nullable<decimal> Val15 { get; set; }
        public Nullable<decimal> Val16 { get; set; }
        public Nullable<decimal> Val17 { get; set; }
        public Nullable<decimal> Val18 { get; set; }
        public Nullable<decimal> Val19 { get; set; }
        public Nullable<decimal> Val20 { get; set; }
        public string WONumber { get; set; }
        public string zQty { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string CashBankCode { get; set; }
        public string CustGroup { get; set; }
        public string CPerson { get; set; }
    }
}
