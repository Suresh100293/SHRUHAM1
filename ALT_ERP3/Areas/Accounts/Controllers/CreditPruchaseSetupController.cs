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
    public class CreditPruchaseSetupController : BaseController
    {
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;

        // GET: Accounts/CreditPruchaseSetup
        public ActionResult Index(CreditPurSetupVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());

            UpdateAuditTrail(mbranchcode, "Setup", mModel.Header, "", DateTime.Now, 0, "", "", "NA");


            var mList = ctxTFAT.CreditPurchaseSetup.FirstOrDefault();
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

                mModel.CutTDS = mList.CutTDS;
                mModel.ShowLedgerPost = mList.ShowLedgerPost;
                //mModel.RelatedACReq = mList.RelatedPosting;
                //mModel.CCAmtMatch = mList.CCAmtMatch;
                
                mModel.DuplExpLRFMConfirm = mList.DuplExpLRFMConfirm;
                mModel.NoDuplExpLRFM = mList.NoDuplExpLRFM;
                
                mModel.ShowDocSerial = mList.ShowDocSerial;
                mModel.AllowZeroAmt = mList.AllowZeroAmt;
                mModel.AllowAutoRemark = mList.AutoRemark;
                mModel.NoDuplExpDt = mList.NoDuplExpDt;
                mModel.DuplExpDtConfirm = mList.DuplExpDtConfirm;
                //mModel.ActiveBranchTransfer = mList.ActiveBranchTransfer;
                mModel.TyreStock = mList.TyreStockDelete;
                mModel.SplitPosting = mList.SplitPosting;
                mModel.ShowConsignmentExp = mList.ShowConsignmentExp;
                mModel.RestrictLrDateExp = mList.RestrictLrDateExp;
                mModel.RestrictLrExpDays = mList.RestrictLrExpDays;
                if (mList.NoDuplExpDt == true)
                {
                    mModel.ExpDtType = "Restrict";
                }
                else if (mList.DuplExpDtConfirm == true)
                {
                    mModel.ExpDtType = "Confirm";
                }

                mModel.Class_CurrDatetOnlyreq = mList.CurrDatetOnlyreq;
                mModel.BackDays = mList.BackDays;
                mModel.BackDated = mList.BackDated;
                mModel.ForwardDays = mList.ForwardDays;
                mModel.ForwardDated = mList.ForwardDated;
                mModel.Class_BranchwiseSrlReq = mList.BranchwiseSrlReq;
                mModel.Class_YearwiseSrlReq = mList.YearwiseSrlReq;
                mModel.Class_CetralisedSrlReq = mList.CetralisedSrlReq;
                mModel.Class_Srl = mList.Srl;
                mModel.Class_Width = mList.Width;
            }

            return View(mModel);
        }

        public ActionResult SaveData(CreditPurSetupVM mModel)
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

                    CreditPurchaseSetup mobj = new CreditPurchaseSetup();
                    bool mAdd = true;

                    if (ctxTFAT.CreditPurchaseSetup.FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.CreditPurchaseSetup.FirstOrDefault();
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

                    mobj.CutTDS = mModel.CutTDS;
                    mobj.ShowLedgerPost = mModel.ShowLedgerPost;
                    //mobj.RelatedPosting = mModel.RelatedACReq;
                    mobj.ShowConsignmentExp = mModel.ShowConsignmentExp;
                    mobj.RestrictLrDateExp = mModel.RestrictLrDateExp;
                    mobj.RestrictLrExpDays = mModel.RestrictLrExpDays;
                    mobj.SplitPosting = mModel.SplitPosting;
                   
                    mobj.NoDuplExpLRFM = mModel.NoDuplExpLRFM;
                    
                    //mobj.CCAmtMatch = mModel.CCAmtMatch;
                    mobj.DuplExpLRFMConfirm = mModel.DuplExpLRFMConfirm;
                    mobj.AutoRemark = mModel.AllowAutoRemark;
                    mobj.AllowZeroAmt = mModel.AllowZeroAmt;
                    //mobj.ActiveBranchTransfer = mModel.ActiveBranchTransfer;
                    mobj.NoDuplExpDt = mModel.ExpDtType == "Restrict" ? true : false;
                    mobj.DuplExpDtConfirm = mModel.ExpDtType == "Confirm" ? true : false;
                    mobj.TyreStockDelete = mModel.TyreStock;

                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    mobj.AUTHORISE = mauthorise;
                    mobj.AUTHIDS = muserid;

                    mobj.CurrDatetOnlyreq = mModel.Class_CurrDatetOnlyreq;
                    mobj.ForwardDated = mModel.ForwardDated;
                    mobj.ForwardDays = mModel.ForwardDays;
                    mobj.BackDated = mModel.BackDated;
                    mobj.BackDays = mModel.BackDays;
                    mobj.BranchwiseSrlReq = mModel.Class_BranchwiseSrlReq;
                    mobj.YearwiseSrlReq = mModel.Class_YearwiseSrlReq;
                    mobj.CetralisedSrlReq = mModel.Class_CetralisedSrlReq;
                    mobj.Srl = mModel.Class_Srl;
                    mobj.Width = mModel.Class_Width;

                    DocTypes docTypes = ctxTFAT.DocTypes.Where(x => x.Code == "PUR00").FirstOrDefault();
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
                        ctxTFAT.CreditPurchaseSetup.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, "Setup", mModel.Header, "", DateTime.Now, 0, "", "Save CreditPruchase-Setup", "NA");
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

        public ActionResult DeleteStateMaster(CreditPurSetupVM mModel)
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
            //ctxTFAT.LRBillSetup.Remove(mList);
            //ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
    }
}