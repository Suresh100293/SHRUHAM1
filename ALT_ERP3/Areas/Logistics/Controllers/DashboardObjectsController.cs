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
    public class DashboardObjectsController : BaseController
    {
         
        //private IBusinessCommon mBuss = new BusinessCommon();
        private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mauthorise = "A00";
        private static string mdocument = "";

        #region GetLists
        public List<SelectListItem> GetObjectTypeList()
        {
            List<SelectListItem> CallObjectTypeList = new List<SelectListItem>();
            CallObjectTypeList.Add(new SelectListItem { Value = "Report", Text = "Report" });
            CallObjectTypeList.Add(new SelectListItem { Value = "Column", Text = "Column" });
            CallObjectTypeList.Add(new SelectListItem { Value = "Bar", Text = "Bar" });
            CallObjectTypeList.Add(new SelectListItem { Value = "Pie", Text = "Pie" });
            CallObjectTypeList.Add(new SelectListItem { Value = "Candlestick", Text = "Candlestick" });
            CallObjectTypeList.Add(new SelectListItem { Value = "Bubble", Text = "Bubble" });
            CallObjectTypeList.Add(new SelectListItem { Value = "DoughNut", Text = "DoughNut" });
            CallObjectTypeList.Add(new SelectListItem { Value = "StackedBar100", Text = "StackedBar100" });
            CallObjectTypeList.Add(new SelectListItem { Value = "BoxPlot", Text = "BoxPlot" });
            CallObjectTypeList.Add(new SelectListItem { Value = "Pyramid", Text = "Pyramid" });
            CallObjectTypeList.Add(new SelectListItem { Value = "Polar", Text = "Polar" });
            CallObjectTypeList.Add(new SelectListItem { Value = "Radar", Text = "Radar" });
            CallObjectTypeList.Add(new SelectListItem { Value = "RangeBar", Text = "RangeBar" });
            CallObjectTypeList.Add(new SelectListItem { Value = "Funnel", Text = "Funnel" });
            return CallObjectTypeList;
        }
        public List<SelectListItem> GetModulesList()
        {
            List<SelectListItem> CallModulesList = new List<SelectListItem>();
            CallModulesList.Add(new SelectListItem { Value = "Admin", Text = "Admin" });
            CallModulesList.Add(new SelectListItem { Value = "Finance", Text = "Finance" });
            CallModulesList.Add(new SelectListItem { Value = "Purchase", Text = "Purchase" });
            CallModulesList.Add(new SelectListItem { Value = "Sales", Text = "Sales" });
            CallModulesList.Add(new SelectListItem { Value = "Inventory", Text = "Inventory" });
            CallModulesList.Add(new SelectListItem { Value = "Mfg", Text = "Mfg" });
            CallModulesList.Add(new SelectListItem { Value = "HR", Text = "HR" });
            CallModulesList.Add(new SelectListItem { Value = "AfterSales", Text = "AfterSales" });
            CallModulesList.Add(new SelectListItem { Value = "CRM", Text = "CRM" });
            CallModulesList.Add(new SelectListItem { Value = "Projects", Text = "Projects" });
            CallModulesList.Add(new SelectListItem { Value = "Plant", Text = "Plant" });
            return CallModulesList;
        }
        public List<SelectListItem> GetSizeTypeList()
        {
            List<SelectListItem> CallSizeTypeList = new List<SelectListItem>();
            CallSizeTypeList.Add(new SelectListItem { Value = "0", Text = "0-Mini" });
            CallSizeTypeList.Add(new SelectListItem { Value = "1", Text = "1-Full Width" });
            CallSizeTypeList.Add(new SelectListItem { Value = "2", Text = "2-Half Width" });
            return CallSizeTypeList;
        }
        #endregion GetLists

        // GET: Logistics/DashboardObjects
        public ActionResult Index(DashboardObjectsVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, "Dashboard Objects", "", DateTime.Now, 0, "", "", "A");
            mdocument = mModel.Document;
            mModel.ObjectTypeList = GetObjectTypeList();
            mModel.ModulesList = GetModulesList();
            mModel.SizeTypeList = GetSizeTypeList();

            List<SelectListItem> UsersList = new List<SelectListItem>();
            var UsersResultX = ctxTFAT.TfatPass.Select(x => new { Code = x.Code, Name = x.Name }).ToList().Distinct();
            foreach (var Usersitem in UsersResultX)
            {
                UsersList.Add(new SelectListItem { Text = Usersitem.Name, Value = Usersitem.Code.ToString() });
            }
            mModel.UsersMultiX = UsersList;

            mModel.ActiveObjects_Code = mModel.Document;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.ActiveObjects.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    mModel.ActiveObjects_Code = mList.Code;
                    mModel.ActiveObjects_Name = mList.Name;
                    mModel.ActiveObjects_ObjectType = mList.ObjectType;
                    mModel.ActiveObjects_Modules = mList.Modules;
                    mModel.ActiveObjects_Height = mList.Height != null ? (double)mList.Height : 0;
                    mModel.ActiveObjects_Width = mList.Width != null ? (double)mList.Width : 0;
                    mModel.ActiveObjects_SizeType = mList.SizeType;
                    mModel.ActiveObjects_Query = mList.Query;
                    mModel.ActiveObjects_Users = mList.Users;
                    mModel.ActiveObjects_ReportCode = mList.ReportCode;
                    mModel.ActiveObjects_Status = mList.Status;
                }
            }
            else
            {
                mModel.ActiveObjects_Code = "";
                mModel.ActiveObjects_Height = 0;
                mModel.ActiveObjects_Modules = "";
                mModel.ActiveObjects_Name = "";
                mModel.ActiveObjects_ObjectType = "";
                mModel.ActiveObjects_Query = "";
                mModel.ActiveObjects_ReportCode = "";
                mModel.ActiveObjects_SizeType = "";
                mModel.ActiveObjects_Status = false;
                mModel.ActiveObjects_Users = "";
                mModel.ActiveObjects_Width = 0;
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(DashboardObjectsVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteDashboardObjects(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    ActiveObjects mobj = new ActiveObjects();
                    bool mAdd = true;
                    if (ctxTFAT.ActiveObjects.Where(x => (x.Code == mModel.ActiveObjects_Code)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.ActiveObjects.Where(x => (x.Code == mModel.ActiveObjects_Code)).FirstOrDefault();
                        mAdd = false;
                    }
                    mobj.Code = mModel.ActiveObjects_Code;
                    mobj.Name = mModel.ActiveObjects_Name;
                    mobj.ObjectType = mModel.ActiveObjects_ObjectType;
                    mobj.Modules = mModel.ActiveObjects_Modules;
                    mobj.Height = 0;// (Double)mModel.ActiveObjects_Height;
                    mobj.Width = 0;// (Double)mModel.ActiveObjects_Width;
                    mobj.SizeType = mModel.ActiveObjects_SizeType;
                    mobj.Query = mModel.ActiveObjects_Query;
                    mobj.Users = mModel.ActiveObjects_Users;
                    mobj.ReportCode = mModel.ActiveObjects_ReportCode;
                    mobj.Status = mModel.ActiveObjects_Status;
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mAdd == true)
                    {
                        ctxTFAT.ActiveObjects.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    string mNewCode = "";
                    mNewCode = mobj.Code;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, "Dashboard Objects", "", DateTime.Now, 0, mNewCode, "", "A");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "DashboardObjects" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "DashboardObjects" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "DashboardObjects" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "DashboardObjects" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteDashboardObjects(DashboardObjectsVM mModel)
        {
            if (mModel.ActiveObjects_Code == null || mModel.ActiveObjects_Code == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var mList = ctxTFAT.ActiveObjects.Where(x => (x.Code == mModel.ActiveObjects_Code)).FirstOrDefault();
            ctxTFAT.ActiveObjects.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}