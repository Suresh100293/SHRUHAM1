using EntitiModel;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Controllers;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class LedgersController : BaseController
    {
        private static decimal mOpeningBalance = 0;
        private static string msubcodeof = "";
        private static string mbasegr = "";

        // GET: Reports/AccountLedger
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
                Model.ToDate = DateTime.Today.ToShortDateString();
                //System.Web.HttpContext.Current.Session["LastDate"].ToString();
            }
            if (Model.FromDate == null)
            {
                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                Model.ToDate = DateTime.Today.ToShortDateString();
            }
            ViewBag.ViewDataId = Model.ViewDataId;
            msubcodeof = Model.ViewDataId;
            Model.AccountName = NameofAccount(Model.Document);
            Model.ViewCode = GetDefaultCode(Model.ViewDataId);
            mbasegr = Model.MainType;
            ViewBag.Header = Model.Header;
            ViewBag.Controller = Model.Controller;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;
            return View(Model);
        }

        public ActionResult GetAccountList(string term)
        {
            if (mbasegr == "CB")
            {
                if (term == "")
                {
                    return Json(GetDataTableList("Select Top 15 Code,Name from Master where (BaseGr='B' or BaseGr='C') and Hide=0 Order by Name"), JsonRequestBehavior.AllowGet);
                    //var result = ctxTFAT.Master.Where(z => (z.BaseGr == "C" || z.BaseGr == "B") && z.Hide == false).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).Take(15).ToList();
                    //return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(GetDataTableList("Select Code,Name from Master where Name like '%" + term + "%' and (BaseGr='B' or BaseGr='C') and Hide=0 Order by Name"), JsonRequestBehavior.AllowGet);
                    //var result = ctxTFAT.Master.Where(z => (z.BaseGr == "C" || z.BaseGr == "B") && z.Hide == false).Where(x => x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).ToList();
                    //return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                if (term == "")
                {
                    return Json(GetDataTableList("Select Top 15 Code, Name = '[' + Code + '] ' + ShortName + ' ' + Name + ' ' + (case when basegr='D' or Basegr='S' then City else '' end) from Master Where BaseGr<>'C' and BaseGr <> 'B' and Hide=0 Order by Name"), JsonRequestBehavior.AllowGet);
                    //return Json(GetDataTableList("Select Top 15 Code,Name from Master where BaseGr<>'B' and BaseGr<>'C' and Hide=0 Order by Name"), JsonRequestBehavior.AllowGet);
                    //var result = ctxTFAT.Master.Where(x => x.BaseGr != "C" && x.BaseGr != "B" && x.Hide == false).Select(m => new { m.Code, Name = "[" + m.Code + "] " + m.ShortName + " " + m.Name + " " + m.City }).OrderBy(n => n.Name).Take(15).ToList();
                    //return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(GetDataTableList("Select Code, Name = '[' + Code + '] ' + ShortName + ' ' + Name + ' ' + (Case when basegr='D' or Basegr='S' then City else '' end) from Master Where BaseGr <> 'C' and BaseGr <> 'B' and Hide=0 and Charindex('" + term + "', Name)<>0 Order by Name"), JsonRequestBehavior.AllowGet);
                    //return Json(GetDataTableList("Select Code,Name from Master where Name like '%" + term + "%' and BaseGr<>'B' and BaseGr<>'C' and Hide=0 Order by Name"), JsonRequestBehavior.AllowGet);
                    //var result = ctxTFAT.Master.Where(x => x.BaseGr != "C" && x.BaseGr != "B" && x.Hide == false && x.Name.Contains(term)).Select(m => new { m.Code, Name = "[" + m.Code + "] " + m.ShortName + " " + m.Name + " " + m.City }).OrderBy(n => n.Name).ToList();
                    //return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            try
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
                decimal? OpeningBal = 0;
                var SDate = Convert.ToDateTime(Model.FromDate);
                var LDate = Convert.ToDateTime(Model.ToDate);
                int mlocation = Convert.ToInt32(ppara01 == null || ppara01 == "" ? "0" : ppara01);
                //string mpal = (ppara02 == null || ppara02 == "" ? "no" : ppara02).ToLower();
                //string mauth = (ppara04 == null || ppara04 == "" ? "no" : ppara04).ToLower();
                bool mpal = ppara02 == null || ppara02 == "" ? false : true;
                bool mauth = ppara04 == null || ppara04 == "" ? false : true;
                bool mmvpv = false;
                string mbranch = ppara05 == null || ppara05 == "" ? "'" + mbranchcode + "'" : ppara05;
                //if (mlocation != 0)
                //{
                OpeningBal = GetBalance(Model.Code, SDate.AddDays(-1), mbranch, mlocation, mpal, mauth, mmvpv);
                //(from L in ctxTFAT.Ledger
                //          where L.MainType != "MV" && L.MainType != "PV" && L.Code == Model.Code && L.DocDate < SDate.Date && L.Branch == mbranchcode && L.LocationCode == mlocation && (L.AUTHORISE.StartsWith("A") || mauth == "yes" ? L.AUTHORISE.StartsWith("N") : true) && (mpal != "no" ? true : L.Type.StartsWith("PAL") == false) && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                //          select L.Debit - L.Credit).DefaultIfEmpty(0).Sum();
                //}
                //else
                //{
                //OpeningBal = (from L in ctxTFAT.Ledger
                //where L.MainType != "MV" && L.MainType != "PV" && L.Code == Model.Code && L.DocDate < SDate.Date && L.Branch == mbranchcode && (L.AUTHORISE.StartsWith("A") || mauth == "yes" ? L.AUTHORISE.StartsWith("N") : true) && (mpal != "no" ? true : L.Type.StartsWith("PAL") == false) && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                //select L.Debit - L.Credit).DefaultIfEmpty(0).Sum();
                //}
                mOpeningBalance = OpeningBal != null ? OpeningBal.Value : 0;
                string mOpening = Math.Abs(mOpeningBalance) + (mOpeningBalance > 0 ? " Dr" : " Cr");
                //string.Format("{0,0:N2}", Math.Abs(mOpeningBalance)) + (mOpeningBalance > 0 ? " Dr" : " Cr");
                decimal? ClosingBal = 0;
                //if (mlocation != 0)
                //{
                //ClosingBal = (from L in ctxTFAT.Ledger
                //where L.MainType != "MV" && L.MainType != "PV" && L.Code == Model.Code && L.DocDate <= LDate.Date && L.Branch == mbranchcode && L.LocationCode == mlocation && (L.AUTHORISE.StartsWith("A") || mauth == "yes" ? L.AUTHORISE.StartsWith("N") : true) && (mpal != "no" ? true : L.Type.StartsWith("PAL") == false) && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                //select L.Debit - L.Credit).DefaultIfEmpty(0).Sum();
                //}
                //else
                //{
                ClosingBal = GetBalance(Model.Code, LDate.Date, mbranch, mlocation, mpal, mauth, mmvpv);

                //(from L in ctxTFAT.Ledger
                //where L.MainType != "MV" && L.MainType != "PV" && L.Code == Model.Code && L.DocDate <= LDate.Date && L.Branch == mbranchcode && (L.AUTHORISE.StartsWith("A") || mauth == "yes" ? L.AUTHORISE.StartsWith("N") : true) && (mpal != "no" ? true : L.Type.StartsWith("PAL") == false) && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                //select L.Debit - L.Credit).DefaultIfEmpty(0).Sum();
                //}
                ClosingBal = ClosingBal ?? 0;
                string mClosing = Math.Abs((decimal)ClosingBal) + (ClosingBal > 0 ? " Dr" : " Cr");
                //string.Format("{0,0:N2}", Math.Abs((decimal)ClosingBal)) + (ClosingBal > 0 ? " Dr" : " Cr");
                decimal? TCredit = GetBalanceDateRange(Model.Code, SDate.Date, LDate.Date, "C", mbranch, mlocation, mpal, mauth, mmvpv);
                //if (mlocation != 0)
                //{
                //    TCredit = (from L in ctxTFAT.Ledger
                //               where L.MainType != "MV" && L.MainType != "PV" && L.Code == Model.Code && L.DocDate >= SDate.Date && L.DocDate <= LDate.Date && L.Branch == mbranchcode && L.LocationCode == mlocation && (L.AUTHORISE.StartsWith("A") || mauth == "yes" ? L.AUTHORISE.StartsWith("N") : true) && (mpal != "no" ? true : L.Type.StartsWith("PAL") == false) && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                //               select L.Credit).DefaultIfEmpty(0).Sum();
                //}
                //else
                //{
                //    TCredit = (from L in ctxTFAT.Ledger
                //               where L.MainType != "MV" && L.MainType != "PV" && L.Code == Model.Code && L.DocDate >= SDate.Date && L.DocDate <= LDate.Date && L.Branch == mbranchcode && (L.AUTHORISE.StartsWith("A") || mauth == "yes" ? L.AUTHORISE.StartsWith("N") : true) && (mpal != "no" ? true : L.Type.StartsWith("PAL") == false) && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                //               select L.Credit).DefaultIfEmpty(0).Sum();
                //}
                string mCredit = string.Format("{0,0:N2}", TCredit != null ? TCredit : 0);
                decimal? TDebit = GetBalanceDateRange(Model.Code, SDate.Date, LDate.Date, "D", mbranch, mlocation, mpal, mauth, mmvpv);
                //if (mlocation != 0)
                //{
                //    TDebit = (from L in ctxTFAT.Ledger
                //              where L.MainType != "MV" && L.MainType != "PV" && L.Code == Model.Code && L.DocDate >= SDate.Date && L.DocDate <= LDate.Date && L.Branch == mbranchcode && L.LocationCode == mlocation && L.AUTHORISE.StartsWith("A") && (mpal != "no" ? true : L.Type.StartsWith("PAL") == false) && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                //              select L.Debit).DefaultIfEmpty(0).Sum();
                //}
                //else
                //{
                //    TDebit = (from L in ctxTFAT.Ledger
                //              where L.MainType != "MV" && L.MainType != "PV" && L.Code == Model.Code && L.DocDate >= SDate.Date && L.DocDate <= LDate.Date && L.Branch == mbranchcode && (L.AUTHORISE.StartsWith("A") || mauth == "yes" ? L.AUTHORISE.StartsWith("N") : true) && (mpal != "no" ? true : L.Type.StartsWith("PAL") == false) && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                //              select L.Debit).DefaultIfEmpty(0).Sum();
                //}
                string mDebit = string.Format("{0,0:N2}", TDebit != null ? TDebit : 0);
                ////IReportGridOperation mIlst = new ListViewGridOperationreport();
                //string mstr = "";
                //var mas = ctxTFAT.Master.Where(z => z.Code == Model.Code).Select(x => new { x.Name, x.City, x.BaseGr, x.Grp }).FirstOrDefault();
                //mstr += mas.Name + " " + mas.City.Trim() + "|";
                //mstr += (ctxTFAT.MasterGroups.Where(z => z.Code == mas.Grp).Select(x => x.Name).FirstOrDefault() ?? "") + "|";
                //var mvar = ctxTFAT.Address.Where(z => z.Code == Model.Code).Select(x => x).FirstOrDefault();
                //if (mvar != null)
                //{
                //    mstr += (mvar.Person ?? "") + "|";
                //    mstr += ((mvar.Adrl1 ?? "") + " " + (mvar.Adrl2 ?? "") + (mvar.Adrl3 ?? "") + " " + (mvar.Adrl4 ?? "") + ", " + (mvar.City ?? "") + " " + (mvar.State ?? "") + " " + (mvar.Country ?? "")) + "|";
                //    mstr += (mvar.GSTNo ?? "") + "|";
                //    mstr += (mvar.Mobile ?? "") + "|";
                //    mstr += (mvar.Email ?? "") + "|";
                //}
                //else
                //{
                //    mstr += "" + "|";
                //    mstr += "" + "|";
                //    mstr += "" + "|";
                //    mstr += "" + "|";
                //    mstr += "" + "|";
                //}
                //var mvar2 = ctxTFAT.MasterInfo.Where(z => z.Code == Model.Code).Select(x => x).FirstOrDefault();
                //if (mvar2 != null)
                //{
                //    mstr += (mvar2.CrLimit ?? 0) + "|";
                //    mstr += (mvar2.CrPeriod ?? 0) + "|";
                //    mstr += (mvar2.Rank ?? 0) + "|";
                //}
                //else
                //{
                //    mstr += 0 + "|";
                //    mstr += 0 + "|";
                //    mstr += 0 + "|";
                //}
                string mstr = "";
                DataTable mas = GetDataTable("Select Name, City, BaseGr, Grp from Master Where Code = '" + Model.Code + "'");
                //var mas = ctxooroo.Master.Where(z => z.Code == Model.Code).Select(x => new { x.Name, x.City, x.BaseGr, x.Grp }).FirstOrDefault();
                mstr += mas.Rows[0]["Name"] + " " + mas.Rows[0]["City"].ToString().Trim() + " | ";
                mstr += (Fieldoftable("MasterGroups", "Name", "Code = '" + mas.Rows[0]["Grp"].ToString() + "'") ?? "") + " |";
                //var mvar = ctxooroo.Address.Where(z => z.Code == Model.Code).Select(x => x).FirstOrDefault();
                DataTable mvar = GetDataTable("Select * from Address Where Code = '" + Model.Code + "'");
                if (mvar.Rows.Count>0)
                {
                    mstr += (mvar.Rows[0]["Person"] ?? "") + " | ";
                    mstr += (mvar.Rows[0]["Adrl1"] ?? "") + " " + (mvar.Rows[0]["Adrl2"] ?? "") + (mvar.Rows[0]["Adrl3"] ?? "") + " " + (mvar.Rows[0]["Adrl4"] ?? "") + ", " + (mvar.Rows[0]["City"] ?? "") + " " + (mvar.Rows[0]["State"] ?? "") + " " + (mvar.Rows[0]["Country"] ?? "") + " | ";
                    mstr += (mvar.Rows[0]["GSTNo"] ?? "") + " | ";
                    mstr += (mvar.Rows[0]["Mobile"] ?? "") + " | ";
                    mstr += (mvar.Rows[0]["Email"] ?? "") + " | ";
                    //mstr += (mvar.Person ?? "") + "|";
                    //mstr += ((mvar.Adrl1 ?? "") + " " + (mvar.Adrl2 ?? "") + (mvar.Adrl3 ?? "") + " " + (mvar.Adrl4 ?? "") + ", " + (mvar.City ?? "") + " " + (mvar.State ?? "") + " " + (mvar.Country ?? "")) + "|";
                    //mstr += (mvar.GSTNo ?? "") + "|";
                    //mstr += (mvar.Mobile ?? "") + "|";
                    //mstr += (mvar.Email ?? "") + "|";
                }
                else
                {
                    mstr += "" + "|";
                    mstr += "" + "|";
                    mstr += "" + "|";
                    mstr += "" + "|";
                    mstr += "" + "|";
                }
                //var mvar2 = ctxooroo.MasterInfo.Where(z => z.Code == Model.Code).Select(x => x).FirstOrDefault();
                DataTable mvar2 = GetDataTable("Select * from MasterInfo Where Code = '" + Model.Code + "'");
                if (mvar2.Rows.Count>0)
                {
                    mstr += (mvar2.Rows[0]["CrLimit"] ?? 0) + "|";
                    mstr += (mvar2.Rows[0]["CrPeriod"] ?? 0) + " | ";
                    mstr += (mvar2.Rows[0]["Rank"] ?? 0) + " | ";
                }
                else
                {
                    mstr += 0 + "|";
                    mstr += 0 + "|";
                    mstr += 0 + "|";
                }

                string mquery = "Select FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4)) as MonthName, Sum(Debit-Credit) as Amt from Ledger where Code='" + Model.Code + "' and docdate>='%RepStartDate' and docdate<='%RepEndDate' " + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(Type,'" + mDocString + "')=0") + " Group by Code,FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4))";
                string[] mArr = GetMonthlyBalance(mquery);
                mstr += string.Join(",", mArr) + "|";
                if (mas.Rows[0]["BaseGr"].ToString() == "D")
                //if (mas.BaseGr == "D")
                {
                    mquery = "Select FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4)) as MonthName, Sum(Amt) as Amt from Sales where Code='" + Model.Code + "' and docdate>='%RepStartDate' and docdate<='%RepEndDate'" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(Type,'" + mDocString + "')=0") + " Group by Code,FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4))";
                    mArr = GetMonthlyBalance(mquery);
                    mstr += string.Join(",", mArr) + "|";
                    mquery = "Select FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4)) as MonthName, Sum(Credit) as Amt from Ledger where MainType='RC' and Credit>0 and Code='" + Model.Code + "' and Docdate>='%RepStartDate' and Docdate<='%RepEndDate'" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(Type,'" + mDocString + "')= 0") + " Group by Code,FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4))";
                    mArr = GetMonthlyBalance(mquery);
                    mstr += string.Join(",", mArr) + "|";
                }
                else if (mas.Rows[0]["BaseGr"].ToString() == "S")
                //else if (mas.BaseGr == "S")
                {
                    mquery = "Select FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4)) as MonthName, Sum(Amt) as Amt from Purchase where Code='" + Model.Code + "' and docdate>='%RepStartDate' and docdate<='%RepEndDate'" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(Type,'" + mDocString + "')=0") + " Group by Code,FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4))";
                    mArr = GetMonthlyBalance(mquery);
                    mstr += string.Join(",", mArr) + "|";
                    mquery = "Select FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4)) as MonthName, Sum(Debit) as Amt from Ledger where MainType='PM' and Debit>0 and Code='" + Model.Code + "' and Docdate>='%RepStartDate' and Docdate<='%RepEndDate'" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(Type,'" + mDocString + "')=0") + " Group by Code,FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4))";
                    mArr = GetMonthlyBalance(mquery);
                    mstr += string.Join(",", mArr) + "|";
                }
                else // for General Ledger a/cs
                {
                    mstr += "" + "|";
                    mstr += "" + "|";
                }
                return GetGridDataColumns(Model.ViewDataId, "X", "", mOpening + "|" + mClosing + "|" + mCredit + "|" + mDebit, mstr, GetMonthString());
            }
            catch (Exception ex)
            {
                return Json(new { Status = "Error", Message = "Error! While Generating Report..\n" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetGridData(GridOption Model)
        {
            //ppara05 = ppara05 == null || ppara05 == "" ? "'" + mbranchcode + "'" : ppara05;
            return GetGridReport(Model, "R", "Code^" + Model.Code + (mpara != "" ? "~" + mpara : ""), true, mOpeningBalance);
        }

        public ActionResult GetExcel(GridOption Model)
        {
            Model.mWhat = "XLS";
            return GetGridReport(Model, "R", "Code^" + Model.SelectContent + (mpara != "" ? "~" + mpara : ""), true, mOpeningBalance, "", "A4");
        }

        #region subreport
        [HttpPost]
        public ActionResult GetGridStructureSales(GridOption Model)
        {
            //LedSalesRegister,LedSalesOrders,LedSalesStockReg,LedOSAgeing,LedFollowupRegister
            //LedPurchRegister,LedPurchOrders,LedPurchStockReg,LedOSAgeing,MthlyAccSummary
            ////IReportGridOperation mIlst = new ListViewGridOperationreport();
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        [HttpPost]
        public ActionResult GetGridDataSales(GridOption Model)
        {
            if (Model.ViewDataId == "LedOSAgeing" || Model.ViewDataId == "LedgerDailySummary")
            {
                //DataTable dtreport = new DataTable();
                SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                SqlCommand cmd = new SqlCommand("SPTFAT_" + Model.ViewDataId, tfat_conx)
                {
                    CommandType = CommandType.StoredProcedure
                };
                var date = Model.Date.Replace("-", "/").Split(':');
                Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                cmd.Parameters.Add("@mCode", SqlDbType.VarChar).Value = Model.Code ?? "";
                if (Model.ViewDataId == "LedOSAgeing")
                {
                    cmd.Parameters.Add("@mDate", SqlDbType.VarChar).Value = Model.ToDate;
                }
                else
                {
                    cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
                    cmd.Parameters.Add("@mStartDate", SqlDbType.Date).Value = Model.FromDate;
                    cmd.Parameters.Add("@mEndDate", SqlDbType.Date).Value = Model.ToDate;
                }
                cmd.Parameters.Add("@mDocString", SqlDbType.VarChar).Value = mDocString;
                tfat_conx.Open();
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                tfat_conx.Close();
            }
            else if (Model.ViewDataId == "LedgerMonthlySummary")
            {
                //DataTable dtreport = new DataTable();
                ExecuteStoredProc("Drop Table ztmp_TempMth");
                SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                SqlCommand cmd = new SqlCommand("SPTFAT_LedgerMthSummary", tfat_conx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;
                cmd.Parameters.Add("@mCode", SqlDbType.VarChar).Value = Model.Code ?? "";
                cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
                cmd.Parameters.Add("@mStartDate", SqlDbType.Date).Value = Convert.ToDateTime(System.Web.HttpContext.Current.Session["StartDate"].ToString());
                cmd.Parameters.Add("@mDocString", SqlDbType.VarChar).Value = mDocString;
                tfat_conx.Open();
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
                tfat_conx.Close();
            }
            return GetGridReport(Model, "R", "Document^" + (Model.Document ?? "") + "~Code^" + (Model.Code ?? ""), false, 0);
        }
        #endregion subreport
    }
}