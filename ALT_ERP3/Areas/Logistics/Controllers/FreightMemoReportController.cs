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
    public class FreightMemoReportController : BaseController
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

        DataTable dataTable = new DataTable();

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
                Text = "FreightMemo Report",
                Value = "FreightMemoReport"
            });
            items.Add(new SelectListItem
            {
                Text = "UN-Paid FM Report",
                Value = "UnPaidFMReport"
            });
            return items;
        }

        private List<SelectListItem> PopulateBroker()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM Master where AcType = 'S' or OthPostType like '%B%'  order by Recordkey ";
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
            return items;
        }

        private List<SelectListItem> PopulateTruckNo()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code,TruckNo,TruckStatus FROM VehicleMaster where Acitve = 'true'   order by Recordkey ";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            string Flag = sdr["TruckStatus"].ToString().Trim() == "100000" ? " - A" : " - O";
                            items.Add(new SelectListItem
                            {
                                Text = sdr["TruckNo"].ToString() + Flag,
                                Value = sdr["Code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code,TruckNo FROM HireVehicleMaster where Acitve = 'true'   order by Recordkey ";
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
                                Text = sdr["TruckNo"].ToString()+ " - H",
                                Value = sdr["Code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }

            return items;
        }

        public JsonResult Branch(string term)
        {
            List<TfatBranch> list = ctxTFAT.TfatBranch.Where(x => x.Category != "Zone" && x.Category != "0").ToList();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }

            List<TfatBranch> treeTables = new List<TfatBranch>();

            foreach (var item in list)
            {
                if (item.Category == "Zone")
                {
                    item.Name += " - Z";
                    treeTables.Add(item);
                }
                else if (item.Category == "Branch")
                {
                    item.Name += " - B";
                    treeTables.Add(item);
                }
                else if (item.Category == "SubBranch")
                {
                    item.Name += " - SB";
                    treeTables.Add(item);
                }
                else
                {
                    item.Name = item.Name + " - A";
                    treeTables.Add(item);
                }
            }
            var Modified = treeTables.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBroker(string term)
        {
            var result = ctxTFAT.Master.Where(x => x.Hide == false /*&& x.IsLast== true*/ && (x.AcType == "S" || x.RelatedTo == "6")).Select(m => new { m.Code, m.Name }).ToList();
            if (!(String.IsNullOrEmpty(term)))
            {
                result = ctxTFAT.Master.Where(x => x.Hide == false /*&& x.IsLast== true*/ && (x.AcType == "S" || x.RelatedTo == "6") && x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).ToList();
            }
            var Modified = result.Select(x => new
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

        #endregion

        // GET: Logistics/FreightMemoReport
        public ActionResult Index(FreightMemoReportVM Model)
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
            Model.Brokers = PopulateBroker();
            Model.Drivers = PopulateDriver();
            Model.TruckNos = PopulateTruckNo();
            Model.ReportsType = PopulateReportType();
            Model.ReportTypeL = Model.ViewDataId;
            //Model.DispatchDetails = true;
            //Model.LorryReceiptDetails = true;
            string[] HideColmn = new string[] { "P/L AC", "FMTablekey", "VDTablekey", "LCTablekey", "LRTablekey", "LREXPTablekey", "FMEXPTablekey", "TripTablekey" };
            string[] DefaultColumnsOnly = new string[] { "Date",    "FmNo", "Branch",   "[From]",   "RouteViaName", "[TO]", "TruckNo",  "Driver",   "Broker",   "Category", "PaidAt",   "Freight",  "Advance",  "Balance" };
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = true);
            tfatSearch.Where(x => DefaultColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            tfatSearch.Where(x => HideColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            ctxTFAT.SaveChanges();

            return View(Model);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(FreightMemoReportVM Model)
        {
            try
            {
                TempData["FMBranch"] = Model.Branch;
                TempData["FMLoadFrom"] = Model.LoadFrom;
                TempData["FMSendTo"] = Model.SendTo;
                TempData["FMBroker"] = Model.Broker;
                TempData["FMDriver"] = Model.Driver;
                TempData["FMPAyableAt"] = Model.PAyableAt;
                TempData["FMTruckNo"] = Model.TruckNo;

                TempData["FMPaymentDetails"] = Model.PaymentDetails;
                TempData["FMAdvance"] = Model.Advance;
                TempData["FMBalance"] = Model.Balance;
                if (Model.VehicleType=="A")
                {
                    Model.VehicleType = "";
                }
                else if (Model.VehicleType == "AA")
                {
                    Model.VehicleType = "FM.VehicleStatus=100000";
                }
                else if (Model.VehicleType == "H")
                {
                    Model.VehicleType = "FM.VehicleStatus=100001";
                }
                else if (Model.VehicleType == "O")
                {
                    Model.VehicleType = "FM.VehicleStatus=100002";
                }
                TempData["FMVehicleType"] = Model.VehicleType;
                TempData["FMFMExpensesDetails"] = Model.FMExpensesDetails;
                TempData["FMLRExpensesDetails"] = Model.LRExpensesDetails;
                TempData["FMDispatchDetails"] = Model.DispatchDetails;
                TempData["FMLorryReceiptDetails"] = Model.LorryReceiptDetails;
                TempData["FMTripDetails"] = Model.TripDetails;
                TempData["FMPLAccount"] = Model.PLAccount;

                var ReportHeader = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).FirstOrDefault();
                ReportHeader.pMerge = "3,16^3,56^33,50^27,33^3,27^3,62^3";
                ReportHeader.pToMerge = "16,17,18,19,20,21^56,57,58,59,60,61^50,51,52,53,54,55^31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49^24,25,26,27,28,29,30^62,63,64,65,66,67,68,69,70^1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,22,23,71";

                Model.GetReportParameter = true;
                var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();

                if (Model.GetReportParameter == false)
                {
                    tfatSearch.ForEach(x => x.IsHidden = false);
                    string[] HideColmn = new string[] { "FMTablekey", "VDTablekey", "LCTablekey", "LRTablekey", "LREXPTablekey", "FMEXPTablekey", "TripTablekey" };
                    tfatSearch.Where(x => HideColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);

                    string[] PaymentDetailsColumnsOnly = new string[] { "VouDate", "PayBranch", "Account", "NetAmt", "PaidOF" };
                    if (Model.PaymentDetails == false)
                    {
                        tfatSearch.Where(x => PaymentDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                    }

                    string[] BalanceColumnsOnly = new string[] { "AdvBal", "Bal" };
                    if (Model.Advance == false && Model.Balance == false)
                    {
                        tfatSearch.Where(x => BalanceColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                    }
                    else if (Model.Advance == true && Model.Balance == false)
                    {
                        tfatSearch.Where(x => x.ColHead == "Bal").ToList().ForEach(x => x.IsHidden = true);
                    }
                    else if (Model.Advance == false && Model.Balance == true)
                    {
                        tfatSearch.Where(x => x.ColHead == "AdvBal").ToList().ForEach(x => x.IsHidden = true);
                    }

                    string[] LorryReceiptDetailsColumnsOnly = new string[] { "LRBranch", "LrNo", "LRDate", "LRFrom", "LRTo", "Consignor", "Consignee", "BillParty", "LRtype", "BillBranch", "Collection", "Delivery", "LRQty", "Unit", "ActWt", "ChgWt", "ChargeType","Bill Amt" };
                    if (Model.LorryReceiptDetails == false)
                    {
                        tfatSearch.Where(x => LorryReceiptDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                    }

                    string[] FMExpensesDetailsColumnsOnly = new string[] { "FM-ExpBranch", "FM-ExpAccount", "FM-ExpDate", "FM-ExpAmount", "FM-IncAmount" };
                    if (Model.FMExpensesDetails == false)
                    {
                        tfatSearch.Where(x => FMExpensesDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                    }

                    string[] LRExpensesDetailsColumnsOnly = new string[] { "LR-ExpBranch", "LR-ExpAccount", "LR-ExpDate", "LR-ExpAmount", "LR-IncAmount" };
                    if (Model.LRExpensesDetails == false)
                    {
                        tfatSearch.Where(x => LRExpensesDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                    }

                    string[] DispatchDetailsColumnsOnly = new string[] { "LCBranch", "LCDate", "LCno", "LCFrom", "LCTo", "LCQty" };
                    if (Model.DispatchDetails == false)
                    {
                        tfatSearch.Where(x => DispatchDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                    }

                    string[] TripDetailsColmn = new string[] { "TripNo", "TripBranch", "TripDate", "TripDriver", "TripCharge", "LocalCharge", "ViaCharge", "TripTotal" };
                    if (Model.TripDetails == false)
                    {
                        tfatSearch.Where(x => TripDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                    }

                    string[] PLDetailsColmn = new string[] { "P/L AC" };
                    if (Model.PLAccount)
                    {
                        tfatSearch.Where(x => PLDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                    }
                    else
                    {
                        tfatSearch.Where(x => PLDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                    }
                }
                else
                {
                    string[] PLDetailsColmn = new string[] { "P/L AC" };
                    if (Model.PLAccount)
                    {
                        tfatSearch.Where(x => PLDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                    }
                    else
                    {
                        tfatSearch.Where(x => PLDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
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

        public ActionResult GetGridData(FreightMemoReportVM Model)
        {
            if (String.IsNullOrEmpty(Model.mWhat))
            {
                ExecuteStoredProc("Drop Table ztmp_FreightMemoReports");
                SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                SqlCommand cmd = new SqlCommand("SP_FreightMemoReports", tfat_conx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;
                cmd.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = ConvertDDMMYYTOYYMMDD(Model.FromDate);
                cmd.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = ConvertDDMMYYTOYYMMDD(Model.ToDate);
                cmd.Parameters.Add("@ReportType", SqlDbType.VarChar).Value = Model.ReportTypeL;


                cmd.Parameters.Add("@FMNO", SqlDbType.VarChar).Value = Model.FreightMemoNo;
                cmd.Parameters.Add("@Branch", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("FMBranch"));
                cmd.Parameters.Add("@FromBranch", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("FMLoadFrom"));
                cmd.Parameters.Add("@ToBranch", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("FMSendTo"));
                cmd.Parameters.Add("@Broker", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("FMBroker"));
                cmd.Parameters.Add("@Payable", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("FMPAyableAt"));
                cmd.Parameters.Add("@TruckNo", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("FMTruckNo"));
                if (Model.ReportTypeL== "UnPaidFMReport")
                {
                    cmd.Parameters.Add("@PaymentDetails", SqlDbType.VarChar).Value = true;
                    cmd.Parameters.Add("@Advance", SqlDbType.VarChar).Value = true;
                    cmd.Parameters.Add("@Balance", SqlDbType.VarChar).Value = true;
                }
                else
                {
                    cmd.Parameters.Add("@PaymentDetails", SqlDbType.VarChar).Value = Convert.ToBoolean(TempData.Peek("FMPaymentDetails"));
                    cmd.Parameters.Add("@Advance", SqlDbType.VarChar).Value = Convert.ToBoolean(TempData.Peek("FMAdvance"));
                    cmd.Parameters.Add("@Balance", SqlDbType.VarChar).Value = Convert.ToBoolean(TempData.Peek("FMBalance"));
                }
                

                cmd.Parameters.Add("@mVehicleType", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("FMVehicleType"));

                cmd.Parameters.Add("@FMExpensesDetails", SqlDbType.VarChar).Value = Convert.ToBoolean(TempData.Peek("FMFMExpensesDetails"));
                cmd.Parameters.Add("@LRExpensesDetails", SqlDbType.VarChar).Value = Convert.ToBoolean(TempData.Peek("FMLRExpensesDetails"));
                cmd.Parameters.Add("@DispatchDetails", SqlDbType.VarChar).Value = Convert.ToBoolean(TempData.Peek("FMDispatchDetails"));
                cmd.Parameters.Add("@LorryReceiptDetails", SqlDbType.VarChar).Value = Convert.ToBoolean(TempData.Peek("FMLorryReceiptDetails"));
                cmd.Parameters.Add("@TripDetails", SqlDbType.VarChar).Value = Convert.ToBoolean(TempData.Peek("FMTripDetails"));

                cmd.Parameters.Add("@Driver", SqlDbType.VarChar).Value = Convert.ToString(TempData.Peek("FMDriver"));

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

        public void A_B_ALL_DetailsColumns(bool AFlag, bool BFlag, string ViewDataId)
        {
            string[] BalanceColumnsOnly = new string[] { "AdvBal", "Bal" };
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            tfatSearch.Where(x => BalanceColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);

            if (AFlag == false && BFlag == false)
            {
                tfatSearch.Where(x => BalanceColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            }
            else if (AFlag == true && BFlag == false)
            {
                tfatSearch.Where(x => x.ColHead == "AdvBal").ToList().ForEach(x => x.IsHidden = false);
            }
            else if (AFlag == false && BFlag == true)
            {
                tfatSearch.Where(x => x.ColHead == "Bal").ToList().ForEach(x => x.IsHidden = false);
            }

            ctxTFAT.SaveChanges();
        }

        public void FM_Expense_DetailsColumns(bool Flag, string ViewDataId)
        {
            string[] FMExpensesDetailsColumnsOnly = new string[] { "FM-ExpBranch", "FM-ExpAccount", "FM-ExpDate", "FM-ExpAmount", "FM-IncAmount" };

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            if (Flag)
            {
                tfatSearch.Where(x => FMExpensesDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            }
            else
            {
                tfatSearch.Where(x => FMExpensesDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }
            ctxTFAT.SaveChanges();
        }

        public void LR_Expense_DetailsColumns(bool Flag, string ViewDataId)
        {
            string[] LRExpensesDetailsColumnsOnly = new string[] { "LR-ExpBranch", "LR-ExpAccount", "LR-ExpDate", "LR-ExpAmount", "LR-IncAmount" };

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

        public void LC_DetailsColumns(bool Flag, string ViewDataId)
        {
            string[] DispatchDetailsColumnsOnly = new string[] { "LCBranch", "LCDate", "LCno", "LCFrom", "LCTo", "LCQty" };

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            if (Flag)
            {
                tfatSearch.Where(x => DispatchDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            }
            else
            {
                tfatSearch.Where(x => DispatchDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }
            ctxTFAT.SaveChanges();
        }

        public void LR_DetailsColumns(bool Flag, string ViewDataId)
        {
            string[] LorryReceiptDetailsColumnsOnly = new string[] { "LRBranch", "LrNo", "LRDate", "LRFrom", "LRTo", "Consignor", "Consignee", "BillParty", "LRtype", "BillBranch", "Collection", "Delivery", "LRQty", "Unit", "ActWt", "ChgWt", "ChargeType", "Bill Amt" };

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            if (Flag)
            {
                tfatSearch.Where(x => LorryReceiptDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            }
            else
            {
                tfatSearch.Where(x => LorryReceiptDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }
            ctxTFAT.SaveChanges();
        }

        public void Trip_DetailsColumns(bool Flag, string ViewDataId)
        {
            string[] TripDetailsColmn = new string[] { "TripNo", "TripBranch", "TripDate", "TripDriver", "TripCharge", "LocalCharge", "ViaCharge", "TripTotal" };

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            if (Flag)
            {
                tfatSearch.Where(x => TripDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            }
            else
            {
                tfatSearch.Where(x => TripDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }
            ctxTFAT.SaveChanges();
        }

        public void Payment_DetailsColumns(bool Flag, string ViewDataId)
        {
            string[] PaymentDetailsColumnsOnly = new string[] { "VouDate", "PayBranch", "Account", "NetAmt", "PaidOF" };

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            if (Flag)
            {
                tfatSearch.Where(x => PaymentDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            }
            else
            {
                tfatSearch.Where(x => PaymentDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }
            ctxTFAT.SaveChanges();
        }

        public void SetColumnsReset(FreightMemoReportVM Model)
        {
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = false);
            string[] HideColmn = new string[] { "P/L AC", "FMTablekey", "VDTablekey", "LCTablekey", "LRTablekey", "LREXPTablekey", "FMEXPTablekey", "TripTablekey" };
            tfatSearch.Where(x => HideColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);

            string[] PaymentDetailsColumnsOnly = new string[] { "VouDate", "PayBranch", "Account", "NetAmt", "PaidOF" };
            if (Model.PaymentDetails == false)
            {
                tfatSearch.Where(x => PaymentDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }

            string[] TripDetailsColmn = new string[] { "TripNo", "TripBranch", "TripDate", "TripDriver", "TripCharge", "LocalCharge", "ViaCharge", "TripTotal" };
            if (Model.TripDetails == false)
            {
                tfatSearch.Where(x => TripDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }

            string[] BalanceColumnsOnly = new string[] { "AdvBal", "Bal" };
            if (Model.Advance == false && Model.Balance == false)
            {
                tfatSearch.Where(x => BalanceColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }
            else if (Model.Advance == true && Model.Balance == false)
            {
                tfatSearch.Where(x => x.ColHead == "Bal").ToList().ForEach(x => x.IsHidden = true);
            }
            else if (Model.Advance == false && Model.Balance == true)
            {
                tfatSearch.Where(x => x.ColHead == "AdvBal").ToList().ForEach(x => x.IsHidden = true);
            }

            string[] LorryReceiptDetailsColumnsOnly = new string[] { "LRBranch", "LrNo", "LRDate", "LRFrom", "LRTo", "Consignor", "Consignee", "BillParty", "LRtype", "BillBranch", "Collection", "Delivery", "LRQty", "Unit", "ActWt", "ChgWt", "ChargeType", "Bill Amt" };
            if (Model.LorryReceiptDetails == false)
            {
                tfatSearch.Where(x => LorryReceiptDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }

            string[] FMExpensesDetailsColumnsOnly = new string[] { "FM-ExpBranch", "FM-ExpAccount", "FM-ExpDate", "FM-ExpAmount", "FM-IncAmount" };
            if (Model.FMExpensesDetails == false)
            {
                tfatSearch.Where(x => FMExpensesDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }

            string[] LRExpensesDetailsColumnsOnly = new string[] { "LR-ExpBranch", "LR-ExpAccount", "LR-ExpDate", "LR-ExpAmount", "LR-IncAmount" };
            if (Model.LRExpensesDetails == false)
            {
                tfatSearch.Where(x => LRExpensesDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }

            string[] DispatchDetailsColumnsOnly = new string[] { "LCBranch", "LCDate", "LCno", "LCFrom", "LCTo", "LCQty" };
            if (Model.DispatchDetails == false)
            {
                tfatSearch.Where(x => DispatchDetailsColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }

            ctxTFAT.SaveChanges();
        }

        public void PLAccountColumns(bool Flag, string ViewDataId)
        {
            string[] TripDetailsColmn = new string[] { "P/L AC" };

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            if (Flag)
            {
                tfatSearch.Where(x => TripDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            }
            else
            {
                tfatSearch.Where(x => TripDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }
            ctxTFAT.SaveChanges();
        }

        #endregion

        public ActionResult ParameterReset(FreightMemoReportVM Model)
        {
            Model.Branches = PopulateBranches();
            Model.BranchesOnly = PopulateBranchesOnly();
            Model.ReportsType = PopulateReportType();
            Model.Brokers = PopulateBroker();
            Model.Drivers = PopulateDriver();
            Model.TruckNos = PopulateTruckNo();
            Model.Branch = mbranchcode;

            string[] DefaultColumnsOnly = new string[] { "Date", "FmNo", "Branch", "[From]", "RouteViaName", "[TO]", "TruckNo", "Driver", "Broker", "Category", "PaidAt", "Freight", "Advance", "Balance" };
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = true);
            tfatSearch.Where(x => DefaultColumnsOnly.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            ctxTFAT.SaveChanges();

            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = DateTime.Today.ToShortDateString();

            var html = ViewHelper.RenderPartialView(this, "_OptionsView", Model);
            var jsonResult = Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }
        public ActionResult GetParameterAuto(FreightMemoReportVM Model)
        {
            var MainReportName = Model.ViewDataId;

            Model.Branches = PopulateBranches();
            Model.BranchesOnly = PopulateBranchesOnly();
            Model.Brokers = PopulateBroker();
            Model.Drivers = PopulateDriver();
            Model.TruckNos = PopulateTruckNo();
            Model.ReportsType = PopulateReportType();

            Model.ReportTypeL = ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).Select(x => x.ReportName).FirstOrDefault();
            var Tfatsearch = ctxTFAT.TfatSearch.Where(x => x.Code.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower()).ToList();
            Tfatsearch.ForEach(x => x.IsHidden = false);
            //Model.SaveReportList = PopulateSaveReports(MainReportName);
            ReportParameters mobj = new ReportParameters();
            if (ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).FirstOrDefault() != null)
            {
                mobj = ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).FirstOrDefault();

                Model.FreightMemoNo = mobj.DocNo;
                Model.FromDate = mobj.StartDate.Value.ToShortDateString();
                Model.ToDate = mobj.EndDate.Value.ToShortDateString();
                Model.HideColumnList = mobj.HideColumnList;

                Model.VehicleType = mobj.Para1;

                Model.Branch = mobj.Para2 == null ? "" : mobj.Para2.Replace("'", "");
                Model.LoadFrom = mobj.Para3 == null ? "" : mobj.Para3.Replace("'", "");
                Model.SendTo = mobj.Para4 == null ? "" : mobj.Para4.Replace("'", "");

                Model.Broker = mobj.Para5 == null ? "" : mobj.Para5.Replace("'", "");
                Model.PAyableAt = mobj.Para6 == null ? "" : mobj.Para6.Replace("'", "");
                Model.TruckNo = mobj.Para7 == null ? "" : mobj.Para7.Replace("'", "");

                Model.FMExpensesDetails = mobj.Para8 == "T" ? true : false;
                Model.DispatchDetails = mobj.Para9 == "T" ? true : false;
                Model.LorryReceiptDetails = mobj.Para10 == "T" ? true : false;

                Model.LRExpensesDetails = mobj.Para11 == "T" ? true : false;
                Model.PaymentDetails = mobj.Para12 == "T" ? true : false;
                Model.Advance = mobj.Para13 == "T" ? true : false;
                Model.Balance = mobj.Para14 == "T" ? true : false;
                Model.TripDetails = mobj.Para15 == "T" ? true : false;
                Model.Driver = mobj.Para16 == null ? "" : mobj.Para16.Replace("'", "");
                Model.PLAccount = mobj.Para17 == "T" ? true : false;

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
        public ActionResult SaveParameter(FreightMemoReportVM Model)
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


                    mobj.DocNo = Model.FreightMemoNo;
                    mobj.StartDate = ConvertDDMMYYTOYYMMDD(Model.FromDate);
                    mobj.EndDate = ConvertDDMMYYTOYYMMDD(Model.ToDate);
                    mobj.HideColumnList = HiddenColumn;

                    mobj.Para1 = Model.VehicleType;

                    mobj.Para2 = Model.Branch;
                    mobj.Para3 = Model.LoadFrom;
                    mobj.Para4 = Model.SendTo;

                    mobj.Para5 = Model.Broker;
                    mobj.Para6 = Model.PAyableAt;
                    mobj.Para7 = Model.TruckNo;

                    mobj.Para8 = Model.FMExpensesDetails == true ? "T" : "F";
                    mobj.Para9 = Model.DispatchDetails == true ? "T" : "F";
                    mobj.Para10 = Model.LorryReceiptDetails == true ? "T" : "F";

                    mobj.Para11 = Model.LRExpensesDetails == true ? "T" : "F";
                    mobj.Para12 = Model.PaymentDetails == true ? "T" : "F";
                    mobj.Para13 = Model.Advance == true ? "T" : "F";
                    mobj.Para14 = Model.Balance == true ? "T" : "F";
                    mobj.Para15 = Model.TripDetails == true ? "T" : "F";

                    mobj.Para16 = Model.Driver;
                    mobj.Para17 = Model.PLAccount == true ? "T" : "F";


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
        public ActionResult DeleteParamete(FreightMemoReportVM Model)
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