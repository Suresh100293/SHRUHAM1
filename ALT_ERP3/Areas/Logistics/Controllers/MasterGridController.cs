using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using System.Data.Entity.Validation;
using System.Data.Entity;
using System.Globalization;
using System.Data.SqlClient;
using System.Data;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class MasterGridController : BaseController
    {
        public static string connstring = "";
        //ITransactionGridOperation mIlst = new TransactionGridOperation();
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        public string muserid = System.Web.HttpContext.Current.Session["UserId"].ToString();
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
        // GET: Logistics/MasterGrid
        public ActionResult Index(GridOption Model)
        {
            Session["TempAttach"] = null;
            GetAllMenu(Session["ModuleName"].ToString());
            //UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "NA");
            Model.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
            Model.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
            if (Model.Document == null)
            {
                Model.Document = "";
                Model.AccountName = "";
            }
            moptioncode = Model.OptionCode;
            msubcodeof = Model.ViewDataId;
            mmodule = Model.Module;
            ViewBag.id = Model.ViewDataId;
            ViewBag.ViewDataId = Model.ViewDataId;
            ViewBag.Header = Model.Header;
            ViewBag.Table = Model.TableName;
            ViewBag.Controller = Model.Controller;
            ViewBag.MainType = Model.MainType;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;
            ViewBag.ViewName = Model.ViewName;
            if (Model.AcType != null)
            {
                ViewBag.MainType = Model.AcType;
            }
            TfatMenu tfatMenu = ctxTFAT.TfatMenu.Where(z => z.OptionCode == moptioncode && z.ModuleName == mmodule).FirstOrDefault();
            if (muserid.ToUpper() == "SUPER")
            {
                Model.xAdd = true;
                Model.xDelete = true;
                Model.xEdit = true;
                Model.xPrint = true;
            }
            else
            {
                var result = ctxTFAT.UserRights.Where(z => z.MenuID == tfatMenu.ID && z.ModuleName == mmodule && z.Code == muserid).FirstOrDefault();

                if (result != null)
                {
                    Model.xAdd = result.xAdd;
                    Model.xDelete = result.xDelete;
                    Model.xEdit = result.xEdit;
                    Model.xPrint = result.xPrint;
                }
                if (Model.ViewDataId == "AccountingPeriods" || Model.ViewDataId == "VehicleGroupStatus")
                {
                    Model.xDelete = false;
                    Model.xEdit = false;
                    Model.xPrint = false;
                }
            }

            if (Model.ViewDataId == "MultiDelivery")
            {
                Model.xEdit = false;
                Model.xPrint = false;
            }
            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();


            return View(Model);
        }

        public ActionResult GetFormats()
        {
            var result = ctxTFAT.ReportHeader.Where(x => x.SubCodeOf == msubcodeof && !x.Code.EndsWith(".bak")).Select(m => new
            {
                Code = m.Code,
                Name = m.Code
            }).OrderBy(n => n.Code).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LorryDraft()
        {
            var result = ctxTFAT.ReportHeader.Where(x => x.SubCodeOf == "LorryDraft" && !x.Code.EndsWith(".bak")).Select(m => new
            {
                Code = m.Code,
                Name = m.Code
            }).OrderBy(n => n.Code).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);

        }
        public ActionResult GetFormats1()
        {
            var result = ctxTFAT.ReportHeader.Select(m => new
            {
                Code = "LorryDraft",
                Name = "LorryDraft"
            }).FirstOrDefault();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadTemplate(string term)//Template
        {
            var list = ctxTFAT.Templates.ToList().Distinct();
            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);

        }

        public void SetConncection()
        {
            connstring = GetConnectionString();
        }
        private List<SelectListItem> PopulateUsersBranches()
        {
            SetConncection();
            string muserid = (System.Web.HttpContext.Current.Session["UserId"] ?? "Super").ToString();
            List<SelectListItem> items = new List<SelectListItem>();
            //string constr = ConfigurationManager.ConnectionStrings["tfatEntitiesConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connstring))
            {
                string query = " SELECT Code, Name FROM TfatBranch where Code <> 'G00000' and Grp <> 'G00000' and Category <> 'Area' and Users Like '%" + muserid + "%'";
                if (muserid == "Super")
                {
                    query = " SELECT Code, Name FROM TfatBranch where Code <> 'G00000' and Grp <> 'G00000' and Category <> 'Area' ";
                }
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
        public ActionResult GetMessengerEvents()
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            //GSt.Add(new SelectListItem { Value = "Clear", Text = "Clear" });
            //GSt.Add(new SelectListItem { Value = "UnClear", Text = "UnClear" });
            GSt.Add(new SelectListItem { Value = "Read", Text = "Read" });
            GSt.Add(new SelectListItem { Value = "UnRead", Text = "UnRead" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetNotificationEvents()
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "Clear", Text = "Clear" });
            GSt.Add(new SelectListItem { Value = "UnClear", Text = "UnClear" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
            if (id == "EmergencyMail")
            {
                ExecuteStoredProc("Drop Table tempEmergencyMail");
                string Query = "with DummyOS as (SELECT C.NextHitDate,C.UptoDate,case when C.EveryDay='true' then 'Every Day' else case when C.Day='true' then 'Day' else 'Date' end    end as Particular,case when C.Active='true' then 'Yes' else 'No' end As Active,case when C.DailyBasis='true' then 'Yes' else 'No' end As DailyBasis,C.Type,C.Code,C.EmailTo,C.EmailCC,C.EmailBCC,C.Report,C.FormatCode,(select M.Name from CustomerMaster M where M.code=C.Customer) as Account FROM tfatAutoConsignmentMail C UNION all SELECT O.NextHitDate,O.UptoDate,case when O.EveryDay='true' then 'Every Day' else case when O.Day='true' then 'Day' else 'Date' end    end as Particular,case when O.Active='true' then 'Yes' else 'No' end As Active, 'No' As DailyBasis,'' as Type,O.Code,O.EmailTo,O.EmailCC,O.EmailBCC,O.Report,'Ouststanding' as FormatCode,(case when O.Customer = 'true' then(select M.Name from CustomerMaster M where M.code = O.Account) else (select M.Name from Master M where M.code = O.Account) end) as Account FROM tfatAutoOSMail O) select* into tempEmergencyMail from DummyOS";
                ExecuteStoredProc(Query);
            }

            
            return GetGridDataColumns(id, "L", "XXXX");
        }

        //[HttpPost]
        public ActionResult GetMasterGridData(GridOption Model)
        {
            if (Model.IsFormatSelected == true)
            {
                int noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=0 Where SubCodeOf='" + msubcodeof + "'");
                noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=-1 Where Code='" + Model.ViewDataId + "'");
            }
            if (string.IsNullOrEmpty(Model.searchField))
            {
                if (!String.IsNullOrEmpty(Model.searchString) && Model.searchString != "undefined")
                {
                    Model.searchOper = "cn";
                    if (Model.ViewDataId == "Delivery" || Model.ViewDataId == "OpeningLR")
                    {
                        Model.searchField = " LrNO ";
                    }
                    else if (Model.ViewDataId == "Attachment")
                    {
                        Model.searchField = "   MIN(Srl)    ";
                    }
                    else if (Model.ViewDataId == "BroadcastMaster")
                    {
                        Model.searchField = " Narr ";
                    }
                    else if (Model.ViewDataId == "AlertNote")
                    {
                        Model.searchField = " TypeCode ";
                    }
                    else if (Model.ViewDataId == "MyMenu")
                    {
                        Model.searchField = " Menu ";
                    }
                    else if (Model.ViewDataId == "MyReportCentre" || Model.ViewDataId == "EmailTemplates")
                    {
                        Model.searchField = " Code ";
                    }
                    else if (Model.ViewDataId == "Messenger")
                    {
                        Model.searchField = " FromIDs ";
                    }
                    else if (Model.ViewDataId == "AssignTasks")
                    {
                        Model.searchField = " AssignedBy ";
                    }
                    else if (Model.ViewDataId == "DirectSubmission")
                    {
                        Model.searchField = "    BSF.BillNo     ";
                    }
                    else if (Model.ViewDataId == "PartySubmission")
                    {
                        Model.searchField = "    (Select Name From CustomerMaster where Code=Party)     ";
                    }
                    else if (Model.ViewDataId == "SendBill")
                    {
                        Model.searchField = "   (Select Name From tfatbranch where Code=FTBranch)  ";
                    }
                    else if (Model.ViewDataId == "ReceiveBill")
                    {
                        Model.searchField = "   (Select Name From tfatbranch where Code=FTBranch)  ";
                    }
                    else
                    {
                        Model.searchField = " Name ";
                    }

                }
            }

            if (Model.ViewDataId == "UserProfile")
            {
                if (String.IsNullOrEmpty(Model.Code))
                {
                    Model.Code = "Hide = 'false'";
                }
            }


            //if (Model.ViewDataId == "TFATEWBEway" && Model.Customer == false)
            //{
            //    if (Model.Date != null)
            //    {
            //        var Split = Model.Date.Split(':');
            //        Model.Date = "01/01/1900:" + Split[1];
            //    }
            //}
            mpara = "para01^1=1~";
            //if (Model.ViewDataId == "TFATEWBEway" && Model.ARAPReqOnly == false)//ARAPReqOnly Means Zoom From SideBar
            //{
            //    if (ctxTFAT.tfatEwaySetup.Select(x => x.UserBranch).FirstOrDefault())
            //    {
            //        var USersBranch = PopulateUsersBranches();
            //        var GetIntoStrng = String.Join(",", USersBranch.Select(x => x.Value).ToList());
            //        mpara = "para01^(charindex((case when s.LrTablekey is null then '' else  (select top 1 ( case when ( select T.Category From TfatBranch T where T.code= LRSTK.Branch)='Area' then ( select T.Grp From TfatBranch T where T.code= LRSTK.Branch) else LRSTK.Branch end ) from LRStock LRSTK where LRSTK.LRRefTablekey = s.LrTablekey and LRSTK.type <> 'DEL' and(case when LRSTK.TotalQty = 0 then 0 else (LRSTK.TotalQty - (((select  ISNULL(SUM(LRT.TotalQty), 0) from LRStock LRT where LRT.ParentKey = LRSTK.TableKey)) +(select ISNULL(SUM(lcd.lrqty), 0) from lcdetail lcd where lcd.Parentkey = LRSTK.Tablekey and lcd.LCRefTablekey in (select LCM.Tablekey from lcmaster LCM where LCm.DispachFM = 0))  ) ) end ) <> 0 )end),'" + GetIntoStrng + "')<> 0)~";
            //    }
            //    if (Model.searchField == "Expired")
            //    {
            //        Model.Code += " and ( s.EWBValid < '" + (Convert.ToDateTime(DateTime.Now.ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) + "' or s.ewbvalid is not null )";
            //    }
            //}
            //else if (Model.ViewDataId == "TFATEWBEway" && Model.ARAPReqOnly == true)
            //{
            //    var GetSideBar = ctxTFAT.ActiveSideBarObjects.Where(x => x.Users.Trim() == muserid.Trim() && x.Code == "EwayBillDetails").FirstOrDefault();
            //    if (GetSideBar != null)
            //    {
            //        var AllowEmptyEway = "  ";
            //        if (GetSideBar.Para12 == "T")
            //        {
            //            AllowEmptyEway = " or s.LrTablekey is null ";
            //        }
            //        mpara = "para01^(charindex((case when s.LrTablekey is null then '' else  (select top 1 ( case when ( select T.Category From TfatBranch T where T.code= LRSTK.Branch)='Area' then ( select T.Grp From TfatBranch T where T.code= LRSTK.Branch) else LRSTK.Branch end ) from LRStock LRSTK where LRSTK.LRRefTablekey = s.LrTablekey and LRSTK.type <> 'DEL' and(case when LRSTK.TotalQty = 0 then 0 else (LRSTK.TotalQty - (((select  ISNULL(SUM(LRT.TotalQty), 0) from LRStock LRT where LRT.ParentKey = LRSTK.TableKey)) +(select ISNULL(SUM(lcd.lrqty), 0) from lcdetail lcd where lcd.Parentkey = LRSTK.Tablekey and lcd.LCRefTablekey in (select LCM.Tablekey from lcmaster LCM where LCm.DispachFM = 0))  ) ) end ) <> 0 )end),'" + GetSideBar.Para10 + "')<> 0 " + AllowEmptyEway + ")~";
            //    }
            //    if (Model.searchField == "Expired")
            //    {
            //        Model.Code += " and ( s.EWBValid < '" + (Convert.ToDateTime(DateTime.Now.ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) + "' or s.ewbvalid is null )";
            //    }
            //}
            //else 
            if (Model.ViewDataId == "ConsoleTFATEWBEway" && Model.Customer == true)//Current Branch
            {
                Model.Code = "1=1";
                var USersBranch = PopulateUsersBranches();
                USersBranch.Add(new SelectListItem { Text = "General", Value = "G00000" });
                var GetIntoStrng = String.Join(",", USersBranch.Select(x => x.Value).ToList());
                mpara = "para01^s.Doctype='LR000' and s.Clear='false' and  s.EWBValid >= '"+ (Convert.ToDateTime(DateTime.Now.ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) + "' and   (LrTablekey is null or LrTablekey not in (select D.ParentKey from DeliveryMaster D)) and (charindex((case when s.LrTablekey is null then '"+mbranchcode+ "' else  (select top 1 ( case when ( select T.Category From TfatBranch T where T.code= LRSTK.Branch)='Area' then ( select T.Grp From TfatBranch T where T.code= LRSTK.Branch) else LRSTK.Branch end ) from LRStock LRSTK where LRSTK.LRRefTablekey = s.LrTablekey and LRSTK.type <> 'DEL' and(case when LRSTK.TotalQty = 0 then 0 else (LRSTK.TotalQty - (((select  ISNULL(SUM(LRT.TotalQty), 0) from LRStock LRT where LRT.ParentKey = LRSTK.TableKey)) +(select ISNULL(SUM(lcd.lrqty), 0) from lcdetail lcd where lcd.Parentkey = LRSTK.Tablekey and lcd.LCRefTablekey in (select LCM.Tablekey from lcmaster LCM where LCm.DispachFM = 0))  ) ) end ) <> 0 )end),'" + GetIntoStrng + "')<> 0) ";
            }
            else if (Model.ViewDataId== "ConsoleTFATEWBEway" && Model.Customer == false)
            {
                Model.Code = "s.DocType='LR000' and s.Clear='false' and s.EWBValid>='" + (Convert.ToDateTime(DateTime.Now.ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) + "' and (s.LrTablekey is null or s.LrTablekey not in (select D.ParentKey from DeliveryMaster D))";

                //mpara = "para01^s.EWBValid >= '" + (Convert.ToDateTime(DateTime.Now.ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) + "'~";
            }


            return GetGridReport(Model, "M", "Code^" + Model.Code + "~MainType^" + Model.MainType + (mpara != "" ? "~" + mpara : ""), false, 0);
        }



        #region Special Work For Messanger

        public ActionResult ReadMessanger(GridOption mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var mList = ctxTFAT.MessageLog.Where(x => (x.RECORDKEY.ToString() == mModel.Document)).FirstOrDefault();

                    if (mList != null)
                    {

                        mList.MessageRead = true;

                        ctxTFAT.Entry(mList).State = EntityState.Modified;

                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    //UpdateAuditTrail(mbranchcode, "Edit", "Messenger", "", DateTime.Now, 0, "", "", "A");
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
        public ActionResult UnReadMessanger(GridOption mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var mList = ctxTFAT.MessageLog.Where(x => (x.RECORDKEY.ToString() == mModel.Document)).FirstOrDefault();

                    if (mList != null)
                    {

                        mList.MessageRead = false;

                        ctxTFAT.Entry(mList).State = EntityState.Modified;

                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    //UpdateAuditTrail(mbranchcode, "Edit", "Messenger", "", DateTime.Now, 0, "", "");
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
        public ActionResult UnClearMessanger(GridOption mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var mList = ctxTFAT.MessageLog.Where(x => (x.RECORDKEY.ToString() == mModel.Document)).FirstOrDefault();

                    if (mList != null)
                    {

                        mList.MessageDelete = false;

                        ctxTFAT.Entry(mList).State = EntityState.Modified;

                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    //UpdateAuditTrail(mbranchcode, "Edit", "Messenger", "", DateTime.Now, 0, "", "");
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
        public ActionResult ClearMessanger(GridOption mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var mList = ctxTFAT.MessageLog.Where(x => (x.RECORDKEY.ToString() == mModel.Document)).FirstOrDefault();

                    if (mList != null)
                    {

                        mList.MessageDelete = true;

                        ctxTFAT.Entry(mList).State = EntityState.Modified;

                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    //UpdateAuditTrail(mbranchcode, "Edit", "Messenger", "", DateTime.Now, 0, "", "", "A");
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

        public ActionResult ActionMessanger(GridOption mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    if (mModel.list!=null)
                    {
                        foreach (var item in mModel.list)
                        {
                            var mList = ctxTFAT.MessageLog.Where(x => (x.RECORDKEY.ToString() == item.Document)).FirstOrDefault();
                            if (mModel.AcType== "Read")
                            {
                                if (mList != null)
                                {
                                    mList.MessageRead = true;
                                    ctxTFAT.Entry(mList).State = EntityState.Modified;
                                }
                            }
                            else if (mModel.AcType == "UnRead")
                            {
                                if (mList != null)
                                {
                                    mList.MessageRead = false;
                                    ctxTFAT.Entry(mList).State = EntityState.Modified;
                                }
                            }
                            else if (mModel.AcType == "Clear")
                            {
                                if (mList != null)
                                {
                                    mList.MessageDelete = true;
                                    ctxTFAT.Entry(mList).State = EntityState.Modified;
                                }
                            }
                            else if (mModel.AcType == "UnClear")
                            {
                                if (mList != null)
                                {
                                    mList.MessageDelete = false;
                                    ctxTFAT.Entry(mList).State = EntityState.Modified;
                                }
                            }
                        }
                    }
                    else
                    {
                        return Json(new { Status = "Error", Message="Data Not Found...", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    //UpdateAuditTrail(mbranchcode, "Edit", "Messenger", "", DateTime.Now, 0, "", "", "A");
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



        public ActionResult ClearNotification(GridOption mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var mList = ctxTFAT.tfatNotification.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
                    if (mList != null)
                    {
                        mList.Clear = true;
                        ctxTFAT.Entry(mList).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
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
        public ActionResult UnClearNotification(GridOption mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var mList = ctxTFAT.tfatNotification.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
                    if (mList != null)
                    {
                        mList.Clear = false;
                        ctxTFAT.Entry(mList).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
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

        public ActionResult ActionNotification(GridOption mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    if (mModel.list != null)
                    {
                        var query = (from item in mModel.list
                                    join mList in ctxTFAT.tfatNotification
                                    on item.Document equals mList.RECORDKEY.ToString()
                                    select mList).ToList();

                        //var mListToUpdate = query.ToList();

                        query.ForEach(item =>
                        {
                            item.Clear = (mModel.AcType == "Clear");
                            ctxTFAT.Entry(item).State = EntityState.Modified;
                        });

                        ////ctxTFAT.SaveChanges();
                        //foreach (var item in mModel.list)
                        //{
                        //    var mList = ctxTFAT.tfatNotification.Where(x => (x.RECORDKEY.ToString() == item.Document)).FirstOrDefault();
                        //    if (mModel.AcType == "Clear")
                        //    {
                        //        if (mList != null)
                        //        {
                        //            mList.Clear = true;
                        //            ctxTFAT.Entry(mList).State = EntityState.Modified;
                        //        }
                        //    }
                        //    else if (mModel.AcType == "UnClear")
                        //    {
                        //        if (mList != null)
                        //        {
                        //            mList.Clear = false;
                        //            ctxTFAT.Entry(mList).State = EntityState.Modified;
                        //        }
                        //    }
                        //}
                    }
                    else
                    {
                        return Json(new { Status = "Error", Message = "Data Not Found...", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    //UpdateAuditTrail(mbranchcode, "Edit", "Messenger", "", DateTime.Now, 0, "", "", "A");
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
        #endregion
        [HttpPost]
        public ActionResult GetURLDoc(string mdocument, string mode = "Edit")
        {
            if (mdocument.StartsWith("%"))
            {
                mdocument = mdocument.Substring(1);
                mode = "Delete";
            }
            string murl = "";
            try
            {
                string mtype = "", mBranch = "", mPrefix = "";
                string NewBranch = mdocument.Substring(0, 6);
                if (ctxTFAT.TfatBranch.Where(x => x.Code == NewBranch).FirstOrDefault() == null)
                {
                    mtype = mdocument.Substring(0, 5);
                    mPrefix = ctxTFAT.tfatNotification.Where(x => x.Tablekey == mdocument).Select(x => x.prefix).FirstOrDefault();
                    mBranch = ctxTFAT.tfatNotification.Where(x => x.Tablekey == mdocument).Select(x => x.Branch).FirstOrDefault();
                    if (mtype == "Trip0")
                    {
                        mdocument = mdocument.Substring(7, (mdocument.Length - 7));
                    }
                    else if (mtype == "FM000" || mtype == "LR000" || mtype == "LC000")
                    {
                        mdocument = mdocument;
                    }
                    else
                    {
                        mdocument = mBranch + mdocument;
                    }
                }
                else
                {
                    var mdocumentnew = mdocument.Substring(6, mdocument.Length - 6);
                    mtype = mdocumentnew.Substring(0, 5);
                    mBranch = ctxTFAT.tfatNotification.Where(x => x.Tablekey == mdocument).Select(x => x.Branch).FirstOrDefault();
                    mPrefix = ctxTFAT.tfatNotification.Where(x => x.Tablekey == mdocument).Select(x => x.prefix).FirstOrDefault();
                    if (mtype == "Trip0")
                    {
                        mdocument = mdocument.Substring(13, (mdocument.Length - 13));
                    }
                    else if (mtype == "FM000" || mtype == "LR000" || mtype == "LC000")
                    {
                        mdocument = mdocument;
                    }
                    else
                    {
                        mdocument = mBranch + mdocument;
                    }
                }

                int ID = GetId(mtype.Trim());


                var mrights = ctxTFAT.UserRights.Where(z => z.Code == muserid && z.MenuID == ID).FirstOrDefault();

                if (mrights != null || muserid.ToLower() == "super")
                {
                    string msubtype = GetSubType(mtype);
                    var mstr = ctxTFAT.TfatMenu.Select(x => new { x.ID, x.OptionCode, x.TableName, x.ModuleName, x.Menu, x.Controller, x.FormatCode, x.SubType, x.ParentMenu }).Where(z => z.ID == ID && z.ModuleName == "Transactions" && (z.ParentMenu == "Accounts" || z.ParentMenu == "Logistics" || z.ParentMenu == "Vehicles")).FirstOrDefault();
                    murl = "/" + mstr.ParentMenu + "/" + mstr.Controller + "/Index?Document=" + mdocument + "&Mode=" + mode + "&Prefix=" + mPrefix + "&ChangeLog=" + mode + "&ViewDataId=" + HttpUtility.UrlEncode(mstr.FormatCode.Trim()) + "&Header=" + HttpUtility.UrlEncode(mstr.Menu.Trim()) + "&Module=" + HttpUtility.UrlEncode(mstr.ModuleName.Trim()) + "&TableName=" + mstr.TableName.Trim() + "&OptionType=T&OptionCode=" + HttpUtility.UrlEncode(mstr.OptionCode.Trim()) + "&Controller2=" + mstr.Controller.Trim() + "&LedgerThrough=true";
                }
                return Json(new { url = murl, Message = "In-sufficient Rights to Execute this Action." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { url = "", Message = "Error." }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}