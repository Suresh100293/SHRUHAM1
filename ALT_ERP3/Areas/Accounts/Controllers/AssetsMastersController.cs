using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class AssetsMastersController : BaseController
    {
        //private //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        //private IBusinessCommon mBuss = new BusinessCommon();
        private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mAUTHORISE = "A00";
        private static string mdocument = "";

        #region GetLists
        public List<SelectListItem> GetDeprMethodList()
        {
            List<SelectListItem> CallDeprMethodList = new List<SelectListItem>();
            CallDeprMethodList.Add(new SelectListItem { Value = "S", Text = "Staright Line" });
            CallDeprMethodList.Add(new SelectListItem { Value = "W", Text = "WDV" });
            return CallDeprMethodList;
        }
        public List<SelectListItem> GetAcqTypeList()
        {
            List<SelectListItem> CallAcqTypeList = new List<SelectListItem>();
            CallAcqTypeList.Add(new SelectListItem { Value = "C", Text = "Constructed" });
            CallAcqTypeList.Add(new SelectListItem { Value = "D", Text = "Donated" });
            CallAcqTypeList.Add(new SelectListItem { Value = "L", Text = "Leased" });
            CallAcqTypeList.Add(new SelectListItem { Value = "P", Text = "Purchased" });
            CallAcqTypeList.Add(new SelectListItem { Value = "T", Text = "Trading" });
            CallAcqTypeList.Add(new SelectListItem { Value = "T", Text = "Transfer" });
            return CallAcqTypeList;
        }
        public List<SelectListItem> GetAssetStatusList()
        {
            List<SelectListItem> CallAssetStatusList = new List<SelectListItem>();
            CallAssetStatusList.Add(new SelectListItem { Value = "B", Text = "Budgeted" });
            CallAssetStatusList.Add(new SelectListItem { Value = "C", Text = "Commitment" });
            CallAssetStatusList.Add(new SelectListItem { Value = "D", Text = "Desposed" });
            CallAssetStatusList.Add(new SelectListItem { Value = "I", Text = "In Service" });
            CallAssetStatusList.Add(new SelectListItem { Value = "M", Text = "Mothballed" });
            CallAssetStatusList.Add(new SelectListItem { Value = "R", Text = "Received" });
            CallAssetStatusList.Add(new SelectListItem { Value = "R", Text = "Requisitioned" });
            CallAssetStatusList.Add(new SelectListItem { Value = "T", Text = "Transferred" });
            CallAssetStatusList.Add(new SelectListItem { Value = "W", Text = "WIP" });
            return CallAssetStatusList;
        }
        public List<SelectListItem> GetParentStatusList()
        {
            List<SelectListItem> CallParentStatusList = new List<SelectListItem>();
            CallParentStatusList.Add(new SelectListItem { Value = "N", Text = "None" });
            CallParentStatusList.Add(new SelectListItem { Value = "C", Text = "Child" });
            CallParentStatusList.Add(new SelectListItem { Value = "P", Text = "Parent" });
            return CallParentStatusList;
        }
        public JsonResult AutoCompleteDeprAccount(string term)
        {
            return Json((from m in ctxTFAT.Master
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteProduct(string term)
        {
            return Json((from m in ctxTFAT.ItemMaster
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteAccountCode(string term)
        {
            return Json((from m in ctxTFAT.Master
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteAssetCategory(string term)
        {
            return Json((from m in ctxTFAT.AssetCategory
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteCostCentre(string term)
        {
            return Json((from m in ctxTFAT.CostCentre
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteParentAsset(string term)
        {
            return Json((from m in ctxTFAT.FixedAsset
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        #endregion GetLists

        // GET: Accounts/AssetsMasters
        public ActionResult Index(AssetsMastersVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mModel.FixedAsset_WarExpDate = DateTime.Now;
            mModel.FixedAsset_LicIssueDt = DateTime.Now;
            mModel.FixedAsset_ExpDate = DateTime.Now;
            mModel.FixedAsset_PurchDate = DateTime.Now;
            mModel.FixedAsset_UseDate = DateTime.Now;
            mModel.FixedAsset_DeprEndDt = DateTime.Now;
            mModel.FixedAsset_policyDate = DateTime.Now;
            mModel.FixedAsset_LeaseFrom = DateTime.Now;
            mModel.FixedAsset_NextPremiumDue = DateTime.Now;
            mModel.FixedAsset_LeaseTo = DateTime.Now;
            mModel.FixedAsset_AMCFrom = DateTime.Now;
            mModel.FixedAsset_AMCTo = DateTime.Now;
            mModel.DeprMethodList = GetDeprMethodList();
            mModel.AcqTypeList = GetAcqTypeList();
            mModel.AssetStatusList = GetAssetStatusList();
            mModel.ParentStatusList = GetParentStatusList();
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                mdocument = mModel.Document;
                var mList = ctxTFAT.FixedAsset.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                var mDeprAccount = ctxTFAT.Master.Where(x => x.Code == mList.DeprAccount).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mProduct = ctxTFAT.ItemMaster.Where(x => x.Code == mList.Product).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mAccountCode = ctxTFAT.Master.Where(x => x.Code == mList.AccountCode).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mAssetCategory = ctxTFAT.AssetCategory.Where(x => x.Code == mList.AssetCategory).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mCostCentre = ctxTFAT.CostCentre.Where(x => x.Code == mList.CostCentre).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mParentAsset = ctxTFAT.FixedAsset.Where(x => x.Code == mList.ParentAsset).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                mModel.FixedAsset_DeprAccount = mDeprAccount != null ? mDeprAccount.Code.ToString() : "";
                mModel.DeprAccountName = mDeprAccount != null ? mDeprAccount.Name : "";
                mModel.FixedAsset_Product = mProduct != null ? mProduct.Code.ToString() : "";
                mModel.ProductName = mProduct != null ? mProduct.Name : "";
                mModel.FixedAsset_AccountCode = mAccountCode != null ? mAccountCode.Code.ToString() : "";
                mModel.AccountCodeName = mAccountCode != null ? mAccountCode.Name : "";
                mModel.FixedAsset_AssetCategory = mAssetCategory != null ? mAssetCategory.Code : 0;
                mModel.AssetCategoryName = mAssetCategory != null ? mAssetCategory.Name : "";
                mModel.FixedAsset_CostCentre = mCostCentre != null ? mCostCentre.Code : 0;
                mModel.CostCentreName = mCostCentre != null ? mCostCentre.Name : "";
                mModel.FixedAsset_ParentAsset = mParentAsset != null ? mParentAsset.Code.ToString() : "";
                mModel.ParentAssetName = mParentAsset != null ? mParentAsset.Name : "";
                mModel.FixedAsset_Depreciable = mList.Depreciable;
                mModel.FixedAsset_Code = mList.Code;
                mModel.FixedAsset_Warranty = mList.Warranty != null ? mList.Warranty.Value : 0;
                mModel.FixedAsset_AgencyName = mList.AgencyName;
                mModel.FixedAsset_WarExpDate = mList.WarExpDate != null ? mList.WarExpDate.Value : DateTime.Now;
                mModel.FixedAsset_Name = mList.Name;
                mModel.FixedAsset_RegNo = mList.RegNo;
                mModel.FixedAsset_TagNo = mList.TagNo;
                mModel.FixedAsset_OnSite = mList.OnSite;
                mModel.FixedAsset_AccumDepr = mList.AccumDepr != null ? mList.AccumDepr.Value : 0;
                mModel.FixedAsset_MinCharges = mList.MinCharges != null ? mList.MinCharges.Value : 0;
                mModel.FixedAsset_FreeParts = mList.FreeParts;
                mModel.FixedAsset_LicIssueDt = mList.LicIssueDt != null ? mList.LicIssueDt.Value : DateTime.Now;
                mModel.FixedAsset_DeprMethod = mList.DeprMethod;
                mModel.FixedAsset_FreeLabour = mList.FreeLabour;
                mModel.FixedAsset_ExpDate = mList.ExpDate != null ? mList.ExpDate.Value : DateTime.Now;
                mModel.FixedAsset_BookValue = mList.BookValue != null ? mList.BookValue.Value : 0;
                mModel.FixedAsset_Cost = mList.Cost != null ? mList.Cost.Value : 0;
                mModel.FixedAsset_Pickup = mList.Pickup != null ? mList.Pickup.Value : 0;
                mModel.FixedAsset_PurchDate = mList.PurchDate != null ? mList.PurchDate.Value : DateTime.Now;
                mModel.FixedAsset_DeprRate = mList.DeprRate != null ? mList.DeprRate.Value : 0;
                mModel.FixedAsset_DeprRateIT = mList.DeprRateIT != null ? mList.DeprRateIT.Value : 0;
                mModel.FixedAsset_UseDate = mList.UseDate != null ? mList.UseDate.Value : DateTime.Now;
                mModel.FixedAsset_DeprEndDt = mList.DeprEndDt != null ? mList.DeprEndDt.Value : DateTime.Now;
                mModel.FixedAsset_AcqType = mList.AcqType;
                mModel.FixedAsset_InsuranceComp = mList.InsuranceComp;
                mModel.FixedAsset_DeprSinceInUse = mList.DeprSinceInUse;
                mModel.FixedAsset_AssetStatus = mList.AssetStatus;
                mModel.FixedAsset_WarrantyNote = mList.WarrantyNote;
                mModel.FixedAsset_PolicyNo = mList.PolicyNo;
                mModel.FixedAsset_NextDeprAmt = mList.NextDeprAmt != null ? mList.NextDeprAmt.Value : 0;
                mModel.FixedAsset_LifeMonths = mList.LifeMonths != null ? mList.LifeMonths.Value : 0;
                mModel.FixedAsset_policyDate = mList.policyDate != null ? mList.policyDate.Value : DateTime.Now;
                mModel.FixedAsset_ValueInsured = mList.ValueInsured != null ? mList.ValueInsured.Value : 0;
                mModel.FixedAsset_Narr = mList.Narr;
                mModel.FixedAsset_ParentStatus = mList.ParentStatus;
                mModel.FixedAsset_LeaseFrom = mList.LeaseFrom != null ? mList.LeaseFrom.Value : DateTime.Now;
                mModel.FixedAsset_PremiumAmt = mList.PremiumAmt != null ? mList.PremiumAmt.Value : 0;
                mModel.FixedAsset_NextPremiumDue = mList.NextPremiumDue != null ? mList.NextPremiumDue.Value : DateTime.Now;
                mModel.FixedAsset_LeaseTo = mList.LeaseTo != null ? mList.LeaseTo.Value : DateTime.Now;
                mModel.FixedAsset_LeaseRate = mList.LeaseRate != null ? mList.LeaseRate.Value : 0;
                mModel.FixedAsset_Comprehensive = mList.Comprehensive;
                mModel.FixedAsset_AMCRate = mList.AMCRate != null ? mList.AMCRate.Value : 0;
                mModel.FixedAsset_AMCFrom = mList.AMCFrom != null ? mList.AMCFrom.Value : DateTime.Now;
                mModel.FixedAsset_AMCTo = mList.AMCTo != null ? mList.AMCTo.Value : DateTime.Now;
                mModel.FixedAsset_CompreDet = mList.CompreDet;
            }
            else
            {
                mModel.FixedAsset_AccountCode = "";
                mModel.FixedAsset_AccountGrp = "";
                mModel.FixedAsset_AccumDepr = 0;
                mModel.FixedAsset_AccumDepr10 = 0;
                mModel.FixedAsset_AccumDepr11 = 0;
                mModel.FixedAsset_AccumDepr2 = 0;
                mModel.FixedAsset_AccumDepr3 = 0;
                mModel.FixedAsset_AccumDepr4 = 0;
                mModel.FixedAsset_AccumDepr5 = 0;
                mModel.FixedAsset_AccumDepr6 = 0;
                mModel.FixedAsset_AccumDepr7 = 0;
                mModel.FixedAsset_AccumDepr8 = 0;
                mModel.FixedAsset_AccumDepr9 = 0;
                mModel.FixedAsset_AcqType = "";
                mModel.FixedAsset_AgencyName = "";
                mModel.FixedAsset_AMCFrom = System.DateTime.Now;
                mModel.FixedAsset_AMCRate = 0;
                mModel.FixedAsset_AMCTo = System.DateTime.Now;
                mModel.FixedAsset_AssetCategory = 0;
                mModel.FixedAsset_AssetStatus = "";
                mModel.FixedAsset_BookValue = 0;
                mModel.FixedAsset_Code = "";
                mModel.FixedAsset_CompreDet = "";
                mModel.FixedAsset_Comprehensive = false;
                mModel.FixedAsset_Cost = 0;
                mModel.FixedAsset_CostCentre = 0;
                mModel.FixedAsset_DeprAccount = "";
                mModel.FixedAsset_Depreciable = false;
                mModel.FixedAsset_DeprEndDt = System.DateTime.Now;
                mModel.FixedAsset_DeprMethod = "";
                mModel.FixedAsset_DeprRate = 0;
                mModel.FixedAsset_DeprRateIT = 0;
                mModel.FixedAsset_DeprSinceInUse = false;
                mModel.FixedAsset_ExpDate = System.DateTime.Now;
                mModel.FixedAsset_FreeLabour = false;
                mModel.FixedAsset_FreeParts = false;
                mModel.FixedAsset_InsuranceComp = "";
                mModel.FixedAsset_LeaseFrom = System.DateTime.Now;
                mModel.FixedAsset_LeaseRate = 0;
                mModel.FixedAsset_LeaseTo = System.DateTime.Now;
                mModel.FixedAsset_LicIssueDt = System.DateTime.Now;
                mModel.FixedAsset_LifeMonths = 0;
                mModel.FixedAsset_MinCharges = 0;
                mModel.FixedAsset_Name = "";
                mModel.FixedAsset_Narr = "";
                mModel.FixedAsset_NextDeprAmt = 0;
                mModel.FixedAsset_NextPremiumDue = System.DateTime.Now;
                mModel.FixedAsset_OnSite = false;
                mModel.FixedAsset_ParentAsset = "";
                mModel.FixedAsset_ParentStatus = "";
                mModel.FixedAsset_Pickup = 0;
                mModel.FixedAsset_policyDate = System.DateTime.Now;
                mModel.FixedAsset_PolicyNo = "";
                mModel.FixedAsset_PremiumAmt = 0;
                mModel.FixedAsset_Product = "";
                mModel.FixedAsset_PurchDate = System.DateTime.Now;
                mModel.FixedAsset_RegNo = "";
                mModel.FixedAsset_TagNo = "";
                mModel.FixedAsset_UseDate = System.DateTime.Now;
                mModel.FixedAsset_ValueInsured = 0;
                mModel.FixedAsset_WarExpDate = System.DateTime.Now;
                mModel.FixedAsset_Warranty = 0;
                mModel.FixedAsset_WarrantyNote = "";
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(AssetsMastersVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteAssetsMasters(mModel);
                        if (mModel.Mode == "Delete")
                        {
                            transaction.Commit();
                            transaction.Dispose();
                            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.FixedAsset_Code, DateTime.Now, 0, "", "Delete Assets Masters", "NA");

                            return Json(new
                            {
                                Status = "success",
                                Message = "Data is Deleted."
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    FixedAsset mobj = new FixedAsset();
                    if (mModel.Mode == "Edit")
                    {
                        mobj = ctxTFAT.FixedAsset.Where(x => (x.Code == mModel.FixedAsset_Code)).FirstOrDefault();
                    }
                    mobj.Depreciable = mModel.FixedAsset_Depreciable;
                    mobj.Code = mModel.FixedAsset_Code;
                    mobj.Warranty = mModel.FixedAsset_Warranty;
                    mobj.AgencyName = mModel.FixedAsset_AgencyName;
                    mobj.WarExpDate = ConvertDDMMYYTOYYMMDD(mModel.FixedAsset_WarExpDateVM);
                    mobj.Name = mModel.FixedAsset_Name;
                    mobj.RegNo = mModel.FixedAsset_RegNo;
                    mobj.TagNo = mModel.FixedAsset_TagNo;
                    mobj.DeprAccount = mModel.FixedAsset_DeprAccount;
                    mobj.OnSite = mModel.FixedAsset_OnSite;
                    mobj.AccumDepr = mModel.FixedAsset_AccumDepr;
                    mobj.Product = mModel.FixedAsset_Product;
                    mobj.MinCharges = mModel.FixedAsset_MinCharges;
                    mobj.FreeParts = mModel.FixedAsset_FreeParts;
                    mobj.LicIssueDt = ConvertDDMMYYTOYYMMDD(mModel.FixedAsset_LicIssueDtVM);
                    mobj.AccountCode = mModel.FixedAsset_AccountCode;
                    mobj.DeprMethod = mModel.FixedAsset_DeprMethod;
                    mobj.AssetCategory = mModel.FixedAsset_AssetCategory;
                    mobj.FreeLabour = mModel.FixedAsset_FreeLabour;
                    mobj.CostCentre = mModel.FixedAsset_CostCentre;
                    mobj.ExpDate = ConvertDDMMYYTOYYMMDD(mModel.FixedAsset_ExpDateVM);
                    mobj.BookValue = mModel.FixedAsset_BookValue;
                    mobj.Cost = mModel.FixedAsset_Cost;
                    mobj.Pickup = mModel.FixedAsset_Pickup;
                    mobj.PurchDate = ConvertDDMMYYTOYYMMDD(mModel.FixedAsset_PurchDateVM);
                    mobj.DeprRate = mModel.FixedAsset_DeprRate;
                    mobj.DeprRateIT = mModel.FixedAsset_DeprRateIT;
                    mobj.UseDate = ConvertDDMMYYTOYYMMDD(mModel.FixedAsset_UseDateVM);
                    mobj.DeprEndDt = ConvertDDMMYYTOYYMMDD(mModel.FixedAsset_DeprEndDtVM);
                    mobj.AcqType = mModel.FixedAsset_AcqType;
                    mobj.InsuranceComp = mModel.FixedAsset_InsuranceComp;
                    mobj.DeprSinceInUse = mModel.FixedAsset_DeprSinceInUse;
                    mobj.AssetStatus = mModel.FixedAsset_AssetStatus;
                    mobj.WarrantyNote = mModel.FixedAsset_WarrantyNote;
                    mobj.PolicyNo = mModel.FixedAsset_PolicyNo;
                    mobj.NextDeprAmt = mModel.FixedAsset_NextDeprAmt;
                    mobj.LifeMonths = mModel.FixedAsset_LifeMonths;
                    mobj.policyDate = ConvertDDMMYYTOYYMMDD(mModel.FixedAsset_policyDateVM);
                    mobj.ValueInsured = mModel.FixedAsset_ValueInsured;
                    mobj.Narr = mModel.FixedAsset_Narr;
                    mobj.ParentStatus = mModel.FixedAsset_ParentStatus;
                    mobj.ParentAsset = mModel.FixedAsset_ParentAsset;
                    mobj.LeaseFrom = ConvertDDMMYYTOYYMMDD(mModel.FixedAsset_LeaseFromVM);
                    mobj.PremiumAmt = mModel.FixedAsset_PremiumAmt;
                    mobj.NextPremiumDue = ConvertDDMMYYTOYYMMDD(mModel.FixedAsset_NextPremiumDueVM);
                    mobj.LeaseTo = ConvertDDMMYYTOYYMMDD(mModel.FixedAsset_LeaseToVM);
                    mobj.LeaseRate = mModel.FixedAsset_LeaseRate;
                    mobj.Comprehensive = mModel.FixedAsset_Comprehensive;
                    mobj.AMCRate = mModel.FixedAsset_AMCRate;
                    mobj.AMCFrom = ConvertDDMMYYTOYYMMDD(mModel.FixedAsset_AMCFromVM);
                    mobj.AMCTo = ConvertDDMMYYTOYYMMDD(mModel.FixedAsset_AMCToVM);
                    mobj.CompreDet = mModel.FixedAsset_CompreDet;
                    // iX9: default values for the fields not used @Form
                    mobj.AccountGrp = "";
                    mobj.AccumDepr10 = 0;
                    mobj.AccumDepr11 = 0;
                    mobj.AccumDepr2 = 0;
                    mobj.AccumDepr3 = 0;
                    mobj.AccumDepr4 = 0;
                    mobj.AccumDepr5 = 0;
                    mobj.AccumDepr6 = 0;
                    mobj.AccumDepr7 = 0;
                    mobj.AccumDepr8 = 0;
                    mobj.AccumDepr9 = 0;
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mAUTHORISE;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mModel.Mode == "Add")
                    {
                        ctxTFAT.FixedAsset.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.FixedAsset_Code, DateTime.Now, 0, "", "Save Assets Masters", "NA");

                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "AssetsMasters" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "AssetsMasters" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                }
            }
            return Json(new { Status = "Success", id = "AssetsMasters" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteAssetsMasters(AssetsMastersVM mModel)
        {
            if (mModel.FixedAsset_Code == null || mModel.FixedAsset_Code == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var mList = ctxTFAT.FixedAsset.Where(x => (x.Code == mModel.FixedAsset_Code)).FirstOrDefault();
            ctxTFAT.FixedAsset.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}