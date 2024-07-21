using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class LRSetupVM
    {

        #region Classification Setup

        public bool Class_CurrDatetOnlyreq  { get; set; }
        public bool Class_BackDateAllow     { get; set; }
        public int  Class_BackDaysUpto      { get; set; }
        public bool Class_ForwardDateAllow  { get; set; }
        public int  Class_ForwardDaysUpto   { get; set; }
        public bool Class_BranchwiseSrlReq  { get; set; }
        public bool Class_YearwiseSrlReq    { get; set; }
        public bool Class_CetralisedSrlReq  { get; set; }
        public int  Class_Srl               { get; set; }

        public bool YearwiseManualSrlReq { get; set; }
        public bool CetralisedManualSrlReq { get; set; }
        #endregion


        public string TrnMode { get; set; }

        public bool LrAutomatic { get; set; }
        public bool LrManual { get; set; }
        public bool Both { get; set; }

        public string FooterDetails1 { get; set; }
        public string FooterDetails2 { get; set; }
        public string FooterDetails3 { get; set; }
        public string FooterDetails4 { get; set; }

        public bool EditReq { get; set; }
        public bool DeleteReq { get; set; }
        public int EditHours { get; set; }
        public int EditMinutes { get; set; }
        public int DeleteHours { get; set; }
        public int DeleteMinutes { get; set; }
        public bool DispachLREdit { get; set; }
        public bool BillLREdit { get; set; }
        public bool GetAutoTripNo { get; set; }

        public List<SelectListItem> PrintFormats { get; set; }
        public string PrintFormat { get; set; }
        public string PrintFormatsL { get; set; }

        public List<SelectListItem> Users { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }

        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime Hour { get; set; }


        public int defaultQty { get; set; }

        public string LRType { get; set; }
        public string LRType_Name { get; set; }

        public bool Topay { get; set; }
        public bool Paid { get; set; }
        public Consignor_Consignee TopayCustomer { get; set; }
        public Consignor_Consignee PaidCustomer { get; set; }
        public string DefaultToPay { get; set; }
        public string DefaultToPay_Name { get; set; }
        public string DefaultPaid { get; set; }
        public string DefaultPaid_Name { get; set; }


        public string BillBranch { get; set; }
        public string BillBranch_Name { get; set; }

        public string Unit { get; set; }
        public string Unit_Name { get; set; }

        public string ChrType { get; set; }
        public string ChrType_Name { get; set; }

        public string ServiceType { get; set; }
        public string ServiceType_Name { get; set; }
        
        public bool CurrentBranch { get; set; }
        public bool AllBranch { get; set; }


        public int Declare_Value { get; set; }
        public bool GST { get; set; }
        public bool Eway_Bill { get; set; }

        public string GST_Ticklr { get; set; }
        public string EWB_Ticklr { get; set; }

        public bool Vehicle { get; set; }

        public bool ParticularFlag { get; set; }
        public string Particular { get; set; }
        public string Particular_Name { get; set; }

        public bool Party_Challan { get; set; }
        public bool Party_Invoice { get; set; }

        public string Colln { get; set; }
        public string Del { get; set; }
        public bool DeclareValueZero { get; set; }

        public bool Charges { get; set; }
        public bool ActWeightReq { get; set; }

        public bool CheckManualLR { get; set; }

        public bool FetchContract { get; set; }
        public bool GenralContract { get; set; }

        public bool FillFromCurr { get; set; }

        //public bool LrAutomatic { get; set; }
        //public bool LrManual { get; set; }
        //public bool Both { get; set; }

        //public bool Date { get; set; }
        //public int BeforeDays { get; set; }
        //public int AfterDays { get; set; }

        //public bool LoginBase { get; set; }
        //public bool All { get; set; }

        //public bool PartyChallanAutomatic { get; set; }
        //public bool PartyChallanOptional { get; set; }

        //public bool PartyInvoiceAutomatic { get; set; }
        //public bool PartyInvoiceOptional { get; set; }

        //public bool EWayBillAutomatic { get; set; }
        //public bool EWayBillOptional { get; set; }

        //public bool VEhicleAutomatic { get; set; }
        //public bool VEhicleOptional { get; set; }

        //public int Declare_Value { get; set; }
        //public bool GST { get; set; }
        //public bool EwayBill { get; set; }
        //public bool Vehicle { get; set; }

        // iX9: Common default Fields
        public string Document { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Mode { get; set; }
        // iX9: used while passing parameters from MasterGrid to Option and Back 
        public string MainType { get; set; }
        public string Controller2 { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
        
    }
    
    public enum Consignor_Consignee
    {
        NOTRequired,Consignor, Consignee
    }
}