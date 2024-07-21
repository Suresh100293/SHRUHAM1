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
    public class VehicleGroupStatusController : BaseController
    {
         
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;

        // GET: Vehicles/VehicleGroupStatus
        public ActionResult Index(VehicleGrpStatusMasVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "","NA");
            mdocument = mModel.Document;

            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.VehicleGrpStatusMas.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                //var PartyCode = Convert.ToInt32(mList.Code);

                if (mList != null)
                {
                    mModel.Code = mList.Code;
                    mModel.VehicleGroupStatus = mList.VehicleGroupStatus;
                    mModel.Acitve = mList.Acitve;
                }
            }
            else
            {
                mModel.Code = "";
                mModel.VehicleGroupStatus = "";
                mModel.Acitve = true;
            }
            return View(mModel);
        }


        #region SaveData
        public ActionResult SaveData(VehicleGrpStatusMasVM mModel)
        {

            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        var Retur = DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Retur;
                    }
                    
                    VehicleGrpStatusMas mobj = new VehicleGrpStatusMas();
                    bool mAdd = true;
                    if (ctxTFAT.VehicleGrpStatusMas.Where(x => (x.Code == mModel.Document)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.VehicleGrpStatusMas.Where(x => (x.Code == mModel.Document)).FirstOrDefault();

                        mAdd = false;
                    }

                    //mobj.ShortName = mModel.ShortName;
                    mobj.VehicleGroupStatus = mModel.VehicleGroupStatus;
                    mobj.Acitve = mModel.Acitve;
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
                        var NewCode = ctxTFAT.VehicleGrpStatusMas.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).Take(1).FirstOrDefault();
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
                        ctxTFAT.VehicleGrpStatusMas.Add(mobj);
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
                   // UpdateAuditTrail(mbranchcode, mModel.Mode, "Vehicle-Group-Status-Master", "", DateTime.Now, 0, "", "");
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.Code, DateTime.Now, 0, mobj.Code, "Save Vehicle Group Status", "VGM");

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

        public ActionResult DeleteStateMaster(VehicleGrpStatusMasVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            string mactivestring = "";

            var Default = ctxTFAT.VehicleGrpStatusMas.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
            if (Default != null)
            {
                if (Default.Code.Trim() == "100000" || Default.Code.Trim() == "100001"|| Default.Code.Trim() == "100002")
                {
                    mactivestring = mactivestring + "\nCant  Allow To Delete This Is Default Value:: " + Default.VehicleGroupStatus;
                }
            }

            var mactive1 = ctxTFAT.FMMaster.Where(x => (x.VehicleStatus == mModel.Document)).Select(x => x.FmNo).FirstOrDefault();
            if (mactive1 != 0)
            {
                mactivestring = mactivestring + "\nFmNo: " + mactive1;
            }
            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "Active Account, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var mList = ctxTFAT.VehicleGrpStatusMas.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                    ctxTFAT.VehicleGrpStatusMas.Remove(mList);

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mList.Code, DateTime.Now, 0, mList.Code, "Delete Vehicle Group Status", "VGM");

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
        #endregion SaveData

        
    }
}