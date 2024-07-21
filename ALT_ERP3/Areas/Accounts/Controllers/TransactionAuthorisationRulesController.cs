using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
////using ALT_ERP3.DynamicBusinessLayer;
////using ALT_ERP3.DynamicBusinessLayer.Repository;
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
    public class TransactionAuthorisationRulesController : BaseController
    {
        //private //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        //private IBusinessCommon mBuss = new BusinessCommon();
        ////ITransactionGridOperation mIlst = new TransactionGridOperation();
        private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mAUTHORISE = "A00";
        private static string mdocument = "";
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        private DataTable table = new DataTable();

        #region GetLists

        public List<string> GetChild(string id)
        {
            List<string> ChildList = new List<string>();

            var mTreeList = ctxTFAT.TfatBranch.Where(x => x.Code==id).FirstOrDefault();
            NRecursiveObject abc = new NRecursiveObject();
            abc.data = mTreeList.Name;
            abc.id = mTreeList.Code;
            abc.children = FillRecursive1(id);



            return ChildList;
        }


        public  List<NRecursiveObject> FillRecursive1( string parentId)
        {

            var mTreeeList = ctxTFAT.TfatBranch.Where(x => x.Status == false && (x.Category == "Branch" || x.Category == "SubBranch" || x.Category == "Area")).Select(x => new { x.Name, x.Grp, x.Code, x.Category, x.Status }).ToList();

            var Mobje = (from TfatBranch in ctxTFAT.TfatBranch
                         where TfatBranch.Status == false && (TfatBranch.Category == "Branch" || TfatBranch.Category == "SubBranch" || TfatBranch.Category == "Area")
                         select new NFlatObject()
                         {
                             ParentId= TfatBranch.Grp,
                             Id=TfatBranch.Code
                         }).ToList();
            List<NRecursiveObject> recursiveObjects = new List<NRecursiveObject>();
            foreach (var item in Mobje.Where(x => x.ParentId.Equals(parentId)))
            {
                var Name = item.data;
                if (item.Category == "Zone")
                {
                    Name = item.data + " - (Zone)";
                }
                else if (item.Category == "Branch")
                {
                    Name = item.data + " - (Branch)";
                }
                else if (item.Category == "SubBranch")
                {
                    Name = item.data + " - (Sub-Branch)";
                }
                else if (item.Category == "Area")
                {
                    Name = item.data + " - (Area)";
                }

                recursiveObjects.Add(new NRecursiveObject
                {
                    data = Name,
                    id = item.Id,
                    attr = new NFlatTreeAttribute { id = item.Id.ToString(), selected = item.isSelected },
                    children = FillRecursive1( item.Id)
                });
            }
            return recursiveObjects;
        }

        public JsonResult AutoCompleteType(string term)
        {

            var TypeList = ctxTFAT.TfatUserAuditHeader.Select(x => x.Type).ToList();
            return Json((from m in ctxTFAT.DocTypes
                         where m.Name.ToLower().Contains(term.ToLower())  && !TypeList.Contains(m.Code) && m.Code.Length==5
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteUserID(string term)
        {
            return Json((from m in ctxTFAT.TfatPass
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteAuthNewType(string term)
        {
            return Json((from m in ctxTFAT.DocTypes
                         where m.Name.ToLower().Contains(term.ToLower()) 
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        private List<SelectListItem> PopulateBranches()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM TfatBranch where Code='HO0000' or Category='Branch' or Category='SubBranch' ";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            items.Add(new SelectListItem
                            {
                                Text = sdr["Name"].ToString(),
                                Value = sdr["Code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return items;
        }
        #endregion GetLists


        public List<string> Child(string id)
        {
            List<string> Child = new List<string>();

            var GetChild = GetChildGrp(id);

            var NewDistinct = GetChild.Distinct().ToList();


            return NewDistinct;
        }
        public List<string> GetChildGrp (string Id)
        {
            List<string> Child = new List<string>();
            Child.Add(Id);
            var GrpList = ctxTFAT.TfatBranch.Where(x => x.Grp == Id).ToList();

            foreach (var item in GrpList)
            {
                Child.Add(item.Code);
                var AnotherChild = GetChildGrp(item.Code);
                Child.AddRange(AnotherChild);
            }
            return Child.Distinct().ToList();
        }

        public ActionResult Index(TransactionAuthorisationRulesVM mModel)
        {
            //GetAllMenu(Session["ModuleName"].ToString());
            ////AccountingGetAllMenu(Session["ModuleName"].ToString());
            ////VehicleGetAllMenu(Session["ModuleName"].ToString());
            ///

            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "D");
            mdocument = mModel.Document;
            Session["GridDataSession"] = null;
            mModel.TfatUserAuditHeader_Type = mModel.Document;
            mModel.Branches = PopulateBranches();
            mModel.ReqBranches = new List<SelectListItem>();
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.TfatUserAuditHeader.Where(x => (x.Type == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    var mType = ctxTFAT.DocTypes.Where(x => x.Code == mList.Type).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mAuthNewType = ctxTFAT.DocTypes.Where(x => x.Code == mList.AuthNewType).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    mModel.TfatUserAuditHeader_Type = mType != null ? mType.Code.ToString() : "";
                    mModel.TypeName = mType != null ? mType.Name : "";
                    mModel.TfatUserAuditHeader_AuthNewType = mAuthNewType != null ? mAuthNewType.Code.ToString() : "";
                    mModel.AuthNewTypeName = mAuthNewType != null ? mAuthNewType.Name : "";
                    //mModel.TfatUserAuditHeader_AuthReq = mList.AuthReq;
                    mModel.TfatUserAuditHeader_AuthEach = mList.AuthEach;
                    mModel.TfatUserAuditHeader_AuthLock = mList.AuthLock;
                    mModel.TfatUserAuditHeader_AuthLockDelete = mList.DeleteLock;
                    mModel.TfatUserAuditHeader_OptAuthLock = mList.OptAuthLock;
                    mModel.TfatUserAuditHeader_OptAuthLockDelete = mList.OptDeleteLock;
                    mModel.TfatUserAuditHeader_AuthAgain = mList.AuthAgain;
                    mModel.TfatUserAuditHeader_AuthNoPrint = mList.AuthNoPrint;
                    mModel.TfatUserAuditHeader_AuthNewSerial = mList.AuthNewSerial;
                    mModel.TfatUserAuditHeader_AuthCond = mList.AuthCond;
                    mModel.TfatUserAuditHeader_AuthTimeBound = mList.AuthTimeBound;
                    mModel.TfatUserAuditHeader_AuthTimeLimit = mList.AuthTimeLimit != null ? mList.AuthTimeLimit.Value : 0;
                    mModel.AuthenticateBranchList = mList.AuthBranch;
                    mModel.AuthenticateReqBranchList = mList.AuthReqBranch;


                    var mList2 = ctxTFAT.TfatUserAudit.Where(x => x.Type == mModel.TfatUserAuditHeader_Type).ToList();
                    List<TransactionAuthorisationRulesVM> mList3 = new List<TransactionAuthorisationRulesVM>();
                    int n = 1;
                    foreach (var eachvalue in mList2)
                    {
                        var mUserID = ctxTFAT.TfatPass.Where(x => x.Code == eachvalue.UserID).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                        List<SelectListItem> SelectedBranch = new List<SelectListItem>();
                        if (!String.IsNullOrEmpty(eachvalue.AppBranch))
                        {
                            var ListBtanchCode = eachvalue.AppBranch.Split(',').ToList();
                            SelectedBranch = mModel.Branches.Where(x => ListBtanchCode.Contains(x.Value)).ToList();
                        }

                        
                        mList3.Add(new TransactionAuthorisationRulesVM()
                        {
                            TfatUserAudit_Sno = eachvalue.Sno != null ? eachvalue.Sno.Value : 0,
                            TfatUserAudit_UserLevel = eachvalue.UserLevel,
                            TfatUserAudit_SancLimit = eachvalue.SancLimit != null ? eachvalue.SancLimit.Value : 0,
                            TfatUserAudit_SendEmail = eachvalue.SendEmail,
                            TfatUserAudit_SendMSG = eachvalue.SendMSG,
                            TfatUserAudit_SendSMS = eachvalue.SendSMS,
                            TfatUserAudit_UserID = mUserID != null ? mUserID.Code.ToString() : "",
                            AppBranch = eachvalue.AppBranch,
                            Branches= SelectedBranch,
                            UserIDName = mUserID != null ? mUserID.Name : "",
                            tEmpID = n,
                            tempIsDeleted = false
                        });
                        n = n + 1;
                    }
                    Session.Add("GridDataSession", mList3);
                    mModel.GridDataVM = mList3;
                }
            }
            else
            {
                mModel.TfatUserAuditHeader_AuthAgain = false;
                mModel.TfatUserAuditHeader_AuthCond = false;
                mModel.TfatUserAuditHeader_AuthEach = false;
                mModel.TfatUserAuditHeader_AuthLock = false;
                mModel.TfatUserAuditHeader_AuthNewSerial = false;
                mModel.TfatUserAuditHeader_AuthNewType = "";
                mModel.TfatUserAuditHeader_AuthNoPrint = false;
                mModel.TfatUserAuditHeader_AuthReq = false;
                mModel.TfatUserAuditHeader_AuthRule = 0;
                mModel.TfatUserAuditHeader_AuthSame = false;
                mModel.TfatUserAuditHeader_AuthTimeBound = false;
                mModel.TfatUserAuditHeader_AuthTimeLimit = 0;
                mModel.TfatUserAuditHeader_Type = "";
            }
            return View(mModel);
        }

        #region GridOperations
        // following action is used when row is added to grid in add mode
        [HttpPost]
        public ActionResult AddToTable(TransactionAuthorisationRulesVM Model)
        {
            string Status = "Success",Message="";
            Model.Branches = PopulateBranches();
            List<SelectListItem> SelectedBranch = new List<SelectListItem>();
            if (!String.IsNullOrEmpty(Model.AppBranch))
            {
                var ListBtanchCode = Model.AppBranch.Split(',').ToList();
                SelectedBranch = Model.Branches.Where(x => ListBtanchCode.Contains(x.Value)).ToList();
            }

            List<TransactionAuthorisationRulesVM> objgriddetail = new List<TransactionAuthorisationRulesVM>();
            if (Session["GridDataSession"] != null)
            {
                objgriddetail = (List<TransactionAuthorisationRulesVM>)Session["GridDataSession"];
            }

            if (objgriddetail.Where(x=>x.TfatUserAudit_UserID == Model.TfatUserAudit_UserID && x.TfatUserAudit_UserLevel == Model.TfatUserAudit_UserLevel).FirstOrDefault()==null)
            {
                objgriddetail.Add(new TransactionAuthorisationRulesVM()
                {
                    TfatUserAudit_Sno = Model.TfatUserAudit_Sno,
                    TfatUserAudit_UserLevel = Model.TfatUserAudit_UserLevel,
                    TfatUserAudit_UserID = Model.TfatUserAudit_UserID,
                    UserIDName = Model.UserIDName,
                    TfatUserAudit_SancLimit = Model.TfatUserAudit_SancLimit,
                    TfatUserAudit_SendEmail = Model.TfatUserAudit_SendEmail,
                    TfatUserAudit_SendMSG = Model.TfatUserAudit_SendMSG,
                    TfatUserAudit_SendSMS = Model.TfatUserAudit_SendSMS,
                    tEmpID = objgriddetail.Count + 1,
                    AppBranch = Model.AppBranch,
                    Branches = SelectedBranch,
                    tempIsDeleted = false
                });
            }
            else
            {
                Status = "Error";
                Message = "Same User With Same Level Already Created So Cant Create Duplicate Authorosation Rule..!";
            }
            
            Session.Add("GridDataSession", objgriddetail);
            Model.AppBranch = "";
            var html = ViewHelper.RenderPartialView(this, "GridDataView", new TransactionAuthorisationRulesVM() { Branches=Model.Branches, GridDataVM = objgriddetail, Mode = "Add" });
            return Json(new
            {
                GridDataVM = objgriddetail,
                Html = html,
                Mode = "Add",
                Status= Status,
                Message= Message
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PickFromTable(TransactionAuthorisationRulesVM Model)
        {
            Model.Branches = PopulateBranches();
            

            var result = (List<TransactionAuthorisationRulesVM>)Session["GridDataSession"];
            var result1 = result.Where(x => x.tEmpID == Model.tEmpID);
            foreach (var item in result1)
            {
                Model.TfatUserAudit_Sno = item.TfatUserAudit_Sno;
                Model.TfatUserAudit_UserLevel = item.TfatUserAudit_UserLevel;
                Model.TfatUserAudit_UserID = item.TfatUserAudit_UserID;
                Model.UserIDName = item.UserIDName;
                Model.TfatUserAudit_SancLimit = item.TfatUserAudit_SancLimit;
                Model.TfatUserAudit_SendEmail = item.TfatUserAudit_SendEmail;
                Model.TfatUserAudit_SendMSG = item.TfatUserAudit_SendMSG;
                Model.TfatUserAudit_SendSMS = item.TfatUserAudit_SendSMS;
                Model.tEmpID = item.tEmpID;
                Model.GridDataVM = result;
                Model.AppBranch = item.AppBranch;
            }
            return Json(new
            {
                Html = this.RenderPartialView("GridDataView", Model),
                AppBranch= Model.AppBranch
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult AddToTableEdit(TransactionAuthorisationRulesVM Model)
        {
            Model.Branches = PopulateBranches();
            List<SelectListItem> SelectedBranch = new List<SelectListItem>();
            if (!String.IsNullOrEmpty(Model.AppBranch))
            {
                var ListBtanchCode = Model.AppBranch.Split(',').ToList();
                SelectedBranch = Model.Branches.Where(x => ListBtanchCode.Contains(x.Value)).ToList();
            }

            var result = (List<TransactionAuthorisationRulesVM>)Session["GridDataSession"];
            foreach (var item in result.Where(x => x.tEmpID == Model.tEmpID))
            {
                item.TfatUserAudit_Sno = Model.TfatUserAudit_Sno;
                item.TfatUserAudit_UserLevel = Model.TfatUserAudit_UserLevel;
                item.TfatUserAudit_UserID = Model.TfatUserAudit_UserID;
                item.UserIDName = Model.UserIDName;
                item.TfatUserAudit_SancLimit = Model.TfatUserAudit_SancLimit;
                item.TfatUserAudit_SendEmail = Model.TfatUserAudit_SendEmail;
                item.TfatUserAudit_SendMSG = Model.TfatUserAudit_SendMSG;
                item.TfatUserAudit_SendSMS = Model.TfatUserAudit_SendSMS;
                item.tEmpID = Model.tEmpID;
                item.tempIsDeleted = false;
                item.AppBranch = Model.AppBranch;
                item.Branches = SelectedBranch;
            }
            Session.Add("GridDataSession", result);
            var html = ViewHelper.RenderPartialView(this, "GridDataView", new TransactionAuthorisationRulesVM() { Branches = Model.Branches, GridDataVM = result, Mode = "Add" });
            return Json(new
            {
                GridDataVM = result,
                Html = html,
                Mode = "Add"
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteTableRow(int tEmpID, TransactionAuthorisationRulesVM Model)
        {
            var result = (List<TransactionAuthorisationRulesVM>)Session["GridDataSession"];
            result.Where(x => x.tEmpID == tEmpID).FirstOrDefault().tempIsDeleted = true;
            result = result.Where(x => x.tEmpID != tEmpID).ToList();
            int i = 1;
            foreach (var item in result)
            {
                item.tEmpID = i++;
            }
            Session.Add("GridDataSession", result);
            var html = ViewHelper.RenderPartialView(this, "GridDataView", new TransactionAuthorisationRulesVM() { Branches = PopulateBranches(), GridDataVM = result });
            return Json(new { SelectedIndent = result, Html = html }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region SaveData
        public ActionResult SaveData(TransactionAuthorisationRulesVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteTransactionAuthorisationRules(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.TfatUserAuditHeader_Type, DateTime.Now, 0, mModel.TfatUserAuditHeader_Type, "Delete Transaction Authorisation Rules", "D");
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    TfatUserAuditHeader mobj = new TfatUserAuditHeader();
                    bool mAdd = true;
                    string mNewCode = "";
                    if (ctxTFAT.TfatUserAuditHeader.Where(x => (x.Type == mModel.TfatUserAuditHeader_Type)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.TfatUserAuditHeader.Where(x => (x.Type == mModel.TfatUserAuditHeader_Type)).FirstOrDefault();
                        mAdd = false;
                    }
                    mobj.Type = mModel.TfatUserAuditHeader_Type;
                    //mobj.AuthReq = mModel.TfatUserAuditHeader_AuthReq;
                    mobj.AuthEach = mModel.TfatUserAuditHeader_AuthEach;
                    mobj.AuthLock = mModel.TfatUserAuditHeader_AuthLock;
                    mobj.DeleteLock = mModel.TfatUserAuditHeader_AuthLockDelete;
                    mobj.OptAuthLock = mModel.TfatUserAuditHeader_OptAuthLock;
                    mobj.OptDeleteLock = mModel.TfatUserAuditHeader_OptAuthLockDelete;
                    mobj.AuthAgain = mModel.TfatUserAuditHeader_AuthAgain;
                    mobj.AuthNoPrint = mModel.TfatUserAuditHeader_AuthNoPrint;
                    mobj.AuthNewType = mModel.TfatUserAuditHeader_AuthNewType;
                    mobj.AuthNewSerial = mModel.TfatUserAuditHeader_AuthNewSerial;
                    mobj.AuthCond = mModel.TfatUserAuditHeader_AuthCond;
                    mobj.AuthTimeBound = mModel.TfatUserAuditHeader_AuthTimeBound;
                    mobj.AuthTimeLimit = mModel.TfatUserAuditHeader_AuthTimeLimit;
                    mobj.AuthBranch = mModel.AuthenticateBranchList;
                    mobj.AuthReqBranch = mModel.AuthenticateReqBranchList;
                    // iX9: default values for the fields not used @Form
                    mobj.AuthRule = 0;
                    mobj.AuthSame = false;
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mAUTHORISE;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mAdd == true)
                    {
                        ctxTFAT.TfatUserAuditHeader.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    mNewCode = mobj.Type;
                    SaveGridData(mModel);
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mNewCode, DateTime.Now, 0, mNewCode, "Save Transaction Authorisation Rules", "D");

                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "TransactionAuthorisationRules" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "TransactionAuthorisationRules" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "TransactionAuthorisationRules" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "TransactionAuthorisationRules" }, JsonRequestBehavior.AllowGet);
        }
        public void SaveGridData(TransactionAuthorisationRulesVM mModel)
        {
            // delete the existing data from the table
            var mList = ctxTFAT.TfatUserAudit.Where(x => x.Type == mModel.TfatUserAuditHeader_Type).ToList();
            if (mList.Count != 0)
            {
                ctxTFAT.TfatUserAudit.RemoveRange(mList);
                ctxTFAT.SaveChanges();
            }
            var mList2 = (List<TransactionAuthorisationRulesVM>)Session["GridDataSession"];
            if (mList2 != null)
            {
                var mList3 = ((List<TransactionAuthorisationRulesVM>)Session["GridDataSession"]).Where(x => x.tempIsDeleted == false);
                foreach (var eachvalue in mList3)
                {
                    TfatUserAudit mgriddata = new TfatUserAudit();
                    mgriddata.Type = mModel.TfatUserAuditHeader_Type;
                    mgriddata.UserLevel = eachvalue.TfatUserAudit_UserLevel;
                    mgriddata.UserID = eachvalue.TfatUserAudit_UserID;
                    mgriddata.AppBranch = eachvalue.AppBranch;
                    mgriddata.SancLimit = eachvalue.TfatUserAudit_SancLimit;
                    mgriddata.SendEmail = eachvalue.TfatUserAudit_SendEmail;
                    mgriddata.SendMSG = eachvalue.TfatUserAudit_SendMSG;
                    mgriddata.SendSMS = eachvalue.TfatUserAudit_SendSMS;
                    mgriddata.Sno = eachvalue.tEmpID;
                    mgriddata.ENTEREDBY = muserid;
                    mgriddata.LASTUPDATEDATE = DateTime.Now;
                    mgriddata.AUTHORISE = mAUTHORISE;
                    mgriddata.AUTHIDS = muserid;
                    ctxTFAT.TfatUserAudit.Add(mgriddata);
                    ctxTFAT.SaveChanges();
                }
            }
            Session["GridDataSession"] = null;
        }

        public ActionResult DeleteTransactionAuthorisationRules(TransactionAuthorisationRulesVM mModel)
        {
            if (mModel.TfatUserAuditHeader_Type == null || mModel.TfatUserAuditHeader_Type == "")
            {
                return Json(new
                {
                    Message = "Type not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            // iX9: Check for Active Master TfatUserAuditHeader
            string mactivestring = "";
            //var mactive1 = ctxTFAT.TfatUserAudit.Where(x => (x.Type == mModel.TfatUserAuditHeader_Type)).Select(x => x.Type).FirstOrDefault();
            //if (mactive1 != null) { mactivestring = mactivestring + "\nTfatUserAudit: " + mactive1; }
            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "Active Item, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var mList = ctxTFAT.TfatUserAuditHeader.Where(x => (x.Type == mModel.TfatUserAuditHeader_Type)).FirstOrDefault();
            ctxTFAT.TfatUserAuditHeader.Remove(mList);
            var mList2 = ctxTFAT.TfatUserAudit.Where(x => x.Type == mModel.TfatUserAuditHeader_Type).ToList();
            ctxTFAT.TfatUserAudit.RemoveRange(mList2);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData


        public ActionResult AddToFavorite(TransactionAuthorisationRulesVM mModel)
        {

            TfatMenu tfatMenu = ctxTFAT.TfatMenu.Where(x => x.FormatCode.Trim().ToUpper() == mModel.OptionCode.ToUpper().Trim()).FirstOrDefault();
            if (tfatMenu.QuickMaster)
            {
                tfatMenu.QuickMaster = false;
                tfatMenu.QuickMenu = false;
                ctxTFAT.Entry(tfatMenu).State = EntityState.Modified;
                return Json(new { Status = 1,  JsonRequestBehavior.AllowGet });
            }
            else
            {
                tfatMenu.QuickMaster = true;
                tfatMenu.QuickMenu = true;
                ctxTFAT.Entry(tfatMenu).State = EntityState.Modified;
                return Json(new { Status = 0, JsonRequestBehavior.AllowGet });
            }
            


            
        }
    }
}