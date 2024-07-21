using EntitiModel;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using ALT_ERP3.Controllers;
using ALT_ERP3.DynamicBusinessLayer;
using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Reports.Controllers
{
    public class StockStatementController : BaseController
    {
        tfatEntities ctxTFAT = new tfatEntities();
        private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        private string mperiod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        //IReportGridOperation mIlst = new ListViewGridOperationreport();
        double mOpeningBalance = 0;

        // GET: Reports/StockStatement
        public ActionResult Index(GridOption Model)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "");
            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            // StockStateBasicAdvanced
            ViewBag.ViewDataId = Model.ViewDataId;
            ViewBag.Header = Model.Header;
            ViewBag.Controller = Model.Controller;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;
            return View(Model);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            return mIlst.getGridDataColumns(Model.ViewDataId,"","","","","");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            DataTable dtreport = new DataTable();
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("SPTFAT_StockStatement", tfat_conx);
            cmd.CommandType = CommandType.StoredProcedure;
            var date = Model.Date.Replace("-", "/").Split(':');
            Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
            cmd.Parameters.Add("@mDate1", SqlDbType.VarChar).Value = Model.FromDate;
            cmd.Parameters.Add("@mDate2", SqlDbType.VarChar).Value = Model.ToDate;
            cmd.Parameters.Add("@mStores", SqlDbType.VarChar).Value = "";
            cmd.Parameters.Add("@mTypes", SqlDbType.VarChar).Value = "";
            cmd.Parameters.Add("@mCategory", SqlDbType.VarChar).Value = "";
            cmd.Parameters.Add("@mItemGroups", SqlDbType.VarChar).Value = "";
            cmd.Parameters.Add("@mItems", SqlDbType.VarChar).Value = "";
            cmd.Parameters.Add("@mLocations", SqlDbType.VarChar).Value = "";
            cmd.Parameters.Add("@mTransfer", SqlDbType.Bit).Value = 0;
            cmd.Parameters.Add("@mNonStock", SqlDbType.Bit).Value = 0;
            cmd.Parameters.Add("@mInActive", SqlDbType.Bit).Value = 0;
            cmd.Parameters.Add("@mFilter", SqlDbType.Int).Value = 0;
            cmd.Parameters.Add("@mCostRate", SqlDbType.Bit).Value = 0;
            cmd.Parameters.Add("@mMethod", SqlDbType.VarChar).Value = "X";
            tfat_conx.Open();
            cmd.ExecuteNonQuery();
            tfat_conx.Close();
            return GetGridReport(Model, "R", "", false, 0);
        }

        public ActionResult GetExcel(GridOption Model)
        {
            return GetGridData(Model);
        }

        public ActionResult GetPDF(GridOption Model)
        {
            return GetGridData(Model);
        }
    }
}