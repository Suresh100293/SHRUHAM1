using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class AccountOpeningManualController : BaseController
    {
        //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();

        // GET: Accounts/AccountOpeningManual
        #region Index
        // GET: Accounts/AccountOpeningManual
        public ActionResult Index(AccountOpeningVM Model)
        {
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "NA");

            if (!String.IsNullOrEmpty(Model.Document))
            {
                Master master = ctxTFAT.Master.Where(x => x.Code == Model.Document).FirstOrDefault();
                if (master != null)
                {
                    Model.Account = master.Code;
                    Model.AccountN = master.Name;
                }
            }

            TempData.Remove("Type");
            TempData.Remove("Branch");
            TempData.Remove("addAmtType");
            TempData.Remove("addDocuDate");

            GetAllMenu(Session["ModuleName"].ToString());
            var doctype = ctxTFAT.DocTypes.Where(x => x.Code == "XOP00").Select(x => x).FirstOrDefault();
            Model.SubType = doctype.SubType;
            Model.MainType = doctype.MainType;
            DateTime firstdate = ctxTFAT.TfatPerd.Where(x => x.Code == mcompcode).OrderBy(z => z.StartDate).Select(x => x.StartDate).FirstOrDefault();
            var prevfirstdate = firstdate.AddDays(-1);

            var mAuth = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == "XOP00").Select(x => new
            {
                x.AuthLock,
                x.AuthNoPrint,
                x.AuthReq
            }).FirstOrDefault();
            if (mAuth != null)
            {
                Model.AuthLock = mAuth.AuthLock;
                Model.AuthNoPrint = mAuth.AuthNoPrint;
                Model.AuthReq = mAuth.AuthReq;
            }
            Model.AUTHORISE = "A00";
            Session["AccountOpeningList"] = null;
            Session["CostCentreList"] = null;

            Model.LocationCode = ctxTFAT.Warehouse.Where(x => x.Branch == mbranchcode).Select(x => x.Code).FirstOrDefault();
            Model.AmtType = "Debit";

            var currname = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.CurrName).FirstOrDefault();
            Model.CurrCode = currname.ToString();



            if (Model.Mode != "Add" && Model.AuthReq == true && Model.AUTHORISE.Substring(0, 1) == "A" && Model.AuthLock)
            {
                Model.Mode = "View";
            }



            return View(Model);
        }
        #endregion

        #region GET

        public ActionResult GetAccountDetails(string Account, string Branch)
        {
            AccountOpeningVM Model = new AccountOpeningVM();
            Model.BaseGr = ctxTFAT.Master.Where(x => x.Code == Account).Select(x => x.BaseGr).FirstOrDefault();
            DateTime firstdate = ctxTFAT.TfatPerd.Where(x => x.Code == mcompcode).OrderBy(z => z.StartDate).Select(x => x.StartDate).FirstOrDefault();
            var prevfirstdate = firstdate.AddDays(-1);

            Master master = ctxTFAT.Master.Where(x => x.Code == Account).FirstOrDefault();
            MasterGroups groups = ctxTFAT.MasterGroups.Where(x => x.Code == master.Grp).FirstOrDefault();
            string Msssage = "Document is Already Adjusted In Cash Bank Against : ", Message1 = "", Message2 = "",ErroMsg="";
            Message1 = " Cant ";
            bool Error = false;
            if (Model.BaseGr != "D" && Model.BaseGr != "S" && Model.BaseGr != "U")
            {
                List<AccountOpeningVM> objledgerdetail = new List<AccountOpeningVM>();
                List<AccountOpeningVM> ccdetail = new List<AccountOpeningVM>();
                var ledgers = (from l in ctxTFAT.Ledger
                               where l.Code == Account && l.DocDate <= prevfirstdate && l.Type == "XOP00"
                               select new { l.BillNumber,l.Sno,l.Branch, l.Party, l.Code, Type = l.Type, DocDate = l.DocDate, Prefix = l.Prefix, Credit = l.Credit, Debit = l.Debit, l.CrPeriod, l.CurrName, l.CurrRate, l.Srl, l.LocationCode, l.RefDoc }).ToList();
                foreach (var a in ledgers)
                {
                    decimal acredit = (a.Credit == null) ? 0 : a.Credit.Value;
                    decimal adebit = (a.Debit == null) ? 0 : a.Debit.Value;
                    objledgerdetail.Add(new AccountOpeningVM()
                    {
                        Code = a.Code,
                        Type = a.Type,
                        addDocuDate = a.DocDate.ToString("dd-MM-yyyy"),
                        InvNumber = a.Srl,
                        Amount =  decimal.Round(acredit, 2, MidpointRounding.AwayFromZero) +  decimal.Round(adebit, 2, MidpointRounding.AwayFromZero),
                        addAmtType = acredit == 0 ? "Debit" : "Credit",
                        Credit =  decimal.Round(acredit, 2, MidpointRounding.AwayFromZero),
                        Debit =  decimal.Round(adebit, 2, MidpointRounding.AwayFromZero),
                        CreditDays = a.CrPeriod == null ? 0 : a.CrPeriod.Value,
                        CurrName = a.CurrName.ToString(),
                        CurrRate = a.CurrRate,
                        tEmpID = a.Sno,
                        ACGroup = groups.Code,
                        ACGroupN = groups.Name,
                        LocationCode = a.LocationCode,
                        CurrencyCode = a.CurrName,
                        CurrencyName = ctxTFAT.CurrencyMaster.Where(x => x.Code == a.CurrName).Select(x => x.Name).FirstOrDefault(),
                        addBillAmt = (a.RefDoc == null || a.RefDoc == "") ? 0 : Convert.ToDecimal(a.RefDoc),
                        CostBranch = a.Branch,
                        BillNumber = a.BillNumber,
                        CostBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == a.Branch).Select(x => x.Name).FirstOrDefault(),

                    });
                }
                int io=1;
                foreach (var item in objledgerdetail.OrderBy(x=>x.BillNumber).ToList())
                {
                    item.tEmpID = io;
                    ++io;
                }

                Model.CostCentreList = objledgerdetail;
                if (objledgerdetail.Count > 0)
                {
                    var upperledger = objledgerdetail.Select(x => x).FirstOrDefault();
                    Model.LocationCode = upperledger.LocationCode;
                    Model.Account = upperledger.Code;
                    Model.CurrCode = upperledger.CurrName;
                    Model.CurrName = ctxTFAT.CurrencyMaster.Where(x => x.Code == upperledger.CurrencyCode).Select(x => x.Name).FirstOrDefault();
                    ErroMsg = "" + master.Name + " Account Already Available So,We Can Not Processed In Add Mode.........!";
                    Error = true;
                }
                Session["CostCentreList"] = ccdetail;
                Model.HasRecord = ccdetail.Count > 0 ? true : false;
                //Model.Amount = objledgerdetail.Sum(x => (decimal?)x.addAmount) ?? 0;
                Model.Credit = objledgerdetail.Sum(x => (decimal?) x.Credit) ?? 0;
                Model.Debit = objledgerdetail.Sum(x => (decimal?)x.Debit) ?? 0;
                Model.Credit = decimal.Round(Model.Credit, 2, MidpointRounding.AwayFromZero);
                Model.Debit = decimal.Round(Model.Debit, 2, MidpointRounding.AwayFromZero);
                Model.AmtType = Model.Credit > Model.Debit ? "Credit" : "Debit";
                Model.Amount = Math.Abs(Model.Debit - Model.Credit);
                Session["AccountOpeningList"] = objledgerdetail;

                var html = ViewHelper.RenderPartialView(this, "CostCentre", new AccountOpeningVM() { CostCentreList = Model.CostCentreList });

                return Json(new
                {
                    ErroMsg = ErroMsg,
                    Error = Error,
                    Html = html,
                    AmtType = Model.AmtType,
                    Amount = Model.Amount,
                    CurrCode = Model.CurrCode,
                    LocationCode = Model.LocationCode,
                    BaseGr = Model.BaseGr,
                    HasRecord = Model.HasRecord,
                    Branch = Branch
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<AccountOpeningVM> objledgerdetail = new List<AccountOpeningVM>();
                var ledgers = (from l in ctxTFAT.Ledger
                               where l.Code == Account && l.DocDate <= prevfirstdate
                               select new {l.BillNumber,l.Sno, l.Branch, l.Party, l.Code, Type = l.Type, DocDate = l.DocDate, Prefix = l.Prefix, Credit = l.Credit, Debit = l.Debit, l.CrPeriod, l.CurrName, l.CurrRate, l.Srl, l.LocationCode, l.RefDoc }).ToList();

                

                foreach (var a in ledgers)
                {
                    decimal acredit = (a.Credit == null) ? 0 : a.Credit.Value;
                    decimal adebit = (a.Debit == null) ? 0 : a.Debit.Value;
                    string CustChild = null;
                    if (Model.BaseGr == "D")
                    {
                        CustChild = a.Party;
                    }
                    objledgerdetail.Add(new AccountOpeningVM()
                    {
                        Type = a.Type,
                        addDocuDate = a.DocDate.ToString("dd-MM-yyyy"),
                        InvNumber = a.Srl,
                        addAmount = acredit + adebit,
                        addAmtType = acredit == 0 ? "Debit" : "Credit",
                        Credit =  decimal.Round(acredit, 2, MidpointRounding.AwayFromZero),
                        Debit =  decimal.Round(adebit, 2, MidpointRounding.AwayFromZero),
                        CreditDays = a.CrPeriod == null ? 0 : a.CrPeriod.Value,
                        CurrName = a.CurrName.ToString(),
                        CurrRate = a.CurrRate,

                        tEmpID = a.Sno,
                        Code = a.Code,
                        //addBillAmt = (a.RefDoc == null || a.RefDoc == "") ? 0 : Convert.ToDecimal(a.RefDoc),
                        LocationCode = a.LocationCode,
                        CurrencyCode = a.CurrName,
                        CustChild = CustChild,
                        CustChildN = ctxTFAT.CustomerMaster.Where(x => x.Code == CustChild).Select(x => x.Name).FirstOrDefault(),
                        CurrencyName = ctxTFAT.CurrencyMaster.Where(x => x.Code == a.CurrName).Select(x => x.Name).FirstOrDefault(),
                        Branch = a.Branch,
                        BranchN = ctxTFAT.TfatBranch.Where(x => x.Code == a.Branch).Select(x => x.Name).FirstOrDefault(),
                        ACGroup = groups.Code,
                        BillNumber = a.BillNumber,
                        ACGroupN = groups.Name,
                        AdjustBill = ctxTFAT.Outstanding.Where(x => x.aType == a.Type && x.aSrl == a.Srl && x.aPrefix == a.Prefix ).ToList().Count() > 0 ? true : false,
                    });

                    int io = 1;
                    foreach (var item in objledgerdetail.OrderBy(x=>x.InvNumber).ToList())
                    {
                        item.tEmpID = io;
                        ++io;
                    }

                    var CheckDependency2 = ctxTFAT.Outstanding.Where(x => x.aType == a.Type && x.aSrl == a.Srl && x.aPrefix == a.Prefix).Select(x => x).FirstOrDefault();
                    if (CheckDependency2 != null)
                    {
                        if (CheckDependency2.Type == Model.Type && CheckDependency2.Srl == Model.Srl && CheckDependency2.Prefix == Model.Prefix && CheckDependency2.DocBranch == Model.Branch)
                        {

                        }
                        else
                        {
                            Model.AdjustBill = true;

                            Message2 += CheckDependency2.aSrl.ToString() + ",";
                        }

                    }

                }

                if (Model.AdjustBill)
                {
                    Message2 = Message2.Substring(0, Message2.Length - 1);
                }

                Model.Message = Msssage + Message2 + Message1;
                Model.BreakUpList = objledgerdetail;
                if (objledgerdetail.Count > 0)
                {
                    var upperledger = objledgerdetail.Select(x => x).FirstOrDefault();
                    Model.LocationCode = upperledger.LocationCode;
                    Model.Account = upperledger.Code;
                    Model.CurrCode = upperledger.CurrName;
                    Model.CurrName = ctxTFAT.CurrencyMaster.Where(x => x.Code == upperledger.CurrencyCode).Select(x => x.Name).FirstOrDefault();
                    ErroMsg = ""+ master.Name + " Account Already Available So,We Can Not Processed In Add Mode.........!";
                    Error = true;
                }
                Model.HasRecord = objledgerdetail.Count > 0 ? true : false;
                //Model.Amount = objledgerdetail.Sum(x => (decimal?)x.addAmount) ?? 0;
                Model.Credit = objledgerdetail.Sum(x => (decimal?)x.Credit) ?? 0;
                Model.Debit = objledgerdetail.Sum(x => (decimal?)x.Debit) ?? 0;

                Model.Credit = decimal.Round(Model.Credit, 2, MidpointRounding.AwayFromZero);
                Model.Debit = decimal.Round(Model.Debit, 2, MidpointRounding.AwayFromZero);

                Model.Amount = Math.Abs(Model.Debit - Model.Credit);
                Model.AmtType = Model.Credit > Model.Debit ? "Credit" : "Debit";

                Session["AccountOpeningList"] = objledgerdetail;
                var html = ViewHelper.RenderPartialView(this, "BreakUpList", new AccountOpeningVM() { BreakUpList = Model.BreakUpList.OrderBy(x=>x.tEmpID).ToList() });

                

                return Json(new
                {
                    ErroMsg= ErroMsg,
                    Error= Error,
                    Html = html,
                    AmtType = Model.AmtType,
                    Amount = Model.Amount,
                    CurrCode = Model.CurrCode,
                    LocationCode = Model.LocationCode,
                    BaseGr = Model.BaseGr,
                    HasRecord = Model.HasRecord,
                    Branch = Branch,
                    AdjustBill=Model.AdjustBill,
                    Message=Model.Message,
                }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetAccount(string term)
        {
            if (term == "")
            {
                var result = ctxTFAT.Master.Select(m => new
                {
                    m.Code,
                    Name = m.Name
                }).OrderBy(n => n.Name).Take(10).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var result = ctxTFAT.Master.Where(x => (x.Name.Contains(term) || x.ShortName.Contains(term))).Select(m => new
                {
                    m.Code,
                    Name = m.Name,
                }).OrderBy(n => n.Name).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetAccountGroup(string term)
        {

            if (term == "")
            {
                var result = ctxTFAT.MasterGroups.Select(m => new
                {
                    m.Code,
                    Name = m.Name

                }).OrderBy(n => n.Name).Take(10).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var result = ctxTFAT.MasterGroups.Where(x => (x.Name.Contains(term))).Select(m => new
                {
                    m.Code,
                    Name = m.Name
                }).OrderBy(n => n.Name).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetAccountParentGroup(string Code)
        {
            string AcGRp = "", AcGRpN = "";
            if (!String.IsNullOrEmpty(Code))
            {
                Master master = ctxTFAT.Master.Where(x => x.Code == Code).FirstOrDefault();
                if (master != null)
                {
                    MasterGroups groups = ctxTFAT.MasterGroups.Where(x => x.Code == master.Grp).FirstOrDefault();
                    if (groups != null)
                    {
                        AcGRp = groups.Code;
                        AcGRpN = groups.Name;
                    }
                }
            }
            return Json(new
            {
                AcGRp = AcGRp,
                AcGRpN = AcGRpN,
                JsonRequestBehavior.AllowGet
            });
        }

        public ActionResult GetBranchOpening(string term)
        {
            if (term == "")
            {

                var result = ctxTFAT.TfatBranch.Where(m =>m.Code!="G00000" && m.Category!="Area"  && m.Status == true).Select(m => new
                {
                    m.Code,
                    Name = m.Name
                }).OrderBy(n => n.Name).Take(10).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var result = ctxTFAT.TfatBranch.Where(m => m.Code != "G00000" && m.Category != "Area" &&  m.Status == true && (m.Name.Contains(term))).Select(m => new
                {
                    m.Code,
                    Name = m.Name,
                }).OrderBy(n => n.Name).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }

        public ActionResult GetTypes(string term)
        {
            if (term == "")
            {

                var result = ctxTFAT.DocTypes.Where(m => m.MainType != m.SubType && m.Code != m.SubType && (m.MainType == "RC" || m.MainType == "PM" || m.MainType == "JV" || m.SubType == "CP" || m.SubType == "IC" || m.SubType == "RP" || m.SubType == "NP" || m.SubType == "IM" || m.SubType == "PX" || m.SubType == "GP" || m.SubType == "OC" || m.SubType == "RS" || m.SubType == "CS" || m.SubType == "XS" || m.SubType == "SX" || m.SubType == "NS")).Select(m => new
                {
                    m.Code,
                    Name = m.Name
                }).OrderBy(n => n.Name).Take(10).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var result = ctxTFAT.DocTypes.Where(m => m.MainType != m.SubType && m.Code != m.SubType && (m.Name.Contains(term)) && (m.MainType == "RC" || m.MainType == "PM" || m.MainType == "JV" || m.SubType == "CP" || m.SubType == "IC" || m.SubType == "RP" || m.SubType == "NP" || m.SubType == "IM" || m.SubType == "PX" || m.SubType == "GP" || m.SubType == "OC" || m.SubType == "RS" || m.SubType == "CS" || m.SubType == "XS" || m.SubType == "SX" || m.SubType == "NS")).Select(m => new
                {
                    m.Code,
                    Name = m.Name,
                }).OrderBy(n => n.Name).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }

        public JsonResult LoadCustChild(string term, string Code)
        {
            List<CustomerMaster> customers = new List<CustomerMaster>();
            if (!String.IsNullOrEmpty(Code))
            {
                customers = ctxTFAT.CustomerMaster.Where(x => x.AccountParentGroup == Code).ToList();
            }

            if (!(String.IsNullOrEmpty(term)))
            {
                customers = customers.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }
            var Modified = customers.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetLocation(string Branch)
        {
            List<SelectListItem> LocationCodelst = new List<SelectListItem>();
            var result = ctxTFAT.Warehouse.Where(x => x.Branch == Branch).Select(c => new
            {
                c.Code,
                c.Name
            }).Distinct().ToList();
            foreach (var item in result)
            {
                LocationCodelst.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(LocationCodelst, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetCostCentre(string term)
        {
            if (term == "")
            {
                var result = ctxTFAT.CostCentre.Select(m => new
                {
                    m.Code,
                    Name = m.Name,

                }).OrderBy(n => n.Name).Take(10).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var result = ctxTFAT.CostCentre.Where(x => (x.Name.Contains(term))).Select(m => new
                {
                    m.Code,
                    Name = "[" + m.Code + "] " + " " + m.Name,

                }).OrderBy(n => n.Name).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }

        public ActionResult GetCurrency(string term)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.CurrencyMaster.Select(x => new
                {
                    x.Code,
                    x.Name
                }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.CurrencyMaster.Where(x => x.Name.Contains(term)).Select(m => new
                {
                    m.Code,
                    m.Name
                }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetCurrency2()
        {
            List<SelectListItem> LocationCodelst = new List<SelectListItem>();
            var result = ctxTFAT.CurrencyMaster.Select(c => new
            {
                c.Code,
                c.Name
            }).Distinct().ToList();
            foreach (var item in result)
            {
                LocationCodelst.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(LocationCodelst, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSalesMan(string term)
        {
            if (term == "")
            {
                var result = ctxTFAT.SalesMan.Select(m => new
                {
                    Code = m.Code.ToString(),
                    Name = m.Name,

                }).OrderBy(n => n.Name).Take(10).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var result = ctxTFAT.SalesMan.Where(x => (x.Name.Contains(term))).Select(m => new
                {
                    Code = m.Code.ToString(),
                    Name = m.Name,

                }).OrderBy(n => n.Name).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }

        public ActionResult GetBroker(string term)
        {
            if (term == "")
            {
                var result = ctxTFAT.Broker.Select(m => new
                {
                    Code = m.Code.ToString(),
                    Name = m.Name,

                }).OrderBy(n => n.Name).Take(10).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var result = ctxTFAT.Broker.Where(x => (x.Name.Contains(term))).Select(m => new
                {
                    Code = m.Code.ToString(),
                    Name = m.Name,

                }).OrderBy(n => n.Name).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetChildSummary(string term)
        {
            List<SelectListItem> CallCategoryList = new List<SelectListItem>();

            var list = (List<AccountOpeningVM>)Session["AccountOpeningList"];
            foreach (var item in list.Select(x => x.CustChild).Distinct())
            {
                var TotalAmt = (list.Where(x => x.CustChild == item).Sum(x => x.Debit)) - (list.Where(x => x.CustChild == item).Sum(x => x.Credit));
                //Childcode.Add(item);
                //ChildName.Add(TotalAmt.ToString());
                CallCategoryList.Add(new SelectListItem
                {
                    Value = item,
                    Text = "[" + list.Where(x => x.CustChild == item).Select(x => x.CustChildN).FirstOrDefault() + "] " + " " + TotalAmt.ToString("0.##"),
                });
            }

            var Modified = CallCategoryList.Select(x => new
            {
                Code = x.Value,
                Name = x.Text
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);

        }


        #endregion

        #region Add LEDGER ITEM
        public ActionResult GetBreakLedger(AccountOpeningVM Model)
        {
            if (Model.SessionFlag == "Edit")
            {
                var result = (List<AccountOpeningVM>)Session["AccountOpeningList"];
                var result1 = result.Where(x => x.tEmpID == Model.tEmpID);
                foreach (var item in result1)
                {

                    Model.Type = item.Type;
                    Model.TypeN = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => x.Name).FirstOrDefault();
                    Model.addDocuDate = item.addDocuDate;
                    Model.InvNumber = item.InvNumber;
                    Model.addAmount = decimal.Round(item.addAmount, 2, MidpointRounding.AwayFromZero); 
                    Model.addAmtType = item.addAmtType;
                    Model.addOrgAmount = item.addOrgAmount;
                    Model.CreditDays = item.CreditDays;
                    Model.CurrName = item.CurrName;
                    Model.CurrRate = item.CurrRate;
                    Model.TypeName = item.Type;
                    Model.CurrencyCode = (item.CurrCode == null || item.CurrCode == "") ? 0 : Convert.ToInt32(item.CurrCode);
                    Model.CurrencyName = ctxTFAT.CurrencyMaster.Where(x => x.Code.ToString() == item.CurrName).Select(x => x.Name).FirstOrDefault();
                    Model.addBillAmt = item.addBillAmt;
                    Model.CustChild = item.CustChild;
                    Model.CustChildN = ctxTFAT.CustomerMaster.Where(x => x.Code == item.CustChild).Select(x => x.Name).FirstOrDefault();
                    Model.Branch = item.Branch;
                    Model.BranchN = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault();
                    Model.BillNumber = item.BillNumber==null?"": item.BillNumber.Trim();
                }
                var jsonResult = Json(new { Html = this.RenderPartialView("AddNewLedger", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            else
            {
                
                Model.Type = TempData.Peek("Type") as string;
                if (!String.IsNullOrEmpty(Model.Type))
                {
                    Model.TypeName = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => x.Name).FirstOrDefault();
                    Model.TypeN = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => x.Name).FirstOrDefault();

                }

                Model.Branch = TempData.Peek("Branch") as string;
                if (!String.IsNullOrEmpty(Model.Branch))
                {
                    Model.BranchN = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Branch).Select(x => x.Name).FirstOrDefault();
                }
                else
                {
                    Model.Branch = mbranchcode;
                    Model.BranchN = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault();
                }

                Model.addAmtType = TempData.Peek("addAmtType") as string;
                if (String.IsNullOrEmpty(Model.addAmtType))
                {
                    Model.addAmtType = "Debit";
                }


                var customer = ctxTFAT.CustomerMaster.Where(x => x.AccountParentGroup == Model.Account).Select(x => new { x.Code, x.Name }).FirstOrDefault();
                if (customer!=null)
                {
                    Model.CustChild = customer.Code;
                    Model.CustChildN = customer.Name;
                }
                


                Model.addDocuDate = TempData.Peek("addDocuDate") as string;
                if (String.IsNullOrEmpty(Model.addDocuDate))
                {
                    DateTime firstdate = ctxTFAT.TfatPerd.Where(x => x.Code == mcompcode).OrderBy(z => z.StartDate).Select(x => x.StartDate).FirstOrDefault();
                    var prevfirstdate = firstdate.AddDays(-1);
                    Model.addDocuDate = prevfirstdate.ToString("dd-MM-yyyy");
                }
                var currname = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.CurrName).FirstOrDefault();
                Model.CurrName = currname.ToString();
                Model.CurrencyName = ctxTFAT.CurrencyMaster.Where(x => x.Code == currname).Select(x => x.Name).FirstOrDefault();
                Model.CurrRate = ctxTFAT.CurrencyMaster.Where(x => x.Code == currname).Select(x => x.CurrRate).FirstOrDefault();
                if (!String.IsNullOrEmpty(Model.Type))
                {
                    DateTime firstdate = ctxTFAT.TfatPerd.Where(x => x.Code == mcompcode).OrderBy(z => z.StartDate).Select(x => x.StartDate).FirstOrDefault();
                    var prevfirstdate = firstdate.AddDays(-1);
                    Model.Prefix = GetPrefix(prevfirstdate);

                    var Width = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => x.DocWidth).FirstOrDefault();
                    var DocSerial = ctxTFAT.Ledger.Where(x => x.Type == Model.Type && x.Prefix== Model.Prefix).OrderByDescending(x => x.Srl).FirstOrDefault();
                    if (DocSerial != null)
                    {
                        if (String.IsNullOrEmpty(DocSerial.Srl))
                        {
                            var From = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type ).Select(x => x.LimitFrom).FirstOrDefault();
                            Model.InvNumber = From.ToString().PadLeft(Width, '0');
                        }
                        else
                        {
                            var SerialNo = (Convert.ToInt32(DocSerial.Srl) + 1).ToString().PadLeft(Width, '0');
                            var result = (List<AccountOpeningVM>)Session["AccountOpeningList"];
                            var ExistOrNot = result.Where(x => x.InvNumber.Trim() == SerialNo.Trim() && x.Type.Trim()==Model.Type.Trim()).FirstOrDefault();
                            if (ExistOrNot==null)
                            {
                                SerialNo = (Convert.ToInt32(DocSerial.Srl) + 1).ToString();
                                Model.InvNumber = SerialNo.PadLeft(6, '0');
                            }
                            else
                            {
                                var DocNO = result.Where(x=>x.Type.Trim()==Model.Type.Trim()).OrderByDescending(x => x.InvNumber).Select(x => x.InvNumber).FirstOrDefault();
                                SerialNo = (Convert.ToInt32(DocNO) + 1).ToString();
                                Model.InvNumber = SerialNo.PadLeft(6, '0');
                            }
                        }
                    }
                    else
                    {
                        var From = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => x.LimitFrom).FirstOrDefault();
                        Model.InvNumber = From.ToString().PadLeft(Width, '0');
                    }
                }
                


                var jsonResult = Json(new { Html = this.RenderPartialView("AddNewLedger", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }
        [HttpPost]
        public ActionResult AddEditSelectedLedger(AccountOpeningVM Model)
        {
            if (Model.SessionFlag == "Add")
            {
                List<AccountOpeningVM> objledgerdetail = new List<AccountOpeningVM>();
                DateTime firstdate = ctxTFAT.TfatPerd.Where(x => x.Code == mcompcode).OrderBy(z => z.StartDate).Select(x => x.StartDate).FirstOrDefault();
                var prevfirstdate = firstdate.AddDays(-1);
                if (ConvertDDMMYYTOYYMMDD(Model.addDocuDate) > prevfirstdate)
                {
                    return Json(new
                    {
                        Message = "DATE MUST BE SELECTED ON OR BEFORE " + prevfirstdate,
                        Status = "CancelError"
                    }, JsonRequestBehavior.AllowGet);
                }

                int Id = 1;
                if (Session["AccountOpeningList"] != null)
                {
                    objledgerdetail = (List<AccountOpeningVM>)Session["AccountOpeningList"];
                    Id = (objledgerdetail.OrderByDescending(x => x.tEmpID).Select(x => x.tEmpID).FirstOrDefault())+1;
                }

                var currname = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.CurrName).FirstOrDefault();

                
                MasterGroups groups = ctxTFAT.MasterGroups.Where(x => x.Code == Model.ACGroup).FirstOrDefault();

                objledgerdetail.Add(new AccountOpeningVM()
                {
                    Type = Model.Type,
                    addDocuDate = Model.addDocuDate,
                    InvNumber = Model.InvNumber,
                    addAmount = Model.addAmount,
                    addAmtType = Model.addAmtType,
                    CreditDays = Model.CreditDays,
                    CurrName = Model.CurrName,
                    CurrRate = Model.CurrRate,
                    Debit = Model.addAmtType == "Debit" ? decimal.Round(Model.addAmount, 2, MidpointRounding.AwayFromZero) : 0,
                    Credit = Model.addAmtType == "Credit" ? decimal.Round(Model.addAmount, 2, MidpointRounding.AwayFromZero) : 0,
                    CurrencyName = ctxTFAT.CurrencyMaster.Where(x => x.Code == currname).Select(x => x.Name).FirstOrDefault(),
                    addBillAmt = Model.addBillAmt,
                    tEmpID = Id,
                    CustChild = Model.CustChild,
                    CustChildN = ctxTFAT.CustomerMaster.Where(x => x.Code == Model.CustChild).Select(x => x.Name).FirstOrDefault(),
                    Branch = Model.Branch,
                    BranchN = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Branch).Select(x => x.Name).FirstOrDefault(),
                    BillNumber=Model.BillNumber,
                    ACGroup = groups.Code,
                    ACGroupN = groups.Name,
                });

                var CheckDependency2 = ctxTFAT.Outstanding.Where(x => x.aType == Model.Type && x.aSrl == Model.Srl && x.aPrefix == Model.Prefix && x.DocBranch == Model.Branch).Select(x => x).FirstOrDefault();
                if (CheckDependency2 != null)
                {
                    if (CheckDependency2.Type == Model.Type && CheckDependency2.Srl == Model.Srl && CheckDependency2.Prefix == Model.Prefix && CheckDependency2.DocBranch == Model.Branch)
                    {

                    }
                    else
                    {
                        Model.AdjustBill = true;

                        Model.Message = " Document is Already Adjusted In Cash Bank Against : " + CheckDependency2.TableRefKey.ToString() + ", Cant " + Model.Mode;
                    }

                }
                Model.TypeName = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => x.Name).FirstOrDefault();
                Model.TypeN = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => x.Name).FirstOrDefault();
                TempData["Type"] = Model.Type;
                TempData["Branch"] = Model.Branch;
                TempData["addAmtType"] = Model.addAmtType;
                TempData["addDocuDate"] = Model.addDocuDate;

                Model.Credit = objledgerdetail.Sum(x => (decimal?)x.Credit) ?? 0;
                Model.Debit = objledgerdetail.Sum(x => (decimal?)x.Debit) ?? 0;
                Model.AmtType = Model.Credit > Model.Debit ? "Credit" : "Debit";
                Model.Amount = Math.Abs(Model.Debit - Model.Credit);
                Session.Add("AccountOpeningList", objledgerdetail);
                var html = ViewHelper.RenderPartialView(this, "BreakUpList", new AccountOpeningVM() { BreakUpList = objledgerdetail.OrderBy(x => x.tEmpID).ToList() });
                return Json(new { Html = html, AmtType = Model.AmtType, Amount = Model.Amount }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = (List<AccountOpeningVM>)Session["AccountOpeningList"];
                foreach (var item in result.Where(x => x.tEmpID == Model.tEmpID))
                {
                    item.Type = Model.Type;
                    item.addDocuDate = Model.addDocuDate;
                    item.InvNumber = Model.InvNumber;
                    item.addAmount = decimal.Round(Model.addAmount, 2, MidpointRounding.AwayFromZero);
                    item.addAmtType = Model.addAmtType;
                    item.CreditDays = Model.CreditDays;
                    item.CurrName = Model.CurrName;
                    item.CurrRate = Model.CurrRate;
                    item.Debit = Model.addAmtType == "Debit" ? Model.addAmount : 0;
                    item.Credit = Model.addAmtType == "Credit" ? Model.addAmount : 0;
                    item.addBillAmt = Model.addBillAmt;
                    item.CustChild = Model.CustChild;
                    item.CustChildN = ctxTFAT.CustomerMaster.Where(x => x.Code == Model.CustChild).Select(x => x.Name).FirstOrDefault();
                    item.Branch = Model.Branch;
                    item.BranchN = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Branch).Select(x => x.Name).FirstOrDefault();
                    item.BillNumber = Model.BillNumber;
                }

                Model.Credit = result.Sum(x => (decimal?)x.Credit) ?? 0;
                Model.Debit = result.Sum(x => (decimal?)x.Debit) ?? 0;
                Model.AmtType = Model.Credit > Model.Debit ? "Credit" : "Debit";
                Model.Amount = Math.Abs(Model.Debit - Model.Credit);
                Session.Add("AccountOpeningList", result);
                var html = ViewHelper.RenderPartialView(this, "BreakUpList", new AccountOpeningVM() { BreakUpList = result.OrderByDescending(x => x.tEmpID).ToList() });
                return Json(new
                {
                    Html = html,
                    AmtType = Model.AmtType,
                    Amount = Model.Amount
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult DeleteLedger(AccountOpeningVM Model)
        {
            var result2 = (List<AccountOpeningVM>)Session["AccountOpeningList"];
            var result = result2.Where(x => x.tEmpID != Model.tEmpID).ToList();
            Session.Add("AccountOpeningList", result);

            Model.Credit = result.Sum(x => (decimal?)x.Credit) ?? 0;
            Model.Debit = result.Sum(x => (decimal?)x.Debit) ?? 0;
            Model.AmtType = Model.Credit > Model.Debit ? "Credit" : "Debit";
            Model.Amount = Math.Abs(Model.Debit - Model.Credit);

            var html = ViewHelper.RenderPartialView(this, "BreakUpList", new AccountOpeningVM() { BreakUpList = result.OrderByDescending(x => x.tEmpID).ToList() });
            return Json(new { Html = html, AmtType = Model.AmtType, Amount = Model.Amount }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckInvoiceNo(AccountOpeningVM Model)
        {
            DateTime firstdate = ctxTFAT.TfatPerd.Where(x => x.Code == mcompcode).OrderBy(z => z.StartDate).Select(x => x.StartDate).FirstOrDefault();
            var prevfirstdate = firstdate.AddDays(-1);
            Model.Prefix = GetPrefix(prevfirstdate);

            var Width = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => x.DocWidth).FirstOrDefault();
            var DocSerial = ctxTFAT.Ledger.Where(x => x.Type == Model.Type && x.Prefix == Model.Prefix).OrderByDescending(x => x.Srl).FirstOrDefault();

            if (DocSerial != null)
            {
                if (String.IsNullOrEmpty(DocSerial.Srl))
                {
                    var From = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => x.LimitFrom).FirstOrDefault();
                    Model.Srl = From.ToString().PadLeft(Width, '0');
                }
                else
                {


                    var SerialNo = (Convert.ToInt32(DocSerial.Srl) + 1).ToString();
                    var result = (List<AccountOpeningVM>)Session["AccountOpeningList"];
                    var ExistOrNot = result.Where(x => x.InvNumber.Trim() == SerialNo.Trim() && x.Type.Trim() == Model.Type.Trim()).FirstOrDefault();
                    if (ExistOrNot == null)
                    {
                        SerialNo = (Convert.ToInt32(DocSerial.Srl) + 1).ToString();
                        Model.Srl = SerialNo.PadLeft(6, '0');
                    }
                    else
                    {
                        var DocNO = result.Where(x => x.Type.Trim() == Model.Type.Trim()).OrderByDescending(x => x.InvNumber).Select(x => x.InvNumber).FirstOrDefault();
                        SerialNo = (Convert.ToInt32(DocNO) + 1).ToString();
                        Model.Srl = SerialNo.PadLeft(6, '0');
                    }
                }
            }
            else
            {
                var From = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => x.LimitFrom).FirstOrDefault();
                Model.Srl = From.ToString().PadLeft(Width, '0');
            }

            return Json(new { Status = "Success",Serial= Model.Srl }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetExistingCostCenter(AccountOpeningVM Model)
        {
            List<AccountOpeningVM> objledgerdetail = new List<AccountOpeningVM>();
            if (Session["AccountOpeningList"] != null)
            {
                objledgerdetail = (List<AccountOpeningVM>)Session["AccountOpeningList"];

                var GetBranch = objledgerdetail.Where(x => x.CostBranch == Model.CostBranch).FirstOrDefault();
                if (GetBranch == null)
                {
                    return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Status = "Error", Message = "Not Allow To Add " + ctxTFAT.TfatBranch.Where(x => x.Code == Model.CostBranch).Select(x => x.Name).FirstOrDefault() + " Branch...!" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region CostCentre
        public ActionResult GetCCBreakLedger(AccountOpeningVM Model)
        {
            if (Model.SessionFlag == "Edit")
            {
                var result = (List<AccountOpeningVM>)Session["AccountOpeningList"];
                var result1 = result.Where(x => x.tEmpID == Model.tEmpID);
                foreach (var item in result1)
                {

                    //Model.CostCentre = item.CostCentre;
                    //int CostCentreint = Convert.ToInt32(item.CostCentre);
                    //Model.CostCentreName = ctxTFAT.CostCentre.Where(x => x.Code == CostCentreint).Select(x => x.Name).FirstOrDefault();
                    Model.Amount = item.Amount;
                    Model.addAmtType = item.addAmtType;
                    Model.Credit = item.addAmtType == "Credit" ?  decimal.Round(item.Amount, 2, MidpointRounding.AwayFromZero) : 0;
                    Model.Debit = item.addAmtType == "Debit" ? decimal.Round(item.Amount, 2, MidpointRounding.AwayFromZero) : 0;
                    Model.CostBranch = item.CostBranch;
                    Model.CostBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == item.CostBranch).Select(x => x.Name).FirstOrDefault();
                    Model.addAmount = item.addAmount;
                }

                var jsonResult = Json(new { Html = this.RenderPartialView("CostCentreAdd", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            else
            {
                var jsonResult = Json(new { Html = this.RenderPartialView("CostCentreAdd", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }
        [HttpPost]
        public ActionResult AddEditSelectedCC(AccountOpeningVM Model)
        {
            if (Model.SessionFlag == "Add")
            {
                List<AccountOpeningVM> objledgerdetail = new List<AccountOpeningVM>();
                int Id = 1;
                if (Session["AccountOpeningList"] != null)
                {
                    objledgerdetail = (List<AccountOpeningVM>)Session["AccountOpeningList"];
                    Id = (objledgerdetail.OrderByDescending(x => x.tEmpID).Select(x => x.tEmpID).FirstOrDefault()) + 1;
                }

                objledgerdetail.Add(new AccountOpeningVM()
                {
                    Amount = Model.Amount,
                    addAmtType = Model.addAmtType,
                    tEmpID = Id,
                    Credit = Model.addAmtType == "Credit" ?  decimal.Round(Model.Amount, 2, MidpointRounding.AwayFromZero): 0,
                    Debit = Model.addAmtType == "Debit" ? decimal.Round(Model.Amount, 2, MidpointRounding.AwayFromZero) : 0,
                    CostBranch = Model.CostBranch,
                    CostBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == Model.CostBranch).Select(x => x.Name).FirstOrDefault(),
                    addAmount = Model.addAmount,
                });
                Model.CostCentreList = objledgerdetail;
                Model.Credit = objledgerdetail.Sum(x => (decimal?)x.Credit) ?? 0;
                Model.Debit = objledgerdetail.Sum(x => (decimal?)x.Debit) ?? 0;
                Model.AmtType = Model.Credit > Model.Debit ? "Credit" : "Debit";
                Model.Amount = Math.Abs(Model.Debit - Model.Credit);
                Session.Add("AccountOpeningList", objledgerdetail);
                var html = ViewHelper.RenderPartialView(this, "CostCentre", new AccountOpeningVM() { CostCentreList = objledgerdetail });
                return Json(new { Html = html, AmtType = Model.AmtType, Amount = Model.Amount }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = (List<AccountOpeningVM>)Session["AccountOpeningList"];
                foreach (var item in result.Where(x => x.tEmpID == Model.tEmpID))
                {
                    item.CostCentre = Model.CostCentre;
                    item.Amount =  decimal.Round(Model.Amount, 2, MidpointRounding.AwayFromZero);
                    item.addAmtType = Model.addAmtType;
                    item.Credit = Model.addAmtType == "Credit" ? decimal.Round(Model.Amount, 2, MidpointRounding.AwayFromZero) : 0;
                    item.Debit = Model.addAmtType == "Debit" ? decimal.Round(Model.Amount, 2, MidpointRounding.AwayFromZero) : 0;
                    item.CostBranch = Model.CostBranch;
                    item.CostBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == Model.CostBranch).Select(x => x.Name).FirstOrDefault();
                }
                Model.CostCentreList = result;
                Model.Credit = result.Sum(x => (decimal?)x.Credit) ?? 0;
                Model.Debit = result.Sum(x => (decimal?)x.Debit) ?? 0;
                Model.AmtType = Model.Credit > Model.Debit ? "Credit" : "Debit";
                Model.Amount = Math.Abs(Model.Debit - Model.Credit);
                Session.Add("AccountOpeningList", result);
                var html = ViewHelper.RenderPartialView(this, "CostCentre", new AccountOpeningVM() { CostCentreList = result });
                return Json(new
                {
                    Html = html,
                    AmtType = Model.AmtType,
                    Amount = Model.Amount
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult DeleteCCLedger(AccountOpeningVM Model)
        {
            var result2 = (List<AccountOpeningVM>)Session["AccountOpeningList"];
            var result = result2.Where(x => x.tEmpID != Model.tEmpID).ToList();
            Session.Add("AccountOpeningList", result);

            Model.Credit = result.Sum(x => (decimal?)x.Credit) ?? 0;
            Model.Debit = result.Sum(x => (decimal?)x.Debit) ?? 0;
            Model.AmtType = Model.Credit > Model.Debit ? "Credit" : "Debit";
            Model.Amount = Math.Abs(Model.Debit - Model.Credit);

            var html = ViewHelper.RenderPartialView(this, "CostCentre", new AccountOpeningVM() { CostCentreList = result });
            return Json(new { Html = html, AmtType = Model.AmtType, Amount = Model.Amount }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Save
        private void DeUpdate(AccountOpeningVM Model)
        {
            DateTime firstdate = ctxTFAT.TfatPerd.OrderBy(z => z.StartDate).Select(x => x.StartDate).FirstOrDefault();
            var prevfirstdate = firstdate.AddDays(-1);
            var mDeleteLed = ctxTFAT.Ledger.Where(x => x.Code == Model.Account && x.DocDate <= prevfirstdate).ToList();
            ctxTFAT.Ledger.RemoveRange(mDeleteLed);
            var mDeleteCC = ctxTFAT.CostLedger.Where(x => x.Code == Model.Account && x.DocDate <= prevfirstdate).ToList();
            ctxTFAT.CostLedger.RemoveRange(mDeleteCC);
            ctxTFAT.SaveChanges();
        }

        [HttpPost]
        public ActionResult SaveData(AccountOpeningVM Model)
        {
            var result = (List<AccountOpeningVM>)Session["AccountOpeningList"];
            var costcentreresult = (List<AccountOpeningVM>)Session["CostCentreList"];


            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {

                    if (Model.AUTHORISE.Substring(0, 1) == "R" || Model.Mode == "Add" || (Model.Mode == "Edit" && Model.AuthAgain))
                    {
                        if (Model.AUTHORISE.Substring(0, 1) != "N") //if AUTHORISEd then check for the RateDiff Auth. Rule
                            Model.AUTHORISE = (Model.AuthReq == true ? GetAuthorise(Model.Type, 0, Model.Branch) : Model.AUTHORISE = "A00");
                    }

                    List<string> NewSrl = new List<string>();




                    //if (Model.Mode == "Edit")
                    //{

                    //}

                    int mCnt = 1;
                    if ((Model.BaseGr != "D" && Model.BaseGr != "S" && Model.BaseGr != "U"))
                    {
                        if (Model.Mode == "Add")
                        {
                            if (result == null)
                            {
                                return Json(new
                                {
                                    Status = "failure",
                                    Message = "Nothing to Save.."
                                }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        DateTime firstdate = ctxTFAT.TfatPerd.OrderBy(z => z.StartDate).Select(x => x.StartDate).FirstOrDefault();
                        var prevfirstdate = firstdate.AddDays(-1);
                        Model.Prefix = GetPrefix(prevfirstdate);
                        //Model.Srl = GetLastSerial("Ledger", Model.Branch, "XOP00", Model.Prefix, "RJ", prevfirstdate);
                        //Model.ParentKey = "XOP00" + Model.Prefix.Substring(0, 2) + Model.Srl;


                        var doctypes2 = ctxTFAT.DocTypes.Where(x => x.Code == "XOP00").Select(x => x).FirstOrDefault();


                        //Ledger mobj1 = new Ledger();
                        //mobj1.TableKey = "XOP00" + Model.Prefix.Substring(0, 2) + mCnt.ToString("D3") + Model.Srl;
                        //mobj1.ParentKey = Model.ParentKey;
                        //mobj1.Sno = mCnt;
                        //mobj1.Srl = Model.Srl;
                        //mobj1.SubType = "RJ";
                        //mobj1.Type = "XOP00";
                        //mobj1.AltCode = Model.Account;
                        //mobj1.Audited = false;
                        //mobj1.AUTHIDS = muserid;
                        //mobj1.AUTHORISE = Model.AUTHORISE;
                        //mobj1.BillDate = DateTime.Now;
                        //mobj1.BillNumber = "";
                        //mobj1.Branch = "NotReq";
                        //mobj1.CompCode = mcompcode;
                        //mobj1.Cheque = "";
                        //mobj1.ChequeReturn = false;
                        //mobj1.ClearDate = Convert.ToDateTime("01-01-1950");
                        //mobj1.ChequeDate = Convert.ToDateTime("1900-01-01");
                        //mobj1.Code = Model.Account;
                        //mobj1.CrPeriod = 0;
                        //mobj1.CurrName = Convert.ToInt32(Model.CurrCode);
                        //mobj1.CurrRate = Model.CurrRate;
                        //mobj1.Debit = (Model.AmtType == "Debit") ? Model.Amount : 0;
                        //mobj1.Credit = (Model.AmtType == "Credit") ? Model.Amount : 0;
                        //mobj1.Discounted = true;
                        //mobj1.DocDate = prevfirstdate;
                        //mobj1.MainType = "JV";
                        //mobj1.Narr = "";
                        //mobj1.Party = Model.Account;
                        //mobj1.Prefix = Model.Prefix;
                        //mobj1.RecoFlag = "";
                        //mobj1.RefDoc = "";
                        //mobj1.TDSChallanNumber = "";
                        //mobj1.TDSCode = 0;
                        //mobj1.TDSFlag = false;

                        //mobj1.ENTEREDBY = muserid;
                        //mobj1.DueDate = Convert.ToDateTime("1900-01-01");
                        //mobj1.Reminder = false;
                        //mobj1.TaskID = 0;
                        //mobj1.ChqCategory = 0;
                        //mobj1.CurrAmount = Model.CurrRate * Model.Amount;
                        //mobj1.LocationCode = 100001;
                        //mobj1.LASTUPDATEDATE = DateTime.Now;
                        //mobj1.GSTType = 0;
                        //mobj1.IGSTAmt = Convert.ToDecimal(0);
                        //mobj1.IGSTRate = Convert.ToDecimal(0);
                        //mobj1.CGSTAmt = Convert.ToDecimal(0);
                        //mobj1.CGSTRate = Convert.ToDecimal(0);
                        //mobj1.SGSTAmt = Convert.ToDecimal(0);
                        //mobj1.SGSTRate = Convert.ToDecimal(0);
                        //ctxTFAT.Ledger.Add(mobj1);

                        int mmcnt = 1;
                        //foreach (var a in costcentreresult)
                        //{
                        //    CostLedger CL = new CostLedger();
                        //    CL.Branch = a.CostBranch;
                        //    CL.Code = Model.Account;
                        //    CL.CompCode = mcompcode;
                        //    CL.CostCode = Convert.ToInt32(a.CostCentre);
                        //    CL.CostGrp = Convert.ToInt32(a.CostCentre);
                        //    CL.Credit = (a.addAmtType == "Credit") ? a.Amount : 0;
                        //    CL.Debit = (a.addAmtType == "Debit") ? a.Amount : 0;
                        //    CL.DocAmount = a.Amount;
                        //    CL.DocDate = prevfirstdate;
                        //    CL.LocationCode = 100001;
                        //    CL.MainType = "JV";
                        //    CL.ParentKey = Model.ParentKey;
                        //    CL.Prefix = Model.Prefix;
                        //    CL.Sno = mmcnt;
                        //    CL.Srl = Model.Srl;
                        //    CL.SrNo = mmcnt;
                        //    CL.SubType = "RJ";
                        //    CL.TableKey = "XOP00" + Model.Prefix.Substring(0, 2) + mmcnt.ToString("D3") + Model.Srl;
                        //    CL.Type = "XOP00";
                        //    CL.ENTEREDBY = muserid;
                        //    CL.LASTUPDATEDATE = DateTime.Now;
                        //    CL.AUTHORISE = Model.AUTHORISE;
                        //    CL.AUTHIDS = muserid;
                        //    ctxTFAT.CostLedger.Add(CL);
                        //    ++mmcnt;
                        //}

                        foreach (var item in result)
                        {
                            var Srr = GetLastSerial("Ledger", item.CostBranch, "XOP00", Model.Prefix, "RJ", prevfirstdate);
                            NewSrl.Add(Srr);
                        }
                        DeUpdate(Model);

                        foreach (var item in result)
                        {

                            var SRL = NewSrl.ElementAt((mmcnt - 1));


                            //var doct = ConvertDDMMYYTOYYMMDD(item.addDocuDate);
                            item.Prefix = Model.Prefix;
                            Ledger mobj1 = new Ledger();
                            mobj1.Srl = SRL;
                            mobj1.TableKey = "XOP00" + Model.Prefix.Substring(0, 2) + mmcnt.ToString("D3") + SRL;
                            mobj1.ParentKey = "XOP00" + Model.Prefix.Substring(0, 2) + SRL;
                            mobj1.Sno = mmcnt;
                            mobj1.SubType = "RJ";
                            mobj1.Type = "XOP00";
                            mobj1.AltCode = Model.Account;
                            mobj1.Audited = false;
                            mobj1.AUTHIDS = muserid;
                            mobj1.AUTHORISE = Model.AUTHORISE;
                            mobj1.BillDate = prevfirstdate;
                            mobj1.BillNumber = "";
                            mobj1.Branch = item.CostBranch;
                            mobj1.CompCode = mcompcode;
                            mobj1.Cheque = "";
                            mobj1.ChequeReturn = false;
                            mobj1.ClearDate = Convert.ToDateTime("01-01-1950");
                            mobj1.ChequeDate = Convert.ToDateTime("1900-01-01");
                            mobj1.Code = Model.Account;
                            mobj1.CrPeriod = 0;
                            mobj1.CurrName = Convert.ToInt32(Model.CurrCode);
                            mobj1.CurrRate = Model.CurrRate;
                            mobj1.Debit = (item.addAmtType == "Debit") ? item.Debit : 0;
                            mobj1.Credit = (item.addAmtType == "Credit") ? item.Credit : 0;
                            mobj1.Discounted = true;
                            mobj1.DocDate = prevfirstdate;
                            mobj1.MainType = "JV";
                            mobj1.Narr = "";
                            mobj1.Party = Model.Account;
                            mobj1.Prefix = Model.Prefix;
                            mobj1.RecoFlag = "";
                            mobj1.RefDoc = "";
                            mobj1.TDSChallanNumber = "";
                            mobj1.TDSCode = 0;
                            mobj1.TDSFlag = false;

                            mobj1.ENTEREDBY = muserid;
                            mobj1.DueDate = Convert.ToDateTime("1900-01-01");
                            mobj1.Reminder = false;
                            mobj1.TaskID = 0;
                            mobj1.ChqCategory = 0;
                            mobj1.CurrAmount = Model.CurrRate * Model.Amount;
                            mobj1.LocationCode = 100001;
                            mobj1.LASTUPDATEDATE = DateTime.Now;
                            mobj1.GSTType = 0;
                            mobj1.IGSTAmt = Convert.ToDecimal(0);
                            mobj1.IGSTRate = Convert.ToDecimal(0);
                            mobj1.CGSTAmt = Convert.ToDecimal(0);
                            mobj1.CGSTRate = Convert.ToDecimal(0);
                            mobj1.SGSTAmt = Convert.ToDecimal(0);
                            mobj1.SGSTRate = Convert.ToDecimal(0);

                            ctxTFAT.Ledger.Add(mobj1);
                            ++mmcnt;
                        }
                    }
                    else
                    {
                        DeUpdate(Model);
                        if (Model.Mode == "Add")
                        {
                            if (result == null)
                            {
                                return Json(new
                                {
                                    Status = "failure",
                                    Message = "Nothing to Save.."
                                }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        foreach (var item in result)
                        {
                            var doctypes2 = ctxTFAT.DocTypes.Where(x => x.Code == item.Type).Select(x => x).FirstOrDefault();
                            var doct = ConvertDDMMYYTOYYMMDD(item.addDocuDate);
                            item.Prefix = GetPrefix(doct);
                            Ledger mobj1 = new Ledger();
                            mobj1.TableKey = item.Type + item.Prefix.Substring(0, 2) + "001" + item.InvNumber;
                            mobj1.ParentKey = item.Type + item.Prefix.Substring(0, 2) + item.InvNumber;
                            mobj1.Sno = 1;
                            mobj1.Srl = item.InvNumber;
                            mobj1.SubType = doctypes2.SubType;
                            mobj1.Type = item.Type;
                            mobj1.AltCode = Model.Account;
                            mobj1.Audited = false;
                            mobj1.AUTHIDS = muserid;
                            mobj1.AUTHORISE = Model.AUTHORISE;
                            mobj1.BillDate = ConvertDDMMYYTOYYMMDD(item.addDocuDate);
                            mobj1.BillNumber = String.IsNullOrEmpty(item.BillNumber)==true?"   ": item.BillNumber;
                            mobj1.Branch = item.Branch;
                            mobj1.CompCode = mcompcode;
                            mobj1.Cheque = "";
                            mobj1.ChequeReturn = false;
                            mobj1.ClearDate = Convert.ToDateTime("01-01-1950");
                            mobj1.ChequeDate = Convert.ToDateTime("1900-01-01");
                            mobj1.Code = Model.Account;
                            mobj1.CrPeriod = 0;
                            mobj1.CurrName = (item.CurrName == null || item.CurrName == "") ? 0 : Convert.ToInt32(item.CurrName);
                            mobj1.CurrRate = (item.CurrRate == null) ? 0 : item.CurrRate;
                            mobj1.Debit = (item.addAmtType == "Debit") ? item.Debit : 0;
                            mobj1.Credit = (item.addAmtType == "Credit") ? item.Credit : 0;
                            mobj1.Discounted = true;
                            mobj1.DocDate = ConvertDDMMYYTOYYMMDD(item.addDocuDate);
                            mobj1.MainType = doctypes2.MainType;
                            mobj1.Narr = item.Narr != null ? item.Narr : "";
                            if (!String.IsNullOrEmpty(item.CustChild))
                            {
                                mobj1.Party = item.CustChild;
                            }
                            else
                            {
                                mobj1.Party = Model.Account;
                            }
                            //mobj1.Party = Model.Account;
                            mobj1.Prefix = GetPrefix(doct);
                            mobj1.RecoFlag = "";
                            mobj1.RefDoc = item.Type == "FM000" ? "B" : "";//This is used to save additional bill amount which is not pending 
                            mobj1.TDSChallanNumber = "";
                            mobj1.TDSCode = item.TDSCode;
                            mobj1.TDSFlag = item.TDSFlag;

                            mobj1.ENTEREDBY = muserid;
                            mobj1.DueDate = Convert.ToDateTime("1900-01-01");
                            mobj1.Reminder = false;
                            mobj1.TaskID = 0;
                            mobj1.ChqCategory = 0;
                            mobj1.CurrAmount = item.CurrRate * item.addAmount;
                            mobj1.LocationCode = 100001;
                            mobj1.LASTUPDATEDATE = DateTime.Now;
                            mobj1.GSTType = 0;
                            mobj1.IGSTAmt = Convert.ToDecimal(item.IGSTAmt);
                            mobj1.IGSTRate = Convert.ToDecimal(item.IGSTRate);
                            mobj1.CGSTAmt = Convert.ToDecimal(item.CGSTAmt);
                            mobj1.CGSTRate = Convert.ToDecimal(item.CGSTRate);
                            mobj1.SGSTAmt = Convert.ToDecimal(item.SGSTAmt);
                            mobj1.SGSTRate = Convert.ToDecimal(item.SGSTRate);

                            ctxTFAT.Ledger.Add(mobj1);
                            ++mCnt;
                        }
                    }

                    if (Model.AUTHORISE.Substring(0, 1) != "A")
                    {
                        string mAuthUser;
                        if (Model.AUTHORISE.Substring(0, 1) == "D")
                        {
                            //mAuthUser=SaveAUTHORISEDOC( Model.ParentKey, 0, Model.Date, mcurrency, Model.Branch, muserid);
                        }
                        else
                        {
                            mAuthUser = SaveAuthorise(Model.ParentKey, 0, DateTime.Now.ToString(), Convert.ToInt32(Model.CurrCode), 1, DateTime.Now, "", Model.Branch, muserid, -1);
                            //SendAUTHORISEMessage(mAuthUser, Model.ParentKey, Model.Date);
                        }
                    }
                    UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, Model.Amount, Model.Account, "Save Accounts Opening Definition", "A");
                    //SendTrnsMsg(Model.Mode, Model.Amount, Model.Branch + Model.ParentKey, DateTime.Now, "");
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    Session["AccountOpeningList"] = null;

                    return Json(new { Status = "Success", Model, NewSrl = (Model.Branch + Model.ParentKey) }, JsonRequestBehavior.AllowGet);
                }

                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    string dd1 = ex1.InnerException.Message;
                    return Json(new
                    {
                        Status = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    string dd = ex.InnerException.InnerException.Message;
                    return Json(new
                    {
                        Status = "Error",
                        Message = dd
                    }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public ActionResult DeleteData(AccountOpeningVM Model)
        {
            string Msssage = "Document is Already Adjusted In Cash Bank Against : ", Message1 = "", Message2 = "";
            Message1 = " Cant " + Model.Mode;
            DateTime firstdate = ctxTFAT.TfatPerd.OrderBy(z => z.StartDate).Select(x => x.StartDate).FirstOrDefault();
            var prevfirstdate = firstdate.AddDays(-1);
            var mDeleteLed = ctxTFAT.Ledger.Where(x => x.Code == Model.Account && x.DocDate <= prevfirstdate).ToList();
            var mDeleteCC = ctxTFAT.CostLedger.Where(x => x.Code == Model.Account && x.DocDate <= prevfirstdate).ToList();
            if (mDeleteLed.Count() > 0)
            {
                foreach (var a in mDeleteLed)
                {
                    var CheckDependency2 = ctxTFAT.Outstanding.Where(x => x.aType == a.Type && x.aSrl == a.Srl && x.aPrefix == a.Prefix && x.DocBranch == Model.Branch).Select(x => x).FirstOrDefault();
                    if (CheckDependency2 != null)
                    {
                        if (CheckDependency2.Type == Model.Type && CheckDependency2.Srl == Model.Srl && CheckDependency2.Prefix == Model.Prefix && CheckDependency2.DocBranch == Model.Branch)
                        {

                        }
                        else
                        {
                            Model.AdjustBill = true;

                            Message2 += CheckDependency2.aSrl.ToString() + ", ";
                        }

                    }
                }
            }

            if (mDeleteCC.Count() > 0)
            {
                foreach (var a in mDeleteCC)
                {
                    var CheckDependency2 = ctxTFAT.Outstanding.Where(x => x.aType == a.Type && x.aSrl == a.Srl && x.aPrefix == a.Prefix && x.DocBranch == Model.Branch).Select(x => x).FirstOrDefault();
                    if (CheckDependency2 != null)
                    {
                        if (CheckDependency2.Type == Model.Type && CheckDependency2.Srl == Model.Srl && CheckDependency2.Prefix == Model.Prefix && CheckDependency2.DocBranch == Model.Branch)
                        {

                        }
                        else
                        {
                            Model.AdjustBill = true;

                            Message2 += CheckDependency2.aSrl.ToString() + ", ";
                        }

                    }
                }
            }


            if (Model.AdjustBill)
            {
                Message2 = Message2.Substring(0, Message2.Length - 1);
                Model.Message = Msssage + Message2 + Message1;
                return Json(new { Status = "Error", Message = Model.Message }, JsonRequestBehavior.AllowGet);
            }

            DeUpdate(Model);

            Session["AccountOpeningList"] = null;
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, Model.Amount, Model.Account, "Delete Accounts Opening Definition", "A");

            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        private string GetPrefix(DateTime mDate)
        {
            string mstr = "";
            var perdstring = ctxTFAT.TfatPerd.Select(x => x).OrderBy(x => x.StartDate).FirstOrDefault();
            string p1 = perdstring.PerdCode.Substring(2, 2);
            string p2 = perdstring.PerdCode.Substring(6, 2);

            int d10 = Convert.ToInt16(perdstring.PerdCode.Substring(0, 2));
            d10 = d10 - 1;
            string d1 = d10.ToString();

            int d20 = Convert.ToInt16(perdstring.PerdCode.Substring(4, 2));
            d20 = d20 - 1;
            string d2 = d20.ToString();
            mstr = d1 + p1 + d2 + p2;
            return (mstr);
        }
    }
}