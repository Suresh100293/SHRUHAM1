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
    public class LRTypeMasterController : BaseController
    {
         
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;

        #region Functions

        //public string ConvertDDMMYYTOYYMMDD(string da)
        //{
        //    string abc = da.Substring(6, 4) + "/" + da.Substring(3, 2) + "/" + da.Substring(0, 2);
        //    return abc;
        //}

        #endregion

        // GET: Logistics/LRTypeMaster
        public ActionResult Index(LRTypeMasterVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
           
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "LRTP");

            mdocument = mModel.Document;
            //mModel.TfatState_Name = mModel.Document;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var id = Convert.ToInt64(mModel.Document);
                var mList = ctxTFAT.LRTypeMaster.Where(x => (x.RECORDKEY == id)).FirstOrDefault();
                if (mList != null)
                {
                    mModel.Code = mList.Code;
                    mModel.LRType = mList.LRType;
                    mModel.Acitve = mList.Acitve;
                }
            }
            else
            {
                mModel.Code = "";
                mModel.LRType = "";
                mModel.Acitve = true;
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(LRTypeMasterVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {

                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        var mli = ctxTFAT.LRTypeMaster.Where(x => (x.RECORDKEY.ToString() == mModel.Document)).FirstOrDefault();

                        var Json=DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mli.Code, DateTime.Now, 0, mli.Code.ToString(), "Delete Doc. Type Master", "LRTP");
                        return Json;
                    }
                    LRTypeMaster mobj = new LRTypeMaster();
                    bool mAdd = true;
                    var id = Convert.ToInt64(mModel.Document);
                    if (ctxTFAT.LRTypeMaster.Where(x => (x.RECORDKEY == id)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.LRTypeMaster.Where(x => (x.RECORDKEY == id)).FirstOrDefault();
                        mAdd = false;
                    }

                    //mobj.Code = mModel.Code;
                    mobj.LRType = mModel.LRType;
                    mobj.Acitve = mModel.Acitve;

                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                    if (mAdd == true)
                    {
                        int NewCode1;
                        var NewCode = ctxTFAT.LRTypeMaster.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).Take(1).FirstOrDefault();
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
                        ctxTFAT.LRTypeMaster.Add(mobj);
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
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.Code, DateTime.Now, 0, mobj.Code, "Save Doc. Type Master", "LRTP");

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

        public ActionResult DeleteStateMaster(LRTypeMasterVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Not Geting LRTypeMaster Data",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var mList = ctxTFAT.LRTypeMaster.Where(x => (x.RECORDKEY.ToString() == mModel.Document)).FirstOrDefault();
            string mactivestring = "";
            if (mList!=null)
            {
                if (mList.Code.Trim() == "100000" || mList.Code.Trim() == "100001" || mList.Code.Trim() == "100002" || mList.Code.Trim() == "100003" || mList.Code.Trim() == "100004")
                {
                    mactivestring = mactivestring + "\nCant  Allow To Delete This Is Default Value: " + mList.LRType;
                }
            }
            var mactive1 = ctxTFAT.LRMaster.Where(x => (x.LRtype == mList.Code)).Select(x => x.LrNo).FirstOrDefault();
            if (mactive1 != 0)
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

            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {

                    ctxTFAT.LRTypeMaster.Remove(mList);

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
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