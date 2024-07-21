using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
////using ALT_ERP3.DynamicBusinessLayer;
////using ALT_ERP3.DynamicBusinessLayer.Repository;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class TransactionChargesSetupController : BaseController
    {
        // GET: Accounts/TransactionChargesSetup
        //private //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        //private IBusinessCommon mBuss = new BusinessCommon();
        ////ITransactionGridOperation mIlst = new TransactionGridOperation();
        private int mlocation = 100001;
        //private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        //private string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        //private string muserid = System.Web.HttpContext.Current.Session["UserId"].ToString();
        //private string mperiod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        private int mnewrecordkey = 0;
        private static string mAUTHORISE = "A00";
        private static int mdocument = 0;
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";

        #region GetLists
        public List<SelectListItem> GetEqAmtList()
        {
            List<SelectListItem> CallEqAmtList = new List<SelectListItem>();
            CallEqAmtList.Add(new SelectListItem { Value = " ", Text = " " });
            CallEqAmtList.Add(new SelectListItem { Value = "+", Text = "+" });
            CallEqAmtList.Add(new SelectListItem { Value = "-", Text = "-" });
            return CallEqAmtList;
        }
        public List<SelectListItem> GetEqCostList()
        {
            List<SelectListItem> CallEqCostList = new List<SelectListItem>();
            CallEqCostList.Add(new SelectListItem { Value = " ", Text = " " });
            CallEqCostList.Add(new SelectListItem { Value = "+", Text = "+" });
            CallEqCostList.Add(new SelectListItem { Value = "-", Text = "-" });
            return CallEqCostList;
        }
        public List<SelectListItem> GetEqSaleList()
        {
            List<SelectListItem> CallEqSaleList = new List<SelectListItem>();
            CallEqSaleList.Add(new SelectListItem { Value = " ", Text = " " });
            CallEqSaleList.Add(new SelectListItem { Value = "+", Text = "+" });
            CallEqSaleList.Add(new SelectListItem { Value = "-", Text = "-" });
            return CallEqSaleList;
        }
        public List<SelectListItem> GetEqBroList()
        {
            List<SelectListItem> CallEqBroList = new List<SelectListItem>();
            CallEqBroList.Add(new SelectListItem { Value = " ", Text = " " });
            CallEqBroList.Add(new SelectListItem { Value = "+", Text = "+" });
            CallEqBroList.Add(new SelectListItem { Value = "-", Text = "-" });
            return CallEqBroList;
        }
        public List<SelectListItem> GetEqTaxList()
        {
            List<SelectListItem> CallEqTaxList = new List<SelectListItem>();
            CallEqTaxList.Add(new SelectListItem { Value = " ", Text = " " });
            CallEqTaxList.Add(new SelectListItem { Value = "+", Text = "+" });
            CallEqTaxList.Add(new SelectListItem { Value = "-", Text = "-" });
            return CallEqTaxList;
        }
        public JsonResult AutoCompleteCode(string term)
        {
            return Json((from m in ctxTFAT.Master
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteHSNCode(string term)
        {
            return Json((from m in ctxTFAT.HSNMaster
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteTaxCode(string term)
        {
            return Json((from m in ctxTFAT.TaxMaster
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteMainType(string term)
        {
            return Json((from m in ctxTFAT.MainTypes
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteSubType(string term)
        {
            var mPara = term.Split('^');
            var mP1 = mPara[0].ToLower();
            var mP2 = mPara[1];
            // linq doesnt support array in query, so parameters are stored in var
            return Json((from m in ctxTFAT.SubTypes
                         where m.MainType == mP2 && m.Name.ToLower().Contains(mP1)
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public List<SelectListItem> DefaultType()
        {
            List<SelectListItem> CallEqAmtList = new List<SelectListItem>();
            CallEqAmtList.Add(new SelectListItem { Value = "LR000", Text = "Consignment And Bill Charges" });
            CallEqAmtList.Add(new SelectListItem { Value = "FM000", Text = "Freight Memo Charges" });
            CallEqAmtList.Add(new SelectListItem { Value = "FMH00", Text = "Hire Freight Memo Charges" });
            CallEqAmtList.Add(new SelectListItem { Value = "FMP00", Text = "Advance && Balance Payment Charges" });
            CallEqAmtList.Add(new SelectListItem { Value = "BRC00", Text = "Debtor Receipt Charges" });
            CallEqAmtList.Add(new SelectListItem { Value = "BPM00", Text = "Creditor Payment Charges" });
            return CallEqAmtList;
        }


        public JsonResult AutoCompleteType(string term)
        {
            List<SelectListItem> CallEqAmtList = new List<SelectListItem>();
            CallEqAmtList.Add(new SelectListItem { Value = "LR000", Text = "Consignment And Bill Charges" });
            CallEqAmtList.Add(new SelectListItem { Value = "FM000", Text = "Freight Memo Charges" });
            CallEqAmtList.Add(new SelectListItem { Value = "FMH00", Text = "Hire Freight Memo Charges" });
            CallEqAmtList.Add(new SelectListItem { Value = "FMP00", Text = "Advance && Balance Payment Charges" });
            CallEqAmtList.Add(new SelectListItem { Value = "BRC00", Text = "Debtor Receipt Charges" });
            CallEqAmtList.Add(new SelectListItem { Value = "BPM00", Text = "Creditor Payment Charges" });

            if (String.IsNullOrEmpty(term))
            {
                var Modified = CallEqAmtList.Select(x => new
                {
                    Code = x.Value,
                    Name = x.Text
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = CallEqAmtList.Where(x => x.Text.ToLower().Trim().Contains(term.Trim().ToLower())).ToList();
                var Modified = list.Select(x => new
                {
                    Code = x.Value,
                    Name = x.Text
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }


            //var mPara = term.Split('^');
            //var mP1 = mPara[0].ToLower();
            //var mP2 = mPara[1];
            //var mP3 = mPara[2];
            //return Json((from m in ctxTFAT.DocTypes
            //             where m.MainType == mP2 && m.SubType == mP3 && m.Name.ToLower().Contains(mP1)
            //             select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        #endregion GetLists

        #region Index List

        // GET: Accounts/MasterGrid
        public ActionResult Index(GridOption Model)
        {
            //GetAllMenu(Session["ModuleName"].ToString());
            ////AccountingGetAllMenu(Session["ModuleName"].ToString());
            ////VehicleGetAllMenu(Session["ModuleName"].ToString());
            GetAllMenu(Session["ModuleName"].ToString());

            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "NA");

            if (Model.Document == null)
            {
                Model.Document = "";
                Model.AccountName = "";
            }
            moptioncode = Model.OptionCode;
            msubcodeof = Model.ViewDataId;
            mmodule = Model.Module;
            ViewBag.id = Model.ViewDataId;
            ViewBag.ViewDataId = Model.ViewDataId;
            ViewBag.Header = Model.Header;
            ViewBag.Table = Model.TableName;
            ViewBag.Controller = Model.Controller;
            ViewBag.MainType = Model.MainType;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;
            ViewBag.ViewName = Model.ViewName;
            if (Model.AcType != null)
            {
                ViewBag.MainType = Model.AcType;
            }

            Model.TempMainType = TempData.Peek("TempMainType") as string;
            Model.TempMainTypeN = ctxTFAT.MainTypes.Where(x => x.Code == Model.TempMainType).Select(x => x.Name).FirstOrDefault();
            Model.TempSubType = TempData.Peek("TempSubType") as string;
            Model.TempSubTypeN = ctxTFAT.SubTypes.Where(x => x.Code == Model.TempSubType).Select(x => x.Name).FirstOrDefault();
            Model.TempType = TempData.Peek("TempType") as string;
            Model.TempTypeN = ctxTFAT.DocTypes.Where(x => x.Code == Model.TempType).Select(x => x.Name).FirstOrDefault();
            var Doctypes = DefaultType();
            Model.TempTypeN = Doctypes.Where(x => x.Value == Model.TempType).Select(x => x.Text).FirstOrDefault();

            return View(Model);
        }

        public ActionResult GetFormats()
        {
            var result = ctxTFAT.ReportHeader.Where(x => x.SubCodeOf == msubcodeof && !x.Code.EndsWith(".bak")).Select(m => new
            {
                Code = m.Code,
                Name = m.Code
            }).OrderBy(n => n.Code).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
            var result = ctxTFAT.TfatMenu.Where(z => z.OptionCode == moptioncode && z.ModuleName == mmodule).Select(x => new { x.AllowEdit, x.AllowDelete }).FirstOrDefault();
            string mopt = "EDVX";
            if (result != null)
            {
                mopt = (result.AllowEdit == false ? "X" : "E") + (result.AllowDelete == false ? "X" : "D") + "VX";
            }
            //return GetGridDataColumns(id, "L", mopt);
            return GetGridDataColumns(id, "L", mopt);
        }

        //[HttpPost]
        public ActionResult GetMasterGridData(GridOption Model)
        {
            TempData["TempMainType"] = Model.TempMainType;
            TempData["TempSubType"] = Model.TempSubType;
            TempData["TempType"] = Model.TempType;

            //if (String.IsNullOrEmpty(Model.TempMainType))
            //{
            //    Model.TempMainType = TempData.Peek("TempMainType") as string;
            //}
            //if (String.IsNullOrEmpty(Model.TempSubType))
            //{
            //    Model.TempSubType = TempData.Peek("TempSubType") as string;
            //}
            //if (String.IsNullOrEmpty(Model.TempType))
            //{
            //    Model.TempType = TempData.Peek("TempType") as string;
            //}


            if (Model.IsFormatSelected == true)
            {
                int noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=0 Where SubCodeOf='" + msubcodeof + "'");
                noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=-1 Where Code='" + Model.ViewDataId + "'");
            }
            if (!String.IsNullOrEmpty(Model.searchField))
            {
                Model.Filter = null;
            }
            if (String.IsNullOrEmpty(Model.Filter))
            {
                return GetGridReport1(Model, "M", "MainType^" + Model.MainType, false, 0);
            }
            else
            {
                return GetGridReport1(Model, "M", "MainType^" + Model.MainType, false, 0, Model.Filter);
            }
        }

        public ActionResult GetGridReport1(GridOption Model, string mReportType = "R", string mParaString = "", bool mRunning = false, float mopening = 0, string mFilter = "", string mpapersize = "A4")
        {
            string connstring = GetConnectionString();
            string mFixedPara = "";
            if (Model.Para != null)
            {
                mFixedPara = Model.Para.ToString();
            }
            if (mFixedPara != "")
            {
                mFixedPara += "~";
            }
            mParaString = mFixedPara + mParaString;

            string mWhat = Model.mWhat == null ? "" : Model.mWhat;
            int startIndex = mWhat == "" ? (Model.page - 1) * Model.rows + 1 : -1;
            int endIndex = mWhat == "" ? (Model.page * Model.rows) : -1;

            SqlDataAdapter da = new SqlDataAdapter();
            using (DataTable dt = new DataTable())
            {
                SqlCommand cmd = new SqlCommand();

                //string mFilter = "";
                if (Model.searchField != "" && Model.searchField != null && mFilter == "")
                {
                    switch (Model.searchOper)
                    {
                        case "eq":
                            mFilter = Model.searchField + " = '" + Model.searchString + "'";
                            break;
                        case "ne":
                            mFilter = Model.searchField + " <> " + Model.searchString;
                            break;
                        case "bw":
                            mFilter = Model.searchField + " like '" + Model.searchString + "%'";
                            break;
                        case "bn":
                            mFilter = Model.searchField + " Not like '" + Model.searchString + "%'";
                            break;
                        case "ew":
                            mFilter = Model.searchField + " like '%" + Model.searchString + "'";
                            break;
                        case "en":
                            mFilter = Model.searchField + " Not like '%" + Model.searchString + "'";
                            break;
                        case "cn":
                            mFilter = Model.searchField + " like '%" + Model.searchString + "%'";
                            break;
                        case "in":
                            mFilter = Model.searchField + " like '%" + Model.searchString + "%'";
                            break;
                        case "nc":
                            mFilter = Model.searchField + " Not like '%" + Model.searchString + "%'";
                            break;
                        case "ni":
                            mFilter = Model.searchField + " Not like '%" + Model.searchString + "%'";
                            break;
                    }
                }
                else if (mFilter == "Filter")
                {
                    var FilterString = "";
                    if (!String.IsNullOrEmpty(Model.TempMainType))
                    {
                        FilterString += "MainType" + " = '" + Model.TempMainType + "' and ";
                    }
                    if (!String.IsNullOrEmpty(Model.TempSubType))
                    {
                        FilterString += "SubType" + " = '" + Model.TempSubType + "' and ";
                    }
                    if (!String.IsNullOrEmpty(Model.TempType))
                    {
                        FilterString += "Type" + " = '" + Model.TempType + "' and ";
                    }

                    mFilter = FilterString.Substring(0, FilterString.Length - 4);
                }

                try
                {
                    SqlConnection con = new SqlConnection(connstring);
                    cmd = new SqlCommand("dbo.ExecuteReport", con)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.Add("@mFormatCode", SqlDbType.VarChar).Value = Model.ViewDataId;
                    cmd.Parameters.Add("@mAlias", SqlDbType.VarChar).Value = "";
                    cmd.Parameters.Add("@mCurrDec", SqlDbType.TinyInt).Value = 2;
                    cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
                    cmd.Parameters.Add("@mUserID", SqlDbType.VarChar).Value = muserid;
                    if (mReportType == "M")
                    {
                        cmd.Parameters.Add("@mReportStartDate", SqlDbType.VarChar).Value = "2018-01-01";
                        cmd.Parameters.Add("@mReportEndDate", SqlDbType.VarChar).Value = "2019-01-01";
                    }
                    else
                    {
                        if (Model.Date != null)
                        {
                            var date = Model.Date.Replace("-", "/").Split(':');
                            Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            Model.FromDate = (Convert.ToDateTime(Model.FromDate)).ToString("yyyy-MM-dd");
                            Model.ToDate = (Convert.ToDateTime(Model.ToDate)).ToString("yyyy-MM-dd");
                        }
                        cmd.Parameters.Add("@mReportStartDate", SqlDbType.VarChar).Value = Model.FromDate.Trim();
                        cmd.Parameters.Add("@mReportEndDate", SqlDbType.VarChar).Value = Model.ToDate.Trim();
                    }
                    cmd.Parameters.Add("@mIsRunBalance", SqlDbType.Bit).Value = false;// mRunning;
                    cmd.Parameters.Add("@mOrderBy", SqlDbType.VarChar).Value = Model.sidx != null ? Model.sidx.Replace(",", "") + ' ' + (Model.sidx.Contains("asc") || Model.sidx.Contains("desc") ? "" : Model.sord) : "";
                    cmd.Parameters.Add("@mStartIndex", SqlDbType.Int).Value = startIndex;
                    cmd.Parameters.Add("@mEndIndex", SqlDbType.Int).Value = endIndex;
                    cmd.Parameters.Add("@mRunBalance", SqlDbType.Float).Value = mRunning == true ? mopening : 0;
                    cmd.Parameters.Add("@mInsertIntoTable", SqlDbType.VarChar).Value = "";// mRunning == true ? Model.ViewDataId : "";
                    cmd.Parameters.Add("@mPara", SqlDbType.VarChar).Value = mParaString;
                    cmd.Parameters.Add("@mFilter", SqlDbType.VarChar).Value = mFilter;

                    // for output
                    cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
                    cmd.Parameters.Add("@mSumString", SqlDbType.VarChar, 5000).Value = "";
                    cmd.Parameters["@mSumString"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;

                    con.Open();
                    da.SelectCommand = cmd;
                    da.Fill(dt);
                    string mSumString = (string)cmd.Parameters["@mSumString"].Value;
                    string mReturnString = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
                    con.Close();
                    con.Dispose();

                    // physical merge rows
                    var mvar = ctxTFAT.ReportHeader.Where(z => z.Code == Model.ViewDataId).Select(x => new { x.pMerge, x.pToMerge, x.pBlank }).FirstOrDefault();
                    string mpmerge = mvar.pMerge != null ? mvar.pMerge.Trim() : "";

                    if (mpmerge != "")
                    {
                        string mptomerge = mvar.pToMerge == null ? mvar.pToMerge.Trim() : "";
                        bool mpblank = true; // mvar.pBlank; its forcibly set to true since false raises some error
                        var marr = mpmerge.Split(',');
                        if (mptomerge.EndsWith(",") == false) mptomerge += ",";
                        string mstr = "";
                        Array mtotals = Array.CreateInstance(typeof(double), dt.Columns.Count);
                        Array mfixeds = Array.CreateInstance(typeof(double), dt.Columns.Count);
                        bool mflg = false;
                        int mstart = 0;
                        for (int n = 0; n <= dt.Rows.Count - 1; n++)
                        {
                            string mstr2 = "";
                            for (int m = 0; m <= marr.Count() - 1; m++)
                            {
                                if (marr[m] != "")
                                    mstr2 += dt.Rows[n][Convert.ToInt32(marr[m]) - 1];
                            }

                            if (mstr == mstr2)
                            {
                                for (int z = 0; z <= dt.Columns.Count - 1; z++)
                                {
                                    if (mptomerge.Contains((z + 1).ToString() + ","))
                                    {
                                        mtotals.SetValue(Convert.ToDouble(mtotals.GetValue(z)) + Convert.ToDouble(dt.Rows[mstart][z]), z);
                                        mfixeds.SetValue(Convert.ToDouble(mfixeds.GetValue(z)) + Convert.ToDouble(dt.Rows[mstart][z]), z);
                                    }
                                }
                                if (mpblank == false)
                                {
                                    dt.Rows[n].Delete();
                                }
                                else
                                {
                                    for (int m = 0; m <= marr.Count() - 1; m++)
                                    {
                                        if (marr[m] != "")
                                        {
                                            //dt.Rows[n][m] = "";
                                            int x = Convert.ToInt32(marr[m]) - 1;
                                            if (dt.Columns[x].DataType == System.Type.GetType("System.Byte") || dt.Columns[x].DataType == System.Type.GetType("System.Decimal") || dt.Columns[x].DataType == System.Type.GetType("System.Double") || dt.Columns[x].DataType == System.Type.GetType("System.Int16") || dt.Columns[x].DataType == System.Type.GetType("System.Int32") || dt.Columns[x].DataType == System.Type.GetType("System.Int64") || dt.Columns[x].DataType == System.Type.GetType("System.Single"))
                                            {
                                                dt.Rows[n][x] = 0;
                                            }
                                            else
                                            {
                                                dt.Rows[n][x] = "";
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int z = 0; z <= dt.Columns.Count - 1; z++)
                                {
                                    if (mptomerge.Contains((z + 1).ToString() + ","))
                                    {
                                        if (mflg == true)
                                        {
                                            dt.Rows[mstart][z] = mtotals.GetValue(z);

                                            if (Convert.ToDouble(mfixeds.GetValue(z)) == 0)
                                            {
                                                if (dt.Columns[z].DataType == System.Type.GetType("System.Byte") || dt.Columns[z].DataType == System.Type.GetType("System.Decimal") || dt.Columns[z].DataType == System.Type.GetType("System.Double") || dt.Columns[z].DataType == System.Type.GetType("System.Int16") || dt.Columns[z].DataType == System.Type.GetType("System.Int32") || dt.Columns[z].DataType == System.Type.GetType("System.Int64") || dt.Columns[z].DataType == System.Type.GetType("System.Single"))
                                                {
                                                    dt.Rows[mstart][z] = 0;
                                                }
                                                else
                                                {
                                                    dt.Rows[mstart][z] = "";
                                                }
                                            }
                                        }
                                        mtotals.SetValue(Convert.ToDouble(dt.Rows[mstart][z]), z);
                                        mfixeds.SetValue(Convert.ToDouble(dt.Rows[mstart][z]), z);
                                    }
                                }
                                mstart = n;
                                mstr = mstr2;
                            }
                            mflg = true;
                        }

                        for (int z = 0; z <= dt.Columns.Count - 1; z++)
                        {
                            if (mptomerge.Contains((z + 1).ToString() + ","))
                            {
                                dt.Rows[mstart][z] = mtotals.GetValue(z);
                                if (Convert.ToDouble(mfixeds.GetValue(z)) == 0)
                                {
                                    //if (dt.Columns[z].DataType == System.Type.GetType("System.String"))
                                    //{
                                    //    dt.Rows[mstart][z] = "";
                                    //}else 
                                    if (dt.Columns[z].DataType == System.Type.GetType("System.Byte") || dt.Columns[z].DataType == System.Type.GetType("System.Decimal") || dt.Columns[z].DataType == System.Type.GetType("System.Double") || dt.Columns[z].DataType == System.Type.GetType("System.Int16") || dt.Columns[z].DataType == System.Type.GetType("System.Int32") || dt.Columns[z].DataType == System.Type.GetType("System.Int64") || dt.Columns[z].DataType == System.Type.GetType("System.Single"))
                                    {
                                        dt.Rows[mstart][z] = 0;
                                    }
                                    else
                                    {
                                        dt.Rows[mstart][z] = "";
                                    }
                                }
                            }
                        }
                    }
                    // merge routine over

                    if (mRunning == true)
                    {
                        int mbalcol = -1;
                        int mruncol = -1;
                        int i;
                        for (i = 0; i < dt.Columns.Count; i++)
                        {
                            string mcolname = dt.Columns[i].ColumnName.Trim().ToLower();
                            if (mcolname == "balancefield")
                            {
                                mbalcol = i;
                            }
                            if (mcolname == "runningbalance" || mcolname == "balance")
                            {
                                mruncol = i;
                            }
                        }
                        if (mbalcol != -1 && mruncol != -1)
                        {
                            decimal mbal = (decimal)mopening;
                            foreach (DataRow dr in dt.Rows)
                            {
                                mbal += (decimal)dr[mbalcol];
                                dr[mruncol] = mbal;
                            }
                        }
                    }

                    //string mSumJson = "";
                    StringBuilder jsonBuilder = new StringBuilder();
                    if (mReportType == "R" && dt.Rows.Count > 0)
                    {
                        DataTable msumdt = GetDataTable(@mSumString, connstring);
                        if (msumdt.Rows.Count > 0)
                        {
                            jsonBuilder.Append(",\"userdata\":{");
                            foreach (DataRow row in msumdt.Rows)
                            {
                                //jsonBuilder.Append("\"name\":\"Total:\",");
                                foreach (DataColumn column in msumdt.Columns)
                                {
                                    jsonBuilder.Append("\"" + column.ColumnName + "\":");
                                    jsonBuilder.Append(row[column].ToString() + ",");
                                }
                            }
                            if (jsonBuilder.ToString().EndsWith(","))
                                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                            jsonBuilder.Append("}");
                        }
                        msumdt.Dispose();
                    }
                    else
                    {
                        //jsonBuilder.Append("");
                    }

                    if (mReportType != "R" && mWhat != "PDF" && mWhat != "PDL")
                    {
                        dt.Columns.Add("XYZ", typeof(string)).SetOrdinal(0);
                        dt.Columns.Add("ABC", typeof(string)).SetOrdinal(1);
                        dt.Columns.Add("FGH", typeof(string)).SetOrdinal(2);
                        dt.Columns.Add("PTG", typeof(string)).SetOrdinal(3);
                    }

                    if (mWhat == "")
                    {
                        // for count
                        SqlDataAdapter da2 = new SqlDataAdapter();
                        DataTable dt2 = new DataTable();
                        SqlCommand cmd2 = new SqlCommand();
                        SqlConnection con2 = new SqlConnection(connstring);
                        cmd2 = new SqlCommand("dbo.GetRowCount", con2)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmd2.Parameters.Add("@mFormatCode", SqlDbType.VarChar).Value = Model.ViewDataId;
                        cmd2.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
                        cmd2.Parameters.Add("@mUserID", SqlDbType.VarChar).Value = muserid;
                        if (mReportType == "M")
                        {
                            cmd2.Parameters.Add("@mReportStartDate", SqlDbType.VarChar).Value = "2018-01-01";
                            cmd2.Parameters.Add("@mReportEndDate", SqlDbType.VarChar).Value = "2019-01-01";
                        }
                        else
                        {
                            cmd2.Parameters.Add("@mReportStartDate", SqlDbType.VarChar).Value = Model.FromDate;
                            cmd2.Parameters.Add("@mReportEndDate", SqlDbType.VarChar).Value = Model.ToDate;
                        }
                        cmd2.Parameters.Add("@mPara", SqlDbType.VarChar).Value = mParaString;
                        cmd2.Parameters.Add("@mFilter", SqlDbType.VarChar).Value = mFilter;
                        // for output
                        cmd2.Parameters.Add("@mRowCount", SqlDbType.Int).Value = 0;
                        cmd2.Parameters["@mRowCount"].Direction = ParameterDirection.Output;
                        con2.Open();
                        da2.SelectCommand = cmd2;
                        int mxRowCount = 0;
                        try { da2.Fill(dt2); mxRowCount = (int)cmd2.Parameters["@mRowCount"].Value; } catch { mxRowCount = Model.rows; }
                        cmd2.Dispose();
                        dt2.Dispose();
                        da2.Dispose();
                        con2.Close();
                        con2.Dispose();
                        return Content(JQGridHelper.JsonForJqgrid(dt, Model.rows, mxRowCount, Model.page, jsonBuilder.ToString()), "application/json");
                    }
                    else     // XLS or PDF
                    {
                        if (Model.mWhat == "XLS")
                        {
                            string attachment = "attachment; filename=" + Model.ViewDataId + @".xls";
                            Response.ClearContent();
                            Response.AddHeader("content-disposition", attachment);
                            Response.ContentType = "application/vnd.ms-excel";
                            string tab = "";
                            foreach (DataColumn dc in dt.Columns)
                            {
                                Response.Write(tab + dc.ColumnName);
                                tab = "\t";
                            }
                            Response.Write("\n");
                            int i;
                            foreach (DataRow dr in dt.Rows)
                            {
                                tab = "";
                                for (i = 0; i < dt.Columns.Count; i++)
                                {
                                    Response.Write(tab + dr[i].ToString().Replace("\"", "").Replace(@"\", "").Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace("\b", ""));
                                    tab = "\t";
                                }
                                Response.Write("\n");
                            }
                            Response.End();
                        }
                        else if (Model.mWhat == "PDF" || Model.mWhat == "PDL")
                        {
                            Model.AccountDescription = ctxTFAT.ReportHeader.Where(z => z.Code == Model.ViewDataId).Select(x => x.FormatHead).FirstOrDefault() ?? "";
                            if (Model.AccountDescription != "")
                            {
                                Model.AccountDescription = Model.AccountDescription.Replace("%RepStartDate", Model.FromDate);
                                Model.AccountDescription = Model.AccountDescription.Replace("%RepEndDate", Model.ToDate);
                            }
                            CreatePDF(Model, dt, Model.AccountDescription, Model.mWhat == "PDL" ? "Landscape" : "Portrait", mpapersize);
                        }
                        else if (Model.mWhat == "SRS")
                        {
                            //DS_xReport ds = new DS_xReport();
                            //ReportViewer reportViewer = new Microsoft.Reporting.WebForms.ReportViewer();
                            //reportViewer.ProcessingMode = ProcessingMode.Local;
                            //reportViewer.SizeToReportContent = true;
                            //reportViewer.ZoomMode = ZoomMode.PageWidth;
                            //reportViewer.ShowToolBar = true;
                            //reportViewer.AsyncRendering = true;
                            //reportViewer.Reset();
                            //var mConn = GetConnectionString();
                            //SqlConnection conx = new SqlConnection(mConn);
                            //SqlDataAdapter adp = new SqlDataAdapter("Select Master.*,TfatBranch.* from Master,TfatBranch Where TfatBranch.Code='" + mbranchcode + "';", conx);
                            ////SqlConnection conx1 = new SqlConnection(mConn); SqlDataAdapter adp1 = new SqlDataAdapter("SELECT RECORDKEY, ComplaintId, PendingReasonId, Prefix,(select name from repair where code = RepairStatus)as RepairName,RepairStatus, SrNo, VisitComment, VisitDate, VisitEnggId, VisitInTime, VisitOutTime, TOUCHVALUE, ENTEREDBY, LASTUPDATEDATE, ServiceCenterId,(select name from scuserinfo where code = VisitEnggId) as Technician from complaintvisitdetails WHERE ComplaintId IS NOT NULL  and ComplaintId = " + "'" + Model.Srl + "'" + ";", conx1);
                            ////SqlConnection conx2 = new SqlConnection(mConn); SqlDataAdapter adp2 = new SqlDataAdapter("SELECT  RecordKey, Code, FirstName, LastName, Adrl1, Adrl2, Adrl3, Pin, StateId, CityId, MobileNo, Tel1, Email, LandMark, Locality, DocDate  from sccustomermaster where sccustomermaster.Code = " + "'" + Model.CustomerId + "'" + ";", conx2);
                            ////SqlConnection conx3 = new SqlConnection(mConn); SqlDataAdapter adp3 = new SqlDataAdapter("SELECT  RECORDKEY, CustomerId, PartCode, Prefix, ProductId, Qty, Rate, SerialNo, ServiceCenterId, TOUCHVALUE, ENTEREDBY, LASTUPDATEDATE, ComplaintId, VisitId, SrNo, (select name from itemdetail where code = PartCode and grp = '000009' and branch = 'HO0000') as PartName,Qty* Rate as Amount from ComplaintPart  where ComplaintPart.ComplaintId = " + "'" + Model.Srl + "'" + ";", conx3);

                            //adp.Fill(ds, ds.DataTable1.TableName);
                            ////adp1.Fill(ds, ds.ComplaintVisitDetails.TableName);
                            ////adp2.Fill(ds, ds.SCCustomerMaster.TableName);
                            ////adp3.Fill(ds, ds.ComplaintPart.TableName);
                            //reportViewer.LocalReport.ReportPath = Server.MapPath("/Reports/xReport.rdlc");
                            //ReportDataSource rds = new ReportDataSource("DS_xReport", ds.Tables[0]);
                            ////ReportDataSource rds1 = new ReportDataSource("ComplDataSet", ds.Tables[3]);
                            ////ReportDataSource rds2 = new ReportDataSource("CVisitDataSet", ds.Tables[1]);
                            ////ReportDataSource rds3 = new ReportDataSource("CustDataSet", ds.Tables[2]);
                            //reportViewer.LocalReport.DataSources.Clear();
                            //reportViewer.LocalReport.DataSources.Add(rds);
                            ////reportViewer.LocalReport.DataSources.Add(rds1);
                            ////reportViewer.LocalReport.DataSources.Add(rds2);
                            ////reportViewer.LocalReport.DataSources.Add(rds3);
                            //reportViewer.LocalReport.Refresh();
                            //ViewBag.ReportViewer = reportViewer;
                            //return View();
                        }
                        return null;
                    }
                }
                catch (Exception e)
                {
                    Session["ErrorMessage"] = e.Message;
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(new { Message = e.Message.Replace("'", "") });
                    //return Json(new { Message = e.Message, Status = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }



        #endregion

        // GET: Logistics/TransactionChargesSetup
        public ActionResult Index1(TransactionChargesSetupVM mModel)
        {
            //GetAllMenu(Session["ModuleName"].ToString());
            ////AccountingGetAllMenu(Session["ModuleName"].ToString());
            ////VehicleGetAllMenu(Session["ModuleName"].ToString());
            GetAllMenu(Session["ModuleName"].ToString());

            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "CHARGE");
            mdocument = mModel.Document;
            mModel.EqAmtList = GetEqAmtList();
            mModel.EqCostList = GetEqCostList();
            mModel.EqSaleList = GetEqSaleList();
            mModel.EqBroList = GetEqBroList();
            mModel.EqTaxList = GetEqTaxList();
            mModel.Charges_RECORDKEY = mModel.Document;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.Charges.Where(x => (x.RECORDKEY == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    var mCode = ctxTFAT.Master.Where(x => x.Code == mList.Code).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mHSNCode = ctxTFAT.HSNMaster.Where(x => x.Code == mList.HSNCode).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mTaxCode = ctxTFAT.TaxMaster.Where(x => x.Code == mList.TaxCode).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    mModel.Charges_Code = mCode != null ? mCode.Code.ToString() : "";
                    mModel.CodeName = mCode != null ? mCode.Name : "";
                    mModel.Charges_HSNCode = mHSNCode != null ? mHSNCode.Code.ToString() : "";
                    mModel.HSNCodeName = mHSNCode != null ? mHSNCode.Name : "";
                    mModel.Charges_TaxCode = mTaxCode != null ? mTaxCode.Code.ToString() : "";
                    mModel.TaxCodeName = mTaxCode != null ? mTaxCode.Name : "";
                    mModel.Charges_RECORDKEY = mList.RECORDKEY;
                    mModel.Charges_Type = mList.Type;
                    mModel.Charges_Fld = mList.Fld;
                    mModel.Charges_Head = mList.Head;
                    mModel.Charges_AfterTax = mList.AfterTax;
                    mModel.Charges_RoundOff = mList.RoundOff != null ? mList.RoundOff.Value : 0;
                    mModel.Charges_EqAmt = mList.EqAmt;
                    mModel.Charges_EqCost = mList.EqCost;
                    mModel.Charges_EqSale = mList.EqSale;
                    mModel.Charges_EqBro = mList.EqBro;
                    mModel.Charges_EqTax = mList.EqTax;
                    mModel.Charges_Equation = mList.Equation;
                    mModel.Charges_Post = mList.Post;
                    mModel.Charges_PostNo = mList.PostNo != null ? mList.PostNo.Value : 0;
                    mModel.Charges_ToPrint = mList.ToPrint;
                    mModel.Charges_ToHide = mList.DontUse;
                }
            }
            else
            {
                mModel.Charges_AfterTax = false;
                mModel.Charges_Code = "";
                mModel.Charges_CompCode = "";
                mModel.Charges_DontUse = false;
                mModel.Charges_EqAmt = "";
                mModel.Charges_EqBro = "";
                mModel.Charges_EqCost = "";
                mModel.Charges_EqSale = "";
                mModel.Charges_EqTax = "";
                mModel.Charges_Equation = "";
                mModel.Charges_Fld = "";
                mModel.Charges_Head = "";
                mModel.Charges_HSNCode = "";
                mModel.Charges_MainType = "";
                mModel.Charges_Post = false;
                mModel.Charges_PostNo = 0;
                mModel.Charges_RoundOff = 0;
                mModel.Charges_SubType = "";
                mModel.Charges_TaxCode = "";
                mModel.Charges_ToPrint = false;
                mModel.Charges_ToHide = false;
                mModel.Charges_Type = "";
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(TransactionChargesSetupVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteTransactionChargesSetup(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.Charges_RECORDKEY, DateTime.Now, 0, mModel.Charges_RECORDKEY.ToString(), "Delete Transaction Charge SetUP", "CHARGE");
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    Charges mobj = new Charges();
                    bool mAdd = true;
                    if (ctxTFAT.Charges.Where(x => (x.RECORDKEY == mModel.Charges_RECORDKEY)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.Charges.Where(x => (x.RECORDKEY == mModel.Charges_RECORDKEY)).FirstOrDefault();
                        mAdd = false;
                    }

                    if (mAdd == false && mModel.Charges_Type == "LR000")
                    {
                        TfatSearch tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == "InvoicePickUpDocument" && x.ColHead.Trim().ToLower() == mModel.Charges_Fld.Trim().ToLower()).FirstOrDefault();
                        if (tfatSearch != null)
                        {
                            tfatSearch.IsHidden = mModel.Charges_ToHide;
                            ctxTFAT.Entry(tfatSearch).State = EntityState.Modified;
                        }
                    }
                    else if (mAdd == false && mModel.Charges_Type == "FMP00")
                    {
                        TfatSearch tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == "AdvBalPayPickUpDocument" && x.ColHead.Trim().ToLower() == mModel.Charges_Fld.Trim().ToLower()).FirstOrDefault();
                        if (tfatSearch != null)
                        {
                            tfatSearch.IsHidden = mModel.Charges_ToHide;
                            ctxTFAT.Entry(tfatSearch).State = EntityState.Modified;
                        }
                    }



                    mobj.RECORDKEY = mModel.Charges_RECORDKEY;
                    mobj.Type = mModel.Charges_Type;
                    mobj.Fld = mModel.Charges_Fld;
                    mobj.Head = mModel.Charges_Head;
                    mobj.AfterTax = mModel.Charges_AfterTax;
                    mobj.RoundOff = mModel.Charges_RoundOff;
                    mobj.EqAmt = mModel.Charges_EqAmt;
                    mobj.EqCost = mModel.Charges_EqCost;
                    mobj.EqSale = mModel.Charges_EqSale;
                    mobj.EqBro = mModel.Charges_EqBro;
                    mobj.EqTax = mModel.Charges_EqTax;
                    mobj.Equation = mModel.Charges_Equation;
                    mobj.Post = mModel.Charges_Post;
                    mobj.Code = mModel.Charges_Code;
                    mobj.HSNCode = mModel.Charges_HSNCode;
                    mobj.PostNo = mModel.Charges_PostNo;
                    mobj.TaxCode = mModel.Charges_TaxCode;
                    mobj.ToPrint = mModel.Charges_ToPrint;
                    mobj.DontUse = mModel.Charges_ToHide;
                    // iX9: default values for the fields not used @Form
                    //mobj.CompCode = "";
                    //mobj.DontUse = false;
                    //mobj.MainType = "";
                    //mobj.SubType = "";
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mAUTHORISE;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mAdd == true)
                    {
                        ctxTFAT.Charges.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mnewrecordkey, DateTime.Now, 0, mnewrecordkey.ToString(), "Save Transaction Charge SetUP", "CHARGE");

                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "TransactionChargesSetup" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "TransactionChargesSetup" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "TransactionChargesSetup" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "TransactionChargesSetup" }, JsonRequestBehavior.AllowGet);
        }

        public int GetNextCode()
        {
            var nextcode = (from x in ctxTFAT.Charges select (int?)x.RECORDKEY).Max() ?? 0;
            return (++nextcode);
        }

        public ActionResult DeleteTransactionChargesSetup(TransactionChargesSetupVM mModel)
        {
            var mList = ctxTFAT.Charges.Where(x => (x.RECORDKEY == mModel.Charges_RECORDKEY)).FirstOrDefault();
            ctxTFAT.Charges.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}