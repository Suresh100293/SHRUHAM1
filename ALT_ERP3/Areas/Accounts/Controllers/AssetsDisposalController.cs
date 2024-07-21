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
    public class AssetsDisposalController : BaseController
    {
        //private //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        //private IBusinessCommon mBuss = new BusinessCommon();
        //private int mlocation = 100001;
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

        public JsonResult AutoCompleteBranch(string term)
        {
            return Json((from m in ctxTFAT.TfatBranch
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteStore(string term)
        {
            return Json((from m in ctxTFAT.Stores
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        #endregion GetLists

        // GET: Accounts/AssetsDisposal
        public ActionResult Index(AssetsDisposalVM mModel)
        {
            //GetAllMenu(Session["ModuleName"].ToString());
            ////AccountingGetAllMenu(Session["ModuleName"].ToString());
            ////VehicleGetAllMenu(Session["ModuleName"].ToString());
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mModel.AssetDisposal_DocDate = DateTime.Now;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                mdocument = mModel.Document;
                var mList = ctxTFAT.AssetDisposal.Where(x => (x.Srl == mModel.Document)).FirstOrDefault();
                var mAssetID = ctxTFAT.Assets.Where(x => x.Code == mList.AssetID).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mList.Branch).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mStore = ctxTFAT.Stores.Where(x => x.Code == mList.Store).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                mModel.AssetDisposal_AssetID = mAssetID != null ? mAssetID.Code.ToString() : "";
                mModel.AssetIDName = mAssetID != null ? mAssetID.Name : "";
                mModel.AssetDisposal_Branch = mBranch != null ? mBranch.Code.ToString() : "";
                mModel.BranchName = mBranch != null ? mBranch.Name : "";
                mModel.AssetDisposal_Store = mStore != null ? mStore.Code : 0;
                mModel.StoreName = mStore != null ? mStore.Name : "";
                mModel.AssetDisposal_Srl = mList.Srl;
                mModel.AssetDisposal_DocDate = mList.DocDate != null ? mList.DocDate : DateTime.Now;
                mModel.AssetDisposal_AssetValue = mList.AssetValue;
                mModel.AssetDisposal_Qty = mList.Qty;
                mModel.AssetDisposal_Qty2 = mList.Qty2;
                mModel.AssetDisposal_Reason = mList.Reason;
            }
            else
            {
                mModel.AssetDisposal_AssetID = "";
                mModel.AssetDisposal_AssetValue = 0;
                mModel.AssetDisposal_Branch = "";
                mModel.AssetDisposal_CompCode = "";
                mModel.AssetDisposal_DocDate = System.DateTime.Now;
                mModel.AssetDisposal_LocationCode = 0;
                mModel.AssetDisposal_Qty = 0;
                mModel.AssetDisposal_Qty2 = 0;
                mModel.AssetDisposal_Reason = "";
                mModel.AssetDisposal_Srl = 0;
                mModel.AssetDisposal_Store = 0;
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(AssetsDisposalVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteAssetsDisposal(mModel);
                        if (mModel.Mode == "Delete")
                        {
                            transaction.Commit();
                            transaction.Dispose();
                            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.Document, DateTime.Now, 0, "", "Delete Assets Disposal", "NA");
                            return Json(new
                            {
                                Status = "success",
                                Message = "Data is Deleted."
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    AssetDisposal mobj = new AssetDisposal();
                    if (mModel.Mode == "Edit")
                    {
                        mobj = ctxTFAT.AssetDisposal.Where(x => (x.Srl == mModel.AssetDisposal_Srl)).FirstOrDefault();
                    }
                    mobj.Srl = mModel.AssetDisposal_Srl;
                    mobj.DocDate = ConvertDDMMYYTOYYMMDD(mModel.DocDateVM);
                    mobj.AssetID = mModel.AssetDisposal_AssetID;
                    mobj.AssetValue = mModel.AssetDisposal_AssetValue;
                    mobj.Branch = mModel.AssetDisposal_Branch;
                    mobj.Store = mModel.AssetDisposal_Store;
                    mobj.Qty = mModel.AssetDisposal_Qty;
                    mobj.Qty2 = mModel.AssetDisposal_Qty2;
                    mobj.Reason = mModel.AssetDisposal_Reason;
                    // iX9: default values for the fields not used @Form
                    mobj.CompCode = "";
                    mobj.LocationCode = 0;
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mAUTHORISE;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mModel.Mode == "Add")
                    {
                        mobj.Srl = GetNextCode();
                    }
                    if (mModel.Mode == "Add")
                    {
                        ctxTFAT.AssetDisposal.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.Srl.Value.ToString(), DateTime.Now, 0, "", "Save Assets Disposal", "NA");

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
            return Json(new { Status = "Success", id = "AssetsDisposal" }, JsonRequestBehavior.AllowGet);
        }

        public int GetNextCode()
        {
            var nextcode = (from x in ctxTFAT.AssetDisposal select (int?)x.Srl).Max() ?? 0;
            return (++nextcode);
        }

        public ActionResult DeleteAssetsDisposal(AssetsDisposalVM mModel)
        {
            var mList = ctxTFAT.AssetDisposal.Where(x => (x.Srl == mModel.AssetDisposal_Srl)).FirstOrDefault();
            ctxTFAT.AssetDisposal.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}