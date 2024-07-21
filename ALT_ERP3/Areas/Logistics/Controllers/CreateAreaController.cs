using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class CreateAreaController : BaseController
    {
         
        //ITransactionGridOperation mIlst = new TransactionGridOperation();
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        private static string mauthorise = "A00";

        public ActionResult TempGetBranchList(string CompCode)
        {
            List<SelectListItem> CallCategoryList = new List<SelectListItem>();
            if (CompCode == "Zone")
            {
                var List = ctxTFAT.TfatBranch.Where(x => x.Category == "0" && x.Status == true).ToList();
                foreach (var item in List)
                {
                    CallCategoryList.Add(new SelectListItem { Value = item.Code, Text = item.Name + " - HO" });
                }
            }
            else if (CompCode == "Branch")
            {
                var List = ctxTFAT.TfatBranch.Where(x => x.Category == "Zone" && x.Status == true).ToList();
                foreach (var item in List)
                {
                    CallCategoryList.Add(new SelectListItem { Value = item.Code, Text = item.Name + " - Z" });
                }
            }
            else if (CompCode == "SubBranch")
            {
                var List = ctxTFAT.TfatBranch.Where(x => x.Category == "Branch" && x.Status == true).ToList();
                foreach (var item in List)
                {
                    CallCategoryList.Add(new SelectListItem { Value = item.Code, Text = item.Name + " - B" });
                }
            }
            else if (CompCode == "Area")
            {
                var List = ctxTFAT.TfatBranch.Where(x => (x.Category == "Branch" || x.Category == "SubBranch") && x.Status == true).ToList();
                foreach (var item in List)
                {
                    if (item.Category == "Branch")
                    {
                        CallCategoryList.Add(new SelectListItem { Value = item.Code, Text = item.Name + " - B" });
                    }
                    else
                    {
                        CallCategoryList.Add(new SelectListItem { Value = item.Code, Text = item.Name + " - SB" });
                    }
                }
            }

            return Json(CallCategoryList, JsonRequestBehavior.AllowGet);
        }

        public List<SelectListItem> GetTempParentList(string Category)
        {
            List<SelectListItem> CallCategoryList = new List<SelectListItem>();
            if (Category == "Zone")
            {
                var List = ctxTFAT.TfatBranch.Where(x => x.Category == "0" && x.Status == true).ToList();
                foreach (var item in List)
                {
                    CallCategoryList.Add(new SelectListItem { Value = item.Code, Text = item.Name + " - HO" });
                }
            }
            else if (Category == "Branch")
            {
                var List = ctxTFAT.TfatBranch.Where(x => x.Category == "Zone" && x.Status == true).ToList();
                foreach (var item in List)
                {
                    CallCategoryList.Add(new SelectListItem { Value = item.Code, Text = item.Name + " - Z" });
                }
            }
            else if (Category == "SubBranch")
            {
                var List = ctxTFAT.TfatBranch.Where(x => x.Category == "Branch" && x.Status == true).ToList();
                foreach (var item in List)
                {
                    CallCategoryList.Add(new SelectListItem { Value = item.Code, Text = item.Name + " - B" });
                }
            }
            else if (Category == "Area")
            {
                var List = ctxTFAT.TfatBranch.Where(x => (x.Category == "Branch" || x.Category == "SubBranch") && x.Status == true).ToList();
                foreach (var item in List)
                {
                    if (item.Category == "Branch")
                    {
                        CallCategoryList.Add(new SelectListItem { Value = item.Code, Text = item.Name + " - B" });
                    }
                    else
                    {
                        CallCategoryList.Add(new SelectListItem { Value = item.Code, Text = item.Name + " - SB" });
                    }
                }
            }
            return CallCategoryList;
        }

        public JsonResult LoadState(string term)
        {
            string Msg = "";

            var list = ctxTFAT.TfatState.Where(x => x.Code == 1).ToList();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = ctxTFAT.TfatState.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }
            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public List<SelectListItem> GetCategoryList(string Category)
        {
            List<SelectListItem> CallCategoryList = new List<SelectListItem>();
            CallCategoryList.Add(new SelectListItem { Value = "0", Text = "--------Select Category--------" });
            if (Category == "0")
            {
                CallCategoryList.Add(new SelectListItem { Value = "Zone", Text = "Zone" });
                CallCategoryList.Add(new SelectListItem { Value = "Branch", Text = "Branch" });
                //CallCategoryList.Add(new SelectListItem { Value = "SubBranch", Text = "SubBranch" });
                //CallCategoryList.Add(new SelectListItem { Value = "Area", Text = "Area" });
            }
            else if (Category == "Zone")
            {
                CallCategoryList.Add(new SelectListItem { Value = "Branch", Text = "Branch" });
                //CallCategoryList.Add(new SelectListItem { Value = "SubBranch", Text = "SubBranch" });
                //CallCategoryList.Add(new SelectListItem { Value = "Area", Text = "Area" });
            }
            else if (Category == "Branch")
            {
                CallCategoryList.Add(new SelectListItem { Value = "SubBranch", Text = "SubBranch" });
               // CallCategoryList.Add(new SelectListItem { Value = "Area", Text = "Area" });
            }
            else if (Category == "SubBranch" || Category == "G00000")
            {
                //CallCategoryList.Add(new SelectListItem { Value = "Area", Text = "Area" });
            }
            else
            {
                CallCategoryList.Add(new SelectListItem { Value = "Zone", Text = "Zone" });
                CallCategoryList.Add(new SelectListItem { Value = "Branch", Text = "Branch" });
                CallCategoryList.Add(new SelectListItem { Value = "SubBranch", Text = "SubBranch" });
                //CallCategoryList.Add(new SelectListItem { Value = "Area", Text = "Area" });
            }
            return CallCategoryList;
        }
        public List<SelectListItem> GetTypeList()
        {
            List<SelectListItem> CallCategoryList = new List<SelectListItem>();
            CallCategoryList.Add(new SelectListItem { Value = "0", Text = "--------Select Type--------" });
            CallCategoryList.Add(new SelectListItem { Value = "H", Text = "Office" });
            CallCategoryList.Add(new SelectListItem { Value = "G", Text = "Godown" });
            return CallCategoryList;
        }

       


        #region GetLists
        public List<SelectListItem> GetBusinessList()
        {
            List<SelectListItem> CallBusinessList = new List<SelectListItem>();
            CallBusinessList.Add(new SelectListItem { Value = "T", Text = "Trading" });
            CallBusinessList.Add(new SelectListItem { Value = "M", Text = "Manufacturing" });
            CallBusinessList.Add(new SelectListItem { Value = "T", Text = "Trading/Manufacturing" });
            CallBusinessList.Add(new SelectListItem { Value = "O", Text = "Others" });
            CallBusinessList.Add(new SelectListItem { Value = "A", Text = "Accountant" });
            return CallBusinessList;
        }
        public List<SelectListItem> Getgp_VATGSTList()
        {
            List<SelectListItem> Callgp_VATGSTList = new List<SelectListItem>();
            Callgp_VATGSTList.Add(new SelectListItem { Value = "G", Text = "GST" });
            Callgp_VATGSTList.Add(new SelectListItem { Value = "V", Text = "VAT" });
            Callgp_VATGSTList.Add(new SelectListItem { Value = "N", Text = "None" });
            return Callgp_VATGSTList;
        }
        public List<SelectListItem> Getgp_AutoAccStyleList()
        {
            List<SelectListItem> Callgp_AutoAccStyleList = new List<SelectListItem>();
            Callgp_AutoAccStyleList.Add(new SelectListItem { Value = "0", Text = "Continues" });
            Callgp_AutoAccStyleList.Add(new SelectListItem { Value = "1", Text = "Group Based" });
            return Callgp_AutoAccStyleList;
        }
        public JsonResult AutoCompleteGrp(string term, string Area)
        {

            List<SelectListItem> CallCategoryList = new List<SelectListItem>();
            if (Area == "Zone")
            {
                var List = ctxTFAT.TfatBranch.Where(x => x.Category == "0" && x.Status == true).ToList();
                foreach (var item in List)
                {
                    CallCategoryList.Add(new SelectListItem { Value = item.Code, Text = item.Name + " - HO" });
                }
            }
            else if (Area == "Branch")
            {
                var List = ctxTFAT.TfatBranch.Where(x => x.Category == "Zone" && x.Status == true).ToList();
                foreach (var item in List)
                {
                    CallCategoryList.Add(new SelectListItem { Value = item.Code, Text = item.Name + " - Z" });
                }
            }
            else if (Area == "SubBranch")
            {
                var List = ctxTFAT.TfatBranch.Where(x => x.Category == "Branch" && x.Status == true).ToList();
                foreach (var item in List)
                {
                    CallCategoryList.Add(new SelectListItem { Value = item.Code, Text = item.Name + " - B" });
                }
            }
            else if (Area == "Area")
            {
                var List = ctxTFAT.TfatBranch.Where(x => (x.Category == "Branch" || x.Category == "SubBranch") && x.Status == true).ToList();
                foreach (var item in List)
                {
                    if (item.Category == "Branch")
                    {
                        CallCategoryList.Add(new SelectListItem { Value = item.Code, Text = item.Name + " - B" });
                    }
                    else
                    {
                        CallCategoryList.Add(new SelectListItem { Value = item.Code, Text = item.Name + " - SB" });
                    }
                }
            }


            var Modified = CallCategoryList.Select(x => new
            {
                Code = x.Value,
                Name = x.Text
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
            // return Json(CallCategoryList, JsonRequestBehavior.AllowGet);



            //return Json((from m in ctxTFAT.TfatBranch
            //             where m.Name.ToLower().Contains(term.ToLower())
            //             select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteCompCode(string term)
        {
            return Json((from m in ctxTFAT.TfatComp
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteLocationCode(string term)
        {

            return Json((from m in ctxTFAT.Warehouse
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteCountry(string term)
        {
            return Json((from m in ctxTFAT.TfatCountry
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code.ToString() }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteState(string term)
        {
            return Json((from m in ctxTFAT.TfatState
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code.ToString() }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteCity(string term)
        {
            return Json((from m in ctxTFAT.TfatCity
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code.ToString() }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteCurrName(string term)
        {
            return Json((from m in ctxTFAT.CurrencyMaster
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteAccount(string term)
        {
            return Json((from m in ctxTFAT.Master
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        #endregion GetLists
        // GET: Logistics/CreateArea
        public ActionResult Index(BranchVM mModel)
        {
            //ViewBag.compname = compname;

            //GetAllMenu(Session["ModuleName"].ToString());
            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "B");

            mModel.CategoryList = GetCategoryList("0");
            mModel.TypeList = GetTypeList();

            return View(mModel);
        }

        #region TreeView
        public string TreeView()
        {
            string BranchCode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
            var mTreeList = ctxTFAT.TfatBranch.Where(x => x.Status == true).OrderBy(x=>x.RECORDKEY).Select(x => new { x.Name, x.Grp, x.Code, x.Category, x.Status }).ToList();

            List<NFlatObject> flatObjects2 = new List<NFlatObject>();
            for (int n = 0; n < mTreeList.Count; n++)
            {
                NFlatObject abc = new NFlatObject();
                abc.data = mTreeList[n].Name;
                abc.Id = mTreeList[n].Code;
                abc.Status = mTreeList[n].Status;
                abc.Category = mTreeList[n].Category;
                if (BranchCode == abc.Id)
                {
                    abc.isSelected = true;
                }
                else
                {
                    abc.isSelected = false;
                }

                if (mTreeList[n].Code == mTreeList[n].Grp)
                {
                    abc.ParentId = "0";
                }
                else
                {
                    abc.ParentId = mTreeList[n].Grp;
                }
                flatObjects2.Add(abc);
            }
            var recursiveObjects = FillRecursive1(flatObjects2, "0");
            string myjsonmodel = new JavaScriptSerializer().Serialize(recursiveObjects);
            ViewBag.Tree = myjsonmodel;
            return myjsonmodel;

        }
        public string GeneralTreeView()
        {
            string BranchCode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
            var mTreeList = ctxTFAT.TfatBranch.Where(x => x.Status == true).Select(x => new { x.Name, x.Grp, x.Code, x.Category, x.Status }).ToList();

            List<NFlatObject> flatObjects2 = new List<NFlatObject>();
            for (int n = 0; n < mTreeList.Count; n++)
            {
                NFlatObject abc = new NFlatObject();
                abc.data = mTreeList[n].Name;
                abc.Id = mTreeList[n].Code;
                abc.Status = mTreeList[n].Status;
                abc.Category = mTreeList[n].Category;
                if (BranchCode == abc.Id)
                {
                    abc.isSelected = true;
                }
                else
                {
                    abc.isSelected = false;
                }

                if (mTreeList[n].Code == mTreeList[n].Grp)
                {
                    abc.ParentId = "0";
                }
                else
                {
                    abc.ParentId = mTreeList[n].Grp;
                }
                flatObjects2.Add(abc);
            }
            var recursiveObjects = FillRecursive1(flatObjects2, "0");
            string myjsonmodel = new JavaScriptSerializer().Serialize(recursiveObjects);
            ViewBag.Tree = myjsonmodel;
            return myjsonmodel;

        }
        public string TreeViewOff()
        {
            string BranchCode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
            var mTreeList = ctxTFAT.TfatBranch.Where(x => x.Status == false && (x.Category == "Branch" || x.Category == "SubBranch" || x.Category == "Area")).Select(x => new { x.Name, x.Grp, x.Code, x.Category, x.Status }).ToList();

            List<NFlatObject> flatObjects2 = new List<NFlatObject>();
            for (int n = 0; n < mTreeList.Count; n++)
            {
                NFlatObject abc = new NFlatObject();
                abc.data = mTreeList[n].Name;
                abc.Id = mTreeList[n].Code;
                abc.Status = mTreeList[n].Status;
                abc.Category = mTreeList[n].Category;
                if (BranchCode == abc.Id)
                {
                    abc.isSelected = true;
                }
                else
                {
                    abc.isSelected = false;
                }

                if (mTreeList[n].Code == mTreeList[n].Grp)
                {
                    abc.ParentId = "0";
                }
                else
                {
                    abc.ParentId = mTreeList[n].Grp;
                }
                flatObjects2.Add(abc);
            }

            List<NRecursiveObject> nRecursiveObjects = new List<NRecursiveObject>();
            foreach (var item in flatObjects2)
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
                nRecursiveObjects.Add(new NRecursiveObject
                {
                    data = Name,
                    id = item.Id,
                    attr = new NFlatTreeAttribute { id = item.Id.ToString(), selected = item.isSelected }
                    //children = FillRecursive1(flatObjects, item.Id)
                });
            }

            var recursiveObjects = nRecursiveObjects;
            string myjsonmodel = new JavaScriptSerializer().Serialize(recursiveObjects);
            ViewBag.Tree = myjsonmodel;
            return myjsonmodel;

        }
        public static List<NRecursiveObject> FillRecursive1(List<NFlatObject> flatObjects, string parentId)
        {
            bool mSelected = false;
            List<NRecursiveObject> recursiveObjects = new List<NRecursiveObject>();
            foreach (var item in flatObjects.Where(x => x.ParentId.Equals(parentId)))
            {
                mSelected = item.isSelected;
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
                    children = FillRecursive1(flatObjects, item.Id)
                });
            }
            return recursiveObjects;
        }
        #endregion

        public ActionResult TreeNodeClick(BranchVM mModel)
        {

            mModel.TfatBranch_gp_RCMDate = DateTime.Now;
            mModel.BusinessList = GetBusinessList();
            mModel.gp_VATGSTList = Getgp_VATGSTList();
            mModel.gp_AutoAccStyleList = Getgp_AutoAccStyleList();
            mModel.TypeList = GetTypeList();
            List<SelectListItem> UsersList = new List<SelectListItem>();
            


            var mList = ctxTFAT.TfatBranch.Where(x => (x.Code == mModel.Code)).FirstOrDefault();
            if (mList != null)
            {
                if (!String.IsNullOrEmpty(mList.Users))
                {
                    var GetBranchUserList = mList.Users.Split(',').ToList();
                    var UsersResultX = ctxTFAT.TfatPass.Where(x => GetBranchUserList.Contains(x.Code)).Select(x => new { Code = x.Code, Name = x.Name }).ToList().Distinct();
                    foreach (var Usersitem in UsersResultX)
                    {
                        UsersList.Add(new SelectListItem { Text = Usersitem.Name, Value = Usersitem.Code.ToString() });
                    }
                }
                
                




                if (mList.WorkTimeFrom == null && mList.WorkTimeTo == null)
                {
                    mModel.AllTime = true;
                }
                else
                {
                    mModel.WorkingHoursFrom = mList.WorkTimeFrom;
                    mModel.WorkingHoursTo = mList.WorkTimeTo;
                }
                mModel.Mode = "Edit";
                mModel.Type = mList.BranchType;
                mModel.Category =mList.Code== "G00000" ? "Area": mList.Category;
                mModel.Status = mList.Status;
                mModel.WorkingHoursFrom = mList.WorkTimeFrom;
                mModel.WorkingHoursTo = mList.WorkTimeTo;
                mModel.VehicleActivity = mList.VehicleWaitTime;
                mModel.ParentName = ctxTFAT.TfatBranch.Where(x => x.Code == mList.Grp && mList.Code != mList.Grp).Select(x => x.Name).FirstOrDefault();
                var mGrp = ctxTFAT.TfatBranch.Where(x => x.Code == mList.Grp).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mCompCode = ctxTFAT.TfatComp.Where(x => x.Code == mList.CompCode).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mLocationCode = ctxTFAT.Warehouse.Where(x => x.Code == mList.LocationCode).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mCountry = ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == mList.Country).Select(x => new { Name = x.Name, Code = x.Code.ToString() }).FirstOrDefault();
                var mState = ctxTFAT.TfatState.Where(x => x.Code.ToString() == mList.State).Select(x => new { Name = x.Name, Code = x.Code.ToString() }).FirstOrDefault();
                var mCity = ctxTFAT.TfatCity.Where(x => x.Code.ToString() == mList.City).Select(x => new { Name = x.Name, Code = x.Code.ToString() }).FirstOrDefault();
                var mCurrName = ctxTFAT.CurrencyMaster.Where(x => x.Code == mList.CurrName).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mAccount = ctxTFAT.Master.Where(x => x.Code == mList.Account).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                mModel.TfatBranch_Grp = mGrp != null ? mGrp.Code.ToString() : "";
                mModel.GrpName = mGrp != null ? mGrp.Name : "";
                mModel.TfatBranch_CompCode = mCompCode != null ? mCompCode.Code.ToString() : "";
                mModel.CompCodeName = mCompCode != null ? mCompCode.Name : "";
                mModel.TfatBranch_LocationCode = mLocationCode != null ? mLocationCode.Code : 0;
                mModel.LocationCodeName = mLocationCode != null ? mLocationCode.Name : "";
                mModel.TfatBranch_Country = mCountry != null ? mCountry.Code.ToString() : "";
                mModel.CountryName = mCountry != null ? mCountry.Name : "";
                mModel.TfatBranch_State = mState != null ? mState.Code.ToString() : "";
                mModel.StateName = mState != null ? mState.Name : "";
                mModel.TfatBranch_City = mCity != null ? mCity.Code.ToString() : "";
                mModel.CityName = mCity != null ? mCity.Name : "";
                mModel.TfatBranch_CurrName = mCurrName != null ? mCurrName.Code : 0;
                mModel.CurrNameName = mCurrName != null ? mCurrName.Name : "";
                mModel.TfatBranch_Account = mAccount != null ? mAccount.Code.ToString() : "";
                mModel.AccountName = mAccount != null ? mAccount.Name : "";
                mModel.TfatBranch_Addrl1 = mList.Addrl1;
                mModel.TfatBranch_Code = mList.Code;
                mModel.TfatBranch_gp_DiscAP = mList.gp_DiscAP;
                mModel.TfatBranch_Business = mList.Business;
                mModel.TfatBranch_SMSURL = mList.SMSURL;
                mModel.TfatBranch_Name = mList.Name;
                mModel.TfatBranch_Addrl3 = mList.Addrl3;
                mModel.TfatBranch_gp_AllowDiscAP = mList.gp_AllowDiscAP;
                mModel.TfatBranch_SMSUserId = mList.SMSUserId;
                mModel.TfatBranch_Addrl2 = mList.Addrl2;
                mModel.TfatBranch_aAuthno = mList.aAuthno;
                mModel.TfatBranch_gp_DiscAS = mList.gp_DiscAS;
                mModel.TfatBranch_gp_AllowDiscAS = mList.gp_AllowDiscAS;
                mModel.TfatBranch_Licence2 = mList.Licence2;
                mModel.TfatBranch_Addrl4 = mList.Addrl4;
                mModel.TfatBranch_gp_DiscPP = mList.gp_DiscPP;
                mModel.TfatBranch_aLstno = mList.aLstno;
                mModel.TfatBranch_SMSPass = mList.SMSPass;
                mModel.TfatBranch_gp_AllowDiscPP = mList.gp_AllowDiscPP;
                mModel.TfatBranch_Users = mList.Users;
                mModel.TfatBranch_aCstNo = mList.aCstNo;
                mModel.TfatBranch_SMSCaption = mList.SMSCaption;
                mModel.TfatBranch_gp_DiscPS = mList.gp_DiscPS;
                mModel.TfatBranch_GSTNo = mList.GSTNo;
                mModel.TfatBranch_LogIn = mList.LogIn;
                mModel.TfatBranch_aPin = mList.aPin;
                mModel.TfatBranch_PanNo = mList.PanNo;
                mModel.TfatBranch_gp_AllowDiscPS = mList.gp_AllowDiscPS;
                mModel.TfatBranch_gp_AllowRateP = mList.gp_AllowRateP;
                mModel.TfatBranch_CINNo = mList.CINNo;
                mModel.TfatBranch_SMSPrefix = mList.SMSPrefix;
                mModel.TfatBranch_aFax = mList.aFax;
                mModel.TfatBranch_TimeDiff = mList.TimeDiff != null ? mList.TimeDiff.Value : 0;
                mModel.TfatBranch_VATReg = mList.VATReg;
                mModel.TfatBranch_CurrDec = mList.CurrDec != null ? mList.CurrDec.Value : 0;
                mModel.TfatBranch_gp_AllowRateS = mList.gp_AllowRateS;
                mModel.TfatBranch_Tel1 = mList.Tel1;
                mModel.TfatBranch_gp_BIN = mList.gp_BIN;
                mModel.TfatBranch_gp_VATGST = mList.gp_VATGST;
                mModel.TfatBranch_gp_CLStock = mList.gp_CLStock;
                mModel.TfatBranch_Tel2 = mList.Tel2;
                mModel.TfatBranch_Tel3 = mList.Tel3;
                mModel.TfatBranch_gp_CashLimit = mList.gp_CashLimit;
                mModel.TfatBranch_TINNumber = mList.TINNumber;
                mModel.TfatBranch_Tel4 = mList.Tel4;
                mModel.TfatBranch_gp_CashLimitAmt = mList.gp_CashLimitAmt != null ? mList.gp_CashLimitAmt.Value : 0;
                mModel.TfatBranch_Sun = mList.Sun;
                mModel.TfatBranch_gp_CashLimitWarn = mList.gp_CashLimitWarn;
                mModel.TfatBranch_TDSReg = mList.TDSReg;
                
                mModel.TfatBranch_www = mList.www;
                mModel.TfatBranch_TDSOffice = mList.TDSOffice;
                mModel.TfatBranch_Mon = mList.Mon;
                mModel.TfatBranch_TDSAuthorise = mList.TDSAuthorise;
                mModel.TfatBranch_gp_EnableParty = mList.gp_EnableParty;
                mModel.TfatBranch_PrintInfo = mList.PrintInfo;
                mModel.TfatBranch_TDSCir = mList.TDSCir;
                mModel.TfatBranch_gp_GINQty = mList.gp_GINQty;
                mModel.TfatBranch_gp_GSTStyle = mList.gp_GSTStyle != null ? mList.gp_GSTStyle.Value : 0;
                mModel.TfatBranch_Tue = mList.Tue;
                mModel.TfatBranch_gp_GSTSupply = mList.gp_GSTSupply;
                mModel.TfatBranch_Wed = mList.Wed;
                mModel.TfatBranch_gp_Holiday1 = mList.gp_Holiday1;
                mModel.TfatBranch_Thu = mList.Thu;
                mModel.TfatBranch_gp_Holiday2 = mList.gp_Holiday2;
                mModel.TfatBranch_gp_MultiUnit = mList.gp_MultiUnit;
                mModel.TfatBranch_Fri = mList.Fri;
                mModel.TfatBranch_Sat = mList.Sat;
                mModel.TfatBranch_gp_NegStock = mList.gp_NegStock;
                mModel.TfatBranch_gp_NegStockAsOn = mList.gp_NegStockAsOn;
                mModel.TfatBranch_gp_NegWarn = mList.gp_NegWarn;
                mModel.TfatBranch_gp_PostP = mList.gp_PostP;
                mModel.TfatBranch_gp_PurchPostTDS = mList.gp_PurchPostTDS;
                mModel.TfatBranch_gp_PSP = mList.gp_PSP;
                mModel.TfatBranch_gp_RCMDate = mList.gp_RCMDate != null ? mList.gp_RCMDate.Value : DateTime.Now;
                mModel.TfatBranch_gp_SEZChargeParty = mList.gp_SEZChargeParty;
                mModel.TfatBranch_gp_VatDecP = (byte)(mList.gp_VatDecP != null ? mList.gp_VatDecP.Value : 0);
                mModel.TfatBranch_gp_RoundVAT = (byte)(mList.gp_RoundVAT != null ? mList.gp_RoundVAT.Value : 0);
                mModel.TfatBranch_gp_VatDecS = (byte)(mList.gp_VatDecS != null ? mList.gp_VatDecS.Value : 0);
                mModel.TfatBranch_gp_SPAdjForce = mList.gp_SPAdjForce;
                mModel.TfatBranch_gp_Serial = mList.gp_Serial;
                mModel.TfatBranch_gp_Batch = mList.gp_Batch;
                mModel.TfatBranch_gp_AutoAccCode = mList.gp_AutoAccCode;
                mModel.TfatBranch_gp_AutoAccStyle = (byte)(mList.gp_AutoAccStyle != null ? mList.gp_AutoAccStyle.Value : 0);
                mModel.TfatBranch_gp_AutoAccLength = (byte)(mList.gp_AutoAccLength != null ? mList.gp_AutoAccLength.Value : 0);
                mModel.TfatBranch_gp_SONoDupl = mList.gp_SONoDupl;
                mModel.TfatBranch_DuplicateItemName = mList.gp_DuplicateItemName;
                mModel.gp_AddonBased = mList.gp_AddonBased;
                mModel.gp_AddonBasedDescr = mList.gp_AddonBasedDescr;
                mModel.gp_AddonSepCode = mList.gp_AddonSepCode;
                mModel.gp_AddonSepName = mList.gp_AddonSepName;
                mModel.gp_ItemAutoCode = mList.gp_ItemAutoCode;
                mModel.gp_ItemAutoDescr = mList.gp_ItemAutoDescr;
                mModel.gp_ItemCodeStyle = mList.gp_ItemCodeStyle;
                mModel.gp_ItemDescrStyle = mList.gp_ItemDescrStyle;
                mModel.gp_ItemPrefixName = mList.gp_ItemPrefixName;
                mModel.gp_Length = mList.gp_Length;
                mModel.gp_OrdIncludeRet = mList.gp_OrdIncludeRet;
                mModel.gp_CLStockAddOrder = mList.gp_CLStockAddORder;

                mModel.TfatBranch_Email = mList.Email;
                mModel.BCCTo = mList.BCCTo;
                mModel.CCTo = mList.CCTo;
                mModel.SMTPServer = mList.SMTPServer;
                mModel.SMTPUser = mList.SMTPUser;
                mModel.SMTPPassword = mList.SMTPPassword;
                mModel.SMTPPort = mList.SMTPPort;
                mModel.BranchMail = Convert.ToBoolean(mList.BranchMail);
                mModel.LocalMail = Convert.ToBoolean(mList.LocalMail);

                List<SelectListItem> CallCategoryList = new List<SelectListItem>();
                CallCategoryList.Add(new SelectListItem { Value = "0", Text = "--------Select Category--------" });
                if (mList.Category == "0")
                {
                    CallCategoryList.Add(new SelectListItem { Value = "0", Text = "--------Select Category--------" });
                }
                else if (mList.Category == "Zone")
                {
                    CallCategoryList.Add(new SelectListItem { Value = "Zone", Text = "Zone" });
                }
                else if (mList.Category == "Branch")
                {
                    CallCategoryList.Add(new SelectListItem { Value = "Branch", Text = "Branch" });
                }
                else if (mList.Category == "SubBranch")
                {
                    CallCategoryList.Add(new SelectListItem { Value = "SubBranch", Text = "SubBranch" });
                }
                else if (mList.Category == "Area")
                {
                    CallCategoryList.Add(new SelectListItem { Value = "Area", Text = "Area" });
                }



                mModel.CategoryList = CallCategoryList;

                if (mModel.Inactive)
                {
                    mModel.CategoryList = GetCategoryList("All");
                }
            }
            mModel.UsersMultiX = UsersList;
            var html = ViewHelper.RenderPartialView(this, "_ShowNodeDetails", mModel);
            return Json(new { Html = html, Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult AddChild(BranchVM mModel)
        {
            mModel.TfatBranch_gp_RCMDate = DateTime.Now;
            mModel.BusinessList = GetBusinessList();
            mModel.gp_VATGSTList = Getgp_VATGSTList();
            mModel.gp_AutoAccStyleList = Getgp_AutoAccStyleList();
            mModel.TypeList = GetTypeList();

            if (mModel.Code == null)
            {
                return Json(new { Message = "Item is Not Selected, Can't Add any Child..", Status = "Active" }, JsonRequestBehavior.AllowGet);
            }
            var mList = ctxTFAT.TfatBranch.Where(x => (x.Code == mModel.Code)).FirstOrDefault();

            if (mList != null)
            {
                List<SelectListItem> UsersList = new List<SelectListItem>();
                mModel.UsersMultiX = UsersList;

                mModel.Mode = "Add";
                var mGrp = ctxTFAT.TfatBranch.Where(x => x.Code == mList.Code).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mCompCode = ctxTFAT.TfatComp.Where(x => x.Code == mList.CompCode).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var mCountry = ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == mList.Country).Select(x => new { Name = x.Name, Code = x.Code.ToString() }).FirstOrDefault();
                var mState = ctxTFAT.TfatState.Where(x => x.Code.ToString() == mList.State).Select(x => new { Name = x.Name, Code = x.Code.ToString() }).FirstOrDefault();
                var mCity = ctxTFAT.TfatCity.Where(x => x.Code.ToString() == mList.City).Select(x => new { Name = x.Name, Code = x.Code.ToString() }).FirstOrDefault();
                var mCurrName = ctxTFAT.CurrencyMaster.Where(x => x.Code == mList.CurrName).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                if (mList.Code== "G00000")
                {
                    mModel.CategoryList = GetCategoryList(mList.Code);
                }
                else
                {
                    mModel.CategoryList = GetCategoryList(mList.Category);
                }
                mModel.Category = mList.Category;
                if (mList.Code != mList.Grp)
                {

                }
                mModel.ParentName = ctxTFAT.TfatBranch.Where(x => x.Code == mList.Code).Select(x => x.Name).FirstOrDefault();
                //mModel.ParentName= ctxTFAT.TfatBranch.Where(x => x.Code == mList.Grp && mList.Code != mList.Grp).Select(x => x.Name).FirstOrDefault();
                mModel.ParentCode = mModel.Code;
                mModel.Status = true;


                mModel.TfatBranch_Grp = mGrp != null ? mGrp.Code.ToString() : "";
                mModel.GrpName = mGrp != null ? mGrp.Name : "";
                mModel.TfatBranch_CompCode = mCompCode != null ? mCompCode.Code.ToString() : "";
                mModel.CompCodeName = mCompCode != null ? mCompCode.Name : "";
                mModel.TfatBranch_Country = mCountry != null ? mCountry.Code.ToString() : "";
                mModel.CountryName = mCountry != null ? mCountry.Name : "";
                mModel.TfatBranch_State = mState != null ? mState.Code.ToString() : "";
                mModel.StateName = mState != null ? mState.Name : "";
                mModel.TfatBranch_City = mCity != null ? mCity.Code.ToString() : "";
                mModel.CityName = mCity != null ? mCity.Name : "";
                mModel.TfatBranch_CurrName = mCurrName != null ? mCurrName.Code : 0;
                mModel.CurrNameName = mCurrName != null ? mCurrName.Name : "";

                mModel.TfatBranch_aAuthno = "";
                mModel.TfatBranch_Account = "";
                mModel.TfatBranch_aCstNo = "";
                mModel.TfatBranch_Addrl1 = "";
                mModel.TfatBranch_Addrl2 = "";
                mModel.TfatBranch_Addrl3 = "";
                mModel.TfatBranch_Addrl4 = "";
                mModel.TfatBranch_aFax = "";
                mModel.TfatBranch_aLstno = "";
                mModel.TfatBranch_aPin = "";
                mModel.TfatBranch_Business = "";
                mModel.TfatBranch_CINNo = "";

                //if (ctxTFAT.TfatBranch.ToList().Count() == 2 || ctxTFAT.TfatBranch.ToList().Count() == 1 || ctxTFAT.TfatBranch.ToList().Count() == 0)
                //{
                //    mModel.TfatBranch_Code = "000001";
                //}
                //else
                //{
                //    var NewCode = ctxTFAT.TfatBranch.Where(x=>x.Code!= "G00000").OrderByDescending(x => x.RECORDKEY).Take(1).Select(x => x.Code).FirstOrDefault();
                //    mModel.TfatBranch_Code = (Convert.ToInt32(NewCode) + 1).ToString("D6");
                //}
                if (ctxTFAT.TfatBranch.Where(x => x.Code != "G00000" && x.Code != "HO0000").ToList().Count() == 0)
                {
                    mModel.TfatBranch_Code = "000001";
                }
                else
                {
                    var NewCode = ctxTFAT.TfatBranch.OrderByDescending(x => x.RECORDKEY).Take(1).Select(x => x.Code).FirstOrDefault();
                    mModel.TfatBranch_Code = (Convert.ToInt32(NewCode) + 1).ToString("D6");
                }

                mModel.TfatBranch_CurrDec = 0;
                mModel.TfatBranch_Email = "";
                mModel.TfatBranch_Flag = "";
                mModel.TfatBranch_Fri = true;
                mModel.TfatBranch_gp_AllowDiscAP = false;
                mModel.TfatBranch_gp_AllowDiscAS = false;
                mModel.TfatBranch_gp_AllowDiscPP = false;
                mModel.TfatBranch_gp_AllowDiscPS = false;
                mModel.TfatBranch_gp_AllowEditDelete = false;
                mModel.TfatBranch_gp_AllowRateP = false;
                mModel.TfatBranch_gp_AllowRateS = false;
                mModel.TfatBranch_gp_AutoAccCode = true;
                mModel.TfatBranch_gp_AutoAccLength = 0;
                mModel.TfatBranch_gp_AutoAccStyle = 0;
                mModel.TfatBranch_gp_Batch = false;
                mModel.TfatBranch_gp_BillStock = false;
                mModel.TfatBranch_gp_BIN = false;
                mModel.TfatBranch_gp_CashLimit = false;
                mModel.TfatBranch_gp_CashLimitAmt = 0;
                mModel.TfatBranch_gp_CashLimitWarn = false;
                mModel.TfatBranch_gp_CLStock = false;
                mModel.TfatBranch_gp_DiscAP = false;
                mModel.TfatBranch_gp_DiscAS = false;
                mModel.TfatBranch_gp_DiscPP = false;
                mModel.TfatBranch_gp_DiscPS = false;
                mModel.TfatBranch_gp_EnableParty = false;
                mModel.TfatBranch_gp_GINQty = false;
                mModel.TfatBranch_gp_GSTStyle = 0;
                mModel.TfatBranch_gp_GSTSupply = false;
                mModel.TfatBranch_gp_Holiday1 = "";
                mModel.TfatBranch_gp_Holiday2 = "";
                mModel.TfatBranch_gp_LocWiseTax = false;
                mModel.TfatBranch_gp_MultiUnit = false;
                mModel.TfatBranch_gp_NegStock = false;
                mModel.TfatBranch_gp_NegStockAsOn = false;
                mModel.TfatBranch_gp_NegWarn = false;
                mModel.TfatBranch_gp_PostP = false;
                mModel.TfatBranch_gp_PSP = false;
                mModel.TfatBranch_gp_PurchPostTDS = false;
                mModel.TfatBranch_gp_QtnA = false;
                mModel.TfatBranch_gp_RCMDate = System.DateTime.Now;
                mModel.TfatBranch_gp_RoundVAT = 0;
                mModel.TfatBranch_gp_Serial = false;
                mModel.TfatBranch_gp_SEZChargeParty = false;
                mModel.TfatBranch_gp_SOPropagation = false;
                mModel.TfatBranch_gp_SPAdjForce = false;
                mModel.TfatBranch_gp_VatDecP = 0;
                mModel.TfatBranch_gp_VatDecS = 0;
                mModel.TfatBranch_gp_VATGST = "";

                mModel.TfatBranch_GSTNo = "";
                mModel.TfatBranch_LastBranch = false;
                mModel.TfatBranch_LastUpdated = false;
                mModel.TfatBranch_Licence2 = "";
                mModel.TfatBranch_LocationCode = 0;
                mModel.TfatBranch_LogIn = false;
                mModel.TfatBranch_Mon = true;
                mModel.TfatBranch_Name = "";
                mModel.TfatBranch_PanNo = "";
                mModel.TfatBranch_PCCode = 0;
                mModel.TfatBranch_PrintInfo = "";
                mModel.TfatBranch_ProxyServer = "";
                mModel.TfatBranch_Sat = true;
                mModel.TfatBranch_SMSCaption = "";
                mModel.TfatBranch_SMSPass = "";
                mModel.TfatBranch_SMSPrefix = false;
                mModel.TfatBranch_SMSURL = "";
                mModel.TfatBranch_SMSUserId = "";

                mModel.TfatBranch_Sun = true;
                mModel.TfatBranch_TDSAuthorise = "";
                mModel.TfatBranch_TDSCir = "";
                mModel.TfatBranch_TDSOffice = "";
                mModel.TfatBranch_TDSReg = "";
                mModel.TfatBranch_Tel1 = "";
                mModel.TfatBranch_Tel2 = "";
                mModel.TfatBranch_Tel3 = "";
                mModel.TfatBranch_Tel4 = "";
                mModel.TfatBranch_Thu = true;
                mModel.TfatBranch_TimeDiff = 0;
                mModel.TfatBranch_TINNumber = "";
                mModel.TfatBranch_Tue = true;
                mModel.TfatBranch_Users = "";
                mModel.TfatBranch_VATReg = "";
                mModel.TfatBranch_Wed = true;
                mModel.TfatBranch_www = "";
                mModel.VehicleActivity = "00:00";
                mModel.WorkingHoursFrom = "09:00";
                mModel.WorkingHoursTo = "21:00";
            }

            var html = ViewHelper.RenderPartialView(this, "_ShowNodeDetails", mModel);
            return Json(new { Html = html, Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CopyKM(BranchVM mModel)
        {
            if (mModel.Code == null)
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            //var KMlist = ctxTFAT.KilometerMaster.Where(x => x.FromBranch == mModel.Code).Select(x => new { x.ToBranch, x.KM }).ToList();
            string KMList = "";
            string BranchCodeList = "";
            string TimeList = "";
            //var list = ctxTFAT.KilometerMaster.Where(x => x.FromBranch == mModel.Code).ToList().OrderBy(x => x.RECORDKEY);
            //foreach (var item in list)
            //{
            //    KMList += item.KM + ",";
            //    BranchCodeList += item.ToBranch + ",";
            //}
            //if (!String.IsNullOrEmpty(KMList))
            //{
            //    KMList = KMList.Substring(0, KMList.Length - 1);
            //    BranchCodeList = BranchCodeList.Substring(0, BranchCodeList.Length - 1);
            //}

            return Json(new
            {
                TimeList = TimeList,
                KMList = KMList,
                BranchCodeList = BranchCodeList,
                Status = "Success",
                JsonRequestBehavior.AllowGet
            });
            //return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveData(BranchVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {

                    TfatBranch mobj = new TfatBranch();
                    bool mAdd = true;
                    if (ctxTFAT.TfatBranch.Where(x => (x.Code == mModel.TfatBranch_Code)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.TfatBranch.Where(x => (x.Code == mModel.TfatBranch_Code)).FirstOrDefault();
                        mAdd = false;
                    }
                    
                    //if (!String.IsNullOrEmpty(mModel.KMList))
                    //{
                    //    var GetKM = mModel.KMList.Split(',');
                    //    var BranchCodeList = mModel.BranchList.Split(',');
                    //    var TimeList = mModel.TimeList.Split(',');
                    //    foreach (var branhcode in BranchCodeList)
                    //    {
                    //        int index = Array.IndexOf(BranchCodeList, branhcode);
                    //        var kilomtert = ctxTFAT.KilometerMaster.Where(x => x.FromBranch == mModel.TfatBranch_Code && x.ToBranch == branhcode).FirstOrDefault();
                    //        if (kilomtert != null)
                    //        {
                    //            if (index >= 0)
                    //            {
                    //                kilomtert.KM = Convert.ToDouble(GetKM[index]);
                    //               // kilomtert.Time = TimeList[index];
                    //                ctxTFAT.Entry(kilomtert).State = EntityState.Modified;
                    //            }
                    //        }
                    //        else
                    //        {
                    //            KilometerMaster kilometerMaster = new KilometerMaster
                    //            {
                    //                FromBranch = mModel.TfatBranch_Code,
                    //                ToBranch = BranchCodeList[index],
                    //                KM = Convert.ToDouble(GetKM[index]),
                    //              //  Time = TimeList[index],
                    //                AUTHIDS = muserid,
                    //                AUTHORISE = mauthorise,
                    //                ENTEREDBY = muserid,
                    //                LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString())
                    //            };
                    //            ctxTFAT.KilometerMaster.Add(kilometerMaster);
                    //        }
                    //    }

                    //}

                    mobj.WorkTimeFrom = mModel.WorkingHoursFrom;
                    mobj.WorkTimeTo = mModel.WorkingHoursTo;
                    mobj.Addrl1 = mModel.TfatBranch_Addrl1;
                    mobj.Code = mModel.TfatBranch_Code;
                    mobj.VehicleWaitTime = mModel.VehicleActivity;
                    
                    mobj.gp_DiscAP = mModel.TfatBranch_gp_DiscAP;
                    mobj.Business = mModel.TfatBranch_Business;
                    mobj.SMSURL = mModel.TfatBranch_SMSURL;
                    mobj.Name = mModel.TfatBranch_Name;
                    mobj.Addrl3 = mModel.TfatBranch_Addrl3;
                    mobj.gp_AllowDiscAP = mModel.TfatBranch_gp_AllowDiscAP;
                    mobj.SMSUserId = mModel.TfatBranch_SMSUserId;
                    mobj.Addrl2 = mModel.TfatBranch_Addrl2;
                    mobj.aAuthno = mModel.TfatBranch_aAuthno;
                    mobj.Grp = mModel.TfatBranch_Grp;
                    mobj.gp_DiscAS = mModel.TfatBranch_gp_DiscAS;
                    mobj.gp_AllowDiscAS = mModel.TfatBranch_gp_AllowDiscAS;
                    mobj.Licence2 = mModel.TfatBranch_Licence2;
                    mobj.CompCode = mModel.TfatBranch_CompCode;
                    mobj.Addrl4 = mModel.TfatBranch_Addrl4;
                    mobj.gp_DiscPP = mModel.TfatBranch_gp_DiscPP;
                    mobj.aLstno = mModel.TfatBranch_aLstno;
                    mobj.LocationCode = mModel.TfatBranch_LocationCode;
                    mobj.SMSPass = mModel.TfatBranch_SMSPass;
                    mobj.Country = mModel.TfatBranch_Country;
                    mobj.gp_AllowDiscPP = mModel.TfatBranch_gp_AllowDiscPP;
                    mobj.State = mModel.TfatBranch_State;
                    //mobj.Users = mModel.TfatBranch_Users;
                    mobj.aCstNo = mModel.TfatBranch_aCstNo;
                    mobj.SMSCaption = mModel.TfatBranch_SMSCaption;
                    mobj.City = mModel.TfatBranch_City;
                    mobj.gp_DiscPS = mModel.TfatBranch_gp_DiscPS;
                    mobj.GSTNo = mModel.TfatBranch_GSTNo;
                    mobj.LogIn = mModel.TfatBranch_LogIn;
                    mobj.aPin = mModel.TfatBranch_aPin;
                    mobj.PanNo = mModel.TfatBranch_PanNo;
                    mobj.CurrName = mModel.TfatBranch_CurrName;
                    mobj.gp_AllowDiscPS = mModel.TfatBranch_gp_AllowDiscPS;
                    mobj.gp_AllowRateP = mModel.TfatBranch_gp_AllowRateP;
                    mobj.CINNo = mModel.TfatBranch_CINNo;
                    mobj.SMSPrefix = mModel.TfatBranch_SMSPrefix;
                    mobj.aFax = mModel.TfatBranch_aFax;
                    mobj.TimeDiff = mModel.TfatBranch_TimeDiff;
                    mobj.VATReg = mModel.TfatBranch_VATReg;
                    mobj.CurrDec = mModel.TfatBranch_CurrDec;
                    mobj.gp_AllowRateS = mModel.TfatBranch_gp_AllowRateS;
                    mobj.Account = mModel.TfatBranch_Account;
                    mobj.Tel1 = mModel.TfatBranch_Tel1;
                    mobj.gp_BIN = mModel.TfatBranch_gp_BIN;
                    mobj.gp_VATGST = mModel.TfatBranch_gp_VATGST == null ? "G" : mModel.TfatBranch_gp_VATGST;
                    mobj.gp_CLStock = mModel.TfatBranch_gp_CLStock;
                    mobj.Tel2 = mModel.TfatBranch_Tel2;
                    mobj.Tel3 = mModel.TfatBranch_Tel3;
                    mobj.gp_CashLimit = mModel.TfatBranch_gp_CashLimit;
                    mobj.TINNumber = mModel.TfatBranch_TINNumber;
                    mobj.Tel4 = mModel.TfatBranch_Tel4;
                    mobj.gp_CashLimitAmt = mModel.TfatBranch_gp_CashLimitAmt;
                    mobj.Sun = mModel.TfatBranch_Sun;
                    mobj.gp_CashLimitWarn = mModel.TfatBranch_gp_CashLimitWarn;
                    mobj.TDSReg = mModel.TfatBranch_TDSReg;
                    
                    mobj.www = mModel.TfatBranch_www;
                    mobj.TDSOffice = mModel.TfatBranch_TDSOffice;
                    mobj.Mon = mModel.TfatBranch_Mon;
                    mobj.TDSAuthorise = mModel.TfatBranch_TDSAuthorise;
                    mobj.gp_EnableParty = mModel.TfatBranch_gp_EnableParty;
                    mobj.PrintInfo = mModel.TfatBranch_PrintInfo;
                    mobj.TDSCir = mModel.TfatBranch_TDSCir;
                    mobj.gp_GINQty = mModel.TfatBranch_gp_GINQty;
                    mobj.gp_GSTStyle = mModel.TfatBranch_gp_GSTStyle;
                    mobj.Tue = mModel.TfatBranch_Tue;
                    mobj.gp_GSTSupply = mModel.TfatBranch_gp_GSTSupply;
                    mobj.Wed = mModel.TfatBranch_Wed;
                    mobj.gp_Holiday1 = mModel.TfatBranch_gp_Holiday1;
                    mobj.Thu = mModel.TfatBranch_Thu;
                    mobj.gp_Holiday2 = mModel.TfatBranch_gp_Holiday2;
                    mobj.gp_MultiUnit = mModel.TfatBranch_gp_MultiUnit;
                    mobj.Fri = mModel.TfatBranch_Fri;
                    mobj.Sat = mModel.TfatBranch_Sat;
                    mobj.gp_NegStock = mModel.TfatBranch_gp_NegStock;
                    mobj.gp_NegStockAsOn = mModel.TfatBranch_gp_NegStockAsOn;
                    mobj.gp_NegWarn = mModel.TfatBranch_gp_NegWarn;
                    mobj.gp_PostP = mModel.TfatBranch_gp_PostP;
                    mobj.gp_PurchPostTDS = mModel.TfatBranch_gp_PurchPostTDS;
                    mobj.gp_PSP = mModel.TfatBranch_gp_PSP;
                    mobj.gp_RCMDate = ConvertDDMMYYTOYYMMDD(mModel.TfatBranch_gp_RCMDateVM);
                    mobj.gp_SEZChargeParty = mModel.TfatBranch_gp_SEZChargeParty;
                    mobj.gp_VatDecP = mModel.TfatBranch_gp_VatDecP;
                    mobj.gp_RoundVAT = mModel.TfatBranch_gp_RoundVAT;
                    mobj.gp_VatDecS = mModel.TfatBranch_gp_VatDecS;
                    mobj.gp_SPAdjForce = mModel.TfatBranch_gp_SPAdjForce;
                    mobj.gp_Serial = mModel.TfatBranch_gp_Serial;
                    mobj.gp_Batch = mModel.TfatBranch_gp_Batch;
                    mobj.gp_AutoAccCode = mModel.TfatBranch_gp_AutoAccCode;
                    mobj.gp_AutoAccStyle = mModel.TfatBranch_gp_AutoAccStyle;
                    mobj.gp_AutoAccLength = mModel.TfatBranch_gp_AutoAccLength;
                    // iX9: default values for the fields not used @Form
                    mobj.Flag = "";
                    mobj.gp_AllowEditDelete = false;
                    mobj.gp_BillStock = false;
                    mobj.gp_LocWiseTax = false;
                    mobj.gp_QtnA = false;
                    mobj.gp_SOPropagation = false;
                    mobj.LastBranch = false;
                    mobj.LastUpdated = false;
                    mobj.PCCode = 0;
                    mobj.ProxyServer = "";
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.gp_SONoDupl = mModel.TfatBranch_gp_SONoDupl;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    mobj.gp_DuplicateItemName = mModel.TfatBranch_DuplicateItemName;
                    mobj.gp_AddonBased = mModel.gp_AddonBased;
                    mobj.gp_AddonBasedDescr = mModel.gp_AddonBasedDescr;
                    mobj.gp_AddonSepCode = mModel.gp_AddonSepCode;
                    mobj.gp_AddonSepName = mModel.gp_AddonSepName;
                    mobj.gp_ItemAutoCode = mModel.gp_ItemAutoCode;
                    mobj.gp_ItemAutoDescr = mModel.gp_ItemAutoDescr;
                    mobj.gp_ItemCodeStyle = mModel.gp_ItemCodeStyle;
                    mobj.gp_ItemDescrStyle = mModel.gp_ItemDescrStyle;
                    mobj.gp_ItemPrefixName = mModel.gp_ItemPrefixName;
                    mobj.gp_Length = mModel.gp_Length;
                    mobj.gp_OrdIncludeRet = mModel.gp_OrdIncludeRet;
                    mobj.gp_CLStockAddORder = mModel.gp_CLStockAddOrder;
                    mobj.BranchType = mModel.Type;
                    mobj.Category = mModel.Category;
                    mobj.Status = mModel.Status;

                    mobj.Email = mModel.TfatBranch_Email;
                    mobj.BCCTo = mModel.BCCTo;
                    mobj.CCTo = mModel.CCTo;
                    mobj.SMTPServer = mModel.SMTPServer;
                    mobj.SMTPUser = mModel.SMTPUser;
                    mobj.SMTPPassword = mModel.SMTPPassword;
                    mobj.SMTPPort = mModel.SMTPPort;
                    mobj.BranchMail = mModel.BranchMail;
                    mobj.LocalMail = mModel.LocalMail;

                    if (mAdd == true)
                    {
                        mobj.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                        mModel.Mode = "Add";
                        ctxTFAT.TfatBranch.Add(mobj);

                        
                    }
                    else
                    {
                        mModel.Mode = "Edit";
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;

                    }
                    ctxTFAT.SaveChanges();


                    
                    ctxTFAT.SaveChanges();
                    //SaveBRanchChild(mModel.TfatBranch_Code);
                    //mnewrecordkey = mobj.RECORDKEY;
                    string mNewCode = "";
                    mNewCode = mobj.Code;
                    transaction.Commit();
                    transaction.Dispose();
                    if (mModel.TfatBranch_gp_VATGST == null || mModel.TfatBranch_gp_VATGST == "G")
                    {
                        ctxTFAT.Database.ExecuteSqlCommand("Update TaxMaster Set Locked=0 where VATGST<>0");
                        ctxTFAT.Database.ExecuteSqlCommand("Update TaxMaster Set Locked=-1 where VATGST=0");
                    }
                    else
                    {
                        ctxTFAT.Database.ExecuteSqlCommand("Update TaxMaster Set Locked=0 where VATGST=0");
                        ctxTFAT.Database.ExecuteSqlCommand("Update TaxMaster Set Locked=-1 where VATGST<>0");
                    }
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.Code, DateTime.Now, 0, mobj.Code, "Save Branch", "B");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "ChangePlantBUDetails" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "ChangePlantBUDetails" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "ChangePlantBUDetails" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "AccountGroups" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteAccountGroups(BranchVM mModel)
        {
            if (mModel.Code == null)
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            // iX9: Check for Active Master MasterGroups
            var Delete = true;
            string mactivestring = "";

            var Default = ctxTFAT.TfatBranch.Where(x => (x.Code == mModel.Code)).FirstOrDefault();
            if (Default!=null)
            {
                if (Default.Code.Trim()=="HO0000"|| Default.Code.Trim() == "G00000")
                {
                    mactivestring = mactivestring + "\nCant  Allow To Delete This Is Default Value:: " + Default.Name;
                }
            }


            var mactive1 = ctxTFAT.TfatBranch.Where(x => (x.Grp == mModel.Code)).Select(x => x.Name).FirstOrDefault();
            if (mactive1 != null) { mactivestring = mactivestring + mactive1; }

            var mactive2 = ctxTFAT.LRMaster.Where(x => x.Source == mModel.Code || x.Dest == mModel.Code || x.BillBran==mModel.Code).FirstOrDefault();
            if (mactive2!=null)
            {
                mactivestring = mactivestring + "\nLrNo: " + mactive2.LrNo;
            }
            var mactive3 = ctxTFAT.LCMaster.Where(x => x.FromBranch == mModel.Code || x.ToBranch == mModel.Code).FirstOrDefault();
            if (mactive3 != null)
            {
                mactivestring = mactivestring + "\nLcNo: " + mactive3.LCno;
            }
            var mactive4 = ctxTFAT.Ledger.Where(x => x.Branch == mModel.Code).FirstOrDefault();
            if (mactive4 != null)
            {
                mactivestring = mactivestring + "\nLedger: " + mactive4.Srl;
            }
            //var mactive4 = ctxTFAT.Master.Where(x => x.AppBranch.Contains(mModel.Code)).FirstOrDefault();
            //if (mactive4 != null)
            //{
            //    mactivestring = mactivestring + "\nName: " + mactive4.Name +" In Master Account.";
            //}
            //var mactive5 = ctxTFAT.VehicleMaster.Where(x => x.Branch.Contains(mModel.Code)).FirstOrDefault();
            //if (mactive5 != null)
            //{
            //    mactivestring = mactivestring + "\nName: " + mactive5.TruckNo + " In Vehicle Master.";
            //}
            //var mactive6 = ctxTFAT.DriverMaster.Where(x => x.Branch == mModel.Code).FirstOrDefault();
            //if (mactive6 != null)
            //{
            //    mactivestring = mactivestring + "\nName: " + mactive6.Name + " In Driver Master.";
            //}


            var mactive9 = ctxTFAT.KilometerMaster.Where(x => x.FromBranch == mModel.Code).FirstOrDefault();
            if (mactive9 != null)
            {
                mactivestring = mactivestring + "\nKilometer Master.";
            }

            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "Active Item, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var mList = ctxTFAT.TfatBranch.Where(x => (x.Code == mModel.Code)).FirstOrDefault();
                    ctxTFAT.TfatBranch.Remove(mList);

                    var ml = ctxTFAT.KilometerMasterRef.Where(x => x.ToBranch == mModel.Code).ToList();
                    ctxTFAT.KilometerMasterRef.RemoveRange(ml);

                    

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.Code, DateTime.Now, 0, mModel.Code, "Delete Branch", "B");

                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new
                    {
                        Message = ex1.InnerException.Message,
                        Status = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new
                    {
                        Message = ex.InnerException.Message,
                        Status = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new
            {
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }
        public string GetNextCode()
        {
            int number;
            var countBranch = ctxTFAT.TfatBranch.ToList().Count();
            if (countBranch <= 1)
            {
                number = 0;
            }
            else
            {
                var NewNumber = ctxTFAT.TfatBranch.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).Take(1).FirstOrDefault();
                number = Convert.ToInt32(NewNumber);
            }

            return (++number).ToString();
        }

        #region Save Branch Child

        List<TfatBranch> GEtArea = new List<TfatBranch>();
        
        public List<TfatBranch> GetChild(string Code)
        {
            string Child = "";
            List<TfatBranch> tfatBranches = GetBranch(Code);

            return tfatBranches;
        }

        public string SaveChild(NRecursiveObject item)
        {
            foreach (var item1 in item.children)
            {
                var branch1 = ctxTFAT.TfatBranch.Where(x => x.Code == item1.id).FirstOrDefault();
                if (GEtArea.Where(x => x.Code == item1.id).FirstOrDefault() == null)
                {
                    GEtArea.Add(branch1);
                }
                if (item1.children.Count > 0)
                {
                    SaveChild(item1);
                }
            }
            return "";
        }


        public string GetParent(string Parent)
        {
            string ParentList = "";

            TfatBranch tfat = ctxTFAT.TfatBranch.Where(x => x.Code == Parent).FirstOrDefault();
            ParentList += tfat.Code+",";
            if (tfat.Grp!=tfat.Code)
            {
                string ParentCode = GetParent(tfat.Grp);
                ParentList += ParentCode + ",";
            }
            return ParentList.Substring(0,ParentList.Length-1);
        }




        #endregion



        #region ChiLDSNoode
        public static List<NRecursiveObject> FillRecursive2(List<NFlatObject> flatObjects, string parentId)
        {
            bool mSelected = false;
            List<NRecursiveObject> recursiveObjects = new List<NRecursiveObject>();
            foreach (var item in flatObjects.Where(x => x.ParentId.Equals(parentId)))
            {
                mSelected = item.isSelected;
                recursiveObjects.Add(new NRecursiveObject
                {
                    data = item.data,
                    id = item.Id,
                    attr = new NFlatTreeAttribute { id = item.Id.ToString(), selected = item.isSelected },
                    children = FillRecursive2(flatObjects, item.Id)
                });
            }
            return recursiveObjects;
        }
        public List<TfatBranch> GetBranch(string BRanchCode)
        {
            var mTreeList = ctxTFAT.TfatBranch.Where(x=>x.Status==true).Select(x => new { x.Name, x.Grp, x.Code }).ToList();
            List<NFlatObject> flatObjects2 = new List<NFlatObject>();
            GEtArea = new List<TfatBranch>();
            for (int n = 0; n < mTreeList.Count; n++)
            {
                NFlatObject abc = new NFlatObject();
                abc.data = mTreeList[n].Name;
                abc.Id = mTreeList[n].Code;
                if (mTreeList[n].Code == mTreeList[n].Grp)
                {
                    abc.ParentId = "0";
                }
                else
                {
                    abc.ParentId = mTreeList[n].Grp;
                }
                flatObjects2.Add(abc);
            }
            var recursiveObjects = FillRecursive2(flatObjects2, BRanchCode);

            var Currentbranch = ctxTFAT.TfatBranch.Where(x => x.Code == BRanchCode).FirstOrDefault();
            if (GEtArea.Where(x => x.Code == BRanchCode).FirstOrDefault() == null)
            {
                GEtArea.Add(Currentbranch);
            }

            foreach (var item in recursiveObjects)
            {
                var branch = ctxTFAT.TfatBranch.Where(x => x.Code == item.id).FirstOrDefault();
                if (GEtArea.Where(x => x.Code == item.id).FirstOrDefault() == null)
                {
                    GEtArea.Add(branch);
                }
                if (item.children.Count > 0)
                {
                    SaveChild(item);
                }
            }
            return GEtArea;
        }
        #endregion
    }
}