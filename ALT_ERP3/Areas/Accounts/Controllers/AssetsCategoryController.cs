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
    public class AssetsCategoryController : BaseController
    {
        //private //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        //private IBusinessCommon mBuss = new BusinessCommon();
        //private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mAUTHORISE = "A00";
        private static int mdocument = 0;

        #region GetLists
        public List<SelectListItem> GetDeprMethodList()
        {
            List<SelectListItem> CallDeprMethodList = new List<SelectListItem>();
            CallDeprMethodList.Add(new SelectListItem { Value = "S", Text = "Straight Line" });
            CallDeprMethodList.Add(new SelectListItem { Value = "W", Text = "WDV" });
            return CallDeprMethodList;
        }
        #endregion GetLists

        // GET: Accounts/AssetsCategory
        public ActionResult Index(AssetsCategoryVM mModel)
        {
            //GetAllMenu(Session["ModuleName"].ToString());
            ////AccountingGetAllMenu(Session["ModuleName"].ToString());
            ////VehicleGetAllMenu(Session["ModuleName"].ToString());
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mModel.DeprMethodList = GetDeprMethodList();
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                mdocument = mModel.Document;
                var mList = ctxTFAT.AssetCategory.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                mModel.AssetCategory_Code = mList.Code;
                mModel.AssetCategory_Name = mList.Name;
                mModel.AssetCategory_Depreciable = mList.Depreciable;
                mModel.AssetCategory_DeprMethod = mList.DeprMethod;
                mModel.AssetCategory_DeprRate = mList.DeprRate;
                mModel.AssetCategory_DeprRateIT = mList.DeprRateIT;
            }
            else
            {
                mModel.AssetCategory_Account = "";
                mModel.AssetCategory_Code = 0;
                mModel.AssetCategory_DeprAccount = "";
                mModel.AssetCategory_Depreciable = false;
                mModel.AssetCategory_DeprMethod = "";
                mModel.AssetCategory_DeprRate = 0;
                mModel.AssetCategory_DeprRateIT = 0;
                mModel.AssetCategory_Name = "";
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(AssetsCategoryVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteAssetsCategory(mModel);
                        if (mModel.Mode == "Delete")
                        {
                            transaction.Commit();
                            transaction.Dispose();
                            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.AssetCategory_Code.ToString(), DateTime.Now, 0, "", "Delete Assets Category", "NA");
                            return Json(new
                            {
                                Status = "success",
                                Message = "Data is Deleted."
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    AssetCategory mobj = new AssetCategory();
                    if (mModel.Mode == "Edit")
                    {
                        mobj = ctxTFAT.AssetCategory.Where(x => (x.Code == mModel.AssetCategory_Code)).FirstOrDefault();
                    }
                    mobj.Code = mModel.AssetCategory_Code;
                    mobj.Name = mModel.AssetCategory_Name;
                    mobj.Depreciable = mModel.AssetCategory_Depreciable;
                    mobj.DeprMethod = mModel.AssetCategory_DeprMethod;
                    mobj.DeprRate = mModel.AssetCategory_DeprRate;
                    mobj.DeprRateIT = mModel.AssetCategory_DeprRateIT;
                    // iX9: default values for the fields not used @Form
                    mobj.Account = "";
                    mobj.DeprAccount = "";
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mAUTHORISE;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mModel.Mode == "Add")
                    {
                        mobj.Code = GetNextCode();
                    }
                    if (mModel.Mode == "Add")
                    {
                        ctxTFAT.AssetCategory.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.Code, DateTime.Now, 0, "", "Save Assets Category", "NA");

                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    string dd1 = ex1.InnerException.Message;
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    string dd = ex.InnerException.Message;
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                }
            }
            return Json(new { Status = "Success", id = "AssetsCategory" }, JsonRequestBehavior.AllowGet);
        }

        public int GetNextCode()
        {
            var nextcode = (from x in ctxTFAT.AssetCategory select (int?)x.Code).Max() ?? 0;
            return (++nextcode);
        }

        public ActionResult DeleteAssetsCategory(AssetsCategoryVM mModel)
        {
            // iX9: Check for Active Master AssetCategory
            string mactivestring = "";
            var mactive1 = ctxTFAT.FixedAsset.Where(x => (x.AssetCategory == mModel.AssetCategory_Code)).Select(x => x.Name).FirstOrDefault();
            if (mactive1 != null) { mactivestring = mactivestring + "\nFixedAsset: " + mactive1; }
            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "Active Item, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var mList = ctxTFAT.AssetCategory.Where(x => (x.Code == mModel.AssetCategory_Code)).FirstOrDefault();
            ctxTFAT.AssetCategory.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}