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
    public class LocalDeliverySheetSetupController : BaseController
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

        // GET: Logistics/LocalDeliverySheetSetup
        public ActionResult Index(LocalDeliverySheetSetupVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, "LDSPage SetUp", "", DateTime.Now, 0, "", "", "A");
            mdocument = mModel.Document;


            var id = Convert.ToInt64(1);
            var mList = ctxTFAT.LDSsetup.Where(x => (x.RECORDKEY == id)).FirstOrDefault();
            if (mList != null)
            {
                if (mList.LDSBoth == true)
                {
                    mModel.Both = true;
                    //mModel.LrAutomatic = false;
                    //mModel.LrManual = false;
                }
                else if (mList.LDSGenerate == true)
                {
                    //mModel.Both = false;
                    mModel.LDSAutomatic = true;
                    //mModel.LrManual = false;
                }
                else
                {
                    //mModel.LrAutomatic = false;
                    //mModel.Both = false;
                    mModel.LDSManual = true;
                }

                mModel.LDS_Date = mList.LDSDate;
                mModel.Before_LDS_Date = mList.BeforeLDSDate;
                mModel.After_LDS_Date = mList.AfterLDSDate;

               
                
                
                mModel.FreightCalculate = mList.AutoFreight;
            }

            ReportHeader reportHeader = ctxTFAT.ReportHeader.Where(x => x.Code == "LocalDeliverySheet").FirstOrDefault();
            if (reportHeader.OrderBy.Contains("Date"))
            {
                mModel.SelectColumn = (AscDescLDS)Enum.Parse(typeof(AscDescLDS), "DocumentDate");

            }
            else
            {
                mModel.SelectColumn = (AscDescLDS)Enum.Parse(typeof(AscDescLDS), "LDSNo");

            }
            if (reportHeader.OrderBy.Contains("asc"))
            {

                mModel.Asc = true;
            }
            else
            {

                mModel.Asc = false;
            }

            return View(mModel);
        }

        public ActionResult SaveData(LocalDeliverySheetSetupVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    //if (mModel.Mode == "Delete")
                    //{
                    //    //DeleteStateMaster(mModel);
                    //    transaction.Commit();
                    //    transaction.Dispose();
                    //    return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    //}

                    LDSsetup mobj = new LDSsetup();
                    bool mAdd = true;
                    var id = Convert.ToInt64(1);
                    if (ctxTFAT.LDSsetup.Where(x => (x.RECORDKEY == id)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.LDSsetup.Where(x => (x.RECORDKEY == id)).FirstOrDefault();
                        mAdd = false;
                    }

                    if (mModel.Both == true)
                    {
                        mobj.LDSBoth = true;
                        mobj.LDSGenerate = false;
                    }
                    else if (mModel.LDSAutomatic == true)
                    {
                        mobj.LDSBoth = false;
                        mobj.LDSGenerate = true;
                    }
                    else
                    {
                        mobj.LDSBoth = false;
                        mobj.LDSGenerate = false;
                    }

                    mobj.LDSDate = mModel.LDS_Date;
                    mobj.BeforeLDSDate = mModel.Before_LDS_Date;
                    mobj.AfterLDSDate = mModel.After_LDS_Date;

                    


                    mobj.AutoFreight = mModel.FreightCalculate;

                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    mobj.AUTHORISE = mauthorise;
                    mobj.AUTHIDS = muserid;

                    ReportHeader reportHeader = ctxTFAT.ReportHeader.Where(x => x.Code == "LocalDeliverySheet").FirstOrDefault();

                    if (mModel.SelectColumn == (AscDescLDS)Enum.Parse(typeof(AscDescLDS), "DocumentDate"))
                    {
                        if (mModel.Asc == true)
                        {
                            reportHeader.OrderBy = "     CAST(Date AS datetime) asc     ";
                        }
                        else
                        {
                            reportHeader.OrderBy = "     CAST(Date AS datetime) desc     ";
                        }
                    }
                    else
                    {
                        if (mModel.Asc == true)
                        {
                            reportHeader.OrderBy = "     CAST(LDSNo AS int) asc     ";
                        }
                        else
                        {
                            reportHeader.OrderBy = "     CAST(LDSNo AS int) desc     ";
                        }
                    }

                    ctxTFAT.Entry(reportHeader).State = EntityState.Modified;

                    if (mAdd == true)
                    {
                        ctxTFAT.LDSsetup.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, "LDSPage SetUp ", "", DateTime.Now, 0, "", "", "A");
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

        public ActionResult DeleteStateMaster(LocalDeliverySheetSetupVM mModel)
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
            var mList = ctxTFAT.LDSsetup.Where(x => (x.RECORDKEY == id)).FirstOrDefault();
            ctxTFAT.LDSsetup.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
    }
}