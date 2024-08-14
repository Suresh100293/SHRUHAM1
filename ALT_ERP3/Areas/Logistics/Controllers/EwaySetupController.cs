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
    public class EwaySetupController : BaseController
    {
        private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mauthorise = "A00";
        private static string mdocument = "";

        #region EwayBill Functions

        public ActionResult GetSuppltType()
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            mList.Add(new SelectListItem { Text = "Inward", Value = "I" });
            mList.Add(new SelectListItem { Text = "Outward", Value = "O" });
            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSubSuppltType(string SupplyType)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            if (SupplyType == "I")
            {
                mList.Add(new SelectListItem { Text = "Supply", Value = "1" });
                mList.Add(new SelectListItem { Text = "Import", Value = "2" });
                mList.Add(new SelectListItem { Text = "For Own Use", Value = "5" });
                mList.Add(new SelectListItem { Text = "Job work Returns", Value = "6" });
                mList.Add(new SelectListItem { Text = "Sales Return", Value = "7" });
                mList.Add(new SelectListItem { Text = "Others", Value = "8" });
                mList.Add(new SelectListItem { Text = "SKD/CKD/Lots", Value = "9" });
                mList.Add(new SelectListItem { Text = "Exhibition or Fairs", Value = "12" });
            }
            else
            {
                mList.Add(new SelectListItem { Text = "Supply", Value = "1" });
                mList.Add(new SelectListItem { Text = "Export", Value = "3" });
                mList.Add(new SelectListItem { Text = "Job Work", Value = "4" });
                mList.Add(new SelectListItem { Text = "For Own Use", Value = "5" });
                mList.Add(new SelectListItem { Text = "Others", Value = "8" });
                mList.Add(new SelectListItem { Text = "SKD/CKD/Lots", Value = "9" });
                mList.Add(new SelectListItem { Text = "Line Sales", Value = "10" });
                mList.Add(new SelectListItem { Text = "Recipient  Not Known", Value = "11" });
                mList.Add(new SelectListItem { Text = "Exhibition or Fairs", Value = "12" });

            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDocType()
        {
            List<SelectListItem> mList = new List<SelectListItem>();

            mList.Add(new SelectListItem { Text = "Tax Invoice", Value = "INV" });
            mList.Add(new SelectListItem { Text = "Bill of Supply", Value = "BIL" });
            mList.Add(new SelectListItem { Text = "Bill of Entry", Value = "BOE" });
            mList.Add(new SelectListItem { Text = "Delivery Challan", Value = "CHL" });
            mList.Add(new SelectListItem { Text = "Others", Value = "OTH" });
            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTrasactType()
        {
            List<SelectListItem> mList = new List<SelectListItem>();

            mList.Add(new SelectListItem { Text = "Regular", Value = "1" });
            mList.Add(new SelectListItem { Text = "Bill To-Ship To", Value = "2" });
            mList.Add(new SelectListItem { Text = "Bill From-Dispatch From", Value = "3" });
            mList.Add(new SelectListItem { Text = "Combination of 2 and 3", Value = "4" });

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetVehicleType()
        {
            List<SelectListItem> mList = new List<SelectListItem>();

            mList.Add(new SelectListItem { Text = "Regular", Value = "R" });
            mList.Add(new SelectListItem { Text = "ODC", Value = "O" });

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetConsignmentStatus()
        {
            List<SelectListItem> mList = new List<SelectListItem>();

            mList.Add(new SelectListItem { Text = "In Transit", Value = "T" });
            mList.Add(new SelectListItem { Text = "In Movement", Value = "M" });

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetReasonCode()
        {
            List<SelectListItem> mList = new List<SelectListItem>();

            mList.Add(new SelectListItem { Text = "Due To Break Down", Value = "1" });
            mList.Add(new SelectListItem { Text = "Due To Transhipment", Value = "2" });
            mList.Add(new SelectListItem { Text = "Others", Value = "3" });
            mList.Add(new SelectListItem { Text = "First Time", Value = "4" });

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetExtendReasonCode()
        {
            List<SelectListItem> mList = new List<SelectListItem>();

            mList.Add(new SelectListItem { Text = "Natural Calamity", Value = "1" });
            mList.Add(new SelectListItem { Text = "Law & Order", Value = "2" });
            mList.Add(new SelectListItem { Text = "Transhipment", Value = "3" });
            mList.Add(new SelectListItem { Text = "Accident", Value = "4" });
            mList.Add(new SelectListItem { Text = "Others", Value = "5" });

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCancelReasonCode()
        {
            List<SelectListItem> mList = new List<SelectListItem>();

            mList.Add(new SelectListItem { Text = "Duplicate", Value = "1" });
            mList.Add(new SelectListItem { Text = "Order Cancelled", Value = "2" });
            mList.Add(new SelectListItem { Text = "Data Entry Mistake", Value = "3" });
            mList.Add(new SelectListItem { Text = "Others", Value = "4" });

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        #endregion

        // GET: Logistics/EwaySetup
        public ActionResult Index(EwaySetupVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;

            var id = Convert.ToInt64(1);
            var mList = ctxTFAT.tfatEwaySetup.FirstOrDefault();
            if (mList != null)
            {
                mModel.AllBranch = mList.AllBranch;
                mModel.UserBranch = mList.UserBranch;
                mModel.ConsoleForAllBranch = mList.ConsoleForAllBranch;
                mModel.ConsoleForUserBranch = mList.ConsoleForUserBranch;
                mModel.AutoConsole = mList.AutoConsole;


                mModel.GenSupplyType = mList.GenSupplyType;
                mModel.GenSubType = mList.GenSubType;
                mModel.GenDoctype = mList.GenDoctype;
                mModel.GenTranType = mList.GenTranType;
                mModel.GenVehicleType = mList.GenVehicleType;
                mModel.MulReason = mList.MulReason;
                mModel.BPartReason = mList.BPartReason;
                mModel.ExtConsignIs = mList.ExtConsignIs;
                mModel.ExtTranType = mList.ExtTranType;
                mModel.ExtReason = mList.ExtReason;
                mModel.CanReason = mList.CanReason;
                mModel.AutoExtendMailID = mList.AutoExtendMailID;
            }

            return View(mModel);
        }

        public ActionResult SaveData(EwaySetupVM mModel)
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

                    tfatEwaySetup mobj = new tfatEwaySetup();
                    bool mAdd = true;
                    var id = Convert.ToInt64(1);
                    if (ctxTFAT.tfatEwaySetup.Where(x => x.RECORDKEY == id).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.tfatEwaySetup.Where(x => x.RECORDKEY == id).FirstOrDefault();
                        mAdd = false;
                    }

                    mobj.AllBranch = mModel.AllBranch;
                    mobj.UserBranch = mModel.UserBranch;
                    mobj.ConsoleForAllBranch = mModel.ConsoleForAllBranch;
                    mobj.ConsoleForUserBranch = mModel.ConsoleForUserBranch;


                    mobj.GenSupplyType = mModel.GenSupplyType;
                    mobj.GenSubType = mModel.GenSubType;
                    mobj.GenDoctype = mModel.GenDoctype;
                    mobj.GenTranType = mModel.GenTranType;
                    mobj.GenVehicleType = mModel.GenVehicleType;
                    mobj.MulReason = mModel.MulReason;
                    mobj.BPartReason = mModel.BPartReason;
                    mobj.ExtConsignIs = mModel.ExtConsignIs;
                    mobj.ExtTranType = mModel.ExtTranType;
                    mobj.ExtReason = mModel.ExtReason;
                    mobj.CanReason = mModel.CanReason;
                    mobj.AutoExtendMailID = mModel.AutoExtendMailID;
                    mobj.AutoConsole = mModel.AutoConsole;

                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    mobj.AUTHORISE = mauthorise;
                    mobj.AUTHIDS = muserid;

                    if (mAdd == true)
                    {
                        ctxTFAT.tfatEwaySetup.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "Save Eway Bill Setup", "NA");
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

        public ActionResult DeleteStateMaster(EwaySetupVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Document Missing..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var id = Convert.ToInt64(mModel.Document);
            var mList = ctxTFAT.tfatEwaySetup.Where(x => (x.RECORDKEY == id)).FirstOrDefault();
            ctxTFAT.tfatEwaySetup.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
    }
}