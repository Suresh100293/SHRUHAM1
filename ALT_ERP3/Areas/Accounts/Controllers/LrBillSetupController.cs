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
    public class LrBillSetupController : BaseController
    {
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;

        private List<SelectListItem> PopulatePrintFormats()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            var list = ctxTFAT.DocFormats.Where(x => x.Type == "SLR00").OrderBy(x => x.FormatCode).ToList();
            foreach (var item in list)
            {
                items.Add(new SelectListItem
                {
                    Text = item.FormatCode.ToString(),
                    Value = item.FormatCode.ToString()
                });
            }
            return items;
        }

        // GET: Accounts/LrBillSetup
        public ActionResult Index(LrBillSetupVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());

            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;
            mModel.PrintFormats = PopulatePrintFormats();

            var mList = ctxTFAT.LRBillSetup.FirstOrDefault();
            if (mList != null)
            {
                if (mList.BillBoth == true)
                {
                    mModel.Both = true;
                }
                else if (mList.BillAuto == true)
                {
                    mModel.Automatic = true;
                }
                else
                {
                    mModel.Manual = true;
                }

                mModel.ShowLR = mList.OthLRShow;
                mModel.CutTDS = mList.CutTDS;
                mModel.ShowLedgerPost = mList.ShowLedgerPost;
                mModel.PODReq = mList.PODReq;
                mModel.FooterDetails1 = mList.FooterDetails1;
                mModel.FooterDetails2 = mList.FooterDetails2;
                mModel.FooterDetails3 = mList.FooterDetails3;
                mModel.FooterDetails4 = mList.FooterDetails4;
                mModel.FooterDetails5 = mList.FooterDetails5;
                mModel.FooterDetails6 = mList.FooterDetails6;
                mModel.FooterDetails7 = mList.FooterDetails7;
                mModel.PrintFormat = mList.DefaultPrint;
                mModel.MergeSerial = mList.MergeSerial;

                mModel.Class_CurrDatetOnlyreq = mList.CurrDatetOnlyreq;
                mModel.Class_BackDateAllow = mList.BackDateAllow;
                mModel.Class_BackDaysUpto = mList.BackDaysUpto;
                mModel.Class_ForwardDateAllow = mList.ForwardDateAllow;
                mModel.Class_ForwardDaysUpto = mList.ForwardDaysUpto;
                mModel.Class_BranchwiseSrlReq = mList.BranchwiseSrlReq;
                mModel.Class_YearwiseSrlReq = mList.YearwiseSrlReq;
                mModel.Class_CetralisedSrlReq = mList.CetralisedSrlReq;
                mModel.Class_Srl = mList.Srl;
                mModel.Class_Width = mList.Width;
                mModel.follow_GST_HSN_Ledgerwise = mList.follow_GST_HSN_Ledgerwise;
            }

            return View(mModel);
        }

        public ActionResult SaveData(LrBillSetupVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    //iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }

                    LRBillSetup mobj = new LRBillSetup();
                    bool mAdd = true;

                    if (ctxTFAT.LRBillSetup.FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.LRBillSetup.FirstOrDefault();
                        mAdd = false;
                    }

                    if (mModel.Both == true)
                    {
                        mobj.BillBoth = true;
                        mobj.BillAuto = false;
                    }
                    else if (mModel.Automatic == true)
                    {
                        mobj.BillBoth = false;
                        mobj.BillAuto = true;
                    }
                    else
                    {
                        mobj.BillBoth = false;
                        mobj.BillAuto = false;
                    }

                    mobj.OthLRShow = mModel.ShowLR;
                    mobj.CutTDS = mModel.CutTDS;
                    mobj.ShowLedgerPost = mModel.ShowLedgerPost;
                    mobj.PODReq = mModel.PODReq;
                    mobj.follow_GST_HSN_Ledgerwise = mModel.follow_GST_HSN_Ledgerwise;
                    mobj.MergeSerial = mModel.MergeSerial;

                    mobj.FooterDetails1 = mModel.FooterDetails1;
                    mobj.FooterDetails2 = mModel.FooterDetails2;
                    mobj.FooterDetails3 = mModel.FooterDetails3;
                    mobj.FooterDetails4 = mModel.FooterDetails4;
                    mobj.FooterDetails5 = mModel.FooterDetails5;
                    mobj.FooterDetails6 = mModel.FooterDetails6;
                    mobj.FooterDetails7 = mModel.FooterDetails7;

                    mobj.CurrDatetOnlyreq = mModel.Class_CurrDatetOnlyreq;
                    mobj.BackDateAllow = mModel.Class_BackDateAllow;
                    mobj.BackDaysUpto = mModel.Class_BackDaysUpto;
                    mobj.ForwardDateAllow = mModel.Class_ForwardDateAllow;
                    mobj.ForwardDaysUpto = mModel.Class_ForwardDaysUpto;
                    mobj.BranchwiseSrlReq = mModel.Class_BranchwiseSrlReq;
                    mobj.YearwiseSrlReq = mModel.Class_YearwiseSrlReq;
                    mobj.CetralisedSrlReq = mModel.Class_CetralisedSrlReq;
                    mobj.Srl = mModel.Class_Srl;
                    mobj.Width = mModel.Class_Width;

                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    mobj.AUTHORISE = mauthorise;
                    mobj.AUTHIDS = muserid;
                    mobj.DefaultPrint = "";
                    ExecuteStoredProc("Update DocFormats set Selected='false' where type='SLR00'");
                    if (!String.IsNullOrEmpty(mModel.PrintFormat))
                    {
                        mobj.DefaultPrint = mModel.PrintFormat;
                        var DefaultList = mModel.PrintFormat.Split(',').ToList();
                        var Formatlist = ctxTFAT.DocFormats.Where(x => DefaultList.Contains(x.FormatCode)).ToList();
                        foreach (var item in Formatlist)
                        {
                            item.Selected = true;
                            ctxTFAT.Entry(item).State = EntityState.Modified;
                        }
                    }

                    DocTypes docTypes = ctxTFAT.DocTypes.Where(x => x.Code == "SLR00").FirstOrDefault();
                    if (docTypes != null)
                    {
                        docTypes.GSTNoCommon = mModel.Class_BranchwiseSrlReq;
                        docTypes.CommonSeries = mModel.Class_YearwiseSrlReq;
                        docTypes.Centralised = mModel.Class_CetralisedSrlReq;
                        docTypes.LimitFrom = mModel.Class_Srl.ToString().Trim();
                        docTypes.LimitTo = "999999".PadLeft(docTypes.LimitFrom.Length, '9');
                        docTypes.DocWidth = Convert.ToInt32(mModel.Class_Width);
                        ctxTFAT.Entry(docTypes).State = EntityState.Modified;
                    }

                    if (mAdd == true)
                    {
                        ctxTFAT.LRBillSetup.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "Save Credit Sale Setup (Invoice)", "NA");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteStateMaster(LrBillSetupVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Charge Type not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var mList = ctxTFAT.LRBillSetup.FirstOrDefault();
            ctxTFAT.LRBillSetup.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
    }
}