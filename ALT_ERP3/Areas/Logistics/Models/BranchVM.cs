using EntitiModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class BranchVM
    {
        #region ChangePlantBUDetailsVM Properties
        // iX9: Field Structure of TfatBranch
        public int TfatBranch_RECORDKEY { get; set; }
        public string TfatBranch_aAuthno { get; set; }
        public string TfatBranch_Account { get; set; }
        public string TfatBranch_aCstNo { get; set; }
        public string TfatBranch_Addrl1 { get; set; }
        public string TfatBranch_Addrl2 { get; set; }
        public string TfatBranch_Addrl3 { get; set; }
        public string TfatBranch_Addrl4 { get; set; }
        public string TfatBranch_aFax { get; set; }
        public string TfatBranch_aLstno { get; set; }
        public string TfatBranch_aPin { get; set; }
        public string TfatBranch_Business { get; set; }
        public string TfatBranch_CINNo { get; set; }
        public string TfatBranch_City { get; set; }
        public string TfatBranch_Code { get; set; }
        public string TfatBranch_CompCode { get; set; }
        public string TfatBranch_Country { get; set; }
        public int TfatBranch_CurrDec { get; set; }
        public int TfatBranch_CurrName { get; set; }
        public string TfatBranch_Flag { get; set; }
        public bool TfatBranch_Fri { get; set; }
        public bool TfatBranch_gp_AllowDiscAP { get; set; }
        public bool TfatBranch_gp_AllowDiscAS { get; set; }
        public bool TfatBranch_gp_AllowDiscPP { get; set; }
        public bool TfatBranch_gp_AllowDiscPS { get; set; }
        public bool TfatBranch_gp_AllowEditDelete { get; set; }
        public bool TfatBranch_gp_AllowRateP { get; set; }
        public bool TfatBranch_gp_AllowRateS { get; set; }
        public bool TfatBranch_gp_AutoAccCode { get; set; }
        public byte TfatBranch_gp_AutoAccLength { get; set; }
        public byte TfatBranch_gp_AutoAccStyle { get; set; }
        public bool TfatBranch_gp_Batch { get; set; }
        public bool TfatBranch_gp_BillStock { get; set; }
        public bool TfatBranch_gp_BIN { get; set; }
        public bool TfatBranch_gp_CashLimit { get; set; }
        public double TfatBranch_gp_CashLimitAmt { get; set; }
        public bool TfatBranch_gp_CashLimitWarn { get; set; }
        public bool TfatBranch_gp_CLStock { get; set; }
        public bool TfatBranch_gp_DiscAP { get; set; }
        public bool TfatBranch_gp_DiscAS { get; set; }
        public bool TfatBranch_gp_DiscPP { get; set; }
        public bool TfatBranch_gp_DiscPS { get; set; }
        public bool TfatBranch_gp_EnableParty { get; set; }
        public bool TfatBranch_gp_GINQty { get; set; }
        public int TfatBranch_gp_GSTStyle { get; set; }
        public bool TfatBranch_gp_GSTSupply { get; set; }
        public string TfatBranch_gp_Holiday1 { get; set; }
        public string TfatBranch_gp_Holiday2 { get; set; }
        public bool TfatBranch_gp_LocWiseTax { get; set; }
        public bool TfatBranch_gp_MultiUnit { get; set; }
        public bool TfatBranch_gp_NegStock { get; set; }
        public bool TfatBranch_gp_NegStockAsOn { get; set; }
        public bool TfatBranch_gp_NegWarn { get; set; }
        public bool TfatBranch_gp_PostP { get; set; }
        public bool TfatBranch_gp_PSP { get; set; }
        public bool TfatBranch_gp_PurchPostTDS { get; set; }
        public bool TfatBranch_gp_QtnA { get; set; }
        public System.DateTime TfatBranch_gp_RCMDate { get; set; }
        public string TfatBranch_gp_RCMDateVM { get; set; }
        public byte TfatBranch_gp_RoundVAT { get; set; }
        public bool TfatBranch_gp_Serial { get; set; }
        public bool TfatBranch_gp_SEZChargeParty { get; set; }
        public bool TfatBranch_gp_SOPropagation { get; set; }
        public bool TfatBranch_gp_SPAdjForce { get; set; }
        public byte TfatBranch_gp_VatDecP { get; set; }
        public byte TfatBranch_gp_VatDecS { get; set; }
        public string TfatBranch_gp_VATGST { get; set; }
        public string TfatBranch_Grp { get; set; }
        public string TfatBranch_GSTNo { get; set; }
        public bool TfatBranch_LastBranch { get; set; }
        public bool TfatBranch_LastUpdated { get; set; }
        public string TfatBranch_Licence2 { get; set; }
        public int TfatBranch_LocationCode { get; set; }
        public bool TfatBranch_LogIn { get; set; }
        public bool TfatBranch_Mon { get; set; }
        public string TfatBranch_Name { get; set; }
        public string TfatBranch_PanNo { get; set; }
        public int TfatBranch_PCCode { get; set; }
        public string TfatBranch_PrintInfo { get; set; }
        public string TfatBranch_ProxyServer { get; set; }
        public bool TfatBranch_Sat { get; set; }
        public string TfatBranch_SMSCaption { get; set; }
        public string TfatBranch_SMSPass { get; set; }
        public bool TfatBranch_SMSPrefix { get; set; }
        public string TfatBranch_SMSURL { get; set; }
        public string TfatBranch_SMSUserId { get; set; }
        public string TfatBranch_State { get; set; }
        public bool TfatBranch_Sun { get; set; }
        public string TfatBranch_TDSAuthorise { get; set; }
        public string TfatBranch_TDSCir { get; set; }
        public string TfatBranch_TDSOffice { get; set; }
        public string TfatBranch_TDSReg { get; set; }
        public string TfatBranch_Tel1 { get; set; }
        public string TfatBranch_Tel2 { get; set; }
        public string TfatBranch_Tel3 { get; set; }
        public string TfatBranch_Tel4 { get; set; }
        public bool TfatBranch_Thu { get; set; }
        public decimal TfatBranch_TimeDiff { get; set; }
        public string TfatBranch_TINNumber { get; set; }
        public bool TfatBranch_Tue { get; set; }
        public string TfatBranch_Users { get; set; }
        public string TfatBranch_VATReg { get; set; }
        public bool TfatBranch_Wed { get; set; }
        public string TfatBranch_www { get; set; }
        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public List<SelectListItem> BusinessList { get; set; }
        public List<SelectListItem> gp_VATGSTList { get; set; }
        public List<SelectListItem> gp_AutoAccStyleList { get; set; }
        public string GrpName { get; set; }
        public string CompCodeName { get; set; }
        public string LocationCodeName { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
        public string CityName { get; set; }
        public string CurrNameName { get; set; }
        public string AccountName { get; set; }
        public List<SelectListItem> UsersMultiX { get; set; }
        public string UsersItemX { get; set; }

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
        public bool TfatBranch_gp_SONoDupl { get; set; }
        public bool TfatBranch_DuplicateItemName { get; set; }
        public bool gp_AddonBased { get; set; }
        public bool gp_AddonBasedDescr { get; set; }
        public string gp_AddonSepCode { get; set; }
        public string gp_AddonSepName { get; set; }
        public bool gp_ItemAutoCode { get; set; }
        public bool gp_ItemAutoDescr { get; set; }
        public bool gp_ItemCodeStyle { get; set; }
        public bool gp_ItemDescrStyle { get; set; }
        public bool gp_ItemPrefixName { get; set; }
        public string gp_Length { get; set; }
        public bool gp_OrdIncludeRet { get; set; }
        public bool gp_CLStockAddOrder { get; set; }
        public bool Status { get; set; }
        public bool AllTime { get; set; }
        public string Category { get; set; }
        public string TempPCode { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }

        #endregion
        #region Own Create
        public List<SelectListItem> TempPCategoryList { get; set; }
        public List<SelectListItem> CategoryList { get; set; }
        public List<SelectListItem> TypeList { get; set; }


        public string ParentName { get; set; }
        public string State_Name { get; set; }
        public string GSTState { get; set; }
        public string GSTState_Name { get; set; }
        public string ParentCode { get; set; }
        public string ParentCategory { get; set; }
        public string WorkingHoursFrom { get; set; }
        public string WorkingHoursTo { get; set; }
        
        public string VehicleActivity { get; set; }
        public bool Inactive { get; set; }


        public string TfatBranch_Email { get; set; }
        public string BCCTo { get; set; }
        public string CCTo { get; set; }
        public string SMTPServer { get; set; }
        public string SMTPUser { get; set; }
        public string SMTPPassword { get; set; }
        public Nullable<int> SMTPPort { get; set; }
        public bool BranchMail { get; set; }
        public bool LocalMail { get; set; }
        #endregion
    }

    public class NFlatObject
    {
        public bool Status;
        public string Id { get; set; }
        public string ParentId { get; set; }
        public string data { get; set; }
        public bool isSelected { get; set; }
        public string Category { get; set; }
        //public FlatObject(string name, int id, int parentId)
        //{
        //    data = name;
        //    Id = id;
        //    ParentId = parentId;
        //}
    }

    public class NRecursiveObject
    {
        public string data { get; set; }
        public string id { get; set; }
        public NFlatTreeAttribute attr { get; set; }
        public List<NRecursiveObject> children { get; set; }
    }

    public class NFlatTreeAttribute
    {
        public string id;
        public bool selected;
    }

    public enum EnumCategory
    {
        Zone = 1, Branch = 2,SubBranch=3,Area=4
    }
}