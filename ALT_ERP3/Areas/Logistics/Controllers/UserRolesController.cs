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

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class UserRolesController : BaseController
    {
         
        ////private IBusinessCommon mBuss = new BusinessCommon();
        private int mlocation = 100001;
        //private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        //private string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        //private string muserid = System.Web.HttpContext.Current.Session["UserId"].ToString();
        //private string mperiod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        private int mnewrecordkey = 0;
        private static string mauthorise = "A00";
        private static string mdocument = "";

        #region GetLists
        #endregion GetLists


        // GET: Logistics/UserRoles
        public ActionResult Index(UserRolesVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;
            mModel.UserRoles_Code = mModel.Document;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.UserRoles.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    mModel.UserRoles_Code = mList.Code;
                    mModel.UserRoles_Name = mList.Name;
                    mModel.UserRoles_Sun = mList.Sun;
                    mModel.UserRoles_Mon = mList.Mon;
                    mModel.UserRoles_Tue = mList.Tue;
                    mModel.UserRoles_Wed = mList.Wed;
                    mModel.UserRoles_Thu = mList.Thu;
                    mModel.UserRoles_Fri = mList.Fri;
                    mModel.UserRoles_Sat = mList.Sat;
                }
            }
            else
            {
                mModel.UserRoles_Code = "";
                mModel.UserRoles_Fri = false;
                mModel.UserRoles_Mon = false;
                mModel.UserRoles_Name = "";
                mModel.UserRoles_Sat = false;
                mModel.UserRoles_Sun = false;
                mModel.UserRoles_Thu = false;
                mModel.UserRoles_Tue = false;
                mModel.UserRoles_Wed = false;
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(UserRolesVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteUserRoles(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.Document, DateTime.Now, 0, "", "Delete UserRole.", "NA");
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    UserRoles mobj = new UserRoles();
                    bool mAdd = true;
                    if (ctxTFAT.UserRoles.Where(x => (x.Code == mModel.UserRoles_Code)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.UserRoles.Where(x => (x.Code == mModel.UserRoles_Code)).FirstOrDefault();
                        mAdd = false;
                    }
                    mobj.Code = mModel.UserRoles_Code;
                    mobj.Name = mModel.UserRoles_Name;
                    mobj.Sun = mModel.UserRoles_Sun;
                    mobj.Mon = mModel.UserRoles_Mon;
                    mobj.Tue = mModel.UserRoles_Tue;
                    mobj.Wed = mModel.UserRoles_Wed;
                    mobj.Thu = mModel.UserRoles_Thu;
                    mobj.Fri = mModel.UserRoles_Fri;
                    mobj.Sat = mModel.UserRoles_Sat;
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mAdd == true)
                    {
                        ctxTFAT.UserRoles.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.Code, DateTime.Now, 0, "", "Save UserRole.", "NA");

                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "UserRoles" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "UserRoles" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "UserRoles" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "UserRoles" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteUserRoles(UserRolesVM mModel)
        {
            if (mModel.UserRoles_Code == null || mModel.UserRoles_Code == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var mList = ctxTFAT.UserRoles.Where(x => (x.Code == mModel.UserRoles_Code)).FirstOrDefault();
            ctxTFAT.UserRoles.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}