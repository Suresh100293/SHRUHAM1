using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using ClosedXML.Excel;
using EntitiModel;
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
    public class AutoConsignmentMailController : BaseController
    {
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;


        #region Functions
        public ActionResult PopulateSaveReports(string Type)
        {
            string GetReportsDraft = "";
            if (Type.Trim()=="LR000")
            {
                GetReportsDraft = "LorryReceiptReports";
            }
            else
            {
                GetReportsDraft = "FreightMemoReport";
            }

            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT ReportNameAlias FROM ReportParameters where Reports='"+ GetReportsDraft + "' order by ReportNameAlias ";
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

        public ActionResult PopulateTypes()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem
            {
                Text = "Lorry Receipt",
                Value = "LR000"
            });
            items.Add(new SelectListItem
            {
                Text = "Freight Memo",
                Value = "FM000"
            });
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

        public ActionResult GetEmailID(string Codes)
        {
            string message = "";
            var Array = Codes.Split(',').ToList();

            var List = ctxTFAT.Caddress.Where(x => Array.Contains(x.Code)).Select(x => x.Email).ToList();
            foreach (var item in List)
            {
                if (!String.IsNullOrEmpty(item))
                {
                    message += item + ",";
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

        // GET: Logistics/AutoConsignmentMail
        public ActionResult Index(AutoConsignmentMailVM mModel)
        {
            //GenerateData(DateTime.Now);
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");

            mdocument = mModel.Document;

            mModel.Customers = PopulateCustomers();

            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.tfatAutoConsignmentMail.Where(x => x.Code == mModel.Document).FirstOrDefault();
                if (mList != null)
                {
                    mModel.Type = mList.Type;
                    mModel.DailyBasis = mList.DailyBasis;
                    mModel.YestDailyBasis = mList.YestDailyBasis;
                    mModel.Code = mList.Code;
                    mModel.Account = mList.Customer;
                    mModel.ReportName = mList.Report;
                    mModel.Active = mList.Active;
                    mModel.EmailTo = mList.EmailTo;
                    mModel.EmailCC = mList.EmailCC;
                    mModel.EmailBCC = mList.EmailBCC;
                    mModel.Time = mList.Time;
                    mModel.EveryDay = mList.EveryDay;
                    mModel.Day = mList.Day;
                    mModel.DayName = mList.DayName;
                    mModel.DateR = mList.DateR;
                    mModel.UptoDate = mList.UptoDate.ToShortDateString();
                    if (mList.DateR)
                    {
                        mModel.Date = mList.Date.Value.ToShortDateString();
                    }
                }
            }

            if (mModel.Mode == "Add")
            {
                mModel.Type = "LR000";
                mModel.Active = true;
                mModel.EveryDay = true;
                mModel.Time = DateTime.Now.ToString("HH:mm");
                mModel.Date = DateTime.Now.ToShortDateString();
                mModel.UptoDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            }
            return View(mModel);
        }

        #region SaveData

        public ActionResult SaveData(AutoConsignmentMailVM mModel)
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
                    tfatAutoConsignmentMail mobj = new tfatAutoConsignmentMail();
                    bool mAdd = true;
                    if (ctxTFAT.tfatAutoConsignmentMail.Where(x => (x.Code == mModel.Document)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.tfatAutoConsignmentMail.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                        mAdd = false;
                    }

                    //mobj.ShortName = mModel.ShortName;
                    mobj.Report = mModel.ReportName;
                    if (mModel.Type=="LR000")
                    {
                        mobj.FormatCode = ctxTFAT.ReportParameters.Where(x => x.Reports == "LorryReceiptReports" && x.ReportNameAlias.Trim() == mModel.ReportName.Trim()).Select(x => x.ReportName).FirstOrDefault();
                    }
                    else
                    {
                        mobj.FormatCode = ctxTFAT.ReportParameters.Where(x => x.Reports == "FreightMemoReport" && x.ReportNameAlias.Trim() == mModel.ReportName.Trim()).Select(x => x.ReportName).FirstOrDefault();
                    }
                    mobj.Customer = mModel.Account;
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
                    mobj.DailyBasis = mModel.DailyBasis;
                    mobj.YestDailyBasis = mModel.YestDailyBasis;
                    mobj.Type = mModel.Type;
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
                        var NewCode = ctxTFAT.tfatAutoConsignmentMail.OrderByDescending(x => x.Code).Select(x => x.Code).Take(1).FirstOrDefault();
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
                        ctxTFAT.tfatAutoConsignmentMail.Add(mobj);
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
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mNewCode, DateTime.Now, 0, "", "Save Send Consignment Mail", "NA");

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

        public void SetSchedule(tfatAutoConsignmentMail mobj)
        {
            var OldList = ctxTFAT.ReportScheduleDate.Where(x => x.Code == mobj.Code && x.Type == mobj.Type && x.Status == "X").ToList();
            ctxTFAT.ReportScheduleDate.RemoveRange(OldList);
            if (mobj.Active)
            {
                foreach (DateTime day in EachDay(mobj.NextHitDate, mobj.UptoDate))
                {
                    ReportScheduleDate reportSchedule = new ReportScheduleDate();
                    reportSchedule.Code = mobj.Code;
                    reportSchedule.Type = mobj.Type;
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

        public ActionResult DeleteStateMaster(AutoConsignmentMailVM mModel)
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
                    var mList = ctxTFAT.tfatAutoConsignmentMail.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                    var OldList = ctxTFAT.ReportScheduleDate.Where(x => x.Code == mList.Code && x.Type == mList.Type && x.Status == "X").ToList();
                    ctxTFAT.ReportScheduleDate.RemoveRange(OldList);
                    ctxTFAT.tfatAutoConsignmentMail.Remove(mList);

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.Document, DateTime.Now, 0, "", "Delete Send Consignment Mail", "NA");

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

        #region Email
        string EmailmStr = "";
        public void GenerateData(DateTime date)
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

            EmailmStr = "<html>";
            //EmailmStr += "<p><span style=\"font-family:Tahoma;font-size:16pt;\"><b>" + (ctxTFAT.TfatBranch.Where(z => z.Code == mbranchcode).Select(x => x.Name).FirstOrDefault() ?? "") + "</b></span></p>";
            EmailmStr += "<p><span style=\"font-family:Tahoma;font-size:16pt;\"><b>" + "Please Find The Attachment." + "</b></span></p>";
            EmailmStr += "<br/>";
            EmailmStr += "</html>";

            #region Check EveryDay Mail Reports
            var EveryDayList = ctxTFAT.tfatAutoConsignmentMail.Where(x => x.EveryDay == true && x.SendMail == false && x.Time.Trim() == SetDataTime.Trim() && x.Active == true).ToList();
            foreach (var item in EveryDayList)
            {
                #region First TempData->EXCEL Outstanding

                GenerateTempData(item.Report, item.FormatCode, item.EmailTo, item.EmailBCC, item.EmailCC);

                #endregion

                #region Generate PDF DataTable
                item.SendMail = true;
                ctxTFAT.Entry(item).State = EntityState.Modified;
                ctxTFAT.SaveChanges();
                #endregion
            }
            #endregion

            #region Check Day Wise Mail Reports
            var DayWiseList = ctxTFAT.tfatAutoConsignmentMail.Where(x => x.Day == true && x.SendMail == false && x.DayName.ToString().Trim().ToLower() == CurrDay.ToString().Trim().ToLower() && x.Time.Trim() == SetDataTime.Trim() && x.Active == true).ToList();
            foreach (var item in DayWiseList)
            {
                #region First Get Outstanding
                var FromDate = (Convert.ToDateTime(date.ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                #region First TempData->EXCEL Outstanding

                GenerateTempData(item.Report, item.FormatCode, item.EmailTo, item.EmailBCC, item.EmailCC);

                #endregion
                #endregion

                #region Generate EXCEL DataTable
                item.SendMail = true;
                ctxTFAT.Entry(item).State = EntityState.Modified;
                ctxTFAT.SaveChanges();
                #endregion
            }

            #endregion

            #region Check Date Mail Reports
            DateTime OnlyCurrDate = ConvertDDMMYYTOYYMMDD(CurrDATE.ToShortDateString());
            var DaTeWiseList = ctxTFAT.tfatAutoConsignmentMail.Where(x => x.DateR == true && x.SendMail == false && x.Time.Trim() == SetDataTime.Trim() && x.Active == true).ToList();
            foreach (var item in DaTeWiseList)
            {
                if (item.Date.Value == OnlyCurrDate)
                {
                    #region First Get Outstanding
                    var FromDate = (Convert.ToDateTime(date.ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    #region First TempData->EXCEL Outstanding

                    GenerateTempData(item.Report, item.FormatCode, item.EmailTo, item.EmailBCC, item.EmailCC);

                    #endregion

                    #endregion

                    #region Generate EXCEL DataTable
                    //var mstream = GetStream(item.Customer == true ? "true" : "false", FromDate);
                    //SendSendEMail(mstream, item.EmailTo, "Pending Outstanding", EmailmStr + "\n\n\n\n:Generated from ALT.AIR.3 on Cloud, info@Shruhamsoftware.com", item.EmailCC, item.EmailBCC, true);
                    item.SendMail = true;
                    ctxTFAT.Entry(item).State = EntityState.Modified;
                    ctxTFAT.SaveChanges();
                    #endregion
                }

            }

            #endregion


        }

        private void GenerateTempData(string report, string formatCode, string ToEmail, string BCC, string CC)
        {
            LorryReceiptReportsVM Model = new LorryReceiptReportsVM();
            Model.ViewDataId = formatCode;
            Model.ReportName = report;
            var MainReportName = Model.ViewDataId;
            Model.ReportTypeL = formatCode;
            var Tfatsearch = ctxTFAT.TfatSearch.Where(x => x.Code.Trim().ToLower() == Model.ReportTypeL.Trim().ToLower()).ToList();
            Tfatsearch.ForEach(x => x.IsHidden = false);
            ReportParameters mobj = new ReportParameters();
            if (ctxTFAT.ReportParameters.Where(x => x.ReportName.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).FirstOrDefault() != null)
            {
                mobj = ctxTFAT.ReportParameters.Where(x => x.ReportName.Trim().ToLower() == MainReportName.Trim().ToLower() && x.ReportNameAlias.Trim().ToLower() == Model.ReportName.Trim().ToLower()).FirstOrDefault();

                Model.ConsignmentNo = mobj.DocNo;
                Model.FromDate = mobj.StartDate.Value.ToShortDateString();
                Model.ToDate = mobj.EndDate.Value.ToShortDateString();
                Model.HideColumnList = mobj.HideColumnList;

                Model.LrMode = mobj.Para1;
                Model.FromBranch = mobj.Para2 == null ? "" : mobj.Para2;
                Model.ToBranch = mobj.Para3 == null ? "" : mobj.Para3;
                Model.LrType = mobj.Para4 == null ? "" : mobj.Para4;
                Model.FromDestination = mobj.Para5 == null ? "" : mobj.Para5;
                Model.ToDestination = mobj.Para6 == null ? "" : mobj.Para6;
                Model.BillingBranch = mobj.Para7 == null ? "" : mobj.Para7;
                Model.Consignor = mobj.Para8 == null ? "" : mobj.Para8;
                Model.Consignee = mobj.Para9 == null ? "" : mobj.Para9;
                Model.BillingParty = mobj.Para10 == null ? "" : mobj.Para10;
                Model.Delivery = mobj.Para11 == null ? "" : mobj.Para11;
                Model.Collection = mobj.Para12 == null ? "" : mobj.Para12;
                Model.ChargeType = mobj.Para13 == null ? "" : mobj.Para13;
                Model.Particular = mobj.Para14 == null ? "" : mobj.Para14;
                Model.Unit = mobj.Para15 == null ? "" : mobj.Para15;
                Model.LrGenetate = mobj.Para16 == null ? "" : mobj.Para16;

                Model.BillRelationDetails = mobj.Para17 == "T" ? true : false;
                Model.DispatchDetails = mobj.Para18 == "T" ? true : false;
                Model.DeliveryDetails = mobj.Para19 == "T" ? true : false;
                Model.ExpensesDetails = mobj.Para20 == "T" ? true : false;

                Model.ChargeShow = mobj.Para21 == "T" ? true : false;
                Model.BillChargeShow = mobj.Para22 == "T" ? true : false;
                Model.StockBranch = mobj.Para23 == null ? "" : mobj.Para23;
                Model.StockType = mobj.Para24 == null ? "" : mobj.Para24;
            }
            if (!String.IsNullOrEmpty(Model.HideColumnList))
            {
                var HiddenColumns = Model.HideColumnList.Split(',').ToList();
                Tfatsearch.Where(x => HiddenColumns.Contains(x.Sno.ToString())).ToList().ForEach(x => x.IsHidden = true);
                ctxTFAT.SaveChanges();
            }

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
                    cmd.Parameters.Add("@DeliveryDetails", SqlDbType.Bit).Value = Model.DeliveryDetails;
                    cmd.Parameters.Add("@ExpensesDetails", SqlDbType.Bit).Value = Model.ExpensesDetails;
                }
                if (Model.ViewDataId == "LorryReceiptNOLC")
                {

                    cmd.Parameters.Add("@BillDetails", SqlDbType.Bit).Value = Model.BillRelationDetails;
                    cmd.Parameters.Add("@DeliveryDetails", SqlDbType.Bit).Value = Model.DeliveryDetails;
                    cmd.Parameters.Add("@ExpensesDetails", SqlDbType.Bit).Value = Model.ExpensesDetails;
                }
                if (Model.ViewDataId == "UNBillLorryReceipt")
                {

                    cmd.Parameters.Add("@DispatchDetails", SqlDbType.Bit).Value = Model.DispatchDetails;
                    cmd.Parameters.Add("@DeliveryDetails", SqlDbType.Bit).Value = Model.DeliveryDetails;
                    cmd.Parameters.Add("@ExpensesDetails", SqlDbType.Bit).Value = Model.ExpensesDetails;
                }
                if (Model.ViewDataId == "UNDispatchLorryReceipt")
                {

                    cmd.Parameters.Add("@BillDetails", SqlDbType.Bit).Value = Model.BillRelationDetails;
                    cmd.Parameters.Add("@DispatchDetails", SqlDbType.Bit).Value = Model.DispatchDetails;
                    cmd.Parameters.Add("@DeliveryDetails", SqlDbType.Bit).Value = Model.DeliveryDetails;
                    cmd.Parameters.Add("@ExpensesDetails", SqlDbType.Bit).Value = Model.ExpensesDetails;
                }
                if (Model.ViewDataId == "LorryReceiptStock")
                {
                    cmd.Parameters.Add("@StockBranch", SqlDbType.VarChar).Value = Model.StockBranch;
                    cmd.Parameters.Add("@StockType", SqlDbType.VarChar).Value = Model.StockType;
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

                GetExcel(gridOption, "EXLS", ToEmail, BCC, CC);
            }

        }

        public ActionResult GetExcel(GridOption Model, string mwhat, string ToEmail, string BCC, string CC)
        {
            Model.mWhat = mwhat;
            PreExecute(Model);
            if (Model.Date == null || Model.Date == "undefined:undefined")
            {
                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
                Model.FromDate = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                Model.ToDate = (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            }
            else
            {
                var Date = Model.Date.Split(':');
                if (Date[0] == null || Date[0] == "undefined")
                {
                    //Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                }
                else
                {
                    Model.FromDate = Date[0];
                }
                if (Date[1] == null || Date[1] == "undefined")
                {
                    Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
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
                    return SendMail(CreateSSRSReportt(Model, "L", mArr, "Landscape", "Code^" + Model.SelectContent + (mpara != "" ? "~" + mpara : ""), Model.Opening), Model.ViewDataId, ToEmail, BCC, CC);
                default:
                    break;
            }
            return null;
        }
        public System.Data.DataTable CreateSSRSReportt(GridOption Model, string mReportType, string[] mparameters, string mpageorient = "Landscape", string mParaString = "", decimal mopening = -1)
        {
            string mPrintFormat = (ctxTFAT.ReportHeader.Where(z => z.Code == Model.ViewDataId).Select(x => x.PrintFormat).FirstOrDefault() ?? "").Trim();
            if (mPrintFormat == "")
            {
                mPrintFormat = Model.ViewDataId.Replace("/", "").Replace("\\", "").Replace(" ", "");
            }
            else
            {
                Model.ViewDataId = mPrintFormat;
            }
            //mPrintFormat= mPrintFormat.Replace("/","").Replace("\\","").Replace(" ","");
            if (FileExists("/Reports/REP_" + mPrintFormat + "_" + (mpageorient == "Landscape" ? "L" : "P") + ".rdlc") == false)
            {
                mPrintFormat = "NoReportDefined";
                Model.ViewDataId = "NoReportDefined";
                mpageorient = "Landscape";
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
                mFixedPara = Model.Para.ToString();
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
            cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
            cmd.Parameters.Add("@mUserID", SqlDbType.VarChar).Value = muserid;
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
            cmd.Parameters.Add("@mDocString", SqlDbType.VarChar).Value = mDocString;
            cmd.CommandTimeout = 0;
            con.Open();
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = cmd;
            //
            string mreportheading = "";
            Model.AccountDescription = ctxTFAT.ReportHeader.Where(z => z.Code == Model.ViewDataId).Select(x => x.FormatHead).FirstOrDefault() ?? "";
            if (Model.AccountDescription != "")
            {
                Model.AccountDescription = Model.AccountDescription.Replace("%RepStartDate", Model.FromDate);
                Model.AccountDescription = Model.AccountDescription.Replace("%RepEndDate", Model.ToDate);
                for (int xx = 0; xx <= 23; xx++)
                {
                    string mfld = "%para" + (xx + 1).ToString().PadLeft(2, '0');
                    if (Model.AccountDescription.Contains(mfld))
                    {
                        Model.AccountDescription = Model.AccountDescription.Replace(mfld, mparameters[xx]);
                    }
                }
                mreportheading = Model.AccountDescription.Trim();
            }
            System.Data.DataTable dt = new System.Data.DataTable();
            da.Fill(dt);
            cmd.Dispose();
            da.Dispose();
            con.Close();
            con.Dispose();

            System.Data.DataTable data = new System.Data.DataTable();
            data = dt.Copy();

            var DeleteColumns = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId && x.IsHidden == true).Select(x => x.Sno).ToList();
            foreach (var item in DeleteColumns)
            {
                DataColumn dataColumn = data.Columns[item - 1];
                dt.Columns.Remove(dataColumn.ColumnName);
            }

            for (int i = 0; i < dt.Columns.Count; i++)
            {

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
            ReportDataSource rds = new ReportDataSource("DataSet1", dt); // DataSet1 is defined in .rdlc, 

            System.Data.DataTable dt2 = GetDataTable(@"Select Code,Name,Addrl1,Addrl2,Addrl3,Addrl4,City,Tel1,Tel2,Tel3,Tel4,TINNumber,VATReg,www,State,aPin,Email,CINNo,GSTNo,PanNo,Country from TfatBranch Where Code='" + mbranchcode + "'");
            ReportDataSource rds2 = new ReportDataSource("DataSet2", dt2); // DataSet2 is defined in .rdlc, 
            List<ReportParameter> reportParams = new List<ReportParameter>
            {
                new ReportParameter("muserid", muserid),
                new ReportParameter("mreportheading", mreportheading)
            };
            rv.LocalReport.SetParameters(reportParams);
            rv.LocalReport.DataSources.Clear();
            rv.LocalReport.DataSources.Add(rds);
            rv.LocalReport.DataSources.Add(rds2);
            rv.LocalReport.Refresh();
            rv.ShowZoomControl = true;
            rv.ShowPrintButton = true;
            return dt;
        }

        public ActionResult SendMail(System.Data.DataTable dataTable, string ViewDataId, string ToEmail, string BCC, string CC)
        {
            if (dataTable.Columns.Count > 0)
            {
                //byte[] bytes = rv.LocalReport.Render("Excel", null);


                SendSendEMail(ViewDataId, "Excel", dataTable, ToEmail, "Auto Generated Report", EmailmStr + "\n\n\n\n:Generated from ALT.AIR.3 on Cloud, info@Shruhamsoftware.com", CC, BCC, true);


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
                        mFromEmail = (mEmailInfo.Email ?? "").Trim();
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
                        mFromEmail = (mEmailInfo.SMTPUser ?? "").Trim();
                    }
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
                    var worksheet =  wb.Worksheets.Add(dataTable,Formatcode);
                    worksheet.CellsUsed().Style.Border.BottomBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    worksheet.CellsUsed().Style.Border.TopBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    worksheet.CellsUsed().Style.Border.LeftBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    worksheet.CellsUsed().Style.Border.RightBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    //worksheet.CellsUsed().Style.Border.TopBorderColor = ClosedXML.Excel.XLColor.Red;
                    worksheet.Tables.FirstOrDefault().ShowAutoFilter = false;
                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        //Convert MemoryStream to Byte array.
                        byte[] bytes1 = stream.ToArray();
                        TypeEx = "xlsx";
                        System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(new MemoryStream(bytes1), Formatcode + "." + TypeEx);
                        message.Attachments.Add(attachment);
                        //return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Grid.xlsx");
                    }
                }

                //Convert MemoryStream to Byte array.
                //byte[] bytes = memoryStream.ToArray();
                //memoryStream.Close();

                //MemoryStream memoryStream = new MemoryStream(bytes);
                //memoryStream.Seek(0, SeekOrigin.Begin);
                //TypeEx = "xlsx";
                //System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(memoryStream, Formatcode + "." + TypeEx);
                //message.Attachments.Add(attachment);

                


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
        #endregion
    }
}