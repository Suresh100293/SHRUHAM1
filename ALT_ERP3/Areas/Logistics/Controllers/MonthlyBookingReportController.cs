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
    public class MonthlyBookingReportController : BaseController
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

        private List<SelectListItem> PopulateBranches()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM TfatBranch where Category='Branch' or Category='Zone' or Category='SubBranch' or Code='HO0000' order by Recordkey ";
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
        private List<SelectListItem> PopulateCustomers()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM CustomerMaster where Hide='false' order by Name ";
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
        private List<SelectListItem> PopulateMasters()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM Master where Hide='false' order by Name ";
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
        private List<SelectListItem> PopulateCharges()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "select Head as Name,REPLACE(Fld, 'F00', 'Val') as Code from charges where type='LR000' and DontUse='false' order by Name ";
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
        public JsonResult DelBranch(string term)
        {
            List<TfatBranch> list = ctxTFAT.TfatBranch.Where(x => x.Status == true && (x.Category == "Branch" || x.Category == "SubBranch")).OrderBy(x => x.Name).ToList();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).OrderBy(x => x.Name).ToList();
            }

            //list.Where(w => w.Category == "Zone").ToList().ForEach(w => w.Name = w.Name + " - Z");
            //list.Where(w => w.Category == "0").ToList().ForEach(w => w.Name = w.Name + " - HO");
            list.Where(w => w.Category == "Branch").ToList().ForEach(w => w.Name = w.Name + " - B");
            list.Where(w => w.Category == "SubBranch").ToList().ForEach(w => w.Name = w.Name + " - SB");
            //list.Where(w => w.Category == "Area").ToList().ForEach(w => w.Name = w.Name + " - A");

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
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

        // GET: Logistics/MonthlyBookingReport
        public ActionResult Index(MonthlyBookingLRVM Model)
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
            Model.Customers = PopulateCustomers();
            Model.Masters = PopulateMasters();
            Model.Charges = PopulateCharges();

            //Model.CustomerF = true;
            if (String.IsNullOrEmpty(Model.Branch))
            {
                Model.LRBillAmtF = true;
            }


            if (Model.BranchF==false && Model.AccountF==false)
            {
                Model.BranchF = true;
            }
            string[] Colmn = new string[] { "Code", "Customer" };
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = false);
            tfatSearch.Where(x => Colmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);

            var DeliveryReporttfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == "DeliveryReport").ToList();
            DeliveryReporttfatSearch.ForEach(x => x.IsHidden = false);
            DeliveryReporttfatSearch.Where(x => Colmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);

            ctxTFAT.SaveChanges();

            return View(Model);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(MonthlyBookingLRVM Model)
        {
            try
            {
                mpara = "";
                if (Model.ViewDataId == "MonthlyBookingReportLR")
                {
                    //if (Model.GetReportParameter == false)
                    {
                        //var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
                        //string[] BrahchColumn = new string[] { "Branch", "Name" };
                        //string[] CustomerColumn = new string[] { "Code", "Customer" };
                        //if (Model.BranchF)
                        //{
                        //    tfatSearch.Where(x => BrahchColumn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        //}
                        //else
                        //{
                        //    tfatSearch.Where(x => BrahchColumn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        //}
                        //if (Model.AccountF)
                        //{
                        //    tfatSearch.Where(x => CustomerColumn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        //}
                        //else
                        //{
                        //    tfatSearch.Where(x => CustomerColumn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        //}
                        //ctxTFAT.SaveChanges();
                    }
                    ExecuteStoredProc("Drop Table ztmpmothlyLR_zOS");
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("SPREP_MonthlyBookingReportLR", tfat_conx);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.Add("@mBranchF", SqlDbType.VarChar).Value = Model.BranchF;
                    cmd.Parameters.Add("@mAccountF", SqlDbType.VarChar).Value = Model.AccountF;
                    cmd.Parameters.Add("@mBillAmtF", SqlDbType.VarChar).Value = Model.LRBillAmtF;
                    cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = Model.Branch == null ? "" : Model.Branch;
                    cmd.Parameters.Add("@mCharges", SqlDbType.VarChar).Value = Model.Charge == null ? "" : Model.Charge;
                    cmd.Parameters.Add("@mChargesFlag", SqlDbType.VarChar).Value = String.IsNullOrEmpty(Model.Charge) == true ? false : true;
                    cmd.Parameters.Add("@mCustomerF", SqlDbType.VarChar).Value = Model.CustomerF;
                    cmd.Parameters.Add("@mParties", SqlDbType.VarChar).Value = Model.Customer == null ? "" : Model.Customer;
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
                else if (Model.ViewDataId == "DeliveryReport")
                {
                    //if (Model.GetReportParameter == false)
                    {
                        //var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
                        //string[] BrahchColumn = new string[] { "Branch", "Name" };
                        //string[] CustomerColumn = new string[] { "Code", "Customer" };
                        //if (Model.BranchF)
                        //{
                        //    tfatSearch.Where(x => BrahchColumn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        //}
                        //else
                        //{
                        //    tfatSearch.Where(x => BrahchColumn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        //}
                        //if (Model.AccountF)
                        //{
                        //    tfatSearch.Where(x => CustomerColumn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        //}
                        //else
                        //{
                        //    tfatSearch.Where(x => CustomerColumn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        //}
                        //ctxTFAT.SaveChanges();
                    }

                    ExecuteStoredProc("Drop Table ztmpmothlyDelLR_zOS");
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("SPREP_MonthlyBookedDeliveryReportLR", tfat_conx);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.Add("@mBranchF", SqlDbType.VarChar).Value = Model.BranchF;
                    cmd.Parameters.Add("@mAccountF", SqlDbType.VarChar).Value = Model.AccountF;
                    cmd.Parameters.Add("@mBillAmtF", SqlDbType.VarChar).Value = Model.LRBillAmtF;
                    cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = Model.Branch == null ? "" : Model.Branch;
                    cmd.Parameters.Add("@mCustomerF", SqlDbType.VarChar).Value = Model.CustomerF;
                    cmd.Parameters.Add("@mParties", SqlDbType.VarChar).Value = Model.Customer == null ? "" : Model.Customer; ;
                    cmd.Parameters.Add("@mDate1", SqlDbType.VarChar).Value = (Convert.ToDateTime(StartDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    cmd.Parameters.Add("@mDate2", SqlDbType.VarChar).Value = (Convert.ToDateTime(EndDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

                    tfat_conx.Open();
                    cmd.CommandTimeout = 0;
                    cmd.ExecuteNonQuery();

                    tfat_conx.Close();
                }
                else if (Model.ViewDataId == "ZoneBookingReport")
                {
                    ExecuteStoredProc("Drop Table ztmpzonemothlyLR_zOS");
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("SPREP_ZoneMonthlyBookingReportLR", tfat_conx);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
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

        public ActionResult GetGridData(MonthlyBookingLRVM Model)
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
            return GetGridReport(gridOption, "R", "", true, 0);
        }

        #region Set Column Show Hide

        public void BranchDetailsColumns(bool Flag, string ViewDataId)
        {
            string[] BrahchColumn = new string[] { "Branch", "Name" };
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            var DtfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == "DeliveryReport").ToList();
            if (Flag)
            {
                tfatSearch.Where(x => BrahchColumn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                DtfatSearch.Where(x => BrahchColumn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            }
            else
            {
                tfatSearch.Where(x => BrahchColumn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                DtfatSearch.Where(x => BrahchColumn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }



            ctxTFAT.SaveChanges();
        }

        public void AccountDetailsColumns(bool Flag, string ViewDataId)
        {
            string[] CustomerColumn = new string[] { "Code", "Customer" };

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            var DtfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == "DeliveryReport").ToList();

            if (Flag)
            {
                tfatSearch.Where(x => CustomerColumn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                DtfatSearch.Where(x => CustomerColumn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            }
            else
            {
                tfatSearch.Where(x => CustomerColumn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                DtfatSearch.Where(x => CustomerColumn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }



            ctxTFAT.SaveChanges();
        }

        #endregion

        [HttpPost]
        public ActionResult ParameterReset(MonthlyBookingLRVM Model)
        {
            Model.Branches = PopulateBranches();
            Model.Customers = PopulateCustomers();
            Model.Masters = PopulateMasters();
            Model.Charges = PopulateCharges();
            Model.LRBillAmtF = true;
            Model.CustomerF = true;
            Model.BranchF = true;

            string[] Colmn = new string[] { "Code", "Customer" };
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = false);
            tfatSearch.Where(x => Colmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);

            var DeliveryReporttfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == "DeliveryReport").ToList();
            DeliveryReporttfatSearch.ForEach(x => x.IsHidden = false);
            DeliveryReporttfatSearch.Where(x => Colmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);

            ctxTFAT.SaveChanges();

            var html = ViewHelper.RenderPartialView(this, "_OptionsView", Model);
            var jsonResult = Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        [HttpPost]
        public ActionResult GetParameterAuto(MonthlyBookingLRVM Model)
        {
            var MainReportName = Model.ViewDataId;

            Model.Branches = PopulateBranches();
            Model.Customers = PopulateCustomers();
            Model.Masters = PopulateMasters();
            Model.Charges = PopulateCharges();

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
                Model.CustomerF = mobj.Para2 == "T" ? true : false;
                Model.Customer = mobj.Para3 == null ? "" : mobj.Para3.Replace("'", "");
                Model.BranchF = mobj.Para4 == "T" ? true : false;
                Model.AccountF = mobj.Para5 == "T" ? true : false;
                Model.LRBillAmtF = mobj.Para6 == "T" ? true : false;
                Model.Charge = mobj.Para7 == null ? "" : mobj.Para7.Replace("'", "").Replace("lr.", "").Replace("l.", "");

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
        public ActionResult SaveParameter(MonthlyBookingLRVM Model)
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
                    mobj.Para2 = Model.CustomerF == true ? "T" : "F";
                    mobj.Para3 = Model.Customer;
                    mobj.Para4 = Model.BranchF == true ? "T" : "F";
                    mobj.Para5 = Model.AccountF == true ? "T" : "F";
                    mobj.Para6 = Model.LRBillAmtF == true ? "T" : "F";
                    mobj.Para7 = Model.Charge;

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

        public ActionResult DeleteParamete(MonthlyBookingLRVM Model)
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