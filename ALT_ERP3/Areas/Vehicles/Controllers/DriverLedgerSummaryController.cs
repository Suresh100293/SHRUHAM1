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
    public class DriverLedgerSummaryController : BaseController
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
                string query = "SELECT Code, Name FROM Master where OthPostType like '%D%' order by Recordkey ";
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

        // GET: Vehicles/DriverLedgerSummary
        public ActionResult Index(TruckLedgerSummaryVM Model)
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
            if (String.IsNullOrEmpty(Model.Vehicle))
            {
                Model.Vehicle = "";
            }

            return View(Model);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(TruckLedgerSummaryVM Model)
        {
            try
            {
                TempData["DVehicle"] = Model.Vehicle;
                TempData["DExpenses"] = Model.Expenses;
                return GetGridDataColumns(Model.ViewDataId, "X", "", GetMonthString());
            }
            catch (Exception ex)
            {
                return Json(new { Status = "Error", Message = "Error! While Generating Report..\n" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetGridData(TruckLedgerSummaryVM Model)
        {
            var SDate = Convert.ToDateTime(Model.FromDate);
            var LDate = Convert.ToDateTime(Model.ToDate);
            Model.Vehicle = TempData.Peek("DVehicle") as string;
            Model.Expenses = TempData.Peek("DExpenses") as string;
            Model.FromDate = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            Model.ToDate = (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            string Cost = "";
            if (String.IsNullOrEmpty(Model.Expenses) || Model.Expenses == "''")
            {
                mpara = "para01" + "^" + "1=1" + "~";
                Cost = "1=1";
            }
            else
            {
                mpara = "para01" + "^" + "(Charindex(R.Value8," + Model.Expenses + ")<>0 or Charindex(R.Combo1," + Model.Expenses + ")<>0)" + "~";
                Cost = "(Charindex(R.Value8," + Model.Expenses + ")<>0 or Charindex(R.Combo1," + Model.Expenses + ")<>0)";
            }


            if (String.IsNullOrEmpty(Model.mWhat))
            {
                if (Model.ViewDataId == "DriverLedgerDetails")
                {
                    string Expenses = "";
                    if (String.IsNullOrEmpty(Model.Expenses) || Model.Expenses == "''")
                    {
                        Expenses = "1=1";
                    }
                    else
                    {
                        Expenses = " (Charindex(R.Value8," + Model.Expenses + ")<>0 or Charindex(R.Combo1," + Model.Expenses + ")<>0) ";
                    }

                    string Query = " WITH CTE AS ( "+
                                    " SELECT l.code, " +
                                    " l.DocDate, " +
                                    " (SELECT M.Name FROM Master M WHERE M.code = L.Code) as [Account], " +
                                    " (SELECT T.Name FROM TfatBranch T WHERE T.Code = l.Branch) as [Branch], " +
                                    " (SELECT D.Name FROM DocTypes D WHERE D.Code = l.Type) as [Type], " +
                                    " l.Srl as [Serial], " +
                                    " l.BillNumber as [BillNumber], " +
                                    " (SELECT M.Name FROM Master M WHERE M.code = L.AltCode) as [Particular],  " +
                                    " ISNULL((SELECT M.name FROM master M WHERE M.code = R.combo1), (SELECT M.name FROM master M WHERE M.code = R.Value8)) as [ExpAc], " +
                                    " Cast((l.Debit) as Decimal(14, 2)) as [Debit], " +
                                    " Cast((l.Credit) as Decimal(14, 2)) as [Credit], " +
                                    " l.Narr as [Narr],  " +
                                    " l.ENTEREDBY as [ENTEREDBY], " +
                                    " ROW_NUMBER() OVER(PARTITION BY l.DocDate, L.Code, l.Branch, l.Type, l.Srl, l.BillNumber, L.AltCode, R.combo1, R.Value8, l.Debit, l.Credit, l.Narr, l.ENTEREDBY ORDER BY l.DocDate) as RowNumber " +
                                    " FROM " +
                                    " Ledger l " +
                                    " LEFT JOIN " +
                                    " RelateData R ON R.ParentKey + R.Branch + R.Code = L.ParentKey + l.Branch + l.Code " +
                                    " WHERE " +
                                    " l.DocDate >= '"+ Model.FromDate + "' " +
                                    " AND l.DocDate <= '"+ Model.ToDate + "' " +
                                    " AND CHARINDEX(l.Code, "+ Model.Vehicle + ") <> 0 and "+ Expenses + " " +
                                    " ) " +
                                    " SELECT " +
                                    "    Code, DocDate, Account, Branch, Type, Serial, BillNumber, " +
                                    "     Particular, ExpAc, Debit, Credit, Narr, ENTEREDBY, RowNumber " +
                                    " into tmpDriverLedgerDetails FROM CTE " +
                                    " WHERE     RowNumber = 1; ";

                    ExecuteStoredProc("Drop Table tmpDriverLedgerDetails");
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand(Query, tfat_conx);

                    tfat_conx.Open();
                    cmd.CommandTimeout = 0;
                    cmd.ExecuteNonQuery();

                    tfat_conx.Close();
                }
                else if (Model.ViewDataId == "DLedgerExpSummyDetails")
                {
                    string Query = "  Select * into ztmp_DLedgerExpSummyDetails from (" +
                                    " select(select M.Name From Master M where M.code = L.Code) as [Account],   (isnull((select M.name from master M where M.code = R.combo1), (select M.name from master M where M.code = R.Value8))) as [ExpAc], (Cast(l.Debit as Decimal(14, 2))) as [Debit], (Cast(l.Credit as Decimal(14, 2))) as [Credit]" +
                                    " from Ledger l  left" +
                                    " join RelateData R on R.ParentKey + R.Branch + R.Code = L.ParentKey + l.Branch + l.Code where l.DocDate>='" + Model.FromDate + "' And l.DocDate<='" + Model.ToDate + "' and Charindex(l.Code, " + Model.Vehicle + ") <> 0 and " + Cost + "  " +
                                    " ) as Temp";

                    ExecuteStoredProc("Drop Table ztmp_DLedgerExpSummyDetails");
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand(Query, tfat_conx);

                    tfat_conx.Open();
                    cmd.CommandTimeout = 0;
                    cmd.ExecuteNonQuery();

                    tfat_conx.Close();
                }
                else if (Model.ViewDataId == "DriverLedgerBalDetails")
                {

                    #region Create TempTable

                    string Query = "create table ztmp_DriverLedgerBalDetails" +
                                    "  ( " +
                                    "      Driver varchar(100) not null, " +
                                    "      OpeningDr money not null, " +
                                    "      OpeningCr money not null, " +
                                    "      Debit money not null, " +
                                    "      Credit money not null, " +
                                    "      ClosingDr money not null, " +
                                    "      ClosingCr money not null, " +
                                    "  ) ";

                    ExecuteStoredProc("Drop Table ztmp_DriverLedgerBalDetails");
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand(Query, tfat_conx);

                    tfat_conx.Open();
                    cmd.CommandTimeout = 0;
                    cmd.ExecuteNonQuery();

                    tfat_conx.Close();

                    #endregion


                    DataTable tbl = new DataTable();
                    tbl.Columns.Add(new DataColumn("Driver", typeof(string)));
                    tbl.Columns.Add(new DataColumn("OpeningDr", typeof(decimal)));
                    tbl.Columns.Add(new DataColumn("OpeningCr", typeof(decimal)));
                    tbl.Columns.Add(new DataColumn("Debit", typeof(decimal)));
                    tbl.Columns.Add(new DataColumn("Credit", typeof(decimal)));
                    tbl.Columns.Add(new DataColumn("ClosingDr", typeof(decimal)));
                    tbl.Columns.Add(new DataColumn("ClosingCr", typeof(decimal)));


                    var VehicleCode = Model.Vehicle.Replace("'", "").Split(',');
                    for (int i = 0; i < VehicleCode.Length; i++)
                    {
                        bool addNewRow = true;
                        var VehicleCodet = VehicleCode[i];
                        var OpeningBal = GetBalance(VehicleCodet, SDate.AddDays(-1), "ALL", 0, false, false, false);
                        var ClosingBal = GetBalance(VehicleCodet, LDate.Date, "ALL", 0, false, false, false);
                        if (OpeningBal == 0 && ClosingBal == 0)
                        {
                            addNewRow = false;
                        }
                        if (Model.Supress == false)
                        {
                            addNewRow = true;
                        }
                        if (addNewRow)
                        {
                            DataRow dr = tbl.NewRow();
                            dr["Driver"] = ctxTFAT.Master.Where(y => y.Code == VehicleCodet.ToString()).Select(y => y.Name).FirstOrDefault();
                            if (OpeningBal > 0)
                            {
                                dr["OpeningDr"] = Math.Abs(OpeningBal);
                                dr["OpeningCr"] = Convert.ToDecimal(0);
                            }
                            else
                            {
                                dr["OpeningDr"] = Convert.ToDecimal(0);
                                dr["OpeningCr"] = Math.Abs(OpeningBal);
                            }

                            if (ClosingBal > 0)
                            {
                                dr["ClosingDr"] = Math.Abs(ClosingBal);
                                dr["ClosingCr"] = Convert.ToDecimal(0);
                            }
                            else
                            {
                                dr["ClosingDr"] = Convert.ToDecimal(0);
                                dr["ClosingCr"] = Math.Abs(ClosingBal);
                            }

                            var TCredit = GetBalanceDateRange(VehicleCodet, SDate.Date, LDate.Date, "C", "ALL", 0, false, false, false);
                            var TDebit = GetBalanceDateRange(VehicleCodet, SDate.Date, LDate.Date, "D", "ALL", 0, false, false, false);

                            dr["Debit"] = Convert.ToDecimal(TDebit);
                            dr["Credit"] = Convert.ToDecimal(TCredit);

                            tbl.Rows.Add(dr);
                        }
                    }

                    SqlConnection con = new SqlConnection(GetConnectionString());
                    //create object of SqlBulkCopy which help to insert  
                    SqlBulkCopy objbulk = new SqlBulkCopy(con);

                    //assign Destination table name  
                    objbulk.DestinationTableName = "ztmp_DriverLedgerBalDetails";
                    objbulk.ColumnMappings.Add("Driver", "Driver");
                    objbulk.ColumnMappings.Add("OpeningDr", "OpeningDr");
                    objbulk.ColumnMappings.Add("OpeningCr", "OpeningCr");
                    objbulk.ColumnMappings.Add("Debit", "Debit");
                    objbulk.ColumnMappings.Add("Credit", "Credit");
                    objbulk.ColumnMappings.Add("ClosingDr", "ClosingDr");
                    objbulk.ColumnMappings.Add("ClosingCr", "ClosingCr");

                    con.Open();
                    //insert bulk Records into DataBase.  
                    objbulk.WriteToServer(tbl);
                    con.Close();
                }
            }

            GridOption gridOption = new GridOption();
            gridOption.ViewDataId = Model.ViewDataId;
            gridOption.FromDate = Model.FromDate;
            gridOption.ToDate = Model.ToDate;
            gridOption.Date = Model.Date;
            gridOption.mWhat = Model.mWhat;

            int x = (int)System.Web.HttpContext.Current.Session["GridRows"];
            gridOption.rows = Model.rows;
            gridOption.page = Model.page == 0 ? 1 : Model.page;
            gridOption.searchField = Model.searchField;
            gridOption.searchOper = Model.searchOper;
            gridOption.searchString = Model.searchString;
            gridOption.sidx = Model.sidx;
            gridOption.sord = Model.sord;






            return GetGridReport(gridOption, "R", "Code^" + Model.Vehicle + (mpara != "" ? "~" + mpara : ""), false, 0);
        }
        [HttpPost]
        public ActionResult ParameterReset(TruckLedgerSummaryVM Model)
        {
            Model.ExpensesList = PopulateExpenses();
            Model.VehicleList = PopulateVehicleMasterAcc();

            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = DateTime.Today.ToShortDateString();

            var html = ViewHelper.RenderPartialView(this, "_OptionsView", Model);
            var jsonResult = Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }
        [HttpPost]
        public ActionResult GetParameterAuto(TruckLedgerSummaryVM Model)
        {
            var MainReportName = Model.ViewDataId;

            Model.ExpensesList = PopulateExpenses();
            Model.VehicleList = PopulateVehicleMasterAcc();

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
        public ActionResult SaveParameter(TruckLedgerSummaryVM Model)
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
        public ActionResult DeleteParamete(TruckLedgerSummaryVM Model)
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