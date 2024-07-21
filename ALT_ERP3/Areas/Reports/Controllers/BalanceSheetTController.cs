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
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Reports.Controllers
{
    public class BalanceSheetTController : BaseController
    {
        //tfatEntities ctxTFAT = new tfatEntities();
        private static string mReport = "BS";
        //decimal mOpeningBalance = 0;
        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, "Report", Model.Header, "", DateTime.Now, 0, "", "", "A");
            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            mReport = Model.MainType;
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

        public ActionResult GetLocations()
        {
            var result = ctxTFAT.Warehouse.Where(z => z.Branch == mbranchcode && (z.Users + ",").Contains(muserid + ",")).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            ////IReportGridOperation mIlst = new ListViewGridOperationreport();
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            PreExecute(Model);
            return GetGridReport(Model, "R", "",false, 0);
        }

        [HttpPost]
        public ActionResult GetSubGridStructureBS(GridOption Model)
        {
            if (Model.Document == null) return null;
            string msubgrid = "";
            string mstr = "";
            msubgrid = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).Select(z => z.DrillQuery).FirstOrDefault() ?? "";
            if (msubgrid.Trim() == "")
            {
                return Json(new { Status = "Error", Message = "Sub-Grid format not found.." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (Model.Document.Substring(0, 1) == "G")
                {
                    msubgrid = "AccountsBalances";
                }
                else
                {
                    // to get 12 months balances
                    mstr = string.Join(",", GetMonthlyBalance("", Model.Document.Substring(1)));
                }
                ////IReportGridOperation mIlst = new ListViewGridOperationreport();
                return GetGridDataColumns(msubgrid, "X", "", mstr);
            }
        }

        [HttpPost]
        public ActionResult GetSubGridDataBS(GridOption Model)
        {
            if (Model.Document == null) return null;
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
                if (Model.Document.Substring(0, 1) == "G")
                {
                    Model.ViewDataId = "AccountsBalances";
                }
                else
                {
                    Model.ViewDataId = msubgrid.Trim();
                }
                return GetGridReport(Model, "R", "Code^" + Model.Document.Substring(1), false, 0);
            }
        }

        //public ActionResult GetPDF(GridOption Model, string mwhat, string mpageorient, string mpapersize, string memaildata)
        //{
        //    PreExecute(Model);
        //    Model.mWhat = mwhat;
        //    Model.Code = Model.SelectContent;
        //    string[] mArr = { ppara1, ppara2, ppara3, ppara4, ppara5, ppara6, ppara7, ppara8, ppara9 };
        //    switch (mwhat)
        //    {
        //        case "RPDF":
        //            return ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Portrait", "", 0), Model.ViewDataId, "pdf", false, Model.SelectContent);
        //        case "RPDL":
        //            return ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", "", 0), Model.ViewDataId, "pdf", false, Model.SelectContent);
        //        case "RXLS":
        //            return ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", "", 0), Model.ViewDataId, "Excel", false, Model.SelectContent);
        //        case "RWRD":
        //            return ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", "", 0), Model.ViewDataId, "Word", false, Model.SelectContent);
        //        case "EPDF":
        //            return ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", "", 0), Model.ViewDataId, "pdf", true, Model.SelectContent, memaildata);
        //        case "EXLS":
        //            return ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", "", 0), Model.ViewDataId, "Excel", true, Model.SelectContent, memaildata);
        //        case "EWRD":
        //            return ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", "", 0), Model.ViewDataId, "Word", true, Model.SelectContent, memaildata);
        //        case "ECRPDF":   // crystal report format
        //            return PrintReportsCrystal(Model, "REP_" + Model.ViewDataId, "SPREP_" + Model.ViewDataId, "PDF", true, "Code^" + Model.SelectContent + (mpara != "" ? "~" + mpara : ""), mpageorient, mpapersize, memaildata);
        //        case "CRPDF":   // crystal report format
        //            return PrintReportsCrystal(Model, "REP_" + Model.ViewDataId, "SPREP_" + Model.ViewDataId, "PDF", false, "Code^" + Model.SelectContent + (mpara != "" ? "~" + mpara : ""), mpageorient, mpapersize, memaildata);
        //        default:
        //            break;
        //    }
        //    return GetGridReport(Model, "R", "Code^" + Model.SelectContent + (mpara != "" ? "~" + mpara : ""), true, 0, "", mpapersize);
        //}
    }
}