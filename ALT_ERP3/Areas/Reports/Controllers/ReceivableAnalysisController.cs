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
    public class ReceivableAnalysisController : BaseController
    {
        private static string mbasegr = "";
        decimal mOpeningBalance = 0;

        // GET: Reports/AccountLedger
        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, "Report", Model.Header, "", DateTime.Now, 0, "", "", "A");
            if (Model.Document == "" || Model.Document == null)
            {
                Model.FromDate = (new DateTime(1950, 1, 1)).ToShortDateString();
                //System.Web.HttpContext.Current.Session["StartDate"].ToString();
                Model.ToDate = DateTime.Today.ToShortDateString();
                //System.Web.HttpContext.Current.Session["LastDate"].ToString();
            }
            ExecuteStoredProc("dbo.SPTFAT_DeleteTempOS");
            mbasegr = Model.MainType;
            ViewBag.ViewDataId = Model.ViewDataId;
            Model.AccountName = NameofAccount(Model.Document);
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
            if (term == "")
            {
                var result = ctxTFAT.Master.Where(x => (x.BaseGr == mbasegr || x.BaseGr == "U") && x.Hide == false).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).Take(15).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.Master.Where(x => (x.BaseGr == mbasegr || x.BaseGr == "U") && x.Hide == false && x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            var date = Model.Date.Replace("-", "/").Split(':');
            Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand();// ("SPTFAT_ReceivableAnalysis", tfat_conx);
            cmd.Connection = tfat_conx;

            if (Model.ViewDataId == "Outstanding Report" || Model.ViewDataId == "Invoice wise Outstanding" || Model.ViewDataId == "OS Ageing" || Model.ViewDataId == "Party Ageing Summary")
            {
                cmd.CommandText = "SPTFAT_ReceivableAnalysis";
            }
            else
            {
                cmd.CommandText = "SPTFAT_ReceivableWithRefDoc";
            }

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@mBaseGr", SqlDbType.VarChar).Value = mbasegr;
            cmd.Parameters.Add("@mDate", SqlDbType.Date).Value = Model.ToDate;
            cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
            cmd.Parameters.Add("@mLocations", SqlDbType.VarChar).Value = Fieldoftable("Warehouse", "Code", "Name='" + ppara01 + "'");
            cmd.Parameters.Add("@mConsiderCRPR", SqlDbType.Bit).Value = ppara06 == "Yes" ? true : false;
            cmd.Parameters.Add("@mSuppressZero", SqlDbType.Bit).Value = ppara08 == "Yes" ? true : false;
            cmd.Parameters.Add("@mArea", SqlDbType.VarChar).Value = ppara04;
            cmd.Parameters.Add("@mParties", SqlDbType.VarChar).Value = Model.Code == null ? "" : Model.Code;
            cmd.Parameters.Add("@mGrps", SqlDbType.VarChar).Value = Fieldoftable("MasterGroups", "Code", "Name='" + ppara15 + "'");
            cmd.Parameters.Add("@mSalesman", SqlDbType.VarChar).Value = ppara02;//FieldoftableNumber("Salesman", "Code", "Name='" + 
            cmd.Parameters.Add("@mBroker", SqlDbType.VarChar).Value = ppara03;//FieldoftableNumber("Broker", "Code", "Name='" + ppara03 + "'");
            cmd.Parameters.Add("@mCategory", SqlDbType.VarChar).Value = ppara05;
            cmd.Parameters.Add("@mRefTillDate", SqlDbType.Bit).Value = ppara07 == "Yes" ? true : false;
            cmd.Parameters.Add("@mDocString", SqlDbType.VarChar).Value = mDocString;
            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            tfat_conx.Close();
            tfat_conx.Dispose();
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            return GetGridReport(Model, "R", mpara, false, 0);
        }
    }
}