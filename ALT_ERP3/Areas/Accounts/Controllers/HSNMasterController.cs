using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class HSNMasterController : BaseController
    {
        //private tfatEntities ctxTFAT = new tfatEntities();
        //private IBusinessCommon mBuss = new BusinessCommon();
        private int mlocation = 100001;
        //private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        //private string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        //private string muserid = System.Web.HttpContext.Current.Session["UserId"].ToString();
        //private string mperiod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        private int mnewrecordkey = 0;
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private DataTable table = new DataTable();

        #region GetLists
        public JsonResult AutoCompleteGrp(string term)
        {
            return Json((from m in ctxTFAT.HSNCategory
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteUnit(string term)
        {
            if (term == "")
            {
                return Json((from m in ctxTFAT.UnitMaster
                             select new { Name = m.Name, Code = m.Code }).Take(10).ToArray(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json((from m in ctxTFAT.UnitMaster
                             where m.Name.ToLower().Contains(term.ToLower())
                             select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult AutoCompleteIGSTIn(string term)
        {
            return Json((from m in ctxTFAT.Master
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteIGSTOut(string term)
        {
            return Json((from m in ctxTFAT.Master
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteSGSTIn(string term)
        {
            return Json((from m in ctxTFAT.Master
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteSGSTOut(string term)
        {
            return Json((from m in ctxTFAT.Master
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteCGSTIn(string term)
        {
            return Json((from m in ctxTFAT.Master
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteCGSTOut(string term)
        {
            return Json((from m in ctxTFAT.Master
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteCessIn(string term)
        {
            return Json((from m in ctxTFAT.Master
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteCessOut(string term)
        {
            return Json((from m in ctxTFAT.Master
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public string GetNewCode()
        {
            var mSQLQuery = "select Top 1 Code From HSNMaster Order by cast(Code as int) desc";
            SqlConnection conn = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand(mSQLQuery, conn);
            conn.Open();
            cmd.CommandTimeout = 0;
            var mName = (string)cmd.ExecuteScalar();
            cmd.Dispose();
            conn.Close();
            conn.Dispose();
            //var mName = ctxTFAT.HSNMaster.OrderByDescending(X =>X.Code).Select(X => X.Code).FirstOrDefault();

            if (String.IsNullOrEmpty(mName))
            {
                mName = "100000";
            }
            else
            {
                mName = (Convert.ToInt32(mName) + 1).ToString();
            }
            return mName;
        }


        #endregion GetLists

        public ActionResult Index(HSNMasterVM mModel)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mModel.HSNRates_EffDate = DateTime.Now;
            Session["GridDataSession"] = null;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                mdocument = mModel.Document;
                var mList = ctxTFAT.HSNMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                var mGrp = ctxTFAT.HSNCategory.Where(x => x.Code == mList.Grp).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mUnit = ctxTFAT.UnitMaster.Where(x => x.Code == mList.Unit).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mIGSTIn = ctxTFAT.Master.Where(x => x.Code == mList.IGSTIn).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mIGSTOut = ctxTFAT.Master.Where(x => x.Code == mList.IGSTOut).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mSGSTIn = ctxTFAT.Master.Where(x => x.Code == mList.SGSTIn).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mSGSTOut = ctxTFAT.Master.Where(x => x.Code == mList.SGSTOut).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mCGSTIn = ctxTFAT.Master.Where(x => x.Code == mList.CGSTIn).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mCGSTOut = ctxTFAT.Master.Where(x => x.Code == mList.CGSTOut).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mCessIn = ctxTFAT.Master.Where(x => x.Code == mList.CessIn).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mCessOut = ctxTFAT.Master.Where(x => x.Code == mList.CessOut).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                mModel.HSNMaster_Grp = mGrp != null ? mGrp.Code : 0;
                mModel.GrpName = mGrp != null ? mGrp.Name : "";
                mModel.HSNMaster_Unit = mUnit != null ? mUnit.Code.ToString() : "";
                mModel.UnitName = mUnit != null ? mUnit.Name : "";
                mModel.HSNMaster_IGSTIn = mIGSTIn != null ? mIGSTIn.Code.ToString() : "";
                mModel.IGSTInName = mIGSTIn != null ? mIGSTIn.Name : "";
                mModel.HSNMaster_IGSTOut = mIGSTOut != null ? mIGSTOut.Code.ToString() : "";
                mModel.IGSTOutName = mIGSTOut != null ? mIGSTOut.Name : "";
                mModel.HSNMaster_SGSTIn = mSGSTIn != null ? mSGSTIn.Code.ToString() : "";
                mModel.SGSTInName = mSGSTIn != null ? mSGSTIn.Name : "";
                mModel.HSNMaster_SGSTOut = mSGSTOut != null ? mSGSTOut.Code.ToString() : "";
                mModel.SGSTOutName = mSGSTOut != null ? mSGSTOut.Name : "";
                mModel.HSNMaster_CGSTIn = mCGSTIn != null ? mCGSTIn.Code.ToString() : "";
                mModel.CGSTInName = mCGSTIn != null ? mCGSTIn.Name : "";
                mModel.HSNMaster_CGSTOut = mCGSTOut != null ? mCGSTOut.Code.ToString() : "";
                mModel.CGSTOutName = mCGSTOut != null ? mCGSTOut.Name : "";
                mModel.HSNMaster_CessIn = mCessIn != null ? mCessIn.Code.ToString() : "";
                mModel.CessInName = mCessIn != null ? mCessIn.Name : "";
                mModel.HSNMaster_CessOut = mCessOut != null ? mCessOut.Code.ToString() : "";
                mModel.CessOutName = mCessOut != null ? mCessOut.Name : "";
                mModel.HSNMaster_Code = mList.Code;
                mModel.HSNMaster_Name = mList.Name;
                mModel.HSNMaster_Narr = mList.Narr;

                var mList2 = ctxTFAT.HSNRates.Where(x => x.Code == mModel.HSNMaster_Code).ToList();
                List<HSNMasterVM> mList3 = new List<HSNMasterVM>();
                int n = 1;
                foreach (var eachvalue in mList2)
                {
                    mList3.Add(new HSNMasterVM()
                    {
                        HSNRates_Sno = eachvalue.Sno,
                        HSNRates_EffDate = eachvalue.EffDate,
                        HSNRates_Abatement = eachvalue.Abatement != null ? eachvalue.Abatement.Value : 0,
                        HSNRates_IGSTRate = eachvalue.IGSTRate != null ? eachvalue.IGSTRate.Value : 0,
                        HSNRates_SGSTRate = eachvalue.SGSTRate != null ? eachvalue.SGSTRate.Value : 0,
                        HSNRates_CGSTRate = eachvalue.CGSTRate != null ? eachvalue.CGSTRate.Value : 0,
                        HSNRates_CessRate = eachvalue.CessRate != null ? eachvalue.CessRate.Value : 0,
                        HSNRates_RateLimit = eachvalue.RateLimit != null ? eachvalue.RateLimit.Value : 0,
                        HSNRates_DiscOnTxbl = eachvalue.DiscOnTxbl,
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
                mModel.HSNMaster_CessIn = "";
                mModel.HSNMaster_CessOut = "";
                mModel.HSNMaster_CGSTIn = "";
                mModel.HSNMaster_CGSTOut = "";
                mModel.HSNMaster_Code = "";
                mModel.HSNMaster_Flag = "";
                mModel.HSNMaster_Grp = 0;
                mModel.HSNMaster_IGSTIn = "";
                mModel.HSNMaster_IGSTOut = "";
                mModel.HSNMaster_Name = "";
                mModel.HSNMaster_Narr = "";
                mModel.HSNMaster_SGSTIn = "";
                mModel.HSNMaster_SGSTOut = "";
                mModel.HSNMaster_Unit = "";
            }
            return View(mModel);
        }

        #region GridOperations
        // following action is used when row is added to grid in add mode
        [HttpPost]
        public ActionResult AddToTable(HSNMasterVM Model)
        {
            List<HSNMasterVM> objgriddetail = new List<HSNMasterVM>();
            if (Session["GridDataSession"] != null)
            {
                objgriddetail = (List<HSNMasterVM>)Session["GridDataSession"];
            }
            objgriddetail.Add(new HSNMasterVM()
            {
                HSNRates_Sno = Model.HSNRates_Sno,
                HSNRates_EffDate = Model.HSNRates_EffDate,
                HSNRates_Abatement = Model.HSNRates_Abatement,
                HSNRates_IGSTRate = Model.HSNRates_IGSTRate,
                HSNRates_SGSTRate = Model.HSNRates_SGSTRate,
                HSNRates_CGSTRate = Model.HSNRates_CGSTRate,
                HSNRates_CessRate = Model.HSNRates_CessRate,
                HSNRates_RateLimit = Model.HSNRates_RateLimit,
                HSNRates_DiscOnTxbl = Model.HSNRates_DiscOnTxbl,
                tEmpID = objgriddetail.Count + 1,
                tempIsDeleted = false
            });
            Session.Add("GridDataSession", objgriddetail);
            var html = ViewHelper.RenderPartialView(this, "GridDataView", new HSNMasterVM() { GridDataVM = objgriddetail, Mode = "Add" });
            return Json(new
            {
                GridDataVM = objgriddetail,
                Html = html,
                Mode = "Add"
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PickFromTable(HSNMasterVM Model)
        {
            var result = (List<HSNMasterVM>)Session["GridDataSession"];
            var result1 = result.Where(x => x.tEmpID == Model.tEmpID);
            foreach (var item in result1)
            {
                Model.HSNRates_Sno = item.HSNRates_Sno;
                Model.HSNRates_EffDate = item.HSNRates_EffDate;
                Model.HSNRates_Abatement = item.HSNRates_Abatement;
                Model.HSNRates_IGSTRate = item.HSNRates_IGSTRate;
                Model.HSNRates_SGSTRate = item.HSNRates_SGSTRate;
                Model.HSNRates_CGSTRate = item.HSNRates_CGSTRate;
                Model.HSNRates_CessRate = item.HSNRates_CessRate;
                Model.HSNRates_RateLimit = item.HSNRates_RateLimit;
                Model.HSNRates_DiscOnTxbl = item.HSNRates_DiscOnTxbl;
                Model.tEmpID = item.tEmpID;
                Model.GridDataVM = result;
            }
            return Json(new
            {
                Html = this.RenderPartialView("GridDataView", Model)
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddToTableEdit(HSNMasterVM Model)
        {
            var result = (List<HSNMasterVM>)Session["GridDataSession"];
            foreach (var item in result.Where(x => x.tEmpID == Model.tEmpID))
            {
                item.HSNRates_Sno = Model.HSNRates_Sno;
                item.HSNRates_EffDate = Model.HSNRates_EffDate;
                item.HSNRates_Abatement = Model.HSNRates_Abatement;
                item.HSNRates_IGSTRate = Model.HSNRates_IGSTRate;
                item.HSNRates_SGSTRate = Model.HSNRates_SGSTRate;
                item.HSNRates_CGSTRate = Model.HSNRates_CGSTRate;
                item.HSNRates_CessRate = Model.HSNRates_CessRate;
                item.HSNRates_RateLimit = Model.HSNRates_RateLimit;
                item.HSNRates_DiscOnTxbl = Model.HSNRates_DiscOnTxbl;
                item.tEmpID = Model.tEmpID;
                item.tempIsDeleted = false;
            }
            Session.Add("GridDataSession", result);
            var html = ViewHelper.RenderPartialView(this, "GridDataView", new HSNMasterVM() { GridDataVM = result, Mode = "Add" });
            return Json(new
            {
                GridDataVM = result,
                Html = html,
                Mode = "Add"
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteTableRow(int tEmpID, HSNMasterVM Model)
        {
            var result = (List<HSNMasterVM>)Session["GridDataSession"];
            result.Where(x => x.tEmpID == tEmpID).FirstOrDefault().tempIsDeleted = true;
            Session.Add("GridDataSession", result);
            var html = ViewHelper.RenderPartialView(this, "GridDataView", new HSNMasterVM() { GridDataVM = result });
            return Json(new { SelectedIndent = result, Html = html }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region SaveData
        public ActionResult SaveData(HSNMasterVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {


                    if (mModel.Mode == "Add")
                    {
                        var item = ctxTFAT.HSNMaster.Where(x => x.Code == mModel.HSNMaster_Code).Select(x => x).FirstOrDefault();
                        if (item != null)
                        {
                            return Json(new
                            {
                                Message = "HSN Master Code is Duplicate Cant Save..",
                                Status = "Error"
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteHSNMaster(mModel);
                        if (mModel.Mode == "Delete")
                        {
                            transaction.Commit();
                            transaction.Dispose();
                            return Json(new
                            {
                                Status = "Success",
                                Message = "Data is Deleted."
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    HSNMaster mobj = new HSNMaster();
                    if (mModel.Mode == "Edit")
                    {
                        mobj = ctxTFAT.HSNMaster.Where(x => (x.Code == mModel.HSNMaster_Code)).FirstOrDefault();
                    }
                    mobj.Name = mModel.HSNMaster_Name;
                    mobj.Grp = mModel.HSNMaster_Grp;
                    mobj.Unit = mModel.HSNMaster_Unit;
                    mobj.IGSTIn = mModel.HSNMaster_IGSTIn;
                    mobj.IGSTOut = mModel.HSNMaster_IGSTOut;
                    mobj.SGSTIn = mModel.HSNMaster_SGSTIn;
                    mobj.SGSTOut = mModel.HSNMaster_SGSTOut;
                    mobj.CGSTIn = mModel.HSNMaster_CGSTIn;
                    mobj.CGSTOut = mModel.HSNMaster_CGSTOut;
                    mobj.CessIn = mModel.HSNMaster_CessIn;
                    mobj.CessOut = mModel.HSNMaster_CessOut;
                    mobj.Narr = mModel.HSNMaster_Narr;
                    // iX9: default values for the fields not used @Form
                    mobj.Flag = "";
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;

                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mModel.Mode == "Add")
                    {
                        //mobj.Code = GetNewCode();
                        mobj.Code = mModel.HSNMaster_Code;
                        mobj.ENTEREDBY = muserid;
                        ctxTFAT.HSNMaster.Add(mobj);
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
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.Code, DateTime.Now, 0, "", "Save HSN Master", "NA");

                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "HSNMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "HSNMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                }
            }
            return Json(new { Status = "Success", id = "HSNMaster" }, JsonRequestBehavior.AllowGet);
        }

        public void SaveGridData(HSNMasterVM mModel)
        {
            // delete the existing data from the table
            var mList = ctxTFAT.HSNRates.Where(x => x.Code == mModel.HSNMaster_Code).ToList();
            if (mList.Count != 0)
            {
                ctxTFAT.HSNRates.RemoveRange(mList);
                ctxTFAT.SaveChanges();
            }
            var mList2 = (List<HSNMasterVM>)Session["GridDataSession"];
            if (mList2 != null)
            {
                var mList3 = ((List<HSNMasterVM>)Session["GridDataSession"]).Where(x => x.tempIsDeleted == false);
                foreach (var eachvalue in mList3)
                {
                    HSNRates mgriddata = new HSNRates();
                    mgriddata.Code = mModel.HSNMaster_Code;
                    mgriddata.EffDate = eachvalue.HSNRates_EffDate;
                    mgriddata.Abatement = eachvalue.HSNRates_Abatement;
                    mgriddata.IGSTRate = eachvalue.HSNRates_IGSTRate;
                    mgriddata.SGSTRate = eachvalue.HSNRates_SGSTRate;
                    mgriddata.CGSTRate = eachvalue.HSNRates_CGSTRate;
                    mgriddata.CessRate = eachvalue.HSNRates_CessRate;
                    mgriddata.RateLimit = eachvalue.HSNRates_RateLimit;
                    mgriddata.DiscOnTxbl = eachvalue.HSNRates_DiscOnTxbl;
                    mgriddata.Sno = eachvalue.tEmpID;
                    mgriddata.ENTEREDBY = muserid;
                    mgriddata.LASTUPDATEDATE = DateTime.Now;
                    mgriddata.AUTHORISE = mauthorise;
                    mgriddata.AUTHIDS = muserid;
                    ctxTFAT.HSNRates.Add(mgriddata);
                    ctxTFAT.SaveChanges();
                }
            }
            Session["GridDataSession"] = null;
        }

        public ActionResult DeleteHSNMaster(HSNMasterVM mModel)
        {
            if (mModel.HSNMaster_Code == null || mModel.HSNMaster_Code == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var mList = ctxTFAT.HSNMaster.Where(x => (x.Code == mModel.HSNMaster_Code)).FirstOrDefault();
            ctxTFAT.HSNMaster.Remove(mList);
            var mList2 = ctxTFAT.HSNRates.Where(x => x.Code == mModel.HSNMaster_Code).ToList();
            ctxTFAT.HSNRates.RemoveRange(mList2);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}