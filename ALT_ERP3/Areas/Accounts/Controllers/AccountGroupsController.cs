using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class AccountGroupsController : BaseController
    {
        //private //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        //private IBusinessCommon mBuss = new BusinessCommon();
        //private int mlocation = 100001;

        private int mnewrecordkey = 0;
        private static string mAUTHORISE = "A00";
        private static string mdocument = "";

        #region GetLists
        public List<SelectListItem> GetBaseGrList()
        {
            List<SelectListItem> CallBaseGrList = new List<SelectListItem>();
            CallBaseGrList.Add(new SelectListItem { Value = "S", Text = "S - Suppliers, Vendors & Creditors" });
            CallBaseGrList.Add(new SelectListItem { Value = "D", Text = "D - Customers / Debtors" });
            CallBaseGrList.Add(new SelectListItem { Value = "B", Text = "B - Bank Accounts" });
            CallBaseGrList.Add(new SelectListItem { Value = "C", Text = "C - Cash Accounts" });
            CallBaseGrList.Add(new SelectListItem { Value = "F", Text = "F - Fixed Assets" });
            CallBaseGrList.Add(new SelectListItem { Value = "H", Text = "H - Inter-Branch Accounts" });
            CallBaseGrList.Add(new SelectListItem { Value = "O", Text = "O - Loans And Advances" });
            CallBaseGrList.Add(new SelectListItem { Value = "P", Text = "P - Capital Accounts" });
            CallBaseGrList.Add(new SelectListItem { Value = "R", Text = "R - Reserves & Surplus" });
            CallBaseGrList.Add(new SelectListItem { Value = "T", Text = "T - Credit Card Accounts" });
            CallBaseGrList.Add(new SelectListItem { Value = "V", Text = "V - Investment Accounts" });
            CallBaseGrList.Add(new SelectListItem { Value = "A", Text = "A - Assets" });
            CallBaseGrList.Add(new SelectListItem { Value = "L", Text = "L - Liabilities" });
            CallBaseGrList.Add(new SelectListItem { Value = "I", Text = "I - Incomes" });
            CallBaseGrList.Add(new SelectListItem { Value = "X", Text = "X - Expenses" });
            return CallBaseGrList;
        }
        public JsonResult AutoCompleteGrp(string term)
        {
            return Json((from m in ctxTFAT.MasterGroups
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteRevGroup(string term)
        {
            return Json((from m in ctxTFAT.MasterGroups
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        #endregion GetLists

        // GET: Accounts/AccountGroups
        public ActionResult Index(AccountGroupsVM mModel)
        {
            //GetAllMenu(Session["ModuleName"].ToString());
            ////AccountingGetAllMenu(Session["ModuleName"].ToString());
            ////VehicleGetAllMenu(Session["ModuleName"].ToString());
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "G");
            mdocument = mModel.Document;
            mModel.BaseGrList = GetBaseGrList();
            return View(mModel);
        }

        public string AccountGroupTreeView()
        {
            var mTreeList = ctxTFAT.MasterGroups.Select(x => new { x.Name, x.GrpKey, x.RECORDKEY, x.Code }).ToList();
            List<FlatObject> flatObjects2 = new List<FlatObject>();
            for (int n = 0; n < mTreeList.Count; n++)
            {
                FlatObject abc = new FlatObject();

                #region Temparary Remove Wrong Master Message Suresh 7.4.2023
                var WrongLedgerCount = "";
                if(1==2)
                {
                    var TempCode = mTreeList[n].Code.ToString();
                    var MasterGroup = ctxTFAT.MasterGroups.Where(x => x.Grp.ToString() != x.Code.ToString() && x.Grp.ToString() == TempCode).FirstOrDefault();
                    if (MasterGroup != null)
                    {
                        var Count = ctxTFAT.Master.Where(x => x.Grp == TempCode).ToList().Count();
                        if (Count > 0)
                        {
                            WrongLedgerCount = " ( "+Count.ToString()+" Wrong Master Group )";
                        }
                    }
                }
                #endregion

                abc.data = mTreeList[n].Name + WrongLedgerCount;
                
                
                
                abc.Id = mTreeList[n].RECORDKEY;
                if (mTreeList[n].RECORDKEY == mTreeList[n].GrpKey)
                {
                    abc.ParentId = 0;
                }
                else
                {
                    abc.ParentId = (int)mTreeList[n].GrpKey;
                }
                flatObjects2.Add(abc);
            }
            var recursiveObjects = FillRecursive(flatObjects2, 0);
            string myjsonmodel = new JavaScriptSerializer().Serialize(recursiveObjects);
            return myjsonmodel;
        }

        public ActionResult TreeNodeClick(AccountGroupsVM mModel)
        {
            var mList = ctxTFAT.MasterGroups.Where(x => (x.RECORDKEY == mModel.MasterGroups_RECORDKEY)).FirstOrDefault();
            if (mList != null)
            {
                mModel.Mode = "Edit";
                var mGrp = ctxTFAT.MasterGroups.Where(x => x.Code == mList.Grp).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mRevGroup = ctxTFAT.MasterGroups.Where(x => x.Code == mList.RevGroup).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                mModel.MasterGroups_Grp = mGrp != null ? mGrp.Code.ToString() : "";
                mModel.GrpName = mGrp != null ? mGrp.Name : "";
                mModel.MasterGroups_RevGroup = mRevGroup != null ? mRevGroup.Code.ToString() : "";
                mModel.RevGroupName = mRevGroup != null ? mRevGroup.Name : "";
                mModel.MasterGroups_Code = mList.Code;
                mModel.MasterGroups_Name = mList.Name;
                mModel.MasterGroups_BaseGr = mList.BaseGr;
                mModel.MasterGroups_Seq = mList.Seq != null ? mList.Seq.Value : 0;
                mModel.MasterGroups_Prefix = mList.Prefix;
                mModel.MasterGroups_Hide = mList.Hide;
                mModel.MasterGroups_DisplayOrder = mList.DisplayOrder != null ? mList.DisplayOrder.Value : 0;
                mModel.MasterGroups_ForceCC = mList.ForceCC;
                mModel.MasterGroups_Sch = mList.Sch;
                mModel.MasterGroups_SystemCode = mList.SystemCode;
            }
            mModel.BaseGrList = GetBaseGrList();
            var html = ViewHelper.RenderPartialView(this, "AccountGroupsView", mModel);
            return Json(new { Html = html, Status = "Success" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddChild(AccountGroupsVM mModel)
        {
            if (mModel.MasterGroups_Code == null || mModel.MasterGroups_Code == "")
            {
                return Json(new { Message = "Item is Not Selected, Can't Add any Child..", Status = "Active" }, JsonRequestBehavior.AllowGet);
            }
            mModel.MasterGroups_AcType = "";
            mModel.MasterGroups_AliasPrefix = "";
            mModel.MasterGroups_BaseGr = "";
            mModel.MasterGroups_Code = "";
            mModel.MasterGroups_DisplayOrder = 0;
            mModel.MasterGroups_ForceCC = false;
            mModel.MasterGroups_Grp = "";

            mModel.MasterGroups_Hide = false;
            mModel.MasterGroups_IsLast = false;
            mModel.MasterGroups_Level = 0;
            mModel.MasterGroups_Name = "";
            mModel.MasterGroups_NoDetails = false;
            mModel.MasterGroups_PCCode = 0;
            mModel.MasterGroups_Prefix = "";
            mModel.MasterGroups_RevGroup = "";
            mModel.MasterGroups_Sch = "";
            mModel.MasterGroups_Seq = 0;
            mModel.MasterGroups_StructureCode = "";
            mModel.MasterGroups_SystemCode = false;

            var mList = ctxTFAT.MasterGroups.Where(x => (x.RECORDKEY == mModel.MasterGroups_GrpKey)).FirstOrDefault();
            if (mList != null)
            {
                // treeview specific fields
                mModel.MasterGroups_Grp = mList.Code;
                mModel.GrpName = mList.Name;
                mModel.MasterGroups_Level = (byte)((mList.Level != null ? mList.Level : 0) + 1);
            }
            mModel.Mode = "Add";
            mModel.BaseGrList = GetBaseGrList();
            var html = ViewHelper.RenderPartialView(this, "AccountGroupsView", mModel);
            return Json(new { Html = html, Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #region SaveData
        public ActionResult SaveData(AccountGroupsVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteAccountGroups(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    MasterGroups mobj = new MasterGroups();
                    bool mAdd = true;
                    string mNewCode = "";
                    if (ctxTFAT.MasterGroups.Where(x => (x.Code == mModel.MasterGroups_Code)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.MasterGroups.Where(x => (x.Code == mModel.MasterGroups_Code)).FirstOrDefault();
                        mAdd = false;
                    }
                    mobj.Code = mModel.MasterGroups_Code;
                    mobj.Name = mModel.MasterGroups_Name;
                    mobj.Grp = mModel.MasterGroups_Grp;
                    mobj.RevGroup = mModel.MasterGroups_RevGroup;
                    mobj.BaseGr = mModel.MasterGroups_BaseGr;
                    mobj.Seq = mModel.MasterGroups_Seq;
                    mobj.Prefix = mModel.MasterGroups_Prefix;
                    mobj.Hide = mModel.MasterGroups_Hide;
                    mobj.DisplayOrder = mModel.MasterGroups_DisplayOrder;
                    mobj.ForceCC = mModel.MasterGroups_ForceCC;
                    mobj.Sch = mModel.MasterGroups_Sch;
                    mobj.SystemCode = mModel.MasterGroups_SystemCode;
                    // iX9: default values for the fields not used @Form
                    mobj.AcType = "";
                    mobj.AliasPrefix = "";
                    mobj.GrpKey = 0;
                    mobj.IsLast = false;
                    mobj.Level = ctxTFAT.MasterGroups.Where(x => x.Code == mModel.MasterGroups_Grp).Select(x => x.Level).FirstOrDefault() + 1;
                    mobj.NoDetails = false;
                    mobj.PCCode = 0;
                    mobj.StructureCode = "";
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mAUTHORISE;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mAdd == true)
                    {
                        mobj.Code = GetNextCode();
                    }
                    if (mAdd == true)
                    {
                        ctxTFAT.MasterGroups.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    mNewCode = mobj.Code;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mNewCode, DateTime.Now, 0, mNewCode, "Save Account Groups", "G");
                    UpdateGrpKey("MasterGroups");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "AccountGroups" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "AccountGroups" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "AccountGroups" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "AccountGroups" }, JsonRequestBehavior.AllowGet);
        }

        public string GetNextCode()
        {
            string Code = (from x in ctxTFAT.MasterGroups select x.Code).Max();
            string digits = new string(Code.Where(char.IsDigit).ToArray());
            int number;
            if (!int.TryParse(digits, out number)) //int.Parse would do the job since only digits are selected
            {
                number = 0;
            }
            return (++number).ToString("D9");
        }

        public ActionResult DeleteAccountGroups(AccountGroupsVM mModel)
        {
            if (mModel.MasterGroups_Code == null || mModel.MasterGroups_Code == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            // iX9: Check for Active Master MasterGroups
            string mactivestring = "";
            var mactive1 = ctxTFAT.Master.Where(x => (x.Grp == mModel.MasterGroups_Code)).Select(x => x.Name).FirstOrDefault();
            if (mactive1 != null) { mactivestring = mactivestring + "\nMaster: " + mactive1; }
            var mactive2 = ctxTFAT.MasterGroups.Where(x => (x.Grp == mModel.MasterGroups_Code)).Select(x => x.Name).FirstOrDefault();
            if (mactive2 != null) { mactivestring = mactivestring + "\nMasterGroups: " + mactive2; }
            var mactive3 = ctxTFAT.TypeAccountGroups.Where(x => (x.Code == mModel.MasterGroups_Code)).Select(x => x.Code).FirstOrDefault();
            if (mactive3 != null) { mactivestring = mactivestring + "\nTypeAccountGroups: " + mactive3; }
            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "Active Item, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var mList = ctxTFAT.MasterGroups.Where(x => (x.Code == mModel.MasterGroups_Code)).FirstOrDefault();
            ctxTFAT.MasterGroups.Remove(mList);
            ctxTFAT.SaveChanges();
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.MasterGroups_Code, DateTime.Now, 0, mModel.MasterGroups_Code, "Delete Account Groups", "G");

            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            //////IReportGridOperation mIlst = new ListViewGridOperationreport();
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        public ActionResult GetGridData(GridOption Model)
        {
            var mList = ctxTFAT.MasterGroups.Where(x => (x.RECORDKEY.ToString() == Model.Code)).FirstOrDefault();
            if (mList != null)
            {
                Model.Code = mList.Code;
            }
            //GridOption gridOption = new GridOption();
            //gridOption.ViewDataId = Model.ViewDataId;
            //gridOption.FromDate = Model.FromDate;
            //gridOption.ToDate = Model.ToDate;
            //gridOption.mWhat = Model.mWhat;

            //int x = (int)System.Web.HttpContext.Current.Session["GridRows"];
            //gridOption.rows = Model.rows;
            //gridOption.page = Model.page == 0 ? 1 : Model.page;
            //gridOption.searchField = Model.searchField;
            //gridOption.searchOper = Model.searchOper;
            //gridOption.searchString = Model.searchString;
            //gridOption.sidx = Model.sidx;
            //gridOption.sord = Model.sord;
            Model.rows = 5000;
            return GetGridReport(Model, "X", "Code^" + Model.Code, false, 0);
        }

    }
}