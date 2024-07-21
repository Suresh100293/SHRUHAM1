using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class PriceListVM
    {
        public int RECORDKEY
        {
            get; set;
        }
        public string Mode { get; set; }
        public string Document { get; set; }
        public string CustomerName { get; set; }
        public string CategoryName { get; set; }
        public string Category { get; set; }
        public string Code { get; set; }
        //public int? Sno { get; set; }
        public double Disc { get; set; }
        public double DiscAmt { get; set; }
        public double MRP { get; set; }
        public Nullable<decimal> MRPDisc { get; set; }
        public Nullable<double> PartyLevel { get; set; }
        public string PartyOption { get; set; }
        public string PartyValue { get; set; }
        public string ProductOption { get; set; }
        public string ProductValue { get; set; }
        public double Rate { get; set; }
        public double RatePer { get; set; }
        public decimal SalesMargin { get; set; }
        public int tempid { get; set; }
        public Nullable<decimal> SalesMaxDisc { get; set; }
        public Nullable<double> SalesMinRate { get; set; }
        public int Sno { get; set; }
        public string TableKey { get; set; }
        public string Type { get; set; }
        public string ENTEREDBY { get; set; }
        public string State { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public List<SelectListItem> ProductOptionList { get; set; }
        public List<SelectListItem> PartyOptionList { get; set; }
        public DateTime? DateModified { get; set; }

        public bool PerValue { get; set; }
        public string PriceListCode { get; set; }
        public string ItemGroup { get; set; }
        public string AppBranch { get; set; }
        public string AppBranch2 { get; set; }
        public string StoreOption { get; set; }
        public string StoreValue { get; set; }
        public string PartyOptionName { get; set; }
        public List<SelectListItem> Branches { get; set; }
        public bool tempIsDeleted { get; set; }
        public bool Status { get; set; }
        public List<PriceListVM> NewPartialItemList { get; set; }
        public List<PriceListVM> NewItemList { get; set; }
        public string SessionFlag { get; set; }
        public string Customer { get; set; }
        public string AcGroup { get; set; }
        public string DocDateStr { get; set; }
        public string Name { get; set; }
        public string EffDateToStr { get; set; }
        public string VerifyDateStr { get; set; }
        public string EffDateFromStr { get; set; }
        public string StateName { get; set; }
        public string AcGroupName { get; set; }
        public string ItemGroupName { get; set; }
        public string ItemName { get; set; }
        public string MainType { get; set; }
        public decimal Disc1 { get; set; }
        public decimal Disc2 { get; set; }
        public decimal Disc3 { get; set; }
        public decimal Disc4 { get; set; }
        public decimal Disc5 { get; set; }
        public decimal Disc6 { get; set; }
        public string AddLess { get; set; }
        public string AddLess1 { get; set; }
        public string AddLess2 { get; set; }
        public string AddLess3 { get; set; }
        public string AddLess4 { get; set; }
        public string AddLess5 { get; set; }
        public string AddLess6 { get; set; }
        public string CalcOn { get; set; }
        public string CalcOn2 { get; set; }
        public string CalcOn3 { get; set; }
        public string CalcOn4 { get; set; }
        public string CalcOn5 { get; set; }
        public string CalcOn6 { get; set; }
        public decimal Taxable { get; set; }
        public string InnerEffDateToStr { get; set; }
        public string InnerEffDateFromStr { get; set; }
        public string ViewDataId { get; set; }
        public bool DiscNotAllowed { get; set; }
        public string HeadPartyOption { get; set; }
        public string HeadState { get; set; }

        public string HeadCustomer { get; set; }
        public string HeadCategory { get; set; }
        public string HeadAcGroup { get; set; }
        public string HeadProductOption { get; set; }
        public string HeadItemGroup { get; set; }
        public string HeadProductValue { get; set; }

        public string HeadStateName { get; set; }
        public string HeadCustomerName { get; set; }
        public string HeadCategoryName { get; set; }
        public string HeadAcGroupName { get; set; }
        public string HeadItemGroupName { get; set; }
        public string HeadProductValueName { get; set; }
        public string HeadPartyValue { get; set; }
        public string PartyValueName { get; set; }
        public string ProductValueName { get; set; }

        public string DiscCaption1 { get; set; }
        public string DiscCaption2 { get; set; }
        public string DiscCaption3 { get; set; }
        public string DiscCaption4 { get; set; }
        public string DiscCaption5 { get; set; }
        public string DiscCaption6 { get; set; }
        public string HeadSplItemGroup { get; set; }
        public string DiscCaption { get; set; }
        public string ChangeLog { get; set; }
        public string SearchBy { get; set; }
        public string Header { get; set; }
        public string AddLessName1 { get; set; }
        public string AddLessName2 { get; set; }
        public string AddLessName3 { get; set; }
        public string AddLessName4 { get; set; }
        public string AddLessName5 { get; set; }
        public string AddLessName6 { get; set; }
        public string PriceListDiscCode { get; set; }
        public bool ApplyBeforeDisc { get; set; }
        public string ClassValues1 { get; set; }
        public string ClassValues2 { get; set; }
        public string Unit { get; set; }

    }
}