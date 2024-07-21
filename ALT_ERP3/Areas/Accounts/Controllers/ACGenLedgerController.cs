using ALT_ERP3.Areas.Accounts.Models;
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
using System.Text;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class ACGenLedgerController : BaseController
    {
        //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        //IAddOnGridList AOlst = new AddonGridlist();

        private new int mlocationcode = 100001;
        ////private IBusinessCommon mIBusi = new BusinessCommon();
        //private string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        //private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        //private string muserid = System.Web.HttpContext.Current.Session["UserId"].ToString();
        //private string mperiod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        private static string mmaintype = "";
        public static string connstring = "";
        //string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings[""].ConnectionString) == true ? "" : ConfigurationManager.ConnectionStrings[""].ConnectionString;
        private static bool mAutoAccCode = false;
        private static byte mAutoAccStyle = 0;
        private static byte mAutoAccLength = 9;

        List<int> list = new List<int>();
        List<MasterVM> MailList1 = new List<MasterVM>();
        public ActionResult GetRelatedTo(string term)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            string[] DefaultCost = new string[] { "000100949", "000100950", "000100811", "000100343", "000100736", "000100653", "000100326", "000100951", "000100952", "000100404", "000100781", "000100953", "000100954", "000100788", "000101135", "000100789", "000100346", "000100341", "000100345" };
            var mVehicles = ctxTFAT.Master.Where(x => (DefaultCost.Contains(x.Code))).OrderBy(n => n.Name).ToList();
            foreach (var item in mVehicles)
            {
                items.Add(new SelectListItem { Value = item.Code, Text = item.Name });
            }
            items.Add(new SelectListItem { Value = "LR", Text = "LR" });
            items.Add(new SelectListItem { Value = "FM", Text = "FM" });
            items.Add(new SelectListItem { Value = "Branch", Text = "Branch" });
            items.Add(new SelectListItem { Value = "NA", Text = "NA" });


            if (term == "" || term == null)
            {
                var Modified = items.Select(x => new
                {
                    Code = x.Value,
                    Name = x.Text
                }).ToList();

                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
            else
            {
                items = items.Where(x => x.Text.ToLower().Contains(term.ToLower())).OrderBy(n => n.Text).ToList();
                var Modified = items.Select(x => new
                {
                    Code = x.Value,
                    Name = x.Text
                }).ToList();

                return Json(Modified, JsonRequestBehavior.AllowGet);
            }






            //items.Add(new SelectListItem
            //{
            //    Text = "NA",
            //    Value = "0"
            //});
            //items.Add(new SelectListItem
            //{
            //    Text = "Vehicle Expenses Cost Center",
            //    Value = "1"
            //});
            //items.Add(new SelectListItem
            //{
            //    Text = "Branch",
            //    Value = "2"
            //});
            //items.Add(new SelectListItem
            //{
            //    Text = "LR",
            //    Value = "3"
            //});
            //items.Add(new SelectListItem
            //{
            //    Text = "Vehicle No Cost Center",
            //    Value = "4"
            //});
            //items.Add(new SelectListItem
            //{
            //    Text = "FM",
            //    Value = "5"
            //});
            //items.Add(new SelectListItem
            //{
            //    Text = "Other",
            //    Value = "8"
            //});
            //items.Add(new SelectListItem
            //{
            //    Text = "Broker",
            //    Value = "6"
            //});
            //items.Add(new SelectListItem
            //{
            //    Text = "Driver",
            //    Value = "7"
            //});

            //return items;
        }
        #region Save Index Edit
        // GET: SetUp/ACGenLedger
        public ActionResult Index(MasterVM Model)
        {
            connstring = GetConnectionString();
            //GetAllMenu(Session["ModuleName"].ToString());
            ////AccountingGetAllMenu(Session["ModuleName"].ToString());
            ////VehicleGetAllMenu(Session["ModuleName"].ToString());
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "A");

            List<MasterVM> AddressList1 = new List<MasterVM>();
            Session["MailInfo"] = null;
            Model.id = Model.Header;
            Model.Form15HDate = DateTime.Now;
            Model.Form15HCITDate = DateTime.Now;
            Model.ChildBdate = DateTime.Now;
            Model.SpouseBdate = DateTime.Now;
            Model.Anndate = DateTime.Now;
            Model.Bdate = DateTime.Now;
            Model.HoldDespatchDt = DateTime.Now;
            Model.HoldEnquiryDt = DateTime.Now;
            Model.HoldEnquiryDt = DateTime.Now;
            Model.HoldInvoiceDt = DateTime.Now;

            mmaintype = Model.MainType;
            Model.Branches = PopulateBranches();
            // preferences from TfatBranch
            var mpara = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => new
            {
                x.gp_AutoAccCode,
                x.gp_AutoAccStyle,
                x.gp_AutoAccLength
            }).FirstOrDefault();
            mAutoAccCode = mpara.gp_AutoAccCode;
            mAutoAccStyle = (byte)(mpara.gp_AutoAccStyle == null ? 0 : mpara.gp_AutoAccStyle.Value);
            mAutoAccLength = (byte)(mpara.gp_AutoAccLength == null ? 6 : mpara.gp_AutoAccLength.Value);
            Model.AutoAccCode = mpara.gp_AutoAccCode;

            TfatComp tfatComp = ctxTFAT.TfatComp.Where(x => x.Code == mcompcode).FirstOrDefault();

            var countryname = ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == tfatComp.Country).FirstOrDefault();
            var statename = ctxTFAT.TfatState.Where(x => x.Code.ToString() == tfatComp.State).FirstOrDefault();
            var cityname = ctxTFAT.TfatCity.Where(x => x.Code.ToString() == tfatComp.City).FirstOrDefault();
            if (Model.Mode == "Add")
            {
                AddressList1.Add(new MasterVM
                {
                    Ifexist = true,
                    SrNo = 0,
                    AName = "",
                    CorpID = mcompcode,
                    Person = "",
                    Adrl1 = "",
                    Adrl2 = "",
                    Adrl3 = "",
                    Country = countryname.Code.ToString(),
                    CountryName = countryname.Name,
                    State = statename.Code.ToString(),
                    StateName = statename.Name,
                    City = cityname.Code.ToString(),
                    CityName = cityname.Name,
                    Pin = "",
                    Area = 0,
                    AreaName = "",
                    Tel1 = "",
                    Fax = "",
                    Mobile = "",
                    www = "",
                    Email = "",
                    MailingCategory = 0,
                    UserID = muserid,
                    CorrespondenceType = 0,
                    Password = "",
                    Source = 0,
                    Segment = "",
                    STaxCode = "",
                    PTaxCode = "",
                    Licence1 = "",
                    Licence2 = "",
                    PanNo = "",
                    TINNo = "",
                    Designation = 0,
                    Language = 0,
                    Dept = 0,
                    Religion = 0,
                    Division = 0,
                    Bdate = DateTime.Now,
                    Anndate = DateTime.Now,
                    SpouseName = "",
                    SpouseBdate = DateTime.Now,
                    ChildName = "",
                    ChildBdate = DateTime.Now,
                    Code = "",
                    ContactType = "1",
                    AssistEmail = "",
                    AssistMobile = "",
                    AssistTel = "",
                    AssistName = "",
                    DefaultIGst = 0,
                    DefaultSGst = 0,
                    DefaultCGst = 0,
                    VATReg = "",
                    AadharNo = "",
                    GSTNo = "",
                    GSTType = "0",
                    PoisonLicense = "",
                    ReraNo = "",
                    DealerType = "0"
                });
                Model.AddressList = AddressList1;

                if (AddressList1 != null)
                {
                    Model.Ifexist = true;
                    Model.SrNo = 0;
                    Model.AName = "";
                    Model.CorpID = mcompcode;
                    Model.Person = "";
                    Model.Adrl1 = "";
                    Model.Adrl2 = "";
                    Model.Adrl3 = "";
                    Model.Country = countryname.Code.ToString();
                    Model.CountryName = countryname.Name;
                    Model.State = statename.Code.ToString();
                    Model.StateName = statename.Name;
                    Model.City = cityname.Code.ToString();
                    Model.CityName = cityname.Name;
                    Model.Pin = "";
                    Model.Area = 0;
                    Model.AreaName = "";
                    Model.Tel1 = "";
                    Model.Fax = "";
                    Model.Mobile = "";
                    Model.www = "";
                    Model.Email = "";
                    Model.MailingCategory = 0;
                    Model.UserID = muserid;
                    Model.CorrespondenceType = 0;
                    Model.Password = "";
                    Model.Source = 0;
                    Model.Segment = "";
                    Model.STaxCode = "";
                    Model.PTaxCode = "";
                    Model.Licence1 = "";
                    Model.Licence2 = "";
                    Model.PanNo = "";
                    Model.TINNo = "";
                    Model.Designation = 0;
                    Model.Language = 0;
                    Model.Dept = 0;
                    Model.Religion = 0;
                    Model.Division = 0;
                    Model.Bdate = DateTime.Now;
                    Model.Anndate = DateTime.Now;
                    Model.SpouseName = "";
                    Model.SpouseBdate = DateTime.Now;
                    Model.ChildName = "";
                    Model.ChildBdate = DateTime.Now;
                    Model.Code = "";
                    Model.ContactType = "1";
                    Model.AssistEmail = "";
                    Model.AssistMobile = "";
                    Model.AssistTel = "";
                    Model.AssistName = "";
                    Model.DefaultIGst = 0;
                    Model.DefaultSGst = 0;
                    Model.DefaultCGst = 0;
                    Model.VATReg = "";
                    Model.AadharNo = "";
                    Model.GSTNo = "";
                    Model.GSTType = "0";
                    Model.PoisonLicense = "";
                    Model.ReraNo = "";
                    Model.DealerType = "0";
                    Model.Tel2 = "";
                    Model.Tel3 = "";
                }

                Model.MailList = AddressList1;
                Session.Add("MailInfo", AddressList1);

                Model.AllBranch = true; ;



                List<AddOns> objitemlist = new List<AddOns>();
                var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%Master" && x.Hide == false)
                              select i).ToList();

                foreach (var i in addons)
                {
                    AddOns c = new AddOns();
                    c.Fld = i.Fld;
                    c.Head = i.Head;
                    c.ApplCode = "";
                    c.QueryText = i.QueryText == null ? "" : GetQueryText(i.QueryText);
                    c.FldType = i.FldType;
                    c.PlaceValue = i.PlaceValue;
                    c.Eqsn = i.Eqsn;
                    objitemlist.Add(c);
                }
                Model.AddOnList = objitemlist;

                //Model.RelatedToList = PopulateRelatedToLists();

            }


            if (Model.Mode != "Add")
            {
                var master = ctxTFAT.Master.Where(x => x.Code == Model.Document.Trim()).Select(x => x).FirstOrDefault();
                var masterinfo = ctxTFAT.MasterInfo.Where(x => x.Code == Model.Document.Trim()).Select(x => x).FirstOrDefault();
                var holdtrx = ctxTFAT.HoldTransactions.Where(x => x.Code == Model.Document.Trim()).Select(x => x).FirstOrDefault();
                var address = ctxTFAT.Address.Where(x => x.Code == Model.Document.Trim()).Select(x => x).ToList();
                var FirstAddress = ctxTFAT.Address.Where(x => x.Code == Model.Document.Trim() && x.Sno == 0).Select(x => x).FirstOrDefault();
                var taxdetails = ctxTFAT.TaxDetails.Where(x => x.Code == Model.Document.Trim()).Select(x => x).FirstOrDefault();

                if (!String.IsNullOrEmpty(master.OthPostType))
                {

                    var OthPostType = master.OthPostType.Split(',');

                    foreach (var item in OthPostType)
                    {
                        if (item == "B")
                        {
                            Model.Brokr = true;
                        }
                        else if (item == "V")
                        {
                            Model.Vehicle = true;
                        }
                        else if (item == "D")
                        {
                            Model.Driver = true;
                        }
                    }
                }
                if (FirstAddress != null)
                {
                    Model.SrNo = FirstAddress.Sno;
                    Model.AName = FirstAddress.Name;
                    Model.CorpID = FirstAddress.CorpID;
                    Model.Person = FirstAddress.Person;
                    Model.Adrl1 = FirstAddress.Adrl1;
                    Model.Adrl2 = FirstAddress.Adrl2;
                    Model.Adrl3 = FirstAddress.Adrl3;
                    Model.Country = FirstAddress.Country;
                    Model.CountryName = ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == FirstAddress.Country).Select(x => x.Name).FirstOrDefault();
                    Model.State = FirstAddress.State;
                    Model.StateName = ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == FirstAddress.State).Select(x => x.Name).FirstOrDefault();
                    Model.City = FirstAddress.City;
                    Model.CityName = ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == FirstAddress.City).Select(x => x.Name).FirstOrDefault();
                    Model.Pin = FirstAddress.Pin;
                    Model.Area = (FirstAddress.Area == null) ? 0 : FirstAddress.Area.Value;
                    Model.AreaName = (FirstAddress.Area == null) ? "0" : GetAreaName(FirstAddress.Area.Value);
                    Model.Tel1 = FirstAddress.Tel1;
                    Model.Fax = FirstAddress.Fax;
                    Model.Mobile = FirstAddress.Mobile;
                    Model.www = FirstAddress.www;
                    Model.Email = FirstAddress.Email;
                    Model.MailingCategory = (FirstAddress.MailingCategory == null) ? 0 : FirstAddress.MailingCategory.Value;
                    Model.UserID = FirstAddress.UserID;
                    Model.CorrespondenceType = (FirstAddress.CorrespondenceType == null) ? 0 : FirstAddress.CorrespondenceType.Value;
                    Model.Password = FirstAddress.Password;
                    Model.Source = (FirstAddress.Source == null) ? 0 : FirstAddress.Source.Value;
                    Model.Segment = FirstAddress.Segment;
                    Model.STaxCode = FirstAddress.STaxCode;
                    Model.PTaxCode = FirstAddress.PTaxCode;
                    Model.Licence1 = FirstAddress.Licence1;
                    Model.Licence2 = FirstAddress.Licence2;
                    Model.PanNo = FirstAddress.PanNo;
                    Model.TINNo = FirstAddress.TINNo;
                    Model.Designation = (FirstAddress.Designation == null) ? 0 : FirstAddress.Designation.Value;
                    Model.Language = (FirstAddress.Language == null) ? 0 : FirstAddress.Language.Value;
                    Model.Dept = (FirstAddress.Dept == null) ? 0 : FirstAddress.Dept.Value;
                    Model.Religion = (FirstAddress.Religion == null) ? 0 : FirstAddress.Religion.Value;
                    Model.Division = (FirstAddress.Division == null) ? 0 : FirstAddress.Division.Value;
                    Model.Bdate = (FirstAddress.BDate == null) ? Convert.ToDateTime("1900-01-01") : FirstAddress.BDate.Value;
                    Model.Anndate = (FirstAddress.AnnDate == null) ? Convert.ToDateTime("1900-01-01") : FirstAddress.BDate.Value;
                    Model.SpouseName = FirstAddress.SpouseName;
                    Model.SpouseBdate = (FirstAddress.SpouseBdate == null) ? Convert.ToDateTime("1900-01-01") : FirstAddress.BDate.Value;
                    Model.ChildName = FirstAddress.ChildName;
                    Model.ChildBdate = (FirstAddress.ChildBdate == null) ? Convert.ToDateTime("1900-01-01") : FirstAddress.BDate.Value;
                    Model.Code = FirstAddress.Code;
                    Model.ContactType = (FirstAddress.AddOrContact == null) ? "0" : FirstAddress.AddOrContact.Value.ToString();
                    Model.AssistEmail = FirstAddress.AssistEmail;
                    Model.AssistMobile = FirstAddress.AssistMobile;
                    Model.AssistTel = FirstAddress.AssistTel;
                    Model.AssistName = FirstAddress.AssistName;
                    Model.DefaultIGst = (FirstAddress.IGSTRate == null) ? 0 : FirstAddress.IGSTRate.Value;
                    Model.DefaultSGst = (FirstAddress.SGSTRate == null) ? 0 : FirstAddress.SGSTRate.Value;
                    Model.DefaultCGst = (FirstAddress.CGSTRate == null) ? 0 : FirstAddress.CGSTRate.Value;
                    Model.VATReg = FirstAddress.VATReg;
                    Model.AadharNo = FirstAddress.AadharNo;
                    Model.GSTNo = FirstAddress.GSTNo;
                    Model.GSTType = (FirstAddress.GSTType == null) ? "0" : FirstAddress.GSTType.Value.ToString();
                    Model.PoisonLicense = "";
                    Model.ReraNo = FirstAddress.ReraRegNo;
                    Model.DealerType = (FirstAddress.DealerType == null) ? "0" : FirstAddress.DealerType.Value.ToString();
                    Model.Tel2 = FirstAddress.Tel2;
                    Model.Tel3 = FirstAddress.Tel3;
                    Model.DraweeBank = FirstAddress.DraweeBank == null ? 0 : FirstAddress.DraweeBank.Value;
                }

                Model.FetchBalAcc = master.FetchBalAcc;
                Model.AddressList = AddressList1;
                Model.Name = master.Name;
                Model.ShortName = master.ShortName;
                Model.Name = master.Name;
                Model.Code = master.Code;
                Model.Grp = master.Grp;
                Model.GrpName = GetGrpName(master.Grp);
                Model.SalesMan = (int)(master.SalesMan != null ? master.SalesMan : 0);
                Model.SalesManName = GetSalesManName(master.SalesMan);
                Model.Broker = (int)(master.Broker != null ? master.Broker : 0);
                Model.BrokerName = GetBrokerName(master.Broker);
                Model.AcHeadCode = master.AcHeadCode;
                Model.AutoEmail = master.AutoEmail;
                Model.AutoSMS = master.AutoSMS;
                Model.IsPublic = master.IsPublic;
                Model.AcType = master.BaseGr;
                Model.Category = (master.Category == null) ? 0 : master.Category.Value;
                Model.CCBudget = master.CCBudget;
                Model.CCReqd = master.ForceCC;
                Model.ARAP = master.ARAP;
                Model.Hide = master.Hide;
                Model.NonActive = master.NonActive;
                Model.IsSubLedger = master.IsSubLedger;
                Model.AdminUser = master.UserID;
                Model.ReferAcReq = master.ReferAccReq;
                Model.CostCenterTally = master.CostCenterAmtTally;

                if (master.AppBranch == "All")
                {
                    Model.AllBranch = true;
                }
                else
                {
                    Model.AppBranch = master.AppBranch;
                }
                Model.AllowtoChange = master.AllowToChanges;
                Model.RelatedTo = master.RelatedTo;
                Model.RelatedToN = ctxTFAT.Master.Where(x => x.Code == Model.RelatedTo).Select(x => x.Name).FirstOrDefault();
                if (String.IsNullOrEmpty(Model.RelatedToN))
                {
                    Model.RelatedToN = Model.RelatedTo;
                }
                //Model.RelatedToList = PopulateRelatedToLists();
                if (masterinfo != null)
                {
                    Model.FreqOS = (masterinfo.FreqOS == null) ? (byte)0 : Convert.ToByte(masterinfo.FreqOS.Value);
                    Model.FreqForm = (masterinfo.FreqForm == null) ? (byte)0 : Convert.ToByte(masterinfo.FreqForm.Value);
                    Model.EmailParty = masterinfo.EmailParty;
                    Model.EmailSalesman = masterinfo.EmailSalesman;
                    Model.SMSParty = masterinfo.SMSParty;
                    Model.SMSSalesman = masterinfo.SMSSalesman;
                    Model.CurrCode = masterinfo.CurrName.Value;
                    Model.CurrName = NameofAccount(masterinfo.CurrName.Value, "C");
                    Model.RTGS = masterinfo.RTGS;
                    Model.Brokerage = (masterinfo.Brokerage == null) ? 0 : masterinfo.Brokerage.Value;
                    Model.Transporter = masterinfo.Transporter;
                    Model.ReminderFormat = masterinfo.ReminderFormat;
                    Model.Narr = masterinfo.Narr;
                    Model.IncoTerms = masterinfo.IncoTerms == null ? 0 : masterinfo.IncoTerms.Value;
                    Model.CheckCRDays = masterinfo.CheckCRDays;
                    Model.CheckCRLimit = masterinfo.CheckCRLimit;
                    Model.CRLimitWarn = (masterinfo.CRLimitWarn == true) ? 1 : 0;
                    Model.CRLimitWithTrx = masterinfo.CRLimitWithTrx;
                    Model.CRLimitWithPO = masterinfo.CRLimitWithPO;
                    Model.CRDaysWarn = (masterinfo.CRDaysWarn == true) ? 1 : 0;
                    Model.CashDisc = masterinfo.CashDisc;
                    Model.DiscDays = (masterinfo.DiscDays == null) ? 0 : masterinfo.DiscDays.Value;
                    Model.CrLimit = (masterinfo.CrLimit == null) ? 0 : masterinfo.CrLimit.Value;
                    Model.DiscPerc = (masterinfo.DiscPerc == null) ? 0 : masterinfo.DiscPerc.Value;
                    Model.CutTDS = masterinfo.CutTDS;
                    Model.TDSCode = (masterinfo.TDSCode == null) ? 0 : masterinfo.TDSCode.Value;
                    Model.CRLimitTole = (masterinfo.CRLimitTole == null) ? 0 : masterinfo.CRLimitTole.Value;
                    Model.CRPeriod = (masterinfo.CrPeriod == null) ? 0 : masterinfo.CrPeriod.Value;
                    Model.EmailPartyAlert = masterinfo.EmailPartyAlert;
                    Model.EmailTemplate = masterinfo.EmailTemplate;
                    Model.PaymentTerms = masterinfo.PaymentTerms;
                    Model.Rank = (masterinfo.Rank == null) ? (byte)0 : Convert.ToByte(masterinfo.Rank.Value);
                    Model.SMSTemplate = masterinfo.SMSTemplate;
                    Model.IncoPlace = masterinfo.IncoPlace;

                    Model.ODLimit = (masterinfo.ODLImit == null) ? 0 : masterinfo.ODLImit.Value;
                    Model.HSN = masterinfo.HSNCode;
                    Model.HSNName = GetHSNName(masterinfo.HSNCode);
                    if (String.IsNullOrEmpty(Model.HSNName))
                    {
                        Model.HSNName = GetGSTName(masterinfo.HSNCode);
                    }
                    Model.OptionGstType = (masterinfo.GSTType == null) ? "" : masterinfo.GSTType.Value.ToString();
                    Model.OptionGSTName = (masterinfo.GSTType == null) ? "" : GetGSTTypeName(masterinfo.GSTType.Value.ToString());
                    Model.AcCode = masterinfo.DrAcNo;
                    Model.SGST = (masterinfo.SGSTRate == null) ? 0 : masterinfo.SGSTRate.Value;
                    Model.IGST = (masterinfo.IGSTRate == null) ? 0 : masterinfo.IGSTRate.Value;
                    Model.CGST = (masterinfo.CGSTRate == null) ? 0 : masterinfo.CGSTRate.Value;
                    Model.GstApplicable = masterinfo.GSTFlag;
                    Model.ItemType = masterinfo.ItemType;
                    Model.EmailPartyAlert = masterinfo.EmailPartyAlert;
                    Model.SMSPartyAlert = masterinfo.SMSPartyAlert;
                    Model.IntRate = (masterinfo.IntRate == null) ? 0 : masterinfo.IntRate.Value;
                    Model.BSRCode = masterinfo.BSRCode;

                }
                if (holdtrx != null)
                {
                    Model.Ticklers = holdtrx.Ticklers;
                    Model.HoldDespatch = holdtrx.HoldDespatch;
                    Model.HoldEnquiry = holdtrx.HoldEnquiry;
                    Model.HoldInvoice = holdtrx.HoldInvoice;
                    Model.HoldDespatchDt = (holdtrx.HoldDespatchDt1 == null) ? Convert.ToDateTime("1900-01-01") : holdtrx.HoldDespatchDt1.Value;
                    Model.HoldEnquiryDt = (holdtrx.HoldEnquiryDt1 == null) ? Convert.ToDateTime("1900-01-01") : holdtrx.HoldEnquiryDt1.Value;
                    Model.HoldInvoiceDt = (holdtrx.HoldInvoiceDt1 == null) ? Convert.ToDateTime("1900-01-01") : holdtrx.HoldInvoiceDt1.Value;
                    Model.HoldNarr = holdtrx.HoldNarr;

                }
                if (taxdetails != null)
                {
                    Model.CutTCS = taxdetails.CutTCS;
                    Model.CutTDS = taxdetails.CutTDS;

                    Model.DifferRate = (decimal)taxdetails.DifferRate;
                    Model.DifferRateCertNo = taxdetails.DifferRateCertNo;

                    Model.Form15HCITDate = (taxdetails.Form15HCITDate == null) ? Convert.ToDateTime("1900-01-01") : taxdetails.Form15HCITDate.Value;
                    Model.Form15HDate = (taxdetails.Form15HDate == null) ? Convert.ToDateTime("1900-01-01") : taxdetails.Form15HDate.Value;
                    Model.IsDifferRate = taxdetails.IsDifferRate;
                    Model.IsForm15H = taxdetails.IsForm15H;

                    Model.TDSCode = (taxdetails.TDSCode == null) ? 0 : taxdetails.TDSCode.Value;
                }

                Model.AddOnList = GetAddOnListOnEdit(Model.Code);

            }
            return View(Model);

        }

        [HttpPost]
        public ActionResult SaveData(MasterVM Model)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    if (Model.Mode == "Add")
                    {
                        if (mAutoAccCode == true)
                        {
                            Model.Code = GetCode(mAutoAccStyle, mAutoAccLength, Model.Grp);
                        }
                        else if (Model.Document == null || Model.Document.Trim() == "")
                        {
                            return Json(new { Status = "Error", Message = "Account Code is Required.." }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            Model.Code = Model.Document;
                        }
                    }
                    else
                    {
                        Model.Code = Model.Document;
                    }
                    var delmasterinfo = ctxTFAT.MasterInfo.Where(x => x.Code == Model.Document.Trim()).FirstOrDefault();
                    var deladdress = ctxTFAT.Address.Where(x => x.Code == Model.Document.Trim()).ToList();
                    var delholdtrx = ctxTFAT.HoldTransactions.Where(x => x.Code == Model.Document.Trim()).FirstOrDefault();
                    var deltaxdet = ctxTFAT.TaxDetails.Where(x => x.Code == Model.Document.Trim()).FirstOrDefault();
                    var deltaddons = ctxTFAT.AddonValues.Where(x => x.TableKey == Model.Document.Trim()).ToList();
                    //var CustomerMaster = ctxTFAT.CustomerMaster.Where(x => x.Code == Model.Document).FirstOrDefault();
                    if (delmasterinfo != null)
                    {
                        ctxTFAT.MasterInfo.Remove(delmasterinfo);
                    }
                    if (deladdress != null)
                    {
                        ctxTFAT.Address.RemoveRange(deladdress);
                    }
                    if (delholdtrx != null)
                    {
                        ctxTFAT.HoldTransactions.Remove(delholdtrx);
                    }
                    if (deltaxdet != null)
                    {
                        ctxTFAT.TaxDetails.Remove(deltaxdet);
                    }
                    if (deltaddons.Count > 0)
                    {
                        ctxTFAT.AddonValues.RemoveRange(deltaddons);
                    }

                    ctxTFAT.SaveChanges();

                    Master mobj = new Master();
                    //CustomerMaster customer = new CustomerMaster();
                    if (Model.Mode == "Edit")
                    {
                        mobj = ctxTFAT.Master.Where(x => x.Code == Model.Code.Trim()).Select(x => x).FirstOrDefault();
                        //customer = ctxTFAT.CustomerMaster.Where(x => x.Code == Model.Code).Select(x => x).FirstOrDefault();
                    }
                    mobj.Code = Model.Code;
                    mobj.Grp = Model.Grp;
                    mobj.Name = Model.Name;
                    mobj.ForceCC = Model.CCReqd;
                    mobj.AcHeadCode = (Model.AcHeadCode == null) ? "" : Model.AcHeadCode;
                    mobj.AcType = (Model.AcType == null) ? "" : Model.AcType;
                    mobj.ARAP = Model.ARAP;
                    mobj.AUTHIDS = muserid;
                    mobj.ReferAccReq = Model.ReferAcReq;
                    mobj.CostCenterAmtTally = Model.CostCenterTally;





                    if (!(Model.Brokr == false && Model.Vehicle == false && Model.Driver == false))
                    {
                        string OthPostType = "";
                        if (Model.Brokr)
                        {
                            //
                            OthPostType += "B" + ",";
                        }
                        if (Model.Vehicle)
                        {
                            //Add Vehicle
                            OthPostType += "V" + ",";
                        }
                        if (Model.Driver)
                        {
                            //Add Driver
                            OthPostType += "D" + ",";
                        }
                        mobj.OthPostType = OthPostType.Substring(0, OthPostType.Length - 1);
                    }
                    else
                    {
                        mobj.OthPostType = "";
                    }
                    if (Model.AllBranch)
                    {
                        mobj.AppBranch = "All";
                    }
                    else
                    {
                        mobj.AppBranch = Model.AppBranch;
                    }

                    mobj.FetchBalAcc = Model.FetchBalAcc;
                    mobj.AUTHORISE = "A00";
                    mobj.BaseGr = (Model.AcType == null) ? "" : Model.AcType;
                    mobj.Category = (Model.Category == 0) ? 0 : Model.Category;
                    mobj.CCBudget = Model.CCBudget;
                    mobj.Hide = Model.Hide;
                    mobj.IsPublic = Model.IsPublic;
                    mobj.NonActive = Model.NonActive;
                    mobj.ShortName = (Model.ShortName == null) ? "" : Model.ShortName;
                    mobj.SalesMan = Model.SalesMan;
                    mobj.Broker = Model.Broker;
                    mobj.ENTEREDBY = muserid;
                    mobj.GroupTree = GetGroupTree(Model.Grp);
                    mobj.IsSubLedger = Model.IsSubLedger;
                    mobj.LASTUPDATEDATE = DateTime.Now;
                    mobj.UserID = (Model.AdminUser == null) ? "" : Model.AdminUser;
                    mobj.AllowToChanges = Model.AllowtoChange;
                    mobj.RelatedTo = Model.RelatedTo;


                    //customer.AccountParentGroup = Model.Code;
                    //customer.AcHeadCode = (Model.AcHeadCode == null) ? "" : Model.AcHeadCode;
                    //customer.AcType = (Model.AcType == null) ? "" : Model.AcType;
                    //customer.AppBranch = Model.AppBranch;
                    //customer.ARAP = Model.ARAP;
                    //customer.BaseGr = (Model.AcType == null) ? "" : Model.AcType;
                    //customer.Broker = Model.Broker;
                    //customer.Category = (Model.Category == null) ? 0 : Model.Category;
                    //customer.CCBudget = Model.CCBudget;
                    //customer.Code = Model.Code;
                    //customer.ForceCC = Model.CCReqd;
                    //customer.GroupTree = GetGroupTree(Model.Grp);
                    //customer.Grp = Model.Grp;
                    //customer.Hide = Model.Hide;
                    //customer.IsPublic = Model.IsPublic;
                    //customer.IsSubLedger = Model.IsSubLedger;
                    //customer.Name = Model.Name;
                    //customer.NonActive = Model.NonActive;
                    //customer.SalesMan = Model.SalesMan;
                    //customer.ShortName = (Model.ShortName == null) ? "" : Model.ShortName;
                    //customer.UserID = (Model.AdminUser == null) ? "" : Model.AdminUser;
                    //customer.ENTEREDBY = muserid;
                    //customer.LASTUPDATEDATE = DateTime.Now;
                    //customer.AUTHIDS = muserid;
                    //customer.AUTHORISE = "A00";


                    if (Model.Mode == "Edit")
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                        //ctxTFAT.Entry(customer).State = EntityState.Modified;
                    }
                    else
                    {
                        mobj.CreateDate = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                        ctxTFAT.Master.Add(mobj);
                        //ctxTFAT.CustomerMaster.Add(customer);
                    }

                    MasterInfo mobj2 = new MasterInfo();
                    mobj2.AppProduct = "";
                    mobj2.Area = null;
                    mobj2.AUTHIDS = muserid;
                    mobj2.AUTHORISE = "A00";
                    mobj2.BaseGr = (Model.AcType == null) ? "" : Model.AcType;
                    mobj2.Brokerage = 0;
                    mobj2.CashDisc = Model.CashDisc;
                    mobj2.CheckCRDays = Model.CheckCRDays;
                    mobj2.CheckCRLimit = Model.CheckCRLimit;
                    mobj2.Code = Model.Code;
                    mobj2.CompanyType = "";
                    mobj2.CostCentre = null;
                    mobj2.CRDaysWarn = (Model.CRDaysWarn == 0) ? false : true;
                    mobj2.CreatedOn = DateTime.Now;
                    mobj2.CrLimit = Model.CrLimit;
                    mobj2.CRLimitTole = Model.CRLimitTole;
                    mobj2.CRLimitWarn = (Model.CRLimitWarn == 0) ? false : true;
                    mobj2.CRLimitWithPO = Model.CRLimitWithPO;
                    mobj2.CRLimitWithTrx = Model.CRLimitWithTrx;
                    mobj2.CrPeriod = Convert.ToInt32(Model.CRPeriod);
                    mobj2.CurrName = Convert.ToInt32(Model.CurrCode);
                    mobj2.CutTDS = Model.CutTDS;
                    mobj2.DepricAC = "";
                    mobj2.DiscDays = Model.DiscDays;
                    mobj2.DiscPerc = Model.DiscPerc;
                    mobj2.EmailParty = Model.EmailParty;
                    mobj2.EmailPartyAlert = Model.EmailPartyAlert;
                    mobj2.EmailSalesman = Model.EmailSalesman;
                    mobj2.EmailTemplate = Model.EmailTemplate;
                    mobj2.EmailUsers = "";
                    mobj2.FreqOS = (Model.FreqOS == null) ? 0 : Convert.ToInt32(Model.FreqOS);
                    mobj2.SGSTRate = Model.SGST;
                    mobj2.IGSTRate = Model.IGST;
                    mobj2.CGSTRate = Model.CGST;
                    mobj2.FreqForm = (Model.FreqForm == null) ? 0 : Convert.ToInt32(Model.FreqForm);
                    mobj2.Grp = Model.Grp;
                    mobj2.IntAmt = Convert.ToDecimal(0.00);
                    mobj2.IntRate = Model.IntRate;
                    mobj2.LastUpdateBy = muserid;

                    mobj2.LeadCode = "";
                    mobj2.LeadConvertDt = DateTime.Now;
                    mobj2.Name = Model.Name;
                    mobj2.Narr = (Model.Narr == null) ? "" : Model.Narr;
                    mobj2.PaymentTerms = (Model.PaymentTerms == null) ? "" : Model.PaymentTerms;
                    mobj2.Rank = (Model.Rank == null) ? 0 : Convert.ToInt32(Model.Rank);
                    mobj2.ReminderFormat = (Model.ReminderFormat == null) ? "" : Model.ReminderFormat;
                    mobj2.SMSTemplate = (Model.SMSTemplate == null) ? "" : Model.SMSTemplate;
                    mobj2.SMSUsers = "";
                    mobj2.SMSParty = Model.SMSParty;
                    mobj2.SMSSalesman = Model.SMSSalesman;
                    mobj2.IncoPlace = (Model.IncoPlace == null) ? "" : Model.IncoPlace;
                    mobj2.IncoTerms = (Model.IncoTerms == null) ? 0 : Model.IncoTerms;
                    mobj2.CurrName = Model.CurrCode;
                    mobj2.HSNCode = (Model.HSN == null) ? "" : Model.HSN;
                    mobj2.ReminderFormat = (Model.ReminderFormat == null) ? "" : Model.ReminderFormat;
                    mobj2.RTGS = (Model.RTGS == null) ? "" : Model.RTGS;
                    mobj2.TDSCode = (Model.TDSCode == null) ? 0 : Model.TDSCode;
                    mobj2.Transporter = (Model.Transporter == null) ? "" : Model.Transporter;
                    mobj2.xBranch = mbranchcode;
                    mobj2.ENTEREDBY = muserid;
                    mobj2.LastSent = DateTime.Now;
                    mobj2.ItemType = (Model.ItemType == null) ? "" : Model.ItemType;
                    mobj2.LocationCode = 100001;
                    mobj2.LASTUPDATEDATE = DateTime.Now;
                    mobj2.GSTType = (Model.OptionGstType == null) ? 0 : Convert.ToInt32(Model.OptionGstType);
                    mobj2.GSTFlag = Model.GstApplicable;
                    mobj2.ODLImit = Model.ODLimit;
                    mobj2.DrAcNo = (Model.AcCode == null) ? "" : Model.AcCode;
                    mobj2.SMSPartyAlert = Model.SMSPartyAlert;
                    mobj2.BSRCode = Model.BSRCode;
                    ctxTFAT.MasterInfo.Add(mobj2);

                    HoldTransactions mobj4 = new HoldTransactions();
                    mobj4.AUTHIDS = muserid;
                    mobj4.AUTHORISE = "A00";
                    mobj4.CheckCRDays = Model.CheckCRDays;
                    mobj4.CheckCRLimit = Model.CheckCRLimit;
                    mobj4.ChkTempCRDays = false;
                    mobj4.ChkTempCRLimit = false;
                    mobj4.Code = Model.Code;
                    mobj4.CRDaysWarn = (Model.CRDaysWarn == 0) ? false : true;
                    mobj4.CRLimitWarn = (Model.CRLimitWarn == 0) ? false : true;
                    mobj4.CRLimitWithTrx = Model.CRLimitWithTrx;
                    mobj4.CRLimitWithPO = Model.CRLimitWithPO;
                    mobj4.CrPeriod = Convert.ToInt32(Model.CRPeriod);
                    mobj4.ENTEREDBY = muserid;
                    mobj4.HoldDespatch = Model.HoldDespatch;
                    mobj4.HoldDespatchDt1 = (Model.StrHoldDespatchDt == null || Model.StrHoldDespatchDt == "01-01-0001" || Model.StrHoldDespatchDt == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrHoldDespatchDt);
                    mobj4.HoldDespatchDt2 = DateTime.Now;
                    mobj4.HoldEnquiry = Model.HoldEnquiry;
                    mobj4.HoldEnquiryDt1 = (Model.StrHoldEnquiryDt == null || Model.StrHoldEnquiryDt == "01-01-0001" || Model.StrHoldEnquiryDt == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrHoldEnquiryDt);
                    mobj4.HoldEnquiryDt2 = DateTime.Now;
                    mobj4.HoldInvoice = Model.HoldInvoice;
                    mobj4.HoldInvoiceDt1 = (Model.StrHoldInvoiceDt == null || Model.StrHoldInvoiceDt == "01-01-0001" || Model.StrHoldInvoiceDt == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrHoldInvoiceDt);
                    mobj4.DocDate = DateTime.Now;
                    mobj4.HoldInvoiceDt2 = DateTime.Now;
                    mobj4.HoldNarr = Model.HoldNarr;
                    mobj4.HoldOrder = false;
                    mobj4.HoldOrderDt1 = DateTime.Now;
                    mobj4.HoldOrderDt2 = DateTime.Now;
                    mobj4.HoldPayment = false;
                    mobj4.HoldQuote = false;
                    mobj4.HoldQuoteDt1 = DateTime.Now;
                    mobj4.HoldQuoteDt2 = DateTime.Now; ;
                    mobj4.LASTUPDATEDATE = DateTime.Now;
                    mobj4.TempCrDayDt1 = DateTime.Now;
                    mobj4.TempCrDayDt2 = DateTime.Now;
                    mobj4.TempCrLimit = 0;
                    mobj4.TempCrLimitDt1 = DateTime.Now;
                    mobj4.TempCrLimitDt2 = DateTime.Now;
                    mobj4.TempCrPeriod = 0;
                    mobj4.TempRemark = "";
                    mobj4.Ticklers = Model.Ticklers;
                    ctxTFAT.HoldTransactions.Add(mobj4);

                    TaxDetails mobj3 = new TaxDetails();
                    mobj3.AUTHIDS = muserid;
                    mobj3.AUTHORISE = "A00";
                    mobj3.Code = Model.Code;
                    mobj3.CutTCS = Model.CutTCS;
                    mobj3.CutTDS = Model.CutTDS;
                    mobj3.Deductee = "";
                    mobj3.DifferRate = Model.DifferRate;
                    mobj3.DifferRateCertNo = (Model.DifferRateCertNo == null) ? "" : Model.DifferRateCertNo;
                    mobj3.ENTEREDBY = muserid;
                    mobj3.Form15HCITDate = (Model.StrForm15HCITDate == null || Model.StrForm15HCITDate == "01-01-0001" || Model.StrForm15HCITDate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrForm15HCITDate);
                    mobj3.Form15HDate = (Model.StrForm15HDate == null || Model.StrForm15HDate == "01-01-0001" || Model.StrForm15HDate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrForm15HDate);
                    mobj3.IsDifferRate = Model.IsDifferRate;
                    mobj3.IsForm15H = Model.IsForm15H;
                    mobj3.LASTUPDATEDATE = DateTime.Now;
                    mobj3.LocationCode = 100001;

                    mobj3.TDSCode = (Model.TDSCode == null) ? 0 : Convert.ToInt32(Model.TDSCode);
                    ctxTFAT.TaxDetails.Add(mobj3);


                    Address mobj1 = new Address();
                    mobj1.AddOrContact = (Model.ContactType == null || Model.ContactType.Trim() == "") ? 0 : Convert.ToInt32(Model.ContactType);
                    mobj1.Adrl1 = (Model.Adrl1 == null) ? "" : Model.Adrl1;
                    mobj1.Adrl2 = (Model.Adrl2 == null) ? "" : Model.Adrl2;
                    mobj1.Adrl3 = (Model.Adrl3 == null) ? "" : Model.Adrl3;
                    mobj1.Adrl4 = "";
                    mobj1.AnnDate = (Model.StrAnndate == null || Model.StrAnndate == "01-01-0001" || Model.StrAnndate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrAnndate);
                    mobj1.Area = Model.Area;
                    mobj1.AssistEmail = (Model.AssistEmail == null) ? "" : Model.AssistEmail;
                    mobj1.AssistMobile = (Model.AssistMobile == null) ? "" : Model.AssistMobile;
                    mobj1.AssistName = (Model.AssistName == null) ? "" : Model.AssistName;
                    mobj1.AssistTel = (Model.AssistTel == null) ? "" : Model.AssistTel;
                    mobj1.AUTHIDS = muserid;
                    mobj1.AUTHORISE = "A00";
                    mobj1.BaseGr = (Model.AcType == null) ? "" : Model.AcType;
                    mobj1.BDate = (Model.Budate == null || Model.Budate == "01-01-0001" || Model.Budate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.Budate);
                    mobj1.ChildBdate = (Model.StrChildBdate == null || Model.StrChildBdate == "01-01-0001" || Model.StrChildBdate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrChildBdate);
                    mobj1.ChildName = (Model.ChildName == null) ? "" : Model.ChildName;
                    mobj1.City = (Model.City == null) ? "" : Model.City;
                    mobj1.Code = Model.Code;
                    mobj1.CorpID = (Model.CorpID == null) ? "" : Model.CorpID;
                    mobj1.CorrespondenceType = Model.CorrespondenceType;
                    mobj1.Country = (Model.Country == null) ? "" : Model.Country;
                    mobj1.Dept = Model.Dept;
                    mobj1.Designation = Model.Designation;
                    mobj1.Division = Model.Division;
                    mobj1.DraweeBank = Model.DraweeBank;
                    mobj1.Email = (Model.Email == null) ? "" : Model.Email;
                    mobj1.ENTEREDBY = muserid;
                    mobj1.Fax = (Model.Fax == null) ? "" : Model.Fax;
                    mobj1.Language = Model.Language;
                    mobj1.LASTUPDATEDATE = DateTime.Now;
                    mobj1.Licence1 = (Model.Licence1 == null) ? "" : Model.Licence1;
                    mobj1.Licence2 = (Model.Licence2 == null) ? "" : Model.Licence2;
                    mobj1.LocationCode = mlocationcode;
                    mobj1.MailingCategory = Model.MailingCategory;
                    mobj1.Mobile = (Model.Mobile == null) ? "" : Model.Mobile;
                    mobj1.Name = (Model.AName == null) ? "" : Model.AName;
                    mobj1.PanNo = (Model.PanNo == null) ? "" : Model.PanNo;
                    mobj1.Password = (Model.Password == null) ? "" : Model.Password;
                    mobj1.Person = (Model.Person == null) ? "" : Model.Person;
                    mobj1.PhotoPath = "";
                    mobj1.Pin = (Model.Pin == null) ? "" : Model.Pin;
                    mobj1.PTaxCode = (Model.PTaxCode == null) ? "" : Model.PTaxCode;
                    mobj1.STaxCode = (Model.STaxCode == null) ? "" : Model.STaxCode;
                    mobj1.Religion = Model.Religion;
                    mobj1.Segment = (Model.Segment == null) ? "" : Model.Segment;
                    mobj1.Sno = Convert.ToInt32(Model.SrNo);
                    mobj1.Source = Model.Source;
                    mobj1.SpouseBdate = (Model.SpouseBudate == null || Model.SpouseBudate == "01-01-0001" || Model.SpouseBudate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.SpouseBudate);
                    mobj1.SpouseName = (Model.SpouseName == null) ? "" : Model.SpouseName;
                    mobj1.State = (Model.State == null) ? "" : Model.State;
                    mobj1.Tel1 = (Model.Tel1 == null) ? "" : Model.Tel1;
                    mobj1.Tel2 = (Model.Tel2 == null) ? "" : Model.Tel2;
                    mobj1.Tel3 = (Model.Tel3 == null) ? "" : Model.Tel3;
                    mobj1.Tel4 = "";
                    mobj1.TINNo = (Model.TINNo == null) ? "" : Model.TINNo;
                    mobj1.UserID = (Model.UserID == null) ? "" : Model.UserID;
                    mobj1.www = (Model.www == null) ? "" : Model.www;
                    mobj1.AadharNo = (Model.AadharNo == null) ? "" : Model.AadharNo;
                    mobj1.GSTNo = (Model.GSTNo == null) ? "" : Model.GSTNo;
                    mobj1.IGSTRate = Model.DefaultIGst;
                    mobj1.CGSTRate = Model.DefaultCGst;
                    mobj1.SGSTRate = Model.DefaultSGst;
                    mobj1.GSTType = (Model.GSTType == null || Model.GSTType.Trim() == "") ? 0 : Convert.ToInt32(Model.GSTType);
                    mobj1.DealerType = (Model.DealerType == null || Model.DealerType.Trim() == "") ? (byte)0 : Convert.ToByte(Model.DealerType);
                    mobj1.VATReg = (Model.VATReg == null) ? "" : Model.VATReg;
                    mobj1.ReraRegNo = (Model.ReraNo == null) ? "" : Model.ReraNo;
                    ctxTFAT.Address.Add(mobj1);



                    if (Session["FixedAssets"] != null)
                    {
                        var FixedAssets = (List<MasterVM>)Session["FixedAssets"];
                        if (FixedAssets.Count != 0)
                        {
                            foreach (var item in FixedAssets)
                            {
                                Assets mobj12 = new Assets();
                                mobj12.AUTHORISE = "A00";
                                mobj12.Code = Model.Code;
                                mobj12.Branch = mbranchcode;
                                mobj12.AUTHIDS = muserid;
                                mobj12.Store = 100001;
                                mobj12.LocationCode = mlocationcode;
                                mobj12.AcDep = "";
                                mobj12.Method = item.Method;
                                mobj12.Rate = 1;
                                mobj12.AcCode = item.AcCode;
                                mobj12.BookValue = item.BookValue;
                                mobj12.CostPrice = item.CostPrice;
                                mobj12.PurchDate = item.PurchDate;
                                mobj12.UseDate = item.UseDate;
                                mobj12.LASTUPDATEDATE = DateTime.Now;
                                mobj12.ENTEREDBY = muserid;
                                ctxTFAT.Assets.Add(mobj12);
                            }
                        }
                    }

                    SaveAddons(Model);
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    var Prefix = GetPrefix(DateTime.Now);
                    var parentkey = "LED00" + Prefix.Substring(0, 2) + mobj.Code;
                    SendSMS_MSG_EmailOfMaster(Model.Mode, 0, mbranchcode + parentkey, DateTime.Now, mobj.Name, "NA");
                    UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, parentkey, DateTime.Now, 0, mobj.Code, "Save GENERAL LEDGERS", "A");

                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();

                    return Json(new { Message = ex1.EntityValidationErrors.Select(x => x.ValidationErrors.Select(xa => xa.ErrorMessage).FirstOrDefault()), Status = "Error" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.InnerException.Message, Status = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get

        public JsonResult AutoCompleteCountry(string term)
        {
            return Json((from m in ctxTFAT.TfatCountry
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code.ToString() }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteState(string term)
        {
            var mPara = term.Split('^');
            var mP1 = mPara[0].ToLower();
            var mP2 = mPara[1];
            // linq doesnt support array in query, so parameters are stored in var
            return Json((from m in ctxTFAT.TfatState
                         where m.Country == mP2 && m.Name.ToLower().Contains(mP1)
                         select new { Name = m.Name, Code = m.Code.ToString() }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteCity(string term)
        {
            var mPara = term.Split('^');
            var mP1 = mPara[0].ToLower();
            var mP2 = mPara[1];
            // linq doesnt support array in query, so parameters are stored in var
            return Json((from m in ctxTFAT.TfatCity
                         where m.State == mP2 && m.Name.ToLower().Contains(mP1)
                         select new { Name = m.Name, Code = m.Code.ToString() }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteTfatArea(string term)
        {
            if (term != "")
            {
                var result = ctxTFAT.AreaMaster.Where(X => X.Name.Contains(term)).Select(m => new { Code = m.Code, Name = m.Name }).OrderBy(n => n.Name).ToList().Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var result = ctxTFAT.AreaMaster.Select(m => new { Code = m.Code, Name = m.Name }).OrderBy(n => n.Name).ToList().Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetPartyCategory(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.PartyCategory.Select(c => new { c.Code, c.Name }).Distinct().OrderBy(x => x.Name).ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteSalesMan(string term)
        {
            return Json((from m in ctxTFAT.SalesMan
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteBroker(string term)
        {
            return Json((from m in ctxTFAT.Broker
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult GetCorrespondenceType(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.CorrespondenceType.Select(m => new { m.Code, m.Name }).Distinct().OrderBy(x => x.Name).ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDealerType(MasterVM Model)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "Tax Invoice" });
            GSt.Add(new SelectListItem { Value = "2", Text = "Reverse Charge" });
            GSt.Add(new SelectListItem { Value = "3", Text = "TCS" });
            GSt.Add(new SelectListItem { Value = "4", Text = "TDS" });
            GSt.Add(new SelectListItem { Value = "5", Text = "Bill Of Supply" });
            GSt.Add(new SelectListItem { Value = "6", Text = "Export( Under LUT)" });
            GSt.Add(new SelectListItem { Value = "7", Text = "UnRegistered Dealer" });
            GSt.Add(new SelectListItem { Value = "8", Text = "Sez with Payment" });
            GSt.Add(new SelectListItem { Value = "9", Text = "Sez w/0 Payment" });
            GSt.Add(new SelectListItem { Value = "10", Text = "Export (with Duty)" });
            GSt.Add(new SelectListItem { Value = "11", Text = "Exempted" });
            GSt.Add(new SelectListItem { Value = "12", Text = "No GST" });
            GSt.Add(new SelectListItem { Value = "13", Text = "Composition Dealer" });
            GSt.Add(new SelectListItem { Value = "14", Text = "Deemed Export" });
            GSt.Add(new SelectListItem { Value = "15", Text = "Nil Rated" });
            GSt.Add(new SelectListItem { Value = "16", Text = "Export @ 0.1%" });
            GSt.Add(new SelectListItem { Value = "17", Text = "Export @ 0.05%" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetHSNList(string term)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.HSNMaster.Select(x => new { x.Code, x.Name, Scope = "" }).ToList().Take(10);
                var result1 = ctxTFAT.TaxMaster.Where(x => x.VATGST == true).Select(x => new { x.Code, x.Name, x.Scope }).ToList().Take(10);

                List<SelectListItem> selectListItems = new List<SelectListItem>();
                foreach (var item in result)
                {
                    selectListItems.Add(new SelectListItem { Text = "[" + item.Name + "]", Value = item.Code });
                }
                foreach (var item in result1)
                {
                    selectListItems.Add(new SelectListItem { Text = "[" + item.Name + "][" + item.Scope + "]", Value = item.Code });
                }

                var Modified = selectListItems.Select(x => new
                {
                    Code = x.Value,
                    Name = x.Text
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.HSNMaster.Where(x => x.Name.ToLower().Contains(term.ToLower())).Select(m => new { m.Code, m.Name }).ToList().Take(10);
                var result1 = ctxTFAT.TaxMaster.Where(x => x.VATGST == true && x.Name.ToLower().Contains(term.ToLower())).Select(x => new { x.Code, x.Name, x.Scope }).ToList().Take(10);

                List<SelectListItem> selectListItems = new List<SelectListItem>();
                foreach (var item in result)
                {
                    selectListItems.Add(new SelectListItem { Text = "[" + item.Name + "]", Value = item.Code });
                }
                foreach (var item in result1)
                {
                    selectListItems.Add(new SelectListItem { Text = "[" + item.Name + "][" + item.Scope + "]", Value = item.Code });
                }
                var Modified = selectListItems.Select(x => new
                {
                    Code = x.Value,
                    Name = x.Text
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetIncoterm(GridOption Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.IncoTerms.Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBankname(GridOption Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.BankMaster.Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTdsMaster(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.TDSMaster.Select(c => new { c.Name, c.Code }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPayTerms(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.PaymentTerms.Select(c => new { c.Name, c.Code }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDesignation(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.Designations.Select(c => new { c.Name, c.Code }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Getlanguage(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.Language.Select(c => new { c.Name, c.Code }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDepartments(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.Dept.Select(c => new { c.Name, c.Code }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetReligion(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.Religion.Select(c => new { c.Name, c.Code }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSegmentType(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.Segments.Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDivisions(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.Divisions.Select(c => new { c.Name, c.Code }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }
            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAccountGrps(string term)
        {
            if (term == "")
            {
                List<SelectListItem> items = new List<SelectListItem>();
                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    string query = "select k.Code as Code,k.Name as Name from mastergroups k where  k.Code not in (select kk.grp from MasterGroups kk) order by k.Name ";
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
                var Modified = items.Select(x => new
                {
                    Code = x.Value,
                    Name = x.Text
                });
                //var result = ctxTFAT.MasterGroups.Where(x => x.Hide == false /*&& x.IsLast== true*/ && x.Code != x.Grp && x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).Take(10).ToList();
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<SelectListItem> items = new List<SelectListItem>();
                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    string query = "select k.Code as Code,k.Name as Name from mastergroups k where K.Name like '%"+ term + "%' and  k.Code not in (select kk.grp from MasterGroups kk) order by k.Name ";
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

                var Modified = items.Select(x => new
                {
                    Code = x.Value,
                    Name = x.Text
                });
                //var result = ctxTFAT.MasterGroups.Where(x => x.Hide == false /*&& x.IsLast == true*/ && x.Code != x.Grp && x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).ToList();
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetAccountGrp(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();

            var result = ctxTFAT.MasterGroups.Where(x => x.Hide == false /*&& x.IsLast == true*/ && x.Code != x.Grp).Select(c => new { c.Code, c.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Getsalestaxcode()
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.TaxMaster.Where(x => x.Scope == "S").Select(c => new { c.Code, c.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Getpurchasetaxcode()
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.TaxMaster.Where(x => x.Scope == "P").Select(c => new { c.Code, c.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAccountOwner(TfatPass Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.TfatPass.Select(c => new { c.Code, c.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMailCategory(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.MailingCategory.Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTemplate(MasterVM Model)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            var result = ctxTFAT.MsgTemplate.Select(c => new { Name = c.MsgText, c.Code }).Distinct().ToList();
            foreach (var item in result)
            {
                mList.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteCurrency(string term)
        {
            return Json(from m in ctxTFAT.CurrencyMaster
                        select new { m.Name, m.Code }, JsonRequestBehavior.AllowGet);
        }

        public string GetGSTTypeName(string Val)
        {
            string Name;
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "Tax Invoice" });
            GSt.Add(new SelectListItem { Value = "2", Text = "Reverse Charge" });
            GSt.Add(new SelectListItem { Value = "3", Text = "TCS" });
            GSt.Add(new SelectListItem { Value = "4", Text = "TDS" });
            GSt.Add(new SelectListItem { Value = "5", Text = "Bill Of Supply" });
            GSt.Add(new SelectListItem { Value = "6", Text = "Export( Under LUT)" });
            GSt.Add(new SelectListItem { Value = "7", Text = "UnRegistered Dealer" });
            GSt.Add(new SelectListItem { Value = "8", Text = "SEZ with Payment" });
            GSt.Add(new SelectListItem { Value = "9", Text = "SEZ w/0 Payment" });
            GSt.Add(new SelectListItem { Value = "10", Text = "Export (with Duty)" });
            GSt.Add(new SelectListItem { Value = "11", Text = "Exempted" });
            GSt.Add(new SelectListItem { Value = "12", Text = "No GST" });
            GSt.Add(new SelectListItem { Value = "13", Text = "Composition Dealer" });
            GSt.Add(new SelectListItem { Value = "14", Text = "Deemed Export" });
            GSt.Add(new SelectListItem { Value = "15", Text = "NIL Rated" });
            GSt.Add(new SelectListItem { Value = "16", Text = "Export @ 0.1%" });
            GSt.Add(new SelectListItem { Value = "17", Text = "Export @ 0.05%" });
            Name = GSt.Where(x => x.Value == Val).Select(x => x.Text).FirstOrDefault();
            return Name;
        }

        public ActionResult GetGSTType(string term)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "Tax Invoice" });
            GSt.Add(new SelectListItem { Value = "2", Text = "Reverse Charge" });
            GSt.Add(new SelectListItem { Value = "3", Text = "TCS" });
            GSt.Add(new SelectListItem { Value = "4", Text = "TDS" });
            GSt.Add(new SelectListItem { Value = "5", Text = "Bill Of Supply" });
            GSt.Add(new SelectListItem { Value = "6", Text = "Export( Under LUT)" });
            GSt.Add(new SelectListItem { Value = "7", Text = "UnRegistered Dealer" });
            GSt.Add(new SelectListItem { Value = "8", Text = "SEZ with Payment" });
            GSt.Add(new SelectListItem { Value = "9", Text = "SEZ w/0 Payment" });
            GSt.Add(new SelectListItem { Value = "10", Text = "Export (with Duty)" });
            GSt.Add(new SelectListItem { Value = "11", Text = "Exempted" });
            GSt.Add(new SelectListItem { Value = "12", Text = "No GST" });
            GSt.Add(new SelectListItem { Value = "13", Text = "Composition Dealer" });
            GSt.Add(new SelectListItem { Value = "14", Text = "Deemed Export" });
            GSt.Add(new SelectListItem { Value = "15", Text = "NIL Rated" });
            GSt.Add(new SelectListItem { Value = "16", Text = "Export @ 0.1%" });
            GSt.Add(new SelectListItem { Value = "17", Text = "Export @ 0.05%" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPartyCrLmtType(string term)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "Warning Only" });
            GSt.Add(new SelectListItem { Value = "2", Text = "Restrict Entry" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetContactType(string term)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "Both" });
            GSt.Add(new SelectListItem { Value = "2", Text = "Delivery Address" });
            GSt.Add(new SelectListItem { Value = "3", Text = "Contact details" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPartyCrDayLmtType(string term)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "Warning Only" });
            GSt.Add(new SelectListItem { Value = "2", Text = "Restrict Entry" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetGSTItemType(string term)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "Service" });
            GSt.Add(new SelectListItem { Value = "2", Text = "Goods" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSourceDoc(string term)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "Service" });
            GSt.Add(new SelectListItem { Value = "2", Text = "Goods" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAcType(string term)
        {
            List<SelectListItem> maclist = new List<SelectListItem>();
            if (mmaintype == "D")
            {
                maclist.Add(new SelectListItem { Value = "D", Text = "D - Customers / Debtors" });
                maclist.Add(new SelectListItem { Value = "U", Text = "U - Debtors - Cum - Creditors" });
            }
            else if (mmaintype == "S")
            {
                maclist.Add(new SelectListItem { Value = "S", Text = "S - Supplier / Creditor" });
                maclist.Add(new SelectListItem { Value = "U", Text = "U - Debtors - Cum - Creditors" });
            }
            else
            {
                maclist.Add(new SelectListItem { Value = "B", Text = "B - Bank Accounts" });
                maclist.Add(new SelectListItem { Value = "C", Text = "C - Cash Accounts" });
                maclist.Add(new SelectListItem { Value = "F", Text = "F - Fixed Assets" });
                maclist.Add(new SelectListItem { Value = "H", Text = "H - Inter - Branch Accounts" });
                maclist.Add(new SelectListItem { Value = "O", Text = "O - Loans And Advance" });
                maclist.Add(new SelectListItem { Value = "P", Text = "P - Capital Accounts" });
                maclist.Add(new SelectListItem { Value = "R", Text = "R - Reserves & Surplus" });
                maclist.Add(new SelectListItem { Value = "T", Text = "T - Credit Card Accounts" });
                maclist.Add(new SelectListItem { Value = "V", Text = "V - Investment Accounts" });
                maclist.Add(new SelectListItem { Value = "A", Text = "A - Assets" });
                maclist.Add(new SelectListItem { Value = "L", Text = "L - Liabilities" });
                maclist.Add(new SelectListItem { Value = "I", Text = "I - Incomes" });
                maclist.Add(new SelectListItem { Value = "X", Text = "X - Expenses" });
            }
            return Json(maclist, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCreditRank(string term)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "1" });
            GSt.Add(new SelectListItem { Value = "2", Text = "2" });
            GSt.Add(new SelectListItem { Value = "3", Text = "3" });
            GSt.Add(new SelectListItem { Value = "4", Text = "4" });
            GSt.Add(new SelectListItem { Value = "5", Text = "5" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFrequencyOS(string term)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "Never Followup" });
            GSt.Add(new SelectListItem { Value = "2", Text = "Every n Days" });
            GSt.Add(new SelectListItem { Value = "3", Text = "Selected Weekdays" });
            GSt.Add(new SelectListItem { Value = "4", Text = "First Weekday of Each Month" });
            GSt.Add(new SelectListItem { Value = "5", Text = "First Date of Each Month" });
            GSt.Add(new SelectListItem { Value = "6", Text = "Weekday of 1 & 3 Weeks" });
            GSt.Add(new SelectListItem { Value = "7", Text = "Weekday of 2 & 4 Weeks" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFrequencyForm(string term)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "Never Followup" });
            GSt.Add(new SelectListItem { Value = "2", Text = "Every n Days" });
            GSt.Add(new SelectListItem { Value = "3", Text = "Selected Weekdays" });
            GSt.Add(new SelectListItem { Value = "4", Text = "First Weekday of Each Month" });
            GSt.Add(new SelectListItem { Value = "5", Text = "First Date of Each Month" });
            GSt.Add(new SelectListItem { Value = "6", Text = "Weekday of 1 & 3 Weeks" });
            GSt.Add(new SelectListItem { Value = "7", Text = "Weekday of 2 & 4 Weeks" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        private static List<SelectListItem> PopulateBranches()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            //string constr = ConfigurationManager.ConnectionStrings["ALT_ERP21EntitiesConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(ACGenLedgerController.connstring))
            {
                string query = " SELECT Code, Name FROM TfatBranch where Code='HO0000' or ( category!='Area' and code!='G00000')";
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

        public ActionResult GetGSTDetails(CustomerMasterVM Model)
        {
            decimal IGST = 0, SGST = 0, CGST = 0;
            var CurrDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
            if (ctxTFAT.HSNMaster.Where(x => x.Code == Model.HSN).FirstOrDefault() != null)
            {
                var taxdetails = ctxTFAT.HSNRates.Where(x => x.Code == Model.HSN && x.EffDate <= CurrDate).OrderByDescending(x => x.EffDate).FirstOrDefault();
                if (taxdetails != null)
                {
                    IGST = taxdetails.IGSTRate == null ? 0 : taxdetails.IGSTRate.Value;
                    SGST = taxdetails.SGSTRate == null ? 0 : taxdetails.SGSTRate.Value;
                    CGST = taxdetails.CGSTRate == null ? 0 : taxdetails.CGSTRate.Value;
                }
            }
            else
            {
                var taxdetails = ctxTFAT.TaxMaster.Where(x => x.Code == Model.HSN).FirstOrDefault();
                if (taxdetails != null)
                {
                    IGST = taxdetails.IGSTRate == null ? 0 : taxdetails.IGSTRate.Value;
                    SGST = taxdetails.SGSTRate == null ? 0 : taxdetails.SGSTRate.Value;
                    CGST = taxdetails.CGSTRate == null ? 0 : taxdetails.CGSTRate.Value;
                }
            }
            return Json(new
            {
                IGST = IGST.ToString("F"),
                SGST = SGST.ToString("F"),
                CGST = CGST.ToString("F"),
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTDSDetails(CustomerMasterVM Model)
        {
            decimal TdsAmtPerc = 0;
            var CurrDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
            TDSRates tDSRates = ctxTFAT.TDSRates.Where(x => x.Code == Model.TDSCode && x.EffDate <= CurrDate).OrderByDescending(x => x.EffDate).FirstOrDefault();
            if (tDSRates != null)
            {
                TdsAmtPerc = tDSRates.TDSRate ?? 0;
            }

            var jsonResult = Json(new
            {
                TdsAmtPerc = TdsAmtPerc.ToString("F")
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }
        #endregion

        #region AddonGrid

        [HttpGet]
        public ActionResult GetEditAddOnList(string Code)
        {
            MasterVM Model = new MasterVM();
            List<AddOns> objitemlist = new List<AddOns>();
            var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%Master" && x.Hide == false)
                          select i).ToList();

            int t = 1;
            int n = 0;
            foreach (var i in addons)
            {
                AddOns c = new AddOns();
                c.Fld = i.Fld;
                c.Head = i.Head;
                c.ApplCode = GetMasterAddonValue(i.Fld, Code);
                c.QueryText = i.QueryText == null ? "" : GetQueryText(i.QueryText);
                c.FldType = i.FldType;
                c.PlaceValue = i.PlaceValue;
                c.Eqsn = i.Eqsn;
                objitemlist.Add(c);
                t = t + 1;
                n = n + 1;
            }

            Model.AddOnList = objitemlist;
            var html = ViewHelper.RenderPartialView(this, "~/Areas/SetUp/Views/Master/AddOnGrid.cshtml", new MasterVM() { AddOnList = Model.AddOnList, Mode = "Edit" });
            var jsonResult = Json(new
            {
                AddOnList = Model.AddOnList,
                Mode = "Edit",
                Html = html
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }

        public List<AddOns> GetAddOnListOnEdit(string Code)
        {
            List<AddOns> objitemlist = new List<AddOns>();
            var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%Master" && x.Hide == false)
                          select i).ToList();
            int t = 1;
            int n = 0;
            foreach (var i in addons)
            {
                AddOns c = new AddOns();
                c.Fld = i.Fld;
                c.Head = i.Head;
                c.ApplCode = GetMasterAddonValue(i.Fld, Code);
                c.QueryText = i.QueryText == null ? "" : GetQueryText(i.QueryText);
                c.FldType = i.FldType;
                c.PlaceValue = i.PlaceValue;
                c.Eqsn = i.Eqsn;
                objitemlist.Add(c);
                t = t + 1;
                n = n + 1;
            }
            return objitemlist;
        }

        [HttpPost]
        public ActionResult CalByEquationAddon(PurchaseVM Model)
        {
            List<string> declist = new List<string>();
            List<AddOns> declist2 = new List<AddOns>();
            if (Model.AddonValueLast == null || Model.AddonValueLast.Count < 1)
            {

            }
            else
            {
                declist2 = Model.AddonValueLast;
            }
            string finaleqn;
            decimal mamt = 0;

            declist = ConvertAddonString(declist2);
            List<AddOns> objitemlist = new List<AddOns>();
            var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%Master" && x.Hide == false)
                          select i).ToList();
            int t = 1;
            int n = 0;
            foreach (var i in addons)
            {
                AddOns c = new AddOns();
                c.Fld = i.Fld;
                c.Head = i.Head;
                c.ApplCode = declist[n];
                c.QueryText = i.QueryText == null ? "" : GetQueryText(i.QueryText);
                c.FldType = i.FldType;
                c.PlaceValue = i.PlaceValue;
                c.Eqsn = i.Eqsn;
                objitemlist.Add(c);
                t = t + 1;
                n = n + 1;
            }

            Model.AddOnList = objitemlist;
            var html = ViewHelper.RenderPartialView(this, "~/Areas/SetUp/Views/Master/AddOnGrid.cshtml", new MasterVM() { AddOnList = Model.AddOnList, Mode = Model.Mode, Fld = Model.Fld });
            var jsonResult = Json(new
            {
                AddOnList = Model.AddOnList,
                Mode = Model.Mode,
                Fld = Model.Fld,
                Html = html
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public List<string> ConvertAddonString(List<AddOns> LastModel)
        {
            int j;
            string ab;
            string mamt = "";
            List<string> PopulateStr = new List<string>();
            var trnaddons = ctxTFAT.AddOns.Where(x => x.TableKey == "%Master" && x.Hide == false).Select(x => new { x.Eqsn, x.FldType }).ToList();
            for (int i = 0; i < trnaddons.Count; i++)
            {
                var Eqn = trnaddons[i].Eqsn == null ? "" : trnaddons[i].Eqsn.Trim();
                if (Eqn.Contains("%F"))
                {
                    for (int ai = 0; ai < trnaddons.Count; ai++)
                    {
                        j = ai + 1;
                        if (LastModel[ai].FldType == "N")
                        {
                            Eqn = Eqn.Replace("%F" + j.ToString("D3"), (LastModel[ai].ApplCode == "" || LastModel[ai].ApplCode == null) ? "0" : LastModel[ai].ApplCode);
                            if (!Eqn.Contains("%F"))
                            {
                                break;
                            }
                        }
                        else if (LastModel[ai].FldType == "T" || LastModel[ai].FldType == "M" || LastModel[ai].FldType == "C")
                        {
                            Eqn = Eqn.Replace("%F" + j.ToString("D3"), (LastModel[ai].ApplCode == "" || LastModel[ai].ApplCode == null) ? "''" : LastModel[ai].ApplCode);
                            if (!Eqn.Contains("%F"))
                            {
                                break;
                            }
                        }
                        else if (LastModel[ai].FldType == "D" || LastModel[ai].FldType == "L")
                        {
                            Eqn = "";
                            break;

                        }

                    }
                }
                if (Eqn != "")
                {
                    mamt = GetAmtValueAddon(Eqn);
                }
                else
                {
                    mamt = LastModel[i].ApplCode;
                }

                PopulateStr.Add(mamt);
            }
            return PopulateStr;
        }

        public string GetAmtValueAddon(string finalvalue)
        {
            string sql = "";
            string mamtm = "";
            if (finalvalue.Contains("select"))
            {
                sql = finalvalue;
            }
            else
            {
                sql = @"select " + finalvalue + " from TfatComp";
            }

            DataTable smDt = GetDataTable(sql, GetConnectionString());

            if (smDt.Rows.Count > 0)
            {
                mamtm = (smDt.Rows[0][0].ToString() == "" || smDt.Rows[0][0].ToString() == null) ? "" : smDt.Rows[0][0].ToString();
            }
            else
            {
                mamtm = "";
            }
            return mamtm;
        }

        //public string GetQueryText(string sql)
        //{
        //    string bca = "";
        //    if (sql.Contains("^"))
        //    {
        //        bca = sql;
        //    }
        //    else
        //    {
        //        StringBuilder addonT = new StringBuilder();
        //        DataTable mDt2 = GetDataTable(sql, GetConnectionString());
        //        if (mDt2.Rows.Count > 0)
        //        {
        //            for (var i = 0; i < mDt2.Rows.Count; i++)
        //            {
        //                bca = (mDt2.Rows[i][0].ToString() == "" || mDt2.Rows[i][0].ToString() == null) ? "" : mDt2.Rows[i][0].ToString();
        //                addonT.Append(bca + "^");
        //            }
        //        }
        //        string addonVT = addonT.ToString();
        //        if (addonVT != "")
        //        {
        //            bca = addonVT.Remove(addonVT.Length - 1);
        //        }
        //    }
        //    return bca;
        //}

        public string GetMasterAddonValue(string fld, string TableKey)
        {
            string connstring = GetConnectionString();
            string bca = "";
            var loginQuery3 = @"select " + fld + " from AddonMas where tablekey=" + "'" + TableKey + "'" + "";
            DataTable mDt2 = GetDataTable(loginQuery3, connstring);
            if (mDt2.Rows.Count > 0)
            {
                bca = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? "" : mDt2.Rows[0][0].ToString();
            }
            return bca;
        }
        #endregion

        [HttpPost]
        public ActionResult DeleteAccounts(MasterVM Model)
        {
            if (Model.Document == null || Model.Document == "")
            {
                return Json(new
                {
                    Message = "Missing Account Code..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            // iX9: Check for Active Master
            string mactivestring = "";
            var mactive1 = ctxTFAT.Ledger.Where(x => (x.Code == Model.Document)).Select(x => x.TableKey).FirstOrDefault();
            if (mactive1 != null)
            {
                mactivestring = mactivestring + "\nLedger: " + mactive1;
            }
            var mactive11 = ctxTFAT.Ledger.Where(x => (x.AltCode == Model.Document)).Select(x => x.TableKey).FirstOrDefault();
            if (mactive11 != null)
            {
                mactivestring = mactivestring + "\nLedger: " + mactive11;
            }

            var mactive2 = ctxTFAT.Orders.Where(x => (x.Code == Model.Document)).Select(x => x.TableKey).FirstOrDefault();
            if (mactive2 != null)
            {
                mactivestring = mactivestring + "\nOrders: " + mactive2;
            }

            var mactive3 = ctxTFAT.FMVouRel.Where(x => (x.PostCode == Model.Document)).Select(x => x.FMNo).FirstOrDefault();
            if (mactive3 != null)
            {
                mactivestring = mactivestring + "\nFMNo: " + mactive3;
            }




            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "Active Account, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var mList = ctxTFAT.MasterInfo.Where(x => (x.Code == Model.Document)).FirstOrDefault();
                    if (mList != null)
                    {
                        ctxTFAT.MasterInfo.Remove(mList);
                    }
                    var mList2 = ctxTFAT.Address.Where(x => (x.Code == Model.Document)).FirstOrDefault();
                    if (mList2 != null)
                    {
                        ctxTFAT.Address.Remove(mList2);
                    }
                    var mList4 = ctxTFAT.HoldTransactions.Where(x => (x.Code == Model.Document)).FirstOrDefault();
                    if (mList4 != null)
                    {
                        ctxTFAT.HoldTransactions.Remove(mList4);
                    }
                    var mList5 = ctxTFAT.TaxDetails.Where(x => (x.Code == Model.Document)).FirstOrDefault();
                    if (mList5 != null)
                    {
                        ctxTFAT.TaxDetails.Remove(mList5);
                    }
                    var mList6 = ctxTFAT.AddonValues.Where(x => (x.TableKey == Model.Document)).FirstOrDefault();

                    if (mList6 != null)
                    {
                        ctxTFAT.AddonValues.Remove(mList6);
                    }
                    var mList3 = ctxTFAT.Master.Where(x => (x.Code == Model.Document)).FirstOrDefault();
                    if (mList3 != null)
                    {
                        ctxTFAT.Master.Remove(mList3);
                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "LED00" + mperiod.Substring(0, 2) + Model.Document, DateTime.Now, 0, Model.Document, "Delete GENERAL LEDGERS", "A");

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
        private void SaveAddons(MasterVM Model)
        {
            string addo1, addo2;
            StringBuilder addonT = new StringBuilder();
            List<string> addlist = new List<string>();
            if (Model.AddOnList != null && Model.AddOnList.Count > 0)
            {

                var addoni = ctxTFAT.AddonMas.Where(x => x.TableKey == Model.Code).Select(x => x).FirstOrDefault();
                if (addoni != null)
                {
                    ctxTFAT.AddonMas.Remove(addoni);
                }
                AddonMas aip = new AddonMas();
                aip.AUTHIDS = muserid;
                aip.AUTHORISE = "A00";
                aip.ENTEREDBY = muserid;
                aip.F001 = Model.AddOnList.Where(x => x.Fld == "F001").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F002 = Model.AddOnList.Where(x => x.Fld == "F002").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F003 = Model.AddOnList.Where(x => x.Fld == "F003").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F004 = Model.AddOnList.Where(x => x.Fld == "F004").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F005 = Model.AddOnList.Where(x => x.Fld == "F005").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F005").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F006 = Model.AddOnList.Where(x => x.Fld == "F006").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F006").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F007 = Model.AddOnList.Where(x => x.Fld == "F007").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F007").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F008 = Model.AddOnList.Where(x => x.Fld == "F008").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F009 = Model.AddOnList.Where(x => x.Fld == "F009").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F009").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F010 = Model.AddOnList.Where(x => x.Fld == "F010").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F010").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F011 = Model.AddOnList.Where(x => x.Fld == "F011").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F011").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F012 = Model.AddOnList.Where(x => x.Fld == "F012").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F013 = Model.AddOnList.Where(x => x.Fld == "F013").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F013").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F014 = Model.AddOnList.Where(x => x.Fld == "F014").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F014").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F015 = Model.AddOnList.Where(x => x.Fld == "F015").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F015").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F016 = Model.AddOnList.Where(x => x.Fld == "F016").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F016").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F017 = Model.AddOnList.Where(x => x.Fld == "F017").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F017").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F018 = Model.AddOnList.Where(x => x.Fld == "F018").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F018").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F019 = Model.AddOnList.Where(x => x.Fld == "F019").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F019").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F020 = Model.AddOnList.Where(x => x.Fld == "F020").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F020").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F021 = Model.AddOnList.Where(x => x.Fld == "F021").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F021").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F022 = Model.AddOnList.Where(x => x.Fld == "F022").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F022").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F023 = Model.AddOnList.Where(x => x.Fld == "F023").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F023").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F024 = Model.AddOnList.Where(x => x.Fld == "F024").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F024").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F025 = Model.AddOnList.Where(x => x.Fld == "F025").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F025").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F026 = Model.AddOnList.Where(x => x.Fld == "F026").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F026").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F027 = Model.AddOnList.Where(x => x.Fld == "F027").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F027").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F028 = Model.AddOnList.Where(x => x.Fld == "F028").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F028").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F029 = Model.AddOnList.Where(x => x.Fld == "F029").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F029").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F030 = Model.AddOnList.Where(x => x.Fld == "F030").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F030").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.LASTUPDATEDATE = DateTime.Now;
                aip.TableKey = Model.Code;
                ctxTFAT.AddonMas.Add(aip);

            }
        }
    }
}