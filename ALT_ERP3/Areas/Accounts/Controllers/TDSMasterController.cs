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
using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
////using ALT_ERP3.DynamicBusinessLayer;
////using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class TDSMasterController : BaseController
    {
        //private //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        //private IBusinessCommon mBuss = new BusinessCommon();
        private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mAUTHORISE = "A00";
        private static int mdocument = 0;
        private DataTable table = new DataTable();

        #region GetLists
        public JsonResult AutoCompletePostCode(string term)
        {
            return Json((from m in ctxTFAT.Master
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }
        
        #endregion GetLists

        // GET: Accounts/TDSMaster
        public ActionResult Index(TDSMasterVM mModel)
        {
            //GetAllMenu(Session["ModuleName"].ToString());
            ////AccountingGetAllMenu(Session["ModuleName"].ToString());
            ////VehicleGetAllMenu(Session["ModuleName"].ToString());
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0,"", "", "NA");
            mModel.TDSRates_EffDate = DateTime.Now;
            mModel.TDSMaster_FileDate = DateTime.Now;
            Session["GridDataSession"] = null;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                mdocument = mModel.Document;
                var mList = ctxTFAT.TDSMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                var mPostCode = ctxTFAT.Master.Where(x => x.Code == mList.PostCode).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                mModel.TDSMaster_PostCode = mPostCode != null ? mPostCode.Code.ToString() : "";
                mModel.PostCodeName = mPostCode != null ? mPostCode.Name : "";
                mModel.TDSMaster_Code = mList.Code;
                mModel.TDSMaster_Name = mList.Name;
                mModel.TDSMaster_CertAuto = mList.CertAuto;
                mModel.TDSMaster_Prefix = mList.Prefix;
                mModel.TDSMaster_CertAfter = mList.CertAfter;
                mModel.TDSMaster_CertBefore = mList.CertBefore;
                mModel.TDSMaster_Sections = mList.Sections;
                mModel.TDSMaster_DaysCert = mList.DaysCert;
                mModel.TDSMaster_DaysDeposit = mList.DaysDeposit;
                mModel.TDSMaster_Form = mList.Form;
                mModel.TDSMaster_DepositAfter = mList.DepositAfter;
                mModel.TDSMaster_DepositBefore = mList.DepositBefore;
                mModel.TDSMaster_Form15H = mList.Form15H;
                mModel.TDSMaster_Differ = mList.Differ;
                mModel.TDSMaster_FormType = mList.FormType;
                mModel.TDSMaster_FileAfter = mList.FileAfter;
                mModel.TDSMaster_FileBefore = mList.FileBefore;
                mModel.TDSMaster_FileDate = mList.FileDate != null ? mList.FileDate : DateTime.Now;

                var mList2 = ctxTFAT.TDSRates.Where(x => x.Code == mModel.TDSMaster_Code).ToList();
                List<TDSMasterVM> mList3 = new List<TDSMasterVM>();
                int n = 1;
                foreach (var eachvalue in mList2)
                {
                    mList3.Add(new TDSMasterVM()
                    {
                        TDSRates_Sno = eachvalue.Sno,
                        TDSRates_EffDate = eachvalue.EffDate,
                        TDSRates_LimitFrom = eachvalue.LimitFrom,
                        TDSRates_LimitTo = eachvalue.LimitTo,
                        TDSRates_TDSRate = eachvalue.TDSRate,
                        TDSRates_Cess = eachvalue.Cess,
                        TDSRates_SHECess = eachvalue.SHECess,
                        TDSRates_SurCharge = eachvalue.SurCharge,
                        TDSRates_Tax = eachvalue.Tax,
                        tEmpID = n,
                        tempIsDeleted = false
                    });
                    n = n + 1;
                }
                Session.Add("GridDataSession", mList3);
                mModel.GridDataVM = mList3;
            }
            else
            {
                mModel.TDSMaster_CertAfter = 0;
                mModel.TDSMaster_CertAuto = false;
                mModel.TDSMaster_CertBefore = 0;
                mModel.TDSMaster_Code = 0;
                mModel.TDSMaster_DaysCert = 0;
                mModel.TDSMaster_DaysDeposit = 0;
                mModel.TDSMaster_DepositAfter = 0;
                mModel.TDSMaster_DepositBefore = 0;
                mModel.TDSMaster_Differ = false;
                mModel.TDSMaster_FileAfter = 0;
                mModel.TDSMaster_FileBefore = 0;
                mModel.TDSMaster_FileDate = System.DateTime.Now;
                mModel.TDSMaster_Form = "";
                mModel.TDSMaster_Form15H = false;
                mModel.TDSMaster_FormType = 0;
                mModel.TDSMaster_Name = "";
                mModel.TDSMaster_PostCode = "";
                mModel.TDSMaster_Prefix = "";
                mModel.TDSMaster_Sections = "";
            }
            return View(mModel);
        }

        #region GridOperations
        // following action is used when row is added to grid in add mode
        [HttpPost]
        public ActionResult AddToTable(TDSMasterVM Model)
        {
            List<TDSMasterVM> objgriddetail = new List<TDSMasterVM>();
            if (Session["GridDataSession"] != null)
            {
                objgriddetail = (List<TDSMasterVM>)Session["GridDataSession"];
            }
            objgriddetail.Add(new TDSMasterVM()
            {
                TDSRates_Sno = Model.TDSRates_Sno,
                TDSRates_EffDate = Model.TDSRates_EffDate,
                TDSRates_LimitFrom = Model.TDSRates_LimitFrom,
                TDSRates_LimitTo = Model.TDSRates_LimitTo,
                TDSRates_TDSRate = Model.TDSRates_TDSRate,
                TDSRates_Cess = Model.TDSRates_Cess,
                TDSRates_SHECess = Model.TDSRates_SHECess,
                TDSRates_SurCharge = Model.TDSRates_SurCharge,
                TDSRates_Tax = Model.TDSRates_Tax,
                tEmpID = objgriddetail.Count + 1,
                tempIsDeleted = false
            });
            Session.Add("GridDataSession", objgriddetail);
            var html = ViewHelper.RenderPartialView(this, "GridDataView", new TDSMasterVM() { GridDataVM = objgriddetail, Mode = "Add" });
            return Json(new
            {
                GridDataVM = objgriddetail,
                Html = html,
                Mode = "Add"
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PickFromTable(TDSMasterVM Model)
        {
            var result = (List<TDSMasterVM>)Session["GridDataSession"];
            var result1 = result.Where(x => x.tEmpID == Model.tEmpID);
            foreach (var item in result1)
            {
                Model.TDSRates_Sno = item.TDSRates_Sno;
                Model.TDSRates_EffDate = item.TDSRates_EffDate;
                Model.TDSRates_LimitFrom = item.TDSRates_LimitFrom;
                Model.TDSRates_LimitTo = item.TDSRates_LimitTo;
                Model.TDSRates_TDSRate = item.TDSRates_TDSRate;
                Model.TDSRates_Cess = item.TDSRates_Cess;
                Model.TDSRates_SHECess = item.TDSRates_SHECess;
                Model.TDSRates_SurCharge = item.TDSRates_SurCharge;
                Model.TDSRates_Tax = item.TDSRates_Tax;
                Model.tEmpID = item.tEmpID;
                Model.GridDataVM = result;
            }
            return Json(new
            {
                Html = this.RenderPartialView("GridDataView", Model)
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult AddToTableEdit(TDSMasterVM Model)
        {
            var result = (List<TDSMasterVM>)Session["GridDataSession"];
            foreach (var item in result.Where(x => x.tEmpID == Model.tEmpID))
            {
                item.TDSRates_Sno = Model.TDSRates_Sno;
                item.TDSRates_EffDate = Model.TDSRates_EffDate;
                item.TDSRates_LimitFrom = Model.TDSRates_LimitFrom;
                item.TDSRates_LimitTo = Model.TDSRates_LimitTo;
                item.TDSRates_TDSRate = Model.TDSRates_TDSRate;
                item.TDSRates_Cess = Model.TDSRates_Cess;
                item.TDSRates_SHECess = Model.TDSRates_SHECess;
                item.TDSRates_SurCharge = Model.TDSRates_SurCharge;
                item.TDSRates_Tax = Model.TDSRates_Tax;
                item.tEmpID = Model.tEmpID;
                item.tempIsDeleted = false;
            }
            Session.Add("GridDataSession", result);
            var html = ViewHelper.RenderPartialView(this, "GridDataView", new TDSMasterVM() { GridDataVM = result, Mode = "Add" });
            return Json(new
            {
                GridDataVM = result,
                Html = html,
                Mode = "Add"
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteTableRow(int tEmpID, TDSMasterVM Model)
        {
            var result = (List<TDSMasterVM>)Session["GridDataSession"];
            result.Where(x => x.tEmpID == tEmpID).FirstOrDefault().tempIsDeleted = true;
            Session.Add("GridDataSession", result);
            var html = ViewHelper.RenderPartialView(this, "GridDataView", new TDSMasterVM() { GridDataVM = result });
            return Json(new { SelectedIndent = result, Html = html }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region SaveData
        public ActionResult SaveData(TDSMasterVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteTDSMaster(mModel);
                        if (mModel.Mode == "Delete")
                        {
                            transaction.Commit();
                            transaction.Dispose();
                            return Json(new
                            {
                                Status = "success",
                                Message = "Data is Deleted."
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    TDSMaster mobj = new TDSMaster();
                    if (mModel.Mode == "Edit")
                    {
                        mobj = ctxTFAT.TDSMaster.Where(x => (x.Code == mModel.TDSMaster_Code)).FirstOrDefault();
                    }
                    //mobj.Code = mModel.TDSMaster_Code.Value;
                    mobj.Name = mModel.TDSMaster_Name;
                    mobj.CertAuto = mModel.TDSMaster_CertAuto;
                    mobj.Prefix = mModel.TDSMaster_Prefix;
                    mobj.PostCode = mModel.TDSMaster_PostCode;
                    mobj.CertAfter = mModel.TDSMaster_CertAfter;
                    mobj.CertBefore = mModel.TDSMaster_CertBefore;
                    mobj.Sections = mModel.TDSMaster_Sections;
                    mobj.DaysCert = mModel.TDSMaster_DaysCert;
                    mobj.DaysDeposit = mModel.TDSMaster_DaysDeposit;
                    mobj.Form = mModel.TDSMaster_Form;
                    mobj.DepositAfter = mModel.TDSMaster_DepositAfter;
                    mobj.DepositBefore = mModel.TDSMaster_DepositBefore;
                    mobj.Form15H = mModel.TDSMaster_Form15H;
                    mobj.Differ = mModel.TDSMaster_Differ;
                    mobj.FormType =(byte)mModel.TDSMaster_FormType;
                    mobj.FileAfter = mModel.TDSMaster_FileAfter;
                    mobj.FileBefore = mModel.TDSMaster_FileBefore;
                    mobj.FileDate = ConvertDDMMYYTOYYMMDD(mModel.FileDateVM);
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mAUTHORISE;
                    mobj.Branch = mbranchcode;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD( System.DateTime.Now.ToShortDateString());
                    if (mModel.Mode == "Add")
                    {
                        mobj.Code = GetNextCode();
                        mModel.TDSMaster_Code = mobj.Code;
                        
                        mobj.ENTEREDBY = muserid;
                    }
                    if (mModel.Mode == "Add")
                    {
                        ctxTFAT.TDSMaster.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    SaveGridData(mModel);
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.Code, DateTime.Now, 0, "", "Save TDS Master", "NA");

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
                }
            }
            return Json(new { Status = "Success", id = "TDSMaster" }, JsonRequestBehavior.AllowGet);
        }
        public void SaveGridData(TDSMasterVM mModel)
        {
            // delete the existing data from the table
            var mList = ctxTFAT.TDSRates.Where(x => x.Code == mModel.TDSMaster_Code).ToList();
            if (mList.Count != 0)
            {
                ctxTFAT.TDSRates.RemoveRange(mList);
                ctxTFAT.SaveChanges();
            }
            var mList2 = (List<TDSMasterVM>)Session["GridDataSession"];
            if (mList2 != null)
            {
                var mList3 = ((List<TDSMasterVM>)Session["GridDataSession"]).Where(x => x.tempIsDeleted == false);
                foreach (var eachvalue in mList3)
                {
                    TDSRates mgriddata = new TDSRates();
                    mgriddata.Code = mModel.TDSMaster_Code.Value;
                    mgriddata.EffDate = eachvalue.TDSRates_EffDate.Value != null ? eachvalue.TDSRates_EffDate.Value : DateTime.Now;
                    mgriddata.LimitFrom = eachvalue.TDSRates_LimitFrom;
                    mgriddata.LimitTo = eachvalue.TDSRates_LimitTo;
                    mgriddata.TDSRate = eachvalue.TDSRates_TDSRate;
                    mgriddata.Cess = eachvalue.TDSRates_Cess;
                    mgriddata.SHECess = eachvalue.TDSRates_SHECess;
                    mgriddata.SurCharge = eachvalue.TDSRates_SurCharge;
                    mgriddata.Tax = eachvalue.TDSRates_Tax;
                    mgriddata.Sno = eachvalue.tEmpID;
                    mgriddata.ENTEREDBY = muserid;
                    mgriddata.LASTUPDATEDATE = DateTime.Now;
                    mgriddata.AUTHORISE = mAUTHORISE;
                    mgriddata.AUTHIDS = muserid;
                    ctxTFAT.TDSRates.Add(mgriddata);
                    ctxTFAT.SaveChanges();
                }
            }
            Session["GridDataSession"] = null;
        }

        public int GetNextCode()
        {
            int nextcode = (from x in ctxTFAT.TDSMaster select x.Code).Max() ;
            return (++nextcode);
        }

        public ActionResult DeleteTDSMaster(TDSMasterVM mModel)
        {
            // iX9: Check for Active Master TDSMaster
            string mactivestring = "";
            var mactive1 = ctxTFAT.TDSChallan.Where(x => (x.TDSCode == mModel.TDSMaster_Code)).Select(x => x.TDSCode).FirstOrDefault();
            if (mactive1 != null) { mactivestring = mactivestring + "\nTDSChallan: " + mactive1; }
            var mactive2 = ctxTFAT.TDSPayments.Where(x => (x.TDSCode == mModel.TDSMaster_Code)).Select(x => x.TDSCode).FirstOrDefault();
            if (mactive2 != null) { mactivestring = mactivestring + "\nTDSPayments: " + mactive2; }
            var mactive3 = ctxTFAT.TDSRates.Where(x => (x.Code == mModel.TDSMaster_Code)).Select(x => x.Code).FirstOrDefault();
            if (mactive3 != null) { mactivestring = mactivestring + "\nTDSRates: " + mactive3; }
            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "Active Item, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var mList = ctxTFAT.TDSMaster.Where(x => (x.Code == mModel.TDSMaster_Code)).FirstOrDefault();
            ctxTFAT.TDSMaster.Remove(mList);
            var mList2 = ctxTFAT.TDSRates.Where(x => x.Code == mModel.TDSMaster_Code).ToList();
            ctxTFAT.TDSRates.RemoveRange(mList2);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}