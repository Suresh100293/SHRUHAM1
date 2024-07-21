using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
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
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class PurchaseReportController : BaseController
    {
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;
        private new string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private new string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        public new string muserid = System.Web.HttpContext.Current.Session["UserId"].ToString();
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        private static string mbasegr = "";

        #region Functions

        private List<SelectListItem> PopulateReportType()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem
            {
                Text = "Purchase Report",
                Value = "PurchaseReport"
            });
            items.Add(new SelectListItem
            {
                Text = "UnPaid Purchase Bill",
                Value = "UnPaidPurchase"
            });

            return items;
        }

        private List<SelectListItem> PopulateBillType()
        {
            List<SelectListItem> maclist = new List<SelectListItem>();

            maclist.Add(new SelectListItem { Value = "PUR00", Text = "Credit Purchase" });
            maclist.Add(new SelectListItem { Value = "CPH00", Text = "Cash Purchase" });
            maclist.Add(new SelectListItem { Value = "", Text = "ALL" });

            return maclist;
        }

        private List<SelectListItem> PopulateBranchesOnly()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM TfatBranch where Category<>'Area' and Code<>'G00000' and Grp <>'G00000'  order by Recordkey ";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            items.Add(new SelectListItem
                            {
                                Text = sdr["Name"].ToString(),
                                Value = sdr["Code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return items;
        }

        private List<SelectListItem> PopulateCustomer()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM Master where BaseGr='S'  order by Name ";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            items.Add(new SelectListItem
                            {
                                Text = sdr["Name"].ToString(),
                                Value = sdr["Code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return items;
        }

        private List<SelectListItem> PopulateMaster()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM Master where BaseGr='S' or BaseGr='B' or BaseGr='C'  order by Name ";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            items.Add(new SelectListItem
                            {
                                Text = sdr["Name"].ToString(),
                                Value = sdr["Code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return items;
        }

        public ActionResult PopulateSaveReports(string ViewDataId)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT ReportNameAlias, ReportName FROM ReportParameters where Reports='" + ViewDataId + "' order by ReportNameAlias ";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            items.Add(new SelectListItem
                            {
                                Text = sdr["ReportNameAlias"].ToString(),
                                Value = sdr["ReportName"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        #endregion

        // GET: Accounts/PurchaseReport
        public ActionResult Index(PurchaseReportVM Model)
        {
            UpdateAuditTrail(mbranchcode, "Report", Model.Header, "", DateTime.Now, 0, "", "", "NA");
            if (Model.Document == "" || Model.Document == null)
            {
                GetAllMenu(Session["ModuleName"].ToString());
                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
                //System.Web.HttpContext.Current.Session["LastDate"].ToString();
            }
            if (Model.FromDate == null)
            {
                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            }
            ViewBag.ViewDataId = Model.ViewDataId;
            msubcodeof = Model.ViewDataId;
            //Model.AccountName = NameofAccount(Model.Document);
            Model.ViewCode = GetDefaultCode(Model.ViewDataId);
            mbasegr = Model.MainType;
            ViewBag.Header = Model.Header;
            ViewBag.Controller = Model.Controller;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;

            Model.Branch = mbranchcode;
            Model.BranchesOnly = PopulateBranchesOnly();
            Model.Customers = PopulateCustomer();
            Model.Masters = PopulateMaster();
            Model.ReportsType = PopulateReportType();
            Model.BillTypes = PopulateBillType();
            Model.ReportTypeL = Model.ViewDataId;
            Model.BillType = "PUR00";

            string[] Colmn = new string[] { "PaymenyKey", "ExpenseKey", "PurchaseKey" };
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = false);
            tfatSearch.Where(x => Colmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            var HideHeaderList = ctxTFAT.Charges.Where(x => x.Type == "BPM00").OrderBy(x => x.Fld).Select(x => x.DontUse).ToList();
            string[] ChargesColmn = new string[] { "F001", "F002", "F003", "F004", "F005", "F006", "F007", "F008", "F009", "F010" };
            int i = 0;
            foreach (var item in HideHeaderList)
            {
                if (item)
                {
                    tfatSearch.Where(x => x.ColHead== ChargesColmn[i]).ToList().ForEach(x => x.IsHidden = true);
                }
                ++i;
            }
            
            ctxTFAT.SaveChanges();

            Model.AllHeaderList = ctxTFAT.Charges.Where(x => x.Type == "BPM00").OrderBy(x=>x.Fld).Select(x => x.Head).ToList();


            return View(Model);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(PurchaseReportVM Model)
        {
            try
            {
                TempData["PurchaseBranch"] = Model.Branch;
                TempData["PurchaseCustomer"] = Model.Customer;
                TempData["PurchaseMaster"] = Model.Master;
                TempData["PurchaseBillType"] = Model.BillType;

                TempData["PurchasePaymentPaidDetails"] = Model.PaymentPaidDetails;

                var ReportHeader = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).FirstOrDefault();
                if (Model.ViewDataId == "PurchaseReport")
                {
                    ReportHeader.pMerge = "26^17^1";
                    ReportHeader.pToMerge = "26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41^17,18,19,20,21,22,23,24,25^1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16";
                }
                ctxTFAT.SaveChanges();
                Model.GetReportParameter = true;
                return GetGridDataColumns(Model.ViewDataId, "X", "", GetMonthString());
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Status = "Error",
                    Message = "Error! While Generating Report..\n" + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetGridData(PurchaseReportVM Model)
        {

            var BillNo = Model.BillNo;
            var BillType = Convert.ToString(TempData.Peek("PurchaseBillType"));
            var Branch = Convert.ToString(TempData.Peek("PurchaseBranch"));
            var Vendor = Convert.ToString(TempData.Peek("PurchaseCustomer"));
            var Master = Convert.ToString(TempData.Peek("PurchaseMaster"));

            mpara = "";
            if (!String.IsNullOrEmpty(BillNo))
            {
                mpara += "para01" + "^L.srl='" + Model.BillNo + "'~";
            }
            else
            {
                mpara += "para01^1=1~";
            }
            if (!String.IsNullOrEmpty(BillType))
            {
                mpara += "para02" + "^P.Type='" + BillType + "'~";
            }
            else
            {
                mpara += "para02^1=1~";
            }

            if (!String.IsNullOrEmpty(Branch))
            {
                mpara += "para03" + "^charindex(P.Branch,'" + Branch + "')<>0~";
            }
            else
            {
                mpara += "para03^1=1~";
            }

            if (!String.IsNullOrEmpty(Master))
            {
                mpara += "para04" + "^charindex(P.Code,'" + Master + "')<>0~";
            }
            else
            {
                mpara += "para04^1=1~";
            }

            if (!String.IsNullOrEmpty(Vendor))
            {
                mpara += "para05" + "^charindex(P.VendorAC,'" + Vendor + "')<>0~";
            }
            else
            {
                mpara += "para05^1=1~";
            }



            GridOption gridOption = new GridOption();
            gridOption.ViewDataId = Model.ViewDataId;
            gridOption.FromDate = Model.FromDate;
            gridOption.ToDate = Model.ToDate;
            gridOption.mWhat = Model.mWhat;
            gridOption.Date = Model.FromDate + ":" + Model.ToDate;
            int x = (int)System.Web.HttpContext.Current.Session["GridRows"];
            gridOption.rows = Model.rows;
            gridOption.page = Model.page;
            gridOption.searchField = Model.searchField;
            gridOption.searchOper = Model.searchOper;
            gridOption.searchString = Model.searchString;
            gridOption.sidx = Model.sidx;
            gridOption.sord = Model.sord;
            return GetGridReport1(gridOption, "X", (mpara != "" ? "~" + mpara : ""), false, 0);
        }

        public ActionResult GetGridReport1(GridOption Model, string mReportType = "R", string mParaString = "", bool mRunning = false, decimal mopening = 0, string mFilter = "", string mpapersize = "A4", string[] mparameters = null)
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
            Model.searchField = Model.searchField == null || Model.searchField == "null" ? "" : Model.searchField;
            Model.searchString = Model.searchString ?? "";
            string mWhat = Model.mWhat ?? "";
            int startIndex = mWhat == "" ? (Model.page - 1) * Model.rows + 1 : -1;
            int endIndex = mWhat == "" ? (Model.page * Model.rows) : -1;

            SqlDataAdapter da = new SqlDataAdapter();
            using (DataTable dt = new DataTable())
            {
                SqlCommand cmd = new SqlCommand();

                if (Model.searchField != "" && Model.searchString != "" && mFilter == "")
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
                            mFilter = Model.searchField + " in ( " + Model.searchString + ")";
                            break;
                        case "nc":
                            mFilter = Model.searchField + " Not like '%" + Model.searchString + "%'";
                            break;
                        case "ni":
                            mFilter = Model.searchField + " Not like '%" + Model.searchString + "%'";
                            break;
                        case "Active":
                            mFilter = Model.searchField + Model.searchString;
                            break;
                        case "Running":
                            mFilter = Model.searchField + Model.searchString;
                            break;
                    }
                }


                try
                {
                    DataTable Newdt = new DataTable();
                    SqlConnection con = new SqlConnection(connstring);
                    cmd = new SqlCommand("dbo.ExecuteReport", con)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.Add("@mFormatCode", SqlDbType.VarChar).Value = Model.ViewDataId;
                    cmd.Parameters.Add("@mAlias", SqlDbType.VarChar).Value = (Model.searchtype ?? "").StartsWith("^S") ? "^" + Model.searchField : ""; // since currently not used, we use it for summarised report flag
                    cmd.Parameters.Add("@mCurrDec", SqlDbType.TinyInt).Value = 2;
                    cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
                    cmd.Parameters.Add("@mUserID", SqlDbType.VarChar).Value = muserid;
                    //if (mReportType == "M")
                    //{
                    //    Model.FromDate = Convert.ToDateTime(System.Web.HttpContext.Current.Session["StartDate"]).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    //    Model.ToDate = Convert.ToDateTime(System.Web.HttpContext.Current.Session["LastDate"]).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    //}
                    //else
                    {
                        if (Model.Date != null && Model.Date != "undefined:undefined")
                        {
                            var date = Model.Date.Replace("-", "/").Split(':');
                            if (date[0] != "undefined")
                            {
                                Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                                Model.FromDate = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

                            }
                            if (date[1] != "undefined")
                            {
                                Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
                                Model.ToDate = (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            }
                        }
                        else
                        {
                            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
                            Model.FromDate = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            Model.ToDate = (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                        }
                    }
                    cmd.Parameters.Add("@mReportStartDate", SqlDbType.VarChar).Value = Model.FromDate;
                    cmd.Parameters.Add("@mReportEndDate", SqlDbType.VarChar).Value = Model.ToDate;
                    cmd.Parameters.Add("@mIsRunBalance", SqlDbType.Bit).Value = false;// mRunning;
                    //cmd.Parameters.Add("@mOrderBy", SqlDbType.VarChar).Value = Model.sidx != null ? (Model.sidx.Replace(",", "") + ' ' + (Model.sidx.Contains("asc") || Model.sidx.Contains("desc") ? "" : Model.sord)) : "";
                    string mstrx = (Model.searchtype ?? "").StartsWith("^S") ? Model.searchField : (Model.sidx != null ? (Model.sidx + ' ' + (Model.sidx.Contains("asc") || Model.sidx.Contains("desc") ? "" : Model.sord)) : "");
                    mstrx = CutRightString(mstrx.Trim(), 1, ",");
                    cmd.Parameters.Add("@mOrderBy", SqlDbType.VarChar).Value = mstrx;// Model.sidx != null ? (Model.sidx + ' ' + (Model.sidx.Contains("asc") || Model.sidx.Contains("desc") ? "" : Model.sord)) : "";
                    cmd.Parameters.Add("@mStartIndex", SqlDbType.Int).Value = startIndex;
                    cmd.Parameters.Add("@mEndIndex", SqlDbType.Int).Value = endIndex;
                    cmd.Parameters.Add("@mRunBalance", SqlDbType.Decimal).Value = mRunning == true ? Model.Opening : 0;
                    cmd.Parameters.Add("@mInsertIntoTable", SqlDbType.VarChar).Value = "";// mRunning == true ? Model.ViewDataId : "";
                    cmd.Parameters.Add("@mPara", SqlDbType.VarChar).Value = mParaString;
                    cmd.Parameters.Add("@mFilter", SqlDbType.VarChar).Value = mFilter;
                    cmd.Parameters.Add("@mDocString", SqlDbType.VarChar).Value = mDocString;
                    // for output
                    cmd.Parameters.Add("@mSumString", SqlDbType.VarChar, 5000).Value = "";
                    cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
                    cmd.Parameters["@mSumString"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;
                    con.Open();
                    da.SelectCommand = cmd;
                    da.Fill(dt);
                    string mSumString = (string)(cmd.Parameters["@mSumString"].Value ?? "");
                    string mReturnString = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
                    con.Close();
                    con.Dispose();
                    // physical merge rows
                    var mvar = ctxTFAT.ReportHeader.Where(z => z.Code == Model.ViewDataId).Select(x => new { x.pMerge, x.pToMerge, x.pBlank }).FirstOrDefault();
                    string mpmerge = "";
                    string mptomerge = "";


                    if (mvar != null)
                    {


                        mpmerge = (mvar.pMerge ?? "").Trim();
                        mptomerge = (mvar.pToMerge ?? "").Trim();
                    }
                    if (mpmerge != "")
                    {
                        string[] columnNames = dt.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
                        DataTable FT = dt.Copy();
                        var MergingSingleColumn = mvar.pMerge.Split('^');
                        var MergingMultiplColumnList = mvar.pToMerge.Split('^');
                        for (int i = 0; i < MergingSingleColumn.Length; i++)
                        {
                            mpmerge = MergingSingleColumn[i];
                            mptomerge = MergingMultiplColumnList[i];

                            //Sorting the Table
                            var String = columnNames[Convert.ToInt32(mpmerge)-1];
                            DataView dv = FT.DefaultView;
                            dv.Sort = String; 
                            Newdt = dv.ToTable();

                            if (mpmerge.Contains("+"))
                            {
                                var marr = mpmerge.Replace("+", "");
                                var marr1 = mptomerge.Split('-');
                                decimal NetProfit = 0;

                                for (int l = 0; l < Newdt.Rows.Count - 1; l++)
                                {
                                    NetProfit = 0;
                                    foreach (var item in marr1[0].Split(','))
                                    {
                                        int Col = Convert.ToInt32(item) - 1;
                                        NetProfit += Convert.ToDecimal(Newdt.Rows[l][Col]);
                                    }
                                    foreach (var item in marr1[1].Split(','))
                                    {
                                        int Col = Convert.ToInt32(item) - 1;
                                        NetProfit -= Convert.ToDecimal(Newdt.Rows[l][Col]);
                                    }
                                    int Col1 = Convert.ToInt32(marr) - 1;
                                    Newdt.Rows[l][Col1] = NetProfit;
                                }

                            }
                            else
                            {
                                var marr = mpmerge.Split(',');
                                if (mptomerge.EndsWith(",") == false)
                                {
                                    mptomerge += ",";
                                }

                                if (mptomerge.StartsWith(",") == false)
                                {
                                    mptomerge = "," + mptomerge;
                                }
                                string mstr = "";
                                for (int n = 0; n <= Newdt.Rows.Count - 1; n++)
                                {
                                    string mstr2 = "";
                                    for (int m = 0; m <= marr.Count() - 1; m++)
                                    {
                                        if (marr[m] != "")
                                        {
                                            mstr2 += Newdt.Rows[n][Convert.ToInt32(marr[m]) - 1];
                                        }
                                    }

                                    if (mstr == mstr2)
                                    {
                                        for (int z = 0; z <= Newdt.Columns.Count - 1; z++)
                                        {
                                            if (mptomerge.Contains("," + (z + 1).ToString() + ","))
                                            {
                                                if (Newdt.Columns[z].DataType == System.Type.GetType("System.Byte") || Newdt.Columns[z].DataType == System.Type.GetType("System.Decimal") || Newdt.Columns[z].DataType == System.Type.GetType("System.Double") || Newdt.Columns[z].DataType == System.Type.GetType("System.Int16") || Newdt.Columns[z].DataType == System.Type.GetType("System.Int32") || Newdt.Columns[z].DataType == System.Type.GetType("System.Int64") || Newdt.Columns[z].DataType == System.Type.GetType("System.Single"))
                                                {
                                                    Newdt.Rows[n][z] = 0;
                                                }
                                                else
                                                {
                                                    Newdt.Rows[n][z] = "";
                                                }
                                            }
                                        }
                                    }
                                    mstr = mstr2;
                                }
                            }

                            FT = Newdt.Copy();
                        }

                        Newdt = FT.Copy();

                    }
                    // merge routine over

                    if (mRunning == true)
                    {
                        int mbalcol = -1;
                        int mruncol = -1;
                        int mCodecol = -1;
                        int i;
                        string Code = "NA", PrevCode = "NA";
                        for (i = 0; i < Newdt.Columns.Count; i++)
                        {
                            string mcolname = Newdt.Columns[i].ColumnName.Trim().ToLower();
                            if (mcolname == "balancefield")
                            {
                                mbalcol = i;
                            }
                            if (mcolname == "runningbalance" || mcolname == "balance")
                            {
                                mruncol = i;
                            }
                            if (mcolname == "Code")
                            {
                                mCodecol = i;
                            }
                        }
                        if (mbalcol != -1 && mruncol != -1)
                        {
                            decimal mbal = mopening;
                            foreach (DataRow dr in Newdt.Rows)
                            {
                                if (mCodecol != -1)
                                {
                                    if (Code == "NA" && PrevCode == "NA")
                                    {
                                        Code = (string)dr[mCodecol];
                                        PrevCode = (string)dr[mCodecol];
                                    }
                                    else
                                    {
                                        PrevCode = (string)dr[mCodecol];
                                    }

                                    if (Code != PrevCode)
                                    {
                                        mbal = 0;
                                    }
                                }
                                mbal += (decimal)dr[mbalcol];
                                dr[mruncol] = mbal;
                            }
                        }
                    }

                    //string mSumJson = "";
                    StringBuilder jsonBuilder = new StringBuilder();
                    if ((mReportType == "R" || mReportType == "T") && Newdt.Rows.Count > 0)
                    {
                        try
                        {
                            DataTable msumdt = GetDataTable(@mSumString.Replace("[[", "[").Replace("]]", "]"), connstring);
                            //float[] marr = new float[dt.Columns.Count];
                            Newdt.Rows.Add();
                            if (msumdt.Rows.Count > 0)
                            {
                                int x = Newdt.Rows.Count;
                                for (int m = 0; m <= msumdt.Columns.Count - 1; m++)
                                {
                                    if (msumdt.Rows[0][m].ToString() == "")
                                    {
                                        Newdt.Rows[x - 1][m] = "";
                                    }
                                    else
                                    {
                                        try { Newdt.Rows[x - 1][m] = Convert.ToDecimal(msumdt.Rows[0][m]); }
                                        catch (Exception ex)
                                        {

                                        }
                                    }
                                }
                            }
                            msumdt.Dispose();
                        }
                        catch (Exception exx)
                        {
                        }
                    }
                    else
                    {
                        //jsonBuilder.Append("");
                    }

                    if (mReportType != "R" && mReportType != "X" && mWhat != "PDF" && mWhat != "PDL" && mWhat != "XLS")
                    {
                        Newdt.Columns.Add("XYZ", typeof(string)).SetOrdinal(0);
                        Newdt.Columns.Add("ABC", typeof(string)).SetOrdinal(1);
                        Newdt.Columns.Add("FGH", typeof(string)).SetOrdinal(2);
                        Newdt.Columns.Add("PTG", typeof(string)).SetOrdinal(3);
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
                        //if (mReportType == "M")
                        //{
                        //    cmd2.Parameters.Add("@mReportStartDate", SqlDbType.VarChar).Value = "2018-01-01";
                        //    cmd2.Parameters.Add("@mReportEndDate", SqlDbType.VarChar).Value = "2019-01-01";
                        //}
                        //else
                        //{
                        cmd2.Parameters.Add("@mReportStartDate", SqlDbType.VarChar).Value = Model.FromDate;
                        cmd2.Parameters.Add("@mReportEndDate", SqlDbType.VarChar).Value = Model.ToDate;
                        //}
                        cmd2.Parameters.Add("@mPara", SqlDbType.VarChar).Value = mParaString;
                        cmd2.Parameters.Add("@mFilter", SqlDbType.VarChar).Value = mFilter;
                        cmd2.Parameters.Add("@mDocString", SqlDbType.VarChar).Value = mDocString;
                        // for output
                        cmd2.Parameters.Add("@mRowCount", SqlDbType.Int).Value = 0;
                        cmd2.Parameters["@mRowCount"].Direction = ParameterDirection.Output;
                        con2.Open();
                        da2.SelectCommand = cmd2;
                        int mxRowCount = 0;
                        try { da2.Fill(dt2); mxRowCount = (int)cmd2.Parameters["@mRowCount"].Value; } catch (Exception e) { mxRowCount = Model.rows; }
                        cmd2.Dispose();
                        dt2.Dispose();
                        da2.Dispose();
                        con2.Close();
                        con2.Dispose();
                        return Content(JQGridHelper.JsonForJqgrid(Newdt, Model.rows, mxRowCount, Model.page, jsonBuilder.ToString()), "application/json");
                    }
                    else     // XLS or PDF
                    {
                        if (Model.mWhat == "XLS")
                        {
                            string attachment = "attachment; filename=" + Model.ViewDataId + @".xls";
                            Response.ClearContent();
                            Response.AddHeader("content-disposition", attachment);
                            Response.ContentType = "application/vnd.ms-excel";
                            var mWidths = (from xx in ctxTFAT.TfatSearch
                                           where xx.Code == Model.ViewDataId && xx.CalculatedCol != true
                                           orderby xx.Sno
                                           select new { xx.ColHead, ColWidth = (float)(xx.IsHidden == true ? 0 : xx.ColWidth) }).ToList();
                            float[] headerx = mWidths.Select(z => z.ColWidth).ToArray();
                            string tab = "";
                            string mHead = "";
                            DateTime mDate = Convert.ToDateTime(Model.ToDate);
                            int x = 0;
                            foreach (DataColumn dc in Newdt.Columns)
                            {
                                if (dc.ColumnName != "_Style" && headerx[x] > 5)
                                {
                                    mHead = mWidths[x].ColHead.Trim().Replace("##", "");
                                    if (mHead == "") mHead = dc.ColumnName;
                                    if (mHead.Contains("%"))
                                    {
                                        mHead = ProcessReportHeader(mHead, mDate);
                                    }
                                    Response.Write(tab + mHead);//dc.ColumnName
                                    tab = "\t";
                                }
                                ++x;
                            }
                            Response.Write("\n");
                            x = 0;
                            foreach (DataRow dr in Newdt.Rows)
                            {
                                tab = "";
                                x = 0;
                                for (int i = 0; i < Newdt.Columns.Count; i++)
                                {
                                    if (Newdt.Columns[i].ColumnName != "_Style" && headerx[x] > 5)
                                    {
                                        Response.Write(tab + dr[i].ToString().Replace("\"", "").Replace(@"\", "").Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace("\b", ""));
                                        tab = "\t";
                                    }
                                    ++x;
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
                            if (mparameters != null)
                            {
                                for (int xx = 0; xx <= 23; xx++)
                                {
                                    string mfld = "%para" + (xx + 1).ToString().PadLeft(2, '0');
                                    if (Model.AccountDescription.Contains(mfld))
                                    {
                                        Model.AccountDescription = Model.AccountDescription.Replace(mfld, mparameters[xx]);
                                    }
                                }
                            }
                            CreatePDF(Model, Newdt, Model.AccountDescription, Model.mWhat == "PDL" ? "Landscape" : "Portrait", mpapersize);
                        }
                        else if (Model.mWhat == "SRS")
                        {

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
                finally
                {
                    cmd.Dispose();
                    da.Dispose();
                }
            }
        }


        #region Set Column Show Hide

        public void SetColumnsReset(PurchaseReportVM Model)
        {
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();

            if (Model.ViewDataId == "BillReport")
            {
                tfatSearch.ForEach(x => x.IsHidden = false);
                string[] PaymentReceivedDetailsDetailsColumnsOnly = new string[] { "ReceivedBranch", "ReceivedDate", "ReceivedFrom", "Amount", "TdsAmout", "TotalReceivedAmount" };
                if (Model.PaymentPaidDetails == true)
                {
                    tfatSearch.Where(x => PaymentReceivedDetailsDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                }
                else
                {
                    tfatSearch.Where(x => PaymentReceivedDetailsDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                }
            }
            else if (Model.ViewDataId == "BillReportWithLR")
            {
                tfatSearch.ForEach(x => x.IsHidden = false);

                string[] ChrColmn = new string[] { "Freight Charges", "LR Charges", "Hamali Charges", "Collection Charges", "Door Delivery Charges", "Varai Charges", "Loading Charges", "Unloading Charges", "Detention Charges", "Union Charges", "Demurrage Charges", "Other Charges", "Charges-13", "Charges-14", "Charges-15", "Charges-16", "Charges-17", "Charges-18", "Charges-19", "Charges-20", "Charges-21", "Charges-22", "Charges-23", "Charges-24", "Charges-25" };
                var ChargesList = tfatSearch.Where(x => ChrColmn.Contains(x.ColHead)).ToList();
                tfatSearch.Where(x => ChrColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);

                #region Only This Chrges show As Per DocType Rule
                var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "LR000" && x.DontUse == false).ToList();
                List<string> LRcharge = Lrcharges.Select(x => x.Head).ToList();
                ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                #endregion
            }
            ctxTFAT.SaveChanges();
        }

        #endregion

        [HttpPost]
        public ActionResult ParameterReset(PurchaseReportVM Model)
        {
            Model.BranchesOnly = PopulateBranchesOnly();
            Model.Customers = PopulateCustomer();
            Model.Masters = PopulateMaster();
            Model.ReportsType = PopulateReportType();
            Model.BillTypes = PopulateBillType();
            Model.Branch = mbranchcode;
            Model.BillType = "PUR00";

            string[] Colmn = new string[] { "PaymenyKey", "ExpenseKey", "PurchaseKey" };
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = false);
            tfatSearch.Where(x => Colmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            var HideHeaderList = ctxTFAT.Charges.Where(x => x.Type == "BPM00").OrderBy(x => x.Fld).Select(x => x.DontUse).ToList();
            string[] ChargesColmn = new string[] { "F001", "F002", "F003", "F004", "F005", "F006", "F007", "F008", "F009", "F010" };
            int i = 0;
            foreach (var item in HideHeaderList)
            {
                if (item)
                {
                    tfatSearch.Where(x => x.ColHead == ChargesColmn[i]).ToList().ForEach(x => x.IsHidden = true);
                }
                ++i;
            }

            ctxTFAT.SaveChanges();

            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = DateTime.Today.ToShortDateString();

            var html = ViewHelper.RenderPartialView(this, "_OptionsView", Model);
            var jsonResult = Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        [HttpPost]
        public ActionResult GetParameterAuto(PurchaseReportVM Model)
        {
            var MainReportName = Model.ViewDataId;

            Model.BranchesOnly = PopulateBranchesOnly();
            Model.Customers = PopulateCustomer();
            Model.Masters = PopulateMaster();
            Model.ReportsType = PopulateReportType();
            Model.BillTypes = PopulateBillType();

            Model.ReportTypeL = ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).Select(x => x.ReportName).FirstOrDefault();
            var Tfatsearch = ctxTFAT.TfatSearch.Where(x => x.Code.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower()).ToList();
            Tfatsearch.ForEach(x => x.IsHidden = false);
            //Model.SaveReportList = PopulateSaveReports(MainReportName);
            ReportParameters mobj = new ReportParameters();
            if (ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).FirstOrDefault() != null)
            {
                mobj = ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).FirstOrDefault();

                Model.BillNo = mobj.DocNo;
                Model.FromDate = mobj.StartDate.Value.ToShortDateString();
                Model.ToDate = mobj.EndDate.Value.ToShortDateString();
                Model.HideColumnList = mobj.HideColumnList;

                Model.BillType = mobj.Para1 == null ? "SLR00" : mobj.Para1;

                Model.Branch = mobj.Para2 == null ? "" : mobj.Para2.Replace("'", "");
                Model.Customer = mobj.Para4 == null ? "" : mobj.Para4.Replace("'", "");
                Model.Master = mobj.Para5 == null ? "" : mobj.Para5.Replace("'", "");

                Model.PaymentPaidDetails = mobj.Para6 == "T" ? true : false;

            }
            if (!String.IsNullOrEmpty(Model.HideColumnList))
            {
                var HiddenColumns = Model.HideColumnList.Split(',').ToList();
                Tfatsearch.Where(x => HiddenColumns.Contains(x.Sno.ToString())).ToList().ForEach(x => x.IsHidden = true);
                ctxTFAT.SaveChanges();
            }


            var html = ViewHelper.RenderPartialView(this, "_OptionsView", Model);
            var jsonResult = Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        [HttpPost]
        public ActionResult SaveParameter(PurchaseReportVM Model)
        {
            var MainReportName = Model.ViewDataId;
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (Model.Mode == "Delete")
                    {
                        var MSG = DeleteParamete(Model);
                        transaction.Commit();
                        transaction.Dispose();
                        return MSG;
                    }
                    ReportParameters mobj = new ReportParameters();
                    bool mAdd = true;
                    if (ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).FirstOrDefault();
                        mAdd = false;
                    }

                    var HiddenColumn = "";
                    var Tfatsearch = ctxTFAT.TfatSearch.Where(x => x.Code.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.IsHidden == true).Select(x => x.Sno.ToString()).ToList();
                    foreach (var item in Tfatsearch)
                    {
                        HiddenColumn += item + ",";
                    }
                    if (!String.IsNullOrEmpty(HiddenColumn))
                    {
                        HiddenColumn = HiddenColumn.Substring(0, HiddenColumn.Length - 1);
                    }


                    mobj.DocNo = Model.BillNo;
                    mobj.StartDate = ConvertDDMMYYTOYYMMDD(Model.FromDate);
                    mobj.EndDate = ConvertDDMMYYTOYYMMDD(Model.ToDate);
                    mobj.HideColumnList = HiddenColumn;

                    mobj.Para1 = Model.BillType;

                    mobj.Para2 = Model.Branch;
                    mobj.Para4 = Model.Customer;
                    mobj.Para5 = Model.Master;

                    mobj.Para6 = Model.PaymentPaidDetails == true ? "T" : "F";

                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                    if (mAdd == true)
                    {
                        mobj.ReportNameAlias = Model.ReportName;
                        mobj.ReportName = Model.ReportTypeL;
                        mobj.Reports = MainReportName;
                        ctxTFAT.ReportParameters.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = Convert.ToInt32(mobj.RECORDKEY);
                    string mNewCode = "";
                    transaction.Commit();
                    transaction.Dispose();

                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteParamete(PurchaseReportVM Model)
        {
            var mList = ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == Model.ViewDataId.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).FirstOrDefault();
            ctxTFAT.ReportParameters.Remove(mList);
            ctxTFAT.SaveChanges();

            return Json(new
            {
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }

    }
}