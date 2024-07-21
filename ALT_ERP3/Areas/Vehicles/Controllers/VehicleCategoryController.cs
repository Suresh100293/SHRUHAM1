using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Vehicles.Models;
using ALT_ERP3.Controllers;

namespace ALT_ERP3.Areas.Vehicles.Controllers
{
    public class VehicleCategoryController : BaseController
    {
         
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;
        // GET: Vehicles/VehicleCategory
        public ActionResult Index(VehicleCategoryVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());

            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "VCA");

            mdocument = mModel.Document;

            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.VehicleCategory.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                //var PartyCode = Convert.ToInt32(mList.Code);

                if (mList != null)
                {
                    mModel.Code = mList.Code;
                    mModel.VehicleCategory = mList.VehicleCategory1;
                    mModel.Acitve = mList.Acitve;
                }
            }
            else
            {
                mModel.Code = "";
                mModel.VehicleCategory = "";
                mModel.Acitve = true;
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(VehicleCategoryVM mModel)
        {
            
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        var Retur=DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Retur;
                    }
                    bool Status = false;
                    if (mModel.AcitveorNot.ToString() == "True")
                    {
                        Status = true;
                    }
                    VehicleCategory mobj = new VehicleCategory();
                    bool mAdd = true;
                    if (ctxTFAT.VehicleCategory.Where(x => (x.Code == mModel.Document)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.VehicleCategory.Where(x => (x.Code == mModel.Document)).FirstOrDefault();

                        mAdd = false;
                    }

                    //mobj.ShortName = mModel.ShortName;
                    mobj.VehicleCategory1 = mModel.VehicleCategory;
                    mobj.Acitve = Status;
                    //mobj.Country = mModel.TfatState_Country;
                    //// iX9: default values for the fields not used @Form
                    //mobj.StateCode = "";
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                    if (mAdd == true)
                    {
                        int NewCode1;
                        var NewCode = ctxTFAT.VehicleCategory.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).Take(1).FirstOrDefault();
                        if (NewCode == null || NewCode == "")
                        {
                            NewCode1 = 100000;
                        }
                        else
                        {
                            NewCode1 = Convert.ToInt32(NewCode) + 1;
                        }
                        string FinalCode = NewCode1.ToString("D6");
                        mobj.Code = FinalCode;
                        ctxTFAT.VehicleCategory.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = Convert.ToInt32(mobj.RECORDKEY);
                    string mNewCode = "";
                    mNewCode = mobj.Code;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.Code, DateTime.Now, 0, mobj.Code, "Save Vehicle Category Master",  "VCA");

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

        public ActionResult DeleteStateMaster(VehicleCategoryVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            if ( mModel.Document == "100000")
            {
                return Json(new
                {
                    Message = "\nCant  Allow To Delete This Is Default Value:",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var CheckInFmmaster = ctxTFAT.FMMaster.Where(x => x.VehicleCategory == mModel.Document).ToList();
            if (CheckInFmmaster.Count()>0)
            {
                return Json(new
                {
                    Message = "Not Allow To Delete",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var CheckInvehicleMaster = ctxTFAT.VehicleMaster.Where(x => x.TruckStatus == mModel.Document).ToList();
            if (CheckInvehicleMaster.Count() > 0)
            {
                return Json(new
                {
                    Message = "Not Allow To Delete",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var CheckInvehicleCategoryRateMaster = ctxTFAT.VehicleCategoryRates.Where(x => x.VehicleCategory == mModel.Document).ToList();
            if (CheckInvehicleMaster.Count() > 0)
            {
                return Json(new
                {
                    Message = "Not Allow To Delete",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var mList = ctxTFAT.VehicleCategory.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
            ctxTFAT.VehicleCategory.Remove(mList);
            ctxTFAT.SaveChanges();
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mList.Code, DateTime.Now, 0, mList.Code, "Delete Vehicle Category Master", "VCA");

            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData

        //public string ConvertDDMMYYTOYYMMDD(string da)
        //{
        //    string abc = da.Substring(6, 4) + "/" + da.Substring(3, 2) + "/" + da.Substring(0, 2);
        //    return abc;
        //}
        [HttpPost]
        public ActionResult CheckExistN(  string Value, string PKValue)
        {
            int count = 0;
            string message = "";

            count = ctxTFAT.VehicleCategory.Where(x => x.VehicleCategory1 == Value && x.Code!=PKValue).ToList().Count;

            if (count == 0)
            {
                message = "T";
            }
            else
            {
                message = "F";
            }
            return Json(new { Message = message, JsonRequestBehavior.AllowGet });
        }

    }
}