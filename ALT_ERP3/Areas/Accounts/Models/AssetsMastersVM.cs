using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class AssetsMastersVM
    {
        // iX9: Field Structure of FixedAsset
        public int FixedAsset_RECORDKEY { get; set; }
        public string FixedAsset_AccountCode { get; set; }
        public string FixedAsset_AccountGrp { get; set; }
        public decimal FixedAsset_AccumDepr { get; set; }
        public decimal FixedAsset_AccumDepr10 { get; set; }
        public decimal FixedAsset_AccumDepr11 { get; set; }
        public decimal FixedAsset_AccumDepr2 { get; set; }
        public decimal FixedAsset_AccumDepr3 { get; set; }
        public decimal FixedAsset_AccumDepr4 { get; set; }
        public decimal FixedAsset_AccumDepr5 { get; set; }
        public decimal FixedAsset_AccumDepr6 { get; set; }
        public decimal FixedAsset_AccumDepr7 { get; set; }
        public decimal FixedAsset_AccumDepr8 { get; set; }
        public decimal FixedAsset_AccumDepr9 { get; set; }
        public string FixedAsset_AcqType { get; set; }
        public string FixedAsset_AgencyName { get; set; }
        public System.DateTime FixedAsset_AMCFrom { get; set; }
        public string FixedAsset_AMCFromVM { get; set; }
        public decimal FixedAsset_AMCRate { get; set; }
        public System.DateTime FixedAsset_AMCTo { get; set; }
        public string FixedAsset_AMCToVM { get; set; }
        public int FixedAsset_AssetCategory { get; set; }
        public string FixedAsset_AssetStatus { get; set; }
        public decimal FixedAsset_BookValue { get; set; }
        public string FixedAsset_Code { get; set; }
        public string FixedAsset_CompreDet { get; set; }
        public bool FixedAsset_Comprehensive { get; set; }
        public decimal FixedAsset_Cost { get; set; }
        public int FixedAsset_CostCentre { get; set; }
        public string FixedAsset_DeprAccount { get; set; }
        public bool FixedAsset_Depreciable { get; set; }
        public System.DateTime FixedAsset_DeprEndDt { get; set; }
        public string FixedAsset_DeprEndDtVM { get; set; }
        public string FixedAsset_DeprMethod { get; set; }
        public decimal FixedAsset_DeprRate { get; set; }
        public decimal FixedAsset_DeprRateIT { get; set; }
        public bool FixedAsset_DeprSinceInUse { get; set; }
        public System.DateTime FixedAsset_ExpDate { get; set; }
        public string FixedAsset_ExpDateVM { get; set; }
        public bool FixedAsset_FreeLabour { get; set; }
        public bool FixedAsset_FreeParts { get; set; }
        public string FixedAsset_InsuranceComp { get; set; }
        public System.DateTime FixedAsset_LeaseFrom { get; set; }
        public string FixedAsset_LeaseFromVM { get; set; }
        public decimal FixedAsset_LeaseRate { get; set; }
        public System.DateTime FixedAsset_LeaseTo { get; set; }
        public string FixedAsset_LeaseToVM { get; set; }
        public System.DateTime FixedAsset_LicIssueDt { get; set; }
        public string FixedAsset_LicIssueDtVM { get; set; }
        public double FixedAsset_LifeMonths { get; set; }
        public decimal FixedAsset_MinCharges { get; set; }
        public string FixedAsset_Name { get; set; }
        public string FixedAsset_Narr { get; set; }
        public decimal FixedAsset_NextDeprAmt { get; set; }
        public System.DateTime FixedAsset_NextPremiumDue { get; set; }
        public string FixedAsset_NextPremiumDueVM { get; set; }
        public bool FixedAsset_OnSite { get; set; }
        public string FixedAsset_ParentAsset { get; set; }
        public string FixedAsset_ParentStatus { get; set; }
        public int FixedAsset_Pickup { get; set; }
        public System.DateTime FixedAsset_policyDate { get; set; }
        public string FixedAsset_policyDateVM { get; set; }
        public string FixedAsset_PolicyNo { get; set; }
        public decimal FixedAsset_PremiumAmt { get; set; }
        public string FixedAsset_Product { get; set; }
        public System.DateTime FixedAsset_PurchDate { get; set; }
        public string FixedAsset_PurchDateVM { get; set; }
        public string FixedAsset_RegNo { get; set; }
        public string FixedAsset_TagNo { get; set; }
        public System.DateTime FixedAsset_UseDate { get; set; }
        public string FixedAsset_UseDateVM { get; set; }
        public decimal FixedAsset_ValueInsured { get; set; }
        public System.DateTime FixedAsset_WarExpDate { get; set; }
        public string FixedAsset_WarExpDateVM { get; set; }
        public int FixedAsset_Warranty { get; set; }
        public string FixedAsset_WarrantyNote { get; set; }
        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public List<SelectListItem> DeprMethodList { get; set; }
        public List<SelectListItem> AcqTypeList { get; set; }
        public List<SelectListItem> AssetStatusList { get; set; }
        public List<SelectListItem> ParentStatusList { get; set; }
        public string DeprAccountName { get; set; }
        public string ProductName { get; set; }
        public string AccountCodeName { get; set; }
        public string AssetCategoryName { get; set; }
        public string CostCentreName { get; set; }
        public string ParentAssetName { get; set; }

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
}