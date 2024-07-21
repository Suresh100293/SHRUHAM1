using EntitiModel;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using ALT_ERP3.Controllers;


namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class MonthlyAccountSummaryController : BaseController
    {

        //GET: Accounts/MonthlyAccountSummary
        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, "Report", Model.Header, "", DateTime.Now, 0, "", "", "NA");
            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            // AccountSchedules
            ViewBag.ViewDataId = Model.ViewDataId;
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
                var result = ctxTFAT.MasterGroups.Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).Take(15).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.MasterGroups.Where(x => x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            return GetGridReport(Model, "R", "Grp^" + Model.Code, false, 0);
        }


        #region subreport
        [HttpPost]
        public ActionResult GetGridStructureSub(GridOption Model)
        {
            string msubgrid = "";
            msubgrid = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).Select(z => z.DrillQuery).FirstOrDefault() ?? "";
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
                return GetGridDataColumns(msubgrid, "X", "");
            }
        }

        [HttpPost]
        public ActionResult GetGridDataSub(GridOption Model)
        {
            string msubgrid = "";
            msubgrid = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).Select(z => z.DrillQuery).FirstOrDefault() ?? "";
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
                Model.ViewDataId = msubgrid.Trim();
                return GetGridReport(Model, "R", "Code^" + Model.Document, false, 0);
            }
        }
        #endregion subreport
    }
}