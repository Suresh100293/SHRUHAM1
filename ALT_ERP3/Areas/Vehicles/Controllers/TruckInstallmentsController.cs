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
using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Vehicles.Models;
using ALT_ERP3.Controllers;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Vehicles.Controllers
{
    public class TruckInstallmentsController : BaseController
    {

        //ITransactionGridOperation mIlst = new TransactionGridOperation();
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        private static string mdocument = "";
        private static string mauthorise = "A00";
        private int mnewrecordkey = 0;

        public ActionResult GetFormats()
        {
            var result = ctxTFAT.ReportHeader.Where(x => x.SubCodeOf == msubcodeof && !x.Code.EndsWith(".bak")).Select(m => new
            {
                Code = m.Code,
                Name = m.Code
            }).OrderBy(n => n.Code).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetVehiclesNo(string term)
        {
            var vehiclelist = ctxTFAT.VehicleMaster.ToList();
            if (!(string.IsNullOrEmpty(term)))
            {
                vehiclelist = ctxTFAT.VehicleMaster.Where(x => x.TruckNo.ToLower().Contains(term.ToLower())).ToList();
            }
            var Modified = vehiclelist.Select(x => new
            {
                Code = x.Code,
                Name = x.TruckNo
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);

            //return Json((from m in ctxTFAT.VehicleMaster
            //             where m.Vehicle_No.ToLower().Contains(term.ToLower())
            //             select new { Name = m.Vehicle_No, Code = m.Vehicle_No }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        // GET: Vehicles/TruckInstallments
        public ActionResult Index(GridOption Model)
        {

            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());

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
            //mdocument = mModel.Document;

            //if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            //{
            //    var mList = ctxTFAT.TyreMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
            //    //var PartyCode = Convert.ToInt32(mList.Code);

            //    if (mList != null)
            //    {
            //        mModel.Code = mList.Code;
            //        mModel.Name = mList.Name;
            //        mModel.Active = mList.Active;
            //        mModel.Tyre_Type = mList.Tyre_Type;
            //        mModel.Expiry_Date = mList.Expiry_Date;
            //        mModel.Expiry_Days = mList.Expiry_Days;
            //        mModel.KM = mList.KM;
            //    }
            //}
            //else
            //{
            //    mModel.Code = "";
            //    mModel.Name = "";
            //    mModel.Active = false;
            //    mModel.Tyre_Type = "";
            //    mModel.KM = 0;
            //    mModel.Expiry_Days = 0;
            //    mModel.Expiry_Date = "";
            //}
            return View(Model);
        }

        #region Grid Data & Columns

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
            if (Model.IsFormatSelected == true)
            {
                int noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=0 Where SubCodeOf='" + msubcodeof + "'");
                noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=-1 Where Code='" + Model.ViewDataId + "'");
            }
            return GetGridReport1(Model, "M", "MainType^" + Model.MainType, false, 0);
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
                            Model.FromDate = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            Model.ToDate = (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                        }
                        cmd.Parameters.Add("@mReportStartDate", SqlDbType.VarChar).Value = Model.FromDate;
                        cmd.Parameters.Add("@mReportEndDate", SqlDbType.VarChar).Value = Model.ToDate;
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
                    cmd.Parameters.Add("@mSumString", SqlDbType.VarChar, 5000).Value = "";
                    cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
                    cmd.Parameters["@mSumString"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;
                    con.Open();
                    da.SelectCommand = cmd;
                    da.Fill(dt);
                    //string mSumString = (string)(cmd.Parameters["@mSumString"].Value ?? "");
                    string mReturnString = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
                    #region Datatable Operation

                    DataTable newDT = new DataTable();
                    DataTable FinalDt = new DataTable();
                    DataRow[] foundRows = dt.Select("[Vehicle No]='" + Model.Value1 + "'");
                    //DataRow[] foundRows1 = dt.Select("[Vehicle No]=''"+Model.Value1+"''");


                    if (foundRows.Length > 0)
                    {
                        newDT = foundRows.CopyToDataTable();
                        FinalDt = newDT.Copy();
                    }
                    else
                    {
                        FinalDt = dt;
                    }

                    #endregion

                    string mSumString = (string)cmd.Parameters["@mSumString"].Value;
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
                        Array mtotals = Array.CreateInstance(typeof(double), FinalDt.Columns.Count);
                        Array mfixeds = Array.CreateInstance(typeof(double), FinalDt.Columns.Count);
                        bool mflg = false;
                        int mstart = 0;
                        for (int n = 0; n <= FinalDt.Rows.Count - 1; n++)
                        {
                            string mstr2 = "";
                            for (int m = 0; m <= marr.Count() - 1; m++)
                            {
                                if (marr[m] != "")
                                    mstr2 += FinalDt.Rows[n][Convert.ToInt32(marr[m]) - 1];
                            }

                            if (mstr == mstr2)
                            {
                                for (int z = 0; z <= FinalDt.Columns.Count - 1; z++)
                                {
                                    if (mptomerge.Contains((z + 1).ToString() + ","))
                                    {
                                        mtotals.SetValue(Convert.ToDouble(mtotals.GetValue(z)) + Convert.ToDouble(FinalDt.Rows[mstart][z]), z);
                                        mfixeds.SetValue(Convert.ToDouble(mfixeds.GetValue(z)) + Convert.ToDouble(FinalDt.Rows[mstart][z]), z);
                                    }
                                }
                                if (mpblank == false)
                                {
                                    FinalDt.Rows[n].Delete();
                                }
                                else
                                {
                                    for (int m = 0; m <= marr.Count() - 1; m++)
                                    {
                                        if (marr[m] != "")
                                        {
                                            //dt.Rows[n][m] = "";
                                            int x = Convert.ToInt32(marr[m]) - 1;
                                            if (FinalDt.Columns[x].DataType == System.Type.GetType("System.Byte") || FinalDt.Columns[x].DataType == System.Type.GetType("System.Decimal") || FinalDt.Columns[x].DataType == System.Type.GetType("System.Double") || FinalDt.Columns[x].DataType == System.Type.GetType("System.Int16") || FinalDt.Columns[x].DataType == System.Type.GetType("System.Int32") || FinalDt.Columns[x].DataType == System.Type.GetType("System.Int64") || FinalDt.Columns[x].DataType == System.Type.GetType("System.Single"))
                                            {
                                                FinalDt.Rows[n][x] = 0;
                                            }
                                            else
                                            {
                                                FinalDt.Rows[n][x] = "";
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int z = 0; z <= FinalDt.Columns.Count - 1; z++)
                                {
                                    if (mptomerge.Contains((z + 1).ToString() + ","))
                                    {
                                        if (mflg == true)
                                        {
                                            FinalDt.Rows[mstart][z] = mtotals.GetValue(z);

                                            if (Convert.ToDouble(mfixeds.GetValue(z)) == 0)
                                            {
                                                if (FinalDt.Columns[z].DataType == System.Type.GetType("System.Byte") || FinalDt.Columns[z].DataType == System.Type.GetType("System.Decimal") || FinalDt.Columns[z].DataType == System.Type.GetType("System.Double") || FinalDt.Columns[z].DataType == System.Type.GetType("System.Int16") || FinalDt.Columns[z].DataType == System.Type.GetType("System.Int32") || FinalDt.Columns[z].DataType == System.Type.GetType("System.Int64") || FinalDt.Columns[z].DataType == System.Type.GetType("System.Single"))
                                                {
                                                    FinalDt.Rows[mstart][z] = 0;
                                                }
                                                else
                                                {
                                                    FinalDt.Rows[mstart][z] = "";
                                                }
                                            }
                                        }
                                        mtotals.SetValue(Convert.ToDouble(FinalDt.Rows[mstart][z]), z);
                                        mfixeds.SetValue(Convert.ToDouble(FinalDt.Rows[mstart][z]), z);
                                    }
                                }
                                mstart = n;
                                mstr = mstr2;
                            }
                            mflg = true;
                        }

                        for (int z = 0; z <= FinalDt.Columns.Count - 1; z++)
                        {
                            if (mptomerge.Contains((z + 1).ToString() + ","))
                            {
                                FinalDt.Rows[mstart][z] = mtotals.GetValue(z);
                                if (Convert.ToDouble(mfixeds.GetValue(z)) == 0)
                                {
                                    //if (dt.Columns[z].DataType == System.Type.GetType("System.String"))
                                    //{
                                    //    dt.Rows[mstart][z] = "";
                                    //}else 
                                    if (FinalDt.Columns[z].DataType == System.Type.GetType("System.Byte") || FinalDt.Columns[z].DataType == System.Type.GetType("System.Decimal") || FinalDt.Columns[z].DataType == System.Type.GetType("System.Double") || FinalDt.Columns[z].DataType == System.Type.GetType("System.Int16") || FinalDt.Columns[z].DataType == System.Type.GetType("System.Int32") || FinalDt.Columns[z].DataType == System.Type.GetType("System.Int64") || FinalDt.Columns[z].DataType == System.Type.GetType("System.Single"))
                                    {
                                        FinalDt.Rows[mstart][z] = 0;
                                    }
                                    else
                                    {
                                        FinalDt.Rows[mstart][z] = "";
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
                        for (i = 0; i < FinalDt.Columns.Count; i++)
                        {
                            string mcolname = FinalDt.Columns[i].ColumnName.Trim().ToLower();
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
                            foreach (DataRow dr in FinalDt.Rows)
                            {
                                mbal += (decimal)dr[mbalcol];
                                dr[mruncol] = mbal;
                            }
                        }
                    }

                    //string mSumJson = "";
                    StringBuilder jsonBuilder = new StringBuilder();
                    if (mReportType == "R" && FinalDt.Rows.Count > 0)
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
                        FinalDt.Columns.Add("XYZ", typeof(string)).SetOrdinal(0);
                        FinalDt.Columns.Add("ABC", typeof(string)).SetOrdinal(1);
                        FinalDt.Columns.Add("FGH", typeof(string)).SetOrdinal(2);
                        FinalDt.Columns.Add("PTG", typeof(string)).SetOrdinal(3);
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
                        return Content(JQGridHelper.JsonForJqgrid(FinalDt, Model.rows, mxRowCount, Model.page, jsonBuilder.ToString()), "application/json");
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
                            foreach (DataColumn dc in FinalDt.Columns)
                            {
                                Response.Write(tab + dc.ColumnName);
                                tab = "\t";
                            }
                            Response.Write("\n");
                            int i;
                            foreach (DataRow dr in FinalDt.Rows)
                            {
                                tab = "";
                                for (i = 0; i < FinalDt.Columns.Count; i++)
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
                            CreatePDF(Model, FinalDt, Model.AccountDescription, Model.mWhat == "PDL" ? "Landscape" : "Portrait", mpapersize);
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

        public ActionResult NewIndex(TruckInstallmentVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());

            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");

            mdocument = mModel.Document;

            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.TruckInstallmentMa.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                //var PartyCode = Convert.ToInt32(mList.Code);

                if (mList != null)
                {
                    mModel.Code = mList.Code;
                    mModel.Vehicle_No = mList.VehicleNo;
                    mModel.Vehicle_NoName = ctxTFAT.VehicleMaster.Where(x => x.Code == mList.VehicleNo).Select(x => x.TruckNo).FirstOrDefault();
                    mModel.Date = mList.Date.ToShortDateString();
                    mModel.Installment_Amount = mList.InstallmentAmt.ToString();
                    mModel.Clear = (Clear)Enum.Parse(typeof(Clear), mList.Clear == true ? "Yes" : "No");
                }
            }
            else
            {
                mModel.Code = "";
                mModel.Vehicle_No = "";
                mModel.Date = "";
                mModel.Installment_Amount = "";
                mModel.Clear = (Clear)Enum.Parse(typeof(Clear), "No");
            }
            return View(mModel);
        }


        #region SaveData
        public ActionResult SaveData(TruckInstallmentVM mModel)
        {
            //bool Status = false;

            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    //if (mModel.AcitveorNot.ToString() == "True")
                    //{
                    //    Status = true;
                    //}
                    TruckInstallmentMa mobj = new TruckInstallmentMa();
                    bool mAdd = true;
                    if (ctxTFAT.TruckInstallmentMa.Where(x => (x.Code == mModel.Document)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.TruckInstallmentMa.Where(x => (x.Code == mModel.Document)).FirstOrDefault();

                        mAdd = false;
                    }

                    //mobj.ShortName = mModel.ShortName;
                    mobj.Date = ConvertDDMMYYTOYYMMDD(mModel.Date);
                    mobj.InstallmentAmt = Convert.ToDecimal(mModel.Installment_Amount);
                    mobj.Clear = mModel.Clear.ToString() == "Yes" ? true : false;
                    //mobj.Country = mModel.TfatState_Country;
                    //// iX9: default values for the fields not used @Form
                    //mobj.StateCode = "";
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                    if (mAdd == true)
                    {
                        int NewCode1;
                        var NewCode = ctxTFAT.TruckInstallmentMa.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).Take(1).FirstOrDefault();
                        if (NewCode == null || NewCode == "")
                        {
                            NewCode1 = 100000;
                        }
                        else
                        {
                            NewCode1 = Convert.ToInt32(NewCode) + 1;
                        }
                        string FinalCode = NewCode1.ToString("D6");
                        mobj.Code = FinalCode;
                        mobj.VehicleNo = mModel.Vehicle_No;

                        ctxTFAT.TruckInstallmentMa.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = Convert.ToInt32(mobj.RECORDKEY);
                    string mNewCode = "";
                    mNewCode = mobj.Code;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "VINST" + mperiod.Substring(0, 2) + mobj.Code, DateTime.Now, 0, "", "Save Truck Installments", "NA");

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

        public ActionResult DeleteStateMaster(TruckInstallmentVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var mList = ctxTFAT.TruckInstallmentMa.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
            ctxTFAT.TruckInstallmentMa.Remove(mList);
            ctxTFAT.SaveChanges();
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "VINST" + mperiod.Substring(0, 2) + mList.Code, DateTime.Now, 0, "", "Delete Truck Installments", "NA");

            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData

        //public string ConvertDDMMYYTOYYMMDD(string da)
        //{
        //    string abc = da.Substring(6, 4) + "/" + da.Substring(3, 2) + "/" + da.Substring(0, 2);
        //    return abc;
        //}

        public JsonResult AutoCompleteVehicleCategory(string term)
        {
            return Json((from m in ctxTFAT.VehicleMaster
                         where m.TruckNo.ToLower().Contains(term.ToLower())
                         select new { Name = m.TruckNo, Code = m.TruckNo }).ToArray(), JsonRequestBehavior.AllowGet);
        }
    }
}