using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
using EntitiModel;
using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class EmployeeGradesController : BaseController
    {
        //private //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        //private IBusinessCommon mBuss = new BusinessCommon();
        private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mAUTHORISE = "A00";
        private static int mdocument = 0;

        #region GetLists
        #endregion GetLists

        // GET: Accounts/EmployeeGrades
        public ActionResult Index(EmployeeGradesVM mModel)
        {
            //GetAllMenu(Session["ModuleName"].ToString());
            ////AccountingGetAllMenu(Session["ModuleName"].ToString());
            ////VehicleGetAllMenu(Session["ModuleName"].ToString());
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "A");
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                mdocument = mModel.Document;
                var mList = ctxTFAT.Grade.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                mModel.Grade_Code = mList.Code;
                mModel.Grade_Name = mList.Name;
                mModel.Grade_CrewSize = mList.CrewSize;
            }
            else
            {
                mModel.Grade_CrewSize = 0;
                mModel.Grade_Name = "";
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(EmployeeGradesVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteEmployeeGrades(mModel);
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
                    Grade mobj = new Grade();
                    if (mModel.Mode == "Edit")
                    {
                        mobj = ctxTFAT.Grade.Where(x => (x.Code == mModel.Grade_Code)).FirstOrDefault();
                    }
                    mobj.Code = mModel.Grade_Code;
                    mobj.Name = mModel.Grade_Name;
                    mobj.CrewSize = mModel.Grade_CrewSize;
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mAUTHORISE;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mModel.Mode == "Add")
                    {
                        mobj.Code = GetNextCode();
                    }
                    if (mModel.Mode == "Add")
                    {
                        ctxTFAT.Grade.Add(mobj);
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
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "EmployeeGrades" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "EmployeeGrades" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                }
            }
            return Json(new { Status = "Success", id = "EmployeeGrades" }, JsonRequestBehavior.AllowGet);
        }

        public int GetNextCode()
        {
            var nextcode = (from x in ctxTFAT.Grade select (int?)x.Code).Max() ?? 0;
            return (++nextcode);
        }

        public ActionResult DeleteEmployeeGrades(EmployeeGradesVM mModel)
        {
            // iX9: Check for Active Master Grade
            string mactivestring = "";
            var mactive1 = ctxTFAT.EmpGradeRates.Where(x => (x.Code == mModel.Grade_Code)).Select(x => x.Code).FirstOrDefault();
            if (mactive1 == 0) { mactivestring = mactivestring + "\nEmpGradeRates: " + mactive1; }
            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "Active Item, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var mList = ctxTFAT.Grade.Where(x => (x.Code == mModel.Grade_Code)).FirstOrDefault();
            ctxTFAT.Grade.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}