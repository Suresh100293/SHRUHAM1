using ALT_ERP3.Areas.Vehicles.Models;
using ALT_ERP3.Controllers;
using Common;
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
    public class TripChargesMasterController : BaseController
    {
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        private int mnewrecordkey = 0;
        private static string mauthorise = "A00";
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

        public JsonResult GetBranch(string term, bool Branch)
        {
            //var GetParentCode = ctxTFAT.TfatBranch.Where(x => x.Code == mcompcode).Select(x => x.Code).FirstOrDefault();
            List<TfatBranch> list = new List<TfatBranch>();
            if (Branch)
            {
                list = ctxTFAT.TfatBranch.Where(x => (x.Category != "Area") && x.Status == true).OrderBy(x => x.Name).ToList();
            }
            else
            {
                list = ctxTFAT.TfatBranch.Where(x => x.Code != "G00000" && x.Status == true).OrderBy(x => x.Name).ToList();
                //var GeneralArea = ctxTFAT.TfatBranch.Where(x => x.Grp == "G00000" && x.Category == "Area" && x.Status == true).OrderBy(x => x.Name).ToList();
                //list.AddRange(GeneralArea);
            }
            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).OrderBy(x => x.Name).ToList();
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
                else if (item.Category == "0")
                {
                    item.Name += " - HO";
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

        public JsonResult GetVehicle(string term, bool Vehicle)
        {
            //var GetParentCode = ctxTFAT.TfatBranch.Where(x => x.Code == mcompcode).Select(x => x.Code).FirstOrDefault();

            if (Vehicle)
            {
                var list = ctxTFAT.VehicleMaster.Where(x => x.Acitve == true).ToList();
                if (!(String.IsNullOrEmpty(term)))
                {
                    list = list.Where(x => x.TruckNo.ToLower().Contains(term.ToLower())).OrderBy(x => x.TruckNo).ToList();
                }
                var Modified = list.Select(x => new
                {
                    Code = x.Code,
                    Name = x.TruckNo
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = ctxTFAT.VehicleCategory.Where(x => x.Acitve == true).ToList();
                if (!(String.IsNullOrEmpty(term)))
                {
                    list = list.Where(x => x.VehicleCategory1.ToLower().Contains(term.ToLower())).OrderBy(x => x.VehicleCategory1).ToList();
                }
                var Modified = list.Select(x => new
                {
                    Code = x.Code,
                    Name = x.VehicleCategory1
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }

        }

        public string GetCode()
        {
            var mobj = ctxTFAT.TripChargesMa.OrderByDescending(x => x.RECORDKEY).Select(x => x.DocNo).FirstOrDefault();
            if (String.IsNullOrEmpty(mobj))
            {
                return "100000";
            }
            else
            {
                return (Convert.ToInt32(mobj) + 1).ToString();
            }
        }

        public ActionResult CheckPeriod(string FromDate)
        {

            if (String.IsNullOrEmpty(FromDate))
            {
                return Json(new { Status = "Error", Message = "Please Select From Date..!", JsonRequestBehavior.AllowGet });
            }
            else
            {
                DateTime FromDt = ConvertDDMMYYTOYYMMDD(FromDate);
                string Status = "Success", Message = "";
                if (ConvertDDMMYYTOYYMMDD(StartDate) <= FromDt && FromDt <= ConvertDDMMYYTOYYMMDD(EndDate))
                {

                    Status = "Success";

                }
                //else
                //{
                //    Status = "Error";
                //    Message = "From Date Allow Financial Range Only";
                //}
                return Json(new { Status = Status, Message = Message, JsonRequestBehavior.AllowGet });
            }
        }

        // GET: Vehicles/TripChargesMaster
        public ActionResult Index(TripChargesMaVM Model)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0,"", "", "NA");
            if (Model.Document == null)
            {
                Model.Document = "";
            }
            moptioncode = Model.OptionCode;
            msubcodeof = Model.ViewDataId;
            mmodule = Model.Module;
            if (Model.Mode == "Edit" || Model.Mode == "View" || Model.Mode == "Delete")
            {
                var mList = ctxTFAT.TripChargesMa.Where(x => x.DocNo == Model.Document).FirstOrDefault();
                if (mList != null)
                {
                    Model.FromDate = mList.FromPeriod.ToShortDateString();
                    Model.VehicleType = mList.VehicleType;
                    Model.Vehicle = mList.Vehicle;
                    Model.BranchType = mList.BranchType;
                    Model.Branch = mList.FromBranch;

                    Model.BranchName = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Branch).Select(x => x.Name).FirstOrDefault();
                    Model.VehicleN = ctxTFAT.VehicleCategory.Where(x => x.Code == Model.Vehicle).Select(x => x.VehicleCategory1).FirstOrDefault();
                    if (String.IsNullOrEmpty(Model.VehicleN))
                    {
                        Model.VehicleN = ctxTFAT.VehicleMaster.Where(x => x.Code == Model.Vehicle).Select(x => x.TruckNo).FirstOrDefault();
                    }

                    var List = ctxTFAT.TripChargesMaRef.Where(x => x.DocNo == Model.Document).ToList();
                    var Mobj = (from TfatBranch in ctxTFAT.TfatBranch
                                where TfatBranch.Code != "G00000" && TfatBranch.Grp != "G00000" && TfatBranch.Status == true
                                orderby TfatBranch.RECORDKEY
                                select new TripChargesMaVM
                                {
                                    Area = TfatBranch.Name,
                                    Trip = 0,
                                    Advance = 0,
                                    Category = TfatBranch.Category,
                                    ID = TfatBranch.Code,
                                    GRP = TfatBranch.Grp,
                                }).ToList();


                    var Mobj1 = (from TfatBranch in Mobj
                                 select new TripChargesMaVM
                                 {
                                     Area = TfatBranch.Area,
                                     Trip = List.Where(x => x.ToBranch == TfatBranch.ID).Select(x => x.TripCharge).FirstOrDefault() ?? 0,
                                     Advance = List.Where(x => x.ToBranch == TfatBranch.ID).Select(x => x.TripAdvance).FirstOrDefault() ?? 0,
                                     Category = TfatBranch.Category,
                                     ID = TfatBranch.ID,
                                     GRP = TfatBranch.GRP,
                                 }).ToList();




                    List<TripChargesMaVM> tripCharges = new List<TripChargesMaVM>();
                    foreach (var item in Mobj1)
                    {
                        if (item.Category == "0" || item.Category == "Zone")
                        {
                            if (item.Category == "0")
                            {
                                item.Area = item.Area + " -HO";
                            }
                            else
                            {
                                item.Area = item.Area + " -Z";
                            }
                            tripCharges.Add(item);
                        }
                        else if (item.Category == "Branch")
                        {
                            if (item.ID != Model.Branch)
                            {
                                item.Area = item.Area + " -B";
                                tripCharges.Add(item);
                            }
                            else
                            {
                                item.Area = item.Area + " -B";
                                item.Category = "OwnB";
                                tripCharges.Add(item);
                            }
                            var Area = Mobj1.Where(x => x.GRP == item.ID && x.Category == "Area" && x.ID != Model.Branch).ToList();
                            Area.ToList().ForEach(i => i.Area = i.Area + " -A");

                            tripCharges.AddRange(Area);
                            var SubBranch = Mobj1.Where(x => x.GRP == item.ID && x.Category == "SubBranch").ToList();
                            foreach (var item1 in SubBranch)
                            {
                                if (item1.ID != Model.Branch)
                                {
                                    item1.Area = item1.Area + " -SB";
                                    tripCharges.Add(item1);
                                }
                                else
                                {
                                    item1.Area = item1.Area + " -SB";
                                    item1.Category = "OwnSB";
                                    tripCharges.Add(item1);
                                }
                                var Area1 = Mobj1.Where(x => x.GRP == item1.ID && x.Category == "Area" && x.ID != Model.Branch).ToList();
                                Area1.ToList().ForEach(i => i.Area = i.Area + " -A");
                                Area1.ToList().ForEach(i => i.ParentOfID = item.ID);
                                tripCharges.AddRange(Area1);
                            }
                        }
                    }

                    Model.TripCharges = tripCharges;
                }
            }
            else
            {
                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                //Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();//GetEffectiveDate().Date.ToString();
            }
            return View(Model);
        }

        public ActionResult TripCharges(TripChargesMaVM Model)
        {
            Session["TripCharges"] = null;
            List<TripChargesMaVM> tripCharges = new List<TripChargesMaVM>();
            Model.BranchName = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Branch).Select(x => x.Name).FirstOrDefault();
            Model.VehicleN = ctxTFAT.VehicleCategory.Where(x => x.Code == Model.Vehicle).Select(x => x.VehicleCategory1).FirstOrDefault();
            if (String.IsNullOrEmpty(Model.VehicleN))
            {
                Model.VehicleN = Model.Vehicle;
            }

            var Statdate = ConvertDDMMYYTOYYMMDD(Model.FromDate);
            var CheckExist = ctxTFAT.TripChargesMa.Where(x => x.DocNo != Model.Document && x.VehicleType == Model.VehicleType && x.Vehicle == Model.Vehicle && x.FromBranch == Model.Branch && x.FromPeriod == Statdate).ToList();

            string Status = "Success", Message = "", Html = "";
            if (CheckExist.Count() > 0)
            {
                Status = "Error";
                Message = "Based On Your Data We Found Trip So U Can Not Create Duplicate Trip...!";
            }
            else
            {
                bool NewTrip = true;
                var GetTripMaster = ctxTFAT.TripChargesMa.Where(x => x.DocNo == Model.Document).FirstOrDefault();
                if (GetTripMaster != null)
                {
                    if (Model.Vehicle == GetTripMaster.Vehicle && Model.Branch == GetTripMaster.FromBranch)
                    {
                        NewTrip = false;
                    }
                }

                if (Model.copy)
                {
                    var list = Model.TripCharges.Select(x => x.ID).ToList();
                    var List = ctxTFAT.TripChargesMaRef.Where(x => x.DocNo == Model.copyDocument && list.Contains(x.ToBranch)).ToList();
                    var Mobj = (from TfatBranch in ctxTFAT.TfatBranch
                                where TfatBranch.Code != "G00000" && TfatBranch.Grp != "G00000"
                                orderby TfatBranch.RECORDKEY
                                select new TripChargesMaVM
                                {
                                    Area = TfatBranch.Name,
                                    Trip = 0,
                                    Advance = 0,
                                    Category = TfatBranch.Category,
                                    ID = TfatBranch.Code,
                                    GRP = TfatBranch.Grp,
                                }).ToList();
                    var Mobj1 = (from TfatBranch in Mobj
                                 select new TripChargesMaVM
                                 {
                                     Area = TfatBranch.Area,
                                     Trip = List.Where(x => x.ToBranch == TfatBranch.ID).Select(x => x.TripCharge).FirstOrDefault() ?? 0,
                                     Advance = List.Where(x => x.ToBranch == TfatBranch.ID).Select(x => x.TripAdvance).FirstOrDefault() ?? 0,
                                     Category = TfatBranch.Category,
                                     ID = TfatBranch.ID,
                                     GRP = TfatBranch.GRP,
                                 }).ToList();
                    foreach (var item in Mobj1)
                    {
                        if (item.Category == "0" || item.Category == "Zone")
                        {
                            if (item.Category == "0")
                            {
                                item.Area = item.Area + " -HO";
                            }
                            else
                            {
                                item.Area = item.Area + " -Z";
                            }
                            tripCharges.Add(item);
                        }
                        else if (item.Category == "Branch")
                        {
                            if (item.ID != Model.Branch)
                            {
                                item.Area = item.Area + " -B";
                                tripCharges.Add(item);
                            }
                            else
                            {
                                item.Area = item.Area + " -B";
                                item.Category = "OwnB";
                                tripCharges.Add(item);
                            }
                            var Area = Mobj1.Where(x => x.GRP == item.ID && x.Category == "Area" && x.ID != Model.Branch).ToList();
                            Area.ToList().ForEach(i => i.Area = i.Area + " -A");

                            tripCharges.AddRange(Area);
                            var SubBranch = Mobj1.Where(x => x.GRP == item.ID && x.Category == "SubBranch").ToList();
                            foreach (var item1 in SubBranch)
                            {
                                if (item1.ID != Model.Branch)
                                {
                                    item1.Area = item1.Area + " -SB";
                                    tripCharges.Add(item1);
                                }
                                else
                                {
                                    item1.Area = item1.Area + " -SB";
                                    item1.Category = "OwnSB";
                                    tripCharges.Add(item1);
                                }
                                var Area1 = Mobj1.Where(x => x.GRP == item1.ID && x.Category == "Area" && x.ID != Model.Branch).ToList();
                                Area1.ToList().ForEach(i => i.Area = i.Area + " -A");
                                Area1.ToList().ForEach(i => i.ParentOfID = item.ID);
                                tripCharges.AddRange(Area1);
                            }
                        }
                    }
                }
                else
                {
                    if (NewTrip)
                    {
                        var Mobj = (from TfatBranch in ctxTFAT.TfatBranch
                                    where TfatBranch.Code != "G00000" && TfatBranch.Grp != "G00000" && TfatBranch.Status == true
                                    orderby TfatBranch.RECORDKEY
                                    select new TripChargesMaVM
                                    {
                                        Area = TfatBranch.Name,
                                        Trip = 0,
                                        Advance = 0,
                                        Category = TfatBranch.Category,
                                        ID = TfatBranch.Code,
                                        GRP = TfatBranch.Grp,
                                    }).ToList();
                        foreach (var item in Mobj)
                        {
                            if (item.Category == "0" || item.Category == "Zone")
                            {
                                if (item.Category == "0")
                                {
                                    item.Area = item.Area + " -HO";
                                }
                                else
                                {
                                    item.Area = item.Area + " -Z";
                                }
                                tripCharges.Add(item);
                            }
                            else if (item.Category == "Branch")
                            {
                                if (item.ID != Model.Branch)
                                {
                                    item.Area = item.Area + " -B";
                                    tripCharges.Add(item);
                                }
                                else
                                {
                                    item.Area = item.Area + " -B";
                                    item.Category = "OwnB";
                                    tripCharges.Add(item);
                                }
                                var Area = Mobj.Where(x => x.GRP == item.ID && x.Category == "Area" && x.ID != Model.Branch).ToList();
                                Area.ToList().ForEach(i => i.Area = i.Area + " -A");

                                tripCharges.AddRange(Area);
                                var SubBranch = Mobj.Where(x => x.GRP == item.ID && x.Category == "SubBranch").ToList();
                                foreach (var item1 in SubBranch)
                                {
                                    if (item1.ID != Model.Branch)
                                    {
                                        item1.Area = item1.Area + " -SB";
                                        tripCharges.Add(item1);
                                    }
                                    else
                                    {
                                        item1.Area = item1.Area + " -SB";
                                        item1.Category = "OwnSB";
                                        tripCharges.Add(item1);
                                    }
                                    var Area1 = Mobj.Where(x => x.GRP == item1.ID && x.Category == "Area" && x.ID != Model.Branch).ToList();
                                    Area1.ToList().ForEach(i => i.Area = i.Area + " -A");
                                    Area1.ToList().ForEach(i => i.ParentOfID = item.ID);
                                    tripCharges.AddRange(Area1);
                                }
                            }
                        }
                    }
                    else
                    {
                        var List = ctxTFAT.TripChargesMaRef.Where(x => x.DocNo == Model.Document).ToList();
                        var Mobj = (from TfatBranch in ctxTFAT.TfatBranch
                                    where TfatBranch.Code != "G00000" && TfatBranch.Grp != "G00000" && TfatBranch.Status == true
                                    orderby TfatBranch.RECORDKEY
                                    select new TripChargesMaVM
                                    {
                                        Area = TfatBranch.Name,
                                        Trip = 0,
                                        Advance = 0,
                                        Category = TfatBranch.Category,
                                        ID = TfatBranch.Code,
                                        GRP = TfatBranch.Grp,
                                    }).ToList();
                        var Mobj1 = (from TfatBranch in Mobj
                                     select new TripChargesMaVM
                                     {
                                         Area = TfatBranch.Area,
                                         Trip = List.Where(x => x.ToBranch == TfatBranch.ID).Select(x => x.TripCharge).FirstOrDefault() ?? 0,
                                         Advance = List.Where(x => x.ToBranch == TfatBranch.ID).Select(x => x.TripAdvance).FirstOrDefault() ?? 0,
                                         Category = TfatBranch.Category,
                                         ID = TfatBranch.ID,
                                         GRP = TfatBranch.GRP,
                                     }).ToList();
                        foreach (var item in Mobj1)
                        {
                            if (item.Category == "0" || item.Category == "Zone")
                            {
                                if (item.Category == "0")
                                {
                                    item.Area = item.Area + " -HO";
                                }
                                else
                                {
                                    item.Area = item.Area + " -Z";
                                }
                                tripCharges.Add(item);
                            }
                            else if (item.Category == "Branch")
                            {
                                if (item.ID != Model.Branch)
                                {
                                    item.Area = item.Area + " -B";
                                    tripCharges.Add(item);
                                }
                                else
                                {
                                    item.Area = item.Area + " -B";
                                    item.Category = "OwnB";
                                    tripCharges.Add(item);
                                }
                                var Area = Mobj1.Where(x => x.GRP == item.ID && x.Category == "Area" && x.ID != Model.Branch).ToList();
                                Area.ToList().ForEach(i => i.Area = i.Area + " -A");

                                tripCharges.AddRange(Area);
                                var SubBranch = Mobj1.Where(x => x.GRP == item.ID && x.Category == "SubBranch").ToList();
                                foreach (var item1 in SubBranch)
                                {
                                    if (item1.ID != Model.Branch)
                                    {
                                        item1.Area = item1.Area + " -SB";
                                        tripCharges.Add(item1);
                                    }
                                    else
                                    {
                                        item1.Area = item1.Area + " -SB";
                                        item1.Category = "OwnSB";
                                        tripCharges.Add(item1);
                                    }
                                    var Area1 = Mobj1.Where(x => x.GRP == item1.ID && x.Category == "Area" && x.ID != Model.Branch).ToList();
                                    Area1.ToList().ForEach(i => i.Area = i.Area + " -A");
                                    Area1.ToList().ForEach(i => i.ParentOfID = item.ID);
                                    tripCharges.AddRange(Area1);
                                }
                            }
                        }
                    }
                }


                Status = "Success";
            }
            Model.TripCharges = tripCharges;
            Session["TripCharges"] = tripCharges;
            var html = ViewHelper.RenderPartialView(this, "TripChargesPartial", Model);
            return Json(new { Status = Status, Message = Message, Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveData(TripChargesMaVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteTripMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    var Statdate = ConvertDDMMYYTOYYMMDD(mModel.FromDate);
                    var CheckExist = ctxTFAT.TripChargesMa.Where(x => x.DocNo != mModel.Document && x.VehicleType == mModel.VehicleType && x.Vehicle == mModel.Vehicle && x.FromBranch == mModel.Branch && x.FromPeriod == Statdate).ToList();

                    
                    if (CheckExist.Count() > 0)
                    {
                        string Status = "Success", Message = "";
                        Status = "Error";
                        Message = "Based On Your Data We Found Trip So U Can Not Create Duplicate Trip...!";
                        return Json(new { Status = "Error", Message = Message }, JsonRequestBehavior.AllowGet);
                    }



                    TripChargesMa mobj = new TripChargesMa();


                    bool mAdd = true;
                    if (ctxTFAT.TripChargesMa.Where(x => x.DocNo == mModel.Document).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.TripChargesMa.Where(x => x.DocNo == mModel.Document).FirstOrDefault();
                        mAdd = false;
                        DeUpdate(mModel);
                    }
                    if (mAdd)
                    {
                        mobj.DocNo = GetCode();
                    }
                    mobj.DocDate = DateTime.Now;
                    mobj.FromPeriod = ConvertDDMMYYTOYYMMDD(mModel.FromDate);
                    mobj.VehicleType = mModel.VehicleType;
                    mobj.Vehicle = mModel.Vehicle;
                    mobj.BranchType = mModel.BranchType;
                    mobj.FromBranch = mModel.Branch;
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;


                    foreach (var item in mModel.TripCharges)
                    {

                        if (item.Trip != 0 || item.Advance != 0 )
                        {
                            TripChargesMaRef advRef = new TripChargesMaRef();
                            advRef.DocNo = mobj.DocNo;
                            advRef.FromBranch = mobj.FromBranch;
                            advRef.ToBranch = item.ID;
                            advRef.TripAdvance = item.Advance;
                            advRef.TripCharge = item.Trip;
                            advRef.AUTHIDS = muserid;
                            advRef.AUTHORISE = mauthorise;
                            advRef.ENTEREDBY = muserid;
                            advRef.LASTUPDATEDATE = System.DateTime.Now;
                            ctxTFAT.TripChargesMaRef.Add(advRef);
                        }
                    }
                    if (mAdd == true)
                    {
                        ctxTFAT.TripChargesMa.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    string mNewCode = "";
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.DocNo, DateTime.Now, 0, mobj.DocNo, "Save Driver Trip Charges Master", "NA");
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

        public void DeUpdate(TripChargesMaVM Model)
        {
            var BillSubRef = ctxTFAT.TripChargesMaRef.Where(x => x.DocNo == Model.Document).ToList();
            ctxTFAT.TripChargesMaRef.RemoveRange(BillSubRef);
            ctxTFAT.SaveChanges();
        }

        public ActionResult DeleteTripMaster(TripChargesMaVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var mList = ctxTFAT.TripChargesMa.Where(x => (x.DocNo == mModel.Document)).FirstOrDefault();
            ctxTFAT.TripChargesMa.Remove(mList);
            var BillSubRef = ctxTFAT.TripChargesMaRef.Where(x => x.DocNo == mModel.Document).ToList();
            ctxTFAT.TripChargesMaRef.RemoveRange(BillSubRef);
            ctxTFAT.SaveChanges();
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mList.DocNo, DateTime.Now, 0, mList.DocNo, "Delete Driver Trip Charges Master", "NA");

            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Copy(string DocNo)
        {
            List<CopyTripVM> List = new List<CopyTripVM>();

            var Mobj = (from TripCharges in ctxTFAT.TripChargesMa
                        where TripCharges.DocNo != DocNo
                        select new CopyTripVM
                        {
                            DocumentNo = TripCharges.DocNo,
                            FromDate = TripCharges.FromPeriod,
                            Vehicle = ctxTFAT.VehicleCategory.Where(x => x.Code == TripCharges.Vehicle).Select(x => x.VehicleCategory1).FirstOrDefault() ?? TripCharges.Vehicle,
                            FromBranch = ctxTFAT.TfatBranch.Where(x => x.Code == TripCharges.FromBranch).Select(x => x.Name).FirstOrDefault()
                        }).ToList();
            if (Mobj == null)
            {
                Mobj = new List<CopyTripVM>();
            }
            var html = ViewHelper.RenderPartialView(this, "ListOFTripCharges", Mobj);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }
    }
}