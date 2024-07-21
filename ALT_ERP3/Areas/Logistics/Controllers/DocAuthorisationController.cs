using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class DocAuthorisationController : BaseController
    {
        // GET: Logistics/DocAuthorisation
        private string msubcodeof = "";
        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            UpdateAuditTrail(mbranchcode, "Auth", Model.Header, "", DateTime.Now, 0, "", "","A");
            GetAllMenu(Session["ModuleName"].ToString());
            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = DateTime.Today.ToShortDateString();
            ViewBag.ViewDataId = Model.ViewDataId;
            msubcodeof = Model.ViewDataId;
            Model.AccountName = NameofAccount(Model.Document);
            Model.ViewCode = GetDefaultCode(Model.ViewDataId);
            ViewBag.Header = Model.Header;
            ViewBag.Controller = Model.Controller;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;

            Model.mVar1 = muserid;
            Model.mVar2 = ctxTFAT.TfatPass.Where(x => x.Code == muserid).Select(x => x.Name).FirstOrDefault();
            if (muserid.ToString().ToUpper().Trim()=="SUPERXSA")
            {
                Model.ARAPReqOnly = true;
            }
            else
            {
                Model.ARAPReqOnly = false;
            }
            return View(Model);
        }

        public ActionResult GetDisplayType()
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "Authorised", Text = "Authorised" });
            GSt.Add(new SelectListItem { Value = "Un-Authorised", Text = "Un-Authorised" });
            GSt.Add(new SelectListItem { Value = "Rejected", Text = "Rejected" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTypes(string term)
        {
            List<TfatPass> tfatPasses = new List<TfatPass>();

            if (muserid=="Super")
            {
                tfatPasses.AddRange(ctxTFAT.TfatPass.ToList());
                if (!(String.IsNullOrEmpty(term)))
                {
                    tfatPasses = tfatPasses.Where(x => x.Name.ToLower().Contains(term.ToLower())).OrderBy(x => x.Name).ToList();
                }
            }
            else
            {
                TfatPass tfatPass = ctxTFAT.TfatPass.Where(x => x.Code == muserid).FirstOrDefault();
                tfatPasses.Add(tfatPass);
                if (!String.IsNullOrEmpty(tfatPass.UserList))
                {
                    var ListChildUser = tfatPass.UserList.Split(',').ToList();
                    var List = ctxTFAT.TfatPass.Where(x => ListChildUser.Contains(x.Code)).ToList();
                    tfatPasses.AddRange(List);
                }
                if (!(String.IsNullOrEmpty(term)))
                {
                    tfatPasses = tfatPasses.Where(x => x.Name.ToLower().Contains(term.ToLower())).OrderBy(x => x.Name).ToList();
                }
            }

            

            var Modified = tfatPasses.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetDocInformation(GridOption Model)
        {
            string mstr = "";
            string mparentkey = "";
            Authorisation mobj = null;
            if (Model.Document.StartsWith("@Acc"))
            {
                mparentkey = Model.Document.Substring(4);
                mobj = ctxTFAT.Authorisation.Where(z => z.Code == mparentkey && z.Branch == mbranchcode && z.AUTHIDS == muserid).Select(x => x).FirstOrDefault();
            }
            else if (Model.Document.StartsWith("@Item"))
            {
                mparentkey = Model.Document.Substring(5);
                mobj = ctxTFAT.Authorisation.Where(z => z.Code == mparentkey && z.Branch == mbranchcode && z.AUTHIDS == muserid).Select(x => x).FirstOrDefault();
            }
            else
            {
                mparentkey = Model.Document;
                mobj = ctxTFAT.Authorisation.Where(z => z.ParentKey == mparentkey && z.AuthLevel.Value.ToString() == Model.Level).Select(x => x).FirstOrDefault();
            }
            if (mobj != null)
            {
                mstr += mobj.Type + "|";
                mstr += mobj.Prefix + "|";
                mstr += mobj.Srl + "|";
                mstr += mobj.DocDate + "|";
                if (Model.Document.Contains("@LEEV"))
                {
                    mstr += NameofAccount(mobj.Code, "E") + "|";
                }
                else
                {
                    mstr += NameofAccount(mobj.Code, Model.Document.StartsWith("@Item") ? "I" : "A") + "|";
                }
                mstr += mobj.Amount + "|";
                mstr += mobj.ENTEREDBY + "|";
                mstr += mobj.EntryDate + "|";
                string mnarr = ctxTFAT.Narration.Where(z => z.ParentKey == mparentkey).Select(x => x.Narr).FirstOrDefault() ?? "";
                if (mnarr == "") mnarr = mobj.Narr;
                mstr += mnarr + "|";
                mstr += mobj.AuthLevel + "|";
                // who others to auth
                mstr += RecToString("Select Distinct AuthIDs from Authorisation Where Locked=0 And Branch='" + mbranchcode + "' And Parentkey='" + mparentkey + "' And (left(Authorise,1)='N' or left(Authorise,1)='D')", ", ") + "|";
                // remark by previous
                mstr += RecToString("Select AuthIDs+'-'+rtrim(Narr) from Authorisation Where AuthIDs<>'" + muserid + "' And Narr<>'' And Locked=0 And Branch='" + mbranchcode + "' And Parentkey='" + mparentkey + "' And (left(Authorise,1)='N' or left(Authorise,1)='D')", ", ") + "|";
                mstr += mobj.Narr + "|";    // your remark
            }
            else
            {
                mstr += "|";
                mstr += "|";
                mstr += "|";
                mstr += "|";
                mstr += "|";
                mstr += "|";
                mstr += "|";
                mstr += "|";
                mstr += "|";
                mstr += "|";
                mstr += "|";
                mstr += "|";
                mstr += "|";
            }
            return Json(mstr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            Model.ViewDataId = "Authorisation-" + (Model.MainType == "R" ? "RejectedList" : (Model.MainType == "U" ? "Un" : "") + "AuthorisedList");
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        public ActionResult GetGridData(GridOption Model)
        {
            Model.ViewDataId = "Authorisation-" + (Model.MainType == "R" ? "RejectedList" : (Model.MainType == "U" ? "Un" : "") + "AuthorisedList");
            return GetDocGridReport(Model, "R");
        }

        #region subreport
        private string GetFormatName(string mdocument)
        {
            var OldDocument = mdocument;
            mdocument = mdocument.Substring(6);
            string msub = "";
            if (mdocument.Length>=5)
            {
                msub = GetSubType(mdocument.Substring(0, 5));
            }
            
            string mmain = GetMainType(msub);
            string mformat = "";


            
                msub = mdocument.Substring(0, 2);
                if (msub.ToUpper() == "LR")
                {
                    mformat = "LRMaster";
                }
                else if (msub.ToUpper() == "LC")
                {
                    mformat = "LCMaster";
                }
                else if (msub.ToUpper() == "FM")
                {
                    mformat = "FMMaster";
                }
                else if (msub.ToUpper() == "PO")
                {
                    mformat = "POD";
                }
                else if (msub.ToUpper() == "DE")
                {
                    mformat = "Delivery";
                }
            if (String.IsNullOrEmpty(mformat))
            {
                if (msub == "ES" || msub == "EP")
                {
                    mformat = "Enquiry" + mmain.Substring(0, 1);
                }
                else if (msub == "QS" || msub == "QP")
                {
                    mformat = "Quote" + mmain.Substring(0, 1);
                }
                else if (msub == "OS" || msub == "OP")
                {
                    mformat = "Order" + mmain.Substring(0, 1);
                }
                else if (msub == "IP")
                {
                    mformat = "Indents";
                }
                else if (msub == "IA")
                {
                    mformat = "InventAdj";
                }
                else if (msub == "JE")
                {
                    mformat = "JobESTimation";
                }
                else if (msub == "JX")
                {
                    mformat = "JobExecution";
                }
                else if (msub == "JI")
                {
                    mformat = "JobIndents";
                }
                else if (msub == "JI")
                {
                    mformat = "JobInvoice";
                }
                else if (msub == "JO")
                {
                    mformat = "JobOpening";
                }
                else if (msub == "JS")
                {
                    mformat = "JobSheets";
                }
                else if (mmain == "RC" || mmain == "PM" || mmain == "JV" || mmain == "MV" || mmain == "PV")
                {
                    mformat = "Ledger";
                }
                else if (msub == "MP")
                {
                    mformat = "MfgDirect";
                }
                else if (msub == "PK")
                {
                    mformat = "PackingSlip";
                }
                else if (msub == "PS")
                {
                    mformat = "PhysicalStock";
                }
                else if (msub == "PL")
                {
                    mformat = "PickList";
                }
                else if (msub == "PI")
                {
                    mformat = "pInvoiceS";
                }
                else if (msub == "PL")
                {
                    mformat = "PlanOrders";
                }
                else if (msub == "PP")
                {
                    mformat = "Production";
                }
                else if (mmain == "PR")
                {
                    mformat = "Ledger";
                }
                else if (msub == "PU")
                {
                    mformat = "Putaway";
                }
                else if (msub == "MR")
                {
                    mformat = "Requisition";
                }
                else if (mmain == "SL")
                {
                    mformat = "Sales";
                }
                else if (msub == "GT")
                {
                    mformat = "StoreTransfer";
                }
                else if (msub == "FS")
                {
                    mformat = "SubContractFS";
                }
                else if (msub == "IS")
                {
                    mformat = "SubContractIS";
                }
                else if (msub == "WO")
                {
                    mformat = "WorkOrder";
                }
            }
            


            return mformat;
        }

        [HttpPost]
        public ActionResult GetGridStructureSub(GridOption Model)
        {
            if (!Model.Document.StartsWith("@Acc") && !Model.Document.StartsWith("@Item"))
            {
                var GetName = GetFormatName(Model.Document);
                if (String.IsNullOrEmpty(GetName))
                {
                    return null;
                }
                else
                {
                    Model.ViewDataId = "SubGrid-" + GetName;
                    return GetGridDataColumns(Model.ViewDataId, "X", "");
                }
            }
            else
            {
                return null;
            }
        }

        [HttpPost]
        public ActionResult GetGridDataSub(GridOption Model)
        {
            Model.ViewDataId = "SubGrid-" + GetFormatName(Model.Document);
            return GetGridReport(Model, "R", "Document^" + (Model.Document ?? ""), false, 0);
            return GetGridReport(Model, "R", "Document^" + (Model.Document.Substring(6) ?? ""), false, 0);
        }
        #endregion subreport

        [HttpPost]
        public ActionResult UpdateAuthorise(GridOption Model)//string mdocument, string mnarr, string mparty, string maction)
        {
            if (Model.list!=null)
            {
                foreach (var getItem in Model.list)
                {
                    string mdocument = getItem.Document;
                    string mnarr = getItem.Narr;
                    string mparty = getItem.Party;
                    string maction = getItem.AcType;
                    string muser = muserid;
                    if (getItem.mVar1 != null && getItem.mVar1 != "")
                    {
                        muser = getItem.mVar1;
                    }

                    using (var transaction = ctxTFAT.Database.BeginTransaction())
                    {
                        try
                        {
                            decimal mAmt = 0;
                            DateTime mDate = DateTime.Now;
                            DateTime mEntryDate = DateTime.Now;
                            string mEnteredBy = muser;
                            if (mdocument.Contains("@LEEV"))// leave application approval
                            {
                                mdocument = mdocument.Substring(6);
                                string mParentKey = mdocument;
                                string mType = mdocument.Substring(0, 5);
                                if (maction == "A")
                                {
                                    int mLevel = (int)ctxTFAT.Authorisation.Where(z => z.ParentKey == mdocument && z.Branch == mbranchcode && z.AuthBy == "" && z.Locked == false && z.AUTHIDS == muser && (z.AUTHORISE.StartsWith("N") || z.AUTHORISE.StartsWith("D"))).Select(x => x.AuthLevel).FirstOrDefault();
                                    var indentAuth = ctxTFAT.Authorisation.Where(x => x.ParentKey == mdocument && x.Branch == mbranchcode && x.Locked == false && x.AuthBy == "" && (x.AUTHORISE.Substring(0, 1) == "N" || x.AUTHORISE.Substring(0, 1) == "D") && x.AuthLevel == mLevel).Select(x => x).ToList();
                                    foreach (Authorisation indaut in indentAuth)
                                    {
                                        indaut.AuthoriseDate = DateTime.Now;
                                        indaut.LASTUPDATEDATE = DateTime.Now;
                                        indaut.AUTHORISE = maction;
                                        indaut.AuthBy = muser;
                                        indaut.Narr = mnarr;
                                        mAmt = (decimal)indaut.Amount;
                                        mDate = indaut.DocDate;
                                        //if (mparty == null)
                                        //{
                                        mparty = indaut.Code ?? mparty;
                                        //if (mparty.Length > 9)
                                        //{
                                        //    mparty = Fieldoftable("Employee", "Empid", "Name='" + mparty + "'");
                                        //}
                                        //}
                                        //else
                                        //{
                                        //mparty = Fieldoftable("Employee", "Empid", "Name='" + mparty + "'");
                                        //}
                                        mEnteredBy = indaut.ENTEREDBY;
                                        mEntryDate = (DateTime)indaut.EntryDate;
                                        ctxTFAT.Entry(indaut).State = EntityState.Modified;
                                    }
                                    UpdateAuditTrail(mbranchcode, "Auth", "Leave Application Approval", mParentKey, DateTime.Now, 0, mparty, "Leave Approved by " + muser + ", Remark: " + mnarr, "A");
                                    ExecuteStoredProc("Update LeaveApplication Set Authorise='A00' Where Srl='" + mdocument.Substring(7) + "'");
                                    mparty = Fieldoftable("Authorisation", "Code", "ParentKey='" + mdocument + "' and Branch='" + mbranchcode + "'");
                                    string memail = Fieldoftable("EmpAddress", "Email", "EmpID='" + mparty + "'");
                                    SendEMail(memail, "Leave Application Approved", "Dear " + NameofAccount(mparty, "E") + ",\nYour following Leave application is Sanctioned.\n" + mnarr, false, "", "", mParentKey, mparty);
                                    //string memail = Fieldoftable("EmpAddress", "Mobile", "EmpID='" + mparty + "'");
                                    //SendSMS(memail, "Leave Application Approved", "Dear " + NameofAccount(mparty, "E") + ",\nYour following Leave application is Sanctioned.\n" + mnarr, false, "", "", mParentKey, mparty);
                                }
                                else if (maction == "N" || maction == "R")
                                {
                                    mparty = Fieldoftable("Authorisation", "Code", "ParentKey='" + mdocument + "' and Branch='" + mbranchcode + "'");
                                    UpdateAuditTrail(mbranchcode, "Auth", "Leave Application Approval", mParentKey, DateTime.Now, mAmt, mparty, "Leave Rejected by " + muser + ", Remark: " + mnarr, "A");
                                    ExecuteStoredProc("Update Authorisation Set FinishBy='',LastUpdateDate=Getdate(),Authorise='" + maction + "',AuthBy='' Where ParentKey='" + mParentKey + "' ");//And Branch='" + mbranchcode + "'And AuthIDs='" + muser + "' 
                                    ExecuteStoredProc("Update LeaveApplication Set Authorise='" + maction + "00' Where Srl='" + mdocument.Substring(7) + "'");
                                    string memail = Fieldoftable("EmpAddress", "Email", "EmpID='" + mparty + "'");
                                    SendEMail(memail, "Leave Application Rejected", "Dear " + NameofAccount(mparty, "E") + ",\nYour following Leave application is Rejected.\n" + mnarr, false, "", "", mParentKey, mparty);
                                }
                            }
                            else if (mdocument.StartsWith("@Acc") || mdocument.StartsWith("@Item"))
                            {
                                string mType = mdocument.Substring(0, mdocument.StartsWith("@Item") ? 5 : 4);//Type
                                mdocument = mdocument.Substring(mdocument.StartsWith("@Item") ? 5 : 4);//TableKey
                                string mParentKey = mdocument;//TableKey
                                if (maction == "A")
                                {
                                    int mLevel = (int)ctxTFAT.Authorisation.Where(z => z.Code == mParentKey && z.Branch == mbranchcode && z.AuthBy == "" && z.Locked == false && z.AUTHIDS == muser && (z.AUTHORISE.StartsWith("N") || z.AUTHORISE.StartsWith("D"))).Select(x => x.AuthLevel).FirstOrDefault();
                                    var indentAuth = ctxTFAT.Authorisation.Where(x => x.Code == mParentKey && x.Branch == mbranchcode && x.AUTHIDS == muser && x.Locked == false && x.AuthBy == "" && (x.AUTHORISE.Substring(0, 1) == "N" || x.AUTHORISE.Substring(0, 1) == "D") && x.AuthLevel == mLevel).Select(x => x).ToList();
                                    //UpdateAuditTrail "Authorisation-Dashboard", mType, oPrefix, oSerial, CtoD(txtDate.Text), 0, "A", "", mBranch, , "Update Authorisation Set FinishDate=" & MmDdYy(CtoD(txtDate.Text)) & ",FinishBy='" + cmbUser.Text + "',Narr='" & txtNarr.Text + "', AuthoriseDate=GetDate(),LastUpdateDate=Getdate(),Authorise='A',AuthBy='" + gsUserId + "' Where Locked=0 And AuthBy='' And Branch='" + mBranch + "' And Type='" + mType + "' And Prefix='" + oPrefix + "' And Srl='" + oSerial + "' And AuthLevel=" & mLevel & " And Process='" + mProcess + "' And (left(Authorise,1)='N' or left(Authorise,1)='D')"
                                    foreach (var indaut in indentAuth)
                                    {
                                        indaut.AuthoriseDate = DateTime.Now;
                                        indaut.LASTUPDATEDATE = DateTime.Now;
                                        indaut.AUTHORISE = maction;
                                        indaut.AuthBy = muser;
                                        indaut.Narr = mnarr;
                                        mAmt = (decimal)indaut.Amount;
                                        mDate = indaut.DocDate;
                                        //if (mparty == null)
                                        //{
                                        mparty = indaut.Code ?? mparty;
                                        if (mdocument.StartsWith("@Acc") && mparty.Length > 9)
                                        {
                                            mparty = Fieldoftable("Master", "Code", "Name='" + mparty + "'");
                                        }
                                        //}
                                        //else
                                        //{
                                        //mparty = Fieldoftable(mdocument.StartsWith("@Acc") ? "Master" : "ItemMaster", "Code", "Name='" + mparty + "'");
                                        //}
                                        mEnteredBy = indaut.ENTEREDBY;
                                        mEntryDate = (DateTime)indaut.EntryDate;
                                        ctxTFAT.Entry(indaut).State = EntityState.Modified;
                                    }

                                    int mNextLevel = (int)FieldoftableNumber("TfatUserAudit", "UserLevel", "UserLevel>" + mLevel + " And Type='" + mType + "' and (SancLimit = 0 or SancLimit>=" + mAmt + ")");
                                    //ctxTFAT.TfatUserAudit.Where(z => z.UserLevel > mLevel && z.Type == mType && (z.SancLimit == 0 || z.SancLimit >= mAmt)).Select(x => x.UserLevel).FirstOrDefault();
                                    if (mNextLevel != 0)
                                    {
                                        var mmusers = SaveAuthorise(mType + mParentKey.PadRight(17, ' '), mAmt, mDate.ToString(), 1, 1, mEntryDate, mparty, mbranchcode, mEnteredBy, mNextLevel);
                                        UpdateAuditTrail(mbranchcode, "Auth", "Transaction Authorisation", mParentKey, DateTime.Now, mAmt, mparty, "Document Authorised by " + muser + " and Escalated to Next level: " + mNextLevel + " to Users: " + mmusers + ", Remark: " + mnarr, "A");
                                        SendAuthoriseMessage(mmusers, mParentKey, mDate.ToString(), "A00", mparty);
                                    }
                                    else
                                    {
                                        UpdateAuditTrail(mbranchcode, "Auth", "Transaction Authorisation", mParentKey, DateTime.Now, mAmt, mparty, "Document Authorised by " + muser + ", Remark: " + mnarr, "A");
                                        UpdateMasAuth(mType, "A00", mParentKey);
                                        //ExecuteStoredProc("SPTFAT_SetAuthorisation '" + "@Acc " + mParentKey.PadRight(17, ' ') + "','" + muser + "'," + "'A00'");
                                    }
                                }
                                else if (maction == "N")
                                {
                                    UpdateAuditTrail(mbranchcode, "Auth", "Transaction Authorisation", mParentKey, DateTime.Now, mAmt, mparty, "Document Un-Authorised by " + muser + ", Remark: " + mnarr, "A");
                                    ExecuteStoredProc("Update Authorisation Set FinishBy='',LastUpdateDate=Getdate(),Authorise='N',AuthBy='' Where Type='" + mType + "' and Code='" + mParentKey + "' And Locked=0 And left(Authorise,1)='A'");//And Branch='" + mbranchcode + "' And AuthIDs='" + muser + "' And AuthBy='" + muser + "' 
                                    UpdateMasAuth(mType, "N00", mParentKey);
                                    //ExecuteStoredProc("SPTFAT_SetAuthorisation '" + mParentKey + "','" + muser + "'," + "'N00'");
                                }
                                else if (maction == "R")
                                {
                                    UpdateAuditTrail(mbranchcode, "Auth", "Transaction Authorisation", mParentKey, DateTime.Now, mAmt, mparty, "Document Rejected by " + muser + ", Remark: " + mnarr, "A");
                                    int mLevel = (int)ctxTFAT.Authorisation.Where(z => z.ParentKey == mParentKey && z.Branch == mbranchcode && z.AuthBy == "" && z.Locked == false && z.AUTHIDS == muser && (z.AUTHORISE.StartsWith("N") || z.AUTHORISE.StartsWith("D"))).Select(x => x.AuthLevel).FirstOrDefault();
                                    string menteredy = ctxTFAT.Authorisation.Where(z => z.ParentKey == mParentKey && z.Branch == mbranchcode && z.AuthBy == "" && z.Locked == false && z.AUTHIDS == muser && (z.AUTHORISE.StartsWith("N") || z.AUTHORISE.StartsWith("D"))).Select(x => x.ENTEREDBY).FirstOrDefault() ?? "";
                                    var mDeleteAuthorise = ctxTFAT.Authorisation.Where(x => x.ParentKey == mParentKey && x.Branch == mbranchcode && x.AUTHIDS == muser && x.Locked == false && x.AuthBy == "" && (x.AUTHORISE.Substring(0, 1) == "N" || x.AUTHORISE.Substring(0, 1) == "D") && x.AuthLevel == mLevel).ToList();
                                    if (mDeleteAuthorise != null)
                                    {
                                        ctxTFAT.Authorisation.RemoveRange(mDeleteAuthorise);
                                    }
                                    ExecuteStoredProc("Update Authorisation Set FinishBy='',LastUpdateDate=Getdate(),Authorise='N',AuthBy='' Where Type='" + mType + "' and Code='" + mParentKey + "' And Locked=0 And AuthBy='" + muser + "' And AuthIDs='" + muser + "' And left(Authorise,1)='A'");//And Branch='" + mbranchcode + "' 
                                    UpdateMasAuth(mType, "R00", mParentKey);
                                    SendAuthoriseMessage(menteredy, mParentKey, DateTime.Now.Date.ToString(), "R00", mparty);
                                }
                            }
                            else if ((getItem.Type == "FMH00") || (getItem.Type == "FMP00") || (getItem.Type == "Trip0") || (getItem.Type == "COT00") || (getItem.Type == "CPO00") || (getItem.Type == "SLR00") || (getItem.Type == "PUR00") || (getItem.Type == "BRC00") || (getItem.Type == "BPM00") || (getItem.Type == "SLW00") || (getItem.Type == "LR000") || (getItem.Type == "LC000") || (getItem.Type == "FM000") || (getItem.Type == "POD00") || (getItem.Type == "DELV0"))
                            {
                                string MainBranch = "";
                                string mParentKey = mdocument;
                                string mType = getItem.Type;
                                if (maction == "A")
                                {
                                    int mLevel = Convert.ToInt32(getItem.Level);
                                    var indaut = ctxTFAT.Authorisation.Where(x => x.ParentKey == mdocument && x.Locked == false && x.AuthLevel == mLevel).Select(x => x).FirstOrDefault();
                                    MainBranch = indaut.Branch;
                                    indaut.AuthoriseDate = DateTime.Now;
                                    indaut.LASTUPDATEDATE = DateTime.Now;
                                    indaut.AUTHORISE = maction;
                                    indaut.AuthBy = muser;
                                    indaut.Narr = mnarr;
                                    mAmt = (decimal)indaut.Amount;
                                    mDate = indaut.DocDate;
                                    mparty = indaut.Code ?? mparty;
                                    if (mparty.Length > 9)
                                    {
                                        mparty = Fieldoftable("Master", "Code", "Name='" + mparty + "'");
                                    }
                                    mEnteredBy = indaut.ENTEREDBY;
                                    mEntryDate = (DateTime)indaut.EntryDate;
                                    ctxTFAT.Entry(indaut).State = EntityState.Modified;
                                    DocAuthorisationNotification(indaut);

                                    #region Get Authenticate Or Not Next Level
                                    var GetRule = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == getItem.Type).FirstOrDefault();
                                    List<string> Child = GetChildGrp(indaut.Branch);
                                    bool Authenticate = false;
                                    List<string> AuthBranchList = new List<string>();
                                    if (GetRule != null)
                                    {
                                        if (!String.IsNullOrEmpty(GetRule.AuthReqBranch))
                                        {
                                            AuthBranchList = GetRule.AuthReqBranch.Split(',').ToList();
                                        }

                                    }
                                    var CommonList = Child.Intersect(AuthBranchList);
                                    if (CommonList.Count() > 0)
                                    {
                                        Authenticate = true;
                                    }
                                    #endregion

                                    int mNextLevel = 0; var UserID = "";
                                    if (Authenticate)
                                    {
                                        int NextLevel = Convert.ToInt32(getItem.Level);
                                        List<int> TotalLevel = ctxTFAT.TfatUserAudit.Where(x => x.UserLevel > NextLevel && x.Type == getItem.Type).Select(x => x.UserLevel).Distinct().ToList();
                                        foreach (var level in TotalLevel)
                                        {
                                            var GetUserAudit = ctxTFAT.TfatUserAudit.Where(x => x.UserLevel == level && x.Type == getItem.Type).ToList();
                                            foreach (var item in GetUserAudit)
                                            {
                                                if (String.IsNullOrEmpty(item.AppBranch))
                                                {
                                                    UserID += item.UserID + "^";
                                                }
                                                else
                                                {
                                                    AuthBranchList = item.AppBranch.Split(',').ToList();
                                                    CommonList = Child.Intersect(AuthBranchList);
                                                    if (CommonList.Count() > 0)
                                                    {
                                                        UserID += item.UserID + "^";
                                                    }
                                                }
                                            }
                                            if (!String.IsNullOrEmpty(UserID))
                                            {
                                                UserID = UserID.Substring(0, UserID.Length - 1);
                                                mNextLevel = level;
                                                break;
                                            }
                                        }
                                    }
                                    if (mNextLevel != 0)
                                    {
                                        //var mmusers = FirstSaveAuthorise(Model.Type, Model.Serial, mParentKey, mAmt, mDate.ToString(), 1, 1, mEntryDate, mparty, mbranchcode, mEnteredBy, mNextLevel);
                                        //var mmusers = Authorised(MainBranch, "LR", 0, 0, UserID, lRMaster.LrNo.ToString(), DateTime.Now.ToString(), lRMaster.Val1, lRMaster.BillParty, lRMaster.TableKey, lRMaster.BookDate);
                                        var mmusers = Authorised(MainBranch, getItem.Type, 0, mNextLevel, UserID, getItem.Serial, mDate.ToString(), mAmt, mparty, mParentKey, mEntryDate);
                                        UpdateAuditTrail(mbranchcode, "Auth", "Transaction Authorisation", mParentKey, DateTime.Now, mAmt, mparty, "Document Authorised by " + muser + " and Escalated to Next level: " + mNextLevel + " to Users: " + mmusers + ", Remark: " + mnarr, "A");
                                        var Authorise = "N0" + getItem.Level;
                                        ExecuteStoredProc("SPTFAT_SetAuthorisationNew '" + mParentKey + "','" + muser + "','" + Authorise + "','" + mType + "'");
                                        SendAuthoriseMessage(UserID, mParentKey, mDate.ToString(), Authorise, mparty);
                                    }
                                    else
                                    {
                                        UpdateAuditTrail(mbranchcode, "Auth", "Transaction Authorisation", mParentKey, DateTime.Now, mAmt, mparty, "Document Authorised by " + muser + ", Remark: " + mnarr, "A");
                                        ExecuteStoredProc("SPTFAT_SetAuthorisationNew '" + mParentKey + "','" + muser + "'," + "'A00'" + ",'" + mType + "'");
                                    }
                                }
                                else if (maction == "N")
                                {
                                    int Level = Convert.ToInt32(getItem.Level);
                                    var GetNextLevelAuthorise = ctxTFAT.Authorisation.Where(x => x.Type == getItem.Type && x.Srl == getItem.Serial && x.AuthLevel > Level).OrderBy(x => x.AuthLevel).FirstOrDefault();
                                    if (GetNextLevelAuthorise != null)
                                    {
                                        if (GetNextLevelAuthorise.AUTHORISE.Trim().ToUpper().ToString().Substring(0, 1) != "N")
                                        {
                                            return Json(new
                                            {
                                                Status = "Success",
                                                Message = "Next Level Authorise Process Complete So U Cant Un-Authorise This Document.."
                                            }, JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {
                                            var Authorise = ctxTFAT.Authorisation.Where(x => x.Type == getItem.Type && x.Srl == getItem.Serial && x.AuthLevel > Level).OrderBy(x => x.AuthLevel).ToList();
                                            ctxTFAT.Authorisation.RemoveRange(Authorise);
                                        }
                                    }
                                    var Authorisel = "N0" + Level;
                                    UpdateAuditTrail(mbranchcode, "Auth", "Transaction Authorisation", mParentKey, DateTime.Now, mAmt, mparty, "Document Un-Authorised by " + muser + ", Remark: " + mnarr, "A");
                                    ExecuteStoredProc("Update Authorisation Set FinishBy='',LastUpdateDate=Getdate(),Authorise='" + Authorisel + "',AuthBy='' Where ParentKey='" + mParentKey + "' And AuthLevel=" + getItem.Level + " And Locked=0  And left(Authorise,1)='A'");//And AuthIDs='" + muser + "' 
                                    ExecuteStoredProc("SPTFAT_SetAuthorisationNew '" + mParentKey + "','" + muser + "','" + Authorisel + "','" + mType + "'");
                                }
                                else if (maction == "R")
                                {
                                    int Level = Convert.ToInt32(getItem.Level);
                                    var GetNextLevelAuthorise = ctxTFAT.Authorisation.Where(x => x.Type == getItem.Type && x.Srl == getItem.Serial && x.AuthLevel > Level).OrderBy(x => x.AuthLevel).FirstOrDefault();
                                    if (GetNextLevelAuthorise != null)
                                    {
                                        if (GetNextLevelAuthorise.AUTHORISE.Trim().ToUpper().ToString().Substring(0, 1) != "N")
                                        {
                                            return Json(new
                                            {
                                                Status = "Success",
                                                Message = "Next Level Authorise Process Complete So U Cant Reject This Document.."
                                            }, JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {
                                            var Authorise = ctxTFAT.Authorisation.Where(x => x.Type == getItem.Type && x.Srl == getItem.Serial && x.AuthLevel > Level).OrderBy(x => x.AuthLevel).ToList();
                                            ctxTFAT.Authorisation.RemoveRange(Authorise);
                                        }
                                    }

                                    var mDeleteAuthorise = ctxTFAT.Authorisation.Where(x => x.Type == getItem.Type && x.Srl == getItem.Serial && x.AuthLevel == Level).FirstOrDefault();
                                    if (mDeleteAuthorise != null)
                                    {
                                        mDeleteAuthorise.AuthoriseDate = DateTime.Now;
                                        mDeleteAuthorise.LASTUPDATEDATE = DateTime.Now;
                                        mDeleteAuthorise.AUTHORISE = maction;
                                        mDeleteAuthorise.AuthBy = muser;
                                        mDeleteAuthorise.Narr = mnarr;
                                        mAmt = (decimal)mDeleteAuthorise.Amount;
                                        mDate = mDeleteAuthorise.DocDate;
                                        mparty = mDeleteAuthorise.Code ?? mparty;
                                        if (mparty.Length > 9)
                                        {
                                            mparty = Fieldoftable("Master", "Code", "Name='" + mparty + "'");
                                        }
                                        mEnteredBy = mDeleteAuthorise.ENTEREDBY;
                                        mEntryDate = (DateTime)mDeleteAuthorise.EntryDate;
                                        ctxTFAT.Entry(mDeleteAuthorise).State = EntityState.Modified;
                                    }
                                    var Authorisel = "R0" + Level;
                                    UpdateAuditTrail(mbranchcode, "Auth", "Transaction Authorisation", mParentKey, DateTime.Now, mAmt, mparty, "Document Rejected by " + muser + ", Remark: " + mnarr, "A");
                                    ExecuteStoredProc("SPTFAT_SetAuthorisationNew '" + mParentKey + "','" + muser + "','" + Authorisel + "','" + mType + "'");
                                }
                            }
                            else
                            {
                                mdocument = mdocument;
                                string mParentKey = mdocument;
                                string mType = mdocument.Substring(0, 5);
                                if (maction == "A")
                                {
                                    int mLevel = (int)ctxTFAT.Authorisation.Where(z => z.ParentKey == mdocument && z.AuthBy == "" && z.Locked == false && (z.AUTHORISE.StartsWith("N") || z.AUTHORISE.StartsWith("D"))).Select(x => x.AuthLevel).FirstOrDefault();
                                    var indentAuth = ctxTFAT.Authorisation.Where(x => x.ParentKey == mdocument && x.Locked == false && x.AuthBy == "" && (x.AUTHORISE.Substring(0, 1) == "N" || x.AUTHORISE.Substring(0, 1) == "D") && x.AuthLevel == mLevel).Select(x => x).ToList();
                                    foreach (Authorisation indaut in indentAuth)
                                    {
                                        indaut.AuthoriseDate = DateTime.Now;
                                        indaut.LASTUPDATEDATE = DateTime.Now;
                                        indaut.AUTHORISE = maction;
                                        indaut.AuthBy = muser;
                                        indaut.Narr = mnarr;
                                        mAmt = (decimal)indaut.Amount;
                                        mDate = indaut.DocDate;

                                        mparty = indaut.Code ?? mparty;
                                        if (mparty.Length > 9)
                                        {
                                            mparty = Fieldoftable("Master", "Code", "Name='" + mparty + "'");
                                        }
                                        mEnteredBy = indaut.ENTEREDBY;
                                        mEntryDate = (DateTime)indaut.EntryDate;
                                        ctxTFAT.Entry(indaut).State = EntityState.Modified;
                                    }

                                    int mNextLevel = (int)FieldoftableNumber("TfatUserAudit", "UserLevel", "UserLevel>" + mLevel + " And Type='" + mType + "' and (SancLimit = 0 or SancLimit>=" + mAmt + ")");
                                    if (mNextLevel != 0)
                                    {
                                        var mmusers = SaveAuthorise(mParentKey, mAmt, mDate.ToString(), 1, 1, mEntryDate, mparty, mbranchcode, mEnteredBy, mNextLevel);
                                        UpdateAuditTrail(mbranchcode, "Auth", "Transaction Authorisation", mParentKey, DateTime.Now, mAmt, mparty, "Document Authorised by " + muser + " and Escalated to Next level: " + mNextLevel + " to Users: " + mmusers + ", Remark: " + mnarr, "A");
                                        SendAuthoriseMessage(mmusers, mParentKey, mDate.ToString(), "A00", mparty);
                                    }
                                    else
                                    {
                                        var Authorise = "A00";
                                        UpdateAuditTrail(mbranchcode, "Auth", "Transaction Authorisation", mParentKey, DateTime.Now, mAmt, mparty, "Document Authorised by " + muser + ", Remark: " + mnarr, "A");
                                        //ExecuteStoredProc("SPTFAT_SetAuthorisationNew '" + mParentKey + "','" + muser + "','" + "'A00'" + "','" + mType + "'");
                                        ExecuteStoredProc("SPTFAT_SetAuthorisationNew '" + mParentKey + "','" + muser + "','" + Authorise + "','" + mType + "'");
                                    }
                                }
                                else if (maction == "N")
                                {
                                    var Authorise = "N00";
                                    UpdateAuditTrail(mbranchcode, "Auth", "Transaction Authorisation", mParentKey, DateTime.Now, mAmt, mparty, "Document Un-Authorised by " + muser + ", Remark: " + mnarr, "A");
                                    ExecuteStoredProc("Update Authorisation Set FinishBy='',LastUpdateDate=Getdate(),Authorise='N',AuthBy='' Where ParentKey='" + mParentKey + "' And Locked=0 And AuthBy='" + muser + "' And left(Authorise,1)='A'");//And Branch='" + mbranchcode + "' And AuthIDs='" + muser + "' 
                                                                                                                                                                                                                                                      //ExecuteStoredProc("SPTFAT_SetAuthorisationNew '" + mParentKey + "','" + muser + "'," + "'N00'" + "','" + mType + "'");
                                    ExecuteStoredProc("SPTFAT_SetAuthorisationNew '" + mParentKey + "','" + muser + "','" + Authorise + "','" + mType + "'");

                                }
                                else if (maction == "R")
                                {
                                    UpdateAuditTrail(mbranchcode, "Auth", "Transaction Authorisation", mParentKey, DateTime.Now, mAmt, mparty, "Document Rejected by " + muser + ", Remark: " + mnarr, "A");
                                    int mLevel = (int)ctxTFAT.Authorisation.Where(z => z.ParentKey == mParentKey).Select(x => x.AuthLevel).FirstOrDefault();
                                    string menteredy = ctxTFAT.Authorisation.Where(z => z.ParentKey == mParentKey).Select(x => x.ENTEREDBY).FirstOrDefault() ?? "";
                                    var mDeleteAuthorise = ctxTFAT.Authorisation.Where(x => x.ParentKey == mParentKey && x.AuthLevel == mLevel).ToList();
                                    if (mDeleteAuthorise != null)
                                    {
                                        foreach (var RejectItem in mDeleteAuthorise)
                                        {
                                            RejectItem.AuthoriseDate = DateTime.Now;
                                            RejectItem.LASTUPDATEDATE = DateTime.Now;
                                            RejectItem.AUTHORISE = maction;
                                            RejectItem.AuthBy = muser;
                                            RejectItem.Narr = mnarr;
                                            mAmt = (decimal)RejectItem.Amount;
                                            mDate = RejectItem.DocDate;

                                            mparty = RejectItem.Code ?? mparty;
                                            if (mparty.Length > 9)
                                            {
                                                mparty = Fieldoftable("Master", "Code", "Name='" + mparty + "'");
                                            }
                                            mEnteredBy = RejectItem.ENTEREDBY;
                                            mEntryDate = (DateTime)RejectItem.EntryDate;
                                            ctxTFAT.Entry(RejectItem).State = EntityState.Modified;
                                        }
                                    }
                                    //ExecuteStoredProc("Update Authorisation Set FinishBy='',LastUpdateDate=Getdate(),Authorise='R00',AuthBy='" + muser + "' Where ParentKey='" + mParentKey + "' And Locked=0 ");//And Branch='" + mbranchcode + "' And AuthIDs='" + muser + "' 

                                    //ExecuteStoredProc("SPTFAT_SetAuthorisationNew '" + mParentKey + "','" + muser + "'," + "'R00'" + "','" + mType + "'");
                                    var Authorise = "R00";
                                    ExecuteStoredProc("SPTFAT_SetAuthorisationNew '" + mParentKey + "','" + muser + "','" + Authorise + "','" + mType + "'");
                                    SendAuthoriseMessage(menteredy, mParentKey, DateTime.Now.Date.ToString(), "R00", mparty);
                                }
                            }
                            ctxTFAT.SaveChanges();
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

                }

            }

            return Json(new
            {
                Status = "Success",
                Message = "Document Status Updated.."
            }, JsonRequestBehavior.AllowGet);
        }

        private void UpdateMasAuth(string mtype, string mauthorise, string mParentKey)
        {
            if (mtype == "@Acc")
            {
                ExecuteStoredProc("Update Master set Authorise='" + mauthorise + "' where Code='" + mParentKey + "'");
                ExecuteStoredProc("Update Address set Authorise='" + mauthorise + "' where Code='" + mParentKey + "'");
                ExecuteStoredProc("Update MasterInfo set Authorise='" + mauthorise + "' where Code='" + mParentKey + "'");
                ExecuteStoredProc("Update Assets set Authorise='" + mauthorise + "' where Code='" + mParentKey + "'");
                ExecuteStoredProc("Update MasterSubLedger set Authorise='" + mauthorise + "' where Code='" + mParentKey + "'");
                ExecuteStoredProc("Update TaxDetails set Authorise='" + mauthorise + "' where Code='" + mParentKey + "'");
                ExecuteStoredProc("Update MasterTransporter set Authorise='" + mauthorise + "' where Code='" + mParentKey + "'");
                ExecuteStoredProc("Update HoldTransactions set Authorise='" + mauthorise + "' where Code='" + mParentKey + "'");
            }
            else
            {
                ExecuteStoredProc("Update ItemMaster set Authorise='" + mauthorise + "' where Code='" + mParentKey + "'");
                ExecuteStoredProc("Update ItemDetail set Authorise='" + mauthorise + "' where Code='" + mParentKey + "'");
                ExecuteStoredProc("Update ItemWarranty set Authorise='" + mauthorise + "' where Code='" + mParentKey + "'");
                ExecuteStoredProc("Update ItemMore set Authorise='" + mauthorise + "' where Code='" + mParentKey + "'");
            }
        }

        public Authorisation Authorised(string mbranch, string Type, decimal mCurrRate, int mLevel, string USerID, string DocNO, string mDocDate, decimal mTotAmt, string Party, string mParentKey, DateTime mEntryDate)
        {
            if (Type != null)
            {
                Type = Type.Trim();
            }
            if (mCurrRate == 0)
            {
                mCurrRate = 1;
            }

            Authorisation mObj = new Authorisation();
            mObj.Branch = mbranch;
            mObj.Type = Type;
            mObj.Prefix = mperiod ?? "";
            mObj.Srl = DocNO ?? "";
            mObj.DocDate = Convert.ToDateTime(mDocDate);
            mObj.Amount = mTotAmt;
            mObj.Code = Party ?? "";
            mObj.ParentKey = mParentKey ?? "";
            mObj.CompCode = "";
            mObj.TableKey = mParentKey ?? "";
            mObj.CurrName = 1;
            mObj.CurrRate = mCurrRate;
            mObj.AUTHIDS = USerID ?? "";
            mObj.AuthLevel = mLevel;
            mObj.AuthBy = "";
            mObj.AuthType = "T";
            mObj.AUTHORISE = "N00";
            mObj.ENTEREDBY = muserid ?? "";
            mObj.EntryDate = Convert.ToDateTime(mEntryDate);
            mObj.AuthTimeBound = false;
            mObj.AuthTimeLimit = 0;
            mObj.AuthDueDate = DateTime.Now.AddHours((double)0);
            mObj.SancLimit = 0;
            mObj.LASTUPDATEDATE = DateTime.Now;
            mObj.Narr = "";
            mObj.ProcessCode = 0;
            mObj.Locked = false;
            mObj.AuthCompulsary = false;

            ctxTFAT.Authorisation.Add(mObj);
            return mObj;
        }

        public ActionResult GetDocGridReport(GridOption Model, string mReportType = "R", string mParaString = "", bool mRunning = false, decimal mopening = 0, string mFilter = "", string mpapersize = "A4", string[] mparameters = null)
        {
            var User = muserid;
            if (!String.IsNullOrEmpty(Model.mVar3))
            {
                User = Model.mVar3;
            }
            if (Model.ARAPReqOnly)
            {
                User = "";
            }
            string connstring = GetConnectionString();
            string mFixedPara = "";
            if (Model.Para != null)
            {
                mFixedPara = Model.Para.ToString();
            }
            if (mFixedPara != "")
            {
                mFixedPara += "~";
            }
            mParaString = mFixedPara + mParaString;
            Model.searchField = Model.searchField == null || Model.searchField == "null" ? "" : Model.searchField;
            Model.searchString = Model.searchString ?? "";
            string mWhat = Model.mWhat ?? "";
            int startIndex = mWhat == "" ? (Model.page - 1) * Model.rows + 1 : -1;
            int endIndex = mWhat == "" ? (Model.page * Model.rows) : -1;

            SqlDataAdapter da = new SqlDataAdapter();
            using (DataTable dt = new DataTable())
            {
                SqlCommand cmd = new SqlCommand();

                if (Model.searchField != "" && Model.searchString != "" && mFilter == "")
                {
                    switch (Model.searchOper)
                    {
                        case "eq":
                            mFilter = Model.searchField + " = '" + Model.searchString + "'";
                            break;
                        case "ne":
                            mFilter = Model.searchField + " <> " + Model.searchString;
                            break;
                        case "bw":
                            mFilter = Model.searchField + " like '" + Model.searchString + "%'";
                            break;
                        case "bn":
                            mFilter = Model.searchField + " Not like '" + Model.searchString + "%'";
                            break;
                        case "ew":
                            mFilter = Model.searchField + " like '%" + Model.searchString + "'";
                            break;
                        case "en":
                            mFilter = Model.searchField + " Not like '%" + Model.searchString + "'";
                            break;
                        case "cn":
                            mFilter = Model.searchField + " like '%" + Model.searchString + "%'";
                            break;
                        case "in":
                            mFilter = Model.searchField + " like '%" + Model.searchString + "%'";
                            break;
                        case "nc":
                            mFilter = Model.searchField + " Not like '%" + Model.searchString + "%'";
                            break;
                        case "ni":
                            mFilter = Model.searchField + " Not like '%" + Model.searchString + "%'";
                            break;
                    }
                }

                try
                {
                    SqlConnection con = new SqlConnection(connstring);
                    cmd = new SqlCommand("dbo.ExecuteReport", con)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.Add("@mFormatCode", SqlDbType.VarChar).Value = Model.ViewDataId;
                    cmd.Parameters.Add("@mAlias", SqlDbType.VarChar).Value = (Model.searchtype ?? "").StartsWith("^S") ? "^" + Model.searchField : ""; // since currently not used, we use it for summarised report flag
                    cmd.Parameters.Add("@mCurrDec", SqlDbType.TinyInt).Value = 2;
                    cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
                    cmd.Parameters.Add("@mUserID", SqlDbType.VarChar).Value = User;
                    if (mReportType == "M")
                    {
                        Model.FromDate = Convert.ToDateTime(System.Web.HttpContext.Current.Session["StartDate"]).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                        Model.ToDate = Convert.ToDateTime(System.Web.HttpContext.Current.Session["LastDate"]).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        if (Model.Date != null && Model.Date != "undefined:undefined")
                        {
                            var date = Model.Date.Replace("-", "/").Split(':');
                            if (date[0] != "undefined")
                            {
                                Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                            }
                            if (date[1] != "undefined")
                            {
                                Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
                            }
                        }
                        else
                        {
                            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
                            Model.FromDate = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            Model.ToDate = (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                        }
                    }
                    cmd.Parameters.Add("@mReportStartDate", SqlDbType.VarChar).Value = Model.FromDate;
                    cmd.Parameters.Add("@mReportEndDate", SqlDbType.VarChar).Value = Model.ToDate;
                    cmd.Parameters.Add("@mIsRunBalance", SqlDbType.Bit).Value = false;// mRunning;
                    //cmd.Parameters.Add("@mOrderBy", SqlDbType.VarChar).Value = Model.sidx != null ? (Model.sidx.Replace(",", "") + ' ' + (Model.sidx.Contains("asc") || Model.sidx.Contains("desc") ? "" : Model.sord)) : "";
                    string mstrx = (Model.searchtype ?? "").StartsWith("^S") ? Model.searchField : (Model.sidx != null ? (Model.sidx + ' ' + (Model.sidx.Contains("asc") || Model.sidx.Contains("desc") ? "" : Model.sord)) : "");
                    mstrx = CutRightString(mstrx.Trim(), 1, ",");
                    cmd.Parameters.Add("@mOrderBy", SqlDbType.VarChar).Value = mstrx;// Model.sidx != null ? (Model.sidx + ' ' + (Model.sidx.Contains("asc") || Model.sidx.Contains("desc") ? "" : Model.sord)) : "";
                    cmd.Parameters.Add("@mStartIndex", SqlDbType.Int).Value = startIndex;
                    cmd.Parameters.Add("@mEndIndex", SqlDbType.Int).Value = endIndex;
                    cmd.Parameters.Add("@mRunBalance", SqlDbType.Decimal).Value = mRunning == true ? Model.Opening : 0;
                    cmd.Parameters.Add("@mInsertIntoTable", SqlDbType.VarChar).Value = "";// mRunning == true ? Model.ViewDataId : "";
                    cmd.Parameters.Add("@mPara", SqlDbType.VarChar).Value = mParaString;
                    cmd.Parameters.Add("@mFilter", SqlDbType.VarChar).Value = mFilter;
                    cmd.Parameters.Add("@mDocString", SqlDbType.VarChar).Value = mDocString;
                    // for output
                    cmd.Parameters.Add("@mSumString", SqlDbType.VarChar, 5000).Value = "";
                    cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
                    cmd.Parameters["@mSumString"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;
                    con.Open();
                    da.SelectCommand = cmd;
                    da.Fill(dt);
                    string mSumString = (string)(cmd.Parameters["@mSumString"].Value ?? "");
                    string mReturnString = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
                    con.Close();
                    con.Dispose();
                    // physical merge rows
                    var mvar = ctxTFAT.ReportHeader.Where(z => z.Code == Model.ViewDataId).Select(x => new { x.pMerge, x.pToMerge, x.pBlank }).FirstOrDefault();
                    string mpmerge = "";
                    string mptomerge = "";
                    if (mvar != null)
                    {
                        mpmerge = (mvar.pMerge ?? "").Trim();
                        mptomerge = (mvar.pToMerge ?? "").Trim();
                    }
                    if (mpmerge != "")
                    {
                        var marr = mpmerge.Split(',');
                        if (mptomerge.EndsWith(",") == false)
                        {
                            mptomerge += ",";
                        }

                        if (mptomerge.StartsWith(",") == false)
                        {
                            mptomerge = "," + mptomerge;
                        }
                        string mstr = "";
                        for (int n = 0; n <= dt.Rows.Count - 1; n++)
                        {
                            string mstr2 = "";
                            for (int m = 0; m <= marr.Count() - 1; m++)
                            {
                                if (marr[m] != "")
                                {
                                    mstr2 += dt.Rows[n][Convert.ToInt32(marr[m]) - 1];
                                }
                            }

                            if (mstr == mstr2)
                            {
                                for (int z = 0; z <= dt.Columns.Count - 1; z++)
                                {
                                    if (mptomerge.Contains("," + (z + 1).ToString() + ","))
                                    {
                                        if (dt.Columns[z].DataType == System.Type.GetType("System.Byte") || dt.Columns[z].DataType == System.Type.GetType("System.Decimal") || dt.Columns[z].DataType == System.Type.GetType("System.Double") || dt.Columns[z].DataType == System.Type.GetType("System.Int16") || dt.Columns[z].DataType == System.Type.GetType("System.Int32") || dt.Columns[z].DataType == System.Type.GetType("System.Int64") || dt.Columns[z].DataType == System.Type.GetType("System.Single"))
                                        {
                                            dt.Rows[n][z] = 0;
                                        }
                                        else
                                        {
                                            dt.Rows[n][z] = "";
                                        }
                                    }
                                }
                            }
                            mstr = mstr2;
                        }
                    }
                    // merge routine over

                    if (mRunning == true)
                    {
                        int mbalcol = -1;
                        int mruncol = -1;
                        int i;
                        for (i = 0; i < dt.Columns.Count; i++)
                        {
                            string mcolname = dt.Columns[i].ColumnName.Trim().ToLower();
                            if (mcolname == "balancefield")
                            {
                                mbalcol = i;
                            }
                            if (mcolname == "runningbalance" || mcolname == "balance")
                            {
                                mruncol = i;
                            }
                        }
                        if (mbalcol != -1 && mruncol != -1)
                        {
                            decimal mbal = mopening;
                            foreach (DataRow dr in dt.Rows)
                            {
                                mbal += (decimal)dr[mbalcol];
                                dr[mruncol] = mbal;
                            }
                        }
                    }

                    //string mSumJson = "";
                    StringBuilder jsonBuilder = new StringBuilder();
                    if ((mReportType == "R" || mReportType == "T") && dt.Rows.Count > 0)
                    {
                        try
                        {
                            DataTable msumdt = GetDataTable(@mSumString.Replace("[[", "[").Replace("]]", "]"), connstring);
                            //float[] marr = new float[dt.Columns.Count];
                            dt.Rows.Add();
                            if (msumdt.Rows.Count > 0)
                            {
                                int x = dt.Rows.Count;
                                for (int m = 0; m <= msumdt.Columns.Count - 1; m++)
                                {
                                    if (msumdt.Rows[0][m].ToString() == "")
                                    {
                                        dt.Rows[x - 1][m] = "";
                                    }
                                    else
                                    {
                                        try { dt.Rows[x - 1][m] = Convert.ToDecimal(msumdt.Rows[0][m]); }
                                        catch (Exception ex)
                                        {

                                        }
                                    }
                                }
                            }
                            msumdt.Dispose();
                        }
                        catch (Exception exx)
                        {
                        }
                    }
                    else
                    {
                        //jsonBuilder.Append("");
                    }

                    if (mReportType != "R" && mWhat != "PDF" && mWhat != "PDL" && mWhat != "XLS")
                    {
                        dt.Columns.Add("XYZ", typeof(string)).SetOrdinal(0);
                        dt.Columns.Add("ABC", typeof(string)).SetOrdinal(1);
                        dt.Columns.Add("FGH", typeof(string)).SetOrdinal(2);
                        dt.Columns.Add("PTG", typeof(string)).SetOrdinal(3);
                    }

                    if (mWhat == "")
                    {
                        // for count
                        SqlDataAdapter da2 = new SqlDataAdapter();
                        DataTable dt2 = new DataTable();
                        SqlCommand cmd2 = new SqlCommand();
                        SqlConnection con2 = new SqlConnection(connstring);
                        cmd2 = new SqlCommand("dbo.GetRowCount", con2)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmd2.Parameters.Add("@mFormatCode", SqlDbType.VarChar).Value = Model.ViewDataId;
                        cmd2.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
                        cmd2.Parameters.Add("@mUserID", SqlDbType.VarChar).Value = muserid;
                        cmd2.Parameters.Add("@mReportStartDate", SqlDbType.VarChar).Value = Model.FromDate;
                        cmd2.Parameters.Add("@mReportEndDate", SqlDbType.VarChar).Value = Model.ToDate;
                        cmd2.Parameters.Add("@mPara", SqlDbType.VarChar).Value = mParaString;
                        cmd2.Parameters.Add("@mFilter", SqlDbType.VarChar).Value = mFilter;
                        cmd2.Parameters.Add("@mDocString", SqlDbType.VarChar).Value = mDocString;
                        // for output
                        cmd2.Parameters.Add("@mRowCount", SqlDbType.Int).Value = 0;
                        cmd2.Parameters["@mRowCount"].Direction = ParameterDirection.Output;
                        con2.Open();
                        da2.SelectCommand = cmd2;
                        int mxRowCount = 0;
                        try { da2.Fill(dt2); mxRowCount = (int)cmd2.Parameters["@mRowCount"].Value; } catch (Exception e) { mxRowCount = Model.rows; }
                        cmd2.Dispose();
                        dt2.Dispose();
                        da2.Dispose();
                        con2.Close();
                        con2.Dispose();
                        return Content(JQGridHelper.JsonForJqgrid(dt, Model.rows, mxRowCount, Model.page, jsonBuilder.ToString()), "application/json");
                    }
                    else     // XLS or PDF
                    {
                        if (Model.mWhat == "XLS")
                        {
                            string attachment = "attachment; filename=" + Model.ViewDataId + @".xls";
                            Response.ClearContent();
                            Response.AddHeader("content-disposition", attachment);
                            Response.ContentType = "application/vnd.ms-excel";
                            var mWidths = (from xx in ctxTFAT.TfatSearch
                                           where xx.Code == Model.ViewDataId && xx.CalculatedCol != true
                                           orderby xx.Sno
                                           select new { xx.ColHead, ColWidth = (float)(xx.IsHidden == true ? 0 : xx.ColWidth) }).ToList();
                            float[] headerx = mWidths.Select(z => z.ColWidth).ToArray();
                            string tab = "";
                            string mHead = "";
                            DateTime mDate = Convert.ToDateTime(Model.ToDate);
                            int x = 0;
                            foreach (DataColumn dc in dt.Columns)
                            {
                                if (dc.ColumnName != "_Style" && headerx[x] > 5)
                                {
                                    mHead = mWidths[x].ColHead.Trim().Replace("##", "");
                                    if (mHead == "") mHead = dc.ColumnName;
                                    if (mHead.Contains("%"))
                                    {
                                        mHead = DocProcessReportHeader(mHead, mDate);
                                    }
                                    Response.Write(tab + mHead);//dc.ColumnName
                                    tab = "\t";
                                }
                                ++x;
                            }
                            Response.Write("\n");
                            x = 0;
                            foreach (DataRow dr in dt.Rows)
                            {
                                tab = "";
                                x = 0;
                                for (int i = 0; i < dt.Columns.Count; i++)
                                {
                                    if (dt.Columns[i].ColumnName != "_Style" && headerx[x] > 5)
                                    {
                                        Response.Write(tab + dr[i].ToString().Replace("\"", "").Replace(@"\", "").Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace("\b", ""));
                                        tab = "\t";
                                    }
                                    ++x;
                                }
                                Response.Write("\n");
                            }
                            Response.End();
                        }
                        else if (Model.mWhat == "PDF" || Model.mWhat == "PDL")
                        {
                            Model.AccountDescription = ctxTFAT.ReportHeader.Where(z => z.Code == Model.ViewDataId).Select(x => x.FormatHead).FirstOrDefault() ?? "";
                            if (Model.AccountDescription != "")
                            {
                                Model.AccountDescription = Model.AccountDescription.Replace("%RepStartDate", Model.FromDate);
                                Model.AccountDescription = Model.AccountDescription.Replace("%RepEndDate", Model.ToDate);
                            }
                            if (mparameters != null)
                            {
                                for (int xx = 0; xx <= 23; xx++)
                                {
                                    string mfld = "%para" + (xx + 1).ToString().PadLeft(2, '0');
                                    if (Model.AccountDescription.Contains(mfld))
                                    {
                                        Model.AccountDescription = Model.AccountDescription.Replace(mfld, mparameters[xx]);
                                    }
                                }
                            }
                            CreatePDF(Model, dt, Model.AccountDescription, Model.mWhat == "PDL" ? "Landscape" : "Portrait", mpapersize);
                        }
                        else if (Model.mWhat == "SRS")
                        {
                           
                        }
                        return null;
                    }
                }
                catch (Exception e)
                {
                    Session["ErrorMessage"] = e.Message;
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(new { Message = e.Message.Replace("'", "") });
                    //return Json(new { Message = e.Message, Status = "Error" }, JsonRequestBehavior.AllowGet);
                }
                finally
                {
                    cmd.Dispose();
                    da.Dispose();
                }
            }
        }

        public string DocProcessReportHeader(string mHead, DateTime mDate)
        {
            if (mHead.Contains("%MonthYear"))
            {
                mHead = mHead.Replace("%MonthYear1", mDate.ToString("MMM") + "-" + mDate.Year);
                mHead = mHead.Replace("%MonthYear2", mDate.AddMonths(1).ToString("MMM") + "-" + mDate.AddMonths(1).Year);
                mHead = mHead.Replace("%MonthYear3", mDate.AddMonths(2).ToString("MMM") + "-" + mDate.AddMonths(2).Year);
                mHead = mHead.Replace("%MonthYear4", mDate.AddMonths(3).ToString("MMM") + "-" + mDate.AddMonths(3).Year);
                mHead = mHead.Replace("%MonthYear5", mDate.AddMonths(4).ToString("MMM") + "-" + mDate.AddMonths(4).Year);
                mHead = mHead.Replace("%MonthYear6", mDate.AddMonths(5).ToString("MMM") + "-" + mDate.AddMonths(5).Year);
                mHead = mHead.Replace("%MonthYear7", mDate.AddMonths(6).ToString("MMM") + "-" + mDate.AddMonths(6).Year);
                mHead = mHead.Replace("%MonthYear8", mDate.AddMonths(7).ToString("MMM") + "-" + mDate.AddMonths(7).Year);
                mHead = mHead.Replace("%MonthYear9", mDate.AddMonths(8).ToString("MMM") + "-" + mDate.AddMonths(8).Year);
                mHead = mHead.Replace("%MonthYearA", mDate.AddMonths(9).ToString("MMM") + "-" + mDate.AddMonths(9).Year);
                mHead = mHead.Replace("%MonthYearB", mDate.AddMonths(10).ToString("MMM") + "-" + mDate.AddMonths(10).Year);
                mHead = mHead.Replace("%MonthYearC", mDate.AddMonths(11).ToString("MMM") + "-" + mDate.AddMonths(11).Year);
            }
            if (mHead.ToLower().Contains("%para"))
            {
                // para10/20 processed first as para1 will spoil para10
                mHead = mHead.Replace("%para24", ppara24);
                mHead = mHead.Replace("%para23", ppara23);
                mHead = mHead.Replace("%para22", ppara22);
                mHead = mHead.Replace("%para21", ppara21);
                mHead = mHead.Replace("%para20", ppara20);
                mHead = mHead.Replace("%para19", ppara19);
                mHead = mHead.Replace("%para18", ppara18);
                mHead = mHead.Replace("%para17", ppara17);
                mHead = mHead.Replace("%para16", ppara16);
                mHead = mHead.Replace("%para15", ppara15);
                mHead = mHead.Replace("%para14", ppara14);
                mHead = mHead.Replace("%para13", ppara13);
                mHead = mHead.Replace("%para12", ppara12);
                mHead = mHead.Replace("%para11", ppara11);
                mHead = mHead.Replace("%para10", ppara10);
                mHead = mHead.Replace("%para09", ppara09);
                mHead = mHead.Replace("%para08", ppara08);
                mHead = mHead.Replace("%para07", ppara07);
                mHead = mHead.Replace("%para06", ppara06);
                mHead = mHead.Replace("%para05", ppara05);
                mHead = mHead.Replace("%para04", ppara04);
                mHead = mHead.Replace("%para03", ppara03);
                mHead = mHead.Replace("%para02", ppara02);
                mHead = mHead.Replace("%para01", ppara01);
                mHead = mHead.Replace("%para9", ppara09);
                mHead = mHead.Replace("%para8", ppara08);
                mHead = mHead.Replace("%para7", ppara07);
                mHead = mHead.Replace("%para6", ppara06);
                mHead = mHead.Replace("%para5", ppara05);
                mHead = mHead.Replace("%para4", ppara04);
                mHead = mHead.Replace("%para3", ppara03);
                mHead = mHead.Replace("%para2", ppara02);
                mHead = mHead.Replace("%para1", ppara01);
            }
            return mHead;
        }


        public ActionResult ViewDocument (string Type,string Key)
        {
            string html = "";
            if (Type=="PUR00")
            {
                CreditPurchaseVM Model = new CreditPurchaseVM();
                Model.Document = Key;
                Model.TableName = "Purchase";
                Model.MainType = "PR";
                Model.Mode = "View";
                Model.Header = "Credit Purchase";
                Model.ViewDataId = "ViewData-Purchase";
                Model.Controller2 = "CreditPurchase";
                Model.Module = "Transactions";
                Model.OptionType = "M";
                Model.OptionCode = "CreditPurchase";

                html = ViewHelper.RenderPartialView(this, "~/Areas/Accounts/Views/CreditPurchase/Index.cshtml", Model);
            }



            
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetURLDoc(string mdocument, string mode = "Edit")
        {
            if (mdocument.StartsWith("%"))
            {
                mdocument = mdocument.Substring(1);
                mode = "Delete";
            }
            string murl = "";
            try
            {
                string mtype = "", mBranch = "", mPrefix="";
                string NewBranch = mdocument.Substring(0, 6);
                if (ctxTFAT.TfatBranch.Where(x=>x.Code==NewBranch).FirstOrDefault()==null)
                {
                    mtype = mdocument.Substring(0, 5);
                    mPrefix = ctxTFAT.Authorisation.Where(x => x.ParentKey == mdocument).Select(x => x.Prefix).FirstOrDefault();
                    mBranch = ctxTFAT.Authorisation.Where(x => x.ParentKey == mdocument).Select(x => x.Branch).FirstOrDefault();
                    if (mtype == "Trip0")
                    {
                        mdocument = mdocument.Substring(7, (mdocument.Length - 7));
                    }
                    else if (mtype == "FM000" || mtype == "LR000" || mtype == "LC000")
                    {
                        mdocument = mdocument;
                    }
                    else
                    {
                        mdocument = mBranch + mdocument;
                    }
                }
                else
                {
                    var mdocumentnew = mdocument.Substring(6, mdocument.Length-6);
                    mtype = mdocumentnew.Substring(0, 5);
                    mBranch = ctxTFAT.Authorisation.Where(x => x.ParentKey == mdocument).Select(x => x.Branch).FirstOrDefault();
                    mPrefix = ctxTFAT.Authorisation.Where(x => x.ParentKey == mdocument).Select(x => x.Prefix).FirstOrDefault();
                    if (mtype == "Trip0")
                    {
                        mdocument = mdocument.Substring(13, (mdocument.Length - 13));
                    }
                    else if (mtype == "FM000" || mtype == "LR000" || mtype == "LC000")
                    {
                        mdocument = mdocument;
                    }
                    else
                    {
                        mdocument = mBranch + mdocument;
                    }
                }

                int ID = GetId(mtype.Trim());

                
                var mrights = ctxTFAT.UserRights.Where(z => z.Code == muserid && z.MenuID == ID).FirstOrDefault();

                if (mrights != null || muserid.ToLower() == "super")
                {
                    string msubtype = GetSubType(mtype);
                    var mstr = ctxTFAT.TfatMenu.Select(x => new { x.ID, x.OptionCode, x.TableName, x.ModuleName, x.Menu, x.Controller, x.FormatCode, x.SubType, x.ParentMenu }).Where(z => z.ID == ID && z.ModuleName == "Transactions" && (z.ParentMenu == "Accounts" || z.ParentMenu == "Logistics" || z.ParentMenu == "Vehicles")).FirstOrDefault();
                    murl = "/" + mstr.ParentMenu + "/" + mstr.Controller + "/Index?Document=" + mdocument + "&Mode=" + mode+ "&Prefix=" + mPrefix + "&ChangeLog=" + mode + "&ViewDataId=" + HttpUtility.UrlEncode(mstr.FormatCode.Trim()) + "&Header=" + HttpUtility.UrlEncode(mstr.Menu.Trim()) + "&Module=" + HttpUtility.UrlEncode(mstr.ModuleName.Trim()) + "&TableName=" + mstr.TableName.Trim() + "&OptionType=T&OptionCode=" + HttpUtility.UrlEncode(mstr.OptionCode.Trim()) + "&Controller2=" + mstr.Controller.Trim() + "&LedgerThrough=true";
                }
                return Json(new { url = murl, Message = "In-sufficient Rights to Execute this Action." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { url = "", Message = "Error." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetURLDocForTrack(string mdocument)
        {
            if (mdocument.StartsWith("%"))
            {
                mdocument = mdocument.Substring(1);
            }
            string murl = "";
            try
            {
                string mtype = "";
                string NewBranch = mdocument.Substring(0, 6);
                if (ctxTFAT.TfatBranch.Where(x => x.Code == NewBranch).FirstOrDefault() == null)
                {
                    mtype = mdocument.Substring(0, 5);
                    mdocument = mdocument;
                }
                else
                {
                    var mdocumentnew = mdocument.Substring(6, mdocument.Length - 6);
                    mtype = mdocumentnew.Substring(0, 5);

                    mdocument = mdocument;
                }

                int ID = 0;
                string Property = "";
                switch (mtype.Trim())
                {
                    case "LR000":
                        ID = 2379;
                        Property = "ConsignmentKey";
                        break;
                    case "FM000":
                        ID = 2380;
                        Property = "FreightMemoKey";
                        break;
                    case "SLR00":
                    case "SLW00":
                    case "CMM00":
                        ID = 2381;
                        Property = "Tablekey";
                        break;
                    default:
                        break;
                }

                var mrights = ctxTFAT.UserRights.Where(z => z.Code == muserid && z.MenuID == ID).FirstOrDefault();

                if (mrights != null || muserid.ToLower() == "super")
                {
                    string msubtype = GetSubType(mtype);
                    var mstr = ctxTFAT.TfatMenu.Select(x => new { x.ID, x.OptionCode, x.TableName, x.ModuleName, x.Menu, x.Controller, x.FormatCode, x.SubType, x.ParentMenu }).Where(z => z.ID == ID  && (z.ParentMenu == "Accounts" || z.ParentMenu == "Logistics" || z.ParentMenu == "Vehicles")).FirstOrDefault();
                    murl = "/" + mstr.ParentMenu + "/" + mstr.Controller + "/Index?"+ Property +"= " + mdocument +  "&ViewDataId=" + HttpUtility.UrlEncode(mstr.FormatCode.Trim()) + "&Header=" + HttpUtility.UrlEncode(mstr.Menu.Trim()) + "&Module=" + HttpUtility.UrlEncode(mstr.ModuleName.Trim()) + "&TableName=" + mstr.TableName.Trim() + "&OptionType=T&OptionCode=" + HttpUtility.UrlEncode(mstr.OptionCode.Trim()) + "&Controller2=" + mstr.Controller.Trim() + "&Shortcut=true";
                }
                return Json(new { url = murl, Message = "In-sufficient Rights to Execute this Action." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { url = "", Message = "Error." }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}