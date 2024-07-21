using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class UserDashboardController : BaseController
    {
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;
        private new string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private new string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        public new string muserid = System.Web.HttpContext.Current.Session["UserId"].ToString();
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        private static string mbasegr = "";

        private List<SelectListItem> PopulateTypes()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem
            {
                Text = "Outstanding",
                Value = "Outstanding"
            });
            items.Add(new SelectListItem
            {
                Text = "Outstanding Payable",
                Value = "Payable"
            });
            items.Add(new SelectListItem
            {
                Text = "Consignment Booking",
                Value = "ConsignmetBook"
            });
            items.Add(new SelectListItem
            {
                Text = "Godown Consignment Stock",
                Value = "ConsignmetStock"
            });
            items.Add(new SelectListItem
            {
                Text = "Transit Consignment Stock",
                Value = "ConsignmeTRNStock"
            });
            items.Add(new SelectListItem
            {
                Text = "Un-Bill Consignment",
                Value = "UnBillConsignmet"
            });
            items.Add(new SelectListItem
            {
                Text = "Top 10 Expenses",
                Value = "TopExpenses"
            });
            items.Add(new SelectListItem
            {
                Text = "Top 10 Customers",
                Value = "TopCustomers"
            });
            items.Add(new SelectListItem
            {
                Text = "Top 10 Group-Customers",
                Value = "TopGroupCustomers"
            });
            items.Add(new SelectListItem
            {
                Text = "Top 10 Vendors",
                Value = "TopVendors"
            });
            items.Add(new SelectListItem
            {
                Text = "Vehicle Status",
                Value = "VehicleStatus"
            });
            items.Add(new SelectListItem
            {
                Text = "Driver Status",
                Value = "DriverStatus"
            });
            items.Add(new SelectListItem
            {
                Text = "Vehicle Location",
                Value = "VehicleLocation"
            });
            items.Add(new SelectListItem
            {
                Text = "Driver Trip & Balance",
                Value = "DriverTripBalance"
            });
            items.Add(new SelectListItem
            {
                Text = "Vehicle Trip Details",
                Value = "VehicleTripDetails"
            });
            items.Add(new SelectListItem
            {
                Text = "Vehicle Exp Due",
                Value = "VehicleExpDue"
            });
            items.Add(new SelectListItem
            {
                Text = "Eway Bill Details",
                Value = "EwayBillDetails"
            });
            return items;
        }

        private List<SelectListItem> PopulateVehicles()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            var list = ctxTFAT.VehicleMaster.Where(x => x.Acitve == true && x.Code != "99998" && x.Code != "99999").ToList();
            foreach (var item in list)
            {
                string PostFix = item.TruckStatus == "100000" ? " - A" : " - O";
                items.Add(new SelectListItem
                {
                    Text = item.TruckNo.ToString().ToUpper() + PostFix,
                    Value = item.Code.ToString()
                });
            }
            return items;
        }

        private List<SelectListItem> PopulateDrivers()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            var list = ctxTFAT.DriverMaster.Where(x => x.Status == true && x.Code != "99999").ToList();
            foreach (var item in list)
            {
                items.Add(new SelectListItem
                {
                    Text = item.Name.ToString().ToUpper(),
                    Value = item.Code.ToString()
                });
            }
            return items;
        }

        private List<SelectListItem> PopulateUsers()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            var list = ctxTFAT.TfatPass.Where(x => x.Locked == false).ToList();
            foreach (var item in list)
            {
                items.Add(new SelectListItem
                {
                    Text = item.Name.ToString(),
                    Value = item.Code.ToString()
                });
            }
            return items;
        }

        private List<SelectListItem> PopulateObjectType()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem
            {
                Text = "Pie Chart",
                Value = "PC"
            });
            items.Add(new SelectListItem
            {
                Text = "Bar Graph (Horizontal)",
                Value = "BGH"
            });
            items.Add(new SelectListItem
            {
                Text = "Bar Graph (Vertical)",
                Value = "BGV"
            });
            return items;
        }

        private List<SelectListItem> PopulateBranchesOnly()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM TfatBranch where Category<>'Area'  order by Recordkey ";
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

        private List<SelectListItem> PopulateCustomers()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM CustomerMaster where Hide='false' order by Name ";
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

        private List<SelectListItem> PopulateDebtors()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM Master where Hide='false' and (BaseGr='D' or BaseGr='U') order by Name ";
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

        private List<SelectListItem> PopulateVehicleExpDues()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM Master where Hide='false' and Code in ('000100326','000100653','000100736','000100781','000100788','000100789','000100811','000100951','000100953','000101135') order by Name ";
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

        private List<SelectListItem> PopulateVehicleMaster()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM Master where OthPostType like '%V%' order by Name ";
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

        // GET: Logistics/UserDashboard
        public ActionResult Index(UserDashboardVM Model)
        {
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "NA");
            GetAllMenu(Session["ModuleName"].ToString());
            if (Model.Document == "" || Model.Document == null)
            {
                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                Model.ToDate = DateTime.Today.ToShortDateString();
                //System.Web.HttpContext.Current.Session["LastDate"].ToString();
            }
            if (Model.FromDate == null)
            {
                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                Model.ToDate = DateTime.Today.ToShortDateString();
            }

            ViewBag.ViewDataId = Model.ViewDataId;
            msubcodeof = Model.ViewDataId;
            //Model.AccountName = NameofAccount(Model.Document);
            mbasegr = Model.MainType;
            ViewBag.Header = Model.Header;
            ViewBag.Controller = Model.Controller;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;


            Model.BranchsOnly = PopulateBranchesOnly();

            List<UserDashboardVM> mLeftList = new List<UserDashboardVM>();
            var mlist = ctxTFAT.TfatPass.Where(x => x.Locked == false).Select(x => x).OrderBy(x => x.Name).ToList();
            foreach (var i in mlist)
            {
                mLeftList.Add(new UserDashboardVM()
                {
                    User = i.Code,
                    UserL = i.Name,
                });
            }
            Model.mLeftList = mLeftList;
            return View(Model);
        }

        public ActionResult ClickLeftGrid(UserDashboardVM mModel)
        {
            Session["Sidebarlist"] = null;
            Session["DeleteSidebarlist"] = null;
            mModel.BranchsOnly = PopulateBranchesOnly();
            var Reports = PopulateTypes();
            var Userslist = ctxTFAT.ActiveSideBarObjects.Where(x => x.Users == mModel.User)
                .Select(x => new
                {
                    ID = x.ID,
                    Code = x.Code,
                    Name = x.Name,
                    BranchL = x.Para10,
                    ZoomURL = x.ZoomURL,
                    Status = x.Status,
                    FromDate = x.Para8,
                    ToDate = x.Para9,
                    Master = x.Para11,
                    Para1 = x.Para1,
                    Para2 = x.Para2,
                    Para3 = x.Para3,
                    Para4 = x.Para4,
                    Para5 = x.Para5,
                    Para6 = x.Para6,
                    Para7 = x.Para7,
                    DisplayOrder = x.DisplayOrder ?? 0,
                    EmptyBranch=x.Para12,
                }).ToList();
            List<UserDashboardVM> mlist = new List<UserDashboardVM>();
            int I = 1;
            foreach (var i in Userslist)
            {
                mlist.Add(new UserDashboardVM()
                {
                    Srno = I++,
                    Code = i.Code,
                    ID = i.ID,
                    Name = i.Name,
                    BranchL = i.BranchL,
                    Branch = i.BranchL,
                    ZoomURL = i.ZoomURL,
                    Status = i.Status,
                    FromDate = i.FromDate,
                    ToDate = i.ToDate,
                    Master = i.Master,
                    Para1 = i.Para1,
                    Para2 = i.Para2,
                    Para3 = i.Para3,
                    Para4 = i.Para4,
                    Para5 = i.Para5,
                    Para6 = i.Para6,
                    Para7 = i.Para7,
                    User = mModel.User,
                    DisplayOrder = i.DisplayOrder,
                    EmptyBranchEwayBill=i.EmptyBranch=="T"?true:false,
                });
            }

            Session.Add("Sidebarlist", mlist);
            var html = ViewHelper.RenderPartialView(this, "TransactionMessageRulesPartial", new UserDashboardVM { BranchsOnly = mModel.BranchsOnly, mRightList = mlist.OrderBy(x => x.DisplayOrder).ToList(), SelectedUserName = mModel.User });
            return Json(new { mRightList = mlist, SelectedUserName = mModel.User, Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index1(UserDashboardVM Model)
        {
            Model.Codes = PopulateTypes();
            Model.Users = PopulateUsers();
            Model.Vehicles = PopulateVehicles();
            Model.Drivers = PopulateDrivers();
            Model.ObjectTypes = PopulateObjectType();
            Model.BranchsOnly = PopulateBranchesOnly();
            Model.Customers = PopulateCustomers();
            Model.Masters = PopulateDebtors();
            Model.VehicleExpDues = PopulateVehicleExpDues();
            Model.VehicleMasters = PopulateVehicleMaster();
            if ((Model.Mode == "Edit") || (Model.Mode == "View") || (Model.Mode == "Delete"))
            {
                var mobj = ctxTFAT.ActiveSideBarObjects.Where(x => x.ID == Model.Document).FirstOrDefault();

                Model.Code = mobj.Code;
                Model.Status = mobj.Status;
                Model.ZoomURL = mobj.ZoomURL;
                Model.FromDate = mobj.Para8;
                Model.ToDate = mobj.Para9;
                Model.Name = mobj.Name;
                Model.ObjectType = mobj.ObjectType;
                Model.User = mobj.Users;
                Model.Branch = mobj.Para10;
                Model.Master = mobj.Para11;
                Model.Para1 = mobj.Para1;
                Model.Para2 = mobj.Para2;
                Model.Para3 = mobj.Para3;
                Model.Para4 = mobj.Para4;
                Model.Para5 = mobj.Para5;
                Model.Para6 = mobj.Para6;
                Model.Para7 = mobj.Para7;

            }
            else
            {
                Model.Status = true;
                Model.ZoomURL = true;
            }
            return View(Model);
        }

        #region ADD LEDGER ITEM
        public ActionResult GetSideBar(UserDashboardVM Model)
        {
            Model.Codes = PopulateTypes();
            Model.Users = PopulateUsers();
            Model.Vehicles = PopulateVehicles();
            Model.Drivers = PopulateDrivers();
            Model.ObjectTypes = PopulateObjectType();
            Model.BranchsOnly = PopulateBranchesOnly();
            Model.Customers = PopulateCustomers();
            Model.Masters = PopulateDebtors();
            Model.VehicleExpDues = PopulateVehicleExpDues();
            Model.VehicleMasters = PopulateVehicleMaster();
            if (Model.GridMode == "Edit")
            {
                var result = (List<UserDashboardVM>)Session["Sidebarlist"];
                var result1 = result.Where(x => x.Srno == Model.Srno);
                foreach (var item in result1)
                {
                    Model.Code = item.Code;
                    Model.Status = item.Status;
                    Model.ZoomURL = item.ZoomURL;
                    Model.FromDate = item.FromDate;
                    Model.ToDate = item.ToDate;
                    Model.Name = item.Name;
                    Model.ObjectType = item.ObjectType;
                    Model.User = item.User;
                    Model.Branch = item.Branch;
                    Model.Master = item.Master;
                    Model.Para1 = item.Para1;
                    Model.Para2 = item.Para2;
                    Model.Para3 = item.Para3;
                    Model.Para4 = item.Para4;
                    Model.Para5 = item.Para5;
                    Model.Para6 = item.Para6;
                    Model.Para7 = item.Para7;
                    Model.DisplayOrder = item.DisplayOrder;
                    Model.EmptyBranchEwayBill = item.EmptyBranchEwayBill;

                }
                Model.GridMode = "Edit";
            }
            else
            {
                Model.Status = true;
                Model.ZoomURL = true;

                var Code = ctxTFAT.ActiveSideBarObjects.Where(x => x.Users == Model.User).Select(x => x.Code).ToList();
                foreach (var item in Code)
                {
                    var Item = Model.Codes.Where(x => x.Value == item).FirstOrDefault();
                    if (Item != null)
                    {
                        Model.Codes.Remove(Item);
                    }
                }
            }

            var html = ViewHelper.RenderPartialView(this, "UserDashboardView", Model);
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult AddSideBar(UserDashboardVM Model)
        {

            Model.BranchsOnly = PopulateBranchesOnly();
            if (Model.GridMode == "Add")
            {
                var List = ctxTFAT.ActiveSideBarObjects.Where(x => x.Code.Trim() == Model.Code).Select(x => x.Users).ToList();
                var CurrentUsers = Model.User.Split(',').ToList();
                string AlreadyUseUserCurrentDashboard = "";
                foreach (var item in List)
                {
                    var OldUsers = item.Split(',').ToList();
                    var CommonList = CurrentUsers.Intersect(OldUsers);
                    foreach (var CommonUser in CommonList)
                    {
                        AlreadyUseUserCurrentDashboard += CommonUser + ",";
                    }
                }
                if (!String.IsNullOrEmpty(AlreadyUseUserCurrentDashboard))
                {
                    return Json(new { Status = "Error", Message = "Following Users Already Used " + Model.Name + ".\nSo Please Remove " + AlreadyUseUserCurrentDashboard + "  From The Users List...!" }, JsonRequestBehavior.AllowGet);
                }

                List<UserDashboardVM> objledgerdetail = new List<UserDashboardVM>();
                if (Session["Sidebarlist"] != null)
                {
                    objledgerdetail = (List<UserDashboardVM>)Session["Sidebarlist"];
                }
                if (objledgerdetail != null)
                {
                    var Item = objledgerdetail.Where(x => x.DisplayOrder == Model.DisplayOrder && x.Srno != Model.Srno).FirstOrDefault();
                    if (Item != null)
                    {
                        return Json(new { Status = "Error", Message = "Following Display Order No Already Used To " + Item.Name + ".\nSo Please Change Display Order NO...!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                objledgerdetail.Add(new UserDashboardVM()
                {
                    Srno = objledgerdetail.Count() + 1,
                    Code = Model.Code,
                    Status = Model.Status,
                    ZoomURL = Model.ZoomURL,
                    FromDate = Model.FromDate,
                    ToDate = Model.ToDate,
                    Name = Model.Name,
                    ObjectType = Model.ObjectType,
                    User = Model.User,
                    Branch = Model.Branch,
                    Master = Model.Master,
                    Para1 = Model.Para1,
                    Para2 = Model.Para2,
                    Para3 = Model.Para3,
                    Para4 = Model.Para4,
                    Para5 = Model.Para5,
                    Para6 = Model.Para6,
                    Para7 = Model.Para7,
                    DisplayOrder = Model.DisplayOrder,
                    EmptyBranchEwayBill = Model.EmptyBranchEwayBill,
            });

                Session.Add("Sidebarlist", objledgerdetail);
                var html = ViewHelper.RenderPartialView(this, "TransactionMessageRulesPartial", new UserDashboardVM { BranchsOnly = Model.BranchsOnly, mRightList = objledgerdetail.OrderBy(x => x.DisplayOrder).ToList(), SelectedUserName = Model.User });
                var jsonResult = Json(new { mRightList = objledgerdetail, SelectedUserName = Model.User, Html = html }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
            else
            {
                var objledgerdetail = (List<UserDashboardVM>)Session["Sidebarlist"];
                if (objledgerdetail != null)
                {
                    var Item = objledgerdetail.Where(x => x.DisplayOrder == Model.DisplayOrder && x.Srno != Model.Srno).FirstOrDefault();
                    if (Item != null)
                    {
                        return Json(new { Status = "Error", Message = "Following Display Order No Already Used To " + Item.Name + ".\nSo Please Change Display Order NO...!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                foreach (var item in objledgerdetail.Where(x => x.Srno == Model.Srno))
                {
                    item.Code = Model.Code;
                    item.Status = Model.Status;
                    item.ZoomURL = Model.ZoomURL;
                    item.FromDate = Model.FromDate;
                    item.ToDate = Model.ToDate;
                    item.Name = Model.Name;
                    item.ObjectType = Model.ObjectType;
                    item.User = Model.User;
                    item.Branch = Model.Branch;
                    item.Master = Model.Master;
                    item.Para1 = Model.Para1;
                    item.Para2 = Model.Para2;
                    item.Para3 = Model.Para3;
                    item.Para4 = Model.Para4;
                    item.Para5 = Model.Para5;
                    item.Para6 = Model.Para6;
                    item.Para7 = Model.Para7;
                    item.DisplayOrder = Model.DisplayOrder;
                    item.EmptyBranchEwayBill = Model.EmptyBranchEwayBill;

                }
                Session.Add("Sidebarlist", objledgerdetail);

                var html = ViewHelper.RenderPartialView(this, "TransactionMessageRulesPartial", new UserDashboardVM { BranchsOnly = Model.BranchsOnly, mRightList = objledgerdetail.OrderBy(x => x.DisplayOrder).ToList(), SelectedUserName = Model.User });
                var jsonResult = Json(new { mRightList = objledgerdetail, SelectedUserName = Model.User, Html = html }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
        }

        [HttpPost]
        public ActionResult DeleteSidebar(UserDashboardVM Model)
        {
            Model.BranchsOnly = PopulateBranchesOnly();
            var result = (List<UserDashboardVM>)Session["Sidebarlist"];

            var result1 = (List<UserDashboardVM>)Session["DeleteSidebarlist"];
            if (result1 == null)
            {
                result1 = new List<UserDashboardVM>();
            }

            var resultdelete = result.Where(x => x.Srno == Model.Srno).Select(x => x).ToList();
            result1.AddRange(resultdelete);
            var result2 = result.Where(x => x.Srno != Model.Srno).Select(x => x).ToList();
            Session.Add("Sidebarlist", result2);
            Session.Add("DeleteSidebarlist", result1);

            var html = ViewHelper.RenderPartialView(this, "TransactionMessageRulesPartial", new UserDashboardVM { BranchsOnly = Model.BranchsOnly, mRightList = result2.OrderBy(x => x.DisplayOrder).ToList(), SelectedUserName = Model.User });
            var jsonResult = Json(new { mRightList = result2, SelectedUserName = Model.User, Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        [HttpPost]
        public ActionResult BackTolist(UserDashboardVM Model)
        {
            Model.BranchsOnly = PopulateBranchesOnly();
            var result = (List<UserDashboardVM>)Session["Sidebarlist"];

            Session.Add("Sidebarlist", result);

            var html = ViewHelper.RenderPartialView(this, "TransactionMessageRulesPartial", new UserDashboardVM { BranchsOnly = Model.BranchsOnly, mRightList = result.OrderBy(x => x.DisplayOrder).ToList(), SelectedUserName = Model.User });
            var jsonResult = Json(new { mRightList = result, SelectedUserName = Model.User, Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        [HttpPost]
        public ActionResult SaveGridData(UserDashboardVM Model)
        {
            Model.BranchsOnly = PopulateBranchesOnly();
            var result = (List<UserDashboardVM>)Session["Sidebarlist"];
            if (result != null)
            {
                if (Model.mRightList != null)
                {
                    foreach (var item in Model.mRightList)
                    {
                        var COUNTlIST = result.Where(x => x.DisplayOrder == item.DisplayOrder).ToList();
                        if (COUNTlIST != null)
                        {
                            if (COUNTlIST.Count() > 1)
                            {
                                return Json(new { Status = "Error", Message = "Display No Not Allow Duplicate .\nSo Please Check Display NO...!" }, JsonRequestBehavior.AllowGet);

                            }
                        }
                        var UpdateItem = result.Where(x => x.Srno == item.Srno).FirstOrDefault();
                        if (UpdateItem != null)
                        {
                            if (String.IsNullOrEmpty(item.DisplayOrder.ToString()))
                            {
                                return Json(new { Status = "Error", Message = "Display No Not Allow Empty " + item.Name + ".\nSo Please Check Display NO...!" }, JsonRequestBehavior.AllowGet);
                            }



                            UpdateItem.DisplayOrder = item.DisplayOrder;
                            UpdateItem.Status = item.Status;
                            UpdateItem.ZoomURL = item.ZoomURL;
                        }
                    }
                }

            }


            Session.Add("Sidebarlist", result);

            var html = ViewHelper.RenderPartialView(this, "TransactionMessageRulesPartial", new UserDashboardVM { BranchsOnly = Model.BranchsOnly, mRightList = result.OrderBy(x => x.DisplayOrder).ToList(), SelectedUserName = Model.User });
            var jsonResult = Json(new { mRightList = result, SelectedUserName = Model.User, Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }


        #endregion



        //Return View Base On Code
        public ActionResult GetReportParameter(UserDashboardVM Model)
        {
            Model.ObjectTypes = PopulateObjectType();
            Model.BranchsOnly = PopulateBranchesOnly();
            Model.Customers = PopulateCustomers();
            Model.Masters = PopulateDebtors();
            Model.Users = PopulateUsers();
            Model.Vehicles = PopulateVehicles();
            Model.Drivers = PopulateDrivers();
            Model.VehicleExpDues = PopulateVehicleExpDues();
            Model.VehicleMasters = PopulateVehicleMaster();
            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            Model.DisplayOrder = 1;
            var result = (List<UserDashboardVM>)Session["Sidebarlist"];
            if (result!=null)
            {
                var Count = result.OrderByDescending(x => x.DisplayOrder).Select(x=>x.DisplayOrder).FirstOrDefault();
                Model.DisplayOrder = Count + 1;
            }
            if (Model.Code == "Outstanding" || Model.Code == "Payable")
            {
                Model.Para2 = "30";
                Model.Para3 = "60";
                Model.Para4 = "90";
                Model.Para5 = "120";
                Model.Para6 = "150";
                Model.Para7 = "180";
            }
            else if (Model.Code == "UnBillConsignmet" || Model.Code == "ConsignmetStock" || Model.Code == "ConsignmeTRNStock")
            {
                Model.Para1 = "30";
                Model.Para2 = "60";
                Model.Para3 = "90";
                Model.Para4 = "120";
            }
            else if (Model.Code == "VehicleExpDue")
            {
                Model.Para3 = "0";
                Model.Para4 = "5";
                Model.Para5 = "15";
                Model.Para6 = "30";
            }
            else if (Model.Code == "EwayBillDetails")
            {
                Model.EmptyBranchEwayBill = true;
                Model.Para1 = "Active";
                Model.Para2 = "Todays Expired";
                Model.Para3 = "Tomorrow Expired";
                Model.Para4 = "Already Expired";
            }
            if (Model.Code == "ConsignmetBook")
            {
                Model.Para1 = "T";
            }
            var html = ViewHelper.RenderPartialView(this, "_ParameterDesign", Model);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        #region SaveData
        public ActionResult SaveData(UserDashboardVM mModel)
        {
            string NewSrl = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var NewID = getNewCode();
                    var result = (List<UserDashboardVM>)Session["Sidebarlist"];

                    if (mModel.mRightList != null)
                    {
                        foreach (var item in mModel.mRightList)
                        {
                            var COUNTlIST = result.Where(x => x.DisplayOrder == item.DisplayOrder).ToList();
                            if (COUNTlIST != null)
                            {
                                if (COUNTlIST.Count() > 1)
                                {
                                    return Json(new { Status = "Error", Message = "Display No Not Allow Duplicate .\nSo Please Check Display NO...!" }, JsonRequestBehavior.AllowGet);

                                }
                            }
                            var UpdateItem = result.Where(x => x.Srno == item.Srno).FirstOrDefault();
                            if (UpdateItem != null)
                            {
                                if (String.IsNullOrEmpty(item.DisplayOrder.ToString()))
                                {
                                    return Json(new { Status = "Error", Message = "Display No Not Allow Empty " + item.Name + ".\nSo Please Check Display NO...!" }, JsonRequestBehavior.AllowGet);
                                }
                                UpdateItem.DisplayOrder = item.DisplayOrder;
                                UpdateItem.Status = item.Status;
                                UpdateItem.ZoomURL = item.ZoomURL;
                            }
                        }
                    }



                    foreach (var item in result)
                    {
                        ActiveSideBarObjects mobj = new ActiveSideBarObjects();
                        bool mAdd = true;
                        if (ctxTFAT.ActiveSideBarObjects.Where(x => x.ID == item.ID).FirstOrDefault() != null)
                        {
                            mobj = ctxTFAT.ActiveSideBarObjects.Where(x => x.ID == item.ID).FirstOrDefault();
                            mAdd = false;
                            NewSrl = item.ID;
                        }
                        mobj.Code = item.Code;
                        mobj.Status = item.Status;
                        mobj.ZoomURL = item.ZoomURL;
                        mobj.Para8 = item.FromDate;//From Date
                        mobj.Para9 = item.ToDate;//To Date
                        mobj.Name = item.Name;
                        mobj.ObjectType = item.ObjectType;
                        mobj.Users = item.User;
                        mobj.Para10 = item.Branch;//Branch
                        mobj.Para11 = item.Master;//Master
                        mobj.Para1 = item.Para1;
                        mobj.Para2 = item.Para2;
                        mobj.Para3 = item.Para3;
                        mobj.Para4 = item.Para4;
                        mobj.Para5 = item.Para5;
                        mobj.Para6 = item.Para6;
                        mobj.Para7 = item.Para7;
                        mobj.DisplayOrder = item.DisplayOrder;
                        mobj.Para12 = item.EmptyBranchEwayBill == true ? "T" : "F";//Branch
                        // iX9: Save default values to Std fields
                        mobj.AUTHIDS = muserid;
                        mobj.AUTHORISE = mauthorise;
                        mobj.ENTEREDBY = muserid;
                        mobj.LASTUPDATEDATE = System.DateTime.Now;
                        if (mAdd == true)
                        {
                            mobj.ID = NewID;
                            NewID = (Convert.ToInt32(NewID) + 1).ToString();
                            NewSrl = mobj.ID;
                            ctxTFAT.ActiveSideBarObjects.Add(mobj);
                        }
                        else
                        {
                            ctxTFAT.Entry(mobj).State = EntityState.Modified;
                        }

                    }

                    var result1 = (List<UserDashboardVM>)Session["DeleteSidebarlist"];
                    if (result1 != null)
                    {
                        foreach (var item in result1)
                        {
                            var DeleteSideBar = ctxTFAT.ActiveSideBarObjects.Where(x => x.ID == item.ID).FirstOrDefault();
                            if (DeleteSideBar != null)
                            {
                                ctxTFAT.ActiveSideBarObjects.Remove(DeleteSideBar);
                            }
                        }
                    }


                    ctxTFAT.SaveChanges();
                    string mNewCode = "";
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2), DateTime.Now, 0, mModel.User, "Save SideBar", "U");
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
            return Json(new { Status = "Success", id = "UserProfile", NewSrl = NewSrl }, JsonRequestBehavior.AllowGet);
        }

        private string getNewCode()
        {

            string Code = ctxTFAT.ActiveSideBarObjects.OrderByDescending(x => x.ID).Select(x => x.ID).Take(1).FirstOrDefault();
            if (String.IsNullOrEmpty(Code))
            {

                return "100000";
            }
            else
            {
                return (Convert.ToInt32(Code) + 1).ToString("D6");
            }
        }

        public ActionResult DeleteUserProfile(UserDashboardVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var mList = ctxTFAT.ActiveSideBarObjects.Where(x => (x.ID == mModel.Document)).FirstOrDefault();
            ctxTFAT.ActiveSideBarObjects.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}