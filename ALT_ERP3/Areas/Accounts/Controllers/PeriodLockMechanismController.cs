using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class PeriodLockMechanismController : BaseController
    {
        private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mAUTHORISE = "A00";
        private static int mdocument = 0;



        public JsonResult GetBranch(string term)
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

            var Modified = items.Select(x => new
            {
                Code = x.Value,
                Name = x.Text
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);

        }

        // GET: Accounts/PeriodLockMechanism
        public ActionResult Index(PeriodLockVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            List<PeriodLockVM> mLeftList = new List<PeriodLockVM>();
            var mlist = ctxTFAT.DocTypes.Where(x => x.MainType != x.SubType && x.Code != x.SubType).Select(x => x).OrderBy(x => x.Name).ToList();
            foreach (var i in mlist)
            {
                mLeftList.Add(new PeriodLockVM()
                {
                    DocTypes_Code = i.Code,
                    DocTypes_Name = i.Name,
                    DocTypes_MainType = i.MainType,
                    DocTypes_SubType = i.SubType,
                });
            }
            mModel.mLeftList = mLeftList;
            return View(mModel);
        }

        public ActionResult ClickLeftGrid(PeriodLockVM mModel)
        {
            List<PeriodLockVM> periodLocks = new List<PeriodLockVM>();
            List<DateTime> PeriodMonthList = new List<DateTime>();
            for (int i = 0; i < 12; i++)
            {
                if (i == 0)
                {
                    DateTime StartDate = ConvertDDMMYYTOYYMMDD(System.Web.HttpContext.Current.Session["StartDate"].ToString());
                    PeriodMonthList.Add(StartDate);
                }
                else
                {
                    DateTime StartDate = PeriodMonthList[i - 1];
                    var NextDate = StartDate.AddMonths(1);
                    PeriodMonthList.Add(NextDate);
                }
            }
            foreach (var item in PeriodMonthList)
            {
                var mstr = " WITH numbers   " +
                           "  as " +
                           "  (    " +
                           "      Select 1 as value   " +
                           "      UNion ALL   " +
                           "      Select value +1 from numbers    " +
                           "      where value + 1 <= Day(EOMONTH(datefromparts(" + item.ToString("yyyy") + "," + item.ToString("MM") + ", 1)))    " +
                           "  )  " +
                           " SELECT datefromparts(" + item.ToString("yyyy") + "," + item.ToString("MM") + ",numbers.value) LockDate, DATENAME(month, datefromparts(" + item.ToString("yyyy") + "," + item.ToString("MM") + ",numbers.value))+' -"+ item.ToString("yyyy") + "' AS MonthN,  " +
                           "  case when  ( select  count(*) from PeriodLock where Branch='" + mModel.CurrBranch + "' and Type='" + mModel.Rules_Type+"' and LockDate=datefromparts(" + item.ToString("yyyy") + "," + item.ToString("MM") + ",numbers.value) ) >0 then 'true' else 'false' end as Lock  FROM numbers  ";

                List<DataRow> ordersstk = GetDataTable(mstr).AsEnumerable().ToList();
                var DaysList = Add2ItemForPickup(ordersstk);
                PeriodLockVM periodLock = new PeriodLockVM();
                periodLock.MonthName = DaysList.Select(x => x.MonthName).FirstOrDefault();
                periodLock.mRightList = DaysList;
                periodLocks.Add(periodLock);
            }

            mModel.mRightList = periodLocks;


            var html = ViewHelper.RenderPartialView(this, "TransactionPeriodLockPartial", new PeriodLockVM { PeriodList = PeriodMonthList, mRightList = mModel.mRightList, DocTypes_Code = mModel.Rules_Type });
            return Json(new { mRightList = mModel.mRightList, MessageRules_Type = mModel.Rules_Type, Html = html }, JsonRequestBehavior.AllowGet);
        }

        public List<PeriodLockVM> Add2ItemForPickup(List<DataRow> ordersstk)
        {
            double PendFactor = 0;
            List<PeriodLockVM> objitemlist = new List<PeriodLockVM>();
            int i = 1;
            foreach (var item in ordersstk)
            {

                objitemlist.Add(new PeriodLockVM()
                {
                    Date = ConvertDDMMYYTOYYMMDD(item["LockDate"].ToString()),
                    Lock = item["Lock"].ToString() == "true" ? true : false,
                    MonthName = item["MonthN"].ToString().Trim(),
                });
            }

            return objitemlist;
        }

        #region SaveData
        public ActionResult SaveData(PeriodLockVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var mobjstk1 = ctxTFAT.PeriodLock.Where(x => x.Type == mModel.Rules_Type && x.Branch==mModel.CurrBranch).ToList();
                    ctxTFAT.PeriodLock.RemoveRange(mobjstk1);
                    ctxTFAT.SaveChanges();
                    if (mModel.mRightList!=null)
                    {
                        foreach (var eachvalue in mModel.mRightList)
                        {
                            if (eachvalue.Lock)
                            {
                                PeriodLock mobj = new PeriodLock();
                                mobj.Branch = mModel.CurrBranch;
                                mobj.CompCode = mcompcode;
                                mobj.LockDate = eachvalue.Date;
                                mobj.Locked = eachvalue.Lock;
                                mobj.Type = mModel.Rules_Type;
                                mobj.AUTHIDS = muserid;
                                mobj.AUTHORISE = mAUTHORISE;
                                mobj.CompCode = mcompcode;
                                mobj.ENTEREDBY = muserid;
                                mobj.LASTUPDATEDATE = DateTime.Now;
                                ctxTFAT.PeriodLock.Add(mobj);
                                ctxTFAT.SaveChanges();
                            }

                        }
                    }
                    
                    transaction.Commit();
                    transaction.Dispose();
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    string dd1 = ex1.InnerException.Message;
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    string dd = ex.InnerException.Message;
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);

                }
            }
            return Json(new { Status = "Success", id = "PeriodLock" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}