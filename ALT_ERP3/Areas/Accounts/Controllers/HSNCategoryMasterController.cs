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
    public class HSNCategoryMasterController : BaseController
    {
        private int mnewrecordkey = 0;

        #region GetLists
        #endregion GetLists

        public ActionResult Index(HSNCategoryMasterVM Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, "HSN Category Master", "", DateTime.Now, 0, "", "", "A");
            Model.HSNCategory_Code = Model.Document;
            if ((Model.Mode == "Edit") || (Model.Mode == "View") || (Model.Mode == "Delete"))
            {
                var mList = ctxTFAT.HSNCategory.Where(x => (x.Code == Model.Document)).FirstOrDefault();
                if (mList != null)
                {
                    Model.HSNCategory_Code = mList.Code;
                    Model.HSNCategory_Name = mList.Name;
                    Model.AUTHORISE = mList.AUTHORISE;
                }
            }
            else
            {
                Model.HSNCategory_Code = 0;
                Model.HSNCategory_Name = "";
            }
            return View(Model);
        }

        #region SaveData
        public ActionResult SaveData(HSNCategoryMasterVM Model)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (Model.Mode == "Delete")
                    {
                        DeleteData(Model);
                        transaction.Commit();
                        transaction.Dispose();
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    HSNCategory mobj = new HSNCategory();
                    bool mAdd = true;
                    if (ctxTFAT.HSNCategory.Where(x => (x.Code == Model.HSNCategory_Code)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.HSNCategory.Where(x => (x.Code == Model.HSNCategory_Code)).FirstOrDefault();
                        mAdd = false;
                    }
                    mobj.Code = Model.HSNCategory_Code;
                    mobj.Name = Model.HSNCategory_Name;
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = DateTime.Now;
                    mobj.AUTHORISE = "A00";
                    if (mAdd == true)
                    {
                        mobj.Code = GetNextCode();
                    }
                    if (mAdd == true)
                    {
                        ctxTFAT.HSNCategory.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    int mNewCode = 0;
                    mNewCode = mobj.Code;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, mNewCode.ToString(), DateTime.Now, 0, mNewCode.ToString(), "", "A");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { ex1.InnerException.Message, Status = "Error", id = "HSNCategoryMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { ex.InnerException.Message, Status = "Error", id = "HSNCategoryMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "HSNCategoryMaster" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "HSNCategoryMaster" }, JsonRequestBehavior.AllowGet);
        }

        public int GetNextCode()
        {
            var nextcode = (from x in ctxTFAT.HSNCategory select (int?)x.Code).Max() ?? 0;
            return (++nextcode);
        }

        public ActionResult DeleteData(HSNCategoryMasterVM Model)
        {
            // iX9: Check for Active Master HSNCategory
            string mactivestring = "";
            var mactive1 = ctxTFAT.HSNMaster.Where(x => (x.Grp == Model.HSNCategory_Code)).Select(x => x.Name).FirstOrDefault();
            if (mactive1 != null) { mactivestring = mactivestring + "\nHSNMaster: " + mactive1; }
            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "Active Item, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            DeUpdate(Model);
            return Json(new { Status = "Success", Message = "The Document is Deleted." }, JsonRequestBehavior.AllowGet);
        }

        private void DeUpdate(HSNCategoryMasterVM Model)
        {
            var mList = ctxTFAT.HSNCategory.Where(x => (x.Code == Model.HSNCategory_Code)).ToList();
            ctxTFAT.HSNCategory.RemoveRange(mList);
            ctxTFAT.SaveChanges();
        }
        #endregion SaveData
    }
}