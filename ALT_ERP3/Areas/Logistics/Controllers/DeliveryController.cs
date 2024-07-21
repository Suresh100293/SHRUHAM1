using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class DeliveryController : BaseController
    {
        string attachmentPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentPath"].ToString()) == true ? "" : ConfigurationManager.AppSettings["AttachmentPath"].ToString();
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private string mnewrecordkey = "";
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

        public ActionResult CheckDeliveryNO(string DeliveryNO, string DocumentNO)
        {
            DeliveryMaster deliveryMaster = new DeliveryMaster();
            if (String.IsNullOrEmpty(DocumentNO))
            {
                deliveryMaster = ctxTFAT.DeliveryMaster.Where(x => x.Prefix == mperiod && x.DeliveryNo.ToString() == DeliveryNO).FirstOrDefault();
            }
            else
            {
                deliveryMaster = ctxTFAT.DeliveryMaster.Where(x => x.Prefix == mperiod && x.DeliveryNo.ToString() == DeliveryNO && x.RECORDKEY.ToString() != DocumentNO).FirstOrDefault();
            }
            string message = "";

            if (deliveryMaster == null)
            {
                return Json(new { Message = "T", JsonRequestBehavior.AllowGet });
            }
            else
            {
                return Json(new { Message = "F", JsonRequestBehavior.AllowGet });
            }
        }

        public ActionResult CheckDeliveryDate(string DocDate, bool POD, string LRNO)
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
                    message = "Delivery Date / POD Date Should Be Between Financial Year...!";
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
            if (status)
            {
                if (POD)
                {
                    if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.Type == "POD00" && x.LockDate == Date).FirstOrDefault() != null)
                    {
                        status = false;
                        message = "POD Date is Locked By Period Lock System...!";
                    }
                }
                else
                {
                    if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.Type == "DELV0" && x.LockDate == Date).FirstOrDefault() != null)
                    {
                        status = false;
                        message = "Delivery Date is Locked By Period Lock System...!";
                    }
                }
            }
            return Json(new { Status = status, Message = message, JsonRequestBehavior.AllowGet });
        }

        public JsonResult VehicleNo(string term)
        {
            var BranchChild = GetChildGrp(mbranchcode);
            var list = ctxTFAT.VehicleMaster.Where(x => x.Acitve == true && BranchChild.Contains(x.Branch) && x.Status == "Ready").ToList();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.TruckNo.ToLower().Contains(term.ToLower())).ToList();
            }
            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.TruckNo
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckManual(int No, string document)
        {
            string Flag = "F";
            string Msg = "";
            bool checkalloctionFound = false;
            List<TblBranchAllocation> tblBranchAllocations = tblBranchAllocations = ctxTFAT.TblBranchAllocation.Where(x => x.Prefix == mperiod && x.Branch == mbranchcode).ToList();

            foreach (var item in tblBranchAllocations)
            {
                if (item.ManualFrom <= No && No <= item.ManualTo)
                {
                    checkalloctionFound = true;
                    break;
                }
            }

            if (checkalloctionFound)
            {
                DeliveryMaster deliveryMaster = ctxTFAT.DeliveryMaster.Where(x => x.Prefix == mperiod && x.DeliveryNo == No && x.RECORDKEY.ToString() != document).FirstOrDefault();
                if (deliveryMaster != null)
                {
                    Flag = "T";
                    Msg = "This DeliveryNo Exist \nSo,Please Change Delivery No....!";
                }
                else
                {
                    var result = ctxTFAT.DocTypes.Where(x => x.Code == "DELV0").Select(x => x).FirstOrDefault();
                    if (No.ToString().Length > (result.DocWidth))
                    {
                        Flag = "T";
                        Msg = "Delivery NO Allow " + result.DocWidth + " Digit Only....!";
                    }
                }
            }
            else
            {
                Flag = "T";
                Msg = "Manual Range Not Found....!";
            }

            return Json(new { Message = Flag, Msg = Msg, JsonRequestBehavior.AllowGet });
        }

        public ActionResult CheckAuto(int No, string document)
        {
            string Flag = "F";
            string Msg = "";

            DeliveryMaster deliveryMaster = ctxTFAT.DeliveryMaster.Where(x => x.Prefix == mperiod && x.DeliveryNo == No && x.RECORDKEY.ToString() != document).FirstOrDefault();
            if (deliveryMaster != null)
            {
                Flag = "T";
                Msg = "Please Change Delivery No....!";
            }
            return Json(new { Message = Flag, Msg = Msg, JsonRequestBehavior.AllowGet });
        }

        public string GetNewCode()
        {
            var mPrevSrl = GetLastSerial("DeliveryMaster", mbranchcode, "DELV0", mperiod, "RP", DateTime.Now.Date);

            //var NewLcNo = ctxTFAT.DeliveryMaster.Where(x => x.Prefix == mperiod).OrderByDescending(x => x.DeliveryNo).Select(x => x.DeliveryNo).Take(1).FirstOrDefault();
            //int LcNo;
            //if (NewLcNo == 0)
            //{
            //    var DocType = ctxTFAT.DocTypes.Where(x => x.Code == "DELV0").Select(x => new { x.DocWidth, x.LimitFrom }).FirstOrDefault();
            //    LcNo = Convert.ToInt32(DocType.LimitFrom);
            //}
            //else
            //{
            //    LcNo = Convert.ToInt32(NewLcNo) + 1;
            //}

            return mPrevSrl.ToString();
        }

        public string GetPODNewCode()
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

        public List<SelectListItem> GetBranchList(string VehicleNo)
        {
            List<SelectListItem> branch = new List<SelectListItem>();
            if (!String.IsNullOrEmpty(VehicleNo))
            {
                branch.Add(new SelectListItem { Text = VehicleNo, Value = VehicleNo });
            }
            var branchlist = ctxTFAT.TfatBranch.Where(x => x.Status == true && x.Category != "Area" && x.Code != "G00000" && x.Grp != "G00000").OrderBy(x => x.Code).ToList().Select(b => new { b.Code, b.Name, b.Category });
            foreach (var item in branchlist)
            {
                if (item.Category == "Branch")
                {
                    branch.Add(new SelectListItem { Text = item.Name + " -B", Value = item.Code });
                }
                else
                {
                    branch.Add(new SelectListItem { Text = item.Name + " -SB", Value = item.Code });
                }
            }
            return branch;
        }

        #endregion

        // GET: Logistics/Delivery

        public ActionResult Index(DeliveryVM mModel)
        {
            TempData.Remove("AttachmentList");
            TempData.Remove("ExistingLRLIST");
            TempData.Remove("AllStockLRLIst");
            TempData.Remove("AllDoorStockLRLIst");
            TempData.Remove("AllTransitLRLIst");
            TempData.Remove("AllLDSLRLIst");
            TempData.Remove("MergerDetails");
            Session["TempAttach"] = null;

            mModel.DeliverySetup = ctxTFAT.DeliverySetup.FirstOrDefault();
            if (mModel.DeliverySetup == null)
            {
                mModel.DeliverySetup = new DeliverySetup();
            }
            if (mModel.DeliverySetup.CurrDatetOnlyreq == false && mModel.DeliverySetup.BackDateAllow == false && mModel.DeliverySetup.ForwardDateAllow == false)
            {
                mModel.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
                mModel.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
            }
            if (mModel.DeliverySetup.CurrDatetOnlyreq == true)
            {
                mModel.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
                mModel.EndDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            if (mModel.DeliverySetup.BackDateAllow == true)
            {
                mModel.StartDate = (DateTime.Now.AddDays(-mModel.DeliverySetup.BackDaysUpto)).ToString("yyyy-MM-dd");
            }
            if (mModel.DeliverySetup.ForwardDateAllow == true)
            {
                mModel.EndDate = (DateTime.Now.AddDays(mModel.DeliverySetup.ForwardDaysUpto)).ToString("yyyy-MM-dd");
            }
            string BranchCode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();

            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                DeliveryMaster deliveryMaster = ctxTFAT.DeliveryMaster.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
                List<DelRelation> delRelations = ctxTFAT.DelRelation.Where(x => x.DeliveryNo == deliveryMaster.DeliveryNo && x.Prefix == deliveryMaster.Prefix).ToList();
                var OneConsignmentKey = delRelations.Select(x => x.ParentKey).FirstOrDefault();
                LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey == OneConsignmentKey).FirstOrDefault();
                LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey == lRStock.LRRefTablekey).FirstOrDefault();

                mModel.PeriodLock = PeriodLock(deliveryMaster.Branch, "DELV0", deliveryMaster.DeliveryDate);
                if (deliveryMaster.AUTHORISE.Substring(0, 1) == "A")
                {
                    mModel.LockAuthorise = LockAuthorise("DELV0", mModel.Mode, deliveryMaster.TableKey, deliveryMaster.ParentKey);
                }

                mModel.DeliveryNo = deliveryMaster.DeliveryNo.ToString();
                mModel.Lrno = deliveryMaster.LrNO.ToString();
                mModel.Time = lRMaster.Time;
                mModel.Date = lRMaster.BookDate.ToShortDateString();
                mModel.ConsignerName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.RecCode).Select(x => x.Name).FirstOrDefault();
                mModel.ConsigneeName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.SendCode).Select(x => x.Name).FirstOrDefault();
                mModel.FromName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Source).Select(x => x.Name).FirstOrDefault();
                mModel.ToName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name).FirstOrDefault();
                mModel.LRQty = lRMaster.TotQty;
                mModel.LRWeight = lRMaster.ActWt;
                mModel.DeliveryDate = deliveryMaster.DeliveryDate.ToShortDateString();
                mModel.DocDate = deliveryMaster.DeliveryDate;
                mModel.DeliveryTime = deliveryMaster.DeliveryTime;
                mModel.DeliveryRemark = deliveryMaster.DeliveryRemark;
                mModel.DeliveryGoodStatus = deliveryMaster.DeliveryGoodStatus;
                mModel.ShortQty = Convert.ToInt32(deliveryMaster.ShortQty);
                mModel.BillQty = Convert.ToInt32(deliveryMaster.BillQty);
                mModel.VehicleNo = deliveryMaster.VehicleNO;
                mModel.PersonName = deliveryMaster.PersonName;
                mModel.MobileNO = deliveryMaster.MobileNO;
                mModel.Parentkey = deliveryMaster.ParentKey;



                //Get Attachment
                AttachmentVM Att = new AttachmentVM();
                Att.Type = "LR000";
                Att.Srl = deliveryMaster.LrNO.ToString();

                AttachmentController attachmentC = new AttachmentController();
                List<AttachmentVM> attachments = attachmentC.GetAttachmentListInEdit(Att);
                Session["TempAttach"] = attachments;

                List<DelRetion> dels = new List<DelRetion>();

                var UndispatchLC = ctxTFAT.LCMaster.Where(x => x.DispachFM == 0).Select(x => x.TableKey).ToList();
                foreach (var delRelation in delRelations)
                {
                    bool Block = false;
                    LRStock lR = ctxTFAT.LRStock.Where(x => x.TableKey == delRelation.ParentKey).FirstOrDefault();
                    if (deliveryMaster != null)
                    {
                        if (lR.Type.ToString() == "TRN")
                        {
                            FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey == lR.FMRefTablekey).FirstOrDefault();
                            if (fMMaster.FmStatus == "C")
                            {
                                Block = true;
                            }
                            else
                            {
                                if (fMMaster.ScheduleFollowup)
                                {
                                    var Areas = GetChildGrp(mbranchcode);
                                    if (!Areas.Contains(fMMaster.CurrBranch))
                                    {
                                        Block = true;
                                    }
                                }
                            }
                        }
                    }

                    var ConsumeStock = ctxTFAT.LRStock.Where(x => x.ParentKey == lR.TableKey).Sum(x => (int?)x.TotalQty) ?? 0;
                    var UndispatchAloocateStock = ctxTFAT.LCDetail.Where(x => x.ParentKey == lR.TableKey && UndispatchLC.Contains(x.LCRefTablekey)).Sum(x => (int?)x.LrQty) ?? 0;

                    DelRetion item = new DelRetion();
                    item.ParentKey = delRelation.ParentKey.ToString();
                    item.StkBranch = ctxTFAT.TfatBranch.Where(x => x.Code == delRelation.Branch).Select(x => x.Name).FirstOrDefault();
                    item.Type = delRelation.Type;
                    item.DelQty = delRelation.DelQty;
                    item.DelWeight = delRelation.DelWeight ?? 0;
                    item.DelBalQty = delRelation.DelQty + (lR.TotalQty - (ConsumeStock + UndispatchAloocateStock));
                    item.DelBalWeight = delRelation.DelWeight.Value + (((lR.TotalQty - (ConsumeStock + UndispatchAloocateStock)) * lR.ActWeight) / lR.TotalQty);
                    item.BlockDelivery = lR.AUTHORISE.Substring(0, 1) != "A" ? true : Block;
                    item.Authorise = lR.AUTHORISE.Substring(0, 1);
                    dels.Add(item);
                }
                mModel.DelRetions = dels;
                TempData["AllStock"] = delRelations;
                LCVM CurrentBranchVM = new LCVM();
                CurrentBranchVM.lCDetails = new List<LcDetailsVM>();
                mModel.CurrentBranch = CurrentBranchVM;

                LCVM AllBranchVM = new LCVM();
                AllBranchVM.lCDetails = new List<LcDetailsVM>();
                mModel.All = AllBranchVM;
            }
            else
            {
                mModel.DeliveryNo = "0";
                mModel.DocDate = DateTime.Now;

                LCVM CurrentBranchVM = new LCVM();
                CurrentBranchVM.lCDetails = new List<LcDetailsVM>();
                mModel.CurrentBranch = CurrentBranchVM;

                LCVM AllBranchVM = new LCVM();
                AllBranchVM.lCDetails = new List<LcDetailsVM>();
                TempData["AllStock"] = AllBranchVM.lCDetails;
                mModel.All = AllBranchVM;
            }
            mModel.CurrentBranchLR = true;
            return View(mModel);
        }
        public ActionResult SaveData(DeliveryVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    DeliveryMaster deliveryMaster = new DeliveryMaster();
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
                        var Demo = ctxTFAT.DeliveryMaster.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
                        if (mbranchcode != Demo.Branch)
                        {
                            transaction.Rollback();
                            return Json(new { Message = "Login Branch Does Not Match With Documnet Branch.", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    bool mAdd = true;

                    PODMaster pODMaster = new PODMaster();

                    if (ctxTFAT.DeliveryMaster.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault() != null)
                    {
                        deliveryMaster = ctxTFAT.DeliveryMaster.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
                        mAdd = false;
                        mnewrecordkey = deliveryMaster.DeliveryNo.ToString();
                        var DelRel = ctxTFAT.DelRelation.Where(x => x.DeliveryNo == deliveryMaster.DeliveryNo && x.Prefix == mperiod).ToList();
                        foreach (var item in DelRel)
                        {
                            LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey == item.ParentKey).FirstOrDefault();
                            if (lRStock.Type == "TRN")
                            {
                                FMMaster fM = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == lRStock.FMRefTablekey.ToString()).FirstOrDefault();
                                if (fM.FmStatus == "CC")
                                {
                                    return Json(new { Status = "Error", Message = "Not Allow To Edit Or Delete Delivery....! Because OF Fm Completed." }, JsonRequestBehavior.AllowGet);
                                }
                                if (fM.ActivityFollowup == true)
                                {
                                    if (mbranchcode != fM.CurrBranch)
                                    {
                                        return Json(new { Status = "Error", Message = "Not Allow To Edit Delivery....!\n Bcoz Fm Not In Our Branch...!" }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                        }
                        DeUpdate(mModel);
                    }

                    if (mAdd)
                    {
                        deliveryMaster = new DeliveryMaster();
                        deliveryMaster.DeliveryNo = mModel.DelGenerate == "A" ? Convert.ToInt32(GetNewCode()) : Convert.ToInt32(mModel.DeliveryNo);
                        deliveryMaster.GenerateType = mModel.DelGenerate;
                        deliveryMaster.CreateDate = DateTime.Now;
                        mnewrecordkey = deliveryMaster.DeliveryNo.ToString();
                        deliveryMaster.TableKey = mbranchcode + "DEL00" + mperiod.Substring(0, 2) + "001" + Convert.ToInt32(deliveryMaster.DeliveryNo).ToString("D6");

                    }

                    var ConsignmentKey = mModel.DelRetions.Select(x => x.ParentKey).FirstOrDefault();
                    LRStock lRStockConsignment = ctxTFAT.LRStock.Where(x => x.TableKey == ConsignmentKey).FirstOrDefault();
                    LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == lRStockConsignment.LRRefTablekey).FirstOrDefault();
                    deliveryMaster.LoginBranch = mbranchcode;
                    deliveryMaster.Branch = mbranchcode;
                    deliveryMaster.LrNO = Convert.ToInt32(mModel.Lrno);
                    deliveryMaster.DeliveryTime = mModel.DeliveryTime == null ? "00:00" : mModel.DeliveryTime;
                    deliveryMaster.DeliveryDate = ConvertDDMMYYTOYYMMDD(mModel.DeliveryDate);
                    deliveryMaster.Consigner = lRMaster.RecCode;
                    deliveryMaster.Consignee = lRMaster.SendCode;
                    deliveryMaster.FromBranch = lRMaster.Source;
                    deliveryMaster.ToBranch = lRMaster.Dest;
                    deliveryMaster.Qty = mModel.DelRetions.Sum(x => x.DelQty);
                    deliveryMaster.Weight = mModel.DelRetions.Sum(x => x.DelWeight);
                    deliveryMaster.DeliveryGoodStatus = mModel.DeliveryGoodStatus;
                    deliveryMaster.ShortQty = mModel.ShortQty;
                    deliveryMaster.DeliveryRemark = mModel.DeliveryRemark;
                    deliveryMaster.ParentKey = lRMaster.TableKey;
                    deliveryMaster.VehicleNO = mModel.VehicleNo;
                    deliveryMaster.BillQty = mModel.BillQty;
                    deliveryMaster.PersonName = mModel.PersonName;
                    deliveryMaster.MobileNO = (mModel.MobileNO);
                    deliveryMaster.Prefix = mperiod;

                    deliveryMaster.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    deliveryMaster.ENTEREDBY = muserid;
                    deliveryMaster.AUTHORISE = mauthorise;
                    deliveryMaster.AUTHIDS = muserid;

                    #region Update Vehicle Master
                    VehicleMaster vehicleMaster = ctxTFAT.VehicleMaster.Where(x => x.Code == mModel.VehicleNo).FirstOrDefault();
                    if (vehicleMaster != null)
                    {
                        vehicleMaster.Status = "Transit";
                        ctxTFAT.Entry(vehicleMaster).State = EntityState.Modified;
                    }
                    #endregion

                    if (mModel.Mode == "Add")
                    {
                        if (mModel.POD == "Yes")
                        {
                            pODMaster = new PODMaster();
                            #region Podmaster
                            pODMaster.CreateDate = DateTime.Now;
                            pODMaster.PODNo = Convert.ToInt32(GetPODNewCode());
                            pODMaster.TableKey = mbranchcode + "POD00" + mperiod.Substring(0, 2) + "001" + pODMaster.PODNo;
                            pODMaster.ParentKey = mbranchcode + "POD00" + mperiod.Substring(0, 2) + pODMaster.PODNo;
                            pODMaster.CurrentBranch = mbranchcode;
                            pODMaster.Prefix = mperiod;
                            pODMaster.Task = "Direct";
                            pODMaster.SendReceive = "R";
                            pODMaster.ModuleName = "Delivery";
                            pODMaster.FromBranch = mbranchcode;
                            pODMaster.PODDate = ConvertDDMMYYTOYYMMDD(mModel.PODDate);
                            pODMaster.PODTime = mModel.PODTime;
                            pODMaster.PODRemark = mModel.PODRemark;
                            pODMaster.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                            pODMaster.ENTEREDBY = muserid;
                            pODMaster.AUTHORISE = mauthorise;
                            pODMaster.AUTHIDS = muserid;
                            pODMaster.DeliveryNo = deliveryMaster.DeliveryNo;
                            #endregion

                            #region  PODRel
                            PODRel pODRel = new PODRel();
                            pODRel.Task = "Direct";
                            pODRel.SendReceive = "R";
                            pODRel.PODNo = pODMaster.PODNo.Value;
                            //pODRel.ChildNo = pODMaster.PODNo.Value;
                            pODRel.LrNo = Convert.ToInt32(mModel.Lrno);
                            pODRel.Prefix = mperiod;
                            pODRel.TableKey = mbranchcode + "PODRL" + mperiod.Substring(0, 2) + "001" + pODMaster.PODNo;
                            pODRel.ParentKey = pODMaster.TableKey;
                            pODRel.FromBranch = mbranchcode;
                            pODRel.ToBranch = mbranchcode;
                            pODRel.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                            pODRel.ENTEREDBY = muserid;
                            pODRel.AUTHIDS = muserid;
                            pODRel.AUTHORISE = mauthorise;
                            pODRel.LRRefTablekey = lRMaster.TableKey;
                            //var GetLastPOD = ctxTFAT.PODRel.OrderByDescending(x => x.TableKey).Where(x => x.LRRefTablekey == lRMaster.TableKey).FirstOrDefault();
                            var GetLastPOD = ctxTFAT.PODRel.OrderByDescending(x => x.RECORDKEY).Where(x => x.LRRefTablekey == lRMaster.TableKey && (x.SendReceive == "R" || x.SendReceive == "S")).FirstOrDefault();
                            if (GetLastPOD != null)
                            {
                                pODRel.PODRefTablekey = GetLastPOD.TableKey;
                            }
                            #endregion

                            string Athorise = "A00";
                            #region Authorisation
                            TfatUserAuditHeader authorisation = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == "POD00").FirstOrDefault();
                            if (authorisation != null)
                            {
                                Athorise = SetAuthorisationLogistics(authorisation, pODMaster.TableKey, pODMaster.PODNo.ToString(), 0, pODMaster.PODDate.ToShortDateString(), 0, "", mbranchcode);
                                pODMaster.AUTHORISE = Athorise;
                                pODRel.AUTHORISE = Athorise;
                            }
                            #endregion
                            ctxTFAT.PODMaster.Add(pODMaster);
                            ctxTFAT.PODRel.Add(pODRel);
                            PODNotification(pODMaster, pODRel, false);
                        }
                    }

                    #region Multiple Delivery

                    string Athorise1 = "A00";
                    #region Authorisation
                    TfatUserAuditHeader authorisation1 = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == "DELV0").FirstOrDefault();
                    if (authorisation1 != null)
                    {
                        Athorise1 = SetAuthorisationLogistics(authorisation1, deliveryMaster.TableKey, deliveryMaster.DeliveryNo.ToString(), 0, deliveryMaster.DeliveryDate.ToShortDateString(), 0, "", mbranchcode);
                        deliveryMaster.AUTHORISE = Athorise1;
                    }
                    #endregion



                    List<DelRelation> DelReList = ctxTFAT.DelRelation.Where(x => x.DeliveryNo == deliveryMaster.DeliveryNo && x.Prefix == deliveryMaster.Prefix).ToList();
                    if (DelReList == null)
                    {
                        DelReList = new List<DelRelation>();
                    }
                    var I = 0;
                    bool OtherBranchDel = false;
                    foreach (var item in mModel.DelRetions)
                    {
                        ++I;
                        LRStock Lrstock = ctxTFAT.LRStock.Where(x => x.TableKey == item.ParentKey).FirstOrDefault();
                        if (Lrstock.Branch != mbranchcode && OtherBranchDel == false)
                        {
                            OtherBranchDel = true;
                        }
                        var BalQty = ctxTFAT.LRStock.Where(x => x.ParentKey == Lrstock.TableKey && x.TableKey != deliveryMaster.TableKey).Sum(x => (int?)x.TotalQty) ?? 0;
                        var UnDispatchLC = ctxTFAT.LCMaster.Where(x => x.DispachFM == 0).Select(x => x.TableKey).ToList();
                        var LCLoadQty = ctxTFAT.LCDetail.Where(x => x.ParentKey == Lrstock.TableKey && UnDispatchLC.Contains(x.LCRefTablekey)).Sum(x => (int?)x.LrQty) ?? 0;
                        int Qty = 0; double Weight = 0;
                        Qty = item.DelQty;
                        Weight = item.DelWeight;
                        DelRelation delRelation = new DelRelation();
                        delRelation.DeliveryNo = deliveryMaster.DeliveryNo;
                        delRelation.Branch = Lrstock.Branch;
                        delRelation.Type = item.Type;
                        delRelation.ParentKey = item.ParentKey;
                        delRelation.DelQty = Qty;
                        delRelation.DelWeight = Weight;
                        delRelation.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                        delRelation.ENTEREDBY = muserid;
                        delRelation.AUTHORISE = Athorise1;
                        delRelation.AUTHIDS = muserid;
                        delRelation.Prefix = mperiod;

                        if ((Lrstock.TotalQty - (BalQty + LCLoadQty)) < (Qty))
                        {
                            return Json(new
                            {
                                Status = "failure",
                                Message = "LR No: " + Lrstock.LrNo + "  Available Only " + (((Lrstock.TotalQty - (BalQty + LCLoadQty)))) + " Qty.....!"
                            }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            Lrstock.BalQty -= Qty;
                            Lrstock.BalWeight -= Weight;
                            ctxTFAT.Entry(Lrstock).State = EntityState.Modified;
                            if (Lrstock.BalQty<0)
                            {
                                return Json(new
                                {
                                    Status = "failure",
                                    Message = "LR No: " + Lrstock.LrNo + "  Not Allowed To Delivery Due To Negative Stock .....!"
                                }, JsonRequestBehavior.AllowGet);
                            }
                            
                            ctxTFAT.DelRelation.Add(delRelation);
                        }


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
                    }
                    #endregion

                    if (mModel.Mode == "Add")
                    {
                        deliveryMaster.MultiDel = false;
                        ctxTFAT.DeliveryMaster.Add(deliveryMaster);

                    }
                    else
                    {
                        ctxTFAT.Entry(deliveryMaster).State = EntityState.Modified;
                    }



                    var KEy = mbranchcode + "DELV0" + mperiod.Substring(0, 2) + deliveryMaster.DeliveryNo;
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    DeliveryNotification(deliveryMaster, OtherBranchDel, "Delivery");
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, deliveryMaster.ParentKey, deliveryMaster.DeliveryDate, 0, "", " Save Delivery No:" + deliveryMaster.DeliveryNo, "NA");
                    SendSMS_MSG_Email(mModel.Mode, lRMaster.Val1, deliveryMaster.Branch + KEy, deliveryMaster.DeliveryDate, lRMaster.BillParty, "CA");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.InnerException.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", SerialNo = mnewrecordkey, id = "StateMaster" }, JsonRequestBehavior.AllowGet);
        }
        public void DeUpdate(DeliveryVM mModel)
        {
            var deliveryMaster = ctxTFAT.DeliveryMaster.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
            var Deliveryralation = ctxTFAT.DelRelation.Where(x => x.DeliveryNo == deliveryMaster.DeliveryNo && x.Prefix == mperiod).ToList();
            for (int i = 1; i <= Deliveryralation.Count(); i++)
            {
                var tablekey = mbranchcode + "DEL00" + mperiod.Substring(0, 2) + i.ToString("D3") + deliveryMaster.DeliveryNo;
                var OldDelStkEntry = ctxTFAT.LRStock.Where(x => x.TableKey == tablekey && x.Type == "DEL").FirstOrDefault();
                if (OldDelStkEntry != null)
                {
                    ctxTFAT.LRStock.Remove(OldDelStkEntry);
                }
            }

            foreach (var item in Deliveryralation)
            {
                LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey == item.ParentKey).FirstOrDefault();
                lRStock.BalQty += item.DelQty;
                lRStock.BalWeight += item.DelWeight.Value;
                ctxTFAT.Entry(lRStock).State = EntityState.Modified;
            }
            ctxTFAT.DelRelation.RemoveRange(Deliveryralation);
            ctxTFAT.SaveChanges();
        }
        public string DeleteStateMaster(DeliveryVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return "Code not Entered..";
            }

            var ChildList = GetChildGrp(mbranchcode);
            var deliveryMaster = ctxTFAT.DeliveryMaster.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
            var podmaster = ctxTFAT.PODMaster.Where(x => x.DeliveryNo == deliveryMaster.DeliveryNo && x.Prefix == deliveryMaster.Prefix).FirstOrDefault();
            var DeliveryRelList = ctxTFAT.DelRelation.Where(x => x.DeliveryNo == deliveryMaster.DeliveryNo && x.Prefix == deliveryMaster.Prefix).ToList();

            #region Delete Old DeliveryStk Entry
            var GetParentKeyOfDeliveryLRNO = ctxTFAT.DelRelation.Where(x => x.DeliveryNo == deliveryMaster.DeliveryNo).ToList().Count();
            for (int i = 1; i <= GetParentKeyOfDeliveryLRNO; i++)
            {
                var tablekey = mbranchcode + "DEL00" + mperiod.Substring(0, 2) + i.ToString("D3") + deliveryMaster.DeliveryNo;
                var OldDelStkEntry = ctxTFAT.LRStock.Where(x => x.TableKey == tablekey && x.Type == "DEL").FirstOrDefault();
                if (OldDelStkEntry != null)
                {
                    ctxTFAT.LRStock.Remove(OldDelStkEntry);
                }
            }
            #endregion

            if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.Type == "DELV0" && x.LockDate == deliveryMaster.DeliveryDate).FirstOrDefault() != null)
            {
                return "This Delivery Date Locked By Period Locking System..";
            }

            foreach (var item in DeliveryRelList)
            {
                LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey.ToString().Trim() == item.ParentKey.Trim()).FirstOrDefault();

                if (lRStock.Type == "TRN")
                {
                    FMMaster fM = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == lRStock.FMRefTablekey.ToString()).FirstOrDefault();
                    if (fM.FmStatus == "CC")
                    {
                        return "Not Allow To Delete Delivery....!\n Because OF Fm Completed.";
                    }
                    if (fM.ActivityFollowup == true)
                    {
                        if (!ChildList.Contains(fM.CurrBranch))
                        {
                            return "Not Allow To Delete Delivery....! \n Bcoz Fm Not In Our Branch...!";
                        }
                    }

                }

                lRStock.BalQty += item.DelQty;
                lRStock.BalWeight += item.DelWeight.Value;
                ctxTFAT.Entry(lRStock).State = EntityState.Modified;

                ctxTFAT.DelRelation.Remove(item);
            }

            var AuthorisationEntry = ctxTFAT.Authorisation.Where(x => x.Type == "DELV0" && x.Srl == deliveryMaster.DeliveryNo.ToString()).FirstOrDefault();
            if (AuthorisationEntry != null)
            {
                ctxTFAT.Authorisation.Remove(AuthorisationEntry);
            }
            string mSQLQuery = "with Demo as( SELECT  RECORDKEY,value  FROM FMROUTETable CROSS APPLY STRING_SPLIT(Deliveries, ',') where Deliveries is not null and Deliveries<>'') select RECORDKEY from Demo where Value='" + deliveryMaster.TableKey + "'";
            connstring = GetConnectionString();
            SqlConnection conn = new SqlConnection(connstring);
            SqlCommand cmd = new SqlCommand(mSQLQuery, conn);
            try
            {
                conn.Open();
                cmd.CommandTimeout = 0;
                Int32 CustomerCnt = (Int32)cmd.ExecuteScalar();
                if (CustomerCnt > 0)
                {
                    FMROUTETable fMROUTETable = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY == CustomerCnt).FirstOrDefault();
                    if (fMROUTETable != null)
                    {
                        if (!String.IsNullOrEmpty(fMROUTETable.Deliveries))
                        {
                            string NewString = "";
                            var SplitData = fMROUTETable.Deliveries.Split(',');
                            foreach (var item in SplitData)
                            {
                                if (item.Trim() != deliveryMaster.TableKey.Trim())
                                {
                                    NewString += item + ",";
                                }
                            }
                            if (!String.IsNullOrEmpty(NewString))
                            {
                                NewString = NewString.Substring(0, NewString.Length - 1);
                            }

                            fMROUTETable.Deliveries = NewString;
                            ctxTFAT.Entry(fMROUTETable).State = EntityState.Modified;
                        }
                    }
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







            ctxTFAT.DeliveryMaster.Remove(deliveryMaster);
            if (podmaster != null)
            {
                podmaster.DeliveryNo = null;
                ctxTFAT.Entry(podmaster).State = EntityState.Modified;
            }

            var DelAttachment = ctxTFAT.Attachment.Where(x => x.Type == "LR000" && x.Srl == deliveryMaster.LrNO.ToString() && x.RefType.Trim() == "Delivery").ToList();
            foreach (var item in DelAttachment)
            {
                if (System.IO.File.Exists(item.FilePath))
                {
                    System.IO.File.Delete(item.FilePath);
                }
            }
            ctxTFAT.Attachment.RemoveRange(DelAttachment);


            ctxTFAT.SaveChanges();
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, deliveryMaster.ParentKey, deliveryMaster.DeliveryDate, 0, "", "Delete Delivery No:" + deliveryMaster.DeliveryNo, "NA");
            return "Success";
        }


        #region Combo Function
        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
            return GetGridDataColumns(id, "L", "XXXX");
        }

        public ActionResult GetGridData(GridOption Model)
        {

            if (!String.IsNullOrEmpty(Model.searchString) && Model.searchString != "undefined")
            {
                Model.searchOper = "cn";
                Model.searchField = " LrNo  ";
            }


            ExecuteStoredProc("Drop Table tempDelivery1");
            ///string osadjnarr = String.Join(",", Model.MasterList);
            //ppara05 = ppara05 == null || ppara05 == "" ? "'" + mbranchcode + "'" : ppara05;ExecuteStoredProc("Drop Table ztmp_TempMth");
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("SP_BranchWisePendingStock", tfat_conx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@BranchList", SqlDbType.VarChar).Value = mbranchcode;
            //cmd.Parameters.Add("@TableKetList", SqlDbType.VarChar).Value = Model.mVar4;

            cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";

            cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;

            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();

            string mSelectQuery = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");

            tfat_conx.Close();

            return GetGridReport(Model, "M", "", false, 0);
        }
        public ActionResult GetGridData1(GridOption Model)
        {

            if (!String.IsNullOrEmpty(Model.searchString) && Model.searchString != "undefined")
            {
                Model.searchOper = "cn";
                Model.searchField = " LrNo  ";
            }


            ExecuteStoredProc("Drop Table tempDelivery");
            ///string osadjnarr = String.Join(",", Model.MasterList);
            //ppara05 = ppara05 == null || ppara05 == "" ? "'" + mbranchcode + "'" : ppara05;ExecuteStoredProc("Drop Table ztmp_TempMth");
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("SP_AllBranchStock_Delivery", tfat_conx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;

            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();

            string mSelectQuery = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");

            tfat_conx.Close();

            return GetGridReport(Model, "M", "", false, 0);
        }
        public ActionResult GetGridData2(GridOption Model)
        {

            if (!String.IsNullOrEmpty(Model.searchString) && Model.searchString != "undefined")
            {
                Model.searchOper = "cn";
                Model.searchField = " LrNo  ";
            }

            ExecuteStoredProc("Drop Table tempDelivery2");
            ///string osadjnarr = String.Join(",", Model.MasterList);
            //ppara05 = ppara05 == null || ppara05 == "" ? "'" + mbranchcode + "'" : ppara05;ExecuteStoredProc("Drop Table ztmp_TempMth");
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("SP_SearchLR_AllBranchStock_Delivery", tfat_conx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@LRNO", SqlDbType.VarChar).Value = Model.Value3;
            cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;

            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();

            string mSelectQuery = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");

            tfat_conx.Close();

            return GetGridReport(Model, "M", "", false, 0);
        }
        public ActionResult ShowLRDeliveryDetails(DeliveryVM mModel)
        {
            //mModel.OtherBranchLr = false;
            LRMaster lRMaster = new LRMaster();
            mModel.DeliverySetup = ctxTFAT.DeliverySetup.Where(x => (x.RECORDKEY == 1)).FirstOrDefault();
            if (mModel.DeliverySetup == null)
            {
                mModel.DeliverySetup = new DeliverySetup();
            }
            if (mModel.DeliverySetup.CurrDatetOnlyreq == false && mModel.DeliverySetup.BackDateAllow == false && mModel.DeliverySetup.ForwardDateAllow == false)
            {
                mModel.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
                mModel.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
            }
            if (mModel.DeliverySetup.CurrDatetOnlyreq == true)
            {
                mModel.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
                mModel.EndDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            if (mModel.DeliverySetup.BackDateAllow == true)
            {
                mModel.StartDate = (DateTime.Now.AddDays(-mModel.DeliverySetup.BackDaysUpto)).ToString("yyyy-MM-dd");
            }
            if (mModel.DeliverySetup.ForwardDateAllow == true)
            {
                mModel.EndDate = (DateTime.Now.AddDays(mModel.DeliverySetup.ForwardDaysUpto)).ToString("yyyy-MM-dd");
            }
            List<string> BranchChild = new List<string>();
            if (mbranchcode != "HO0000")
            {
                BranchChild = GetChildGrp(mbranchcode);
            }

            var lcDetailsVM = new LcDetailsVM();
            if (!String.IsNullOrEmpty(mModel.Lrno))
            {
                LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey == mModel.Lrno).FirstOrDefault();
                lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == lRStock.LRRefTablekey).FirstOrDefault();
                mModel.OtherBranchLr = mbranchcode == "HO0000" ? false : BranchChild.Contains(lRMaster.Dest) == true ? false : true;
                if (mModel.DeliverySetup.PODReceived)
                {
                    var POdRel = ctxTFAT.PODRel.Where(x => x.ParentKey.ToString().Trim() == lRMaster.TableKey.ToString().Trim()).FirstOrDefault();
                    if (POdRel != null)
                    {
                        mModel.POD = "No";
                    }
                    else
                    {
                        mModel.POD = "Yes";
                    }
                }
                else
                {
                    mModel.POD = "No";
                }

                if (lRMaster != null)
                {
                    mModel.Lrno = lRMaster.LrNo.ToString();
                    mModel.Time = lRMaster.Time;
                    mModel.Date = lRMaster.BookDate.ToShortDateString();
                    mModel.ConsigneeName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.SendCode).Select(x => x.Name).FirstOrDefault();
                    mModel.ConsignerName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.RecCode).Select(x => x.Name).FirstOrDefault();
                    mModel.FromName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Source).Select(x => x.Name).FirstOrDefault();
                    mModel.ToName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name).FirstOrDefault();
                    mModel.LRQty = lRMaster.TotQty;
                    mModel.LRWeight = lRMaster.ActWt;
                    mModel.RecordKey = lRMaster.LrNo.ToString();
                }
            }
            mModel.DeliveryRemark = lcDetailsVM == null ? "OK" : lcDetailsVM.remark;
            //mModel.DeliveryTime = mModel.Time;//DateTime.Now.ToString("HH:mm");
            mModel.DeliveryDate = mModel.Date;// DateTime.Now.ToShortDateString();
            mModel.PODTime = mModel.Time;// DateTime.Now.ToString("HH:mm");
            mModel.PODDate = mModel.Date;// mModel.Date;// DateTime.Now.ToShortDateString();
            mModel.ShortQty = 0;
            mModel.DocDate = DateTime.Now;
            var PONO = ctxTFAT.PODMaster.OrderByDescending(x => x.PODNo).Select(x => x.PODNo).FirstOrDefault();
            if (PONO == 0 || PONO == null)
            {
                var DocType = ctxTFAT.DocTypes.Where(x => x.Code == "POD00").Select(x => new { x.DocWidth, x.LimitFrom }).FirstOrDefault();
                mModel.PODNo = DocType.LimitFrom;
            }
            else
            {
                mModel.PODNo = (PONO + 1).ToString();
            }

            if (mModel.CurrentBranchLR)
            {
                string connstring = GetConnectionString();
                SqlDataAdapter da = new SqlDataAdapter();
                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand();
                int LRNo = mModel.Lrno == null ? 0 : Convert.ToInt32(mModel.Lrno);

                SqlConnection con = new SqlConnection(connstring);
                string Query = " select LRSTK.Branch As Branch, LRSTK.Tablekey As TableKey ,( select Name From TfatBranch where Code=LRSTK.Branch) As BranchN,LRSTK.LrNo , case When LRSTK.Type='LR' then substring(LRSTK.AUTHORISE,0,2) Else 'A' End  AS Authenticate," +
                               " LRSTK.type As StkType,	case when LRSTK.TotalQty=0 then 0 else ( LRSTK.TotalQty-(  ((select  ISNULL(SUM(LRT.TotalQty),0) from LRStock LRT where LRT.ParentKey=LRSTK.TableKey)) +(select ISNULL(SUM(lcd.lrqty),0) from lcdetail lcd where lcd.parentkey=LRSTK.tablekey and  lcd.LCRefTablekey in (select LCM.TableKey from lcmaster LCM where LCm.DispachFM=0))  ) ) end as BalQty" +
                               " from LRStock LRSTK where lrstk.LRRefTablekey='" + lRMaster.TableKey + "' and Branch in (SELECT * FROM SP_GetBranchChild('" + mbranchcode + "')) and LRSTK.type<>'DEL' and (case when LRSTK.TotalQty=0 then 0 else ( LRSTK.TotalQty-(  ((select  ISNULL(SUM(LRT.TotalQty),0) from LRStock LRT where LRT.ParentKey=LRSTK.TableKey)) +(select ISNULL(SUM(lcd.lrqty),0) from lcdetail lcd where lcd.parentkey=LRSTK.tablekey and  lcd.LCRefTablekey in (select LCM.Tablekey from lcmaster LCM where LCm.DispachFM=0))  ) ) end ) <>0 ";
                cmd = new SqlCommand(Query, con);
                con.Open();
                da.SelectCommand = cmd;
                da.Fill(dt);
                con.Close();
                con.Dispose();

                var Mobj = GetDeliveryDetails(dt);

                Mobj = Mobj.Where(x => x.DelBalQty > 0).ToList();
                Mobj.ForEach(x =>
                {
                    x.DelBalWeight = /*Math.Round*/((mModel.LRWeight / mModel.LRQty) * (x.DelBalQty));
                    x.DelQty = x.Authorise == "A" ? x.DelBalQty : 0;
                    x.DelWeight = x.Authorise == "A" ? /*Math.Round*/((mModel.LRWeight / mModel.LRQty) * (x.DelBalQty)) : 0;
                });
                mModel.DelRetions = Mobj;
                var html = ViewHelper.RenderPartialView(this, "_ShowLRDeliveryDetails", mModel);
                return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            }
            else
            {

                string connstring = GetConnectionString();
                SqlDataAdapter da = new SqlDataAdapter();
                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand();
                SqlConnection con = new SqlConnection(connstring);

                int LRNo = mModel.Lrno == null ? 0 : Convert.ToInt32(mModel.Lrno);
                string Query = "";
                Query = "select LRSTK.Branch As Branch,LRSTK.Tablekey As TableKey ,( select Name From TfatBranch where Code=LRSTK.Branch) As BranchN,LRSTK.LrNo , case When LRSTK.Type='LR' then substring(LRSTK.AUTHORISE,0,2) Else 'A' End  AS Authenticate, LRSTK.type As StkType,	" +
                    "case when LRSTK.TotalQty=0 then 0 else ( LRSTK.TotalQty-(  ((select  ISNULL(SUM(LRT.TotalQty),0) from LRStock LRT where LRT.ParentKey=LRSTK.TableKey)) +(select ISNULL(SUM(lcd.lrqty),0) from lcdetail lcd where lcd.parentkey=LRSTK.tablekey and  lcd.LCRefTablekey in (select LCM.Tablekey from lcmaster LCM where LCm.DispachFM=0))  ) ) end as BalQty " +
                    " from LRStock LRSTK where lrstk.LRRefTablekey= '" + lRMaster.TableKey + "'  and LRSTK.type<>'DEL' and (case when LRSTK.TotalQty=0 then 0 else ( LRSTK.TotalQty-(  ((select  ISNULL(SUM(LRT.TotalQty),0) from LRStock LRT where LRT.ParentKey=LRSTK.TableKey)) +(select ISNULL(SUM(lcd.lrqty),0) from lcdetail lcd where lcd.parentkey=LRSTK.tablekey and  lcd.LCRefTablekey in (select LCM.Tablekey from lcmaster LCM where LCm.DispachFM=0))  ) ) end ) <>0 ";
                cmd = new SqlCommand(Query, con);
                con.Open();
                da.SelectCommand = cmd;
                da.Fill(dt);
                con.Close();
                con.Dispose();

                var Mobj = GetDeliveryDetails(dt);

                Mobj = Mobj.Where(x => x.DelBalQty > 0).ToList();

                Mobj.ForEach(x =>
                {
                    x.DelBalWeight = /*Math.Round*/((mModel.LRWeight / mModel.LRQty) * (x.DelBalQty));
                    x.DelQty = x.Authorise == "A" ? x.DelBalQty : 0;
                    x.DelWeight = x.Authorise == "A" ? /*Math.Round*/((mModel.LRWeight / mModel.LRQty) * (x.DelBalQty)) : 0;
                });
                var BranchStok = Mobj.Select(x => x.StkBranchN).ToList();
                foreach (var item in BranchStok)
                {
                    if (mModel.OtherBranchLr == false)
                    {
                        mModel.OtherBranchLr = mbranchcode == "HO0000" ? false : BranchChild.Contains(item) == true ? false : true;
                    }
                    else
                    {
                        break;
                    }
                }

                mModel.DelRetions = Mobj;
                var html = ViewHelper.RenderPartialView(this, "_ShowLRDeliveryDetails", mModel);
                return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            }
        }

        public List<DelRetion> GetDeliveryDetails(DataTable table)
        {
            var categoryList = new List<DelRetion>(table.Rows.Count);
            foreach (DataRow row in table.Rows)
            {
                var values = row.ItemArray;
                var category = new DelRetion()
                {
                    ParentKey = Convert.ToString(values[1]),
                    StkBranchN = Convert.ToString(values[0]),
                    StkBranch = Convert.ToString(values[2]),
                    Type = Convert.ToString(values[5]),
                    DelBalQty = Convert.ToInt32(values[6]),
                    Authorise = Convert.ToString(values[4]).Substring(0, 1),
                    BlockDelivery = Convert.ToString(values[4]).Substring(0, 1) == "A" ? false : true,
                };
                categoryList.Add(category);
            }

            return categoryList;
        }

        #endregion


        #region AlertNote Stop CHeck

        public ActionResult CheckStopAlertNote(string Type, List<string> TypeCode, string DocTpe, bool POD)
        {
            string Status = "Success", Message = "";

            var GetConsignmentKey = ctxTFAT.LRStock.Where(x => TypeCode.Contains(x.TableKey)).Select(x => x.LRRefTablekey).ToList();

            var Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && GetConsignmentKey.Contains(AlertMater.ParentKey) && AlertMater.RefType.Contains(DocTpe)
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
                        Message += item.TypeCode + " Not Allowed To Delivery Of This Consignment Please Remove IT....\n";
                        break;
                    }
                }
            }

            if (POD)

            {
                Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && GetConsignmentKey.Contains(AlertMater.ParentKey) && AlertMater.RefType.Contains("POD00")
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
                            Message += item.TypeCode + " Not Allowed TO Create POD OF This Consignment Please Remove IT....\n";
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

    }
}