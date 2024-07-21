using ALT_ERP3.Areas.Vehicles.Models;
using ALT_ERP3.Controllers;
using Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Vehicles.Controllers
{
    public class LedgerWithTripsheetController : BaseController
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
        private string[] ReferenceColumns = new string[] { "DocNo", "TripDate", "TripBranch", "TripNarr", "TripEnteredBy", "NetAmt" };

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

        private List<SelectListItem> PopulateDriver()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM Master where OthPostType like '%D%'  order by Name ";
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

        private List<SelectListItem> PopulateVehicle()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM Master where OthPostType like '%V%'  order by Name ";
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

        // GET: Vehicles/LedgerWithTripsheet
        public ActionResult Index(TripSheetRVM Model)
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

            Model.Branch = mbranchcode;

            Model.BranchList = PopulateBranchesOnly();
            Model.DriverList = PopulateDriver();
            Model.VehicleList = PopulateVehicle();

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = false);
            ctxTFAT.SaveChanges();

            return View(Model);
        }

        [HttpPost]
        public ActionResult ParameterReset(TripSheetRVM Model)
        {
            Model.BranchList = PopulateBranchesOnly();
            Model.DriverList = PopulateDriver();
            Model.VehicleList = PopulateVehicle();

            Model.Branch = mbranchcode;

            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = DateTime.Today.ToShortDateString();


            var html = ViewHelper.RenderPartialView(this, "_OptionsView", Model);
            var jsonResult = Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(TripSheetRVM Model)
        {
            try
            {
                TempData["trpBranch"] = Model.Branch;
                TempData["trpDriver"] = Model.Driver;
                TempData["trpVehicle"] = Model.Vehicle;
                TempData["trpReferenceReq"] = Model.ReferenceReq;
                TempData["trpAllLedgerReq"] = Model.AllLedgerReq;
                TempData["trpDriverAcReq"] = Model.DriverAcReq;
                if (Model.ViewDataId.Trim().ToLower() == "LedgerWithTripsheet".ToLower())
                {
                    var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
                    if (Model.ReferenceReq)
                    {
                        tfatSearch.ForEach(x => x.IsHidden = false);
                    }
                    else
                    {
                        tfatSearch.Where(x => ReferenceColumns.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
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

        public ActionResult GetGridData(TripSheetRVM Model)
        {
            var LDate = Convert.ToDateTime(Model.ToDate);
            if (String.IsNullOrEmpty(Model.mWhat))
            {
                if (Model.ViewDataId.Trim().ToLower() == "LedgerWithTripsheet".ToLower())
                {
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("SP_LedgerWithTripsheet", tfat_conx);
                    try
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        //cmd.CommandTimeout = 120;
                        cmd.Parameters.Add("@mFromDate", SqlDbType.VarChar).Value = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                        cmd.Parameters.Add("@mToDate", SqlDbType.VarChar).Value = (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                        cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("trpBranch"));
                        if (Convert.ToBoolean(TempData.Peek("trpDriverAcReq")))
                        {
                            cmd.Parameters.Add("@mCode", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("trpDriver"));
                        }
                        else
                        {
                            cmd.Parameters.Add("@mCode", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("trpVehicle"));
                        }
                        cmd.Parameters.Add("@mReferenceReq", SqlDbType.Bit).Value = Convert.ToBoolean(TempData.Peek("trpReferenceReq"));
                        cmd.Parameters.Add("@mAllLedgerReq", SqlDbType.Bit).Value = Convert.ToBoolean(TempData.Peek("trpAllLedgerReq"));

                        cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 500000).Value = "";
                        cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;

                        tfat_conx.Open();
                        cmd.CommandTimeout = 0;
                        cmd.ExecuteNonQuery();
                        string mReturnQuery = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
                    }
                    catch (Exception mex)
                    {

                    }
                    finally
                    {

                        cmd.Dispose();
                        tfat_conx.Close();
                        tfat_conx.Dispose();
                    }
                }
            }
            GridOption gridOption = new GridOption();
            gridOption.ViewDataId = Model.ViewDataId;
            gridOption.FromDate = Model.FromDate;
            gridOption.ToDate = Model.ToDate;
            gridOption.mWhat = Model.mWhat;
            gridOption.Date = Model.FromDate+":"+ Model.ToDate;

            int x = (int)System.Web.HttpContext.Current.Session["GridRows"];
            gridOption.rows = Model.rows;
            gridOption.page = Model.page;
            gridOption.searchField = Model.searchField;
            gridOption.searchOper = Model.searchOper;
            gridOption.searchString = Model.searchString;
            gridOption.sidx = Model.sidx;
            gridOption.sord = Model.sord;
            mpara = "";
            return GetGridReport(gridOption, "R", mpara, true, 0);
        }


    }
}