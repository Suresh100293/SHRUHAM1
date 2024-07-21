/* -----------------------------------------
   Copyright 2019, Suchan Software Pvt. Ltd.
   ----------------------------------------- */
using ALT_ERP3.Controllers;
using ALT_ERP3.Areas.Accounts.Models;
using EntitiModel;
using System;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;
////using ALT_ERP3.DynamicBusinessLayer;
////using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Reports.Controllers
{
    public class ProfitLossClosingController : BaseController
    {
        private static string mheader = "";
        public ActionResult Index(LedgerVM mModel)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            mheader = mModel.Header;
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "A");
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData()
        {
            string mparentkey = "";
            decimal mprofit = 0;
            string mdocstring = RecToString("Select Code from DocTypes Where AppBranch='" + mbranchcode + "' And DocBackward<>0").Trim();
            string mserial = "";
            DateTime mDate1 = Convert.ToDateTime(Session["StartDate"]);
            DateTime mDate2 = Convert.ToDateTime(Session["LastDate"]);
            var mList = ctxTFAT.Ledger.Where(x => x.Branch == mbranchcode && x.DocDate >= mDate1 && x.DocDate <= mDate2 && x.Type.StartsWith("PAL")).ToList();
            if (mList.Count != 0)
            {
                mserial = ctxTFAT.Ledger.Where(x => x.Branch == mbranchcode && x.DocDate >= mDate1 && x.DocDate <= mDate2 && x.Type.StartsWith("PAL")).Select(x => x.Srl).FirstOrDefault() ?? "";
                ctxTFAT.Ledger.RemoveRange(mList);
                ctxTFAT.SaveChanges();
            }
            int mcurrency = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(z => z.CurrName).FirstOrDefault();
            if (mcurrency == 0)
            {
                mcurrency = 1;//india
            }
            string mtype;
            int mnpass = mdocstring == "" ? 1 : 2;
            for (int mpass = 1; mpass <= mnpass; ++mpass)
            {
                using (var transaction = ctxTFAT.Database.BeginTransaction())
                {
                    try
                    {
                        if (mpass == 1)
                        {
                            mtype = ctxTFAT.DocTypes.Where(z =>  z.DocBackward == false && z.Code.StartsWith("PAL")).Select(x => x.Code).FirstOrDefault() ?? "PAL00";
                        }
                        else
                        {
                            mtype = ctxTFAT.DocTypes.Where(z =>  z.DocBackward == true && z.Code.StartsWith("PAL")).Select(x => x.Code).FirstOrDefault() ?? "PAL00";
                        }
                        if (mserial == "") mserial = GetLastSerial("Ledger", mbranchcode, mtype, mperiod, "RJ", mDate2);
                        int mCnt = 1;
                        mprofit = 0;
                        mparentkey = mtype + mperiod.Substring(0, 2) + mserial;
                        var macclist = ctxTFAT.Master.Where(z =>  (z.GroupTree.Contains("000000003") || z.GroupTree.Contains("000000004") || z.GroupTree.Contains("000000005") || z.GroupTree.Contains("000000006"))).Select(x => x.Code).ToList();
                        foreach (string mcode in macclist)
                        {
                            decimal mamt = 0;
                            //if (mcode == "235101556")
                            //{
                            //    mamt = 0;
                            //}
                            if (mpass == 1)
                            {
                                mamt = ctxTFAT.Ledger.Where(x =>  x.DocDate >= mDate1 && x.DocDate <= mDate2 && x.MainType != "MV" && x.MainType != "PV" && x.Code == mcode && !mdocstring.Contains(x.Type)).Sum(x => x.Debit - x.Credit) ?? 0;
                                //mamt = FieldoftableNumber("Ledger", "Sum(Debit-Credit)", "Branch='"+ mbranchcode + "' && x.MainType!= "MV" && x.MainType!="PV" and DocDate>='"+MMDDYY(mDate1) +"' and DocDate<='"+ MMDDYY( mDate2) + "' and Code='"+ mcode +"'");
                            }
                            else
                            {
                                mamt = ctxTFAT.Ledger.Where(x =>  x.DocDate >= mDate1 && x.DocDate <= mDate2 && x.MainType != "MV" && x.MainType != "PV" && x.Code == mcode && mdocstring.Contains(x.Type)).Sum(x => x.Debit - x.Credit) ?? 0;
                            }
                            //GetBalance(mcode, mDate2, mbranchcode, 0, true);
                            mprofit += mamt;
                            if (mamt != 0)
                            {
                                Ledger mobj = new Ledger();
                                mobj.MainType = "JV";
                                mobj.SubType = "RJ";    // reserved journals
                                mobj.Type = mtype;
                                mobj.TableKey = mtype + mperiod.Substring(0, 2) + mCnt.ToString("D3") + mserial;
                                mobj.ParentKey = mparentkey;
                                mobj.Sno = mCnt++;
                                mobj.Srl = mserial;
                                mobj.DocDate = mDate2;
                                mobj.Branch = mbranchcode;
                                mobj.CompCode = mcompcode;
                                mobj.LocationCode = mlocationcode;
                                mobj.Audited = false;
                                mobj.BillDate = mDate2;
                                mobj.BillNumber = "";
                                mobj.Cheque = "";
                                mobj.ChequeReturn = false;
                                mobj.ClearDate = Convert.ToDateTime("1900-01-01");
                                mobj.ChequeDate = mobj.ClearDate;
                                mobj.Code = mcode;
                                mobj.CrPeriod = 0;
                                mobj.CurrName = mcurrency;
                                mobj.CurrRate = 1;
                                //mobj.Debit = mamt > 0 ? mamt : 0;
                                //mobj.Credit = mamt < 0 ? -mamt : 0;
                                mobj.Debit = mamt < 0 ? -mamt : 0;
                                mobj.Credit = mamt > 0 ? mamt : 0;
                                mobj.Discounted = false;
                                mobj.Narr = "Profit/Loss Journal.";
                                mobj.Party = mcode;
                                mobj.Prefix = mperiod;
                               // mobj.ProjCode = null;
                                //mobj.ProjectStage = null;
                                //mobj.ProjectUnit = null;
                                mobj.RecoFlag = "";
                                mobj.RefDoc = "";
                                mobj.TDSChallanNumber = "";
                                mobj.TDSCode = 0;
                                mobj.TDSFlag = false;
                                mobj.BankCode = 0;
                                mobj.DueDate = mobj.DocDate;
                                mobj.Reminder = false;
                                mobj.TaskID = 0;
                                mobj.ChqCategory = 0;
                                mobj.CurrAmount = 0;
                                mobj.GSTType = 0;
                                mobj.TaxCode = "";
                                mobj.IGSTAmt = 0;
                                mobj.IGSTRate = 0;
                                mobj.CGSTAmt = 0;
                                mobj.CGSTRate = 0;
                                mobj.SGSTAmt = 0;
                                mobj.SGSTRate = 0;

                                // iX9: Save default values to Std fields
                                mobj.ENTEREDBY = muserid;
                                mobj.LASTUPDATEDATE = DateTime.Now;
                                mobj.AUTHIDS = muserid;
                                mobj.AUTHORISE = "A00";
                                mobj.ENTEREDBY = muserid;
                                mobj.LASTUPDATEDATE = System.DateTime.Now;
                                ctxTFAT.Ledger.Add(mobj);
                            }
                        }
                        // Profit Distributed
                        decimal mTotal = 0;
                        decimal mper = 0;
                        string mlastcode = "";
                        var mplist = ctxTFAT.ProfitRatios.Select(x => new { x.Code, x.Ratio }).ToList();
                        foreach (var mrow in mplist)
                        {
                            if (mrow.Ratio != 0)
                            {
                                Ledger mobj = new Ledger();
                                mobj.MainType = "JV";
                                mobj.SubType = "RJ";    // reserved journals
                                mobj.Type = mtype;
                                mobj.TableKey = mtype + mperiod.Substring(0, 2) + mCnt.ToString("D3") + mserial;
                                mobj.ParentKey = mparentkey;
                                mobj.Sno = mCnt++;
                                mobj.Srl = mserial;
                                mobj.DocDate = mDate2;
                                mobj.Branch = mbranchcode;
                                mobj.CompCode = mcompcode;
                                mobj.LocationCode = mlocationcode;
                                mobj.Audited = false;
                                mobj.BillDate = mDate2;
                                mobj.BillNumber = "";
                                mobj.Cheque = "";
                                mobj.ChequeReturn = false;
                                mobj.ClearDate = Convert.ToDateTime("1900-01-01");
                                mobj.ChequeDate = mobj.ClearDate;
                                mobj.Code = mrow.Code;
                                mlastcode = mrow.Code;
                                mobj.CrPeriod = 0;
                                mobj.CurrName = mcurrency;
                                mobj.CurrRate = 1;
                                mper = mprofit * (mrow.Ratio ?? 100) / 100;
                                mTotal += mper;
                                //If mRs.EOF Then ' to calculate the remainder of profit adjust into the last account
                                //    If nRound(mTotal, gsCurrDec) <> mProfit Then mPer = mPer + (mProfit - mTotal)
                                //End If
                                mobj.Debit = mper > 0 ? mper : 0;
                                mobj.Credit = mper < 0 ? -mper : 0;
                                mobj.Discounted = false;
                                mobj.Narr = "Transferred to Balance Sheet.";
                                mobj.Party = mrow.Code;
                                mobj.Prefix = mperiod;
                                //mobj.ProjCode = null;
                                //mobj.ProjectStage = null;
                                //mobj.ProjectUnit = null;
                                mobj.RecoFlag = "";
                                mobj.RefDoc = "";
                                mobj.TDSChallanNumber = "";
                                mobj.TDSCode = 0;
                                mobj.TDSFlag = false;
                                mobj.BankCode = 0;
                                mobj.DueDate = mobj.DocDate;
                                mobj.Reminder = false;
                                mobj.TaskID = 0;
                                mobj.ChqCategory = 0;
                                mobj.CurrAmount = 0;
                                mobj.GSTType = 0;
                                mobj.TaxCode = "";
                                mobj.IGSTAmt = 0;
                                mobj.IGSTRate = 0;
                                mobj.CGSTAmt = 0;
                                mobj.CGSTRate = 0;
                                mobj.SGSTAmt = 0;
                                mobj.SGSTRate = 0;
                                // iX9: Save default values to Std fields
                                mobj.ENTEREDBY = muserid;
                                mobj.LASTUPDATEDATE = DateTime.Now;
                                mobj.AUTHIDS = muserid;
                                mobj.AUTHORISE = "A00";
                                mobj.ENTEREDBY = muserid;
                                mobj.LASTUPDATEDATE = System.DateTime.Now;
                                ctxTFAT.Ledger.Add(mobj);
                            }
                        }
                        // update the diff. into the last record
                        if (mTotal != mprofit && mlastcode != "")
                        {
                            mprofit -= mTotal;
                            Ledger mobj = new Ledger();
                            mobj.MainType = "JV";
                            mobj.SubType = "RJ";    // reserved journals
                            mobj.Type = mtype;
                            mobj.TableKey = mtype + mperiod.Substring(0, 2) + mCnt.ToString("D3") + mserial;
                            mobj.ParentKey = mparentkey;
                            mobj.Sno = mCnt++;
                            mobj.Srl = mserial;
                            mobj.DocDate = mDate2;
                            mobj.Branch = mbranchcode;
                            mobj.CompCode = mcompcode;
                            mobj.LocationCode = mlocationcode;
                            mobj.Audited = false;
                            mobj.BillDate = mDate2;
                            mobj.BillNumber = "";
                            mobj.Cheque = "";
                            mobj.ChequeReturn = false;
                            mobj.ClearDate = Convert.ToDateTime("1900-01-01");
                            mobj.ChequeDate = mobj.ClearDate;
                            mobj.Code = mlastcode;
                            mobj.CrPeriod = 0;
                            mobj.CurrName = mcurrency;
                            mobj.CurrRate = 1;
                            mobj.Debit = mprofit > 0 ? mprofit : 0;
                            mobj.Credit = mprofit < 0 ? -mprofit : 0;
                            mobj.Discounted = false;
                            mobj.Narr = "Transferred to Profit/Loss..";
                            mobj.Party = mlastcode;
                            mobj.Prefix = mperiod;
                            //mobj.ProjCode = null;
                            //mobj.ProjectStage = null;
                            //mobj.ProjectUnit = null;
                            mobj.RecoFlag = "";
                            mobj.RefDoc = "";
                            mobj.TDSChallanNumber = "";
                            mobj.TDSCode = 0;
                            mobj.TDSFlag = false;
                            mobj.BankCode = 0;
                            mobj.DueDate = mobj.DocDate;
                            mobj.Reminder = false;
                            mobj.TaskID = 0;
                            mobj.ChqCategory = 0;
                            mobj.CurrAmount = 0;
                            mobj.GSTType = 0;
                            mobj.TaxCode = "";
                            mobj.IGSTAmt = 0;
                            mobj.IGSTRate = 0;
                            mobj.CGSTAmt = 0;
                            mobj.CGSTRate = 0;
                            mobj.SGSTAmt = 0;
                            mobj.SGSTRate = 0;
                            // iX9: Save default values to Std fields
                            mobj.ENTEREDBY = muserid;
                            mobj.LASTUPDATEDATE = DateTime.Now;
                            mobj.AUTHIDS = muserid;
                            mobj.AUTHORISE = "A00";
                            mobj.ENTEREDBY = muserid;
                            mobj.LASTUPDATEDATE = System.DateTime.Now;
                            ctxTFAT.Ledger.Add(mobj);
                        }
                        ctxTFAT.SaveChanges();
                        UpdateAltCode(mparentkey);
                        transaction.Commit();
                        transaction.Dispose();
                        UpdateAuditTrail(mbranchcode, "Add", mheader, mparentkey, mDate2, mprofit, "", "", "A");
                    }
                    catch (DbEntityValidationException ex1)
                    {
                        transaction.Rollback();
                        return Json(new { Status = "Error", ex1.Message }, JsonRequestBehavior.AllowGet);
                    }
                    catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                    {
                        transaction.Rollback();
                        return Json(new { Status = "Error", ex.Message }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                        return Json(new { Status = "Error", e.Message }, JsonRequestBehavior.AllowGet);
                    }
                }
                mserial = "";
            } // mpass
            return Json(new { Status = "Success", Message = "Saved Successfuly..\nJournal Generated: " + mparentkey }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}