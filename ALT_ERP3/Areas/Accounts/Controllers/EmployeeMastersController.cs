using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class EmployeeMastersController : BaseController
    {
        //private //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        //private IBusinessCommon mBuss = new BusinessCommon();
        private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mAUTHORISE = "A00";
        private static string mdocument = "";

        #region GetLists
        public List<SelectListItem> GetOTUnitList()
        {
            List<SelectListItem> CallOTUnitList = new List<SelectListItem>();
            CallOTUnitList.Add(new SelectListItem { Value = "Hour", Text = "Hour" });
            CallOTUnitList.Add(new SelectListItem { Value = "Day", Text = "Day" });
            return CallOTUnitList;
        }
        public JsonResult AutoCompleteDept(string term)
        {
            return Json((from m in ctxTFAT.Dept
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteCateNo(string term)
        {
            return Json((from m in ctxTFAT.EmpCategory
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteEmpGrade(string term)
        {
            return Json((from m in ctxTFAT.Grade
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        #endregion GetLists

        // GET: Accounts/EmployeeMasters
        public ActionResult Index(EmployeeMastersVM mModel)
        {
            //GetAllMenu(Session["ModuleName"].ToString());
            ////AccountingGetAllMenu(Session["ModuleName"].ToString());
            ////VehicleGetAllMenu(Session["ModuleName"].ToString());
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "A");
            mdocument = mModel.Document;
            mModel.Employee_Dob = DateTime.Now;
            mModel.Employee_Doc = DateTime.Now;
            mModel.Employee_Doj = DateTime.Now;
            mModel.Employee_Doi = DateTime.Now;
            mModel.Employee_Dol = DateTime.Now;
            mModel.Employee_Dor = DateTime.Now;
            mModel.OTUnitList = GetOTUnitList();
            mModel.Employee_EmpID = mModel.Document;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.Employee.Where(x => (x.EmpID == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    var mDept = ctxTFAT.Dept.Where(x => x.Code == mList.Dept).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mCateNo = ctxTFAT.EmpCategory.Where(x => x.Code == mList.CateNo).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mEmpGrade = ctxTFAT.Grade.Where(x => x.Code == mList.EmpGrade).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    mModel.Employee_Dept = mDept != null ? mDept.Code : 0;
                    mModel.DeptName = mDept != null ? mDept.Name : "";
                    mModel.Employee_CateNo = mCateNo != null ? mCateNo.Code : 0;
                    mModel.CateNoName = mCateNo != null ? mCateNo.Name : "";
                    mModel.Employee_EmpGrade = mEmpGrade != null ? mEmpGrade.Code : 0;
                    mModel.EmpGradeName = mEmpGrade != null ? mEmpGrade.Name : "";
                    mModel.Employee_EmpID = mList.EmpID;
                    mModel.Employee_Name = mList.Name;
                    mModel.Employee_EmpCompId = mList.EmpCompId;
                    mModel.Employee_CitizenId = mList.CitizenId;
                    mModel.Employee_AadharNo = mList.AadharNo;
                    mModel.Employee_Dob = mList.Dob != null ? mList.Dob.Value : DateTime.Now;
                    mModel.Employee_Doc = mList.Doc != null ? mList.Doc.Value : DateTime.Now;
                    mModel.Employee_Doj = mList.Doj != null ? mList.Doj : DateTime.Now;
                    mModel.Employee_Doi = mList.Doi != null ? mList.Doi.Value : DateTime.Now;
                    mModel.Employee_Dol = mList.Dol != null ? mList.Dol.Value : DateTime.Now;
                    mModel.Employee_Dor = mList.Dor != null ? mList.Dor.Value : DateTime.Now;
                    mModel.Employee_EmpType = mList.EmpType != null ? mList.EmpType.Value : 0;
                    mModel.Employee_GradeNo = mList.GradeNo;
                    mModel.Employee_BasicType = mList.BasicType;
                    mModel.Employee_Basic = mList.Basic;
                    mModel.Employee_Monthly = mList.Monthly;
                    mModel.Employee_MonthlyBasic = mList.MonthlyBasic;
                    mModel.Employee_OTRate = mList.OTRate != null ? mList.OTRate.Value : 0;
                    mModel.Employee_OTUnit = mList.OTUnit;
                }
            }
            else
            {
                mModel.Employee_AadharNo = "";
                mModel.Employee_AccNo = "";
                mModel.Employee_AnnualBonus = 0;
                mModel.Employee_AppBranch = "";
                mModel.Employee_Basic = 0;
                mModel.Employee_BasicType = "";
                mModel.Employee_BkCode = "";
                mModel.Employee_CateNo = 0;
                mModel.Employee_CitizenId = "";
                mModel.Employee_CPRExpiDt = System.DateTime.Now;
                mModel.Employee_CPRIssuDt = System.DateTime.Now;
                mModel.Employee_CPRNo = "";
                mModel.Employee_DaysPerPeriod = 0;
                mModel.Employee_Dept = 0;
                mModel.Employee_Dob = System.DateTime.Now;
                mModel.Employee_Doc = System.DateTime.Now;
                mModel.Employee_Doi = System.DateTime.Now;
                mModel.Employee_Doj = System.DateTime.Now;
                mModel.Employee_Dol = System.DateTime.Now;
                mModel.Employee_Dor = System.DateTime.Now;
                mModel.Employee_EmpCompId = "";
                mModel.Employee_EmpGrade = 0;
                mModel.Employee_EmpID = "";
                mModel.Employee_EmpType = 0;
                mModel.Employee_ESICAppl = 0;
                mModel.Employee_ESICNo = "";
                mModel.Employee_FPFAppl = 0;
                mModel.Employee_FPFNo = "";
                mModel.Employee_GradeNo = "";
                mModel.Employee_HolidayAppl = 0;
                mModel.Employee_HrsPerDay = 0;
                mModel.Employee_LeftReason = "";
                mModel.Employee_LnOpen = 0;
                mModel.Employee_LoanOpen = 0;
                mModel.Employee_LWFappl = 0;
                mModel.Employee_LWFNo = "";
                mModel.Employee_MaxLoan = 0;
                mModel.Employee_Monthly = false;
                mModel.Employee_MonthlyBasic = false;
                mModel.Employee_Name = "";
                mModel.Employee_NonPension = false;
                mModel.Employee_OTRate = 0;
                mModel.Employee_OTUnit = "";
                mModel.Employee_PAN = "";
                mModel.Employee_PaymentMode = 0;
                mModel.Employee_PF = 0;
                mModel.Employee_PFAmt = 0;
                mModel.Employee_PfAppl = 0;
                mModel.Employee_PFCode = "";
                mModel.Employee_PfNo = "";
                mModel.Employee_PostNo = 0;
                mModel.Employee_PostNoJoin = 0;
                mModel.Employee_Prefix = 0;
                mModel.Employee_ProfAmt = 0;
                mModel.Employee_ProfAppl = 0;
                mModel.Employee_PTNo = "";
                mModel.Employee_RateHour = 0;
                mModel.Employee_Shift = 0;
                mModel.Employee_Status = "";
                mModel.Employee_SumPresent = 0;
                mModel.Employee_TDSAppl = 0;
                mModel.Employee_TotLoan = 0;
                mModel.Employee_UnBasic = 0;
                mModel.Employee_UpToDate = System.DateTime.Now;
                mModel.Employee_WkDays = 0;
                mModel.Employee_WkHoli1 = "";
                mModel.Employee_WkHoli2 = "";
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(EmployeeMastersVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteEmployeeMasters(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    Employee mobj = new Employee();
                    bool mAdd = true;
                    if (ctxTFAT.Employee.Where(x => (x.EmpID == mModel.Employee_EmpID)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.Employee.Where(x => (x.EmpID == mModel.Employee_EmpID)).FirstOrDefault();
                        mAdd = false;
                    }
                    mobj.EmpID = mModel.Employee_EmpID;
                    mobj.Name = mModel.Employee_Name;
                    mobj.EmpCompId = mModel.Employee_EmpCompId;
                    mobj.CitizenId = mModel.Employee_CitizenId;
                    mobj.AadharNo = mModel.Employee_AadharNo;
                    mobj.Dept = mModel.Employee_Dept;
                    mobj.CateNo = mModel.Employee_CateNo;
                    mobj.Dob = ConvertDDMMYYTOYYMMDD(mModel.Employee_DobVM);
                    mobj.Doc = ConvertDDMMYYTOYYMMDD(mModel.Employee_DocVM);
                    mobj.Doj = ConvertDDMMYYTOYYMMDD(mModel.Employee_DojVM);
                    mobj.Doi = ConvertDDMMYYTOYYMMDD(mModel.Employee_DoiVM);
                    mobj.Dol = ConvertDDMMYYTOYYMMDD(mModel.Employee_DolVM);
                    mobj.Dor = ConvertDDMMYYTOYYMMDD(mModel.Employee_DorVM);
                    mobj.EmpGrade = mModel.Employee_EmpGrade;
                    mobj.EmpType = mModel.Employee_EmpType;
                    mobj.GradeNo = mModel.Employee_GradeNo;
                    mobj.BasicType = mModel.Employee_BasicType;
                    mobj.Basic = mModel.Employee_Basic;
                    mobj.Monthly = mModel.Employee_Monthly;
                    mobj.MonthlyBasic = mModel.Employee_MonthlyBasic;
                    mobj.OTRate = mModel.Employee_OTRate;
                    mobj.OTUnit = mModel.Employee_OTUnit;
                    // iX9: default values for the fields not used @Form
                    mobj.AccNo = "";
                    mobj.AnnualBonus = 0;
                    mobj.AppBranch = "";
                    mobj.BkCode = "";
                    mobj.CPRExpiDt = System.DateTime.Now;
                    mobj.CPRIssuDt = System.DateTime.Now;
                    mobj.CPRNo = "";
                    mobj.DaysPerPeriod = 0;
                    mobj.ESICAppl = 0;
                    mobj.ESICNo = "";
                    mobj.FPFAppl = 0;
                    mobj.FPFNo = "";
                    mobj.HolidayAppl = 0;
                    mobj.HrsPerDay = 0;
                    mobj.LeftReason = "";
                    mobj.LnOpen = 0;
                    mobj.LoanOpen = 0;
                    mobj.LWFappl = 0;
                    mobj.LWFNo = "";
                    mobj.MaxLoan = 0;
                    mobj.NonPension = false;
                    mobj.PAN = "";
                    mobj.PaymentMode = 0;
                    mobj.PF = 0;
                    mobj.PFAmt = 0;
                    mobj.PfAppl = 0;
                    mobj.PFCode = "";
                    mobj.PfNo = "";
                    mobj.PostNo = 0;
                    mobj.PostNoJoin = 0;
                    mobj.Prefix = 0;
                    mobj.ProfAmt = 0;
                    mobj.ProfAppl = 0;
                    mobj.PTNo = "";
                    mobj.RateHour = 0;
                    mobj.Shift = 0;
                    mobj.Status = "";
                    mobj.SumPresent = 0;
                    mobj.TDSAppl = 0;
                    mobj.TotLoan = 0;
                    mobj.UnBasic = 0;
                    mobj.UpToDate = System.DateTime.Now;
                    mobj.WkDays = 0;
                    mobj.WkHoli1 = "";
                    mobj.WkHoli2 = "";
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mAUTHORISE;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mAdd == true)
                    {
                        ctxTFAT.Employee.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    transaction.Commit();
                    transaction.Dispose();
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "EmployeeMasters" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "EmployeeMasters" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "EmployeeMasters" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "EmployeeMasters" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteEmployeeMasters(EmployeeMastersVM mModel)
        {
            if (mModel.Employee_EmpID == null || mModel.Employee_EmpID == "")
            {
                return Json(new
                {
                    Message = "EmpID not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            // iX9: Check for Active Master Employee
            string mactivestring = "";
            //var mactive1 = ctxTFAT.AllowMaster.Where(x => (x.Code == mModel.Employee_EmpID)).Select(x => x.Code).FirstOrDefault();
            var mactive1 = ctxTFAT.AllowMaster.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive1 != null) { mactivestring = mactivestring + "\nAllowMaster: " + mactive1; }
            var mactive2 = ctxTFAT.AssetEmp.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive2 != null) { mactivestring = mactivestring + "\nAssetEmp: " + mactive2; }
            var mactive3 = ctxTFAT.AssetToEmp.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive3 != null) { mactivestring = mactivestring + "\nAssetToEmp: " + mactive3; }
            var mactive4 = ctxTFAT.DailyAtten.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive4 != null) { mactivestring = mactivestring + "\nDailyAtten: " + mactive4; }
            var mactive5 = ctxTFAT.DailyLeave.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive5 != null) { mactivestring = mactivestring + "\nDailyLeave: " + mactive5; }
            var mactive6 = ctxTFAT.DailyOT.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive6 != null) { mactivestring = mactivestring + "\nDailyOT: " + mactive6; }
            var mactive7 = ctxTFAT.DailyTime.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive7 != null) { mactivestring = mactivestring + "\nDailyTime: " + mactive7; }
            var mactive8 = ctxTFAT.EmpAddress.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive8 != null) { mactivestring = mactivestring + "\nEmpAddress: " + mactive8; }
            var mactive9 = ctxTFAT.EmpAllowDeduct.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive9 != null) { mactivestring = mactivestring + "\nEmpAllowDeduct: " + mactive9; }
            var mactive10 = ctxTFAT.EmpFamily.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive10 != null) { mactivestring = mactivestring + "\nEmpFamily: " + mactive10; }
            var mactive11 = ctxTFAT.EmpHoursSetup.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive11 != null) { mactivestring = mactivestring + "\nEmpHoursSetup: " + mactive11; }
            var mactive12 = ctxTFAT.EmployeeHistory.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.Name).FirstOrDefault();
            if (mactive12 != null) { mactivestring = mactivestring + "\nEmployeeHistory: " + mactive12; }
            var mactive13 = ctxTFAT.EmployeeLeave.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive13 != null) { mactivestring = mactivestring + "\nEmployeeLeave: " + mactive13; }
            var mactive14 = ctxTFAT.EmployeeWork.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive14 != null) { mactivestring = mactivestring + "\nEmployeeWork: " + mactive14; }
            var mactive15 = ctxTFAT.EmpTravel.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive15 != null) { mactivestring = mactivestring + "\nEmpTravel: " + mactive15; }
            var mactive16 = ctxTFAT.EmpTravel.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive16 != null) { mactivestring = mactivestring + "\nEmpTravel: " + mactive16; }
            var mactive17 = ctxTFAT.EmpTraveldet.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive17 != null) { mactivestring = mactivestring + "\nEmpTraveldet: " + mactive17; }
            var mactive18 = ctxTFAT.HourlyAtten.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive18 != null) { mactivestring = mactivestring + "\nHourlyAtten: " + mactive18; }
            var mactive19 = ctxTFAT.HoursDetails.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive19 != null) { mactivestring = mactivestring + "\nHoursDetails: " + mactive19; }
            var mactive20 = ctxTFAT.HoursFromMapping.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive20 != null) { mactivestring = mactivestring + "\nHoursFromMapping: " + mactive20; }
            var mactive23 = ctxTFAT.LoanApplication.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive23 != null) { mactivestring = mactivestring + "\nLoanApplication: " + mactive23; }
            var mactive24 = ctxTFAT.LoanDet.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive24 != null) { mactivestring = mactivestring + "\nLoanDet: " + mactive24; }
            var mactive25 = ctxTFAT.LoanRecover.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive25 != null) { mactivestring = mactivestring + "\nLoanRecover: " + mactive25; }
            var mactive28 = ctxTFAT.Monthly.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive28 != null) { mactivestring = mactivestring + "\nMonthly: " + mactive28; }
            var mactive29 = ctxTFAT.MonthlyHourly.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive29 != null) { mactivestring = mactivestring + "\nMonthlyHourly: " + mactive29; }
            var mactive30 = ctxTFAT.MonthlyLeave.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive30 != null) { mactivestring = mactivestring + "\nMonthlyLeave: " + mactive30; }
            var mactive31 = ctxTFAT.MonthlyLoan.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive31 != null) { mactivestring = mactivestring + "\nMonthlyLoan: " + mactive31; }
            var mactive32 = ctxTFAT.TDSBank.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive32 != null) { mactivestring = mactivestring + "\nTDSBank: " + mactive32; }
            var mactive33 = ctxTFAT.TDSForm16.Where(x => (x.EmpID == mModel.Employee_EmpID)).Select(x => x.EmpID).FirstOrDefault();
            if (mactive33 != null) { mactivestring = mactivestring + "\nTDSForm16: " + mactive33; }
            var mactive35 = ctxTFAT.Warehouse.Where(x => (x.Incharge == mModel.Employee_EmpID)).Select(x => x.Name).FirstOrDefault();
            if (mactive35 != null) { mactivestring = mactivestring + "\nWarehouse: " + mactive35; }
            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "Active Item, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var mList = ctxTFAT.Employee.Where(x => (x.EmpID == mModel.Employee_EmpID)).FirstOrDefault();
            ctxTFAT.Employee.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}