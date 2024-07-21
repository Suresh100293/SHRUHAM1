using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Controllers;
using System.Web.Script.Serialization;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WebForms;
using ALT_ERP3.Areas.Logistics.Models;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class AdvancePayController : BaseController
    {
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
        private static string mauthorise = "A00";

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
            return GetGridDataColumns(id, "L", "XXXX");
        }

        public ActionResult GetMasterGridData(GridOption Model)
        {
            Model.Code = "r.Code ='" + Model.Code + "'";

            List<AdvancePayModel> objitemlist = new List<AdvancePayModel>();
            if (Session["Aadvancepaylist"] != null)
            {
                objitemlist = (List<AdvancePayModel>)Session["Aadvancepaylist"];
            }
            string LRList = "";
            if (objitemlist != null)
            {
                foreach (var item in objitemlist)
                {
                    LRList += "'" + item.RefTableKey + "',";
                }
                if (!String.IsNullOrEmpty(LRList))
                {
                    LRList = LRList.Substring(0, LRList.Length - 1);
                    Model.Code += " And r.tablekey not in (" + LRList + ")";
                }
            }

            return GetGridReport(Model, "M", "Code^" + Model.Code + (mpara != "" ? "~" + mpara : ""), false, 0);
        }
        
        // GET: Accounts/AdvancePay
        public ActionResult Index(AdvancePayModel Model)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, Model.ParentKey, DateTime.Now, 0, "", "", "NA");
            AdvBalSetup advBalSetup = ctxTFAT.AdvBalSetup.FirstOrDefault();
            if (advBalSetup == null)
            {
                advBalSetup = new AdvBalSetup();
            }
            Model.TDSBillWiseCut = advBalSetup.BillwiseCutTds;
            Model.RoundOff = advBalSetup.RoundOff;
            if (advBalSetup.CurrDatetOnlyreq == false && advBalSetup.BackDateAllow == false && advBalSetup.ForwardDateAllow == false)
            {
                Model.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
                Model.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
            }
            if (advBalSetup.CurrDatetOnlyreq == true)
            {
                Model.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
                Model.EndDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            if (advBalSetup.BackDateAllow == true)
            {
                Model.StartDate = (DateTime.Now.AddDays(-advBalSetup.BackDaysUpto)).ToString("yyyy-MM-dd");
            }
            if (advBalSetup.ForwardDateAllow == true)
            {
                Model.EndDate = (DateTime.Now.AddDays(advBalSetup.ForwardDaysUpto)).ToString("yyyy-MM-dd");
            }


            if (Model.Mode == "Add")
            {
                Model.DocDate = DateTime.Now.Date;
                Session["Aadvancepaylist"] = null;
                Session["AothertrxlistAdv"] = null;
                Model.MainType = "PM";
                Model.SubType = "BP";
                Model.Type = "FMP00";
                Model.Prefix = mperiod;
                Model.Branch = mbranchcode;
                Model.HeaderList = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.DontUse == false).Select(x => x.Head).ToList();
                Model.AddlessList = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.DontUse == false).Select(x => x.EqAmt).ToList();
            }
            else
            {
                Model.MainType = "PM";
                Model.SubType = "BP";
                Model.Type = "FMP00";

                Model.Branch = Model.Document.Substring(0, 6);
                Model.ParentKey = Model.Document.Substring(6);
                //Model.Type = Model.ParentKey.Substring(0, 5);
                var mVouchMaster = ctxTFAT.VoucherMaster.Where(x => x.TableKey == Model.ParentKey && x.Branch == Model.Branch).FirstOrDefault();
                var mVoucherDetail = ctxTFAT.VoucherDetail.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).Select(x => x).ToList();
                Model.DocDate = mVouchMaster.VouDate;
                Model.Bank = mVouchMaster.Bank;
                Model.BankName = ctxTFAT.Master.Where(x => x.Code == mVouchMaster.Bank).Select(x => x.Name).FirstOrDefault();
                Model.ChequeNo = mVouchMaster.ChqNo;
                Model.Remark = mVouchMaster.Remark;
                Model.Account = mVouchMaster.Account;
                Model.AccountName = ctxTFAT.Master.Where(x => x.Code == mVouchMaster.Account).Select(x => x.Name).FirstOrDefault();
                Model.Srl = mVouchMaster.VouNo;
                Model.PeriodLock = PeriodLock(mVouchMaster.Branch, mVouchMaster.Type, mVouchMaster.VouDate);
                if (mVouchMaster.AUTHORISE.Substring(0, 1) == "A")
                {
                    Model.LockAuthorise = LockAuthorise(mVouchMaster.Type, Model.Mode, mVouchMaster.TableKey, mVouchMaster.PayParentKey);
                }
                Model.Prefix = mVouchMaster.Prefix;
                List<AdvancePayModel> SelectedLedger = new List<AdvancePayModel>();
                var objledgerdetail = ctxTFAT.VoucherDetail.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).Select(x => x).ToList();
                foreach (var item in objledgerdetail)
                {
                    int mFmNo = (string.IsNullOrEmpty(item.FMNo) == true) ? 0 : Convert.ToInt32(item.FMNo);
                    {
                        var mFMROUVEL = ctxTFAT.Ledger.Where(X => X.TableKey == item.FMTableKey && X.Branch == item.FmBran).Select(x => x).FirstOrDefault();
                        if (mFMROUVEL != null)
                        {
                            var mchargelist = GetFMWiseCharges(Model.Srl, item.FMNo, item.FMTableKey);
                            //var DDFD = ctxTFAT.VoucherDetail.Where(x => x.FMTableKey == item.FMTableKey && x.FmBran == item.FmBran && ( x.ParentKey != Model.ParentKey && x.Branch != item.Branch)).ToList();
                            decimal actualamount = (ctxTFAT.VoucherDetail.Where(x => x.FMTableKey == item.FMTableKey && x.FmBran == item.FmBran && x.Branch + x.ParentKey != item.Branch + Model.ParentKey).Sum(x => (decimal?)x.Amount) ?? 0);
                            decimal ClaimAmt = (ctxTFAT.VoucherDetail.Where(x => x.FMTableKey == item.FMTableKey && x.FmBran == item.FmBran && x.Branch + x.ParentKey != item.Branch + Model.ParentKey).Sum(x => (decimal?)x.Val5) ?? 0);
                            decimal TdsAmt = (ctxTFAT.VoucherDetail.Where(x => x.FMTableKey == item.FMTableKey && x.FmBran == item.FmBran && x.Branch + x.ParentKey != item.Branch + Model.ParentKey).Sum(x => (decimal?)x.TdsAmout) ?? 0);

                            FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.ParentKey == item.FMRefParentkey.Trim()).FirstOrDefault();

                            SelectedLedger.Add(new AdvancePayModel()
                            {

                                Branch = item.FmBran,
                                BranchName = ctxTFAT.TfatBranch.Where(x => x.Code == item.FmBran).Select(x => x.Name).FirstOrDefault(),
                                FMDateStr = "",
                                FMDate = fMMaster == null ? DateTime.Now : fMMaster.Date,
                                FMNo = item.FMNo,
                                FMRefParentkey = fMMaster.ParentKey,
                                AdvPending = (item.InsClr == "A") ? (mFMROUVEL.Credit.Value - (actualamount + ClaimAmt + TdsAmt)) : (decimal)0,
                                BalPending = (item.InsClr == "B") ? (mFMROUVEL.Credit.Value - (actualamount + ClaimAmt + TdsAmt)) : (decimal)0,
                                Amt = item.Amount.Value,
                                AdvAmt = mchargelist.Where(x => x.AddLess != "+").Sum(x => (decimal?)x.Amt) ?? 0,
                                BalAmt = 0,
                                ChargesList = mchargelist,
                                Freight = ctxTFAT.Ledger.Where(X => X.Srl == item.FMNo && X.Branch == item.FmBran).FirstOrDefault() == null ? 0 : ctxTFAT.Ledger.Where(X => X.TableKey == item.FMTableKey && X.Branch == item.FmBran).Sum(x => (decimal?)x.Credit) ?? 0,
                                tempid = SelectedLedger.Count + 1,
                                RefTableKey = item.FMTableKey,
                                aSno = ctxTFAT.Outstanding.Where(x => x.TableKey == item.TableKey).Select(x => x.aSno).FirstOrDefault(),
                                AdvType = item.InsClr,
                                NetAmt = item.NetAmt == null ? 0 : item.NetAmt.Value,
                                ChgPickupList = mchargelist.Select(x => x.Amt).ToList(),
                                Remark = item.Remark,
                                PartyCode = item.Party,
                                RelatedToCode = item.RelatedTo,
                                VehicleNO = fMMaster == null ? "" : String.IsNullOrEmpty(ctxTFAT.VehicleMaster.Where(x => x.Code == fMMaster.TruckNo).Select(x => x.TruckNo).FirstOrDefault()) == true ? ctxTFAT.HireVehicleMaster.Where(x => x.Code == fMMaster.TruckNo).Select(x => x.TruckNo).FirstOrDefault() : ctxTFAT.VehicleMaster.Where(x => x.Code == fMMaster.TruckNo).Select(x => x.TruckNo).FirstOrDefault(),
                                DriverName = fMMaster == null ? "" : ctxTFAT.DriverMaster.Where(x => x.Code == fMMaster.Driver).Select(x => x.Name).FirstOrDefault(),
                                ChargeRef = ctxTFAT.Master.Where(x => x.Code == mFMROUVEL.ProjCode).Select(x => x.Name).FirstOrDefault(),

                            });
                        }
                        else
                        {
                            var mFMROUVELactual = ctxTFAT.FMVouRel.Where(X => X.FMNo == item.FMTableKey && X.Branch == item.FmBran).Select(x => x).FirstOrDefault();
                            var mchargelist = GetFMWiseCharges(Model.Srl, item.FMNo, item.FMTableKey);
                            decimal actualamount = (ctxTFAT.VoucherDetail.Where(x => x.FMTableKey == item.FMTableKey && x.FmBran == item.FmBran && x.ParentKey != Model.ParentKey && x.Branch != item.Branch).Sum(x => (decimal?)x.Amount) ?? 0);
                            decimal ClaimAmt = (ctxTFAT.VoucherDetail.Where(x => x.FMTableKey == item.FMTableKey && x.FmBran == item.FmBran && x.ParentKey != Model.ParentKey && x.Branch != item.Branch).Sum(x => (decimal?)x.Val5) ?? 0);
                            decimal TdsAmt = (ctxTFAT.VoucherDetail.Where(x => x.FMTableKey == item.FMTableKey && x.FmBran == item.FmBran && x.ParentKey != Model.ParentKey && x.Branch != item.Branch).Sum(x => (decimal?)x.Val6) ?? 0);
                            FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.FmNo == mFmNo).FirstOrDefault();

                            SelectedLedger.Add(new AdvancePayModel()
                            {
                                Branch = item.FmBran,
                                BranchName = ctxTFAT.TfatBranch.Where(x => x.Code == item.FmBran).Select(x => x.Name).FirstOrDefault(),
                                FMDateStr = "",
                                FMDate = fMMaster == null ? DateTime.Now : ctxTFAT.FMMaster.Where(x => x.FmNo == mFmNo).Select(x => x.Date).FirstOrDefault(),
                                FMNo = item.FMNo,
                                AdvPending = item.InsClr == "A" ? (mFMROUVELactual.Adv.Value - (actualamount + ClaimAmt + TdsAmt)) : (decimal)0,
                                BalPending = item.InsClr == "B" ? (mFMROUVELactual.Bal.Value - (actualamount + ClaimAmt + TdsAmt)) : (decimal)0,
                                Amt = item.Amount.Value,
                                AdvAmt = mchargelist.Where(x => x.AddLess != "+").Sum(x => (decimal?)x.Amt) ?? 0,
                                BalAmt = 0,
                                ChargesList = mchargelist,
                                Freight = mFMROUVELactual.Adv.Value + mFMROUVELactual.Bal.Value,
                                tempid = SelectedLedger.Count + 1,
                                RefTableKey = item.FMTableKey,
                                aSno = 0,
                                AdvType = item.InsClr,
                                NetAmt = item.NetAmt == null ? 0 : item.NetAmt.Value,
                                PrevAmt = item.Amount.Value,
                                ChgPickupList = mchargelist.Select(x => x.Amt).ToList(),
                                Remark = item.Remark,
                                PartyCode = item.Party,
                                RelatedToCode = item.RelatedTo,
                                VehicleNO = fMMaster == null ? "" : String.IsNullOrEmpty(ctxTFAT.VehicleMaster.Where(x => x.Code == fMMaster.TruckNo).Select(x => x.TruckNo).FirstOrDefault()) == true ? ctxTFAT.HireVehicleMaster.Where(x => x.Code == fMMaster.TruckNo).Select(x => x.TruckNo).FirstOrDefault() : ctxTFAT.VehicleMaster.Where(x => x.Code == fMMaster.TruckNo).Select(x => x.TruckNo).FirstOrDefault(),
                                DriverName = fMMaster == null ? "" : ctxTFAT.DriverMaster.Where(x => x.Code == fMMaster.Driver).Select(x => x.Name).FirstOrDefault(),
                                ChargeRef = "",
                            });
                        }
                    }
                }
                Model.SelectedLedger = SelectedLedger;
                Model.HeaderList = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.DontUse == false).Select(x => x.Head).ToList();
                Model.AddlessList = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.DontUse == false).Select(x => x.EqAmt).ToList();


                #region LRCost List

                var mVouchList = ctxTFAT.RelateData.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).ToList();
                List<AdvancePayModel> objledgerdetailC = new List<AdvancePayModel>();
                int mCnt = 1;
                foreach (var item in mVouchList)
                {
                    objledgerdetailC.Add(new AdvancePayModel()
                    {
                        Code = item.Code,
                        AccountName = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault(),
                        LRDetailList = GetLRDetailList(item.Branch + item.TableKey),
                        Debit = (item.AmtType == true) ? item.Amount.Value : 0,
                        Credit = (item.AmtType == false) ? item.Amount.Value : 0,
                        //Narr = item.Narr,
                        Amt = item.Amount.Value,
                        RelatedTo = item.Combo1,
                        TableKey = item.TableKey,
                        tempid = mCnt,
                        Narr = item.Narr,
                        PartialDivName = item.RelateTo.Value.ToString()
                    });
                    mCnt = mCnt + 1;
                }
                Session.Add("AothertrxlistAdv", objledgerdetailC);
                Model.Selectedleger = objledgerdetailC;
                Model.SumDebit = objledgerdetailC.Sum(x => x.Debit);
                Model.SumCredit = objledgerdetailC.Sum(x => x.Credit);
                Model.SumDebit = Math.Round((decimal)Model.SumDebit, 2);
                Model.SumCredit = Math.Round((decimal)Model.SumCredit, 2);

                #endregion
                //Model.LRDetailList = GetLRDetailList(Model.Branch + Model.ParentKey);

                #region GstGet
                var mLedger = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).OrderBy(x => x.Sno).FirstOrDefault();
                Model.TotalTaxable = (mLedger.Debit.Value + mLedger.Credit.Value);
                Model.IGSTAmt = mLedger.IGSTAmt == null ? 0 : (double)mLedger.IGSTAmt.Value;
                Model.CGSTAmt = mLedger.CGSTAmt == null ? 0 : (double)mLedger.CGSTAmt.Value;
                Model.SGSTAmt = mLedger.SGSTAmt == null ? 0 : (double)mLedger.SGSTAmt.Value;

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
                Model.NetAmt = SelectedLedger.Sum(x => x.NetAmt);

                var mVouchMaster1 = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch && x.Code == Model.Account && x.AltCode == Model.Bank).OrderBy(x => x.Sno).FirstOrDefault();
                Model.AccAmt = mVouchMaster1 == null ? 0 : mVouchMaster1.Debit.Value + mVouchMaster1.Credit.Value;
                Model.Taxable = Model.AccAmt;
                Model.InvoiceAmt = Math.Abs(Model.Taxable + (decimal)Model.IGSTAmt + (decimal)Model.CGSTAmt + (decimal)Model.SGSTAmt);

                #endregion

                #region TDS

                if (mLedger.TDSFlag)
                {

                    Model.TDSFlag = mLedger.TDSFlag;
                    Model.TDSCode = mLedger.TDSCode == null ? "" : mLedger.TDSCode.Value.ToString();
                    Model.TDSCodeName = ctxTFAT.TDSMaster.Where(x => x.Code.ToString() == Model.TDSCode).Select(x => x.Name).FirstOrDefault();
                    var TdsAC = mLedger.TDSCode.ToString();
                    TDSMaster tDSMaster = ctxTFAT.TDSMaster.Where(x => x.Code == mLedger.TDSCode).FirstOrDefault();
                    if (tDSMaster != null)
                    {
                        var Ledger1 = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch && x.Code.ToString() == tDSMaster.PostCode.ToString()).FirstOrDefault();
                        if (Ledger1 != null)
                        {
                            Model.TDSAmt = Convert.ToDouble(Ledger1.Debit.Value + Ledger1.Credit.Value);
                        }
                    }
                    //Model.TDSAmt = Math.Round((double)(ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch && x.Code == "000009994").Sum(x => (decimal?)x.Credit) ?? (decimal?)0));
                    if (Model.Taxable > 0)
                    {
                        Model.TDSRate = Math.Round(Math.Abs(((decimal)Model.TDSAmt * 100) / Model.Taxable));
                    }
                }

                #endregion

                decimal[] mCharges2 = new decimal[(Model.HeaderList.Count + 2)];

                for (int ai = 0; ai < Model.HeaderList.Count + 2; ai++)
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


                Session["Aadvancepaylist"] = SelectedLedger;
            }



            List<GridOption> Grlist = new List<GridOption>();
            var list = ctxTFAT.DocFormats.Where(x => x.Type == "FMP00").Select(x => x).ToList();
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

            Model.AllHeaderList = ctxTFAT.Charges.Where(x => x.Type == "FMP00").Select(x => x.Head).ToList();
            Model.AllAddlessList = ctxTFAT.Charges.Where(x => x.Type == "FMP00").Select(x => x.EqAmt).ToList();



            return View(Model);
        }

        #region ALL GET 

        public ActionResult GetLoanTrxTypes()
        {

            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "A", Text = "Advance" });
            GSt.Add(new SelectListItem { Value = "B", Text = "Balance" });

            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBankList(string term)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.Master.Where(x => (x.AppBranch.Contains(mbranchcode) || x.AppBranch == "All") && (x.BaseGr == "B" || x.BaseGr == "C") && x.Hide == false).Select(c => new { c.Code, c.Name }).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var result = ctxTFAT.Master.Where(x => x.Name.Contains(term) && (x.AppBranch.Contains(mbranchcode) || x.AppBranch == "All") && (x.BaseGr == "B" || x.BaseGr == "C") && x.Hide == false).Select(m => new { m.Code, m.Name }).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

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

        public ActionResult GetTDSRateDetail(AdvancePayModel Model)
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

        public ActionResult GetFMList(string AccountCode)
        {

            List<SelectListItem> branch = new List<SelectListItem>();
            //var mOtherType = ctxTFAT.Master.Where(x => x.Code == AccountCode).Select(x => x.OthPostType).FirstOrDefault();
            //if (mOtherType == "D")
            //{
            string mstr = "Select f.FMNo from FMVourel f where f.Postcode = '" + AccountCode + "' ";
            mstr = mstr + " Union All Select r.Srl as FMNo from Ledger r  where r.Type = 'FM000' and r.Code = '" + AccountCode + "'   and (r.Refdoc = 'A' or r.RefDoc = 'B') ";
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
                var result = ctxTFAT.Master.Where(x => (x.AppBranch.Contains(mbranchcode) || x.AppBranch == "All") && (x.OthPostType.Contains("V") || x.OthPostType.Contains("D") || x.OthPostType.Contains("B"))).Select(c => new { c.Code, c.Name }).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.Master.Where(x => x.Name.Contains(term) && (x.AppBranch.Contains(mbranchcode) || x.AppBranch == "All") && (x.OthPostType.Contains("V") || x.OthPostType.Contains("D") || x.OthPostType.Contains("B"))).Select(m => new { m.Code, m.Name }).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetAdvanceTypeList()
        {

            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "A", Text = "Advance" });
            GSt.Add(new SelectListItem { Value = "B", Text = "Balance" });

            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetPartyDetails(AdvancePayModel Model)
        {
            var CurrDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            var taxdetails = ctxTFAT.TaxDetails.Where(x => x.Code == Model.Account).Select(x => new { x.TDSCode, x.CutTDS, x.TDSRate }).FirstOrDefault();
            var CutTDS = taxdetails == null ? false : taxdetails.CutTDS;
            var TDSCode = taxdetails == null ? 0 : taxdetails.TDSCode == null ? 0 : taxdetails.TDSCode.Value;
            //var TDSRate = taxdetails == null ? 0 : taxdetails.TDSRate == null ? 0 : taxdetails.TDSRate.Value;
            decimal TDSRate = 0;
            TDSRate = ctxTFAT.TDSRates.Where(x => x.Code == TDSCode && x.EffDate <= CurrDate).OrderByDescending(x => x.EffDate).Select(x => x.TDSRate ?? 0).FirstOrDefault();
            Model.TDSRate = TDSRate;
            var TDSName = ctxTFAT.TDSMaster.Where(x => x.Code == TDSCode).Select(x => x.Name).FirstOrDefault();
            var TDSFlagSetup = ctxTFAT.AdvBalSetup.Select(x => x.CutTDS).FirstOrDefault();
            if (TDSFlagSetup == false)
            {
                CutTDS = false;
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
                CutTDS = CutTDS,
                TDSCode = TDSCode,
                TDSName = TDSName,
                TDSRate = TDSRate,
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region ADD LEDGER ITEM

        public ActionResult AddEditSelectedAdvance(AdvancePayModel Model)
        {
            if (Model.SessionFlag == "Add")
            {
                List<AdvancePayModel> objledgerdetail = new List<AdvancePayModel>();

                if (Session["Aadvancepaylist"] != null)
                {
                    objledgerdetail = (List<AdvancePayModel>)Session["Aadvancepaylist"];
                }

                List<AdvancePayModel> mCharges = new List<AdvancePayModel>();
                foreach (var chg in Model.ChargesList)
                {
                    mCharges.Add(new AdvancePayModel()
                    {
                        Amt = (chg.AddLess == "+") ? chg.Amt : -chg.Amt
                    });
                }

                objledgerdetail.Add(new AdvancePayModel()
                {
                    Branch = Model.Branch,
                    BranchName = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Branch).Select(x => x.Name).FirstOrDefault(),
                    FMDateStr = Model.FMDateStr,
                    FMDate = ConvertDDMMYYTOYYMMDD(Model.FMDateStr),
                    FMNo = Model.FMNo,
                    AdvPending = Model.AdvPending,
                    BalPending = Model.BalPending,
                    Amt = Model.Amt,
                    ChargesList = Model.ChargesList,
                    HeaderList = Model.ChargesList.Select(x => x.Header).ToList(),
                    ChgPickupList = Model.ChargesList.Select(x => x.Amt).ToList(),
                    tempid = objledgerdetail.Count + 1,
                    AdvAmt = Model.AdvAmt,
                    BalAmt = Model.BalAmt,
                    Freight = Model.Freight,
                    NetAmt = Model.Amt + ((mCharges != null && mCharges.Count > 0) ? mCharges.Sum(x => (decimal?)x.Amt) ?? 0 : 0),
                    Remark = Model.Remark
                });


                Session.Add("Aadvancepaylist", objledgerdetail);
                decimal sumAmt = objledgerdetail.Sum(x => x.NetAmt);

                Model.HeaderList = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.DontUse == false).Select(x => x.Head).ToList();
                Model.AddlessList = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.DontUse == false).Select(x => x.EqAmt).ToList();

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

                var html = ViewHelper.RenderPartialView(this, "LedgerList", new AdvancePayModel() { SelectedLedger = objledgerdetail, HeaderList = Model.HeaderList, TotalTaxable = Model.TotalTaxable, TotalChgPickupList = Model.TotalChgPickupList });
                return Json(new { Html = html, SumAmt = sumAmt }, JsonRequestBehavior.AllowGet);
            }
            else
            {


                var result = (List<AdvancePayModel>)Session["Aadvancepaylist"];
                foreach (var item in result.Where(x => x.tempid == Model.tempid))
                {
                    item.Branch = Model.Branch;
                    item.BranchName = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Branch).Select(x => x.Name).FirstOrDefault();
                    item.FMDateStr = Model.FMDateStr;
                    item.FMDate = ConvertDDMMYYTOYYMMDD(Model.FMDateStr);
                    item.FMNo = Model.FMNo;
                    //item.AdvPending = Model.AdvPending;
                    //item.BalPending = Model.BalPending;
                    item.Amt = Model.Amt;
                    item.ChargesList = Model.ChargesList;
                    item.HeaderList = Model.ChargesList.Select(x => x.Header).ToList();
                    item.ChgPickupList = Model.ChargesList.Select(x => x.Amt).ToList();
                    item.AdvAmt = Model.AdvAmt;
                    item.BalAmt = Model.BalAmt;
                    item.Amt = Model.Amt;
                    item.Freight = Model.Freight;
                    item.NetAmt = ((Model.ChargesList != null && Model.ChargesList.Count > 0) ? Model.ChargesList.Where(x => x.AddLess != "-" && x.Fld != "TDS").Sum(x => (decimal?)x.Amt) ?? 0 : 0);
                    item.NetAmt = item.NetAmt - ((Model.ChargesList != null && Model.ChargesList.Count > 0) ? Model.ChargesList.Where(x => x.Fld == "TDS").Sum(x => (decimal?)x.Amt) ?? 0 : 0);

                    item.Remark = Model.Remark;

                }


                Session.Add("Aadvancepaylist", result);
                decimal sumAmt = result.Sum(x => x.NetAmt);
                Model.HeaderList = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.DontUse == false).Select(x => x.Head).ToList();
                Model.AddlessList = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.DontUse == false).Select(x => x.EqAmt).ToList();

                Model.TotalTaxable = Model.AccAmt;
                decimal[] mCharges2 = new decimal[(Model.HeaderList.Count + 2)];

                for (int ai = 0; ai < Model.HeaderList.Count + 2; ai++)
                {
                    decimal mchgamt = 0;
                    foreach (var i in result)
                    {
                        mchgamt += i.ChgPickupList[ai];

                    }
                    mCharges2[ai] = mchgamt;
                }






                Model.TotalChgPickupList = mCharges2.ToList();
                var html = ViewHelper.RenderPartialView(this, "LedgerList", new AdvancePayModel() { AddlessList = Model.AddlessList, SelectedLedger = result, HeaderList = Model.HeaderList, TotalTaxable = Model.TotalTaxable, TotalChgPickupList = Model.TotalChgPickupList });
                return Json(new
                {
                    Selectedleger = result,
                    Html = html,
                    SumAmt = sumAmt,
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetAdvanceLedger(AdvancePayModel Model)
        {
            Model.OthPostType = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.OthPostType).FirstOrDefault();
            if (Model.SessionFlag == "Edit")
            {
                var result = (List<AdvancePayModel>)Session["Aadvancepaylist"];
                var result1 = result.Where(x => x.tempid == Model.tempid).ToList();
                foreach (var item in result1)
                {
                    Model.Branch = item.Branch;
                    Model.FMNo = item.FMNo;
                    Model.FMDate = item.FMDate;
                    Model.FMDateStr = item.FMDateStr;
                    Model.AdvPending = (item.AdvPending);
                    Model.BalPending = item.BalPending;
                    Model.Amt = item.Amt;
                    Model.BalAmt = item.BalAmt;
                    Model.AdvAmt = item.AdvAmt;
                    Model.Freight = item.Freight;
                    Model.AdvType = item.AdvType;
                    Model.NetAmt = item.NetAmt;
                    Model.Remark = item.Remark;
                    Model.Party = ctxTFAT.Master.Where(X => X.Code == item.PartyCode).Select(X => X.Name).FirstOrDefault();
                    Model.RelatedTo = ctxTFAT.Master.Where(X => X.Code == item.RelatedToCode).Select(X => X.Name).FirstOrDefault();

                    Model.Amt = item.ChargesList.Where(x => x.Fld == "Amt").Select(x => x.Amt).FirstOrDefault();
                    Model.BillTDS = item.ChargesList.Where(x => x.Fld == "TDS").Select(x => x.Amt).FirstOrDefault();

                    Model.AdvPending = (Model.AdvPending) - (item.ChargesList.Where(x => x.Fld == "Amt").Select(x => x.Amt).FirstOrDefault() + item.ChargesList.Where(x => x.AddLess == "-").Sum(x => x.Amt));
                    Model.BalPending = (Model.BalPending) - (item.ChargesList.Where(x => x.Fld == "Amt").Select(x => x.Amt).FirstOrDefault() + item.ChargesList.Where(x => x.AddLess == "-").Sum(x => x.Amt));


                    List<AdvancePayModel> newlist1 = new List<AdvancePayModel>();
                    var mCharges = (from C in ctxTFAT.Charges
                                    where C.Type == "FMP00" && C.DontUse == false

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
                        AdvancePayModel c = new AdvancePayModel();
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
                }
                var jsonResult = Json(new { Html = this.RenderPartialView("AddAdvancePayPopup", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            else
            {
                List<AdvancePayModel> newlist1 = new List<AdvancePayModel>();

                var CostCentre = (from C in ctxTFAT.Charges
                                  where C.Type == "FMP00" && C.DontUse == false

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
                    AdvancePayModel c = new AdvancePayModel();
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
        public ActionResult DeleteAdvanceLedger(AdvancePayModel Model)
        {
            var result2 = (List<AdvancePayModel>)Session["Aadvancepaylist"];

            var result = result2.Where(x => x.tempid != Model.tempid).ToList();

            decimal sumAmt = result.Sum(x => x.NetAmt);
            Model.HeaderList = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.DontUse == false).Select(x => x.Head).ToList();
            Model.AddlessList = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.DontUse == false).Select(x => x.EqAmt).ToList();

            Model.TotalTaxable = sumAmt;
            decimal[] mCharges2 = new decimal[(Model.HeaderList.Count + 2)];

            for (int ai = 0; ai < Model.HeaderList.Count + 2; ai++)
            {
                decimal mchgamt = 0;
                foreach (var i in result)
                {
                    mchgamt += i.ChgPickupList[ai];

                }
                mCharges2[ai] = mchgamt;
            }




            Model.TotalChgPickupList = mCharges2.ToList();
            Session["Aadvancepaylist"] = result;
            var html = ViewHelper.RenderPartialView(this, "LedgerList", new AdvancePayModel() { AddlessList = Model.AddlessList, SelectedLedger = result, HeaderList = Model.HeaderList, TotalTaxable = Model.TotalTaxable, TotalChgPickupList = Model.TotalChgPickupList });
            return Json(new
            {
                Html = html,
                SumAmt = sumAmt,
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFMDetailsFromPickup(AdvancePayModel Model)
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
                List<AdvancePayModel> newlist1 = new List<AdvancePayModel>();
                var mCharges = (from C in ctxTFAT.Charges
                                where C.Type == "FMP00" && C.DontUse == false

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
                    AdvancePayModel c = new AdvancePayModel();
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

        #region ADD LEDGER ITEM

        public ActionResult AddCashLedger(AdvancePayModel Model)
        {

            if (Model.SessionFlag == "Add")
            {
                List<AdvancePayModel> objledgerdetail = new List<AdvancePayModel>();

                List<AdvancePayModel> lrdetaillist = new List<AdvancePayModel>();


                if (Session["AothertrxlistAdv"] != null)
                {
                    objledgerdetail = (List<AdvancePayModel>)Session["AothertrxlistAdv"];
                }

                #region Add Default Narr 

                if (Model.LRDetailList != null && Model.LRDetailList.Count > 0)
                {
                    List<string> item3 = new List<string>();
                    item3 = Model.LRDetailList.Select(x => x.LRNumber + "-" + x.LRAmt).ToList();

                    string osadjnarr = String.Join(",", item3);
                    Model.Narr += osadjnarr;
                }

                #endregion



                objledgerdetail.Add(new AdvancePayModel()
                {
                    Code = Model.Code,
                    AccountName = Model.AccountName,
                    LRDetailList = Model.LRDetailList,
                    Debit = Model.LRDetailList.Sum(x => x.LRAmt),
                    Credit = 0,
                    Narr = Model.Narr,
                    Amt = Model.Amt,
                    tempid = objledgerdetail.Count + 1,
                    RelatedTo = "3",
                });

                // objledgerdetail = objledgerdetail.Distinct().ToList();

                Session.Add("AothertrxlistAdv", objledgerdetail);
                decimal sumdebit = objledgerdetail.Sum(x => x.Debit);
                decimal sumcredit = objledgerdetail.Sum(x => x.Credit);

                Session["ATempOthCashBkAttach"] = null;
                var html = ViewHelper.RenderPartialView(this, "LedgerListOFExpenses", new AdvancePayModel() { Selectedleger = objledgerdetail });
                var jsonResult = Json(new { Selectedleger = objledgerdetail, Html = html, Sumdebit = sumdebit, Sumcredit = sumcredit }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
            else
            {
                var objledgerdetail = (List<AdvancePayModel>)Session["AothertrxlistAdv"];

                foreach (var item in objledgerdetail.Where(x => x.tempid == Model.tempid))
                {
                    item.Code = Model.Code;
                    item.AccountName = Model.AccountName;
                    item.LRDetailList = Model.LRDetailList;
                    item.Debit = Model.LRDetailList.Sum(x => x.LRAmt);
                    item.Credit = 0;
                    item.Narr = Model.Narr;
                    item.Amt = Model.Amt;
                    item.RelatedTo = Model.RelatedTo;
                    item.Narr = Model.Narr;
                }
                Session.Add("AothertrxlistAdv", objledgerdetail);
                decimal sumdebit = objledgerdetail.Sum(x => x.Debit);
                decimal sumcredit = objledgerdetail.Sum(x => x.Credit);

                var html = ViewHelper.RenderPartialView(this, "LedgerListOFExpenses", new AdvancePayModel() { Selectedleger = objledgerdetail });
                var jsonResult = Json(new { Selectedleger = objledgerdetail, Html = html, Sumdebit = sumdebit, Sumcredit = sumcredit }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
        }

        [HttpPost]
        public ActionResult Deleteledger(AdvancePayModel Model)
        {
            var result = (List<AdvancePayModel>)Session["AothertrxlistAdv"];

            var result2 = result.Where(x => x.tempid != Model.tempid).Select(x => x).ToList();
            Session.Add("AothertrxlistAdv", result2);
            decimal sumdebit = result2.Sum(x => x.Debit);
            decimal sumcredit = result2.Sum(x => x.Credit);
            int i = 1;
            foreach (var item in result2)
            {
                item.tempid = i++;
            }
            var html = ViewHelper.RenderPartialView(this, "LedgerListOFExpenses", new AdvancePayModel() { Selectedleger = result2 });
            var jsonResult = Json(new
            {
                Selectedleger = result2,
                Html = html,
                Sumdebit = sumdebit,
                Sumcredit = sumcredit
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public ActionResult GetCashLedger(AdvancePayModel Model)
        {
            string Branch = "";

            if (Model.SessionFlag == "Edit")
            {
                var result = (List<AdvancePayModel>)Session["AothertrxlistAdv"];
                var result1 = result.Where(x => x.tempid == Model.tempid);



                foreach (var item in result1)
                {
                    //var Count = ctxTFAT.TyreStockSerial.Where(x => x.ParentKey == item.TableKey).ToList().Count();
                    Model.Code = item.Code;
                    Model.AccountName = item.AccountName;
                    Model.LRDetailList = item.LRDetailList;
                    Model.tempid = Model.tempid;
                    Model.Debit = Model.Amt;
                    Model.Credit = 0;
                    Model.Narr = item.Narr;
                    Model.Amt = item.Amt;
                    Model.RelatedTo = item.RelatedTo;
                    Model.TableKey = item.TableKey;

                    var mAcc = ctxTFAT.Master.Where(x => x.Code == Model.Code).Select(x => new { x.RelatedTo, x.Name }).FirstOrDefault();
                    string mName = "";
                    var mIsVehicle = ctxTFAT.Master.Where(x => x.Code == Model.Code).Select(x => x.OthPostType).FirstOrDefault();
                    Model.PartialDivName = mAcc.RelatedTo;
                }
                Model.Selectedleger = result;
                Model.Mode = "Edit";
            }
            else
            {
                Model.Mode = "Add";
                //Model.SetupReqRelatedAc = Setup.RelatedPosting;
            }
            if (Model.LRDetailList == null)
            {
                Model.LRDetailList = new List<AdvancePayModel>();
            }

            var jsonResult = Json(new { Branch = Branch, Html = this.RenderPartialView("AddEditTransaction", Model), Amt = Model.LRDetailList.Sum(x => x.LRAmt) }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;



        }

        public ActionResult GetAccountListExp(string term/*, string BaseGr*/)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.Master.Where(x => x.RelatedTo == "LR").Select(m => new
                {
                    m.Code,
                    m.Name
                }).OrderBy(n => n.Name).ToList();



                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.Master.Where(x => x.RelatedTo == "LR" && x.Name.Contains(term)).Select(m => new
                {
                    m.Code,
                    m.Name
                }).OrderBy(n => n.Name).ToList();





                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult RelatedToDecide(AdvancePayModel Model)
        {
            string PartialDivName = "", html = "";
            var mAcc = ctxTFAT.Master.Where(x => x.Code == Model.Code).Select(x => new { x.RelatedTo, x.Name }).FirstOrDefault();
            string mName = "";
            var mIsVehicle = ctxTFAT.Master.Where(x => x.Code == Model.Code).Select(x => x.OthPostType).FirstOrDefault();

            if (mAcc.RelatedTo == "0")
            {
                PartialDivName = mAcc.RelatedTo;// "OtherDetails";
            }
            else if (mAcc.RelatedTo == "LR")
            {
                PartialDivName = "3";// "LRDetails";
            }
            return Json(new { Html = html, PartialDivName = PartialDivName, Name = mName }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ConfirmRelationAddEdit(AdvancePayModel Model)
        {
            string mMessage = "";

            //Branch Validation

            Model.Status = "Success";
            Model.Message = "";
            return Json(Model, JsonRequestBehavior.AllowGet);

        }

        #endregion

        #region Pickup

        public List<AdvancePayModel> Add2ItemForPickup(List<DataRow> ordersstk, string query, List<string> ExistDoc)
        {
            double PendFactor = 0;
            List<AdvancePayModel> objitemlist = new List<AdvancePayModel>();
            int i = 1;
            foreach (var item in ordersstk)
            {
                AdvancePayModel objitemlist1 = new AdvancePayModel();


                objitemlist1.tempid = i;
                objitemlist1.FMNo = item["FMNo"].ToString();
                objitemlist1.FMDateStr = item["FMDateStr"].ToString();
                objitemlist1.FMDate = string.IsNullOrEmpty(item["FMDateStr"].ToString()) == true ? ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString()) : ConvertDDMMYYTOYYMMDD(item["FMDateStr"].ToString());
                objitemlist1.AdvAmt = (item["AdvType"].ToString() == "Advance") ? Convert.ToDecimal(item["AdvAmt"].ToString()) : Convert.ToDecimal(item["BalAmt"].ToString());
                objitemlist1.Freight = Convert.ToDecimal(item["Freight"].ToString());
                objitemlist1.AdvPending = (item["AdvType"].ToString() == "Advance") ? Convert.ToDecimal(item["AdvPending"].ToString()) : Convert.ToDecimal(item["BalPending"].ToString());
                objitemlist1.RecordKey = Convert.ToInt32(item["RecordKey"].ToString());
                objitemlist1.AdvType = item["AdvType"].ToString();
                objitemlist1.Branch = item["Branch"].ToString();
                objitemlist1.Party = item["Party"].ToString();
                objitemlist1.RelatedTo = item["RelatedTo"].ToString();
                objitemlist1.RefTableKey = item["RefTableKey"].ToString();
                objitemlist1.aSno = Convert.ToInt32(item["aSno"].ToString());
                objitemlist1.ChgPickupList = GetAdvChargesPickup(query, item["RefTableKey"].ToString());
                objitemlist1.AdjustedAmt = Convert.ToDecimal(item["AdjustedAmt"].ToString());
                objitemlist1.RelatedToCode = item["RelatedToCode"].ToString();
                objitemlist1.PartyCode = item["PartyCode"].ToString();
                objitemlist1.FromTable = item["FromTable"].ToString();
                objitemlist1.VehicleNO = item["VehicleNo"].ToString();
                objitemlist1.DriverName = item["DriverName"].ToString();
                objitemlist1.ChargeRef = item["ChargeRef"].ToString();
                if (!ExistDoc.Contains(objitemlist1.FMNo))
                {
                    objitemlist.Add(objitemlist1);
                    i = i + 1;

                }


            }

            return objitemlist;
        }

        public ActionResult GetPickUp(AdvancePayModel Model)
        {
            string mstr = "";
            string abc = "";
            var mOtherPostType = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.RelatedTo).FirstOrDefault();
            Model.OthPostType = mOtherPostType;

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            var mcharges = ctxTFAT.Charges.Where(x => x.Type == "FMP00").Select(x => x.Head).ToArray();
            var mcharges1 = ctxTFAT.Charges.Where(x => x.Type == "FMP00").Select(x => x.EqAmt).ToArray();
            var html = ViewHelper.RenderPartialView(this, "PickUp", new AdvancePayModel() { AllHeaderList = mcharges.ToList(), AllAddlessList = mcharges1.ToList(), OthPostType = Model.OthPostType });
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public ActionResult PostPickUp(AdvancePayModel Model)
        {
            try
            {
                List<AdvancePayModel> objitemlist = new List<AdvancePayModel>();

                List<string> mtablekeys = new List<string>();

                if (Session["Aadvancepaylist"] != null)
                {
                    objitemlist = (List<AdvancePayModel>)Session["Aadvancepaylist"];
                }

                int mMaxtempid = (objitemlist.Count == 0) ? 0 : objitemlist.Select(x => x.tempid).Max();

                foreach (var c in Model.PickupList.OrderBy(x => x.tempid))
                {
                    mMaxtempid = mMaxtempid + 1;
                    Model.ChargesList = GetChargesListPickUp(c, Model.Type);
                    c.AdvType = (c.AdvType == "Advance") ? "A" : "B";

                    foreach (var item in Model.ChargesList)
                    {
                        item.Amt = item.Amt < 0 ? (-1) * item.Amt : item.Amt;
                    }

                    decimal NetAmtOfCurrentFM = 0;

                    if (Model.ChargesList != null && Model.ChargesList.Count() > 0)
                    {
                        NetAmtOfCurrentFM = (Model.ChargesList.Where(x => x.AddLess != "-" && x.Fld != "TDS").Sum(x => (decimal?)x.Amt) ?? 0) - (Model.ChargesList.Where(x => x.Fld == "TDS").Sum(x => (decimal?)x.Amt) ?? 0);
                    }

                    objitemlist.Add(new AdvancePayModel()
                    {
                        FMNo = c.FMNo,
                        FMDateStr = c.FMDateStr,
                        Branch = ctxTFAT.TfatBranch.Where(x => x.Name == c.Branch).Select(x => x.Code).FirstOrDefault(),
                        BranchName = c.Branch,
                        FMDate = ConvertDDMMYYTOYYMMDD(c.FMDateStr),
                        AdvAmt = (c.AdvType == "A") ? c.AdvAmt : 0,
                        AdvPending = (c.AdvType == "A") ? c.AdvPending : 0,
                        BalPending = (c.AdvType == "A") ? 0 : c.AdvPending,
                        Amt = c.Amt,
                        ChargesList = Model.ChargesList,
                        BalAmt = (c.AdvType == "A") ? 0 : c.BalAmt,
                        AdvType = c.AdvType,
                        Freight = c.Freight,
                        tempid = mMaxtempid,
                        //NetAmt = (c.Amt) + ((Model.ChargesList != null && Model.ChargesList.Count > 0) ? Model.ChargesList.Sum(x => (decimal?)x.Amt) ?? 0 : 0),
                        NetAmt = (c.Amt) + NetAmtOfCurrentFM,
                        RefTableKey = c.RefTableKey,
                        aSno = c.aSno,
                        HeaderList = Model.ChargesList.Select(x => x.Header).ToList(),
                        ChgPickupList = Model.ChargesList.Select(x => x.Amt).ToList(),
                        Remark = c.Remark,
                        RelatedTo = c.RelatedTo,
                        Party = c.Party,
                        PartyCode = c.PartyCode,
                        RelatedToCode = c.RelatedToCode,
                        FromTable = c.FromTable,
                        VehicleNO = c.VehicleNO,
                        DriverName = c.DriverName,
                        ChargeRef = c.ChargeRef,
                        FMRefParentkey = c.FMRefParentkey.Trim(),
                    });


                }
                decimal SumAmt = Model.AccAmt;
                decimal NetAmt = objitemlist.Sum(x => x.NetAmt);

                Session.Add("Aadvancepaylist", objitemlist);

                Model.SelectedLedger = objitemlist;

                Model.HeaderList = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.DontUse == false).Select(x => x.Head).ToList();
                Model.AddlessList = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.DontUse == false).Select(x => x.EqAmt).ToList();

                decimal[] mCharges2 = new decimal[(Model.HeaderList.Count + 2)];

                for (int ai = 0; ai < (Model.HeaderList.Count + 2); ai++)
                {
                    decimal mchgamt = 0;
                    foreach (var i in objitemlist)
                    {
                        mchgamt += i.ChgPickupList[ai];

                    }
                    mCharges2[ai] = mchgamt;
                }




                Model.TotalChgPickupList = mCharges2.ToList();


                decimal TdsAmt = 0, IGST = 0, SGST = 0, CGST = 0;
                if (Model.TDSFlag)
                {
                    TdsAmt = (Model.AccAmt * Model.TDSRate) / 100;
                }
                if (Model.GSTFlag)
                {
                    IGST = (Model.AccAmt * Model.IGSTRate) / 100;
                    SGST = (Model.AccAmt * Model.SGSTRate) / 100;
                    CGST = (Model.AccAmt * Model.CGSTRate) / 100;
                }
                var InvAmt = Model.AccAmt + IGST + SGST + CGST;


                string html = ViewHelper.RenderPartialView(this, "LedgerList", Model);

                var jsonResult = Json(new
                {
                    IGST = IGST,
                    SGST = SGST,
                    CGST = CGST,
                    InvAmt = InvAmt,
                    TdsAmt = TdsAmt,
                    SumAmt = SumAmt,
                    NetAmt = NetAmt,
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

        public List<AdvancePayModel> GetChargesListPickUp(AdvancePayModel Model, string Type)
        {
            List<AdvancePayModel> newlist1 = new List<AdvancePayModel>();
            var mCharges = (from C in ctxTFAT.Charges
                            where C.Type == "FMP00" && C.DontUse == false

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
            AdvancePayModel c1 = new AdvancePayModel();
            c1.Fld = "Amt";
            c1.Header = "";
            c1.AddLess = "";
            c1.FMNo = Model.FMNo;
            c1.AdvType = Model.AdvType;
            c1.tempid = Model.tempid;
            c1.PostCode = "";
            //c.Amt = (mStrArray[a] == null) ? (decimal)0 : Convert.ToDecimal(mStrArray[a]);
            c1.Amt = (decimal)mStrArray[a];
            newlist1.Add(c1);

            a = a + 1;
            AdvancePayModel c2 = new AdvancePayModel();
            c2.Fld = "TDS";
            c2.Header = "";
            c2.AddLess = "";
            c2.FMNo = Model.FMNo;
            c2.AdvType = Model.AdvType;
            c2.tempid = Model.tempid;
            c2.PostCode = "";
            //c.Amt = (mStrArray[a] == null) ? (decimal)0 : Convert.ToDecimal(mStrArray[a]);
            c2.Amt = -((decimal)mStrArray[a]);
            newlist1.Add(c2);

            a = a + 1;



            foreach (var i in mCharges)
            {
                AdvancePayModel c = new AdvancePayModel();
                c.Fld = i.Fld;
                c.Header = i.Head;
                c.AddLess = i.EqAmt;
                c.FMNo = Model.FMNo;
                c.AdvType = Model.AdvType;
                c.tempid = Model.tempid;
                c.PostCode = i.Code;
                //c.Amt = (mStrArray[a] == null) ? (decimal)0 : Convert.ToDecimal(mStrArray[a]);
                c.Amt = (i.EqAmt != "-") ? (decimal)mStrArray[a] : -((decimal)mStrArray[a]);
                newlist1.Add(c);

                a = a + 1;
            }


            return newlist1;
        }

        [HttpPost]
        public ActionResult PickUpCalCulation(AdvancePayModel Model)
        {
            List<AdvancePayModel> mCharges = new List<AdvancePayModel>();
            List<decimal> ChargesSummaryFldWise = new List<decimal>();
            var mNetAmt = (decimal)0;
            string Message = "", Status = "success", TempId = "";

            List<AdvancePayModel> mCharges2 = new List<AdvancePayModel>();

            if (Model.PickupList != null)
            {
                foreach (var a in Model.PickupList)
                {
                    mCharges = GetChargesListPickUp(a, "FM");

                    mCharges2.AddRange(mCharges);

                }
            }


            if (Model.PickupList != null)
            {

                #region NetAmout

                var mAmt = mCharges2.Sum(x => (decimal)x.Amt);
                mNetAmt += mAmt;

                #endregion

                #region Set Labels Amount

                //Default COlumns
                ChargesSummaryFldWise.Add(mCharges2.Where(x => x.Fld == "Amt").Sum(x => (decimal)x.Amt));
                ChargesSummaryFldWise.Add(mCharges2.Where(x => x.Fld == "TDS").Sum(x => (decimal)x.Amt));

                foreach (var item in ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.DontUse == false).ToList())
                {
                    //ChargesSummaryFldWise.Add(mCharges2.Where(x => x.Fld == item.Fld && !ExcludeFmno.Contains(x.tempid.ToString())).Sum(x => (decimal)x.Amt));
                    ChargesSummaryFldWise.Add(mCharges2.Where(x => x.Fld == item.Fld).Sum(x => (decimal)x.Amt));
                }
                #endregion

            }




            string LabelAmountSummary = "";
            foreach (var item in ChargesSummaryFldWise)
            {
                LabelAmountSummary += item + ",";
            }
            if (!String.IsNullOrEmpty(LabelAmountSummary))
            {
                LabelAmountSummary = LabelAmountSummary.Substring(0, LabelAmountSummary.Length - 1);
            }

            return Json(new
            {
                mNetAmt = mNetAmt,
                LabelAmountSummary = LabelAmountSummary,
                Message = Message,
                TempId = TempId
            }, JsonRequestBehavior.AllowGet);


        }

        public List<decimal> GetAdvChargesPickup(string abc, string TableKey)
        {
            List<decimal> newlist1 = new List<decimal>();
            //string mquery = (abc != "") ? ("Select " + abc + " from Ledger r WHERE r.TABLEKEY = '" + TableKey + "'") : abc;
            //DataTable chargeslist = GetDataTable(mquery);
            var mCharges = (from C in ctxTFAT.Charges
                            where C.Type == "FMP00" && C.DontUse == false

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
                decimal FLD = Convert.ToDecimal("0.00");
                newlist1.Add(FLD);
            }

            return newlist1;
        }

        [HttpPost]
        public ActionResult PushPickupChargeList(AdvancePayModel Model)
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

        public ActionResult GetPostingNew(AdvancePayModel Model)
        {

            List<AdvancePayModel> LedPostList = new List<AdvancePayModel>();
            int mCnt = 1;
            var objledgerdetail = (List<AdvancePayModel>)Session["Aadvancepaylist"];

            #region Validation 

            if (objledgerdetail == null || objledgerdetail.Count() == 0)
            {
                return Json(new
                {
                    Status = "ErrorValid",
                    Message = "No FM list Found Cant Save"
                }, JsonRequestBehavior.AllowGet); ;
            }

            var Date = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.Type == "FMP00" && x.LockDate == Date).FirstOrDefault() != null)
            {
                return Json(new
                {
                    Status = "ErrorValid",
                    Message = "Date is Locked By Period Lock System...!"
                }, JsonRequestBehavior.AllowGet); ;
            }

            if (!(ConvertDDMMYYTOYYMMDD(StartDate) <= Date && Date <= ConvertDDMMYYTOYYMMDD(EndDate)))
            {
                return Json(new
                {
                    Status = "ErrorValid",
                    Message = "Financial Date Range Allow Only...!"
                }, JsonRequestBehavior.AllowGet); ;
            }
            var LRNos = objledgerdetail.Select(x => x.FMNo).ToList();

            string Status2 = "Success", Message2 = "";
            var Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where (AlertMater.Type == "FM000" || AlertMater.Type == "FMH00") && LRNos.Contains(AlertMater.TypeCode) && AlertMater.RefType.Contains("FMP00")
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
                    if (stp.Trim().Contains("FMP00"))
                    {
                        Status2 = "Error";
                        Message2 += item.TypeCode + " Not Allowed In Payment.Lock As Per The AlertNote Rule. Please Remove IT....\n";
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

            #endregion

            #region Merge All Charges

            List<AdvancePayModel> mPostChargeList = new List<AdvancePayModel>();
            //charges ledger posting
            foreach (var a in objledgerdetail)
            {
                mPostChargeList.AddRange(a.ChargesList);
            }

            var mTotalChargeList = mPostChargeList.GroupBy(x => x.Fld).Select(x => new
            {
                FLD = x.Select(x1 => x1.Fld).FirstOrDefault(),
                Amt = x.Sum(x1 => x1.Amt),
                PostCode = x.Select(x1 => x1.PostCode).FirstOrDefault(),
                Addless = x.Select(x1 => x1.AddLess).FirstOrDefault(),
            }).ToList();
            //var ExtraAmountPaid = mTotalChargeList.Where(x => x.Addless != "-" && x.FLD != "F001").Sum(x => x.Amt);

            #endregion

            #region First Default Entry Of FMP00

            LedPostList.Add(new AdvancePayModel()
            {
                Code = Model.Account,
                AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                Debit = Math.Round((decimal)Model.AccAmt, 2),
                Credit = Math.Round((decimal)0, 2),
                Branch = mbranchcode,
                tempid = mCnt,
                PostCode = Model.Bank,
            });
            mCnt = mCnt + 1;
            LedPostList.Add(new AdvancePayModel()
            {
                Code = Model.Bank,
                AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Bank).Select(x => x.Name).FirstOrDefault(),
                Debit = Math.Round((decimal)0, 2),
                Credit = Math.Round((decimal)Model.AccAmt, 2),
                Branch = mbranchcode,
                tempid = mCnt,
                PostCode = Model.Account,
            });
            #endregion

            #region Charges Wise Ledger Posting

            foreach (var item in mTotalChargeList.Where(x => x.Amt != 0).ToList())
            {
                var PostingAllow = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.Fld.Trim() == item.FLD.Trim()).Select(x => x.Post).FirstOrDefault();
                if (PostingAllow)
                {
                    if (item.Addless == "+")
                    {
                        #region Broker Ko Extra Cash Diya He 
                        //mCnt = mCnt + 1;
                        //LedPostList.Add(new AdvancePayModel()
                        //{
                        //    Code = Model.Account,
                        //    AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                        //    Debit = Math.Round(item.Amt, 2),
                        //    Credit = Math.Round((decimal)0, 2),
                        //    Branch = mbranchcode,
                        //    tempid = mCnt,
                        //    PostCode = Model.Bank,
                        //});
                        //mCnt = mCnt + 1;
                        //LedPostList.Add(new AdvancePayModel()
                        //{
                        //    Code = Model.Bank,
                        //    AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Bank).Select(x => x.Name).FirstOrDefault(),
                        //    Debit = Math.Round((decimal)0, 2),
                        //    Credit = Math.Round(item.Amt, 2),
                        //    Branch = mbranchcode,
                        //    tempid = mCnt,
                        //    PostCode = Model.Account,
                        //});
                        #endregion

                        #region Jo Extra Cash Diya He uska Extra Ledger Posting
                        mCnt = mCnt + 1;
                        LedPostList.Add(new AdvancePayModel()
                        {
                            Code = item.PostCode,
                            AccountName = ctxTFAT.Master.Where(x => x.Code == item.PostCode).Select(x => x.Name).FirstOrDefault(),
                            Debit = Math.Round(item.Amt, 2),
                            Credit = Math.Round((decimal)0, 2),
                            Branch = mbranchcode,
                            tempid = mCnt,
                            PostCode = Model.Account,
                        });
                        mCnt = mCnt + 1;
                        LedPostList.Add(new AdvancePayModel()
                        {
                            Code = Model.Account,
                            AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                            Credit = Math.Round(item.Amt, 2),
                            Debit = Math.Round((decimal)0, 2),
                            Branch = mbranchcode,
                            tempid = mCnt,
                            PostCode = item.PostCode,
                        });
                        #endregion
                    }
                    else
                    {
                        mCnt = mCnt + 1;
                        LedPostList.Add(new AdvancePayModel()
                        {
                            Code = Model.Account,
                            AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                            Credit = Math.Round((decimal)0, 2),
                            Debit = Math.Round(item.Amt, 2),
                            Branch = mbranchcode,
                            tempid = mCnt,
                            PostCode = item.PostCode,
                        });
                        mCnt = mCnt + 1;
                        LedPostList.Add(new AdvancePayModel()
                        {
                            Code = item.PostCode,
                            AccountName = ctxTFAT.Master.Where(x => x.Code == item.PostCode).Select(x => x.Name).FirstOrDefault(),
                            Debit = Math.Round((decimal)0, 2),
                            Credit = Math.Round(item.Amt, 2),
                            Branch = mbranchcode,
                            tempid = mCnt,
                            PostCode = Model.Account,
                        });
                    }
                }
            }

            #endregion

            #region TDS Posting

            if (Model.TDSFlag == true && Model.TDSAmt > 0)
            {
                //TDSMaster tDSMaster = ctxTFAT.TDSMaster.Where(x => x.Code.ToString().Trim() == Model.TDSCode.Trim()).FirstOrDefault();
                //if (tDSMaster != null)
                {
                    mCnt = mCnt + 1;
                    LedPostList.Add(new AdvancePayModel()
                    {
                        Code = Model.Account,
                        AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                        Credit = Math.Round((decimal)0, 2),
                        Debit = Math.Round((decimal)Model.TDSAmt, 2),
                        Branch = mbranchcode,
                        tempid = mCnt,
                        PostCode = "000009994",
                    });
                    mCnt = mCnt + 1;
                    LedPostList.Add(new AdvancePayModel()
                    {
                        Code = "000009994",
                        AccountName = ctxTFAT.Master.Where(x => x.Code == "000009994").Select(x => x.Name).FirstOrDefault(),
                        Debit = Math.Round((decimal)0, 2),
                        Credit = Math.Round((decimal)Model.TDSAmt, 2),
                        Branch = mbranchcode,
                        tempid = mCnt,
                        PostCode = Model.Account,
                    });
                }

            }

            #endregion

            #region GST Posting

            if (Model.GSTFlag)
            {
                TaxMaster taxMaster = ctxTFAT.TaxMaster.Where(x => x.Code == Model.GSTCode).FirstOrDefault();
                if (taxMaster != null)
                {
                    string SGSTInputAccount = "000100032", CGSTInputAccount = "000100033", IGSTInputAccount = "000100034";
                    if (Model.IGSTAmt > 0)
                    {
                        mCnt = mCnt + 1;
                        LedPostList.Add(new AdvancePayModel()
                        {
                            Code = Model.Account,
                            AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                            Credit = Math.Round((decimal)0, 2),
                            Debit = Math.Round((decimal)Model.IGSTAmt, 2),
                            Branch = mbranchcode,
                            tempid = mCnt,
                            PostCode = IGSTInputAccount,
                        });
                        mCnt = mCnt + 1;
                        LedPostList.Add(new AdvancePayModel()
                        {
                            Code = IGSTInputAccount,
                            AccountName = ctxTFAT.Master.Where(x => x.Code == IGSTInputAccount).Select(x => x.Name).FirstOrDefault(),
                            Debit = Math.Round((decimal)0, 2),
                            Credit = Math.Round((decimal)Model.IGSTAmt, 2),
                            Branch = mbranchcode,
                            tempid = mCnt,
                            PostCode = Model.Account,
                        });

                    }
                    if (Model.SGSTAmt > 0)
                    {
                        mCnt = mCnt + 1;
                        LedPostList.Add(new AdvancePayModel()
                        {
                            Code = Model.Account,
                            AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                            Credit = Math.Round((decimal)0, 2),
                            Debit = Math.Round((decimal)Model.SGSTAmt, 2),
                            Branch = mbranchcode,
                            tempid = mCnt,
                            PostCode = SGSTInputAccount,
                        });
                        mCnt = mCnt + 1;
                        LedPostList.Add(new AdvancePayModel()
                        {
                            Code = SGSTInputAccount,
                            AccountName = ctxTFAT.Master.Where(x => x.Code == SGSTInputAccount).Select(x => x.Name).FirstOrDefault(),
                            Debit = Math.Round((decimal)0, 2),
                            Credit = Math.Round((decimal)Model.SGSTAmt, 2),
                            Branch = mbranchcode,
                            tempid = mCnt,
                            PostCode = Model.Account,
                        });

                    }
                    if (Model.CGSTAmt > 0)
                    {
                        mCnt = mCnt + 1;
                        LedPostList.Add(new AdvancePayModel()
                        {
                            Code = Model.Account,
                            AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                            Credit = Math.Round((decimal)0, 2),
                            Debit = Math.Round((decimal)Model.CGSTAmt, 2),
                            Branch = mbranchcode,
                            tempid = mCnt,
                            PostCode = CGSTInputAccount,
                        });
                        mCnt = mCnt + 1;
                        LedPostList.Add(new AdvancePayModel()
                        {
                            Code = CGSTInputAccount,
                            AccountName = ctxTFAT.Master.Where(x => x.Code == CGSTInputAccount).Select(x => x.Name).FirstOrDefault(),
                            Debit = Math.Round((decimal)0, 2),
                            Credit = Math.Round((decimal)Model.CGSTAmt, 2),
                            Branch = mbranchcode,
                            tempid = mCnt,
                            PostCode = Model.Account,
                        });
                    }
                }
                else
                {
                    HSNMaster hSNMaster = ctxTFAT.HSNMaster.Where(x => x.Code == Model.GSTCode).FirstOrDefault();
                    if (hSNMaster != null)
                    {
                        if (Model.IGSTAmt > 0)
                        {
                            mCnt = mCnt + 1;
                            LedPostList.Add(new AdvancePayModel()
                            {
                                Code = Model.Account,
                                AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                                Credit = Math.Round((decimal)0, 2),
                                Debit = Math.Round((decimal)Model.IGSTAmt, 2),
                                Branch = mbranchcode,
                                tempid = mCnt,
                                PostCode = hSNMaster.IGSTIn,
                            });
                            mCnt = mCnt + 1;
                            LedPostList.Add(new AdvancePayModel()
                            {
                                Code = hSNMaster.IGSTIn,
                                AccountName = ctxTFAT.Master.Where(x => x.Code == hSNMaster.IGSTIn).Select(x => x.Name).FirstOrDefault(),
                                Debit = Math.Round((decimal)0, 2),
                                Credit = Math.Round((decimal)Model.IGSTAmt, 2),
                                Branch = mbranchcode,
                                tempid = mCnt,
                                PostCode = Model.Account,
                            });
                        }
                        if (Model.SGSTAmt > 0)
                        {
                            mCnt = mCnt + 1;
                            LedPostList.Add(new AdvancePayModel()
                            {
                                Code = Model.Account,
                                AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                                Credit = Math.Round((decimal)0, 2),
                                Debit = Math.Round((decimal)Model.SGSTAmt, 2),
                                Branch = mbranchcode,
                                tempid = mCnt,
                                PostCode = hSNMaster.SGSTIn,
                            });
                            mCnt = mCnt + 1;
                            LedPostList.Add(new AdvancePayModel()
                            {
                                Code = hSNMaster.SGSTIn,
                                AccountName = ctxTFAT.Master.Where(x => x.Code == hSNMaster.SGSTIn).Select(x => x.Name).FirstOrDefault(),
                                Debit = Math.Round((decimal)0, 2),
                                Credit = Math.Round((decimal)Model.SGSTAmt, 2),
                                Branch = mbranchcode,
                                tempid = mCnt,
                                PostCode = Model.Account,
                            });
                        }
                        if (Model.CGSTAmt > 0)
                        {
                            mCnt = mCnt + 1;
                            LedPostList.Add(new AdvancePayModel()
                            {
                                Code = Model.Account,
                                AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                                Credit = Math.Round((decimal)0, 2),
                                Debit = Math.Round((decimal)Model.CGSTAmt, 2),
                                Branch = mbranchcode,
                                tempid = mCnt,
                                PostCode = hSNMaster.CGSTIn,
                            });
                            mCnt = mCnt + 1;
                            LedPostList.Add(new AdvancePayModel()
                            {
                                Code = hSNMaster.CGSTIn,
                                AccountName = ctxTFAT.Master.Where(x => x.Code == hSNMaster.CGSTIn).Select(x => x.Name).FirstOrDefault(),
                                Debit = Math.Round((decimal)0, 2),
                                Credit = Math.Round((decimal)Model.CGSTAmt, 2),
                                Branch = mbranchcode,
                                tempid = mCnt,
                                PostCode = Model.Account,
                            });
                        }
                    }
                }
            }

            #endregion



            Model.TotDebit = LedPostList.Sum(x => x.Debit);
            Model.TotCredit = LedPostList.Sum(x => x.Credit);
            Model.LedgerPostList = LedPostList;

            // display view form
            var html = ViewHelper.RenderPartialView(this, "LedgerPosting", new AdvancePayModel() { LedgerPostList = Model.LedgerPostList, TotDebit = Math.Round(Model.TotDebit, 2), TotCredit = Math.Round(Model.TotCredit, 2), Mode = Model.Mode, Controller2 = Model.Controller2, Type = Model.Type, Module = Model.Module, ViewDataId = Model.ViewDataId, TableName = Model.TableName, Header = Model.Header, OptionType = Model.OptionType, OptionCode = Model.OptionCode, MainType = "PM", SubType = Model.SubType });
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

        public ActionResult DeleteData(AdvancePayModel Model)
        {
            var Date = ConvertDDMMYYTOYYMMDD(Model.DocuDate);

            if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.Type == "FMP00" && x.LockDate == Date).FirstOrDefault() != null)
            {
                return Json(new
                {

                    Status = "Error",
                    Message = "Date is Locked By Period Lock System...!"
                }, JsonRequestBehavior.AllowGet); ;
            }

            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    Model.OthPostType = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.OthPostType).FirstOrDefault();

                    var mDeleteVoucherMas = ctxTFAT.VoucherMaster.Where(x => x.TableKey == Model.ParentKey && x.Branch == Model.Branch).FirstOrDefault();
                    DeUpdate(Model);
                    //UpdateAuditTrail(mbranchcode, Model.ChangeLog, Model.Header, Model.Srl, DateTime.Now, 0, Model.Srl, "");
                    //SendTrnsMsg("Delete", 0, mbranchcode + Model.Srl, ConvertDDMMYYTOYYMMDD(Model.DocuDate), "");

                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, Model.ParentKey, mDeleteVoucherMas.VouDate, Model.Amt, mDeleteVoucherMas.Bank, "Delete Adv && Bal Paid Document:" + Model.Srl, "A");

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

        private void DeUpdate(AdvancePayModel Model)
        {

            var mPayLedger = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).Select(x => x).ToList();
            ctxTFAT.Ledger.RemoveRange(mPayLedger);

            var mrelatedata = ctxTFAT.RelateData.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).Select(x => x).ToList();
            ctxTFAT.RelateData.RemoveRange(mrelatedata);

            var mrelatedata1 = ctxTFAT.RelLr.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).Select(x => x).ToList();
            ctxTFAT.RelLr.RemoveRange(mrelatedata1);




            var mDeleteVoucherDet = ctxTFAT.VoucherDetail.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).ToList();
            var mOutstanding = ctxTFAT.Outstanding.Where(x => x.ParentKey == Model.ParentKey && x.DocBranch == Model.Branch).ToList();
            ctxTFAT.Outstanding.RemoveRange(mOutstanding);

            ctxTFAT.VoucherDetail.RemoveRange(mDeleteVoucherDet);

            var mDeleteVoucherMas = ctxTFAT.VoucherMaster.Where(x => x.TableKey == Model.ParentKey && x.Branch == Model.Branch).FirstOrDefault();
            ctxTFAT.VoucherMaster.Remove(mDeleteVoucherMas);


            var objledgerdetail = (List<AdvancePayModel>)Session["Aadvancepaylist"];

            var AccountType = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.RelatedTo).FirstOrDefault();

            //if (AccountType != "6")
            //{
            //    foreach (var i in objledgerdetail)
            //    {
            //        string mfmno = i.FMNo;
            //        var mFmRouRel = ctxTFAT.FMVouRel.Where(x => x.FMNo == mfmno && x.Type == true && x.Pay == i.AdvType).Select(x => x).FirstOrDefault();
            //        if (mFmRouRel != null)
            //        {
            //            if (i.AdvType == "A")
            //            {
            //                mFmRouRel.AdvPen = mFmRouRel.AdvPen + i.PrevAmt;
            //            }
            //            else
            //            {
            //                mFmRouRel.BalPen = mFmRouRel.BalPen + i.PrevAmt;
            //            }
            //            ctxTFAT.Entry(mFmRouRel).State = EntityState.Modified;
            //        }


            //    }
            //}




            ctxTFAT.SaveChanges();
        }

        public ActionResult SaveData(AdvancePayModel Model)
        {
            var result = (List<AdvancePayModel>)Session["Aadvancepaylist"];
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
            Model.OthPostType = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.RelatedTo).FirstOrDefault();


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

                    Model.Srl = (Model.Mode == "Edit") ? Model.Srl : GetLastSerial("Ledger", Model.Branch, Model.Type, mperiod, Model.SubType, DateTime.Now.Date);
                    Model.ParentKey = mbranchcode + Model.Type + mperiod.Substring(0, 2) + Model.Srl;

                    #region Authorisation
                    TfatUserAuditHeader authorisation = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == "FMP00").FirstOrDefault();
                    if (authorisation != null)
                    {
                        mauthorise = SetAuthorisationLogistics(authorisation, Model.ParentKey, Model.Srl.ToString(), 0, Model.DocuDate, Model.AccAmt, Model.Account, mbranchcode);
                    }
                    #endregion


                    #region Save VoucherMaster
                    VoucherMaster mMas = new VoucherMaster();
                    mMas.VouNo = Model.Srl;
                    mMas.Amount = Model.AccAmt;
                    mMas.Bank = Model.Bank;
                    mMas.Branch = Model.Branch;
                    mMas.ChqNo = Model.ChequeNo;
                    mMas.PayMode = (byte)1;
                    mMas.Remark = Model.Remark;
                    mMas.Type = Model.Type;
                    mMas.VouDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                    mMas.ENTEREDBY = muserid;
                    mMas.LASTUPDATEDATE = DateTime.Now;
                    mMas.AUTHORISE = mauthorise;
                    mMas.AUTHIDS = muserid;
                    mMas.Account = Model.Account;
                    mMas.Prefix = mperiod;
                    mMas.TableKey = mbranchcode + "FMP00" + mperiod.Substring(0, 2) + Model.Srl;
                    mMas.PayParentKey = Model.ParentKey;
                    ctxTFAT.VoucherMaster.Add(mMas);
                    #endregion

                    #region Save Ledger
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
                        mobj3.AltCode = chg.PostCode;
                        mobj3.Audited = false;
                        mobj3.AUTHIDS = muserid;
                        mobj3.AUTHORISE = mauthorise;
                        mobj3.BillDate = DateTime.Now;
                        mobj3.BillNumber = Model.Srl;
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
                        mobj3.MainType = "PM";
                        mobj3.Narr = Model.Remark;
                        mobj3.Party = " ";
                        mobj3.Prefix = Model.Prefix;
                        mobj3.RecoFlag = "";
                        mobj3.RefDoc = "";
                        mobj3.TDSChallanNumber = "";
                        mobj3.BankCode = 0;
                        mobj3.ENTEREDBY = muserid;
                        mobj3.DueDate = Convert.ToDateTime("1900-01-01");
                        mobj3.Reminder = false;
                        mobj3.TaskID = 0;
                        mobj3.ChqCategory = 0;
                        mobj3.CurrAmount = 0;
                        mobj3.LocationCode = 100001;
                        mobj3.LASTUPDATEDATE = DateTime.Now;

                        mobj3.TDSCode = Model.TDSCode == null ? 0 : Convert.ToInt32(Model.TDSCode.ToString());
                        mobj3.TDSFlag = Model.TDSFlag;

                        mobj3.CGSTAmt = (decimal)Model.CGSTAmt;
                        mobj3.CGSTRate = Model.CGSTRate;
                        mobj3.IGSTAmt = (decimal)Model.IGSTAmt;
                        mobj3.IGSTRate = Model.IGSTRate;
                        mobj3.SGSTAmt = (decimal)Model.SGSTAmt;
                        mobj3.SGSTRate = Model.SGSTRate;
                        mobj3.TaxCode = Model.GSTCode;

                        mobj3.GSTType = 0;
                        mobj3.SLCode = 0/*item.SLCode*/;
                        ctxTFAT.Ledger.Add(mobj3);
                        LedgerList.Add(mobj3);

                        mLcnt = mLcnt + 1;
                    }

                    #endregion

                    #region Save Outstanding && VoucherDetail
                    int mCnt = 1;
                    var objledgerdetail = (List<AdvancePayModel>)Session["Aadvancepaylist"];
                    if (objledgerdetail == null)
                    {
                        objledgerdetail = new List<AdvancePayModel>();
                    }

                    bool ExtraTdsEntry = true;//Ledger Posting Tds Cut Krke Jata He Pr Outstanding Full Amount Jata He Esliye Tds Ka Bhi Entry Us Amount Ke Sath Dalna Jaruri He.
                    foreach (var item in objledgerdetail)
                    {
                        VoucherDetail vd = new VoucherDetail();
                        vd.Amount = item.ChargesList.Where(x => x.Fld == "Amt").Select(X => X.Amt).FirstOrDefault();
                        vd.Branch = Model.Branch;
                        vd.FmBran = item.Branch;
                        vd.FMNo = item.FMNo;
                        vd.InsClr = item.AdvType;
                        vd.SNo = mCnt;
                        vd.VouNo = Model.Srl;
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
                        vd.TableKey = mbranchcode + "FMP00" + mperiod.Substring(0, 2) + mCnt.ToString("D3") + Model.Srl;
                        vd.ParentKey = mbranchcode + "FMP00" + mperiod.Substring(0, 2) + Model.Srl;
                        vd.Prefix = mperiod;
                        vd.FMTableKey = item.RefTableKey;
                        vd.NetAmt = (vd.Amount + item.ChargesList.Where(x => x.AddLess == "+").Sum(x => x.Amt) - item.ChargesList.Where(x => x.Fld == "TDS").Sum(x => x.Amt));
                        vd.Type = "FMP00";
                        vd.FMRefParentkey = item.FMRefParentkey.Trim();
                        vd.TdsAmout = item.ChargesList.Where(x => x.Fld == "TDS").Select(X => X.Amt).FirstOrDefault();
                        ctxTFAT.VoucherDetail.Add(vd);


                        #region Update FMVOUREL
                        bool MaintainOutstanding = true;
                        var mFmRouRel = ctxTFAT.FMVouRel.Where(x => x.FMNo == item.FMNo && x.Type == true && x.PostCode == Model.Account).Select(x => x).FirstOrDefault();
                        if (mFmRouRel != null)
                        {
                            MaintainOutstanding = false;
                        }
                        #endregion

                        var BIllBranch = ctxTFAT.Ledger.Where(x => x.TableKey == item.RefTableKey).Select(x => x.Branch).FirstOrDefault() == null ? ctxTFAT.FMMaster.Where(x => x.FmNo.ToString().Trim() == item.FMNo.Trim()).Select(x => x.Branch).FirstOrDefault() : ctxTFAT.Ledger.Where(x => x.TableKey == item.RefTableKey).Select(x => x.Branch).FirstOrDefault();
                        var BIllDate = ctxTFAT.Ledger.Where(x => x.TableKey == item.RefTableKey).FirstOrDefault() == null ? ctxTFAT.FMMaster.Where(x => x.FmNo.ToString().Trim() == item.FMNo.Trim()).Select(x => x.Date).FirstOrDefault() : ctxTFAT.Ledger.Where(x => x.TableKey == item.RefTableKey).Select(x => x.DocDate).FirstOrDefault();

                        if (vd.Amount > 0 && MaintainOutstanding == true)
                        {
                            decimal TotalTdsCut = 0;
                            if (ExtraTdsEntry == true && Model.TDSFlag == true && vd.TdsAmout > 0 && String.IsNullOrEmpty(Model.TDSCode) == false)
                            {
                                TotalTdsCut = (decimal)vd.TdsAmout;
                                ExtraTdsEntry = false;
                            }
                            var BillDetails = LedgerList.Where(x => x.AltCode == Model.Bank && x.Code == Model.Account).Select(x => new { x.TableKey, x.Sno }).FirstOrDefault();
                            if (!String.IsNullOrEmpty(BillDetails.TableKey))
                            {
                                Outstanding osobj1 = new Outstanding();
                                osobj1.Branch = BIllBranch;
                                osobj1.DocBranch = Model.Branch;
                                osobj1.MainType = "PM";
                                osobj1.SubType = Model.SubType;
                                osobj1.Type = Model.Type;
                                osobj1.Prefix = mperiod;
                                osobj1.Srl = Model.Srl;
                                osobj1.Sno = BillDetails.Sno;
                                osobj1.ParentKey = Model.ParentKey;
                                osobj1.TableKey = BillDetails.TableKey;
                                osobj1.aMaintype = "LO";
                                osobj1.aSubType = "FR";
                                osobj1.aType = item.RefTableKey.Substring(0, 5);
                                osobj1.aPrefix = mperiod;
                                osobj1.aSrl = item.FMNo;
                                osobj1.aSno = item.aSno;
                                osobj1.Amount = Convert.ToDecimal(vd.Amount);
                                osobj1.TableRefKey = item.RefTableKey;
                                osobj1.AUTHIDS = muserid;
                                osobj1.AUTHORISE = mauthorise;
                                osobj1.BillDate = Convert.ToDateTime(BIllDate);
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
                                osobj1.CurrAmount = (decimal)(vd.Amount);
                                osobj1.ValueDate = DateTime.Now;
                                osobj1.LocationCode = 100001;
                                ctxTFAT.Outstanding.Add(osobj1);
                                // second effect
                                Outstanding osobj2 = new Outstanding();
                                osobj2.Branch = Model.Branch;
                                osobj2.DocBranch = Model.Branch;
                                osobj2.aMaintype = "PM";
                                osobj2.aSubType = Model.SubType;
                                osobj2.aType = Model.Type;
                                osobj2.aPrefix = mperiod;
                                osobj2.aSrl = Model.Srl;
                                osobj2.aSno = BillDetails.Sno;
                                osobj2.ParentKey = Model.ParentKey;
                                osobj2.TableRefKey = BillDetails.TableKey;

                                osobj2.Type = item.RefTableKey.Substring(0, 5);
                                osobj2.MainType = "LO";
                                osobj2.SubType = "FR";
                                osobj2.Prefix = mperiod;
                                osobj2.Srl = item.FMNo;
                                osobj2.Sno = item.aSno;
                                osobj2.TableKey = item.RefTableKey;
                                osobj2.Amount = (decimal)(vd.Amount - TotalTdsCut);
                                osobj2.AUTHIDS = muserid;
                                osobj2.AUTHORISE = mauthorise;
                                osobj2.BillDate = Convert.ToDateTime(BIllDate);
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
                                osobj2.CurrAmount = (decimal)(vd.Amount - TotalTdsCut);
                                osobj2.ValueDate = DateTime.Now;
                                osobj2.LocationCode = 100001;
                                ctxTFAT.Outstanding.Add(osobj2);
                            }

                        }

                        #region Save Charges Outstanding
                        if (MaintainOutstanding == true)
                        {
                            foreach (var Chrg in item.ChargesList.Where(x => x.Fld != "Amt" && x.Fld != "TDS").ToList())
                            {
                                var Charges = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.Fld == Chrg.Fld).FirstOrDefault();
                                if (Chrg.Amt > 0)
                                {
                                    //if posting avalable of Charges then found Tablekey of Ledger
                                    var BillDetails = LedgerList.Where(x => x.AltCode == Charges.Code && x.Code == Model.Account).Select(x => new { x.TableKey, x.Sno }).FirstOrDefault();
                                    if (!String.IsNullOrEmpty(BillDetails.TableKey))
                                    {
                                        if (Chrg.AddLess != "+")
                                        {
                                            Outstanding osobj1 = new Outstanding();
                                            osobj1.Branch = BIllBranch;
                                            osobj1.DocBranch = Model.Branch;
                                            osobj1.MainType = "PM";
                                            osobj1.SubType = Model.SubType;
                                            osobj1.Type = Model.Type;
                                            osobj1.Prefix = mperiod;
                                            osobj1.Srl = Model.Srl;
                                            osobj1.Sno = BillDetails.Sno;
                                            osobj1.ParentKey = Model.ParentKey;
                                            osobj1.TableKey = BillDetails.TableKey;
                                            osobj1.aMaintype = "LO";
                                            osobj1.aSubType = "FR";
                                            osobj1.aType = item.RefTableKey.Substring(0, 5);
                                            osobj1.aPrefix = mperiod;
                                            osobj1.aSrl = item.FMNo;
                                            osobj1.aSno = item.aSno;
                                            osobj1.Amount = Convert.ToDecimal(Chrg.Amt);
                                            osobj1.TableRefKey = item.RefTableKey;
                                            osobj1.AUTHIDS = muserid;
                                            osobj1.AUTHORISE = mauthorise;
                                            osobj1.BillDate = Convert.ToDateTime(BIllDate);
                                            osobj1.BillNumber = "";
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
                                            osobj1.CurrAmount = (decimal)Chrg.Amt;
                                            osobj1.ValueDate = DateTime.Now;
                                            osobj1.LocationCode = 100001;
                                            ctxTFAT.Outstanding.Add(osobj1);
                                        }

                                        // second effect
                                        Outstanding osobj2 = new Outstanding();
                                        osobj2.Branch = Model.Branch;
                                        osobj2.DocBranch = Model.Branch;
                                        osobj2.aMaintype = "PM";
                                        osobj2.aSubType = Model.SubType;
                                        osobj2.aType = Model.Type;
                                        osobj2.aPrefix = mperiod;
                                        osobj2.aSrl = Model.Srl;
                                        osobj2.aSno = BillDetails.Sno;
                                        osobj2.ParentKey = Model.ParentKey;
                                        osobj2.TableRefKey = BillDetails.TableKey;

                                        osobj2.Type = item.RefTableKey.Substring(0, 5);
                                        osobj2.MainType = "LO";
                                        osobj2.SubType = "FR";
                                        osobj2.Prefix = mperiod;
                                        osobj2.Srl = item.FMNo;
                                        osobj2.Sno = item.aSno;
                                        osobj2.TableKey = item.RefTableKey;
                                        osobj2.Amount = (decimal)Chrg.Amt;
                                        osobj2.AUTHIDS = muserid;
                                        osobj2.AUTHORISE = mauthorise;
                                        osobj2.BillDate = Convert.ToDateTime(BIllDate);
                                        osobj2.BillNumber = "";
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
                                        osobj2.CurrAmount = (decimal)Chrg.Amt;
                                        osobj2.ValueDate = DateTime.Now;
                                        osobj2.LocationCode = 100001;
                                        ctxTFAT.Outstanding.Add(osobj2);

                                    }
                                }

                                if (Chrg.AddLess == "+" && Chrg.Amt > 0)
                                {
                                    var BillDetails = LedgerList.Where(x => x.AltCode == Model.Bank && x.Code == Model.Account).Select(x => new { x.TableKey, x.Sno }).FirstOrDefault();
                                    if (!String.IsNullOrEmpty(BillDetails.TableKey))
                                    {
                                        Outstanding osobj1 = new Outstanding();
                                        // second effect
                                        Outstanding osobj2 = new Outstanding();
                                        osobj2.Branch = Model.Branch;
                                        osobj2.DocBranch = Model.Branch;
                                        osobj2.aMaintype = "PM";
                                        osobj2.aSubType = Model.SubType;
                                        osobj2.aType = Model.Type;
                                        osobj2.aPrefix = mperiod;
                                        osobj2.aSrl = Model.Srl;
                                        osobj2.aSno = BillDetails.Sno;
                                        osobj2.ParentKey = Model.ParentKey;
                                        osobj2.TableRefKey = BillDetails.TableKey;

                                        osobj2.Type = item.RefTableKey.Substring(0, 5);
                                        osobj2.MainType = "LO";
                                        osobj2.SubType = "FR";
                                        osobj2.Prefix = mperiod;
                                        osobj2.Srl = item.FMNo;
                                        osobj2.Sno = item.aSno;
                                        osobj2.TableKey = item.RefTableKey;
                                        osobj2.Amount = (decimal)Chrg.Amt;
                                        osobj2.AUTHIDS = muserid;
                                        osobj2.AUTHORISE = mauthorise;
                                        osobj2.BillDate = Convert.ToDateTime(BIllDate);
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
                                        osobj2.CurrAmount = (decimal)Chrg.Amt;
                                        osobj2.ValueDate = DateTime.Now;
                                        osobj2.LocationCode = 100001;
                                        ctxTFAT.Outstanding.Add(osobj2);
                                    }
                                }
                            }

                            if (Model.TDSFlag == true && vd.TdsAmout > 0 && String.IsNullOrEmpty(Model.TDSCode) == false)
                            {
                                var BillDetails = LedgerList.Where(x => x.AltCode == "000009994" && x.Code == Model.Account).Select(x => new { x.TableKey, x.Sno }).FirstOrDefault();
                                if (!String.IsNullOrEmpty(BillDetails.TableKey))
                                {
                                    //Outstanding osobj1 = new Outstanding();
                                    //osobj1.Branch = BIllBranch;
                                    //osobj1.DocBranch = Model.Branch;
                                    //osobj1.MainType = "PM";
                                    //osobj1.SubType = Model.SubType;
                                    //osobj1.Type = Model.Type;
                                    //osobj1.Prefix = mperiod;
                                    //osobj1.Srl = Model.Srl;
                                    //osobj1.Sno = BillDetails.Sno;
                                    //osobj1.ParentKey = Model.ParentKey;
                                    //osobj1.TableKey = BillDetails.TableKey;
                                    //osobj1.aMaintype = "LO";
                                    //osobj1.aSubType = "FR";
                                    //osobj1.aType = "FM000";
                                    //osobj1.aPrefix = mperiod;
                                    //osobj1.aSrl = item.FMNo;
                                    //osobj1.aSno = item.aSno;
                                    //osobj1.Amount = Convert.ToDecimal(vd.TdsAmout);
                                    //osobj1.TableRefKey = item.RefTableKey;
                                    //osobj1.AUTHIDS = muserid;
                                    //osobj1.AUTHORISE = "A00";
                                    //osobj1.BillDate = Convert.ToDateTime(BIllDate);
                                    //osobj1.BillNumber = "";
                                    //osobj1.CompCode = mcompcode;
                                    //osobj1.Broker = 100001;
                                    //osobj1.Brokerage = Convert.ToDecimal(0.00);
                                    //osobj1.BrokerAmt = Convert.ToDecimal(0.00);
                                    //osobj1.BrokerOn = Convert.ToDecimal(0.00);
                                    //osobj1.ChlnDate = DateTime.Now;
                                    //osobj1.ChlnNumber = "";
                                    //osobj1.Code = tDSMaster.PostCode;
                                    //osobj1.CrPeriod = 0;
                                    //osobj1.CurrName = 0;
                                    //osobj1.CurrRate = 1;
                                    //osobj1.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                    //osobj1.Narr = "";
                                    //osobj1.OrdDate = DateTime.Now;
                                    //osobj1.OrdNumber = "";
                                    //osobj1.ProjCode = "";
                                    //osobj1.ProjectStage = 0;
                                    //osobj1.ProjectUnit = 0;
                                    //osobj1.RefParty = "";
                                    //osobj1.SalemanAmt = Convert.ToDecimal(0.00);
                                    //osobj1.SalemanOn = Convert.ToDecimal(0.00);
                                    //osobj1.SalemanPer = Convert.ToDecimal(0.00);
                                    //osobj1.Salesman = 100001;
                                    //osobj1.TDSAmt = 0;
                                    //osobj1.ENTEREDBY = muserid;
                                    //osobj1.LASTUPDATEDATE = DateTime.Now;
                                    //osobj1.CurrAmount = (decimal)vd.TdsAmout;
                                    //osobj1.ValueDate = DateTime.Now;
                                    //osobj1.LocationCode = 100001;
                                    //ctxTFAT.Outstanding.Add(osobj1);


                                    // second effect
                                    Outstanding osobj2 = new Outstanding();
                                    osobj2.Branch = Model.Branch;
                                    osobj2.DocBranch = Model.Branch;
                                    osobj2.aMaintype = "PM";
                                    osobj2.aSubType = Model.SubType;
                                    osobj2.aType = Model.Type;
                                    osobj2.aPrefix = mperiod;
                                    osobj2.aSrl = Model.Srl;
                                    osobj2.aSno = BillDetails.Sno;
                                    osobj2.ParentKey = Model.ParentKey;
                                    osobj2.TableRefKey = BillDetails.TableKey;

                                    osobj2.Type = item.RefTableKey.Substring(0, 5);
                                    osobj2.MainType = "LO";
                                    osobj2.SubType = "FR";
                                    osobj2.Prefix = mperiod;
                                    osobj2.Srl = item.FMNo;
                                    osobj2.Sno = item.aSno;
                                    osobj2.TableKey = item.RefTableKey;
                                    osobj2.Amount = (decimal)vd.TdsAmout;
                                    osobj2.AUTHIDS = muserid;
                                    osobj2.AUTHORISE = mauthorise;
                                    osobj2.BillDate = Convert.ToDateTime(BIllDate);
                                    osobj2.BillNumber = "";
                                    osobj2.CompCode = mcompcode;
                                    osobj2.Broker = 100001;
                                    osobj2.Brokerage = Convert.ToDecimal(0.00);
                                    osobj2.BrokerAmt = Convert.ToDecimal(0.00);
                                    osobj2.BrokerOn = Convert.ToDecimal(0.00);
                                    osobj2.ChlnDate = DateTime.Now;
                                    osobj2.ChlnNumber = "";
                                    osobj2.Code = "000009994";
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
                                    osobj2.CurrAmount = (decimal)vd.TdsAmout;
                                    osobj2.ValueDate = DateTime.Now;
                                    osobj2.LocationCode = 100001;
                                    ctxTFAT.Outstanding.Add(osobj2);

                                }
                            }

                            if (Model.GSTFlag == true && String.IsNullOrEmpty(Model.GSTCode) == false)
                            {
                                string SGSTInputAccount = "000100032", CGSTInputAccount = "000100033", IGSTInputAccount = "000100034";

                                if (Model.IGSTAmt > 0)
                                {
                                    var BillDetails = LedgerList.Where(x => x.AltCode == IGSTInputAccount && x.Code == Model.Account).Select(x => new { x.TableKey, x.Sno }).FirstOrDefault();
                                    if (!String.IsNullOrEmpty(BillDetails.TableKey))
                                    {

                                        Outstanding osobj1 = new Outstanding();
                                        osobj1.Branch = BIllBranch;
                                        osobj1.DocBranch = Model.Branch;
                                        osobj1.MainType = "PM";
                                        osobj1.SubType = Model.SubType;
                                        osobj1.Type = Model.Type;
                                        osobj1.Prefix = mperiod;
                                        osobj1.Srl = Model.Srl;
                                        osobj1.Sno = BillDetails.Sno;
                                        osobj1.ParentKey = Model.ParentKey;
                                        osobj1.TableKey = BillDetails.TableKey;
                                        osobj1.aMaintype = "LO";
                                        osobj1.aSubType = "FR";
                                        osobj1.aType = item.RefTableKey.Substring(0, 5);
                                        osobj1.aPrefix = mperiod;
                                        osobj1.aSrl = item.FMNo;
                                        osobj1.aSno = item.aSno;
                                        osobj1.Amount = Convert.ToDecimal((vd.Amount * Model.IGSTRate) / 100);
                                        osobj1.TableRefKey = item.RefTableKey;
                                        osobj1.AUTHIDS = muserid;
                                        osobj1.AUTHORISE = mauthorise;
                                        osobj1.BillDate = Convert.ToDateTime(BIllDate);
                                        osobj1.BillNumber = "";
                                        osobj1.CompCode = mcompcode;
                                        osobj1.Broker = 100001;
                                        osobj1.Brokerage = Convert.ToDecimal(0.00);
                                        osobj1.BrokerAmt = Convert.ToDecimal(0.00);
                                        osobj1.BrokerOn = Convert.ToDecimal(0.00);
                                        osobj1.ChlnDate = DateTime.Now;
                                        osobj1.ChlnNumber = "";
                                        osobj1.Code = IGSTInputAccount;
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
                                        osobj1.CurrAmount = Convert.ToDecimal((vd.Amount * Model.IGSTRate) / 100);
                                        osobj1.ValueDate = DateTime.Now;
                                        osobj1.LocationCode = 100001;
                                        ctxTFAT.Outstanding.Add(osobj1);


                                        // second effect
                                        Outstanding osobj2 = new Outstanding();
                                        osobj2.Branch = Model.Branch;
                                        osobj2.DocBranch = Model.Branch;
                                        osobj2.aMaintype = "PM";
                                        osobj2.aSubType = Model.SubType;
                                        osobj2.aType = Model.Type;
                                        osobj2.aPrefix = mperiod;
                                        osobj2.aSrl = Model.Srl;
                                        osobj2.aSno = BillDetails.Sno;
                                        osobj2.ParentKey = Model.ParentKey;
                                        osobj2.TableRefKey = BillDetails.TableKey;

                                        osobj2.Type = item.RefTableKey.Substring(0, 5);
                                        osobj2.MainType = "LO";
                                        osobj2.SubType = "FR";
                                        osobj2.Prefix = mperiod;
                                        osobj2.Srl = item.FMNo;
                                        osobj2.Sno = item.aSno;
                                        osobj2.TableKey = item.RefTableKey;
                                        osobj2.Amount = Convert.ToDecimal((vd.Amount * Model.IGSTRate) / 100);
                                        osobj2.AUTHIDS = muserid;
                                        osobj2.AUTHORISE = mauthorise;
                                        osobj2.BillDate = Convert.ToDateTime(BIllDate);
                                        osobj2.BillNumber = "";
                                        osobj2.CompCode = mcompcode;
                                        osobj2.Broker = 100001;
                                        osobj2.Brokerage = Convert.ToDecimal(0.00);
                                        osobj2.BrokerAmt = Convert.ToDecimal(0.00);
                                        osobj2.BrokerOn = Convert.ToDecimal(0.00);
                                        osobj2.ChlnDate = DateTime.Now;
                                        osobj2.ChlnNumber = "";
                                        osobj2.Code = IGSTInputAccount;
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
                                        osobj2.CurrAmount = Convert.ToDecimal((vd.Amount * Model.IGSTRate) / 100);
                                        osobj2.ValueDate = DateTime.Now;
                                        osobj2.LocationCode = 100001;
                                        ctxTFAT.Outstanding.Add(osobj2);

                                    }
                                }
                                if (Model.SGSTAmt > 0)
                                {
                                    var BillDetails = LedgerList.Where(x => x.AltCode == SGSTInputAccount && x.Code == Model.Account).Select(x => new { x.TableKey, x.Sno }).FirstOrDefault();
                                    if (!String.IsNullOrEmpty(BillDetails.TableKey))
                                    {

                                        Outstanding osobj1 = new Outstanding();
                                        osobj1.Branch = BIllBranch;
                                        osobj1.DocBranch = Model.Branch;
                                        osobj1.MainType = "PM";
                                        osobj1.SubType = Model.SubType;
                                        osobj1.Type = Model.Type;
                                        osobj1.Prefix = mperiod;
                                        osobj1.Srl = Model.Srl;
                                        osobj1.Sno = BillDetails.Sno;
                                        osobj1.ParentKey = Model.ParentKey;
                                        osobj1.TableKey = BillDetails.TableKey;
                                        osobj1.aMaintype = "LO";
                                        osobj1.aSubType = "FR";
                                        osobj1.aType = item.RefTableKey.Substring(0, 5);
                                        osobj1.aPrefix = mperiod;
                                        osobj1.aSrl = item.FMNo;
                                        osobj1.aSno = item.aSno;
                                        osobj1.Amount = Convert.ToDecimal((vd.Amount * Model.SGSTRate) / 100);
                                        osobj1.TableRefKey = item.RefTableKey;
                                        osobj1.AUTHIDS = muserid;
                                        osobj1.AUTHORISE = mauthorise;
                                        osobj1.BillDate = Convert.ToDateTime(BIllDate);
                                        osobj1.BillNumber = "";
                                        osobj1.CompCode = mcompcode;
                                        osobj1.Broker = 100001;
                                        osobj1.Brokerage = Convert.ToDecimal(0.00);
                                        osobj1.BrokerAmt = Convert.ToDecimal(0.00);
                                        osobj1.BrokerOn = Convert.ToDecimal(0.00);
                                        osobj1.ChlnDate = DateTime.Now;
                                        osobj1.ChlnNumber = "";
                                        osobj1.Code = SGSTInputAccount;
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
                                        osobj1.CurrAmount = Convert.ToDecimal((vd.Amount * Model.SGSTRate) / 100);
                                        osobj1.ValueDate = DateTime.Now;
                                        osobj1.LocationCode = 100001;
                                        ctxTFAT.Outstanding.Add(osobj1);


                                        // second effect
                                        Outstanding osobj2 = new Outstanding();
                                        osobj2.Branch = Model.Branch;
                                        osobj2.DocBranch = Model.Branch;
                                        osobj2.aMaintype = "PM";
                                        osobj2.aSubType = Model.SubType;
                                        osobj2.aType = Model.Type;
                                        osobj2.aPrefix = mperiod;
                                        osobj2.aSrl = Model.Srl;
                                        osobj2.aSno = BillDetails.Sno;
                                        osobj2.ParentKey = Model.ParentKey;
                                        osobj2.TableRefKey = BillDetails.TableKey;

                                        osobj2.Type = item.RefTableKey.Substring(0, 5);
                                        osobj2.MainType = "LO";
                                        osobj2.SubType = "FR";
                                        osobj2.Prefix = mperiod;
                                        osobj2.Srl = item.FMNo;
                                        osobj2.Sno = item.aSno;
                                        osobj2.TableKey = item.RefTableKey;
                                        osobj2.Amount = Convert.ToDecimal((vd.Amount * Model.SGSTRate) / 100);
                                        osobj2.AUTHIDS = muserid;
                                        osobj2.AUTHORISE = mauthorise;
                                        osobj2.BillDate = Convert.ToDateTime(BIllDate);
                                        osobj2.BillNumber = "";
                                        osobj2.CompCode = mcompcode;
                                        osobj2.Broker = 100001;
                                        osobj2.Brokerage = Convert.ToDecimal(0.00);
                                        osobj2.BrokerAmt = Convert.ToDecimal(0.00);
                                        osobj2.BrokerOn = Convert.ToDecimal(0.00);
                                        osobj2.ChlnDate = DateTime.Now;
                                        osobj2.ChlnNumber = "";
                                        osobj2.Code = SGSTInputAccount;
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
                                        osobj2.CurrAmount = Convert.ToDecimal((vd.Amount * Model.SGSTRate) / 100);
                                        osobj2.ValueDate = DateTime.Now;
                                        osobj2.LocationCode = 100001;
                                        ctxTFAT.Outstanding.Add(osobj2);

                                    }
                                }
                                if (Model.CGSTAmt > 0)
                                {
                                    var BillDetails = LedgerList.Where(x => x.AltCode == CGSTInputAccount && x.Code == Model.Account).Select(x => new { x.TableKey, x.Sno }).FirstOrDefault();
                                    if (!String.IsNullOrEmpty(BillDetails.TableKey))
                                    {

                                        Outstanding osobj1 = new Outstanding();
                                        osobj1.Branch = BIllBranch;
                                        osobj1.DocBranch = Model.Branch;
                                        osobj1.MainType = "PM";
                                        osobj1.SubType = Model.SubType;
                                        osobj1.Type = Model.Type;
                                        osobj1.Prefix = mperiod;
                                        osobj1.Srl = Model.Srl;
                                        osobj1.Sno = BillDetails.Sno;
                                        osobj1.ParentKey = Model.ParentKey;
                                        osobj1.TableKey = BillDetails.TableKey;
                                        osobj1.aMaintype = "LO";
                                        osobj1.aSubType = "FR";
                                        osobj1.aType = item.RefTableKey.Substring(0, 5);
                                        osobj1.aPrefix = mperiod;
                                        osobj1.aSrl = item.FMNo;
                                        osobj1.aSno = item.aSno;
                                        osobj1.Amount = Convert.ToDecimal((vd.Amount * Model.CGSTRate) / 100);
                                        osobj1.TableRefKey = item.RefTableKey;
                                        osobj1.AUTHIDS = muserid;
                                        osobj1.AUTHORISE = mauthorise;
                                        osobj1.BillDate = Convert.ToDateTime(BIllDate);
                                        osobj1.BillNumber = "";
                                        osobj1.CompCode = mcompcode;
                                        osobj1.Broker = 100001;
                                        osobj1.Brokerage = Convert.ToDecimal(0.00);
                                        osobj1.BrokerAmt = Convert.ToDecimal(0.00);
                                        osobj1.BrokerOn = Convert.ToDecimal(0.00);
                                        osobj1.ChlnDate = DateTime.Now;
                                        osobj1.ChlnNumber = "";
                                        osobj1.Code = CGSTInputAccount;
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
                                        osobj1.CurrAmount = Convert.ToDecimal((vd.Amount * Model.CGSTRate) / 100);
                                        osobj1.ValueDate = DateTime.Now;
                                        osobj1.LocationCode = 100001;
                                        ctxTFAT.Outstanding.Add(osobj1);


                                        // second effect
                                        Outstanding osobj2 = new Outstanding();
                                        osobj2.Branch = Model.Branch;
                                        osobj2.DocBranch = Model.Branch;
                                        osobj2.aMaintype = "PM";
                                        osobj2.aSubType = Model.SubType;
                                        osobj2.aType = Model.Type;
                                        osobj2.aPrefix = mperiod;
                                        osobj2.aSrl = Model.Srl;
                                        osobj2.aSno = BillDetails.Sno;
                                        osobj2.ParentKey = Model.ParentKey;
                                        osobj2.TableRefKey = BillDetails.TableKey;

                                        osobj2.Type = item.RefTableKey.Substring(0, 5);
                                        osobj2.MainType = "LO";
                                        osobj2.SubType = "FR";
                                        osobj2.Prefix = mperiod;
                                        osobj2.Srl = item.FMNo;
                                        osobj2.Sno = item.aSno;
                                        osobj2.TableKey = item.RefTableKey;
                                        osobj2.Amount = Convert.ToDecimal((vd.Amount * Model.CGSTRate) / 100);
                                        osobj2.AUTHIDS = muserid;
                                        osobj2.AUTHORISE = mauthorise;
                                        osobj2.BillDate = Convert.ToDateTime(BIllDate);
                                        osobj2.BillNumber = "";
                                        osobj2.CompCode = mcompcode;
                                        osobj2.Broker = 100001;
                                        osobj2.Brokerage = Convert.ToDecimal(0.00);
                                        osobj2.BrokerAmt = Convert.ToDecimal(0.00);
                                        osobj2.BrokerOn = Convert.ToDecimal(0.00);
                                        osobj2.ChlnDate = DateTime.Now;
                                        osobj2.ChlnNumber = "";
                                        osobj2.Code = CGSTInputAccount;
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
                                        osobj2.CurrAmount = Convert.ToDecimal((vd.Amount * Model.CGSTRate) / 100);
                                        osobj2.ValueDate = DateTime.Now;
                                        osobj2.LocationCode = 100001;
                                        ctxTFAT.Outstanding.Add(osobj2);

                                    }
                                }


                            }


                        }
                        else
                        {
                            foreach (var Chrg in item.ChargesList.Where(x => x.Fld != "Amt" && x.Fld != "TDS").ToList())
                            {
                                var Charges = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.Fld == Chrg.Fld).FirstOrDefault();
                                if (Chrg.Amt > 0 && Chrg.AddLess == "+")
                                {
                                    //if posting avalable of Charges then found Tablekey of Ledger
                                    var BillDetails = LedgerList.Where(x => x.AltCode == Charges.Code && x.Code == Model.Account).Select(x => new { x.TableKey, x.Sno }).FirstOrDefault();
                                    if (!String.IsNullOrEmpty(BillDetails.TableKey))
                                    {
                                        // second effect
                                        Outstanding osobj2 = new Outstanding();
                                        osobj2.Branch = Model.Branch;
                                        osobj2.DocBranch = Model.Branch;
                                        osobj2.aMaintype = "PM";
                                        osobj2.aSubType = Model.SubType;
                                        osobj2.aType = Model.Type;
                                        osobj2.aPrefix = mperiod;
                                        osobj2.aSrl = Model.Srl;
                                        osobj2.aSno = BillDetails.Sno;
                                        osobj2.ParentKey = Model.ParentKey;
                                        osobj2.TableRefKey = BillDetails.TableKey;

                                        osobj2.Type = item.RefTableKey.Substring(0, 5);
                                        osobj2.MainType = "LO";
                                        osobj2.SubType = "FR";
                                        osobj2.Prefix = mperiod;
                                        osobj2.Srl = item.FMNo;
                                        osobj2.Sno = item.aSno;
                                        osobj2.TableKey = item.RefTableKey;
                                        osobj2.Amount = (decimal)Chrg.Amt;
                                        osobj2.AUTHIDS = muserid;
                                        osobj2.AUTHORISE = mauthorise;
                                        osobj2.BillDate = Convert.ToDateTime(BIllDate);
                                        osobj2.BillNumber = "";
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
                                        osobj2.CurrAmount = (decimal)Chrg.Amt;
                                        osobj2.ValueDate = DateTime.Now;
                                        osobj2.LocationCode = 100001;
                                        ctxTFAT.Outstanding.Add(osobj2);

                                    }

                                }
                                if (Chrg.AddLess == "+" && Chrg.Amt > 0)
                                {
                                    var BillDetails = LedgerList.Where(x => x.AltCode == Model.Bank && x.Code == Model.Account).Select(x => new { x.TableKey, x.Sno }).FirstOrDefault();
                                    if (!String.IsNullOrEmpty(BillDetails.TableKey))
                                    {
                                        Outstanding osobj1 = new Outstanding();
                                        // second effect
                                        Outstanding osobj2 = new Outstanding();
                                        osobj2.Branch = Model.Branch;
                                        osobj2.DocBranch = Model.Branch;
                                        osobj2.aMaintype = "PM";
                                        osobj2.aSubType = Model.SubType;
                                        osobj2.aType = Model.Type;
                                        osobj2.aPrefix = mperiod;
                                        osobj2.aSrl = Model.Srl;
                                        osobj2.aSno = BillDetails.Sno;
                                        osobj2.ParentKey = Model.ParentKey;
                                        osobj2.TableRefKey = BillDetails.TableKey;

                                        osobj2.Type = item.RefTableKey.Substring(0, 5);
                                        osobj2.MainType = "LO";
                                        osobj2.SubType = "FR";
                                        osobj2.Prefix = mperiod;
                                        osobj2.Srl = item.FMNo;
                                        osobj2.Sno = item.aSno;
                                        osobj2.TableKey = item.RefTableKey;
                                        osobj2.Amount = (decimal)Chrg.Amt;
                                        osobj2.AUTHIDS = muserid;
                                        osobj2.AUTHORISE = mauthorise;
                                        osobj2.BillDate = Convert.ToDateTime(BIllDate);
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
                                        osobj2.CurrAmount = (decimal)Chrg.Amt;
                                        osobj2.ValueDate = DateTime.Now;
                                        osobj2.LocationCode = 100001;
                                        ctxTFAT.Outstanding.Add(osobj2);
                                    }
                                }
                            }
                        }

                        #endregion

                        mCnt = mCnt + 1;

                    }

                    var LRCostList = (List<AdvancePayModel>)Session["AothertrxlistAdv"];
                    if (LRCostList == null)
                    {
                        LRCostList = new List<AdvancePayModel>();
                    }

                    if (LRCostList.Count() > 0)
                    {
                        var GetLrID = ctxTFAT.RelLr.OrderByDescending(x => x.LrID).Select(x => x.LrID).FirstOrDefault();
                        int GetLrID1 = (Convert.ToInt32(GetLrID) + 1);
                        var xCnt = 1;

                        foreach (var item in LRCostList)
                        {
                            GetLrID = GetLrID1.ToString();
                            if (GetLrID.Length > 6)
                            {
                                GetLrID.PadLeft(6, '0');
                            }

                            RelateData reldt = new RelateData();
                            reldt.Amount = item.LRDetailList.Sum(x => x.LRAmt);
                            reldt.AUTHIDS = muserid;
                            reldt.AUTHORISE = mauthorise;
                            reldt.Branch = Model.Branch;
                            reldt.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                            reldt.ENTEREDBY = muserid;
                            reldt.Deleted = false;
                            reldt.Type = Model.Type;
                            reldt.Srl = Convert.ToInt32(Model.Srl);
                            reldt.Sno = xCnt.ToString("D3");
                            reldt.SubType = Model.SubType;
                            reldt.LASTUPDATEDATE = DateTime.Now;
                            reldt.MainType = Model.MainType;
                            reldt.Code = item.Code;
                            reldt.Narr = item.Narr;
                            reldt.RelateTo = (byte)(3);
                            reldt.Value8 = "";//Vehicle NO
                            reldt.Combo1 = "";//OTher Cost Account
                            reldt.TableKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString();
                            reldt.ParentKey = Model.ParentKey;
                            reldt.AmtType = true;
                            reldt.ReqRelated = true;
                            reldt.Clear = false;
                            reldt.Status = false;
                            ctxTFAT.RelateData.Add(reldt);
                            int xrellrCnt = 1;
                            if (item.LRDetailList != null && item.LRDetailList.Count > 0)
                            {
                                foreach (var l in item.LRDetailList)
                                {
                                    RelLr rllr = new RelLr();
                                    rllr.AUTHIDS = muserid;
                                    rllr.AUTHORISE = mauthorise;
                                    rllr.Branch = Model.Branch;
                                    rllr.Deleted = false;
                                    rllr.ENTEREDBY = muserid;
                                    rllr.LASTUPDATEDATE = DateTime.Now;
                                    rllr.LrAmt = l.LRAmt;
                                    rllr.LrID = GetLrID;
                                    rllr.LrNo = l.LRNumber;
                                    rllr.TableKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString();
                                    rllr.SrNo = xrellrCnt;
                                    rllr.ParentKey = Model.ParentKey;
                                    rllr.LRRefTablekey = l.ConsignmentKey;
                                    rllr.Prefix = mperiod;

                                    ctxTFAT.RelLr.Add(rllr);



                                    xrellrCnt = xrellrCnt + 1;
                                }
                                ++GetLrID1;
                            }
                            ++xCnt;
                        }
                    }


                    #endregion


                    ctxTFAT.SaveChanges();

                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, Model.ParentKey, mMas.VouDate, Model.Amt, Model.Bank, "Save Adv && Bal Paid Through Doc NO:" + Model.Srl, "A");
                    AdvanceBalancePaymentNotification(mMas);
                    Session["Aadvancepaylist"] = null;
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

        #region ADD LR details

        public ActionResult AddLRDetails(AdvancePayModel Model)
        {
            List<AdvancePayModel> lrdetaillist = new List<AdvancePayModel>();
            var mlrmaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == Model.ConsignmentKey).Select(x => x).FirstOrDefault();

            if (mlrmaster == null)
            {
                Model.Status = "ValidError";
                Model.Message = "Consignment Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            if (Model.SessionFlag == "Add")
            {

                if (Model.LRDetailList != null)
                {
                    lrdetaillist = Model.LRDetailList;
                }

                if (lrdetaillist.Count() > 0 && (lrdetaillist.Select(x => x.ConsignmentKey).Contains(Model.ConsignmentKey)))
                {
                    Model.Status = "ValidError";
                    Model.Message = "Consignment Already in List Cant Save..";
                    return Json(Model, JsonRequestBehavior.AllowGet);
                }
                if (String.IsNullOrEmpty(Model.ConsignmentKey))
                {
                    Model.Status = "ConfirmError";
                    Model.Message = "Please Enter The Consignment No..";
                    return Json(Model, JsonRequestBehavior.AllowGet);
                }
                lrdetaillist.Add(new AdvancePayModel()
                {
                    ConsignmentKey = Model.ConsignmentKey,
                    LRNumber = Model.LRNumber,
                    LRAmt = Model.LRAmt,
                    tempid = lrdetaillist.Count + 1,
                });

            }
            else
            {
                if (Model.LRDetailList != null)
                {
                    lrdetaillist = Model.LRDetailList;
                }
                foreach (var item in lrdetaillist.Where(x => x.tempid == Model.tempid))
                {

                    item.ConsignmentKey = Model.ConsignmentKey;
                    item.LRNumber = Model.LRNumber;
                    item.LRAmt = Model.LRAmt;
                    item.tempid = Model.tempid;
                }
            }
            var html = ViewHelper.RenderPartialView(this, "LRDetails", new AdvancePayModel() { LRDetailList = lrdetaillist });
            return Json(new { LRDetailList = lrdetaillist, Html = html, Amt = lrdetaillist.Sum(x => x.LRAmt) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteLRDetails(AdvancePayModel Model)
        {
            var result2 = Model.LRDetailList.Where(x => x.tempid != Model.tempid).Select(x => x).ToList();
            var html = ViewHelper.RenderPartialView(this, "LRDetails", new AdvancePayModel() { LRDetailList = result2 });
            return Json(new { LRDetailList = result2, Html = html, Amt = result2.Sum(x => x.LRAmt) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetLRDetails(AdvancePayModel Model)
        {
            if (Model.LRDetailList != null && Model.LRDetailList.Count() > 0)
            {
                foreach (var a in Model.LRDetailList.Where(x => x.tempid == Model.tempid))
                {
                    Model.LRNumber = a.LRNumber;
                    Model.LRAmt = a.LRAmt;
                    Model.tempid = a.tempid;
                    Model.ConsignmentKey = a.ConsignmentKey;
                }
            }

            var html = ViewHelper.RenderPartialView(this, "LRDetails", Model);
            return Json(new { ConsignmentKey = Model.ConsignmentKey, Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetLrMasterDetails(AdvancePayModel Model)
        {
            var mlrmaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == Model.LRNumber).Select(x => x).FirstOrDefault();

            if (mlrmaster == null)
            {
                Model.Status = "ValidError";
                Model.Message = "Consignment Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            if (mlrmaster != null)
            {
                Model.ConsignmentKey = mlrmaster.TableKey.ToString();
                Model.LRNumber = mlrmaster.LrNo.ToString();
                Model.DocDate = mlrmaster.BookDate;
                Model.Qty = mlrmaster.TotQty;
                Model.ActWt = mlrmaster.ActWt;
                Model.ChgWt = mlrmaster.ChgWt;
                Model.LRFrom = ctxTFAT.TfatBranch.Where(x => x.Code == mlrmaster.Source).Select(x => x.Name).FirstOrDefault();
                Model.LRTo = ctxTFAT.TfatBranch.Where(x => x.Code == mlrmaster.Dest).Select(x => x.Name).FirstOrDefault();
                Model.LRConginer = ctxTFAT.Consigner.Where(x => x.Code == mlrmaster.SendCode).Select(x => x.Name).FirstOrDefault();
                Model.LRConsignee = ctxTFAT.Consigner.Where(x => x.Code == mlrmaster.RecCode).Select(x => x.Name).FirstOrDefault();
                var Setup = ctxTFAT.AdvBalSetup.FirstOrDefault();
                if (Setup != null)
                {
                    if (Setup.ShowConsignmentExp)
                    {
                        var result = (from lrExp in ctxTFAT.RelLr
                                      where lrExp.LRRefTablekey == mlrmaster.TableKey
                                      join Relateda in ctxTFAT.RelateData.Where(x => x.AmtType == true)
                                      on lrExp.TableKey equals Relateda.TableKey
                                      select new AdvancePayModel()
                                      {
                                          Amt = lrExp.LrAmt.Value,
                                          AccountName = ctxTFAT.Master.Where(x => x.Code == Relateda.Code).Select(x => x.Name).FirstOrDefault(),
                                          DocDate = Relateda.DocDate.Value,
                                          ENTEREDBY = Relateda.ENTEREDBY,
                                      }).OrderBy(x => x.DocDate).ToList();
                        Model.ConsignmentExplist = result;
                    }
                }
            }
            else
            {
                Model.ConsignmentExplist = new List<AdvancePayModel>();
            }

            var html = ViewHelper.RenderPartialView(this, "LRMasterDetails", Model);
            return Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FetchDocumentList(AdvancePayModel Model)
        {
            List<AdvancePayModel> ValueList = new List<AdvancePayModel>();

            var mlrmaster = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == Model.LRNumber).Select(x => x).ToList();
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
                AdvancePayModel otherTransact = new AdvancePayModel();
                otherTransact.ConsignmentKey = item.TableKey.ToString();
                otherTransact.LRNumber = item.LrNo.ToString();
                otherTransact.DocDate = item.BookDate;
                otherTransact.Qty = item.TotQty;
                otherTransact.ActWt = item.ActWt;
                otherTransact.ChgWt = item.ChgWt;
                otherTransact.LRFrom = ctxTFAT.TfatBranch.Where(x => x.Code == item.Source).Select(x => x.Name).FirstOrDefault();
                otherTransact.LRTo = ctxTFAT.TfatBranch.Where(x => x.Code == item.Dest).Select(x => x.Name).FirstOrDefault();
                otherTransact.LRConginer = ctxTFAT.Consigner.Where(x => x.Code == item.SendCode).Select(x => x.Name).FirstOrDefault();
                otherTransact.LRConsignee = ctxTFAT.Consigner.Where(x => x.Code == item.RecCode).Select(x => x.Name).FirstOrDefault();
                ValueList.Add(otherTransact);
            }
            Model.ValueList = ValueList;
            var html = ViewHelper.RenderPartialView(this, "ConsignmentList", Model);
            return Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ConfirmLRSave(AdvancePayModel Model)
        {
            if (Model.SessionFlag == "Add")
            {
                var Setup = ctxTFAT.AdvBalSetup.FirstOrDefault();
                if (Setup != null)
                {
                    if (Setup.RestrictLrDateExp)
                    {
                        var Days = String.IsNullOrEmpty(Setup.RestrictLrExpDays) == true ? 0 : Convert.ToInt32(Setup.RestrictLrExpDays);
                        var DocumentDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                        DocumentDate = DocumentDate.AddDays(-Days);

                        var ConsignemtDate = ctxTFAT.LRMaster.Where(x => x.TableKey == Model.ConsignmentKey).Select(x => x.BookDate).FirstOrDefault();
                        if (!(DocumentDate <= ConsignemtDate))
                        {
                            Model.Status = "ValidError";
                            Model.Message = "Consignemt Date Always Greater Than '" + DocumentDate.ToShortDateString() + "' Only...";
                        }

                    }
                }
            }
            else
            {

            }


            return Json(new { Status = Model.Status, Message = Model.Message }, JsonRequestBehavior.AllowGet);
        }

        public List<AdvancePayModel> GetLRDetailList(string TableKey)
        {
            List<AdvancePayModel> objledgerdetail2 = new List<AdvancePayModel>();
            var LRDetailList = ctxTFAT.RelLr.Where(x => (x.Branch + x.TableKey).ToString().Trim() == TableKey).Select(x => x).ToList();


            foreach (var a in LRDetailList)
            {
                objledgerdetail2.Add(new AdvancePayModel()
                {
                    LRNumber = a.LrNo,
                    LRAmt = a.LrAmt.Value,
                    tempid = a.SrNo,
                    ConsignmentKey = a.LRRefTablekey,
                });

            }
            return objledgerdetail2;
        }

        #endregion

        public List<AdvancePayModel> GetFMWiseCharges(string VouchNo, string FMNo, string FMTableKey)
        {
            List<AdvancePayModel> objledgerdetail = new List<AdvancePayModel>();

            AdvancePayModel c1 = new AdvancePayModel();
            c1.Fld = "Amt";
            c1.Header = "";
            c1.AddLess = "";
            c1.tempid = 1;
            c1.PostCode = "";
            //c.Amt = (mStrArray[a] == null) ? (decimal)0 : Convert.ToDecimal(mStrArray[a]);
            c1.Amt = (decimal)ctxTFAT.VoucherDetail.Where(x => x.FMTableKey == FMTableKey && x.FMNo == FMNo && x.VouNo == VouchNo).Select(x => x.Amount).FirstOrDefault();
            objledgerdetail.Add(c1);

            AdvancePayModel c2 = new AdvancePayModel();
            c2.Fld = "TDS";
            c2.Header = "";
            c2.AddLess = "";
            c2.PostCode = "";
            c1.tempid = 1;
            //c.Amt = (mStrArray[a] == null) ? (decimal)0 : Convert.ToDecimal(mStrArray[a]);
            c2.Amt = (decimal)ctxTFAT.VoucherDetail.Where(x => x.FMTableKey == FMTableKey && x.FMNo == FMNo && x.VouNo == VouchNo).Select(x => x.TdsAmout).FirstOrDefault();
            objledgerdetail.Add(c2);



            var trncharges = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.DontUse == false).Select(x => new { x.Fld, x.Head, x.EqAmt, x.Equation, x.Code }).ToList();
            foreach (var i in trncharges)
            {
                AdvancePayModel c = new AdvancePayModel();
                c.Fld = i.Fld;
                c.Header = i.Head;
                c.AddLess = i.EqAmt;
                c.PostCode = i.Code;
                c.tempid = Convert.ToInt16(i.Fld.Substring(1, 3));
                c.Amt = GetChargeValValue(c.tempid, VouchNo, FMNo, FMTableKey);


                objledgerdetail.Add(c);
            }
            return objledgerdetail;
        }

        public decimal GetChargeValValue(int i, string VouNo, string FMNo, string Fmtablekey)
        {
            string connstring = GetConnectionString();
            decimal abc;

            var loginQuery3 = @"select Val" + i + " from VoucherDetail where VouNo = '" + VouNo + "' and FMNO = '" + FMNo + "'and FMTableKey = '" + Fmtablekey + "'";
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
            string mCodeSerial = @"select Top 1 VouNo from VoucherMaster order by VouNo desc";
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
            mCode = mmCode.ToString();
            return mCode;
        }

        #region Do NOt Hit Function (Update All Advance And Balance Automatically)

        public ActionResult UpdateAllFMP()
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    foreach (var tfatBranch in ctxTFAT.TfatBranch.Where(x => x.Category == "Branch" && x.Code == "000007"))
                    {
                        var voucherMasterlist = ctxTFAT.VoucherMaster.Where(x => x.Branch == tfatBranch.Code).OrderBy(x => x.VouNo).ToList();

                        for (int i = 251; i <= 352; i++)
                        {
                            var voucherMaster = voucherMasterlist[i];
                            AdvancePayModel Model = new AdvancePayModel();
                            Model.MainType = "PM";
                            Model.SubType = "BP";
                            Model.Type = "FMP00";
                            Model.Branch = voucherMaster.Branch;
                            Model.ParentKey = voucherMaster.TableKey;
                            Model.Type = voucherMaster.Type;
                            var mVouchMaster = voucherMaster;
                            var mVoucherDetail = ctxTFAT.VoucherDetail.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).Select(x => x).ToList();
                            Model.DocDate = mVouchMaster.VouDate;
                            Model.DocuDate = mVouchMaster.VouDate.ToShortDateString();
                            Model.Bank = mVouchMaster.Bank;
                            Model.BankName = ctxTFAT.Master.Where(x => x.Code == mVouchMaster.Bank).Select(x => x.Name).FirstOrDefault();
                            Model.ChequeNo = mVouchMaster.ChqNo;
                            Model.Remark = mVouchMaster.Remark;
                            Model.NetAmt = mVouchMaster.Amount.Value;
                            Model.Account = mVouchMaster.Account;
                            Model.AccountName = ctxTFAT.Master.Where(x => x.Code == mVouchMaster.Account).Select(x => x.Name).FirstOrDefault();
                            Model.OthPostType = ctxTFAT.Master.Where(x => x.Code == mVouchMaster.Account).Select(x => x.RelatedTo).FirstOrDefault();
                            Model.Srl = mVouchMaster.VouNo;

                            Model.Prefix = mVouchMaster.Prefix;
                            List<AdvancePayModel> SelectedLedger = new List<AdvancePayModel>();
                            var objledgerdetail = ctxTFAT.VoucherDetail.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).Select(x => x).ToList();

                            foreach (var item in objledgerdetail)
                            {
                                int mFmNo = (string.IsNullOrEmpty(item.FMNo) == true) ? 0 : Convert.ToInt32(item.FMNo);

                                if (Model.OthPostType != "6")
                                {
                                    var mFMROUVEL = ctxTFAT.Ledger.Where(X => X.TableKey == item.FMTableKey && X.Branch == item.FmBran).Select(x => x).FirstOrDefault();
                                    if (mFMROUVEL != null)
                                    {
                                        var mchargelist = GetFMWiseCharges(Model.Srl, item.FMNo, item.FMTableKey);
                                        SelectedLedger.Add(new AdvancePayModel()
                                        {
                                            Branch = item.FmBran,
                                            BranchName = ctxTFAT.TfatBranch.Where(x => x.Code == item.FmBran).Select(x => x.Name).FirstOrDefault(),
                                            FMDateStr = "",
                                            FMDate = ctxTFAT.FMMaster.Where(x => x.FmNo == mFmNo).Select(x => x.Date).FirstOrDefault(),
                                            FMNo = item.FMNo,
                                            AdvPending = (item.InsClr == "A") ? (mFMROUVEL.Debit.Value - (ctxTFAT.VoucherDetail.Where(x => x.FMTableKey == item.FMTableKey && x.ParentKey != Model.ParentKey).Sum(x => (decimal?)x.Amount) ?? 0)) : (decimal)0,
                                            BalPending = 0,
                                            Amt = item.Amount.Value,
                                            AdvAmt = item.Amount == null ? 0 : item.Amount.Value,
                                            BalAmt = 0,
                                            ChargesList = mchargelist,
                                            Freight = mFMROUVEL.Credit == null ? 0 : mFMROUVEL.Credit.Value,
                                            tempid = SelectedLedger.Count + 1,
                                            RefTableKey = item.FMTableKey,
                                            aSno = ctxTFAT.Outstanding.Where(x => x.TableKey == item.TableKey).Select(x => x.aSno).FirstOrDefault(),
                                            AdvType = item.InsClr,
                                            NetAmt = item.NetAmt == null ? 0 : item.NetAmt.Value,
                                            ChgPickupList = mchargelist.Select(x => x.Amt).ToList(),
                                            Remark = item.Remark,
                                            PartyCode = item.Party,
                                            RelatedToCode = item.RelatedTo,

                                        });
                                    }
                                    else
                                    {
                                        var mFMROUVELactual = ctxTFAT.FMVouRel.Where(X => X.FMNo == item.FMTableKey && X.Branch == item.FmBran).Select(x => x).FirstOrDefault();
                                        var mchargelist = GetFMWiseCharges(Model.Srl, item.FMNo, item.FMTableKey);
                                        SelectedLedger.Add(new AdvancePayModel()
                                        {
                                            Branch = item.FmBran,
                                            BranchName = ctxTFAT.TfatBranch.Where(x => x.Code == item.FmBran).Select(x => x.Name).FirstOrDefault(),
                                            FMDateStr = "",
                                            FMDate = ctxTFAT.FMMaster.Where(x => x.FmNo == mFmNo).Select(x => x.Date).FirstOrDefault(),
                                            FMNo = item.FMNo,
                                            AdvPending = mFMROUVELactual.Adv.Value - (ctxTFAT.VoucherDetail.Where(x => x.FMTableKey == item.FMTableKey && x.ParentKey == Model.ParentKey).Sum(x => (decimal?)x.Amount) ?? 0),
                                            BalPending = 0,
                                            Amt = item.Amount.Value,
                                            AdvAmt = item.Amount == null ? 0 : item.Amount.Value,
                                            BalAmt = 0,
                                            ChargesList = mchargelist,
                                            Freight = mFMROUVELactual.Freight == null ? 0 : mFMROUVELactual.Freight.Value,
                                            tempid = SelectedLedger.Count + 1,
                                            RefTableKey = item.FMTableKey,
                                            aSno = 0,
                                            AdvType = item.InsClr,
                                            NetAmt = item.NetAmt == null ? 0 : item.NetAmt.Value,
                                            PrevAmt = item.Amount.Value,
                                            ChgPickupList = mchargelist.Select(x => x.Amt).ToList(),
                                            Remark = item.Remark,
                                            PartyCode = item.Party,
                                            RelatedToCode = item.RelatedTo,
                                        });
                                    }
                                }
                                else
                                {
                                    var mchargelist = GetFMWiseCharges(Model.Srl, item.FMNo, item.FMTableKey);
                                    var mlEDGER = ctxTFAT.Ledger.Where(X => X.TableKey == item.FMTableKey && X.Branch == item.FmBran).Select(x => x).FirstOrDefault();
                                    SelectedLedger.Add(new AdvancePayModel()
                                    {
                                        Branch = item.FmBran,
                                        BranchName = ctxTFAT.TfatBranch.Where(x => x.Code == item.FmBran).Select(x => x.Name).FirstOrDefault(),
                                        FMDateStr = "",
                                        FMDate = ctxTFAT.FMMaster.Where(x => x.FmNo == mFmNo).Select(x => x.Date).FirstOrDefault(),
                                        FMNo = item.FMNo,
                                        AdvPending = (item.InsClr == "A") ? mlEDGER.Credit.Value - (ctxTFAT.Outstanding.Where(x => x.TableRefKey == item.FMTableKey && x.ParentKey != Model.ParentKey).Sum(x => (decimal?)x.Amount) ?? 0) : 0,
                                        BalPending = (item.InsClr == "B") ? mlEDGER.Credit.Value - (ctxTFAT.Outstanding.Where(x => x.TableRefKey == item.FMTableKey && x.ParentKey != Model.ParentKey).Sum(x => (decimal?)x.Amount) ?? 0) : 0,
                                        Amt = item.Amount.Value,
                                        AdvAmt = item.Amount == null ? 0 : item.Amount.Value,
                                        BalAmt = 0,
                                        ChargesList = mchargelist,
                                        Freight = mlEDGER.Credit == null ? 0 : mlEDGER.Credit.Value,// check this must be debit
                                        tempid = SelectedLedger.Count + 1,
                                        RefTableKey = item.FMTableKey,
                                        aSno = ctxTFAT.Outstanding.Where(x => x.TableKey == item.TableKey).Select(x => x.aSno).FirstOrDefault(),
                                        AdvType = item.InsClr,
                                        NetAmt = item.NetAmt == null ? 0 : item.NetAmt.Value,
                                        ChgPickupList = mchargelist.Select(x => x.Amt).ToList(),
                                        Remark = mlEDGER.Narr
                                    });
                                }



                            }
                            Model.SelectedLedger = SelectedLedger;

                            Session["Aadvancepaylist"] = SelectedLedger;

                            Model.AccAmt = SelectedLedger.Sum(x => x.NetAmt);
                            if (objledgerdetail.Count() > 0)
                            {
                                Model.TDSAmt = (double)(ctxTFAT.VoucherDetail.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).Sum(x => x.Val6));
                            }

                            if (Model.TDSAmt > 0)
                            {
                                Model.TDSFlag = true;
                            }

                            Model.LedgerPostList = LedgerPostingReturn(Model);


                            #region Delete 

                            var mPayLedger = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).Select(x => x).ToList();
                            ctxTFAT.Ledger.RemoveRange(mPayLedger);

                            var mDeleteVoucherDet = ctxTFAT.VoucherDetail.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).ToList();
                            foreach (var item in mDeleteVoucherDet)
                            {
                                var mOutstanding = ctxTFAT.Outstanding.Where(x => (x.Srl == item.VouNo || x.aSrl == item.VouNo) && (x.Srl == item.FMNo || x.aSrl == item.FMNo)).Select(x => x).ToList();
                                ctxTFAT.Outstanding.RemoveRange(mOutstanding);
                            }

                            #endregion

                            #region Save Ledger
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
                                mobj3.AltCode = chg.PostCode;
                                mobj3.Audited = false;
                                mobj3.AUTHIDS = muserid;
                                mobj3.AUTHORISE = "A00";
                                mobj3.BillDate = DateTime.Now;
                                mobj3.BillNumber = "";
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
                                mobj3.MainType = "PM";
                                mobj3.Narr = Model.Remark;
                                mobj3.Party = chg.Code;
                                mobj3.Prefix = Model.Prefix;
                                mobj3.RecoFlag = "";
                                mobj3.RefDoc = "";
                                mobj3.TDSChallanNumber = "";
                                mobj3.BankCode = 0;
                                mobj3.ENTEREDBY = muserid;
                                mobj3.DueDate = Convert.ToDateTime("1900-01-01");
                                mobj3.Reminder = false;
                                mobj3.TaskID = 0;
                                mobj3.ChqCategory = 0;
                                mobj3.CurrAmount = 0;
                                mobj3.LocationCode = 100001;
                                mobj3.LASTUPDATEDATE = DateTime.Now;

                                mobj3.TDSCode = Model.TDSCode == null ? 0 : Convert.ToInt32(Model.TDSCode.ToString());
                                mobj3.TDSFlag = Model.TDSFlag;

                                mobj3.CGSTAmt = (decimal)Model.CGSTAmt;
                                mobj3.CGSTRate = Model.CGSTRate;
                                mobj3.IGSTAmt = (decimal)Model.IGSTAmt;
                                mobj3.IGSTRate = Model.IGSTRate;
                                mobj3.SGSTAmt = (decimal)Model.SGSTAmt;
                                mobj3.SGSTRate = Model.SGSTRate;
                                mobj3.TaxCode = Model.GSTCode;

                                mobj3.GSTType = 0;
                                mobj3.SLCode = 0/*item.SLCode*/;
                                ctxTFAT.Ledger.Add(mobj3);
                                LedgerList.Add(mobj3);

                                mLcnt = mLcnt + 1;
                            }
                            #endregion

                            #region Save Outstanding && VoucherDetail
                            int mCnt = 1;
                            var objledgerdetail1 = (List<AdvancePayModel>)Session["Aadvancepaylist"];
                            if (objledgerdetail1 == null)
                            {
                                objledgerdetail1 = new List<AdvancePayModel>();
                            }
                            foreach (var item in objledgerdetail1)
                            {
                                VoucherDetail vd = new VoucherDetail();
                                vd.Amount = item.ChargesList.Where(x => x.Fld == "F001").Select(X => X.Amt).FirstOrDefault();

                                #region Update FMVOUREL

                                var mFmRouRel = ctxTFAT.FMVouRel.Where(x => x.FMNo == item.FMNo && x.Type == true && x.PostCode == Model.Account).Select(x => x).FirstOrDefault();
                                if (mFmRouRel != null)
                                {
                                    var TotalAdjAmt = item.ChargesList.Where(x => x.AddLess != "+").Sum(x => x.Amt);
                                    if (item.AdvType == "A")
                                    {

                                        mFmRouRel.AdvPen = mFmRouRel.AdvPen - TotalAdjAmt;
                                        //mFmRouRel.AdvPen = mFmRouRel.AdvPen - item.Amt;
                                    }
                                    else
                                    {
                                        //mFmRouRel.BalPen = mFmRouRel.BalPen - item.Amt;
                                        mFmRouRel.BalPen = mFmRouRel.BalPen - TotalAdjAmt;
                                    }
                                    ctxTFAT.Entry(mFmRouRel).State = EntityState.Modified;
                                }
                                #endregion

                                var BIllBranch = ctxTFAT.Ledger.Where(x => x.TableKey == item.RefTableKey).Select(x => x.Branch).FirstOrDefault() == null ? ctxTFAT.FMMaster.Where(x => x.FmNo.ToString().Trim() == item.FMNo.Trim()).Select(x => x.Branch).FirstOrDefault() : ctxTFAT.Ledger.Where(x => x.TableKey == item.RefTableKey).Select(x => x.Branch).FirstOrDefault();
                                var BIllDate = ctxTFAT.Ledger.Where(x => x.TableKey == item.RefTableKey).FirstOrDefault() == null ? ctxTFAT.FMMaster.Where(x => x.FmNo.ToString().Trim() == item.FMNo.Trim()).FirstOrDefault() == null ? DateTime.Now : ctxTFAT.FMMaster.Where(x => x.FmNo.ToString().Trim() == item.FMNo.Trim()).Select(x => x.Date).FirstOrDefault() : ctxTFAT.Ledger.Where(x => x.TableKey == item.RefTableKey).Select(x => x.DocDate).FirstOrDefault();
                                if (String.IsNullOrEmpty(BIllBranch))
                                {
                                    BIllBranch = item.Branch;
                                }



                                if (vd.Amount > 0)
                                {
                                    var BillDetails = LedgerList.Where(x => x.AltCode == Model.Bank && x.Code == Model.Account).Select(x => new { x.TableKey, x.Sno }).FirstOrDefault();
                                    if (!String.IsNullOrEmpty(BillDetails.TableKey))
                                    {
                                        Outstanding osobj1 = new Outstanding();
                                        osobj1.Branch = BIllBranch;
                                        osobj1.DocBranch = Model.Branch;
                                        osobj1.MainType = "PM";
                                        osobj1.SubType = Model.SubType;
                                        osobj1.Type = Model.Type;
                                        osobj1.Prefix = mperiod;
                                        osobj1.Srl = Model.Srl;
                                        osobj1.Sno = BillDetails.Sno;
                                        osobj1.ParentKey = Model.ParentKey;
                                        osobj1.TableKey = BillDetails.TableKey;
                                        osobj1.aMaintype = "LO";
                                        osobj1.aSubType = "FR";
                                        osobj1.aType = item.RefTableKey.Substring(0, 5);
                                        osobj1.aPrefix = mperiod;
                                        osobj1.aSrl = item.FMNo;
                                        osobj1.aSno = item.aSno;
                                        osobj1.Amount = Convert.ToDecimal(vd.Amount);
                                        osobj1.TableRefKey = item.RefTableKey;
                                        osobj1.AUTHIDS = muserid;
                                        osobj1.AUTHORISE = "A00";
                                        osobj1.BillDate = Convert.ToDateTime(BIllDate);
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
                                        osobj1.CurrAmount = (decimal)vd.Amount;
                                        osobj1.ValueDate = DateTime.Now;
                                        osobj1.LocationCode = 100001;
                                        ctxTFAT.Outstanding.Add(osobj1);
                                        // second effect
                                        Outstanding osobj2 = new Outstanding();
                                        osobj2.Branch = Model.Branch;
                                        osobj2.DocBranch = Model.Branch;
                                        osobj2.aMaintype = "PM";
                                        osobj2.aSubType = Model.SubType;
                                        osobj2.aType = Model.Type;
                                        osobj2.aPrefix = mperiod;
                                        osobj2.aSrl = Model.Srl;
                                        osobj2.aSno = BillDetails.Sno;
                                        osobj2.ParentKey = Model.ParentKey;
                                        osobj2.TableRefKey = BillDetails.TableKey;

                                        osobj2.Type = item.RefTableKey.Substring(0, 5);
                                        osobj2.MainType = "LO";
                                        osobj2.SubType = "FR";
                                        osobj2.Prefix = mperiod;
                                        osobj2.Srl = item.FMNo;
                                        osobj2.Sno = item.aSno;
                                        osobj2.TableKey = item.RefTableKey;
                                        osobj2.Amount = (decimal)vd.Amount;
                                        osobj2.AUTHIDS = muserid;
                                        osobj2.AUTHORISE = "A00";
                                        osobj2.BillDate = Convert.ToDateTime(BIllDate);
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
                                        osobj2.CurrAmount = (decimal)vd.Amount;
                                        osobj2.ValueDate = DateTime.Now;
                                        osobj2.LocationCode = 100001;
                                        ctxTFAT.Outstanding.Add(osobj2);
                                    }
                                }

                                #region Save Charges Outstanding
                                foreach (var Chrg in item.ChargesList.Where(x => x.Fld != "F001").ToList())
                                {
                                    var Charges = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.Fld == Chrg.Fld).FirstOrDefault();
                                    if (Chrg.Amt > 0)
                                    {
                                        //if posting avalable of Charges then found Tablekey of Ledger
                                        var BillDetails = LedgerList.Where(x => x.AltCode == Charges.Code && x.Code == Model.Account).Select(x => new { x.TableKey, x.Sno }).FirstOrDefault();
                                        if (!String.IsNullOrEmpty(BillDetails.TableKey))
                                        {
                                            Outstanding osobj1 = new Outstanding();
                                            osobj1.Branch = BIllBranch;
                                            osobj1.DocBranch = Model.Branch;
                                            osobj1.MainType = "PM";
                                            osobj1.SubType = Model.SubType;
                                            osobj1.Type = Model.Type;
                                            osobj1.Prefix = mperiod;
                                            osobj1.Srl = Model.Srl;
                                            osobj1.Sno = BillDetails.Sno;
                                            osobj1.ParentKey = Model.ParentKey;
                                            osobj1.TableKey = BillDetails.TableKey;
                                            osobj1.aMaintype = "LO";
                                            osobj1.aSubType = "FR";
                                            osobj1.aType = item.RefTableKey.Substring(0, 5);
                                            osobj1.aPrefix = mperiod;
                                            osobj1.aSrl = item.FMNo;
                                            osobj1.aSno = item.aSno;
                                            osobj1.Amount = Convert.ToDecimal(Chrg.Amt);
                                            osobj1.TableRefKey = item.RefTableKey;
                                            osobj1.AUTHIDS = muserid;
                                            osobj1.AUTHORISE = "A00";
                                            osobj1.BillDate = Convert.ToDateTime(BIllDate);
                                            osobj1.BillNumber = "";
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
                                            osobj1.CurrAmount = (decimal)Chrg.Amt;
                                            osobj1.ValueDate = DateTime.Now;
                                            osobj1.LocationCode = 100001;
                                            ctxTFAT.Outstanding.Add(osobj1);
                                            // second effect
                                            Outstanding osobj2 = new Outstanding();
                                            osobj2.Branch = Model.Branch;
                                            osobj2.DocBranch = Model.Branch;
                                            osobj2.aMaintype = "PM";
                                            osobj2.aSubType = Model.SubType;
                                            osobj2.aType = Model.Type;
                                            osobj2.aPrefix = mperiod;
                                            osobj2.aSrl = Model.Srl;
                                            osobj2.aSno = BillDetails.Sno;
                                            osobj2.ParentKey = Model.ParentKey;
                                            osobj2.TableRefKey = BillDetails.TableKey;

                                            osobj2.Type = item.RefTableKey.Substring(0, 5);
                                            osobj2.MainType = "LO";
                                            osobj2.SubType = "FR";
                                            osobj2.Prefix = mperiod;
                                            osobj2.Srl = item.FMNo;
                                            osobj2.Sno = item.aSno;
                                            osobj2.TableKey = item.RefTableKey;
                                            osobj2.Amount = (decimal)Chrg.Amt;
                                            osobj2.AUTHIDS = muserid;
                                            osobj2.AUTHORISE = "A00";
                                            osobj2.BillDate = Convert.ToDateTime(BIllDate);
                                            osobj2.BillNumber = "";
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
                                            osobj2.CurrAmount = (decimal)Chrg.Amt;
                                            osobj2.ValueDate = DateTime.Now;
                                            osobj2.LocationCode = 100001;
                                            ctxTFAT.Outstanding.Add(osobj2);

                                        }
                                        else
                                        {
                                            Outstanding osobj1 = new Outstanding();
                                            osobj1.Branch = BIllBranch;
                                            osobj1.DocBranch = Model.Branch;
                                            osobj1.MainType = "PM";
                                            osobj1.SubType = Model.SubType;
                                            osobj1.Type = Model.Type;
                                            osobj1.Prefix = mperiod;
                                            osobj1.Srl = Model.Srl;
                                            osobj1.Sno = mCnt;
                                            osobj1.ParentKey = Model.ParentKey;
                                            osobj1.TableKey = mbranchcode + Model.Type + mperiod.Substring(0, 2) + mCnt.ToString("D3") + Model.Srl;
                                            osobj1.aMaintype = "LO";
                                            osobj1.aSubType = "FR";
                                            osobj1.aType = item.RefTableKey.Substring(0, 5);
                                            osobj1.aPrefix = mperiod;
                                            osobj1.aSrl = item.FMNo;
                                            osobj1.aSno = item.aSno;
                                            osobj1.Amount = Convert.ToDecimal(Chrg.Amt);
                                            osobj1.TableRefKey = item.RefTableKey;
                                            osobj1.AUTHIDS = muserid;
                                            osobj1.AUTHORISE = "A00";
                                            osobj1.BillDate = Convert.ToDateTime(BIllDate);
                                            osobj1.BillNumber = "";
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
                                            osobj1.CurrAmount = (decimal)Chrg.Amt;
                                            osobj1.ValueDate = DateTime.Now;
                                            osobj1.LocationCode = 100001;
                                            ctxTFAT.Outstanding.Add(osobj1);
                                            // second effect
                                            Outstanding osobj2 = new Outstanding();
                                            osobj2.Branch = Model.Branch;
                                            osobj2.DocBranch = Model.Branch;
                                            osobj2.aMaintype = "PM";
                                            osobj2.aSubType = Model.SubType;
                                            osobj2.aType = Model.Type;
                                            osobj2.aPrefix = mperiod;
                                            osobj2.aSrl = Model.Srl;
                                            osobj2.aSno = mCnt;
                                            osobj2.ParentKey = Model.ParentKey;
                                            osobj2.TableRefKey = mbranchcode + Model.Type + mperiod.Substring(0, 2) + mCnt.ToString("D3") + Model.Srl;

                                            osobj2.Type = item.RefTableKey.Substring(0, 5);
                                            osobj2.MainType = "LO";
                                            osobj2.SubType = "FR";
                                            osobj2.Prefix = mperiod;
                                            osobj2.Srl = item.FMNo;
                                            osobj2.Sno = item.aSno;
                                            osobj2.TableKey = item.RefTableKey;
                                            osobj2.Amount = (decimal)Chrg.Amt;
                                            osobj2.AUTHIDS = muserid;
                                            osobj2.AUTHORISE = "A00";
                                            osobj2.BillDate = Convert.ToDateTime(BIllDate);
                                            osobj2.BillNumber = "";
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
                                            osobj2.CurrAmount = (decimal)Chrg.Amt;
                                            osobj2.ValueDate = DateTime.Now;
                                            osobj2.LocationCode = 100001;
                                            ctxTFAT.Outstanding.Add(osobj2);
                                        }
                                    }

                                    if (Chrg.AddLess == "+" && Chrg.Amt > 0)
                                    {
                                        var BillDetails = LedgerList.Where(x => x.AltCode == Model.Bank && x.Code == Model.Account).Select(x => new { x.TableKey, x.Sno }).FirstOrDefault();
                                        if (!String.IsNullOrEmpty(BillDetails.TableKey))
                                        {
                                            Outstanding osobj1 = new Outstanding();
                                            // second effect
                                            Outstanding osobj2 = new Outstanding();
                                            osobj2.Branch = Model.Branch;
                                            osobj2.DocBranch = Model.Branch;
                                            osobj2.aMaintype = "PM";
                                            osobj2.aSubType = Model.SubType;
                                            osobj2.aType = Model.Type;
                                            osobj2.aPrefix = mperiod;
                                            osobj2.aSrl = Model.Srl;
                                            osobj2.aSno = BillDetails.Sno;
                                            osobj2.ParentKey = Model.ParentKey;
                                            osobj2.TableRefKey = BillDetails.TableKey;

                                            osobj2.Type = item.RefTableKey.Substring(0, 5);
                                            osobj2.MainType = "LO";
                                            osobj2.SubType = "FR";
                                            osobj2.Prefix = mperiod;
                                            osobj2.Srl = item.FMNo;
                                            osobj2.Sno = item.aSno;
                                            osobj2.TableKey = item.RefTableKey;
                                            osobj2.Amount = (decimal)Chrg.Amt;
                                            osobj2.AUTHIDS = muserid;
                                            osobj2.AUTHORISE = "A00";
                                            osobj2.BillDate = Convert.ToDateTime(BIllDate);
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
                                            osobj2.CurrAmount = (decimal)Chrg.Amt;
                                            osobj2.ValueDate = DateTime.Now;
                                            osobj2.LocationCode = 100001;
                                            ctxTFAT.Outstanding.Add(osobj2);
                                        }
                                    }
                                }
                                #endregion

                                mCnt = mCnt + 1;

                            }

                            #endregion

                            ctxTFAT.SaveChanges();
                        }

                    }


                    transaction.Commit();
                    transaction.Dispose();

                    return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
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

        public List<AdvancePayModel> LedgerPostingReturn(AdvancePayModel Model)
        {

            List<AdvancePayModel> LedPostList = new List<AdvancePayModel>();
            int mCnt = 1;
            var objledgerdetail = (List<AdvancePayModel>)Session["Aadvancepaylist"];

            #region Merge All Charges

            List<AdvancePayModel> mPostChargeList = new List<AdvancePayModel>();
            //charges ledger posting
            foreach (var a in objledgerdetail)
            {
                mPostChargeList.AddRange(a.ChargesList);
            }

            var mTotalChargeList = mPostChargeList.GroupBy(x => x.Fld).Select(x => new
            {
                FLD = x.Select(x1 => x1.Fld).FirstOrDefault(),
                Amt = x.Sum(x1 => x1.Amt),
                PostCode = x.Select(x1 => x1.PostCode).FirstOrDefault(),
                Addless = x.Select(x1 => x1.AddLess).FirstOrDefault(),
            }).ToList();
            //var ExtraAmountPaid = mTotalChargeList.Where(x => x.Addless != "-" && x.FLD != "F001").Sum(x => x.Amt);

            #endregion

            #region First Default Entry Of FMP00

            LedPostList.Add(new AdvancePayModel()
            {
                Code = Model.Account,
                AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                Debit = Math.Round((decimal)Model.AccAmt, 2),
                Credit = Math.Round((decimal)0, 2),
                Branch = mbranchcode,
                tempid = mCnt,
                PostCode = Model.Bank,
            });
            mCnt = mCnt + 1;
            LedPostList.Add(new AdvancePayModel()
            {
                Code = Model.Bank,
                AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Bank).Select(x => x.Name).FirstOrDefault(),
                Debit = Math.Round((decimal)0, 2),
                Credit = Math.Round((decimal)Model.AccAmt, 2),
                Branch = mbranchcode,
                tempid = mCnt,
                PostCode = Model.Account,
            });
            #endregion

            #region Charges Wise Ledger Posting

            foreach (var item in mTotalChargeList.Where(x => x.Amt != 0).ToList())
            {
                var PostingAllow = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.Fld.Trim() == item.FLD.Trim()).Select(x => x.Post).FirstOrDefault();
                if (PostingAllow)
                {
                    if (item.Addless == "+")
                    {
                        #region Broker Ko Extra Cash Diya He 
                        //mCnt = mCnt + 1;
                        //LedPostList.Add(new AdvancePayModel()
                        //{
                        //    Code = Model.Account,
                        //    AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                        //    Debit = Math.Round(item.Amt, 2),
                        //    Credit = Math.Round((decimal)0, 2),
                        //    Branch = mbranchcode,
                        //    tempid = mCnt,
                        //    PostCode = Model.Bank,
                        //});
                        //mCnt = mCnt + 1;
                        //LedPostList.Add(new AdvancePayModel()
                        //{
                        //    Code = Model.Bank,
                        //    AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Bank).Select(x => x.Name).FirstOrDefault(),
                        //    Debit = Math.Round((decimal)0, 2),
                        //    Credit = Math.Round(item.Amt, 2),
                        //    Branch = mbranchcode,
                        //    tempid = mCnt,
                        //    PostCode = Model.Account,
                        //});
                        #endregion

                        #region Jo Extra Cash Diya He uska Extra Ledger Posting
                        mCnt = mCnt + 1;
                        LedPostList.Add(new AdvancePayModel()
                        {
                            Code = item.PostCode,
                            AccountName = ctxTFAT.Master.Where(x => x.Code == item.PostCode).Select(x => x.Name).FirstOrDefault(),
                            Debit = Math.Round(item.Amt, 2),
                            Credit = Math.Round((decimal)0, 2),
                            Branch = mbranchcode,
                            tempid = mCnt,
                            PostCode = Model.Account,
                        });
                        mCnt = mCnt + 1;
                        LedPostList.Add(new AdvancePayModel()
                        {
                            Code = Model.Account,
                            AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                            Credit = Math.Round(item.Amt, 2),
                            Debit = Math.Round((decimal)0, 2),
                            Branch = mbranchcode,
                            tempid = mCnt,
                            PostCode = item.PostCode,
                        });
                        #endregion
                    }
                    else
                    {
                        mCnt = mCnt + 1;
                        LedPostList.Add(new AdvancePayModel()
                        {
                            Code = Model.Account,
                            AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                            Credit = Math.Round((decimal)0, 2),
                            Debit = Math.Round(item.Amt, 2),
                            Branch = mbranchcode,
                            tempid = mCnt,
                            PostCode = item.PostCode,
                        });
                        mCnt = mCnt + 1;
                        LedPostList.Add(new AdvancePayModel()
                        {
                            Code = item.PostCode,
                            AccountName = ctxTFAT.Master.Where(x => x.Code == item.PostCode).Select(x => x.Name).FirstOrDefault(),
                            Debit = Math.Round((decimal)0, 2),
                            Credit = Math.Round(item.Amt, 2),
                            Branch = mbranchcode,
                            tempid = mCnt,
                            PostCode = Model.Account,
                        });
                    }
                }
            }

            #endregion

            #region TDS Posting

            if (Model.TDSFlag == true && Model.TDSAmt > 0)
            {
                mCnt = mCnt + 1;
                LedPostList.Add(new AdvancePayModel()
                {
                    Code = Model.Account,
                    AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                    Credit = Math.Round((decimal)0, 2),
                    Debit = Math.Round((decimal)Model.TDSAmt, 2),
                    Branch = mbranchcode,
                    tempid = mCnt,
                    PostCode = "000009994",
                });
                mCnt = mCnt + 1;
                LedPostList.Add(new AdvancePayModel()
                {
                    Code = "000009994",
                    AccountName = ctxTFAT.Master.Where(x => x.Code == "000009994").Select(x => x.Name).FirstOrDefault(),
                    Debit = Math.Round((decimal)0, 2),
                    Credit = Math.Round((decimal)Model.TDSAmt, 2),
                    Branch = mbranchcode,
                    tempid = mCnt,
                    PostCode = Model.Account,
                });
            }

            #endregion

            Model.TotDebit = LedPostList.Sum(x => x.Debit);
            Model.TotCredit = LedPostList.Sum(x => x.Credit);
            Model.LedgerPostList = LedPostList;

            // display view form
            return LedPostList;
        }
        
        #endregion

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