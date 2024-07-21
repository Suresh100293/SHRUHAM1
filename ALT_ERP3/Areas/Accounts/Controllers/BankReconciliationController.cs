//using System.Web.UI.WebControls;
using ALT_ERP3.Controllers;
using Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class BankReconciliationController : BaseController
    {
        private static decimal mOpeningBalance = 0;
        private static decimal mClosing = 0;
        private static decimal mbClosing = 0;

        // GET: Accounts/AccountLedger
        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, "Report", Model.Header, "", DateTime.Now, 0, "", "", "NA");
            if (Model.Document == "" || Model.Document == null)
            {
                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            }
            ViewBag.ViewDataId = Model.ViewDataId;
            Model.AccountName = NameofAccount(Model.Document);
            ViewBag.Header = Model.Header;
            ViewBag.Controller = Model.Controller;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;
            var mmenu1 = ctxTFAT.TfatMenu.Where(z => z.OptionCode == Model.OptionCode).Select(x => x.ID).FirstOrDefault();
            var mmenu = ctxTFAT.UserRights.Where(z => z.MenuID == mmenu1 && z.Code == muserid).Select(x => new { x.xAdd, x.xEdit, x.xDelete }).FirstOrDefault();

            if (mmenu != null)
            {
                Model.xAdd = mmenu.xAdd;
                Model.xEdit = mmenu.xEdit;
                Model.xDelete = mmenu.xDelete;
            }
            if (muserid.ToLower() == "super")
            {
                Model.xAdd = true;
                Model.xEdit = true;
                Model.xDelete = true;
                Model.xView = true;
            }
            return View(Model);
        }

        public ActionResult GetFlags()
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "N", Text = "Un Cleared" });
            GSt.Add(new SelectListItem { Value = "C", Text = "Cleared" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAccountList(string term)
        {
            if (term == "")
            {
                return Json(GetDataTableList("Select Top 15 Code, Name from Master Where BaseGr='B' and Hide=0 and Charindex('" + mbranchcode + "', AppBranch)<>0 Order by Name"), JsonRequestBehavior.AllowGet);
                //var result = ctxTFAT.Master.Where(z => z.BaseGr == "B" && z.Hide == false && z.AppBranch.Contains(mbranchcode)).Select(m => new
                //{
                //    m.Code,
                //    m.Name
                //}).OrderBy(n => n.Name).Take(15).ToList();
                //return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(GetDataTableList("Select Code, Name from Master Where BaseGr='B' and Hide=0 and Charindex('" + mbranchcode + "', AppBranch)<>0 and Name like '%" + term + "%' Order by Name"), JsonRequestBehavior.AllowGet);
                //var result = ctxTFAT.Master.Where(z => z.BaseGr == "B" && z.Hide == false && z.AppBranch.Contains(mbranchcode)).Where(x => x.Name.Contains(term)).Select(m => new
                //{
                //    m.Code,
                //    m.Name
                //}).OrderBy(n => n.Name).ToList();
                //return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            if (IsValidDate(Model.Date) == false)
            {
                return Json(new
                {
                    Message = "Invalid Date..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var date = Model.Date.Replace("-", "/").Split(':');
            Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            //DateTime mFirstDate = GetFirstDate();
            var SDate = Convert.ToDateTime(Model.FromDate);
            var LDate = Convert.ToDateTime(Model.ToDate);
            // opening as per book
            //mOpeningBalance = GetBalance(Model.Code, SDate.AddDays(-1), mbranchcode, 0, false, false, false);
            mOpeningBalance = (from L in ctxTFAT.Ledger
                               where L.Code == Model.Code && L.DocDate < SDate.Date && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                               select L.Debit - L.Credit).DefaultIfEmpty(0).Sum() ?? 0;
            // closing as per book
            //mClosing = GetBalance(Model.Code, LDate.Date, mbranchcode, 0, false, false, false);
            mClosing = (from L in ctxTFAT.Ledger
                        where L.Code == Model.Code && L.DocDate <= LDate.Date && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                        select L.Debit - L.Credit).DefaultIfEmpty(0).Sum() ?? 0;
            // totoal pay
            //decimal mCredit = GetBalanceDateRange(Model.Code, SDate.Date, LDate.Date, "C", mbranchcode, 0, false, false, false);
            decimal mCredit = (from L in ctxTFAT.Ledger
                               where L.Code == Model.Code && L.DocDate >= SDate.Date && L.DocDate <= LDate.Date && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                               select L.Credit).DefaultIfEmpty(0).Sum() ?? 0;
            // total rec
            //decimal mDebit = GetBalanceDateRange(Model.Code, SDate.Date, LDate.Date, "D", mbranchcode, 0, false, false, false);
            decimal mDebit = (from L in ctxTFAT.Ledger
                              where L.Code == Model.Code && L.DocDate >= SDate.Date && L.DocDate <= LDate.Date && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                              select L.Debit).DefaultIfEmpty(0).Sum() ?? 0;

            DateTime mDate = Convert.ToDateTime("01/01/1950");
            // opening as per bank
            //decimal mbOpening = FieldoftableNumber("Ledger", "Sum(Debit-Credit)", "MainType<>'MV' and MainType<>'PV' and Code='" + Model.Code + "' and Year(ClearDate)<>1950 and DocDate<'" + MMDDYY(SDate.Date) + "' and Branch='" + mbranchcode + "' and left(AUTHORISE,1)='A'" + (string.IsNullOrEmpty(mDocString) ? "" : " and Charindex(Type,'" + mDocString + "')=0") + (Model.LocationCode != 0 ? " and LocationCode=" + Model.LocationCode : ""));
            decimal mbOpening = (from L in ctxTFAT.Ledger
                                 where L.Code == Model.Code && L.DocDate < SDate.Date && DbFunctions.TruncateTime(L.ClearDate) != mDate.Date && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                                 select L.Debit - L.Credit).DefaultIfEmpty(0).Sum() ?? 0;
            // rec as per bank
            //decimal mbDebit = FieldoftableNumber("Ledger", "Sum(Debit)", "MainType<>'MV' and MainType<>'PV' and Code='" + Model.Code + "' and Year(ClearDate)<>1950 and ClearDate<='"+ MMDDYY( LDate.Date) + "' and DocDate>='" + MMDDYY(SDate.Date) + "' and DocDate<='" + MMDDYY(LDate.Date) + "' and Branch='" + mbranchcode + "' and left(AUTHORISE,1)='A'" + (string.IsNullOrEmpty(mDocString) ? "" : " and Charindex(Type,'" + mDocString + "')=0") + (Model.LocationCode != 0 ? " and LocationCode=" + Model.LocationCode : ""));
            decimal mbDebit = (from L in ctxTFAT.Ledger
                               where L.Code == Model.Code && ((L.DocDate >= SDate.Date && L.DocDate <= LDate.Date) && DbFunctions.TruncateTime(L.ClearDate) != mDate.Date && L.ClearDate <= LDate.Date) && L.Debit > 0 && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                               select L.Debit).DefaultIfEmpty(0).Sum() ?? 0;
            // pay as per bank
            //decimal mbCredit = FieldoftableNumber("Ledger", "Sum(Credit)", "MainType<>'MV' and MainType<>'PV' and Code='" + Model.Code + "' and Year(ClearDate)<>1950 and DocDate>='" + MMDDYY(SDate.Date) + "' and DocDate<='" + MMDDYY(LDate.Date) + "' and Branch='" + mbranchcode + "' and left(AUTHORISE,1)='A'" + (string.IsNullOrEmpty(mDocString) ? "" : " and Charindex(Type,'" + mDocString + "')=0") + (Model.LocationCode != 0 ? " and LocationCode=" + Model.LocationCode : ""));
            decimal mbCredit = (from L in ctxTFAT.Ledger
                                where L.Code == Model.Code && ((L.DocDate >= SDate.Date && L.DocDate <= LDate.Date) && DbFunctions.TruncateTime(L.ClearDate) != mDate.Date && L.ClearDate <= LDate.Date) && L.Credit > 0 && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                                select L.Credit).DefaultIfEmpty(0).Sum() ?? 0;

            // closing as per bank
            //mbClosing = FieldoftableNumber("Ledger", "Sum(Debit-Credit)", "MainType<>'MV' and MainType<>'PV' and Code='" + Model.Code + "' and Year(ClearDate)<>1950 and DocDate<='" + MMDDYY(LDate.Date) + "' and Branch='" + mbranchcode + "' and left(AUTHORISE,1)='A'" + (string.IsNullOrEmpty(mDocString) ? "" : " and Charindex(Type,'" + mDocString + "')=0") + (Model.LocationCode != 0 ? " and LocationCode=" + Model.LocationCode : ""));
            mbClosing = (from L in ctxTFAT.Ledger
                         where L.Code == Model.Code && (L.DocDate <= LDate.Date && L.ClearDate <= LDate.Date && DbFunctions.TruncateTime(L.ClearDate) != mDate.Date) && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                         select L.Debit - L.Credit).DefaultIfEmpty(0).Sum() ?? 0;

            return GetGridDataColumns(Model.ViewDataId, "X", "", string.Format("{0:0.00}", mOpeningBalance) + "|" + string.Format("{0:0.00}", mClosing) + "|" + string.Format("{0:0.00}", mDebit) + "|" + string.Format("{0:0.00}", mCredit) + "|" + string.Format("{0:0.00}", mbOpening) + "|" + string.Format("{0:0.00}", mbClosing) + "|" + string.Format("{0:0.00}", mbDebit) + "|" + string.Format("{0:0.00}", mbCredit));
        }

        //[HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            return GetGridReport(Model, "R", "Code^" + Model.Code, false, mOpeningBalance);
        }

        [HttpPost]
        public ActionResult UpdateRecoStatus(GridOption Model)
        {
            string mparentkey = "";
            string mchq = "";
            string mflag = "N";
            string mDoc = Model.Document.Substring(6);
            DataTable mobjs = GetDataTable("Select * from Ledger Where TableKey = '" + mDoc + "'");
            DataRow mobj = mobjs.Rows[0];
            //ctxooroo.Ledger.Where(x => x.TableKey == mDoc).FirstOrDefault();
            if (Model.BaseGr == "C")
            {
                mparentkey = mobj["ParentKey"].ToString();
                mchq = mobj["Cheque"].ToString();
                mflag = "C";

                //mobj["RecoFlag"] = "C";
                DateTime mDate;
                mDate = ConvertDDMMYYTOYYMMDD(Model.ClearDate);
                if (mDate == new DateTime(1950, 1, 1))
                {
                    mDate = DateTime.Now.Date;
                }
                var date = Model.Date.Replace("-", "/").Split(':');
                Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                if (mDate > Convert.ToDateTime(Model.ToDate))
                {
                    mDate = Convert.ToDateTime(Model.ToDate);
                }

                //mobj.ClearDate = ConvertDDMMYYTOYYMMDD(Model.ClearDate);
                //if (mobj.ClearDate == new DateTime(1950, 1, 1))
                //{
                //    mobj.ClearDate = DateTime.Now.Date;
                //}
                //var date = Model.Date.Replace("-", "/").Split(':');
                //Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                //if (mobj.ClearDate > Convert.ToDateTime(Model.ToDate))
                //{
                //    mobj.ClearDate = Convert.ToDateTime(Model.ToDate);
                //}
                ExecuteStoredProc(@"Update Ledger Set Recoflag='C',ClearDate='" + MMDDYY(mDate) + "' where TableKey = '" + mDoc + "'");
            }
            else
            {
                ExecuteStoredProc(@"Update Ledger Set Recoflag='N',ClearDate='01-Jan-1950' where TableKey = '" + mDoc + "'");
                //mobj.RecoFlag = "N";
                //mobj.ClearDate = new DateTime(1950, 1, 1);
            }
            //ctxooroo.SaveChanges();
            ExecuteStoredProc(@"Update Ledger Set RecoFlag='" + mflag + "' where ParentKey='" + mparentkey + "' And Cheque='" + mchq + "'");
            string html;
            html = ViewHelper.RenderPartialView(this, "index", Model);
            return Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetUpdatePopUp(GridOption Model)
        {
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Reports/Views/BankReconciliation/UpdateDatePartial.cshtml", Model);
            var jsonResult = Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public ActionResult GetExcel(GridOption Model)
        {
            Model.mWhat = "XLS";
            return GetGridData(Model);
        }
    }
}