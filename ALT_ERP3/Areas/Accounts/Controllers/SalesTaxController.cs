using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
////using ALT_ERP3.DynamicBusinessLayer;
////using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class SalesTaxController : BaseController
    {
        //private //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        //private IBusinessCommon mBuss = new BusinessCommon();
        private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mAUTHORISE = "A00";
        private static string mdocument = "";

        #region GetLists
        public List<SelectListItem> GetScopeList()
        {
            List<SelectListItem> CallScopeList = new List<SelectListItem>();
            CallScopeList.Add(new SelectListItem { Value = "S", Text = "Sales" });
            CallScopeList.Add(new SelectListItem { Value = "P", Text = "Purchase" });
            return CallScopeList;
        }
        public List<SelectListItem> GetFormList()
        {
            List<SelectListItem> CallFormList = new List<SelectListItem>();
            CallFormList.Add(new SelectListItem { Value = "Y", Text = "Yes" });
            CallFormList.Add(new SelectListItem { Value = "N", Text = "No" });
            return CallFormList;
        }
        public JsonResult AutoCompletePostCode(string term)
        {
            return Json((from m in ctxTFAT.Master
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteSGSTCode(string term)
        {
            return Json((from m in ctxTFAT.Master
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteCGSTCode(string term)
        {
            return Json((from m in ctxTFAT.Master
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteIGSTCode(string term)
        {
            return Json((from m in ctxTFAT.Master
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteCessCode(string term)
        {
            return Json((from m in ctxTFAT.Master
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        #endregion GetLists

        // GET: Accounts/SalesTax
        public ActionResult Index(SalesTaxVM mModel)
        {
            //GetAllMenu(Session["ModuleName"].ToString());
            ////AccountingGetAllMenu(Session["ModuleName"].ToString());
            ////VehicleGetAllMenu(Session["ModuleName"].ToString());
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0,"", "", "NA");
            mdocument = mModel.Document;
            mModel.ScopeList = GetScopeList();
            mModel.FormList = GetFormList();
            mModel.TaxMaster_Code = mModel.Document;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.TaxMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    var mPostCode = ctxTFAT.Master.Where(x => x.Code == mList.PostCode).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mSGSTCode = ctxTFAT.Master.Where(x => x.Code == mList.SGSTCode).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mCGSTCode = ctxTFAT.Master.Where(x => x.Code == mList.CGSTCode).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mIGSTCode = ctxTFAT.Master.Where(x => x.Code == mList.IGSTCode).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mCessCode = ctxTFAT.Master.Where(x => x.Code == mList.CessCode).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    mModel.TaxMaster_PostCode = mPostCode != null ? mPostCode.Code.ToString() : "";
                    mModel.PostCodeName = mPostCode != null ? mPostCode.Name : "";
                    mModel.TaxMaster_SGSTCode = mSGSTCode != null ? mSGSTCode.Code.ToString() : "";
                    mModel.SGSTCodeName = mSGSTCode != null ? mSGSTCode.Name : "";
                    mModel.TaxMaster_CGSTCode = mCGSTCode != null ? mCGSTCode.Code.ToString() : "";
                    mModel.CGSTCodeName = mCGSTCode != null ? mCGSTCode.Name : "";
                    mModel.TaxMaster_IGSTCode = mIGSTCode != null ? mIGSTCode.Code.ToString() : "";
                    mModel.IGSTCodeName = mIGSTCode != null ? mIGSTCode.Name : "";
                    mModel.TaxMaster_CessCode = mCessCode != null ? mCessCode.Code.ToString() : "";
                    mModel.CessCodeName = mCessCode != null ? mCessCode.Name : "";
                    mModel.TaxMaster_Code = mList.Code;
                    mModel.TaxMaster_Scope = mList.Scope;
                    mModel.TaxMaster_Name = mList.Name;
                    mModel.TaxMaster_VATGST = mList.VATGST;
                    mModel.TaxMaster_DiscOnTxbl = mList.DiscOnTxbl;
                    mModel.TaxMaster_Pct = mList.Pct != null ? mList.Pct.Value : 0;
                    mModel.TaxMaster_MRPTax = mList.MRPTax;
                    mModel.TaxMaster_Inclusive = mList.Inclusive;
                    mModel.TaxMaster_SGSTRate = mList.SGSTRate != null ? mList.SGSTRate.Value : 0;
                    mModel.TaxMaster_CGSTRate = mList.CGSTRate != null ? mList.CGSTRate.Value : 0;
                    mModel.TaxMaster_IGSTRate = mList.IGSTRate != null ? mList.IGSTRate.Value : 0;
                    mModel.TaxMaster_CessRate = mList.CessRate != null ? mList.CessRate.Value : 0;
                    mModel.TaxMaster_Composition = mList.Composition;
                    mModel.TaxMaster_Form = mList.Form;
                    mModel.TaxMaster_Exempted = mList.Exempted;
                }
            }
            else
            {
                mModel.TaxMaster_Cess = 0;
                mModel.TaxMaster_CessCode = "";
                mModel.TaxMaster_CessRate = 0;
                mModel.TaxMaster_CGSTCode = "";
                mModel.TaxMaster_CGSTRate = 0;
                mModel.TaxMaster_Code = "";
                mModel.TaxMaster_Composition = false;
                mModel.TaxMaster_DiscOnTxbl = false;
                mModel.TaxMaster_Exempted = false;
                mModel.TaxMaster_Form = "";
                mModel.TaxMaster_FormName = "";
                mModel.TaxMaster_GSTType = 0;
                mModel.TaxMaster_IGSTCode = "";
                mModel.TaxMaster_IGSTRate = 0;
                mModel.TaxMaster_Inclusive = false;
                mModel.TaxMaster_Labour = false;
                mModel.TaxMaster_Locked = false;
                mModel.TaxMaster_MRPTax = false;
                mModel.TaxMaster_Name = "";
                mModel.TaxMaster_Pct = 0;
                mModel.TaxMaster_PostCode = "";
                mModel.TaxMaster_Scope = "";
                mModel.TaxMaster_SetOff = 0;
                mModel.TaxMaster_SGSTCode = "";
                mModel.TaxMaster_SGSTRate = 0;
                mModel.TaxMaster_Taxable = false;
                mModel.TaxMaster_TaxableCode = "";
                mModel.TaxMaster_VATGST = false;
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(SalesTaxVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteSalesTax(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.TaxMaster_Code, DateTime.Now, 0,"", "Delete Sales Tax", "NA");
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }

                    var GetItem = ctxTFAT.TaxMaster.Where(x => x.Code != mModel.Document && x.Code == mModel.TaxMaster_Code).FirstOrDefault();
                    if (GetItem!=null)
                    {
                        return Json(new { Status = "Error", Message = "Code Already Generated.\n Please Change The Code...!" }, JsonRequestBehavior.AllowGet);
                    }


                    TaxMaster mobj = new TaxMaster();
                    bool mAdd = true;
                    if (ctxTFAT.TaxMaster.Where(x => (x.Code == mModel.TaxMaster_Code)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.TaxMaster.Where(x => (x.Code == mModel.TaxMaster_Code)).FirstOrDefault();
                        mAdd = false;
                    }
                    
                    mobj.Scope = mModel.TaxMaster_Scope;
                    mobj.Name = mModel.TaxMaster_Name;
                    mobj.VATGST = mModel.TaxMaster_VATGST;
                    mobj.DiscOnTxbl = mModel.TaxMaster_DiscOnTxbl;
                    mobj.Pct = mModel.TaxMaster_Pct;
                    mobj.MRPTax = mModel.TaxMaster_MRPTax;
                    mobj.Inclusive = mModel.TaxMaster_Inclusive;
                    mobj.PostCode = mModel.TaxMaster_PostCode;
                    mobj.SGSTRate = mModel.TaxMaster_SGSTRate;
                    mobj.SGSTCode = mModel.TaxMaster_SGSTCode;
                    mobj.CGSTRate = mModel.TaxMaster_CGSTRate;
                    mobj.CGSTCode = mModel.TaxMaster_CGSTCode;
                    mobj.IGSTRate = mModel.TaxMaster_IGSTRate;
                    mobj.IGSTCode = mModel.TaxMaster_IGSTCode;
                    mobj.CessRate = mModel.TaxMaster_CessRate;
                    mobj.CessCode = mModel.TaxMaster_CessCode;
                    mobj.Composition = mModel.TaxMaster_Composition;
                    mobj.Form = mModel.TaxMaster_Form;
                    mobj.Exempted = mModel.TaxMaster_Exempted;
                    // iX9: default values for the fields not used @Form
                    mobj.Cess = 0;
                    mobj.FormName = "";
                    mobj.GSTType = 0;
                    mobj.Labour = false;
                    mobj.Locked = false;
                    mobj.SetOff = 0;
                    mobj.Taxable = false;
                    mobj.TaxableCode = "";
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mAUTHORISE;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mAdd == true)
                    {
                        mobj.Code = mModel.TaxMaster_Code;
                        mobj.ENTEREDBY = muserid;
                        ctxTFAT.TaxMaster.Add(mobj);
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
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mNewCode, DateTime.Now, 0, "", "Save Sales Tax", "NA");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "SalesTax" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "SalesTax" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "SalesTax" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "SalesTax" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteSalesTax(SalesTaxVM mModel)
        {
            if (mModel.TaxMaster_Code == null || mModel.TaxMaster_Code == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            // iX9: Check for Active Master TaxMaster
            string mactivestring = "";
            var mactive3 = ctxTFAT.Charges.Where(x => (x.TaxCode == mModel.TaxMaster_Code)).Select(x => x.TaxCode).FirstOrDefault();
            if (mactive3 != null) { mactivestring = mactivestring + "\nCharges: " + mactive3; }
            var mactive5 = ctxTFAT.OrdersStk.Where(x => (x.TaxCode == mModel.TaxMaster_Code)).Select(x => x.TaxCode).FirstOrDefault();
            if (mactive5 != null) { mactivestring = mactivestring + "\nOrdersStk: " + mactive5; }
            var mactive11 = ctxTFAT.StockTax.Where(x => (x.TaxCode == mModel.TaxMaster_Code)).Select(x => x.TaxCode).FirstOrDefault();
            if (mactive11 != null) { mactivestring = mactivestring + "\nStockTax: " + mactive11; }
            var mactive12 = ctxTFAT.TaxForms.Where(x => (x.TaxCode == mModel.TaxMaster_Code)).Select(x => x.TaxCode).FirstOrDefault();
            if (mactive12 != null) { mactivestring = mactivestring + "\nTaxForms: " + mactive12; }
            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "Active Item, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var mList = ctxTFAT.TaxMaster.Where(x => (x.Code == mModel.TaxMaster_Code)).FirstOrDefault();
            ctxTFAT.TaxMaster.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}