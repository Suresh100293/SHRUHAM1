using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class ChangeCompanyDetailsVM
    {
        // iX9: Field Structure of Eway Bill
        public string EwayEmail { get; set; }
        public string EwayUsername { get; set; }
        public string EwayPass { get; set; }
        public string EwayClientID { get; set; }
        public string EwayClientSecret { get; set; }
        public string EwayGSTIn { get; set; }

        // iX9: Field Structure of Einvoice
        public string EInvoiceID { get; set; }
        public string EInvoicePassword { get; set; }
        public string EInvoiceGSTNo { get; set; }
        //public string EwayClientID { get; set; }
        //public string EwayClientSecret { get; set; }
        //public string EwayGSTIn { get; set; }


        // iX9: Field Structure of SMS
        public string TfatComp_SSMSURL { get; set; }
        public string TfatComp_UserNameWithValue { get; set; }
        public string TfatComp_PasswordithValue { get; set; }
        public string TfatComp_IDWithValue { get; set; }
        public string TfatComp_Para1 { get; set; }
        public string TfatComp_Para2 { get; set; }
        public string TfatComp_Para3 { get; set; }
        public string TfatComp_Para4 { get; set; }
        public string TfatComp_Para5 { get; set; }
        
        // iX9: Field Structure of VehicleTracking
        public List<SelectListItem> VehicleList { get; set; }
        public int tempId { get; set; }
        public string TfatComp_VehicleTrackURL { get; set; }
        public string TfatComp_VehicleTrackUserNameWithValue { get; set; }
        public string TfatComp_VehicleTrackPasswordithValue { get; set; }
        public string TfatComp_VehicleTrackPara1 { get; set; }
        public string TfatComp_VehicleTrackPara2 { get; set; }
        public string TfatComp_VehicleTrackPara3 { get; set; }
        public string TfatComp_VehicleTrackPara4 { get; set; }
        public string TfatComp_VehicleTrackPara5 { get; set; }
        public string TfatComp_VehicleList { get; set; }
        public string TfatComp_VehicleListL { get; set; }

        public List<ChangeCompanyDetailsVM> VehicleTrackinglist { get; set; }

        // iX9: Field Structure of TfatComp
        public int TfatComp_RECORDKEY { get; set; }
        public string TfatComp_Adrl1 { get; set; }
        public string TfatComp_Adrl2 { get; set; }
        public string TfatComp_Adrl3 { get; set; }
        public string TfatComp_Adrl4 { get; set; }
        public decimal TfatComp_AuthCap { get; set; }
        public string TfatComp_AuthNo { get; set; }
        public string TfatComp_Business { get; set; }
        public string TfatComp_CINNumber { get; set; }
        public string TfatComp_City { get; set; }
        public string TfatComp_Code { get; set; }
        public string TfatComp_CompanyLogo { get; set; }
        public byte TfatComp_CompanyType { get; set; }
        public string TfatComp_CompInfo { get; set; }
        public string TfatComp_Const { get; set; }
        public string TfatComp_Country { get; set; }
        public string TfatComp_CSTNo { get; set; }
        public string TfatComp_DDOCode { get; set; }
        public string TfatComp_DDOReg { get; set; }
        public string TfatComp_DeductorType { get; set; }
        public string TfatComp_Email { get; set; }
        public string TfatComp_fax { get; set; }
        public string TfatComp_GSTNo { get; set; }
        public string TfatComp_LastBranch { get; set; }
        public string TfatComp_LastPeriod { get; set; }
        public string TfatComp_Licence1 { get; set; }
        public string TfatComp_Licence2 { get; set; }
        public string TfatComp_LstNo { get; set; }
        public int TfatComp_Ministry { get; set; }
        public string TfatComp_Name { get; set; }
        public string TfatComp_Nature { get; set; }
        public string TfatComp_PAN { get; set; }
        public string TfatComp_PAOCode { get; set; }
        public string TfatComp_PAOReg { get; set; }
        public bool TfatComp_PFFix { get; set; }
        public bool TfatComp_UseHSNMaster { get; set; }
        public string TfatComp_PFupLimit { get; set; }
        public string TfatComp_Pin { get; set; }
        public string TfatComp_SMSURL { get; set; }
        public string TfatComp_State { get; set; }
        public string TfatComp_STDCode { get; set; }
        public string TfatComp_Taluka { get; set; }
        public string TfatComp_TDSCir { get; set; }
        public string TfatComp_TDSReg { get; set; }
        public string TfatComp_Tel1 { get; set; }
        public string TfatComp_Tel2 { get; set; }
        public string TfatComp_Tel3 { get; set; }
        public string TfatComp_Tel4 { get; set; }
        public string TfatComp_TINNo { get; set; }
        public string TfatComp_USERPass { get; set; }
        public string TfatComp_Users { get; set; }
        public string TfatComp_VATReg { get; set; }
        public string TfatComp_www { get; set; }
        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public List<SelectListItem> BusinessList { get; set; }
        public List<SelectListItem> ConstList { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
        public string CityName { get; set; }
        public List<SelectListItem> UsersMultiX { get; set; }
        public string UsersItemX { get; set; }
        public string TransportID { get; set; }



        public string TfatPass_Email { get; set; }
        public string TfatPass_SMTPServer { get; set; }
        public string TfatPass_SMTPUser { get; set; }
        public string TfatPass_SMTPPassword { get; set; }
        public string TfatPass_BCCTo { get; set; }
        public string TfatPass_CCTo { get; set; }
        public int TfatPass_SMTPPort { get; set; }
        public bool TfatPass_GlobalMail { get; set; }



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
        public string SessionFlag { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
    
}