using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using ALT_ERP3.Models;
using NFlatObject = ALT_ERP3.Areas.Logistics.Models.NFlatObject;
using System.Data.SqlClient;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class UserProfileController : BaseController
    {
        ////private IBusinessCommon mBuss = new BusinessCommon();
        private int mlocation = 100001;
        private static string moptioncode = "";
        private static string mmodule = "";
        //private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        //private string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        //private string muserid = System.Web.HttpContext.Current.Session["UserId"].ToString();
        //private string mperiod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        private int mnewrecordkey = 0;
        private static string mauthorise = "A00";
        private static string mdocument = "";

        #region GetLists

        public List<SelectListItem> GetGridRowsList()
        {
            List<SelectListItem> CallGridRowsList = new List<SelectListItem>();
            CallGridRowsList.Add(new SelectListItem { Value = "0", Text = "10" });
            CallGridRowsList.Add(new SelectListItem { Value = "1", Text = "15" });
            CallGridRowsList.Add(new SelectListItem { Value = "2", Text = "20" });
            CallGridRowsList.Add(new SelectListItem { Value = "3", Text = "50" });
            CallGridRowsList.Add(new SelectListItem { Value = "4", Text = "100" });
            CallGridRowsList.Add(new SelectListItem { Value = "5", Text = "500" });
            
            return CallGridRowsList;
        }

        public JsonResult AutoCompleteUserRole(string term)
        {
            return Json((from m in ctxTFAT.UserRoles
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteDept(string term)
        {
            return Json((from m in ctxTFAT.Dept
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteLanguage(string term)
        {
            return Json((from m in ctxTFAT.Language
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteAssistant(string term)
        {
            return Json((from m in ctxTFAT.TfatPass
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        private List<SelectListItem> PopulateUsers(string User)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM TfatPass where  Code<>'Super' and Code<>'"+User+"' order by Recordkey ";
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

        public JsonResult SetDefaultBranch(string term,string UserId , string Mode)
        {
            List<SelectListItem> branch = new List<SelectListItem>();

            UserId = UserId.ToUpper();
            if (UserId == "SUPER" || Mode == "Add")
            {

                List<TfatBranch> BranchList = new List<TfatBranch>();
                if (String.IsNullOrEmpty(term))
                {
                    BranchList = ctxTFAT.TfatBranch.Where(x => x.Status == true && (x.Category != "Area" && x.Code != "G00000")).OrderBy(x => x.Code).ToList();
                }
                else
                {
                    BranchList = ctxTFAT.TfatBranch.Where(x => x.Name.Contains(term) && x.Status == true && (x.Category != "Area" && x.Code != "G00000")).OrderBy(x => x.Code).ToList();

                }
                foreach (var item in BranchList)
                {
                    var Prefix = "";
                    if (item.Category == "Branch")
                    {
                        Prefix = " - " + item.Category.Substring(0, 1);
                    }
                    else if (item.Category == "0")
                    {
                        Prefix = " - HO";
                    }
                    else
                    {
                        Prefix = " - " + item.Category.Substring(0, 2);
                    }
                    branch.Add(new SelectListItem { Text = item.Name + Prefix, Value = item.Code });
                    //branch.Add(new SelectListItem { Text = item.Name, Value = item.Code });
                }
            }
            else
            {
                List<TfatBranch> BranchList = new List<TfatBranch>();
                if (String.IsNullOrEmpty(term))
                {
                    BranchList = ctxTFAT.TfatBranch.Where(x => x.Status == true && x.Users.ToUpper().Contains(UserId) && (x.Category != "Area" && x.Code != "G00000")).ToList();
                }
                else
                {
                    BranchList = ctxTFAT.TfatBranch.Where(x => x.Name.Contains(term) &&  x.Status == true && x.Users.ToUpper().Contains(UserId) && (x.Category != "Area" && x.Code != "G00000")).ToList();
                }
                foreach (var item in BranchList)
                {
                    var Prefix = "";
                    if (item.Category == "Branch")
                    {
                        Prefix = " - " + item.Category.Substring(0, 1);
                    }
                    else if (item.Category == "0")
                    {
                        Prefix = " - HO";
                    }
                    else
                    {
                        Prefix = " - " + item.Category.Substring(0, 2);
                    }
                    branch.Add(new SelectListItem { Text = item.Name + Prefix, Value = item.Code });
                    //branch.Add(new SelectListItem { Text = item.Name, Value = item.Code });
                }
            }
            var Modified = branch.Select(x => new
            {
                Code = x.Value,
                Name = x.Text
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);

        }

        #endregion GetLists

        #region TreeView
        public string TreeView(string Mode, string Document)
        {
            string BranchCode = "";
            string[] BranchArray = new string[100];
            if (Mode == "Add")
            {
                BranchCode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
            }
            else
            {
                
                var Branchlist = ctxTFAT.TfatPass.Where(x => x.Code == Document).Select(x => x.AppBranch).FirstOrDefault();
                BranchArray = Branchlist.ToString().Split(',');
            }

            var mTreeList = ctxTFAT.TfatBranch.Where(x => x.Category != "Area" && x.Status==true).Select(x => new { x.Name, x.Grp, x.Code, x.Category }).ToList();

            List<NFlatObject> flatObjects2 = new List<NFlatObject>();
            for (int n = 0; n < mTreeList.Count; n++)
            {
                string alias = "";
                NFlatObject abc = new NFlatObject();
                if (mTreeList[n].Category == "Zone")
                {
                    alias = mTreeList[n].Name + "-" + "Z";
                }
                else if (mTreeList[n].Category == "Branch")
                {
                    alias = mTreeList[n].Name + "-" + "B";
                }
                else if (mTreeList[n].Category == "SubBranch")
                {
                    alias = mTreeList[n].Name + "-" + "SB";
                }
                else
                {
                    alias = mTreeList[n].Name + "-" + "HO";
                }

                abc.data = alias;
                abc.Id = mTreeList[n].Code;
                if (Mode == "Add")
                {
                    if (BranchCode == abc.Id)
                    {
                        abc.isSelected = true;
                    }
                }
                else
                {
                    if (BranchArray.Contains(abc.Id))
                    {
                        abc.isSelected = true;
                    }
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
        public string CheckUncheckTree(string Check)
        {
            var mTreeList = ctxTFAT.TfatBranch.Where(x => x.Category != "Area" && x.Status==true).Select(x => new { x.Name, x.Grp, x.Code, x.Category }).ToList();
            string alias = "";
            List<NFlatObject> flatObjects2 = new List<NFlatObject>();
            for (int n = 0; n < mTreeList.Count; n++)
            {
                NFlatObject abc = new NFlatObject();
                if (mTreeList[n].Category == "Zone")
                {
                    alias = mTreeList[n].Name + "-" + "Z";
                }
                else if (mTreeList[n].Category == "Branch")
                {
                    alias = mTreeList[n].Name + "-" + "B";
                }
                else if (mTreeList[n].Category == "SubBranch")
                {
                    alias = mTreeList[n].Name + "-" + "SB";
                }
                else
                {
                    alias = mTreeList[n].Name + "-" + "HO";
                }
                abc.data = alias;
                abc.Id = mTreeList[n].Code;
                if (Check == "Check")
                {
                    abc.isSelected = true;
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
        public static List<NRecursiveObject> FillRecursive1(List<NFlatObject> flatObjects, string parentId)
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
                    children = FillRecursive1(flatObjects, item.Id)
                });
            }
            return recursiveObjects;
        }
        #endregion

        public List<ConsignmentQueryPanel> ConsignmentPanelList()
        {
            List<ConsignmentQueryPanel> list = new List<ConsignmentQueryPanel>();

            ConsignmentQueryPanel panel = new ConsignmentQueryPanel();
            panel.Name = "Consignmet Details";
            panel.Code = "CD";
            list.Add(panel);

            panel = new ConsignmentQueryPanel();
            panel.Name = "Consignmet Stock";
            panel.Code = "CS";
            list.Add(panel);

            panel = new ConsignmentQueryPanel();
            panel.Name = "Dispatch Details";
            panel.Code = "DS";
            list.Add(panel);

            panel = new ConsignmentQueryPanel();
            panel.Name = "Delivery Details";
            panel.Code = "DD";
            list.Add(panel);

            panel = new ConsignmentQueryPanel();
            panel.Name = "POD Details";
            panel.Code = "PD";
            list.Add(panel);

            panel = new ConsignmentQueryPanel();
            panel.Name = "Bill Details";
            panel.Code = "BD";
            list.Add(panel);

            panel = new ConsignmentQueryPanel();
            panel.Name = "Cash Sale Details";
            panel.Code = "CSD";
            list.Add(panel);

            panel = new ConsignmentQueryPanel();
            panel.Name = "Expenses Details";
            panel.Code = "ED";
            list.Add(panel);

            return list;
        }
        public List<FreightMemoQueryPanel> FreightMemoPanelList()
        {
            List<FreightMemoQueryPanel> list = new List<FreightMemoQueryPanel>();

            FreightMemoQueryPanel panel = new FreightMemoQueryPanel();
            panel.Name = "Freight Memo Details";
            panel.Code = "FMD";
            list.Add(panel);

            panel = new FreightMemoQueryPanel();
            panel.Name = "Dispatch Details";
            panel.Code = "DS";
            list.Add(panel);

            panel = new FreightMemoQueryPanel();
            panel.Name = "Payment Details";
            panel.Code = "PD";
            list.Add(panel);

            panel = new FreightMemoQueryPanel();
            panel.Name = "Expenses Details";
            panel.Code = "ED";
            list.Add(panel);

            return list;
        }
        
        // GET: Logistics/UserProfile
        public ActionResult Index(UserProfileVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "U");
            mdocument = mModel.Document;
            mModel.TfatPass_Expiry = DateTime.Now;
            mModel.GridRowsList = GetGridRowsList();

            List<SelectListItem> AppBranchList = new List<SelectListItem>();
            var AppBranchResultX = ctxTFAT.TfatBranch.Select(x => new { Code = x.Code, Name = x.Name }).ToList().Distinct();
            foreach (var AppBranchitem in AppBranchResultX)
            {
                AppBranchList.Add(new SelectListItem { Text = AppBranchitem.Name, Value = AppBranchitem.Code.ToString() });
            }
            mModel.AppBranchMultiX = AppBranchList;
            mModel.TfatPass_Code = mModel.Document;
            mModel.ConsignmentPanels = ConsignmentPanelList();
            mModel.FreightMemoPanels = FreightMemoPanelList();


            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.TfatPass.Where(x => (x.Code.Trim().ToLower() == mModel.Document.Trim().ToLower())).FirstOrDefault();
                if (mList != null)
                {
                    mModel.DefaultLoginBranch = mList.DefaultLoginBranch;
                    mModel.DefaultLoginBranchN =ctxTFAT.TfatBranch.Where(x=>x.Code== mList.DefaultLoginBranch).Select(x=>x.Name).FirstOrDefault();

                    mModel.UserList = PopulateUsers(mList.Code);
                    var mUserRole = ctxTFAT.UserRoles.Where(x => x.Code == mList.UserRole).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mDept = ctxTFAT.Dept.Where(x => x.Code == mList.Dept).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mLanguage = ctxTFAT.Language.Where(x => x.Code == mList.Language).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mAssistant = ctxTFAT.TfatPass.Where(x => x.Code == mList.Assistant).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    mModel.TfatPass_UserRole = mUserRole != null ? mUserRole.Code.ToString() : "";
                    mModel.ChildUsers = mList.UserList == null?"": mList.UserList.ToString();
                    mModel.UserRoleName = mUserRole != null ? mUserRole.Name : "";
                    mModel.TfatPass_Dept = mDept != null ? mDept.Code : 0;
                    mModel.DeptName = mDept != null ? mDept.Name : "";
                    mModel.TfatPass_Language = mLanguage != null ? mLanguage.Code : 0;
                    mModel.LanguageName = mLanguage != null ? mLanguage.Name : "";
                    mModel.TfatPass_Assistant = mAssistant != null ? mAssistant.Code.ToString() : "";
                    mModel.AssistantName = mAssistant != null ? mAssistant.Name : "";
                    mModel.TfatPass_Mobile = mList.Mobile;
                    mModel.TfatPass_Code = mList.Code;
                    mModel.TfatPass_AppBranch = mList.AppBranch;
                    mModel.TfatPass_GridRows = mList.GridRows.Value;
                    mModel.TfatPass_CorpID = mList.CorpID;
                    mModel.TfatPass_Email = mList.Email;
                    mModel.TfatPass_Name = mList.Name;
                    mModel.TfatPass_MinItem = mList.MinItem != null ? mList.MinItem.Value : 0;
                    mModel.TfatPass_PrintControl = mList.PrintControl;
                    mModel.TfatPass_IsHead = mList.IsHead;
                    mModel.TfatPass_MinAcc = mList.MinAcc != null ? mList.MinAcc.Value : 0;
                    mModel.TfatPass_Telephone = mList.Telephone;
                    mModel.TfatPass_Hide = mList.Hide;
                    mModel.TfatPass_Locked = mList.Locked;
                    mModel.TfatPass_DontChangePassword = mList.DontChangePassword;
                    mModel.TfatPass_MinOthers = mList.MinOthers != null ? mList.MinOthers.Value : 0;
                    mModel.TfatPass_DashboardActive = mList.DashboardActive;
                    mModel.TfatPass_LogIns = mList.LogIns;
                    mModel.TfatPass_POPServer = mList.POPServer;
                    mModel.TfatPass_Expiry = mList.Expiry != null ? mList.Expiry.Value : DateTime.Now;
                    mModel.TfatPass_PassWords = mList.PassWords;
                    mModel.TfatPass_SMTPServer = mList.SMTPServer;
                    mModel.TfatPass_PassRead = mList.PassRead;
                    mModel.TfatPass_DailyPassword = mList.DailyPassword;
                    mModel.TfatPass_SMTPUser = mList.SMTPUser;
                    mModel.TfatPass_RecMsg = mList.RecMsg;
                    mModel.TfatPass_Sun = mList.Sun;
                    mModel.TfatPass_RecSMS = mList.RecSMS;
                    mModel.TfatPass_SMTPPassword = mList.SMTPPassword;
                    mModel.TfatPass_Mon = mList.Mon;
                    mModel.TfatPass_Tue = mList.Tue;
                    mModel.TfatPass_Photograph = mList.Photograph;
                    mModel.TfatPass_SMTPPort = mList.SMTPPort != null ? mList.SMTPPort.Value : 0;
                    mModel.TfatPass_Wed = mList.Wed;
                    mModel.TfatPass_Thu = mList.Thu;
                    mModel.TfatPass_Signature = mList.Signature;
                    mModel.TfatPass_Fri = mList.Fri;
                    mModel.TfatPass_Sat = mList.Sat;
                    mModel.TfatPass_StrictIP = mList.StrictIP;

                    if (String.IsNullOrEmpty(mList.ConsignQryBranch))
                    {
                        mModel.ConsignmenyQueryBranch = true;
                    }
                    else
                    {
                        if (mList.ConsignQryBranch=="TR")
                        {
                            mModel.ConsignmenyQueryBranch = true;
                        }
                        else
                        {
                            mModel.ConsignmenyQueryBranch = false;
                        }
                    }

                    if (String.IsNullOrEmpty(mList.FreightMemoQryBranch))
                    {
                        mModel.FreightMemoQueryBranch = true;
                    }
                    else
                    {
                        if (mList.FreightMemoQryBranch == "TR")
                        {
                            mModel.FreightMemoQueryBranch = true;
                        }
                        else
                        {
                            mModel.FreightMemoQueryBranch = false;
                        }
                    }

                    if (!String.IsNullOrEmpty(mList.ConsignQryPanel))
                    {
                        var List = mList.ConsignQryPanel.Split('^').ToList();
                        mModel.ConsignmentPanels.Where(x=>List.Contains(x.Code)).ToList().ForEach(z => z.Check = true);
                    }
                    if (!String.IsNullOrEmpty(mList.FreightMemoQryPanel))
                    {
                        var List = mList.FreightMemoQryPanel.Split('^').ToList();
                        mModel.FreightMemoPanels.Where(x=>List.Contains(x.Code)).ToList().ForEach(z => z.Check = true);
                    }

                }
            }
            else
            {
                mModel.UserList = PopulateUsers("");
                mModel.TfatPass_AppBranch = "";
                mModel.TfatPass_Assistant = "";
                mModel.TfatPass_BCCTo = "";
                mModel.TfatPass_CCTo = "";
                mModel.TfatPass_Code = "";
                mModel.TfatPass_ColScheme = 0;
                mModel.TfatPass_CorpID = "";
                mModel.TfatPass_DailyPassword = false;
                mModel.TfatPass_DashboardActive = false;
                mModel.TfatPass_Dept = 0;
                mModel.TfatPass_DontChangePassword = false;
                mModel.TfatPass_Email = "";
                mModel.TfatPass_EmailClient = "";
                mModel.TfatPass_Expiry = System.DateTime.Now;
                mModel.TfatPass_Fri = true;
                mModel.TfatPass_GridRows = 0;
                mModel.TfatPass_Hide = false;
                mModel.TfatPass_IsHead = false;
                mModel.TfatPass_Language = 0;
                mModel.TfatPass_LastBranch = "";
                mModel.TfatPass_LastCompany = "";
                mModel.TfatPass_LastLogin = System.DateTime.Now;
                mModel.TfatPass_LastStartDt = System.DateTime.Now;
                mModel.TfatPass_LicenseID = "";
                mModel.TfatPass_Locked = false;
                mModel.TfatPass_LogIns = "";
                mModel.TfatPass_MinAcc = 0;
                mModel.TfatPass_MinItem = 0;
                mModel.TfatPass_MinOthers = 0;
                mModel.TfatPass_Mobile = "";
                mModel.TfatPass_Mon = true;
                mModel.TfatPass_MsgDays = 0;
                mModel.TfatPass_Name = "";
                mModel.TfatPass_PassRead = "";
                mModel.TfatPass_PassWords = "";
                mModel.TfatPass_Photograph = "";
                mModel.TfatPass_POPServer = "";
                mModel.TfatPass_PrintControl = false;
                mModel.TfatPass_RecMsg = false;
                mModel.TfatPass_RecSMS = false;
                mModel.TfatPass_Remember = false;
                mModel.TfatPass_Sat = true;
                mModel.TfatPass_Signature = "";
                mModel.TfatPass_SMTPPassword = "";
                mModel.TfatPass_SMTPPort = 0;
                mModel.TfatPass_SMTPRoute = false;
                mModel.TfatPass_SMTPServer = "";
                mModel.TfatPass_SMTPUser = "";
                mModel.TfatPass_StrictIP = "";
                mModel.TfatPass_Sun = true;
                mModel.TfatPass_Telephone = "";
                mModel.TfatPass_Thu = true;
                mModel.TfatPass_Tue = true;
                mModel.TfatPass_UserRole = "";
                mModel.TfatPass_Wed = true;
                mModel.ConsignmenyQueryBranch = true;
                mModel.FreightMemoQueryBranch = true;

            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(UserProfileVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteUserProfile(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.Document, DateTime.Now, 0, mModel.Document, "Delete User", "U");
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    TfatPass mobj = new TfatPass();
                    bool mAdd = true;
                    if (ctxTFAT.TfatPass.Where(x => (x.Code == mModel.TfatPass_Code)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.TfatPass.Where(x => (x.Code == mModel.TfatPass_Code)).FirstOrDefault();
                        mAdd = false;
                    }
                    mobj.Mobile = mModel.TfatPass_Mobile;
                    mobj.UserList = mModel.ChildUsers;
                    mobj.Code = mModel.TfatPass_Code;
                    mobj.AppBranch = mModel.TfatPass_AppBranch;
                    mobj.GridRows = mModel.TfatPass_GridRows;
                    mobj.CorpID = mModel.TfatPass_CorpID;
                    mobj.Email = mModel.TfatPass_Email;
                    mobj.Name = mModel.TfatPass_Name;
                    mobj.MinItem = mModel.TfatPass_MinItem;
                    mobj.PrintControl = mModel.TfatPass_PrintControl;
                    mobj.IsHead = mModel.TfatPass_IsHead;
                    mobj.MinAcc = mModel.TfatPass_MinAcc;
                    mobj.Telephone = mModel.TfatPass_Telephone;
                    mobj.Hide = mModel.TfatPass_Hide;
                    mobj.Locked = mModel.TfatPass_Locked;
                    mobj.DontChangePassword = mModel.TfatPass_DontChangePassword;
                    mobj.MinOthers = mModel.TfatPass_MinOthers;
                    mobj.UserRole = mModel.TfatPass_UserRole;
                    mobj.DashboardActive = mModel.TfatPass_DashboardActive;
                    mobj.LogIns = mModel.TfatPass_LogIns;
                    mobj.POPServer = mModel.TfatPass_POPServer;
                    mobj.Expiry = ConvertDDMMYYTOYYMMDD(mModel.TfatPass_ExpiryVM);
                    mobj.PassWords = mModel.TfatPass_PassWords;
                    mobj.SMTPServer = mModel.TfatPass_SMTPServer;
                    mobj.Dept = mModel.TfatPass_Dept;
                    mobj.PassRead = mModel.TfatPass_PassRead;
                    mobj.Language = mModel.TfatPass_Language;
                    mobj.DailyPassword = mModel.TfatPass_DailyPassword;
                    mobj.SMTPUser = mModel.TfatPass_SMTPUser;
                    mobj.RecMsg = mModel.TfatPass_RecMsg;
                    mobj.Sun = mModel.TfatPass_Sun;
                    mobj.RecSMS = mModel.TfatPass_RecSMS;
                    mobj.SMTPPassword = mModel.TfatPass_SMTPPassword;
                    mobj.Assistant = mModel.TfatPass_Assistant;
                    mobj.Mon = mModel.TfatPass_Mon;
                    mobj.Tue = mModel.TfatPass_Tue;
                    mobj.Photograph = mModel.TfatPass_Photograph;
                    mobj.SMTPPort = mModel.TfatPass_SMTPPort;
                    mobj.Wed = mModel.TfatPass_Wed;
                    mobj.Thu = mModel.TfatPass_Thu;
                    mobj.Signature = mModel.TfatPass_Signature;
                    mobj.Fri = mModel.TfatPass_Fri;
                    mobj.Sat = mModel.TfatPass_Sat;
                    mobj.StrictIP = mModel.TfatPass_StrictIP;
                    // iX9: default values for the fields not used @Form
                    mobj.BCCTo = "";
                    mobj.CCTo = "";
                    mobj.ColScheme = 0;
                    mobj.EmailClient = "";
                    mobj.LastBranch = "";
                    mobj.LastCompany = "";
                    mobj.LastLogin = System.DateTime.Now;
                    mobj.LastStartDt = System.DateTime.Now;
                    mobj.LicenseID = "";
                    mobj.MsgDays = 0;
                    mobj.Remember = false;
                    mobj.SMTPRoute = false;

                    mobj.DefaultLoginBranch = mModel.DefaultLoginBranch;
                    mobj.ConsignQryBranch = mModel.ConsignmenyQueryBranch==true?"TR":"FA";
                    mobj.FreightMemoQryBranch = mModel.FreightMemoQueryBranch == true ? "TR" : "FA";

                    string ConsignCode = "", FreightCode = "";
                    if (mModel.ConsignmentPanels!=null)
                    {
                        foreach (var item in mModel.ConsignmentPanels)
                        {
                            ConsignCode += item.Code + "^";
                        }
                    }
                    if (mModel.FreightMemoPanels!=null)
                    {
                        foreach (var item in mModel.FreightMemoPanels)
                        {
                            FreightCode += item.Code + "^";
                        }
                    }

                    if (String.IsNullOrEmpty(ConsignCode))
                    {
                        mobj.ConsignQryPanel = "";
                    }
                    else
                    {
                        ConsignCode = ConsignCode.Substring(0, ConsignCode.Length - 1);
                        mobj.ConsignQryPanel = ConsignCode;
                    }

                    if (String.IsNullOrEmpty(FreightCode))
                    {
                        mobj.FreightMemoQryPanel = "";
                    }
                    else
                    {
                        FreightCode = FreightCode.Substring(0, FreightCode.Length - 1);
                        mobj.FreightMemoQryPanel = FreightCode;
                    }

                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mAdd == true)
                    {
                        ctxTFAT.TfatPass.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }

                    var AppBranchOfUser = mModel.TfatPass_AppBranch.Split(',');
                    foreach (var item in AppBranchOfUser)
                    {
                        TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code.ToLower() == item.ToLower()).FirstOrDefault();
                        if (String.IsNullOrEmpty(tfatBranch.Users))
                        {
                            tfatBranch.Users = mModel.TfatPass_Code;
                        }
                        else
                        {
                            var UserList = tfatBranch.Users.Split(',');
                            if (Array.IndexOf(UserList, mModel.TfatPass_Code) < 0)
                            {
                                tfatBranch.Users = tfatBranch.Users + "," + mModel.TfatPass_Code;
                            }
                        }
                        
                        ctxTFAT.Entry(tfatBranch).State = EntityState.Modified;

                    }
                    List<TfatBranch> tfatBranches = ctxTFAT.TfatBranch.Where(x => !AppBranchOfUser.Contains(x.Code)).ToList();
                    foreach (var item in tfatBranches)
                    {
                        string NewUsers = "";
                        if (String.IsNullOrEmpty(item.Users))
                        {
                            item.Users = NewUsers;
                        }
                        else
                        {
                            var UserList = item.Users.Split(',');
                            foreach (var item1 in UserList)
                            {
                                if (item1.ToLower().Trim() != mModel.TfatPass_Code.ToLower().Trim())
                                {
                                    NewUsers += item1 + ",";
                                }
                            }
                            item.Users = NewUsers.Substring(0, NewUsers.Length - 1);
                        }
                        
                        ctxTFAT.Entry(item).State = EntityState.Modified;
                    }

                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    string mNewCode = "";
                    mNewCode = mobj.Code;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mNewCode, DateTime.Now, 0, mNewCode, "Save User", "U");

                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "UserProfile" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "UserProfile" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "UserProfile" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "UserProfile" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteUserProfile(UserProfileVM mModel)
        {
            if (mModel.TfatPass_Code == null || mModel.TfatPass_Code == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            // iX9: Check for Active Master TfatPass
            string mactivestring = "";
            var mactive1 = ctxTFAT.TfatUserAudit.Where(x => (x.UserID == mModel.TfatPass_Code)).Select(x => x.UserID).FirstOrDefault();
            if (mactive1 != null) { mactivestring = mactivestring + "\nTfatUserAudit: " + mactive1; }
            var mactive2 = ctxTFAT.UserRights.Where(x => (x.Code == mModel.TfatPass_Code)).Select(x => x.Code).FirstOrDefault();
            if (mactive2 != null) { mactivestring = mactivestring + "\nUserRights: " + mactive2; }
            var mactive3 = ctxTFAT.UserRightsTrx.Where(x => (x.Code == mModel.TfatPass_Code)).Select(x => x.Code).FirstOrDefault();
            if (mactive3 != null) { mactivestring = mactivestring + "\nUserRightsTrx: " + mactive3; }
            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "Active Item, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var mList = ctxTFAT.TfatPass.Where(x => (x.Code == mModel.TfatPass_Code)).FirstOrDefault();
            ctxTFAT.TfatPass.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}