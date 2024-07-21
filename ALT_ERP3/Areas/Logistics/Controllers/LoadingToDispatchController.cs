using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using System.Reflection;
using System.Net;
using System.Collections;
using System.Globalization;
using Newtonsoft.Json.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using CrystalDecisions.CrystalReports.Engine;
using Microsoft.Reporting.WebForms;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class LoadingToDispatchController : BaseController
    {

        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        public static string connstring = "";
        private string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

        #region Function List

        private static List<SelectListItem> PopulateUsersBranches()
        {
            string muserid = (System.Web.HttpContext.Current.Session["UserId"] ?? "Super").ToString();
            List<SelectListItem> items = new List<SelectListItem>();
            //string constr = ConfigurationManager.ConnectionStrings["tfatEntitiesConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connstring))
            {
                string query = " SELECT Code, Name FROM TfatBranch where Code <> 'G00000' and Grp <> 'G00000' and Category <> 'Area' and Users Like '%" + muserid + "%'";
                if (muserid == "Super")
                {
                    query = " SELECT Code, Name FROM TfatBranch where Code <> 'G00000' and Grp <> 'G00000' and Category <> 'Area' ";
                }
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

        private List<SelectListItem> PopulateDeliveryStatus()
        {
            List<SelectListItem> StoreCodelst = new List<SelectListItem>();
            StoreCodelst.Add(new SelectListItem { Text = "OK", Value = "OK" });
            StoreCodelst.Add(new SelectListItem { Text = "Package Damage", Value = "Package Damage" });
            StoreCodelst.Add(new SelectListItem { Text = "Material Damage", Value = "Material Damage" });
            StoreCodelst.Add(new SelectListItem { Text = "Short", Value = "Short" });
            return StoreCodelst;
        }

        public JsonResult From(string term, bool Flag)
        {
            if (Flag)
            {
                string BranchCode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();

                //var GetParentCode = ctxTFAT.TfatBranch.Where(x => x.Code == mcompcode).Select(x => x.Code).FirstOrDefault();
                List<TfatBranch> list = GetBranch(BranchCode);

                list = list.Where(x => x.Category != "0").ToList();
                if (!(String.IsNullOrEmpty(term)))
                {
                    list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
                }

                List<TfatBranch> treeTables = new List<TfatBranch>();

                foreach (var item in list)
                {
                    if (item.Category == "Zone")
                    {
                        item.Name += " - Z";
                        treeTables.Add(item);
                    }
                    else if (item.Category == "Branch")
                    {
                        item.Name += " - B";
                        treeTables.Add(item);
                    }
                    else if (item.Category == "SubBranch")
                    {
                        item.Name += " - SB";
                        treeTables.Add(item);
                    }
                    else
                    {
                        item.Name = item.Name + " - A";
                        treeTables.Add(item);
                    }

                }

                var Modified = treeTables.Select(x => new
                {
                    Code = x.Code,
                    Name = x.Name
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);




                //return Json(Modified, JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<TfatBranch> list = GetBranch("HO0000");
                if (!(String.IsNullOrEmpty(term)))
                {
                    list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
                }

                List<TfatBranch> treeTables = new List<TfatBranch>();

                foreach (var item in list)
                {
                    if (item.Category == "Zone")
                    {
                        item.Name += " - Z";
                        treeTables.Add(item);
                    }
                    else if (item.Category == "Branch")
                    {
                        item.Name += " - B";
                        treeTables.Add(item);
                    }
                    else if (item.Category == "SubBranch")
                    {
                        item.Name += " - SB";
                        treeTables.Add(item);
                    }
                    else
                    {
                        item.Name = item.Name + " - A";
                        treeTables.Add(item);
                    }
                }
                var Modified = treeTables.Select(x => new
                {
                    Code = x.Code,
                    Name = x.Name
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);


            }
        }

        public JsonResult To(string term, string From)
        {
            List<TfatBranch> list = ctxTFAT.TfatBranch.Where(x => x.Category != "Zone" && x.Category != "0" && (!(x.Code.Contains(From)))).ToList();

            //list = list.Where(x => x.Category != "Area" && x.Category != "0" && (!(x.Code.Contains(From)))).ToList();
            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }

            List<TfatBranch> treeTables = new List<TfatBranch>();

            foreach (var item in list)
            {
                if (item.Category == "Zone")
                {
                    item.Name += " - Z";
                    treeTables.Add(item);
                }
                else if (item.Category == "Branch")
                {
                    item.Name += " - B";
                    treeTables.Add(item);
                }
                else if (item.Category == "SubBranch")
                {
                    item.Name += " - SB";
                    treeTables.Add(item);
                }
                else
                {
                    item.Name = item.Name + " - A";
                    treeTables.Add(item);
                }
            }
            var Modified = treeTables.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public JsonResult OTHBranchUnload(string term)
        {
            List<TfatBranch> list = new List<TfatBranch>();
            list = ctxTFAT.TfatBranch.Where(x => x.Status == true && x.Code != "G00000" && x.Category != "Zone").ToList();
            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }

            List<TfatBranch> treeTables = new List<TfatBranch>();

            foreach (var item in list)
            {
                if (item.Category == "Branch")
                {
                    item.Name += " - B";
                    treeTables.Add(item);
                }
                else if (item.Category == "SubBranch")
                {
                    item.Name += " - SB";
                    treeTables.Add(item);
                }
                else
                {
                    item.Name = item.Name + " - A";
                    treeTables.Add(item);
                }
            }
            var Modified = treeTables.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            }).Take(10).ToList();
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        private static List<SelectListItem> PopulateBranches()
        {

            List<SelectListItem> items = new List<SelectListItem>();
            //string constr = ConfigurationManager.ConnectionStrings["tfatEntitiesConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connstring))
            {
                string query = " SELECT Code, Name FROM TfatBranch";
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

        private static List<SelectListItem> PopulateGeneralArea()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            //string constr = ConfigurationManager.ConnectionStrings["tfatEntitiesConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connstring))
            {
                string query = " SELECT Code, Name FROM TfatBranch where grp='G00000' and status='true' ";
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

        public DateTime ConvertDDMMYYTOYYMMDD(string da)
        {
            string abc = da.Substring(6, 4) + "/" + da.Substring(3, 2) + "/" + da.Substring(0, 2);
            return Convert.ToDateTime(abc);
        }

        public string GetNewCode_VehiHistory()
        {
            string Code = ctxTFAT.tfatVehicleStatusHistory.OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefault();
            if (String.IsNullOrEmpty(Code))
            {

                return "100000";
            }
            else
            {
                return (Convert.ToInt32(Code) + 1).ToString();
            }
        }

        public string GetNewCodeHistory()
        {
            var NewLcNo = ctxTFAT.TfatVehicleStatus.OrderByDescending(x => x.DocNo).Select(x => x.DocNo).Take(1).FirstOrDefault();
            int LcNo;
            if (String.IsNullOrEmpty(NewLcNo))
            {

                LcNo = 100000;
            }
            else
            {
                LcNo = Convert.ToInt32(NewLcNo) + 1;
            }

            return LcNo.ToString();
        }
        #endregion

        // GET: Logistics/LoadingToDispatch
        #region Index(Grid List)

        public ActionResult Index(GridOption Model)
        {

            connstring = GetConnectionString();
            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());

            UpdateAuditTrail(mbranchcode, "Veh-ACT", Model.Header, "", DateTime.Now, 0, "", "", "NA");

            if (Model.Document == null)
            {
                Model.Document = "";
                Model.AccountName = "";
            }
            Model.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
            Model.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
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

            LogisticsFlow flow = ctxTFAT.LogisticsFlow.FirstOrDefault();
            if (flow == null)
            {
                Model.ScheduleFlow = false;
                Model.Rework = false;
            }
            else
            {
                Model.ScheduleFlow = flow.ScheduleFollowUp;
                Model.Rework = flow.Rework;
            }

            Model.Branches = PopulateUsersBranches();
            foreach (var item in Model.Branches)
            {
                Model.Branch += item.Value + ",";
            }
            Model.Branch = mbranchcode;


            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            if (Model.AcType != null)
            {
                ViewBag.MainType = Model.AcType;
            }
            TempData["StoreGridModalForDraft"] = Model;
            return View(Model);
        }

        public ActionResult GetFormats1()
        {
            var result = ctxTFAT.ReportHeader.Where(x => x.SubCodeOf == msubcodeof && !x.Code.EndsWith(".bak")).Select(m => new
            {
                Code = m.Code,
                Name = m.Code
            }).OrderBy(n => n.Code).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords1(string id)
        {
            var result = ctxTFAT.TfatMenu.Where(z => z.OptionCode == moptioncode && z.ModuleName == mmodule).Select(x => new { x.AllowEdit, x.AllowDelete }).FirstOrDefault();
            string mopt = "XXXX";
            if (result != null)
            {
                mopt = (result.AllowEdit == false ? "X" : "E") + (result.AllowDelete == false ? "X" : "D") + "XX";
            }
            //return GetGridDataColumns(id, "L", mopt);
            // mopt = "EDXX";
            return GetGridDataColumns(id, "L", mopt);
        }

        public ActionResult GetMasterGridData1(GridOption Model)
        {

            if (Model.IsFormatSelected == true)
            {
                int noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=0 Where SubCodeOf='" + msubcodeof + "'");
                noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=-1 Where Code='" + Model.ViewDataId + "'");
            }
            if (!String.IsNullOrEmpty(Model.searchString) && Model.searchString != "undefined")
            {
                string VehicleCodes = "";
                Model.searchOper = "in";
                Model.searchField = " FM.TruckNo ";
                var VehicleMaster = ctxTFAT.VehicleMaster.Where(x => x.TruckNo.Contains(Model.searchString)).Select(x => x.Code).ToList();
                foreach (var item in VehicleMaster)
                {
                    VehicleCodes += "'" + item + "',";
                }
                var HireVehicleMaster = ctxTFAT.HireVehicleMaster.Where(x => x.TruckNo.Contains(Model.searchString)).Select(x => x.Code).ToList();
                foreach (var item in HireVehicleMaster)
                {
                    VehicleCodes += "'" + item + "',";
                }
                if (!String.IsNullOrEmpty(VehicleCodes))
                {
                    VehicleCodes = VehicleCodes.Substring(0, VehicleCodes.Length - 1);
                }
                Model.searchString = VehicleCodes;
            }

            if (String.IsNullOrEmpty(Model.Branch))
            {
                Model.Branch = "'" + mbranchcode + "'";
                mpara = "para01" + "^" + Model.Branch + "~";
            }
            else
            {
                mpara = "para01" + "^" + Model.Branch + "~";
            }
            if (String.IsNullOrEmpty(Model.Code))
            {
                Model.Code = "1=1";
            }
            else
            {
                if (Model.Code == "Active")
                {
                    Model.Code = "FMR.RouteClear='false'";
                }
                else if (Model.Code == "Clear")
                {
                    Model.Code = "FMR.RouteClear='true'";
                }
                else if (Model.Code == "Complete")
                {
                    Model.Code = "FM.FmStatus='C'";
                }
                else if (Model.Code == "All")
                {
                    Model.Code = "1=1";
                }

            }


            return GetGridReport1(Model, "M", "Code^" + Model.Code + "~MainType^" + Model.MainType + (mpara != "" ? "~" + mpara : ""), false, 0);
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

                    DataTable dataTable = new DataTable();
                    dataTable = RemoveDuplicateRows(dt, "Tablekey");

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
                        var MergingSingleColumn = mvar.pMerge.Split('^');
                        var MergingMultiplColumnList = mvar.pToMerge.Split('^');
                        for (int i = 0; i < MergingSingleColumn.Length; i++)
                        {
                            mpmerge = MergingSingleColumn[i];
                            mptomerge = MergingMultiplColumnList[i];

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
                            for (int n = 0; n <= dataTable.Rows.Count - 1; n++)
                            {
                                string mstr2 = "";
                                for (int m = 0; m <= marr.Count() - 1; m++)
                                {
                                    if (marr[m] != "")
                                    {
                                        mstr2 += dataTable.Rows[n][Convert.ToInt32(marr[m]) - 1];
                                    }
                                }

                                if (mstr == mstr2)
                                {
                                    for (int z = 0; z <= dataTable.Columns.Count - 1; z++)
                                    {
                                        if (mptomerge.Contains("," + (z + 1).ToString() + ","))
                                        {
                                            if (dataTable.Columns[z].DataType == System.Type.GetType("System.Byte") || dataTable.Columns[z].DataType == System.Type.GetType("System.Decimal") || dataTable.Columns[z].DataType == System.Type.GetType("System.Double") || dataTable.Columns[z].DataType == System.Type.GetType("System.Int16") || dataTable.Columns[z].DataType == System.Type.GetType("System.Int32") || dataTable.Columns[z].DataType == System.Type.GetType("System.Int64") || dataTable.Columns[z].DataType == System.Type.GetType("System.Single"))
                                            {
                                                dataTable.Rows[n][z] = 0;
                                            }
                                            else
                                            {
                                                dataTable.Rows[n][z] = "";
                                            }
                                        }
                                    }
                                }
                                mstr = mstr2;
                            }
                        }

                    }
                    // merge routine over

                    if (mRunning == true)
                    {
                        int mbalcol = -1;
                        int mruncol = -1;
                        int mCodecol = -1;
                        int i;
                        string Code = "NA", PrevCode = "NA";
                        for (i = 0; i < dataTable.Columns.Count; i++)
                        {
                            string mcolname = dataTable.Columns[i].ColumnName.Trim().ToLower();
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
                            foreach (DataRow dr in dataTable.Rows)
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
                    if ((mReportType == "R" || mReportType == "T") && dataTable.Rows.Count > 0)
                    {
                        try
                        {
                            DataTable msumdt = GetDataTable(@mSumString.Replace("[[", "[").Replace("]]", "]"), connstring);
                            //float[] marr = new float[dt.Columns.Count];
                            dataTable.Rows.Add();
                            if (msumdt.Rows.Count > 0)
                            {
                                int x = dataTable.Rows.Count;
                                for (int m = 0; m <= msumdt.Columns.Count - 1; m++)
                                {
                                    if (msumdt.Rows[0][m].ToString() == "")
                                    {
                                        dataTable.Rows[x - 1][m] = "";
                                    }
                                    else
                                    {
                                        try { dataTable.Rows[x - 1][m] = Convert.ToDecimal(msumdt.Rows[0][m]); }
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
                        dataTable.Columns.Add("XYZ", typeof(string)).SetOrdinal(0);
                        dataTable.Columns.Add("ABC", typeof(string)).SetOrdinal(1);
                        dataTable.Columns.Add("FGH", typeof(string)).SetOrdinal(2);
                        dataTable.Columns.Add("PTG", typeof(string)).SetOrdinal(3);
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
                        return Content(JQGridHelper.JsonForJqgrid(dataTable, Model.rows, mxRowCount, Model.page, jsonBuilder.ToString()), "application/json");
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
                            foreach (DataColumn dc in dataTable.Columns)
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
                            foreach (DataRow dr in dataTable.Rows)
                            {
                                tab = "";
                                x = 0;
                                for (int i = 0; i < dataTable.Columns.Count; i++)
                                {
                                    if (dataTable.Columns[i].ColumnName != "_Style" && headerx[x] > 5)
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
                            CreatePDF(Model, dataTable, Model.AccountDescription, Model.mWhat == "PDL" ? "Landscape" : "Portrait", mpapersize);
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

        public DataTable RemoveDuplicateRows(DataTable dTable, string colName)
        {
            DataTable table = new DataTable();
            Hashtable hTable = new Hashtable();
            table = dTable.Clone();
            var results = from myRow in dTable.AsEnumerable()
                          where myRow.Field<string>("Parent") == mbranchcode
                          select myRow;
            foreach (DataRow dr in results)
            {
                table.Rows.Add(dr.ItemArray);
                hTable.Add(dr[colName], string.Empty);
            }

            foreach (DataRow item in dTable.Rows)
            {
                if (hTable.Contains(item[colName]))
                {
                }
                else
                {
                    table.Rows.Add(item.ItemArray);
                    hTable.Add(item[colName], string.Empty);
                }
            }

            DataView objDV = new DataView(table);
            objDV.Sort = "RouteClear, Date desc";
            table = objDV.Table.Copy();

            return table;
        }

        private string ProcessReportHeader(string mHead, DateTime mDate)
        {
            if (mHead.Contains("%MonthYear"))
            {
                mHead = mHead.Replace("%MonthYear1", mDate.ToString("MMM") + "-" + mDate.Year);
                mHead = mHead.Replace("%MonthYear2", mDate.AddMonths(1).ToString("MMM") + "-" + mDate.AddMonths(1).Year);
                mHead = mHead.Replace("%MonthYear3", mDate.AddMonths(2).ToString("MMM") + "-" + mDate.AddMonths(2).Year);
                mHead = mHead.Replace("%MonthYear4", mDate.AddMonths(3).ToString("MMM") + "-" + mDate.AddMonths(3).Year);
                mHead = mHead.Replace("%MonthYear5", mDate.AddMonths(4).ToString("MMM") + "-" + mDate.AddMonths(4).Year);
                mHead = mHead.Replace("%MonthYear6", mDate.AddMonths(5).ToString("MMM") + "-" + mDate.AddMonths(5).Year);
                mHead = mHead.Replace("%MonthYear7", mDate.AddMonths(6).ToString("MMM") + "-" + mDate.AddMonths(6).Year);
                mHead = mHead.Replace("%MonthYear8", mDate.AddMonths(7).ToString("MMM") + "-" + mDate.AddMonths(7).Year);
                mHead = mHead.Replace("%MonthYear9", mDate.AddMonths(8).ToString("MMM") + "-" + mDate.AddMonths(8).Year);
                mHead = mHead.Replace("%MonthYearA", mDate.AddMonths(9).ToString("MMM") + "-" + mDate.AddMonths(9).Year);
                mHead = mHead.Replace("%MonthYearB", mDate.AddMonths(10).ToString("MMM") + "-" + mDate.AddMonths(10).Year);
                mHead = mHead.Replace("%MonthYearC", mDate.AddMonths(11).ToString("MMM") + "-" + mDate.AddMonths(11).Year);
            }
            if (mHead.ToLower().Contains("%para"))
            {
                // para10/20 processed first as para1 will spoil para10
                mHead = mHead.Replace("%para24", ppara24);
                mHead = mHead.Replace("%para23", ppara23);
                mHead = mHead.Replace("%para22", ppara22);
                mHead = mHead.Replace("%para21", ppara21);
                mHead = mHead.Replace("%para20", ppara20);
                mHead = mHead.Replace("%para19", ppara19);
                mHead = mHead.Replace("%para18", ppara18);
                mHead = mHead.Replace("%para17", ppara17);
                mHead = mHead.Replace("%para16", ppara16);
                mHead = mHead.Replace("%para15", ppara15);
                mHead = mHead.Replace("%para14", ppara14);
                mHead = mHead.Replace("%para13", ppara13);
                mHead = mHead.Replace("%para12", ppara12);
                mHead = mHead.Replace("%para11", ppara11);
                mHead = mHead.Replace("%para10", ppara10);
                mHead = mHead.Replace("%para09", ppara09);
                mHead = mHead.Replace("%para08", ppara08);
                mHead = mHead.Replace("%para07", ppara07);
                mHead = mHead.Replace("%para06", ppara06);
                mHead = mHead.Replace("%para05", ppara05);
                mHead = mHead.Replace("%para04", ppara04);
                mHead = mHead.Replace("%para03", ppara03);
                mHead = mHead.Replace("%para02", ppara02);
                mHead = mHead.Replace("%para01", ppara01);
                mHead = mHead.Replace("%para9", ppara09);
                mHead = mHead.Replace("%para8", ppara08);
                mHead = mHead.Replace("%para7", ppara07);
                mHead = mHead.Replace("%para6", ppara06);
                mHead = mHead.Replace("%para5", ppara05);
                mHead = mHead.Replace("%para4", ppara04);
                mHead = mHead.Replace("%para3", ppara03);
                mHead = mHead.Replace("%para2", ppara02);
                mHead = mHead.Replace("%para1", ppara01);
            }
            return mHead;
        }

        #endregion


        public ActionResult Index1(LoadingToDispatchVM mModel)
        {
            Session["TempAttach"] = null;
            Session["FMAttachmentList"] = null;
            List<ConsignmentTracking> consignments = new List<ConsignmentTracking>();
            TempData.Remove("ExistLC");
            TempData.Remove("LoadingList");
            TempData.Remove("PendingLC");
            TempData.Remove("AlreadyUnloadLrList");
            TempData.Remove("OTHAlreadyUnloadLrList");

            TempData.Remove("ExistAllLR");//new
            TempData.Remove("UnLoadingList");//new
            TempData.Remove("AllLrList");//new
            TempData.Remove("Destination");
            TempData.Remove("FMAttachmentList");
            TempData.Remove("UnloadLcList");
            TempData.Remove("AllLCLoaded");//new
            TempData.Remove("StoreGridModalForDraft");//new

            mModel.LogisticsFlow = ctxTFAT.LogisticsFlow.FirstOrDefault();
            if (mModel.LogisticsFlow == null)
            {
                mModel.LogisticsFlow = new LogisticsFlow();
            }
            mModel.UnLoadSetup = ctxTFAT.UnLoadSetup.FirstOrDefault();
            if (mModel.UnLoadSetup == null)
            {
                mModel.UnLoadSetup = new UnLoadSetup();
            }
            GetAllMenu(Session["ModuleName"].ToString());
            connstring = GetConnectionString();
            mdocument = mModel.Document;
            mModel.Branches = PopulateBranches();
            FMROUTETable fM_ROUTE = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == mdocument).FirstOrDefault();
            FMMaster fM = ctxTFAT.FMMaster.Where(x => x.TableKey == fM_ROUTE.Parentkey).FirstOrDefault();
            UpdateAuditTrail(mbranchcode, "Veh-ACT", "Vehicle Activity", fM.ParentKey, fM.Date, fM.Freight, fM.BroCode, "", "A");

            ViewBag.Authenticate = fM.AUTHORISE.Substring(0, 1);
            mModel.NarrStr = fM_ROUTE.Narr;

            #region Attachment
            //Get Attachment
            AttachmentVM Att = new AttachmentVM();
            Att.Type = "FM000";
            Att.Srl = fM.FmNo.ToString();

            AttachmentController attachmentC = new AttachmentController();
            List<AttachmentVM> attachments = attachmentC.GetAttachmentListInEdit(Att);

            Session["TempAttach"] = attachments;



            #endregion

            #region Fm Details

            mModel.FMNO = fM.FmNo.ToString();
            mModel.PayLoad = fM.PayLoad?.ToString();
            mModel.FM_Time = fM.Time;
            mModel.FM_Date = fM.Date.ToShortDateString();
            mModel.VehicleGroup = fM.VehicleStatus;
            mModel.VehicleGroup_Name = ctxTFAT.VehicleGrpStatusMas.Where(x => x.Code == mModel.VehicleGroup).Select(x => x.VehicleGroupStatus).FirstOrDefault();
            mModel.VehicleNo = fM.TruckNo;
            mModel.VehicleNoName = fM.VehicleStatus == "100001" ? ctxTFAT.HireVehicleMaster.Where(x => x.Code == fM.TruckNo).Select(x => x.TruckNo).FirstOrDefault() : ctxTFAT.VehicleMaster.Where(x => x.Code == fM.TruckNo).Select(x => x.TruckNo).FirstOrDefault();
            mModel.Broker = fM.BroCode;
            mModel.Broker_Name = ctxTFAT.Master.Where(x => x.Code.ToString() == mModel.Broker).Select(x => x.Name).FirstOrDefault();
            mModel.KM = Convert.ToDecimal(fM.KM);
            mModel.From = fM.FromBranch;
            mModel.From_Name = ctxTFAT.TfatBranch.Where(x => x.Code == fM.FromBranch).Select(x => x.Name).FirstOrDefault();
            mModel.To = fM.ToBranch;
            mModel.To_Name = ctxTFAT.TfatBranch.Where(x => x.Code == fM.ToBranch).Select(x => x.Name).FirstOrDefault();
            mModel.VehicleCategory = fM.VehicleCategory;
            mModel.VehicleCategory_Name = ctxTFAT.VehicleCategory.Where(x => x.Code == mModel.VehicleCategory).Select(x => x.VehicleCategory1).FirstOrDefault();
            mModel.ReceiptNo = fM.ReceiptNo;
            if (fM.VehicleStatus == "100001")
            {
                mModel.DriverName = fM.Driver;
            }
            else
            {
                mModel.DriverName = ctxTFAT.DriverMaster.Where(x => x.Code == fM.Driver).Select(x => x.Name).FirstOrDefault();
            }
            mModel.LicenceNo = fM.LicenCeNo;
            mModel.LicenceExpDate = Convert.ToDateTime(fM.LicenceExpDate).ToShortDateString();
            mModel.Owner = fM.OwnerName;
            mModel.ChallanNo = fM.ChallanNo;
            mModel.ContactNo = fM.ContactNo;
            mModel.Freight = fM.Freight;
            mModel.Advance = fM.Adv;
            mModel.PayableAt = fM.PayAt;
            mModel.PayableAt_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.PayableAt).Select(x => x.Name).FirstOrDefault();
            mModel.Remark = fM.Remark;
            mModel.Balance = fM.Balance;
            mModel.AppBranch = fM.RouteVia;
            mModel.PayLoad = fM.PayLoad.ToString();
            mModel.Parentkey = fM.TableKey.ToString();

            #endregion

            #region GetAll RouteVia

            if (!(String.IsNullOrEmpty(fM.RouteVia)))
            {
                List<LR_LC_Combine_VM> lR_LC_Combine_VMs = new List<LR_LC_Combine_VM>();
                var GetSourceArry = fM.RouteVia.Split(',');
                for (int i = 0; i < GetSourceArry.Length; i++)
                {
                    var SourceCode = GetSourceArry[i];
                    var SourceName = ctxTFAT.TfatBranch.Where(x => x.Code == SourceCode).Select(x => x.Name).FirstOrDefault();

                    LR_LC_Combine_VM lR_LC_Combine_VM = new LR_LC_Combine_VM
                    {
                        Consigner = SourceName,
                        From = SourceCode,
                    };
                    lR_LC_Combine_VMs.Add(lR_LC_Combine_VM);
                }
                TempData["Destination"] = lR_LC_Combine_VMs;
                mModel.AllDestList = lR_LC_Combine_VMs;
            }

            #endregion

            #region Schedule
            List<FMROUTETable> FMROUTETables = ctxTFAT.FMROUTETable.Where(x => x.FmNo == fM.FmNo).ToList();
            List<RouteDetails> routeDetails = new List<RouteDetails>();
            foreach (var item in FMROUTETables)
            {
                RouteDetails routeDetail = new RouteDetails
                {
                    Branch = ctxTFAT.TfatBranch.Where(x => x.Code == item.RouteVia).Select(x => x.Name).FirstOrDefault(),
                    ArrivalSchDate = item.ArrivalSchDate == null ? "" : item.ArrivalSchDate.Value.ToShortDateString(),
                    ArrivalSchTime = item.ArrivalSchTime ?? "",
                    ArrivalSchKm = item.ArrivalSchKm == null ? "" : item.ArrivalSchKm.Value.ToString(),
                    DispatchSchDate = item.DispatchSchDate == null ? "" : item.DispatchSchDate.Value.ToShortDateString(),
                    DispatchSchTime = item.DispatchSchTime ?? ""
                };
                routeDetails.Add(routeDetail);
            }

            mModel.ViewSchedule = routeDetails;
            #endregion

            #region Route Details

            List<FMROUTETable> fM_s = ctxTFAT.FMROUTETable.Where(x => x.SubRoute == fM_ROUTE.SubRoute && x.FmNo == fM_ROUTE.FmNo).ToList();
            List<ArrivalVM> arrivalVMs = new List<ArrivalVM>();
            List<DispatchVM> dispatchVMs = new List<DispatchVM>();

            foreach (var item in fM_s)
            {
                TfatBranch branch = ctxTFAT.TfatBranch.Where(x => x.Code == item.RouteVia).FirstOrDefault();
                ArrivalVM arrivalVM = new ArrivalVM();
                DispatchVM dispatchVM = new DispatchVM();
                dispatchVM.RecordKey = item.RECORDKEY.ToString();
                dispatchVM.AreaNameOf_A_D = branch.Category == "Branch" ? branch.Name + " - B" : branch.Category == "SubBranch" ? branch.Name + " - SB" : branch.Name + " - A";
                dispatchVM.DispatchDate = item == null ? null : item.DispatchDate == null ? null : item.DispatchDate.Value.ToShortDateString();
                dispatchVM.DispatchTime = item == null ? null : item.DispatchTime == null ? null : item.DispatchTime;
                dispatchVM.DispachKM = item == null ? "0" : item.DispatchKM == null ? "0" : item.DispatchKM.Value.ToString();
                dispatchVM.DispachRemark = item == null ? "" : item.DispatchRemark == null ? "" : item.DispatchRemark;
                arrivalVM.RecordKey = item.RECORDKEY.ToString();
                arrivalVM.AreaNameOf_A_D = branch.Category == "Branch" ? branch.Name + " - B" : branch.Category == "SubBranch" ? branch.Name + " - SB" : branch.Name + " - A";
                arrivalVM.ArrivalDate = item == null ? null : item.ArrivalDate == null ? null : item.ArrivalDate.Value.ToShortDateString();
                arrivalVM.ArrivalTime = item == null ? null : item.ArrivalTime == null ? null : item.ArrivalTime;
                arrivalVM.ArrivalKM = item == null ? "0" : item.ArrivalKM == null ? "0" : item.ArrivalKM.Value.ToString();
                arrivalVM.ArrivalRemark = item == null ? "" : item.ArrivalRemark == null ? "" : item.ArrivalRemark;
                arrivalVMs.Add(arrivalVM);
                dispatchVMs.Add(dispatchVM);
            }

            mModel.ArrivalList = arrivalVMs;
            mModel.DispatchList = dispatchVMs;


            #endregion

            #region Loading And Unloading Details

            List<LCModal> lCs = new List<LCModal>();
            lCs = GetAlreadyLoadedeLCRoute(mdocument);
            TempData["ExistLC"] = lCs;
            mModel.ExistLClist = lCs;
            mModel.Loaded = lCs.Sum(x => x.Weight).ToString();

            var AlltransactionData = ctxTFAT.LRStock.Where(x => x.Fmno == fM.FmNo).Select(x => new { x.ActWeight, x.AllocatBalWght }).ToList();
            var TtotalLoadWeight = AlltransactionData.Sum(x => x.ActWeight);
            var TtotalUnLoadWeight = AlltransactionData.Sum(x => x.AllocatBalWght);

            var AvalWeight = Convert.ToInt32(mModel.PayLoad) - Convert.ToInt32(TtotalLoadWeight);
            AvalWeight += Convert.ToInt32(TtotalLoadWeight) - Convert.ToInt32(TtotalUnLoadWeight);
            mModel.AvailablePayload = (AvalWeight).ToString();

            var UnloadMaterialList = GetUnLoadLClist(fM, "");

            List<LRModal> ldd = new List<LRModal>();
            ldd = AlreadyUnloadLRList(fM);
            mModel.UnloadLrList = ldd;

            #endregion

            #region Check Material For Loading And Unloading Available Or Not

            if (UnloadMaterialList.Count() > 0)//Set Flag Of Unloading Material
            {
                mModel.MaterialAvailableForUnLoading = true;
            }
            else
            {
                mModel.MaterialAvailableForUnLoading = false;
            }

            var PendingLcList = GetPendingLCList(fM.FmNo.ToString());
            if (PendingLcList.Count() > 0)
            {
                mModel.MaterialAvailableForLoading = true;
            }
            else
            {
                mModel.MaterialAvailableForLoading = false;
            }

            #endregion

            #region Deliveries
            List<VehicleDelivery> DeliveryListList = new List<VehicleDelivery>();
            if (!String.IsNullOrEmpty(fM_ROUTE.Deliveries))
            {
                var DelTablekey = fM_ROUTE.Deliveries.Split(',').ToList();
                var DeliveryMaster = ctxTFAT.DeliveryMaster.Where(x => DelTablekey.Contains(x.TableKey)).ToList();
                var DeliveryNos = DeliveryMaster.Select(x => x.DeliveryNo).ToList();
                var DelRelations = ctxTFAT.DelRelation.Where(x => DeliveryNos.Contains(x.DeliveryNo)).ToList();
                foreach (var item in DeliveryMaster)
                {
                    DelRelation delRelation = ctxTFAT.DelRelation.Where(x => x.DeliveryNo == item.DeliveryNo && x.Prefix == item.Prefix).FirstOrDefault();
                    LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey == delRelation.ParentKey).FirstOrDefault();
                    LCMaster lCMaster = ctxTFAT.LCMaster.Where(x => x.TableKey == lRStock.LCRefTablekey).FirstOrDefault();
                    LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey == lRStock.LRRefTablekey).FirstOrDefault();
                    VehicleDelivery vehicleDelivery = new VehicleDelivery();
                    vehicleDelivery.DeliveryNo = item.DeliveryNo.ToString();
                    vehicleDelivery.DelTableyKey = item.TableKey.ToString();
                    vehicleDelivery.DelDate = item.DeliveryDate.ToShortDateString().ToString();
                    vehicleDelivery.ShortQty = item.ShortQty.Value;
                    vehicleDelivery.DeliveredQty = delRelation.DelQty;
                    vehicleDelivery.DelVehicleNo = item.VehicleNO;
                    vehicleDelivery.DelStatus = item.DeliveryGoodStatus;
                    vehicleDelivery.DelNarr = item.DeliveryRemark;
                    vehicleDelivery.Lcno = lCMaster == null ? "0" : lCMaster.LCno.ToString();
                    vehicleDelivery.LcFrom = lCMaster == null ? "" : ctxTFAT.TfatBranch.Where(x => x.Code == lCMaster.FromBranch).Select(x => x.Name).FirstOrDefault();
                    vehicleDelivery.LCTo = lCMaster == null ? "" : ctxTFAT.TfatBranch.Where(x => x.Code == lCMaster.ToBranch).Select(x => x.Name).FirstOrDefault();
                    vehicleDelivery.Lrno = item.LrNO;
                    vehicleDelivery.From = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Source).Select(x => x.Name).FirstOrDefault();
                    vehicleDelivery.To = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name).FirstOrDefault();

                    DeliveryListList.Add(vehicleDelivery);
                }
            }
            mModel.vehicleDeliveries = DeliveryListList;
            #endregion

            #region Vehicle Material History

            var VehicleRouteList = ctxTFAT.FMROUTETable.Where(x => x.Parentkey == fM_ROUTE.Parentkey).OrderBy(x => x.RECORDKEY).ToList();

            foreach (var item in VehicleRouteList)
            {
                if (!String.IsNullOrEmpty(item.LODRefTablekey))
                {
                    var LoadKey = item.LODRefTablekey.Split(',').ToList();
                    foreach (var lo in LoadKey)
                    {
                        var LCmasterDetails = ctxTFAT.LCMaster.Where(x => x.TableKey == lo).FirstOrDefault();
                        var LoadingDetails = ctxTFAT.LCDetail.Where(x => x.LCRefTablekey == lo).ToList();
                        foreach (var lCDetail in LoadingDetails)
                        {
                            ConsignmentTracking tracking = new ConsignmentTracking();
                            tracking.LRRefTablekey = lCDetail.LRRefTablekey.ToString();
                            tracking.LCRefTablekey = lCDetail.LCRefTablekey.ToString();
                            tracking.FMRefTablekey = fM_ROUTE.Parentkey.ToString();
                            tracking.ConsignNo = lCDetail.LRno.ToString();
                            tracking.ConsignDate = ctxTFAT.LRMaster.Where(x => x.TableKey == lCDetail.LRRefTablekey).Select(x => x.BookDate).FirstOrDefault().ToShortDateString();
                            tracking.ConsignNoQty = lCDetail.LrQty;
                            tracking.ConsignNoWeight = lCDetail.LRActWeight;
                            tracking.ConsignNoFrom = ctxTFAT.TfatBranch.Where(x => x.Code == lCDetail.FromBranch).Select(x => x.Name).FirstOrDefault();
                            tracking.ConsignNoTo = ctxTFAT.TfatBranch.Where(x => x.Code == lCDetail.ToBranch).Select(x => x.Name).FirstOrDefault();
                            tracking.ConsignLoadBranch = ctxTFAT.TfatBranch.Where(x => x.Code == LCmasterDetails.Branch).Select(x => x.Name).FirstOrDefault();
                            tracking.ConsignLoadDate = LCmasterDetails.LoadDate.Value.ToShortDateString();
                            tracking.trackingUnloads = new List<ConsignmentTrackingUnload>();
                            tracking.trackingDeliveries = new List<ConsignmentTrackingDelivery>();
                            consignments.Add(tracking);
                        }
                    }
                }
            }

            foreach (var item in consignments)
            {
                List<ConsignmentTrackingDelivery> trackingDeliveries = new List<ConsignmentTrackingDelivery>();
                List<ConsignmentTrackingUnload> trackingUnloads = new List<ConsignmentTrackingUnload>();

                var GetDelivery = ctxTFAT.LRStock.Where(x => x.LRRefTablekey == item.LRRefTablekey && x.Type == "DEL").ToList();
                foreach (var del in GetDelivery)
                {
                    var DeliveryMasterDetails = ctxTFAT.DeliveryMaster.Where(x => x.TableKey == del.TableKey).FirstOrDefault();
                    if (DeliveryMasterDetails != null)
                    {
                        ConsignmentTrackingDelivery trackingDelivery = new ConsignmentTrackingDelivery();
                        trackingDelivery.DeliveryBranch = ctxTFAT.TfatBranch.Where(x => x.Code == DeliveryMasterDetails.Branch).Select(x => x.Name).FirstOrDefault();
                        trackingDelivery.DeliveryDate = DeliveryMasterDetails.DeliveryDate.ToShortDateString();
                        trackingDelivery.DeliveryQty = DeliveryMasterDetails.Qty;
                        trackingDelivery.DeliveryWeight = DeliveryMasterDetails.Weight;
                        trackingDeliveries.Add(trackingDelivery);
                    }

                }

                var GetUnload = ctxTFAT.UnLoadDetails.Where(x => x.LRRefTablekey == item.LRRefTablekey && x.LCRefTablekey == item.LCRefTablekey && x.FMRefTablekey == item.FMRefTablekey).ToList();
                foreach (var del in GetUnload)
                {
                    ConsignmentTrackingUnload trackingDelivery = new ConsignmentTrackingUnload();
                    trackingDelivery.UnloadBranch = ctxTFAT.TfatBranch.Where(x => x.Code == del.Branch).Select(x => x.Name).FirstOrDefault();
                    trackingDelivery.UnloadDate = del.Date.ToShortDateString();
                    trackingDelivery.UnloadQty = del.GQty;
                    trackingDelivery.UnloadWeight = del.Weight;
                    trackingUnloads.Add(trackingDelivery);
                }

                item.trackingDeliveries = trackingDeliveries;
                item.trackingUnloads = trackingUnloads;

                item.ConsignNoBalQty = item.ConsignNoQty - ((trackingDeliveries.Sum(x => x.DeliveryQty)) + (trackingUnloads.Sum(x => x.UnloadQty)));
                item.ConsignNoBalWeight = item.ConsignNoWeight - ((trackingDeliveries.Sum(x => x.DeliveryWeight)) + (trackingUnloads.Sum(x => x.UnloadWeight)));
            }

            mModel.consignmentTrackings = consignments;

            #endregion

            #region Unload Other Branch Details

            List<LRModal> OTHldd = new List<LRModal>();
            OTHldd = OTHAlreadyUnloadLRList(fM);
            mModel.OTHUnloadLrList = OTHldd;
            mModel.UnloadBranches = PopulateGeneralArea();

            #endregion Unload Other Branch Details

            #region Direct Loading Consignment Details

            mModel.loadConsignmentList = new List<LoadConsignment>();
            if (!String.IsNullOrEmpty(fM_ROUTE.ConsignmentLoad))
            {
                var AllLoadedStockKeys = fM_ROUTE.ConsignmentLoad.Split(',').ToList();
                var list = ctxTFAT.LRStock.Where(x => AllLoadedStockKeys.Any(key => key == x.ParentKey) && x.FMRefTablekey == fM_ROUTE.Parentkey)
                              .Select(x => new
                              {
                                  Parentkey = x.TableKey,
                                  LRNO = x.LrNo.ToString(),
                                  BookDate = ctxTFAT.LRMaster.Where(y => y.TableKey == x.LRRefTablekey).Select(y => y.BookDate).FirstOrDefault(),
                                  LoadQty = x.LoadForGodown,
                                  LoadWeight = x.ActWeight,
                                  ChgWT = x.ChrgWeight,
                                  Unit = ctxTFAT.UnitMaster.Where(y => y.Code == x.Unit).Select(y => y.Name).FirstOrDefault().ToUpper(),
                                  ChargeType = ctxTFAT.ChargeTypeMaster.Where(y => y.Code == x.ChrgType).Select(y => y.ChargeType).FirstOrDefault().ToUpper(),
                                  From = ctxTFAT.TfatBranch.Where(y => y.Code == x.FromBranch).Select(y => y.Name).FirstOrDefault().ToUpper(),
                                  To = ctxTFAT.TfatBranch.Where(y => y.Code == x.ToBranch).Select(y => y.Name).FirstOrDefault().ToUpper(),
                                  Delivery = x.Delivery == "G" ? "Godown" : "Door",
                                  Collection = x.Coln == "G" ? "Godown" : x.Coln == "C" ? "Crossing" : "Direct",
                                  Consignor = ctxTFAT.Consigner.Where(y => y.Code == x.Consigner).Select(y => y.Name).FirstOrDefault().ToUpper(),
                                  Consignee = ctxTFAT.Consigner.Where(y => y.Code == x.Consignee).Select(y => y.Name).FirstOrDefault().ToUpper(),
                                  LRType = ctxTFAT.LRTypeMaster.Where(y => y.Code == x.LrType).Select(y => y.LRType).FirstOrDefault().ToUpper(),
                                  LRMode = x.LRMode,
                                  ConsignmentDest = ctxTFAT.TfatBranch.Where(y => y.Code == x.Branch).Select(y => y.Name).FirstOrDefault().ToUpper(),
                              })
                              .ToList();

                var LoadList = list.Select(x => new LoadConsignment
                {
                    Parentkey = x.Parentkey,
                    LRNO = x.LRNO,
                    BookDate = x.BookDate.ToShortDateString(),
                    LoadQty = x.LoadQty,
                    LoadWeight = Convert.ToDecimal(x.LoadWeight),
                    ChgWT = Convert.ToDecimal(x.ChgWT),
                    Unit = x.Unit,
                    ChargeType = x.ChargeType,
                    From = x.From,
                    To = x.To,
                    Delivery = x.Delivery,
                    Collection = x.Collection,
                    Consignor = x.Consignor,
                    Consignee = x.Consignee,
                    LRType = x.LRType,
                    LRMode = x.LRMode,
                    ConsignmentDest = x.ConsignmentDest
                }).ToList();
                if (LoadList == null)
                {
                    mModel.loadConsignmentList = new List<LoadConsignment>();
                }
                else
                {
                    mModel.loadConsignmentList = LoadList;
                }
            }
            #endregion Direct Loading Consignment Details

            return View(mModel);
        }

        #region Attachment (Download,View,Delete,Save)

        [HttpPost]
        public ActionResult AttachDocument(FMAttachment model, string DocumentStr, string FileNameStr)
        {
            List<FMAttachment> AttachList = new List<FMAttachment>();
            List<FMAttachment> SessionAttachList = Session["FMAttachmentList"] as List<FMAttachment>;
            if (SessionAttachList != null)
            {
                AttachList = SessionAttachList;
            }
            MemoryStream memory = new MemoryStream();
            if (AttachList.Where(x => x.FileName == model.UploadFile.FileName).FirstOrDefault() == null)
            {
                model.UploadFile.InputStream.CopyTo(memory);
                byte[] bytes = memory.ToArray();

                FMAttachment PersonalDocument = new FMAttachment
                {
                    AttachFMNo = model.UploadFile.FileName.ToString(),
                    DocumentString = Convert.ToBase64String(bytes),
                    ContentType = model.UploadFile.ContentType,
                    FileName = model.UploadFile.FileName,
                    Image = bytes
                };
                AttachList.Add(PersonalDocument);
            }
            Session["FMAttachmentList"] = AttachList;
            var html = ViewHelper.RenderPartialView(this, "_AtachmentListView", new LoadingToDispatchVM { attachments = AttachList });
            var jsonResult = Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        [HttpPost]
        public ActionResult AttachDocumentList()
        {
            List<FMAttachment> AttachList = new List<FMAttachment>();
            List<FMAttachment> SessionAttachList = Session["FMAttachmentList"] as List<FMAttachment>;
            if (SessionAttachList != null)
            {
                AttachList = SessionAttachList;
            }

            Session["FMAttachmentList"] = AttachList;
            var html = ViewHelper.RenderPartialView(this, "_AttachmentList", new LoadingToDispatchVM { attachments = AttachList });
            var jsonResult = Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public FileResult Download(string tempId)
        {
            List<FMAttachment> attachlist = new List<FMAttachment>();
            if (Session["FMAttachmentList"] != null)
            {
                attachlist = Session["FMAttachmentList"] as List<FMAttachment>;
            }

            var dwnfile = attachlist.Where(x => x.AttachFMNo == tempId).FirstOrDefault();

            string filename = dwnfile.FileName;
            byte[] fileBytes = Convert.FromBase64String(dwnfile.DocumentString);

            return File(fileBytes, dwnfile.ContentType, filename);
        }

        [HttpPost]
        public ActionResult Delete(string tempId)
        {
            string message = "False";

            List<FMAttachment> attachlist = new List<FMAttachment>();
            if (Session["FMAttachmentList"] != null)
            {
                attachlist = Session["FMAttachmentList"] as List<FMAttachment>;
            }

            var dwnfile = attachlist.Where(x => x.AttachFMNo == tempId).FirstOrDefault();

            attachlist.Remove(dwnfile);
            message = "True";
            Session["FMAttachmentList"] = attachlist;
            var html = ViewHelper.RenderPartialView(this, "_AtachmentListView", new LoadingToDispatchVM { attachments = attachlist });
            var jsonResult = Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        [HttpPost]
        public ActionResult ViewImage(FMAttachment mModel)
        {
            List<FMAttachment> attachmentDocumentVMs = new List<FMAttachment>();
            attachmentDocumentVMs = Session["FMAttachmentList"] as List<FMAttachment>;
            FMAttachment attachmentDocument = attachmentDocumentVMs.Where(x => x.AttachFMNo == mModel.AttachFMNo).FirstOrDefault();

            byte[] Image;
            byte[] byteArray = Encoding.UTF8.GetBytes(attachmentDocument.DocumentString);
            MemoryStream stream = new MemoryStream(byteArray);
            using (var binaryReader = new BinaryReader(stream))
            {
                Image = binaryReader.ReadBytes(attachmentDocument.DocumentString.Length);
            }
            byte[] fileBinary = Convert.FromBase64String(attachmentDocument.DocumentString);
            attachmentDocument.Image = fileBinary;
            var html = ViewHelper.RenderPartialView(this, "_ImageView", attachmentDocument);
            var jsonResult = Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        #endregion

        #region Arrival->Loading->UnLoading->->Delivery->Dispatch

        #region Arrival
        public ActionResult ArrivalPartial(string Reco)
        {
            FMROUTETable fM_ROUTE = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == Reco).FirstOrDefault();
            LoadingToDispatchVM mModel = new LoadingToDispatchVM();
            List<ArrivalVM> arrivalVMs = new List<ArrivalVM>();
            if (fM_ROUTE != null)
            {
                List<FMROUTETable> fM_s = ctxTFAT.FMROUTETable.Where(x => x.SubRoute == fM_ROUTE.SubRoute && x.FmNo == fM_ROUTE.FmNo).OrderBy(x => x.RECORDKEY).ToList();
                foreach (var item in fM_s)
                {
                    TfatBranch branch = ctxTFAT.TfatBranch.Where(x => x.Code == item.RouteVia).FirstOrDefault();
                    ArrivalVM arrivalVM = new ArrivalVM();
                    arrivalVM.RecordKey = item.RECORDKEY.ToString();
                    arrivalVM.AreaNameOf_A_D = branch.Category == "Branch" ? branch.Name + " - B" : branch.Category == "SubBranch" ? branch.Name + " - SB" : branch.Name + " - A";
                    arrivalVM.ArrivalDate = item == null ? null : item.ArrivalDate == null ? null : item.ArrivalDate.Value.ToShortDateString();
                    arrivalVM.ArrivalTime = item == null ? null : item.ArrivalTime == null ? null : item.ArrivalTime;
                    arrivalVM.ArrivalKM = item == null ? "0" : item.ArrivalKM == null ? "0" : item.ArrivalKM.Value.ToString();
                    arrivalVM.ArrivalRemark = item == null ? "" : item.ArrivalRemark == null ? "" : item.ArrivalRemark;
                    arrivalVMs.Add(arrivalVM);
                }

                mModel.ArrivalList = arrivalVMs;
            }
            var html = ViewHelper.RenderPartialView(this, "_ArrivalPartialView", mModel);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Loading

        public List<LCModal> GetAlreadyLoadedeLCRoute(string Reco)//Already Loaded Lc In Current Route
        {
            List<LCModal> lCModals = new List<LCModal>();
            FMROUTETable FMROUTETable = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == Reco).FirstOrDefault();
            if (FMROUTETable != null)
            {
                if (!String.IsNullOrEmpty(FMROUTETable.LODRefTablekey))
                {
                    var Lclist = FMROUTETable.LODRefTablekey.Split(',');
                    foreach (var lcno in Lclist)
                    {
                        var Add = false;
                        var LRAlert = false;

                        var LorrychallanStockKey = ctxTFAT.LCDetail.Where(x => x.LCRefTablekey == lcno).Select(x => x.ParentKey).ToList();
                        var BlockLclist = ctxTFAT.LRStock.Where(x => LorrychallanStockKey.Contains(x.ParentKey) && x.StockStatus == "T").Select(x => x.TableKey).ToList();

                        foreach (var item in BlockLclist)
                        {
                            if (Add == false)
                            {
                                if (ctxTFAT.LRStock.Where(x => x.ParentKey == item).ToList().Count() > 0)
                                {
                                    Add = true;
                                }
                            }
                            else
                            {
                                break;
                            }

                        }


                        //if (ctxTFAT.LRStock.Where(x => BlockLclist.Contains(x.ParentKey)).ToList().Count() > 0)
                        //{
                        //    Add = true;
                        //}
                        //if (Add == false)
                        //{
                        //    var LClist = ctxTFAT.LCMaster.Where(x => x.DispachFM == 0).Select(x => x.LCno).ToList();
                        //    if (ctxTFAT.LCDetail.Where(x => BlockLclist.Contains(x.ParentKey) && LClist.Contains(x.LCno)).ToList().Count() > 0)
                        //    {
                        //        Add = true;
                        //    }
                        //}
                        var LRList = ctxTFAT.LCDetail.Where(x => x.LCRefTablekey.ToString() == lcno).Select(x => x.LRRefTablekey.ToString()).ToList();
                        if (LRList != null)
                        {
                            var AlertNOte = ctxTFAT.AlertNoteMaster.Where(x => x.Type == "LR000" && LRList.Contains(x.ParentKey)).FirstOrDefault();
                            if (AlertNOte != null)
                            {
                                LRAlert = true;
                            }
                        }
                        List<LRModal> lRModals = new List<LRModal>();
                        var GetLCDetail = ctxTFAT.LCMaster.Where(x => x.TableKey.ToString() == lcno).FirstOrDefault();
                        if (GetLCDetail != null)
                        {
                            LCModal lCModal = new LCModal
                            {
                                Date = GetLCDetail.Date.ToShortDateString(),
                                Time = GetLCDetail.Time,
                                lcno = GetLCDetail.LCno.ToString(),
                                TotalQty = GetLCDetail.TotalQty,
                                From = GetLCDetail.FromBranch,
                                From_Name = ctxTFAT.TfatBranch.Where(x => x.Code == GetLCDetail.FromBranch).Select(x => x.Name).FirstOrDefault(),
                                To = GetLCDetail.ToBranch,
                                To_Name = ctxTFAT.TfatBranch.Where(x => x.Code == GetLCDetail.ToBranch).Select(x => x.Name).FirstOrDefault(),
                                Weight = 0,
                                EnableDeleteOrNot = Add,
                                IMG = true,
                                LRAlertNote = LRAlert,
                                Tablekey = GetLCDetail.TableKey,
                                LoadDate = GetLCDetail.LoadDate == null ? "" : GetLCDetail.LoadDate.Value.ToShortDateString(),
                                LoadTime = GetLCDetail.LoadTime == null ? "" : GetLCDetail.LoadTime
                            };
                            var GetCurrentLrListOfLC = ctxTFAT.LCDetail.Where(x => x.LCRefTablekey.ToString() == lcno).ToList();
                            foreach (var item1 in GetCurrentLrListOfLC)
                            {
                                var GetLRDetail = item1;
                                LRModal lRModal = new LRModal
                                {
                                    recordkey = lcno.ToString(),
                                    Lcno = item1.LCno.ToString(),
                                    Date = GetLRDetail.Date.ToShortDateString(),
                                    Time = GetLRDetail.Time,
                                    Lrno = GetLRDetail.LRno,
                                    Qty = GetLRDetail.Qty,
                                    From = GetLRDetail.FromBranch,
                                    From_Name = ctxTFAT.TfatBranch.Where(x => x.Code == GetLRDetail.FromBranch).Select(x => x.Name).FirstOrDefault(),
                                    To = GetLRDetail.ToBranch,
                                    To_Name = ctxTFAT.TfatBranch.Where(x => x.Code == GetLRDetail.ToBranch).Select(x => x.Name).FirstOrDefault(),
                                    Weight = GetLRDetail.LRActWeight,
                                    ChargeType = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == GetLRDetail.ChrgeType).Select(x => x.ChargeType).FirstOrDefault(),
                                    PorductType = ctxTFAT.UnitMaster.Where(x => x.Code == GetLRDetail.Unit).Select(x => x.Name).FirstOrDefault()
                                };
                                lRModals.Add(lRModal);
                            }
                            lCModal.Weight = lRModals.Select(x => x.Weight).ToList().Sum();
                            lCModal.LrListOfLC = lRModals;
                            lCModals.Add(lCModal);
                        }
                    }

                }


            }
            TempData["ExistLC"] = lCModals;
            return lCModals;
        }

        public ActionResult LoadedAllLCPartialView(string Reco)
        {

            LoadingDispachVM mModel = new LoadingDispachVM();
            List<LCModal> lCModals = new List<LCModal>();
            FMROUTETable FMROUTETable = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == Reco).FirstOrDefault();
            if (FMROUTETable != null)
            {
                var GetRoute = ctxTFAT.FMROUTETable.Where(x => x.Parentkey == FMROUTETable.Parentkey && x.LCNO != null).ToList();
                if (GetRoute.Count() > 0)
                {
                    foreach (var item in GetRoute)
                    {
                        var Lclist = item.LODRefTablekey.Split(',');
                        foreach (var lcno in Lclist)
                        {
                            var Add = false;
                            var BlockLclist = ctxTFAT.LRStock.Where(x => x.LCRefTablekey.ToString() == lcno && x.StockStatus == "T").ToList();
                            if (BlockLclist.Sum(x => x.TotalQty) != BlockLclist.Sum(x => x.AllocatBalQty))
                            {
                                Add = true;
                            }
                            List<LRModal> lRModals = new List<LRModal>();
                            var GetLCDetail = ctxTFAT.LCMaster.Where(x => x.TableKey.ToString() == lcno).FirstOrDefault();
                            if (GetLCDetail != null)
                            {
                                LCModal lCModal = new LCModal
                                {
                                    Tablekey = GetLCDetail.TableKey.ToString(),
                                    Date = GetLCDetail.Date.ToShortDateString(),
                                    Time = GetLCDetail.Time,
                                    lcno = GetLCDetail.LCno.ToString(),
                                    TotalQty = GetLCDetail.TotalQty,
                                    From = GetLCDetail.FromBranch,
                                    From_Name = ctxTFAT.TfatBranch.Where(x => x.Code == GetLCDetail.FromBranch).Select(x => x.Name).FirstOrDefault(),
                                    To = GetLCDetail.ToBranch,
                                    To_Name = ctxTFAT.TfatBranch.Where(x => x.Code == GetLCDetail.ToBranch).Select(x => x.Name).FirstOrDefault(),
                                    Weight = 0,
                                    EnableDeleteOrNot = Add,
                                    IMG = true,
                                    Branch = ctxTFAT.TfatBranch.Where(x => x.Code == item.Parent).Select(x => x.Name).FirstOrDefault(),
                                };
                                var GetCurrentLrListOfLC = ctxTFAT.LCDetail.Where(x => x.LCRefTablekey.ToString() == lcno).ToList();
                                foreach (var item1 in GetCurrentLrListOfLC)
                                {
                                    var GetLRDetail = item1;
                                    LRModal lRModal = new LRModal
                                    {
                                        recordkey = item1.LCRefTablekey.ToString(),
                                        Lcno = item1.LCno.ToString(),
                                        Date = GetLRDetail.Date.ToShortDateString(),
                                        Time = GetLRDetail.Time,
                                        Lrno = GetLRDetail.LRno,
                                        Qty = GetLRDetail.Qty,
                                        From = GetLRDetail.FromBranch,
                                        From_Name = ctxTFAT.TfatBranch.Where(x => x.Code == GetLRDetail.FromBranch).Select(x => x.Name).FirstOrDefault(),
                                        To = GetLRDetail.ToBranch,
                                        To_Name = ctxTFAT.TfatBranch.Where(x => x.Code == GetLRDetail.ToBranch).Select(x => x.Name).FirstOrDefault(),
                                        Weight = GetLRDetail.LRActWeight,
                                        ChargeType = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == GetLRDetail.ChrgeType).Select(x => x.ChargeType).FirstOrDefault(),
                                        PorductType = ctxTFAT.UnitMaster.Where(x => x.Code == GetLRDetail.Unit).Select(x => x.Name).FirstOrDefault()
                                    };
                                    lRModals.Add(lRModal);
                                }
                                lCModal.Weight = lRModals.Select(x => x.Weight).ToList().Sum();
                                lCModal.LrListOfLC = lRModals;
                                lCModals.Add(lCModal);
                            }
                        }

                    }
                }
            }

            mModel.LClist = lCModals.OrderBy(x => x.lcno).ToList();
            TempData["AllLCLoaded"] = mModel.LClist;
            var html = ViewHelper.RenderPartialView(this, "_GetAllLoadedLCList", mModel);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);

            //var html = ViewHelper.RenderPartialView(this, "_LoadLCGridView", lCs);
            //return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowLrAlreadyLoaded(string Lcno)//Show Lr List Of Current LC
        {
            List<LCModal> ExistLC = TempData.Peek("AllLCLoaded") as List<LCModal>;
            if (ExistLC == null)
            {
                ExistLC = new List<LCModal>();
            }
            var GetLC = ExistLC.Where(x => x.Tablekey == Lcno).FirstOrDefault();

            var html = ViewHelper.RenderPartialView(this, "_ShowLrListOfLC", GetLC.LrListOfLC);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public List<LCModal> GetPendingLCList(string FMNO)//Pending Lc List
        {

            SqlDataAdapter da = new SqlDataAdapter();
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand();
            SqlConnection con = new SqlConnection(connstring);
            cmd = new SqlCommand("dbo.Sp_PendingLc_Load", con)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@CurrBranch", SqlDbType.VarChar).Value = mbranchcode;
            cmd.Parameters.Add("@CurrentFMNO", SqlDbType.VarChar).Value = FMNO;
            cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;
            con.Open();
            da.SelectCommand = cmd;
            da.Fill(dt);
            string mReturnString = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
            con.Close();
            con.Dispose();

            List<DataRow> list = dt.AsEnumerable().ToList();

            var LRlist = new List<LRModal>(dt.Rows.Count);
            foreach (DataRow row in dt.Rows)
            {
                var values = row.ItemArray;
                var category = new LRModal()
                {
                    recordkey = Convert.ToString(values[20]),
                    Lcno = Convert.ToString(values[2]),
                    Date = Convert.ToString(values[9]),
                    Time = Convert.ToString(values[10]),
                    Lrno = Convert.ToInt32(values[11]),
                    Qty = Convert.ToInt32(values[12]),
                    From = Convert.ToString(values[13]),
                    From_Name = Convert.ToString(values[14]),
                    To = Convert.ToString(values[15]),
                    To_Name = Convert.ToString(values[16]),
                    Weight = Convert.ToDouble(values[17]),
                    ChargeType = Convert.ToString(values[18]),
                    PorductType = Convert.ToString(values[19]),
                };
                LRlist.Add(category);
            }

            var PendingLC = LRlist.GroupBy(x => x.recordkey)
                                .Select(x => new LCModal
                                {
                                    Tablekey = x.Key,
                                    Date = list.Where(y => y.ItemArray[20].ToString() == x.Key.ToString()).Select(y => y.ItemArray[0].ToString()).FirstOrDefault(),
                                    Time = list.Where(y => y.ItemArray[20].ToString() == x.Key.ToString()).Select(y => y.ItemArray[1].ToString()).FirstOrDefault(),
                                    lcno = list.Where(y => y.ItemArray[20].ToString() == x.Key.ToString()).Select(y => y.ItemArray[2].ToString()).FirstOrDefault(),
                                    TotalQty = LRlist.Where(y => y.recordkey == x.Key).Sum(y => y.Qty),
                                    From = list.Where(y => y.ItemArray[20].ToString() == x.Key.ToString()).Select(y => y.ItemArray[4].ToString()).FirstOrDefault(),
                                    From_Name = list.Where(y => y.ItemArray[20].ToString() == x.Key.ToString()).Select(y => y.ItemArray[5].ToString()).FirstOrDefault(),
                                    To = list.Where(y => y.ItemArray[20].ToString() == x.Key.ToString()).Select(y => y.ItemArray[6].ToString()).FirstOrDefault(),
                                    To_Name = list.Where(y => y.ItemArray[20].ToString() == x.Key.ToString()).Select(y => y.ItemArray[7].ToString()).FirstOrDefault(),
                                    Weight = 0,
                                    Authorise = list.Where(y => y.ItemArray[20].ToString() == x.Key.ToString()).Select(y => y.ItemArray[8].ToString()).FirstOrDefault(),
                                    LrListOfLC = LRlist.Where(y => y.recordkey == x.Key).ToList()
                                }).ToList();

            foreach (var item in PendingLC)
            {
                bool LRAlert = false;
                var LRList = ctxTFAT.LCDetail.Where(x => x.LCRefTablekey.ToString() == item.Tablekey).Select(x => x.LRRefTablekey.ToString()).ToList();
                if (LRList != null)
                {
                    var AlertNOte = ctxTFAT.AlertNoteMaster.Where(x => x.Type == "LR000" && LRList.Contains(x.ParentKey)).FirstOrDefault();
                    if (AlertNOte != null)
                    {
                        LRAlert = true;
                    }
                }
                item.LRAlertNote = LRAlert;
            }
            TempData["PendingLC"] = PendingLC;
            return PendingLC;
        }

        public ActionResult LoadTabPartialView(string Reco)
        {
            TempData.Remove("PendingLC");
            LoadingDispachVM mModel = new LoadingDispachVM();

            FMROUTETable fM = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == Reco).FirstOrDefault();

            List<LCModal> lCs = new List<LCModal>();
            lCs = GetPendingLCList(fM.FmNo.ToString());

            TempData["PendingLC"] = lCs;

            List<LCModal> ExistLC = TempData.Peek("ExistLC") as List<LCModal>;
            if (ExistLC == null)
            {
                ExistLC = new List<LCModal>();
            }

            if (ExistLC.Count() > 0)
            {
                lCs = lCs.Where(p => !ExistLC.Any(p2 => p2.Tablekey == p.Tablekey)).ToList();
            }

            mModel.LClist = lCs.OrderBy(x => x.lcno).ToList();

            var html = ViewHelper.RenderPartialView(this, "_GetPendingLCList", mModel);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);

            //var html = ViewHelper.RenderPartialView(this, "_LoadLCGridView", lCs);
            //return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadGridView(string LClist)//Loading Partial View
        {
            LoadingDispachVM loadingDispachVM = new LoadingDispachVM();

            List<LCModal> ExistLC = TempData.Peek("ExistLC") as List<LCModal>;
            if (ExistLC == null)
            {
                ExistLC = new List<LCModal>();
            }

            List<LCModal> PendingLC = TempData.Peek("PendingLC") as List<LCModal>;
            if (PendingLC == null)
            {
                PendingLC = new List<LCModal>();
            }

            if (!string.IsNullOrEmpty(LClist))
            {
                var lcNolist = LClist.Split(',').ToList().Distinct();
                var SelectedLC = PendingLC.Where(x => lcNolist.Contains(x.lcno)).ToList();
                foreach (var item in SelectedLC)
                {
                    ExistLC.Add(item);
                }

            }

            loadingDispachVM.LClist = ExistLC.OrderBy(x => x.lcno).ToList();
            TempData["ExistLC"] = ExistLC;
            TempData["PendingLC"] = PendingLC;

            var html = ViewHelper.RenderPartialView(this, "_LoadLCGridView", loadingDispachVM.LClist);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadingRefresh(string Reco)//Loading Partial View
        {
            LoadingDispachVM loadingDispachVM = new LoadingDispachVM();

            FMROUTETable fM = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == Reco).FirstOrDefault();
            List<LCModal> ExistLC = TempData.Peek("ExistLC") as List<LCModal>;
            ExistLC = ExistLC.Select(c => { c.IMG = true; return c; }).ToList();
            loadingDispachVM.LClist = ExistLC.OrderBy(x => x.lcno).ToList();

            var html = ViewHelper.RenderPartialView(this, "_LoadLCGridView", loadingDispachVM.LClist);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowLrLIstOfLC(string Lcno)//Show Lr List Of Current LC
        {
            List<LCModal> PendingLC = TempData.Peek("PendingLC") as List<LCModal>;
            List<LCModal> ExistLC = TempData.Peek("ExistLC") as List<LCModal>;
            List<LCModal> AllLcList = new List<LCModal>();
            if (PendingLC == null)
            {
                PendingLC = new List<LCModal>();
            }
            if (ExistLC == null)
            {
                ExistLC = new List<LCModal>();
            }

            AllLcList.AddRange(ExistLC);
            AllLcList.AddRange(PendingLC);

            var GetLC = AllLcList.Where(x => x.Tablekey == Lcno).FirstOrDefault();

            var html = ViewHelper.RenderPartialView(this, "_ShowLrListOfLC", GetLC.LrListOfLC);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Direct-Loading(Consignment)

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
            TfatSearch tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == "LCMergeData" && x.ColField.Contains("STKBranch")).Select(x => x).FirstOrDefault();
            tfatSearch.IsHidden = false;
            ctxTFAT.Entry(tfatSearch).State = EntityState.Modified;
            ctxTFAT.SaveChanges();
            return GetGridDataColumns(id, "L", "XXXX");
        }

        public ActionResult GetGridData(GridOption Model)
        {
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("Sp_Lc_MergeStock", tfat_conx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@CurrBranch", SqlDbType.VarChar).Value = mbranchcode;
            cmd.Parameters.Add("@TableKetList", SqlDbType.VarChar).Value = "";
            cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters.Add("@mReturnQuery1", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters.Add("@mReturnQuery2", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;
            cmd.Parameters["@mReturnQuery1"].Direction = ParameterDirection.Output;
            cmd.Parameters["@mReturnQuery2"].Direction = ParameterDirection.Output;

            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();

            string mSelectQuery = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
            string mSelectQuery1 = (string)(cmd.Parameters["@mReturnQuery1"].Value ?? "");
            string mSelectQuery2 = (string)(cmd.Parameters["@mReturnQuery2"].Value ?? "");
            tfat_conx.Close();

            return GetGridReport(Model, "M", "", false, 0);
        }
        //Search Stock
        public ActionResult GetGridData1(GridOption Model)
        {
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("Sp_Lc_SearchLRStock", tfat_conx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@CurrBranch", SqlDbType.VarChar).Value = mbranchcode;
            cmd.Parameters.Add("@TableKetList", SqlDbType.VarChar).Value = Model.mVar4;
            cmd.Parameters.Add("@LRno", SqlDbType.VarChar).Value = Model.mVar3;
            cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters.Add("@mReturnQuery1", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters.Add("@mReturnQuery2", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;
            cmd.Parameters["@mReturnQuery1"].Direction = ParameterDirection.Output;
            cmd.Parameters["@mReturnQuery2"].Direction = ParameterDirection.Output;

            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();

            string mSelectQuery = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
            string mSelectQuery1 = (string)(cmd.Parameters["@mReturnQuery1"].Value ?? "");
            string mSelectQuery2 = (string)(cmd.Parameters["@mReturnQuery2"].Value ?? "");
            tfat_conx.Close();
            return GetGridReport(Model, "M", "", false, 0);
        }

        public ActionResult CheckNewRoute(LoadingToDispatchVM mModel)
        {
            bool RoutFlag = false;
            List<string> NewRouteName = new List<string>();
            string NewRoute = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    FMROUTETable Route = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
                    if (Route != null)
                    {
                        FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey == Route.Parentkey).FirstOrDefault();
                        foreach (var item in mModel.loadConsignmentList)
                        {
                            var CheckCurrRoute = false;
                            CheckCurrRoute = CheckNewRouteReqierdOrNot(fMMaster.TableKey, item.ConsignmentDest);
                            if (RoutFlag == false && CheckCurrRoute == true)
                            {
                                RoutFlag = true;
                            }
                            if (CheckCurrRoute && NewRouteName.Where(x => x == (ctxTFAT.TfatBranch.Where(y => y.Code == item.ConsignmentDest).Select(y => y.Name).FirstOrDefault())).FirstOrDefault() == null)
                            {
                                NewRoute += item.ConsignmentDest + ",";
                                NewRouteName.Add(ctxTFAT.TfatBranch.Where(x => x.Code == item.ConsignmentDest).Select(x => x.Name).FirstOrDefault());
                            }
                        }
                    }
                    else
                    {
                        return Json(new { Status = "Error", id = "StateMaster", Message = "Something Wrong...!" }, JsonRequestBehavior.AllowGet);
                    }
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
            return Json(new { Status = "Success", id = "StateMaster", NewRoute = NewRoute, RoutFlag = RoutFlag, NewRouteName = string.Join(",", NewRouteName) }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult DirectLoading(LoadingToDispatchVM mModel)
        {
            bool RoutFlag = false;
            string Message = "", html = "", FreightMemoKey = "", NewRoute = "", NewRouteName = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    FMROUTETable Route = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
                    if (Route != null)
                    {
                        FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey == Route.Parentkey).FirstOrDefault();
                        FreightMemoKey = fMMaster.ParentKey;

                        #region Update Loading In Current Route.

                        var Stockkeys = string.Join(",", mModel.loadConsignmentList.Select(x => x.Parentkey).ToList());
                        if (string.IsNullOrEmpty(Route.ConsignmentLoad))
                        {
                            Route.ConsignmentLoad = Stockkeys;
                        }
                        else
                        {
                            Route.ConsignmentLoad = Route.ConsignmentLoad + "," + Stockkeys;
                        }

                        #endregion

                        #region Reduce Old Stock And Add New Stock

                        var LastTrnasitEntryOfFM = ctxTFAT.LRStock.Where(x => x.FMRefTablekey == Route.Parentkey && x.Type == "TRN").OrderByDescending(x => x.RECORDKEY).Select(x => x.TableKey).FirstOrDefault();
                        var CheckBranchCode = LastTrnasitEntryOfFM == null ? "" : LastTrnasitEntryOfFM.Substring(0, 6);
                        int I = 1;
                        if (ctxTFAT.TfatBranch.Where(x => x.Code == CheckBranchCode).FirstOrDefault() == null)
                        {
                            I = LastTrnasitEntryOfFM == null ? 0 : Convert.ToInt32(LastTrnasitEntryOfFM.Substring(7, 3));
                        }
                        else
                        {
                            I = LastTrnasitEntryOfFM == null ? 0 : Convert.ToInt32(LastTrnasitEntryOfFM.Substring(13, 3));
                        }
                        I = I == 0 ? 1 : (I + 1);
                        foreach (var item in mModel.loadConsignmentList)
                        {
                            var CheckCurrRoute = false;
                            CheckCurrRoute = CheckNewRouteReqierdOrNot(fMMaster.TableKey, item.ConsignmentDest);
                            if (RoutFlag == false && CheckCurrRoute == true)
                            {
                                RoutFlag = true;
                            }
                            if (CheckCurrRoute)
                            {
                                NewRoute += item.ConsignmentDest + ",";
                                NewRouteName += (ctxTFAT.TfatBranch.Where(x => x.Code == item.ConsignmentDest).Select(x => x.Name).FirstOrDefault()) + ",";

                            }

                            #region Decrese Stock
                            LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey.ToString() == item.Parentkey).FirstOrDefault();
                            lRStock.BalQty -= item.LoadQty;
                            lRStock.BalWeight -= Convert.ToDouble(item.LoadWeight);
                            ctxTFAT.Entry(lRStock).State = EntityState.Modified;
                            #endregion

                            #region Transit Entry IN LR Stock
                            LRStock LoadlRStock = new LRStock();
                            LoadlRStock.LoginBranch = mbranchcode;
                            LoadlRStock.Branch = item.ConsignmentDest;
                            LoadlRStock.LrNo = lRStock.LrNo;
                            LoadlRStock.LoadForGodown = item.LoadQty;
                            LoadlRStock.LoadForDirect = 0;
                            LoadlRStock.Date = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                            LoadlRStock.Time = (DateTime.Now.ToString("HH:mm"));
                            LoadlRStock.TotalQty = item.LoadQty;
                            LoadlRStock.AllocatBalQty = item.LoadQty;
                            LoadlRStock.BalQty = item.LoadQty;
                            LoadlRStock.ActWeight = Convert.ToDouble(item.LoadWeight);
                            LoadlRStock.AllocatBalWght = Convert.ToDouble(item.LoadWeight);
                            LoadlRStock.BalWeight = Convert.ToDouble(item.LoadWeight);
                            LoadlRStock.ChrgWeight = lRStock.ChrgWeight;
                            LoadlRStock.ChrgType = lRStock.ChrgType;
                            LoadlRStock.Description = lRStock.Description;
                            LoadlRStock.Unit = lRStock.Unit;
                            LoadlRStock.FromBranch = lRStock.FromBranch;
                            LoadlRStock.ToBranch = lRStock.ToBranch;
                            LoadlRStock.Consigner = lRStock.Consigner;
                            LoadlRStock.Consignee = lRStock.Consignee;
                            LoadlRStock.LrType = lRStock.LrType;
                            LoadlRStock.Coln = lRStock.Coln;
                            LoadlRStock.Delivery = lRStock.Delivery;
                            LoadlRStock.Remark = lRStock.Remark;
                            LoadlRStock.StockAt = fMMaster.TruckNo;
                            LoadlRStock.StockStatus = "T";
                            LoadlRStock.LCNO = 0;
                            LoadlRStock.AUTHIDS = muserid;
                            LoadlRStock.AUTHORISE = mauthorise;
                            LoadlRStock.ENTEREDBY = muserid;
                            LoadlRStock.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                            LoadlRStock.UnloadDirectQty = 0;
                            LoadlRStock.UnloadGodwonQty = 0;
                            LoadlRStock.Fmno = fMMaster.FmNo;
                            LoadlRStock.TableKey = mbranchcode + "TRN00" + mperiod.Substring(0, 2) + I.ToString("D3") + fMMaster.FmNo;
                            LoadlRStock.ParentKey = item.Parentkey;
                            LoadlRStock.Type = "TRN";
                            LoadlRStock.LRMode = lRStock.LRMode;
                            LoadlRStock.Prefix = mperiod;
                            LoadlRStock.FMRefTablekey = fMMaster.TableKey;
                            LoadlRStock.LCRefTablekey = "";
                            LoadlRStock.LRRefTablekey = lRStock.LRRefTablekey;
                            ctxTFAT.LRStock.Add(LoadlRStock);
                            #endregion

                            ++I;
                            Message += item.LRNO + " Loaded Sucessfully\n";
                        }

                        #endregion

                        #region Maintain History Of Loading Consignment

                        var LoadingDetails = mModel.loadConsignmentList.Select(x => new LoadingConsignment
                        {
                            CreateOn = DateTime.Now,
                            Branch = mbranchcode,
                            RouteKey = mModel.Document,
                            StockKey = x.Parentkey,
                            Qty = x.LoadQty,
                            Weight = Convert.ToDouble(x.LoadWeight),
                            AUTHIDS = muserid,
                            AUTHORISE = mauthorise,
                            ENTEREDBY = muserid,
                            LASTUPDATEDATE = System.DateTime.Now,
                        }).ToList();
                        ctxTFAT.LoadingConsignment.AddRange(LoadingDetails);

                        #endregion


                    }
                    else
                    {
                        return Json(new { Status = "Error", id = "StateMaster", Message = "Something Wrong...!" }, JsonRequestBehavior.AllowGet);
                    }

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, "Veh-ACT", "Loading Master", FreightMemoKey, DateTime.Now, 0, "", "Load Consignment :" + string.Join(",", mModel.loadConsignmentList.Select(x => x.LRNO).ToList()), "NA");

                    #region Now Bind Grid Of Loaded Consignment In Current Route

                    var AllLoadedStockKeys = Route.ConsignmentLoad.Split(',').ToList();
                    var list = ctxTFAT.LRStock.Where(x => AllLoadedStockKeys.Any(key => key == x.ParentKey) && x.FMRefTablekey == Route.Parentkey)
                              .Select(x => new
                              {
                                  Parentkey = x.TableKey,
                                  LRNO = x.LrNo.ToString(),
                                  BookDate = ctxTFAT.LRMaster.Where(y => y.TableKey == x.LRRefTablekey).Select(y => y.BookDate).FirstOrDefault(),
                                  LoadQty = x.LoadForGodown,
                                  LoadWeight = x.ActWeight,
                                  ChgWT = x.ChrgWeight,
                                  Unit = ctxTFAT.UnitMaster.Where(y => y.Code == x.Unit).Select(y => y.Name).FirstOrDefault().ToUpper(),
                                  ChargeType = ctxTFAT.ChargeTypeMaster.Where(y => y.Code == x.ChrgType).Select(y => y.ChargeType).FirstOrDefault().ToUpper(),
                                  From = ctxTFAT.TfatBranch.Where(y => y.Code == x.FromBranch).Select(y => y.Name).FirstOrDefault().ToUpper(),
                                  To = ctxTFAT.TfatBranch.Where(y => y.Code == x.ToBranch).Select(y => y.Name).FirstOrDefault().ToUpper(),
                                  Delivery = x.Delivery == "G" ? "Godown" : "Door",
                                  Collection = x.Coln == "G" ? "Godown" : x.Coln == "C" ? "Crossing" : "Direct",
                                  Consignor = ctxTFAT.Consigner.Where(y => y.Code == x.Consigner).Select(y => y.Name).FirstOrDefault().ToUpper(),
                                  Consignee = ctxTFAT.Consigner.Where(y => y.Code == x.Consignee).Select(y => y.Name).FirstOrDefault().ToUpper(),
                                  LRType = ctxTFAT.LRTypeMaster.Where(y => y.Code == x.LrType).Select(y => y.LRType).FirstOrDefault().ToUpper(),
                                  LRMode = x.LRMode,
                                  ConsignmentDest = ctxTFAT.TfatBranch.Where(y => y.Code == x.Branch).Select(y => y.Name).FirstOrDefault().ToUpper(),
                              })
                              .ToList();

                    var LoadList = list.Select(x => new LoadConsignment
                    {
                        Parentkey = x.Parentkey,
                        LRNO = x.LRNO,
                        BookDate = x.BookDate.ToShortDateString(),
                        LoadQty = x.LoadQty,
                        LoadWeight = Convert.ToDecimal(x.LoadWeight),
                        ChgWT = Convert.ToDecimal(x.ChgWT),
                        Unit = x.Unit,
                        ChargeType = x.ChargeType,
                        From = x.From,
                        To = x.To,
                        Delivery = x.Delivery,
                        Collection = x.Collection,
                        Consignor = x.Consignor,
                        Consignee = x.Consignee,
                        LRType = x.LRType,
                        LRMode = x.LRMode,
                        ConsignmentDest = x.ConsignmentDest
                    }).ToList();


                    html = ViewHelper.RenderPartialView(this, "loadLrGridView", LoadList);

                    #endregion
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
            return Json(new { Status = "Success", id = "StateMaster", Message = Message, NewRoute = NewRoute, NewRouteName = NewRouteName, RoutFlag = RoutFlag, Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteLoadedConsignment(LoadingToDispatchVM mModel)
        {
            string Message = "", FreightMemoKey = "", LRNO = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var UseStockDetails = ctxTFAT.LRStock.Where(x => x.ParentKey == mModel.Parentkey).ToList().Count();
                    var Lcdetails = ctxTFAT.LCDetail.Where(x => x.ParentKey == mModel.Parentkey).ToList().Count();
                    if (UseStockDetails > 0)
                    {
                        return Json(new { Status = "Error", id = "StateMaster", Message = "Not Allowed To Delete Stock Already Used It...!" }, JsonRequestBehavior.AllowGet);
                    }
                    else if (Lcdetails > 0)
                    {
                        return Json(new { Status = "Error", id = "StateMaster", Message = "Not Allowed To Delete...\n Stock Used In Lorry Challan...!" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var GetStock = ctxTFAT.LRStock.Where(x => x.TableKey == mModel.Parentkey).FirstOrDefault();
                        if (GetStock != null)
                        {
                            LRNO = GetStock.LrNo.ToString();
                            var GetRoute = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
                            if (GetRoute != null)
                            {
                                FreightMemoKey = GetRoute.Parentkey;

                                #region Remove Loading In Current Route.

                                var Stockkeys = string.Join(",", GetRoute.ConsignmentLoad.Split(',').Where(x => x != GetStock.ParentKey).ToList());
                                GetRoute.ConsignmentLoad = Stockkeys;

                                #endregion

                                #region Increase Old Stock And Remove New Stock

                                #region Increase Stock
                                LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey.ToString() == GetStock.ParentKey).FirstOrDefault();
                                lRStock.BalQty += GetStock.TotalQty;
                                lRStock.BalWeight += GetStock.ActWeight;
                                ctxTFAT.Entry(lRStock).State = EntityState.Modified;
                                #endregion

                                #region Remove New Stock

                                ctxTFAT.LRStock.Remove(GetStock);

                                #endregion

                                #endregion

                                #region  Remove History Of Loading Consignment

                                var History = ctxTFAT.LoadingConsignment.Where(x => x.RouteKey == mModel.Document && x.StockKey == GetStock.ParentKey).FirstOrDefault();
                                if (History != null)
                                {
                                    ctxTFAT.LoadingConsignment.Remove(History);
                                }
                                #endregion
                            }
                            else
                            {
                                return Json(new { Status = "Error", id = "StateMaster", Message = "Something Wrong...!" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            return Json(new { Status = "Error", id = "StateMaster", Message = "Stock Not Found...!" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, "Veh-ACT", "Loading Master", FreightMemoKey, DateTime.Now, 0, "", "Delete Consignment :" + LRNO, "NA");

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
            return Json(new
            {
                Status = "Success",
                id = "StateMaster",
                Message = Message
            }, JsonRequestBehavior.AllowGet);
        }

        private bool CheckNewRouteReqierdOrNot(string Tablekey, string toBranch)
        {
            bool RouteRequire = false;
            string Parent = mbranchcode;
            TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == toBranch).FirstOrDefault();
            if (tfatBranch.Category == "Area")
            {
                if (tfatBranch.Grp != "G00000")
                {
                    RouteRequire = true;
                    Parent = tfatBranch.Grp;
                }
            }
            else
            {
                RouteRequire = true;
                Parent = tfatBranch.Code;

            }
            if (RouteRequire)
            {
                var ExistingRout = ctxTFAT.FMROUTETable.Where(x => x.Parentkey == Tablekey && x.RouteType == "R").Select(x => x.Parent).ToList();
                if (!(ExistingRout.Contains(Parent)))
                {
                    RouteRequire = true;
                }
                else
                {
                    RouteRequire = false;
                }
            }

            return RouteRequire;
        }

        #endregion

        #region UnLoading

        public List<LRModal> AlreadyUnloadLRList(FMMaster fMMaster)//Already Unloded Lr List In Current Route
        {
            String CurrBranch = "'" + mbranchcode + "'";
            String CurrFM = "'" + fMMaster.TableKey + "'";
            string connstring = GetConnectionString();
            SqlDataAdapter da = new SqlDataAdapter();
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand();
            SqlConnection con = new SqlConnection(connstring);
            cmd = new SqlCommand("dbo.Sp_AlreadyUnlodedLr_In_CurrBranhch_Of_FM", con)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@CurrBranch", SqlDbType.VarChar).Value = CurrBranch;
            cmd.Parameters.Add("@CurrentFMNO", SqlDbType.VarChar).Value = CurrFM;
            cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;
            con.Open();
            da.SelectCommand = cmd;
            da.Fill(dt);
            string mReturnString = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
            con.Close();
            con.Dispose();

            var mobj = ForEachMethod(dt);
            TempData["AlreadyUnloadLrList"] = mobj;


            return mobj;
        }

        public List<LRModal> OTHAlreadyUnloadLRList(FMMaster fMMaster)//Already Unloded Lr List In Current Route
        {
            String CurrBranch = "'" + mbranchcode + "'";
            String CurrFM = "'" + fMMaster.TableKey + "'";
            string connstring = GetConnectionString();
            SqlDataAdapter da = new SqlDataAdapter();
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand();
            SqlConnection con = new SqlConnection(connstring);
            cmd = new SqlCommand("dbo.Sp_OTHAlreadyUnlodedLr_In_CurrBranhch_Of_FM", con)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@CurrBranch", SqlDbType.VarChar).Value = CurrBranch;
            cmd.Parameters.Add("@CurrentFMNO", SqlDbType.VarChar).Value = CurrFM;
            cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;
            con.Open();
            da.SelectCommand = cmd;
            da.Fill(dt);
            string mReturnString = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
            con.Close();
            con.Dispose();

            var mobj = ForEachMethod(dt);
            TempData["OTHAlreadyUnloadLrList"] = mobj;


            return mobj;
        }

        public List<LRModal> GetUnLoadLClist(FMMaster fMMaster, string Branch)//Jo Current Route Ke Liye Unload Available He Wo LR List Return Krta he
        {
            String CurrBranch = "'" + mbranchcode + "'";
            String CurrFM = "'" + fMMaster.TableKey + "'";
            string connstring = GetConnectionString();
            SqlDataAdapter da = new SqlDataAdapter();
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand();
            SqlConnection con = new SqlConnection(connstring);
            cmd = new SqlCommand("dbo.Sp_UnloadCurrentBranchStock", con)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@CurrBranch", SqlDbType.VarChar).Value = CurrBranch;
            cmd.Parameters.Add("@CurrentFMNO", SqlDbType.VarChar).Value = CurrFM;
            cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;
            con.Open();
            da.SelectCommand = cmd;
            da.Fill(dt);
            string mReturnString = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
            con.Close();
            con.Dispose();

            List<LRModal> mobj = new List<LRModal>();
            if (String.IsNullOrEmpty(Branch))
            {
                mobj = ForEachMethod(dt);
            }
            else
            {
                mobj = ForEachMethod(dt, Branch);
            }


            mobj = mobj.Where(x => x.Qty > 0).ToList();
            return mobj;
        }

        public List<LRModal> ForEachMethod(DataTable table)
        {
            bool BranchColumnCheck = false;
            if (table.Columns.Contains("Branch"))
            {
                BranchColumnCheck = true;
            }
            var convertedList = (from rw in table.AsEnumerable()
                                 select new LRModal()
                                 {
                                     recordkey = Convert.ToString(rw["recordkey"]),
                                     LcFrom = Convert.ToString(rw["LcFrom"]),
                                     LCTo = Convert.ToString(rw["LCTo"]),
                                     Consignor = Convert.ToString(rw["Consignor"]),
                                     Consignee = Convert.ToString(rw["Consignee"]),
                                     Lcno = Convert.ToString(rw["Lcno"]),
                                     Date = Convert.ToString(rw["Date"]),
                                     Time = Convert.ToString(rw["Time"]),
                                     Lrno = Convert.ToInt32(rw["Lrno"]),
                                     Qty = Convert.ToInt32(rw["Qty"]),
                                     From = Convert.ToString(rw["From"]),
                                     From_Name = Convert.ToString(rw["From_Name"]),
                                     To = Convert.ToString(rw["To"]),
                                     To_Name = Convert.ToString(rw["To_Name"]),
                                     Weight = Convert.ToDouble(rw["Weight"]),
                                     ChargeType = Convert.ToString(rw["ChargeType"]),
                                     PorductType = Convert.ToString(rw["PorductType"]),
                                     Delivery = Convert.ToString(rw["Delivery"]),
                                     unloadGQty = Convert.ToInt32(rw["unloadGQty"]),
                                     unloadDQty = Convert.ToInt32(rw["unloadDQty"]),
                                     loadGQty = Convert.ToInt32(rw["loadGQty"]),
                                     loadDQty = Convert.ToInt32(rw["loadDQty"]),
                                     UnWeight = Convert.ToDouble(rw["UnWeight"]),
                                     OTHBranch = BranchColumnCheck == true ? Convert.ToString(rw["Branch"]) : "",

                                 }).ToList();

            if (BranchColumnCheck)
            {
                convertedList.ForEach(x => x.OTHBranchName = (ctxTFAT.TfatBranch.Where(y => y.Code == x.OTHBranch).Select(y => y.Name).FirstOrDefault()));
            }
            return convertedList;
        }

        public List<LRModal> ForEachMethod(DataTable table, string Branch)
        {
            var convertedList = (from rw in table.AsEnumerable()
                                 select new LRModal()
                                 {
                                     recordkey = Convert.ToString(rw["recordkey"]),
                                     LcFrom = Convert.ToString(rw["LcFrom"]),
                                     LCTo = Convert.ToString(rw["LCTo"]),
                                     Consignor = Convert.ToString(rw["Consignor"]),
                                     Consignee = Convert.ToString(rw["Consignee"]),
                                     Lcno = Convert.ToString(rw["Lcno"]),
                                     Date = Convert.ToString(rw["Date"]),
                                     Time = Convert.ToString(rw["Time"]),
                                     Lrno = Convert.ToInt32(rw["Lrno"]),
                                     Qty = Convert.ToInt32(rw["Qty"]),
                                     From = Convert.ToString(rw["From"]),
                                     From_Name = Convert.ToString(rw["From_Name"]),
                                     To = Convert.ToString(rw["To"]),
                                     To_Name = Convert.ToString(rw["To_Name"]),
                                     Weight = Convert.ToDouble(rw["Weight"]),
                                     ChargeType = Convert.ToString(rw["ChargeType"]),
                                     PorductType = Convert.ToString(rw["PorductType"]),
                                     Delivery = Convert.ToString(rw["Delivery"]),
                                     unloadGQty = Convert.ToInt32(rw["unloadGQty"]),
                                     unloadDQty = Convert.ToInt32(rw["unloadDQty"]),
                                     loadGQty = Convert.ToInt32(rw["loadGQty"]),
                                     loadDQty = Convert.ToInt32(rw["loadDQty"]),
                                     UnWeight = Convert.ToDouble(rw["UnWeight"]),
                                     OTHBranch = Branch
                                 }).ToList();
            return convertedList;
        }

        public List<LRModal> GetUnLoadLClistOfAllMaterial(FMMaster fMMaster, string Branch)//Get All LR List In FM
        {
            String CurrFM = "'" + fMMaster.TableKey + "'";
            string connstring = GetConnectionString();
            SqlDataAdapter da = new SqlDataAdapter();
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand();
            SqlConnection con = new SqlConnection(connstring);
            cmd = new SqlCommand("dbo.Sp_UnloadAllMaterialOfFM", con)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@CurrentFMNO", SqlDbType.VarChar).Value = CurrFM;
            cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;
            con.Open();
            da.SelectCommand = cmd;
            da.Fill(dt);
            string mReturnString = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
            con.Close();
            con.Dispose();

            List<LRModal> mobj = new List<LRModal>();
            if (String.IsNullOrEmpty(Branch))
            {
                mobj = ForEachMethod(dt);
            }
            else
            {
                mobj = ForEachMethod(dt, Branch);
            }


            mobj = mobj.Where(x => x.Qty > 0).ToList();

            return mobj;
        }

        public ActionResult UnloadTabPartialView(string Fmno)
        {
            TempData.Remove("UnloadLcList");

            FMROUTETable fMROUTETable = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == Fmno).FirstOrDefault();

            FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == fMROUTETable.Parentkey).FirstOrDefault();
            List<LRModal> UnloadList = new List<LRModal>();

            UnloadList = GetUnLoadLClist(fMMaster, "");
            TempData["UnloadLcList"] = UnloadList;

            List<LRModal> AlreadyUnloadLrList = TempData.Peek("AlreadyUnloadLrList") as List<LRModal>;
            if (AlreadyUnloadLrList == null)
            {
                AlreadyUnloadLrList = new List<LRModal>();
            }
            if (AlreadyUnloadLrList.Count() > 0)
            {
                UnloadList = UnloadList.Where(p => !AlreadyUnloadLrList.Any(p2 => p2.recordkey == p.recordkey)).ToList();
            }

            AlreadyUnloadLrList = TempData.Peek("OTHAlreadyUnloadLrList") as List<LRModal>;
            if (AlreadyUnloadLrList == null)
            {
                AlreadyUnloadLrList = new List<LRModal>();
            }

            if (AlreadyUnloadLrList.Count() > 0)
            {
                UnloadList = UnloadList.Where(p => !AlreadyUnloadLrList.Any(p2 => p2.recordkey == p.recordkey)).ToList();
            }
            var html = ViewHelper.RenderPartialView(this, "_ReadyForUnloadingLrList", UnloadList);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult OTHUnloadTabPartialView(string Fmno, string Branch)
        {
            TempData.Remove("UnloadLcList");

            FMROUTETable fMROUTETable = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == Fmno).FirstOrDefault();

            FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == fMROUTETable.Parentkey).FirstOrDefault();
            List<LRModal> UnloadList = new List<LRModal>();

            UnloadList = GetUnLoadLClist(fMMaster, Branch);
            TempData["UnloadLcList"] = UnloadList;

            List<LRModal> AlreadyUnloadLrList = TempData.Peek("AlreadyUnloadLrList") as List<LRModal>;
            if (AlreadyUnloadLrList == null)
            {
                AlreadyUnloadLrList = new List<LRModal>();
            }
            if (AlreadyUnloadLrList.Count() > 0)
            {
                UnloadList = UnloadList.Where(p => !AlreadyUnloadLrList.Any(p2 => p2.recordkey == p.recordkey)).ToList();
            }

            AlreadyUnloadLrList = TempData.Peek("OTHAlreadyUnloadLrList") as List<LRModal>;
            if (AlreadyUnloadLrList == null)
            {
                AlreadyUnloadLrList = new List<LRModal>();
            }

            if (AlreadyUnloadLrList.Count() > 0)
            {
                UnloadList = UnloadList.Where(p => !AlreadyUnloadLrList.Any(p2 => p2.recordkey == p.recordkey)).ToList();
            }
            LoadingDispachVM loading = new LoadingDispachVM();
            loading.LRLIst = UnloadList;
            loading.To = Branch;
            loading.To_Name = ctxTFAT.TfatBranch.Where(x => x.Code == Branch).Select(x => x.Name).FirstOrDefault();

            var html = ViewHelper.RenderPartialView(this, "_OTHReadyForUnloadingLrList", loading);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AllMaterialForUnloadPartialView(string Fmno)
        {
            TempData.Remove("UnloadLcList");

            FMROUTETable fMROUTETable = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == Fmno).FirstOrDefault();

            FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == fMROUTETable.Parentkey).FirstOrDefault();
            List<LRModal> UnloadList = new List<LRModal>();

            UnloadList = GetUnLoadLClistOfAllMaterial(fMMaster, "");
            TempData["UnloadLcList"] = UnloadList;

            List<LRModal> AlreadyUnloadLrList = TempData.Peek("AlreadyUnloadLrList") as List<LRModal>;
            if (AlreadyUnloadLrList == null)
            {
                AlreadyUnloadLrList = new List<LRModal>();
            }

            if (AlreadyUnloadLrList.Count() > 0)
            {
                UnloadList = UnloadList.Where(p => !AlreadyUnloadLrList.Any(p2 => p2.recordkey == p.recordkey)).ToList();
            }
            AlreadyUnloadLrList = TempData.Peek("OTHAlreadyUnloadLrList") as List<LRModal>;
            if (AlreadyUnloadLrList == null)
            {
                AlreadyUnloadLrList = new List<LRModal>();
            }

            if (AlreadyUnloadLrList.Count() > 0)
            {
                UnloadList = UnloadList.Where(p => !AlreadyUnloadLrList.Any(p2 => p2.recordkey == p.recordkey)).ToList();
            }


            UnloadList = UnloadList.Select(c => { c.unloadGQty = 0; c.unloadDQty = 0; c.UnWeight = 0; return c; }).ToList();
            var html = ViewHelper.RenderPartialView(this, "_ReadyForUnloadingLrList", UnloadList);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult OTHAllMaterialForUnloadPartialView(string Fmno, string Branch)
        {
            TempData.Remove("UnloadLcList");

            FMROUTETable fMROUTETable = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == Fmno).FirstOrDefault();

            FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == fMROUTETable.Parentkey).FirstOrDefault();
            List<LRModal> UnloadList = new List<LRModal>();

            UnloadList = GetUnLoadLClistOfAllMaterial(fMMaster, Branch);
            TempData["UnloadLcList"] = UnloadList;

            List<LRModal> AlreadyUnloadLrList = TempData.Peek("AlreadyUnloadLrList") as List<LRModal>;
            if (AlreadyUnloadLrList == null)
            {
                AlreadyUnloadLrList = new List<LRModal>();
            }

            if (AlreadyUnloadLrList.Count() > 0)
            {
                UnloadList = UnloadList.Where(p => !AlreadyUnloadLrList.Any(p2 => p2.recordkey == p.recordkey)).ToList();
            }
            AlreadyUnloadLrList = TempData.Peek("OTHAlreadyUnloadLrList") as List<LRModal>;
            if (AlreadyUnloadLrList == null)
            {
                AlreadyUnloadLrList = new List<LRModal>();
            }

            if (AlreadyUnloadLrList.Count() > 0)
            {
                UnloadList = UnloadList.Where(p => !AlreadyUnloadLrList.Any(p2 => p2.recordkey == p.recordkey)).ToList();
            }


            UnloadList = UnloadList.Select(c => { c.unloadGQty = 0; c.unloadDQty = 0; c.UnWeight = 0; return c; }).ToList();

            LoadingDispachVM loading = new LoadingDispachVM();
            loading.LRLIst = UnloadList;
            loading.To = Branch;
            loading.To_Name = ctxTFAT.TfatBranch.Where(x => x.Code == Branch).Select(x => x.Name).FirstOrDefault();



            var html = ViewHelper.RenderPartialView(this, "_OTHReadyForUnloadingLrList", loading);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PendingMaterialView(string Fmno)
        {
            TempData.Remove("UnloadLcList");

            FMROUTETable fMROUTETable = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == Fmno).FirstOrDefault();

            FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == fMROUTETable.Parentkey).FirstOrDefault();
            List<LRModal> UnloadList = new List<LRModal>();

            UnloadList = GetUnLoadLClistOfAllMaterial(fMMaster, "");
            TempData["UnloadLcList"] = UnloadList;

            List<LRModal> AlreadyUnloadLrList = TempData.Peek("AlreadyUnloadLrList") as List<LRModal>;
            if (AlreadyUnloadLrList == null)
            {
                AlreadyUnloadLrList = new List<LRModal>();
            }

            if (AlreadyUnloadLrList.Count() > 0)
            {
                UnloadList = UnloadList.Where(p => !AlreadyUnloadLrList.Any(p2 => p2.recordkey == p.recordkey)).ToList();
            }

            AlreadyUnloadLrList = TempData.Peek("OTHAlreadyUnloadLrList") as List<LRModal>;
            if (AlreadyUnloadLrList == null)
            {
                AlreadyUnloadLrList = new List<LRModal>();
            }

            if (AlreadyUnloadLrList.Count() > 0)
            {
                UnloadList = UnloadList.Where(p => !AlreadyUnloadLrList.Any(p2 => p2.recordkey == p.recordkey)).ToList();
            }



            UnloadList = UnloadList.Select(c => { c.unloadGQty = 0; c.unloadDQty = 0; c.UnWeight = 0; return c; }).ToList();
            var html = ViewHelper.RenderPartialView(this, "PendingMaterial", UnloadList);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PayloadMaterialView(string Fmno)
        {
            TempData.Remove("UnloadLcList");

            FMROUTETable fMROUTETable = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == Fmno).FirstOrDefault();

            FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == fMROUTETable.Parentkey).FirstOrDefault();
            List<LRModal> UnloadList = new List<LRModal>();

            UnloadList = GetUnLoadLClistOfAllMaterial(fMMaster, "");
            TempData["UnloadLcList"] = UnloadList;

            List<LRModal> AlreadyUnloadLrList = TempData.Peek("AlreadyUnloadLrList") as List<LRModal>;
            if (AlreadyUnloadLrList == null)
            {
                AlreadyUnloadLrList = new List<LRModal>();
            }
            if (AlreadyUnloadLrList.Count() > 0)
            {
                UnloadList = UnloadList.Where(p => !AlreadyUnloadLrList.Any(p2 => p2.recordkey == p.recordkey)).ToList();
            }
            AlreadyUnloadLrList = TempData.Peek("OTHAlreadyUnloadLrList") as List<LRModal>;
            if (AlreadyUnloadLrList == null)
            {
                AlreadyUnloadLrList = new List<LRModal>();
            }
            if (AlreadyUnloadLrList.Count() > 0)
            {
                UnloadList = UnloadList.Where(p => !AlreadyUnloadLrList.Any(p2 => p2.recordkey == p.recordkey)).ToList();
            }

            double Payload = fMMaster.PayLoad ?? 0;
            double LoadedWeight = UnloadList.Sum(x => x.Weight);
            double Available = Payload - LoadedWeight;
            return Json(new { LoadedWeight = LoadedWeight, Available = Available }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UnLoadGridView(string RecoList, string GQty, string DQty, string Weight)//UnLoading Partial View
        {
            List<LRModal> AllUnloadLrList = TempData.Peek("UnloadLcList") as List<LRModal>;
            List<LRModal> AlreadyUnloadLrList = TempData.Peek("AlreadyUnloadLrList") as List<LRModal>;
            if (AlreadyUnloadLrList == null)
            {
                AlreadyUnloadLrList = new List<LRModal>();
            }

            var ids = RecoList.Split(',');
            var UnloadGQty = GQty.Split(',');
            var UnloadDQty = DQty.Split(',');
            var UnloadWeight = Weight.Split(',');

            for (int i = 0; i < ids.Length; i++)
            {
                //LRModal lRModal = new LRModal();
                if (AlreadyUnloadLrList.Where(x => x.recordkey.ToString() == ids[i]).FirstOrDefault() == null)
                {
                    var LR = AllUnloadLrList.Where(x => x.recordkey.ToString() == ids[i]).FirstOrDefault();
                    LR.unloadGQty = Convert.ToInt32(UnloadGQty[i]);
                    LR.unloadDQty = Convert.ToInt32(UnloadDQty[i]);
                    LR.UnWeight = Convert.ToDouble(UnloadWeight[i]);
                    //LR.UnloadLRDate = null;
                    //LR.UnloadLRTime = DateTime.Now.ToShortTimeString();
                    AlreadyUnloadLrList.Add(LR);
                }
                else
                {
                    var LR = AlreadyUnloadLrList.Where(x => x.recordkey.ToString() == ids[i]).FirstOrDefault();
                    LR.unloadGQty = Convert.ToInt32(UnloadGQty[i]);
                    LR.unloadDQty = Convert.ToInt32(UnloadDQty[i]);
                    LR.UnWeight = Convert.ToDouble(UnloadWeight[i]);
                }
            }

            TempData["AlreadyUnloadLrList"] = AlreadyUnloadLrList;

            var html = ViewHelper.RenderPartialView(this, "UnloadLrGridView", AlreadyUnloadLrList);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult OTHUnLoadGridView(string RecoList, string GQty, string DQty, string Weight, string Branch)//UnLoading Partial View
        {
            List<LRModal> AllUnloadLrList = TempData.Peek("UnloadLcList") as List<LRModal>;
            List<LRModal> AlreadyUnloadLrList = TempData.Peek("OTHAlreadyUnloadLrList") as List<LRModal>;
            if (AlreadyUnloadLrList == null)
            {
                AlreadyUnloadLrList = new List<LRModal>();
            }

            var ids = RecoList.Split(',');
            var UnloadGQty = GQty.Split(',');
            var UnloadDQty = DQty.Split(',');
            var UnloadWeight = Weight.Split(',');
            var UnlBranch = Branch.Split(',');

            for (int i = 0; i < ids.Length; i++)
            {
                //LRModal lRModal = new LRModal();
                if (AlreadyUnloadLrList.Where(x => x.recordkey.ToString() == ids[i]).FirstOrDefault() == null)
                {
                    var LR = AllUnloadLrList.Where(x => x.recordkey.ToString() == ids[i]).FirstOrDefault();
                    LR.unloadGQty = Convert.ToInt32(UnloadGQty[i]);
                    LR.unloadDQty = Convert.ToInt32(UnloadDQty[i]);
                    LR.UnWeight = Convert.ToDouble(UnloadWeight[i]);
                    LR.OTHBranch = UnlBranch[i];
                    LR.OTHBranchName = ctxTFAT.TfatBranch.Where(x => x.Code == LR.OTHBranch).Select(x => x.Name).FirstOrDefault();
                    //LR.UnloadLRDate = null;
                    //LR.UnloadLRTime = DateTime.Now.ToShortTimeString();
                    AlreadyUnloadLrList.Add(LR);
                }
                else
                {
                    var LR = AlreadyUnloadLrList.Where(x => x.recordkey.ToString() == ids[i]).FirstOrDefault();
                    LR.unloadGQty = Convert.ToInt32(UnloadGQty[i]);
                    LR.unloadDQty = Convert.ToInt32(UnloadDQty[i]);
                    LR.UnWeight = Convert.ToDouble(UnloadWeight[i]);
                    LR.OTHBranch = UnlBranch[i];
                    LR.OTHBranchName = ctxTFAT.TfatBranch.Where(x => x.Code == LR.OTHBranch).Select(x => x.Name).FirstOrDefault();
                }
            }

            TempData["OTHAlreadyUnloadLrList"] = AlreadyUnloadLrList;

            var html = ViewHelper.RenderPartialView(this, "OTHUnloadLrGridView", AlreadyUnloadLrList);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult DeleteUnloadLR(string recordkey)//Delete LR From Unloded List (Only View)
        {
            string Msg = "Sucess";
            List<LRModal> AlreadyUnloadLrList = TempData.Peek("AlreadyUnloadLrList") as List<LRModal>;
            var lr = AlreadyUnloadLrList.Where(x => x.recordkey.ToString() == recordkey).FirstOrDefault();
            lr.unloadDQty = 0;
            lr.unloadGQty = 0;
            lr.UnWeight = 0;
            AlreadyUnloadLrList.Remove(lr);

            TempData["AlreadyUnloadLrList"] = AlreadyUnloadLrList;
            return Json(new { Msg = Msg }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult RefreshUnLoadGridView()
        {
            List<LRModal> AlreadyUnloadLrList = TempData.Peek("AlreadyUnloadLrList") as List<LRModal>;
            if (AlreadyUnloadLrList == null)
            {
                AlreadyUnloadLrList = new List<LRModal>();
            }

            var html = ViewHelper.RenderPartialView(this, "UnloadLrGridView", AlreadyUnloadLrList);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult OTHRefreshUnLoadGridView()
        {
            List<LRModal> AlreadyUnloadLrList = TempData.Peek("OTHAlreadyUnloadLrList") as List<LRModal>;
            if (AlreadyUnloadLrList == null)
            {
                AlreadyUnloadLrList = new List<LRModal>();
            }

            var html = ViewHelper.RenderPartialView(this, "OTHUnloadLrGridView", AlreadyUnloadLrList);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Delivery

        public string GetNewCode()
        {
            var NewLcNo = ctxTFAT.DeliveryMaster.Where(x => x.Prefix == mperiod).OrderByDescending(x => x.DeliveryNo).Select(x => x.DeliveryNo).Take(1).FirstOrDefault();
            int LcNo;
            if (NewLcNo == 0)
            {
                var DocType = ctxTFAT.DocTypes.Where(x => x.Code == "DELV0").Select(x => new { x.DocWidth, x.LimitFrom }).FirstOrDefault();
                LcNo = Convert.ToInt32(DocType.LimitFrom);
            }
            else
            {
                LcNo = Convert.ToInt32(NewLcNo) + 1;
            }

            return LcNo.ToString();
        }

        public ActionResult Delivery(string Fmno)
        {
            LoadingDispachVM mModel = new LoadingDispachVM();
            mModel.DelStatuss = PopulateDeliveryStatus();
            TempData.Remove("UnloadLcList");

            FMROUTETable fMROUTETable = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == Fmno).FirstOrDefault();


            FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == fMROUTETable.Parentkey).FirstOrDefault();
            List<LRModal> UnloadList = new List<LRModal>();

            UnloadList = GetUnLoadLClistOfAllMaterial(fMMaster, "");
            TempData["UnloadLcList"] = UnloadList;

            List<VehicleDelivery> vehicleDeliveries = new List<VehicleDelivery>();
            foreach (var item in UnloadList)
            {
                VehicleDelivery vehicleDelivery = new VehicleDelivery();
                vehicleDelivery.TableKey = item.recordkey;
                vehicleDelivery.Lrno = item.Lrno;
                vehicleDelivery.LRDate = item.Date;
                vehicleDelivery.Qty = item.Qty;
                vehicleDelivery.Weight = item.Weight;
                vehicleDelivery.Consignor = item.Consignor;
                vehicleDelivery.Consignee = item.Consignee;
                vehicleDelivery.From = item.From_Name;
                vehicleDelivery.To = item.To_Name;

                vehicleDelivery.Lcno = item.Lcno;
                vehicleDelivery.LcFrom = item.LcFrom;
                vehicleDelivery.LCTo = item.LCTo;

                vehicleDelivery.ShortQty = 0;
                vehicleDelivery.DelStatus = "OK";
                vehicleDelivery.DelNarr = "OK";
                vehicleDelivery.DelDate = DateTime.Now.ToShortDateString();
                vehicleDeliveries.Add(vehicleDelivery);
            }
            mModel.vehicleDeliveries = vehicleDeliveries;
            var html = ViewHelper.RenderPartialView(this, "_ReadyForDeliveryList", mModel);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeliveryStopAlertNote(string Type, List<string> TypeCode)
        {
            string Status = "Success", Message = "Please Remove Following Consignment : \n";

            var GetConsignmentKey = ctxTFAT.LRStock.Where(x => TypeCode.Contains(x.TableKey)).Select(x => x.LRRefTablekey).ToList();


            var Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && GetConsignmentKey.Contains(AlertMater.ParentKey) && AlertMater.Stop.Contains("DELV0")
                        orderby AlertMater.DocNo
                        select new AlertNoteVM()
                        {
                            TypeCode = AlertMater.TypeCode,
                            DocReceived = AlertMater.TableKey,
                        }).ToList();
            foreach (var item in Mobj)
            {
                var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                foreach (var stp in Activirty)
                {

                    Status = "Error";
                    Message += item.TypeCode + " \n";
                    break;

                }
            }
            var jsonResult = Json(new { Status = Status, Message = Message }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult SaveDelivery(LoadingToDispatchVM mModel)
        {
            string Popup = "";
            bool OtherBranchDel = false;
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    string DeliveryKey = "";
                    FMROUTETable fM_ROUTE_ = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
                    if (fM_ROUTE_ != null)
                    {
                        if (mModel.vehicleDeliveries != null)
                        {
                            int DeliveryNo = Convert.ToInt32(GetNewCode());
                            foreach (var item in mModel.vehicleDeliveries)
                            {
                                int ConsignmentAvlQty = 0, DelQty = 0;
                                LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey == item.TableKey).FirstOrDefault();
                                LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == lRStock.LRRefTablekey.ToString()).FirstOrDefault();
                                if (lRStock.Branch != mbranchcode && OtherBranchDel == false)
                                {
                                    OtherBranchDel = true;
                                }
                                var BalQty = ctxTFAT.LRStock.Where(x => x.ParentKey == lRStock.TableKey).Sum(x => (int?)x.TotalQty) ?? 0;
                                var UnDispatchLC = ctxTFAT.LCMaster.Where(x => x.DispachFM == 0).Select(x => x.TableKey).ToList();
                                var LCLoadQty = ctxTFAT.LCDetail.Where(x => x.ParentKey == lRStock.TableKey && UnDispatchLC.Contains(x.LCRefTablekey)).Sum(x => (int?)x.LrQty) ?? 0;

                                ConsignmentAvlQty = lRStock.TotalQty - (BalQty + LCLoadQty);
                                if (ConsignmentAvlQty >= item.DeliveredQty)
                                {
                                    DelQty = item.DeliveredQty;
                                    lRStock.BalQty -= DelQty;
                                    lRStock.BalWeight -= (DelQty * lRMaster.ActWt) / lRMaster.TotQty;
                                    ctxTFAT.Entry(lRStock).State = EntityState.Modified;

                                }
                                else
                                {
                                    DelQty = ConsignmentAvlQty;
                                    lRStock.BalQty -= DelQty;
                                    lRStock.BalWeight -= (DelQty * lRMaster.ActWt) / lRMaster.TotQty;
                                    ctxTFAT.Entry(lRStock).State = EntityState.Modified;
                                    Popup += lRMaster.LrNo.ToString() + " This Consignment Available Qty Only " + ConsignmentAvlQty + "  .\n So We Deliverd Only Available Qty Of This Consignment...!\n";
                                }



                                DeliveryMaster deliveryMaster = new DeliveryMaster();
                                deliveryMaster.DeliveryNo = DeliveryNo;
                                ++DeliveryNo;
                                deliveryMaster.GenerateType = "A";
                                deliveryMaster.CreateDate = DateTime.Now;

                                deliveryMaster.LoginBranch = mbranchcode;
                                deliveryMaster.Branch = mbranchcode;
                                deliveryMaster.LrNO = Convert.ToInt32(item.Lrno);
                                deliveryMaster.DeliveryTime = DateTime.Now.ToString("HH:mm");
                                deliveryMaster.DeliveryDate = ConvertDDMMYYTOYYMMDD(item.DelDate);
                                deliveryMaster.Consigner = lRMaster.RecCode;
                                deliveryMaster.Consignee = lRMaster.SendCode;
                                deliveryMaster.FromBranch = lRStock.FromBranch;
                                deliveryMaster.ToBranch = lRStock.ToBranch;
                                deliveryMaster.Qty = DelQty;
                                deliveryMaster.Weight = (DelQty * lRMaster.ActWt) / lRMaster.TotQty;
                                deliveryMaster.DeliveryGoodStatus = item.DelStatus;
                                deliveryMaster.ShortQty = item.ShortQty;
                                deliveryMaster.DeliveryRemark = item.DelNarr;
                                deliveryMaster.TableKey = mbranchcode + "DEL00" + mperiod.Substring(0, 2) + "001" + Convert.ToInt32(deliveryMaster.DeliveryNo).ToString("D6");
                                deliveryMaster.ParentKey = lRStock.LRRefTablekey;
                                deliveryMaster.VehicleNO = item.DelVehicleNo;
                                deliveryMaster.BillQty = 0;
                                deliveryMaster.PersonName = "";
                                deliveryMaster.MobileNO = "";

                                deliveryMaster.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                                deliveryMaster.ENTEREDBY = muserid;
                                deliveryMaster.AUTHORISE = mauthorise;
                                deliveryMaster.AUTHIDS = muserid;
                                deliveryMaster.Prefix = mperiod;

                                #region Multiple Delivery

                                string Athorise1 = "A00";
                                #region Authorisation
                                TfatUserAuditHeader authorisation1 = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == "DELV0").FirstOrDefault();
                                if (authorisation1 != null)
                                {
                                    Athorise1 = SetAuthorisationLogistics(authorisation1, deliveryMaster.TableKey, deliveryMaster.DeliveryNo.ToString(), 0, deliveryMaster.DeliveryDate.ToShortDateString(), 0, "", mbranchcode);
                                    deliveryMaster.AUTHORISE = Athorise1;
                                }
                                #endregion
                                DelRelation delRelation = new DelRelation();
                                delRelation.DeliveryNo = deliveryMaster.DeliveryNo;
                                delRelation.Branch = mbranchcode;
                                delRelation.Type = "TRN";
                                delRelation.ParentKey = item.TableKey;
                                delRelation.DelQty = DelQty;
                                delRelation.DelWeight = (DelQty * lRMaster.ActWt) / lRMaster.TotQty;
                                delRelation.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                                delRelation.ENTEREDBY = muserid;
                                delRelation.AUTHORISE = Athorise1;
                                delRelation.AUTHIDS = muserid;
                                delRelation.Prefix = mperiod;

                                #endregion
                                LRStock LoadlRStock = new LRStock();
                                LoadlRStock.LoginBranch = mbranchcode;
                                LoadlRStock.Branch = mbranchcode;
                                LoadlRStock.LrNo = Convert.ToInt32(item.Lrno);
                                LoadlRStock.LoadForGodown = 0;
                                LoadlRStock.LoadForDirect = 0;
                                LoadlRStock.Date = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                                LoadlRStock.Time = (DateTime.Now.ToString("HH:mm"));
                                LoadlRStock.TotalQty = DelQty;
                                LoadlRStock.AllocatBalQty = DelQty;
                                LoadlRStock.BalQty = DelQty;
                                LoadlRStock.ActWeight = Convert.ToDouble(delRelation.DelWeight);
                                LoadlRStock.AllocatBalWght = Convert.ToDouble(delRelation.DelWeight);
                                LoadlRStock.BalWeight = Convert.ToDouble(delRelation.DelWeight);
                                LoadlRStock.ChrgWeight = lRMaster.ChgWt;
                                LoadlRStock.ChrgType = lRMaster.ChgType;
                                LoadlRStock.Description = lRMaster.DescrType;
                                LoadlRStock.Unit = lRMaster.UnitCode;
                                LoadlRStock.FromBranch = lRMaster.Source;
                                LoadlRStock.ToBranch = lRMaster.Dest;
                                LoadlRStock.Consigner = lRMaster.RecCode;
                                LoadlRStock.Consignee = lRMaster.SendCode;
                                LoadlRStock.LrType = lRMaster.LRtype;
                                LoadlRStock.Coln = lRMaster.Colln;
                                LoadlRStock.Delivery = lRMaster.Delivery;
                                LoadlRStock.Remark = "";
                                LoadlRStock.StockAt = "Delivery";
                                LoadlRStock.StockStatus = "D";
                                LoadlRStock.LCNO = lRStock.LCNO;
                                LoadlRStock.AUTHIDS = muserid;
                                LoadlRStock.AUTHORISE = mauthorise;
                                LoadlRStock.ENTEREDBY = muserid;
                                LoadlRStock.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                                LoadlRStock.UnloadDirectQty = 0;
                                LoadlRStock.UnloadGodwonQty = 0;
                                LoadlRStock.Fmno = lRStock.Fmno;
                                LoadlRStock.TableKey = mbranchcode + "DEL00" + mperiod.Substring(0, 2) + 1.ToString("D3") + deliveryMaster.DeliveryNo.ToString("D6");
                                LoadlRStock.ParentKey = item.TableKey;
                                LoadlRStock.Type = "DEL";
                                LoadlRStock.LRMode = lRMaster.LRMode;
                                LoadlRStock.Prefix = mperiod;
                                LoadlRStock.LRRefTablekey = lRStock.LRRefTablekey;
                                LoadlRStock.LCRefTablekey = lRStock.LCRefTablekey;
                                LoadlRStock.FMRefTablekey = lRStock.FMRefTablekey;
                                deliveryMaster.MultiDel = false;
                                ctxTFAT.DeliveryMaster.Add(deliveryMaster);
                                ctxTFAT.DelRelation.Add(delRelation);
                                ctxTFAT.LRStock.Add(LoadlRStock);
                                DeliveryKey += deliveryMaster.TableKey + ",";
                                DeliveryNotification(deliveryMaster, OtherBranchDel, "VehicleActivity");
                            }
                        }
                        fM_ROUTE_.Deliveries += String.IsNullOrEmpty(fM_ROUTE_.Deliveries) ? DeliveryKey.Substring(0, DeliveryKey.Length - 1) : "," + DeliveryKey.Substring(0, DeliveryKey.Length - 1);
                        ctxTFAT.Entry(fM_ROUTE_).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    //UpdateAuditTrail(mbranchcode, "Add", "Delivery", "", DateTime.Now, 0, "", "", "NA");
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
            return Json(new { Status = "Success", Popup = Popup, id = "StateMaster" }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult DeliverdDone(string Document)
        {
            List<VehicleDelivery> DeliveryListList = new List<VehicleDelivery>();
            LoadingToDispatchVM loadingDispachVM = new LoadingToDispatchVM();
            FMROUTETable fMROUTE = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == Document).FirstOrDefault();
            if (fMROUTE != null)
            {
                if (!String.IsNullOrEmpty(fMROUTE.Deliveries))
                {
                    var DelTablekey = fMROUTE.Deliveries.Split(',').ToList();
                    var DeliveryMaster = ctxTFAT.DeliveryMaster.Where(x => DelTablekey.Contains(x.TableKey)).ToList();
                    var DeliveryNos = DeliveryMaster.Select(x => x.DeliveryNo).ToList();
                    var DelRelations = ctxTFAT.DelRelation.Where(x => DeliveryNos.Contains(x.DeliveryNo)).ToList();
                    foreach (var item in DeliveryMaster)
                    {
                        DelRelation delRelation = ctxTFAT.DelRelation.Where(x => x.DeliveryNo == item.DeliveryNo).FirstOrDefault();
                        LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey == delRelation.ParentKey).FirstOrDefault();
                        LCMaster lCMaster = ctxTFAT.LCMaster.Where(x => x.LCno == lRStock.LCNO).FirstOrDefault();
                        LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.LrNo == lRStock.LrNo).FirstOrDefault();
                        VehicleDelivery vehicleDelivery = new VehicleDelivery();
                        vehicleDelivery.DeliveryNo = item.DeliveryNo.ToString();
                        vehicleDelivery.DelTableyKey = item.TableKey.ToString();
                        vehicleDelivery.DelDate = item.DeliveryDate.ToShortDateString().ToString();
                        vehicleDelivery.ShortQty = item.ShortQty.Value;
                        vehicleDelivery.DeliveredQty = delRelation.DelQty;
                        vehicleDelivery.DelVehicleNo = item.VehicleNO;
                        vehicleDelivery.DelStatus = item.DeliveryGoodStatus;
                        vehicleDelivery.DelNarr = item.DeliveryRemark;
                        vehicleDelivery.Lcno = lCMaster.LCno.ToString();
                        vehicleDelivery.LcFrom = ctxTFAT.TfatBranch.Where(x => x.Code == lCMaster.FromBranch).Select(x => x.Name).FirstOrDefault();
                        vehicleDelivery.LCTo = ctxTFAT.TfatBranch.Where(x => x.Code == lCMaster.ToBranch).Select(x => x.Name).FirstOrDefault();
                        vehicleDelivery.Lrno = item.LrNO;
                        vehicleDelivery.From = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Source).Select(x => x.Name).FirstOrDefault();
                        vehicleDelivery.To = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name).FirstOrDefault();

                        DeliveryListList.Add(vehicleDelivery);
                    }
                }
            }

            loadingDispachVM.vehicleDeliveries = DeliveryListList;
            var html = ViewHelper.RenderPartialView(this, "_DeliverdView", loadingDispachVM);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteDelivery(LoadingToDispatchVM mModel)//Delete LR From Unloded List (Only View)
        {
            string Message = "", Status = "Success";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    FMROUTETable fM_ROUTE_ = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
                    if (fM_ROUTE_ != null)
                    {
                        var DeliveryKeyRoutetable = fM_ROUTE_.Deliveries.Split(',').ToList();

                        var ChildList = GetChildGrp(mbranchcode);
                        foreach (var item in mModel.vehicleDeliveries)
                        {

                            var deliveryMaster = ctxTFAT.DeliveryMaster.Where(x => x.TableKey.ToString() == item.DelTableyKey).FirstOrDefault();
                            var DeliveryRelList = ctxTFAT.DelRelation.Where(x => x.DeliveryNo == deliveryMaster.DeliveryNo && x.Prefix == deliveryMaster.Prefix).ToList();
                            var Lrstock = ctxTFAT.LRStock.Where(x => x.TableKey.ToString() == item.DelTableyKey).FirstOrDefault();

                            if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.Type == "DELV0" && x.LockDate == deliveryMaster.DeliveryDate).FirstOrDefault() != null)
                            {
                                Message += "This Delivery Date Locked By Period Locking System..";
                            }
                            foreach (var item1 in DeliveryRelList)
                            {
                                LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey.ToString().Trim() == item1.ParentKey.Trim()).FirstOrDefault();

                                if (lRStock.Type == "TRN")
                                {
                                    FMMaster fM = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == lRStock.FMRefTablekey.ToString()).FirstOrDefault();
                                    if (fM.FmStatus == "CC")
                                    {
                                        Message += "Not Allow To Delete Delivery....!\n Because OF Fm Completed.";
                                    }
                                    if (fM.ActivityFollowup == true)
                                    {
                                        if (!ChildList.Contains(fM.CurrBranch))
                                        {
                                            Message += "Not Allow To Delete Delivery....! \n Bcoz Fm Not In Our Branch...!";
                                        }
                                    }
                                }
                                if (String.IsNullOrEmpty(Message))
                                {
                                    lRStock.BalQty += deliveryMaster.Qty;
                                    lRStock.BalWeight += deliveryMaster.Weight;
                                    ctxTFAT.Entry(lRStock).State = EntityState.Modified;
                                }
                            }
                            if (String.IsNullOrEmpty(Message))
                            {
                                DeliveryKeyRoutetable.Remove(item.DelTableyKey);
                                ctxTFAT.DeliveryMaster.Remove(deliveryMaster);
                                ctxTFAT.LRStock.Remove(Lrstock);
                                ctxTFAT.DelRelation.RemoveRange(DeliveryRelList);
                            }
                        }
                        if (!String.IsNullOrEmpty(Message))
                        {
                            Status = "Error";
                        }
                        string DeliveriesKey = "";
                        foreach (var item in DeliveryKeyRoutetable.Distinct().ToList())
                        {
                            if (!String.IsNullOrEmpty(item))
                            {
                                DeliveriesKey += item + ",";
                            }
                        }
                        if (String.IsNullOrEmpty(DeliveriesKey))
                        {
                            fM_ROUTE_.Deliveries = "";
                        }
                        else
                        {
                            fM_ROUTE_.Deliveries = DeliveriesKey.Substring(0, (DeliveriesKey.Length - 1));
                        }
                        ctxTFAT.Entry(fM_ROUTE_).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    //UpdateAuditTrail(mbranchcode, "Add", "Delivery", "", DateTime.Now, 0, "", "");
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
            return Json(new { Status = Status, Message = Message, id = "StateMaster" }, JsonRequestBehavior.AllowGet);

        }

        #endregion

        #region Dispatch
        public ActionResult DispatchPartial(string Reco)
        {
            FMROUTETable fM_ROUTE = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == Reco).FirstOrDefault();
            LoadingToDispatchVM mModel = new LoadingToDispatchVM();
            List<DispatchVM> dispatchVMs = new List<DispatchVM>();
            if (fM_ROUTE != null)
            {
                List<FMROUTETable> fM_s = ctxTFAT.FMROUTETable.Where(x => x.SubRoute == fM_ROUTE.SubRoute && x.FmNo == fM_ROUTE.FmNo).ToList();
                foreach (var item in fM_s)
                {
                    TfatBranch branch = ctxTFAT.TfatBranch.Where(x => x.Code == item.RouteVia).FirstOrDefault();
                    DispatchVM dispatchVM = new DispatchVM();
                    dispatchVM.RecordKey = item.RECORDKEY.ToString();
                    dispatchVM.AreaNameOf_A_D = branch.Category == "Branch" ? branch.Name + " - B" : branch.Category == "SubBranch" ? branch.Name + " - SB" : branch.Name + " - A";
                    dispatchVM.DispatchDate = item == null ? null : item.DispatchDate == null ? null : item.DispatchDate.Value.ToShortDateString();
                    dispatchVM.DispatchTime = item == null ? null : item.DispatchTime == null ? null : item.DispatchTime;
                    dispatchVM.DispachKM = item == null ? "0" : item.DispatchKM == null ? "0" : item.DispatchKM.Value.ToString();
                    dispatchVM.DispachRemark = item == null ? "" : item.DispatchRemark == null ? "" : item.DispatchRemark;
                    dispatchVMs.Add(dispatchVM);
                }

                mModel.DispatchList = dispatchVMs;
            }
            var html = ViewHelper.RenderPartialView(this, "_DispatchPartialView", mModel);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #endregion

        #region Vehicle Tracking Details

        public ActionResult TrackID(LoadingToDispatchVM Model)
        {
            var SetUrl = "";
            string Status = "Sucess", VehicleNO = "";
            string Latitude = "", Longitude = "", Vehicle = "";
            bool TrackReq = false;
            string Msg = "Tracking Not Avalable...!";
            var CurrDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());

            FMROUTETable fM_ROUTE_ = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == Model.Document).FirstOrDefault();
            if (fM_ROUTE_ != null)
            {
                TfatVehicleTrackingSetup vehicleTrackingSetup = ctxTFAT.TfatVehicleTrackingSetup.FirstOrDefault();
                if (vehicleTrackingSetup != null)
                {
                    if (vehicleTrackingSetup.VA_AllTime)
                    {
                        TrackReq = true;
                    }
                    else if (vehicleTrackingSetup.VA_UptoDaysReq)
                    {
                        var Docdate = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == fM_ROUTE_.Parentkey.ToString().Trim()).Select(x => x.Date).FirstOrDefault();
                        Docdate = Docdate.AddDays(vehicleTrackingSetup.VA_UptoDays);
                        if (CurrDate <= Docdate)
                        {
                            TrackReq = true;
                        }
                        else
                        {
                            Msg = "This Vehicle Tacking Allow Upto " + Docdate.ToShortDateString() + " . We Can Not Processed To Tracking Of This Vehicle...!";
                        }
                    }
                    else if (vehicleTrackingSetup.VA_CompleteReq)
                    {
                        var FMmaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == fM_ROUTE_.Parentkey.ToString().Trim()).FirstOrDefault();
                        if (FMmaster != null)
                        {
                            if (FMmaster.FmStatus != "C")
                            {
                                TrackReq = true;
                            }
                            else
                            {
                                Msg = "This Vehicle Trip Completed.  We Can Not Processed To Tracking Of This Vehicle...!";
                            }
                        }
                        else
                        {
                            Msg = "Vehicle Not Found...!";
                        }

                    }
                }

                var GetVehicleTrackId = ctxTFAT.TfatVehicleTrackApiList.Where(x => x.VehicleList.Contains(fM_ROUTE_.VehicleNo)).FirstOrDefault();
                if (GetVehicleTrackId == null)
                {
                    TrackReq = false;
                    Msg = "This Vehicle Not Fount To Any VehicleTracking List.\nPlease Check Tracking Details In Company Profile...!";
                }
                else
                {
                    if (!String.IsNullOrEmpty(GetVehicleTrackId.TrackApi))
                    {
                        SetUrl += GetVehicleTrackId.TrackApi;
                    }
                    if (!String.IsNullOrEmpty(GetVehicleTrackId.Username))
                    {
                        SetUrl += GetVehicleTrackId.Username;
                    }
                    if (!String.IsNullOrEmpty(GetVehicleTrackId.Password))
                    {
                        SetUrl += GetVehicleTrackId.Password;
                    }
                    if (!String.IsNullOrEmpty(GetVehicleTrackId.Para1))
                    {
                        SetUrl += GetVehicleTrackId.Para1;
                    }
                    if (!String.IsNullOrEmpty(GetVehicleTrackId.Para2))
                    {
                        SetUrl += GetVehicleTrackId.Para2;
                    }
                    if (!String.IsNullOrEmpty(GetVehicleTrackId.Para3))
                    {
                        SetUrl += GetVehicleTrackId.Para3;
                    }
                    if (!String.IsNullOrEmpty(GetVehicleTrackId.Para4))
                    {
                        SetUrl += GetVehicleTrackId.Para4;
                    }
                    if (!String.IsNullOrEmpty(GetVehicleTrackId.Para5))
                    {
                        SetUrl += GetVehicleTrackId.Para5;
                    }
                    if (fM_ROUTE_.VehicleNo.Contains("H"))
                    {
                        SetUrl = SetUrl.Replace("@vehiclePara", ctxTFAT.HireVehicleMaster.Where(x => x.Code == fM_ROUTE_.VehicleNo).Select(x => x.TruckNo).FirstOrDefault().Replace(" ", ""));
                    }
                    else
                    {
                        SetUrl = SetUrl.Replace("@vehiclePara", ctxTFAT.VehicleMaster.Where(x => x.Code == fM_ROUTE_.VehicleNo).Select(x => x.TruckNo).FirstOrDefault().Replace(" ", ""));
                    }
                }
            }

            if (TrackReq)
            {
                var GenerateUrl = "http://api.ilogistek.com:6049/tracking?AuthKey=PV7q60SurjHlacX398hbQ4SiWfYktoL&vehicle_no=" + VehicleNO;
                WebClient client = new WebClient();
                string jsonstring = client.DownloadString(SetUrl);
                dynamic dynObj = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonstring);
                var Check = "";
                if (SetUrl.Contains("ilogistek"))
                {
                    foreach (var item in dynObj)
                    {
                        Check = item["VehicleNo"];
                    }
                }
                else if (SetUrl.Contains("elixiatech"))
                {
                    var status = dynObj.Status;

                    if (status.Value == "0")
                    {
                        Check = null;
                    }
                    else
                    {
                        Check = "Success";
                    }
                }

                if (Check == null)
                {
                    Status = "Error";
                }
                else
                {
                    if (SetUrl.Contains("ilogistek"))
                    {
                        foreach (var item in dynObj)
                        {
                            Latitude = item["Latitude"];
                            Longitude = item["Longitude"];
                            Vehicle = item["VehicleNo"];
                        }
                        //Latitude = dynObj.Latitude.Value;
                        //Longitude = dynObj.Longitude.Value;
                        //Vehicle = dynObj.VehicleNo.Value;
                    }
                    else if (SetUrl.Contains("elixiatech"))
                    {

                        Latitude = dynObj.Result.data[0]["lat"];
                        Longitude = dynObj.Result.data[0]["lng"];
                        Vehicle = dynObj.Result.data[0]["vehicleno"];
                    }
                }


                //Latitude = Status == "Error" ? "" : dynObj.Latitude.Value;
                //Longitude = Status == "Error" ? "" : dynObj.Longitude.Value;

            }

            var jsonResult = Json(new
            {
                TrackReq = TrackReq,
                Msg = Msg,
                Status = Status,
                Latitude = Latitude,
                Longitude = Longitude,
                Vehicle = Status == "Error" ? "" : Vehicle,
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        #endregion

        #region AlertNote Stop CHeck

        public ActionResult CheckStopLODAlertNote(string Type, List<string> TypeCode, string DocTpe)
        {
            string Status = "Success", Message = "";

            foreach (var item in TypeCode)
            {
                var LRListOfLC = ctxTFAT.LCDetail.Where(x => x.LCRefTablekey.ToString() == item).Select(x => x.LRRefTablekey.ToString()).ToList();
                var Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                            where AlertMater.Type == Type && LRListOfLC.Contains(AlertMater.ParentKey) && AlertMater.RefType.Contains(DocTpe)
                            orderby AlertMater.DocNo
                            select new AlertNoteVM()
                            {
                                TypeCode = AlertMater.TypeCode,
                                DocReceived = AlertMater.TableKey,
                            }).ToList();
                foreach (var item1 in Mobj)
                {
                    var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item1.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                    foreach (var stp in Activirty)
                    {
                        if (DocTpe.Trim() == stp.Trim())
                        {
                            Status = "Error";
                            Message += item1.TypeCode + " LR Stopped For Loading And This LR Include In " + item + " LC So U Cannot Load This LC....!\n";
                            break;
                        }
                    }
                }
            }
            var jsonResult = Json(new { Status = Status, Message = Message }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult CheckStopUNLODAlertNote(string Type, List<string> TypeCode, string DocTpe)
        {
            string Status = "Success", Message = "";
            foreach (var item in TypeCode)
            {
                LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey == item).FirstOrDefault();
                var Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                            where AlertMater.Type == Type && AlertMater.ParentKey == lRStock.LRRefTablekey && AlertMater.RefType.Contains(DocTpe)
                            orderby AlertMater.DocNo
                            select new AlertNoteVM()
                            {
                                TypeCode = AlertMater.TypeCode,
                                DocReceived = AlertMater.TableKey,
                            }).ToList();
                foreach (var item1 in Mobj)
                {
                    var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item1.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                    foreach (var stp in Activirty)
                    {
                        if (DocTpe.Trim() == stp.Trim())
                        {
                            Status = "Error";
                            Message += item1.TypeCode + " Not Allowed TO UNLOAD Please Remove IT....\n";
                            break;
                        }
                    }
                }
            }

            var jsonResult = Json(new { Status = Status, Message = Message }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        #endregion


        public ActionResult CheckLastBranch(GridOption Model)
        {
            bool LastBranch = false, VehicleModalShow = false, CheckMaterial = false;

            LogisticsFlow flow = ctxTFAT.LogisticsFlow.FirstOrDefault();
            if (flow == null)
            {
                flow = new LogisticsFlow();
            }

            FMROUTETable fMROUTE = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == Model.Document).FirstOrDefault();
            if (ctxTFAT.FMROUTETable.Where(x => x.Parentkey == fMROUTE.Parentkey && x.RouteType == "R").ToList().Count() - 1 == Convert.ToInt32(fMROUTE.SequenceRoute))
            {
                LastBranch = true;
                CheckMaterial = flow.DestCheckMaterial;

                FMSetup fMSetup = ctxTFAT.FMSetup.FirstOrDefault();
                if (String.IsNullOrEmpty(fMSetup.VehicleCateStsMain))
                {
                    VehicleModalShow = false;
                }
                else
                {
                    FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey == fMROUTE.Parentkey).FirstOrDefault();
                    var VehicleCategoryStsMain = fMSetup.VehicleCateStsMain.Split(',');
                    if (VehicleCategoryStsMain.Contains(fMMaster.VehicleStatus))
                    {
                        VehicleModalShow = true;
                    }
                    else
                    {
                        VehicleModalShow = false;
                    }
                }
            }
            else
            {
                LastBranch = false;
                CheckMaterial = flow.RouteCheckMaterial;

            }



            return Json(new { CheckMaterial = CheckMaterial, Message = LastBranch, VehicleModalShow = VehicleModalShow, Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckClear(LoadingToDispatchVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    connstring = GetConnectionString();
                    FMROUTETable fM_ROUTE_ = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();

                    if (fM_ROUTE_ != null)
                    {
                        List<FMROUTETable> listroute = ctxTFAT.FMROUTETable.Where(x => x.Parentkey == fM_ROUTE_.Parentkey && x.SubRoute == fM_ROUTE_.SubRoute).ToList();
                        FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey == fM_ROUTE_.Parentkey).FirstOrDefault();
                        var Authorise = fMMaster.AUTHORISE.Substring(0, 1);
                        if (Authorise == "A")
                        {
                            LogisticsFlow flow = ctxTFAT.LogisticsFlow.FirstOrDefault();
                            if (flow == null)
                            {
                                flow = new LogisticsFlow();
                            }

                            if (mModel.LastRoute == false)
                            {
                                if (flow.RouteCheckMaterial)
                                {
                                    string Branch = mbranchcode == "HO0000" ? "'HO0000'" : "SELECT * FROM SP_GetBranchChild('" + mbranchcode + "')";
                                    SqlDataAdapter da = new SqlDataAdapter();
                                    DataTable dt = new DataTable();
                                    SqlCommand cmd = new SqlCommand();
                                    SqlConnection con = new SqlConnection(connstring);
                                    var Query = "Select  (select Name From TfatBranch where Code=LRSTK.Branch) As Branch, LRSTK.LrNo,LRSTK.Tablekey As TableKey " +
                                        "from LRStock LRSTK where LRSTK.FMRefTablekey= '" + fMMaster.TableKey + "'   and   LRSTK.Branch in (  " + Branch + ")  and" +
                                        " LRSTK.type='TRN' and LRSTK.type<>'DEL' and (case when LRSTK.TotalQty=0 then 0 else " +
                                        "( LRSTK.TotalQty-(  ((select  ISNULL(SUM(LRT.TotalQty),0) from LRStock LRT where LRT.ParentKey=LRSTK.TableKey)) +(select ISNULL(SUM(lcd.lrqty),0) from lcdetail lcd where lcd.parentkey=LRSTK.tablekey and  lcd.LCRefTablekey in (select LCM.Tablekey from lcmaster LCM where LCm.DispachFM=0))  ) ) end ) <>0 ";
                                    cmd = new SqlCommand(Query, con);
                                    con.Open();
                                    da.SelectCommand = cmd;
                                    da.Fill(dt);
                                    con.Close();
                                    con.Dispose();

                                    if (dt.Rows.Count > 0)
                                    {
                                        ClearVehicleNotification(fM_ROUTE_);
                                        if (flow.RouteClearReq)
                                        {
                                            return Json(new { Flag = flow.RouteClearReq, Message = "Please Clear The Your Material From The Vehicle..\n ", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {
                                            return Json(new { Flag = flow.RouteClearReq, Message = " Your Material Exist From The Vehicle ...\n Are You Sure To Clear The Vehicle? ", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                    else
                                    {
                                        return Json(new { Flag = false, Message = " ", Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                            else
                            {
                                bool VehicleModalShow = false;
                                FMSetup fMSetup = ctxTFAT.FMSetup.FirstOrDefault();
                                if (String.IsNullOrEmpty(fMSetup.VehicleCateStsMain))
                                {
                                    VehicleModalShow = false;
                                }
                                else
                                {
                                    var VehicleCategoryStsMain = fMSetup.VehicleCateStsMain.Split(',');
                                    if (VehicleCategoryStsMain.Contains(fMMaster.VehicleStatus))
                                    {
                                        VehicleModalShow = true;
                                    }
                                    else
                                    {
                                        VehicleModalShow = false;
                                    }
                                }

                                if (flow.DestCheckMaterial)
                                {
                                    SqlDataAdapter da = new SqlDataAdapter();
                                    DataTable dt = new DataTable();
                                    SqlCommand cmd = new SqlCommand();
                                    SqlConnection con = new SqlConnection(connstring);
                                    var Query = "Select  (select Name From TfatBranch where Code=LRSTK.Branch) As Branch, LRSTK.LrNo,LRSTK.Tablekey As TableKey " +
                                        "from LRStock LRSTK where LRSTK.FMRefTablekey= '" + fMMaster.TableKey + "'   and" +
                                        " LRSTK.type='TRN' and LRSTK.type<>'DEL' and (case when LRSTK.TotalQty=0 then 0 else " +
                                        "( LRSTK.TotalQty-(  ((select  ISNULL(SUM(LRT.TotalQty),0) from LRStock LRT where LRT.ParentKey=LRSTK.TableKey)) +(select ISNULL(SUM(lcd.lrqty),0) from lcdetail lcd where lcd.parentkey=LRSTK.tablekey and  lcd.LCRefTablekey in (select LCM.Tablekey from lcmaster LCM where LCm.DispachFM=0))  ) ) end ) <>0 ";
                                    cmd = new SqlCommand(Query, con);
                                    con.Open();
                                    da.SelectCommand = cmd;
                                    da.Fill(dt);
                                    con.Close();
                                    con.Dispose();

                                    if (dt.Rows.Count > 0)
                                    {
                                        ClearVehicleNotification(fM_ROUTE_);
                                        if (flow.RouteClearReq)
                                        {
                                            return Json(new { VehicleModalShow = VehicleModalShow, Flag = flow.RouteClearReq, Message = "Please Clear All Material From The Vehicle..\n ", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {
                                            return Json(new { VehicleModalShow = VehicleModalShow, Flag = flow.RouteClearReq, Message = " Material Exist From The Vehicle ...\n Are You Sure To Clear The Vehicle? ", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                    else
                                    {
                                        return Json(new { VehicleModalShow = VehicleModalShow, Flag = false, Message = " ", Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                        }
                    }
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

        public ActionResult Clear(LoadingToDispatchVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    string Parentkey = "";
                    FMROUTETable fM_ROUTE_ = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();

                    if (fM_ROUTE_ != null)
                    {
                        List<FMROUTETable> listroute = ctxTFAT.FMROUTETable.Where(x => x.Parentkey == fM_ROUTE_.Parentkey && x.SubRoute == fM_ROUTE_.SubRoute).ToList();
                        FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey == fM_ROUTE_.Parentkey).FirstOrDefault();
                        Parentkey = fMMaster.ParentKey;
                        var Authorise = fMMaster.AUTHORISE.Substring(0, 1);
                        if (Authorise == "A")
                        {
                            LogisticsFlow flow = ctxTFAT.LogisticsFlow.FirstOrDefault();
                            if (flow != null)
                            {
                                if (!String.IsNullOrEmpty(flow.ADVehicleCategReq))
                                {
                                    var ArriveDispatchReq = flow.ADVehicleCategReq.Split(',').ToList();
                                    if (ArriveDispatchReq.Contains(fMMaster.VehicleStatus))
                                    {
                                        var CheckArrivalAndDispatch = listroute.Where(x => x.ArrivalDate == null || x.ArrivalTime == null || x.ArrivalTime == "" || x.DispatchDate == null || x.DispatchTime == null || x.DispatchTime == "" || x.ArrivalKM == null || x.ArrivalKM == 0).ToList();
                                        if (CheckArrivalAndDispatch.Count() > 0)
                                        {
                                            return Json(new { Message = "Arrival And Dispatch Compulsary So Please Fill The Required Data..\n ", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                }
                            }

                            if (mModel.LastRoute == false)
                            {
                                fM_ROUTE_.RouteClear = true;
                                ctxTFAT.Entry(fM_ROUTE_).State = EntityState.Modified;
                                ctxTFAT.SaveChanges();

                                var NexrSeq = (Convert.ToInt32(fM_ROUTE_.SequenceRoute) + 1).ToString();
                                FMROUTETable Next_rOUTE_Table = ctxTFAT.FMROUTETable.Where(x => x.SequenceRoute.ToString() == NexrSeq && x.Parentkey == fM_ROUTE_.Parentkey).FirstOrDefault();
                                fMMaster.CurrBranch = Next_rOUTE_Table.RouteVia;
                                fMMaster.CurrRoute = Convert.ToInt32(Next_rOUTE_Table.SequenceRoute);

                                var CheckAllClearOrNOt = ctxTFAT.FMROUTETable.Where(x => x.RouteType == "R" && x.Parentkey == fM_ROUTE_.Parentkey && x.RouteClear == false).ToList();
                                if (CheckAllClearOrNOt.Count() > 0)
                                {

                                }
                                else
                                {
                                    fMMaster.FmStatus = "C";
                                }


                                ctxTFAT.Entry(fMMaster).State = EntityState.Modified;

                            }
                            else
                            {

                                fM_ROUTE_.RouteClear = true;
                                ctxTFAT.Entry(fM_ROUTE_).State = EntityState.Modified;
                                ctxTFAT.SaveChanges();

                                var CheckAllClearOrNOt = ctxTFAT.FMROUTETable.Where(x => x.RouteType == "R" && x.Parentkey == fM_ROUTE_.Parentkey && x.RouteClear == false).ToList();
                                if (CheckAllClearOrNOt.Count() > 0)
                                {

                                }
                                else
                                {
                                    fMMaster.FmStatus = "C";
                                }
                                fMMaster.CurrBranch = mbranchcode;
                                fMMaster.CurrRoute = fM_ROUTE_.SequenceRoute.Value;

                                #region Update Vehicle Status
                                FMSetup fMSetup = ctxTFAT.FMSetup.FirstOrDefault();

                                if (!String.IsNullOrEmpty(fMSetup.VehicleCateStsMain))
                                {
                                    var VehicleCategoryStsMain = fMSetup.VehicleCateStsMain.Split(',');
                                    if (VehicleCategoryStsMain.Contains(fMMaster.VehicleStatus))
                                    {
                                        if (fMMaster.VehicleStatus == "100001")
                                        {
                                            HireVehicleMaster hireVehicleMaster = ctxTFAT.HireVehicleMaster.Where(x => x.Code == fMMaster.TruckNo).FirstOrDefault();
                                            if (hireVehicleMaster != null)
                                            {
                                                hireVehicleMaster.Status = mModel.vehicleStatus.ToString();
                                                ctxTFAT.Entry(hireVehicleMaster).State = EntityState.Modified;
                                            }
                                        }
                                        else
                                        {
                                            tfatVehicleStatusHistory vehicleDri_Hist = new tfatVehicleStatusHistory();
                                            vehicleDri_Hist = ctxTFAT.tfatVehicleStatusHistory.Where(x => x.refParentKey == mModel.Document).FirstOrDefault();
                                            TfatVehicleStatus tfatVehicleStatus = new TfatVehicleStatus();
                                            tfatVehicleStatus = ctxTFAT.TfatVehicleStatus.Where(x => x.refParentKey == mModel.Document).FirstOrDefault();
                                            if (tfatVehicleStatus == null)
                                            {
                                                vehicleDri_Hist = new tfatVehicleStatusHistory();
                                                vehicleDri_Hist.Code = GetNewCode_VehiHistory();
                                                vehicleDri_Hist.ENTEREDBY = muserid;
                                                vehicleDri_Hist.refParentKey = mModel.Document;
                                                vehicleDri_Hist.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());

                                                vehicleDri_Hist.Status = mModel.vehicleStatus.ToString();
                                                vehicleDri_Hist.FromPeriod = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                                                vehicleDri_Hist.FromTime = DateTime.Now.ToString("HH:mm");
                                                vehicleDri_Hist.TruckNo = fMMaster.TruckNo;
                                                vehicleDri_Hist.Narr = "Complete Freight Memo";
                                                vehicleDri_Hist.AUTHIDS = muserid;
                                                vehicleDri_Hist.AUTHORISE = mauthorise;
                                                vehicleDri_Hist.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                                                ctxTFAT.tfatVehicleStatusHistory.Add(vehicleDri_Hist);

                                                tfatVehicleStatus = new TfatVehicleStatus();
                                                tfatVehicleStatus.DocNo = GetNewCodeHistory();
                                                tfatVehicleStatus.AUTHIDS = muserid;
                                                tfatVehicleStatus.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                                                tfatVehicleStatus.ENTEREDBY = muserid;
                                                tfatVehicleStatus.refParentKey = mModel.Document;

                                                tfatVehicleStatus.Vehicle = fMMaster.TruckNo;
                                                tfatVehicleStatus.Branch = mbranchcode;
                                                tfatVehicleStatus.Status = mModel.vehicleStatus.ToString();
                                                tfatVehicleStatus.Narr = "Complete Freight Memo";
                                                tfatVehicleStatus.EffDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                                                tfatVehicleStatus.EffTime = DateTime.Now.ToString("HH:mm");
                                                tfatVehicleStatus.AUTHORISE = mauthorise;
                                                tfatVehicleStatus.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                                                ctxTFAT.TfatVehicleStatus.Add(tfatVehicleStatus);
                                            }
                                            else
                                            {
                                                vehicleDri_Hist.Status = mModel.vehicleStatus.ToString();
                                                vehicleDri_Hist.FromPeriod = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                                                vehicleDri_Hist.FromTime = DateTime.Now.ToString("HH:mm");
                                                vehicleDri_Hist.TruckNo = fMMaster.TruckNo;
                                                vehicleDri_Hist.Narr = "Complete Freight Memo";
                                                vehicleDri_Hist.AUTHIDS = muserid;
                                                vehicleDri_Hist.AUTHORISE = mauthorise;
                                                vehicleDri_Hist.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                                                ctxTFAT.Entry(vehicleDri_Hist).State = EntityState.Modified;

                                                tfatVehicleStatus.Vehicle = fMMaster.TruckNo;
                                                tfatVehicleStatus.Branch = mbranchcode;
                                                tfatVehicleStatus.Status = mModel.vehicleStatus.ToString();
                                                tfatVehicleStatus.Narr = "Complete Freight Memo";
                                                tfatVehicleStatus.EffDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                                                tfatVehicleStatus.EffTime = DateTime.Now.ToString("HH:mm");
                                                tfatVehicleStatus.AUTHORISE = mauthorise;
                                                tfatVehicleStatus.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                                                ctxTFAT.Entry(tfatVehicleStatus).State = EntityState.Modified;
                                            }
                                            VehicleMaster vehicleMaster = ctxTFAT.VehicleMaster.Where(x => x.Code == fMMaster.TruckNo).FirstOrDefault();
                                            if (vehicleMaster != null)
                                            {
                                                vehicleMaster.Status = mModel.vehicleStatus.ToString();
                                                ctxTFAT.Entry(vehicleMaster).State = EntityState.Modified;
                                            }
                                        }

                                    }
                                }
                                #endregion
                                ctxTFAT.Entry(fMMaster).State = EntityState.Modified;

                            }
                        }
                        else if (Authorise == "N")
                        {
                            return Json(new { Message = "This FM Un-Authorised Please Contact To The Admin..\n ", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                        }
                        else if (Authorise == "R")
                        {
                            return Json(new { Message = "This FM Rejected Please Contact To The Admin..\n ", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                        }

                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, "Veh-ACT", "Vehicle Activity", Parentkey, DateTime.Now, 0, "", "Clear", "NA");
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

        public ActionResult UnclearRoute(LoadingToDispatchVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    string Parentkey = "";
                    FMROUTETable fM_ROUTE_ = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
                    string NextSeq = (Convert.ToInt32(fM_ROUTE_.SequenceRoute) + 1).ToString();
                    FMROUTETable NextfM_ROUTE_ = ctxTFAT.FMROUTETable.Where(x => x.SequenceRoute.ToString() == NextSeq && x.Parentkey == fM_ROUTE_.Parentkey).FirstOrDefault();

                    if (fM_ROUTE_ != null)
                    {
                        FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey == fM_ROUTE_.Parentkey).FirstOrDefault();

                        if (fMMaster != null)
                        {
                            Parentkey = fMMaster.ParentKey;
                            var Authorise = fMMaster.AUTHORISE.Substring(0, 1);
                            if (Authorise == "A")
                            {
                                if (fMMaster.FmStatus == "C")
                                {
                                    return Json(new { Message = "Not Allow Because Of Fm Completed.... \n", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                                }

                                var SystemFlow = ctxTFAT.LogisticsFlow.FirstOrDefault();
                                if (SystemFlow != null)
                                {
                                    if (SystemFlow.ScheduleFollowUp)
                                    {
                                        if (NextfM_ROUTE_ != null)
                                        {
                                            if (fMMaster.CurrBranch == NextfM_ROUTE_.RouteVia)
                                            {
                                                var Name = ctxTFAT.TfatBranch.Where(x => x.Code == fMMaster.CurrBranch).Select(x => x.Name).FirstOrDefault();
                                                if (!(String.IsNullOrEmpty(NextfM_ROUTE_.ArrivalDate.ToString()) && String.IsNullOrEmpty(NextfM_ROUTE_.ArrivalTime) && String.IsNullOrEmpty(NextfM_ROUTE_.ArrivalKM.ToString()) && String.IsNullOrEmpty(NextfM_ROUTE_.ArrivalRemark)))
                                                {
                                                    return Json(new { Message = "Not Allow Because Vehicle Arrived  In " + Name + " Branch \n", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                                                }
                                                if (!String.IsNullOrEmpty(NextfM_ROUTE_.LCNO))
                                                {
                                                    return Json(new { Message = "Not Allow Because Some Material Load It In " + Name + " Branch \n", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                                                }
                                                if (!String.IsNullOrEmpty(NextfM_ROUTE_.UnLoadLCNO))
                                                {
                                                    return Json(new { Message = "Not Allow Because Some Material UnLoad It In " + Name + " Branch \n", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                                                }
                                                if (!(String.IsNullOrEmpty(NextfM_ROUTE_.DispatchDate.ToString()) && String.IsNullOrEmpty(NextfM_ROUTE_.DispatchTime) && String.IsNullOrEmpty(NextfM_ROUTE_.DispatchKM.ToString()) && String.IsNullOrEmpty(NextfM_ROUTE_.DispatchRemark)))
                                                {
                                                    return Json(new { Message = "Not Allow Because Vehicle Dispatch  From " + Name + " Branch \n", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                                                }

                                                fMMaster.CurrBranch = fM_ROUTE_.RouteVia;
                                                fM_ROUTE_.RouteClear = false;
                                                ctxTFAT.Entry(fMMaster).State = EntityState.Modified;
                                                ctxTFAT.Entry(fM_ROUTE_).State = EntityState.Modified;
                                            }
                                            else
                                            {
                                                var Name = ctxTFAT.TfatBranch.Where(x => x.Code == fMMaster.CurrBranch).Select(x => x.Name).FirstOrDefault();
                                                return Json(new { Message = "Not Allow Because Vehicle Arrived  In " + Name + " Branch \n", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                                            }
                                        }
                                        else
                                        {
                                            var Name = ctxTFAT.TfatBranch.Where(x => x.Code == fMMaster.CurrBranch).Select(x => x.Name).FirstOrDefault();
                                            return Json(new { Message = "Not Allow Because Vehicle Arrived  In " + Name + " Branch \n", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                    else
                                    {
                                        fMMaster.CurrBranch = fM_ROUTE_.RouteVia;
                                        fM_ROUTE_.RouteClear = false;
                                        ctxTFAT.Entry(fMMaster).State = EntityState.Modified;
                                        ctxTFAT.Entry(fM_ROUTE_).State = EntityState.Modified;
                                    }
                                }
                                else
                                {
                                    fMMaster.CurrBranch = fM_ROUTE_.RouteVia;
                                    fM_ROUTE_.RouteClear = false;
                                    ctxTFAT.Entry(fMMaster).State = EntityState.Modified;
                                    ctxTFAT.Entry(fM_ROUTE_).State = EntityState.Modified;
                                }
                            }
                            else if (Authorise == "N")
                            {
                                return Json(new { Message = "This FM Un-Authorised Please Contact To The Admin..\n ", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                            }
                            else if (Authorise == "R")
                            {
                                return Json(new { Message = "This FM Rejected Please Contact To The Admin..\n ", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                            }

                        }
                        else
                        {
                            return Json(new { Message = "Error,FM Not Found....\n", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                        }

                    }
                    else
                    {
                        return Json(new { Message = "Error,Route Not Found....\n", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                    }

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, "Veh-ACT", "Vehicle Activity", Parentkey, DateTime.Now, 0, "", "UnClear", "NA");
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

        public ActionResult ReWorking(LoadingToDispatchVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    string Parentkey = "";
                    FMROUTETable fM_ROUTE_ = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();

                    if (fM_ROUTE_ != null)
                    {
                        FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey == fM_ROUTE_.Parentkey).FirstOrDefault();
                        Parentkey = fMMaster.ParentKey;
                        var Authorise = fMMaster.AUTHORISE.Substring(0, 1);
                        if (Authorise == "A")
                        {
                            fM_ROUTE_.RouteClear = false;
                            fMMaster.FmStatus = "W";
                            ctxTFAT.Entry(fM_ROUTE_).State = EntityState.Modified;
                            ctxTFAT.Entry(fMMaster).State = EntityState.Modified;
                        }
                        else if (Authorise == "N")
                        {
                            return Json(new { Message = "This FM Un-Authorised Please Contact To The Admin..\n ", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                        }
                        else if (Authorise == "R")
                        {
                            return Json(new { Message = "This FM Rejected Please Contact To The Admin..\n ", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                        }


                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, "Veh-ACT", "Vehicle Activity", Parentkey, DateTime.Now, 0, "", "ReWork", "NA");
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


        #region AlertNOteOF LRFrom The FM

        public ActionResult GetCurrnetBranchLoadLRFromLC(string Type, string Reco, string DocTpe)
        {
            AlertNoteController attachmentC = new AlertNoteController();
            List<string> TypeCode = new List<string>();
            TypeCode = ctxTFAT.LCDetail.Where(x => x.LCno.ToString() == Reco).Select(x => x.LRno.ToString()).ToList();
            List<AlertNoteVM> alertNotes = new List<AlertNoteVM>();

            List<AlertNoteVM> Mobj = new List<AlertNoteVM>();
            var Status = "Error";
            string LoadingMessage = "";
            bool Stop = false;

            Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                    where AlertMater.Type == Type && TypeCode.Contains(AlertMater.TypeCode)
                    orderby AlertMater.DocNo
                    select new AlertNoteVM()
                    {
                        ENTEREDBY = AlertMater.CreateBy,
                        TypeCode = AlertMater.TypeCode,
                        Remark = AlertMater.Note,
                        //Stop = AlertMater.Stop,
                        DocDate = AlertMater.DocDate,
                        DocReceived = AlertMater.TableKey,
                        Bling = AlertMater.Bling,
                    }).ToList();
            alertNotes.AddRange(Mobj);

            foreach (var item in Mobj)
            {
                Status = "Success";
                if (item.attachments == null)
                {
                    item.attachments = new List<AttachmentVM>();
                }
            }

            //var html1 = ViewHelper.RenderPartialView(this, "ListOfAlertNoteView", Mobj);
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Logistics/Views/AlertNote/ListOfAlertNoteView.cshtml", Mobj);
            var jsonResult = Json(new { Status = Status, Html = html, Stop = Stop }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;


        }

        #endregion

        #region Not Required Functions

        public ActionResult GetAllRemarkOfDocument(string Fmno)
        {
            FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.FmNo.ToString() == Fmno).FirstOrDefault();

            List<LoadingToDispatchVM> loadingTos = new List<LoadingToDispatchVM>();


            var FMList = (from FreightMemo in ctxTFAT.FMMaster
                          where FreightMemo.FmNo.ToString().Trim() == Fmno.ToString().Trim() && String.IsNullOrEmpty(FreightMemo.Remark) == false
                          orderby FreightMemo.FmNo
                          select new LoadingToDispatchVM()
                          {
                              FMNO = FreightMemo.FmNo.ToString(),
                              AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Branch).Select(x => x.Name).FirstOrDefault(),
                              NarrStr = FreightMemo.Remark,
                              ENTEREDBY = FreightMemo.ENTEREDBY,
                              AUTHIDS = "N",
                              NarrSno = 0,
                              PayLoadL = "Freight Memo",
                          }).ToList();
            loadingTos.AddRange(FMList);
            var GetAlertNote = (from FreightMemo in ctxTFAT.AlertNoteMaster
                                where FreightMemo.TypeCode.ToString().Trim() == Fmno.ToString().Trim() && FreightMemo.Type == "FM000"
                                orderby FreightMemo.DocNo
                                select new LoadingToDispatchVM()
                                {
                                    FMNO = FreightMemo.TypeCode.ToString(),
                                    AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Branch).Select(x => x.Name).FirstOrDefault(),
                                    NarrStr = FreightMemo.Note,
                                    ENTEREDBY = FreightMemo.CreateBy,
                                    AUTHIDS = "N",
                                    NarrSno = 0,
                                    PayLoadL = "Alert Note",
                                }).ToList();
            loadingTos.AddRange(GetAlertNote);
            var ArrivalFMList = (from FreightMemo in ctxTFAT.FMROUTETable
                                 where FreightMemo.FmNo.ToString().Trim() == Fmno.ToString().Trim() && String.IsNullOrEmpty(FreightMemo.ArrivalRemark) == false
                                 orderby FreightMemo.FmNo
                                 select new LoadingToDispatchVM()
                                 {
                                     FMNO = FreightMemo.FmNo.ToString(),
                                     AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Parent).Select(x => x.Name).FirstOrDefault(),
                                     NarrStr = FreightMemo.Narr,
                                     ENTEREDBY = FreightMemo.ENTEREDBY,
                                     AUTHIDS = "N",
                                     NarrSno = 0,
                                     PayLoadL = "Arrival",
                                 }).ToList();
            loadingTos.AddRange(ArrivalFMList);

            var DispatchFMList = (from FreightMemo in ctxTFAT.FMROUTETable
                                  where FreightMemo.FmNo.ToString().Trim() == Fmno.ToString().Trim() && String.IsNullOrEmpty(FreightMemo.DispatchRemark) == false
                                  orderby FreightMemo.FmNo
                                  select new LoadingToDispatchVM()
                                  {
                                      FMNO = FreightMemo.FmNo.ToString(),
                                      AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Parent).Select(x => x.Name).FirstOrDefault(),
                                      NarrStr = FreightMemo.Narr,
                                      ENTEREDBY = FreightMemo.ENTEREDBY,
                                      AUTHIDS = "N",
                                      NarrSno = 0,
                                      PayLoadL = "Dispatch",
                                  }).ToList();
            loadingTos.AddRange(DispatchFMList);


            var GetRemarkDocList = ctxTFAT.Narration.Where(x => x.Srl == Fmno && x.Type == "FM000").ToList();
            foreach (var item in GetRemarkDocList)
            {
                loadingTos.Add(new LoadingToDispatchVM
                {
                    FMNO = item.Srl,
                    AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault(),
                    NarrStr = item.Narr,
                    ENTEREDBY = item.ENTEREDBY,
                    AUTHIDS = item.ENTEREDBY.Trim().ToLower() == muserid.Trim().ToLower() ? "Y" : "N",
                    NarrSno = item.Sno,
                    PayLoadL = item.NarrRich

                });
            }
            var html = ViewHelper.RenderPartialView(this, "_DocumentNarrList", loadingTos);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveNarration(LoadingToDispatchVM Model)
        {
            string html = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    FMROUTETable fM_ROUTE_ = ctxTFAT.FMROUTETable.Where(x => x.FmNo.ToString() == Model.FMNO).FirstOrDefault();
                    if (fM_ROUTE_ != null)
                    {
                        if (Model.NarrStr != null)
                        {
                            FMMaster fM = ctxTFAT.FMMaster.Where(x => x.FmNo == fM_ROUTE_.FmNo).FirstOrDefault();

                            var LastSno = ctxTFAT.Narration.Where(x => x.ParentKey == fM.ParentKey).ToList().Count();
                            ++LastSno;
                            Narration narr = new Narration();
                            narr.Branch = mbranchcode;
                            narr.Narr = Model.NarrStr;
                            narr.NarrRich = Model.Header;
                            narr.Prefix = mperiod;
                            narr.Sno = LastSno;
                            narr.Srl = fM_ROUTE_.FmNo.ToString();
                            narr.Type = "FM000";
                            narr.ENTEREDBY = muserid;
                            narr.LASTUPDATEDATE = DateTime.Now;
                            narr.AUTHORISE = mauthorise;
                            narr.AUTHIDS = muserid;
                            narr.LocationCode = 0;
                            narr.TableKey = mbranchcode + "FM000" + mperiod.Substring(0, 2) + LastSno.ToString("D3") + fM.FmNo.ToString();
                            narr.CompCode = mcompcode;
                            narr.ParentKey = fM.ParentKey;
                            ctxTFAT.Narration.Add(narr);
                            ctxTFAT.SaveChanges();
                            transaction.Commit();
                            transaction.Dispose();

                            List<LoadingToDispatchVM> loadingTos = new List<LoadingToDispatchVM>();


                            var FMList = (from FreightMemo in ctxTFAT.FMMaster
                                          where FreightMemo.FmNo.ToString().Trim() == fM.FmNo.ToString().Trim() && String.IsNullOrEmpty(FreightMemo.Remark) == false
                                          orderby FreightMemo.FmNo
                                          select new LoadingToDispatchVM()
                                          {
                                              FMNO = FreightMemo.FmNo.ToString(),
                                              AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Branch).Select(x => x.Name).FirstOrDefault(),
                                              NarrStr = FreightMemo.Remark,
                                              ENTEREDBY = FreightMemo.ENTEREDBY,
                                              AUTHIDS = "N",
                                              NarrSno = 0,
                                              PayLoadL = "Freight Memo",
                                          }).ToList();
                            loadingTos.AddRange(FMList);
                            var GetAlertNote = (from FreightMemo in ctxTFAT.AlertNoteMaster
                                                where FreightMemo.TypeCode.ToString().Trim() == fM.FmNo.ToString().Trim() && FreightMemo.Type == "FM000"
                                                orderby FreightMemo.DocNo
                                                select new LoadingToDispatchVM()
                                                {
                                                    FMNO = FreightMemo.TypeCode.ToString(),
                                                    AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Branch).Select(x => x.Name).FirstOrDefault(),
                                                    NarrStr = FreightMemo.Note,
                                                    ENTEREDBY = FreightMemo.CreateBy,
                                                    AUTHIDS = "N",
                                                    NarrSno = 0,
                                                    PayLoadL = "Alert Note",
                                                }).ToList();
                            loadingTos.AddRange(GetAlertNote);
                            var ArrivalFMList = (from FreightMemo in ctxTFAT.FMROUTETable
                                                 where FreightMemo.FmNo.ToString().Trim() == fM.FmNo.ToString().Trim() && String.IsNullOrEmpty(FreightMemo.ArrivalRemark) == false
                                                 orderby FreightMemo.FmNo
                                                 select new LoadingToDispatchVM()
                                                 {
                                                     FMNO = FreightMemo.FmNo.ToString(),
                                                     AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Parent).Select(x => x.Name).FirstOrDefault(),
                                                     NarrStr = FreightMemo.Narr,
                                                     ENTEREDBY = FreightMemo.ENTEREDBY,
                                                     AUTHIDS = "N",
                                                     NarrSno = 0,
                                                     PayLoadL = "Arrival",
                                                 }).ToList();
                            loadingTos.AddRange(ArrivalFMList);

                            var DispatchFMList = (from FreightMemo in ctxTFAT.FMROUTETable
                                                  where FreightMemo.FmNo.ToString().Trim() == fM.FmNo.ToString().Trim() && String.IsNullOrEmpty(FreightMemo.DispatchRemark) == false
                                                  orderby FreightMemo.FmNo
                                                  select new LoadingToDispatchVM()
                                                  {
                                                      FMNO = FreightMemo.FmNo.ToString(),
                                                      AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Parent).Select(x => x.Name).FirstOrDefault(),
                                                      NarrStr = FreightMemo.Narr,
                                                      ENTEREDBY = FreightMemo.ENTEREDBY,
                                                      AUTHIDS = "N",
                                                      NarrSno = 0,
                                                      PayLoadL = "Dispatch",
                                                  }).ToList();
                            loadingTos.AddRange(DispatchFMList);


                            var GetRemarkDocList = ctxTFAT.Narration.Where(x => x.Srl == fM.FmNo.ToString() && x.Type == "FM000").ToList();
                            foreach (var item in GetRemarkDocList)
                            {
                                loadingTos.Add(new LoadingToDispatchVM
                                {
                                    FMNO = item.Srl,
                                    AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault(),
                                    NarrStr = item.Narr,
                                    ENTEREDBY = item.ENTEREDBY,
                                    AUTHIDS = item.ENTEREDBY.Trim().ToLower() == muserid.Trim().ToLower() ? "Y" : "N",
                                    NarrSno = item.Sno,
                                    PayLoadL = item.NarrRich
                                });
                            }
                            html = ViewHelper.RenderPartialView(this, "_DocumentNarrList", loadingTos);
                        }
                    }
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
            return Json(new { Html = html, Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteNarr(LoadingToDispatchVM mModel)
        {
            string html = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    Narration narration = ctxTFAT.Narration.Where(x => x.Sno.ToString() == mModel.NarrSno.ToString() && x.Type == "FM000").FirstOrDefault();
                    if (narration != null)
                    {
                        ctxTFAT.Narration.Remove(narration);
                        ctxTFAT.SaveChanges();
                        transaction.Commit();
                        transaction.Dispose();
                    }
                    List<LoadingToDispatchVM> loadingTos = new List<LoadingToDispatchVM>();

                    FMROUTETable fM_ROUTE_ = ctxTFAT.FMROUTETable.Where(x => x.FmNo.ToString() == mModel.Document).FirstOrDefault();
                    if (fM_ROUTE_ != null)
                    {

                        var FMList = (from FreightMemo in ctxTFAT.FMMaster
                                      where FreightMemo.FmNo.ToString().Trim() == fM_ROUTE_.FmNo.ToString().Trim() && String.IsNullOrEmpty(FreightMemo.Remark) == false
                                      orderby FreightMemo.FmNo
                                      select new LoadingToDispatchVM()
                                      {
                                          FMNO = FreightMemo.FmNo.ToString(),
                                          AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Branch).Select(x => x.Name).FirstOrDefault(),
                                          NarrStr = FreightMemo.Remark,
                                          ENTEREDBY = FreightMemo.ENTEREDBY,
                                          AUTHIDS = "N",
                                          NarrSno = 0,
                                          PayLoadL = "Freight Memo",
                                      }).ToList();
                        loadingTos.AddRange(FMList);
                        var GetAlertNote = (from FreightMemo in ctxTFAT.AlertNoteMaster
                                            where FreightMemo.TypeCode.ToString().Trim() == fM_ROUTE_.FmNo.ToString().Trim() && FreightMemo.Type == "FM000"
                                            orderby FreightMemo.DocNo
                                            select new LoadingToDispatchVM()
                                            {
                                                FMNO = FreightMemo.TypeCode.ToString(),
                                                AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Branch).Select(x => x.Name).FirstOrDefault(),
                                                NarrStr = FreightMemo.Note,
                                                ENTEREDBY = FreightMemo.CreateBy,
                                                AUTHIDS = "N",
                                                NarrSno = 0,
                                                PayLoadL = "Alert Note",
                                            }).ToList();
                        loadingTos.AddRange(GetAlertNote);
                        var ArrivalFMList = (from FreightMemo in ctxTFAT.FMROUTETable
                                             where FreightMemo.FmNo.ToString().Trim() == fM_ROUTE_.FmNo.ToString().Trim() && String.IsNullOrEmpty(FreightMemo.ArrivalRemark) == false
                                             orderby FreightMemo.FmNo
                                             select new LoadingToDispatchVM()
                                             {
                                                 FMNO = FreightMemo.FmNo.ToString(),
                                                 AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Parent).Select(x => x.Name).FirstOrDefault(),
                                                 NarrStr = FreightMemo.Narr,
                                                 ENTEREDBY = FreightMemo.ENTEREDBY,
                                                 AUTHIDS = "N",
                                                 NarrSno = 0,
                                                 PayLoadL = "Arrival",
                                             }).ToList();
                        loadingTos.AddRange(ArrivalFMList);

                        var DispatchFMList = (from FreightMemo in ctxTFAT.FMROUTETable
                                              where FreightMemo.FmNo.ToString().Trim() == fM_ROUTE_.FmNo.ToString().Trim() && String.IsNullOrEmpty(FreightMemo.DispatchRemark) == false
                                              orderby FreightMemo.FmNo
                                              select new LoadingToDispatchVM()
                                              {
                                                  FMNO = FreightMemo.FmNo.ToString(),
                                                  AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Parent).Select(x => x.Name).FirstOrDefault(),
                                                  NarrStr = FreightMemo.Narr,
                                                  ENTEREDBY = FreightMemo.ENTEREDBY,
                                                  AUTHIDS = "N",
                                                  NarrSno = 0,
                                                  PayLoadL = "Dispatch",
                                              }).ToList();
                        loadingTos.AddRange(DispatchFMList);

                        var GetRemarkDocList = ctxTFAT.Narration.Where(x => x.Srl == fM_ROUTE_.FmNo.ToString() && x.Type == "FM000").ToList();
                        foreach (var item in GetRemarkDocList)
                        {
                            loadingTos.Add(new LoadingToDispatchVM
                            {
                                FMNO = item.Srl,
                                AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault(),
                                NarrStr = item.Narr,
                                ENTEREDBY = item.ENTEREDBY,
                                NarrSno = item.Sno,
                                PayLoadL = item.NarrRich,
                                AUTHIDS = item.ENTEREDBY.Trim().ToLower() == muserid.Trim().ToLower() ? "Y" : "N",
                            });
                        }
                    }

                    html = ViewHelper.RenderPartialView(this, "_DocumentNarrList", loadingTos);
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
            return Json(new { Html = html, Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region Set New Print

        [HttpPost]
        public ActionResult GetMultiPrint(GridOption Model)
        {
            List<GridOption> Grlist = new List<GridOption>();
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
            //var PDFName = Model.Document.Substring(20);
            var PDFName = "";
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

            //string mParentKey = Model.Document.Substring(6, Model.Document.Length - 6);
            string mParentKey = Model.Document;

            DataTable dtreport = new DataTable();
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("SPDoc_" + Model.StoreProcedure, tfat_conx); //name of the storedprocedure
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@mParentKey", mParentKey);
            cmd.Parameters.AddWithValue("@mBranch", mbranchcode);
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            adp.Fill(dtreport);

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), Model.Format.Trim() + ".rpt"));
            rd.SetDataSource(dtreport);

            try
            {
                Stream mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                mstream.Seek(0, SeekOrigin.Begin);
                rd.Close();
                rd.Dispose();
                if (String.IsNullOrEmpty(PDFName))
                {
                    return File(mstream, "application/PDF");
                }
                else
                {
                    return File(mstream, "application/PDF", PDFName + ".pdf");
                }
                //return File(mstream, "application/PDF");
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
            var ms = new MemoryStream();
            {
                PdfCopy pdf = new PdfCopy(document, ms);
                document.Open();
                pdf.Open();

                List<string> mformats = Model.Format.Split(',').ToList();
                foreach (var mformat in mformats)
                {
                    //mParentKey = Model.Document.Substring(6, Model.Document.Length - 6);
                    mParentKey = Model.Document;
                    Model.StoreProcedure = ctxTFAT.DocFormats.Where(x => x.FormatCode.Trim() == mformat.Trim()).Select(x => x.StoredProc).FirstOrDefault();
                    DataTable dtreport = new DataTable();
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("SPDoc_" + Model.StoreProcedure, tfat_conx); //name of the storedprocedure
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@mParentKey", mParentKey);
                    cmd.Parameters.AddWithValue("@mBranch", mbranchcode);
                    SqlDataAdapter adp = new SqlDataAdapter(cmd);
                    adp.Fill(dtreport);

                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/Reports"), mformat.Trim() + ".rpt"));
                    rd.SetDataSource(dtreport);
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
            }
            document.Close();

            if (String.IsNullOrEmpty(PDFName))
            {
                return File(ms.ToArray(), "application/PDF");
            }
            else
            {
                return File(ms.ToArray(), "application/PDF", PDFName + ".pdf");
            }
            //return File(ms.ToArray(), "application/PDF");

        }

        #endregion



    }
}