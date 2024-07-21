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
    public class ViaFreightMasterController : BaseController
    {
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        private int mnewrecordkey = 0;
        private static string mauthorise = "A00";
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

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
            var mobj = ctxTFAT.ViaFreightMa.OrderByDescending(x => x.RECORDKEY).Select(x => x.DocNo).FirstOrDefault();
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

        // GET: Vehicles/ViaFreightMaster
        public ActionResult Index(ViaFreightMaVM Model)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "NA");
            if (Model.Document == null)
            {
                Model.Document = "";
            }
            moptioncode = Model.OptionCode;
            msubcodeof = Model.ViewDataId;
            mmodule = Model.Module;
            if (Model.Mode == "Edit" || Model.Mode == "View" || Model.Mode == "Delete")
            {
                var mList = ctxTFAT.ViaFreightMa.Where(x => x.DocNo == Model.Document).FirstOrDefault();
                if (mList != null)
                {
                    Model.FromDate = mList.FromPeriod.ToShortDateString();
                    Model.VehicleType = mList.VehicleType;
                    Model.Vehicle = mList.Vehicle;
                    Model.VehicleN = ctxTFAT.VehicleCategory.Where(x => x.Code == Model.Vehicle).Select(x => x.VehicleCategory1).FirstOrDefault();
                    if (String.IsNullOrEmpty(Model.VehicleN))
                    {
                        Model.VehicleN = ctxTFAT.VehicleMaster.Where(x => x.Code == Model.Vehicle).Select(x => x.TruckNo).FirstOrDefault();
                    }

                    var List = ctxTFAT.ViaFreightMaRef.Where(x => x.DocNo == Model.Document).ToList();
                    var Mobj = (from TfatBranch in ctxTFAT.TfatBranch
                                where TfatBranch.Code != "G00000" && TfatBranch.Grp != "G00000"
                                orderby TfatBranch.RECORDKEY
                                select new ViaFreightMaVM
                                {
                                    Area = TfatBranch.Name,
                                    //ExraFreight = 0,
                                    ViaFreight = 0,
                                    ViaAdvance = 0,
                                    //LocalTime = "00:00",
                                    //LocalKM = "0",
                                    Category = TfatBranch.Category,
                                    ID = TfatBranch.Code,
                                    GRP = TfatBranch.Grp,
                                }).ToList();


                    var Mobj1 = (from TfatBranch in Mobj
                                 select new ViaFreightMaVM
                                 {
                                     Area = TfatBranch.Area,
                                     //ExraFreight = List.Where(x => x.Area == TfatBranch.ID).Select(x => x.LocalFreight).FirstOrDefault() ?? 0,
                                     ViaFreight = List.Where(x => x.Area == TfatBranch.ID).Select(x => x.ViaFreight).FirstOrDefault() ?? 0,
                                     ViaAdvance = List.Where(x => x.Area == TfatBranch.ID).Select(x => x.ViaAdvance).FirstOrDefault() ?? 0,
                                     //LocalTime = List.Where(x => x.Area == TfatBranch.ID).Select(x => x.LocalTime).FirstOrDefault() ?? "00:00",
                                     //LocalKM = List.Where(x => x.Area == TfatBranch.ID).Select(x => x.LocalKM).FirstOrDefault() ?? "0",
                                     Category = TfatBranch.Category,
                                     ID = TfatBranch.ID,
                                     GRP = TfatBranch.GRP,
                                 }).ToList();




                    List<ViaFreightMaVM> tripCharges = new List<ViaFreightMaVM>();
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

                            item.Area = item.Area + " -B";
                            tripCharges.Add(item);

                            var Area = Mobj1.Where(x => x.GRP == item.ID && x.Category == "Area").ToList();
                            Area.ToList().ForEach(i => i.Area = i.Area + " -A");

                            tripCharges.AddRange(Area);
                            var SubBranch = Mobj1.Where(x => x.GRP == item.ID && x.Category == "SubBranch").ToList();
                            foreach (var item1 in SubBranch)
                            {

                                item1.Area = item1.Area + " -SB";
                                tripCharges.Add(item1);


                                var Area1 = Mobj1.Where(x => x.GRP == item1.ID && x.Category == "Area").ToList();
                                Area1.ToList().ForEach(i => i.Area = i.Area + " -A");
                                Area1.ToList().ForEach(i => i.ParentOfID = item.ID);
                                tripCharges.AddRange(Area1);
                            }
                        }
                    }

                    Model.list = tripCharges;
                }
            }
            else
            {
                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                //Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();//GetEffectiveDate().Date.ToString();

            }
            return View(Model);
        }

        public ActionResult Via_LocalExp(ViaFreightMaVM Model)
        {
            Session["ViaFreight"] = null;
            List<ViaFreightMaVM> tripCharges = new List<ViaFreightMaVM>();

            var Statdate = ConvertDDMMYYTOYYMMDD(Model.FromDate);

            Model.VehicleN = ctxTFAT.VehicleCategory.Where(x => x.Code == Model.Vehicle).Select(x => x.VehicleCategory1).FirstOrDefault();
            if (String.IsNullOrEmpty(Model.VehicleN))
            {
                Model.VehicleN = Model.Vehicle;
            }
            int count = 0;
            if (Model.copy == false)
            {
                var CheckExist = ctxTFAT.ViaFreightMa.Where(x => x.DocNo != Model.Document && x.Vehicle == Model.Vehicle && x.FromPeriod == Statdate).ToList();
                count = CheckExist.Count();
            }

            string Status = "Success", Message = "", Html = "";
            if (count > 0)
            {
                Status = "Error";
                Message = "Based On Your Data We Found Master So U Can Not Create Duplicate Master...!";
            }
            else
            {
                bool NewTrip = true;
                var GetTripMaster = ctxTFAT.ViaFreightMa.Where(x => x.DocNo == Model.Document).FirstOrDefault();
                if (GetTripMaster != null)
                {
                    if (GetTripMaster.FromPeriod.ToShortDateString() == Statdate.ToShortDateString())
                    {
                        NewTrip = false;
                    }
                }

                if (Model.copy)
                {
                    var list = Model.list.Select(x => x.ID).ToList();
                    var List = ctxTFAT.ViaFreightMaRef.Where(x => x.DocNo == Model.copyDocument && list.Contains(x.Area)).ToList();
                    var Mobj = (from TfatBranch in ctxTFAT.TfatBranch
                                where TfatBranch.Code != "G00000" && TfatBranch.Grp != "G00000" && TfatBranch.Status == true
                                orderby TfatBranch.RECORDKEY
                                select new ViaFreightMaVM
                                {
                                    Area = TfatBranch.Name,
                                    //ExraFreight = 0,
                                    ViaFreight = 0,
                                    ViaAdvance = 0,
                                    //LocalTime = "00:00",
                                    //LocalKM = "0",
                                    Category = TfatBranch.Category,
                                    ID = TfatBranch.Code,
                                    GRP = TfatBranch.Grp,
                                }).ToList();
                    var Mobj1 = (from TfatBranch in Mobj
                                 select new ViaFreightMaVM
                                 {
                                     Area = TfatBranch.Area,
                                     //ExraFreight = List.Where(x => x.Area == TfatBranch.ID).Select(x => x.LocalFreight).FirstOrDefault() ?? 0,
                                     ViaFreight = List.Where(x => x.Area == TfatBranch.ID).Select(x => x.ViaFreight).FirstOrDefault() ?? 0,
                                     ViaAdvance = List.Where(x => x.Area == TfatBranch.ID).Select(x => x.ViaAdvance).FirstOrDefault() ?? 0,
                                     //LocalTime = List.Where(x => x.Area == TfatBranch.ID).Select(x => x.LocalTime).FirstOrDefault() ?? "00:00",
                                     //LocalKM = List.Where(x => x.Area == TfatBranch.ID).Select(x => x.LocalKM).FirstOrDefault() ?? "0",
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
                            item.Area = item.Area + " -B";
                            tripCharges.Add(item);

                            var Area = Mobj1.Where(x => x.GRP == item.ID && x.Category == "Area").ToList();
                            Area.ToList().ForEach(i => i.Area = i.Area + " -A");

                            tripCharges.AddRange(Area);
                            var SubBranch = Mobj1.Where(x => x.GRP == item.ID && x.Category == "SubBranch").ToList();
                            foreach (var item1 in SubBranch)
                            {
                                item1.Area = item1.Area + " -SB";
                                tripCharges.Add(item1);

                                var Area1 = Mobj1.Where(x => x.GRP == item1.ID && x.Category == "Area").ToList();
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
                                    where TfatBranch.Code != "G00000" && TfatBranch.Grp != "G00000"
                                    orderby TfatBranch.RECORDKEY
                                    select new ViaFreightMaVM
                                    {
                                        Area = TfatBranch.Name,
                                        //ExraFreight = 0,
                                        ViaFreight = 0,
                                        ViaAdvance = 0,
                                        //LocalTime = "00:00",
                                        //LocalKM = "0",
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
                                item.Area = item.Area + " -B";
                                tripCharges.Add(item);

                                var Area = Mobj.Where(x => x.GRP == item.ID && x.Category == "Area").ToList();
                                Area.ToList().ForEach(i => i.Area = i.Area + " -A");

                                tripCharges.AddRange(Area);
                                var SubBranch = Mobj.Where(x => x.GRP == item.ID && x.Category == "SubBranch").ToList();
                                foreach (var item1 in SubBranch)
                                {
                                    item1.Area = item1.Area + " -SB";
                                    tripCharges.Add(item1);

                                    var Area1 = Mobj.Where(x => x.GRP == item1.ID && x.Category == "Area").ToList();
                                    Area1.ToList().ForEach(i => i.Area = i.Area + " -A");
                                    Area1.ToList().ForEach(i => i.ParentOfID = item.ID);
                                    tripCharges.AddRange(Area1);
                                }
                            }
                        }
                    }
                    else
                    {
                        var List = ctxTFAT.ViaFreightMaRef.Where(x => x.DocNo == Model.Document).ToList();
                        var Mobj = (from TfatBranch in ctxTFAT.TfatBranch
                                    where TfatBranch.Code != "G00000" && TfatBranch.Grp != "G00000" && TfatBranch.Status == true
                                    orderby TfatBranch.RECORDKEY
                                    select new ViaFreightMaVM
                                    {
                                        Area = TfatBranch.Name,
                                        //ExraFreight = 0,
                                        ViaFreight = 0,
                                        ViaAdvance = 0,
                                        //LocalTime = "00:00",
                                        //LocalKM = "0",
                                        Category = TfatBranch.Category,
                                        ID = TfatBranch.Code,
                                        GRP = TfatBranch.Grp,
                                    }).ToList();
                        var Mobj1 = (from TfatBranch in Mobj
                                     select new ViaFreightMaVM
                                     {
                                         Area = TfatBranch.Area,
                                         //ExraFreight = List.Where(x => x.Area == TfatBranch.ID).Select(x => x.LocalFreight).FirstOrDefault() ?? 0,
                                         ViaFreight = List.Where(x => x.Area == TfatBranch.ID).Select(x => x.ViaFreight).FirstOrDefault() ?? 0,
                                         ViaAdvance = List.Where(x => x.Area == TfatBranch.ID).Select(x => x.ViaAdvance).FirstOrDefault() ?? 0,
                                         //LocalTime = List.Where(x => x.Area == TfatBranch.ID).Select(x => x.LocalTime).FirstOrDefault() ?? "00:00",
                                         //LocalKM = List.Where(x => x.Area == TfatBranch.ID).Select(x => x.LocalKM).FirstOrDefault() ?? "0",
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
                                item.Area = item.Area + " -B";
                                tripCharges.Add(item);

                                var Area = Mobj1.Where(x => x.GRP == item.ID && x.Category == "Area").ToList();
                                Area.ToList().ForEach(i => i.Area = i.Area + " -A");

                                tripCharges.AddRange(Area);
                                var SubBranch = Mobj1.Where(x => x.GRP == item.ID && x.Category == "SubBranch").ToList();
                                foreach (var item1 in SubBranch)
                                {
                                    item1.Area = item1.Area + " -SB";
                                    tripCharges.Add(item1);

                                    var Area1 = Mobj1.Where(x => x.GRP == item1.ID && x.Category == "Area").ToList();
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
            Model.list = tripCharges;
            Session["ViaFreight"] = tripCharges;
            var html = ViewHelper.RenderPartialView(this, "ViaFreightPartial", Model);
            return Json(new { Status = Status, Message = Message, Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveData(ViaFreightMaVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        var Json = DeleteTripMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Json;
                    }
                    ViaFreightMa mobj = new ViaFreightMa();
                    var Statdate = ConvertDDMMYYTOYYMMDD(mModel.FromDate);
                    var CheckExist = ctxTFAT.ViaFreightMa.Where(x => x.DocNo != mModel.Document && x.Vehicle == mModel.Vehicle && x.FromPeriod == Statdate).ToList();
                    if (CheckExist.Count() > 0)
                    {
                        return Json(new { Message = "Based On Your Data We Found Trip So U Can Not Create Duplicate Trip...!", Status = "Error", id = "CountryMaster" }, JsonRequestBehavior.AllowGet);
                    }

                    bool mAdd = true;
                    if (ctxTFAT.ViaFreightMa.Where(x => x.DocNo == mModel.Document).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.ViaFreightMa.Where(x => x.DocNo == mModel.Document).FirstOrDefault();
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
                    mobj.Branch = mbranchcode;
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;


                    foreach (var item in mModel.list)
                    {

                        if (item.ViaFreight != 0 || item.ViaAdvance != 0)
                        {
                            ViaFreightMaRef advRef = new ViaFreightMaRef();
                            advRef.DocNo = mobj.DocNo;
                            advRef.Area = item.ID;
                            //////advRef.LocalFreight = item.ExraFreight;
                            advRef.ViaFreight = item.ViaFreight;
                            advRef.ViaAdvance = item.ViaAdvance;
                            //advRef.LocalTime = item.LocalTime;
                            //advRef.LocalKM = item.LocalKM;
                            advRef.AUTHIDS = muserid;
                            advRef.AUTHORISE = mauthorise;
                            advRef.ENTEREDBY = muserid;
                            advRef.LASTUPDATEDATE = System.DateTime.Now;
                            ctxTFAT.ViaFreightMaRef.Add(advRef);
                        }
                    }
                    if (mAdd == true)
                    {
                        ctxTFAT.ViaFreightMa.Add(mobj);
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
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.DocNo, DateTime.Now, 0, mobj.DocNo, "Save Freight Via Charges Master", "NA");
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

        public void DeUpdate(ViaFreightMaVM Model)
        {
            var BillSubRef = ctxTFAT.ViaFreightMaRef.Where(x => x.DocNo == Model.Document).ToList();
            ctxTFAT.ViaFreightMaRef.RemoveRange(BillSubRef);
            ctxTFAT.SaveChanges();
        }

        public ActionResult DeleteTripMaster(ViaFreightMaVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var mList = ctxTFAT.ViaFreightMa.Where(x => (x.DocNo == mModel.Document)).FirstOrDefault();
            ctxTFAT.ViaFreightMa.Remove(mList);
            var BillSubRef = ctxTFAT.ViaFreightMaRef.Where(x => x.DocNo == mModel.Document).ToList();
            ctxTFAT.ViaFreightMaRef.RemoveRange(BillSubRef);
            ctxTFAT.SaveChanges();
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mList.DocNo, DateTime.Now, 0, mList.DocNo, "Delete Freight Via Charges Master", "NA");

            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Copy(string DocNo)
        {
            var Mobj = (from ViaCharges in ctxTFAT.ViaFreightMa
                        where ViaCharges.DocNo != DocNo
                        select new CopyViaFreightVM
                        {
                            DocumentNo = ViaCharges.DocNo,
                            FromDate = ViaCharges.FromPeriod,
                            Vehicle = ctxTFAT.VehicleCategory.Where(x => x.Code == ViaCharges.Vehicle).Select(x => x.VehicleCategory1).FirstOrDefault() ?? ViaCharges.Vehicle,
                        }).ToList();
            if (Mobj == null)
            {
                Mobj = new List<CopyViaFreightVM>();
            }
            var html = ViewHelper.RenderPartialView(this, "ListOFViaFreight", Mobj);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }
    }
}