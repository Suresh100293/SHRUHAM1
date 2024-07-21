using ALT_ERP3.Areas.Vehicles.Models;
using ALT_ERP3.Controllers;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Vehicles.Controllers
{
    public class TripSheetSetupController : BaseController
    {
        private static string mauthorise = "A00";
        private static string mdocument = "";

        public JsonResult LoadDebitAc(string term)
        {
            var list = ctxTFAT.Master.Where(x => x.AcType == "X" || x.AcType == "E").ToList();

            if (!String.IsNullOrEmpty(term))
            {
                list = list.Where(x => x.AcType == "X" && x.Name.ToLower().Contains(term.ToLower())).ToList();
            }


            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }
        private List<SelectListItem> PopulatePrintFormats()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            var list = ctxTFAT.DocFormats.Where(x => x.Type == "Trip0").OrderBy(x=>x.FormatCode).ToList();
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
        // GET: Vehicles/TripSheetSetup
        public ActionResult Index(TripSheetSetupVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, "Setup", mModel.Header, "", DateTime.Now, 0, "", "","NA");
            mdocument = mModel.Document;
            mModel.PrintFormats = PopulatePrintFormats();

            var mList = ctxTFAT.TripSheetSetup.FirstOrDefault();
            if (mList != null)
            {
                mModel.DebitAc = mList.DebitAc;
                mModel.DebitAcName = ctxTFAT.Master.Where(x => x.Code == mList.DebitAc).Select(x => x.Name).FirstOrDefault();
                mModel.ChangeAc = mList.ChgDebitAc;
                mModel.TDSDeduction = mList.TDSDeduction;
                mModel.ChangeCharge = mList.ChangeChrgAmt;
                mModel.SplitPosting = mList.SplitPostingReq;
                mModel.FetchFmFrom = mList.FetchFmFrom == null ? "" : mList.FetchFmFrom.Value.ToShortDateString();
                mModel.PrintFormat = mList.DefaultPrint;
                mModel.NoDocumentAllow = mList.NoDocumentAllow;
                mModel.RestrictLrDateExp = mList.ShowConsignmentExp;
                mModel.RestrictLrDateExp = mList.RestrictLrDateExp;
                mModel.RestrictLrExpDays = mList.RestrictLrExpDays;
                mModel.Class_Width = mList.Width;

                mModel.Class_CurrDatetOnlyreq = mList.CurrDatetOnlyreq;
                mModel.Class_BackDaysUpto = mList.BackDaysUpto;
                mModel.Class_BackDateAllow = mList.BackDateAllow;
                mModel.Class_ForwardDaysUpto = mList.ForwardDaysUpto;
                mModel.Class_ForwardDateAllow = mList.ForwardDateAllow;
                mModel.Class_BranchwiseSrlReq = mList.BranchwiseSrlReq;
                mModel.Class_YearwiseSrlReq = mList.YearwiseSrlReq;
                mModel.Class_CetralisedSrlReq = mList.CetralisedSrlReq;
                mModel.Class_Srl = mList.Srl;
                mModel.DriverTripDefault = mList.DriverTripDefault;
                mModel.TripFMDefaultDoc = mList.TripFMDefaultDoc;
                mModel.Pick_Financial_Document = mList.Pick_Financial_Document;
                mModel.ShowSummary = mList.ShowSummary;

                mModel.FitnessExp = mList.FitnessExp;
                mModel.InsuranceExp = mList.InsuranceExp;
                mModel.PUCExp = mList.PUCExp;
                mModel.AIPExp = mList.AIPExp;
                mModel.StateTaxExp = mList.StateTaxExp;
                mModel.TPStateExp = mList.TPStateExp;
                mModel.GreenTaxExp = mList.GreenTaxExp;
                mModel.DriverExp = mList.DriverExp;
                mModel.ConfirmDupDateOfTrip = mList.ConfirmDupDateOfTrip;
                mModel.RestrictDupDateOfTrip = mList.RestrictDupDateOfTrip;

            }

            return View(mModel);
        }

        public ActionResult SaveData(TripSheetSetupVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {

                    TripSheetSetup mobj = new TripSheetSetup();
                    bool mAdd = true;
                    var id = Convert.ToInt64(1);
                    if (ctxTFAT.TripSheetSetup.FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.TripSheetSetup.FirstOrDefault();
                        mAdd = false;
                    }

                    mobj.DebitAc = mModel.DebitAc;
                    mobj.ChgDebitAc = mModel.ChangeAc;
                    mobj.TDSDeduction = mModel.TDSDeduction;
                    mobj.ChangeChrgAmt = mModel.ChangeCharge;
                    mobj.SplitPostingReq = mModel.SplitPosting;
                    mobj.NoDocumentAllow = mModel.NoDocumentAllow;
                    mobj.ShowConsignmentExp = mModel.ShowConsignmentExp;
                    mobj.RestrictLrDateExp = mModel.RestrictLrDateExp;
                    mobj.RestrictLrExpDays = mModel.RestrictLrExpDays;
                    mobj.ShowSummary = mModel.ShowSummary;
                    if (String.IsNullOrEmpty(mModel.FetchFmFrom))
                    {
                        mobj.FetchFmFrom = null;
                    }
                    else
                    {
                        mobj.FetchFmFrom = ConvertDDMMYYTOYYMMDD(mModel.FetchFmFrom);
                    }

                    mobj.CurrDatetOnlyreq = mModel.Class_CurrDatetOnlyreq;
                    mobj.ForwardDateAllow = mModel.Class_ForwardDateAllow;
                    mobj.ForwardDaysUpto = mModel.Class_ForwardDaysUpto;
                    mobj.BackDateAllow = mModel.Class_BackDateAllow;
                    mobj.BackDaysUpto = mModel.Class_BackDaysUpto;
                    mobj.BranchwiseSrlReq = mModel.Class_BranchwiseSrlReq;
                    mobj.YearwiseSrlReq = mModel.Class_YearwiseSrlReq;
                    mobj.CetralisedSrlReq = mModel.Class_CetralisedSrlReq;
                    mobj.Srl = mModel.Class_Srl;
                    mobj.Width = mModel.Class_Width;
                    mobj.TripFMDefaultDoc = mModel.TripFMDefaultDoc;
                    mobj.DriverTripDefault = mModel.DriverTripDefault;

                    mobj.FitnessExp = mModel.FitnessExp;
                    mobj.InsuranceExp = mModel.InsuranceExp;
                    mobj.PUCExp = mModel.PUCExp;
                    mobj.AIPExp = mModel.AIPExp;
                    mobj.StateTaxExp = mModel.StateTaxExp;
                    mobj.TPStateExp = mModel.TPStateExp;
                    mobj.GreenTaxExp = mModel.GreenTaxExp;
                    mobj.DriverExp = mModel.DriverExp;

                    mobj.ConfirmDupDateOfTrip = mModel.ConfirmDupDateOfTrip;
                    mobj.RestrictDupDateOfTrip = mModel.RestrictDupDateOfTrip;
                    mobj.Pick_Financial_Document = mModel.Pick_Financial_Document;

                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    mobj.AUTHORISE = mauthorise;
                    mobj.AUTHIDS = muserid;
                    mobj.DefaultPrint = "";
                    ExecuteStoredProc("Update DocFormats set Selected='false' where type='Trip0'");
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
                        ctxTFAT.TripSheetSetup.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }

                    DocTypes docTypes = ctxTFAT.DocTypes.Where(x => x.Code == "Trip0").FirstOrDefault();
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



                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, "Setup", mModel.Header, "", DateTime.Now, 0, "", "Save TripSheet Setup", "NA");
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

        
    }
}