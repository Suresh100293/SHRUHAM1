using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class TransactionUserRightsController : BaseController
    {
        //private //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        //private IBusinessCommon mBuss = new BusinessCommon();
        private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mAUTHORISE = "A00";
        private static int mdocument = 0;

        #region GetLists
        public ActionResult GetSearchList()
        {
            List<SelectListItem> CallItemOptionList = new List<SelectListItem>();

            CallItemOptionList.Add(new SelectListItem { Value = "MainType", Text = "MainType" });//2
            CallItemOptionList.Add(new SelectListItem { Value = "SubType", Text = "SubType" });//3
            CallItemOptionList.Add(new SelectListItem { Value = "Code", Text = "Code" });//3
            CallItemOptionList.Add(new SelectListItem { Value = "Name", Text = "Name" });//3
            return Json(CallItemOptionList, JsonRequestBehavior.AllowGet);
        }
        #endregion GetLists

        // GET: Accounts/TransactionUserRights
        public ActionResult Index(TransactionUserRightsVM mModel)
        {
            //GetAllMenu(Session["ModuleName"].ToString());
            ////AccountingGetAllMenu(Session["ModuleName"].ToString());
            ////VehicleGetAllMenu(Session["ModuleName"].ToString());
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, "View", mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            List<TransactionUserRightsVM> mLeftList = new List<TransactionUserRightsVM>();
            var mlist = ctxTFAT.TfatMenu.Where(x => x.Hide == false && String.IsNullOrEmpty(x.ZoomURL) == true && (x.ModuleName == "ControlPanel" || x.ModuleName == "Master" || x.ModuleName == "Reports" || x.ModuleName == "SetUP" || x.ModuleName == "Transactions")).ToList();

            foreach (var i in mlist)
            {
                mLeftList.Add(new TransactionUserRightsVM()
                {
                    DocTypes_Code = i.ID.ToString(),
                    DocTypes_Name = i.Menu,
                    DocTypes_MainType = i.MainType,
                    DocTypes_SubType = i.SubType,
                });
            }
            mModel.mLeftList = mLeftList;
            return View(mModel);
        }
        [HttpPost]
        public ActionResult GetLeftGrid(TransactionUserRightsVM mModel)
        {
            List<TransactionUserRightsVM> mLeftList = new List<TransactionUserRightsVM>();
            List<TfatMenu> mlist = new List<TfatMenu>();

            if (mModel.SearchBy == "Name")
            {
                //mlist = ctxTFAT.DocTypes.Where(x => x.MainType != x.SubType && x.Code != x.SubType && x.Name.ToLower().Contains(mModel.SearchContent)).Select(x => x).OrderBy(x => x.Name).ToList();
                if (String.IsNullOrEmpty(mModel.SearchContent))
                {
                    mlist = ctxTFAT.TfatMenu.Where(x => x.Hide == false && String.IsNullOrEmpty(x.ZoomURL)==true &&( x.ModuleName == "ControlPanel" || x.ModuleName == "Master" || x.ModuleName == "Reports" || x.ModuleName == "SetUP" || x.ModuleName == "Transactions")).ToList();
                }
                else
                {
                    mlist = ctxTFAT.TfatMenu.Where(x => x.Hide == false && String.IsNullOrEmpty(x.ZoomURL) == true  && x.Menu.ToLower().Contains(mModel.SearchContent.ToLower()) && (x.ModuleName == "ControlPanel" || x.ModuleName == "Master" || x.ModuleName == "Reports" || x.ModuleName == "SetUP" || x.ModuleName == "Transactions")).ToList();
                }
                //mModel.mLeftList = mLeftList.Where(x => x.DocTypes_Name.Contains(mModel.SearchContent)).Select(x => x).ToList();
            }
            else
            {
                //mlist = ctxTFAT.DocTypes.Where(x => x.MainType != x.SubType && x.Code != x.SubType && x.Code.ToLower().Contains(mModel.SearchContent)).Select(x => x).OrderBy(x => x.Name).ToList();
                mlist = ctxTFAT.TfatMenu.Where(x => x.Hide == false && String.IsNullOrEmpty(x.ZoomURL) == true  && (x.ModuleName == "ControlPanel" || x.ModuleName == "Master" || x.ModuleName == "Reports" || x.ModuleName == "SetUP" || x.ModuleName == "Transactions")).ToList();
                //mModel.mLeftList = mLeftList.Where(x => x.DocTypes_Code.Contains(mModel.SearchContent)).Select(x => x).ToList();
            }
            foreach (var i in mlist)
            {
                mLeftList.Add(new TransactionUserRightsVM()
                {
                    DocTypes_Code = i.ID.ToString(),
                    DocTypes_Name = i.Menu,
                    DocTypes_MainType = i.MainType,
                    DocTypes_SubType = i.SubType,
                });
            }
            var html = ViewHelper.RenderPartialView(this, "PartialTypeView", new TransactionUserRightsVM { mLeftList = mLeftList });
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ClickLeftGrid(TransactionUserRightsVM mModel)
        {
            var result = (from mrightlink in ctxTFAT.TfatPass
                          where mrightlink.Locked == false
                          join xUserRightsTrx in ctxTFAT.UserRights.Where(x => x.MenuID.ToString() == mModel.DocTypes_Code) on mrightlink.Code equals xUserRightsTrx.Code into ljointresult
                          from finalresult in ljointresult.DefaultIfEmpty()
                          select new
                          {
                              Code = mrightlink.Code,
                              xCess = finalresult != null ? finalresult.xCess : false,
                              xAdd = finalresult != null ? finalresult.xAdd : false,
                              xEdit = finalresult != null ? finalresult.xEdit : false,
                              xDelete = finalresult != null ? finalresult.xDelete : false,
                              xPrint = finalresult != null ? finalresult.xPrint : false,
                              xBackDated = finalresult != null ? finalresult.xBackDated : false,
                              xLimit = finalresult != null ? finalresult.xLimit.Value : 0
                          }).OrderBy(x => x.Code).ToList();

            List<TransactionUserRightsVM> mlist = new List<TransactionUserRightsVM>();
            foreach (var i in result)
            {
                mlist.Add(new TransactionUserRightsVM()
                {
                    UserRightsTrx_Code = i.Code,
                    UserRightsTrx_xCess = i.xCess,
                    UserRightsTrx_xAdd = i.xAdd,
                    UserRightsTrx_xEdit = i.xEdit,
                    UserRightsTrx_xDelete = i.xDelete,
                    UserRightsTrx_xPrint = i.xPrint,
                    UserRightsTrx_xBackDated = i.xBackDated,
                    UserRightsTrx_xLimit = i.xLimit
                });
            }
            mModel.DocTypes_Code = ctxTFAT.TfatMenu.Where(x => x.ID.ToString() == mModel.DocTypes_Code).Select(x => x.Menu).FirstOrDefault();
            var html = ViewHelper.RenderPartialView(this, "TransactionUserRightsPartial", new TransactionUserRightsVM { mRightList = mlist, UserRightsTrx_Type = mModel.DocTypes_Code });
            return Json(new { mRightList = mlist, UserRightsTrx_Type = mModel.DocTypes_Code, Html = html }, JsonRequestBehavior.AllowGet);
        }
        #region SaveData
        public ActionResult SaveData(TransactionUserRightsVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var mobjstk1 = ctxTFAT.UserRightsTrx.Where(x => x.Type == mModel.UserRightsTrx_Type).ToList();
                    ctxTFAT.UserRightsTrx.RemoveRange(mobjstk1);
                    ctxTFAT.SaveChanges();
                    foreach (var eachvalue in mModel.mRightList)
                    {
                        UserRightsTrx mobj = new UserRightsTrx();
                        mobj.Type = mModel.UserRightsTrx_Type;
                        mobj.Code = eachvalue.UserRightsTrx_Code;
                        mobj.xCess = eachvalue.UserRightsTrx_xCess;
                        mobj.xAdd = eachvalue.UserRightsTrx_xAdd;
                        mobj.xEdit = eachvalue.UserRightsTrx_xEdit;
                        mobj.xDelete = eachvalue.UserRightsTrx_xDelete;
                        mobj.xPrint = eachvalue.UserRightsTrx_xPrint;
                        mobj.xBackDated = eachvalue.UserRightsTrx_xBackDated;
                        mobj.xLimit = eachvalue.UserRightsTrx_xLimit;
                        mobj.AUTHIDS = muserid;
                        mobj.AUTHORISE = mAUTHORISE;
                        mobj.CompCode = mcompcode;
                        mobj.ENTEREDBY = muserid;
                        mobj.LASTUPDATEDATE = DateTime.Now;
                        ctxTFAT.UserRightsTrx.Add(mobj);
                        ctxTFAT.SaveChanges();
                    }
                    transaction.Commit();
                    transaction.Dispose();
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
            return Json(new { Status = "Success", id = "TransactionUserRights" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}