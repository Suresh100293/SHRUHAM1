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
    public class VehicleTrackingSetupController : BaseController
    {
        private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mauthorise = "A00";
        private static string mdocument = "";

        // GET: Vehicles/VehicleTrackingSetup
        public ActionResult Index(VehicleTrackingVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, "Setup", mModel.Header, "", DateTime.Now, 0, "", "","NA");

            TfatVehicleTrackingSetup mobj = ctxTFAT.TfatVehicleTrackingSetup.FirstOrDefault();
            mModel.Mode = "Edit";

            if (mobj != null)
            {
                mModel.AllTime = mobj.AllTime;
                mModel.UptoDays = mobj.UptoDays;
                mModel.UptoDaysReq = mobj.UptoDaysReq;
                mModel.OnlySchedule = mobj.OnlySchedule;
                mModel.ScheduleAndUptoDays = mobj.ScheduleAndUptoDays;
                mModel.ScheduleAndUptoDaysReq = mobj.ScheduleAndUptoDaysReq;

                mModel.VA_AllTime = mobj.VA_AllTime;
                mModel.VA_UptoDays = mobj.VA_UptoDays;
                mModel.VA_UptoDaysReq = mobj.VA_UptoDaysReq;
                mModel.VA_CompleteReq = mobj.VA_CompleteReq;

                mModel.CT_AllTime = mobj.CT_AllTime;
                mModel.CT_UptoDays = mobj.CT_UptoDays;
                mModel.CT_UptoDaysReq = mobj.CT_UptoDaysReq;
                mModel.CT_DeliveryReq = mobj.CT_DeliveryReq;

                mModel.VM_AllTime = mobj.VM_AllTime;

            }
            else
            {
                mModel.Mode = "Add";
            }

            return View(mModel);
        }

        public ActionResult SaveData(VehicleTrackingVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode

                    TfatVehicleTrackingSetup mobj = new TfatVehicleTrackingSetup();
                    mobj = ctxTFAT.TfatVehicleTrackingSetup.FirstOrDefault();
                    if (mobj != null)
                    {
                        mobj.AllTime = mModel.AllTime;
                        mobj.UptoDays = mModel.UptoDays;
                        mobj.UptoDaysReq = mModel.UptoDaysReq;
                        mobj.OnlySchedule = mModel.OnlySchedule;
                        mobj.ScheduleAndUptoDays = mModel.ScheduleAndUptoDays;
                        mobj.ScheduleAndUptoDaysReq = mModel.ScheduleAndUptoDaysReq;

                        mobj.VA_AllTime = mModel.VA_AllTime;
                        mobj.VA_UptoDays = mModel.VA_UptoDays;
                        mobj.VA_UptoDaysReq = mModel.VA_UptoDaysReq;
                        mobj.VA_CompleteReq = mModel.VA_CompleteReq;

                        mobj.CT_AllTime = mModel.CT_AllTime;
                        mobj.CT_UptoDays = mModel.CT_UptoDays;
                        mobj.CT_UptoDaysReq = mModel.CT_UptoDaysReq;
                        mobj.CT_DeliveryReq = mModel.CT_DeliveryReq;

                        mobj.VM_AllTime = mModel.VM_AllTime;

                        mobj.AUTHIDS = muserid;
                        mobj.AUTHORISE = mauthorise;
                        mobj.ENTEREDBY = muserid;
                        mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    else
                    {
                        mobj = new TfatVehicleTrackingSetup();
                        mobj.AllTime = mModel.AllTime;
                        mobj.UptoDays = mModel.UptoDays;
                        mobj.UptoDaysReq = mModel.UptoDaysReq;
                        mobj.OnlySchedule = mModel.OnlySchedule;
                        mobj.ScheduleAndUptoDays = mModel.ScheduleAndUptoDays;
                        mobj.ScheduleAndUptoDaysReq = mModel.ScheduleAndUptoDaysReq;

                        mobj.AUTHIDS = muserid;
                        mobj.AUTHORISE = mauthorise;
                        mobj.ENTEREDBY = muserid;
                        mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                        ctxTFAT.TfatVehicleTrackingSetup.Add(mobj);
                    }




                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    string mNewCode = "";
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, "Setup", mModel.Header, "", DateTime.Now, 0, "", "Save Vehicle Tracking Setup", "NA");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "CountryMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "CountryMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "CountryMaster" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "CountryMaster" }, JsonRequestBehavior.AllowGet);
        }
    }
}