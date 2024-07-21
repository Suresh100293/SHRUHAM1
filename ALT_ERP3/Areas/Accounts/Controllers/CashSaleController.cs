using Common;
using EntitiModel;
using Microsoft.Reporting.WebForms;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.SqlServer;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ALT_ERP3.Controllers;
using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Areas.Logistics.Controllers;
using CrystalDecisions.CrystalReports.Engine;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class CashSaleController : BaseController
    {
        string attachmentPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentPath"].ToString()) == true ? "" : ConfigurationManager.AppSettings["AttachmentPath"].ToString();

        private static string gpHoliday1 = "Saturday";
        private static string gpHoliday2 = "Sunday";
        private static int mGSTStyle = 0;
        public static object[,] objarray = null;
        private static string msearchstyle = "";
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
        #region index

        public ActionResult Index(LRInvoiceVM Model)
        {
            Session["CommnNarrlist"] = null;
            Session["TempAttach"] = null;
            GetAllMenu(Session["ModuleName"].ToString());
            //UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "NA");

            string connstring = GetConnectionString();
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            ViewBag.list = Session["MenuList"];
            ViewBag.Modules = Session["ModulesList"];
            //to-do remove updateitemlist. UPurchSerialStkList
            Session["CNewItemlist"] = null;

            // preferences from tfatbranch
            var mpara = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x).FirstOrDefault();
            bool mCashLimit = mpara.gp_CashLimit;
            double mCashLimitAmt = mpara.gp_CashLimitAmt == null ? 0 : mpara.gp_CashLimitAmt.Value;
            gpHoliday1 = mpara.gp_Holiday1 == null ? "" : mpara.gp_Holiday1;
            gpHoliday2 = mpara.gp_Holiday2 == null ? "" : mpara.gp_Holiday2;
            Model.EnableParty = mpara.gp_EnableParty;

            if (Model.Mode != "Add")
            {
                Model.Branch = Model.Document.Substring(0, 6);
                Model.ParentKey = Model.Document.Substring(6);
                Model.Type = "CMM00";
                UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, Model.ParentKey, DateTime.Now, 0, "", "", "NA");
            }
            else
            {
                Model.Branch = mbranchcode;
                Model.Type = "CMM00";
                UpdateAuditTrail(Model.Branch, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "NA");
            }
            //var LRBillQuery = @"select isnull(BillBoth,0) as BillBoth,BillAuto,OthLRShow,CutTDS,ShowLedgerPost,LRReqd,TDSCode from CashSalesetup";
            //DataTable imDt = GetDataTable(LRBillQuery, connstring);
            var LRBillSetup = ctxTFAT.CashSaleSetup.FirstOrDefault();
            if (LRBillSetup == null)
            {
                LRBillSetup = new CashSaleSetup();
            }
            if (LRBillSetup.CurrDatetOnlyreq == false && LRBillSetup.BackDateAllow == false && LRBillSetup.ForwardDateAllow == false)
            {
                Model.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
                Model.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
            }
            if (LRBillSetup.CurrDatetOnlyreq == true)
            {
                Model.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
                Model.EndDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            if (LRBillSetup.BackDateAllow == true)
            {
                Model.StartDate = (DateTime.Now.AddDays(-LRBillSetup.BackDaysUpto)).ToString("yyyy-MM-dd");
            }
            if (LRBillSetup.ForwardDateAllow == true)
            {
                Model.EndDate = (DateTime.Now.AddDays(LRBillSetup.ForwardDaysUpto)).ToString("yyyy-MM-dd");
            }

            Model.BillBoth = LRBillSetup.BillBoth;
            Model.BillAuto = LRBillSetup.BillAuto;
            Model.OthLRShow = LRBillSetup.OthLRShow;
            Model.TDSFlag = LRBillSetup.CutTDS;
            Model.ShowLedgerPost = LRBillSetup.ShowLedgerPost;
            if (Model.Mode == "Add")
            {
                var mDocDate = DateTime.Now.Date;
                Model.TDSRate = ctxTFAT.TDSRates.Where(x => x.Code.ToString() == Model.TDSCode && x.EffDate <= mDocDate).OrderByDescending(x => x.EffDate).Select(x => (decimal?)x.TDSRate).FirstOrDefault() ?? 0;
            }

            var result = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => x).FirstOrDefault();
            Model.MainType = result.MainType;
            Model.SubType = result.SubType;
            Model.Authorise = "A00";
            Model.IsManual = result.vAuto != "M" ? false : true;
            string mvprefix = result.PrefixConst == null ? "" : result.PrefixConst;
            int mWidth = result.DocWidth;
            var mPrevSrl = GetLastSerial(Model.TableName, Model.Branch, Model.Type, mperiod, Model.SubType, DateTime.Now.Date);
            if (mvprefix != "")
            {
                mPrevSrl = mPrevSrl.Replace(mvprefix, "");
            }
            int mPreIntSrl = Convert.ToInt32(mPrevSrl) - 1;
            ViewData["PrevSrl"] = mPreIntSrl.ToString("D" + mWidth);
            var mserial = mPreIntSrl + 1;
            if (Model.BillBoth == true || Model.BillAuto == true)
            {
                Model.LRGenerate = "A";
            }
            else
            {
                Model.LRGenerate = "M";
            }
            Model.VATGSTApp = (mpara.gp_VATGST == null || mpara.gp_VATGST == "") ? "G" : mpara.gp_VATGST;
            var mAuth = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == Model.Type).Select(x => new
            {
                x.AuthLock,
                x.AuthNoPrint,
                x.AuthReq,
                x.AuthAgain
            }).FirstOrDefault();
            if (mAuth != null)
            {
                Model.AuthLock = mAuth.AuthLock;
                Model.AuthNoPrint = mAuth.AuthNoPrint;
                Model.AuthReq = mAuth.AuthReq;
                Model.AuthAgain = mAuth.AuthAgain;
            }

            bool mBackDated = ctxTFAT.UserRightsTrx.Where(x => x.Code == muserid && x.Type == Model.Type).Select(z => z.xBackDated).FirstOrDefault();
            decimal mBackDays = result.BackDays == null ? 0 : result.BackDays.Value;
            //Model.IsRoundOff = 2;
            Model.id = result.Name;
            Model.Prefix = mperiod;
            Model.GSTType = result.GSTType.ToString();
            Model.GstTypeName = GetGSTTypeName(Model.GSTType);
            Model.LocationCode = result.LocationCode;
            ViewData["DocAttach"] = result.DocAttach;

            List<AddOns> objitemlist = new List<AddOns>();
            if (Model.Mode == "Add")
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
                Model.DocDate = DateTime.Now;
            }
            if (Model.Mode == "Edit" || Model.Mode == "View" || Model.Mode == "Delete")
            {
                Model.AddOnList = GetAddOnListOnEdit(Model.MainType, Model.ParentKey, Model.Type);

                #region Sales
                if (Model.SubType == "OC" || Model.SubType == "RS" || Model.SubType == "CS" || Model.SubType == "XS" || Model.SubType == "SX" || Model.SubType == "NS")
                {
                    var mobjtax = (from s in ctxTFAT.LRBill
                                   where s.ParentKey == Model.ParentKey && s.Branch == Model.Branch

                                   select new
                                   {
                                       s.Code,
                                       s.Sno,
                                       s.Amt,
                                       s.Narr,
                                       s.BalQty,
                                       s.TotQty,
                                       s.Branch,
                                       s.Freight,
                                       s.LrNo,
                                       s.TableKey,
                                       s.Type,
                                       s.DocDate,
                                       s.POD,
                                       s.LRRefTablekey
                                   }).ToList();
                    var mobj1 = ctxTFAT.Sales.Where(x => (x.TableKey) == Model.ParentKey && x.Branch == Model.Branch).FirstOrDefault();
                    Model.PeriodLock = PeriodLock(mobj1.Branch, mobj1.Type, mobj1.DocDate.Value);
                    if (mobj1.AUTHORISE.Substring(0, 1) == "A")
                    {
                        Model.LockAuthorise = LockAuthorise(mobj1.Type, Model.Mode, mobj1.TableKey, mobj1.TableKey);
                    }
                    Model.Authorise = mobj1.AUTHORISE;
                    var mdoctype = ctxTFAT.DocTypes.Where(x => x.Code == mobj1.Type).Select(x => new { x.DocWidth }).FirstOrDefault();
                    Model.Srl = mobj1.Srl.PadLeft(mdoctype.DocWidth, '0');
                    Model.Prefix = mobj1.Prefix;
                    Model.ChequeNo = mobj1.ChqNo;
                    Model.DocDate = mobj1.DocDate.Value;
                    Model.Branch = mobj1.Branch;
                    Model.LocationCode = mobj1.LocationCode;
                    Model.Account = mobj1.CashBankCode;
                    Model.AccountName = (ctxTFAT.Master.Where(x => x.Code == mobj1.CashBankCode).Select(x => x.Name).FirstOrDefault().ToString());
                    Model.AccParentGrp = ctxTFAT.CustomerMaster.Where(x => x.Code == mobj1.Code).Select(x => x.AccountParentGroup).FirstOrDefault();
                    Model.AccParentGrpName = ctxTFAT.Master.Where(x => x.Code == Model.AccParentGrp).Select(x => x.Name).FirstOrDefault();
                    Model.Customer = mobj1.Code;
                    Model.CustomerName = ctxTFAT.CustomerMaster.Where(x => x.Code == mobj1.Code).Select(x => x.Name).FirstOrDefault();

                    Model.AltAddress = Convert.ToByte(mobj1.AltAddress);
                    Model.AddressFrom = ctxTFAT.Address.Where(x => x.Code == mobj1.CashBankCode && x.Sno == mobj1.AltAddress).Select(x => x.Adrl1 + x.Adrl2 + x.Adrl3).FirstOrDefault();
                    Model.Document = Model.Document;
                    Model.GSTType = (mobj1.GSTType == null) ? "0" : mobj1.GSTType.Value.ToString();
                    Model.GstTypeName = GetGSTTypeName(Model.GSTType);
                    Model.CrPeriod = mobj1.CrPeriod == null ? 0 : mobj1.CrPeriod.Value;
                    Model.GSTCode = mobj1.TaxCode;
                    Model.BillNarr = mobj1.Narr;
                    Model.CPerson = ctxTFAT.Caddress.Where(x => x.Code == mobj1.CashBankCode && x.Sno == 0).Select(x => x.Name).FirstOrDefault();
                    if (Model.SubType == "SX" || Model.SubType == "NS")
                    {
                        Model.RoundOff = mobj1.RoundOff == null ? 0 : mobj1.RoundOff.Value * -1;
                    }
                    else
                    {
                        Model.RoundOff = mobj1.RoundOff == null ? 0 : mobj1.RoundOff.Value;
                    }
                    Model.PlaceOfSupply = mobj1.PlaceOfSupply;
                    var Branch = "";
                    var master = ctxTFAT.Master.Where(x => x.Code == Model.Account).FirstOrDefault();
                    if (master.FetchBalAcc == false)
                    {
                        Branch = mbranchcode;
                    }
                    string mStr = @"select dbo.GetBalance('" + Model.Account + "','" + MMDDYY(DateTime.Now) + "','" + Branch + "',0,0)";
                    DataTable smDt = GetDataTable(mStr);
                    double mBalance = 0;
                    if (smDt.Rows.Count > 0)
                    {
                        mBalance = Convert.ToDouble(smDt.Rows[0][0].ToString());
                    }
                    Model.Balance = mBalance;
                    int mCnt = 1;
                    List<LRInvoiceVM> Upobjitemdetail = new List<LRInvoiceVM>();
                    foreach (var item in mobjtax)
                    {
                        var lrmaster = ctxTFAT.LRMaster.Where(x => x.TableKey == item.LRRefTablekey).Select(x => x).FirstOrDefault();
                        var mchargeslist = GetLRCharges(item.TableKey);
                        Upobjitemdetail.Add(new LRInvoiceVM()
                        {
                            tempId = mCnt,
                            Code = item.LrNo,
                            LRName = item.LrNo,
                            Qty = Convert.ToDouble(item.TotQty),
                            Val1 = item.Amt == null ? 0 : item.Amt.Value,
                            Narr = item.Narr,
                            LRChargeList = mchargeslist,
                            HeaderList = mchargeslist.Select(x => x.Header).ToList(),
                            ChgPickupList = mchargeslist.Select(x => x.Amt).ToList(),
                            Branch = ctxTFAT.TfatBranch.Where(X => X.Code == lrmaster.Branch).Select(x => x.Name).FirstOrDefault(),
                            Account = ctxTFAT.Master.Where(X => X.Code == lrmaster.BillParty).Select(x => x.Name).FirstOrDefault(),
                            Weightage = Convert.ToDecimal(lrmaster.ActWt),
                            Consignor = ctxTFAT.Consigner.Where(x => x.Code == lrmaster.RecCode).Select(x => x.Name).FirstOrDefault(),
                            Consignee = ctxTFAT.Consigner.Where(x => x.Code == lrmaster.SendCode).Select(x => x.Name).FirstOrDefault(),
                            ToLocation = ctxTFAT.TfatBranch.Where(X => X.Code == lrmaster.Dest).Select(X => X.Name).FirstOrDefault(),
                            FromLocation = ctxTFAT.TfatBranch.Where(X => X.Code == lrmaster.Source).Select(X => X.Name).FirstOrDefault(),
                            BookNarr = lrmaster.Narr,
                            BillNarr = item.Narr,
                            LRDocDate = item.DocDate == null ? Convert.ToDateTime("1900-01-01") : item.DocDate.Value,
                            TotalQty = lrmaster.TotQty,
                            POD = item.POD,
                            LRRefTableKey = lrmaster.TableKey
                        });
                        ++mCnt;
                    }
                    Session.Add("CNewItemlist", Upobjitemdetail);
                    Model.NewItemList = Upobjitemdetail;
                    Model.TotalQty = mobj1.Qty == null ? 0 : Convert.ToDouble(mobj1.Qty.Value);
                    Model.HeaderList = ctxTFAT.Charges.Where(x => x.Type == "LR000").Select(x => x.Head).ToList();
                    decimal[] mCharges2 = new decimal[(Model.HeaderList.Count)];
                    for (int ai = 0; ai < Model.HeaderList.Count; ai++)
                    {
                        decimal mchgamt = 0;
                        foreach (var i in Upobjitemdetail)
                        {
                            mchgamt += i.ChgPickupList[ai];
                        }
                        mCharges2[ai] = mchgamt;
                    }
                    Model.TotalChgPickupList = mCharges2.ToList();
                }
                #endregion
                Model.HeaderList = ctxTFAT.Charges.Where(x => x.Type == "LR000").Select(x => x.Head).ToList();
                #region Narration 
                var mnarr = ctxTFAT.Narration.Where(x => x.TableKey == Model.ParentKey).FirstOrDefault();
                if (mnarr != null)
                {
                    Model.RichNote = mnarr.NarrRich;
                }
                else
                {
                    Model.RichNote = "";
                }
                #endregion

                #region ATTACHMENT
                AttachmentVM Att = new AttachmentVM();
                Att.Type = "CMM00";
                Att.Srl = Model.Srl.ToString();

                AttachmentController attachmentC = new AttachmentController();
                List<AttachmentVM> attachments = attachmentC.GetAttachmentListInEdit(Att);
                Session["TempAttach"] = attachments;

                #endregion
                var CheckDependency2 = ctxTFAT.Outstanding.Where(x => x.aType == Model.Type && x.aSrl == Model.Srl && x.aPrefix == Model.Prefix && x.DocBranch == Model.Branch).Select(x => x).FirstOrDefault();
                if (CheckDependency2 != null)
                {
                    if (CheckDependency2.Type == Model.Type && CheckDependency2.Srl == Model.Srl && CheckDependency2.Prefix == Model.Prefix && CheckDependency2.DocBranch == Model.Branch)
                    {
                    }
                    else
                    {
                        Model.CheckMode = true;

                        Model.Message = " Document is Already Adjusted In Cash Bank Against : " + CheckDependency2.TableRefKey.ToString() + ", Cant " + Model.Mode;
                    }

                }
                if (Model.Mode != "Add" && Model.AuthReq == true && Model.Authorise.Substring(0, 1) == "A" && Model.AuthLock)
                {
                    Model.CheckMode = true;
                    Model.Message = "Document is Already Authorised Cant Edit";
                }
                if (Model.Authorise.Substring(0, 1) == "X")
                {
                    Model.IsDraftSave = true;
                    Model.Message = "Document is saved As Draft";
                }
                #region GstGet
                var mLedger = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).OrderBy(x => x.Sno).FirstOrDefault();
                if (mLedger != null)
                {
                    Model.IGSTAmt = mLedger.IGSTAmt == null ? 0 : mLedger.IGSTAmt.Value;
                    Model.CGSTAmt = mLedger.CGSTAmt == null ? 0 : mLedger.CGSTAmt.Value;
                    Model.SGSTAmt = mLedger.SGSTAmt == null ? 0 : mLedger.SGSTAmt.Value;
                    if (Model.IGSTAmt > 0 || Model.CGSTAmt > 0 || Model.SGSTAmt > 0)
                    {
                        Model.GSTFlag = true;
                    }
                    Model.SGSTRate = mLedger.SGSTRate == null ? 0 : mLedger.SGSTRate.Value;
                    Model.CGSTRate = mLedger.CGSTRate == null ? 0 : mLedger.CGSTRate.Value;
                    Model.IGSTRate = mLedger.IGSTRate == null ? 0 : mLedger.IGSTRate.Value;
                    Model.GSTCode = mLedger.TaxCode == null ? "" : mLedger.TaxCode.ToString();
                    Model.GSTCodeName = ctxTFAT.TaxMaster.Where(x => x.Code.ToString() == Model.GSTCode).Select(x => x.Name).FirstOrDefault();
                    if (String.IsNullOrEmpty(Model.GSTCodeName))
                    {
                        Model.GSTCodeName = ctxTFAT.HSNMaster.Where(x => x.Code.ToString() == Model.GSTCode).Select(x => x.Name).FirstOrDefault();
                    }

                }

                Model.Taxable = Model.TotalChgPickupList == null ? 0 : Model.TotalChgPickupList.Sum();
                Model.InvoiceAmt = Math.Abs(Model.Taxable + Model.IGSTAmt + Model.CGSTAmt + Model.SGSTAmt);
                #endregion
                var tdsdetails = ctxTFAT.TDSPayments.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).Select(x => x).FirstOrDefault();
                Model.TDSAmt = (tdsdetails != null) ? tdsdetails.TDSAmt.Value : 0;
                Model.TDSRate = (tdsdetails != null) ? tdsdetails.TDSTax == null ? 0 : tdsdetails.TDSTax.Value : 0;
                Model.TDSCode = (tdsdetails != null) ? tdsdetails.TDSCode.Value.ToString() : "";
                Model.TDSCodeName = ctxTFAT.TDSMaster.Where(x => x.Code.ToString() == Model.TDSCode).Select(x => x.Name).FirstOrDefault();
                if (Model.TDSAmt > 0)
                {
                    Model.TDSFlag = true;
                }
            }
            if (Model.Mode == "Add")
            {
                List<LRInvoiceVM> objledgerdetail = new List<LRInvoiceVM>();
                var trncharges = (from c in ctxTFAT.Charges.Where(x => x.Type == Model.Type && x.DontUse == false)

                                  select new { c.Fld, c.Head, c.EqAmt, c.Equation, c.Code }).ToList();
                DataTable mdt = GetChargeValValue(Model.SubType, Model.ParentKey);
                int mfld;
                foreach (var i in trncharges)
                {
                    LRInvoiceVM c = new LRInvoiceVM();
                    c.Fld = i.Fld;
                    c.Head = i.Head;
                    c.AddLess = i.EqAmt;
                    c.Equation = i.Equation;
                    c.tempId = Convert.ToInt16(i.Fld.Substring(1, 3));
                    if (Model.Mode != "Add")
                    {
                        if (mdt != null && mdt.Rows.Count > 0)
                        {
                            mfld = Convert.ToInt32(i.Fld.Substring(1));
                            c.ColVal = mdt.Rows[0]["Amt" + mfld].ToString();
                        }
                        else
                        {
                            c.ColVal = "0";
                        }
                    }
                    c.ChgPostCode = i.Code;
                    objledgerdetail.Add(c);
                }
                Model.Charges = objledgerdetail;


            }
            List<GridOption> Grlist = new List<GridOption>();
            var list = ctxTFAT.DocFormats.Where(x => x.Type == "CMM00").Select(x => x).ToList();
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
            return View(Model);
        }

        #endregion index

        #region Get
        public int GetNewAttachCode()
        {
            string Code = ctxTFAT.Attachment.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).Take(1).FirstOrDefault();
            if (String.IsNullOrEmpty(Code))
            {
                return 100000;
            }
            else
            {
                return Convert.ToInt32(Code) + 1;
            }
        }

        public ActionResult GetTDSList(string term)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.TDSMaster.Select(c => new { c.Code, c.Name }).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var result = ctxTFAT.TDSMaster.Where(x => x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetTDSRateDetail(LRInvoiceVM Model)
        {
            var CurrDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            var TDSRate = ctxTFAT.TDSRates.Where(x => x.Code.ToString() == Model.TDSCode && x.EffDate <= CurrDate).OrderByDescending(x => x.EffDate).Select(x => new { x.TDSRate/*, x.Cess, x.SurCharge, x.SHECess*/ }).FirstOrDefault();
            if (TDSRate != null)
            {
                Model.TDSRate = (TDSRate.TDSRate == null) ? 0 : Convert.ToDecimal(TDSRate.TDSRate.Value);
            }
            else
            {
                Model.TDSRate = 0;
            }
            return Json(Model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetGSTList(string term)
        {
            if (ctxTFAT.TfatComp.Select(x => x.UseHSNMaster).FirstOrDefault() == true)
            {
                if (term == "" || term == null)
                {
                    var result = ctxTFAT.HSNMaster.Select(x => new { x.Code, x.Name }).ToList().Take(10);
                    var Modified = result.Select(x => new
                    {
                        Code = x.Code,
                        Name = "[" + x.Name + "]"
                    });


                    return Json(Modified, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = ctxTFAT.HSNMaster.Where(x => x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).ToList().Take(10);
                    var Modified = result.Select(x => new
                    {
                        Code = x.Code,
                        Name = "[" + x.Name + "]"
                    });
                    return Json(Modified, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                if (term == "" || term == null)
                {
                    var result = ctxTFAT.TaxMaster.Where(x => x.VATGST == true).Select(x => new { x.Code, x.Name, x.Scope }).ToList().Take(10);
                    var Modified = result.Select(x => new
                    {
                        Code = x.Code,
                        Name = "[" + x.Name + "][" + x.Scope + "]"
                    });


                    return Json(Modified, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = ctxTFAT.TaxMaster.Where(x => x.VATGST == true && x.Name.Contains(term)).Select(m => new { m.Code, m.Name, m.Scope }).ToList().Take(10);
                    var Modified = result.Select(x => new
                    {
                        Code = x.Code,
                        Name = "[" + x.Name + "][" + x.Scope + "]"
                    });
                    return Json(Modified, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult GetGSTRateDetail(string Code, string Party, string DocDate)
        {
            var CurrDate = ConvertDDMMYYTOYYMMDD(DocDate);
            var UseHSN = ctxTFAT.TfatComp.Select(x => x.UseHSNMaster).FirstOrDefault();
            var pstate = ctxTFAT.Address.Where(x => x.Code == Party).Select(x => x.State).FirstOrDefault();
            if (String.IsNullOrEmpty(pstate))
            {
                pstate = "19";
            }
            var branchstate = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.State).FirstOrDefault();
            if (pstate == branchstate)
            {
                if (UseHSN)
                {
                    var result = ctxTFAT.HSNRates.Where(x => x.Code == Code && x.EffDate <= CurrDate).OrderByDescending(x => x.EffDate).Select(x => new { IGSTRate = (decimal)0, SGSTRate = x.SGSTRate.Value, CGSTRate = x.CGSTRate.Value }).FirstOrDefault();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = ctxTFAT.TaxMaster.Where(x => x.Code == Code).Select(x => new { IGSTRate = (decimal)0, SGSTRate = x.SGSTRate.Value, CGSTRate = x.CGSTRate.Value }).FirstOrDefault();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                if (UseHSN)
                {
                    var result = ctxTFAT.HSNRates.Where(x => x.Code == Code && x.EffDate <= CurrDate).OrderByDescending(x => x.EffDate).Select(x => new { IGSTRate = x.IGSTRate.Value, SGSTRate = (decimal)0, CGSTRate = (decimal)0 }).FirstOrDefault();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = ctxTFAT.TaxMaster.Where(x => x.Code == Code).Select(x => new { IGSTRate = x.IGSTRate.Value, SGSTRate = (decimal)0, CGSTRate = (decimal)0 }).FirstOrDefault();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

            }
        }

        public ActionResult GetPartyList(string term)
        {
            if (term == "" || term == null)
            {

                var result = (from m in ctxTFAT.Master.Where(x => (x.BaseGr == "C" || x.BaseGr == "B" ) && x.Hide == false)
                              select new
                              {
                                  m.Code,
                                  Name = m.Name,
                                  OName = m.Name
                              }).OrderBy(n => n.OName).Take(10).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = (from m in ctxTFAT.Master
                              where m.Name.ToLower().Trim().Contains(term.ToLower().Trim()) && (m.BaseGr == "C" || m.BaseGr == "B") &&  m.Hide == false
                              select new
                              {
                                  m.Code,
                                  Name = m.Name,
                                  OName = m.Name
                              }).OrderBy(n => n.OName).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetDebtorsList(string term)
        {
            if (term == "" || term == null)
            {

                var result = (from m in ctxTFAT.Master.Where(x => ( x.BaseGr == "U" || x.BaseGr == "D") && x.Hide == false) 
                              select new
                              {
                                  m.Code,
                                  Name = m.Name,
                                  OName = m.Name
                              }).OrderBy(n => n.OName).Take(10).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = (from m in ctxTFAT.Master
                              where m.Name.ToLower().Trim().Contains(term.ToLower().Trim()) && (m.BaseGr == "U" || m.BaseGr == "D") && m.Hide == false
                              select new
                              {
                                  m.Code,
                                  Name = m.Name,
                                  OName = m.Name
                              }).OrderBy(n => n.OName).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetCustomerList(string term)
        {
            if (term == "" || term == null)
            {

                var result = (from m in ctxTFAT.CustomerMaster.Where(x => x.Hide==false)
                              select new
                              {
                                  m.Code,
                                  Name = m.Name,
                                  OName = m.Name
                              }).OrderBy(n => n.OName).Take(10).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = (from m in ctxTFAT.CustomerMaster
                              where m.Name.ToLower().Trim().Contains(term.ToLower().Trim()) && m.Hide == false
                              select new
                              {
                                  m.Code,
                                  Name = m.Name,
                                  OName = m.Name
                              }).OrderBy(n => n.OName).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult GetCustomerParentDetails(LRInvoiceVM Model)
        {
            //var result = ctxTFAT.CustomerMaster.Where(x => x.Code == Model.Code).Select(x => new { x.Code, x.Name, x.SalesMan, x.Broker, x.AccountParentGroup }).FirstOrDefault();
            Model.AccParentGrp = ctxTFAT.CustomerMaster.Where(x=>x.Code==Model.Customer).Select(x=>x.AccountParentGroup).FirstOrDefault();
            Model.AccParentGrpName = ctxTFAT.Master.Where(x=>x.Code==Model.AccParentGrp).Select(x=>x.Name).FirstOrDefault();

            return Json(new
            {
                
                AccParentGrpName = Model.AccParentGrpName,
                AccParentGrp = Model.AccParentGrp,
                
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetPartyDetails(LRInvoiceVM Model)
        {
            var CurrDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);

            //var result = ctxTFAT.CustomerMaster.Where(x => x.Code == Model.Code).Select(x => new { x.Code, x.Name, x.SalesMan, x.Broker, x.AccountParentGroup }).FirstOrDefault();
            Model.AccParentGrp = "";
            var msterinf = ctxTFAT.MasterInfo.Where(x => x.Code == Model.Code).Select(x => new { x.CurrName, x.TDSCode, x.CutTDS, x.CrPeriod, x.Brokerage, x.Transporter, x.IncoTerms, x.IncoPlace, x.PaymentTerms, x.Rank }).FirstOrDefault();
            var addrl = ctxTFAT.Address.Where(x => x.Code == Model.Code && x.Sno == 0).Select(x => new { x.Email, x.State, x.Name }).FirstOrDefault();
            var taxdetails = ctxTFAT.TaxDetails.Where(x => x.Code == Model.Code).Select(x => new { x.TDSCode, x.CutTDS }).FirstOrDefault();
            Model.CrPeriod = msterinf == null ? 0 : (msterinf.CrPeriod == null) ? 0 : msterinf.CrPeriod.Value;
            Model.AccParentGrpName = ""/* ctxTFAT.Master.Where(x => x.Code == Model.AccParentGrp).Select(x => x.Name).FirstOrDefault()*/;
            Model.CPerson = addrl == null ? "" : addrl.Name;
            Model.PlaceOfSupply = addrl == null ? "" : addrl.State;
            Model.CutTDS = taxdetails == null ? false : taxdetails.CutTDS;
            Model.TDSCode = taxdetails == null ? "0" : taxdetails.TDSCode == null ? "0" : taxdetails.TDSCode.Value.ToString();
            Model.TDSCodeName = ctxTFAT.TDSMaster.Where(x => x.Code.ToString() == Model.TDSCode).Select(x => x.Name).FirstOrDefault();
            var mDocDate = DateTime.Now.Date;
            //var TDSRATEtab = ctxTFAT.TDSRates.Where(x => x.Code.ToString() == Model.TDSCode && x.EffDate <= mDocDate).OrderByDescending(x => x.EffDate).Select(x => new { x.TDSRate, x.Cess, x.SHECess, x.SurCharge }).FirstOrDefault();
            decimal TDSRate = 0;
            TDSRate = ctxTFAT.TDSRates.Where(x => x.Code.ToString() == Model.TDSCode && x.EffDate <= CurrDate).OrderByDescending(x => x.EffDate).Select(x => x.TDSRate ?? 0).FirstOrDefault();
            Model.TDSRate = TDSRate; var Branch = "";
            var master = ctxTFAT.Master.Where(x => x.Code == Model.Code).FirstOrDefault();
            if (master.FetchBalAcc == false)
            {
                Branch = mbranchcode;
            }
            string mStr = @"select dbo.GetBalance('" + Model.Code + "','" + MMDDYY(DateTime.Now) + "','" + Branch + "',0,0)";
            DataTable smDt = GetDataTable(mStr);
            double mBalance = 0;
            if (smDt.Rows.Count > 0)
            {
                mBalance = Convert.ToDouble(smDt.Rows[0][0].ToString());
            }
            Model.Balance = mBalance;
            var cmsterinf = ctxTFAT.CHoldTransactions.Where(x => x.Code == Model.Code).Select(x => new { x.HoldInvoice, x.HoldInvoiceDt, x.HoldNarr, x.Ticklers }).FirstOrDefault();
            Model.HoldInvoice = cmsterinf == null ? false : cmsterinf.HoldInvoice;

            Model.HoldInvoiceDt = cmsterinf == null ? Convert.ToDateTime("1900-01-01") : cmsterinf.HoldInvoiceDt == null ? Convert.ToDateTime("1900-01-01") : cmsterinf.HoldInvoiceDt.Value;
            if (Model.HoldInvoiceDt < DateTime.Now && Model.HoldInvoiceDt != Convert.ToDateTime("1900-01-01"))
            {
                Model.AlertHoldInvoice = true;
            }
            Model.HoldNarr = cmsterinf == null ? "" : cmsterinf.HoldNarr;
            Model.Tickler = cmsterinf == null ? "" : cmsterinf.Ticklers;

            var TDSFlagSetup = ctxTFAT.CashSaleSetup.Select(x => x.CutTDS).FirstOrDefault();
            if (TDSFlagSetup == false)
            {
                Model.CutTDS = false;
            }

            #region GST
            string GSTCode = "0", GSTName = "";
            bool GstFlag = false;
            decimal IGST = 0, CGST = 0, SGST = 0;
            var UseHSN = ctxTFAT.TfatComp.Select(x => x.UseHSNMaster).FirstOrDefault();
            MasterInfo Masteraddress = ctxTFAT.MasterInfo.Where(x => x.Code == Model.Code.Trim()).FirstOrDefault();
            GstFlag = Masteraddress == null ? false : Masteraddress.GSTFlag;

            if (UseHSN)
            {
                if (!string.IsNullOrEmpty(Masteraddress.HSNCode))
                {
                    GSTCode = Masteraddress.HSNCode;
                    GSTName = ctxTFAT.HSNMaster.Where(x => x.Code == GSTCode).Select(x => x.Name).FirstOrDefault();
                }
                var EffectDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                var pstate = ctxTFAT.Address.Where(x => x.Code == Model.Code.Trim()).Select(x => x.State).FirstOrDefault();
                if (String.IsNullOrEmpty(pstate))
                {
                    pstate = "19";
                }
                var branchstate = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.State).FirstOrDefault();
                if (pstate == branchstate)
                {
                    var result1 = ctxTFAT.HSNRates.Where(x => x.Code == Masteraddress.HSNCode && x.EffDate <= EffectDate).OrderByDescending(x => x.EffDate).Select(x => new { IGSTRate = (decimal)0, SGSTRate = x.SGSTRate.Value, CGSTRate = x.CGSTRate.Value }).FirstOrDefault();
                    IGST = result1 == null ? 0 : result1.IGSTRate;
                    SGST = result1 == null ? 0 : result1.SGSTRate;
                    CGST = result1 == null ? 0 : result1.CGSTRate;
                }
                else
                {
                    var result1 = ctxTFAT.HSNRates.Where(x => x.Code == Masteraddress.HSNCode && x.EffDate <= EffectDate).OrderByDescending(x => x.EffDate).Select(x => new { IGSTRate = x.IGSTRate.Value, SGSTRate = (decimal)0, CGSTRate = (decimal)0 }).FirstOrDefault();
                    IGST = result1 == null ? 0 : result1.IGSTRate;
                    SGST = result1 == null ? 0 : result1.SGSTRate;
                    CGST = result1 == null ? 0 : result1.CGSTRate;
                }
            }

            #endregion
            return Json(new
            {
                GstFlag = GstFlag,
                GSTCode = GSTCode,
                GSTName = GSTName,
                IGST = IGST,
                SGST = SGST,
                CGST = CGST,
                CrPeriod = Model.CrPeriod,
                PlaceOfSupply = Model.PlaceOfSupply,
                AccParentGrp = Model.AccParentGrp,
                TDSRate = Model.TDSRate,
                CutTDS = Model.CutTDS,
                TDSCode = Model.TDSCode,
                TDSCodeName = Model.TDSCodeName,
                AccParentGrpName = Model.AccParentGrpName,
                CPerson = Model.CPerson,
                Balance = Model.Balance,
                HoldNarr = Model.HoldNarr,
                HoldInvoice = Model.HoldInvoice,
                AlertHoldInvoice = Model.AlertHoldInvoice,
                Tickler = Model.Tickler
            }, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetGSTCode(string MainType, string VATGSTApp)
        {
            List<SelectListItem> gstcodelist = new List<SelectListItem>();

            var result = ctxTFAT.TaxMaster.Where(x => x.VATGST != false && x.Scope == MainType.Substring(0, 1)).Select(m => new
            {
                m.Code,
                m.Name
            }).Distinct().ToList();
            foreach (var item in result)
            {
                gstcodelist.Add(new SelectListItem { Text = item.Name, Value = item.Code });
            }

            return Json(gstcodelist, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAddrSnoList(string Code)
        {
            List<SelectListItem> AddrSnoList = new List<SelectListItem>();
            var addrsnolist = ctxTFAT.CustomerMaster.Where(x => x.Code == Code).ToList().Select(b => new
            {
                b.AccountParentGroup,

            }).ToList();
            foreach (var item in addrsnolist)
            {
                var mName = ctxTFAT.Master.Where(x => x.Code == item.AccountParentGroup).Select(x => x.Name).FirstOrDefault();
                AddrSnoList.Add(new SelectListItem { Text = mName, Value = item.AccountParentGroup });
            }
            return Json(AddrSnoList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAddressBySno(string Code/*, int Sno*/)
        {
            string Address = "";
            Caddress result;

            result = (from Add in ctxTFAT.Caddress where Add.Code == Code && Add.Sno == 0 select Add).FirstOrDefault();
            if (result != null)
            {
                if (result.Adrl1 != null)
                {
                    Address = result.Adrl1;
                }
                if (result.Adrl2 != null && result.Adrl2 != "")
                {
                    if (Address != "")
                    {
                        Address = Address + ",\n" + result.Adrl2;
                    }
                    else
                    {
                        Address = result.Adrl2;
                    }
                }
                if (result.Adrl3 != null && result.Adrl3 != "")
                {
                    if (Address != "")
                    {
                        Address = Address + ",\n" + result.Adrl3;
                    }
                    else
                    {
                        Address = result.Adrl3;
                    }
                }
                if (result.Adrl4 != "" && result.Adrl4 != "")
                {
                    if (Address != "")
                    {
                        Address = Address + ",\n" + result.Adrl4;
                    }
                    else
                    {
                        Address = result.Adrl4;
                    }
                }
                if (result.City != null && result.City != "")
                {
                    if (Address != "")
                    {
                        Address = Address + ",\n" + result.City + (result.Pin == null || result.Pin == "" ? "" : "-" + result.Pin);
                    }
                    else
                    {
                        Address = result.City;
                    }
                }
                if (result.State != null && result.State != "")
                {
                    if (Address != "")
                    {
                        Address = Address + ",\n" + result.State;
                    }
                    else
                    {
                        Address = result.State;
                    }
                }
                if (result.Country != null && result.Country != "")
                {
                    if (Address != "")
                    {
                        Address = Address + ",\n" + result.Country;
                    }
                    else
                    {
                        Address = result.Country;
                    }

                }
                if (result.Mobile != null && result.Mobile != "")
                {
                    if (Address != "")
                    {
                        Address = Address + ",\n" + result.Mobile;
                    }
                    else
                    {
                        Address = result.Mobile;
                    }

                }
                if (result.Email != null)
                {
                    if (Address != "")
                    {
                        Address = Address + ",\n" + result.Email;
                    }
                    else
                    {
                        Address = result.Email;
                    }
                }
            }

            return Json(Address, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPlaceOfSupply(string term)
        {
            if (term == null || term == "")
            {
                var result = (from id in ctxTFAT.TfatState
                              select new
                              {
                                  Code = id.Code.ToString(),
                                  Name = id.Name
                              }).OrderBy(n => n.Name).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = (from id in ctxTFAT.TfatState
                              where id.Name.Contains(term)
                              select new
                              {
                                  Code = id.Code.ToString(),
                                  Name = id.Name
                              }).OrderBy(n => n.Name).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        public string GetNewCode()
        {

            var mPrevSrl = GetLastSerial("PODMaster", mbranchcode, "POD00", mperiod, "RS", DateTime.Now.Date);

            //var NewLcNo = ctxTFAT.PODMaster.Where(x => x.Prefix == mperiod).OrderByDescending(x => x.PODNo).Select(x => x.PODNo).Take(1).FirstOrDefault();
            //int LcNo;
            //if (NewLcNo == 0 || NewLcNo == null)
            //{
            //    var DocType = ctxTFAT.DocTypes.Where(x => x.Code == "POD00").Select(x => new { x.DocWidth, x.LimitFrom }).FirstOrDefault();
            //    LcNo = Convert.ToInt32(DocType.LimitFrom);
            //}
            //else
            //{
            //    LcNo = Convert.ToInt32(NewLcNo) + 1;
            //}

            return mPrevSrl.ToString();
        }

        #endregion

        #region Item Add Edit
        [HttpPost]
        public ActionResult FetchDocumentList(LRInvoiceVM Model)
        {
            List<LRInvoiceVM> ValueList = new List<LRInvoiceVM>();
            var mlrmaster = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == Model.Code).Select(x => x).ToList();
            if (mlrmaster == null)
            {
                Model.Status = "ValidError";
                Model.Message = "Consignment Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            if (mlrmaster.Count() == 0)
            {
                Model.Status = "ValidError";
                Model.Message = "Consignment Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            else if (mlrmaster.Count() == 1)
            {
                Model.Status = "Processed";
                Model.Message = mlrmaster.Select(x => x.TableKey).FirstOrDefault();
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            foreach (var item in mlrmaster)
            {
                LRInvoiceVM otherTransact = new LRInvoiceVM();
                otherTransact.LRRefTableKey = item.TableKey.ToString();
                otherTransact.LRDocDate = item.BookDate;
                otherTransact.TotalQty = item.TotQty;
                otherTransact.ActWt = item.ActWt;
                otherTransact.ChgWt = item.ChgWt;
                otherTransact.FromLocation = ctxTFAT.TfatBranch.Where(x => x.Code == item.Source).Select(x => x.Name).FirstOrDefault();
                otherTransact.ToLocation = ctxTFAT.TfatBranch.Where(x => x.Code == item.Dest).Select(x => x.Name).FirstOrDefault();
                otherTransact.Consignor = ctxTFAT.Consigner.Where(x => x.Code == item.SendCode).Select(x => x.Name).FirstOrDefault();
                otherTransact.Consignee = ctxTFAT.Consigner.Where(x => x.Code == item.RecCode).Select(x => x.Name).FirstOrDefault();
                ValueList.Add(otherTransact);
            }
            Model.ValueList = ValueList;
            var html = ViewHelper.RenderPartialView(this, "ConsignmentList", Model);
            return Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetSearchDocument(LRInvoiceVM Model)
        {
            try
            {
                var result = (List<LRInvoiceVM>)Session["LONewItemlist"];

                if (result == null)
                {
                    result = new List<LRInvoiceVM>();
                }
                #region Get Charges
                List<LRInvoiceVM> newlist1 = new List<LRInvoiceVM>();
                //Default Charges
                var ChargesCount = ctxTFAT.Charges.Where(x => x.Type == "LR000" && x.DontUse == false).ToList().Count();

                List<bool> ChargesFlag = new List<bool>();
                ChargesFlag.Add(false);
                if (result.Count() > 0)
                {
                    var GetFirstObjectChargesList = result.FirstOrDefault();
                    var ExistChargesList = GetFirstObjectChargesList.LRChargeList.Count();
                    if (ExistChargesList > ChargesCount)
                    {
                        ChargesFlag.Add(true);
                    }

                }
                var mCharges = (from C in ctxTFAT.Charges
                                where C.Type == "LR000" && ChargesFlag.Contains(C.DontUse)

                                select new
                                {
                                    C.Fld,
                                    C.Head,
                                    C.EqAmt,
                                    C.Equation,
                                    C.Code
                                }).ToList();
                int a = 1;
                foreach (var i in mCharges)
                {
                    LRInvoiceVM c = new LRInvoiceVM();
                    c.Fld = i.Fld;
                    c.Header = i.Head;
                    c.AddLess = i.EqAmt;
                    c.tempId = Convert.ToInt16(i.Fld.Substring(1, 3));
                    c.ChgPostCode = i.Code;
                    c.Amt = GetLorryMasterCharges(c.tempId, Model.Code);

                    newlist1.Add(c);

                    a = a + 1;
                }

                Model.LRChargeList = newlist1;
                Model.POD = false;
                #endregion

                string mStatus = "";
                string mMessage = "";

                //Change
                //var mCode = (string.IsNullOrEmpty(Model.Code) == true) ? 0 : Convert.ToInt32(Model.Code);
                var lrdetails = ctxTFAT.LRMaster.Where(x => x.TableKey == Model.Code).Select(x => x).FirstOrDefault();
                if (lrdetails == null)
                {
                    Model.Status = "ValidtnError";
                    Model.Message = "Document Not Found..";
                    return Json(Model, JsonRequestBehavior.AllowGet);
                }
                //Model.Code = lrdetails.LrNo.ToString();
                Model.LRName = lrdetails.LrNo.ToString();
                Model.TotalQty = lrdetails.TotQty;
                Model.LRDocDate = lrdetails.BookDate;
                Model.LRPartyInvoice = lrdetails.PartyInvoice;
                Model.LRPONumber = lrdetails.PONumber;
                Model.LRRefTableKey = lrdetails.TableKey;

                Model.LRDocuDate = lrdetails.BookDate.ToString("dd-MM-yyyy");
                Model.Branch = ctxTFAT.TfatBranch.Where(X => X.Code == lrdetails.BillBran).Select(X => X.Name).FirstOrDefault();
                Model.Consignor = ctxTFAT.Consigner.Where(X => X.Code == lrdetails.RecCode).Select(x => x.Name).FirstOrDefault();
                Model.Consignee = ctxTFAT.Consigner.Where(X => X.Code == lrdetails.SendCode).Select(x => x.Name).FirstOrDefault();
                Model.Narr = lrdetails.Narr;
                Model.Weightage = Convert.ToDecimal(lrdetails.ActWt);
                Model.FromLocation = ctxTFAT.TfatBranch.Where(X => X.Code == lrdetails.Source).Select(X => X.Name).FirstOrDefault();
                Model.ToLocation = ctxTFAT.TfatBranch.Where(X => X.Code == lrdetails.Dest).Select(X => X.Name).FirstOrDefault();
                var mAccount = ctxTFAT.CustomerMaster.Where(X => X.Code == Model.Account).Select(x => x.Name).FirstOrDefault();
                Model.Account = ctxTFAT.CustomerMaster.Where(X => X.Code == lrdetails.BillParty).Select(x => x.Name).FirstOrDefault();
                //Change
                var mPendingQty = (ctxTFAT.LRMaster.Where(x => x.TableKey == Model.Code).Sum(x => (int?)x.TotQty ?? 0) - Math.Abs(ctxTFAT.LRBill.Where(x => x.LRRefTablekey == Model.Code).Sum(x => (int?)x.TotQty) ?? 0));
                Model.Qty = mPendingQty;
                Model.Pending = mPendingQty;
                Model.ActWt = lrdetails.ActWt;
                Model.ChgWt = lrdetails.ChgWt;
                if (!String.IsNullOrEmpty(lrdetails.DescrType))
                {
                    Model.ChargeType = ctxTFAT.DescriptionMaster.Where(x => x.Code == lrdetails.DescrType).Select(x => x.Description).FirstOrDefault();
                }
                //if (lrdetails.BillBran != mbranchcode)
                //{
                //    Model.Status = "ConfirmError";
                //    Model.Message += "LR Bill Branch is Not of Current Branch Please Confirm Do You Want to Continue..\n";
                //}
                //if (mPendingQty <= 0)
                //{
                //    Model.Status = "ConfirmError";
                //    Model.Message += "No Pending Quantity of Selected LR " + Model.Code + ".. Do You want to Continue\n";
                //    Model.LockQty = true;
                //}
                //if (mAccount != Model.Account)
                //{
                //    Model.Status = "ConfirmError";
                //    Model.Message += "" + Model.Code + " is of " + Model.Account + " Do you want to continue..\n";
                //}
                //if (Model.Account == null)
                //{
                //    Model.Status = "ConfirmError";
                //    Model.Message += "LR Bill to Party Not Found Please Confirm Do You Want to Continue..\n";
                //}
                List<string> mLRCodes = new List<string>();

                if (result != null && result.Count > 0)
                {
                    mLRCodes = result.Select(x => x.LRRefTableKey).ToList();
                }
                //Change
                if (mLRCodes.Where(x => x == lrdetails.TableKey.ToString()).FirstOrDefault() != null)
                {
                    Model.Status = "ValidtnError";
                    Model.Message = "Already Selected";
                }
                //var abc = this.RenderPartialView("UpdateItemlist", Model);
                var jsonResult = Json(new { LockQty = Model.LockQty, Status = Model.Status, Message = Model.Message, Html = this.RenderPartialView("UpdateItemlist", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
            catch (Exception ex)
            {
                var jsonResult = Json(new { Status = "Error" }, JsonRequestBehavior.AllowGet);
                return jsonResult;
            }

        }
        public decimal GetLorryMasterCharges(int i, string LRno)
        {
            string connstring = GetConnectionString();
            decimal abc;
            var loginQuery3 = @"select Val" + i + " from lrmaster where  Tablekey = '" + LRno + "'";
            DataTable mDt2 = GetDataTable(loginQuery3, connstring);
            if (mDt2.Rows.Count > 0)
            {
                abc = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? 0 : Convert.ToDecimal(mDt2.Rows[0][0].ToString());
            }
            else
            {
                abc = 0;
            }
            return decimal.Round(abc, 2, MidpointRounding.AwayFromZero);
        }

        [HttpPost]
        public ActionResult LRValidation(LRInvoiceVM Model)
        {
            var mLrMaster = ctxTFAT.PODRel.Where(x => x.LRRefTablekey == Model.Code && x.ParentKey != Model.ParentKey).Select(x => x).FirstOrDefault();
            if (mLrMaster != null && Model.POD == true)
            {
                Model.Status = "ConfirmError";
                Model.Message = "LRNo " + Model.Code + " POD is already sended";
                Model.POD = false;
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            else
            {
                Model.POD = false;
            }
            Model.POD = false;
            return Json(Model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetItemListforUpdate(LRInvoiceVM Model)
        {
            try
            {
                var result = (List<LRInvoiceVM>)Session["CNewItemlist"];
                if (result == null)
                {
                    result = new List<LRInvoiceVM>();
                }
                if (Model.SessionFlag == "Edit")
                {
                    var result1 = result.Where(x => x.tempId == Model.tempId).ToList();
                    foreach (var item in result1)
                    {
                        //int mCode = Convert.ToInt32(item.Code);
                        var mPendingQty = (ctxTFAT.LRMaster.Where(x => x.TableKey == item.LRRefTableKey).Sum(x => (int?)x.TotQty ?? 0) - Math.Abs(ctxTFAT.LRBill.Where(x => x.LRRefTablekey == item.LRRefTableKey).Sum(x => (int?)x.TotQty) ?? 0));
                        Model.tempId = item.tempId;
                        Model.Code = item.Code;
                        Model.LRName = item.Code;
                        var mLrMaster = ctxTFAT.LRMaster.Where(x => x.TableKey == item.LRRefTableKey).Select(x => x).FirstOrDefault();
                        Model.Qty = item.Qty;
                        Model.Val1 = item.Val1;
                        Model.Narr = item.Narr;
                        Model.SubType = Model.SubType;
                        Model.MainType = Model.MainType;
                        Model.Mode = Model.Mode;
                        Model.Account = ctxTFAT.Master.Where(X => X.Code == mLrMaster.BillParty).Select(X => X.Name).FirstOrDefault();
                        Model.LRDocDate = mLrMaster.CreateDate;
                        Model.IsPickUp = item.IsPickUp;
                        Model.PickedUpIn = item.PickedUpIn;
                        Model.ChgWt = mLrMaster.ChgWt;
                        Model.ActWt = mLrMaster.ActWt;
                        Model.ChargeType = mLrMaster.ChgType;
                        //Model.LRChargeList = item.LRChargeList;
                        Model.TotalQty = mLrMaster.TotQty;
                        Model.Pending = mPendingQty + item.Qty;
                        Model.Branch = ctxTFAT.TfatBranch.Where(X => X.Code == mLrMaster.BillBran).Select(X => X.Name).FirstOrDefault();
                        Model.Consignor = ctxTFAT.Consigner.Where(x => x.Code == mLrMaster.RecCode).Select(x => x.Name).FirstOrDefault();
                        Model.Consignee = ctxTFAT.Consigner.Where(x => x.Code == mLrMaster.SendCode).Select(x => x.Name).FirstOrDefault();

                        Model.ToLocation = ctxTFAT.TfatBranch.Where(X => X.Code == mLrMaster.Dest).Select(X => X.Name).FirstOrDefault();
                        Model.FromLocation = ctxTFAT.TfatBranch.Where(X => X.Code == mLrMaster.Source).Select(X => X.Name).FirstOrDefault();
                        Model.BookNarr = item.BookNarr;
                        Model.BillNarr = item.BillNarr;
                        Model.POD = item.POD;
                        Model.LRRefTableKey = item.LRRefTableKey;
                        List<LRInvoiceVM> newlist1 = new List<LRInvoiceVM>();
                        //Default Charges
                        var ChargesCount = ctxTFAT.Charges.Where(x => x.Type == "LR000" && x.DontUse == false).ToList().Count();
                        List<bool> ChargesFlag = new List<bool>();
                        ChargesFlag.Add(false);
                        if (result.Count() > 0)
                        {
                            var GetFirstObjectChargesList = result.FirstOrDefault();
                            var ExistChargesList = GetFirstObjectChargesList.LRChargeList.Count();
                            if (ExistChargesList > ChargesCount)
                            {
                                ChargesFlag.Add(true);
                            }

                        }


                        var mCharges = (from C in ctxTFAT.Charges
                                        where C.Type == "LR000" && ChargesFlag.Contains(C.DontUse)

                                        select new
                                        {
                                            C.Fld,
                                            C.Head,
                                            C.EqAmt,
                                            C.Equation,
                                            C.Code
                                        }).ToList();
                        int a = 1;
                        foreach (var i in mCharges)
                        {
                            LRInvoiceVM c = new LRInvoiceVM();
                            c.Fld = i.Fld;
                            c.Header = i.Head;
                            c.AddLess = i.EqAmt;
                            c.tempId = Convert.ToInt16(i.Fld.Substring(1, 3));
                            c.ChgPostCode = i.Code;
                            c.Amt = item.LRChargeList.Where(x => x.Fld == i.Fld).Select(x => x.Amt).FirstOrDefault();
                            newlist1.Add(c);

                            a = a + 1;
                        }

                        Model.LRChargeList = newlist1;


                    }
                }
                else
                {
                    List<LRInvoiceVM> newlist1 = new List<LRInvoiceVM>();
                    //Default Charges
                    var ChargesCount = ctxTFAT.Charges.Where(x => x.Type == "LR000" && x.DontUse == false).ToList().Count();

                    List<bool> ChargesFlag = new List<bool>();
                    ChargesFlag.Add(false);
                    if (result.Count() > 0)
                    {
                        var GetFirstObjectChargesList = result.FirstOrDefault();
                        var ExistChargesList = GetFirstObjectChargesList.LRChargeList.Count();
                        if (ExistChargesList > ChargesCount)
                        {
                            ChargesFlag.Add(true);
                        }

                    }
                    var mCharges = (from C in ctxTFAT.Charges
                                    where C.Type == "LR000" && ChargesFlag.Contains(C.DontUse)

                                    select new
                                    {
                                        C.Fld,
                                        C.Head,
                                        C.EqAmt,
                                        C.Equation,
                                        C.Code
                                    }).ToList();
                    int a = 1;
                    foreach (var i in mCharges)
                    {
                        LRInvoiceVM c = new LRInvoiceVM();
                        c.Fld = i.Fld;
                        c.Header = i.Head;
                        c.AddLess = i.EqAmt;
                        c.tempId = Convert.ToInt16(i.Fld.Substring(1, 3));
                        c.ChgPostCode = i.Code;
                        newlist1.Add(c);

                        a = a + 1;
                    }

                    Model.LRChargeList = newlist1;
                    Model.POD = true;
                }

                string mStatus = "";
                string mMessage = "";

                var abc = this.RenderPartialView("UpdateItemlist", Model);
                var jsonResult = Json(new { Html = this.RenderPartialView("UpdateItemlist", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
            catch (Exception ex)
            {
                var jsonResult = Json(new { Status = "Error" }, JsonRequestBehavior.AllowGet);
                return jsonResult;
            }

        }

        public ActionResult UpdateItemList(LRInvoiceVM Model)
        {
            List<LRInvoiceVM> AddLRList = new List<LRInvoiceVM>();
            List<LRInvoiceVM> AddLRList2 = new List<LRInvoiceVM>();
            var result = (List<LRInvoiceVM>)Session["CNewItemlist"];
            AddLRList2 = (result == null) ? AddLRList2 : result;

            int Maxtempid = (AddLRList2.Count == 0) ? 0 : AddLRList2.Select(x => x.tempId).Max();

            AddLRList.AddRange(AddLRList2);
            if (Model.SessionFlag == "Add")
            {
                Maxtempid = Maxtempid + 1;
                //int mCode = Convert.ToInt32(Model.Code);
                var mLrMaster = ctxTFAT.LRMaster.Where(x => x.TableKey == Model.Code).Select(x => x).FirstOrDefault();
                AddLRList.Add(new LRInvoiceVM()
                {
                    tempId = Maxtempid,
                    Code = mLrMaster.LrNo.ToString(),
                    LRName = Model.LRName,
                    Qty = Model.Qty,
                    Val1 = Model.Val1,
                    Narr = Model.Narr,
                    LRChargeList = Model.LRChargeList,
                    HeaderList = Model.LRChargeList.Select(x => x.Header).ToList(),
                    ChgPickupList = Model.LRChargeList.Select(x => x.Amt).ToList(),
                    Branch = ctxTFAT.TfatBranch.Where(X => X.Code == mLrMaster.Branch).Select(X => X.Name).FirstOrDefault(),
                    Consignor = ctxTFAT.Consigner.Where(x => x.Code == mLrMaster.RecCode).Select(x => x.Name).FirstOrDefault(),
                    Consignee = ctxTFAT.Consigner.Where(x => x.Code == mLrMaster.SendCode).Select(x => x.Name).FirstOrDefault(),
                    ToLocation = ctxTFAT.TfatBranch.Where(X => X.Code == mLrMaster.Dest).Select(X => X.Name).FirstOrDefault(),
                    FromLocation = ctxTFAT.TfatBranch.Where(X => X.Code == mLrMaster.Source).Select(X => X.Name).FirstOrDefault(),
                    TotalQty = mLrMaster.TotQty,
                    BookNarr = Model.BookNarr,
                    BillNarr = Model.BillNarr,
                    ChgWt = mLrMaster.ChgWt,
                    ActWt = mLrMaster.ActWt,
                    ChargeType = mLrMaster.ChgType,
                    LRDocDate = ConvertDDMMYYTOYYMMDD(Model.LRDocuDate),
                    POD = Model.POD,
                    LRRefTableKey = mLrMaster.TableKey
                });
            }
            if (Model.SessionFlag == "Edit")
            {
                foreach (var item in AddLRList.Where(x => x.tempId == Model.tempId))
                {
                    //int mCode = Convert.ToInt32(Model.Code);
                    var mLrMaster = ctxTFAT.LRMaster.Where(x => x.TableKey == item.LRRefTableKey).Select(x => x).FirstOrDefault();
                    item.tempId = Model.tempId;
                    item.Code = mLrMaster.LrNo.ToString();
                    item.LRName = Model.LRName;
                    item.Qty = Model.Qty;
                    item.Val1 = Model.Val1;
                    item.Narr = Model.Narr;
                    item.tempId = Model.tempId;
                    item.tempIsDeleted = false;
                    item.LRChargeList = Model.LRChargeList;
                    item.HeaderList = Model.LRChargeList.Select(x => x.Header).ToList();
                    item.ChgPickupList = Model.LRChargeList.Select(x => x.Amt).ToList();
                    item.Branch = ctxTFAT.TfatBranch.Where(X => X.Code == mLrMaster.Branch).Select(X => X.Name).FirstOrDefault();
                    item.Consignor = ctxTFAT.Consigner.Where(x => x.Code == mLrMaster.RecCode).Select(x => x.Name).FirstOrDefault();
                    item.Consignee = ctxTFAT.Consigner.Where(x => x.Code == mLrMaster.SendCode).Select(x => x.Name).FirstOrDefault();
                    item.ToLocation = ctxTFAT.TfatBranch.Where(X => X.Code == mLrMaster.Dest).Select(X => X.Name).FirstOrDefault();
                    item.FromLocation = ctxTFAT.TfatBranch.Where(X => X.Code == mLrMaster.Source).Select(X => X.Name).FirstOrDefault();
                    item.BookNarr = Model.BookNarr;
                    item.ChgWt = mLrMaster.ChgWt;
                    item.ActWt = mLrMaster.ActWt;
                    item.ChargeType = mLrMaster.ChgType;
                    item.BillNarr = Model.BillNarr;
                    item.LRDocDate = ConvertDDMMYYTOYYMMDD(Model.LRDocuDate);
                    item.POD = Model.POD;

                }
            }


            //Default Charges
            var ChargesCount = ctxTFAT.Charges.Where(x => x.Type == "LR000" && x.DontUse == false).ToList().Count();

            List<bool> ChargesFlag = new List<bool>();
            ChargesFlag.Add(false);
            if (AddLRList2.Count() > 0)
            {
                var GetFirstObjectChargesList = AddLRList2.FirstOrDefault();
                var ExistChargesList = GetFirstObjectChargesList.LRChargeList.Count() - 1;
                if (ExistChargesList >= ChargesCount)
                {
                    ChargesFlag.Add(true);
                }

            }
            Model.HeaderList = ctxTFAT.Charges.Where(x => x.Type == "LR000" && ChargesFlag.Contains(x.DontUse)).Select(x => x.Head).ToList();


            decimal[] mCharges2 = new decimal[(Model.HeaderList.Count)];

            for (int ai = 0; ai < Model.HeaderList.Count; ai++)
            {
                decimal mchgamt = 0;
                foreach (var i in AddLRList)
                {
                    if (i.ChgPickupList.Count > ai)
                    {
                        mchgamt += i.ChgPickupList[ai];
                    }
                    else
                    {
                        mchgamt = 0;
                    }


                }
                mCharges2[ai] = mchgamt;
            }
            Model.TotalChgPickupList = mCharges2.ToList();
            Session.Add("CNewItemlist", AddLRList);
            decimal mTotal = AddLRList.Sum(x => x.Val1);
            double mTotalQty = AddLRList.Sum(x => x.Qty);
            string html;
            html = ViewHelper.RenderPartialView(this, "ItemChargeMoreView", new LRInvoiceVM() { NewItemList = AddLRList, HeaderList = Model.HeaderList, TotalChgPickupList = Model.TotalChgPickupList, Taxable = mTotal });
            var jsonResult = Json(new
            {
                Html = html,
                Total = mTotal,
                TotalQty = mTotalQty
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public ActionResult DeleteItemList(LRInvoiceVM Model)
        {
            var result2 = (List<LRInvoiceVM>)Session["CNewItemlist"];

            var result = result2.Where(x => x.tempId != Model.tempId).ToList();
            Session.Add("CNewItemlist", result);
            string html;

            Model.HeaderList = result2.Where(x => x.tempId == Model.tempId).Select(x => x.HeaderList).FirstOrDefault();

            decimal[] mCharges2 = new decimal[(Model.HeaderList.Count)];

            for (int ai = 0; ai < Model.HeaderList.Count; ai++)
            {
                decimal mchgamt = 0;
                foreach (var i in result)
                {
                    mchgamt += i.ChgPickupList[ai];

                }
                mCharges2[ai] = mchgamt;
            }
            Model.TotalChgPickupList = mCharges2.ToList();
            decimal mTotal = result.Sum(x => x.Val1);
            double mTotalQty = result.Sum(x => x.Qty);

            Model.NewItemList = result;

            Model.Taxable = mTotal;
            Model.TotalQty = mTotalQty;

            html = ViewHelper.RenderPartialView(this, "ItemChargeMoreView", Model);
            var jsonResult = Json(new
            {
                Status = "Success",
                Html = html,
                Total = mTotal,
                TotalQty = mTotalQty
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public ActionResult GetLRDetailsFromPickup(LRInvoiceVM Model)
        {
            try
            {

                var lrdetails = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == Model.Code).Select(x => x).FirstOrDefault();


                var mPendingQty = (ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == Model.Code).Sum(x => (int?)x.TotQty ?? 0) - Math.Abs(ctxTFAT.LRBill.Where(x => x.LrNo == Model.Code).Sum(x => (int?)x.TotQty) ?? 0));


                Model.LRName = Model.Code;

                Model.Qty = mPendingQty;
                Model.Val1 = 0;
                Model.Narr = "";
                Model.SubType = Model.SubType;
                Model.MainType = Model.MainType;
                Model.Mode = Model.Mode;
                //Model.Freight = item.Freight;
                Model.Account = ctxTFAT.Master.Where(X => X.Code == lrdetails.BillParty).Select(X => X.Name).FirstOrDefault();
                Model.LRDocDate = lrdetails.CreateDate;

                Model.ChgWt = lrdetails.ChgWt;
                Model.ActWt = lrdetails.ActWt;
                Model.ChargeType = lrdetails.ChgType;
                //Model.LRChargeList = item.LRChargeList;
                Model.TotalQty = lrdetails.TotQty;
                Model.Pending = mPendingQty;
                Model.Branch = ctxTFAT.TfatBranch.Where(X => X.Code == lrdetails.BillBran).Select(X => X.Name).FirstOrDefault();
                Model.Consignor = ctxTFAT.Consigner.Where(x => x.Code == lrdetails.RecCode).Select(x => x.Name).FirstOrDefault();
                Model.Consignee = ctxTFAT.Consigner.Where(x => x.Code == lrdetails.SendCode).Select(x => x.Name).FirstOrDefault();

                Model.ToLocation = ctxTFAT.TfatBranch.Where(X => X.Code == lrdetails.Dest).Select(X => X.Name).FirstOrDefault();
                Model.FromLocation = ctxTFAT.TfatBranch.Where(X => X.Code == lrdetails.Source).Select(X => X.Name).FirstOrDefault();
                Model.BookNarr = "";
                Model.BillNarr = "";


                List<LRInvoiceVM> newlist1 = new List<LRInvoiceVM>();

                var mCharges = (from C in ctxTFAT.Charges
                                where C.Type == "LR000" && C.DontUse == false

                                select new
                                {
                                    C.Fld,
                                    C.Head,
                                    C.EqAmt,
                                    C.Equation,
                                    C.Code
                                }).ToList();
                int a = 1;
                int b = 0;
                foreach (var i in mCharges)
                {
                    LRInvoiceVM c = new LRInvoiceVM();
                    c.Fld = i.Fld;
                    c.Header = i.Head;
                    c.AddLess = i.EqAmt;
                    c.tempId = Convert.ToInt16(i.Fld.Substring(1, 3));
                    c.ChgPostCode = i.Code;
                    c.Amt = Model.ChgPickupList[b];
                    newlist1.Add(c);

                    a = a + 1;
                    b = b + 1;
                }

                Model.LRChargeList = newlist1;


                var jsonResult = Json(new { Html = this.RenderPartialView("UpdateItemlist", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
            catch (Exception ex)
            {
                var jsonResult = Json(new { Status = "Error" }, JsonRequestBehavior.AllowGet);
                return jsonResult;
            }

        }

        #endregion

        #region Save Update


        public string CheckValidations(LRInvoiceVM Model)
        {
            string connstring = GetConnectionString();
            var mpara = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Branch).Select(x => new { x.gp_NegStock, x.gp_Serial, x.gp_Batch, x.gp_SONoDupl }).FirstOrDefault();
            var mtfatperd = ctxTFAT.TfatPerd.Where(x => x.PerdCode == mperiod).Select(x => new { x.StartDate, x.LastDate }).FirstOrDefault();
            var trxUserRgts = ctxTFAT.UserRightsTrx.Where(x => x.Code == muserid && x.Type == Model.Type).Select(x => x).FirstOrDefault();

            bool mNegStock = mpara.gp_NegStock;
            bool gpSerial = mpara.gp_Serial;
            bool gpBatch = mpara.gp_Batch;
            var mresult = (List<LRInvoiceVM>)Session["CNewItemlist"];
            DateTime mStartDate = Convert.ToDateTime(System.Web.HttpContext.Current.Session["StartDate"]);
            DateTime mLastDate = Convert.ToDateTime(System.Web.HttpContext.Current.Session["LastDate"]);
            // Model.SubType = Model.SubType;
            int xCnt = 0;
            string mStr = "";
            string mMessage = "";
            Model.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            var resultdoctype = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => new
            { x.AllowZero, x.SkipStock, x.CurrConv, x.NonStock, x.ForceChln, x.ForceOrder, x.ForceIndents, x.PricelistReqd, x.SalesmanReqd, x.BrokerReqd, x.DocAttach, x.RequireAttach, x.BackDays }).FirstOrDefault();
            bool mAllowZeroSales = resultdoctype.AllowZero;
            bool mSkipStock = resultdoctype.SkipStock == "Y" ? true : false;
            bool mTradef = resultdoctype.CurrConv == "Y" ? true : false;
            bool mNonStock = resultdoctype.NonStock;
            bool mForceChln = resultdoctype.ForceChln;
            bool mForceOrder = resultdoctype.ForceOrder;
            bool mForceIndents = resultdoctype.ForceIndents;
            bool PartyPriceListReqd = resultdoctype.PricelistReqd;
            bool mBackDated = ctxTFAT.UserRightsTrx.Where(x => x.Code == muserid && x.Type == Model.Type).Select(z => z.xBackDated).FirstOrDefault();
            decimal mBackDays = resultdoctype.BackDays == null ? 0 : resultdoctype.BackDays.Value;
            decimal mBalance = 0;
            decimal mCreditLimit = 0;
            decimal mCrLimitTole = 0;
            bool mCreditDayCheck = false;
            bool mCreditCheck = false;
            bool mCrLimitPO = false;
            bool mCrLimitWarn = false;
            bool mCRDaysWarn = false;
            string mPartyPriceList = "";
            bool mDocAttach = resultdoctype.DocAttach;
            bool mRequireAttach = resultdoctype.RequireAttach;
            if (muserid == null || muserid == "")
            {
                mMessage = mMessage + "\nUser Session is Expired..";
            }

            // check if period locked for the document date
            if (ctxTFAT.PeriodLock.Where(x => x.Type == Model.Type && x.LockDate == Model.DocDate).Select(x => x.Locked).FirstOrDefault() == true)
            {
                mMessage = mMessage + "\nPeriod is Locked..";
                //return false;
            }

            //var CheckTypeBranch = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => x.AppBranch).FirstOrDefault();
            //if(Model.Branch != CheckTypeBranch)
            //{
            //    mMessage = mMessage + "\nDocument Type is Not of Current Branch Cant Save..";
            //}



            if (Model.Branch == "" || Model.Branch == null)
            {
                Model.Branch = mbranchcode;
            }
            string mCode = Model.Account;

            string mTrx = "";
            if (Model.MainType == "SL" && "ES~OS~QS~PI~SG".Contains(Model.SubType) == false)
            {
                mTrx = "S";
            }
            else if (Model.MainType == "PR" && "EP~OP~QP~PG".Contains(Model.SubType) == false)
            {
                mTrx = "P";
            }


            if (mDocAttach && mRequireAttach)
                mMessage = mMessage + "\nAttachment is Required..";


            if (mtfatperd.StartDate > Model.DocDate || Model.DocDate > mtfatperd.LastDate)
            {
                mMessage = mMessage + "\nSelected Document Date is not in Current Accounting Period..";
            }

            if (Model.SubType == "OP" || Model.SubType == "OS")
            {

                //if (Model.SubType == "OP")for omsai commented for promot
                //{
                //    if (Model.BillNumber == "" || Model.BillNumber == null)
                //        mMessage = "Order Number is Required..";

                //}
                //else
                //{
                if (Model.BillNumber == "" || Model.BillNumber == null)
                    Model.BillNumber = Model.Srl.ToString(); ;
                //}
                if (mpara.gp_SONoDupl == false)
                {
                    string mbill = ctxTFAT.Orders.Where(x => x.TableKey != Model.ParentKey && x.DocDate >= mStartDate && x.DocDate <= mLastDate && x.Code == Model.Account && x.SubType == Model.SubType && x.Branch == Model.Branch && x.BillNumber == Model.BillNumber).Select(x => x.TableKey).FirstOrDefault();
                    if (mbill != "" && mbill != null)
                    {
                        mMessage = mMessage + "\nDuplicate Order Number.. \nAlready used in " + mbill;
                    }
                }

            }
            else
            {
                if (Model.BillNumber == "" || Model.BillNumber == null)
                    Model.BillNumber = Model.Srl;
            }

            if (mForceChln == true)
            {
                int mFChln = mresult.Where(x => x.ChlnKey != null && x.ChlnKey.Trim() != "").Count();
                if (mFChln == 0)
                {
                    mMessage = mMessage + "\nPlease Pickup Challan From Source Doc..";
                }
            }


            if (mForceOrder == true)
            {
                int mFOrd = mresult.Where(x => x.OrdKey != null && x.OrdKey.Trim() != "").Count();
                if (mFOrd == 0)
                {
                    mMessage = mMessage + "\nPlease Pickup Orders From Source Doc..";
                }
            }
            if (mForceIndents == true)
            {
                int mFInd = mresult.Where(x => x.IndKey != null && x.IndKey.Trim() != "").Count();
                if (mFInd == 0)
                {
                    mMessage = mMessage + "\nPlease Pickup Indents From Source Doc..";
                }
            }



            decimal mLimitAmount = (trxUserRgts == null) ? 0 : (trxUserRgts.xLimit == null) ? 0 : trxUserRgts.xLimit.Value;
            if (mLimitAmount != 0 && Model.Amt > mLimitAmount)
            {
                mMessage = mMessage + "\nDocument Amount Limit Restrictions for the User: " + muserid + " is set to Amount: " + mLimitAmount;
            }


            //if (Model.Amt < 0 && mAllowNegSales == false)
            //{
            //    mMessage = mMessage + "\nNegative Document Amount not Allowed..";
            //}


            if (Model.Amt == 0 && mAllowZeroSales == false)
            {
                mMessage = mMessage + "\nZero Document Amount not Allowed..";
            }


            //if (mSkipStock == false && (mresult.Where(x => x.tempIsDeleted != true).Count() == 0))
            //{
            //    mMessage = mMessage + "\nInventory not Entered..";
            //}


            string mMsg = CheckEntryDate(ConvertDDMMYYTOYYMMDD(Model.DocuDate), mBackDated, mStartDate, mLastDate, mBackDays, DateTime.Now, false, gpHoliday1, gpHoliday2);
            if (mMsg != "")
            {
                mMessage = mMessage + "\n" + mMsg;
            }





            if (mresult != null && mresult.Count() > 0)
            {
                string mduplcode = mresult.GroupBy(x => x.Code).Where(g => g.Count() > 1).Select(y => y.Key).FirstOrDefault();
                if (string.IsNullOrEmpty(mduplcode) == false)
                {
                    mMessage = mMessage + "\nProduct is Duplicate Cant Save the Product is .." + mduplcode;
                }

            }





            if (("RP IC IM OP").Contains(Model.SubType))
            {

            }
            var masterinfo = ctxTFAT.MasterInfo.Where(x => x.Code == Model.Account).Select(x => new { x.CrLimit, x.CheckCRLimit, x.CheckCRDays, x.CRLimitWithPO, x.CRLimitWarn, x.PriceList, x.CRDaysWarn }).FirstOrDefault();
            if (masterinfo != null)
            {
                mCreditLimit = masterinfo.CrLimit == null ? 0 : masterinfo.CrLimit.Value;
                mCreditCheck = masterinfo.CheckCRLimit;
                mCreditDayCheck = masterinfo.CheckCRDays;
                mCRDaysWarn = masterinfo.CRDaysWarn;
                mCrLimitPO = masterinfo.CRLimitWithPO;
                mCrLimitWarn = masterinfo.CRLimitWarn;
                mPartyPriceList = masterinfo.PriceList;
            }
            if (PartyPriceListReqd == true)
            {
                if (string.IsNullOrEmpty(mPartyPriceList))
                {
                    mMessage = mMessage + "\n PriceList Code is Required in Party Master ..";
                }
            }
            if (Model.Mode == "Add")
            {
                // if CreditLimit Check to be compulsory in case of Credit Sales
                if (mCreditCheck == true && mCrLimitWarn == true && (Model.SubType == "RS" || Model.SubType == "OC" || Model.SubType == "OS"))
                {
                    decimal madj = 0;
                    // deduct from the cr.limit the picked up ord. from the current doc.
                    //if (mCrLimitPO)
                    //{
                    //    //madj = mresult.Where(x => x.tempIsDeleted != true && x.OrdKey != "").Sum(z => z.Val1);
                    //    madj = GetCreditPendingPO(Model.Account, Model.SubType);
                    //}
                    mBalance = GetBalance(Model.Account, DateTime.Now, Model.Branch);
                    if ((mBalance + Model.Amt - madj) > mCreditLimit + (mCreditLimit * mCrLimitTole / 100))
                    {
                        mMessage = mMessage + "\nParty will Cross Credit Limit..";
                    }

                }

                if (mCreditDayCheck == true && mCRDaysWarn == true && (Model.SubType == "OC" || Model.SubType == "RS" || Model.SubType == "OS"))
                {
                    decimal mBal = GetBalance(Model.Account, Model.DocDate, Model.Branch);
                    if (mBal > 0)
                    {
                        string mCrOverSrl = "";
                        string mOSStr = @"Select Top 1 ParentKey from Ledger Where Branch='" + Model.Branch + "' and Code='" + Model.Account + "' and Debit<>0 and MainType <> 'MV' and MainType <> 'PV' and (Debit+Credit) - isnull((Select Sum(o.Amount) from Outstanding o where o.TableRefKey=Ledger.TableKey),0)<>0 and Left(Ledger.Authorise,1)='A' and DocDate+CrPeriod<'" + MMDDYY(Model.DocDate) + "'";
                        DataTable smDt = GetDataTable(mOSStr, connstring);
                        if (smDt.Rows.Count > 0)
                        {
                            mCrOverSrl = (smDt.Rows[0][0].ToString() == null) ? "0" : smDt.Rows[0][0].ToString();
                        }

                        if (mCrOverSrl != "")
                        {
                            decimal mOnAccAmt = 0;
                            string mStr2 = @"Select Sum(Credit) from Ledger Where Credit <> 0 and Branch='" + Model.Branch + "' and (MainType <> 'MV' and MainType<>'PV') and Code='" + Model.Account + "' and ((Credit)-isnull((Select sum(o.Amount) from Outstanding o where o.TableRefKey=Ledger.TableKey),0)) <> 0 and DocDate<='" + MMDDYY(Model.DocDate) + "'";
                            DataTable smDt2 = GetDataTable(mStr2, connstring);
                            if (smDt2.Rows.Count > 0)
                            {
                                mOnAccAmt = (smDt2.Rows[0][0].ToString() == null || smDt2.Rows[0][0].ToString() == "") ? 0 : Convert.ToDecimal(smDt2.Rows[0][0].ToString());
                            }

                            decimal mOutPending = 0;
                            string mStr3 = @"Select Sum(Debit) from Ledger Where Debit <> 0 and Branch='" + Model.Branch + "' and (MainType <> 'MV' and MainType <> 'PV') and Code='" + Model.Account + "' and ((Credit)-isnull((Select sum(o.Amount) from Outstanding o where o.TableRefKey=Ledger.TableKey),0)) <> 0 and DocDate+CrPeriod<='" + MMDDYY(Model.DocDate) + "'";
                            DataTable smDt3 = GetDataTable(mStr3, connstring);
                            if (smDt3.Rows.Count > 0)
                            {
                                mOutPending = (smDt3.Rows[0][0].ToString() == null || smDt3.Rows[0][0].ToString() == "") ? 0 : Convert.ToDecimal(smDt3.Rows[0][0].ToString());
                            }

                            int mDays = 0;
                            if (mOnAccAmt < mOutPending)
                            {
                                mMessage = mMessage + "\nThe Invoice " + mCrOverSrl + " is Pending for >" + mDays + " Days..";
                            }
                        }
                    }
                }
            }
            else if (Model.Mode == "Edit")
            {
                // if CreditLimit Check compulsory in case of Credit Sales
                if (mCreditCheck == true && mCrLimitWarn == true && (Model.SubType == "RS" || Model.SubType == "OC" || Model.SubType == "OS"))
                {
                    //if (mCreditLimit + Model.Amt > mCreditLimit + (mCreditLimit * mCrLimitTole / 100))
                    //    mMessage = mMessage + "\nParty will Cross Credit Limit..";

                    mBalance = GetBalance(Model.Account, DateTime.Now, Model.Branch);
                    if ((mBalance - Model.PrevInvAmt + Model.Amt) > mCreditLimit + (mCreditLimit * mCrLimitTole / 100))
                    {
                        mMessage = mMessage + "\nParty will Cross Credit Limit..";
                    }

                }
            }

            if (Model.SubType == "OP")
            {
                xCnt = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == Model.Type && x.AuthRule == 5).Select(z => z.AuthRule).FirstOrDefault() ?? 0;
                if (xCnt == 5)
                {
                    // if picked from sales order
                    var mList = mresult.Where(x => x.tempIsDeleted != true && x.OrdKey != "").ToList();
                    if (mList.Count() > 0)
                    {
                        foreach (var mobj in mList)
                        {
                            double mOrdRate = (double)ctxTFAT.OrdersStk.Where(x => x.TableKey == Model.OrdKey).Select(z => (z.Qty * z.Rate - (double)z.DiscAmt) / z.Qty).FirstOrDefault();
                            if (mOrdRate < mobj.Rate)
                                mMessage = mMessage + "\nPurchase Rate Should not be Greater than Sales Rate..\nProduct: " + mobj.Code;
                        }
                    }
                }
            }
            // addons validations for compulsory input
            if (Model.AddOnList != null && Model.AddOnList.Count > 0)
            {
                var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%Doc" + Model.MainType && x.Hide == false && x.Types.Contains(Model.Type) && x.Reqd == true)
                              select i.Fld).ToList();
                foreach (var mobj in addons)
                {
                    string mval = Model.AddOnList.Where(z => z.Fld == mobj).Select(x => x.ApplCode).FirstOrDefault() ?? "";
                    if (mval == "")
                        mMessage = mMessage + "\nAddon Input is Required for.." + mobj;
                }
            }


            return mMessage;
        }

        public string CheckPrimaryKey(LRInvoiceVM Model)
        {
            string mMessage = "";
            string mTable = "";

            if ((Model.SubType == "ES") || (Model.SubType == "EP"))
            {
                mTable = "Enquiry";
            }
            if (Model.SubType == "QS" || Model.SubType == "QP")
            {
                mTable = "Quote";
            }
            if ((Model.SubType == "OS") || (Model.SubType == "OP"))
            {
                mTable = "Orders";
            }
            if (Model.SubType == "PI")
            {
                mTable = "pInvoice";
            }
            if ((Model.SubType == "CP") || (Model.SubType == "IC") || (Model.SubType == "RP") || (Model.SubType == "NP") || (Model.SubType == "IM") || (Model.SubType == "PX") || (Model.SubType == "GP"))
            {
                mTable = "Purchase";
            }
            if ((Model.SubType == "OC") || (Model.SubType == "RS") || (Model.SubType == "CS") || Model.SubType == "XS" || Model.SubType == "SX" || Model.SubType == "NS")
            {
                mTable = "Sales";
            }
            var checkPkQuery = @"select tablekey from " + mTable + " where tablekey=" + "'" + Model.ParentKey + "' and branch='" + Model.Branch + "'";
            DataTable mDt2 = GetDataTable(checkPkQuery, GetConnectionString());
            if (mDt2.Rows.Count > 0)
            {
                mMessage = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? "" : mDt2.Rows[0][0].ToString();
                if (mMessage != "")
                {
                    mMessage = "Document Number Already Exists. Key is " + mMessage;
                }
            }
            return mMessage;
        }

        public void DeUpdate(LRInvoiceVM Model)
        {
            string connstring = GetConnectionString();
            var mobjList = ctxTFAT.LRBill.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).ToList();
            if (mobjList != null)
            {
                ctxTFAT.LRBill.RemoveRange(mobjList);
            }
            var mobj2 = ctxTFAT.SalesMore.Where(x => x.TableKey == Model.ParentKey && x.Branch == Model.Branch).FirstOrDefault();
            if (mobj2 != null)
            {
                ctxTFAT.SalesMore.Remove(mobj2);
            }

            var mobj1 = ctxTFAT.Sales.Where(x => x.TableKey == Model.ParentKey && x.Branch == Model.Branch).FirstOrDefault();
            if (mobj1 != null)
            {
                ctxTFAT.Sales.Remove(mobj1);
            }
            var mobj11 = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).ToList();
            if (mobj11 != null)
            {
                ctxTFAT.Ledger.RemoveRange(mobj11);
            }

            var mDeleteNote = ctxTFAT.Narration.Where(x => x.TableKey == Model.ParentKey).ToList();
            if (mDeleteNote != null)
            {
                ctxTFAT.Narration.RemoveRange(mDeleteNote);
            }


            var mDeleteAttach = ctxTFAT.Attachment.Where(x => x.ParentKey == Model.ParentKey).ToList();
            if (mDeleteAttach != null)
            {
                ctxTFAT.Attachment.RemoveRange(mDeleteAttach);
            }

            var mDeleteAuthorise = ctxTFAT.Authorisation.Where(x => x.ParentKey == Model.ParentKey).ToList();
            if (mDeleteAuthorise != null)
            {
                ctxTFAT.Authorisation.RemoveRange(mDeleteAuthorise);
            }


            var addond = ctxTFAT.AddonDocSL.Where(x => x.TableKey == Model.ParentKey).Select(x => x).FirstOrDefault();
            if (addond != null)
            {
                ctxTFAT.AddonDocSL.Remove(addond);
            }
            var mtdspayment = ctxTFAT.TDSPayments.Where(x => x.ParentKey == Model.ParentKey).Select(x => x).FirstOrDefault();
            if (mtdspayment != null)
            {
                ctxTFAT.TDSPayments.Remove(mtdspayment);
            }

            var moutstanding = ctxTFAT.Outstanding.Where(x => x.ParentKey == Model.ParentKey && x.DocBranch == Model.Branch).Select(x => x).ToList();
            if (moutstanding != null)
            {
                ctxTFAT.Outstanding.RemoveRange(moutstanding);
            }
            var mPodmASTER = ctxTFAT.PODMaster.Where(x => x.ParentKey == Model.ParentKey).Select(x => x).ToList();
            if (mPodmASTER != null)
            {
                ctxTFAT.PODMaster.RemoveRange(mPodmASTER);
            }
            var mPODtablekeys = mPodmASTER.Select(x => x.TableKey).ToList();
            var mPodmASTERREl = ctxTFAT.PODRel.Where(x => mPODtablekeys.Contains(x.ParentKey)).Select(x => x).ToList();
            if (mPodmASTERREl != null)
            {
                ctxTFAT.PODRel.RemoveRange(mPodmASTERREl);
            }
            ctxTFAT.SaveChanges();
        }

        public ActionResult SaveData(LRInvoiceVM Model)
        {
            string mTable = "";
            string brMessage = "";

            using (var transaction = ctxTFAT.Database.BeginTransaction(System.Data.IsolationLevel.Unspecified))
            {
                try
                {

                    // REMOVE EXISTING DATA FOR EDIT && DELETE MODE
                    if (Model.Mode == "Edit")
                    {
                        if (mbranchcode != Model.Branch)
                        {
                            transaction.Rollback();
                            return Json(new { Message = "Login Branch Does Not Match With Documnet Branch.", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                        }
                        DeUpdate(Model);

                        Model.ParentKey = Model.ParentKey;

                    }

                    if (Model.Mode == "Add")
                    {

                        if (Model.LRGenerate == "M")
                        {
                            var result1 = ctxTFAT.DocTypes.Where(x => x.Code == "CMM00").Select(x => x).FirstOrDefault();
                            Model.Srl = Model.Srl.PadLeft(result1.DocWidth, '0');
                        }
                        else
                        {
                            Model.Srl = GetLastSerial(Model.TableName, Model.Branch, Model.Type, Model.Prefix, Model.SubType, ConvertDDMMYYTOYYMMDD(Model.DocuDate));
                        }


                        Model.ParentKey = mbranchcode+ Model.Type + Model.Prefix.Substring(0, 2) + Model.Srl.ToString(); ;
                        string pkStr = "";

                        pkStr = CheckPrimaryKey(Model);
                        if (pkStr != "")
                        {
                            return Json(new
                            {
                                Message = pkStr,
                                Status = "PrimaryKeyError"
                            }, JsonRequestBehavior.AllowGet);
                        }
                        AlertNoteMaster alertNoteMaster = ctxTFAT.AlertNoteMaster.Where(x => x.Type == "CMM00" && x.ParentKey.Trim() == Model.ParentKey.Trim() && x.Stop.Contains("CMM00")).FirstOrDefault();
                        if (alertNoteMaster != null)
                        {
                            return Json(new
                            {
                                Message = Model.ParentKey.Trim(),
                                Status = "AutoDocumentAlert"
                            }, JsonRequestBehavior.AllowGet);
                        }

                        AttachmentVM vM = new AttachmentVM();
                        vM.ParentKey = Model.ParentKey;
                        vM.Srl = Model.Srl.ToString();
                        vM.Type = "CMM00";
                        SaveAttachment(vM);

                        SaveNarrationAdd(Model.Srl.ToString(), Model.ParentKey);
                    }

                    if (Model.Authorise.Substring(0, 1) == "X" || Model.Authorise.Substring(0, 1) == "R" || Model.Mode == "Add" || (Model.Mode == "Edit" && Model.AuthAgain))
                    {
                        if (Model.Authorise.Substring(0, 1) != "N") //if authorised then check for the RateDiff Auth. Rule
                            Model.Authorise = (Model.AuthReq == true ? GetAuthorise(Model.Type, 0, Model.Branch) : Model.Authorise = "A00");
                    }

                    List<LRInvoiceVM> result = new List<LRInvoiceVM>();
                    result = (List<LRInvoiceVM>)Session["CNewItemlist"];


                    if (Model.Mode == "Add" || Model.Mode == "Edit")
                    {
                        //var CheckTypeBranch = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => x.AppBranch).FirstOrDefault();
                        //if (Model.Branch != CheckTypeBranch)
                        //{
                        //    brMessage = brMessage + "\nDocument Type is Not of Current Branch Cant Save..";
                        //}
                        //if (brMessage != "")
                        //{
                        //    return Json(new
                        //    {
                        //        Message = brMessage,
                        //        Status = "CancelError"
                        //    }, JsonRequestBehavior.AllowGet);
                        //}


                        int xCnt = 1; // used for counts
                        int mFirstCount = 1;


                        string ourstatename = ctxTFAT.Warehouse.Where(x => x.Code == Model.LocationCode).Select(x => x.State).FirstOrDefault();

                        int posneg = 1;//Stock ,stock tax,stock more table
                        if (Model.SubType == "NP" || Model.SubType == "PX" || Model.SubType == "OC" || Model.SubType == "RS" || Model.SubType == "CS" || Model.SubType == "XS")
                        {
                            posneg = -1;
                        }
                        else if (Model.SubType == "IC" || Model.SubType == "CP" || Model.SubType == "RP" || Model.SubType == "IM" || Model.SubType == "GP" || Model.SubType == "NS" || Model.SubType == "SX")
                        {
                            posneg = 1;

                        }

                        var partyaddress = ctxTFAT.Address.Where(x => x.Code == Model.Account).Select(x => new { x.DealerType, x.GSTNo }).FirstOrDefault();
                        int mamtvalneg;// purchase ,sales table
                        if (Model.SubType == "NP" || Model.SubType == "PX" || Model.SubType == "SX" || Model.SubType == "NS")
                        {
                            mamtvalneg = -1;
                        }
                        else
                        {
                            mamtvalneg = 1;
                        }


                        #region Other Sales Menu
                        if ((Model.SubType == "OC") || (Model.SubType == "RS") || (Model.SubType == "CS") || Model.SubType == "XS" || Model.SubType == "SX" || Model.SubType == "NS")
                        {
                            mTable = "Sales";
                            #region Normal Sales
                            if (result != null)
                            {
                                var list = result.ToList();

                                //save data in table Quatetion
                                Sales mobj1 = new Sales();
                                mobj1.AltAddress = Convert.ToByte(Model.AltAddress);
                                mobj1.Amt = Model.Amt * mamtvalneg;
                                mobj1.AUTHIDS = muserid;
                                mobj1.AUTHORISE = Model.Authorise;
                                mobj1.BillDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                mobj1.BillNumber = (Model.BillNumber == null || Model.BillNumber.Trim() == "") ? Model.Srl : Model.BillNumber;
                                mobj1.Branch = Model.Branch;
                                mobj1.Broker = "";
                                mobj1.Cess = Convert.ToDecimal(0.00) * mamtvalneg;
                                mobj1.ChlnDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                mobj1.ChlnNumber = "";
                                mobj1.CashBankCode = Model.Account;
                                mobj1.CrPeriod = Model.CrPeriod;
                                mobj1.CurrAmount = Model.Amt * mamtvalneg;
                                mobj1.CurrName = "1";
                                mobj1.CurrRate = 1;
                                mobj1.DelyAltAdd = Convert.ToByte(Model.DelyAltAdd);
                                mobj1.Delycode = (Model.DelyCode == null || Model.DelyCode.Trim() == "") ? Model.Account : Model.DelyCode;
                                mobj1.DelyDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                mobj1.BillContact = Model.BillContact;
                                mobj1.DelyContact = Model.DelyContact;
                                mobj1.Disc = Convert.ToDecimal(0.00) * mamtvalneg;
                                mobj1.Discount = Convert.ToDecimal(0.00) * mamtvalneg;
                                mobj1.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                mobj1.InsuranceNo = "";
                                mobj1.LocationCode = Model.LocationCode;
                                mobj1.MainType = Model.MainType;
                                mobj1.Narr = Model.BillNarr;
                                mobj1.Prefix = Model.Prefix;
                                mobj1.Qty = Convert.ToDecimal(list.Select(x => x.Qty).Sum()) * mamtvalneg;
                                mobj1.Qty2 = list.Select(x => x.Qty2).Sum() * mamtvalneg;
                                mobj1.Srl = (Model.Srl);
                                mobj1.SubType = Model.SubType;
                                mobj1.Taxable = Convert.ToDecimal(list.Select(x => x.Val1).Sum()) * mamtvalneg;
                                mobj1.TaxAmt = Convert.ToDecimal(0.00) * mamtvalneg;
                                mobj1.TaxCode = Model.GSTCode;
                                mobj1.Type = Model.Type;
                                mobj1.ENTEREDBY = muserid;
                                mobj1.LASTUPDATEDATE = DateTime.Now;
                                mobj1.CGSTAmt = Convert.ToDecimal(0.00) * mamtvalneg;
                                mobj1.GSTNoITC = (Model.GSTNoITC == false) ? 0 : 1;
                                mobj1.GSTType = (Model.GSTType == null) ? 0 : Convert.ToInt32(Model.GSTType);
                                mobj1.CGSTAmt = Model.CGSTAmt * mamtvalneg;
                                mobj1.IGSTAmt = Model.IGSTAmt * mamtvalneg;
                                mobj1.SGSTAmt = Model.SGSTAmt * mamtvalneg;
                                mobj1.SalesMan = "";
                                mobj1.TDSCode = 0;
                                mobj1.CompCode = mcompcode;
                                mobj1.TableKey = Model.ParentKey;
                                mobj1.RoundOff = Model.RoundOff * mamtvalneg;//check sds
                                mobj1.SourceDoc = Model.SourceDoc;
                                mobj1.PCCode = 100002;
                                mobj1.DCCode = 100001;
                                mobj1.Val1 = 0;
                                mobj1.Val2 = 0;
                                mobj1.Val3 = 0;
                                mobj1.Val4 = 0;
                                mobj1.Val5 = 0;
                                mobj1.Val6 = 0;
                                mobj1.Val7 = 0;
                                mobj1.Val8 = 0;
                                mobj1.Val9 = 0;
                                mobj1.Val10 = 0;
                                mobj1.ChqNo = Model.ChequeNo;

                                mobj1.Amt1 = 0;
                                mobj1.Amt2 = 0;
                                mobj1.Amt3 = 0;
                                mobj1.Amt4 = 0;
                                mobj1.Amt5 = 0;
                                mobj1.Amt6 = 0;
                                mobj1.Amt7 = 0;
                                mobj1.Amt8 = 0;
                                mobj1.Amt9 = 0;
                                mobj1.Amt10 = 0;
                                mobj1.ReasonCode = 0;
                                mobj1.PlaceOfSupply = Model.PlaceOfSupply;
                                mobj1.LoadingKey = "";
                                mobj1.ShipFrom = Model.ShipFrom;
                                mobj1.CustGroup = Model.AccParentGrp;
                                mobj1.Code = Model.Customer;

                                ctxTFAT.Sales.Add(mobj1);



                                // sales more
                                SalesMore mobj2 = new SalesMore();
                                mobj2.TableKey = Model.ParentKey;

                                mobj2.Branch = Model.Branch;

                                mobj2.DeliverBy = Model.DeliverBy;
                                mobj2.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);

                                mobj2.InvoiceQty = 0;

                                mobj2.LocationCode = Model.LocationCode;
                                mobj2.MainType = Model.MainType;
                                mobj2.Prefix = Model.Prefix;


                                mobj2.Reason = "0";
                                mobj2.ReceiveBy = "";
                                mobj2.RefBy = "";
                                mobj2.RefDoc = "";
                                mobj2.Reference = "";

                                mobj2.RefSno = 0;
                                mobj2.RefParty = Model.Account;

                                mobj2.SalePurchNumber = "";
                                mobj2.Srl = Model.Srl;
                                mobj2.SubType = Model.SubType;

                                mobj2.TDSAble = 0;
                                mobj2.TDSAmt = 0;
                                mobj2.TDSCess = 0;
                                mobj2.TDSFlag = false;
                                mobj2.TDSReason = "";
                                mobj2.TDSSchg = 0;
                                mobj2.TDSTax = 0;
                                mobj2.Type = Model.Type;
                                mobj2.WONumber = "";
                                mobj2.ENTEREDBY = muserid;
                                mobj2.AUTHIDS = muserid;
                                mobj2.AUTHORISE = "A00";
                                mobj2.LASTUPDATEDATE = DateTime.Now;

                                ctxTFAT.SalesMore.Add(mobj2);

                                xCnt = 1;

                                foreach (var li in list)
                                {
                                    //if (Model.Mode == "Edit")
                                    //{ xCnt = li.tempId; }

                                    LRBill lb = new LRBill();
                                    lb.Amt = li.Val1;
                                    lb.BalQty = Convert.ToInt32(li.TotalQty) - Convert.ToInt32(li.Qty);
                                    lb.Branch = Model.Branch;
                                    lb.Code = li.LRName;
                                    lb.DocDate = li.LRDocDate;
                                    lb.Freight = li.Freight;
                                    lb.LrNo = li.Code;
                                    lb.Narr = li.BillNarr;
                                    lb.ParentKey = Model.ParentKey;
                                    lb.Prefix = Model.Prefix;
                                    lb.Sno = xCnt;
                                    lb.Srl = Model.Srl;
                                    lb.TotQty = Convert.ToInt32(li.Qty);
                                    lb.TrType = true;
                                    lb.Type = Model.Type;
                                    lb.LRRefTablekey = li.LRRefTableKey;

                                    lb.Val1 = li.LRChargeList.Where(x => x.Fld == "F001").Select(x => x.Amt).FirstOrDefault();
                                    lb.Val10 = li.LRChargeList.Where(x => x.Fld == "F010").Select(x => x.Amt).FirstOrDefault();
                                    lb.Val2 = li.LRChargeList.Where(x => x.Fld == "F002").Select(x => x.Amt).FirstOrDefault();
                                    lb.Val3 = li.LRChargeList.Where(x => x.Fld == "F003").Select(x => x.Amt).FirstOrDefault();
                                    lb.Val4 = li.LRChargeList.Where(x => x.Fld == "F004").Select(x => x.Amt).FirstOrDefault();
                                    lb.Val5 = li.LRChargeList.Where(x => x.Fld == "F005").Select(x => x.Amt).FirstOrDefault();
                                    lb.Val6 = li.LRChargeList.Where(x => x.Fld == "F006").Select(x => x.Amt).FirstOrDefault();
                                    lb.Val7 = li.LRChargeList.Where(x => x.Fld == "F007").Select(x => x.Amt).FirstOrDefault();
                                    lb.Val8 = li.LRChargeList.Where(x => x.Fld == "F008").Select(x => x.Amt).FirstOrDefault();
                                    lb.Val9 = li.LRChargeList.Where(x => x.Fld == "F009").Select(x => x.Amt).FirstOrDefault();

                                    lb.Val11 = li.LRChargeList.Where(x => x.Fld == "F011").Select(x => x.Amt).FirstOrDefault();
                                    lb.Val12 = li.LRChargeList.Where(x => x.Fld == "F012").Select(x => x.Amt).FirstOrDefault();
                                    lb.Val13 = li.LRChargeList.Where(x => x.Fld == "F013").Select(x => x.Amt).FirstOrDefault();
                                    lb.Val14 = li.LRChargeList.Where(x => x.Fld == "F014").Select(x => x.Amt).FirstOrDefault();
                                    lb.Val15 = li.LRChargeList.Where(x => x.Fld == "F015").Select(x => x.Amt).FirstOrDefault();
                                    lb.Val16 = li.LRChargeList.Where(x => x.Fld == "F016").Select(x => x.Amt).FirstOrDefault();
                                    lb.Val17 = li.LRChargeList.Where(x => x.Fld == "F017").Select(x => x.Amt).FirstOrDefault();
                                    lb.Val18 = li.LRChargeList.Where(x => x.Fld == "F018").Select(x => x.Amt).FirstOrDefault();
                                    lb.Val19 = li.LRChargeList.Where(x => x.Fld == "F019").Select(x => x.Amt).FirstOrDefault();
                                    lb.Val20 = li.LRChargeList.Where(x => x.Fld == "F020").Select(x => x.Amt).FirstOrDefault();
                                    lb.Val21 = li.LRChargeList.Where(x => x.Fld == "F021").Select(x => x.Amt).FirstOrDefault();
                                    lb.Val22 = li.LRChargeList.Where(x => x.Fld == "F022").Select(x => x.Amt).FirstOrDefault();
                                    lb.Val23 = li.LRChargeList.Where(x => x.Fld == "F023").Select(x => x.Amt).FirstOrDefault();
                                    lb.Val24 = li.LRChargeList.Where(x => x.Fld == "F024").Select(x => x.Amt).FirstOrDefault();
                                    lb.Val25 = li.LRChargeList.Where(x => x.Fld == "F025").Select(x => x.Amt).FirstOrDefault();

                                    lb.ENTEREDBY = muserid;
                                    lb.LASTUPDATEDATE = DateTime.Now;
                                    lb.AUTHORISE = Model.Authorise;
                                    lb.AUTHIDS = muserid;
                                    lb.TableKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString();
                                    lb.POD = li.POD;

                                    //if (Model.Mode == "Add")
                                    //{ ++xCnt; }

                                    ctxTFAT.LRBill.Add(lb);


                                    if (li.POD == true)
                                    {
                                        //int mpod = GetMaxPOD();
                                        var mlrmaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == li.LRRefTableKey).Select(x => x).FirstOrDefault();
                                        DataTable PODtbl = new DataTable();
                                        using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                                        {
                                            string sql = string.Format(@"WITH ranked_messages AS(	select ROW_NUMBER() OVER (PARTITION BY PODRel.LRRefTablekey ORDER BY PODREL.Tablekey DESC) AS rn,PODM.PODNo as PODNO,LR.LrNo as Lrno,LR.Time as LRTime,Convert(char(10),LR.BookDate, 103) as LRDate, (select C.Name from Consigner C where C.code= LR.RecCode) as ConsignerName,(select C.Name from Consigner C where C.code=LR.SendCode) as ConsigneeName,(select T.Name from TfatBranch T where T.code=LR.Source) as FromName,(select T.Name from TfatBranch T where T.code=LR.Dest) as ToName,PODM.PODRemark as Remark,PODREL.TableKey as PODRefTablekey,PODM.AUTHORISE as Authorise,PODRel.LRRefTablekey as ConsignmentKey,PODM.CurrentBranch,PODM.SendReceive,PODREL.FromBranch,PODREL.ToBranch 	from PODMaster PODM	join PODRel PODREL on PODM.TableKey=PODREL.ParentKey join LRMaster LR on PODRel.LRRefTablekey=LR.TableKey  where PODREL.SendReceive in ('S','R') and PODREL.LRRefTablekey='" + mlrmaster.TableKey + "' ) SELECT *  FROM ranked_messages WHERE rn = 1;");
                                            SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                                            da.Fill(PODtbl);
                                        }
                                        bool GenerateFreshPOD = true;
                                        List<ListOfPod> PODList = new List<ListOfPod>();
                                        if (PODtbl.Rows.Count > 0)
                                        {
                                            PODList = GetDatatableToPodList(PODtbl);
                                            GenerateFreshPOD = false;
                                        }
                                        if (GenerateFreshPOD)
                                        {
                                            PODMaster pod = new PODMaster();
                                            pod.CreateDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                            pod.PODNo = Convert.ToInt32(GetNewCode());
                                            pod.CurrentBranch = Model.Branch;
                                            //pod.FromBranch = Model.Branch;
                                            pod.Task = "Customer";
                                            pod.SendReceive = "C";
                                            pod.ModuleName = "POD Send Customer";
                                            pod.Prefix = Model.Prefix;
                                            pod.TableKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString();
                                            pod.ParentKey = Model.ParentKey;
                                            pod.PODDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                            pod.PODTime = DateTime.Now.ToString("HH:mm");
                                            //pod.PODRemark = "Send POD At Time Bill Generate...! " + Model.Type + Model.Prefix.Substring(0, 2) + Model.Srl.ToString();
                                            pod.AUTHIDS = muserid;
                                            pod.AUTHORISE = "A00";
                                            pod.CustCode = Model.Account;
                                            pod.DeliveryDate = null;
                                            pod.DeliveryNo = null;
                                            pod.DeliveryRemark = null;
                                            pod.DeliveryTime = null;
                                            pod.ENTEREDBY = muserid;
                                            pod.LASTUPDATEDATE = DateTime.Now;
                                            ctxTFAT.PODMaster.Add(pod);
                                            ctxTFAT.SaveChanges();


                                            PODRel podrel = new PODRel();
                                            podrel.Task = "Customer";
                                            podrel.SendReceive = "C";
                                            podrel.PODNo = pod.PODNo.Value;
                                            //podrel.ChildNo = pod.PODNo.Value;
                                            podrel.LrNo = Convert.ToInt32(li.Code);
                                            podrel.FromBranch = Model.Branch;
                                            podrel.ToBranch = Model.Branch;
                                            podrel.TableKey = mbranchcode + "PODRL" + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString();
                                            podrel.ParentKey = pod.TableKey;
                                            podrel.LRRefTablekey = mlrmaster.TableKey;
                                            podrel.PODRefTablekey = "";
                                            podrel.AUTHIDS = muserid;
                                            podrel.AUTHORISE = "A00";
                                            podrel.ENTEREDBY = muserid;
                                            podrel.LASTUPDATEDATE = DateTime.Now;
                                            podrel.Prefix = Model.Prefix;
                                            ctxTFAT.PODRel.Add(podrel);
                                        }
                                        else
                                        {
                                            PODMaster pod = new PODMaster();
                                            pod.CreateDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                            pod.PODNo = Convert.ToInt32(GetNewCode());
                                            pod.CurrentBranch = Model.Branch;
                                            //pod.FromBranch = Model.Branch;
                                            pod.Task = "Customer";
                                            pod.SendReceive = "C";
                                            pod.ModuleName = "POD Send Customer";
                                            pod.Prefix = Model.Prefix;
                                            pod.TableKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString();
                                            pod.ParentKey = Model.ParentKey;
                                            pod.PODDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                            pod.PODTime = DateTime.Now.ToString("HH:mm");
                                            //pod.PODRemark = "Send POD At Time Bill Generate...! " + Model.Type + Model.Prefix.Substring(0, 2) + Model.Srl.ToString();
                                            pod.AUTHIDS = muserid;
                                            pod.AUTHORISE = "A00";
                                            pod.CustCode = Model.Account;
                                            pod.DeliveryDate = null;
                                            pod.DeliveryNo = null;
                                            pod.DeliveryRemark = null;
                                            pod.DeliveryTime = null;
                                            pod.ENTEREDBY = muserid;
                                            pod.LASTUPDATEDATE = DateTime.Now;
                                            ctxTFAT.PODMaster.Add(pod);
                                            ctxTFAT.SaveChanges();

                                            ListOfPod listOfPod = PODList.FirstOrDefault();

                                            PODRel podrel = new PODRel();
                                            podrel.Task = "Customer";
                                            podrel.SendReceive = "C";
                                            podrel.PODNo = pod.PODNo.Value;
                                            //podrel.ChildNo = listOfPod.PODNO;
                                            podrel.LrNo = Convert.ToInt32(li.Code);
                                            podrel.FromBranch = Model.Branch;
                                            podrel.ToBranch = Model.Branch;
                                            podrel.TableKey = mbranchcode + "PODRL" + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString();
                                            podrel.ParentKey = pod.TableKey;
                                            podrel.LRRefTablekey = mlrmaster.TableKey;
                                            podrel.PODRefTablekey = listOfPod.PODRefTablekey;
                                            podrel.AUTHIDS = muserid;
                                            podrel.AUTHORISE = "A00";
                                            podrel.ENTEREDBY = muserid;
                                            podrel.LASTUPDATEDATE = DateTime.Now;
                                            podrel.Prefix = Model.Prefix;
                                            ctxTFAT.PODRel.Add(podrel);
                                        }

                                        //mlrmaster.POD = !li.POD;

                                        //ctxTFAT.Entry(mlrmaster).State = EntityState.Modified;

                                    }
                                    ++xCnt;
                                }

                            }
                            int lCnt = 1;
                            if (Model.SubType == "RS" || Model.SubType == "CS" || Model.SubType == "XS" || Model.SubType == "NS")
                            {
                                var ledpost = Model.LedgerPostList;
                                for (int u = 0; u < ledpost.Count; u++)
                                {
                                    Ledger mobjL = new Ledger();
                                    mobjL.AltCode = ledpost[u].DelyCode;
                                    mobjL.Audited = true;
                                    mobjL.AUTHIDS = muserid;
                                    mobjL.AUTHORISE = Model.Authorise;
                                    mobjL.BillDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                    mobjL.BillNumber = (Model.BillNumber == null || Model.BillNumber.Trim() == "") ? Model.Srl : Model.BillNumber;
                                    mobjL.Branch = Model.Branch;
                                    mobjL.Cheque = Model.ChequeNo;
                                    mobjL.ChequeReturn = false;
                                    mobjL.ChqCategory = 1;
                                    mobjL.ClearDate = DateTime.Now;
                                    mobjL.Code = ledpost[u].Code;
                                    mobjL.Credit = Convert.ToDecimal(ledpost[u].Credit);
                                    mobjL.CrPeriod = Model.CrPeriod;
                                    mobjL.CurrAmount = Convert.ToDecimal(ledpost[u].Credit + ledpost[u].Debit);
                                    mobjL.CurrName = 1;
                                    mobjL.CurrRate = 1;
                                    mobjL.Debit = Convert.ToDecimal(ledpost[u].Debit);
                                    mobjL.Discounted = true;
                                    mobjL.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                    mobjL.DueDate = DateTime.Now;
                                    mobjL.LocationCode = Model.LocationCode;
                                    mobjL.MainType = Model.MainType;
                                    mobjL.Narr = Model.RichNote;
                                    mobjL.Party = (ledpost[u].Code == Model.AccParentGrp) ? Model.Account : ledpost[u].Code;
                                    mobjL.Prefix = Model.Prefix;
                                    mobjL.RecoFlag = "";
                                    mobjL.RefDoc = "";
                                    mobjL.Reminder = true;
                                    mobjL.Sno = ledpost[u].tempId;
                                    mobjL.Srl = Model.Srl.ToString();
                                    mobjL.SubType = Model.SubType;
                                    mobjL.TaskID = 0;
                                    mobjL.TDSChallanNumber = "";
                                    mobjL.TDSCode = Model.TDSCode == null ? 0 : Convert.ToInt32(Model.TDSCode);
                                    mobjL.TDSFlag = Model.TDSFlag;
                                    mobjL.Type = Model.Type;
                                    mobjL.ENTEREDBY = muserid;
                                    mobjL.LASTUPDATEDATE = DateTime.Now;
                                    mobjL.ChequeDate = DateTime.Now;
                                    mobjL.CompCode = mcompcode;
                                    mobjL.ParentKey = Model.ParentKey;
                                    mobjL.TableKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + lCnt.ToString("D3") + Model.Srl.ToString(); ;
                                    mobjL.PCCode = 100002;
                                    mobjL.CGSTAmt = Model.CGSTAmt;
                                    mobjL.CGSTRate = Model.CGSTRate;
                                    mobjL.IGSTAmt = Model.IGSTAmt;
                                    mobjL.IGSTRate = Model.IGSTRate;
                                    mobjL.SGSTAmt = Model.SGSTAmt;
                                    mobjL.SGSTRate = Model.SGSTRate;
                                    mobjL.TaxCode = Model.GSTCode;

                                    ctxTFAT.Ledger.Add(mobjL);


                                    ++lCnt;
                                }
                            }


                            #endregion

                        }
                        #endregion

                        SaveAddons(Model);
                        SaveNarration(Model, Model.ParentKey);
                        if (Model.TDSFlag == true && String.IsNullOrEmpty(Model.TDSCode) == false)
                        {
                            SaveTDSPayments(Model);
                        }
                        ctxTFAT.SaveChanges();
                        transaction.Commit();
                        transaction.Dispose();

                        var mpods = ctxTFAT.PODMaster.Where(x => x.ParentKey == (mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + Model.Srl)).Select(x => x).ToList();
                        if (mpods != null && mpods.Count > 0)
                        {
                            foreach (var p in mpods)
                            {
                                var msno = p.TableKey.Substring(7, 3);

                                p.TableKey = mbranchcode + "POS00" + Model.Prefix.Substring(0, 2) + msno + p.RECORDKEY.ToString("D6");
                                p.ParentKey = mbranchcode + "POS00" + Model.Prefix.Substring(0, 2) + p.RECORDKEY.ToString("D6");
                                ctxTFAT.Entry(p).State = EntityState.Modified;
                                ctxTFAT.SaveChanges();
                            }
                        }

                        var mpodrels = ctxTFAT.PODRel.Where(x => x.ParentKey == (mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + Model.Srl)).Select(x => x).ToList();
                        if (mpodrels != null && mpodrels.Count > 0)
                        {
                            foreach (var pr in mpodrels)
                            {
                                var msno = pr.TableKey.Substring(7, 3);

                                pr.TableKey = mbranchcode + "POS00" + Model.Prefix.Substring(0, 2) + msno + pr.PODNo.ToString("D6");
                                ctxTFAT.Entry(pr).State = EntityState.Modified;
                                ctxTFAT.SaveChanges();
                            }
                        }

                        UpdateAuditTrail(Model.Branch, Model.Mode, Model.Header, Model.ParentKey, ConvertDDMMYYTOYYMMDD(Model.DocuDate), Model.Amt, Model.Account, "Save Cash Memo", "A");
                        if (Model.Authorise.Substring(0, 1) != "A")
                        {
                            if (Model.Authorise.Substring(0, 1) != "X")
                            {
                                string mAuthUser;
                                if (Model.Authorise.Substring(0, 1) == "D")
                                {
                                    //mAuthUser=SaveAuthoriseDOC( Model.ParentKey, 0, Model.Date, mcurrency, Model.Branch, muserid);
                                }
                                else
                                {
                                    mAuthUser = SaveAuthorise(Model.ParentKey, Model.Amt, Model.DocuDate, 1, 1, DateTime.Now, Model.Account, Model.Branch, muserid, -1);
                                    SendAuthoriseMessage(mAuthUser, Model.ParentKey, Model.DocuDate, Model.Authorise, Model.AccountName);
                                }
                            }

                        }
                        //if (ctxTFAT.DocTypes.Where(z => z.Code == OriginalType).Select(x => x.SendAlert).FirstOrDefault() == true)
                        //{
                        //    SendPartywiseSMS(OriginalSubType, OriginalParentCode, Model.Account, Model.AltAddress, true, Model.Branch);
                        //}

                        if (Model.Authorise != "X00")
                        {
                            //SendTrnsMsg(Model.Mode, Model.Amt, Model.Branch + Model.ParentKey, ConvertDDMMYYTOYYMMDD(Model.DocuDate), Model.Account);
                        }

                        if (Model.SaveAsDraft != "Y")
                        {
                            Session["CNewItemlist"] = null;
                        }
                    }
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    return Json(new
                    {
                        Message = ex1.EntityValidationErrors.Select(x => x.ValidationErrors.Select(xa => xa.ErrorMessage).FirstOrDefault()),
                        Status = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    return Json(new
                    {
                        Message = ex.InnerException.InnerException.Message,
                        Status = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new
            {
                NewSrl = (Model.Branch + Model.ParentKey),
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }

        private List<ListOfPod> GetDatatableToPodList(DataTable tbl)
        {
            List<ListOfPod> Mobj = new List<ListOfPod>();
            foreach (DataRow item in tbl.Rows)
            {
                ListOfPod ofPod = new ListOfPod();
                ofPod.PODNO = Convert.ToInt32(item["PODNO"]);
                ofPod.Lrno = item["Lrno"].ToString();
                ofPod.LRTime = item["LRTime"].ToString();
                ofPod.LRDate = item["LRDate"].ToString();
                ofPod.ConsignerName = item["ConsignerName"].ToString();
                ofPod.ConsigneeName = item["ConsigneeName"].ToString();
                ofPod.FromName = item["FromName"].ToString();
                ofPod.ToName = item["ToName"].ToString();
                ofPod.Remark = item["Remark"].ToString();
                ofPod.PODRefTablekey = item["PODRefTablekey"].ToString();
                ofPod.LRRefTablekey = item["ConsignmentKey"].ToString();
                ofPod.CheckBox = false;
                ofPod.Authorise = item["Authorise"].ToString().Substring(0, 1);
                ofPod.CurrentBranch = item["CurrentBranch"].ToString();
                ofPod.SendReceive = item["SendReceive"].ToString();
                ofPod.FromBranch = item["FromBranch"].ToString();
                ofPod.ToBranch = item["ToBranch"].ToString();
                Mobj.Add(ofPod);
            }
            return Mobj;
        }

        public string GetCode()
        {
            string DocNo = "";

            DocNo = ctxTFAT.AlertNoteMaster.OrderByDescending(x => x.RECORDKEY).Select(x => x.DocNo).FirstOrDefault();

            if (String.IsNullOrEmpty(DocNo))
            {
                DocNo = "100000";
            }
            else
            {
                var Integer = Convert.ToInt32(DocNo) + 1;
                DocNo = Integer.ToString("D6");
            }

            return DocNo;
        }

        public void SaveNarrationAdd(string DocNo, string Parentkey)
        {
            List<AlertNoteMaster> objledgerdetail = new List<AlertNoteMaster>();

            if (Session["CommnNarrlist"] != null)
            {
                objledgerdetail = (List<AlertNoteMaster>)Session["CommnNarrlist"];
            }
            int Sno = Convert.ToInt32(GetCode());
            foreach (var item in objledgerdetail)
            {
                item.DocNo = Sno.ToString("D6");
                item.TypeCode = DocNo;
                item.TableKey = "ALERT" + mperiod.Substring(0, 2) + 1.ToString("D3") + item.DocNo;
                item.ParentKey = Parentkey;
                ctxTFAT.AlertNoteMaster.Add(item);
                ++Sno;
            }
        }

        public ActionResult DeleteData(LRInvoiceVM Model)
        {
            // Check for Active Documents
            string mactivestring = "";

            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "The Document is already Adjusted, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var Date = ConvertDDMMYYTOYYMMDD(Model.DocuDate);

            if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.Type == "CMM00" && x.LockDate == Date).FirstOrDefault() != null)
            {
                return Json(new
                {

                    Status = "Error",
                    Message = "Date is Locked By Period Lock System...!"
                }, JsonRequestBehavior.AllowGet); ;
            }
            using (var transaction = ctxTFAT.Database.BeginTransaction(System.Data.IsolationLevel.Unspecified))
            {
                try
                {
                    var mobj1 = ctxTFAT.Sales.Where(x => x.TableKey == Model.ParentKey && x.Branch == Model.Branch).FirstOrDefault();
                    if (mobj1 != null)
                    {
                        Model.Account = mobj1.CashBankCode;
                        var RemoveAttach = ctxTFAT.Attachment.Where(x => x.Type == "CMM00" && x.Srl == mobj1.Srl).ToList();
                        ctxTFAT.Attachment.RemoveRange(RemoveAttach);
                    }
                    DeUpdate(Model);
                    UpdateAuditTrail(Model.Branch, "Delete", Model.Header, Model.ParentKey, ConvertDDMMYYTOYYMMDD(Model.DocuDate), Model.Amt, Model.Account, "Delete Cash Memo", "A");

                    transaction.Commit();
                    transaction.Dispose();
                    //SendTrnsMsg("Delete", Model.Amt, Model.Branch + Model.ParentKey, ConvertDDMMYYTOYYMMDD(Model.DocuDate), "");
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
                Message = "The Document is Deleted."
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region LedgerPost

        public ActionResult GetPostingNew(LRInvoiceVM Model)
        {

            string mpostcode;
            int n;
            string mIGSTCode = "";
            string mCGSTCode = "";
            string mSGSTCode = "";
            decimal mMultTax = 0;

            decimal mvalue = 0;
            decimal mTaxable = 0;
            decimal mDebit = 0;
            decimal mCredit = 0;
            decimal mPostAmt = Model.Amt;

            string mCode = "";
            int xCnt = 0;
            string mTrx = "";
            var resultlrlist = (List<LRInvoiceVM>)Session["CNewItemlist"];
            if (resultlrlist == null || resultlrlist.Count() == 0)
            {
                return Json(new
                {

                    Status = "ErrorValid",
                    Message = "No lr list Found Cant Save"
                }, JsonRequestBehavior.AllowGet); ;
            }
            if (!String.IsNullOrEmpty(Model.Srl))
            {
                var resultw = ctxTFAT.DocTypes.Where(x => x.Code == "CMM00").Select(x => x).FirstOrDefault();
                if (Model.Srl.Length > (resultw.DocWidth))
                {
                    return Json(new
                    {

                        Status = "ErrorValid",
                        Message = "Document NO. Allow " + resultw.DocWidth + " Digit Only....!"
                    }, JsonRequestBehavior.AllowGet); ;
                }
            }




            var Date = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            var mtfatperd = ctxTFAT.TfatPerd.Where(x => x.PerdCode == mperiod).Select(x => new { x.StartDate, x.LastDate }).FirstOrDefault();
            DateTime mStartDate = Convert.ToDateTime(System.Web.HttpContext.Current.Session["StartDate"]);
            DateTime mLastDate = Convert.ToDateTime(System.Web.HttpContext.Current.Session["LastDate"]);

            if (mtfatperd.StartDate > Date || Date > mtfatperd.LastDate)
            {
                return Json(new
                {

                    Status = "ErrorValid",
                    Message = "Selected Document Date is not in Current Accounting Period..!"
                }, JsonRequestBehavior.AllowGet);
            }
            if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.Type == "CMM00" && x.LockDate == Date).FirstOrDefault() != null)
            {
                return Json(new
                {

                    Status = "ErrorValid",
                    Message = "Selected Document Date is Lock by Period Locking System.."
                }, JsonRequestBehavior.AllowGet); ;
            }


            var LRNos = resultlrlist.Select(x => x.LRRefTableKey).ToList();

            string Status2 = "Success", Message2 = "";
            var Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == "LR000" && LRNos.Contains(AlertMater.ParentKey) && AlertMater.RefType.Contains(Model.Type)
                        orderby AlertMater.DocNo
                        select new AlertNoteVM()
                        {
                            TypeCode = AlertMater.TypeCode,
                            DocReceived = AlertMater.TableKey,
                            DocReceivedN = AlertMater.ParentKey,
                        }).ToList();
            foreach (var item in Mobj)
            {
                var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                foreach (var stp in Activirty)
                {
                    if (Model.Type.Trim() == stp.Trim())
                    {
                        var ConsignmentBookDate = ctxTFAT.LRMaster.Where(x => x.TableKey == item.DocReceivedN).Select(x => x.BookDate).FirstOrDefault();
                        Status2 = "Error";
                        Message2 += item.TypeCode + " Consignment Stopped For The Cash Sale And This Consignment Booked Date Was " + ConsignmentBookDate.ToShortDateString() + " .\nSo We Cannot Allow This Consignment To Cash Sale Please Remove It....!\n";
                        break;
                    }
                }
            }
            if (Message2 != "")
            {
                return Json(new
                {

                    Status = "ErrorValid",
                    Message = Message2
                }, JsonRequestBehavior.AllowGet);
            }


            if (Model.MainType == "SL" && "ES~OS~QS~PI~SG".Contains(Model.SubType) == false)
            {
                mTrx = "S";
            }
            else if (Model.MainType == "PR" && "EP~OP~QP~PG".Contains(Model.SubType) == false)
            {
                mTrx = "P";
            }



            //decimal chggstamt = GetAmtValue(TaxAmtStr);
            var resultdoctype = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => x).FirstOrDefault();
            bool mProductPost = resultdoctype.ProductPost;
            bool mTradef = resultdoctype.CurrConv == "Y" ? true : false;
            List<LRInvoiceVM> mTaxPostCode2 = new List<LRInvoiceVM>();

            bool mRCM = false;
            if (Model.GSTType == "1")
            {
                mRCM = true;
            }

            List<LRInvoiceVM> mProdPostCode2 = new List<LRInvoiceVM>();

            // create item wise posting array


            mMultTax = Model.SGSTAmt + Model.CGSTAmt + Model.IGSTAmt;// + mobj.CessAmt

            mTaxable = Model.Taxable;


            //--------------- GST posting starts
            if ((mTrx == "P" && mRCM == false && mTradef == false) || mTrx == "S")
            {
                // igst
                if (Model.IGSTAmt != 0)
                {
                    mTaxPostCode2.Add(new LRInvoiceVM()
                    {
                        TaxCode = "000100037",
                        TaxAmt = Model.IGSTAmt
                    });
                }
                // cgst
                if (Model.CGSTAmt != 0)
                {
                    mTaxPostCode2.Add(new LRInvoiceVM()
                    {
                        TaxCode = "000100036",
                        TaxAmt = Model.CGSTAmt
                    });
                }
                // sgst
                if (Model.SGSTAmt != 0)
                {
                    mTaxPostCode2.Add(new LRInvoiceVM()
                    {
                        TaxCode = "000100035",
                        TaxAmt = Model.SGSTAmt
                    });
                }
                //var mtaxs = ctxTFAT.TaxMaster.Select(x => new
                //{
                //    x.Code,
                //    x.SGSTCode,
                //    x.CGSTCode,
                //    x.IGSTCode,
                //    x.CessCode,
                //    x.Scope
                //}).Where(z => z.Code == Model.GSTCode).FirstOrDefault();
                //if (mtaxs != null)
                //{
                //    // igst
                //    if (Model.IGSTAmt != 0)
                //    {
                //        mTaxPostCode2.Add(new LRInvoiceVM()
                //        {
                //            TaxCode = mtaxs.IGSTCode,
                //            TaxAmt = Model.IGSTAmt
                //        });
                //    }
                //    // cgst
                //    if (Model.CGSTAmt != 0)
                //    {
                //        mTaxPostCode2.Add(new LRInvoiceVM()
                //        {
                //            TaxCode = mtaxs.CGSTCode,
                //            TaxAmt = Model.CGSTAmt
                //        });
                //    }
                //    // sgst
                //    if (Model.SGSTAmt != 0)
                //    {
                //        mTaxPostCode2.Add(new LRInvoiceVM()
                //        {
                //            TaxCode = mtaxs.SGSTCode,
                //            TaxAmt = Model.SGSTAmt
                //        });
                //    }
                //}
                //else
                //{
                //    HSNMaster hSNMaster = ctxTFAT.HSNMaster.Where(x => x.Code == Model.GSTCode).FirstOrDefault();
                //    if (hSNMaster != null)
                //    {
                //        // igst
                //        if (Model.IGSTAmt != 0)
                //        {
                //            mTaxPostCode2.Add(new LRInvoiceVM()
                //            {
                //                TaxCode = hSNMaster.IGSTOut,
                //                TaxAmt = Model.IGSTAmt
                //            });
                //        }
                //        // cgst
                //        if (Model.CGSTAmt != 0)
                //        {
                //            mTaxPostCode2.Add(new LRInvoiceVM()
                //            {
                //                TaxCode = hSNMaster.CGSTOut,
                //                TaxAmt = Model.CGSTAmt
                //            });
                //        }
                //        // sgst
                //        if (Model.SGSTAmt != 0)
                //        {
                //            mTaxPostCode2.Add(new LRInvoiceVM()
                //            {
                //                TaxCode = hSNMaster.SGSTOut,
                //                TaxAmt = Model.SGSTAmt
                //            });
                //        }
                //    }
                //}
            }
            else
            {
                if (Model.MainType == "PR" && mTradef == true)
                {
                    mIGSTCode = "000100089"; //   IGST Output for Imports Account (Liability)
                    mCGSTCode = ""; //   CGST Output URD Account (Liability)
                    mSGSTCode = ""; //   SGST Output URD Account (Liability)
                }
                else if (Model.GSTType == "6")    //urd
                {
                    mCGSTCode = "000100058"; //   CGST Output URD Account (Liability)
                    mSGSTCode = "000100059"; //   SGST Output URD Account (Liability)
                    mIGSTCode = "000100060"; //   IGST Output URD Account (Liability)
                }
                else
                {
                    mCGSTCode = "000100061"; //   CGST Output RCM Account (Liability)
                    mSGSTCode = "000100062"; //  SGST Output RCM Account (Liability)
                    mIGSTCode = "000100063"; //   IGST Output RCM Account (Liability)
                }

                // igst
                if (Model.IGSTAmt != 0)
                {

                    mTaxPostCode2.Add(new LRInvoiceVM()
                    {
                        TaxCode = mIGSTCode,
                        TaxAmt = Model.IGSTAmt * (mTrx == "P" ? -1 : 1)
                    });
                }

                // cgst
                if (Model.CGSTAmt != 0)
                {

                    mTaxPostCode2.Add(new LRInvoiceVM()
                    {
                        TaxCode = mCGSTCode,
                        TaxAmt = Model.CGSTAmt * (mTrx == "P" ? -1 : 1)
                    });
                }
                // sgst
                if (Model.SGSTAmt != 0)
                {

                    mTaxPostCode2.Add(new LRInvoiceVM()
                    {
                        TaxCode = mSGSTCode,
                        TaxAmt = Model.SGSTAmt * (mTrx == "P" ? -1 : 1)
                    });
                }

                if (Model.MainType == "PR" && mTradef == true)
                {
                    mIGSTCode = "000100090"; //   IGST Input for Imports Account
                    mCGSTCode = ""; //   CGST Output URD Account (Liability)
                    mSGSTCode = ""; //   SGST Output URD Account (Liability)
                }
                else if (Model.GSTType == "6")    //urd
                {
                    mCGSTCode = "000100043"; //   CGST Input URD Account
                    mSGSTCode = "000100044"; //   SGST Input URD Account
                    mIGSTCode = "000100045"; //   IGST Input URD Account
                }
                else
                {
                    mCGSTCode = "000100040"; //   CGST Input RCM Account
                    mSGSTCode = "000100041"; //   SGST Input RCM Account
                    mIGSTCode = "000100042"; //   IGST Input RCM Account
                }

                // igst
                if (Model.IGSTAmt != 0)
                {

                    mTaxPostCode2.Add(new LRInvoiceVM()
                    {
                        TaxCode = mIGSTCode,
                        TaxAmt = Model.IGSTAmt
                    });
                }
                // cgst
                if (Model.CGSTAmt != 0)
                {

                    mTaxPostCode2.Add(new LRInvoiceVM()
                    {
                        TaxCode = mCGSTCode,
                        TaxAmt = Model.CGSTAmt
                    });
                }
                // sgst
                if (Model.SGSTAmt != 0)
                {

                    mTaxPostCode2.Add(new LRInvoiceVM()
                    {
                        TaxCode = mSGSTCode,
                        TaxAmt = Model.SGSTAmt
                    });
                }
            }


            // ----- actual posting routine starts from here
            xCnt = 1;

            mvalue = mTaxable;

            List<LRInvoiceVM> LedPostList = new List<LRInvoiceVM>();

            if (mTrx == "P" && (mTradef == true || (mRCM == false && Model.GSTType == "6")))
            {
                mPostAmt = mvalue;   // inventory value
            }
            if (mTrx == "P" && mTradef == true)
            {
                decimal mroundcrdt = 0;
                if (Model.IsRoundOff == true && Model.RoundOff != 0)
                {
                    mroundcrdt = Model.RoundOff;// this time -ve
                }

                mPostAmt = mvalue + mroundcrdt;
            }

            // posting for Party account
            if (Model.SubType != "NS" && Model.SubType != "NP")
            {
                mDebit = mPostAmt;
                mCredit = 0;
            }
            else
            {
                mDebit = 0;
                mCredit = mPostAmt;
            }

            if (Model.MainType == "SL")
            {

                LedPostList.Add(new LRInvoiceVM()
                {

                    Code = Model.Account,
                    AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                    Debit = Math.Round(mDebit, 2),
                    Credit = Math.Round(mCredit, 2),
                    Branch = mbranchcode,
                    FilBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                    tempId = xCnt,
                    DelyCode = Model.Account
                });

            }
            else
            {

                LedPostList.Add(new LRInvoiceVM()
                {
                    Code = Model.Account,
                    AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                    Debit = Math.Round(mCredit, 2),
                    Credit = Math.Round(mDebit, 2),
                    Branch = mbranchcode,
                    FilBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                    tempId = xCnt,
                    DelyCode = Model.Account
                });
            }
            xCnt++;


            // Charges Updations
            n = 0;

            List<LRInvoiceVM> mCharges = new List<LRInvoiceVM>();
            foreach (var adc in resultlrlist)
            {
                mCharges.AddRange(adc.LRChargeList);
            }
            var mCharges2 = mCharges.GroupBy(x => x.Fld).Select(x => new LRInvoiceVM()
            {
                AddLess = x.Select(x1 => x1.AddLess).First(),
                Header = x.Select(x1 => x1.Header).First(),
                ChgPostCode = x.Select(x1 => x1.ChgPostCode).First(),
                Amt = x.Select(x1 => x1.Amt).Sum()
            }).ToList();

            var mchg = mCharges2.Where(x => x.AddLess == "+" || x.AddLess == "-").Select(x => x).ToList();
            foreach (LRInvoiceVM mc in mchg)
            {
                // if (product wise posting ) dont post net amount
                if (mc.ChgPostCode != "" && mc.Amt != 0 && (mProductPost == false))
                {
                    mPostAmt = mc.Amt;
                    if (Model.SubType != "NS" && Model.SubType != "NP")
                    {
                        if (mc.AddLess == "-")
                        {
                            mDebit = Math.Round(mPostAmt, 2);
                            mCredit = 0;
                        }
                        else
                        {
                            mDebit = 0;
                            mCredit = Math.Round(mPostAmt, 2);
                        }
                    }
                    else
                    {
                        if (mc.AddLess == "-")
                        {
                            mDebit = 0;
                            mCredit = Math.Round(mPostAmt, 2);
                        }
                        else
                        {
                            mDebit = Math.Round(mPostAmt, 2);
                            mCredit = 0;
                        }
                    }
                    if (Model.MainType == "SL")
                    {
                        LedPostList.Add(new LRInvoiceVM()
                        {
                            Code = mc.ChgPostCode,
                            AccountName = NameofAccount(mc.ChgPostCode),
                            Debit = Math.Round(mDebit, 2),
                            Credit = Math.Round(mCredit, 2),
                            Branch = mbranchcode,
                            FilBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                            tempId = xCnt++,
                            DelyCode = Model.Account
                        });
                    }
                    else
                    {
                        LedPostList.Add(new LRInvoiceVM()
                        {
                            Code = mc.ChgPostCode,
                            AccountName = NameofAccount(mc.ChgPostCode),
                            Debit = Math.Round(mCredit, 2),
                            Credit = Math.Round(mDebit, 2),
                            Branch = mbranchcode,
                            FilBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                            tempId = xCnt++,
                            DelyCode = Model.Account
                        });
                    }
                }
                n++;
            }

            var mTaxPostCode = mTaxPostCode2.GroupBy(x => x.TaxCode).ToList();
            string ourstatename = ctxTFAT.Warehouse.Where(x => x.Code == Model.LocationCode).Select(x => x.State).FirstOrDefault();


            foreach (var a in mTaxPostCode)
            {
                if (a.Key != null)
                {
                    mCode = a.Key;
                    mPostAmt = mTaxPostCode2.Where(x => x.TaxCode == a.Key).Sum(x => (decimal?)x.TaxAmt) ?? (decimal)0;

                    if (mCode == "")
                        break;
                    if (mPostAmt != 0)
                    {
                        mDebit = 0;
                        mCredit = 0;
                        if (Model.SubType != "NS" && Model.SubType != "NP")
                        {
                            if (mPostAmt < 0)
                            {
                                mDebit = Math.Abs(mPostAmt);
                            }
                            else
                            {
                                mCredit = mPostAmt;
                            }
                        }
                        else
                        {
                            if (mPostAmt < 0)
                            {
                                mCredit = Math.Abs(mPostAmt);
                            }
                            else
                            {
                                mDebit = Math.Abs(mPostAmt);
                            }
                        }
                        if (Model.MainType == "SL")
                        {
                            LedPostList.Add(new LRInvoiceVM()
                            {
                                Code = mCode,
                                AccountName = NameofAccount(mCode),
                                Debit = Math.Round(mDebit, 2),
                                Credit = Math.Round(mCredit, 2),
                                Branch = mbranchcode,
                                FilBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                tempId = xCnt++,
                                DelyCode = Model.Account
                            });
                        }
                        else
                        {
                            LedPostList.Add(new LRInvoiceVM()
                            {
                                Code = mCode,
                                AccountName = NameofAccount(mCode),
                                Debit = Math.Round(mCredit, 2),
                                Credit = Math.Round(mDebit, 2),
                                Branch = mbranchcode,
                                FilBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                tempId = xCnt++,
                                DelyCode = Model.Account
                            });
                        }
                    }
                }
            }

            //}

            //SDS
            if (Model.IsRoundOff == true && Model.RoundOff != 0)
            {
                var result = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => new { x.Constant }).FirstOrDefault();
                mCode = result.Constant == "" || result.Constant == null ? "000100001" : result.Constant;
                if (Model.SubType != "NS" && Model.SubType != "NP")
                {
                    if (Model.RoundOff > 0)
                    {
                        mDebit = 0;
                        mCredit = Model.RoundOff;
                    }
                    else
                    {
                        mDebit = Model.RoundOff * -1;
                        mCredit = 0;
                    }
                }
                else
                {
                    if (Model.RoundOff > 0)
                    {
                        mDebit = Model.RoundOff;
                        mCredit = 0;
                    }
                    else
                    {
                        mDebit = 0;
                        mCredit = Model.RoundOff * -1;
                    }
                }
                if (Model.MainType == "SL")
                {
                    LedPostList.Add(new LRInvoiceVM()
                    {
                        Code = mCode,
                        AccountName = NameofAccount(mCode),
                        Debit = Math.Round(mDebit, 2),
                        Credit = Math.Round(mCredit, 2),
                        Branch = mbranchcode,
                        FilBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                        tempId = xCnt++,
                        DelyCode = Model.Account
                    });
                }
                else
                {
                    LedPostList.Add(new LRInvoiceVM()
                    {
                        Code = mCode,
                        AccountName = NameofAccount(mCode),
                        Debit = Math.Round(mCredit, 2),
                        Credit = Math.Round(mDebit, 2),
                        Branch = mbranchcode,
                        FilBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                        tempId = xCnt++,
                        DelyCode = Model.Account
                    });
                }
            }

            bool bTDS = Model.TDSFlag;
            decimal mTDS = Model.TDSAmt;
            if (bTDS == true && mTDS != 0)
            {
                mPostAmt = mTDS;
                if (Model.SubType == "NP" || Model.SubType == "PX")
                    mPostAmt = mPostAmt * -1;

                if (mPostAmt > 0)
                {
                    mDebit = 0;
                    mCredit = mPostAmt;
                }
                else
                {
                    mDebit = mPostAmt * -1;
                    mCredit = 0;
                }

                //var mTdsPostCode = ctxTFAT.TDSMaster.Where(x => x.Code.ToString() == Model.TDSCode).Select(x => x.PostCode).FirstOrDefault();
                var mTdsPostCode = "000009992";






                //LedPostList.Add(new LRInvoiceVM()
                //{
                //    Code = Model.Account,
                //    AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                //    Debit = Math.Round(mDebit, 2),
                //    Credit = Math.Round(mCredit, 2),
                //    Branch = mbranchcode,
                //    FilBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                //    tempId = xCnt++,
                //    DelyCode = mTdsPostCode

                //});


                if (Model.SubType != "NP" && Model.SubType != "PX" && Model.SubType != "SX" && Model.SubType != "NS")
                {
                    mTDS = mTDS * -1;
                }

                if (mTDS < 0)
                {
                    mDebit = mTDS * -1;
                    mCredit = 0;
                }
                else
                {
                    mDebit = 0;
                    mCredit = mTDS;
                }

                var UpdateLEdger = LedPostList.Where(x => x.Code == Model.Account).FirstOrDefault();
                UpdateLEdger.Debit -= mDebit;
                UpdateLEdger.Credit -= mCredit;

                LedPostList.Add(new LRInvoiceVM()
                {
                    Code = mTdsPostCode,
                    AccountName = NameofAccount(mTdsPostCode),
                    Debit = Math.Round(mDebit, 2),
                    Credit = Math.Round(mCredit, 2),
                    Branch = mbranchcode,
                    FilBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                    TDSFlag = true,
                    tempId = xCnt++,
                    DelyCode = Model.Account
                });

            }

            var List = LedPostList.Where(x => x.Code != x.DelyCode).Select(x => x.Code).ToList();
            string concat = string.Join(",", List);
            foreach (var item in LedPostList.Where(x => x.Code == x.DelyCode).ToList())
            {
                item.DelyCode = concat;
            }

            Model.TotDebit = LedPostList.Sum(x => x.Debit);
            Model.TotCredit = LedPostList.Sum(x => x.Credit);
            Model.LedgerPostList = LedPostList;

            // display view form
            var html = ViewHelper.RenderPartialView(this, "LedgerPosting", new LRInvoiceVM() { LedgerPostList = Model.LedgerPostList, TotDebit = Math.Round(Model.TotDebit, 2), TotCredit = Math.Round(Model.TotCredit, 2), Mode = Model.Mode, Controller2 = Model.Controller2, Type = Model.Type, Module = Model.Module, ViewDataId = Model.ViewDataId, TableName = Model.TableName, Header = Model.Header, optiontype = Model.optiontype, OptionCode = Model.OptionCode, MainType = Model.MainType, SubType = Model.SubType });
            return Json(new
            {
                LedgerPostList = Model.LedgerPostList,
                TotDebit = Math.Round(Model.TotDebit, 2),
                TotCredit = Math.Round(Model.TotCredit, 2),
                Mode = Model.Mode,
                Html = html
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Addons

        [HttpGet]
        public ActionResult GetEditAddOnList(string Code, string ParentKey, string Type)
        {
            LRInvoiceVM Model = new LRInvoiceVM();
            List<AddOns> objitemlist = new List<AddOns>();
            var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%Doc" + Code && x.Hide == false && x.Types.Contains(Type))
                          select i).OrderBy(x => new { x.Sno, x.Fld }).ToList();

            int t = 1;
            int n = 0;
            foreach (var i in addons)
            {
                AddOns c = new AddOns();
                c.Fld = i.Fld;
                c.Head = i.Head;
                c.ApplCode = GetAddonValue(i.Fld, Code, ParentKey);
                c.QueryText = i.QueryText == null ? "" : GetQueryTextAddon(i.QueryText);
                c.FldType = i.FldType;
                c.PlaceValue = i.PlaceValue;
                c.Eqsn = i.Eqsn;
                objitemlist.Add(c);
                t = t + 1;
                n = n + 1;
            }

            Model.AddOnList = objitemlist;
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/LRInvoice/AddOnGrid.cshtml", new LRInvoiceVM() { AddOnList = Model.AddOnList, Mode = "Edit" });
            var jsonResult = Json(new
            {
                AddOnList = Model.AddOnList,
                Mode = "Edit",
                Html = html
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }

        public List<AddOns> GetAddOnListOnEdit(string Code, string ParentKey, string Type)
        {
            List<AddOns> objitemlist = new List<AddOns>();
            var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%Doc" + Code && x.Hide == false && x.Types.Contains(Type))
                          select i).OrderBy(x => new { x.Sno, x.Fld }).ToList();
            var loginQuery3 = @"select * from addondoc" + Code + " where tablekey=" + "'" + ParentKey + "'" + "";
            DataTable mDt = GetDataTable(loginQuery3, GetConnectionString());

            int t = 1;
            int n = 0;
            foreach (var i in addons)
            {
                AddOns c = new AddOns();
                c.Fld = i.Fld;
                c.Head = i.Head;
                if (mDt.Rows.Count > 0)
                {
                    c.ApplCode = (string.IsNullOrEmpty(mDt.Rows[0][i.Fld].ToString()) == true) ? "" : mDt.Rows[0][i.Fld].ToString();
                }
                else
                {
                    c.ApplCode = "";
                }
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
        public ActionResult CalByEquationAddon(LRInvoiceVM Model)
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
                          select i).OrderBy(x => new { x.Sno, x.Fld }).ToList();
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
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/LRInvoice/AddOnGrid.cshtml", new LRInvoiceVM() { AddOnList = Model.AddOnList, Mode = Model.Mode, Fld = Model.Fld });
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
            string connstring = GetConnectionString();
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

            DataTable smDt = GetDataTable(sql, connstring);

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

        #endregion

        #region Charges

        [HttpPost]
        public ActionResult GetLRChargesTotal(LRInvoiceVM Model)
        {
            decimal mamtm = 0;
            List<LRInvoiceVM> mCharges = new List<LRInvoiceVM>();
            foreach (var chg in Model.LRChargeList)
            {
                mCharges.Add(new LRInvoiceVM()
                {
                    Amt = (chg.AddLess == "+") ? chg.Amt : -chg.Amt
                });
            }
            mamtm = mCharges.Sum(x => x.Amt) + Model.Freight;
            return Json(new
            {
                Total = mamtm,
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetTotal(LRInvoiceVM Model)
        {
            decimal TdsAmt = 0, IGST = 0, SGST = 0, CGST = 0;

            if (Model.TDSFlag)
            {
                Model.TDSAmt = (Model.Taxable * Model.TDSRate) / 100;
            }
            else
            {
                Model.TDSAmt = 0;
            }
            if (Model.GSTFlag)
            {
                Model.IGSTAmt = Math.Round((Model.Taxable * Model.IGSTRate) / 100, 2);
                Model.SGSTAmt = Math.Round((Model.Taxable * Model.SGSTRate) / 100, 2);
                Model.CGSTAmt = Math.Round((Model.Taxable * Model.CGSTRate) / 100, 2);
            }
            else
            {
                Model.IGSTAmt = 0;
                Model.SGSTAmt = 0;
                Model.CGSTAmt = 0;
            }
            //decimal mamtm = Model.Taxable + Model.IGSTAmt + Model.SGSTAmt + Model.CGSTAmt;
            decimal mamtm = Math.Round(Model.Taxable + Model.IGSTAmt + Model.SGSTAmt + Model.CGSTAmt, 2);
            //decimal mamtm = Model.Taxable + Model.IGSTAmt + Model.CGSTAmt + Model.SGSTAmt;
            //if (Model.CutTDS == true && Model.LRCutTDS == true)//for tcs
            //{
            //    mamtm = mamtm + Model.TDSAmt;
            //}

            return Json(new
            {

                Total = mamtm,
                IGSTAmt = Model.IGSTAmt,
                CGSTAmt = Model.CGSTAmt,
                SGSTAmt = Model.SGSTAmt,
                TDSAmt = Model.TDSAmt,

                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }

        public List<LRInvoiceVM> GetLRCharges(string TableKey)
        {
            List<LRInvoiceVM> objledgerdetail = new List<LRInvoiceVM>();
            var trncharges = ctxTFAT.Charges.Where(x => x.Type == "LR000").Select(x => new { x.Fld, x.Head, x.EqAmt, x.Equation, x.Code }).ToList();
            foreach (var i in trncharges)
            {
                LRInvoiceVM c = new LRInvoiceVM();
                c.Fld = i.Fld;
                c.Header = i.Head;
                c.AddLess = i.EqAmt;
                c.ChgPostCode = i.Code;
                c.tempId = Convert.ToInt16(i.Fld.Substring(1, 3));
                c.Amt = GetLrWiseChargeValValue(c.tempId, TableKey);


                objledgerdetail.Add(c);
            }
            return objledgerdetail;
        }

        public decimal GetLrWiseChargeValValue(int i, string TableKey)
        {
            string connstring = GetConnectionString();
            decimal abc;

            var loginQuery3 = @"select Val" + i + " from LRBill where  TableKey = '" + TableKey + "'";
            DataTable mDt2 = GetDataTable(loginQuery3, connstring);
            if (mDt2.Rows.Count > 0)
            {
                abc = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? 0 : Convert.ToDecimal(mDt2.Rows[0][0].ToString());
            }
            else
            {
                abc = 0;
            }

            return abc;
        }

        public decimal GetChargeValValue(int i, string TableKey)
        {
            string connstring = GetConnectionString();
            decimal abc;

            var loginQuery3 = @"select Amt" + i + " from Sales where  TableKey = '" + TableKey + "'";
            DataTable mDt2 = GetDataTable(loginQuery3, connstring);
            if (mDt2.Rows.Count > 0)
            {
                abc = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? 0 : Convert.ToDecimal(mDt2.Rows[0][0].ToString());
            }
            else
            {
                abc = 0;
            }

            return abc;
        }

        #endregion

        #region Pickup
        public List<LRInvoiceVM> Add2ItemForPickup(List<DataRow> ordersstk, string query)
        {
            double PendFactor = 0;
            List<LRInvoiceVM> objitemlist = new List<LRInvoiceVM>();
            int i = 1;
            foreach (var item in ordersstk)
            {

                objitemlist.Add(new LRInvoiceVM()
                {
                    tempId = i,
                    Code = item["Code"].ToString(),
                    Pending = Math.Abs(Math.Round(Convert.ToDouble(item["Pending"].ToString()))),
                    Type = item["Type"].ToString(),
                    ParentKey = item["ParentKey"].ToString(),
                    TableKey = item["TableKey"].ToString(),

                    Qty = Convert.ToDouble(item["Qty"].ToString()),
                    Narr = item["Narr"].ToString(),
                    LRDocuDate = item["LRDocuDate"].ToString(),
                    Branch = item["Branch"].ToString(),
                    Consignor = item["Consignor"].ToString(),
                    Consignee = item["Consignee"].ToString(),
                    Unit = item["Unit"].ToString(),
                    ChargeType = item["ChgType"].ToString(),
                    Account = item["BillParty"].ToString(),
                    ToLocation = item["ToLocation"].ToString(),
                    FromLocation = item["FromLocation"].ToString(),
                    ActWt = Convert.ToDouble(item["Weightage"].ToString()),
                    ChgPickupList = GetLRChargesPickup(item["TableKey"].ToString())
                });
                i = i + 1;
            }

            return objitemlist;
        }

        public ActionResult GetPickUp(LRInvoiceVM Model)
        {
            string mstr = "";
            string abc = "";

            List<LRInvoiceVM> objitemlist = new List<LRInvoiceVM>();
            if (Session["CNewItemlist"] != null)
            {
                objitemlist = (List<LRInvoiceVM>)Session["CNewItemlist"];
            }
            //Default Charges
            var ChargesCount = ctxTFAT.Charges.Where(x => x.Type == "LR000" && x.DontUse == false).ToList().Count();

            List<bool> ChargesFlag = new List<bool>();
            ChargesFlag.Add(false);
            if (objitemlist.Count() > 0)
            {
                var GetFirstObjectChargesList = objitemlist.FirstOrDefault();
                var ExistChargesList = GetFirstObjectChargesList.LRChargeList.Count();
                if (ExistChargesList > ChargesCount)
                {
                    ChargesFlag.Add(true);
                }

            }

            var charges = ctxTFAT.Charges.Where(x => x.Type == "LR000" && ChargesFlag.Contains(x.DontUse)).Select(C => new
            {
                C.Fld,
                C.Head,
                C.EqAmt,
                C.Equation,
                C.Code,

            }).ToList();
            if (charges != null && charges.Count > 0)
            {
                foreach (var a in charges)
                {
                    int tempid = Convert.ToInt16(a.Fld.Substring(1, 3));
                    abc = abc + "isnull(r.Val" + tempid + ",0) as FLD" + tempid.ToString("D2") + ",";

                }
            }
            var mLrListSess = (List<LRInvoiceVM>)Session["CNewItemlist"];
            List<string> mLrList = new List<string>();
            if (mLrListSess != null)
            {
                mLrList = mLrListSess.Select(x => x.Code).ToList();
            }
            abc = (abc != "") ? abc.Remove(abc.Length - 1, 1) : abc;


            string mBranchFilter = "";
            if (string.IsNullOrEmpty(Model.FilBranch) == true)
            {
                mBranchFilter = "  r.BillBran = '" + mbranchcode + "'";
            }
            else if (Model.FilBranch == "Current")
            {
                mBranchFilter = "  r.BillBran = '" + mbranchcode + "'";
            }
            else if (Model.FilBranch == "All")
            {
                mBranchFilter = "1=1";
            }

            mstr = "Select r.LRNO as Code,r.TrType as Type ,r.ParentKey ,convert(varchar, r.BookDate, 103) as LRDocuDate,r.TableKey,r.TotQty as Qty,(abs(r.TotQty) - abs(isnull((Select sum(LRBill.TotQty) from LRBill Where LRBill.lrno = r.lRNO),0))) as Pending,r.Narr,(select top 1 tb.Name from tfatbranch tb where tb.Code = r.BillBran) as Branch,(select cr.name from Consigner cr where cr.Code = r.RecCode) as Consignor,(select cr.name from Consigner cr where cr.Code = r.SendCode) as Consignee,r.ActWt as Weightage,(select tb1.Name from TfatBranch tb1 where tb1.code=r.Dest) as ToLocation,(select tb1.Name from TfatBranch tb1 where tb1.code=r.Source) as FromLocation,r.TableKey,(select mm.name from master mm where mm.code = r.BillParty) as BillParty,(select ch.ChargeType from ChargeTypeMaster ch where ch.Code = r.ChgType) as ChgType,r.UnitCode as Unit from LRMaster r  where  " + mBranchFilter + " and left(r.AUTHORISE,1)='A' And (abs(r.TotQty) -  abs(isnull((Select sum(LRBill.TotQty) from LRBill Where LRBill.lrno = r.lRNO),0))) >0 Order by r.CreateDate";
            List<DataRow> ordersstk = GetDataTable(mstr).AsEnumerable().ToList();
            Model.NewItemList = Add2ItemForPickup(ordersstk, abc);
            var mChargesHeaderList = charges.Select(x => x.Head).ToList();
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            var mCount = charges.Count;
            var html = ViewHelper.RenderPartialView(this, "PickUp", new LRInvoiceVM() { NewItemList = Model.NewItemList.Where(x => !mLrList.Contains(x.Code)).ToList(), TotalQty = mCount, HeaderList = mChargesHeaderList, FilCustomer = Model.FilCustomer, FilBranch = Model.FilBranch });
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public ActionResult PostPickUp(LRInvoiceVM Model)
        {
            try
            {
                List<LRInvoiceVM> objitemlist = new List<LRInvoiceVM>();

                List<string> mtablekeys = new List<string>();

                if (Session["CNewItemlist"] != null)
                {
                    objitemlist = (List<LRInvoiceVM>)Session["CNewItemlist"];
                }

                int mMaxtempid = (objitemlist.Count == 0) ? 0 : objitemlist.Select(x => x.tempId).Max();

                foreach (var c in Model.PickUpList.OrderBy(x => x.tempId))
                {

                    var mPrevQty = objitemlist.Where(x => x.Code == c.Code).Sum(x => (decimal?)x.Qty) ?? 0;
                    int mCode = Convert.ToInt32(c.Code);
                    var mPendingQty = (ctxTFAT.LRMaster.Where(x => x.LrNo == mCode).Sum(x => (int?)x.TotQty ?? 0) - Math.Abs(ctxTFAT.LRBill.Where(x => x.LrNo == c.Code).Sum(x => (int?)x.TotQty) ?? 0));
                    var mCurrPlusBefore = Convert.ToInt32(mPrevQty) + Convert.ToInt32(c.Qty);
                    if (mPendingQty < mCurrPlusBefore)
                    {
                        return Json(new { Status = "ErrorStatus", Message = "Quantity Selected is greater than Pending Balance Quantity" }, JsonRequestBehavior.AllowGet);
                    }

                    mMaxtempid = mMaxtempid + 1;
                    Model.LRChargeList = GetLrWiseChargesLsit(c);

                    List<LRInvoiceVM> mCharges = new List<LRInvoiceVM>();
                    foreach (var chg in Model.LRChargeList)
                    {
                        mCharges.Add(new LRInvoiceVM()
                        {
                            Amt = (chg.AddLess == "+") ? chg.Amt : -chg.Amt,

                        }); ;
                    }

                    Model.Val1 = mCharges.Sum(x => x.Amt) + c.Freight;


                    objitemlist.Add(new LRInvoiceVM()
                    {
                        Code = c.Code,
                        LRName = c.Code,
                        TotalQty = c.TotalQty,
                        Qty = c.Qty,
                        Pending = mPendingQty,
                        Val1 = Model.Val1,
                        Narr = c.Narr,

                        LRChargeList = Model.LRChargeList,
                        ActWt = ctxTFAT.LRMaster.Where(x => x.LrNo == mCode).Select(x => x.ActWt).FirstOrDefault(),
                        ChargeType = ctxTFAT.LRMaster.Where(x => x.LrNo == mCode).Select(x => x.ChgType).FirstOrDefault(),
                        ChgWt = ctxTFAT.LRMaster.Where(x => x.LrNo == mCode).Select(x => x.ChgWt).FirstOrDefault(),
                        Branch = c.Branch,
                        Consignor = c.Consignor,
                        Consignee = c.Consignee,
                        Weightage = c.Weightage,
                        ToLocation = c.ToLocation,
                        FromLocation = c.FromLocation,
                        BookNarr = c.BookNarr,
                        BillNarr = c.BillNarr,
                        HeaderList = Model.LRChargeList.Select(x => x.Header).ToList(),
                        ChgPickupList = Model.LRChargeList.Select(x => x.Amt).ToList(),
                        tempId = mMaxtempid,
                        LRDocDate = ConvertDDMMYYTOYYMMDD(c.LRDocuDate),
                        POD = c.POD
                    }); ; ;
                }

                Session.Add("CNewItemlist", objitemlist);

                Model.Taxable = objitemlist.Sum(x => x.Val1);
                Model.TotalQty = objitemlist.Sum(x => x.Qty);

                if (Model.Mode == "Add")
                {


                    //#region Charge
                    //List<LRInvoiceVM> objledgerdetail = new List<LRInvoiceVM>();
                    //var trncharges = (from c in ctxTFAT.Charges.Where(x => x.Type == Model.Type && x.DontUse == false)
                    //                  select new { c.Fld, c.Head, c.EqAmt, c.Equation, c.Code }).ToList();

                    //int mfld;
                    //foreach (var i in trncharges)
                    //{
                    //    LRInvoiceVM c = new LRInvoiceVM();
                    //    c.Fld = i.Fld;
                    //    c.Code = i.Head;
                    //    c.AddLess = i.EqAmt;
                    //    c.Equation = i.Equation;
                    //    c.tempId = Convert.ToInt16(i.Fld.Substring(1, 3));


                    //    mfld = Convert.ToInt32(i.Fld.Substring(1));
                    //    if (c.Fld == "F001")
                    //    {
                    //        c.ColVal = Model.Taxable.ToString();
                    //    }
                    //    else
                    //    {
                    //        c.ColVal = "0";
                    //    }

                    //    c.ChgPostCode = i.Code;
                    //    objledgerdetail.Add(c);

                    //}
                    //Model.Charges = objledgerdetail;
                    //#endregion



                    //#region Doc
                    //Model.DocumentList = GetAttachmentListInEdit(Model);

                    //Session["TempPurSaleAttach"] = Model.DocumentList;
                    //#endregion
                    //#region Addon

                    //List<AddOns> objitemlist2 = new List<AddOns>();
                    //var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%Doc" + Model.MainType && x.Hide == false && x.Types.Contains(Model.Type))
                    //              select i).OrderBy(x => new { x.Sno, x.Fld }).ToList();
                    //var loginQuery3 = @"select * from addondoc" + Model.MainType + " where tablekey=" + "'" + Model.ParentKey + "'" + "";
                    //DataTable mDt = GetDataTable(loginQuery3, GetConnectionString());
                    //int t = 1;
                    //int n = 0;
                    //foreach (var i in addons)
                    //{
                    //    AddOns c = new AddOns();
                    //    c.Fld = i.Fld;
                    //    c.Head = i.Head;
                    //    if (mDt != null && mDt.Rows.Count > 0)
                    //    {
                    //        c.ApplCode = (string.IsNullOrEmpty(mDt.Rows[0][i.Fld].ToString()) == true) ? "" : mDt.Rows[0][i.Fld].ToString();
                    //    }
                    //    else
                    //    {
                    //        c.ApplCode = "";
                    //    }
                    //    c.QueryText = i.QueryText == null ? "" : GetQueryTextAddon(i.QueryText);
                    //    c.FldType = i.FldType;
                    //    c.PlaceValue = i.PlaceValue;
                    //    c.Eqsn = i.Eqsn;
                    //    objitemlist2.Add(c);
                    //    t = t + 1;
                    //    n = n + 1;
                    //}
                    //Model.AddOnList = objitemlist2;

                    //string bca = "";
                    //#endregion Addon

                }
                Model.NewItemList = objitemlist;

                Model.HeaderList = Model.LRChargeList.Select(x => x.Header).ToList();


                decimal[] mCharges2 = new decimal[(Model.HeaderList.Count)];

                for (int ai = 0; ai < Model.HeaderList.Count; ai++)
                {
                    decimal mchgamt = 0;
                    foreach (var i in objitemlist)
                    {
                        mchgamt += i.ChgPickupList[ai];

                    }
                    mCharges2[ai] = mchgamt;
                }
                Model.TotalChgPickupList = mCharges2.ToList();
                string html = "";


                html = ViewHelper.RenderPartialView(this, "ItemChargeMoreView", Model);

                var jsonResult = Json(new
                {
                    TotalQty = Model.TotalQty,

                    Taxable = Model.Taxable,

                    Status = "Success",
                    Html = html
                }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of json
                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Status = "Fail",
                    Message = ex.StackTrace + ex.InnerException.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public List<LRInvoiceVM> GetLrWiseChargesLsit(LRInvoiceVM Model)
        {
            List<LRInvoiceVM> newlist1 = new List<LRInvoiceVM>();

            List<LRInvoiceVM> objitemlist = new List<LRInvoiceVM>();

            if (Session["CNewItemlist"] != null)
            {
                objitemlist = (List<LRInvoiceVM>)Session["CNewItemlist"];
            }

            //Default Charges
            var ChargesCount = ctxTFAT.Charges.Where(x => x.Type == "LR000" && x.DontUse == false).ToList().Count();

            List<bool> ChargesFlag = new List<bool>();
            ChargesFlag.Add(false);
            if (objitemlist.Count() > 0)
            {
                var GetFirstObjectChargesList = objitemlist.FirstOrDefault();
                var ExistChargesList = GetFirstObjectChargesList.LRChargeList.Count();
                if (ExistChargesList > ChargesCount)
                {
                    ChargesFlag.Add(true);
                }

            }


            var mCharges = (from C in ctxTFAT.Charges
                            where C.Type == "LR000" && ChargesFlag.Contains(C.DontUse)

                            select new
                            {
                                C.Fld,
                                C.Head,
                                C.EqAmt,
                                C.Equation,
                                C.Code
                            }).ToList();
            int a = 0;
            decimal[] mStrArray = Model.ChgPickupList.ToArray();
            foreach (var i in mCharges)
            {
                LRInvoiceVM c = new LRInvoiceVM();
                c.Fld = i.Fld;
                c.Header = i.Head;
                c.AddLess = i.EqAmt;
                c.tempId = Convert.ToInt16(i.Fld.Substring(1, 3));
                c.ChgPostCode = i.Code;
                c.Amt = (mStrArray[a] == 0) ? (decimal)0 : Convert.ToDecimal(mStrArray[a]);
                newlist1.Add(c);

                a = a + 1;
            }


            return newlist1;
        }

        public List<decimal> GetLRChargesPickup(string TableKey)
        {
            List<decimal> newlist1 = new List<decimal>();

            List<LRInvoiceVM> objitemlist = new List<LRInvoiceVM>();

            List<string> mtablekeys = new List<string>();

            if (Session["CNewItemlist"] != null)
            {
                objitemlist = (List<LRInvoiceVM>)Session["CNewItemlist"];
            }
            //Default Charges
            var ChargesCount = ctxTFAT.Charges.Where(x => x.Type == "LR000" && x.DontUse == false).ToList().Count();
            List<bool> ChargesFlag = new List<bool>();
            ChargesFlag.Add(false);

            if (objitemlist.Count() > 0)
            {
                var GetFirstObjectChargesList = objitemlist.FirstOrDefault();
                var ExistChargesList = GetFirstObjectChargesList.LRChargeList.Count();
                if (ExistChargesList > ChargesCount)
                {
                    ChargesFlag.Add(true);
                }

            }


            var mCharges = (from C in ctxTFAT.Charges
                            where C.Type == "LR000" && ChargesFlag.Contains(C.DontUse)

                            select new
                            {
                                C.Fld,
                                C.Head,
                                C.EqAmt,
                                C.Equation,
                                C.Code
                            }).ToList();
            int a = 1;
            foreach (var i in mCharges)
            {
                int tempId = Convert.ToInt16(i.Fld.Substring(1, 3));
                decimal FLD = 0;
                newlist1.Add(FLD);
            }

            return newlist1;
        }

        [HttpPost]
        public ActionResult PickUpCalCulation(LRInvoiceVM Model)
        {
            List<LRInvoiceVM> mCharges = new List<LRInvoiceVM>();
            List<LRInvoiceVM> mPickLis = new List<LRInvoiceVM>();
            var mNetAmt = (decimal)0;
            List<decimal> mchargesall = new List<decimal>();
            List<LRInvoiceVM> mChgAllList = new List<LRInvoiceVM>();
            if (Model.PickUpList != null)
            {
                foreach (var a in Model.PickUpList)
                {
                    List<LRInvoiceVM> mCharges2 = new List<LRInvoiceVM>();
                    mCharges = GetLrWiseChargesLsit(a);

                    foreach (var chg in mCharges)
                    {
                        mCharges2.Add(new LRInvoiceVM()
                        {
                            Fld = chg.Fld,
                            Header = chg.Header,
                            Amt = (chg.AddLess == "+") ? chg.Amt : -chg.Amt
                        });
                    }



                    var mAmt = /*(a.Freight) +*/ ((mCharges2 != null && mCharges2.Count > 0) ? mCharges2.Sum(x => (decimal?)x.Amt) ?? 0 : 0);
                    mNetAmt += mAmt;

                    mPickLis.Add(new LRInvoiceVM()
                    {
                        Amt = mAmt,
                        Code = a.Code,
                    });

                    mChgAllList.AddRange(mCharges2);

                }

            }
            mchargesall = mChgAllList.GroupBy(x => x.Fld).Select(x1 => x1.Sum(x2 => x2.Amt)).ToList();

            Model.Val1 = mNetAmt;
            Model.TotalChgPickupList = mchargesall;
            Model.AccAmt = mPickLis.Where(x => x.Code == Model.Code).Select(x => (decimal?)x.Amt).FirstOrDefault() ?? 0;
            return Json(Model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PushPickupChargeList(LRInvoiceVM Model)
        {
            List<decimal> ChargesListSelect = new List<decimal>();
            if (Model.LRChargeList != null)
            {
                foreach (var item in Model.LRChargeList)
                {
                    ChargesListSelect.Add(item.Amt);
                }
            }

            return Json(new
            {
                LRNo = Model.Code,
                ChargesListSelect = ChargesListSelect
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LRPickupValidation(LRInvoiceVM Model)
        {
            string mStatus = "";
            string mMessage = "";
            foreach (var a in Model.Code.Split(','))
            {
                int mCode = Convert.ToInt32(a);
                var mLrMaster = ctxTFAT.PODRel.Where(x => x.LrNo == mCode).Select(x => x).FirstOrDefault();
                if (mLrMaster != null)
                {
                    mStatus = "ConfirmError";
                    mMessage += "LRNo " + Model.Code + " POD is already sended";


                }

            }
            if (mMessage != "")
            {
                Model.POD = true;
            }
            Model.Message = mMessage;
            return Json(Model, JsonRequestBehavior.AllowGet);

        }

        #endregion

        #region attachment

        public void SaveAttachment(AttachmentVM Model)
        {
            
            List<AttachmentVM> DocList = new List<AttachmentVM>();
            if (Session["TempAttach"] != null)
            {
                DocList = (List<AttachmentVM>)Session["TempAttach"];
            }

            if (DocList != null && DocList.Count != 0)
            {
                var AttachCode = GetNewAttachCode();
                int J = 1;
                foreach (var item in DocList.ToList())
                {
                    Directory.CreateDirectory(attachmentPath + Model.ParentKey);
                    string directoryPath = attachmentPath + Model.ParentKey + "/" + item.FileName;
                    byte[] IData = Convert.FromBase64String(item.ImageStr);
                    Attachment att = new Attachment();
                    att.AUTHIDS = String.IsNullOrEmpty(item.ENTEREDBY) == true ? muserid : item.ENTEREDBY;
                    att.DocDate = ConvertDDMMYYTOYYMMDD(item.DocDate);
                    att.AUTHORISE = "A00";
                    att.Branch = mbranchcode;
                    att.Code = AttachCode.ToString();
                    att.ENTEREDBY = String.IsNullOrEmpty(item.ENTEREDBY) == true ? muserid : item.ENTEREDBY;
                    att.FilePath = directoryPath;
                    att.LASTUPDATEDATE = DateTime.Now;
                    att.LocationCode = Model.LocationCode;
                    att.Prefix = mperiod;
                    att.Sno = J;
                    att.Srl = Model.Srl;
                    att.SrNo = J;
                    att.TableKey = Model.Type + mperiod.Substring(0, 2) + J.ToString("D3") + Model.Srl;
                    att.Type = String.IsNullOrEmpty(item.Type) == true ? "CMM00" : item.Type;
                    att.ParentKey = Model.ParentKey;

                    att.CompCode = mcompcode;
                    att.ExternalAttach = item.ExternalAttach;
                    att.RefDocNo = item.RefCode;
                    att.RefType = item.RefType;
                    ctxTFAT.Attachment.Add(att);
                    ctxTFAT.SaveChanges();
                    System.IO.File.WriteAllBytes(directoryPath, IData);

                    ++J;
                    ++AttachCode;
                }
            }

        }

        #endregion

        #region MultiPrint

        [HttpPost]
        public ActionResult GetMultiPrint(LRInvoiceVM Model)
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
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/LRInvoice/MultiPrint.cshtml", new LRInvoiceVM() { PrintGridList = Model.PrintGridList, Document = Model.Document });
            var jsonResult = Json(new
            {
                Document = Model.Document,
                PrintGridList = Model.PrintGridList,
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

        #region Reference Adjustment

        public List<LedgerVM> ShowAdjustListInSession(LRInvoiceVM Model)//while in session
        {
            List<LedgerVM> OSAdjList = new List<LedgerVM>();
            List<LedgerVM> ledgers = new List<LedgerVM>();
            List<LedgerVM> objledgerdetail = new List<LedgerVM>();
            List<string> TblKeys = new List<string>();
            DateTime currdate = DateTime.Now.Date;
            int a = 1;

            List<string> TableKeys = Model.OSAdjList == null ? TblKeys : Model.OSAdjList.Select(x => x.TableKey).ToList();

            if (Model.SubType == "CS" || Model.SubType == "RS" || Model.SubType == "NP")
            {
                ledgers = (from l in ctxTFAT.Ledger
                           where l.Branch == Model.Branch && l.Code == Model.Account /*&& l.LocationCode == Model.LocationCode*/ && l.AUTHORISE.Substring(0, 1) == "A"
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
                               BalanceAmt = (l.Credit == null) ? 0 : l.Credit.Value - (ctxTFAT.Outstanding.Where(x => x.TableKey == l.TableKey).Sum(x => x.Amount) ?? 0),
                               BillAmt = l.Credit.Value,
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
                           where l.Branch == Model.Branch && l.Code == Model.Account/* && l.LocationCode == Model.LocationCode */&& l.AUTHORISE.Substring(0, 1) == "A"
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
                               BalanceAmt = (l.Debit == null) ? 0 : l.Debit.Value - (ctxTFAT.Outstanding.Where(x => x.TableKey == l.TableKey).Sum(x => x.Amount) ?? 0),
                               BillAmt = l.Debit.Value,
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
                    BalanceAmt = (r.BalanceAmt + GetAdjAmtFromSession(r.TableKey, Model.OSAdjList)) > r.BillAmt ? r.BillAmt : (r.BalanceAmt + GetAdjAmtFromSession(r.TableKey, Model.OSAdjList)),
                    BillAmt = r.BillAmt,
                    Narr = r.Narr,
                    Party = r.Party,
                    ParentKey = r.ParentKey,
                    TableKey = r.TableKey,
                    AdjustAmt = GetAdjAmtFromSession(r.TableKey, Model.OSAdjList),
                    OSAdjFlag = GetAdjBoolFromSession(r.TableKey, Model.OSAdjList),
                    MainType = r.MainType,
                    SubType = r.SubType,
                    tempId = a
                });
                a = a + 1;
            }


            return OSAdjList;
        }

        [HttpPost]
        public ActionResult GetReferenceAdjustments(LRInvoiceVM Model)
        {
            List<LedgerVM> OSAdjList = new List<LedgerVM>();
            List<LedgerVM> ledgers = new List<LedgerVM>();
            DateTime currdate = DateTime.Now.Date;
            bool mARAP = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(z => z.ARAP).FirstOrDefault();
            if (mARAP == true)
            {
                if (Model.SubType == "CS" || Model.SubType == "RS" || Model.SubType == "NP")
                {
                    ledgers = (from l in ctxTFAT.Ledger
                               where l.Branch == mbranchcode && l.Code == Model.Account /*&& l.LocationCode == Model.LocationCode*/ && l.AUTHORISE.Substring(0, 1) == "A"
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
                                   BalanceAmt = (l.Credit == null) ? 0 : l.Credit.Value - (ctxTFAT.Outstanding.Where(x => x.TableKey == l.TableKey).Sum(x => x.Amount) ?? 0),
                                   BillAmt = l.Credit.Value,
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
                               where l.Branch == mbranchcode && l.Code == Model.Account/* && l.LocationCode == Model.LocationCode */&& l.AUTHORISE.Substring(0, 1) == "A"
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
                                   BalanceAmt = (l.Debit == null) ? 0 : l.Debit.Value - (ctxTFAT.Outstanding.Where(x => x.TableKey == l.TableKey).Sum(x => x.Amount) ?? 0),
                                   BillAmt = l.Debit.Value,
                                   Narr = l.Narr,
                                   Party = l.Party,
                                   ParentKey = l.ParentKey,
                                   TableKey = l.TableKey,
                                   MainType = l.MainType,
                                   SubType = l.SubType
                               }).Where(x => x.BalanceAmt != 0).ToList();
                }
            }
            int a = 1;
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
                    MainType = r.MainType,
                    SubType = r.SubType,
                    tempId = a
                });
                a = a + 1;
            }
            //Model.PartyAdvances = GetPartyAdvances(Model.Code, Model.SubType);
            //Model.PartyNetOutstanding = GetNetOutstanding(Model.Code);
            //Model.PartyConsOutstanding = GetConsOutstanding(Model.Code);
            var html = ViewHelper.RenderPartialView(this, "ReferenceAdjustView", new LRInvoiceVM() { OSAdjList = OSAdjList });
            var jsonResult = Json(new { OSAdjList = OSAdjList, Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public decimal GetAdjAmtFromSession(string TableKey, List<LedgerVM> list)
        {
            decimal Adjamt;

            var AdjamtLIST = list.Where(x => x.TableKey == TableKey).FirstOrDefault();
            if (AdjamtLIST != null)
            {
                Adjamt = AdjamtLIST.AdjustAmt;
            }
            else
            {
                Adjamt = 0;
            }

            return Adjamt;
        }

        public bool GetAdjBoolFromSession(string TableKey, List<LedgerVM> list)
        {
            bool Adjamtbool;

            var AdjamtLIST = list.Where(x => x.TableKey == TableKey).FirstOrDefault();
            if (AdjamtLIST != null)
            {
                Adjamtbool = AdjamtLIST.OSAdjFlag;
            }
            else
            {
                Adjamtbool = false;
            }
            return Adjamtbool;
        }

        public List<LedgerVM> GetOutstandingInEdit(string ParentKey, string MainType)
        {
            List<LedgerVM> OSAdjList = new List<LedgerVM>();
            var ledgers = (from o in ctxTFAT.Outstanding
                           where o.ParentKey == ParentKey && o.MainType == MainType
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
                a = a + 1;
            }
            return OSAdjList;
        }

        #endregion

        #region Contract RAte

        public ActionResult CustomerCharges(LRVM Model)
        {
            bool GeneralContract = false;
            var mLrmaster = ctxTFAT.LRMaster.Where(x => x.LrNo == Model.LrNo).Select(x => x).FirstOrDefault();
            Model.BillParty = mLrmaster.BillParty;
            Model.Source = mLrmaster.Source;
            Model.Dest = mLrmaster.Dest;
            Model.ChgType = mLrmaster.ChgType;
            Model.ActWt = mLrmaster.ActWt;
            Model.ChgWt = mLrmaster.ChgWt;
            Model.TotQty = mLrmaster.TotQty;

            var FromDt = mLrmaster.BookDate;
            ContractList SingleContract = new ContractList();
            List<ContractList> WeightCartoon = new List<ContractList>();
            ConMaster conMaster = new ConMaster();
            if (Model.ChgType == "100000" || Model.ChgType == "100001")
            {
                conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();
                if (conMaster != null)
                {
                    WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                     where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                     select new ContractList()
                                     {
                                         Sno = ConDetail.SrNo,
                                         Service = ConDetail.Services,
                                         TypeOfChrg = ConDetail.WtType,
                                         FromWT = ConDetail.Wtfrom.Value,
                                         ToWT = ConDetail.WtTo.Value,
                                         Rate = (float)ConDetail.Rate,
                                         ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                         ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                         ConDetilsCode = ConDetail.Code,
                                     }).ToList();
                }
                else
                {
                    GeneralContract = true;
                }
                if (WeightCartoon == null || WeightCartoon.Count() == 0)
                {
                    string FromBranch = Model.Source, Tobranch = Model.Dest;
                    TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        FromBranch = tfatBranch.Grp;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }

                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                    if (conMaster != null)
                    {
                        WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                         where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                         select new ContractList()
                                         {
                                             Sno = ConDetail.SrNo,
                                             Service = ConDetail.Services,
                                             TypeOfChrg = ConDetail.WtType,
                                             FromWT = ConDetail.Wtfrom.Value,
                                             ToWT = ConDetail.WtTo.Value,
                                             Rate = (float)ConDetail.Rate,
                                             ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                             ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                             ConDetilsCode = ConDetail.Code,
                                         }).ToList();
                    }

                }

                foreach (var item in WeightCartoon)
                {
                    if (item.Service == "100000")
                    {
                        if (item.ChargeOfChrgWT)
                        {
                            SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ChgWt && x.ToWT >= Model.ChgWt).FirstOrDefault();
                            if (SingleContract != null)
                            {
                                item.Charges = GetChargesOfService(item.ConDetilsCode, item.Service, item.Sno);
                                SingleContract.Charges = item.Charges;
                                break;
                            }
                        }
                        else
                        {
                            SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                            if (SingleContract != null)
                            {
                                item.Charges = GetChargesOfService(item.ConDetilsCode, item.Service, item.Sno);
                                SingleContract.Charges = item.Charges;
                                break;
                            }
                        }
                    }
                    else
                    {
                        SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.TotQty && x.ToWT >= Model.TotQty).FirstOrDefault();
                        if (SingleContract != null)
                        {
                            item.Charges = GetChargesOfService(item.ConDetilsCode, item.Service, item.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }

                }

                if (SingleContract != null)
                {
                    if (SingleContract.Service != null)
                    {
                        if (SingleContract.Service == "100000")
                        {
                            double Freight = 0;
                            var Rate = SingleContract.Rate;
                            if (SingleContract.TypeOfChrg == "KG")
                            {
                                if (SingleContract.ChargeOfChrgWT)
                                {
                                    Freight = Math.Round(Rate * Model.ChgWt);
                                }
                                else
                                {
                                    Freight = Math.Round(Rate * Model.ActWt);
                                }
                            }
                            else
                            {
                                Freight = Math.Round(Rate);
                            }

                            SingleContract.Charges.FirstOrDefault(c => c.Fld == "F001").Val1 = Convert.ToDecimal(Freight);
                            Model.Charges = SingleContract.Charges;
                        }
                        if (SingleContract.Service == "100001")
                        {
                            double Freight = 0;
                            var Rate = SingleContract.Rate;
                            if (SingleContract.TypeOfChrg == "KG")
                            {
                                Freight = Math.Round(Rate * Model.TotQty);
                            }
                            else
                            {
                                Freight = Math.Round(Rate);
                            }
                            SingleContract.Charges.FirstOrDefault(c => c.Fld == "F001").Val1 = Convert.ToDecimal(Freight);
                            Model.Charges = SingleContract.Charges;
                        }
                    }
                }
            }
            else
            {
                conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();
                if (conMaster != null)
                {
                    SingleContract = (from ConDetail in ctxTFAT.ConDetail
                                      where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                      select new ContractList()
                                      {
                                          Sno = ConDetail.SrNo,
                                          Service = ConDetail.Services,
                                          TypeOfChrg = ConDetail.WtType,
                                          FromWT = ConDetail.Wtfrom.Value,
                                          ToWT = ConDetail.WtTo.Value,
                                          Rate = (float)ConDetail.Rate,
                                          ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                          ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                          ConDetilsCode = ConDetail.Code,
                                      }).FirstOrDefault();
                }

                if (SingleContract.Service == null)
                {
                    string FromBranch = Model.Source, Tobranch = Model.Dest;
                    TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        FromBranch = tfatBranch.Grp;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }

                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                    if (conMaster != null)
                    {
                        SingleContract = (from ConDetail in ctxTFAT.ConDetail
                                          where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                          select new ContractList()
                                          {
                                              Sno = ConDetail.SrNo,
                                              Service = ConDetail.Services,
                                              TypeOfChrg = ConDetail.WtType,
                                              FromWT = ConDetail.Wtfrom.Value,
                                              ToWT = ConDetail.WtTo.Value,
                                              Rate = (float)ConDetail.Rate,
                                              ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                              ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                              ConDetilsCode = ConDetail.Code,
                                          }).FirstOrDefault();
                    }

                }
                if (SingleContract != null)
                {
                    if (SingleContract.Service != null)
                    {
                        SingleContract.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                        Model.Charges = SingleContract.Charges;
                    }
                }
            }


            string Status = "", Message = "";
            if (Model.ChgType == "100000" || Model.ChgType == "100001")
            {
                if (SingleContract != null)
                {
                    if (SingleContract.Service == null)
                    {

                        if (GeneralContract)
                        {
                            Status = "Error";
                            Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";
                        }

                        else if (Model.ChgType == "100000")
                        {
                            var ConDetail = ctxTFAT.ConDetail.Where(x => x.CustCode == conMaster.Code && x.Services == "100001").ToList();
                            if (ConDetail.Count() > 0)
                            {
                                Status = "OtherFunction";
                                Message = "Not Found Weight Contract...\n Find Carton Rate Accept ?";
                            }
                            else
                            {
                                Status = "Error";
                                Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";
                            }

                        }
                        else
                        {
                            var ConDetail = ctxTFAT.ConDetail.Where(x => x.CustCode == conMaster.Code && x.Services == "100000").ToList();
                            if (ConDetail.Count() > 0)
                            {
                                Status = "OtherFunction";
                                Message = "Not Found Carton Contract...\n Find Weight Rate Accept ?";
                            }
                            else
                            {
                                Status = "Error";
                                Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";
                            }
                        }
                    }
                    else
                    {
                        Status = "Success";
                    }
                }
                else
                {
                    if (GeneralContract)
                    {
                        Status = "Error";
                        Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";

                    }

                    else if (Model.ChgType == "100000")
                    {
                        var ConDetail = ctxTFAT.ConDetail.Where(x => x.CustCode == conMaster.Code && x.Services == "100001").ToList();
                        if (ConDetail.Count() > 0)
                        {
                            Status = "OtherFunction";
                            Message = "Not Found Weight Contract...\n Find Carton Rate Accept ?";
                        }
                        else
                        {
                            Status = "Error";
                            Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";
                        }

                    }
                    else
                    {
                        var ConDetail = ctxTFAT.ConDetail.Where(x => x.CustCode == conMaster.Code && x.Services == "100000").ToList();
                        if (ConDetail.Count() > 0)
                        {
                            Status = "OtherFunction";
                            Message = "Not Found Carton Contract...\n Find Weight Rate Accept ?";
                        }
                        else
                        {
                            Status = "Error";
                            Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";
                        }
                    }
                }

            }
            else
            {
                if (SingleContract != null)
                {
                    if (SingleContract.Service == null)
                    {
                        Status = "Error";
                        Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";
                    }
                    else
                    {
                        Status = "Success";
                    }
                }
                else
                {
                    Status = "Error";
                    Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";
                }
            }

            List<decimal> ChargesListSelect = new List<decimal>();
            if (Model.Charges != null)
            {
                foreach (var item in Model.Charges)
                {
                    ChargesListSelect.Add(item.Val1);
                }
            }

            return Json(new
            {
                Status = Status,
                Message = Message,
                ChargesListSelect = ChargesListSelect
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CustomerOTherContract(LRVM Model)
        {
            var mLrmaster = ctxTFAT.LRMaster.Where(x => x.LrNo == Model.LrNo).Select(x => x).FirstOrDefault();
            Model.BillParty = mLrmaster.BillParty;
            Model.Source = mLrmaster.Source;
            Model.Dest = mLrmaster.Dest;
            Model.ChgType = mLrmaster.ChgType;
            Model.ActWt = mLrmaster.ActWt;
            Model.ChgWt = mLrmaster.ChgWt;
            Model.TotQty = mLrmaster.TotQty;



            var FromDt = mLrmaster.BookDate;
            Model.BillParty = ctxTFAT.CustomerMaster.Where(x => x.Code == Model.BillParty).Select(x => x.AccountParentGroup).FirstOrDefault();
            ContractList SingleContract = new ContractList();
            List<ContractList> WeightCartoon = new List<ContractList>();
            bool Weight = false; string OthCode = "", OthName = "";
            if (Model.ChgType == "100000")
            {
                Weight = true;
            }
            else
            {
                Weight = false;
            }

            string FromBranch = Model.Source, Tobranch = Model.Dest;
            TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
            {
                FromBranch = tfatBranch.Grp;
            }
            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
            {
                Tobranch = tfatBranch.Grp;
            }

            if (Weight)
            {
                OthCode = "100001";
                OthName = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == OthCode).Select(x => x.ChargeType).FirstOrDefault();
                ConMaster conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();
                if (conMaster != null)
                {
                    WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                     where ConDetail.CustCode == conMaster.Code && ConDetail.Services == "100001"
                                     select new ContractList()
                                     {
                                         Sno = ConDetail.SrNo,
                                         Service = ConDetail.Services,
                                         TypeOfChrg = ConDetail.WtType,
                                         FromWT = ConDetail.Wtfrom.Value,
                                         ToWT = ConDetail.WtTo.Value,
                                         Rate = (float)ConDetail.Rate,
                                         ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                         ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                         ConDetilsCode = ConDetail.Code,
                                     }).ToList();
                }
                if (WeightCartoon == null || WeightCartoon.Count() == 0)
                {
                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                    if (conMaster != null)
                    {
                        WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                         where ConDetail.CustCode == conMaster.Code && ConDetail.Services == "100001"
                                         select new ContractList()
                                         {
                                             Sno = ConDetail.SrNo,
                                             Service = ConDetail.Services,
                                             TypeOfChrg = ConDetail.WtType,
                                             FromWT = ConDetail.Wtfrom.Value,
                                             ToWT = ConDetail.WtTo.Value,
                                             Rate = (float)ConDetail.Rate,
                                             ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                             ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                             ConDetilsCode = ConDetail.Code,
                                         }).ToList();
                    }

                }
            }
            else
            {
                OthCode = "100000";
                OthName = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == OthCode).Select(x => x.ChargeType).FirstOrDefault();
                ConMaster conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();
                if (conMaster != null)
                {
                    WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                     where ConDetail.CustCode == conMaster.Code && ConDetail.Services == "100000"
                                     select new ContractList()
                                     {
                                         Sno = ConDetail.SrNo,
                                         Service = ConDetail.Services,
                                         TypeOfChrg = ConDetail.WtType,
                                         FromWT = ConDetail.Wtfrom.Value,
                                         ToWT = ConDetail.WtTo.Value,
                                         Rate = (float)ConDetail.Rate,
                                         ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                         ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                         ConDetilsCode = ConDetail.Code,
                                     }).ToList();
                }

                if (WeightCartoon == null || WeightCartoon.Count() == 0)
                {
                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                    if (conMaster != null)
                    {
                        WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                         where ConDetail.CustCode == conMaster.Code && ConDetail.Services == "100000"
                                         select new ContractList()
                                         {
                                             Sno = ConDetail.SrNo,
                                             Service = ConDetail.Services,
                                             TypeOfChrg = ConDetail.WtType,
                                             FromWT = ConDetail.Wtfrom.Value,
                                             ToWT = ConDetail.WtTo.Value,
                                             Rate = (float)ConDetail.Rate,
                                             ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                             ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                             ConDetilsCode = ConDetail.Code,
                                         }).ToList();
                    }

                }
            }

            foreach (var item in WeightCartoon)
            {
                if (item.Service == "100000")
                {
                    if (item.ChargeOfChrgWT)
                    {
                        SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ChgWt && x.ToWT >= Model.ChgWt).FirstOrDefault();
                        if (SingleContract != null)
                        {
                            item.Charges = GetChargesOfService(item.ConDetilsCode, item.Service, item.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }
                    else
                    {
                        SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                        if (SingleContract != null)
                        {
                            item.Charges = GetChargesOfService(item.ConDetilsCode, item.Service, item.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }
                }
                else
                {
                    SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.TotQty && x.ToWT >= Model.TotQty).FirstOrDefault();
                    if (SingleContract != null)
                    {
                        item.Charges = GetChargesOfService(item.ConDetilsCode, item.Service, item.Sno);
                        SingleContract.Charges = item.Charges;
                        break;
                    }
                }

            }

            if (SingleContract != null)
            {
                if (SingleContract.Service != null)
                {
                    if (SingleContract.Service == "100000")
                    {
                        double Freight = 0;
                        var Rate = SingleContract.Rate;
                        if (SingleContract.TypeOfChrg == "KG")
                        {
                            if (SingleContract.ChargeOfChrgWT)
                            {
                                Freight = Math.Round(Rate * Model.ChgWt);
                            }
                            else
                            {
                                Freight = Math.Round(Rate * Model.ActWt);
                            }
                        }
                        else
                        {
                            Freight = Math.Round(Rate);
                        }

                        SingleContract.Charges.FirstOrDefault(c => c.Fld == "F001").Val1 = Convert.ToDecimal(Freight);
                        Model.Charges = SingleContract.Charges;
                    }
                    if (SingleContract.Service == "100001")
                    {
                        double Freight = 0;
                        var Rate = SingleContract.Rate;
                        if (SingleContract.TypeOfChrg == "KG")
                        {
                            Freight = Math.Round(Rate * Model.TotQty);
                        }
                        else
                        {
                            Freight = Math.Round(Rate);
                        }
                        SingleContract.Charges.FirstOrDefault(c => c.Fld == "F001").Val1 = Convert.ToDecimal(Freight);
                        Model.Charges = SingleContract.Charges;
                    }
                }
            }


            string Status = "", Message = "";
            if (SingleContract != null)
            {
                if (SingleContract.Service == null)
                {
                    Status = "Error";
                    Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";
                }
                else
                {
                    Status = "Success";
                }
            }
            else
            {
                Status = "Error";
                Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";
            }


            List<decimal> ChargesListSelect = new List<decimal>();
            foreach (var item in Model.Charges)
            {
                ChargesListSelect.Add(item.Val1);
            }

            return Json(new
            {
                Status = Status,
                Message = Message,
                ChargesListSelect = ChargesListSelect
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Charges(LRVM Model)
        {
            bool GeneralContract = false;
            var mLrmaster = ctxTFAT.LRMaster.Where(x => x.LrNo == Model.LrNo).Select(x => x).FirstOrDefault();
            Model.BillParty = mLrmaster.BillParty;
            Model.Source = mLrmaster.Source;
            Model.Dest = mLrmaster.Dest;
            Model.ChgType = mLrmaster.ChgType;
            Model.ActWt = mLrmaster.ActWt;
            Model.ChgWt = mLrmaster.ChgWt;
            Model.TotQty = mLrmaster.TotQty;



            var FromDt = mLrmaster.BookDate;
            Model.BillParty = ctxTFAT.CustomerMaster.Where(x => x.Code == Model.BillParty).Select(x => x.AccountParentGroup).FirstOrDefault();
            ContractList SingleContract = new ContractList();
            List<ContractList> WeightCartoon = new List<ContractList>();
            ConMaster conMaster = new ConMaster();
            if (Model.ChgType == "100000" || Model.ChgType == "100001")
            {
                conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();
                if (conMaster != null)
                {
                    WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                     where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                     select new ContractList()
                                     {
                                         Sno = ConDetail.SrNo,
                                         Service = ConDetail.Services,
                                         TypeOfChrg = ConDetail.WtType,
                                         FromWT = ConDetail.Wtfrom.Value,
                                         ToWT = ConDetail.WtTo.Value,
                                         Rate = (float)ConDetail.Rate,
                                         ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                         ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                         ConDetilsCode = ConDetail.Code,
                                     }).ToList();
                }
                else
                {
                    GeneralContract = true;
                }
                if (WeightCartoon == null || WeightCartoon.Count() == 0)
                {
                    string FromBranch = Model.Source, Tobranch = Model.Dest;
                    TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        FromBranch = tfatBranch.Grp;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }

                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                    if (conMaster != null)
                    {
                        WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                         where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                         select new ContractList()
                                         {
                                             Sno = ConDetail.SrNo,
                                             Service = ConDetail.Services,
                                             TypeOfChrg = ConDetail.WtType,
                                             FromWT = ConDetail.Wtfrom.Value,
                                             ToWT = ConDetail.WtTo.Value,
                                             Rate = (float)ConDetail.Rate,
                                             ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                             ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                             ConDetilsCode = ConDetail.Code,
                                         }).ToList();
                    }

                }

                foreach (var item in WeightCartoon)
                {
                    if (item.Service == "100000")
                    {
                        if (item.ChargeOfChrgWT)
                        {
                            SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ChgWt && x.ToWT >= Model.ChgWt).FirstOrDefault();
                            if (SingleContract != null)
                            {
                                item.Charges = GetChargesOfService(item.ConDetilsCode, item.Service, item.Sno);
                                SingleContract.Charges = item.Charges;
                                break;
                            }
                        }
                        else
                        {
                            SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                            if (SingleContract != null)
                            {
                                item.Charges = GetChargesOfService(item.ConDetilsCode, item.Service, item.Sno);
                                SingleContract.Charges = item.Charges;
                                break;
                            }
                        }
                    }
                    else
                    {
                        SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.TotQty && x.ToWT >= Model.TotQty).FirstOrDefault();
                        if (SingleContract != null)
                        {
                            item.Charges = GetChargesOfService(item.ConDetilsCode, item.Service, item.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }

                }

                if (SingleContract != null)
                {
                    if (SingleContract.Service != null)
                    {
                        if (SingleContract.Service == "100000")
                        {
                            double Freight = 0;
                            var Rate = SingleContract.Rate;
                            if (SingleContract.TypeOfChrg == "KG")
                            {
                                if (SingleContract.ChargeOfChrgWT)
                                {
                                    Freight = Math.Round(Rate * Model.ChgWt);
                                }
                                else
                                {
                                    Freight = Math.Round(Rate * Model.ActWt);
                                }
                            }
                            else
                            {
                                Freight = Math.Round(Rate);
                            }

                            SingleContract.Charges.FirstOrDefault(c => c.Fld == "F001").Val1 = Convert.ToDecimal(Freight);
                            Model.Charges = SingleContract.Charges;
                        }
                        if (SingleContract.Service == "100001")
                        {
                            double Freight = 0;
                            var Rate = SingleContract.Rate;
                            if (SingleContract.TypeOfChrg == "KG")
                            {
                                Freight = Math.Round(Rate * Model.TotQty);
                            }
                            else
                            {
                                Freight = Math.Round(Rate);
                            }
                            SingleContract.Charges.FirstOrDefault(c => c.Fld == "F001").Val1 = Convert.ToDecimal(Freight);
                            Model.Charges = SingleContract.Charges;
                        }
                    }
                }
            }
            else
            {
                conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();
                if (conMaster != null)
                {
                    SingleContract = (from ConDetail in ctxTFAT.ConDetail
                                      where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                      select new ContractList()
                                      {
                                          Sno = ConDetail.SrNo,
                                          Service = ConDetail.Services,
                                          TypeOfChrg = ConDetail.WtType,
                                          FromWT = ConDetail.Wtfrom.Value,
                                          ToWT = ConDetail.WtTo.Value,
                                          Rate = (float)ConDetail.Rate,
                                          ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                          ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                          ConDetilsCode = ConDetail.Code,
                                      }).FirstOrDefault();
                }

                if (SingleContract.Service == null)
                {
                    string FromBranch = Model.Source, Tobranch = Model.Dest;
                    TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        FromBranch = tfatBranch.Grp;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }

                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                    if (conMaster != null)
                    {
                        SingleContract = (from ConDetail in ctxTFAT.ConDetail
                                          where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                          select new ContractList()
                                          {
                                              Sno = ConDetail.SrNo,
                                              Service = ConDetail.Services,
                                              TypeOfChrg = ConDetail.WtType,
                                              FromWT = ConDetail.Wtfrom.Value,
                                              ToWT = ConDetail.WtTo.Value,
                                              Rate = (float)ConDetail.Rate,
                                              ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                              ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                              ConDetilsCode = ConDetail.Code,
                                          }).FirstOrDefault();
                    }

                }
                if (SingleContract != null)
                {
                    if (SingleContract.Service != null)
                    {
                        SingleContract.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                        Model.Charges = SingleContract.Charges;
                    }
                }
            }


            string Status = "", Message = "";
            if (Model.ChgType == "100000" || Model.ChgType == "100001")
            {
                if (SingleContract != null)
                {
                    if (SingleContract.Service == null)
                    {

                        if (GeneralContract)
                        {
                            Status = "Error";
                            Message = "Contract Not Found";
                        }

                        else if (Model.ChgType == "100000")
                        {
                            var ConDetail = ctxTFAT.ConDetail.Where(x => x.CustCode == conMaster.Code && x.Services == "100001").ToList();
                            if (ConDetail.Count() > 0)
                            {
                                Status = "OtherFunction";
                                Message = "Not Found Weight Contract...\n Find Carton Rate Accept ?";
                            }
                            else
                            {
                                Status = "Error";
                                Message = "Contract Not Found";
                            }

                        }
                        else
                        {
                            var ConDetail = ctxTFAT.ConDetail.Where(x => x.CustCode == conMaster.Code && x.Services == "100000").ToList();
                            if (ConDetail.Count() > 0)
                            {
                                Status = "OtherFunction";
                                Message = "Not Found Carton Contract...\n Find Weight Rate Accept ?";
                            }
                            else
                            {
                                Status = "Error";
                                Message = "Contract Not Found";
                            }
                        }
                    }
                    else
                    {
                        Status = "Success";
                    }
                }
                else
                {
                    if (GeneralContract)
                    {
                        Status = "Error";
                        Message = "Contract Not Found";
                    }

                    else if (Model.ChgType == "100000")
                    {
                        var ConDetail = ctxTFAT.ConDetail.Where(x => x.CustCode == conMaster.Code && x.Services == "100001").ToList();
                        if (ConDetail.Count() > 0)
                        {
                            Status = "OtherFunction";
                            Message = "Not Found Weight Contract...\n Find Carton Rate Accept ?";
                        }
                        else
                        {
                            Status = "Error";
                            Message = "Contract Not Found";
                        }

                    }
                    else
                    {
                        var ConDetail = ctxTFAT.ConDetail.Where(x => x.CustCode == conMaster.Code && x.Services == "100000").ToList();
                        if (ConDetail.Count() > 0)
                        {
                            Status = "OtherFunction";
                            Message = "Not Found Carton Contract...\n Find Weight Rate Accept ?";
                        }
                        else
                        {
                            Status = "Error";
                            Message = "Contract Not Found";
                        }
                    }
                }

            }
            else
            {
                if (SingleContract != null)
                {
                    if (SingleContract.Service == null)
                    {
                        Status = "Error";
                        Message = "Contract Not Found";
                    }
                    else
                    {
                        Status = "Success";
                    }
                }
                else
                {
                    Status = "Error";
                    Message = "Contract Not Found";
                }
            }


            //var html = ViewHelper.RenderPartialView(this, "_Charges", Model);
            //var jsonResult = Json(new
            //{
            //    Status = Status,
            //    Message = Message,
            //    Html = html
            //}, JsonRequestBehavior.AllowGet);
            //return jsonResult;
            List<decimal> ChargesListSelect = new List<decimal>();
            if (Model.Charges != null)
            {
                foreach (var item in Model.Charges)
                {
                    ChargesListSelect.Add(item.Val1);
                }
            }

            return Json(new
            {
                Status = Status,
                Message = Message,
                ChargesListSelect = ChargesListSelect
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult OTherContract(LRVM Model)
        {
            var mLrmaster = ctxTFAT.LRMaster.Where(x => x.LrNo == Model.LrNo).Select(x => x).FirstOrDefault();
            Model.BillParty = mLrmaster.BillParty;
            Model.Source = mLrmaster.Source;
            Model.Dest = mLrmaster.Dest;
            Model.ChgType = mLrmaster.ChgType;
            Model.ActWt = mLrmaster.ActWt;
            Model.ChgWt = mLrmaster.ChgWt;
            Model.TotQty = mLrmaster.TotQty;



            var FromDt = mLrmaster.BookDate;
            Model.BillParty = ctxTFAT.CustomerMaster.Where(x => x.Code == Model.BillParty).Select(x => x.AccountParentGroup).FirstOrDefault();
            ContractList SingleContract = new ContractList();
            List<ContractList> WeightCartoon = new List<ContractList>();
            bool Weight = false; string OthCode = "", OthName = "";
            if (Model.ChgType == "100000")
            {
                Weight = true;
            }
            else
            {
                Weight = false;
            }

            string FromBranch = Model.Source, Tobranch = Model.Dest;
            TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
            {
                FromBranch = tfatBranch.Grp;
            }
            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
            {
                Tobranch = tfatBranch.Grp;
            }

            if (Weight)
            {
                OthCode = "100001";
                OthName = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == OthCode).Select(x => x.ChargeType).FirstOrDefault();
                ConMaster conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();
                if (conMaster != null)
                {
                    WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                     where ConDetail.CustCode == conMaster.Code && ConDetail.Services == "100001"
                                     select new ContractList()
                                     {
                                         Sno = ConDetail.SrNo,
                                         Service = ConDetail.Services,
                                         TypeOfChrg = ConDetail.WtType,
                                         FromWT = ConDetail.Wtfrom.Value,
                                         ToWT = ConDetail.WtTo.Value,
                                         Rate = (float)ConDetail.Rate,
                                         ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                         ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                         ConDetilsCode = ConDetail.Code,
                                     }).ToList();
                }
                if (WeightCartoon == null || WeightCartoon.Count() == 0)
                {
                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                    if (conMaster != null)
                    {
                        WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                         where ConDetail.CustCode == conMaster.Code && ConDetail.Services == "100001"
                                         select new ContractList()
                                         {
                                             Sno = ConDetail.SrNo,
                                             Service = ConDetail.Services,
                                             TypeOfChrg = ConDetail.WtType,
                                             FromWT = ConDetail.Wtfrom.Value,
                                             ToWT = ConDetail.WtTo.Value,
                                             Rate = (float)ConDetail.Rate,
                                             ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                             ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                             ConDetilsCode = ConDetail.Code,
                                         }).ToList();
                    }

                }
            }
            else
            {
                OthCode = "100000";
                OthName = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == OthCode).Select(x => x.ChargeType).FirstOrDefault();
                ConMaster conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();
                if (conMaster != null)
                {
                    WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                     where ConDetail.CustCode == conMaster.Code && ConDetail.Services == "100000"
                                     select new ContractList()
                                     {
                                         Sno = ConDetail.SrNo,
                                         Service = ConDetail.Services,
                                         TypeOfChrg = ConDetail.WtType,
                                         FromWT = ConDetail.Wtfrom.Value,
                                         ToWT = ConDetail.WtTo.Value,
                                         Rate = (float)ConDetail.Rate,
                                         ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                         ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                         ConDetilsCode = ConDetail.Code,
                                     }).ToList();
                }

                if (WeightCartoon == null || WeightCartoon.Count() == 0)
                {
                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                    if (conMaster != null)
                    {
                        WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                         where ConDetail.CustCode == conMaster.Code && ConDetail.Services == "100000"
                                         select new ContractList()
                                         {
                                             Sno = ConDetail.SrNo,
                                             Service = ConDetail.Services,
                                             TypeOfChrg = ConDetail.WtType,
                                             FromWT = ConDetail.Wtfrom.Value,
                                             ToWT = ConDetail.WtTo.Value,
                                             Rate = (float)ConDetail.Rate,
                                             ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                             ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                             ConDetilsCode = ConDetail.Code,
                                         }).ToList();
                    }

                }
            }

            foreach (var item in WeightCartoon)
            {
                if (item.Service == "100000")
                {
                    if (item.ChargeOfChrgWT)
                    {
                        SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ChgWt && x.ToWT >= Model.ChgWt).FirstOrDefault();
                        if (SingleContract != null)
                        {
                            item.Charges = GetChargesOfService(item.ConDetilsCode, item.Service, item.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }
                    else
                    {
                        SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                        if (SingleContract != null)
                        {
                            item.Charges = GetChargesOfService(item.ConDetilsCode, item.Service, item.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }
                }
                else
                {
                    SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.TotQty && x.ToWT >= Model.TotQty).FirstOrDefault();
                    if (SingleContract != null)
                    {
                        item.Charges = GetChargesOfService(item.ConDetilsCode, item.Service, item.Sno);
                        SingleContract.Charges = item.Charges;
                        break;
                    }
                }

            }

            if (SingleContract != null)
            {
                if (SingleContract.Service != null)
                {
                    if (SingleContract.Service == "100000")
                    {
                        double Freight = 0;
                        var Rate = SingleContract.Rate;
                        if (SingleContract.TypeOfChrg == "KG")
                        {
                            if (SingleContract.ChargeOfChrgWT)
                            {
                                Freight = Math.Round(Rate * Model.ChgWt);
                            }
                            else
                            {
                                Freight = Math.Round(Rate * Model.ActWt);
                            }
                        }
                        else
                        {
                            Freight = Math.Round(Rate);
                        }

                        SingleContract.Charges.FirstOrDefault(c => c.Fld == "F001").Val1 = Convert.ToDecimal(Freight);
                        Model.Charges = SingleContract.Charges;
                    }
                    if (SingleContract.Service == "100001")
                    {
                        double Freight = 0;
                        var Rate = SingleContract.Rate;
                        if (SingleContract.TypeOfChrg == "KG")
                        {
                            Freight = Math.Round(Rate * Model.TotQty);
                        }
                        else
                        {
                            Freight = Math.Round(Rate);
                        }
                        SingleContract.Charges.FirstOrDefault(c => c.Fld == "F001").Val1 = Convert.ToDecimal(Freight);
                        Model.Charges = SingleContract.Charges;
                    }
                }
            }


            string Status = "", Message = "";
            if (SingleContract != null)
            {
                if (SingleContract.Service == null)
                {
                    Status = "Error";
                    Message = "Contract Not Found";
                }
                else
                {
                    Status = "Success";
                }
            }
            else
            {
                Status = "Error";
                Message = "Contract Not Found";
            }



            //var html = ViewHelper.RenderPartialView(this, "_Charges", Model);
            //var jsonResult = Json(new
            //{
            //    Status = Status,
            //    Message = Message,
            //    OthCode = OthCode,
            //    OthName = OthName,
            //    Html = html
            //}, JsonRequestBehavior.AllowGet);
            //return jsonResult;

            List<decimal> ChargesListSelect = new List<decimal>();
            foreach (var item in Model.Charges)
            {
                ChargesListSelect.Add(item.Val1);
            }

            return Json(new
            {
                Status = Status,
                Message = Message,
                ChargesListSelect = ChargesListSelect
            }, JsonRequestBehavior.AllowGet);
        }

        public List<PurchaseVM> GetChargesOfService(string code, string Service, int Sno)
        {
            List<PurchaseVM> purchases = new List<PurchaseVM>();

            var trncharges = ctxTFAT.Charges.Where(x => x.MainType == "SL" && x.SubType == "RS" && x.Type == "LR000" && x.DontUse == false).Select(x => new { x.Fld, x.Head, x.EqAmt, x.Equation, x.Code }).ToList();
            foreach (var i in trncharges)
            {
                PurchaseVM c = new PurchaseVM();
                c.Fld = i.Fld;
                c.Code = i.Head;
                c.AddLess = i.EqAmt;
                c.Equation = i.Equation;
                c.tempId = Convert.ToInt16(i.Fld.Substring(1, 3));
                c.Val1 = GetLRChargeValValue(code, Service, Sno, "Val", c.tempId);
                c.Type = GetLRChargeValType(code, Service, Sno, "Flg", c.tempId);
                purchases.Add(c);
            }
            return purchases;
        }

        public decimal GetLRChargeValValue(string code, string Service, int Sno, string Val, int i)
        {
            string connstring = GetConnectionString();
            string abc;

            var loginQuery3 = @"select " + Val + i + " from ConDetail where Code='" + code + "' and Services='" + Service + "' and SrNo=" + Sno + " ";
            DataTable mDt2 = GetDataTable(loginQuery3, connstring);
            if (mDt2.Rows.Count > 0)
            {
                abc = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? "0" : mDt2.Rows[0][0].ToString();
            }
            else
            {
                abc = "0";
            }
            return Convert.ToDecimal(abc);
        }

        public string GetLRChargeValType(string code, string Service, int Sno, string Val, int i)
        {
            string connstring = GetConnectionString();
            string abc;

            var loginQuery3 = @"select " + Val + i + " from ConDetail where Code='" + code + "' and Services='" + Service + "' and SrNo=" + Sno + " ";
            DataTable mDt2 = GetDataTable(loginQuery3, connstring);
            if (mDt2.Rows.Count > 0)
            {
                abc = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? "" : mDt2.Rows[0][0].ToString();
            }
            else
            {
                abc = "";
            }
            return abc;
        }

        #endregion

        public ActionResult GetGSTCalculation(LRInvoiceVM Model)
        {
            string pgst;

            string ourstatename = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.State).FirstOrDefault();
            if (ourstatename == null || ourstatename == "")
                ourstatename = "19";
            ourstatename = ourstatename.ToUpper();


            string partystate = Model.PlaceOfSupply;
            if (partystate == null || partystate == "")
            {
                partystate = "19";
            }
            partystate = partystate.ToUpper();

            if (partystate == ourstatename && Model.GSTType != "7")
            {
                var result = (from i in ctxTFAT.TaxMaster where i.Code == Model.GSTCode select new { i.Code, i.Inclusive, i.CGSTRate, i.SGSTRate, IGSTRate = 0, i.DiscOnTxbl }).FirstOrDefault();
                if (result != null)
                {
                    Model.Inclusive = result.Inclusive;
                    Model.CGSTRate = (result.CGSTRate == null || Model.GSTType == "8" || Model.GSTType == "11") ? 0 : result.CGSTRate.Value;
                    Model.SGSTRate = (result.SGSTRate == null || Model.GSTType == "8" || Model.GSTType == "11") ? 0 : result.SGSTRate.Value;
                    Model.IGSTRate = 0;
                    Model.DiscOnTaxable = result.DiscOnTxbl;
                }

                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            else
            {

                var result = (from i in ctxTFAT.TaxMaster
                              where i.Code == Model.GSTCode
                              select new
                              {
                                  i.Code,
                                  i.Inclusive,
                                  i.IGSTRate,
                                  SGSTRate = 0,
                                  CGSTRate = 0,
                                  i.DiscOnTxbl
                              }).FirstOrDefault();
                if (result != null)
                {
                    Model.Inclusive = result.Inclusive;
                    Model.CGSTRate = 0;
                    Model.SGSTRate = 0;
                    Model.IGSTRate = (result.IGSTRate == null || Model.GSTType == "8" || Model.GSTType == "11") ? 0 : result.IGSTRate.Value;
                    Model.DiscOnTaxable = result.DiscOnTxbl;
                }

                Model.CGSTAmt = Model.Taxable * (Model.CGSTRate / 100);
                Model.SGSTAmt = Model.Taxable * (Model.SGSTRate / 100);
                Model.IGSTAmt = Model.Taxable * (Model.IGSTRate / 100);
                Model.InvoiceAmt = Model.Taxable + Model.CGSTAmt + Model.SGSTAmt + Model.IGSTAmt;

                var datenow = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                var TDSRATEtab = ctxTFAT.TDSRates.Where(x => x.Code.ToString() == Model.TDSCode && x.EffDate <= datenow && x.LimitFrom <= Model.Taxable).OrderByDescending(x => x.EffDate).Select(x => new { x.TDSRate, x.Cess, x.SHECess, x.SurCharge }).FirstOrDefault();
                if (TDSRATEtab != null && Model.CutTDS == true && Model.LRCutTDS == true)
                {
                    Model.TDSAmt = (Model.TDSAmt != 0) ? Model.TDSAmt : (TDSRATEtab.TDSRate == null ? 0 : Math.Round(((TDSRATEtab.TDSRate.Value * Model.Taxable) / 100), 0));
                }
                else
                {
                    Model.TDSAmt = 0;
                }
                decimal mamtm = Model.InvoiceAmt;
                //if (Model.CutTDS == true && Model.LRCutTDS == true)//for tcs
                //{
                //    mamtm = mamtm + Model.TDSAmt;
                //}

                Model.InvoiceAmt = mamtm;

                return Json(Model, JsonRequestBehavior.AllowGet);
            }


        }

        public LRInvoiceVM GetGSTCalculationOnScan(LRInvoiceVM Model)
        {
            string pgst;
            if (Model.VATGSTApp == "G")
            {

                string ourstatename = ctxTFAT.Warehouse.Where(x => x.Code == Model.LocationCode).Select(x => x.State).FirstOrDefault();
                if (ourstatename == null || ourstatename == "")
                    ourstatename = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.State).FirstOrDefault();
                if (ourstatename == null || ourstatename == "")
                    ourstatename = "MAHARASHTRA";
                ourstatename = ourstatename.ToUpper();

                //string partystate = (from i in ctxTFAT.Address where i.Code == Model.DelyCode && i.Sno == Model.Sno select i.State).FirstOrDefault().Trim();
                //if (partystate == null || partystate == "")
                string partystate = Model.PlaceOfSupply;
                if (partystate == null || partystate == "")
                {
                    partystate = "MAHARASHTRA";
                }
                partystate = partystate.ToUpper();

                if (partystate == ourstatename && Model.GSTType != "7")
                {
                    if (Model.MainType == "PR")
                    {
                        if (Model.GSTCode != "0")
                        {
                            pgst = Model.GSTCode;
                        }
                        else
                        {
                            pgst = (from i in ctxTFAT.ItemDetail where i.Code == Model.Code && i.Branch == mbranchcode select i.PGSTCode).FirstOrDefault();
                        }
                    }
                    else
                    {
                        if (Model.GSTCode != "0")
                        {
                            pgst = Model.GSTCode;
                        }
                        else
                        {
                            pgst = (from i in ctxTFAT.ItemDetail where i.Code == Model.Code && i.Branch == mbranchcode select i.SGSTCode).FirstOrDefault();
                        }
                    }
                    var result = (from i in ctxTFAT.TaxMaster where i.Code == pgst select new { i.Code, i.Inclusive, i.CGSTRate, i.SGSTRate, IGSTRate = 0, i.DiscOnTxbl }).FirstOrDefault();
                    if (result != null)
                    {
                        Model.Inclusive = result.Inclusive;
                        Model.CGSTRate = (result.CGSTRate == null || Model.GSTType == "8" || Model.GSTType == "11") ? 0 : result.CGSTRate.Value;
                        Model.SGSTRate = (result.SGSTRate == null || Model.GSTType == "8" || Model.GSTType == "11") ? 0 : result.SGSTRate.Value;
                        Model.IGSTRate = 0;
                        Model.DiscOnTaxable = result.DiscOnTxbl;
                    }
                    if (Model.GSTType == "6")
                    {
                        Model.Inclusive = true;
                    }
                    if (Model.GSTType == "1")
                    {
                        Model.Inclusive = false;
                    }
                    return Model;
                }
                else
                {
                    if (Model.MainType == "PR")
                    {
                        if (Model.GSTCode != "0")
                        {
                            pgst = Model.GSTCode;
                        }
                        else
                        {
                            pgst = (from i in ctxTFAT.ItemDetail where i.Code == Model.Code && i.Branch == mbranchcode select i.PGSTCode).FirstOrDefault();
                        }
                    }
                    else
                    {
                        if (Model.GSTCode != "0")
                        {
                            pgst = Model.GSTCode;
                        }
                        else
                        {
                            pgst = (from i in ctxTFAT.ItemDetail where i.Code == Model.Code && i.Branch == mbranchcode select i.SGSTCode).FirstOrDefault();
                        }
                    }
                    var result = (from i in ctxTFAT.TaxMaster
                                  where i.Code == pgst
                                  select new
                                  {
                                      i.Code,
                                      i.Inclusive,
                                      i.IGSTRate,
                                      SGSTRate = 0,
                                      CGSTRate = 0,
                                      i.DiscOnTxbl
                                  }).FirstOrDefault();
                    if (result != null)
                    {
                        Model.Inclusive = result.Inclusive;
                        Model.CGSTRate = 0;
                        Model.SGSTRate = 0;
                        Model.IGSTRate = (result.IGSTRate == null || Model.GSTType == "8" || Model.GSTType == "11") ? 0 : result.IGSTRate.Value;
                        Model.DiscOnTaxable = result.DiscOnTxbl;
                    }
                    if (Model.GSTType == "6")
                    {
                        Model.Inclusive = true;
                    }
                    if (Model.GSTType == "1")
                    {
                        Model.Inclusive = false;
                    }

                    return Model;
                }
            }
            else if (Model.VATGSTApp == "V")//vatgstapp = V
            {

                if (Model.MainType == "PR")
                {
                    if (Model.GSTCode != "0")
                    {
                        pgst = Model.GSTCode;
                    }
                    else
                    {
                        pgst = (from i in ctxTFAT.ItemDetail where i.Code == Model.Code && i.Branch == mbranchcode select i.PGSTCode).FirstOrDefault();
                    }
                }
                else
                {
                    if (Model.GSTCode != "0")
                    {
                        pgst = Model.GSTCode;
                    }
                    else
                    {
                        pgst = (from i in ctxTFAT.ItemDetail where i.Code == Model.Code && i.Branch == mbranchcode select i.SGSTCode).FirstOrDefault();
                    }
                }
                var result = (from i in ctxTFAT.TaxMaster where i.Code == pgst select new { i.Code, i.Inclusive, i.CGSTRate, i.SGSTRate, i.IGSTRate, i.DiscOnTxbl }).FirstOrDefault();
                if (result != null)
                {
                    Model.Inclusive = result.Inclusive;
                    Model.IGSTRate = result.IGSTRate == null ? 0 : result.IGSTRate.Value;
                    Model.SGSTRate = result.SGSTRate == null ? 0 : (result.SGSTRate.Value / 100) * result.IGSTRate.Value;
                    Model.CGSTRate = result.CGSTRate == null ? 0 : result.CGSTRate.Value;
                    Model.DiscOnTaxable = result.DiscOnTxbl;
                }

                return Model;


            }
            else
            {
                Model.Inclusive = false;
                Model.IGSTRate = 0;
                Model.SGSTRate = 0;
                Model.CGSTRate = 0;
                return Model;
            }
        }

        private void SaveOutstandingDetails(LRInvoiceVM Model)
        {
            int mCnt = 1;
            if (Model.OSAdjList != null)
            {
                foreach (var item1 in Model.OSAdjList)
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
                        osobj1.Srl = Model.Srl.ToString(); ;
                        osobj1.Sno = mCnt;
                        osobj1.ParentKey = Model.ParentKey;
                        osobj1.TableKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + mCnt.ToString("D3") + Model.Srl.ToString(); ;
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
                        osobj1.Code = Model.Account;
                        osobj1.CrPeriod = 0;
                        osobj1.CurrName = 1;
                        osobj1.CurrRate = 1;
                        osobj1.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                        osobj1.Narr = "";
                        osobj1.OrdDate = DateTime.Now;
                        osobj1.OrdNumber = "";
                        osobj1.ProjCode = "";
                        osobj1.ProjectStage = 0;
                        osobj1.ProjectUnit = 0;
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
                        osobj1.LocationCode = Model.LocationCode;

                        ctxTFAT.Outstanding.Add(osobj1);

                        // second effect
                        Outstanding osobj2 = new Outstanding();

                        osobj2.Branch = mbranchcode;
                        osobj2.DocBranch = mbranchcode;
                        osobj2.aType = Model.Type;
                        osobj2.aPrefix = Model.Prefix;
                        osobj2.aSrl = Model.Srl.ToString(); ;
                        osobj2.aSno = mCnt;
                        osobj2.ParentKey = Model.ParentKey;
                        osobj2.TableRefKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + mCnt.ToString("D3") + Model.Srl.ToString(); ;
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
                        osobj2.Code = Model.Account;
                        osobj2.CrPeriod = 0;
                        osobj2.CurrName = 1;
                        osobj2.CurrRate = 1;
                        osobj2.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                        osobj2.Narr = "";
                        osobj2.OrdDate = DateTime.Now;
                        osobj2.OrdNumber = "";
                        osobj2.ProjCode = "";
                        osobj2.ProjectStage = 0;
                        osobj2.ProjectUnit = 0;
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
                        osobj2.LocationCode = Model.LocationCode;

                        ctxTFAT.Outstanding.Add(osobj2);

                    }
                    mCnt = mCnt + 1;
                }
            }
        }

        public void SaveItemAddons(List<AddOns> AddOnList, int mSno, string Code, LRInvoiceVM Model, string TableKey)
        {
            string addop1, addop2;
            StringBuilder addonTp = new StringBuilder();
            List<string> addlistp = new List<string>();
            if (AddOnList != null && AddOnList.Count > 0)
            {
                if (Model.MainType == "PR")
                {
                    var addoni = ctxTFAT.AddonItemPR.Where(x => x.ParentKey == Model.ParentKey && x.TableKey == TableKey).Select(x => x).FirstOrDefault();
                    if (addoni != null)
                    {
                        ctxTFAT.AddonItemPR.Remove(addoni);
                    }
                }
                else
                {
                    var addoni = ctxTFAT.AddonItemSL.Where(x => x.ParentKey == Model.ParentKey && x.TableKey == TableKey).Select(x => x).FirstOrDefault();
                    if (addoni != null)
                    {
                        ctxTFAT.AddonItemSL.Remove(addoni);
                    }
                }

                if (Model.MainType == "PR")
                {
                    AddonItemPR aip = new AddonItemPR();
                    aip.AUTHIDS = muserid;
                    aip.AUTHORISE = "A00";
                    aip.Code = Code;
                    aip.DocDate = DateTime.Now;
                    aip.ENTEREDBY = muserid;
                    aip.F001 = AddOnList.Where(x => x.Fld == "F001").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F002 = AddOnList.Where(x => x.Fld == "F002").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F003 = AddOnList.Where(x => x.Fld == "F003").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F004 = AddOnList.Where(x => x.Fld == "F004").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F005 = AddOnList.Where(x => x.Fld == "F005").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F005").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F006 = AddOnList.Where(x => x.Fld == "F006").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F006").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F007 = AddOnList.Where(x => x.Fld == "F007").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F007").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F008 = AddOnList.Where(x => x.Fld == "F008").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F009 = AddOnList.Where(x => x.Fld == "F009").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F009").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F010 = AddOnList.Where(x => x.Fld == "F010").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F010").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F011 = AddOnList.Where(x => x.Fld == "F011").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F011").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F012 = AddOnList.Where(x => x.Fld == "F012").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F013 = AddOnList.Where(x => x.Fld == "F013").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F013").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F014 = AddOnList.Where(x => x.Fld == "F014").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F014").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F015 = AddOnList.Where(x => x.Fld == "F015").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F015").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F016 = AddOnList.Where(x => x.Fld == "F016").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F016").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F017 = AddOnList.Where(x => x.Fld == "F017").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F017").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F018 = AddOnList.Where(x => x.Fld == "F018").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F018").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F019 = AddOnList.Where(x => x.Fld == "F019").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F019").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F020 = AddOnList.Where(x => x.Fld == "F020").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F020").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F021 = AddOnList.Where(x => x.Fld == "F021").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F021").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F022 = AddOnList.Where(x => x.Fld == "F022").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F022").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F023 = AddOnList.Where(x => x.Fld == "F023").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F023").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F024 = AddOnList.Where(x => x.Fld == "F024").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F024").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F025 = AddOnList.Where(x => x.Fld == "F025").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F025").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F026 = AddOnList.Where(x => x.Fld == "F026").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F026").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F027 = AddOnList.Where(x => x.Fld == "F027").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F027").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F028 = AddOnList.Where(x => x.Fld == "F028").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F028").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F029 = AddOnList.Where(x => x.Fld == "F029").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F029").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F030 = AddOnList.Where(x => x.Fld == "F030").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F030").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F031 = AddOnList.Where(x => x.Fld == "F031").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F031").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F032 = AddOnList.Where(x => x.Fld == "F032").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F032").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F033 = AddOnList.Where(x => x.Fld == "F033").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F033").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F034 = AddOnList.Where(x => x.Fld == "F034").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F034").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F035 = AddOnList.Where(x => x.Fld == "F035").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F035").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F036 = AddOnList.Where(x => x.Fld == "F036").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F036").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F037 = AddOnList.Where(x => x.Fld == "F037").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F037").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F038 = AddOnList.Where(x => x.Fld == "F038").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F038").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F039 = AddOnList.Where(x => x.Fld == "F039").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F039").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F040 = AddOnList.Where(x => x.Fld == "F040").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F040").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F041 = AddOnList.Where(x => x.Fld == "F041").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F041").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F042 = AddOnList.Where(x => x.Fld == "F042").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F042").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F043 = AddOnList.Where(x => x.Fld == "F043").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F043").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F044 = AddOnList.Where(x => x.Fld == "F044").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F044").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F045 = AddOnList.Where(x => x.Fld == "F045").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F045").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F046 = AddOnList.Where(x => x.Fld == "F046").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F046").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F047 = AddOnList.Where(x => x.Fld == "F047").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F047").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F048 = AddOnList.Where(x => x.Fld == "F048").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F048").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F049 = AddOnList.Where(x => x.Fld == "F049").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F049").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F050 = AddOnList.Where(x => x.Fld == "F050").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F050").Select(x => x.ApplCode).FirstOrDefault() : "0";

                    aip.LASTUPDATEDATE = DateTime.Now;
                    aip.ParentKey = Model.ParentKey;
                    aip.SrNo = mSno;
                    aip.TableKey = TableKey;
                    aip.Type = Model.Type;
                    ctxTFAT.AddonItemPR.Add(aip);
                }
                else
                {
                    AddonItemSL aip = new AddonItemSL();
                    aip.AUTHIDS = muserid;
                    aip.AUTHORISE = "A00";
                    aip.Code = Code;
                    aip.DocDate = DateTime.Now;
                    aip.ENTEREDBY = muserid;
                    aip.F001 = AddOnList.Where(x => x.Fld == "F001").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F002 = AddOnList.Where(x => x.Fld == "F002").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F003 = AddOnList.Where(x => x.Fld == "F003").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F004 = AddOnList.Where(x => x.Fld == "F004").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F005 = AddOnList.Where(x => x.Fld == "F005").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F005").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F006 = AddOnList.Where(x => x.Fld == "F006").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F006").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F007 = AddOnList.Where(x => x.Fld == "F007").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F007").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F008 = AddOnList.Where(x => x.Fld == "F008").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F009 = AddOnList.Where(x => x.Fld == "F009").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F009").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F010 = AddOnList.Where(x => x.Fld == "F010").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F010").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F011 = AddOnList.Where(x => x.Fld == "F011").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F011").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F012 = AddOnList.Where(x => x.Fld == "F012").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F013 = AddOnList.Where(x => x.Fld == "F013").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F013").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F014 = AddOnList.Where(x => x.Fld == "F014").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F014").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F015 = AddOnList.Where(x => x.Fld == "F015").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F015").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F016 = AddOnList.Where(x => x.Fld == "F016").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F016").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F017 = AddOnList.Where(x => x.Fld == "F017").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F017").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F018 = AddOnList.Where(x => x.Fld == "F018").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F018").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F019 = AddOnList.Where(x => x.Fld == "F019").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F019").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F020 = AddOnList.Where(x => x.Fld == "F020").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F020").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F021 = AddOnList.Where(x => x.Fld == "F021").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F021").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F022 = AddOnList.Where(x => x.Fld == "F022").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F022").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F023 = AddOnList.Where(x => x.Fld == "F023").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F023").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F024 = AddOnList.Where(x => x.Fld == "F024").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F024").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F025 = AddOnList.Where(x => x.Fld == "F025").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F025").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F026 = AddOnList.Where(x => x.Fld == "F026").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F026").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F027 = AddOnList.Where(x => x.Fld == "F027").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F027").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F028 = AddOnList.Where(x => x.Fld == "F028").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F028").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F029 = AddOnList.Where(x => x.Fld == "F029").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F029").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F030 = AddOnList.Where(x => x.Fld == "F030").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F030").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F031 = AddOnList.Where(x => x.Fld == "F031").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F031").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F032 = AddOnList.Where(x => x.Fld == "F032").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F032").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F033 = AddOnList.Where(x => x.Fld == "F033").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F033").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F034 = AddOnList.Where(x => x.Fld == "F034").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F034").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F035 = AddOnList.Where(x => x.Fld == "F035").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F035").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F036 = AddOnList.Where(x => x.Fld == "F036").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F036").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F037 = AddOnList.Where(x => x.Fld == "F037").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F037").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F038 = AddOnList.Where(x => x.Fld == "F038").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F038").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F039 = AddOnList.Where(x => x.Fld == "F039").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F039").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F040 = AddOnList.Where(x => x.Fld == "F040").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F040").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F041 = AddOnList.Where(x => x.Fld == "F041").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F041").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F042 = AddOnList.Where(x => x.Fld == "F042").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F042").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F043 = AddOnList.Where(x => x.Fld == "F043").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F043").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F044 = AddOnList.Where(x => x.Fld == "F044").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F044").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F045 = AddOnList.Where(x => x.Fld == "F045").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F045").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F046 = AddOnList.Where(x => x.Fld == "F046").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F046").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F047 = AddOnList.Where(x => x.Fld == "F047").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F047").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F048 = AddOnList.Where(x => x.Fld == "F048").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F048").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F049 = AddOnList.Where(x => x.Fld == "F049").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F049").Select(x => x.ApplCode).FirstOrDefault() : "0";
                    aip.F050 = AddOnList.Where(x => x.Fld == "F050").Select(x => x) != null ? AddOnList.Where(x => x.Fld == "F050").Select(x => x.ApplCode).FirstOrDefault() : "0";

                    aip.LASTUPDATEDATE = DateTime.Now;
                    aip.ParentKey = Model.ParentKey;
                    aip.SrNo = mSno;
                    aip.TableKey = TableKey;
                    aip.Type = Model.Type;
                    ctxTFAT.AddonItemSL.Add(aip);
                }
            }
        }

        private void SaveAddons(LRInvoiceVM Model)
        {
            string addo1, addo2;
            StringBuilder addonT = new StringBuilder();
            List<string> addlist = new List<string>();
            if (Model.AddOnList != null && Model.AddOnList.Count > 0)
            {

                if (Model.MainType == "PR")
                {
                    var addoni = ctxTFAT.AddonDocPR.Where(x => x.TableKey == Model.ParentKey).Select(x => x).FirstOrDefault();
                    if (addoni != null)
                    {
                        ctxTFAT.AddonDocPR.Remove(addoni);
                    }

                }
                else
                {
                    var addoni = ctxTFAT.AddonDocSL.Where(x => x.TableKey == Model.ParentKey).Select(x => x).FirstOrDefault();
                    if (addoni != null)
                    {
                        ctxTFAT.AddonDocSL.Remove(addoni);
                    }
                }

                if (Model.MainType == "PR")
                {
                    AddonDocPR aip = new AddonDocPR();
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
                    ctxTFAT.AddonDocPR.Add(aip);
                }
                else
                {
                    AddonDocSL aip = new AddonDocSL();
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
                    ctxTFAT.AddonDocSL.Add(aip);
                }
            }
        }

        private void SaveTDSPayments(LRInvoiceVM Model)
        {
            var tdsrates = ctxTFAT.TDSRates.Where(x => x.Code.ToString() == Model.TDSCode).Select(x => new { x.Tax, x.Cess, x.SHECess, x.SurCharge, x.TDSRate }).FirstOrDefault();
            if (Model.TDSFlag == true && Model.TDSAmt > 0)
            {
                TDSPayments tspay = new TDSPayments();
                tspay.aMainType = Model.MainType;
                tspay.Amount = Model.Amt;
                tspay.aPrefix = Model.Prefix;
                tspay.aSno = 1;
                tspay.aSrl = Model.Srl.ToString(); ;
                tspay.SubType = Model.SubType;
                tspay.aSubType = Model.SubType;
                tspay.aType = Model.Type;
                tspay.BankCode = "";
                tspay.BillNumber = (Model.BillNumber == null || Model.BillNumber == "") ? "" : Model.BillNumber;
                tspay.Branch = mbranchcode;
                tspay.CertDate = DateTime.Now;
                tspay.CertNumber = "";
                tspay.ChallanDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                tspay.ChallanNumber = "";
                tspay.CNO = "";
                tspay.Code = Model.Account;
                tspay.CompCode = mcompcode;
                tspay.DepositSerial = "";
                tspay.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                tspay.DueDate = DateTime.Now.Date;
                tspay.EndCredit = false;

                tspay.LocationCode = Model.LocationCode;
                tspay.MainType = Model.MainType;
                tspay.Narr = "";
                tspay.PaidAmt = 0;
                tspay.ParentKey = Model.ParentKey;
                tspay.Party = Model.Account;
                tspay.PaymentMode = 0;
                tspay.Prefix = Model.Prefix;
                tspay.Sno = 1;
                tspay.Srl = Model.Srl.ToString(); ;
                tspay.SubType = Model.SubType;
                tspay.TableKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + 1.ToString("D3") + Model.Srl.ToString(); ;
                tspay.TDSAble = Model.TDSAble;
                tspay.TDSAmt = Model.TDSAmt;
                tspay.TDSCess = tdsrates == null ? 0 : tdsrates.Cess;
                tspay.TDSCessAmt = Model.TDSCess;
                tspay.TDSCode = Model.TDSCode == null ? 0 : Convert.ToInt32(Model.TDSCode);
                tspay.TDSReason = 0;
                tspay.TDSSheCess = tdsrates == null ? 0 : tdsrates.SHECess;
                tspay.TDSSheCessAmt = Model.TDSSHECess;
                tspay.TDSSurCharge = tdsrates == null ? 0 : tdsrates.SurCharge;
                tspay.TDSSurChargeAmt = Model.TDSSchg;
                tspay.TDSTax = Model.TDSRate;
                tspay.TDSTaxAmt = 0;
                tspay.TotalTDSAmt = Model.TDSAmt;
                tspay.Type = Model.Type;
                tspay.ENTEREDBY = muserid;
                tspay.LASTUPDATEDATE = DateTime.Now;
                tspay.AUTHORISE = Model.Authorise;
                tspay.AUTHIDS = muserid;
                ctxTFAT.TDSPayments.Add(tspay);


                var mBalCntNoCred = Model.LedgerPostList.Where(x => x.Code == Model.Account && x.Credit == Model.TDSAmt).Select(x => x.tempId).FirstOrDefault();


                Outstanding osobj1 = new Outstanding();

                osobj1.Branch = mbranchcode;
                osobj1.DocBranch = mbranchcode;
                osobj1.MainType = Model.MainType;
                osobj1.SubType = Model.SubType;
                osobj1.Type = Model.Type;
                osobj1.Prefix = Model.Prefix;
                osobj1.Srl = Model.Srl;
                osobj1.Sno = 1;
                osobj1.ParentKey = Model.ParentKey;
                osobj1.TableKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + 1.ToString("D3") + Model.Srl;
                osobj1.aMaintype = Model.MainType;
                osobj1.aSubType = Model.SubType;
                osobj1.aType = Model.Type;
                osobj1.aPrefix = Model.Prefix;
                osobj1.aSrl = Model.Srl;
                osobj1.aSno = 2;
                osobj1.Amount = Model.TDSAmt;
                osobj1.TableRefKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + mBalCntNoCred.ToString("D3") + Model.Srl;
                osobj1.AUTHIDS = muserid;
                osobj1.AUTHORISE = "A00";
                osobj1.BillDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                osobj1.BillNumber = "";
                osobj1.CompCode = mcompcode;
                osobj1.Broker = 100001;
                osobj1.Brokerage = Convert.ToDecimal(0.00);
                osobj1.BrokerAmt = Convert.ToDecimal(0.00);
                osobj1.BrokerOn = Convert.ToDecimal(0.00);
                osobj1.ChlnDate = DateTime.Now;
                osobj1.ChlnNumber = "";
                osobj1.Code = Model.Account;
                osobj1.CrPeriod = 0;
                osobj1.CurrName = 1;
                osobj1.CurrRate = 1;
                osobj1.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                osobj1.Narr = "";
                osobj1.OrdDate = DateTime.Now;
                osobj1.OrdNumber = "";
                osobj1.ProjCode = "";
                osobj1.ProjectStage = 0;
                osobj1.ProjectUnit = 0;
                osobj1.RefParty = "";
                osobj1.SalemanAmt = Convert.ToDecimal(0.00);
                osobj1.SalemanOn = Convert.ToDecimal(0.00);
                osobj1.SalemanPer = Convert.ToDecimal(0.00);
                osobj1.Salesman = 100001;
                osobj1.TDSAmt = 0;
                osobj1.ENTEREDBY = muserid;
                osobj1.LASTUPDATEDATE = DateTime.Now;
                osobj1.CurrAmount = Model.TDSAmt;
                osobj1.ValueDate = DateTime.Now;
                osobj1.LocationCode = 100001;

                ctxTFAT.Outstanding.Add(osobj1);

                // second effect
                Outstanding osobj2 = new Outstanding();

                osobj2.Branch = mbranchcode;
                osobj2.DocBranch = mbranchcode;
                osobj2.ParentKey = Model.ParentKey;
                osobj2.Type = Model.Type;
                osobj2.Prefix = Model.Prefix;
                osobj2.Srl = Model.Srl;
                osobj2.Sno = 2;
                osobj2.TableKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + mBalCntNoCred.ToString("D3") + Model.Srl;
                osobj2.aType = Model.Type;
                osobj2.aPrefix = Model.Prefix;
                osobj2.aSrl = Model.Srl;
                osobj2.aSno = 1;
                osobj2.aMaintype = Model.MainType;
                osobj2.TableRefKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + 1.ToString("D3") + Model.Srl;
                osobj2.MainType = Model.MainType;
                osobj2.SubType = Model.SubType;
                osobj2.aSubType = Model.SubType;
                osobj2.Amount = Model.TDSAmt;
                osobj2.AUTHIDS = muserid;
                osobj2.AUTHORISE = "A00";
                osobj2.BillDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                osobj2.BillNumber = "";
                osobj2.CompCode = mcompcode;
                osobj2.Broker = 100001;
                osobj2.Brokerage = Convert.ToDecimal(0.00);
                osobj2.BrokerAmt = Convert.ToDecimal(0.00);
                osobj2.BrokerOn = Convert.ToDecimal(0.00);
                osobj2.ChlnDate = DateTime.Now;
                osobj2.ChlnNumber = "";
                osobj2.Code = Model.Account;
                osobj2.CrPeriod = 0;
                osobj2.CurrName = 1;
                osobj2.CurrRate = 1;
                osobj2.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                osobj2.Narr = "";
                osobj2.OrdDate = DateTime.Now;
                osobj2.OrdNumber = "";
                osobj2.ProjCode = "";
                osobj2.ProjectStage = 0;
                osobj2.ProjectUnit = 0;
                osobj2.RefParty = "";
                osobj2.SalemanAmt = Convert.ToDecimal(0.00);
                osobj2.SalemanOn = Convert.ToDecimal(0.00);
                osobj2.SalemanPer = Convert.ToDecimal(0.00);
                osobj2.Salesman = 100001;
                osobj2.TDSAmt = 0;
                osobj2.ENTEREDBY = muserid;
                osobj2.LASTUPDATEDATE = DateTime.Now;
                osobj2.CurrAmount = Model.TDSAmt;
                osobj2.ValueDate = DateTime.Now;
                osobj2.LocationCode = 100001;

                ctxTFAT.Outstanding.Add(osobj2);


            }

        }

        private void SaveNarration(LRInvoiceVM Model, string ParentKey)
        {
            if (Model.RichNote != null)
            {
                Narration narr = new Narration();
                narr.Branch = mbranchcode;
                narr.Narr = Model.RichNote;
                narr.NarrRich = Model.RichNote;
                narr.Prefix = mperiod;
                narr.Sno = 0;
                narr.Srl = Model.Srl;
                narr.Type = Model.Type;
                narr.ENTEREDBY = muserid;
                narr.LASTUPDATEDATE = DateTime.Now;
                narr.AUTHORISE = Model.Authorise;
                narr.AUTHIDS = muserid;
                narr.LocationCode = Model.LocationCode;
                narr.TableKey = ParentKey;
                narr.CompCode = mcompcode;
                narr.ParentKey = ParentKey;
                ctxTFAT.Narration.Add(narr);
            }
        }

        public decimal GetChargesVal(List<LRInvoiceVM> Charges, string FToken)
        {
            string connstring = GetConnectionString();
            string sql;
            decimal mamtm;
            var Val = Charges.Where(x => x.Fld == FToken).Select(x => x.Amt).FirstOrDefault();
            var PosNeg = Charges.Where(x => x.Fld == FToken).Select(x => x.AddLess).FirstOrDefault();
            sql = @"Select Top 1 " + PosNeg + Val + " from TfatComp";
            DataTable smDt = GetDataTable(sql, connstring);
            if (smDt.Rows.Count > 0)
            {
                mamtm = (smDt.Rows[0][0].ToString() == "" || smDt.Rows[0][0].ToString() == null) ? 0 : Convert.ToDecimal(smDt.Rows[0][0]);
            }
            else
            {
                mamtm = 0;
            }
            return mamtm;
        }

        public DataTable GetChargeValValue(string SubType, string mTableKey)
        {
            if (SubType == "NP" || SubType == "PX" || SubType == "SX" || SubType == "NS")
            {
                return GetDataTable(@"Select Amt1 = (Amt1 * (-1)) , Amt2 = (Amt2 * (-1)) ,Amt3 = (Amt3 * (-1)) ,Amt4 = (Amt4 * (-1)) ,Amt5 = (Amt5 * (-1)), Amt6 = (Amt6 * (-1)) ,Amt7 = (Amt7 * (-1)) ,Amt8 = (Amt8 * (-1)) ,Amt9 = (Amt9 * (-1)) ,Amt10 = (Amt10 * (-1))  , Val1 = (Val1 * (-1)) ,Val2 = (Val2 * (-1)) ,Val3 = (Val3 * (-1)) ,Val4 = (Val4 * (-1)) ,Val5 = (Val5 * (-1)),Val6 = (Val6 * (-1)) ,Val7 = (Val7 * (-1)) ,Val8 = (Val8 * (-1)) ,Val9 = (Val9 * (-1)) ,Val10 = (Val10 * (-1)) from " + GetTableName(SubType) + " where TableKey = '" + mTableKey + "'", GetConnectionString());
            }
            else
            {
                return GetDataTable(@"Select Amt1 = Amt1 , Amt2 = Amt2 ,Amt3 = Amt3 ,Amt4 = Amt4 ,Amt5 = Amt5,Amt6 = Amt6,Amt7 = Amt7,Amt8 = Amt8,Amt9 = Amt9,Amt10 = Amt10 ,Val1 = Val1 ,Val2 = Val2 ,Val3 = Val3 ,Val4 = Val4 ,Val5 = Val5,Val6 = Val6 ,Val7 = Val7 ,Val8 = Val8 ,Val9 = Val9 ,Val10 = Val10 from " + GetTableName(SubType) + " where TableKey = '" + mTableKey + "'", GetConnectionString());
            }

        }

        public string GetGSTItemCode(string Code, string Branch)
        {
            string GstCode = "";
            GstCode = ctxTFAT.ItemDetail.Where(x => x.Code == Code && x.Branch == Branch).Select(x => x.PGSTCode).FirstOrDefault();
            if (GstCode == null)
            {
                GstCode = "";
            }
            return GstCode;
        }

        public DataTable GetPurSaleAddonValue(string MainType, string ParentKey)
        {
            string connstring = GetConnectionString();
            string bca = "";
            var loginQuery3 = @"select * from addondoc" + MainType + " where tablekey=" + "'" + ParentKey + "'" + "";
            DataTable mDt2 = GetDataTable(loginQuery3, connstring);

            return mDt2;
        }

        public ActionResult MoveNextPrevious(string Mode, string mdocument)
        {
            string mtype = mdocument.Substring(0, 5);
            string msubtype = ctxTFAT.DocTypes.Where(x => x.Code == mtype).Select(x => x.SubType).FirstOrDefault();
            string mvar;
            if (Mode == "N")
            {
                if (msubtype == "ES" || msubtype == "EP")
                {
                    mvar = Fieldoftable("Enquiry", "Top 1 Tablekey", "Tablekey>'" + mdocument + "' And Type='" + mtype + "' And Prefix='" + mperiod + "' order by Tablekey", "T") ?? "";
                }
                else if (msubtype == "QS" || msubtype == "QP")
                {
                    mvar = Fieldoftable("Quote", "Top 1 Tablekey", "Tablekey>'" + mdocument + "' And Type='" + mtype + "' And Prefix='" + mperiod + "' order by Tablekey", "T") ?? "";
                }
                else if (msubtype == "OS" || msubtype == "OP")
                {
                    mvar = Fieldoftable("Orders", "Top 1 Tablekey", "Tablekey>'" + mdocument + "' And Type='" + mtype + "' And Prefix='" + mperiod + "' order by Tablekey", "T") ?? "";
                }
                else if (msubtype == "PI")
                {
                    mvar = Fieldoftable("pInvoice", "Top 1 Tablekey", "Tablekey>'" + mdocument + "' And Type='" + mtype + "' And Prefix='" + mperiod + "' order by Tablekey", "T") ?? "";
                }
                else if (msubtype == "PK")
                {
                    mvar = Fieldoftable("PackingList", "Top 1 Tablekey", "Tablekey>'" + mdocument + "' And Type='" + mtype + "' And Prefix='" + mperiod + "' order by Tablekey", "T") ?? "";
                }
                else if (msubtype == "DI")
                {
                    mvar = Fieldoftable("DespatchIns", "Top 1 Tablekey", "Tablekey>'" + mdocument + "' And Type='" + mtype + "' And Prefix='" + mperiod + "' order by Tablekey", "T") ?? "";
                }
                else if ((msubtype == "CP") || (msubtype == "IC") || (msubtype == "RP") || (msubtype == "NP") || (msubtype == "IM") || (msubtype == "PX") || (msubtype == "GP"))
                {
                    mvar = Fieldoftable("Purchase", "Top 1 Tablekey", "Tablekey>'" + mdocument + "' And Type='" + mtype + "' And Prefix='" + mperiod + "' order by Tablekey", "T") ?? "";
                }
                else if ((msubtype == "OC") || (msubtype == "RS") || (msubtype == "CS") || msubtype == "XS" || msubtype == "SX" || msubtype == "NS")
                {
                    mvar = Fieldoftable("Sales", "Top 1 Tablekey", "Tablekey>'" + mdocument + "' And Type='" + mtype + "' And Prefix='" + mperiod + "' order by Tablekey", "T") ?? "";
                }
                else
                {
                    mvar = "";
                }
            }
            else
            {
                if (msubtype == "ES" || msubtype == "EP")
                {
                    mvar = Fieldoftable("Enquiry", "Top 1 Tablekey", "Tablekey<'" + mdocument + "' And Type='" + mtype + "' And Prefix='" + mperiod + "' order by Tablekey desc", "T") ?? "";
                }
                else if (msubtype == "QS" || msubtype == "QP")
                {
                    mvar = Fieldoftable("Quote", "Top 1 Tablekey", "Tablekey<'" + mdocument + "' And Type='" + mtype + "' And Prefix='" + mperiod + "' order by Tablekey desc", "T") ?? "";
                }
                else if (msubtype == "OS" || msubtype == "OP")
                {
                    mvar = Fieldoftable("Orders", "Top 1 Tablekey", "Tablekey<'" + mdocument + "' And Type='" + mtype + "' And Prefix='" + mperiod + "' order by Tablekey desc", "T") ?? "";
                }
                else if (msubtype == "PI")
                {
                    mvar = Fieldoftable("pInvoice", "Top 1 Tablekey", "Tablekey<'" + mdocument + "' And Type='" + mtype + "' And Prefix='" + mperiod + "' order by Tablekey desc", "T") ?? "";
                }
                else if (msubtype == "PK")
                {
                    mvar = Fieldoftable("PackingList", "Top 1 Tablekey", "Tablekey<'" + mdocument + "' And Type='" + mtype + "' And Prefix='" + mperiod + "' order by Tablekey desc", "T") ?? "";

                }
                else if (msubtype == "DI")
                {
                    mvar = Fieldoftable("DespatchIns", "Top 1 Tablekey", "Tablekey<'" + mdocument + "' And Type='" + mtype + "' And Prefix='" + mperiod + "' order by Tablekey desc", "T") ?? "";

                }
                else if ((msubtype == "CP") || (msubtype == "IC") || (msubtype == "RP") || (msubtype == "NP") || (msubtype == "IM") || (msubtype == "PX") || (msubtype == "GP"))
                {
                    mvar = Fieldoftable("Purchase", "Top 1 Tablekey", "Tablekey<'" + mdocument + "' And Type='" + mtype + "' And Prefix='" + mperiod + "' order by Tablekey desc", "T") ?? "";

                }
                else if ((msubtype == "OC") || (msubtype == "RS") || (msubtype == "CS") || msubtype == "XS" || msubtype == "SX" || msubtype == "NS")
                {
                    mvar = Fieldoftable("Sales", "Top 1 Tablekey", "Tablekey<'" + mdocument + "' And Type='" + mtype + "' And Prefix='" + mperiod + "' order by Tablekey desc", "T") ?? "";

                }
                else
                {
                    mvar = "";
                }
            }
            if (mvar != "")
            {
                mvar = mbranchcode + mvar;
            }

            return Json(new { Status = "Success", data = mvar }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCreditLimit(string Party)
        {
            LRInvoiceVM Model = new LRInvoiceVM();
            var party = ctxTFAT.MasterInfo.Where(x => x.Code == Party).Select(x => new { x.CrLimit, x.CrPeriod }).FirstOrDefault();
            if (party == null)
            {
                Model.CrLimit = 0;
                Model.CrPeriod = 0;
            }
            else
            {
                Model.CrLimit = party.CrLimit == null ? 0 : party.CrLimit.Value;
                Model.CrPeriod = party.CrPeriod == null ? 0 : party.CrPeriod.Value;
            }
            return Json(Model, JsonRequestBehavior.AllowGet);
        }

        public int GetMaxPOD()
        {
            int mPod = 0;
            mPod = ctxTFAT.PODMaster.Select(x => (int?)x.PODNo).Max() ?? 0;
            mPod = mPod + 1;
            return mPod;
        }

        #region AlertNote Stop CHeck

        public ActionResult CheckStopAlertNote(string Type, List<string> TypeCode, string DocTpe)
        {
            string Status = "Success", Message = "";
            var Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && TypeCode.Contains(AlertMater.TypeCode) && AlertMater.RefType.Contains(DocTpe)
                        orderby AlertMater.DocNo
                        select new AlertNoteVM()
                        {
                            TypeCode = AlertMater.TypeCode,
                            DocReceived = AlertMater.TableKey,
                        }).ToList();
            foreach (var item in Mobj)
            {
                var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                foreach (var stp in Activirty)
                {
                    if (DocTpe.Trim() == stp.Trim())
                    {
                        Status = "Error";
                        Message += item.TypeCode + " Not Allowed In Cash Sale PLease Remove IT....\n";
                        break;
                    }
                }
            }
            var jsonResult = Json(new { Status = Status, Message = Message }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        #endregion


        #region PRINT

        public ActionResult PrintSingleDocumentCrystal(GridOption Model)
        {
            //var PDFName = Model.Document.Substring(19);
            var PDFName = "";
            //var relativePath = "~/Reports/" + Model.Format + ".rpt";
            if (Model.Format == null)
            {
                return Json(new { Status = "filenotfound" }, JsonRequestBehavior.AllowGet);
            }

            if (FileExists("~/Reports/" + Model.Format.Trim() + ".rpt") == false)
            {
                return Json(new { Status = "filenotfound" }, JsonRequestBehavior.AllowGet);
            }
            #region Remove Comment
            var absolutePath = HttpContext.Server.MapPath("~/Reports/" + Model.Format.Trim() + ".rpt");
            if (System.IO.File.Exists(absolutePath) == false)
            {
                return Json(new { Status = "filenotfound" }, JsonRequestBehavior.AllowGet);
            }
            #endregion

            string mParentKey = Model.Document.Substring(6, Model.Document.Length - 6);
            Model.Branch = Model.Document.Substring(0, 6);

            DataTable dtreport = new DataTable();
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("SPDoc_" + Model.StoreProcedure, tfat_conx); //name of the storedprocedure
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@mParentKey", mParentKey);
            cmd.Parameters.AddWithValue("@mBranch", Model.Branch);
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            adp.Fill(dtreport);

            ReportDocument rd = new ReportDocument();
            rd.Load(System.IO.Path.Combine(Server.MapPath("~/Reports"), Model.Format.Trim() + ".rpt"));
            rd.SetDataSource(dtreport);

            try
            {
                Stream mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                mstream.Seek(0, SeekOrigin.Begin);
                rd.Close();
                rd.Dispose();
                if (String.IsNullOrEmpty(PDFName))
                {
                    return File(mstream, "application/PDF");
                }
                else
                {
                    return File(mstream, "application/PDF", PDFName + ".pdf");
                }
                //return File(mstream, "application/PDF");
            }
            catch
            {
                rd.Close();
                rd.Dispose();
                throw;
            }
            finally
            {
                rd.Close();
                rd.Dispose();
            }
        }

        public ActionResult SendMultiReport(GridOption Model)
        {
            //var PDFName = Model.Document.Substring(19);
            var PDFName = "";
            if (Model.Format == null)
            {
                return null;
            }

            string mParentKey = "";
            Document document = new Document(PageSize.A4, 10, 10, 10, 10);
            var ms = new MemoryStream();
            {
                PdfCopy pdf = new PdfCopy(document, ms);
                document.Open();
                pdf.Open();

                List<string> mformats = Model.Format.Split(',').ToList();
                foreach (var mformat in mformats)
                {
                    mParentKey = Model.Document.Substring(6, Model.Document.Length - 6);
                    Model.Branch = Model.Document.Substring(0, 6);
                    Model.StoreProcedure = ctxTFAT.DocFormats.Where(x => x.FormatCode.Trim() == mformat.Trim()).Select(x => x.StoredProc).FirstOrDefault();
                    DataTable dtreport = new DataTable();
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("SPDoc_" + Model.StoreProcedure, tfat_conx); //name of the storedprocedure
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@mParentKey", mParentKey);
                    cmd.Parameters.AddWithValue("@mBranch", Model.Branch);
                    SqlDataAdapter adp = new SqlDataAdapter(cmd);
                    adp.Fill(dtreport);

                    ReportDocument rd = new ReportDocument();
                    rd.Load(System.IO.Path.Combine(Server.MapPath("~/Reports"), mformat.Trim() + ".rpt"));
                    rd.SetDataSource(dtreport);
                    try
                    {
                        Stream mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        mstream.Seek(0, SeekOrigin.Begin);
                        Warning[] warnings;
                        string[] streamids;
                        string mimeType;
                        string encoding;
                        string extension;
                        MemoryStream memory1 = new MemoryStream();
                        mstream.CopyTo(memory1);
                        byte[] bytes = memory1.ToArray();
                        MemoryStream memoryStream = new MemoryStream(bytes);
                        PdfReader imageDocumentReader = new PdfReader(memoryStream.ToArray());
                        int ab = imageDocumentReader.NumberOfPages;
                        for (int a = 1; a <= ab; a++)
                        {
                            var page = pdf.GetImportedPage(imageDocumentReader, a);
                            pdf.AddPage(page);
                        }
                        imageDocumentReader.Close();
                    }
                    catch
                    {
                        rd.Close();
                        rd.Dispose();
                        throw;
                    }
                    finally
                    {
                        rd.Close();
                        rd.Dispose();
                    }

                }
            }
            document.Close();

            if (String.IsNullOrEmpty(PDFName))
            {
                return File(ms.ToArray(), "application/PDF");
            }
            else
            {
                return File(ms.ToArray(), "application/PDF", PDFName + ".pdf");
            }
            //return File(ms.ToArray(), "application/PDF");

        }

        #endregion
    }
}