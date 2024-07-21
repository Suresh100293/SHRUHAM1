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
    public class MsgTemplateController : BaseController
    {
         
        ////private IBusinessCommon mBuss = new BusinessCommon();
        private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mauthorise = "A00";
        private static string mdocument = "";

        #region GetLists
        #endregion GetLists

        // GET: Logistics/MsgTemplate
        public ActionResult Index(MsgTemplateVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;
            mModel.MsgTemplate_Code = mModel.Document;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.MsgTemplate.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    mModel.MsgTemplate_Code = mList.Code;
                    mModel.MsgTemplate_Subject = mList.Subject;
                    mModel.MsgTemplate_MsgText = mList.MsgText;
                }
            }
            else
            {
                mModel.MsgTemplate_AttachPath = "";
                mModel.MsgTemplate_Code = "";
                mModel.MsgTemplate_IsHTML = false;
                mModel.MsgTemplate_MsgHTML = "";
                mModel.MsgTemplate_MsgRTF = "";
                mModel.MsgTemplate_MsgText = "";
                mModel.MsgTemplate_Subject = "";
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(MsgTemplateVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteMsgTemplate(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    MsgTemplate mobj = new MsgTemplate();
                    bool mAdd = true;
                    if (ctxTFAT.MsgTemplate.Where(x => (x.Code == mModel.MsgTemplate_Code)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.MsgTemplate.Where(x => (x.Code == mModel.MsgTemplate_Code)).FirstOrDefault();
                        mAdd = false;
                    }
                    mobj.Code = mModel.MsgTemplate_Code;
                    mobj.Subject = mModel.MsgTemplate_Subject;
                    mobj.MsgText = mModel.MsgTemplate_MsgText;
                    // iX9: default values for the fields not used @Form
                    mobj.AttachPath = "";
                    mobj.IsHTML = false;
                    mobj.MsgHTML = "";
                    mobj.MsgRTF = "";
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mAdd == true)
                    {
                        ctxTFAT.MsgTemplate.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    string mNewCode = "";
                    mNewCode = mobj.Code;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.Code, DateTime.Now, 0, "", "Save Email Templates", "NA");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "MsgTemplate" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "MsgTemplate" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "MsgTemplate" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "MsgTemplate" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteMsgTemplate(MsgTemplateVM mModel)
        {
            if (mModel.MsgTemplate_Code == null || mModel.MsgTemplate_Code == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var mList = ctxTFAT.MsgTemplate.Where(x => (x.Code == mModel.MsgTemplate_Code)).FirstOrDefault();
            ctxTFAT.MsgTemplate.Remove(mList);
            ctxTFAT.SaveChanges();
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.MsgTemplate_Code, DateTime.Now, 0, "", "Delete Email Templates", "NA");

            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}