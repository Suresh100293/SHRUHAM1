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
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class TransactionClassificationController : BaseController
    {
        //private tfatEntities ctxTFAT = new tfatEntities();
        //private IBusinessCommon mBuss = new BusinessCommon();
        private int mlocation = 100001;
        //private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        //private string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        //private string muserid = System.Web.HttpContext.Current.Session["UserId"].ToString();
        //private string mperiod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        private int mnewrecordkey = 0;
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        private static string msubcodeof = "";

        #region GetLists
        public List<SelectListItem> GetWOFetchStyleList()
        {
            List<SelectListItem> CallWOFetchStyleList = new List<SelectListItem>();
            CallWOFetchStyleList.Add(new SelectListItem { Value = "0", Text = "WO Stage (Routing Required)" });
            CallWOFetchStyleList.Add(new SelectListItem { Value = "1", Text = "Link to FG Item" });
            CallWOFetchStyleList.Add(new SelectListItem { Value = "2", Text = "Detailed Material" });
            return CallWOFetchStyleList;
        }
        public List<SelectListItem> GetSkipStockList()
        {
            List<SelectListItem> CallSkipStockList = new List<SelectListItem>();
            CallSkipStockList.Add(new SelectListItem { Value = "Y", Text = "Yes" });
            CallSkipStockList.Add(new SelectListItem { Value = "N", Text = "No" });
            return CallSkipStockList;
        }
        public List<SelectListItem> GetNegWarnList()
        {
            List<SelectListItem> CallNegWarnList = new List<SelectListItem>();
            CallNegWarnList.Add(new SelectListItem { Value = "0", Text = "Warning Only" });
            CallNegWarnList.Add(new SelectListItem { Value = "1", Text = "Restrict Entry" });
            CallNegWarnList.Add(new SelectListItem { Value = "2", Text = "Mark for Approval" });
            return CallNegWarnList;
        }
        public List<SelectListItem> GetvAutoList()
        {
            List<SelectListItem> CallvAutoList = new List<SelectListItem>();
            CallvAutoList.Add(new SelectListItem { Value = "A", Text = "Auto" });
            CallvAutoList.Add(new SelectListItem { Value = "M", Text = "Manual" });
            return CallvAutoList;
        }
        public List<SelectListItem> GetWOInterfaceList()
        {
            List<SelectListItem> CallWOInterfaceList = new List<SelectListItem>();
            CallWOInterfaceList.Add(new SelectListItem { Value = "1", Text = "1 - Job Contracting" });
            CallWOInterfaceList.Add(new SelectListItem { Value = "0", Text = "0 - Standard" });
            return CallWOInterfaceList;
        }

        public List<SelectListItem> GetWOStyleList()
        {
            List<SelectListItem> CallWOInterfaceList = new List<SelectListItem>();

            CallWOInterfaceList.Add(new SelectListItem { Value = "0", Text = "Sending For Jobwork" });
            CallWOInterfaceList.Add(new SelectListItem { Value = "1", Text = "Doing Jobwork" });
            return CallWOInterfaceList;
        }
        public List<SelectListItem> GetCurrConvList()
        {
            List<SelectListItem> CallCurrConvList = new List<SelectListItem>();
            CallCurrConvList.Add(new SelectListItem { Value = "Y", Text = "Y-Import / Export" });
            CallCurrConvList.Add(new SelectListItem { Value = "N", Text = "N-Domestic" });
            return CallCurrConvList;
        }
        public List<SelectListItem> GetCircularsList()
        {
            List<SelectListItem> CallCircularsList = new List<SelectListItem>();
            CallCircularsList.Add(new SelectListItem { Value = "0", Text = "0-Never follow PriceList (Disabled)" });
            CallCircularsList.Add(new SelectListItem { Value = "1", Text = "1-Follow PriceList first (Top Most Priority)" });
            CallCircularsList.Add(new SelectListItem { Value = "2", Text = "2-Follow PriceList Last (Least Priority)" });
            CallCircularsList.Add(new SelectListItem { Value = "3", Text = "3-Follow only PriceList (Pricelist is Compulsary)" });
            return CallCircularsList;
        }
        public List<SelectListItem> GetGSTTypeList()
        {
            List<SelectListItem> CallGSTTypeList = new List<SelectListItem>();
            CallGSTTypeList.Add(new SelectListItem { Value = "0", Text = "0-Tax Invoice" });
            CallGSTTypeList.Add(new SelectListItem { Value = "1", Text = "1-Reverse Charge" });
            CallGSTTypeList.Add(new SelectListItem { Value = "2", Text = "2-TCS" });
            CallGSTTypeList.Add(new SelectListItem { Value = "3", Text = "3-TDS" });
            CallGSTTypeList.Add(new SelectListItem { Value = "4", Text = "4-Bill of Supply" });
            CallGSTTypeList.Add(new SelectListItem { Value = "5", Text = "5-Export (under LUT)" });
            CallGSTTypeList.Add(new SelectListItem { Value = "6", Text = "6-UnRegistered Dealer Purchase" });
            CallGSTTypeList.Add(new SelectListItem { Value = "7", Text = "7-SEZ with Payment" });
            CallGSTTypeList.Add(new SelectListItem { Value = "8", Text = "8-SEZ w/o Payment" });
            CallGSTTypeList.Add(new SelectListItem { Value = "9", Text = "9-Export (with Duty Payment)" });
            CallGSTTypeList.Add(new SelectListItem { Value = "10", Text = "10-Exempted" });
            CallGSTTypeList.Add(new SelectListItem { Value = "11", Text = "11-No GST" });
            CallGSTTypeList.Add(new SelectListItem { Value = "12", Text = "12-Composition Dealer" });
            CallGSTTypeList.Add(new SelectListItem { Value = "13", Text = "13-Deemed Exports" });
            CallGSTTypeList.Add(new SelectListItem { Value = "14", Text = "14-NIL Rated" });
            CallGSTTypeList.Add(new SelectListItem { Value = "15", Text = "15-Export @0.1%" });
            CallGSTTypeList.Add(new SelectListItem { Value = "16", Text = "16-Export @0.05%" });
            return CallGSTTypeList;
        }
        public List<SelectListItem> GetBasedTypeList()
        {
            List<SelectListItem> CallBasedTypeList = new List<SelectListItem>();
            CallBasedTypeList.Add(new SelectListItem { Value = "Q", Text = "Q-Qty Based" });
            CallBasedTypeList.Add(new SelectListItem { Value = "V", Text = "V-Value Based" });
            CallBasedTypeList.Add(new SelectListItem { Value = "M", Text = "M-Volume Based" });
            CallBasedTypeList.Add(new SelectListItem { Value = "W", Text = "W-Weight Based" });
            return CallBasedTypeList;
        }

        private List<SelectListItem> GetBranchList()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM TfatBranch where Category='Branch' or Category='SubBranch' ";
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



        public List<SelectListItem> GetSourceDocList()
        {
            List<SelectListItem> CallSourceDocList = new List<SelectListItem>();
            CallSourceDocList.Add(new SelectListItem { Value = "Indents", Text = "Indents" });
            CallSourceDocList.Add(new SelectListItem { Value = "Enquiry", Text = "Enquiry" });
            CallSourceDocList.Add(new SelectListItem { Value = "Quotation", Text = "Quotation" });
            CallSourceDocList.Add(new SelectListItem { Value = "Orders", Text = "Orders" });
            CallSourceDocList.Add(new SelectListItem { Value = "Challans", Text = "Challans" });
            CallSourceDocList.Add(new SelectListItem { Value = "Invoice", Text = "Invoice" });
            CallSourceDocList.Add(new SelectListItem { Value = "Packing Slips", Text = "Packing Slips" });
            CallSourceDocList.Add(new SelectListItem { Value = "Pick Lists", Text = "Pick Lists" });
            CallSourceDocList.Add(new SelectListItem { Value = "Proforma Invoice", Text = "Proforma Invoice" });
            CallSourceDocList.Add(new SelectListItem { Value = "Purchase", Text = "Purchase" });
            CallSourceDocList.Add(new SelectListItem { Value = "Inter-Branch Doc.", Text = "Inter-Branch Doc." });
            CallSourceDocList.Add(new SelectListItem { Value = "Sub-Contract", Text = "Sub-Contract" });
            CallSourceDocList.Add(new SelectListItem { Value = "Production", Text = "Production" });
            CallSourceDocList.Add(new SelectListItem { Value = "Blanket Order", Text = "Blanket Order" });
            CallSourceDocList.Add(new SelectListItem { Value = "Inter-Branch Doc.", Text = "Inter-Branch Doc." });
            return CallSourceDocList;
        }

        public JsonResult AutoCompleteRefType(string term)
        {
            return Json((from m in ctxTFAT.DocTypes
                         where m.MainType != m.SubType && m.Code != m.SubType
                         where m.AppBranch.Contains(mbranchcode) && m.Name.ToLower().Contains(term.ToLower())
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
                         where m.MainType == mP2 && (m.Code.ToLower().Contains(mP1) || m.Name.ToLower().Contains(mP1))
                         select new { Name = "[" + m.Code + "] " + m.Name, m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompletePDFFormat(string term)
        {
            return Json((from m in ctxTFAT.TfatFormats
                         where m.Code.ToLower().Contains(term.ToLower())
                         select new { Name = m.Code, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteConstant(string term)
        {
            return Json((from m in ctxTFAT.Master
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteStore(string term)
        {
            return Json((from m in ctxTFAT.Stores
                         where m.AppBranch.Contains(mbranchcode) && m.Name.ToLower().Contains(term.ToLower()) && m.Code != m.Grp && m.Flag != "G"
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteAppBranch(string term)
        {
            return Json((from m in ctxTFAT.TfatBranch
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { m.Name, m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteLocationCode(string term)
        {
            return Json((from m in ctxTFAT.Warehouse
                         where m.Name.ToLower().Contains(term.ToLower()) && m.Flag != "G"
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteReasonCategory(string term)
        {
            return Json((from m in ctxTFAT.ReasonCategory
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteCashAcc(string term)
        {
            return Json((from m in ctxTFAT.Master
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteChequeAcc(string term)
        {
            return Json((from m in ctxTFAT.Master
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteCreditCardAcc(string term)
        {
            return Json((from m in ctxTFAT.Master
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteFromStore(string term)
        {
            return Json((from m in ctxTFAT.Stores
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteToStore(string term)
        {
            return Json((from m in ctxTFAT.Stores
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteCurrName(string term)
        {
            return Json((from m in ctxTFAT.CurrencyMaster
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteRejStore(string term)
        {
            return Json((from m in ctxTFAT.Stores
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteGoodStore(string term)
        {
            return Json((from m in ctxTFAT.Stores
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteQCGTRType(string term)
        {
            return Json((from m in ctxTFAT.DocTypes
                         where m.AppBranch.Contains(mbranchcode) && m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteCategory(string term)
        {
            return Json((from m in ctxTFAT.PartyCategory
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetGSTDocTypeList()
        {
            List<SelectListItem> CallWOInterfaceList = new List<SelectListItem>();
            CallWOInterfaceList.Add(new SelectListItem { Value = "0", Text = "Taxable Normal Calculation" });
            CallWOInterfaceList.Add(new SelectListItem { Value = "1", Text = "Taxable Direct Input" });
            return Json(CallWOInterfaceList, JsonRequestBehavior.AllowGet);
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
                    cmd.Parameters.Add("@mSumString", SqlDbType.VarChar, 5000).Value = "";
                    cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";

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

        public ActionResult Index1(TransactionClassificationVM mModel)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "","D");
            mdocument = mModel.Document;
            mModel.WOFetchStyleList = GetWOFetchStyleList();
            mModel.SkipStockList = GetSkipStockList();
            mModel.NegWarnList = GetNegWarnList();
            mModel.vAutoList = GetvAutoList();
            mModel.WOInterfaceList = GetWOInterfaceList();
            mModel.WOStyleList = GetWOStyleList();
            mModel.CurrConvList = GetCurrConvList();
            mModel.CircularsList = GetCircularsList();
            mModel.GSTTypeList = GetGSTTypeList();
            mModel.BasedTypeList = GetBasedTypeList();
            mModel.Branches = GetBranchList();
            mModel.SourceDocList = GetSourceDocList();
            mModel.DocTypes_Code = mModel.Document;

            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete") || mModel.Mode == "Copy")
            {

                var mList = ctxTFAT.DocTypes.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    var mRefType = ctxTFAT.DocTypes.Where(x => x.Code == mList.RefType).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mMainType = ctxTFAT.MainTypes.Where(x => x.Code == mList.MainType).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mSubType = ctxTFAT.SubTypes.Where(x => x.Code == mList.SubType).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mPDFFormat = ctxTFAT.TfatFormats.Where(x => x.Code == mList.PDFFormat).Select(x => new { Name = x.Code, Code = x.Code }).FirstOrDefault();
                    var mConstant = ctxTFAT.Master.Where(x => x.Code == mList.Constant).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mStore = ctxTFAT.Stores.Where(x => x.Code == mList.Store).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mAppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mList.AppBranch).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mLocationCode = ctxTFAT.Warehouse.Where(x => x.Code == mList.LocationCode).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mReasonCategory = ctxTFAT.ReasonCategory.Where(x => x.Code == mList.ReasonCategory).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mCashAcc = ctxTFAT.Master.Where(x => x.Code == mList.CashAcc).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mChequeAcc = ctxTFAT.Master.Where(x => x.Code == mList.ChequeAcc).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mCreditCardAcc = ctxTFAT.Master.Where(x => x.Code == mList.CreditCardAcc).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mFromStore = ctxTFAT.Stores.Where(x => x.Code == mList.FromStore).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mToStore = ctxTFAT.Stores.Where(x => x.Code == mList.ToStore).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mCurrName = ctxTFAT.CurrencyMaster.Where(x => x.Code == mList.CurrName).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mRejStore = ctxTFAT.Stores.Where(x => x.Code == mList.RejStore).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mGoodStore = ctxTFAT.Stores.Where(x => x.Code == mList.GoodStore).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mQCGTRType = ctxTFAT.DocTypes.Where(x => x.Code == mList.QCGTRType).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mCategory = ctxTFAT.PartyCategory.Where(x => x.Code == mList.Category).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    mModel.DocTypes_RefType = mRefType != null ? mRefType.Code.ToString() : "";
                    mModel.RefTypeName = mRefType != null ? mRefType.Name : "";
                    mModel.DocTypes_MainType = mMainType != null ? mMainType.Code.ToString() : "";
                    mModel.MainTypeName = mMainType != null ? mMainType.Name : "";
                    mModel.DocTypes_SubType = mSubType != null ? mSubType.Code.ToString() : "";
                    mModel.SubTypeName = mSubType != null ? mSubType.Name : "";
                    mModel.DocTypes_PDFFormat = mPDFFormat != null ? mPDFFormat.Code.ToString() : "";
                    mModel.PDFFormatName = mPDFFormat != null ? mPDFFormat.Name : "";
                    mModel.DocTypes_Constant = mConstant != null ? mConstant.Code.ToString() : "";
                    mModel.ConstantName = mConstant != null ? mConstant.Name : "";
                    mModel.DocTypes_Store = mStore != null ? mStore.Code : 0;
                    mModel.StoreName = mStore != null ? mStore.Name : "";
                    mModel.DocTypes_AppBranch = mAppBranch != null ? mAppBranch.Code.ToString() : "";
                    mModel.AppBranchName = mAppBranch != null ? mAppBranch.Name : "";
                    mModel.DocTypes_LocationCode = mLocationCode != null ? mLocationCode.Code : 0;
                    mModel.LocationCodeName = mLocationCode != null ? mLocationCode.Name : "";
                    mModel.DocTypes_ReasonCategory = mReasonCategory != null ? mReasonCategory.Code : 0;
                    mModel.ReasonCategoryName = mReasonCategory != null ? mReasonCategory.Name : "";
                    mModel.DocTypes_CashAcc = mCashAcc != null ? mCashAcc.Code.ToString() : "";
                    mModel.CashAccName = mCashAcc != null ? mCashAcc.Name : "";
                    mModel.DocTypes_ChequeAcc = mChequeAcc != null ? mChequeAcc.Code.ToString() : "";
                    mModel.ChequeAccName = mChequeAcc != null ? mChequeAcc.Name : "";
                    mModel.DocTypes_CreditCardAcc = mCreditCardAcc != null ? mCreditCardAcc.Code.ToString() : "";
                    mModel.CreditCardAccName = mCreditCardAcc != null ? mCreditCardAcc.Name : "";
                    mModel.DocTypes_FromStore = mFromStore != null ? mFromStore.Code : 0;
                    mModel.FromStoreName = mFromStore != null ? mFromStore.Name : "";
                    mModel.DocTypes_ToStore = mToStore != null ? mToStore.Code : 0;
                    mModel.ToStoreName = mToStore != null ? mToStore.Name : "";
                    mModel.DocTypes_CurrName = mCurrName != null ? mCurrName.Code : 0;
                    mModel.CurrNameName = mCurrName != null ? mCurrName.Name : "";
                    mModel.DocTypes_RejStore = mRejStore != null ? mRejStore.Code : 0;
                    mModel.RejStoreName = mRejStore != null ? mRejStore.Name : "";
                    mModel.DocTypes_GoodStore = mGoodStore != null ? mGoodStore.Code : 0;
                    mModel.GoodStoreName = mGoodStore != null ? mGoodStore.Name : "";
                    mModel.DocTypes_QCGTRType = mQCGTRType != null ? mQCGTRType.Code.ToString() : "";
                    mModel.QCGTRTypeName = mQCGTRType != null ? mQCGTRType.Name : "";
                    mModel.DocTypes_Category = mCategory != null ? mCategory.Code : 0;
                    mModel.CategoryName = mCategory != null ? mCategory.Name : "";
                    mModel.DocTypes_FetchWoNo = mList.FetchWoNo;
                    mModel.DocTypes_PrintControl = mList.PrintControl;
                    mModel.DocTypes_PostSave = mList.PostSave;
                    mModel.DocTypes_SkipStock = mList.SkipStock;
                    mModel.DocTypes_Code = mList.Code;
                    mModel.DocTypes_WOFetchStyle = (byte)(mList.WOFetchStyle != null ? mList.WOFetchStyle.Value : 0);
                    mModel.DocTypes_HideAddChg = mList.HideAddChg;
                    mModel.DocTypes_MaxPrints = mList.MaxPrints != null ? mList.MaxPrints.Value : 0;
                    mModel.DocTypes_PreSave = mList.PreSave;
                    mModel.DocTypes_DefaultNet = mList.DefaultNet != null ? mList.DefaultNet.Value : 0;
                    mModel.DocTypes_FetchWoDet = mList.FetchWoDet;
                    mModel.DocTypes_WONegative = mList.WONegative;
                    mModel.DocTypes_HideAddon = mList.HideAddon;
                    mModel.DocTypes_Name = mList.Name;
                    mModel.DocTypes_DiscFlag = mList.DiscFlag;
                    mModel.DocTypes_HideAmt = mList.HideAmt;
                    mModel.DocTypes_AutoGT = mList.AutoGT;
                    mModel.DocTypes_NegStock = mList.NegStock;
                    mModel.DocTypes_PartyDisc = mList.PartyDisc;
                    mModel.DocTypes_BulkCond = mList.BulkCond;
                    mModel.DocTypes_MakePDF = mList.MakePDF;
                    mModel.DocTypes_HideBatchSrl = mList.HideBatchSrl;
                    mModel.DocTypes_NonStock = mList.NonStock;
                    mModel.DocTypes_NegWarn = (byte)(mList.NegWarn != null ? mList.NegWarn.Value : 0);
                    mModel.DocTypes_PrintCond = mList.PrintCond;
                    mModel.DocTypes_HideDiscount = mList.HideDiscount;
                    mModel.DocTypes_AutoGTEntry = mList.AutoGTEntry;
                    mModel.DocTypes_ProductPost = mList.ProductPost;
                    mModel.DocTypes_DontCalcFactor = mList.DontCalcFactor;
                    mModel.DocTypes_HideNarr = mList.HideNarr;
                    mModel.DocTypes_RevStage = mList.RevStage;
                    mModel.DocTypes_AddCond = mList.AddCond;
                    mModel.DocTypes_TypeLock = mList.TypeLock;
                    mModel.DocTypes_RoundOff = mList.RoundOff;
                    mModel.DocTypes_ExpandKIT = mList.ExpandKIT;
                    mModel.DocTypes_Formats = mList.Formats;
                    mModel.DocTypes_ForceOrder = mList.ForceOrder;
                    mModel.DocTypes_HideProcess = mList.HideProcess;
                    mModel.DocTypes_RoundTo = mList.RoundTo != null ? mList.RoundTo.Value : 0;
                    mModel.DocTypes_GSTNoCommon = mList.GSTNoCommon;
                    mModel.DocTypes_ForceIndents = mList.ForceIndents;
                    mModel.DocTypes_HideProduct = mList.HideProduct;
                    mModel.DocTypes_UseCR = mList.UseCR;
                    mModel.DocTypes_PrevQty = mList.PrevQty;
                    mModel.DocTypes_NoItemEdit = mList.NoItemEdit;
                    mModel.DocTypes_ItemAttach = mList.ItemAttach;
                    mModel.DocTypes_DataSource = mList.DataSource;
                    mModel.DocTypes_CommonSeries = mList.CommonSeries;
                    mModel.DocTypes_CommonSeriesCentralised = mList.Centralised;
                    mModel.DocTypes_HideReason = mList.HideReason;
                    mModel.DocTypes_RateOnMRP = mList.RateOnMRP;
                    mModel.DocTypes_SendAlert = mList.SendAlert;
                    mModel.DocTypes_DontGenIndent = mList.DontGenIndent;
                    mModel.DocTypes_PrintOnSave = mList.PrintOnSave;
                    mModel.DocTypes_EditCond = mList.EditCond;
                    mModel.DocTypes_InnerBrkp = mList.InnerBrkp;
                    mModel.DocTypes_HideQty = mList.HideQty;
                    mModel.DocTypes_FIFOOrder = mList.FIFOOrder;
                    mModel.DocTypes_QtyTole = mList.QtyTole;
                    mModel.DocTypes_CheckBillRef = mList.CheckBillRef;
                    mModel.DocTypes_NoPrintWindow = mList.NoPrintWindow;
                    mModel.DocTypes_HideQty2 = mList.HideQty2;
                    mModel.DocTypes_ShowStores = mList.ShowStores != null ? mList.ShowStores.Value : 0;
                    mModel.DocTypes_DelyWise = mList.DelyWise;
                    mModel.DocTypes_LockPosting = mList.LockPosting;
                    mModel.DocTypes_DontGenWO = mList.DontGenWO;
                    mModel.DocTypes_HideUnits = mList.HideUnits;
                    mModel.DocTypes_BINRequired = mList.BINRequired;
                    mModel.DocTypes_RequireTIN = mList.RequireTIN;
                    mModel.DocTypes_ValueOrder = mList.ValueOrder;
                    mModel.DocTypes_vAuto = mList.vAuto;
                    mModel.DocTypes_AllowZero = mList.AllowZero;
                    mModel.DocTypes_DeleteCond = mList.DeleteCond;
                    mModel.DocTypes_DontGenReserve = mList.DontGenReserve;
                    mModel.DocTypes_HideRate = mList.HideRate;
                    mModel.DocTypes_DocWidth = mList.DocWidth;
                    mModel.DocTypes_HideRatePer = mList.HideRatePer;
                    mModel.DocTypes_MoreQty = mList.MoreQty;
                    mModel.DocTypes_AllowNeg = mList.AllowNeg;
                    mModel.DocTypes_QCReqd = mList.QCReqd;
                    mModel.DocTypes_RequireHSN = mList.RequireHSN;
                    mModel.DocTypes_ReturnsType = mList.ReturnsType;
                    mModel.DocTypes_ExclConsWO = mList.ExclConsWO;
                    mModel.DocTypes_FOC = mList.FOC;
                    mModel.DocTypes_WOBatch = mList.WOBatch;
                    mModel.DocTypes_HideTax = mList.HideTax;
                    mModel.DocTypes_PrefixConst = mList.PrefixConst;
                    mModel.DocTypes_DCReqd = mList.DCReqd;
                    mModel.DocTypes_RequireTRNS = mList.RequireTRNS;
                    mModel.DocTypes_AllowStkAdjust = mList.AllowStkAdjust;
                    mModel.DocTypes_LastSerial = mList.LastSerial;
                    mModel.DocTypes_HideStore = mList.HideStore;
                    mModel.DocTypes_WOEditRM = mList.WOEditRM;
                    mModel.DocTypes_SubContCombine = mList.SubContCombine;
                    mModel.DocTypes_RequireAttach = mList.RequireAttach;
                    mModel.DocTypes_WOEditLoss = mList.WOEditLoss;
                    mModel.DocTypes_HideValue = mList.HideValue;
                    mModel.DocTypes_PickChlnRate = mList.PickChlnRate;
                    mModel.DocTypes_OrderEditParty = mList.OrderEditParty;
                    mModel.DocTypes_ExternalApp = mList.ExternalApp;
                    mModel.DocTypes_LimitFrom = mList.LimitFrom;
                    mModel.DocTypes_PickSOFromMember = mList.PickSOFromMember;
                    mModel.DocTypes_BarCode = mList.BarCode;
                    mModel.DocTypes_RequireCrLimit = mList.RequireCrLimit;
                    mModel.DocTypes_RequireCrPeriod = mList.RequireCrPeriod;
                    mModel.DocTypes_BarCodeStruct = mList.BarCodeStruct;
                    mModel.DocTypes_LimitTo = mList.LimitTo;
                    mModel.DocTypes_BrokerReqd = mList.BrokerReqd;
                    mModel.DocTypes_DontDlySchedule = mList.DontDlySchedule;
                    mModel.DocTypes_SalesmanReqd = mList.SalesmanReqd;
                    mModel.DocTypes_Projects = mList.Projects;
                    mModel.DocTypes_LockAddChg = mList.LockAddChg;
                    mModel.DocTypes_BudgetControl = mList.BudgetControl;
                    mModel.DocTypes_OrdTypes = mList.OrdTypes;
                    mModel.DocTypes_CurrConv = mList.CurrConv;
                    mModel.DocTypes_SOIndPend = mList.SOIndPend;
                    mModel.DocTypes_BrokerCalculate = mList.BrokerCalculate;
                    mModel.DocTypes_LockAddon = mList.LockAddon;
                    mModel.DocTypes_BudgetFlags = mList.BudgetFlags;
                    mModel.DocTypes_BlanketOrder = mList.BlanketOrder;
                    mModel.DocTypes_AllowCurr = mList.AllowCurr;
                    mModel.DocTypes_LockAmt = mList.LockAmt;
                    mModel.DocTypes_ConstantMode = mList.ConstantMode != null ? mList.ConstantMode.Value : 0;
                    mModel.DocTypes_Circulars = mList.Circulars != null ? mList.Circulars.Value : 0;
                    mModel.DocTypes_LockBatchSrl = mList.LockBatchSrl;
                    mModel.DocTypes_GSTFlag = mList.GSTFlag;
                    mModel.DocTypes_AutoReserve = mList.AutoReserve;
                    mModel.DocTypes_GSTType = mList.GSTType != null ? mList.GSTType.Value : 0;
                    mModel.DocTypes_DocBackward = mList.DocBackward;
                    mModel.DocTypes_OverReserve = mList.OverReserve;
                    mModel.DocTypes_LockDiscount = mList.LockDiscount;
                    mModel.DocTypes_ForceCirculars = mList.ForceCirculars;
                    mModel.DocTypes_ConsigneeStock = mList.ConsigneeStock;
                    mModel.DocTypes_LockNarr = mList.LockNarr;
                    mModel.DocTypes_GSTTypeChange = mList.GSTTypeChange;
                    mModel.DocTypes_DontSchemeAppl = mList.DontSchemeAppl;
                    mModel.DocTypes_InterBranch = mList.InterBranch;
                    mModel.DocTypes_SPAdj = mList.SPAdj;
                    mModel.DocTypes_DocReview = mList.DocReview;
                    mModel.DocTypes_LockProcess = mList.LockProcess;
                    mModel.DocTypes_ForceReason = mList.ForceReason;
                    mModel.DocTypes_PostSummary = mList.PostSummary;
                    mModel.DocTypes_LockProduct = mList.LockProduct;
                    mModel.DocTypes_BackDays = mList.BackDays != null ? mList.BackDays.Value : 0;
                    mModel.DocTypes_LockReason = mList.LockReason;
                    mModel.DocTypes_PostShow = mList.PostShow;
                    mModel.DocTypes_Terms = mList.Terms;
                    mModel.DocTypes_LockQty = mList.LockQty;
                    mModel.DocTypes_BasedType = mList.BasedType;
                    mModel.DocTypes_LockQty2 = mList.LockQty2;
                    mModel.DocTypes_LockUnits = mList.LockUnits;
                    mModel.DocTypes_LockRate = mList.LockRate;
                    mModel.DocTypes_NoDuplItems = mList.NoDuplItems;
                    mModel.DocTypes_CatRates = mList.CatRates;
                    mModel.DocTypes_LockRatePer = mList.LockRatePer;
                    mModel.DocTypes_SourceDoc = mList.SourceDoc;
                    mModel.DocTypes_LockTax = mList.LockTax;
                    mModel.DocTypes_DocAttach = mList.DocAttach;
                    mModel.DocTypes_LockStore = mList.LockStore;
                    mModel.DocTypes_LockValue = mList.LockValue;
                    mModel.DocTypes_AllowTerms = mList.AllowTerms;
                    mModel.DocTypes_JobCurrentWork = mList.JobCurrentWork;
                    mModel.DocTypes_AllowDraftSave = mList.AllowDraftSave;
                    mModel.DocTypes_LockFactor = mList.LockFactor;
                    mModel.DocTypes_HideFactor = mList.HideFactor;
                    mModel.DocTypes_HideHSN = mList.HideHSN;
                    mModel.DocTypes_LockHSN = mList.LockHSN;
                    mModel.DocTypes_MilestoneReqd = mList.MilestoneReqd;
                    mModel.DocTypes_ApprovalRequired = mList.ApprovalRequired;
                    mModel.DocTypes_PriceListReqd = mList.PricelistReqd;
                    mModel.CutTDS = mList.CutTDS;
                    mModel.GSTDocType = mList.GSTDocType == null ? (byte)0 : mList.GSTDocType.Value;
                }

                if (mModel.Mode == "Copy")
                {
                    mModel.Mode = "Add";
                    mModel.DocTypes_Code = "";
                }
            }
            else
            {
                mModel.DocTypes_AccountGroups = "";
                mModel.DocTypes_AddCond = "";
                mModel.DocTypes_AddVatPurchase = false;
                mModel.DocTypes_Agreement = 0;
                mModel.DocTypes_AllowCurr = true;
                mModel.DocTypes_AllowDraftSave = false;
                mModel.DocTypes_AllowNeg = false;
                mModel.DocTypes_AllowOrdDupl = false;
                mModel.DocTypes_AllowStkAdjust = false;
                mModel.DocTypes_AllowTerms = false;
                mModel.DocTypes_AllowZero = false;
                mModel.DocTypes_AppBranch = mbranchcode;
                mModel.DocTypes_AssCategory = "";
                mModel.DocTypes_AttachShow = false;
                mModel.DocTypes_AuditDetail = true;
                mModel.DocTypes_AuthReq = false;
                mModel.DocTypes_AutoChild = false;
                mModel.DocTypes_AutoGT = false;
                mModel.DocTypes_AutoGTEntry = false;
                mModel.DocTypes_AutoReserve = false;
                mModel.DocTypes_BackDays = 0;
                mModel.DocTypes_BackHrs = 0;
                mModel.DocTypes_BarCode = false;
                mModel.DocTypes_BarCodeStruct = "";
                mModel.DocTypes_BasedType = "";
                mModel.DocTypes_BatchReqd = false;
                mModel.DocTypes_BINRequired = false;
                mModel.DocTypes_BlanketOrder = false;
                mModel.DocTypes_BMRFormat = "";
                mModel.DocTypes_BPRFormat = "";
                mModel.DocTypes_BRCFormat = "";
                mModel.DocTypes_BrokerCalculate = false;
                mModel.DocTypes_BrokerReqd = false;
                mModel.DocTypes_BudgetControl = false;
                mModel.DocTypes_BudgetFlags = "";
                mModel.DocTypes_BulkCond = "";
                mModel.DocTypes_CallCenter = false;
                mModel.DocTypes_CashAcc = "";
                mModel.DocTypes_Category = 0;
                mModel.DocTypes_CatRates = false;
                mModel.DocTypes_CheckBillRef = false;
                mModel.DocTypes_ChequeAcc = "";
                mModel.DocTypes_Circulars = 0;
                mModel.DocTypes_COAFormat = "";
                mModel.DocTypes_Code = "";
                mModel.DocTypes_CommonSeries = false;
                mModel.DocTypes_CommonSeriesCentralised = false;
                mModel.DocTypes_CompCode = mcompcode;
                mModel.DocTypes_ConsigneeStock = false;
                mModel.DocTypes_Constant = "";
                mModel.DocTypes_ConstantMode = 0;
                mModel.DocTypes_CreditCardAcc = "";
                mModel.DocTypes_CRFormat = "";
                mModel.DocTypes_CurrConv = "";
                mModel.DocTypes_CurrName = 1;
                mModel.DocTypes_DataSource = "";
                mModel.DocTypes_DCCode = 0;
                mModel.DocTypes_DCReqd = false;
                mModel.DocTypes_DefaultNet = 0;
                mModel.DocTypes_DeleteCond = "";
                mModel.DocTypes_DelyWise = false;
                mModel.DocTypes_DiscFlag = false;
                mModel.DocTypes_DisplayOrder = 0;
                mModel.DocTypes_DocApprovalReq = false;
                mModel.DocTypes_DocAttach = false;
                mModel.DocTypes_DocBackward = false;
                mModel.DocTypes_DocReview = false;
                mModel.DocTypes_DocWidth = 6;
                mModel.DocTypes_DontCalcFactor = false;
                mModel.DocTypes_DontConsPendOrd = false;
                mModel.DocTypes_DontDlySchedule = false;
                mModel.DocTypes_DontGenIndent = false;
                mModel.DocTypes_DontGenReserve = false;
                mModel.DocTypes_DontGenWO = false;
                mModel.DocTypes_DontSchemeAppl = false;
                mModel.DocTypes_EditCond = "";
                mModel.DocTypes_ExclConsWO = false;
                mModel.DocTypes_ExpandKIT = false;
                mModel.DocTypes_ExternalApp = "";
                mModel.DocTypes_FasterOpr = false;
                mModel.DocTypes_FetchWoDet = false;
                mModel.DocTypes_FetchWoNo = false;
                mModel.DocTypes_FIFOOrder = false;
                mModel.DocTypes_FOC = false;
                mModel.DocTypes_ForceChln = false;
                mModel.DocTypes_ForceCirculars = false;
                mModel.DocTypes_ForceIndents = false;
                mModel.DocTypes_ForceOrder = false;
                mModel.DocTypes_ForceReason = false;
                mModel.DocTypes_Formats = "";
                mModel.DocTypes_FromBin = 0;
                mModel.DocTypes_FromStore = 0;
                mModel.DocTypes_GenMFG = false;
                mModel.DocTypes_GoodStore = 0;
                mModel.DocTypes_GrpKey = 0;
                mModel.DocTypes_GSIRate = 0;
                mModel.DocTypes_GSTFlag = false;
                mModel.DocTypes_GSTNoCommon = false;
                mModel.DocTypes_GSTNoITC = 0;
                mModel.DocTypes_GSTType = 0;
                mModel.DocTypes_DocBackward = false;
                mModel.DocTypes_GSTTypeChange = false;
                mModel.DocTypes_HideAddChg = false;
                mModel.DocTypes_HideAddon = false;
                mModel.DocTypes_HideAmt = false;
                mModel.DocTypes_HideBatchSrl = false;
                mModel.DocTypes_HideDiscount = false;
                mModel.DocTypes_HideNarr = false;
                mModel.DocTypes_HideProcess = false;
                mModel.DocTypes_HideProduct = false;
                mModel.DocTypes_HideQty = false;
                mModel.DocTypes_HideQty2 = false;
                mModel.DocTypes_HideRate = false;
                mModel.DocTypes_HideRatePer = false;
                mModel.DocTypes_HideReason = false;
                mModel.DocTypes_HideStore = false;
                mModel.DocTypes_HideTax = false;
                mModel.DocTypes_HideUnits = false;
                mModel.DocTypes_HideValue = false;
                mModel.DocTypes_HotKey = "";
                mModel.DocTypes_IndPending = false;
                mModel.DocTypes_InnerBrkp = false;
                mModel.DocTypes_InterBranch = false;
                mModel.DocTypes_ItemAttach = false;
                mModel.DocTypes_ItemGroups = "";
                mModel.DocTypes_JobCurrentWork = false;
                mModel.DocTypes_JobworkStyle = false;
                mModel.DocTypes_LastDate = System.DateTime.Now;
                mModel.DocTypes_LastPrefix = "";
                mModel.DocTypes_LastSerial = "";
                mModel.DocTypes_Level = 0;
                mModel.DocTypes_LimitFrom = "000001";
                mModel.DocTypes_LimitTo = "999999";
                mModel.DocTypes_LocationCode = mlocationcode;
                mModel.DocTypes_LockAddChg = false;
                mModel.DocTypes_LockAddon = false;
                mModel.DocTypes_LockAmt = false;
                mModel.DocTypes_LockBatchSrl = false;
                mModel.DocTypes_LockDiscount = false;
                mModel.DocTypes_LockNarr = false;
                mModel.DocTypes_LockPosting = false;
                mModel.DocTypes_LockProcess = false;
                mModel.DocTypes_LockProduct = false;
                mModel.DocTypes_LockQty = false;
                mModel.DocTypes_LockQty2 = false;
                mModel.DocTypes_LockRate = false;
                mModel.DocTypes_LockRatePer = false;
                mModel.DocTypes_LockReason = false;
                mModel.DocTypes_LockStore = false;
                mModel.DocTypes_LockTax = false;
                mModel.DocTypes_LockUnits = false;
                mModel.DocTypes_LockValue = false;
                mModel.DocTypes_MainType = "";
                mModel.DocTypes_MakePDF = false;
                mModel.DocTypes_MaximumDetail = 0;
                mModel.DocTypes_MaxPrints = 0;
                mModel.DocTypes_MfgMove = false;
                mModel.DocTypes_MfgType = 0;
                mModel.DocTypes_MilestoneReqd = false;
                mModel.DocTypes_MoreQty = false;
                mModel.DocTypes_MSDFormat = "";
                mModel.DocTypes_MultiLevelBOM = false;
                mModel.DocTypes_Name = "";
                mModel.DocTypes_NegStock = false;
                mModel.DocTypes_NegWarn = 0;
                mModel.DocTypes_NoDuplItems = false;
                mModel.DocTypes_NoItemEdit = false;
                mModel.DocTypes_NoITF = false;
                mModel.DocTypes_NonStock = false;
                mModel.DocTypes_NoOfEntries = 0;
                mModel.DocTypes_NoPrintWindow = false;
                mModel.DocTypes_NoReleaseAdj = false;
                mModel.DocTypes_OrderEditParty = false;
                mModel.DocTypes_OrdTypes = "";
                mModel.DocTypes_OverReserve = false;
                mModel.DocTypes_PartyDisc = false;
                mModel.DocTypes_PCCode = 0;
                mModel.DocTypes_PDFFormat = "";
                mModel.DocTypes_PickChlnRate = false;
                mModel.DocTypes_PickSOFromMember = false;
                mModel.DocTypes_PickSummary = false;
                mModel.DocTypes_PostSave = "";
                mModel.DocTypes_PostShow = false;
                mModel.DocTypes_PostSummary = false;
                mModel.DocTypes_PrefixAuto = 0;
                mModel.DocTypes_PrefixConst = "";
                mModel.DocTypes_PrefixStyle = 0;
                mModel.DocTypes_PreSave = "";
                mModel.DocTypes_PrevQty = false;
                mModel.DocTypes_PrintCond = "";
                mModel.DocTypes_PrintControl = false;
                mModel.DocTypes_PrintOnSave = false;
                mModel.DocTypes_ProcessCode = 0;
                mModel.DocTypes_ProductPost = false;
                mModel.DocTypes_PROFormat = "";
                mModel.DocTypes_Projects = false;
                mModel.DocTypes_QCGTRType = "";
                mModel.DocTypes_QCReqd = false;
                mModel.DocTypes_QtyTole = false;
                mModel.DocTypes_RateFlag = "";
                mModel.DocTypes_RateOnMRP = false;
                mModel.DocTypes_ReasonCategory = 0;
                mModel.DocTypes_RefDoc = "";
                mModel.DocTypes_RefType = "";
                mModel.DocTypes_RejStore = 0;
                mModel.DocTypes_RequireAttach = false;
                mModel.DocTypes_RequireCrLimit = false;
                mModel.DocTypes_RequireCrPeriod = false;
                mModel.DocTypes_RequireHSN = false;
                mModel.DocTypes_RequireTIN = false;
                mModel.DocTypes_RequireTRNS = false;
                mModel.DocTypes_ReturnsType = false;
                mModel.DocTypes_RevStage = false;
                mModel.DocTypes_RoundOff = false;
                mModel.DocTypes_RoundTax = false;
                mModel.DocTypes_RoundTo = 0;
                mModel.DocTypes_SalesmanReqd = false;
                mModel.DocTypes_SendAlert = false;
                mModel.DocTypes_Seq = 0;
                mModel.DocTypes_SerTaxMul = false;
                mModel.DocTypes_ShowStores = 0;
                mModel.DocTypes_SkipStock = "N";
                mModel.DocTypes_SOIndPend = false;
                mModel.DocTypes_SOIndPending = false;
                mModel.DocTypes_SOPFormat = "";
                mModel.DocTypes_SourceDoc = "";
                mModel.DocTypes_SPAdj = false;
                mModel.DocTypes_StockReserve = false;
                mModel.DocTypes_Store = 100002;
                mModel.DocTypes_SubContCombine = false;
                mModel.DocTypes_SubType = "";
                mModel.DocTypes_SuppInvoice = false;
                mModel.DocTypes_TaxablePost = false;
                mModel.DocTypes_Template = "";
                mModel.DocTypes_Terms = "";
                mModel.DocTypes_ToBin = 0;
                mModel.DocTypes_ToStore = 100002;
                mModel.DocTypes_TypeLock = false;
                mModel.DocTypes_UseCR = false;
                mModel.DocTypes_ValueOrder = false;
                mModel.DocTypes_vAuto = "";
                mModel.DocTypes_WOBatch = false;
                mModel.DocTypes_WOEditLoss = false;
                mModel.DocTypes_WOEditRM = false;
                mModel.DocTypes_WOFetchStyle = 0;
                mModel.DocTypes_WOFilterPro = false;
                mModel.DocTypes_WOInterface = 0;
                mModel.DocTypes_WOLockCapacity = false;
                mModel.DocTypes_WONegative = false;
                mModel.GSTDocType = (byte)0;
            }
            return View(mModel);
        }

        private void CreateCharges(string mType, string mMainType, string mauthorise)
        {
            string msubtype = ctxTFAT.DocTypes.Where(x => x.Code == mType).Select(x => x.SubType).FirstOrDefault();
            var mList = ctxTFAT.Charges.Where(x => x.Type == mType).Select(x => x).ToList();
            ctxTFAT.Charges.RemoveRange(mList);
            if (mMainType == "PR")
            {
                var charges = ctxTFAT.Charges.Where(x => x.Type == "PUR00").Select(x => x).ToList();
                foreach (var item in charges)
                {
                    Charges mobj = new Charges();
                    mobj.Type = mType;
                    mobj.Fld = item.Fld;
                    mobj.Head = item.Head;
                    mobj.AfterTax = item.AfterTax;
                    mobj.RoundOff = item.RoundOff;
                    mobj.EqAmt = item.EqAmt;
                    mobj.EqCost = item.EqCost;
                    mobj.EqSale = item.EqSale;
                    mobj.EqBro = item.EqBro;
                    mobj.EqTax = item.EqTax;
                    mobj.Equation = item.Equation;
                    mobj.Post = item.Post;
                    mobj.Code = item.Code;
                    mobj.HSNCode = item.HSNCode;
                    mobj.PostNo = item.PostNo;
                    mobj.TaxCode = item.TaxCode;
                    mobj.ToPrint = item.ToPrint;
                    // iX9: default values for the fields not used @Form
                    mobj.CompCode = "";
                    mobj.DontUse = false;
                    mobj.MainType = mMainType;
                    mobj.SubType = msubtype;
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    ctxTFAT.Charges.Add(mobj);
                    ctxTFAT.SaveChanges();
                }
            }

            if (mMainType == "SL")
            {
                var charges = ctxTFAT.Charges.Where(x => x.Type == "SAL00").Select(x => x).ToList();
                foreach (var item in charges)
                {
                    Charges mobj = new Charges();
                    mobj.Type = mType;
                    mobj.Fld = item.Fld;
                    mobj.Head = item.Head;
                    mobj.AfterTax = item.AfterTax;
                    mobj.RoundOff = item.RoundOff;
                    mobj.EqAmt = item.EqAmt;
                    mobj.EqCost = item.EqCost;
                    mobj.EqSale = item.EqSale;
                    mobj.EqBro = item.EqBro;
                    mobj.EqTax = item.EqTax;
                    mobj.Equation = item.Equation;
                    mobj.Post = item.Post;
                    mobj.Code = item.Code;
                    mobj.HSNCode = item.HSNCode;
                    mobj.PostNo = item.PostNo;
                    mobj.TaxCode = item.TaxCode;
                    mobj.ToPrint = item.ToPrint;
                    // iX9: default values for the fields not used @Form
                    mobj.CompCode = "";
                    mobj.DontUse = false;
                    mobj.MainType = mMainType;
                    mobj.SubType = msubtype;
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    ctxTFAT.Charges.Add(mobj);
                    ctxTFAT.SaveChanges();
                }
            }
        }

        #region SaveData
        public ActionResult SaveData(TransactionClassificationVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteTransactionClassification(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.DocTypes_Code, DateTime.Now, 0, mModel.DocTypes_Code, "Delete Transaction Classification", "D");
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    DocTypes mobj = new DocTypes();
                    bool mAdd = true;
                    if (ctxTFAT.DocTypes.Where(x => (x.Code == mModel.DocTypes_Code)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.DocTypes.Where(x => (x.Code == mModel.DocTypes_Code)).FirstOrDefault();
                        mAdd = false;
                    }
                    mobj.FetchWoNo = mModel.DocTypes_FetchWoNo;
                    mobj.PrintControl = mModel.DocTypes_PrintControl;
                    mobj.RefType = mModel.DocTypes_RefType;
                    mobj.PostSave = mModel.DocTypes_PostSave;
                    mobj.SkipStock = mModel.DocTypes_SkipStock;
                    mobj.Code = mModel.DocTypes_Code;
                    mobj.WOFetchStyle = mModel.DocTypes_WOFetchStyle;
                    mobj.HideAddChg = mModel.DocTypes_HideAddChg;
                    mobj.MaxPrints = mModel.DocTypes_MaxPrints;
                    mobj.PreSave = mModel.DocTypes_PreSave;
                    mobj.DefaultNet = mModel.DocTypes_DefaultNet;
                    mobj.FetchWoDet = mModel.DocTypes_FetchWoDet;
                    mobj.WONegative = mModel.DocTypes_WONegative;
                    mobj.HideAddon = mModel.DocTypes_HideAddon;
                    mobj.Name = mModel.DocTypes_Name;
                    mobj.DiscFlag = mModel.DocTypes_DiscFlag;
                    mobj.HideAmt = mModel.DocTypes_HideAmt;
                    mobj.AutoGT = mModel.DocTypes_AutoGT;
                    mobj.NegStock = mModel.DocTypes_NegStock;
                    mobj.PartyDisc = mModel.DocTypes_PartyDisc;
                    mobj.MainType = mModel.DocTypes_MainType;
                    mobj.BulkCond = mModel.DocTypes_BulkCond;
                    mobj.MakePDF = mModel.DocTypes_MakePDF;
                    mobj.HideBatchSrl = mModel.DocTypes_HideBatchSrl;
                    mobj.NonStock = mModel.DocTypes_NonStock;
                    mobj.NegWarn = mModel.DocTypes_NegWarn;
                    mobj.PrintCond = mModel.DocTypes_PrintCond;
                    mobj.SubType = mModel.DocTypes_SubType;
                    mobj.HideDiscount = mModel.DocTypes_HideDiscount;
                    mobj.AutoGTEntry = mModel.DocTypes_AutoGTEntry;
                    mobj.ProductPost = mModel.DocTypes_ProductPost;
                    mobj.PDFFormat = mModel.DocTypes_PDFFormat;
                    mobj.DontCalcFactor = mModel.DocTypes_DontCalcFactor;
                    mobj.HideNarr = mModel.DocTypes_HideNarr;
                    mobj.RevStage = mModel.DocTypes_RevStage;
                    mobj.AddCond = mModel.DocTypes_AddCond;
                    mobj.TypeLock = mModel.DocTypes_TypeLock;
                    mobj.RoundOff = mModel.DocTypes_RoundOff;
                    mobj.ExpandKIT = mModel.DocTypes_ExpandKIT;
                    mobj.Formats = mModel.DocTypes_Formats;
                    mobj.ForceOrder = mModel.DocTypes_ForceOrder;
                    mobj.HideProcess = mModel.DocTypes_HideProcess;
                    mobj.RoundTo = mModel.DocTypes_RoundTo;
                    mobj.GSTNoCommon = mModel.DocTypes_GSTNoCommon;
                    mobj.ForceIndents = mModel.DocTypes_ForceIndents;
                    mobj.HideProduct = mModel.DocTypes_HideProduct;
                    mobj.UseCR = mModel.DocTypes_UseCR;
                    mobj.PrevQty = mModel.DocTypes_PrevQty;
                    mobj.NoItemEdit = mModel.DocTypes_NoItemEdit;
                    mobj.ItemAttach = mModel.DocTypes_ItemAttach;
                    mobj.DataSource = mModel.DocTypes_DataSource;
                    mobj.CommonSeries = mModel.DocTypes_CommonSeries;
                    mobj.Centralised = mModel.DocTypes_CommonSeriesCentralised;
                    mobj.Constant = mModel.DocTypes_Constant;
                    mobj.HideReason = mModel.DocTypes_HideReason;
                    mobj.RateOnMRP = mModel.DocTypes_RateOnMRP;
                    mobj.SendAlert = mModel.DocTypes_SendAlert;
                    mobj.DontGenIndent = mModel.DocTypes_DontGenIndent;
                    mobj.Store = mModel.DocTypes_Store;
                    mobj.PrintOnSave = mModel.DocTypes_PrintOnSave;
                    mobj.EditCond = mModel.DocTypes_EditCond;
                    mobj.InnerBrkp = mModel.DocTypes_InnerBrkp;
                    mobj.AppBranch = mModel.DocTypes_AppBranch;
                    mobj.HideQty = mModel.DocTypes_HideQty;
                    mobj.FIFOOrder = mModel.DocTypes_FIFOOrder;
                    mobj.QtyTole = mModel.DocTypes_QtyTole;
                    mobj.CheckBillRef = mModel.DocTypes_CheckBillRef;
                    mobj.NoPrintWindow = mModel.DocTypes_NoPrintWindow;
                    mobj.LocationCode = 100000;
                    mobj.HideQty2 = mModel.DocTypes_HideQty2;
                    mobj.ShowStores = mModel.DocTypes_ShowStores;
                    mobj.DelyWise = mModel.DocTypes_DelyWise;
                    mobj.LockPosting = mModel.DocTypes_LockPosting;
                    mobj.DontGenWO = mModel.DocTypes_DontGenWO;
                    mobj.HideUnits = mModel.DocTypes_HideUnits;
                    mobj.BINRequired = mModel.DocTypes_BINRequired;
                    mobj.RequireTIN = mModel.DocTypes_RequireTIN;
                    mobj.ValueOrder = mModel.DocTypes_ValueOrder;
                    mobj.vAuto = mModel.DocTypes_vAuto;
                    mobj.AllowZero = mModel.DocTypes_AllowZero;
                    mobj.DeleteCond = mModel.DocTypes_DeleteCond;
                    mobj.DontGenReserve = mModel.DocTypes_DontGenReserve;
                    mobj.HideRate = mModel.DocTypes_HideRate;
                    mobj.DocWidth = mModel.DocTypes_DocWidth;
                    mobj.ProcessCode = mModel.DocTypes_ProcessCode;
                    mobj.HideRatePer = mModel.DocTypes_HideRatePer;
                    mobj.MoreQty = mModel.DocTypes_MoreQty;
                    mobj.AllowNeg = mModel.DocTypes_AllowNeg;
                    mobj.QCReqd = mModel.DocTypes_QCReqd;
                    mobj.RequireHSN = mModel.DocTypes_RequireHSN;
                    mobj.ReturnsType = mModel.DocTypes_ReturnsType;
                    mobj.ExclConsWO = mModel.DocTypes_ExclConsWO;
                    mobj.ReasonCategory = mModel.DocTypes_ReasonCategory;
                    mobj.FOC = mModel.DocTypes_FOC;
                    mobj.WOBatch = mModel.DocTypes_WOBatch;
                    mobj.HideTax = mModel.DocTypes_HideTax;
                    mobj.PrefixConst = mModel.DocTypes_PrefixConst;
                    mobj.DCReqd = mModel.DocTypes_DCReqd;
                    mobj.RequireTRNS = mModel.DocTypes_RequireTRNS;
                    mobj.AllowStkAdjust = mModel.DocTypes_AllowStkAdjust;
                    mobj.DCCode = mModel.DocTypes_DCCode;
                    mobj.LastSerial = mModel.DocTypes_LastSerial;
                    mobj.HideStore = mModel.DocTypes_HideStore;
                    mobj.WOEditRM = mModel.DocTypes_WOEditRM;
                    mobj.SubContCombine = mModel.DocTypes_SubContCombine;
                    mobj.RequireAttach = mModel.DocTypes_RequireAttach;
                    mobj.WOEditLoss = mModel.DocTypes_WOEditLoss;
                    mobj.HideValue = mModel.DocTypes_HideValue;
                    mobj.PickChlnRate = mModel.DocTypes_PickChlnRate;
                    mobj.CashAcc = mModel.DocTypes_CashAcc;
                    mobj.OrderEditParty = mModel.DocTypes_OrderEditParty;
                    mobj.ExternalApp = mModel.DocTypes_ExternalApp;
                    mobj.LimitFrom = mModel.DocTypes_LimitFrom;
                    mobj.PickSOFromMember = mModel.DocTypes_PickSOFromMember;
                    mobj.BarCode = mModel.DocTypes_BarCode;
                    mobj.RequireCrLimit = mModel.DocTypes_RequireCrLimit;
                    mobj.RequireCrPeriod = mModel.DocTypes_RequireCrPeriod;
                    mobj.ChequeAcc = mModel.DocTypes_ChequeAcc;
                    mobj.BarCodeStruct = mModel.DocTypes_BarCodeStruct;
                    mobj.LimitTo = mModel.DocTypes_LimitTo;
                    mobj.BrokerReqd = mModel.DocTypes_BrokerReqd;
                    mobj.DontDlySchedule = mModel.DocTypes_DontDlySchedule;
                    mobj.CreditCardAcc = mModel.DocTypes_CreditCardAcc;
                    mobj.SalesmanReqd = mModel.DocTypes_SalesmanReqd;
                    mobj.Projects = mModel.DocTypes_Projects;
                    mobj.LockAddChg = mModel.DocTypes_LockAddChg;
                    mobj.BudgetControl = mModel.DocTypes_BudgetControl;
                    mobj.FromStore = mModel.DocTypes_FromStore;
                    mobj.OrdTypes = mModel.DocTypes_OrdTypes;
                    mobj.CurrConv = mModel.DocTypes_CurrConv;
                    mobj.SOIndPend = mModel.DocTypes_SOIndPend;
                    mobj.WOInterface = (mModel.DocTypes_WOInterface == 0) ? false : true;
                    mobj.ToStore = mModel.DocTypes_ToStore;
                    mobj.BrokerCalculate = mModel.DocTypes_BrokerCalculate;
                    mobj.CurrName = mModel.DocTypes_CurrName;
                    mobj.JobworkStyle = mModel.DocTypes_JobworkStyle ;
                    mobj.LockAddon = mModel.DocTypes_LockAddon;
                    mobj.BudgetFlags = mModel.DocTypes_BudgetFlags;
                    mobj.BlanketOrder = mModel.DocTypes_BlanketOrder;
                    mobj.AllowCurr = mModel.DocTypes_AllowCurr;
                    mobj.LockAmt = mModel.DocTypes_LockAmt;
                    mobj.ConstantMode = mModel.DocTypes_ConstantMode;
                    mobj.FromBin = mModel.DocTypes_FromBin;
                    mobj.Circulars = mModel.DocTypes_Circulars;
                    mobj.LockBatchSrl = mModel.DocTypes_LockBatchSrl;
                    mobj.GSTFlag = mModel.DocTypes_GSTFlag;
                    mobj.AutoReserve = mModel.DocTypes_AutoReserve;
                    mobj.GSTType = mModel.DocTypes_GSTType;
                    mobj.DocBackward = mModel.DocTypes_DocBackward;
                    mobj.OverReserve = mModel.DocTypes_OverReserve;
                    mobj.LockDiscount = mModel.DocTypes_LockDiscount;
                    mobj.ForceCirculars = mModel.DocTypes_ForceCirculars;
                    mobj.ConsigneeStock = mModel.DocTypes_ConsigneeStock;
                    mobj.ToBin = mModel.DocTypes_ToBin;
                    mobj.LockNarr = mModel.DocTypes_LockNarr;
                    mobj.GSTTypeChange = mModel.DocTypes_GSTTypeChange;
                    mobj.DontSchemeAppl = mModel.DocTypes_DontSchemeAppl;
                    mobj.InterBranch = mModel.DocTypes_InterBranch;
                    mobj.SPAdj = mModel.DocTypes_SPAdj;
                    mobj.RejStore = mModel.DocTypes_RejStore;
                    mobj.DocReview = mModel.DocTypes_DocReview;
                    mobj.LockProcess = mModel.DocTypes_LockProcess;
                    mobj.ForceReason = mModel.DocTypes_ForceReason;
                    mobj.PostSummary = mModel.DocTypes_PostSummary;
                    mobj.GoodStore = mModel.DocTypes_GoodStore;
                    mobj.LockProduct = mModel.DocTypes_LockProduct;
                    mobj.BackDays = mModel.DocTypes_BackDays;
                    mobj.LockReason = mModel.DocTypes_LockReason;
                    mobj.PostShow = mModel.DocTypes_PostShow;
                    mobj.Terms = mModel.DocTypes_Terms;
                    mobj.LockQty = mModel.DocTypes_LockQty;
                    mobj.BasedType = mModel.DocTypes_BasedType;
                    mobj.LockQty2 = mModel.DocTypes_LockQty2;
                    mobj.QCGTRType = mModel.DocTypes_QCGTRType;
                    mobj.LockUnits = mModel.DocTypes_LockUnits;
                    mobj.LockRate = mModel.DocTypes_LockRate;
                    mobj.NoDuplItems = mModel.DocTypes_NoDuplItems;
                    mobj.CatRates = mModel.DocTypes_CatRates;
                    mobj.LockRatePer = mModel.DocTypes_LockRatePer;
                    mobj.Category = mModel.DocTypes_Category;
                    mobj.SourceDoc = mModel.DocTypes_SourceDoc;
                    mobj.LockTax = mModel.DocTypes_LockTax;
                    mobj.DocAttach = mModel.DocTypes_DocAttach;
                    mobj.LockStore = mModel.DocTypes_LockStore;
                    mobj.LockValue = mModel.DocTypes_LockValue;
                    mobj.AllowTerms = mModel.DocTypes_AllowTerms;
                    mobj.JobCurrentWork = mModel.DocTypes_JobCurrentWork;
                    // iX9: default values for the fields not used @Form
                    mobj.AccountGroups = "";
                    mobj.AddVatPurchase = false;
                    mobj.Agreement = 0;
                    mobj.AllowDraftSave = mModel.DocTypes_AllowDraftSave;
                    mobj.AllowOrdDupl = false;
                    mobj.AssCategory = "";
                    mobj.AttachShow = false;
                    mobj.AuditDetail = false;
                    mobj.AuthReq = false;
                    mobj.AutoChild = false;
                    mobj.BackHrs = 0;
                    mobj.BatchReqd = false;
                    mobj.BMRFormat = "";
                    mobj.BPRFormat = "";
                    mobj.BRCFormat = "";
                    mobj.CallCenter = false;
                    mobj.COAFormat = "";
                    mobj.CompCode = "";
                    mobj.CRFormat = "";
                    mobj.DisplayOrder = 0;
                    mobj.DocApprovalReq = false;
                    mobj.DontConsPendOrd = false;
                    mobj.FasterOpr = false;
                    mobj.ForceChln = false;
                    mobj.GenMFG = false;
                    mobj.GrpKey = 0;
                    mobj.GSIRate = 0;
                    mobj.GSTNoITC = 0;
                    mobj.HotKey = "";
                    mobj.IndPending = false;
                    mobj.ItemGroups = "";
                    mobj.LastDate = System.DateTime.Now;
                    mobj.LastPrefix = "";
                    mobj.Level = 0;
                    mobj.MaximumDetail = 0;
                    mobj.MfgMove = false;
                    mobj.MfgType = 0;
                    mobj.MilestoneReqd = false;
                    mobj.MSDFormat = "";
                    mobj.MultiLevelBOM = false;
                    mobj.NoITF = false;
                    mobj.NoOfEntries = 0;
                    mobj.NoReleaseAdj = false;
                    mobj.PCCode = 0;
                    mobj.PickSummary = false;
                    mobj.PrefixAuto = 0;
                    mobj.PrefixStyle = 0;
                    mobj.PROFormat = "";
                    mobj.RateFlag = "";
                    mobj.RefDoc = "";
                    mobj.RoundTax = false;
                    mobj.Seq = 0;
                    mobj.SerTaxMul = false;
                    mobj.SOIndPending = false;
                    mobj.SOPFormat = "";
                    mobj.StockReserve = false;
                    mobj.SuppInvoice = false;
                    mobj.TaxablePost = false;
                    mobj.Template = "";
                    mobj.WOFilterPro = false;
                    mobj.WOLockCapacity = false;
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    mobj.LockFactor = mModel.DocTypes_LockFactor;
                    mobj.HideFactor = mModel.DocTypes_HideFactor;
                    mobj.LockHSN = mModel.DocTypes_LockHSN;
                    mobj.HideHSN = mModel.DocTypes_HideHSN;
                    mobj.MilestoneReqd = mModel.DocTypes_MilestoneReqd;
                    mobj.ApprovalRequired = mModel.DocTypes_ApprovalRequired;
                    mobj.PricelistReqd = mModel.DocTypes_PriceListReqd;
                    mobj.CutTDS = mModel.CutTDS;
                    mobj.GSTDocType = mModel.GSTDocType;
                    if (mAdd == true)
                    {
                        ctxTFAT.DocTypes.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    string mNewCode = "";
                    mNewCode = mobj.Code;
                    transaction.Commit();
                    transaction.Dispose();
                    if (mAdd == true)
                    {
                        CreateCharges(mModel.DocTypes_Code, mModel.DocTypes_MainType, mauthorise);
                    }
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mNewCode, DateTime.Now, 0, mNewCode, "Save Transaction Classification", "D");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "TransactionClassification" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "TransactionClassification" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "TransactionClassification" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "TransactionClassification" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteTransactionClassification(TransactionClassificationVM mModel)
        {
            if (mModel.DocTypes_Code == null || mModel.DocTypes_Code == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            // iX9: Check for Active Master DocTypes
            string mactivestring = "";
            var mactive1 = ctxTFAT.TfatUserAuditHeader.Where(x => (x.Type == mModel.DocTypes_Code)).Select(x => x.Type).FirstOrDefault();
            if (mactive1 != null) { mactivestring = mactivestring + "\nTfatUserAuditHeader: " + mactive1; }
            var mactive2 = ctxTFAT.UserRightsTrx.Where(x => (x.Type == mModel.DocTypes_Code)).Select(x => x.Type).FirstOrDefault();
            if (mactive2 != null) { mactivestring = mactivestring + "\nUserRightsTrx: " + mactive2; }
            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "Active Item, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var mList = ctxTFAT.DocTypes.Where(x => (x.Code == mModel.DocTypes_Code)).FirstOrDefault();
            ctxTFAT.DocTypes.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData

    }
}