using Common;
using EntitiModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using System.Configuration;
using iTextSharp.text;
using CrystalDecisions.CrystalReports.Engine;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WebForms;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class PODController : BaseController
    {
        string attachmentPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentPath"].ToString()) == true ? "" : ConfigurationManager.AppSettings["AttachmentPath"].ToString();
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        public static string connstring = "";
        //ITransactionGridOperation mIlst = new TransactionGridOperation();
        private string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();


        #region Function List

        public DateTime ConvertDDMMYYTOYYMMDD(string da)
        {
            string abc = da.Substring(6, 4) + "/" + da.Substring(3, 2) + "/" + da.Substring(0, 2);
            return Convert.ToDateTime(abc);
        }

        public ActionResult CheckPONO(string PONO, string DocumentNO)
        {
            //POD_Received_Master pOD_Received_Master = new POD_Received_Master();
            //if (String.IsNullOrEmpty(DocumentNO))
            //{
            //    pOD_Received_Master = ctxTFAT.POD_Received_Master.Where(x => x.POD.ToString() == PONO).FirstOrDefault();
            //}
            //else
            //{
            //    pOD_Received_Master = ctxTFAT.POD_Received_Master.Where(x => x.POD.ToString() == PONO && x.RecordKey.ToString() != DocumentNO).FirstOrDefault();
            //}
            //string message = "";

            //if (pOD_Received_Master == null)
            //{
            //    return Json(new { Message = "T", JsonRequestBehavior.AllowGet });
            //}
            //else
            {
                return Json(new { Message = "F", JsonRequestBehavior.AllowGet });
            }
        }

        public ActionResult CheckffDeliveryDate(string DocDate, bool POD, string LRNO)
        {
            bool status = true;
            string message = "T";
            DateTime Date = ConvertDDMMYYTOYYMMDD(DocDate);

            if (!String.IsNullOrEmpty(DocDate))
            {
                if (ConvertDDMMYYTOYYMMDD(StartDate) <= Date && Date <= ConvertDDMMYYTOYYMMDD(EndDate))
                {
                    status = true;
                }
                else
                {
                    status = false;
                    if (POD)
                    {
                        message = "Pod Date Should Be Between Financial Year...!";
                    }
                    else
                    {
                        message = "Delivery Date Should Be Between Financial Year...!";
                    }
                }
            }

            if (POD)
            {
                if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.Type == "POD00" && x.LockDate == Date).FirstOrDefault() != null)
                {
                    status = false;
                    message = "Pod Date is Locked By Period Lock System...!";
                }
            }
            else
            {
                if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.Type == "DELV0" && x.LockDate == Date).FirstOrDefault() != null)
                {
                    status = false;
                    message = "Delivery Date Should Be Between Financial Year...!";
                }
            }


            if (status)
            {
                LRStock lRStock = ctxTFAT.LRStock.Where(x => (x.TableKey == LRNO || x.ParentKey == LRNO)).FirstOrDefault();
                if (lRStock != null)
                {
                    LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == lRStock.LRRefTablekey).FirstOrDefault();
                    if (lRMaster != null)
                    {
                        if (lRMaster.BookDate <= Date)
                        {
                            status = true;
                        }
                        else
                        {
                            status = false;
                            if (POD)
                            {
                                message = "Pod Date Should Be Greater Than Consignment Book Date...!";
                            }
                            else
                            {
                                message = "Delivery Date Should Be Greater Than Consignment Book Date...!";
                            }
                        }

                    }
                }

            }

            return Json(new { Status = status, Message = message, JsonRequestBehavior.AllowGet });
        }

        public JsonResult BranchList(string term)
        {

            List<TfatBranch> list = new List<TfatBranch>();

            list = ctxTFAT.TfatBranch.Where(x => x.Code != mbranchcode && (x.Status == true && x.Code != "G00000" && x.Grp != "G00000" && x.Category != "Area")).ToList();

            var Newlist = list.Distinct();
            if (!(String.IsNullOrEmpty(term)))
            {
                Newlist = Newlist.Where(x => x.Name.ToLower().Contains(term.ToLower())).OrderBy(x => x.Name).Take(10).ToList();
            }

            Newlist = Newlist.Where(x => x.Category != "Area").ToList();

            Newlist.Where(w => w.Category == "Zone").ToList().ForEach(w => w.Name = w.Name + " - Z");
            Newlist.Where(w => w.Category == "0").ToList().ForEach(w => w.Name = w.Name + " - HO");
            Newlist.Where(w => w.Category == "Branch").ToList().ForEach(w => w.Name = w.Name + " - B");
            Newlist.Where(w => w.Category == "SubBranch").ToList().ForEach(w => w.Name = w.Name + " - SB");




            var Modified = Newlist.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public string GetNewCode()
        {
            var mPrevSrl = GetLastSerial("PODMaster", mbranchcode, "POD00", mperiod, "RS", DateTime.Now.Date);

            //var NewLcNo = ctxTFAT.PODMaster.Where(x => x.Prefix == mperiod).OrderByDescending(x => x.PODNo).Select(x => x.PODNo).Take(1).FirstOrDefault();
            //int LcNo;
            //if (NewLcNo == 0 || NewLcNo == null)
            //{
            //    var DocType = ctxTFAT.DocTypes.Where(x => x.Code == "POD00").Select(x => new { x.DocWidth, x.LimitFrom }).FirstOrDefault();
            //    LcNo = Convert.ToInt32(DocType.LimitFrom);
            //}
            //else
            //{
            //    LcNo = Convert.ToInt32(NewLcNo) + 1;
            //}

            return mPrevSrl.ToString();
        }

        #endregion

        // GET: Logistics/POD

        #region Index(List)

        public ActionResult Index(GridOption Model)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "NA");

            if (Model.Document == null)
            {
                Model.Document = "";
                Model.AccountName = "";
            }
            Model.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
            Model.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
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
            }

            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();


            return View(Model);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
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


            if (!String.IsNullOrEmpty(Model.searchString) && Model.searchString != "undefined")
            {
                Model.searchOper = "cn";
                if (Model.ViewDataId == "POD")
                {
                    Model.searchField = "  (Select SUBSTRING( ( SELECT ',' + CAST(lrno as varchar(max)) AS 'data()' FROM PODRel where PODNo=PODMaster.RECORDKEY FOR XML PATH('') ), 2 , 9999)) ";
                }
                else
                {
                    Model.searchField = " Name ";
                }
            }

            return GetGridReport(Model, "M", "Code^" + Model.Code, false, 0);
        }

        public ActionResult GetMasterGridData1(GridOption Model)
        {
            if (Model.IsFormatSelected == true)
            {
                int noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=0 Where SubCodeOf='" + msubcodeof + "'");
                noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=-1 Where Code='" + Model.ViewDataId + "'");
            }


            if (!String.IsNullOrEmpty(Model.searchString) && Model.searchString != "undefined")
            {
                Model.searchOper = "cn";
                if (Model.ViewDataId == "PendingLRForPOD")
                {
                    Model.searchField = "  Lrno ";
                }
                else
                {
                    Model.searchField = " Name ";
                }
            }
            else
            {
                ExecuteStoredProc("Drop Table tempPODReceivedLR");
                string connstring = GetConnectionString();
                SqlDataAdapter da = new SqlDataAdapter();
                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand();
                SqlConnection con = new SqlConnection(connstring);
                cmd = new SqlCommand("dbo.SP_PendingPODLR_DirectReceived", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
                cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;
                con.Open();
                da.SelectCommand = cmd;
                da.Fill(dt);
                string mReturnString = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
                con.Close();
                con.Dispose();

            }

            return GetGridReport(Model, "M", "MainType^" + Model.MainType, false, 0);
        }

        public ActionResult GetPODBranchReceivedGridData(GridOption Model)
        {
            if (Model.IsFormatSelected == true)
            {
                int noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=0 Where SubCodeOf='" + msubcodeof + "'");
                noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=-1 Where Code='" + Model.ViewDataId + "'");
            }

            List<ListOfPod> ExistingPOD = TempData.Peek("ExistingPOD") as List<ListOfPod>;
            var ExistKey = "";
            if (ExistingPOD != null)
            {
                foreach (var item in ExistingPOD)
                {
                    ExistKey += "'" + item.PODRefTablekey + "',";
                }
            }
            if (!String.IsNullOrEmpty(ExistKey))
            {
                ExistKey = ExistKey.Substring(0, ExistKey.Length - 1);
            }
            else
            {
                ExistKey = "''";
            }


            ExecuteStoredProc("Drop Table Ztemp_PODReceivedFromBranch");
            ExecuteStoredProc("WITH ranked_messages AS(	select ROW_NUMBER() OVER (PARTITION BY PODRel.LRRefTablekey ORDER BY PODRel.recordkey DESC) AS rn,PODM.PODNo as PODNO,LR.LrNo as Lrno,LR.Time as LRTime,Convert(char(10),LR.BookDate, 103) as LRDate, (select C.Name from Consigner C where C.code= LR.RecCode) as ConsignerName,(select C.Name from Consigner C where C.code=LR.SendCode) as ConsigneeName,(select T.Name from TfatBranch T where T.code=LR.Source) as FromName,(select T.Name from TfatBranch T where T.code=LR.Dest) as ToName,PODM.PODRemark as Remark,PODREL.TableKey as PODRefTablekey,PODM.AUTHORISE as Authorise,PODRel.LRRefTablekey as ConsignmentKey,PODM.CurrentBranch,PODM.SendReceive,PODREL.FromBranch,PODREL.ToBranch 	from PODMaster PODM	join PODRel PODREL on PODM.TableKey=PODREL.ParentKey join LRMaster LR on PODRel.LRRefTablekey=LR.TableKey where PODREL.TableKey not in (select distinct POSD.PODRefTablekey from PODRel POSD where POSD.PODRefTablekey is not null)   ) SELECT * into Ztemp_PODReceivedFromBranch FROM ranked_messages WHERE SendReceive='S' and ToBranch='" + mbranchcode + "' and FromBranch='" + Model.Age1 + "' and PODRefTablekey not in (" + ExistKey + ")");

            return GetGridReport(Model, "M", "Code^" + Model.Code + "~MainType^" + Model.MainType, false, 0);
        }


        #endregion

        public ActionResult Index1(PODReceivedVM mModel)
        {
            Session["TempAttach"] = null;
            TempData.Remove("ExistingPOD");
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;
            var Setup = ctxTFAT.PODSetup.FirstOrDefault();
            if (Setup != null)
            {
                mModel.GlobalSearch = Setup.GlobalSearch;
            }


            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {

                PODMaster pODMaster = ctxTFAT.PODMaster.Where(x => x.TableKey.ToString() == mModel.Document).FirstOrDefault();
                PODRel pODRel = ctxTFAT.PODRel.Where(x => x.ParentKey.ToString() == mModel.Document).FirstOrDefault();

                //Get Attachment
                AttachmentVM Att = new AttachmentVM();
                Att.Type = "LR000";
                Att.Srl = pODRel.LrNo.ToString();

                AttachmentController attachmentC = new AttachmentController();
                List<AttachmentVM> attachments = attachmentC.GetAttachmentListInEdit(Att);
                Session["TempAttach"] = attachments;


                if (pODMaster.Task == "Direct")
                {
                    mModel.Parentkey = pODRel.LRRefTablekey;
                    var DeliverySetup = ctxTFAT.DeliverySetup.FirstOrDefault();
                    if (DeliverySetup == null)
                    {
                        DeliverySetup = new DeliverySetup();
                    }
                    if (DeliverySetup.CurrDatetOnlyreq == false && DeliverySetup.BackDateAllow == false && DeliverySetup.ForwardDateAllow == false)
                    {
                        mModel.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
                        mModel.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
                    }
                    if (DeliverySetup.CurrDatetOnlyreq == true)
                    {
                        mModel.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
                        mModel.EndDate = DateTime.Now.ToString("yyyy-MM-dd");
                    }
                    if (DeliverySetup.BackDateAllow == true)
                    {
                        mModel.StartDate = (DateTime.Now.AddDays(-DeliverySetup.BackDaysUpto)).ToString("yyyy-MM-dd");
                    }
                    if (DeliverySetup.ForwardDateAllow == true)
                    {
                        mModel.EndDate = (DateTime.Now.AddDays(DeliverySetup.ForwardDaysUpto)).ToString("yyyy-MM-dd");
                    }


                    mModel.Task = "Direct";

                    mModel.PeriodLock = PeriodLock(pODMaster.CurrentBranch, "POD00", pODMaster.PODDate);
                    if (pODMaster.AUTHORISE.Substring(0, 1) == "A")
                    {
                        mModel.LockAuthorise = LockAuthorise("POD00", mModel.Mode, pODMaster.TableKey, pODMaster.ParentKey);
                    }


                    LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == pODRel.LRRefTablekey.ToString()).FirstOrDefault();

                    if (pODMaster != null)
                    {
                        mModel.PODDate = ConvertDDMMYYTOYYMMDD(pODMaster.PODDate.ToString()).ToShortDateString();
                        mModel.PODTime = pODMaster.PODTime;
                        mModel.PODRemark = pODMaster.PODRemark;

                        var GetUseOrNotThisPOD = ctxTFAT.PODRel.OrderByDescending(x => x.TableKey).Where(x => x.LRRefTablekey == pODRel.LRRefTablekey).FirstOrDefault();
                        if (GetUseOrNotThisPOD != null)
                        {
                            if (GetUseOrNotThisPOD.TableKey != pODRel.TableKey)
                            {
                                mModel.BlockPOD = true;
                            }

                        }

                    }
                    else
                    {
                        mModel.PODDate = DateTime.Now.ToShortDateString();
                        mModel.PODTime = DateTime.Now.ToString("HH:mm");
                    }

                    mModel.PODNO = pODMaster.PODNo.Value;
                    mModel.Lrno = pODRel.LrNo.ToString();
                    mModel.LRDate = lRMaster.BookDate.ToShortDateString();
                    mModel.LRTime = lRMaster.Time;
                    mModel.ConsignerName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.RecCode).Select(x => x.Name).FirstOrDefault();
                    mModel.ConsigneeName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.SendCode).Select(x => x.Name).FirstOrDefault();
                    mModel.FromName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Source).Select(x => x.Name).FirstOrDefault();
                    mModel.ToName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name).FirstOrDefault();
                    mModel.LRQty = lRMaster.TotQty;
                    mModel.LRWeight = lRMaster.ActWt;
                    mModel.ConsignmentKey = lRMaster.TableKey;
                }
                else
                {
                    mModel.Task = "Branch";
                    mModel.BranchPODNO = pODMaster.PODNo.Value;
                    mModel.PODDate = ConvertDDMMYYTOYYMMDD(pODMaster.PODDate.ToString()).ToShortDateString();
                    mModel.PODTime = pODMaster.PODTime;
                    mModel.ComingFromBranchPOD = pODRel.FromBranch;
                    mModel.ComingFromBranchPODN = ctxTFAT.TfatBranch.Where(x => x.Code == pODRel.FromBranch).Select(x => x.Name).FirstOrDefault();

                    List<ListOfPod> listOfPods = new List<ListOfPod>();
                    List<PODRel> pODMasters = ctxTFAT.PODRel.Where(x => x.ParentKey == pODMaster.TableKey).ToList();
                    var ListOfTablekey = pODMasters.Select(x => x.TableKey).ToList();
                    var podCheck = ctxTFAT.PODRel.Where(x => ListOfTablekey.Contains(x.PODRefTablekey)).FirstOrDefault();
                    if (podCheck != null)
                    {
                        mModel.BlockPOD = true;
                    }

                    foreach (var item in pODMasters.OrderByDescending(x => x.TableKey).ToList())
                    {
                        LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey == item.LRRefTablekey).FirstOrDefault();
                        ListOfPod listOfPod = new ListOfPod
                        {
                            //PODNO = item.ChildNo,
                            Lrno = item.LrNo.ToString(),
                            LRTime = lRMaster.Time,
                            LRDate = lRMaster.BookDate.ToShortDateString(),
                            ConsignerName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.RecCode).Select(x => x.Name).FirstOrDefault(),
                            ConsigneeName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.SendCode).Select(x => x.Name).FirstOrDefault(),
                            FromName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Source).Select(x => x.Name).FirstOrDefault(),
                            ToName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name).FirstOrDefault(),
                            PODRefTablekey = item.PODRefTablekey == null ? "" : item.PODRefTablekey,
                            LRRefTablekey = item.LRRefTablekey.ToString(),
                            RecePODRemark = item.RecePODRemark,
                        };
                        listOfPods.Add(listOfPod);
                    }
                    int LK = 1;
                    foreach (var item in listOfPods.ToList())
                    {
                        item.Sno = LK;
                        ++LK;
                    }

                    TempData["ExistingPOD"] = listOfPods;
                    mModel.PODPendingReceiveList = listOfPods;


                }

                mModel.pODDelRetions = new List<PODDelRetion>();
            }
            else
            {
                mModel.Task = "Direct";
                mModel.pODDelRetions = new List<PODDelRetion>();
                mModel.PODDate = DateTime.Now.ToShortDateString();
                mModel.PODTime = DateTime.Now.ToString("HH:mm");
            }
            return View(mModel);
        }

        public void DeUpdate(PODReceivedVM mModel)
        {
            var PODMaster = ctxTFAT.PODMaster.Where(x => x.TableKey.ToString() == mModel.Document).FirstOrDefault();
            var PODRels = ctxTFAT.PODRel.Where(x => x.ParentKey == PODMaster.TableKey).ToList();
            ctxTFAT.PODRel.RemoveRange(PODRels);
            ctxTFAT.SaveChanges();
        }

        public ActionResult SaveData(PODReceivedVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        string Msg = DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        if (Msg == "Success")
                        {
                            return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { Status = "Error", Message = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    if (mModel.Mode == "Edit")
                    {
                        var Demo = ctxTFAT.PODMaster.Where(x => x.TableKey.ToString() == mModel.Document).FirstOrDefault();
                        if (mbranchcode != Demo.CurrentBranch)
                        {
                            transaction.Rollback();
                            return Json(new { Message = "Login Branch Does Not Match With Documnet Branch.", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    if (mModel.Task == "Direct")
                    {
                        bool mAdd = true;
                        PODMaster pODMaster = new PODMaster();
                        PODRel pODRel = new PODRel();
                        if (ctxTFAT.PODMaster.Where(x => x.TableKey.ToString().Trim() == mModel.Document.Trim()).FirstOrDefault() != null)
                        {
                            pODMaster = ctxTFAT.PODMaster.Where(x => x.TableKey.ToString().Trim() == mModel.Document.Trim()).FirstOrDefault();
                            pODRel = ctxTFAT.PODRel.Where(x => x.ParentKey.ToString().Trim() == mModel.Document.Trim()).FirstOrDefault();
                            mAdd = false;
                        }
                        LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == mModel.ConsignmentKey).FirstOrDefault();
                        if (mAdd)
                        {
                            pODMaster.CreateDate = DateTime.Now;
                            pODMaster.PODNo = Convert.ToInt32(GetNewCode());
                            pODMaster.TableKey = mbranchcode + "POD00" + mperiod.Substring(0, 2) + "001" + pODMaster.PODNo;
                            pODMaster.ParentKey = mbranchcode + "POD00" + mperiod.Substring(0, 2) + pODMaster.PODNo;
                            pODMaster.CurrentBranch = mbranchcode;
                            pODMaster.Prefix = mperiod;
                            pODMaster.Task = "Direct";
                            pODMaster.SendReceive = "R";
                            pODMaster.ModuleName = "POD Received";
                            pODMaster.FromBranch = mbranchcode;

                            pODRel.Task = "Direct";
                            pODRel.SendReceive = "R";
                            pODRel.PODNo = pODMaster.PODNo.Value;
                            //pODRel.ChildNo = pODMaster.PODNo.Value;
                            pODRel.Prefix = mperiod;
                            pODRel.TableKey = mbranchcode + "PODRL" + mperiod.Substring(0, 2) + "001" + pODMaster.PODNo;
                            pODRel.ParentKey = pODMaster.TableKey;
                            pODRel.Sno = 1;
                            var GetLastPOD = ctxTFAT.PODRel.OrderByDescending(x => x.RECORDKEY).Where(x => x.LRRefTablekey == lRMaster.TableKey && (x.SendReceive == "R" || x.SendReceive == "S")).FirstOrDefault();
                            if (GetLastPOD != null)
                            {
                                pODRel.PODRefTablekey = GetLastPOD.TableKey;
                            }
                        }
                        pODMaster.PODDate = ConvertDDMMYYTOYYMMDD(mModel.PODDate);
                        pODMaster.PODTime = mModel.PODTime;
                        pODMaster.PODRemark = mModel.PODRemark;
                        pODMaster.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                        pODMaster.ENTEREDBY = muserid;
                        pODMaster.AUTHORISE = mauthorise;
                        pODMaster.AUTHIDS = muserid;

                        pODRel.LrNo = Convert.ToInt32(mModel.Lrno);
                        pODRel.FromBranch = mbranchcode;
                        pODRel.ToBranch = mbranchcode;

                        pODRel.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                        pODRel.ENTEREDBY = muserid;
                        pODRel.AUTHORISE = mauthorise;
                        pODRel.AUTHIDS = muserid;
                        pODRel.LRRefTablekey = lRMaster.TableKey;

                        string Athorise = "A00";
                        #region Authorisation
                        TfatUserAuditHeader authorisation = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == "POD00").FirstOrDefault();
                        if (authorisation != null)
                        {
                            Athorise = SetAuthorisationLogistics(authorisation, pODMaster.TableKey, pODMaster.RECORDKEY.ToString(), 0, pODMaster.PODDate.ToShortDateString(), 0, "", mbranchcode);
                            pODMaster.AUTHORISE = Athorise;
                            pODRel.AUTHORISE = Athorise;
                        }
                        #endregion
                        if (mAdd == true && (mModel.pODDelRetions == null ? 0 : mModel.pODDelRetions.Sum(x => x.DelQty)) > 0)
                        {
                            var mPrevSrl = GetLastSerial("DeliveryMaster", mbranchcode, "DELV0", mperiod, "RP", DateTime.Now.Date);

                            mModel.DeliveryNo = Convert.ToInt32(mPrevSrl);
                            DeliveryMaster deliveryMaster = new DeliveryMaster();
                            deliveryMaster.DeliveryNo = Convert.ToInt32(mModel.DeliveryNo);
                            deliveryMaster.GenerateType = "A";
                            deliveryMaster.CreateDate = DateTime.Now;
                            deliveryMaster.LoginBranch = mbranchcode;
                            deliveryMaster.Branch = mbranchcode;
                            deliveryMaster.LrNO = Convert.ToInt32(mModel.Lrno);
                            deliveryMaster.DeliveryTime = mModel.DeliveryTime;
                            deliveryMaster.DeliveryDate = ConvertDDMMYYTOYYMMDD(mModel.DeliveryDate);
                            deliveryMaster.Consigner = lRMaster.RecCode;
                            deliveryMaster.Consignee = lRMaster.SendCode;
                            deliveryMaster.FromBranch = lRMaster.Source;
                            deliveryMaster.ToBranch = lRMaster.Dest;
                            deliveryMaster.Qty = mModel.pODDelRetions.Sum(x => x.DelQty);
                            deliveryMaster.Weight = mModel.pODDelRetions.Sum(x => x.DelWeight);
                            deliveryMaster.DeliveryGoodStatus = mModel.DeliveryGoodStatus;
                            deliveryMaster.ShortQty = mModel.ShortQty;
                            deliveryMaster.DeliveryRemark = mModel.DeliveryRemark;
                            deliveryMaster.TableKey = mbranchcode + "DEL00" + mperiod.Substring(0, 2) + "001" + Convert.ToInt32(mModel.DeliveryNo).ToString("D6");
                            deliveryMaster.ParentKey = lRMaster.TableKey;
                            deliveryMaster.VehicleNO = mModel.VehicleNo;
                            deliveryMaster.PersonName = mModel.PersonName;
                            deliveryMaster.MobileNO = (mModel.MobileNO);
                            deliveryMaster.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                            deliveryMaster.ENTEREDBY = muserid;
                            deliveryMaster.AUTHORISE = mauthorise;
                            deliveryMaster.AUTHIDS = muserid;
                            deliveryMaster.Prefix = mperiod;
                            pODMaster.DeliveryNo = deliveryMaster.DeliveryNo;
                            string Athorise1 = "A00";
                            #region Authorisation
                            TfatUserAuditHeader authorisation1 = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == "DELV0").FirstOrDefault();
                            if (authorisation1 != null)
                            {
                                Athorise1 = SetAuthorisationLogistics(authorisation1, deliveryMaster.TableKey, deliveryMaster.DeliveryNo.ToString(), 0, deliveryMaster.DeliveryDate.ToShortDateString(), 0, "", mbranchcode);
                                deliveryMaster.AUTHORISE = Athorise1;
                            }
                            #endregion
                            deliveryMaster.MultiDel = false;
                            ctxTFAT.DeliveryMaster.Add(deliveryMaster);
                            var I = 0;
                            bool OtherBranchDel = false;
                            foreach (var item in mModel.pODDelRetions.Where(x => x.DelQty > 0).ToList())
                            {
                                LRStock Lrstock = ctxTFAT.LRStock.Where(x => x.TableKey == item.ParentKey).FirstOrDefault();
                                if (Lrstock.Branch != mbranchcode && OtherBranchDel == false)
                                {
                                    OtherBranchDel = true;
                                }
                                var BalQty = ctxTFAT.LRStock.Where(x => x.ParentKey == Lrstock.TableKey && x.TableKey != deliveryMaster.TableKey).Sum(x => (int?)x.TotalQty) ?? 0;
                                var UnDispatchLC = ctxTFAT.LCMaster.Where(x => x.DispachFM == 0).Select(x => x.TableKey).ToList();
                                var LCLoadQty = ctxTFAT.LCDetail.Where(x => x.ParentKey == Lrstock.TableKey && UnDispatchLC.Contains(x.LCRefTablekey)).Sum(x => (int?)x.LrQty) ?? 0;
                                int Qty = 0; double Weight = 0;
                                if (mAdd)
                                {
                                    ++I;
                                    Qty = item.DelQty;
                                    Weight = item.DelWeight;
                                    if ((Lrstock.TotalQty - (BalQty + LCLoadQty)) < (Qty))
                                    {
                                        Qty = Lrstock.TotalQty - (BalQty + LCLoadQty);
                                    }

                                    DelRelation delRelation = new DelRelation();
                                    delRelation.DeliveryNo = deliveryMaster.DeliveryNo;
                                    delRelation.Branch = Lrstock.Branch;
                                    delRelation.Type = item.Type.Length > 6 ? item.Type.Substring(0, 6) : item.Type;
                                    delRelation.ParentKey = item.ParentKey;
                                    delRelation.DelQty = Qty;
                                    delRelation.DelWeight = Weight;
                                    delRelation.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                                    delRelation.ENTEREDBY = muserid;
                                    delRelation.AUTHORISE = Athorise1;
                                    delRelation.AUTHIDS = muserid;
                                    delRelation.Prefix = mperiod;
                                    Lrstock.BalQty -= Qty;
                                    Lrstock.BalWeight -= Weight;
                                    ctxTFAT.Entry(Lrstock).State = EntityState.Modified;
                                    if (Lrstock.BalQty < 0)
                                    {
                                        transaction.Rollback();
                                        return Json(new { Message = "Delivery Not Allowed Due To Negative Stock...!", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                                    }

                                    ctxTFAT.DelRelation.Add(delRelation);

                                    #region Delivery Entry In Stock
                                    var LRData = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == Lrstock.LRRefTablekey).FirstOrDefault();
                                    LRStock LoadlRStock = new LRStock();
                                    LoadlRStock.LoginBranch = mbranchcode;
                                    LoadlRStock.Branch = mbranchcode;
                                    LoadlRStock.LrNo = Convert.ToInt32(mModel.Lrno);
                                    LoadlRStock.LoadForGodown = 0;
                                    LoadlRStock.LoadForDirect = 0;
                                    LoadlRStock.Date = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                                    LoadlRStock.Time = (DateTime.Now.ToString("HH:mm"));
                                    LoadlRStock.TotalQty = Qty;
                                    LoadlRStock.AllocatBalQty = Qty;
                                    LoadlRStock.BalQty = Qty;
                                    LoadlRStock.ActWeight = Weight;
                                    LoadlRStock.AllocatBalWght = Weight;
                                    LoadlRStock.BalWeight = Weight;
                                    LoadlRStock.ChrgWeight = LRData.ChgWt;
                                    LoadlRStock.ChrgType = LRData.ChgType;
                                    LoadlRStock.Description = LRData.DescrType;
                                    LoadlRStock.Unit = LRData.UnitCode;
                                    LoadlRStock.FromBranch = LRData.Source;
                                    LoadlRStock.ToBranch = LRData.Dest;
                                    LoadlRStock.Consigner = LRData.RecCode;
                                    LoadlRStock.Consignee = LRData.SendCode;
                                    LoadlRStock.LrType = LRData.LRtype;
                                    LoadlRStock.Coln = LRData.Colln;
                                    LoadlRStock.Delivery = LRData.Delivery;
                                    LoadlRStock.Remark = "";
                                    LoadlRStock.StockAt = "Delivery";
                                    LoadlRStock.StockStatus = "D";
                                    LoadlRStock.LCNO = Lrstock.LCNO;
                                    LoadlRStock.AUTHIDS = muserid;
                                    LoadlRStock.AUTHORISE = mauthorise;
                                    LoadlRStock.ENTEREDBY = muserid;
                                    LoadlRStock.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                                    LoadlRStock.UnloadDirectQty = 0;
                                    LoadlRStock.UnloadGodwonQty = 0;
                                    LoadlRStock.Fmno = Lrstock.Fmno;
                                    LoadlRStock.TableKey = mbranchcode + "DEL00" + mperiod.Substring(0, 2) + I.ToString("D3") + deliveryMaster.DeliveryNo;
                                    LoadlRStock.ParentKey = item.ParentKey;
                                    LoadlRStock.LRRefTablekey = Lrstock.LRRefTablekey;
                                    LoadlRStock.LCRefTablekey = Lrstock.LCRefTablekey;
                                    LoadlRStock.FMRefTablekey = Lrstock.FMRefTablekey;
                                    LoadlRStock.Type = "DEL";
                                    LoadlRStock.LRMode = LRData.LRMode;
                                    LoadlRStock.Prefix = mperiod;
                                    ctxTFAT.LRStock.Add(LoadlRStock);
                                    #endregion

                                    DeliveryNotification(deliveryMaster, OtherBranchDel, "POD");
                                }
                            }
                        }

                        if (mAdd)
                        {
                            ctxTFAT.PODMaster.Add(pODMaster);
                            ctxTFAT.PODRel.Add(pODRel);
                        }
                        else
                        {
                            ctxTFAT.Entry(pODMaster).State = EntityState.Modified;
                            ctxTFAT.Entry(pODRel).State = EntityState.Modified;
                        }
                        PODNotification(pODMaster, pODRel, mModel.NODeliveryPOD);
                        UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header + "-Direct", pODMaster.ParentKey, pODMaster.PODDate, 0, "", "Save POD NO:" + pODMaster.PODNo, "NA");

                    }
                    else
                    {
                        bool mAdd = true;
                        List<PODMaster> BranchPodList = new List<PODMaster>();
                        PODMaster pODMaster = new PODMaster();
                        if (ctxTFAT.PODMaster.Where(x => x.TableKey.ToString().Trim() == mModel.Document.Trim()).FirstOrDefault() != null)
                        {
                            pODMaster = ctxTFAT.PODMaster.Where(x => x.TableKey.ToString().Trim() == mModel.Document.Trim()).FirstOrDefault();
                            mAdd = false;
                            DeUpdate(mModel);
                        }

                        if (mAdd)
                        {
                            pODMaster.CreateDate = DateTime.Now;
                            pODMaster.PODNo = Convert.ToInt32(GetNewCode());
                            pODMaster.CurrentBranch = mbranchcode;
                            pODMaster.Task = "Branch";
                            pODMaster.SendReceive = "R";
                            pODMaster.ModuleName = "POD Received";
                            pODMaster.Prefix = mperiod;
                            pODMaster.TableKey = mbranchcode + "POD00" + mperiod.Substring(0, 2) + "001" + pODMaster.PODNo;
                            pODMaster.ParentKey = mbranchcode + "POD00" + mperiod.Substring(0, 2) + pODMaster.PODNo;
                        }
                        pODMaster.PODDate = ConvertDDMMYYTOYYMMDD(mModel.PODDate);
                        pODMaster.PODTime = mModel.PODTime;
                        pODMaster.PODRemark = mModel.PODRemark;
                        pODMaster.FromBranch = mModel.ComingFromBranchPOD;
                        pODMaster.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                        pODMaster.ENTEREDBY = muserid;
                        pODMaster.AUTHORISE = mauthorise;
                        pODMaster.AUTHIDS = muserid;

                        if (mAdd)
                        {
                            ctxTFAT.PODMaster.Add(pODMaster);
                        }
                        else
                        {
                            ctxTFAT.Entry(pODMaster).State = EntityState.Modified;
                        }

                        int Sno = 1;
                        foreach (var item1 in mModel.PODPendingReceiveList)
                        {
                            PODRel pODRel = new PODRel();
                            pODRel.Task = "Branch";
                            pODRel.SendReceive = "R";
                            pODRel.PODNo = pODMaster.PODNo.Value;
                            //pODRel.ChildNo = item1.PODNO;
                            pODRel.LrNo = Convert.ToInt32(item1.Lrno);
                            pODRel.FromBranch = mModel.ComingFromBranchPOD;
                            pODRel.ToBranch = mbranchcode;
                            pODRel.Sno = Sno;
                            pODRel.TableKey = mbranchcode + "PODRL" + mperiod.Substring(0, 2) + Sno.ToString("D3") + pODMaster.PODNo;
                            pODRel.ParentKey = pODMaster.TableKey;
                            pODRel.RecePODRemark = item1.RecePODRemark;
                            pODRel.Prefix = mperiod;
                            pODRel.LRRefTablekey = item1.LRRefTablekey;
                            pODRel.PODRefTablekey = item1.PODRefTablekey;
                            if (String.IsNullOrEmpty(item1.PODRefTablekey))
                            {
                                var GetLastPOD = ctxTFAT.PODRel.OrderByDescending(x => x.RECORDKEY).Where(x => x.LRRefTablekey == item1.LRRefTablekey && (x.SendReceive == "R" || x.SendReceive == "S")).FirstOrDefault();
                                if (GetLastPOD != null)
                                {
                                    pODRel.PODRefTablekey = GetLastPOD.TableKey;
                                }
                            }
                            //pODRel.PODRefTablekey = item1.PODRefTablekey;
                            pODRel.AUTHIDS = muserid;
                            pODRel.AUTHORISE = mauthorise;
                            pODRel.ENTEREDBY = muserid;
                            pODRel.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                            ctxTFAT.PODRel.Add(pODRel);
                            PODNotification(pODMaster, pODRel, false);
                            ++Sno;
                        }
                        UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header + "-Branch", pODMaster.ParentKey, pODMaster.PODDate, 0, "", "Save POD NO :" + pODMaster.PODNo, "NA");
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

        private string DeleteStateMaster(PODReceivedVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return "Code not Entered..";
            }
            PODMaster POdMaster = ctxTFAT.PODMaster.Where(x => x.TableKey.ToString() == mModel.Document).FirstOrDefault();

            if (POdMaster.Task == "Direct")
            {

                if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.Type == "POD00" && x.LockDate == POdMaster.PODDate).FirstOrDefault() != null)
                {
                    return "This POD Date Locked By Period Locking System ...";
                }
                var AuthorisationEntry = ctxTFAT.Authorisation.Where(x => x.Type == "POD00" && x.Srl == POdMaster.PODNo.ToString()).FirstOrDefault();
                if (AuthorisationEntry != null)
                {
                    ctxTFAT.Authorisation.Remove(AuthorisationEntry);
                }

                PODRel pODRel = ctxTFAT.PODRel.Where(x => x.ParentKey == POdMaster.TableKey).FirstOrDefault();
                LRMaster lRMaster = ctxTFAT.LRMaster.Where(X => X.LrNo == pODRel.LrNo).FirstOrDefault();
                lRMaster.POD = true;

                var PODAttachment = ctxTFAT.Attachment.Where(x => x.Type == "LR000" && x.Srl == pODRel.LrNo.ToString() && x.RefType.Trim() == "POD Received").ToList();
                foreach (var item in PODAttachment)
                {
                    if (System.IO.File.Exists(item.FilePath))
                    {
                        System.IO.File.Delete(item.FilePath);
                    }
                }
                ctxTFAT.Attachment.RemoveRange(PODAttachment);
                //ctxTFAT.Entry(lRMaster).State = EntityState.Modified;
                ctxTFAT.PODMaster.Remove(POdMaster);
                ctxTFAT.PODRel.Remove(pODRel);

                UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header + "-Direct", POdMaster.ParentKey, POdMaster.PODDate, 0, "", "Delete POD NO:" + POdMaster.RECORDKEY, "NA");

            }
            else
            {
                var POdList = ctxTFAT.PODRel.Where(x => x.ParentKey.ToString() == POdMaster.TableKey.ToString()).ToList();
                ctxTFAT.PODRel.RemoveRange(POdList);
                ctxTFAT.PODMaster.Remove(POdMaster);
                UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header + "-Branch", POdMaster.ParentKey, POdMaster.PODDate, 0, "", "Delete POD NO:" + POdMaster.RECORDKEY, "NA");
            }




            ctxTFAT.SaveChanges();
            return "Success";
        }

        //Direct Pod Received 

        [HttpPost]
        public ActionResult FetchDocumentList(LorryReceiptQueryVM Model)
        {
            var mlrmaster = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == Model.Lrno).Select(x => x).ToList();
            if (mlrmaster == null)
            {
                Model.Status = "ValidError";
                Model.Message = "Consignment Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            if (mlrmaster.Count() == 0)
            {
                Model.Status = "ValidError";
                Model.Message = "Consignment Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            else if (mlrmaster.Count() == 1)
            {
                Model.Status = "Processed";
                Model.Message = mlrmaster.Select(x => x.TableKey).FirstOrDefault();
                return Json(Model, JsonRequestBehavior.AllowGet);
            }

            var html = ViewHelper.RenderPartialView(this, "ConsignmentList", Model);
            return Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ShowLRDeliveryDetails(PODReceivedVM mModel)
        {
            var DeliverySetup = ctxTFAT.DeliverySetup.FirstOrDefault();
            if (DeliverySetup == null)
            {
                DeliverySetup = new DeliverySetup();
            }
            if (DeliverySetup.CurrDatetOnlyreq == false && DeliverySetup.BackDateAllow == false && DeliverySetup.ForwardDateAllow == false)
            {
                mModel.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
                mModel.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
            }
            if (DeliverySetup.CurrDatetOnlyreq == true)
            {
                mModel.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
                mModel.EndDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            if (DeliverySetup.BackDateAllow == true)
            {
                mModel.StartDate = (DateTime.Now.AddDays(-DeliverySetup.BackDaysUpto)).ToString("yyyy-MM-dd");
            }
            if (DeliverySetup.ForwardDateAllow == true)
            {
                mModel.EndDate = (DateTime.Now.AddDays(DeliverySetup.ForwardDaysUpto)).ToString("yyyy-MM-dd");
            }

            List<PODDelRetion> Mobj = new List<PODDelRetion>();
            LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == mModel.Lrno).FirstOrDefault();

            string DamageMessage = "";
            //PODSetup setup = ctxTFAT.PODSetup.FirstOrDefault();
            //if (setup != null)
            //{
            //    if (setup.AlertDamageMaterial)
            //    {
            //        var Deleivery = ctxTFAT.DeliveryMaster.Where(x => x.ParentKey == lRMaster.TableKey && (x.DeliveryGoodStatus == "Package Damage" || x.DeliveryGoodStatus == "Short" || x.DeliveryGoodStatus == "Material Damage")).FirstOrDefault();
            //        if (Deleivery != null)
            //        {
            //            DamageMessage = "In this POD Attachment Required Because Material is Damage/Short";
            //        }
            //    }
            //}


            mModel.PODTime = DateTime.Now.ToString("HH:mm");
            mModel.PODDate = DateTime.Now.ToShortDateString();
            if (lRMaster.AUTHORISE.Substring(0, 1) == "A")
            {
                mModel.ConsignmentKey = lRMaster.TableKey;
                mModel.Lrno = lRMaster.LrNo.ToString();
                mModel.LRTime = lRMaster.Time;
                mModel.LRDate = lRMaster.BookDate.ToShortDateString();
                mModel.ConsigneeName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.SendCode).Select(x => x.Name).FirstOrDefault();
                mModel.ConsignerName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.RecCode).Select(x => x.Name).FirstOrDefault();
                mModel.FromName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Source).Select(x => x.Name).FirstOrDefault();
                mModel.ToName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name).FirstOrDefault();
                mModel.LRQty = lRMaster.TotQty;
                mModel.LRWeight = lRMaster.ActWt;

                mModel.DeliveryTime = DateTime.Now.ToString("HH:mm");
                mModel.DeliveryDate = DateTime.Now.ToShortDateString();
                mModel.DocDate = DateTime.Now;
                mModel.DeliveryRemark = "OK";

                string connstring = GetConnectionString();
                SqlDataAdapter da = new SqlDataAdapter();
                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand();
                SqlConnection con = new SqlConnection(connstring);
                string Query = " select LRSTK.Branch As Branch, LRSTK.Tablekey As TableKey ,( select Name From TfatBranch where Code=LRSTK.Branch) As BranchN,LRSTK.LrNo , case When LRSTK.Type='LR' then substring(LRSTK.AUTHORISE,0,2) Else 'A' End  AS Authenticate," +
                               " LRSTK.type As StkType,	case when LRSTK.TotalQty=0 then 0 else ( LRSTK.TotalQty-(  ((select  ISNULL(SUM(LRT.TotalQty),0) from LRStock LRT where LRT.ParentKey=LRSTK.TableKey)) +(select ISNULL(SUM(lcd.lrqty),0) from lcdetail lcd where lcd.parentkey=LRSTK.tablekey and  lcd.LCRefTablekey in (select LCM.Tablekey from lcmaster LCM where LCm.DispachFM=0))  ) ) end as BalQty" +
                               " from LRStock LRSTK where lrstk.LRRefTablekey='" + lRMaster.TableKey + "' and LRSTK.type<>'DEL' and (case when LRSTK.TotalQty=0 then 0 else ( LRSTK.TotalQty-(  ((select  ISNULL(SUM(LRT.TotalQty),0) from LRStock LRT where LRT.ParentKey=LRSTK.TableKey)) +(select ISNULL(SUM(lcd.lrqty),0) from lcdetail lcd where lcd.parentkey=LRSTK.tablekey and  lcd.LCRefTablekey in (select LCM.Tablekey from lcmaster LCM where LCm.DispachFM=0))  ) ) end ) <>0 ";
                cmd = new SqlCommand(Query, con);
                con.Open();
                da.SelectCommand = cmd;
                da.Fill(dt);
                con.Close();
                con.Dispose();

                Mobj = GetDeliveryDetails(dt);
                Mobj = Mobj.Where(x => x.DelBalQty > 0).ToList();
                Mobj.ForEach(x =>
                {
                    x.DelBalWeight = /*Math.Round*/((mModel.LRWeight / mModel.LRQty) * (x.DelBalQty));
                    x.DelQty = x.Authorise == "A" ? x.DelBalQty : 0;
                    x.DelWeight = x.Authorise == "A" ? /*Math.Round*/((mModel.LRWeight / mModel.LRQty) * (x.DelBalQty)) : 0;
                });
                mModel.pODDelRetions = Mobj;

                var html = ViewHelper.RenderPartialView(this, "_ShowLRDeliveryDetails", mModel);
                return Json(new { Status = "Success", Html = html, DamageMessage= DamageMessage }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                mModel.pODDelRetions = Mobj;
                var html = ViewHelper.RenderPartialView(this, "_ShowLRDeliveryDetails", mModel);
                return Json(new { Status = "Error", Html = html, Message = "LR Authorise Not Match Criteria.." }, JsonRequestBehavior.AllowGet);
            }
        }

        public List<PODDelRetion> GetDeliveryDetails(DataTable table)
        {
            var categoryList = new List<PODDelRetion>(table.Rows.Count);
            foreach (DataRow row in table.Rows)
            {
                var values = row.ItemArray;
                var category = new PODDelRetion()
                {
                    ParentKey = Convert.ToString(values[1]),
                    StkBranchN = Convert.ToString(values[0]),
                    StkBranch = Convert.ToString(values[2]),
                    Type = Convert.ToString(values[5]),
                    DelBalQty = Convert.ToInt32(values[6]),
                    DelQty = Convert.ToInt32(values[6]),
                    Authorise = Convert.ToString(values[4]).Substring(0, 1),
                    BlockDelivery = Convert.ToString(values[4]).Substring(0, 1) == "A" ? false : true,
                };
                categoryList.Add(category);
            }

            return categoryList;
        }

        #region Not Use 
        //Get LRStock Using LR NO In Direct
        public ActionResult SearchLRDeliveryDetails(PODReceivedVM mModel)
        {
            var DeliverySetup = ctxTFAT.DeliverySetup.FirstOrDefault();
            if (DeliverySetup == null)
            {
                DeliverySetup = new DeliverySetup();
            }
            if (DeliverySetup.CurrDatetOnlyreq == false && DeliverySetup.BackDateAllow == false && DeliverySetup.ForwardDateAllow == false)
            {
                mModel.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
                mModel.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
            }
            if (DeliverySetup.CurrDatetOnlyreq == true)
            {
                mModel.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
                mModel.EndDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            if (DeliverySetup.BackDateAllow == true)
            {
                mModel.StartDate = (DateTime.Now.AddDays(-DeliverySetup.BackDaysUpto)).ToString("yyyy-MM-dd");
            }
            if (DeliverySetup.ForwardDateAllow == true)
            {
                mModel.EndDate = (DateTime.Now.AddDays(DeliverySetup.ForwardDaysUpto)).ToString("yyyy-MM-dd");
            }
            var Mobj = new List<PODDelRetion>();
            string Status = "", Message = "";
            LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == mModel.Lrno && x.Prefix == mperiod).FirstOrDefault();
            if (lRMaster != null)
            {
                if (lRMaster.AUTHORISE.Substring(0, 1) == "A")
                {
                    mModel.ConsignmentKey = lRMaster.TableKey;
                    mModel.Lrno = lRMaster.LrNo.ToString();
                    mModel.LRTime = lRMaster.Time;
                    mModel.LRDate = lRMaster.BookDate.ToShortDateString();
                    mModel.ConsigneeName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.SendCode).Select(x => x.Name).FirstOrDefault();
                    mModel.ConsignerName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.RecCode).Select(x => x.Name).FirstOrDefault();
                    mModel.FromName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Source).Select(x => x.Name).FirstOrDefault();
                    mModel.ToName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name).FirstOrDefault();
                    mModel.LRQty = lRMaster.TotQty;
                    mModel.LRWeight = lRMaster.ActWt;
                    var UnDispatchLCNo = ctxTFAT.LCMaster.Where(x => x.DispachFM == 0).Select(x => x.TableKey).ToList();


                    Mobj = (from LRStock in ctxTFAT.LRStock
                            where LRStock.TotalQty > 0 && LRStock.LrNo.ToString() == mModel.Lrno && (LRStock.Type == "LR" || LRStock.Type == "TRN")
                            orderby (LRStock.LrNo)
                            select new PODDelRetion()
                            {
                                ParentKey = LRStock.TableKey ?? "",
                                StkBranch = ctxTFAT.TfatBranch.Where(x => x.Code == LRStock.Branch).Select(x => x.Name).FirstOrDefault() ?? "",
                                Type = LRStock.Type == "LR" ? "Godown" : "Transit",
                                DelBalQty = LRStock.TotalQty - ((ctxTFAT.LCDetail.Where(x => x.ParentKey == LRStock.TableKey && UnDispatchLCNo.Contains(x.LCRefTablekey)).Sum(x => (int?)x.Qty) ?? 0) + (ctxTFAT.LRStock.Where(x => x.ParentKey == LRStock.TableKey).Sum(x => (int?)x.TotalQty) ?? 0)),
                                DelBalWeight = ((LRStock.TotalQty - ((ctxTFAT.LCDetail.Where(x => x.ParentKey == LRStock.TableKey && UnDispatchLCNo.Contains(x.LCRefTablekey)).Sum(x => (int?)x.Qty) ?? 0) + (ctxTFAT.LRStock.Where(x => x.ParentKey == LRStock.TableKey).Sum(x => (int?)x.TotalQty) ?? 0))) / LRStock.TotalQty) * LRStock.ActWeight,
                                DelQty = LRStock.AUTHORISE.Substring(0, 1) == "A" ? LRStock.TotalQty - ((ctxTFAT.LCDetail.Where(x => x.ParentKey == LRStock.TableKey && UnDispatchLCNo.Contains(x.LCRefTablekey)).Sum(x => (int?)x.Qty) ?? 0) + (ctxTFAT.LRStock.Where(x => x.ParentKey == LRStock.TableKey).Sum(x => (int?)x.TotalQty) ?? 0)) : 0,
                                DelWeight = LRStock.AUTHORISE.Substring(0, 1) == "A" ? ((LRStock.TotalQty - ((ctxTFAT.LCDetail.Where(x => x.ParentKey == LRStock.TableKey && UnDispatchLCNo.Contains(x.LCRefTablekey)).Sum(x => (int?)x.Qty) ?? 0) + (ctxTFAT.LRStock.Where(x => x.ParentKey == LRStock.TableKey).Sum(x => (int?)x.TotalQty) ?? 0))) / LRStock.TotalQty) * LRStock.ActWeight : 0,
                                Authorise = LRStock.AUTHORISE.Substring(0, 1),
                                BlockDelivery = LRStock.AUTHORISE.Substring(0, 1) == "A" ? false : true,
                            }).ToList();
                    Status = "Succsess";

                    if (ctxTFAT.PODRel.Where(x => x.LRRefTablekey == lRMaster.TableKey && x.Task == "Direct").FirstOrDefault() != null)
                    {
                        Status = "SuccsessWithPrompt";
                        Message = "This Consignment POD Already Created R U Sure To Create Once Again?";
                    }


                }
                else
                {
                    Status = "Error";
                    if (lRMaster.AUTHORISE.Substring(0, 1) == "N")
                    {
                        Message = mModel.Lrno + " This Consignment Un-Authorised....\n Please Contact To Admin...";
                    }
                    else
                    {
                        Message = mModel.Lrno + " This Consignment Rejected....\n Please Contact To Admin...";
                    }
                }
            }
            else
            {
                Status = "Error";
                Message = mModel.Lrno + " Not Found This Consignment...";
                mModel.Lrno = null;
            }

            if (Mobj == null)
            {
                Mobj = new List<PODDelRetion>();
            }
            Mobj = Mobj.Where(x => x.DelBalQty > 0).ToList();
            mModel.pODDelRetions = Mobj;

            mModel.PODTime = DateTime.Now.ToString("HH:mm");
            mModel.PODDate = DateTime.Now.ToShortDateString();
            mModel.DeliveryTime = DateTime.Now.ToString("HH:mm");
            mModel.DeliveryDate = DateTime.Now.ToShortDateString();
            mModel.DocDate = DateTime.Now;
            mModel.DeliveryRemark = "OK";

            var html = ViewHelper.RenderPartialView(this, "_ShowLRDeliveryDetails", mModel);
            return Json(new { mdocument = lRMaster.TableKey, Html = html, Status = Status, Message = Message }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        //Pending Pod List In Direct
        public ActionResult PendingPodList()
        {
            LCVM lCVM = new LCVM();

            string connstring = GetConnectionString();
            SqlDataAdapter da = new SqlDataAdapter();
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand();
            SqlConnection con = new SqlConnection(connstring);
            cmd = new SqlCommand("dbo.SP_PendingPODLR_DirectReceived", con)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;
            con.Open();
            da.SelectCommand = cmd;
            da.Fill(dt);
            string mReturnString = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
            con.Close();
            con.Dispose();


            var mobj = new List<LcDetailsVM>(dt.Rows.Count);
            foreach (DataRow row in dt.Rows)
            {
                var values = row.ItemArray;
                var category = new LcDetailsVM()
                {
                    recordekey = Convert.ToString(values[0]),
                    Branch = Convert.ToString(values[1]),
                    ChrgeType = Convert.ToString(values[2]),
                    Unit = Convert.ToString(values[3]),
                    From = Convert.ToString(values[4]),
                    To = Convert.ToString(values[5]),
                    Consignor = Convert.ToString(values[6]),
                    Consignee = Convert.ToString(values[7]),
                    LrType = Convert.ToString(values[8]),
                    LRDelivery = Convert.ToString(values[9]),
                    LRColln = Convert.ToString(values[10]),
                    Lrno = Convert.ToInt32(values[11]),
                    Authenticate = Convert.ToString(values[12]),
                };
                mobj.Add(category);
            }




            TempData["AllCurrBranchStock"] = mobj;
            List<LcDetailsVM> lcDetailsVMs = new List<LcDetailsVM>();
            lcDetailsVMs.AddRange(mobj.OrderBy(x => x.Lrno));
            lCVM.lCDetails = lcDetailsVMs;

            var html = ViewHelper.RenderPartialView(this, "PodPendingListPartialView", lCVM);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        #region Not Required 23.12.2022
        //Pending POD List For Received Branch Through
        public ActionResult ReceivedPodPendingList(PODReceivedVM mModel)
        {
            List<ListOfPod> OLdReceivedList = TempData.Peek("ExistingPOD") as List<ListOfPod>;
            if (OLdReceivedList == null)
            {
                OLdReceivedList = new List<ListOfPod>();
            }
            var TableKeyList = OLdReceivedList.Select(x => x.PODRefTablekey).ToList();

            List<ListOfPod> listOfPods = new List<ListOfPod>();
            if (!String.IsNullOrEmpty(mModel.ComingFromBranchPOD))
            {
                DataTable PODtbl = new DataTable();
                ExecuteStoredProc("Drop Table Ztemp_PODReceivedFromBranch");
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string sql = string.Format(@"WITH ranked_messages AS(	select ROW_NUMBER() OVER (PARTITION BY PODRel.LRRefTablekey ORDER BY PODREL.Tablekey DESC) AS rn,PODM.PODNo as PODNO,LR.LrNo as Lrno,LR.Time as LRTime,Convert(char(10),LR.BookDate, 103) as LRDate, (select C.Name from Consigner C where C.code= LR.RecCode) as ConsignerName,(select C.Name from Consigner C where C.code=LR.SendCode) as ConsigneeName,(select T.Name from TfatBranch T where T.code=LR.Source) as FromName,(select T.Name from TfatBranch T where T.code=LR.Dest) as ToName,PODM.PODRemark as Remark,PODREL.TableKey as PODRefTablekey,PODM.AUTHORISE as Authorise,PODRel.LRRefTablekey as ConsignmentKey,PODM.CurrentBranch,PODM.SendReceive,PODREL.FromBranch,PODREL.ToBranch 	from PODMaster PODM	join PODRel PODREL on PODM.TableKey=PODREL.ParentKey join LRMaster LR on PODRel.LRRefTablekey=LR.TableKey ) SELECT * into Ztemp_PODReceivedFromBranch FROM ranked_messages WHERE rn = 1 and  SendReceive='S' and ToBranch='" + mbranchcode + "' and FromBranch='" + mModel.ComingFromBranchPOD + "';");
                    SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                    da.Fill(PODtbl);
                }

                //List<ListOfPod> List = GetDatatableToPodList(PODtbl);

                //List = List.Where(x => x.ToBranch == mbranchcode && x.FromBranch == mModel.ComingFromBranchPOD && x.SendReceive=="S" && !(TableKeyList.Contains(x.PODRefTablekey))).ToList();


                //listOfPods.AddRange(List);

                //mModel.ComingFromBranchPODN = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.ComingFromBranchPOD).Select(x => x.Name).FirstOrDefault();
            }

            //List<ListOfPod> DeletePOD = TempData.Peek("DeletePOD") as List<ListOfPod>;
            //if (DeletePOD == null)
            //{
            //    DeletePOD = new List<ListOfPod>();
            //}
            //var Reco = listOfPods.Select(x => x.PODRefTablekey).ToList();
            //var CheckDistinctdelete = DeletePOD.Where(x => !Reco.Contains(x.PODRefTablekey)).ToList();
            //listOfPods.AddRange(CheckDistinctdelete);

            //mModel.PODPendingReceiveList = listOfPods.Distinct().Take(10).ToList();
            //var html = ViewHelper.RenderPartialView(this, "PodListModal", mModel);
            //listOfPods.AddRange(OLdReceivedList);
            //TempData["AllStockPODLIst"] = listOfPods.Distinct().ToList();
            return Json(new { Html = "" }, JsonRequestBehavior.AllowGet);

        }
        #endregion
        private List<ListOfPod> GetDatatableToPodList(DataTable tbl)
        {
            List<ListOfPod> Mobj = new List<ListOfPod>();
            foreach (DataRow item in tbl.Rows)
            {
                ListOfPod ofPod = new ListOfPod();
                ofPod.PODNO = Convert.ToInt32(item["PODNO"]);
                ofPod.Lrno = item["Lrno"].ToString();
                ofPod.LRTime = item["LRTime"].ToString();
                ofPod.LRDate = item["LRDate"].ToString();
                ofPod.ConsignerName = item["ConsignerName"].ToString();
                ofPod.ConsigneeName = item["ConsigneeName"].ToString();
                ofPod.FromName = item["FromName"].ToString();
                ofPod.ToName = item["ToName"].ToString();
                ofPod.Remark = item["Remark"].ToString();
                ofPod.PODRefTablekey = item["PODRefTablekey"].ToString();
                ofPod.LRRefTablekey = item["ConsignmentKey"].ToString();
                ofPod.CheckBox = false;
                ofPod.Authorise = item["Authorise"].ToString().Substring(0, 1);
                ofPod.CurrentBranch = item["CurrentBranch"].ToString();
                ofPod.SendReceive = item["SendReceive"].ToString();
                ofPod.FromBranch = item["FromBranch"].ToString();
                ofPod.ToBranch = item["ToBranch"].ToString();
                Mobj.Add(ofPod);
            }
            return Mobj;
        }

        //Show Grid IN Branch POD
        public ActionResult GridViewPOD(PODReceivedVM mModel)
        {
            List<ListOfPod> ExistingPOD = TempData.Peek("ExistingPOD") as List<ListOfPod>;
            if (ExistingPOD == null)
            {
                ExistingPOD = new List<ListOfPod>();
            }
            if (mModel.PODPendingReceiveList == null)
            {
                mModel.PODPendingReceiveList = new List<ListOfPod>();
            }
            ExistingPOD.AddRange(mModel.PODPendingReceiveList);
            mModel.PODPendingReceiveList = ExistingPOD.OrderByDescending(x => x.Sno).ToList();
            TempData["ExistingPOD"] = ExistingPOD;
            var html = ViewHelper.RenderPartialView(this, "_PendingPODReceiveList", mModel);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);

        }

        //Search IN Branch Tab
        public ActionResult SearchLRNOInBranch(PODReceivedVM mModel)
        {
            ListOfPod listOf = new ListOfPod();
            List<ListOfPod> ExistingPOD = TempData.Peek("ExistingPOD") as List<ListOfPod>;
            if (ExistingPOD == null)
            {
                ExistingPOD = new List<ListOfPod>();
            }
            if (mModel.PODPendingReceiveList == null)
            {
                mModel.PODPendingReceiveList = new List<ListOfPod>();
            }
            if (ExistingPOD.Where(x => x.LRRefTablekey == mModel.SearchLrnoForPodReceived).FirstOrDefault() == null)
            {
                LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == mModel.SearchLrnoForPodReceived).FirstOrDefault();
                if (lRMaster != null)
                {
                    var GetLastPOD = ctxTFAT.PODRel.OrderByDescending(x => x.RECORDKEY).Where(x => x.LRRefTablekey == lRMaster.TableKey && (x.SendReceive == "R" || x.SendReceive == "S")).FirstOrDefault();
                    if (GetLastPOD != null)
                    {
                        listOf.Sno = ExistingPOD.Count() + 1;
                        listOf.PODRefTablekey = GetLastPOD.TableKey;
                        listOf.PODNO = GetLastPOD.PODNo;
                        listOf.Lrno = GetLastPOD.LrNo.ToString();
                        listOf.LRRefTablekey = GetLastPOD.LRRefTablekey;
                        listOf.LRDate = lRMaster.BookDate.ToShortDateString();
                        listOf.LRTime = lRMaster.Time;
                        listOf.ConsignerName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.RecCode).Select(x => x.Name).FirstOrDefault();
                        listOf.ConsigneeName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.SendCode).Select(x => x.Name).FirstOrDefault();
                        listOf.FromName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Source).Select(x => x.Name).FirstOrDefault();
                        listOf.ToName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name).FirstOrDefault();
                        listOf.Remark = GetLastPOD.RecePODRemark;
                        ExistingPOD.Add(listOf);
                    }
                    else
                    {
                        listOf.PODRefTablekey = "";
                        listOf.PODNO = 0;
                        listOf.Lrno = lRMaster.LrNo.ToString();
                        listOf.LRRefTablekey = lRMaster.TableKey;
                        listOf.LRDate = lRMaster.BookDate.ToShortDateString();
                        listOf.LRTime = lRMaster.Time;
                        listOf.ConsignerName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.RecCode).Select(x => x.Name).FirstOrDefault();
                        listOf.ConsigneeName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.SendCode).Select(x => x.Name).FirstOrDefault();
                        listOf.FromName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Source).Select(x => x.Name).FirstOrDefault();
                        listOf.ToName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name).FirstOrDefault();
                        listOf.Remark = lRMaster.Narr;
                        ExistingPOD.Add(listOf);
                    }
                }

            }

            mModel.PODPendingReceiveList = ExistingPOD.OrderByDescending(x => x.Sno).ToList();
            TempData["ExistingPOD"] = ExistingPOD;
            var html = ViewHelper.RenderPartialView(this, "_PendingPODReceiveList", mModel);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        //Delete Pod From Branch POd Using TempData
        public ActionResult DeleteLRFromTempData(string TableKey)
        {
            string Msg = "Sucess";
            List<ListOfPod> ExistPOD = TempData.Peek("ExistingPOD") as List<ListOfPod>;
            if (ExistPOD == null)
            {
                ExistPOD = new List<ListOfPod>();
            }
            var DeletePOD1 = ExistPOD.Where(x => x.PODRefTablekey == TableKey).FirstOrDefault();
            if (DeletePOD1 != null)
            {
                ExistPOD.Remove(DeletePOD1);
            }
            TempData["ExistPOD"] = ExistPOD;
            return Json(new { Msg = Msg }, JsonRequestBehavior.AllowGet);

        }



        #region AlertNote Stop CHeck

        public ActionResult CheckStopAlertNote(string Type, List<string> TypeCode, string DocTpe, bool Delivery)
        {
            string Status = "Success", Message = "";
            var Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && TypeCode.Contains(AlertMater.ParentKey) && AlertMater.RefType.Contains(DocTpe)
                        orderby AlertMater.DocNo
                        select new AlertNoteVM()
                        {
                            TypeCode = AlertMater.TypeCode,
                            DocReceived = AlertMater.TableKey,
                        }).ToList();
            foreach (var item in Mobj)
            {
                var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                foreach (var stp in Activirty)
                {
                    if (DocTpe.Trim() == stp.Trim())
                    {
                        Status = "Error";
                        Message += item.TypeCode + " Not Allowed Create POD Please Remove IT....\n";
                        break;
                    }
                }
            }

            if (Delivery)

            {
                Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && TypeCode.Contains(AlertMater.ParentKey) && AlertMater.RefType.Contains("DELV0")
                        orderby AlertMater.DocNo
                        select new AlertNoteVM()
                        {
                            TypeCode = AlertMater.TypeCode,
                            DocReceived = AlertMater.TableKey,
                        }).ToList();
                foreach (var item in Mobj)
                {
                    var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                    foreach (var stp in Activirty)
                    {
                        if (DocTpe.Trim() == stp.Trim())
                        {
                            Status = "Error";
                            Message += item.TypeCode + " Not Allowed To Delivery Of This LR Please Remove IT....\n";
                            break;
                        }
                    }
                }
            }
            var jsonResult = Json(new { Status = Status, Message = Message }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        #endregion

        [HttpPost]
        public ActionResult GetMultiPrint(GridOption Model)
        {
            List<GridOption> Grlist = new List<GridOption>();
            Model.Type = ctxTFAT.DocFormats.Where(x => x.FormatCode == Model.ViewDataId).Select(x => x.Type).FirstOrDefault();


            var list = ctxTFAT.DocFormats.Where(x => x.Type == Model.Type).Select(x => x).ToList();
            if (list != null)
            {
                foreach (var a in list)
                {
                    Grlist.Add(new GridOption
                    {
                        Format = a.FormatCode,
                        IsFormatSelected = a.Selected,
                        StoreProcedure = a.StoredProc
                    });
                }

            }
            Model.PrintGridList = Grlist;
            var html = ViewHelper.RenderPartialView(this, "ReportPrintOptions", new GridOption() { PrintGridList = Model.PrintGridList, Document = Model.Document });
            var jsonResult = Json(new
            {
                Document = Model.Document,
                PrintGridList = Model.PrintGridList,
                Html = html
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }

        public ActionResult PrintSingleDocumentCrystal(GridOption Model)
        {
            //var relativePath = "~/Reports/" + Model.Format + ".rpt";
            if (Model.Format == null)
            {
                return Json(new { Status = "filenotfound" }, JsonRequestBehavior.AllowGet);
            }

            if (FileExists("~/Reports/" + Model.Format.Trim() + ".rpt") == false)
            {
                return Json(new { Status = "filenotfound" }, JsonRequestBehavior.AllowGet);
            }
            #region Remove Comment
            var absolutePath = HttpContext.Server.MapPath("~/Reports/" + Model.Format.Trim() + ".rpt");
            if (System.IO.File.Exists(absolutePath) == false)
            {
                return Json(new { Status = "filenotfound" }, JsonRequestBehavior.AllowGet);
            }
            #endregion

            string mParentKey = Model.Document;
            Model.Branch = mbranchcode;
            DataTable dtreport = new DataTable();
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("SPDoc_" + Model.StoreProcedure, tfat_conx); //name of the storedprocedure
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@mParentKey", mParentKey);
            cmd.Parameters.AddWithValue("@mBranch", Model.Branch);
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            adp.Fill(dtreport);

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), Model.Format.Trim() + ".rpt"));
            rd.SetDataSource(dtreport);

            try
            {
                Stream mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                mstream.Seek(0, SeekOrigin.Begin);
                rd.Close();
                rd.Dispose();
                return File(mstream, "application/PDF");
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
        }

        public ActionResult SendMultiReport(GridOption Model)
        {
            if (Model.Format == null)
            {
                return null;
            }

            string mParentKey = "";
            Document document = new Document(PageSize.A4, 10, 10, 10, 10);
            var ms = new MemoryStream();
            {
                PdfCopy pdf = new PdfCopy(document, ms);
                document.Open();
                pdf.Open();

                List<string> mformats = Model.Format.Split(',').ToList();
                foreach (var mformat in mformats)
                {
                    mParentKey = Model.Document;
                    Model.Branch = mbranchcode;
                    Model.StoreProcedure = ctxTFAT.DocFormats.Where(x => x.FormatCode.Trim() == mformat.Trim()).Select(x => x.StoredProc).FirstOrDefault();
                    DataTable dtreport = new DataTable();
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("SPDoc_" + Model.StoreProcedure, tfat_conx); //name of the storedprocedure
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@mParentKey", mParentKey);
                    cmd.Parameters.AddWithValue("@mBranch", Model.Branch);
                    SqlDataAdapter adp = new SqlDataAdapter(cmd);
                    adp.Fill(dtreport);

                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/Reports"), mformat.Trim() + ".rpt"));
                    rd.SetDataSource(dtreport);
                    try
                    {
                        Stream mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
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
                }
            }
            document.Close();

            return File(ms.ToArray(), "application/PDF");

        }
    }
}