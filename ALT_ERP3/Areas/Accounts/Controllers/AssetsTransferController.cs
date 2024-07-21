using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
using ALT_ERP3.Models;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class AssetsTransferController : BaseController
    {
        //private //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        //private IBusinessCommon mBuss = new BusinessCommon();
        private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mAUTHORISE = "A00";
        private static string mdocument = "";

        #region GetLists
        public JsonResult AutoCompletefBranch(string term)
        {
            return Json((from m in ctxTFAT.TfatBranch
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompletefLocation(string term)
        {
            return Json((from m in ctxTFAT.Stores
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompletetBranch(string term)
        {
            return Json((from m in ctxTFAT.TfatBranch
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompletetLocation(string term)
        {
            return Json((from m in ctxTFAT.Stores
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteAssetID(string term)
        {
            return Json((from m in ctxTFAT.Assets
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        #endregion GetLists

        // GET: Accounts/AssetsTransfer
        public ActionResult Index(AssetsTransferVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mModel.AssetTracking_STDate = DateTime.Now;
            mModel.AssetTracking_stTime = DateTime.Now;
            mModel.AssetTracking_EndDate = DateTime.Now;
            mModel.AssetTracking_EndTime = DateTime.Now;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                mdocument = mModel.Document;
                var mList = ctxTFAT.AssetTracking.Where(x => (x.Srl == mModel.Document)).FirstOrDefault();
                var mfBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mList.fBranch).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mfLocation = ctxTFAT.Stores.Where(x => x.Code == mList.fLocation).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mtBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mList.tBranch).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mtLocation = ctxTFAT.Stores.Where(x => x.Code == mList.tLocation).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mAssetID = ctxTFAT.Assets.Where(x => x.Code == mList.AssetID).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                mModel.AssetTracking_fBranch = mfBranch != null ? mfBranch.Code.ToString() : "";
                mModel.fBranchName = mfBranch != null ? mfBranch.Name : "";
                mModel.AssetTracking_fLocation = mfLocation != null ? mfLocation.Code : 0;
                mModel.fLocationName = mfLocation != null ? mfLocation.Name : "";
                mModel.AssetTracking_tBranch = mtBranch != null ? mtBranch.Code.ToString() : "";
                mModel.tBranchName = mtBranch != null ? mtBranch.Name : "";
                mModel.AssetTracking_tLocation = mtLocation != null ? mtLocation.Code : 0;
                mModel.tLocationName = mtLocation != null ? mtLocation.Name : "";
                mModel.AssetTracking_AssetID = mAssetID != null ? mAssetID.Code.ToString() : "";
                mModel.AssetIDName = mAssetID != null ? mAssetID.Name : "";
                mModel.AssetTracking_Srl = mList.Srl;
                mModel.AssetTracking_STDate = mList.STDate != null ? mList.STDate : DateTime.Now;
                mModel.AssetTracking_stTime = mList.STTime != null ? mList.STTime : DateTime.Now;
                mModel.AssetTracking_EndDate = mList.EndDate != null ? mList.EndDate : DateTime.Now;
                mModel.AssetTracking_EndTime = mList.EndTime != null ? mList.EndTime : DateTime.Now;
                mModel.AssetTracking_Qty = mList.Qty;
                mModel.AssetTracking_Qty2 = mList.Qty2;
                mModel.AssetTracking_operators = mList.operators;
                mModel.AssetTracking_Narr = mList.Narr;
            }
            else
            {
                mModel.AssetTracking_AssetID = "";
                mModel.AssetTracking_EndDate = System.DateTime.Now;
                mModel.AssetTracking_EndTime = System.DateTime.Now;
                mModel.AssetTracking_fBranch = "";
                mModel.AssetTracking_fLocation = 0;
                mModel.AssetTracking_Narr = "";
                mModel.AssetTracking_operators = "";
                mModel.AssetTracking_Prefix = "";
                mModel.AssetTracking_Qty = 0;
                mModel.AssetTracking_Qty2 = 0;
                mModel.AssetTracking_Srl = "";
                mModel.AssetTracking_STDate = System.DateTime.Now;
                mModel.AssetTracking_stTime = System.DateTime.Now;
                mModel.AssetTracking_TagNo = "";
                mModel.AssetTracking_tBranch = "";
                mModel.AssetTracking_tLocation = 0;
                mModel.AssetTracking_Type = "";
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(AssetsTransferVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteAssetsTransfer(mModel);
                        if (mModel.Mode == "Delete")
                        {
                            transaction.Commit();
                            transaction.Dispose();
                            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.AssetTracking_Srl, DateTime.Now, 0, "", "Delete Assets Transfer", "NA");
                            return Json(new
                            {
                                Status = "success",
                                Message = "Data is Deleted."
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    AssetTracking mobj = new AssetTracking();
                    if (mModel.Mode == "Edit")
                    {
                        mobj = ctxTFAT.AssetTracking.Where(x => (x.Srl == mModel.AssetTracking_Srl)).FirstOrDefault();
                    }
                    mobj.Srl = mModel.AssetTracking_Srl;
                    mobj.STDate = ConvertDDMMYYTOYYMMDD(mModel.STDateVM);
                    mobj.STTime = ConvertDDMMYYTOYYMMDD(mModel.stTimeVM);
                    mobj.EndDate = ConvertDDMMYYTOYYMMDD(mModel.EndDateVM);
                    mobj.EndTime = ConvertDDMMYYTOYYMMDD(mModel.EndTimeVM);
                    mobj.fBranch = mModel.AssetTracking_fBranch;
                    mobj.fLocation = mModel.AssetTracking_fLocation;
                    mobj.tBranch = mModel.AssetTracking_tBranch;
                    mobj.tLocation = mModel.AssetTracking_tLocation;
                    mobj.AssetID = mModel.AssetTracking_AssetID;
                    mobj.Qty = mModel.AssetTracking_Qty;
                    mobj.Qty2 = mModel.AssetTracking_Qty2;
                    mobj.operators = mModel.AssetTracking_operators;
                    mobj.Narr = mModel.AssetTracking_Narr;
                    // iX9: default values for the fields not used @Form
                    mobj.Prefix = "";
                    mobj.TagNo = "";
                    mobj.Type = "";
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
                        ctxTFAT.AssetTracking.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.Srl, DateTime.Now, 0,"", "Save Assets Transfer", "NA");

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
            return Json(new { Status = "Success", id = "AssetsTransfer" }, JsonRequestBehavior.AllowGet);
        }

        public string GetNextCode()
        {
            string Code = (from x in ctxTFAT.AssetTracking select x.Srl).Max();
            string digits = new string(Code.Where(char.IsDigit).ToArray());
            int number;
            if (!int.TryParse(digits, out number)) //int.Parse would do the job since only digits are selected
            {
                number = 0;
            }
            return (++number).ToString("D15");
        }

        public ActionResult DeleteAssetsTransfer(AssetsTransferVM mModel)
        {
            if (mModel.AssetTracking_Srl == null || mModel.AssetTracking_Srl == "")
            {
                return Json(new
                {
                    Message = "Srl not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var mList = ctxTFAT.AssetTracking.Where(x => (x.Srl == mModel.AssetTracking_Srl)).FirstOrDefault();
            ctxTFAT.AssetTracking.Remove(mList);
            ctxTFAT.SaveChanges();
            // delete from Addons
            var mTableKey = "AssetTracking_" + mModel.AssetTracking_Srl;
            var DeleteAddon = ctxTFAT.AddonValues.Where(x => x.TableKey == mTableKey).ToList();
            foreach (var item in DeleteAddon)
            {
                ctxTFAT.AddonValues.Remove(item);
                ctxTFAT.SaveChanges();
            }

            // delete from narration
            var mTableKey2 = "AssetTracking_" + mModel.AssetTracking_Srl;
            var DeleteAddNote = ctxTFAT.Narration.Where(x => x.TableKey == mTableKey2).ToList();
            foreach (var item in DeleteAddNote)
            {
                ctxTFAT.Narration.Remove(item);
                ctxTFAT.SaveChanges();
            }
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult AddOnGrid()
        //{
        //    ViewBag.ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
        //    return Json(new { Status = "Success", Controller = ViewBag.ControllerName }, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult GetAddOnList(string sidx, string sord, int page, int rows, string Code)
        {
            int pageindex = Convert.ToInt32(page) - 1;
            int pagesize = rows;
            var cntName = this.ControllerContext.RouteData.Values["controller"].ToString();
            var addonListResult = ctxTFAT.AddOns.Where(s => s.TableKey == cntName).Select(a => new { a.Fld, a.FldType, a.Head, a.RECORDKEY });
            int totalRecords = addonListResult.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (float)rows);
            if (sord.ToUpper() == "DESC")
            {
                addonListResult = addonListResult.OrderByDescending(s => s.Fld);
                addonListResult = addonListResult.Skip(pageindex * pagesize).Take(pagesize);
            }
            else
            {
                addonListResult = addonListResult.OrderBy(s => s.Fld);
                addonListResult = addonListResult.Skip(pageindex * pagesize).Take(pagesize);
            }
            var jsonData = new
            {
                total = totalPages,
                page,
                records = totalRecords,
                rows = addonListResult,
            };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddAddOnsValues(string mString)
        {
            string mStr = mString.Replace("\"", " ").Replace("[", "").Replace("]", "");
            string[] strArr = mStr.Split(',');
            int i = 0;
            var query = from s in strArr
                        let num = i++
                        group s by num / 5 into g
                        select g.ToArray();

            var results = query.ToArray();
            IList<AddOnValueVM> mobj = new List<AddOnValueVM>();
            if (Session["AddonList"] != null)
            {
                mobj = (List<AddOnValueVM>)Session["AddonList"];
            }

            foreach (var mResult in results)
            {
                //var mrecordkey = mResult[0];
                var mfld = mResult[1];
                var mInput = mResult[2];
                mobj.Add(new AddOnValueVM()
                {
                    TableKey = mResult[3].Trim(),
                    Branch = mbranchcode,
                    Fld = mfld.Trim(),
                    Value = mInput.Trim(),
                    MainType = "",
                    SubType = "",
                    PeriodCode = mperiod,
                    AccPeriod = mperiod,
                    CompCode = mcompcode
                });
            }
            Session.Add("AddonList", mobj);
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditAddOnGridNew(string mCode)
        {
            ViewBag.ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
            return Json(new { Status = "Success", Controller = ViewBag.ControllerName, Code = mCode }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EditGetAddOnList(string sidx, string sord, int page, int rows, string mCode) //Get AddOnList
        {
            int pageindex = Convert.ToInt32(page) - 1;
            int pagesize = rows;
            var cntName = this.ControllerContext.RouteData.Values["controller"].ToString();
            var mAddonvalueresult = ctxTFAT.AddonValues.Where(x => x.TableKey == mCode && x.Branch == mbranchcode).ToList();
            var mAddonListResult = ctxTFAT.AddOns.Where(s => s.TableKey == cntName).Select(a => new { a.Fld, a.FldType, a.Head, a.RECORDKEY }).ToList();

            List<AddOnViewModel> mNewAddonResult = new List<AddOnViewModel>();
            foreach (var mListitem in mAddonListResult)
            {
                var mAddonValue = mAddonvalueresult.Where(m => m.Fld == mListitem.Fld).FirstOrDefault();
                if (mAddonValue != null)
                {
                    mNewAddonResult.Add(new AddOnViewModel
                    {
                        Fld = mListitem.Fld,
                        FldType = mListitem.FldType,
                        Head = mListitem.Head,
                        //RECORDKEY = mListitem.RecordKey,
                        Value = mAddonValue.Value,
                        Addoncode = mCode
                    });
                }
                else
                {
                    mNewAddonResult.Add(new AddOnViewModel
                    {
                        Fld = mListitem.Fld,
                        FldType = mListitem.FldType,
                        Head = mListitem.Head,
                        //RecordKey = mListitem.RecordKey,
                        Addoncode = mCode
                    });
                }
            }
            int totalRecords = mNewAddonResult.Count();
            var totalPages = (int)Math.Ceiling((float)totalRecords / (float)rows);
            if (sord.ToUpper() == "DESC")
            {
                mNewAddonResult = mNewAddonResult.OrderByDescending(s => s.Fld).ToList();
                mNewAddonResult = mNewAddonResult.Skip(pageindex * pagesize).Take(pagesize).ToList();
            }
            else
            {
                mNewAddonResult = mNewAddonResult.OrderBy(s => s.Fld).ToList();
                mNewAddonResult = mNewAddonResult.Skip(pageindex * pagesize).Take(pagesize).ToList();
            }
            var jsonData = new
            {
                total = totalPages,
                page,
                records = totalRecords,
                rows = mNewAddonResult,
            };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditAddOnsValues(string mInputValues)
        {
            string mStr = mInputValues.Replace("\"", " ").Replace("[", "").Replace("]", "");
            string[] strArr = mStr.Split(',');
            int i = 0;
            var query = from s in strArr
                        let num = i++
                        group s by num / 5 into g
                        select g.ToArray();
            var results = query.ToArray();
            foreach (var mResult in results)
            {
                AddonValues mAddonValues = new AddonValues();
                //var recordkey = mResult[0];
                var fld = mResult[1];
                var input = mResult[2];
                mAddonValues.Branch = mbranchcode;
                mAddonValues.Fld = fld.Trim();
                mAddonValues.Value = input.Trim();
                mAddonValues.TableKey = mResult[3].Trim();
                var mExists = ctxTFAT.AddonValues.Where(x => x.Fld == mAddonValues.Fld && x.Branch == mAddonValues.Branch && x.TableKey == mAddonValues.TableKey).Select(m => new { m.TableKey }).FirstOrDefault();
                if (mExists != null)
                {
                    AddonValues addonvalue = ctxTFAT.AddonValues.Where(x => x.TableKey == mExists.TableKey && x.Branch == mbranchcode).FirstOrDefault();
                    addonvalue.Branch = mAddonValues.Branch;
                    addonvalue.Fld = mAddonValues.Fld;
                    addonvalue.Value = mAddonValues.Value;
                    addonvalue.TableKey = mAddonValues.TableKey;
                    ctxTFAT.Entry(addonvalue).State = EntityState.Modified;
                }
                else
                {
                    AddonValues mTest = new AddonValues
                    {
                        Branch = mbranchcode,
                        Fld = fld.Trim(),
                        Value = input.Trim(),
                        TableKey = mResult[3].Trim(),
                        LocationCode = mlocation,
                        ENTEREDBY = muserid,
                        LASTUPDATEDATE = DateTime.Now,
                        AUTHORISE = mAUTHORISE,
                        AUTHIDS = muserid,
                    };
                    ctxTFAT.AddonValues.Add(mTest);
                }
            }
            try
            {
                ctxTFAT.SaveChanges();
            }
            catch (Exception dbEx)
            {
                Exception raise = dbEx;
                throw raise;
            }
            return View();
        }
        public ActionResult AddNote()
        {
            ViewBag.ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
            return Json(new { Status = "Success", Controller = ViewBag.ControllerName }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveAddNote(Narration mModel)
        {
            try
            {
                mModel.Prefix = GetPrefix();
                IList<AddNoteVM> Note = new List<AddNoteVM>();

                if (Session["AddNote"] != null)
                {
                    Note = (List<AddNoteVM>)Session["AddNote"];
                }

                Note.Add(new AddNoteVM()
                {
                    Branch = mbranchcode,
                    Prefix = mModel.Prefix,
                    Sno = 0,
                    NarrRich = mModel.Narr,
                    ENTEREDBY = muserid,
                    LASTUPDATEDATE = DateTime.Now,
                    AUTHIDS = muserid,
                    AUTHORISE = mAUTHORISE,
                    LocationCode = mlocation
                });
                Session.Add("AddNote", Note);
            }
            catch (DbEntityValidationException ex1)
            {
                string dd1 = ex1.InnerException.Message;
            }
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditAddNote(string mCode, Narration mModel)
        {
            ViewBag.ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
            var mController = this.ControllerContext.RouteData.Values["controller"].ToString();
            mModel.Branch = mbranchcode;
            mModel.Sno = 0;
            var mobj = ctxTFAT.Narration.Where(x => (x.TableKey) == (mController + mCode)).FirstOrDefault();
            if (mobj != null)
            {
                mModel.NarrRich = mobj.NarrRich.ToString();
            }
            return Json(new { Status = "Success", Controller = ViewBag.ControllerName, Code = mCode, narval = mModel.NarrRich, valid = mCode, changelog = "edit", changelogs = "view" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditAddNotes(Narration mModel, string mCode, string mMode)
        {
            try
            {
                ViewBag.ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                var mController = this.ControllerContext.RouteData.Values["controller"].ToString();
                mModel.Prefix = GetPrefix();

                var mobj = ctxTFAT.Narration.Where(x => (x.TableKey) == (mController + mCode)).FirstOrDefault();
                if (mobj != null)
                {
                    mobj.NarrRich = mModel.Narr;
                    ctxTFAT.Entry(mobj).State = EntityState.Modified;
                }
                else
                {
                    Narration mNarr = new Narration
                    {
                        Branch = mbranchcode,
                        Type = mModel.Type,
                        Prefix = mModel.Prefix,
                        Srl = mCode,
                        Sno = 0,
                        TableKey = mController + mCode,
                        Narr = mModel.Narr,
                        NarrRich = mModel.Narr,
                        ENTEREDBY = muserid,
                        LASTUPDATEDATE = DateTime.Now,
                        AUTHIDS = muserid,
                        AUTHORISE = mAUTHORISE,
                        LocationCode = mlocation,
                    };
                    ctxTFAT.Narration.Add(mNarr);
                }
            }
            catch (DbEntityValidationException ex1)
            {
                string dd1 = ex1.InnerException.Message;
            }
            try
            {
                ctxTFAT.SaveChanges();
            }
            catch (Exception dbEx)
            {
                Exception raise = dbEx;
                throw raise;
            }
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}