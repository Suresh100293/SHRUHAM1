using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Models;
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
    public class CreditDebitNoteSetupController : BaseController
    {
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;

        private List<SelectListItem> PopulatePrintFormats()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            var list = ctxTFAT.DocFormats.Where(x => x.Type == "CRN00" || x.Type == "DRN00").OrderBy(x => x.FormatCode).ToList();
            foreach (var item in list.OrderBy(x=>x.Type))
            {
                items.Add(new SelectListItem
                {
                    Text = item.FormatCode.ToString(),
                    Value = item.FormatCode.ToString()
                });
            }
            return items;
        }


        // GET: Accounts/CreditDebitNoteSetup
        public ActionResult Index(CreditDebitNoteSetupVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, "Setup", mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;
            mModel.PrintFormats = PopulatePrintFormats();

            var mList = ctxTFAT.CreditDebitNoteSetup.FirstOrDefault();
            if (mList != null)
            {
                mModel.FooterDetails1 = mList.FooterDetails1;
                mModel.FooterDetails2 = mList.FooterDetails2;
                mModel.FooterDetails3 = mList.FooterDetails3;
                mModel.FooterDetails4 = mList.FooterDetails4;
                mModel.FooterDetails5 = mList.FooterDetails5;
                mModel.FooterDetails6 = mList.FooterDetails6;
                mModel.FooterDetails7 = mList.FooterDetails7;
                mModel.PrintFormat = mList.DefaultPrint;
            }

            return View(mModel);
        }

        public ActionResult SaveData(CreditDebitNoteSetupVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    //iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        //DeleteStateMaster(mModel);
                        //transaction.Commit();
                        //transaction.Dispose();
                        //return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }

                    CreditDebitNoteSetup mobj = new CreditDebitNoteSetup();
                    bool mAdd = true;

                    if (ctxTFAT.CreditDebitNoteSetup.FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.CreditDebitNoteSetup.FirstOrDefault();
                        mAdd = false;
                    }

                    mobj.FooterDetails1 = mModel.FooterDetails1;
                    mobj.FooterDetails2 = mModel.FooterDetails2;
                    mobj.FooterDetails3 = mModel.FooterDetails3;
                    mobj.FooterDetails4 = mModel.FooterDetails4;
                    mobj.FooterDetails5 = mModel.FooterDetails5;
                    mobj.FooterDetails6 = mModel.FooterDetails6;
                    mobj.FooterDetails7 = mModel.FooterDetails7;

                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    mobj.AUTHORISE = mauthorise;
                    mobj.AUTHIDS = muserid;
                    mobj.DefaultPrint = "";
                    ExecuteStoredProc("Update DocFormats set Selected='false' where type='CRN00' or type='DRN00'");
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
                        ctxTFAT.CreditDebitNoteSetup.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, "Setup", mModel.Header, "", DateTime.Now, 0, "", "Save Credit/Debit Note Setup", "NA");
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