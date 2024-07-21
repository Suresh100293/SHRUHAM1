using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
using ALT_ERP3.Models;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class CashBankController : BaseController
    {
        List<SelectListItem> branch = new List<SelectListItem>();
        string attachmentPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentPath"].ToString()) == true ? "" : ConfigurationManager.AppSettings["AttachmentPath"].ToString();
        private static string msubtype = "";
        private static bool gpHolidayWarn = false;
        private static string gpHoliday1 = "Sat";
        private static string gpHoliday2 = "Sun";
        public ActionResult Index(LedgerVM Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            int mcurrency = (int)FieldoftableNumber("TfatBranch", "CurrName", "Code='" + mbranchcode + "'");
            //ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(z => z.CurrName).FirstOrDefault();
            if (mcurrency == 0)
            {
                mcurrency = 1;//india
            }

            Session["ledgerlist"] = null;
            Session["TempCashBkAttach"] = null;
            Model.ViewDataId = Model.ViewDataId;
            Model.Controller2 = Model.Controller2;
            Model.optiontype = Model.optiontype;
            Model.OptionCode = Model.OptionCode;
            Model.Header = Model.Header;
            Model.TableName = Model.TableName;
            Model.Module = Model.Module;
            Model.Authorise = "A00";
            string[] mAuth = FieldArray("TfatUserAuditHeader", new string[] { "AuthLock", "AuthNoPrint", "AuthReq" }, new string[] { "L", "L", "L" }, "Type='" + Model.Type + "'");
            Model.AuthLock = Convert.ToBoolean(mAuth[0]);
            Model.AuthNoPrint = Convert.ToBoolean(mAuth[1]);
            Model.AuthReq = Convert.ToBoolean(mAuth[2]);

            //var mAuth = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == Model.Type).Select(x => new
            //{
            //    x.AuthLock,
            //    x.AuthNoPrint,
            //    x.AuthReq
            //}).FirstOrDefault();
            //if (mAuth != null)
            //{
            //    Model.AuthLock = mAuth.AuthLock;
            //    Model.AuthNoPrint = mAuth.AuthNoPrint;
            //    Model.AuthReq = mAuth.AuthReq;
            //}

            UpdateAuditTrail(mbranchcode, Model.ChangeLog, Model.Header, "", DateTime.Now, 0, "", "", "NA");
            if (Model.ChangeLog == "Add")
            {
                List<LedgerVM> OSAdjList = new List<LedgerVM>();
                mAuth = FieldArray("DocTypes", new string[] { "MainType", "SubType", "Name", "ConstantMode", "Constant", "LockPosting" }, new string[] { "T", "T", "T", "N", "T", "L" }, "Code='" + Model.Type + "'");
                //var result = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => new
                //{
                //    x.MainType,
                //    x.SubType,
                //    x.Name,
                //    x.ConstantMode,
                //    x.Constant,
                //    x.LockPosting
                //}).FirstOrDefault();

                Model.BankCashCode = mAuth[4].ToString();// result.Constant;
                if (Model.BankCashCode == "" || Model.BankCashCode == null)
                {
                }
                else
                {
                    Model.BankCashName = NameofAccount(Model.BankCashCode, "A");
                    //ctxTFAT.Master.Where(x => x.Code == Model.BankCashCode).Select(x => x.Name).FirstOrDefault().ToString();
                }

                Model.ConstantMode = Convert.ToInt32(mAuth[3]); //== null ? 0 : result.ConstantMode.Value;
                int mconstantmode = Model.ConstantMode;
                Model.id = mAuth[2].ToString();// result.Name;
                Model.Branch = mbranchcode;
                Model.BalanceAmt = GetBalance(Model.BankCashCode, DateTime.Now.Date, Model.Branch);
                Model.MainType = mAuth[0].ToString();// result.MainType;
                Model.SubType = mAuth[1].ToString();// result.SubType;
                msubtype = Model.SubType;
                Model.Prefix = mperiod;
                Model.Document = "";
                Model.LockPosting = Convert.ToBoolean(mAuth[5]);// result.LockPosting;
                Model.DocDate = GetEffectiveDate();
                //int CurrCode = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.CurrName).FirstOrDefault();
                Model.CurrCode = mcurrency.ToString();
                Model.CurrName = NameofAccount(mcurrency, "C");
                //GetCurrName(Model.CurrCode);
                Model.CurrRate = FieldoftableNumber("CurrencyMaster", "CurrRate", "Code=" + mcurrency);
                //ctxTFAT.CurrencyMaster.Where(x => x.Code == mcurrency).Select(x => x.CurrRate).FirstOrDefault();
                List<AddOns> objitemlist = new List<AddOns>();
                if (Model.ChangeLog == "Add")
                {
                    DataTable addons = GetDataTable("Select * from AddOns Where TableKey='%Doc" + Model.MainType + "' and Hide=0 and Charindex('" + Model.Type + "',Types)<>0");
                    //var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%Doc" + Model.MainType && x.Hide == false && x.Types.Contains(Model.Type))
                    //            select i).ToList();
                    foreach (DataRow i in addons.Rows)
                    //foreach (var i in addons)
                    {
                        AddOns c = new AddOns();
                        c.Fld = i["Fld"].ToString();
                        c.Head = i["Head"].ToString();
                        c.ApplCode = "";
                        c.QueryText = i["QueryText"] == null ? "" : GetQueryTextAddon(i["QueryText"].ToString());
                        c.FldType = i["FldType"].ToString();
                        c.PlaceValue = i["PlaceValue"].ToString();
                        c.Eqsn = i["Eqsn"].ToString();
                        objitemlist.Add(c);
                    }
                    Model.AddOnList = objitemlist;
                }

                if (Model.CurrRate == 0)
                {
                    Model.CurrRate = 1;
                }
                if (Model.MainType == "JV")
                {
                    Model.ConstantMode = 2;
                }
                if ((Model.ConstantMode == 0 || Model.ConstantMode == 1))
                {
                    List<LedgerVM> objledgerdetail = new List<LedgerVM>();
                    if (Session["ledgerlist"] != null)
                    {
                        objledgerdetail = (List<LedgerVM>)Session["ledgerlist"];
                    }
                    objledgerdetail.Add(new LedgerVM()
                    {
                        Branch = mbranchcode,
                        Code = Model.BankCashCode,
                        AccountName = Model.BankCashName,
                        PaymentMode = Model.PaymentMode,
                        Cheque = "",
                        StrChqDate = "",
                        MainType = Model.MainType,
                        CurrCode = "0",
                        CurrName = "",
                        Debit = 0,
                        Credit = 0,
                        DraweeCode = "",
                        RefDoc = "",
                        RefParty = "",
                        RefPartyName = "",
                        GSTType = "",
                        GSTNoItc = false,
                        Taxable = 0,
                        IGSTRate = 0,
                        IGSTAmt = 0,
                        CGSTRate = 0,
                        CGSTAmt = 0,
                        SGSTRate = 0,
                        SGSTAmt = 0,
                        TransactionType = "",
                        StrDueDate = "",
                        TDSCess = 0,
                        TDSSurch = 0,
                        SHECess = 0,
                        TDSRate = 0,
                        TDSAmt = 0,
                        TDSFlag = false,
                        TDSCode = 0,
                        Narr = "",
                        OSAdjList = OSAdjList,
                        LocationCode = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.LocationCode).FirstOrDefault(),
                        ClearDate = Convert.ToDateTime("1950-01-01"),
                        SrNo = objledgerdetail.Count + 1,
                        tempId = objledgerdetail.Count + 1,
                        tempIsDeleted = false,
                        ProjCode = "",
                    });

                    Session.Add("ledgerlist", objledgerdetail);
                    decimal sumdebit = objledgerdetail.Where(x => x.tempIsDeleted == false).Sum(x => x.Debit);
                    decimal sumcredit = objledgerdetail.Where(x => x.tempIsDeleted == false).Sum(x => x.Credit);
                    Model.Selectedleger = objledgerdetail;
                }
            }
            else
            {
                List<LedgerVM> OSAdjList = new List<LedgerVM>();
                Model.Document = Model.Document;
                Model.Branch = Model.Document.Substring(0, 6);
                Model.ParentKey = Model.Document.Substring(6, (Model.Document.Length - 6));
                Model.Type = Model.ParentKey.Substring(0, 5);
                var result = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => new
                {
                    x.MainType,
                    x.SubType,
                    x.Name,
                    x.ConstantMode,
                    x.Constant,
                    x.LockPosting
                }).FirstOrDefault();
                Model.LockPosting = result.LockPosting;
                Model.MainType = result.MainType;
                Model.SubType = result.SubType;
                var ledger1 = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey).Select(x => x).FirstOrDefault();
                Model.Prefix = ledger1.Prefix;
                Model.RecoFlag = ledger1.RecoFlag;
                Model.Audited = ledger1.Audited;
                Model.AddOnList = GetAddOnListOnEdit(Model.MainType, Model.ParentKey, Model.Type);
                Model.NarrStr = ctxTFAT.Narration.Where(x => x.ParentKey == Model.ParentKey).Select(x => x.Narr).FirstOrDefault() ?? "";
                if (Model.MainType == "JV")
                {
                    Model.ConstantMode = 2;
                }
                List<LedgerVM> objledgerdetail = new List<LedgerVM>();
                var mobj = (from l in ctxTFAT.Ledger
                            join td in ctxTFAT.TDSPayments on new { l.ParentKey, l.TableKey } equals new { td.ParentKey, td.TableKey } into tds
                            from lefttds in tds.DefaultIfEmpty()
                            where l.ParentKey == Model.ParentKey
                            select new
                            {
                                l.Branch,
                                l.Code,
                                l.ChqCategory,
                                l.Cheque,
                                l.ChequeDate,
                                l.CurrName,
                                l.Debit,
                                l.Credit,
                                l.BankCode,
                                l.RefDoc,
                                l.Party,
                                l.GSTType,
                                l.TaxCode,
                                l.IGSTAmt,
                                l.IGSTRate,
                                l.CGSTAmt,
                                l.CGSTRate,
                                l.SGSTAmt,
                                l.SGSTRate,
                                l.RecoFlag,
                                l.DueDate,
                                TDSCess = (lefttds == null) ? 0 : lefttds.TDSCess,
                                TDSSurch = (lefttds == null) ? 0 : lefttds.TDSSurCharge,
                                SHECess = (lefttds == null) ? 0 : lefttds.TDSSheCess,
                                TDSAmt = (lefttds == null) ? 0 : lefttds.TDSAmt,
                                TDSFlag = l.TDSFlag,
                                Narr = l.Narr,
                                TDSCode = (lefttds == null) ? 0 : lefttds.TDSCode,
                                l.ParentKey,
                                l.TableKey,
                                l.LocationCode,
                                l.ClearDate,
                                l.SLCode,
                                l.ProjCode
                            }).ToList();
                foreach (var item in mobj)
                {
                    objledgerdetail.Add(new LedgerVM()
                    {
                        Branch = item.Branch,
                        Code = item.Code,
                        AccountName = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault(),
                        PaymentMode = (item.ChqCategory == null) ? "" : item.ChqCategory.Value.ToString(),
                        Cheque = item.Cheque,
                        StrChqDate = (item.ChequeDate == null) ? "" : item.ChequeDate.Value.ToString("dd-MM-yyyy"),
                        MainType = Model.MainType,
                        CurrCode = (item.CurrName == 0) ? "0" : item.CurrName.ToString(),
                        Debit = (item.Debit == null) ? 0 : item.Debit.Value,
                        Credit = (item.Credit == null) ? 0 : item.Credit.Value,
                        DraweeCode = (item.BankCode == null) ? "" : item.BankCode.Value.ToString(),
                        RefDoc = item.RefDoc,
                        RefParty = item.Party,
                        RefPartyName = ctxTFAT.Master.Where(x => x.Code == item.Party).Select(x => x.Name).FirstOrDefault(),
                        GSTType = (item.GSTType == null) ? "" : item.GSTType.Value.ToString(),
                        GSTNoItc = false,
                        Taxable = Convert.ToDouble(item.Debit.Value) + Convert.ToDouble(item.Credit.Value),
                        IGSTRate = (item.IGSTRate == null) ? 0 : Convert.ToDouble(item.IGSTRate.Value),
                        IGSTAmt = (item.IGSTAmt == null) ? 0 : Convert.ToDouble(item.IGSTAmt.Value),
                        CGSTRate = (item.CGSTRate == null) ? 0 : Convert.ToDouble(item.CGSTRate.Value),
                        CGSTAmt = (item.CGSTAmt == null) ? 0 : Convert.ToDouble(item.CGSTAmt.Value),
                        SGSTRate = (item.SGSTRate == null) ? 0 : Convert.ToDouble(item.SGSTRate.Value),
                        SGSTAmt = (item.SGSTAmt == null) ? 0 : Convert.ToDouble(item.SGSTAmt.Value),
                        TransactionType = item.RecoFlag,
                        StrDueDate = (item.DueDate == null) ? "" : item.DueDate.Value.ToString("dd-MM-yyyy"),
                        TDSCess = (item.TDSCess == null) ? 0 : Convert.ToDouble(item.TDSCess.Value),
                        TDSSurch = (item.TDSSurch == null) ? 0 : Convert.ToDouble(item.TDSSurch.Value),
                        SHECess = (item.SHECess == null) ? 0 : Convert.ToDouble(item.SHECess.Value),
                        TDSRate = GetTDSRate(item.TDSCode),
                        TDSAmt = (item.TDSAmt == null) ? 0 : Convert.ToDouble(item.TDSAmt.Value),
                        TDSFlag = item.TDSFlag,
                        TDSCode = (item.TDSCode == null) ? 0 : item.TDSCode.Value,
                        Narr = item.Narr,
                        AmtType = (item.Debit > 0) ? "Debit" : "Credit",
                        OSAdjList = GetOutstandingInEdit(item.ParentKey, item.TableKey, Model.MainType),
                        costcentre1 = GetCCenterInEdit(item.ParentKey, item.TableKey, Model.MainType),
                        SubLedgerList = GetSubLedgerListInEdit(item.ParentKey, item.TableKey, Model.MainType),
                        LocationCode = item.LocationCode,
                        GSTCode = item.TaxCode,
                        SLCode = (int)(item.SLCode == null ? 0 : item.SLCode),
                        ClearDate = item.ClearDate == null ? Convert.ToDateTime("1950-01-01") : item.ClearDate.Value,
                        SrNo = objledgerdetail.Count + 1,
                        tempId = objledgerdetail.Count + 1,
                        tempIsDeleted = false,
                        ProjCode = item.ProjCode
                    });
                }
                Session.Add("ledgerlist", objledgerdetail);
                Model.SumDebit = objledgerdetail.Where(x => x.tempIsDeleted == false).Sum(x => x.Debit);
                Model.SumCredit = objledgerdetail.Where(x => x.tempIsDeleted == false).Sum(x => x.Credit);
                var ledger = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey).Select(x => x).FirstOrDefault();
                Model.Authorise = ledger.AUTHORISE;
                Model.DocDate = ledger.DocDate;
                Model.Branch = ledger.Branch;
                Model.Srl = ledger.Srl;
                Model.BankCashCode = ledger.Code;
                Model.BankCashName = ctxTFAT.Master.Where(x => x.Code == ledger.Code).Select(x => x.Name).FirstOrDefault();
                Model.BalanceAmt = GetBalance(Model.BankCashCode, DateTime.Now.Date, Model.Branch);
                Model.CurrCode = (ledger.CurrName == 0) ? "" : ledger.CurrName.ToString();
                Model.CurrName = GetCurrName(Model.CurrCode);
                Model.CurrRate = ledger.CurrRate;
                Model.Selectedleger = objledgerdetail;
                var tablerefkeys = mobj.Select(x => x.TableKey).ToList();
                var KeyList = ctxTFAT.Outstanding.Where(x => x.ParentKey != Model.ParentKey && tablerefkeys.Contains(x.TableRefKey)).Select(x => x.TableKey).ToList();

                if (KeyList != null && KeyList.Count > 0)
                {
                    Model.CheckMode = true;

                    Model.Message = "Document is Already Adjusted Against: " + KeyList.FirstOrDefault() + ", Cant Edit";
                }
                if (Model.ConstantMode != 2)
                {
                    var maccnames = objledgerdetail.Where(x => x.tempId != 1).Select(x => x.Code).ToList();
                    ViewBag.AccountNameList = String.Join(",", maccnames);
                }
                else
                {
                    var maccnames = objledgerdetail.Select(x => x.Code).ToList();
                    ViewBag.AccountNameList = String.Join(",", maccnames);
                }

                #region ATTACHMENT
                Model.DocumentList = GetAttachmentListInEdit(Model);
                string docstr = "";
                if (Model.DocumentList.Count > 0)
                {
                    foreach (var a in Model.DocumentList)
                    {
                        docstr = docstr + a.ImageStr + ",";
                    }
                    if (docstr != "")
                    {
                        docstr = docstr.Remove(docstr.Length - 1);
                    }
                    string docfilnam = "";
                    foreach (var b in Model.DocumentList)
                    {
                        docfilnam = docfilnam + b.FileName + ",";
                    }
                    if (docfilnam != "")
                    {
                        docfilnam = docfilnam.Remove(docfilnam.Length - 1);
                    }
                    Model.AllFileStr = docstr;
                    Model.FileNameStr = docfilnam;
                }
                Session["TempCashBkAttach"] = Model.DocumentList;
                #endregion
            }
            // lock if Authorised
            if (Model.ChangeLog != "Add" && Model.AuthReq == true && Model.Authorise.Substring(0, 1) == "A" && Model.AuthLock)
            {
                Model.Mode = "View";
            }
            return View(Model);
        }

        #region Get
        [HttpGet]
        public ActionResult GetAccountList(string term, string BaseGr, string Code)
        {
            List<int> partycategs = new List<int>();
            List<string> accountgrps = new List<string>();
            var partycategs2 = ctxTFAT.TypePartyCategory.Where(x => x.Type == Code).Select(x => x.Category).ToList();
            var accountgrps2 = ctxTFAT.TypeAccountGroups.Where(x => x.Type == Code).Select(x => x.Code).ToList();
            if (partycategs2 != null)
            {
                partycategs = partycategs2;
            }
            if (accountgrps2 != null)
            {
                accountgrps = accountgrps2;
            }
            if (term == "")
            {
                if (accountgrps.Count() > 0 && partycategs.Count() == 0)//associated accgroup available but party categ are not
                {
                    var result = (from m in ctxTFAT.Master
                                  where (m.AppBranch.Contains(mbranchcode) || m.AppBranch=="All") && accountgrps.Contains(m.Grp) && m.Hide == false
                                  select new
                                  {
                                      m.Code,
                                      Name = m.Name + " " + " [" + ctxTFAT.Address.Where(z => z.Code == m.Code && z.Sno == 0).Select(x => x.City).FirstOrDefault() + "]" + " [" + ctxTFAT.Address.Where(z => z.Code == m.Code && z.Sno == 0).Select(x => x.City).FirstOrDefault() + "]",
                                      OName = m.Name
                                  }).OrderBy(n => n.OName).Take(10).ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else if (accountgrps.Count() > 0 && partycategs.Count() > 0)
                {
                    var result = (from m in ctxTFAT.Master
                                  where (m.AppBranch.Contains(mbranchcode) || m.AppBranch == "All") && accountgrps.Contains(m.Grp) && partycategs.Contains((int?)m.Category ?? 0) && m.Hide == false
                                  select new
                                  {
                                      m.Code,
                                      Name = m.Name + " " + " [" + ctxTFAT.Address.Where(z => z.Code == m.Code && z.Sno == 0).Select(x => x.City).FirstOrDefault() + "]",
                                      OName = m.Name
                                  }).OrderBy(n => n.OName).Take(10).ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else if (accountgrps.Count() == 0 && partycategs.Count() > 0)
                {
                    var result = (from m in ctxTFAT.Master

                                  where (m.AppBranch.Contains(mbranchcode) || m.AppBranch == "All") && partycategs.Contains((int?)m.Category ?? 0) && m.Hide == false
                                  select new
                                  {
                                      m.Code,
                                      Name = m.Name + " " + " [" + ctxTFAT.Address.Where(z => z.Code == m.Code && z.Sno == 0).Select(x => x.City).FirstOrDefault() + "]",
                                      OName = m.Name
                                  }).OrderBy(n => n.OName).Take(10).ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = (from m in ctxTFAT.Master
                                  where (m.AppBranch.Contains(mbranchcode) || m.AppBranch == "All") && m.Hide == false
                                  select new
                                  {
                                      m.Code,
                                      Name = m.Name + " " + " [" + ctxTFAT.Address.Where(z => z.Code == m.Code && z.Sno == 0).Select(x => x.City).FirstOrDefault() + "]",
                                      OName = m.Name
                                  }).OrderBy(n => n.OName).Take(10).ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                if (accountgrps.Count() > 0 && partycategs.Count() == 0)//associated accgroup available but party categ are not
                {
                    var result = (from m in ctxTFAT.Master
                                  where (m.Name.Contains(term) || m.ShortName.Contains(term)) && (m.AppBranch.Contains(mbranchcode) || m.AppBranch == "All") && accountgrps.Contains(m.Grp) && m.Hide == false
                                  select new
                                  {
                                      m.Code,
                                      Name = m.Name + " " + " [" + ctxTFAT.Address.Where(z => z.Code == m.Code && z.Sno == 0).Select(x => x.City).FirstOrDefault() + "]",
                                      OName = m.Name
                                  }).OrderBy(n => n.OName).ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else if (accountgrps.Count() > 0 && partycategs.Count() > 0)
                {
                    var result = (from m in ctxTFAT.Master
                                  where (m.Name.Contains(term) || m.ShortName.Contains(term)) && (m.AppBranch.Contains(mbranchcode) || m.AppBranch == "All") && accountgrps.Contains(m.Grp) && partycategs.Contains((int?)m.Category ?? 0) && m.Hide == false
                                  select new
                                  {
                                      m.Code,
                                      Name = m.Name + " " + " [" + ctxTFAT.Address.Where(z => z.Code == m.Code && z.Sno == 0).Select(x => x.City).FirstOrDefault() + "]",
                                      OName = m.Name
                                  }).OrderBy(n => n.OName).ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else if (accountgrps.Count() == 0 && partycategs.Count() > 0)
                {
                    var result = (from m in ctxTFAT.Master
                                  where (m.Name.Contains(term) || m.ShortName.Contains(term)) && (m.AppBranch.Contains(mbranchcode) || m.AppBranch == "All") && partycategs.Contains((int?)m.Category ?? 0) && m.Hide == false
                                  select new
                                  {
                                      m.Code,
                                      Name = m.Name + " " + " [" + ctxTFAT.Address.Where(z => z.Code == m.Code && z.Sno == 0).Select(x => x.City).FirstOrDefault() + "]",
                                      OName = m.Name
                                  }).OrderBy(n => n.OName).ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = (from m in ctxTFAT.Master
                                  where (m.Name.Contains(term) || m.ShortName.Contains(term)) && m.Hide == false
                                  select new
                                  {
                                      m.Code,
                                      Name = m.Name + " " + " [" + ctxTFAT.Address.Where(z => z.Code == m.Code && z.Sno == 0).Select(x => x.City).FirstOrDefault() + "]",
                                      OName = m.Name
                                  }).OrderBy(n => n.OName).ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult GetCashBankPartyList(string term)
        {
            if (term == "")
            {
                var result = ctxTFAT.CustomerMaster.Where(x => x.Hide == false).Select(m => new { m.Code, m.Name }).Take(10).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.CustomerMaster.Where(x => x.Hide == false && x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).Take(10).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetCashBankList(string SubType)
        {
            //var contstantbank = ctxTFAT.DocTypes.Where(x => x.Code == mP2).Select(x => new { x.Constant, x.SubType }).FirstOrDefault();
            if (SubType == "BR" || SubType == "BP")
            {
                var result = ctxTFAT.Master.Where(x => x.BaseGr == "B" && x.Hide == false && (x.AppBranch.Contains(mbranchcode) || x.AppBranch=="All")).Select(c => new { c.Code, c.Name }).ToList();//&& x.AppBranch.Contains(mbranchcode)
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.Master.Where(x => x.BaseGr == "C" && x.Hide == false && (x.AppBranch.Contains(mbranchcode) || x.AppBranch == "All")).Select(c => new { c.Code, c.Name }).ToList();// && x.AppBranch.Contains(mbranchcode)
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

        public ActionResult GetWarehouseList(string term)
        {
            var warehouselist = ctxTFAT.Warehouse.Where(z => (z.Users + ",").Contains(muserid + ",")).Select(b => new
            {
                b.Code,
                b.Name
            }).ToList();
            foreach (var item in warehouselist)
            {
                branch.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }
            return Json(branch, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBranchList()
        {
            var branchlist = ctxTFAT.TfatBranch.Where(x => x.CompCode == mcompcode).ToList().Select(b => new
            {
                b.Code,
                b.Name
            });
            foreach (var item in branchlist)
            {
                branch.Add(new SelectListItem { Text = item.Name, Value = item.Code });
            }
            return Json(branch, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult GetLoanTrxTypes(string term)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "0", Text = "Principle AMt" });
            GSt.Add(new SelectListItem { Value = "1", Text = "Interest Amt" });
            GSt.Add(new SelectListItem { Value = "2", Text = "TDS Amt" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public string GetLoanTrxTypesName(string Code)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "0", Text = "Principle AMt" });
            GSt.Add(new SelectListItem { Value = "1", Text = "Interest Amt" });
            GSt.Add(new SelectListItem { Value = "2", Text = "TDS Amt" });
            string Name = GSt.Where(x => x.Value == Code).Select(x => x.Text).FirstOrDefault();
            return Name;
        }

        public ActionResult GetGSTList(string term)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.TaxMaster.Where(x => x.VATGST == true).Select(x => new { x.Code, x.Name }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.TaxMaster.Where(x => x.VATGST == true && x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetPaymentMode(string term)
        {
            var mPara = term.Split('^');
            var mP1 = mPara[0].ToLower();
            var mP2 = mPara[0];
            List<SelectListItem> GSt = new List<SelectListItem>();
            if (mP2 == "TP")
            {
                GSt.Add(new SelectListItem { Value = "0", Text = "Cash" });
            }
            else
            {
                GSt.Add(new SelectListItem { Value = "0", Text = "Cash" });
                GSt.Add(new SelectListItem { Value = "1", Text = "Cheque" });
                GSt.Add(new SelectListItem { Value = "2", Text = "Transfer" });
                GSt.Add(new SelectListItem { Value = "3", Text = "Other" });
                GSt.Add(new SelectListItem { Value = "4", Text = "CreditCard" });
            }
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public string GetPaymentModeName(string Code)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "0", Text = "Cash" });
            GSt.Add(new SelectListItem { Value = "1", Text = "Cheque" });
            GSt.Add(new SelectListItem { Value = "2", Text = "Transfer" });
            GSt.Add(new SelectListItem { Value = "3", Text = "Other" });
            GSt.Add(new SelectListItem { Value = "4", Text = "CreditCard" });
            string Name = GSt.Where(x => x.Value == Code).Select(x => x.Text).FirstOrDefault();
            return Name;
        }

        public ActionResult GetGSTRateDetail(string Code, string Party)
        {
            var pstate = ctxTFAT.Address.Where(x => x.Code == Party).Select(x => x.State).FirstOrDefault();
            if (String.IsNullOrEmpty(pstate))
            {
                pstate = "19";
            }
            var branchstate = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.State).FirstOrDefault();
            if (pstate == branchstate)
            {
                var result = ctxTFAT.TaxMaster.Where(x => x.Code == Code).Select(x => new { IGSTRate = (decimal)0, SGSTRate = x.SGSTRate.Value, CGSTRate = x.CGSTRate.Value }).FirstOrDefault();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.TaxMaster.Where(x => x.Code == Code).Select(x => new { IGSTRate = x.IGSTRate.Value, SGSTRate = (decimal)0, CGSTRate = (decimal)0 }).FirstOrDefault();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult GetChequeList(string term)
        //{
        //    if (term == "" || term == null)
        //    {
        //        var result = ctxTFAT.Ledger.Select(x => new { Code = x.Cheque, Name = x.Cheque }).ToList();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        var result = ctxTFAT.Ledger.Where(x => x.Cheque.Contains(term)).Select(m => new { Code = m.Cheque, Name = m.Cheque }).ToList();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public ActionResult GetTDSCodeList(string term)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.TDSMaster.Select(x => new { Code = x.Code, Name = x.Name }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.TDSMaster.Where(x => x.Name.Contains(term)).Select(m => new { Code = m.Code, Name = m.Name }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetTDSRateDetail(LedgerVM Model)
        {
            var TDSRate = ctxTFAT.TDSRates.Where(x => x.Code == Model.TDSCode).Select(x => new { x.TDSRate, x.Cess, x.SurCharge, x.SHECess }).FirstOrDefault();
            if (TDSRate != null)
            {
                Model.TDSRate = (TDSRate.TDSRate == null) ? 0 : Convert.ToDecimal(TDSRate.TDSRate.Value);
                Model.TDSCess = (TDSRate.Cess == null) ? 0 : Convert.ToDouble(TDSRate.Cess.Value);
                Model.TDSSurch = (TDSRate.SurCharge == null) ? 0 : Convert.ToDouble(TDSRate.SurCharge.Value);
                Model.SHECess = (TDSRate.SHECess == null) ? 0 : Convert.ToDouble(TDSRate.SHECess.Value);
            }
            else
            {
                Model.TDSRate = 0;
                Model.TDSCess = 0;
                Model.TDSSurch = 0;
                Model.SHECess = 0;
            }
            return Json(Model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDraweeBankList(string term)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.BankMaster.Select(x => new { x.Code, x.Name }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.BankMaster.Where(x => x.Name.Contains(term)).Select(x => new { x.Code, x.Name }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetTDSBoolean(LedgerVM Model)
        {
            Model.CutTDS = ctxTFAT.MasterInfo.Where(x => x.Code == Model.Party).Select(x => x.CutTDS).FirstOrDefault();
            Model.CCReqd = ctxTFAT.Master.Where(x => x.Code == Model.Party).Select(x => x.CCReqd).FirstOrDefault();
            return Json(Model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetGSTType(string term)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "0", Text = "Tax Invoice" });
            GSt.Add(new SelectListItem { Value = "1", Text = "Reverse Charge" });
            GSt.Add(new SelectListItem { Value = "2", Text = "TCS" });
            GSt.Add(new SelectListItem { Value = "3", Text = "TDS" });
            GSt.Add(new SelectListItem { Value = "4", Text = "Bill Of Supply" });
            GSt.Add(new SelectListItem { Value = "5", Text = "Export( Under LUT)" });
            GSt.Add(new SelectListItem { Value = "6", Text = "UnRegistered Dealer" });
            GSt.Add(new SelectListItem { Value = "7", Text = "Sez with Payment" });
            GSt.Add(new SelectListItem { Value = "8", Text = "Sez w/0 Payment" });
            GSt.Add(new SelectListItem { Value = "9", Text = "Export (with Duty)" });
            GSt.Add(new SelectListItem { Value = "10", Text = "Exempted" });
            GSt.Add(new SelectListItem { Value = "11", Text = "No GST" });
            GSt.Add(new SelectListItem { Value = "12", Text = "Composition Dealer" });
            GSt.Add(new SelectListItem { Value = "13", Text = "Deemed Export" });
            GSt.Add(new SelectListItem { Value = "14", Text = "Nil Rated" });
            GSt.Add(new SelectListItem { Value = "15", Text = "Export @ 0.1%" });
            GSt.Add(new SelectListItem { Value = "16", Text = "Export @ 0.05%" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public string GetGSTTypeName(string Val)
        {
            string Name;
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "0", Text = "Tax Invoice" });
            GSt.Add(new SelectListItem { Value = "1", Text = "Reverse Charge" });
            GSt.Add(new SelectListItem { Value = "2", Text = "TCS" });
            GSt.Add(new SelectListItem { Value = "3", Text = "TDS" });
            GSt.Add(new SelectListItem { Value = "4", Text = "Bill Of Supply" });
            GSt.Add(new SelectListItem { Value = "5", Text = "Export( Under LUT)" });
            GSt.Add(new SelectListItem { Value = "6", Text = "UnRegistered Dealer" });
            GSt.Add(new SelectListItem { Value = "7", Text = "Sez with Payment" });
            GSt.Add(new SelectListItem { Value = "8", Text = "Sez w/0 Payment" });
            GSt.Add(new SelectListItem { Value = "9", Text = "Export (with Duty)" });
            GSt.Add(new SelectListItem { Value = "10", Text = "Exempted" });
            GSt.Add(new SelectListItem { Value = "11", Text = "No GST" });
            GSt.Add(new SelectListItem { Value = "12", Text = "Composition Dealer" });
            GSt.Add(new SelectListItem { Value = "13", Text = "Deemed Export" });
            GSt.Add(new SelectListItem { Value = "14", Text = "Nil Rated" });
            GSt.Add(new SelectListItem { Value = "15", Text = "Export @ 0.1%" });
            GSt.Add(new SelectListItem { Value = "16", Text = "Export @ 0.05%" });
            Name = GSt.Where(x => x.Value == Val).Select(x => x.Text).FirstOrDefault();
            return Name;
        }

        public ActionResult GetSubLedgers()
        {
            List<SelectListItem> subledgerselect = new List<SelectListItem>();
            var result = ctxTFAT.SubLedger.Select(c => new { c.Code, c.Name }).ToList();
            foreach (var item in result)
            {
                subledgerselect.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Code.ToString()
                });
            }

            return Json(subledgerselect, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public ActionResult MoveNextPrevious(string Mode, string mdocument)
        {
            string mtype = mdocument.Substring(0, 5);
            string mvar;
            if (Mode == "N")
            {
                mvar = Fieldoftable("Ledger", "Top 1 Parentkey", "Parentkey>'" + mdocument + "' And Type='" + mtype + "' And Prefix='" + mperiod + "' order by ParentKey", "T") ?? "";
            }
            else
            {
                mvar = Fieldoftable("Ledger", "Top 1 Parentkey", "Parentkey<'" + mdocument + "' And Type='" + mtype + "' And Prefix='" + mperiod + "' order by ParentKey desc", "T") ?? "";
            }
            if (mvar != "")
            {
                mvar = mbranchcode + mvar;
            }

            return Json(new { Status = "Success", data = mvar }, JsonRequestBehavior.AllowGet);
        }

        #region ADD LEDGER ITEM
        public ActionResult AddEditSelectedLedger(LedgerVM Model)
        {
            if (string.IsNullOrEmpty(Model.RefDoc) == false)
            {
                if (Model.RefDoc.Contains(','))
                {
                    var refdocs = Model.RefDoc.Split(',');
                    foreach (var a in refdocs)
                    {
                        string tblkey = ctxTFAT.Ledger.Where(x => x.ParentKey == a).Select(x => x.ParentKey).FirstOrDefault();
                        if (string.IsNullOrEmpty(tblkey))
                        {
                            tblkey = ctxTFAT.Orders.Where(x => x.TableKey == a).Select(x => x.TableKey).FirstOrDefault();
                        }
                        if (string.IsNullOrEmpty(tblkey))
                        {
                            return Json(new
                            {
                                Message = "No such Reference Bill Document Number " + a + " Please Select Again",
                                Status = "Error"
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    var tblkey = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.RefDoc).Select(x => x.ParentKey).FirstOrDefault();
                    if (string.IsNullOrEmpty(tblkey))
                    {
                        tblkey = ctxTFAT.Orders.Where(x => x.TableKey == Model.RefDoc).Select(x => x.TableKey).FirstOrDefault();
                    }
                    if (string.IsNullOrEmpty(tblkey))
                    {
                        return Json(new
                        {
                            Message = "No such Reference Bill Document Number " + Model.RefDoc + " Please Select Again",
                            Status = "Error"
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            if (Model.SessionFlag == "Add")
            {
                List<LedgerVM> objledgerdetail = new List<LedgerVM>();
                List<OutstandingVM> outstandinglist = new List<OutstandingVM>();
                List<CostLedgerVM> costcentrelist = new List<CostLedgerVM>();
                if (Session["ledgerlist"] != null)
                {
                    objledgerdetail = (List<LedgerVM>)Session["ledgerlist"];
                }
                if (Model.OSAdjList != null && Model.OSAdjList.Count > 0)
                {
                    List<string> item3 = new List<string>();
                    if (Model.AmtType == "Debit")
                    {
                        item3 = Model.OSAdjList.Select(x => x.BillNumber).ToList();
                    }
                    else
                    {
                        item3 = Model.OSAdjList.Select(x => x.Srl).ToList();
                    }
                    string osadjnarr = String.Join(",", item3);
                    Model.Narr = Model.Narr + "\n" + "Adjusted Bill No(s): " + osadjnarr;
                }
                objledgerdetail.Add(new LedgerVM()
                {
                    Branch = Model.Branch,
                    Code = Model.Code,
                    AccountName = Model.AccountName,
                    PaymentMode = Model.PaymentMode,
                    Cheque = Model.Cheque,
                    StrChqDate = Model.StrChqDate,
                    MainType = Model.MainType,
                    CurrCode = Model.CurrCode,
                    CurrName = GetCurrName(Model.CurrCode),
                    Debit = (Model.AmtType == "Debit") ? Model.Amt : 0,
                    Credit = (Model.AmtType == "Credit") ? Model.Amt : 0,
                    DraweeCode = Model.DraweeCode,
                    DraweeName = GetDraweeBankName(Model.DraweeCode),
                    RefDoc = Model.RefDoc,
                    RefParty = Model.RefParty,
                    RefPartyName = ctxTFAT.Master.Where(x => x.Code == Model.RefParty).Select(x => x.Name).FirstOrDefault(),
                    GSTType = Model.GSTType,
                    GSTNoItc = Model.GSTNoItc,
                    Taxable = Model.Taxable,
                    IGSTRate = Model.IGSTRate,
                    IGSTAmt = Model.IGSTAmt,
                    CGSTRate = Model.CGSTRate,
                    CGSTAmt = Model.CGSTAmt,
                    SGSTRate = Model.SGSTRate,
                    SGSTAmt = Model.SGSTAmt,
                    TransactionType = Model.TransactionType,
                    StrDueDate = Model.StrDueDate,
                    TDSCess = Model.TDSCess,
                    TDSSurch = Model.TDSSurch,
                    SHECess = Model.SHECess,
                    TDSRate = Model.TDSRate,
                    TDSAmt = Model.TDSAmt,
                    TDSFlag = Model.TDSFlag,
                    TDSCode = Model.TDSCode,
                    Narr = Model.Narr,
                    OSAdjList = Model.OSAdjList,
                    Amt = Model.Amt,
                    AmtType = Model.AmtType,
                    GSTCode = Model.GSTCode,
                    LocationCode = Model.LocationCode,
                    costcentre1 = Model.costcentre1,
                    ClearDate = Convert.ToDateTime("1950-01-01"),
                    SLCode = Model.SLCode,
                    SubLedgerList = Model.SubLedgerList,
                    SrNo = objledgerdetail.Count + 1,
                    tempId = objledgerdetail.Count + 1,
                    tempIsDeleted = false,
                    ProjCode = Model.ProjCode
                });

                if (Model.ConstantMode == 0 || Model.ConstantMode == 1)
                {
                    decimal sumDebit = objledgerdetail.Where(x => x.tempIsDeleted == false && x.tempId != 1).Sum(x => x.Debit);
                    decimal sumCredit = objledgerdetail.Where(x => x.tempIsDeleted == false && x.tempId != 1).Sum(x => x.Credit);
                    int loccode = objledgerdetail.Where(x => x.tempIsDeleted == false && x.tempId != 1).Select(x => x.LocationCode).FirstOrDefault();
                    decimal mAmt = sumDebit - sumCredit;
                    var item = objledgerdetail.Where(x => x.tempId == 1).FirstOrDefault();
                    var item2 = objledgerdetail.Where(x => x.tempId != 1).Select(x => x.Narr).ToList();
                    string anarr = String.Join("\n", item2);
                    {
                        if (mAmt < 0)
                        {
                            item.Debit = mAmt * (-1);
                            item.Credit = 0;
                            item.CurrCode = Model.CurrCode;
                            item.CurrRate = Model.CurrRate;
                            item.LocationCode = loccode;
                            item.Cheque = Model.Cheque;
                            item.StrChqDate = Model.StrChqDate;
                            item.Narr = anarr/* + Model.Narr*/;
                        }
                        else
                        {
                            item.Debit = 0;
                            item.Credit = mAmt;
                            item.CurrCode = Model.CurrCode;
                            item.CurrRate = Model.CurrRate;
                            item.LocationCode = loccode;
                            item.Cheque = Model.Cheque;
                            item.StrChqDate = Model.StrChqDate;
                            item.Narr = anarr/*+ Model.Narr*/;
                        }
                    }
                }

                if ((Model.IGSTAmt + Model.CGSTAmt + Model.SGSTAmt) > 0)//added one for gst bifurfication
                {
                    if (Model.IGSTAmt == 0)
                    {
                        string cgstcode = ctxTFAT.TaxMaster.Where(x => x.Code == Model.GSTCode).Select(x => x.CGSTCode).FirstOrDefault();
                        objledgerdetail.Add(new LedgerVM()
                        {
                            Branch = Model.Branch,
                            Code = cgstcode,
                            AccountName = ctxTFAT.Master.Where(x => x.Code == cgstcode).Select(x => x.Name).FirstOrDefault(),
                            PaymentMode = Model.PaymentMode,
                            Cheque = Model.Cheque,
                            StrChqDate = Model.StrChqDate,
                            MainType = Model.MainType,
                            CurrCode = Model.CurrCode,
                            CurrName = GetCurrName(Model.CurrCode),
                            Debit = (Model.AmtType == "Debit") ? Convert.ToDecimal(Model.CGSTAmt) : 0,
                            Credit = (Model.AmtType == "Credit") ? Convert.ToDecimal(Model.CGSTAmt) : 0,
                            DraweeCode = Model.DraweeCode,
                            DraweeName = GetDraweeBankName(Model.DraweeCode),
                            RefDoc = Model.RefDoc,
                            RefParty = Model.RefParty,
                            RefPartyName = ctxTFAT.Master.Where(x => x.Code == Model.RefParty).Select(x => x.Name).FirstOrDefault(),
                            GSTType = Model.GSTType,
                            GSTNoItc = Model.GSTNoItc,
                            Taxable = Model.Taxable,
                            IGSTRate = Model.IGSTRate,
                            IGSTAmt = Model.IGSTAmt,
                            CGSTRate = Model.CGSTRate,
                            CGSTAmt = Model.CGSTAmt,
                            SGSTRate = Model.SGSTRate,
                            SGSTAmt = Model.SGSTAmt,
                            TransactionType = Model.TransactionType,
                            StrDueDate = Model.StrDueDate,
                            TDSCess = Model.TDSCess,
                            TDSSurch = Model.TDSSurch,
                            SHECess = Model.SHECess,
                            TDSRate = Model.TDSRate,
                            TDSAmt = Model.TDSAmt,
                            TDSFlag = Model.TDSFlag,
                            TDSCode = Model.TDSCode,
                            Narr = Model.Narr,
                            //OSAdjList = Model.OSAdjList,
                            Amt = Convert.ToDecimal(Model.CGSTAmt),
                            AmtType = Model.AmtType,
                            GSTCode = Model.GSTCode,
                            LocationCode = Model.LocationCode,
                            //costcentre1 = costcentrelist,
                            ClearDate = Convert.ToDateTime("1950-01-01"),
                            SLCode = Model.SLCode,
                            ProjCode = Model.ProjCode,
                            //SubLedgerList = Model.SubLedgerList,
                            SrNo = objledgerdetail.Count + 1,
                            tempId = objledgerdetail.Count + 1,
                            tempIsDeleted = false
                        });
                        string sgstcode = ctxTFAT.TaxMaster.Where(x => x.Code == Model.GSTCode).Select(x => x.SGSTCode).FirstOrDefault();
                        objledgerdetail.Add(new LedgerVM()
                        {
                            Branch = Model.Branch,
                            Code = sgstcode,
                            AccountName = ctxTFAT.Master.Where(x => x.Code == sgstcode).Select(x => x.Name).FirstOrDefault(),
                            PaymentMode = Model.PaymentMode,
                            Cheque = Model.Cheque,
                            StrChqDate = Model.StrChqDate,
                            MainType = Model.MainType,
                            CurrCode = Model.CurrCode,
                            CurrName = GetCurrName(Model.CurrCode),
                            Debit = (Model.AmtType == "Debit") ? Convert.ToDecimal(Model.SGSTAmt) : 0,
                            Credit = (Model.AmtType == "Credit") ? Convert.ToDecimal(Model.SGSTAmt) : 0,
                            DraweeCode = Model.DraweeCode,
                            DraweeName = GetDraweeBankName(Model.DraweeCode),
                            RefDoc = Model.RefDoc,
                            RefParty = Model.RefParty,
                            RefPartyName = ctxTFAT.Master.Where(x => x.Code == Model.RefParty).Select(x => x.Name).FirstOrDefault(),
                            GSTType = Model.GSTType,
                            GSTNoItc = Model.GSTNoItc,
                            Taxable = Model.Taxable,
                            IGSTRate = Model.IGSTRate,
                            IGSTAmt = Model.IGSTAmt,
                            CGSTRate = Model.CGSTRate,
                            CGSTAmt = Model.CGSTAmt,
                            SGSTRate = Model.SGSTRate,
                            SGSTAmt = Model.SGSTAmt,
                            TransactionType = Model.TransactionType,
                            StrDueDate = Model.StrDueDate,
                            TDSCess = Model.TDSCess,
                            TDSSurch = Model.TDSSurch,
                            SHECess = Model.SHECess,
                            TDSRate = Model.TDSRate,
                            TDSAmt = Model.TDSAmt,
                            TDSFlag = Model.TDSFlag,
                            TDSCode = Model.TDSCode,
                            Narr = Model.Narr,
                            //OSAdjList = Model.OSAdjList,
                            Amt = Convert.ToDecimal(Model.SGSTAmt),
                            AmtType = Model.AmtType,
                            GSTCode = Model.GSTCode,
                            LocationCode = Model.LocationCode,
                            //costcentre1 = costcentrelist,
                            ClearDate = Convert.ToDateTime("1950-01-01"),
                            SLCode = Model.SLCode,
                            //SubLedgerList = Model.SubLedgerList,
                            ProjCode = Model.ProjCode,
                            SrNo = objledgerdetail.Count + 1,
                            tempId = objledgerdetail.Count + 1,
                            tempIsDeleted = false
                        });
                    }
                    else
                    {
                        string igstcode = ctxTFAT.TaxMaster.Where(x => x.Code == Model.GSTCode).Select(x => x.IGSTCode).FirstOrDefault();
                        objledgerdetail.Add(new LedgerVM()
                        {
                            Branch = Model.Branch,
                            Code = igstcode,
                            AccountName = ctxTFAT.Master.Where(x => x.Code == igstcode).Select(x => x.Name).FirstOrDefault(),
                            PaymentMode = Model.PaymentMode,
                            Cheque = Model.Cheque,
                            StrChqDate = Model.StrChqDate,
                            MainType = Model.MainType,
                            CurrCode = Model.CurrCode,
                            CurrName = GetCurrName(Model.CurrCode),
                            Debit = (Model.AmtType == "Debit") ? Convert.ToDecimal(Model.IGSTAmt) : 0,
                            Credit = (Model.AmtType == "Credit") ? Convert.ToDecimal(Model.IGSTAmt) : 0,
                            DraweeCode = Model.DraweeCode,
                            DraweeName = GetDraweeBankName(Model.DraweeCode),
                            RefDoc = Model.RefDoc,
                            RefParty = Model.RefParty,
                            RefPartyName = ctxTFAT.Master.Where(x => x.Code == Model.RefParty).Select(x => x.Name).FirstOrDefault(),
                            GSTType = Model.GSTType,
                            GSTNoItc = Model.GSTNoItc,
                            Taxable = Model.Taxable,
                            IGSTRate = Model.IGSTRate,
                            IGSTAmt = Model.IGSTAmt,
                            CGSTRate = Model.CGSTRate,
                            CGSTAmt = Model.CGSTAmt,
                            SGSTRate = Model.SGSTRate,
                            SGSTAmt = Model.SGSTAmt,
                            TransactionType = Model.TransactionType,
                            StrDueDate = Model.StrDueDate,
                            TDSCess = Model.TDSCess,
                            TDSSurch = Model.TDSSurch,
                            SHECess = Model.SHECess,
                            TDSRate = Model.TDSRate,
                            TDSAmt = Model.TDSAmt,
                            TDSFlag = Model.TDSFlag,
                            TDSCode = Model.TDSCode,
                            Narr = Model.Narr,
                            //OSAdjList = Model.OSAdjList,
                            Amt = Convert.ToDecimal(Model.IGSTAmt),
                            AmtType = Model.AmtType,
                            GSTCode = Model.GSTCode,
                            LocationCode = Model.LocationCode,
                            //costcentre1 = costcentrelist,
                            ClearDate = Convert.ToDateTime("1950-01-01"),
                            SLCode = Model.SLCode,
                            //SubLedgerList = Model.SubLedgerList,
                            ProjCode = Model.ProjCode,
                            SrNo = objledgerdetail.Count + 1,
                            tempId = objledgerdetail.Count + 1,
                            tempIsDeleted = false
                        });
                    }
                }

                Session.Add("ledgerlist", objledgerdetail);
                decimal sumdebit = objledgerdetail.Where(x => x.tempIsDeleted == false).Sum(x => x.Debit);
                decimal sumcredit = objledgerdetail.Where(x => x.tempIsDeleted == false).Sum(x => x.Credit);
                var html = ViewHelper.RenderPartialView(this, "LedgerList", new LedgerVM() { Selectedleger = objledgerdetail });
                var jsonResult = Json(new { Selectedleger = objledgerdetail, Html = html, Sumdebit = sumdebit, Sumcredit = sumcredit }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            else
            {
                if (Model.OSAdjList != null && Model.OSAdjList.Count > 0)
                {
                    List<string> item3 = new List<string>();
                    if (Model.AmtType == "Debit")
                    {
                        item3 = Model.OSAdjList.Select(x => x.BillNumber).ToList();
                    }
                    else
                    {
                        item3 = Model.OSAdjList.Select(x => x.Srl).ToList();
                    }
                    string osadjnarr = String.Join(",", item3);
                    if (Model.Narr == null || Model.Narr == "")
                    {
                        Model.Narr = Model.Narr + "\n" + "Adjusted Bill No(s): " + osadjnarr;
                    }
                    else
                    {
                        Model.Narr = (Model.Narr.Contains("Adjusted Bill No(s): ") == true) ? Model.Narr.Remove(Model.Narr.IndexOf("Adjusted Bill No(s): "), (Model.Narr.Length - Model.Narr.IndexOf("Adjusted Bill No(s): "))) + "\n" + "Adjusted Bill No(s): " + osadjnarr : Model.Narr + "\n" + "Adjusted Bill No(s): " + osadjnarr;
                    }
                }

                var result = (List<LedgerVM>)Session["ledgerlist"];
                foreach (var item in result.Where(x => x.tempId == Model.tempId))
                {
                    item.Branch = Model.Branch;
                    item.Code = Model.Code;
                    item.AccountName = Model.AccountName;
                    item.PaymentMode = Model.PaymentMode;
                    item.Cheque = Model.Cheque;
                    item.StrChqDate = Model.StrChqDate;
                    item.MainType = Model.MainType;
                    item.CurrCode = Model.CurrCode;
                    item.CurrName = GetCurrName(Model.CurrCode);
                    item.Debit = (Model.AmtType == "Debit") ? Model.Amt : 0;
                    item.Credit = (Model.AmtType == "Credit") ? Model.Amt : 0;
                    item.DraweeCode = Model.DraweeCode;
                    item.DraweeName = GetDraweeBankName(Model.DraweeCode);
                    item.RefDoc = Model.RefDoc;
                    item.RefParty = Model.RefParty;
                    item.RefPartyName = ctxTFAT.CustomerMaster.Where(x => x.Code == Model.RefParty).Select(x => x.Name).FirstOrDefault();
                    item.GSTType = Model.GSTType;
                    item.GSTNoItc = Model.GSTNoItc;
                    item.Taxable = Model.Taxable;
                    item.IGSTRate = Model.IGSTRate;
                    item.IGSTAmt = Model.IGSTAmt;
                    item.CGSTRate = Model.CGSTRate;
                    item.CGSTAmt = Model.CGSTAmt;
                    item.SGSTRate = Model.SGSTRate;
                    item.SGSTAmt = Model.SGSTAmt;
                    item.TransactionType = Model.TransactionType;
                    item.StrDueDate = Model.StrDueDate;
                    item.TDSCess = Model.TDSCess;
                    item.TDSSurch = Model.TDSSurch;
                    item.SHECess = Model.SHECess;
                    item.TDSRate = Model.TDSRate;
                    item.TDSAmt = Model.TDSAmt;
                    item.TDSFlag = Model.TDSFlag;
                    item.TDSCode = Model.TDSCode;
                    item.Narr = Model.Narr;
                    item.OSAdjList = Model.OSAdjList;
                    item.Amt = Model.Amt;
                    item.AmtType = Model.AmtType;
                    item.SLCode = Model.SLCode;
                    item.GSTCode = Model.GSTCode;
                    item.LocationCode = Model.LocationCode;
                    item.costcentre1 = Model.costcentre1;
                    item.SubLedgerList = Model.SubLedgerList;
                    item.tempIsDeleted = false;
                    item.ProjCode = Model.ProjCode;
                }

                if (Model.ConstantMode == 0 || Model.ConstantMode == 1)
                {
                    decimal sumDebit = result.Where(x => x.tempIsDeleted == false && x.tempId != 1).Sum(x => x.Debit);
                    decimal sumCredit = result.Where(x => x.tempIsDeleted == false && x.tempId != 1).Sum(x => x.Credit);
                    int loccode = result.Where(x => x.tempIsDeleted == false && x.tempId != 1).Select(x => x.LocationCode).FirstOrDefault();
                    decimal mAmt = sumDebit - sumCredit;
                    var item2 = result.Where(x => x.tempId != 1).Select(x => x.Narr).ToList();
                    string anarr = String.Join(" \n ", item2);
                    var item = result.Where(x => x.tempId == 1).FirstOrDefault();
                    {
                        if (mAmt < 0)
                        {
                            item.Debit = mAmt * (-1);
                            item.Credit = 0;
                            item.CurrCode = Model.CurrCode;
                            item.CurrRate = Model.CurrRate;
                            item.LocationCode = loccode;
                            item.Cheque = Model.Cheque;
                            item.StrChqDate = Model.StrChqDate;
                            item.Narr = anarr /*+ Model.Narr*/;
                        }
                        else
                        {
                            item.Debit = 0;
                            item.Credit = mAmt;
                            item.CurrCode = Model.CurrCode;
                            item.CurrRate = Model.CurrRate;
                            item.LocationCode = loccode;
                            item.Cheque = Model.Cheque;
                            item.StrChqDate = Model.StrChqDate;
                            item.Narr = anarr/* + Model.Narr*/;
                        }
                    }
                }
                Session.Add("ledgerlist", result);
                decimal sumdebit = result.Where(x => x.tempIsDeleted != true).Sum(x => x.Debit);
                decimal sumcredit = result.Where(x => x.tempIsDeleted != true).Sum(x => x.Credit);
                var html = ViewHelper.RenderPartialView(this, "LedgerList", new LedgerVM() { Selectedleger = result });
                var jsonResult = Json(new
                {
                    Selectedleger = result,
                    Html = html,
                    Sumdebit = sumdebit,
                    Sumcredit = sumcredit
                }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        [HttpPost]
        public ActionResult Deleteledger(LedgerVM Model)
        {
            var result = (List<LedgerVM>)Session["ledgerlist"];
            if (Model.ConstantMode == 1 || Model.ConstantMode == 0)
            {
                if (Model.tempId == 1)
                {
                }
                else
                {
                    result.Where(x => x.tempId == Model.tempId).FirstOrDefault().tempIsDeleted = true;
                }
            }
            else
            {
                result.Where(x => x.tempId == Model.tempId).FirstOrDefault().tempIsDeleted = true;
            }
            Session.Add("ledgerlist", result);
            decimal sumdebit = result.Where(x => x.tempIsDeleted != true).Sum(x => x.Debit);
            decimal sumcredit = result.Where(x => x.tempIsDeleted != true).Sum(x => x.Credit);
            var html = ViewHelper.RenderPartialView(this, "LedgerList", new LedgerVM() { Selectedleger = result });
            return Json(new
            {
                Selectedleger = result,
                Html = html,
                Sumdebit = sumdebit,
                Sumcredit = sumcredit
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCashLedger(LedgerVM Model)
        {
            if (Model.SessionFlag == "Edit")
            {
                var result = (List<LedgerVM>)Session["ledgerlist"];
                var result1 = result.Where(x => x.tempId == Model.tempId);
                foreach (var item in result1)
                {
                    item.ChangeLog = Model.ChangeLog;
                    Model.Branch = item.Branch;
                    Model.Code = item.Code;
                    Model.AccountName = item.AccountName;
                    Model.PaymentMode = item.PaymentMode;
                    Model.PaymentModeName = GetPaymentModeName(item.PaymentMode);
                    Model.Cheque = item.Cheque;
                    Model.StrChqDate = item.StrChqDate;
                    Model.MainType = item.MainType;
                    Model.CurrCode = item.CurrCode;
                    Model.CurrName = GetCurrName(item.CurrCode);
                    Model.CurrRate = item.CurrRate;
                    Model.Debit = item.Debit;
                    Model.Credit = item.Credit;
                    Model.DraweeCode = item.DraweeCode;
                    Model.DraweeName = GetDraweeBankName(item.DraweeCode);
                    Model.RefDoc = item.RefDoc;
                    Model.RefParty = item.RefParty;
                    Model.RefPartyName = ctxTFAT.CustomerMaster.Where(x => x.Code == item.RefParty).Select(x => x.Name).FirstOrDefault();
                    Model.GSTType = item.GSTType;
                    Model.GSTTypeName = GetGSTTypeName(item.GSTType);
                    Model.GSTCodeName = GetGSTName(item.GSTCode);
                    Model.GSTNoItc = item.GSTNoItc;
                    Model.Taxable = item.Taxable;
                    Model.IGSTRate = item.IGSTRate;
                    Model.IGSTAmt = item.IGSTAmt;
                    Model.CGSTRate = item.CGSTRate;
                    Model.CGSTAmt = item.CGSTAmt;
                    Model.SGSTRate = item.SGSTRate;
                    Model.SGSTAmt = item.SGSTAmt;
                    Model.TransactionType = item.TransactionType;
                    Model.TransactionTypeName = GetLoanTrxTypesName(item.TransactionType);
                    Model.StrDueDate = item.StrDueDate;
                    Model.TDSCess = item.TDSCess;
                    Model.TDSSurch = item.TDSSurch;
                    Model.SHECess = item.SHECess;
                    Model.TDSRate = item.TDSRate;
                    Model.TDSAmt = item.TDSAmt;
                    Model.TDSFlag = item.TDSFlag;
                    Model.TDSCode = item.TDSCode;
                    Model.TDSCodeName = GetTDSName(item.TDSCode);
                    Model.Narr = item.Narr;
                    Model.Amt = item.Debit + item.Credit;
                    Model.LocationCode = item.LocationCode;
                    Model.GSTCode = item.GSTCode;
                    Model.AmtType = (item.Debit > 0) ? "Debit" : "Credit";
                    Model.OSAdjList = ShowAdjustListInSession(item);
                    Model.costcentre1 = ShowCostCentreListInSession(item);
                    Model.PartyAdvances = GetPartyAdvances(Model.Code, Model.SubType);
                    Model.PartyNetOutstanding = GetBalance(Model.Code, DateTime.Now, mbranchcode, 0);
                    Model.PartyConsOutstanding = GetConsOutstanding(Model.Code);
                    Model.SLCode = item.SLCode;
                    Model.CCReqd = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.CCReqd).FirstOrDefault();
                    Model.SubLedgerList = ShowSubLedgerListInSession(item);
                    Model.ProjCode = item.ProjCode;
                }
                var jsonResult = Json(new { Html = this.RenderPartialView("AddCashBankList", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            else
            {
                List<LedgerVM> SubList = new List<LedgerVM>();
                List<LedgerVM> newlist1 = new List<LedgerVM>();
                var CostCentre = (from C in ctxTFAT.CostCentre
                                  where C.Grp != C.Code && C.Locked == false && C.AppBranch == mbranchcode
                                  orderby C.Name
                                  select new { C.Code, C.Name }).ToList();
                int a = 1;
                foreach (var item in CostCentre)
                {
                    newlist1.Add(new LedgerVM
                    {
                        tempId = a,
                        CostCode = item.Code,
                        Name = item.Name,
                        CCAmt = 0
                    });
                    ++a;
                }
                var slledgers = ctxTFAT.SubLedger.Select(x => x).ToList();
                foreach (var r in slledgers)
                {
                    SubList.Add(new LedgerVM()
                    {
                        CCAmt = 0,
                        SLCode = r.Code,
                        Name = r.Name,
                        tempId = a
                    });
                    ++a;
                }
                Model.SubLedgerList = SubList;
                Model.costcentre1 = newlist1.ToList();
                Model.AmtType = Model.MainType == "RC" ? "Credit" : "Debit";
                Model.Branch = mbranchcode;
                var CurrCode = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.CurrName).FirstOrDefault();
                Model.CurrCode = CurrCode.ToString();
                Model.CurrName = GetCurrName(Model.CurrCode);
                Model.CurrRate = ctxTFAT.CurrencyMaster.Where(x => x.Code == CurrCode).Select(x => x.CurrRate).FirstOrDefault();
                if (Model.CurrRate == 0)
                {
                    Model.CurrRate = 1;
                }
                if (Model.SubType == "CP" || Model.SubType == "HP" || Model.SubType == "CR")
                {
                    Model.PaymentMode = "0";
                    Model.PaymentModeName = "Cash";
                }
                if (Model.SubType == "BP" || Model.SubType == "BR")
                {
                    Model.PaymentMode = "1";
                    Model.PaymentModeName = "Cheque";
                }
                if (Model.MainType == "JV" || Model.MainType == "JM")
                {
                    Model.PaymentMode = "1";
                    Model.PaymentModeName = "Cheque";
                    Model.Amt = Model.SumDebit - Model.SumCredit;
                    if (Model.Amt > 0)
                    {
                        Model.AmtType = "Credit";
                    }
                    else
                    {
                        Model.Amt = Math.Abs(Model.Amt);
                        Model.AmtType = "Debit";
                    }
                }
                Model.LocationCode = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => x.LocationCode).FirstOrDefault();
                //return Json(new { Html = this.RenderPartialView("AddCashBankList", Model) }, JsonRequestBehavior.AllowGet);
                var jsonResult = Json(new { Html = this.RenderPartialView("AddCashBankList", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        [HttpPost]
        public ActionResult ChangeFirstEntry(LedgerVM Model)
        {
            var result = (List<LedgerVM>)Session["ledgerlist"];
            if (Model.ConstantMode == 1 || Model.ConstantMode == 0)
            {
                foreach (var item in result.Where(x => x.tempId == 1))
                {
                    item.Branch = Model.Branch;
                    item.Code = Model.Code;
                    item.AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Code).Select(x => x.Name).FirstOrDefault();
                    item.tempIsDeleted = false;
                }
            }

            Session.Add("ledgerlist", result);
            decimal sumdebit = result.Where(x => x.tempIsDeleted != true).Sum(x => x.Debit);
            decimal sumcredit = result.Where(x => x.tempIsDeleted != true).Sum(x => x.Credit);
            DateTime mDate = DateTime.Now.Date;
            decimal balance = GetBalance(Model.Code, mDate, Model.Branch);
            var html = ViewHelper.RenderPartialView(this, "LedgerList", new LedgerVM() { Selectedleger = result });
            return Json(new
            {
                Selectedleger = result,
                Html = html,
                Sumdebit = sumdebit,
                Sumcredit = sumcredit,
                Balance = balance
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region SAVE EDIT


        public string CheckValidations(LedgerVM Model)
        {
            string connstring = GetConnectionString();
            DateTime mStartDate = Convert.ToDateTime(System.Web.HttpContext.Current.Session["StartDate"]);
            DateTime mLastDate = Convert.ToDateTime(System.Web.HttpContext.Current.Session["LastDate"]);
            // Model.SubType = Model.SubType;
            string mMessage = "";
            Model.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            var resultdoctype = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => new { x.BackDays }).FirstOrDefault();

            bool mBackDated = ctxTFAT.UserRightsTrx.Where(x => x.Code == muserid && x.Type == Model.Type).Select(z => z.xBackDated).FirstOrDefault();
            decimal mBackDays = resultdoctype.BackDays == null ? 0 : resultdoctype.BackDays.Value;

            string mMsg = CheckEntryDate(ConvertDDMMYYTOYYMMDD(Model.DocuDate), (/*Model.ChangeLog == "Add" ?*/ mBackDated /*: true*/), mStartDate, mLastDate, mBackDays, DateTime.Now, gpHolidayWarn, gpHoliday1, gpHoliday2);
            if (mMsg != "")
            {
                mMessage = mMessage + "\n" + mMsg;
            }

            return mMessage;
        }
        public ActionResult DeleteData(LedgerVM Model)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var result = (List<LedgerVM>)Session["ledgerlist"];
                    var list = result.Where(x => x.tempIsDeleted != true).ToList();
                    var Amount = list.Sum(x => x.Debit);
                    DeUpdate(Model);
                    UpdateAuditTrail(mbranchcode, Model.ChangeLog, Model.Header, Model.ParentKey, DateTime.Now, Amount, Model.ParentKey, "Delete", "NA");
                    //SendTrnsMsg("Delete", 0, mbranchcode + Model.ParentKey, ConvertDDMMYYTOYYMMDD(Model.DocuDate), "");

                    transaction.Commit();
                    transaction.Dispose();
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    return Json(new
                    {
                        Status = "Error",
                        Message = ex1.InnerException.InnerException.Message
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    return Json(new
                    {
                        Status = "Error",
                        Message = ex.InnerException.InnerException.Message
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new
            {
                Status = "Success",
                Message = "The Voucher is Deleted."
            }, JsonRequestBehavior.AllowGet);
        }

        private void DeUpdate(LedgerVM Model)
        {
            if (ctxTFAT.TfatBranch.Where(z => z.Code == Model.Branch).Select(x => x.gp_AllowEditDelete).FirstOrDefault() == false)
            {
                var mDeleteAuth = ctxTFAT.Authorisation.Where(x => x.ParentKey == Model.ParentKey).ToList();
                ctxTFAT.Authorisation.RemoveRange(mDeleteAuth);

                //var mDeleteLedLB = ctxTFAT.LedgerBranch.Where(x => x.ParentKey == Model.ParentKey).ToList();
                //ctxTFAT.LedgerBranch.RemoveRange(mDeleteLedLB);
                var mDeleteOS = ctxTFAT.Outstanding.Where(x => x.ParentKey == Model.ParentKey && x.DocBranch== Model.Branch).ToList();
                ctxTFAT.Outstanding.RemoveRange(mDeleteOS);
                var mDeleteLed = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).ToList();
                //foreach (var mled in mDeleteLed)
                //{
                //    var mDeleteOS2 = ctxTFAT.Outstanding.Where(x => x.TableRefKey == mled.TableKey).ToList();
                //    ctxTFAT.Outstanding.RemoveRange(mDeleteOS2);
                //}
                //var mDeleteLed = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey).ToList();
                ctxTFAT.Ledger.RemoveRange(mDeleteLed);

                var mDeleteNote = ctxTFAT.Narration.Where(x => x.TableKey == Model.ParentKey).ToList();
                ctxTFAT.Narration.RemoveRange(mDeleteNote);
                var mDeleteTdspay = ctxTFAT.TDSPayments.Where(x => x.ParentKey == Model.ParentKey).ToList();
                ctxTFAT.TDSPayments.RemoveRange(mDeleteTdspay);
                var mDeleteCC = ctxTFAT.CostLedger.Where(x => x.ParentKey == Model.ParentKey).ToList();
                ctxTFAT.CostLedger.RemoveRange(mDeleteCC);
                var mDeleteSb = ctxTFAT.LedgerSL.Where(x => x.ParentKey == Model.ParentKey).ToList();
                ctxTFAT.LedgerSL.RemoveRange(mDeleteSb);
                var mDeleteAttach = ctxTFAT.Attachment.Where(x => x.ParentKey == Model.ParentKey).ToList();
                if (mDeleteAttach != null)
                {
                    ctxTFAT.Attachment.RemoveRange(mDeleteAttach);
                }

                var mDeleteaddon = ctxTFAT.AddonDocCB.Where(x => x.ParentKey == Model.ParentKey).ToList();
                if (mDeleteaddon != null)
                {
                    ctxTFAT.AddonDocCB.RemoveRange(mDeleteaddon);
                }
                ctxTFAT.SaveChanges();



                //using (SqlConnection con = new SqlConnection(connstring))
                //{
                //    using (SqlCommand cmd = new SqlCommand("DELETE FROM addondocCB  WHERE ParentKey=@ParentKey"))
                //    {
                //        cmd.Parameters.AddWithValue("@ParentKey", Model.ParentKey);
                //        cmd.Connection = con;
                //        con.Open();
                //        cmd.ExecuteNonQuery();
                //        con.Close();
                //    }
                //}
            }
            else
            {
                var led = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey).Select(x => new { x.Prefix, x.DocDate }).FirstOrDefault();

                SqlConnection conx = new SqlConnection(GetConnectionString());
                SqlCommand cmdx = new SqlCommand("dbo.SPTFAT_ReverseCashBank", conx);
                cmdx.CommandType = CommandType.StoredProcedure;
                cmdx.Parameters.Add("@mparentkey", SqlDbType.VarChar).Value = Model.ParentKey;
                cmdx.Parameters.Add("@mbranch", SqlDbType.VarChar).Value = Model.Branch;
                cmdx.Parameters.Add("@mprefix", SqlDbType.VarChar).Value = led.Prefix;
                cmdx.Parameters.Add("@mdocdate", SqlDbType.Date).Value = led.DocDate;
                conx.Open();
                cmdx.ExecuteNonQuery();
                conx.Close();
            }
        }

        public ActionResult SaveData(LedgerVM Model)
        {
            string mStr = "";
            var result = (List<LedgerVM>)Session["ledgerlist"];
            if (Model.ChangeLog == "Add")
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
            Model.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            var mtfatperd = ctxTFAT.TfatPerd.Where(x => x.PerdCode == mperiod).Select(x => new { x.StartDate, x.LastDate }).FirstOrDefault();
            if (mtfatperd.StartDate > Model.DocDate || Model.DocDate > mtfatperd.LastDate)
            {
                mStr = mStr + "\nSelected Document Date is not in Current Accounting Period..";
            }
            if (mStr != "")
            {
                return Json(new
                {
                    Message = mStr,
                    Status = "CancelError"
                }, JsonRequestBehavior.AllowGet);
            }
            var CheckTypeBranch = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => x.AppBranch).FirstOrDefault();
            if (mbranchcode != CheckTypeBranch)
            {
                mStr = mStr + "\nDocument Type is Not of Current Branch Cant Save..";
            }
            mStr = CheckValidations(Model);

            if (mStr != "")
            {
                return Json(new
                {
                    Message = mStr,
                    Status = "CancelError"
                }, JsonRequestBehavior.AllowGet);
            }
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    if (Model.Authorise.Substring(0, 1) == "R" || Model.ChangeLog == "Add" || (Model.ChangeLog == "Edit" && Model.AuthAgain))
                    {
                        if (Model.Authorise.Substring(0, 1) != "N") //if authorised then check for the RateDiff Auth. Rule
                        {
                            Model.Authorise = (Model.AuthReq == true ? GetAuthorise(Model.Type, 0, mbranchcode) : Model.Authorise = "A00");
                        }
                    }

                    if (Model.ChangeLog == "Edit")
                    {
                        if (mbranchcode != Model.Branch)
                        {
                            transaction.Rollback();
                            return Json(new { Message = "Login Branch Does Not Match With Documnet Branch.", Status = "CancelError", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                        }
                        DeUpdate(Model);
                    }

                    var list = result.Where(x => x.tempIsDeleted != true).ToList();
                    var altcode = list.Select(x => x.Code).First();
                    if (Model.ChangeLog == "Add")
                    {
                        Model.Srl = GetLastSerial("Ledger", mbranchcode, Model.Type, Model.Prefix, Model.SubType, Model.DocDate);
                        Model.ParentKey = Model.Type + Model.Prefix.Substring(0, 2) + Model.Srl;
                    }

                    Model.Prefix = Model.Prefix;
                    int mCnt = 1;
                    foreach (var item in list)
                    {
                        Ledger mobj1 = new Ledger();
                        mobj1.TableKey = Model.Type + Model.Prefix.Substring(0, 2) + mCnt.ToString("D3") + Model.Srl;
                        item.TableKey = mobj1.TableKey;
                        mobj1.ParentKey = Model.ParentKey;
                        mobj1.Sno = mCnt;
                        mobj1.Srl = Model.Srl;
                        mobj1.SubType = Model.SubType;
                        mobj1.Type = Model.Type;
                        if (Model.MainType != "JV") //Check here
                        {
                            if (mCnt == 1)
                            {
                                int maxsno = list.Select(x => x.tempId).Max();
                                mobj1.AltCode = list.Where(x => x.tempId == maxsno).Select(x => x.Code).FirstOrDefault();
                            }
                            else
                            {
                                mobj1.AltCode = list.Where(x => x.tempId == 1).Select(x => x.Code).FirstOrDefault();
                            }
                        }
                        else
                        {
                            mobj1.AltCode = item.Code;
                        }
                        mobj1.Audited = false;
                        mobj1.AUTHIDS = muserid;
                        mobj1.AUTHORISE = Model.Authorise;
                        mobj1.BillDate = DateTime.Now;
                        mobj1.BillNumber = "";
                        mobj1.Branch = mbranchcode;
                        mobj1.CompCode = mcompcode;
                        mobj1.Cheque = item.Cheque != null ? item.Cheque : "";
                        mobj1.ChequeReturn = false;
                        mobj1.ClearDate = item.ClearDate;
                        mobj1.ChequeDate = (item.StrChqDate == null || item.StrChqDate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(item.StrChqDate);
                        mobj1.Code = item.Code;
                        mobj1.CrPeriod = 0;
                        mobj1.CurrName = (item.CurrCode == null || item.CurrCode == "") ? 0 : Convert.ToInt32(item.CurrCode);
                        mobj1.CurrRate = (item.CurrRate == null) ? 0 : item.CurrRate;
                        mobj1.Debit = item.Debit;
                        mobj1.Credit = item.Credit;
                        mobj1.Discounted = true;
                        mobj1.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                        mobj1.MainType = Model.MainType;
                        mobj1.Narr = item.Narr != null ? item.Narr : "";
                        mobj1.Party = item.RefParty;
                        mobj1.Prefix = Model.Prefix;
                        mobj1.RecoFlag = item.TransactionType ?? "";
                        mobj1.RefDoc = item.RefDoc;
                        mobj1.TDSChallanNumber = "";
                        mobj1.TDSCode = item.TDSCode;
                        mobj1.TDSFlag = item.TDSFlag;
                        mobj1.BankCode = (item.DraweeCode == null || item.DraweeCode == "") ? 0 : Convert.ToInt32(item.DraweeCode);
                        mobj1.ENTEREDBY = muserid;
                        mobj1.DueDate = (item.StrDueDate == null || item.StrDueDate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(item.StrDueDate);
                        mobj1.Reminder = false;
                        mobj1.TaskID = 0;
                        mobj1.ChqCategory = (item.PaymentMode == null) ? 0 : Convert.ToInt32(item.PaymentMode);
                        mobj1.CurrAmount = 0;
                        mobj1.LocationCode = item.LocationCode;
                        mobj1.LASTUPDATEDATE = DateTime.Now;
                        mobj1.GSTType = (item.GSTType == null || item.GSTType.Trim() == "") ? 0 : Convert.ToInt32(item.GSTType);
                        mobj1.TaxCode = item.GSTCode;
                        mobj1.IGSTAmt = Convert.ToDecimal(item.IGSTAmt);
                        mobj1.IGSTRate = Convert.ToDecimal(item.IGSTRate);
                        mobj1.CGSTAmt = Convert.ToDecimal(item.CGSTAmt);
                        mobj1.CGSTRate = Convert.ToDecimal(item.CGSTRate);
                        mobj1.SGSTAmt = Convert.ToDecimal(item.SGSTAmt);
                        mobj1.SGSTRate = Convert.ToDecimal(item.SGSTRate);
                        mobj1.SLCode = 0/*item.SLCode*/;
                        ctxTFAT.Ledger.Add(mobj1);

                        if ((string.IsNullOrEmpty(item.GSTCode) == false) && ((item.SGSTAmt + item.CGSTAmt + item.IGSTAmt) > 0))
                        {
                            StockTax mobjtax = new StockTax();
                            mobjtax.TableKey = Model.Type + Model.Prefix.Substring(0, 2) + mCnt.ToString("D3") + Model.Srl;
                            mobjtax.Code = "";
                            mobjtax.ParentKey = Model.ParentKey;
                            mobjtax.AUTHIDS = muserid;
                            mobjtax.AUTHORISE = Model.Authorise;
                            mobjtax.Branch = mbranchcode;
                            mobjtax.Cess = 0;
                            mobjtax.Taxable = item.Debit + item.Credit;
                            mobjtax.TaxAmt = 0;
                            mobjtax.TaxCode = item.GSTCode;
                            mobjtax.ENTEREDBY = muserid;
                            mobjtax.LASTUPDATEDATE = DateTime.Now;
                            mobjtax.CGSTAmt = Convert.ToDecimal(item.CGSTAmt);
                            mobjtax.CGSTRate = Convert.ToDecimal(item.CGSTRate);
                            mobjtax.GSTNoITC = 0;
                            mobjtax.GSTType = (Model.GSTType == null) ? 0 : Convert.ToInt32(Model.GSTType);
                            mobjtax.HSNCode = "";
                            mobjtax.IGSTAmt = Convert.ToDecimal(item.IGSTAmt);
                            mobjtax.IGSTRate = Convert.ToDecimal(item.IGSTRate);
                            mobjtax.SGSTAmt = Convert.ToDecimal(item.SGSTAmt);
                            mobjtax.SGSTRate = Convert.ToDecimal(item.SGSTRate);
                            mobjtax.CVDAmt = 0;
                            mobjtax.CVDCessAmt = 0;
                            mobjtax.CVDExtra = 0;
                            mobjtax.CVDSCessAmt = 0;
                            mobjtax.GSTNo = "";
                            mobjtax.Party = item.RefParty;
                            mobjtax.AltAddress = 0;
                            mobjtax.SubType = Model.SubType;
                            mobjtax.DealerType = 0;
                            mobjtax.PlaceOfSupply = "";
                            mobjtax.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                            mobjtax.ItemType = "I";
                            ctxTFAT.StockTax.Add(mobjtax);
                        }
                        if (item.TDSCode != 0)
                        {
                            TDSPayments tds = new TDSPayments();
                            tds.Amount = item.Credit + item.Debit;
                            tds.aMainType = Model.MainType;
                            tds.aPrefix = Model.Prefix;
                            tds.aSno = mCnt;
                            tds.aSrl = Model.Srl;
                            tds.SubType = Model.SubType;
                            tds.TableKey = Model.Type + Model.Prefix.Substring(0, 2) + mCnt.ToString("D3") + Model.Srl;
                            tds.TDSAble = 0;
                            tds.TDSAmt = Convert.ToDecimal(item.TDSAmt);
                            tds.TDSCess = Convert.ToDecimal(item.TDSCess);
                            tds.TDSCessAmt = 0;
                            tds.TDSCode = item.TDSCode;
                            tds.TDSReason = 0;
                            tds.TDSSheCess = Convert.ToDecimal(item.SHECess);
                            tds.TDSSheCessAmt = 0;
                            tds.TDSSurCharge = Convert.ToDecimal(item.TDSSurch);
                            tds.TDSSurChargeAmt = 0;
                            tds.TDSTax = 0;
                            tds.TDSTaxAmt = 0;
                            tds.TotalTDSAmt = 0;
                            tds.TotalTDSAmt = 0;
                            tds.Type = Model.Type;
                            tds.aPrefix = Model.Prefix;
                            tds.aSubType = Model.SubType;
                            tds.aType = Model.Type;
                            tds.AUTHIDS = muserid;
                            tds.AUTHORISE = Model.Authorise;
                            tds.BankCode = "";
                            tds.BillNumber = "";
                            tds.Branch = mbranchcode;
                            tds.CertDate = DateTime.Now;
                            tds.CertNumber = "";
                            tds.ChallanDate = DateTime.Now;
                            tds.ChallanNumber = "";
                            tds.CNO = "";
                            tds.Code = item.Code;
                            tds.CompCode = mcompcode;
                            tds.DepositSerial = "";
                            tds.DocDate = DateTime.Now;
                            tds.DueDate = DateTime.Now;
                            tds.EndCredit = false;
                            tds.ENTEREDBY = muserid;
                            tds.LASTUPDATEDATE = DateTime.Now;
                            tds.LocationCode = item.LocationCode;
                            tds.MainType = Model.MainType;
                            tds.Narr = "";
                            tds.PaidAmt = 0;
                            tds.ParentKey = Model.ParentKey;
                            tds.Party = item.RefParty;
                            tds.PaymentMode = 0;
                            tds.Prefix = Model.Prefix;
                            tds.Sno = mCnt;
                            tds.Srl = Model.Srl;
                            ctxTFAT.TDSPayments.Add(tds);
                        }
                        if (item.OSAdjList != null)
                        {
                            Model.count = 1;
                            foreach (var item1 in item.OSAdjList)
                            {
                                if (item1.AdjustAmt > 0)
                                {
                                    Outstanding osobj1 = new Outstanding();
                                    osobj1.Branch = mbranchcode;
                                    osobj1.DocBranch = mbranchcode;
                                    osobj1.MainType = Model.MainType;
                                    osobj1.SubType = Model.SubType;
                                    osobj1.Type = Model.Type;
                                    osobj1.Prefix = Model.Prefix;
                                    osobj1.Srl = Model.Srl;
                                    osobj1.Sno = mCnt;
                                    osobj1.ParentKey = Model.ParentKey;
                                    osobj1.TableKey = Model.Type + Model.Prefix.Substring(0, 2) + mCnt.ToString("D3") + Model.Srl;
                                    osobj1.aMaintype = item1.MainType;
                                    osobj1.aSubType = item1.SubType;
                                    osobj1.aType = item1.Type;
                                    osobj1.aPrefix = item1.Prefix;
                                    osobj1.aSrl = item1.Srl;
                                    osobj1.aSno = item1.Sno;
                                    osobj1.Amount = Convert.ToDecimal(item1.AdjustAmt);
                                    osobj1.TableRefKey = item1.TableKey;
                                    osobj1.AUTHIDS = muserid;
                                    osobj1.AUTHORISE = Model.Authorise;
                                    osobj1.BillDate = (item1.StrBillDate == null || item1.StrBillDate.Trim() == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(item1.StrBillDate);
                                    osobj1.BillNumber = (item1.BillNumber == null) ? "" : item1.BillNumber;
                                    osobj1.CompCode = mcompcode;
                                    osobj1.Broker = 100001;
                                    osobj1.Brokerage = Convert.ToDecimal(0.00);
                                    osobj1.BrokerAmt = Convert.ToDecimal(0.00);
                                    osobj1.BrokerOn = Convert.ToDecimal(0.00);
                                    osobj1.ChlnDate = DateTime.Now;
                                    osobj1.ChlnNumber = "";
                                    osobj1.Code = item.Code;
                                    osobj1.CrPeriod = 0;
                                    osobj1.CurrName = (Model.CurrCode == null || Model.CurrCode == "") ? 0 : Convert.ToInt32(Model.CurrCode);
                                    osobj1.CurrRate = 1;
                                    osobj1.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                    osobj1.Narr = "";
                                    osobj1.OrdDate = DateTime.Now;
                                    osobj1.OrdNumber = "";
                                    osobj1.ProjCode = item.ProjCode;
                                    osobj1.ProjectStage = Model.ProjectStage;
                                    osobj1.ProjectUnit = Model.ProjectUnit;
                                    osobj1.RefParty = "";
                                    osobj1.SalemanAmt = Convert.ToDecimal(0.00);
                                    osobj1.SalemanOn = Convert.ToDecimal(0.00);
                                    osobj1.SalemanPer = Convert.ToDecimal(0.00);
                                    osobj1.Salesman = 100001;
                                    osobj1.TDSAmt = 0;
                                    osobj1.ENTEREDBY = muserid;
                                    osobj1.LASTUPDATEDATE = DateTime.Now;
                                    osobj1.CurrAmount = item1.AdjustAmt;
                                    osobj1.ValueDate = DateTime.Now;
                                    osobj1.LocationCode = item.LocationCode;
                                    ctxTFAT.Outstanding.Add(osobj1);
                                    // second effect
                                    Outstanding osobj2 = new Outstanding();
                                    osobj2.Branch = mbranchcode;
                                    osobj2.DocBranch = mbranchcode;
                                    osobj2.aType = Model.Type;
                                    osobj2.aPrefix = Model.Prefix;
                                    osobj2.aSrl = Model.Srl;
                                    osobj2.aSno = mCnt;
                                    osobj2.ParentKey = Model.ParentKey;
                                    osobj2.TableRefKey = Model.Type + Model.Prefix.Substring(0, 2) + mCnt.ToString("D3") + Model.Srl;
                                    osobj2.Type = item1.Type;
                                    osobj2.aMaintype = Model.MainType;
                                    osobj2.MainType = item1.MainType;
                                    osobj2.SubType = item1.SubType;
                                    osobj2.aSubType = Model.SubType;
                                    osobj2.Prefix = item1.Prefix;
                                    osobj2.Srl = item1.Srl;
                                    osobj2.Sno = item1.Sno;
                                    osobj2.TableKey = item1.TableKey;
                                    osobj2.Amount = (decimal)item1.AdjustAmt;
                                    osobj2.AUTHIDS = muserid;
                                    osobj2.AUTHORISE = Model.Authorise;
                                    osobj2.BillDate = (item1.StrBillDate == null) ? Convert.ToDateTime("1900-01-01") : Convert.ToDateTime(item1.StrBillDate);
                                    osobj2.BillNumber = (item1.BillNumber == null) ? "" : item1.BillNumber;
                                    osobj2.CompCode = mcompcode;
                                    osobj2.Broker = 100001;
                                    osobj2.Brokerage = Convert.ToDecimal(0.00);
                                    osobj2.BrokerAmt = Convert.ToDecimal(0.00);
                                    osobj2.BrokerOn = Convert.ToDecimal(0.00);
                                    osobj2.ChlnDate = DateTime.Now;
                                    osobj2.ChlnNumber = "";
                                    osobj2.Code = item.Code;
                                    osobj2.CrPeriod = 0;
                                    osobj2.CurrName = (item.CurrCode == null || item.CurrCode == "") ? 0 : Convert.ToInt32(item.CurrCode);
                                    osobj2.CurrRate = 1;
                                    osobj2.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                    osobj2.Narr = "";
                                    osobj2.OrdDate = DateTime.Now;
                                    osobj2.OrdNumber = "";
                                    osobj2.ProjCode = item.ProjCode;
                                    osobj2.ProjectStage = Model.ProjectStage;
                                    osobj2.ProjectUnit = Model.ProjectUnit;
                                    osobj2.RefParty = "";
                                    osobj2.SalemanAmt = Convert.ToDecimal(0.00);
                                    osobj2.SalemanOn = Convert.ToDecimal(0.00);
                                    osobj2.SalemanPer = Convert.ToDecimal(0.00);
                                    osobj2.Salesman = 100001;
                                    osobj2.TDSAmt = 0;
                                    osobj2.ENTEREDBY = muserid;
                                    osobj2.LASTUPDATEDATE = DateTime.Now;
                                    osobj2.CurrAmount = item1.AdjustAmt;
                                    osobj2.ValueDate = DateTime.Now;
                                    osobj2.LocationCode = item.LocationCode;
                                    ctxTFAT.Outstanding.Add(osobj2);
                                    Model.count = Model.count + 1;
                                }
                            }
                        }
                        if (item.costcentre1 != null)
                        {
                            int mcnt3 = 1;
                            foreach (var item3 in item.costcentre1)
                            {
                                CostLedger ccobj = new CostLedger();
                                ccobj.MainType = Model.MainType;
                                ccobj.Prefix = Model.Prefix;
                                ccobj.Sno = mcnt3;
                                ccobj.Srl = Model.Srl;
                                ccobj.SubType = Model.SubType;
                                ccobj.Type = Model.Type;
                                ccobj.TableKey = Model.Type + Model.Prefix.Substring(0, 2) + mCnt.ToString("D3") + Model.Srl;
                                ccobj.AUTHIDS = muserid;
                                ccobj.AUTHORISE = Model.Authorise;
                                ccobj.ParentKey = Model.ParentKey;
                                ccobj.Branch = mbranchcode;
                                ccobj.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                ccobj.CompCode = mcompcode;
                                ccobj.Code = item.Code;
                                ccobj.CostCode = item3.CostCode;
                                ccobj.CostGrp = null;
                                ccobj.Debit = (item.AmtType == "Debit") ? item3.CCAmt : 0;
                                ccobj.Credit = (item.AmtType == "Debit") ? 0 : item3.CCAmt;
                                ccobj.ENTEREDBY = muserid;
                                ccobj.LASTUPDATEDATE = DateTime.Now;
                                ccobj.DocAmount = Convert.ToDecimal(0.00);
                                ccobj.LocationCode = 100001;
                                ccobj.SrNo = mcnt3;
                                ctxTFAT.CostLedger.Add(ccobj);
                                ++mcnt3;
                            }
                        }
                        if (item.SubLedgerList != null)
                        {
                            int mcnt4 = 1;
                            foreach (var sublitem in item.SubLedgerList)
                            {
                                LedgerSL ledsl = new LedgerSL();
                                ledsl.Amount = sublitem.CCAmt;
                                ledsl.Branch = mbranchcode;
                                ledsl.Credit = (item.AmtType == "Debit") ? 0 : sublitem.CCAmt;
                                ledsl.Debit = (item.AmtType == "Debit") ? sublitem.CCAmt : 0;
                                ledsl.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                ledsl.LocationCode = 100001;
                                ledsl.MainType = Model.MainType;
                                ledsl.ParentKey = Model.ParentKey;
                                ledsl.Prefix = Model.Prefix;
                                ledsl.SLCode = sublitem.SLCode;
                                ledsl.Sno = mcnt4;
                                ledsl.Srl = Model.Srl;
                                ledsl.SubType = Model.SubType;
                                ledsl.TableKey = Model.Type + Model.Prefix.Substring(0, 2) + mCnt.ToString("D3") + Model.Srl;
                                ledsl.Type = Model.Type;
                                ledsl.ENTEREDBY = muserid;
                                ledsl.LASTUPDATEDATE = DateTime.Now;
                                ledsl.AUTHORISE = Model.Authorise;
                                ledsl.AUTHIDS = muserid;
                                ctxTFAT.LedgerSL.Add(ledsl);
                            }
                        }
                        ++mCnt;
                    }
                    if (Model.NarrStr != null)
                    {
                        Narration mobj2 = new Narration();
                        mobj2.TableKey = Model.ParentKey;
                        mobj2.Branch = mbranchcode;
                        mobj2.Srl = Model.Srl;
                        mobj2.Type = Model.Type;
                        mobj2.CompCode = mcompcode;
                        mobj2.Sno = 0;
                        mobj2.Prefix = Model.Prefix;
                        mobj2.ENTEREDBY = muserid;
                        mobj2.LASTUPDATEDATE = DateTime.Now;
                        mobj2.AUTHIDS = muserid;
                        mobj2.AUTHORISE = Model.Authorise;
                        mobj2.LocationCode = Model.LocationCode;
                        mobj2.NarrRich = Model.NarrStr;
                        mobj2.Narr = Model.NarrStr;
                        mobj2.ParentKey = Model.ParentKey;
                        ctxTFAT.Narration.Add(mobj2);
                    }
                    SaveAddons(Model);
                    if (Model.ChangeLog == "Add")
                    {
                        SaveAttachment(Model);
                    }
                    ctxTFAT.SaveChanges();
                    int mcurrency = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(z => z.CurrName).FirstOrDefault();
                    var amount = list.Sum(x => x.Debit);
                    UpdateAuditTrail(mbranchcode, Model.ChangeLog, Model.Header, Model.ParentKey, DateTime.Now, amount, Model.ParentKey, "Save", "NA");
                    transaction.Commit();
                    transaction.Dispose();
                    //SendTrnsMsg(Model.ChangeLog, Model.Amt, mbranchcode + Model.ParentKey, ConvertDDMMYYTOYYMMDD(Model.DocuDate), "");

                    // process for email/sms sds 18.9.2020
                    if (ctxTFAT.DocTypes.Where(z => z.Code == Model.Type).Select(x => x.SendAlert).FirstOrDefault() == true)
                    {
                        foreach (var item in list)
                        {
                            if ("DSU".Contains(Fieldoftable("Master", "BaseGr", "Code='" + item.Code + "'")))
                            {
                                SendPartywiseSMS(Model.SubType, item.TableKey, item.Code, 0, true, mbranchcode);
                            }
                        }
                    }

                    // unauthorised then generate message
                    if (Model.Authorise.Substring(0, 1) != "A")
                    {
                        string mAuthUser;
                        if (Model.Authorise.Substring(0, 1) == "D")
                        {
                            //mAuthUser=SaveAuthoriseDOC( Model.ParentKey, 0, Model.Date, mcurrency, mbranchcode, muserid);
                        }
                        else
                        {
                            mAuthUser = SaveAuthorise(Model.ParentKey, 0, Model.DocuDate, mcurrency, 1, DateTime.Now, "", mbranchcode, muserid, -1);
                            SendAuthoriseMessage(mAuthUser, Model.ParentKey, Model.DocuDate, Model.Authorise, "" /*Model.AccountName*/);
                        }
                    }

                    Session["ledgerlist"] = null;
                    Session["TempCashBkAttach"] = null;
                    return Json(new { Status = "Success", Model, NewSrl = (mbranchcode + Model.ParentKey) }, JsonRequestBehavior.AllowGet);
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
                    string dd = ex.InnerException.Message;
                    return Json(new
                    {
                        Status = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult PrintData(string Document)
        {
            bool musercr = ctxTFAT.DocTypes.Where(x => x.Code == Document.Substring(6, 5)).Select(x => x.UseCR).FirstOrDefault();
            if (musercr == false)
            {
                return PrintDocumentCrystal(Document, "PDF", false);
                //return PrintDocumentSSRS(Document);
            }
            else
            {
                return PrintDocumentCrystal(Document, "PDF", false);
            }
        }

        //public ActionResult PrintData(string Document)
        //{
        //    if (mAuthNoPrint == true && Model.Authorise.Substring(0, 1) != "A")
        //    {
        //        return Json(new
        //        {
        //            Message = "Can't Print Un-Authorised Document..",
        //            Status = "Error"
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    //return PrintDocumentCrystal(Document,"PDF",false);

        //    bool musercr = ctxTFAT.DocTypes.Where(x => x.Code == Document.Substring(6, 5)).Select(x => x.UseCR).FirstOrDefault();
        //    if (musercr == false)
        //    {
        //        return PrintDocumentSSRS(Document);
        //    }
        //    else
        //    {
        //        return PrintDocumentCrystal(Document, "PDF", false);
        //    }
        //}
        #endregion

        #region Addons
        //[HttpPost]
        //public ActionResult GetAddOnList(LedgerVM Model)
        //{
        //    if (Model.ChangeLog == "Add")
        //    {
        //        List<AddOns> objitemlist = new List<AddOns>();
        //        var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%Doc" + Model.MainType)
        //                      select i).ToList();
        //        foreach (var i in addons)
        //        {
        //            AddOns c = new AddOns();
        //            c.Fld = i.Fld;
        //            c.Head = i.Head;
        //            c.ApplCode = "";
        //            c.QueryText = i.QueryText == null ? "" : GetQueryTextAddon(i.QueryText);
        //            c.FldType = i.FldType;
        //            c.PlaceValue = i.PlaceValue;
        //            c.Eqsn = i.Eqsn;
        //            objitemlist.Add(c);
        //         }

        //        Model.AddOnList = objitemlist;
        //        var html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/CashBank/AddOnGrid.cshtml", new LedgerVM() { AddOnList = Model.AddOnList, ChangeLog = Model.ChangeLog });
        //        var jsonResult = Json(new
        //        {
        //            AddOnList = Model.AddOnList,
        //            ChangeLog = Model.ChangeLog,
        //            Html = html
        //        }, JsonRequestBehavior.AllowGet);
        //        jsonResult.MaxJsonLength = int.MaxValue;
        //        return jsonResult;

        //    }
        //    else
        //    {
        //        List<AddOns> objitemlist = new List<AddOns>();
        //        var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%Doc" + Model.MainType)
        //                      select i).ToList();

        //        int t = 1;
        //        int n = 0;
        //        foreach (var i in addons)
        //        {
        //            AddOns c = new AddOns();
        //            c.Fld = i.Fld;
        //            c.Head = i.Head;
        //            c.ApplCode = GetAddonValue(i.Fld, "CB", Model.ParentKey);
        //            objitemlist.Add(c);
        //            t = t + 1;
        //            n = n + 1;
        //        }


        //        Model.AddOnList = objitemlist;
        //        var html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/CashBank/AddOnGrid.cshtml", new LedgerVM() { AddOnList = Model.AddOnList, ChangeLog = "Edit" });
        //        var jsonResult = Json(new
        //        {
        //            AddOnList = Model.AddOnList,
        //            ChangeLog = "Edit",
        //            Html = html
        //        }, JsonRequestBehavior.AllowGet);
        //        jsonResult.MaxJsonLength = int.MaxValue;
        //        return jsonResult;
        //    }
        //}

        //[HttpGet]
        //public ActionResult GetEditAddOnList(string Code, string ParentKey)
        //{
        //    PurchaseVM Model = new PurchaseVM();
        //    List<AddOns> objitemlist = new List<AddOns>();
        //    var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%Doc" + Code)
        //                  select i).ToList();

        //    int t = 1;
        //    int n = 0;
        //    foreach (var i in addons)
        //    {
        //        AddOns c = new AddOns();
        //        c.Fld = i.Fld;
        //        c.Head = i.Head;
        //        c.ApplCode = GetAddonValue(t, Code, ParentKey);
        //        objitemlist.Add(c);
        //        t = t + 1;
        //        n = n + 1;
        //    }


        //    Model.AddOnList = objitemlist;
        //    var html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/CashBank/AddOnGrid.cshtml", new PurchaseVM() { AddOnList = Model.AddOnList, ChangeLog = "Edit" });
        //    var jsonResult = Json(new
        //    {
        //        AddOnList = Model.AddOnList,
        //        ChangeLog = "Edit",
        //        Html = html
        //    }, JsonRequestBehavior.AllowGet);
        //    jsonResult.MaxJsonLength = int.MaxValue;
        //    return jsonResult;

        //}

        //[HttpPost]
        //public ActionResult AddAddOnsValues(string inputvalues)
        //{
        //    string abc = inputvalues;
        //    return Json(new
        //    {
        //        Status = "Success",
        //        AddonVals = abc
        //    }, JsonRequestBehavior.AllowGet);
        //}

        public List<AddOns> GetAddOnListOnEdit(string Code, string ParentKey, string Type)
        {
            List<AddOns> objitemlist = new List<AddOns>();
            var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%Doc" + Code && x.Hide == false && x.Types.Contains(Type))
                          select i).ToList();
            int t = 1;
            int n = 0;
            foreach (var i in addons)
            {
                AddOns c = new AddOns();
                c.Fld = i.Fld;
                c.Head = i.Head;
                c.ApplCode = GetAddonValueCB(i.Fld, Code, ParentKey);
                c.QueryText = i.QueryText == null ? "" : GetQueryTextAddon(i.QueryText);
                c.FldType = i.FldType;
                c.PlaceValue = i.PlaceValue;
                c.Eqsn = i.Eqsn;
                objitemlist.Add(c);
                ++t;
                ++n;
            }
            return objitemlist;
        }

        [HttpPost]
        public ActionResult CalByEquationAddon(LedgerVM Model)
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

            declist = ConvertAddonString(declist2, Model.MainType, Model.Type);
            List<AddOns> objitemlist = new List<AddOns>();
            var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%Doc" + Model.MainType && x.Hide == false && x.Types.Contains(Model.Type))
                          select i).ToList();
            int t = 1;
            int n = 0;
            foreach (var i in addons)
            {
                AddOns c = new AddOns();
                c.Fld = i.Fld;
                c.Head = i.Head;
                c.ApplCode = declist[n];
                c.QueryText = i.QueryText == null ? "" : GetQueryTextAddon(i.QueryText);
                c.FldType = i.FldType;
                c.PlaceValue = i.PlaceValue;
                c.Eqsn = i.Eqsn;
                c.RECORDKEY = t;
                objitemlist.Add(c);
                ++t;
                ++n;
            }
            var CurrFldRec = objitemlist.Where(x => x.Fld == Model.Fld).Select(x => x.RECORDKEY).FirstOrDefault();
            var NextFldRec = objitemlist.Where(x => x.RECORDKEY > CurrFldRec).Select(x => x.Fld).FirstOrDefault();
            Model.Fld = NextFldRec;
            Model.AddOnList = objitemlist;
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/CashBank/AddOnGrid.cshtml", new LedgerVM() { AddOnList = Model.AddOnList, ChangeLog = Model.ChangeLog, Fld = Model.Fld });
            var jsonResult = Json(new
            {
                AddOnList = Model.AddOnList,
                ChangeLog = Model.ChangeLog,
                Fld = Model.Fld,
                Html = html
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public List<string> ConvertAddonString(List<AddOns> LastModel, string MainType, string Type)
        {
            string j;
            string ab;
            string mamt = "";
            List<string> PopulateStr = new List<string>();
            var trnaddons = ctxTFAT.AddOns.Where(x => x.TableKey == "%Doc" + MainType && x.Hide == false && x.Types.Contains(Type)).Select(x => new { x.Eqsn, x.FldType, x.Fld, x.Sno }).OrderBy(x => new { x.Sno, x.Fld }).ToList();
            for (int i = 0; i < trnaddons.Count; i++)
            {

                var Eqn = trnaddons[i].Eqsn == null ? "" : trnaddons[i].Eqsn.Trim();
                if (Eqn.Contains("%F"))
                {
                    for (int ai = 0; ai < PopulateStr.Count; ai++)
                    {
                        j = trnaddons[ai].Fld;
                        if (LastModel[ai].FldType == "N")
                        {
                            Eqn = Eqn.Replace("%" + j, (PopulateStr[ai] == "" || PopulateStr[ai] == null) ? "0" : PopulateStr[ai]);
                            if (!Eqn.Contains("%F"))
                            {
                                break;
                            }
                        }
                        else if (LastModel[ai].FldType == "T" || LastModel[ai].FldType == "M" || LastModel[ai].FldType == "C" || LastModel[ai].FldType == "X")
                        {
                            Eqn = Eqn.Replace("%" + j, (PopulateStr[ai] == "" || PopulateStr[ai] == null) ? "''" : PopulateStr[ai]);
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
            finalvalue = ReplaceVariables(finalvalue);
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

        //public string GetQueryTextAddon(string sql)
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
        #endregion

        #region Addnote
        public ActionResult AddNote(string SalesManCode)
        {
            ViewBag.ControllerName = ControllerContext.RouteData.Values["controller"].ToString();
            return Json(new
            {
                Status = "Success",
                Controller = ViewBag.ControllerName
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveAddNote(Narration Model, string Code, string mode)
        {
            try
            {
                IList<AddNoteVM> Note = new List<AddNoteVM>();

                if (Session["AddNote"] != null)
                {
                    Note = (List<AddNoteVM>)Session["AddNote"];
                }

                Note.Add(new AddNoteVM()
                {
                    Branch = mbranchcode,
                    Prefix = Model.Prefix,
                    Sno = 0,
                    ENTEREDBY = muserid,
                    LASTUPDATEDATE = DateTime.Now,
                    AUTHIDS = muserid,
                    AUTHORISE = "",
                    LocationCode = 100001,
                    NarrRich = Model.Narr
                });
                Session.Add("AddNote", Note);

            }
            catch (DbEntityValidationException ex1)
            {

                string dd1 = ex1.InnerException.Message;
            }

            return Json(new
            {
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Reference Adjustment
        public ActionResult CheckForRefAdj(LedgerVM Model)
        {
            var result = ctxTFAT.Master.Where(x => x.Code == Model.Code).Select(x => new
            {
                x.ARAP,
                x.ForceCC
            }).FirstOrDefault();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public List<LedgerVM> ShowAdjustListInSession(LedgerVM Model)//while in session
        {
            List<LedgerVM> OSAdjList = new List<LedgerVM>();
            List<LedgerVM> ledgers = new List<LedgerVM>();
            List<LedgerVM> objledgerdetail = new List<LedgerVM>();
            List<string> TblKeys = new List<string>();
            DateTime currdate = DateTime.Now.Date;
            int a = 1;
            bool mARAP = ctxTFAT.Master.Where(x => x.Code == Model.Code).Select(z => z.ARAP).FirstOrDefault();
            if (mARAP == true)
            {
                if (Model.ChangeLog == "Edit")
                {
                    List<string> TableKeys = Model.OSAdjList == null ? TblKeys : Model.OSAdjList.Select(x => x.TableKey).ToList();
                    if (Model.AmtType == "Credit")
                    {
                        ledgers = (from l in ctxTFAT.Ledger
                                   where l.Branch == Model.Branch && l.Code == Model.Code/* && l.LocationCode == Model.LocationCode */&& l.AUTHORISE.Substring(0, 1) == "A" && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(l.Type))
                                   && l.Debit != 0 && l.DocDate <= currdate
                                   select new LedgerVM()
                                   {
                                       Type = l.Type,
                                       Prefix = l.Prefix,
                                       Srl = l.Srl,
                                       Sno = l.Sno,
                                       BillNumber = l.BillNumber,
                                       BillDate = (l.BillDate == null) ? DateTime.Now : l.BillDate.Value,
                                       CurrName = ctxTFAT.CurrencyMaster.Where(x => x.Code == l.CurrName).Select(x => x.Name).FirstOrDefault(),
                                       BalanceAmt = (l.Debit == null) ? 0 : l.Debit.Value - (ctxTFAT.Outstanding.Where(x => x.Code == l.Code && x.TableKey == l.TableKey && x.DocBranch == l.Branch).Sum(x => x.Amount) ?? 0),
                                       BillAmt = l.Debit.Value,
                                       Narr = l.Narr,
                                       Party = l.Party,
                                       ParentKey = l.ParentKey,
                                       TableKey = l.TableKey,
                                       MainType = l.MainType,
                                       SubType = l.SubType
                                   }).Where(x => x.BalanceAmt != 0 || TableKeys.Contains(x.TableKey)).ToList();
                    }
                    else
                    {
                        ledgers = (from l in ctxTFAT.Ledger
                                   where l.Branch == Model.Branch && l.Code == Model.Code /*&& l.LocationCode == Model.LocationCode*/ && l.AUTHORISE.Substring(0, 1) == "A" && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(l.Type))
                                   && l.Credit != 0 && l.DocDate <= currdate
                                   select new LedgerVM()
                                   {
                                       Type = l.Type,
                                       Prefix = l.Prefix,
                                       Srl = l.Srl,
                                       Sno = l.Sno,
                                       BillNumber = l.BillNumber,
                                       BillDate = (l.BillDate == null) ? DateTime.Now : l.BillDate.Value,
                                       CurrName = ctxTFAT.CurrencyMaster.Where(x => x.Code == l.CurrName).Select(x => x.Name).FirstOrDefault(),
                                       BalanceAmt = (l.Credit == null) ? 0 : l.Credit.Value - (ctxTFAT.Outstanding.Where(x => x.Code == l.Code && x.TableKey == l.TableKey && x.DocBranch == l.Branch).Sum(x => x.Amount) ?? 0),
                                       BillAmt = l.Credit.Value,
                                       Narr = l.Narr,
                                       Party = l.Party,
                                       ParentKey = l.ParentKey,
                                       TableKey = l.TableKey,
                                       MainType = l.MainType,
                                       SubType = l.SubType
                                   }).Where(x => x.BalanceAmt != 0 || TableKeys.Contains(x.TableKey)).ToList();
                    }

                    foreach (var r in ledgers)
                    {
                        OSAdjList.Add(new LedgerVM()
                        {
                            Type = r.Type,
                            Prefix = r.Prefix,
                            Srl = r.Srl,
                            Sno = r.Sno,
                            BillNumber = r.BillNumber,
                            StrBillDate = r.BillDate.ToString("dd-MM-yyyy"),
                            CurrName = r.CurrName,
                            BalanceAmt = (r.BalanceAmt + GetAdjAmtFromSession(r.TableKey, Model.tempId)) > r.BillAmt ? r.BillAmt : (r.BalanceAmt + GetAdjAmtFromSession(r.TableKey, Model.tempId)),
                            BillAmt = r.BillAmt,
                            Narr = r.Narr,
                            Party = r.Party,
                            ParentKey = r.ParentKey,
                            TableKey = r.TableKey,
                            AdjustAmt = GetAdjAmtFromSession(r.TableKey, Model.tempId),
                            OSAdjFlag = GetAdjBoolFromSession(r.TableKey, Model.tempId),
                            MainType = r.MainType,
                            SubType = r.SubType,
                            tempId = a
                        });
                        ++a;
                    }
                }
                else
                {
                    if (Model.AmtType == "Credit")
                    {
                        ledgers = (from l in ctxTFAT.Ledger
                                   where l.Branch == Model.Branch && l.Code == Model.Code/* && l.LocationCode == Model.LocationCode */&& l.AUTHORISE.Substring(0, 1) == "A" && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(l.Type))
                                   && l.Debit != 0 && l.DocDate <= currdate
                                   select new LedgerVM()
                                   {
                                       Type = l.Type,
                                       Prefix = l.Prefix,
                                       Srl = l.Srl,
                                       Sno = l.Sno,
                                       BillNumber = l.BillNumber,
                                       BillDate = (l.BillDate == null) ? DateTime.Now : l.BillDate.Value,
                                       CurrName = ctxTFAT.CurrencyMaster.Where(x => x.Code == l.CurrName).Select(x => x.Name).FirstOrDefault(),
                                       BalanceAmt = (l.Debit == null) ? 0 : l.Debit.Value - (ctxTFAT.Outstanding.Where(x => x.Code == l.Code && x.TableKey == l.TableKey && x.DocBranch == l.Branch).Sum(x => x.Amount) ?? 0),
                                       BillAmt = l.Debit.Value,
                                       Narr = l.Narr,
                                       Party = l.Party,
                                       ParentKey = l.ParentKey,
                                       TableKey = l.TableKey,
                                       MainType = l.MainType,
                                       SubType = l.SubType
                                   }).Where(x => x.BalanceAmt != 0).ToList();
                    }
                    else
                    {
                        ledgers = (from l in ctxTFAT.Ledger
                                   where l.Branch == Model.Branch && l.Code == Model.Code /*&& l.LocationCode == Model.LocationCode*/ && l.AUTHORISE.Substring(0, 1) == "A" && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(l.Type))
                                   && l.Credit != 0 && l.DocDate <= currdate
                                   select new LedgerVM()
                                   {
                                       Type = l.Type,
                                       Prefix = l.Prefix,
                                       Srl = l.Srl,
                                       Sno = l.Sno,
                                       BillNumber = l.BillNumber,
                                       BillDate = (l.BillDate == null) ? DateTime.Now : l.BillDate.Value,
                                       CurrName = ctxTFAT.CurrencyMaster.Where(x => x.Code == l.CurrName).Select(x => x.Name).FirstOrDefault(),
                                       BalanceAmt = (l.Credit == null) ? 0 : l.Credit.Value - (ctxTFAT.Outstanding.Where(x => x.Code == l.Code && x.TableKey == l.TableKey && x.DocBranch == l.Branch).Sum(x => x.Amount) ?? 0),
                                       BillAmt = l.Credit.Value,
                                       Narr = l.Narr,
                                       Party = l.Party,
                                       ParentKey = l.ParentKey,
                                       TableKey = l.TableKey,
                                       MainType = l.MainType,
                                       SubType = l.SubType
                                   }).Where(x => x.BalanceAmt != 0).ToList();
                    }
                    foreach (var r in ledgers)
                    {
                        OSAdjList.Add(new LedgerVM()
                        {

                            Type = r.Type,
                            Prefix = r.Prefix,
                            Srl = r.Srl,
                            Sno = r.Sno,
                            BillNumber = r.BillNumber,
                            StrBillDate = r.BillDate.ToString("dd-MM-yyyy"),
                            CurrName = r.CurrName,
                            BalanceAmt = r.BalanceAmt,
                            BillAmt = r.BillAmt,
                            Narr = r.Narr,
                            Party = r.Party,
                            ParentKey = r.ParentKey,
                            TableKey = r.TableKey,
                            AdjustAmt = GetAdjAmtFromSession(r.TableKey, Model.tempId),
                            OSAdjFlag = GetAdjBoolFromSession(r.TableKey, Model.tempId),
                            MainType = r.MainType,
                            SubType = r.SubType,
                            tempId = a
                        });
                        ++a;
                    }
                }
            }
            return OSAdjList;
        }

        public ActionResult GetReferenceAdjustments(LedgerVM Model)
        {
            List<LedgerVM> OSAdjList = new List<LedgerVM>();
            List<LedgerVM> ledgers = new List<LedgerVM>();
            DateTime currdate = DateTime.Now.Date;
            //bool mARAP = ctxTFAT.Master.Where(x => x.Code == Model.Code ).Select(z => z.ARAP).FirstOrDefault();
            //if (mARAP == true)
            //{
            //    if (Model.AmtType == "Credit")
            //    {
            //        ledgers = (from l in ctxTFAT.Ledger
            //                   where /*l.Branch == Model.Branch &&*/ l.Code == Model.Code/* && l.LocationCode == Model.LocationCode */&& l.AUTHORISE.Substring(0, 1) == "A" && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(l.Type))
            //                   && l.Debit != 0 && l.DocDate <= currdate
            //                   select new LedgerVM()
            //                   {
            //                       Branch = l.Branch,
            //                       Type = l.Type,
            //                       Prefix = l.Prefix,
            //                       Srl = l.Srl,
            //                       Sno = l.Sno,
            //                       BillNumber = l.BillNumber,
            //                       BillDate = (l.BillDate == null) ? DateTime.Now : l.BillDate.Value,
            //                       CurrName = ctxTFAT.CurrencyMaster.Where(x => x.Code == l.CurrName).Select(x => x.Name).FirstOrDefault(),
            //                       BalanceAmt = (l.Debit == null) ? 0 : l.Debit.Value - (ctxTFAT.Outstanding.Where(x => /*x.Code == l.Code &&*/ x.TableRefKey == l.TableKey && x.Branch == l.Branch).Sum(x => x.Amount) ?? 0),
            //                       BillAmt = l.Debit.Value,
            //                       Narr = l.Narr,
            //                       Party = l.Party,
            //                       ParentKey = l.ParentKey,
            //                       TableKey = l.TableKey,
            //                       MainType = l.MainType,
            //                       SubType = l.SubType
            //                   }).Where(x => x.BalanceAmt != 0).ToList();
            //    }
            //    else
            //    {
            //        ledgers = (from l in ctxTFAT.Ledger
            //                   where /*l.Branch == Model.Branch && */l.Code == Model.Code /*&& l.LocationCode == Model.LocationCode*/ && l.AUTHORISE.Substring(0, 1) == "A" && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(l.Type))
            //                   && l.Credit != 0 && l.DocDate <= currdate
            //                   select new LedgerVM()
            //                   {
            //                       Branch = l.Branch,
            //                       Type = l.Type,
            //                       Prefix = l.Prefix,
            //                       Srl = l.Srl,
            //                       Sno = l.Sno,
            //                       BillNumber = l.BillNumber,
            //                       BillDate = (l.BillDate == null) ? DateTime.Now : l.BillDate.Value,
            //                       CurrName = ctxTFAT.CurrencyMaster.Where(x => x.Code == l.CurrName).Select(x => x.Name).FirstOrDefault(),
            //                       BalanceAmt = (l.Credit == null) ? 0 : l.Credit.Value - (ctxTFAT.Outstanding.Where(x => /*x.Code == l.Code &&*/ x.TableRefKey == l.TableKey && x.Branch == l.Branch).Sum(x => x.Amount) ?? 0),
            //                       BillAmt = l.Credit.Value,
            //                       Narr = l.Narr,
            //                       Party = l.Party,
            //                       ParentKey = l.ParentKey,
            //                       TableKey = l.TableKey,
            //                       MainType = l.MainType,
            //                       SubType = l.SubType
            //                   }).Where(x => x.BalanceAmt != 0).ToList();
            //    }
            //}
            //int a = 1;
            //foreach (var r in ledgers)
            //{
            //    OSAdjList.Add(new LedgerVM()
            //    {
            //        Branch=r.Branch,
            //        Type = r.Type,
            //        Prefix = r.Prefix,
            //        Srl = r.Srl,
            //        Sno = r.Sno,
            //        BillNumber = r.BillNumber,
            //        StrBillDate = r.BillDate.ToString("dd-MM-yyyy"),
            //        CurrName = r.CurrName,
            //        BalanceAmt = r.BalanceAmt,
            //        BillAmt = r.BillAmt,
            //        Narr = r.Narr,
            //        Party = r.Party,
            //        ParentKey = r.ParentKey,
            //        TableKey = r.TableKey,
            //        MainType = r.MainType,
            //        SubType = r.SubType,
            //        tempId = a
            //    });
            //    ++a;
            //}
            //Model.PartyAdvances = GetPartyAdvances(Model.Code, Model.SubType);
            //Model.PartyNetOutstanding = GetBalance(Model.Code, currdate, mbranchcode, Model.LocationCode);
            //Model.PartyConsOutstanding = GetBalance(Model.Code, currdate, "", 0);
            var html = ViewHelper.RenderPartialView(this, "ReferenceAdj", new LedgerVM() { OSAdjList = OSAdjList, PartyAdvances = Model.PartyAdvances, PartyNetOutstanding = Model.PartyNetOutstanding, PartyConsOutstanding = Model.PartyConsOutstanding });
            return Json(new { OSAdjList = OSAdjList, Html = html }, JsonRequestBehavior.AllowGet);
        }

        public decimal GetPartyAdvances(string Party, string SubType)
        {
            string sql = "";
            decimal AdvAmt = 0;
            if (SubType == "BP" || SubType == "HP")
            {
                AdvAmt = (from l in ctxTFAT.Ledger
                          where l.Branch == mbranchcode && l.Code == Party && l.Debit != 0
                          select new
                          {
                              Amount = l.Debit + l.Credit - (ctxTFAT.Outstanding.Where(x => x.Code == l.Code && x.TableKey == l.TableKey && x.DocBranch == l.Branch).Sum(x => x.Amount) ?? 0)
                          }).Sum(x => x.Amount) ?? 0;
            }
            else
            {
                AdvAmt = (from l in ctxTFAT.Ledger
                          where l.Branch == mbranchcode && l.Code == Party && l.Credit != 0
                          select new
                          {
                              Amount = l.Debit + l.Credit - (ctxTFAT.Outstanding.Where(x => x.Code == l.Code && x.TableKey == l.TableKey && x.DocBranch == l.Branch).Sum(x => x.Amount) ?? 0)
                          }).Sum(x => x.Amount) ?? 0;
            }
            return AdvAmt;
        }

        public decimal GetNetOutstanding(string Party)
        {
            string sql = "";
            decimal abc = 0;
            sql = @"dbo.GetBalance(" + Party + ", , , , " + mbranchcode + ", , , , , , )";
            string mamtm;
            DataTable smDt = GetDataTable(sql, GetConnectionString());
            if (smDt.Rows.Count > 0)
            {
                mamtm = (smDt.Rows[0][0].ToString() == "" || smDt.Rows[0][0].ToString() == null) ? "" : String.Format("{0:0.00}", smDt.Rows[0][0]);
                if (mamtm != "")
                {
                    abc = Convert.ToDecimal(mamtm);
                }
            }
            else
            {
                abc = 0;
            }
            return abc;
        }

        public decimal GetConsOutstanding(string Party)
        {
            string sql = "";
            decimal abc = 0;
            sql = @"dbo.GetBalance(" + Party + ", , , , 'ALL')";
            string mamtm;
            DataTable smDt = GetDataTable(sql, GetConnectionString());
            if (smDt.Rows.Count > 0)
            {
                mamtm = (smDt.Rows[0][0].ToString() == "" || smDt.Rows[0][0].ToString() == null) ? "" : String.Format("{0:0.00}", smDt.Rows[0][0]);
                if (mamtm != "")
                {
                    abc = Convert.ToDecimal(mamtm);
                }
            }
            else
            {
                abc = 0;
            }
            return abc;
        }

        public ActionResult GetReferencePickUp(LedgerVM Model)
        {
            List<LedgerVM> objitemlist = new List<LedgerVM>();
            List<LedgerVM> ordersstk = new List<LedgerVM>();
            var mBaseGr = ctxTFAT.Master.Where(x => x.Code == Model.Code).Select(x => x.BaseGr).FirstOrDefault();
            List<string> mrefdocs = new List<string>();
            var mledgerrefs = ctxTFAT.Ledger.Where(x => x.Code == Model.Code && (x.RefDoc != null && x.RefDoc != "")).Select(x => x.RefDoc).ToList();
            if (mledgerrefs != null && mledgerrefs.Count > 0)
            {
                foreach (var a in mledgerrefs)
                {
                    if (string.IsNullOrEmpty(a) == false)
                    {
                        var mrefvars = a.Split(',');
                        mrefdocs.AddRange(mrefvars);
                    }
                }
            }
            if (mBaseGr == "D" || mBaseGr == "S" || mBaseGr == "U")
            {
                ordersstk = (from r in ctxTFAT.Orders
                             where r.Branch == mbranchcode && r.Code == Model.Code && r.AUTHORISE.StartsWith("A") && !mrefdocs.Contains(r.TableKey)
                             select new LedgerVM()
                             {
                                 Type = r.Type,
                                 Srl = r.Srl,
                                 BillNumber = r.BillNumber,
                                 BillDate = r.BillDate,
                                 Party = r.Code,
                                 Amount = r.Amt,
                                 Prefix = r.Prefix
                             }).ToList();
            }
            else
            {
                ordersstk = (from r in ctxTFAT.Ledger
                             where r.Branch == mbranchcode && r.Party == Model.Code
                             select new LedgerVM()
                             {
                                 Type = r.Type,
                                 Srl = r.Srl,
                                 BillNumber = r.BillNumber,
                                 BillDate = r.BillDate.Value,
                                 Party = r.Party,
                                 Amount = r.Debit.Value + r.Credit.Value,
                                 Prefix = r.Prefix
                             }).ToList();
            }

            foreach (var r in ordersstk)
            {
                objitemlist.Add(new LedgerVM()
                {
                    Type = r.Type,
                    Srl = r.Srl,
                    BillNumber = r.BillNumber,
                    StrBillDate = r.BillDate.ToString("dd-MM-yyyy"),
                    Party = r.Party,
                    Amount = r.Amount,
                    Prefix = r.Prefix
                });
            }
            var html = ViewHelper.RenderPartialView(this, "RefPickUp", new LedgerVM() { NewReferenceList = objitemlist });
            return Json(new { NewReferenceList = objitemlist, Html = html }, JsonRequestBehavior.AllowGet);
        }

        public decimal GetAdjAmtFromSession(string TableKey, int tempid)
        {
            decimal Adjamt;
            List<LedgerVM> objledgerdetail = new List<LedgerVM>();
            if (Session["ledgerlist"] != null)
            {
                objledgerdetail = (List<LedgerVM>)Session["ledgerlist"];
            }
            var AdjamtLIST = objledgerdetail.Where(x => x.tempId == tempid && x.tempIsDeleted == false).Select(x => x.OSAdjList).FirstOrDefault();
            if (AdjamtLIST != null)
            {
                Adjamt = AdjamtLIST.Where(x => x.TableKey == TableKey).Select(x => x.AdjustAmt).FirstOrDefault();
            }
            else
            {
                Adjamt = 0;
            }
            return Adjamt;
        }

        public bool GetAdjBoolFromSession(string TableKey, int tempid)
        {
            bool Adjamtbool;
            List<LedgerVM> objledgerdetail = new List<LedgerVM>();
            if (Session["ledgerlist"] != null)
            {
                objledgerdetail = (List<LedgerVM>)Session["ledgerlist"];
            }

            var AdjamtLIST = objledgerdetail.Where(x => x.tempId == tempid && x.tempIsDeleted == false).Select(x => x.OSAdjList).FirstOrDefault();
            if (AdjamtLIST != null)
            {
                Adjamtbool = AdjamtLIST.Where(x => x.TableKey == TableKey).Select(x => x.OSAdjFlag).FirstOrDefault();
            }
            else
            {
                Adjamtbool = false;
            }
            return Adjamtbool;
        }
        #endregion

        #region Account Details
        public ActionResult FindAccountdetails()
        {
            ViewBag.ControllerName = ControllerContext.RouteData.Values["controller"].ToString();
            return Json(new
            {
                Status = "Success",
                Controller = ViewBag.ControllerName
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            //////ITransactionGridOperation mIlst = new TransactionGridOperation();
            //return GetGridDataColumns(Model.id, "X", "");
            DateTime currdate = DateTime.Now.Date;
            var PartyAdvances = GetPartyAdvances(Model.Code, Model.SubType);
            var PartyNetOutstanding = GetBalance(Model.Code, currdate, mbranchcode, Model.LocationCode);
            var PartyConsOutstanding = GetBalance(Model.Code, currdate, "", 0);
            return GetGridDataColumns(Model.id, "X", "", PartyAdvances + "|" + PartyNetOutstanding + "|" + PartyConsOutstanding);
        }
        public ActionResult GetGridData(GridOption Model)
        {
            if (Model.ViewDataId == "CROSList")
            {
                mpara = "para01" + "^" + Model.AcType.Substring(0, 1) + "~";
                Model.Date = DateTime.Now.ToShortDateString() + ":" + DateTime.Now.ToShortDateString();
                if (String.IsNullOrEmpty(Model.ReportTypeL))
                {
                    mpara += "para02" + "^" + "" + "~";
                }
                else
                {
                    mpara += "para02" + "^" + Model.ReportTypeL + "~";
                }

            }
            return GetGridReport(Model, "X", "Code^" + Model.Code + (mpara != "" ? "~" + mpara : ""), false, 0);
        }

        [HttpPost]
        public ActionResult GetAccDetailList(GridOption Model)
        {
            return Content(JQGridHelper.JsonForJqgrid(GetAccData(Model), Model.rows, GetTotalCount(Model), Model.page), "application/json");
        }

        public DataTable GetAccData(GridOption Model)
        {
            int startIndex = (Model.page - 1) * Model.rows;
            int endIndex = Model.page * Model.rows;
            if (Model.searchtext != null)
            {
                if (Model.MainType == "PM")
                {
                    Model.query = @"WITH PAGED_Stock  AS
                        (
                         select Code,Name,ROW_NUMBER() OVER (ORDER BY " + Model.sidx + @" " + Model.sord + @") AS RowNumber FROM Master
                        where  BaseGr ='S')
                        SELECT Code,Name,RowNumber
                        FROM PAGED_Stock
                        WHERE " + Model.searchtype + @" like " + "'" + "%" + Model.searchtext + "%" + "'" + @"";
                }
                else
                {
                    Model.query = @"WITH PAGED_Stock  AS
                        (
                         select Code,Name,ROW_NUMBER() OVER (ORDER BY " + Model.sidx + @" " + Model.sord + @") AS RowNumber FROM Master
                        where  BaseGr ='D' or BaseGr = 'U')
                        SELECT Code,Name,RowNumber
                        FROM PAGED_Stock
                        WHERE " + Model.searchtype + @" like " + "'" + "%" + Model.searchtext + "%" + "'" + @"";
                }

            }
            else
            {
                if (Model.MainType == "PM")
                {
                    Model.query = @"WITH PAGED_Stock  AS
                        (
                         select Code,Name,ROW_NUMBER() OVER (ORDER BY " + Model.sidx + @" " + Model.sord + @") 
                         AS RowNumber FROM Master
                         where  BaseGr ='S')
                        SELECT Code,Name,RowNumber
                        FROM PAGED_Stock
                        WHERE RowNumber BETWEEN " + startIndex + @" AND " + endIndex + @"";
                }
                else
                {
                    Model.query = @"WITH PAGED_Stock  AS
                        (
                         select Code,Name,ROW_NUMBER() OVER (ORDER BY " + Model.sidx + @" " + Model.sord + @") 
                         AS RowNumber FROM Master
                         where  BaseGr ='D' or BaseGr ='U' )
                        SELECT Code,Name,RowNumber
                        FROM PAGED_Stock
                        WHERE RowNumber BETWEEN " + startIndex + @" AND " + endIndex + @"";
                }
            }
            DataTable dtable = new DataTable();
            DataSet ds = new DataSet();
            SqlConnection conn = new SqlConnection(GetConnectionString());
            SqlDataAdapter adap = new SqlDataAdapter(Model.query, GetConnectionString());
            //var rows = adap.Fill(dt);
            adap.Fill(ds);
            dtable = ds.Tables[0];
            dtable.Columns.Add("XYZ", typeof(string)).SetOrdinal(0);
            return dtable;
        }

        public int GetTotalCount(GridOption Model)
        {
            string sql = "select Count(*) FROM Master where ((BaseGr ='S')or(BaseGr ='D')or(BaseGr ='U'))";
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(GetConnectionString());
                SqlCommand comm = new SqlCommand(sql, conn);
                conn.Open();
                return (int)comm.ExecuteScalar();
            }
            catch
            {
            }
            finally
            {
                try
                {
                    if (ConnectionState.Closed != conn.State)
                    {
                        conn.Close();
                    }
                }
                catch
                {
                }
            }
            return -1;
        }
        #endregion

        #region attachment
        public ActionResult UploadFile()
        {
            ViewBag.ControllerName = ControllerContext.RouteData.Values["controller"].ToString();
            return Json(new { Status = "Success", Controller = ViewBag.ControllerName }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AttachDocument(HttpPostedFileBase files)
        {
            string XYZ = "";
            string docstr = "";
            string FLN = "";
            List<LedgerVM> DocList = new List<LedgerVM>();
            if (Session["TempCashBkAttach"] != null)
            {
                DocList = (List<LedgerVM>)Session["TempCashBkAttach"];
            }
            int n = DocList.Count() + 1;
            byte[] fileData = null;
            for (int i = 0; i < Request.Files.Count; i++)
            {
                LedgerVM Model = new LedgerVM();
                var file = Request.Files[i];
                var fileName = Path.GetFileName(file.FileName);

                if (DocList.Select(x => x.FileName).Contains(fileName))
                {
                    return Json(new { Message = "File with same Name Already Uploaded Cant Upload", Status = "CancelError" }, JsonRequestBehavior.AllowGet);
                }
                MemoryStream target = new MemoryStream();
                using (var binaryReader = new BinaryReader(Request.Files[i].InputStream))
                {
                    fileData = binaryReader.ReadBytes(Request.Files[i].ContentLength);
                }
                Model.ImageData = fileData;
                string FileString = Convert.ToBase64String(fileData);
                file.InputStream.CopyTo(target);
                Model.FileContent = file.ContentType;
                Model.ImageStr = FileString;
                Model.FileName = fileName;
                Model.ContentType = file.ContentType;
                Model.tempId = i + 1;
                DocList.Add(Model);
            }

            if (DocList.Count > 0)
            {
                foreach (var a in DocList.Where(x => x.tempIsDeleted == false))
                {
                    docstr = docstr + a.ImageStr + ",";
                }
                if (docstr != "")
                {
                    docstr = docstr.Remove(docstr.Length - 1);
                }
                string docfilnam = "";
                foreach (var b in DocList.Where(x => x.tempIsDeleted == false))
                {
                    docfilnam = docfilnam + b.FileName + ",";
                }
                if (docfilnam != "")
                {
                    docfilnam = docfilnam.Remove(docfilnam.Length - 1);
                }
                XYZ = docstr;
                FLN = docfilnam;
            }
            Session["TempCashBkAttach"] = DocList;
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Accounts/Views/CashBank/AttachmentDocument.cshtml", new LedgerVM() { DocumentList = DocList, AllFileStr = XYZ, FileNameStr = FLN, ChangeLog = "Add" });
            var jsonResult = Json(new { DocumentList = DocList, AllFileStr = XYZ, FileNameStr = FLN, Html = html, ChangeLog = "Add" }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }
        [HttpPost]
        public ActionResult EditImage(HttpPostedFileBase files, string Document)
        {
            string XYZ = "";

            string FLN = "";
            int length = Document.Length;

            var ParentKey = Document.Substring(6, length - 6);
            string Type = ParentKey.Substring(0, 5);
            List<LedgerVM> DocList = new List<LedgerVM>();
            //var img = ctxTFAT.Attachment.Where(x => x.ParentKey == ParentKey).Select(x => x).ToList();
            if (Session["TempCashBkAttach"] != null)
            {
                DocList = (List<LedgerVM>)Session["TempCashBkAttach"];
            }
            int n = DocList.Count() + 1;

            byte[] fileData = null;
            string docstr = "";

            for (int i = 0; i < Request.Files.Count; i++)
            {
                LedgerVM Model = new LedgerVM();

                var file = Request.Files[i];
                var fileName = Path.GetFileName(file.FileName);
                MemoryStream target = new MemoryStream();
                using (var binaryReader = new BinaryReader(Request.Files[i].InputStream))
                {
                    fileData = binaryReader.ReadBytes(Request.Files[i].ContentLength);
                }
                Model.ImageData = fileData;
                string FileString = Convert.ToBase64String(fileData);
                file.InputStream.CopyTo(target);
                Model.FileContent = file.ContentType;
                Model.ImageStr = FileString;
                Model.FileName = fileName;
                Model.ContentType = file.ContentType;
                Model.tempId = n;
                DocList.Add(Model);
                ++n;
            }
            if (DocList.Count > 0)
            {
                foreach (var a in DocList.Where(x => x.tempIsDeleted == false))
                {
                    docstr = docstr + a.ImageStr + ",";
                }
                if (docstr != "")
                {
                    docstr = docstr.Remove(docstr.Length - 1);
                }
                string docfilnam = "";
                foreach (var b in DocList.Where(x => x.tempIsDeleted == false))
                {
                    docfilnam = docfilnam + b.FileName + ",";
                }
                if (docfilnam != "")
                {
                    docfilnam = docfilnam.Remove(docfilnam.Length - 1);
                }
                XYZ = docstr;
                FLN = docfilnam;
            }
            Session["TempCashBkAttach"] = DocList;
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Accounts/Views/CashBank/AttachmentDocument.cshtml", new LedgerVM() { DocumentList = DocList, AllFileStr = XYZ, FileNameStr = FLN, ChangeLog = "Edit" });
            var jsonResult = Json(new { DocumentList = DocList, AllFileStr = XYZ, FileNameStr = FLN, Html = html, ChangeLog = "Edit" }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public ActionResult DeleteUploadFile(LedgerVM Model)
        {
            string XYZ = "";

            string FLN = "";
            string docstr = "";

            Model.DocumentList = Model.DocumentList.Where(x => x.tempId != Model.tempId).Select(x => x).ToList();
            if (Model.DocumentList.Count > 0)
            {
                foreach (var a in Model.DocumentList)
                {
                    docstr = docstr + a.ImageStr + ",";
                }
                if (docstr != "")
                {
                    docstr = docstr.Remove(docstr.Length - 1);
                }
                string docfilnam = "";
                foreach (var b in Model.DocumentList)
                {
                    docfilnam = docfilnam + b.FileName + ",";
                }
                if (docfilnam != "")
                {
                    docfilnam = docfilnam.Remove(docfilnam.Length - 1);
                }
                XYZ = docstr;
                FLN = docfilnam;
            }
            Session["TempCashBkAttach"] = Model.DocumentList;

            var html = ViewHelper.RenderPartialView(this, "~/Areas/Accounts/Views/CashBank/AttachmentDocument.cshtml", new LedgerVM() { DocumentList = Model.DocumentList, AllFileStr = XYZ, FileNameStr = FLN, ChangeLog = Model.ChangeLog });
            var jsonResult = Json(new { DocumentList = Model.DocumentList, AllFileStr = XYZ, FileNameStr = FLN, Html = html, ChangeLog = Model.ChangeLog }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public FileResult Download(int tempId)
        {
            List<LedgerVM> DocList = new List<LedgerVM>();
            if (Session["TempCashBkAttach"] != null)
            {
                DocList = (List<LedgerVM>)Session["TempCashBkAttach"];
            }

            var dwnfile = DocList.Where(x => x.tempId == tempId).FirstOrDefault();

            string filename = dwnfile.FileName;
            byte[] fileBytes = Convert.FromBase64String(dwnfile.ImageStr);

            return File(fileBytes, dwnfile.FileContent, filename);
        }

        public void SaveAttachment(LedgerVM Model)
        {
            List<LedgerVM> DocList = new List<LedgerVM>();
            if (Session["TempCashBkAttach"] != null)
            {
                DocList = (List<LedgerVM>)Session["TempCashBkAttach"];
            }

            if (DocList != null && DocList.Count != 0)
            {
                //List<string> FileString = Model.AllFileStr.Split(',').ToList();
                //List<string> NameString = Model.FileNameStr.Split(',').ToList();
                int c = 0;
                int an = 1;
                foreach (var item in DocList)
                {
                    Directory.CreateDirectory(attachmentPath + Model.ParentKey);
                    string directoryPath = attachmentPath + Model.ParentKey + "/" + item.FileName;
                    byte[] IData = Convert.FromBase64String(item.ImageStr);
                    Attachment att = new Attachment();
                    att.AUTHIDS = muserid;
                    att.AUTHORISE = Model.Authorise;
                    att.Branch = mbranchcode;
                    att.Code = "";
                    att.ENTEREDBY = muserid;
                    att.FilePath = directoryPath;
                    att.LASTUPDATEDATE = DateTime.Now;
                    att.LocationCode = Model.LocationCode;
                    att.Prefix = Model.Prefix;
                    att.Sno = an;
                    att.Srl = Model.Srl;
                    att.SrNo = an;
                    att.TableKey = Model.Type + Model.Prefix.Substring(0, 2) + an.ToString("D3") + Model.Srl;
                    att.ParentKey = Model.ParentKey;
                    att.Type = Model.Type;
                    att.CompCode = mcompcode;
                    ctxTFAT.Attachment.Add(att);
                    ctxTFAT.SaveChanges();
                    System.IO.File.WriteAllBytes(directoryPath, IData);
                    ++an;
                    ++c;
                }
            }
        }

        public List<LedgerVM> GetAttachmentListInEdit(LedgerVM Model)
        {
            List<LedgerVM> AttachmentList = new List<LedgerVM>();
            var docdetail = ctxTFAT.Attachment.Where(x => x.ParentKey == Model.ParentKey).ToList();
            foreach (var item in docdetail)
            {
                AttachmentList.Add(new LedgerVM()
                {
                    FileName = Path.GetFileName(item.FilePath),
                    Srl = item.Srl,
                    Code = item.Code,
                    TableKey = item.TableKey,
                    ParentKey = item.ParentKey,
                    Type = item.Type,
                    tempId = item.Sno,
                    SrNo = item.Sno,
                    Path = item.FilePath,
                    FileContent = Path.GetExtension(item.FilePath),
                    ImageStr = Convert.ToBase64String(System.IO.File.ReadAllBytes(item.FilePath)),
                    tempIsDeleted = false
                });
            }
            return AttachmentList;
        }
        #endregion

        #region Convert MV
        [HttpPost]
        public ActionResult ConvertMemVoucher(LedgerVM Model)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    int mcurrency = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(z => z.CurrName).FirstOrDefault();
                    if (mcurrency == 0)
                    {
                        mcurrency = 1;//india
                    }

                    Model.ParentKey = Model.Document.Substring(6, Model.Document.Length - 6);

                    var mLedger = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey).Select(x => x).FirstOrDefault();
                    string mType = mLedger.Type;
                    string mBranch = mLedger.Branch;
                    string mRefType = ctxTFAT.DocTypes.Where(x => x.Code == mType).Select(x => x.RefType).FirstOrDefault();
                    string mPrefix = mLedger.Prefix;
                    var MLedList = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey).Select(x => x).ToList();
                    //var MLedBrList = ctxTFAT.LedgerBranch.Where(x => x.ParentKey == Model.ParentKey).Select(x => x).ToList();
                    var MTdsPayment = ctxTFAT.TDSPayments.Where(x => x.ParentKey == Model.ParentKey).Select(x => x).ToList();
                    var Moutstanding = ctxTFAT.Outstanding.Where(x => x.ParentKey == Model.ParentKey && x.Type == mType && x.DocBranch== mBranch).Select(x => x).ToList();
                    var Moutstanding2 = ctxTFAT.Outstanding.Where(x => x.ParentKey == Model.ParentKey && x.DocBranch == mBranch).Select(x => x).ToList();
                    var MAddons = ctxTFAT.AddonDocCB.Where(x => x.ParentKey == Model.ParentKey).Select(x => x).ToList();
                    var MAttachments = ctxTFAT.Attachment.Where(x => x.ParentKey == Model.ParentKey).Select(x => x).ToList();

                    DateTime mDocdate = mLedger.DocDate;
                    if (mRefType != null)
                    {
                        var mcdoctype = ctxTFAT.DocTypes.Where(x => x.Code == mRefType).Select(x => x).FirstOrDefault();
                        string mcSubtype = mcdoctype.SubType;
                        string mcType = mcdoctype.Code;
                        string mcMainType = mcdoctype.MainType;

                        Model.Srl = GetLastSerial("Ledger", mbranchcode, mRefType, mPrefix, mcSubtype, mDocdate);


                        foreach (var item in MLedList)
                        {
                            Ledger mobj1 = new Ledger();
                            mobj1.TableKey = mRefType + mPrefix.Substring(0, 2) + item.Sno.ToString("D3") + Model.Srl;
                            mobj1.ParentKey = mRefType + mPrefix.Substring(0, 2) + Model.Srl;
                            mobj1.Sno = item.Sno;
                            mobj1.Srl = Model.Srl;
                            mobj1.SubType = mcSubtype;
                            mobj1.Type = mRefType;
                            mobj1.AltCode = item.AltCode;
                            mobj1.Audited = item.Audited;
                            mobj1.AUTHIDS = item.AUTHIDS;
                            mobj1.AUTHORISE = item.AUTHORISE;
                            mobj1.BillDate = item.BillDate;
                            mobj1.BillNumber = item.BillNumber;
                            mobj1.Branch = item.Branch;
                            mobj1.CompCode = item.CompCode;
                            mobj1.Cheque = item.Cheque;
                            mobj1.ChequeReturn = item.ChequeReturn;
                            mobj1.ClearDate = item.ClearDate;
                            mobj1.ChequeDate = item.ChequeDate;
                            mobj1.Code = item.Code;
                            mobj1.CrPeriod = item.CrPeriod;
                            mobj1.CurrName = item.CurrName;
                            mobj1.CurrRate = item.CurrRate;
                            mobj1.Debit = item.Debit;
                            mobj1.Credit = item.Credit;
                            mobj1.Discounted = item.Discounted;
                            mobj1.DocDate = item.DocDate;
                            mobj1.MainType = mcMainType;
                            mobj1.Narr = item.Narr;
                            mobj1.Party = item.Party;
                            mobj1.Prefix = mPrefix;
                            mobj1.RecoFlag = item.RecoFlag;
                            mobj1.RefDoc = item.RefDoc;
                            mobj1.TDSChallanNumber = item.TDSChallanNumber;
                            mobj1.TDSCode = item.TDSCode;
                            mobj1.TDSFlag = item.TDSFlag;
                            mobj1.ENTEREDBY = item.ENTEREDBY;
                            mobj1.DueDate = item.DueDate;
                            mobj1.Reminder = item.Reminder;
                            mobj1.TaskID = item.TaskID;
                            mobj1.ChqCategory = item.ChqCategory;
                            mobj1.CurrAmount = item.CurrAmount;
                            mobj1.LocationCode = item.LocationCode;
                            mobj1.LASTUPDATEDATE = item.LASTUPDATEDATE;
                            mobj1.GSTType = item.GSTType;
                            mobj1.IGSTAmt = item.IGSTAmt;
                            mobj1.IGSTRate = item.IGSTRate;
                            mobj1.CGSTAmt = item.CGSTAmt;
                            mobj1.CGSTRate = item.CGSTRate;
                            mobj1.SGSTAmt = item.SGSTAmt;
                            mobj1.SGSTRate = item.SGSTRate;
                            ctxTFAT.Ledger.Add(mobj1);
                        }

                        //foreach (var item in MLedBrList)
                        //{
                        //    LedgerBranch mobjLB1 = new LedgerBranch();
                        //    mobjLB1.Branch = item.Branch;
                        //    mobjLB1.xBranch = item.xBranch;
                        //    mobjLB1.CompCode = item.CompCode;
                        //    mobjLB1.TableKey = mRefType + mPrefix.Substring(0, 2) + item.Sno.ToString("D3") + Model.Srl;
                        //    mobjLB1.ParentKey = mRefType + mPrefix.Substring(0, 2) + Model.Srl;
                        //    mobjLB1.Sno = item.Sno;
                        //    mobjLB1.Srl = Model.Srl;
                        //    mobjLB1.SubType = mcSubtype;
                        //    mobjLB1.Audited = item.Audited;
                        //    mobjLB1.AUTHIDS = item.AUTHIDS;
                        //    mobjLB1.AUTHORISE = item.AUTHORISE;
                        //    mobjLB1.BillDate = item.BillDate;
                        //    mobjLB1.Cheque = item.Cheque;
                        //    mobjLB1.BillNumber = item.BillNumber;
                        //    mobjLB1.ClearDate = item.ClearDate;
                        //    mobjLB1.Code = item.Code;
                        //    mobjLB1.Debit = item.Debit;
                        //    mobjLB1.CurrName = item.CurrName;
                        //    mobjLB1.CurrRate = item.CurrRate;
                        //    mobjLB1.Credit = item.Credit;
                        //    mobjLB1.DocDate = item.DocDate;
                        //    mobjLB1.MainType = mcMainType;
                        //    mobjLB1.Narr = item.Narr;
                        //    mobjLB1.Party = item.Party;
                        //    mobjLB1.Prefix = mPrefix;
                        //    mobjLB1.RecoFlag = item.RecoFlag;
                        //    mobjLB1.TDSCode = item.TDSCode;
                        //    mobjLB1.TDSFlag = item.TDSFlag;
                        //    mobjLB1.Type = mRefType;
                        //    mobjLB1.ValueDate = item.ValueDate;
                        //    mobjLB1.xBranch = item.xBranch;
                        //    mobjLB1.ENTEREDBY = item.ENTEREDBY;
                        //    mobjLB1.LASTUPDATEDATE = item.LASTUPDATEDATE;
                        //    mobjLB1.BankCode = item.BankCode;
                        //    mobjLB1.ChequeReturn = item.ChequeReturn;
                        //    mobjLB1.ChqCategory = item.ChqCategory;
                        //    mobjLB1.CurrAmount = item.CurrAmount;
                        //    mobjLB1.ProjCode = item.ProjCode;
                        //    mobjLB1.ProjectStage = item.ProjectStage;
                        //    mobjLB1.ProjectUnit = item.ProjectUnit;
                        //    mobjLB1.RefDoc = item.RefDoc;
                        //    mobjLB1.SLCode = item.SLCode;
                        //    mobjLB1.LocationCode = item.LocationCode;
                        //    ctxTFAT.LedgerBranch.Add(mobjLB1);

                        //}

                        int tCnt = 1;
                        foreach (var item in MTdsPayment)
                        {
                            TDSPayments tds = new TDSPayments();
                            tds.Amount = item.Amount;
                            tds.aMainType = item.aMainType;
                            tds.aPrefix = mPrefix;
                            tds.aSno = tCnt;
                            tds.aSrl = Model.Srl;
                            tds.SubType = mcSubtype;
                            tds.TableKey = mRefType + mPrefix.Substring(0, 2) + tCnt.ToString("D3") + Model.Srl;
                            tds.ParentKey = mRefType + mPrefix.Substring(0, 2) + Model.Srl;
                            tds.TDSAble = item.TDSAble;
                            tds.TDSAmt = item.TDSAmt;
                            tds.TDSCess = item.TDSCess;
                            tds.TDSCessAmt = item.TDSCessAmt;
                            tds.TDSCode = item.TDSCode;
                            tds.TDSReason = item.TDSReason;
                            tds.TDSSheCess = item.TDSSheCess;
                            tds.TDSSheCessAmt = item.TDSSheCessAmt;
                            tds.TDSSurCharge = item.TDSSurCharge;
                            tds.TDSSurChargeAmt = item.TDSSurChargeAmt;
                            tds.TDSTax = item.TDSTax;
                            tds.TDSTaxAmt = item.TDSTaxAmt;
                            tds.TotalTDSAmt = item.TDSTaxAmt;
                            tds.TotalTDSAmt = item.TotalTDSAmt;
                            tds.Type = mRefType;
                            tds.aSubType = mcSubtype;
                            tds.aType = mRefType;
                            tds.AUTHIDS = item.AUTHIDS;
                            tds.AUTHORISE = item.AUTHORISE;
                            tds.BankCode = item.BankCode;
                            tds.BillNumber = item.BillNumber;
                            tds.Branch = item.Branch;
                            tds.CertDate = item.CertDate;
                            tds.CertNumber = item.CertNumber;
                            tds.ChallanDate = item.ChallanDate;
                            tds.ChallanNumber = item.ChallanNumber;
                            tds.CNO = item.CNO;
                            tds.Code = item.Code;
                            tds.CompCode = item.CompCode;
                            tds.DepositSerial = item.DepositSerial;
                            tds.DocDate = item.DocDate;
                            tds.DueDate = item.DueDate;
                            tds.EndCredit = item.EndCredit;
                            tds.ENTEREDBY = item.ENTEREDBY;
                            tds.LASTUPDATEDATE = item.LASTUPDATEDATE;
                            tds.LocationCode = item.LocationCode;
                            tds.MainType = mcMainType;
                            tds.Narr = item.Narr;
                            tds.PaidAmt = item.PaidAmt;

                            tds.Party = item.Party;
                            tds.PaymentMode = item.PaymentMode;
                            tds.Prefix = mPrefix;
                            tds.Sno = tCnt;
                            tds.Srl = Model.Srl;
                            ctxTFAT.TDSPayments.Add(tds);
                        }

                        int oCnt = 1;
                        foreach (var item in Moutstanding)
                        {
                            Outstanding osobj1 = new Outstanding();

                            osobj1.Branch = item.Branch;
                            osobj1.DocBranch = item.Branch;
                            osobj1.MainType = mcMainType;
                            osobj1.SubType = mcSubtype;
                            osobj1.Type = mRefType;
                            osobj1.Prefix = mPrefix;
                            osobj1.Srl = Model.Srl;
                            osobj1.Sno = item.Sno;
                            osobj1.ParentKey = mRefType + mPrefix.Substring(0, 2) + Model.Srl;
                            osobj1.TableKey = mRefType + mPrefix.Substring(0, 2) + item.Sno.ToString("D3") + Model.Srl;
                            osobj1.aMaintype = item.aMaintype;
                            osobj1.aSubType = item.aSubType;
                            osobj1.aType = item.aType;
                            osobj1.aPrefix = item.aPrefix;
                            osobj1.aSrl = item.aSrl;
                            osobj1.aSno = item.aSno;
                            osobj1.Amount = item.Amount;
                            osobj1.TableRefKey = item.TableRefKey;
                            osobj1.AUTHIDS = item.AUTHIDS;
                            osobj1.AUTHORISE = item.AUTHORISE;
                            osobj1.BillDate = item.BillDate;
                            osobj1.BillNumber = item.BillNumber;
                            osobj1.CompCode = item.CompCode;
                            osobj1.Broker = item.Broker;
                            osobj1.Brokerage = item.Brokerage;
                            osobj1.BrokerAmt = item.BrokerAmt;
                            osobj1.BrokerOn = item.BrokerOn;
                            osobj1.ChlnDate = item.ChlnDate;
                            osobj1.ChlnNumber = item.ChlnNumber;
                            osobj1.Code = item.Code;
                            osobj1.CrPeriod = item.CrPeriod;
                            osobj1.CurrName = item.CurrName;
                            osobj1.CurrRate = item.CurrRate;
                            osobj1.DocDate = item.DocDate;
                            osobj1.Narr = item.Narr;
                            osobj1.OrdDate = item.OrdDate;
                            osobj1.OrdNumber = item.OrdNumber;
                            osobj1.ProjCode = item.ProjCode;
                            osobj1.ProjectStage = item.ProjectStage;
                            osobj1.ProjectUnit = item.ProjectUnit;
                            osobj1.RefParty = item.RefParty;
                            osobj1.SalemanAmt = item.SalemanAmt;
                            osobj1.SalemanOn = item.SalemanOn;
                            osobj1.SalemanPer = item.SalemanPer;
                            osobj1.Salesman = item.Salesman;
                            osobj1.TDSAmt = item.TDSAmt;
                            osobj1.ENTEREDBY = item.ENTEREDBY;
                            osobj1.LASTUPDATEDATE = item.LASTUPDATEDATE;
                            osobj1.CurrAmount = item.CurrAmount;
                            osobj1.ValueDate = item.ValueDate;
                            osobj1.LocationCode = item.LocationCode;
                            ctxTFAT.Outstanding.Add(osobj1);

                            Outstanding osobj2 = new Outstanding();
                            osobj2.Branch = item.Branch;
                            osobj2.DocBranch = item.Branch;
                            osobj2.aMaintype = mcMainType;
                            osobj2.aSubType = mcSubtype;
                            osobj2.aType = mRefType;
                            osobj2.aPrefix = mPrefix;
                            osobj2.aSrl = Model.Srl;
                            osobj2.aSno = item.Sno;
                            osobj2.ParentKey = mRefType + mPrefix.Substring(0, 2) + Model.Srl;
                            osobj2.TableKey = item.TableRefKey;
                            osobj2.MainType = item.aMaintype;
                            osobj2.SubType = item.aSubType;
                            osobj2.Type = item.aType;
                            osobj2.Prefix = item.aPrefix;
                            osobj2.Srl = item.aSrl;
                            osobj2.Sno = item.aSno;
                            osobj2.Amount = item.Amount;
                            osobj2.TableRefKey = mRefType + mPrefix.Substring(0, 2) + item.Sno.ToString("D3") + Model.Srl;
                            osobj2.AUTHIDS = item.AUTHIDS;
                            osobj2.AUTHORISE = item.AUTHORISE;
                            osobj2.BillDate = item.BillDate;
                            osobj2.BillNumber = item.BillNumber;
                            osobj2.CompCode = item.CompCode;
                            osobj2.Broker = item.Broker;
                            osobj2.Brokerage = item.Brokerage;
                            osobj2.BrokerAmt = item.BrokerAmt;
                            osobj2.BrokerOn = item.BrokerOn;
                            osobj2.ChlnDate = item.ChlnDate;
                            osobj2.ChlnNumber = item.ChlnNumber;
                            osobj2.Code = item.Code;
                            osobj2.CrPeriod = item.CrPeriod;
                            osobj2.CurrName = item.CurrName;
                            osobj2.CurrRate = item.CurrRate;
                            osobj2.DocDate = item.DocDate;
                            osobj2.Narr = item.Narr;
                            osobj2.OrdDate = item.OrdDate;
                            osobj2.OrdNumber = item.OrdNumber;
                            osobj2.ProjCode = item.ProjCode;
                            osobj2.ProjectStage = item.ProjectStage;
                            osobj2.ProjectUnit = item.ProjectUnit;
                            osobj2.RefParty = item.RefParty;
                            osobj2.SalemanAmt = item.SalemanAmt;
                            osobj2.SalemanOn = item.SalemanOn;
                            osobj2.SalemanPer = item.SalemanPer;
                            osobj2.Salesman = item.Salesman;
                            osobj2.TDSAmt = item.TDSAmt;
                            osobj2.ENTEREDBY = item.ENTEREDBY;
                            osobj2.LASTUPDATEDATE = item.LASTUPDATEDATE;
                            osobj2.CurrAmount = item.CurrAmount;
                            osobj2.ValueDate = item.ValueDate;
                            osobj2.LocationCode = item.LocationCode;

                            ctxTFAT.Outstanding.Add(osobj2);
                        }
                        foreach (var item in MAddons)
                        {
                            AddonDocCB aip = new AddonDocCB();
                            aip.AUTHIDS = item.AUTHIDS;
                            aip.AUTHORISE = item.AUTHORISE;
                            aip.DocDate = item.DocDate;
                            aip.ENTEREDBY = item.ENTEREDBY;
                            aip.F001 = item.F001;
                            aip.F002 = item.F002;
                            aip.F003 = item.F003;
                            aip.F004 = item.F004;
                            aip.F005 = item.F005;
                            aip.F006 = item.F006;
                            aip.F007 = item.F007;
                            aip.F008 = item.F008;
                            aip.F009 = item.F009;
                            aip.F010 = item.F010;
                            aip.F011 = item.F011;
                            aip.F012 = item.F012;
                            aip.F013 = item.F013;
                            aip.F014 = item.F014;
                            aip.F015 = item.F015;
                            aip.F016 = item.F016;
                            aip.F017 = item.F017;
                            aip.F018 = item.F018;
                            aip.F019 = item.F019;
                            aip.F020 = item.F020;
                            aip.F021 = item.F021;
                            aip.F022 = item.F022;
                            aip.F023 = item.F023;
                            aip.F024 = item.F024;
                            aip.F025 = item.F025;
                            aip.F026 = item.F026;
                            aip.F027 = item.F027;
                            aip.F028 = item.F028;
                            aip.F029 = item.F029;
                            aip.F030 = item.F030;
                            aip.LASTUPDATEDATE = DateTime.Now;
                            aip.ParentKey = mRefType + mPrefix.Substring(0, 2) + Model.Srl;
                            aip.TableKey = mRefType + mPrefix.Substring(0, 2) + Model.Srl;
                            aip.Type = mRefType;
                            ctxTFAT.AddonDocCB.Add(aip);
                        }
                        int attCnt = 1;
                        foreach (var item in MAttachments)
                        {
                            Attachment att = new Attachment();
                            att.AUTHIDS = item.AUTHIDS;
                            att.AUTHORISE = item.AUTHORISE;
                            att.Branch = item.Branch;
                            att.Code = item.Code;
                            att.ENTEREDBY = item.ENTEREDBY;
                            att.FilePath = item.FilePath;
                            att.LASTUPDATEDATE = item.LASTUPDATEDATE;
                            att.LocationCode = item.LocationCode;
                            att.Prefix = mPrefix;
                            att.Sno = attCnt;
                            att.Srl = Model.Srl;
                            att.SrNo = attCnt;
                            att.TableKey = mRefType + mPrefix.Substring(0, 2) + attCnt.ToString("D3") + Model.Srl;
                            att.ParentKey = mRefType + mPrefix.Substring(0, 2) + Model.Srl;
                            att.Type = mRefType;
                            att.CompCode = item.CompCode;
                            ctxTFAT.Attachment.Add(att);
                        }
                        string mAuthUser = SaveAuthorise(mRefType + mPrefix.Substring(0, 2) + Model.Srl, 0, mDocdate.ToString("dd-MM-yyyy"), mcurrency, 1, DateTime.Now, "", mbranchcode, muserid, -1);
                        UpdateAuditTrail(mbranchcode, "Add", "Converion of Memorandum Voucher", mRefType + mPrefix.Substring(0, 2) + Model.Srl, DateTime.Now, 0, mRefType + mPrefix.Substring(0, 2) + Model.Srl, "", "NA");
                        ctxTFAT.TDSPayments.RemoveRange(MTdsPayment);
                        ctxTFAT.Outstanding.RemoveRange(Moutstanding2);
                        ctxTFAT.AddonDocCB.RemoveRange(MAddons);
                        ctxTFAT.Attachment.RemoveRange(MAttachments);
                        //ctxTFAT.LedgerBranch.RemoveRange(MLedBrList);
                        ctxTFAT.Ledger.RemoveRange(MLedList);
                        ctxTFAT.SaveChanges();
                        transaction.Commit();
                        transaction.Dispose();
                    }
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    return Json(new
                    {
                        Status = "Error",
                        ex1.InnerException.InnerException.Message
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    return Json(new
                    {
                        Status = "Error",
                        ex.InnerException.InnerException.Message
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new
            {
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region MultiPrint
        [HttpPost]
        public ActionResult GetMultiPrint(LedgerVM Model)
        {
            List<GridOption> Grlist = new List<GridOption>();
            var list = ctxTFAT.DocFormats.Where(x => x.Type == Model.Type).Select(x => x).ToList();
            if (list != null)
            {
                foreach (var a in list)
                {
                    Grlist.Add(new GridOption
                    {
                        Format = a.FormatCode,
                        IsFormatSelected = a.Selected,
                        StoreProcedure = a.StoredProc
                    });
                }
            }
            Model.PrintGridList = Grlist;
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/CashBank/MultiPrint.cshtml", new LedgerVM() { PrintGridList = Model.PrintGridList, Document = Model.Document });
            var jsonResult = Json(new
            {
                Model.Document,
                Model.PrintGridList,
                Html = html
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [HttpPost]
        public ActionResult SaveMultiPrint(GridOption Model)
        {
            var FormatList = Model.Format.Split(',');
            foreach (var a in FormatList)
            {
                var docformat = ctxTFAT.DocFormats.Where(x => x.Type == Model.Type && x.FormatCode == a).Select(x => x).FirstOrDefault();
                docformat.Selected = true;
                ctxTFAT.Entry(docformat).State = EntityState.Modified;
                ctxTFAT.SaveChanges();
            }
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public List<LedgerVM> GetOutstandingInEdit(string ParentKey, string TableKey, string MainType)
        {
            List<LedgerVM> OSAdjList = new List<LedgerVM>();
            var ledgers = (from o in ctxTFAT.Outstanding
                           join l in ctxTFAT.Ledger on new { o.Branch, o.TableKey, o.ParentKey } equals new { l.Branch, l.TableKey, l.ParentKey }
                           where o.TableKey == TableKey && o.ParentKey == ParentKey && o.MainType == MainType
                           select new
                           {
                               o.aType,
                               o.aPrefix,
                               o.aSrl,
                               o.aSno,
                               o.BillNumber,
                               o.BillDate,
                               o.CurrName,
                               o.Amount,
                               o.Code,
                               o.ParentKey,
                               o.TableRefKey,
                               o.Narr,
                               o.aMaintype,
                               o.aSubType,
                           }).ToList();
            int a = 1;
            foreach (var r in ledgers)
            {
                OSAdjList.Add(new LedgerVM()
                {
                    Type = r.aType,
                    Prefix = r.aPrefix,
                    Srl = r.aSrl,
                    Sno = r.aSno,
                    BillNumber = r.BillNumber,
                    BillDate = (r.BillDate == null) ? Convert.ToDateTime("1900-01-01") : r.BillDate.Value,
                    CurrName = (ctxTFAT.CurrencyMaster.Where(x => x.Code == r.CurrName).Select(x => x).FirstOrDefault() == null) ? "" : ctxTFAT.CurrencyMaster.Where(x => x.Code == r.CurrName).Select(x => x.Name).FirstOrDefault(),
                    BalanceAmt = 0,
                    BillAmt = 0,
                    Narr = r.Narr,
                    Party = r.Code,
                    ParentKey = r.ParentKey,
                    TableKey = r.TableRefKey,
                    AdjustAmt = (r.Amount == null) ? 0 : r.Amount.Value,
                    OSAdjFlag = (r.Amount == null) ? false : true,
                    SubType = r.aSubType,
                    MainType = r.aMaintype,
                    tempId = a
                });
                ++a;
            }
            return OSAdjList;
        }

        public List<LedgerVM> GetCCenterInEdit(string ParentKey, string TableKey, string MainType)
        {
            List<LedgerVM> costcentre = new List<LedgerVM>();
            var cclist = ctxTFAT.CostLedger.Where(x => x.ParentKey == ParentKey && x.TableKey == TableKey).Select(x => x).ToList();
            foreach (var item in cclist)
            {
                costcentre.Add(new LedgerVM
                {
                    CostCode = item.CostCode,
                    Name = ctxTFAT.CostCentre.Where(x => x.Code == item.CostCode).Select(x => x.Name).FirstOrDefault(),
                    CCAmt = (item.Debit == null ? 0 : item.Debit.Value) + (item.Credit == null ? 0 : item.Credit.Value)
                });
            }
            return costcentre;
        }

        public List<LedgerVM> GetSubLedgerListInEdit(string ParentKey, string TableKey, string MainType)
        {
            List<LedgerVM> subledgers = new List<LedgerVM>();
            var subllist = ctxTFAT.LedgerSL.Where(x => x.ParentKey == ParentKey && x.TableKey == TableKey).Select(x => x).ToList();
            foreach (var item in subllist)
            {
                subledgers.Add(new LedgerVM
                {
                    SLCode = item.SLCode,
                    Name = ctxTFAT.SubLedger.Where(x => x.Code == item.SLCode).Select(x => x.Name).FirstOrDefault(),
                    CCAmt = (item.Debit == null ? 0 : item.Debit) + (item.Credit == null ? 0 : item.Credit)
                });
            }
            return subledgers;
        }

        public decimal GetSubLedgerAmtFromSession(int SLCode, int tempid)
        {
            decimal Adjamt;
            List<LedgerVM> objledgerdetail = new List<LedgerVM>();
            if (Session["ledgerlist"] != null)
            {
                objledgerdetail = (List<LedgerVM>)Session["ledgerlist"];
            }
            var AdjamtLIST = objledgerdetail.Where(x => x.tempId == tempid).Select(x => x.SubLedgerList).FirstOrDefault();
            if (AdjamtLIST != null)
            {
                Adjamt = AdjamtLIST.Where(x => x.SLCode == SLCode).Select(x => x.CCAmt).FirstOrDefault();
            }
            else
            {
                Adjamt = 0;
            }
            return Adjamt;
        }

        public List<LedgerVM> ShowSubLedgerListInSession(LedgerVM Model)//while in session
        {
            List<LedgerVM> SubList = new List<LedgerVM>();
            List<LedgerVM> objledgerdetail = new List<LedgerVM>();
            List<string> TblKeys = new List<string>();
            DateTime currdate = DateTime.Now.Date;
            int a = 1;
            var ledgers = ctxTFAT.SubLedger.Select(x => x).ToList();
            foreach (var r in ledgers)
            {
                SubList.Add(new LedgerVM()
                {
                    CCAmt = GetSubLedgerAmtFromSession(r.Code, Model.tempId),
                    SLCode = r.Code,
                    Name = r.Name,
                    tempId = a
                });
                ++a;
            }
            return SubList;
        }

        public List<LedgerVM> ShowCostCentreListInSession(LedgerVM Model)//while in session
        {
            List<LedgerVM> objledgerdetail = new List<LedgerVM>();
            if (Session["ledgerlist"] != null)
            {
                objledgerdetail = (List<LedgerVM>)Session["ledgerlist"];
            }
            var CCLIST = objledgerdetail.Where(x => x.tempId == Model.tempId).Select(x => x.costcentre1).FirstOrDefault();
            List<LedgerVM> SubList = new List<LedgerVM>();

            List<string> TblKeys = new List<string>();
            DateTime currdate = DateTime.Now.Date;
            int a = 1;
            var ledgers = ctxTFAT.CostCentre.Select(x => x).ToList();
            foreach (var r in ledgers)
            {
                SubList.Add(new LedgerVM()
                {
                    CCAmt = (CCLIST == null) ? 0 : CCLIST.Where(x => x.CostCode == r.Code).Select(x => (decimal?)x.CCAmt).FirstOrDefault() ?? 0,
                    CostCode = r.Code,
                    Name = r.Name,
                    tempId = a
                });
                ++a;
            }

            return SubList;
        }

        public string GetGSTName(string Code)
        {
            string name = "";

            var tname = ctxTFAT.TaxMaster.Where(x => x.Code == Code).Select(x => x).FirstOrDefault();
            if (tname != null)
            {
                name = tname.Name;
            }
            return name;
        }

        public string GetCurrName(string Code)
        {
            string name = "";
            int CCODE = Convert.ToInt32(Code);
            var tname = ctxTFAT.CurrencyMaster.Where(x => x.Code == CCODE).Select(x => x).FirstOrDefault();
            if (tname != null)
            {
                name = tname.Name;
            }
            return name;
        }

        public decimal GetTDSRate(int? Code)
        {
            decimal rate = 0;
            var tds = ctxTFAT.TDSRates.Where(x => x.Code == Code).Select(x => x).FirstOrDefault();
            if (tds != null)
            {
                rate = tds.TDSRate == null ? 0 : tds.TDSRate.Value;
            }
            return rate;
        }

        private void SaveAddons(LedgerVM Model)
        {
            string addo1, addo2;
            StringBuilder addonT = new StringBuilder();
            List<string> addlist = new List<string>();
            if (Model.AddOnList != null && Model.AddOnList.Count > 0)
            {
                var addoni = ctxTFAT.AddonDocCB.Where(x => x.ParentKey == Model.ParentKey && x.TableKey == Model.ParentKey).Select(x => x).FirstOrDefault();
                if (addoni != null)
                {
                    ctxTFAT.AddonDocCB.Remove(addoni);
                }

                AddonDocCB aip = new AddonDocCB();
                aip.AUTHIDS = muserid;
                aip.AUTHORISE = "A00";
                aip.DocDate = DateTime.Now;
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
                aip.F031 = Model.AddOnList.Where(x => x.Fld == "F031").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F031").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F032 = Model.AddOnList.Where(x => x.Fld == "F032").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F032").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F033 = Model.AddOnList.Where(x => x.Fld == "F033").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F033").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F034 = Model.AddOnList.Where(x => x.Fld == "F034").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F034").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F035 = Model.AddOnList.Where(x => x.Fld == "F035").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F035").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F036 = Model.AddOnList.Where(x => x.Fld == "F036").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F036").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F037 = Model.AddOnList.Where(x => x.Fld == "F037").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F037").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F038 = Model.AddOnList.Where(x => x.Fld == "F038").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F038").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F039 = Model.AddOnList.Where(x => x.Fld == "F039").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F039").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F040 = Model.AddOnList.Where(x => x.Fld == "F040").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F040").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F041 = Model.AddOnList.Where(x => x.Fld == "F041").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F041").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F042 = Model.AddOnList.Where(x => x.Fld == "F042").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F042").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F043 = Model.AddOnList.Where(x => x.Fld == "F043").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F043").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F044 = Model.AddOnList.Where(x => x.Fld == "F044").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F044").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F045 = Model.AddOnList.Where(x => x.Fld == "F045").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F045").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F046 = Model.AddOnList.Where(x => x.Fld == "F046").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F046").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F047 = Model.AddOnList.Where(x => x.Fld == "F047").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F047").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F048 = Model.AddOnList.Where(x => x.Fld == "F048").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F048").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F049 = Model.AddOnList.Where(x => x.Fld == "F049").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F049").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.F050 = Model.AddOnList.Where(x => x.Fld == "F050").Select(x => x) != null ? Model.AddOnList.Where(x => x.Fld == "F050").Select(x => x.ApplCode).FirstOrDefault() : "0";
                aip.LASTUPDATEDATE = DateTime.Now;
                aip.ParentKey = Model.ParentKey;
                aip.TableKey = Model.ParentKey;
                aip.Type = Model.Type;
                ctxTFAT.AddonDocCB.Add(aip);
            }
        }

        public string GetTDSName(int Code)
        {
            string name = "";
            var tds = ctxTFAT.TDSMaster.Where(x => x.Code == Code).Select(x => x).FirstOrDefault();
            if (tds != null)
            {
                name = tds == null ? "" : tds.Name;
            }
            return name;
        }

        public string GetAddonValueCB(string fld, string MainType, string ParentKey)
        {
            string connstring = GetConnectionString();
            string bca = "";
            var loginQuery3 = @"select " + fld + " from addondoccb where tablekey=" + "'" + ParentKey + "'" + "";
            DataTable mDt2 = GetDataTable(loginQuery3, connstring);
            if (mDt2.Rows.Count > 0)
            {
                bca = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? "" : mDt2.Rows[0][0].ToString();
            }
            return bca;
        }

        public string GetDraweeBankName(string Code)
        {
            int mCode = 0;
            string name = "";
            if (Code != null && Code != "")
            {
                mCode = Convert.ToInt32(Code);
                var tname = ctxTFAT.BankMaster.Where(x => x.Code == mCode).Select(x => x).FirstOrDefault();
                if (tname != null)
                {
                    name = tname.Name;
                }
            }
            else
            {
            }
            return name;
        }
    }
}