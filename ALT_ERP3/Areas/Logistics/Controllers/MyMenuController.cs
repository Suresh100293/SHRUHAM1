using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class MyMenuController : BaseController
    {

        ////private IBusinessCommon mBuss = new BusinessCommon();
        private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mauthorise = "A00";
        private static int mdocument = 0;

        #region GetLists
        public List<SelectListItem> GetModuleNameList()
        {
            List<SelectListItem> CallModuleNameList = new List<SelectListItem>();
            CallModuleNameList.Add(new SelectListItem { Value = "SetUP", Text = "SetUP" });
            CallModuleNameList.Add(new SelectListItem { Value = "Master", Text = "Master" });
            CallModuleNameList.Add(new SelectListItem { Value = "Transactions", Text = "Transactions" });
            CallModuleNameList.Add(new SelectListItem { Value = "Reports", Text = "Reports" });
            CallModuleNameList.Add(new SelectListItem { Value = "ControlPanel", Text = "ControlPanel" });

            return CallModuleNameList;
        }
        public List<SelectListItem> GetOptionTypeList()
        {
            List<SelectListItem> CallOptionTypeList = new List<SelectListItem>();
            CallOptionTypeList.Add(new SelectListItem { Value = "M", Text = "M-Master" });
            CallOptionTypeList.Add(new SelectListItem { Value = "R", Text = "R-Report" });
            CallOptionTypeList.Add(new SelectListItem { Value = "T", Text = "T-Transactions" });
            CallOptionTypeList.Add(new SelectListItem { Value = "I", Text = "I-Interface" });
            CallOptionTypeList.Add(new SelectListItem { Value = "X", Text = "X-UserForm" });
            return CallOptionTypeList;
        }
        public List<SelectListItem> GetParentMenuList()
        {
            List<SelectListItem> CallParentMenuList = new List<SelectListItem>();
            CallParentMenuList.Add(new SelectListItem { Value = "Accounts", Text = "Accounts" });
            CallParentMenuList.Add(new SelectListItem { Value = "Logistics", Text = "Logistics" });
            CallParentMenuList.Add(new SelectListItem { Value = "Vehicles", Text = "Vehicles" });
            return CallParentMenuList;
        }
        #endregion GetLists

        // GET: Logistics/MyMenu
        public ActionResult Index(MyMenuVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;
            mModel.ModuleNameList = GetModuleNameList();
            mModel.OptionTypeList = GetOptionTypeList();
            mModel.ParentMenuList = GetParentMenuList();
            mModel.TfatMenu_ID = mModel.Document;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.TfatMenu.Where(x => (x.ID == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    mModel.TfatMenu_ID = mList.ID;
                    mModel.TfatMenu_ModuleName = mList.ModuleName;
                    mModel.TfatMenu_OptionType = mList.OptionType;
                    mModel.TfatMenu_OptionCode = mList.OptionCode;
                    mModel.TfatMenu_Menu = mList.Menu;
                    mModel.TfatMenu_ParentMenu = mList.ParentMenu;
                    mModel.TfatMenu_FormatCode = mList.FormatCode;
                    mModel.TfatMenu_AllowClick = mList.AllowClick;
                    mModel.TfatMenu_Hide = mList.Hide;
                    mModel.TfatMenu_TableName = mList.TableName;
                    mModel.TfatMenu_Controller = mList.Controller;
                    mModel.TfatMenu_DisplayOrder = mList.DisplayOrder;
                    mModel.TfatMenu_Level = mList.Level;
                    mModel.TfatMenu_QuickMenu = mList.QuickMenu;
                    mModel.TfatMenu_QuickMaster = mList.QuickMaster;
                    mModel.TfatMenu_MainType = mList.MainType;
                    mModel.TfatMenu_SubType = mList.SubType;
                    mModel.TfatMenu_ZoomURL = mList.ZoomURL;
                    mModel.TfatMenu_SystemDefault = mList.SystemDefault;
                }
            }
            else
            {
                mModel.TfatMenu_AllowClick = false;
                mModel.TfatMenu_AutoGenerate = false;
                mModel.TfatMenu_Controller = "ReportCentrePlus";
                mModel.TfatMenu_DisplayOrder = ctxTFAT.TfatMenu.Max(x => x.DisplayOrder) + 1;
                if (mModel.TfatMenu_DisplayOrder < 10000) mModel.TfatMenu_DisplayOrder = 10000;
                mModel.TfatMenu_FormatCode = "";
                mModel.TfatMenu_Hide = false;
                mModel.TfatMenu_ID = 0;
                mModel.TfatMenu_IsDone = false;
                mModel.TfatMenu_Level = 0;
                mModel.TfatMenu_MainType = "";
                mModel.TfatMenu_Menu = "";
                mModel.TfatMenu_ModuleName = "";
                mModel.TfatMenu_OptionCode = "";
                mModel.TfatMenu_OptionType = "R";
                mModel.TfatMenu_ParentMenu = "Reports";
                mModel.TfatMenu_QuickMaster = false;
                mModel.TfatMenu_QuickMenu = false;
                mModel.TfatMenu_SubType = "";
                mModel.TfatMenu_TableName = "";
                mModel.TfatMenu_ZoomURL = "";
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(MyMenuVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        var JsonRetureFromDelete=DeleteMyMenu(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return JsonRetureFromDelete;
                    }
                    TfatMenu mobj = new TfatMenu();
                    bool mAdd = true;
                    if (ctxTFAT.TfatMenu.Where(x => (x.ID == mModel.TfatMenu_ID)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.TfatMenu.Where(x => (x.ID == mModel.TfatMenu_ID)).FirstOrDefault();
                        mAdd = false;
                    }
                    mobj.ID = mModel.TfatMenu_ID;
                    mobj.ModuleName = mModel.TfatMenu_ModuleName;
                    mobj.OptionType = mModel.TfatMenu_OptionType;
                    mobj.OptionCode = mModel.TfatMenu_OptionCode;
                    mobj.Menu = mModel.TfatMenu_Menu;
                    mobj.ParentMenu = mModel.TfatMenu_ParentMenu;
                    mobj.FormatCode = mModel.TfatMenu_FormatCode == null ? "" : mModel.TfatMenu_FormatCode;
                    mobj.AllowClick = mModel.TfatMenu_AllowClick;
                    mobj.Hide = mModel.TfatMenu_Hide;
                    mobj.TableName = mModel.TfatMenu_TableName == null ? "" : mModel.TfatMenu_TableName;
                    mobj.Controller = mModel.TfatMenu_Controller == null || mModel.TfatMenu_Controller == "" ? "ReportCentrePlus" : mModel.TfatMenu_Controller;
                    mobj.DisplayOrder = mModel.TfatMenu_DisplayOrder;
                    mobj.Level = mModel.TfatMenu_Level;
                    mobj.QuickMenu = mModel.TfatMenu_QuickMenu;
                    mobj.QuickMaster = mModel.TfatMenu_QuickMaster;
                    mobj.MainType = mModel.TfatMenu_MainType == null ? "" : mModel.TfatMenu_MainType;
                    mobj.SubType = mModel.TfatMenu_SubType == null ? "" : mModel.TfatMenu_SubType;
                    mobj.ZoomURL = mModel.TfatMenu_ZoomURL == null ? "" : mModel.TfatMenu_ZoomURL;
                    mobj.SystemDefault = mModel.TfatMenu_SystemDefault;
                    // iX9: default values for the fields not used @Form
                    mobj.AutoGenerate = false;
                    mobj.IsDone = false;
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mAdd == true)
                    {
                        mobj.ID = GetNextCode();
                    }
                    if (mAdd == true)
                    {
                        ctxTFAT.TfatMenu.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    int mNewCode = 0;
                    mNewCode = mobj.ID;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mNewCode.ToString(), DateTime.Now, 0, "", "Save My Menu", "NA");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "MyMenu" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "MyMenu" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "MyMenu" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "MyMenu" }, JsonRequestBehavior.AllowGet);
        }

        public int GetNextCode()
        {
            var nextcode = (from x in ctxTFAT.TfatMenu select (int?)x.ID).Max() ?? 100000;
            if (nextcode < 100000)
                nextcode = 100000;
            return (++nextcode);
        }

        public ActionResult DeleteMyMenu(MyMenuVM mModel)
        {
            // iX9: Check for Active Master TfatMenu
            string mactivestring = "";
            var mactive1 = ctxTFAT.UserRights.Where(x => (x.MenuID == mModel.TfatMenu_ID)).Select(x => x.Code).FirstOrDefault();
            if (String.IsNullOrEmpty(mactive1) == false) { mactivestring = mactivestring + "\nUserRights: " + mactive1; }
            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "Active Item, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var mList = ctxTFAT.TfatMenu.Where(x => (x.ID == mModel.TfatMenu_ID)).FirstOrDefault();
            ctxTFAT.TfatMenu.Remove(mList);
            var ReportHeader = ctxTFAT.ReportHeader.Where(x => x.Code == mList.FormatCode).FirstOrDefault();
            if (ReportHeader!=null)
            {
                ctxTFAT.ReportHeader.Remove(ReportHeader);
            }
            var tfatsearch = ctxTFAT.TfatSearch.Where(x => x.Code == mList.FormatCode).ToList();
            ctxTFAT.TfatSearch.RemoveRange(tfatsearch);

            ctxTFAT.SaveChanges();
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.TfatMenu_ID.ToString(), DateTime.Now, 0, "", "Delete My Menu", "NA");

            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}