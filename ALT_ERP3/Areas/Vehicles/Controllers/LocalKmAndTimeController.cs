﻿using ALT_ERP3.Areas.Vehicles.Models;
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
    public class LocalKmAndTimeController : BaseController
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
                else if (item.Category == "SubBranch")
                {
                    item.Name += " - SB";
                    treeTables.Add(item);
                }else if (item.Category == "0")
                {
                    item.Name += " - HO";
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
        public string GetCode()
        {
            var mobj = ctxTFAT.LocalKMandTimeMa.OrderByDescending(x => x.RECORDKEY).Select(x => x.DocNo).FirstOrDefault();
            if (String.IsNullOrEmpty(mobj))
            {
                return "100000";
            }
            else
            {
                return (Convert.ToInt32(mobj) + 1).ToString();
            }
        }

        // GET: Vehicles/LocalKmAndTime
        public ActionResult Index(LocalKmAndTimeVM Model)
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
                var mList = ctxTFAT.LocalKMandTimeMa.Where(x => x.DocNo == Model.Document).FirstOrDefault();
                if (mList != null)
                {
                    Model.BranchType = mList.BranchType;
                    Model.Branch = mList.FromBranch;
                    Model.BranchName = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Branch).Select(x => x.Name).FirstOrDefault();

                    var List = ctxTFAT.LocalKMandTimeMaRef.Where(x => x.DocNo == Model.Document).ToList();
                    var Mobj = (from TfatBranch in ctxTFAT.TfatBranch
                                where TfatBranch.Code != "G00000" && TfatBranch.Grp != "G00000"
                                orderby TfatBranch.RECORDKEY
                                select new LocalKmAndTimeVM
                                {
                                    Area = TfatBranch.Name,
                                    KM = "0",
                                    Time = "00:00",
                                    Category = TfatBranch.Category,
                                    ID = TfatBranch.Code,
                                    GRP = TfatBranch.Grp,
                                }).ToList();


                    var Mobj1 = (from TfatBranch in Mobj
                                 select new LocalKmAndTimeVM
                                 {
                                     Area = TfatBranch.Area,
                                     KM = List.Where(x => x.ToBranch == TfatBranch.ID).Select(x => x.KM).FirstOrDefault() ?? "0",
                                     Time = List.Where(x => x.ToBranch == TfatBranch.ID).Select(x => x.ReportTime).FirstOrDefault() ?? "00:00",
                                     Category = TfatBranch.Category,
                                     ID = TfatBranch.ID,
                                     GRP = TfatBranch.GRP,
                                 }).ToList();




                    List<LocalKmAndTimeVM> tripCharges = new List<LocalKmAndTimeVM>();
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

                    Model.list = tripCharges;
                }
            }

            return View(Model);
        }

        public ActionResult Kilometerlist(LocalKmAndTimeVM Model)
        {
            List<LocalKmAndTimeVM> tripCharges = new List<LocalKmAndTimeVM>();
            Model.BranchName = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Branch).Select(x => x.Name).FirstOrDefault();

            var CheckExist = ctxTFAT.LocalKMandTimeMa.Where(x => x.DocNo != Model.Document && x.FromBranch == Model.Branch).ToList();

            string Status = "Success", Message = "", Html = "";
            if (CheckExist.Count() > 0)
            {
                Status = "Error";
                Message = "Based On Your Data We Found Kilometer Master So U Can Not Create Duplicate Master...!";
            }
            else
            {
                bool NewTrip = true;
                var GetTripMaster = ctxTFAT.LocalKMandTimeMa.Where(x => x.DocNo == Model.Document).FirstOrDefault();
                if (GetTripMaster != null)
                {
                    if (Model.Branch == GetTripMaster.FromBranch)
                    {
                        NewTrip = false;
                    }
                }

                if (Model.copy)
                {
                    var list = Model.list.Select(x => x.ID).ToList();
                    var List = ctxTFAT.LocalKMandTimeMaRef.Where(x => x.DocNo == Model.copyDocument && list.Contains(x.ToBranch)).ToList();
                    var Mobj = (from TfatBranch in ctxTFAT.TfatBranch
                                where TfatBranch.Code != "G00000" && TfatBranch.Grp != "G00000"
                                orderby TfatBranch.RECORDKEY
                                select new LocalKmAndTimeVM
                                {
                                    Area = TfatBranch.Name,
                                    KM = "0",
                                    Time = "00:00",
                                    Category = TfatBranch.Category,
                                    ID = TfatBranch.Code,
                                    GRP = TfatBranch.Grp,
                                }).ToList();
                    var Mobj1 = (from TfatBranch in Mobj
                                 select new LocalKmAndTimeVM
                                 {
                                     Area = TfatBranch.Area,
                                     KM = List.Where(x => x.ToBranch == TfatBranch.ID).Select(x => x.KM).FirstOrDefault() ?? "0",
                                     Time = List.Where(x => x.ToBranch == TfatBranch.ID).Select(x => x.ReportTime).FirstOrDefault() ?? "00:00",
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
                                    where TfatBranch.Code != "G00000" && TfatBranch.Grp != "G00000"
                                    orderby TfatBranch.RECORDKEY
                                    select new LocalKmAndTimeVM
                                    {
                                        Area = TfatBranch.Name,
                                        KM = "0",
                                        Time = "00:00",
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
                        var List = ctxTFAT.LocalKMandTimeMaRef.Where(x => x.DocNo == Model.Document).ToList();
                        var Mobj = (from TfatBranch in ctxTFAT.TfatBranch
                                    where TfatBranch.Code != "G00000" && TfatBranch.Grp != "G00000"
                                    orderby TfatBranch.RECORDKEY
                                    select new LocalKmAndTimeVM
                                    {
                                        Area = TfatBranch.Name,
                                        KM = "0",
                                        Time = "00:00",
                                        Category = TfatBranch.Category,
                                        ID = TfatBranch.Code,
                                        GRP = TfatBranch.Grp,
                                    }).ToList();
                        var Mobj1 = (from TfatBranch in Mobj
                                     select new LocalKmAndTimeVM
                                     {
                                         Area = TfatBranch.Area,
                                         KM = List.Where(x => x.ToBranch == TfatBranch.ID).Select(x => x.KM).FirstOrDefault() ?? "0",
                                         Time = List.Where(x => x.ToBranch == TfatBranch.ID).Select(x => x.ReportTime).FirstOrDefault() ?? "00:00",
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
            Model.list = tripCharges;
            var html = ViewHelper.RenderPartialView(this, "LocalKmAndTimeView", Model);
            return Json(new { Status = Status, Message = Message, Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveData(LocalKmAndTimeVM mModel)
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
                    LocalKMandTimeMa mobj = new LocalKMandTimeMa();


                    bool mAdd = true;
                    if (ctxTFAT.LocalKMandTimeMa.Where(x => x.DocNo == mModel.Document).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.LocalKMandTimeMa.Where(x => x.DocNo == mModel.Document).FirstOrDefault();
                        mAdd = false;
                        DeUpdate(mModel);
                    }
                    if (mAdd)
                    {
                        mobj.DocNo = GetCode();
                    }
                    mobj.DocDate = DateTime.Now;
                    mobj.BranchType = mModel.BranchType;
                    mobj.FromBranch = mModel.Branch;
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;


                    foreach (var item in mModel.list)
                    {

                        if (item.KM != "0" || item.Time != "00:00")
                        {
                            LocalKMandTimeMaRef advRef = new LocalKMandTimeMaRef();
                            advRef.DocNo = mobj.DocNo;
                            advRef.FromBranch = mobj.FromBranch;
                            advRef.ToBranch = item.ID;
                            advRef.KM = item.KM;
                            advRef.ReportTime = item.Time;
                            advRef.AUTHIDS = muserid;
                            advRef.AUTHORISE = mauthorise;
                            advRef.ENTEREDBY = muserid;
                            advRef.LASTUPDATEDATE = System.DateTime.Now;
                            ctxTFAT.LocalKMandTimeMaRef.Add(advRef);
                        }
                    }
                    if (mAdd == true)
                    {
                        ctxTFAT.LocalKMandTimeMa.Add(mobj);
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
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0,2)+ mobj.DocNo, DateTime.Now, 0, "", "Save Local Km", "NA");
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

        public ActionResult DeleteTripMaster(LocalKmAndTimeVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var mList = ctxTFAT.LocalKMandTimeMa.Where(x => (x.DocNo == mModel.Document)).FirstOrDefault();
            ctxTFAT.LocalKMandTimeMa.Remove(mList);
            var BillSubRef = ctxTFAT.LocalKMandTimeMaRef.Where(x => x.DocNo == mModel.Document).ToList();
            ctxTFAT.LocalKMandTimeMaRef.RemoveRange(BillSubRef);
            ctxTFAT.SaveChanges();
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mList.DocNo, DateTime.Now, 0, "", "Delete Local Km", "NA");

            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }

        public void DeUpdate(LocalKmAndTimeVM Model)
        {
            var BillSubRef = ctxTFAT.LocalKMandTimeMaRef.Where(x => x.DocNo == Model.Document).ToList();
            ctxTFAT.LocalKMandTimeMaRef.RemoveRange(BillSubRef);
            ctxTFAT.SaveChanges();
        }

        public ActionResult Copy(string DocNo)
        {
            List<LocalKMTimeCopyVM> List = new List<LocalKMTimeCopyVM>();

            var Mobj = (from LocalKMandTime in ctxTFAT.LocalKMandTimeMa
                        where LocalKMandTime.DocNo != DocNo
                        select new LocalKMTimeCopyVM
                        {
                            DocumentNo = LocalKMandTime.DocNo,
                            FromBranch = ctxTFAT.TfatBranch.Where(x => x.Code == LocalKMandTime.FromBranch).Select(x => x.Name).FirstOrDefault()
                        }).ToList();
            if (Mobj == null)
            {
                Mobj = new List<LocalKMTimeCopyVM>();
            }
            var html = ViewHelper.RenderPartialView(this, "ListOFLocalKMTime", Mobj);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }
    }
}