using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Vehicles.Models;
using ALT_ERP3.Controllers;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Vehicles.Controllers
{
    public class VehiclesRatesController : BaseController
    {
        // GET: Vehicles/VehiclesRates
         
        //ITransactionGridOperation mIlst = new TransactionGridOperation();
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        private static string mdocument = "";
        private static string mauthorise = "A00";
        private int mnewrecordkey = 0;

        public ActionResult Index(GridOption Model)
        {

            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());

            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "","A");

            if (Model.Document == null)
            {
                Model.Document = "";
                Model.AccountName = "";
            }
            moptioncode = Model.OptionCode;
            msubcodeof = Model.ViewDataId;
            mmodule = Model.Module;
            ViewBag.id = Model.ViewDataId;
            ViewBag.ViewDataId = Model.ViewDataId;
            ViewBag.Header = Model.Header;
            ViewBag.Table = Model.TableName;
            ViewBag.Controller = Model.Controller;
            ViewBag.MainType = Model.MainType;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;
            ViewBag.ViewName = Model.ViewName;
            if (Model.AcType != null)
            {
                ViewBag.MainType = Model.AcType;
            }

            #region Dropdown
            List<string> list = new List<string>();
            list.Add("-- Select Rate Type --");
            list.Add("Vehicle Wise");
            list.Add("Vehicle Category Wise");
            List<string> listValue = new List<string>();
            listValue.Add("0");
            listValue.Add("VehicleRate");
            listValue.Add("VehicleCategoryRate");

            var selectList = new List<SelectListItem>();
            for (int i = 0; i < list.Count; i++)
            {
                selectList.Add(new SelectListItem
                {
                    Value = listValue[i].ToString(),
                    Text = list[i].ToString()
                });
            }
            #endregion

            ViewBag.RateType = selectList;
            return View(Model);
        }

        public ActionResult NewIndex(NewVehiclesRatesVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());

            //Below Model Table Object
            List<VehicleRateAndCategory> AllVehicleRates = new List<VehicleRateAndCategory>();
            VehicleRateAndCategory allVehicleRates = new VehicleRateAndCategory();

            //Below DataBase Table Object
            VehicleRates VehicleRates = new VehicleRates();
            VehicleCategoryRates allVehicleCategoryRates = new VehicleCategoryRates();
            KilometerMaster kilometerMaster = new KilometerMaster();
            //convert Code To Name(From Field)
            mModel.FromName = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.From).Select(x => x.Name).FirstOrDefault();
            if (mModel.typeofRate == "VehicleRate")
            {

                var GetTolist = ctxTFAT.TfatBranch.Where(x => x.Code != mModel.From && x.Category != "Zone" && x.Category != "0").Select(x => new { x.Code, x.Name,x.Category }).ToList();
                foreach (var item in GetTolist)
                {
                    //kilometerMaster = ctxTFAT.KilometerMaster.Where(x => x.FromBranch == mModel.From && x.ToBranch == item.Code).FirstOrDefault();
                    VehicleRates = ctxTFAT.VehicleRates.Where(x => x.VehicleNo == mModel.VehicleNo_Category && x.FromBranch == mModel.From && x.ToBranch == item.Code).FirstOrDefault();
                    if (VehicleRates != null)
                    {
                        var AreaType = item.Category == "Branch" ? "-B" : item.Category == "SubBranch" ? "-SB" : "-A";
                        allVehicleRates = new VehicleRateAndCategory
                        {
                            Code = VehicleRates.Code,
                            To = VehicleRates.ToBranch,
                            ToName = ctxTFAT.TfatBranch.Where(x => x.Code == VehicleRates.ToBranch).Select(x => x.Name).FirstOrDefault().ToLower() + AreaType,
                            //KM = kilometerMaster.KM,
                            Rate = VehicleRates.Rate,
                            AdvRate = VehicleRates.AdvRate,
                            Reporting = VehicleRates.Reporting
                        };
                    }
                    else
                    {
                        var AreaType = item.Category == "Branch" ? "-B" : item.Category == "SubBranch" ? "-SB" : "-A";

                        allVehicleRates = new VehicleRateAndCategory
                        {
                            To = item.Code,
                            ToName = ctxTFAT.TfatBranch.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault().ToLower() + AreaType,
                            //KM = kilometerMaster == null ? 0 : kilometerMaster.KM,
                            Rate = 0,
                            AdvRate = 0,
                            Reporting = "0.0",
                        };
                    }
                    AllVehicleRates.Add(allVehicleRates);
                }

            }
            else
            {
                var GetTolist = ctxTFAT.TfatBranch.Where(x => x.Code != mModel.From && x.Category != "Zone" && x.Category != "0").Select(x => new { x.Code, x.Name,x.Category }).ToList();
                foreach (var item in GetTolist)
                {
                    //kilometerMaster = ctxTFAT.KilometerMaster.Where(x => x.FromBranch == mModel.From && x.ToBranch == item.Code).FirstOrDefault();
                    allVehicleCategoryRates = ctxTFAT.VehicleCategoryRates.Where(x => x.VehicleCategory == mModel.VehicleNo_Category && x.FromBranch == mModel.From && x.ToBranch == item.Code).FirstOrDefault();
                    if (allVehicleCategoryRates != null)
                    {
                        var AreaType = item.Category == "Branch" ? "-B" : item.Category == "SubBranch" ? "-SB" : "-A";
                        allVehicleRates = new VehicleRateAndCategory
                        {
                            Code = allVehicleCategoryRates.Code,
                            To = allVehicleCategoryRates.ToBranch,
                            ToName = ctxTFAT.TfatBranch.Where(x => x.Code == allVehicleCategoryRates.ToBranch).Select(x => x.Name).FirstOrDefault().ToLower() + AreaType,
                            //KM = kilometerMaster.KM,
                            Rate = allVehicleCategoryRates.Rate,
                            AdvRate = allVehicleCategoryRates.AdvRate,
                            Reporting = allVehicleCategoryRates.Reporting
                        };
                    }
                    else
                    {
                        var AreaType = item.Category == "Branch" ? "-B" : item.Category == "SubBranch" ? "-SB" : "-A";

                        allVehicleRates = new VehicleRateAndCategory
                        {
                            To = item.Code,
                            ToName = ctxTFAT.TfatBranch.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault().ToLower() + AreaType,
                            //KM = kilometerMaster == null ? 0 : kilometerMaster.KM,
                            Rate = 0,
                            AdvRate = 0,
                            Reporting = "0.0",
                        };
                    }
                    AllVehicleRates.Add(allVehicleRates);
                }
            }
            mModel.VehiclesList = AllVehicleRates;
            return View(mModel);
        }

        public ActionResult CopyKM(NewVehiclesRatesVM mModel)
        {
            //Below Model Table Object
            List<VehicleRateAndCategory> AllVehicleRates = new List<VehicleRateAndCategory>();
            VehicleRateAndCategory allVehicleRates = new VehicleRateAndCategory();

            //Below DataBase Table Object
            VehicleRates VehicleRates = new VehicleRates();
            VehicleCategoryRates allVehicleCategoryRates = new VehicleCategoryRates();
            KilometerMasterRef kilometerMaster = new KilometerMasterRef();
            List<string> SelectedToDestination = new List<string>();
            if (!String.IsNullOrEmpty(mModel.Document))
            {
                SelectedToDestination = mModel.Document.Split(',').ToList();
            }
            //convert Code To Name(From Field)
            mModel.FromName = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.From).Select(x => x.Name).FirstOrDefault();
            if (mModel.typeofRate == "VehicleRate")
            {

                var GetTolist = ctxTFAT.TfatBranch.Where(x => x.Code != mModel.From && x.Category != "Zone" && x.Category != "0").Select(x => new { x.Code, x.Name }).ToList();
                foreach (var item in GetTolist)
                {
                    kilometerMaster = ctxTFAT.KilometerMasterRef.Where(x => x.FromBranch == mModel.From && x.ToBranch == item.Code).FirstOrDefault();
                    if (SelectedToDestination.Contains(item.Code))
                    {
                        VehicleRates = ctxTFAT.VehicleRates.Where(x => x.VehicleNo == mModel.VehicleNo_Category && x.FromBranch == mModel.CopyBranch && x.ToBranch == item.Code).FirstOrDefault();
                    }
                    else
                    {
                        VehicleRates = ctxTFAT.VehicleRates.Where(x => x.VehicleNo == mModel.VehicleNo_Category && x.FromBranch == mModel.From && x.ToBranch == item.Code).FirstOrDefault();
                    }
                    if (VehicleRates != null)
                    {
                        var AreaType = ctxTFAT.TfatBranch.Where(x => x.Code == VehicleRates.ToBranch).Select(x => x.Category).FirstOrDefault() == "Branch" ? "-B" : ctxTFAT.TfatBranch.Where(x => x.Code == VehicleRates.ToBranch).Select(x => x.Name).FirstOrDefault() == "SubBranch" ? "-SB" : "-A";
                        allVehicleRates = new VehicleRateAndCategory
                        {
                            Code = VehicleRates.Code,
                            To = VehicleRates.ToBranch,
                            ToName = ctxTFAT.TfatBranch.Where(x => x.Code == VehicleRates.ToBranch).Select(x => x.Name).FirstOrDefault().ToLower() + AreaType,
                            KM = Convert.ToDouble(kilometerMaster.KM),
                            Rate = VehicleRates.Rate,
                            AdvRate = VehicleRates.AdvRate,
                            Reporting = VehicleRates.Reporting
                        };
                    }
                    else
                    {
                        var AreaType = ctxTFAT.TfatBranch.Where(x => x.Code == item.Code).Select(x => x.Category).FirstOrDefault() == "Branch" ? "-B" : ctxTFAT.TfatBranch.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault() == "SubBranch" ? "-SB" : "-A";

                        allVehicleRates = new VehicleRateAndCategory
                        {
                            To = item.Code,
                            ToName = ctxTFAT.TfatBranch.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault().ToLower() + AreaType,
                            KM = kilometerMaster == null ? 0 : Convert.ToDouble(kilometerMaster.KM),
                            Rate = 0,
                            AdvRate = 0,
                            Reporting = "0.0",
                        };
                    }
                    AllVehicleRates.Add(allVehicleRates);
                }

            }
            else
            {
                //mModel.VehicleNo_Category = ctxTFAT.VehicleCategory.Where(x => x.Code == mModel.VehicleNo_Category).Select(x => x.vehicle_Category).FirstOrDefault();
                var GetTolist = ctxTFAT.TfatBranch.Where(x => x.Code != mModel.CopyBranch && x.Category != "Zone" && x.Category != "0").Select(x => new { x.Code, x.Name }).ToList();
                foreach (var item in GetTolist)
                {
                    kilometerMaster = ctxTFAT.KilometerMasterRef.Where(x => x.FromBranch == mModel.From && x.ToBranch == item.Code).FirstOrDefault();
                    if (SelectedToDestination.Contains(item.Code))
                    {
                        allVehicleCategoryRates = ctxTFAT.VehicleCategoryRates.Where(x => x.VehicleCategory == mModel.VehicleNo_Category && x.FromBranch == mModel.CopyBranch && x.ToBranch == item.Code).FirstOrDefault();
                    }
                    else
                    {
                        allVehicleCategoryRates = ctxTFAT.VehicleCategoryRates.Where(x => x.VehicleCategory == mModel.VehicleNo_Category && x.FromBranch == mModel.From && x.ToBranch == item.Code).FirstOrDefault();
                    }


                    if (allVehicleCategoryRates != null)
                    {
                        var AreaType = ctxTFAT.TfatBranch.Where(x => x.Code == allVehicleCategoryRates.ToBranch).Select(x => x.Category).FirstOrDefault() == "Branch" ? "-B" : ctxTFAT.TfatBranch.Where(x => x.Code == allVehicleCategoryRates.ToBranch).Select(x => x.Name).FirstOrDefault() == "SubBranch" ? "-SB" : "-A";
                        allVehicleRates = new VehicleRateAndCategory
                        {
                            Code = allVehicleCategoryRates.Code,
                            To = allVehicleCategoryRates.ToBranch,
                            ToName = ctxTFAT.TfatBranch.Where(x => x.Code == allVehicleCategoryRates.ToBranch).Select(x => x.Name).FirstOrDefault().ToLower() + AreaType,
                            KM = Convert.ToDouble(kilometerMaster.KM),
                            Rate = allVehicleCategoryRates.Rate,
                            AdvRate = allVehicleCategoryRates.AdvRate,
                            Reporting = allVehicleCategoryRates.Reporting
                        };
                    }
                    else
                    {
                        var AreaType = ctxTFAT.TfatBranch.Where(x => x.Code == item.Code).Select(x => x.Category).FirstOrDefault() == "Branch" ? "-B" : ctxTFAT.TfatBranch.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault() == "SubBranch" ? "-SB" : "-A";

                        allVehicleRates = new VehicleRateAndCategory
                        {
                            To = item.Code,
                            ToName = ctxTFAT.TfatBranch.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault().ToLower() + AreaType,
                            KM = kilometerMaster == null ? 0 : Convert.ToDouble(kilometerMaster.KM),
                            Rate = 0,
                            AdvRate = 0,
                            Reporting = "0.0",
                        };
                    }
                    AllVehicleRates.Add(allVehicleRates);
                }
            }
            mModel.VehiclesList = AllVehicleRates;

            var html = ViewHelper.RenderPartialView(this, "_PartialView", mModel);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            //return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }


        #region SaveData
        public ActionResult SaveData(NewVehiclesRatesVM mModel)
        {
            //bool Status = false;

            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {


                try
                {

                    if (mModel.typeofRate == "VehicleRate")
                    {
                        var GetPreviousCode = ctxTFAT.VehicleRates.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).Take(1).FirstOrDefault();
                        int NewCodeGenerate = Convert.ToInt32(GetPreviousCode) + 1;

                        VehicleRates newVehiclesRatesVM = new VehicleRates();
                        List<VehicleRates> Add = new List<VehicleRates>();
                        List<VehicleRates> Update = new List<VehicleRates>();

                        foreach (var item in mModel.VehiclesList)
                        {
                            VehicleRates GetVehicleKm = ctxTFAT.VehicleRates.Where(x => x.Code == item.Code).FirstOrDefault();
                            if (GetVehicleKm == null)
                            {
                                //add
                                newVehiclesRatesVM = new VehicleRates
                                {
                                    Code = NewCodeGenerate.ToString("D6"),
                                    VehicleNo = mModel.VehicleNo_Category,
                                    FromBranch = mModel.From,
                                    ToBranch = item.To,
                                    //KM = item.KM,
                                    Rate = item.Rate,
                                    AdvRate = item.AdvRate,
                                    Reporting = item.Reporting,
                                    AUTHIDS = muserid,
                                    AUTHORISE = mauthorise,
                                    ENTEREDBY = muserid,
                                    LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString())
                                };
                                Add.Add(newVehiclesRatesVM);
                                ++NewCodeGenerate;
                            }
                            else
                            {
                                //update
                                //GetVehicleKm.KM = item.KM;
                                GetVehicleKm.Rate = item.Rate;
                                GetVehicleKm.AdvRate = item.AdvRate;
                                GetVehicleKm.Reporting = item.Reporting;
                                GetVehicleKm.AUTHIDS = muserid;
                                GetVehicleKm.AUTHORISE = mauthorise;
                                GetVehicleKm.ENTEREDBY = muserid;
                                GetVehicleKm.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                                Update.Add(GetVehicleKm);
                            }

                        }

                        if (Add.Count != 0)
                        {
                            foreach (var item in Add)
                            {
                                ctxTFAT.VehicleRates.Add(item);
                            }
                        }
                        if (Update.Count != 0)
                        {
                            foreach (var item in Update)
                            {
                                ctxTFAT.Entry(item).State = EntityState.Modified;
                            }
                        }
                        ctxTFAT.SaveChanges();
                        transaction.Commit();
                        transaction.Dispose();
                        UpdateAuditTrail(mbranchcode, mModel.Mode, "Vehicle Rate Master", "", DateTime.Now, 0, "", "","A");
                    }
                    else
                    {
                        var GetPreviousCode = ctxTFAT.VehicleCategoryRates.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).Take(1).FirstOrDefault();
                        int NewCodeGenerate = Convert.ToInt32(GetPreviousCode) + 1;
                        //mModel.VehicleNo_Category = ctxTFAT.VehicleCategory.Where(x => x.vehicle_Category.ToUpper().Trim().Contains(mModel.VehicleNo_Category.ToUpper().Trim())).Select(x => x.Code).FirstOrDefault();
                        VehicleCategoryRates newVehiclesRatesVM = new VehicleCategoryRates();
                        List<VehicleCategoryRates> Add = new List<VehicleCategoryRates>();
                        List<VehicleCategoryRates> Update = new List<VehicleCategoryRates>();

                        foreach (var item in mModel.VehiclesList)
                        {
                            if (item.KM != 0 || item.Rate != 0 || item.AdvRate != 0 || item.Reporting != "00:00")
                            {
                                VehicleCategoryRates GetVehicleKm = ctxTFAT.VehicleCategoryRates.Where(x => x.Code == item.Code).FirstOrDefault();
                                if (GetVehicleKm == null)
                                {
                                    item.To = item.To.Substring(0, (item.To.IndexOf(" ")));
                                    //add
                                    newVehiclesRatesVM = new VehicleCategoryRates
                                    {
                                        Code = NewCodeGenerate.ToString("D6"),
                                        VehicleCategory = mModel.VehicleNo_Category,
                                        FromBranch = mModel.From,
                                        ToBranch = item.To,
                                        //KM = item.KM,
                                        Rate = (Int32)item.Rate,
                                        AdvRate = (Int32)item.AdvRate,
                                        Reporting = item.Reporting,
                                        AUTHIDS = muserid,
                                        AUTHORISE = mauthorise,
                                        ENTEREDBY = muserid,
                                        LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString())
                                    };
                                    Add.Add(newVehiclesRatesVM);
                                    ++NewCodeGenerate;
                                }
                                else
                                {
                                    //update
                                    //GetVehicleKm.KM = item.KM;
                                    GetVehicleKm.Rate = (Int32)item.Rate;
                                    GetVehicleKm.AdvRate = (Int32)item.AdvRate;
                                    GetVehicleKm.Reporting = item.Reporting;
                                    GetVehicleKm.AUTHIDS = muserid;
                                    GetVehicleKm.AUTHORISE = mauthorise;
                                    GetVehicleKm.ENTEREDBY = muserid;
                                    GetVehicleKm.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                                    Update.Add(GetVehicleKm);
                                }
                            }
                        }

                        if (Add.Count != 0)
                        {
                            foreach (var item in Add)
                            {
                                ctxTFAT.VehicleCategoryRates.Add(item);
                            }
                        }
                        if (Update.Count != 0)
                        {
                            foreach (var item in Update)
                            {
                                ctxTFAT.Entry(item).State = EntityState.Modified;
                            }
                        }
                        ctxTFAT.SaveChanges();
                        transaction.Commit();
                        transaction.Dispose();
                        UpdateAuditTrail(mbranchcode, mModel.Mode, "Vehicle Rate Master", "     ", DateTime.Now, 0, "", "","A");
                    }



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

        public ActionResult DeleteStateMaster(VehicleRateMaster mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            //if (mModel.typeofRate == "Vehicle Wise")
            //{
            //    var mList = ctxTFAT.VehicleRate.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
            //    ctxTFAT.VehicleRate.Remove(mList);
            //    ctxTFAT.SaveChanges();
            //    return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
            //}
            //else
            //{
            //    var mList = ctxTFAT.VehicleCategoryRate.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
            //    ctxTFAT.VehicleCategoryRate.Remove(mList);
            //    ctxTFAT.SaveChanges();
            //    return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
            //}
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData


        #region Functions

        public ActionResult GetFormats()
        {
            var result = ctxTFAT.ReportHeader.Where(x => x.SubCodeOf == msubcodeof && !x.Code.EndsWith(".bak")).Select(m => new
            {
                Code = m.Code,
                Name = m.Code
            }).OrderBy(n => n.Code).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //public string ConvertDDMMYYTOYYMMDD(string da)
        //{
        //    string abc = da.Substring(6, 4) + "/" + da.Substring(3, 2) + "/" + da.Substring(0, 2);
        //    return abc;
        //}

        public JsonResult GetDependencyData(string term, string typeOfRate)
        {
            if (typeOfRate == "VehicleRate")
            {
                var list = ctxTFAT.VehicleMaster.ToList();
                if (!(String.IsNullOrEmpty(term)))
                {
                    list = ctxTFAT.VehicleMaster.Where(x => x.TruckNo.ToLower().Contains(term.ToLower())).ToList();
                }

                var Modified = list.Select(x => new
                {
                    Code = x.TruckNo,
                    Name = x.TruckNo
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = ctxTFAT.VehicleCategory.ToList();
                if (!(String.IsNullOrEmpty(term)))
                {
                    list = ctxTFAT.VehicleCategory.Where(x => x.VehicleCategory1.ToLower().Contains(term.ToLower())).ToList();
                }

                var Modified = list.Select(x => new
                {
                    Code = x.Code,
                    Name = x.VehicleCategory1
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }

            //return Json((from m in ctxTFAT.VehicleMaster
            //             where m.Vehicle_No.ToLower().Contains(term.ToLower())
            //             select new { Name = m.Vehicle_No, Code = m.Vehicle_No }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetVehiclesCategory(string term)
        {
            var list = ctxTFAT.VehicleCategory.ToList();
            if (!(String.IsNullOrEmpty(term)))
            {
                list = ctxTFAT.VehicleCategory.Where(x => x.VehicleCategory1.ToLower().Contains(term.ToLower())).ToList();
            }

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.VehicleCategory1
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
            //return Json((from m in ctxTFAT.VehicleCategory
            //             where m.vehicle_Category.ToLower().Contains(term.ToLower())
            //             select new { Name = m.vehicle_Category, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult From(string term)
        {
            var list = ctxTFAT.TfatBranch.Where(x => (!(x.Code.Contains("h"))) && x.Category != "Zone" && x.Category != "0").ToList();


            //var list = ctxTFAT.KilometerMaster.Join(ctxTFAT.TreeTable, p => p.From, pc => pc.Code, (p, pc) => new { p, pc }).Where(x =>  (!(x.pc.Code.ToLower().Contains("h"))) && (!(x.pc.Code.ToLower().Contains("z")))).Select(X=>X.pc).ToList().Distinct();
            if (!(String.IsNullOrEmpty(term)))
            {
                list = ctxTFAT.TfatBranch.Where(x => x.Name.ToLower().Contains(term.ToLower()) && x.Category != "Zone" && x.Category != "0").ToList();
            }


            List<TfatBranch> treeTables = new List<TfatBranch>();

            foreach (var item in list)
            {
                if (item.Category == "Branch")
                {

                    item.Name += " - B";
                    treeTables.Add(item);
                }
                else if (item.Category == "SubBranch")
                {
                    item.Name += " - SB";
                    treeTables.Add(item);
                }
                else
                {
                    item.Name = item.Name + " - A";
                    treeTables.Add(item);
                }

            }

            var Modified = treeTables.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);

            //return Json((ctxTFAT.KilometerMaster.Join(ctxTFAT.TreeTable, p => p.From, pc => pc.Code, (p, pc) => new { p, pc })
            //             .Where(x => x.pc.Name.ToLower().Contains(term.ToLower()) && (!(x.pc.Code.ToLower().Contains("h"))) &&
            //             (!(x.pc.Code.ToLower().Contains("z")))).Select(m => new
            //             {
            //                 Name = m.pc.Name, // or m.ppc.pc.ProdId
            //                 Code = m.pc.Code
            //             })).ToArray().Distinct(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult RateTypeWise(GridOption Model)
        {
            ViewBag.id = Model.ViewDataId;
            ViewBag.ViewDataId = Model.ViewDataId;
            ViewBag.Header = Model.Header;
            ViewBag.Table = Model.TableName;
            ViewBag.Controller = Model.Controller;
            ViewBag.MainType = Model.MainType;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;
            ViewBag.ViewName = Model.ViewName;
            if (Model.AcType != null)
            {
                ViewBag.MainType = Model.AcType;
            }
            ViewBag.Value2 = Model.Value2;

            DataTable dt = new DataTable();

            if (Model.Value2 == "VehicleRate")
            {
                #region Pivot Query

                var getcolumn = ctxTFAT.VehicleMaster.Select(x => x.TruckNo).ToList();
                string vehicleColumnList = string.Empty;
                foreach (var item in getcolumn)
                {
                    vehicleColumnList += "[" + item.ToString().ToUpper() + "],";
                }
                vehicleColumnList = vehicleColumnList.Substring(0, vehicleColumnList.Length - 1);


                if (Model.IsFormatSelected == true)
                {
                    int noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=0 Where SubCodeOf='" + msubcodeof + "'");
                    noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=-1 Where Code='" + Model.ViewDataId + "'");
                }
                string connstring = GetConnectionString();
                SqlConnection con = new SqlConnection(connstring);
                //string query = "select [From],[To],[KM]," + vehicleColumnList + " From" +
                //    "	(Select V.[Vehicle No],(select Name from TreeTable where K.[From]=Code ) as [From],(select Name from TreeTable where K.[To]=Code ) as [To],[KM],(select Rate from vehiclerate Vr where vr.FromD=K.[From] and vr.ToD=K.[To] and vr.[vehicle no]=V.[Vehicle No] ) as Rate From KilometerMaster K,VehicleMaster V  )  " +
                //    " as pt  pivot  (max(Rate) for [Vehicle No] in (" + vehicleColumnList + ") )as pt1";


                string query = "select  VehicleNO,[From],[To],[KM],Rate,AdvRate,Reporting From	(Select K.Code as Code, K.VehicleNO, (select Name from TfatBranch where code = K.[FromBranch]) as [From],(select Name from TfatBranch where code = K.[ToBranch])as [To],(select KM from KilometerMaster where [FromBranch] = K.[FromBranch] and [ToBranch]=K.[ToBranch]) as[KM],K.Rate,k.AdvRate,K.Reporting       From VehicleRates K, VehicleMaster V )   as pt  pivot(max(Code) for Code in (" + vehicleColumnList + ") )as pt1";
                SqlCommand cmd = new SqlCommand(query, con);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                sda.Fill(dt);
                con.Close();
                con.Dispose();

                #endregion
            }
            else
            {
                #region Pivot Query

                var getcolumn = ctxTFAT.VehicleCategory.Select(x => x.VehicleCategory1).ToList();
                string vehicleColumnList = string.Empty;
                foreach (var item in getcolumn)
                {
                    vehicleColumnList += "[" + item.ToString().ToUpper() + "],";
                }
                vehicleColumnList = vehicleColumnList.Substring(0, vehicleColumnList.Length - 1);


                if (Model.IsFormatSelected == true)
                {
                    int noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=0 Where SubCodeOf='" + msubcodeof + "'");
                    noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=-1 Where Code='" + Model.ViewDataId + "'");
                }
                string connstring = GetConnectionString();
                SqlConnection con = new SqlConnection(connstring);
                //string query = "select [From],[To],[KM]," + vehicleColumnList + " From" +
                //    "	(Select V.[vehicle Category],(select Name from TreeTable where K.[From]=Code ) as [From],(select Name from TreeTable where K.[To]=Code ) as [To],[KM],(select Rate from VehicleCategoryRate Vr where vr.FromD=K.[From] and vr.ToD=K.[To] and vr.[vehicle Category]=V.Code ) as Rate From KilometerMaster K,VehicleCategory V  )  " +
                //    " as pt  pivot  (max(Rate) for [vehicle Category] in (" + vehicleColumnList + ") )as pt1";


                string query = "select  VehicleCategory,[From],[To],[KM],Rate,AdvRate,Reporting From (Select K.Code as Code, (select[vehicleCategory] from VehicleCategory where code = K.VehicleCategory) as VehicleCategory,(select Name from TfatBranch where code = K.[FromBranch])as [From],(select Name from TfatBranch where code = K.[ToBranch])as [To],(select KM from KilometerMaster where [FromBranch] = K.[FromBranch] and [ToBranch]=K.[ToBranch]) as[KM],K.Rate,k.AdvRate,K.Reporting      From VehicleCategoryRates K, VehicleMaster V )   as pt  pivot(max(Code) for Code in (" + vehicleColumnList + ") )as pt1";
                SqlCommand cmd = new SqlCommand(query, con);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                sda.Fill(dt);
                con.Close();
                con.Dispose();

                #endregion
            }

            //var html = ViewHelper.RenderPartialView(this, "~/Views/Shared/_AllVehicleRate.cshtml", Model);
            var html = ViewHelper.RenderPartialView(this, "_AllVehicleRate", dt);
            var jsonResult = Json(new
            {
                list = Model.list,
                Html = html
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public JsonResult To(string term)
        {
            List<TfatBranch> list = ctxTFAT.TfatBranch.Where(x => x.Category != "Zone" && x.Category != "0").ToList();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }

            List<TfatBranch> treeTables = new List<TfatBranch>();

            foreach (var item in list)
            {
                if (item.Category == "Zone")
                {
                    item.Name += " - Z";
                    treeTables.Add(item);
                }
                else if (item.Category == "Branch")
                {
                    item.Name += " - B";
                    treeTables.Add(item);
                }
                else if (item.Category == "SubBranch")
                {
                    item.Name += " - SB";
                    treeTables.Add(item);
                }
                else
                {
                    item.Name = item.Name + " - A";
                    treeTables.Add(item);
                }
            }
            var Modified = treeTables.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }


        #endregion
    }
}