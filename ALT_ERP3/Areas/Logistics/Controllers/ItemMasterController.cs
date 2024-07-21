using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class ItemMasterController : BaseController
    {
         
        //private IBusinessCommon mBuss = new BusinessCommon();
        private static string mdocument = "";
        private static string mauthorise = "A00";
        private int mnewrecordkey = 0;


        //GET: SetUp/ItemMaster
        public ActionResult Index(ItemMasterVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());

            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, mModel.Document, "", "I");

            mdocument = mModel.Document;
           
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.ItemMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    mModel.Code = mList.Code;
                    mModel.Name = mList.Name;
                    
                }
            }
            else
            {
                var newLrNo = ctxTFAT.ItemMaster.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).Take(1).FirstOrDefault();
                if (newLrNo == null)
                {
                    mModel.Code = "1";
                }
                else
                {
                    mModel.Code = (Convert.ToInt32(newLrNo) + 1).ToString();
                }
                mModel.Hide = false;
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(ItemMasterVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteUnitofMeasurementMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    ItemMaster mobj = new ItemMaster();
                    bool mAdd = true;
                    if (ctxTFAT.ItemMaster.Where(x => (x.Code == mModel.Code)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.ItemMaster.Where(x => (x.Code == mModel.Code)).FirstOrDefault();
                        mAdd = false;
                    }
                    mobj.Code = mModel.Code;
                    mobj.Name = mModel.Name;
                    
                    
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mAdd == true)
                    {
                        ctxTFAT.ItemMaster.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, mobj.Code, "","I");

                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "UnitofMeasurementMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "UnitofMeasurementMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "UnitofMeasurementMaster" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "UnitofMeasurementMaster" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteUnitofMeasurementMaster(ItemMasterVM mModel)
        {
            if (mModel.Code == null || mModel.Code == "")
            {
                return Json(new
                {
                    Message = "Code Not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var mList = ctxTFAT.ItemMaster.Where(x => (x.Code == mModel.Code)).FirstOrDefault();
            ctxTFAT.ItemMaster.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData

    }
}