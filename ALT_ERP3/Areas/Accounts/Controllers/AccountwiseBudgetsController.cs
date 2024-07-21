using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class AccountwiseBudgetsController : BaseController
    {
        private string mauthorise = "A00";

        #region GetLists
        public List<SelectListItem> GetPrefixList()
        {
            List<SelectListItem> CallPrefixList = new List<SelectListItem>();
            CallPrefixList.Add(new SelectListItem { Value = "20042103", Text = "20042103" });
            CallPrefixList.Add(new SelectListItem { Value = "21042203", Text = "21042203" });
            CallPrefixList.Add(new SelectListItem { Value = "22042303", Text = "22042303" });
            CallPrefixList.Add(new SelectListItem { Value = "23042403", Text = "23042403" });
            CallPrefixList.Add(new SelectListItem { Value = "24042503", Text = "24042503" });
            CallPrefixList.Add(new SelectListItem { Value = "25042603", Text = "25042603" });
            CallPrefixList.Add(new SelectListItem { Value = "26042703", Text = "26042703" });
            CallPrefixList.Add(new SelectListItem { Value = "27042803", Text = "27042803" });
            CallPrefixList.Add(new SelectListItem { Value = "28042903", Text = "28042903" });
            CallPrefixList.Add(new SelectListItem { Value = "29043003", Text = "29043003" });
            return CallPrefixList;
        }

        public JsonResult AutoCompleteCode(string term)
        {
            if (term != null && term != "")
            {
                return Json((from m in ctxTFAT.Master
                             where m.Code.ToLower().Contains(term.ToLower()) || m.Name.ToLower().Contains(term.ToLower())
                             select new { Name = "[" + m.Code + "] " + m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json((from m in ctxTFAT.Master
                             select new { Name = "[" + m.Code + "] " + m.Name, Code = m.Code }).Take(10).ToArray(), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion GetLists


        // GET: Accounts/AccountwiseBudgets
        public ActionResult Index(AccountwiseBudgetsVM Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, "Account wise Budgets", "", DateTime.Now, 0, "", "","A");
            Model.PrefixList = GetPrefixList();
            Model.Budgets_RECORDKEY = Model.Document;
            if ((Model.Mode == "Edit") || (Model.Mode == "View") || (Model.Mode == "Delete"))
            {
                var mList = ctxTFAT.Budgets.Where(x => (x.RECORDKEY == Model.Document)).FirstOrDefault();
                if (mList != null)
                {
                    var mCode = ctxTFAT.Master.Where(x => x.Code == mList.Code).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    Model.Budgets_Code = mCode != null ? mCode.Code.ToString() : "";
                    Model.CodeName = mCode != null ? mCode.Name : "";
                    Model.Budgets_RECORDKEY = mList.RECORDKEY;
                    Model.Budgets_Prefix = mList.Prefix;
                    Model.Budgets_Annual = (decimal)(mList.Annual != null ? mList.Annual : 0);
                    Model.Budgets_Val1 = (decimal)(mList.Val1 != null ? mList.Val1 : 0);
                    Model.Budgets_Val2 = (decimal)(mList.Val2 != null ? mList.Val2 : 0);
                    Model.Budgets_Val3 = (decimal)(mList.Val3 != null ? mList.Val3 : 0);
                    Model.Budgets_Val4 = (decimal)(mList.Val4 != null ? mList.Val4 : 0);
                    Model.Budgets_Val5 = (decimal)(mList.Val5 != null ? mList.Val5 : 0);
                    Model.Budgets_Val6 = (decimal)(mList.Val6 != null ? mList.Val6 : 0);
                    Model.Budgets_Val7 = (decimal)(mList.Val7 != null ? mList.Val7 : 0);
                    Model.Budgets_Val8 = (decimal)(mList.Val8 != null ? mList.Val8 : 0);
                    Model.Budgets_Val9 = (decimal)(mList.Val9 != null ? mList.Val9 : 0);
                    Model.Budgets_Val10 = (decimal)(mList.Val10 != null ? mList.Val10 : 0);
                    Model.Budgets_Val11 = (decimal)(mList.Val11 != null ? mList.Val11 : 0);
                    Model.Budgets_Val12 = (decimal)(mList.Val12 != null ? mList.Val12 : 0);
                    Model.AUTHORISE = mList.AUTHORISE;
                    mauthorise = Model.AUTHORISE;
                }
            }
            // No ADD mode applicable
            return View(Model);
        }

        public ActionResult MoveNextPrevious(string Mode, string mdocument)
        {
            string mvar;
            if (Mode == "N")
            {
                mvar = Fieldoftable("Budgets", "Top 1 RECORDKEY", "RECORDKEY>'" + mdocument + "' order by RECORDKEY", "T") ?? "";
            }
            else
            {
                mvar = Fieldoftable("Budgets", "Top 1 RECORDKEY", "RECORDKEY<'" + mdocument + "' order by RECORDKEY desc", "T") ?? "";
            }
            return Json(new { Status = "Success", data = mvar }, JsonRequestBehavior.AllowGet);
        }

        #region SaveData
        public ActionResult SaveData(AccountwiseBudgetsVM Model)
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
                        return Json(new { Status = "Success", Message = "Data is Deleted.", NewSrl = Model.Budgets_RECORDKEY }, JsonRequestBehavior.AllowGet);
                    }
                    Budgets mobj = new Budgets();
                    bool mAdd = true;
                    if (ctxTFAT.Budgets.Where(x => (x.RECORDKEY == Model.Budgets_RECORDKEY)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.Budgets.Where(x => (x.RECORDKEY == Model.Budgets_RECORDKEY)).FirstOrDefault();
                        mAdd = false;
                    }
                    mobj.RECORDKEY = Model.Budgets_RECORDKEY;
                    mobj.Prefix = Model.Budgets_Prefix ?? "";
                    mobj.Code = Model.Budgets_Code ?? "";
                    mobj.Annual = Model.Budgets_Annual;
                    mobj.Val1 = Model.Budgets_Val1;
                    mobj.Val2 = Model.Budgets_Val2;
                    mobj.Val3 = Model.Budgets_Val3;
                    mobj.Val4 = Model.Budgets_Val4;
                    mobj.Val5 = Model.Budgets_Val5;
                    mobj.Val6 = Model.Budgets_Val6;
                    mobj.Val7 = Model.Budgets_Val7;
                    mobj.Val8 = Model.Budgets_Val8;
                    mobj.Val9 = Model.Budgets_Val9;
                    mobj.Val10 = Model.Budgets_Val10;
                    mobj.Val11 = Model.Budgets_Val11;
                    mobj.Val12 = Model.Budgets_Val12;
                    // iX9: default values for the fields not used @Form
                    mobj.AnnualQty = 0;
                    mobj.Area = 0;
                    mobj.Branch = mbranchcode;
                    mobj.BudgetType = "";
                    mobj.CompCode = mcompcode;
                    mobj.ControlBP = false;
                    mobj.ControlBS = false;
                    mobj.ControlBY = false;
                    mobj.ControlMCP = false;
                    mobj.ControlMCS = false;
                    mobj.ControlMCY = false;
                    mobj.ControlToleP = 0;
                    mobj.ControlToleS = 0;
                    mobj.ControlToleY = 0;
                    mobj.ControlTypeP = false;
                    mobj.ControlTypeS = false;
                    mobj.ControlTypeY = false;
                    mobj.CostCentre = "0";
                    mobj.Factor = 1;
                    mobj.Flag = "";
                    mobj.ItemCategory = 0;
                    mobj.ItemGroup = "";
                    mobj.LocationCode = 0;
                    mobj.Party = "";
                    mobj.Product = "";
                    mobj.Qty1 = 0;
                    mobj.Qty10 = 0;
                    mobj.Qty11 = 0;
                    mobj.Qty12 = 0;
                    mobj.Qty3 = 0;
                    mobj.Qty4 = 0;
                    mobj.Qty5 = 0;
                    mobj.Qty6 = 0;
                    mobj.Qty7 = 0;
                    mobj.Qty8 = 0;
                    mobj.Qty9 = 0;
                    mobj.Ratio1 = 0;
                    mobj.Ratio10 = 0;
                    mobj.Ratio11 = 0;
                    mobj.Ratio12 = 0;
                    mobj.Ratio2 = 0;
                    mobj.Ratio3 = 0;
                    mobj.Ratio4 = 0;
                    mobj.Ratio5 = 0;
                    mobj.Ratio6 = 0;
                    mobj.Ratio7 = 0;
                    mobj.Ratio8 = 0;
                    mobj.Ratio9 = 0;
                    mobj.Salesman = 0;
                    mobj.Sanctioned = 0;
                    mobj.SelectFlags = "";
                    mobj.Type = Model.Budgets_Type ?? "";
                    mobj.xValue1 = "";
                    mobj.xValue2 = "";
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = DateTime.Now;
                    mobj.AUTHORISE = "A00";
                    if (mAdd == true)
                    {
                        ctxTFAT.Budgets.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    string mNewCode = mobj.RECORDKEY.ToString();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "     " + mperiod.Substring(0, 2) + mobj.Code.ToString(), DateTime.Now, 0, "", "Save Account wise Budgets", "A");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { ex1.InnerException.Message, Status = "Error", id = "AccountwiseBudgets" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { ex.InnerException.Message, Status = "Error", id = "AccountwiseBudgets" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "AccountwiseBudgets" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", NewSrl = Model.Budgets_RECORDKEY, id = "AccountwiseBudgets" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteData(AccountwiseBudgetsVM Model)
        {
            DeUpdate(Model);
            return Json(new { Status = "Success", Message = "The Document is Deleted." }, JsonRequestBehavior.AllowGet);
        }

        private void DeUpdate(AccountwiseBudgetsVM Model)
        {
            var mList = ctxTFAT.Budgets.Where(x => (x.RECORDKEY == Model.Budgets_RECORDKEY)).ToList();
            if (mList != null)
            {
                ctxTFAT.Budgets.RemoveRange(mList);
            }
            ctxTFAT.SaveChanges();
        }
        #endregion SaveData
    }
}