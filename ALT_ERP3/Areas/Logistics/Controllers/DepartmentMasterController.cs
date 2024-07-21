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
    public class DepartmentMasterController : BaseController
    {
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;

        // GET: Logistics/DepartmentMaster
        public ActionResult Index(DeptVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");

            mdocument = mModel.Document;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.Dept.Where(x => (x.Code.ToString() == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    mModel.Name = mList.Name;
                    mModel.Code = mList.Code;
                    var UserList = ctxTFAT.TfatPass.Where(x => x.Dept.ToString() == mModel.Code.ToString()).Select(x => x.Name).ToList();
                    mModel.Users = UserList;
                }
            }
            else
            {
                //mModel.TfatState_Code = 0;
                //mModel.TfatState_Country = "";
                //mModel.TfatState_Name = "";
                //mModel.TfatState_StateCode = "";
                
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(DeptVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        var MSG = DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.Document, DateTime.Now, 0, mModel.Document.ToString(), "Delete Department Master", "DEPT");

                        return MSG;
                    }
                    Dept mobj = new Dept();
                    bool mAdd = true;
                    if (ctxTFAT.Dept.Where(x => (x.Code.ToString() == mModel.Document)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.Dept.Where(x => (x.Code.ToString() == mModel.Document)).FirstOrDefault();
                        mAdd = false;
                    }

                    mobj.Name = mModel.Name;

                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                    if (mAdd == true)
                    {
                        int NewCode1;
                        var NewCode = ctxTFAT.Dept.OrderByDescending(x => x.Code).Select(x => x.Code).Take(1).FirstOrDefault();
                        if (NewCode == 0 )
                        {
                            NewCode1 = 100000;
                        }
                        else
                        {
                            NewCode1 = Convert.ToInt32(NewCode) + 1;
                        }
                        mobj.Code = NewCode1;
                        ctxTFAT.Dept.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = Convert.ToInt32(mobj.RECORDKEY);
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.Code, DateTime.Now, 0, mobj.Code.ToString(), "Save Department Master", "DEPT");

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

        public JsonResult DeleteStateMaster(DeptVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Department not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            
            string mactivestring = "";
            var mactive1 = ctxTFAT.TfatPass.Where(x => (x.Dept.ToString() == mModel.Document)).Select(x => x.Name).FirstOrDefault();
            if (!String.IsNullOrEmpty(mactive1))
            {
                mactivestring = mactivestring + "\nLrNo: " + mactive1;
            }
            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "Active Account, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var mList = ctxTFAT.Dept.Where(x => (x.Code.ToString() == mModel.Document)).FirstOrDefault();
            ctxTFAT.Dept.Remove(mList);

            ctxTFAT.SaveChanges();

            return Json(new
            {
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData

    }
}