using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
////using ALT_ERP3.DynamicBusinessLayer;
////using ALT_ERP3.DynamicBusinessLayer.Repository;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
//using PurchaseVM = ALT_ERP3.Areas.Accounts.Models.PurchaseVM;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class AccountMasterController : BaseController
    {
        //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        //IAddOnGridList AOlst = new AddonGridlist();

        private static string mmaintype = "";
        private static bool mAutoAccCode = false;
        private static byte mAutoAccStyle = 0;
        private static byte mAutoAccLength = 9;
        string attachmentPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentPath"].ToString()) == true ? "" : ConfigurationManager.AppSettings["AttachmentPath"].ToString();
        List<int> list = new List<int>();
        List<MasterVM> MailList1 = new List<MasterVM>();

        #region Get


        public JsonResult AutoCompleteCountry(string term)
        {
            return Json((from m in ctxTFAT.TfatCountry
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Name }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteState(string term)
        {
            var mPara = term.Split('^');
            var mP1 = mPara[0].ToLower();
            var mP2 = mPara[1];
            // linq doesnt support array in query, so parameters are stored in var
            return Json((from m in ctxTFAT.TfatState
                         where m.Country == mP2 && m.Name.ToLower().Contains(mP1)
                         select new { Name = m.Name, Code = m.Code.ToString() }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteCity(string term)
        {
            var mPara = term.Split('^');
            var mP1 = mPara[0].ToLower();
            var mP2 = mPara[1];
            // linq doesnt support array in query, so parameters are stored in var
            return Json((from m in ctxTFAT.TfatCity
                         where m.State == mP2 && m.Name.ToLower().Contains(mP1)
                         select new { Name = m.Name, Code = m.Code.ToString() }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteTfatArea(string term)
        {
            if (term != "")
            {
                var result = ctxTFAT.AreaMaster.Where(X => X.Name.Contains(term)).Select(m => new { Code = m.Code, Name = m.Name }).OrderBy(n => n.Name).ToList().Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var result = ctxTFAT.AreaMaster.Select(m => new { Code = m.Code, Name = m.Name }).OrderBy(n => n.Name).ToList().Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetPartyCategory(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.PartyCategory.Select(c => new { c.Code, c.Name }).Distinct().OrderBy(x => x.Name).ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteSalesMan(string term)
        {
            return Json((from m in ctxTFAT.SalesMan
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        //public ActionResult GetSalesMan(GridOption Model)
        //{
        //    var result = ctxTFAT.SalesMan.Where(x => x.Locked == false).Select(m => new { m.Code, m.Name }).Distinct().OrderBy(x => x.Name).ToList();
        //    foreach (var item in result)
        //    {
        //        SalesManName.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
        //    }

        //    return Json(SalesManName, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult AutoCompleteBroker(string term)
        {
            return Json((from m in ctxTFAT.Broker
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        //public JsonResult AutoCompletePriceList(string term)
        //{
        //    return Json((from m in ctxTFAT.PriceListHeader
        //                 where (m.Name.ToLower().Contains(term.ToLower()) || m.Code.ToLower().Contains(term.ToLower()))
        //                 select new { Name = m.Code, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult GetBroker(GridOption Model)
        //{
        //    var result = ctxTFAT.Broker.Where(x => x.Locked == false).Select(m => new { m.Code, m.Name }).Distinct().OrderBy(x => x.Name).ToList();
        //    foreach (var item in result)
        //    {
        //        Brokername.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
        //    }

        //    return Json(Brokername, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult GetCorrespondenceType(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.CorrespondenceType.Select(m => new { m.Code, m.Name }).Distinct().OrderBy(x => x.Name).ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDealerType(MasterVM Model)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "Tax Invoice" });
            GSt.Add(new SelectListItem { Value = "2", Text = "Reverse Charge" });
            GSt.Add(new SelectListItem { Value = "3", Text = "TCS" });
            GSt.Add(new SelectListItem { Value = "4", Text = "TDS" });
            GSt.Add(new SelectListItem { Value = "5", Text = "Bill Of Supply" });
            GSt.Add(new SelectListItem { Value = "6", Text = "Export( Under LUT)" });
            GSt.Add(new SelectListItem { Value = "7", Text = "UnRegistered Dealer" });
            GSt.Add(new SelectListItem { Value = "8", Text = "Sez with Payment" });
            GSt.Add(new SelectListItem { Value = "9", Text = "Sez w/0 Payment" });
            GSt.Add(new SelectListItem { Value = "10", Text = "Export (with Duty)" });
            GSt.Add(new SelectListItem { Value = "11", Text = "Exempted" });
            GSt.Add(new SelectListItem { Value = "12", Text = "No GST" });
            GSt.Add(new SelectListItem { Value = "13", Text = "Composition Dealer" });
            GSt.Add(new SelectListItem { Value = "14", Text = "Deemed Export" });
            GSt.Add(new SelectListItem { Value = "15", Text = "Nil Rated" });
            GSt.Add(new SelectListItem { Value = "16", Text = "Export @ 0.1%" });
            GSt.Add(new SelectListItem { Value = "17", Text = "Export @ 0.05%" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetHSNList(string term)
        {
            if (term != "")
            {
                var result = ctxTFAT.HSNMaster.Where(X => X.Name.Contains(term)).Select(m => new { Code = m.Code, Name = m.Name }).OrderBy(n => n.Name).ToList().Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.HSNMaster.Select(m => new { Code = m.Code, Name = m.Name }).OrderBy(n => n.Name).ToList().Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult GetIncoterm(GridOption Model)
        //{
        //    List<SelectListItem> mList = new List<SelectListItem>();
        //    var result = ctxTFAT.IncoTerms.Select(m => new { m.Code, m.Name }).Distinct().ToList();
        //    foreach (var item in result)
        //    {
        //        mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
        //    }

        //    return Json(mList, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult GetBankname(GridOption Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.BankMaster.Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTdsMaster(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.TDSMaster.Select(c => new { c.Name, c.Code }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPayTerms(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.PaymentTerms.Select(c => new { c.Name, c.Code }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDesignation(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.Designations.Select(c => new { c.Name, c.Code }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Getlanguage(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.Language.Select(c => new { c.Name, c.Code }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDepartments(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.Dept.Select(c => new { c.Name, c.Code }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetReligion(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.Religion.Select(c => new { c.Name, c.Code }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSegmentType(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.Segments.Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDivisions(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.Divisions.Select(c => new { c.Name, c.Code }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }
            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAccountGrps(string term)
        {
            if (term == "")
            {
                var result = ctxTFAT.MasterGroups.Where(x => x.Hide == false /*&& x.IsLast== true*/ && x.Code != x.Grp && x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).Take(10).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.MasterGroups.Where(x => x.Hide == false /*&& x.IsLast == true*/ && x.Code != x.Grp && x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetAccountGrp(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.MasterGroups.Where(x => x.Hide == false /*&& x.IsLast == true*/ && x.Code != x.Grp).Select(c => new { c.Code, c.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Getsalestaxcode()
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.TaxMaster.Where(x => x.Scope == "S").Select(c => new { c.Code, c.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Getpurchasetaxcode()
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.TaxMaster.Where(x => x.Scope == "P").Select(c => new { c.Code, c.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAccountOwner(TfatPass Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.TfatPass.Select(c => new { c.Code, c.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTranspoter(Master Model)
        {

            List<SelectListItem> StoreCodelst = new List<SelectListItem>();
            var result = ctxTFAT.Master.Where(x => x.Category == 100002).Select(m => new
            {
                m.Code,
                m.Name
            }).OrderBy(n => n.Name).ToList();
            foreach (var item in result)
            {
                StoreCodelst.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }
            return Json(StoreCodelst, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMailCategory(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.MailingCategory.Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTemplate(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.MsgTemplate.Select(c => new { Name = c.MsgText, c.Code }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteCurrency(string term)
        {
            return Json(from m in ctxTFAT.CurrencyMaster
                        select new { m.Name, m.Code }, JsonRequestBehavior.AllowGet);
        }

        public string GetGSTTypeName(string Val)
        {
            string Name;
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "Tax Invoice" });
            GSt.Add(new SelectListItem { Value = "2", Text = "Reverse Charge" });
            GSt.Add(new SelectListItem { Value = "3", Text = "TCS" });
            GSt.Add(new SelectListItem { Value = "4", Text = "TDS" });
            GSt.Add(new SelectListItem { Value = "5", Text = "Bill Of Supply" });
            GSt.Add(new SelectListItem { Value = "6", Text = "Export( Under LUT)" });
            GSt.Add(new SelectListItem { Value = "7", Text = "UnRegistered Dealer" });
            GSt.Add(new SelectListItem { Value = "8", Text = "SEZ with Payment" });
            GSt.Add(new SelectListItem { Value = "9", Text = "SEZ w/0 Payment" });
            GSt.Add(new SelectListItem { Value = "10", Text = "Export (with Duty)" });
            GSt.Add(new SelectListItem { Value = "11", Text = "Exempted" });
            GSt.Add(new SelectListItem { Value = "12", Text = "No GST" });
            GSt.Add(new SelectListItem { Value = "13", Text = "Composition Dealer" });
            GSt.Add(new SelectListItem { Value = "14", Text = "Deemed Export" });
            GSt.Add(new SelectListItem { Value = "15", Text = "NIL Rated" });
            GSt.Add(new SelectListItem { Value = "16", Text = "Export @ 0.1%" });
            GSt.Add(new SelectListItem { Value = "17", Text = "Export @ 0.05%" });
            Name = GSt.Where(x => x.Value == Val).Select(x => x.Text).FirstOrDefault();
            return Name;
        }

        public ActionResult GetGSTType(string term)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "Tax Invoice" });
            GSt.Add(new SelectListItem { Value = "2", Text = "Reverse Charge" });
            GSt.Add(new SelectListItem { Value = "3", Text = "TCS" });
            GSt.Add(new SelectListItem { Value = "4", Text = "TDS" });
            GSt.Add(new SelectListItem { Value = "5", Text = "Bill Of Supply" });
            GSt.Add(new SelectListItem { Value = "6", Text = "Export( Under LUT)" });
            GSt.Add(new SelectListItem { Value = "7", Text = "UnRegistered Dealer" });
            GSt.Add(new SelectListItem { Value = "8", Text = "SEZ with Payment" });
            GSt.Add(new SelectListItem { Value = "9", Text = "SEZ w/0 Payment" });
            GSt.Add(new SelectListItem { Value = "10", Text = "Export (with Duty)" });
            GSt.Add(new SelectListItem { Value = "11", Text = "Exempted" });
            GSt.Add(new SelectListItem { Value = "12", Text = "No GST" });
            GSt.Add(new SelectListItem { Value = "13", Text = "Composition Dealer" });
            GSt.Add(new SelectListItem { Value = "14", Text = "Deemed Export" });
            GSt.Add(new SelectListItem { Value = "15", Text = "NIL Rated" });
            GSt.Add(new SelectListItem { Value = "16", Text = "Export @ 0.1%" });
            GSt.Add(new SelectListItem { Value = "17", Text = "Export @ 0.05%" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPartyCrLmtType(string term)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "Warning Only" });
            GSt.Add(new SelectListItem { Value = "2", Text = "Restrict Entry" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetContactType(string term)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "Both" });
            GSt.Add(new SelectListItem { Value = "2", Text = "Delivery Address" });
            GSt.Add(new SelectListItem { Value = "3", Text = "Contact details" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPartyCrDayLmtType(string term)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "Warning Only" });
            GSt.Add(new SelectListItem { Value = "2", Text = "Restrict Entry" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetGSTItemType(string term)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "Service" });
            GSt.Add(new SelectListItem { Value = "2", Text = "Goods" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSourceDoc(string term)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "Service" });
            GSt.Add(new SelectListItem { Value = "2", Text = "Goods" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAcType(string term)
        {
            List<SelectListItem> maclist = new List<SelectListItem>();
            if (mmaintype == "D")
            {
                maclist.Add(new SelectListItem { Value = "D", Text = "D - Customers / Debtors" });
                maclist.Add(new SelectListItem { Value = "U", Text = "U - Debtors - Cum - Creditors" });
            }
            else if (mmaintype == "S")
            {
                maclist.Add(new SelectListItem { Value = "S", Text = "S - Supplier / Creditor" });
                maclist.Add(new SelectListItem { Value = "U", Text = "U - Debtors - Cum - Creditors" });
            }
            else
            {
                maclist.Add(new SelectListItem { Value = "B", Text = "B - Bank Accounts" });
                maclist.Add(new SelectListItem { Value = "C", Text = "C - Cash Accounts" });
                maclist.Add(new SelectListItem { Value = "F", Text = "F - Fixed Assets" });
                maclist.Add(new SelectListItem { Value = "H", Text = "H - Inter - Branch Accounts" });
                maclist.Add(new SelectListItem { Value = "O", Text = "O - Loans And Advance" });
                maclist.Add(new SelectListItem { Value = "P", Text = "P - Capital Accounts" });
                maclist.Add(new SelectListItem { Value = "R", Text = "R - Reserves & Surplus" });
                maclist.Add(new SelectListItem { Value = "T", Text = "T - Credit Card Accounts" });
                maclist.Add(new SelectListItem { Value = "V", Text = "V - Investment Accounts" });
                maclist.Add(new SelectListItem { Value = "A", Text = "A - Assets" });
                maclist.Add(new SelectListItem { Value = "L", Text = "L - Liabilities" });
                maclist.Add(new SelectListItem { Value = "I", Text = "I - Incomes" });
                maclist.Add(new SelectListItem { Value = "X", Text = "X - Expenses" });
            }
            return Json(maclist, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCreditRank(string term)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "1" });
            GSt.Add(new SelectListItem { Value = "2", Text = "2" });
            GSt.Add(new SelectListItem { Value = "3", Text = "3" });
            GSt.Add(new SelectListItem { Value = "4", Text = "4" });
            GSt.Add(new SelectListItem { Value = "5", Text = "5" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFrequencyOS(string term)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "Never Followup" });
            GSt.Add(new SelectListItem { Value = "2", Text = "Every n Days" });
            GSt.Add(new SelectListItem { Value = "3", Text = "Selected Weekdays" });
            GSt.Add(new SelectListItem { Value = "4", Text = "First Weekday of Each Month" });
            GSt.Add(new SelectListItem { Value = "5", Text = "First Date of Each Month" });
            GSt.Add(new SelectListItem { Value = "6", Text = "Weekday of 1 & 3 Weeks" });
            GSt.Add(new SelectListItem { Value = "7", Text = "Weekday of 2 & 4 Weeks" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFrequencyForm(string term)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "Never Followup" });
            GSt.Add(new SelectListItem { Value = "2", Text = "Every n Days" });
            GSt.Add(new SelectListItem { Value = "3", Text = "Selected Weekdays" });
            GSt.Add(new SelectListItem { Value = "4", Text = "First Weekday of Each Month" });
            GSt.Add(new SelectListItem { Value = "5", Text = "First Date of Each Month" });
            GSt.Add(new SelectListItem { Value = "6", Text = "Weekday of 1 & 3 Weeks" });
            GSt.Add(new SelectListItem { Value = "7", Text = "Weekday of 2 & 4 Weeks" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        private List<SelectListItem> PopulateBranches()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM TfatBranch";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            items.Add(new SelectListItem
                            {
                                Text = sdr["Name"].ToString(),
                                Value = sdr["Code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return items;
        }

        #endregion

        #region Fixed Asset
        public ActionResult SaveFixedAssets(MasterVM Model)
        {
            try
            {
                IList<MasterVM> Assets = new List<MasterVM>();

                if (Session["MFixedAssets"] != null)
                {
                    Assets = (List<MasterVM>)Session["MFixedAssets"];
                }

                Assets.Add(new MasterVM()
                {
                    AcCode = Model.AcCode,
                    PurchDate = Model.PurchDate,
                    UseDate = Model.UseDate,
                    LocationCode = Model.LocationCode,
                    AcDep = Model.AcDep,
                    Method = Model.Method,
                    Rate = Model.Rate,
                    IntRate = Model.IntRate,
                    CostPrice = Model.CostPrice,
                    BookValue = Model.BookValue,
                });
                Session.Add("MFixedAssets", Assets);

            }
            catch (DbEntityValidationException ex1)
            {

                string dd1 = ex1.InnerException.Message;
            }

            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditfixedAssets(MasterVM Model, string Code)
        {

            var mobj = ctxTFAT.Assets.Where(x => (x.Code) == Code).FirstOrDefault();
            Model.AcCode = mobj.AcCode;
            Model.BookValue = (decimal)mobj.BookValue;
            Model.CostPrice = (decimal)mobj.CostPrice;
            Model.LocationCode = (int)mobj.LocationCode;
            Model.AcDep = mobj.AcDep;
            Model.Method = mobj.Method;
            Model.PurchDate = (mobj.PurchDate == null) ? Convert.ToDateTime("1900-01-01") : mobj.PurchDate.Value;
            Model.UseDate = (mobj.UseDate == null) ? Convert.ToDateTime("1900-01-01") : mobj.UseDate.Value;
            Model.LocationCode = mlocationcode;
            Model.Rate = (decimal)mobj.Rate;
            Model.AcDep = mobj.AcDep;
            Model.Code = mobj.Code;
            return Json(new { Html = this.RenderPartialView("EditFixedAssetInfo", Model) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditFixedAssetsInfo(MasterVM Model)
        {
            try
            {
                var mobj1 = ctxTFAT.Assets.Where(x => x.Code == Model.Code).FirstOrDefault();
                mobj1.AUTHIDS = muserid;
                mobj1.AUTHORISE = "A00";
                mobj1.AcCode = Model.AcCode;
                mobj1.PurchDate = Model.PurchDate;
                mobj1.UseDate = Model.UseDate;
                mobj1.AcDep = Model.AcDep;
                mobj1.Method = Model.Method;
                mobj1.Rate = Model.Rate;
                mobj1.CostPrice = (decimal)Model.CostPrice;
                mobj1.BookValue = Model.BookValue;
                mobj1.Rate = Model.Rate;
                mobj1.Branch = mbranchcode;
                mobj1.Code = Model.Code;
                mobj1.ENTEREDBY = muserid;
                mobj1.LocationCode = mlocationcode;
                mobj1.LASTUPDATEDATE = DateTime.Now;
                ctxTFAT.Entry(mobj1).State = EntityState.Modified;
                ctxTFAT.SaveChanges();
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                ViewBag.Message = "Error, While updating the Data../n" + e.Message;
            }

            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region TreeView
        public string TreeView(string Mode, string Document)
        {
            string BranchCode = "";
            string[] BranchArray = new string[100];
            if (Mode == "Add")
            {
                BranchCode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
            }
            else
            {
                var Branchlist = ctxTFAT.Master.Where(x => x.Code == Document).Select(x => x.AppBranch).FirstOrDefault();
                BranchArray = Branchlist.ToString().Split(',');
            }

            var mTreeList = ctxTFAT.TfatBranch.Where(x => x.Category != "Area").Select(x => new { x.Name, x.Grp, x.Code, x.Category }).ToList();

            List<NFlatObject> flatObjects2 = new List<NFlatObject>();
            for (int n = 0; n < mTreeList.Count; n++)
            {
                string alias = "";
                NFlatObject abc = new NFlatObject();
                if (mTreeList[n].Category == "Zone")
                {
                    alias = mTreeList[n].Name + "-" + "Z";
                }
                else if (mTreeList[n].Category == "Branch")
                {
                    alias = mTreeList[n].Name + "-" + "B";
                }
                else if (mTreeList[n].Category == "SubBranch")
                {
                    alias = mTreeList[n].Name + "-" + "SB";
                }
                else
                {
                    alias = mTreeList[n].Name + "-" + "HO";
                }

                abc.data = alias;
                abc.Id = mTreeList[n].Code;
                if (Mode == "Add")
                {
                    if (BranchCode == abc.Id)
                    {
                        abc.isSelected = true;
                    }
                }
                else
                {
                    if (BranchArray.Contains(abc.Id))
                    {
                        abc.isSelected = true;
                    }
                }

                if (mTreeList[n].Code == mTreeList[n].Grp)
                {
                    abc.ParentId = "0";
                }
                else
                {
                    abc.ParentId = mTreeList[n].Grp;
                }
                flatObjects2.Add(abc);
            }
            var recursiveObjects = FillRecursive1(flatObjects2, "0");
            string myjsonmodel = new JavaScriptSerializer().Serialize(recursiveObjects);
            ViewBag.Tree = myjsonmodel;
            return myjsonmodel;

        }
        public string CheckUncheckTree(string Check)
        {
            var mTreeList = ctxTFAT.TfatBranch.Where(x => x.Category != "Area").Select(x => new { x.Name, x.Grp, x.Code, x.Category }).ToList();
            string alias = "";
            List<NFlatObject> flatObjects2 = new List<NFlatObject>();
            for (int n = 0; n < mTreeList.Count; n++)
            {
                NFlatObject abc = new NFlatObject();
                if (mTreeList[n].Category == "Zone")
                {
                    alias = mTreeList[n].Name + "-" + "Z";
                }
                else if (mTreeList[n].Category == "Branch")
                {
                    alias = mTreeList[n].Name + "-" + "B";
                }
                else if (mTreeList[n].Category == "SubBranch")
                {
                    alias = mTreeList[n].Name + "-" + "SB";
                }
                else
                {
                    alias = mTreeList[n].Name + "-" + "HO";
                }
                abc.data = alias;
                abc.Id = mTreeList[n].Code;
                if (Check == "Check")
                {
                    abc.isSelected = true;
                }
                if (mTreeList[n].Code == mTreeList[n].Grp)
                {
                    abc.ParentId = "0";
                }
                else
                {
                    abc.ParentId = mTreeList[n].Grp;
                }
                flatObjects2.Add(abc);
            }
            var recursiveObjects = FillRecursive1(flatObjects2, "0");
            string myjsonmodel = new JavaScriptSerializer().Serialize(recursiveObjects);
            ViewBag.Tree = myjsonmodel;
            return myjsonmodel;

        }
        public static List<NRecursiveObject> FillRecursive1(List<NFlatObject> flatObjects, string parentId)
        {
            bool mSelected = false;
            List<NRecursiveObject> recursiveObjects = new List<NRecursiveObject>();
            foreach (var item in flatObjects.Where(x => x.ParentId.Equals(parentId)))
            {
                mSelected = item.isSelected;
                recursiveObjects.Add(new NRecursiveObject
                {
                    data = item.data,
                    id = item.Id,
                    attr = new NFlatTreeAttribute { id = item.Id.ToString(), selected = item.isSelected },
                    children = FillRecursive1(flatObjects, item.Id)
                });
            }
            return recursiveObjects;
        }
        #endregion

        #region Save Index Edit

        // GET: Accounts/AccountMaster
        public ActionResult Index(MasterVM Model)
        {
            //GetAllMenu(Session["ModuleName"].ToString());
            ////AccountingGetAllMenu(Session["ModuleName"].ToString());
            ////VehicleGetAllMenu(Session["ModuleName"].ToString());
            GetAllMenu(Session["ModuleName"].ToString());
            List<MasterVM> AddressList1 = new List<MasterVM>();
            Session["MMailInfo"] = null;
            Session["MTempAccMasterAttach"] = null;
            Model.id = Model.Header;
            Model.Form15HDate = DateTime.Now;
            Model.Form15HCITDate = DateTime.Now;
            Model.ChildBdate = DateTime.Now;
            Model.SpouseBdate = DateTime.Now;
            Model.Anndate = DateTime.Now;
            Model.Bdate = DateTime.Now;
            Model.HoldDespatchDt = DateTime.Now;
            Model.HoldEnquiryDt = DateTime.Now;
            Model.HoldEnquiryDt = DateTime.Now;
            Model.HoldInvoiceDt = DateTime.Now;
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "","A");
            mmaintype = Model.MainType;
            Model.Branches = PopulateBranches();
            //Model.PriceLists = PopulatePriceLists();
            //Model.DiscLists = PopulateDiscLists();
            //Model.SchemeLists = PopulateSchemeLists();
            //
            List<SelectListItem> subledgerselect = new List<SelectListItem>();
            var result = ctxTFAT.SubLedger.Select(c => new { c.Code, c.Name }).ToList();
            foreach (var item in result)
            {
                subledgerselect.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }
            Model.SubLedgers = subledgerselect;

            // preferences from TfatBranch
            var mpara = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => new
            {
                x.gp_AutoAccCode,
                x.gp_AutoAccStyle,
                x.gp_AutoAccLength
            }).FirstOrDefault();
            mAutoAccCode = mpara.gp_AutoAccCode;
            mAutoAccStyle = (byte)(mpara.gp_AutoAccStyle == null ? 0 : mpara.gp_AutoAccStyle.Value);
            mAutoAccLength = (byte)(mpara.gp_AutoAccLength == null ? 6 : mpara.gp_AutoAccLength.Value);
            Model.AutoAccCode = mpara.gp_AutoAccCode;

            var countryname = ctxTFAT.TfatComp.Where(x => x.Code == mcompcode).Select(x => x.Country).FirstOrDefault();
            var statename = ctxTFAT.TfatComp.Where(x => x.Code == mcompcode).Select(x => x.State).FirstOrDefault();
            var cityname = ctxTFAT.TfatComp.Where(x => x.Code == mcompcode).Select(x => x.City).FirstOrDefault();

            if (Model.Mode == "Add")
            {
                if (Model.ViewDataId == "ReceivablesCustomers")
                {
                    Model.AcType = "D";
                }
                if (Model.ViewDataId == "PayablesSuppliers")
                {
                    Model.AcType = "S";
                }
                AddressList1.Add(new MasterVM
                {
                    Ifexist = true,
                    SrNo = 0,
                    AName = "",
                    CorpID = mcompcode,
                    Person = "",
                    Adrl1 = "",
                    Adrl2 = "",
                    Adrl3 = "",
                    Country = countryname,
                    CountryName = countryname,
                    State = statename,
                    StateName = statename,
                    City = cityname,
                    CityName = cityname,
                    Pin = "",
                    Area = 0,
                    AreaName = "",
                    Tel1 = "",
                    Fax = "",
                    Mobile = "",
                    www = "",
                    Email = "",
                    MailingCategory = 0,
                    UserID = muserid,
                    CorrespondenceType = 0,
                    Password = "",
                    Source = 0,
                    Segment = "",
                    STaxCode = "",
                    PTaxCode = "",
                    Licence1 = "",
                    Licence2 = "",
                    PanNo = "",
                    TINNo = "",
                    Designation = 0,
                    Language = 0,
                    Dept = 0,
                    Religion = 0,
                    Division = 0,
                    Bdate = DateTime.Now,
                    Anndate = DateTime.Now,
                    SpouseName = "",
                    SpouseBdate = DateTime.Now,
                    ChildName = "",
                    ChildBdate = DateTime.Now,
                    Code = "",
                    ContactType = "1",
                    AssistEmail = "",
                    AssistMobile = "",
                    AssistTel = "",
                    AssistName = "",
                    DefaultIGst = 0,
                    DefaultSGst = 0,
                    DefaultCGst = 0,
                    VATReg = "",
                    AadharNo = "",
                    GSTNo = "",
                    GSTType = "0",
                    PoisonLicense = "",
                    ReraNo = "",
                    DealerType = "0"
                });
                Model.AddressList = AddressList1;

                if (AddressList1 != null)
                {
                    Model.Ifexist = true;
                    Model.SrNo = 0;
                    Model.AName = "";
                    Model.CorpID = mcompcode;
                    Model.Person = "";
                    Model.Adrl1 = "";
                    Model.Adrl2 = "";
                    Model.Adrl3 = "";
                    Model.Country = countryname;
                    Model.CountryName = countryname;
                    Model.State = statename;
                    Model.StateName = statename;
                    Model.City = cityname;
                    Model.CityName = cityname;
                    Model.Pin = "";
                    Model.Area = 0;
                    Model.AreaName = "";
                    Model.Tel1 = "";
                    Model.Fax = "";
                    Model.Mobile = "";
                    Model.www = "";
                    Model.Email = "";
                    Model.MailingCategory = 0;
                    Model.UserID = muserid;
                    Model.CorrespondenceType = 0;
                    Model.Password = "";
                    Model.Source = 0;
                    Model.Segment = "";
                    Model.STaxCode = "";
                    Model.PTaxCode = "";
                    Model.Licence1 = "";
                    Model.Licence2 = "";
                    Model.PanNo = "";
                    Model.TINNo = "";
                    Model.Designation = 0;
                    Model.Language = 0;
                    Model.Dept = 0;
                    Model.Religion = 0;
                    Model.Division = 0;
                    Model.Bdate = DateTime.Now;
                    Model.Anndate = DateTime.Now;
                    Model.SpouseName = "";
                    Model.SpouseBdate = DateTime.Now;
                    Model.ChildName = "";
                    Model.ChildBdate = DateTime.Now;
                    Model.Code = "";
                    Model.ContactType = "1";
                    Model.AssistEmail = "";
                    Model.AssistMobile = "";
                    Model.AssistTel = "";
                    Model.AssistName = "";
                    Model.DefaultIGst = 0;
                    Model.DefaultSGst = 0;
                    Model.DefaultCGst = 0;
                    Model.VATReg = "";
                    Model.AadharNo = "";
                    Model.GSTNo = "";
                    Model.GSTType = "0";
                    Model.PoisonLicense = "";
                    Model.ReraNo = "";
                    Model.DealerType = "0";
                    Model.Tel2 = "";
                    Model.Tel3 = "";
                }

                Model.MailList = AddressList1;
                Session.Add("MMailInfo", AddressList1);

                List<AddOns> objitemlist = new List<AddOns>();
                var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%Master" && x.Hide == false)
                              select i).ToList();

                foreach (var i in addons)
                {
                    AddOns c = new AddOns();
                    c.Fld = i.Fld;
                    c.Head = i.Head;
                    c.ApplCode = "";
                    c.QueryText = i.QueryText == null ? "" : GetQueryText(i.QueryText);
                    c.FldType = i.FldType;
                    c.PlaceValue = i.PlaceValue;
                    c.Eqsn = i.Eqsn;
                    objitemlist.Add(c);
                }
                Model.AddOnList = objitemlist;
            }


            if (Model.Mode != "Add")
            {
                var master = ctxTFAT.Master.Where(x => x.Code == Model.Document).Select(x => x).FirstOrDefault();
                var masterinfo = ctxTFAT.MasterInfo.Where(x => x.Code == Model.Document).Select(x => x).FirstOrDefault();
                var holdtrx = ctxTFAT.HoldTransactions.Where(x => x.Code == Model.Document).Select(x => x).FirstOrDefault();
                var address = ctxTFAT.Address.Where(x => x.Code == Model.Document).Select(x => x).ToList();
                var FirstAddress = ctxTFAT.Address.Where(x => x.Code == Model.Document && x.Sno == 0).Select(x => x).FirstOrDefault();
                var taxdetails = ctxTFAT.TaxDetails.Where(x => x.Code == Model.Document).Select(x => x).FirstOrDefault();

                foreach (var mobj1 in address)
                {
                    AddressList1.Add(new MasterVM
                    {
                        Ifexist = true,
                        SrNo = mobj1.Sno,
                        AName = mobj1.Name,
                        CorpID = mobj1.CorpID,
                        Person = mobj1.Person,
                        Adrl1 = mobj1.Adrl1,
                        Adrl2 = mobj1.Adrl2,
                        Adrl3 = mobj1.Adrl3,
                        Country = mobj1.Country,
                        CountryName = mobj1.Country,
                        State = mobj1.State,
                        StateName = mobj1.State,
                        City = mobj1.City,
                        CityName = mobj1.City,
                        Pin = mobj1.Pin,
                        Area = (mobj1.Area == null) ? 0 : (int)mobj1.Area.Value,
                        AreaName = (mobj1.Area == null) ? "0" : GetAreaName(mobj1.Area),
                        Tel1 = mobj1.Tel1,
                        Fax = mobj1.Fax,
                        Mobile = mobj1.Mobile,
                        www = mobj1.www,
                        Email = mobj1.Email,
                        MailingCategory = (mobj1.MailingCategory == null) ? 0 : (int)mobj1.MailingCategory.Value,
                        UserID = mobj1.UserID,
                        CorrespondenceType = (mobj1.CorrespondenceType == null) ? 0 : (int)mobj1.CorrespondenceType.Value,
                        Password = mobj1.Password,
                        Source = (mobj1.Source == null) ? 0 : (int)mobj1.Source.Value,
                        Segment = mobj1.Segment,
                        STaxCode = mobj1.STaxCode,
                        PTaxCode = mobj1.PTaxCode,
                        Licence1 = mobj1.Licence1,
                        Licence2 = mobj1.Licence2,
                        PanNo = mobj1.PanNo,
                        TINNo = mobj1.TINNo,
                        Designation = (mobj1.Designation == null) ? 0 : (int)mobj1.Designation.Value,
                        Language = (mobj1.Language == null) ? 0 : (int)mobj1.Language.Value,
                        Dept = (mobj1.Dept == null) ? 0 : (int)mobj1.Dept.Value,
                        Religion = (mobj1.Religion == null) ? 0 : mobj1.Religion.Value,
                        Division = (mobj1.Division == null) ? 0 : mobj1.Division.Value,
                        Bdate = (mobj1.BDate == null) ? Convert.ToDateTime("1900-01-01") : mobj1.BDate.Value,
                        Anndate = (mobj1.AnnDate == null) ? Convert.ToDateTime("1900-01-01") : mobj1.BDate.Value,
                        SpouseName = mobj1.SpouseName,
                        SpouseBdate = (mobj1.SpouseBdate == null) ? Convert.ToDateTime("1900-01-01") : mobj1.BDate.Value,
                        ChildName = mobj1.ChildName,
                        ChildBdate = (mobj1.ChildBdate == null) ? Convert.ToDateTime("1900-01-01") : mobj1.BDate.Value,
                        Code = mobj1.Code,
                        ContactType = (mobj1.AddOrContact == null) ? "0" : mobj1.AddOrContact.Value.ToString(),
                        AssistEmail = mobj1.AssistEmail,
                        AssistMobile = mobj1.AssistMobile,
                        AssistTel = mobj1.AssistTel,
                        AssistName = mobj1.AssistName,
                        DefaultIGst = (mobj1.IGSTRate == null) ? 0 : (decimal)mobj1.IGSTRate.Value,
                        DefaultSGst = (mobj1.SGSTRate == null) ? 0 : (decimal)mobj1.SGSTRate.Value,
                        DefaultCGst = (mobj1.CGSTRate == null) ? 0 : (decimal)mobj1.CGSTRate.Value,
                        VATReg = mobj1.VATReg,
                        AadharNo = mobj1.AadharNo,
                        GSTNo = mobj1.GSTNo,
                        GSTType = (mobj1.GSTType == null) ? "0" : mobj1.GSTType.Value.ToString(),
                        PoisonLicense = "",
                        ReraNo = mobj1.ReraRegNo,
                        DealerType = (mobj1.DealerType == null) ? "0" : mobj1.DealerType.Value.ToString(),
                        Tel2 = mobj1.Tel2,
                        Tel3 = mobj1.Tel3
                    });
                    Model.AddressList = AddressList1;
                    Model.MailList = AddressList1;
                    Session.Add("MMailInfo", AddressList1);
                }

                if (FirstAddress != null)
                {
                    Model.SrNo = FirstAddress.Sno;
                    Model.AName = FirstAddress.Name;
                    Model.CorpID = FirstAddress.CorpID;
                    Model.Person = FirstAddress.Person;
                    Model.Adrl1 = FirstAddress.Adrl1;
                    Model.Adrl2 = FirstAddress.Adrl2;
                    Model.Adrl3 = FirstAddress.Adrl3;
                    Model.Country = FirstAddress.Country;
                    Model.CountryName = FirstAddress.Country;
                    //GetCountryName(FirstAddress.Country);
                    Model.State = FirstAddress.State;
                    Model.City = FirstAddress.City;
                    Model.StateName = FirstAddress.State;
                    Model.CityName = FirstAddress.City;
                    Model.Pin = FirstAddress.Pin;
                    Model.Area = (FirstAddress.Area == null) ? 0 : FirstAddress.Area.Value;
                    Model.AreaName = (FirstAddress.Area == null) ? "0" : GetAreaName(FirstAddress.Area);
                    Model.Tel1 = FirstAddress.Tel1;
                    Model.Fax = FirstAddress.Fax;
                    Model.Mobile = FirstAddress.Mobile;
                    Model.www = FirstAddress.www;
                    Model.Email = FirstAddress.Email;
                    Model.MailingCategory = (FirstAddress.MailingCategory == null) ? 0 : FirstAddress.MailingCategory.Value;
                    Model.UserID = FirstAddress.UserID;
                    Model.CorrespondenceType = (FirstAddress.CorrespondenceType == null) ? 0 : FirstAddress.CorrespondenceType.Value;
                    Model.Password = FirstAddress.Password;
                    Model.Source = (FirstAddress.Source == null) ? 0 : FirstAddress.Source.Value;
                    Model.Segment = FirstAddress.Segment;
                    Model.STaxCode = FirstAddress.STaxCode;
                    Model.PTaxCode = FirstAddress.PTaxCode;
                    Model.Licence1 = FirstAddress.Licence1;
                    Model.Licence2 = FirstAddress.Licence2;
                    Model.PanNo = FirstAddress.PanNo;
                    Model.TINNo = FirstAddress.TINNo;
                    Model.Designation = (FirstAddress.Designation == null) ? 0 : FirstAddress.Designation.Value;
                    Model.Language = (FirstAddress.Language == null) ? 0 : FirstAddress.Language.Value;
                    Model.Dept = (FirstAddress.Dept == null) ? 0 : FirstAddress.Dept.Value;
                    Model.Religion = (FirstAddress.Religion == null) ? 0 : FirstAddress.Religion.Value;
                    Model.Division = (FirstAddress.Division == null) ? 0 : FirstAddress.Division.Value;
                    Model.Bdate = (FirstAddress.BDate == null) ? Convert.ToDateTime("1900-01-01") : FirstAddress.BDate.Value;
                    Model.Anndate = (FirstAddress.AnnDate == null) ? Convert.ToDateTime("1900-01-01") : FirstAddress.BDate.Value;
                    Model.SpouseName = FirstAddress.SpouseName;
                    Model.SpouseBdate = (FirstAddress.SpouseBdate == null) ? Convert.ToDateTime("1900-01-01") : FirstAddress.BDate.Value;
                    Model.ChildName = FirstAddress.ChildName;
                    Model.ChildBdate = (FirstAddress.ChildBdate == null) ? Convert.ToDateTime("1900-01-01") : FirstAddress.BDate.Value;
                    Model.Code = FirstAddress.Code;
                    Model.ContactType = (FirstAddress.AddOrContact == null) ? "0" : FirstAddress.AddOrContact.Value.ToString();
                    Model.AssistEmail = FirstAddress.AssistEmail;
                    Model.AssistMobile = FirstAddress.AssistMobile;
                    Model.AssistTel = FirstAddress.AssistTel;
                    Model.AssistName = FirstAddress.AssistName;
                    Model.DefaultIGst = (FirstAddress.IGSTRate == null) ? 0 : FirstAddress.IGSTRate.Value;
                    Model.DefaultSGst = (FirstAddress.SGSTRate == null) ? 0 : FirstAddress.SGSTRate.Value;
                    Model.DefaultCGst = (FirstAddress.CGSTRate == null) ? 0 : FirstAddress.CGSTRate.Value;
                    Model.VATReg = FirstAddress.VATReg;
                    Model.AadharNo = FirstAddress.AadharNo;
                    Model.GSTNo = FirstAddress.GSTNo;
                    Model.GSTType = (FirstAddress.GSTType == null) ? "0" : FirstAddress.GSTType.Value.ToString();
                    Model.PoisonLicense = "";
                    Model.ReraNo = FirstAddress.ReraRegNo;
                    Model.DealerType = (FirstAddress.DealerType == null) ? "0" : FirstAddress.DealerType.Value.ToString();
                    Model.Tel2 = FirstAddress.Tel2;
                    Model.Tel3 = FirstAddress.Tel3;
                }
                Model.DraweeBank = FirstAddress.DraweeBank == null ? 0 : FirstAddress.DraweeBank.Value;
                Model.AddressList = AddressList1;
                Model.Name = master.Name;
                Model.ShortName = master.ShortName;
                Model.Name = master.Name;
                Model.Code = master.Code;
                Model.Grp = master.Grp;
                Model.GrpName = GetGrpName(master.Grp);
                Model.SalesMan = (int)(master.SalesMan != null ? master.SalesMan : 0);
                Model.SalesManName = GetSalesManName(master.SalesMan);
                Model.Broker = (int)(master.Broker != null ? master.Broker : 0);
                Model.BrokerName = GetBrokerName(master.Broker);
                Model.AcHeadCode = master.AcHeadCode;
                Model.AutoEmail = master.AutoEmail;
                Model.AutoSMS = master.AutoSMS;
                Model.IsPublic = master.IsPublic;
                Model.AcType = master.BaseGr;
                Model.Category = (master.Category == null) ? 0 : master.Category.Value;
                Model.CCBudget = master.CCBudget;
                Model.CCReqd = master.ForceCC;
                Model.ARAP = master.ARAP;
                Model.Hide = master.Hide;
                Model.NonActive = master.NonActive;
                Model.IsSubLedger = master.IsSubLedger;
                Model.AdminUser = master.UserID;
                Model.AppBranch = master.AppBranch;
                //Model.PONumber = master.PONumber;
                //Model.BENumber = master.BENumber;
                //Model.PartyChallan = master.PartyChallan;
                //Model.PartyInvoice = master.PartyInvoice;
                //Model.Collection = master.Collection;
                //Model.Delivery = master.Delivery;
                if (masterinfo != null)
                {
                    Model.PriceList = masterinfo.PriceList != null ? masterinfo.PriceList : "";
                    Model.FreqOS = (masterinfo.FreqOS == null) ? (byte)0 : Convert.ToByte(masterinfo.FreqOS.Value);
                    Model.FreqForm = (masterinfo.FreqForm == null) ? (byte)0 : Convert.ToByte(masterinfo.FreqForm.Value);
                    Model.EmailParty = masterinfo.EmailParty;
                    Model.EmailSalesman = masterinfo.EmailSalesman;
                    Model.SMSParty = masterinfo.SMSParty;
                    Model.SMSSalesman = masterinfo.SMSSalesman;
                    Model.CurrCode = masterinfo.CurrName.Value;
                    Model.CurrName = NameofAccount(masterinfo.CurrName, "C");
                    Model.RTGS = masterinfo.RTGS;
                    Model.Brokerage = (masterinfo.Brokerage == null) ? 0 : masterinfo.Brokerage.Value;
                    Model.Transporter = masterinfo.Transporter;
                    Model.ReminderFormat = masterinfo.ReminderFormat;
                    Model.Narr = masterinfo.Narr;
                    Model.IncoTerms = masterinfo.IncoTerms == null ? 0 : masterinfo.IncoTerms.Value;
                    Model.CheckCRDays = masterinfo.CheckCRDays;
                    Model.CheckCRLimit = masterinfo.CheckCRLimit;
                    Model.CRLimitWarn = (masterinfo.CRLimitWarn == true) ? 1 : 0;
                    Model.CRLimitWithTrx = masterinfo.CRLimitWithTrx;
                    Model.CRLimitWithPO = masterinfo.CRLimitWithPO;
                    Model.CRDaysWarn = (masterinfo.CRDaysWarn == true) ? 1 : 0;
                    Model.CashDisc = masterinfo.CashDisc;
                    Model.DiscDays = (masterinfo.DiscDays == null) ? 0 : masterinfo.DiscDays.Value;
                    Model.CrLimit = (masterinfo.CrLimit == null) ? 0 : masterinfo.CrLimit.Value;
                    Model.DiscPerc = (masterinfo.DiscPerc == null) ? 0 : masterinfo.DiscPerc.Value;
                    Model.CutTDS = masterinfo.CutTDS;
                    Model.TDSCode = (masterinfo.TDSCode == null) ? 0 : masterinfo.TDSCode.Value;
                    Model.CRLimitTole = (masterinfo.CRLimitTole == null) ? 0 : masterinfo.CRLimitTole.Value;
                    Model.CRPeriod = (masterinfo.CrPeriod == null) ? 0 : masterinfo.CrPeriod.Value;
                    Model.EmailPartyAlert = masterinfo.EmailPartyAlert;
                    Model.EmailTemplate = masterinfo.EmailTemplate;
                    Model.PaymentTerms = masterinfo.PaymentTerms;
                    Model.Rank = (masterinfo.Rank == null) ? (byte)0 : Convert.ToByte(masterinfo.Rank.Value);
                    Model.SMSTemplate = masterinfo.SMSTemplate;
                    Model.IncoPlace = masterinfo.IncoPlace;

                    Model.ODLimit = (masterinfo.ODLImit == null) ? 0 : masterinfo.ODLImit.Value;
                    Model.HSN = masterinfo.HSNCode;
                    Model.HSNName = GetHSNName(masterinfo.HSNCode);
                    Model.OptionGstType = (masterinfo.GSTType == null) ? "" : masterinfo.GSTType.Value.ToString();
                    Model.OptionGSTName = (masterinfo.GSTType == null) ? "" : GetGSTTypeName(masterinfo.GSTType.Value.ToString());
                    Model.AcCode = masterinfo.DrAcNo;
                    Model.SGST = (masterinfo.SGSTRate == null) ? 0 : masterinfo.SGSTRate.Value;
                    Model.IGST = (masterinfo.IGSTRate == null) ? 0 : masterinfo.IGSTRate.Value;
                    Model.CGST = (masterinfo.CGSTRate == null) ? 0 : masterinfo.CGSTRate.Value;
                    Model.GstApplicable = masterinfo.GSTFlag;
                    Model.ItemType = masterinfo.ItemType;
                    Model.EmailPartyAlert = masterinfo.EmailPartyAlert;
                    Model.SMSPartyAlert = masterinfo.SMSPartyAlert;
                    Model.IntRate = (masterinfo.IntRate == null) ? 0 : masterinfo.IntRate.Value;
                    Model.PDiscList = masterinfo.PriceDiscList;
                    Model.SchemeList = masterinfo.SchemeList;
                }
                if (holdtrx != null)
                {
                    Model.Ticklers = holdtrx.Ticklers;
                    Model.HoldDespatch = holdtrx.HoldDespatch;
                    Model.HoldEnquiry = holdtrx.HoldEnquiry;
                    Model.HoldInvoice = holdtrx.HoldInvoice;
                    Model.HoldDespatchDt = (holdtrx.HoldDespatchDt1 == null) ? Convert.ToDateTime("1900-01-01") : holdtrx.HoldDespatchDt1.Value;
                    Model.HoldEnquiryDt = (holdtrx.HoldEnquiryDt1 == null) ? Convert.ToDateTime("1900-01-01") : holdtrx.HoldEnquiryDt1.Value;
                    Model.HoldInvoiceDt = (holdtrx.HoldInvoiceDt1 == null) ? Convert.ToDateTime("1900-01-01") : holdtrx.HoldInvoiceDt1.Value;
                    Model.HoldNarr = holdtrx.HoldNarr;

                }
                if (taxdetails != null)
                {
                    Model.CutTCS = taxdetails.CutTCS;
                    Model.CutTDS = taxdetails.CutTDS;

                    Model.DifferRate = (decimal)taxdetails.DifferRate;
                    Model.DifferRateCertNo = taxdetails.DifferRateCertNo;

                    Model.Form15HCITDate = (taxdetails.Form15HCITDate == null) ? Convert.ToDateTime("1900-01-01") : taxdetails.Form15HCITDate.Value;
                    Model.Form15HDate = (taxdetails.Form15HDate == null) ? Convert.ToDateTime("1900-01-01") : taxdetails.Form15HDate.Value;
                    Model.IsDifferRate = taxdetails.IsDifferRate;
                    Model.IsForm15H = taxdetails.IsForm15H;

                    Model.TDSCode = (taxdetails.TDSCode == null) ? 0 : taxdetails.TDSCode.Value;
                }

                Model.AddOnList = GetAddOnListOnEdit(Model.Code);


                #region ATTACHMENT
                Model.DocumentList = GetAttachmentListInEdit(Model);
                string docstr = "";
                if (Model.DocumentList.Count > 0)
                {
                    foreach (var a in Model.DocumentList)
                    {
                        docstr = docstr + a.ImageStr + ",";
                    }
                    if (docstr != "")
                    {
                        docstr = docstr.Remove(docstr.Length - 1);
                    }
                    string docfilnam = "";
                    foreach (var b in Model.DocumentList)
                    {
                        docfilnam = docfilnam + b.FileName + ",";
                    }
                    if (docfilnam != "")
                    {
                        docfilnam = docfilnam.Remove(docfilnam.Length - 1);
                    }
                    Model.AllFileStr = docstr;
                    Model.FileNameStr = docfilnam;
                }
                Session["MTempAccMasterAttach"] = Model.DocumentList;
                #endregion
            }
            return View(Model);
        }

        [HttpPost]
        public ActionResult SaveData(MasterVM Model)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    if (Model.Mode == "Add")
                    {
                        var mailsession = (List<MasterVM>)Session["MMailInfo"];
                        if (mAutoAccCode == true)
                        {
                            Model.Code = GetCode(mAutoAccStyle, mAutoAccLength, Model.Grp);
                        }
                        else if (Model.Document == null || Model.Document.Trim() == "")
                        {
                            return Json(new { Status = "Error", Message = "Account Code is Required.." }, JsonRequestBehavior.AllowGet);
                        }
                        else if (mailsession.Count == 0 && (Model.BaseGr == "D" || Model.BaseGr == "S" || Model.BaseGr == "U"))
                        {
                            return Json(new { Status = "Error", Message = "Address Mail Info is Required.." }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            Model.Code = Model.Document;
                        }
                    }
                    else
                    {
                        Model.Code = Model.Document;
                    }
                    var delmasterinfo = ctxTFAT.MasterInfo.Where(x => x.Code == Model.Document).FirstOrDefault();
                    var deladdress = ctxTFAT.Address.Where(x => x.Code == Model.Document).ToList();
                    var delholdtrx = ctxTFAT.HoldTransactions.Where(x => x.Code == Model.Document).FirstOrDefault();
                    var deltaxdet = ctxTFAT.TaxDetails.Where(x => x.Code == Model.Document).FirstOrDefault();
                    var deltaddons = ctxTFAT.AddonMas.Where(x => x.TableKey == Model.Document).ToList();
                    //var deltdoc = ctxTFAT.Attachment.Where(x => x.ParentKey == "Master_" + Model.Document).ToList();
                    if (delmasterinfo != null)
                    {
                        ctxTFAT.MasterInfo.Remove(delmasterinfo);
                    }
                    if (deladdress != null)
                    {
                        ctxTFAT.Address.RemoveRange(deladdress);
                    }
                    if (delholdtrx != null)
                    {
                        ctxTFAT.HoldTransactions.Remove(delholdtrx);
                    }
                    if (deltaxdet != null)
                    {
                        ctxTFAT.TaxDetails.Remove(deltaxdet);
                    }
                    if (deltaddons.Count > 0)
                    {
                        ctxTFAT.AddonMas.RemoveRange(deltaddons);
                    }

                    //if (deltdoc.Count > 0)
                    //{
                    //    ctxTFAT.Attachment.RemoveRange(deltdoc);
                    //}
                    ctxTFAT.SaveChanges();

                    Master mobj = new Master();
                    if (Model.Mode == "Edit")
                    {
                        mobj = ctxTFAT.Master.Where(x => x.Code == Model.Code).Select(x => x).FirstOrDefault();
                    }
                    mobj.Code = Model.Code;
                    mobj.Grp = Model.Grp;
                    mobj.Name = Model.Name;
                    mobj.ForceCC = Model.CCReqd;
                    mobj.AcHeadCode = (Model.AcHeadCode == null) ? "" : Model.AcHeadCode;
                    mobj.AcType = (Model.AcType == null) ? "" : Model.AcType;
                    mobj.ARAP = Model.ARAP;
                    mobj.AUTHIDS = muserid;
                    mobj.AppBranch = Model.AppBranch == null ? mbranchcode : Model.AppBranch;
                    mobj.AUTHORISE = "A00";
                    mobj.BaseGr = (Model.AcType == null) ? "" : Model.AcType;
                    mobj.Category = (Model.Category == 0) ? 0 : Model.Category;
                    mobj.CCBudget = Model.CCBudget;
                    mobj.Hide = Model.Hide;
                    mobj.IsPublic = Model.IsPublic;
                    mobj.NonActive = Model.NonActive;
                    mobj.ShortName = (Model.ShortName == null) ? "" : Model.ShortName;
                    mobj.SalesMan = Model.SalesMan;
                    mobj.Broker = Model.Broker;
                    mobj.ENTEREDBY = muserid;
                    mobj.GroupTree = GetGroupTree(Model.Grp);
                    mobj.IsSubLedger = Model.IsSubLedger;
                    mobj.LASTUPDATEDATE = DateTime.Now;
                    mobj.UserID = (Model.AdminUser == null) ? "" : Model.AdminUser;
                    mobj.PORequired = Model.PONumber;
                    mobj.BERequired = Model.BENumber;
                    mobj.InvRequired = Model.PartyInvoice;
                    mobj.ChlnRequired = Model.PartyChallan;
                    mobj.Collection = Model.Collection;
                    mobj.Delivery = Model.Delivery;

                    if (Model.Mode == "Edit")
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    else
                    {
                        ctxTFAT.Master.Add(mobj);
                    }
                   
                    MasterInfo mobj2 = new MasterInfo();
                    mobj2.AppProduct = "";
                    mobj2.Area = null;
                    mobj2.AUTHIDS = muserid;
                    mobj2.AUTHORISE = "A00";
                    mobj2.BaseGr = (Model.AcType == null) ? "" : Model.AcType;
                    mobj2.Brokerage = 0;
                    mobj2.CashDisc = Model.CashDisc;
                    mobj2.CheckCRDays = Model.CheckCRDays;
                    mobj2.CheckCRLimit = Model.CheckCRLimit;
                    mobj2.Code = Model.Code;
                    mobj2.CompanyType = "";
                    mobj2.CostCentre = null;
                    mobj2.CRDaysWarn = (Model.CRDaysWarn == 0) ? false : true;
                    mobj2.CreatedOn = DateTime.Now;
                    mobj2.CrLimit = Model.CrLimit;
                    mobj2.CRLimitTole = Model.CRLimitTole;
                    mobj2.CRLimitWarn = (Model.CRLimitWarn == 0) ? false : true;
                    mobj2.CRLimitWithPO = Model.CRLimitWithPO;
                    mobj2.CRLimitWithTrx = Model.CRLimitWithTrx;
                    mobj2.CrPeriod = Convert.ToInt32(Model.CRPeriod);
                    mobj2.CurrName = Convert.ToInt32(Model.CurrCode);
                    mobj2.CutTDS = Model.CutTDS;
                    mobj2.DepricAC = "";
                    mobj2.DiscDays = Model.DiscDays;
                    mobj2.DiscPerc = Model.DiscPerc;
                    mobj2.EmailParty = Model.EmailParty;
                    mobj2.EmailPartyAlert = Model.EmailPartyAlert;
                    mobj2.EmailSalesman = Model.EmailSalesman;
                    mobj2.EmailTemplate = Model.EmailTemplate;
                    mobj2.EmailUsers = "";
                    mobj2.PriceList = Model.PriceList;
                    mobj2.FreqOS = (Model.FreqOS == null) ? 0 : Convert.ToInt32(Model.FreqOS);
                    mobj2.SGSTRate = Model.SGST;
                    mobj2.IGSTRate = Model.IGST;
                    mobj2.CGSTRate = Model.CGST;
                    mobj2.FreqForm = (Model.FreqForm == null) ? 0 : Convert.ToInt32(Model.FreqForm);
                    mobj2.Grp = Model.Grp;
                    mobj2.IntAmt = Convert.ToDecimal(0.00);
                    mobj2.IntRate = Model.IntRate;
                    mobj2.LastUpdateBy = muserid;

                    mobj2.LeadCode = "";
                    mobj2.LeadConvertDt = DateTime.Now;
                    mobj2.Name = Model.Name;
                    mobj2.Narr = (Model.Narr == null) ? "" : Model.Narr;
                    mobj2.PaymentTerms = (Model.PaymentTerms == null) ? "" : Model.PaymentTerms;
                    mobj2.Rank = (Model.Rank == null) ? 0 : Convert.ToInt32(Model.Rank);
                    mobj2.ReminderFormat = (Model.ReminderFormat == null) ? "" : Model.ReminderFormat;
                    mobj2.SMSTemplate = (Model.SMSTemplate == null) ? "" : Model.SMSTemplate;
                    mobj2.SMSUsers = "";
                    mobj2.SMSParty = Model.SMSParty;
                    mobj2.SMSSalesman = Model.SMSSalesman;
                    mobj2.IncoPlace = (Model.IncoPlace == null) ? "" : Model.IncoPlace;
                    mobj2.IncoTerms = (Model.IncoTerms == null) ? 0 : Model.IncoTerms;
                    mobj2.CurrName = Model.CurrCode;
                    mobj2.HSNCode = (Model.HSN == null) ? "" : Model.HSN;
                    mobj2.ReminderFormat = (Model.ReminderFormat == null) ? "" : Model.ReminderFormat;
                    mobj2.RTGS = (Model.RTGS == null) ? "" : Model.RTGS;
                    mobj2.TDSCode = (Model.TDSCode == null) ? 0 : Model.TDSCode;
                    mobj2.Transporter = (Model.Transporter == null) ? "" : Model.Transporter;
                    mobj2.xBranch = mbranchcode;
                    mobj2.ENTEREDBY = muserid;
                    mobj2.LastSent = DateTime.Now;
                    mobj2.ItemType = (Model.ItemType == null) ? "" : Model.ItemType;
                    mobj2.LocationCode = 100001;
                    mobj2.LASTUPDATEDATE = DateTime.Now;
                    mobj2.GSTType = (Model.OptionGstType == null) ? 0 : Convert.ToInt32(Model.OptionGstType);
                    mobj2.GSTFlag = Model.GstApplicable;
                    mobj2.ODLImit = Model.ODLimit;
                    mobj2.DrAcNo = (Model.AcCode == null) ? "" : Model.AcCode;
                    mobj2.SMSPartyAlert = Model.SMSPartyAlert;
                    mobj2.PriceDiscList = Model.PDiscList;
                    mobj2.SchemeList = Model.SchemeList;
                    ctxTFAT.MasterInfo.Add(mobj2);

                    HoldTransactions mobj4 = new HoldTransactions();
                    mobj4.AUTHIDS = muserid;
                    mobj4.AUTHORISE = "A00";
                    mobj4.CheckCRDays = Model.CheckCRDays;
                    mobj4.CheckCRLimit = Model.CheckCRLimit;
                    mobj4.ChkTempCRDays = false;
                    mobj4.ChkTempCRLimit = false;
                    mobj4.Code = Model.Code;
                    mobj4.CRDaysWarn = (Model.CRDaysWarn == 0) ? false : true;
                    mobj4.CRLimitWarn = (Model.CRLimitWarn == 0) ? false : true;
                    mobj4.CRLimitWithTrx = Model.CRLimitWithTrx;
                    mobj4.CRLimitWithPO = Model.CRLimitWithPO;
                    mobj4.CrPeriod = Convert.ToInt32(Model.CRPeriod);
                    mobj4.ENTEREDBY = muserid;
                    mobj4.HoldDespatch = Model.HoldDespatch;
                    mobj4.HoldDespatchDt1 = (Model.StrHoldDespatchDt == null || Model.StrHoldDespatchDt == "01-01-0001" || Model.StrHoldDespatchDt == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrHoldDespatchDt);
                    mobj4.HoldDespatchDt2 = DateTime.Now;
                    mobj4.HoldEnquiry = Model.HoldEnquiry;
                    mobj4.HoldEnquiryDt1 = (Model.StrHoldEnquiryDt == null || Model.StrHoldEnquiryDt == "01-01-0001" || Model.StrHoldEnquiryDt == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrHoldEnquiryDt);
                    mobj4.HoldEnquiryDt2 = DateTime.Now;
                    mobj4.HoldInvoice = Model.HoldInvoice;
                    mobj4.HoldInvoiceDt1 = (Model.StrHoldInvoiceDt == null || Model.StrHoldInvoiceDt == "01-01-0001" || Model.StrHoldInvoiceDt == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrHoldInvoiceDt);
                    mobj4.DocDate = DateTime.Now;
                    mobj4.HoldInvoiceDt2 = DateTime.Now;
                    mobj4.HoldNarr = Model.HoldNarr;
                    mobj4.HoldOrder = false;
                    mobj4.HoldOrderDt1 = DateTime.Now;
                    mobj4.HoldOrderDt2 = DateTime.Now;
                    mobj4.HoldPayment = false;
                    mobj4.HoldQuote = false;
                    mobj4.HoldQuoteDt1 = DateTime.Now;
                    mobj4.HoldQuoteDt2 = DateTime.Now; ;
                    mobj4.LASTUPDATEDATE = DateTime.Now;
                    mobj4.TempCrDayDt1 = DateTime.Now;
                    mobj4.TempCrDayDt2 = DateTime.Now;
                    mobj4.TempCrLimit = 0;
                    mobj4.TempCrLimitDt1 = DateTime.Now;
                    mobj4.TempCrLimitDt2 = DateTime.Now;
                    mobj4.TempCrPeriod = 0;
                    mobj4.TempRemark = "";
                    mobj4.Ticklers = Model.Ticklers;
                    ctxTFAT.HoldTransactions.Add(mobj4);

                    TaxDetails mobj3 = new TaxDetails();
                    mobj3.AUTHIDS = muserid;
                    mobj3.AUTHORISE = "A00";
                    mobj3.Code = Model.Code;
                    mobj3.CutTCS = Model.CutTCS;
                    mobj3.CutTDS = Model.CutTDS;
                    mobj3.Deductee = "";
                    mobj3.DifferRate = Model.DifferRate;
                    mobj3.DifferRateCertNo = (Model.DifferRateCertNo == null) ? "" : Model.DifferRateCertNo;
                    mobj3.ENTEREDBY = muserid;
                    mobj3.Form15HCITDate = (Model.StrForm15HCITDate == null || Model.StrForm15HCITDate == "01-01-0001" || Model.StrForm15HCITDate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrForm15HCITDate);
                    mobj3.Form15HDate = (Model.StrForm15HDate == null || Model.StrForm15HDate == "01-01-0001" || Model.StrForm15HDate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrForm15HDate);
                    mobj3.IsDifferRate = Model.IsDifferRate;
                    mobj3.IsForm15H = Model.IsForm15H;
                    mobj3.LASTUPDATEDATE = DateTime.Now;
                    mobj3.LocationCode = 100001;

                    mobj3.TDSCode = (Model.TDSCode == null) ? 0 : Convert.ToInt32(Model.TDSCode);
                    ctxTFAT.TaxDetails.Add(mobj3);

                    if (Session["MMailInfo"] != null)
                    {
                        var mailinformation = (List<MasterVM>)Session["MMailInfo"];
                        if (mailinformation.Count == 1)
                        {
                            Address mobj1 = new Address();
                            mobj1.AddOrContact = (Model.ContactType == null || Model.ContactType.Trim() == "") ? 0 : Convert.ToInt32(Model.ContactType);
                            mobj1.Adrl1 = (Model.Adrl1 == null) ? "" : Model.Adrl1;
                            mobj1.Adrl2 = (Model.Adrl2 == null) ? "" : Model.Adrl2;
                            mobj1.Adrl3 = (Model.Adrl3 == null) ? "" : Model.Adrl3;
                            mobj1.Adrl4 = "";
                            mobj1.AnnDate = (Model.StrAnndate == null || Model.StrAnndate == "01-01-0001" || Model.StrAnndate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrAnndate);
                            mobj1.Area = Model.Area;
                            mobj1.AssistEmail = (Model.AssistEmail == null) ? "" : Model.AssistEmail;
                            mobj1.AssistMobile = (Model.AssistMobile == null) ? "" : Model.AssistMobile;
                            mobj1.AssistName = (Model.AssistName == null) ? "" : Model.AssistName;
                            mobj1.AssistTel = (Model.AssistTel == null) ? "" : Model.AssistTel;
                            mobj1.AUTHIDS = muserid;
                            mobj1.AUTHORISE = "A00";
                            mobj1.BaseGr = (Model.AcType == null) ? "" : Model.AcType;
                            mobj1.BDate = (Model.Budate == null || Model.Budate == "01-01-0001" || Model.Budate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.Budate);
                            mobj1.ChildBdate = (Model.StrChildBdate == null || Model.StrChildBdate == "01-01-0001" || Model.StrChildBdate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrChildBdate);
                            mobj1.ChildName = (Model.ChildName == null) ? "" : Model.ChildName;
                            mobj1.City = (Model.City == null) ? "" : Model.City;
                            mobj1.Code = Model.Code;
                            mobj1.CorpID = (Model.CorpID == null) ? "" : Model.CorpID;
                            mobj1.CorrespondenceType = Model.CorrespondenceType;
                            mobj1.Country = (Model.Country == null) ? "" : Model.Country;
                            mobj1.Dept = Model.Dept;
                            mobj1.Designation = Model.Designation;
                            mobj1.Division = Model.Division;
                            mobj1.DraweeBank = Model.DraweeBank;
                            mobj1.Email = (Model.Email == null) ? "" : Model.Email;
                            mobj1.ENTEREDBY = muserid;
                            mobj1.Fax = (Model.Fax == null) ? "" : Model.Fax;
                            mobj1.Language = Model.Language;
                            mobj1.LASTUPDATEDATE = DateTime.Now;
                            mobj1.Licence1 = (Model.Licence1 == null) ? "" : Model.Licence1;
                            mobj1.Licence2 = (Model.Licence2 == null) ? "" : Model.Licence2;
                            mobj1.LocationCode = mlocationcode;
                            mobj1.MailingCategory = Model.MailingCategory;
                            mobj1.Mobile = (Model.Mobile == null) ? "" : Model.Mobile;
                            mobj1.Name = (Model.AName == null) ? "" : Model.AName;
                            mobj1.PanNo = (Model.PanNo == null) ? "" : Model.PanNo;
                            mobj1.Password = (Model.Password == null) ? "" : Model.Password;
                            mobj1.Person = (Model.Person == null) ? "" : Model.Person;
                            mobj1.PhotoPath = "";
                            mobj1.Pin = (Model.Pin == null) ? "" : Model.Pin;
                            mobj1.PTaxCode = (Model.PTaxCode == null) ? "" : Model.PTaxCode;
                            mobj1.STaxCode = (Model.STaxCode == null) ? "" : Model.STaxCode;
                            mobj1.Religion = Model.Religion;
                            mobj1.Segment = (Model.Segment == null) ? "" : Model.Segment;
                            mobj1.Sno = Convert.ToInt32(Model.SrNo);
                            mobj1.Source = Model.Source;
                            mobj1.SpouseBdate = (Model.SpouseBudate == null || Model.SpouseBudate == "01-01-0001" || Model.SpouseBudate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.SpouseBudate);
                            mobj1.SpouseName = (Model.SpouseName == null) ? "" : Model.SpouseName;
                            mobj1.State = (Model.State == null) ? "" : Model.State;
                            mobj1.Tel1 = (Model.Tel1 == null) ? "" : Model.Tel1;
                            mobj1.Tel2 = (Model.Tel2 == null) ? "" : Model.Tel2;
                            mobj1.Tel3 = (Model.Tel3 == null) ? "" : Model.Tel3;
                            mobj1.Tel4 = "";
                            mobj1.TINNo = (Model.TINNo == null) ? "" : Model.TINNo;
                            mobj1.UserID = (Model.UserID == null) ? "" : Model.UserID;
                            mobj1.www = (Model.www == null) ? "" : Model.www;
                            mobj1.AadharNo = (Model.AadharNo == null) ? "" : Model.AadharNo;
                            mobj1.GSTNo = (Model.GSTNo == null) ? "" : Model.GSTNo;
                            mobj1.IGSTRate = Model.DefaultIGst;
                            mobj1.CGSTRate = Model.DefaultCGst;
                            mobj1.SGSTRate = Model.DefaultSGst;
                            mobj1.GSTType = (Model.GSTType == null || Model.GSTType.Trim() == "") ? 0 : Convert.ToInt32(Model.GSTType);
                            mobj1.DealerType = (Model.DealerType == null || Model.DealerType.Trim() == "") ? (byte)0 : Convert.ToByte(Model.DealerType);
                            mobj1.VATReg = (Model.VATReg == null) ? "" : Model.VATReg;
                            mobj1.ReraRegNo = (Model.ReraNo == null) ? "" : Model.ReraNo;
                            ctxTFAT.Address.Add(mobj1);
                        }
                        else
                        {
                            foreach (var item in mailinformation)
                            {
                                Address mobj1 = new Address();
                                mobj1.AddOrContact = (item.ContactType == null || item.ContactType.Trim() == "") ? 0 : Convert.ToInt32(item.ContactType);
                                mobj1.Adrl1 = (item.Adrl1 == null) ? "" : item.Adrl1;
                                mobj1.Adrl2 = (item.Adrl2 == null) ? "" : item.Adrl2;
                                mobj1.Adrl3 = (item.Adrl3 == null) ? "" : item.Adrl3;
                                mobj1.Adrl4 = "";
                                mobj1.AnnDate = (item.Anndate == null) ? DateTime.Now : item.Anndate;
                                mobj1.Area = item.Area;
                                mobj1.AssistEmail = (item.AssistEmail == null) ? "" : item.AssistEmail;
                                mobj1.AssistMobile = (item.AssistMobile == null) ? "" : item.AssistMobile;
                                mobj1.AssistName = (item.AssistName == null) ? "" : item.AssistName;
                                mobj1.AssistTel = (item.AssistTel == null) ? "" : item.AssistTel;
                                mobj1.AUTHIDS = muserid;
                                mobj1.AUTHORISE = "A00";
                                mobj1.BaseGr = (Model.AcType == null) ? "" : Model.AcType;
                                mobj1.BDate = (item.Bdate == null) ? DateTime.Now : item.Bdate;
                                mobj1.ChildBdate = (item.ChildBdate == null) ? DateTime.Now : item.ChildBdate;
                                mobj1.ChildName = (item.ChildName == null) ? "" : item.ChildName;
                                mobj1.City = (item.City == null) ? "" : item.City;
                                mobj1.Code = Model.Code;
                                mobj1.CorpID = (item.CorpID == null) ? "" : item.CorpID;
                                mobj1.CorrespondenceType = item.CorrespondenceType;
                                mobj1.Country = (item.Country == null) ? "" : item.Country;
                                mobj1.Dept = item.Dept;
                                mobj1.Designation = item.Designation;
                                mobj1.Division = item.Division;
                                mobj1.DraweeBank = Model.DraweeBank;
                                mobj1.Email = (item.Email == null) ? "" : item.Email;
                                mobj1.ENTEREDBY = muserid;
                                mobj1.Fax = (item.Fax == null) ? "" : item.Fax;
                                mobj1.Language = item.Language;
                                mobj1.LASTUPDATEDATE = DateTime.Now;
                                mobj1.Licence1 = (item.Licence1 == null) ? "" : item.Licence1;
                                mobj1.Licence2 = (item.Licence2 == null) ? "" : item.Licence2;
                                mobj1.LocationCode = mlocationcode;
                                mobj1.MailingCategory = item.MailingCategory;
                                mobj1.Mobile = (item.Mobile == null) ? "" : item.Mobile;
                                mobj1.Name = (item.AName == null) ? "" : item.AName;
                                mobj1.PanNo = (item.PanNo == null) ? "" : item.PanNo;
                                mobj1.Password = (item.Password == null) ? "" : item.Password;
                                mobj1.Person = (item.Person == null) ? "" : item.Person;
                                mobj1.PhotoPath = "";
                                mobj1.Pin = (item.Pin == null) ? "" : item.Pin;
                                mobj1.PTaxCode = (item.PTaxCode == null) ? "" : item.PTaxCode;
                                mobj1.STaxCode = (item.STaxCode == null) ? "" : item.STaxCode;
                                mobj1.Religion = item.Religion;
                                mobj1.Segment = (item.Segment == null) ? "" : item.Segment;
                                mobj1.Sno = Convert.ToInt32(item.SrNo);
                                mobj1.Source = item.Source;
                                mobj1.SpouseBdate = (item.SpouseBdate == null) ? DateTime.Now : item.SpouseBdate;
                                mobj1.SpouseName = (item.SpouseName == null) ? "" : item.SpouseName;
                                mobj1.State = (item.State == null) ? "" : item.State;
                                mobj1.Tel1 = (item.Tel1 == null) ? "" : item.Tel1;
                                mobj1.Tel2 = (item.Tel2 == null) ? "" : item.Tel2;
                                mobj1.Tel3 = (item.Tel3 == null) ? "" : item.Tel3;
                                mobj1.Tel4 = "";
                                mobj1.TINNo = (item.TINNo == null) ? "" : item.TINNo;
                                mobj1.UserID = (item.UserID == null) ? "" : item.UserID;
                                mobj1.www = (item.www == null) ? "" : item.www;
                                mobj1.AadharNo = (item.AadharNo == null) ? "" : item.AadharNo;
                                mobj1.GSTNo = (item.GSTNo == null) ? "" : item.GSTNo;
                                mobj1.IGSTRate = item.DefaultIGst;
                                mobj1.CGSTRate = item.DefaultCGst;
                                mobj1.SGSTRate = item.DefaultSGst;
                                mobj1.GSTType = (item.GSTType == null || item.GSTType.Trim() == "") ? 0 : Convert.ToInt32(item.GSTType);
                                mobj1.DealerType = (item.DealerType == null || item.DealerType.Trim() == "") ? (byte)0 : Convert.ToByte(item.DealerType);
                                mobj1.VATReg = (item.VATReg == null) ? "" : item.VATReg;
                                mobj1.ReraRegNo = (item.ReraNo == null) ? "" : item.ReraNo;
                                ctxTFAT.Address.Add(mobj1);
                            }
                        }
                    }

                    if (Session["MFixedAssets"] != null)
                    {
                        var FixedAssets = (List<MasterVM>)Session["MFixedAssets"];
                        if (FixedAssets.Count != 0)
                        {
                            foreach (var item in FixedAssets)
                            {
                                Assets mobj1 = new Assets();
                                mobj1.AUTHORISE = "A00";
                                mobj1.Code = Model.Code;
                                mobj1.Branch = mbranchcode;
                                mobj1.AUTHIDS = muserid;
                                mobj1.Store = 100001;
                                mobj1.LocationCode = mlocationcode;
                                mobj1.AcDep = "";
                                mobj1.Method = item.Method;
                                mobj1.Rate = 1;
                                mobj1.AcCode = item.AcCode;
                                mobj1.BookValue = item.BookValue;
                                mobj1.CostPrice = item.CostPrice;
                                mobj1.PurchDate = item.PurchDate;
                                mobj1.UseDate = item.UseDate;
                                mobj1.LASTUPDATEDATE = DateTime.Now;
                                mobj1.ENTEREDBY = muserid;
                                ctxTFAT.Assets.Add(mobj1);
                            }
                        }
                    }

                    SaveAddons(Model);
                    if (Model.Mode == "Add")
                    {
                        SaveAttachment(Model);
                    }
                    ctxTFAT.SaveChanges();
                    //UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, Model.Code, DateTime.Now, 0, Model.Code, "");
                    UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, mobj.Code, "","A");
                    //int n = ctxTFAT.Database.ExecuteSqlCommand("Update Master Set GroupTree = dbo.fn_GetGroupTree(Grp)");
                    transaction.Commit();
                    transaction.Dispose();
                    Session["MTempAccMasterAttach"] = null;
                    Session["MMailInfo"] = null;
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();

                    return Json(new { Message = ex1.EntityValidationErrors.Select(x => x.ValidationErrors.Select(xa => xa.ErrorMessage).FirstOrDefault()), Status = "Error" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.InnerException.Message, Status = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Mail

        public ActionResult GetMailInfo(MasterVM Model)
        {
            var result = (List<MasterVM>)Session["MMailInfo"];
            var result1 = result.Where(x => x.SrNo == Model.SrNo);
            foreach (var item in result1)
            {
                Model.AName = item.AName;
                Model.CorpID = item.CorpID;
                Model.Person = item.Person;
                Model.Adrl1 = item.Adrl1;
                Model.Adrl2 = item.Adrl2;
                Model.Adrl3 = item.Adrl3;
                Model.Country = item.Country;
                Model.CountryName = item.CountryName;
                Model.State = item.State;
                Model.StateName = item.StateName;
                Model.City = item.City;
                Model.CityName = item.CityName;
                Model.Pin = item.Pin;
                Model.Area = item.Area;
                Model.AreaName = GetAreaName(item.Area);
                Model.Tel1 = item.Tel1;
                Model.Fax = item.Fax;
                Model.Mobile = item.Mobile;
                Model.www = item.www;
                Model.Email = item.Email;
                Model.MailingCategory = item.MailingCategory;
                Model.UserID = item.UserID;
                Model.CorrespondenceType = item.CorrespondenceType;
                Model.Password = item.Password;
                Model.Source = item.Source;
                Model.Segment = item.Segment;
                Model.STaxCode = item.STaxCode;
                Model.PTaxCode = item.PTaxCode;
                Model.Licence1 = item.Licence1;
                Model.Licence2 = item.Licence2;
                Model.PanNo = item.PanNo;
                Model.GSTNo = item.GSTNo;
                //Model.CSTNo = item.CSTNo;
                Model.VATReg = item.VATReg;
                Model.TINNo = item.TINNo;
                Model.Designation = item.Designation;
                Model.Language = item.Language;
                Model.Dept = item.Dept;
                Model.Religion = item.Religion;
                Model.Division = item.Division;
                Model.Bdate = item.Bdate;
                Model.Anndate = item.Anndate;
                Model.SpouseName = item.SpouseName;
                Model.SpouseBdate = item.SpouseBdate;
                Model.ChildName = item.ChildName;
                Model.ChildBdate = item.ChildBdate;
                Model.SrNo = item.SrNo;
                Model.ContactType = item.ContactType;
                Model.GSTType = item.GSTType;
                Model.DealerType = item.DealerType;
                Model.DefaultCGst = item.DefaultCGst;
                Model.DefaultSGst = item.DefaultSGst;
                Model.DefaultIGst = item.DefaultIGst;
                Model.AadharNo = item.AadharNo;
                Model.PoisonLicense = item.PoisonLicense;
                Model.ReraNo = item.ReraNo;
                Model.AssistEmail = item.AssistEmail;
                Model.AssistMobile = item.AssistMobile;
                Model.AssistName = item.AssistName;
                Model.AssistTel = item.AssistTel;
                Model.Tel2 = item.Tel2;
                Model.Tel3 = item.Tel3;
            }
            Model.MailList = result;
            return Json(new { Html = this.RenderPartialView("MailingInfo", Model) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveMailInfo(MasterVM Model)
        {
            int srno;
            List<MasterVM> MailInformation = new List<MasterVM>();
            try
            {
                if (Session["MMailInfo"] != null)
                {
                    MailInformation = (List<MasterVM>)Session["MMailInfo"];
                }
                var countryname = ctxTFAT.TfatComp.Where(x => x.Code == mcompcode).Select(x => x.Country).FirstOrDefault();

                var statename = ctxTFAT.TfatComp.Where(x => x.Code == mcompcode).Select(x => x.State).FirstOrDefault();

                var cityname = ctxTFAT.TfatComp.Where(x => x.Code == mcompcode).Select(x => x.City).FirstOrDefault();
                srno = MailInformation.Count();
                MailInformation.Add(new MasterVM()
                {
                    Ifexist = true,
                    SrNo = srno,
                    AName = "",
                    CorpID = "",
                    Person = "",
                    Adrl1 = "",
                    Adrl2 = "",
                    Adrl3 = "",
                    Country = countryname,
                    CountryName = countryname,
                    State = statename,
                    StateName = statename,
                    City = cityname,
                    CityName = cityname,
                    Pin = "",
                    Area = 0,
                    AreaName = "",
                    Tel1 = "",
                    Fax = "",
                    Mobile = "",
                    www = "",
                    Email = "",
                    MailingCategory = 0,
                    UserID = "",
                    CorrespondenceType = 0,
                    Password = "",
                    Source = 0,
                    Segment = "",
                    STaxCode = "",
                    PTaxCode = "",
                    Licence1 = "",
                    Licence2 = "",
                    PanNo = "",
                    TINNo = "",
                    Designation = 0,
                    Language = 0,
                    Dept = 0,
                    Religion = 0,
                    Division = 0,
                    Bdate = DateTime.Now,
                    Anndate = DateTime.Now,
                    SpouseName = "",
                    SpouseBdate = DateTime.Now,
                    ChildName = "",
                    ChildBdate = DateTime.Now,
                    Code = "",
                    ContactType = "",
                    AssistEmail = "",
                    AssistMobile = "",
                    AssistTel = "",
                    AssistName = "",
                    DefaultIGst = 0,
                    DefaultSGst = 0,
                    DefaultCGst = 0,
                    VATReg = "",
                    AadharNo = "",
                    GSTNo = "",
                    GSTType = "0",
                    PoisonLicense = "",
                    ReraNo = "",
                    DealerType = "0",
                    Tel2 = "",
                    Tel3 = ""
                });
                Session.Add("MMailInfo", MailInformation);
                Model.MailList = MailInformation;


            }
            catch (DbEntityValidationException ex1)
            {
                string dd1 = ex1.InnerException.Message;
            }
            var html = ViewHelper.RenderPartialView(this, "MailingInfo", new MasterVM() { MailList = MailInformation });
            return Json(new { MailList = MailInformation, Html = html }, JsonRequestBehavior.AllowGet);
            //return Json(new { Html = this.RenderPartialView("MailingInfo", Model) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditMailData(MasterVM Model)
        {
            List<MasterVM> MailInformation1 = new List<MasterVM>();
            List<MasterVM> MailInformation = new List<MasterVM>();
            try
            {
                if (Session["MMailInfo"] != null)
                {
                    MailInformation = (List<MasterVM>)Session["MMailInfo"];
                }
                foreach (var item in MailInformation.Where(x => x.SrNo == Model.SrNo))
                {
                    item.AName = Model.AName;
                    item.CorpID = Model.CorpID;
                    item.Person = Model.Person;
                    item.Adrl1 = Model.Adrl1;
                    item.Adrl2 = Model.Adrl2;
                    item.Adrl3 = Model.Adrl3;
                    item.Country = Model.Country;
                    item.CountryName = Model.Country;
                    Model.CountryName = Model.Country;
                    item.State = Model.State;
                    item.StateName = Model.State;
                    Model.StateName = Model.State;

                    item.City = Model.City;
                    Model.CityName = Model.City;
                    item.CityName = Model.City;
                    item.Pin = Model.Pin;
                    item.Area = Model.Area;
                    Model.AreaName = GetAreaName(Model.Area);
                    item.Tel1 = Model.Tel1;
                    item.Fax = Model.Fax;
                    item.Mobile = Model.Mobile;
                    item.www = Model.www;
                    item.Email = Model.Email;
                    item.MailingCategory = Model.MailingCategory;
                    item.UserID = Model.UserID;
                    item.CorrespondenceType = Model.CorrespondenceType;
                    item.Password = Model.Password;
                    item.Source = Model.Source;
                    item.Segment = Model.Segment;
                    item.STaxCode = Model.STaxCode;
                    item.PTaxCode = Model.PTaxCode;
                    item.Licence1 = Model.Licence1;
                    item.Licence2 = Model.Licence2;
                    item.PanNo = Model.PanNo;
                    item.GSTNo = Model.GSTNo;
                    //CSTNo = Model.CSTNo;
                    item.VATReg = Model.VATReg;
                    item.TINNo = Model.TINNo;
                    item.Designation = Model.Designation;
                    item.Language = Model.Language;
                    item.Dept = Model.Dept;
                    item.Religion = Model.Religion;
                    item.Division = Model.Division;
                    item.Bdate = (Model.Budate == null || Model.Budate == "01-01-0001" || Model.Budate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.Budate);
                    item.Anndate = (Model.StrAnndate == null || Model.StrAnndate == "01-01-0001" || Model.StrAnndate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrAnndate);
                    item.SpouseName = Model.SpouseName;
                    item.SpouseBdate = (Model.SpouseBudate == null || Model.SpouseBudate == "01-01-0001" || Model.SpouseBudate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.SpouseBudate);
                    item.ChildName = Model.ChildName;
                    item.ChildBdate = (Model.StrChildBdate == null || Model.StrChildBdate == "01-01-0001" || Model.StrChildBdate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrChildBdate);
                    item.SrNo = Model.SrNo;
                    item.ContactType = Model.ContactType;
                    item.GSTType = Model.GSTType;
                    item.DealerType = Model.DealerType;
                    item.DefaultCGst = Model.DefaultCGst;
                    item.DefaultSGst = Model.DefaultSGst;
                    item.DefaultIGst = Model.DefaultIGst;
                    item.AadharNo = Model.AadharNo;
                    item.PoisonLicense = Model.PoisonLicense;
                    item.ReraNo = Model.ReraNo;
                    item.AssistEmail = Model.AssistEmail;
                    item.AssistMobile = Model.AssistMobile;
                    item.AssistName = Model.AssistName;
                    item.AssistTel = Model.AssistTel;
                    item.Tel2 = Model.Tel2;
                    item.Tel3 = Model.Tel3;
                };
                Session.Add("MMailInfo", MailInformation);
                Model.MailList = MailInformation;
            }
            catch (DbEntityValidationException ex1)
            {
                string dd1 = ex1.InnerException.Message;
            }
            var html = ViewHelper.RenderPartialView(this, "MailingInfo", Model);
            return Json(new { MailList = MailInformation, Html = html }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region AddonGrid

        [HttpGet]
        public ActionResult GetEditAddOnList(string Code)
        {
            MasterVM Model = new MasterVM();
            List<AddOns> objitemlist = new List<AddOns>();
            var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%Master" && x.Hide == false)
                          select i).ToList();

            int t = 1;
            int n = 0;
            foreach (var i in addons)
            {
                AddOns c = new AddOns();
                c.Fld = i.Fld;
                c.Head = i.Head;
                c.ApplCode = GetMasterAddonValue(i.Fld, Code);
                c.QueryText = i.QueryText == null ? "" : GetQueryText(i.QueryText);
                c.FldType = i.FldType;
                c.PlaceValue = i.PlaceValue;
                c.Eqsn = i.Eqsn;
                objitemlist.Add(c);
                t = t + 1;
                n = n + 1;
            }

            Model.AddOnList = objitemlist;
            var html = ViewHelper.RenderPartialView(this, "~/Areas/SetUp/Views/Master/AddOnGrid.cshtml", new MasterVM() { AddOnList = Model.AddOnList, Mode = "Edit" });
            var jsonResult = Json(new
            {
                AddOnList = Model.AddOnList,
                Mode = "Edit",
                Html = html
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }

        public List<AddOns> GetAddOnListOnEdit(string Code)
        {
            List<AddOns> objitemlist = new List<AddOns>();
            var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%Master" && x.Hide == false)
                          select i).ToList();
            int t = 1;
            int n = 0;
            foreach (var i in addons)
            {
                AddOns c = new AddOns();
                c.Fld = i.Fld;
                c.Head = i.Head;
                c.ApplCode = GetMasterAddonValue(i.Fld, Code);
                c.QueryText = i.QueryText == null ? "" : GetQueryText(i.QueryText);
                c.FldType = i.FldType;
                c.PlaceValue = i.PlaceValue;
                c.Eqsn = i.Eqsn;
                objitemlist.Add(c);
                t = t + 1;
                n = n + 1;
            }
            return objitemlist;
        }

        //[HttpPost]
        //public ActionResult CalByEquationAddon(PurchaseVM Model)
        //{
        //    List<string> declist = new List<string>();
        //    List<AddOns> declist2 = new List<AddOns>();
        //    if (Model.AddonValueLast == null || Model.AddonValueLast.Count < 1)
        //    {

        //    }
        //    else
        //    {
        //        declist2 = Model.AddonValueLast;
        //    }
        //    string finaleqn;
        //    decimal mamt = 0;

        //    declist = ConvertAddonString(declist2);
        //    List<AddOns> objitemlist = new List<AddOns>();
        //    var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%Master" && x.Hide == false)
        //                  select i).ToList();
        //    int t = 1;
        //    int n = 0;
        //    foreach (var i in addons)
        //    {
        //        AddOns c = new AddOns();
        //        c.Fld = i.Fld;
        //        c.Head = i.Head;
        //        c.ApplCode = declist[n];
        //        c.QueryText = i.QueryText == null ? "" : GetQueryText(i.QueryText);
        //        c.FldType = i.FldType;
        //        c.PlaceValue = i.PlaceValue;
        //        c.Eqsn = i.Eqsn;
        //        c.RECORDKEY = t;
        //        objitemlist.Add(c);
        //        t = t + 1;
        //        n = n + 1;
        //    }
        //    var CurrFldRec = objitemlist.Where(x => x.Fld == Model.Fld).Select(x => x.RECORDKEY).FirstOrDefault();
        //    var NextFldRec = objitemlist.Where(x => x.RECORDKEY > CurrFldRec).Select(x => x.Fld).FirstOrDefault();
        //    Model.Fld = NextFldRec;
        //    Model.AddOnList = objitemlist;
        //    var html = ViewHelper.RenderPartialView(this, "~/Areas/SetUp/Views/Master/AddOnGrid.cshtml", new MasterVM() { AddOnList = Model.AddOnList, Mode = Model.Mode, Fld = Model.Fld });
        //    var jsonResult = Json(new
        //    {
        //        AddOnList = Model.AddOnList,
        //        Mode = Model.Mode,
        //        Fld = Model.Fld,
        //        Html = html
        //    }, JsonRequestBehavior.AllowGet);
        //    jsonResult.MaxJsonLength = int.MaxValue;
        //    return jsonResult;
        //}

        public List<string> ConvertAddonString(List<AddOns> LastModel)
        {
            string j;
            string ab;
            string mamt = "";
            List<string> PopulateStr = new List<string>();
            var trnaddons = ctxTFAT.AddOns.Where(x => x.TableKey == "%Master" && x.Hide == false).Select(x => new { x.Eqsn, x.FldType, x.Fld }).ToList();
            for (int i = 0; i < trnaddons.Count; i++)
            {

                var Eqn = trnaddons[i].Eqsn == null ? "" : trnaddons[i].Eqsn.Trim();
                if (Eqn.Contains("%F"))
                {
                    for (int ai = 0; ai < trnaddons.Count; ai++)
                    {
                        j = trnaddons[ai].Fld;
                        if (LastModel[ai].FldType == "N")
                        {
                            Eqn = Eqn.Replace("%" + j, (LastModel[ai].ApplCode == "" || LastModel[ai].ApplCode == null) ? "0" : LastModel[ai].ApplCode);
                            if (!Eqn.Contains("%F"))
                            {
                                break;
                            }
                        }
                        else if (LastModel[ai].FldType == "T" || LastModel[ai].FldType == "M" || LastModel[ai].FldType == "C")
                        {
                            Eqn = Eqn.Replace("%" + j, (LastModel[ai].ApplCode == "" || LastModel[ai].ApplCode == null) ? "''" : LastModel[ai].ApplCode);
                            if (!Eqn.Contains("%F"))
                            {
                                break;
                            }
                        }
                        else if (LastModel[ai].FldType == "D" || LastModel[ai].FldType == "L")
                        {
                            Eqn = "";
                            break;

                        }

                    }
                }
                if (Eqn != "")
                {
                    mamt = GetAmtValueAddon(Eqn);
                }
                else
                {
                    mamt = LastModel[i].ApplCode;
                }

                PopulateStr.Add(mamt);
            }
            return PopulateStr;
        }

        public string GetAmtValueAddon(string finalvalue)
        {
            string sql = "";
            string mamtm = "";
            if (finalvalue.Contains("select"))
            {
                sql = finalvalue;
            }
            else
            {
                sql = @"select " + finalvalue + " from TfatComp";
            }

            DataTable smDt = GetDataTable(sql, GetConnectionString());

            if (smDt.Rows.Count > 0)
            {
                mamtm = (smDt.Rows[0][0].ToString() == "" || smDt.Rows[0][0].ToString() == null) ? "" : smDt.Rows[0][0].ToString();
            }
            else
            {
                mamtm = "";
            }
            return mamtm;
        }

        //public string GetQueryText(string sql)
        //{
        //    string bca = "";
        //    if (sql.Contains("^"))
        //    {
        //        bca = sql;
        //    }
        //    else
        //    {
        //        StringBuilder addonT = new StringBuilder();

        //        DataTable mDt2 = GetDataTable(sql, GetConnectionString());
        //        if (mDt2.Rows.Count > 0)
        //        {
        //            for (var i = 0; i < mDt2.Rows.Count; i++)
        //            {
        //                bca = (mDt2.Rows[i][0].ToString() == "" || mDt2.Rows[i][0].ToString() == null) ? "" : mDt2.Rows[i][0].ToString();
        //                addonT.Append(bca + "^");
        //            }

        //        }
        //        string addonVT = addonT.ToString();
        //        if (addonVT != "")
        //        {
        //            bca = addonVT.Remove(addonVT.Length - 1);
        //        }


        //    }
        //    return bca;
        //}

        public string GetMasterAddonValue(string fld, string TableKey)
        {
            string connstring = GetConnectionString();
            string bca = "";
            var loginQuery3 = @"select " + fld + " from AddonMas where tablekey=" + "'" + TableKey + "'" + "";
            DataTable mDt2 = GetDataTable(loginQuery3, connstring);
            if (mDt2.Rows.Count > 0)
            {
                bca = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? "" : mDt2.Rows[0][0].ToString();
            }
            return bca;
        }
        #endregion

        [HttpPost]
        public ActionResult DeleteAccounts(MasterVM Model)
        {
            if (Model.Document == null || Model.Document == "")
            {
                return Json(new
                {
                    Message = "Missing Account Code..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            // iX9: Check for Active Master
            string mactivestring = "";
            var mactive1 = ctxTFAT.Ledger.Where(x => (x.Code == Model.Document)).Select(x => x.TableKey).FirstOrDefault();
            if (mactive1 != null)
            {
                mactivestring = mactivestring + "\nLedger: " + mactive1;
            }
            var mactive2 = ctxTFAT.Orders.Where(x => (x.Code == Model.Document)).Select(x => x.TableKey).FirstOrDefault();
            if (mactive2 != null)
            {
                mactivestring = mactivestring + "\nOrders: " + mactive2;
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
                    var mList = ctxTFAT.MasterInfo.Where(x => (x.Code == Model.Document)).FirstOrDefault();
                    ctxTFAT.MasterInfo.Remove(mList);
                    var mList2 = ctxTFAT.Address.Where(x => (x.Code == Model.Document)).FirstOrDefault();
                    if (mList2 != null)
                    {
                        ctxTFAT.Address.Remove(mList2);
                    }
                    var mList4 = ctxTFAT.HoldTransactions.Where(x => (x.Code == Model.Document)).FirstOrDefault();
                    ctxTFAT.HoldTransactions.Remove(mList4);
                    var mList5 = ctxTFAT.TaxDetails.Where(x => (x.Code == Model.Document)).FirstOrDefault();
                    ctxTFAT.TaxDetails.Remove(mList5);
                    var mList6 = ctxTFAT.AddonMas.Where(x => (x.TableKey == Model.Document)).ToList();
                    if (mList6 != null && mList6.Count > 0)
                    {
                        ctxTFAT.AddonMas.RemoveRange(mList6);
                    }
                    //var mListdoc = ctxTFAT.Attachment.Where(x => (x.ParentKey == "Master_" + Model.Document)).ToList();
                    //if (mListdoc != null && mListdoc.Count > 0)
                    //{
                    //    ctxTFAT.Attachment.RemoveRange(mListdoc);
                    //}
                    var mList3 = ctxTFAT.Master.Where(x => (x.Code == Model.Document)).FirstOrDefault();
                    ctxTFAT.Master.Remove(mList3);
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

        private void SaveAddons(MasterVM Model)
        {
            string addo1, addo2;
            StringBuilder addonT = new StringBuilder();
            List<string> addlist = new List<string>();
            if (Model.AddOnList != null && Model.AddOnList.Count > 0)
            {

                var addoni = ctxTFAT.AddonMas.Where(x => x.TableKey == Model.Code).Select(x => x).FirstOrDefault();
                if (addoni != null)
                {
                    ctxTFAT.AddonMas.Remove(addoni);
                }
                AddonMas aip = new AddonMas();
                aip.AUTHIDS = muserid;
                aip.AUTHORISE = "A00";
                aip.ENTEREDBY = muserid;
                aip.F001 = Model.AddOnList.Where(x => x.Fld == "F001").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F002 = Model.AddOnList.Where(x => x.Fld == "F002").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F003 = Model.AddOnList.Where(x => x.Fld == "F003").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F004 = Model.AddOnList.Where(x => x.Fld == "F004").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F005 = Model.AddOnList.Where(x => x.Fld == "F005").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F005").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F006 = Model.AddOnList.Where(x => x.Fld == "F006").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F006").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F007 = Model.AddOnList.Where(x => x.Fld == "F007").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F007").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F008 = Model.AddOnList.Where(x => x.Fld == "F008").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F009 = Model.AddOnList.Where(x => x.Fld == "F009").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F009").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F010 = Model.AddOnList.Where(x => x.Fld == "F010").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F010").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F011 = Model.AddOnList.Where(x => x.Fld == "F011").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F011").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F012 = Model.AddOnList.Where(x => x.Fld == "F012").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F013 = Model.AddOnList.Where(x => x.Fld == "F013").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F013").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F014 = Model.AddOnList.Where(x => x.Fld == "F014").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F014").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F015 = Model.AddOnList.Where(x => x.Fld == "F015").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F015").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F016 = Model.AddOnList.Where(x => x.Fld == "F016").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F016").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F017 = Model.AddOnList.Where(x => x.Fld == "F017").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F017").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F018 = Model.AddOnList.Where(x => x.Fld == "F018").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F018").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F019 = Model.AddOnList.Where(x => x.Fld == "F019").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F019").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F020 = Model.AddOnList.Where(x => x.Fld == "F020").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F020").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F021 = Model.AddOnList.Where(x => x.Fld == "F021").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F021").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F022 = Model.AddOnList.Where(x => x.Fld == "F022").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F022").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F023 = Model.AddOnList.Where(x => x.Fld == "F023").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F023").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F024 = Model.AddOnList.Where(x => x.Fld == "F024").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F024").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F025 = Model.AddOnList.Where(x => x.Fld == "F025").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F025").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F026 = Model.AddOnList.Where(x => x.Fld == "F026").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F026").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F027 = Model.AddOnList.Where(x => x.Fld == "F027").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F027").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F028 = Model.AddOnList.Where(x => x.Fld == "F028").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F028").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F029 = Model.AddOnList.Where(x => x.Fld == "F029").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F029").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F030 = Model.AddOnList.Where(x => x.Fld == "F030").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F030").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.LASTUPDATEDATE = DateTime.Now;
                aip.TableKey = Model.Code;
                ctxTFAT.AddonMas.Add(aip);

            }
        }

        #region attachment
        public ActionResult UploadFile()
        {
            ViewBag.ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
            return Json(new { Status = "Success", Controller = ViewBag.ControllerName }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AttachDocument(HttpPostedFileBase files, string DocumentStr, string FileNameStr)
        {
            string XYZ = "";
            string docstr = "";
            string FLN = "";
            List<MasterVM> DocList = new List<MasterVM>();
            if (Session["MTempAccMasterAttach"] != null)
            {
                DocList = (List<MasterVM>)Session["MTempAccMasterAttach"];
            }


            int n = DocList.Count() + 1;

            byte[] fileData = null;

            for (int i = 0; i < Request.Files.Count; i++)
            {

                MasterVM Model = new MasterVM();

                var file = Request.Files[i];
                var fileName = Path.GetFileName(file.FileName);

                MemoryStream target = new MemoryStream();
                using (var binaryReader = new BinaryReader(Request.Files[i].InputStream))
                {
                    fileData = binaryReader.ReadBytes(Request.Files[i].ContentLength);
                }


                Model.ImageData = fileData;

                string FileString = Convert.ToBase64String(fileData);
                file.InputStream.CopyTo(target);

                Model.FileContent = file.ContentType;
                Model.ImageStr = FileString;

                Model.FileName = fileName;
                Model.ContentType = file.ContentType;

                Model.tEmpID = i + 1;
                DocList.Add(Model);

            }

            if (DocList.Count > 0)
            {
                foreach (var a in DocList.Where(x => x.tempIsDeleted == false))
                {
                    docstr = docstr + a.ImageStr + ",";
                }
                if (docstr != "")
                {
                    docstr = docstr.Remove(docstr.Length - 1);
                }
                string docfilnam = "";
                foreach (var b in DocList.Where(x => x.tempIsDeleted == false))
                {
                    docfilnam = docfilnam + b.FileName + ",";
                }
                if (docfilnam != "")
                {
                    docfilnam = docfilnam.Remove(docfilnam.Length - 1);
                }
                XYZ = docstr;
                FLN = docfilnam;
            }
            Session["MTempAccMasterAttach"] = DocList;
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            var html = ViewHelper.RenderPartialView(this, "~/Areas/SetUp/Views/Master/AttachmentDocument.cshtml", new MasterVM() { DocumentList = DocList, AllFileStr = XYZ, FileNameStr = FLN, Mode = "Add" });
            var jsonResult = Json(new { DocumentList = DocList, AllFileStr = XYZ, FileNameStr = FLN, Html = html, Mode = "Add" }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }
        [HttpPost]
        public ActionResult EditImage(HttpPostedFileBase files, string Document)
        {
            string XYZ = "";

            string FLN = "";

            List<MasterVM> DocList = new List<MasterVM>();
            if (Session["MTempAccMasterAttach"] != null)
            {
                DocList = (List<MasterVM>)Session["MTempAccMasterAttach"];
            }
            int n = DocList.Count() + 1;

            byte[] fileData = null;
            string docstr = "";

            for (int i = 0; i < Request.Files.Count; i++)
            {
                MasterVM Model = new MasterVM();

                var file = Request.Files[i];
                var fileName = Path.GetFileName(file.FileName);
                MemoryStream target = new MemoryStream();
                using (var binaryReader = new BinaryReader(Request.Files[i].InputStream))
                {
                    fileData = binaryReader.ReadBytes(Request.Files[i].ContentLength);
                }
                Model.ImageData = fileData;
                string FileString = Convert.ToBase64String(fileData);
                file.InputStream.CopyTo(target);
                Model.FileContent = file.ContentType;
                Model.ImageStr = FileString;
                Model.FileName = fileName;
                Model.ContentType = file.ContentType;
                Model.tEmpID = n;
                DocList.Add(Model);
                n = n + 1;

            }
            if (DocList.Count > 0)
            {
                foreach (var a in DocList.Where(x => x.tempIsDeleted == false))
                {
                    docstr = docstr + a.ImageStr + ",";
                }
                if (docstr != "")
                {
                    docstr = docstr.Remove(docstr.Length - 1);
                }
                string docfilnam = "";
                foreach (var b in DocList.Where(x => x.tempIsDeleted == false))
                {
                    docfilnam = docfilnam + b.FileName + ",";
                }
                if (docfilnam != "")
                {
                    docfilnam = docfilnam.Remove(docfilnam.Length - 1);
                }
                XYZ = docstr;
                FLN = docfilnam;
            }
            Session["MTempAccMasterAttach"] = DocList;


            var html = ViewHelper.RenderPartialView(this, "~/Areas/SetUp/Views/Master/AttachmentDocument.cshtml", new MasterVM() { DocumentList = DocList, AllFileStr = XYZ, FileNameStr = FLN, Mode = "Edit" });
            var jsonResult = Json(new { DocumentList = DocList, AllFileStr = XYZ, FileNameStr = FLN, Html = html, Mode = "Edit" }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }
        [HttpPost]
        public ActionResult DeleteUploadFile(MasterVM Model)
        {
            string XYZ = "";

            string FLN = "";
            string docstr = "";

            Model.DocumentList = Model.DocumentList.Where(x => x.tEmpID != Model.tEmpID).Select(x => x).ToList();
            if (Model.DocumentList.Count > 0)
            {
                foreach (var a in Model.DocumentList)
                {
                    docstr = docstr + a.ImageStr + ",";
                }
                if (docstr != "")
                {
                    docstr = docstr.Remove(docstr.Length - 1);
                }
                string docfilnam = "";
                foreach (var b in Model.DocumentList)
                {
                    docfilnam = docfilnam + b.FileName + ",";
                }
                if (docfilnam != "")
                {
                    docfilnam = docfilnam.Remove(docfilnam.Length - 1);
                }
                XYZ = docstr;
                FLN = docfilnam;
            }
            Session["MTempAccMasterAttach"] = Model.DocumentList;

            var html = ViewHelper.RenderPartialView(this, "~/Areas/SetUp/Views/Master/AttachmentDocument.cshtml", new MasterVM() { DocumentList = Model.DocumentList, AllFileStr = XYZ, FileNameStr = FLN, Mode = Model.Mode });
            var jsonResult = Json(new { DocumentList = Model.DocumentList, AllFileStr = XYZ, FileNameStr = FLN, Html = html, Mode = Model.Mode }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public FileResult Download(int tEmpID)
        {
            List<MasterVM> DocList = new List<MasterVM>();
            if (Session["MTempAccMasterAttach"] != null)
            {
                DocList = (List<MasterVM>)Session["MTempAccMasterAttach"];
            }

            var dwnfile = DocList.Where(x => x.tEmpID == tEmpID).FirstOrDefault();

            string filename = dwnfile.FileName;
            byte[] fileBytes = Convert.FromBase64String(dwnfile.ImageStr);

            return File(fileBytes, dwnfile.FileContent, filename);
        }

        public void SaveAttachment(MasterVM Model)
        {
            List<MasterVM> DocList = new List<MasterVM>();
            if (Session["MTempAccMasterAttach"] != null)
            {
                DocList = (List<MasterVM>)Session["MTempAccMasterAttach"];
            }

            if (DocList != null && DocList.Count != 0)
            {
                //List<string> FileString = Model.AllFileStr.Split(',').ToList();
                //List<string> NameString = Model.FileNameStr.Split(',').ToList();
                int c = 0;
                int an = 1;
                foreach (var item in DocList)
                {

                    System.IO.Directory.CreateDirectory(attachmentPath + "Master_" + Model.Code);
                    string directoryPath = attachmentPath + "Master_" + Model.Code + "/" + item.FileName;
                    byte[] IData = Convert.FromBase64String(item.ImageStr);
                    Attachment att = new Attachment();
                    att.AUTHIDS = muserid;
                    att.AUTHORISE = "A00";
                    att.Branch = mbranchcode;
                    att.Code = Model.Code;
                    att.ENTEREDBY = muserid;
                    att.FilePath = directoryPath;
                    att.LASTUPDATEDATE = DateTime.Now;
                    att.LocationCode = Model.LocationCode;
                    att.Prefix = mperiod;
                    att.Sno = an;
                    att.Srl = MasterSerial();
                    att.SrNo = an;
                    att.TableKey = "Master_" + Model.Code + an.ToString("D3");
                    att.ParentKey = "Master_" + Model.Code;
                    att.Type = "";
                    att.CompCode = mcompcode;
                    ctxTFAT.Attachment.Add(att);
                    ctxTFAT.SaveChanges();
                    System.IO.File.WriteAllBytes(directoryPath, IData);
                    an = an + 1;
                    c = c + 1;

                }
            }

        }

        public List<MasterVM> GetAttachmentListInEdit(MasterVM Model)
        {
            List<MasterVM> AttachmentList = new List<MasterVM>();
            //var docdetail = ctxTFAT.Attachment.Where(x => x.ParentKey == "Master_" + Model.Code).ToList();
            //foreach (var item in docdetail)
            //{
            //    AttachmentList.Add(new MasterVM()
            //    {
            //        FileName = Path.GetFileName(item.FilePath),
            //        Code = item.Code,
            //        tEmpID = item.Sno,
            //        SrNo = item.Sno,
            //        Path = item.FilePath,
            //        TableKey = item.TableKey,
            //        ParentKey = item.ParentKey,
            //        FileContent = Path.GetExtension(item.FilePath),
            //        ImageStr = Convert.ToBase64String(System.IO.File.ReadAllBytes(item.FilePath)),
            //        tempIsDeleted = false
            //    });
            //}
            return AttachmentList;
        }

        public string MasterSerial()
        {
            string name;
            string srl;
            string lastcode = "";
            int mainsrl;
            //List<string> str1 = (from TS in ctxTFAT.Attachment
            //                     select TS.Srl).ToList();
            //if (str1.Count > 0)

            //{
            //    int str2 = GetMaxFromStringList(str1);
            //    name = str2.ToString();
            //}
            //else
            //{
            //    name = "";
            //}

            //if (name == null || name == "")
            //{
            //    srl = "000001";
            //    mainsrl = 0;
            //    lastcode = "000001";

            //}
            //else
            //{
            //    srl = Regex.Replace(name, "[^A-Za-z]", "");
            //    string strsrl = Regex.Replace(name, "[^0-9]", "");
            //    mainsrl = int.Parse(strsrl);

            //    lastcode = srl + string.Format("{0:000000}", (mainsrl + 1));
            //}
            return lastcode;
        }
        public int GetMaxFromStringList(List<string> strings)
        {
            List<int> ints = new List<int>();
            foreach (var a in strings)
            {
                string c = Regex.Replace(a, "[^0-9]", "");
                if (c == "")
                {
                    c = "0";
                }
                int b = Convert.ToInt32(c);
                ints.Add(b);

            }

            int abc = ints.Select(x => x).Max();
            return abc;
        }
        #endregion
    }
}