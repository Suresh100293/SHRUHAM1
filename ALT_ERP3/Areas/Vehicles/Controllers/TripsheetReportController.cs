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
    public class TripsheetReportController : BaseController
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
                Text = "TripSheet Report",
                Value = "TripsheetReport"
            });
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

        private List<SelectListItem> PopulateDriver(string Type)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            if (Type== "ALL")
            {
                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    string query = "SELECT Code, Name FROM DriverMaster  order by Name ";
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
            }
            else if (Type == "Active")
            {
                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    string query = "SELECT Code, Name FROM DriverMaster where Status=1  order by Name ";
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
            }
            else if (Type == "InActive")
            {
                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    string query = "SELECT Code, Name FROM DriverMaster where Status=0  order by Name ";
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
            }
            return items;
        }



        #endregion

        // GET: Vehicles/TripsheetReport
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

            //Model.Branch = mbranchcode;
            Model.Branch = String.IsNullOrEmpty(Model.Branch) == true ? string.Join(",", PopulateBranchesOnly().Select(x=>x.Value).ToList()) : Model.Branch;
            Model.BranchList = PopulateBranchesOnly();
            //Model.Customers = PopulateCustomer();
            Model.DriverList = PopulateDriver("ALL");
            Model.ActiveDriverList = PopulateDriver("Active");
            Model.InActiveDriverList = PopulateDriver("InActive");
            Model.ReportsType = PopulateReportType();


            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = false);
            ctxTFAT.SaveChanges();

            //Model.SaveReportList = PopulateSaveReports(Model.ViewDataId);

            return View(Model);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(TripSheetRVM Model)
        {
            try
            {
                TempData["trpBranch"] = Model.Branch;
                TempData["trpDriver"] = Model.Driver;
                if (Model.ViewDataId.Trim().ToLower() != "TripsheetReport".ToLower())
                {
                    string[] ReferenceColumns = new string[] { "Unadj Voucher", "Trip Reco.", "Difference" };
                    var tfatSearch = ctxTFAT.TfatSearch.Where(y => y.Code == Model.ViewDataId).ToList();
                    tfatSearch.ForEach(y => y.IsHidden = false);
                    if (!Model.AllLedgerReq)
                    {
                        tfatSearch.Where(y => ReferenceColumns.Contains(y.ColHead)).ToList().ForEach(y => y.IsHidden = true);
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
                if (Model.ViewDataId.Trim().ToLower() == "TripsheetReport".ToLower())
                {
                    ExecuteStoredProc("Drop Table ztmp_TripsheetReport");
                    ExecuteStoredProc("Drop Table ztmp_TripsheetReport1");
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("SP_TripsheetReport", tfat_conx);
                    try
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        //cmd.CommandTimeout = 120;

                        cmd.Parameters.Add("@mReportType", SqlDbType.VarChar).Value = Model.ReportTypeL;
                        cmd.Parameters.Add("@mFromDate", SqlDbType.VarChar).Value = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                        cmd.Parameters.Add("@mToDate", SqlDbType.VarChar).Value = (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                        cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("trpBranch"));
                        cmd.Parameters.Add("@mCode", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("trpDriver"));

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
                else
                {
                    //#region Create TempTable

                    

                    var DriverList = Convert.ToString(TempData.Peek("trpDriver")).Split(',').ToList();
                    var PostCode = ctxTFAT.DriverMaster.Where(t => DriverList.Any(y => y == t.Code)).Select(t => t.Posting).ToList();
                    var CombinedCode = String.Join(",", PostCode);

                    if (Model.AllLedgerReq)
                    {
                        string Query = "with Tripsheet as ( " +
                        "SELECT " +
                        "   DocNo, DocDate, Branch, NetAmt, Narr, EnteredBy, " +
                        "     TRIM(value) AS Adv " +
                        " FROM " +
                        "     TripSheetMaster " +
                        "     CROSS APPLY STRING_SPLIT(AdjustLedgerRef, '^') " +
                        "     where len(TRIM(value))> 0 " +
                        "  union all " +
                        " SELECT " +
                        "     DocNo, DocDate, Branch, NetAmt, Narr, EnteredBy, " +
                        "     TRIM(value) AS Adv " +
                        " FROM " +
                        "     tripsheetmaster " +
                        "     CROSS APPLY STRING_SPLIT(AdjustBalLedgerRef, '^') " +
                        " where len(TRIM(value))> 0 " +
                        " ),  TotalLedger as ( " +
                        " select L.Code as LedgerCode,(select M.Name From Master M where M.Code = L.Code) as Code,L.DocDate,L.Srl,L.Branch,L.AltCode,L.Debit,L.Credit,L.Narr,L.EnteredBy,T.DocNo,T.DocDate As TripDate,T.Branch As TripBranch,0 as NetAmt,T.Narr as TripNarr,T.EnteredBy As TripEnteredBy " +
                        "   from Ledger L left Join Tripsheet T on L.Branch + L.TableKey = T.Adv " +
                        " where CHARINDEX(L.Code, '" + CombinedCode + "') <> 0 and CHARINDEX(L.Branch, '" + Convert.ToString(TempData.Peek("trpBranch")) + "') <> 0 and L.DocDate >= '" + (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) + "' and L.DocDate <= '" + (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) + "' and L.Type <> 'Trip0' ) " +
                        " select LedgerCode,(isnull(sum(Debit), 0) - isnull(sum(Credit), 0)) as Unadjust " +
                        " into UnadjustLedgerDriver from TotalLedger where Docno is null " +
                        " group by LedgerCode";

                        ExecuteStoredProc("Drop Table UnadjustLedgerDriver");
                        SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                        SqlCommand cmd = new SqlCommand(Query, tfat_conx);

                        tfat_conx.Open();
                        cmd.CommandTimeout = 0;
                        cmd.ExecuteNonQuery();
                        tfat_conx.Close();
                    }
                    else
                    {
                        string Query = "select Value as LedgerCode,0 as Unadjust  into UnadjustLedgerDriver from STRING_SPLIT('" + CombinedCode + "',',')";
                        ExecuteStoredProc("Drop Table UnadjustLedgerDriver");
                        SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                        SqlCommand cmd = new SqlCommand(Query, tfat_conx);

                        tfat_conx.Open();
                        cmd.CommandTimeout = 0;
                        cmd.ExecuteNonQuery();
                        tfat_conx.Close();
                    }
                    

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

            mpara = "para05" + "^" + (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) + "~";
            return GetGridReport(gridOption, "R", mpara, true, 0);
        }

        [HttpPost]
        public ActionResult ParameterReset(TripSheetRVM Model)
        {
            Model.BranchList = PopulateBranchesOnly();
            Model.DriverList = PopulateDriver("ALL");
            Model.ActiveDriverList = PopulateDriver("Active");
            Model.InActiveDriverList = PopulateDriver("InActive");
            Model.ReportsType = PopulateReportType();

            Model.Branch = mbranchcode;

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = false);
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
        public ActionResult GetParameterAuto(TripSheetRVM Model)
        {
            var MainReportName = "CashMemoReport";

            Model.BranchList = PopulateBranchesOnly();
            Model.DriverList = PopulateDriver("ALL");
            Model.ActiveDriverList = PopulateDriver("Active");
            Model.InActiveDriverList = PopulateDriver("InActive");
            Model.ReportsType = PopulateReportType();


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
                Model.Branch = mobj.Para1;
                Model.Driver = mobj.Para2;
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
        public ActionResult SaveParameter(TripSheetRVM Model)
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


                    mobj.StartDate = ConvertDDMMYYTOYYMMDD(Model.FromDate);
                    mobj.EndDate = ConvertDDMMYYTOYYMMDD(Model.ToDate);
                    mobj.HideColumnList = HiddenColumn;
                    mobj.Para1 = Model.Branch;
                    mobj.Para2 = Model.Driver;


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

        public ActionResult DeleteParamete(TripSheetRVM Model)
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