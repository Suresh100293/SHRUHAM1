using ALT_ERP3.Areas.Logistics.Models;
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

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class PickOrderReportController : BaseController
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

        private List<SelectListItem> PopulateBranch()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM TfatBranch where Category<>'Area' and BranchType='G' order by Recordkey ";
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

        // GET: Logistics/PickOrderReport
        public ActionResult Index(PickOrderRVM Model)
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


            Model.BranchList = PopulateBranch();
            Model.OrderForBranchList = PopulateBranch();
            string[] LRExpensesDetailsColumnsOnly = new string[] { "LR Branch", "LR Date", "LrNo", "Source", "Dest", "Consignor", "Consignee", "LR Qty", "LR Weight" };
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = false);
            tfatSearch.Where(x => LRExpensesDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            ctxTFAT.SaveChanges();

            return View(Model);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(PickOrderRVM Model)
        {
            try
            {
                TempData["pBranch"] = Model.Branch;
                TempData["pOrderForBranch"] = Model.OrderForBranch;
                TempData["pLRDetails"] = Model.LRDetails;
                return GetGridDataColumns(Model.ViewDataId, "X", "", GetMonthString());
            }
            catch (Exception ex)
            {
                return Json(new { Status = "Error", Message = "Error! While Generating Report..\n" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetGridData(PickOrderRVM Model)
        {
            Model.Branch = TempData.Peek("pBranch") as string;
            Model.OrderForBranch = TempData.Peek("pOrderForBranch") as string;
            

            if (String.IsNullOrEmpty(Model.mWhat))
            {
                ExecuteStoredProc("Drop Table ztmp_PickOrderReportDetails");
                SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                SqlCommand cmd = new SqlCommand("SPTFAT_PickOrderReport", tfat_conx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;
                cmd.Parameters.Add("@FromDate", SqlDbType.VarChar).Value = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                cmd.Parameters.Add("@ToDate", SqlDbType.VarChar).Value = (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                cmd.Parameters.Add("@Branchs", SqlDbType.VarChar).Value = Model.Branch;
                cmd.Parameters.Add("@OrderForBranch", SqlDbType.VarChar).Value = Model.OrderForBranch;
                cmd.Parameters.Add("@LRDetails", SqlDbType.Bit).Value = Convert.ToBoolean(TempData.Peek("pLRDetails"));

                cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
                cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;

                tfat_conx.Open();
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();

                string mReturnString = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");

                tfat_conx.Close();
            }



            GridOption gridOption = new GridOption();
            gridOption.ViewDataId = Model.ViewDataId;
            gridOption.FromDate = Model.FromDate;
            gridOption.ToDate = Model.ToDate;
            gridOption.mWhat = Model.mWhat;

            int x = (int)System.Web.HttpContext.Current.Session["GridRows"];
            gridOption.rows = Model.rows;
            gridOption.page = Model.page == 0 ? 1 : Model.page;
            gridOption.searchField = Model.searchField;
            gridOption.searchOper = Model.searchOper;
            gridOption.searchString = Model.searchString;
            gridOption.sidx = Model.sidx;
            gridOption.sord = Model.sord;


            return GetGridReport(gridOption, "X", "", false, 0);
        }

        [HttpPost]
        public ActionResult ParameterReset(PickOrderRVM Model)
        {
            Model.BranchList = PopulateBranch();
            Model.OrderForBranchList = PopulateBranch();

            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = DateTime.Today.ToShortDateString();

            string[] LRExpensesDetailsColumnsOnly = new string[] { "LR Branch", "LR Date", "LrNo", "Source", "Dest", "Consignor", "Consignee", "LR Qty", "LR Weight" };
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = false);
            tfatSearch.Where(x => LRExpensesDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            ctxTFAT.SaveChanges();

            var html = ViewHelper.RenderPartialView(this, "_OptionsView", Model);
            var jsonResult = Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        #region Set Column Show Hide

        public void LR_DetailsColumns(bool Flag, string ViewDataId)
        {
            string[] LRExpensesDetailsColumnsOnly = new string[] { "LR Branch", "LR Date", "LrNo", "Source", "Dest" , "Consignor", "Consignee", "LR Qty", "LR Weight" };

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            if (Flag)
            {
                tfatSearch.Where(x => LRExpensesDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            }
            else
            {
                tfatSearch.Where(x => LRExpensesDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }
            ctxTFAT.SaveChanges();
        }

        #endregion
    }
}