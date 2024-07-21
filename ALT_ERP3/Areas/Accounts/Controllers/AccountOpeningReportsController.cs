using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class AccountOpeningReportsController : BaseController
    {
        private static string Masterlist = "";
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;
        private new string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private new string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        public new string muserid = System.Web.HttpContext.Current.Session["UserId"].ToString();
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        //ITransactionGridOperation mIlst = new TransactionGridOperation();
        private static string mbasegr = "";

        private List<SelectListItem> PopulateMaster()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "select Code,Name from Master  order by Recordkey ";
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
        private List<SelectListItem> PopulateMasterGroup()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "select Code,Name from MasterGroups  order by Recordkey ";
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
        private List<SelectListItem> PopulateCustomerMaster()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "select Code,Name from CustomerMaster  order by Recordkey ";
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
        public JsonResult GetMaster()
        {
            List<SelectListItem> statesList = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "select Code,Name from Master  order by Recordkey ";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            statesList.Add(new SelectListItem
                            {
                                Text = sdr["Name"].ToString(),
                                Value = sdr["Code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }

            return Json(statesList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetMasterGroup()
        {
            List<SelectListItem> statesList = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "select Code,Name from MasterGroups  order by Recordkey ";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            statesList.Add(new SelectListItem
                            {
                                Text = sdr["Name"].ToString(),
                                Value = sdr["Code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }

            return Json(statesList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCustomerMaster(string id)
        {
            List<SelectListItem> statesList = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "select Code,Name from CustomerMaster where AccountParentGroup in ( " + id + " )  order by Recordkey ";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            statesList.Add(new SelectListItem
                            {
                                Text = sdr["Name"].ToString(),
                                Value = sdr["Code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }

            return Json(statesList, JsonRequestBehavior.AllowGet);
        }

        // GET: Accounts/AccountOpeningReports
        public ActionResult Index(AccountOpeningReportVM Model)
        {
            UpdateAuditTrail(mbranchcode, "Report", Model.Header, "", DateTime.Now, 0, "", "", "NA");
            if (Model.Document == "" || Model.Document == null)
            {
                GetAllMenu(Session["ModuleName"].ToString());
                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                Model.ToDate = DateTime.Today.ToShortDateString();
                //System.Web.HttpContext.Current.Session["LastDate"].ToString();
            }
            if (Model.FromDate == null)
            {
                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                Model.ToDate = DateTime.Today.ToShortDateString();
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

            Model.Masters = PopulateMaster();
            // Model.Customers = PopulateCustomerMaster();

            return View(Model);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(AccountOpeningReportVM Model)
        {
            try
            {

                Masterlist = String.Join(",", Model.MasterList);
                string mbranch = ppara05 == null || ppara05 == "" ? "'" + mbranchcode + "'" : ppara05;
                //Model.ViewDataId = "LorryReceiptReports";
                var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.ColHead == "Customer" && x.Code == Model.ViewDataId).FirstOrDefault();
                if (Model.CustomerMaster)
                {
                    tfatSearch.IsHidden = false;
                }
                else
                {
                    tfatSearch.IsHidden = true;
                }

                ctxTFAT.SaveChanges();

                return GetGridDataColumns(Model.ViewDataId, "X", "", GetMonthString());
            }
            catch (Exception ex)
            {
                return Json(new { Status = "Error", Message = "Error! While Generating Report..\n" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetGridData(AccountOpeningReportVM Model)
        {
            ///string osadjnarr = String.Join(",", Model.MasterList);
            //ppara05 = ppara05 == null || ppara05 == "" ? "'" + mbranchcode + "'" : ppara05;ExecuteStoredProc("Drop Table ztmp_TempMth");
            ExecuteStoredProc("Drop Table ztmp_AccountOpening");
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("SP_AccountOpening", tfat_conx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@ShowMaster", SqlDbType.Bit).Value = Model.MasterShow;
            cmd.Parameters.Add("@Master", SqlDbType.VarChar).Value = Masterlist;


            cmd.Parameters.Add("@mSelectQuery", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters.Add("@mUserQuery", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters.Add("@mTableQuery", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters["@mSelectQuery"].Direction = ParameterDirection.Output;
            cmd.Parameters["@mUserQuery"].Direction = ParameterDirection.Output;
            cmd.Parameters["@mTableQuery"].Direction = ParameterDirection.Output;

            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();

            string mSelectQuery = (string)(cmd.Parameters["@mSelectQuery"].Value ?? "");
            string mUserQuery = (string)(cmd.Parameters["@mUserQuery"].Value ?? "");
            string mTableQuery = (string)(cmd.Parameters["@mTableQuery"].Value ?? "");

            tfat_conx.Close();

            GridOption gridOption = new GridOption();
            gridOption.ViewDataId = Model.ViewDataId;
            gridOption.FromDate = Model.FromDate;
            gridOption.ToDate = Model.ToDate;

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
    }
}