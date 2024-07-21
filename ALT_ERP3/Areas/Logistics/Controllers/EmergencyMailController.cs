using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using ClosedXML.Excel;
using CrystalDecisions.CrystalReports.Engine;
using EntitiModel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class EmergencyMailController : BaseController
    {
        public static string ReportName = "";
        public static string CompanyName = (System.Web.HttpContext.Current.Session["CompName"].ToString() ?? "");
        public static string EmailmStr = "";
        public static string AccountName = "";

        // GET: Logistics/EmergencyMail
        public ActionResult Index(EmergencyMailVM mModel)
        {
            AccountName = "";
            string connstring = GetConnectionString();
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    if (String.IsNullOrEmpty(mModel.Type))
                    {
                        EmailmStr = "";
                        EmailmStr = "<html>";
                        EmailmStr += "<p><span style=\"font-family:Tahoma;font-size:16pt;\"><b>" + "Dear Sir/Madam,<br>Please Find Attached Outstanding Report With This Mail." + "</b></span></p>";
                        EmailmStr += "<br/>";
                        EmailmStr += "</html>";

                        EmialLogHeader = "Auto Send Outstanding Mail";

                        var Outstanding = ctxTFAT.tfatAutoOSMail.Where(x => x.Code == mModel.Document).FirstOrDefault();
                        if (Outstanding != null)
                        {
                            if (String.IsNullOrEmpty(Outstanding.Report))
                            {
                                var FromDate = (Convert.ToDateTime(DateTime.Now.ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                                string Query = "";
                                if (Outstanding.Customer)
                                {
                                    #region Store AccountName
                                    string IntoSingleLine = "";
                                    foreach (var item in Outstanding.Account.Split(',').ToList())
                                    {
                                        IntoSingleLine += "'" + item + "',";
                                    }
                                    if (!String.IsNullOrEmpty(IntoSingleLine))
                                    {
                                        IntoSingleLine = IntoSingleLine.Substring(0, IntoSingleLine.Length - 1);
                                        string mSQLQuery = "select SUBSTRING( ( SELECT ',' + CAST(M.Name as varchar(max)) AS 'data()' from Customermaster M where M.code in ("+ IntoSingleLine + ")FOR XML PATH('') ), 2 , 9999)";
                                        //var connstring = GetConnectionString();
                                        SqlConnection conn = new SqlConnection(connstring);
                                        SqlCommand cmd = new SqlCommand(mSQLQuery, conn);
                                        try
                                        {
                                            conn.Open();
                                            cmd.CommandTimeout = 0;
                                            string CustomerCnt = (string)cmd.ExecuteScalar();
                                            if (!String.IsNullOrEmpty(CustomerCnt))
                                            {
                                                AccountName = CustomerCnt;
                                            }
                                        }
                                        catch (Exception mex)
                                        {
                                        }
                                        finally
                                        {
                                            cmd.Dispose();
                                            conn.Close();
                                            conn.Dispose();
                                        }
                                    }
                                    #endregion
                                    Query = " drop table ztmp_AutoMailData   select c.Code,l.DocDate, l.Branch,l.BillNumber,l.Type,Amt=(Case when (Debit<>0 and ('D'='D' or 'D'='U')) or (Credit<>0 and ('D'='S' or 'D'='U')) then (l.Debit+l.Credit) else 0 End), " +
                                     " Pending = (Case when(Debit <> 0 and('D' = 'D' or 'D' = 'U')) or(Credit <> 0 and('D' = 'S' or 'D' = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + FromDate + "', l.Branch) end) else 0 End), " +
                                     " OnAccount = (Case when(Debit <> 0 and 'D' = 'S' or 'D' = 'U1') or(Credit <> 0 and('D' = 'D' or 'D' = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + FromDate + "', l.Branch) end) else 0 end), " +
                                     " Datediff(d, l.DocDate + l.CrPeriod, Getdate()) as oDueDays " +
                                     " into ztmp_AutoMailData from Ledger l left " +
                                     " join Master m on m.Code = l.Code left " +
                                     " join CustomerMaster c on c.code = l.party left " +
                                     " join CMasterInfo x on l.code = x.code " +
                                     " left " +
                                     " join Address a on a.Code = l.Code and a.Sno = 0 where(m.BaseGr = 'U' or m.BaseGr = 'D') and l.MainType <> 'MV' and l.MainType <> 'PV' and Charindex(l.party,'" + Outstanding.Account + "')<> 0  ";
                                }
                                else
                                {
                                    #region Store AccountName
                                    string IntoSingleLine = "";
                                    foreach (var item in Outstanding.Account.Split(',').ToList())
                                    {
                                        IntoSingleLine += "'" + item + "',";
                                    }
                                    if (!String.IsNullOrEmpty(IntoSingleLine))
                                    {
                                        IntoSingleLine = IntoSingleLine.Substring(0, IntoSingleLine.Length - 1);
                                        string mSQLQuery = "select SUBSTRING( ( SELECT ',' + CAST(M.Name as varchar(max)) AS 'data()' from master M where M.code in (" + IntoSingleLine + ")FOR XML PATH('') ), 2 , 9999)";
                                        //var connstring = GetConnectionString();
                                        SqlConnection conn = new SqlConnection(connstring);
                                        SqlCommand cmd = new SqlCommand(mSQLQuery, conn);
                                        try
                                        {
                                            conn.Open();
                                            cmd.CommandTimeout = 0;
                                            string CustomerCnt = (string)cmd.ExecuteScalar();
                                            if (!String.IsNullOrEmpty(CustomerCnt))
                                            {
                                                AccountName = CustomerCnt;
                                            }
                                        }
                                        catch (Exception mex)
                                        {
                                        }
                                        finally
                                        {
                                            cmd.Dispose();
                                            conn.Close();
                                            conn.Dispose();
                                        }
                                    }
                                    #endregion
                                    Query = " drop table ztmp_AutoMailData   Select l.Code,l.DocDate,l.Branch,l.BillNumber,l.Type, Amt=(Case when (Debit<>0 and ('D'='D' or 'D'='U')) or (Credit<>0 and ('D'='S' or 'D'='U')) then (l.Debit+l.Credit) else 0 End), " +
                                        " Pending = (Case when(Debit <> 0 and('D' = 'D' or 'D' = 'U')) or(Credit <> 0 and('D' = 'S' or 'D' = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + FromDate + "', l.Branch) end) else 0 End), " +
                                        " OnAccount = (Case when(Debit <> 0 and 'D' = 'S' or 'D' = 'U1') or(Credit <> 0 and('D' = 'D' or 'D' = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + FromDate + "', l.Branch) end) else 0 end), " +
                                        " Datediff(d, l.DocDate + l.CrPeriod, Getdate()) as oDueDays " +
                                        " into ztmp_AutoMailData from Ledger l left " +
                                        " join Master m on m.Code = l.Code left " +
                                        " join MasterInfo x on l.code = x.code " +
                                        " left " +
                                        " join Address a on a.Code = l.Code and a.Sno = 0 where(m.BaseGr = 'U' or m.BaseGr = 'D') and l.MainType <> 'MV' and l.MainType <> 'PV' and Charindex(l.code, '" + Outstanding.Account + "')<> 0  ";
                                }
                                ExecuteStoredProc(Query);
                                ReportName = "Outstanding Statement";
                                DataTable Chechdata = new DataTable();
                                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                                {
                                    string sql = string.Format(@"Select * from ztmp_AutoMailData where (Pending - OnAccount) <> 0");
                                    SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                                    da.Fill(Chechdata);
                                }
                                if (Chechdata.Rows.Count > 0)
                                {
                                    var mstream = GetStream(Outstanding.Customer == true ? "true" : "false", FromDate);
                                    SendSendEMail(mstream, Outstanding.EmailTo, CompanyName + " - " + ReportName.ToUpper().Trim(), EmailmStr + "\n\n\n\n:Auto Generated Report ", Outstanding.EmailCC, Outstanding.EmailBCC, true);
                                }
                            }
                            else
                            {
                                string Formatcode = "";
                                var OSReportParameter = ctxTFAT.ReportParameters.Where(x => x.Reports == "Payment Reminder Letter" && x.ReportNameAlias.Trim().ToUpper() == Outstanding.Report.Trim().ToUpper()).FirstOrDefault();
                                if (OSReportParameter != null)
                                {
                                    Formatcode = OSReportParameter.ReportName;
                                    ReportName = OSReportParameter.ReportNameAlias;
                                    GenerateOSTempData(Outstanding.Report, Formatcode, Outstanding.EmailTo, Outstanding.EmailBCC, Outstanding.EmailCC);
                                }
                                else
                                {
                                    return Json(new
                                    {
                                        Message = "Reports Not Found Not Found...",
                                        Status = "Error"
                                    }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                        else
                        {
                            return Json(new
                            {
                                Message = "Data Not Found...",
                                Status = "Error"
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        EmailmStr = "";
                        EmailmStr = "<html>";
                        EmailmStr += "<p><span style=\"font-family:Tahoma;font-size:16pt;\"><b>" + "Dear Sir/Madam,<br>Please Find Attached MIS Report With This Mail." + "</b></span></p>";
                        EmailmStr += "<br/>";
                        EmailmStr += "</html>";

                        if (mModel.Type == "LR000")
                        {
                            EmialLogHeader = "Auto Send Consignment Mail";
                        }
                        else
                        {
                            EmialLogHeader = "Auto Send Freight Memo Mail";
                        }

                        var Consignment = ctxTFAT.tfatAutoConsignmentMail.Where(x => x.Code == mModel.Document).FirstOrDefault();
                        if (Consignment != null)
                        {
                            var GetReportsParameter = ctxTFAT.ReportParameters.Where(x => x.ReportName.Trim().ToUpper() == Consignment.FormatCode.Trim().ToUpper() && x.ReportNameAlias.Trim().ToUpper() == Consignment.Report.Trim().ToUpper()).FirstOrDefault();
                            if (GetReportsParameter!=null)
                            {
                                GenerateTempData(Consignment.Report, Consignment.FormatCode, Consignment.EmailTo, Consignment.EmailBCC, Consignment.EmailCC, Consignment.Type, Consignment.DailyBasis, Consignment.YestDailyBasis);
                            }
                            else
                            {
                                return Json(new
                                {
                                    Message = "Reports Not Found Not Found...",
                                    Status = "Error"
                                }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            return Json(new
                            {
                                Message = "Data Not Found...",
                                Status = "Error"
                            }, JsonRequestBehavior.AllowGet);
                        }


                    }
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, "Mail", mModel.Header, "", DateTime.Now, 0, "", "Send Emergency Mail.", "NA");

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
        public void GenerateTempData(string report, string formatCode, string ToEmail, string BCC, string CC, string Type, bool DailyBasis ,bool YestDailyBasis)
        {
            EmergencyMailVM Model = new EmergencyMailVM();
            Model.ViewDataId = formatCode;
            Model.ReportName = report;
            ReportName = report;
            var MainReportName = Model.ViewDataId;
            Model.ReportTypeL = formatCode;

            ExecuteStoredProc("Update TfatSearch Set IsHidden='false' where upper(Code)=" + Model.ReportTypeL.Trim().ToUpper());

            var GetReportsParameter = ctxTFAT.ReportParameters.Where(x => x.ReportName.Trim().ToUpper() == MainReportName.Trim().ToUpper() && x.ReportNameAlias.Trim().ToUpper() == Model.ReportName.Trim().ToUpper()).FirstOrDefault();
            if (GetReportsParameter != null)
            {
                Model.Type = Type;

                if (Model.Type == "LR000")
                {
                    Model.ConsignmentNo = GetReportsParameter.DocNo;
                    if (DailyBasis)
                    {
                        Model.FromDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString()).ToShortDateString();
                        Model.ToDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString()).ToShortDateString();
                    }
                    else if (YestDailyBasis)
                    {
                        Model.FromDate = ConvertDDMMYYTOYYMMDD((DateTime.Now.AddDays(-1)).ToShortDateString()).ToShortDateString();
                        Model.ToDate = ConvertDDMMYYTOYYMMDD((DateTime.Now.AddDays(-1)).ToShortDateString()).ToShortDateString();
                    }
                    else
                    {
                        Model.FromDate = GetReportsParameter.StartDate == null ? "" : ConvertDDMMYYTOYYMMDD(GetReportsParameter.StartDate.ToString()).ToShortDateString();
                        Model.ToDate = GetReportsParameter.EndDate == null ? "" : ConvertDDMMYYTOYYMMDD(GetReportsParameter.EndDate.ToString()).ToShortDateString();
                    }

                    Model.HideColumnList = GetReportsParameter.HideColumnList;

                    Model.LrMode = GetReportsParameter.Para1;
                    Model.FromBranch = GetReportsParameter.Para2 == null ? "" : GetReportsParameter.Para2;
                    Model.ToBranch = GetReportsParameter.Para3 == null ? "" : GetReportsParameter.Para3;
                    Model.LrType = GetReportsParameter.Para4 == null ? "" : GetReportsParameter.Para4;
                    Model.FromDestination = GetReportsParameter.Para5 == null ? "" : GetReportsParameter.Para5;
                    Model.ToDestination = GetReportsParameter.Para6 == null ? "" : GetReportsParameter.Para6;
                    Model.BillingBranch = GetReportsParameter.Para7 == null ? "" : GetReportsParameter.Para7;
                    Model.Consignor = GetReportsParameter.Para8 == null ? "" : GetReportsParameter.Para8;
                    Model.Consignee = GetReportsParameter.Para9 == null ? "" : GetReportsParameter.Para9;
                    Model.BillingParty = GetReportsParameter.Para10 == null ? "" : GetReportsParameter.Para10;
                    Model.Delivery = GetReportsParameter.Para11 == null ? "" : GetReportsParameter.Para11;
                    Model.Collection = GetReportsParameter.Para12 == null ? "" : GetReportsParameter.Para12;
                    Model.ChargeType = GetReportsParameter.Para13 == null ? "" : GetReportsParameter.Para13;
                    Model.Particular = GetReportsParameter.Para14 == null ? "" : GetReportsParameter.Para14;
                    Model.Unit = GetReportsParameter.Para15 == null ? "" : GetReportsParameter.Para15;
                    Model.LrGenetate = GetReportsParameter.Para16 == null ? "" : GetReportsParameter.Para16;

                    Model.BillRelationDetails = GetReportsParameter.Para17 == "T" ? true : false;
                    Model.DispatchDetails = GetReportsParameter.Para18 == "T" ? true : false;
                    Model.DeliveryDetails = GetReportsParameter.Para19 == "T" ? true : false;
                    Model.ExpensesDetails = GetReportsParameter.Para20 == "T" ? true : false;

                    Model.ChargeShow = GetReportsParameter.Para21 == "T" ? true : false;
                    Model.BillChargeShow = GetReportsParameter.Para22 == "T" ? true : false;
                    Model.StockBranch = GetReportsParameter.Para23 == null ? "" : GetReportsParameter.Para23;
                    Model.StockType = GetReportsParameter.Para24 == null ? "" : GetReportsParameter.Para24;
                    Model.TripDetails = GetReportsParameter.Para25 == "T" ? true : false;
                    Model.AdvBalDetails = GetReportsParameter.Para26 == "T" ? true : false;
                    Model.PLAccount = GetReportsParameter.Para27 == "T" ? true : false;
                    Model.SkipDuplicateFM = GetReportsParameter.Para28 == "T" ? true : false;
                }
                else
                {
                    Model.FreightMemoNo = GetReportsParameter.DocNo;
                    if (DailyBasis)
                    {
                        Model.FromDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString()).ToShortDateString();
                        Model.ToDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString()).ToShortDateString();
                    }
                    else if (YestDailyBasis)
                    {
                        Model.FromDate = ConvertDDMMYYTOYYMMDD((DateTime.Now.AddDays(-1)).ToShortDateString()).ToShortDateString();
                        Model.ToDate = ConvertDDMMYYTOYYMMDD((DateTime.Now.AddDays(-1)).ToShortDateString()).ToShortDateString();
                    }
                    else
                    {
                        Model.FromDate = GetReportsParameter.StartDate == null ? "" : ConvertDDMMYYTOYYMMDD(GetReportsParameter.StartDate.ToString()).ToShortDateString();
                        Model.ToDate = GetReportsParameter.EndDate == null ? "" : ConvertDDMMYYTOYYMMDD(GetReportsParameter.EndDate.ToString()).ToShortDateString();
                    }

                    Model.HideColumnList = GetReportsParameter.HideColumnList;

                    Model.VehicleType = GetReportsParameter.Para1;
                    Model.Branch = GetReportsParameter.Para2 == null ? "" : GetReportsParameter.Para2;
                    Model.LoadFrom = GetReportsParameter.Para3 == null ? "" : GetReportsParameter.Para3;
                    Model.SendTo = GetReportsParameter.Para4 == null ? "" : GetReportsParameter.Para4;

                    Model.Broker = GetReportsParameter.Para5 == null ? "" : GetReportsParameter.Para5;
                    Model.PAyableAt = GetReportsParameter.Para6 == null ? "" : GetReportsParameter.Para6;
                    Model.TruckNo = GetReportsParameter.Para7 == null ? "" : GetReportsParameter.Para7;

                    Model.FMExpensesDetails = GetReportsParameter.Para8 == "T" ? true : false;
                    Model.DispatchDetails = GetReportsParameter.Para9 == "T" ? true : false;
                    Model.LorryReceiptDetails = GetReportsParameter.Para10 == "T" ? true : false;
                    Model.LRExpensesDetails = GetReportsParameter.Para11 == "T" ? true : false;
                    Model.PaymentDetails = GetReportsParameter.Para12 == "T" ? true : false;
                    Model.Advance = GetReportsParameter.Para13 == "T" ? true : false;
                    Model.Balance = GetReportsParameter.Para14 == "T" ? true : false;
                    Model.TripDetails = GetReportsParameter.Para15 == "T" ? true : false;

                    Model.Driver = GetReportsParameter.Para16 == null ? "" : GetReportsParameter.Para16;

                }
                if (Model.Type == "LR000")
                {
                    if (String.IsNullOrEmpty(Model.mWhat))
                    {
                        if (Model.ChargeShow == false && Model.BillChargeShow == false)
                        {
                            Model.ChargeShow = true;
                        }

                        if (String.IsNullOrEmpty(Model.FromBranch))
                        {
                            Model.FromBranch = "";
                        }

                        ExecuteStoredProc("Drop Table ztmp_ConsignmentReport");
                        ExecuteStoredProc("Drop Table TempLRList");
                        SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                        SqlCommand cmd = new SqlCommand("ConsigmentReports", tfat_conx);

                        if (Model.ViewDataId == "LorryReceiptReports")
                        {
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
                        }
                        if (Model.ViewDataId == "LorryReceiptNOLC")
                        {

                            cmd.Parameters.Add("@BillDetails", SqlDbType.Bit).Value = Model.BillRelationDetails;
                            cmd.Parameters.Add("@DispatchDetails", SqlDbType.Bit).Value = Model.DispatchDetails;//add 
                            cmd.Parameters.Add("@AdvBalDetails", SqlDbType.Bit).Value = Model.AdvBalDetails;
                            cmd.Parameters.Add("@DeliveryDetails", SqlDbType.Bit).Value = Model.DeliveryDetails;
                            cmd.Parameters.Add("@ExpensesDetails", SqlDbType.Bit).Value = Model.ExpensesDetails;
                            cmd.Parameters.Add("@TripDetails", SqlDbType.Bit).Value = Model.TripDetails;

                        }
                        if (Model.ViewDataId == "UNBillLorryReceipt")
                        {
                            cmd.Parameters.Add("@BillDetails", SqlDbType.Bit).Value = Model.BillRelationDetails;//add
                            cmd.Parameters.Add("@DispatchDetails", SqlDbType.Bit).Value = Model.DispatchDetails;
                            cmd.Parameters.Add("@AdvBalDetails", SqlDbType.Bit).Value = Model.AdvBalDetails;
                            cmd.Parameters.Add("@DeliveryDetails", SqlDbType.Bit).Value = Model.DeliveryDetails;
                            cmd.Parameters.Add("@ExpensesDetails", SqlDbType.Bit).Value = Model.ExpensesDetails;
                            cmd.Parameters.Add("@TripDetails", SqlDbType.Bit).Value = Model.TripDetails;

                        }
                        if (Model.ViewDataId == "UNDispatchLorryReceipt")
                        {

                            cmd.Parameters.Add("@BillDetails", SqlDbType.Bit).Value = Model.BillRelationDetails;
                            cmd.Parameters.Add("@DispatchDetails", SqlDbType.Bit).Value = Model.DispatchDetails;
                            cmd.Parameters.Add("@AdvBalDetails", SqlDbType.Bit).Value = Model.AdvBalDetails;
                            cmd.Parameters.Add("@DeliveryDetails", SqlDbType.Bit).Value = Model.DeliveryDetails;
                            cmd.Parameters.Add("@ExpensesDetails", SqlDbType.Bit).Value = Model.ExpensesDetails;
                            cmd.Parameters.Add("@TripDetails", SqlDbType.Bit).Value = Model.TripDetails;

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

                        }


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

                        if (Model.ViewDataId == "LorryReceiptStock")
                        {
                            ExecuteStoredProc("delete from ztmp_ConsignmentReport where StockBranchOnly not in (" + Model.StockBranch + ")");
                        }

                        GridOption gridOption = new GridOption();
                        gridOption.ViewDataId = Model.ViewDataId;
                        gridOption.Date = Model.FromDate + ":" + Model.ToDate;

                        GetExcel(gridOption, "EXLS", ToEmail, BCC, CC, Model.HideColumnList);
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(Model.mWhat))
                    {
                        if (Model.VehicleType == "A")
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
                        ExecuteStoredProc("Drop Table ztmp_FreightMemoReports");
                        SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                        SqlCommand cmd = new SqlCommand("SP_FreightMemoReports", tfat_conx);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 120;
                        cmd.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = ConvertDDMMYYTOYYMMDD(Model.FromDate);
                        cmd.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = ConvertDDMMYYTOYYMMDD(Model.ToDate);
                        cmd.Parameters.Add("@ReportType", SqlDbType.VarChar).Value = Model.ReportTypeL;


                        cmd.Parameters.Add("@FMNO", SqlDbType.VarChar).Value = Model.FreightMemoNo ?? "";
                        cmd.Parameters.Add("@Branch", SqlDbType.VarChar).Value = Convert.ToString(Model.Branch);
                        cmd.Parameters.Add("@FromBranch", SqlDbType.VarChar).Value = Convert.ToString(Model.LoadFrom);
                        cmd.Parameters.Add("@ToBranch", SqlDbType.VarChar).Value = Convert.ToString(Model.SendTo);
                        cmd.Parameters.Add("@Broker", SqlDbType.VarChar).Value = Convert.ToString(Model.Broker);
                        cmd.Parameters.Add("@Payable", SqlDbType.VarChar).Value = Convert.ToString(Model.PAyableAt);
                        cmd.Parameters.Add("@TruckNo", SqlDbType.VarChar).Value = Convert.ToString(Model.TruckNo);
                        if (Model.ReportTypeL == "UnPaidFMReport")
                        {
                            cmd.Parameters.Add("@PaymentDetails", SqlDbType.VarChar).Value = true;
                            cmd.Parameters.Add("@Advance", SqlDbType.VarChar).Value = true;
                            cmd.Parameters.Add("@Balance", SqlDbType.VarChar).Value = true;
                        }
                        else
                        {
                            cmd.Parameters.Add("@PaymentDetails", SqlDbType.VarChar).Value = Convert.ToBoolean(Model.PaymentDetails);
                            cmd.Parameters.Add("@Advance", SqlDbType.VarChar).Value = Convert.ToBoolean(Model.Advance);
                            cmd.Parameters.Add("@Balance", SqlDbType.VarChar).Value = Convert.ToBoolean(Model.Balance);
                        }


                        cmd.Parameters.Add("@mVehicleType", SqlDbType.VarChar).Value = Convert.ToString(Model.VehicleType);

                        cmd.Parameters.Add("@FMExpensesDetails", SqlDbType.VarChar).Value = Convert.ToBoolean(Model.FMExpensesDetails);
                        cmd.Parameters.Add("@LRExpensesDetails", SqlDbType.VarChar).Value = Convert.ToBoolean(Model.LRExpensesDetails);
                        cmd.Parameters.Add("@DispatchDetails", SqlDbType.VarChar).Value = Convert.ToBoolean(Model.DispatchDetails);
                        cmd.Parameters.Add("@LorryReceiptDetails", SqlDbType.VarChar).Value = Convert.ToBoolean(Model.LorryReceiptDetails);
                        cmd.Parameters.Add("@TripDetails", SqlDbType.VarChar).Value = Convert.ToBoolean(Model.TripDetails);

                        cmd.Parameters.Add("@Driver", SqlDbType.VarChar).Value = Convert.ToString(Model.Driver);

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

                        GridOption gridOption = new GridOption();
                        gridOption.ViewDataId = Model.ViewDataId;
                        gridOption.Date = Model.FromDate + ":" + Model.ToDate;

                        GetExcel(gridOption, "EXLS", ToEmail, BCC, CC, Model.HideColumnList);
                    }
                }
            }
        }
        public string GetExcel(GridOption Model, string mwhat, string ToEmail, string BCC, string CC, string HideColumnList)
        {
            Model.mWhat = mwhat;
            //PreExecute(Model);
            if (Model.Date == null || Model.Date == "undefined:undefined")
            {
                //Model.FromDate = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                //Model.ToDate = (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            }
            else
            {
                var Date = Model.Date.Split(':');
                if (Date[0] == null || Date[0] == "undefined")
                {
                    //Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"];
                }
                else
                {
                    Model.FromDate = Date[0];
                }
                if (Date[1] == null || Date[1] == "undefined")
                {
                    //Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"];
                }
                else
                {
                    Model.ToDate = Date[1];
                }
                Model.FromDate = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                Model.ToDate = (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

            }
            if (Model.mParaString != null && Model.mParaString != "" && mpara == "")// mpara="" added SDS 25/07/2020 since mpara gets initialised in other tabs
            {
                mpara = Model.mParaString;
            }
            if (Model.SelectContent == null)
            {
                Model.SelectContent = Model.Code;
            }

            ppara02 = NameofAccount(Model.SelectContent);
            ppara23 = ppara02;
            //Model.Code = Model.SelectContent;
            ppara24 = Model.Opening.ToString();
            ppara22 = Model.Closing.ToString();
            string[] mArr = { ppara01, ppara02, ppara03, ppara04, ppara05, ppara06, ppara07, ppara08, ppara09, ppara10, ppara11, ppara12, ppara13, ppara14, ppara15, ppara16, ppara17, ppara18, ppara19, ppara20, ppara21, ppara22, ppara23, ppara24 };

            switch (mwhat)
            {
                case "EXLS":
                    return SendMail(CreateSSRSReportt(HideColumnList, Model, "L", mArr, "Landscape", "Code^" + Model.SelectContent + (mpara != "" ? "~" + mpara : ""), Model.Opening), Model.ViewDataId, ToEmail, BCC, CC);
                default:
                    break;
            }
            return null;
        }
        public System.Data.DataTable CreateSSRSReportt(string HideColumnList, GridOption Model, string mReportType, string[] mparameters, string mpageorient = "Landscape", string mParaString = "", decimal mopening = -1)
        {
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            string Query = "select Top 1 isnull(PrintFormat,'') from ReportHeader where Code='" + Model.ViewDataId + "'";
            SqlCommand CmdCnt = tfat_conx.CreateCommand();
            CmdCnt.CommandText = Query;
            tfat_conx.Open();
            string mPrintFormat = (string)CmdCnt.ExecuteScalar();
            tfat_conx.Close();

            if (String.IsNullOrEmpty(mPrintFormat))
            {
                mPrintFormat = Model.ViewDataId.Replace("/", "").Replace("\\", "").Replace(" ", "");
            }
            else
            {
                mPrintFormat = Model.ViewDataId.Replace("/", "").Replace("\\", "").Replace(" ", "");
            }

            ReportViewer rv = new Microsoft.Reporting.WebForms.ReportViewer
            {
                ProcessingMode = ProcessingMode.Local,
                SizeToReportContent = true,
                ZoomMode = ZoomMode.PageWidth,
                ShowToolBar = true,
                AsyncRendering = true,
                BorderStyle = System.Web.UI.WebControls.BorderStyle.Solid,
                ShowFindControls = true,
                ShowZoomControl = true,
                ShowPrintButton = true
            };
            rv.Reset();

            // get query from reportheader/tfatsearch
            string connstring = GetConnectionString();
            string mFixedPara = "";
            if (Model.Para != null)
            {
                mFixedPara = Model.Para;
            }
            if (mFixedPara != "")
            {
                mFixedPara += "~";
            }
            mParaString = mFixedPara + mParaString;

            string mWhat = Model.mWhat ?? "";
            int startIndex = mWhat == "" ? (Model.page - 1) * Model.rows + 1 : -1;
            int endIndex = mWhat == "" ? (Model.page * Model.rows) : -1;

            string mFilter = "";
            if (Model.searchField != "" && Model.searchField != null && Model.searchField != "null" && Model.searchString != null && Model.searchString != "")
            {
                switch (Model.searchOper)
                {
                    case "eq":
                        mFilter = Model.searchField + " = '" + Model.searchString + "'";
                        break;
                    case "ne":
                        mFilter = Model.searchField + " <> " + Model.searchString;
                        break;
                    case "bw":
                        mFilter = Model.searchField + " like '" + Model.searchString + "%'";
                        break;
                    case "bn":
                        mFilter = Model.searchField + " Not like '" + Model.searchString + "%'";
                        break;
                    case "ew":
                        mFilter = Model.searchField + " like '%" + Model.searchString + "'";
                        break;
                    case "en":
                        mFilter = Model.searchField + " Not like '%" + Model.searchString + "'";
                        break;
                    case "cn":
                        mFilter = Model.searchField + " like '%" + Model.searchString + "%'";
                        break;
                    case "in":
                        mFilter = Model.searchField + " like '%" + Model.searchString + "%'";
                        break;
                    case "nc":
                        mFilter = Model.searchField + " Not like '%" + Model.searchString + "%'";
                        break;
                    case "ni":
                        mFilter = Model.searchField + " Not like '%" + Model.searchString + "%'";
                        break;
                }
            }

            SqlConnection con = new SqlConnection(connstring);
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("dbo.SPTFAT_ExecuteSSRSReport", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@mFormatCode", SqlDbType.VarChar).Value = Model.ViewDataId;
            cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = "HO0000";
            cmd.Parameters.Add("@mUserID", SqlDbType.VarChar).Value = "Super";
            cmd.Parameters.Add("@mAlias", SqlDbType.VarChar).Value = (Model.searchtype ?? "").StartsWith("^S") ? "^" + Model.searchField : ""; // since currently not used, we use it for summarised report flag
            string mstrx = (Model.searchtype ?? "").StartsWith("^S") ? Model.searchField : (Model.sidx != null ? (Model.sidx + ' ' + (Model.sidx.Contains("asc") || Model.sidx.Contains("desc") ? "" : Model.sord)) : "");
            mstrx = CutRightString(mstrx.Trim(), 1, ",");
            cmd.Parameters.Add("@mOrderBy", SqlDbType.VarChar).Value = mstrx;// Model.sidx != null ? (Model.sidx + ' ' + (Model.sidx.Contains("asc") || Model.sidx.Contains("desc") ? "" : Model.sord)) : "";
            if (mReportType == "M")
            {
                cmd.Parameters.Add("@mReportStartDate", SqlDbType.VarChar).Value = "2018-01-01";
                cmd.Parameters.Add("@mReportEndDate", SqlDbType.VarChar).Value = "2019-01-01";
            }
            else
            {
                if (Model.Date != null)
                {
                    Model.Date = Model.Date.Replace("undefined", "01-01-2000");
                    var date = Model.Date.Replace("-", "/").Split(':');
                    Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                }
                else
                {
                    Model.FromDate = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    Model.ToDate = (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                }
                cmd.Parameters.Add("@mReportStartDate", SqlDbType.VarChar).Value = Model.FromDate;
                cmd.Parameters.Add("@mReportEndDate", SqlDbType.VarChar).Value = Model.ToDate;
            }
            cmd.Parameters.Add("@mPara", SqlDbType.VarChar).Value = mParaString;
            cmd.Parameters.Add("@mFilter", SqlDbType.VarChar).Value = mFilter;
            cmd.Parameters.Add("@mDocString", SqlDbType.VarChar).Value = "";
            cmd.CommandTimeout = 0;
            con.Open();
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = cmd;
            //
            string mreportheading = "";

            System.Data.DataTable dt = new System.Data.DataTable();
            da.Fill(dt);
            cmd.Dispose();
            da.Dispose();
            con.Close();
            con.Dispose();

            System.Data.DataTable data = new System.Data.DataTable();
            data = dt.Copy();

            List<int> items = new List<int>();
            foreach (var item in HideColumnList.Split(',').ToList())
            {
                if (!String.IsNullOrEmpty(item))
                {
                    items.Add(Convert.ToInt32(item));
                }

            }
            //using (SqlConnection con1 = new SqlConnection(GetConnectionString()))
            //{
            //    string query = " SELECT Sno FROM TfatSearch where Code='" + Model.ViewDataId + "' and IsHidden='true'";
            //    using (SqlCommand cmd1 = new SqlCommand(query))
            //    {
            //        cmd1.Connection = con1;
            //        con1.Open();
            //        using (SqlDataReader sdr = cmd1.ExecuteReader())
            //        {
            //            while (sdr.Read())
            //            {
            //                items.Add(Convert.ToInt32(sdr["Sno"]));
            //            }
            //        }
            //        con1.Close();
            //    }
            //}

            foreach (var item in items)
            {
                DataColumn dataColumn = data.Columns[item - 1];
                dt.Columns.Remove(dataColumn.ColumnName);
            }

            if (mopening != -1)
            {
                int mbalcol = -1;
                int mruncol = -1;
                int i;
                for (i = 0; i < dt.Columns.Count; i++)
                {
                    string mcolname = dt.Columns[i].ColumnName.Trim().ToLower();
                    if (mcolname == "balancefield")
                    {
                        mbalcol = i;
                    }
                    if (mcolname == "runningbalance" || mcolname == "balance")
                    {
                        mruncol = i;
                    }
                }
                if (mbalcol != -1 && mruncol != -1)
                {
                    decimal mbal = mopening;
                    foreach (DataRow dr in dt.Rows)
                    {
                        mbal = mbal + (decimal)dr[mbalcol];
                        dr[mruncol] = mbal;
                    }
                }
            }
            rv.LocalReport.ReportPath = Server.MapPath("/Reports/REP_" + mPrintFormat + "_" + (mpageorient == "Landscape" ? "L" : "P") + ".rdlc");

            //rv.LocalReport.ReportPath = @"F:\Auto Mail All Prints And SSRS\REP_" + mPrintFormat + "_" + (mpageorient == "Landscape" ? "L" : "P") + ".rdlc";
            ReportDataSource rds = new ReportDataSource("DataSet1", dt); // DataSet1 is defined in .rdlc, 

            //System.Data.DataTable dt2 = GetDataTable(@"Select Code,Name,Addrl1,Addrl2,Addrl3,Addrl4,City,Tel1,Tel2,Tel3,Tel4,TINNumber,VATReg,www,State,aPin,Email,CINNo,GSTNo,PanNo,Country from TfatBranch Where Code='HO0000'");
            //ReportDataSource rds2 = new ReportDataSource("DataSet2", dt2); // DataSet2 is defined in .rdlc, 
            //List<ReportParameter> reportParams = new List<ReportParameter>
            //{
            //    new ReportParameter("muserid", "Super"),
            //    new ReportParameter("mreportheading", mreportheading)
            //};
            //rv.LocalReport.SetParameters(reportParams);
            //rv.LocalReport.DataSources.Clear();
            //rv.LocalReport.DataSources.Add(rds);
            //rv.LocalReport.DataSources.Add(rds2);
            //rv.LocalReport.Refresh();
            //rv.ShowZoomControl = true;
            //rv.ShowPrintButton = true;
            return dt;
        }
        public string SendMail(System.Data.DataTable dataTable, string ViewDataId, string ToEmail, string BCC, string CC)
        {
            if (dataTable.Rows.Count > 0)
            {
                SendSendEMail(ViewDataId, "Excel", dataTable, ToEmail, CompanyName + " - " + ReportName.ToUpper().Trim(), EmailmStr + "\n\n\n\n:Auto Generated Report", CC, BCC, true);

            }
            return null;
        }
        public void SendSendEMail(string Formatcode, string TypeEx, System.Data.DataTable dataTable, string mEmail, string mSubject, string mMsg, string mCC = "", string mBCC = "", bool GlbalMail = false)
        {
            int mid = 0;
            try
            {
                string msmtppassword = "";
                string msmtphost = "";
                int msmtpport = 25;
                string msmtpuser = "";
                string mFromEmail = "";
                mEmail = mEmail.Trim();
                mCC = mCC;
                mBCC = mBCC;

                DataTable dt = new DataTable();
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string sql = string.Format(@"Select top 1 * from TfatComp");
                    SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                    da.Fill(dt);
                }
                //var lst = dt.AsEnumerable().ToList();  
                var lst = dt.AsEnumerable()
                             .Select(r => r.Table.Columns.Cast<DataColumn>()
                             .Select(c => new KeyValuePair<string, object>(c.ColumnName, r[c.Ordinal])
                          ).ToDictionary(z => z.Key, z => z.Value)
                       ).ToList();


                foreach (var item in lst)
                {
                    mCC = (mCC != "" ? mCC + "," : "");
                    mCC += item["CCTo"] == null ? "" : item["CCTo"].ToString().Trim();
                    mBCC = (mBCC != "" ? mBCC + "," : "");
                    mBCC += item["BCCTo"] == null ? "" : item["BCCTo"].ToString().Trim();
                    msmtpuser = item["SMTPUser"] == null ? "" : item["SMTPUser"].ToString().Trim();
                    msmtppassword = item["SMTPPassword"] == null ? "" : item["SMTPPassword"].ToString().Trim();
                    msmtphost = item["SMTPServer"] == null ? "" : item["SMTPServer"].ToString().Trim();
                    msmtpport = item["SMTPPort"] == null ? 25 : Convert.ToInt32(item["SMTPPort"]);
                    mFromEmail = (item["SMTPUser"] ?? "").ToString().Trim();
                }

                mCC = CutRightString(mCC, 1, ",");
                mBCC = CutRightString(mBCC, 1, ",");

                if (msmtpport != 587)
                {
                    msmtpport = 587;
                }
                MailMessage message = new MailMessage();

                using (XLWorkbook wb = new XLWorkbook())
                {
                    var worksheet = wb.Worksheets.Add(dataTable, Formatcode);
                    worksheet.CellsUsed().Style.Border.BottomBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    worksheet.CellsUsed().Style.Border.TopBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    worksheet.CellsUsed().Style.Border.LeftBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    worksheet.CellsUsed().Style.Border.RightBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    //worksheet.CellsUsed().Style.Border.TopBorderColor = ClosedXML.Excel.XLColor.Red;
                    //worksheet.AutoFilter.IsEnabled = false;
                    worksheet.Tables.FirstOrDefault().ShowAutoFilter = false;
                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        //Convert MemoryStream to Byte array.
                        byte[] bytes1 = stream.ToArray();
                        TypeEx = "xlsx";
                        System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(new MemoryStream(bytes1), ReportName.ToUpper() + "." + TypeEx);
                        message.Attachments.Add(attachment);
                        //return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Grid.xlsx");
                    }
                }

                message.From = new MailAddress(mFromEmail);
                mEmail = CutRightString(mEmail, 1, ";");
                mEmail = CutRightString(mEmail, 1, ",");
                if (mEmail != "")
                {
                    string[] ccid = mEmail.Split(',');
                    foreach (var item in ccid)
                    {
                        message.To.Add(new MailAddress(item.Trim()));
                    }
                }
                if (mCC != "")
                {
                    string[] ccid = mCC.Split(',');
                    foreach (var item in ccid)
                    {
                        message.CC.Add(new MailAddress(item.Trim()));
                    }
                }
                if (mBCC != "")
                {
                    string[] bccid = mBCC.Split(',');
                    foreach (var item in bccid)
                    {
                        message.Bcc.Add(new MailAddress(item.Trim()));
                    }
                }
                message.Subject = mSubject;
                message.IsBodyHtml = true;
                message.BodyEncoding = System.Text.Encoding.UTF8;
                mMsg = mMsg.Replace("^~|", "<br>");
                if (mMsg.Contains("<html>") == false)
                {
                    mMsg = TextToHtml(mMsg);
                }
                message.Body = mMsg;
                message.Priority = MailPriority.High;
                message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = msmtphost;
                smtp.Port = msmtpport;
                smtp.Credentials = new System.Net.NetworkCredential(msmtpuser, msmtppassword);

                smtp.EnableSsl = true;


                smtp.Send(message);
                mid = SaveEmailLog(mEmail, mCC, mBCC, mSubject, mMsg, "", "", ReportName, "", EmialLogHeader, EmialLogAutoRemark,"",AccountName);

                //return Json(new { Status = "Success", Message = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (SmtpFailedRecipientException smtex)
            {
                ExecuteStoredProc("Update Emaillog Set sentStatus=0 where RecordKey=" + mid);
                //return Json(new { Status = "Error", Message = smtex.InnerException }, JsonRequestBehavior.AllowGet);
            }
        }
        public string TextToHtml(string text)
        {
            //text = HttpUtility.HtmlEncode(text);
            text = text.Replace("\r\n", "\r");
            text = text.Replace("\n", "\r");
            text = text.Replace("\r", "<br>\r\n");
            text = text.Replace("  ", " &nbsp;");
            string mstr = "<html>";
            if (text.Contains("^b"))
            {
                text = text.Replace("^b", "<strong>").Replace("^eb", "</strong>");
            }
            mstr += text + "</html>";
            return mstr;
        }



        public byte[] GetStream(string CustomerOrNot, string CurrDate)
        {
            Stream streamR;
            Document document = new Document(PageSize.A4, 10, 10, 10, 10);
            var ms = new MemoryStream();
            {
                PdfCopy pdf = new PdfCopy(document, ms);
                document.Open();
                pdf.Open();
                string Query = " select  " +
                               " (select isnull(C.Name, '') from TfatComp C where Code = (select T.CompCode from TfatBranch T where T.Code = 'HO0000')) as CompanyName , " +
                               " (select isnull(C.PAN, '') from TfatComp C where Code = (select T.CompCode from TfatBranch T where T.Code = 'HO0000')) as CompanyPAN , " +
                               " (select isnull(C.GSTNo, '') from TfatComp C where Code = (select T.CompCode from TfatBranch T where T.Code = 'HO0000')) as CompanyGST, " +
                               " (select  isnull(C.Adrl1, '') + ' ' + isnull(C.Adrl2, '') + ' ' + isnull(C.Adrl3, '') + ' ' + isnull(C.Adrl4, '') + isnull(C.City, '') + '-'++isnull(C.Pin, '') + '. ' + isnull(C.State, '') from TfatComp C where Code = (select T.CompCode from TfatBranch T where T.Code = 'HO0000')) as CompanyAddress, " +
                               " (select  ISNULL(C.Tel1, '') + ISNULL(C.Tel2, '') + ISNULL(C.Tel3, '') + ISNULL(C.Tel4, '')  from TfatComp C where Code = (select T.CompCode from TfatBranch T where T.Code = 'HO0000')) as CompanyContact, " +
                               " (select isnull(C.Email, '') from TfatComp C where Code = (select T.CompCode from TfatBranch T where T.Code = 'HO0000')) as CompanyEmail, " +
                               " (select isnull(C.www, '')  from TfatComp C where Code = (select T.CompCode from TfatBranch T where T.Code = 'HO0000')) as CompanyWebsite, " +
                               "  convert(varchar, '" + CurrDate + "', 103) as DocDate, " +
                               " case when '" + CustomerOrNot + "' = 'true' then(select isnull(C.Name, '') from CustomerMaster C where C.code = V.code ) else (select isnull(C.Name, '') from Master C where C.code = V.code )  end as CustomerName,  " +
                               " case when '" + CustomerOrNot + "' = 'true' then(select isnull(C.Designation, '') from CustomerMaster C where C.code = V.code ) else (select isnull(C.Designation, '') from Master C where C.code = V.code )  end as CustomerDesignation, " +
                               " case when '" + CustomerOrNot + "' = 'true' then(select top 1 isnull(C.Adrl1, '') + ' ' + isnull(C.Adrl2, '') + ' ' + isnull(C.Adrl3, '') + ' ' + isnull(C.Adrl4, '')  from Caddress C where C.code = V.code )  else (select top 1 isnull(C.Adrl1, '') + ' ' + isnull(C.Adrl2, '') + ' ' + isnull(C.Adrl3, '') + ' ' + isnull(C.Adrl4, '')  from address C where C.code = V.code ) end as Address, " +
                               " case when '" + CustomerOrNot + "' = 'true' then(select top 1  isnull(C.Mobile, '') from Caddress C where C.code = V.code) else (select top 1 isnull(C.Mobile, '') from address C where C.code = V.code) end as CustomerMobile, " +
                               " case when '" + CustomerOrNot + "' = 'true' then(select top 1  case when   len(C.Tel1) > 0 then  ',' + C.Tel1 else '' end     from Caddress C where C.code = V.code ) else (select top 1 case when len(C.Tel1) > 0 then  ',' + C.Tel1 else '' end from address C where C.code = V.code ) end as CustomerTelephone, " +
                               " case when '" + CustomerOrNot + "' = 'true' then(select top 1 ISNULL(C.state, '')  from Caddress C where C.code = V.code ) else (select top 1 ISNULL(C.state, '')  from address C where C.code = V.code )  end as State, " +
                               " case when '" + CustomerOrNot + "' = 'true' then(select top 1 ISNULL(C.City, '')  from Caddress C where C.code = V.code) else (select  top 1 ISNULL(C.City, '')  from address C where C.code = V.code) end as City, " +
                               " case when '" + CustomerOrNot + "' = 'true' then(select top 1 ISNULL(C.Pin, '')  from Caddress C where C.code = V.code ) else (select top 1 ISNULL(C.Pin, '')  from address C where C.code = V.code ) end as PinCode, " +
                               " case when '" + CustomerOrNot + "' = 'true' then(select top 1 ISNULL(C.Person, '')  from Caddress C where C.code = V.code ) else (select top 1 ISNULL(C.Person, '')  from address C where C.code = V.code )  end as ContactPerson, " +
                               " V.DocDate as InvDate, (select T.Name From tfatbranch T where T.code = V.Branch) as InvBranch,(V.BillNumber + ' ( ' + SUBSTRING(V.Type, 1, 3) + ' )') as InvNo,V.Amt as InvAmt, (cast((V.Pending - V.OnAccount) as money)) as BalAmt,V.oDueDays as DueDays " +
                               " from ztmp_AutoMailData V where (V.Pending - V.OnAccount) <> 0 " +
                               " order by V.DocDate ";
                DataTable dtreport = new DataTable();
                SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                SqlCommand cmd = new SqlCommand(Query, tfat_conx); //name of the storedprocedure
                SqlDataAdapter adp = new SqlDataAdapter(cmd);
                adp.Fill(dtreport);
                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "REP_Payment Reminder Letter Register" + ".rpt"));
                rd.SetDataSource(dtreport);
                try
                {
                    Stream mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                    var mWhat = "PDF";
                    switch (mWhat)
                    {
                        case "PDF":
                            mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                            break;
                        case "XLS":
                            mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.Excel);
                            break;
                        case "WORD":
                            mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.WordForWindows);
                            break;
                        case "CSV":
                            mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.CharacterSeparatedValues);
                            break;
                    }
                    mstream.Seek(0, SeekOrigin.Begin);
                    Warning[] warnings;
                    string[] streamids;
                    string mimeType;
                    string encoding;
                    string extension;
                    MemoryStream memory1 = new MemoryStream();
                    mstream.CopyTo(memory1);
                    byte[] bytes = memory1.ToArray();
                    MemoryStream memoryStream = new MemoryStream(bytes);
                    PdfReader imageDocumentReader = new PdfReader(memoryStream.ToArray());
                    int ab = imageDocumentReader.NumberOfPages;
                    for (int a = 1; a <= ab; a++)
                    {
                        var page = pdf.GetImportedPage(imageDocumentReader, a);
                        pdf.AddPage(page);
                    }
                    imageDocumentReader.Close();

                }
                catch
                {
                    rd.Close();
                    rd.Dispose();
                    throw;
                }
                finally
                {
                    rd.Close();
                    rd.Dispose();
                }
                document.Close();
            }
            return ms.ToArray();
        }
        public void SendSendEMail(byte[] bytes, string mEmail, string mSubject, string mMsg, string mCC = "", string mBCC = "", bool GlbalMail = false)
        {
            int mid = 0;
            try
            {
                string msmtppassword = "";
                string msmtphost = "";
                int msmtpport = 25;
                string msmtpuser = "";
                string mFromEmail = "";
                mEmail = mEmail.Trim();
                mCC = mCC;
                mBCC = mBCC;

                TfatComp tfatComp = ctxTFAT.TfatComp.FirstOrDefault();
                if (tfatComp != null)
                {
                    mCC = (mCC != "" ? mCC + "," : "");
                    mCC += tfatComp.CCTo == null ? "" : tfatComp.CCTo.Trim();
                    mBCC = (mBCC != "" ? mBCC + "," : "");
                    mBCC += tfatComp.BCCTo == null ? "" : tfatComp.BCCTo.Trim();
                    msmtpuser = tfatComp.SMTPUser == null ? "" : tfatComp.SMTPUser.Trim();
                    msmtppassword = tfatComp.SMTPPassword == null ? "" : tfatComp.SMTPPassword.Trim();
                    msmtphost = tfatComp.SMTPServer == null ? "" : tfatComp.SMTPServer.Trim();
                    msmtpport = tfatComp.SMTPPort == null ? 25 : Convert.ToInt32(tfatComp.SMTPPort);
                    mFromEmail = (tfatComp.Email ?? "").Trim();
                }


                mCC = CutRightString(mCC, 1, ",");
                mBCC = CutRightString(mBCC, 1, ",");

                if (msmtpport != 587)
                {
                    msmtpport = 587;
                }
                MailMessage message = new MailMessage();

                MemoryStream memoryStream = new MemoryStream(bytes);
                memoryStream.Seek(0, SeekOrigin.Begin);

                System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(memoryStream, ReportName + ".pdf");
                message.Attachments.Add(attachment);



                message.From = new MailAddress(mFromEmail);
                mEmail = CutRightString(mEmail, 1, ";");
                mEmail = CutRightString(mEmail, 1, ",");
                //message.To.Add(mEmail);

                if (mEmail != "")
                {
                    string[] ccid = mEmail.Split(',');
                    foreach (var item in ccid)
                    {
                        message.To.Add(new MailAddress(item.Trim()));
                    }
                }
                if (mCC != "")
                {
                    string[] ccid = mCC.Split(',');
                    foreach (var item in ccid)
                    {
                        message.CC.Add(new MailAddress(item.Trim()));
                    }
                }
                if (mBCC != "")
                {
                    string[] bccid = mBCC.Split(',');
                    foreach (var item in bccid)
                    {
                        message.Bcc.Add(new MailAddress(item.Trim()));
                    }
                }
                message.Subject = mSubject;
                message.IsBodyHtml = true;
                message.BodyEncoding = System.Text.Encoding.UTF8;
                mMsg = mMsg.Replace("^~|", "<br>");
                if (mMsg.Contains("<html>") == false)
                {
                    mMsg = TextToHtml(mMsg);
                }
                message.Body = mMsg;
                message.Priority = MailPriority.High;
                message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = msmtphost;
                smtp.Port = msmtpport;
                smtp.Credentials = new System.Net.NetworkCredential(msmtpuser, msmtppassword);
                smtp.EnableSsl = true;

                smtp.Send(message);
                mid = SaveEmailLog(mEmail, mCC, mBCC, mSubject, mMsg, "", "", ReportName, "", EmialLogHeader, EmialLogAutoRemark,"",AccountName);

            }
            catch (SmtpFailedRecipientException smtex)
            {
                ExecuteStoredProc("Update Emaillog Set sentStatus=0 where RecordKey=" + mid);
            }
        }
        public void GenerateOSTempData(string report, string formatCode, string ToEmail, string BCC, string CC)
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
            mpara = "";

            GridOption Model = new GridOption();
            Model.ViewDataId = formatCode;
            Model.ReportName = report;
            ReportName = report;
            var MainReportName = Model.ViewDataId;
            Model.ReportTypeL = formatCode;

            ExecuteStoredProc("Update TfatSearch Set IsHidden='false' where upper(Code)=" + Model.ReportTypeL.Trim().ToUpper());

            var ReportParameter = ctxTFAT.ReportParameters.Where(x => x.ReportName.Trim().ToUpper() == MainReportName.Trim().ToUpper() && x.ReportNameAlias.Trim().ToUpper() == Model.ReportName.Trim().ToUpper()).FirstOrDefault();
            if (ReportParameter != null)
            {
                Model.FromDate = ReportParameter.StartDate.ToString() == "" ? "" : ConvertDDMMYYTOYYMMDD(ReportParameter.StartDate.ToString()).ToShortDateString();
                Model.ToDate = ReportParameter.EndDate == null ? "" : ConvertDDMMYYTOYYMMDD(ReportParameter.EndDate.ToString()).ToShortDateString();
                Model.HideColumnList = ReportParameter.HideColumnList;

                Model.MainType = ReportParameter.Para1;
                Model.Branch = ReportParameter.Para2 == null ? "" : ReportParameter.Para2.Replace("'", "");
                Model.BillDetails = ReportParameter.Para3 == "T" ? true : false;
                Model.CustomerF = ReportParameter.Para4 == "T" ? true : false;
                Model.Code = ReportParameter.Para5 == null ? "" : ReportParameter.Para5.Replace("'", "");
                Model.Age1 = ReportParameter.Para6 == null ? "" : ReportParameter.Para6.Replace("'", "");
                Model.Age2 = ReportParameter.Para7 == null ? "" : ReportParameter.Para7.Replace("'", "");
                Model.Age3 = ReportParameter.Para8 == null ? "" : ReportParameter.Para8.Replace("'", "");
                Model.Age4 = ReportParameter.Para9 == null ? "" : ReportParameter.Para9.Replace("'", "");
                Model.Age5 = ReportParameter.Para10 == null ? "" : ReportParameter.Para10.Replace("'", "");
                Model.Age6 = ReportParameter.Para11 == null ? "" : ReportParameter.Para11.Replace("'", "");

                ppara04 = Model.Branch;
                ppara09 = Model.Age1;
                ppara10 = Model.Age2;
                ppara11 = Model.Age3;
                ppara12 = Model.Age4;
                ppara13 = Model.Age5;
                ppara14 = Model.Age6;
                ppara15 = Model.BillDetails == true ? "Yes" : "No";
                if (!String.IsNullOrEmpty(Model.HideColumnList))
                {
                    ExecuteStoredProc("Update TfatSearch Set IsHidden='true' where upper(Code)='" + Model.ReportTypeL.Trim().ToUpper() + "' and Sno in (" + Model.HideColumnList + ")");
                }

                if (String.IsNullOrEmpty(Model.mWhat))
                {
                    if (ppara15 == "Yes")
                    {
                        ExecuteStoredProc("Update TfatSearch Set IsHidden='false' where upper(Code)='" + Model.ReportTypeL.Trim().ToUpper() + "' and ColHead = 'Submission Date' ");
                        ExecuteStoredProc("Update TfatSearch Set IsHidden='false' where upper(Code)='" + Model.ReportTypeL.Trim().ToUpper() + "' and ColHead = 'Bill Reamrk' ");
                        ExecuteStoredProc("Update TfatSearch Set IsHidden='false' where upper(Code)='" + Model.ReportTypeL.Trim().ToUpper() + "' and ColHead = 'Through' ");
                    }
                    else
                    {
                        ExecuteStoredProc("Update TfatSearch Set IsHidden='true' where upper(Code)='" + Model.ReportTypeL.Trim().ToUpper() + "' and ColHead = 'Submission Date' ");
                        ExecuteStoredProc("Update TfatSearch Set IsHidden='true' where upper(Code)='" + Model.ReportTypeL.Trim().ToUpper() + "' and ColHead = 'Bill Reamrk' ");
                        ExecuteStoredProc("Update TfatSearch Set IsHidden='true' where upper(Code)='" + Model.ReportTypeL.Trim().ToUpper() + "' and ColHead = 'Through' ");
                    }
                    if (Model.MainType == "S")
                    {
                        ExecuteStoredProc("Update TfatSearch Set IsHidden='true' where upper(Code)='" + Model.ReportTypeL.Trim().ToUpper() + "' and ColHead = 'Customer' ");
                    }
                    else
                    {
                        ExecuteStoredProc("Update TfatSearch Set IsHidden='false' where upper(Code)='" + Model.ReportTypeL.Trim().ToUpper() + "' and ColHead = 'Customer' ");
                    }

                    Model.ToDate = (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    if (String.IsNullOrEmpty(Model.FromDate))
                    {
                        DateTime dateValue = new DateTime(1900, 1, 1);
                        Model.FromDate = (Convert.ToDateTime(dateValue.ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    }


                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand();// ("SPTFAT_ReceivableAnalysis", tfat_conx);
                    cmd.Connection = tfat_conx;
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
                            ExecuteStoredProc("Update ReportHeader Set pMerge='7',pToMerge='1,2,3,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31' where upper(Code)='" + Model.ViewDataId.Trim().ToUpper() + "'");
                        }
                        else
                        {
                            ExecuteStoredProc("Update ReportHeader Set pMerge='',pToMerge='' where upper(Code)='" + Model.ViewDataId.Trim().ToUpper() + "'");
                        }
                        ExecuteStoredProc("Drop Table ztmp_zOS");
                        AddPrameter = true;
                        cmd.CommandText = "SPTFAT_ReceivableAnalysis";
                    }
                    else
                    {
                        ExecuteStoredProc("Drop Table ztmp_zOSRef");
                        cmd.CommandText = "SPTFAT_ReceivableWithRefDoc";
                    }

                    var GsetBaseGr = "D";
                    ppara07 = "Yes";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.Add("@mBaseGr", SqlDbType.VarChar).Value = "D";
                    cmd.Parameters.Add("@mDate", SqlDbType.Date).Value = Model.ToDate;
                    cmd.Parameters.Add("@mDate2", SqlDbType.Date).Value = Model.FromDate;
                    cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = ppara04;
                    cmd.Parameters.Add("@mLocations", SqlDbType.VarChar).Value = Fieldoftable("Warehouse", "Code", "Name='" + ppara01 + "'");
                    cmd.Parameters.Add("@mConsiderCRPR", SqlDbType.Bit).Value = ppara06 == "Yes" ? true : false;
                    cmd.Parameters.Add("@mSuppressZero", SqlDbType.Bit).Value = ppara08 == "Yes" ? true : false;
                    cmd.Parameters.Add("@mArea", SqlDbType.VarChar).Value = "";// ppara04;
                    cmd.Parameters.Add("@mParties", SqlDbType.VarChar).Value = Model.Code == null ? "" : Model.Code;
                    cmd.Parameters.Add("@mGrps", SqlDbType.VarChar).Value = Fieldoftable("MasterGroups", "Code", "Name='" + ppara16 + "'");
                    cmd.Parameters.Add("@mSalesman", SqlDbType.VarChar).Value = ppara02;//FieldoftableNumber("Salesman", "Code", "Name='" + 
                    cmd.Parameters.Add("@mBroker", SqlDbType.VarChar).Value = ppara03;//FieldoftableNumber("Broker", "Code", "Name='" + ppara03 + "'");
                    cmd.Parameters.Add("@mCategory", SqlDbType.VarChar).Value = ppara05;
                    cmd.Parameters.Add("@mRefTillDate", SqlDbType.Bit).Value = ppara07 == "Yes" ? true : false;
                    cmd.Parameters.Add("@mDocString", SqlDbType.VarChar).Value = "";
                    cmd.Parameters.Add("@mCustomer", SqlDbType.VarChar).Value = Model.CustomerF == true ? "1" : "0";
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

                    GridOption gridOption = new GridOption();
                    gridOption.ViewDataId = Model.ViewDataId;
                    gridOption.Date = Model.FromDate + ":" + Model.ToDate;

                    GetExcel(gridOption, "EXLS", ToEmail, BCC, CC, Model.HideColumnList);
                }
            }
        }
    }
}