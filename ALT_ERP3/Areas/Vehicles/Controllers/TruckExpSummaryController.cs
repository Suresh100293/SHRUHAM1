using ALT_ERP3.Areas.Vehicles.Models;
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
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Vehicles.Controllers
{
    public class TruckExpSummaryController : BaseController
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
        //ITransactionGridOperation mIlst = new TransactionGridOperation();
        private static string mbasegr = "";

        #region Function List
        
        public JsonResult GetExpenses(string term)
        {
            var list = ctxTFAT.Master.Where(x => x.Grp != "000000013" && x.Grp != "000000023" && x.Grp != "000000055").ToList();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            }).Take(10);
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetVehiclePostAcc(string term)
        {
            var list = ctxTFAT.Master.Where(x => x.RelatedTo == "1").ToList();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            }).Take(10);
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        private List<SelectListItem> PopulateExpenses()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM Master where Grp<>'000000013' and  Grp<>'000000023' and  Grp<>'000000055' order by Recordkey ";
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

        private List<SelectListItem> PopulateVehicleMasterAcc()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM Master where OthPostType like '%V%' order by Recordkey ";
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

        private List<SelectListItem> PopulateItemGropu()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM itemgroups order by Name ";
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

        private List<SelectListItem> PopulateItemMaster()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM itemmaster order by Name ";
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

        // GET: Vehicles/TruckExpSummary
        public ActionResult Index(TruckExpSummaryVM Model)
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
            Model.ViewCode = GetDefaultCode(Model.ViewDataId);
            mbasegr = Model.MainType;
            ViewBag.Header = Model.Header;
            ViewBag.Controller = Model.Controller;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;


            Model.VehicleList = PopulateVehicleMasterAcc();
            Model.ExpensesList = PopulateExpenses();
            Model.ItemGroups = PopulateItemGropu();
            Model.ItemMasters = PopulateItemMaster();

            return View(Model);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(TruckExpSummaryVM Model)
        {
            try
            {
                TempData["Vehicle"] = Model.Vehicle;
                TempData["Expenses"] = Model.Expenses;
                TempData["ItemMaster"] = Model.ItemMaster;
                TempData["ItemGroup"] = Model.ItemGroup;
                TempData["WrongDue"] = Model.WrongDue;
                return GetGridDataColumns(Model.ViewDataId, "X", "", GetMonthString());
            }
            catch (Exception ex)
            {
                return Json(new { Status = "Error", Message = "Error! While Generating Report..\n" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetGridData(TruckExpSummaryVM Model)
        {
            mpara = "";
            Model.Vehicle = TempData.Peek("Vehicle") as string;
            Model.Expenses = TempData.Peek("Expenses") as string;
            Model.ItemGroup = TempData.Peek("ItemGroup") as string;
            Model.ItemMaster = TempData.Peek("ItemMaster") as string;
            Model.WrongDue = Convert.ToBoolean(TempData.Peek("WrongDue")) ;
            if (String.IsNullOrEmpty(Model.mWhat))
            {
                if (Model.ViewDataId == "VehicleDetails")
                {
                    mpara = "para01^1=1~";
                    if (Model.WrongDue)
                    {
                        mpara = "para01^(AllowRow='T')~";
                    }
                    Model.FromDate = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    Model.ToDate = (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

                    ExecuteStoredProc("Drop Table ztmp_VehicleDetails");
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("SPTFAT_VehicleDetails", tfat_conx);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.Add("@Vehicles", SqlDbType.VarChar).Value = Model.Vehicle;
                    cmd.Parameters.Add("@Expenses", SqlDbType.VarChar).Value = Model.Expenses;
                    cmd.Parameters.Add("@ItemGroup", SqlDbType.VarChar).Value = Model.ItemGroup;
                    cmd.Parameters.Add("@ItemMaster", SqlDbType.VarChar).Value = Model.ItemMaster;
                    cmd.Parameters.Add("@WrongDue", SqlDbType.Bit).Value = Model.WrongDue;
                    cmd.Parameters.Add("@FromDate", SqlDbType.VarChar).Value = Model.FromDate;
                    cmd.Parameters.Add("@ToDate", SqlDbType.VarChar).Value = Model.ToDate;

                    cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
                    cmd.Parameters.Add("@mReturnQuery1", SqlDbType.VarChar, 5000).Value = "";
                    cmd.Parameters.Add("@mReturnQuery2", SqlDbType.VarChar, 5000).Value = "";
                    cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@mReturnQuery1"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@mReturnQuery2"].Direction = ParameterDirection.Output;

                    tfat_conx.Open();
                    cmd.CommandTimeout = 0;
                    cmd.ExecuteNonQuery();

                    string mReturnString = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
                    string mReturnString1 = (string)(cmd.Parameters["@mReturnQuery1"].Value ?? "");
                    string mReturnString2 = (string)(cmd.Parameters["@mReturnQuery2"].Value ?? "");

                    tfat_conx.Close();
                }
                else if (Model.ViewDataId == "VehicleTruckLedgerSummary")
                {
                    Model.FromDate = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    Model.ToDate = (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

                    ExecuteStoredProc("Drop Table ztmp_VehicleTruckLedgerSummary");
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("SPTFAT_VehicleTruckLedgerSummary", tfat_conx);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.Add("@Vehicles", SqlDbType.VarChar).Value = Model.Vehicle;
                    cmd.Parameters.Add("@Expenses", SqlDbType.VarChar).Value = Model.Expenses;
                    cmd.Parameters.Add("@FromDate", SqlDbType.VarChar).Value = Model.FromDate;
                    cmd.Parameters.Add("@ToDate", SqlDbType.VarChar).Value = Model.ToDate;

                    cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
                    cmd.Parameters.Add("@mReturnQuery1", SqlDbType.VarChar, 5000).Value = "";
                    cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@mReturnQuery1"].Direction = ParameterDirection.Output;

                    tfat_conx.Open();
                    cmd.CommandTimeout = 0;
                    cmd.ExecuteNonQuery();

                    string mReturnString = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
                    string mReturnString1 = (string)(cmd.Parameters["@mReturnQuery1"].Value ?? "");

                    tfat_conx.Close();
                }
                else if (Model.ViewDataId == "VehicleExpenseSummary")
                {
                    Model.FromDate = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    Model.ToDate = (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

                    ExecuteStoredProc("Drop Table ztmp_VehicleExpenseSummary");
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("SPTFAT_VehicleExpenseSummary", tfat_conx);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.Add("@Vehicles", SqlDbType.VarChar).Value = Model.Vehicle;
                    cmd.Parameters.Add("@Expenses", SqlDbType.VarChar).Value = Model.Expenses;
                    cmd.Parameters.Add("@FromDate", SqlDbType.VarChar).Value = Model.FromDate;
                    cmd.Parameters.Add("@ToDate", SqlDbType.VarChar).Value = Model.ToDate;

                    cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
                    cmd.Parameters.Add("@mReturnQuery1", SqlDbType.VarChar, 5000).Value = "";
                    cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@mReturnQuery1"].Direction = ParameterDirection.Output;

                    tfat_conx.Open();
                    cmd.CommandTimeout = 0;
                    cmd.ExecuteNonQuery();

                    string mReturnString = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
                    string mReturnString1 = (string)(cmd.Parameters["@mReturnQuery1"].Value ?? "");

                    tfat_conx.Close();
                }
            }

            GridOption gridOption = new GridOption();
            gridOption.ViewDataId = Model.ViewDataId;
            gridOption.FromDate = Model.FromDate;
            gridOption.ToDate = Model.ToDate;
            gridOption.Date = Model.FromDate+":"+ Model.ToDate;

            gridOption.mWhat = Model.mWhat;

            int x = (int)System.Web.HttpContext.Current.Session["GridRows"];
            gridOption.rows = Model.rows;
            gridOption.page = Model.page == 0 ? 1 : Model.page;
            gridOption.searchField = Model.searchField;
            gridOption.searchOper = Model.searchOper;
            gridOption.searchString = Model.searchString;
            gridOption.sidx = Model.sidx;
            gridOption.sord = Model.sord;
            return GetGridReport(gridOption, "R", (mpara != "" ? "~" + mpara : ""), false, 0);
        }
        [HttpPost]
        public ActionResult ParameterReset(TruckExpSummaryVM Model)
        {
            Model.ExpensesList = PopulateExpenses();
            Model.VehicleList = PopulateVehicleMasterAcc();
            Model.ItemGroups = PopulateItemGropu();
            Model.ItemMasters = PopulateItemMaster();

            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = DateTime.Today.ToShortDateString();

            var html = ViewHelper.RenderPartialView(this, "_OptionsView", Model);
            var jsonResult = Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }
        [HttpPost]
        public ActionResult GetParameterAuto(TruckExpSummaryVM Model)
        {
            var MainReportName = Model.ViewDataId;

            Model.ExpensesList = PopulateExpenses();
            Model.VehicleList = PopulateVehicleMasterAcc();
            Model.ItemGroups = PopulateItemGropu();
            Model.ItemMasters = PopulateItemMaster();

            Model.ReportTypeL = ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).Select(x => x.ReportName).FirstOrDefault();
            var Tfatsearch = ctxTFAT.TfatSearch.Where(x => x.Code.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower()).ToList();
            Tfatsearch.ForEach(x => x.IsHidden = false);
            //Model.SaveReportList = PopulateSaveReports(MainReportName);
            ReportParameters mobj = new ReportParameters();
            if (ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).FirstOrDefault() != null)
            {
                mobj = ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).FirstOrDefault();

                Model.FromDate = mobj.StartDate.Value.ToShortDateString();
                Model.ToDate = mobj.EndDate.Value.ToShortDateString();
                Model.HideColumnList = mobj.HideColumnList;


                Model.Vehicle = mobj.Para1 == null ? "" : mobj.Para1.Replace("'", "");
                Model.Expenses = mobj.Para2 == null ? "" : mobj.Para2.Replace("'", "");
                Model.ItemGroup = mobj.Para3 == null ? "" : mobj.Para3.Replace("'", "");
                Model.ItemMaster = mobj.Para4 == null ? "" : mobj.Para4.Replace("'", "");
                Model.WrongDue = mobj.Para5 == "T" ? true : false;

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
        public ActionResult SaveParameter(TruckExpSummaryVM Model)
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


                    //mobj.DocNo = Model.FreightMemoNo;
                    mobj.StartDate = ConvertDDMMYYTOYYMMDD(Model.FromDate);
                    mobj.EndDate = ConvertDDMMYYTOYYMMDD(Model.ToDate);
                    mobj.HideColumnList = HiddenColumn;

                    mobj.Para1 = Model.Vehicle;

                    mobj.Para2 = Model.Expenses;
                    mobj.Para3 = Model.ItemGroup;
                    mobj.Para4 = Model.ItemMaster;
                    mobj.Para5 = Model.WrongDue == true ? "T" : "F";
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
        [HttpPost]
        public ActionResult DeleteParamete(TruckExpSummaryVM Model)
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