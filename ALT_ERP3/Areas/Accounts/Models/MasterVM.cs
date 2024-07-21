﻿using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class MasterVM
    {
        //Radio Button list
        
        public Type TypeRD { get; set; }

        public int RECORDKEY { get; set; }
        public string AcHeadCode { get; set; }
        public decimal AcHeadPerc { get; set; }
        public string AcType { get; set; }
        public string AliasPrefix { get; set; }
        public string AppProduct { get; set; }
        public bool ARAP { get; set; }
        public int Area { get; set; }
        public string AreaTree { get; set; }
        public string AUTHIDS { get; set; }
        public string id { get; set; }
        public string AUTHORISE { get; set; }
        public bool AutoEmail { get; set; }
        public bool AutoSMS { get; set; }
        public string BaseGr { get; set; }
        public string Branch { get; set; }
        public int Broker { get; set; }
        public string BrokerName { get; set; }
        public string PriceList { get; set; }
        public string PriceListName { get; set; }
        public decimal Brokerage { get; set; }
        public string BSRCode { get; set; }
        public bool CashDisc { get; set; }
        public int Category { get; set; }
        public bool CCBudget { get; set; }
        public bool CCReqd { get; set; }
        public bool FetchBalAcc { get; set; }
        public string Code { get; set; }
        public string CompanyType { get; set; }
        public int CostCentre { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public bool CutTDS { get; set; }
        public string DepricAC { get; set; }
        public decimal DftAddress { get; set; }
        public decimal DiscDays { get; set; }
        public decimal DiscPerc { get; set; }
        public int DisplayOrder { get; set; }
        public int ExcType { get; set; }
        public bool FBT { get; set; }
        public string FBTCode { get; set; }
        public decimal FBTRate { get; set; }
        public decimal FBTSlab { get; set; }
        public string Flag { get; set; }
        public bool ForceCC { get; set; }
        public string Grp { get; set; }
        public bool Hide { get; set; }
        public decimal IntAmt { get; set; }
        public decimal IntRate { get; set; }
        public bool IsChanged { get; set; }
        public bool IsLast { get; set; }
        public bool IsLead { get; set; }
        public bool IsPublic { get; set; }
        public string LastUpdateBy { get; set; }
        public System.DateTime LastUpdateOn { get; set; }
        public string LeadCode { get; set; }
        public System.DateTime LeadConvertDt { get; set; }
        public bool LedInner { get; set; }
        public bool LedInnerNarr { get; set; }
        public byte LedNarr { get; set; }
        public byte LedNarrNo { get; set; }
        public byte LedNarrOwn { get; set; }
        public byte LedNarrOpposite { get; set; }
        public byte LedSummary { get; set; }
        public byte LedSummaryNo { get; set; }
        public byte LedSummaryDaily { get; set; }
        public byte LedSummaryMonth { get; set; }
        public bool MergeCrLimit { get; set; }
        public string Name { get; set; }

        public bool NoDetails { get; set; }
        public bool NonActive { get; set; }
        public decimal Opening { get; set; }
        public string PaymentTerms { get; set; }
        public byte Rank { get; set; }
        public byte Rank1 { get; set; }
        public byte Rank2 { get; set; }
        public byte Rank3 { get; set; }
        public byte Rank4 { get; set; }
        public byte Rank5 { get; set; }
        public string RevGroup { get; set; }
        public int SalesMan { get; set; }
        public string SalesManName { get; set; }
        public string Sch { get; set; }
        public bool ServiceTax { get; set; }
        public string ServiceTaxCode { get; set; }
        public string ShortName { get; set; }
        public string StringValue { get; set; }
        public int TDSCode { get; set; }
        public decimal TDSPercent { get; set; }
        public string Transporter { get; set; }
        public string UserID { get; set; }
        public string xBranch { get; set; }
        public string EnteredBy { get; set; }
        public System.DateTime LastUpdateDate { get; set; }
        public string StdCode { get; set; }
        public bool IsSubLedger { get; set; }
        public int LocationCode { get; set; }
        public string GroupTree { get; set; }
        public byte Level { get; set; }
        public int Seq { get; set; }
        public List<SelectListItem> SubLedgers { get; set; }
        //Added for FixedAssets

        public string Mode { get; set; }
        public string AcCode { get; set; }
        public string AcDep { get; set; }
        public decimal BookValue { get; set; }
        public decimal CostPrice { get; set; }
        public string Method { get; set; }
        public decimal Rate { get; set; }
        public string Store { get; set; }
        public System.DateTime PurchDate { get; set; }
        public System.DateTime UseDate { get; set; }
        public IList<MasterVM> FixedAssets { get; set; }
        public decimal ODLimit { get; set; }

        //Added for mailing info
        public byte AddOrContact { get; set; }
        public string AName { get; set; }
        public string Adrl1 { get; set; }
        public string Adrl2 { get; set; }
        public string Adrl3 { get; set; }
        public string Adrl4 { get; set; }
        public bool AlreadySent { get; set; }
        public System.DateTime Anndate { get; set; }
        public string AssistEmail { get; set; }
        public string AssistMobile { get; set; }
        public string AssistName { get; set; }
        public string AssistTel { get; set; }
        public System.DateTime Bdate { get; set; }
        public bool CheckCRDays { get; set; }
        public bool CheckCRLimit { get; set; }
        public System.DateTime ChildBdate { get; set; }
        public string ChildName { get; set; }
        public string City { get; set; }
        public string CorpID { get; set; }
        public int CorrespondenceType { get; set; }
        public string Country { get; set; }
        public int CRDaysWarn { get; set; }
        public decimal CrLimit { get; set; }
        public decimal CRLimitTole { get; set; }
        public int CRLimitWarn { get; set; }
        public bool CRLimitWithPO { get; set; }
        public bool CRLimitWithTrx { get; set; }
        //public int CrPeriod { get; set; }
        public string CurrName { get; set; }
        public int Dept { get; set; }
        public int Designation { get; set; }
        public int Division { get; set; }
        public string DrAcNo { get; set; }
        public int DraweeBank { get; set; }
        public string Email { get; set; }
        public bool FailedSent { get; set; }
        public string Fax { get; set; }
        public bool IsLBT { get; set; }
        public int Language { get; set; }
        public System.DateTime LastDate { get; set; }
        public string LastFormat { get; set; }
        public decimal LastOS { get; set; }
        public string LastRemark { get; set; }
        public string LFlag { get; set; }
        public string Licence1 { get; set; }
        public string Licence2 { get; set; }
        public string LstNo { get; set; }
        public int MailingCategory { get; set; }
        public string Mobile { get; set; }
        public string Narr { get; set; }
        public bool NeverSend { get; set; }
        public string PanNo { get; set; }
        public string Password { get; set; }
        public string Person { get; set; }
        public string PhotoPath { get; set; }
        public string Pin { get; set; }
        public string PTaxCode { get; set; }
        public int Religion { get; set; }
        public string ReminderFormat { get; set; }
        public string RTGS { get; set; }
        public string Segment { get; set; }
        public string ServiceTaxNo { get; set; }
        public int Sno { get; set; }
        public int Source { get; set; }
        public System.DateTime SpouseBdate { get; set; }
        public string SpouseName { get; set; }
        public string State { get; set; }
        public string STaxCode { get; set; }
        public string Tel1 { get; set; }
        public string Tel2 { get; set; }
        public string Tel3 { get; set; }
        public string Tel4 { get; set; }
        public string TINNo { get; set; }
        public string VATReg { get; set; }
        public string www { get; set; }
        public string GSTNo { get; set; }
        public string AadharNo { get; set; }
        public string ReraRegNo { get; set; }
        public string MasterDesignation { get; set; }
        public IList<MasterVM> MailInfo { get; set; }

        //Added for TaxDetails
        public bool CutTCS { get; set; }
        public string Deductee { get; set; }
        public decimal DifferRate { get; set; }
        public string DifferRateCertNo { get; set; }
        public System.DateTime Form15HCITDate { get; set; }
        public System.DateTime Form15HDate { get; set; }
        public bool IsDifferRate { get; set; }
        public bool IsForm15H { get; set; }

        //Added For TaxMaster
        public decimal AddTax { get; set; }
        public string AddTaxCode { get; set; }
        public string aDescr { get; set; }
        public string ASchgCode { get; set; }
        public decimal ASurCharge { get; set; }
        public decimal Cess { get; set; }
        public string CessCode { get; set; }
        public bool DiscOnTxbl { get; set; }
        public string Form { get; set; }
        public string FormName { get; set; }
        public bool Inclusive { get; set; }
        public bool IsSERTax { get; set; }
        public bool Locked { get; set; }
        public bool MRPTax { get; set; }
        public bool NoCost { get; set; }
        public decimal Pct { get; set; }
        public string PostCode { get; set; }
        public string SchgCode { get; set; }
        public string Scope { get; set; }
        public decimal SetOff { get; set; }
        public decimal SurCharge { get; set; }
        public bool VATApp { get; set; }
        public bool Composition { get; set; }
        public bool Exempted { get; set; }
        public bool Labour { get; set; }
        public bool Other { get; set; }
        public string RetForm { get; set; }
        public string RTransCode { get; set; }
        public bool Taxable { get; set; }
        public string TransCode { get; set; }
        public string TaxableCode { get; set; }

        //added for transactions
        public bool ChkTempCRDays { get; set; }
        public bool ChkTempCRLimit { get; set; }
        public bool HoldDespatch { get; set; }
        public System.DateTime HoldDespatchDt { get; set; }
        public System.DateTime HoldDespatchDt1 { get; set; }
        public System.DateTime HoldDespatchDt2 { get; set; }
        public bool HoldEnquiry { get; set; }
        public System.DateTime HoldEnquiryDt { get; set; }
        public System.DateTime HoldEnquiryDt2 { get; set; }
        public bool HoldInvoice { get; set; }
        public System.DateTime HoldInvoiceDt { get; set; }
        public System.DateTime HoldInvoiceDt1 { get; set; }
        public System.DateTime HoldInvoiceDt2 { get; set; }
        public bool HoldInward { get; set; }
        public System.DateTime HoldInwardDt { get; set; }
        public string HoldNarr { get; set; }
        public bool HoldOrder { get; set; }
        public System.DateTime HoldOrderDt1 { get; set; }
        public System.DateTime HoldOrderDt2 { get; set; }
        public bool HoldPayment { get; set; }
        public System.DateTime HoldPaymentDt { get; set; }
        public bool HoldPurchase { get; set; }
        public System.DateTime HoldPurchaseDt { get; set; }
        public bool HoldPurcOrder { get; set; }
        public System.DateTime HoldPurcOrderDt { get; set; }
        public bool HoldQuote { get; set; }
        public System.DateTime HoldQuoteDt1 { get; set; }
        public System.DateTime HoldQuoteDt2 { get; set; }
        public bool HoldSaleOrder { get; set; }
        public System.DateTime HoldSaleOrderDt { get; set; }
        public System.DateTime HoldSaleOrderDt1 { get; set; }
        public System.DateTime HoldSaleOrderDt2 { get; set; }
        public System.DateTime TempCrDayDt1 { get; set; }
        public System.DateTime TempCrDayDt2 { get; set; }
        public decimal TempCrLimit { get; set; }
        public System.DateTime TempCrLimitDt1 { get; set; }
        public System.DateTime TempCrLimitDt2 { get; set; }
        public int TempCrPeriod { get; set; }
        public string TempRemark { get; set; }
        public string Ticklers { get; set; }


        public string DefaultCustomerCode { get; set; }
        public string DefaultCustomerCodeN { get; set; }


        //added by dipalee

        public string BankName { get; set; }

        public string SalesChannel { get; set; }
        public List<MasterVM> MailList { get; set; }
        public List<MasterVM> EditMailList { get; set; }
        public IList<MasterVM> AddressList { get; set; }
        public int SrNo1 { get; set; }
        public int? SrNo { get; set; }

        //MasterInfo
        public bool EmailParty { get; set; }
        public bool EmailSalesman { get; set; }
        public string EmailTemplate { get; set; }
        public string EmailUsers { get; set; }
        public byte FormDays { get; set; }
        public bool FormFri { get; set; }
        public bool FormMon { get; set; }
        public bool FormSat { get; set; }
        public bool FormSun { get; set; }
        public bool FormThu { get; set; }
        public bool FormTue { get; set; }
        public bool FormWed { get; set; }
        public byte FreqForm { get; set; }
        public byte FreqOS { get; set; }
        public System.DateTime LastSent { get; set; }
        public int nFormDays { get; set; }
        public int nOSDays { get; set; }
        public byte OSDays { get; set; }
        public bool OSFri { get; set; }
        public bool OSMon { get; set; }
        public bool OSSat { get; set; }
        public bool OSSun { get; set; }
        public bool OSThu { get; set; }
        public bool OSTue { get; set; }
        public bool OSWed { get; set; }
        public bool SMSParty { get; set; }
        public bool SMSSalesman { get; set; }
        public string SMSTemplate { get; set; }
        public string SMSUsers { get; set; }
        public string IncoPlace { get; set; }
        public int IncoTerms { get; set; }
        public decimal CRPeriod { get; set; }
        public bool EmailPartyAlert { get; set; }

        //public decimal ODLImit { get; set; }
        public bool SMSPartyAlert { get; set; }
        public string IDs { get; set; }
        public bool Warning { get; set; }
        public bool Restrict { get; set; }
        public bool Warning1 { get; set; }
        public bool Restrict1 { get; set; }

        public bool Ifexist { get; set; }
        public string Module { get; set; }
        public List<int> List2 { get; set; }
        public string CountryName { get; set; }
        public string CityName { get; set; }
        public string StateName { get; set; }
        public string AreaName { get; set; }
        public string OptionCode { get; set; }

        public string ViewDataId { get; set; }
        public string Header { get; set; }
        public string TableName { get; set; }
        public string Controller { get; set; }
        public string MainType { get; set; }
        public string Controller2 { get; set; }
        public string sidx { get; set; }
        public int rows { get; set; }
        public int page { get; set; }
        public string Document { get; set; }
        public string PoisonLicense { get; set; }
        public string DealerType { get; set; }
        public decimal DefaultCGst { get; set; }
        public decimal DefaultSGst { get; set; }
        public decimal DefaultIGst { get; set; }
        public string GSTType { get; set; }

        public string ReraNo { get; set; }
        public bool GstApplicable { get; set; }
        public string ItemType { get; set; }
        public string HSN { get; set; }
        public string HSNName { get; set; }
        public decimal IGST { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public string OptionGstType { get; set; }
        public string ContactType { get; set; }
        public string Budate { get; set; }
        public string StrAnndate { get; set; }
        public string SpouseBudate { get; set; }
        public string StrChildBdate { get; set; }
        public string StrForm15HDate { get; set; }
        public string StrForm15HCITDate { get; set; }
        public string StrHoldEnquiryDt { get; set; }
        public string StrHoldDespatchDt { get; set; }
        public string StrHoldInvoiceDt { get; set; }
        public string OptionGSTName { get; set; }
        public int CurrCode { get; set; }
        public string GrpName { get; set; }
        public string AdminUser { get; set; }
        public List<AddOns> AddOnList { get; set; }
        public string AddOnSaved { get; set; }
        public List<AddOns> AddonValueLast { get; set; }
        public string Fld { get; set; }
        public List<SelectListItem> Branches { get; set; }
        public string AppBranch { get; set; }
        public string AppBranchL { get; set; }
        public bool AutoAccCode { get; set; }

        public List<MasterVM> DocumentList { get; set; }
        public string ImageStr { get; set; }
        public string FileName { get; set; }
        public string AllFileStr { get; set; }
        public string FileContStr { get; set; }
        public string FileNameStr { get; set; }
        public byte[] ImageData { get; set; }
        public string FileContent { get; set; }
        public string ContentType { get; set; }
        public string Path { get; set; }
        public int tEmpID { get; set; }
        public bool tempIsDeleted { get; set; }
        public string TableKey { get; set; }
        public string ParentKey { get; set; }
        public string RichNote { get; set; }
        public List<SelectListItem> PriceLists { get; set; }
        public string PriceListL { get; set; }

        public List<SelectListItem> DiscLists { get; set; }
        public string DiscListL { get; set; }
        public string PDiscList { get; set; }
        public string PDiscListName { get; set; }
        public List<SelectListItem> SchemeLists { get; set; }
        public string SchemeListL { get; set; }
        public string SchemeList { get; set; }
        public string SchemeListName { get; set; }

        //New Entites (Required Follow By Rajesh Sir Instructions)Suresh 

        public bool PONumber { get; set; }
        public bool BENumber { get; set; }
        public bool PartyInvoice { get; set; }
        public bool PartyChallan { get; set; }
        public string Collection { get; set; }
        public string Delivery { get; set; }
        public bool AllowtoChange { get; set; }
        public string RelatedTo { get; set; }
        public string RelatedToN { get; set; }
        //public List<SelectListItem> RelatedToList { get; set; }
        public bool Brokr { get; set; }
        public bool Vehicle { get; set; }
        public bool Driver { get; set; }
        public bool AllBranch { get; set; }


        public bool GenerateDefaultCust { get; set; }


        public bool ReferAcReq { get; set; }
        public bool CostCenterTally { get; set; }

    }
    public enum Type
    {
        Broker=1,Vehicle,LR,Truck
    }
}