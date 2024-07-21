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
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class LorryChallanReportsController : BaseController
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


        #region Functions

        private List<SelectListItem> PopulateBranches()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM TfatBranch  order by Recordkey ";
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

        private List<SelectListItem> PopulateReportType()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem
            {
                Text = "LorryChallan Report",
                Value = "LorryChallanReports"
            });
            items.Add(new SelectListItem
            {
                Text = "UN-Dispatch LC Report",
                Value = "UnDispatchLCReport"
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
        
        #endregion


        // GET: Logistics/LorryChallanReports
        public ActionResult Index(LorryChallanReportVM Model)
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
            Model.Branches = PopulateBranches();
            Model.BranchesOnly = PopulateBranchesOnly();
            Model.ReportsType = PopulateReportType();
            Model.ReportTypeL = Model.ViewDataId;

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = false);
            string[] LorryReceiptDetailsColumnsOnly = new string[] { "LRTablekey", "LCTablekey", "FMTablekey", "LRBranch", "LrNo", "Date", "From", "To", "Consignor", "Consignee", "BillParty", "LRtype", "BillBranch", "Collection", "Delivery", "Qty", "Unit", "ActWt", "ChgWt", "ChargeType", "Amt", "Description", "PartyChallan", "PartyInvoice", "PONumber", "BENumber", "DecVal", "GSTNO", "EwayBill", "LRMode", "ServiceType", "LRRemark" };
            tfatSearch.Where(x => LorryReceiptDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            ctxTFAT.SaveChanges();

            return View(Model);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(LorryChallanReportVM Model)
        {
            try
            {
                TempData["LCBranch"] = Model.Branch;
                TempData["LCLoadFrom"] = Model.LoadFrom;
                TempData["LCSendTo"] = Model.SendTo;


                TempData["LCLorryReceiptDetails"] = Model.LorryReceiptDetailsReq;

                var ReportHeader = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).FirstOrDefault();

                ReportHeader.pMerge = "8,29^8,15^8";
                ReportHeader.pToMerge = "27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56^12,13,14,15,16,17,18,19,20,21,22,23,24,25,26^1,2,3,4,5,6,7,8,9,10,11";

                Model.GetReportParameter = true;//Temparary lock This Flag
                if (Model.GetReportParameter == false)
                {
                    var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
                    tfatSearch.ForEach(x => x.IsHidden = false);
                    string[] HideColumns = new string[] { "LRTablekey", "LCTablekey", "FMTablekey" };
                    tfatSearch.Where(x => HideColumns.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);

                    if (Model.ReportTypeL.ToLower().Trim() == "LorryChallanReports".ToLower().Trim())
                    {
                        string[] LorryReceiptDetailsColumnsOnly = new string[] { "LRBranch", "LrNo", "Date", "From", "To", "Consignor", "Consignee", "BillParty", "LRtype", "BillBranch", "Collection", "Delivery", "Qty", "Unit", "ActWt", "ChgWt", "ChargeType", "Amt", "Description", "PartyChallan", "PartyInvoice", "PONumber", "BENumber", "DecVal", "GSTNO", "EwayBill", "LRMode", "ServiceType", "LRRemark" };
                        if (Model.LorryReceiptDetailsReq == false)
                        {
                            tfatSearch.Where(x => LorryReceiptDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }
                    }
                    else
                    {
                        string[] FreightMemoColumnsOnly = new string[] { "FMBranch", "FMDate", "FMNo", "FromBranch", "RouteViaName", "ToBranch", "Vehicle NO", "Vehicle Type", "Driver", "Broker", "Fm Freight", "Advance", "Balance", "PaidAt" };
                        tfatSearch.Where(x => FreightMemoColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                    }
                }
                ctxTFAT.SaveChanges();
                return GetGridDataColumns(Model.ViewDataId, "X", "", GetMonthString());
            }
            catch (Exception ex)
            {
                return Json(new { Status = "Error", Message = "Error! While Generating Report..\n" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetGridData(LorryChallanReportVM Model)
        {
            if (String.IsNullOrEmpty(Model.mWhat))
            {
                ExecuteStoredProc("Drop Table ztmp_LorryChallanReports");
                SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                SqlCommand cmd = new SqlCommand("SP_LorryChallanReports", tfat_conx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;
                cmd.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = ConvertDDMMYYTOYYMMDD(Model.FromDate);
                cmd.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = ConvertDDMMYYTOYYMMDD(Model.ToDate);
                cmd.Parameters.Add("@ReportType", SqlDbType.VarChar).Value = Model.ReportTypeL;


                cmd.Parameters.Add("@LCNO", SqlDbType.VarChar).Value = Model.LorryChallanNo;
                cmd.Parameters.Add("@Branch", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("LCBranch"));
                cmd.Parameters.Add("@FromBranch", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("LCLoadFrom"));
                cmd.Parameters.Add("@ToBranch", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("LCSendTo"));

                cmd.Parameters.Add("@LorryReceiptDetails", SqlDbType.VarChar).Value = Convert.ToBoolean(TempData.Peek("LCLorryReceiptDetails"));

                cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
                cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;

                tfat_conx.Open();
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();

                string mReturnQuery = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");


                tfat_conx.Close();
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

        public void LRDetailsColumns(bool Flag, string ViewDataId)
        {
            string[] LorryReceiptDetailsColumnsOnly = new string[] { "LRBranch", "LrNo", "Date", "From", "To", "Consignor", "Consignee", "BillParty", "LRtype", "BillBranch", "Collection", "Delivery", "Qty", "Unit", "ActWt", "ChgWt", "ChargeType", "Amt", "Description", "PartyChallan", "PartyInvoice", "PONumber", "BENumber", "DecVal", "GSTNO", "EwayBill", "LRMode", "ServiceType", "LRRemark" };
            string[] FreightMemoColumnsOnly = new string[] { "FMBranch", "FMDate", "FMNo", "FromBranch", "RouteViaName", "ToBranch", "Vehicle NO", "Vehicle Type", "Driver", "Broker", "Fm Freight", "Advance", "Balance", "PaidAt" };
            string[] HideColumns = new string[] { "LRTablekey", "LCTablekey", "FMTablekey" };
            

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == "LorryChallanReports").ToList();
            tfatSearch.ForEach(x => x.IsHidden = false);
            tfatSearch.Where(x => HideColumns.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            if (ViewDataId== "LorryChallanReports")
            {
                if (Flag)
                {
                    tfatSearch.Where(x => LorryReceiptDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                }
                else
                {
                    tfatSearch.Where(x => LorryReceiptDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                }
            }
            else
            {
                if (Flag)
                {
                    tfatSearch.Where(x => LorryReceiptDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                }
                else
                {
                    tfatSearch.Where(x => LorryReceiptDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                }
                tfatSearch.Where(x => FreightMemoColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }
            ctxTFAT.SaveChanges();
        }

        public void SetColumnsReset(LorryChallanReportVM Model)
        {
            string[] LorryReceiptDetailsColumnsOnly = new string[] { "LRBranch", "LrNo", "Date", "From", "To", "Consignor", "Consignee", "BillParty", "LRtype", "BillBranch", "Collection", "Delivery", "Qty", "Unit", "ActWt", "ChgWt", "ChargeType", "Amt", "Description", "PartyChallan", "PartyInvoice", "PONumber", "BENumber", "DecVal", "GSTNO", "EwayBill", "LRMode", "ServiceType", "LRRemark" };
            string[] FreightMemoColumnsOnly = new string[] { "FMBranch", "FMDate", "FMNo", "FromBranch", "RouteViaName", "ToBranch", "Vehicle NO", "Vehicle Type", "Driver", "Broker", "Fm Freight", "Advance", "Balance", "PaidAt" };
            string[] HideColumns = new string[] { "LRTablekey", "LCTablekey", "FMTablekey" };

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = false);
            tfatSearch.Where(x => HideColumns.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);

            if (Model.ReportTypeL == "LorryChallanReports")
            {
                if (Model.LorryReceiptDetailsReq)
                {
                    tfatSearch.Where(x => LorryReceiptDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                }
                else
                {
                    tfatSearch.Where(x => LorryReceiptDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                }
            }
            else
            {
                if (Model.LorryReceiptDetailsReq)
                {
                    tfatSearch.Where(x => LorryReceiptDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                }
                else
                {
                    tfatSearch.Where(x => LorryReceiptDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                }
                tfatSearch.Where(x => FreightMemoColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }

            ctxTFAT.SaveChanges();
        }

        #endregion

        public ActionResult ParameterReset(LorryChallanReportVM Model)
        {
            Model.Branches = PopulateBranches();
            Model.BranchesOnly = PopulateBranchesOnly();
            Model.ReportsType = PopulateReportType();
            Model.Branch = mbranchcode;

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = false);
            string[] LorryReceiptDetailsColumnsOnly = new string[] { "LRBranch", "LrNo", "Date", "From", "To", "Consignor", "Consignee", "BillParty", "LRtype", "BillBranch", "Collection", "Delivery", "Qty", "Unit", "ActWt", "ChgWt", "ChargeType", "Amt", "Description", "PartyChallan", "PartyInvoice", "PONumber", "BENumber", "DecVal", "GSTNO", "EwayBill", "LRMode", "ServiceType", "LRRemark" };
            tfatSearch.Where(x => LorryReceiptDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            string[] HideColumns = new string[] { "LRTablekey", "LCTablekey", "FMTablekey" };
            tfatSearch.Where(x => HideColumns.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            ctxTFAT.SaveChanges();

            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = DateTime.Today.ToShortDateString();

            var html = ViewHelper.RenderPartialView(this, "_OptionsView", Model);
            var jsonResult = Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }
        public ActionResult GetParameterAuto(LorryChallanReportVM Model)
        {
            var MainReportName = Model.ViewDataId;

            Model.Branches = PopulateBranches();
            Model.BranchesOnly = PopulateBranchesOnly();
            Model.ReportsType = PopulateReportType();

            Model.ReportTypeL = ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).Select(x => x.ReportName).FirstOrDefault();
            var Tfatsearch = ctxTFAT.TfatSearch.Where(x => x.Code.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower()).ToList();
            Tfatsearch.ForEach(x => x.IsHidden = false);
            //Model.SaveReportList = PopulateSaveReports(MainReportName);
            ReportParameters mobj = new ReportParameters();
            if (ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).FirstOrDefault() != null)
            {
                mobj = ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).FirstOrDefault();

                Model.LorryChallanNo = mobj.DocNo;
                Model.FromDate = mobj.StartDate.Value.ToShortDateString();
                Model.ToDate = mobj.EndDate.Value.ToShortDateString();
                Model.HideColumnList = mobj.HideColumnList;

                Model.Branch = mobj.Para1 == null ? "" : mobj.Para1.Replace("'", "");
                Model.LoadFrom = mobj.Para2 == null ? "" : mobj.Para2.Replace("'", "");
                Model.SendTo = mobj.Para3 == null ? "" : mobj.Para3.Replace("'", "");

                Model.LorryReceiptDetailsReq = mobj.Para4 == "T" ? true : false;
                
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
        public ActionResult SaveParameter(LorryChallanReportVM Model)
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


                    mobj.DocNo = Model.LorryChallanNo;
                    mobj.StartDate = ConvertDDMMYYTOYYMMDD(Model.FromDate);
                    mobj.EndDate = ConvertDDMMYYTOYYMMDD(Model.ToDate);
                    mobj.HideColumnList = HiddenColumn;

                    mobj.Para1 = Model.Branch;
                    mobj.Para2 = Model.LoadFrom;
                    mobj.Para3 = Model.SendTo;

                    mobj.Para4 = Model.LorryReceiptDetailsReq == true ? "T" : "F";
                    


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
        public ActionResult DeleteParamete(LorryChallanReportVM Model)
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