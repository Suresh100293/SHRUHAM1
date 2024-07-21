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
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class ReceivableAnalysisController : BaseController
    {
        private static string mbasegr = "";
        decimal mOpeningBalance = 0;

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

        private List<SelectListItem> PopulateDebtors()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM Master where Hide='false' and (BaseGr='D' or BaseGr='U') order by Name ";
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

        private List<SelectListItem> PopulateCreditors()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM Master where Hide='false' and (BaseGr='S' or BaseGr='U') order by Name ";
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

        public ActionResult PopulateSaveReports(string ViewDataId, string MainType)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT ReportNameAlias, ReportName FROM ReportParameters where Reports='" + ViewDataId + "' and Para1='" + MainType + "'  order by ReportNameAlias ";
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

        public ActionResult GetFormat(string Format)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem
            {
                Text = "Invoice Wise Outstanding",
                Value = "Payment Reminder Letter"
            });
            items.Add(new SelectListItem
            {
                Text = "Invoice Wise Ageing Analysis",
                Value = "OS Ageing"
            });
            items.Add(new SelectListItem
            {
                Text = "Party Wise Ageing Summary",
                Value = "New Outstanding Report"
            });
            items.Add(new SelectListItem
            {
                Text = "Party Wise Ageing Summary Total Debit-Credit",
                Value = "Party Ageing Summary"
            });
            items.Add(new SelectListItem
            {
                Text = "Party-Invoice Outstanding With Consignment",
                Value = "Outstanding Report With LR"
            });
            items.Add(new SelectListItem
            {
                Text = "Party-Invoice O/S (with Ref)",
                Value = "OS With Reference-2"
            });
            items.Add(new SelectListItem
            {
                Text = "Party-Invoice O/S (with Ref And Ageing)",
                Value = "OS Ageing with Ref"
            });
            items.Add(new SelectListItem
            {
                Text = "Only Un-Adjust ",
                Value = "UnAdjust Report"
            });
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPrintFormat(string Format)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem
            {
                Text = "Invoice Wise Outstanding",
                Value = "Payment Reminder Letter"
            });
            items.Add(new SelectListItem
            {
                Text = "Invoice Wise Register Outstanding ",
                Value = "Payment Reminder Letter Register"
            });
            items.Add(new SelectListItem
            {
                Text = "Invoice Wise Ageing Analysis",
                Value = "OS Ageing"
            });
            items.Add(new SelectListItem
            {
                Text = "Party Wise Ageing Summary",
                Value = "New Outstanding Report"
            });
            items.Add(new SelectListItem
            {
                Text = "Party Wise Ageing Summary Total Debit-Credit",
                Value = "Party Ageing Summary"
            });
            items.Add(new SelectListItem
            {
                Text = "Party-Invoice Outstanding With Consignment",
                Value = "Outstanding Report With LR"
            });
            items.Add(new SelectListItem
            {
                Text = "Party-Invoice O/S (with Ref)",
                Value = "OS With Reference-2"
            });
            items.Add(new SelectListItem
            {
                Text = "Party-Invoice O/S (with Ref And Ageing)",
                Value = "OS Ageing with Ref"
            });
            items.Add(new SelectListItem
            {
                Text = "Only Un-Adjust ",
                Value = "UnAdjust Report"
            });
            items.Add(new SelectListItem
            {
                Text = "Dynamic Summary O/S ",
                Value = "DynamicSummary"
            });




            //items.Add(new SelectListItem
            //{
            //    Text = "Invoice wise Outstanding With Ageing Analysis",//OS Ageing Same
            //    Value = "Invoice wise Outstanding"
            //});



            return Json(items, JsonRequestBehavior.AllowGet);
        }

        // GET: Accounts/ReceivableAnalysis
        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, "Report", Model.Header, "", DateTime.Now, 0, "", "", "NA");
            if (Model.Document == "" || Model.Document == null)
            {
                Model.FromDate = (new DateTime(1950, 1, 1)).ToShortDateString();
                //System.Web.HttpContext.Current.Session["StartDate"].ToString();
                Model.ToDate = DateTime.Today.ToShortDateString();
                //System.Web.HttpContext.Current.Session["LastDate"].ToString();

            }

            Model.StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

            ExecuteStoredProc("dbo.SPTFAT_DeleteTempOS");
            //mbasegr = Model.MainType;
            ViewBag.ViewDataId = Model.ViewDataId;
            Model.ReportTypeL = String.IsNullOrEmpty(Model.ReportTypeL) == true ? Model.ViewDataId : Model.ReportTypeL;
            //Model.ReportTypeL = Model.ViewDataId;
            Model.AccountName = NameofAccount(Model.Document);
            Model.MainType = Model.MainType;
            ViewBag.Header = Model.Header;
            ViewBag.Controller = Model.Controller;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;

            if (Model.MainType == "SL")
            {
                Model.Accounts = PopulateDebtors();
            }
            else
            {
                Model.Accounts = PopulateCreditors();
                Model.SundryCreditorsFilterGroups = "";
                Model.ARAPReqOnly = true;
            }
            Model.Customers = PopulateCustomers();

            //Model.Branch = mbranchcode;
            Model.Branch = String.IsNullOrEmpty(Model.Branch) == true ? mbranchcode : Model.Branch;

            Model.Age1 = "30";
            Model.Age2 = "60";
            Model.Age3 = "90";
            Model.Age4 = "120";
            Model.Age5 = "150";
            Model.Age6 = "180";
            Model.Supress = true;

            var CodeList = ctxTFAT.ReportHeader.Where(x => x.SubCodeOf == Model.ViewDataId).Select(x => x.Code).ToList();
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => CodeList.Contains(x.Code)).ToList();
            tfatSearch.ForEach(x => x.IsHidden = false);
            tfatSearch.Where(x => x.ColWidth == 0).ToList().ForEach(z => z.IsHidden = true);
            ctxTFAT.SaveChanges();


            return View(Model);
        }

        public ActionResult GetAccountList(string term, string MainType, bool Customer)
        {
            if (MainType == "SL")
            {
                if (term == "")
                {
                    if (Customer)
                    {
                        var result = ctxTFAT.CustomerMaster.Where(x => (x.BaseGr == "D" || x.BaseGr == "U") && x.Hide == false).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).Take(15).ToList();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = ctxTFAT.Master.Where(x => (x.BaseGr == "D" || x.BaseGr == "U") && x.Hide == false).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).Take(15).ToList();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }

                }
                else
                {
                    if (Customer)
                    {
                        var result = ctxTFAT.CustomerMaster.Where(x => (x.BaseGr == "D" || x.BaseGr == "U") && x.Hide == false && x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).ToList();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = ctxTFAT.Master.Where(x => (x.BaseGr == "D" || x.BaseGr == "U") && x.Hide == false && x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).ToList();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }

                }
            }
            else
            {
                if (term == "")
                {
                    var result = ctxTFAT.Master.Where(x => (x.BaseGr == "S" || x.BaseGr == "U") && x.Hide == false && x.ARAP == true).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).Take(15).ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = ctxTFAT.Master.Where(x => (x.BaseGr == "S" || x.BaseGr == "U") && x.Hide == false && x.Name.Contains(term) && x.ARAP == true).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }

        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            #region Set Parameters
            ppara01 = ""; ppara02 = ""; ppara03 = ""; ppara04 = ""; ppara05 = "";
            ppara06 = ""; ppara07 = ""; ppara08 = ""; ppara09 = ""; ppara10 = "";
            ppara11 = ""; ppara12 = ""; ppara13 = ""; ppara14 = ""; ppara15 = "";
            ppara16 = ""; ppara17 = ""; ppara18 = ""; ppara19 = ""; ppara20 = "";
            ppara21 = ""; ppara22 = ""; ppara23 = ""; ppara24 = ""; mpara = "";

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
                                ppara04 = GetPara[i];
                                mpara = mpara + "para" + (i + 4).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                break;
                            case 2:
                                ppara09 = GetPara[i];
                                mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                break;
                            case 3:
                                ppara10 = GetPara[i];
                                mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                break;
                            case 4:
                                ppara11 = GetPara[i];
                                mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                break;
                            case 5:
                                ppara12 = GetPara[i];
                                mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                break;
                            case 6:
                                ppara13 = GetPara[i];
                                mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                break;
                            case 7:
                                ppara14 = GetPara[i];
                                mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                break;
                            case 8:
                                ppara15 = GetPara[i];
                                mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                break;
                        }
                    }
                }
            }

            #endregion

            #region Show / Hide BillSubmission Columns
            string[] BillSubmissionDetailsColumns = new string[] { "Submission Date", "Bill Reamrk", "Through" };
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            if (ppara15 == "Yes")
            {
                Model.BillDetails = true;
                tfatSearch.Where(x => BillSubmissionDetailsColumns.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            }
            else
            {
                Model.BillDetails = false;
                tfatSearch.Where(x => BillSubmissionDetailsColumns.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }
            string[] CUstomerCode = new string[] { "Customer" };
            if (Model.MainType == "S")
            {
                tfatSearch.Where(x => CUstomerCode.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }
            ctxTFAT.SaveChanges();
            #endregion


            List<string> CodeList = new List<string>();
            if (Model.MainType != "SL")
            {
                if (String.IsNullOrEmpty(Model.Code))
                {
                    if (!String.IsNullOrEmpty(Model.SundryCreditorsFilterGroups))
                    {
                        //var List = Model.SundryCreditorsFilterGroups.Split('^');
                        //foreach (var item in List)
                        //{
                        //    if (item == "B")
                        //    {
                        //        CodeList.AddRange(ctxTFAT.Master.Where(x => x.OthPostType.Contains("B")).Select(x => x.Code).ToList());
                        //    }
                        //    else if (item == "H")
                        //    {
                        //        CodeList.AddRange(ctxTFAT.Master.Where(x => x.OthPostType.Contains("V")).Select(x => x.Code).ToList());
                        //    }
                        //    else if (item == "D")
                        //    {
                        //        CodeList.AddRange(ctxTFAT.Master.Where(x => x.OthPostType.Contains("D")).Select(x => x.Code).ToList());
                        //    }
                        //    else if (item == "V")
                        //    {
                        //        CodeList.AddRange(ctxTFAT.Master.Where(x => (x.BaseGr == "U" || x.BaseGr == "S") && x.OthPostType.Length == 0).Select(x => x.Code).ToList());
                        //    }
                        //}
                        //Model.Code = string.Join(",", CodeList);

                        Model.Code = "Payable";
                    }

                    if (Model.ARAPReqOnly)
                    {
                        //CodeList.AddRange(ctxTFAT.Master.Where(x => x.ARAP == true).Select(x => x.Code).ToList());
                        //Model.Code = string.Join(",", CodeList);
                        Model.Code = "Payable";
                    }
                }
            }

            var date = Model.Date.Replace("-", "/").Split(':');
            Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

            if ((Model.ViewDataId == "Payment Reminder Letter" || Model.ViewDataId == "Payment Reminder Letter Register") && Model.MainType != "S")
            {
                var ReportHeader = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).FirstOrDefault();
                ReportHeader.pMerge = "7";
                ReportHeader.pToMerge = "1,2,3,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31";
                ctxTFAT.SaveChanges();
            }
            else if (Model.ViewDataId == "Outstanding Report With LR")
            {
                var ReportHeader = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).FirstOrDefault();
                ReportHeader.pMerge = "7";
                ReportHeader.pToMerge = "1,2,3,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27";
                ctxTFAT.SaveChanges();
            }
            else
            {
                var ReportHeader = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).FirstOrDefault();
                ReportHeader.pMerge = "";
                ReportHeader.pToMerge = "";
                ctxTFAT.SaveChanges();
            }
            if (Model.ViewDataId != "OS With Reference-2" && Model.ViewDataId != "OS Ageing with Ref")
            {
                Model.Supress = true;
            }

            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand();// ("SPTFAT_ReceivableAnalysis", tfat_conx);
            cmd.Connection = tfat_conx;
            bool Noskip = true;
            bool AddPrameter = false;
            if (Model.ViewDataId == "Outstanding Report With LR")
            {
                ExecuteStoredProc("Drop Table ztmp_zOS");
                cmd.CommandText = "SPTFAT_ReceivableAnalysisWithLR";
            }
            else if (Model.ViewDataId == "Payment Reminder Letter" || Model.ViewDataId == "Payment Reminder Letter Register" || Model.ViewDataId == "UnAdjust Report" || Model.ViewDataId == "Invoice wise Outstanding" || Model.ViewDataId == "OS Ageing" || Model.ViewDataId == "Party Ageing Summary" || Model.ViewDataId == "New Outstanding Report")
            {
                AddPrameter = true;
                Noskip = false;
                cmd.CommandText = "SPTFAT_ReceivableAnalysis";
                Createztmp_zOS(Model);
            }
            else
            {
                Noskip = false;
                cmd.CommandText = "SSPTFAT_ReceivableWithRefDoc";
                Createztmp_zOSRef(Model);
            }
            if (Noskip)
            {
                var GsetBaseGr = Model.Customer == true ? "D" : ctxTFAT.Master.Where(x => x.Code == Model.Code).Select(x => x.BaseGr).FirstOrDefault();
                ppara07 = "Yes";
                mbasegr = GsetBaseGr;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;
                cmd.Parameters.Add("@mBaseGr", SqlDbType.VarChar).Value = mbasegr == null ? Model.MainType == "SL" ? "D" : "S" : mbasegr;
                cmd.Parameters.Add("@mDate", SqlDbType.Date).Value = Model.ToDate;
                cmd.Parameters.Add("@mDate2", SqlDbType.Date).Value = Model.FromDate;
                cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = ppara04 == null || ppara04 == "" ? mbranchcode : ppara04;
                cmd.Parameters.Add("@mLocations", SqlDbType.VarChar).Value = Fieldoftable("Warehouse", "Code", "Name='" + ppara01 + "'");
                cmd.Parameters.Add("@mConsiderCRPR", SqlDbType.Bit).Value = ppara06 == "Yes" ? true : false;
                cmd.Parameters.Add("@mSuppressZero", SqlDbType.Bit).Value = Model.Supress;
                cmd.Parameters.Add("@mArea", SqlDbType.VarChar).Value = "";// ppara04;
                cmd.Parameters.Add("@mParties", SqlDbType.VarChar).Value = Model.Code == null ? "" : Model.Code;
                cmd.Parameters.Add("@mGrps", SqlDbType.VarChar).Value = Fieldoftable("MasterGroups", "Code", "Name='" + ppara16 + "'");
                cmd.Parameters.Add("@mSalesman", SqlDbType.VarChar).Value = ppara02;//FieldoftableNumber("Salesman", "Code", "Name='" + 
                cmd.Parameters.Add("@mBroker", SqlDbType.VarChar).Value = ppara03;//FieldoftableNumber("Broker", "Code", "Name='" + ppara03 + "'");
                cmd.Parameters.Add("@mCategory", SqlDbType.VarChar).Value = ppara05;
                cmd.Parameters.Add("@mRefTillDate", SqlDbType.Bit).Value = ppara07 == "Yes" ? true : false;
                cmd.Parameters.Add("@mDocString", SqlDbType.VarChar).Value = mDocString;
                cmd.Parameters.Add("@mCustomer", SqlDbType.VarChar).Value = Model.Customer == true ? "1" : "0";
                if (AddPrameter)
                {
                    cmd.Parameters.Add("@mBillSubmission", SqlDbType.VarChar).Value = ppara15 == "Yes" ? true : false;
                }

                tfat_conx.Open();
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                tfat_conx.Close();
                tfat_conx.Dispose();
            }


            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        public void Createztmp_zOSRef(GridOption Model)
        {
            string Query = "";

            if (String.IsNullOrEmpty(Model.Code))
            {
                if (!Model.Customer)
                {
                    Query = "Select l.Branch,l.ParentKey,l.TableKey,l.Type,l.Prefix,l.Srl,l.Sno,l.DocDate,l.Code,m.Name,l.BillNumber," +
                        "l.BillDate,x.intrate as InterestRate,l.CrPeriod,x.CrPeriod as MasterCrPeriod, l.DocDate + l.CrPeriod as DueDate," +
                        " Datediff(d, l.DocDate + l.CrPeriod, Getdate()) as oDueDays,(Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' " +
                        "then(Select s.BillNumber from Sales s where s.TableKey = l.ParentKey) Else" +
                        "(Select s.BillNumber from Purchase s where s.TableKey = l.ParentKey) End) as OrdNumber," +
                        "(Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' then(Select s.BillDate from Sales s where s.TableKey = l.ParentKey) " +
                        "Else(Select s.BillDate from Purchase s where s.TableKey = l.ParentKey) End) as OrdDate," +
                        "Pending = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) " +
                        "then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + Model.ToDate + "', l.Branch) end) else 0 End)," +
                        "OnAccount = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U1') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then" +
                        "(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + Model.ToDate + "', l.Branch) end) else 0 end)," +
                        "Amt = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 End)," +
                        "UnAdj = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 end)," +
                        "Flag = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then 0 else 1 End)," +
                        "a.Adrl1,a.Adrl2,a.Adrl3,a.City,a.State,a.Pin,a.Country,a.Person,m.Category," +
                        "(Select top 1 Name from PartyCategory where PartyCategory.Code = m.Category) as CategoryName," +
                        "m.SalesMan,(Select top 1 Name from Salesman where Salesman.Code = m.Salesman) as SalesManName," +
                        "a.Area,(Select top 1 Name from AreaMaster where AreaMaster.Code = a.Area) as AreaName,m.Grp," +
                        "(Select top 1 Name from MasterGroups where MasterGroups.Code = m.Grp) as GrpName,isnull(r.type, '') as rType," +
                        "isnull(r.prefix, '') as rPrefix,isnull(r.srl, '') as rSrl,isnull(r.sno, 0) as rSno,isnull(r.amount, 0) as rAmount," +
                        "r.DocDate as rDocDate,l.Narr,l.LocationCode" +
                        " into ztmp_zOSRef from Ledger l Left join Master m on m.Code = l.Code " +
                        " Left join MasterInfo x on l.code = x.code Left join Address a on a.Code = l.Code and a.Sno = 0 " +
                        " Left Outer Join Outstanding r on r.TableRefKey = l.TableKey And r.DocDate <= '" + Model.ToDate + "' " +
                        " where(m.BaseGr = 'U' or m.BaseGr = " + (Model.MainType == "SL" ? "'D'" : "'S'") + ") and Charindex(l.Branch, '" + ppara04 + "')<> 0 and l.MainType <> 'MV'" +
                        " and l.MainType <> 'PV' and (case when 0 <> 0 then" +
                        " l.DocDate + (Case when l.crperiod = null then 0 else l.crperiod end) else l.DocDate end) between '" + Model.FromDate + "' and '" + Model.ToDate + "'";
                }
                else
                {
                    Query = "Select l.Branch,l.ParentKey,l.TableKey,l.Type,l.Prefix,l.Srl,l.Sno,l.DocDate,l.Code,c.Name,l.BillNumber,l.BillDate, " +
                        "x.intrate as InterestRate,l.CrPeriod,x.CrPeriod as MasterCrPeriod, l.DocDate + l.CrPeriod as DueDate, Datediff(d, l.DocDate + l.CrPeriod, Getdate()) as oDueDays, " +
                        "(Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' then(Select s.BillNumber from Sales s where s.TableKey = l.ParentKey) Else(Select s.BillNumber from Purchase s where s.TableKey = l.ParentKey) End) as OrdNumber," +
                        "(Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' then(Select s.BillDate from Sales s where s.TableKey = l.ParentKey) Else(Select s.BillDate from Purchase s where s.TableKey = l.ParentKey) End) as OrdDate," +
                        "Pending = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) " +
                        "else dbo.GetOutstandingDate(l.TableKey, '" + Model.ToDate + "', l.Branch) end) else 0 End)," +
                        "OnAccount = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U1') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) " +
                        "else dbo.GetOutstandingDate(l.TableKey, '" + Model.ToDate + "', l.Branch) end) else 0 end)," +
                        "Amt = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 End)," +
                        "UnAdj = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 end)," +
                        "Flag = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then 0 else 1 End)," +
                        "a.Adrl1,a.Adrl2,a.Adrl3,a.City,a.State,a.Pin,a.Country,a.Person,m.Category,(Select top 1 Name from PartyCategory where PartyCategory.Code = m.Category) as CategoryName," +
                        "m.SalesMan,(Select top 1 Name from Salesman where Salesman.Code = m.Salesman) as SalesManName," +
                        "a.Area,(Select top 1 Name from AreaMaster where AreaMaster.Code = a.Area) as AreaName,m.Grp,(Select top 1 Name from MasterGroups where MasterGroups.Code = m.Grp) as GrpName," +
                        "isnull(r.type, '') as rType,isnull(r.prefix, '') as rPrefix,isnull(r.srl, '') as rSrl,isnull(r.sno, 0) as rSno,isnull(r.amount, 0) as rAmount,r.DocDate as rDocDate,l.Narr,l.LocationCode " +
                        " into ztmp_zOSRef from Ledger l Left join Master m on m.Code = l.Code Left join customerMaster c on c.Code = l.Party " +
                        " Left join MasterInfo x on l.code = x.code " +
                        " Left join Address a on a.Code = l.Code and a.Sno = 0 " +
                        " Left Outer Join Outstanding r on r.TableRefKey = l.TableKey And r.DocDate <= '" + Model.ToDate + "' " +
                        " where(m.BaseGr = 'U' or m.BaseGr = " + (Model.MainType == "SL" ? "'D'" : "'S'") + ") and Charindex(l.Branch, '" + ppara04 + "')<> 0 and l.MainType <> 'MV' " +
                        " and l.MainType <> 'PV' and (case when 0 <> 0 then l.DocDate + (Case when l.crperiod = null then 0 else l.crperiod end) else l.DocDate end) between '" + Model.FromDate + "' and '" + Model.ToDate + "'";
                }
            }
            else
            {
                string FilterOnCode = "";
                if (Model.Code == "Payable")//Payable
                {
                    if (!string.IsNullOrEmpty(Model.SundryCreditorsFilterGroups))
                    {
                        FilterOnCode = " l.code in (select M.Code From Master M where  ";
                        var SplitOtherPostType = Model.SundryCreditorsFilterGroups.Split('^');
                        foreach (var item in SplitOtherPostType)
                        {
                            if (item == "B")
                            {
                                FilterOnCode += " M.OthPostType like '%B%' or";
                            }
                            else if (item == "H")
                            {
                                FilterOnCode += " M.OthPostType like '%V%' or";
                            }
                            else if (item == "D")
                            {
                                FilterOnCode += " M.OthPostType like '%D%' or";
                            }
                            else if (item == "V")
                            {
                                FilterOnCode = " l.code in (select M.Code From Master M where (M.BaseGr ='U' or  M.BaseGr ='S' ) and len(M.OthPostType)=0  or";
                            }
                        }
                        FilterOnCode = FilterOnCode.Substring(0, FilterOnCode.Length - 2) + ") ";
                    }
                    else if (Model.ARAPReqOnly)
                    {
                        FilterOnCode = " l.code in (select M.Code From Master M where M.ARAP ='true') ";
                    }
                }
                else//Receivable
                {
                    if (!Model.Customer)
                    {
                        FilterOnCode = " Charindex(l.code,'" + Model.Code + "')<>0 ";
                    }
                    else
                    {
                        FilterOnCode = " Charindex(l.party,'" + Model.Code + "')<>0 ";
                    }
                }

                if (!Model.Customer)
                {
                    Query = "Select l.Branch,l.ParentKey,l.TableKey,l.Type,l.Prefix,l.Srl,l.Sno,l.DocDate,l.Code,m.Name,l.BillNumber," +
                        "l.BillDate,x.intrate as InterestRate,l.CrPeriod,x.CrPeriod as MasterCrPeriod, l.DocDate + l.CrPeriod as DueDate," +
                        " Datediff(d, l.DocDate + l.CrPeriod, Getdate()) as oDueDays,(Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' " +
                        "then(Select s.BillNumber from Sales s where s.TableKey = l.ParentKey) Else" +
                        "(Select s.BillNumber from Purchase s where s.TableKey = l.ParentKey) End) as OrdNumber," +
                        "(Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' then(Select s.BillDate from Sales s where s.TableKey = l.ParentKey) " +
                        "Else(Select s.BillDate from Purchase s where s.TableKey = l.ParentKey) End) as OrdDate," +
                        "Pending = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) " +
                        "then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + Model.ToDate + "', l.Branch) end) else 0 End)," +
                        "OnAccount = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U1') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then" +
                        "(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + Model.ToDate + "', l.Branch) end) else 0 end)," +
                        "Amt = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 End)," +
                        "UnAdj = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 end)," +
                        "Flag = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then 0 else 1 End)," +
                        "a.Adrl1,a.Adrl2,a.Adrl3,a.City,a.State,a.Pin,a.Country,a.Person,m.Category," +
                        "(Select top 1 Name from PartyCategory where PartyCategory.Code = m.Category) as CategoryName," +
                        "m.SalesMan,(Select top 1 Name from Salesman where Salesman.Code = m.Salesman) as SalesManName," +
                        "a.Area,(Select top 1 Name from AreaMaster where AreaMaster.Code = a.Area) as AreaName,m.Grp," +
                        "(Select top 1 Name from MasterGroups where MasterGroups.Code = m.Grp) as GrpName,isnull(r.type, '') as rType," +
                        "isnull(r.prefix, '') as rPrefix,isnull(r.srl, '') as rSrl,isnull(r.sno, 0) as rSno,isnull(r.amount, 0) as rAmount," +
                        "r.DocDate as rDocDate,l.Narr,l.LocationCode" +
                        " into ztmp_zOSRef from Ledger l Left join Master m on m.Code = l.Code " +
                        " Left join MasterInfo x on l.code = x.code Left join Address a on a.Code = l.Code and a.Sno = 0 " +
                        " Left Outer Join Outstanding r on r.TableRefKey = l.TableKey And r.DocDate <= '" + Model.ToDate + "' " +
                        " where(m.BaseGr = 'U' or m.BaseGr = " + (Model.MainType == "SL" ? "'D'" : "'S'") + ") and Charindex(l.Branch, '" + ppara04 + "')<> 0 and l.MainType <> 'MV'" +
                        " and l.MainType <> 'PV' and "+ FilterOnCode + "  and (case when 0 <> 0 then" +
                        "  l.DocDate + (Case when l.crperiod = null then 0 else l.crperiod end) else l.DocDate end) between '" + Model.FromDate + "' and '" + Model.ToDate + "'";

                }
                else
                {
                    Query = "Select l.Branch,l.ParentKey,l.TableKey,l.Type,l.Prefix,l.Srl,l.Sno,l.DocDate,l.Code,c.Name,l.BillNumber,l.BillDate, " +
                        "x.intrate as InterestRate,l.CrPeriod,x.CrPeriod as MasterCrPeriod, l.DocDate + l.CrPeriod as DueDate, Datediff(d, l.DocDate + l.CrPeriod, Getdate()) as oDueDays, " +
                        "(Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' then(Select s.BillNumber from Sales s where s.TableKey = l.ParentKey) Else(Select s.BillNumber from Purchase s where s.TableKey = l.ParentKey) End) as OrdNumber," +
                        "(Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' then(Select s.BillDate from Sales s where s.TableKey = l.ParentKey) Else(Select s.BillDate from Purchase s where s.TableKey = l.ParentKey) End) as OrdDate," +
                        "Pending = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) " +
                        "else dbo.GetOutstandingDate(l.TableKey, '" + Model.ToDate + "', l.Branch) end) else 0 End)," +
                        "OnAccount = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U1') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) " +
                        "else dbo.GetOutstandingDate(l.TableKey, '" + Model.ToDate + "', l.Branch) end) else 0 end)," +
                        "Amt = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 End)," +
                        "UnAdj = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 end)," +
                        "Flag = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then 0 else 1 End)," +
                        "a.Adrl1,a.Adrl2,a.Adrl3,a.City,a.State,a.Pin,a.Country,a.Person,m.Category,(Select top 1 Name from PartyCategory where PartyCategory.Code = m.Category) as CategoryName," +
                        "m.SalesMan,(Select top 1 Name from Salesman where Salesman.Code = m.Salesman) as SalesManName," +
                        "a.Area,(Select top 1 Name from AreaMaster where AreaMaster.Code = a.Area) as AreaName,m.Grp,(Select top 1 Name from MasterGroups where MasterGroups.Code = m.Grp) as GrpName," +
                        "isnull(r.type, '') as rType,isnull(r.prefix, '') as rPrefix,isnull(r.srl, '') as rSrl,isnull(r.sno, 0) as rSno,isnull(r.amount, 0) as rAmount,r.DocDate as rDocDate,l.Narr,l.LocationCode " +
                        " into ztmp_zOSRef from Ledger l Left join Master m on m.Code = l.Code Left join customerMaster c on c.Code = l.Party " +
                        " Left join MasterInfo x on l.code = x.code " +
                        " Left join Address a on a.Code = l.Code and a.Sno = 0 " +
                        " Left Outer Join Outstanding r on r.TableRefKey = l.TableKey And r.DocDate <= '" + Model.ToDate + "' " +
                        " where(m.BaseGr = 'U' or m.BaseGr = " + (Model.MainType == "SL" ? "'D'" : "'S'") + ") and Charindex(l.Branch, '" + ppara04 + "')<> 0 and l.MainType <> 'MV' " +
                        " and l.MainType <> 'PV' and " + FilterOnCode + " and (case when 0 <> 0 then l.DocDate + (Case when l.crperiod = null then 0 else l.crperiod end) else l.DocDate end) between '" + Model.FromDate + "' and '" + Model.ToDate + "'";

                }
            }
            ExecuteStoredProc("DROP TABLE IF EXISTS ztmp_zOSRef");
            ExecuteStoredProc(Query);
            if (Model.Supress)
            {
                ExecuteStoredProc("Delete from ztmp_zOSRef where Pending=0 and OnAccount=0;");
            }
        }
        public void Createztmp_zOS(GridOption Model)
        {
            string Query = "";

            string FilterOnCode = "";
            if (Model.Code == "Payable")//Payable
            {
                if (!string.IsNullOrEmpty(Model.SundryCreditorsFilterGroups))
                {
                    FilterOnCode = " l.code in (select M.Code From Master M where  ";
                    var SplitOtherPostType = Model.SundryCreditorsFilterGroups.Split('^');
                    foreach (var item in SplitOtherPostType)
                    {
                        if (item == "B")
                        {
                            FilterOnCode += " M.OthPostType like '%B%' or";
                        }
                        else if (item == "H")
                        {
                            FilterOnCode += " M.OthPostType like '%V%' or";
                        }
                        else if (item == "D")
                        {
                            FilterOnCode += " M.OthPostType like '%D%' or";
                        }
                        else if (item == "V")
                        {
                            FilterOnCode = " l.code in (select M.Code From Master M where (M.BaseGr ='U' or  M.BaseGr ='S' ) and len(M.OthPostType)=0  or";
                        }
                        
                    }
                    FilterOnCode = FilterOnCode.Substring(0, FilterOnCode.Length - 2) + ") ";
                }
                else if (Model.ARAPReqOnly)
                {
                    FilterOnCode = " l.code in (select M.Code From Master M where M.ARAP ='true') ";
                }
            }
            else//Receivable
            {
                if (!Model.Customer)
                {
                    FilterOnCode = (string.IsNullOrEmpty(Model.Code) == false ? " Charindex(l.code,'" + Model.Code + "')<>0 " : "1=1");
                }
                else
                {
                    FilterOnCode = (string.IsNullOrEmpty(Model.Code) == false ? " Charindex(l.party,'" + Model.Code + "')<>0 " : "1=1");
                }
            }




            if (Model.Customer == false)
            {
                if (Model.BillDetails)
                {
                    Query = "Select distinct l.RefDoc as RefDoc,l.Branch,l.ParentKey,l.TableKey,l.Type,l.Prefix,l.Srl,l.Sno,l.DocDate,l.Code,m.Name,l.BillNumber,l.BillDate,l.Narr as Narr, "
                            + " x.intrate as InterestRate,l.CrPeriod,x.CrPeriod as MasterCrPeriod, l.DocDate + l.CrPeriod as DueDate, Datediff(d, l.DocDate + l.CrPeriod, Getdate()) as oDueDays,isnull(BillS.Remark, '') as BillRemark,isnull(BillS.Through, '') as BillThrough,Bills.SubDt as BillSubDate, "
                            + "	(Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' then(Select s.BillNumber from Sales s where s.TableKey = l.ParentKey and s.Branch = l.Branch) Else(Select s.BillNumber from Purchase s where s.TableKey = l.ParentKey and s.Branch = l.Branch) End) as OrdNumber, "
                            + "	(Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' then(Select s.BillDate from Sales s where s.TableKey = l.ParentKey and s.Branch = l.Branch) Else(Select s.BillDate from Purchase s where s.TableKey = l.ParentKey and s.Branch = l.Branch) End) as OrdDate, "
                            + "	Pending = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + Model.ToDate + "', l.Branch) end) else 0 End), "
                            + "	OnAccount = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U1') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey,'" + Model.ToDate + "', l.Branch) end) else 0 end), "
                            + "	Amt = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 End), "
                            + "	UnAdj = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 end), "
                            + "	Flag = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then 0 else 1 End), "
                            + "	a.Adrl1,a.Adrl2,a.Adrl3,a.City,a.State,a.Pin,a.Country,a.Person,m.Category,(Select top 1 Name from PartyCategory where PartyCategory.Code = m.Category) as CategoryName, "
                            + "	m.SalesMan,isnull((Select top 1 Name from Salesman where Salesman.Code = m.Salesman),'~') as SalesManName, "
                            + "	a.Area,(Select top 1 Name from AreaMaster where AreaMaster.Code = a.Area) as AreaName, "
                            + "	m.Grp,(Select top 1 Name from MasterGroups where MasterGroups.Code = m.Grp) as GrpName,l.LocationCode, "
                            + "	m.Broker,isnull((Select top 1 Name from Broker where Broker.Code = m.Broker),'~') as BrokerName,l.party as Party, "
                            + "	(Select SUBSTRING((Select ',[' + Stock.Code + '] ' + Itemmaster.name AS 'data()' FROM Stock, Itemmaster where Stock.code = Itemmaster.code and Stock.Parentkey = l.Parentkey FOR XML PATH('')), 2, 9999)) as Itemdetails "
                            + "   into ztmp_zOS from Ledger l "
                            + "   left join Master m on m.Code = l.Code  "
                            + "   left join MasterInfo x on l.code = x.code  "
                            + "   left join Address a on a.Code = l.Code and a.Sno = 0 "
                            + "   left join BillSubRef BillRef on BillRef.BillBranch + BillRef.BillTableKey = l.Branch + l.TableKey left join BillSubmission BillS on BillS.DocNo = BillRef.DocNo "
                            + "   where  " + (string.IsNullOrEmpty(Model.Code) == false ? " Charindex(l.code,'" + Model.Code + "')<>0 " : "1=1") + " and  (m.BaseGr = 'U' or m.BaseGr = " + (Model.MainType == "SL" ? "'D'" : "'S'") + ") and Charindex(l.Branch, '" + ppara04 + "')<> 0 and l.MainType <> 'MV' and l.MainType <> 'PV' and "
                            + "   (case when 0 <> 0 then l.DocDate + (Case when l.crperiod = null then 0 else l.crperiod end) else l.DocDate end) between '" + Model.FromDate + "' and '" + Model.ToDate + "'";
                }
                else
                {
                    Query = "Select distinct l.RefDoc as RefDoc,l.Branch,l.ParentKey,l.TableKey,l.Type,l.Prefix,l.Srl,l.Sno,l.DocDate,l.Code,m.Name,l.BillNumber,l.BillDate,l.Narr as Narr, "
                            + " x.intrate as InterestRate,l.CrPeriod,x.CrPeriod as MasterCrPeriod, l.DocDate + l.CrPeriod as DueDate, Datediff(d, l.DocDate + l.CrPeriod, Getdate()) as oDueDays,'' as BillRemark,'' as BillThrough,GETDATE() as BillSubDate, "
                            + " (Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' then(Select s.BillNumber from Sales s where s.TableKey = l.ParentKey and s.Branch = l.Branch) Else(Select s.BillNumber from Purchase s where s.TableKey = l.ParentKey and s.Branch = l.Branch) End) as OrdNumber, "
                            + " (Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' then(Select s.BillDate from Sales s where s.TableKey = l.ParentKey and s.Branch = l.Branch) Else(Select s.BillDate from Purchase s where s.TableKey = l.ParentKey and s.Branch = l.Branch) End)  as OrdDate, "
                            + " Pending = (Case when(Debit <> 0 and (" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + Model.ToDate + "', l.Branch) end) else 0 End), "
                            + " OnAccount = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U1') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + Model.ToDate + "', l.Branch) end) else 0 end), "
                            + " Amt = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 End), "
                            + " UnAdj = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 end), "
                            + " Flag = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then 0 else 1 End),a.Adrl1,a.Adrl2,a.Adrl3,a.City,a.State,a.Pin,a.Country,a.Person,m.Category,(Select top 1 Name from PartyCategory where PartyCategory.Code = m.Category) as CategoryName, "
                            + " m.SalesMan,isnull((Select top 1 Name from Salesman where Salesman.Code = m.Salesman),'~')  as SalesManName, "
                            + " a.Area,(Select top 1 Name from AreaMaster where AreaMaster.Code = a.Area) as AreaName, "
                            + " m.Grp,(Select top 1 Name from MasterGroups where MasterGroups.Code = m.Grp) as GrpName,l.LocationCode, "
                            + " m.Broker,isnull((Select top 1 Name from Broker where Broker.Code = m.Broker),'~') as BrokerName,l.party as Party,(Select SUBSTRING((Select ',[' + Stock.Code + '] ' + Itemmaster.name AS 'data()' FROM Stock, Itemmaster where Stock.code = Itemmaster.code and Stock.Parentkey = l.Parentkey FOR XML PATH('')), 2, 9999)) as Itemdetails "
                            + "   into ztmp_zOS from Ledger l  "
                            + "   left join Master m on m.Code = l.Code  "
                            + "   left join MasterInfo x on l.code = x.code  "
                            + "   left join Address a on a.Code = l.Code and a.Sno = 0 "
                            + "   where " + FilterOnCode + " and (m.BaseGr = 'U' or m.BaseGr = " + (Model.MainType == "SL" ? "'D'" : "'S'") + ") and Charindex(l.Branch, '" + ppara04 + "')<> 0 and l.MainType <> 'MV' and l.MainType <> 'PV' and "
                            + "   (case when 0 <> 0 then l.DocDate + (Case when l.crperiod = null then 0 else l.crperiod end) else l.DocDate end) between '" + Model.FromDate + "' and '" + Model.ToDate + "'";
                }
            }
            else
            {
                if (Model.BillDetails)
                {
                    Query = "Select distinct l.RefDoc as RefDoc,l.Branch,l.ParentKey,l.TableKey,l.Type,l.Prefix,l.Srl,l.Sno,l.DocDate,c.Code,c.Name,l.BillNumber,l.BillDate,l.Narr as Narr,isnull(BillS.Remark,'') as BillRemark,isnull(BillS.Through,'') as BillThrough,Bills.SubDt as BillSubDate, "
                            + " x.intrate as InterestRate,l.CrPeriod,x.CrPeriod as MasterCrPeriod, l.DocDate + l.CrPeriod as DueDate, Datediff(d, l.DocDate + l.CrPeriod, Getdate()) as oDueDays, "
                            + " (Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' then(Select s.BillNumber from Sales s where s.TableKey = l.ParentKey and s.Branch = l.Branch) Else(Select s.BillNumber from Purchase s where s.TableKey = l.ParentKey and s.Branch = l.Branch) End) as OrdNumber, "
                            + " (Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' then(Select s.BillDate from Sales s where s.TableKey = l.ParentKey and s.Branch = l.Branch) Else(Select s.BillDate from Purchase s where s.TableKey = l.ParentKey and s.Branch = l.Branch) End) as OrdDate, "
                            + " Pending = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + Model.ToDate + "', l.Branch) end) else 0 End), "
                            + " OnAccount = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U1') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + Model.ToDate + "', l.Branch) end) else 0 end), "
                            + " Amt = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 End), "
                            + " UnAdj = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 end), "
                            + " Flag = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then 0 else 1 End), "
                            + " a.Adrl1,a.Adrl2,a.Adrl3,a.City,a.State,a.Pin,a.Country,a.Person,m.Category,(Select top 1 Name from PartyCategory where PartyCategory.Code = m.Category) as CategoryName, "
                            + " m.SalesMan,isnull((Select top 1 Name from Salesman where Salesman.Code = m.Salesman),'~') as SalesManName, "
                            + " a.Area,(Select top 1 Name from AreaMaster where AreaMaster.Code = a.Area) as AreaName, "
                            + " m.Grp,(Select top 1 Name from MasterGroups where MasterGroups.Code = m.Grp) as GrpName,l.LocationCode, "
                            + " m.Broker,isnull((Select top 1 Name from Broker where Broker.Code = m.Broker),'~') as BrokerName,l.party as Party, "
                            + " (Select SUBSTRING((Select ',[' + Stock.Code + '] ' + Itemmaster.name AS 'data()' FROM Stock, Itemmaster where Stock.code = Itemmaster.code and Stock.Parentkey = l.Parentkey FOR XML PATH('')), 2, 9999)) as Itemdetails "
                            + "   into ztmp_zOS from Ledger l "
                            + "   left join Master m on m.Code = l.Code  "
                            + "   left join customerMaster c on c.Code = l.Party  "
                            + "   left join CMasterInfo x on l.code = x.code  "
                            + "   left join Address a on a.Code = l.Code and a.Sno = 0 left join BillSubRef BillRef on BillRef.BillBranch + BillRef.BillTableKey = l.Branch + l.TableKey left join BillSubmission BillS on BillS.DocNo = BillRef.DocNo "
                            + "   where " + (string.IsNullOrEmpty(Model.Code) == false ? " Charindex(l.party,'" + Model.Code + "')<>0 " : "1=1") + " and (m.BaseGr = 'U' or m.BaseGr = " + (Model.MainType == "SL" ? "'D'" : "'S'") + ") and Charindex(l.Branch, '" + ppara04 + "')<> 0 and l.MainType <> 'MV' and l.MainType <> 'PV' and "
                            + "   (case when 0 <> 0 then l.DocDate + (Case when l.crperiod = null then 0 else l.crperiod end) else l.DocDate end) between '" + Model.FromDate + "' and '" + Model.ToDate + "'";
                }
                else
                {
                    Query = "Select distinct l.RefDoc as RefDoc,l.Branch,l.ParentKey,l.TableKey,l.Type,l.Prefix,l.Srl,l.Sno,l.DocDate,c.Code,c.Name,l.BillNumber,l.BillDate,l.Narr as Narr,'' as BillRemark,'' as BillThrough,GETDATE() as BillSubDate, "
                            + " x.intrate as InterestRate,l.CrPeriod,x.CrPeriod as MasterCrPeriod, l.DocDate + l.CrPeriod as DueDate, Datediff(d, l.DocDate + l.CrPeriod, Getdate()) as oDueDays, "
                            + " (Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' then(Select s.BillNumber from Sales s where s.TableKey = l.ParentKey and s.Branch = l.Branch) Else(Select s.BillNumber from Purchase s where s.TableKey = l.ParentKey and s.Branch = l.Branch) End) as OrdNumber, "
                            + " (Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' then(Select s.BillDate from Sales s where s.TableKey = l.ParentKey and s.Branch = l.Branch) Else(Select s.BillDate from Purchase s where s.TableKey = l.ParentKey and s.Branch = l.Branch) End) as OrdDate, "
                            + " Pending = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + Model.ToDate + "', l.Branch) end) else 0 End), "
                            + " OnAccount = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U1') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + Model.ToDate + "', l.Branch) end) else 0 end), "
                            + " Amt = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 End), "
                            + " UnAdj = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 end), "
                            + " Flag = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then 0 else 1 End), "
                            + " a.Adrl1,a.Adrl2,a.Adrl3,a.City,a.State,a.Pin,a.Country,a.Person,m.Category,(Select top 1 Name from PartyCategory where PartyCategory.Code = m.Category) as CategoryName, "
                            + " m.SalesMan,isnull((Select top 1 Name from Salesman where Salesman.Code = m.Salesman),'~') as SalesManName, "
                            + " a.Area,(Select top 1 Name from AreaMaster where AreaMaster.Code = a.Area) as AreaName, "
                            + " m.Grp,(Select top 1 Name from MasterGroups where MasterGroups.Code = m.Grp) as GrpName,l.LocationCode, "
                            + " m.Broker,isnull((Select top 1 Name from Broker where Broker.Code = m.Broker),'~') as BrokerName,l.party as Party, "
                            + " (Select SUBSTRING((Select ',[' + Stock.Code + '] ' + Itemmaster.name AS 'data()' FROM Stock, Itemmaster where Stock.code = Itemmaster.code and Stock.Parentkey = l.Parentkey FOR XML PATH('')), 2, 9999)) as Itemdetails "
                            + "  into ztmp_zOS from Ledger l "
                            + "  left join Master m on m.Code = l.Code  "
                            + "  left join customerMaster c on c.Code = l.Party  "
                            + "  left join CMasterInfo x on l.code = x.code  "
                            + "  left join Address a on a.Code = l.Code and a.Sno = 0 "
                            + "  where " + FilterOnCode + " and (m.BaseGr = 'U' or m.BaseGr = " + (Model.MainType == "SL" ? "'D'" : "'S'") + ") and Charindex(l.Branch, '" + ppara04 + "')<> 0 and l.MainType <> 'MV' and l.MainType <> 'PV' and "
                            + "  (case when 0 <> 0 then l.DocDate + (Case when l.crperiod = null then 0 else l.crperiod end) else l.DocDate end) between '" + Model.FromDate + "' and '" + Model.ToDate + "'";
                }
            }
            ExecuteStoredProc("DROP TABLE IF EXISTS ztmp_zOS");
            ExecuteStoredProc(Query);
            if (Model.Supress)
            {
                ExecuteStoredProc("Delete from ztmp_zOS where Pending=0 and OnAccount=0;");
            }

        }




        [HttpPost]
        public ActionResult GetGridStructureRecords1(GridOption Model)
        {

            #region Set Parameters

            ppara01 = ""; ppara02 = ""; ppara03 = ""; ppara04 = ""; ppara05 = "";
            ppara06 = ""; ppara07 = ""; ppara08 = ""; ppara09 = ""; ppara10 = "";
            ppara11 = ""; ppara12 = ""; ppara13 = ""; ppara14 = ""; ppara15 = "";
            ppara16 = ""; ppara17 = ""; ppara18 = ""; ppara19 = ""; ppara20 = "";
            ppara21 = ""; ppara22 = ""; ppara23 = ""; ppara24 = ""; mpara = "";


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
                                ppara04 = GetPara[i];
                                mpara = mpara + "para" + (i + 4).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                break;
                            case 2:
                                ppara09 = GetPara[i];
                                mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                break;
                            case 3:
                                ppara10 = GetPara[i];
                                mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                break;
                            case 4:
                                ppara11 = GetPara[i];
                                mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                break;
                            case 5:
                                ppara12 = GetPara[i];
                                mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                break;
                            case 6:
                                ppara13 = GetPara[i];
                                mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                break;
                            case 7:
                                ppara14 = GetPara[i];
                                mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                break;
                            case 8:
                                ppara15 = GetPara[i];
                                mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                break;
                        }
                    }
                }
            }

            #endregion

            #region Show / Hide BillSubmission Columns
            string[] BillSubmissionDetailsColumns = new string[] { "Submission Date", "Bill Reamrk", "Through" };
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            if (ppara15 == "Yes")
            {
                tfatSearch.Where(x => BillSubmissionDetailsColumns.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = false);
            }
            else
            {
                tfatSearch.Where(x => BillSubmissionDetailsColumns.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }
            string[] CUstomerCode = new string[] { "Customer" };
            if (Model.MainType == "S")
            {
                tfatSearch.Where(x => CUstomerCode.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            }
            ctxTFAT.SaveChanges();
            #endregion

            //if (1 == 2)
            {
                List<string> CodeList = new List<string>();
                if (Model.MainType != "SL")
                {
                    if (String.IsNullOrEmpty(Model.Code))
                    {
                        if (!String.IsNullOrEmpty(Model.SundryCreditorsFilterGroups))
                        {
                            var List = Model.SundryCreditorsFilterGroups.Split('^');
                            foreach (var item in List)
                            {
                                if (item == "B")
                                {
                                    CodeList.AddRange(ctxTFAT.Master.Where(x => x.OthPostType.Contains("B")).Select(x => x.Code).ToList());
                                }
                                else if (item == "H")
                                {
                                    CodeList.AddRange(ctxTFAT.Master.Where(x => x.OthPostType.Contains("V")).Select(x => x.Code).ToList());
                                }
                                else if (item == "D")
                                {
                                    CodeList.AddRange(ctxTFAT.Master.Where(x => x.OthPostType.Contains("D")).Select(x => x.Code).ToList());
                                }
                                else if (item == "V")
                                {
                                    CodeList.AddRange(ctxTFAT.Master.Where(x => (x.BaseGr == "U" || x.BaseGr == "S") && x.OthPostType.Length == 0).Select(x => x.Code).ToList());
                                }
                            }
                            Model.Code = string.Join(",", CodeList);
                        }

                        if (Model.ARAPReqOnly)
                        {
                            CodeList.AddRange(ctxTFAT.Master.Where(x => x.ARAP == true).Select(x => x.Code).ToList());
                            Model.Code = string.Join(",", CodeList);
                        }
                    }
                }

                var date = Model.Date.Replace("-", "/").Split(':');
                Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

                SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                SqlCommand cmd = new SqlCommand();// ("SPTFAT_ReceivableAnalysis", tfat_conx);
                cmd.Connection = tfat_conx;
                bool Skip = false;
                bool AddPrameter = false;
                if (Model.ViewDataId == "Outstanding Report With LR")
                {
                    ExecuteStoredProc("Drop Table ztmp_zOS");
                    cmd.CommandText = "SPTFAT_ReceivableAnalysisWithLR";
                }
                else if (Model.ViewDataId == "Payment Reminder Letter" || Model.ViewDataId == "Payment Reminder Letter Register" || Model.ViewDataId == "UnAdjust Report" || Model.ViewDataId == "Invoice wise Outstanding" || Model.ViewDataId == "OS Ageing" || Model.ViewDataId == "Party Ageing Summary" || Model.ViewDataId == "New Outstanding Report")
                {
                    if ((Model.ViewDataId == "Payment Reminder Letter" || Model.ViewDataId == "Payment Reminder Letter Register") && Model.MainType != "S")
                    {
                        var ReportHeader = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).FirstOrDefault();
                        ReportHeader.pMerge = "7";
                        ReportHeader.pToMerge = "1,2,3,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31";
                        ctxTFAT.SaveChanges();
                    }
                    else
                    {
                        var ReportHeader = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).FirstOrDefault();
                        ReportHeader.pMerge = "";
                        ReportHeader.pToMerge = "";
                        ctxTFAT.SaveChanges();
                    }
                    ExecuteStoredProc("Drop Table ztmp_zOS");
                    AddPrameter = true;
                    cmd.CommandText = "SPTFAT_ReceivableAnalysis";
                }
                else
                {
                    ExecuteStoredProc("Drop Table ztmp_zOSRef");
                    cmd.CommandText = "SPTFAT_ReceivableWithRefDoc";
                    Skip = true;
                }

                if (Model.ViewDataId != "OS With Reference-2" && Model.ViewDataId != "OS Ageing with Ref")
                {
                    Model.Supress = true;
                }

                var GsetBaseGr = Model.Customer == true ? "D" : ctxTFAT.Master.Where(x => x.Code == Model.Code).Select(x => x.BaseGr).FirstOrDefault();
                ppara07 = "Yes";
                mbasegr = GsetBaseGr;
                //var hfgdfhd = ppara04 == null || ppara04 == "" ? mbranchcode : ppara04;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;
                cmd.Parameters.Add("@mBaseGr", SqlDbType.VarChar).Value = mbasegr == null ? Model.MainType == "SL" ? "D" : "S" : mbasegr;
                cmd.Parameters.Add("@mDate", SqlDbType.Date).Value = Model.ToDate;
                cmd.Parameters.Add("@mDate2", SqlDbType.Date).Value = Model.FromDate;
                cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = ppara04 == null || ppara04 == "" ? mbranchcode : ppara04;
                cmd.Parameters.Add("@mLocations", SqlDbType.VarChar).Value = Fieldoftable("Warehouse", "Code", "Name='" + ppara01 + "'");
                cmd.Parameters.Add("@mConsiderCRPR", SqlDbType.Bit).Value = ppara06 == "Yes" ? true : false;
                cmd.Parameters.Add("@mSuppressZero", SqlDbType.Bit).Value = Model.Supress;
                cmd.Parameters.Add("@mArea", SqlDbType.VarChar).Value = "";// ppara04;
                cmd.Parameters.Add("@mParties", SqlDbType.VarChar).Value = Model.Code == null ? "" : Model.Code;
                cmd.Parameters.Add("@mGrps", SqlDbType.VarChar).Value = Fieldoftable("MasterGroups", "Code", "Name='" + ppara16 + "'");
                cmd.Parameters.Add("@mSalesman", SqlDbType.VarChar).Value = ppara02;//FieldoftableNumber("Salesman", "Code", "Name='" + 
                cmd.Parameters.Add("@mBroker", SqlDbType.VarChar).Value = ppara03;//FieldoftableNumber("Broker", "Code", "Name='" + ppara03 + "'");
                cmd.Parameters.Add("@mCategory", SqlDbType.VarChar).Value = ppara05;
                cmd.Parameters.Add("@mRefTillDate", SqlDbType.Bit).Value = ppara07 == "Yes" ? true : false;
                cmd.Parameters.Add("@mDocString", SqlDbType.VarChar).Value = mDocString;
                cmd.Parameters.Add("@mCustomer", SqlDbType.VarChar).Value = Model.Customer == true ? "1" : "0";
                if (AddPrameter)
                {
                    cmd.Parameters.Add("@mBillSubmission", SqlDbType.VarChar).Value = ppara15 == "Yes" ? true : false;
                }

                tfat_conx.Open();
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                tfat_conx.Close();
                tfat_conx.Dispose();
            }
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        public ActionResult GetGridData(GridOption Model)
        {

            return GetGridReport(Model, "X", mpara, true, 0);
        }

        [HttpPost]
        public ActionResult GetGridData1(GridOption Model)
        {
            //Model.ViewDataId = "OS Ageing with Ref1";
            return GetGridReport(Model, "X", mpara, true, 0);
        }

        public ActionResult GetGridDataExport(GridOption Model)
        {

            return GetGridReport(Model, "X", mpara, true, 0);
        }


        #region TreeView

        public string TreeView(string Branch)
        {
            //string BranchCode = "";

            List<string> BranchList = new List<string>();
            if (String.IsNullOrEmpty(Branch))
            {
                BranchList.Add(mbranchcode);
            }
            else
            {
                BranchList = Branch.Split(',').ToList();
            }



            var mTreeList = ctxTFAT.TfatBranch.Where(x => x.Category != "Area" && x.Code != "G00000").OrderBy(x => x.RECORDKEY).Select(x => new { x.Name, x.Grp, x.Code, x.Category }).ToList();

            if (muserid.Trim().ToUpper() != "SUPER")
            {
                mTreeList = ctxTFAT.TfatBranch.Where(x => x.Category != "Area" && x.Code != "G00000" && x.Users.Contains(muserid)).OrderBy(x => x.RECORDKEY).Select(x => new { x.Name, x.Grp, x.Code, x.Category }).ToList();
            }

            List<NFlatObject> flatObjects2 = new List<NFlatObject>();
            for (int n = 0; n < mTreeList.Count; n++)
            {
                string alias = "";
                NFlatObject abc = new NFlatObject();
                if (mTreeList[n].Category == "Zone")
                {
                    alias = mTreeList[n].Name + "-" + "Z";
                }
                else if (mTreeList[n].Category == "Branch")
                {
                    alias = mTreeList[n].Name + "-" + "B";
                }
                else if (mTreeList[n].Category == "SubBranch")
                {
                    alias = mTreeList[n].Name + "-" + "SB";
                }
                else
                {
                    alias = mTreeList[n].Name + "-" + "HO";
                }

                abc.data = alias;
                abc.Id = mTreeList[n].Code;

                if (BranchList.Contains(abc.Id.Trim()))
                {
                    abc.isSelected = true;
                }

                if (mTreeList[n].Code == mTreeList[n].Grp)
                {
                    abc.ParentId = "0";
                }
                else
                {
                    abc.ParentId = mTreeList[n].Code;
                }
                flatObjects2.Add(abc);
            }
            var recursiveObjects = FillRecursive1(flatObjects2, "0");
            string myjsonmodel = new JavaScriptSerializer().Serialize(recursiveObjects);
            ViewBag.Tree = myjsonmodel;
            return myjsonmodel;

        }

        public string CheckUncheckTree(string Check)
        {
            var mTreeList = ctxTFAT.TfatBranch.Where(x => x.Category != "Area" && x.Code != "G00000").Select(x => new { x.Name, x.Grp, x.Code, x.Category }).ToList();
            string alias = "";
            List<NFlatObject> flatObjects2 = new List<NFlatObject>();
            for (int n = 0; n < mTreeList.Count; n++)
            {
                NFlatObject abc = new NFlatObject();
                if (mTreeList[n].Category == "Zone")
                {
                    alias = mTreeList[n].Name + "-" + "Z";
                }
                else if (mTreeList[n].Category == "Branch")
                {
                    alias = mTreeList[n].Name + "-" + "B";
                }
                else if (mTreeList[n].Category == "SubBranch")
                {
                    alias = mTreeList[n].Name + "-" + "SB";
                }
                else
                {
                    alias = mTreeList[n].Name + "-" + "HO";
                }
                abc.data = alias;
                abc.Id = mTreeList[n].Code;
                if (Check == "Check")
                {
                    abc.isSelected = true;
                }
                if (mTreeList[n].Code == mTreeList[n].Grp)
                {
                    abc.ParentId = "0";
                }
                else
                {
                    abc.ParentId = mTreeList[n].Grp;
                }
                flatObjects2.Add(abc);
            }
            var recursiveObjects = FillRecursive1(flatObjects2, "0");
            string myjsonmodel = new JavaScriptSerializer().Serialize(recursiveObjects);
            ViewBag.Tree = myjsonmodel;
            return myjsonmodel;

        }

        public static List<NRecursiveObject> FillRecursive1(List<NFlatObject> flatObjects, string parentId)
        {
            bool mSelected = false;
            List<NRecursiveObject> recursiveObjects = new List<NRecursiveObject>();
            foreach (var item in flatObjects)
            {
                mSelected = item.isSelected;
                recursiveObjects.Add(new NRecursiveObject
                {
                    data = item.data,
                    id = item.Id,
                    attr = new NFlatTreeAttribute { id = item.Id.ToString(), selected = item.isSelected },
                    //children = FillRecursive1(flatObjects, item.Id)
                });
            }
            return recursiveObjects;
        }

        #endregion

        [HttpPost]
        public ActionResult SetBranchParameters(GridOption Model)
        {
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
                                ppara04 = GetPara[i];
                                break;
                            case 2:
                                ppara09 = GetPara[i];
                                break;
                            case 3:
                                ppara10 = GetPara[i];
                                break;
                            case 4:
                                ppara11 = GetPara[i];
                                break;
                            case 5:
                                ppara12 = GetPara[i];
                                break;
                            case 6:
                                ppara13 = GetPara[i];
                                break;
                            case 7:
                                ppara14 = GetPara[i];
                                break;
                        }
                    }
                }
            }


            if (Model.IsFormatSelected == true)
            {
                ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=0 Where SubCodeOf='" + Model.OptionCode + "'");
                ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=-1 Where Code='" + Model.ViewDataId + "'");
            }
            return Json(new
            {
                Status = "Success",
                Message = ""
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ParameterReset(GridOption Model)
        {
            if (Model.MainType == "SL")
            {
                Model.Accounts = PopulateDebtors();
            }
            else
            {
                Model.Accounts = PopulateCreditors();
            }
            Model.Customers = PopulateCustomers();
            Model.Branch = mbranchcode;
            Model.Age1 = "30";
            Model.Age2 = "60";
            Model.Age3 = "90";
            Model.Age4 = "120";
            Model.Age5 = "150";
            Model.Age6 = "180";


            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = DateTime.Today.ToShortDateString();


            Model.StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

            var CodeList = ctxTFAT.ReportHeader.Where(x => x.SubCodeOf == Model.ViewDataId).Select(x => x.Code).ToList();
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => CodeList.Contains(x.Code)).ToList();
            tfatSearch.ForEach(x => x.IsHidden = false);
            tfatSearch.Where(x => x.ColWidth == 0).ToList().ForEach(z => z.IsHidden = true);
            ctxTFAT.SaveChanges();

            var html = ViewHelper.RenderPartialView(this, "SetParameters", Model);
            var jsonResult = Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        [HttpPost]
        public ActionResult GetParameterAuto(GridOption Model)
        {

            Model.StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

            var MainReportName = Model.ViewDataId;
            if (Model.MainType == "SL")
            {
                Model.Accounts = PopulateDebtors();
            }
            else
            {
                Model.Accounts = PopulateCreditors();
            }
            Model.Customers = PopulateCustomers();


            Model.ReportTypeL = ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower() && x.Para1.Trim().ToLower() == Model.MainType.Trim().ToLower()).Select(x => x.ReportName).FirstOrDefault();
            var Tfatsearch = ctxTFAT.TfatSearch.Where(x => x.Code.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower()).ToList();
            Tfatsearch.ForEach(x => x.IsHidden = false);
            //Model.SaveReportList = PopulateSaveReports(MainReportName);
            ReportParameters mobj = new ReportParameters();
            if (ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower() && x.Para1.Trim().ToLower() == Model.MainType.Trim().ToLower()).FirstOrDefault() != null)
            {
                mobj = ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower() && x.Para1.Trim().ToLower() == Model.MainType.Trim().ToLower()).FirstOrDefault();

                //Model.FromDate = mobj.StartDate.Value.ToShortDateString();
                Model.ToDate = mobj.EndDate.Value.ToShortDateString();
                Model.HideColumnList = mobj.HideColumnList;

                Model.MainType = mobj.Para1;
                Model.Branch = mobj.Para2 == null ? "" : mobj.Para2.Replace("'", "");
                Model.BillDetails = mobj.Para3 == "T" ? true : false;
                Model.CustomerF = mobj.Para4 == "T" ? true : false;
                Model.Code = mobj.Para5 == null ? "" : mobj.Para5.Replace("'", "");
                Model.Age1 = mobj.Para6 == null ? "" : mobj.Para6.Replace("'", "");
                Model.Age2 = mobj.Para7 == null ? "" : mobj.Para7.Replace("'", "");
                Model.Age3 = mobj.Para8 == null ? "" : mobj.Para8.Replace("'", "");
                Model.Age4 = mobj.Para9 == null ? "" : mobj.Para9.Replace("'", "");
                Model.Age5 = mobj.Para10 == null ? "" : mobj.Para10.Replace("'", "");
                Model.Age6 = mobj.Para11 == null ? "" : mobj.Para11.Replace("'", "");
                Model.SundryCreditorsFilterGroups = mobj.Para12 == null ? "" : mobj.Para12;
                Model.ARAPReqOnly = mobj.Para13 == "T" ? true : false;
            }
            if (!String.IsNullOrEmpty(Model.HideColumnList))
            {
                var HiddenColumns = Model.HideColumnList.Split(',').ToList();
                Tfatsearch.Where(x => HiddenColumns.Contains(x.Sno.ToString())).ToList().ForEach(x => x.IsHidden = true);
                ctxTFAT.SaveChanges();
            }


            var html = ViewHelper.RenderPartialView(this, "SetParameters", Model);
            var jsonResult = Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        [HttpPost]
        public ActionResult SaveParameter(GridOption Model)
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
                    if (ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower() && x.Para1.Trim().ToLower() == Model.MainType.Trim().ToLower()).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower() && x.Para1.Trim().ToLower() == Model.MainType.Trim().ToLower()).FirstOrDefault();
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
                    mobj.EndDate = ConvertDDMMYYTOYYMMDD(Model.ToDate);
                    mobj.HideColumnList = HiddenColumn;

                    mobj.Para1 = Model.MainType;
                    mobj.Para2 = Model.Branch;
                    mobj.Para3 = Model.BillDetails == true ? "T" : "F";
                    mobj.Para4 = Model.CustomerF == true ? "T" : "F";
                    mobj.Para5 = Model.Code;
                    mobj.Para6 = Model.Age1;
                    mobj.Para7 = Model.Age2;
                    mobj.Para8 = Model.Age3;
                    mobj.Para9 = Model.Age4;
                    mobj.Para10 = Model.Age5;
                    mobj.Para11 = Model.Age6;
                    mobj.Para12 = Model.SundryCreditorsFilterGroups;
                    mobj.Para13 = Model.ARAPReqOnly == true ? "T" : "F";
                    //mobj.Para2 = Model.Expenses;

                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = "A00";
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
                    //mnewrecordkey = Convert.ToInt32(mobj.RECORDKEY);
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

        public ActionResult DeleteParamete(GridOption Model)
        {
            var mList = ctxTFAT.ReportParameters.Where(x => x.Reports.Trim().ToLower() == Model.ViewDataId.Trim().ToLower() && x.ReportName.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower() && x.Para1.Trim().ToLower() == Model.MainType.Trim().ToLower()).FirstOrDefault();
            ctxTFAT.ReportParameters.Remove(mList);
            ctxTFAT.SaveChanges();

            return Json(new
            {
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }
    }
}