using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class PartyCreditStatusManagementController : BaseController
    {
        private int mlocation = 100001;
        private static string mauthorise = "A00";
        private static string mheader = "";
        private static int mdocument = 0;

        #region GetLists
        public List<SelectListItem> GetCRLimitWarnList()
        {
            List<SelectListItem> CallCRLimitWarnList = new List<SelectListItem>();
            CallCRLimitWarnList.Add(new SelectListItem { Value = "W", Text = "Warning" });
            CallCRLimitWarnList.Add(new SelectListItem { Value = "R", Text = "Restrict" });
            return CallCRLimitWarnList;
        }

        public List<SelectListItem> GetCRDaysWarnList()
        {
            List<SelectListItem> CallCRDaysWarnList = new List<SelectListItem>();
            CallCRDaysWarnList.Add(new SelectListItem { Value = "W", Text = "Warning" });
            CallCRDaysWarnList.Add(new SelectListItem { Value = "R", Text = "Restrict" });
            return CallCRDaysWarnList;
        }

        public JsonResult AutoCompleteCode(string term)
        {
            if (term == "")
            {
                return Json((from m in ctxTFAT.Master
                             where m.BaseGr == "D" || m.BaseGr == "U"
                             select new { Name = m.Name, Code = m.Code }).Take(10).ToArray(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json((from m in ctxTFAT.Master
                             where m.Name.ToLower().Contains(term.ToLower()) && (m.BaseGr == "D" || m.BaseGr == "U")
                             select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult AutoCompleteUserID(string term)
        {
            if (term == "")
            {
                return Json((from m in ctxTFAT.TfatPass
                             select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json((from m in ctxTFAT.TfatPass
                             where m.Name.ToLower().Contains(term.ToLower())
                             select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion GetLists

        // GET: Accounts/PartyCreditStatusManagement
        public ActionResult Index(PartyCreditStatusManagementVM mModel)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mheader = mModel.Header;
            mModel.HoldTransactions_DocDate = DateTime.Now;
            mModel.HoldTransactions_HoldEnquiryDt1 = DateTime.Now;
            mModel.HoldTransactions_HoldEnquiryDt2 = DateTime.Now;
            mModel.HoldTransactions_HoldQuoteDt1 = DateTime.Now;
            mModel.HoldTransactions_HoldQuoteDt2 = DateTime.Now;
            mModel.HoldTransactions_HoldOrderDt1 = DateTime.Now;
            mModel.HoldTransactions_HoldOrderDt2 = DateTime.Now;
            mModel.HoldTransactions_HoldDespatchDt1 = DateTime.Now;
            mModel.HoldTransactions_HoldDespatchDt2 = DateTime.Now;
            mModel.HoldTransactions_TempCrLimitDt1 = DateTime.Now;
            mModel.HoldTransactions_TempCrLimitDt2 = DateTime.Now;
            mModel.HoldTransactions_HoldInvoiceDt1 = DateTime.Now;
            mModel.HoldTransactions_HoldInvoiceDt2 = DateTime.Now;
            mModel.HoldTransactions_TempCrDayDt1 = DateTime.Now;
            mModel.HoldTransactions_HoldPaymentDt1 = DateTime.Now;
            mModel.HoldTransactions_HoldPaymentDt2 = DateTime.Now;
            mModel.HoldTransactions_TempCrDayDt2 = DateTime.Now;
            mModel.CRLimitWarnList = GetCRLimitWarnList();
            mModel.CRDaysWarnList = GetCRDaysWarnList();
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                mdocument = mModel.Document;
                var mList = ctxTFAT.HoldTransactions.Where(x => (x.RECORDKEY == mModel.Document)).FirstOrDefault();
                var mCode = ctxTFAT.Master.Where(x => x.Code == mList.Code).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mUserID = ctxTFAT.TfatPass.Where(x => x.Code == mList.UserID).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                mModel.HoldTransactions_Code = mCode != null ? mCode.Code.ToString() : "";
                mModel.CodeName = mCode != null ? mCode.Name : "";
                mModel.HoldTransactions_UserID = mUserID != null ? mUserID.Code.ToString() : "";
                mModel.UserIDName = mUserID != null ? mUserID.Name : "";
                mModel.HoldTransactions_Ticklers = mList.Ticklers;
                mModel.HoldTransactions_DocDate = mList.DocDate != null ? mList.DocDate : DateTime.Now;
                mModel.HoldTransactions_HoldEnquiry = mList.HoldEnquiry;
                mModel.HoldTransactions_CheckCRLimit = mList.CheckCRLimit;
                mModel.HoldTransactions_HoldEnquiryDt1 = mList.HoldEnquiryDt1 != null ? mList.HoldEnquiryDt1.Value : DateTime.Now;
                mModel.HoldTransactions_HoldEnquiryDt2 = mList.HoldEnquiryDt2 != null ? mList.HoldEnquiryDt2.Value : DateTime.Now;
                mModel.HoldTransactions_CrLimit = mList.CrLimit != null ? mList.CrLimit.Value : 0;
                mModel.HoldTransactions_CRLimitWarn = mList.CRLimitWarn;
                mModel.HoldTransactions_HoldQuote = mList.HoldQuote;
                mModel.HoldTransactions_CRLimitTole = mList.CRLimitTole != null ? mList.CRLimitTole.Value : 0;
                mModel.HoldTransactions_CRLimitWithTrx = mList.CRLimitWithTrx;
                mModel.HoldTransactions_HoldQuoteDt1 = mList.HoldQuoteDt1 != null ? mList.HoldQuoteDt1.Value : DateTime.Now;
                mModel.HoldTransactions_HoldQuoteDt2 = mList.HoldQuoteDt2 != null ? mList.HoldQuoteDt2.Value : DateTime.Now;
                mModel.HoldTransactions_CRLimitWithPO = mList.CRLimitWithPO;
                mModel.HoldTransactions_CheckCRDays = mList.CheckCRDays;
                mModel.HoldTransactions_HoldOrder = mList.HoldOrder;
                mModel.HoldTransactions_CrPeriod = mList.CrPeriod != null ? mList.CrPeriod.Value : 0;
                mModel.HoldTransactions_HoldOrderDt1 = mList.HoldOrderDt1 != null ? mList.HoldOrderDt1.Value : DateTime.Now;
                mModel.HoldTransactions_CRDaysWarn = mList.CRDaysWarn;
                mModel.HoldTransactions_HoldOrderDt2 = mList.HoldOrderDt2 != null ? mList.HoldOrderDt2.Value : DateTime.Now;
                mModel.HoldTransactions_HoldDespatch = mList.HoldDespatch;
                mModel.HoldTransactions_HoldDespatchDt1 = mList.HoldDespatchDt1 != null ? mList.HoldDespatchDt1.Value : DateTime.Now;
                mModel.HoldTransactions_ChkTempCRLimit = mList.ChkTempCRLimit;
                mModel.HoldTransactions_TempCrLimit = mList.TempCrLimit != null ? mList.TempCrLimit.Value : 0;
                mModel.HoldTransactions_HoldDespatchDt2 = mList.HoldDespatchDt2 != null ? mList.HoldDespatchDt2.Value : DateTime.Now;
                mModel.HoldTransactions_TempCrLimitDt1 = mList.TempCrLimitDt1 != null ? mList.TempCrLimitDt1.Value : DateTime.Now;
                mModel.HoldTransactions_HoldInvoice = mList.HoldInvoice;
                mModel.HoldTransactions_TempCrLimitDt2 = mList.TempCrLimitDt2 != null ? mList.TempCrLimitDt2.Value : DateTime.Now;
                mModel.HoldTransactions_HoldInvoiceDt1 = mList.HoldInvoiceDt1 != null ? mList.HoldInvoiceDt1.Value : DateTime.Now;
                mModel.HoldTransactions_HoldInvoiceDt2 = mList.HoldInvoiceDt2 != null ? mList.HoldInvoiceDt2.Value : DateTime.Now;
                mModel.HoldTransactions_HoldPayment = mList.HoldPayment;
                mModel.HoldTransactions_ChkTempCRDays = mList.ChkTempCRDays;
                mModel.HoldTransactions_TempCrPeriod = mList.TempCrPeriod != null ? mList.TempCrPeriod.Value : 0;
                mModel.HoldTransactions_TempCrDayDt1 = mList.TempCrDayDt1 != null ? mList.TempCrDayDt1.Value : DateTime.Now;
                mModel.HoldTransactions_HoldPaymentDt1 = mList.HoldPaymentDt1 != null ? mList.HoldPaymentDt1.Value : DateTime.Now;
                mModel.HoldTransactions_HoldPaymentDt2 = mList.HoldPaymentDt2 != null ? mList.HoldPaymentDt2.Value : DateTime.Now;
                mModel.HoldTransactions_TempCrDayDt2 = mList.TempCrDayDt2 != null ? mList.TempCrDayDt2.Value : DateTime.Now;
                mModel.HoldTransactions_HoldNarr = mList.HoldNarr;
                mModel.HoldTransactions_TempRemark = mList.TempRemark;
            }
            else
            {
                mModel.Document = 0;
                mModel.HoldTransactions_CheckCRDays = false;
                mModel.HoldTransactions_CheckCRLimit = false;
                mModel.HoldTransactions_ChkTempCRDays = false;
                mModel.HoldTransactions_ChkTempCRLimit = false;
                mModel.HoldTransactions_Code = "";
                mModel.HoldTransactions_CRDaysWarn = false;
                mModel.HoldTransactions_CrLimit = 0;
                mModel.HoldTransactions_CRLimitTole = 0;
                mModel.HoldTransactions_CRLimitWarn = false;
                mModel.HoldTransactions_CRLimitWithPO = false;
                mModel.HoldTransactions_CRLimitWithTrx = false;
                mModel.HoldTransactions_CrPeriod = 0;
                mModel.HoldTransactions_DocDate = System.DateTime.Now;
                mModel.HoldTransactions_HoldDespatch = false;
                mModel.HoldTransactions_HoldDespatchDt1 = System.DateTime.Now;
                mModel.HoldTransactions_HoldDespatchDt2 = System.DateTime.Now;
                mModel.HoldTransactions_HoldEnquiry = false;
                mModel.HoldTransactions_HoldEnquiryDt1 = System.DateTime.Now;
                mModel.HoldTransactions_HoldEnquiryDt2 = System.DateTime.Now;
                mModel.HoldTransactions_HoldInvoice = false;
                mModel.HoldTransactions_HoldInvoiceDt1 = System.DateTime.Now;
                mModel.HoldTransactions_HoldInvoiceDt2 = System.DateTime.Now;
                mModel.HoldTransactions_HoldNarr = "";
                mModel.HoldTransactions_HoldOrder = false;
                mModel.HoldTransactions_HoldOrderDt1 = System.DateTime.Now;
                mModel.HoldTransactions_HoldOrderDt2 = System.DateTime.Now;
                mModel.HoldTransactions_HoldPayment = false;
                mModel.HoldTransactions_HoldPaymentDt1 = System.DateTime.Now;
                mModel.HoldTransactions_HoldPaymentDt2 = System.DateTime.Now;
                mModel.HoldTransactions_HoldQuote = false;
                mModel.HoldTransactions_HoldQuoteDt1 = System.DateTime.Now;
                mModel.HoldTransactions_HoldQuoteDt2 = System.DateTime.Now;
                mModel.HoldTransactions_TempCrDayDt1 = System.DateTime.Now;
                mModel.HoldTransactions_TempCrDayDt2 = System.DateTime.Now;
                mModel.HoldTransactions_TempCrLimit = 0;
                mModel.HoldTransactions_TempCrLimitDt1 = System.DateTime.Now;
                mModel.HoldTransactions_TempCrLimitDt2 = System.DateTime.Now;
                mModel.HoldTransactions_TempCrPeriod = 0;
                mModel.HoldTransactions_TempRemark = "";
                mModel.HoldTransactions_Ticklers = "";
                mModel.HoldTransactions_UserID = "";
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(PartyCreditStatusManagementVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeletePartyCreditStatusManagement(mModel);
                        if (mModel.Mode == "Delete")
                        {
                            transaction.Commit();
                            transaction.Dispose();
                            return Json(new
                            {
                                Status = "Success",
                                Message = "Data is Deleted."
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    HoldTransactions mobj = new HoldTransactions();
                    if (mModel.Mode == "Edit")
                    {
                        mobj = ctxTFAT.HoldTransactions.Where(x => (x.RECORDKEY == mModel.Document)).FirstOrDefault();
                    }
                    mobj.Ticklers = mModel.HoldTransactions_Ticklers;
                    mobj.Code = mModel.HoldTransactions_Code;
                    mobj.DocDate = ConvertDDMMYYTOYYMMDD(mModel.HoldTransactions_DocDateVM);
                    mobj.UserID = mModel.HoldTransactions_UserID;
                    mobj.HoldEnquiry = mModel.HoldTransactions_HoldEnquiry;
                    mobj.CheckCRLimit = mModel.HoldTransactions_CheckCRLimit;
                    mobj.HoldEnquiryDt1 = ConvertDDMMYYTOYYMMDD(mModel.HoldTransactions_HoldEnquiryDt1VM);
                    mobj.HoldEnquiryDt2 = ConvertDDMMYYTOYYMMDD(mModel.HoldTransactions_HoldEnquiryDt2VM);
                    mobj.CrLimit = mModel.HoldTransactions_CrLimit;
                    mobj.CRLimitWarn = mModel.HoldTransactions_CRLimitWarn;
                    mobj.HoldQuote = mModel.HoldTransactions_HoldQuote;
                    mobj.CRLimitTole = mModel.HoldTransactions_CRLimitTole;
                    mobj.CRLimitWithTrx = mModel.HoldTransactions_CRLimitWithTrx;
                    mobj.HoldQuoteDt1 = ConvertDDMMYYTOYYMMDD(mModel.HoldTransactions_HoldQuoteDt1VM);
                    mobj.HoldQuoteDt2 = ConvertDDMMYYTOYYMMDD(mModel.HoldTransactions_HoldQuoteDt2VM);
                    mobj.CRLimitWithPO = mModel.HoldTransactions_CRLimitWithPO;
                    mobj.CheckCRDays = mModel.HoldTransactions_CheckCRDays;
                    mobj.HoldOrder = mModel.HoldTransactions_HoldOrder;
                    mobj.CrPeriod = mModel.HoldTransactions_CrPeriod;
                    mobj.HoldOrderDt1 = ConvertDDMMYYTOYYMMDD(mModel.HoldTransactions_HoldOrderDt1VM);
                    mobj.CRDaysWarn = mModel.HoldTransactions_CRDaysWarn;
                    mobj.HoldOrderDt2 = ConvertDDMMYYTOYYMMDD(mModel.HoldTransactions_HoldOrderDt2VM);
                    mobj.HoldDespatch = mModel.HoldTransactions_HoldDespatch;
                    mobj.HoldDespatchDt1 = ConvertDDMMYYTOYYMMDD(mModel.HoldTransactions_HoldDespatchDt1VM);
                    mobj.ChkTempCRLimit = mModel.HoldTransactions_ChkTempCRLimit;
                    mobj.TempCrLimit = mModel.HoldTransactions_TempCrLimit;
                    mobj.HoldDespatchDt2 = ConvertDDMMYYTOYYMMDD(mModel.HoldTransactions_HoldDespatchDt2VM);
                    mobj.TempCrLimitDt1 = ConvertDDMMYYTOYYMMDD(mModel.HoldTransactions_TempCrLimitDt1VM);
                    mobj.HoldInvoice = mModel.HoldTransactions_HoldInvoice;
                    mobj.TempCrLimitDt2 = ConvertDDMMYYTOYYMMDD(mModel.HoldTransactions_TempCrLimitDt2VM);
                    mobj.HoldInvoiceDt1 = ConvertDDMMYYTOYYMMDD(mModel.HoldTransactions_HoldInvoiceDt1VM);
                    mobj.HoldInvoiceDt2 = ConvertDDMMYYTOYYMMDD(mModel.HoldTransactions_HoldInvoiceDt2VM);
                    mobj.HoldPayment = mModel.HoldTransactions_HoldPayment;
                    mobj.ChkTempCRDays = mModel.HoldTransactions_ChkTempCRDays;
                    mobj.TempCrPeriod = mModel.HoldTransactions_TempCrPeriod;
                    mobj.TempCrDayDt1 = ConvertDDMMYYTOYYMMDD(mModel.HoldTransactions_TempCrDayDt1VM);
                    mobj.HoldPaymentDt1 = ConvertDDMMYYTOYYMMDD(mModel.HoldTransactions_HoldPaymentDt1VM);
                    mobj.HoldPaymentDt2 = ConvertDDMMYYTOYYMMDD(mModel.HoldTransactions_HoldPaymentDt2VM);
                    mobj.TempCrDayDt2 = ConvertDDMMYYTOYYMMDD(mModel.HoldTransactions_TempCrDayDt2VM);
                    mobj.HoldNarr = mModel.HoldTransactions_HoldNarr;
                    mobj.TempRemark = mModel.HoldTransactions_TempRemark;
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mModel.Mode == "Add")
                    {
                        ctxTFAT.HoldTransactions.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mheader, "     " + mperiod.Substring(0, 2) + mobj.Code, DateTime.Now, 0, "", "Save Party Credit Status Management", "NA");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "PartyCreditStatusManagement" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "PartyCreditStatusManagement" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                }
            }
            return Json(new { Status = "Success", id = "PartyCreditStatusManagement", Message = "Data Saved Successfully" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeletePartyCreditStatusManagement(PartyCreditStatusManagementVM mModel)
        {
            var mList = ctxTFAT.HoldTransactions.Where(x => (x.RECORDKEY == mModel.Document)).FirstOrDefault();
            ctxTFAT.HoldTransactions.Remove(mList);
            ctxTFAT.SaveChanges();
            UpdateAuditTrail(mbranchcode, mModel.Mode, mheader, "     " + mperiod.Substring(0, 2) + mList.Code, DateTime.Now, 0, "", "Delete Party Credit Status Management", "NA");
            // delete from narration
            //var mTableKey2 = "HoldTransactions_" + mModel.HoldTransactions_RECORDKEY;
            //var DeleteAddNote = ctxTFAT.Narration.Where(x => x.TableKey == mTableKey2).ToList();
            //foreach (var item in DeleteAddNote)
            //{
            //    ctxTFAT.Narration.Remove(item);
            //    ctxTFAT.SaveChanges();
            //}
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult SaveAddNote(Narration mModel)
        //{
        //    try
        //    {
        //        mModel.Prefix = GetPrefix();
        //        IList<AddNoteVM> Note = new List<AddNoteVM>();

        //        if (Session["AddNote"] != null)
        //        {
        //            Note = (List<AddNoteVM>)Session["AddNote"];
        //        }

        //        Note.Add(new AddNoteVM()
        //        {
        //            Branch = mbranchcode,
        //            Prefix = mModel.Prefix,
        //            Sno = 0,
        //            NarrRich = mModel.Narr,
        //            ENTEREDBY = muserid,
        //            LASTUPDATEDATE = DateTime.Now,
        //            AUTHIDS = muserid,
        //            AUTHORISE = mauthorise,
        //            LocationCode = mlocation
        //        });
        //        Session.Add("AddNote", Note);
        //    }
        //    catch (DbEntityValidationException ex1)
        //    {
        //        string dd1 = ex1.InnerException.Message;
        //    }
        //    return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult EditAddNote(string mCode, Narration mModel)
        //{
        //    ViewBag.ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
        //    var mController = this.ControllerContext.RouteData.Values["controller"].ToString();
        //    mModel.Branch = mbranchcode;
        //    mModel.Sno = 0;
        //    var mobj = ctxTFAT.Narration.Where(x => (x.TableKey) == (mController + mCode)).FirstOrDefault();
        //    if (mobj != null)
        //    {
        //        mModel.NarrRich = mobj.NarrRich.ToString();
        //    }
        //    return Json(new { Status = "Success", Controller = ViewBag.ControllerName, Code = mCode, narval = mModel.NarrRich, valid = mCode, changelog = "edit", changelogs = "view" }, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult EditAddNotes(Narration mModel, string mCode, string mMode)
        //{
        //    try
        //    {
        //        ViewBag.ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
        //        var mController = this.ControllerContext.RouteData.Values["controller"].ToString();
        //        mModel.Prefix = GetPrefix();

        //        var mobj = ctxTFAT.Narration.Where(x => (x.TableKey) == (mController + mCode)).FirstOrDefault();
        //        if (mobj != null)
        //        {
        //            mobj.NarrRich = mModel.Narr;
        //            ctxTFAT.Entry(mobj).State = EntityState.Modified;
        //        }
        //        else
        //        {
        //            Narration mNarr = new Narration
        //            {
        //                Branch = mbranchcode,
        //                Type = mModel.Type,
        //                Prefix = mModel.Prefix,
        //                Srl = mCode,
        //                Sno = 0,
        //                TableKey = mController + mCode,
        //                Narr = mModel.Narr,
        //                NarrRich = mModel.Narr,
        //                ENTEREDBY = muserid,
        //                LASTUPDATEDATE = DateTime.Now,
        //                AUTHIDS = muserid,
        //                AUTHORISE = mauthorise,
        //                LocationCode = mlocation,
        //            };
        //            ctxTFAT.Narration.Add(mNarr);
        //        }
        //    }
        //    catch (DbEntityValidationException ex1)
        //    {
        //        string dd1 = ex1.InnerException.Message;
        //    }
        //    try
        //    {
        //        ctxTFAT.SaveChanges();
        //    }
        //    catch (Exception dbEx)
        //    {
        //        Exception raise = dbEx;
        //        throw raise;
        //    }
        //    return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        //}
        #endregion SaveData
    }
}