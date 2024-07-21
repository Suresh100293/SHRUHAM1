using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
using EntitiModel;
using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class AssetUsageController : BaseController
    {
        //private //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        //private IBusinessCommon mBuss = new BusinessCommon();
        private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mAUTHORISE = "A00";
        private static int mdocument = 0;

        #region GetLists
        public JsonResult AutoCompleteAssetID(string term)
        {
            return Json((from m in ctxTFAT.Assets
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteShift(string term)
        {
            return Json((from m in ctxTFAT.Shift
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        #endregion GetLists

        // GET: Accounts/AssetUsage
        public ActionResult Index(AssetUsageVM mModel)
        {
            //GetAllMenu(Session["ModuleName"].ToString());
            ////AccountingGetAllMenu(Session["ModuleName"].ToString());
            ////VehicleGetAllMenu(Session["ModuleName"].ToString());
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mModel.AssetUsage_FromDate = DateTime.Now;
            mModel.AssetUsage_ToDate = DateTime.Now;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                mdocument = mModel.Document;
                var mList = ctxTFAT.AssetUsage.Where(x => (x.RECORDKEY == mModel.Document)).FirstOrDefault();
                var mAssetID = ctxTFAT.Assets.Where(x => x.Code == mList.AssetID).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mShift = ctxTFAT.Shift.Where(x => x.Code == mList.Shift).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                mModel.AssetUsage_AssetID = mAssetID != null ? mAssetID.Code.ToString() : "";
                mModel.AssetIDName = mAssetID != null ? mAssetID.Name : "";
                mModel.AssetUsage_Shift = mShift != null ? mShift.Code : 0;
                mModel.ShiftName = mShift != null ? mShift.Name : "";
                mModel.AssetUsage_FromDate = mList.FromDate != null ? mList.FromDate : DateTime.Now;
                mModel.AssetUsage_ToDate = mList.ToDate != null ? mList.ToDate : DateTime.Now;
            }
            else
            {
                mModel.AssetUsage_AssetID = "";
                mModel.AssetUsage_Branch = "";
                mModel.AssetUsage_CompCode = "";
                mModel.AssetUsage_FromDate = System.DateTime.Now;
                mModel.AssetUsage_LocationCode = 0;
                mModel.AssetUsage_Shift = 0;
                mModel.AssetUsage_ToDate = System.DateTime.Now;
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(AssetUsageVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteAssetUsage(mModel);
                        if (mModel.Mode == "Delete")
                        {
                            transaction.Commit();
                            transaction.Dispose();
                            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.AssetUsage_RECORDKEY, DateTime.Now, 0, "", " Delete AssetUsage", "NA");
                            return Json(new
                            {
                                Status = "success",
                                Message = "Data is Deleted."
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    AssetUsage mobj = new AssetUsage();
                    if (mModel.Mode == "Edit")
                    {
                        mobj = ctxTFAT.AssetUsage.Where(x => (x.RECORDKEY == mModel.AssetUsage_RECORDKEY)).FirstOrDefault();
                    }
                    mobj.AssetID = mModel.AssetUsage_AssetID;
                    mobj.Shift = mModel.AssetUsage_Shift;
                    mobj.FromDate = ConvertDDMMYYTOYYMMDD(mModel.FromDateVM);
                    mobj.ToDate = ConvertDDMMYYTOYYMMDD(mModel.ToDateVM);
                    // iX9: default values for the fields not used @Form
                    mobj.Branch = "";
                    mobj.CompCode = "";
                    mobj.LocationCode = 0;
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mAUTHORISE;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mModel.Mode == "Add")
                    {
                        mobj.RECORDKEY = GetNextCode();
                    }
                    if (mModel.Mode == "Add")
                    {
                        ctxTFAT.AssetUsage.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.RECORDKEY, DateTime.Now, 0,"", " Save AssetUsage", "NA");

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
            return Json(new { Status = "Success", id = "AssetUsage" }, JsonRequestBehavior.AllowGet);
        }

        public int GetNextCode()
        {
            var nextcode = (from x in ctxTFAT.AssetUsage select (int?)x.RECORDKEY).Max() ?? 0;
            return (++nextcode);
        }

        public ActionResult DeleteAssetUsage(AssetUsageVM mModel)
        {
            var mList = ctxTFAT.AssetUsage.Where(x => (x.RECORDKEY == mModel.AssetUsage_RECORDKEY)).FirstOrDefault();
            ctxTFAT.AssetUsage.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}