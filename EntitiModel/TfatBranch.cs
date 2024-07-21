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
    
    public partial class TfatBranch
    {
        public int RECORDKEY { get; set; }
        public System.DateTime DocDate { get; set; }
        public string aAuthno { get; set; }
        public string Account { get; set; }
        public string aCstNo { get; set; }
        public string Addrl1 { get; set; }
        public string Addrl2 { get; set; }
        public string Addrl3 { get; set; }
        public string Addrl4 { get; set; }
        public string aFax { get; set; }
        public string aLstno { get; set; }
        public string aPin { get; set; }
        public string BankDetails { get; set; }
        public string BranchType { get; set; }
        public string Business { get; set; }
        public string Category { get; set; }
        public string CINNo { get; set; }
        public string City { get; set; }
        public string Code { get; set; }
        public string CompCode { get; set; }
        public string Country { get; set; }
        public Nullable<int> CurrDec { get; set; }
        public int CurrName { get; set; }
        public string EInvoiceID { get; set; }
        public string EInvoicePassword { get; set; }
        public string Email { get; set; }
        public string Flag { get; set; }
        public bool Fri { get; set; }
        public bool gp_AddonBased { get; set; }
        public bool gp_AddonBasedDescr { get; set; }
        public string gp_AddonSepCode { get; set; }
        public string gp_AddonSepName { get; set; }
        public bool gp_AllowDiscAP { get; set; }
        public bool gp_AllowDiscAS { get; set; }
        public bool gp_AllowDiscPP { get; set; }
        public bool gp_AllowDiscPS { get; set; }
        public bool gp_AllowEditDelete { get; set; }
        public bool gp_AllowRateP { get; set; }
        public bool gp_AllowRateS { get; set; }
        public bool gp_AutoAccCode { get; set; }
        public Nullable<byte> gp_AutoAccLength { get; set; }
        public Nullable<byte> gp_AutoAccStyle { get; set; }
        public bool gp_Batch { get; set; }
        public bool gp_BillStock { get; set; }
        public bool gp_BIN { get; set; }
        public bool gp_CashLimit { get; set; }
        public Nullable<double> gp_CashLimitAmt { get; set; }
        public bool gp_CashLimitWarn { get; set; }
        public bool gp_CLStock { get; set; }
        public bool gp_CLStockAddORder { get; set; }
        public bool gp_DiscAP { get; set; }
        public bool gp_DiscAS { get; set; }
        public bool gp_DiscPP { get; set; }
        public bool gp_DiscPS { get; set; }
        public bool gp_DuplicateItemName { get; set; }
        public bool gp_EnableParty { get; set; }
        public bool gp_GINQty { get; set; }
        public bool gp_GSTDupl { get; set; }
        public Nullable<int> gp_GSTStyle { get; set; }
        public bool gp_GSTSupply { get; set; }
        public string gp_Holiday1 { get; set; }
        public string gp_Holiday2 { get; set; }
        public bool gp_ItemAutoCode { get; set; }
        public bool gp_ItemAutoDescr { get; set; }
        public string gp_ItemAutoPattern { get; set; }
        public bool gp_ItemClass { get; set; }
        public Nullable<byte> gp_ItemCodesStyle { get; set; }
        public bool gp_ItemCodeStyle { get; set; }
        public bool gp_ItemDescrStyle { get; set; }
        public bool gp_ItemPrefixName { get; set; }
        public string gp_ItemSearchFlds { get; set; }
        public string gp_ItemSearchStyle { get; set; }
        public string gp_Length { get; set; }
        public bool gp_LocWiseTax { get; set; }
        public bool gp_MultiUnit { get; set; }
        public bool gp_NegStock { get; set; }
        public bool gp_NegStockAsOn { get; set; }
        public bool gp_NegWarn { get; set; }
        public bool gp_OrdIncludeRet { get; set; }
        public bool gp_Pharma { get; set; }
        public bool gp_PostP { get; set; }
        public bool gp_PricelistReqd { get; set; }
        public bool gp_PSP { get; set; }
        public bool gp_PurchPostTDS { get; set; }
        public bool gp_QtnA { get; set; }
        public Nullable<System.DateTime> gp_RCMDate { get; set; }
        public Nullable<byte> gp_RoundVAT { get; set; }
        public bool gp_Serial { get; set; }
        public bool gp_SEZChargeParty { get; set; }
        public bool gp_SONoDupl { get; set; }
        public bool gp_SOPropagation { get; set; }
        public bool gp_SPAdjForce { get; set; }
        public bool gp_SPAdjNoStore { get; set; }
        public string gp_SPAdjTypes { get; set; }
        public Nullable<byte> gp_VatDecP { get; set; }
        public Nullable<byte> gp_VatDecS { get; set; }
        public string gp_VATGST { get; set; }
        public string Grp { get; set; }
        public string GSTAuthToken { get; set; }
        public string GSTNo { get; set; }
        public string GSTSEK { get; set; }
        public Nullable<System.DateTime> GSTTokenExpiry { get; set; }
        public bool LastBranch { get; set; }
        public bool LastUpdated { get; set; }
        public string Licence2 { get; set; }
        public int LocationCode { get; set; }
        public bool LogIn { get; set; }
        public byte[] Logo { get; set; }
        public string Modules { get; set; }
        public bool Mon { get; set; }
        public string Name { get; set; }
        public string PanNo { get; set; }
        public Nullable<int> PCCode { get; set; }
        public string PrintInfo { get; set; }
        public string ProxyServer { get; set; }
        public bool Sat { get; set; }
        public string SMSCaption { get; set; }
        public string SMSPass { get; set; }
        public bool SMSPrefix { get; set; }
        public string SMSURL { get; set; }
        public string SMSUserId { get; set; }
        public string State { get; set; }
        public bool Status { get; set; }
        public bool Sun { get; set; }
        public string TDSAuthorise { get; set; }
        public string TDSCir { get; set; }
        public string TDSOffice { get; set; }
        public string TDSReg { get; set; }
        public string Tel1 { get; set; }
        public string Tel2 { get; set; }
        public string Tel3 { get; set; }
        public string Tel4 { get; set; }
        public bool Thu { get; set; }
        public Nullable<decimal> TimeDiff { get; set; }
        public string TINNumber { get; set; }
        public bool Tue { get; set; }
        public string Users { get; set; }
        public string VATReg { get; set; }
        public string VehicleWaitTime { get; set; }
        public bool Wed { get; set; }
        public string WorkTimeFrom { get; set; }
        public string WorkTimeTo { get; set; }
        public string www { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public Nullable<bool> BranchMail { get; set; }
        public Nullable<bool> LocalMail { get; set; }
        public string BCCTo { get; set; }
        public string CCTo { get; set; }
        public string SMTPServer { get; set; }
        public string SMTPUser { get; set; }
        public string SMTPPassword { get; set; }
        public Nullable<int> SMTPPort { get; set; }
    }
}
