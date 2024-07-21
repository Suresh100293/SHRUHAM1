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
    public class OtherTransactionSetupController : BaseController
    {
        // GET: Accounts/OtherTransactionSetup
        public ActionResult Index(OTransSetup Model)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, "Setup", Model.Header, "", DateTime.Now, 0, "", "", "NA");

            var mOtherTrnsSet = ctxTFAT.OtherTransactSetup.Select(x => x).FirstOrDefault();
            if(mOtherTrnsSet != null)
            {
                Model.BillBoth = mOtherTrnsSet.BillBoth;
                if (mOtherTrnsSet.BillBoth == true || mOtherTrnsSet.BillAuto == true)
                {
                    Model.BillManual = false;
                }
                else
                {
                    Model.BillManual = true;
                }
                Model.CutTDS = mOtherTrnsSet.CutTDS;
                Model.TyreStock = mOtherTrnsSet.TyreStockDelete;

                Model.BillAuto = mOtherTrnsSet.BillAuto;
                
                Model.DuplExpLRFMConfirm = mOtherTrnsSet.DuplExpLRFMConfirm;
                Model.NoDuplExpLRFM = mOtherTrnsSet.NoDuplExpLRFM;
                Model.ShowLedgerPost = mOtherTrnsSet.ShowLedgerPost;
                
                Model.ShowDocSerial = mOtherTrnsSet.ShowDocSerial;
                Model.AllowZeroAmt = mOtherTrnsSet.AllowZeroAmt;
                Model.AllowAutoRemark = mOtherTrnsSet.AutoRemark;
                Model.NoDuplExpDt = mOtherTrnsSet.NoDuplExpDt;
                Model.DuplExpDtConfirm = mOtherTrnsSet.DuplExpDtConfirm;
                Model.ShowConsignmentExp = mOtherTrnsSet.ShowConsignmentExp;
                Model.RestrictLrDateExp = mOtherTrnsSet.RestrictLrDateExp;
                Model.RestrictLrExpDays = mOtherTrnsSet.RestrictLrExpDays;
                if (mOtherTrnsSet.NoDuplExpDt == true)
                {
                    Model.ExpDtType = "Restrict";
                }
                else if (mOtherTrnsSet.DuplExpDtConfirm == true)
                {
                    Model.ExpDtType = "Confirm";
                }

                Model.Class_CurrDatetOnlyreq = mOtherTrnsSet.CurrDatetOnlyreq;
                Model.BackDays = mOtherTrnsSet.BackDays;
                Model.BackDated = mOtherTrnsSet.BackDated;
                Model.ForwardDays = mOtherTrnsSet.ForwardDays;
                Model.ForwardDated = mOtherTrnsSet.ForwardDated;
                Model.Class_BranchwiseSrlReq = mOtherTrnsSet.BranchwiseSrlReq;
                Model.Class_YearwiseSrlReq = mOtherTrnsSet.YearwiseSrlReq;
                Model.Class_CetralisedSrlReq = mOtherTrnsSet.CetralisedSrlReq;
                Model.Class_Srl = mOtherTrnsSet.Srl;
                Model.Class_Width = mOtherTrnsSet.Width;
            }
           
          

            return View(Model);
        }


        public void DeUpdate(OTransSetup Model)
        {
            string connstring = GetConnectionString();
            var mobjList = ctxTFAT.OtherTransactSetup.FirstOrDefault();
            if (mobjList != null)
            {
                ctxTFAT.OtherTransactSetup.Remove(mobjList);
            }
          


            ctxTFAT.SaveChanges();
        }

        [HttpPost]
        public ActionResult SaveData(OTransSetup Model)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    //iX9: Remove Existing Data for Delete Mode
                    if (Model.Mode == "Delete")
                    {
                        DeUpdate(Model);
                        transaction.Commit();
                        transaction.Dispose();
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    DeUpdate(Model);

                    OtherTransactSetup OTSET = new OtherTransactSetup();

                    OTSET.AUTHIDS = muserid;
                    OTSET.AUTHORISE = "A00";
                    OTSET.BillAuto = Model.BillAuto;
                    OTSET.BillBoth = Model.BillBoth;
                    OTSET.LASTUPDATEDATE = DateTime.Now;
                    OTSET.ShowLedgerPost = Model.ShowLedgerPost;
                    OTSET.ENTEREDBY = muserid;
                    OTSET.CutTDS = Model.CutTDS;

                    OTSET.ShowConsignmentExp = Model.ShowConsignmentExp;
                    OTSET.RestrictLrDateExp = Model.RestrictLrDateExp;
                    OTSET.RestrictLrExpDays = Model.RestrictLrExpDays;


                    OTSET.NoDuplExpLRFM = Model.NoDuplExpLRFM;
                    OTSET.DuplExpLRFMConfirm = Model.DuplExpLRFMConfirm;
                    OTSET.AutoRemark = Model.AllowAutoRemark;
                    OTSET.AllowZeroAmt = Model.AllowZeroAmt;
                    OTSET.NoDuplExpDt =Model.ExpDtType == "Restrict" ? true : false;
                    OTSET.DuplExpDtConfirm = Model.ExpDtType == "Confirm" ? true : false;
                    OTSET.TyreStockDelete = Model.TyreStock;

                    OTSET.CurrDatetOnlyreq = Model.Class_CurrDatetOnlyreq;
                    OTSET.ForwardDated = Model.ForwardDated;
                    OTSET.ForwardDays = Model.ForwardDays;
                    OTSET.BackDated = Model.BackDated;
                    OTSET.BackDays = Model.BackDays;
                    OTSET.BranchwiseSrlReq = Model.Class_BranchwiseSrlReq;
                    OTSET.YearwiseSrlReq = Model.Class_YearwiseSrlReq;
                    OTSET.CetralisedSrlReq = Model.Class_CetralisedSrlReq;
                    OTSET.Srl = Model.Class_Srl;
                    OTSET.Width = Model.Class_Width;

                    ctxTFAT.OtherTransactSetup.Add(OTSET);

                    DocTypes docTypes = ctxTFAT.DocTypes.Where(x => x.Code == "CPO00").FirstOrDefault();
                    if (docTypes != null)
                    {
                        docTypes.GSTNoCommon = Model.Class_BranchwiseSrlReq;
                        docTypes.CommonSeries = Model.Class_YearwiseSrlReq;
                        docTypes.Centralised = Model.Class_CetralisedSrlReq;
                        docTypes.LimitFrom = Model.Class_Srl.ToString().Trim();
                        docTypes.LimitTo = "999999".PadLeft(docTypes.LimitFrom.Length, '9');
                        docTypes.DocWidth = Convert.ToInt32(Model.Class_Width);
                        ctxTFAT.Entry(docTypes).State = EntityState.Modified;
                    }

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, "Setup", Model.Header, "", DateTime.Now, 0, "", "Save Cash Bank Transaction Setup", "NA");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "Other Transaction Setup" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "Other Transaction Setup" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "Other Transaction Setup" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
        }
    }
}