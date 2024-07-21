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
    public class ChangeDriverStatusController : BaseController
    {
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;

        #region GetList

        public string GetNewCode()
        {
            var NewLcNo = ctxTFAT.TfatDriverStatus.OrderByDescending(x => x.DocNo).Select(x => x.DocNo).Take(1).FirstOrDefault();
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

        public string GetNewCode_Vehi_Driver()
        {
            string Code = ctxTFAT.VehicleDri_Hist.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).FirstOrDefault();
            if (String.IsNullOrEmpty(Code))
            {

                return "100000";
            }
            else
            {
                return (Convert.ToInt32(Code) + 1).ToString();
            }
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

        public JsonResult GetDriver(string term)
        {
            var list = ctxTFAT.DriverMaster.Where(x => x.Status == true).ToList();
            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }
            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name.ToUpper()
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDriverInfo(string Driver)
        {
            ChangeDriverStatusVM Model = new ChangeDriverStatusVM();
            DriverMaster driver = ctxTFAT.DriverMaster.Where(x => x.Code == Driver).FirstOrDefault();
            Model.VehicleNoCode = ctxTFAT.VehicleDri_Hist.Where(x => x.Driver == driver.Code).OrderByDescending(x => new { x.FromPeriod, x.FromTime }).Select(x => x.TruckNo).FirstOrDefault();
            Model.VehicleNo = ctxTFAT.VehicleMaster.Where(x => x.Code == Model.VehicleNoCode).Select(x => x.TruckNo.ToUpper()).FirstOrDefault();
            Model.MobileNo1 = driver.MobileNo1;
            Model.MobileNo2 = driver.MobileNo2;
            Model.Guaranter = driver.Guaranter;
            Model.LicenceNo = driver.LicenceNo;
            if (driver.LicenceExpDate != null)
            {
                Model.LicenceExpDate = driver.LicenceExpDate.Value.ToShortDateString();
            }
            return Json(new
            {
                Model,
                JsonRequestBehavior.AllowGet
            });
        }

        #endregion

        // GET: Vehicles/ChangeDriverStatus
        public ActionResult Index(ChangeDriverStatusVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.TfatDriverStatus.Where(x => (x.DocNo == mModel.Document)).FirstOrDefault();

                if (mList != null)
                {
                    var Driver = ctxTFAT.DriverMaster.Where(x => x.Code == mList.Driver).FirstOrDefault();
                    mModel.VehicleNoCode = mList.Vehicle;
                    mModel.VehicleNo = ctxTFAT.VehicleMaster.Where(x => x.Code == mList.Vehicle).Select(x => x.TruckNo.ToUpper()).FirstOrDefault();
                    mModel.DriverCode = mList.Driver;
                    mModel.Driver = Driver.Name.ToUpper();
                    mModel.MobileNo1 = Driver.MobileNo1;
                    mModel.MobileNo2 = Driver.MobileNo2;
                    mModel.Guaranter = Driver.Guaranter;
                    mModel.LicenceNo = Driver.LicenceNo;
                    mModel.DriverStatusChangeNarr = mList.Narr;
                    mModel.DocDate = mList.DocDate.ToShortDateString();
                    mModel.EffDate = mList.EffDate.ToShortDateString();
                    mModel.EFFTime = mList.EffTime.ToString();
                    mModel.refParentKey = mList.refParentKey.ToString();
                    if (Driver.LicenceExpDate != null)
                    {
                        mModel.LicenceExpDate = Driver.LicenceExpDate.Value.ToShortDateString();
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

        public ActionResult SaveData(ChangeDriverStatusVM mModel)
        {
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
                    TfatDriverStatus mobj = new TfatDriverStatus();
                    VehicleDri_Hist vehicleDri_Hist = new VehicleDri_Hist();
                    bool mAdd = true;
                    if (ctxTFAT.TfatDriverStatus.Where(x => x.DocNo == mModel.Document).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.TfatDriverStatus.Where(x => x.DocNo == mModel.Document).FirstOrDefault();
                        vehicleDri_Hist = ctxTFAT.VehicleDri_Hist.Where(x => x.refParentKey == mobj.refParentKey).FirstOrDefault();
                        mAdd = false;
                    }



                    string Sno = "";
                    if (mAdd == true)
                    {
                        mobj.DocNo = GetNewCode();
                        mobj.AUTHIDS = muserid;
                        mobj.DocDate = ConvertDDMMYYTOYYMMDD(mModel.DocDate);
                        mobj.ENTEREDBY = muserid;
                        mobj.refParentKey = "CD000" + mperiod.Substring(0, 2) + "001" + mobj.DocNo;

                        vehicleDri_Hist.Code = GetNewCode_Vehi_Driver();
                        Sno = vehicleDri_Hist.Code;
                        vehicleDri_Hist.ENTEREDBY = muserid;
                        vehicleDri_Hist.refParentKey = mobj.refParentKey;
                    }

                    #region Change Vehicle Status
                    mobj.Vehicle = mModel.VehicleNoCode.Trim();
                    mobj.Branch = mbranchcode;
                    mobj.EffDate = ConvertDDMMYYTOYYMMDD(mModel.EffDate);
                    mobj.EffTime = mModel.EFFTime;
                    mobj.Driver = mModel.DriverCode.ToString().Trim();
                    mobj.Narr = String.IsNullOrEmpty(mModel.DriverStatusChangeNarr) == true ? "" : mModel.DriverStatusChangeNarr.ToString();
                    //iX9: Save default values to Std fields
                    mobj.AUTHORISE = mauthorise;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                    vehicleDri_Hist.TruckNo = mobj.Vehicle.ToUpper().Trim();
                    vehicleDri_Hist.Driver = mobj.Driver;
                    vehicleDri_Hist.FromPeriod = ConvertDDMMYYTOYYMMDD(mModel.EffDate);
                    vehicleDri_Hist.FromTime = mModel.EFFTime;
                    vehicleDri_Hist.Narr = mModel.DriverStatusChangeNarr;
                    vehicleDri_Hist.AUTHIDS = muserid;
                    vehicleDri_Hist.AUTHORISE = mauthorise;
                    vehicleDri_Hist.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                    //DriverMaster driver = ctxTFAT.DriverMaster.Where(x => x.Code == mModel.DriverCode).FirstOrDefault();
                    //string OldVehicleNo = "";
                    //if (driver != null)
                    //{
                    //    OldVehicleNo = driver.VehicleNo == null ? "" : driver.VehicleNo;
                    //    driver.VehicleNo = mModel.VehicleNoCode.ToString().Trim();
                    //    ctxTFAT.Entry(driver).State = EntityState.Modified;
                    //}

                    #region Maintain History

                    //if (mobj.Driver != "99999")
                    //{
                    //    if (mobj.Vehicle.Trim() != OldVehicleNo.Trim())
                    //    {


                    //        #region OLD VEHICLE

                    //        //if (OldVehicleNo.Trim() != "99998" && OldVehicleNo.Trim() != "99999")
                    //        //{
                    //        //    VehicleMaster OldvehicleMaster = ctxTFAT.VehicleMaster.Where(x => x.Code == OldVehicleNo.Trim()).FirstOrDefault();
                    //        //    if (OldvehicleMaster != null)
                    //        //    {
                    //        //        OldvehicleMaster.Driver = "99999";
                    //        //        ctxTFAT.Entry(OldvehicleMaster).State = EntityState.Modified;
                    //        //    }
                    //        //}

                    //        //VehicleDri_Hist OldvehicleHistory = ctxTFAT.VehicleDri_Hist.Where(x => x.TruckNo == OldVehicleNo.Trim() && x.Driver == mobj.Driver.Trim() && x.ToPeriod == null).FirstOrDefault();
                    //        //if (OldvehicleHistory != null)
                    //        //{
                    //        //    OldvehicleHistory.ToPeriod = DateTime.Now;
                    //        //    ctxTFAT.Entry(OldvehicleHistory).State = EntityState.Modified;
                    //        //}
                    //        #endregion

                    //        #region CURRENT VEHICLE

                    //        if (mobj.Vehicle == "99999" || mobj.Vehicle == "99998")
                    //        {

                    //        }
                    //        else
                    //        {
                    //            var CurrnetVehicle = ctxTFAT.VehicleMaster.Where(x => x.Code == mobj.Vehicle).FirstOrDefault();
                    //            if (CurrnetVehicle != null)
                    //            {
                    //                OldvehicleHistory = ctxTFAT.VehicleDri_Hist.Where(x => x.TruckNo == mobj.Vehicle.Trim() && x.Driver == CurrnetVehicle.Driver.Trim() && x.ToPeriod == null).FirstOrDefault();
                    //                if (OldvehicleHistory != null)
                    //                {
                    //                    OldvehicleHistory.ToPeriod = DateTime.Now;
                    //                    ctxTFAT.Entry(OldvehicleHistory).State = EntityState.Modified;
                    //                }
                    //                var OldDriver = ctxTFAT.DriverMaster.Where(x => x.Code == CurrnetVehicle.Driver && x.Code != "99999").FirstOrDefault();
                    //                if (OldDriver != null)
                    //                {
                    //                    VehicleDri_Hist vehicleDriver1 = new VehicleDri_Hist();
                    //                    vehicleDriver1.Code = HistoryCode;
                    //                    vehicleDriver1.TruckNo = "99999";
                    //                    vehicleDriver1.Driver = OldDriver.Code;
                    //                    vehicleDriver1.FromPeriod = DateTime.Now;
                    //                    vehicleDriver1.Narr = mModel.DriverStatusChangeNarr;
                    //                    vehicleDriver1.ToPeriod = null;
                    //                    vehicleDriver1.AUTHIDS = muserid;
                    //                    vehicleDriver1.AUTHORISE = mauthorise;
                    //                    vehicleDriver1.ENTEREDBY = muserid;
                    //                    vehicleDriver1.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                    //                    ctxTFAT.VehicleDri_Hist.Add(vehicleDriver1);
                    //                    HistoryCode = (Convert.ToInt32(HistoryCode) + 1).ToString();

                    //                    OldDriver.VehicleNo = "99999";
                    //                    ctxTFAT.Entry(OldDriver).State = EntityState.Modified;
                    //                }

                    //                CurrnetVehicle.Driver = mobj.Driver;
                    //                ctxTFAT.Entry(CurrnetVehicle).State = EntityState.Modified;

                    //            }
                    //            VehicleDri_Hist vehicleDriver = new VehicleDri_Hist();
                    //            vehicleDriver.Code = HistoryCode;
                    //            vehicleDriver.TruckNo = mobj.Vehicle.ToUpper().Trim();
                    //            vehicleDriver.Driver = mobj.Driver;
                    //            vehicleDriver.FromPeriod = DateTime.Now;
                    //            vehicleDriver.Narr = mModel.DriverStatusChangeNarr;
                    //            vehicleDriver.ToPeriod = null;
                    //            vehicleDriver.AUTHIDS = muserid;
                    //            vehicleDriver.AUTHORISE = mauthorise;
                    //            vehicleDriver.ENTEREDBY = muserid;
                    //            vehicleDriver.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                    //            ctxTFAT.VehicleDri_Hist.Add(vehicleDriver);
                    //            HistoryCode = (Convert.ToInt32(HistoryCode) + 1).ToString();
                    //        }
                    //        #endregion
                    //    }
                    //}


                    #endregion

                    #endregion

                    var DriverHistorycheck = ctxTFAT.VehicleDri_Hist.Where(x => x.refParentKey != mobj.refParentKey && x.Driver == mModel.DriverCode && x.FromPeriod == mobj.EffDate && x.FromTime == mobj.EffTime).FirstOrDefault();
                    if (DriverHistorycheck != null)
                    {
                        return Json(new { Status = "Error", Message = "This Driver EffDate And Efftime Found In History.\n Please Change Eff Date And Time...!" }, JsonRequestBehavior.AllowGet);
                    }
                    if (mAdd)
                    {
                        var OldVehicle = ctxTFAT.VehicleDri_Hist.OrderByDescending(x => new { x.FromPeriod, x.FromTime }).Where(x => x.Driver == mModel.DriverCode).Select(x => x.TruckNo).FirstOrDefault();
                        if (!String.IsNullOrEmpty(OldVehicle))
                        {
                            if (OldVehicle != "99999" && OldVehicle != "99998")
                            {
                                VehicleDri_Hist setVehicleEmpty = new VehicleDri_Hist();
                                setVehicleEmpty.Code = (Convert.ToInt32(Sno) + 1).ToString();
                                Sno = setVehicleEmpty.Code;
                                setVehicleEmpty.ENTEREDBY = muserid;
                                setVehicleEmpty.refParentKey = mobj.refParentKey;
                                setVehicleEmpty.TruckNo = OldVehicle.ToUpper().Trim();
                                setVehicleEmpty.Driver = "99999";
                                setVehicleEmpty.FromPeriod = ConvertDDMMYYTOYYMMDD(mModel.EffDate);
                                setVehicleEmpty.FromTime = mModel.EFFTime;
                                setVehicleEmpty.Narr = mModel.DriverStatusChangeNarr;
                                setVehicleEmpty.AUTHIDS = muserid;
                                setVehicleEmpty.AUTHORISE = mauthorise;
                                setVehicleEmpty.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                                ctxTFAT.VehicleDri_Hist.Add(setVehicleEmpty);
                            }
                        }

                        if (mModel.VehicleNoCode!= "99999")
                        {
                            var CurrVehicleOldDriver = ctxTFAT.VehicleDri_Hist.OrderByDescending(x => new { x.FromPeriod, x.FromTime }).Where(x => x.TruckNo == mModel.VehicleNoCode).Select(x => x.Driver).FirstOrDefault();
                            if (!String.IsNullOrEmpty(CurrVehicleOldDriver))
                            {
                                if (CurrVehicleOldDriver != "99999")
                                {
                                    VehicleDri_Hist setVehicleEmpty = new VehicleDri_Hist();
                                    setVehicleEmpty.Code = (Convert.ToInt32(Sno) + 1).ToString();
                                    Sno = setVehicleEmpty.Code;
                                    setVehicleEmpty.ENTEREDBY = muserid;
                                    setVehicleEmpty.refParentKey = mobj.refParentKey;
                                    setVehicleEmpty.TruckNo = "99999";
                                    setVehicleEmpty.Driver = CurrVehicleOldDriver;
                                    setVehicleEmpty.FromPeriod = ConvertDDMMYYTOYYMMDD(mModel.EffDate);
                                    setVehicleEmpty.FromTime = mModel.EFFTime;
                                    setVehicleEmpty.Narr = mModel.DriverStatusChangeNarr;
                                    setVehicleEmpty.AUTHIDS = muserid;
                                    setVehicleEmpty.AUTHORISE = mauthorise;
                                    setVehicleEmpty.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                                    ctxTFAT.VehicleDri_Hist.Add(setVehicleEmpty);
                                }
                            }
                        }
                        
                    }


                    if (mAdd == true)
                    {
                        ctxTFAT.TfatDriverStatus.Add(mobj);
                        ctxTFAT.VehicleDri_Hist.Add(vehicleDri_Hist);
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
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.DocNo, DateTime.Now, 0, mobj.Driver, "Save Change Driver Status", "DM");

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

        public ActionResult DeleteStateMaster(ChangeDriverStatusVM mModel)
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
            var mList = ctxTFAT.TfatDriverStatus.Where(x => x.DocNo == mModel.Document).FirstOrDefault();

            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    ctxTFAT.TfatDriverStatus.Remove(mList);
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mList.DocNo, DateTime.Now, 0, mList.Driver, "Delete Change Driver Status", "DM");

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
            ExecuteStoredProc("Drop Table ztmp_TfatDriverHistory");
            string Query = "with CTE_RN as ( select t.*, ROW_NUMBER() OVER(ORDER BY fromperiod, FromTime) as RN from VehicleDri_Hist as t where Driver = '" + Model.Code + "') select   LASTUPDATEDATE, FromPeriod, FromTime, TruckNo,DATEDIFF(Day, FromPeriod, (select FromPeriod from CTE_RN G where G.RN = C.RN + 1)) as [Days],ENTEREDBY,Narr into ztmp_TfatDriverHistory from CTE_RN as c";
            ExecuteStoredProc(Query);
            mpara = "para09^" + Model.Code;
            return GetGridReport(Model, "M", mpara, false, 0);
        }

    }
}