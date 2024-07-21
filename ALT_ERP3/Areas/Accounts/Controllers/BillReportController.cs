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
    public class BillReportController : BaseController
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
                Text = "Bill Report",
                Value = "BillReport"
            });
            items.Add(new SelectListItem
            {
                Text = "Bill Report With LR Details",
                Value = "BillReportWithLR"
            });
            items.Add(new SelectListItem
            {
                Text = "Un_Submission Bill",
                Value = "UnSubmissionBill"
            });

            return items;
        }

        private List<SelectListItem> PopulateBillType()
        {
            List<SelectListItem> maclist = new List<SelectListItem>();

            maclist.Add(new SelectListItem { Value = "SLR00", Text = "Freight Bill" });
            maclist.Add(new SelectListItem { Value = "SLW00", Text = "Freight Bill (No LR)" });
            maclist.Add(new SelectListItem { Value = "CMM00", Text = "Cash Sale" });
            maclist.Add(new SelectListItem { Value = "", Text = "ALL" });

            return maclist;
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
                string query = "SELECT Code, Name FROM Master where BaseGr='D'  order by Name ";
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

        // GET: Accounts/BillReport
        public ActionResult Index(BillReportVM Model)
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
            Model.BillTypes = PopulateBillType();
            Model.ReportTypeL = Model.ViewDataId;
            Model.BillType = "SLR00";
            Model.CustomerReq = true;

            string[] Colmn = new string[] { "Branch", "BillDate", "DocDate", "BillNumber", "Taxable", "IGSTAmt", "CGSTAmt", "SGSTAmt", "BillAmt","RoundOff", "Customer", "AccountHead", "Remark", "ENTEREDBY", "LASTUPDATEDATE" };
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = true);
            tfatSearch.Where(x => Colmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            ctxTFAT.SaveChanges();


            return View(Model);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(BillReportVM Model)
        {
            try
            {
                TempData["BILLBranch"] = Model.Branch;
                TempData["BILLCustomer"] = Model.Customer;
                TempData["BILLBillType"] = Model.BillType;


                TempData["BILLCustomerReq"] = Model.CustomerReq;
                TempData["BILLPaymentReceivedDetails"] = Model.PaymentReceivedDetails;
                TempData["BILLPaymentReceDetailsChargesReq"] = Model.PaymentReceDetailsChargesReq;
                TempData["BILLBillSubMissionDetails"] = Model.BillSubMissionDetails;

                var ReportHeader = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).FirstOrDefault();
                if (Model.ViewDataId == "BillReport" )
                {
                    ReportHeader.pMerge = "1,36^1,18^1";
                    ReportHeader.pToMerge = "35,36,37,38,39,40,41,42^18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34^1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17";
                }
                else if (Model.ViewDataId == "BillReportWithLR")
                {
                    ReportHeader.pMerge = "4,11^4";
                    ReportHeader.pToMerge = "11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49^1,2,3,4,5,6,7,8,9,10";
                }
                ctxTFAT.SaveChanges();

                Model.GetReportParameter = true;
                if (Model.GetReportParameter == false)
                {
                    if (Model.ViewDataId == "BillReport")
                    {
                        var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
                        tfatSearch.ForEach(x => x.IsHidden = false);
                        string[] BillSubMissionDetailsColumnsOnly = new string[] { "SubBranch", "SubDocNo", "SubType", "SubThrough", "SubDate", "SubRemark", "SubENTEREDBY", "SubLASTUPDATEDATE" };
                        string[] PaymentReceivedDetailsDetailsColumnsOnly = new string[] { "ReceivedBranch", "ReceivedDate", "ReceivedFrom", "Amount", "TdsAmout", "TotalReceivedAmount" };
                        string[] PaymentReceivedDetailsDetailsChargesColumnsOnly = new string[] { "Freight Rebate", "Excess Recd", "tds", "Charges-4", "Charges-5", "Charges-6", "Charges-7", "Charges-8", "Charges-9", "Charges-10" };
                        if (Model.PaymentReceivedDetails == true)
                        {
                            tfatSearch.Where(x => PaymentReceivedDetailsDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            if (Model.PaymentReceDetailsChargesReq == true)
                            {
                                var ChargesList = tfatSearch.Where(x => PaymentReceivedDetailsDetailsChargesColumnsOnly.Contains(x.ColHead)).ToList();
                                tfatSearch.Where(x => PaymentReceivedDetailsDetailsChargesColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);

                                #region Only This Chrges show As Per DocType Rule
                                var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "BRC00" && x.DontUse == false).ToList();
                                List<string> LRcharge = Lrcharges.Select(x => x.Head).ToList();
                                ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                                ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                                #endregion
                            }
                            else
                            {
                                tfatSearch.Where(x => PaymentReceivedDetailsDetailsChargesColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                            }
                        }
                        else
                        {
                            tfatSearch.Where(x => PaymentReceivedDetailsDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                            tfatSearch.Where(x => PaymentReceivedDetailsDetailsChargesColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);

                        }
                        if (Model.BillSubMissionDetails)
                        {
                            tfatSearch.Where(x => BillSubMissionDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        }
                        else
                        {
                            tfatSearch.Where(x => BillSubMissionDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }
                    }
                    else if (Model.ViewDataId == "BillReportWithLR")
                    {
                        var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
                        tfatSearch.ForEach(x => x.IsHidden = false);

                        string[] ChrColmn = new string[] { "Freight Charges", "LR Charges", "Hamali Charges", "Collection Charges", "Door Delivery Charges", "Varai Charges", "Loading Charges", "Unloading Charges", "Detention Charges", "Union Charges", "Demurrage Charges", "Other Charges", "Charges-13", "Charges-14", "Charges-15", "Charges-16", "Charges-17", "Charges-18", "Charges-19", "Charges-20", "Charges-21", "Charges-22", "Charges-23", "Charges-24", "Charges-25" };
                        var ChargesList = tfatSearch.Where(x => ChrColmn.Contains(x.ColHead)).ToList();
                        tfatSearch.Where(x => ChrColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);

                        #region Only This Chrges show As Per DocType Rule
                        var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "LR000" && x.DontUse == false).ToList();
                        List<string> LRcharge = Lrcharges.Select(x => x.Head).ToList();
                        ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        #endregion
                    }
                    ctxTFAT.SaveChanges();
                }
                return GetGridDataColumns(Model.ViewDataId, "X", "", GetMonthString());
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Status = "Error",
                    Message = "Error! While Generating Report..\n" + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetGridData(BillReportVM Model)
        {
            if (String.IsNullOrEmpty(Model.mWhat))
            {
                if (Model.ViewDataId.Trim().ToLower() == "BillReport".ToLower() || Model.ViewDataId.Trim().ToLower() == "UnSubmissionBill".ToLower())
                {


                    ExecuteStoredProc("Drop Table ztmp_BillReport");
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("BillReports", tfat_conx);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;

                    cmd.Parameters.Add("@mReportType", SqlDbType.VarChar).Value = Model.ReportTypeL;
                    cmd.Parameters.Add("@mBillType", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("BILLBillType"));
                    cmd.Parameters.Add("@mBillNumber", SqlDbType.VarChar).Value = Model.BillNo;

                    cmd.Parameters.Add("@mFromDate", SqlDbType.DateTime).Value = ConvertDDMMYYTOYYMMDD(Model.FromDate);
                    cmd.Parameters.Add("@mToDate", SqlDbType.DateTime).Value = ConvertDDMMYYTOYYMMDD(Model.ToDate);


                    cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("BILLBranch"));
                    cmd.Parameters.Add("@mCode", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("BILLCustomer"));

                    cmd.Parameters.Add("@mCustomerReq", SqlDbType.VarChar).Value = Convert.ToBoolean(TempData.Peek("BILLCustomerReq"));
                    cmd.Parameters.Add("@mPaymentReceDetailsReq", SqlDbType.VarChar).Value = Convert.ToBoolean(TempData.Peek("BILLPaymentReceivedDetails"));

                    if (Model.ViewDataId.Trim().ToLower() == "UnSubmissionBill".ToLower())
                    {
                        cmd.Parameters.Add("@mBillSubMissionDetailsReq", SqlDbType.VarChar).Value = true;
                    }
                    else
                    {
                        cmd.Parameters.Add("@mBillSubMissionDetailsReq", SqlDbType.VarChar).Value = Convert.ToBoolean(TempData.Peek("BILLBillSubMissionDetails"));
                    }

                    cmd.Parameters.Add("@mPaymentReceDetailsChargesReq", SqlDbType.VarChar).Value = Convert.ToBoolean(TempData.Peek("BILLPaymentReceDetailsChargesReq"));

                    cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";

                    cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;


                    tfat_conx.Open();
                    cmd.CommandTimeout = 0;
                    cmd.ExecuteNonQuery();

                    string mReturnQuery = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");

                    tfat_conx.Close();
                }
                else if (Model.ViewDataId.Trim().ToLower() == "BillReportWithLR".ToLower())
                {
                    ExecuteStoredProc("Drop Table ztmp_BillReportWithLR");
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("SP_BillReportWithLR", tfat_conx);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;

                    cmd.Parameters.Add("@mReportType", SqlDbType.VarChar).Value = Model.ReportTypeL;
                    cmd.Parameters.Add("@mBillType", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("BILLBillType"));
                    cmd.Parameters.Add("@mBillNumber", SqlDbType.VarChar).Value = Model.BillNo;

                    cmd.Parameters.Add("@mFromDate", SqlDbType.DateTime).Value = ConvertDDMMYYTOYYMMDD(Model.FromDate);
                    cmd.Parameters.Add("@mToDate", SqlDbType.DateTime).Value = ConvertDDMMYYTOYYMMDD(Model.ToDate);


                    cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("BILLBranch"));
                    cmd.Parameters.Add("@mCode", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("BILLCustomer"));

                    cmd.Parameters.Add("@mCustomerReq", SqlDbType.VarChar).Value = Convert.ToBoolean(TempData.Peek("BILLCustomerReq"));

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
            return GetGridReport(gridOption, "X", "", false, 0);
        }

        #region Set Column Show Hide

        public void BillSubmissionDetailsColumns(bool Flag, string ViewDataId)
        {
            string[] BillSubMissionDetailsColumnsOnly = new string[] { "SubBranch", "SubDocNo", "SubType", "SubThrough", "SubDate", "SubRemark", "SubENTEREDBY", "SubLASTUPDATEDATE" };

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            if (Flag)
            {
                tfatSearch.Where(x => BillSubMissionDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            }
            else
            {
                tfatSearch.Where(x => BillSubMissionDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }
            ctxTFAT.SaveChanges();
        }

        public void PaymentReceivedDetailsColumns(bool Flag, string ViewDataId)
        {
            string[] PaymentReceivedDetailsDetailsColumnsOnly = new string[] { "ReceivedBranch", "ReceivedDate", "ReceivedFrom", "Amount", "TdsAmout", "TotalReceivedAmount" };

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            if (Flag)
            {
                tfatSearch.Where(x => PaymentReceivedDetailsDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            }
            else
            {
                tfatSearch.Where(x => PaymentReceivedDetailsDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }
            ctxTFAT.SaveChanges();
        }

        public void PaymentReceivedChargesDetailsColumns(bool Flag, string ViewDataId)
        {
            string[] PaymentReceivedDetailsDetailsChargesColumnsOnly = new string[] { "Freight Rebate", "Excess Recd", "tds", "Charges-4", "Charges-5", "Charges-6", "Charges-7", "Charges-8", "Charges-9", "Charges-10" };

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            if (Flag)
            {
                var ChargesList = tfatSearch.Where(x => PaymentReceivedDetailsDetailsChargesColumnsOnly.Contains(x.ColHead)).ToList();
                tfatSearch.Where(x => PaymentReceivedDetailsDetailsChargesColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);

                #region Only This Chrges show As Per DocType Rule
                var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "BRC00" && x.DontUse == false).ToList();
                List<string> LRcharge = Lrcharges.Select(x => x.Head).ToList();
                ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                #endregion
            }
            else
            {
                tfatSearch.Where(x => PaymentReceivedDetailsDetailsChargesColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }
            ctxTFAT.SaveChanges();
        }

        public void SetColumnsReset(BillReportVM Model)
        {
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();

            if (Model.ViewDataId == "BillReport")
            {
                tfatSearch.ForEach(x => x.IsHidden = false);
                string[] BillSubMissionDetailsColumnsOnly = new string[] { "SubBranch", "SubDocNo", "SubType", "SubThrough", "SubDate", "SubRemark", "SubENTEREDBY", "SubLASTUPDATEDATE" };
                string[] PaymentReceivedDetailsDetailsColumnsOnly = new string[] { "ReceivedBranch", "ReceivedDate", "ReceivedFrom", "Amount", "TdsAmout", "TotalReceivedAmount" };
                string[] PaymentReceivedDetailsDetailsChargesColumnsOnly = new string[] { "Freight Rebate", "Excess Recd", "tds", "Charges-4", "Charges-5", "Charges-6", "Charges-7", "Charges-8", "Charges-9", "Charges-10" };
                if (Model.PaymentReceivedDetails == true)
                {
                    tfatSearch.Where(x => PaymentReceivedDetailsDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                    if (Model.PaymentReceDetailsChargesReq == true)
                    {
                        var ChargesList = tfatSearch.Where(x => PaymentReceivedDetailsDetailsChargesColumnsOnly.Contains(x.ColHead)).ToList();
                        tfatSearch.Where(x => PaymentReceivedDetailsDetailsChargesColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);

                        #region Only This Chrges show As Per DocType Rule
                        var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "BRC00" && x.DontUse == false).ToList();
                        List<string> LRcharge = Lrcharges.Select(x => x.Head).ToList();
                        ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        #endregion
                    }
                    else
                    {
                        tfatSearch.Where(x => PaymentReceivedDetailsDetailsChargesColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                    }
                }
                else
                {
                    tfatSearch.Where(x => PaymentReceivedDetailsDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                    tfatSearch.Where(x => PaymentReceivedDetailsDetailsChargesColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);

                }


                if (Model.BillSubMissionDetails)
                {
                    tfatSearch.Where(x => BillSubMissionDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                }
                else
                {
                    tfatSearch.Where(x => BillSubMissionDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                }
            }
            else if (Model.ViewDataId == "BillReportWithLR")
            {
                tfatSearch.ForEach(x => x.IsHidden = false);

                string[] ChrColmn = new string[] { "Freight Charges", "LR Charges", "Hamali Charges", "Collection Charges", "Door Delivery Charges", "Varai Charges", "Loading Charges", "Unloading Charges", "Detention Charges", "Union Charges", "Demurrage Charges", "Other Charges", "Charges-13", "Charges-14", "Charges-15", "Charges-16", "Charges-17", "Charges-18", "Charges-19", "Charges-20", "Charges-21", "Charges-22", "Charges-23", "Charges-24", "Charges-25" };
                var ChargesList = tfatSearch.Where(x => ChrColmn.Contains(x.ColHead)).ToList();
                tfatSearch.Where(x => ChrColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);

                #region Only This Chrges show As Per DocType Rule
                var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "LR000" && x.DontUse == false).ToList();
                List<string> LRcharge = Lrcharges.Select(x => x.Head).ToList();
                ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                #endregion
            }
            ctxTFAT.SaveChanges();
        }

        #endregion

        [HttpPost]
        public ActionResult ParameterReset(BillReportVM Model)
        {
            Model.BranchesOnly = PopulateBranchesOnly();
            Model.Customers = PopulateCustomer();
            Model.Masters = PopulateMaster();
            Model.ReportsType = PopulateReportType();
            Model.BillTypes = PopulateBillType();
            Model.Branch = mbranchcode;
            Model.BillType = "SLR00";
            Model.CustomerReq = true;
            string[] Colmn = new string[] { "Branch", "BillDate", "DocDate", "BillNumber", "Taxable", "IGSTAmt", "CGSTAmt", "SGSTAmt", "BillAmt", "RoundOff", "Customer", "AccountHead", "Remark", "ENTEREDBY", "LASTUPDATEDATE" };
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = true);
            tfatSearch.Where(x => Colmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            ctxTFAT.SaveChanges();

            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = DateTime.Today.ToShortDateString();

            var html = ViewHelper.RenderPartialView(this, "_OptionsView", Model);
            var jsonResult = Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        [HttpPost]
        public ActionResult GetParameterAuto(BillReportVM Model)
        {
            var MainReportName = Model.ViewDataId;

            Model.BranchesOnly = PopulateBranchesOnly();
            Model.Customers = PopulateCustomer();
            Model.Masters = PopulateMaster();
            Model.ReportsType = PopulateReportType();
            Model.BillTypes = PopulateBillType();

            Model.ReportTypeL = ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).Select(x => x.ReportName).FirstOrDefault();
            var Tfatsearch = ctxTFAT.TfatSearch.Where(x => x.Code.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower()).ToList();
            Tfatsearch.ForEach(x => x.IsHidden = false);
            //Model.SaveReportList = PopulateSaveReports(MainReportName);
            ReportParameters mobj = new ReportParameters();
            if (ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).FirstOrDefault() != null)
            {
                mobj = ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).FirstOrDefault();

                Model.BillNo = mobj.DocNo;
                Model.FromDate = mobj.StartDate.Value.ToShortDateString();
                Model.ToDate = mobj.EndDate.Value.ToShortDateString();
                Model.HideColumnList = mobj.HideColumnList;

                Model.BillType = mobj.Para1 == null ? "SLR00" : mobj.Para1;

                Model.Branch = mobj.Para2 == null ? "" : mobj.Para2.Replace("'", "");
                Model.CustomerReq = mobj.Para3 == "T" ? true : false;
                Model.Customer = mobj.Para4 == null ? "" : mobj.Para4.Replace("'", "");

                Model.BillSubMissionDetails = mobj.Para5 == "T" ? true : false;
                Model.PaymentReceivedDetails = mobj.Para6 == "T" ? true : false;
                Model.PaymentReceDetailsChargesReq = mobj.Para7 == "T" ? true : false;

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
        public ActionResult SaveParameter(BillReportVM Model)
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


                    mobj.DocNo = Model.BillNo;
                    mobj.StartDate = ConvertDDMMYYTOYYMMDD(Model.FromDate);
                    mobj.EndDate = ConvertDDMMYYTOYYMMDD(Model.ToDate);
                    mobj.HideColumnList = HiddenColumn;

                    mobj.Para1 = Model.BillType;

                    mobj.Para2 = Model.Branch;
                    mobj.Para3 = Model.CustomerReq == true ? "T" : "F";
                    mobj.Para4 = Model.Customer;

                    mobj.Para5 = Model.BillSubMissionDetails == true ? "T" : "F";
                    mobj.Para6 = Model.PaymentReceivedDetails == true ? "T" : "F";
                    mobj.Para7 = Model.PaymentReceDetailsChargesReq == true ? "T" : "F";

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

        public ActionResult DeleteParamete(BillReportVM Model)
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