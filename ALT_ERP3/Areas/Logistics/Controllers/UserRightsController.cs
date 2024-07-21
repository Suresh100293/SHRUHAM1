using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class UserRightsController : BaseController
    {
        ////private IBusinessCommon mBuss = new BusinessCommon();
        private int mlocation = 100001;
        //private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        //private string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        //private string muserid = System.Web.HttpContext.Current.Session["UserId"].ToString();
        //private string mperiod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        private int mnewrecordkey = 0;
        private static string mauthorise = "A00";
        private static string mmaintype = "";
        private static int mdocument = 0;

        
        #region GetLists
        public ActionResult GetPartyList(string term)
        {
            if (term == "")
            {
                var result = ctxTFAT.TfatPass.Where(x =>  x.Code != "Super" && x.Locked==false).Select(m => new
                {
                    m.Code,
                    m.Name
                }).OrderBy(n => n.Name).Take(10).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.TfatPass.Where(x =>  x.Name.Contains(term) && x.Code != "Super" && x.Locked == false).Select(m => new
                {
                    m.Code,
                    m.Name
                }).OrderBy(n => n.Name).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetModuleList(string term)
        {
            List<SelectListItem> ModulList = new List<SelectListItem>();
            ModulList.Add(new SelectListItem { Value = "ALL", Text = "ALL" });
            ModulList.Add(new SelectListItem { Value = "SetUP", Text = "SetUP" });
            ModulList.Add(new SelectListItem { Value = "Master", Text = "Master" });
            ModulList.Add(new SelectListItem { Value = "Transactions", Text = "Transactions" });
            ModulList.Add(new SelectListItem { Value = "Reports", Text = "Reports" });
            ModulList.Add(new SelectListItem { Value = "ControlPanel", Text = "ControlPanel" });

            if (!String.IsNullOrEmpty(term))
            {
                ModulList = ModulList.Where(x => term.Contains(x.Text)).ToList();
            }

            var Modified = ModulList.Select(x => new
            {
                Code = x.Value,
                Name = x.Text
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }
        #endregion GetLists

        // GET: Logistics/UserRights
        public ActionResult Index(UserRightsVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, "", "User Rights", "", DateTime.Now, 0, "", "","U");
            mmaintype = mModel.MainType;
            List<UserRightsVM> mLeftList = new List<UserRightsVM>();
            var mlist = ctxTFAT.TfatPass.Select(x => x).OrderBy(x => x.Name).ToList();
            foreach (var i in mlist.Where(x=>x.Locked==false))
            {
                mLeftList.Add(new UserRightsVM()
                {
                    TfatPass_Code = i.Code,
                    TfatPass_Name = i.Name,
                });
            }
            mModel.mLeftList = mLeftList;
            mModel.EnumModuleName = "ALL";
            return View(mModel);
        }

        [HttpPost]
        public ActionResult ClickLeftGrid(UserRightsVM mModel)
        {
            
            var result = (from mrightlink in ctxTFAT.TfatMenu.Where(x=>x.Hide==false && x.ModuleName== "ControlPanel" ||x.ModuleName== "Master" || x.ModuleName== "Reports" || x.ModuleName== "SetUP" || x.ModuleName== "Transactions")
                          
                          join xUserRights in ctxTFAT.UserRights.Where(x => x.Code == mModel.TfatPass_Code  /*&& x.ModuleName == mmaintype*/) on mrightlink.ID equals xUserRights.MenuID into ljointresult
                          from finalresult in ljointresult.DefaultIfEmpty()
                          orderby mrightlink.ModuleName,mrightlink.ParentMenu,mrightlink.DisplayOrder
                          select new
                          {
                              MenuID = mrightlink.ID,
                              Menu = mrightlink.Menu,
                              xCess = finalresult != null ? finalresult.xCess : false,
                              xAdd = finalresult != null ? finalresult.xAdd : false,
                              xEdit = finalresult != null ? finalresult.xEdit : false,
                              xDelete = finalresult != null ? finalresult.xDelete : false,
                              xPrint = finalresult != null ? finalresult.xPrint : false,
                              xBackDated = finalresult != null ? finalresult.xBackDated : false,
                              xLimit = finalresult != null ? finalresult.xLimit.Value : 0,
                              ModuleName = mrightlink != null ? mrightlink.ModuleName : "",
                              ParentName = mrightlink != null ? mrightlink.ParentMenu : "",
                              ZoomUrl = mrightlink != null ? mrightlink.ZoomURL : ""
                          }).ToList();

            List<UserRightsVM> mlist = new List<UserRightsVM>();
            foreach (var i in result)
            {
                mlist.Add(new UserRightsVM()
                {
                    UserRights_MenuID = i.MenuID,
                    TfatMenu_Menu = i.Menu,
                    UserRights_xCess = i.xCess,
                    UserRights_xAdd = i.xAdd,
                    UserRights_xEdit = i.xEdit,
                    UserRights_xDelete = i.xDelete,
                    UserRights_xPrint = i.xPrint,
                    UserRights_xBackDated = i.xBackDated,
                    UserRights_xLimit = i.xLimit,
                    Modulename = i.ModuleName,
                    ParentName = i.ParentName,
                    ZoomUrl = i.ZoomUrl
                });
            }
            var html = ViewHelper.RenderPartialView(this, "UserRightsPartial", new UserRightsVM { mRightList = mlist, UserRights_Code = mModel.TfatPass_Code });
            return Json(new {  UserRights_Code = mModel.TfatPass_Code, Html = html }, JsonRequestBehavior.AllowGet);
        }

        #region SaveData
        public ActionResult SaveData(UserRightsVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var mobjstk1 = ctxTFAT.UserRights.Where(x => x.Code == mModel.UserRights_Code ).ToList();
                    ctxTFAT.UserRights.RemoveRange(mobjstk1);
                    ctxTFAT.SaveChanges();
                    foreach (var eachvalue in mModel.mRightList)
                    {
                        UserRights mobj = new UserRights();
                        mobj.ModuleName = eachvalue.Modulename;
                        //mobj.ParentMenu = eachvalue.ParentName;
                        mobj.Code = mModel.UserRights_Code;
                        mobj.MenuID = eachvalue.UserRights_MenuID;
                        mobj.xCess = eachvalue.UserRights_xCess;
                        mobj.xAdd = eachvalue.UserRights_xAdd;
                        mobj.xEdit = eachvalue.UserRights_xEdit;
                        mobj.xDelete = eachvalue.UserRights_xDelete;
                        mobj.xPrint = eachvalue.UserRights_xPrint;
                        mobj.xBackDated = eachvalue.UserRights_xBackDated;
                        mobj.xLimit = eachvalue.UserRights_xLimit;
                        //mobj.ZoomURL = eachvalue.ZoomUrl;
                        mobj.Branch = mbranchcode;
                        mobj.AUTHIDS = muserid;
                        mobj.AUTHORISE = mauthorise;
                        mobj.CompCode = mcompcode;
                        mobj.ENTEREDBY = muserid;
                        mobj.LASTUPDATEDATE = DateTime.Now;
                        ctxTFAT.UserRights.Add(mobj);
                        ctxTFAT.SaveChanges();
                    }
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.UserRights_Code, DateTime.Now, 0, mModel.UserRights_Code, "Save User Rights", "U");

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
            return Json(new { Status = "Success", id = "UserRights" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveCopyRights(UserRightsVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var mobjstk1 = ctxTFAT.UserRights.Where(x => x.Code == mModel.UserRights_Code ).ToList();
                    ctxTFAT.UserRights.RemoveRange(mobjstk1);


                    var uslist = ctxTFAT.UserRights.Where(x => x.Code == mModel.Account ).ToList();
                    foreach (var eachvalue in uslist)
                    {
                        UserRights mobj = new UserRights();
                        mobj.ModuleName = mmaintype;
                        mobj.Code = mModel.UserRights_Code;
                        mobj.MenuID = eachvalue.MenuID;
                        mobj.xCess = eachvalue.xCess;
                        mobj.xAdd = eachvalue.xAdd;
                        mobj.xEdit = eachvalue.xEdit;
                        mobj.xDelete = eachvalue.xDelete;
                        mobj.xPrint = eachvalue.xPrint;
                        mobj.xBackDated = eachvalue.xBackDated;
                        mobj.xLimit = eachvalue.xLimit;
                        mobj.Branch = mbranchcode;
                        mobj.AUTHIDS = muserid;
                        mobj.AUTHORISE = mauthorise;
                        mobj.CompCode = mcompcode;
                        mobj.ENTEREDBY = muserid;
                        mobj.LASTUPDATEDATE = DateTime.Now;
                        ctxTFAT.UserRights.Add(mobj);

                    }
                    ctxTFAT.SaveChanges();
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
            return Json(new { Status = "Success", id = "UserRights" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}