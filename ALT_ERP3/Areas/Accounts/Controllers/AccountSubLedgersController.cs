using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
using EntitiModel;
using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class AccountSubLedgersController : BaseController
    {
        //private //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        //private IBusinessCommon mBuss = new BusinessCommon();
        //private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mAUTHORISE = "A00";
        private static int mdocument = 0;

        // GET: Accounts/AccountSubLedgers
        #region GetLists
        #endregion GetLists

        public ActionResult Index(AccountSubLedgersVM mModel)
        {
            //GetAllMenu(Session["ModuleName"].ToString());
            ////AccountingGetAllMenu(Session["ModuleName"].ToString());
            ////VehicleGetAllMenu(Session["ModuleName"].ToString());
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "","A");
            mdocument = mModel.Document;
            mModel.SubLedger_Code = mModel.Document;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.SubLedger.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    mModel.SubLedger_Code = mList.Code;
                    mModel.SubLedger_Name = mList.Name;
                    mModel.SubLedger_Locked = mList.Locked;
                }
            }
            else
            {
                mModel.SubLedger_Locked = false;
                mModel.SubLedger_Name = "Default";
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(AccountSubLedgersVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteAccountSubLedgers(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    SubLedger mobj = new SubLedger();
                    bool mAdd = true;
                    if (ctxTFAT.SubLedger.Where(x => (x.Code == mModel.SubLedger_Code)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.SubLedger.Where(x => (x.Code == mModel.SubLedger_Code)).FirstOrDefault();
                        mAdd = false;
                    }
                    mobj.Code = mModel.SubLedger_Code;
                    mobj.Name = mModel.SubLedger_Name;
                    mobj.Locked = mModel.SubLedger_Locked;
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mAUTHORISE;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mAdd == true)
                    {
                        mobj.Code = GetNextCode();
                    }
                    if (mAdd == true)
                    {
                        ctxTFAT.SubLedger.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    transaction.Commit();
                    transaction.Dispose();
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "AccountSubLedgers" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "AccountSubLedgers" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "AccountSubLedgers" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "AccountSubLedgers" }, JsonRequestBehavior.AllowGet);
        }

        public int GetNextCode()
        {
            var nextcode = (from x in ctxTFAT.SubLedger select (int?)x.Code).Max() ?? 0;
            return (++nextcode);
        }

        public ActionResult DeleteAccountSubLedgers(AccountSubLedgersVM mModel)
        {
            // iX9: Check for Active Master SubLedger
            string mactivestring = "";
            var mactive1 = ctxTFAT.MasterSubLedger.Where(x => (x.SLCode == mModel.SubLedger_Code)).Select(x => x.SLCode).FirstOrDefault();
            if (mactive1 == 0) { mactivestring = mactivestring + "\nMasterSubLedger: " + mactive1; }
            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "Active Item, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var mList = ctxTFAT.SubLedger.Where(x => (x.Code == mModel.SubLedger_Code)).FirstOrDefault();
            ctxTFAT.SubLedger.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}