using ALT_ERP3.Controllers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class BalanceSheetController : BaseController
    {
        private static string mReport = "TB";
        private static string mReporttype = "TB";
        private string msubcodeof = "";
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

        // GET: Accounts/BalanceSheet
        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, "Report", Model.Header, "", DateTime.Now, 0, "", "", "NA");
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


            ViewBag.PLClosing = "0";
            //Suresh Insert Extra Row
            //if (Model.ViewDataId == "ProfitLossStatement")
            //{
            //    string Parentkey = "PAL00" + mperiod.Substring(0, 2) + "1000001";
            //    var mlist = ctxTFAT.Ledger.Where(x => x.ParentKey == Parentkey && x.Code == "000100002").OrderBy(x => x.TableKey).FirstOrDefault();
            //    if (mlist!=null)
            //    {
            //        ViewBag.PLClosing = mlist.Debit > 0 ? mlist.Debit.Value.ToString("0.00") + "DR" : mlist.Credit.Value.ToString("0.00") + "CR";
            //    }
            //    else
            //    {
            //        ViewBag.PLClosing = "0";
            //    }
            //    //string Query= "Insert Into ztmp_temp Values (0,'','%boldblack','Expenditure','G','Adjust','{Profit Loss Clossing Journal }',999999,1,'',0,'"+ mlist.Debit + "','"+ mlist.Credit + "','"+(mlist.Debit - mlist.Credit) + "')";
            //    //ExecuteStoredProc(Query);
            //}

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
                return Json(GetDataTableList("Select Code, Name from TfatBranch where Code<>'G00000' and Grp<>'G00000' and Category<>'Area' Order by Name"), JsonRequestBehavior.AllowGet);
                //var branchlist = ctxTFAT.TfatBranch.ToList().Select(b => new { b.Code, b.Name }).ToList();
                //return Json(branchlist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(GetDataTableList("Select Code, Name from TfatBranch Where Code<>'G00000' and Grp<>'G00000' and Category<>'Area' and Charindex('" + muserid + "',Users)<>0 Order by Name"), JsonRequestBehavior.AllowGet);
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

        public ActionResult GetGridData(GridOption Model)
        {
            ViewBag.PLClosing = "";
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





            //Suresh Remove Total Row As Per Rajesh And Suresh Sir Said It.
            //ExecuteStoredProc("delete ztmp_temp where DisplayOrder=48");


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


                if (Model.ViewDataId == "AccountsBalances")
                {
                    ExecuteStoredProc("Drop Table ztmpAccountsBalancesS");
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("SPREP_AccountsBalances", tfat_conx);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.Add("@mCode", SqlDbType.VarChar).Value = Model.Document.Substring(1);
                    cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = Model.Branch == null ? "" : Model.Branch;
                    cmd.Parameters.Add("@mDate1", SqlDbType.VarChar).Value = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    cmd.Parameters.Add("@mDate2", SqlDbType.VarChar).Value = (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    cmd.Parameters.Add("@mSelectQuery", SqlDbType.VarChar, 5000).Value = "";
                    cmd.Parameters["@mSelectQuery"].Direction = ParameterDirection.Output;

                    tfat_conx.Open();
                    cmd.CommandTimeout = 0;
                    cmd.ExecuteNonQuery();

                    tfat_conx.Close();
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

        [HttpPost]
        public ActionResult GetGridStructureRecordsClosing(GridOption Model)
        {
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        public ActionResult GetGridDataClosing(GridOption Model)
        {
            return GetGridReport(Model, "R", "", false, 0);
        }

        public ActionResult GetClosingAmount()
        {
            var PLClosing = "0";
            var Closing = FieldoftableNumber("ztmp_temp", "sum(dr+cr)", " code ='000000059' ");
            if (Closing != 0)
            {
                PLClosing = Closing.ToString("0.00") + " CR";
            }
            else
            {
                Closing = FieldoftableNumber("ztmp_temp", "sum(dr+cr)", " code ='000000060' ");
                if (Closing != 0)
                {
                    PLClosing = Closing.ToString("0.00") + " DR";
                }
            }

            return Json(new { ClosingAmt = PLClosing.ToString() }, JsonRequestBehavior.AllowGet);


        }


    }
}