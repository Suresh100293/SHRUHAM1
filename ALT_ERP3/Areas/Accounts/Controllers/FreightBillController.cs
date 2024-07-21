using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Controllers;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class FreightBillController : BaseController
    {
        #region Scripting Functions
        string attachmentPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentPath"].ToString()) == true ? "" : ConfigurationManager.AppSettings["AttachmentPath"].ToString();
        private static string mauthorise = "A00";
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
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

        public ActionResult GetGSTType(string term)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();

            var GetList = ctxTFAT.TaxMaster.Where(x => x.VATGST == true && x.Scope == "S").Select(x => new { x.Code, x.Name }).ToList();

            foreach (var item in GetList)
            {
                GSt.Add(new SelectListItem { Value = item.Code, Text = item.Name.Trim() });
            }

            //GSt.Add(new SelectListItem { Value = "0", Text = "Tax Invoice" });
            //GSt.Add(new SelectListItem { Value = "1", Text = "Reverse Charge" });
            //GSt.Add(new SelectListItem { Value = "2", Text = "TCS" });
            //GSt.Add(new SelectListItem { Value = "3", Text = "TDS" });
            //GSt.Add(new SelectListItem { Value = "4", Text = "Bill Of Supply" });
            //GSt.Add(new SelectListItem { Value = "5", Text = "Export( Under LUT)" });
            //GSt.Add(new SelectListItem { Value = "6", Text = "UnRegistered Dealer" });
            //GSt.Add(new SelectListItem { Value = "7", Text = "Sez with Payment" });
            //GSt.Add(new SelectListItem { Value = "8", Text = "Sez w/0 Payment" });
            //GSt.Add(new SelectListItem { Value = "9", Text = "Export (with Duty)" });
            //GSt.Add(new SelectListItem { Value = "10", Text = "Exempted" });
            //GSt.Add(new SelectListItem { Value = "11", Text = "No GST" });
            //GSt.Add(new SelectListItem { Value = "12", Text = "Composition Dealer" });
            //GSt.Add(new SelectListItem { Value = "13", Text = "Deemed Export" });
            //GSt.Add(new SelectListItem { Value = "14", Text = "Nil Rated" });
            //GSt.Add(new SelectListItem { Value = "15", Text = "Export @ 0.1%" });
            //GSt.Add(new SelectListItem { Value = "16", Text = "Export @ 0.05%" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetPartyList(string term)
        {
            if (term == "")
            {

                var result = (from m in ctxTFAT.CustomerMaster
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
                              where m.Name.Contains(term)
                              select new
                              {
                                  m.Code,
                                  Name = m.Name,
                                  OName = m.Name
                              }).OrderBy(n => n.OName).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

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
        public ActionResult GetCreditLimit(string Party)
        {
            FreightBillVM Model = new FreightBillVM();
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
        public ActionResult GetAddressBySno(string Code, int Sno)
        {
            string Address = "";
            Caddress result;
            FreightBillVM Model = new FreightBillVM();
            result = (from Add in ctxTFAT.Caddress where Add.Code == Code && Add.Sno == Sno select Add).FirstOrDefault();
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

            Model.IntegerValue = ctxTFAT.Caddress.Where(x => x.Code == Code).ToList().Count;
            Model.SelectedIntegerValue = Sno;
            var html = ViewHelper.RenderPartialView(this, "_CustomerAddressList", Model);

            return Json(new { html = html, Address = Address }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetTDSType(string term)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();

            var GetList = ctxTFAT.TDSMaster.Select(x => new { x.Code, x.Name }).ToList();

            foreach (var item in GetList)
            {
                GSt.Add(new SelectListItem { Value = item.Code.ToString(), Text = item.Name.Trim() });
            }

            //GSt.Add(new SelectListItem { Value = "0", Text = "Tax Invoice" });
            //GSt.Add(new SelectListItem { Value = "1", Text = "Reverse Charge" });
            //GSt.Add(new SelectListItem { Value = "2", Text = "TCS" });
            //GSt.Add(new SelectListItem { Value = "3", Text = "TDS" });
            //GSt.Add(new SelectListItem { Value = "4", Text = "Bill Of Supply" });
            //GSt.Add(new SelectListItem { Value = "5", Text = "Export( Under LUT)" });
            //GSt.Add(new SelectListItem { Value = "6", Text = "UnRegistered Dealer" });
            //GSt.Add(new SelectListItem { Value = "7", Text = "Sez with Payment" });
            //GSt.Add(new SelectListItem { Value = "8", Text = "Sez w/0 Payment" });
            //GSt.Add(new SelectListItem { Value = "9", Text = "Export (with Duty)" });
            //GSt.Add(new SelectListItem { Value = "10", Text = "Exempted" });
            //GSt.Add(new SelectListItem { Value = "11", Text = "No GST" });
            //GSt.Add(new SelectListItem { Value = "12", Text = "Composition Dealer" });
            //GSt.Add(new SelectListItem { Value = "13", Text = "Deemed Export" });
            //GSt.Add(new SelectListItem { Value = "14", Text = "Nil Rated" });
            //GSt.Add(new SelectListItem { Value = "15", Text = "Export @ 0.1%" });
            //GSt.Add(new SelectListItem { Value = "16", Text = "Export @ 0.05%" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTDSList(string term)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.TDSMaster.Select(c => new { c.Code, c.Name }).Distinct().ToList().Take(10);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var result = ctxTFAT.TDSMaster.Where(x => x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).Distinct().ToList().Take(10);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetTDSRateDetail(CreditPurchaseVM Model)
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

        [HttpPost]
        public ActionResult GetPartyDetails(FreightBillVM Model)
        {
            var CurrDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            var result = ctxTFAT.CustomerMaster.Where(x => x.Code == Model.Code).Select(x => new { x.Code, x.Name, x.SalesMan, x.Broker, x.AccountParentGroup }).FirstOrDefault();
            Model.IntegerValue = ctxTFAT.Caddress.Where(x => x.Code == Model.Code).ToList().Count;
            Model.SelectedIntegerValue = Model.AltAddress;
            var html = ViewHelper.RenderPartialView(this, "_CustomerAddressList", Model);
            Model.AccParentGrp = result.AccountParentGroup;
            var msterinf = ctxTFAT.CMasterInfo.Where(x => x.Code == Model.Code).Select(x => new { x.CurrName, x.TDSCode, x.CutTDS, x.CrPeriod, x.Brokerage, x.Transporter, x.IncoTerms, x.IncoPlace, x.PaymentTerms, x.Rank }).FirstOrDefault();
            var addrl = ctxTFAT.Caddress.Where(x => x.Code == Model.Code && x.Sno == Model.AltAddress).Select(x => new { x.Email, x.State, x.Name }).FirstOrDefault();
            var taxdetails = ctxTFAT.CTaxDetails.Where(x => x.Code == Model.Code).Select(x => new { x.TDSCode, x.CutTDS }).FirstOrDefault();
            Model.CrPeriod = msterinf == null ? 0 : (msterinf.CrPeriod == null) ? 0 : msterinf.CrPeriod.Value;
            Model.AccParentGrpName = ctxTFAT.Master.Where(x => x.Code == Model.AccParentGrp).Select(x => x.Name).FirstOrDefault();
            Model.CPerson = addrl == null ? "" : addrl.Name;
            Model.PlaceOfSupply = addrl == null ? "" : addrl.State;
            Model.CutTDS = taxdetails == null ? false : taxdetails.CutTDS;
            Model.TDSCode = taxdetails == null ? 0 : taxdetails.TDSCode == null ? 0 : taxdetails.TDSCode.Value;
            Model.TDSName = ctxTFAT.TDSMaster.Where(x => x.Code == Model.TDSCode).Select(x => x.Name).FirstOrDefault();
            var mDocDate = DateTime.Now.Date;
            var TDSRATEtab = ctxTFAT.TDSRates.Where(x => x.Code == Model.TDSCode && x.EffDate <= mDocDate).OrderByDescending(x => x.EffDate).Select(x => new { x.TDSRate, x.Cess, x.SHECess, x.SurCharge }).FirstOrDefault();
            decimal TDSRate = 0;
            TDSRate = ctxTFAT.TDSRates.Where(x => x.Code == Model.TDSCode && x.EffDate <= CurrDate).OrderByDescending(x => x.EffDate).Select(x => x.TDSRate ?? 0).FirstOrDefault();
            Model.TDSRate = TDSRate;
            var Branch = "";
            var master = ctxTFAT.Master.Where(x => x.Code == Model.AccParentGrp).FirstOrDefault();
            if (master.FetchBalAcc == false)
            {
                Branch = mbranchcode;
            }
            string mStr = @"select dbo.GetBalance('" + Model.AccParentGrp + "','" + MMDDYY(DateTime.Now) + "','" + Branch + "',0,0)";
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

            var TDSFlagSetup = ctxTFAT.WithoutLRBillSetup.Select(x => x.CutTDS).FirstOrDefault();
            if (TDSFlagSetup == false)
            {
                Model.CutTDS = false;
            }

            #region GST
            string GSTCode = "0", GSTName = "";
            bool GstFlag = false;
            decimal IGST = 0, CGST = 0, SGST = 0;
            var setup = ctxTFAT.WithoutLRBillSetup.FirstOrDefault();
            if (setup == null)
            {
                setup = new WithoutLRBillSetup();
            }

            if (setup.follow_GST_HSN_Ledgerwise)
            {
                MasterInfo Masteraddress = ctxTFAT.MasterInfo.Where(x => x.Code == Model.AccParentGrp.Trim()).FirstOrDefault();
                if (Masteraddress != null)
                {
                    GstFlag = Masteraddress == null ? false : Masteraddress.GSTFlag;
                    if (ctxTFAT.HSNMaster.Where(x => x.Code == Masteraddress.HSNCode).FirstOrDefault() != null)
                    {
                        GSTCode = Masteraddress.HSNCode;
                        GSTName = ctxTFAT.HSNMaster.Where(x => x.Code == GSTCode).Select(x => x.Name).FirstOrDefault();

                        var EffectDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                        var pstate = ctxTFAT.Caddress.Where(x => x.Code == Model.Code.Trim()).Select(x => x.State).FirstOrDefault();
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
                    else
                    {
                        GSTCode = Masteraddress.HSNCode;
                        GSTName = ctxTFAT.TaxMaster.Where(x => x.Code == GSTCode).Select(x => x.Name).FirstOrDefault();

                        var EffectDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                        var pstate = ctxTFAT.Caddress.Where(x => x.Code == Model.Code.Trim()).Select(x => x.State).FirstOrDefault();
                        if (String.IsNullOrEmpty(pstate))
                        {
                            pstate = "19";
                        }
                        var branchstate = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.State).FirstOrDefault();
                        if (pstate == branchstate)
                        {
                            var result1 = ctxTFAT.TaxMaster.Where(x => x.Code == Masteraddress.HSNCode).Select(x => new { IGSTRate = (decimal)0, SGSTRate = x.SGSTRate.Value, CGSTRate = x.CGSTRate.Value }).FirstOrDefault();
                            IGST = result1 == null ? 0 : result1.IGSTRate;
                            SGST = result1 == null ? 0 : result1.SGSTRate;
                            CGST = result1 == null ? 0 : result1.CGSTRate;
                        }
                        else
                        {
                            var result1 = ctxTFAT.TaxMaster.Where(x => x.Code == Masteraddress.HSNCode).Select(x => new { IGSTRate = x.IGSTRate.Value, SGSTRate = (decimal)0, CGSTRate = (decimal)0 }).FirstOrDefault();
                            IGST = result1 == null ? 0 : result1.IGSTRate;
                            SGST = result1 == null ? 0 : result1.SGSTRate;
                            CGST = result1 == null ? 0 : result1.CGSTRate;
                        }
                    }
                }
            }
            else
            {
                CMasterInfo Masteraddress = ctxTFAT.CMasterInfo.Where(x => x.Code == Model.Code.Trim()).FirstOrDefault();
                if (Masteraddress != null)
                {
                    GstFlag = Masteraddress == null ? false : Masteraddress.GSTFlag;
                    if (ctxTFAT.HSNMaster.Where(x => x.Code == Masteraddress.HSNCode).FirstOrDefault() != null)
                    {
                        GSTCode = Masteraddress.HSNCode;
                        GSTName = ctxTFAT.HSNMaster.Where(x => x.Code == GSTCode).Select(x => x.Name).FirstOrDefault();

                        var EffectDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                        var pstate = ctxTFAT.Caddress.Where(x => x.Code == Model.Code.Trim()).Select(x => x.State).FirstOrDefault();
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
                    else
                    {
                        GSTCode = Masteraddress.HSNCode;
                        GSTName = ctxTFAT.TaxMaster.Where(x => x.Code == GSTCode).Select(x => x.Name).FirstOrDefault();

                        var EffectDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                        var pstate = ctxTFAT.Caddress.Where(x => x.Code == Model.Code.Trim()).Select(x => x.State).FirstOrDefault();
                        if (String.IsNullOrEmpty(pstate))
                        {
                            pstate = "19";
                        }
                        var branchstate = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.State).FirstOrDefault();
                        if (pstate == branchstate)
                        {
                            var result1 = ctxTFAT.TaxMaster.Where(x => x.Code == Masteraddress.HSNCode).Select(x => new { IGSTRate = (decimal)0, SGSTRate = x.SGSTRate.Value, CGSTRate = x.CGSTRate.Value }).FirstOrDefault();
                            IGST = result1 == null ? 0 : result1.IGSTRate;
                            SGST = result1 == null ? 0 : result1.SGSTRate;
                            CGST = result1 == null ? 0 : result1.CGSTRate;
                        }
                        else
                        {
                            var result1 = ctxTFAT.TaxMaster.Where(x => x.Code == Masteraddress.HSNCode).Select(x => new { IGSTRate = x.IGSTRate.Value, SGSTRate = (decimal)0, CGSTRate = (decimal)0 }).FirstOrDefault();
                            IGST = result1 == null ? 0 : result1.IGSTRate;
                            SGST = result1 == null ? 0 : result1.SGSTRate;
                            CGST = result1 == null ? 0 : result1.CGSTRate;
                        }
                    }
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
                TDSName = Model.TDSName,
                AccParentGrpName = Model.AccParentGrpName,
                CPerson = Model.CPerson,
                Balance = Model.Balance,
                HoldNarr = Model.HoldNarr,
                HoldInvoice = Model.HoldInvoice,
                AlertHoldInvoice = Model.AlertHoldInvoice,
                html = html,
                Tickler = Model.Tickler
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetGSTRateDetail(string Code, string Party, string DocDate)
        {
            var setup = ctxTFAT.LRBillSetup.FirstOrDefault();
            if (setup == null)
            {
                setup = new LRBillSetup();
            }
            var CurrDate = ConvertDDMMYYTOYYMMDD(DocDate);
            var pstate = ctxTFAT.Caddress.Where(x => x.Code == Party).Select(x => x.State).FirstOrDefault();
            if (setup.follow_GST_HSN_Ledgerwise)
            {
                var Group = ctxTFAT.CustomerMaster.Where(x => x.Code == Party).Select(x => x.AccountParentGroup).FirstOrDefault();
                pstate = ctxTFAT.Address.Where(x => x.Code == Group).Select(x => x.State).FirstOrDefault();
            }
            if (String.IsNullOrEmpty(pstate))
            {
                pstate = "19";
            }
            var branchstate = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.State).FirstOrDefault();
            if (pstate == branchstate)
            {
                if (ctxTFAT.HSNMaster.Where(x => x.Code == Code).FirstOrDefault() != null)
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
                if (ctxTFAT.HSNMaster.Where(x => x.Code == Code).FirstOrDefault() != null)
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
        #endregion

        #region Server Functions

        public string GetGSTTypeName(string Val)
        {
            string Name;
            List<SelectListItem> GSt = new List<SelectListItem>();
            var GetList = ctxTFAT.TaxMaster.Where(x => x.Scope == "S").Select(x => new { x.Code, x.Name }).ToList();

            foreach (var item in GetList)
            {
                GSt.Add(new SelectListItem { Value = item.Code, Text = item.Name.Trim() });
            }
            //GSt.Add(new SelectListItem { Value = "0", Text = "Tax Invoice" });
            //GSt.Add(new SelectListItem { Value = "1", Text = "Reverse Charge" });
            //GSt.Add(new SelectListItem { Value = "2", Text = "TCS" });
            //GSt.Add(new SelectListItem { Value = "3", Text = "TDS" });
            //GSt.Add(new SelectListItem { Value = "4", Text = "Bill Of Supply" });
            //GSt.Add(new SelectListItem { Value = "5", Text = "Export( Under LUT)" });
            //GSt.Add(new SelectListItem { Value = "6", Text = "UnRegistered Dealer" });
            //GSt.Add(new SelectListItem { Value = "7", Text = "Sez with Payment" });
            //GSt.Add(new SelectListItem { Value = "8", Text = "Sez w/0 Payment" });
            //GSt.Add(new SelectListItem { Value = "9", Text = "Export (with Duty)" });
            //GSt.Add(new SelectListItem { Value = "10", Text = "Exempted" });
            //GSt.Add(new SelectListItem { Value = "11", Text = "No GST" });
            //GSt.Add(new SelectListItem { Value = "12", Text = "Composition Dealer" });
            //GSt.Add(new SelectListItem { Value = "13", Text = "Deemed Export" });
            //GSt.Add(new SelectListItem { Value = "14", Text = "Nil Rated" });
            //GSt.Add(new SelectListItem { Value = "15", Text = "Export @ 0.1%" });
            //GSt.Add(new SelectListItem { Value = "16", Text = "Export @ 0.05%" });
            Name = GSt.Where(x => x.Value == Val).Select(x => x.Text).FirstOrDefault();
            return Name;
        }
        public List<FreightBillVM> GetAttachmentListInEdit(FreightBillVM Model)
        {
            List<FreightBillVM> AttachmentList = new List<FreightBillVM>();
            try
            {
                var docdetail = ctxTFAT.Attachment.Where(x => x.ParentKey == Model.ParentKey).OrderBy(x => x.Sno).ToList();
                foreach (var item in docdetail)
                {
                    AttachmentList.Add(new FreightBillVM()
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
            }
            catch { }
            return AttachmentList;
        }
        
        public string CheckPrimaryKey(FreightBillVM Model)
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


        #endregion

        // GET: Accounts/FreightBill
        public ActionResult Index(FreightBillVM Model)
        {
            Session["CommnNarrlist"] = null;
            Session["TempAttach"] = null;
            GetAllMenu(Session["ModuleName"].ToString());
            if (Model.Mode != "Add")
            {
                Model.Branch = Model.Document.Substring(0, 6);
                Model.ParentKey = Model.Document.Substring(6);
                Model.Type = "SLW00";
                UpdateAuditTrail(Model.Branch, Model.Mode, Model.Header, Model.ParentKey, DateTime.Now, 0, "", "", "NA");
            }
            else
            {
                Model.Branch = mbranchcode;
                Model.Type = "SLW00";
                UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "NA");
            }
            #region Setup
            //string connstring = GetConnectionString();
            //var LRBillQuery = @"select BillBoth,BillAuto,ShowLedgerPost from WithoutLRBillSetup";
            //DataTable imDt = GetDataTable(LRBillQuery, connstring);
            WithoutLRBillSetup withoutLRBillSetup = ctxTFAT.WithoutLRBillSetup.FirstOrDefault();
            if (withoutLRBillSetup == null)
            {
                withoutLRBillSetup = new WithoutLRBillSetup();
            }
            Model.BillBoth = withoutLRBillSetup.BillBoth;
            Model.BillAuto = withoutLRBillSetup.BillAuto;
            Model.ShowLedgerPost = withoutLRBillSetup.ShowLedgerPost;
            if (withoutLRBillSetup.CurrDatetOnlyreq == false && withoutLRBillSetup.BackDateAllow == false && withoutLRBillSetup.ForwardDateAllow == false)
            {
                Model.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
                Model.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
            }
            if (withoutLRBillSetup.CurrDatetOnlyreq == true)
            {
                Model.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
                Model.EndDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            if (withoutLRBillSetup.BackDateAllow == true)
            {
                Model.StartDate = (DateTime.Now.AddDays(-withoutLRBillSetup.BackDaysUpto)).ToString("yyyy-MM-dd");
            }
            if (withoutLRBillSetup.ForwardDateAllow == true)
            {
                Model.EndDate = (DateTime.Now.AddDays(withoutLRBillSetup.ForwardDaysUpto)).ToString("yyyy-MM-dd");
            }
            #endregion

            #region DocType
            var result = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => x).FirstOrDefault();
            var mpara = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x).FirstOrDefault();

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
            Model.IsRoundOff = result.RoundOff;

            Model.id = result.Name;
            Model.Prefix = mperiod;
            Model.GSTType = result.GSTType.ToString();
            Model.GstTypeName = GetGSTTypeName(Model.GSTType);

            Model.LocationCode = result.LocationCode;

            ViewData["DocAttach"] = result.DocAttach;
            #endregion

            if (Model.Mode == "Add")
            {
                Model.DocDate = DateTime.Now;
                Model.IGSTAmt = 0;
                Model.SGSTAmt = 0;
                Model.CGSTAmt = 0;
                Model.TotalQty = 0;
                Model.Taxable = 0;
                Model.InvoiceAmt = 0;
                Model.HeaderList = ctxTFAT.Charges.Where(x => x.Type == "LR000").Select(x => x.Head).ToList();
                Model.EquationList = ctxTFAT.Charges.Where(x => x.Type == "LR000").Select(x => x.EqAmt).ToList();
                Model.FLDList = ctxTFAT.Charges.Where(x => x.Type == "LR000").Select(x => x.Fld).ToList();
                Model.ChargeCodeList = ctxTFAT.Charges.Where(x => x.Type == "LR000").Select(x => x.Code).ToList();
                decimal[] mCharges2 = new decimal[(Model.HeaderList.Count)];
                mCharges2[0] = 0;
                mCharges2[1] = 0;
                mCharges2[2] = 0;
                mCharges2[3] = 0;
                mCharges2[4] = 0;
                mCharges2[5] = 0;
                mCharges2[6] = 0;
                mCharges2[7] = 0;
                mCharges2[8] = 0;
                mCharges2[9] = 0;
                mCharges2[10] = 0;
                mCharges2[11] = 0;
                mCharges2[12] = 0;
                mCharges2[13] = 0;
                mCharges2[14] = 0;
                mCharges2[15] = 0;
                mCharges2[16] = 0;
                mCharges2[17] = 0;
                mCharges2[18] = 0;
                mCharges2[19] = 0;
                mCharges2[20] = 0;
                mCharges2[21] = 0;
                mCharges2[22] = 0;
                mCharges2[23] = 0;
                mCharges2[24] = 0;
                Model.TotalChgPickupList = mCharges2.ToList();
                Model.TDSCode = 0;
                Model.GSTCode = null;

                Model.IntegerValue = 0;
                Model.SelectedIntegerValue = 0; 
            }
            else
            {

                var mLedger = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).OrderBy(x => x.Sno).FirstOrDefault();
                Model.PeriodLock = PeriodLock(mLedger.Branch, mLedger.Type, mLedger.DocDate);
                if (mLedger.AUTHORISE.Substring(0, 1) == "A")
                {
                    Model.LockAuthorise = LockAuthorise(mLedger.Type, Model.Mode, mLedger.TableKey, mLedger.ParentKey);
                }

                var mobj1 = ctxTFAT.Sales.Where(x => (x.TableKey) == Model.ParentKey && x.Branch == Model.Branch).FirstOrDefault();
                Model.Authorise = mobj1.AUTHORISE;
                var mdoctype = ctxTFAT.DocTypes.Where(x => x.Code == mobj1.Type).Select(x => new { x.DocWidth }).FirstOrDefault();
                Model.Srl = mobj1.Srl.PadLeft(mdoctype.DocWidth, '0');
                Model.Prefix = mobj1.Prefix;
                Model.DocDate = mobj1.DocDate.Value;
                Model.TDSCode = mobj1.TDSCode == null ? 0 : mobj1.TDSCode.Value;
                Model.TDSName = ctxTFAT.TDSMaster.Where(x => x.Code == Model.TDSCode).Select(x => x.Name).FirstOrDefault();
                Model.Branch = mobj1.Branch;
                Model.LocationCode = mobj1.LocationCode;
                Model.Account = mobj1.Code;
                Model.AccountName = (ctxTFAT.CustomerMaster.Where(x => x.Code == mobj1.Code).Select(x => x.Name).FirstOrDefault().ToString());
                Model.AltAddress = Convert.ToByte(mobj1.AltAddress);
                Model.AddressFrom = ctxTFAT.Caddress.Where(x => x.Code == mobj1.Code && x.Sno == mobj1.AltAddress).Select(x => x.Adrl1 + x.Adrl2 + x.Adrl3).FirstOrDefault();

                Model.IntegerValue = ctxTFAT.Caddress.Where(x => x.Code == mobj1.Code).ToList().Count;
                Model.SelectedIntegerValue = Model.AltAddress;

                Model.Document = Model.Document;
                Model.GSTType = (mobj1.GSTType == null) ? "0" : mobj1.GSTType.Value.ToString();
                Model.GstTypeName = GetGSTTypeName(Model.GSTType);
                Model.CrPeriod = mobj1.CrPeriod == null ? 0 : mobj1.CrPeriod.Value;
                Model.GSTCode = mobj1.TaxCode;
                Model.GSTCodeName = ctxTFAT.TaxMaster.Where(x => x.Code == Model.GSTCode).Select(x => x.Name).FirstOrDefault();
                if (String.IsNullOrEmpty(Model.GSTCodeName))
                {
                    Model.GSTCodeName = ctxTFAT.HSNMaster.Where(x => x.Code.ToString() == Model.GSTCode).Select(x => x.Name).FirstOrDefault();
                }

                Model.AccParentGrp = ctxTFAT.CustomerMaster.Where(x => x.Code == mobj1.Code).Select(x => x.AccountParentGroup).FirstOrDefault();
                Model.AccParentGrpName = ctxTFAT.Master.Where(x => x.Code == Model.AccParentGrp).Select(x => x.Name).FirstOrDefault();
                Model.BillNarr = mobj1.Narr;
                Model.CPerson = ctxTFAT.Caddress.Where(x => x.Code == mobj1.Code && x.Sno == 0).Select(x => x.Name).FirstOrDefault();
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
                var master = ctxTFAT.Master.Where(x => x.Code == Model.AccParentGrp).FirstOrDefault();
                if (master.FetchBalAcc == false)
                {
                    Branch = mbranchcode;
                }
                string mStr = @"select dbo.GetBalance('" + Model.AccParentGrp + "','" + MMDDYY(DateTime.Now) + "','" + Branch + "',0,0)";
                DataTable smDt = GetDataTable(mStr);
                double mBalance = 0;
                if (smDt.Rows.Count > 0)
                {
                    mBalance = Convert.ToDouble(smDt.Rows[0][0].ToString());
                }
                Model.Balance = mBalance;
                Model.IGSTAmt = mobj1.IGSTAmt == null ? 0 : mobj1.IGSTAmt.Value;
                Model.SGSTAmt = mobj1.SGSTAmt == null ? 0 : mobj1.SGSTAmt.Value;
                Model.CGSTAmt = mobj1.CGSTAmt == null ? 0 : mobj1.CGSTAmt.Value;
                if (Model.IGSTAmt > 0 || Model.CGSTAmt > 0 || Model.SGSTAmt > 0)
                {
                    Model.GSTFlag = true;
                }
                Model.TotalQty = mobj1.Qty == null ? 0 : Convert.ToDouble(mobj1.Qty.Value);
                Model.Taxable = mobj1.Taxable == null ? 0 : mobj1.Taxable.Value;
                Model.InvoiceAmt = Math.Abs(mobj1.Amt.Value);
                Model.HeaderList = ctxTFAT.Charges.Where(x => x.Type == "LR000").Select(x => x.Head).ToList();
                Model.EquationList = ctxTFAT.Charges.Where(x => x.Type == "LR000").Select(x => x.EqAmt).ToList();
                Model.FLDList = ctxTFAT.Charges.Where(x => x.Type == "LR000").Select(x => x.Fld).ToList();
                Model.ChargeCodeList = ctxTFAT.Charges.Where(x => x.Type == "LR000").Select(x => x.Code).ToList();

                decimal[] mCharges2 = new decimal[(Model.HeaderList.Count)];
                mCharges2[0] = mobj1.Val1 == null ? 0 : mobj1.Val1.Value;
                mCharges2[1] = mobj1.Val2 == null ? 0 : mobj1.Val2.Value;
                mCharges2[2] = mobj1.Val3 == null ? 0 : mobj1.Val3.Value;
                mCharges2[3] = mobj1.Val4 == null ? 0 : mobj1.Val4.Value;
                mCharges2[4] = mobj1.Val5 == null ? 0 : mobj1.Val5.Value;
                mCharges2[5] = mobj1.Val6 == null ? 0 : mobj1.Val6.Value;
                mCharges2[6] = mobj1.Val7 == null ? 0 : mobj1.Val7.Value;
                mCharges2[7] = mobj1.Val8 == null ? 0 : mobj1.Val8.Value;
                mCharges2[8] = mobj1.Val9 == null ? 0 : mobj1.Val9.Value;
                mCharges2[9] = mobj1.Val10 == null ? 0 : mobj1.Val10.Value;
                mCharges2[10] = mobj1.Val11 == null ? 0 : mobj1.Val11.Value;
                mCharges2[11] = mobj1.Val12 == null ? 0 : mobj1.Val12.Value;
                mCharges2[12] = mobj1.Val13 == null ? 0 : mobj1.Val13.Value;
                mCharges2[13] = mobj1.Val14 == null ? 0 : mobj1.Val14.Value;
                mCharges2[14] = mobj1.Val15 == null ? 0 : mobj1.Val15.Value;
                mCharges2[15] = mobj1.Val16 == null ? 0 : mobj1.Val16.Value;
                mCharges2[16] = mobj1.Val17 == null ? 0 : mobj1.Val17.Value;
                mCharges2[17] = mobj1.Val18 == null ? 0 : mobj1.Val18.Value;
                mCharges2[18] = mobj1.Val19 == null ? 0 : mobj1.Val19.Value;
                mCharges2[19] = mobj1.Val20 == null ? 0 : mobj1.Val20.Value;
                //mCharges2[20] = mobj1.Val21 == null ? 0 : mobj1.Val21.Value;
                //mCharges2[21] = mobj1.Val22 == null ? 0 : mobj1.Val22.Value;
                //mCharges2[22] = mobj1.Val23 == null ? 0 : mobj1.Val23.Value;
                //mCharges2[23] = mobj1.Val24 == null ? 0 : mobj1.Val24.Value;
                //mCharges2[24] = mobj1.Val25 == null ? 0 : mobj1.Val25.Value;

                Model.TotalChgPickupList = mCharges2.ToList();

                #region ATTACHMENT
                AttachmentVM Att = new AttachmentVM();
                Att.Type = "SLW00";
                Att.Srl = Model.Srl.ToString();

                AttachmentController attachmentC = new AttachmentController();
                List<AttachmentVM> attachments = attachmentC.GetAttachmentListInEdit(Att);
                Session["TempAttach"] = attachments;
                #endregion
            }

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



            var tdsdetails = ctxTFAT.TDSPayments.Where(x => x.ParentKey == Model.ParentKey).Select(x => x).FirstOrDefault();
            Model.CutTDS = (tdsdetails != null) ? true : false;
            Model.TDSAmt = (tdsdetails != null) ? tdsdetails.TDSAmt.Value : 0;
            Model.TDSRate = (tdsdetails != null) ? tdsdetails.TDSTax == null ? 0 : tdsdetails.TDSTax.Value : 0;
            Model.TDSCode = (tdsdetails != null) ? tdsdetails.TDSCode.Value : 0;
            Model.TDSCodeName = ctxTFAT.TDSMaster.Where(x => x.Code.ToString() == Model.TDSCode.ToString()).Select(x => x.Name).FirstOrDefault();
            if (Model.TDSAmt > 0)
            {
                Model.TDSFlag = true;
            }
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
                //Model.Mode = "View";
                Model.Message = "Document is Already Authorised Cant Edit";
            }
            if (Model.Authorise.Substring(0, 1) == "X")
            {
                Model.IsDraftSave = true;
                Model.Message = "Document is saved As Draft";
            }
            return View(Model);
        }

        #region Calculate GST and TDS

        public ActionResult GetGSTCalculation(FreightBillVM Model)
        {
            string pgst;

            string ourstatename = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.State).FirstOrDefault();
            if (ourstatename == null || ourstatename == "" || ourstatename == "Maharashtra")
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
                #region Calculate Gst
                Model.CGSTAmt = Math.Round(Model.Taxable * (Model.CGSTRate / 100), 2);
                Model.SGSTAmt = Math.Round(Model.Taxable * (Model.SGSTRate / 100), 2);
                Model.IGSTAmt = Math.Round(Model.Taxable * (Model.IGSTRate / 100), 2);
                Model.InvoiceAmt = Math.Round(Model.Taxable + Model.IGSTAmt + Model.SGSTAmt + Model.CGSTAmt, 2);
                #endregion

                var datenow = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                var TDSRATEtab = ctxTFAT.TDSRates.Where(x => x.Code == Model.TDSCode && x.EffDate <= datenow && x.LimitFrom <= Model.Taxable).OrderByDescending(x => x.EffDate).Select(x => new { x.TDSRate, x.Cess, x.SHECess, x.SurCharge }).FirstOrDefault();
                if (TDSRATEtab != null)
                {
                    Model.TDSAmt = (TDSRATEtab.TDSRate == null ? 0 : Math.Round(((TDSRATEtab.TDSRate.Value * Model.Taxable) / 100), 0));
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

                Model.CGSTAmt = Math.Round(Model.Taxable * (Model.CGSTRate / 100), 2);
                Model.SGSTAmt = Math.Round(Model.Taxable * (Model.SGSTRate / 100), 2);
                Model.IGSTAmt = Math.Round(Model.Taxable * (Model.IGSTRate / 100), 2);
                Model.InvoiceAmt = Math.Round(Model.Taxable + Model.IGSTAmt + Model.SGSTAmt + Model.CGSTAmt, 2);

                var datenow = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                var TDSRATEtab = ctxTFAT.TDSRates.Where(x => x.Code == Model.TDSCode && x.EffDate <= datenow && x.LimitFrom <= Model.Taxable).OrderByDescending(x => x.EffDate).Select(x => new { x.TDSRate, x.Cess, x.SHECess, x.SurCharge }).FirstOrDefault();
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

        #endregion

        #region LedgerPost

        public ActionResult GetPostingNew(FreightBillVM Model)
        {

            var Date = ConvertDDMMYYTOYYMMDD(Model.DocuDate);

            if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.Type == "SLW00" && x.LockDate == Date).FirstOrDefault() != null)
            {
                return Json(new
                {

                    Status = "ErrorValid",
                    Message = "Date is Locked By Period Lock System...!"
                }, JsonRequestBehavior.AllowGet); ;
            }
            if (!String.IsNullOrEmpty(Model.Srl))
            {
                var resultw = ctxTFAT.DocTypes.Where(x => x.Code == "SLW00").Select(x => x).FirstOrDefault();
                if (Model.Srl.Length > (resultw.DocWidth))
                {
                    return Json(new
                    {

                        Status = "ErrorValid",
                        Message = "Document NO. Allow " + resultw.DocWidth + " Digit Only....!"
                    }, JsonRequestBehavior.AllowGet); ;
                }
            }


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



            string Status2 = "Success", Message2 = "";

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
            List<FreightBillVM> mTaxPostCode2 = new List<FreightBillVM>();

            bool mRCM = false;
            if (Model.GSTType == "1")
            {
                mRCM = true;
            }

            List<FreightBillVM> mProdPostCode2 = new List<FreightBillVM>();

            // create item wise posting array

            mMultTax = Model.SGSTAmt + Model.CGSTAmt + Model.IGSTAmt;// + mobj.CessAmt

            mTaxable = Model.Taxable;


            //--------------- GST posting starts
            if ((mTrx == "P" && mRCM == false && mTradef == false) || mTrx == "S")
            {
                // igst
                if (Model.IGSTAmt != 0)
                {
                    mTaxPostCode2.Add(new FreightBillVM()
                    {
                        TaxCode = "000100037",
                        TaxAmt = Model.IGSTAmt
                    });
                }
                // cgst
                if (Model.CGSTAmt != 0)
                {
                    mTaxPostCode2.Add(new FreightBillVM()
                    {
                        TaxCode = "000100036",
                        TaxAmt = Model.CGSTAmt
                    });
                }
                // sgst
                if (Model.SGSTAmt != 0)
                {
                    mTaxPostCode2.Add(new FreightBillVM()
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
                //}).Where(z => z.Scope == Model.MainType.Substring(0, 1) && z.Code == Model.GSTCode).FirstOrDefault();
                //if (mtaxs != null)
                //{
                //    // igst
                //    if (Model.IGSTAmt != 0)
                //    {

                //        mTaxPostCode2.Add(new FreightBillVM()
                //        {
                //            TaxCode = mtaxs.IGSTCode,
                //            TaxAmt = Model.IGSTAmt
                //        });
                //    }
                //    // cgst
                //    if (Model.CGSTAmt != 0)
                //    {

                //        mTaxPostCode2.Add(new FreightBillVM()
                //        {
                //            TaxCode = mtaxs.CGSTCode,
                //            TaxAmt = Model.CGSTAmt
                //        });
                //    }
                //    // sgst
                //    if (Model.SGSTAmt != 0)
                //    {


                //        mTaxPostCode2.Add(new FreightBillVM()
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
                //            mTaxPostCode2.Add(new FreightBillVM()
                //            {
                //                TaxCode = hSNMaster.IGSTOut,
                //                TaxAmt = Model.IGSTAmt
                //            });
                //        }
                //        // cgst
                //        if (Model.CGSTAmt != 0)
                //        {
                //            mTaxPostCode2.Add(new FreightBillVM()
                //            {
                //                TaxCode = hSNMaster.CGSTOut,
                //                TaxAmt = Model.CGSTAmt
                //            });
                //        }
                //        // sgst
                //        if (Model.SGSTAmt != 0)
                //        {
                //            mTaxPostCode2.Add(new FreightBillVM()
                //            {
                //                TaxCode = hSNMaster.SGSTOut,
                //                TaxAmt = Model.SGSTAmt
                //            });
                //        }
                //    }
                //}
            }

            // ----- actual posting routine starts from here
            xCnt = 1;

            mvalue = mTaxable;

            List<FreightBillVM> LedPostList = new List<FreightBillVM>();

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
                LedPostList.Add(new FreightBillVM()
                {
                    Code = Model.AccParentGrp,
                    AccountName = ctxTFAT.Master.Where(x => x.Code == Model.AccParentGrp).Select(x => x.Name).FirstOrDefault(),
                    Debit = Math.Round(mDebit, 2),
                    Credit = Math.Round(mCredit, 2),
                    Branch = mbranchcode,
                    FilBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                    tempId = xCnt,
                    DelyCode = Model.AccParentGrp
                });

            }
            else
            {
                LedPostList.Add(new FreightBillVM()
                {
                    Code = Model.AccParentGrp,
                    AccountName = ctxTFAT.Master.Where(x => x.Code == Model.AccParentGrp).Select(x => x.Name).FirstOrDefault(),
                    Debit = Math.Round(mCredit, 2),
                    Credit = Math.Round(mDebit, 2),
                    Branch = mbranchcode,
                    FilBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                    tempId = xCnt,
                    DelyCode = Model.AccParentGrp
                });
            }
            xCnt++;


            // Charges Updations
            n = 0;

            List<FreightBillVM> mCharges = Model.Charges;

            var mCharges2 = mCharges.GroupBy(x => x.Fld).Select(x => new FreightBillVM()
            {
                AddLess = x.Select(x1 => x1.AddLess).First(),
                Header = x.Select(x1 => x1.Header).First(),
                ChgPostCode = x.Select(x1 => x1.ChgPostCode).First(),
                Amt = x.Select(x1 => x1.Amt).Sum()
            }).ToList();

            var mchg = mCharges2.Where(x => x.AddLess == "+" || x.AddLess == "-").Select(x => x).ToList();
            foreach (FreightBillVM mc in mchg)
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
                        LedPostList.Add(new FreightBillVM()
                        {
                            Code = mc.ChgPostCode,
                            AccountName = NameofAccount(mc.ChgPostCode),
                            Debit = Math.Round(mDebit, 2),
                            Credit = Math.Round(mCredit, 2),
                            Branch = mbranchcode,
                            FilBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                            tempId = xCnt++,
                            DelyCode = Model.AccParentGrp
                        });
                    }
                    else
                    {
                        LedPostList.Add(new FreightBillVM()
                        {
                            Code = mc.ChgPostCode,
                            AccountName = NameofAccount(mc.ChgPostCode),
                            Debit = Math.Round(mCredit, 2),
                            Credit = Math.Round(mDebit, 2),
                            Branch = mbranchcode,
                            FilBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                            tempId = xCnt++,
                            DelyCode = Model.AccParentGrp
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
                            LedPostList.Add(new FreightBillVM()
                            {
                                Code = mCode,
                                AccountName = NameofAccount(mCode),
                                Debit = Math.Round(mDebit, 2),
                                Credit = Math.Round(mCredit, 2),
                                Branch = mbranchcode,
                                FilBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                tempId = xCnt++,
                                DelyCode = Model.AccParentGrp
                            });
                        }
                        else
                        {
                            LedPostList.Add(new FreightBillVM()
                            {
                                Code = mCode,
                                AccountName = NameofAccount(mCode),
                                Debit = Math.Round(mCredit, 2),
                                Credit = Math.Round(mDebit, 2),
                                Branch = mbranchcode,
                                FilBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                tempId = xCnt++,
                                DelyCode = Model.AccParentGrp
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
                    LedPostList.Add(new FreightBillVM()
                    {
                        Code = mCode,
                        AccountName = NameofAccount(mCode),
                        Debit = Math.Round(mDebit, 2),
                        Credit = Math.Round(mCredit, 2),
                        Branch = mbranchcode,
                        FilBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                        tempId = xCnt++,
                        DelyCode = Model.AccParentGrp
                    });
                }
                else
                {
                    LedPostList.Add(new FreightBillVM()
                    {
                        Code = mCode,
                        AccountName = NameofAccount(mCode),
                        Debit = Math.Round(mCredit, 2),
                        Credit = Math.Round(mDebit, 2),
                        Branch = mbranchcode,
                        FilBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                        tempId = xCnt++,
                        DelyCode = Model.AccParentGrp
                    });
                }
            }

            var StringAltcode = String.Join(",", LedPostList.Where(x => x.Code != x.DelyCode).Select(x => x.Code).ToList());
            var UpdateLedgerPost = LedPostList.Where(x => x.Code == x.DelyCode).ToList();
            if (UpdateLedgerPost != null)
            {
                foreach (var item in UpdateLedgerPost)
                {
                    item.DelyCode = StringAltcode;
                }
            }

            decimal mTDS = Model.TDSAmt;
            if (mTDS != 0)
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

                //var mTdsPostCode = ctxTFAT.TDSMaster.Where(x => x.Code == Model.TDSCode).Select(x => x.PostCode).FirstOrDefault();
                var mTdsPostCode = "000009992";
                //if (mTdsPostCode == null || mTdsPostCode.Trim() == "")
                //{
                //    mTdsPostCode = "";
                //}

                LedPostList.Add(new FreightBillVM()
                {
                    Code = Model.AccParentGrp,
                    AccountName = NameofAccount(Model.AccParentGrp),
                    Debit = Math.Round(mDebit, 2),
                    Credit = Math.Round(mCredit, 2),
                    Branch = mbranchcode,
                    FilBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                    tempId = xCnt++,
                    DelyCode = mTdsPostCode
                });


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
                LedPostList.Add(new FreightBillVM()
                {
                    Code = mTdsPostCode,
                    AccountName = NameofAccount(mTdsPostCode),
                    Debit = Math.Round(mDebit, 2),
                    Credit = Math.Round(mCredit, 2),
                    Branch = mbranchcode,
                    FilBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                    TDSFlag = true,
                    tempId = xCnt++,
                    DelyCode = Model.AccParentGrp
                });

            }


            Model.TotDebit = LedPostList.Sum(x => x.Debit);
            Model.TotCredit = LedPostList.Sum(x => x.Credit);
            Model.LedgerPostList = LedPostList;

            // display view form
            var html = ViewHelper.RenderPartialView(this, "LedgerPosting", new FreightBillVM() { LedgerPostList = Model.LedgerPostList, TotDebit = Math.Round(Model.TotDebit, 2), TotCredit = Math.Round(Model.TotCredit, 2), Mode = Model.Mode, Controller2 = Model.Controller2, Type = Model.Type, Module = Model.Module, ViewDataId = Model.ViewDataId, TableName = Model.TableName, Header = Model.Header, optiontype = Model.optiontype, OptionCode = Model.OptionCode, MainType = Model.MainType, SubType = Model.SubType });
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

        #region Save And Delete

        public void DeUpdate(FreightBillVM Model)
        {
            string connstring = GetConnectionString();

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

            var mDeleteNote = ctxTFAT.Narration.Where(x => x.TableKey == Model.ParentKey && x.Branch == Model.Branch).ToList();
            if (mDeleteNote != null)
            {
                ctxTFAT.Narration.RemoveRange(mDeleteNote);
            }


            var mDeleteAttach = ctxTFAT.Attachment.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).ToList();
            if (mDeleteAttach != null)
            {
                ctxTFAT.Attachment.RemoveRange(mDeleteAttach);
            }

            var mDeleteAuthorise = ctxTFAT.Authorisation.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).ToList();
            if (mDeleteAuthorise != null)
            {
                ctxTFAT.Authorisation.RemoveRange(mDeleteAuthorise);
            }



            var mtdspayment = ctxTFAT.TDSPayments.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).Select(x => x).FirstOrDefault();
            if (mtdspayment != null)
            {
                ctxTFAT.TDSPayments.Remove(mtdspayment);
            }

            var moutstanding = ctxTFAT.Outstanding.Where(x => x.ParentKey == Model.ParentKey && x.DocBranch == Model.Branch).Select(x => x).ToList();
            if (moutstanding != null)
            {
                ctxTFAT.Outstanding.RemoveRange(moutstanding);
            }

            ctxTFAT.SaveChanges();
        }

        public ActionResult SaveData(FreightBillVM Model)
        {
            string mTable = "";
            string brMessage = "";
            var Setup = ctxTFAT.WithoutLRBillSetup.FirstOrDefault();

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
                            var result1 = ctxTFAT.DocTypes.Where(x => x.Code == "SLW00").Select(x => x).FirstOrDefault();
                            Model.Srl = Model.Srl.PadLeft(result1.DocWidth, '0');
                        }
                        else
                        {
                            if (Setup.MergeSerial)
                            {
                                Model.Srl = GetLastSerial(Model.TableName, Model.Branch, Model.Type, Model.Prefix, Model.SubType, ConvertDDMMYYTOYYMMDD(Model.DocuDate),true,"'SLR00','SLW00'");
                            }
                            else
                            {
                                Model.Srl = GetLastSerial(Model.TableName, Model.Branch, Model.Type, Model.Prefix, Model.SubType, ConvertDDMMYYTOYYMMDD(Model.DocuDate));
                            }
                        }
                        Model.ParentKey =mbranchcode+ Model.Type + Model.Prefix.Substring(0, 2) + Model.Srl.ToString();
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
                        AlertNoteMaster alertNoteMaster = ctxTFAT.AlertNoteMaster.Where(x => x.Type == "SLW00" && x.TypeCode.Trim() == Model.Srl.Trim() && x.Stop.Contains("SLW00")).FirstOrDefault();
                        if (alertNoteMaster != null)
                        {
                            return Json(new
                            {
                                Message = Model.Srl.Trim(),
                                Status = "AutoDocumentAlert"
                            }, JsonRequestBehavior.AllowGet);
                        }

                        AttachmentVM vM = new AttachmentVM();
                        vM.ParentKey = Model.ParentKey;
                        vM.Srl = Model.Srl.ToString();
                        vM.Type = "SLW00";
                        SaveAttachment(vM);

                        SaveNarrationAdd(Model.Srl.ToString(), Model.ParentKey);
                    }

                    //if (Model.Authorise.Substring(0, 1) == "X" || Model.Authorise.Substring(0, 1) == "R" || Model.Mode == "Add" || (Model.Mode == "Edit" && Model.AuthAgain))
                    //{
                    //    if (Model.Authorise.Substring(0, 1) != "N") //if authorised then check for the RateDiff Auth. Rule
                    //        Model.Authorise = (Model.AuthReq == true ? GetAuthorise(Model.Type, 0, Model.Branch) : Model.Authorise = "A00");
                    //}

                    #region Authorisation
                    TfatUserAuditHeader authorisation = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == "SLW00").FirstOrDefault();
                    if (authorisation != null)
                    {
                        mauthorise = SetAuthorisationLogistics(authorisation, Model.ParentKey, Model.Srl.ToString(), 0, Model.DocuDate, Model.Amt, Model.Account, mbranchcode);
                    }
                    #endregion





                    List<FreightBillVM> result = new List<FreightBillVM>();
                    result = Model.Charges;


                    if (Model.Mode == "Add" || Model.Mode == "Edit")
                    {

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
                                mobj1.AUTHORISE = mauthorise;
                                mobj1.BillDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                mobj1.BillNumber = (Model.BillNumber == null || Model.BillNumber.Trim() == "") ? Model.Srl : Model.BillNumber;
                                mobj1.Branch = Model.Branch;
                                mobj1.Broker = "";
                                mobj1.Cess = Convert.ToDecimal(0.00) * mamtvalneg;
                                mobj1.ChlnDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                mobj1.ChlnNumber = "";
                                mobj1.Code = Model.Account;
                                mobj1.CustGroup = Model.AccParentGrp;
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
                                mobj1.Taxable = Convert.ToDecimal(list.Select(x => x.Amt).Sum()) * mamtvalneg;
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
                                mobj1.TDSCode = Model.TDSCode;
                                mobj1.CompCode = mcompcode;
                                mobj1.TableKey = Model.ParentKey;
                                mobj1.RoundOff = Model.RoundOff * mamtvalneg;//check sds
                                mobj1.SourceDoc = Model.SourceDoc;
                                mobj1.PCCode = 100002;
                                mobj1.DCCode = 100001;
                                mobj1.Val1 = result.Where(x => x.Fld == "F001").Select(x => x.Amt).FirstOrDefault();
                                mobj1.Val2 = result.Where(x => x.Fld == "F002").Select(x => x.Amt).FirstOrDefault();
                                mobj1.Val3 = result.Where(x => x.Fld == "F003").Select(x => x.Amt).FirstOrDefault();
                                mobj1.Val4 = result.Where(x => x.Fld == "F004").Select(x => x.Amt).FirstOrDefault();
                                mobj1.Val5 = result.Where(x => x.Fld == "F005").Select(x => x.Amt).FirstOrDefault();
                                mobj1.Val6 = result.Where(x => x.Fld == "F006").Select(x => x.Amt).FirstOrDefault();
                                mobj1.Val7 = result.Where(x => x.Fld == "F007").Select(x => x.Amt).FirstOrDefault();
                                mobj1.Val8 = result.Where(x => x.Fld == "F008").Select(x => x.Amt).FirstOrDefault();
                                mobj1.Val9 = result.Where(x => x.Fld == "F009").Select(x => x.Amt).FirstOrDefault();
                                mobj1.Val10 = result.Where(x => x.Fld == "F010").Select(x => x.Amt).FirstOrDefault();
                                mobj1.Val11 = result.Where(x => x.Fld == "F011").Select(x => x.Amt).FirstOrDefault();
                                mobj1.Val12 = result.Where(x => x.Fld == "F012").Select(x => x.Amt).FirstOrDefault();
                                mobj1.Val13 = result.Where(x => x.Fld == "F013").Select(x => x.Amt).FirstOrDefault();
                                mobj1.Val14 = result.Where(x => x.Fld == "F014").Select(x => x.Amt).FirstOrDefault();
                                mobj1.Val15 = result.Where(x => x.Fld == "F015").Select(x => x.Amt).FirstOrDefault();
                                mobj1.Val16 = result.Where(x => x.Fld == "F016").Select(x => x.Amt).FirstOrDefault();
                                mobj1.Val17 = result.Where(x => x.Fld == "F017").Select(x => x.Amt).FirstOrDefault();
                                mobj1.Val18 = result.Where(x => x.Fld == "F018").Select(x => x.Amt).FirstOrDefault();
                                mobj1.Val19 = result.Where(x => x.Fld == "F019").Select(x => x.Amt).FirstOrDefault();
                                mobj1.Val20 = result.Where(x => x.Fld == "F020").Select(x => x.Amt).FirstOrDefault();
                                //mobj1.Val21 = result.Where(x => x.Fld == "F021").Select(x => x.Amt).FirstOrDefault();
                                //mobj1.Val22 = result.Where(x => x.Fld == "F022").Select(x => x.Amt).FirstOrDefault();
                                //mobj1.Val23 = result.Where(x => x.Fld == "F023").Select(x => x.Amt).FirstOrDefault();
                                //mobj1.Val24 = result.Where(x => x.Fld == "F024").Select(x => x.Amt).FirstOrDefault();
                                //mobj1.Val25 = result.Where(x => x.Fld == "F025").Select(x => x.Amt).FirstOrDefault();


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
                                mobj2.AUTHORISE = mauthorise;
                                mobj2.LASTUPDATEDATE = DateTime.Now;

                                ctxTFAT.SalesMore.Add(mobj2);

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
                                    mobjL.AUTHORISE = mauthorise;
                                    mobjL.BillDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                    mobjL.BillNumber = (Model.BillNumber == null || Model.BillNumber.Trim() == "") ? Model.Srl : Model.BillNumber;
                                    mobjL.Branch = Model.Branch;
                                    mobjL.Cheque = "";
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
                                    mobjL.Narr = Model.BillNarr;
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
                                    mobjL.TDSCode = Model.TDSCode;
                                    mobjL.TDSFlag = ledpost[u].TDSFlag;
                                    mobjL.Type = Model.Type;
                                    mobjL.ENTEREDBY = muserid;
                                    mobjL.LASTUPDATEDATE = DateTime.Now;
                                    mobjL.ChequeDate = DateTime.Now;
                                    mobjL.CompCode = mcompcode;
                                    mobjL.ParentKey = Model.ParentKey;
                                    mobjL.TableKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + lCnt.ToString("D3") + Model.Srl.ToString(); ;
                                    mobjL.PCCode = 100002;
                                    ctxTFAT.Ledger.Add(mobjL);


                                    ++lCnt;
                                }
                            }


                            #endregion


                        }
                        #endregion

                        SaveNarration(Model, Model.ParentKey);
                        if (Model.TDSAmt > 0)
                        {
                            Model.Authorise = mauthorise;
                            SaveTDSPayments(Model);
                        }
                        ctxTFAT.SaveChanges();
                        transaction.Commit();
                        transaction.Dispose();

                        UpdateAuditTrail(Model.Branch, Model.Mode, Model.Header, Model.ParentKey, ConvertDDMMYYTOYYMMDD(Model.DocuDate), Model.Amt, Model.Account, "Save Freight Bill (Without LR)", "CA");
                        //if (Model.Authorise.Substring(0, 1) != "A")
                        //{
                        //    if (Model.Authorise.Substring(0, 1) != "X")
                        //    {
                        //        string mAuthUser;
                        //        if (Model.Authorise.Substring(0, 1) == "D")
                        //        {
                        //            //mAuthUser=SaveAuthoriseDOC( Model.ParentKey, 0, Model.Date, mcurrency, Model.Branch, muserid);
                        //        }
                        //        else
                        //        {
                        //            mAuthUser = SaveAuthorise(Model.ParentKey, Model.Amt, Model.DocuDate, 1, 1, DateTime.Now, Model.Account, Model.Branch, muserid, -1);
                        //            SendAuthoriseMessage(mAuthUser, Model.ParentKey, Model.DocuDate, Model.Authorise, Model.AccountName);
                        //        }
                        //    }

                        //}

                        if (Model.Authorise != "X00")
                        {
                            //SendTrnsMsg(Model.Mode, Model.Amt, Model.Branch + Model.ParentKey, ConvertDDMMYYTOYYMMDD(Model.DocuDate), Model.Account);
                        }

                        if (Model.SaveAsDraft != "Y")
                        {
                            Session["FNewItemlist"] = null;
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
                NewSrl = (mbranchcode + Model.ParentKey),
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }

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
                    att.Type = String.IsNullOrEmpty(item.Type) == true ? "SLW00" : item.Type;
                    att.ParentKey = Model.ParentKey;

                    att.CompCode = mcompcode;
                    att.ExternalAttach = item.ExternalAttach;
                    att.RefDocNo = item.RefCode;
                    att.RefType = item.RefType;
                    ctxTFAT.Attachment.Add(att);
                    ctxTFAT.SaveChanges();
                    System.IO.File.WriteAllBytes(directoryPath, IData);

                    ++AttachCode;
                    ++J;
                }
            }

        }

        public ActionResult DeleteData(FreightBillVM Model)
        {
            // Check for Active Documents
            string mactivestring = "";
            if (Model.SubType == "RS" || Model.SubType == "XS")
            {
                var TableKeys = ctxTFAT.Stock.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).Select(x => x.TableKey).ToList();
                var mactive1 = ctxTFAT.Stock.Where(x => TableKeys.Contains(x.InvKey)).Select(x => x.TableKey).FirstOrDefault();
                if (mactive1 != null)
                {
                    mactivestring = mactivestring + "\nInvoice: " + mactive1;
                }
            }
            else if (Model.SubType == "NP" || Model.SubType == "NS")
            {
                var TableKeys = ctxTFAT.Stock.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).Select(x => x.TableKey).ToList();
                var mactive1 = ctxTFAT.Stock.Where(x => TableKeys.Contains(x.InvKey)).Select(x => x.TableKey).FirstOrDefault();
                if (mactive1 != null)
                {
                    mactivestring = mactivestring + "\nInvoice: " + mactive1;
                }
            }
            else if (Model.SubType == "SX" || Model.SubType == "PX")
            {
                var TableKeys = ctxTFAT.Stock.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).Select(x => x.TableKey).ToList();
                var mactive1 = ctxTFAT.Stock.Where(x => TableKeys.Contains(x.ChlnKey)).Select(x => x.TableKey).FirstOrDefault();
                if (mactive1 != null)
                {
                    mactivestring = mactivestring + "\nInvoice: " + mactive1;
                }
            }


            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "The Document is already Adjusted, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var Date = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.Type == "SLW00" && x.LockDate == Date).FirstOrDefault() != null)
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
                        Model.Account = mobj1.Code;
                        var RemoveAttach = ctxTFAT.Attachment.Where(x => x.Type == "SLW00" && x.Srl == mobj1.Srl).ToList();
                        foreach (var item in RemoveAttach)
                        {
                            if (System.IO.File.Exists(item.FilePath))
                            {
                                System.IO.File.Delete(item.FilePath);
                            }
                        }
                        ctxTFAT.Attachment.RemoveRange(RemoveAttach);

                        var GetRemarkDocList = ctxTFAT.Narration.Where(x => x.Srl == mobj1.Srl.ToString() && x.Type == "SLW00").ToList();
                        if (GetRemarkDocList != null)
                        {
                            ctxTFAT.Narration.RemoveRange(GetRemarkDocList);
                        }

                    }
                    DeUpdate(Model);

                    UpdateAuditTrail(mbranchcode, "Delete", Model.Header, Model.ParentKey, ConvertDDMMYYTOYYMMDD(Model.DocuDate), Model.Amt, Model.Account, "Delete Freight Bill (Without LR)", "CA");

                    transaction.Commit();
                    transaction.Dispose();
                    //SendTrnsMsg("Delete", Model.Amt, mbranchcode + Model.ParentKey, ConvertDDMMYYTOYYMMDD(Model.DocuDate), "");
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

        private void SaveTDSPayments(FreightBillVM Model)
        {
            var tdsrates = ctxTFAT.TDSRates.Where(x => x.Code == Model.TDSCode).Select(x => new { x.Tax, x.Cess, x.SHECess, x.SurCharge, x.TDSRate }).FirstOrDefault();

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
            tspay.TDSCode = Model.TDSCode;
            tspay.TDSReason = 0;
            tspay.TDSSheCess = tdsrates == null ? 0 : tdsrates.SHECess;
            tspay.TDSSheCessAmt = Model.TDSSHECess;
            tspay.TDSSurCharge = tdsrates == null ? 0 : tdsrates.SurCharge;
            tspay.TDSSurChargeAmt = Model.TDSSchg;
            tspay.TDSTax = tdsrates == null ? 0 : tdsrates.TDSRate;
            tspay.TDSTaxAmt = 0;
            tspay.TotalTDSAmt = Model.TDSAmt;
            tspay.Type = Model.Type;
            tspay.ENTEREDBY = muserid;
            tspay.LASTUPDATEDATE = DateTime.Now;
            tspay.AUTHORISE = Model.Authorise;
            tspay.AUTHIDS = muserid;
            ctxTFAT.TDSPayments.Add(tspay);


            var mBalCntNoCred = Model.LedgerPostList.Where(x => x.Code == Model.AccParentGrp && x.Credit == Model.TDSAmt).Select(x => x.tempId).FirstOrDefault();


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
            osobj1.AUTHORISE = Model.Authorise;
            osobj1.BillDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            osobj1.BillNumber = "";
            osobj1.CompCode = mcompcode;
            osobj1.Broker = 100001;
            osobj1.Brokerage = Convert.ToDecimal(0.00);
            osobj1.BrokerAmt = Convert.ToDecimal(0.00);
            osobj1.BrokerOn = Convert.ToDecimal(0.00);
            osobj1.ChlnDate = DateTime.Now;
            osobj1.ChlnNumber = "";
            osobj1.Code = Model.AccParentGrp;
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
            osobj2.AUTHORISE = Model.Authorise;
            osobj2.BillDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            osobj2.BillNumber = "";
            osobj2.CompCode = mcompcode;
            osobj2.Broker = 100001;
            osobj2.Brokerage = Convert.ToDecimal(0.00);
            osobj2.BrokerAmt = Convert.ToDecimal(0.00);
            osobj2.BrokerOn = Convert.ToDecimal(0.00);
            osobj2.ChlnDate = DateTime.Now;
            osobj2.ChlnNumber = "";
            osobj2.Code = Model.AccParentGrp;
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

        private void SaveNarration(FreightBillVM Model, string ParentKey)
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

        #endregion


        #region attachment
        public ActionResult UploadFile()
        {
            ViewBag.ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
            return Json(new { Status = "Success", Controller = ViewBag.ControllerName }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Not Required Functions

        public ActionResult GetAllRemarkOfDocument(string BillNo)
        {
            List<LoadingToDispatchVM> objledgerdetail = new List<LoadingToDispatchVM>();

            if (Session["Narrlist"] != null)
            {
                objledgerdetail = (List<LoadingToDispatchVM>)Session["Narrlist"];
            }
            if (objledgerdetail == null)
            {
                objledgerdetail = new List<LoadingToDispatchVM>();
            }
            var html = ViewHelper.RenderPartialView(this, "_DocumentNarrList", objledgerdetail);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveNarration(LoadingToDispatchVM Model)
        {
            string html = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    List<LoadingToDispatchVM> objledgerdetail = new List<LoadingToDispatchVM>();
                    LoadingToDispatchVM NewNarr = new LoadingToDispatchVM();

                    if (Session["Narrlist"] != null)
                    {
                        objledgerdetail = (List<LoadingToDispatchVM>)Session["Narrlist"];
                    }
                    if (objledgerdetail == null)
                    {
                        objledgerdetail = new List<LoadingToDispatchVM>();
                    }

                    if (Model.NarrStr != null)
                    {

                        if (Model.Mode != "Add")
                        {
                            Sales fM = ctxTFAT.Sales.Where(x => x.Srl.ToString() == Model.FMNO && x.Type == "SLW00").FirstOrDefault();
                            var LastSno = ctxTFAT.Narration.Where(x => x.ParentKey == fM.TableKey).ToList().Count();
                            ++LastSno;

                            Narration narr = new Narration();
                            narr.Branch = mbranchcode;
                            narr.Narr = Model.NarrStr;
                            narr.NarrRich = Model.Header;
                            narr.Prefix = mperiod;
                            narr.Sno = LastSno;
                            narr.Srl = fM.Srl.ToString();
                            narr.Type = "SLW00";
                            narr.ENTEREDBY = muserid;
                            narr.LASTUPDATEDATE = DateTime.Now;
                            narr.AUTHORISE = "A00";
                            narr.AUTHIDS = muserid;
                            narr.LocationCode = 0;
                            narr.TableKey = mbranchcode + "SLW00" + mperiod.Substring(0, 2) + LastSno.ToString("D3") + fM.Srl.ToString();
                            narr.CompCode = mcompcode;
                            narr.ParentKey = fM.TableKey;
                            ctxTFAT.Narration.Add(narr);
                            ctxTFAT.SaveChanges();
                            transaction.Commit();
                            transaction.Dispose();

                            NewNarr.FMNO = fM.Srl.ToString();
                            NewNarr.AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault();
                            NewNarr.NarrStr = Model.NarrStr;
                            NewNarr.ENTEREDBY = muserid;
                            NewNarr.NarrSno = objledgerdetail.Count() + 1;
                            NewNarr.PayLoadL = Model.Header;
                            objledgerdetail.Add(NewNarr);
                        }
                        else
                        {
                            NewNarr.FMNO = Model.FMNO.ToString();
                            NewNarr.AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault();
                            NewNarr.NarrStr = Model.NarrStr;
                            NewNarr.ENTEREDBY = muserid;
                            NewNarr.NarrSno = objledgerdetail.Count() + 1;
                            NewNarr.PayLoadL = Model.Header;
                            objledgerdetail.Add(NewNarr);
                        }

                        Session["Narrlist"] = objledgerdetail;

                        html = ViewHelper.RenderPartialView(this, "_DocumentNarrList", objledgerdetail);
                    }
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Html = html, Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteNarr(LoadingToDispatchVM mModel)
        {
            string html = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    List<LoadingToDispatchVM> objledgerdetail = new List<LoadingToDispatchVM>();
                    if (Session["Narrlist"] != null)
                    {
                        objledgerdetail = (List<LoadingToDispatchVM>)Session["Narrlist"];
                    }
                    if (objledgerdetail == null)
                    {
                        objledgerdetail = new List<LoadingToDispatchVM>();
                    }

                    if (mModel.Mode != "Add")
                    {
                        Narration narration = ctxTFAT.Narration.Where(x => x.Sno.ToString() == mModel.NarrSno.ToString() && x.Type == "SLW00").FirstOrDefault();
                        if (narration != null)
                        {
                            ctxTFAT.Narration.Remove(narration);
                            ctxTFAT.SaveChanges();
                            transaction.Commit();
                            transaction.Dispose();
                        }
                    }
                    objledgerdetail = objledgerdetail.Where(x => x.NarrSno != mModel.NarrSno).ToList();
                    Session["Narrlist"] = objledgerdetail;
                    html = ViewHelper.RenderPartialView(this, "_DocumentNarrList", objledgerdetail);
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Html = html, Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}