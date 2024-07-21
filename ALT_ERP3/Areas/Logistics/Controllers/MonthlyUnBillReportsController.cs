using ALT_ERP3.Areas.Logistics.Models;
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

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class MonthlyUnBillReportsController : BaseController
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
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

        #region Functions

        private List<SelectListItem> PopulateBranches()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM TfatBranch where Category!='Area'   order by Recordkey ";
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

        // GET: Logistics/MonthlyUnBillReports
        public ActionResult Index(MonthlyBookingUnBillLRVM Model)
        {
            UpdateAuditTrail(mbranchcode, "Report", Model.Header, "", DateTime.Now, 0, "", "", "NA");
            if (Model.Document == "" || Model.Document == null)
            {
                GetAllMenu(Session["ModuleName"].ToString());
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

            Model.Branches = PopulateBranches();


            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = false);

            var DeliveryReporttfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == "MonthlyUnBillAgeReports").ToList();
            DeliveryReporttfatSearch.ForEach(x => x.IsHidden = false);

            ctxTFAT.SaveChanges();

            Model.NoOfLR = true;
            Model.TotalAmt = true;
            Model.TotalQty = true;
            Model.TotalWT = true;

            return View(Model);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(MonthlyBookingUnBillLRVM Model)
        {
            try
            {
                mpara = "";
                #region Set Parameters

                ppara01 = "";
                ppara02 = "";
                ppara03 = "";
                ppara04 = "";
                ppara05 = "";
                ppara06 = "";
                ppara07 = "";
                ppara08 = "";
                ppara09 = "";
                ppara10 = "";
                ppara11 = "";
                ppara12 = "";
                ppara13 = "";
                ppara14 = "";
                ppara15 = "";
                ppara16 = "";
                ppara17 = "";
                ppara18 = "";
                ppara19 = "";
                ppara20 = "";
                ppara21 = "";
                ppara22 = "";
                ppara23 = "";
                ppara24 = "";
                mpara = "";


                if (!String.IsNullOrEmpty(Model.SelectContent))
                {
                    var GetPara = Model.SelectContent.Split('|');
                    for (int i = 0; i < GetPara.Count(); i++)
                    {
                        if (!String.IsNullOrEmpty(GetPara[i]))
                        {
                            switch (i + 1)
                            {
                                case 1:
                                    ppara09 = GetPara[i];
                                    mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                    break;
                                case 2:
                                    ppara10 = GetPara[i];
                                    mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                    break;
                                case 3:
                                    ppara11 = GetPara[i];
                                    mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                    break;
                                case 4:
                                    ppara12 = GetPara[i];
                                    mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                    break;
                                case 5:
                                    ppara13 = GetPara[i];
                                    mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                    break;
                                case 6:
                                    ppara14 = GetPara[i];
                                    mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                    break;
                                case 7:
                                    ppara15 = GetPara[i];
                                    mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                    break;
                                case 8:
                                    ppara16 = GetPara[i];
                                    mpara = mpara + "para" + (i + 9).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                    break;
                            }
                        }
                    }
                }

                #endregion
                ppara09 = "30";
                ppara10 = "60";
                ppara11 = "90";
                ppara12 = "120";
                mpara = "para09^30~para10^60~para11^90~para12^120~";
                if (Model.ViewDataId == "MonthlyUnBillReports")
                {
                    ExecuteStoredProc("Drop Table ztmpmothlyUnbillLR_zOS");
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("SPREP_MonthlyBookingReportUnBillLR", tfat_conx);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = Model.Branch == null ? "" : Model.Branch;
                    cmd.Parameters.Add("@mDate1", SqlDbType.VarChar).Value = (Convert.ToDateTime(StartDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    cmd.Parameters.Add("@mDate2", SqlDbType.VarChar).Value = (Convert.ToDateTime(EndDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    cmd.Parameters.Add("@mSelectQuery", SqlDbType.VarChar, 5000).Value = "";
                    cmd.Parameters["@mSelectQuery"].Direction = ParameterDirection.Output;

                    tfat_conx.Open();
                    cmd.CommandTimeout = 0;
                    cmd.ExecuteNonQuery();

                    //string mSelectQuery = (string)(cmd.Parameters["@mSelectQuery"].Value ?? "");

                    tfat_conx.Close();
                }
                else if (Model.ViewDataId == "MonthlyUnBillAgeReports")
                {
                    ExecuteStoredProc("Drop Table ztmpmothlyDelLR_zOS");
                    ExecuteStoredProc("Drop Table ztmpmothlyUnbillLR_zOSAge");
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("SPREP_MonthlyBookingReportUnBillLRAge", tfat_conx);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = Model.Branch == null ? "" : Model.Branch;
                    cmd.Parameters.Add("@mDate1", SqlDbType.VarChar).Value = (Convert.ToDateTime(StartDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    cmd.Parameters.Add("@mDate2", SqlDbType.VarChar).Value = (Convert.ToDateTime(EndDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    cmd.Parameters.Add("@mSelectQuery", SqlDbType.VarChar, 5000).Value = "";
                    cmd.Parameters["@mSelectQuery"].Direction = ParameterDirection.Output;

                    tfat_conx.Open();
                    cmd.CommandTimeout = 0;
                    cmd.ExecuteNonQuery();

                    tfat_conx.Close();
                }

                
                return GetGridDataColumns(Model.ViewDataId, "X", "", GetMonthString());
            }
            catch (Exception ex)
            {
                return Json(new { Status = "Error", Message = "Error! While Generating Report..\n" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetGridData(MonthlyBookingUnBillLRVM Model)
        {

            
            GridOption gridOption = new GridOption();
            gridOption.ViewDataId = Model.ViewDataId;
            //gridOption.FromDate = Model.FromDate;
            //gridOption.ToDate = Model.ToDate;
            gridOption.mWhat = Model.mWhat;

            int x = (int)System.Web.HttpContext.Current.Session["GridRows"];
            gridOption.rows = Model.rows;
            gridOption.page = Model.page == 0 ? 1 : Model.page;
            gridOption.searchField = Model.searchField;
            gridOption.searchOper = Model.searchOper;
            gridOption.searchString = Model.searchString;
            gridOption.sidx = Model.sidx;
            gridOption.sord = Model.sord;
            var ReportType = "X";
            if (Model.ViewDataId== "MonthlyUnBillAgeReports")
            {
                ReportType = "R";
            }
            return GetGridReport(gridOption, ReportType, mpara, true, 0);
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

        [HttpPost]
        public ActionResult ParameterReset(MonthlyBookingUnBillLRVM Model)
        {
            Model.Branches = PopulateBranches();
            //Model.Customers = PopulateCustomers();

            Model.NoOfLR = true;
            Model.TotalAmt = true;
            Model.TotalQty = true;
            Model.TotalWT = true;

            //string[] Colmn = new string[] { "Code", "Customer" };
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = false);
            //tfatSearch.Where(x => Colmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);

            var DeliveryReporttfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == "DeliveryReport").ToList();
            DeliveryReporttfatSearch.ForEach(x => x.IsHidden = false);
            //DeliveryReporttfatSearch.Where(x => Colmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);

            ctxTFAT.SaveChanges();

            var html = ViewHelper.RenderPartialView(this, "_OptionsView", Model);
            var jsonResult = Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }
        [HttpPost]
        public ActionResult GetParameterAuto(MonthlyBookingUnBillLRVM Model)
        {
            var MainReportName = Model.ViewDataId;

            Model.Branches = PopulateBranches();
            //Model.Customers = PopulateCustomers();


            Model.ReportTypeL = ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).Select(x => x.ReportName).FirstOrDefault();
            var Tfatsearch = ctxTFAT.TfatSearch.Where(x => x.Code.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower()).ToList();
            Tfatsearch.ForEach(x => x.IsHidden = false);
            //Model.SaveReportList = PopulateSaveReports(MainReportName);
            ReportParameters mobj = new ReportParameters();
            if (ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).FirstOrDefault() != null)
            {
                mobj = ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).FirstOrDefault();

                //Model.FromDate = mobj.StartDate.Value.ToShortDateString();
                //Model.ToDate = mobj.EndDate.Value.ToShortDateString();
                Model.HideColumnList = mobj.HideColumnList;


                Model.Branch = mobj.Para1 == null ? "" : mobj.Para1.Replace("'", "");
                Model.NoOfLR = mobj.Para2 == "T" ? true : false; 
                Model.TotalAmt = mobj.Para3 == "T" ? true : false;
                Model.TotalQty = mobj.Para4 == "T" ? true : false;
                Model.TotalWT = mobj.Para5 == "T" ? true : false;

                //Model.Customer = mobj.Para3 == null ? "" : mobj.Para2.Replace("'", "");
            }
            if (!String.IsNullOrEmpty(Model.HideColumnList))
            {
                var HiddenColumns = Model.HideColumnList.Split(',').ToList();
                Tfatsearch.Where(x => HiddenColumns.Contains(x.Sno.ToString())).ToList().ForEach(x => x.IsHidden = true);
                ctxTFAT.SaveChanges();
            }
            var DeliveryReporttfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == "MonthlyUnBillAgeReports").ToList();
            DeliveryReporttfatSearch.ForEach(x => x.IsHidden = false);
            if (Model.NoOfLR)
            {
                DeliveryReporttfatSearch.Where(x => x.ColHead.ToUpper().Contains("LR")).ToList().ForEach(x => x.IsHidden = true);
            }
            if (Model.TotalAmt)
            {
                DeliveryReporttfatSearch.Where(x => x.ColHead.ToUpper().Contains("VALUE")).ToList().ForEach(x => x.IsHidden = true);
            }
            if (Model.TotalQty)
            {
                DeliveryReporttfatSearch.Where(x => x.ColHead.ToUpper().Contains("QTY")).ToList().ForEach(x => x.IsHidden = true);
            }
            if (Model.TotalWT)
            {
                DeliveryReporttfatSearch.Where(x => x.ColHead.ToUpper().Contains("WT")).ToList().ForEach(x => x.IsHidden = true);
            }
            ctxTFAT.SaveChanges();

            var html = ViewHelper.RenderPartialView(this, "_OptionsView", Model);
            var jsonResult = Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }
        [HttpPost]
        public ActionResult SaveParameter(MonthlyBookingUnBillLRVM Model)
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
                    //mobj.StartDate = ConvertDDMMYYTOYYMMDD(Model.FromDate);
                    //mobj.EndDate = ConvertDDMMYYTOYYMMDD(Model.ToDate);
                    mobj.HideColumnList = HiddenColumn;

                    mobj.Para1 = Model.Branch;
                    mobj.Para2 = Model.NoOfLR == true ? "T" : "F"; ;
                    mobj.Para3 = Model.TotalAmt == true ? "T" : "F"; ;
                    mobj.Para4 = Model.TotalQty == true ? "T" : "F"; ;
                    mobj.Para5 = Model.TotalWT == true ? "T" : "F"; ;

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
        public ActionResult DeleteParamete(MonthlyBookingUnBillLRVM Model)
        {
            var mList = ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == Model.ViewDataId.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).FirstOrDefault();
            ctxTFAT.ReportParameters.Remove(mList);
            ctxTFAT.SaveChanges();

            return Json(new
            {
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }

        #region Set Column Show Hide

        public void NoOfLRDetailsColumns(bool Flag, string ViewDataId)
        {

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            var DtfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == "MonthlyUnBillAgeReports").ToList();
            if (!Flag)
            {
                tfatSearch.Where(x => x.ColHead.ToUpper().Contains("LR")).ToList().ForEach(x => x.IsHidden = true);
                DtfatSearch.Where(x => x.ColHead.ToUpper().Contains("LR")).ToList().ForEach(x => x.IsHidden = true);
            }
            else
            {
                tfatSearch.Where(x => x.ColHead.ToUpper().Contains("LR")).ToList().ForEach(x => x.IsHidden = false);
                DtfatSearch.Where(x => x.ColHead.ToUpper().Contains("LR")).ToList().ForEach(x => x.IsHidden = false);
            }
            ctxTFAT.SaveChanges();
        }
        public void TotalQtyDetailsColumns(bool Flag, string ViewDataId)
        {

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            var DtfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == "MonthlyUnBillAgeReports").ToList();
            if (!Flag)
            {
                tfatSearch.Where(x => x.ColHead.ToUpper().Contains("QTY")).ToList().ForEach(x => x.IsHidden = true);
                DtfatSearch.Where(x => x.ColHead.ToUpper().Contains("QTY")).ToList().ForEach(x => x.IsHidden = true);
            }
            else
            {
                tfatSearch.Where(x => x.ColHead.ToUpper().Contains("QTY")).ToList().ForEach(x => x.IsHidden = false);
                DtfatSearch.Where(x => x.ColHead.ToUpper().Contains("QTY")).ToList().ForEach(x => x.IsHidden = false);
            }
            ctxTFAT.SaveChanges();
        }
        public void TotalWTDetailsColumns(bool Flag, string ViewDataId)
        {

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            var DtfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == "MonthlyUnBillAgeReports").ToList();
            if (!Flag)
            {
                tfatSearch.Where(x => x.ColHead.ToUpper().Contains("WT")).ToList().ForEach(x => x.IsHidden = true);
                DtfatSearch.Where(x => x.ColHead.ToUpper().Contains("WT")).ToList().ForEach(x => x.IsHidden = true);
            }
            else
            {
                tfatSearch.Where(x => x.ColHead.ToUpper().Contains("WT")).ToList().ForEach(x => x.IsHidden = false);
                DtfatSearch.Where(x => x.ColHead.ToUpper().Contains("WT")).ToList().ForEach(x => x.IsHidden = false);
            }
            ctxTFAT.SaveChanges();
        }
        public void TotalAmtDetailsColumns(bool Flag, string ViewDataId)
        {

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            var DtfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == "MonthlyUnBillAgeReports").ToList();
            if (!Flag)
            {
                tfatSearch.Where(x => x.ColHead.ToUpper().Contains("AMT")).ToList().ForEach(x => x.IsHidden = true);
                DtfatSearch.Where(x => x.ColHead.ToUpper().Contains("VALUE")).ToList().ForEach(x => x.IsHidden = true);
            }
            else
            {
                tfatSearch.Where(x => x.ColHead.ToUpper().Contains("AMT")).ToList().ForEach(x => x.IsHidden = false);
                DtfatSearch.Where(x => x.ColHead.ToUpper().Contains("VALUE")).ToList().ForEach(x => x.IsHidden = false);
            }
            ctxTFAT.SaveChanges();
        }
        #endregion
    }
}