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

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class SystemFlowController : BaseController
    {
        private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mauthorise = "A00";
        private static string mdocument = "";


        // GET: Logistics/SystemFlow
        public ActionResult Index(SystemFlowVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, "Setup", mModel.Header, "", DateTime.Now, 0, "", "", "NA");

            LogisticsFlow mobj = ctxTFAT.LogisticsFlow.FirstOrDefault();
            mModel.Mode = "Edit";

            if (mobj != null)
            {
                mModel.Days = mobj.DaysList;
                mModel.GeneralUnloadReq = mobj.GeneralUnloadReq;
                mModel.ScheduleFollowUp = mobj.ScheduleFollowUp;
                mModel.ADCriteriaCheck = mobj.ADCriteriaCheck;
                mModel.RouteCheckMaterial = mobj.RouteCheckMaterial;
                mModel.RouteClearReq = mobj.RouteClearReq;
                mModel.DestCheckMaterial = mobj.DestCheckMaterial;
                mModel.DestClearReq = mobj.DestClearReq;
                mModel.ReworkFM = mobj.Rework;
                mModel.UnloadAllMaterialReq = mobj.UnloadAllMaterialReq;


                var Category = ctxTFAT.VehicleGrpStatusMas.ToList();
                var SaveCategoryOfArrivalDispatch = new List<string>();
                var SaveCategoryOfSchedule = new List<string>();
                if (!String.IsNullOrEmpty(mobj.ADVehicleCategReq))
                {
                    SaveCategoryOfArrivalDispatch = mobj.ADVehicleCategReq.Split(',').ToList();
                }
                if (!String.IsNullOrEmpty(mobj.ScheduleCategReq))
                {
                    SaveCategoryOfSchedule = mobj.ScheduleCategReq.Split(',').ToList();
                }

                List<VehiclCategoryList> categoryLists = new List<VehiclCategoryList>();
                foreach (var item in Category)
                {
                    bool ADReq = false, SchedulReq = false;
                    categoryLists.Add(new VehiclCategoryList
                    {
                        Name = item.VehicleGroupStatus,
                        Code = item.Code,
                        ArrivalDispatchReq = SaveCategoryOfArrivalDispatch.Contains(item.Code),
                        SeheduleReq = SaveCategoryOfSchedule.Contains(item.Code),
                    });
                }

                mModel.categoryLists = categoryLists;


                //mModel.FM_Vehicle_Category_Name = Category.Select(x => x.VehicleGroupStatus).ToArray();
                //mModel.FM_Vehicle_Category_Code = Category.Select(x => x.Code).ToArray();

                //List<bool> VehicleCategoryArrival_Dispatch = Enumerable.Repeat(false, Category.Count).ToList();
                //if (!String.IsNullOrEmpty(mobj.ADVehicleCategReq))
                //{
                //    var VehicleStatusArrival_Dispatch_Code = mobj.ADVehicleCategReq.Split(',');
                //    foreach (var item in VehicleStatusArrival_Dispatch_Code)
                //    {
                //        if (Array.IndexOf(mModel.FM_Vehicle_Category_Code, item) != -1)
                //        {
                //            VehicleCategoryArrival_Dispatch[Array.IndexOf(mModel.FM_Vehicle_Category_Code, item)] = true;
                //        }
                //    }
                //}

                //mModel.Arrival_Dispatch_Vehicle_Category_Flag = VehicleCategoryArrival_Dispatch.ToArray();

            }
            else
            {
                mModel.Mode = "Add";
                mModel.Days = 30;

                mModel.ScheduleFollowUp = false;

                var Category = ctxTFAT.VehicleGrpStatusMas.ToList();
                List<VehiclCategoryList> categoryLists = new List<VehiclCategoryList>();
                foreach (var item in Category)
                {
                    bool ADReq = false, SchedulReq = false;
                    categoryLists.Add(new VehiclCategoryList
                    {
                        Name = item.VehicleGroupStatus,
                        Code = item.Code,
                        ArrivalDispatchReq = false,
                        SeheduleReq = false,
                    });
                }

                mModel.categoryLists = categoryLists;
            }

            return View(mModel);
        }

        public ActionResult SaveData(SystemFlowVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode

                    LogisticsFlow mobj = new LogisticsFlow();
                    mobj = ctxTFAT.LogisticsFlow.FirstOrDefault();
                    if (mobj != null)
                    {
                        mobj.DaysList = mModel.Days;
                        mobj.GeneralUnloadReq = mModel.GeneralUnloadReq;
                        mobj.ScheduleFollowUp = mModel.ScheduleFollowUp;
                        mobj.ADVehicleCategReq = mModel.ArrivalAndDispatchReq;
                        mobj.ScheduleCategReq = mModel.ScheduleReq;
                        mobj.ADCriteriaCheck = mModel.ADCriteriaCheck;
                        mobj.RouteCheckMaterial = mModel.RouteCheckMaterial;
                        mobj.RouteClearReq = mModel.RouteClearReq;
                        mobj.DestCheckMaterial = mModel.DestCheckMaterial;
                        mobj.DestClearReq = mModel.DestClearReq;
                        mobj.Rework = mModel.ReworkFM;
                        mobj.UnloadAllMaterialReq = mModel.UnloadAllMaterialReq;

                        mobj.AUTHIDS = muserid;
                        mobj.AUTHORISE = mauthorise;
                        mobj.ENTEREDBY = muserid;
                        mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    else
                    {
                        mobj = new LogisticsFlow();
                        mobj.DaysList = mModel.Days;
                        mobj.ScheduleFollowUp = mModel.ScheduleFollowUp;
                        mobj.ADCriteriaCheck = mModel.ADCriteriaCheck;
                        mobj.ADVehicleCategReq = mModel.ArrivalAndDispatchReq;
                        mobj.ScheduleCategReq = mModel.ScheduleReq;
                        mobj.RouteCheckMaterial = mModel.RouteCheckMaterial;
                        mobj.RouteClearReq = mModel.RouteClearReq;
                        mobj.DestCheckMaterial = mModel.DestCheckMaterial;
                        mobj.DestClearReq = mModel.DestClearReq;

                        mobj.AUTHIDS = muserid;
                        mobj.AUTHORISE = mauthorise;
                        mobj.ENTEREDBY = muserid;
                        mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                        ctxTFAT.LogisticsFlow.Add(mobj);
                    }

                    //ReportHeader Arrival = ctxTFAT.ReportHeader.Where(x => x.Code == "Arrival").FirstOrDefault();
                    //ReportHeader Loading = ctxTFAT.ReportHeader.Where(x => x.Code == "UnLoading").FirstOrDefault();
                    //ReportHeader UnLoading = ctxTFAT.ReportHeader.Where(x => x.Code == "Loading_Dispatch").FirstOrDefault();
                    //ReportHeader Dispatch = ctxTFAT.ReportHeader.Where(x => x.Code == "Dispatch").FirstOrDefault();
                    //ReportHeader LoadingToDispatch = ctxTFAT.ReportHeader.Where(x => x.Code == "LoadingToDispatch").FirstOrDefault();
                    //var TableQry = "";
                    //if (mModel.BranchFlow)
                    //{
                    //    TableQry = "     FMMaster FM where CurrBranch in (select Childs from BranchChild where Code='%BRANCH')  ";
                    //}
                    //else
                    //{
                    //    TableQry = "  FMMaster FM where FM.FmNo = (select FmNo from FM_ROUTE_Table where RouteVia in (select Childs from BranchChild where Code='%BRANCH') and FM.FmNo=FmNo)        ";
                    //}
                    //Arrival.Tables = TableQry;
                    //ctxTFAT.Entry(Arrival).State = EntityState.Modified;
                    //Loading.Tables = TableQry;
                    //ctxTFAT.Entry(Loading).State = EntityState.Modified;
                    //UnLoading.Tables = TableQry;
                    //ctxTFAT.Entry(UnLoading).State = EntityState.Modified;
                    //Dispatch.Tables = TableQry;
                    //ctxTFAT.Entry(Dispatch).State = EntityState.Modified;
                    //LoadingToDispatch.Tables = TableQry;
                    //ctxTFAT.Entry(LoadingToDispatch).State = EntityState.Modified;


                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    string mNewCode = "";
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, "Setup", mModel.Header, "", DateTime.Now, 0, "", "Save Vehicle Activity Setup", "NA");
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