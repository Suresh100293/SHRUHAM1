//using System.Web.UI.WebControls;
using ALT_ERP3.Controllers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Reports.Controllers
{
    public class StockLedgerController : BaseController
    {
        private static decimal mOpeningBalance = 0;

        // GET: Reports/StockLedger
        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "A");
            if (Model.Document == "" || Model.Document == null)
            {
                GetAllMenu(Session["ModuleName"].ToString());
                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            }
            else
            {
                Model.AccountName = ctxTFAT.ItemMaster.Where(z => z.Code == Model.Document).Select(x => x.Name).FirstOrDefault();
            }
            if (Model.FromDate == null)
            {
                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                Model.ToDate = DateTime.Today.ToShortDateString();
            }
            // StockLedgerScr
            ViewBag.ViewDataId = Model.ViewDataId;
            Model.ViewCode = GetDefaultCode(Model.ViewDataId);
            ViewBag.Header = Model.Header;
            ViewBag.Controller = Model.Controller;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;
            return View(Model);
        }

        public ActionResult GetItemList(string term)
        {
            if (term == "")
            {
                var result = ctxTFAT.ItemMaster.Select(m => new { m.Code, Name = "[" + m.Code + "] " + m.Name }).OrderBy(n => n.Name).Take(10).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.ItemMaster.Where(x => (x.Name.Contains(term) || x.Code.Contains(term))).Select(m => new { m.Code, Name = "[" + m.Code + "] " + m.Name }).OrderBy(n => n.Name).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            if (Model.Code == null) return null;
            if (Model.Date == ":")
            {
                Model.Date = System.Web.HttpContext.Current.Session["StartDate"].ToString() + ":" + DateTime.Today.ToShortDateString();
            }
            if (IsValidDate(Model.Date) == false)
            {
                return Json(new { Message = "Invalid Date..", Status = "Error" }, JsonRequestBehavior.AllowGet);
            }
            var date = Model.Date.Replace("-", "/").Split(':');
            Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

            var SDate = Convert.ToDateTime(Model.FromDate);
            var LDate = Convert.ToDateTime(Model.ToDate);
            string mbranch = ppara03 == null || ppara03 == "" ? "'" + mbranchcode + "'" : ppara03;
            int mstore = ppara02 == null || ppara02 == "" ? 0 : Convert.ToInt32(ppara02);
            int mlocation = ppara01 == null || ppara01 == "" ? 0 : Convert.ToInt32(ppara01);

            decimal OpeningBal = (decimal)GetStock(Model.Code, mstore, SDate.AddDays(-1), mbranch, mlocation, false);

            //decimal OpeningBal = (decimal)(from L in ctxTFAT.Stock
            //                               where (string.IsNullOrEmpty(ppara02) ? true : L.Store.ToString() == ppara02.ToString()) && (string.IsNullOrEmpty(ppara01) ? true : L.LocationCode.ToString() == ppara01.ToString()) && L.Code == Model.Code && L.DocDate < SDate.Date && L.Branch == mbranchcode && L.NotInStock == false && L.AUTHORISE.StartsWith("A") && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
            //                               select L.Qty).DefaultIfEmpty(0).Sum();
            mOpeningBalance = OpeningBal;
            string mOpening = string.Format("{0:0.00}", OpeningBal);

            //decimal ClosingBal = (decimal)(from L in ctxTFAT.Stock
            //                               where (string.IsNullOrEmpty(ppara02) ? true : L.Store.ToString() == ppara02.ToString()) && (string.IsNullOrEmpty(ppara01) ? true : L.LocationCode.ToString() == ppara01.ToString()) && L.Code == Model.Code && L.DocDate <= LDate.Date && L.Branch == mbranchcode && L.NotInStock == false && L.AUTHORISE.StartsWith("A") && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
            //                               select L.Qty).DefaultIfEmpty(0).Sum();
            decimal ClosingBal = (decimal)GetStock(Model.Code, mstore, LDate.Date, mbranch, mlocation, false);

            string mClosing = string.Format("{0:0.00}", ClosingBal);

            //decimal TCredit = (decimal)(from L in ctxTFAT.Stock
            //                            where (string.IsNullOrEmpty(ppara02) ? true : L.Store.ToString() == ppara02.ToString()) && (string.IsNullOrEmpty(ppara01) ? true : L.LocationCode.ToString() == ppara01.ToString()) && L.Code == Model.Code && L.DocDate >= SDate.Date && L.DocDate <= LDate.Date && L.Qty < 0 && L.Branch == mbranchcode && L.NotInStock == false && L.AUTHORISE.StartsWith("A") && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
            //                            select L.Qty * -1).DefaultIfEmpty(0).Sum();
            decimal TCredit = (decimal)GetStockRange(Model.Code, mstore, SDate.Date, LDate.Date, "C", mbranch, mlocation, false);

            string mCredit = string.Format("{0:0.00}", TCredit);

            decimal TDebit = (decimal)GetStockRange(Model.Code, mstore, SDate.Date, LDate.Date, "D", mbranch, mlocation, false);
            //decimal TDebit = (decimal)(from L in ctxTFAT.Stock
            //                           where (string.IsNullOrEmpty(ppara02) ? true : L.Store.ToString() == ppara02.ToString()) && (string.IsNullOrEmpty(ppara01) ? true : L.LocationCode.ToString() == ppara01.ToString()) && L.Code == Model.Code && L.DocDate >= SDate.Date && L.DocDate <= LDate.Date && L.Qty > 0 && L.Branch == mbranchcode && L.NotInStock == false && L.AUTHORISE.StartsWith("A") && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
            //                           select L.Qty).DefaultIfEmpty(0).Sum();
            string mDebit = string.Format("{0:0.00}", TDebit);

            return GetGridDataColumns(Model.ViewDataId, "X", "", mOpening + "|" + mClosing + "|" + mCredit + "|" + mDebit);
        }

        public ActionResult GetGridData(GridOption Model)
        {
            return GetGridReport(Model, "R", "Code^" + Model.Code + (mpara != "" ? "~" + mpara : ""), true, mOpeningBalance);
        }

        public ActionResult GetExcel(GridOption Model)
        {
            Model.mWhat = "XLS";
            return GetGridData(Model);
        }

        #region sales
        [HttpPost]
        public ActionResult GetGridStructureSales(GridOption Model)
        {
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        [HttpPost]
        public ActionResult GetGridDataSales(GridOption Model)
        {
            if (Model.ViewDataId == "StockLedgerOSAgeing")
            {
                SqlConnection tfat_conx = new SqlConnection(GetConnectionString());//ConfigurationManager.ConnectionStrings[""].ConnectionString
                SqlCommand cmd = new SqlCommand("SPTFAT_StockAgeing", tfat_conx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;
                var date = Model.Date.Replace("-", "/").Split(':');
                Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
                cmd.Parameters.Add("@mItemGroups", SqlDbType.VarChar).Value = "";
                cmd.Parameters.Add("@mItems", SqlDbType.VarChar).Value = Model.Code ?? "";
                cmd.Parameters.Add("@mStore", SqlDbType.VarChar).Value = 0;
                cmd.Parameters.Add("@mDate", SqlDbType.VarChar).Value = Model.ToDate;
                tfat_conx.Open();
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                tfat_conx.Close();
            }
            else if (Model.ViewDataId == "StockLedgerMonthlySummary")
            {
                SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                SqlCommand cmd = new SqlCommand("SPTFAT_StockMthSummary", tfat_conx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;
                cmd.Parameters.Add("@mCode", SqlDbType.VarChar).Value = Model.Code ?? "";
                cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
                cmd.Parameters.Add("@mStartDate", SqlDbType.Date).Value = Convert.ToDateTime(System.Web.HttpContext.Current.Session["StartDate"].ToString());
                tfat_conx.Open();
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                tfat_conx.Close();
            }
            return GetGridReport(Model, "R", "Code^" + Model.Code, false, 0);
        }
        #endregion sales
    }
}