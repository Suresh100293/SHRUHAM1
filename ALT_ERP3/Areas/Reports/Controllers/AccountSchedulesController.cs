//using System.Web.UI.WebControls;
using ALT_ERP3.Controllers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Reports.Controllers
{
    public class AccountSchedulesController : BaseController
    {
        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            if (Model.Document == "" || Model.Document == null)
            {
                GetAllMenu(Session["ModuleName"].ToString());
                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                Model.ToDate = DateTime.Today.ToShortDateString();
            }
            else
            {
                Model.Code = Model.Document;
                Model.AccountName = NameofAccount(Model.Document, "G");
                //Model.AccountName = ctxTFAT.MasterGroups.Where(z => z.Code == Model.Document).Select(x => x.Name).FirstOrDefault();
            }
            UpdateAuditTrail(mbranchcode, "Report", Model.Header, Model.Document, DateTime.Now, 0, "", "", "A");
            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            // AccountSchedules
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

        public ActionResult GetAccountList(string term)
        {
            if (term == "")
            {
                return Json(GetDataTableList("Select Top 15 Code,Name from MasterGroups Order by Name"), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(GetDataTableList("Select Code,Name from MasterGroups Where Name like '%" + term + "%' Order by Name"), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            //////IReportGridOperation mIlst = new ListViewGridOperationreport();
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        //[HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            GenerateGrpWithBalance(Model.Date, mbranchcode, "TBL", Convert.ToInt32(ppara01 == null || ppara01 == "" ? "0" : ppara01), "", false, 0,true);
            SqlConnection conTFAT = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("dbo.SPTFAT_GetAccountSchedule", conTFAT);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@mGroup", SqlDbType.VarChar).Value = Model.Code ?? "";
            conTFAT.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            conTFAT.Dispose();
            return GetGridReport(Model, "R", "Code^" + Model.Code, true, 0);
        }

        public ActionResult GetExcel(GridOption Model)
        {
            Model.mWhat = "XLS";
            return GetGridData(Model);
        }

        #region subreport
        [HttpPost]
        public ActionResult GetGridStructureSub(GridOption Model)
        {
            string msubgrid = Fieldoftable("ReportHeader", "DrillQuery", "Code='" + Model.ViewDataId + "'", "T");
            if (msubgrid.Trim() == "")
            {
                return Json(new
                {
                    Status = "Error",
                    Message = "Sub-Grid format not found.."
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                // to get 12 months balances
                string mstr = string.Join(",", GetMonthlyBalance("", Model.Document.Trim()));
                return GetGridDataColumns(msubgrid, "X", "", mstr);
            }
        }

        [HttpPost]
        public ActionResult GetGridDataSub(GridOption Model)
        {
            string msubgrid = Fieldoftable("ReportHeader", "DrillQuery", "Code='" + Model.ViewDataId + "'", "T");
            if (msubgrid.Trim() == "")
            {
                return Json(new
                {
                    Status = "Error",
                    Message = "Sub-Grid format not found.."
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var date = Model.Date.Replace("-", "/").Split(':');
                Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                Model.ViewDataId = msubgrid.Trim();
                return GetGridReport(Model, "R", "Code^" + Model.Document.Trim(), false, 0);
            }
        }
        #endregion subreport
    }
}