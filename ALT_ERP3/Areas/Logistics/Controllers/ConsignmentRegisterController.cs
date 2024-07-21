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
    public class ConsignmentRegisterController : BaseController
    {
        // GET: Logistics/BranchStockReport
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
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
        #region Function List

        private List<SelectListItem> PopulateBranches()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM TfatBranch where Category='Branch' or Category='SubBranch' or Category='Area' order by Recordkey ";
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
        private List<SelectListItem> PopulateFromBranches()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM TfatBranch where (Category='Branch' or Category='SubBranch' or Category='Area') and Code In (select * from  [dbo].[SP_GetBranchChild]('" + mbranchcode + "')) order by Recordkey ";
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

        private List<SelectListItem> PopulateStockTypes()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem
            {
                Text = "Godown",
                Value = "LR"
            });
            items.Add(new SelectListItem
            {
                Text = "Transit",
                Value = "TRN"
            });

            return items;
        }
        private List<SelectListItem> PopulateStockBranchesOnly()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM TfatBranch where (Category<>'Area' and BranchType='G') or Code='G00000' order by Recordkey ";
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
        private List<SelectListItem> PopulateFromBranchesOnly()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM TfatBranch where Category<>'Area' and BranchType='G'  and Code In (select * from  [dbo].[SP_GetBranchChild]('" + mbranchcode + "')) order by Recordkey ";
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
                Text = "Consignment Report",
                //Value = "ConsigmentReports"
                Value = "LorryReceiptReports"
            });

            items.Add(new SelectListItem
            {
                Text = "UN-Bill Consignment Report",
                Value = "UNBillLorryReceipt"
            });

            return items;
        }
        private List<SelectListItem> PopulateLrType()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "select Code,[LRType] from LRTypeMaster order by Recordkey ";
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
                                Text = sdr["LRType"].ToString(),
                                Value = sdr["Code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return items;
        }
        private List<SelectListItem> PopulateConsigner()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "select Code,Name from Consigner order by Recordkey ";
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
        private List<SelectListItem> PopulateBillParty()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "select Code,Name from CustomerMaster  order by Recordkey ";
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
        private List<SelectListItem> PopulateChargeType()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "select Code,[ChargeType] from ChargeTypeMaster order by Recordkey ";
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
                                Text = sdr["ChargeType"].ToString(),
                                Value = sdr["Code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return items;
        }
        private List<SelectListItem> PopulateParticulars()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "select Code,Description from DescriptionMaster order by Recordkey ";
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
                                Text = sdr["Description"].ToString(),
                                Value = sdr["Code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return items;
        }
        private List<SelectListItem> PopulateUnit()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "select Code,Name from UnitMaster order by Recordkey ";
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
        private List<SelectListItem> PopulateCollection()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem
            {
                Text = "Godown",
                Value = "G"
            });
            items.Add(new SelectListItem
            {
                Text = "Direct",
                Value = "D"
            });

            return items;
        }
        private List<SelectListItem> PopulateDelivery()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem
            {
                Text = "Godown",
                Value = "G"
            });
            items.Add(new SelectListItem
            {
                Text = "Door",
                Value = "D"
            });

            return items;
        }
        private List<SelectListItem> PopulateDateRange()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem
            {
                Text = "All",
                Value = "All"
            });
            items.Add(new SelectListItem
            {
                Text = "Current Year",
                Value = "CY"
            });
            items.Add(new SelectListItem
            {
                Text = "Current Month",
                Value = "CM"
            });
            items.Add(new SelectListItem
            {
                Text = "Today Only",
                Value = "TD"
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

        // GET: Logistics/ConsignmentRegister
        public ActionResult Index(LorryReceiptReportsVM Model)
        {
            UpdateAuditTrail(mbranchcode, "Report", Model.Header, "", DateTime.Now, 0, "", "", "NA");
            if (Model.Document == "" || Model.Document == null)
            {
                GetAllMenu(Session["ModuleName"].ToString());
                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
                Model.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
                Model.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
                //System.Web.HttpContext.Current.Session["LastDate"].ToString();
            }
            if (Model.FromDate == null)
            {
                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
                Model.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
                Model.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
            }

            if (Model.ViewDataId== "UNBillLorryReceipt")
            {
                var branchStockReport = ctxTFAT.BranchUNBillLorryReceiptSetup.FirstOrDefault();
                if (branchStockReport != null)
                {
                    if (branchStockReport.Year)
                    {
                        Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                        Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
                        Model.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
                        Model.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
                    }
                    else if (branchStockReport.Days30)
                    {
                        Model.ToDate = DateTime.Today.ToShortDateString();
                        var TodaysDate = ConvertDDMMYYTOYYMMDD(Model.ToDate);
                        Model.FromDate = TodaysDate.AddDays(-30).ToShortDateString();
                        Model.StartDate = ConvertDDMMYYTOYYMMDD(Model.FromDate).ToString("yyyy-MM-dd");
                        Model.EndDate = ConvertDDMMYYTOYYMMDD(Model.ToDate).ToString("yyyy-MM-dd");
                    }
                    else if (branchStockReport.Days60)
                    {
                        Model.ToDate = DateTime.Today.ToShortDateString();
                        var TodaysDate = ConvertDDMMYYTOYYMMDD(Model.ToDate);
                        Model.FromDate = TodaysDate.AddDays(-60).ToShortDateString();
                        Model.StartDate = ConvertDDMMYYTOYYMMDD(Model.FromDate).ToString("yyyy-MM-dd");
                        Model.EndDate = ConvertDDMMYYTOYYMMDD(Model.ToDate).ToString("yyyy-MM-dd");
                    }
                    else if (branchStockReport.Days90)
                    {
                        Model.ToDate = DateTime.Today.ToShortDateString();
                        var TodaysDate = ConvertDDMMYYTOYYMMDD(Model.ToDate);
                        Model.FromDate = TodaysDate.AddDays(-90).ToShortDateString();
                        Model.StartDate = ConvertDDMMYYTOYYMMDD(Model.FromDate).ToString("yyyy-MM-dd");
                        Model.EndDate = ConvertDDMMYYTOYYMMDD(Model.ToDate).ToString("yyyy-MM-dd");
                    }
                }
            }
            else
            {
                var branchStockReport = ctxTFAT.ConsignmentRegisterSetup.FirstOrDefault();
                if (branchStockReport != null)
                {
                    if (branchStockReport.Year)
                    {
                        Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                        Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
                        Model.StartDate = ConvertDDMMYYTOYYMMDD(Model.FromDate).ToString("yyyy-MM-dd");
                        Model.EndDate = ConvertDDMMYYTOYYMMDD(Model.ToDate).ToString("yyyy-MM-dd");
                    }
                    else if (branchStockReport.Days30)
                    {
                        Model.ToDate = DateTime.Today.ToShortDateString();
                        var TodaysDate = ConvertDDMMYYTOYYMMDD(Model.ToDate);
                        Model.FromDate = TodaysDate.AddDays(-30).ToShortDateString();
                        Model.StartDate = ConvertDDMMYYTOYYMMDD(Model.FromDate).ToString("yyyy-MM-dd");
                        Model.EndDate = ConvertDDMMYYTOYYMMDD(Model.ToDate).ToString("yyyy-MM-dd");
                    }
                    else if (branchStockReport.Days60)
                    {
                        Model.ToDate = DateTime.Today.ToShortDateString();
                        var TodaysDate = ConvertDDMMYYTOYYMMDD(Model.ToDate);
                        Model.FromDate = TodaysDate.AddDays(-60).ToShortDateString();
                        Model.StartDate = ConvertDDMMYYTOYYMMDD(Model.FromDate).ToString("yyyy-MM-dd");
                        Model.EndDate = ConvertDDMMYYTOYYMMDD(Model.ToDate).ToString("yyyy-MM-dd");
                    }
                    else if (branchStockReport.Days90)
                    {
                        Model.ToDate = DateTime.Today.ToShortDateString();
                        var TodaysDate = ConvertDDMMYYTOYYMMDD(Model.ToDate);
                        Model.FromDate = TodaysDate.AddDays(-90).ToShortDateString();
                        Model.StartDate = ConvertDDMMYYTOYYMMDD(Model.FromDate).ToString("yyyy-MM-dd");
                        Model.EndDate = ConvertDDMMYYTOYYMMDD(Model.ToDate).ToString("yyyy-MM-dd");
                    }
                }
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

            Model.ReportsType = PopulateReportType();
            Model.StockTypes = PopulateStockTypes();
            Model.StockBranchesOnly = PopulateStockBranchesOnly();
            Model.BranchesOnly = PopulateBranchesOnly();
            Model.FromBranchesOnly = PopulateFromBranchesOnly();
            Model.Branches = PopulateBranches();
            Model.FromBranches = PopulateFromBranches();

            Model.LrTypes = PopulateLrType();
            Model.Consignors = PopulateConsigner();
            Model.BillingPartys = PopulateBillParty();
            Model.ChargeTypes = PopulateChargeType();
            Model.Particulars = PopulateParticulars();
            Model.Units = PopulateUnit();
            Model.Collections = PopulateCollection();
            Model.Delivereries = PopulateDelivery();
            Model.DateRanges = PopulateDateRange();
            Model.FromBranch = String.IsNullOrEmpty(Model.FromBranch) == true ? mbranchcode : Model.FromBranch;

            //Model.BillRelationDetails = true;
            //Model.DispatchDetails = true;
            //Model.DeliveryDetails = true;
            //Model.ExpensesDetails = true;
            Model.ReportTypeL = String.IsNullOrEmpty(Model.ReportTypeL) == true ? Model.ViewDataId : Model.ReportTypeL;

            string[] Colmn = new string[] { "EntryDate", "Time", "Vehicleno", "Branch", "LrNo", "Date", "From", "To", "Consignor", "Consignee", "BillParty", "LRtype", "BillBranch", "Collection", "Delivery", "Qty", "Unit", "ActWt", "ChgWt", "ChargeType", "Amt", "Description", "PartyChallan", "PartyInvoice", "Vehicleno", "PONumber", "BENumber", "DecVal", "GSTNO", "EwayBill", "LRMode", "ServiceType", "Remark", "UserName", "Status", "TAT", "Note", "Note Date", "Note CreateBy" };
            string[] DispatchDetailsColmn1 = new string[] { "FMPF001", "FMPF002", "FMPF003", "FMPF004", "FMPF005", "FMPF006", "FMPF007", "FMPF008", "FMPF009", "FMPF010" };
            string[] HideColmn = new string[] { "P/L AC", "STKTablekey", "LRTablekey", "LCTablekey", "FMTablekey", "FMPTablekey", "DELTablekey", "BillTablekey", "EXPTablekey", "ALRTTablekey", "TripTablekey" };
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = true);
            tfatSearch.Where(x => Colmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            tfatSearch.Where(x => HideColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            ctxTFAT.SaveChanges();
            Model.AllHeaderList = ctxTFAT.Charges.Where(x => x.Type == "LR000").Select(x => x.Head).ToList();
            Model.AllFMPHeaderList = ctxTFAT.Charges.Where(x => x.Type == "FMP00").Select(x => x.Head).ToList();
            Model.AllFMHeaderList = ctxTFAT.Charges.Where(x => x.Type == "FM000").Select(x => x.Head).ToList();
            //goButton_Click();
            //SampleMerging();
            return View(Model);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(LorryReceiptReportsVM Model)
        {
            try
            {
                string mbranch = ppara05 == null || ppara05 == "" ? "'" + mbranchcode + "'" : ppara05;
                Model.GetReportParameter = true;
                if (Model.ViewDataId == "LorryReceiptReports")
                {
                    var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
                    var ReportHeader = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).FirstOrDefault();
                    if (Model.SkipDuplicateFM)
                    {
                        //                   LR,Trip^LR,ALT^LR,EXP^LR,SLR^LR,DEL^FM,FMP^FM^LR,LC^LR                                                              
                        ReportHeader.pMerge = "5,122^5,118^5,112^5,108^5,101^72,88^72^5,65^+131^5";
                        ReportHeader.pToMerge = "122,123,124,125,126,127,128,129,130^118,119,120,121^112,113,114,115,116,117^104,105,106,107,108,109,110,111^99,100,101,102,103^88,89,90,91,92,93,94,95,96,97,98^69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87^62,63,64,65,66,67,68^109,117-73,78,79,80,81,82,83,84,85,86,87,89,90,91,92,93,94,95,96,97,98,116,130^1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,131";
                    }
                    else
                    {
                        //                   LR,Trip^LR,ALT^LR,EXP^LR,SLR^LR,DEL^FM,FMP^LC,FM^LR,LC^LR                                                              
                        ReportHeader.pMerge = "5,122^5,118^5,112^5,108^5,101^72,88^65,72^5,65^+131^5";
                        ReportHeader.pToMerge = "122,123,124,125,126,127,128,129,130^118,119,120,121^112,113,114,115,116,117^104,105,106,107,108,109,110,111^99,100,101,102,103^88,89,90,91,92,93,94,95,96,97,98^69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87^62,63,64,65,66,67,68^109,117-73,78,79,80,81,82,83,84,85,86,87,89,90,91,92,93,94,95,96,97,98,116,130^1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,131";
                    }

                    if (Model.GetReportParameter == false)
                    {
                        tfatSearch.ForEach(x => x.IsHidden = false);
                        //string[] ChrColmn = new string[] { "F001", "Freight Charges", "LR Charges", "Hamali Charges", "Collection Charges", "Door Delivery Charges", "Varai Charges", "Loading Charges", "Unloading Charges", "Detention Charges", "Union Charges", "Val11", "Val12", "Val13", "Val14", "Val15", "Val16", "Val17", "Val18", "Val19", "Val20", "Val21", "Val22", "Val23", "Val24", "Val25" };
                        string[] ChrColmn = new string[] { "F001", "F002", "F003", "F004", "F005", "F006", "F007", "F008", "F009", "F010", "F011", "F012", "F013", "F014", "F015", "F016", "F017", "F018", "F019", "F020", "F021", "F022", "F023", "F024", "F025" };

                        if (Model.ChargeShow || Model.BillChargeShow)
                        {
                            var ChargesList = tfatSearch.Where(x => ChrColmn.Contains(x.ColHead)).ToList();
                            tfatSearch.Where(x => ChrColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            #region Only This Chrges show As Per DocType Rule
                            var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "LR000" && x.DontUse == false).ToList();
                            List<string> LRcharge = Lrcharges.Select(x => x.Fld).ToList();
                            ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                            #endregion
                        }
                        else
                        {
                            tfatSearch.Where(x => ChrColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }

                        string[] BillDetailsColmn = new string[] { "BillType", "SaleBillBranch", "BillDate", "BillAmt", "SaleBillParty", "BillQty", "BillNumber" };
                        if (Model.BillRelationDetails)
                        {
                            tfatSearch.Where(x => BillDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        }
                        else
                        {
                            tfatSearch.Where(x => BillDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }

                        string[] DispatchDetailsColmn = new string[] { "LCBranch", "LCDate", "LCno", "LCFrom", "LCTo", "LCQty", "FMBranch", "FMDate", "FMNo", "Fm Freight", "Broker", "FM Driver", "Vehicle NO", "Vehicle Type" };
                        string[] DispatchDetailsColmn1 = new string[] { "FMF001", "FMF002", "FMF003", "FMF004", "FMF005", "FMF006", "FMF007", "FMF008", "FMF009", "FMF010" };

                        if (Model.DispatchDetails)
                        {
                            tfatSearch.Where(x => DispatchDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            #region Only This Chrges show As Per DocType Rule
                            var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "FM000" && x.DontUse == false).ToList();
                            var ChargesList = tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList();

                            List<string> LRcharge = Lrcharges.Select(x => "FM" + x.Fld).ToList();
                            ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                            #endregion
                        }
                        else
                        {
                            tfatSearch.Where(x => DispatchDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                            tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }

                        string[] ADVBALDetailsColmn = new string[] { "FMPF001", "FMPF002", "FMPF003", "FMPF004", "FMPF005", "FMPF006", "FMPF007", "FMPF008", "FMPF009", "FMPF010" };
                        if (Model.AdvBalDetails)
                        {
                            tfatSearch.Where(x => ADVBALDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            #region Only This Chrges show As Per DocType Rule
                            var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.DontUse == false).ToList();
                            var ChargesList = tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList();

                            List<string> LRcharge = Lrcharges.Select(x => "FMP" + x.Fld).ToList();
                            ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                            #endregion
                        }
                        else
                        {
                            tfatSearch.Where(x => ADVBALDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }

                        string[] DeliveryDetailsColmn = new string[] { "DeliveryNo", "DelBranch", "DelDate", "DelQty" };
                        if (Model.DeliveryDetails)
                        {
                            tfatSearch.Where(x => DeliveryDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        }
                        else
                        {
                            tfatSearch.Where(x => DeliveryDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }
                        string[] ExpensesDetailsColmn = new string[] { "Recordkey", "ExpAccount", "ExpAmount", "ExpDate", "ExpBranch", "IncAmount" };
                        if (Model.ExpensesDetails)
                        {
                            tfatSearch.Where(x => ExpensesDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        }
                        else
                        {
                            tfatSearch.Where(x => ExpensesDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }
                        string[] TripDetailsColmn = new string[] { "TripNo", "TripBranch", "TripDate", "Driver", "TripCharge", "LocalCharge", "ViaCharge", "TripTotal" };
                        if (Model.TripDetails)
                        {
                            tfatSearch.Where(x => TripDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        }
                        else
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
                    //else
                    //{
                    //    string[] PLDetailsColmn = new string[] { "P/L AC" };
                    //    if (Model.PLAccount)
                    //    {
                    //        tfatSearch.Where(x => PLDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                    //    }
                    //    else
                    //    {
                    //        tfatSearch.Where(x => PLDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                    //    }
                    //    string[] ADVBALDetailsColmn = new string[] { "FMPF001", "FMPF002", "FMPF003", "FMPF004", "FMPF005", "FMPF006", "FMPF007", "FMPF008", "FMPF009", "FMPF010" };
                    //    if (Model.AdvBalDetails)
                    //    {
                    //        tfatSearch.Where(x => ADVBALDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                    //    }
                    //    else
                    //    {
                    //        tfatSearch.Where(x => ADVBALDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                    //    }

                    //}
                    ctxTFAT.SaveChanges();
                }
                if (Model.ViewDataId == "LorryReceiptNOLC")
                {
                    var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
                    var ReportHeader = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).FirstOrDefault();
                    if (Model.SkipDuplicateFM)
                    {
                        //                   LR,Trip^LR,ALT^LR,EXP^LR,SLR^LR,DEL^FM,FMP^FM^LR,LC^LR                                                              
                        ReportHeader.pMerge = "5,98^5,94^5,88^5,84^5,77^48,64^48^5,41^+107^5";
                        ReportHeader.pToMerge = "98,99,100,101,102,103,104,105,106^94,95,96,97^88,89,90,91,92,93^80,81,82,83,84,85,86,87^75,76,77,78,79^64,65,66,67,68,69,70,71,72,73,74^45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63^38,39,40,41,42,43,44^85,93-49,54,55,56,57,58,59,60,61,62,63,65,66,67,68,69,70,71,72,73,74,92,106^1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,107";
                    }
                    else
                    {
                        //                   LR,Trip^LR,ALT^LR,EXP^LR,SLR^LR,DEL^FM,FMP^LC,FM^LR,LC^LR                                                              
                        ReportHeader.pMerge = "5,98^5,94^5,88^5,84^5,77^48,64^41,48^5,41^+107^5";
                        ReportHeader.pToMerge = "98,99,100,101,102,103,104,105,106^94,95,96,97^88,89,90,91,92,93^80,81,82,83,84,85,86,87^75,76,77,78,79^64,65,66,67,68,69,70,71,72,73,74^45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63^38,39,40,41,42,43,44^85,93-49,54,55,56,57,58,59,60,61,62,63,65,66,67,68,69,70,71,72,73,74,92,106^1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,107";
                    }

                    if (Model.GetReportParameter == false)
                    {
                        tfatSearch.ForEach(x => x.IsHidden = false);

                        string[] BillDetailsColmn = new string[] { "BillType", "SaleBillBranch", "BillDate", "BillAmt", "SaleBillParty", "BillQty", "BillNumber" };
                        if (Model.BillRelationDetails)
                        {
                            tfatSearch.Where(x => BillDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        }
                        else
                        {
                            tfatSearch.Where(x => BillDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }

                        string[] DispatchDetailsColmn = new string[] { "LCBranch", "LCDate", "LCno", "LCFrom", "LCTo", "LCQty", "FMBranch", "FMDate", "FMNo", "Fm Freight", "Broker", "FM Driver", "Vehicle NO", "Vehicle Type" };
                        string[] DispatchDetailsColmn1 = new string[] { "FMF001", "FMF002", "FMF003", "FMF004", "FMF005", "FMF006", "FMF007", "FMF008", "FMF009", "FMF010" };

                        if (Model.DispatchDetails)
                        {
                            tfatSearch.Where(x => DispatchDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            #region Only This Chrges show As Per DocType Rule
                            var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "FM000" && x.DontUse == false).ToList();
                            var ChargesList = tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList();

                            List<string> LRcharge = Lrcharges.Select(x => "FM" + x.Fld).ToList();
                            ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                            #endregion
                        }
                        else
                        {
                            tfatSearch.Where(x => DispatchDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                            tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }

                        string[] ADVBALDetailsColmn = new string[] { "FMPF001", "FMPF002", "FMPF003", "FMPF004", "FMPF005", "FMPF006", "FMPF007", "FMPF008", "FMPF009", "FMPF010" };
                        if (Model.AdvBalDetails)
                        {
                            tfatSearch.Where(x => ADVBALDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            #region Only This Chrges show As Per DocType Rule
                            var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.DontUse == false).ToList();
                            var ChargesList = tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList();

                            List<string> LRcharge = Lrcharges.Select(x => "FMP" + x.Fld).ToList();
                            ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                            #endregion
                        }
                        else
                        {
                            tfatSearch.Where(x => ADVBALDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }

                        string[] DeliveryDetailsColmn = new string[] { "DeliveryNo", "DelBranch", "DelDate", "DelQty" };
                        if (Model.DeliveryDetails)
                        {
                            tfatSearch.Where(x => DeliveryDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        }
                        else
                        {
                            tfatSearch.Where(x => DeliveryDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }
                        string[] ExpensesDetailsColmn = new string[] { "Recordkey", "ExpAccount", "ExpAmount", "ExpDate", "ExpBranch", "IncAmount" };
                        if (Model.ExpensesDetails)
                        {
                            tfatSearch.Where(x => ExpensesDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        }
                        else
                        {
                            tfatSearch.Where(x => ExpensesDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }
                        string[] TripDetailsColmn = new string[] { "TripNo", "TripBranch", "TripDate", "Driver", "TripCharge", "LocalCharge", "ViaCharge", "TripTotal" };
                        if (Model.TripDetails)
                        {
                            tfatSearch.Where(x => TripDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        }
                        else
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
                    //else
                    //{
                    //    string[] PLDetailsColmn = new string[] { "P/L AC" };
                    //    if (Model.PLAccount)
                    //    {
                    //        tfatSearch.Where(x => PLDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                    //    }
                    //    else
                    //    {
                    //        tfatSearch.Where(x => PLDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                    //    }
                    //    string[] ADVBALDetailsColmn = new string[] { "FMPF001", "FMPF002", "FMPF003", "FMPF004", "FMPF005", "FMPF006", "FMPF007", "FMPF008", "FMPF009", "FMPF010" };
                    //    if (Model.AdvBalDetails)
                    //    {
                    //        tfatSearch.Where(x => ADVBALDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                    //    }
                    //    else
                    //    {
                    //        tfatSearch.Where(x => ADVBALDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                    //    }

                    //}


                    ctxTFAT.SaveChanges();
                }
                if (Model.ViewDataId == "UNBillLorryReceipt")
                {
                    var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
                    var ReportHeader = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).FirstOrDefault();
                    if (Model.SkipDuplicateFM)
                    {
                        //                   LR,Trip^LR,ALT^LR,EXP^LR,SLR^LR,DEL^FM,FMP^FM^LR,LC^LR                                                              
                        ReportHeader.pMerge = "5,98^5,94^5,88^5,84^5,77^48,64^48^5,41^+107^5";
                        ReportHeader.pToMerge = "98,99,100,101,102,103,104,105,106^94,95,96,97^88,89,90,91,92,93^80,81,82,83,84,85,86,87^75,76,77,78,79^64,65,66,67,68,69,70,71,72,73,74^45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63^38,39,40,41,42,43,44^85,93-49,54,55,56,57,58,59,60,61,62,63,65,66,67,68,69,70,71,72,73,74,92,106^1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,107";
                    }
                    else
                    {
                        //                   LR,Trip^LR,ALT^LR,EXP^LR,SLR^LR,DEL^FM,FMP^LC,FM^LR,LC^LR                                                              
                        ReportHeader.pMerge = "5,98^5,94^5,88^5,84^5,77^48,64^41,48^5,41^+107^5";
                        ReportHeader.pToMerge = "98,99,100,101,102,103,104,105,106^94,95,96,97^88,89,90,91,92,93^80,81,82,83,84,85,86,87^75,76,77,78,79^64,65,66,67,68,69,70,71,72,73,74^45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63^38,39,40,41,42,43,44^85,93-49,54,55,56,57,58,59,60,61,62,63,65,66,67,68,69,70,71,72,73,74,92,106^1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,107";
                    }

                    if (Model.GetReportParameter == false)
                    {
                        tfatSearch.ForEach(x => x.IsHidden = false);

                        string[] BillDetailsColmn = new string[] { "BillType", "SaleBillBranch", "BillDate", "BillAmt", "SaleBillParty", "BillQty", "BillNumber" };
                        if (Model.BillRelationDetails)
                        {
                            tfatSearch.Where(x => BillDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        }
                        else
                        {
                            tfatSearch.Where(x => BillDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }

                        string[] DispatchDetailsColmn = new string[] { "LCBranch", "LCDate", "LCno", "LCFrom", "LCTo", "LCQty", "FMBranch", "FMDate", "FMNo", "Fm Freight", "Broker", "FM Driver", "Vehicle NO", "Vehicle Type" };
                        string[] DispatchDetailsColmn1 = new string[] { "FMF001", "FMF002", "FMF003", "FMF004", "FMF005", "FMF006", "FMF007", "FMF008", "FMF009", "FMF010" };

                        if (Model.DispatchDetails)
                        {
                            tfatSearch.Where(x => DispatchDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            #region Only This Chrges show As Per DocType Rule
                            var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "FM000" && x.DontUse == false).ToList();
                            var ChargesList = tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList();

                            List<string> LRcharge = Lrcharges.Select(x => "FM" + x.Fld).ToList();
                            ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                            #endregion
                        }
                        else
                        {
                            tfatSearch.Where(x => DispatchDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                            tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }

                        string[] ADVBALDetailsColmn = new string[] { "FMPF001", "FMPF002", "FMPF003", "FMPF004", "FMPF005", "FMPF006", "FMPF007", "FMPF008", "FMPF009", "FMPF010" };
                        if (Model.AdvBalDetails)
                        {
                            tfatSearch.Where(x => ADVBALDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            #region Only This Chrges show As Per DocType Rule
                            var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.DontUse == false).ToList();
                            var ChargesList = tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList();

                            List<string> LRcharge = Lrcharges.Select(x => "FMP" + x.Fld).ToList();
                            ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                            #endregion
                        }
                        else
                        {
                            tfatSearch.Where(x => ADVBALDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }

                        string[] DeliveryDetailsColmn = new string[] { "DeliveryNo", "DelBranch", "DelDate", "DelQty" };
                        if (Model.DeliveryDetails)
                        {
                            tfatSearch.Where(x => DeliveryDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        }
                        else
                        {
                            tfatSearch.Where(x => DeliveryDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }
                        string[] ExpensesDetailsColmn = new string[] { "Recordkey", "ExpAccount", "ExpAmount", "ExpDate", "ExpBranch", "IncAmount" };
                        if (Model.ExpensesDetails)
                        {
                            tfatSearch.Where(x => ExpensesDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        }
                        else
                        {
                            tfatSearch.Where(x => ExpensesDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }
                        string[] TripDetailsColmn = new string[] { "TripNo", "TripBranch", "TripDate", "Driver", "TripCharge", "LocalCharge", "ViaCharge", "TripTotal" };
                        if (Model.TripDetails)
                        {
                            tfatSearch.Where(x => TripDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        }
                        else
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
                    //else
                    //{
                    //    string[] PLDetailsColmn = new string[] { "P/L AC" };
                    //    if (Model.PLAccount)
                    //    {
                    //        tfatSearch.Where(x => PLDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                    //    }
                    //    else
                    //    {
                    //        tfatSearch.Where(x => PLDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                    //    }
                    //    string[] ADVBALDetailsColmn = new string[] { "FMPF001", "FMPF002", "FMPF003", "FMPF004", "FMPF005", "FMPF006", "FMPF007", "FMPF008", "FMPF009", "FMPF010" };
                    //    if (Model.AdvBalDetails)
                    //    {
                    //        tfatSearch.Where(x => ADVBALDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                    //    }
                    //    else
                    //    {
                    //        tfatSearch.Where(x => ADVBALDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                    //    }

                    //}
                    ctxTFAT.SaveChanges();
                }
                if (Model.ViewDataId == "UNDispatchLorryReceipt")
                {
                    var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
                    var ReportHeader = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).FirstOrDefault();
                    if (Model.SkipDuplicateFM)
                    {
                        //                   LR,Trip^LR,ALT^LR,EXP^LR,SLR^LR,DEL^FM,FMP^FM^LR,LC^LR                                                              
                        ReportHeader.pMerge = "5,97^5,93^5,87^5,83^5,76^47,63^47^5,40^+106^5";
                        ReportHeader.pToMerge = "97,98,99,100,101,102,103,104,105^93,94,95,96^87,88,89,90,91,92^79,80,81,82,83,84,85,86^74,75,76,77,78^63,64,65,66,67,68,69,70,71,72,73^44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62^37,38,39,40,41,42,43^84,92-48,53,54,55,56,57,58,59,60,61,62,64,65,66,67,68,69,70,71,72,73,91,105^1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,106";
                    }
                    else
                    {
                        //                   LR,Trip^LR,ALT^LR,EXP^LR,SLR^LR,DEL^FM,FMP^LC,FM^LR,LC^LR                                                              
                        ReportHeader.pMerge = "5,97^5,93^5,87^5,83^5,76^47,63^40,47^5,40^+106^5";
                        ReportHeader.pToMerge = "97,98,99,100,101,102,103,104,105^93,94,95,96^87,88,89,90,91,92^79,80,81,82,83,84,85,86^74,75,76,77,78^63,64,65,66,67,68,69,70,71,72,73^44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62^37,38,39,40,41,42,43^84,92-48,53,54,55,56,57,58,59,60,61,62,64,65,66,67,68,69,70,71,72,73,91,105^1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,106";
                    }
                    if (Model.GetReportParameter == false)
                    {
                        tfatSearch.ForEach(x => x.IsHidden = false);

                        string[] BillDetailsColmn = new string[] { "BillType", "SaleBillBranch", "BillDate", "BillAmt", "SaleBillParty", "BillQty", "BillNumber" };
                        if (Model.BillRelationDetails)
                        {
                            tfatSearch.Where(x => BillDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        }
                        else
                        {
                            tfatSearch.Where(x => BillDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }

                        string[] DispatchDetailsColmn = new string[] { "LCBranch", "LCDate", "LCno", "LCFrom", "LCTo", "LCQty", "FMBranch", "FMDate", "FMNo", "Fm Freight", "Broker", "FM Driver", "Vehicle NO", "Vehicle Type" };
                        string[] DispatchDetailsColmn1 = new string[] { "FMF001", "FMF002", "FMF003", "FMF004", "FMF005", "FMF006", "FMF007", "FMF008", "FMF009", "FMF010" };

                        if (Model.DispatchDetails)
                        {
                            tfatSearch.Where(x => DispatchDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            #region Only This Chrges show As Per DocType Rule
                            var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "FM000" && x.DontUse == false).ToList();
                            var ChargesList = tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList();

                            List<string> LRcharge = Lrcharges.Select(x => "FM" + x.Fld).ToList();
                            ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                            #endregion
                        }
                        else
                        {
                            tfatSearch.Where(x => DispatchDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                            tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }

                        string[] ADVBALDetailsColmn = new string[] { "FMPF001", "FMPF002", "FMPF003", "FMPF004", "FMPF005", "FMPF006", "FMPF007", "FMPF008", "FMPF009", "FMPF010" };
                        if (Model.AdvBalDetails)
                        {
                            tfatSearch.Where(x => ADVBALDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            #region Only This Chrges show As Per DocType Rule
                            var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.DontUse == false).ToList();
                            var ChargesList = tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList();

                            List<string> LRcharge = Lrcharges.Select(x => "FMP" + x.Fld).ToList();
                            ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                            #endregion
                        }
                        else
                        {
                            tfatSearch.Where(x => ADVBALDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }

                        string[] DeliveryDetailsColmn = new string[] { "DeliveryNo", "DelBranch", "DelDate", "DelQty" };
                        if (Model.DeliveryDetails)
                        {
                            tfatSearch.Where(x => DeliveryDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        }
                        else
                        {
                            tfatSearch.Where(x => DeliveryDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }
                        string[] ExpensesDetailsColmn = new string[] { "Recordkey", "ExpAccount", "ExpAmount", "ExpDate", "ExpBranch", "IncAmount" };
                        if (Model.ExpensesDetails)
                        {
                            tfatSearch.Where(x => ExpensesDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        }
                        else
                        {
                            tfatSearch.Where(x => ExpensesDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }
                        string[] TripDetailsColmn = new string[] { "TripNo", "TripBranch", "TripDate", "Driver", "TripCharge", "LocalCharge", "ViaCharge", "TripTotal" };
                        if (Model.TripDetails)
                        {
                            tfatSearch.Where(x => TripDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        }
                        else
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
                    //else
                    //{
                    //    string[] PLDetailsColmn = new string[] { "P/L AC" };
                    //    if (Model.PLAccount)
                    //    {
                    //        tfatSearch.Where(x => PLDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                    //    }
                    //    else
                    //    {
                    //        tfatSearch.Where(x => PLDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                    //    }
                    //    string[] ADVBALDetailsColmn = new string[] { "FMPF001", "FMPF002", "FMPF003", "FMPF004", "FMPF005", "FMPF006", "FMPF007", "FMPF008", "FMPF009", "FMPF010" };
                    //    if (Model.AdvBalDetails)
                    //    {
                    //        tfatSearch.Where(x => ADVBALDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                    //    }
                    //    else
                    //    {
                    //        tfatSearch.Where(x => ADVBALDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                    //    }

                    //}
                    ctxTFAT.SaveChanges();
                }
                if (Model.ViewDataId == "LorryReceiptStock")
                {
                    var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
                    var ReportHeader = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).FirstOrDefault();
                    if (Model.SkipDuplicateFM)
                    {
                        //                   LR,Trip^LR,ALT^LR,EXP^LR,SLR^LR,DEL^FM,FMP^FM^LR,LC^LR,STK^LR                                                              
                        ReportHeader.pMerge = "5,102^5,98^5,92^5,88^5,81^52,68^52^5,45^5,37^+111^5";
                        ReportHeader.pToMerge = "102,103,104,105,106,107,108,109,110^98,99,100,101^92,93,94,95,96,97^84,85,86,87,88,89,90,91^79,80,81,82,83^68,69,70,71,72,73,74,75,76,77,78^49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67^42,43,44,45,46,47,48^37,38,39,40,41^89,97-53,58,59,60,61,62,63,64,65,66,67,69,70,71,72,73,74,75,76,77,78,96,110^1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,111";
                    }
                    else
                    {
                        //                   LR,Trip^LR,ALT^LR,EXP^LR,SLR^LR,DEL^FM,FMP^LC,FM^LR,LC^LR,STK^LR                                                              
                        ReportHeader.pMerge = "5,102^5,98^5,92^5,88^5,81^52,68^45,52^5,45^5,37^+111^5";
                        ReportHeader.pToMerge = "102,103,104,105,106,107,108,109,110^98,99,100,101^92,93,94,95,96,97^84,85,86,87,88,89,90,91^79,80,81,82,83^68,69,70,71,72,73,74,75,76,77,78^49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67^42,43,44,45,46,47,48^37,38,39,40,41^89,97-53,58,59,60,61,62,63,64,65,66,67,69,70,71,72,73,74,75,76,77,78,96,110^1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,111";
                    }
                    if (Model.GetReportParameter == false)
                    {
                        tfatSearch.ForEach(x => x.IsHidden = false);

                        string[] BillDetailsColmn = new string[] { "BillType", "SaleBillBranch", "BillDate", "BillAmt", "SaleBillParty", "BillQty", "BillNumber" };
                        if (Model.BillRelationDetails)
                        {
                            tfatSearch.Where(x => BillDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        }
                        else
                        {
                            tfatSearch.Where(x => BillDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }

                        string[] DispatchDetailsColmn = new string[] { "LCBranch", "LCDate", "LCno", "LCFrom", "LCTo", "LCQty", "FMBranch", "FMDate", "FMNo", "Fm Freight", "Broker", "FM Driver", "Vehicle NO", "Vehicle Type" };
                        string[] DispatchDetailsColmn1 = new string[] { "FMF001", "FMF002", "FMF003", "FMF004", "FMF005", "FMF006", "FMF007", "FMF008", "FMF009", "FMF010" };

                        if (Model.DispatchDetails)
                        {
                            tfatSearch.Where(x => DispatchDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            #region Only This Chrges show As Per DocType Rule
                            var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "FM000" && x.DontUse == false).ToList();
                            var ChargesList = tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList();

                            List<string> LRcharge = Lrcharges.Select(x => "FM" + x.Fld).ToList();
                            ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                            #endregion
                        }
                        else
                        {
                            tfatSearch.Where(x => DispatchDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                            tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }

                        string[] ADVBALDetailsColmn = new string[] { "FMPF001", "FMPF002", "FMPF003", "FMPF004", "FMPF005", "FMPF006", "FMPF007", "FMPF008", "FMPF009", "FMPF010" };
                        if (Model.AdvBalDetails)
                        {
                            tfatSearch.Where(x => ADVBALDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            #region Only This Chrges show As Per DocType Rule
                            var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.DontUse == false).ToList();
                            var ChargesList = tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList();

                            List<string> LRcharge = Lrcharges.Select(x => "FMP" + x.Fld).ToList();
                            ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                            ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                            #endregion
                        }
                        else
                        {
                            tfatSearch.Where(x => ADVBALDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }

                        string[] DeliveryDetailsColmn = new string[] { "DeliveryNo", "DelBranch", "DelDate", "DelQty" };
                        if (Model.DeliveryDetails)
                        {
                            tfatSearch.Where(x => DeliveryDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        }
                        else
                        {
                            tfatSearch.Where(x => DeliveryDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }
                        string[] ExpensesDetailsColmn = new string[] { "Recordkey", "ExpAccount", "ExpAmount", "ExpDate", "ExpBranch", "IncAmount" };
                        if (Model.ExpensesDetails)
                        {
                            tfatSearch.Where(x => ExpensesDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        }
                        else
                        {
                            tfatSearch.Where(x => ExpensesDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                        }
                        string[] TripDetailsColmn = new string[] { "TripNo", "TripBranch", "TripDate", "Driver", "TripCharge", "LocalCharge", "ViaCharge", "TripTotal" };
                        if (Model.TripDetails)
                        {
                            tfatSearch.Where(x => TripDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                        }
                        else
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
                    //else
                    //{
                    //    string[] PLDetailsColmn = new string[] { "P/L AC" };
                    //    if (Model.PLAccount)
                    //    {
                    //        tfatSearch.Where(x => PLDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                    //    }
                    //    else
                    //    {
                    //        tfatSearch.Where(x => PLDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                    //    }
                    //    string[] ADVBALDetailsColmn = new string[] { "FMPF001", "FMPF002", "FMPF003", "FMPF004", "FMPF005", "FMPF006", "FMPF007", "FMPF008", "FMPF009", "FMPF010" };
                    //    if (Model.AdvBalDetails)
                    //    {
                    //        tfatSearch.Where(x => ADVBALDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                    //    }
                    //    else
                    //    {
                    //        tfatSearch.Where(x => ADVBALDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                    //    }

                    //}
                    ctxTFAT.SaveChanges();
                }

                return GetGridDataColumns(Model.ViewDataId, "X", "", GetMonthString());
            }
            catch (Exception ex)
            {
                return Json(new { Status = "Error", Message = "Error! While Generating Report..\n" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetGridData(LorryReceiptReportsVM Model)
        {
            if (String.IsNullOrEmpty(Model.mWhat))
            {
                if (Model.ChargeShow == false && Model.BillChargeShow == false)
                {
                    Model.ChargeShow = true;
                }

                if (String.IsNullOrEmpty(Model.FromBranch))
                {
                    Model.FromBranch = "'" + mbranchcode + "'";
                }

                //ppara05 = ppara05 == null || ppara05 == "" ? "'" + mbranchcode + "'" : ppara05;ExecuteStoredProc("Drop Table ztmp_TempMth");
                ExecuteStoredProc("Drop Table ztmp_ConsignmentReport");
                ExecuteStoredProc("Drop Table TempLRList");
                SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                SqlCommand cmd = new SqlCommand("ConsigmentReports", tfat_conx);

                if (Model.ViewDataId == "LorryReceiptReports")
                {
                    cmd.CommandText = "LorryReceiptReportsMergingDyamically";
                    cmd.CommandText = "LorryReceiptReports";
                }
                else
                {
                    cmd.CommandText = Model.ViewDataId;
                }

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;
                cmd.Parameters.Add("@LRno", SqlDbType.VarChar).Value = Model.ConsignmentNo;
                cmd.Parameters.Add("@FromDate", SqlDbType.VarChar).Value = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                cmd.Parameters.Add("@ToDate", SqlDbType.VarChar).Value = (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                cmd.Parameters.Add("@ReportType", SqlDbType.VarChar).Value = Model.ReportTypeL;
                cmd.Parameters.Add("@FromBranch", SqlDbType.VarChar).Value = Model.FromBranch == null ? "" : Model.FromBranch;
                cmd.Parameters.Add("@ToBranch", SqlDbType.VarChar).Value = Model.ToBranch;
                cmd.Parameters.Add("@LRType", SqlDbType.VarChar).Value = Model.LrType;
                cmd.Parameters.Add("@FromDest", SqlDbType.VarChar).Value = Model.FromDestination;
                cmd.Parameters.Add("@ToDest", SqlDbType.VarChar).Value = Model.ToDestination;
                cmd.Parameters.Add("@BillBranch", SqlDbType.VarChar).Value = Model.BillingBranch;
                cmd.Parameters.Add("@Consigner", SqlDbType.VarChar).Value = Model.Consignor;
                cmd.Parameters.Add("@Consignee", SqlDbType.VarChar).Value = Model.Consignee;
                cmd.Parameters.Add("@BillParty", SqlDbType.VarChar).Value = Model.BillingParty;
                cmd.Parameters.Add("@Delivery", SqlDbType.VarChar).Value = Model.Delivery;
                cmd.Parameters.Add("@Collection", SqlDbType.VarChar).Value = Model.Collection;
                cmd.Parameters.Add("@ChargeType", SqlDbType.VarChar).Value = Model.ChargeType;
                cmd.Parameters.Add("@Particular", SqlDbType.VarChar).Value = Model.Particular;
                cmd.Parameters.Add("@Unit", SqlDbType.VarChar).Value = Model.Unit;
                cmd.Parameters.Add("@LRGenerate", SqlDbType.VarChar).Value = Model.LrGenetate ?? "";

                if (Model.ViewDataId == "LorryReceiptReports")
                {
                    cmd.Parameters.Add("@LrMode", SqlDbType.VarChar).Value = Model.LrMode ?? "";

                    cmd.Parameters.Add("@PickCharges", SqlDbType.Bit).Value = Model.ChargeShow;
                    cmd.Parameters.Add("@PickBillCharges", SqlDbType.Bit).Value = Model.BillChargeShow;
                    cmd.Parameters.Add("@BillDetails", SqlDbType.Bit).Value = Model.BillRelationDetails;
                    cmd.Parameters.Add("@DispatchDetails", SqlDbType.Bit).Value = Model.DispatchDetails;
                    cmd.Parameters.Add("@AdvBalDetails", SqlDbType.Bit).Value = Model.AdvBalDetails;
                    cmd.Parameters.Add("@DeliveryDetails", SqlDbType.Bit).Value = Model.DeliveryDetails;
                    cmd.Parameters.Add("@ExpensesDetails", SqlDbType.Bit).Value = Model.ExpensesDetails;
                    cmd.Parameters.Add("@TripDetails", SqlDbType.Bit).Value = Model.TripDetails;
                    cmd.Parameters.Add("@DeliverdConsignmentStatus", SqlDbType.VarChar).Value = Model.DeliverdConsignmentStatus;
                }
                if (Model.ViewDataId == "LorryReceiptNOLC")
                {

                    cmd.Parameters.Add("@BillDetails", SqlDbType.Bit).Value = Model.BillRelationDetails;
                    cmd.Parameters.Add("@DispatchDetails", SqlDbType.Bit).Value = Model.DispatchDetails;//add 
                    cmd.Parameters.Add("@AdvBalDetails", SqlDbType.Bit).Value = Model.AdvBalDetails;
                    cmd.Parameters.Add("@DeliveryDetails", SqlDbType.Bit).Value = Model.DeliveryDetails;
                    cmd.Parameters.Add("@ExpensesDetails", SqlDbType.Bit).Value = Model.ExpensesDetails;
                    cmd.Parameters.Add("@TripDetails", SqlDbType.Bit).Value = Model.TripDetails;
                    cmd.Parameters.Add("@DeliverdConsignmentStatus", SqlDbType.VarChar).Value = Model.DeliverdConsignmentStatus;
                }
                if (Model.ViewDataId == "UNBillLorryReceipt")
                {
                    cmd.Parameters.Add("@BillDetails", SqlDbType.Bit).Value = Model.BillRelationDetails;//add
                    cmd.Parameters.Add("@DispatchDetails", SqlDbType.Bit).Value = Model.DispatchDetails;
                    cmd.Parameters.Add("@AdvBalDetails", SqlDbType.Bit).Value = Model.AdvBalDetails;
                    cmd.Parameters.Add("@DeliveryDetails", SqlDbType.Bit).Value = Model.DeliveryDetails;
                    cmd.Parameters.Add("@ExpensesDetails", SqlDbType.Bit).Value = Model.ExpensesDetails;
                    cmd.Parameters.Add("@TripDetails", SqlDbType.Bit).Value = Model.TripDetails;
                    cmd.Parameters.Add("@DeliverdConsignmentStatus", SqlDbType.VarChar).Value = Model.DeliverdConsignmentStatus;
                }
                if (Model.ViewDataId == "UNDispatchLorryReceipt")
                {
                    cmd.Parameters.Add("@BillDetails", SqlDbType.Bit).Value = Model.BillRelationDetails;
                    cmd.Parameters.Add("@DispatchDetails", SqlDbType.Bit).Value = Model.DispatchDetails;
                    cmd.Parameters.Add("@AdvBalDetails", SqlDbType.Bit).Value = Model.AdvBalDetails;
                    cmd.Parameters.Add("@DeliveryDetails", SqlDbType.Bit).Value = Model.DeliveryDetails;
                    cmd.Parameters.Add("@ExpensesDetails", SqlDbType.Bit).Value = Model.ExpensesDetails;
                    cmd.Parameters.Add("@TripDetails", SqlDbType.Bit).Value = Model.TripDetails;
                    cmd.Parameters.Add("@DeliverdConsignmentStatus", SqlDbType.VarChar).Value = Model.DeliverdConsignmentStatus;
                }
                if (Model.ViewDataId == "LorryReceiptStock")
                {
                    cmd.Parameters.Add("@StockBranch", SqlDbType.VarChar).Value = Model.StockBranch;
                    cmd.Parameters.Add("@StockType", SqlDbType.VarChar).Value = Model.StockType;
                    cmd.Parameters.Add("@BillDetails", SqlDbType.Bit).Value = Model.BillRelationDetails;
                    cmd.Parameters.Add("@DispatchDetails", SqlDbType.Bit).Value = Model.DispatchDetails;
                    cmd.Parameters.Add("@AdvBalDetails", SqlDbType.Bit).Value = Model.AdvBalDetails;
                    cmd.Parameters.Add("@DeliveryDetails", SqlDbType.Bit).Value = Model.DeliveryDetails;
                    cmd.Parameters.Add("@ExpensesDetails", SqlDbType.Bit).Value = Model.ExpensesDetails;
                    cmd.Parameters.Add("@TripDetails", SqlDbType.Bit).Value = Model.TripDetails;
                    cmd.Parameters.Add("@DeliverdConsignmentStatus", SqlDbType.VarChar).Value = Model.DeliverdConsignmentStatus;
                }

                cmd.Parameters.Add("@mSelectQuery", SqlDbType.VarChar, 5000).Value = "";
                cmd.Parameters.Add("@mUserQuery", SqlDbType.VarChar, 5000).Value = "";
                cmd.Parameters.Add("@mTableQuery", SqlDbType.VarChar, 5000).Value = "";
                cmd.Parameters["@mSelectQuery"].Direction = ParameterDirection.Output;
                cmd.Parameters["@mUserQuery"].Direction = ParameterDirection.Output;
                cmd.Parameters["@mTableQuery"].Direction = ParameterDirection.Output;
                ExecuteStoredProc("Drop Table ztmp_ConsignmentReport");
                tfat_conx.Open();
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();

                string mSelectQuery = (string)(cmd.Parameters["@mSelectQuery"].Value ?? "");
                string mUserQuery = (string)(cmd.Parameters["@mUserQuery"].Value ?? "");
                string mTableQuery = (string)(cmd.Parameters["@mTableQuery"].Value ?? "");

                tfat_conx.Close();

                if (Model.ViewDataId == "LorryReceiptStock")
                {
                    ExecuteStoredProc("delete from ztmp_ConsignmentReport where StockBranchOnly not in (" + Model.StockBranch + ")");
                }
            }

            GridOption gridOption = new GridOption();
            gridOption.ViewDataId = Model.ViewDataId;
            gridOption.FromDate = Model.FromDate;
            gridOption.ToDate = Model.ToDate;
            gridOption.mWhat = Model.mWhat;

            int x = (int)7;
            gridOption.rows = 5000;
            gridOption.page = Model.page == 0 ? 1 : Model.page;
            gridOption.searchField = Model.searchField;
            gridOption.searchOper = Model.searchOper;
            gridOption.searchString = Model.searchString;
            gridOption.sidx = Model.sidx;
            gridOption.sord = Model.sord;
            return GetGridReport(gridOption, "X", "", true, 0);
        }

        #region Set Column Show Hide

        public void BillDetailsColumns(bool Flag, string ViewDataId)
        {
            string[] BillDetailsColmn = new string[] { "BillType", "SaleBillBranch", "BillDate", "BillAmt", "SaleBillParty", "BillQty", "BillNumber" };

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            if (Flag)
            {
                tfatSearch.Where(x => BillDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            }
            else
            {
                tfatSearch.Where(x => BillDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }
            ctxTFAT.SaveChanges();
        }

        public void DispatchDetailsColumns(bool Flag, string ViewDataId)
        {
            string[] DispatchDetailsColmn = new string[] { "LCBranch", "LCDate", "LCno", "LCFrom", "LCTo", "LCQty", "FMBranch", "FMDate", "FMNo", "Fm Freight", "Broker", "FM Driver", "Vehicle NO", "Vehicle Type" };
            string[] DispatchDetailsColmn1 = new string[] { "FMF001", "FMF002", "FMF003", "FMF004", "FMF005", "FMF006", "FMF007", "FMF008", "FMF009", "FMF010" };

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            if (Flag)
            {
                tfatSearch.Where(x => DispatchDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                #region Only This Chrges show As Per DocType Rule
                var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "FM000" && x.DontUse == false).ToList();
                var ChargesList = tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList();

                List<string> LRcharge = Lrcharges.Select(x => "FM" + x.Fld).ToList();
                ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                #endregion
            }
            else
            {
                tfatSearch.Where(x => DispatchDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }
            ctxTFAT.SaveChanges();
        }

        public void ExpensesDetailsColumns(bool Flag, string ViewDataId)
        {
            string[] ExpensesDetailsColmn = new string[] { "Recordkey", "ExpAccount", "ExpAmount", "ExpDate", "ExpBranch", "IncAmount" };

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            if (Flag)
            {
                tfatSearch.Where(x => ExpensesDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            }
            else
            {
                tfatSearch.Where(x => ExpensesDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }
            ctxTFAT.SaveChanges();
        }

        public void DeliveryDetailsColumns(bool Flag, string ViewDataId)
        {
            string[] DeliveryDetailsColmn = new string[] { "DeliveryNo", "DelBranch", "DelDate", "DelQty" };

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            if (Flag)
            {
                tfatSearch.Where(x => DeliveryDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            }
            else
            {
                tfatSearch.Where(x => DeliveryDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }
            ctxTFAT.SaveChanges();
        }

        public void TripDetailsColumns(bool Flag, string ViewDataId)
        {
            string[] TripDetailsColmn = new string[] { "TripNo", "TripBranch", "TripDate", "Driver", "TripCharge", "LocalCharge", "ViaCharge", "TripTotal" };

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

        public void ChargesDetailsColumns(bool Flag, string ViewDataId)
        {
            //string[] ChrColmn = new string[] {"F001", "Freight Charges", "LR Charges", "Hamali Charges", "Collection Charges", "Door Delivery Charges", "Varai Charges", "Loading Charges", "Unloading Charges", "Detention Charges", "Union Charges", "Val11", "Val12", "Val13", "Val14", "Val15", "Val16", "Val17", "Val18", "Val19", "Val20", "Val21", "Val22", "Val23", "Val24", "Val25" };
            string[] ChrColmn = new string[] { "F001", "F002", "F003", "F004", "F005", "F006", "F007", "F008", "F009", "F010", "F011", "F012", "F013", "F014", "F015", "F016", "F017", "F018", "F019", "F020", "F021", "F022", "F023", "F024", "F025" };

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            if (Flag)
            {
                var ChargesList = tfatSearch.Where(x => ChrColmn.Contains(x.ColHead)).ToList();
                tfatSearch.Where(x => ChrColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);

                #region Only This Chrges show As Per DocType Rule
                var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "LR000" && x.DontUse == false).ToList();
                List<string> LRcharge = Lrcharges.Select(x => x.Fld).ToList();
                ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                #endregion

            }
            else
            {
                tfatSearch.Where(x => ChrColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
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

        public void ADVBALDetailsColumns(bool Flag, string ViewDataId)
        {
            string[] DispatchDetailsColmn1 = new string[] { "FMPF001", "FMPF002", "FMPF003", "FMPF004", "FMPF005", "FMPF006", "FMPF007", "FMPF008", "FMPF009", "FMPF010" };

            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == ViewDataId).ToList();
            if (Flag)
            {
                tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                #region Only This Chrges show As Per DocType Rule
                var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.DontUse == false).ToList();
                var ChargesList = tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList();

                List<string> LRcharge = Lrcharges.Select(x => "FMP" + x.Fld).ToList();
                ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                #endregion
            }
            else
            {
                tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }
            ctxTFAT.SaveChanges();
        }

        public void SetColumnsReset(LorryReceiptReportsVM Model)
        {
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = false);
            string[] HideColmn = new string[] { "P/L AC", "STKTablekey", "LRTablekey", "LCTablekey", "FMTablekey", "FMPTablekey", "DELTablekey", "BillTablekey", "EXPTablekey", "ALRTTablekey", "TripTablekey" };
            tfatSearch.Where(x => HideColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);

            //string[] ChrColmn = new string[] { "F001", "Freight Charges", "LR Charges", "Hamali Charges", "Collection Charges", "Door Delivery Charges", "Varai Charges", "Loading Charges", "Unloading Charges", "Detention Charges", "Union Charges", "Val11", "Val12", "Val13", "Val14", "Val15", "Val16", "Val17", "Val18", "Val19", "Val20", "Val21", "Val22", "Val23", "Val24", "Val25" };
            string[] ChrColmn = new string[] { "F001", "F002", "F003", "F004", "F005", "F006", "F007", "F008", "F009", "F010", "F011", "F012", "F013", "F014", "F015", "F016", "F017", "F018", "F019", "F020", "F021", "F022", "F023", "F024", "F025" };
            if (Model.ChargeShow || Model.BillChargeShow)
            {
                var ChargesList = tfatSearch.Where(x => ChrColmn.Contains(x.ColHead)).ToList();
                tfatSearch.Where(x => ChrColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                #region Only This Chrges show As Per DocType Rule
                var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "LR000" && x.DontUse == false).ToList();
                List<string> LRcharge = Lrcharges.Select(x => x.Fld).ToList();
                ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                #endregion
            }
            else
            {
                tfatSearch.Where(x => ChrColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }

            string[] BillDetailsColmn = new string[] { "BillType", "SaleBillBranch", "BillDate", "BillAmt", "SaleBillParty", "BillQty", "BillNumber" };
            if (Model.BillRelationDetails)
            {
                tfatSearch.Where(x => BillDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            }
            else
            {
                tfatSearch.Where(x => BillDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }

            string[] DispatchDetailsColmn = new string[] { "LCBranch", "LCDate", "LCno", "LCFrom", "LCTo", "LCQty", "FMBranch", "FMDate", "FMNo", "Fm Freight", "Broker", "FM Driver", "Vehicle NO", "Vehicle Type" };
            string[] DispatchDetailsColmn1 = new string[] { "FMF001", "FMF002", "FMF003", "FMF004", "FMF005", "FMF006", "FMF007", "FMF008", "FMF009", "FMF010" };
            if (Model.DispatchDetails)
            {

                tfatSearch.Where(x => DispatchDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                #region Only This Chrges show As Per DocType Rule
                var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "FM000" && x.DontUse == false).ToList();
                var ChargesList = tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList();

                List<string> LRcharge = Lrcharges.Select(x => "FM" + x.Fld).ToList();
                ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                #endregion
            }
            else
            {
                tfatSearch.Where(x => DispatchDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }

            string[] ADVBALDetailsColmn = new string[] { "FMPF001", "FMPF002", "FMPF003", "FMPF004", "FMPF005", "FMPF006", "FMPF007", "FMPF008", "FMPF009", "FMPF010" };
            if (Model.AdvBalDetails)
            {
                tfatSearch.Where(x => ADVBALDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                #region Only This Chrges show As Per DocType Rule
                var Lrcharges = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.DontUse == false).ToList();
                var ChargesList = tfatSearch.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList();

                List<string> LRcharge = Lrcharges.Select(x => "FMP" + x.Fld).ToList();
                ChargesList.Where(x => LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
                ChargesList.Where(x => !LRcharge.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                #endregion
            }
            else
            {
                tfatSearch.Where(x => ADVBALDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }

            string[] DeliveryDetailsColmn = new string[] { "DeliveryNo", "DelBranch", "DelDate", "DelQty" };
            if (Model.DeliveryDetails)
            {
                tfatSearch.Where(x => DeliveryDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            }
            else
            {
                tfatSearch.Where(x => DeliveryDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }
            string[] ExpensesDetailsColmn = new string[] { "Recordkey", "ExpAccount", "ExpAmount", "ExpDate", "ExpBranch", "IncAmount" };
            if (Model.ExpensesDetails)
            {
                tfatSearch.Where(x => ExpensesDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            }
            else
            {
                tfatSearch.Where(x => ExpensesDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }

            string[] TripDetailsColmn = new string[] { "TripNo", "TripBranch", "TripDate", "Driver", "TripCharge", "LocalCharge", "ViaCharge", "TripTotal" };
            if (Model.TripDetails)
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

        public ActionResult ParameterReset(LorryReceiptReportsVM Model)
        {
            Model.StockTypes = PopulateStockTypes();
            Model.Branches = PopulateBranches();
            Model.BranchesOnly = PopulateBranchesOnly();
            Model.StockBranchesOnly = PopulateStockBranchesOnly();

            Model.ReportsType = PopulateReportType();
            Model.LrTypes = PopulateLrType();
            Model.Consignors = PopulateConsigner();
            Model.BillingPartys = PopulateBillParty();
            Model.ChargeTypes = PopulateChargeType();
            Model.Particulars = PopulateParticulars();
            Model.Units = PopulateUnit();
            Model.Collections = PopulateCollection();
            Model.Delivereries = PopulateDelivery();
            Model.DateRanges = PopulateDateRange();
            Model.FromBranch = mbranchcode;

            string[] HideColmn = new string[] { "P/L AC", "STKTablekey", "LRTablekey", "LCTablekey", "FMTablekey", "FMPTablekey", "DELTablekey", "BillTablekey", "EXPTablekey", "ALRTTablekey", "TripTablekey" };
            string[] Colmn = new string[] { "EntryDate", "Time", "Branch", "Vehicleno", "LrNo", "Date", "From", "To", "Consignor", "Consignee", "BillParty", "LRtype", "BillBranch", "Collection", "Delivery", "Qty", "Unit", "ActWt", "ChgWt", "ChargeType", "Amt", "Description", "PartyChallan", "PartyInvoice", "PONumber", "BENumber", "DecVal", "GSTNO", "EwayBill", "LRMode", "ServiceType", "Remark", "UserName", "TAT", "Status", "Note", "Note Date", "Note CreateBy" };
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = true);
            tfatSearch.Where(x => Colmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            tfatSearch.Where(x => HideColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            ctxTFAT.SaveChanges();

            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = DateTime.Today.ToShortDateString();

            var html = ViewHelper.RenderPartialView(this, "_OptionsView", Model);
            var jsonResult = Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }
        public ActionResult GetParameterAuto(LorryReceiptReportsVM Model)
        {
            var MainReportName = Model.ViewDataId;

            Model.StockTypes = PopulateStockTypes();
            Model.Branches = PopulateBranches();
            Model.BranchesOnly = PopulateBranchesOnly();
            Model.StockBranchesOnly = PopulateStockBranchesOnly();
            Model.ReportsType = PopulateReportType();
            Model.LrTypes = PopulateLrType();
            Model.Consignors = PopulateConsigner();
            Model.BillingPartys = PopulateBillParty();
            Model.ChargeTypes = PopulateChargeType();
            Model.Particulars = PopulateParticulars();
            Model.Units = PopulateUnit();
            Model.Collections = PopulateCollection();
            Model.Delivereries = PopulateDelivery();
            Model.DateRanges = PopulateDateRange();

            Model.ReportTypeL = ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).Select(x => x.ReportName).FirstOrDefault();
            var Tfatsearch = ctxTFAT.TfatSearch.Where(x => x.Code.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower()).ToList();
            Tfatsearch.ForEach(x => x.IsHidden = false);
            //Model.SaveReportList = PopulateSaveReports(MainReportName);
            ReportParameters mobj = new ReportParameters();
            if (ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).FirstOrDefault() != null)
            {
                mobj = ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).FirstOrDefault();

                Model.ConsignmentNo = mobj.DocNo;
                Model.FromDate = mobj.StartDate.Value.ToShortDateString();
                Model.ToDate = mobj.EndDate.Value.ToShortDateString();
                Model.HideColumnList = mobj.HideColumnList;

                Model.LrMode = mobj.Para1;
                Model.FromBranch = mobj.Para2 == null ? "" : mobj.Para2.Replace("'", "");
                Model.ToBranch = mobj.Para3 == null ? "" : mobj.Para3.Replace("'", "");
                Model.LrType = mobj.Para4 == null ? "" : mobj.Para4.Replace("'", "");
                Model.FromDestination = mobj.Para5 == null ? "" : mobj.Para5.Replace("'", "");
                Model.ToDestination = mobj.Para6 == null ? "" : mobj.Para6.Replace("'", "");
                Model.BillingBranch = mobj.Para7 == null ? "" : mobj.Para7.Replace("'", "");
                Model.Consignor = mobj.Para8 == null ? "" : mobj.Para8.Replace("'", "");
                Model.Consignee = mobj.Para9 == null ? "" : mobj.Para9.Replace("'", "");
                Model.BillingParty = mobj.Para10 == null ? "" : mobj.Para10.Replace("'", "");
                Model.Delivery = mobj.Para11 == null ? "" : mobj.Para11.Replace("'", "");
                Model.Collection = mobj.Para12 == null ? "" : mobj.Para12.Replace("'", "");
                Model.ChargeType = mobj.Para13 == null ? "" : mobj.Para13.Replace("'", "");
                Model.Particular = mobj.Para14 == null ? "" : mobj.Para14.Replace("'", "");
                Model.Unit = mobj.Para15 == null ? "" : mobj.Para15.Replace("'", "");
                Model.LrGenetate = mobj.Para16 == null ? "" : mobj.Para16.Replace("'", "");

                Model.BillRelationDetails = mobj.Para17 == "T" ? true : false;
                Model.DispatchDetails = mobj.Para18 == "T" ? true : false;
                Model.DeliveryDetails = mobj.Para19 == "T" ? true : false;
                Model.ExpensesDetails = mobj.Para20 == "T" ? true : false;

                Model.ChargeShow = mobj.Para21 == "T" ? true : false;
                Model.BillChargeShow = mobj.Para22 == "T" ? true : false;
                Model.StockBranch = mobj.Para23 == null ? "" : mobj.Para23.Replace("'", "");
                Model.StockType = mobj.Para24 == null ? "" : mobj.Para24.Replace("'", "");
                Model.TripDetails = mobj.Para25 == "T" ? true : false;
                Model.AdvBalDetails = mobj.Para26 == "T" ? true : false;
                Model.PLAccount = mobj.Para27 == "T" ? true : false;
                Model.SkipDuplicateFM = mobj.Para28 == "T" ? true : false;

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
        public ActionResult SaveParameter(LorryReceiptReportsVM Model)
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
                    //var tfatSearch1 = ctxTFAT.TfatSearch.Where(x => x.Code.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower()).ToList();
                    //string[] PLDetailsColmn = new string[] { "P/L AC" };
                    //string[] DispatchDetailsColmn1 = new string[] { "FMPF001", "FMPF002", "FMPF003", "FMPF004", "FMPF005", "FMPF006", "FMPF007", "FMPF008", "FMPF009", "FMPF010" };

                    //tfatSearch1.Where(x => PLDetailsColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                    //tfatSearch1.Where(x => DispatchDetailsColmn1.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
                    //ctxTFAT.SaveChanges();


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


                    mobj.DocNo = Model.ConsignmentNo;
                    mobj.StartDate = ConvertDDMMYYTOYYMMDD(Model.FromDate);
                    mobj.EndDate = ConvertDDMMYYTOYYMMDD(Model.ToDate);
                    mobj.HideColumnList = HiddenColumn;

                    mobj.Para1 = Model.LrMode;
                    mobj.Para2 = Model.FromBranch;
                    mobj.Para3 = Model.ToBranch;
                    mobj.Para4 = Model.LrType;
                    mobj.Para5 = Model.FromDestination;
                    mobj.Para6 = Model.ToDestination;
                    mobj.Para7 = Model.BillingBranch;
                    mobj.Para8 = Model.Consignor;
                    mobj.Para9 = Model.Consignee;
                    mobj.Para10 = Model.BillingParty;
                    mobj.Para11 = Model.Delivery;
                    mobj.Para12 = Model.Collection;
                    mobj.Para13 = Model.ChargeType;
                    mobj.Para14 = Model.Particular;
                    mobj.Para15 = Model.Unit;
                    mobj.Para16 = Model.LrGenetate;

                    mobj.Para17 = Model.BillRelationDetails == true ? "T" : "F";
                    mobj.Para18 = Model.DispatchDetails == true ? "T" : "F";
                    mobj.Para19 = Model.DeliveryDetails == true ? "T" : "F";
                    mobj.Para20 = Model.ExpensesDetails == true ? "T" : "F";

                    mobj.Para21 = Model.ChargeShow == true ? "T" : "F";
                    mobj.Para22 = Model.BillChargeShow == true ? "T" : "F";
                    mobj.Para23 = Model.StockBranch;
                    mobj.Para24 = Model.StockType;
                    mobj.Para25 = Model.TripDetails == true ? "T" : "F";
                    mobj.Para26 = Model.AdvBalDetails == true ? "T" : "F";
                    mobj.Para27 = Model.PLAccount == true ? "T" : "F";
                    mobj.Para28 = Model.SkipDuplicateFM == true ? "T" : "F";
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
        public ActionResult DeleteParamete(LorryReceiptReportsVM Model)
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