using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
using System.Net;
using System.Data;
using Common;
using System.Data.SqlClient;
using System.Text;
using System.Globalization;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WebForms;
using System.Net.Mail;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class MasterGriddController : BaseController
    {
        // GET: Logistics/MasterGridd

        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
        // GET: Logistics/MasterGrid

        private List<SelectListItem> PopulateBillParty()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "select C.Code as Code,C.Name as Name from CustomerMaster C where (C.Code in (select CA.Code from caddress CA where len(CA.Mobile) between 10 and 13)) And C.Hide='false' order by C.Recordkey ";
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
        private List<SelectListItem> PopulateUsers()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "select Code,Name from TfatPass where (len(Mobile) between 10 and 13) and Hide='false' Order By Name ";
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
        public JsonResult GetTemplate(string term, string Type)//Unit
        {
            //Descr
            var list = ctxTFAT.MsgTemplate.Where(x => x.Code == "&&").ToList().Distinct();
            if (Type == "LR000")
            {
                list = ctxTFAT.MsgTemplate.Where(x => x.Code == "Laqshya LR Book").ToList().Distinct();

                if (!(String.IsNullOrEmpty(term)))
                {
                    list = ctxTFAT.MsgTemplate.Where(x => x.Code == "Laqshya LR Book" && x.Subject.ToLower().Contains(term.ToLower())).ToList();
                }
            }

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Subject
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);


        }

        public ActionResult Index(GridOption Model)
        {
            Session["TempAttach"] = null;
            Model.Users = PopulateUsers();
            Model.Customers = PopulateBillParty();


            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());
            Model.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
            Model.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
            //UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "NA");

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
            TfatMenu tfatMenu = ctxTFAT.TfatMenu.Where(z => z.OptionCode == moptioncode && z.ModuleName == mmodule).FirstOrDefault();
            if (muserid.ToUpper() == "SUPER")
            {
                Model.xAdd = true;
                Model.xDelete = true;
                Model.xEdit = true;
                Model.xPrint = true;
                Model.xView = true;
            }
            else
            {
                var result = ctxTFAT.UserRights.Where(z => z.MenuID == tfatMenu.ID && z.ModuleName == mmodule && z.Code == muserid).FirstOrDefault();

                if (result != null)
                {
                    Model.xAdd = result.xAdd;
                    Model.xDelete = result.xDelete;
                    Model.xEdit = result.xEdit;
                    Model.xPrint = result.xPrint;
                    Model.xView = result.xCess;
                }
            }


            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

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

        public ActionResult GetFormats1()
        {
            var result = ctxTFAT.ReportHeader.Where(x => x.SubCodeOf == "LorryDraft" && !x.Code.EndsWith(".bak")).Select(m => new
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
                mopt = (result.AllowEdit == false ? "X" : "E") + (result.AllowDelete == false ? "X" : "D") + "XX";
            }
            //return GetGridDataColumns(id, "L", mopt);
            return GetGridDataColumns(id, "L", "XXXX");
        }

        //[HttpPost]
        public ActionResult GetMasterGridData(GridOption Model)
        {
            if (Model.IsFormatSelected == true)
            {
                int noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=0 Where SubCodeOf='" + msubcodeof + "'");
                noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=-1 Where Code='" + Model.ViewDataId + "'");
            }

            if (string.IsNullOrEmpty(Model.searchField))
            {
                if (!String.IsNullOrEmpty(Model.searchString) && Model.searchString != "undefined")
                {
                    Model.searchOper = "cn";
                    if (Model.ViewDataId == "LREntry")
                    {
                        Model.searchField = " LrNo ";
                    }
                    else if (Model.ViewDataId == "LCMaster")
                    {
                        Model.searchField = " Lcno ";
                    }
                    else if (Model.ViewDataId == "FMMaster")
                    {
                        Model.searchField = " FmNo ";
                    }
                }
            }



            if (Model.ViewDataId == "FMMaster")
            {
                var ExcludeFM = false;
                var Setup = ctxTFAT.FMSetup.FirstOrDefault();
                if (Setup != null)
                {
                    ExcludeFM = Setup.ExcludeHire;
                }
                if (ExcludeFM)
                {
                    mpara = "para01" + "^" + "F.VehicleStatus in ('100000','100002')" + "~";
                }
                else
                {
                    mpara = "para01" + "^" + "F.VehicleStatus in ('100000','100002','100001')" + "~";
                }

            }



            return GetGridReport(Model, "M", "MainType^" + Model.MainType + (mpara != "" ? "~" + mpara : ""), false, 0);
        }

        public ActionResult GetGridReports(GridOption Model, string mReportType = "R", string mParaString = "", bool mRunning = false, decimal mopening = 0, string mFilter = "", string mpapersize = "A4", string[] mparameters = null)
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

                try
                {
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
                    if (mReportType == "M")
                    {
                        Model.FromDate = Convert.ToDateTime(System.Web.HttpContext.Current.Session["StartDate"]).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                        Model.ToDate = Convert.ToDateTime(System.Web.HttpContext.Current.Session["LastDate"]).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    }
                    else
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
                            }
                            if (date[1] != "undefined")
                            {
                                Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
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
                        for (int n = 0; n <= dt.Rows.Count - 1; n++)
                        {
                            string mstr2 = "";
                            for (int m = 0; m <= marr.Count() - 1; m++)
                            {
                                if (marr[m] != "")
                                {
                                    mstr2 += dt.Rows[n][Convert.ToInt32(marr[m]) - 1];
                                }
                            }

                            if (mstr == mstr2)
                            {
                                for (int z = 0; z <= dt.Columns.Count - 1; z++)
                                {
                                    if (mptomerge.Contains("," + (z + 1).ToString() + ","))
                                    {
                                        if (dt.Columns[z].DataType == System.Type.GetType("System.Byte") || dt.Columns[z].DataType == System.Type.GetType("System.Decimal") || dt.Columns[z].DataType == System.Type.GetType("System.Double") || dt.Columns[z].DataType == System.Type.GetType("System.Int16") || dt.Columns[z].DataType == System.Type.GetType("System.Int32") || dt.Columns[z].DataType == System.Type.GetType("System.Int64") || dt.Columns[z].DataType == System.Type.GetType("System.Single"))
                                        {
                                            dt.Rows[n][z] = 0;
                                        }
                                        else
                                        {
                                            dt.Rows[n][z] = "";
                                        }
                                    }
                                }
                            }
                            mstr = mstr2;
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
                            decimal mbal = mopening;
                            foreach (DataRow dr in dt.Rows)
                            {
                                mbal += (decimal)dr[mbalcol];
                                dr[mruncol] = mbal;
                            }
                        }
                    }

                    //string mSumJson = "";
                    StringBuilder jsonBuilder = new StringBuilder();
                    if ((mReportType == "R" || mReportType == "T") && dt.Rows.Count > 0)
                    {
                        try
                        {
                            DataTable msumdt = GetDataTable(@mSumString.Replace("[[", "[").Replace("]]", "]"), connstring);
                            //float[] marr = new float[dt.Columns.Count];
                            dt.Rows.Add();
                            if (msumdt.Rows.Count > 0)
                            {
                                int x = dt.Rows.Count;
                                for (int m = 0; m <= msumdt.Columns.Count - 1; m++)
                                {
                                    if (msumdt.Rows[0][m].ToString() == "")
                                    {
                                        dt.Rows[x - 1][m] = "";
                                    }
                                    else
                                    {
                                        try { dt.Rows[x - 1][m] = Convert.ToDecimal(msumdt.Rows[0][m]); }
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

                    if (mReportType != "R" && mWhat != "PDF" && mWhat != "PDL" && mWhat != "XLS")
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
                            var mWidths = (from xx in ctxTFAT.TfatSearch
                                           where xx.Code == Model.ViewDataId && xx.CalculatedCol != true
                                           orderby xx.Sno
                                           select new { xx.ColHead, ColWidth = (float)(xx.IsHidden == true ? 0 : xx.ColWidth) }).ToList();
                            float[] headerx = mWidths.Select(z => z.ColWidth).ToArray();
                            string tab = "";
                            string mHead = "";
                            DateTime mDate = Convert.ToDateTime(Model.ToDate);
                            int x = 0;
                            foreach (DataColumn dc in dt.Columns)
                            {
                                if (dc.ColumnName != "_Style" && headerx[x] > 5)
                                {
                                    mHead = mWidths[x].ColHead.Trim().Replace("##", "");
                                    if (mHead == "") mHead = dc.ColumnName;
                                    if (mHead.Contains("%"))
                                    {
                                        //mHead = ProcessReportHeader(mHead, mDate);
                                    }
                                    Response.Write(tab + mHead);//dc.ColumnName
                                    tab = "\t";
                                }
                                ++x;
                            }
                            Response.Write("\n");
                            x = 0;
                            foreach (DataRow dr in dt.Rows)
                            {
                                tab = "";
                                x = 0;
                                for (int i = 0; i < dt.Columns.Count; i++)
                                {
                                    if (dt.Columns[i].ColumnName != "_Style" && headerx[x] > 5)
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
                            CreatePDF(Model, dt, Model.AccountDescription, Model.mWhat == "PDL" ? "Landscape" : "Portrait", mpapersize);
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

        [HttpPost]
        public ActionResult GetMultiPrint(GridOption Model)
        {
            List<GridOption> Grlist = new List<GridOption>();
            Model.Type = ctxTFAT.DocFormats.Where(x => x.FormatCode == Model.ViewDataId).Select(x => x.Type).FirstOrDefault();


            var list = ctxTFAT.DocFormats.Where(x => x.Type == Model.Type && x.OutputDevice != "H").Select(x => x).ToList();
            if (list != null)
            {
                foreach (var a in list)
                {
                    Grlist.Add(new GridOption
                    {
                        Format = a.FormatCode,
                        IsFormatSelected = a.Selected,
                        StoreProcedure = a.StoredProc
                    });
                }

            }
            Model.PrintGridList = Grlist;
            var html = ViewHelper.RenderPartialView(this, "ReportPrintOptions", new GridOption() { PrintGridList = Model.PrintGridList, Document = Model.Document, ViewDataId = Model.ViewDataId });
            var jsonResult = Json(new
            {
                Document = Model.Document,
                PrintGridList = Model.PrintGridList,
                Html = html
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }

        public ActionResult PrintSingleDocumentCrystal(GridOption Model)
        {
            //var relativePath = "~/Reports/" + Model.Format + ".rpt";
            if (Model.Format == null)
            {
                return Json(new { Status = "filenotfound" }, JsonRequestBehavior.AllowGet);
            }

            if (FileExists("~/Reports/" + Model.Format.Trim() + ".rpt") == false)
            {
                return Json(new { Status = "filenotfound" }, JsonRequestBehavior.AllowGet);
            }
            #region Remove Comment
            var absolutePath = HttpContext.Server.MapPath("~/Reports/" + Model.Format.Trim() + ".rpt");
            if (System.IO.File.Exists(absolutePath) == false)
            {
                return Json(new { Status = "filenotfound" }, JsonRequestBehavior.AllowGet);
            }
            #endregion
            //var PDFName = Model.Document.Substring(20);
            var PDFName = "";
            //var ViewId = Model.ViewDataId.Trim();
            //if (ViewId== "LREntry")
            //{
            //    var DocUment = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == Model.Document).FirstOrDefault();
            //    if (DocUment!=null)
            //    {
            //        PDFName += "LR :" + DocUment.LrNo;
            //    }
            //}

            string mParentKey = Model.Document;
            Model.Branch = mbranchcode;
            DataTable dtreport = new DataTable();
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("SPDoc_" + Model.StoreProcedure, tfat_conx); //name of the storedprocedure
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@mParentKey", mParentKey);
            cmd.Parameters.AddWithValue("@mBranch", Model.Branch);
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            adp.Fill(dtreport);

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), Model.Format.Trim() + ".rpt"));
            rd.SetDataSource(dtreport);

            var VerifiedKey = dtreport.Rows[0]["VerifiedKey"].ToString();

            if (VerifiedKey.ToString().ToLower() == mParentKey.ToString().ToLower())
            {
                try
                {
                    Stream mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                    mstream.Seek(0, SeekOrigin.Begin);

                    rd.Close();
                    rd.Dispose();
                    //Response.Headers.Add("Content-Disposition", "DemoLR");
                    if (String.IsNullOrEmpty(PDFName))
                    {
                        return File(mstream, "application/PDF");
                    }
                    else
                    {
                        return File(mstream, "application/PDF", PDFName + ".pdf");
                    }

                    //return File(mstream,"application/PDF","Demo"); //Automatically Download With Demo Name File But Some Extension Problem Over There.


                }
                catch
                {
                    rd.Close();
                    rd.Dispose();
                    throw;
                }
                finally
                {
                    rd.Close();
                    rd.Dispose();
                }
            }
            else
            {
                rd.Close();
                rd.Dispose();
                return Json(new { Status = "Something Wrong" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SendMultiReport(GridOption Model)
        {
            //var PDFName = Model.Document.Substring(20);
            var PDFName = "";
            if (Model.Format == null)
            {
                return null;
            }

            string mParentKey = "";
            Document document = new Document(PageSize.A4, 10, 10, 10, 10);
            bool CheckDocument = true;
            var ms = new MemoryStream();
            {
                PdfCopy pdf = new PdfCopy(document, ms);
                document.Open();
                pdf.Open();

                List<string> mformats = Model.Format.Split(',').ToList();
                foreach (var mformat in mformats)
                {
                    mParentKey = Model.Document;
                    Model.Branch = mbranchcode;
                    Model.StoreProcedure = ctxTFAT.DocFormats.Where(x => x.FormatCode.Trim() == mformat.Trim()).Select(x => x.StoredProc).FirstOrDefault();
                    DataTable dtreport = new DataTable();
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("SPDoc_" + Model.StoreProcedure, tfat_conx); //name of the storedprocedure
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@mParentKey", mParentKey);
                    cmd.Parameters.AddWithValue("@mBranch", Model.Branch);
                    SqlDataAdapter adp = new SqlDataAdapter(cmd);
                    adp.Fill(dtreport);

                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/Reports"), mformat.Trim() + ".rpt"));
                    rd.SetDataSource(dtreport);
                    var VerifiedKey = dtreport.Rows[0]["VerifiedKey"].ToString();
                    if (VerifiedKey.ToString().ToLower() == mParentKey.ToString().ToLower())
                    {
                        try
                        {
                            Stream mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                            mstream.Seek(0, SeekOrigin.Begin);

                            Warning[] warnings;
                            string[] streamids;
                            string mimeType;
                            string encoding;
                            string extension;
                            MemoryStream memory1 = new MemoryStream();
                            mstream.CopyTo(memory1);
                            byte[] bytes = memory1.ToArray();
                            MemoryStream memoryStream = new MemoryStream(bytes);
                            PdfReader imageDocumentReader = new PdfReader(memoryStream.ToArray());
                            int ab = imageDocumentReader.NumberOfPages;
                            for (int a = 1; a <= ab; a++)
                            {
                                var page = pdf.GetImportedPage(imageDocumentReader, a);
                                pdf.AddPage(page);
                            }
                            imageDocumentReader.Close();
                        }
                        catch
                        {
                            rd.Close();
                            rd.Dispose();
                            throw;
                        }
                        finally
                        {
                            rd.Close();
                            rd.Dispose();
                        }
                    }
                    else
                    {
                        CheckDocument = false;
                        rd.Close();
                        rd.Dispose();
                        break;
                    }
                }
            }
            document.Close();
            if (CheckDocument)
            {
                if (String.IsNullOrEmpty(PDFName))
                {
                    return File(ms.ToArray(), "application/PDF");
                }
                else
                {
                    return File(ms.ToArray(), "application/PDF", PDFName + ".pdf");
                }
            }
            else
            {
                return Json(new { Status = "Something Wrong" }, JsonRequestBehavior.AllowGet);
            }
            //return File(ms.ToArray(), "application/PDF");

        }



    }
}