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
using Common;
using System.Data.SqlClient;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class ChangeCompanyDetailsController : BaseController
    {
         
        ////private IBusinessCommon mBuss = new BusinessCommon();
        private int mlocation = 100001;
        //private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        //private string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        //private string muserid = System.Web.HttpContext.Current.Session["UserId"].ToString();
        //private string mperiod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        private int mnewrecordkey = 0;
        private static string mauthorise = "A00";
        private static string mdocument = "";

        #region GetLists
        public List<SelectListItem> GetBusinessList()
        {
            List<SelectListItem> CallBusinessList = new List<SelectListItem>();
            CallBusinessList.Add(new SelectListItem { Value = "0", Text = "0-Trader" });
            CallBusinessList.Add(new SelectListItem { Value = "1", Text = "1-Manufacturer" });
            CallBusinessList.Add(new SelectListItem { Value = "2", Text = "2-Trader/Manufacturer" });
            CallBusinessList.Add(new SelectListItem { Value = "3", Text = "3-Others" });
            CallBusinessList.Add(new SelectListItem { Value = "4", Text = "4-Chartered Accountant" });
            return CallBusinessList;
        }
        public List<SelectListItem> GetConstList()
        {
            List<SelectListItem> CallConstList = new List<SelectListItem>();
            CallConstList.Add(new SelectListItem { Value = "0", Text = "0-Individual" });
            CallConstList.Add(new SelectListItem { Value = "1", Text = "1-Proprietorship" });
            CallConstList.Add(new SelectListItem { Value = "2", Text = "2-Partnership" });
            CallConstList.Add(new SelectListItem { Value = "3", Text = "3-Private Limited" });
            CallConstList.Add(new SelectListItem { Value = "4", Text = "4-Public Limited" });
            CallConstList.Add(new SelectListItem { Value = "5", Text = "5-Institutions" });
            CallConstList.Add(new SelectListItem { Value = "6", Text = "6-Others" });
            return CallConstList;
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

        private List<SelectListItem> PopulateTruckNo()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code,TruckNo,TruckStatus FROM VehicleMaster where Acitve = 'true'   order by Recordkey ";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            string Flag = sdr["TruckStatus"].ToString().Trim() == "100000" ? " - A" : " - O";
                            items.Add(new SelectListItem
                            {
                                Text = sdr["TruckNo"].ToString() + Flag,
                                Value = sdr["Code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code,TruckNo FROM HireVehicleMaster where Acitve = 'true'   order by Recordkey ";
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
                                Text = sdr["TruckNo"].ToString() + " - H",
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

        // GET: Logistics/ChangeCompanyDetails
        public ActionResult Index(ChangeCompanyDetailsVM mModel)
        {
            Session["CompanyTrackingList"] = null;
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "C");
            mModel.BusinessList = GetBusinessList();
            mModel.ConstList = GetConstList();
            mModel.VehicleList = PopulateTruckNo();

            List<SelectListItem> UsersList = new List<SelectListItem>();
            var UsersResultX = ctxTFAT.TfatPass.Select(x => new { Code = x.Code, Name = x.Name }).ToList().Distinct();
            foreach (var Usersitem in UsersResultX)
            {
                UsersList.Add(new SelectListItem { Text = Usersitem.Name, Value = Usersitem.Code.ToString() });
            }
            mModel.UsersMultiX = UsersList;

            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                mdocument = mModel.Document;
                var mList = ctxTFAT.TfatComp.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                var mCountry = ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == mList.Country).Select(x => new { Name = x.Name, Code = x.Code.ToString() }).FirstOrDefault();
                var mState = ctxTFAT.TfatState.Where(x => x.Code.ToString() == mList.State).Select(x => new { Name = x.Name, Code = x.Code.ToString() }).FirstOrDefault();
                var mCity = ctxTFAT.TfatCity.Where(x => x.Code.ToString() == mList.City).Select(x => new { Name = x.Name, Code = x.Code.ToString() }).FirstOrDefault();
                mModel.TfatComp_Country = mCountry != null ? mCountry.Code.ToString() : "";
                mModel.CountryName = mCountry != null ? mCountry.Name : "";
                mModel.TfatComp_State = mState != null ? mState.Code.ToString() : "";
                mModel.StateName = mState != null ? mState.Name : "";
                mModel.TfatComp_City = mCity != null ? mCity.Code.ToString() : "";
                mModel.CityName = mCity != null ? mCity.Name : "";
                mModel.TfatComp_Code = mList.Code;
                mModel.TfatComp_UseHSNMaster = mList.UseHSNMaster;
                mModel.TfatComp_CSTNo = mList.cstno;
                mModel.TfatComp_TINNo = mList.TINNo;
                mModel.TfatComp_LstNo = mList.lstno;
                mModel.TfatComp_Name = mList.Name;
                mModel.TfatComp_GSTNo = mList.GSTNo;
                mModel.TfatComp_TDSReg = mList.TDSReg;
                mModel.TfatComp_VATReg = mList.VATReg;
                mModel.TfatComp_CINNumber = mList.CINNumber;
                mModel.TfatComp_Adrl1 = mList.Adrl1;
                mModel.TfatComp_TDSCir = mList.TDSCir;
                mModel.TfatComp_Adrl2 = mList.Adrl2;
                mModel.TfatComp_PAN = mList.PAN;
                mModel.TfatComp_Adrl3 = mList.Adrl3;
                mModel.TfatComp_Business = mList.Business;
                mModel.TfatComp_Const = mList.Const;
                mModel.TfatComp_Adrl4 = mList.Adrl4;
                mModel.TfatComp_AuthCap = mList.AuthCap != null ? mList.AuthCap.Value : 0;
                mModel.TfatComp_Users = mList.Users;
                mModel.TfatComp_Pin = mList.Pin;
                mModel.TfatComp_Email = mList.CompEmail;
                mModel.TfatComp_fax = mList.fax;
                mModel.TfatComp_Tel1 = mList.Tel1;
                mModel.TfatComp_Tel2 = mList.Tel2;
                mModel.TfatComp_Tel3 = mList.Tel3;
                mModel.TfatComp_Tel4 = mList.Tel4;
                mModel.TfatComp_www = mList.www;
                mModel.TfatPass_GlobalMail = (bool)mList.GlobalMail;
                mModel.TfatPass_Email = mList.Email;
                mModel.TfatPass_BCCTo = mList.BCCTo;
                mModel.TfatPass_CCTo = mList.CCTo;
                mModel.TfatPass_SMTPServer = mList.SMTPServer;
                mModel.TfatPass_SMTPUser = mList.SMTPUser;
                mModel.TfatPass_SMTPPassword = mList.SMTPPassword;
                mModel.TfatPass_SMTPPort = mList.SMTPPort != null ? mList.SMTPPort.Value : 0;
                mModel.TransportID = mList.TransporteID;

                mModel.TfatComp_SSMSURL = mList.SMSURL;
                mModel.TfatComp_UserNameWithValue = mList.SmsUername;
                mModel.TfatComp_PasswordithValue = mList.Smspassword;
                mModel.TfatComp_IDWithValue = mList.SmsID;
                mModel.TfatComp_Para1 = mList.SmsPara1;
                mModel.TfatComp_Para2 = mList.SmsPara2;
                mModel.TfatComp_Para3 = mList.SmsPara3;
                mModel.TfatComp_Para4 = mList.SmsPara4;
                mModel.TfatComp_Para5 = mList.SmsPara5;

                //Eway Bill
                mModel.EwayEmail = mList.EwayEmail;
                mModel.EwayUsername = mList.EwayUsername;
                mModel.EwayPass = mList.EwayPass;
                mModel.EwayClientID = mList.EwayClientID;
                mModel.EwayClientSecret = mList.EwayClientSecret;
                mModel.EwayGSTIn = mList.EwayGSTIn;

                //EInvoice
                mModel.EInvoiceID = mList.EInvoiceID;
                mModel.EInvoicePassword = mList.EInvoicePassword;
                mModel.EInvoiceGSTNo = mList.EInvoiceGSTNo;


                var VehicleTrackList = (from VehicleTrack in ctxTFAT.TfatVehicleTrackApiList
                              where VehicleTrack.CompCode == mList.Code
                              select new ChangeCompanyDetailsVM()
                              {
                                  TfatComp_VehicleTrackURL= VehicleTrack.TrackApi,
                                  TfatComp_VehicleTrackUserNameWithValue= VehicleTrack.Username,
                                  TfatComp_VehicleTrackPasswordithValue = VehicleTrack.Password,
                                  TfatComp_VehicleTrackPara1 = VehicleTrack.Para1,
                                  TfatComp_VehicleTrackPara2 = VehicleTrack.Para2,
                                  TfatComp_VehicleTrackPara3 = VehicleTrack.Para3,
                                  TfatComp_VehicleTrackPara4 = VehicleTrack.Para4,
                                  TfatComp_VehicleTrackPara5 = VehicleTrack.Para5,
                                  TfatComp_VehicleList = VehicleTrack.VehicleList,
                              }).ToList();
                int k = 1;
                foreach (var item in VehicleTrackList)
                {
                    item.tempId = k;
                    item.TfatComp_VehicleList = item.TfatComp_VehicleList.Replace("'", "");
                    ++k;
                }
                mModel.VehicleTrackinglist = VehicleTrackList;
                Session["CompanyTrackingList"] = VehicleTrackList;
            }
            else
            {
                mModel.TfatComp_Adrl1 = "";
                mModel.TfatComp_Adrl2 = "";
                mModel.TfatComp_Adrl3 = "";
                mModel.TfatComp_Adrl4 = "";
                mModel.TfatComp_AuthCap = 0;
                mModel.TfatComp_AuthNo = "";
                mModel.TfatComp_Business = "";
                mModel.TfatComp_CINNumber = "";
                mModel.TfatComp_City = "";
                mModel.TfatComp_CompanyLogo = "";
                mModel.TfatComp_CompanyType = 0;
                mModel.TfatComp_CompInfo = "";
                mModel.TfatComp_Const = "";
                mModel.TfatComp_Country = "";
                mModel.TfatComp_CSTNo = "";
                mModel.TfatComp_DDOCode = "";
                mModel.TfatComp_DDOReg = "";
                mModel.TfatComp_DeductorType = "";
                mModel.TfatComp_Email = "";
                mModel.TfatComp_fax = "";
                mModel.TfatComp_GSTNo = "";
                mModel.TfatComp_LastBranch = "";
                mModel.TfatComp_LastPeriod = "";
                mModel.TfatComp_Licence1 = "";
                mModel.TfatComp_Licence2 = "";
                mModel.TfatComp_LstNo = "";
                mModel.TfatComp_Ministry = 0;
                mModel.TfatComp_Name = "";
                mModel.TfatComp_Nature = "";
                mModel.TfatComp_PAN = "";
                mModel.TfatComp_PAOCode = "";
                mModel.TfatComp_PAOReg = "";
                mModel.TfatComp_PFFix = false;
                mModel.TfatComp_PFupLimit = "";
                mModel.TfatComp_Pin = "";
                mModel.TfatComp_SMSURL = "";
                mModel.TfatComp_State = "";
                mModel.TfatComp_STDCode = "";
                mModel.TfatComp_Taluka = "";
                mModel.TfatComp_TDSCir = "";
                mModel.TfatComp_TDSReg = "";
                mModel.TfatComp_Tel1 = "";
                mModel.TfatComp_Tel2 = "";
                mModel.TfatComp_Tel3 = "";
                mModel.TfatComp_Tel4 = "";
                mModel.TfatComp_TINNo = "";
                mModel.TfatComp_USERPass = "";
                mModel.TfatComp_Users = "";
                mModel.TfatComp_VATReg = "";
                mModel.TfatComp_www = "";
            }
            return View(mModel);
        }

        #region Tracking Details

        [HttpPost]
        public ActionResult DeleteLRDetails(ChangeCompanyDetailsVM Model)
        {
            Model.VehicleList = PopulateTruckNo();
            List<ChangeCompanyDetailsVM> objledgerdetail = new List<ChangeCompanyDetailsVM>();
            if (Session["CompanyTrackingList"] != null)
            {
                objledgerdetail = (List<ChangeCompanyDetailsVM>)Session["CompanyTrackingList"];
            }
            
            var result2 = objledgerdetail.Where(x => x.tempId != Model.tempId).Select(x => x).ToList();

            Session["CompanyTrackingList"] = result2;
            var html = ViewHelper.RenderPartialView(this, "VehicleTrackList", new ChangeCompanyDetailsVM() { VehicleList= Model.VehicleList, VehicleTrackinglist = result2 });
            return Json(new { LRDetailList = result2, Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetLRDetails(ChangeCompanyDetailsVM Model)
        {
            Model.VehicleList = PopulateTruckNo();
            List<ChangeCompanyDetailsVM> objledgerdetail = new List<ChangeCompanyDetailsVM>();
            if (Session["CompanyTrackingList"] != null)
            {
                objledgerdetail = (List<ChangeCompanyDetailsVM>)Session["CompanyTrackingList"];
            }

            if (objledgerdetail != null && objledgerdetail.Count() > 0)
            {
                foreach (var a in objledgerdetail.Where(x => x.tempId == Model.tempId))
                {
                    Model.TfatComp_VehicleTrackURL = a.TfatComp_VehicleTrackURL;
                    Model.TfatComp_VehicleTrackUserNameWithValue = a.TfatComp_VehicleTrackUserNameWithValue;
                    Model.TfatComp_VehicleTrackPasswordithValue = a.TfatComp_VehicleTrackPasswordithValue;
                    Model.TfatComp_VehicleTrackPara1 = a.TfatComp_VehicleTrackPara1;
                    Model.TfatComp_VehicleTrackPara2 = a.TfatComp_VehicleTrackPara2;
                    Model.TfatComp_VehicleTrackPara3 = a.TfatComp_VehicleTrackPara3;
                    Model.TfatComp_VehicleTrackPara4 = a.TfatComp_VehicleTrackPara4;
                    Model.TfatComp_VehicleTrackPara5 = a.TfatComp_VehicleTrackPara5;
                    Model.TfatComp_VehicleList = a.TfatComp_VehicleList.Replace("'", "");
                    Model.tempId = a.tempId;
                }
            }
            var html = ViewHelper.RenderPartialView(this, "VehicleTrackList", Model);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddLRDetails(ChangeCompanyDetailsVM Model)
        {
            Model.VehicleList = PopulateTruckNo();
            List<ChangeCompanyDetailsVM> lrdetaillist = new List<ChangeCompanyDetailsVM>();
            if (Session["CompanyTrackingList"] != null)
            {
                lrdetaillist = (List<ChangeCompanyDetailsVM>)Session["CompanyTrackingList"];
            }

            if (Model.SessionFlag == "Add")
            {

                if (String.IsNullOrEmpty(Model.TfatComp_VehicleTrackURL))
                {
                    Model.Status = "ConfirmError";
                    Model.Message = "Please Enter Proper Details..";
                    return Json(Model, JsonRequestBehavior.AllowGet);
                }
                lrdetaillist.Add(new ChangeCompanyDetailsVM()
                {
                    TfatComp_VehicleTrackURL = Model.TfatComp_VehicleTrackURL,
                    TfatComp_VehicleTrackUserNameWithValue = Model.TfatComp_VehicleTrackUserNameWithValue,
                    TfatComp_VehicleTrackPasswordithValue = Model.TfatComp_VehicleTrackPasswordithValue,
                    TfatComp_VehicleTrackPara1 = Model.TfatComp_VehicleTrackPara1,
                    TfatComp_VehicleTrackPara2 = Model.TfatComp_VehicleTrackPara2,
                    TfatComp_VehicleTrackPara3 = Model.TfatComp_VehicleTrackPara3,
                    TfatComp_VehicleTrackPara4 = Model.TfatComp_VehicleTrackPara4,
                    TfatComp_VehicleTrackPara5 = Model.TfatComp_VehicleTrackPara5,
                    TfatComp_VehicleList = Model.TfatComp_VehicleList,
                    tempId = lrdetaillist.Count + 1,

                });

            }
            else
            {

                foreach (var item in lrdetaillist.Where(x => x.tempId == Model.tempId))
                {
                    item.TfatComp_VehicleTrackURL = Model.TfatComp_VehicleTrackURL;
                    item.TfatComp_VehicleTrackUserNameWithValue = Model.TfatComp_VehicleTrackUserNameWithValue;
                    item.TfatComp_VehicleTrackPasswordithValue = Model.TfatComp_VehicleTrackPasswordithValue;
                    item.TfatComp_VehicleTrackPara1 = Model.TfatComp_VehicleTrackPara1;
                    item.TfatComp_VehicleTrackPara2 = Model.TfatComp_VehicleTrackPara2;
                    item.TfatComp_VehicleTrackPara3 = Model.TfatComp_VehicleTrackPara3;
                    item.TfatComp_VehicleTrackPara4 = Model.TfatComp_VehicleTrackPara4;
                    item.TfatComp_VehicleTrackPara5 = Model.TfatComp_VehicleTrackPara5;
                    item.TfatComp_VehicleList = Model.TfatComp_VehicleList;
                    item.tempId = Model.tempId;
                }
            }

            Session["CompanyTrackingList"] = lrdetaillist;
            var html = ViewHelper.RenderPartialView(this, "VehicleTrackList", new ChangeCompanyDetailsVM() { VehicleList=Model.VehicleList , VehicleTrackinglist = lrdetaillist });
            return Json(new { LRDetailList = lrdetaillist, Html = html }, JsonRequestBehavior.AllowGet);


        }


        #endregion

        #region SaveData
        public ActionResult SaveData(ChangeCompanyDetailsVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteChangeCompanyDetails(mModel);
                        if (mModel.Mode == "Delete")
                        {
                            transaction.Commit();
                            transaction.Dispose();
                            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.TfatComp_Code, DateTime.Now, 0, mModel.TfatComp_Code, "Delete Company", "C");
                            return Json(new
                            {
                                Status = "success",
                                Message = "Data is Deleted."
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    TfatComp mobj = new TfatComp();
                    if (mModel.Mode == "Edit")
                    {
                        mobj = ctxTFAT.TfatComp.Where(x => (x.Code == mModel.TfatComp_Code)).FirstOrDefault();
                        var VehicleTrack = ctxTFAT.TfatVehicleTrackApiList.ToList();
                        ctxTFAT.TfatVehicleTrackApiList.RemoveRange(VehicleTrack);
                        
                    }
                    mobj.Code = mModel.TfatComp_Code;
                    mobj.UseHSNMaster = mModel.TfatComp_UseHSNMaster;
                    mobj.cstno = mModel.TfatComp_CSTNo;
                    mobj.TINNo = mModel.TfatComp_TINNo;
                    mobj.lstno = mModel.TfatComp_LstNo;
                    mobj.Name = mModel.TfatComp_Name;
                    mobj.GSTNo = mModel.TfatComp_GSTNo;
                    mobj.TDSReg = mModel.TfatComp_TDSReg;
                    mobj.VATReg = mModel.TfatComp_VATReg;
                    mobj.CINNumber = mModel.TfatComp_CINNumber;
                    mobj.Adrl1 = mModel.TfatComp_Adrl1;
                    mobj.TDSCir = mModel.TfatComp_TDSCir;
                    mobj.Adrl2 = mModel.TfatComp_Adrl2;
                    mobj.PAN = mModel.TfatComp_PAN;
                    mobj.Adrl3 = mModel.TfatComp_Adrl3;
                    mobj.Business = mModel.TfatComp_Business;
                    mobj.Const = mModel.TfatComp_Const;
                    mobj.Adrl4 = mModel.TfatComp_Adrl4;
                    mobj.Country = mModel.TfatComp_Country;
                    mobj.AuthCap = mModel.TfatComp_AuthCap;
                    mobj.State = mModel.TfatComp_State;
                    mobj.Users = mModel.TfatComp_Users;
                    mobj.City = mModel.TfatComp_City;
                    mobj.Pin = mModel.TfatComp_Pin;
                    mobj.CompEmail = mModel.TfatComp_Email;
                    mobj.fax = mModel.TfatComp_fax;
                    mobj.Tel1 = mModel.TfatComp_Tel1;
                    mobj.Tel2 = mModel.TfatComp_Tel2;
                    mobj.Tel3 = mModel.TfatComp_Tel3;
                    mobj.Tel4 = mModel.TfatComp_Tel4;
                    mobj.www = mModel.TfatComp_www;
                    if (!String.IsNullOrEmpty(mModel.TransportID))
                    {
                        mobj.TransporteID= mModel.TransportID.ToUpper();
                    }
                    else
                    {
                        mobj.TransporteID = mModel.TransportID;
                    }
               
                    // iX9: default values for the fields not used @Form
                    mobj.AuthNo = "";
                    mobj.CompanyLogo = "";
                    mobj.CompanyType = 0;
                    mobj.CompInfo = "";
                    mobj.DDOCode = "";
                    mobj.DDOReg = "";
                    mobj.DeductorType = "";
                    mobj.LastBranch = "";
                    mobj.LastPeriod = "";
                    mobj.Licence1 = "";
                    mobj.Licence2 = "";
                    mobj.Ministry = 0;
                    mobj.Nature = "";
                    mobj.PAOCode = "";
                    mobj.PAOReg = "";
                    mobj.PFFix = false;
                    mobj.PFupLimit = "";
                    mobj.STDCode = "";
                    mobj.Taluka = "";
                    mobj.USERPass = "";
                    mobj.AssessOffice = "";
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;

                    mobj.Email = string.IsNullOrEmpty(mModel.TfatPass_Email)==true? mModel.TfatComp_Email : mModel.TfatPass_Email;
                    mobj.BCCTo = mModel.TfatPass_BCCTo;
                    mobj.CCTo = mModel.TfatPass_CCTo;
                    mobj.SMTPServer = mModel.TfatPass_SMTPServer;
                    mobj.SMTPUser = mModel.TfatPass_SMTPUser;
                    mobj.SMTPPassword = mModel.TfatPass_SMTPPassword;
                    mobj.SMTPPort = mModel.TfatPass_SMTPPort;
                    mobj.GlobalMail = mModel.TfatPass_GlobalMail;


                    mobj.SMSURL = mModel.TfatComp_SSMSURL;
                    mobj.SmsUername = mModel.TfatComp_UserNameWithValue;
                    mobj.Smspassword = mModel.TfatComp_PasswordithValue;
                    mobj.SmsID = mModel.TfatComp_IDWithValue;
                    mobj.SmsPara1 = mModel.TfatComp_Para1;
                    mobj.SmsPara2 = mModel.TfatComp_Para2;
                    mobj.SmsPara3 = mModel.TfatComp_Para3;
                    mobj.SmsPara4 = mModel.TfatComp_Para4;
                    mobj.SmsPara5 = mModel.TfatComp_Para5;

                    //Eway Bill
                    mobj.EwayEmail = mModel.EwayEmail;
                    mobj.EwayUsername = mModel.EwayUsername;
                    mobj.EwayPass = mModel.EwayPass;
                    mobj.EwayClientID = mModel.EwayClientID;
                    mobj.EwayClientSecret = mModel.EwayClientSecret;
                    mobj.EwayGSTIn = mModel.EwayGSTIn;

                    //EInvoice
                    mobj.EInvoiceID = mModel.EInvoiceID;
                    mobj.EInvoicePassword = mModel.EInvoicePassword;
                    mobj.EInvoiceGSTNo = mModel.EInvoiceGSTNo;

                    if (mModel.Mode == "Add")
                    {
                        mobj.Code = GetNextCode();
                    }
                    if (mModel.Mode == "Add")
                    {
                        mobj.LASTUPDATEDATE = System.DateTime.Now;
                        ctxTFAT.TfatComp.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }

                    #region Save Vehicle Tracking
                    List<ChangeCompanyDetailsVM> lrdetaillist = new List<ChangeCompanyDetailsVM>();
                    if (Session["CompanyTrackingList"] != null)
                    {
                        lrdetaillist = (List<ChangeCompanyDetailsVM>)Session["CompanyTrackingList"];
                    }
                    foreach (var item in lrdetaillist)
                    {
                        TfatVehicleTrackApiList tfatVehicleTrackApiList = new TfatVehicleTrackApiList();
                        tfatVehicleTrackApiList.CompCode = mobj.Code;
                        tfatVehicleTrackApiList.TrackApi = item.TfatComp_VehicleTrackURL;
                        tfatVehicleTrackApiList.Username = item.TfatComp_VehicleTrackUserNameWithValue;
                        tfatVehicleTrackApiList.Password = item.TfatComp_VehicleTrackPasswordithValue;
                        tfatVehicleTrackApiList.Para1 = item.TfatComp_VehicleTrackPara1;
                        tfatVehicleTrackApiList.Para2 = item.TfatComp_VehicleTrackPara2;
                        tfatVehicleTrackApiList.Para3 = item.TfatComp_VehicleTrackPara3;
                        tfatVehicleTrackApiList.Para4 = item.TfatComp_VehicleTrackPara4;
                        tfatVehicleTrackApiList.Para5 = item.TfatComp_VehicleTrackPara5;
                        tfatVehicleTrackApiList.VehicleList = item.TfatComp_VehicleList;
                        tfatVehicleTrackApiList.AUTHIDS = muserid;
                        tfatVehicleTrackApiList.AUTHORISE = mauthorise;
                        tfatVehicleTrackApiList.ENTEREDBY = muserid;
                        tfatVehicleTrackApiList.LASTUPDATEDATE = System.DateTime.Now;
                        ctxTFAT.TfatVehicleTrackApiList.Add(tfatVehicleTrackApiList);

                    }

                    #endregion





                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.TfatComp_Code, DateTime.Now, 0, mModel.TfatComp_Code, "Save Company", "C");

                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "ChangeCompanyDetails" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "ChangeCompanyDetails" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                }
            }
            return Json(new { Status = "Success", id = "ChangeCompanyDetails" }, JsonRequestBehavior.AllowGet);
        }

        public string GetNextCode()
        {
            string Code = (from x in ctxTFAT.TfatComp select x.Code).Max();
            string digits = new string(Code.Where(char.IsDigit).ToArray());
            int number;
            if (!int.TryParse(digits, out number)) //int.Parse would do the job since only digits are selected
            {
                number = 0;
            }
            return (++number).ToString("D6");
        }

        public ActionResult DeleteChangeCompanyDetails(ChangeCompanyDetailsVM mModel)
        {
            if (mModel.TfatComp_Code == null || mModel.TfatComp_Code == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var mList = ctxTFAT.TfatComp.Where(x => (x.Code == mModel.TfatComp_Code)).FirstOrDefault();
            ctxTFAT.TfatComp.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}