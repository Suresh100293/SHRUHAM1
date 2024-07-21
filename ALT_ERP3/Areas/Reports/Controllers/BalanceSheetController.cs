//using EntitiModel;
using ALT_ERP3.Controllers;
using System;
using System.Data;
using System.Globalization;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Reports.Controllers
{
    public class BalanceSheetController : BaseController
    {
        private static string mReport = "TB";
        private static string mReporttype = "TB";
        private string msubcodeof = "";

        // GET: Reports/BalanceSheet
        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, "Report", Model.Header, "", DateTime.Now, 0, "", "", "A");
            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();//GetEffectiveDate().Date.ToString();
            mReport = Model.MainType;
            mReporttype = Model.MainType;
            ViewBag.ViewDataId = Model.ViewDataId;
            Model.Branch = mbranchcode;
            Model.BranchName = GetBranchName(mbranchcode);
            msubcodeof = Model.ViewDataId;
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
            //Where(x => x.Grp!=x.Code)
            return Json(GetDataTableList("Select Code, Name from Warehouse Where Branch = '" + mbranchcode + "' and Charindex('" + muserid + "',Users)<>0 Order by Name"), JsonRequestBehavior.AllowGet);
            //var result = ctxTFAT.Warehouse.Where(z => z.Branch == mbranchcode && (z.Users + ",").Contains(muserid + ",")).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).ToList();
            //return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBranch()
        {
            if (muserid == "SUPER")
            {
                return Json(GetDataTableList("Select Code, Name from TfatBranch Order by Name"), JsonRequestBehavior.AllowGet);
                //var branchlist = ctxTFAT.TfatBranch.ToList().Select(b => new { b.Code, b.Name }).ToList();
                //return Json(branchlist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(GetDataTableList("Select Code, Name from TfatBranch Where Charindex('" + muserid + "',Users)<>0 Order by Name"), JsonRequestBehavior.AllowGet);
                //var branchlist = ctxTFAT.TfatBranch.Where(x => x.Users.ToUpper().Contains(muserid)).ToList().Select(b => new { b.Code, b.Name });
                //return Json(branchlist, JsonRequestBehavior.AllowGet);
            }
        }

        private void GetBSFormat(string mwhat)
        {
            switch (mwhat)
            {
                case "std":
                    mReport = mReporttype;
                    break;
                case "stdplus":
                    mReport = mReporttype + "L";
                    break;
                case "tee":
                    mReport = mReporttype;
                    break;
                case "teeplus":
                    mReport = mReporttype + "L";
                    break;
                case "monthly":
                    mReport = mReporttype;
                    break;
                case "monthlyplus":
                    mReport = mReporttype + "L";
                    break;
                case "branch":
                    mReport = mReporttype;
                    break;
                case "branchplus":
                    mReport = mReporttype + "L";
                    break;
            }
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            GetBSFormat(Model.THead);
            if (Model.THead == "monthly" || Model.THead == "monthlyplus")
            {
                Model.ViewDataId = Model.ViewDataId + "Monthly";
            }
            else if (Model.THead == "branch" || Model.THead == "branchplus")
            {
                Model.ViewDataId = Model.ViewDataId + "Branch";
            }
            if (Model.THead == "tee" || Model.THead == "teeplus")
            {
                Model.ViewDataId += "T";
            }
            Model.MainType = mReport;
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            //Model.THead = std,stdplus,tee,teeplus,monthly,branch
            //GetBSFormat(Model.ViewDataId, Model.THead);
            //GenerateGrpWithBalance(Model.Date, Model.Branch, mReport, Model.LocationCode, Model.THead);
            //if (Model.THead == "monthly" || Model.THead == "monthlyplus")
            //{
            //    Model.ViewDataId = Model.ViewDataId + "Monthly";
            //}
            //else if (Model.THead == "branch" || Model.THead == "branchplus")
            //{
            //    Model.ViewDataId = Model.ViewDataId + "Branch";
            //}
            //if (Model.THead == "tee" || Model.THead == "teeplus")
            //{
            //    Model.ViewDataId += "T";
            //}
            Model.MainType = mReport;
            PreExecute(Model);
            return GetGridReport(Model, "R", "", false, 0);
        }

        [HttpPost]
        public ActionResult GetSubGridStructureBS(GridOption Model)
        {
            if (Model.Document == null) return null;
            string mstr = "";
            string msubgrid = Fieldoftable("ReportHeader", "DrillQuery", "Code = '" + Model.ViewDataId + "'");
            //msubgrid = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).Select(z => z.DrillQuery).FirstOrDefault() ?? "";
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
                    msubgrid = "AccountsBalances";
                }
                else
                {
                    // to get 12 months balances
                    mstr = string.Join(",", GetMonthlyBalance("", Model.Document.Substring(1)));
                }
                return GetGridDataColumns(msubgrid, "X", "", mstr);
            }
        }

        [HttpPost]
        public ActionResult GetSubGridDataBS(GridOption Model)
        {
            string msubgrid = Fieldoftable("ReportHeader", "DrillQuery", "Code = '" + Model.ViewDataId + "'");
            //msubgrid = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).Select(z => z.DrillQuery).FirstOrDefault() ?? "";
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

        public ActionResult GetClosingStock(string mdate, string mbranch, int mlocation)
        {
            decimal mRate;
            var date = mdate.Replace("-", "/").Split(':');
            //string mFromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            string mToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            double mstock;
            double mvaluation = 0;
            if (mbranch == "") mbranch = mbranchcode;
            // only goods items
            //List<string> mitems = ctxTFAT.ItemMaster.Where(z => z.ItemType != "S" && z.Stocking != 4 && z.Stocking != 6 && z.Stocking != 9).Select(x => x.Code).ToList();
            DataTable mitems = GetDataTable("Select Code from ItemMaster where ItemType<>'S' and Stocking<>4 and Stocking<>6 and Stocking<>9");
            foreach (DataRow mrow in mitems.Rows)
            {
                string mitem = mrow["Code"].ToString();
                mstock = GetStock(mitem, 0, Convert.ToDateTime(mToDate), mbranch, 0, false);
                if (mstock != 0)
                {
                    //string mmethod = ctxTFAT.ItemMaster.Where(z => z.Code == mitem).Select(x => x.ValuationMethod).FirstOrDefault() ?? "";
                    string mmethod = Fieldoftable("ItemMaster", "ValuationMethod", "Code='" + mitem + "'");
                    switch (mmethod)
                    {
                        case "S":
                            mRate = FieldoftableNumber("ItemDetail", "SalesRate", "Code='" + mitem + "' and Branch='" + mbranchcode + "'");
                            break;
                        case "P":
                            mRate = FieldoftableNumber("ItemDetail", "PurchRate", "Code='" + mitem + "' and Branch='" + mbranchcode + "'");
                            break;
                        case "C":
                            mRate = FieldoftableNumber("", "dbo.fn_GetLastCostRate('" + mitem + "','" + mToDate + "',0,'" + mbranch + "'," + mlocation + ")", "");
                            break;
                        case "R":
                            mRate = FieldoftableNumber("", "dbo.fn_GetLastPurchaseRate('" + mitem + "','" + mToDate + "',0,'" + mbranch + "'," + mlocation + ")", "");
                            break;
                        case "F":
                            mRate = FieldoftableNumber("", "dbo.fn_GetFIFOValuation('" + mbranch + "','" + mitem + "',0,'F'," + mstock + ",'" + mToDate + "','" + mToDate + "','R')", "");
                            break;
                        case "L":
                            mRate = FieldoftableNumber("", "dbo.fn_GetFIFOValuation('" + mbranch + "','" + mitem + "',0,'L'," + mstock + ",'" + mToDate + "','" + mToDate + "','R')", "");
                            break;
                        default:
                            mRate = 0;
                            break;
                    }
                    mvaluation += mstock * (double)mRate;
                }
            }
            return Json(new { data = Math.Round(mvaluation, 4) }, JsonRequestBehavior.AllowGet);
        }
    }
}