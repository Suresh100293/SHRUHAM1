using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
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

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class LabourGradesController : BaseController
    {
        private string mauthorise = "A00";
        private DataTable table = new DataTable();

        #region GetLists
        public List<SelectListItem> GetUnitList()
        {
            List<SelectListItem> CallUnitList = new List<SelectListItem>();
            CallUnitList.Add(new SelectListItem { Value = "Hour", Text = "Hour" });
            CallUnitList.Add(new SelectListItem { Value = "Day", Text = "Day" });
            return CallUnitList;
        }

        #endregion GetLists

        // GET: Accounts/LabourGrades
        public ActionResult Index(LabourGradesVM Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, "Employee Labour Grades", "", DateTime.Now, 0, "", "", "A");
            Model.EmpGradeRates_EffDate = DateTime.Now;
            Model.UnitList = GetUnitList();
            Session["GridSessionEmpGradeRates"] = null;
            Model.Grade_Code = Model.Document;
            if ((Model.Mode == "Edit") || (Model.Mode == "View") || (Model.Mode == "Delete"))
            {
                var mList = ctxTFAT.Grade.Where(x => (x.Code == Model.Document)).FirstOrDefault();
                if (mList != null)
                {
                    Model.Grade_Code = (int)(mList.Code != null ? mList.Code : 0);
                    Model.Grade_Name = mList.Name;
                    Model.Grade_CrewSize = (int)(mList.CrewSize != null ? mList.CrewSize : 0);
                    Model.AUTHORISE = mList.AUTHORISE;
                    mauthorise = Model.AUTHORISE;

                    var mListEmpGradeRates2 = ctxTFAT.EmpGradeRates.Where(x => x.Code == Model.Grade_Code).ToList();
                    List<LabourGradesVM> mListEmpGradeRates3 = new List<LabourGradesVM>();
                    int n = 1;
                    foreach (var eachvalue in mListEmpGradeRates2)
                    {
                        mListEmpGradeRates3.Add(new LabourGradesVM()
                        {
                            EmpGradeRates_Sno = eachvalue.Sno,
                            EmpGradeRates_EffDate = eachvalue.EffDate,
                            EmpGradeRates_Rate = eachvalue.Rate != null ? eachvalue.Rate.Value : 0,
                            EmpGradeRates_Unit = eachvalue.Unit ?? "",
                            EmpGradeRates_OTRate = eachvalue.OTRate != null ? eachvalue.OTRate.Value : 0,
                            tempId = n,
                            tempIsDeleted = false
                        });
                        ++n;
                    }
                    Session.Add("GridSessionEmpGradeRates", mListEmpGradeRates3);
                    Model.GridEmpGradeRatesVM = mListEmpGradeRates3;

                    if (mauthorise.StartsWith("A"))
                    {
                        var mAuth = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == "LBGRD").Select(x => new { x.AuthLock, x.AuthReq }).FirstOrDefault();
                        if (mAuth != null)
                        {
                            if (mAuth.AuthReq == true && mAuth.AuthLock == true)
                            {
                                Model.CheckMode = true;
                                Model.Mode = "View";
                                Model.Message = "Document is Already Authorised Cant Edit";
                            }
                        }
                    }
                }
            }
            else
            {

            }
            return View(Model);
        }

        public ActionResult MoveNextPrevious(string Mode, string mdocument)
        {
            string mvar;
            if (Mode == "N")
            {
                mvar = Fieldoftable("Grade", "Top 1 Code", "Code>'" + mdocument + "' order by Code", "T") ?? "";
            }
            else
            {
                mvar = Fieldoftable("Grade", "Top 1 Code", "Code<'" + mdocument + "' order by Code desc", "T") ?? "";
            }
            return Json(new { Status = "Success", data = mvar }, JsonRequestBehavior.AllowGet);
        }

        #region GridOperations
        // following action is used when row is added to grid in add mode
        [HttpPost]
        public ActionResult AddToTable(LabourGradesVM Model)
        {
            List<LabourGradesVM> objgriddetail = new List<LabourGradesVM>();
            if (Session["GridSessionEmpGradeRates"] != null)
            {
                objgriddetail = (List<LabourGradesVM>)Session["GridSessionEmpGradeRates"];
            }
            objgriddetail.Add(new LabourGradesVM()
            {
                EmpGradeRates_Sno = Model.EmpGradeRates_Sno,
                EmpGradeRates_EffDate = Model.EmpGradeRates_EffDate,
                EmpGradeRates_Rate = Model.EmpGradeRates_Rate,
                EmpGradeRates_Unit = Model.EmpGradeRates_Unit,
                EmpGradeRates_OTRate = Model.EmpGradeRates_OTRate,
                tempId = objgriddetail.Count + 1,
                tempIsDeleted = false
            });
            Session.Add("GridSessionEmpGradeRates", objgriddetail);
            string html = ViewHelper.RenderPartialView(this, "GridDataView", new LabourGradesVM() { GridEmpGradeRatesVM = objgriddetail, Mode = "Add", UnitList = GetUnitList() });
            return Json(new
            {
                GridEmpGradeRatesVM = objgriddetail,
                Html = html,
                Mode = "Add"
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PickFromTable(LabourGradesVM Model)
        {
            var result = (List<LabourGradesVM>)Session["GridSessionEmpGradeRates"];
            var result1 = result.Where(x => x.tempId == Model.tempId);
            foreach (var item in result1)
            {
                Model.EmpGradeRates_Sno = item.EmpGradeRates_Sno;
                Model.EmpGradeRates_EffDate = item.EmpGradeRates_EffDate;
                Model.EmpGradeRates_Rate = item.EmpGradeRates_Rate;
                Model.EmpGradeRates_Unit = item.EmpGradeRates_Unit;
                Model.EmpGradeRates_OTRate = item.EmpGradeRates_OTRate;
                Model.tempId = item.tempId;
                Model.GridEmpGradeRatesVM = result;
            }
            Model.UnitList = GetUnitList();
            return Json(new
            {
                Html = this.RenderPartialView("GridDataView", Model)
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddToTableEdit(LabourGradesVM Model)
        {
            var result = (List<LabourGradesVM>)Session["GridSessionEmpGradeRates"];
            foreach (var item in result.Where(x => x.tempId == Model.tempId))
            {
                item.EmpGradeRates_Sno = Model.EmpGradeRates_Sno;
                item.EmpGradeRates_EffDate = Model.EmpGradeRates_EffDate;
                item.EmpGradeRates_Rate = Model.EmpGradeRates_Rate;
                item.EmpGradeRates_Unit = Model.EmpGradeRates_Unit;
                item.EmpGradeRates_OTRate = Model.EmpGradeRates_OTRate;
                item.tempId = Model.tempId;
                item.tempIsDeleted = false;
            }
            Session.Add("GridSessionEmpGradeRates", result);
            string html = ViewHelper.RenderPartialView(this, "GridDataView", new LabourGradesVM() { GridEmpGradeRatesVM = result, Mode = "Add", UnitList = GetUnitList() });
            return Json(new
            {
                GridEmpGradeRatesVM = result,
                Html = html,
                Mode = "Add"
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteTableRow(int tempId, LabourGradesVM Model)
        {
            var result = (List<LabourGradesVM>)Session["GridSessionEmpGradeRates"];
            result.Where(x => x.tempId == tempId).FirstOrDefault().tempIsDeleted = true;
            Session.Add("GridSessionEmpGradeRates", result);
            string html = ViewHelper.RenderPartialView(this, "GridDataView", new LabourGradesVM() { GridEmpGradeRatesVM = result });
            return Json(new { SelectedIndent = result, Html = html }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region SaveData
        public ActionResult SaveData(LabourGradesVM Model)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (Model.Mode == "Delete")
                    {
                        DeleteData(Model);
                        transaction.Commit();
                        transaction.Dispose();
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    Grade mobj = new Grade();
                    bool mAdd = true;
                    if (ctxTFAT.Grade.Where(x => (x.Code == Model.Grade_Code)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.Grade.Where(x => (x.Code == Model.Grade_Code)).FirstOrDefault();
                        mAdd = false;
                    }
                    mobj.Code = Model.Grade_Code;
                    mobj.Name = Model.Grade_Name;
                    mobj.CrewSize = Model.Grade_CrewSize;
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = DateTime.Now;
                    mobj.AUTHORISE = "A00";
                    if (mAdd == true)
                    {
                        mobj.Code = GetNextCode();
                        Model.Grade_Code = mobj.Code;
                    }
                    if (mAdd == true)
                    {
                        ctxTFAT.Grade.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    string mNewCode = mobj.Code.ToString();
                    SaveGridEmpGradeRates(Model);
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, mNewCode.ToString(), DateTime.Now, 0, mNewCode.ToString(), "", "A");
                    Session["GridSessionEmpGradeRates"] = null;
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { ex1.InnerException.Message, Status = "Error", id = "LabourGrades" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { ex.InnerException.Message, Status = "Error", id = "LabourGrades" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "LabourGrades" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", NewSrl = Model.Grade_Code, id = "LabourGrades" }, JsonRequestBehavior.AllowGet);
        }

        public void SaveGridEmpGradeRates(LabourGradesVM Model)
        {
            // delete the existing data from the table
            var mList = ctxTFAT.EmpGradeRates.Where(x => x.Code == Model.Grade_Code).ToList();
            if (mList.Count != 0)
            {
                ctxTFAT.EmpGradeRates.RemoveRange(mList);
                ctxTFAT.SaveChanges();
            }
            var mList2 = (List<LabourGradesVM>)Session["GridSessionEmpGradeRates"];
            if (mList2 != null)
            {
                var mList3 = ((List<LabourGradesVM>)Session["GridSessionEmpGradeRates"]).Where(x => x.tempIsDeleted == false);
                int xCnt = 1;
                foreach (var eachvalue in mList3)
                {
                    EmpGradeRates mgriddata = new EmpGradeRates();
                    mgriddata.Code = Model.Grade_Code;
                    mgriddata.EffDate = eachvalue.EmpGradeRates_EffDate;
                    mgriddata.Rate = eachvalue.EmpGradeRates_Rate;
                    mgriddata.Unit = eachvalue.EmpGradeRates_Unit;
                    mgriddata.OTRate = eachvalue.EmpGradeRates_OTRate;
                    mgriddata.Sno = eachvalue.tempId;
                    mgriddata.ENTEREDBY = muserid;
                    mgriddata.LASTUPDATEDATE = DateTime.Now;
                    mgriddata.AUTHORISE = "A00";
                    mgriddata.AUTHIDS = muserid;
                    ctxTFAT.EmpGradeRates.Add(mgriddata);
                    ctxTFAT.SaveChanges();

                    ++xCnt;
                }
            }
        }

        public int GetNextCode()
        {
            var nextcode = (from x in ctxTFAT.EmpGradeRates select (int?)x.Code).Max() ?? 0;
            return (++nextcode);
        }

        public ActionResult DeleteData(LabourGradesVM Model)
        {
            // iX9: Check for Active Master Grade
            string mactivestring = "";
            var mactive1 = ctxTFAT.EmpGradeRates.Where(x => (x.Code == Model.Grade_Code)).Select(x => x.Code).FirstOrDefault();
            if (mactive1 == 0) { mactivestring = mactivestring + "\nEmpGradeRates: " + mactive1; }
            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "Active Item, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            DeUpdate(Model);
            return Json(new { Status = "Success", Message = "The Document is Deleted." }, JsonRequestBehavior.AllowGet);
        }

        private void DeUpdate(LabourGradesVM Model)
        {
            var mList = ctxTFAT.Grade.Where(x => (x.Code == Model.Grade_Code)).ToList();
            if (mList != null)
            {
                ctxTFAT.Grade.RemoveRange(mList);
            }
            var mListEmpGradeRates2 = ctxTFAT.EmpGradeRates.Where(x => x.Code == Model.Grade_Code).ToList();
            if (mListEmpGradeRates2 != null)
            {
                ctxTFAT.EmpGradeRates.RemoveRange(mListEmpGradeRates2);
            }
            ctxTFAT.SaveChanges();
        }
        #endregion SaveData
    }
}