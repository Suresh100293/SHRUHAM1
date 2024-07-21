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
    public class ChangeVehicleStatusController : BaseController
    {
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;

        #region GetFunction

        public string GetNewCode()
        {
            var NewLcNo = ctxTFAT.TfatVehicleStatus.OrderByDescending(x => x.DocNo).Select(x => x.DocNo).Take(1).FirstOrDefault();
            int LcNo;
            if (String.IsNullOrEmpty(NewLcNo))
            {

                LcNo = 100000;
            }
            else
            {
                LcNo = Convert.ToInt32(NewLcNo) + 1;
            }

            return LcNo.ToString();
        }

        public JsonResult GetVehicle(string term)
        {
            var list = ctxTFAT.VehicleMaster.Where(x => x.Acitve == true).ToList();
            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.TruckNo.ToLower().Contains(term.ToLower())).ToList();
            }
            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.TruckNo.ToUpper()
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public string GetNewCode_VehiHistory()
        {
            string Code = ctxTFAT.tfatVehicleStatusHistory.OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefault();
            if (String.IsNullOrEmpty(Code))
            {

                return "100000";
            }
            else
            {
                return (Convert.ToInt32(Code) + 1).ToString();
            }
        }

        #endregion


        // GET: Vehicles/ChangeVehicleStatus
        public ActionResult Index(ChangeVehicleStatusVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");

            mdocument = mModel.Document;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.TfatVehicleStatus.Where(x => (x.DocNo == mModel.Document)).FirstOrDefault();

                if (mList != null)
                {
                    mModel.VehicleCode = mList.Vehicle;
                    mModel.Vehicle =ctxTFAT.VehicleMaster.Where(x=>x.Code==mList.Vehicle).Select(x=>x.TruckNo.ToUpper()).FirstOrDefault();
                    mModel.vehicleReportingSt = (VehicleStatus)Enum.Parse(typeof(VehicleStatus), mList.Status);
                    mModel.VehicleStatusChangeNarr = mList.Narr;
                    mModel.DocDate = mList.DocDate.ToShortDateString();
                    mModel.EffDate = mList.EffDate.ToShortDateString();
                    mModel.EFFTime = mList.EffTime.ToString();

                    if (!mList.refParentKey.Contains("CV000"))
                    {
                        mModel.BlockChangeStatus = true;
                    }
                }
            }
            else
            {
                mModel.DocDate = DateTime.Now.ToShortDateString();
                mModel.EffDate = DateTime.Now.ToShortDateString();
                mModel.EFFTime = DateTime.Now.ToString("HH:mm");
            }
            return View(mModel);
        }

        public ActionResult SaveData(ChangeVehicleStatusVM mModel)
        {
            //string OldVehicleStatus = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    if (mModel.Mode == "Delete")
                    {
                        DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    TfatVehicleStatus mobj = new TfatVehicleStatus();
                    tfatVehicleStatusHistory vehicleDri_Hist = new tfatVehicleStatusHistory();
                    bool mAdd = true;
                    if (ctxTFAT.TfatVehicleStatus.Where(x => x.DocNo == mModel.Document).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.TfatVehicleStatus.Where(x => x.DocNo == mModel.Document).FirstOrDefault();
                        vehicleDri_Hist = ctxTFAT.tfatVehicleStatusHistory.Where(x => x.refParentKey == mobj.refParentKey).FirstOrDefault();
                        mAdd = false;
                    }

                    if (mAdd == true)
                    {
                        mobj.DocNo = GetNewCode();
                        mobj.AUTHIDS = muserid;
                        mobj.DocDate = ConvertDDMMYYTOYYMMDD(mModel.DocDate);
                        mobj.ENTEREDBY = muserid;
                        mobj.refParentKey = "CV000" + mperiod.Substring(0, 2) + "001" + mobj.DocNo;

                        vehicleDri_Hist.Code = GetNewCode_VehiHistory();
                        vehicleDri_Hist.ENTEREDBY = muserid;
                        vehicleDri_Hist.refParentKey = mobj.refParentKey;
                        vehicleDri_Hist.DocDate = ConvertDDMMYYTOYYMMDD(mModel.DocDate);
                    }
                    #region Change Vehicle Status
                    mobj.Vehicle = mModel.VehicleCode.Trim();
                    mobj.Branch = mbranchcode;
                    mobj.Status = mModel.vehicleReportingSt.ToString().Trim();
                    mobj.Narr = String.IsNullOrEmpty(mModel.VehicleStatusChangeNarr)==true?"": mModel.VehicleStatusChangeNarr;
                    mobj.EffDate = ConvertDDMMYYTOYYMMDD(mModel.EffDate);
                    mobj.EffTime = mModel.EFFTime;
                    //iX9: Save default values to Std fields
                    mobj.AUTHORISE = mauthorise;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                    //VehicleMaster vehicle = ctxTFAT.VehicleMaster.Where(x => x.Code == mModel.VehicleCode).FirstOrDefault();
                    //if (vehicle!=null)
                    //{
                    //    vehicle.Status = mModel.vehicleReportingSt.ToString().Trim();
                    //    ctxTFAT.Entry(vehicle).State = EntityState.Modified;
                    //}
                    #endregion

                    
                    vehicleDri_Hist.Status = mobj.Status;
                    vehicleDri_Hist.FromPeriod = ConvertDDMMYYTOYYMMDD(mModel.EffDate);
                    vehicleDri_Hist.FromTime = mModel.EFFTime;
                    vehicleDri_Hist.TruckNo = mModel.VehicleCode;
                    vehicleDri_Hist.Narr = mModel.VehicleStatusChangeNarr;
                    vehicleDri_Hist.AUTHIDS = muserid;
                    vehicleDri_Hist.AUTHORISE = mauthorise;
                    vehicleDri_Hist.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                    #region Maintain History
                    //var statusHistory = ctxTFAT.tfatVehicleStatusHistory.Where(x => x.TruckNo == mModel.VehicleCode).FirstOrDefault();
                    //if (statusHistory == null)
                    //{
                    //    tfatVehicleStatusHistory history = new tfatVehicleStatusHistory();
                    //    history.Code = GetNewCode_VehiHistory();
                    //    history.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                    //    history.Status = mobj.Status;
                    //    history.FromPeriod = DateTime.Now;
                    //    history.TruckNo = mModel.VehicleCode;
                    //    history.Narr = mModel.VehicleStatusChangeNarr;
                    //    history.AUTHIDS = muserid;
                    //    history.AUTHORISE = mauthorise;
                    //    history.ENTEREDBY = muserid;
                    //    history.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                    //    ctxTFAT.tfatVehicleStatusHistory.Add(history);
                    //}
                    //else
                    //{
                    //    if (mobj.Status.Trim() != OldVehicleStatus.Trim())
                    //    {
                    //        var OldvehicleHistory = ctxTFAT.tfatVehicleStatusHistory.Where(x => x.TruckNo == mModel.VehicleCode.Trim() && x.ToPeriod == null).FirstOrDefault();
                    //        if (OldvehicleHistory != null)
                    //        {
                    //            OldvehicleHistory.ToPeriod = DateTime.Now;
                    //            ctxTFAT.Entry(OldvehicleHistory).State = EntityState.Modified;
                    //        }
                    //        tfatVehicleStatusHistory history = new tfatVehicleStatusHistory();
                    //        history.Code = GetNewCode_VehiHistory();
                    //        history.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                    //        history.Status = mobj.Status;
                    //        history.FromPeriod = DateTime.Now;
                    //        history.TruckNo = mModel.VehicleCode;
                    //        history.Narr = mModel.VehicleStatusChangeNarr;
                    //        history.AUTHIDS = muserid;
                    //        history.AUTHORISE = mauthorise;
                    //        history.ENTEREDBY = muserid;
                    //        history.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                    //        ctxTFAT.tfatVehicleStatusHistory.Add(history);
                    //    }
                    //}
                    #endregion

                    var VehicleHistorycheck = ctxTFAT.tfatVehicleStatusHistory.Where(x => x.refParentKey != mobj.refParentKey && x.TruckNo == mModel.VehicleCode && x.FromPeriod == mobj.EffDate && x.FromTime == mobj.EffTime).FirstOrDefault();
                    if (VehicleHistorycheck != null)
                    {
                        return Json(new { Status = "Error", Message = "This Vehicle EffDate And Efftime Found In History.\n Please Change Eff Date And Time...!" }, JsonRequestBehavior.AllowGet);
                    }

                    if (mAdd == true)
                    {
                        ctxTFAT.TfatVehicleStatus.Add(mobj);
                        ctxTFAT.tfatVehicleStatusHistory.Add(vehicleDri_Hist);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                        ctxTFAT.Entry(vehicleDri_Hist).State = EntityState.Modified;
                    }


                    ctxTFAT.SaveChanges();
                    mnewrecordkey = Convert.ToInt32(mobj.RECORDKEY);
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.DocNo, DateTime.Now, 0, mobj.Vehicle, "Save Change Vehicle Status", "VM");

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

        public ActionResult DeleteStateMaster(ChangeVehicleStatusVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            //long recordkey = Convert.ToInt64(mModel.Document);
            var mList = ctxTFAT.TfatVehicleStatus.Where(x => x.DocNo == mModel.Document).FirstOrDefault();
          
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    ctxTFAT.TfatVehicleStatus.Remove(mList);
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mList.DocNo, DateTime.Now, 0, mList.Vehicle, "Delete Change Vehicle Status", "VM");

                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new
                    {
                        Message = ex1.InnerException.Message,
                        Status = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new
                    {
                        Message = ex.InnerException.Message,
                        Status = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new
            {
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
            mpara = "";
            return GetGridDataColumns(id, "L", "XXXX");
        }

        public ActionResult GetMasterGridData(GridOption Model)
        {
            ExecuteStoredProc("Drop Table ztmp_TfatVehicleHistory");
            string Query = "with CTE_RN as (    select  t.*,        ROW_NUMBER() OVER(ORDER BY fromperiod,FromTime) as RN  from tfatVehicleStatusHistory   as t  where truckno = '" + Model.Code + "') select   DocDate, FromPeriod, FromTime, Status,DATEDIFF(Day, FromPeriod, (select FromPeriod from CTE_RN G where G.RN = C.RN + 1)) as [Days], ENTEREDBY , Narr into ztmp_TfatVehicleHistory from CTE_RN as c";
            ExecuteStoredProc(Query);

            mpara = "para09^" + Model.Code;
            return GetGridReport(Model, "M", mpara, false, 0);
        }

    }
}