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
    public class PaymentReminderController : BaseController
    {
        tfatEntities ctxTFAT = new tfatEntities();
        private static string mmaintype = "";
        public ActionResult Index(GridOption Model)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "");
            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            ViewBag.ViewDataId = "ListofAccountsforSelect";
            Model.ViewCode = "ListofAccountsforSelect";
            Model.ViewDataId = "ListofAccountsforSelect";
            Model.BaseGr = Model.MainType;
            mmaintype = Model.MainType;
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
            return GetGridDataColumns(Model.ViewDataId);
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            return GetGridReport(Model, "R", "BaseGr^" + mmaintype);
        }

        public ActionResult GetExcel(GridOption Model)
        {
            Model.mWhat = "XLS";
            return GetGridReport(Model, "R", "", false, 0);
        }

        public ActionResult GetPDF(GridOption Model, string mwhat, string mpageorient, string mpapersize, string memaildata)
        {
            //Model.mWhat = mwhat;
            //string[] mArr = { "", "", "", "", "", "", "", "", "", "" };
            ////ActionResult mreturnvalue = Json("");
            //ActionResult mreturnvalue = Json(new {}, JsonRequestBehavior.AllowGet);
            ////~235103089~235101576~100100127~100100094~100100093'
            //var mcodes = Model.SelectContent.Split('~');
            //for (int x = 0; x < mcodes.Length - 1; x++)
            //{
            //    string mcode = mcodes[x].ToString();
            //    if (mcode != "")
            //    {
            //        Model.ViewDataId = "AcLedgerScreen";
            //        switch (mwhat)
            //        {
            //            case "RPDF":
            //                mreturnvalue = ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Portrait", "Code^" + mcode), Model.ViewDataId, "pdf", false, mcode);
            //                break;
            //            case "RPDL":
            //                mreturnvalue = ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", "Code^" + mcode), Model.ViewDataId, "pdf", false, mcode);
            //                break;
            //            case "RXLS":
            //                mreturnvalue = ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", "Code^" + mcode), Model.ViewDataId, "Excel", false, mcode);
            //                break;
            //            case "RWRD":
            //                mreturnvalue = ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", "Code^" + mcode), Model.ViewDataId, "Word", false, mcode);
            //                break;
            //            case "EPDF":
            //                mreturnvalue = ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", "Code^" + mcode), Model.ViewDataId, "pdf", true, mcode, memaildata);
            //                break;
            //            case "EXLS":
            //                mreturnvalue = ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", "Code^" + mcode), Model.ViewDataId, "Excel", true, mcode, memaildata);
            //                break;
            //            case "EWRD":
            //                mreturnvalue = ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", "Code^" + mcode), Model.ViewDataId, "Word", true, mcode, memaildata);
            //                break;
            //            case "ECRPDF":   // crystal report format
            //                PrintReportsCrystal(Model, "REP_" + Model.ViewDataId, "SPREP_" + Model.ViewDataId, "PDF", true, "Code^" + mcode, mpageorient, mpapersize, memaildata);
            //                break;
            //            case "CRPDF":   // crystal report format
            //                PrintReportsCrystal(Model, "REP_" + Model.ViewDataId, "SPREP_" + Model.ViewDataId, "PDF", false, "Code^" + mcode, mpageorient, mpapersize, memaildata);
            //                break;
            //            case "PDF":
            //                break;
            //            case "PDL":
            //                break;
            //        }
            //    }
            //    //GetGridReport(Model, "R", "", false, 0, "", mpapersize);
            //}
            //return mreturnvalue;
            Model.mWhat = mwhat;
            Model.Date = Model.Date.Replace("undefined:", "01-01-2000:");
            string[] mArr = { "", "", "", "", "", "", "", "", "", "" };
            ActionResult mreturnvalue = Json(new { }, JsonRequestBehavior.AllowGet);
            string mcode = Model.SelectContent;
            if (mcode != "")
            {
                Model.ViewDataId = "AcLedgerScreen";
                switch (mwhat)
                {
                    case "RPDF":
                        return ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Portrait", "Code^" + mcode), Model.ViewDataId, "pdf", false, mcode);
                        ///break;
                    case "RPDL":
                        return ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", "Code^" + mcode), Model.ViewDataId, "pdf", false, mcode);
                        //break;
                    case "RXLS":
                        return ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", "Code^" + mcode), Model.ViewDataId, "Excel", false, mcode);
                        //break;
                    case "RWRD":
                        return ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", "Code^" + mcode), Model.ViewDataId, "Word", false, mcode);
                        //break;
                    case "EPDF":
                        return ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", "Code^" + mcode), Model.ViewDataId, "pdf", true, mcode, memaildata);
                        //break;
                    case "EXLS":
                        return ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", "Code^" + mcode), Model.ViewDataId, "Excel", true, mcode, memaildata);
                        //break;
                    case "EWRD":
                        return ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", "Code^" + mcode), Model.ViewDataId, "Word", true, mcode, memaildata);
                        //break;
                    case "ECRPDF":   // crystal report format
                        return PrintReportsCrystal(Model, "REP_" + Model.ViewDataId, "SPREP_" + Model.ViewDataId, "PDF", true, "Code^" + mcode, mpageorient, mpapersize, memaildata);
                        //break;
                    case "CRPDF":   // crystal report format
                        return PrintReportsCrystal(Model, "REP_" + Model.ViewDataId, "SPREP_" + Model.ViewDataId, "PDF", false, "Code^" + mcode, mpageorient, mpapersize, memaildata);
                }
            }
            return mreturnvalue;
        }

        public ActionResult PrintReport(GridOption Model)
        {
            Model.mWhat = "PDF";
            return PrintReportsCrystal(Model, "REP_PaymentReminder", "SPREP_PaymentReminder", "PDF", false, "", "Landscape", "A4");
        }

        #region subreport
        [HttpPost]
        public ActionResult GetGridStructureSub(GridOption Model)
        {
            string msubgrid = "";
            msubgrid = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).Select(z => z.DrillQuery).FirstOrDefault() ?? "";
            if (msubgrid.Trim() == "")
            {
                return Json(new { Status = "Error", Message = "Sub-Grid format not found.." }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return GetGridDataColumns(msubgrid);
            }
        }

        [HttpPost]
        public ActionResult GetGridDataSub(GridOption Model)
        {
            string msubgrid = "";
            //DateTime mdate = Convert.ToDateTime(Model.ToDate);
            //decimal mbalance=(from L in ctxTFAT.Ledger
            //                  where L.Code == Model.Document && L.DocDate <= mdate && L.Branch == mbranchcode && L.AUTHORISE.StartsWith("A")
            //                  select L.Debit - L.Credit).DefaultIfEmpty(0).Sum() ?? 0;
            //Model.Balance = mbalance;
            msubgrid = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).Select(z => z.DrillQuery).FirstOrDefault() ?? "";
            if (msubgrid.Trim() == "")
            {
                return Json(new { Status = "Error", Message = "Sub-Grid format not found.." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                Model.ViewDataId = msubgrid.Trim();
                return GetGridReport(Model, "R", "Code^" + Model.Document);
            }
        }
        #endregion subreport
    }
}