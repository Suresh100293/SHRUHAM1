using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using CrystalDecisions.CrystalReports.Engine;
using EntitiModel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
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
    public class AutoOutstandingMailController : BaseController
    {
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;

        #region Functions

        public ActionResult PopulateSaveReports()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT ReportNameAlias FROM ReportParameters where Reports='Payment Reminder Letter' order by ReportNameAlias ";
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
                                Value = sdr["ReportNameAlias"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return Json(items, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetEmailID(string Codes, bool Customer)
        {
            string message = "";
            var Array = Codes.Split(',').ToList();
            if (Customer)
            {
                var List = ctxTFAT.Caddress.Where(x => Array.Contains(x.Code)).Select(x => x.Email).ToList();
                foreach (var item in List)
                {
                    if (!String.IsNullOrEmpty(item))
                    {
                        message += item + ",";
                    }
                }
            }
            else
            {
                var List = ctxTFAT.Address.Where(x => Array.Contains(x.Code)).Select(x => x.Email).ToList();
                foreach (var item in List)
                {
                    if (!String.IsNullOrEmpty(item))
                    {
                        message += item + ",";
                    }
                }
            }

            if (!String.IsNullOrEmpty(message))
            {
                message = message.Substring(0, message.Length - 1);
            }

            return Json(new { EMail = message, JsonRequestBehavior.AllowGet });
        }

        public IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }

        #endregion

        // GET: Logistics/AutoOutstandingMail
        public ActionResult Index(AutoOutstandingMailVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");

            mdocument = mModel.Document;

            //mModel.Branches = PopulateBranches();

            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.tfatAutoOSMail.Where(x => x.Code == mModel.Document).FirstOrDefault();
                if (mList != null)
                {
                    mModel.Code = mList.Code;
                    mModel.Customer = mList.Customer;
                    mModel.Account = mList.Account;
                    mModel.Active = mList.Active;
                    mModel.EmailTo = mList.EmailTo;
                    mModel.EmailCC = mList.EmailCC;
                    mModel.EmailBCC = mList.EmailBCC;
                    mModel.Time = mList.Time;
                    mModel.EveryDay = mList.EveryDay;
                    mModel.Day = mList.Day;
                    mModel.DayName = mList.DayName;
                    mModel.DateR = mList.DateR;
                    mModel.ReportName = mList.Report;
                    mModel.UptoDate = mList.UptoDate.ToShortDateString();
                    if (mList.DateR)
                    {
                        mModel.Date = mList.Date.Value.ToShortDateString();
                    }
                }
            }

            mModel.Customers = PopulateCustomers();
            mModel.Debtors = PopulateDebtors();
            if (mModel.Mode == "Add")
            {
                mModel.Active = true;
                mModel.Customer = true;
                mModel.EveryDay = true;
                mModel.Time = DateTime.Now.ToString("HH:mm");
                mModel.Date = DateTime.Now.ToShortDateString();
                mModel.UptoDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            }
            return View(mModel);
        }

        #region SaveData

        public ActionResult SaveData(AutoOutstandingMailVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        var MSG = DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return MSG;
                    }
                    tfatAutoOSMail mobj = new tfatAutoOSMail();
                    bool mAdd = true;
                    if (ctxTFAT.tfatAutoOSMail.Where(x => (x.Code == mModel.Document)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.tfatAutoOSMail.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                        mAdd = false;
                    }

                    //mobj.ShortName = mModel.ShortName;
                    mobj.Report = mModel.ReportName;
                    mobj.Customer = mModel.Customer;
                    mobj.Account = mModel.Account;
                    mobj.Active = mModel.Active;
                    mobj.EmailTo = mModel.EmailTo;
                    mobj.EmailCC = mModel.EmailCC;
                    mobj.EmailBCC = mModel.EmailBCC;
                    mobj.Time = mModel.Time;
                    mobj.EveryDay = mModel.EveryDay;
                    mobj.Day = mModel.Day;
                    mobj.DayName = mModel.DayName;
                    mobj.DateR = mModel.DateR;
                    if (mModel.DateR)
                    {
                        mobj.Date = ConvertDDMMYYTOYYMMDD(mModel.Date);
                    }
                    mobj.UptoDate = ConvertDDMMYYTOYYMMDD(mModel.UptoDate);
                    if (mobj.NextHitDate <= ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString()))
                    {
                        if (mobj.EveryDay)
                        {
                            mobj.NextHitDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                        }
                        else if (mobj.DateR)
                        {
                            var Day = mobj.NextHitDate.Day;
                            var Month = DateTime.Now.Month;
                            var Year = DateTime.Now.Year;

                            mobj.NextHitDate = ConvertDDMMYYTOYYMMDD(Day + "/" + Month + "/" + Year);
                        }
                        else if (mobj.Day)
                        {
                            var Today = DateTime.Now;
                            while (Today.DayOfWeek.ToString() != mobj.DayName)
                            {
                                Today = Today.AddDays(1);
                            }
                            mobj.NextHitDate = ConvertDDMMYYTOYYMMDD(Today.ToShortDateString());
                        }
                    }
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                    if (mAdd == true)
                    {
                        int NewCode1;
                        var NewCode = ctxTFAT.tfatAutoOSMail.OrderByDescending(x => x.Code).Select(x => x.Code).Take(1).FirstOrDefault();
                        if (String.IsNullOrEmpty(NewCode))
                        {
                            NewCode1 = 100000;
                        }
                        else
                        {
                            NewCode1 = Convert.ToInt32(NewCode) + 1;
                        }
                        string FinalCode = NewCode1.ToString("D6");
                        mobj.SendMail = false;
                        mobj.Code = FinalCode;
                        mobj.CreateDate = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                        ctxTFAT.tfatAutoOSMail.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }

                    //Set Schedule Data
                    SetSchedule(mobj);

                    ctxTFAT.SaveChanges();
                    mnewrecordkey = Convert.ToInt32(mobj.Code);
                    string mNewCode = "";
                    mNewCode = mobj.Code;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mNewCode, DateTime.Now, 0, "", "Save Send Outstanding Mail", "NA");

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
        public void SetSchedule(tfatAutoOSMail mobj)
        {
            var OldList = ctxTFAT.ReportScheduleDate.Where(x => x.Code == mobj.Code && x.Type == "OS000" && x.Status == "X").ToList();
            ctxTFAT.ReportScheduleDate.RemoveRange(OldList);
            if (mobj.Active)
            {
                foreach (DateTime day in EachDay(mobj.NextHitDate, mobj.UptoDate))
                {
                    ReportScheduleDate reportSchedule = new ReportScheduleDate();
                    reportSchedule.Code = mobj.Code;
                    reportSchedule.Type = "OS000";
                    reportSchedule.SchDate = ConvertDDMMYYTOYYMMDD(day.ToShortDateString());
                    reportSchedule.SendDate = ConvertDDMMYYTOYYMMDD(day.ToShortDateString());
                    reportSchedule.SchTime = TimeSpan.Parse(Convert.ToDateTime(mobj.Time).ToString("hh:mm:ss.0000")); ;
                    reportSchedule.Status = "X";
                    reportSchedule.AUTHIDS = muserid;
                    reportSchedule.AUTHORISE = mauthorise;
                    reportSchedule.ENTEREDBY = muserid;
                    reportSchedule.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                    ctxTFAT.ReportScheduleDate.Add(reportSchedule);
                }
            }
        }
        public ActionResult DeleteStateMaster(AutoOutstandingMailVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var mList = ctxTFAT.tfatAutoOSMail.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                    var OldList = ctxTFAT.ReportScheduleDate.Where(x => x.Code == mList.Code && x.Type == "OS000" && x.Status == "X").ToList();
                    ctxTFAT.ReportScheduleDate.RemoveRange(OldList);
                    ctxTFAT.tfatAutoOSMail.Remove(mList);

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.Document, DateTime.Now, 0, "", "Delete Send Outstanding Mail", "NA");

                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new
                    {
                        Message = ex1.InnerException.Message,
                        Status = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new
                    {
                        Message = ex.InnerException.Message,
                        Status = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new
            {
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion SaveData

        public void SendAutoMail(DateTime date)
        {
            var CurrDate = date.ToShortDateString().Split('/');
            var CurrTime = date.ToString("HH:mm").Split(':');
            DateTime CurrDATE = new DateTime(Convert.ToInt32(CurrDate[2]), Convert.ToInt32(CurrDate[1]), Convert.ToInt32(CurrDate[0]), Convert.ToInt32(CurrTime[0]), Convert.ToInt32(CurrTime[1]), 00);
            var CurrDay = CurrDATE.DayOfWeek;
            var SetDataTime = CurrDATE.Hour.ToString("D2") + ":" + CurrDATE.Minute.ToString("D2");

            if (SetDataTime == "12:00")
            {
                var collection = ctxTFAT.tfatAutoOSMail.ToList();
                collection.Select(c => { c.SendMail = false; return c; }).ToList();
                ctxTFAT.SaveChanges();
            }

            string EmailmStr = "";
            EmailmStr = "<html>";
            //EmailmStr += "<p><span style=\"font-family:Tahoma;font-size:16pt;\"><b>" + (ctxTFAT.TfatBranch.Where(z => z.Code == mbranchcode).Select(x => x.Name).FirstOrDefault() ?? "") + "</b></span></p>";
            EmailmStr += "<p><span style=\"font-family:Tahoma;font-size:16pt;\"><b>" + "Please Find The Attachment." + "</b></span></p>";
            EmailmStr += "<br/>";
            EmailmStr += "</html>";

            #region Check EveryDay Mail Reports
            var EveryDayList = ctxTFAT.tfatAutoOSMail.Where(x => x.EveryDay == true && x.SendMail == false && x.Time.Trim() == SetDataTime.Trim() && x.Active == true).ToList();
            foreach (var item in EveryDayList)
            {
                #region First Get Outstanding
                var FromDate = (Convert.ToDateTime(date.ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                string Query = "";
                if (item.Customer)
                {
                    Query = " drop table ztmp_AutoMailData   select c.Code,l.DocDate, l.Branch,l.BillNumber,l.Type,Amt=(Case when (Debit<>0 and ('D'='D' or 'D'='U')) or (Credit<>0 and ('D'='S' or 'D'='U')) then (l.Debit+l.Credit) else 0 End), " +
                            " Pending = (Case when(Debit <> 0 and('D' = 'D' or 'D' = 'U')) or(Credit <> 0 and('D' = 'S' or 'D' = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + FromDate + "', l.Branch) end) else 0 End), " +
                            " OnAccount = (Case when(Debit <> 0 and 'D' = 'S' or 'D' = 'U1') or(Credit <> 0 and('D' = 'D' or 'D' = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + FromDate + "', l.Branch) end) else 0 end), " +
                            " Datediff(d, l.DocDate + l.CrPeriod, Getdate()) as oDueDays " +
                            " into ztmp_AutoMailData from Ledger l left " +
                            " join Master m on m.Code = l.Code left " +
                            " join CustomerMaster c on c.code = l.party left " +
                            " join CMasterInfo x on l.code = x.code " +
                            " left " +
                            " join Address a on a.Code = l.Code and a.Sno = 0 where(m.BaseGr = 'U' or m.BaseGr = 'D') and l.MainType <> 'MV' and l.MainType <> 'PV' and Charindex(l.party,'" + item.Account + "')<> 0  ";
                }
                else
                {
                    Query = " drop table ztmp_AutoMailData   Select l.Code,l.DocDate,l.Branch,l.BillNumber,l.Type, Amt=(Case when (Debit<>0 and ('D'='D' or 'D'='U')) or (Credit<>0 and ('D'='S' or 'D'='U')) then (l.Debit+l.Credit) else 0 End), " +
                            " Pending = (Case when(Debit <> 0 and('D' = 'D' or 'D' = 'U')) or(Credit <> 0 and('D' = 'S' or 'D' = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + FromDate + "', l.Branch) end) else 0 End), " +
                            " OnAccount = (Case when(Debit <> 0 and 'D' = 'S' or 'D' = 'U1') or(Credit <> 0 and('D' = 'D' or 'D' = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + FromDate + "', l.Branch) end) else 0 end), " +
                            " Datediff(d, l.DocDate + l.CrPeriod, Getdate()) as oDueDays " +
                            " into ztmp_AutoMailData from Ledger l left " +
                            " join Master m on m.Code = l.Code left " +
                            " join MasterInfo x on l.code = x.code " +
                            " left " +
                            " join Address a on a.Code = l.Code and a.Sno = 0 where(m.BaseGr = 'U' or m.BaseGr = 'D') and l.MainType <> 'MV' and l.MainType <> 'PV' and Charindex(l.code, '" + item.Account + "')<> 0  ";
                }

                //ExecuteStoredProc("Drop Table ztmp_AutoMailData");
                SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                tfat_conx.Open();
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();

                tfat_conx.Close();
                #endregion

                #region Generate PDF DataTable
                var mstream = GetStream(item.Customer == true ? "true" : "false", FromDate);
                SendSendEMail(mstream, item.EmailTo, "Outstanding Reports", EmailmStr + "\n\n\n\n:Generated from ALT.AIR.3 on Cloud, info@Shruhamsoftware.com", item.EmailCC, item.EmailBCC, true);
                item.SendMail = true;
                ctxTFAT.Entry(item).State = EntityState.Modified;
                ctxTFAT.SaveChanges();
                #endregion
            }
            #endregion

            #region Check Day Wise Mail Reports
            var DayWiseList = ctxTFAT.tfatAutoOSMail.Where(x => x.Day == true && x.SendMail == false && x.DayName.ToString().Trim().ToLower() == CurrDay.ToString().Trim().ToLower() && x.Time.Trim() == SetDataTime.Trim() && x.Active == true).ToList();
            foreach (var item in DayWiseList)
            {
                #region First Get Outstanding
                var FromDate = (Convert.ToDateTime(date.ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                string Query = "";
                if (item.Customer)
                {
                    Query = " drop table ztmp_AutoMailData   select c.Code,l.DocDate, l.Branch,l.BillNumber,l.Type,Amt=(Case when (Debit<>0 and ('D'='D' or 'D'='U')) or (Credit<>0 and ('D'='S' or 'D'='U')) then (l.Debit+l.Credit) else 0 End), " +
                            " Pending = (Case when(Debit <> 0 and('D' = 'D' or 'D' = 'U')) or(Credit <> 0 and('D' = 'S' or 'D' = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + FromDate + "', l.Branch) end) else 0 End), " +
                            " OnAccount = (Case when(Debit <> 0 and 'D' = 'S' or 'D' = 'U1') or(Credit <> 0 and('D' = 'D' or 'D' = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + FromDate + "', l.Branch) end) else 0 end), " +
                            " Datediff(d, l.DocDate + l.CrPeriod, Getdate()) as oDueDays " +
                            " into ztmp_AutoMailData from Ledger l left " +
                            " join Master m on m.Code = l.Code left " +
                            " join CustomerMaster c on c.code = l.party left " +
                            " join CMasterInfo x on l.code = x.code " +
                            " left " +
                            " join Address a on a.Code = l.Code and a.Sno = 0 where(m.BaseGr = 'U' or m.BaseGr = 'D') and l.MainType <> 'MV' and l.MainType <> 'PV' and Charindex(l.party,'" + item.Account + "')<> 0  ";
                }
                else
                {
                    Query = " drop table ztmp_AutoMailData   Select l.Code,l.DocDate,l.Branch,l.BillNumber,l.Type, Amt=(Case when (Debit<>0 and ('D'='D' or 'D'='U')) or (Credit<>0 and ('D'='S' or 'D'='U')) then (l.Debit+l.Credit) else 0 End), " +
                            " Pending = (Case when(Debit <> 0 and('D' = 'D' or 'D' = 'U')) or(Credit <> 0 and('D' = 'S' or 'D' = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + FromDate + "', l.Branch) end) else 0 End), " +
                            " OnAccount = (Case when(Debit <> 0 and 'D' = 'S' or 'D' = 'U1') or(Credit <> 0 and('D' = 'D' or 'D' = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + FromDate + "', l.Branch) end) else 0 end), " +
                            " Datediff(d, l.DocDate + l.CrPeriod, Getdate()) as oDueDays " +
                            " into ztmp_AutoMailData from Ledger l left " +
                            " join Master m on m.Code = l.Code left " +
                            " join MasterInfo x on l.code = x.code " +
                            " left " +
                            " join Address a on a.Code = l.Code and a.Sno = 0 where(m.BaseGr = 'U' or m.BaseGr = 'D') and l.MainType <> 'MV' and l.MainType <> 'PV' and Charindex(l.code, '" + item.Account + "')<> 0  ";
                }

                //ExecuteStoredProc("Drop Table ztmp_AutoMailData");
                SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                tfat_conx.Open();
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();

                tfat_conx.Close();
                #endregion

                #region Generate PDF DataTable
                var mstream = GetStream(item.Customer == true ? "true" : "false", FromDate);
                SendSendEMail(mstream, item.EmailTo, "Pending Outstanding", EmailmStr + "\n\n\n\n:Generated from ALT.AIR.3 on Cloud, info@Shruhamsoftware.com", item.EmailCC, item.EmailBCC, true);
                item.SendMail = true;
                ctxTFAT.Entry(item).State = EntityState.Modified;
                ctxTFAT.SaveChanges();
                #endregion
            }

            #endregion

            #region Check Date Mail Reports
            DateTime OnlyCurrDate = ConvertDDMMYYTOYYMMDD(CurrDATE.ToShortDateString());
            var DaTeWiseList = ctxTFAT.tfatAutoOSMail.Where(x => x.DateR == true && x.SendMail == false && x.Time.Trim() == SetDataTime.Trim() && x.Active == true).ToList();
            foreach (var item in DaTeWiseList)
            {
                if (item.Date.Value == OnlyCurrDate)
                {
                    #region First Get Outstanding
                    var FromDate = (Convert.ToDateTime(date.ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    string Query = "";
                    if (item.Customer)
                    {
                        Query = " drop table ztmp_AutoMailData   select c.Code,l.DocDate, l.Branch,l.BillNumber,l.Type,Amt=(Case when (Debit<>0 and ('D'='D' or 'D'='U')) or (Credit<>0 and ('D'='S' or 'D'='U')) then (l.Debit+l.Credit) else 0 End), " +
                                " Pending = (Case when(Debit <> 0 and('D' = 'D' or 'D' = 'U')) or(Credit <> 0 and('D' = 'S' or 'D' = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + FromDate + "', l.Branch) end) else 0 End), " +
                                " OnAccount = (Case when(Debit <> 0 and 'D' = 'S' or 'D' = 'U1') or(Credit <> 0 and('D' = 'D' or 'D' = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + FromDate + "', l.Branch) end) else 0 end), " +
                                " Datediff(d, l.DocDate + l.CrPeriod, Getdate()) as oDueDays " +
                                " into ztmp_AutoMailData from Ledger l left " +
                                " join Master m on m.Code = l.Code left " +
                                " join CustomerMaster c on c.code = l.party left " +
                                " join CMasterInfo x on l.code = x.code " +
                                " left " +
                                " join Address a on a.Code = l.Code and a.Sno = 0 where(m.BaseGr = 'U' or m.BaseGr = 'D') and l.MainType <> 'MV' and l.MainType <> 'PV' and Charindex(l.party,'" + item.Account + "')<> 0  ";
                    }
                    else
                    {
                        Query = " drop table ztmp_AutoMailData   Select l.Code,l.DocDate,l.Branch,l.BillNumber,l.Type, Amt=(Case when (Debit<>0 and ('D'='D' or 'D'='U')) or (Credit<>0 and ('D'='S' or 'D'='U')) then (l.Debit+l.Credit) else 0 End), " +
                                " Pending = (Case when(Debit <> 0 and('D' = 'D' or 'D' = 'U')) or(Credit <> 0 and('D' = 'S' or 'D' = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + FromDate + "', l.Branch) end) else 0 End), " +
                                " OnAccount = (Case when(Debit <> 0 and 'D' = 'S' or 'D' = 'U1') or(Credit <> 0 and('D' = 'D' or 'D' = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + FromDate + "', l.Branch) end) else 0 end), " +
                                " Datediff(d, l.DocDate + l.CrPeriod, Getdate()) as oDueDays " +
                                " into ztmp_AutoMailData from Ledger l left " +
                                " join Master m on m.Code = l.Code left " +
                                " join MasterInfo x on l.code = x.code " +
                                " left " +
                                " join Address a on a.Code = l.Code and a.Sno = 0 where(m.BaseGr = 'U' or m.BaseGr = 'D') and l.MainType <> 'MV' and l.MainType <> 'PV' and Charindex(l.code, '" + item.Account + "')<> 0  ";
                    }

                    //ExecuteStoredProc("Drop Table ztmp_AutoMailData");
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                    tfat_conx.Open();
                    cmd.CommandTimeout = 0;
                    cmd.ExecuteNonQuery();

                    tfat_conx.Close();
                    #endregion

                    #region Generate PDF DataTable
                    var mstream = GetStream(item.Customer == true ? "true" : "false", FromDate);
                    SendSendEMail(mstream, item.EmailTo, "Pending Outstanding", EmailmStr + "\n\n\n\n:Generated from ALT.AIR.3 on Cloud, info@Shruhamsoftware.com", item.EmailCC, item.EmailBCC, true);
                    item.SendMail = true;
                    ctxTFAT.Entry(item).State = EntityState.Modified;
                    ctxTFAT.SaveChanges();
                    #endregion
                }

            }

            #endregion

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
                //rd.PrintToPrinter(1, true, 0, 0);
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
                if (GlbalMail == false)
                {
                    var mEmailInfo = ctxTFAT.TfatPass.Where(z => z.Code == muserid).Select(x => new
                    {
                        x.SMTPUser,
                        x.SMTPServer,
                        x.SMTPPassword,
                        x.SMTPPort,
                        x.CCTo,
                        x.BCCTo,
                        x.Email
                    }).FirstOrDefault();
                    if (mEmailInfo != null)
                    {
                        mCC = (mCC != "" ? mCC + "," : "");
                        mCC += mEmailInfo.CCTo == null ? "" : mEmailInfo.CCTo.Trim();
                        mBCC = (mBCC != "" ? mBCC + "," : "");
                        mBCC += mEmailInfo.BCCTo == null ? "" : mEmailInfo.BCCTo.Trim();
                        msmtpuser = mEmailInfo.SMTPUser == null ? "" : mEmailInfo.SMTPUser.Trim();
                        msmtppassword = mEmailInfo.SMTPPassword == null ? "" : mEmailInfo.SMTPPassword.Trim();
                        msmtphost = mEmailInfo.SMTPServer == null ? "" : mEmailInfo.SMTPServer.Trim();
                        msmtpport = mEmailInfo.SMTPPort == null ? 25 : mEmailInfo.SMTPPort.Value;
                        mFromEmail = (mEmailInfo.SMTPUser ?? "").Trim();
                    }
                }
                else
                {
                    var mEmailInfo = ctxTFAT.TfatComp.Where(z => z.Code == mcompcode).Select(x => new
                    {
                        x.SMTPUser,
                        x.SMTPServer,
                        x.SMTPPassword,
                        x.SMTPPort,
                        x.CCTo,
                        x.BCCTo,
                        x.Email
                    }).FirstOrDefault();
                    if (mEmailInfo != null)
                    {
                        mCC = (mCC != "" ? mCC + "," : "");
                        mCC += mEmailInfo.CCTo == null ? "" : mEmailInfo.CCTo.Trim();
                        mBCC = (mBCC != "" ? mBCC + "," : "");
                        mBCC += mEmailInfo.BCCTo == null ? "" : mEmailInfo.BCCTo.Trim();
                        msmtpuser = mEmailInfo.SMTPUser == null ? "" : mEmailInfo.SMTPUser.Trim();
                        msmtppassword = mEmailInfo.SMTPPassword == null ? "" : mEmailInfo.SMTPPassword.Trim();
                        msmtphost = mEmailInfo.SMTPServer == null ? "" : mEmailInfo.SMTPServer.Trim();
                        msmtpport = mEmailInfo.SMTPPort == null ? 25 : mEmailInfo.SMTPPort.Value;
                        mFromEmail = (mEmailInfo.Email ?? "").Trim();
                    }
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

                System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(memoryStream, "Attachment.pdf");
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
                mid = SaveEmailLog(mEmail, mCC, mBCC, mSubject, mMsg, "", "");

                //return Json(new { Status = "Success", Message = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (SmtpFailedRecipientException smtex)
            {
                ExecuteStoredProc("Update Emaillog Set sentStatus=0 where RecordKey=" + mid);
                //return Json(new { Status = "Error", Message = smtex.InnerException }, JsonRequestBehavior.AllowGet);
            }
        }
        private string TextToHtml(string text)
        {
            //text = HttpUtility.HtmlEncode(text);
            text = text.Replace("\r\n", "\r");
            text = text.Replace("\n", "\r");
            text = text.Replace("\r", "<br>\r\n");
            text = text.Replace("  ", " &nbsp;");
            string mstr = "<html>";

            if (ctxTFAT.TfatBranch.Where(z => z.Code == mbranchcode).Select(x => x.Logo).FirstOrDefault() != null)
                mstr += "<img src = \"data:image/png;base64," + Convert.ToBase64String(ctxTFAT.TfatBranch.Where(z => z.Code == mbranchcode).Select(x => x.Logo).FirstOrDefault()) + "\" width=\"50\" height=\"50\" alt=\"Branch Logo\"/>";
            mstr += "<p><span style=\"font-family:Tahoma;font-size:16pt;\"><b>" + (ctxTFAT.TfatBranch.Where(z => z.Code == mbranchcode).Select(x => x.Name).FirstOrDefault() ?? "") + "</b></span></p>";
            mstr += "<hr>";
            if (text.Contains("^b"))
            {
                text = text.Replace("^b", "<strong>").Replace("^eb", "</strong>");
            }
            mstr += text + "</html>";
            return mstr;
        }

    }
}