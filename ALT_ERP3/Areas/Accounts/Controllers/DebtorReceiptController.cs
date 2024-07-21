using ALT_ERP3.Controllers;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ALT_ERP3.Areas.Accounts.Models;
using System.Data.SqlClient;
using System.Data.Entity;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WebForms;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class DebtorReceiptController : BaseController
    {
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
        private static string mauthorise = "A00";

        // GET: Accounts/DebtorReceipt
        public ActionResult Index(DebtorReceiptVM Model)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            DebtorReceiptSetup debtorReceiptSetup = ctxTFAT.DebtorReceiptSetup.FirstOrDefault();
            if (debtorReceiptSetup == null)
            {
                debtorReceiptSetup = new DebtorReceiptSetup();
            }
            Model.TDSBillWiseCut = debtorReceiptSetup.BillwiseCutTds;
            Model.RoundOff = 2;
            if (debtorReceiptSetup.CurrDatetOnlyreq == false && debtorReceiptSetup.BackDateAllow == false && debtorReceiptSetup.ForwardDateAllow == false)
            {
                Model.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
                Model.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
            }
            if (debtorReceiptSetup.CurrDatetOnlyreq == true)
            {
                Model.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
                Model.EndDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            if (debtorReceiptSetup.BackDateAllow == true)
            {
                Model.StartDate = (DateTime.Now.AddDays(-debtorReceiptSetup.BackDaysUpto)).ToString("yyyy-MM-dd");
            }
            if (debtorReceiptSetup.ForwardDateAllow == true)
            {
                Model.EndDate = (DateTime.Now.AddDays(debtorReceiptSetup.ForwardDaysUpto)).ToString("yyyy-MM-dd");
            }


            if (Model.Mode == "Add")
            {
                var PreviousDate = Convert.ToString(TempData.Peek("BRCPreviousDate"));
                if (String.IsNullOrEmpty(PreviousDate))
                {
                    Model.DocDate = DateTime.Now;
                }
                else
                {
                    Model.DocDate = ConvertDDMMYYTOYYMMDD(PreviousDate);
                }
                Session["Ddebtorlist"] = null;
                Model.MainType = "RC";
                Model.SubType = "BR";
                Model.Type = "BRC00";
                Model.Prefix = mperiod;
                Model.Branch = mbranchcode;
                Model.HeaderList = ctxTFAT.Charges.Where(x => x.Type == "BRC00" && x.DontUse == false).Select(x => x.Head).ToList();
                UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "NA");
            }
            else
            {
                Model.MainType = "RC";
                Model.SubType = "BR";
                Model.Type = "BRC00";

                Model.Branch = Model.Document.Substring(0, 6);
                Model.ParentKey = Model.Document.Substring(6);
                //Model.Type = Model.ParentKey.Substring(0, 5);

                UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, Model.ParentKey, DateTime.Now, 0, "", "", "NA");


                var mVouchMaster = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).OrderBy(x => x.Sno).FirstOrDefault();
                var mVoucherDetail = ctxTFAT.DebtorRecVouDetail.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).Select(x => x).ToList();
                Model.DocDate = mVouchMaster.DocDate;
                Model.Bank = mVouchMaster.AltCode;
                Model.BankName = ctxTFAT.Master.Where(x => x.Code == mVouchMaster.AltCode).Select(x => x.Name).FirstOrDefault();
                Model.ChequeNo = mVouchMaster.Cheque;
                Model.Remark = mVouchMaster.Narr;
                Model.Account = mVouchMaster.Code;

                Model.PeriodLock = PeriodLock(Model.Branch, Model.Type, mVouchMaster.DocDate);
                if (mVouchMaster.AUTHORISE.Substring(0, 1) == "A")
                {
                    Model.LockAuthorise = LockAuthorise(mVouchMaster.Type, Model.Mode, mVouchMaster.TableKey, mVouchMaster.ParentKey);
                }

                var mVouchMaster1 = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch && x.Code == Model.Bank && x.AltCode == Model.Account).OrderBy(x => x.Sno).FirstOrDefault();
                Model.AccAmt = mVouchMaster1 == null ? 0 : mVouchMaster1.Debit.Value + mVouchMaster1.Credit.Value;

                Model.AccountName = ctxTFAT.Master.Where(x => x.Code == mVouchMaster.Code).Select(x => x.Name).FirstOrDefault();
                Model.Srl = mVouchMaster.Srl;
                Model.Prefix = mVouchMaster.Prefix;
                var Branch = "";
                var master = ctxTFAT.Master.Where(x => x.Code == Model.Account).FirstOrDefault();
                if (master.FetchBalAcc == false)
                {
                    Branch = mbranchcode;
                }
                string mStr = @"select dbo.GetBalance('" + Model.Account + "','" + MMDDYY(DateTime.Now) + "','"+Branch+"',0,0)";
                DataTable smDt = GetDataTable(mStr);
                double mBalance = 0;
                if (smDt.Rows.Count > 0)
                {
                    mBalance = Convert.ToDouble(smDt.Rows[0][0].ToString());
                }
                Model.CustBalance = mBalance + (mBalance > 0 ? " Dr" : " Cr");
                Branch = "";
                master = ctxTFAT.Master.Where(x => x.Code == Model.Bank).FirstOrDefault();
                if (master.FetchBalAcc == false)
                {
                    Branch = mbranchcode;
                }
                mStr = @"select dbo.GetBalance('" + Model.Bank + "','" + MMDDYY(DateTime.Now) + "','"+Branch+"',0,0)";
                smDt = new DataTable();
                smDt = GetDataTable(mStr);
                mBalance = 0;
                if (smDt.Rows.Count > 0)
                {
                    mBalance = Convert.ToDouble(smDt.Rows[0][0].ToString());
                }
                Model.CashBalance = mBalance + (mBalance > 0 ? " Dr" : " Cr");

                List<DebtorReceiptVM> SelectedLedger = new List<DebtorReceiptVM>();
                var objledgerdetail = ctxTFAT.DebtorRecVouDetail.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).Select(x => x).ToList();
                decimal AddAmtToAcc = 0;
                foreach (var item in objledgerdetail)
                {
                    var legerleger = ctxTFAT.Ledger.Where(x => x.TableKey == item.InvTableKey && x.Branch == item.InvBranch).OrderBy(x => x.Sno).Select(x => x).FirstOrDefault();

                    var mchargelist = GetFMWiseCharges(item.TableKey, item.Branch);
                    SelectedLedger.Add(new DebtorReceiptVM()
                    {
                        tempid = SelectedLedger.Count() + 1,
                        BillNumber = legerleger.BillNumber,
                        StrBillDate = legerleger.BillDate.Value.ToString("dd-MM-yyyy"),
                        BillDate = legerleger.BillDate.Value,
                        BranchName = ctxTFAT.TfatBranch.Where(x => x.Code == legerleger.Branch).Select(x => x.Name).FirstOrDefault(),
                        Branch = item.InvBranch,
                        BillAmt = legerleger.Debit.Value,
                        BalAmt = legerleger.Debit.Value,
                        //BalAmt = item.Amount - legerleger.Debit.Value,
                        AdjustAmt = item.Amount,
                        Amt = item.Amount,
                        LedgerParentKey = legerleger.ParentKey,
                        LedgerTableKey = item.InvTableKey,
                        LedgerPrefix = legerleger.Prefix,
                        LedgerType = legerleger.Type,
                        LedgerSubType = legerleger.SubType,
                        LedgerMainType = legerleger.MainType,
                        LedgerSrl = legerleger.Srl,
                        LedgerSno = legerleger.Sno,
                        ChargesList = mchargelist,
                        HeaderList = mchargelist.Select(x => x.Header).ToList(),
                        ChgPickupList = mchargelist.Select(x => x.Amt).ToList(),
                        Remark = item.Narr,
                        OptionType = "1",
                        NetAmt = item.NetAmt,
                        BillTDS = item.TdsAmout,
                        AutoAdjInterface = item.AutoAdjInterface
                    });

                }
                Model.SelectedLedger = SelectedLedger;
                Model.NetAmt = SelectedLedger.Sum(x => x.NetAmt);
                Model.HeaderList = ctxTFAT.Charges.Where(x => x.Type == "BRC00" && x.DontUse == false).Select(x => x.Head).ToList();
                Model.TotalBillTds = SelectedLedger.Sum(x => x.BillTDS);


                decimal[] mCharges2 = new decimal[(Model.HeaderList.Count)];

                for (int ai = 0; ai < Model.HeaderList.Count; ai++)
                {
                    decimal mchgamt = 0;
                    foreach (var i in SelectedLedger)
                    {
                        var ChgPickupList = i.ChargesList.Select(x => x.Amt).ToList();
                        mchgamt += ChgPickupList[ai];
                    }
                    mCharges2[ai] = mchgamt;
                }
                Model.TotalChgPickupList = mCharges2.ToList();
                Session["Ddebtorlist"] = SelectedLedger;

                #region TDS Calculate

                var Ledger = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).FirstOrDefault();
                Model.TDSCode = Ledger.TDSCode == null ? "" : Ledger.TDSCode.Value.ToString();
                Model.TDSFlag = Ledger.TDSFlag;
                Model.TDSCodeName = ctxTFAT.TDSMaster.Where(x => x.Code.ToString() == Model.TDSCode).Select(x => x.Name).FirstOrDefault();
                Model.Taxable = (decimal)Model.AccAmt;
                Model.InvoiceAmt = (double)Model.AccAmt;
                if (Model.TDSFlag)
                {
                    var TdsAC = Ledger.TDSCode.ToString();
                    TDSMaster tDSMaster = ctxTFAT.TDSMaster.Where(x => x.Code == Ledger.TDSCode).FirstOrDefault();
                    if (tDSMaster != null)
                    {
                        var Ledger1 = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch && x.Code.ToString() == tDSMaster.PostCode.ToString()).FirstOrDefault();
                        if (Ledger1 != null)
                        {
                            Model.TDSAmt = Ledger1.Debit.Value + Ledger1.Credit.Value;

                        }
                    }

                    var GetTdsFld = ctxTFAT.Charges.Where(x => x.Type == "BRC00" && x.Code == TdsAC).Select(x => x.Fld).FirstOrDefault();

                    //if (!String.IsNullOrEmpty(GetTdsFld))
                    //{
                    //    foreach (var item in SelectedLedger)
                    //    {
                    //        Model.TDSAmt += item.ChargesList.Where(x => x.Fld == GetTdsFld).Sum(x => x.Amt) >= 0 ? item.ChargesList.Where(x => x.Fld == GetTdsFld).Sum(x => x.Amt) : (-1) * item.ChargesList.Where(x => x.Fld == GetTdsFld).Sum(x => x.Amt);
                    //    }
                    //}
                    //if (Model.Taxable > 0)
                    //{
                    //    Model.TDSRate = Math.Round(Math.Abs(((decimal)Model.TDSAmt * 100) / Model.Taxable));
                    //}

                }
                #endregion
            }

            List<GridOption> Grlist = new List<GridOption>();
            var list = ctxTFAT.DocFormats.Where(x => x.Type == "BRC00").Select(x => x).ToList();
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
        #region ALL GET 

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

        public ActionResult GetTDSRateDetail(DebtorReceiptVM Model)
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

        public ActionResult GetLoanTrxTypes()
        {

            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "0", Text = "Payment" });
            GSt.Add(new SelectListItem { Value = "1", Text = "Receipt" });

            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBankList(string term)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.Master.Where(x => (x.BaseGr == "B" || x.BaseGr == "C") && x.Hide == false).Select(c => new { c.Code, c.Name }).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var result = ctxTFAT.Master.Where(x => x.Name.Contains(term) && (x.BaseGr == "B" || x.BaseGr == "C") && x.Hide == false).Select(m => new { m.Code, m.Name }).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetFMList(string AccountCode, string AdvType, string FMNo, string ParentKey)
        {

            List<SelectListItem> branch = new List<SelectListItem>();
            //var mOtherType = ctxTFAT.Master.Where(x => x.Code == AccountCode).Select(x => x.OthPostType).FirstOrDefault();
            //if (mOtherType == "D")
            //{
            string mstr = "Select r.Srl as FMNo from Ledger r  where r.Type = 'FM000' and r.Code = '" + AccountCode + "' and (r.Refdoc = 'A' or r.RefDoc = 'B')   and ((r.credit - abs(isnull((Select sum(VoucherDetail.Amount) from VoucherDetail Where VoucherDetail.VouNo = r.Srl and VoucherDetail.ParentKey <> '" + ParentKey + "'),0))) > 0)  Order by r.RecordKey";
            List<DataRow> ordersstk = GetDataTable(mstr).AsEnumerable().ToList();

            foreach (var item in ordersstk)
            {
                branch.Add(new SelectListItem { Text = item["FMNo"].ToString(), Value = item["FMNo"].ToString() });
            }
            //}
            //else
            //{
            //    string mstr = "Select r.Srl as FMNo from Ledger r  where r.Type = 'FM000' and r.Code = '" + AccountCode + "' and (r.Refdoc = 'A' or r.RefDoc = 'B')   and ((r.credit - abs(isnull((Select sum(VoucherDetail.Amount) from VoucherDetail Where VoucherDetail.VouNo = r.Srl and VoucherDetail.ParentKey <> '" + ParentKey + "'),0))) > 0)  Order by r.RecordKey";
            //    List<DataRow> ordersstk = GetDataTable(mstr).AsEnumerable().ToList();

            //    foreach (var item in ordersstk)
            //    {
            //        branch.Add(new SelectListItem { Text = item["FMNo"].ToString(), Value = item["FMNo"].ToString() });
            //    }
            //}

            return Json(branch, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetBranchList()
        {
            List<SelectListItem> branch = new List<SelectListItem>();
            var warehouselist = ctxTFAT.TfatBranch.Select(b => new
            {
                b.Code,
                b.Name
            }).ToList();
            foreach (var item in warehouselist)
            {
                branch.Add(new SelectListItem { Text = item.Name, Value = item.Code });
            }
            return Json(branch, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetAccountList(string term)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.Master.Where(x => x.BaseGr == "D").Select(c => new { c.Code, c.Name }).Distinct().ToList().Take(10);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var result = ctxTFAT.Master.Where(x => x.Name.Contains(term) && x.BaseGr == "D").Select(m => new { m.Code, m.Name }).Distinct().ToList().Take(10);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetAdvanceTypeList()
        {

            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "0", Text = "Payment" });
            GSt.Add(new SelectListItem { Value = "1", Text = "Receipt" });

            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAccountDetails(DebtorReceiptVM Model)
        {
            Session["Ddebtorlist"] = null;

            var CurrDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            var Branch = "";
            var master = ctxTFAT.Master.Where(x => x.Code == Model.Account).FirstOrDefault();
            if (master.FetchBalAcc == false)
            {
                Branch = mbranchcode;
            }
            string mStr = @"select dbo.GetBalance('" + Model.Account + "','" + MMDDYY(DateTime.Now) + "','"+Branch+"',0,0)";
            DataTable smDt = GetDataTable(mStr);
            double mBalance = 0;
            if (smDt.Rows.Count > 0)
            {
                mBalance = Convert.ToDouble(smDt.Rows[0][0].ToString());
            }
            var taxdetails = ctxTFAT.TaxDetails.Where(x => x.Code == Model.Account).Select(x => new { x.TDSCode, x.CutTDS, x.TDSRate }).FirstOrDefault();
            var CutTDS = taxdetails == null ? false : taxdetails.CutTDS;
            var TDSCode = taxdetails == null ? 0 : taxdetails.TDSCode == null ? 0 : taxdetails.TDSCode.Value;
            //var TDSRate = taxdetails == null ? 0 : taxdetails.TDSRate == null ? 0 : taxdetails.TDSRate.Value;
            decimal TDSRate = 0;
            TDSRate = ctxTFAT.TDSRates.Where(x => x.Code == TDSCode && x.EffDate <= CurrDate).OrderByDescending(x => x.EffDate).Select(x => x.TDSRate ?? 0).FirstOrDefault();
            Model.TDSRate = TDSRate;
            var TDSName = ctxTFAT.TDSMaster.Where(x => x.Code == TDSCode).Select(x => x.Name).FirstOrDefault();
            var TDSFlagSetup = ctxTFAT.DebtorReceiptSetup.Select(x => x.CutTDS).FirstOrDefault();
            if (TDSFlagSetup == false)
            {
                CutTDS = false;
            }
            

            return Json(new
            {
                CutTDS = CutTDS,
                TDSCode = TDSCode,
                TDSRate = TDSRate,
                TDSName = TDSName,
                CustBalance = mBalance + (mBalance > 0 ? " Dr" : " Cr"),


            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBankDetails(DebtorReceiptVM Model)
        {
            var Branch = "";
            var master = ctxTFAT.Master.Where(x => x.Code == Model.Bank).FirstOrDefault();
            if (master.FetchBalAcc == false)
            {
                Branch = mbranchcode;
            }
            string mStr = @"select dbo.GetBalance('" + Model.Bank + "','" + MMDDYY(DateTime.Now) + "','"+Branch+"',0,0)";
            DataTable smDt = GetDataTable(mStr);
            double mBalance = 0;
            if (smDt.Rows.Count > 0)
            {
                mBalance = Convert.ToDouble(smDt.Rows[0][0].ToString());
            }

            return Json(new
            {
                CashBalance = mBalance + (mBalance > 0 ? " Dr" : " Cr"),


            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region ADD LEDGER ITEM

        public ActionResult AddEditSelectedAdvance(DebtorReceiptVM Model)
        {
            if (Model.SessionFlag == "Add")
            {
                List<DebtorReceiptVM> objledgerdetail = new List<DebtorReceiptVM>();

                if (Session["Ddebtorlist"] != null)
                {
                    objledgerdetail = (List<DebtorReceiptVM>)Session["Ddebtorlist"];
                }

                List<DebtorReceiptVM> mCharges = new List<DebtorReceiptVM>();
                foreach (var chg in Model.ChargesList)
                {
                    mCharges.Add(new DebtorReceiptVM()
                    {
                        Amt = (chg.AddLess == "+") ? chg.Amt : -chg.Amt
                    });
                }

                objledgerdetail.Add(new DebtorReceiptVM()
                {
                    BillNumber = Model.BillNumber,
                    StrBillDate = Model.StrBillDate,
                    BillDate = ConvertDDMMYYTOYYMMDD(Model.StrBillDate),
                    BranchName = Model.BranchName,
                    Branch = Model.Branch,
                    BillAmt = Model.BillAmt,
                    BalAmt = Model.BalAmt,
                    AdjustAmt = Model.AdjustAmt,
                    Amt = Model.Amt,
                    LedgerParentKey = Model.LedgerParentKey,
                    LedgerTableKey = Model.LedgerTableKey,
                    ChargesList = Model.ChargesList,
                    HeaderList = Model.ChargesList.Select(x => x.Header).ToList(),
                    ChgPickupList = Model.ChargesList.Select(x => x.Amt).ToList(),
                    Remark = Model.Remark,
                    OptionType = "1",
                    BillTDS = Model.BillTDS,
                    NetAmt = Model.Amt + ((mCharges != null && mCharges.Count > 0) ? mCharges.Sum(x => (decimal?)x.Amt) ?? 0 : 0),

                });


                Session.Add("Ddebtorlist", objledgerdetail);
                decimal sumAmt = objledgerdetail.Sum(x => x.NetAmt);

                Model.HeaderList = ctxTFAT.Charges.Where(x => x.Type == "BRC00" && x.DontUse == false).Select(x => x.Head).ToList();

                Model.TotalTaxable = objledgerdetail.Sum(x => x.Amt);
                decimal[] mCharges2 = new decimal[(Model.HeaderList.Count)];

                for (int ai = 0; ai < Model.HeaderList.Count; ai++)
                {
                    decimal mchgamt = 0;
                    foreach (var i in objledgerdetail)
                    {
                        mchgamt += i.ChgPickupList[ai];

                    }
                    mCharges2[ai] = mchgamt;
                }
                Model.TotalChgPickupList = mCharges2.ToList();

                var html = ViewHelper.RenderPartialView(this, "LedgerList", new DebtorReceiptVM() { SelectedLedger = objledgerdetail, HeaderList = Model.HeaderList, TotalTaxable = Model.TotalTaxable, TotalChgPickupList = Model.TotalChgPickupList });
                return Json(new { Html = html, SumAmt = sumAmt }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                decimal sumAmt = 0;
                List<DebtorReceiptVM> mCharges = new List<DebtorReceiptVM>();
                foreach (var chg in Model.ChargesList)
                {
                    mCharges.Add(new DebtorReceiptVM()
                    {
                        Amt = (chg.AddLess == "-") ? -chg.Amt : chg.Amt,
                        AddLess = chg.AddLess
                    });
                }

                var result = (List<DebtorReceiptVM>)Session["Ddebtorlist"];
                foreach (var item in result.Where(x => x.tempid == Model.tempid))
                {
                    item.BillNumber = Model.BillNumber;
                    item.StrBillDate = item.BillDate.ToShortDateString();
                    item.BillDate = (item.BillDate);
                    item.BranchName = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault();
                    item.Branch = item.Branch;
                    item.BillAmt = item.BillAmt;
                    item.BalAmt = item.BalAmt;
                    item.AdjustAmt = Model.AdjustAmt;
                    item.Amt = Model.Amt;
                    item.ChargesList = Model.ChargesList;
                    item.HeaderList = Model.ChargesList.Select(x => x.Header).ToList();
                    item.ChgPickupList = Model.ChargesList.Select(x => x.Amt).ToList();
                    item.Remark = Model.Remark;
                    item.BillTDS = Model.BillTDS;
                    item.NetAmt = Model.Amt + ((mCharges != null && mCharges.Count > 0) ? mCharges.Where(x => x.AddLess.Trim() != "-").Sum(x => (decimal?)x.Amt) ?? 0 : 0);

                }


                Session.Add("Ddebtorlist", result);
                sumAmt = result.Sum(x => x.NetAmt);





                Model.HeaderList = ctxTFAT.Charges.Where(x => x.Type == "BRC00" && x.DontUse == false).Select(x => x.Head).ToList();

                Model.TotalTaxable = result.Sum(x => x.Amt);
                Model.Taxable = result.Sum(x => x.Amt);
                Model.TotalBillTds = result.Sum(x => x.BillTDS);
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


                #region TDS Calculate
                if (Model.TDSFlag)
                {
                    var TdsAC = "000009992";
                    Model.TDSAmt = 0;
                    var GetTdsFld = ctxTFAT.Charges.Where(x => x.Type == "BRC00" && x.Code == TdsAC).Select(x => x.Fld).FirstOrDefault();

                    if (!String.IsNullOrEmpty(GetTdsFld))
                    {
                        foreach (var item in result)
                        {
                            Model.TDSAmt += item.ChargesList.Where(x => x.Fld == GetTdsFld).Sum(x => x.Amt) >= 0 ? item.ChargesList.Where(x => x.Fld == GetTdsFld).Sum(x => x.Amt) : (-1) * item.ChargesList.Where(x => x.Fld == GetTdsFld).Sum(x => x.Amt);
                        }
                    }
                }

                #endregion

                Model.TDSAmt = Model.TotalBillTds;

                var html = ViewHelper.RenderPartialView(this, "LedgerList", new DebtorReceiptVM() { SelectedLedger = result, Taxable = Model.Taxable, TotalBillTds = Model.TotalBillTds, HeaderList = Model.HeaderList, TotalTaxable = Model.TotalTaxable, TotalChgPickupList = Model.TotalChgPickupList });
                return Json(new
                {
                    Selectedleger = result,
                    Html = html,
                    SumAmt = sumAmt,
                    TDSAmt = Model.TDSAmt,
                }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetAdvanceLedger(DebtorReceiptVM Model)
        {
            if (Model.SessionFlag == "Edit")
            {
                var result = (List<DebtorReceiptVM>)Session["Ddebtorlist"];
                var result1 = result.Where(x => x.tempid == Model.tempid).ToList();
                foreach (var item in result1)
                {

                    Model.Branch = item.Branch;
                    Model.StrBillDate = item.StrBillDate;
                    Model.BillDate = ConvertDDMMYYTOYYMMDD(item.StrBillDate);
                    Model.BranchName = item.BranchName;
                    Model.BillAmt = item.BillAmt;
                    Model.BalAmt = item.BalAmt;
                    Model.AdjustAmt = item.AdjustAmt;
                    Model.Amt = item.Amt;
                    Model.NetAmt = item.Amt;
                    Model.Remark = item.Remark;
                    Model.LedgerPrefix = item.LedgerPrefix;
                    Model.LedgerSrl = item.LedgerSrl;
                    Model.LedgerMainType = item.LedgerMainType;
                    Model.LedgerParentKey = item.LedgerParentKey;
                    Model.LedgerTableKey = item.LedgerTableKey;
                    Model.LedgerSubType = item.LedgerSubType;
                    Model.LedgerType = item.LedgerType;
                    Model.BillNumber = item.BillNumber;
                    Model.BillTDS = item.BillTDS;
                    List<DebtorReceiptVM> newlist1 = new List<DebtorReceiptVM>();
                    var mCharges = (from C in ctxTFAT.Charges
                                    where C.Type == "BRC00" && C.DontUse == false

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
                        DebtorReceiptVM c = new DebtorReceiptVM();
                        c.Fld = i.Fld;
                        c.Header = i.Head;
                        c.AddLess = i.EqAmt;
                        c.tempid = Convert.ToInt16(i.Fld.Substring(1, 3));
                        c.PostCode = i.Code;
                        c.Amt = item.ChargesList.Where(x => x.Fld == i.Fld).Select(x => x.Amt).FirstOrDefault();
                        newlist1.Add(c);

                        a = a + 1;
                    }

                    Model.ChargesList = newlist1;

                    decimal TotalAdjustAmt = 0;
                    foreach (var item1 in result)
                    {
                        TotalAdjustAmt += item1.Amt;
                        TotalAdjustAmt += item1.ChargesList.Where(x => x.AddLess.Trim() != "-" && x.AddLess.Trim() != "+").Sum(x => x.Amt);
                    }

                    Model.UnAdjustedAmt = Model.AccAmt - (TotalAdjustAmt);
                    Model.AdjustAmt = Model.BalAmt - (Model.BillTDS) - (newlist1.Where(x => x.AddLess.Trim() != "").Sum(x => x.Amt) + Model.Amt);
                    Model.NetAmt += newlist1.Where(x => x.AddLess.Trim() == "-").Sum(x => x.Amt);
                }
                var jsonResult = Json(new { Html = this.RenderPartialView("AddAdvancePayPopup", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            else
            {
                List<DebtorReceiptVM> newlist1 = new List<DebtorReceiptVM>();

                var CostCentre = (from C in ctxTFAT.Charges
                                  where C.Type == "BRC00" && C.DontUse == false

                                  select new
                                  {
                                      C.Fld,
                                      C.Head,
                                      C.EqAmt,
                                      C.Equation,
                                      C.Code
                                  }).ToList();
                int a = 1;
                foreach (var i in CostCentre)
                {
                    DebtorReceiptVM c = new DebtorReceiptVM();
                    c.Fld = i.Fld;
                    c.Header = i.Head;
                    c.AddLess = i.EqAmt;
                    c.tempid = Convert.ToInt16(i.Fld.Substring(1, 3));
                    c.PostCode = i.Code;
                    newlist1.Add(c);

                    a = a + 1;
                }

                Model.ChargesList = newlist1;
                Model.FMDate = DateTime.Now.Date;
                var jsonResult = Json(new { Html = this.RenderPartialView("AddAdvancePayPopup", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        [HttpPost]
        public ActionResult DeleteAdvanceLedger(DebtorReceiptVM Model)
        {
            var result2 = (List<DebtorReceiptVM>)Session["Ddebtorlist"];
            if (result2==null)
            {
                result2 = new List<DebtorReceiptVM>();
            }
            var result = result2.Where(x => x.tempid != Model.tempid).ToList();

            decimal sumAmt = result.Sum(x => x.NetAmt);
            Model.HeaderList = ctxTFAT.Charges.Where(x => x.Type == "BRC00" && x.DontUse == false).Select(x => x.Head).ToList();

            Model.TotalTaxable = result.Sum(x => x.Amt);
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


            #region TDS Calculate
            if (Model.TDSFlag)
            {
                var TdsAC = "000009992";
                Model.TDSAmt = 0;
                var GetTdsFld = ctxTFAT.Charges.Where(x => x.Type == "BRC00" && x.Code == TdsAC).Select(x => x.Fld).FirstOrDefault();

                if (!String.IsNullOrEmpty(GetTdsFld))
                {
                    foreach (var item in result)
                    {
                        Model.TDSAmt += item.ChargesList.Where(x => x.Fld == GetTdsFld).Sum(x => x.Amt) >= 0 ? item.ChargesList.Where(x => x.Fld == GetTdsFld).Sum(x => x.Amt) : (-1) * item.ChargesList.Where(x => x.Fld == GetTdsFld).Sum(x => x.Amt);
                    }
                }
            }
            #endregion

            Session["Ddebtorlist"] = result;
            var html = ViewHelper.RenderPartialView(this, "LedgerList", new DebtorReceiptVM() { SelectedLedger = result, HeaderList = Model.HeaderList, TotalTaxable = Model.TotalTaxable, TotalChgPickupList = Model.TotalChgPickupList });
            return Json(new
            {
                Html = html,
                SumAmt = sumAmt,
                TDSAmt = Model.TDSAmt,

            }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetFMDetailsFromPickup(DebtorReceiptVM Model)
        {
            try
            {

                Model.Branch = ctxTFAT.TfatBranch.Where(X => X.Name == Model.Branch).Select(x => x.Code).FirstOrDefault();
                Model.FMNo = Model.FMNo;
                Model.FMDate = ConvertDDMMYYTOYYMMDD(Model.FMDateStr);
                Model.FMDateStr = Model.FMDateStr;
                Model.AdvType = Model.AdvType == "Advance" ? "A" : "B";
                if (Model.AdvType == "A")
                {
                    Model.AdvPending = Model.AdvPending;
                    Model.AdvAmt = Model.AdvAmt;
                }
                else
                {
                    Model.BalPending = Model.AdvPending;
                    Model.BalAmt = Model.AdvAmt;
                }
                Model.RefTableKey = Model.RefTableKey;
                Model.Amt = Model.Amt;
                Model.Remark = Model.Remark;
                Model.Party = ctxTFAT.Master.Where(X => X.Code == Model.PartyCode).Select(X => X.Name).FirstOrDefault();
                Model.RelatedTo = ctxTFAT.Master.Where(X => X.Code == Model.RelatedToCode).Select(X => X.Name).FirstOrDefault();
                List<DebtorReceiptVM> newlist1 = new List<DebtorReceiptVM>();
                var mCharges = (from C in ctxTFAT.Charges
                                where C.Type == "FM" && C.DontUse == false

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
                    DebtorReceiptVM c = new DebtorReceiptVM();
                    c.Fld = i.Fld;
                    c.Header = i.Head;
                    c.AddLess = i.EqAmt;
                    c.tempid = Convert.ToInt16(i.Fld.Substring(1, 3));
                    c.PostCode = i.Code;
                    c.Amt = Model.ChgPickupList[b];
                    newlist1.Add(c);

                    a = a + 1;
                    b = b + 1;
                }

                Model.ChargesList = newlist1;
                var jsonResult = Json(new { Html = this.RenderPartialView("AddAdvancePayPopup", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            catch (Exception ex)
            {
                var jsonResult = Json(new { Status = "Error" }, JsonRequestBehavior.AllowGet);
                return jsonResult;
            }

        }
        #endregion


        #region Pickup
        public List<DebtorReceiptVM> Add2ItemForPickup(List<DataRow> ordersstk, string query)
        {
            double PendFactor = 0;
            List<DebtorReceiptVM> objitemlist = new List<DebtorReceiptVM>();
            int i = 1;
            foreach (var item in ordersstk)
            {

                objitemlist.Add(new DebtorReceiptVM()
                {
                    tempid = i,
                    Type = item["Type"].ToString(),
                    Prefix = item["Prefix"].ToString(),
                    Srl = item["Srl"].ToString(),
                    Sno = Convert.ToInt32(item["Sno"].ToString()),
                    BillNumber = item["BillNumber"].ToString(),
                    StrBillDate = item["BillDate"].ToString(),
                    Branch = item["Branch"].ToString(),
                    BranchName = item["BranchName"].ToString(),
                    BillAmt = Convert.ToDecimal(item["BillAmt"].ToString()),
                    BalAmt = Convert.ToDecimal(item["BalAmt"].ToString()),

                    Party = item["Party"].ToString(),
                    ParentKey = item["ParentKey"].ToString(),
                    TableKey = item["TableKey"].ToString(),
                    MainType = item["MainType"].ToString(),
                    SubType = item["SubType"].ToString(),
                    ChgPickupList = GetAdvChargesPickup(query, item["TableKey"].ToString()),

                });
                i = i + 1;
            }

            return objitemlist;
        }

        public ActionResult GetPickUp(DebtorReceiptVM Model)
        {
            string mstr = "";
            string abc = "";

            var result = (List<DebtorReceiptVM>)Session["Ddebtorlist"];
            decimal TotalAdjustAmt = 0;
            string ExistBill = "";
            if (result != null)
            {
                foreach (var item1 in result)
                {
                    TotalAdjustAmt += item1.Amt;
                    TotalAdjustAmt += item1.ChargesList.Where(x => x.AddLess.Trim() != "-" && x.AddLess.Trim() != "+").Sum(x => x.Amt);
                    ExistBill += "'" + item1.LedgerTableKey + "',";
                }
            }
            if (!String.IsNullOrEmpty(ExistBill))
            {
                ExistBill = ExistBill.Substring(0, ExistBill.Length - 1);
            }
            else
            {
                ExistBill = "'" + "'";
            }
            Model.AccAmt = Model.AccAmt - TotalAdjustAmt;
            Model.UnAdjustedAmt = Model.AccAmt;


            var mOtherPostType = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.OthPostType).FirstOrDefault();
            Model.OthPostType = mOtherPostType;
            var charges = ctxTFAT.Charges.Where(x => x.Type == "BRC00" && x.DontUse == false).Select(C => new
            {
                C.Fld,
                C.Head,
                C.EqAmt,
                C.Equation,
                C.Code,

            }).ToList();

            var mLrListSess = (List<DebtorReceiptVM>)Session["Ddebtorlist"];

            if (charges != null && charges.Count > 0)
            {
                foreach (var a in charges)
                {
                    int tempid = Convert.ToInt16(a.Fld.Substring(1, 3));
                    abc = abc + "isnull(r.Val" + tempid + ",0) as FLD" + tempid.ToString("D2") + ",";

                }
            }

            abc = (abc != "") ? abc.Remove(abc.Length - 1, 1) : abc;
            string mBranchFilter = "";
            if (string.IsNullOrEmpty(Model.FilBranch) == true)
            {
                mBranchFilter = " and l.Branch = '" + mbranchcode + "'";
            }
            else if (Model.FilBranch == "Current")
            {
                mBranchFilter = " and l.Branch = '" + mbranchcode + "'";
            }
            else if (Model.FilBranch == "All")
            {
                mBranchFilter = "";
            }

            mstr = "Select l.Type,l.Prefix,l.Srl,l.Sno,l.BillNumber,convert(varchar, l.BillDate, 103) as BillDate,(select tb.name from tfatbranch tb where tb.Code = l.Branch) as BranchName,l.Branch,l.Debit as BillAmt,(case when l.Debit is null then 0 else (isnull(l.Debit,0) - (select Cast((isnull(sum(o.Amount),0))as Decimal(14,2)) from Outstanding o where o.TableRefKey = l.TableKey)) end) as BalAmt,l.Narr as Remark,l.Party,l.ParentKey,l.TableKey,l.MainType, l.SubType FROM LEDGER l WHERE l.Tablekey Not In (" + ExistBill + ") and l.Debit <> 0 and LEFT(l.Authorise,1) = 'A' and (isnull(l.Debit,0) - (select Cast((isnull(sum(o.Amount),0))as Decimal(14,2)) from Outstanding o where o.TableRefKey = l.TableKey)) > 0 and (l.Subtype = 'RS' or l.Subtype = 'CS') and l.Code = '" + Model.Account + "'" + mBranchFilter + " Order by l.RecordKey";


            List<DataRow> ordersstk = GetDataTable(mstr).AsEnumerable().ToList();
            Model.PickupList = Add2ItemForPickup(ordersstk, abc);
            Model.EquationList = charges.Select(x => x.EqAmt).ToList();
            Model.FldList = charges.Select(x => x.Fld).ToList();
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            var mcharges = ctxTFAT.Charges.Where(x => x.Type == "BRC00" && x.DontUse == false).Select(x => x.Head).ToArray();
            var mCount = charges.Count;
            var html = ViewHelper.RenderPartialView(this, "PickUp", new DebtorReceiptVM() { FldList = Model.FldList, UnAdjustedAmt = Model.UnAdjustedAmt, PickupList = Model.PickupList, EquationList = Model.EquationList/*.Where(x => !mLrList.Contains(x.FMNo)).OrderByDescending(x => x.FMDate).ToList()*/, HeaderList = mcharges.ToList(), TotalQty = mCount, AccAmt = Model.AccAmt });
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public ActionResult PostPickUp(DebtorReceiptVM Model)
        {
            try
            {
                List<DebtorReceiptVM> objitemlist = new List<DebtorReceiptVM>();

                List<string> mtablekeys = new List<string>();

                if (Session["Ddebtorlist"] != null)
                {
                    objitemlist = (List<DebtorReceiptVM>)Session["Ddebtorlist"];
                }
                decimal SumAmt = 0;
                int mMaxtempid = (objitemlist.Count == 0) ? 0 : objitemlist.Select(x => x.tempid).Max();

                foreach (var c in Model.PickupList.OrderBy(x => x.tempid))
                {
                    mMaxtempid = mMaxtempid + 1;
                    Model.ChargesList = GetChargesListPickUp(c, Model.Type);

                    List<DebtorReceiptVM> mCharges = new List<DebtorReceiptVM>();
                    foreach (var chg in Model.ChargesList)
                    {
                        decimal mcamt = 0;
                        if (chg.AddLess == "+")
                        {
                            mcamt = chg.Amt;
                        }
                        else if (chg.AddLess == "-")
                        {
                            mcamt = -chg.Amt;
                        }
                        else
                        {
                            mcamt = chg.Amt;
                        }
                        mCharges.Add(new DebtorReceiptVM()
                        {
                            Amt = mcamt,
                            AddLess = chg.AddLess

                        });
                    }

                    objitemlist.Add(new DebtorReceiptVM()
                    {
                        BillNumber = c.BillNumber,
                        StrBillDate = c.StrBillDate,
                        BillDate = ConvertDDMMYYTOYYMMDD(c.StrBillDate),
                        BranchName = c.BranchName,
                        Branch = c.Branch,
                        BillAmt = c.BillAmt,
                        BalAmt = c.BalAmt,
                        AdjustAmt = c.AdjustAmt,
                        Amt = c.Amt,
                        LedgerParentKey = c.LedgerParentKey,
                        LedgerTableKey = c.LedgerTableKey,
                        LedgerPrefix = c.LedgerPrefix,
                        LedgerType = c.LedgerType,
                        LedgerSubType = c.LedgerSubType,
                        LedgerMainType = c.LedgerMainType,
                        LedgerSrl = c.LedgerSrl,
                        LedgerSno = c.LedgerSno,
                        ChargesList = Model.ChargesList,
                        HeaderList = Model.ChargesList.Select(x => x.Header).ToList(),
                        ChgPickupList = Model.ChargesList.Select(x => x.Amt).ToList(),
                        Remark = c.Remark,
                        OptionType = "1",
                        NetAmt = (c.Amt) + ((mCharges != null && mCharges.Count > 0) ? mCharges.Where(x => x.AddLess.Trim() != "-").Sum(x => (decimal?)x.Amt) ?? 0 : 0),
                        tempid = mMaxtempid,
                        BillTDS = c.BillTDS
                    });


                }
                SumAmt += objitemlist.Sum(x => x.NetAmt);


                Session.Add("Ddebtorlist", objitemlist);

                Model.SelectedLedger = objitemlist;

                Model.HeaderList = ctxTFAT.Charges.Where(x => x.Type == "BRC00" && x.DontUse == false).Select(x => x.Head).ToList();

                Model.TotalTaxable = objitemlist.Sum(x => x.Amt);
                Model.TotalBillTds = objitemlist.Sum(x => x.BillTDS);
                Model.Taxable = objitemlist.Sum(x => x.Amt);
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

                Model.UnAdjustedAmt = Model.AccAmt - (objitemlist.Sum(x => x.NetAmt));


                #region TDS Calculate
                if (Model.TDSFlag)
                {
                    var TdsAC = "000009992";
                    Model.TDSAmt = 0;
                    var GetTdsFld = ctxTFAT.Charges.Where(x => x.Type == "BRC00" && x.Code == TdsAC).Select(x => x.Fld).FirstOrDefault();

                    if (!String.IsNullOrEmpty(GetTdsFld))
                    {
                        foreach (var item in objitemlist)
                        {
                            Model.TDSAmt += item.ChargesList.Where(x => x.Fld == GetTdsFld).Sum(x => x.Amt) >= 0 ? item.ChargesList.Where(x => x.Fld == GetTdsFld).Sum(x => x.Amt) : (-1) * item.ChargesList.Where(x => x.Fld == GetTdsFld).Sum(x => x.Amt);
                        }
                    }
                }

                //var datenow = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                //var TDSRATEtab = ctxTFAT.TDSRates.Where(x => x.Code.ToString() == Model.TDSCode && x.EffDate <= datenow && x.LimitFrom <= Model.Taxable).OrderByDescending(x => x.EffDate).Select(x => new { x.TDSRate, x.Cess, x.SHECess, x.SurCharge }).FirstOrDefault();
                //if (TDSRATEtab != null)
                //{
                //    Model.TDSAmt = (Model.TDSAmt != 0) ? Model.TDSAmt : (TDSRATEtab.TDSRate == null ? 0 : Math.Round(((TDSRATEtab.TDSRate.Value * Model.Taxable) / 100), 0));
                //}
                //else
                //{
                //    Model.TDSAmt = 0;
                //}





                Model.TDSAmt = Model.TotalBillTds;
                decimal mamtm = Model.Taxable + Model.IGSTAmt + Model.CGSTAmt + Model.SGSTAmt;

                #endregion

                string html = ViewHelper.RenderPartialView(this, "LedgerList", Model);

                var jsonResult = Json(new
                {
                    Total = mamtm,
                    IGSTAmt = Model.IGSTAmt,
                    CGSTAmt = Model.CGSTAmt,
                    SGSTAmt = Model.SGSTAmt,
                    TDSAmt = Model.TDSAmt,
                    SumAmt = SumAmt,
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

        public List<DebtorReceiptVM> GetChargesListPickUp(DebtorReceiptVM Model, string Type)
        {
            List<DebtorReceiptVM> newlist1 = new List<DebtorReceiptVM>();
            var mCharges = (from C in ctxTFAT.Charges
                            where C.Type == "BRC00" && C.DontUse == false

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
                DebtorReceiptVM c = new DebtorReceiptVM();
                c.Fld = i.Fld;
                c.Header = i.Head;
                c.AddLess = i.EqAmt;
                c.tempid = Convert.ToInt16(i.Fld.Substring(1, 3));
                c.PostCode = i.Code;
                c.Amt = (mStrArray[a] == 0) ? (decimal)0 : Convert.ToDecimal(mStrArray[a]);
                newlist1.Add(c);

                a = a + 1;
            }


            return newlist1;
        }

        public List<DebtorReceiptVM> GetLrWiseChargesLsit(DebtorReceiptVM Model)
        {
            List<DebtorReceiptVM> newlist1 = new List<DebtorReceiptVM>();
            var mCharges = (from C in ctxTFAT.Charges
                            where C.Type == "BRC00" && C.DontUse == false

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
                DebtorReceiptVM c = new DebtorReceiptVM();
                c.Fld = i.Fld;
                c.Header = i.Head;
                c.AddLess = i.EqAmt;
                c.tempid = Convert.ToInt16(i.Fld.Substring(1, 3));
                c.PostCode = i.Code;
                c.Amt = (mStrArray[a] == null) ? (decimal)0 : Convert.ToDecimal(mStrArray[a]);
                newlist1.Add(c);

                a = a + 1;
            }


            return newlist1;
        }



        public List<decimal> GetAdvChargesPickup(string abc, string TableKey)
        {
            List<decimal> newlist1 = new List<decimal>();
            //string mquery = (abc != "") ? ("Select " + abc + " from Ledger r WHERE r.TABLEKEY = '" + TableKey + "'") : abc;
            //DataTable chargeslist = GetDataTable(mquery);


            var mCharges = (from C in ctxTFAT.Charges
                            where C.Type == "BRC00" && C.DontUse == false

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
        public ActionResult PushPickupChargeList(DebtorReceiptVM Model)
        {
            List<decimal> ChargesListSelect = new List<decimal>();
            if (Model.ChargesList != null)
            {
                foreach (var item in Model.ChargesList)
                {
                    ChargesListSelect.Add(item.Amt);
                }
            }
            Model.AdvType = Model.AdvType == "A" ? "Advance" : "Balance";

            return Json(new
            {
                FMNo = Model.Code,
                AdvType = Model.AdvType,
                ChargesListSelect = ChargesListSelect,
                Remark = Model.Remark,
                RefTableKey = Model.RefTableKey
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region LedgerPost

        public ActionResult GetPostingNew(DebtorReceiptVM Model)
        {
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
            if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.Type == "BRC00" && x.LockDate == Date).FirstOrDefault() != null)
            {
                return Json(new
                {

                    Status = "ErrorValid",
                    Message = "Date is Locked By Period Lock System...!"
                }, JsonRequestBehavior.AllowGet);
            }

            List<DebtorReceiptVM> LedPostList = new List<DebtorReceiptVM>();
            int mCnt = 1;
            decimal MinusAmtOfMainAcc = 0;
            var objledgerdetail = (List<DebtorReceiptVM>)Session["Ddebtorlist"];

            var GetAllCharges = new List<DebtorReceiptVM>();
            if (objledgerdetail == null)
            {
                objledgerdetail = new List<DebtorReceiptVM>();
            }
            foreach (var item in objledgerdetail)
            {
                GetAllCharges.AddRange(item.ChargesList);
            }
            var Charges = ctxTFAT.Charges.Where(x => x.Type == "BRC00" && x.DontUse == false).ToList();

            foreach (var item in Charges)
            {
                if (item.Post)
                {
                    if (item.EqAmt != "-" && item.EqAmt != "+")
                    {
                        var Amount = GetAllCharges.Where(x => x.Fld == item.Fld).Sum(x => x.Amt);
                        MinusAmtOfMainAcc += Amount;
                    }
                }

            }


            LedPostList.Add(new DebtorReceiptVM()
            {
                Code = Model.Account,
                AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                Debit = 0,
                Credit = Math.Round((Model.AccAmt - MinusAmtOfMainAcc), 2),
                Branch = mbranchcode,
                tempid = mCnt,
                AltCode = Model.Bank
            });


            mCnt = mCnt + 1;

            if (MinusAmtOfMainAcc > 0)
            {
                foreach (var item in Charges)
                {
                    var Amount = GetAllCharges.Where(x => x.Fld == item.Fld).Sum(x => x.Amt);
                    if (item.EqAmt != "-" && item.EqAmt != "+")
                    {
                        LedPostList.Add(new DebtorReceiptVM()
                        {
                            Code = item.Code,
                            AccountName = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault(),
                            Debit = 0,
                            Credit = Math.Round(Amount, 2),
                            Branch = mbranchcode,
                            tempid = mCnt,
                            AltCode = Model.Bank
                        });

                        mCnt = mCnt + 1;
                    }
                }
            }


            LedPostList.Add(new DebtorReceiptVM()
            {
                Code = Model.Bank,
                AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Bank).Select(x => x.Name).FirstOrDefault(),
                Debit = Math.Round((Model.AccAmt), 2),
                Credit = 0,
                Branch = mbranchcode,
                tempid = mCnt,
                AltCode = Model.Account
            });

            mCnt = mCnt + 1;

            //if (Model.TDSFlag==true && Model.TDSAmt>0)
            //{
            //    var TdsSMaster = ctxTFAT.TDSMaster.Where(x => x.Code.ToString() == Model.Code).FirstOrDefault();

            //    LedPostList.Add(new DebtorReceiptVM()
            //    {
            //        Code = "000009992",
            //        AccountName = ctxTFAT.Master.Where(x => x.Code == "000009992").Select(x => x.Name).FirstOrDefault(),
            //        Debit = Math.Round(Model.TDSAmt, 2),
            //        Credit = 0,
            //        Branch = mbranchcode,
            //        tempid = mCnt,
            //        AltCode = Model.Account
            //    });

            //    mCnt = mCnt + 1;

            //    LedPostList.Add(new DebtorReceiptVM()
            //    {
            //        Code = Model.Account,
            //        AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
            //        Debit = 0,
            //        Credit = Math.Round(Model.TDSAmt, 2),
            //        Branch = mbranchcode,
            //        tempid = mCnt,
            //        AltCode = "000009992"
            //    });

            //    mCnt = mCnt + 1;
            //}

            foreach (var item in Charges)
            {
                var Amount = GetAllCharges.Where(x => x.Fld == item.Fld).Sum(x => x.Amt);
                if (item.EqAmt == "-" && Amount > 0)
                {
                    LedPostList.Add(new DebtorReceiptVM()
                    {
                        Code = item.Code,
                        AccountName = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault(),
                        Debit = Math.Round(Amount, 2),
                        Credit = 0,
                        Branch = mbranchcode,
                        tempid = mCnt,
                        AltCode = Model.Account
                    });

                    mCnt = mCnt + 1;

                    LedPostList.Add(new DebtorReceiptVM()
                    {
                        Code = Model.Account,
                        AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                        Debit = 0,
                        Credit = Math.Round(Amount, 2),
                        Branch = mbranchcode,
                        tempid = mCnt,
                        AltCode = item.Code
                    });

                    mCnt = mCnt + 1;
                }
                //else if (item.EqAmt != "+")
                //{
                //    LedPostList.Add(new DebtorReceiptVM()
                //    {
                //        Code = Model.Account,
                //        AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                //        Debit = Math.Round(Amount, 2),
                //        Credit = 0,
                //        Branch = mbranchcode,
                //        tempid = mCnt
                //    });

                //    mCnt = mCnt + 1;

                //    LedPostList.Add(new DebtorReceiptVM()
                //    {
                //        Code = item.Code,
                //        AccountName = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault(),
                //        Debit = 0,
                //        Credit = Math.Round(Amount, 2),
                //        Branch = mbranchcode,
                //        tempid = mCnt
                //    });

                //    mCnt = mCnt + 1;




                //}
            }

            var TdsAmt = objledgerdetail.Sum(x => x.BillTDS);
            if (Model.TDSFlag == true && Model.TDSAmt > 0 )
            {
                //TDSMaster tDSMaster = ctxTFAT.TDSMaster.Where(x => x.Code.ToString().Trim() == Model.TDSCode.Trim()).FirstOrDefault();
                //if (tDSMaster != null)
                {
                    LedPostList.Add(new DebtorReceiptVM()
                    {
                        Code = "000009992",
                        AccountName = ctxTFAT.Master.Where(x => x.Code == "000009992").Select(x => x.Name).FirstOrDefault(),
                        Debit = Math.Round(Model.TDSAmt, 2),
                        Credit = 0,
                        Branch = mbranchcode,
                        tempid = mCnt,
                        AltCode = Model.Account
                    });

                    mCnt = mCnt + 1;

                    LedPostList.Add(new DebtorReceiptVM()
                    {
                        Code = Model.Account,
                        AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                        Debit = 0,
                        Credit = Math.Round(Model.TDSAmt, 2),
                        Branch = mbranchcode,
                        tempid = mCnt,
                        AltCode = "000009992"
                    });

                    mCnt = mCnt + 1;
                }
            }


            Model.TotDebit = LedPostList.Sum(x => x.Debit);
            Model.TotCredit = LedPostList.Sum(x => x.Credit);
            Model.LedgerPostList = LedPostList;

            // display view form
            var html = ViewHelper.RenderPartialView(this, "LedgerPosting", new DebtorReceiptVM() { LedgerPostList = Model.LedgerPostList, TotDebit = Math.Round(Model.TotDebit, 2), TotCredit = Math.Round(Model.TotCredit, 2), Mode = Model.Mode, Controller2 = Model.Controller2, Type = Model.Type, Module = Model.Module, ViewDataId = Model.ViewDataId, TableName = Model.TableName, Header = Model.Header, OptionType = Model.OptionType, OptionCode = Model.OptionCode, MainType = Model.MainType, SubType = Model.SubType });
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
        #region SAVE EDIT



        public ActionResult DeleteData(DebtorReceiptVM Model)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var Date = ConvertDDMMYYTOYYMMDD(Model.DocuDate);

                    if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.Type == "BRC00" && x.LockDate == Date).FirstOrDefault() != null)
                    {
                        return Json(new
                        {

                            Status = "Error",
                            Message = "Date is Locked By Period Lock System...!"
                        }, JsonRequestBehavior.AllowGet); ;
                    }
                    else
                    {
                        DeUpdate(Model);
                    }

                    UpdateAuditTrail(mbranchcode, "Delete", Model.Header, Model.ParentKey, ConvertDDMMYYTOYYMMDD(Model.DocuDate), Model.Amt, Model.Bank, "Delete Debtor Receipt", "A");

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

        private void DeUpdate(DebtorReceiptVM Model)
        {
            if (ctxTFAT.TfatBranch.Where(z => z.Code == Model.Branch).Select(x => x.gp_AllowEditDelete).FirstOrDefault() == false)
            {
                var mDeleteAuth = ctxTFAT.Authorisation.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).ToList();
                ctxTFAT.Authorisation.RemoveRange(mDeleteAuth);

                var mDebtorRecVouDetails = ctxTFAT.DebtorRecVouDetail.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).ToList();
                ctxTFAT.DebtorRecVouDetail.RemoveRange(mDebtorRecVouDetails);

                var mOutstanding = ctxTFAT.Outstanding.Where(x => x.ParentKey == Model.ParentKey && x.DocBranch == Model.Branch).ToList();
                ctxTFAT.Outstanding.RemoveRange(mOutstanding);



                var mDeleteLed = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).ToList();
                ctxTFAT.Ledger.RemoveRange(mDeleteLed);

                var mDeleteNote = ctxTFAT.Narration.Where(x => x.TableKey == Model.ParentKey && x.Branch == Model.Branch).ToList();
                ctxTFAT.Narration.RemoveRange(mDeleteNote);
                var mDeleteTdspay = ctxTFAT.TDSPayments.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).ToList();
                ctxTFAT.TDSPayments.RemoveRange(mDeleteTdspay);
                var mDeleteCC = ctxTFAT.CostLedger.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).ToList();
                ctxTFAT.CostLedger.RemoveRange(mDeleteCC);
                var mDeleteSb = ctxTFAT.LedgerSL.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).ToList();
                ctxTFAT.LedgerSL.RemoveRange(mDeleteSb);
                var mDeleteAttach = ctxTFAT.Attachment.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).ToList();
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
                var led = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).Select(x => new { x.Prefix, x.DocDate }).FirstOrDefault();

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

        public ActionResult SaveData(DebtorReceiptVM Model)
        {

            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    if (Model.Mode == "Edit")
                    {
                        if (mbranchcode != Model.Branch)
                        {
                            transaction.Rollback();
                            return Json(new { Message = "Login Branch Does Not Match With Documnet Branch.", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                        }
                        DeUpdate(Model);
                    }
                    TempData["BRCPreviousDate"] = Model.DocuDate;

                    Model.Srl = (Model.Mode == "Edit") ? Model.Srl : GetLastSerial(Model.TableName, Model.Branch, "BRC00", mperiod, Model.SubType, DateTime.Now.Date);
                    Model.ParentKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + Model.Srl;

                    Master master = ctxTFAT.Master.Where(x => x.Code == Model.Account).FirstOrDefault();
                    if (String.IsNullOrEmpty(master.DefaultCustomer))
                    {
                        return Json(new
                        {
                            Message = "Missing Default Customer Code..",
                            Status = "Error"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    #region Authorisation
                    TfatUserAuditHeader authorisation = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == "BRC00").FirstOrDefault();
                    if (authorisation != null)
                    {
                        mauthorise = SetAuthorisationLogistics(authorisation, Model.ParentKey, Model.Srl.ToString(), 0, Model.DocuDate, Model.AccAmt, Model.Account, mbranchcode);
                    }
                    #endregion


                    #region Ledger Save

                    List<Ledger> LedgerList = new List<Ledger>();
                    int mLcnt = 1;
                    foreach (var chg in Model.LedgerPostList)
                    {
                        Ledger mobj3 = new Ledger();
                        mobj3.TableKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + mLcnt.ToString("D3") + Model.Srl;
                        mobj3.ParentKey = Model.ParentKey;
                        mobj3.Sno = mLcnt;
                        mobj3.Srl = Model.Srl;
                        mobj3.SubType = Model.SubType;
                        mobj3.Type = Model.Type;
                        mobj3.AltCode = chg.AltCode;
                        mobj3.Audited = false;
                        mobj3.AUTHIDS = muserid;
                        mobj3.AUTHORISE = mauthorise;
                        mobj3.BillDate = DateTime.Now;
                        mobj3.BillNumber = (Model.BillNumber == null || Model.BillNumber.Trim() == "") ? Model.Srl : Model.BillNumber;
                        mobj3.Branch = Model.Branch;
                        mobj3.CompCode = mcompcode;
                        mobj3.Cheque = Model.ChequeNo;
                        mobj3.ChequeReturn = false;
                        mobj3.ClearDate = Convert.ToDateTime("1900-01-01");
                        mobj3.ChequeDate = Convert.ToDateTime("1900-01-01");
                        mobj3.Code = chg.Code;
                        mobj3.CrPeriod = 0;
                        mobj3.CurrName = 1;
                        mobj3.CurrRate = 1;
                        mobj3.Debit = chg.Debit;
                        mobj3.Credit = chg.Credit;
                        mobj3.Discounted = true;
                        mobj3.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                        mobj3.MainType = Model.MainType;
                        mobj3.Narr = Model.Remark;
                        mobj3.Party = master.DefaultCustomer;
                        mobj3.Prefix = Model.Prefix;
                        mobj3.RecoFlag = "";
                        mobj3.RefDoc = "";
                        mobj3.TDSChallanNumber = "";
                        mobj3.TDSCode = Model.TDSCode == null ? 0 : Convert.ToInt32(Model.TDSCode.ToString());
                        mobj3.TDSFlag = Model.TDSFlag;
                        mobj3.BankCode = 0;
                        mobj3.ENTEREDBY = muserid;
                        mobj3.DueDate = Convert.ToDateTime("1900-01-01");
                        mobj3.Reminder = false;
                        mobj3.TaskID = 0;
                        mobj3.ChqCategory = 0;
                        mobj3.CurrAmount = 0;
                        mobj3.LocationCode = 100001;
                        mobj3.LASTUPDATEDATE = DateTime.Now;
                        mobj3.GSTType = 0;
                        mobj3.TaxCode = "";
                        mobj3.IGSTAmt = 0;
                        mobj3.IGSTRate = 0;
                        mobj3.CGSTAmt = 0;
                        mobj3.CGSTRate = 0;
                        mobj3.SGSTAmt = 0;
                        mobj3.SGSTRate = 0;
                        mobj3.SLCode = 0/*item.SLCode*/;

                        ctxTFAT.Ledger.Add(mobj3);
                        LedgerList.Add(mobj3);

                        mLcnt = mLcnt + 1;
                    }

                    #endregion

                    int mCnt = 1;
                    var objledgerdetail = (List<DebtorReceiptVM>)Session["Ddebtorlist"];
                    if (objledgerdetail == null)
                    {
                        objledgerdetail = new List<DebtorReceiptVM>();
                    }
                    foreach (var item in objledgerdetail)
                    {
                        DebtorRecVouDetail vd = new DebtorRecVouDetail();
                        vd.Amount = item.Amt;
                        vd.Branch = Model.Branch;

                        vd.Sno = mCnt;
                        vd.Srl = Model.Srl;
                        vd.ENTEREDBY = muserid;
                        vd.LASTUPDATEDATE = DateTime.Now;
                        vd.AUTHORISE = mauthorise;
                        vd.AUTHIDS = muserid;
                        vd.Val1 = item.ChargesList.Where(x => x.Fld == "F001").Select(X => X.Amt).FirstOrDefault();
                        vd.Val2 = item.ChargesList.Where(x => x.Fld == "F002").Select(X => X.Amt).FirstOrDefault();
                        vd.Val3 = item.ChargesList.Where(x => x.Fld == "F003").Select(X => X.Amt).FirstOrDefault();
                        vd.Val4 = item.ChargesList.Where(x => x.Fld == "F004").Select(X => X.Amt).FirstOrDefault();
                        vd.Val5 = item.ChargesList.Where(x => x.Fld == "F005").Select(X => X.Amt).FirstOrDefault();
                        vd.Val6 = item.ChargesList.Where(x => x.Fld == "F006").Select(X => X.Amt).FirstOrDefault();
                        vd.Val7 = item.ChargesList.Where(x => x.Fld == "F007").Select(X => X.Amt).FirstOrDefault();
                        vd.Val8 = item.ChargesList.Where(x => x.Fld == "F008").Select(X => X.Amt).FirstOrDefault();
                        vd.Val9 = item.ChargesList.Where(x => x.Fld == "F009").Select(X => X.Amt).FirstOrDefault();
                        vd.Val10 = item.ChargesList.Where(x => x.Fld == "F010").Select(X => X.Amt).FirstOrDefault();
                        vd.TableKey = mbranchcode + "BRC00" + Model.Prefix.Substring(0, 2) + mCnt.ToString("D3") + Model.Srl;
                        vd.ParentKey = mbranchcode + "BRC00" + Model.Prefix.Substring(0, 2) + Model.Srl;
                        vd.Prefix = Model.Prefix;
                        vd.NetAmt = Convert.ToDecimal(item.Amt) + ((item.ChargesList != null && item.ChargesList.Count > 0) ? item.ChargesList.Where(x => x.AddLess.Trim() != "-").Sum(x => (decimal?)x.Amt) ?? 0 : 0);
                        vd.Type = "BRC00";
                        vd.InvBranch = item.Branch;
                        vd.InvTableKey = item.LedgerTableKey;
                        vd.InvPrefix = item.LedgerPrefix;
                        vd.InvType = item.LedgerType;
                        vd.InvSrl = item.LedgerSrl;
                        vd.InvSno = item.LedgerSno;
                        vd.TdsAmout = item.BillTDS;
                        vd.AutoAdjInterface = item.AutoAdjInterface;

                        ctxTFAT.DebtorRecVouDetail.Add(vd);


                        if (vd.Amount > 0)
                        {

                            Outstanding osobj1 = new Outstanding();
                            osobj1.Branch = ctxTFAT.Ledger.Where(x => x.TableKey == item.LedgerTableKey).Select(x => x.Branch).FirstOrDefault();
                            osobj1.DocBranch = Model.Branch;
                            osobj1.MainType = Model.MainType;
                            osobj1.SubType = Model.SubType;
                            osobj1.Type = Model.Type;
                            osobj1.Prefix = Model.Prefix;
                            osobj1.Srl = Model.Srl;
                            osobj1.Sno = 1;
                            osobj1.ParentKey = Model.ParentKey;
                            osobj1.TableKey = mbranchcode + Model.Type + mperiod.Substring(0, 2) + 1.ToString("D3") + Model.Srl;
                            osobj1.aMaintype = item.LedgerMainType;
                            osobj1.aSubType = item.LedgerSubType;
                            osobj1.aType = item.LedgerType;
                            osobj1.aPrefix = item.LedgerPrefix;
                            osobj1.aSrl = item.LedgerSrl;
                            osobj1.aSno = item.LedgerSno;
                            //osobj1.Amount = Convert.ToDecimal(item.Amt)+ ((item.ChargesList != null && item.ChargesList.Count > 0) ? item.ChargesList.Where(x => x.AddLess.Trim() == "-").Sum(x => (decimal?)x.Amt) ?? 0 : 0);
                            osobj1.Amount = Convert.ToDecimal(item.Amt);
                            osobj1.TableRefKey = item.LedgerTableKey;
                            osobj1.AUTHIDS = muserid;
                            osobj1.AUTHORISE = mauthorise;
                            osobj1.BillDate = Convert.ToDateTime(item.StrBillDate);
                            osobj1.BillNumber = item.BillNumber == null ? " " : item.BillNumber;
                            osobj1.CompCode = mcompcode;
                            osobj1.Broker = 100001;
                            osobj1.Brokerage = Convert.ToDecimal(0.00);
                            osobj1.BrokerAmt = Convert.ToDecimal(0.00);
                            osobj1.BrokerOn = Convert.ToDecimal(0.00);
                            osobj1.ChlnDate = DateTime.Now;
                            osobj1.ChlnNumber = "";
                            osobj1.Code = Model.Account;
                            osobj1.CrPeriod = 0;
                            osobj1.CurrName = 0;
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
                            //osobj1.CurrAmount = Convert.ToDecimal(item.Amt) + ((item.ChargesList != null && item.ChargesList.Count > 0) ? item.ChargesList.Where(x => x.AddLess.Trim() == "-").Sum(x => (decimal?)x.Amt) ?? 0 : 0);
                            osobj1.CurrAmount = Convert.ToDecimal(item.Amt);
                            osobj1.ValueDate = DateTime.Now;
                            osobj1.LocationCode = 100001;

                            ctxTFAT.Outstanding.Add(osobj1);
                            // second effect
                            Outstanding osobj2 = new Outstanding();
                            osobj2.Branch = Model.Branch;
                            osobj2.DocBranch = Model.Branch;
                            osobj2.aType = Model.Type;
                            osobj2.aPrefix = Model.Prefix;
                            osobj2.aSrl = Model.Srl;
                            osobj2.aSno = 1;
                            osobj2.aMaintype = Model.MainType;
                            osobj2.aSubType = Model.SubType;
                            osobj2.ParentKey = Model.ParentKey;
                            osobj2.TableRefKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + 1.ToString("D3") + Model.Srl;
                            osobj2.Type = item.LedgerType;
                            osobj2.Prefix = item.LedgerPrefix;
                            osobj2.MainType = item.LedgerMainType;
                            osobj2.SubType = item.LedgerSubType;
                            osobj2.Srl = item.LedgerSrl;
                            osobj2.Sno = item.LedgerSno;
                            osobj2.TableKey = item.LedgerTableKey;
                            //osobj2.Amount = Convert.ToDecimal(item.Amt) + ((item.ChargesList != null && item.ChargesList.Count > 0) ? item.ChargesList.Where(x => x.AddLess.Trim() == "-").Sum(x => (decimal?)x.Amt) ?? 0 : 0);
                            osobj2.Amount = Convert.ToDecimal(item.Amt);
                            osobj2.AUTHIDS = muserid;
                            osobj2.AUTHORISE = mauthorise;
                            osobj2.BillDate = Convert.ToDateTime(item.StrBillDate);
                            osobj2.BillNumber = item.BillNumber == null ? " " : item.BillNumber;
                            osobj2.CompCode = mcompcode;
                            osobj2.Broker = 100001;
                            osobj2.Brokerage = Convert.ToDecimal(0.00);
                            osobj2.BrokerAmt = Convert.ToDecimal(0.00);
                            osobj2.BrokerOn = Convert.ToDecimal(0.00);
                            osobj2.ChlnDate = DateTime.Now;
                            osobj2.ChlnNumber = "";
                            osobj2.Code = Model.Account;
                            osobj2.CrPeriod = 0;
                            osobj2.CurrName = 0;
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
                            //osobj2.CurrAmount = Convert.ToDecimal(item.Amt) + ((item.ChargesList != null && item.ChargesList.Count > 0) ? item.ChargesList.Where(x => x.AddLess.Trim() == "-").Sum(x => (decimal?)x.Amt) ?? 0 : 0);
                            osobj2.CurrAmount = Convert.ToDecimal(item.Amt);
                            osobj2.ValueDate = DateTime.Now;
                            osobj2.LocationCode = 100001;

                            ctxTFAT.Outstanding.Add(osobj2);

                        }

                        foreach (var Chrg in item.ChargesList)
                        {
                            if (Chrg.AddLess == "-" && Chrg.Amt > 0)
                            {
                                var Charges = ctxTFAT.Charges.Where(x => x.Type == "BRC00" && x.Fld == Chrg.Fld).FirstOrDefault();
                                if (Charges.Post)
                                {
                                    var BillDetails = LedgerList.Where(x => x.AltCode == Charges.Code && x.Code == Model.Account).Select(x => new { x.TableKey, x.Sno }).FirstOrDefault();
                                    if (!String.IsNullOrEmpty(BillDetails.TableKey))
                                    {
                                        Outstanding osobj1 = new Outstanding();
                                        osobj1.Branch = ctxTFAT.Ledger.Where(x => x.TableKey == item.LedgerTableKey).Select(x => x.Branch).FirstOrDefault();
                                        osobj1.DocBranch = Model.Branch;
                                        osobj1.MainType = Model.MainType;
                                        osobj1.SubType = Model.SubType;
                                        osobj1.Type = Model.Type;
                                        osobj1.Prefix = Model.Prefix;
                                        osobj1.Srl = Model.Srl;
                                        osobj1.Sno = BillDetails.Sno;
                                        osobj1.ParentKey = Model.ParentKey;
                                        osobj1.TableKey = BillDetails.TableKey;
                                        osobj1.aMaintype = item.LedgerMainType;
                                        osobj1.aSubType = item.LedgerSubType;
                                        osobj1.aType = item.LedgerType;
                                        osobj1.aPrefix = item.LedgerPrefix;
                                        osobj1.aSrl = item.LedgerSrl;
                                        osobj1.aSno = item.LedgerSno;
                                        //osobj1.Amount = Convert.ToDecimal(item.Amt)+ ((item.ChargesList != null && item.ChargesList.Count > 0) ? item.ChargesList.Where(x => x.AddLess.Trim() == "-").Sum(x => (decimal?)x.Amt) ?? 0 : 0);
                                        osobj1.Amount = Convert.ToDecimal(Chrg.Amt);
                                        osobj1.TableRefKey = item.LedgerTableKey;
                                        osobj1.AUTHIDS = muserid;
                                        osobj1.AUTHORISE = mauthorise;
                                        osobj1.BillDate = Convert.ToDateTime(item.StrBillDate);
                                        osobj1.BillNumber = item.BillNumber == null ? " " : item.BillNumber;
                                        osobj1.CompCode = mcompcode;
                                        osobj1.Broker = 100001;
                                        osobj1.Brokerage = Convert.ToDecimal(0.00);
                                        osobj1.BrokerAmt = Convert.ToDecimal(0.00);
                                        osobj1.BrokerOn = Convert.ToDecimal(0.00);
                                        osobj1.ChlnDate = DateTime.Now;
                                        osobj1.ChlnNumber = "";
                                        osobj1.Code = Charges.Code;
                                        osobj1.CrPeriod = 0;
                                        osobj1.CurrName = 0;
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
                                        //osobj1.CurrAmount = Convert.ToDecimal(item.Amt) + ((item.ChargesList != null && item.ChargesList.Count > 0) ? item.ChargesList.Where(x => x.AddLess.Trim() == "-").Sum(x => (decimal?)x.Amt) ?? 0 : 0);
                                        osobj1.CurrAmount = Convert.ToDecimal(Chrg.Amt);
                                        osobj1.ValueDate = DateTime.Now;
                                        osobj1.LocationCode = 100001;

                                        ctxTFAT.Outstanding.Add(osobj1);
                                        // second effect
                                        Outstanding osobj2 = new Outstanding();
                                        osobj2.Branch = Model.Branch;
                                        osobj2.DocBranch = Model.Branch;
                                        osobj2.aType = Model.Type;
                                        osobj2.aPrefix = Model.Prefix;
                                        osobj2.aSrl = Model.Srl;
                                        osobj2.aSno = BillDetails.Sno;
                                        osobj2.aMaintype = Model.MainType;
                                        osobj2.aSubType = Model.SubType;
                                        osobj2.ParentKey = Model.ParentKey;
                                        osobj2.TableRefKey = BillDetails.TableKey;
                                        osobj2.Type = item.LedgerType;
                                        osobj2.Prefix = item.LedgerPrefix;
                                        osobj2.MainType = item.LedgerMainType;
                                        osobj2.SubType = item.LedgerSubType;
                                        osobj2.Srl = item.LedgerSrl;
                                        osobj2.Sno = item.LedgerSno;
                                        osobj2.TableKey = item.LedgerTableKey;
                                        //osobj2.Amount = Convert.ToDecimal(item.Amt) + ((item.ChargesList != null && item.ChargesList.Count > 0) ? item.ChargesList.Where(x => x.AddLess.Trim() == "-").Sum(x => (decimal?)x.Amt) ?? 0 : 0);
                                        osobj2.Amount = Convert.ToDecimal(Chrg.Amt);
                                        osobj2.AUTHIDS = muserid;
                                        osobj2.AUTHORISE = mauthorise;
                                        osobj2.BillDate = Convert.ToDateTime(item.StrBillDate);
                                        osobj2.BillNumber = item.BillNumber == null ? " " : item.BillNumber;
                                        osobj2.CompCode = mcompcode;
                                        osobj2.Broker = 100001;
                                        osobj2.Brokerage = Convert.ToDecimal(0.00);
                                        osobj2.BrokerAmt = Convert.ToDecimal(0.00);
                                        osobj2.BrokerOn = Convert.ToDecimal(0.00);
                                        osobj2.ChlnDate = DateTime.Now;
                                        osobj2.ChlnNumber = "";
                                        osobj2.Code = Charges.Code;
                                        osobj2.CrPeriod = 0;
                                        osobj2.CurrName = 0;
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
                                        //osobj2.CurrAmount = Convert.ToDecimal(item.Amt) + ((item.ChargesList != null && item.ChargesList.Count > 0) ? item.ChargesList.Where(x => x.AddLess.Trim() == "-").Sum(x => (decimal?)x.Amt) ?? 0 : 0);
                                        osobj2.CurrAmount = Convert.ToDecimal(Chrg.Amt);
                                        osobj2.ValueDate = DateTime.Now;
                                        osobj2.LocationCode = 100001;

                                        ctxTFAT.Outstanding.Add(osobj2);
                                    }
                                }
                            }
                        }

                        if (item.BillTDS > 0)
                        {
                            TDSMaster tDSMaster = ctxTFAT.TDSMaster.Where(x => x.Code.ToString().Trim() == Model.TDSCode).FirstOrDefault();
                            var BillDetails = LedgerList.Where(x => x.AltCode == tDSMaster.PostCode && x.Code == Model.Account).Select(x => new { x.TableKey, x.Sno }).FirstOrDefault();
                            if (!String.IsNullOrEmpty(BillDetails.TableKey))
                            {
                                Outstanding osobj1 = new Outstanding();
                                osobj1.Branch = ctxTFAT.Ledger.Where(x => x.TableKey == item.LedgerTableKey).Select(x => x.Branch).FirstOrDefault();
                                osobj1.DocBranch = Model.Branch;
                                osobj1.MainType = Model.MainType;
                                osobj1.SubType = Model.SubType;
                                osobj1.Type = Model.Type;
                                osobj1.Prefix = Model.Prefix;
                                osobj1.Srl = Model.Srl;
                                osobj1.Sno = BillDetails.Sno;
                                osobj1.ParentKey = Model.ParentKey;
                                osobj1.TableKey = BillDetails.TableKey;
                                osobj1.aMaintype = item.LedgerMainType;
                                osobj1.aSubType = item.LedgerSubType;
                                osobj1.aType = item.LedgerType;
                                osobj1.aPrefix = item.LedgerPrefix;
                                osobj1.aSrl = item.LedgerSrl;
                                osobj1.aSno = item.LedgerSno;
                                //osobj1.Amount = Convert.ToDecimal(item.Amt)+ ((item.ChargesList != null && item.ChargesList.Count > 0) ? item.ChargesList.Where(x => x.AddLess.Trim() == "-").Sum(x => (decimal?)x.Amt) ?? 0 : 0);
                                osobj1.Amount = Convert.ToDecimal(item.BillTDS);
                                osobj1.TableRefKey = item.LedgerTableKey;
                                osobj1.AUTHIDS = muserid;
                                osobj1.AUTHORISE = mauthorise;
                                osobj1.BillDate = Convert.ToDateTime(item.StrBillDate);
                                osobj1.BillNumber = item.BillNumber == null ? " " : item.BillNumber;
                                osobj1.CompCode = mcompcode;
                                osobj1.Broker = 100001;
                                osobj1.Brokerage = Convert.ToDecimal(0.00);
                                osobj1.BrokerAmt = Convert.ToDecimal(0.00);
                                osobj1.BrokerOn = Convert.ToDecimal(0.00);
                                osobj1.ChlnDate = DateTime.Now;
                                osobj1.ChlnNumber = "";
                                osobj1.Code = tDSMaster.PostCode;
                                osobj1.CrPeriod = 0;
                                osobj1.CurrName = 0;
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
                                //osobj1.CurrAmount = Convert.ToDecimal(item.Amt) + ((item.ChargesList != null && item.ChargesList.Count > 0) ? item.ChargesList.Where(x => x.AddLess.Trim() == "-").Sum(x => (decimal?)x.Amt) ?? 0 : 0);
                                osobj1.CurrAmount = Convert.ToDecimal(item.BillTDS);
                                osobj1.ValueDate = DateTime.Now;
                                osobj1.LocationCode = 100001;

                                ctxTFAT.Outstanding.Add(osobj1);
                                // second effect
                                Outstanding osobj2 = new Outstanding();
                                osobj2.Branch = Model.Branch;
                                osobj2.DocBranch = Model.Branch;
                                osobj2.aType = Model.Type;
                                osobj2.aPrefix = Model.Prefix;
                                osobj2.aSrl = Model.Srl;
                                osobj2.aSno = BillDetails.Sno;
                                osobj2.aMaintype = Model.MainType;
                                osobj2.aSubType = Model.SubType;
                                osobj2.ParentKey = Model.ParentKey;
                                osobj2.TableRefKey = BillDetails.TableKey;
                                osobj2.Type = item.LedgerType;
                                osobj2.Prefix = item.LedgerPrefix;
                                osobj2.MainType = item.LedgerMainType;
                                osobj2.SubType = item.LedgerSubType;
                                osobj2.Srl = item.LedgerSrl;
                                osobj2.Sno = item.LedgerSno;
                                osobj2.TableKey = item.LedgerTableKey;
                                //osobj2.Amount = Convert.ToDecimal(item.Amt) + ((item.ChargesList != null && item.ChargesList.Count > 0) ? item.ChargesList.Where(x => x.AddLess.Trim() == "-").Sum(x => (decimal?)x.Amt) ?? 0 : 0);
                                osobj2.Amount = Convert.ToDecimal(item.BillTDS);
                                osobj2.AUTHIDS = muserid;
                                osobj2.AUTHORISE = mauthorise;
                                osobj2.BillDate = Convert.ToDateTime(item.StrBillDate);
                                osobj2.BillNumber = item.BillNumber == null ? " " : item.BillNumber;
                                osobj2.CompCode = mcompcode;
                                osobj2.Broker = 100001;
                                osobj2.Brokerage = Convert.ToDecimal(0.00);
                                osobj2.BrokerAmt = Convert.ToDecimal(0.00);
                                osobj2.BrokerOn = Convert.ToDecimal(0.00);
                                osobj2.ChlnDate = DateTime.Now;
                                osobj2.ChlnNumber = "";
                                osobj2.Code = tDSMaster.PostCode;
                                osobj2.CrPeriod = 0;
                                osobj2.CurrName = 0;
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
                                //osobj2.CurrAmount = Convert.ToDecimal(item.Amt) + ((item.ChargesList != null && item.ChargesList.Count > 0) ? item.ChargesList.Where(x => x.AddLess.Trim() == "-").Sum(x => (decimal?)x.Amt) ?? 0 : 0);
                                osobj2.CurrAmount = Convert.ToDecimal(item.BillTDS);
                                osobj2.ValueDate = DateTime.Now;
                                osobj2.LocationCode = 100001;

                                ctxTFAT.Outstanding.Add(osobj2);
                            }
                        }


                        mCnt = mCnt + 1;

                    }



                    ctxTFAT.SaveChanges();

                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, Model.ParentKey, ConvertDDMMYYTOYYMMDD(Model.DocuDate), Model.Amt, Model.Bank, "Save Debtor Receipt", "A");

                    Session["Ddebtorlist"] = null;
                    return Json(new { Status = "Success", Model, NewSrl = (Model.Branch+Model.ParentKey) }, JsonRequestBehavior.AllowGet);
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
                catch (Exception ex)
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

        public List<DebtorReceiptVM> GetFMWiseCharges(string tablekey, string branch)
        {
            List<DebtorReceiptVM> objledgerdetail = new List<DebtorReceiptVM>();
            var trncharges = ctxTFAT.Charges.Where(x => x.Type == "BRC00" && x.DontUse == false).Select(x => new { x.Fld, x.Head, x.EqAmt, x.Equation, x.Code }).ToList();
            foreach (var i in trncharges)
            {
                DebtorReceiptVM c = new DebtorReceiptVM();
                c.Fld = i.Fld;
                c.Header = i.Head;
                c.AddLess = i.EqAmt;
                c.PostCode = i.Code;
                c.tempid = Convert.ToInt16(i.Fld.Substring(1, 3));
                c.Amt = GetChargeValValue(c.tempid, tablekey, branch);


                objledgerdetail.Add(c);
            }
            return objledgerdetail;
        }
        public decimal GetChargeValValue(int i, string tablekey, string Branch)
        {
            string connstring = GetConnectionString();
            decimal abc;

            var loginQuery3 = @"select Val" + i + " from DebtorRecVouDetail where tablekey = '" + tablekey + "' and Branch = '" + Branch + "'";
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
        public string GetSrl()
        {
            string mCode = "";
            int mmCode = 0;
            string mCodeSerial = @"select Top 1 Srl from DebtorRecVouDetail order by cast(Srl as int) desc";
            DataTable mDt2 = GetDataTable(mCodeSerial, GetConnectionString());
            if (mDt2.Rows.Count > 0)
            {
                mmCode = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? 0 : Convert.ToInt32(mDt2.Rows[0][0].ToString());
                ++mmCode;
            }
            else
            {
                mmCode = 0;
                ++mmCode;
            }
            mCode = mmCode.ToString("D6");
            return mCode;
        }




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
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), Model.Format.Trim() + ".rpt"));
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
                    rd.Load(Path.Combine(Server.MapPath("~/Reports"), mformat.Trim() + ".rpt"));
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


    }
}