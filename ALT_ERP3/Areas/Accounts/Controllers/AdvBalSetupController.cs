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
    public class AdvBalSetupController : BaseController
    {
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;

        private List<SelectListItem> PopulatePrintFormats()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            var list = ctxTFAT.DocFormats.Where(x => x.Type == "FMP00").OrderBy(x => x.FormatCode).ToList();
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

        // GET: Accounts/AdvBalSetup
        public ActionResult Index(AdvBalSetupVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, "Setup", mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;
            mModel.PrintFormats = PopulatePrintFormats();

            var mList = ctxTFAT.AdvBalSetup.FirstOrDefault();
            if (mList != null)
            {
                if (mList.ADBBoth == true)
                {
                    mModel.Both = true;
                }
                else if (mList.ADBAuto == true)
                {
                    mModel.Automatic = true;
                }
                else
                {
                    mModel.Manual = true;
                }
                mModel.CutBillwiseTDS = mList.BillwiseCutTds;
                mModel.CutTDS = mList.CutTDS;
                mModel.Roundoff = mList.RoundOff;
                mModel.FooterDetails1 = mList.FooterDetails1;
                mModel.FooterDetails2 = mList.FooterDetails2;
                mModel.PrintFormat = mList.DefaultPrint;
                mModel.ShowConsignmentExp = mList.ShowConsignmentExp;
                mModel.RestrictLrDateExp = mList.RestrictLrDateExp;
                mModel.RestrictLrExpDays = mList.RestrictLrExpDays;

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
            }
            else
            {
                mModel.Roundoff = 0;
            }

            return View(mModel);
        }

        public ActionResult SaveData(AdvBalSetupVM mModel)
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

                    AdvBalSetup mobj = new AdvBalSetup();
                    bool mAdd = true;
                    var id = Convert.ToInt64(1);
                    if (ctxTFAT.AdvBalSetup.FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.AdvBalSetup.FirstOrDefault();
                        mAdd = false;
                    }


                    if (mModel.Both == true)
                    {
                        mobj.ADBBoth = true;
                        mobj.ADBAuto = false;
                    }
                    else if (mModel.Automatic == true)
                    {
                        mobj.ADBBoth = false;
                        mobj.ADBAuto = true;
                    }
                    else
                    {
                        mobj.ADBBoth = false;
                        mobj.ADBAuto = false;
                    }
                    mobj.CutTDS = mModel.CutTDS;
                    mobj.BillwiseCutTds = mModel.CutBillwiseTDS;
                    mobj.RoundOff = mModel.Roundoff;

                    mobj.FooterDetails1 = mModel.FooterDetails1;
                    mobj.FooterDetails2 = mModel.FooterDetails2;

                    mobj.ShowConsignmentExp = mModel.ShowConsignmentExp;
                    mobj.RestrictLrDateExp = mModel.RestrictLrDateExp;
                    mobj.RestrictLrExpDays = mModel.RestrictLrExpDays;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    mobj.AUTHORISE = mauthorise;
                    mobj.AUTHIDS = muserid;

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

                    mobj.DefaultPrint = "";
                    ExecuteStoredProc("Update DocFormats set Selected='false' where type='FMP00'");
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

                    DocTypes docTypes = ctxTFAT.DocTypes.Where(x => x.Code == "FMP00").FirstOrDefault();
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
                        mobj.ENTEREDBY = muserid;
                        ctxTFAT.AdvBalSetup.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, "Setup", mModel.Header, "", DateTime.Now, 0, "", "Save Advance Bal Setup", "NA");
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

        public ActionResult DeleteStateMaster(AdvBalSetupVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Charge Type not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var mList = ctxTFAT.AdvBalSetup.FirstOrDefault();
            ctxTFAT.AdvBalSetup.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
    }
}