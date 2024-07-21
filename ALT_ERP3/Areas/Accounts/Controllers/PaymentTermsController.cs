using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
using ALT_ERP3.Models;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class PaymentTermsController : BaseController
    {
        private string mauthorise = "A00";

        public ActionResult Index(PaymentTermsVM Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, "Payment Terms", "", DateTime.Now, 0, "", "", "A");
            Model.PaymentTerms_Code = Model.Document;
            if ((Model.Mode == "Edit") || (Model.Mode == "View") || (Model.Mode == "Delete"))
            {
                var mList = ctxTFAT.PaymentTerms.Where(x => (x.Code == Model.Document)).FirstOrDefault();
                if (mList != null)
                {
                    Model.PaymentTerms_Code = (int)(mList.Code != null ? mList.Code : 0);
                    Model.PaymentTerms_CrPeriod = (int)(mList.CrPeriod != null ? mList.CrPeriod.Value : 0);
                    Model.PaymentTerms_Name = mList.Name;
                    Model.PaymentTerms_EarlyDay1 = (int)(mList.EarlyDay1 != null ? mList.EarlyDay1.Value : 0);
                    Model.PaymentTerms_EarlyPerc1 = (decimal)(mList.EarlyPerc1 != null ? mList.EarlyPerc1.Value : 0);
                    Model.PaymentTerms_EarlyDay2 = (int)(mList.EarlyDay2 != null ? mList.EarlyDay2.Value : 0);
                    Model.PaymentTerms_EarlyPerc2 = (decimal)(mList.EarlyPerc2 != null ? mList.EarlyPerc2.Value : 0);
                    Model.PaymentTerms_EarlyDay3 = (int)(mList.EarlyDay3 != null ? mList.EarlyDay3.Value : 0);
                    Model.PaymentTerms_EarlyPerc3 = (decimal)(mList.EarlyPerc3 != null ? mList.EarlyPerc3.Value : 0);
                    Model.PaymentTerms_EarlyDay4 = (int)(mList.EarlyDay4 != null ? mList.EarlyDay4.Value : 0);
                    Model.PaymentTerms_EarlyPerc4 = (decimal)(mList.EarlyPerc4 != null ? mList.EarlyPerc4.Value : 0);
                    Model.PaymentTerms_EarlyDay5 = (int)(mList.EarlyDay5 != null ? mList.EarlyDay5.Value : 0);
                    Model.PaymentTerms_EarlyPerc5 = (decimal)(mList.EarlyPerc5 != null ? mList.EarlyPerc5.Value : 0);
                    Model.PaymentTerms_LateDay1 = (int)(mList.LateDay1 != null ? mList.LateDay1.Value : 0);
                    Model.PaymentTerms_LatePerc1 = (decimal)(mList.LatePerc1 != null ? mList.LatePerc1.Value : 0);
                    Model.PaymentTerms_LateDay2 = (int)(mList.LateDay2 != null ? mList.LateDay2.Value : 0);
                    Model.PaymentTerms_LatePerc2 = (decimal)(mList.LatePerc2 != null ? mList.LatePerc2.Value : 0);
                    Model.PaymentTerms_LateDay3 = (int)(mList.LateDay3 != null ? mList.LateDay3.Value : 0);
                    Model.PaymentTerms_LatePerc3 = (decimal)(mList.LatePerc3 != null ? mList.LatePerc3.Value : 0);
                    Model.PaymentTerms_LateDay4 = (int)(mList.LateDay4 != null ? mList.LateDay4.Value : 0);
                    Model.PaymentTerms_LatePerc4 = (decimal)(mList.LatePerc4 != null ? mList.LatePerc4.Value : 0);
                    Model.PaymentTerms_LateDay5 = (int)(mList.LateDay5 != null ? mList.LateDay5.Value : 0);
                    Model.PaymentTerms_LatePerc5 = (decimal)(mList.LatePerc5 != null ? mList.LatePerc5.Value : 0);
                    Model.PaymentTerms_Narr = mList.Narr;
                    Model.AUTHORISE = mList.AUTHORISE;
                    mauthorise = Model.AUTHORISE;
                }
            }
            else
            {
                Model.PaymentTerms_Code = 0;
                Model.PaymentTerms_CrPeriod = 0;
                Model.PaymentTerms_EarlyDay1 = 0;
                Model.PaymentTerms_EarlyDay2 = 0;
                Model.PaymentTerms_EarlyDay3 = 0;
                Model.PaymentTerms_EarlyDay4 = 0;
                Model.PaymentTerms_EarlyDay5 = 0;
                Model.PaymentTerms_EarlyPerc1 = 0;
                Model.PaymentTerms_EarlyPerc2 = 0;
                Model.PaymentTerms_EarlyPerc3 = 0;
                Model.PaymentTerms_EarlyPerc4 = 0;
                Model.PaymentTerms_EarlyPerc5 = 0;
                Model.PaymentTerms_LateDay1 = 0;
                Model.PaymentTerms_LateDay2 = 0;
                Model.PaymentTerms_LateDay3 = 0;
                Model.PaymentTerms_LateDay4 = 0;
                Model.PaymentTerms_LateDay5 = 0;
                Model.PaymentTerms_LatePerc1 = 0;
                Model.PaymentTerms_LatePerc2 = 0;
                Model.PaymentTerms_LatePerc3 = 0;
                Model.PaymentTerms_LatePerc4 = 0;
                Model.PaymentTerms_LatePerc5 = 0;
                Model.PaymentTerms_Name = "";
                Model.PaymentTerms_Narr = "";
            }
            return View(Model);
        }

        public ActionResult MoveNextPrevious(string Mode, string mdocument)
        {
            string mtype = mdocument.Substring(0, 5);
            string mvar;
            if (Mode == "N")
            {
                mvar = Fieldoftable("PaymentTerms", "Top 1 TableKey", "TableKey>'" + mdocument + "' And Type='" + mtype + "' And Prefix='" + mperiod + "' order by TableKey", "T") ?? "";
            }
            else
            {
                mvar = Fieldoftable("PaymentTerms", "Top 1 TableKey", "TableKey<'" + mdocument + "' And Type='" + mtype + "' And Prefix='" + mperiod + "' order by TableKey desc", "T") ?? "";
            }
            if (mvar != "")
            {
                mvar = mbranchcode + mvar;
            }
            return Json(new { Status = "Success", data = mvar }, JsonRequestBehavior.AllowGet);
        }

        #region SaveData
        public ActionResult SaveData(PaymentTermsVM Model)
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
                    PaymentTerms mobj = new PaymentTerms();
                    bool mAdd = true;
                    if (ctxTFAT.PaymentTerms.Where(x => (x.Code == Model.PaymentTerms_Code)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.PaymentTerms.Where(x => (x.Code == Model.PaymentTerms_Code)).FirstOrDefault();
                        mAdd = false;
                    }
                    mobj.Code = Model.PaymentTerms_Code;
                    mobj.CrPeriod = Model.PaymentTerms_CrPeriod;
                    mobj.Name = Model.PaymentTerms_Name;
                    mobj.EarlyDay1 = Model.PaymentTerms_EarlyDay1;
                    mobj.EarlyPerc1 = Model.PaymentTerms_EarlyPerc1;
                    mobj.EarlyDay2 = Model.PaymentTerms_EarlyDay2;
                    mobj.EarlyPerc2 = Model.PaymentTerms_EarlyPerc2;
                    mobj.EarlyDay3 = Model.PaymentTerms_EarlyDay3;
                    mobj.EarlyPerc3 = Model.PaymentTerms_EarlyPerc3;
                    mobj.EarlyDay4 = Model.PaymentTerms_EarlyDay4;
                    mobj.EarlyPerc4 = Model.PaymentTerms_EarlyPerc4;
                    mobj.EarlyDay5 = Model.PaymentTerms_EarlyDay5;
                    mobj.EarlyPerc5 = Model.PaymentTerms_EarlyPerc5;
                    mobj.LateDay1 = Model.PaymentTerms_LateDay1;
                    mobj.LatePerc1 = Model.PaymentTerms_LatePerc1;
                    mobj.LateDay2 = Model.PaymentTerms_LateDay2;
                    mobj.LatePerc2 = Model.PaymentTerms_LatePerc2;
                    mobj.LateDay3 = Model.PaymentTerms_LateDay3;
                    mobj.LatePerc3 = Model.PaymentTerms_LatePerc3;
                    mobj.LateDay4 = Model.PaymentTerms_LateDay4;
                    mobj.LatePerc4 = Model.PaymentTerms_LatePerc4;
                    mobj.LateDay5 = Model.PaymentTerms_LateDay5;
                    mobj.LatePerc5 = Model.PaymentTerms_LatePerc5;
                    mobj.Narr = Model.PaymentTerms_Narr ?? "";
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = DateTime.Now;
                    mobj.AUTHORISE = "A00";
                    if (mAdd == true)
                    {
                        mobj.Code = GetNextCode();
                        Model.PaymentTerms_Code = mobj.Code;
                    }
                    if (mAdd == true)
                    {
                        ctxTFAT.PaymentTerms.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    string mNewCode = mobj.Code.ToString();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, mNewCode.ToString(), DateTime.Now, 0, mNewCode.ToString(), "", "A");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { ex1.InnerException.Message, Status = "Error", id = "PaymentTerms" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { ex.InnerException.Message, Status = "Error", id = "PaymentTerms" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "PaymentTerms" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", NewSrl = Model.PaymentTerms_Code, id = "PaymentTerms" }, JsonRequestBehavior.AllowGet);
        }

        public int GetNextCode()
        {
            var nextcode = (from x in ctxTFAT.PaymentTerms select (int?)x.Code).Max() ?? 0;
            return (++nextcode);
        }

        public ActionResult DeleteData(PaymentTermsVM Model)
        {
            DeUpdate(Model);
            return Json(new { Status = "Success", Message = "The Document is Deleted." }, JsonRequestBehavior.AllowGet);
        }

        private void DeUpdate(PaymentTermsVM Model)
        {
            var mList = ctxTFAT.PaymentTerms.Where(x => (x.Code == Model.PaymentTerms_Code)).ToList();
            ctxTFAT.PaymentTerms.RemoveRange(mList);
            ctxTFAT.SaveChanges();
        }
        #endregion SaveData
    }
}