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
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class CashMemoReportController : BaseController
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
                Text = "Cash Memo Report",
                Value = "CashMemoReport"
            });
            //items.Add(new SelectListItem
            //{
            //    Text = "Cash Memo Report  With LR Details",
            //    Value = "CashMemoReportWithLR"
            //});

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
                string query = "SELECT Code, Name FROM CustomerMaster  order by Name ";
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
                string query = "SELECT Code, Name FROM Master where BaseGr='B' or BaseGr='C'  order by Name ";
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
        #endregion

        // GET: Accounts/CashMemoReport
        public ActionResult Index(CashMemoReportVM Model)
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

            string[] Colmn = new string[] { "Customer", "Branch", "BillDate", "DocDate", "Voucher No", "Account", "Remark", "ENTEREDBY", "LASTUPDATEDATE", "BillAmt" };
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = true);
            tfatSearch.Where(x => x.ColHead.Contains("Tablekey")).ToList().ForEach(x => x.IsHidden = true);
            tfatSearch.Where(x => x.ColHead.Contains("LrTablekey")).ToList().ForEach(x => x.IsHidden = true);
            tfatSearch.Where(x => Colmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            ctxTFAT.SaveChanges();

            //Model.SaveReportList = PopulateSaveReports(Model.ViewDataId);

            return View(Model);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(CashMemoReportVM Model)
        {
            try
            {
                TempData["CMMBranch"] = Model.Branch;
                TempData["CMMCustomer"] = Model.Customer;
                TempData["CMMMaster"] = Model.Master;

                //TempData["CustomerReq"] = Model.CustomerReq;
                TempData["CMMCashMemoChargesReq"] = Model.CashMemoChargesReq;
                if (Model.ViewDataId.Trim().ToLower() == "CashMemoReport".ToLower())
                {
                    var ReportHeader = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).FirstOrDefault();
                    ReportHeader.pMerge = "1,11^1";
                    ReportHeader.pToMerge = "11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38^1,2,3,4,5,6,7,8,9,10,39";
                    ctxTFAT.SaveChanges();
                }

                Model.GetReportParameter = true;
                if (Model.GetReportParameter == false)
                {
                    var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
                    tfatSearch.ForEach(x => x.IsHidden = false);
                    tfatSearch.Where(x => x.ColHead.Contains("Tablekey")).ToList().ForEach(x => x.IsHidden = true);
                    tfatSearch.Where(x => x.ColHead.Contains("LrTablekey")).ToList().ForEach(x => x.IsHidden = true);

                    string[] ChrColmn = new string[] { "Freight Charges", "LR Charges", "Hamali Charges", "Collection Charges", "Door Delivery Charges", "Varai Charges", "Loading Charges", "Unloading Charges", "Detention Charges", "Union Charges", "Demurrage Charges", "Other Charges", "Charges-13", "Charges-14", "Charges-15", "Charges-16", "Charges-17", "Charges-18", "Charges-19", "Charges-20", "Charges-21", "Charges-22", "Charges-23", "Charges-24", "Charges-25" };
                    if (Model.ViewDataId.Trim().ToLower() == "CashMemoReport".ToLower())
                    {
                        if (Model.CashMemoChargesReq)
                        {
                            var ChargesList = tfatSearch.Where(x => ChrColmn.Contains(x.ColHead)).ToList();
                            tfatSearch.Where(x => ChrColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);

                            #region Only This Chrges show As Per DocType Rule
                            var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "LR000" && x.DontUse == false).ToList();
                            List<string> LRcharge = Lrcharges.Select(x => x.Head).ToList();
                            ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                            #endregion

                            tfatSearch.Where(x => x.ColHead.Contains("BillAmt")).ToList().ForEach(x => x.IsHidden = true);
                            tfatSearch.Where(x => x.ColHead.Contains("LR_Total_Amt")).ToList().ForEach(x => x.IsHidden = false);
                            tfatSearch.Where(x => x.ColHead.Contains("LRNO")).ToList().ForEach(x => x.IsHidden = false);

                        }
                        else
                        {
                            var ChargesList = tfatSearch.Where(x => ChrColmn.Contains(x.ColHead)).ToList();
                            tfatSearch.Where(x => ChrColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                            tfatSearch.Where(x => x.ColHead.Contains("BillAmt")).ToList().ForEach(x => x.IsHidden = false);
                            tfatSearch.Where(x => x.ColHead.Contains("LR_Total_Amt")).ToList().ForEach(x => x.IsHidden = true);
                            tfatSearch.Where(x => x.ColHead.Contains("LRNO")).ToList().ForEach(x => x.IsHidden = true);
                        }
                    }

                    ctxTFAT.SaveChanges();
                }
                return GetGridDataColumns(Model.ViewDataId, "X", "", GetMonthString());
            }
            catch (Exception ex)
            {
                return Json(new { Status = "Error", Message = "Error! While Generating Report..\n" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetGridData(CashMemoReportVM Model)
        {
            if (String.IsNullOrEmpty(Model.mWhat))
            {
                if (Model.ViewDataId.Trim().ToLower() == "CashMemoReport".ToLower())
                {
                    ExecuteStoredProc("Drop Table ztmp_CashMemoReport");
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("SP_CashMemoReport", tfat_conx);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;

                    cmd.Parameters.Add("@mReportType", SqlDbType.VarChar).Value = Model.ReportTypeL;
                    cmd.Parameters.Add("@mVoucherNo", SqlDbType.VarChar).Value = Model.VoucherNo;

                    cmd.Parameters.Add("@mFromDate", SqlDbType.DateTime).Value = ConvertDDMMYYTOYYMMDD(Model.FromDate);
                    cmd.Parameters.Add("@mToDate", SqlDbType.DateTime).Value = ConvertDDMMYYTOYYMMDD(Model.ToDate);


                    cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("CMMBranch"));
                    cmd.Parameters.Add("@mCode", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("CMMMaster"));
                    cmd.Parameters.Add("@mCustomer", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("CMMCustomer"));

                    //cmd.Parameters.Add("@mCustomerReq", SqlDbType.VarChar).Value = Convert.ToBoolean(TempData.Peek("CustomerReq"));
                    cmd.Parameters.Add("@mCashMemoChargesReq", SqlDbType.VarChar).Value = Convert.ToBoolean(TempData.Peek("CMMCashMemoChargesReq"));

                    cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";

                    cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;


                    tfat_conx.Open();
                    cmd.CommandTimeout = 0;
                    cmd.ExecuteNonQuery();

                    string mReturnQuery = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");

                    tfat_conx.Close();
                }
                else if (Model.ViewDataId.Trim().ToLower() == "CashMemoReportWithLR".ToLower())
                {
                    ExecuteStoredProc("Drop Table ztmp_CashMemoReportWithLR");
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("SP_CashMemoReportWithLR", tfat_conx);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;

                    cmd.Parameters.Add("@mReportType", SqlDbType.VarChar).Value = Model.ReportTypeL;
                    cmd.Parameters.Add("@mVoucherNo", SqlDbType.VarChar).Value = Model.VoucherNo;

                    cmd.Parameters.Add("@mFromDate", SqlDbType.DateTime).Value = ConvertDDMMYYTOYYMMDD(Model.FromDate);
                    cmd.Parameters.Add("@mToDate", SqlDbType.DateTime).Value = ConvertDDMMYYTOYYMMDD(Model.ToDate);


                    cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("CMMBranch"));
                    cmd.Parameters.Add("@mCode", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("CMMMaster"));
                    cmd.Parameters.Add("@mCustomer", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("CMMMaster"));

                    //cmd.Parameters.Add("@mCustomerReq", SqlDbType.VarChar).Value = Convert.ToBoolean(TempData.Peek("CustomerReq"));
                    //cmd.Parameters.Add("@mCashMemoChargesReq", SqlDbType.VarChar).Value = Convert.ToBoolean(TempData.Peek("CashMemoChargesReq"));

                    cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";

                    cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;


                    tfat_conx.Open();
                    cmd.CommandTimeout = 0;
                    cmd.ExecuteNonQuery();

                    string mReturnQuery = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");

                    tfat_conx.Close();
                }

            }
            GridOption gridOption = new GridOption();
            gridOption.ViewDataId = Model.ViewDataId;
            gridOption.FromDate = Model.FromDate;
            gridOption.ToDate = Model.ToDate;
            gridOption.mWhat = Model.mWhat;

            int x = (int)System.Web.HttpContext.Current.Session["GridRows"];
            gridOption.rows = Model.rows;
            gridOption.page = Model.page;
            gridOption.searchField = Model.searchField;
            gridOption.searchOper = Model.searchOper;
            gridOption.searchString = Model.searchString;
            gridOption.sidx = Model.sidx;
            gridOption.sord = Model.sord;
            return GetGridReport(gridOption, "R", "", false, 0);
        }

        #region Set Column Show Hide

        public void LRChargesDetailsColumns(bool Flag, string ViewDataId)
        {
            string[] ChrColmn = new string[] { "Freight Charges", "LR Charges", "Hamali Charges", "Collection Charges", "Door Delivery Charges", "Varai Charges", "Loading Charges", "Unloading Charges", "Detention Charges", "Union Charges", "Demurrage Charges", "Other Charges", "Charges-13", "Charges-14", "Charges-15", "Charges-16", "Charges-17", "Charges-18", "Charges-19", "Charges-20", "Charges-21", "Charges-22", "Charges-23", "Charges-24", "Charges-25" };

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            if (Flag)
            {
                var ChargesList = tfatSearch.Where(x => ChrColmn.Contains(x.ColHead)).ToList();
                tfatSearch.Where(x => ChrColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);

                #region Only This Chrges show As Per DocType Rule
                var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "LR000" && x.DontUse == false).ToList();
                List<string> LRcharge = Lrcharges.Select(x => x.Head).ToList();
                ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                #endregion

                tfatSearch.Where(x => x.ColHead.Contains("BillAmt")).ToList().ForEach(x => x.IsHidden = true);
                tfatSearch.Where(x => x.ColHead.Contains("LR_Total_Amt")).ToList().ForEach(x => x.IsHidden = false);
                tfatSearch.Where(x => x.ColHead.Contains("LRNO")).ToList().ForEach(x => x.IsHidden = false);
                tfatSearch.Where(x => x.ColHead.Contains("Tablekey")).ToList().ForEach(x => x.IsHidden = true);
                tfatSearch.Where(x => x.ColHead.Contains("LrTablekey")).ToList().ForEach(x => x.IsHidden = true);
            }
            else
            {
                var ChargesList = tfatSearch.Where(x => ChrColmn.Contains(x.ColHead)).ToList();
                tfatSearch.Where(x => ChrColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                tfatSearch.Where(x => x.ColHead.Contains("BillAmt")).ToList().ForEach(x => x.IsHidden = false);
                tfatSearch.Where(x => x.ColHead.Contains("LR_Total_Amt")).ToList().ForEach(x => x.IsHidden = true);
                tfatSearch.Where(x => x.ColHead.Contains("LRNO")).ToList().ForEach(x => x.IsHidden = true);
                tfatSearch.Where(x => x.ColHead.Contains("Tablekey")).ToList().ForEach(x => x.IsHidden = true);
                tfatSearch.Where(x => x.ColHead.Contains("LrTablekey")).ToList().ForEach(x => x.IsHidden = true);
            }
            ctxTFAT.SaveChanges();
        }

        #endregion

        [HttpPost]
        public ActionResult ParameterReset(CashMemoReportVM Model)
        {
            Model.BranchesOnly = PopulateBranchesOnly();
            Model.Customers = PopulateCustomer();
            Model.Masters = PopulateMaster();
            Model.ReportsType = PopulateReportType();

            string[] Colmn = new string[] { "Branch", "BillDate", "DocDate", "Voucher No", "Account", "Remark", "ENTEREDBY", "LASTUPDATEDATE", "BillAmt" };
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = true);
            tfatSearch.Where(x => x.ColHead.Contains("Tablekey")).ToList().ForEach(x => x.IsHidden = true);
            tfatSearch.Where(x => x.ColHead.Contains("LrTablekey")).ToList().ForEach(x => x.IsHidden = true);
            tfatSearch.Where(x => Colmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            ctxTFAT.SaveChanges();

            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = DateTime.Today.ToShortDateString();
            //Model.SaveReportList = PopulateSaveReports(Model.ViewDataId);

            var html = ViewHelper.RenderPartialView(this, "_OptionsView", Model);
            var jsonResult = Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        [HttpPost]
        public ActionResult GetParameterAuto(CashMemoReportVM Model)
        {
            var MainReportName = "CashMemoReport";

            Model.BranchesOnly = PopulateBranchesOnly();
            Model.Customers = PopulateCustomer();
            Model.Masters = PopulateMaster();
            Model.ReportsType = PopulateReportType();


            var Tfatsearch = ctxTFAT.TfatSearch.Where(x => x.Code.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower()).ToList();
            Tfatsearch.ForEach(x => x.IsHidden = false);
            //Model.SaveReportList = PopulateSaveReports(MainReportName);
            ReportParameters mobj = new ReportParameters();
            if (ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).FirstOrDefault() != null)
            {
                mobj = ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).FirstOrDefault();

                Model.VoucherNo = mobj.DocNo;
                Model.FromDate = mobj.StartDate.Value.ToShortDateString();
                Model.ToDate = mobj.EndDate.Value.ToShortDateString();
                Model.HideColumnList = mobj.HideColumnList;
                Model.Branch = mobj.Para1;
                Model.Master = mobj.Para2;
                Model.Customer = mobj.Para4;
                Model.CashMemoChargesReq = mobj.Para3 == "T" ? true : false;
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
        public ActionResult SaveParameter(CashMemoReportVM Model)
        {
            var MainReportName = "CashMemoReport";
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


                    mobj.DocNo = Model.VoucherNo;
                    mobj.StartDate = ConvertDDMMYYTOYYMMDD(Model.FromDate);
                    mobj.EndDate = ConvertDDMMYYTOYYMMDD(Model.ToDate);
                    mobj.HideColumnList = HiddenColumn;
                    mobj.Para1 = Model.Branch;
                    mobj.Para2 = Model.Master;
                    mobj.Para4 = Model.Customer;
                    mobj.Para3 = Model.CashMemoChargesReq == true ? "T" : "F";


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

        public ActionResult DeleteParamete(CashMemoReportVM Model)
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