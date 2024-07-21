using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class StateMasterController : BaseController
    {
         
        //private IBusinessCommon mBuss = new BusinessCommon();
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;

        #region GetLists
        public JsonResult AutoCompleteCountry(string term)
        {
            return Json((from m in ctxTFAT.TfatCountry
                         where m.Name.ToLower().Contains(term.ToLower())
                 
        select new { Name = m.Name, Code = m.Code.ToString() }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public string GetCode()
        {
            string Code = "";
            var LastCode = ctxTFAT.TfatState.OrderByDescending(x => x.Code).FirstOrDefault();
            Code = (Convert.ToInt32(LastCode.Code) + 1).ToString();
            return Code;
        }
        #endregion GetLists

        // GET: Logistics/CountryMaster
        public ActionResult Index(StateMasterVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());

            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, mModel.Document, "", "STATE");

            mdocument = mModel.Document;
            mModel.TfatState_Name = mModel.Document;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.TfatState.Where(x => (x.Name == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    var mCountry = ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == mList.Country).Select(x => new { Name = x.Name, Code = x.Code.ToString() }).FirstOrDefault();
                    mModel.TfatState_Country = mCountry != null ? mCountry.Code.ToString() : "";
                    mModel.CountryName = mCountry != null ? mCountry.Name : "";
                    mModel.TfatState_Name = mList.Name;
                    mModel.TfatState_Code = mList.Code;
                }
            }
            else
            {
                mModel.TfatState_Code = 0;
                mModel.TfatState_Country = "";
                mModel.TfatState_Name = "";
                mModel.TfatState_StateCode = "";
            }
            return View(mModel);
        }


        #region SaveData
        public ActionResult SaveData(StateMasterVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        var mli = ctxTFAT.TfatState.Where(x => (x.Name.ToLower().Trim() == mModel.TfatState_Name.ToLower().Trim())).FirstOrDefault();

                        DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mli.Code, DateTime.Now, 0, mModel.TfatState_Name.ToUpper().Trim(), "Delete State Master", "STATE");
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    TfatState mobj = new TfatState();
                    bool mAdd = true;
                    if (ctxTFAT.TfatState.Where(x => (x.Name.ToLower().Trim() == mModel.TfatState_Name.ToLower().Trim())).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.TfatState.Where(x => (x.Name.ToLower().Trim() == mModel.TfatState_Name.ToLower().Trim())).FirstOrDefault();
                        mAdd = false;
                    }
                    //mobj.Code = mModel.TfatState_Code;
                    mobj.Country = mModel.TfatState_Country;
                    // iX9: default values for the fields not used @Form
                    mobj.StateCode = "";
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mAdd == true)
                    {
                        mobj.Name = mModel.TfatState_Name;
                        mobj.Code = Convert.ToInt32(GetCode());
                        ctxTFAT.TfatState.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    string mNewCode = "";
                    mNewCode = mobj.Name;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.Code, DateTime.Now, 0, mModel.TfatState_Name.ToUpper().Trim(), "Save State Master", "STATE");
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

        public ActionResult DeleteStateMaster(StateMasterVM mModel)
        {
            if (mModel.TfatState_Name == null || mModel.TfatState_Name == "")
            {
                return Json(new
                {
                    Message = "Name not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var mList = ctxTFAT.TfatState.Where(x => (x.Name == mModel.TfatState_Name)).FirstOrDefault();
            ctxTFAT.TfatState.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}