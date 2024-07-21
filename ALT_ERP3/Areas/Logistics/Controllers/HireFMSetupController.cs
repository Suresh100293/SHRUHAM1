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

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class HireFMSetupController : BaseController
    {
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;


        public JsonResult FMType(string term)//FMType
        {
            var list = ctxTFAT.VehicleGrpStatusMas.ToList().Distinct();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = ctxTFAT.VehicleGrpStatusMas.Where(x => x.VehicleGroupStatus.ToLower().Contains(term.ToLower())).ToList().Distinct();
            }

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.VehicleGroupStatus
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DebitAccount(string term)//FMType
        {
            var list = ctxTFAT.Master.Where(x => x.AcType == "X").ToList().Distinct();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = ctxTFAT.Master.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList().Distinct();
            }

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }
        public JsonResult HirePostAccount(string term)//FMType
        {
            if (term != "" && term != null)
            {
                var result = ctxTFAT.Master.Where(x => (x.Name.Contains(term) || x.Code.Contains(term)) && x.Hide == false && (x.AppBranch.Contains(mbranchcode) || x.AppBranch == "All")).Select(c => new { c.Code, c.Name }).Take(10).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.Master.Where(x => x.Hide == false && (x.AppBranch.Contains(mbranchcode) || x.AppBranch == "All")).Select(c => new { c.Code, c.Name }).Take(10).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }
        private List<SelectListItem> PopulatePrintFormats()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            var list = ctxTFAT.DocFormats.Where(x => x.Type == "FMH00").OrderBy(x => x.FormatCode).ToList();
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

        // GET: Logistics/HireFMSetup
        public ActionResult Index(FMSetupVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, "Setup", mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;
            mModel.PrintFormats = PopulatePrintFormats();

            var mList = ctxTFAT.HireFMSetup.FirstOrDefault();
            if (mList != null)
            {
                if (mList.FMBoth == true)
                {
                    mModel.Both = true;
                }
                else if (mList.FMGenerate == true)
                {
                    mModel.FMAutomatic = true;
                }
                else
                {
                    mModel.FMManual = true;
                }

                

                mModel.EditReq = mList.EditReq;
                mModel.DeleteReq = mList.DeleteReq;
                mModel.EditHours = mList.EditUptoHours;
                mModel.DeleteHours = mList.DeleteUptoHours;

                mModel.Class_CurrDatetOnlyreq = mList.CurrDatetOnlyreq;
                mModel.Class_BackDateAllow = mList.BackDateAllow;
                mModel.Class_BackDaysUpto = mList.BackDaysUpto;
                mModel.Class_ForwardDateAllow = mList.ForwardDateAllow;
                mModel.Class_ForwardDaysUpto = mList.ForwardDaysUpto;
                mModel.Class_BranchwiseSrlReq = mList.BranchwiseSrlReq;
                mModel.Class_YearwiseSrlReq = mList.YearwiseSrlReq;
                mModel.Class_CetralisedSrlReq = mList.CetralisedSrlReq;
                mModel.Class_Srl = mList.Srl;

                mModel.KM = mList.KM;
                mModel.Payload = mList.Payload;
                mModel.LicenceNo = mList.LicenceNo;
                mModel.LicenceExpDate = mList.LicenceExpDate;
                mModel.DriverName = mList.DriverName;
                mModel.ContactNo = mList.ContactNo;

                mModel.FillFromCurr = mList.FillFromCurr;


             
                mModel.ShowLedgerPost = mList.ShowLedgerPost;
                mModel.HireTDSCut = mList.HTDSAdjReq;
                var Category = ctxTFAT.VehicleGrpStatusMas.Where(x=>x.Code== "100001").ToList();
                mModel.FM_Vehicle_Category_Name = Category.Select(x => x.VehicleGroupStatus).ToArray();
                mModel.FM_Vehicle_Category_Code = Category.Select(x => x.Code).ToArray();
                mModel.HireDebitAcCode = mList.HireDrAc;
                mModel.HireDebitAcName = ctxTFAT.Master.Where(X => X.Code == mList.HireDrAc).Select(X => X.Name).FirstOrDefault();

                //mModel.NoPosting = mList.NoPosting;
                //mModel.DriverAdvancePayable = mList.DriverAdvancePayable;
                //mModel.FmVouRel = mList.FmVouRel;
                mModel.GenerateSchedule = mList.GenerateSchedule;
                mModel.FooterDetails1 = mList.FooterDetails1;
                mModel.FooterDetails2 = mList.FooterDetails2;
                mModel.FooterDetails3 = mList.FooterDetails3;
                mModel.FooterDetails4 = mList.FooterDetails4;
                mModel.AllowToChgTDS = mList.AllowToChgTDS;

                mModel.PrintFormat = mList.DefaultPrint;

                List<bool> VehicleCategoryMaintains = Enumerable.Repeat(false, Category.Count).ToList();
                if (!String.IsNullOrEmpty(mList.VehicleCateStsMain))
                {
                    var VehicleStatusMaintainsCode = mList.VehicleCateStsMain.Split(',');
                    foreach (var item in VehicleStatusMaintainsCode)
                    {
                        if (Array.IndexOf(mModel.FM_Vehicle_Category_Code, item) != -1)
                        {

                            VehicleCategoryMaintains[Array.IndexOf(mModel.FM_Vehicle_Category_Code, item)] = true;
                        }
                    }
                }

                mModel.FM_Vehicle_Category_Status_Maintain_Flag = VehicleCategoryMaintains.ToArray();

                mModel.FmGetVehicleBranchWise = mList.VehiclesBranchWise;

                mModel.FmGetVehicleReadyStatus = mList.VehicleReadyStsOnly;
                mModel.CheckManualFM = mList.ManualFMCheck;

                mModel.ChangeBroker = mList.ChgBroker;
                mModel.ChangeDriver = mList.ChgDriver;
            }
            else
            {
                var Category = ctxTFAT.VehicleGrpStatusMas.Where(x => x.Code == "100001").ToList();
                mModel.FM_Vehicle_Category_Name = Category.Select(x => x.VehicleGroupStatus).ToArray();
                mModel.FM_Vehicle_Category_Code = Category.Select(x => x.Code).ToArray();
                mModel.FM_Vehicle_Category_Status_Maintain_Flag = Enumerable.Repeat(false, Category.Count).ToArray();
                mModel.FmGetVehicleBranchWise = false;
                mModel.FmGetVehicleReadyStatus = true;
                mModel.GenerateSchedule = false;
            }

            
            return View(mModel);
        }

        public ActionResult SaveData(FMSetupVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    //iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }

                    HireFMSetup mobj = new HireFMSetup();
                    bool mAdd = true;
                    var id = Convert.ToInt64(1);
                    if (ctxTFAT.HireFMSetup.FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.HireFMSetup.FirstOrDefault();
                        mAdd = false;
                    }

                    mobj.GenerateSchedule = mModel.GenerateSchedule;
                    mobj.FooterDetails1 = mModel.FooterDetails1;
                    mobj.FooterDetails2 = mModel.FooterDetails2;
                    mobj.FooterDetails3 = mModel.FooterDetails3;
                    mobj.FooterDetails4 = mModel.FooterDetails4;

                    if (mModel.Both == true)
                    {
                        mobj.FMBoth = true;
                        mobj.FMGenerate = false;
                    }
                    else if (mModel.FMAutomatic == true)
                    {
                        mobj.FMBoth = false;
                        mobj.FMGenerate = true;
                    }
                    else
                    {
                        mobj.FMBoth = false;
                        mobj.FMGenerate = false;
                    }
                    mobj.FillFromCurr = mModel.FillFromCurr;
                    
                    mobj.EditUptoHours = mModel.EditHours;
                    mobj.DeleteUptoHours = mModel.DeleteHours;
                    mobj.EditReq = mModel.EditReq;
                    mobj.DeleteReq = mModel.DeleteReq;
                    mobj.ManualFMCheck = mModel.CheckManualFM;

                    mobj.ChgBroker = mModel.ChangeBroker;
                    mobj.ChgDriver = mModel.ChangeDriver;

                    mobj.ShowLedgerPost = mModel.ShowLedgerPost;

                    mobj.KM = mModel.KM;
                    mobj.Payload = mModel.Payload;
                    mobj.LicenceNo = mModel.LicenceNo;
                    mobj.LicenceExpDate = mModel.LicenceExpDate;
                    mobj.DriverName = mModel.DriverName;
                    mobj.ContactNo = mModel.ContactNo;


                    mobj.VehicleCateStsMain = mModel.Selected_FM_Vehicle_Category_Status_Maintain;
                    mobj.VehiclesBranchWise = mModel.FmGetVehicleBranchWise;
                    mobj.VehicleReadyStsOnly = mModel.FmGetVehicleReadyStatus;
                    mobj.HTDSAdjReq = mModel.HireTDSCut;

                    mobj.HireDrAc = mModel.HireDebitAcCode;
                    
                    mobj.AllowToChgTDS = mModel.AllowToChgTDS;


                    mobj.CurrDatetOnlyreq = mModel.Class_CurrDatetOnlyreq;
                    mobj.BackDateAllow = mModel.Class_BackDateAllow;
                    mobj.BackDaysUpto = mModel.Class_BackDaysUpto;
                    mobj.ForwardDateAllow = mModel.Class_ForwardDateAllow;
                    mobj.ForwardDaysUpto = mModel.Class_ForwardDaysUpto;
                    mobj.BranchwiseSrlReq = mModel.Class_BranchwiseSrlReq;
                    mobj.YearwiseSrlReq = mModel.Class_YearwiseSrlReq;
                    mobj.CetralisedSrlReq = mModel.Class_CetralisedSrlReq;
                    mobj.Srl = mModel.Class_Srl;


                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    mobj.AUTHORISE = mauthorise;
                    mobj.AUTHIDS = muserid;
                    mobj.DefaultPrint = "";

                    ExecuteStoredProc("Update DocFormats set Selected='false' where type='FMH00'");
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
                        ctxTFAT.HireFMSetup.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }

                    DocTypes docTypes = ctxTFAT.DocTypes.Where(x => x.Code == "FMH00").FirstOrDefault();
                    if (docTypes != null)
                    {
                        docTypes.GSTNoCommon = mModel.Class_BranchwiseSrlReq;
                        docTypes.CommonSeries = mModel.Class_YearwiseSrlReq;
                        docTypes.Centralised = mModel.Class_CetralisedSrlReq;
                        docTypes.LimitFrom = mModel.Class_Srl.ToString().Trim();
                        docTypes.LimitTo = "999999".PadLeft(docTypes.LimitFrom.Length, '9');
                        docTypes.DocWidth = docTypes.LimitFrom.Length;
                        ctxTFAT.Entry(docTypes).State = EntityState.Modified;
                    }


                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, "Setup", mModel.Header, "", DateTime.Now, 0, "", "Save Hire Freight Memo Setup","NA");
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

        public ActionResult DeleteStateMaster(FMSetupVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Charge Type not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var mList = ctxTFAT.HireFMSetup.FirstOrDefault();
            ctxTFAT.HireFMSetup.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
    }
}