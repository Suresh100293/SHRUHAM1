using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class LCSetupController : BaseController
    {

        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;

        #region Functions

        public DateTime ConvertDDMMYYTOYYMMDD(string da)
        {
            string abc = da.Substring(6, 4) + "/" + da.Substring(3, 2) + "/" + da.Substring(0, 2);
            return Convert.ToDateTime(abc);
        }

        private List<SelectListItem> PopulatePrintFormats()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            var list = ctxTFAT.DocFormats.Where(x => x.Type == "LC000").OrderBy(x => x.FormatCode).ToList();
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

        #endregion

        // GET: Logistics/LCSetup
        public ActionResult Index(LCSetupVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, "Setup", mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;
            mModel.PrintFormats = PopulatePrintFormats();
            var mList = ctxTFAT.LCSetup.FirstOrDefault();
            if (mList != null)
            {
                if (mList.LCBoth == true)
                {
                    mModel.Both = true;
                }
                else if (mList.LCGenerate == true)
                {
                    mModel.LcAutomatic = true;
                }
                else
                {
                    mModel.LcManual = true;
                }


                mModel.FillFromCurr = mList.FillFromCurr;

                mModel.EditReq = mList.EditReq;
                mModel.DeleteReq = mList.DeleteReq;
                mModel.EditHours = mList.EditUptoHours;
                mModel.DeleteHours = mList.DeleteUptoHours;

                mModel.CheckManualLC = mList.ManualLcCheck;
                mModel.CheckLrDate = mList.CheckLrDate;
                mModel.FooterDetails1 = mList.FooterDetails1;
                mModel.FooterDetails2 = mList.FooterDetails2;
                mModel.FooterDetails3 = mList.FooterDetails3;
                mModel.FooterDetails4 = mList.FooterDetails4;
                mModel.PrintFormat = mList.DefaultPrint;

                mModel.Class_CurrDatetOnlyreq = mList.CurrDatetOnlyreq;
                mModel.Class_BackDateAllow = mList.BackDateAllow;
                mModel.Class_BackDaysUpto = mList.BackDaysUpto;
                mModel.Class_ForwardDateAllow = mList.ForwardDateAllow;
                mModel.Class_ForwardDaysUpto = mList.ForwardDaysUpto;
                mModel.Class_BranchwiseSrlReq = mList.BranchwiseSrlReq;
                mModel.Class_YearwiseSrlReq = mList.YearwiseSrlReq;
                mModel.Class_CetralisedSrlReq = mList.CetralisedSrlReq;
                mModel.Class_Srl = mList.Srl;

                mModel.YearwiseManualSrlReq = mList.YearwiseManualSrlReq;
                mModel.CetralisedManualSrlReq = mList.CetralisedManualSrlReq;
            }

            //ReportHeader reportHeader = ctxTFAT.ReportHeader.Where(x => x.Code == "LCMaster").FirstOrDefault();
            //if (reportHeader.OrderBy.Contains("Lc.LcDate"))
            //{
            //    mModel.SelectColumn = (AscDescLC)Enum.Parse(typeof(AscDescLC), "DocumentDate");

            //}
            //else
            //{
            //    mModel.SelectColumn = (AscDescLC)Enum.Parse(typeof(AscDescLC), "LCNo");

            //}
            //if (reportHeader.OrderBy.Contains("asc"))
            //{

            //    mModel.Asc = true;
            //}
            //else
            //{

            //    mModel.Asc = false;
            //}

            return View(mModel);
        }
        public ActionResult SaveData(LCSetupVM mModel)
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

                    LCSetup mobj = new LCSetup();
                    bool mAdd = true;
                    if (ctxTFAT.LCSetup.FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.LCSetup.FirstOrDefault();
                        mAdd = false;
                    }

                    if (mModel.Both == true)
                    {
                        mobj.LCBoth = true;
                        mobj.LCGenerate = false;
                    }
                    else if (mModel.LcAutomatic == true)
                    {
                        mobj.LCBoth = false;
                        mobj.LCGenerate = true;
                    }
                    else
                    {
                        mobj.LCBoth = false;
                        mobj.LCGenerate = false;
                    }

                    mobj.EditUptoHours = mModel.EditHours;
                    mobj.DeleteUptoHours = mModel.DeleteHours;
                    mobj.EditReq = mModel.EditReq;
                    mobj.DeleteReq = mModel.DeleteReq;
                    mobj.ManualLcCheck = mModel.CheckManualLC;
                    mobj.CheckLrDate = mModel.CheckLrDate;
                    mobj.FillFromCurr = mModel.FillFromCurr;
                    mobj.FooterDetails1 = mModel.FooterDetails1;
                    mobj.FooterDetails2 = mModel.FooterDetails2;
                    mobj.FooterDetails3 = mModel.FooterDetails3;
                    mobj.FooterDetails4 = mModel.FooterDetails4;

                    mobj.CurrDatetOnlyreq = mModel.Class_CurrDatetOnlyreq;
                    mobj.BackDateAllow = mModel.Class_BackDateAllow;
                    mobj.BackDaysUpto = mModel.Class_BackDaysUpto;
                    mobj.ForwardDateAllow = mModel.Class_ForwardDateAllow;
                    mobj.ForwardDaysUpto = mModel.Class_ForwardDaysUpto;
                    mobj.BranchwiseSrlReq = mModel.Class_BranchwiseSrlReq;
                    mobj.YearwiseSrlReq = mModel.Class_YearwiseSrlReq;
                    mobj.CetralisedSrlReq = mModel.Class_CetralisedSrlReq;
                    mobj.Srl = mModel.Class_Srl;

                    mobj.YearwiseManualSrlReq = mModel.YearwiseManualSrlReq;
                    mobj.CetralisedManualSrlReq = mModel.CetralisedManualSrlReq;

                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    mobj.AUTHORISE = mauthorise;
                    mobj.AUTHIDS = muserid;
                    mobj.DefaultPrint = "";
                    ExecuteStoredProc("Update DocFormats set Selected='false' where type='LC000'");
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

                    if (mAdd == true)
                    {
                        ctxTFAT.LCSetup.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }

                    DocTypes docTypes = ctxTFAT.DocTypes.Where(x => x.Code == "LC000").FirstOrDefault();
                    if (docTypes != null)
                    {
                        docTypes.GSTNoCommon = mModel.Class_BranchwiseSrlReq;
                        docTypes.CommonSeries = mModel.Class_YearwiseSrlReq;
                        docTypes.Centralised = mModel.Class_CetralisedSrlReq;
                        docTypes.LimitFrom = mModel.Class_Srl.ToString().Trim();
                        docTypes.LimitTo = "999999".PadLeft(docTypes.LimitFrom.Length, '9');
                        docTypes.DocWidth = docTypes.LimitFrom.Length;
                        ctxTFAT.Entry(docTypes).State = EntityState.Modified;
                    }


                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, "Setup", mModel.Header, "", DateTime.Now, 0, "", "Save Lorry Challan Setup", "NA");
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

        public ActionResult DeleteStateMaster(LCSetupVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Charge Type not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var id = Convert.ToInt64(mModel.Document);
            var mList = ctxTFAT.LRSetup.Where(x => (x.RECORDKEY == id)).FirstOrDefault();
            ctxTFAT.LRSetup.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
    }
}