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
    public class UnloadSetupController : BaseController
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

        #endregion

        // GET: Logistics/UnloadSetup
        public ActionResult Index(UnloadSetupVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, "Setup", mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;

            var id = Convert.ToInt64(1);
            var mList = ctxTFAT.UnLoadSetup.Where(x => (x.RECORDKEY == id)).FirstOrDefault();
            if (mList != null)
            {
                mModel.UnloadInAnotherBranch = mList.UnloadInOthrBra;
                mModel.UnloadMadatory = mList.UnloadMadatory;

            }
            ReportHeader reportHeader = ctxTFAT.ReportHeader.Where(x => x.Code == "UnLoading").FirstOrDefault();
            if (reportHeader==null)
            {
                mModel.SelectColumn = (Unload)Enum.Parse(typeof(Unload), "FMNO");
                mModel.Asc = true;
            }
            else
            {
                if (reportHeader.OrderBy.Contains("Date"))
                {
                    mModel.SelectColumn = (Unload)Enum.Parse(typeof(Unload), "Date");

                }
                else
                {
                    mModel.SelectColumn = (Unload)Enum.Parse(typeof(Unload), "FMNO");

                }
                if (reportHeader.OrderBy.Contains("asc"))
                {

                    mModel.Asc = true;
                }
                else
                {

                    mModel.Asc = false;
                }
            }
            

            return View(mModel);
        }
        public ActionResult SaveData(UnloadSetupVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    //iX9: Remove Existing Data for Delete Mode
                    //if (mModel.Mode == "Delete")
                    //{
                    //    DeleteStateMaster(mModel);
                    //    transaction.Commit();
                    //    transaction.Dispose();
                    //    return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    //}

                    UnLoadSetup mobj = new UnLoadSetup();
                    bool mAdd = true;
                    var id = Convert.ToInt64(1);
                    if (ctxTFAT.UnLoadSetup.Where(x => (x.RECORDKEY == id)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.UnLoadSetup.Where(x => (x.RECORDKEY == id)).FirstOrDefault();
                        mAdd = false;
                    }

                    mobj.UnloadInOthrBra = mModel.UnloadInAnotherBranch;
                    mobj.UnloadMadatory = mModel.UnloadMadatory;

                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    mobj.AUTHORISE = mauthorise;
                    mobj.AUTHIDS = muserid;

                    //ReportHeader reportHeader = ctxTFAT.ReportHeader.Where(x => x.Code == "UnLoading").FirstOrDefault();

                    //if (mModel.SelectColumn == (Unload)Enum.Parse(typeof(Unload), "Date"))
                    //{
                    //    if (mModel.Asc == true)
                    //    {
                    //        reportHeader.OrderBy = "     CAST((select Date from FMMaster where FmNo=LCFM.FmNo) AS datetime) asc      ";
                    //    }
                    //    else
                    //    {
                    //        reportHeader.OrderBy = "       CAST((select Date from FMMaster where FmNo=LCFM.FmNo) AS datetime) desc     ";
                    //    }
                    //}
                    //else
                    //{
                    //    if (mModel.Asc == true)
                    //    {
                    //        reportHeader.OrderBy = "      CAST(LCFM.FmNo AS int) asc       ";
                    //    }
                    //    else
                    //    {
                    //        reportHeader.OrderBy = "      CAST(LCFM.FmNo AS int) desc     ";
                    //    }
                    //}

                    //ctxTFAT.Entry(reportHeader).State = EntityState.Modified;

                    if (mAdd == true)
                    {
                        ctxTFAT.UnLoadSetup.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, "Setup", mModel.Header, "", DateTime.Now, 0, "", "Save Unload Setup", "NA");
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