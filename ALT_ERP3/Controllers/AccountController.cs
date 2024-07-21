using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Models;

namespace ALT_ERP3.Controllers
{
    public class AccountController : BaseController
    {
        List<SelectListItem> company = new List<SelectListItem>();
        List<SelectListItem> branch = new List<SelectListItem>();
        List<SelectListItem> perd = new List<SelectListItem>();
        HttpCookie cookie = new HttpCookie("Cookie");
        HttpCookie compcookie = new HttpCookie("CompCookie");

        // GET: Account
        public ActionResult Login()
        {
            Session["CurrentDatabase"] = "";// @"data source=151.106.32.145\SQL2017,2521;initial catalog=TFAT_ERPiX9_Leebo;user id=sa;password=suc357;MultipleActiveResultSets=True;App=EntityFramework";
            //ctxTFAT = new tfatCTX(Session["CurrentDatabase"].ToString());// = new ALT_ERP21Entities(System.Web.HttpContext.Current.Session["CurrentDatabase"].ToString());
            if (Session["BranchCode"] != null)
            {
                Session["CompCode"] = null;
                Session["CompName"] = null;
                Session["BranchCode"] = null;
                Session["UserId"] = null;
                Session["FPerd"] = null;
                Session["ActivityType"] = null;
            }
            //Session["CurrentDatabase"] = "ALT_ERP21Entities";
            Session["MyIP"] = "";
            Session["location"] = "";
            Session["locationpin"] = "";
            Session["SuchanSMS"] = false;
            Session["Modules"] = "";
            Session["IsERP"] = false;
            Session["Version"] = 3;
            Session["IsBlack"] = false;
            Session["SuchanAdmin"] = "0";
            Session["DocString"] = "";
            Session["CustomerID"] = "";
            Session["ActivityType"] = "";
            UserProfileVM user = new UserProfileVM();
            HttpCookie myCookie;// = new HttpCookie("Cookie");
            myCookie = Request.Cookies["Cookie"];
            if (myCookie != null)
            {
                user.TfatPass_CorpID = myCookie.Values["customerid"];
                user.TfatPass_Email = myCookie.Values["ids"];
                user.TfatPass_PassWords = ""; // myCookie.Values["pass"];
            }
            return View(user);
        }

        [HttpPost]
        public ActionResult Login(UserProfileVM Model)
        {
            if (ModelState.IsValid)
            {
                Session["Language"] = "en-US";
                Session["SuchanAdmin"] = "0";
                Session["MyIP"] = "123.123.12.12";// GetMyIP();

                if (Model.TfatPass_Email == null || Model.TfatPass_PassWords == null)
                {
                    ModelState.AddModelError("", "Invalid Credentials.");
                    return View(Model);
                }
                Session["CustomerID"] = Model.TfatPass_CorpID;
                string mdatabase = ctxTFAT.Database.Connection.Database.ToString();

                string mday = DateTime.Now.DayOfWeek.ToString().Substring(0, 3);
                try
                {
                    var mpass = ctxTFAT.TfatPass.Where(x => x.Email == Model.TfatPass_Email).Select(x => new { x.Expiry, x.Code, x.PassWords, x.Locked, x.Sun, x.Mon, x.Tue, x.Wed, x.Thu, x.Fri, x.Sat, x.Email, x.Mobile, x.Name, x.EmailVerified, x.MobileVerified, x.UserLanguage }).FirstOrDefault();
                    if (mpass == null)
                    {
                        mpass = ctxTFAT.TfatPass.Where(x => x.Mobile == Model.TfatPass_Email).Select(x => new { x.Expiry, x.Code, x.PassWords, x.Locked, x.Sun, x.Mon, x.Tue, x.Wed, x.Thu, x.Fri, x.Sat, x.Email, x.Mobile, x.Name, x.EmailVerified, x.MobileVerified, x.UserLanguage }).FirstOrDefault();
                    }

                    if (mpass != null)
                    {
                        muserid = mpass.Code;
                        if (Model.TfatPass_CorpID == null || Model.TfatPass_CorpID == "")
                        {
                            ModelState.AddModelError("", "Customer ID is Required.");
                            return View(Model);
                        }
                        else if (mpass.PassWords != Model.TfatPass_PassWords)
                        {
                            ModelState.AddModelError("", "Incorrect User ID or Password.");
                            return View(Model);
                        }
                        else if (mpass.Locked == true && mpass.Code.ToLower() != "super")
                        {
                            ModelState.AddModelError("", "You're not allowed to Login, Contact your ADMIN.");
                            return View(Model);
                        }

                        else if (mpass.Code.ToLower() != "super" && ((mday == "Sun" && mpass.Sun == false) || (mday == "Mon" && mpass.Mon == false) || (mday == "Tue" && mpass.Tue == false) || (mday == "Wed" && mpass.Wed == false) || (mday == "Thu" && mpass.Thu == false) || (mday == "Fri" && mpass.Fri == false) || (mday == "Sat" && mpass.Sat == false)))
                        {
                            ModelState.AddModelError("", "You're Not allowed to Login Today, Contact your ADMIN.");
                            return View(Model);
                        }
                        else if (ctxTFAT.UserOTPLogin.Where(z => z.UserID == mpass.Code).Select(x => x.LoginOTP).FirstOrDefault() == true && Session["SuchanAdmin"].ToString() == "0")
                        {
                            //Model.TfatPass_LastBranch holds 
                            if ((ctxTFAT.RequestOTP.Where(z => z.UserID == mpass.Code && z.IsActive == true && z.OptionName == "Login" && z.ExpDate >= DateTime.Now).Select(x => x.OTP).FirstOrDefault() ?? 0).ToString() != Model.TfatPass_LastBranch)
                            {
                                ModelState.AddModelError("", "Invalid OTP..Try Again or Request New.");
                                return View(Model);
                            }
                            else
                            {
                                ExecuteStoredProc(@"Update RequestOTP Set IsActive=0 where UserID='" + muserid + "'");
                            }
                        }
                        // read license
                        using (WebClient client = new WebClient())
                        {
                            string mstring = "";
                            //27.09.22 (Suresh Sir Not Available So Comeent The Licence Part As Per Darshan Said) //
                            //if (1 == 2)
                            #region Licence
                            {
                                try
                                {
                                    mstring = client.DownloadString("http://151.106.32.145:8114/api/License/GetLicense?CustomerID=" + Model.TfatPass_CorpID.ToUpper() + "&UserID=" + mpass.Code + "&MyIP=" + System.Web.HttpContext.Current.Session["MyIP"] + "&DatabaseName=" + mdatabase + "&Email=" + (mpass.Email ?? "") + "&Mobile=" + (mpass.Mobile ?? "") + "&Name=" + (mpass.Name ?? ""));
                                    //update mlic into tfatcomp along with validdate
                                    var Decript = "{  'message': 'Hi! We are doing great..',  'version': 9,  'licenseid': '1235-9363-5763',  'status': 'Active',  'contactperson': '',  'mobile': '',  'email': '',  'whatsapp': '',  'keyperson': '',  'keypersonmobile': '',  'keypersonemail': '',  'servicestartdate': '25-May-2020',  'serviceenddate': '07-Feb-2020',  'forcestop': 'False',  'workhourlimit': 'False',  'workhourfrom': '0',  'workhourto': '0',  'databasename': 'TFAT_ERPiX9',  'allowedusers': 5,  'modules': 'SMTRC',  'otplogin': 'True',  'brandname': 'T.FAT ERP iX9',  'brandownername': 'Suchan Software Pvt.Ltd.',  'iserp': 'True',  'suchansms': 'True'}";
                                    // var strring = Crypto.DecryptString(Decript);
                                    ExecuteStoredProc(@"Update TfatComp Set CompInfo='" + Crypto.EncryptString(DateTime.Now.Date.AddDays(7).ToString("dd-MMM-yyyy") + mstring) + "'");
                                }
                                catch (Exception ex)
                                {
                                    // read license from local copy
                                    var mcomp = ctxTFAT.TfatComp.Select(x => new { x.CompInfo }).FirstOrDefault();
                                    if (mcomp != null)
                                    {
                                        mstring = (mcomp.CompInfo ?? "").Trim();
                                    }
                                    if (mstring != "")
                                    {
                                        mstring = Crypto.DecryptString(mstring);
                                        DateTime mdates = Convert.ToDateTime(mstring.Substring(0, 11)).Date;
                                        if (mdates < DateTime.Now)
                                        {
                                            mstring = "";
                                        }
                                        else
                                        {
                                            mstring = mstring.Substring(11);
                                        }
                                    }
                                    if (mstring == "")
                                    {
                                        ModelState.AddModelError("", "Can't validate your License, Contact your ADMIN.");
                                        return View(Model);
                                    }
                                }

                                try
                                {
                                    dynamic mlic = Newtonsoft.Json.Linq.JValue.Parse(Newtonsoft.Json.JsonConvert.DeserializeObject(mstring).ToString());
                                    string mlicenseid = mlic.GetValue("licenseid").ToString();

                                    Session["CustomMsg"] = mlic.GetValue("message").ToString();
                                    int mcnt = ctxTFAT.TfatPass.Count();
                                    if (mcnt > Convert.ToInt32(Session["AllowdUsers"]))
                                    {
                                        Session["CustomMsg"] = "User Count exceeds your License";
                                    }
                                    // check if database name is matching
                                    Session["ServiceEndDate"] = mlic.GetValue("serviceenddate").ToString();
                                    if (Session["SuchanAdmin"].ToString() != "1" && mdatabase.ToLower() != mlic.GetValue("databasename").ToString().ToLower())
                                    {
                                        ModelState.AddModelError("", "Invalid Database Connectivity.\nContact your Service Provider for more Details..");
                                        return View(Model);
                                    }
                                    else
                                    {
                                        DateTime mservdate = Convert.ToDateTime(mlic.GetValue("serviceenddate").ToString()).Date;
                                        int mDays = (mservdate.Date - DateTime.Now.Date).Days;
                                        string mdate = mservdate.Date.ToString("dd-MMM-yyyy");
                                        if (mDays < 0) // already expired
                                        {
                                            if (mlic.GetValue("forcestop").ToString() == "True")
                                            {
                                                ModelState.AddModelError("", "Your Service Contract is Expired on: " + mdate + "\nContact your Service Provider to Resume the Service..");
                                                Session["CustomMsg"] = "Your Service Contract is Expired on: " + mdate;
                                                return View(Model);
                                            }
                                            else
                                            {
                                                ModelState.AddModelError("", "Your Software Service is Already Expired on: " + mdate + "\nContact your Service Provider to Enjoy Un-Interrupted Service..");
                                                Session["CustomMsg"] = "Your Service Contract is Expired on: " + mdate + ", Renew Immediately";
                                            }
                                        }
                                        else if (mDays <= 30)
                                        {
                                            ModelState.AddModelError("", "Your Service Contract is Expiring on: " + mdate + "\nContact your Service Provider for Renewal..");
                                            Session["CustomMsg"] = "Your Service Contract is Expiring on: " + mdate + ", Renew Now";
                                        }
                                    }
                                    //SMTRC
                                    Session["CurrentDatabase"] = mlic.GetValue("databasename").ToString();
                                    Session["KeyPersonMobile"] = mlic.GetValue("keypersonmobile").ToString();
                                    Session["KeyPersonEmail"] = mlic.GetValue("keypersonemail").ToString();
                                    Session["ContactMobile"] = mlic.GetValue("mobile").ToString();
                                    Session["ContactEmail"] = mlic.GetValue("email").ToString();
                                    Session["AllowdUsers"] = mlic.GetValue("allowedusers").ToString();
                                    Session["Modules"] = mlic.GetValue("modules").ToString();
                                    Session["IsERP"] = Convert.ToBoolean(mlic.GetValue("iserp"));
                                    Session["Version"] = Convert.ToInt32(mlic.GetValue("version"));
                                    Session["IsBlack"] = Convert.ToBoolean(mlic.GetValue("otplogin"));
                                    Session["SuchanSMS"] = Convert.ToBoolean(mlic.GetValue("suchansms"));
                                    Session["Modules"] = "SMTRC";
                                    if (Model.TfatPass_Remember == true)
                                    {
                                        cookie.Value = mpass.Code;
                                        cookie.Values.Add("customerid", Model.TfatPass_CorpID);
                                        if (Session["SuchanAdmin"].ToString() == "0")
                                        {
                                            cookie.Values.Add("ids", Model.TfatPass_Email);
                                            cookie.Values.Add("pass", Model.TfatPass_PassWords);
                                        }
                                        ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                                        cookie.Expires = DateTime.Now.AddDays(30);
                                        //Session["Modules"] = "SMTRC";
                                    }
                                    muserid = mpass.Code;
                                    Session["Language"] = mpass.UserLanguage == null || mpass.UserLanguage == "" ? "en-US" : mpass.UserLanguage;


                                    #region default Login OF User //18.9.22

                                    #endregion
                                    //return RedirectToAction("CompanyLogin", new { UserId = mpass.Code });
                                }
                                catch
                                {
                                    ModelState.AddModelError("", "Invalid Comp.ID or other Fatal Error..");
                                }
                            }
                            #endregion Licence
                            #region default Login OF User //18.9.22

                            Complogin userProfile = new Complogin();

                            if (Session["BranchCode"] != null)
                            {
                                Session["CompCode"] = null;
                                Session["CompName"] = null;
                                Session["BranchCode"] = null;
                                Session["UserId"] = null;
                                Session["FPerd"] = null;
                                Session["LocationCode"] = null;
                                Session["ActivityType"] = null;
                            }
                            HttpCookie mycompCookie = new HttpCookie("CompCookie");
                            mycompCookie = Request.Cookies["CompCookie"];
                            if (mycompCookie != null)
                            {
                                userProfile.CompCode = mycompCookie.Values["compcd"];
                                userProfile.CompName = ctxTFAT.TfatComp.Where(x => x.Code == userProfile.CompCode).Select(x => x.Name).FirstOrDefault(); ;
                                //userProfile.BranchCode = mycompCookie.Values["branch"];
                                userProfile.perd = mycompCookie.Values["period"];
                            }
                            userProfile.BranchCode = ctxTFAT.TfatPass.Where(x => x.Code == mpass.Code).Select(x => x.DefaultLoginBranch).FirstOrDefault();
                            if (userProfile.BranchCode == null || userProfile.BranchCode == "")// || Model.CompCode == null || Model.CompCode == "0"
                            {
                                ModelState.AddModelError("", "Default Branch Not Set...!");
                                return View(Model);
                            }
                            userProfile.UserId = mpass.Code;
                            if (userProfile.UserId.ToUpper() == "SUPER")
                            {
                                var TfatPrd = ctxTFAT.TfatPerd.OrderByDescending(x => x.PerdCode).FirstOrDefault();
                                if (TfatPrd != null /*&& String.IsNullOrEmpty(userProfile.perd)==true*/)
                                {
                                    userProfile.perd = TfatPrd.PerdCode;
                                }
                            }
                            else
                            {
                                var TfatPrd = ctxTFAT.TfatPerd.Where(x=>x.Locked==false).OrderByDescending(x => x.PerdCode).FirstOrDefault();
                                if (TfatPrd != null /*&& String.IsNullOrEmpty(userProfile.perd)==true*/)
                                {
                                    if (!String.IsNullOrEmpty(TfatPrd.LockUsers))
                                    {
                                        var List = TfatPrd.LockUsers.Split(',').ToList();
                                        if (List.Where(x=>x== userProfile.UserId).FirstOrDefault()!=null)
                                        {
                                            ModelState.AddModelError("", "This Accounting Period Is Locked For You. \nPlease Contact To Admin...!");
                                            return View(Model);
                                        }
                                    }
                                    else
                                    {
                                        userProfile.perd = TfatPrd.PerdCode;
                                    }
                                    
                                }
                                else
                                {
                                    ModelState.AddModelError("", "All Accounting Period Is Locked. \nPlease Contact To Admin...!");
                                    return View(Model);
                                }
                            }
                            

                           
                            
                            // company code is now hardcoded - SDS
                            userProfile.CompCode = ctxTFAT.TfatComp.Select(m => m.Code).FirstOrDefault() ?? "100";
                            userProfile.CompName = ctxTFAT.TfatComp.Where(x => x.Code == userProfile.CompCode).Select(m => m.Name).FirstOrDefault();
                            userProfile.BranchName = ctxTFAT.TfatBranch.Where(x => x.Code == userProfile.BranchCode).Select(m => m.Name).FirstOrDefault();
                            if (userProfile.CompName == null || userProfile.CompName == "")
                            {
                                return View(Model);
                            }
                            //if (userProfile.BranchName == null || userProfile.BranchName == "")
                            //{
                            //    ModelState.AddModelError("", "Default Branch Not Set...!");
                            //    return View(Model);
                            //}

                            compcookie.Values.Add("compcd", userProfile.CompCode.ToString());
                            compcookie.Values.Add("branch", userProfile.BranchCode.ToString());
                            compcookie.Values.Add("period", userProfile.perd.ToString());
                            ControllerContext.HttpContext.Response.Cookies.Add(compcookie);
                            compcookie.Expires = DateTime.Now.AddDays(30);
                            System.Web.HttpContext.Current.Session["CompCode"] = userProfile.CompCode;
                            System.Web.HttpContext.Current.Session["CompName"] = userProfile.CompName;
                            System.Web.HttpContext.Current.Session["BranchCode"] = userProfile.BranchCode;
                            System.Web.HttpContext.Current.Session["BranchName"] = userProfile.BranchName;
                            System.Web.HttpContext.Current.Session["UserId"] = userProfile.UserId;
                            System.Web.HttpContext.Current.Session["FPerd"] = userProfile.perd;
                            System.Web.HttpContext.Current.Session["ActivityType"] = ctxTFAT.TfatBranch.Where(x => x.Code == userProfile.BranchCode).Select(m => m.BranchType).FirstOrDefault();

                            int mloc = 100001;
                            //var TfatPrd1 = ctxTFAT.TfatPerd.Where(x => x.PerdCode.Trim() == userProfile.perd.Trim()).FirstOrDefault();
                            //if (TfatPrd1 != null)
                            //{
                            //    if (TfatPrd1.Locked)
                            //    {
                            //        ModelState.AddModelError("", "Selected Period Is Locked.");
                            //        return View(Model);
                            //    }
                            //}
                            System.Web.HttpContext.Current.Session["LocationCode"] = mloc;
                            Session["Modules"] = "SMTRC";
                            // first read modules rights from TfatBranch, then from tfatcomp
                            string mModules = "";// (ctxTFAT.TfatBranch.Where(x => x.Code == Model.BranchCode).Select(m => m.Modules).FirstOrDefault() ?? "").Trim();
                            string mModules2 = System.Web.HttpContext.Current.Session["Modules"].ToString();
                            //(ctxTFAT.TfatComp.Where(x => x.Code == Model.CompCode).Select(m => m.Modules).FirstOrDefault() ?? "").Trim();
                            if (mModules == "")
                            {
                                mModules = mModules2;
                            }
                            else
                            {
                                if (mModules.Length > 10) mModules = mModules.Substring(0, 10);
                                // if branch modules are more than company rights and match them to company rights
                                for (int x = 0; x < mModules.Length; x++)
                                {
                                    if (mModules.Substring(x, 1) != "X" && mModules.Substring(x, 1) != mModules2.Substring(x, 1))
                                    {
                                        mModules = mModules.Substring(0, x) + mModules2.Substring(x, 1) + mModules.Substring(x + 1, mModules.Length - x - 1);
                                    }
                                }
                            }

                            if (mModules == "")
                            {
                                mModules = "AFPSIXXXXX";    // all active means AFPSIMHACP 
                            }

                            System.Web.HttpContext.Current.Session["Modules"] = mModules;
                            System.Web.HttpContext.Current.Session["ErrorMessage"] = "";

                            int mScheme = 0;
                            var mUser = ctxTFAT.TfatPass.Where(x => x.Code == userProfile.UserId).Select(m => new { m.Modules, m.GridRows, m.MinItem, m.MinAcc, m.MinOthers, m.ColScheme, m.UserLanguage, m.AllowAlternate }).FirstOrDefault();
                            if (mUser != null)
                            {
                                var m = "10^15^20^50^100^500^1000^3000^5000^10000^50000".Split('^');
                                m = "10^15^20^50^100^500^1000^5000^10000^20000^50000".Split('^');
                                int mRows = mUser.GridRows.Value == null || mUser.GridRows.Value == 0 ? 1 : mUser.GridRows.Value;
                                mRows = Convert.ToInt32(m[mRows]);
                                Session["GridRows"] = mRows;
                                Session["MinItem"] = mUser.MinItem == null ? 3 : mUser.MinItem;
                                Session["MinAcc"] = mUser.MinAcc == null ? 2 : mUser.MinAcc;
                                Session["MinOthers"] = mUser.MinOthers == null ? 0 : mUser.MinItem;
                                if (Convert.ToBoolean(Session["IsBlack"]) == true)
                                {
                                    Session["IsBlack"] = mUser.AllowAlternate;
                                }
                                mScheme = mUser.ColScheme ?? 0;
                                Session["Language"] = mUser.UserLanguage == null || mUser.UserLanguage == "" ? "en-US" : mUser.UserLanguage;

                                // first read modules rights from tfatpass, then from TfatBranch
                                mModules = (mUser.Modules ?? "").Trim();
                                mModules2 = System.Web.HttpContext.Current.Session["Modules"].ToString();
                                //(ctxTFAT.TfatComp.Where(x => x.Code == Model.CompCode).Select(m => m.Modules).FirstOrDefault() ?? "").Trim();
                                if (mModules == "")
                                {
                                    mModules = mModules2;
                                }
                                else
                                {
                                    if (mModules.Length > 10) mModules = mModules.Substring(0, 10);
                                    // if user modules are more than branch rights and match them to branch rights
                                    for (int x = 0; x < mModules.Length; x++)
                                    {
                                        if (mModules.Substring(x, 1) != "X" && mModules.Substring(x, 1) != mModules2.Substring(x, 1))
                                        {
                                            mModules = mModules.Substring(0, x) + mModules2.Substring(x, 1) + mModules.Substring(x + 1, mModules.Length - x - 1);
                                        }
                                    }
                                }

                                if (mModules == "")
                                {
                                    mModules = mModules2;
                                }
                            }
                            else
                            {
                                Session["GridRows"] = 15;
                                Session["MinItem"] = 3;
                                Session["MinAcc"] = 2;
                                Session["MinOthers"] = 0;
                                Session["IsBlack"] = false;
                                mScheme = 0;
                            }

                            SetColour(mScheme);


                            ExecuteStoredProc("Update TfatPass Set LoginNow=-1, LastLogin='" + DateTime.Now.ToString("dd/MMM/yyyy") + "' Where Code='" + userProfile.UserId + "'");
                            var StartDate = ctxTFAT.TfatPerd.Where(x => x.PerdCode == userProfile.perd).ToList().Select(b => b.StartDate.Date).FirstOrDefault();
                            var EndDate = ctxTFAT.TfatPerd.Where(x => x.PerdCode == userProfile.perd).ToList().Select(b => b.LastDate.Value.Date).FirstOrDefault();
                            System.Web.HttpContext.Current.Session["StartDate"] = StartDate.Date.ToShortDateString();
                            System.Web.HttpContext.Current.Session["LastDate"] = EndDate.Date.ToShortDateString();
                            UpdateAuditTrail(userProfile.BranchCode, "Login", "Login", "", DateTime.Now, 0, "", "", "A");
                            //}
                            //else
                            //{
                            //    ModelState.AddModelError("", "Please enter Details.");
                            //}
                            return RedirectToAction("Index", "FirstPage");



                            #endregion
                        }


                    }
                    else
                    {
                        ModelState.AddModelError("", "Incorrect User Id or Password.");
                    }
                }
                catch (Exception mex)
                {
                    ModelState.AddModelError("", "Invalid Customer ID.");
                    return View(Model);
                }
            }
            else
            {
                ModelState.AddModelError("", "Please Enter valid User-Id and Password.");
            }





            return View(Model);
        }

        public ActionResult GetUserDetails(string emailid)
        {
            string userid = "";
            bool emailverified = false;
            bool mobileverified = false;
            bool otpreqd = false;
            try
            {
                userid = ctxTFAT.TfatPass.Where(x => x.Email == emailid).Select(x => x.Code).FirstOrDefault() ?? "";
                if (userid == "")
                {
                    userid = ctxTFAT.TfatPass.Where(x => x.Mobile == emailid).Select(x => x.Code).FirstOrDefault() ?? "";
                }
                emailverified = ctxTFAT.TfatPass.Where(x => x.Code == userid).Select(x => x.EmailVerified).FirstOrDefault();
                mobileverified = ctxTFAT.TfatPass.Where(x => x.Code == userid).Select(x => x.MobileVerified).FirstOrDefault();
                otpreqd = false;
                if (userid != "")
                {
                    otpreqd = ctxTFAT.UserOTPLogin.Where(z => z.UserID == userid).Select(x => x.LoginOTP).FirstOrDefault();
                }
            }
            catch { }
            return Json(new { userid, otpreqd, emailverified, mobileverified }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult VerifyUser(string userid, int emailotp, int mobileotp)
        {
            bool emailok = false;
            bool mobileok = false;
            try
            {
                if ((ctxTFAT.RequestOTP.Where(x => x.UserID == userid && x.IsActive == true && x.OptionName == "NewUserEmail").Select(x => x.OTP).FirstOrDefault() ?? 0) == emailotp)
                {
                    emailok = true;
                }
                if ((ctxTFAT.RequestOTP.Where(x => x.UserID == userid && x.IsActive == true && x.OptionName == "NewUserMobile").Select(x => x.OTP).FirstOrDefault() ?? 0) == mobileotp)
                {
                    mobileok = true;
                }
                if (emailok && mobileok)
                {
                    ExecuteStoredProc(@"Update RequestOTP Set IsActive=0 where UserID='" + userid + "'");
                    ExecuteStoredProc(@"Update TfatPass Set EmailVerified=-1, MobileVerified=-1 where Code='" + userid + "'");
                }
            }
            catch { }
            return Json(new { emailok, mobileok }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RequestLoginOTP(string UserID)
        {
            string muser = ctxTFAT.TfatPass.Where(x => x.Email == UserID).Select(x => x.Code).FirstOrDefault() ?? "";
            if (muser == "")
            {
                muser = ctxTFAT.TfatPass.Where(x => x.Mobile == UserID).Select(x => x.Code).FirstOrDefault() ?? "";
            }
            string merror = RequestOTP(muser, "L", "", "", "", "Login");
            return Json(new { Message = merror }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ForgotPassword(string emailid)
        {
            string muserid = ctxTFAT.TfatPass.Where(x => x.Email == emailid).Select(x => x.Code).FirstOrDefault() ?? "";
            if (muserid == "")
            {
                muserid = ctxTFAT.TfatPass.Where(x => x.Mobile == emailid).Select(x => x.Code).FirstOrDefault() ?? "";
            }
            if (muserid == "")
            {
                return Json(new { Status = "Error", Message = "Invalid UserID.." }, JsonRequestBehavior.AllowGet);
            }
            string mpassword = ctxTFAT.TfatPass.Where(z => z.Code == muserid).Select(x => x.PassWords).FirstOrDefault() ?? "";
            string memail = ctxTFAT.TfatPass.Where(z => z.Code == muserid).Select(x => x.Email).FirstOrDefault();
            string mmsg = "<html>";
            mmsg += "<head>";
            mmsg += "<title>T.FAT ERPiX9, Suchan Software Pvt. Ltd.</title>";
            mmsg += "</head>";
            mmsg += "<div>";
            // suchan logo in encoded in base64 (to generate on windows command prompt : certutil -encode mypicture.png mypicture.txt)
            mmsg += "<img src = \"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAANIAAABOCAYAAAEiOnjsAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyBpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMC1jMDYwIDYxLjEzNDc3NywgMjAxMC8wMi8xMi0xNzozMjowMCAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENTNSBXaW5kb3dzIiB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOjREMDk1RUJCN0U3NzExRTM5NkExQjFDNUZENkI5OUY0IiB4bXBNTTpEb2N1bWVudElEPSJ4bXAuZGlkOjREMDk1RUJDN0U3NzExRTM5NkExQjFDNUZENkI5OUY0Ij4gPHhtcE1NOkRlcml2ZWRGcm9tIHN0UmVmOmluc3RhbmNlSUQ9InhtcC5paWQ6NEQwOTVFQjk3RTc3MTFFMzk2QTFCMUM1RkQ2Qjk5RjQiIHN0UmVmOmRvY3VtZW50SUQ9InhtcC5kaWQ6NEQwOTVFQkE3RTc3MTFFMzk2QTFCMUM1RkQ2Qjk5RjQiLz4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz5PYqN8AAA4jElEQVR42mL8//8/A70A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\" width=\"210\" height=\"78\" alt=\"SuchanSoftware.Logo\"/>";
            //mmsg += "<img src = \"http://www.suchansoftware.com/img/logo.png\" alt = \"logo\">";
            mmsg += "</div>";
            mmsg += "<br/>";
            mmsg += "<p><span style=\"font-family: arial,helvetica,sans-serif\">Hi " + (ctxTFAT.TfatPass.Where(z => z.Code == muserid).Select(x => x.Name).FirstOrDefault() ?? "") + ",</span></p>";
            mmsg += "<p><span style=\"font-family: arial,helvetica,sans-serif\">Following are your Credentials to <strong>T.FAT ERPiX9</strong>.</span></p>";
            mmsg += "<p><span style=\"font-family: arial,helvetica,sans-serif\">User ID: <strong>" + muserid + "</strong>.</span></p>";
            mmsg += "<p><span style=\"font-family: arial,helvetica,sans-serif\">Password: <strong>" + mpassword + "</strong>.</span></p>";
            mmsg += "<br/>";
            mmsg += "<p><span style=\"font-family: arial,helvetica,sans-serif\">enjoy T.FAT..Team Suchan.</span></p>";
            mmsg += "<br/>";
            mmsg += "<img src = \"data:image/png;base64,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\" width=\"113\" height=\"42\" alt=\"SuchanSoftware.Logo\"/>";
            mmsg += "<p style=\"margin: 3px;\"><span style=\"color:#9c9c9c;font-family: arial,helvetica,sans-serif;\"><strong>Suchan Software Private Limited</strong></span></p>";
            mmsg += "<p style=\"margin: 3px;\"><span style=\"color:#9c9c9c;font-family: arial,helvetica,sans-serif;font-size:11px;\">support@suchansoftware.com.</span></p>";
            //mmsg += "</body>";
            mmsg += "</html>";
            //mmsg = "Dear " + Model.TfatPass_Name +",\n"+ "Welcome to T.FAT ERPiX9.\nUse the following OTP for your Email verification in the system.\n\n" + GetRandomNumber() + "\n\nRegards from Team Suchan.\nSupport: 022 28023030. support@suchansoftware.com.";
            return SendEMail(memail, "Password from T.FAT ERPiX9", mmsg, true);
        }

        public ActionResult CompanyLogin(Complogin Model, string UserId)
        {
            if (Session["BranchCode"] != null)
            {
                Session["CompCode"] = null;
                Session["CompName"] = null;
                Session["BranchCode"] = null;
                Session["UserId"] = null;
                Session["FPerd"] = null;
                Session["LocationCode"] = null;
                Session["ActivityType"] = null;
            }
            HttpCookie mycompCookie = new HttpCookie("CompCookie");
            mycompCookie = Request.Cookies["CompCookie"];
            if (mycompCookie != null)
            {
                Model.CompCode = mycompCookie.Values["compcd"];
                Model.CompName = ctxTFAT.TfatComp.Where(x => x.Code == Model.CompCode).Select(x => x.Name).FirstOrDefault(); ;
                Model.BranchCode = mycompCookie.Values["branch"];
                Model.perd = mycompCookie.Values["period"];
            }
            Model.UserId = UserId;
            //if (String.IsNullOrEmpty(Model.BranchCode))
            {
                Model.BranchCode = ctxTFAT.TfatPass.Where(x => x.Code == UserId).Select(x => x.DefaultLoginBranch).FirstOrDefault();
            }
            var TfatPrd = ctxTFAT.TfatPerd.Where(x => x.PerdCode.Trim() == Model.perd.Trim()).FirstOrDefault();
            if (TfatPrd != null)
            {
                if (TfatPrd.Locked)
                {
                    ModelState.AddModelError("", "Selected Period Is Locked.");
                }
            }
            return View(Model);
        }

        [HttpPost]
        public ActionResult CompanyLogin(Complogin Model)
        {
            if (ModelState.IsValid)
            {
                if (Model.BranchCode == null || Model.BranchCode == "")// || Model.CompCode == null || Model.CompCode == "0"
                {
                    return View(Model);
                }
                // company code is now hardcoded - SDS
                Model.CompCode = ctxTFAT.TfatComp.Select(m => m.Code).FirstOrDefault() ?? "100";
                Model.CompName = ctxTFAT.TfatComp.Where(x => x.Code == Model.CompCode).Select(m => m.Name).FirstOrDefault();
                Model.BranchName = ctxTFAT.TfatBranch.Where(x => x.Code == Model.BranchCode).Select(m => m.Name).FirstOrDefault();
                if (Model.CompName == null || Model.CompName == "")
                {
                    return View(Model);
                }
                if (Model.BranchName == null || Model.BranchName == "")
                {
                    return View(Model);
                }
                compcookie.Values.Add("compcd", Model.CompCode.ToString());
                compcookie.Values.Add("branch", Model.BranchCode.ToString());
                compcookie.Values.Add("period", Model.perd.ToString());
                ControllerContext.HttpContext.Response.Cookies.Add(compcookie);
                compcookie.Expires = DateTime.Now.AddDays(30);
                System.Web.HttpContext.Current.Session["CompCode"] = Model.CompCode;
                System.Web.HttpContext.Current.Session["CompName"] = Model.CompName;
                System.Web.HttpContext.Current.Session["BranchCode"] = Model.BranchCode;
                System.Web.HttpContext.Current.Session["BranchName"] = Model.BranchName;
                System.Web.HttpContext.Current.Session["UserId"] = Model.UserId;
                System.Web.HttpContext.Current.Session["FPerd"] = Model.perd;
                System.Web.HttpContext.Current.Session["ActivityType"] = ctxTFAT.TfatBranch.Where(x => x.Code == Model.BranchCode).Select(m => m.BranchType).FirstOrDefault();

                int mloc = 100001;
                var TfatPrd = ctxTFAT.TfatPerd.Where(x => x.PerdCode.Trim() == Model.perd.Trim()).FirstOrDefault();
                if (TfatPrd != null)
                {
                    if (TfatPrd.Locked)
                    {
                        ModelState.AddModelError("", "Selected Period Is Locked.");
                        return RedirectToAction("CompanyLogin", new { UserId = Model.UserId });
                    }
                }
                System.Web.HttpContext.Current.Session["LocationCode"] = mloc;

                // first read modules rights from TfatBranch, then from tfatcomp
                string mModules = "";// (ctxTFAT.TfatBranch.Where(x => x.Code == Model.BranchCode).Select(m => m.Modules).FirstOrDefault() ?? "").Trim();
                string mModules2 = System.Web.HttpContext.Current.Session["Modules"].ToString();
                //(ctxTFAT.TfatComp.Where(x => x.Code == Model.CompCode).Select(m => m.Modules).FirstOrDefault() ?? "").Trim();
                if (mModules == "")
                {
                    mModules = mModules2;
                }
                else
                {
                    if (mModules.Length > 10) mModules = mModules.Substring(0, 10);
                    // if branch modules are more than company rights and match them to company rights
                    for (int x = 0; x < mModules.Length; x++)
                    {
                        if (mModules.Substring(x, 1) != "X" && mModules.Substring(x, 1) != mModules2.Substring(x, 1))
                        {
                            mModules = mModules.Substring(0, x) + mModules2.Substring(x, 1) + mModules.Substring(x + 1, mModules.Length - x - 1);
                        }
                    }
                }

                if (mModules == "")
                {
                    mModules = "AFPSIXXXXX";    // all active means AFPSIMHACP 
                }

                System.Web.HttpContext.Current.Session["Modules"] = mModules;
                System.Web.HttpContext.Current.Session["ErrorMessage"] = "";

                int mScheme = 0;
                var mUser = ctxTFAT.TfatPass.Where(x => x.Code == Model.UserId).Select(m => new { m.Modules, m.GridRows, m.MinItem, m.MinAcc, m.MinOthers, m.ColScheme, m.UserLanguage, m.AllowAlternate }).FirstOrDefault();
                if (mUser != null)
                {
                    var m = "10^15^20^50^100^500^1000^3000^5000^10000^50000".Split('^');
                    m = "10^15^20^50^100^500^1000^5000^10000^20000^50000".Split('^');
                    int mRows = mUser.GridRows.Value == null || mUser.GridRows.Value == 0 ? 1 : mUser.GridRows.Value;
                    mRows = Convert.ToInt32(m[mRows]);
                    Session["GridRows"] = mRows;
                    Session["MinItem"] = mUser.MinItem == null ? 3 : mUser.MinItem;
                    Session["MinAcc"] = mUser.MinAcc == null ? 2 : mUser.MinAcc;
                    Session["MinOthers"] = mUser.MinOthers == null ? 0 : mUser.MinItem;
                    if (Convert.ToBoolean(Session["IsBlack"]) == true)
                    {
                        Session["IsBlack"] = mUser.AllowAlternate;
                    }
                    mScheme = mUser.ColScheme ?? 0;
                    Session["Language"] = mUser.UserLanguage == null || mUser.UserLanguage == "" ? "en-US" : mUser.UserLanguage;

                    // first read modules rights from tfatpass, then from TfatBranch
                    mModules = (mUser.Modules ?? "").Trim();
                    mModules2 = System.Web.HttpContext.Current.Session["Modules"].ToString();
                    //(ctxTFAT.TfatComp.Where(x => x.Code == Model.CompCode).Select(m => m.Modules).FirstOrDefault() ?? "").Trim();
                    if (mModules == "")
                    {
                        mModules = mModules2;
                    }
                    else
                    {
                        if (mModules.Length > 10) mModules = mModules.Substring(0, 10);
                        // if user modules are more than branch rights and match them to branch rights
                        for (int x = 0; x < mModules.Length; x++)
                        {
                            if (mModules.Substring(x, 1) != "X" && mModules.Substring(x, 1) != mModules2.Substring(x, 1))
                            {
                                mModules = mModules.Substring(0, x) + mModules2.Substring(x, 1) + mModules.Substring(x + 1, mModules.Length - x - 1);
                            }
                        }
                    }

                    if (mModules == "")
                    {
                        mModules = mModules2;
                    }
                }
                else
                {
                    Session["GridRows"] = 15;
                    Session["MinItem"] = 3;
                    Session["MinAcc"] = 2;
                    Session["MinOthers"] = 0;
                    Session["IsBlack"] = false;
                    mScheme = 0;
                }
                //mDocString = GetDocumentString();
                //System.Web.HttpContext.Current.Session["MyIP"] = GetMyIP();
                SetColour(mScheme);
                //switch (mScheme)
                //{
                //    case 1: // blue
                //        System.Web.HttpContext.Current.Session["topcolor1"] = "#08697E";
                //        System.Web.HttpContext.Current.Session["topcolor2"] = "#01A0C3";
                //        System.Web.HttpContext.Current.Session["headerdivcolor"] = "#D1F9FD";
                //        System.Web.HttpContext.Current.Session["headercaptioncolor"] = "#035D88";
                //        System.Web.HttpContext.Current.Session["bottomcolor"] = "#00639C";
                //        break;
                //    case 2: // orange
                //        System.Web.HttpContext.Current.Session["topcolor1"] = "#FF7F00";
                //        System.Web.HttpContext.Current.Session["topcolor2"] = "#FDA02D";
                //        System.Web.HttpContext.Current.Session["headerdivcolor"] = "#F5E3BA";
                //        System.Web.HttpContext.Current.Session["headercaptioncolor"] = "#AF8F1C";
                //        System.Web.HttpContext.Current.Session["bottomcolor"] = "#D86400";
                //        break;
                //    case 3: // green
                //        System.Web.HttpContext.Current.Session["topcolor1"] = "#56AD2A";
                //        System.Web.HttpContext.Current.Session["topcolor2"] = "#5DC30F";
                //        System.Web.HttpContext.Current.Session["headerdivcolor"] = "#D5FFC4";
                //        System.Web.HttpContext.Current.Session["headercaptioncolor"] = "#1A8001";
                //        System.Web.HttpContext.Current.Session["bottomcolor"] = "#3D9E00";
                //        break;
                //    default:
                //        System.Web.HttpContext.Current.Session["topcolor1"] = "#4B4A4A";
                //        System.Web.HttpContext.Current.Session["topcolor2"] = "#696969";
                //        System.Web.HttpContext.Current.Session["headerdivcolor"] = "#D3D3D3";
                //        System.Web.HttpContext.Current.Session["headercaptioncolor"] = "#767676";
                //        System.Web.HttpContext.Current.Session["bottomcolor"] = "#4B4A4A";
                //        break;
                //}

                ExecuteStoredProc("Update TfatPass Set LoginNow=-1, LastLogin='" + DateTime.Now.ToString("dd/MMM/yyyy") + "' Where Code='" + Model.UserId + "'");
                var StartDate = ctxTFAT.TfatPerd.Where(x => x.PerdCode == Model.perd).ToList().Select(b => b.StartDate.Date).FirstOrDefault();
                var EndDate = ctxTFAT.TfatPerd.Where(x => x.PerdCode == Model.perd).ToList().Select(b => b.LastDate.Value.Date).FirstOrDefault();
                System.Web.HttpContext.Current.Session["StartDate"] = StartDate.Date.ToShortDateString();
                System.Web.HttpContext.Current.Session["LastDate"] = EndDate.Date.ToShortDateString();
                UpdateAuditTrail(Model.BranchCode, "Login", "Login", "", DateTime.Now, 0, "", "", "A");
            }
            else
            {
                ModelState.AddModelError("", "Please enter Details.");
            }
            return RedirectToAction("Index", "FirstPage");
        }

        public ActionResult GetCompanyList(string mIP, string UserId)
        {
            var companylist = ctxTFAT.TfatComp.ToList().Select(m => new { m.Code, m.Name });
            foreach (var item in companylist)
            {
                company.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }
            return Json(company, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBranchList(string UserId)
        {
            if (UserId == null)
            {
                return Json("", JsonRequestBehavior.AllowGet);
                //UserId = "SUPER";
            }

            UserId = UserId.ToUpper();
            if (UserId == "SUPER")
            {
                //.Where(x => x.CompCode == CompCode)
                var branchlist = ctxTFAT.TfatBranch.Where(x => x.Status == true && (x.Category != "Area" && x.Code != "G00000")).OrderBy(x => x.Code).ToList().Select(b => new { b.Code, b.Name, b.Category });
                foreach (var item in branchlist)
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
                //x.CompCode == CompCode && 
                var branchlist = ctxTFAT.TfatBranch.Where(x => x.Status == true && x.Users.ToUpper().Contains(UserId) && (x.Category != "Area" && x.Code != "G00000")).ToList().Select(b => new { b.Code, b.Name, b.Category });
                foreach (var item in branchlist)
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
            return Json(branch, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPerdList()
        {
            //Where(x => x.Code == CompCode).
            var perdlist = ctxTFAT.TfatPerd.ToList().Select(b => new { b.StartDate, b.LastDate, b.PerdCode }).OrderByDescending(x => x.PerdCode);
            foreach (var item in perdlist)
            {
                //perd.Add(new SelectListItem { Value = item.StartDate.ToShortDateString() + "-" + item.LastDate.ToShortDateString(), Text = item.StartDate.ToString("MMM yyyy") + " to " + item.LastDate.ToString("MMM yyyy") });
                perd.Add(new SelectListItem { Value = item.PerdCode, Text = item.StartDate.ToString("MMM yyyy") + " to " + item.LastDate.Value.ToString("MMM yyyy") });
            }
            return Json(perd, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RenewVerify(string userid)
        {
            var muser = ctxTFAT.TfatPass.Where(z => z.Code == userid).Select(x => new { x.Name, x.Email, x.Mobile }).FirstOrDefault();
            if (muser != null)
            {
                ExecuteStoredProc(@"Update TfatPass Set EmailVerified=0, MobileVerified=0 where Code='" + userid + "'");
                if (SendVerificationOTP(userid, muser.Name, muser.Email, muser.Mobile) == true)
                {
                    return Json(new { Status = "Success", Message = "Verification OTP is Sent to your Email and Mobile." }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Error", Message = "Failed to Renew your Request..Try again Later." }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveNewUser(UserProfileVM Model)
        {
            // valid password format, 
            try
            {
                if ((ctxTFAT.TfatPass.Where(z => z.Code == Model.TfatPass_Code || z.Name == Model.TfatPass_Name || z.Mobile == Model.TfatPass_Mobile || z.Email == Model.TfatPass_Email).Select(x => x.Code).FirstOrDefault() ?? "") != "")
                {
                    return Json(new { Status = "Error", Message = "The User ID, Name, Email or Mobile is already Used by another User.." }, JsonRequestBehavior.AllowGet);
                }
                TfatPass musers = new TfatPass();
                musers.Code = Model.TfatPass_Code;
                musers.Name = Model.TfatPass_Name;
                musers.Email = Model.TfatPass_Email;
                musers.Mobile = Model.TfatPass_Mobile;
                musers.PassWords = Model.TfatPass_PassWords;
                musers.AUTHIDS = muserid;
                musers.AUTHORISE = "A00";
                musers.ENTEREDBY = muserid;
                musers.LASTUPDATEDATE = DateTime.Now;
                musers.AppBranch = mbranchcode;
                musers.LicenseID = "";
                musers.Mon = true;
                musers.Tue = true;
                musers.Wed = true;
                musers.Thu = true;
                musers.Fri = true;
                musers.Sat = true;
                musers.Sun = true;
                musers.GridRows = 4;
                musers.Locked = true;
                ctxTFAT.TfatPass.Add(musers);
                ctxTFAT.SaveChanges();
                SendVerificationOTP(Model.TfatPass_Code, Model.TfatPass_Name, Model.TfatPass_Email, Model.TfatPass_Mobile);
                return Json(new { Status = "Success", Message = "The User is Saved Successfully.\n\nVerification OTP is Sent to your Email and Mobile." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception mex)
            {
                return Json(new { Status = "Error", mex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public bool SendVerificationOTP(string muserid, string musername, string memail, string mmobile)
        {
            try
            {
                string mCountry = (ctxTFAT.TfatComp.Select(x => x.Country).FirstOrDefault() ?? "");
                int motp;
                if (mCountry == "India")
                {
                    motp = GetRandomNumber();
                }
                else
                {
                    motp = 123456;
                }
                string mmsg = "OTP: " + motp + ", for Mobile Verification at T.FAT ERPiX9 Registration.";
                ExecuteStoredProc(@"Update RequestOTP Set IsActive=0 where UserID='" + muserid + "'");
                UpdateRequestOTP(muserid, "", "NewUserMobile", motp, 600, "", mmsg);
                if (mCountry == "India")
                {
                    SendSMS(mmobile, mmsg, true, "", true);
                }
                // compose an email and send
                motp = GetRandomNumber();
                mmsg = "OTP: " + motp + ", for Email Verification at T.FAT ERPiX9 Registration.";
                UpdateRequestOTP(muserid, "", "NewUserEmail", motp, 600, "", mmsg);
                mmsg = "<html>";
                mmsg += "<head>";
                mmsg += "<title>T.FAT ERPiX9, Suchan Software Pvt. Ltd.</title>";
                mmsg += "</head>";
                //mmsg += "<body bgcolor=\"#ffffff\">";
                //mmsg += "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"width: 500px\">";
                //mmsg += "<tbody>";
                //mmsg += "<tr>";
                //mmsg += "<td><img border=\"0\" height=\"78\" src=\"http://www.suchansoftware.com/img/logo.png\" style=\"float: left\" width=\"210\" /></td>";
                //mmsg += "<td><img border=\"0\" height=\"64\" src=\"http://www.suchansoftware.com/img/logo-info.png\" style=\"float: right\" width=\"168\" /></td>";
                //mmsg += "</tr>";
                //mmsg += "</tbody>";
                //mmsg += "</table>";
                mmsg += "<div>";
                // suchan logo in encoded in base64 (to generate on windows command prompt : certutil -encode mypicture.png mypicture.txt)
                mmsg += "<img src = \"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAANIAAABOCAYAAAEiOnjsAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyBpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMC1jMDYwIDYxLjEzNDc3NywgMjAxMC8wMi8xMi0xNzozMjowMCAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENTNSBXaW5kb3dzIiB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOjREMDk1RUJCN0U3NzExRTM5NkExQjFDNUZENkI5OUY0IiB4bXBNTTpEb2N1bWVudElEPSJ4bXAuZGlkOjREMDk1RUJDN0U3NzExRTM5NkExQjFDNUZENkI5OUY0Ij4gPHhtcE1NOkRlcml2ZWRGcm9tIHN0UmVmOmluc3RhbmNlSUQ9InhtcC5paWQ6NEQwOTVFQjk3RTc3MTFFMzk2QTFCMUM1RkQ2Qjk5RjQiIHN0UmVmOmRvY3VtZW50SUQ9InhtcC5kaWQ6NEQwOTVFQkE3RTc3MTFFMzk2QTFCMUM1RkQ2Qjk5RjQiLz4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz5PYqN8AAA4jElEQVR42mL8//8/A70A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\" width=\"210\" height=\"78\" alt=\"SuchanSoftware.Logo\"/>";
                //mmsg += "<img src = \"http://www.suchansoftware.com/img/logo.png\" alt = \"logo\">";
                mmsg += "</div>";
                mmsg += "<br/>";
                mmsg += "<p><span style=\"font-family: arial,helvetica,sans-serif\">Hi " + musername + ",</span></p>";
                mmsg += "<p><span style=\"font-family: arial,helvetica,sans-serif\">Welcome to <strong>T.FAT ERPiX9</strong>.</span></p>";
                mmsg += "<p><span style=\"font-family: arial,helvetica,sans-serif\">Use the following One Time Password for your Email verification.</span></p>";
                mmsg += "<p><span style=\"font-size:20px;font-weight:bolder;font-family: arial,helvetica,sans-serif\"><strong>" + motp + "</strong></span></p>";
                mmsg += "<p><span style=\"font-family: arial,helvetica,sans-serif\">enjoy T.FATing..Team Suchan.</span></p>";
                mmsg += "<br/>";
                mmsg += "<br/>";
                mmsg += "<img src = \"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAHEAAAAqCAYAAACEN7TkAAAABGdBTUEAALGPC/xhBQAAABl0RVh0U29mdHdhcmUAQWRvYmUgSW1hZ2VSZWFkeXHJZTwAABCGSURBVHhe7ZoHcFZVFselJECAFEggQEIKBBJIAkkoSYAQQgsEQg8lNA1ICVU6KNJD0UVFZRZsC+5YFgdHkHGFXdcCw66suroioAvYVlbdRYp0yP7+mfeYy8v3hYQSHf3ezJn3vlvPPf9zzj333O+OOzyPRwIeCXgk4JGARwIeCXgk4JGARwK/FglUio+P9x82bFjIXXfdVTctLa3yr2Xhv4R1VmQRftOnT1+7evXqwpUrVxbRihUrzk+ePDmKugolLFJ1ArsaVB2qYb31W+Ul9b1dstN6vKEqFulbZb/YR0L2nzFjxkurVq26CqAN5PLly6/k5eWFuFi9+lWNjIxsvHjx4oNLly49DegXCwoKLkMXre9TvI9OmTIlppyl5ztz5sxHxJMIfk7B4/Jy5qFcp5OWJi5btuyCDZzzTd3XLjjy6dWr11CAv+wKfHMMWTdCnF6OqwoZP378FpOHBQsWPFKO85f7VL4VK1bsVRIQAiE3N1eu0n4q+/j4NDddrwSG1V7E6j7Aqj911uk3e6xcbXk8YWPGjHnZBHHevHmPlsfEP9UcAYA48HogxsXFmSDWaNy4cT+zjwX0TBbRDOrUu3fvzSozaeHChbOp85o6dWoX3N2gWbNmDZw7d+7AOXPm1LYWX6xu4sSJdp32tEoTJkzIxKoexLJfW7JkyTu8X7///vvX3XfffYOot/ffYiAyx+NDhw5Nol0B9CR9NkK5DqFrDi8UMXv+/Pkr4PfFRYsW7dY8fD/L90zihmirj9r6oLAD7LXAVy/Kqs+ePTuf9k9AG+BvKV4h9HaDW5MJeiqIcedOYWafgwn/sLCwEU4QWcRaCRoKRDEy77777u3QDhHAvUj9VOrqI8QDJrgA2dkCQHWHzDoEkk5d5Vq1aqlun9PCbZ5Vrn2YQKwu7YuByBxvyvWbaxT/7JlHaF+kIFAQa93rbg71VR1gjqetArd47f82v2w7J1GMT50GoXrW3uJ2AqkIMi47O/spN4HN+WbNmkmTzCjTPzw8PNfZXr9Z1HkY3tS3b98Mgp5E+kVADaFgaS4UjQX+yxQ+AHezBBkNaIfNukmTJqkuEBD2OJUMCzkHcNcAw++vaN/A6U7VV/y54hngW9PHb9y4cetNANkeLgHyceY5pQDPCPZO0r4q1J36S06+XMkRvt65nSBqbD8oNTY2dgHaVGSRYgTNP0x5E0jBj/kIjI5o7Q/urNcEgnYrrc7S9hgnUAaIMbi9I2Zfzqu9O3XqNMQUrr6Tk5OnMFZPb2/vic667t2798T6tzp5Y549HTt2XO8UMkr3MGOFsN79dh+1YWx5CCliHxTvkMkXZfJgmSaI6nPvvff+p0OHDmvYg78250eu391uEGVlCjoaQ8kBAQHdoGTLepwAihe5n1BcZn+EvEtCLGlPtevQ9KZlBXHw4ME5/fr1W+6INN+3hOjPO2natGnvmi54yJAhE5wgohy7aRsJpY0dO3anOR773zrK6+CyM6C+1apV097WBvIloMsYMWJEgazeAaIU/xoQBag1RyzvEVjvVS9Bnaz3hh8B5AUoDTp37jy3a9euk4kSXZ37ijb1xMTE9C5duqyl7Qx+m8GMkwEvCrT/tPT19e2XlJQ0b9SoUdvZE864AxVB/EOKUhZLJEDKRVGuOS4g9BcsZrS2WlBzSBaTIOWCmrgA8XGrT6T2aheRqxIDQYy9g2DkWwA4byqG88hE22Ig0ueUNYcv7yys76qrvRkQi4KMbt26bUWbC/v06VPIXlVEPXv23GygIgBr0W5v//79i9qJEOBl2ul85cy6OLMfEkAAJEtrT+Q6CQ3e4XRnLOQM9Y1KAtFZl5mZOZJkwzVCx+09Z/Du1bRp09pZWVnRIiLQyLp168aUcMQIxxK3mbyx3z4mAAHhK9trWO/LuNGDgL4LcI9dzxINEOVqe9wKECV4v5SUlIcIWgqdJJAQ0GGA24DlbQKsC67aqQzr7esww+qjR4+e4jxCELUtop32yzC5WkC7JoHAIn+UlTiBuueee3pQLoWLt/dkOxJE4UaSVHjQFDrT/NWwxAD2tN0mL8OHDx/vtETjnFgMRID6LUFYjrm3wscJ5kiCtMWk4o6v7pdqdx1LvGUgKupsJstyB05py9HwCw4Qgxo1apTntDRA+i+uLwaFaJGRkTHdjOjUFsF8I2s1QbTK3xg0aFA7LOQx57FFeyJn1MnO4IV9sFPz5s3rpqenD3PWobjFApuSQESJnmT+h81xcP3H4FUexjs0NLQzCnnqp7BEHz8/v+xbAaLcb+vWreXn7aeml5dXD3fhtas9UWWAt4EBYjjA73YqgKuzmcruvPPObPqkYC1fmH0Etqt52JO174aXxRIFYv369cc6eeC4dAkwTzrLLUuUPK4JbG6HO62uKOtWgKj9FIZ1LrIfWXlUhQoVRrgC0tW5iT3lLfooAm5QuXLlXCXKXVjyFUL046bGW+fEUPoMRqBu87vqAy/nCOAUtEWUZU+UO6VPGjx+6erIJIXRnmh7CYGIJ5hVHiDKFSRyxjrgzm0SuJzF9exs27bt67S56K4drlEhuPMRqE2qVq3alzPbCrT5n+xVRVorAiQB8m/2zt/j9rJoGwRp31O/+Dp16uRxyH4XF3uWaPAU328FBgYOYv9dDh/rUZxH6buIdxjtFQnrnYlrnUta7D32we/WrFmjs+y3CHQv+3q+1UZzBMNTvsbBizyOS16JRXeyFlA3NTV1kllHVJ1BXSjepTflG/AYX1hZlq/y8/N3NWnSZBjKMZrvtzluPM94BaT/JtCnLe3XWfw+TAA2zZpDcUGyWcda7DoXonRfpMAmtEqVKrkEBsUAYp/7oVKlSjoL6ezUCtc7mXbFghsOzsdr1Kih/KWre0EJVyG+sjIK81tCdqivs1IjSMcQLcrsLyBlMeoTDymqrQdpHmV2dExoAPlDAkWP3votMHW1pTSW+iqXqbEUTNhtNX4daxzVSYHs8667OnkXrUWBjMa1+RI/mrc+FA4p+2SvSRG5ysWv+JensVN4qtManHUUle0R4zE1a9aU5l0NcLTHtWvXThkPWWtRIhdqzs1EbqtWrbZgvZ9gfR9w9ltKuQR7vRt+jSFSO5NKuogVqBK62qudCbK+3V0mq9ycy9nXlND1xnHOYY9t86W33cYey1Wfknh1V1cmJBWEZJkgap+Mjo7Wjb35CHBpr6xCJA3SgdbW7jJN6mlcegkIZW98fHPyda3JxIS70OBqygGaICpQwdrsPcKV5gq4kqyo9Bx6WpYoAe/atWtHsbd9aWdY5CbZ147GxMSYl69+Cp+dQQsH/Z0eK/tpNUwBRT0APO0qouzRo8dx9rRZ7du3H0wOVJGnywM/h/L3bmIZstSaRJO5jP8baBpRru7YXO0BsuwauPE3SYuJl7xyVKBK5Hb9yUz1sahLCV5Ga3L1p6qicoxEZ1b7KfKCkKs/g/mwxtUDBw7cWJJ8AxDYBllgabMu7lJvWK0isrI+WlRt3PIB6H+E86tQqJ2E3h+6GcgfZVqNEA74+/v3wPXPYYHvlnLS0gYGtvt3tvdhznZSHpR7B3L4hLyuPJZuaZyPH+Ub4W2uo0IBYAhnT52X7UcAhnFjUkgEbyZCNH8w632S9T5T0hrDsYAPbwZA9ZUSMM60UgrTbOaFYFK0gHr16iVQoWNKUy6PFfpX4c4ua8CAAfsRxkHAHYA7b0ta6wcA/w5r/IB5j1J/Oicn5x3er/KW0Lw4s7biWwv3RuCZ9NmM9Yym7GO+DzHeS6T7qrRp0yaeMR5A4DMoX1+9evVAxvkz7T7l/RrtzGDMnyNST4FI8kPHkg5sJW/R72Pav2Itqgrz5DDmm5R/T/nnkJ2fVRPd5iRwDjRBDKT9e5IB8+2F5hMshrG+PdAR1nqE+k0lyTaKuOSjWwEi44y5ARClmW1Y6DmBRdJgvHX7741lZ/Ln40LlOymbyvclvqfohkSWyLFlCZb4Mgv8huPMAo46axCA/iLhDyjKoV4gEAtnzG0Iey1XY3ktWrSYwt9BxiKYz9H6iVFRUcNHjhypm5gduhym/yG2j42U59FvDzytN9Z0FUQrVohgns8YZ5sssmXLljp3hmpsvNuj2mJQoD+Sk9Vh3lYGHcGSDBBlbQ3pWyDlYA0LuEnJgo+j9H0VRZvKOHuvB2IonR5QlOnOTcLkMRj5EEv71h3YWMllmBGDZX20OC2+K5mSLQjuMzIShYz3HIJYAhCfU6eDbioL2QcPT2CdmwFxF2Wx/Mt8Cbzv51tJgnQBwjsRgVySFoeEhHTlfQzLHMC1VhpgPk37gwjpe4T0FH8NGcOa/k6fkKCgoGwpDeV/gF6B/oJyHXeA2EsWwxjvM8c5AaZ5UeDfAfgbfLcQoOydnVnPy4yxijIzeeAEUcPr/NxNc5N9iuUyuTe3J1qHDvktkf2zrNe87ismY/ngNBrud+6L+k35Nup11ksiv5kDU6dctWMRBbRRkHQjj52tkQtNxVLGcYNRiKtbh2A+okzaGoVG/gkQnoGnp+U6KQtOSEiYjyC1f2qMeL4/w5Ke0ltpNZRhN/vrReriAOQkY2wDrFz4fYHvp7k2GkHbt6mv2rBhQ3AZqr9rLAL0pbyX0U6Bk/3IErPVBoUYhevtQIXOww0bNGgwTPPQfoOlYGEo/xZktdAhEFcg6gTQXiCy9rDg4OC+jHXF6hfGep+5HojaxKXpXXBPWwHpR1zEOegEtw0CRsGKoiZtvtqveilHqmgWgZ5RKg1Xp4VK20obOJjr8uL/OJEChXeILmHhYxnafoVrm3ws5jSuLZC8amuEfQLBTrW0vghEvEg+fU+Qj/QlsR3Hgp+XFmPFo0hMyGouKWiibSw5z0LcUzeySekI92OBKHdKfyXUpQQpsiLOyJMAKBqw41njYAeIWQJR/26gXBG05Kd9rg3jnFUdctN+qVjjRcq2RkRE+DFeEN8bUCx5jCQpKe8g2oZYyZI09YUfpeg6koMtZKvowNEvCSz26W+bwoC1PIZn6exQjKKfcmkyaf3PMwVqCykLIys1D+oCU7lD5SnVphUkprSIGwFQc1clSEhA0w7LTdmEhel/nMlY1Sa7DMXZLr4UreFm9e3PhXEGWntObXTvCKjTAf6sLVi5TRRtAb/jcW/b1Y72lxDK3+ROsaghFojipTFCy6H+jD0nfD1kCAwv6dtFwnacnyW/KJR5sayJb1lWMFY+VgpllSXAy2W8SxJ1iSS/r65Vc7GOdMD5Ut9y5TIgzSOLlGxw+c/SL5Q2ZwFStx5uHzFTdHnpAM/ZwW4n7b3ZbIwUQy4plgR7KsnzNIIRJbulGFKsOLktcrZSGHmFAOobI0ztF+ori2iJm5PLV/sIxrBvK4KwRpUr5ae+8YyVhsUmUd4M5VGiuaEiUt5SQlmWlFLjpeif6RYftoJKNmGWFTpTiPJETYm0ldSWTDRWJHMlMJb+5ddI1s1bhhHBWlNEeJhkyrWNKDUZz5xxlGuMZrzTxIdkQ3lREt4aQ3P97B4JSQqhhetmwBSQBKd9RIplJ47NQ7F9qLYVSn3t/nZy205sayzNYR+szaS5LRS1VTvx4Tx8a36VucoB23WmUqud5hJvdjLf5smewzQatbUNyJaH6s21uUoI/OwA9TDkkYBHAh4JeCTgkUB5S+D/4GWnSQeRmWcAAAAASUVORK5CYII=\" width=\"113\" height=\"42\" alt=\"SuchanSoftware.Logo\"/>";
                mmsg += "<p style=\"margin: 3px;\"><span style=\"color:#9c9c9c;font-family: arial,helvetica,sans-serif;\"><strong>Suchan Software Private Limited</strong></span></p>";
                mmsg += "<p style=\"margin: 3px;\"><span style=\"color:#9c9c9c;font-family: arial,helvetica,sans-serif;font-size:11px;\">306, Aura-biplex, S.V.Road, Borivali-West, Mumbai-400092. MH, India.</span></p>";
                mmsg += "<p style=\"margin: 3px;\"><span style=\"color:#9c9c9c;font-family: arial,helvetica,sans-serif;font-size:11px;\"><strong>+9122 28023030, 7700999006, 8104326546.</strong>.</span></p>";
                mmsg += "<p style=\"margin: 3px;\"><span style=\"color:#9c9c9c;font-family: arial,helvetica,sans-serif;font-size:11px;\">support@suchansoftware.com.</span></p>";
                //mmsg += "</body>";
                mmsg += "</html>";
                //mmsg = "Dear " + Model.TfatPass_Name +",\n"+ "Welcome to T.FAT ERPiX9.\nUse the following OTP for your Email verification in the system.\n\n" + GetRandomNumber() + "\n\nRegards from Team Suchan.\nSupport: 022 28023030. support@suchansoftware.com.";
                SendEMail(memail, "Email Verification from T.FAT ERPiX9", mmsg, true);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public ActionResult LogOff()
        {
            ExecuteStoredProc("Update TfatPass Set LoginNow=0, LastLogin='01-Jan-1900' Where Code='" + muserid + "'");
            Session.Clear();
            Session.Abandon();
            Session.RemoveAll();
            return RedirectToAction("Login", new { Controller = "Account" });
        }

        public string TodayiX9(string mPass)
        {
            if (mPass == "9876") return "9876";
            string mleft;
            try
            {
                int mchar = Convert.ToInt32(mPass.Substring(0, 1));
                mleft = (Convert.ToInt32(DateTime.Now.ToString("ddMMyyyy")) * mchar).ToString().PadLeft(10, '0');
                string mright = mleft.Substring(5);
                mleft = mleft.Substring(0, 5);
                mleft = mchar.ToString() + (Convert.ToInt32(mleft) + Convert.ToInt32(mright)).ToString();
            }
            catch
            {
                mleft = "!@#$%^&*(&$^^#$";
            }
            return mleft;
        }
    }
}