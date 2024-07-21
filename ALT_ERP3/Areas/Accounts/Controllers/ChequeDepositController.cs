using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
using ALT_ERP3.Models;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class ChequeDepositController : BaseController
    {
        List<SelectListItem> branch = new List<SelectListItem>();
        private DataTable table = new DataTable();
        List<string> header = new List<string>();
        string attachmentPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentPath"].ToString()) == true ? "" : ConfigurationManager.AppSettings["AttachmentPath"].ToString();
        private static string mauthorise = "A00";
        private static bool mAuthNoPrint = false;
        private static bool mAuthReq = false;
        private static bool mAuthLock = false;
        private static bool mAuthAgain = false;
        private static int mcurrency;
        private static int mconstantmode = 0;

        // GET: Accounts/ChequeDeposit
        public ActionResult Index(LedgerVM Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            mcurrency = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(z => z.CurrName).FirstOrDefault();
            if (mcurrency == 0)
                mcurrency = 1;//india
            Session["ledgerlist"] = null;
            Session["TempCashBkAttach"] = null;
            Model.ViewDataId = Model.ViewDataId;
            Model.Controller2 = Model.Controller2;
            Model.optiontype = Model.optiontype;
            Model.OptionCode = Model.OptionCode;
            Model.Header = Model.Header;
            Model.TableName = Model.TableName;
            Model.Module = Model.Module;
            mauthorise = "A00";
            UpdateAuditTrail(mbranchcode, Model.ChangeLog, Model.Header, "", DateTime.Now, 0, "", "", "A");
            if (Model.ChangeLog == "Add")
            {
                var result = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => new
                {
                    x.MainType,
                    x.SubType,
                    x.Name,
                    x.ConstantMode,
                    x.Constant
                }).FirstOrDefault();
                Model.Header = result.Name;
                Model.Branch = mbranchcode;
                Model.MainType = result.MainType;
                Model.SubType = result.SubType;
                Model.Prefix = mperiod;
                Model.Document = "";
                Model.DocDate = DateTime.Now;
                var CurrCode = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.CurrName).FirstOrDefault();
                Model.CurrCode = CurrCode.ToString();
                Model.CurrName = GetCurrName(Model.CurrCode);
                Model.CurrRate = ctxTFAT.CurrencyMaster.Where(x => x.Code == CurrCode).Select(x => x.CurrRate).FirstOrDefault();
                List<AddOns> objitemlist = new List<AddOns>();
                if (Model.ChangeLog == "Add")
                {
                    var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%Doc" + Model.MainType && x.Hide == false && x.Types.Contains(Model.Type))
                                  select i).ToList();

                    foreach (var i in addons)
                    {
                        AddOns c = new AddOns();
                        c.Fld = i.Fld;
                        c.Head = i.Head;
                        c.ApplCode = "";
                        c.QueryText = i.QueryText == null ? "" : GetQueryTextAddon(i.QueryText);
                        c.FldType = i.FldType;
                        c.PlaceValue = i.PlaceValue;
                        c.Eqsn = i.Eqsn;
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
            }
            else
            {
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
                    x.Constant
                }).FirstOrDefault();

                Model.MainType = result.MainType;
                Model.SubType = result.SubType;
                Model.Prefix = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey).Select(x => x.Prefix).FirstOrDefault();
                Model.AddOnList = GetAddOnListOnEdit(Model.MainType, Model.ParentKey, Model.Type);
                var objledgerdetail = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey).Select(x => x).FirstOrDefault();
                Model.Code = objledgerdetail.Code;
                Model.AccountName = NameofAccount(objledgerdetail.Code);
                Model.BankCashCode = objledgerdetail.AltCode;
                Model.BankCashName = NameofAccount(objledgerdetail.AltCode);
                Model.RefDoc = objledgerdetail.RefDoc;
                var objledgerdetail2 = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey && x.Sno == 3).Select(x => x).FirstOrDefault();
                if (objledgerdetail2 != null)
                {
                    Model.BankChgAcc = objledgerdetail2.Code;
                    Model.BankChgAccName = NameofAccount(objledgerdetail2.Code);
                    Model.BankCharges = objledgerdetail2.Debit.Value;
                    Model.StrBankDate = objledgerdetail2.DocDate == null || objledgerdetail2.DocDate == Convert.ToDateTime("0001-01-01") ? "01-01-1900" : objledgerdetail2.DocDate.ToString("dd-MM-yyyy");
                }
                Model.Amt = objledgerdetail.Debit.Value;
                var objledgerdetail5 = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey && x.Sno == 5).Select(x => x).FirstOrDefault();
                if (objledgerdetail5 != null)
                {
                    Model.PartyCharges = objledgerdetail5.Debit.Value;
                }
                mauthorise = objledgerdetail.AUTHORISE;
                Model.DocDate = objledgerdetail.DocDate;
                Model.Branch = objledgerdetail.Branch;
                Model.Srl = objledgerdetail.Srl;
                Model.BankCashCode = objledgerdetail.AltCode;
                Model.StrChqDate = objledgerdetail.ChequeDate == null ? "01-01-1900" : objledgerdetail.ChequeDate.Value.ToString("dd-MM-yyyy");
                Model.Cheque = objledgerdetail.Cheque;
                Model.CurrCode = (objledgerdetail.CurrName == 0) ? "" : objledgerdetail.CurrName.ToString();
                Model.CurrName = GetCurrName(Model.CurrCode);
                Model.CurrRate = objledgerdetail.CurrRate;
                Model.Narr = objledgerdetail.Narr;
                Model.LocationCode = objledgerdetail.LocationCode;
                //#region ATTACHMENT
                //Model.DocumentList = GetAttachmentListInEdit(Model);
                //string docstr = "";
                //if (Model.DocumentList.Count > 0)
                //{
                //    foreach (var a in Model.DocumentList)
                //    {
                //        docstr = docstr + a.ImageStr + ",";
                //    }
                //    if (docstr != "")
                //    {
                //        docstr = docstr.Remove(docstr.Length - 1);
                //    }
                //    string docfilnam = "";
                //    foreach (var b in Model.DocumentList)
                //    {
                //        docfilnam = docfilnam + b.FileName + ",";
                //    }
                //    if (docfilnam != "")
                //    {
                //        docfilnam = docfilnam.Remove(docfilnam.Length - 1);
                //    }
                //    Model.AllFileStr = docstr;
                //    Model.FileNameStr = docfilnam;
                //}
                //Session["TempCashBkAttach"] = Model.DocumentList;
                //#endregion
            }
            var mAuth = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == Model.Type).Select(x => new
            {
                x.AuthLock,
                x.AuthNoPrint,
                x.AuthReq
            }).FirstOrDefault();
            if (mAuth != null)
            {
                mAuthLock = mAuth.AuthLock;
                mAuthNoPrint = mAuth.AuthNoPrint;
                mAuthReq = mAuth.AuthReq;
            }
            // lock if Authorised
            if (Model.ChangeLog != "Add" && mAuthReq == true && mauthorise.Substring(0, 1) == "A" && mAuthLock)
            {
                Model.Mode = "View";
            }
            return View(Model);
        }

        #region Get
        [HttpGet]
        public ActionResult GetAccountList(string term, string BaseGr)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.Master.Select(m => new
                {
                    m.Code,
                    m.Name
                }).OrderBy(n => n.Name).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.Master.Where(x => x.Name.Contains(term) && x.Hide == false).Select(m => new
                {
                    m.Code,
                    m.Name
                }).OrderBy(n => n.Name).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetCashBankList(string term)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.Master.Where(x => (x.BaseGr == "B" || x.BaseGr == "C") && x.Hide == false).Select(c => new
                {
                    c.Code,
                    c.Name
                }).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.Master.Where(x => x.Name.Contains(term) && x.Hide == false && (x.BaseGr == "B" || x.BaseGr == "C")).Select(m => new
                {
                    m.Code,
                    m.Name
                }).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetRefDoc(string term)
        {
            var mPara = term.Split('^');
            var mP1 = mPara[0].ToLower();
            var mP2 = mPara[1];
            if (term == "" || term == null)
            {
                var result = ctxTFAT.Ledger.Where(x => x.Branch == mbranchcode && x.Code == mP2 && x.SubType == "BR").Select(c => new
                {
                    Code = c.ParentKey,
                    Name = c.ParentKey
                }).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.Ledger.Where(x => x.Branch == mbranchcode && x.Code == mP2 && x.SubType == "BR").Select(m => new
                {
                    Code = m.ParentKey,
                    Name = m.ParentKey,
                }).Distinct().ToList();
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
            var warehouselist = ctxTFAT.Warehouse.Where(z => z.Branch == mbranchcode && (z.Users + ",").Contains(muserid + ",")).Select(b => new
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

        public ActionResult GetReferenceDetails(LedgerVM Model)
        {
            var ledger = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey && x.Code == Model.Code).Select(x => x).FirstOrDefault();
            Model.Cheque = ledger.Cheque;
            Model.StrChqDate = ledger.ChequeDate == null ? "01-01-1900" : ledger.ChequeDate.Value.ToString("dd-MM-yyyy");
            Model.Amt = ledger.Debit.Value + ledger.Credit.Value;
            Model.BankCashCode = ledger.AltCode;
            Model.BankCashName = NameofAccount(ledger.AltCode);
            return Json(Model, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region SAVE EDIT
        public ActionResult DeleteData(LedgerVM Model)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    DeUpdate(Model);
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, Model.ChangeLog, Model.Header, Model.ParentKey, DateTime.Now, 0, Model.ParentKey, "", "A");
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
            List<Authorisation> mDeleteAuth = ctxTFAT.Authorisation.Where(x => x.ParentKey == Model.ParentKey).ToList();
            if (mDeleteAuth != null) ctxTFAT.Authorisation.RemoveRange(mDeleteAuth);
            // remove all O/S adjustments
            List<Outstanding> outstanding = ctxTFAT.Outstanding.Where(x => x.ParentKey == Model.ParentKey).ToList();
            if (outstanding != null) ctxTFAT.Outstanding.RemoveRange(outstanding);
            // remove invoice adjusted against the original receipt
            List<Outstanding> outstanding1 = ctxTFAT.Outstanding.Where(x => x.ParentKey == Model.RefDoc).ToList();
            if (outstanding1 != null) ctxTFAT.Outstanding.RemoveRange(outstanding);

            List<Ledger> mDeleteLed = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey).ToList();
            if (mDeleteLed != null) ctxTFAT.Ledger.RemoveRange(mDeleteLed);
            List<Narration> mDeleteNote = ctxTFAT.Narration.Where(x => x.TableKey == Model.ParentKey).ToList();
            if (mDeleteNote != null) ctxTFAT.Narration.RemoveRange(mDeleteNote);
            List<Attachment> mDeleteAttach = ctxTFAT.Attachment.Where(x => x.ParentKey == Model.ParentKey).ToList();
            if (mDeleteAttach != null) ctxTFAT.Attachment.RemoveRange(mDeleteAttach);
            List<AddonDocCB> mDeleteaddon = ctxTFAT.AddonDocCB.Where(x => x.ParentKey == Model.ParentKey).ToList();
            if (mDeleteaddon != null) ctxTFAT.AddonDocCB.RemoveRange(mDeleteaddon);
            ctxTFAT.SaveChanges();
        }

        public ActionResult SaveData(LedgerVM Model)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    if (mauthorise.Substring(0, 1) == "R" || Model.ChangeLog == "Add" || (Model.ChangeLog == "Edit" && mAuthAgain))
                    {
                        if (mauthorise.Substring(0, 1) != "N") //if authorised then check for the RateDiff Auth. Rule
                            mauthorise = (mAuthReq == true ? GetAuthorise(Model.Type, 0, mbranchcode) : mauthorise = "A00");
                    }
                    if (Model.ChangeLog == "Edit")
                    {
                        Model.Srl = Model.Srl;
                        Model.ParentKey = Model.ParentKey;
                        DeUpdate(Model);
                    }
                    Model.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate == null || Model.DocuDate == "" ? "01-01-1900" : Model.DocuDate);
                    Model.StrBankDate = (Model.StrBankDate == null || Model.StrBankDate == "") ? "01-01-1900" : Model.StrBankDate;
                    if (Model.ChangeLog == "Add")
                    {
                        Model.Srl = GetLastSerial("Ledger", mbranchcode, Model.Type, Model.Prefix, Model.SubType, Model.DocDate);
                        Model.ParentKey = Model.Type + Model.Prefix.Substring(0, 2) + Model.Srl;
                    }
                    Model.Prefix = Model.Prefix;
                    for (int i = 1; i <= 6; i++)
                    {
                        if ((i == 3 || i == 4) && Model.BankCharges == 0)
                        {
                            goto mnext;
                        }
                        if ((i == 5 || i == 6) && Model.PartyCharges == 0)
                        {
                            goto mnext;
                        }
                        Ledger mobj1 = new Ledger();
                        mobj1.TableKey = Model.Type + Model.Prefix.Substring(0, 2) + i.ToString("D3") + Model.Srl;
                        mobj1.ParentKey = Model.ParentKey;
                        mobj1.Sno = i;
                        mobj1.Srl = Model.Srl;
                        mobj1.SubType = Model.SubType;
                        mobj1.Type = Model.Type;
                        mobj1.Debit = 0;
                        mobj1.Credit = 0;
                        switch (i)
                        {
                            case 1:
                                mobj1.Code = Model.Code;//Party
                                mobj1.Debit = Model.Amt;
                                mobj1.AltCode = Model.BankCashCode;
                                mobj1.DocDate = Model.DocDate;
                                break;
                            case 2:
                                mobj1.Code = Model.BankCashCode;//bank
                                mobj1.Credit = Model.Amt;
                                mobj1.AltCode = Model.Code;
                                mobj1.DocDate = Model.DocDate;
                                break;
                            case 3:
                                mobj1.Code = Model.BankChgAcc;//bank chgs
                                mobj1.Debit = Model.BankCharges;
                                mobj1.AltCode = Model.BankCashCode;
                                mobj1.DocDate = ConvertDDMMYYTOYYMMDD(Model.StrBankDate);
                                break;
                            case 4:
                                mobj1.Code = Model.BankCashCode;//bank
                                mobj1.Credit = Model.BankCharges;
                                mobj1.AltCode = Model.BankChgAcc;
                                mobj1.DocDate = ConvertDDMMYYTOYYMMDD(Model.StrBankDate);
                                break;
                            case 5:
                                mobj1.Code = Model.Code;//Party
                                mobj1.Debit = Model.PartyCharges;
                                mobj1.AltCode = Model.BankChgAcc;
                                mobj1.DocDate = ConvertDDMMYYTOYYMMDD(Model.StrBankDate);
                                break;
                            case 6:
                                mobj1.Code = Model.BankChgAcc;//Customer chgs
                                mobj1.Credit = Model.PartyCharges;
                                mobj1.AltCode = Model.Code;
                                mobj1.DocDate = ConvertDDMMYYTOYYMMDD(Model.StrBankDate);
                                break;
                        }
                        mobj1.Party = Model.Code;
                        mobj1.Audited = false;
                        mobj1.AUTHIDS = muserid;
                        mobj1.AUTHORISE = mauthorise;
                        mobj1.BillDate = DateTime.Now;
                        mobj1.BillNumber = "";
                        mobj1.Branch = mbranchcode;
                        mobj1.CompCode = mcompcode;
                        mobj1.Cheque = Model.Cheque != null ? Model.Cheque : "";
                        mobj1.ChequeReturn = false;
                        mobj1.ClearDate = Convert.ToDateTime("01-01-1950");
                        mobj1.ChequeDate = (Model.StrChqDate == null || Model.StrChqDate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrChqDate);
                        mobj1.CrPeriod = 0;
                        mobj1.CurrName = (Model.CurrCode == null || Model.CurrCode == "") ? 0 : Convert.ToInt32(Model.CurrCode);
                        mobj1.CurrRate = (Model.CurrRate == null) ? 0 : Model.CurrRate;
                        mobj1.Discounted = true;
                        mobj1.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                        mobj1.MainType = Model.MainType;
                        mobj1.Narr = Model.Narr != null ? Model.Narr : "";
                        mobj1.Prefix = Model.Prefix;
                        mobj1.ProjCode = null;
                        //mobj1.ProjectStage = null;
                        //mobj1.ProjectUnit = null;
                        mobj1.RecoFlag = "";
                        mobj1.RefDoc = Model.RefDoc;
                        mobj1.TDSChallanNumber = "";
                        mobj1.TDSCode = Model.TDSCode;
                        mobj1.TDSFlag = Model.TDSFlag;
                        mobj1.ENTEREDBY = muserid;
                        mobj1.DueDate = Convert.ToDateTime("1900-01-01");
                        mobj1.Reminder = false;
                        mobj1.TaskID = 0;
                        mobj1.ChqCategory = 0;
                        mobj1.CurrAmount = 0;
                        mobj1.LocationCode = Model.LocationCode;
                        mobj1.LASTUPDATEDATE = DateTime.Now;
                        mobj1.GSTType = 0;
                        mobj1.IGSTAmt = Convert.ToDecimal(0);
                        mobj1.IGSTRate = Convert.ToDecimal(0);
                        mobj1.CGSTAmt = Convert.ToDecimal(0);
                        mobj1.CGSTRate = Convert.ToDecimal(0);
                        mobj1.SGSTAmt = Convert.ToDecimal(0);
                        mobj1.SGSTRate = Convert.ToDecimal(0);
                        ctxTFAT.Ledger.Add(mobj1);
                    mnext:;
                    }
                    // remove all adjustment
                    List<Outstanding> outstanding = ctxTFAT.Outstanding.Where(x => x.ParentKey == Model.RefDoc).Select(x => x).ToList();
                    if (outstanding != null) ctxTFAT.Outstanding.RemoveRange(outstanding);

                    // adjust returned doc against the original receipt
                    int masno = (int)FieldoftableNumber("Ledger", "Sno", "ParentKey='" + Model.RefDoc + "' and Code='" + Model.Code + "' and Credit<>0");
                    string matype = Model.RefDoc.Substring(0, 5);
                    string maprefix = Model.RefDoc.Substring(5, 2);
                    string maserial = Model.RefDoc.Substring(7);
                    string masubtype = GetSubType(matype);
                    string mamaintype = GetMainType(matype, masubtype);
                    decimal maamt = Model.Amt;
                    Outstanding osobj1 = new Outstanding();
                    osobj1.Branch = mbranchcode;
                    osobj1.DocBranch = mbranchcode;
                    osobj1.DocBranch = mbranchcode;
                    osobj1.MainType = Model.MainType;
                    osobj1.SubType = Model.SubType;
                    osobj1.Type = Model.Type;
                    osobj1.Prefix = Model.Prefix;
                    osobj1.Srl = Model.Srl;
                    osobj1.Sno = 1;
                    osobj1.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                    osobj1.ParentKey = Model.ParentKey;
                    osobj1.TableKey = Model.Type + Model.Prefix.Substring(0, 2) + 1.ToString("D3") + Model.Srl;
                    osobj1.Code = Model.Code;
                    osobj1.aMaintype = mamaintype;
                    osobj1.aSubType = masubtype;
                    osobj1.aType = matype;
                    osobj1.aPrefix = maprefix;
                    osobj1.aSrl = maserial;
                    osobj1.aSno = masno;
                    osobj1.Amount = Convert.ToDecimal(maamt);
                    osobj1.TableRefKey = matype + maprefix.Substring(0, 2) + masno.ToString("D3") + maserial;
                    osobj1.AUTHIDS = muserid;
                    osobj1.AUTHORISE = Model.Authorise;
                    osobj1.BillDate = osobj1.DocDate;
                    osobj1.BillNumber = osobj1.Srl;
                    osobj1.CompCode = mcompcode;
                    osobj1.Broker = 100001;
                    osobj1.Brokerage = Convert.ToDecimal(0.00);
                    osobj1.BrokerAmt = Convert.ToDecimal(0.00);
                    osobj1.BrokerOn = Convert.ToDecimal(0.00);
                    osobj1.ChlnDate = DateTime.Now;
                    osobj1.ChlnNumber = "";
                    osobj1.CrPeriod = 0;
                    osobj1.CurrName = (Model.CurrCode == null || Model.CurrCode == "") ? 0 : Convert.ToInt32(Model.CurrCode);
                    osobj1.CurrRate = 1;
                    osobj1.Narr = "";
                    osobj1.OrdDate = DateTime.Now;
                    osobj1.OrdNumber = "";
                    osobj1.ProjCode = Model.ProjCode;
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
                    osobj1.CurrAmount = maamt;
                    osobj1.ValueDate = DateTime.Now;
                    osobj1.LocationCode = mlocationcode;
                    osobj1.AUTHORISE = mauthorise;
                    ctxTFAT.Outstanding.Add(osobj1);
                    // second effect
                    Outstanding osobj2 = new Outstanding();
                    osobj2.Branch = mbranchcode;
                    osobj2.DocBranch = mbranchcode;
                    osobj2.DocBranch = mbranchcode;
                    osobj2.MainType = mamaintype;
                    osobj2.SubType = masubtype;
                    osobj2.Type = matype;
                    osobj2.Prefix = maprefix;
                    osobj2.Srl = maserial;
                    osobj2.Sno = masno;
                    osobj2.ParentKey = Model.ParentKey;
                    osobj2.TableKey = matype + maprefix.Substring(0, 2) + masno.ToString("D3") + maserial;
                    osobj2.Code = Model.Code;
                    osobj2.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);

                    osobj2.aMaintype = Model.MainType;
                    osobj2.aSubType = Model.SubType;
                    osobj2.aType = Model.Type;
                    osobj2.aPrefix = Model.Prefix;
                    osobj2.aSrl = Model.Srl;
                    osobj2.aSno = 1;
                    osobj2.Amount = Convert.ToDecimal(maamt);
                    osobj2.TableRefKey = Model.Type + Model.Prefix.Substring(0, 2) + 1.ToString("D3") + Model.Srl;
                    osobj2.AUTHIDS = muserid;
                    osobj2.AUTHORISE = Model.Authorise;
                    osobj2.BillDate = osobj2.DocDate;
                    osobj2.BillNumber = osobj2.Srl;
                    osobj2.CompCode = mcompcode;
                    osobj2.Broker = 100001;
                    osobj2.Brokerage = Convert.ToDecimal(0.00);
                    osobj2.BrokerAmt = Convert.ToDecimal(0.00);
                    osobj2.BrokerOn = Convert.ToDecimal(0.00);
                    osobj2.ChlnDate = DateTime.Now;
                    osobj2.ChlnNumber = "";
                    osobj2.CrPeriod = 0;
                    osobj2.CurrName = (Model.CurrCode == null || Model.CurrCode == "") ? 0 : Convert.ToInt32(Model.CurrCode);
                    osobj2.CurrRate = 1;
                    osobj2.Narr = "";
                    osobj2.OrdDate = DateTime.Now;
                    osobj2.OrdNumber = "";
                    osobj2.ProjCode = Model.ProjCode;
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
                    osobj2.CurrAmount = maamt;
                    osobj2.ValueDate = DateTime.Now;
                    osobj2.LocationCode = mlocationcode;
                    osobj2.AUTHORISE = mauthorise;
                    ctxTFAT.Outstanding.Add(osobj2);
                    //
                    SaveAddons(Model);
                    SaveAttachment(Model);
                    ctxTFAT.SaveChanges();
                    // unauthorised then generate message
                    if (mauthorise.Substring(0, 1) != "A")
                    {
                        string mAuthUser;
                        if (mauthorise.Substring(0, 1) == "D")
                        {
                            //mAuthUser=SaveAuthoriseDOC( Model.ParentKey, 0, Model.Date, mcurrency, mbranchcode, muserid);
                        }
                        else
                        {
                            mAuthUser = SaveAuthorise(Model.ParentKey, 0, Model.DocuDate, mcurrency, 1, DateTime.Now, "", mbranchcode, muserid, -1);
                            //SendAuthoriseMessage(mAuthUser, Model.ParentKey, Model.Date);
                        }
                    }
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, Model.ChangeLog, Model.Header, Model.ParentKey, DateTime.Now, 0, Model.ParentKey, "", "A");
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
                t = t + 1;
                n = n + 1;
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
                t = t + 1;
                n = n + 1;
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
            var trnaddons = ctxTFAT.AddOns.Where(x => x.TableKey == "%Doc" + MainType && x.Hide == false && x.Types.Contains(Type)).Select(x => new { x.Eqsn, x.FldType, x.Fld }).ToList();
            for (int i = 0; i < trnaddons.Count; i++)
            {

                var Eqn = trnaddons[i].Eqsn == null ? "" : trnaddons[i].Eqsn.Trim();
                if (Eqn.Contains("%F"))
                {
                    for (int ai = 0; ai < trnaddons.Count; ai++)
                    {
                        j = trnaddons[ai].Fld;
                        if (LastModel[ai].FldType == "N")
                        {
                            Eqn = Eqn.Replace("%" + j, (LastModel[ai].ApplCode == "" || LastModel[ai].ApplCode == null) ? "0" : LastModel[ai].ApplCode);
                            if (!Eqn.Contains("%F"))
                            {
                                break;
                            }
                        }
                        else if (LastModel[ai].FldType == "T" || LastModel[ai].FldType == "M" || LastModel[ai].FldType == "C")
                        {
                            Eqn = Eqn.Replace("%" + j, (LastModel[ai].ApplCode == "" || LastModel[ai].ApplCode == null) ? "''" : LastModel[ai].ApplCode);
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
            ViewBag.ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
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

        #region attachment
        public ActionResult UploadFile()
        {
            ViewBag.ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
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
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/CashBank/AttachmentDocument.cshtml", new LedgerVM() { DocumentList = DocList, AllFileStr = XYZ, FileNameStr = FLN, ChangeLog = "Add" });
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
            mbranchcode = Document.Substring(0, 6).Trim();
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
                n = n + 1;

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



            var html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/CashBank/AttachmentDocument.cshtml", new LedgerVM() { DocumentList = DocList, AllFileStr = XYZ, FileNameStr = FLN, ChangeLog = "Edit" });
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

            var html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/CashBank/AttachmentDocument.cshtml", new LedgerVM() { DocumentList = Model.DocumentList, AllFileStr = XYZ, FileNameStr = FLN, ChangeLog = Model.ChangeLog });
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

                    System.IO.Directory.CreateDirectory(attachmentPath + Model.ParentKey);
                    string directoryPath = attachmentPath + Model.ParentKey + "/" + item.FileName;
                    byte[] IData = Convert.FromBase64String(item.ImageStr);
                    Attachment att = new Attachment();
                    att.AUTHIDS = muserid;
                    att.AUTHORISE = mauthorise;
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
                    an = an + 1;
                    c = c + 1;

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
                aip.LASTUPDATEDATE = DateTime.Now;
                aip.ParentKey = Model.ParentKey;
                aip.TableKey = Model.ParentKey;
                aip.Type = Model.Type;
                ctxTFAT.AddonDocCB.Add(aip);

            }
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
    }
}