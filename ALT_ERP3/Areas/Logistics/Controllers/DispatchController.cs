using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;

using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class DispatchController : BaseController
    {

        private static string mdocument = "";
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        private static string mauthorise = "A00";
        string BranchCode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private static string connstring;

        #region Use Function
        public ActionResult SaveData(LoadingDispachVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    bool ErrorDate = false, ErrorKM = false;
                    string Msg = "", Parentkey = "";;
                    FMROUTETable fM_ROUTE = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
                    var VehicleMainKM = ctxTFAT.VehicleKmMaintainMa.Where(x => x.FMRefTablekey == fM_ROUTE.Parentkey && x.Parent == fM_ROUTE.Parent && x.RouteVia == fM_ROUTE.RouteVia && x.ComingFrom == "Dispatch").FirstOrDefault();
                    if (fM_ROUTE != null)
                    {
                        Parentkey = ctxTFAT.FMMaster.Where(x => x.TableKey == fM_ROUTE.Parentkey).Select(x => x.ParentKey).FirstOrDefault();

                        FMROUTETable PreviousVehiclesData = new FMROUTETable();
                        FMROUTETable NextVehiclesData = new FMROUTETable();
                        DateTime DispatchDate = new DateTime();
                        DateTime? PreviousDate = new DateTime(), NextDate = new DateTime();
                        string[] PreviousTime = new string[2], NextTime = new string[2];
                        int StartVehicleKm = 0, EndVehicleKm = 0;
                        if (ctxTFAT.LogisticsFlow.Select(x => x.ADCriteriaCheck).FirstOrDefault())
                        {
                            if (!String.IsNullOrEmpty(mModel.DispatchDate))
                            {
                                var Date = mModel.DispatchDate.Split('/');
                                var Date1 = mModel.DispatchTime.Split(':');
                                DispatchDate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Date1[0]), Convert.ToInt32(Date1[1]), 00);
                            }

                            if (String.IsNullOrEmpty(fM_ROUTE.ArrivalDate.ToString()))
                            {
                                PreviousVehiclesData = ctxTFAT.FMROUTETable.Where(x => x.VehicleNo == fM_ROUTE.VehicleNo && x.Parentkey == fM_ROUTE.Parentkey && x.RECORDKEY < fM_ROUTE.RECORDKEY && (x.ArrivalDate != null || x.DispatchDate != null)).OrderByDescending(x => x.RECORDKEY).FirstOrDefault();
                                if (PreviousVehiclesData != null)
                                {
                                    PreviousDate = PreviousVehiclesData.DispatchDate != null ? ConvertDDMMYYTOYYMMDD(PreviousVehiclesData.DispatchDate.Value.ToString()) : ConvertDDMMYYTOYYMMDD(PreviousVehiclesData.ArrivalDate.Value.ToString());
                                    PreviousTime = PreviousVehiclesData.DispatchTime != null ? PreviousVehiclesData.DispatchTime.Split(':') : PreviousVehiclesData.ArrivalTime.Split(':');

                                    var Date = PreviousDate.Value.ToShortDateString().Split('/');
                                    PreviousDate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(PreviousTime[0]), Convert.ToInt32(PreviousTime[1]), 00);
                                }
                            }
                            else
                            {
                                PreviousDate = ConvertDDMMYYTOYYMMDD(fM_ROUTE.ArrivalDate.ToString());
                                PreviousTime = fM_ROUTE.ArrivalTime.Split(':');
                                StartVehicleKm = Convert.ToInt32(fM_ROUTE.ArrivalKM);

                                var Date = PreviousDate.Value.ToShortDateString().Split('/');
                                PreviousDate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(PreviousTime[0]), Convert.ToInt32(PreviousTime[1]), 00);
                            }

                            NextVehiclesData = ctxTFAT.FMROUTETable.Where(x => x.VehicleNo == fM_ROUTE.VehicleNo && x.Parentkey == fM_ROUTE.Parentkey && x.RECORDKEY > fM_ROUTE.RECORDKEY && (x.ArrivalDate != null || x.DispatchDate != null)).OrderByDescending(x => x.RECORDKEY).FirstOrDefault();
                            if (NextVehiclesData != null)
                            {
                                NextDate = NextVehiclesData.DispatchDate != null ? ConvertDDMMYYTOYYMMDD(NextVehiclesData.DispatchDate.Value.ToString()) : ConvertDDMMYYTOYYMMDD(NextVehiclesData.ArrivalDate.Value.ToString());
                                NextTime = NextVehiclesData.DispatchTime != null ? NextVehiclesData.DispatchTime.Split(':') : NextVehiclesData.ArrivalTime.Split(':');

                                var Date = NextDate.Value.ToShortDateString().Split('/');
                                NextDate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(NextTime[0]), Convert.ToInt32(NextTime[1]), 00);
                            }

                            DateTime DisDate = ConvertDDMMYYTOYYMMDD(mModel.DispatchDate);
                            if (StartVehicleKm == 0)
                            {
                                StartVehicleKm = ctxTFAT.VehicleKmMaintainMa.Where(x => x.Date <= DisDate && x.VehicleNo == fM_ROUTE.VehicleNo && x.Parent != fM_ROUTE.Parent && x.RouteVia != fM_ROUTE.RouteVia).OrderByDescending(x => x.RECORDKEY).Select(x => x.KM).FirstOrDefault();
                            }
                            EndVehicleKm = ctxTFAT.VehicleKmMaintainMa.Where(x => x.Date > DisDate && x.VehicleNo == fM_ROUTE.VehicleNo && x.Parent != fM_ROUTE.Parent && x.RouteVia != fM_ROUTE.RouteVia).OrderBy(x => x.RECORDKEY).Select(x => x.KM).FirstOrDefault();

                            #region Check Date
                            if (PreviousVehiclesData != null && NextVehiclesData != null)
                            {
                                if (!(PreviousDate <= DispatchDate && DispatchDate <= NextDate))
                                {
                                    ErrorDate = true;
                                    Msg += "Date And Time Should Between " + PreviousDate + "-" + NextDate + " This Range Only.....\n ";
                                }
                            }
                            else if ((PreviousVehiclesData == null && NextVehiclesData != null))
                            {
                                if (!(DispatchDate <= NextDate))
                                {
                                    ErrorDate = true;
                                    Msg += "Date And Time Should Be Less Than " + NextDate + " This Date.....\n ";
                                }
                            }
                            else if (PreviousVehiclesData != null && NextVehiclesData == null)
                            {
                                if (!(PreviousDate <= DispatchDate))
                                {
                                    ErrorDate = true;
                                    Msg += "Date And Time Should Be Grater Than " + PreviousDate + " This Date.....\n ";
                                }
                            }
                            #endregion

                            #region Check KM
                            if (StartVehicleKm != 0 && EndVehicleKm != 0)
                            {
                                if (!(StartVehicleKm <= Convert.ToInt32(mModel.DispachKM) && Convert.ToInt32(mModel.DispachKM) <= EndVehicleKm))
                                {
                                    ErrorKM = true;
                                    Msg += "Km Should Be Between " + StartVehicleKm + "-" + EndVehicleKm + " This Range Only.......\n";
                                }
                            }
                            else if (StartVehicleKm == 0 && EndVehicleKm != 0)
                            {
                                if (!(StartVehicleKm <= Convert.ToInt32(mModel.DispachKM) && Convert.ToInt32(mModel.DispachKM) <= EndVehicleKm))
                                {
                                    ErrorKM = true;
                                    Msg += "Km Should Be Between " + StartVehicleKm + "-" + EndVehicleKm + " This Range Only.......\n";
                                }
                            }
                            else if (StartVehicleKm != 0 && EndVehicleKm == 0)
                            {
                                if (!(StartVehicleKm <= Convert.ToInt32(mModel.DispachKM)))
                                {
                                    ErrorKM = true;
                                    Msg += "Km Should Be Grater Than " + StartVehicleKm + " This KM.......\n";
                                }
                            }
                            #endregion
                        }

                        if (ErrorDate == false && ErrorKM == false)
                        {
                            bool Schedule = false;

                            var ListOfScheduleCategory = ctxTFAT.LogisticsFlow.Select(x => x.ScheduleCategReq).FirstOrDefault();
                            if (!String.IsNullOrEmpty(ListOfScheduleCategory))
                            {
                                var List = ListOfScheduleCategory.Split(',').ToList();
                                var FmCategory = ctxTFAT.FMMaster.Where(x => x.TableKey == fM_ROUTE.Parentkey).Select(x => x.VehicleStatus).FirstOrDefault();
                                Schedule = List.Contains(FmCategory);
                            }

                            fM_ROUTE.DispatchDate = ConvertDDMMYYTOYYMMDD(mModel.DispatchDate.Trim());
                            fM_ROUTE.DispatchTime = mModel.DispatchTime;
                            fM_ROUTE.DispatchKM = Convert.ToInt32(mModel.DispachKM);
                            fM_ROUTE.DispatchRemark = mModel.DispachRemark;
                            ctxTFAT.Entry(fM_ROUTE).State = EntityState.Modified;
                            VehicleDispatchNotification(fM_ROUTE);
                            if (Schedule)
                            {
                                SetReScheduleBasedOnDispatch(fM_ROUTE);
                            }

                            if (VehicleMainKM == null)
                            {
                                VehicleMainKM = new VehicleKmMaintainMa();
                                VehicleMainKM.ComingFrom = "Dispatch";
                                VehicleMainKM.Date = fM_ROUTE.DispatchDate.Value;
                                VehicleMainKM.EntryDate = DateTime.Now;
                                VehicleMainKM.KM = Convert.ToInt32(fM_ROUTE.DispatchKM);
                                VehicleMainKM.Time = fM_ROUTE.DispatchTime;
                                VehicleMainKM.VehicleNo = fM_ROUTE.VehicleNo;
                                VehicleMainKM.FMno = fM_ROUTE.FmNo;
                                VehicleMainKM.Parent = fM_ROUTE.Parent;
                                VehicleMainKM.RouteVia = fM_ROUTE.RouteVia;
                                VehicleMainKM.ENTEREDBY = muserid;
                                VehicleMainKM.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                                VehicleMainKM.AUTHORISE = mauthorise;
                                VehicleMainKM.AUTHIDS = muserid;
                                VehicleMainKM.FMRefTablekey = fM_ROUTE.Parentkey;

                                ctxTFAT.VehicleKmMaintainMa.Add(VehicleMainKM);
                            }
                            else
                            {
                                VehicleMainKM.Date = fM_ROUTE.DispatchDate.Value;
                                VehicleMainKM.Time = fM_ROUTE.DispatchTime;
                                VehicleMainKM.KM = Convert.ToInt32(fM_ROUTE.DispatchKM);
                                ctxTFAT.Entry(VehicleMainKM).State = EntityState.Modified;
                            }
                        }
                        else
                        {
                            return Json(new { Message = Msg, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { Message = "Error,Route Not Fount..\n", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                    }

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, "Veh-ACT", "Dispatch Vehicle", Parentkey, DateTime.Now, 0, "", "Save", "NA");

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

        public ActionResult SetReScheduleBasedOnDispatch(FMROUTETable fM_ROUTE)
        {

            DateTime NewDate = new DateTime();
            var Date = fM_ROUTE.DispatchDate.Value.ToShortDateString().Split('/');
            var Date1 = fM_ROUTE.DispatchTime.Split(':');
            NewDate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Date1[0]), Convert.ToInt32(Date1[1]), 00);

            List<FMROUTETable> fM_ROUTE_s = ctxTFAT.FMROUTETable.Where(x => x.Parentkey == fM_ROUTE.Parentkey && x.RECORDKEY > fM_ROUTE.RECORDKEY).OrderBy(x => x.SubRoute).ToList();
            foreach (var item in fM_ROUTE_s)
            {
                string[] Hours_And_Time = new string[2];
                if (item.KilometersTime == null)
                {
                    Hours_And_Time[0] = "0";
                    Hours_And_Time[1] = "0";
                }
                else
                {
                    Hours_And_Time = item.KilometersTime.Split('.');
                }
                var minute = 0;
                if (Hours_And_Time.Count() == 1)
                {
                    minute = 0;
                }
                else
                {
                    minute = Convert.ToInt32(Hours_And_Time[1]);
                }
                NewDate = NewDate.AddHours(Convert.ToInt32(Hours_And_Time[0])).AddMinutes(Convert.ToInt32(minute));
                item.ArrivalReSchDate = ConvertDDMMYYTOYYMMDD(NewDate.ToShortDateString());
                item.ArrivalReSchTime = NewDate.ToString("HH:mm");
                item.ArrivalReSchKm = (Convert.ToInt32(fM_ROUTE.DispatchKM) + Convert.ToInt32(item.Kilometers == null ? 0 : item.Kilometers));

                int Hours = 0, minutes = 0;
                if (!String.IsNullOrEmpty(item.VehicleActivity))
                {
                    var GetTime = item.VehicleActivity.Split(':');
                    Hours = Convert.ToInt32(GetTime[0]);
                    if (GetTime.Length == 2)
                    {
                        minutes = Convert.ToInt32(GetTime[1]);
                    }
                }
                NewDate = NewDate.AddHours(Hours).AddMinutes(minutes);
                item.DispatchReSchDate = ConvertDDMMYYTOYYMMDD(NewDate.ToShortDateString());
                item.DispatchReSchTime = NewDate.ToString("HH:mm");
                ctxTFAT.Entry(item).State = EntityState.Modified;

            }
            ctxTFAT.SaveChanges();


            return Json(new { Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteData(LoadingDispachVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    string Parentkey = "";
                    FMROUTETable fM_ROUTE = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
                    Parentkey = ctxTFAT.FMMaster.Where(x => x.TableKey == fM_ROUTE.Parentkey).Select(x => x.ParentKey).FirstOrDefault();
                    if (ctxTFAT.LogisticsFlow.Select(x => x.ADCriteriaCheck).FirstOrDefault())
                    {
                        var NextVehiclesData = ctxTFAT.FMROUTETable.Where(x => x.Parentkey==fM_ROUTE.Parentkey && x.VehicleNo == fM_ROUTE.VehicleNo && x.RECORDKEY > fM_ROUTE.RECORDKEY && (x.ArrivalDate != null || x.DispatchDate != null)).OrderByDescending(x => x.RECORDKEY).FirstOrDefault();
                        if (NextVehiclesData == null)
                        {
                            fM_ROUTE.DispatchDate = null;
                            fM_ROUTE.DispatchTime = null;
                            fM_ROUTE.DispatchKM = null;
                            fM_ROUTE.DispatchRemark = null;
                            ctxTFAT.Entry(fM_ROUTE).State = EntityState.Modified;

                            var VehicleMainKM = ctxTFAT.VehicleKmMaintainMa.Where(x => x.FMRefTablekey == fM_ROUTE.Parentkey && x.Parent == fM_ROUTE.Parent && x.RouteVia == fM_ROUTE.RouteVia && x.ComingFrom == "Dispatch").FirstOrDefault();
                            ctxTFAT.VehicleKmMaintainMa.Remove(VehicleMainKM);
                        }
                        else
                        {
                            return Json(new { Message = "Not Allow To Delete Because Onward Arrival Or Dispatch Found..\n", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        fM_ROUTE.DispatchDate = null;
                        fM_ROUTE.DispatchTime = null;
                        fM_ROUTE.DispatchKM = null;
                        fM_ROUTE.DispatchRemark = null;
                        ctxTFAT.Entry(fM_ROUTE).State = EntityState.Modified;

                        var VehicleMainKM = ctxTFAT.VehicleKmMaintainMa.Where(x => x.FMRefTablekey == fM_ROUTE.Parentkey && x.Parent == fM_ROUTE.Parent && x.RouteVia == fM_ROUTE.RouteVia && x.ComingFrom == "Dispatch").FirstOrDefault();
                        ctxTFAT.VehicleKmMaintainMa.Remove(VehicleMainKM);
                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, "Veh-ACT", "Dispatch Vehicle", Parentkey, DateTime.Now, 0, "", "Delete", "NA");

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

        #endregion




        public ActionResult Reverse(LoadingDispachVM mModel)
        {
            var ChildList = GetChildGrp(mbranchcode);
            FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.FmNo.ToString() == mModel.Document.ToString()).FirstOrDefault();
            FMROUTETable FMROUTETable = ctxTFAT.FMROUTETable.Where(x => ChildList.Contains(x.RouteVia) && x.FmNo == fMMaster.FmNo).FirstOrDefault();
            if (mModel.UpdateFmStatus && FMROUTETable != null)
            {
                if (!String.IsNullOrEmpty(FMROUTETable.DispatchDate.ToString()))
                {
                    return Json(new { Message = "Not Allow To Reverse", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }

                FMROUTETable.DispatchDate = null;
                FMROUTETable.DispatchTime = null;
                FMROUTETable.DispatchKM = null;
                FMROUTETable.DispatchRemark = null;
                fMMaster.FmStatus = "U";
                ctxTFAT.Entry(fMMaster).State = EntityState.Modified;
                ctxTFAT.Entry(FMROUTETable).State = EntityState.Modified;
                ctxTFAT.SaveChanges();
                return Json(new { Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(new { Message = "Not Found", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetInCurrentBranch()
        {
            string BranchCode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
            var Areas = GetChildGrp(mbranchcode);
            List<ArrivalDispatchVM> List = new List<ArrivalDispatchVM>();
            var GetArrivalList = ctxTFAT.FMROUTETable.Where(x => Areas.Contains(x.RouteVia) && (x.DispatchDate != null)).ToList();
            foreach (var item in GetArrivalList)
            {
                FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.FmNo == item.FmNo).FirstOrDefault();
                ArrivalDispatchVM arrivalDispatchVM = new ArrivalDispatchVM();
                arrivalDispatchVM.Fmno = item.FmNo.ToString();
                arrivalDispatchVM.VehicleNo = fMMaster.TruckNo.ToString();
                arrivalDispatchVM.FMDate = fMMaster.Date.ToShortDateString();

                #region Route Details
                var RouteDetails = fMMaster.RouteVia.Split(',');
                int last = RouteDetails.Length - 1;
                string Route = "";
                for (int i = 0; i < RouteDetails.Length; i++)
                {
                    var index = RouteDetails[i].ToString();
                    if (i == 0)
                    {
                        arrivalDispatchVM.From = ctxTFAT.TfatBranch.Where(x => x.Code == index).Select(x => x.Name).FirstOrDefault();
                    }
                    else if (last == i)
                    {
                        arrivalDispatchVM.To = ctxTFAT.TfatBranch.Where(x => x.Code == index).Select(x => x.Name).FirstOrDefault();
                    }
                    else
                    {
                        Route += ctxTFAT.TfatBranch.Where(x => x.Code == index).Select(x => x.Name).FirstOrDefault() + ",";
                    }
                }
                if (!String.IsNullOrEmpty(Route))
                {
                    arrivalDispatchVM.Route = Route.Substring(0, Route.Length - 1);
                }
                #endregion

                #region Get NoofLR,Weight,Qty
                if (!String.IsNullOrEmpty(fMMaster.LCno))
                {
                    var Lclist = fMMaster.LCno.Split(',').ToList();
                    List<LCDetail> lCDetails = ctxTFAT.LCDetail.Where(x => Lclist.Contains(x.LCno.ToString())).ToList();
                    arrivalDispatchVM.NoofLr = lCDetails.Count.ToString();
                    arrivalDispatchVM.TotalWeight = lCDetails.Sum(x => x.LRActWeight).ToString();
                    arrivalDispatchVM.TotalQty = lCDetails.Sum(x => x.Qty).ToString();
                }
                #endregion

                arrivalDispatchVM.Date = item.DispatchDate.Value.ToShortDateString();
                //DateTime dt = DateTime.ParseExact(item.DispatchTime, "HH:mm", null, DateTimeStyles.None);
                //arrivalDispatchVM.Time = dt.ToString("HH:mm tt");
                arrivalDispatchVM.Time = item.DispatchTime;
                arrivalDispatchVM.KM = item.DispatchKM.Value.ToString();
                arrivalDispatchVM.Remark = item.DispatchRemark;
                arrivalDispatchVM.ScheduleDate = item.DispatchSchDate.Value.ToShortDateString();
                if (!String.IsNullOrEmpty(item.DispatchSchTime))
                {
                    //dt = DateTime.ParseExact(item.DispatchSchTime, "HH:mm", null, DateTimeStyles.None);
                    //arrivalDispatchVM.ScheduleTime = dt.ToString("HH:mm tt");
                    arrivalDispatchVM.ScheduleTime = item.DispatchSchTime;
                }

                //arrivalDispatchVM.ScheduleKM = item.di;
                List.Add(arrivalDispatchVM);
            }

            var html = ViewHelper.RenderPartialView(this, "_DispatchList", List);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }
    }
}