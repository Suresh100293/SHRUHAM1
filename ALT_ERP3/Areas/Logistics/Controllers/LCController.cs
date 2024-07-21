using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using Common;
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
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class LCController : BaseController
    {

        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        private new string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private new string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        public new string muserid = System.Web.HttpContext.Current.Session["UserId"].ToString();
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

        #region Function List

        public List<TfatBranch> GetBranch(string BRanchCode)
        {
            var mTreeList = ctxTFAT.TfatBranch.Select(x => new { x.Name, x.Grp, x.Code }).ToList();

            List<NFlatObject> flatObjects2 = new List<NFlatObject>();
            List<TfatBranch> GEtArea = new List<TfatBranch>();
            for (int n = 0; n < mTreeList.Count; n++)
            {
                NFlatObject abc = new NFlatObject();
                abc.data = mTreeList[n].Name;
                abc.Id = mTreeList[n].Code;
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
            var recursiveObjects = FillRecursive1(flatObjects2, BRanchCode);

            var Currentbranch = ctxTFAT.TfatBranch.Where(x => x.Code == BRanchCode).FirstOrDefault();
            GEtArea.Add(Currentbranch);
            foreach (var item in recursiveObjects)
            {
                var branch = ctxTFAT.TfatBranch.Where(x => x.Code == item.id).FirstOrDefault();
                GEtArea.Add(branch);
                if (item.children.Count > 0)
                {
                    foreach (var item1 in item.children)
                    {
                        var branch1 = ctxTFAT.TfatBranch.Where(x => x.Code == item1.id).FirstOrDefault();
                        GEtArea.Add(branch1);
                    }
                }
            }
            return GEtArea;
        }

        public static List<NRecursiveObject> FillRecursive1(List<NFlatObject> flatObjects, string parentId)
        {
            bool mSelected = false;
            List<NRecursiveObject> recursiveObjects = new List<NRecursiveObject>();
            foreach (var item in flatObjects.Where(x => x.ParentId.Equals(parentId)))
            {
                mSelected = item.isSelected;
                recursiveObjects.Add(new NRecursiveObject
                {
                    data = item.data,
                    id = item.Id,
                    attr = new NFlatTreeAttribute { id = item.Id.ToString(), selected = item.isSelected },
                    children = FillRecursive1(flatObjects, item.Id)
                });
            }
            return recursiveObjects;
        }

        public ActionResult CheckExistLC(string TableName, string Colfield, string Value, string SkipColumnName, string PKValue, string ExtraColumn, string ExtraValue, string ExtraColumn2, string ExtraValue2)
        {
            int count = 0;
            string message = "";
            DataTable dt = new DataTable();
            SqlDataAdapter sda = new SqlDataAdapter();
            string connstring = GetConnectionString();
            SqlConnection con = new SqlConnection(connstring);
            if (ExtraColumn == null && ExtraValue == null && ExtraColumn2 == null && ExtraValue2 == null)
            {
                SqlCommand cmd = new SqlCommand("Select * from " + TableName + " where " + Colfield + " = '" + Value + "' and " + SkipColumnName + "!='" + PKValue + "'", con);
                sda = new SqlDataAdapter("Select * from " + TableName + " where " + Colfield + " = '" + Value + "' and " + SkipColumnName + "!='" + PKValue + "'", con);
            }
            else if (ExtraColumn2 != null && ExtraValue2 != null)
            {
                SqlCommand cmd = new SqlCommand("Select * from " + TableName + " where " + Colfield + " = '" + Value + "' and " + ExtraColumn + "='" + ExtraValue + "'  and " + ExtraColumn2 + "='" + ExtraValue2 + "' and " + SkipColumnName + "!='" + PKValue + "'", con);
                sda = new SqlDataAdapter("Select * from " + TableName + " where " + Colfield + " = '" + Value + "' and " + ExtraColumn + "='" + ExtraValue + "' and " + ExtraColumn2 + "='" + ExtraValue2 + "' and " + SkipColumnName + "!='" + PKValue + "'", con);
            }
            else
            {
                SqlCommand cmd = new SqlCommand("Select * from " + TableName + " where " + Colfield + " = '" + Value + "' and " + ExtraColumn + "='" + ExtraValue + "' and " + SkipColumnName + "!='" + PKValue + "'", con);
                sda = new SqlDataAdapter("Select * from " + TableName + " where " + Colfield + " = '" + Value + "' and " + ExtraColumn + "='" + ExtraValue + "' and " + SkipColumnName + "!='" + PKValue + "'", con);
            }
            sda.Fill(dt);
            con.Close();
            count = dt.Rows.Count;
            if (count == 0)
            {
                message = "T";
            }
            else
            {
                message = "F";
            }
            return Json(new { Message = message, JsonRequestBehavior.AllowGet });
        }

        public JsonResult From(string term)
        {
            var FillFromCombo = ctxTFAT.LCSetup.Select(x => (bool?)x.FillFromCurr ?? false).FirstOrDefault();
            List<TfatBranch> list = new List<TfatBranch>();
            if (FillFromCombo)
            {
                //HOD Zone Branch SubBranch
                list = ctxTFAT.TfatBranch.Where(x => (x.Status == true && x.Code != "G00000" && x.Grp != "G00000") && (x.Code == mbranchcode || x.Grp == mbranchcode)).ToList();

                var ZoneList = list.Where(x => x.Category == "Zone").Select(x => x.Code).ToList();
                list.AddRange(ctxTFAT.TfatBranch.Where(x => ZoneList.Contains(x.Code) || ZoneList.Contains(x.Grp)).ToList());

                var BranchList = list.Where(x => x.Category == "Branch").Select(x => x.Code).ToList();
                list.AddRange(ctxTFAT.TfatBranch.Where(x => BranchList.Contains(x.Code) || BranchList.Contains(x.Grp)).ToList());

                var SubBranchList = list.Where(x => x.Category == "SubBranch").Select(x => x.Code).ToList();
                list.AddRange(ctxTFAT.TfatBranch.Where(x => SubBranchList.Contains(x.Code) || SubBranchList.Contains(x.Grp)).ToList());

                //list = GetBranch(mbranchcode);

                var GeneralArea = ctxTFAT.TfatBranch.Where(x => x.Grp == "G00000" && x.Category == "Area" && x.Status == true).OrderBy(x => x.Name).ToList();
                list.AddRange(GeneralArea);
            }
            else
            {
                list = ctxTFAT.TfatBranch.Where(x => x.Status == true && x.Code != "G00000").ToList();
            }
            var Newlist = list.Distinct();
            if (!(String.IsNullOrEmpty(term)))
            {
                Newlist = Newlist.Where(x => x.Name.ToLower().Contains(term.ToLower())).OrderBy(x => x.Name).Take(10).ToList();
            }

            Newlist.Where(w => w.Category == "Zone").ToList().ForEach(w => w.Name = w.Name + " - Z");
            Newlist.Where(w => w.Category == "0").ToList().ForEach(w => w.Name = w.Name + " - HO");
            Newlist.Where(w => w.Category == "Branch").ToList().ForEach(w => w.Name = w.Name + " - B");
            Newlist.Where(w => w.Category == "SubBranch").ToList().ForEach(w => w.Name = w.Name + " - SB");
            Newlist.Where(w => w.Category == "Area").ToList().ForEach(w => w.Name = w.Name + " - A");


            var Modified = Newlist.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);

        }

        public JsonResult To(string term)
        {
            List<TfatBranch> list = ctxTFAT.TfatBranch.Where(x => x.Status == true && x.Code != "G00000").OrderBy(x => x.Name).ToList();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).OrderBy(x => x.Name).ToList();
            }

            list.Where(w => w.Category == "Zone").ToList().ForEach(w => w.Name = w.Name + " - Z");
            list.Where(w => w.Category == "0").ToList().ForEach(w => w.Name = w.Name + " - HO");
            list.Where(w => w.Category == "Branch").ToList().ForEach(w => w.Name = w.Name + " - B");
            list.Where(w => w.Category == "SubBranch").ToList().ForEach(w => w.Name = w.Name + " - SB");
            list.Where(w => w.Category == "Area").ToList().ForEach(w => w.Name = w.Name + " - A");

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public string ConvertDDMMYYTOYYMMDD1(string da)
        {
            string abc = da.Substring(6, 4) + "/" + da.Substring(3, 2) + "/" + da.Substring(0, 2);
            return abc;
        }

        public DateTime ConvertDDMMYYTOYYMMDD(string da)
        {
            string abc = da.Substring(6, 4) + "/" + da.Substring(3, 2) + "/" + da.Substring(0, 2);
            return Convert.ToDateTime(abc);
        }

        public ActionResult CheckLCDate(string DocDate, string DocTime)
        {
            string message = "", Status = "T";
            var Date = DocDate.Split('/');
            string[] Time = DocTime.Split(':');
            DateTime Lcdate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Time[0]), Convert.ToInt32(Time[1]), 00);
            LCSetup lCSetup = ctxTFAT.LCSetup.FirstOrDefault();
            //if (lCSetup!=null)
            //{
            //    if (lCSetup.LCDate == false)
            //    {
            //        var MinDate = DateTime.Now.AddHours(lCSetup.BeforeLCDate * (-1));
            //        var MaxDate = DateTime.Now.AddHours(lCSetup.AfterLCDate);

            //        if (MinDate <= Lcdate && Lcdate <= MaxDate)
            //        {
            //            Status = "T";
            //        }
            //        else
            //        {
            //            Status = "F";
            //            message = "LCDATE And TIME NOT ALLOW AS PER THE SETUP RULE...!";
            //        }
            //    }
            //    else
            //    {
            //        if (DateTime.Now.ToShortDateString() != Lcdate.ToShortDateString())
            //        {
            //            Status = "F";
            //            message = "Lorry Challan Date Allow Only Todays Date AS PER THE SETUP RULE...!";
            //        }
            //    }
            //}
            if (Status == "T")
            {
                var NewDocDate = ConvertDDMMYYTOYYMMDD(Lcdate.ToShortDateString());
                if (ConvertDDMMYYTOYYMMDD(StartDate) <= NewDocDate && NewDocDate <= ConvertDDMMYYTOYYMMDD(EndDate))
                {
                    Status = "T";
                }
                else
                {
                    Status = "F";
                    message = "Financial Date Range Allow Only...!";
                }
            }
            if (Status == "T")
            {
                var NewDate = ConvertDDMMYYTOYYMMDD(DocDate);
                if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.Type == "LC000" && x.LockDate == NewDate).FirstOrDefault() != null)
                {
                    Status = "F";
                    message = "LC Date is Locked By Period Lock System...!";
                }
            }
            return Json(new { Status = Status, Message = message, JsonRequestBehavior.AllowGet });
        }

        public List<SelectListItem> GetPickList()
        {
            List<SelectListItem> CallBaseGrList = new List<SelectListItem>();
            CallBaseGrList.Add(new SelectListItem { Value = "Godown", Text = "Godown" });
            CallBaseGrList.Add(new SelectListItem { Value = "Direct", Text = "Direct" });
            CallBaseGrList.Add(new SelectListItem { Value = "Crossing", Text = "Crossing" });
            return CallBaseGrList;
        }

        public List<SelectListItem> GetDeliveryList()
        {
            List<SelectListItem> CallBaseGrList = new List<SelectListItem>();
            CallBaseGrList.Add(new SelectListItem { Value = "Godown", Text = "Godown" });
            CallBaseGrList.Add(new SelectListItem { Value = "Direct", Text = "Direct" });
            CallBaseGrList.Add(new SelectListItem { Value = "Crossing", Text = "Crossing" });
            return CallBaseGrList;
        }

        public ActionResult CheckManual(int No, string document)
        {
            string Flag = "F";
            string Msg = "";
            bool checkalloctionFound = false;

            LCSetup lCSetup = ctxTFAT.LCSetup.FirstOrDefault();

            if (lCSetup.ManualLcCheck)
            {
                List<TblBranchAllocation> tblBranchAllocations = new List<TblBranchAllocation>();
                if (lCSetup.CetralisedManualSrlReq)
                {
                    tblBranchAllocations = ctxTFAT.TblBranchAllocation.Where(x => x.Branch == mbranchcode && x.TRN_Type == "LC000").ToList();
                }
                else
                {
                    tblBranchAllocations = ctxTFAT.TblBranchAllocation.Where(x => x.Prefix == mperiod && x.Branch == mbranchcode && x.TRN_Type == "LC000").ToList();
                }
                foreach (var item in tblBranchAllocations)
                {
                    if (item.ManualFrom <= No && No <= item.ManualTo)
                    {
                        checkalloctionFound = true;
                        break;
                    }
                }
            }
            else
            {
                checkalloctionFound = true;
            }


            if (checkalloctionFound)
            {
                LCMaster lCMaster = ctxTFAT.LCMaster.Where(x => x.LCno == No && x.LCno.ToString() != document).FirstOrDefault();
                if (lCMaster != null)
                {
                    Flag = "T";
                    Msg = "This LCNo Exist \nSo,Please Change LC No....!";
                }
                else
                {
                    var result = ctxTFAT.DocTypes.Where(x => x.Code == "LC000").Select(x => x).FirstOrDefault();
                    if (No.ToString().Length > (result.DocWidth))
                    {
                        Flag = "T";
                        Msg = "LC NO. Allow " + result.DocWidth + " Digit Only....!";
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

            LCMaster lCMaster = ctxTFAT.LCMaster.Where(x => x.LCno == No && x.Prefix == mperiod && x.TableKey.ToString() != document).FirstOrDefault();
            if (lCMaster != null)
            {
                Flag = "T";
                Msg = "Please Change Lr No....!";
            }
            return Json(new { Message = Flag, Msg = Msg, JsonRequestBehavior.AllowGet });
        }

        public string GetNewCode()
        {
            var mPrevSrl = GetLastSerial("LCMaster", mbranchcode, "LC000", mperiod, "RP", DateTime.Now.Date);

            //var NewLcNo = ctxTFAT.LCMaster.Where(x => x.Prefix == mperiod).OrderByDescending(x => x.LCno).Select(x => x.LCno).Take(1).FirstOrDefault();
            //var DocType = ctxTFAT.DocTypes.Where(x => x.Code == "LC000").Select(x => new { x.DocWidth, x.LimitFrom }).FirstOrDefault();
            //int LcNo;
            //if (NewLcNo == 0)
            //{
            //    LcNo = Convert.ToInt32(DocType.LimitFrom);
            //}
            //else
            //{
            //    LcNo = Convert.ToInt32(NewLcNo) + 1;

            //}
            return mPrevSrl.ToString();
        }

        #endregion

        // GET: Logistics/LC
        public ActionResult Index(LCVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            if (mModel.Mode=="Add")
            {
                UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            }
            else
            {
                var Document = ctxTFAT.LCMaster.Where(x => (x.TableKey.ToString() == mModel.Document)).FirstOrDefault();
                UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, Document.ParentKey, Document.Date, 0, "", "", "NA");
            }
            mdocument = mModel.Document;
            Session["CommnNarrlist"] = null;
            TempData.Remove("Consignments");
            mModel.LCSetup = ctxTFAT.LCSetup.FirstOrDefault();
            if (mModel.LCSetup == null)
            {
                mModel.LCSetup = new LCSetup();
            }
            if (mModel.LCSetup.CurrDatetOnlyreq == false && mModel.LCSetup.BackDateAllow == false && mModel.LCSetup.ForwardDateAllow == false)
            {
                mModel.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
                mModel.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
            }
            if (mModel.LCSetup.CurrDatetOnlyreq == true)
            {
                mModel.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
                mModel.EndDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            if (mModel.LCSetup.BackDateAllow == true)
            {
                mModel.StartDate = (DateTime.Now.AddDays(-mModel.LCSetup.BackDaysUpto)).ToString("yyyy-MM-dd");
            }
            if (mModel.LCSetup.ForwardDateAllow == true)
            {
                mModel.EndDate = (DateTime.Now.AddDays(mModel.LCSetup.ForwardDaysUpto)).ToString("yyyy-MM-dd");
            }
            List<StockDetails> lCDetails = new List<StockDetails>();

            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                LCMaster lCMaster = ctxTFAT.LCMaster.Where(x => x.TableKey.ToString() == mModel.Document).FirstOrDefault();

                if (lCMaster.DispachFM != 0)
                {
                    mModel.DispatchLc = true;
                }

                mModel.PeriodLock = PeriodLock(lCMaster.Branch, "LC000", lCMaster.Date);
                if (lCMaster.AUTHORISE.Substring(0, 1) == "A")
                {
                    mModel.LockAuthorise = LockAuthorise("LC000", mModel.Mode, lCMaster.TableKey, lCMaster.ParentKey);
                }


                mModel.Branch = lCMaster.Branch;
                mModel.Lcno = lCMaster.LCno.ToString();
                mModel.Time = lCMaster.Time;
                mModel.LcDate = lCMaster.Date.ToShortDateString();
                mModel.DocDate = lCMaster.Date;
                mModel.LcFromSource = lCMaster.FromBranch;
                mModel.LcFromSource_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.LcFromSource).Select(x => x.Name).FirstOrDefault();
                mModel.LcTODest = lCMaster.ToBranch;
                var TO = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.LcTODest).Select(x => new { x.Name, x.Category }).FirstOrDefault();
                if (TO.Category == "Zone")
                {
                    mModel.LcToDest_Name = TO.Name + " - Z";
                }
                else if (TO.Category == "Branch")
                {
                    mModel.LcToDest_Name = TO.Name + " - B";

                }
                else if (TO.Category == "SubBranch")
                {
                    mModel.LcToDest_Name = TO.Name + " - SB";
                }
                else
                {
                    mModel.LcToDest_Name = TO.Name + " - A";
                }

                mModel.Remark = lCMaster.Remark;
                mModel.LcGenerate = lCMaster.GenerateType;

                var mList = ctxTFAT.LCDetail.Where(x => x.LCRefTablekey.ToString() == mModel.Document).ToList().Count();
                if (mList != 0)
                {
                    var lCDetail = ctxTFAT.LCDetail.Where(x => x.LCRefTablekey.ToString() == mModel.Document).ToList();
                    foreach (var item in lCDetail)
                    {
                        LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey == item.ParentKey).FirstOrDefault();
                        LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey == item.LRRefTablekey).FirstOrDefault();

                        StockDetails lcDetailsVM = new StockDetails();
                        lcDetailsVM.LrNo = item.LRno.ToString();
                        lcDetailsVM.LrBookDate = lRMaster.BookDate.ToShortDateString();
                        lcDetailsVM.StockAvlIn = item.PickFrom;
                        lcDetailsVM.AvlQty = item.BalQty;
                        lcDetailsVM.LoadQty = item.Qty;
                        lcDetailsVM.LoadWeight = item.LRActWeight;
                        lcDetailsVM.ActWeight = lRMaster.ActWt;
                        lcDetailsVM.ChrWeight = lRMaster.ChgWt;
                        lcDetailsVM.ChgType = ctxTFAT.ChargeTypeMaster.Where(x => x.Code.ToLower().Trim() == lRMaster.ChgType.ToLower().Trim()).Select(x => x.ChargeType).FirstOrDefault();
                        lcDetailsVM.UnitCode = ctxTFAT.UnitMaster.Where(x => x.Code == lRMaster.UnitCode).Select(x => x.Name).FirstOrDefault();
                        lcDetailsVM.From = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Source).Select(x => x.Name).FirstOrDefault();
                        lcDetailsVM.To = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name).FirstOrDefault();
                        lcDetailsVM.Consigner = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.RecCode).Select(x => x.Name).FirstOrDefault();
                        lcDetailsVM.Consignee = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.SendCode).Select(x => x.Name).FirstOrDefault();
                        lcDetailsVM.LRType = ctxTFAT.LRTypeMaster.Where(x => x.Code == lRMaster.LRtype).Select(x => x.LRType).FirstOrDefault();
                        lcDetailsVM.DeliveryOfLR = lRMaster.Delivery == "G" ? "Godown" : "Door";
                        lcDetailsVM.Collection = item.LRColln == "G" ? "Godown" : item.LRColln == "D" ? "Door" : "Crossing";
                        lcDetailsVM.Mode = lRStock.LRMode;

                        lcDetailsVM.StockTableky = item.ParentKey;
                        lcDetailsVM.ConsignmentTableky = lRMaster.TableKey;

                        lCDetails.Add(lcDetailsVM);
                    }
                    TempData["Consignments"] = lCDetails;
                    mModel.Consignments = lCDetails;
                }
                else
                {
                    mModel.Consignments = lCDetails;
                }
            }
            else
            {
                mModel.Branch = mbranchcode;
                mModel.LcFromSource = mbranchcode;
                mModel.LcFromSource_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.LcFromSource).Select(x => x.Name).FirstOrDefault();
                mModel.Lcno = "0";
                mModel.Time = DateTime.Now.ToString("HH:mm");
                mModel.LcDate = DateTime.Now.ToShortDateString();
                mModel.DocDate = DateTime.Now;
                mModel.Consignments = lCDetails;
            }


            List<GridOption> Grlist = new List<GridOption>();
            var list = ctxTFAT.DocFormats.Where(x => x.Type == "LC000").Select(x => x).ToList();
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
            mModel.PrintGridList = Grlist;

            return View(mModel);
        }

        #region SaveData And Delete

        public ActionResult SaveData(LCVM mModel)
        {
            string ErroMsg = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        var Msg = DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Msg;
                    }
                    if (mModel.LcGenerate != "A")
                    {
                        bool checkalloctionFound = false;
                        LCSetup lCSetup = ctxTFAT.LCSetup.FirstOrDefault();
                        int CHKNo = Convert.ToInt32(mModel.Lcno);
                        string CHKdocument = mModel.Document;
                        if (lCSetup.ManualLcCheck)
                        {
                            //List<TblBranchAllocation> tblBranchAllocations = tblBranchAllocations = ctxTFAT.TblBranchAllocation.Where(x => x.Prefix == mperiod && x.Branch == mbranchcode && x.TRN_Type == "LC000").ToList();

                            //foreach (var item in tblBranchAllocations)
                            //{
                            //    if (item.ManualFrom <= CHKNo && CHKNo <= item.ManualTo)
                            //    {
                            //        checkalloctionFound = true;
                            //        break;
                            //    }
                            //}

                            List<TblBranchAllocation> tblBranchAllocations = new List<TblBranchAllocation>();
                            if (lCSetup.CetralisedManualSrlReq)
                            {
                                tblBranchAllocations = ctxTFAT.TblBranchAllocation.Where(x => x.Branch == mbranchcode && x.TRN_Type == "LC000").ToList();
                            }
                            else
                            {
                                tblBranchAllocations = ctxTFAT.TblBranchAllocation.Where(x => x.Prefix == mperiod && x.Branch == mbranchcode && x.TRN_Type == "LC000").ToList();
                            }
                            foreach (var item in tblBranchAllocations)
                            {
                                if (item.ManualFrom <= CHKNo && CHKNo <= item.ManualTo)
                                {
                                    checkalloctionFound = true;
                                    break;
                                }
                            }



                        }
                        else
                        {
                            checkalloctionFound = true;
                        }


                        if (checkalloctionFound)
                        {
                            LCMaster lCMaster = ctxTFAT.LCMaster.Where(x => x.LCno == CHKNo && x.TableKey.ToString() != CHKdocument).FirstOrDefault();
                            if (lCMaster != null)
                            {
                                transaction.Rollback();
                                return Json(new { Message = "This LCNo Exist \nSo,Please Change LC No....!", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var result = ctxTFAT.DocTypes.Where(x => x.Code == "LC000").Select(x => x).FirstOrDefault();
                                if (CHKNo.ToString().Length > (result.DocWidth))
                                {
                                    transaction.Rollback();
                                    return Json(new { Message = "LC NO. Allow " + result.DocWidth + " Digit Only....!", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                        else
                        {
                            transaction.Rollback();
                            return Json(new { Message = "Manual Range Not Found....!", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    List<StockDetails> lRModals = mModel.Consignments;
                    if (lRModals == null)
                    {
                        lRModals = new List<StockDetails>();
                    }

                    var CheckDuplicateConsignment = lRModals
                        .GroupBy(x => x.StockTableky)
                        .Select(g => new { StockTableky = g.Key, Count = g.Count() })
                        .Where(g => g.Count > 1)
                        .ToList();
                    if (CheckDuplicateConsignment.Count()>1)
                    {
                        return Json(new { Message = "Please Remove the Duplicate Consignment....!", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                    }
                    LCSetup cSetup = ctxTFAT.LCSetup.FirstOrDefault();
                    if (cSetup != null)
                    {
                        if (cSetup.CheckLrDate)
                        {
                            string ErrorMessage = "";
                            var LorryChallanDate = ConvertDDMMYYTOYYMMDD(mModel.LcDate);
                            var LorryReceiptList = lRModals.Select(x => x.ConsignmentTableky).ToList();
                            var ReomveThisLorry = ctxTFAT.LRMaster.Where(x => x.BookDate > LorryChallanDate && LorryReceiptList.Contains(x.TableKey)).Select(x => x.LrNo).ToList();
                            foreach (var item in ReomveThisLorry)
                            {
                                ErrorMessage += item.ToString() + " / ";
                            }
                            if (!String.IsNullOrEmpty(ErrorMessage))
                            {
                                ErrorMessage = ErrorMessage.Substring(0, ErrorMessage.Length - 3);
                                transaction.Rollback();
                                return Json(new { Message = "This Consignments Date Greater Than Lorry Challan.\nPlease Remove Following Consignment : " + ErrorMessage.Trim(), Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }

                    LCMaster mobj = new LCMaster();
                    bool mAdd = true;
                    if (ctxTFAT.LCMaster.Where(x => x.TableKey.ToString() == mModel.Document.ToString()).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.LCMaster.Where(x => x.TableKey.ToString() == mModel.Document).FirstOrDefault();
                        mAdd = false;
                        DeUpdate(mModel);
                        mdocument = mobj.TableKey.ToString();
                    }
                    if (mAdd == false)
                    {
                        if (mbranchcode != mobj.Branch)
                        {
                            transaction.Rollback();
                            return Json(new { Message = "Login Branch Does Not Match With Documnet Branch.", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    if (mAdd)
                    {
                        mobj.CreateDate = DateTime.Now;
                        mobj.LoginBranch = mbranchcode;
                        mobj.Branch = mbranchcode;
                        mobj.LCno = mModel.LcGenerate == "A" ? Convert.ToInt32(GetNewCode()) : Convert.ToInt32(mModel.Lcno);
                        mobj.GenerateType = mModel.LcGenerate;
                        mobj.Prefix = mperiod;
                        mobj.TableKey = mbranchcode + "LC000" + mperiod.Substring(0, 2) + 1.ToString("D3") + mobj.LCno.ToString();
                        mobj.ParentKey = mbranchcode + "LC000" + mperiod.Substring(0, 2) + mobj.LCno.ToString();
                        mdocument = mobj.TableKey.ToString();
                        if (mModel.LcGenerate == "A")
                        {
                            var alertnote = ctxTFAT.AlertNoteMaster.Where(x => x.Type == "LC000" && x.ParentKey.Trim() == mobj.TableKey.ToString().Trim() && String.IsNullOrEmpty(x.Stop) == false).FirstOrDefault();
                            if (alertnote != null)
                            {
                                transaction.Rollback();
                                return Json(new { Message = mobj.TableKey.ToString().Trim(), Status = "AutoDocumentAlert", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }


                    string Athorise = "A00";
                    #region Authorisation
                    TfatUserAuditHeader authorisation = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == "LC000").FirstOrDefault();
                    if (authorisation != null)
                    {
                        Athorise = SetAuthorisationLogistics(authorisation, mobj.TableKey, mobj.LCno.ToString(), 0, mModel.LcDate, 0, "", mbranchcode);
                        mobj.AUTHORISE = Athorise;
                    }
                    #endregion


                    int I = 1, TotalLoadQty = 0;
                    foreach (var item in lRModals)
                    {
                        var ConsignmentAvlQty = 0;
                        var Consignment = ctxTFAT.LRMaster.Where(x => x.TableKey == item.ConsignmentTableky).FirstOrDefault();
                        var ConsignmentStock = ctxTFAT.LRStock.Where(x => x.TableKey == item.StockTableky).FirstOrDefault();
                        var ConsignmentDelivery = ctxTFAT.LRStock.Where(x => x.ParentKey == item.StockTableky && x.Type == "DEL").Sum(x => (int?)x.TotalQty) ?? 0;
                        var UnDispatchLorryChallan = ctxTFAT.LCMaster.Where(x => x.DispachFM == 0).Select(x => x.TableKey).ToList();
                        var LorryChallanAllocateQty = ctxTFAT.LCDetail.Where(x => x.ParentKey == item.StockTableky && UnDispatchLorryChallan.Contains(x.LCRefTablekey)).Sum(x => (int?)x.LrQty) ?? 0;
                        ConsignmentAvlQty = ConsignmentStock.TotalQty - (ConsignmentDelivery + LorryChallanAllocateQty);
                        var Weight = (ConsignmentAvlQty / ConsignmentStock.TotalQty) * Consignment.ActWt;

                        LCDetail lCDetail = new LCDetail();
                        lCDetail.ActWeight = Consignment.ActWt;
                        lCDetail.BalQty = ConsignmentAvlQty;
                        lCDetail.ChrgeType = Consignment.ChgType;
                        lCDetail.ChrWeight = Consignment.ChgWt;
                        lCDetail.Consignee = Consignment.SendCode;
                        lCDetail.Consignor = Consignment.RecCode;
                        lCDetail.Date = ConvertDDMMYYTOYYMMDD(mModel.LcDate);
                        lCDetail.Description = String.IsNullOrEmpty(Consignment.DescrType) == true ? " " : Consignment.DescrType;
                        lCDetail.ENTEREDBY = muserid;
                        lCDetail.FromBranch = Consignment.Source;
                        lCDetail.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                        lCDetail.LCno = mobj.LCno;


                        if (ConsignmentAvlQty >= item.LoadQty)
                        {
                            lCDetail.LrQty = item.LoadQty;
                            lCDetail.Qty = item.LoadQty;
                            lCDetail.UnloadGodwonQty = item.LoadQty;
                            lCDetail.LRActWeight = item.LoadWeight;
                            TotalLoadQty += item.LoadQty;
                        }
                        else
                        {
                            lCDetail.LrQty = ConsignmentAvlQty;
                            lCDetail.Qty = ConsignmentAvlQty;
                            lCDetail.UnloadGodwonQty = ConsignmentAvlQty;
                            lCDetail.LRActWeight = Weight;
                            ErroMsg += item.LrNo + "this Consignment Allocated Only " + ConsignmentAvlQty + " Qty.\n ";
                            TotalLoadQty += ConsignmentAvlQty;
                        }

                        lCDetail.LRColln = Consignment.Colln;
                        lCDetail.LRDelivery = Consignment.Delivery;
                        lCDetail.LRno = Consignment.LrNo;
                        lCDetail.LrType = Consignment.LRtype;
                        lCDetail.ParentKey = item.StockTableky;
                        lCDetail.PickFrom = item.StockAvlIn;
                        lCDetail.TableKey = mbranchcode + "LCD00" + mperiod.Substring(0, 2) + I.ToString("D3") + mobj.LCno.ToString();
                        lCDetail.Time = mModel.Time;
                        lCDetail.ToBranch = Consignment.Dest;
                        lCDetail.Unit = Consignment.UnitCode;
                        lCDetail.UnloadDirectQty = 0;
                        lCDetail.AUTHORISE = Athorise;
                        lCDetail.AUTHIDS = muserid;
                        lCDetail.Prefix = mperiod;
                        lCDetail.LRRefTablekey = item.ConsignmentTableky;
                        lCDetail.LCRefTablekey = mobj.TableKey;

                        ctxTFAT.LCDetail.Add(lCDetail);

                        ++I;
                    }



                    mobj.FromBranch = mModel.LcFromSource;
                    mobj.ToBranch = mModel.LcTODest;
                    mobj.TotalQty = TotalLoadQty;
                    mobj.Date = ConvertDDMMYYTOYYMMDD(mModel.LcDate);
                    mobj.Time = mModel.Time;
                    mobj.DispachFM = 0;
                    mobj.Remark = mModel.Remark;
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;

                    if (mModel.Mode == "Add")
                    {
                        ctxTFAT.LCMaster.Add(mobj);
                        SaveNarrationAdd(mobj.LCno.ToString(), mobj.TableKey);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, mobj.ParentKey, mobj.Date, 0, "", "Save Lorry Challan :" + mobj.LCno, "NA");
                    LorryChallanNotification(mobj);
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
            // Godown_LR_Stock_List("");

            //Transit_LR_List("");

            return Json(new
            {
                Status = "Success",
                id = "LCDetail",
                Msg = ErroMsg,
                SerialNo = mdocument,
            }, JsonRequestBehavior.AllowGet);
        }

        public void DeUpdate(LCVM Model)
        {
            LCMaster lCMaster = ctxTFAT.LCMaster.Where(x => x.TableKey == Model.Document).FirstOrDefault();
            if (lCMaster != null)
            {
                var ConsignmentList = ctxTFAT.LCDetail.Where(x => x.LCRefTablekey == Model.Document).ToList();
                ctxTFAT.LCDetail.RemoveRange(ConsignmentList);
            }

            ctxTFAT.SaveChanges();
        }

        public ActionResult DeleteStateMaster(LCVM mModel)
        {
            string ErroMsg = "N";
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            //var lC_ExtraInformation = ctxTFAT.LockSystem.Where(x => (x.DocumentNo.ToString() == mModel.Document) && x.Type=="LC").FirstOrDefault();

            //if (lC_ExtraInformation.Billing == true || lC_ExtraInformation.Delivery == true || lC_ExtraInformation.Dispatch == true)
            //{
            //    return "PLease Remove Lock Credential First.";
            //}

            //if (lC_Attachments.Count > 0)
            //{
            //    return "PLease Remove Attachment First.";

            //}
            LCMaster lCMaster = ctxTFAT.LCMaster.Where(x => x.TableKey.ToString() == mModel.Document).FirstOrDefault();

            if (ctxTFAT.PeriodLock.Where(x => x.Type == "LC000" && x.LockDate == lCMaster.Date && x.Branch == mbranchcode).FirstOrDefault() != null)
            {
                return Json(new
                {
                    Message = "This LC Date Lock By Period Lock System....",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }


            var FmRelation = lCMaster;
            if (FmRelation.DispachFM != 0)
            {
                return Json(new
                {
                    Message = "Already Loaded In " + FmRelation.DispachFM + "  FM...",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var GetRemarkDocList = ctxTFAT.Narration.Where(x => x.Srl == lCMaster.LCno.ToString() && x.Type == "LC000").ToList();
            if (GetRemarkDocList != null)
            {
                ctxTFAT.Narration.RemoveRange(GetRemarkDocList);
            }


            //var lC_Attachments = ctxTFAT.Attachment.Where(x => (x.ParentCode.ToString() == lCMaster.AttachmentCode)).ToList();
            var AuthorisationEntry = ctxTFAT.Authorisation.Where(x => x.Type == "LC000" && x.Srl == lCMaster.LCno.ToString()).FirstOrDefault();
            if (AuthorisationEntry != null)
            {
                ctxTFAT.Authorisation.Remove(AuthorisationEntry);
            }
            ctxTFAT.LCMaster.Remove(lCMaster);

            //foreach (var item in lC_Attachments)
            //{
            //    ctxTFAT.Attachment.Remove(item);
            //}


            //var GetLrListToUpdateDispatchLC = ctxTFAT.LCDetail.Where(x => (x.LCno.ToString() == mModel.Document)).Select(x => x.LRno).ToList().Distinct();

            var mList = ctxTFAT.LCDetail.Where(x => x.LCRefTablekey.ToString() == mModel.Document).ToList();
            ctxTFAT.LCDetail.RemoveRange(mList);
            //foreach (var item in mList)
            //{
            //    LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey.ToString() == item.ParentKey).FirstOrDefault();
            //    if (lRStock != null)
            //    {
            //        lRStock.AllocatBalQty += item.Qty;
            //        lRStock.AllocatBalWght += item.LRActWeight;
            //        ctxTFAT.Entry(lRStock).State = EntityState.Modified;
            //    }
            //    ctxTFAT.LCDetail.Remove(item);
            //}

            //foreach (var item in GetLrListToUpdateDispatchLC)
            //{
            //    var Lrbill = ctxTFAT.LRMaster.Where(x => (x.LrNo == item)).FirstOrDefault();
            //    if (Lrbill != null && ctxTFAT.LCDetail.Where(x => x.LRno == item).ToList().Count() == 1)
            //    {
            //        Lrbill.DispachLC = Lrbill.DispachLC - 1;
            //        ctxTFAT.Entry(Lrbill).State = EntityState.Modified;
            //    }
            //}


            //var LrStockLcDetails = ctxTFAT.LRStock.Where(x => x.Type == "LC" && x.ParentKey == mModel.Document).ToList();
            //foreach (var item in LrStockLcDetails)
            //{
            //    ctxTFAT.LRStock.Remove(item);
            //}

            ctxTFAT.SaveChanges();
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, lCMaster.ParentKey, lCMaster.Date, 0, "", "Delete Lorry Challan :" + lCMaster.LCno, "NA");
            return Json(new { Msg = ErroMsg, Status = "Success" }, JsonRequestBehavior.AllowGet);


        }

        public string GetCode()
        {
            string DocNo = "";

            DocNo = ctxTFAT.AlertNoteMaster.OrderByDescending(x => x.RECORDKEY).Select(x => x.DocNo).FirstOrDefault();

            if (String.IsNullOrEmpty(DocNo))
            {
                DocNo = "100000";
            }
            else
            {
                var Integer = Convert.ToInt32(DocNo) + 1;
                DocNo = Integer.ToString("D6");
            }

            return DocNo;
        }

        public void SaveNarrationAdd(string DocNo, string Parentkey)
        {
            List<AlertNoteMaster> objledgerdetail = new List<AlertNoteMaster>();

            if (Session["CommnNarrlist"] != null)
            {
                objledgerdetail = (List<AlertNoteMaster>)Session["CommnNarrlist"];
            }
            int Sno = Convert.ToInt32(GetCode());
            foreach (var item in objledgerdetail)
            {
                item.DocNo = Sno.ToString("D6");
                item.TypeCode = DocNo;
                item.TableKey = "ALERT" + mperiod.Substring(0, 2) + 1.ToString("D3") + item.DocNo;
                item.ParentKey = Parentkey;
                ctxTFAT.AlertNoteMaster.Add(item);
                ++Sno;
            }
        }

        #endregion SaveData

        #region LCMergeData JqGrid Combo

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id, bool Flag)
        {
            TfatSearch tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == "LCMergeData" && x.ColField.Contains("STKBranch")).Select(x => x).FirstOrDefault();
            if (Flag)
            {
                tfatSearch.IsHidden = false;
            }
            else
            {
                tfatSearch.IsHidden = true;
            }
            ctxTFAT.Entry(tfatSearch).State = EntityState.Modified;
            ctxTFAT.SaveChanges();

            return GetGridDataColumns(id, "L", "XXXX");
        }
        
        //All Stock
        public ActionResult GetGridData(GridOption Model)
        {
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("Sp_Lc_MergeStock", tfat_conx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@CurrBranch", SqlDbType.VarChar).Value = mbranchcode;
            cmd.Parameters.Add("@TableKetList", SqlDbType.VarChar).Value = Model.mVar4;
            cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters.Add("@mReturnQuery1", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters.Add("@mReturnQuery2", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;
            cmd.Parameters["@mReturnQuery1"].Direction = ParameterDirection.Output;
            cmd.Parameters["@mReturnQuery2"].Direction = ParameterDirection.Output;

            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();

            string mSelectQuery = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
            string mSelectQuery1 = (string)(cmd.Parameters["@mReturnQuery1"].Value ?? "");
            string mSelectQuery2 = (string)(cmd.Parameters["@mReturnQuery2"].Value ?? "");
            tfat_conx.Close();

            return GetGridReport(Model, "M", "", false, 0);
        }
        //Dispatch Stock
        public ActionResult GetGridData1(GridOption Model)
        {
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("Sp_Lc_DispatchStock", tfat_conx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@CurrBranch", SqlDbType.VarChar).Value = mbranchcode;
            cmd.Parameters.Add("@Destination", SqlDbType.VarChar).Value = Model.mVar3 == null ? "" : Model.mVar3;
            cmd.Parameters.Add("@TableKetList", SqlDbType.VarChar).Value = Model.mVar4;
            cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;

            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();

            string mSelectQuery = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
            tfat_conx.Close();

            return GetGridReport(Model, "M", "", false, 0);
        }
        //Godown Stock
        public ActionResult GetGridData2(GridOption Model)
        {
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("Sp_Lc_GodwnStock", tfat_conx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@CurrBranch", SqlDbType.VarChar).Value = mbranchcode;
            cmd.Parameters.Add("@TableKetList", SqlDbType.VarChar).Value = Model.mVar4;
            cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;

            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();

            string mSelectQuery = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
            tfat_conx.Close();

            return GetGridReport(Model, "M", "", false, 0);
        }
        //Transit Stock
        public ActionResult GetGridData3(GridOption Model)
        {

            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("Sp_Lc_TransitStock", tfat_conx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@CurrBranch", SqlDbType.VarChar).Value = mbranchcode;
            cmd.Parameters.Add("@TableKetList", SqlDbType.VarChar).Value = Model.mVar4;
            cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;
            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();

            string mSelectQuery = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
            tfat_conx.Close();
            return GetGridReport(Model, "M", "", false, 0);
        }
        //Search Stock
        public ActionResult GetGridData4(GridOption Model)
        {
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("Sp_Lc_SearchLRStock", tfat_conx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@CurrBranch", SqlDbType.VarChar).Value = mbranchcode;
            cmd.Parameters.Add("@TableKetList", SqlDbType.VarChar).Value = Model.mVar4;
            cmd.Parameters.Add("@LRno", SqlDbType.VarChar).Value = Model.mVar3;
            cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters.Add("@mReturnQuery1", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters.Add("@mReturnQuery2", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;
            cmd.Parameters["@mReturnQuery1"].Direction = ParameterDirection.Output;
            cmd.Parameters["@mReturnQuery2"].Direction = ParameterDirection.Output;

            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();

            string mSelectQuery = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
            string mSelectQuery1 = (string)(cmd.Parameters["@mReturnQuery1"].Value ?? "");
            string mSelectQuery2 = (string)(cmd.Parameters["@mReturnQuery2"].Value ?? "");
            tfat_conx.Close();
            return GetGridReport(Model, "M", "", false, 0);
        }

        //Save Consignment In Grid
        public ActionResult GridView(LCVM mModel)
        {
            //mModel.lCDetails.ToList().ForEach(w => w.EditLDSNo = true);
            List<StockDetails> InserList = TempData.Peek("Consignments") as List<StockDetails>;
            if (InserList == null)
            {
                InserList = new List<StockDetails>();
            }

            InserList.AddRange(mModel.Consignments);
            TempData["Consignments"] = InserList;
            var html = ViewHelper.RenderPartialView(this, "GridView", InserList);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteConsignment(StockDetails mModel)
        {
            //mModel.lCDetails.ToList().ForEach(w => w.EditLDSNo = true);
            List<StockDetails> InserList = TempData.Peek("Consignments") as List<StockDetails>;
            if (InserList == null)
            {
                InserList = new List<StockDetails>();
            }

            InserList = InserList.Where(x => x.StockTableky != mModel.StockTableky).ToList();
            TempData["Consignments"] = InserList;
            return Json(new { Status = "Sucess" }, JsonRequestBehavior.AllowGet);
        }



        #endregion

        #region AlertNote Stop CHeck
        public ActionResult CheckStopAlertNote(string Type, List<string> TypeCode, string DocTpe)
        {
            string Status = "Success", Message = "";
            if (TypeCode == null)
            {
                TypeCode = new List<string>();
                TypeCode.Add("");
            }
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
                        Message += item.TypeCode + " Not Allowed In LC Please Remove IT....\n";
                        break;
                    }
                }
            }
            var jsonResult = Json(new { Status = Status, Message = Message }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public ActionResult DocumentStopAlertNote(string Type, List<string> TypeCode)
        {
            string Status = "Success", Message = "";
            if (TypeCode == null)
            {
                TypeCode = new List<string>();
                TypeCode.Add("");
            }

            var Tablekey = TypeCode.FirstOrDefault().ToString();

            var Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && AlertMater.ParentKey == Tablekey && String.IsNullOrEmpty(AlertMater.Stop) == false
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
                    if (Type.Trim() == stp.Trim())
                    {
                        Status = "Error";
                        Message += Tablekey;
                        break;
                    }
                }
            }
            var jsonResult = Json(new { Status = Status, Message = Message }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        #endregion

        public ActionResult PrintSingleDocumentCrystal(GridOption Model)
        {
            if (String.IsNullOrEmpty(Model.Branch))
            {
                Model.Branch = mbranchcode;
            }
            //var PDFName = Model.Document.Substring(20);
            var PDFName = "";
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

            //string mParentKey = Model.Document.Substring(6, Model.Document.Length - 6);
            string mParentKey = Model.Document;

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
                if (String.IsNullOrEmpty(PDFName))
                {
                    return File(mstream, "application/PDF");
                }
                else
                {
                    return File(mstream, "application/PDF", PDFName + ".pdf");
                }
                //return File(mstream, "application/PDF");
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
            if (String.IsNullOrEmpty(Model.Branch))
            {
                Model.Branch = mbranchcode;
            }
            //var PDFName = Model.Document.Substring(20);
            var PDFName = "";
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
                    //mParentKey = Model.Document.Substring(6, Model.Document.Length - 6);
                    mParentKey = Model.Document;
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

            if (String.IsNullOrEmpty(PDFName))
            {
                return File(ms.ToArray(), "application/PDF");
            }
            else
            {
                return File(ms.ToArray(), "application/PDF", PDFName + ".pdf");
            }
            //return File(ms.ToArray(), "application/PDF");

        }
    }
}