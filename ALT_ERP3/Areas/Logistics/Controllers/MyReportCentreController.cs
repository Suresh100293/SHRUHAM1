using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using System.Data.SqlClient;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class MyReportCentreController : BaseController
    {
         
        //private IBusinessCommon mBuss = new BusinessCommon();
        private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private DataTable table = new DataTable();

        #region GetLists
        public List<SelectListItem> GetColTypeList()
        {
            List<SelectListItem> CallColTypeList = new List<SelectListItem>();
            CallColTypeList.Add(new SelectListItem { Value = "Str", Text = "Str" });
            CallColTypeList.Add(new SelectListItem { Value = "Num", Text = "Num" });
            CallColTypeList.Add(new SelectListItem { Value = "Dte", Text = "Dte" });
            CallColTypeList.Add(new SelectListItem { Value = "Dtm", Text = "Dtm" });
            CallColTypeList.Add(new SelectListItem { Value = "Qty", Text = "Qty" });
            CallColTypeList.Add(new SelectListItem { Value = "Rte", Text = "Rte" });
            CallColTypeList.Add(new SelectListItem { Value = "Chk", Text = "Chk" });
            return CallColTypeList;
        }

        
        #endregion GetLists


        // GET: Logistics/MyReportCentre
        public ActionResult Index(MyReportCentreVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, "Report Centre", "", DateTime.Now, 0, "", "", "A");
            mdocument = mModel.Document;
            mModel.ColTypeList = GetColTypeList();
            Session["GridDataSession"] = null;
            mModel.ReportHeader_Code = mModel.Document;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.ReportHeader.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    mModel.ReportHeader_Code = mList.Code;
                    mModel.ReportHeader_DefaultReport = mList.DefaultReport;
                    mModel.ReportHeader_RunAtStart = mList.RunAtStart;
                    mModel.ReportHeader_SubCodeOf = mList.SubCodeOf;
                    mModel.ReportHeader_FormatHead = mList.FormatHead;
                    mModel.ReportHeader_Locked = mList.Locked;
                    mModel.ReportHeader_SelectQuery = mList.SelectQuery;
                    mModel.ReportHeader_Tables = mList.Tables;
                    mModel.ReportHeader_OrderBy = mList.OrderBy;
                    mModel.ReportHeader_UserQuery = mList.UserQuery;
                    mModel.ReportHeader_InputPara = mList.InputPara;
                    mModel.ReportHeader_ParaString = mList.ParaString;
                    mModel.ReportHeader_SubTotalOn = mList.SubTotalOn;
                    mModel.ReportHeader_SortedOn = mList.SortedOn;
                    mModel.ReportHeader_DrillQuery = mList.DrillQuery;
                    mModel.ReportHeader_FontName = mList.FontName;
                    mModel.ReportHeader_FontSize = mList.FontSize;
                    mModel.ReportHeader_ForeColor = mList.ForeColor;
                    mModel.ReportHeader_DontSort = mList.DontSort;
                    mModel.ReportHeader_DontPrintTotal = mList.DontPrintTotal;
                    mModel.ReportHeader_RemoveUnused = mList.RemoveUnused;

                    var mList2 = ctxTFAT.TfatSearch.Where(x => x.Code == mModel.ReportHeader_Code).ToList();
                    List<MyReportCentreVM> mList3 = new List<MyReportCentreVM>();
                    int n = 1;
                    foreach (var eachvalue in mList2)
                    {
                        mList3.Add(new MyReportCentreVM()
                        {
                            TfatSearch_Sno = eachvalue.Sno,
                            TfatSearch_ColHead = eachvalue.ColHead,
                            TfatSearch_ColField = eachvalue.ColField,
                            TfatSearch_FldAs = eachvalue.FldAs,
                            TfatSearch_ColType = eachvalue.ColType,
                            TfatSearch_ColWidth = eachvalue.ColWidth != null ? eachvalue.ColWidth.Value : 0,
                            TfatSearch_Decs = eachvalue.Decs != null ? eachvalue.Decs.Value : 0,
                            TfatSearch_YesTotal = eachvalue.YesTotal,
                            TfatSearch_IsHidden = eachvalue.IsHidden,
                            TfatSearch_ColCondition = eachvalue.ColCondition,
                            TfatSearch_BackColor = eachvalue.BackColor,
                            tempid = n,
                            tempIsDeleted = false
                        });
                        n = n + 1;
                    }
                    Session.Add("GridDataSession", mList3);
                    mModel.GridDataVM = mList3;
                }
            }
            else
            {
                mModel.ReportHeader_AccGroups = "";
                mModel.ReportHeader_AllowEmail = false;
                mModel.ReportHeader_AllowSMS = false;
                mModel.ReportHeader_BackColor = "";
                mModel.ReportHeader_BarGraph = "";
                mModel.ReportHeader_Code = "";
                mModel.ReportHeader_Comp = "";
                mModel.ReportHeader_DefaultReport = false;
                mModel.ReportHeader_DisplayGrid = false;
                mModel.ReportHeader_Divisor = 0;
                mModel.ReportHeader_DontPrintTotal = false;
                mModel.ReportHeader_DontSort = false;
                mModel.ReportHeader_DrillCaption = "";
                mModel.ReportHeader_DrillQuery = "";
                mModel.ReportHeader_DrillQuerySub = "";
                mModel.ReportHeader_EmailTemplate = "";
                mModel.ReportHeader_FontBold = false;
                mModel.ReportHeader_FontItalics = false;
                mModel.ReportHeader_FontName = "";
                mModel.ReportHeader_FontSize = 0;
                mModel.ReportHeader_FontStrike = false;
                mModel.ReportHeader_FontUnderLine = false;
                mModel.ReportHeader_ForeColor = "";
                mModel.ReportHeader_FormatGroup = "";
                mModel.ReportHeader_FormatHead = "";
                mModel.ReportHeader_GridColor = 0;
                mModel.ReportHeader_GroupHead = "";
                mModel.ReportHeader_HeaderHeight = 0;
                mModel.ReportHeader_InputPara = "";
                mModel.ReportHeader_IsDrillDown = false;
                mModel.ReportHeader_ItemGroups = "";
                mModel.ReportHeader_LabelHORNos = 0;
                mModel.ReportHeader_LabelHORSpace = 0;
                mModel.ReportHeader_LabelOption = false;
                mModel.ReportHeader_LabelPrintTitle = 0;
                mModel.ReportHeader_LabelTitleWidth = 0;
                mModel.ReportHeader_LabelVERNos = 0;
                mModel.ReportHeader_LabelVERSpace = 0;
                mModel.ReportHeader_Locked = false;
                mModel.ReportHeader_MenuPlace = "";
                mModel.ReportHeader_minHeight = 0;
                mModel.ReportHeader_Modules = "";
                mModel.ReportHeader_NoDetails = false;
                mModel.ReportHeader_NoSubHeading = false;
                mModel.ReportHeader_OrderBy = "";
                mModel.ReportHeader_OwnReport = 0;
                mModel.ReportHeader_PageOrient = false;
                mModel.ReportHeader_ParaString = "";
                mModel.ReportHeader_pBlank = false;
                mModel.ReportHeader_pMerge = "";
                mModel.ReportHeader_PostLogic = "";
                mModel.ReportHeader_pToMerge = "";
                mModel.ReportHeader_RecLines = 0;
                mModel.ReportHeader_RecordFilter = "";
                mModel.ReportHeader_RemoveUnused = false;
                mModel.ReportHeader_RunAtStart = false;
                mModel.ReportHeader_SelectQuery = "";
                mModel.ReportHeader_SepLines = 0;
                mModel.ReportHeader_ShowFilter = false;
                mModel.ReportHeader_SMSTemplate = "";
                mModel.ReportHeader_SortedOn = "";
                mModel.ReportHeader_SubCodeOf = "";
                mModel.ReportHeader_SubTotal = false;
                mModel.ReportHeader_SubTotalOn = "";
                mModel.ReportHeader_SubTypes = "";
                mModel.ReportHeader_Summarized = false;
                mModel.ReportHeader_Tables = "";
                mModel.ReportHeader_Tabs = "";
                mModel.ReportHeader_TimesUsed = 0;
                mModel.ReportHeader_UserQuery = "";
                mModel.ReportHeader_Users = "";

                



            }
            return View(mModel);
        }




        

        #region GridOperations
        // following action is used when row is added to grid in add mode
        [HttpPost]
        public ActionResult AddToTable(MyReportCentreVM Model)
        {
            List<MyReportCentreVM> objgriddetail = new List<MyReportCentreVM>();
            if (Session["GridDataSession"] != null)
            {
                objgriddetail = (List<MyReportCentreVM>)Session["GridDataSession"];
            }
            objgriddetail.Add(new MyReportCentreVM()
            {
                TfatSearch_Sno = Model.TfatSearch_Sno,
                TfatSearch_ColHead = Model.TfatSearch_ColHead,
                TfatSearch_ColField = Model.TfatSearch_ColField,
                TfatSearch_FldAs = Model.TfatSearch_FldAs,
                TfatSearch_ColType = Model.TfatSearch_ColType,
                TfatSearch_ColWidth = Model.TfatSearch_ColWidth,
                TfatSearch_Decs = Model.TfatSearch_Decs,
                TfatSearch_YesTotal = Model.TfatSearch_YesTotal,
                TfatSearch_IsHidden = Model.TfatSearch_IsHidden,
                TfatSearch_ColCondition = Model.TfatSearch_ColCondition,
                TfatSearch_BackColor = Model.TfatSearch_BackColor,
                tempid = objgriddetail.Count + 1,
                tempIsDeleted = false
            });
           var ColTypeList = GetColTypeList();

            Session.Add("GridDataSession", objgriddetail);
            var html = ViewHelper.RenderPartialView(this, "GridDataView", new MyReportCentreVM() { GridDataVM = objgriddetail, Mode = "Add", ColTypeList= ColTypeList });
            return Json(new
            {
                GridDataVM = objgriddetail,
                Html = html,
                Mode = "Add"
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PickFromTable(MyReportCentreVM Model)
        {
            var result = (List<MyReportCentreVM>)Session["GridDataSession"];
            var result1 = result.Where(x => x.tempid == Model.tempid);
            Model.ColTypeList = GetColTypeList();
            foreach (var item in result1)
            {
                Model.TfatSearch_Sno = item.TfatSearch_Sno;
                Model.TfatSearch_ColHead = item.TfatSearch_ColHead;
                Model.TfatSearch_ColField = item.TfatSearch_ColField;
                Model.TfatSearch_FldAs = item.TfatSearch_FldAs;
                Model.TfatSearch_ColType = item.TfatSearch_ColType;
                Model.TfatSearch_ColWidth = item.TfatSearch_ColWidth;
                Model.TfatSearch_Decs = item.TfatSearch_Decs;
                Model.TfatSearch_YesTotal = item.TfatSearch_YesTotal;
                Model.TfatSearch_IsHidden = item.TfatSearch_IsHidden;
                Model.TfatSearch_ColCondition = item.TfatSearch_ColCondition;
                Model.TfatSearch_BackColor = item.TfatSearch_BackColor;
                Model.tempid = item.tempid;
                Model.GridDataVM = result;
            }
            return Json(new
            {
                Html = this.RenderPartialView("GridDataView", Model)
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult AddToTableEdit(MyReportCentreVM Model)
        {
            var result = (List<MyReportCentreVM>)Session["GridDataSession"];
            var ColTypeList = GetColTypeList();

            foreach (var item in result.Where(x => x.tempid == Model.tempid))
            {
                item.TfatSearch_Sno = Model.TfatSearch_Sno;
                item.TfatSearch_ColHead = Model.TfatSearch_ColHead;
                item.TfatSearch_ColField = Model.TfatSearch_ColField;
                item.TfatSearch_FldAs = Model.TfatSearch_FldAs;
                item.TfatSearch_ColType = Model.TfatSearch_ColType;
                item.TfatSearch_ColWidth = Model.TfatSearch_ColWidth;
                item.TfatSearch_Decs = Model.TfatSearch_Decs;
                item.TfatSearch_YesTotal = Model.TfatSearch_YesTotal;
                item.TfatSearch_IsHidden = Model.TfatSearch_IsHidden;
                item.TfatSearch_ColCondition = Model.TfatSearch_ColCondition;
                item.TfatSearch_BackColor = Model.TfatSearch_BackColor;
                item.tempid = Model.tempid;
                item.tempIsDeleted = false;
            }
            Session.Add("GridDataSession", result);
            var html = ViewHelper.RenderPartialView(this, "GridDataView", new MyReportCentreVM() { GridDataVM = result, Mode = "Add", ColTypeList= ColTypeList });
            return Json(new
            {
                GridDataVM = result,
                Html = html,
                Mode = "Add"
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteTableRow(int tempid, MyReportCentreVM Model)
        {
            var result = (List<MyReportCentreVM>)Session["GridDataSession"];
            result.Where(x => x.tempid == tempid).FirstOrDefault().tempIsDeleted = true;
            var ColTypeList = GetColTypeList();
            Session.Add("GridDataSession", result);
            var html = ViewHelper.RenderPartialView(this, "GridDataView", new MyReportCentreVM() { GridDataVM = result, ColTypeList= ColTypeList });
            return Json(new { SelectedIndent = result, Html = html }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region SaveData
        public ActionResult SaveData(MyReportCentreVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteMyReportCentre(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    ReportHeader mobj = new ReportHeader();
                    bool mAdd = true;
                    if (ctxTFAT.ReportHeader.Where(x => (x.Code == mModel.ReportHeader_Code)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.ReportHeader.Where(x => (x.Code == mModel.ReportHeader_Code)).FirstOrDefault();
                        mAdd = false;
                    }
                    mobj.Code = mModel.ReportHeader_Code;
                    mobj.DefaultReport = mModel.ReportHeader_DefaultReport;
                    mobj.RunAtStart = mModel.ReportHeader_RunAtStart;
                    mobj.SubCodeOf = mModel.ReportHeader_SubCodeOf;
                    mobj.FormatHead = mModel.ReportHeader_FormatHead;
                    mobj.Locked = mModel.ReportHeader_Locked;
                    mobj.SelectQuery = mModel.ReportHeader_SelectQuery;
                    mobj.Tables = mModel.ReportHeader_Tables;
                    mobj.OrderBy = mModel.ReportHeader_OrderBy;
                    mobj.UserQuery = mModel.ReportHeader_UserQuery;
                    mobj.InputPara = mModel.ReportHeader_InputPara;
                    mobj.ParaString = mModel.ReportHeader_ParaString;
                    mobj.SubTotalOn = mModel.ReportHeader_SubTotalOn;
                    mobj.SortedOn = mModel.ReportHeader_SortedOn;
                    mobj.DrillQuery = mModel.ReportHeader_DrillQuery;
                    mobj.FontName = mModel.ReportHeader_FontName;
                    mobj.FontSize = mModel.ReportHeader_FontSize;
                    mobj.ForeColor = mModel.ReportHeader_ForeColor;
                    mobj.DontSort = mModel.ReportHeader_DontSort;
                    mobj.DontPrintTotal = mModel.ReportHeader_DontPrintTotal;
                    mobj.RemoveUnused = mModel.ReportHeader_RemoveUnused;
                    // iX9: default values for the fields not used @Form
                    mobj.AccGroups = "";
                    mobj.AllowEmail = false;
                    mobj.AllowSMS = false;
                    mobj.BackColor = "";
                    mobj.BarGraph = "";
                    mobj.Comp = "";
                    mobj.DisplayGrid = false;
                    mobj.Divisor = 0;
                    mobj.DrillCaption = "";
                    mobj.DrillQuerySub = "";
                    mobj.EmailTemplate = "";
                    mobj.FontBold = false;
                    mobj.FontItalics = false;
                    mobj.FontStrike = false;
                    mobj.FontUnderLine = false;
                    mobj.FormatGroup = "";
                    mobj.GridColor = 0;
                    mobj.GroupHead = "";
                    mobj.HeaderHeight = 0;
                    mobj.IsDrillDown = false;
                    mobj.ItemGroups = "";
                    mobj.LabelHORNos = 0;
                    mobj.LabelHORSpace = 0;
                    mobj.LabelOption = false;
                    mobj.LabelPrintTitle = 0;
                    mobj.LabelTitleWidth = 0;
                    mobj.LabelVERNos = 0;
                    mobj.LabelVERSpace = 0;
                    mobj.MenuPlace = "";
                    mobj.minHeight = 0;
                    mobj.Modules = "";
                    mobj.NoDetails = false;
                    mobj.NoSubHeading = false;
                    mobj.OwnReport = 0;
                    mobj.PageOrient = false;
                    mobj.pBlank = false;
                    mobj.pMerge = "";
                    mobj.PostLogic = "";
                    mobj.pToMerge = "";
                    mobj.RecLines = 0;
                    mobj.RecordFilter = "";
                    mobj.SepLines = 0;
                    mobj.ShowFilter = false;
                    mobj.SMSTemplate = "";
                    mobj.SubTotal = false;
                    mobj.SubTypes = "";
                    mobj.Summarized = false;
                    mobj.Tabs = "";
                    mobj.TimesUsed = 0;
                    mobj.Users = "";
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mAdd == true)
                    {
                        ctxTFAT.ReportHeader.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    string mNewCode = "";
                    mNewCode = mobj.Code;
                    SaveGridData(mModel);
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, "Report Centre", "", DateTime.Now, 0, mNewCode, "", "A");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "MyReportCentre" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "MyReportCentre" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "MyReportCentre" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "MyReportCentre" }, JsonRequestBehavior.AllowGet);
        }
        public void SaveGridData(MyReportCentreVM mModel)
        {
            // delete the existing data from the table
            var mList = ctxTFAT.TfatSearch.Where(x => x.Code == mModel.ReportHeader_Code).ToList();
            if (mList.Count != 0)
            {
                ctxTFAT.TfatSearch.RemoveRange(mList);
                ctxTFAT.SaveChanges();
            }
            var mList2 = (List<MyReportCentreVM>)Session["GridDataSession"];
            if (mList2 != null)
            {
                var mList3 = ((List<MyReportCentreVM>)Session["GridDataSession"]).Where(x => x.tempIsDeleted == false);
                foreach (var eachvalue in mList3)
                {
                    TfatSearch mgriddata = new TfatSearch();
                    mgriddata.Code = mModel.ReportHeader_Code;
                    mgriddata.ColHead = eachvalue.TfatSearch_ColHead;
                    mgriddata.ColField = eachvalue.TfatSearch_ColField;
                    mgriddata.FldAs = eachvalue.TfatSearch_FldAs;
                    mgriddata.ColType = eachvalue.TfatSearch_ColType;
                    mgriddata.ColWidth = eachvalue.TfatSearch_ColWidth;
                    mgriddata.Decs = eachvalue.TfatSearch_Decs;
                    mgriddata.YesTotal = eachvalue.TfatSearch_YesTotal;
                    mgriddata.IsHidden = eachvalue.TfatSearch_IsHidden;
                    mgriddata.ColCondition = eachvalue.TfatSearch_ColCondition;
                    mgriddata.BackColor = eachvalue.TfatSearch_BackColor;
                    mgriddata.Sno = eachvalue.tempid;
                    mgriddata.ENTEREDBY = muserid;
                    mgriddata.LASTUPDATEDATE = DateTime.Now;
                    mgriddata.AUTHORISE = mauthorise;
                    mgriddata.AUTHIDS = muserid;
                    ctxTFAT.TfatSearch.Add(mgriddata);
                    ctxTFAT.SaveChanges();
                }
            }
            Session["GridDataSession"] = null;
        }

        public ActionResult DeleteMyReportCentre(MyReportCentreVM mModel)
        {
            if (mModel.ReportHeader_Code == null || mModel.ReportHeader_Code == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var mList = ctxTFAT.ReportHeader.Where(x => (x.Code == mModel.ReportHeader_Code)).FirstOrDefault();
            ctxTFAT.ReportHeader.Remove(mList);
            var mList2 = ctxTFAT.TfatSearch.Where(x => x.Code == mModel.ReportHeader_Code).ToList();
            ctxTFAT.TfatSearch.RemoveRange(mList2);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}