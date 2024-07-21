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

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class PurchaseController : BaseController
    {
        //tfatEntities ctxTFAT = new tfatEntities();
        //private static DateTime mRCMDate;
        private static bool mReturnP = false;
        private static bool mReturnS = false;
        private static bool mRequireTRNS = false;
        private static bool mFetchWoNo = false;
        private static bool mProjects = false;
        private static bool mAllowNegSales = false;
        private static int mNetSerl = 0;
        private static bool gpHolidayWarn = false;
        private static string gpHoliday1 = "Saturday";
        private static string gpHoliday2 = "Sunday";
        private static bool gpTDSApplicable = false;
        private static bool mLocTax = false;
        private static bool mDontDlySchedule = false;
        private static bool mFifoOrder = false;
        private static bool mSPAdjForce = false;
        private static bool mSEZChargeParty = false;
        private static int mGSTStyle = 0;
        public static object[,] objarray = null;
        private static string msearchstyle = "";

        #region index
        public ActionResult Index(PurchaseVM Model)
        {
            string connstring = GetConnectionString();
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            //GetAllMenu(Session["ModuleName"].ToString());
            ViewBag.list = Session["MenuList"];
            ViewBag.Modules = Session["ModulesList"];
            //to-do remove updateitemlist. UPurchSerialStkList
            Session["NewItemlist"] = null;
            Session["TempPurSaleAttach"] = null;

            // preferences from tfatbranch
            var mpara = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x).FirstOrDefault();


            ViewData["mBillStock"] = mpara.gp_BillStock;
            Model.NegStock = mpara.gp_NegStock;
            bool mNegWarn = mpara.gp_NegWarn;
            bool mNegStockAsOn = mpara.gp_NegStockAsOn;
            bool mCashLimit = mpara.gp_CashLimit;
            double mCashLimitAmt = mpara.gp_CashLimitAmt == null ? 0 : mpara.gp_CashLimitAmt.Value;

            mSEZChargeParty = mpara.gp_SEZChargeParty;

            DateTime date1 = new DateTime(2017, 10, 13);

            mSPAdjForce = mpara.gp_SPAdjForce;
            gpHoliday1 = mpara.gp_Holiday1 == null ? "" : mpara.gp_Holiday1;
            gpHoliday2 = mpara.gp_Holiday2 == null ? "" : mpara.gp_Holiday2;


            Model.BinConcept = mpara.gp_BIN;
            Model.EnableParty = mpara.gp_EnableParty;
            Model.MultiUnit = mpara.gp_MultiUnit;
            Model.IsItemClass = mpara.gp_ItemClass;
            mLocTax = mpara.gp_LocWiseTax;
            msearchstyle = mpara.gp_ItemSearchStyle ?? "B";
            if (msearchstyle == null || msearchstyle == "") msearchstyle = "B"; // B-search normally from begining, I-Instring Search, C-Complex Search

            if (Model.Mode != "Add")
            {
                var Doc = Model.Document;
                int length = Model.Document.Length;
                Model.Branch = Doc.Substring(0, 6).Trim();
                Model.ParentKey = Doc.Substring(6, length - 6);
                Model.Type = Model.ParentKey.Substring(0, 5);
                UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, Model.ParentKey, DateTime.Now, 0, "", "", "A");
            }
            else
            {
                UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "A");
            }

            var result = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => x).FirstOrDefault();
            Model.MainType = result.MainType;
            Model.SubType = result.SubType;

            Model.AUTHORISE = "A00";
            Model.IsManual = result.vAuto != "M" ? false : true;
            string mvprefix = result.PrefixConst == null ? "" : result.PrefixConst;
            int mWidth = result.DocWidth;
            var mPrevSrl = GetLastSerial(Model.TableName, mbranchcode, Model.Type, mperiod, Model.SubType, DateTime.Now.Date);
            if (mvprefix != "")
            {
                mPrevSrl = mPrevSrl.Replace(mvprefix, "");
            }

            int mPreIntSrl = Convert.ToInt32(mPrevSrl) - 1;
            ViewData["PrevSrl"] = mPreIntSrl.ToString("D" + mWidth);
            Model.CurrConv = result.CurrConv;
            Model.MileStoneReqd = result.MilestoneReqd;
            Model.IsBarCodeScan = result.BarCode;
            Model.NonStock = result.NonStock;
            Model.InterBranch = result.InterBranch;
            Model.VATGSTApp = (mpara.gp_VATGST == null || mpara.gp_VATGST == "") ? "G" : mpara.gp_VATGST;
            Model.ForceChln = result.ForceChln;
            Model.SkipStock = result.SkipStock;
            Model.HideAddlCharges = result.HideAddChg;
            Model.LockStore = result.LockStore;
            Model.LockFactor = result.LockFactor;
            Model.LockTax = result.LockTax;
            Model.LockHSN = result.LockHSN;
            Model.ForceOrderS = result.ForceOrder;
            Model.FIFOOrder = result.FIFOOrder;
            Model.LockItem = result.LockProduct;
            Model.LockQty = result.LockQty;
            Model.LockDiscCharges = result.LockAddChg;
            Model.LockWarehouse = result.LockWarehouse;
            Model.IsGstDocType = (result.GSTDocType == null) ? false : ((result.GSTDocType == 0) ? false : true);
            Model.ScanAndEdit = true;
            if (Model.CurrConv == "Y")
            {
                Model.CurrName = result.CurrName;
                Model.CurrRate = GetCurrRate(result.CurrName);
            }
            else
            {
                Model.CurrName = 1;
                Model.CurrRate = 1;
            }
            Model.AllowDraftSave = result.AllowDraftSave;
            Model.AllowCurr = result.AllowCurr;

            #region Suresh AuthencticateCode
            //var LR = new LorryReceiptController();
            //List<string> Child = LR.GetChildGrp(mbranchcode);
            //bool Authenticate = false;
            //var TfatUserAuditHeader = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == Model.Type).FirstOrDefault();
            //if (!String.IsNullOrEmpty(TfatUserAuditHeader.AuthReqBranch))
            //{
            //    var AuthBranchList = TfatUserAuditHeader.AuthReqBranch.Split(',').ToList();
            //    var CommonList = Child.Intersect(AuthBranchList);
            //    if (CommonList.Count() > 0)
            //    {
            //        Authenticate = true;
            //    }
            //}
            
            #endregion

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

            Model.IsRoundOff = result.RoundOff;

            Model.id = result.Name;
            Model.Prefix = mperiod;
            Model.GSTType = result.GSTType.ToString();
            Model.GstTypeName = GetGSTTypeName(Model.GSTType);

            Model.LocationCode = result.LocationCode;
            Model.ProductAddOn = "%@D" + result.MainType;
            Model.LockRate = result.LockRate;
            Model.LockDiscount = result.LockDiscount;
            Model.LockAddon = result.LockAddon;

            mAllowNegSales = result.AllowNeg;
            ViewData["DocAttach"] = result.DocAttach;
            //mheading = result.Name;
            Model.Store = result.Store == null ? 0 : result.Store.Value;
            Model.NegStock = result.NegStock;
            List<PurchaseVM> objledgerdetail = new List<PurchaseVM>();
            var trncharges = (from c in ctxTFAT.Charges.Where(x => x.Type == Model.Type && x.DontUse == false)
                              join st in ctxTFAT.StockTax.Where(x => x.Branch == mbranchcode && x.ParentKey == Model.ParentKey && x.ItemType == "C") on c.Fld equals st.Code into g
                              from gg in g.DefaultIfEmpty()
                              select new { c.Fld, c.Head, c.EqAmt, c.Equation, c.Code, TaxAmt = (gg.SGSTAmt + gg.IGSTAmt + gg.CGSTAmt) }).ToList();
            DataTable mdt = GetChargeValValue(Model.SubType, Model.ParentKey);
            int mfld;
            foreach (var i in trncharges)
            {
                PurchaseVM c = new PurchaseVM();
                c.Fld = i.Fld;
                c.Code = i.Head;
                c.AddLess = i.EqAmt;
                c.Equation = i.Equation;
                c.tempId = Convert.ToInt16(i.Fld.Substring(1, 3));
                if (Model.Mode != "Add")
                {
                    if (mdt != null && mdt.Rows.Count > 0)
                    {
                        mfld = Convert.ToInt32(i.Fld.Substring(1));
                        c.ColVal = mdt.Rows[0]["Amt" + mfld].ToString();
                        c.ValueLast = mdt.Rows[0]["Val" + mfld].ToString();
                        c.TaxAmt = (i.TaxAmt == null) ? 0 : i.TaxAmt.Value;
                    }
                    else
                    {
                        c.ColVal = "0";
                        c.ValueLast = "0";
                    }
                    c.Amt1 = Convert.ToDecimal((c.ValueLast == "" || c.ValueLast == null) ? "0" : c.ValueLast);
                }
                c.ChgPostCode = i.Code;
                objledgerdetail.Add(c);
            }

            Model.Charges = objledgerdetail;
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
                Model.BillDate = DateTime.Now;
                Model.DocDate = DateTime.Now;
                Model.EWBDate = DateTime.Now;
                Model.NoteDate = DateTime.Now;
                Model.IsSelf = true;
            }

            if (Model.Mode == "Edit" || Model.Mode == "View" || Model.Mode == "Delete")
            {
                Model.AddOnList = GetAddOnListOnEdit(Model.MainType, Model.ParentKey, Model.Type);


                #region Sales
                if (Model.SubType == "OC" || Model.SubType == "RS" || Model.SubType == "CS" || Model.SubType == "XS" || Model.SubType == "SX" || Model.SubType == "NS")
                {

                    var KeyList = ctxTFAT.Stock.Where(x => x.ParentKey == Model.ParentKey).Select(x => x.TableKey).ToList();
                    var CheckDependency = ctxTFAT.Stock.Where(x => KeyList.Contains(x.ChlnKey)).Select(x => x.ChlnKey).ToList();
                    if (CheckDependency.Count > 0)
                    {
                        Model.CheckMode = true;
                        //Model.Mode = "View";
                        Model.Message = "Document is Already Adjusted Against: " + CheckDependency.ToString() + ", Cant Edit";
                    }
                    
                    var mobjtax = (from s in ctxTFAT.Stock
                                   where s.ParentKey == Model.ParentKey
                                   join x in ctxTFAT.StockTax on s.TableKey equals x.TableKey
                                   select new
                                   {
                                       s.Code,
                                       s.Unit,
                                       s.Qty,
                                       s.Factor,
                                       s.Qty2,
                                       s.Disc,
                                       s.DiscAmt,
                                       s.Sno,
                                       s.Amt,
                                       s.Narr,
                                       s.Rate,
                                       s.Store,
                                       x.Taxable,
                                       x.CGSTAmt,
                                       x.SGSTAmt,
                                       x.IGSTAmt,
                                       x.SGSTRate,
                                       x.IGSTRate,
                                       x.CGSTRate,
                                       x.CVDAmt,
                                       x.CVDCessAmt,
                                       x.CVDExtra,
                                       x.CVDSCessAmt,
                                       x.TaxCode,
                                       x.HSNCode,
                                       x.TableKey,
                                       s.ParentKey,
                                       s.MainType,
                                       s.OrdKey,
                                       s.IndKey,
                                       s.ChlnKey,
                                       s.BINNumber,
                                       s.Discount,
                                       s.Unit2,
                                       s.RateOn,
                                       s.RateType,
                                       s.RateCalcType,
                                       s.PKSKey,
                                       s.InvKey,
                                       s.SubType,
                                       s.FreeQty,
                                       s.PriceListDisc,
                                       s.xValue1,
                                       s.xValue2,
                                       s.PriceListRate,
                                       s.SchemeCode,
                                       s.NewRate,
                                       s.cRate
                                   }).ToList();
                    var mobj1 = ctxTFAT.Sales.Where(x => (x.TableKey) == Model.ParentKey).FirstOrDefault();
                    var mobj3 = ctxTFAT.SalesMore.Where(x => x.TableKey == Model.ParentKey).FirstOrDefault();

                    Model.AUTHORISE = mobj1.AUTHORISE;
                    Model.Srl = mobj1.Srl.ToString();
                    Model.Prefix = mobj1.Prefix;

                    if (Model.SubType == "RS" || Model.SubType == "CS" || Model.SubType == "XS" || Model.SubType == "NS")
                    {
                        var tablerefkey = Model.ParentKey.Substring(0, 7) + "001" + Model.Srl.ToString();;
                        var CheckDependency2 = ctxTFAT.Outstanding.Where(x => x.TableRefKey == tablerefkey).Select(x => x.TableKey).FirstOrDefault();
                        if (CheckDependency2 != null)
                        {
                            Model.CheckMode = true;
                            //Model.Mode = "View";
                            Model.Message = "Document is Already Adjusted Against: " + CheckDependency2 + ", Cant Edit";
                        }
                    }

                    Model.DocDate = mobj1.DocDate.Value;
                    Model.BillNumber = mobj1.BillNumber;
                    Model.BillDate = mobj1.BillDate == null ? Convert.ToDateTime("1900-01-01") : mobj1.BillDate.Value;
                    Model.Branch = mobj1.Branch;
                    Model.LocationCode = mobj1.LocationCode;
                    Model.Account = mobj1.Code;
                    Model.AccountName = (ctxTFAT.Master.Where(x => x.Code == mobj1.Code).Select(x => x.Name).FirstOrDefault().ToString());
                    Model.AltAddress = Convert.ToByte(mobj1.AltAddress);
                    Model.DelyCode = mobj1.Delycode;
                    Model.DelyAltAdd = Convert.ToByte(mobj1.DelyAltAdd);
                    Model.DelyName = (ctxTFAT.Master.Where(x => x.Code == mobj1.Delycode).Select(x => x.Name).FirstOrDefault().ToString());
                    Model.BillContact = mobj1.BillContact == null ? 0 : mobj1.BillContact.Value;
                    Model.DelyContact = mobj1.DelyContact == null ? 0 : mobj1.DelyContact.Value;
                    Model.Broker = mobj1.Broker == null ? 0 : Convert.ToInt32(mobj1.Broker);
                    Model.BrokerName = mobj1.Broker == null ? "" : GetBrokerName(mobj1.Broker);
                    Model.Document = Model.Document;
                    Model.GSTType = (mobj1.GSTType == null) ? "0" : mobj1.GSTType.Value.ToString();
                    Model.GstTypeName = GetGSTTypeName(Model.GSTType);
                    Model.ReasonCode = mobj1.ReasonCode == null ? 0 : mobj1.ReasonCode.Value;
                    Model.CrPeriod = mobj1.CrPeriod == null ? 0 : mobj1.CrPeriod.Value;
                    Model.InsuranceNo = mobj1.InsuranceNo;
                    Model.SalesmanCode = mobj1.SalesMan == null ? "100001" : mobj1.SalesMan;
                    // Model.SalesmanName = (mobj1.Salesman == null) ? "" : GetSalesManName(mobj1.Salesman.Value);
                    Model.GSTNoITC = mobj1.GSTNoITC == null ? false : (mobj1.GSTNoITC.Value == 0) ? false : true;
                    Model.LoadKey = mobj1.LoadingKey;
                    if (Model.SubType == "SX" || Model.SubType == "NS")
                    {
                        Model.RoundOff = mobj1.RoundOff == null ? 0 : mobj1.RoundOff.Value * -1;
                    }
                    else
                    {
                        Model.RoundOff = mobj1.RoundOff == null ? 0 : mobj1.RoundOff.Value;
                    }
                    Model.SourceDoc = mobj1.SourceDoc;
                    if (Model.CurrConv == "Y")
                    {
                        Model.CurrName =Convert.ToInt32(mobj1.CurrName);
                        Model.CurrRate = mobj1.CurrRate.Value;
                    }
                    else
                    {
                        Model.CurrName = 1;
                        Model.CurrRate = 1;
                    }
                    Model.PlaceOfSupply = mobj1.PlaceOfSupply;
                    Model.ShipFrom = (mobj1.ShipFrom == null) ? "" : mobj1.ShipFrom;
                    Model.ShipFromName = ctxTFAT.Master.Where(x => x.Code == mobj1.ShipFrom).Select(x => x.Name).FirstOrDefault();
                    if (mobj3 != null)
                    {
                        Model.WONumber = mobj3.WONumber;
                        Model.BrokerOn = mobj3.BrokerOn == null ? 0 : mobj3.BrokerOn.Value;
                        Model.Brokerage = mobj3.Brokerage == null ? 0 : mobj3.Brokerage.Value;
                        Model.BrokerAmt = mobj3.BrokerAmt == null ? 0 : mobj3.BrokerAmt.Value;
                        Model.IncoPlace = mobj3.IncoPlace;
                        Model.IncoTerms = mobj3.IncoTerms == null ? 0 : mobj3.IncoTerms.Value;
                        Model.PayTerms = mobj3.PayTerms == null ? 0 : mobj3.PayTerms.Value;
                        Model.LCNo = mobj3.LCNo;
                        Model.AdvLicence = mobj3.AdvLic;
                       
                        Model.SCommOn = mobj3.SalemanOn == null ? 0 : mobj3.SalemanOn.Value;
                        Model.SCommission = mobj3.SalemanPer == null ? 0 : mobj3.SalemanPer.Value;
                        Model.SAmt = mobj3.SalemanAmt == null ? 0 : mobj3.SalemanAmt.Value;
                        Model.Reference = mobj3.Reference;
                        Model.ProjCode = mobj3.ProjCode;
                    }

                    if (Model.SubType == "RS" || Model.SubType == "CS" || Model.SubType == "NS")
                    {
                        Model.OSAdjList = GetOutstandingInEdit(mobj1.TableKey, mobj1.MainType);
                        Model.OSAdjList = ShowAdjustListInSession(Model);
                    }

                    List<PurchaseVM> Upobjitemdetail = new List<PurchaseVM>();
                    foreach (var item in mobjtax)
                    {
                        var discchargelist = GetDiscountChgListInEdit(item.TableKey, item.SubType, item.MainType, item.Code, Model.NonStock);
                        Upobjitemdetail.Add(new PurchaseVM()
                        {
                            Code = item.Code,
                            ItemName = ctxTFAT.ItemMaster.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault(),
                            Unit = item.Unit,
                            Qty = Math.Abs(item.Qty),
                            Factor = item.Factor,
                            FreeQty = item.FreeQty,
                            Qty2 = Math.Abs(item.Qty2),
                            Unit2 = item.Unit2,
                            RateOn2 = (item.RateOn == null) ? false : (item.RateOn.Value == 0) ? false : true,
                            Rate = (Model.CurrConv == "Y") ? ((item.cRate == null) ? 0 : item.cRate.Value) : ((item.Rate == null) ? 0 : item.Rate.Value),
                            Disc = item.Disc == null ? 0 : Math.Abs(item.Disc.Value),
                            DiscAmt = item.DiscAmt == null ? 0 : Math.Abs(item.DiscAmt.Value),
                            Taxable = item.Taxable == null ? 0 : Math.Abs(item.Taxable.Value),
                            CGSTAmt = item.CGSTAmt == null ? 0 : Math.Abs(item.CGSTAmt.Value),
                            SGSTAmt = item.SGSTAmt == null ? 0 : Math.Abs(item.SGSTAmt.Value),
                            IGSTAmt = item.IGSTAmt == null ? 0 : Math.Abs(item.IGSTAmt.Value),
                            SGSTRate = item.SGSTRate == null ? 0 : item.SGSTRate.Value,
                            IGSTRate = item.IGSTRate == null ? 0 : item.IGSTRate.Value,
                            CGSTRate = item.CGSTRate == null ? 0 : item.CGSTRate.Value,
                            CVDAmt = item.CVDAmt == null ? 0 : item.CVDAmt.Value,
                            CVDCessAmt = item.CVDCessAmt == null ? 0 : item.CVDCessAmt.Value,
                            CVDExtra = item.CVDExtra == null ? 0 : item.CVDExtra.Value,
                            CVDSCessAmt = item.CVDSCessAmt == null ? 0 : item.CVDSCessAmt.Value,
                            PAddOnList = GetPAddInEditView(Model.MainType, item.TableKey, Model.Type),
                            DelyScheList = GetDeliveryScheduleList(item.ParentKey, item.TableKey, item.Store),
                          
                            Val1 = Math.Abs(item.Amt),
                            Store = item.Store,
                            StoreName = GetStoreName(item.Store),
                            Narr = item.Narr,
                            GSTCode = item.TaxCode,
                            HSN = item.HSNCode,
                            MainType = item.MainType,
                            IndKey = item.IndKey,
                            OrdKey = item.OrdKey,
                            ChlnKey = item.ChlnKey,
                            PKSKey = item.PKSKey,
                            InvKey = item.InvKey,
                            EnqKey = "",
                            QtnKey = "",
                            BinNumber = item.BINNumber == null ? 0 : item.BINNumber.Value,
                            tempId = item.Sno,
                            SrNo = item.Sno,
                            tempIsDeleted = false,
                            //ItemType = ctxTFAT.ItemMaster.Where(x => x.Code == item.Code).Select(x => x.ItemType).FirstOrDefault(),
                            DiscPerAmt = Math.Abs(item.Discount.Value) - Math.Abs(item.DiscAmt.Value),
                            RateType = item.RateType,
                            RateCalcType = item.RateCalcType,
                            DiscChargeList = discchargelist,
                            DiscChargeAmt = discchargelist.Select(x => (decimal?)x.DiscAmt).Sum() ?? 0,
                            IsPickUp = (item.OrdKey == null || item.OrdKey == "") && (item.ChlnKey == null || item.ChlnKey == "") && (item.IndKey == null || item.IndKey == "") && (item.PKSKey == null || item.PKSKey == "") && (item.InvKey == null | item.InvKey == "") ? false : true,
                            OriginalTablekey = item.TableKey,
                            PriceDiscCode = item.PriceListDisc,
                            //IsPickUpByOther = (CheckDependencyWhileDelUpd(item.SubType, item.TableKey) != "") ? true : false,
                            PickedUpIn = CheckDependencyWhileDelUpd(item.SubType, item.TableKey),
                            ClassValues1 = item.xValue1,
                            ClassValues2 = item.xValue2,
                            PriceRateCode = item.PriceListRate,
                            ItemSchemeCode = item.SchemeCode,
                            
                            NewRate = (item.NewRate == null) ? 0 : (decimal)item.NewRate.Value
                        });
                    }
                    Session.Add("NewItemlist", Upobjitemdetail);
                    Model.NewItemList = Upobjitemdetail;
                    var chggstamt = objledgerdetail.Select(x => (decimal?)x.TaxAmt).Sum() ?? 0;
                    string ourstatename = Fieldoftable("Warehouse", "State", "Code=" + Model.LocationCode);
                    //string ourstatename = ctxTFAT.Warehouse.Where(x => x.Code == Model.LocationCode).Select(x => x.State).FirstOrDefault();
                    if (ourstatename == Model.PlaceOfSupply)
                    {
                        Model.IGSTAmt = Math.Abs(Upobjitemdetail.Select(x => x.IGSTAmt).Sum());
                        Model.SGSTAmt = Math.Abs(Upobjitemdetail.Select(x => x.SGSTAmt).Sum()) + chggstamt / 2;
                        Model.CGSTAmt = Math.Abs(Upobjitemdetail.Select(x => x.CGSTAmt).Sum()) + chggstamt / 2;
                    }
                    else
                    {
                        Model.IGSTAmt = Math.Abs(Upobjitemdetail.Select(x => x.IGSTAmt).Sum()) + chggstamt;
                        Model.SGSTAmt = Math.Abs(Upobjitemdetail.Select(x => x.SGSTAmt).Sum());
                        Model.CGSTAmt = Math.Abs(Upobjitemdetail.Select(x => x.CGSTAmt).Sum());
                    }
                    Model.TotalQty = Math.Abs(Upobjitemdetail.Select(x => x.Qty).Sum());
                    Model.Taxable = Math.Abs(Upobjitemdetail.Select(x => x.Taxable).Sum());
                    Model.InvoiceAmt = Math.Abs(mobj1.Amt.Value);
                    Model.PrevInvAmt = Math.Abs(mobj1.Amt.Value);
                    Model.IsPickUp = (Upobjitemdetail.Where(x => x.IsPickUp == true).Select(x => x).FirstOrDefault() != null) ? true : false;
                    Model.IsPickUpByOther = (Upobjitemdetail.Where(x => (x.PickedUpIn != "")).Select(x => x.Code).FirstOrDefault() != null) ? true : false;
                    Model.PickedUpIn = Upobjitemdetail.Where(x => (x.PickedUpIn != "")).Select(x => x.PickedUpIn.Replace("\n", "")).FirstOrDefault();
                }
                #endregion

                #region TransportDetail
                //var Tobj = ctxTFAT.Transport.Where(x => x.TableKey == Model.ParentKey).Select(x => x).FirstOrDefault();
                //if (Tobj != null)
                //{
                //    Model.ContactPerson = Tobj.ContactPerson;
                //    Model.NoPkg = Tobj.NoPkg;
                //    Model.NoteDate = Tobj.NoteDate == null ? DateTime.Now : Tobj.NoteDate.Value;
                //    Model.NoteNo = Tobj.NoteNo;
                //    Model.Remark = Tobj.Remark;
                //    Model.TransMode = Tobj.TransMode;
                //    Model.Transporter = Tobj.TransporterCode;
                //    Model.TransporterN = GetTransporterName(Tobj.Transporter);
                //    Model.VehicleNo = Tobj.VehicleNo;
                //    Model.DeliveryType = Tobj.DeliveryType == null ? (byte)0 : Convert.ToByte(Tobj.DeliveryType.Value);
                //    Model.FreightType = Tobj.FreightType == null ? (byte)0 : Convert.ToByte(Tobj.FreightType.Value);
                //    Model.EwBillNo = Tobj.EWBNo;
                //    Model.TrxWeight = Tobj.weight.Value;
                //    Model.TransportType = Convert.ToInt16(Tobj.TransType);
                //    Model.EWBDate = Tobj.EWBDate == null ? DateTime.Now : Tobj.EWBDate.Value;
                //}
                #endregion

                #region TDS Payments
                var mobtdstax = ctxTFAT.TDSPayments.Where(x => x.ParentKey == Model.ParentKey).FirstOrDefault();
                if (mobtdstax != null && (Model.MainType == "SL" || Model.SubType == "RP"))
                {
                    Model.TDSCode = mobtdstax.TDSCode == null ? 0 : mobtdstax.TDSCode.Value;
                    Model.TDSAble = mobtdstax.TDSAble == null ? 0 : mobtdstax.TDSAble.Value;
                    Model.TDSAmt = mobtdstax.TDSAmt == null ? 0 : mobtdstax.TDSAmt.Value;
                    Model.TDSCess = mobtdstax.TDSCessAmt == null ? 0 : mobtdstax.TDSCessAmt.Value;
                    Model.CutTDS = true;
                    Model.TDSReason = "";
                    Model.TDSSchg = mobtdstax.TDSSurChargeAmt == null ? 0 : mobtdstax.TDSSurChargeAmt.Value;
                    Model.TDSSHECess = mobtdstax.TDSSheCessAmt == null ? 0 : mobtdstax.TDSSheCessAmt.Value;

                }
                #endregion

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
                Session["TempPurSaleAttach"] = Model.DocumentList;
                #endregion

                #region Terms And Condition
                var Terms = ctxTFAT.TermsDetails.Where(x => x.ParentKey == Model.ParentKey).Select(x => new { x.TermsTitle, x.TermsConditions, x.Sno }).OrderBy(x => x.Sno).ToList();
                if (Terms != null)
                {
                    List<PurchaseVM> objtermlist = new List<PurchaseVM>();
                    foreach (var a in Terms)
                    {
                        objtermlist.Add(new PurchaseVM()
                        {
                            TermId = a.TermsTitle,
                            TermName = a.TermsConditions,
                            tempId = objtermlist.Count + 1,
                            tempIsDeleted = false
                        });
                    }
                    Model.TermList = objtermlist;
                }

                #endregion

                Model.EmailTo = ctxTFAT.Address.Where(x => x.Code == Model.Account && x.Sno == 0).Select(x => x.Email).FirstOrDefault() ?? "";

                if (Model.Mode != "Add" && Model.AuthReq == true && Model.AUTHORISE.Substring(0, 1) == "A" && Model.AuthLock)
                {
                    Model.CheckMode = true;
                    //Model.Mode = "View";
                    Model.Message = "Document is Already Authorised Cant Edit";
                }
                if (Model.AUTHORISE.Substring(0, 1) == "X")
                {
                    Model.IsDraftSave = true;
                    Model.Message = "Document is saved As Draft";
                }

            }
            return View(Model);
        }
        #endregion index

        #region Get

        public ActionResult GetItemList(string term)
        {
            bool mAssTypeGrp = false;
            var mPara = term.Split('^');
            var msearchtext = mPara[0].ToLower();
            var mP2 = mPara[1];
            var trxassigrp2 = ctxTFAT.TypeItemGroups.Where(x => x.Type == mP2).Select(x => x.Code).ToList();
            var mntype = ctxTFAT.DocTypes.Where(x => x.Code == mP2).Select(x => x.MainType).FirstOrDefault();
            if (trxassigrp2 != null && trxassigrp2.Count() > 0)
            {
                mAssTypeGrp = true;
            }
            var mDt = GetGeneralItemList(mP2, mntype, msearchstyle, mAssTypeGrp, msearchtext);
            var result = (from i in mDt
                          select new
                          {
                              Name = i["Name"].ToString(),
                              Code = i["Code"].ToString()
                          }).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);

        }
        public ActionResult GetPartyList(string term, string BaseGr, string Code)
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
                    if (BaseGr == "PR")
                    {
                        var result = (from m in ctxTFAT.Master
                                      where (m.BaseGr == "S" || m.BaseGr == "U" || m.BaseGr == "H" || m.BaseGr == "C") && m.Hide == false && m.AppBranch.Contains(mbranchcode) && accountgrps.Contains(m.Grp) && m.AUTHORISE.Substring(0, 1) == "A"
                                      select new
                                      {
                                          m.Code,
                                          Name = "[" + m.Code + "] " + m.ShortName + " " + m.Name + " " + m.City,
                                          OName = m.Name
                                      }).OrderBy(n => n.OName).Take(10).ToList();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = (from m in ctxTFAT.Master
                                      where (m.BaseGr == "D" || m.BaseGr == "U" || m.BaseGr == "H" || m.BaseGr == "C") && m.Hide == false && m.AppBranch.Contains(mbranchcode) && accountgrps.Contains(m.Grp) && m.AUTHORISE.Substring(0, 1) == "A"
                                      select new
                                      {
                                          m.Code,
                                          Name = "[" + m.Code + "] " + m.ShortName + " " + m.Name + " " + m.City,
                                          OName = m.Name
                                      }).OrderBy(n => n.OName).Take(10).ToList();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (accountgrps.Count() > 0 && partycategs.Count() > 0)
                {
                    if (BaseGr == "PR")
                    {
                        var result = (from m in ctxTFAT.Master
                                      where (m.BaseGr == "S" || m.BaseGr == "U" || m.BaseGr == "H" || m.BaseGr == "C") && m.Hide == false && m.AppBranch.Contains(mbranchcode) && accountgrps.Contains(m.Grp) && partycategs.Contains((int?)m.Category ?? 0) && m.AUTHORISE.Substring(0, 1) == "A"
                                      select new
                                      {
                                          m.Code,
                                          Name = "[" + m.Code + "] " + m.ShortName + " " + m.Name + " " + m.City,
                                          OName = m.Name
                                      }).OrderBy(n => n.OName).Take(10).ToList();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = (from m in ctxTFAT.Master
                                      where (m.BaseGr == "D" || m.BaseGr == "U" || m.BaseGr == "H" || m.BaseGr == "C") && m.Hide == false && m.AppBranch.Contains(mbranchcode) && accountgrps.Contains(m.Grp) && partycategs.Contains((int?)m.Category ?? 0) && m.AUTHORISE.Substring(0, 1) == "A"
                                      select new
                                      {
                                          m.Code,
                                          Name = "[" + m.Code + "] " + m.ShortName + " " + m.Name + " " + m.City,
                                          OName = m.Name
                                      }).OrderBy(n => n.OName).Take(10).ToList();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (accountgrps.Count() == 0 && partycategs.Count() > 0)
                {
                    if (BaseGr == "PR")
                    {
                        var result = (from m in ctxTFAT.Master
                                      where (m.BaseGr == "S" || m.BaseGr == "U" || m.BaseGr == "H" || m.BaseGr == "C") && m.Hide == false && m.AppBranch.Contains(mbranchcode) && partycategs.Contains((int?)m.Category ?? 0) && m.AUTHORISE.Substring(0, 1) == "A"
                                      select new
                                      {
                                          m.Code,
                                          Name = "[" + m.Code + "] " + m.ShortName + " " + m.Name + " " + m.City,
                                          OName = m.Name
                                      }).OrderBy(n => n.OName).Take(10).ToList();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = (from m in ctxTFAT.Master
                                      where (m.BaseGr == "D" || m.BaseGr == "U" || m.BaseGr == "H" || m.BaseGr == "C") && m.Hide == false && m.AppBranch.Contains(mbranchcode) && partycategs.Contains((int?)m.Category ?? 0) && m.AUTHORISE.Substring(0, 1) == "A"
                                      select new
                                      {
                                          m.Code,
                                          Name = "[" + m.Code + "] " + m.ShortName + " " + m.Name + " " + m.City,
                                          OName = m.Name
                                      }).OrderBy(n => n.OName).Take(10).ToList();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    if (BaseGr == "PR")
                    {
                        var result = (from m in ctxTFAT.Master
                                      where (m.BaseGr == "S" || m.BaseGr == "U" || m.BaseGr == "H" || m.BaseGr == "C") && m.Hide == false && m.AppBranch.Contains(mbranchcode) && m.AUTHORISE.Substring(0, 1) == "A"
                                      select new
                                      {
                                          m.Code,
                                          Name = "[" + m.Code + "] " + m.ShortName + " " + m.Name + " " + m.City,
                                          OName = m.Name
                                      }).OrderBy(n => n.OName).Take(10).ToList();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = (from m in ctxTFAT.Master
                                      where (m.BaseGr == "D" || m.BaseGr == "U" || m.BaseGr == "H" || m.BaseGr == "C") && m.Hide == false && m.AppBranch.Contains(mbranchcode) && m.AUTHORISE.Substring(0, 1) == "A"
                                      select new
                                      {
                                          m.Code,
                                          Name = "[" + m.Code + "] " + m.ShortName + " " + m.Name + " " + m.City,
                                          OName = m.Name
                                      }).OrderBy(n => n.OName).Take(10).ToList();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            else
            {
                if (accountgrps.Count() > 0 && partycategs.Count() == 0)//associated accgroup available but party categ are not
                {
                    if (BaseGr == "PR")
                    {

                        var result = (from m in ctxTFAT.Master
                                      where (m.BaseGr == "S" || m.BaseGr == "U" || m.BaseGr == "H" || m.BaseGr == "C") && (m.Name.Contains(term) || m.ShortName.Contains(term)) && m.Hide == false && m.AppBranch.Contains(mbranchcode) && accountgrps.Contains(m.Grp) && m.AUTHORISE.Substring(0, 1) == "A"
                                      select new
                                      {
                                          m.Code,
                                          Name = "[" + m.Code + "] " + m.ShortName + " " + m.Name + " " + m.City,
                                          OName = m.Name
                                      }).OrderBy(n => n.OName).Take(10).ToList();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = (from m in ctxTFAT.Master
                                      where (m.BaseGr == "D" || m.BaseGr == "U" || m.BaseGr == "H" || m.BaseGr == "C") && (m.Name.Contains(term) || m.ShortName.Contains(term)) && m.Hide == false && m.AppBranch.Contains(mbranchcode) && accountgrps.Contains(m.Grp) && m.AUTHORISE.Substring(0, 1) == "A"
                                      select new
                                      {
                                          m.Code,
                                          Name = "[" + m.Code + "] " + m.ShortName + " " + m.Name + " " + m.City,
                                          OName = m.Name
                                      }).OrderBy(n => n.OName).Take(10).ToList();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (accountgrps.Count() > 0 && partycategs.Count() > 0)
                {
                    if (BaseGr == "PR")
                    {

                        var result = (from m in ctxTFAT.Master
                                      where (m.BaseGr == "S" || m.BaseGr == "U" || m.BaseGr == "H" || m.BaseGr == "C") && (m.Name.Contains(term) || m.ShortName.Contains(term)) && m.Hide == false && m.AppBranch.Contains(mbranchcode) && accountgrps.Contains(m.Grp) && partycategs.Contains((int?)m.Category ?? 0) && m.AUTHORISE.Substring(0, 1) == "A"
                                      select new
                                      {
                                          m.Code,
                                          Name = "[" + m.Code + "] " + m.ShortName + " " + m.Name + " " + m.City,
                                          OName = m.Name
                                      }).OrderBy(n => n.OName).Take(10).ToList();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = (from m in ctxTFAT.Master
                                      where (m.BaseGr == "D" || m.BaseGr == "U" || m.BaseGr == "H" || m.BaseGr == "C") && (m.Name.Contains(term) || m.ShortName.Contains(term)) && m.Hide == false && m.AppBranch.Contains(mbranchcode) && accountgrps.Contains(m.Grp) && partycategs.Contains((int?)m.Category ?? 0) && m.AUTHORISE.Substring(0, 1) == "A"
                                      select new
                                      {
                                          m.Code,
                                          Name = "[" + m.Code + "] " + m.ShortName + " " + m.Name + " " + m.City,
                                          OName = m.Name
                                      }).OrderBy(n => n.OName).Take(10).ToList();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (accountgrps.Count() == 0 && partycategs.Count() > 0)
                {
                    if (BaseGr == "PR")
                    {

                        var result = (from m in ctxTFAT.Master
                                      where (m.BaseGr == "S" || m.BaseGr == "U" || m.BaseGr == "H" || m.BaseGr == "C") && (m.Name.Contains(term) || m.ShortName.Contains(term)) && m.Hide == false && m.AppBranch.Contains(mbranchcode) && partycategs.Contains((int?)m.Category ?? 0) && m.AUTHORISE.Substring(0, 1) == "A"
                                      select new
                                      {
                                          m.Code,
                                          Name = "[" + m.Code + "] " + m.ShortName + " " + m.Name + " " + m.City,
                                          OName = m.Name
                                      }).OrderBy(n => n.OName).Take(10).ToList();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = (from m in ctxTFAT.Master
                                      where (m.BaseGr == "D" || m.BaseGr == "U" || m.BaseGr == "H" || m.BaseGr == "C") && (m.Name.Contains(term) || m.ShortName.Contains(term)) && m.Hide == false && m.AppBranch.Contains(mbranchcode) && partycategs.Contains((int?)m.Category ?? 0) && m.AUTHORISE.Substring(0, 1) == "A"
                                      select new
                                      {
                                          m.Code,
                                          Name = "[" + m.Code + "] " + m.ShortName + " " + m.Name + " " + m.City,
                                          OName = m.Name
                                      }).OrderBy(n => n.OName).Take(10).ToList();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    if (BaseGr == "PR")
                    {
                        var result = (from m in ctxTFAT.Master
                                      where (m.BaseGr == "S" || m.BaseGr == "U" || m.BaseGr == "H" || m.BaseGr == "C") && (m.Name.Contains(term) || m.ShortName.Contains(term)) && m.Hide == false && m.AppBranch.Contains(mbranchcode) && m.AUTHORISE.Substring(0, 1) == "A"
                                      select new
                                      {
                                          m.Code,
                                          Name = "[" + m.Code + "] " + m.ShortName + " " + m.Name + " " + m.City,
                                          OName = m.Name
                                      }).OrderBy(n => n.OName).Take(10).ToList();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = (from m in ctxTFAT.Master
                                      where (m.BaseGr == "D" || m.BaseGr == "U" || m.BaseGr == "H" || m.BaseGr == "C") && (m.Name.Contains(term) || m.ShortName.Contains(term)) && m.Hide == false && m.AppBranch.Contains(mbranchcode) && m.AUTHORISE.Substring(0, 1) == "A"
                                      select new
                                      {
                                          m.Code,
                                          Name = "[" + m.Code + "] " + m.ShortName + " " + m.Name + " " + m.City,
                                          OName = m.Name
                                      }).OrderBy(n => n.OName).Take(10).ToList();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }

        [HttpPost]
        public ActionResult GetPartyDetails(PurchaseVM Model)
        {
            var result = ctxTFAT.Master.Where(x => x.Code == Model.Code).Select(x => new { x.Code, x.Name, x.SalesMan, x.Broker }).FirstOrDefault();
            var msterinf = ctxTFAT.MasterInfo.Where(x => x.Code == Model.Code).Select(x => new { x.CurrName, x.TDSCode, x.CutTDS, x.CrPeriod, x.Brokerage, x.Transporter, x.IncoTerms, x.IncoPlace, x.PaymentTerms, x.Rank }).FirstOrDefault();
            var addrl = ctxTFAT.Address.Where(x => x.Code == Model.Code && x.Sno == 0).Select(x => new { x.Email, x.State }).FirstOrDefault();
            var mType = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => new { x.CutTDS }).FirstOrDefault();
            Model.DelyCode = result.Code;
            Model.DelyName = result.Name;
            Model.CurrName = msterinf == null ? 1 : (msterinf.CurrName == null || msterinf.CurrName == 0) ? 1 : msterinf.CurrName.Value;
            Model.CurrRate = GetCurrRate(Model.CurrName);
            Model.TDSCode = msterinf == null ? 0 : (msterinf.TDSCode == null) ? 0 : msterinf.TDSCode.Value;
            Model.CutTDS = (mType.CutTDS == false) ? false : ((msterinf == null) ? false : msterinf.CutTDS);
            Model.CrPeriod = msterinf == null ? 0 : (msterinf.CrPeriod == null) ? 0 : msterinf.CrPeriod.Value;
            Model.SalesmanCode = result.SalesMan == null ? "0" : result.SalesMan.Value.ToString();
            Model.BrokerName = result.Broker == null ? "0" : result.Broker.Value.ToString();
            Model.Transporter = msterinf == null ? "0" : (msterinf.Transporter == null) ? "0" : msterinf.Transporter;
            // Model.SalesmanName = GetSalesManName(salescode);
            Model.IncoTerms = msterinf == null ? 0 : (msterinf.IncoTerms == null) ? 0 : msterinf.IncoTerms.Value;
            Model.IncoPlace = msterinf == null ? "0" : (msterinf.IncoPlace == null) ? "0" : msterinf.IncoPlace;
            string PayTerms = msterinf == null ? "0" : (msterinf.PaymentTerms == null) ? "0" : msterinf.PaymentTerms;
            Model.EmailTo = addrl == null ? "" : addrl.Email;
            Model.PlaceOfSupply = addrl == null ? "" : addrl.State;
            int mRank = msterinf == null ? 0 : (msterinf.Rank == null) ? 0 : msterinf.Rank.Value;
            string html = ViewHelper.RenderPartialView(this, "ShipParty", Model);

            string placesupphtml = ViewHelper.RenderPartialView(this, "PlaceOfSupply", Model);
            return Json(new
            {
                Html = html,
                CurrName = Model.CurrName,
                CurrRate = Model.CurrRate,
                DelyCode = Model.DelyCode,
                TDSCode = Model.TDSCode,
                CutTDS = Model.CutTDS,
                CrPeriod = Model.CrPeriod,
                SalesmanCode = Model.SalesmanCode,
                BrokerName = Model.BrokerName,
                Transporter = Model.Transporter,
                IncoTerms = Model.IncoTerms,
                IncoPlace = Model.IncoPlace,
                PayTerms = PayTerms,
                EmailTo = Model.EmailTo,
                Rank = mRank,
                PlsHtml = placesupphtml
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSalesMan()
        {
            List<SelectListItem> StoreCodelst = new List<SelectListItem>();
            var result = ctxTFAT.SalesMan.Select(m => new
            {
                m.Code,
                m.Name
            }).ToList();
            foreach (var item in result)
            {
                StoreCodelst.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }
            return Json(StoreCodelst, JsonRequestBehavior.AllowGet);

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

        public ActionResult GetReasonCodes(string MainType)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            if (MainType == "PR")
            {
                GSt.Add(new SelectListItem { Value = "1", Text = "Purchase Return" });
                GSt.Add(new SelectListItem { Value = "2", Text = "Post Purchase Discount" });
                GSt.Add(new SelectListItem { Value = "3", Text = "Deficiency in Services" });
                GSt.Add(new SelectListItem { Value = "4", Text = "Correction In Invoice" });
                GSt.Add(new SelectListItem { Value = "5", Text = "Change In POS" });
                GSt.Add(new SelectListItem { Value = "6", Text = "Finalization of Prov Assess" });
                GSt.Add(new SelectListItem { Value = "7", Text = "Others" });
                GSt.Add(new SelectListItem { Value = "8", Text = "Ecommerce Local Supply" });
            }
            else
            {
                GSt.Add(new SelectListItem { Value = "1", Text = "Sales Return" });
                GSt.Add(new SelectListItem { Value = "2", Text = "Post Sale Discount" });
                GSt.Add(new SelectListItem { Value = "3", Text = "Deficiency in Services" });
                GSt.Add(new SelectListItem { Value = "4", Text = "Correction In Invoice" });
                GSt.Add(new SelectListItem { Value = "5", Text = "Change In POS" });
                GSt.Add(new SelectListItem { Value = "6", Text = "Finalization of Prov Assess" });
                GSt.Add(new SelectListItem { Value = "7", Text = "Others" });
                GSt.Add(new SelectListItem { Value = "8", Text = "Ecommerce Local Supply" });
            }

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

        public ActionResult GetBroker(string term)
        {
            List<SelectListItem> StoreCodelst = new List<SelectListItem>();
            var result = ctxTFAT.Broker.Where(x => x.AppBranch == mbranchcode).Select(m => new
            {
                m.Code,
                m.Name
            }).ToList();
            foreach (var item in result)
            {
                StoreCodelst.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }
            return Json(StoreCodelst, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTransporter(string term)
        {
            List<SelectListItem> StoreCodelst = new List<SelectListItem>();
            var result = ctxTFAT.Master.Where(x => x.Category == 100002).Select(m => new
            {
                m.Code,
                m.Name
            }).OrderBy(n => n.Name).Take(10).ToList();
            foreach (var item in result)
            {
                StoreCodelst.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }
            return Json(StoreCodelst, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetHSN()
        {
            List<SelectListItem> hsnlist = new List<SelectListItem>();
            var result = ctxTFAT.HSNMaster.Select(m => new
            {
                m.Code,
                m.Name
            }).Distinct().ToList();

            foreach (var item in result)
            {
                hsnlist.Add(new SelectListItem { Text = "[" + item.Code + "] " + item.Name, Value = item.Code });
            }
            return Json(hsnlist, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetGSTCode(string MainType, string VATGSTApp)
        {
            List<SelectListItem> gstcodelist = new List<SelectListItem>();
            if (VATGSTApp == "G")
            {
                var result = ctxTFAT.TaxMaster.Where(x => x.VATGST != false && x.Scope == MainType.Substring(0, 1)).Select(m => new
                {
                    m.Code,
                    m.Name
                }).Distinct().ToList();
                foreach (var item in result)
                {
                    gstcodelist.Add(new SelectListItem { Text = item.Name, Value = item.Code });
                }
            }
            else
            {
                var result = ctxTFAT.TaxMaster.Where(x => x.VATGST == false && x.Scope == MainType.Substring(0, 1)).Select(m => new
                {
                    m.Code,
                    m.Name
                }).Distinct().ToList();
                foreach (var item in result)
                {
                    gstcodelist.Add(new SelectListItem { Text = item.Name, Value = item.Code });
                }
            }
            return Json(gstcodelist, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAddrSnoList(string Code, string DelyCode)
        {
            List<SelectListItem> AddrSnoList = new List<SelectListItem>();
            if (DelyCode == null || DelyCode.Trim() == "")
            {
                var addrsnolist = ctxTFAT.Address.Where(x => x.Code == Code).ToList().Select(b => new
                {
                    b.Sno,
                    b.Name
                }).ToList();
                foreach (var item in addrsnolist)
                {
                    AddrSnoList.Add(new SelectListItem { Text = item.Name, Value = item.Sno.ToString() });
                }
            }
            else
            {
                var addrsnolist = ctxTFAT.Address.Where(x => x.Code == DelyCode).ToList().Select(b => new
                {
                    b.Sno,
                    b.Name
                }).ToList();
                foreach (var item in addrsnolist)
                {
                    AddrSnoList.Add(new SelectListItem { Text = item.Name, Value = item.Sno.ToString() });
                }
            }
            return Json(AddrSnoList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAltAddress(string Code)
        {
            List<SelectListItem> AddrSnoList = new List<SelectListItem>();
            var addrsnolist = ctxTFAT.Address.Where(x => x.Code == Code).ToList().Select(b => new
            {
                b.Sno,
                b.Name
            }).ToList();
            foreach (var item in addrsnolist)
            {
                AddrSnoList.Add(new SelectListItem { Text = item.Name, Value = item.Sno.ToString() });
            }
            return Json(AddrSnoList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDelyAltAddByDely(string DelyCode)
        {
            List<SelectListItem> AddrSnoList = new List<SelectListItem>();
            var addrsnolist = ctxTFAT.Address.Where(x => x.Code == DelyCode).ToList().Select(b => new
            {
                b.Sno,
                b.Name
            }).ToList();
            foreach (var item in addrsnolist)
            {
                AddrSnoList.Add(new SelectListItem { Text = item.Name, Value = item.Sno.ToString() });
            }
            return Json(AddrSnoList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDelyAltAddByAcc(string Code)
        {
            List<SelectListItem> AddrSnoList = new List<SelectListItem>();

            var addrsnolist = ctxTFAT.Address.Where(x => x.Code == Code).ToList().Select(b => new
            {
                b.Sno,
                b.Name
            }).ToList();
            foreach (var item in addrsnolist)
            {
                AddrSnoList.Add(new SelectListItem { Text = item.Name, Value = item.Sno.ToString() });
            }
            return Json(AddrSnoList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetContactAddrSnoList(string Code, string DelyCode)
        {
            List<SelectListItem> AddrSnoList = new List<SelectListItem>();
            if (DelyCode == null || DelyCode.Trim() == "")
            {
                var addrsnolist = ctxTFAT.Address.Where(x => x.Code == Code && (x.AddOrContact == 1 || x.AddOrContact == 3)).ToList().Select(b => new
                {
                    b.Sno,
                    b.Person
                }).ToList();
                foreach (var item in addrsnolist)
                {
                    AddrSnoList.Add(new SelectListItem { Text = item.Person, Value = item.Sno.ToString() });
                }
            }
            else
            {
                var addrsnolist = ctxTFAT.Address.Where(x => x.Code == DelyCode && (x.AddOrContact == 1 || x.AddOrContact == 3)).ToList().Select(b => new
                {
                    b.Sno,
                    b.Person
                }).ToList();
                foreach (var item in addrsnolist)
                {
                    AddrSnoList.Add(new SelectListItem { Text = item.Person, Value = item.Sno.ToString() });
                }
            }

            return Json(AddrSnoList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBillContact(string Code)
        {
            List<SelectListItem> AddrSnoList = new List<SelectListItem>();
            var addrsnolist = ctxTFAT.Address.Where(x => x.Code == Code && (x.AddOrContact == 1 || x.AddOrContact == 3)).ToList().Select(b => new
            {
                b.Sno,
                b.Person
            }).ToList();
            foreach (var item in addrsnolist)
            {
                AddrSnoList.Add(new SelectListItem { Text = item.Person, Value = item.Sno.ToString() });
            }
            return Json(AddrSnoList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDelyContactByAcc(string Code)
        {
            List<SelectListItem> AddrSnoList = new List<SelectListItem>();
            var addrsnolist = ctxTFAT.Address.Where(x => x.Code == Code && (x.AddOrContact == 1 || x.AddOrContact == 3)).ToList().Select(b => new
            {
                b.Sno,
                b.Person
            }).ToList();
            foreach (var item in addrsnolist)
            {
                AddrSnoList.Add(new SelectListItem { Text = item.Person, Value = item.Sno.ToString() });
            }
            return Json(AddrSnoList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDelyContactByDely(string DelyCode)
        {
            List<SelectListItem> AddrSnoList = new List<SelectListItem>();
            var addrsnolist = ctxTFAT.Address.Where(x => x.Code == DelyCode && (x.AddOrContact == 1 || x.AddOrContact == 3)).ToList().Select(b => new
            {
                b.Sno,
                b.Person
            }).ToList();
            foreach (var item in addrsnolist)
            {
                AddrSnoList.Add(new SelectListItem { Text = item.Person, Value = item.Sno.ToString() });
            }
            return Json(AddrSnoList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetProductDetails(PurchaseVM Model2)
        {
            PurchaseVM Model = new PurchaseVM();
            Model.Code = Model2.Code;
            Model.HSN = ctxTFAT.ItemMaster.Where(x => x.Code == Model2.Code).Select(x => x.HSNCode).FirstOrDefault();
            Model.HSNName = ctxTFAT.HSNMaster.Where(x => x.Code == Model.HSN).Select(x => x.Name).FirstOrDefault();
            var item = ctxTFAT.ItemDetail.Where(x => x.Code == Model2.Code && x.Branch == mbranchcode).Select(x => new { x.PGSTCode, x.UnitP2, x.BINNumber, x.SerialReq, x.SGSTCode, x.UnitS2, x.MinSaleRate, x.StockSerial, x.BatchReq, x.RateOn2, x.ReOrdLevel, x.CheckReOrderLevel, x.MinQty, x.CheckMinLevel, x.Store, x.CheckStock }).FirstOrDefault();
            //var itemmas = ctxTFAT.ItemMaster.Where(x => x.Code == Model2.Code).Select(x => new { x.UnitP, x.RateCalcType, x.RateType, x.Narr, x.UnitS }).FirstOrDefault();
            Model2.DocDate = ConvertDDMMYYTOYYMMDD(Model2.DocuDate);
            //var classvalues = ctxTFAT.ItemValues.Where(x => x.Code == Model2.Code).Select(x => x.Class).Distinct().ToList();

            //string ClassValue1 = ctxTFAT.ItemValues.Where(x => x.Class == classvalues.FirstOrDefault() && x.Code == Model2.Code).Select(x => x.ClassValue).FirstOrDefault();
            //string ClassValue2 = "";
            //if (classvalues != null && classvalues.Count > 1)
            //{
            //    string mClassValue2 = classvalues.LastOrDefault();
            //    ClassValue2 = ctxTFAT.ItemValues.Where(x => x.Class == mClassValue2 && x.Code == Model2.Code).Select(x => x.ClassValue).FirstOrDefault();
            //}
            //Model2.ClassValues1 = ClassValue1;
            //Model2.ClassValues2 = ClassValue2;
            if (Model2.Store == 0)//if trx class store not set then get it from itemdetail
            {
                Model.Store = item.Store;
                Model2.Store = item.Store;
            }
            else
            {
                Model.Store = Model2.Store;
            }
            if (Model2.MainType == "PR")
            {
                Model.GSTCode = (Model2.GSTType == "10") ? "PGS0" : item.PGSTCode;
                var gsttax = ctxTFAT.TaxMaster.Where(x => x.Code == Model.GSTCode).Select(x => new { x.Name, x.DiscOnTxbl }).FirstOrDefault();
                Model.GSTCodeName = gsttax == null ? "" : gsttax.Name;
                //Model.Unit = itemmas.UnitP;
                PurchaseVM m3 = new PurchaseVM();
                m3 = GetRateByCircular(Model2);
                Model.DiscOnTaxable = gsttax == null ? false : gsttax.DiscOnTxbl;
                Model.Rate = m3.Rate;
                Model.DiscNotAllowed = m3.DiscNotAllowed;
                Model.Disc = m3.Disc;
                Model.DiscAmt = m3.DiscAmt;
                //Model.RateCalcType = itemmas.RateCalcType;
                //Model.RateType = itemmas.RateType == null ? "" : itemmas.RateType;
                Model.Unit2 = item.UnitP2;
                Model.BinNumber = item.BINNumber == null ? 0 : item.BINNumber.Value;
                Model.Stock = GetStock(Model2.Code, Model2.Store, DateTime.Now.Date, mbranchcode);
                Model.Stock2 = GetStock(Model2.Code, 0, DateTime.Now.Date, mbranchcode);
                Model.SerialReq = item.SerialReq;
                Model.IsStockSerial = item.StockSerial;
                Model.IsBatchReqd = item.BatchReq;
                Model.MinSaleRate = 0;
                //Model.Narr = (itemmas.Narr == null) ? "" : itemmas.Narr;
                Model.PriceRateCode = m3.PriceRateCode;
            }
            else
            {
                Model.GSTCode = (Model2.GSTType == "10") ? "GST0" : item.SGSTCode;
                var gsttax = ctxTFAT.TaxMaster.Where(x => x.Code == Model.GSTCode).Select(x => new { x.Name, x.DiscOnTxbl }).FirstOrDefault();
                Model.GSTCodeName = gsttax == null ? "" : gsttax.Name;
                PurchaseVM m4 = new PurchaseVM();
                m4 = GetRateByCircular(Model2);
                Model.Rate = m4.Rate;
                Model.DiscOnTaxable = gsttax == null ? false : gsttax.DiscOnTxbl;
                Model.DiscNotAllowed = m4.DiscNotAllowed;
                Model.Disc = m4.Disc;
                Model.DiscAmt = m4.DiscAmt;
                //Model.Unit = itemmas.UnitS;
                Model.Unit2 = item.UnitS2;
                //Model.RateCalcType = itemmas.RateCalcType;
                //Model.RateType = itemmas.RateType == null ? "" : itemmas.RateType;
                Model.BinNumber = item.BINNumber == null ? 0 : item.BINNumber.Value;
                Model.Stock = GetStock(Model2.Code, Model2.Store, DateTime.Now.Date, mbranchcode);
                Model.Stock2 = GetStock(Model2.Code, 0, DateTime.Now.Date, mbranchcode);
                Model.SerialReq = item.SerialReq;
                Model.IsStockSerial = item.StockSerial;
                Model.IsBatchReqd = item.BatchReq;
                Model.MinSaleRate = item.MinSaleRate == null ? 0 : item.MinSaleRate.Value;
                //Model.Narr = (itemmas.Narr == null) ? "" : itemmas.Narr;
                Model.PriceRateCode = m4.PriceRateCode;
            }
            bool mIsItemClass = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.gp_ItemClass).FirstOrDefault();
            Model.ClassValues1 = Model2.ClassValues1;
            Model.ClassValues2 = Model2.ClassValues2;
            Model.Factor = (ctxTFAT.UnitConversion.Where(x => x.Code == Model2.Code && x.Unit == Model.Unit && x.Unit2 == Model.Unit2).Select(x => x).FirstOrDefault() == null) ? (double)0 : (double)ctxTFAT.UnitConversion.Where(x => x.Code == Model2.Code && x.Unit == Model.Unit && x.Unit2 == Model.Unit2).Select(x => x.Factor).FirstOrDefault();
            Model.RateOn2 = item.RateOn2;
            Model.MainType = Model2.MainType;
            Model.Account = Model2.Account;
            Model.Type = Model2.Type;
            Model.ReOrderLevel = 0;
            Model.ChkReOrderLevel = item.CheckReOrderLevel;
            Model.MinQty = (item.MinQty == null) ? 0 : item.MinQty.Value;
            Model.ChkMinQtyLevel = item.CheckMinLevel;
            Model.CheckStock = item.CheckStock;
            var mDoctypes = ctxTFAT.DocTypes.Where(x => x.Code == Model2.Type).Select(x => new { x.SubType, x.NonStock }).FirstOrDefault();
            // Disc Structure
            List<PriceListVM> objitemlist = new List<PriceListVM>();

            bool allowdisccharges = false;

            if (mDoctypes.SubType == "NS" && mDoctypes.NonStock == true)
            {
                allowdisccharges = false;
            }
            else if (mDoctypes.SubType == "RS" || mDoctypes.SubType == "CS" || mDoctypes.SubType == "XS" || mDoctypes.SubType == "OC" || mDoctypes.SubType == "SX" || mDoctypes.SubType == "OS" || mDoctypes.SubType == "QS" || mDoctypes.SubType == "ES" || mDoctypes.SubType == "DI" || mDoctypes.SubType == "PI" || (mDoctypes.SubType == "NS" && mDoctypes.NonStock == false))
            {
                allowdisccharges = true;
            }
            //else if (mDoctypes.SubType == "EP" || mDoctypes.SubType == "QP" || mDoctypes.SubType == "OP" || mDoctypes.SubType == "CP" || mDoctypes.SubType == "IC" || mDoctypes.SubType == "RP" || mDoctypes.SubType == "NP" || mDoctypes.SubType == "IM" || mDoctypes.SubType == "PX" || mDoctypes.SubType == "GP")
            //{
            //    allowdisccharges = true;
            //}


            if (allowdisccharges == true)
            {
                var PriceDiscList = GetPriceListDisc(Model2.Code, Model2.Account, Model2.DocDate);
                if (PriceDiscList != null && PriceDiscList.PriceListDiscCode != null && Model.DiscNotAllowed == false && Model2.MainType == "SL")
                {
                    for (int i = 1; i <= 6; i++)
                    {
                        PriceListVM pd = new PriceListVM();
                        switch (i)
                        {
                            case 1:
                                pd.Disc = Convert.ToDouble(PriceDiscList.Disc1);
                                pd.AddLess = (PriceDiscList.AddLess1 == "A") ? "+" : (PriceDiscList.AddLess1 == "L") ? "-" : "";
                                pd.CalcOn = "";
                                pd.DiscCaption = (PriceDiscList.DiscCaption1 == "" || PriceDiscList.DiscCaption1 == null) ? "Charge 1" : PriceDiscList.DiscCaption1;
                                pd.PerValue = true;
                                break;
                            case 2:
                                pd.Disc = Convert.ToDouble(PriceDiscList.Disc2);
                                pd.AddLess = (PriceDiscList.AddLess2 == "A") ? "+" : (PriceDiscList.AddLess2 == "L") ? "-" : "";
                                pd.CalcOn = PriceDiscList.CalcOn2;
                                pd.DiscCaption = (PriceDiscList.DiscCaption2 == "" || PriceDiscList.DiscCaption2 == null) ? "Charge 2" : PriceDiscList.DiscCaption2;
                                pd.PerValue = true;
                                break;
                            case 3:
                                pd.Disc = Convert.ToDouble(PriceDiscList.Disc3);
                                pd.AddLess = (PriceDiscList.AddLess3 == "A") ? "+" : (PriceDiscList.AddLess3 == "L") ? "-" : "";
                                pd.CalcOn = PriceDiscList.CalcOn3;
                                pd.DiscCaption = (PriceDiscList.DiscCaption3 == "" || PriceDiscList.DiscCaption3 == null) ? "Charge 3" : PriceDiscList.DiscCaption3;
                                pd.PerValue = true;
                                break;
                            case 4:
                                pd.Disc = Convert.ToDouble(PriceDiscList.Disc4);
                                pd.AddLess = (PriceDiscList.AddLess4 == "A") ? "+" : (PriceDiscList.AddLess4 == "L") ? "-" : "";
                                pd.CalcOn = PriceDiscList.CalcOn4;
                                pd.DiscCaption = (PriceDiscList.DiscCaption4 == "" || PriceDiscList.DiscCaption4 == null) ? "Charge 4" : PriceDiscList.DiscCaption4;
                                pd.PerValue = true;
                                break;
                            case 5:
                                pd.Disc = Convert.ToDouble(PriceDiscList.Disc5);
                                pd.AddLess = (PriceDiscList.AddLess5 == "A") ? "+" : (PriceDiscList.AddLess5 == "L") ? "-" : "";
                                pd.CalcOn = PriceDiscList.CalcOn5;
                                pd.DiscCaption = (PriceDiscList.DiscCaption5 == "" || PriceDiscList.DiscCaption5 == null) ? "Charge 5" : PriceDiscList.DiscCaption5;
                                pd.PerValue = true;
                                break;
                            case 6:
                                pd.Disc = Convert.ToDouble(Model.DiscCharge6);
                                pd.AddLess = "-";
                                pd.CalcOn = "C";
                                pd.DiscCaption = "Scheme Disc 1";
                                pd.PerValue = true;
                                break;
                        }
                        pd.tempid = i;
                        objitemlist.Add(pd);
                    }
                    Model.PriceDiscCode = PriceDiscList.PriceListDiscCode;

                }
            }


            if (objitemlist == null || objitemlist.Count() == 0)
            {
                var PriceDiscList = ctxTFAT.AddCharges.Where(x => x.Hide == false && x.Type == Model2.MainType.Substring(0, 1)).Select(x => x).ToList();
                int distemp = 1;
                foreach (var a in PriceDiscList)
                {
                    objitemlist.Add(new PriceListVM()
                    {
                        Disc = Convert.ToDouble(a.Amount.Value),
                        AddLess = (a.CalOperater == "A") ? "+" : (a.CalOperater == "L") ? "-" : "",
                        CalcOn = "C",
                        DiscCaption = a.Name,
                        PerValue = a.PerOrValue,
                        tempid = distemp
                    });
                    distemp = distemp + 1;

                }

            }
            string html = "";
            if (Model2.Mode == "Add")
            {
                html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/Purchase/DiscountChargesAdd.cshtml", new PurchaseVM() { DiscChargeList = objitemlist });
            }
            else
            {
                html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/Purchase/DiscountChargesEdit.cshtml", new PurchaseVM() { DiscChargeList = objitemlist });
            }
            Model.RichNote = html;


            return Json(Model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAddressBySno(string Code, int Sno, string DelyCode)
        {
            string Address = "";
            Address result;
            if (DelyCode == null || DelyCode.Trim() == "")
            {
                result = (from Add in ctxTFAT.Address where Add.Code == Code && Add.Sno == Sno select Add).FirstOrDefault();
            }
            else
            {
                result = (from Add in ctxTFAT.Address where Add.Code == DelyCode && Add.Sno == Sno select Add).FirstOrDefault();
            }
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
            return Json(Address, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAddressToSnoByAcc(string Code, int Sno)
        {
            string Address = "";
            Address result;

            result = (from Add in ctxTFAT.Address where Add.Code == Code && Add.Sno == Sno select Add).FirstOrDefault();


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
            return Json(Address, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAddressToSnoByDely(string DelyCode, int Sno)
        {
            string Address = "";
            Address result;

            result = (from Add in ctxTFAT.Address where Add.Code == DelyCode && Add.Sno == Sno select Add).FirstOrDefault();


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
            //if (result.Pin != null)
            //{
            //    if (Address != "")
            //    {
            //        Address = Address + ',\n' + result.Pin;
            //    }
            //    else
            //    {
            //        Address = result.Pin;
            //    }
            //}
            return Json(Address, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetLocationCode()
        {
            List<SelectListItem> LocationCodelst = new List<SelectListItem>();
            var result = ctxTFAT.Warehouse.Select(c => new
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

        public ActionResult GetStoreCode(int LocationCode, string Type, string SubType)
        {
            List<SelectListItem> StoreCodelst = new List<SelectListItem>();
            var mQcReqd = ctxTFAT.DocTypes.Where(x => x.Code == Type).Select(x => x.QCReqd).FirstOrDefault();
            if (mQcReqd == true && (SubType == "IC" || SubType == "PR" || SubType == "IM"))
            {
                var result = ctxTFAT.Stores.Where(x => x.StoreType == 7 && x.Flag != "G" && x.Locked == false && x.LocationCode == LocationCode).Select(c => new
                {
                    c.Code,
                    c.Name
                }).Distinct().ToList();
                foreach (var item in result)
                {
                    StoreCodelst.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
                }
            }
            else
            {
                var docShowStores = ctxTFAT.DocTypes.Where(x => x.Code == Type).Select(x => x.ShowStores).FirstOrDefault();
                if (docShowStores == null || docShowStores == 0)
                {
                    var result = ctxTFAT.Stores.Where(x => x.Flag != "G" && x.Locked == false && x.LocationCode == LocationCode).Select(c => new
                    {
                        c.Code,
                        c.Name
                    }).Distinct().ToList();
                    foreach (var item in result)
                    {
                        StoreCodelst.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
                    }
                }
                if (docShowStores == 1)
                {
                    var result = ctxTFAT.Stores.Where(x => x.Flag != "G" && x.Locked == false && x.Party == "" && x.LocationCode == LocationCode).Select(c => new
                    {
                        c.Code,
                        c.Name
                    }).Distinct().ToList();
                    foreach (var item in result)
                    {
                        StoreCodelst.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
                    }
                }
                if (docShowStores == 2)
                {
                    var result = ctxTFAT.Stores.Where(x => x.Flag != "G" && x.Locked == false && x.Party != "" && x.LocationCode == LocationCode).Select(c => new
                    {
                        c.Code,
                        c.Name
                    }).Distinct().ToList();
                    foreach (var item in result)
                    {
                        StoreCodelst.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
                    }
                }
            }
            return Json(StoreCodelst, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTermCode()
        {
            List<SelectListItem> StoreCodelst = new List<SelectListItem>();
            var result = ctxTFAT.TermsConditions.Select(c => new
            {
                c.Code,
                c.Name
            }).Distinct().ToList();
            foreach (var item in result)
            {
                StoreCodelst.Add(new SelectListItem { Text = item.Name, Value = item.Name });
            }

            return Json(StoreCodelst, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTermTemplate()
        {
            List<SelectListItem> StoreCodelst = new List<SelectListItem>();
            var result = ctxTFAT.TermTemplateHeader.Select(c => new
            {
                c.Code,
                c.Name
            }).Distinct().ToList();
            foreach (var item in result)
            {
                StoreCodelst.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(StoreCodelst, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetTermsByCode(string TermId)
        {
            PurchaseVM Model = new PurchaseVM();
            if (TermId == null || TermId.Trim() == "")
            {
                Model.TermName = "";
            }
            else
            {
                Model.TermName = ctxTFAT.TermsConditions.Where(x => x.Name == TermId).Select(x => x.Terms).FirstOrDefault();
            }

            return Json(Model, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetStoreUpdCode(int LocationCode, string Type, string SubType)
        {
            List<SelectListItem> StoreCodelst = new List<SelectListItem>();
            var mQcReqd = ctxTFAT.DocTypes.Where(x => x.Code == Type).Select(x => x.QCReqd).FirstOrDefault();
            if (mQcReqd == true && (SubType == "IC" || SubType == "PR" || SubType == "IM"))
            {
                var result = ctxTFAT.Stores.Where(x => x.StoreType == 7 && x.Flag != "G" && x.Locked == false && x.LocationCode == LocationCode).Select(c => new
                {
                    c.Code,
                    c.Name
                }).Distinct().ToList();
                foreach (var item in result)
                {
                    StoreCodelst.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
                }
            }
            else
            {
                var docShowStores = ctxTFAT.DocTypes.Where(x => x.Code == Type).Select(x => x.ShowStores).FirstOrDefault();
                if (docShowStores == null || docShowStores == 0)
                {
                    var result = ctxTFAT.Stores.Where(x => x.Flag != "G" && x.Locked == false && x.LocationCode == LocationCode).Select(c => new
                    {
                        c.Code,
                        c.Name
                    }).Distinct().ToList();
                    foreach (var item in result)
                    {
                        StoreCodelst.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
                    }
                }
                if (docShowStores == 1)
                {
                    var result = ctxTFAT.Stores.Where(x => x.Flag != "G" && x.Locked == false && x.Party == "" && x.LocationCode == LocationCode).Select(c => new
                    {
                        c.Code,
                        c.Name
                    }).Distinct().ToList();
                    foreach (var item in result)
                    {
                        StoreCodelst.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
                    }
                }
                if (docShowStores == 2)
                {
                    var result = ctxTFAT.Stores.Where(x => x.Flag != "G" && x.Locked == false && x.Party != "" && x.LocationCode == LocationCode).Select(c => new
                    {
                        c.Code,
                        c.Name
                    }).Distinct().ToList();
                    foreach (var item in result)
                    {
                        StoreCodelst.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
                    }
                }
            }
            //var result = ctxTFAT.Stores.Select(c => new
            //{
            //    c.Code,
            //    c.Name
            //}).Distinct().ToList();
            //foreach (var item in result)
            //{
            //    StoreCodelst.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            //}

            return Json(StoreCodelst, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetUnit(string Code)
        {
            var UnitConv = ctxTFAT.UnitConversion.Where(x => x.Code == Code).Select(x => x.Unit2).ToList();
            List<SelectListItem> hsnlist = new List<SelectListItem>();
            if (UnitConv.Count > 0)
            {
                var result = ctxTFAT.UnitMaster.Where(x => UnitConv.Contains(x.Code)).Select(m => new
                {
                    m.Code,
                    m.Name
                }).Distinct().ToList();

                foreach (var item in result)
                {
                    hsnlist.Add(new SelectListItem { Text = item.Name, Value = item.Code });
                }
            }
            else
            {
                var result = ctxTFAT.UnitMaster.Select(m => new
                {
                    m.Code,
                    m.Name
                }).Distinct().ToList();

                foreach (var item in result)
                {
                    hsnlist.Add(new SelectListItem { Text = item.Name, Value = item.Code });
                }
            }

            return Json(hsnlist, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PopulateQty2(PurchaseVM Model)
        {
            bool mIsItemClass = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.gp_ItemClass).FirstOrDefault();
            string operatorsign = ctxTFAT.UnitConversion.Where(x => x.Code == Model.Code && x.Unit == Model.Unit && x.Unit2 == Model.Unit2 && ((mIsItemClass == true) ? (((string.IsNullOrEmpty(Model.ClassValues1) == false) ? (x.xValue1 == Model.ClassValues1) : true) && ((string.IsNullOrEmpty(Model.ClassValues2) == false) ? (x.xValue2 == Model.ClassValues2) : true)) : true)).Select(x => x.Operator).FirstOrDefault();
            if (operatorsign != null && Model.Factor != 0)
            {
                if (operatorsign == "*")
                {
                    Model.Qty2 = Model.Qty * Model.Factor;
                }
                else if (operatorsign == "/")
                {
                    Model.Qty2 = Model.Qty / Model.Factor;
                }
                else if (operatorsign == "-")
                {
                    Model.Qty2 = Model.Qty - Model.Factor;
                }
                else if (operatorsign == "+")
                {
                    Model.Qty2 = Model.Qty + Model.Factor;
                }
                else
                {
                    Model.Qty2 = Model.Qty2;
                }
            }
            else
            {
                Model.Qty2 = Model.Qty2;
            }
            return Json(Model, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadFactorByUnit2(PurchaseVM Model)
        {
            bool mIsItemClass = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.gp_ItemClass).FirstOrDefault();
            var unitconv = ctxTFAT.UnitConversion.Where(x => x.Code == Model.Code && x.Unit == Model.Unit && x.Unit2 == Model.Unit2 && ((mIsItemClass == true) ? (((string.IsNullOrEmpty(Model.ClassValues1) == false) ? (x.xValue1 == Model.ClassValues1) : true) && ((string.IsNullOrEmpty(Model.ClassValues2) == false) ? (x.xValue2 == Model.ClassValues2) : true)) : true)).Select(x => x).FirstOrDefault();
            if (unitconv != null)
            {
                Model.Factor = (double)unitconv.Factor;
            }
            else
            {
                Model.Factor = 0;
            }
            return Json(Model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PopulateQty1byQty2(PurchaseVM Model)
        {
            bool mIsItemClass = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.gp_ItemClass).FirstOrDefault();
            string operatorsign = ctxTFAT.UnitConversion.Where(x => x.Code == Model.Code && x.Unit == Model.Unit && x.Unit2 == Model.Unit2 && ((mIsItemClass == true) ? (((string.IsNullOrEmpty(Model.ClassValues1) == false) ? (x.xValue1 == Model.ClassValues1) : true) && ((string.IsNullOrEmpty(Model.ClassValues2) == false) ? (x.xValue2 == Model.ClassValues2) : true)) : true)).Select(x => x.Operator).FirstOrDefault();
            if (operatorsign != null && Model.Factor != 0 && Model.Qty == 0)
            {
                if (operatorsign == "*")
                {
                    Model.Qty = Model.Qty2 / Model.Factor;
                }
                else if (operatorsign == "/")
                {
                    Model.Qty = Model.Qty2 * Model.Factor;
                }
                else if (operatorsign == "-")
                {
                    Model.Qty = Model.Qty2 + Model.Factor;
                }
                else if (operatorsign == "+")
                {
                    Model.Qty = Model.Qty2 - Model.Factor;
                }
                else
                {
                    Model.Qty = Model.Qty;
                }
            }
            else
            {
                Model.Qty = Model.Qty;
            }
            return Json(Model, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PopulateGSTIDByAcc(PurchaseVM Model)
        {
            if (Model.VATGSTApp == "G")
            {
                var operatorsign = ctxTFAT.Address.Where(x => x.Code == Model.Code && x.Sno == Model.Sno).Select(x => x.GSTNo).FirstOrDefault();
                Model.DisplayGSTId = operatorsign;
            }
            else if (Model.VATGSTApp == "V")
            {
                var operatorsign = ctxTFAT.Address.Where(x => x.Code == Model.Code && x.Sno == Model.Sno).Select(x => x.VATReg).FirstOrDefault();
                Model.DisplayGSTId = operatorsign;
            }
            else
            {
                Model.DisplayGSTId = "";
            }
            return Json(Model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PopulateGSTIDByDely(PurchaseVM Model)
        {
            if (Model.VATGSTApp == "G")
            {
                var operatorsign = ctxTFAT.Address.Where(x => x.Code == Model.DelyCode && x.Sno == Model.Sno).Select(x => x.GSTNo).FirstOrDefault();
                Model.DisplayGSTId = operatorsign;
            }
            else if (Model.VATGSTApp == "V")
            {
                var operatorsign = ctxTFAT.Address.Where(x => x.Code == Model.DelyCode && x.Sno == Model.Sno).Select(x => x.VATReg).FirstOrDefault();
                Model.DisplayGSTId = operatorsign;
            }
            else
            {
                Model.DisplayGSTId = "";

            }
            return Json(Model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTransportType(string Val)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "Regular" });
            GSt.Add(new SelectListItem { Value = "2", Text = "Bill To Ship To" });
            GSt.Add(new SelectListItem { Value = "3", Text = "Bill From Dispatch From" });
            GSt.Add(new SelectListItem { Value = "4", Text = "Combination of 2 and 3" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPayTerms()
        {
            List<SelectListItem> StoreCodelst = new List<SelectListItem>();
            var result = ctxTFAT.PaymentTerms.Select(c => new
            {
                c.Code,
                c.Name
            }).Distinct().ToList();
            foreach (var item in result)
            {
                StoreCodelst.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }
            return Json(StoreCodelst, JsonRequestBehavior.AllowGet);
        }

     
     

        
       
        public ActionResult GetCurrencyName()
        {
            List<SelectListItem> StoreCodelst = new List<SelectListItem>();
            var result = ctxTFAT.CurrencyMaster.Select(c => new
            {
                c.Code,
                c.Name
            }).Distinct().ToList();
            foreach (var item in result)
            {
                StoreCodelst.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(StoreCodelst, JsonRequestBehavior.AllowGet);
        }
        public decimal GetCurrRate(int Code)
        {
            decimal mCurr = ctxTFAT.CurrencyMaster.Where(x => x.Code == Code).Select(z => z.CurrRate).FirstOrDefault();
            if (mCurr == 0)
            {
                mCurr = 1;
            }
            return mCurr;
        }

       
        public ActionResult GetTDSCodes()
        {
            List<SelectListItem> StoreCodelst = new List<SelectListItem>();
            var result = ctxTFAT.TDSMaster.Select(c => new
            {
                c.Code,
                c.Name
            }).Distinct().ToList();
            foreach (var item in result)
            {
                StoreCodelst.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(StoreCodelst, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PopulateTDSRates(PurchaseVM Model)
        {
            var datenow = DateTime.Now.Date;
            var TDSRATEtab = ctxTFAT.TDSRates.Where(x => x.Code == Model.TDSCode && x.EffDate <= datenow && x.LimitFrom <= Model.TDSAble).Select(x => new { x.TDSRate, x.Cess, x.SHECess, x.SurCharge }).FirstOrDefault();
            if (TDSRATEtab != null && Model.CutTDS == true)
            {
                Model.TDSAmt = TDSRATEtab.TDSRate == null ? 0 : (TDSRATEtab.TDSRate.Value * Model.TDSAble) / 100;
                Model.TDSCess = TDSRATEtab.Cess == null ? 0 : (TDSRATEtab.Cess.Value * Model.TDSAmt) / 100;
                Model.TDSSchg = TDSRATEtab.SurCharge == null ? 0 : (TDSRATEtab.SurCharge.Value * Model.TDSAmt) / 100;
                Model.TDSSHECess = TDSRATEtab.SHECess == null ? 0 : (TDSRATEtab.SHECess.Value * Model.TDSAmt) / 100;
            }
            else
            {
                Model.TDSAmt = 0;
                Model.TDSCess = 0;
                Model.TDSSchg = 0;
                Model.TDSSHECess = 0;
            }
            return Json(Model, JsonRequestBehavior.AllowGet);
        }

       
      
       
        public int GetMaxFromStringList(List<string> strings)
        {
            List<int> ints = new List<int>();
            foreach (var a in strings)
            {
                string c = "";
                if (a == "" || a == null)
                {
                    c = "0";
                }
                else
                {
                    c = Regex.Replace(a, "[^0-9]", "");
                    if (c == "")
                    {
                        c = "0";
                    }
                }
                int b = Convert.ToInt32(c);
                ints.Add(b);

            }
            int abc = ints.Select(x => x).Max();
            return abc;
        }

        public ActionResult GetProductPriceListRate(PurchaseVM Model)
        {
            bool mIsItemClass = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.gp_ItemClass).FirstOrDefault();
            Model.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            PurchaseVM m3 = new PurchaseVM();
            m3 = GetRateByCircular(Model);
            m3.Factor = (ctxTFAT.UnitConversion.Where(x => x.Code == Model.Code && x.Unit == Model.Unit && x.Unit2 == Model.Unit2 && ((mIsItemClass == true) ? (((string.IsNullOrEmpty(Model.ClassValues1) == false) ? (x.xValue1 == Model.ClassValues1) : true) && ((string.IsNullOrEmpty(Model.ClassValues2) == false) ? (x.xValue2 == Model.ClassValues2) : true)) : true)).Select(x => x).FirstOrDefault() == null) ? (double)0 : (double)ctxTFAT.UnitConversion.Where(x => x.Code == Model.Code && x.Unit == Model.Unit && x.Unit2 == Model.Unit2 && ((mIsItemClass == true) ? (((string.IsNullOrEmpty(Model.ClassValues1) == false) ? (x.xValue1 == Model.ClassValues1) : true) && ((string.IsNullOrEmpty(Model.ClassValues2) == false) ? (x.xValue2 == Model.ClassValues2) : true)) : true)).Select(x => x.Factor).FirstOrDefault();

            return Json(m3, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetStockDetails(PurchaseVM Model)
        {
            Model.Stock = GetStock(Model.Code, Model.Store, DateTime.Now.Date, mbranchcode);
            Model.Stock2 = GetStock2(Model.Code, Model.Store);
            return Json(Model, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetShippingPartyDetails(PurchaseVM Model)
        {
            var addrl = ctxTFAT.Address.Where(x => x.Code == Model.Code && x.Sno == 0).Select(x => new { x.Email, x.State }).FirstOrDefault();
            Model.PlaceOfSupply = (addrl == null) ? "" : addrl.State;
            string placesupphtml = ViewHelper.RenderPartialView(this, "PlaceOfSupply", Model);
            return Json(new
            {
                PlsHtml = placesupphtml
            }, JsonRequestBehavior.AllowGet);
        }

    
        #endregion

        #region Item Add Edit
        public ActionResult AddItemlist(PurchaseVM Model)
        {
            List<StockSerialVM> abc = new List<StockSerialVM>();

            List<PurchaseVM> objitemlist = new List<PurchaseVM>();
            List<string> DuplicateBatchs = new List<string>();
            List<PurchaseVM> AddProductList = new List<PurchaseVM>();
            List<PurchaseVM> AddProductList2 = new List<PurchaseVM>();
            List<PriceListVM> mDiscChargeList = new List<PriceListVM>();
            List<string> SSBatch = new List<string>();
            if (Session["NewItemlist"] != null)
            {
                objitemlist = (List<PurchaseVM>)Session["NewItemlist"];
            }
            
            AddProductList2 = (objitemlist == null) ? AddProductList2 : objitemlist;
            int Maxtempid = (AddProductList2.Count == 0) ? 0 : AddProductList2.Select(x => x.tempId).Max();
            AddProductList.AddRange(AddProductList2);
            var mdoctypereqdflds = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => new { x.PricelistReqd, x.QCReqd }).FirstOrDefault();

            if (AddProductList.Where(x => x.Code == Model.Code).Any() == true )
            {
                return Json(new { Status = "Duplicate", Message = " Cant Save Duplicate Product is " + Model.Code }, JsonRequestBehavior.AllowGet);
            }

            var mItemDetBQC = ctxTFAT.ItemDetail.Where(x => x.Code == Model.Code && x.Branch == mbranchcode).Select(x => new { x.BatchNoDupl, x.QCReqd }).FirstOrDefault();

            string mPartyPriceList = "";
            mPartyPriceList = ctxTFAT.MasterInfo.Where(x => x.Code == Model.Account).Select(x => x.PriceList).FirstOrDefault();
            if (mdoctypereqdflds.PricelistReqd == true)
            {
                if (string.IsNullOrEmpty(mPartyPriceList))
                {
                    return Json(new
                    {
                        Message = "PriceList Code is Required in Party Master Cant Add Product..",
                        Status = "PriceListReqd"
                    }, JsonRequestBehavior.AllowGet);
                }
            }



            Model = ServerSideTaxCalculation(Model);
            int mqtydecimalpl = ctxTFAT.UnitMaster.Where(x => x.Code == Model.Unit).Select(x => (int?)x.NoOfDecimal).FirstOrDefault() ?? 0;
            if (mqtydecimalpl == 0)
            {
                mqtydecimalpl = 2;
            }
            int mqtydecimalpl2 = ctxTFAT.UnitMaster.Where(x => x.Code == Model.Unit2).Select(x => (int?)x.NoOfDecimal).FirstOrDefault() ?? 0;
            if (mqtydecimalpl2 == 0)
            {
                mqtydecimalpl2 = 2;
            }

            bool mQcreqd = (mItemDetBQC.QCReqd == true && mdoctypereqdflds.QCReqd == true) ? true : false;
            bool mqcdone = (mQcreqd == true) ? false : true;
            AddProductList.Add(new PurchaseVM()
            {
                Code = Model.Code,
                ItemName = ctxTFAT.ItemMaster.Where(x => x.Code == Model.Code).Select(x => x.Name).FirstOrDefault(),
                Unit = Model.Unit,
                Qty = Math.Round((Model.Qty * Model.PendingFactor), mqtydecimalpl),
                Factor = Model.Factor,
                Qty2 = Math.Round((Model.Qty2 * Model.PendingFactor), mqtydecimalpl2),
                Rate = Model.Rate,
                Disc = Model.Disc,
                DiscAmt = Math.Round((Model.DiscAmt * Convert.ToDecimal(Model.PendingFactor)), 2),
                DiscPerAmt = Math.Round((Model.DiscPerAmt * Convert.ToDecimal(Model.PendingFactor)), 2),
                Taxable = Math.Round((Model.Taxable * Convert.ToDecimal(Model.PendingFactor)), 2),
                CGSTAmt = Math.Round((Model.CGSTAmt * Convert.ToDecimal(Model.PendingFactor)), 2),
                SGSTAmt = Math.Round((Model.SGSTAmt * Convert.ToDecimal(Model.PendingFactor)), 2),
                IGSTAmt = Math.Round((Model.IGSTAmt * Convert.ToDecimal(Model.PendingFactor)), 2),
                CVDAmt = Math.Round((Model.CVDAmt * Convert.ToDecimal(Model.PendingFactor)), 2),
                CVDCessAmt = Math.Round((Model.CVDCessAmt * Convert.ToDecimal(Model.PendingFactor)), 2),
                CVDExtra = Math.Round((Model.CVDExtra * Convert.ToDecimal(Model.PendingFactor)), 2),
                CVDSCessAmt = Math.Round((Model.CVDSCessAmt * Convert.ToDecimal(Model.PendingFactor)), 2),
                Val1 = Math.Round((Model.Val1 * Convert.ToDecimal(Model.PendingFactor)), 2),
                Narr = Model.Narr,
                HSN = (Model.HSN == null || Model.HSN.Trim() == "") ? "" : Model.HSN,
                GSTCode = (Model.GSTCode == null || Model.GSTCode.Trim() == "") ? "" : Model.GSTCode,
                IGSTRate = Model.IGSTRate,
                CGSTRate = Model.CGSTRate,
                SGSTRate = Model.SGSTRate,
                PAddOnList = Model.PAddOnList,
                
                Store = Model.Store,
                StoreName = GetStoreName(Model.Store),
                
                DelyScheList = Model.DelyScheList,
                MainType = Model.MainType,
                RateOn2 = Model.RateOn2,
                Unit2 = Model.Unit2,
                BinNumber = Model.BinNumber,
                IsBlanketOrder = false,
                
                //ItemType = ctxTFAT.ItemMaster.Where(x => x.Code == Model.Code).Select(x => x.ItemType).FirstOrDefault(),
                RateType = Model.RateType,
                RateCalcType = Model.RateCalcType,
                DiscChargeList = Model.DiscChargeList,
                DiscChargeAmt = Math.Round((Model.DiscChargeAmt * Convert.ToDecimal(Model.PendingFactor)), 2),
                PriceDiscCode = Model.PriceDiscCode,
              
                ClassValues1 = Model.ClassValues1,
                ClassValues2 = Model.ClassValues2,
                PriceRateCode = Model.PriceRateCode,
                ItemSchemeCode = Model.ItemSchemeCode,
               
                QCReqd = mQcreqd,
                QCDone = mqcdone,
                SrNo = Maxtempid + 1,
                tempId = Maxtempid + 1,
                tempIsDeleted = false
            });


            Session.Add("NewItemlist", AddProductList);

            var html = ViewHelper.RenderPartialView(this, "ItemList", new PurchaseVM() { NewItemList = AddProductList, Taxable = AddProductList.Where(x => x.tempIsDeleted == false).Select(x => x.Taxable).Sum(), Val1 = AddProductList.Where(x => x.tempIsDeleted == false).Select(x => x.Val1).Sum(), SGSTAmt = AddProductList.Where(x => x.tempIsDeleted == false).Select(x => x.SGSTAmt).Sum(), CGSTAmt = AddProductList.Where(x => x.tempIsDeleted == false).Select(x => x.CGSTAmt).Sum(), IGSTAmt = AddProductList.Where(x => x.tempIsDeleted == false).Select(x => x.IGSTAmt).Sum(), SubType = Model.SubType, LocationCode = Model.LocationCode });
            var jsonResult = Json(new
            {
                NewItemList = AddProductList,
                TotalQty = AddProductList.Where(x => x.tempIsDeleted == false).Select(x => x.Qty).Sum(),
                Taxable = AddProductList.Where(x => x.tempIsDeleted == false).Select(x => x.Taxable).Sum(),

                SubType = Model.SubType,
                Html = html
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public ActionResult GetItemListforUpdate(PurchaseVM Model)
        {
            try
            {
                var result = (List<PurchaseVM>)Session["NewItemlist"];
                var result1 = result.Where(x => x.tempId == Model.tempId);
                var mdoctypereqdflds = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => new { x.QCReqd, x.MoreQty }).FirstOrDefault();
                Model.DocMoreQty = (mdoctypereqdflds.MoreQty == true) ? "Y" : "N";
                foreach (var item in result1)
                {
                    Model.tempId = item.tempId;
                    Model.Code = item.Code;
                    Model.ItemName = ctxTFAT.ItemMaster.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault() ?? "";
                    Model.Unit = item.Unit;
                    Model.HSN = item.HSN;
                    Model.HSNName = ctxTFAT.HSNMaster.Where(x => x.Code == item.HSN).Select(x => x.Name).FirstOrDefault() ?? "";
                    Model.GSTCode = item.GSTCode;
                    Model.GSTCodeName = ctxTFAT.TaxMaster.Where(x => x.Code == item.GSTCode).Select(x => x.Name).FirstOrDefault() ?? "";
                    Model.DiscOnTaxable = ctxTFAT.TaxMaster.Where(x => x.Code == item.GSTCode).Select(x => x.DiscOnTxbl).FirstOrDefault();
                    Model.Qty = item.Qty;
                    Model.Factor = item.Factor;
                    Model.FreeQty = item.FreeQty;
                    Model.Qty2 = item.Qty2;
                    Model.Rate = item.Rate;
                    Model.Disc = item.Disc;
                    Model.DiscAmt = item.DiscAmt;
                    Model.Taxable = item.Taxable;
                    Model.SGSTAmt = item.SGSTAmt;
                    Model.CGSTAmt = item.CGSTAmt;
                    Model.IGSTAmt = item.IGSTAmt;
                    Model.IGSTRate = item.IGSTRate;
                    Model.CGSTRate = item.CGSTRate;
                    Model.SGSTRate = item.SGSTRate;
                    Model.IGSTRate = item.IGSTRate;
                    Model.CGSTRate = item.CGSTRate;
                    Model.SGSTRate = item.SGSTRate;
                    Model.CVDAmt = item.CVDAmt;
                    Model.CVDCessAmt = item.CVDCessAmt;
                    Model.CVDExtra = item.CVDExtra;
                    Model.CVDSCessAmt = item.CVDSCessAmt;
                    Model.Store = item.Store;
                    //var st = ctxTFAT.Stores.Where(x => x.Code == item.Store).Select(x => x).FirstOrDefault();
                    Model.StoreName = ctxTFAT.Stores.Where(x => x.Code == item.Store).Select(x => x.Name).FirstOrDefault() ?? "";
                    //(st != null) ? st.Name : "";
                    Model.Val1 = item.Val1;
                    Model.Narr = item.Narr;
                    Model.SubType = Model.SubType;
                    Model.MainType = Model.MainType;
                    Model.Mode = Model.Mode;
                    Model.LocationCode = Model.LocationCode;
                    Model.Account = Model.Account;
                    Model.RateOn2 = item.RateOn2;
                    Model.Unit2 = item.Unit2;
                    Model.BinNumber = item.BinNumber;
                    Model.IsBlanketOrder = item.IsBlanketOrder;
                    Model.Inclusive = ctxTFAT.TaxMaster.Where(x => x.Code == item.GSTCode).Select(x => x.Inclusive).FirstOrDefault();
                    //(gs != null) ? gs.Inclusive : false;
                    Model.DiscPerAmt = item.DiscPerAmt;
                    Model.IsPickUp = item.IsPickUp;
                    Model.PickedUpIn = item.PickedUpIn;
                    var itemdet = ctxTFAT.ItemDetail.Where(x => x.Code == item.Code && x.Branch == mbranchcode).Select(x => new { x.Grp, x.MRP, x.QtyTolePlus, x.SerialReq, x.MinQty, x.MaxQty, x.StockSerial, x.BatchReq, x.MinSaleRate, x.ReOrdLevel, x.CheckReOrderLevel, x.QCReqd }).FirstOrDefault();
                    var storeminmax = ctxTFAT.StoreMinMax.Where(x => x.Code == item.Code && x.Store == item.Store).Select(x => new { x.MinQty, x.MaxQty }).FirstOrDefault();
                    var itemmore = ctxTFAT.ItemMore.Where(x => x.Code == item.Code).Select(x => new { x.PackingSize }).FirstOrDefault();
                    Model.QtyTolePlus = itemdet.QtyTolePlus == null ? 0 : itemdet.QtyTolePlus.Value;
                    Model.SerialReq = itemdet.SerialReq;


                    if (storeminmax != null)
                    {
                        Model.MinQty = (storeminmax == null) ? 0 : ((storeminmax.MinQty == null) ? 0 : storeminmax.MinQty.Value);
                        Model.MaxQty = (storeminmax == null) ? 0 : ((storeminmax.MaxQty == null) ? 0 : storeminmax.MaxQty.Value);
                    }
                    else
                    {
                        Model.MinQty = (itemdet == null) ? 0 : ((itemdet.MinQty == null) ? 0 : itemdet.MinQty.Value);
                        Model.MaxQty = (itemdet == null) ? 0 : ((itemdet.MaxQty == null) ? 0 : itemdet.MaxQty.Value);

                    }

                    Model.MinSaleRate = itemdet.MinSaleRate == null ? 0 : itemdet.MinSaleRate.Value;
                    Model.Grp = GetItemGrpName(itemdet.Grp);
                    Model.PackSize = itemmore == null ? 0 : (itemmore.PackingSize == null) ? 0 : itemmore.PackingSize.Value;
                    Model.MRP = itemdet.MRP == null ? 0 : itemdet.MRP.Value;
                    Model.ReOrderLevel = 0;
                    Model.RateType = item.RateType;
                    Model.RateCalcType = item.RateCalcType;
                    Model.DiscChargeList = item.DiscChargeList;
                    Model.DiscChargeAmt = (item.DiscChargeList == null || item.DiscChargeList.Count == 0) ? 0 : Math.Round((item.DiscChargeList.Select(x => (decimal?)x.DiscAmt).Sum() ?? 0), 2);
                    Model.PriceDiscCode = item.PriceDiscCode;

                    Model.ChkReOrderLevel = itemdet.CheckReOrderLevel;

                    Model.PriceRateCode = item.PriceRateCode;
                    Model.ItemSchemeCode = item.ItemSchemeCode;
                    item.LocationCode = Model.LocationCode;
                    Model.OriginalTablekey = item.OriginalTablekey;
                    Model.NewRate = item.NewRate;



                }
                string mStatus = "";
                string mMessage = "";

                var abc = this.RenderPartialView("UpdateItemlist", Model);
                var jsonResult = Json(new { Status = mStatus, Message = mMessage, Html = this.RenderPartialView("UpdateItemlist", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
            catch (Exception ex)
            {
                var jsonResult = Json(new { Status = "Error" }, JsonRequestBehavior.AllowGet);
                return jsonResult;
            }

        }

        public ActionResult UpdateItemList(PurchaseVM Model)
        {
            Model = ServerSideTaxCalculation(Model);

            var result = (List<PurchaseVM>)Session["NewItemlist"];
            var mdoctypereqdflds = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => new { x.QCReqd }).FirstOrDefault();

            foreach (var item in result.Where(x => x.tempId == Model.tempId))
            {
                var PrevCode = item.Code;
                var PrevStore = item.Store;
                item.tempId = Model.tempId;
                item.Code = Model.Code;
                item.ItemName = Model.ItemName;
                item.Unit = Model.Unit;
                item.HSN = (Model.HSN == null || Model.HSN.Trim() == "") ? "" : Model.HSN;
                item.GSTCode = (Model.GSTCode == null || Model.GSTCode.Trim() == "") ? "" : Model.GSTCode;
                item.Qty = Model.Qty;
                item.Factor = Model.Factor;
                item.Qty2 = Model.Qty2;
                item.Rate = Model.Rate;
                item.Disc = Model.Disc;
                item.DiscAmt = Model.DiscAmt;
                item.Taxable = Model.Taxable;
                item.SGSTAmt = Model.SGSTAmt;
                item.CGSTAmt = Model.CGSTAmt;
                item.IGSTAmt = Model.IGSTAmt;
                item.IGSTRate = Model.IGSTRate;
                item.CGSTRate = Model.CGSTRate;
                item.SGSTRate = Model.SGSTRate;
                item.CVDAmt = Model.CVDAmt;
                item.CVDCessAmt = Model.CVDCessAmt;
                item.CVDExtra = Model.CVDExtra;
                item.CVDSCessAmt = Model.CVDSCessAmt;
                item.Val1 = Model.Val1;
                item.Narr = Model.Narr;
                item.tempId = Model.tempId;
                item.Store = Model.Store;
                item.StoreName = GetStoreName(Model.Store);
                item.tempIsDeleted = false;
                item.RateOn2 = Model.RateOn2;
                item.Unit2 = Model.Unit2;
                item.BinNumber = Model.BinNumber;
                item.DiscPerAmt = Model.DiscPerAmt;
                //item.ItemType = ctxTFAT.ItemMaster.Where(x => x.Code == Model.Code).Select(x => x.ItemType).FirstOrDefault();
                item.RateType = Model.RateType;
                item.RateCalcType = Model.RateCalcType;
                item.PriceDiscCode = Model.PriceDiscCode;
                List<PurchaseVM> DelyScheList = new List<PurchaseVM>();
                var DelyScheListVar = result.Where(x => x.tempId == Model.tempId).Select(x => x.DelyScheList).FirstOrDefault();
                if (DelyScheListVar != null && DelyScheListVar.Count > 0)
                {
                    foreach (var b in DelyScheListVar)
                    {
                        DelyScheList.Add(new PurchaseVM()
                        {
                            SrNo = b.tempId,
                            StrDlyDate = b.StrDlyDate,
                            Qty1 = b.Qty1,
                            Qty2 = b.Qty2,
                            AltAddress = b.AltAddress,
                            ExecutedQty = b.ExecutedQty,
                            Pending = b.Pending,
                            Code = Model.Code,
                            Store = Model.Store,
                            tempId = b.tempId,
                        });

                    }
                }

                item.DelyScheList = DelyScheList;
                item.DiscChargeList = Model.DiscChargeList;
                item.DiscChargeAmt = Model.DiscChargeAmt;
                item.ClassValues1 = Model.ClassValues1;
                item.ClassValues2 = Model.ClassValues2;
                item.PriceRateCode = Model.PriceRateCode;
                item.ItemSchemeCode = Model.ItemSchemeCode;
                
                item.NewRate = Model.NewRate;
                var mItemDetQC = ctxTFAT.ItemDetail.Where(x => x.Code == Model.Code && x.Branch == mbranchcode).Select(x => new { x.QCReqd }).FirstOrDefault();
                bool mQcreqd = (mItemDetQC.QCReqd == true && mdoctypereqdflds.QCReqd == true) ? true : false;
                bool mqcdone = (mQcreqd == true) ? false : true;
                item.QCReqd = mQcreqd;
                item.QCDone = mqcdone;


            }

            Session.Add("NewItemlist", result);
            string html;
            html = ViewHelper.RenderPartialView(this, "ItemList", new PurchaseVM() { NewItemList = result, SubType = Model.SubType, Taxable = result.Where(x => x.tempIsDeleted == false).Select(x => x.Taxable).Sum(), Val1 = result.Select(x => x.Val1).Sum(), SGSTAmt = result.Where(x => x.tempIsDeleted == false).Select(x => x.SGSTAmt).Sum(), CGSTAmt = result.Where(x => x.tempIsDeleted == false).Select(x => x.CGSTAmt).Sum(), IGSTAmt = result.Where(x => x.tempIsDeleted == false).Select(x => x.IGSTAmt).Sum(), LocationCode = Model.LocationCode });
            var jsonResult = Json(new
            {
                NewItemList = result,
                SubType = Model.SubType,
                TotalQty = result.Where(x => x.tempIsDeleted == false).Select(x => x.Qty).Sum(),
                Taxable = result.Where(x => x.tempIsDeleted == false).Select(x => x.Taxable).Sum(),
               
                Html = html
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public ActionResult DeleteItemList(PurchaseVM Model)
        {
            var result2 = (List<PurchaseVM>)Session["NewItemlist"];
            var result3 = result2.Where(x => x.tempId == Model.tempId).FirstOrDefault();
            string mactivestring = "";
            mactivestring = CheckDependencyWhileDelUpd(Model.SubType, result3.OriginalTablekey);

            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "The Product is already PickedUp, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var result = result2.Where(x => x.tempId != Model.tempId).ToList();
            Session.Add("NewItemlist", result);
            string html;
            html = ViewHelper.RenderPartialView(this, "ItemList", new PurchaseVM() { NewItemList = result, SubType = Model.SubType, Taxable = result.Where(x => x.tempIsDeleted == false).Select(x => x.Taxable).Sum(), Val1 = result.Where(x => x.tempIsDeleted == false).Select(x => x.Val1).Sum(), SGSTAmt = result.Where(x => x.tempIsDeleted == false).Select(x => x.SGSTAmt).Sum(), CGSTAmt = result.Where(x => x.tempIsDeleted == false).Select(x => x.CGSTAmt).Sum(), IGSTAmt = result.Where(x => x.tempIsDeleted == false).Select(x => x.IGSTAmt).Sum(), LocationCode = Model.LocationCode });
            var jsonResult = Json(new
            {
                NewItemList = result,
                Status = "Success",
                SubType = Model.SubType,
                TotalQty = result.Where(x => x.tempIsDeleted == false).Select(x => x.Qty).Sum(),
                Taxable = result.Where(x => x.tempIsDeleted == false).Select(x => x.Taxable).Sum(),

                Html = html
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public ActionResult UpdateItemByCurrConv(PurchaseVM Model)
        {
            double taxable = 0;
            decimal ratetypetaxable = 0;
            double value = 0;
            decimal discval = 0;
            double tempdiscval = 0;
            decimal taxablewithcurr = 0;
            var result = (List<PurchaseVM>)Session["NewItemlist"];
            string html;
            if (result != null && result.Count != 0)
            {
                foreach (var item in result)
                {
                    #region GSTRATECALCULATION

                    if (Model.VATGSTApp == "G")
                    {
                        string ourstatename = ctxTFAT.Warehouse.Where(x => x.Code == Model.LocationCode).Select(x => x.State).FirstOrDefault();
                        if (ourstatename == null || ourstatename == "")
                            ourstatename = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.State).FirstOrDefault();
                        if (ourstatename == null || ourstatename == "")
                            ourstatename = "MAHARASHTRA";
                        ourstatename = ourstatename.ToUpper();

                        //string partystate = (from i in ctxTFAT.Address where i.Code == Model.DelyCode && i.Sno == Model.DelyAltAdd select i.State).FirstOrDefault().Trim();
                        //if (partystate == null || partystate == "")
                        string partystate = Model.PlaceOfSupply;
                        if (partystate == null || partystate == "")
                        {
                            partystate = "MAHARASHTRA";
                        }
                        partystate = partystate.ToUpper();

                        if (partystate == ourstatename && Model.GSTType != "7")
                        {
                            var resulttax = (from i in ctxTFAT.TaxMaster where i.Code == item.GSTCode select new { i.Code, i.Inclusive, i.CGSTRate, i.SGSTRate, IGSTRate = 0, i.DiscOnTxbl }).FirstOrDefault();
                            if (resulttax != null)
                            {
                                item.Inclusive = resulttax.Inclusive;
                                item.CGSTRate = (resulttax.CGSTRate == null || Model.GSTType == "8" || Model.GSTType == "11") ? 0 : resulttax.CGSTRate.Value;
                                item.SGSTRate = (resulttax.SGSTRate == null || Model.GSTType == "8" || Model.GSTType == "11") ? 0 : resulttax.SGSTRate.Value;
                                item.IGSTRate = 0;
                                item.DiscOnTaxable = resulttax.DiscOnTxbl;
                            }
                            if (Model.GSTType == "6")
                            {
                                item.Inclusive = true;
                            }
                            if (Model.GSTType == "1")
                            {
                                item.Inclusive = false;
                            }
                        }
                        else
                        {

                            var resulttax = (from i in ctxTFAT.TaxMaster
                                             where i.Code == item.GSTCode
                                             select new
                                             {
                                                 i.Code,
                                                 i.Inclusive,
                                                 i.IGSTRate,
                                                 SGSTRate = 0,
                                                 CGSTRate = 0,
                                                 i.DiscOnTxbl
                                             }).FirstOrDefault();
                            if (resulttax != null)
                            {
                                item.Inclusive = resulttax.Inclusive;
                                item.CGSTRate = 0;
                                item.SGSTRate = 0;
                                item.IGSTRate = (resulttax.IGSTRate == null || Model.GSTType == "8" || Model.GSTType == "11") ? 0 : resulttax.IGSTRate.Value;
                                item.DiscOnTaxable = resulttax.DiscOnTxbl;
                            }
                            if (Model.GSTType == "6")
                            {
                                item.Inclusive = true;
                            }
                            if (Model.GSTType == "1")
                            {
                                item.Inclusive = false;
                            }
                        }
                    }
                    else if (Model.VATGSTApp == "V")//vatgstapp = V
                    {
                        var resulttax = (from i in ctxTFAT.TaxMaster where i.Code == item.GSTCode select new { i.Code, i.Inclusive, i.CGSTRate, i.SGSTRate, i.IGSTRate, i.DiscOnTxbl }).FirstOrDefault();
                        if (resulttax != null)
                        {
                            item.Inclusive = resulttax.Inclusive;
                            item.IGSTRate = resulttax.IGSTRate == null ? 0 : resulttax.IGSTRate.Value;
                            item.SGSTRate = resulttax.SGSTRate == null ? 0 : (resulttax.SGSTRate.Value / 100) * resulttax.IGSTRate.Value;
                            item.CGSTRate = resulttax.CGSTRate == null ? 0 : resulttax.CGSTRate.Value;
                            item.DiscOnTaxable = resulttax.DiscOnTxbl;
                        }

                    }
                    else
                    {
                        item.Inclusive = false;
                        item.IGSTRate = 0;
                        item.SGSTRate = 0;
                        item.CGSTRate = 0;

                    }
                    #endregion

                    //var itemmas = ctxTFAT.ItemMaster.Where(x => x.Code == item.Code).Select(x => new { x.RateCalcType, x.RateType }).FirstOrDefault();
                    //Model.RateCalcType = itemmas.RateCalcType;
                    //Model.RateType = itemmas.RateType;
                    if (Model.RateType == "P" || Model.RateType == "V")
                    {
                        ratetypetaxable = GetRateByRatePercentModel(Model.RateType, Model.RateCalcType, item.Rate);
                    }
                    //var taxinclusive = (from i in ctxTFAT.TaxMaster where i.Code == item.GSTCode select i.Inclusive).FirstOrDefault();

                    if (item.Inclusive == false)
                    {

                        if (Model.IsGstDocType == false)
                        {
                            if (Model.RateType == "P" || Model.RateType == "V")
                            {
                                if (item.RateOn2 == true)
                                {
                                    item.Qty2 = 1;
                                    taxable = Convert.ToDouble(ratetypetaxable);
                                }
                                else
                                {
                                    taxable = Convert.ToDouble(ratetypetaxable);
                                }
                            }
                            else
                            {
                                if (item.RateOn2 == true)
                                {
                                    taxable = item.Qty2 * item.Rate;
                                }
                                else
                                {
                                    taxable = item.Qty * item.Rate;
                                }

                            }
                        }
                        else
                        {
                            taxable = Convert.ToDouble(item.Taxable);
                        }

                        tempdiscval = taxable - Convert.ToDouble(item.DiscAmt) - Convert.ToDouble(item.DiscPerAmt) + Convert.ToDouble(item.DiscChargeAmt);
                        discval = Convert.ToDecimal(tempdiscval) * Model.CurrRate;
                        if (Model.SubType == "IM")
                        {
                            taxablewithcurr = discval + item.CVDAmt + item.CVDCessAmt + item.CVDExtra + item.CVDSCessAmt;
                        }
                        else
                        {
                            taxablewithcurr = discval;
                        }

                        item.Taxable = Math.Round(taxablewithcurr, 2);
                        item.CGSTAmt = Math.Round((taxablewithcurr * item.CGSTRate / 100), 2);
                        item.SGSTAmt = Math.Round((taxablewithcurr * item.SGSTRate / 100), 2);
                        item.IGSTAmt = Math.Round((taxablewithcurr * item.IGSTRate / 100), 2);
                        if (Model.GSTType != "1")
                        {
                            item.Val1 = Math.Round((taxablewithcurr + item.CGSTAmt + item.SGSTAmt + item.IGSTAmt), 2);
                        }
                        else
                        {
                            item.Val1 = Math.Round(taxablewithcurr, 2);
                        }

                    }
                    else
                    {
                        if (Model.IsGstDocType == false)
                        {

                            if (Model.RateType == "P" || Model.RateType == "V")
                            {
                                if (item.RateOn2 == true)
                                {
                                    item.Qty2 = 1;
                                    value = Convert.ToDouble(ratetypetaxable);
                                }
                                else
                                {
                                    item.Qty = 1;
                                    value = Convert.ToDouble(ratetypetaxable);
                                }
                            }
                            else
                            {
                                if (item.RateOn2 == true)
                                {
                                    value = item.Qty2 * item.Rate;
                                }
                                else
                                {
                                    value = item.Qty * item.Rate;
                                }
                            }
                        }
                        else
                        {
                            value = Convert.ToDouble(item.Val1);
                        }
                        tempdiscval = value - Convert.ToDouble(item.DiscAmt) - Convert.ToDouble(item.DiscPerAmt) + Convert.ToDouble(item.DiscChargeAmt);
                        discval = Convert.ToDecimal(tempdiscval) * Model.CurrRate;

                        if (Model.SubType == "IM")
                        {
                            taxablewithcurr = Convert.ToDecimal(discval) + item.CVDAmt + item.CVDCessAmt + item.CVDExtra + item.CVDSCessAmt;
                        }
                        else
                        {
                            taxablewithcurr = Convert.ToDecimal(discval);
                        }


                        var gstrate = (item.IGSTRate + item.CGSTRate + item.SGSTRate);
                        item.Taxable = Math.Round((taxablewithcurr * 100 / (gstrate + 100)), 2);

                        item.CGSTAmt = Math.Round((item.Taxable * item.CGSTRate / 100), 2);
                        item.SGSTAmt = Math.Round((item.Taxable * item.SGSTRate / 100), 2);
                        item.IGSTAmt = Math.Round((item.Taxable * item.IGSTRate / 100), 2);
                        item.Val1 = Math.Round(taxablewithcurr, 2);
                        item.Taxable = Math.Round((taxablewithcurr - (item.CGSTAmt + item.SGSTAmt + item.IGSTAmt)), 2);
                    }

                }
                Session.Add("NewItemlist", result);

                html = ViewHelper.RenderPartialView(this, "ItemList", new PurchaseVM() { NewItemList = result, SubType = Model.SubType, Taxable = result.Where(x => x.tempIsDeleted == false).Select(x => x.Taxable).Sum(), Val1 = result.Select(x => x.Val1).Sum(), SGSTAmt = result.Where(x => x.tempIsDeleted == false).Select(x => x.SGSTAmt).Sum(), CGSTAmt = result.Where(x => x.tempIsDeleted == false).Select(x => x.CGSTAmt).Sum(), IGSTAmt = result.Where(x => x.tempIsDeleted == false).Select(x => x.IGSTAmt).Sum(), LocationCode = Model.LocationCode });
                return Json(new
                {
                    NewItemList = result,
                    SubType = Model.SubType,
                    TotalQty = result.Where(x => x.tempIsDeleted == false).Select(x => x.Qty).Sum(),
                    Taxable = result.Where(x => x.tempIsDeleted == false).Select(x => x.Taxable).Sum(),
                    //Val1 = result.Where(x => x.tempIsDeleted == false).Select(x => x.Val1).Sum(),
                    //SGSTAmt = result.Where(x => x.tempIsDeleted == false).Select(x => x.SGSTAmt).Sum(),
                    //CGSTAmt = result.Where(x => x.tempIsDeleted == false).Select(x => x.CGSTAmt).Sum(),
                    //IGSTAmt = result.Where(x => x.tempIsDeleted == false).Select(x => x.IGSTAmt).Sum(),
                    Html = html
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                html = ViewHelper.RenderPartialView(this, "ItemList", new PurchaseVM() { NewItemList = result, SubType = Model.SubType, Taxable = 0, Val1 = 0, SGSTAmt = 0, CGSTAmt = 0, IGSTAmt = 0, LocationCode = Model.LocationCode });
                return Json(new
                {
                    NewItemList = result,
                    SubType = Model.SubType,
                    Taxable = 0,
                    Val1 = 0,
                    SGSTAmt = 0,
                    CGSTAmt = 0,
                    IGSTAmt = 0,
                    Html = html
                }, JsonRequestBehavior.AllowGet);
            }


        }

        public ActionResult GetRateByRatePercent(PurchaseVM Model)
        {
            List<PurchaseVM> objitemlist = new List<PurchaseVM>();
            if (Session["NewItemlist"] != null)
            {
                objitemlist = (List<PurchaseVM>)Session["NewItemlist"];
            }
            if (Model.RateType == "P")
            {
                if (Model.RateCalcType == "V")//ALL Item VALUE
                {
                    if (objitemlist != null && objitemlist.Count > 0)
                    {
                        var itemwithoutser = objitemlist.Where(x => x.ItemType != "S").Select(x => x).ToList();
                        if (itemwithoutser != null && itemwithoutser.Count > 0)
                        {
                            var Taxable = itemwithoutser.Where(x => x.ItemType != "S").Sum(x => (decimal?)x.Taxable) ?? 0;
                            Model.Taxable = Convert.ToDecimal(Model.Rate) * Taxable / 100;
                        }
                        else
                        {
                            Model.Taxable = 0;
                        }
                    }
                    else
                    {
                        Model.Taxable = 0;
                    }

                }
                if (Model.RateCalcType == "P")//pREVIOUS iTEM vALUE
                {
                    if (objitemlist != null && objitemlist.Count > 0)
                    {
                        var itemwithoutser = objitemlist.Where(x => x.ItemType != "S").Select(x => x).ToList();
                        if (itemwithoutser != null && itemwithoutser.Count > 0)
                        {
                            int maxc = itemwithoutser.Where(x => x.ItemType != "S").Select(x => x.tempId).Max();
                            var Taxable = itemwithoutser.Where(x => x.tempId == maxc && x.ItemType != "S").Sum(x => (decimal?)x.Taxable) ?? 0;
                            Model.Taxable = Convert.ToDecimal(Model.Rate) * Taxable / 100;
                        }
                        else
                        {
                            Model.Taxable = 0;
                        }

                    }
                    else
                    {
                        Model.Taxable = 0;
                    }
                }
            }
            else if (Model.RateType == "V")
            {
                if (Model.RateCalcType == "V")//ALL Item VALUE
                {
                    //var Taxable = objitemlist.Where(x => x.ItemType != "S").Sum(x => (decimal?)x.Taxable) ?? 0;
                    Model.Taxable = Convert.ToDecimal(Model.Rate);
                }
                if (Model.RateCalcType == "P")//pREVIOUS iTEM vALUE
                {
                    //int maxc = objitemlist.Where(x => x.ItemType != "S").Select(x => x.tempId).Max();
                    //var Taxable = objitemlist.Where(x => x.tempId == maxc && x.ItemType != "S").Sum(x => (decimal?)x.Taxable) ?? 0;
                    Model.Taxable = Convert.ToDecimal(Model.Rate);
                }
            }

            return Json(Model, JsonRequestBehavior.AllowGet);
        }

        public decimal GetRateByRatePercentModel(string RateType, string RateCalcType, double Rate)
        {
            decimal Taxable = 0;
            List<PurchaseVM> objitemlist = new List<PurchaseVM>();
            if (Session["NewItemlist"] != null)
            {
                objitemlist = (List<PurchaseVM>)Session["NewItemlist"];
            }
            if (RateType == "P")
            {
                if (RateCalcType == "V")
                {
                    if (objitemlist != null && objitemlist.Count > 0)
                    {
                        var itemwithoutser = objitemlist.Where(x => x.ItemType != "S").Select(x => x).ToList();
                        if (itemwithoutser != null && itemwithoutser.Count > 0)
                        {
                            Taxable = itemwithoutser.Where(x => x.ItemType != "S").Sum(x => (decimal?)x.Taxable) ?? 0;
                            Taxable = Convert.ToDecimal(Rate) * Taxable / 100;
                        }
                        else
                        {
                            Taxable = 0;
                        }

                    }
                    else
                    {
                        Taxable = 0;

                    }
                }
                if (RateCalcType == "P")
                {
                    if (objitemlist != null && objitemlist.Count > 0)
                    {
                        var itemwithoutser = objitemlist.Where(x => x.ItemType != "S").Select(x => x).ToList();
                        if (itemwithoutser != null && itemwithoutser.Count > 0)
                        {
                            int maxc = itemwithoutser.Where(x => x.ItemType != "S").Select(x => x.tempId).Max();
                            Taxable = itemwithoutser.Where(x => x.tempId == maxc && x.ItemType != "S").Sum(x => (decimal?)x.Taxable) ?? 0;
                            Taxable = Convert.ToDecimal(Rate) * Taxable / 100;
                        }
                        else
                        {
                            Taxable = 0;
                        }
                    }
                    else
                    {
                        Taxable = 0;

                    }
                }
            }
            else if (RateType == "V")
            {
                if (RateCalcType == "V")
                {
                    //Taxable = objitemlist.Where(x => x.ItemType != "S").Sum(x => (decimal?)x.Taxable) ?? 0;
                    Taxable = Convert.ToDecimal(Rate);
                }
                if (RateCalcType == "P")
                {
                    //int maxc = objitemlist.Where(x => x.ItemType != "S").Select(x => x.tempId).Max();
                    //Taxable = objitemlist.Where(x => x.tempId == maxc && x.ItemType != "S").Sum(x => (decimal?)x.Taxable) ?? 0;
                    Taxable = Convert.ToDecimal(Rate);
                }
            }
            return Taxable;
        }

        public PurchaseVM ServerSideTaxCalculation(PurchaseVM Model)
        {
            #region GSTRATECALCULATION

            if (Model.VATGSTApp == "G")
            {
                string ourstatename = ctxTFAT.Warehouse.Where(x => x.Code == Model.LocationCode).Select(x => x.State).FirstOrDefault();
                if (ourstatename == null || ourstatename == "")
                    ourstatename = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.State).FirstOrDefault();
                if (ourstatename == null || ourstatename == "")
                    ourstatename = "MAHARASHTRA";
                ourstatename = ourstatename.ToUpper();

                //string partystate = (from i in ctxTFAT.Address where i.Code == Model.DelyCode && i.Sno == Model.DelyAltAdd select i.State).FirstOrDefault().Trim();
                //if (partystate == null || partystate == "")
                string partystate = Model.PlaceOfSupply;
                if (partystate == null || partystate == "")
                {
                    partystate = "MAHARASHTRA";
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
                    if (Model.GSTType == "6")
                    {
                        Model.Inclusive = true;
                    }
                    if (Model.GSTType == "1")
                    {
                        Model.Inclusive = false;
                    }
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
                    if (Model.GSTType == "6")
                    {
                        Model.Inclusive = true;
                    }
                    if (Model.GSTType == "1")
                    {
                        Model.Inclusive = false;
                    }
                }
            }
            else if (Model.VATGSTApp == "V")//vatgstapp = V
            {
                var result = (from i in ctxTFAT.TaxMaster where i.Code == Model.GSTCode select new { i.Code, i.Inclusive, i.CGSTRate, i.SGSTRate, i.IGSTRate, i.DiscOnTxbl }).FirstOrDefault();
                if (result != null)
                {
                    Model.Inclusive = result.Inclusive;
                    Model.IGSTRate = result.IGSTRate == null ? 0 : result.IGSTRate.Value;
                    Model.SGSTRate = result.SGSTRate == null ? 0 : (result.SGSTRate.Value / 100) * result.IGSTRate.Value;
                    Model.CGSTRate = result.CGSTRate == null ? 0 : result.CGSTRate.Value;
                    Model.DiscOnTaxable = result.DiscOnTxbl;
                }

            }
            else
            {
                Model.Inclusive = false;
                Model.IGSTRate = 0;
                Model.SGSTRate = 0;
                Model.CGSTRate = 0;

            }
            #endregion

            decimal taxable2 = 0;

            if (Model.RateOn2 == true)
            {
                taxable2 = Convert.ToDecimal((Model.Qty2 * Model.Rate));
            }
            else
            {
                taxable2 = Convert.ToDecimal((Model.Qty * Model.Rate));
            }


            if (Model.Inclusive == false)
            {
                decimal samt = (Model.Disc * taxable2) / 100;
                Model.DiscPerAmt = Math.Round(samt, 2);
            }
            else
            {
                if (Model.DiscOnTaxable == true)
                {
                    decimal samt = taxable2 - ((taxable2 * 100) / (100 + Model.Disc));
                    Model.DiscPerAmt = Math.Round(samt, 2);
                }
                else
                {
                    decimal samt = (Model.Disc * Convert.ToDecimal(taxable2)) / 100;
                    Model.DiscPerAmt = Math.Round(samt, 2);
                }
            }

            if (Model.RateType == "P" || Model.RateType == "V")
            {
                Model.RateTypeTaxable = GetRateByRatePercentModel(Model.RateType, Model.RateCalcType, Model.Rate);

            }

            // Calculate DiscChargesAmt 
            #region DiscChargesCalc
            double discstr2 = 0;
            decimal taxble = 0;
            List<PriceListVM> PriceList = new List<PriceListVM>();


            if (Model.RateOn2 == true)
            {
                taxble = Convert.ToDecimal(Model.Qty2) * Convert.ToDecimal(Model.Rate);
            }
            else
            {
                taxble = Convert.ToDecimal(Model.Qty) * Convert.ToDecimal(Model.Rate);
            }

            if (taxble == 0)
            {
                taxble = 1;
            }
            decimal txable = taxble;
            if (Model.DiscChargeList != null && Model.DiscChargeList.Count > 0 && Model.DiscNotAllowed == false)
            {
                for (int a = 0; a < Model.DiscChargeList.Count(); a++)
                {
                    PriceListVM Model2 = new PriceListVM();
                    if (Model.DiscChargeList[a].AddLess == "D" || Model.DiscChargeList[a].AddLess == null || Model.DiscChargeList[a].AddLess == "")
                    {
                        Model.DiscChargeList[a].AddLess = "";

                    }
                    if (a == 0)
                    {
                        if (Model.DiscChargeList[a].PerValue == true)
                        {
                            discstr2 = GetValDiscCharge(Model.DiscChargeList[a].AddLess + "Cast(" + Model.DiscChargeList[a].Disc + "as float )" + "*" + "Cast(" + txable + "as float )" + "/" + 100);
                        }
                        else
                        {
                            discstr2 = GetValDiscCharge(Model.DiscChargeList[a].AddLess + "Cast(" + Model.DiscChargeList[a].Disc + "as float )");
                        }

                        Model2.Disc = Model.DiscChargeList[a].Disc;
                        Model2.DiscAmt = discstr2;
                        Model2.AddLess = Model.DiscChargeList[a].AddLess;
                        Model2.CalcOn = Model.DiscChargeList[a].CalcOn;
                        Model2.Taxable = txable + Convert.ToDecimal(discstr2);
                        Model2.tempid = a + 1;
                        Model2.DiscCaption = Model.DiscChargeList[a].DiscCaption;
                        Model2.PerValue = Model.DiscChargeList[a].PerValue;
                    }
                    else
                    {
                        if (Model.DiscChargeList[a].CalcOn == "P")
                        {
                            if (Model.DiscChargeList[a].PerValue == true)
                            {
                                discstr2 = GetValDiscCharge(Model.DiscChargeList[a].AddLess + "Cast(" + Model.DiscChargeList[a].Disc + "as float )" + "*" + "Cast(" + taxble + "as float )" + "/" + 100);
                            }
                            else
                            {
                                discstr2 = GetValDiscCharge(Model.DiscChargeList[a].AddLess + "Cast(" + Model.DiscChargeList[a].Disc + "as float )");
                            }
                            Model2.Disc = Model.DiscChargeList[a].Disc;
                            Model2.DiscAmt = discstr2;
                            Model2.AddLess = Model.DiscChargeList[a].AddLess;
                            Model2.CalcOn = Model.DiscChargeList[a].CalcOn;
                            Model2.Taxable = taxble + Convert.ToDecimal(discstr2);
                            Model2.tempid = a + 1;
                            Model2.DiscCaption = Model.DiscChargeList[a].DiscCaption;
                            Model2.PerValue = Model.DiscChargeList[a].PerValue;
                        }
                        else
                        {
                            for (int b = a - 1; b < a; b++)
                            {
                                if (Model.DiscChargeList[a].PerValue == true)
                                {
                                    discstr2 = GetValDiscCharge(Model.DiscChargeList[a].AddLess + "Cast(" + Model.DiscChargeList[a].Disc + "as float )" + "*" + "Cast(" + PriceList[b].Taxable + "as float )" + "/" + 100);
                                }
                                else
                                {
                                    discstr2 = GetValDiscCharge(Model.DiscChargeList[a].AddLess + "Cast(" + Model.DiscChargeList[a].Disc + "as float )");
                                }
                                Model2.Disc = Model.DiscChargeList[a].Disc;
                                Model2.DiscAmt = discstr2;
                                Model2.AddLess = Model.DiscChargeList[a].AddLess;
                                Model2.CalcOn = Model.DiscChargeList[a].CalcOn;
                                Model2.Taxable = PriceList[b].Taxable + Convert.ToDecimal(discstr2);
                                Model2.tempid = a + 1;
                                Model2.DiscCaption = Model.DiscChargeList[a].DiscCaption;
                                Model2.PerValue = Model.DiscChargeList[a].PerValue;
                            }
                        }

                    }
                    PriceList.Add(Model2);
                }

            }

            decimal mDiscAmt = PriceList.Select(x => (decimal?)x.DiscAmt).Sum() ?? 0;
            //mDiscAmt = Math.Round(mDiscAmt, 2);
            Model.DiscChargeAmt = Math.Round(mDiscAmt, 2);
            #endregion
            double taxable;
            double value;
            if (Model.Inclusive == false)
            {
                if (Model.IsGstDocType == false)
                {
                    if (Model.RateType == "P" || Model.RateType == "V")
                    {
                        if (Model.RateOn2 == true)
                        {
                            Model.Qty2 = 1;
                            taxable = Convert.ToDouble(Model.RateTypeTaxable);
                        }
                        else
                        {
                            Model.Qty = 1;
                            taxable = Convert.ToDouble(Model.RateTypeTaxable);
                        }
                    }
                    else
                    {

                        if (Model.RateOn2 == true)
                        {
                            taxable = (Model.Qty2 * Model.Rate);
                        }
                        else
                        {
                            taxable = (Model.Qty * Model.Rate);
                        }
                    }
                }
                else
                {
                    taxable = Convert.ToDouble(Model.Taxable);
                }

                decimal discval;
                decimal tempdiscval;
                tempdiscval = (Convert.ToDecimal(taxable) - Model.DiscAmt - Model.DiscPerAmt + Model.DiscChargeAmt);
                discval = tempdiscval * Model.CurrRate;
                decimal taxablewithcurr;
                if (Model.SubType == "IM")
                {
                    taxablewithcurr = discval + Model.CVDExtra + Model.CVDAmt + Model.CVDCessAmt + Model.CVDSCessAmt;
                }
                else
                {
                    taxablewithcurr = discval;
                }


                Model.Taxable = Math.Round(taxablewithcurr, 2);

                var AmtCgst = (Model.CGSTRate * Model.Taxable) / 100;

                if (Model.SubType == "RP" || Model.SubType == "CP" || Model.SubType == "IM")
                {
                    var tempcgstamt = Math.Abs(Model.CGSTAmt - AmtCgst);
                    if (tempcgstamt <= 1)
                    {

                    }
                    else
                    {
                        Model.CGSTAmt = Math.Round(AmtCgst, 2);
                    }
                }
                else
                {
                    Model.CGSTAmt = Math.Round(AmtCgst, 2);
                }



                var AmtSgst = (Model.SGSTRate * Model.Taxable) / 100;

                if (Model.SubType == "RP" || Model.SubType == "CP" || Model.SubType == "IM")
                {
                    var tempsgstamt = Math.Abs(Model.SGSTAmt - AmtSgst);
                    if (tempsgstamt <= 1)
                    {

                    }
                    else
                    {
                        Model.SGSTAmt = Math.Round(AmtSgst, 2);
                    }
                }
                else
                {
                    Model.SGSTAmt = Math.Round(AmtSgst, 2);
                }


                var AmtIgst = (Model.IGSTRate * Model.Taxable) / 100;


                if (Model.SubType == "RP" || Model.SubType == "CP" || Model.SubType == "IM")
                {
                    var tempigstamt = Math.Abs(Model.IGSTAmt - AmtIgst);
                    if (tempigstamt <= 1)
                    {

                    }
                    else
                    {
                        Model.IGSTAmt = Math.Round(AmtIgst, 2);
                    }
                }
                else
                {
                    Model.IGSTAmt = Math.Round(AmtIgst, 2);
                }

                var Fval = Model.Taxable + Model.CGSTAmt + Model.SGSTAmt + Model.IGSTAmt;
                if (Model.GSTType != "1")
                {
                    Model.Val1 = Math.Round(Fval, 2);
                }
                else
                {
                    Model.Val1 = Math.Round(taxablewithcurr, 2);
                }

            }
            else
            {
                if (Model.IsGstDocType == false)
                {
                    if (Model.RateType == "P" || Model.RateType == "V")
                    {
                        if (Model.RateOn2 == true)
                        {
                            Model.Qty2 = 1;
                            value = Convert.ToDouble(Model.RateTypeTaxable);
                        }
                        else
                        {
                            Model.Qty = 1;
                            value = Convert.ToDouble(Model.RateTypeTaxable);
                        }
                    }
                    else
                    {
                        if (Model.RateOn2 == true)
                        {
                            value = (Model.Qty2 * Model.Rate);
                        }
                        else
                        {
                            value = (Model.Qty * Model.Rate);
                        }
                    }
                }
                else
                {
                    value = Convert.ToDouble(Model.Val1);
                }
                double val1 = value;
                //var samt = Model.DiscAmt;
                //var dicpamt = Model.DiscPerAmt;

                decimal discval;
                decimal tempdiscval;
                tempdiscval = (Convert.ToDecimal(val1) - Model.DiscAmt - Model.DiscPerAmt + Model.DiscChargeAmt);
                discval = tempdiscval * Model.CurrRate;
                decimal taxablewithcurr;
                if (Model.SubType == "IM")
                {
                    taxablewithcurr = discval + Model.CVDExtra + Model.CVDAmt + Model.CVDCessAmt + Model.CVDSCessAmt;
                }
                else
                {
                    taxablewithcurr = discval;
                }


                Model.Val1 = Math.Round(taxablewithcurr, 2);
                decimal gstintaxable = Model.IGSTRate + Model.CGSTRate + Model.SGSTRate;
                decimal taxablevar = (taxablewithcurr * 100) / (gstintaxable + 100);//DONT TREAT IT AS TAXABLE DUE TO POINT DIFFERENCE
                taxablevar = Math.Round(taxablevar, 2);//DONT TREAT IT AS TAXABLE DUE TO POINT DIFFERENCE
                decimal AmtCgst = (Model.CGSTRate * taxablevar) / 100;
                Model.CGSTAmt = Math.Round(AmtCgst, 2);

                decimal AmtSgst = (Model.SGSTRate * taxablevar) / 100;
                Model.SGSTAmt = Math.Round(AmtSgst, 2);

                decimal AmtIgst = (Model.IGSTRate * taxablevar) / 100;
                Model.IGSTAmt = Math.Round(AmtIgst, 2);

                Model.Taxable = (taxablewithcurr - (Model.CGSTAmt + Model.SGSTAmt + Model.IGSTAmt));
            }
            return Model;
        }

        public string CheckDependencyWhileDelUpd(string SubType, string OriginalTablekey)
        {
            string mactivestring = "";
            if (string.IsNullOrEmpty(OriginalTablekey) == true)
            {
                return mactivestring;
            }
            if (SubType == "RS" || SubType == "XS")
            {
                var mactive1 = ctxTFAT.Stock.Where(x => x.InvKey == OriginalTablekey).Select(x => x.TableKey).FirstOrDefault();
                if (mactive1 != null)
                {
                    mactivestring = mactivestring + "\nInvoice: " + mactive1;
                }
            }
            else if (SubType == "NP" || SubType == "NS")
            {
                var mactive1 = ctxTFAT.Stock.Where(x => x.InvKey == OriginalTablekey).Select(x => x.TableKey).FirstOrDefault();
                if (mactive1 != null)
                {
                    mactivestring = mactivestring + "\nInvoice: " + mactive1;
                }
            }
            else if (SubType == "SX" || SubType == "PX")
            {
                var mactive1 = ctxTFAT.Stock.Where(x => x.ChlnKey == OriginalTablekey).Select(x => x.TableKey).FirstOrDefault();
                if (mactive1 != null)
                {
                    mactivestring = mactivestring + "\nInvoice: " + mactive1;
                }
            }

            return mactivestring;
        }

        #endregion

        #region Save Update
        [HttpPost]
        public ActionResult CheckWarnings(PurchaseVM Model)
        {
            string connstring = GetConnectionString();
            string mMessage = "";
            decimal mCreditLimit = 0;
            decimal mCrLimitTole = 0;
            bool mCreditDayCheck = false;
            bool mCreditCheck = false;
            bool mCrLimitPO = false;
            bool mCrLimitWarn = false;
            bool mCrDaysWarn = false;
            Model.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            var masterinfo = ctxTFAT.MasterInfo.Where(x => x.Code == Model.Account).Select(x => new { x.CrLimit, x.CheckCRLimit, x.CheckCRDays, x.CRLimitWithPO, x.CRLimitWarn, x.PriceList, x.CRDaysWarn }).FirstOrDefault();
            if (masterinfo != null)
            {
                mCreditLimit = masterinfo.CrLimit == null ? 0 : masterinfo.CrLimit.Value;
                mCreditCheck = masterinfo.CheckCRLimit;
                mCreditDayCheck = masterinfo.CheckCRDays;
                mCrDaysWarn = masterinfo.CRDaysWarn;
                mCrLimitPO = masterinfo.CRLimitWithPO;
                mCrLimitWarn = masterinfo.CRLimitWarn;

            }

            if (mCreditDayCheck == true && mCrDaysWarn == false && (Model.SubType == "OC" || Model.SubType == "RS" || Model.SubType == "OS"))
            {
                decimal mBal = GetBalance(Model.Account, Model.DocDate, mbranchcode);
                if (mBal > 0)
                {
                    string mCrOverSrl = "";
                    string mOSStr = @"Select Top 1 ParentKey from Ledger Where Branch='" + mbranchcode + "' and Code='" + Model.Account + "' and Debit<>0 and MainType <> 'MV' and MainType <> 'PV' and (Debit+Credit) - isnull((Select Sum(o.Amount) from Outstanding o where o.TableRefKey=Ledger.TableKey),0)<>0 and Left(Ledger.Authorise,1)='A' and DocDate+CrPeriod<'" + MMDDYY(Model.DocDate) + "'";
                    DataTable smDt = GetDataTable(mOSStr, connstring);
                    if (smDt.Rows.Count > 0)
                    {
                        mCrOverSrl = (smDt.Rows[0][0].ToString() == null) ? "0" : smDt.Rows[0][0].ToString();
                    }

                    if (mCrOverSrl != "")
                    {
                        decimal mOnAccAmt = 0;
                        string mStr2 = @"Select Sum(Credit) from Ledger Where Credit <> 0 and Branch='" + mbranchcode + "' and (MainType <> 'MV' and MainType<>'PV') and Code='" + Model.Account + "' and ((Credit)-isnull((Select sum(o.Amount) from Outstanding o where o.TableRefKey=Ledger.TableKey),0)) <> 0 and DocDate<='" + MMDDYY(Model.DocDate) + "'";
                        DataTable smDt2 = GetDataTable(mStr2, connstring);
                        if (smDt2.Rows.Count > 0)
                        {
                            mOnAccAmt = (smDt2.Rows[0][0].ToString() == null || smDt2.Rows[0][0].ToString() == "") ? 0 : Convert.ToDecimal(smDt2.Rows[0][0].ToString());
                        }

                        decimal mOutPending = 0;
                        string mStr3 = @"Select Sum(Debit) from Ledger Where Debit <> 0 and Branch='" + mbranchcode + "' and (MainType <> 'MV' and MainType <> 'PV') and Code='" + Model.Account + "' and ((Credit)-isnull((Select sum(o.Amount) from Outstanding o where o.TableRefKey=Ledger.TableKey),0)) <> 0 and DocDate+CrPeriod<='" + MMDDYY(Model.DocDate) + "'";
                        DataTable smDt3 = GetDataTable(mStr3, connstring);
                        if (smDt3.Rows.Count > 0)
                        {
                            mOutPending = (smDt3.Rows[0][0].ToString() == null || smDt3.Rows[0][0].ToString() == "") ? 0 : Convert.ToDecimal(smDt3.Rows[0][0].ToString());
                        }

                        int mDays = 0;
                        if (mOnAccAmt < mOutPending)
                        {
                            mMessage = "Warning " + mMessage + " The Invoice " + mCrOverSrl + " is Pending for >" + mDays + " Days..";
                        }
                    }
                }
            }

            decimal mBalance = 0;
            if (Model.Mode == "Add")
            {
                if (mCreditCheck == true && mCrLimitWarn == false && (Model.SubType == "RS" || Model.SubType == "OC" || Model.SubType == "OS"))
                {
                    decimal madj = 0;
                    // deduct from the cr.limit the picked up ord. from the current doc.
                    if (mCrLimitPO)
                    {
                        //madj = mresult.Where(x => x.tempIsDeleted != true && x.OrdKey != "").Sum(z => z.Val1);
                        madj = GetCreditPendingPO(Model.Account, Model.SubType);
                    }
                    mBalance = GetBalance(Model.Account, DateTime.Now, mbranchcode);
                    if ((mBalance + Model.Amt - madj) > mCreditLimit + (mCreditLimit * mCrLimitTole / 100))
                    {
                        mMessage = "Warning " + mMessage + " Party will Cross Credit Limit..";
                    }

                }
            }
            else
            {
                if (mCreditCheck == true && mCrLimitWarn == false && (Model.SubType == "RS" || Model.SubType == "OC" || Model.SubType == "OS"))
                {
                    //if (mCreditLimit + Model.Amt > mCreditLimit + (mCreditLimit * mCrLimitTole / 100))
                    //    mMessage = mMessage + "\nParty will Cross Credit Limit..";

                    mBalance = GetBalance(Model.Account, DateTime.Now, mbranchcode);
                    if ((mBalance - Model.PrevInvAmt + Model.Amt) > mCreditLimit + (mCreditLimit * mCrLimitTole / 100))
                    {
                        mMessage = "Warning " + mMessage + " Party will Cross Credit Limit..";
                    }

                }

            }
            Model.Message = mMessage;
            return Json(Model, JsonRequestBehavior.AllowGet);
        }

        public string CheckValidations(PurchaseVM Model)
        {
            string connstring = GetConnectionString();
            var mpara = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Branch).Select(x => new { x.gp_NegStock, x.gp_Serial, x.gp_Batch, x.gp_SONoDupl }).FirstOrDefault();
            var mtfatperd = ctxTFAT.TfatPerd.Where(x => x.PerdCode == mperiod).Select(x => new { x.StartDate, x.LastDate }).FirstOrDefault();
            var trxUserRgts = ctxTFAT.UserRightsTrx.Where(x => x.Code == muserid && x.Type == Model.Type).Select(x => x).FirstOrDefault();

            bool mNegStock = mpara.gp_NegStock;
            bool gpSerial = mpara.gp_Serial;
            bool gpBatch = mpara.gp_Batch;
            var mresult = (List<PurchaseVM>)Session["NewItemlist"];
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


            if (Model.IncoTerms == 0)
                Model.IncoTerms = 100001;
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
            if (mTrx != "")
            {
                //01-Sales Return, 02 - Post Sale Discount, 03 - Deficiency in services, 04 - Correction in Invoice, 05 - Change in POS, 06 - Finalization of Prov.Assess, 07 - Others
                if ((mReturnS == true || mReturnP == true || Model.SourceDoc == "Invoice") && Model.ReasonCode < 0)
                {
                    mMessage = mMessage + "\nReason is Required..";
                }

                if (mTradef && Model.CurrName <= 0)
                {
                    mMessage = mMessage + "\nCurrency is not Defined..";
                }

            }

            if (Model.MainType == "SL" && mRequireTRNS && (Model.Transporter == null || Model.Transporter == ""))
                mMessage = mMessage + "\nTransporter is Required..";

            if (mDocAttach && mRequireAttach)
                mMessage = mMessage + "\nAttachment is Required..";

            if (mFetchWoNo && Model.WONumber == "")
                mMessage = mMessage + "\nWork Order Number is Required..";

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
                    Model.BillNumber = Model.Srl.ToString();;
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

            if (mProjects == true && Model.ProjCode == "0")
            {
                mMessage = mMessage + "\nProject Information is Required..";
            }

            decimal mLimitAmount = (trxUserRgts == null) ? 0 : (trxUserRgts.xLimit == null) ? 0 : trxUserRgts.xLimit.Value;
            if (mLimitAmount != 0 && Model.Amt > mLimitAmount)
            {
                mMessage = mMessage + "\nDocument Amount Limit Restrictions for the User: " + muserid + " is set to Amount: " + mLimitAmount;
            }


            if (Model.Amt < 0 && mAllowNegSales == false)
            {
                mMessage = mMessage + "\nNegative Document Amount not Allowed..";
            }


            if (Model.Amt == 0 && mAllowZeroSales == false)
            {
                mMessage = mMessage + "\nZero Document Amount not Allowed..";
            }


            if (mSkipStock == false && (mresult.Where(x => x.tempIsDeleted != true).Count() == 0))
            {
                mMessage = mMessage + "\nInventory not Entered..";
            }


            string mMsg = CheckEntryDate(ConvertDDMMYYTOYYMMDD(Model.DocuDate), (/*Model.Mode == "Add" ?*/ mBackDated /*: true*/), mStartDate, mLastDate, mBackDays, DateTime.Now, gpHolidayWarn, gpHoliday1, gpHoliday2);
            if (mMsg != "")
            {
                mMessage = mMessage + "\n" + mMsg;
            }


            if (resultdoctype.SalesmanReqd == true && (Model.SalesmanCode == "" || Model.SalesmanCode == null))
            {
                mMessage = mMessage + "\nSalesman is Required..";
            }


            if (resultdoctype.BrokerReqd == true && Model.Broker == 0)
            {
                mMessage = mMessage + "\nBroker is Required..";
            }

            if (resultdoctype.RequireAttach == true)
            {
                var mtempPSAttach = (List<PurchaseVM>)Session["TempPurSaleAttach"];
                if (mtempPSAttach == null)
                {
                    mMessage = mMessage + "\nAttachment is Required..";
                }
            }
            
                if (mresult != null && mresult.Count() > 0)
                {
                    string mduplcode = mresult.GroupBy(x => x.Code).Where(g => g.Count() > 1).Select(y => y.Key).FirstOrDefault();
                    if (string.IsNullOrEmpty(mduplcode) == false)
                    {
                        mMessage = mMessage + "\nProduct is Duplicate Cant Save the Product is .." + mduplcode;
                    }

                }
            


            if (Model.SubType == "OS" && mLocTax == false && mDontDlySchedule == false)
            {

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
                    if (mCrLimitPO)
                    {
                        //madj = mresult.Where(x => x.tempIsDeleted != true && x.OrdKey != "").Sum(z => z.Val1);
                        madj = GetCreditPendingPO(Model.Account, Model.SubType);
                    }
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

            if (mNonStock == false && Model.SubType != "GO" && mTrx != "")
            {
                if (mNegStock == true && "OS~PI~RS~CS~US~XS~OC~NP".Contains(Model.SubType))
                {
                    mStr = CheckNegative(Model);
                    if (mStr != "")
                    {
                        mMessage = mMessage + "\nStock Goes Negative..\nItems: " + mStr;
                    }

                }



                if ((Model.SubType == "RP" || Model.SubType == "IC"))
                {
                    if (Model.BillNumber == "")
                    {
                        mMessage = (Model.SubType == "IC" ? "Challan" : "Bill") + " Number is Required..";
                    }


                    if (Model.SubType == "RP")
                    {
                        string mbill = "";
                        if (Model.Mode == "Add")
                        {
                            mbill = ctxTFAT.Purchase.Where(x => x.DocDate >= mStartDate && x.DocDate <= mLastDate && x.Code == Model.Account && x.SubType == Model.SubType && x.Branch == Model.Branch && x.BillNumber == Model.BillNumber).Select(x => x.BillNumber).FirstOrDefault();
                        }
                        else
                        {
                            mbill = ctxTFAT.Purchase.Where(x => x.TableKey != Model.ParentKey && x.DocDate >= mStartDate && x.DocDate <= mLastDate && x.Code == Model.Account && x.SubType == Model.SubType && x.Branch == Model.Branch && x.BillNumber == Model.BillNumber).Select(x => x.BillNumber).FirstOrDefault();
                        }
                        if (mbill != "" && mbill != null)
                        {
                            mMessage = mMessage + "\nDuplicate Bill Number..";
                        }

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
            if (mresult != null && mresult.Count > 0)
            {
                foreach (var item in mresult)
                {
                    var prodaddons = (from p in ctxTFAT.AddOns.Where(x => x.TableKey == "%@D" + Model.MainType && x.Hide == false && x.Types.Contains(Model.Type) && x.Reqd == true)
                                      select p.Fld).ToList();
                    if (prodaddons != null && prodaddons.Count > 0)
                    {
                        foreach (var mobj in prodaddons)
                        {
                            if (item.PAddOnList != null && item.PAddOnList.Count > 0)
                            {
                                string mval = item.PAddOnList.Where(z => z.Fld == mobj).Select(x => x.ApplCode).FirstOrDefault() ?? "";
                                if (mval == "")
                                    mMessage = mMessage + "\nProduct Addon Input is Required for.." + mobj;
                            }
                            else
                            {
                                mMessage = mMessage + "\nProduct Addon Input is Required for.." + mobj;
                            }

                        }
                    }
                }
            }


            if (mresult != null && Model.NonStock == false && Model.InterBranch == true/* && (Model.SubType == "OC" || Model.SubType == "RS")*/)
            {
                foreach (var a in mresult)
                {
                    string taxcd = GetGSTItemCode(a.Code, Model.Branch);
                    if (string.IsNullOrEmpty(taxcd))
                    {
                        mMessage = mMessage + "\n In Branch " + Model.Branch + " Tax Code Missing For Product.. " + a.Code + " Cant Save";
                    }
                }
            }
            if (mresult != null && Model.NonStock == false && Model.InterBranch == true/* && (Model.SubType == "OC" || Model.SubType == "RS")*/)
            {
                foreach (var a in mresult)
                {
                    int mprstore = ctxTFAT.ItemDetail.Where(x => x.Code == a.Code && x.Branch == Model.Branch).Select(x => x.Store).FirstOrDefault();
                    if (mprstore == 0)
                    {
                        mMessage = mMessage + "\n In Branch " + Model.Branch + " Store Code Missing For Product.. " + a.Code + " Cant Save";
                    }
                }
            }
            return mMessage;
        }

        public string CheckNegative(PurchaseVM Model)
        {
            double mCurStock = 0;
            var mpara = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Branch).Select(x => new { x.gp_NegStock, x.gp_NegStockAsOn }).FirstOrDefault();
            bool mNegStock = mpara.gp_NegStock;
            bool mNegStockAsOn = mpara.gp_NegStockAsOn;
            var mDocdate = ConvertDDMMYYTOYYMMDDinstr(Model.DocuDate);
            if (mNegStock == false)
                return "";
            string mStr = "";

            List<PurchaseVM> ItemStockList = new List<PurchaseVM>();

            var mresult = (List<PurchaseVM>)Session["NewItemlist"];
            if (mresult != null)
            {
                //var mList2 = mresult.Where(x => x.ChlnKey == null || x.ChlnKey == "").ToList();
                foreach (var mobj in mresult)
                {
                    bool NotInStock = ((mobj.ChlnKey != null && mobj.ChlnKey != "" && !(Model.SubType == "SX" || Model.SubType == "NS")) || Model.NonStock == true) ? true : false;
                    if (NotInStock == false)
                    {
                        var checkneg = ctxTFAT.ItemDetail.Where(x => x.Code == mobj.Code && x.Branch == Model.Branch).Select(x => x.CheckStock).FirstOrDefault();
                        if (checkneg == true)
                        {
                            ItemStockList.Add(new PurchaseVM
                            {
                                Code = mobj.Code,
                                Qty = mobj.Qty,
                                Store = mobj.Store,

                            });
                        }
                    }

                }
            }


            var itemstklist = (from i in ItemStockList
                               group i by new { i.Code, i.Store } into g
                               select new PurchaseVM()
                               {
                                   Code = g.Key.Code,
                                   Store = g.Key.Store,
                                   Qty = g.Sum(x => x.Qty),
                               }).ToList();
            List<PurchaseVM> ItemStkList2 = new List<PurchaseVM>();
            foreach (var item in itemstklist)
            {

                if (Model.Mode == "Add")
                {
                    if (mNegStockAsOn == true)
                    {

                        double mmax = 0;
                        string mstr = "Select SUM(Qty) from Stock Where NotInStock = 0 and Code='" + item.Code + "' and Store=" + item.Store + " and DocDate <='" + mDocdate + "' and Left(Authorise,1) = 'A' and Branch = '" + Model.Branch + "'";
                        DataTable mDt = GetDataTable(mstr);
                        if (mDt.Rows.Count > 0)
                        {
                            mmax = (mDt.Rows[0][0].ToString() == null || mDt.Rows[0][0].ToString() == "") ? 0 : Convert.ToDouble(mDt.Rows[0][0].ToString());
                        }
                        mCurStock = mmax;

                    }
                    else
                    {
                        var tday = DateTime.Now.Date;
                        string mDocudate = tday.Day.ToString("D2") + "-" + tday.Month.ToString("D2") + "-" + tday.Year;
                        var mDocdate2 = ConvertDDMMYYTOYYMMDDinstr(mDocudate);
                        double mmax = 0;
                        string mstr = "Select SUM(Qty) from Stock Where NotInStock = 0 and Code='" + item.Code + "' and Store=" + item.Store + " and DocDate <='" + mDocdate2 + "' and Left(Authorise,1) = 'A' and Branch = '" + Model.Branch + "'";
                        DataTable mDt = GetDataTable(mstr);
                        if (mDt.Rows.Count > 0)
                        {
                            mmax = (mDt.Rows[0][0].ToString() == null || mDt.Rows[0][0].ToString() == "") ? 0 : Convert.ToDouble(mDt.Rows[0][0].ToString());
                        }
                        mCurStock = mmax;

                    }
                }
                else
                {
                    if (mNegStockAsOn == true)
                    {
                        double mmax = 0;
                        string mstr = "Select SUM(Qty) from Stock Where NotInStock = 0 and Code='" + item.Code + "' and Store=" + item.Store + " and DocDate <='" + mDocdate + "' and Left(Authorise,1) = 'A' and Branch = '" + Model.Branch + "' and ParentKey <> '" + Model.ParentKey + "'";
                        DataTable mDt = GetDataTable(mstr);
                        if (mDt.Rows.Count > 0)
                        {
                            mmax = (mDt.Rows[0][0].ToString() == null || mDt.Rows[0][0].ToString() == "") ? 0 : Convert.ToDouble(mDt.Rows[0][0].ToString());
                        }
                        mCurStock = mmax;

                    }
                    else
                    {
                        var tday = DateTime.Now.Date;
                        string mDocudate = tday.Day.ToString("D2") + "-" + tday.Month.ToString("D2") + "-" + tday.Year;
                        var mDocdate2 = ConvertDDMMYYTOYYMMDDinstr(mDocudate);
                        double mmax = 0;
                        string mstr = "Select SUM(Qty) from Stock Where NotInStock = 0 and Code='" + item.Code + "' and Store=" + item.Store + " and DocDate <='" + mDocdate2 + "' and Left(Authorise,1) = 'A' and Branch = '" + Model.Branch + "' and ParentKey <> '" + Model.ParentKey + "'";
                        DataTable mDt = GetDataTable(mstr);
                        if (mDt.Rows.Count > 0)
                        {
                            mmax = (mDt.Rows[0][0].ToString() == null || mDt.Rows[0][0].ToString() == "") ? 0 : Convert.ToDouble(mDt.Rows[0][0].ToString());
                        }
                        mCurStock = mmax;
                    }

                }

                ItemStkList2.Add(new PurchaseVM()
                {
                    Code = item.Code,
                    Store = item.Store,
                    Qty = item.Qty,
                    Stock = mCurStock
                });
            }

            mStr = "";

            foreach (var im in ItemStkList2)
            {
                if (im.Qty > im.Stock)
                {
                    mStr = mStr + im.Code + "\nStore: " + im.Store;
                }
            }

            return mStr;
        }

        public string CheckPrimaryKey(PurchaseVM Model)
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
            var checkPkQuery = @"select tablekey from " + mTable + " where tablekey=" + "'" + Model.ParentKey + "'";
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

        public void DeUpdate(PurchaseVM Model)
        {
            string connstring = GetConnectionString();

            var mtds = ctxTFAT.TDSPayments.Where(x => x.ParentKey == Model.ParentKey).FirstOrDefault();
            if (mtds != null)
            {
                ctxTFAT.TDSPayments.Remove(mtds);
            }



            var mobj5 = ctxTFAT.StockTax.Where(x => x.ParentKey == Model.ParentKey).ToList();
            if (mobj5 != null)
            {
                ctxTFAT.StockTax.RemoveRange(mobj5);
            }

            var mobj6 = ctxTFAT.StockMore.Where(x => x.ParentKey == Model.ParentKey).ToList();
            if (mobj6 != null)
            {
                ctxTFAT.StockMore.RemoveRange(mobj6);
            }

            var mobj7 = ctxTFAT.Outstanding.Where(x => x.ParentKey == Model.ParentKey).ToList();
            if (mobj7 != null)
            {
                ctxTFAT.Outstanding.RemoveRange(mobj7);
            }

            var mobj8 = ctxTFAT.DlySchedule.Where(x => x.ParentKey == Model.ParentKey).ToList();
            if (mobj8 != null)
            {
                ctxTFAT.DlySchedule.RemoveRange(mobj8);
            }



            var mobj4 = ctxTFAT.Stock.Where(x => x.ParentKey == Model.ParentKey).ToList();
            if (mobj4 != null)
            {
                ctxTFAT.Stock.RemoveRange(mobj4);
            }


            var mobj2 = ctxTFAT.SalesMore.Where(x => x.TableKey == Model.ParentKey).FirstOrDefault();
            if (mobj2 != null)
            {
                ctxTFAT.SalesMore.Remove(mobj2);
            }

            var mobj1 = ctxTFAT.Sales.Where(x => x.TableKey == Model.ParentKey).FirstOrDefault();
            if (mobj1 != null)
            {
                ctxTFAT.Sales.Remove(mobj1);
            }
            var mobj11 = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey).ToList();
            if (mobj11 != null)
            {
                ctxTFAT.Ledger.RemoveRange(mobj11);
            }




            var mDeleteNote = ctxTFAT.Narration.Where(x => x.TableKey == Model.ParentKey).ToList();
            if (mDeleteNote != null)
            {
                ctxTFAT.Narration.RemoveRange(mDeleteNote);
            }

            //var mDeleteTrans = ctxTFAT.TransportDetail.Where(x => x.TableKey == Model.ParentKey).FirstOrDefault();
            //if (mDeleteTrans != null)
            //{
            //    ctxTFAT.TransportDetail.Remove(mDeleteTrans);
            //}

            var mDeleteDly = ctxTFAT.DlySchedule.Where(x => x.ParentKey == Model.ParentKey).ToList();
            if (mDeleteDly != null)
            {
                ctxTFAT.DlySchedule.RemoveRange(mDeleteDly);
            }

            var mDeleteAttach = ctxTFAT.Attachment.Where(x => x.ParentKey == Model.ParentKey).ToList();
            if (mDeleteAttach != null)
            {
                ctxTFAT.Attachment.RemoveRange(mDeleteAttach);
            }

            var mDeleteTerms = ctxTFAT.TermsDetails.Where(x => x.ParentKey == Model.ParentKey).ToList();
            if (mDeleteTerms != null)
            {
                ctxTFAT.TermsDetails.RemoveRange(mDeleteTerms);
            }

            var mDeleteAuthorise = ctxTFAT.Authorisation.Where(x => x.ParentKey == Model.ParentKey).ToList();
            if (mDeleteAuthorise != null)
            {
                ctxTFAT.Authorisation.RemoveRange(mDeleteAuthorise);
            }

            if (Model.MainType == "PR")
            {
                var addond = ctxTFAT.AddonDocPR.Where(x => x.TableKey == Model.ParentKey).Select(x => x).FirstOrDefault();
                if (addond != null)
                {
                    ctxTFAT.AddonDocPR.Remove(addond);
                }

            }
            else
            {
                var addond = ctxTFAT.AddonDocSL.Where(x => x.TableKey == Model.ParentKey).Select(x => x).FirstOrDefault();
                if (addond != null)
                {
                    ctxTFAT.AddonDocSL.Remove(addond);
                }
            }


            var addoni = ctxTFAT.AddonItemSL.Where(x => x.ParentKey == Model.ParentKey).Select(x => x).ToList();
            if (addoni != null)
            {
                ctxTFAT.AddonItemSL.RemoveRange(addoni);
            }


            ctxTFAT.SaveChanges();
        }

        public ActionResult SaveData(PurchaseVM Model)
        {
            string mTable = "";
            string OriginalParentCode = "";
            string OriginalSubType = "";
            string OriginalSrl = "";
            string OriginalType = "";
            string OriginalPrefix = "";
            string brMessage = "";
            // check validations against rules set in preferences, doctype, itemmaster && account master
            //DELETE FIRST

            if (Model.SaveAsDraft == "N" && Model.AUTHORISE == "X00" && Model.Mode == "Edit")//delete previous drafts before editing by normal save button 
            {
                PurchaseVM mModel = new PurchaseVM();
                mModel.ParentKey = Model.Type + Model.Prefix.Substring(0, 2) + muserid;
                mModel.SubType = Model.SubType;
                mModel.MainType = Model.MainType;
                DeUpdate(mModel);
            }
            if (Model.SaveAsDraft == "Y" && Model.Mode == "Add")//delete previous drafts before saving current new draft 
            {
                PurchaseVM mModel = new PurchaseVM();
                mModel.ParentKey = Model.Type + Model.Prefix.Substring(0, 2) + muserid;
                mModel.SubType = Model.SubType;
                mModel.MainType = Model.MainType;
                DeUpdate(mModel);
            }
            if (Model.SaveAsDraft == "N" && Model.Mode == "Add" && Model.IsDraftSave == true)//delete previous draft when user is in add mode saving document by normal save
            {
                PurchaseVM mModel = new PurchaseVM();
                mModel.ParentKey = Model.Type + Model.Prefix.Substring(0, 2) + muserid;
                mModel.SubType = Model.SubType;
                mModel.MainType = Model.MainType;
                DeUpdate(mModel);
            }
            //
            //Delete existing drafts

            Model.Branch = mbranchcode;
            string mStr = CheckValidations(Model);
            if (mStr != "")
            {
                return Json(new
                {
                    Message = mStr,
                    Status = "CancelError"
                }, JsonRequestBehavior.AllowGet);
            }
            using (var transaction = ctxTFAT.Database.BeginTransaction(System.Data.IsolationLevel.Unspecified))
            {
                try
                {

                    // REMOVE EXISTING DATA FOR EDIT && DELETE MODE
                    if (Model.Mode == "Edit")
                    {
                        if (Model.RevBool == true && Model.Mode == "Edit" && (Model.SubType == "QP" || Model.SubType == "QS" || Model.SubType == "EP" || Model.SubType == "ES" || Model.SubType == "OP" || Model.SubType == "OS")) // Revision Of Quotation
                        {
                            SqlConnection conx = new SqlConnection(GetConnectionString());
                            SqlCommand cmdx = new SqlCommand("dbo.SPTFAT_CopyAndInsertBack", conx);
                            cmdx.CommandType = CommandType.StoredProcedure;
                            cmdx.Parameters.Add("@mparentkey", SqlDbType.VarChar).Value = Model.ParentKey;
                            if (Model.SubType == "QP" || Model.SubType == "QS")
                            {
                                cmdx.Parameters.Add("@mtable", SqlDbType.VarChar).Value = "Quote";
                            }
                            else if (Model.SubType == "EP" || Model.SubType == "ES")
                            {
                                cmdx.Parameters.Add("@mtable", SqlDbType.VarChar).Value = "Enquiry";
                            }
                            else if (Model.SubType == "OP" || Model.SubType == "OS")
                            {
                                cmdx.Parameters.Add("@mtable", SqlDbType.VarChar).Value = "Orders";
                            }

                            conx.Open();
                            cmdx.ExecuteNonQuery();
                            conx.Close();

                            SqlConnection conx2 = new SqlConnection(GetConnectionString());
                            SqlCommand cmdx2 = new SqlCommand("dbo.SPTFAT_CopyAndInsertBack", conx2);
                            cmdx2.CommandType = CommandType.StoredProcedure;
                            cmdx2.Parameters.Add("@mparentkey", SqlDbType.VarChar).Value = Model.ParentKey;
                            if (Model.SubType == "QP" || Model.SubType == "QS")
                            {
                                cmdx2.Parameters.Add("@mtable", SqlDbType.VarChar).Value = "Quotestk";
                            }
                            else if (Model.SubType == "EP" || Model.SubType == "ES")
                            {
                                cmdx2.Parameters.Add("@mtable", SqlDbType.VarChar).Value = "Enquirystk";
                            }
                            else if (Model.SubType == "OP" || Model.SubType == "OS")
                            {
                                cmdx2.Parameters.Add("@mtable", SqlDbType.VarChar).Value = "Ordersstk";
                            }

                            conx2.Open();
                            cmdx2.ExecuteNonQuery();
                            conx2.Close();

                        }


                        if (Model.SaveAsDraft == "N" && Model.AUTHORISE == "X00")//CONVERT DRAFT TO NORMAL 
                        {
                            Model.DocuDate = DateTime.Now.ToString("dd-MM-yyyy");
                            Model.Srl = GetLastSerial(Model.TableName, mbranchcode, Model.Type, Model.Prefix, Model.SubType, ConvertDDMMYYTOYYMMDD(Model.DocuDate));
                            Model.ParentKey = Model.Type + Model.Prefix.Substring(0, 2) + Model.Srl.ToString();;
                        }
                        else
                        {
                            DeUpdate(Model);

                            Model.ParentKey = Model.ParentKey;
                        }

                    }

                    if (Model.Mode == "Add")
                    {
                        if (Model.SaveAsDraft == "Y")
                        {
                            Model.Srl = muserid;
                        }
                        else
                        {
                            if (Model.IsManual == true && Model.IsSaveAs != "Yes")
                            {
                                Model.Srl = Model.Srl;
                            }
                            else
                            {
                                Model.Srl = GetLastSerial(Model.TableName, mbranchcode, Model.Type, Model.Prefix, Model.SubType, ConvertDDMMYYTOYYMMDD(Model.DocuDate));
                            }
                        }

                        Model.ParentKey = Model.Type + Model.Prefix.Substring(0, 2) + Model.Srl.ToString();;
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
                    }


                    if (Model.SaveAsDraft != "Y")
                    {
                        if (Model.AUTHORISE.Substring(0, 1) == "X" || Model.AUTHORISE.Substring(0, 1) == "R" || Model.Mode == "Add" || (Model.Mode == "Edit" && Model.AuthAgain))
                        {
                            if (Model.AUTHORISE.Substring(0, 1) != "N") //if authorised then check for the RateDiff Auth. Rule
                                Model.AUTHORISE = (Model.AuthReq == true ? GetAuthorise(Model.Type, 0, mbranchcode) : Model.AUTHORISE = "A00");
                        }
                    }
                    List<PurchaseVM> result = new List<PurchaseVM>();
                    //mparentkey = Model.ParentKey;
                    var mdoctypeStock = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => new { x.SkipStock, x.QCReqd }).FirstOrDefault();
                    if (mdoctypeStock.SkipStock == "Y")
                    {

                    }
                    else
                    {
                        result = (List<PurchaseVM>)Session["NewItemlist"];
                    }

                    if (Model.Mode == "Add" || Model.Mode == "Edit")
                    {
                        var CheckTypeBranch = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => x.AppBranch).FirstOrDefault();
                        if (mbranchcode != CheckTypeBranch)
                        {
                            brMessage = brMessage + "\nDocument Type is Not of Current Branch Cant Save..";
                        }
                        if (brMessage != "")
                        {
                            return Json(new
                            {
                                Message = brMessage,
                                Status = "CancelError"
                            }, JsonRequestBehavior.AllowGet);
                        }

                        //Calc Cost Rate
                        List<PurchaseVM> PostCharges = new List<PurchaseVM>();
                        foreach (var a in Model.Charges)
                        {
                            string EqCost = "";
                            decimal vals = 0;
                            EqCost = ctxTFAT.Charges.Where(x => x.Fld == a.Fld && x.Type == Model.Type).Select(x => x.EqCost).FirstOrDefault();
                            if (EqCost == "+")
                            {
                                vals = +a.Val1;
                            }
                            if (EqCost == "-")
                            {
                                vals = -a.Val1;
                            }
                            PostCharges.Add(new PurchaseVM()
                            {
                                Code = a.Code,
                                AddLess = a.AddLess,
                                Amt = a.Amt,
                                Val1 = vals,
                                ChgPostCode = a.ChgPostCode,
                                Fld = a.Fld,
                                EqCost = EqCost
                            });

                        }

                        if (result != null)
                        {
                            var mlist = result.Where(x => x.tempIsDeleted != true).ToList();
                            var mChargeTotal = PostCharges.Where(x => x.EqCost == "+" || x.EqCost == "-").Sum(x => (decimal?)x.Val1) ?? 0;
                            var mItemWithChargeTotal = mChargeTotal;
                            var mItemValTotal = mlist.Where(x => x.ItemType != "S").Sum(x => (decimal?)x.Taxable) ?? 1;
                            if (mItemValTotal == 0) mItemValTotal = 1;

                            foreach (var mobj in mlist)
                            {
                                if (mobj.ItemType != "S")
                                {
                                    mobj.Weightage = (mobj.Taxable / mItemValTotal) * 100;
                                }
                                else
                                {
                                    mobj.Weightage = 0;
                                }

                                decimal mtotalcost = mobj.Weightage * mChargeTotal / 100;
                                decimal mcostperitem = 0;
                                if (Model.IsGstDocType == true)
                                {
                                    mcostperitem = (mtotalcost + mobj.Taxable);
                                }
                                else
                                {
                                    mcostperitem = (mtotalcost + mobj.Taxable) / Convert.ToDecimal(mobj.Qty);
                                }
                                if (mobj.ItemType != "S")
                                {
                                    mobj.NewRate = (Model.SubType == "NS" || Model.SubType == "SX" || (Model.SubType == "OC" && Model.InterBranch == true && Model.NonStock == false)) ? mobj.NewRate : Math.Round(mcostperitem, 2);
                                }
                                else
                                {
                                    mobj.NewRate = 0;
                                }
                            }
                            Session.Add("NewItemlist", mlist);
                        }
                        //

                        int xCnt = 1; // used for counts
                        int mFirstCount = 1;
                        int xCnt2 = 1; // used for stockbatch ,stockserial counts


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
                        //else
                        //{
                        //    posneg = 1;
                        //}
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

                            var ordklst = result.Where(x => string.IsNullOrEmpty(x.OrdKey) == false).Select(x => x.OrdKey).FirstOrDefault();

                            //var result = (List<PurchaseVM>)Session["NewItemlist"];
                           
                                #region Normal Sales
                                if (result != null)
                                {
                                    var list = result.Where(x => x.tempIsDeleted != true).ToList();
                                    //Calc New Rate


                                    //
                                    //save data in table Quatetion
                                    Sales mobj1 = new Sales();
                                    mobj1.AltAddress = Convert.ToByte(Model.AltAddress);
                                    mobj1.Amt = Model.Amt * mamtvalneg;
                                    mobj1.AUTHIDS = muserid;
                                    mobj1.AUTHORISE = Model.AUTHORISE;
                                    mobj1.BillDate = ConvertDDMMYYTOYYMMDD(Model.OrdDate);
                                    mobj1.BillNumber = (Model.BillNumber == null || Model.BillNumber.Trim() == "") ? Model.Srl : Model.BillNumber;
                                    mobj1.Branch = mbranchcode;
                                    mobj1.Broker = Model.Broker.ToString();
                                    mobj1.Cess = Convert.ToDecimal(0.00) * mamtvalneg;
                                    mobj1.ChlnDate = ConvertDDMMYYTOYYMMDD(Model.OrdDate);
                                    mobj1.ChlnNumber = "";
                                    mobj1.Code = Model.Account;
                                    mobj1.CrPeriod = Model.CrPeriod;
                                    mobj1.CurrAmount = Model.Amt * mamtvalneg;
                                    mobj1.CurrName = "1";
                                    mobj1.CurrRate = Model.CurrRate;
                                    mobj1.DelyAltAdd = Convert.ToByte(Model.DelyAltAdd);
                                    mobj1.Delycode = (Model.DelyCode == null || Model.DelyCode.Trim() == "") ? Model.Account : Model.DelyCode;
                                    mobj1.DelyDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                    mobj1.BillContact = Model.BillContact;
                                    mobj1.DelyContact = Model.DelyContact;
                                    mobj1.Disc = Convert.ToDecimal(0.00) * mamtvalneg;
                                    mobj1.Discount = Convert.ToDecimal(0.00) * mamtvalneg;
                                    mobj1.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                    mobj1.InsuranceNo = Model.InsuranceNo;
                                    mobj1.LocationCode = Model.LocationCode;
                                    mobj1.MainType = Model.MainType;
                                    mobj1.Narr = Model.RichNote;
                                    mobj1.Prefix = Model.Prefix;
                                    mobj1.Qty = Convert.ToDecimal(list.Select(x => x.Qty).Sum()) * mamtvalneg;
                                    mobj1.Qty2 = list.Select(x => x.Qty2).Sum() * mamtvalneg;
                                    mobj1.Srl = (Model.Srl);
                                    mobj1.SubType = Model.SubType;
                                    mobj1.Taxable = Convert.ToDecimal(list.Select(x => x.Taxable).Sum()) * mamtvalneg;
                                    mobj1.TaxAmt = Convert.ToDecimal(0.00) * mamtvalneg;
                                    mobj1.TaxCode = "";
                                    mobj1.Type = Model.Type;
                                    mobj1.ENTEREDBY = muserid;
                                    mobj1.LASTUPDATEDATE = DateTime.Now;
                                    mobj1.CGSTAmt = Convert.ToDecimal(0.00) * mamtvalneg;
                                    mobj1.GSTNoITC = (Model.GSTNoITC == false) ? 0 : 1;
                                    mobj1.GSTType = (Model.GSTType == null) ? 0 : Convert.ToInt32(Model.GSTType);
                                    mobj1.CGSTAmt = Model.CGSTAmt * mamtvalneg;
                                    mobj1.IGSTAmt = Model.IGSTAmt * mamtvalneg;
                                    mobj1.SGSTAmt = Model.SGSTAmt * mamtvalneg;
                                    mobj1.SalesMan = Model.SalesmanCode;
                                    mobj1.TDSCode = 0;
                                    mobj1.CompCode = mcompcode;
                                    mobj1.TableKey = Model.ParentKey;
                                    mobj1.RoundOff = Model.RoundOff * mamtvalneg;//check sds
                                    mobj1.SourceDoc = Model.SourceDoc;
                                    mobj1.PCCode = 100002;
                                    mobj1.DCCode = 100001;
                                    mobj1.Val1 = Model.Charges.Where(x => x.Fld == "F001").Select(x => x) != null ? GetChargesVal(Model.Charges, "F001") * mamtvalneg : 0;
                                    mobj1.Val2 = Model.Charges.Where(x => x.Fld == "F002").Select(x => x) != null ? GetChargesVal(Model.Charges, "F002") * mamtvalneg : 0;
                                    mobj1.Val3 = Model.Charges.Where(x => x.Fld == "F003").Select(x => x) != null ? GetChargesVal(Model.Charges, "F003") * mamtvalneg : 0;
                                    mobj1.Val4 = Model.Charges.Where(x => x.Fld == "F004").Select(x => x) != null ? GetChargesVal(Model.Charges, "F004") * mamtvalneg : 0;
                                    mobj1.Val5 = Model.Charges.Where(x => x.Fld == "F005").Select(x => x) != null ? GetChargesVal(Model.Charges, "F005") * mamtvalneg : 0;
                                    mobj1.Val6 = Model.Charges.Where(x => x.Fld == "F006").Select(x => x) != null ? GetChargesVal(Model.Charges, "F006") * mamtvalneg : 0;
                                    mobj1.Val7 = Model.Charges.Where(x => x.Fld == "F007").Select(x => x) != null ? GetChargesVal(Model.Charges, "F007") * mamtvalneg : 0;
                                    mobj1.Val8 = Model.Charges.Where(x => x.Fld == "F008").Select(x => x) != null ? GetChargesVal(Model.Charges, "F008") * mamtvalneg : 0;
                                    mobj1.Val9 = Model.Charges.Where(x => x.Fld == "F009").Select(x => x) != null ? GetChargesVal(Model.Charges, "F009") * mamtvalneg : 0;
                                    mobj1.Val10 = Model.Charges.Where(x => x.Fld == "F010").Select(x => x) != null ? GetChargesVal(Model.Charges, "F010") * mamtvalneg : 0;

                                    mobj1.Amt1 = Model.Charges.Where(x => x.Fld == "F001").Select(x => x) != null ? Model.Charges.Where(x => x.Fld == "F001").Select(x => x.Amt).FirstOrDefault() * mamtvalneg : 0;
                                    mobj1.Amt2 = Model.Charges.Where(x => x.Fld == "F002").Select(x => x) != null ? Model.Charges.Where(x => x.Fld == "F002").Select(x => x.Amt).FirstOrDefault() * mamtvalneg : 0;
                                    mobj1.Amt3 = Model.Charges.Where(x => x.Fld == "F003").Select(x => x) != null ? Model.Charges.Where(x => x.Fld == "F003").Select(x => x.Amt).FirstOrDefault() * mamtvalneg : 0;
                                    mobj1.Amt4 = Model.Charges.Where(x => x.Fld == "F004").Select(x => x) != null ? Model.Charges.Where(x => x.Fld == "F004").Select(x => x.Amt).FirstOrDefault() * mamtvalneg : 0;
                                    mobj1.Amt5 = Model.Charges.Where(x => x.Fld == "F005").Select(x => x) != null ? Model.Charges.Where(x => x.Fld == "F005").Select(x => x.Amt).FirstOrDefault() * mamtvalneg : 0;
                                    mobj1.Amt6 = Model.Charges.Where(x => x.Fld == "F006").Select(x => x) != null ? Model.Charges.Where(x => x.Fld == "F006").Select(x => x.Amt).FirstOrDefault() * mamtvalneg : 0;
                                    mobj1.Amt7 = Model.Charges.Where(x => x.Fld == "F007").Select(x => x) != null ? Model.Charges.Where(x => x.Fld == "F007").Select(x => x.Amt).FirstOrDefault() * mamtvalneg : 0;
                                    mobj1.Amt8 = Model.Charges.Where(x => x.Fld == "F008").Select(x => x) != null ? Model.Charges.Where(x => x.Fld == "F008").Select(x => x.Amt).FirstOrDefault() * mamtvalneg : 0;
                                    mobj1.Amt9 = Model.Charges.Where(x => x.Fld == "F009").Select(x => x) != null ? Model.Charges.Where(x => x.Fld == "F009").Select(x => x.Amt).FirstOrDefault() * mamtvalneg : 0;
                                    mobj1.Amt10 = Model.Charges.Where(x => x.Fld == "F010").Select(x => x) != null ? Model.Charges.Where(x => x.Fld == "F010").Select(x => x.Amt).FirstOrDefault() * mamtvalneg : 0;
                                    mobj1.ReasonCode = Model.ReasonCode;
                                    mobj1.PlaceOfSupply = Model.PlaceOfSupply;
                                    mobj1.LoadingKey = Model.LoadKey;
                                    mobj1.ShipFrom = Model.ShipFrom;
                                    ctxTFAT.Sales.Add(mobj1);

                                    // sales more
                                    SalesMore mobj2 = new SalesMore();
                                    mobj2.TableKey = Model.ParentKey;

                                    mobj2.Branch = mbranchcode;
                                    mobj2.Brokerage = Model.Brokerage;
                                    mobj2.BrokerAmt = Model.BrokerAmt;
                                    mobj2.BrokerOn = Model.BrokerOn;
                                    mobj2.DeliverBy = Model.DeliverBy;
                                    mobj2.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                    mobj2.IncoPlace = Model.IncoPlace;
                                    mobj2.IncoTerms = Model.IncoTerms;
                                    mobj2.InvoiceQty = Model.InvoiceQty;
                                    mobj2.LCNo = Model.LCNo;
                                    mobj2.LocationCode = Model.LocationCode;
                                    mobj2.MainType = Model.MainType;
                                    //mobj2.OrdNumber = Model.BillNumber;
                                    mobj2.PayTerms = Model.PayTerms;
                                    mobj2.Prefix = Model.Prefix;
                                    mobj2.ProjCode = Model.ProjCode;
                                   
                                    mobj2.Reason = Model.Reason;
                                    mobj2.ReceiveBy = Model.ReceiveBy;
                                    mobj2.RefBy = Model.RefBy;
                                    mobj2.RefDoc = Model.RefDoc;
                                    mobj2.Reference = Model.Reference;
                                    mobj2.ITFKey = Model.ITFNumber;
                                    mobj2.RefSno = Model.RefSno;
                                    mobj2.RefParty = Model.Account;
                                    mobj2.SalemanAmt = Model.SAmt;
                                    mobj2.SalemanOn = Model.SCommOn;
                                    mobj2.SalemanPer = Model.SCommission;
                                    mobj2.SalePurchNumber = Model.SalePurchNumber;
                                    mobj2.Srl = Model.Srl;
                                    mobj2.SubType = Model.SubType;
                                    mobj2.TaskID = Model.TaskID;
                                    mobj2.TDSAble = (Model.CutTDS == true && (Model.SubType == "RS" || Model.SubType == "NS")) ? Model.TDSAble : 0;
                                    mobj2.TDSAmt = (Model.CutTDS == true && (Model.SubType == "RS" || Model.SubType == "NS")) ? Model.TDSAmt : 0;
                                    mobj2.TDSCess = (Model.CutTDS == true && (Model.SubType == "RS" || Model.SubType == "NS")) ? Model.TDSCess : 0;
                                    mobj2.TDSFlag = (Model.CutTDS == true && (Model.SubType == "RS" || Model.SubType == "NS")) ? Model.CutTDS : false;
                                    mobj2.TDSReason = Model.TDSReason;
                                    mobj2.TDSSchg = (Model.CutTDS == true && (Model.SubType == "RS" || Model.SubType == "NS")) ? Model.TDSSchg : 0;
                                    mobj2.TDSTax = (Model.CutTDS == true && (Model.SubType == "RS" || Model.SubType == "NS")) ? (Model.TDSAmt + Model.TDSCess + Model.TDSSchg + Model.TDSSHECess) : 0;
                                    mobj2.Type = Model.Type;
                                    mobj2.WONumber = Model.WONumber;
                                    mobj2.ENTEREDBY = muserid;
                                    mobj2.AUTHIDS = muserid;
                                    mobj2.AUTHORISE = "A00";
                                    mobj2.LASTUPDATEDATE = DateTime.Now;
                                    mobj2.AdvLic = Model.AdvLicence;

                                    ctxTFAT.SalesMore.Add(mobj2);

                                    xCnt = 1;
                                    mFirstCount = 1;
                                    foreach (var li in list)
                                    {
                                        if (Model.Mode == "Edit")
                                        { xCnt = li.tempId; }
                                        Stock mobjES = new Stock();
                                        mobjES.Amt = Convert.ToDecimal(li.Val1) * posneg;
                                        mobjES.Audited = false;
                                        mobjES.AUTHIDS = muserid;
                                        mobjES.AUTHORISE = Model.AUTHORISE;
                                        mobjES.Branch = mbranchcode;
                                        mobjES.Code = li.Code;
                                        mobjES.cRate = li.Rate;
                                        mobjES.ParentKey = Model.ParentKey;
                                        mobjES.TableKey = Model.Type + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString();;
                                        mobjES.Disc = Convert.ToDecimal(li.Disc) * posneg;
                                        mobjES.DiscAmt = Convert.ToDecimal(li.DiscAmt) * posneg;
                                        mobjES.Discount = (Convert.ToDecimal(li.DiscPerAmt) + Convert.ToDecimal(li.DiscAmt)) * posneg;
                                        mobjES.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                        mobjES.Factor = Convert.ToDouble(li.Factor);
                                        if (mFirstCount == 1)
                                        {
                                            mobjES.FirstSno = true;
                                        }
                                        else
                                        {
                                            mobjES.FirstSno = false;
                                        }
                                        mobjES.FreeQty = li.FreeQty;
                                        mobjES.IsChargeable = false;
                                        mobjES.IsReturnable = false;
                                        mobjES.LocationCode = Model.LocationCode;
                                        mobjES.MainType = Model.MainType;
                                        mobjES.Narr = li.Narr != null ? li.Narr : "";
                                        mobjES.NewRate = Convert.ToDouble(li.NewRate);
                                        mobjES.NewRateEntry = 0;
                                        mobjES.NewRateLink = 0;
                                        mobjES.NotInStock = ((li.ChlnKey != null && li.ChlnKey != "" && !(Model.SubType == "SX" || Model.SubType == "NS")) || Model.NonStock == true) ? true : false;
                                        mobjES.Party = Model.Account;
                                        mobjES.Prefix = Model.Prefix;
                                        mobjES.QCDone = true;
                                        mobjES.QCIssued = false;
                                        mobjES.QCRequire = false;
                                        mobjES.Qty = li.Qty * posneg;
                                        mobjES.Qty2 = li.Qty2 * posneg;
                                        mobjES.Rate = (Model.CurrConv == "Y") ? (li.Rate * Convert.ToDouble(Model.CurrRate)) : li.Rate;
                                        mobjES.RateOn = (li.RateOn2 == true) ? 1 : 0;
                                        mobjES.RatePer = 1;
                                        mobjES.RouteCode = "";
                                        mobjES.Sno = xCnt;
                                        li.RefSno = xCnt;

                                        mobjES.Srl = Model.Srl.ToString();;
                                        mobjES.Stage = 0;
                                        mobjES.Store = li.Store;
                                        mobjES.SubType = Model.SubType;
                                        mobjES.Type = Model.Type;
                                        mobjES.Unit = li.Unit;
                                        mobjES.Unit2 = li.Unit2 == null ? li.Unit : li.Unit2;
                                        mobjES.WasteFlag = "";
                                        mobjES.Weightage = li.Weightage;
                                        mobjES.ENTEREDBY = muserid;
                                        mobjES.LASTUPDATEDATE = DateTime.Now;
                                        mobjES.CompCode = mcompcode;
                                        mobjES.ChlnKey = li.ChlnKey;
                                        mobjES.IndKey = li.IndKey;
                                        mobjES.OrdKey = li.OrdKey;
                                        mobjES.BINNumber = li.BinNumber;
                                        mobjES.InvKey = li.InvKey;
                                        mobjES.PKSKey = li.PKSKey;
                                        mobjES.PCCode = 100002;
                                        mobjES.DCCode = 100001;
                                        mobjES.RateType = li.RateType;
                                        mobjES.RateCalcType = li.RateType;
                                        mobjES.IssueKey = li.IssueKey;
                                        mobjES.QtnKey = li.QtnKey;
                                        if (li.DiscChargeList != null && li.DiscChargeList.Count > 0)
                                        {
                                            for (int i = 0; i < 6; i++)
                                            {
                                                switch (i)
                                                {
                                                    case 0:
                                                        mobjES.ChgPer1 = (li.DiscChargeList.Count > 0) ? Convert.ToDecimal(li.DiscChargeList[i].Disc) : 0;
                                                        mobjES.ChgAmt1 = (li.DiscChargeList.Count > 0) ? Convert.ToDecimal(li.DiscChargeList[i].DiscAmt) : 0;
                                                        break;
                                                    case 1:
                                                        mobjES.ChgPer2 = (li.DiscChargeList.Count > 1) ? Convert.ToDecimal(li.DiscChargeList[i].Disc) : 0;
                                                        mobjES.ChgAmt2 = (li.DiscChargeList.Count > 1) ? Convert.ToDecimal(li.DiscChargeList[i].DiscAmt) : 0;
                                                        break;
                                                    case 2:
                                                        mobjES.ChgPer3 = (li.DiscChargeList.Count > 2) ? Convert.ToDecimal(li.DiscChargeList[i].Disc) : 0;
                                                        mobjES.ChgAmt3 = (li.DiscChargeList.Count > 2) ? Convert.ToDecimal(li.DiscChargeList[i].DiscAmt) : 0;
                                                        break;
                                                    case 3:
                                                        mobjES.ChgPer4 = (li.DiscChargeList.Count > 3) ? Convert.ToDecimal(li.DiscChargeList[i].Disc) : 0;
                                                        mobjES.ChgAmt4 = (li.DiscChargeList.Count > 3) ? Convert.ToDecimal(li.DiscChargeList[i].DiscAmt) : 0;
                                                        break;
                                                    case 4:
                                                        mobjES.ChgPer5 = (li.DiscChargeList.Count > 4) ? Convert.ToDecimal(li.DiscChargeList[i].Disc) : 0;
                                                        mobjES.ChgAmt5 = (li.DiscChargeList.Count > 4) ? Convert.ToDecimal(li.DiscChargeList[i].DiscAmt) : 0;
                                                        break;
                                                    case 5:
                                                        mobjES.ChgPer6 = (li.DiscChargeList.Count > 5) ? Convert.ToDecimal(li.DiscChargeList[i].Disc) : 0;
                                                        mobjES.ChgAmt6 = (li.DiscChargeList.Count > 5) ? Convert.ToDecimal(li.DiscChargeList[i].DiscAmt) : 0;
                                                        break;
                                                }
                                            }
                                        }

                                        mobjES.PriceListDisc = li.PriceDiscCode;
                                        mobjES.xValue1 = li.ClassValues1;
                                        mobjES.xValue2 = li.ClassValues2;
                                        mobjES.PriceListRate = li.PriceRateCode;
                                        mobjES.SchemeCode = li.ItemSchemeCode;

                                        ctxTFAT.Stock.Add(mobjES);

                                        // update data into stockmore table
                                        StockMore mobj3 = new StockMore();
                                        mobj3.AUTHIDS = muserid;
                                        mobj3.AUTHORISE = Model.AUTHORISE;
                                        mobj3.TableKey = Model.Type + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString();;
                                        mobj3.ParentKey = Model.ParentKey;
                                        mobj3.BillDate = ConvertDDMMYYTOYYMMDD(Model.OrdDate);
                                        mobj3.BillNumber = (Model.BillNumber == null || Model.BillNumber.Trim() == "") ? Model.Srl: Model.BillNumber;
                                        mobj3.BOMSrl = "";
                                        mobj3.Branch = mbranchcode;
                                        mobj3.ChlnDate = DateTime.Now;
                                        mobj3.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                        mobj3.Factor = Convert.ToDouble(li.Factor);
                                        mobj3.LocationCode = Model.LocationCode;
                                        mobj3.MainType = Model.MainType;
                                        mobj3.Party = Model.Account;
                                        mobj3.Prefix = Model.Prefix;
                                        mobj3.ProjCode = Model.ProjCode;
                                       
                                        mobj3.QCQty = li.Qty * posneg;
                                        mobj3.Qty = li.Qty * posneg;
                                        mobj3.Qty2 = li.Qty2 * posneg;
                                        mobj3.Sno = xCnt;
                                        mobj3.Srl = Model.Srl.ToString();
                                        mobj3.SubType = Model.SubType;
                                        mobj3.Type = Model.Type;
                                        mobj3.Unit2 = li.Unit2 == null ? li.Unit : li.Unit2;
                                        mobj3.WasteFlag = "";
                                        mobj3.ENTEREDBY = muserid;
                                        mobj3.LASTUPDATEDATE = DateTime.Now;
                                        mobj3.CompCode = mcompcode;
                                        mobj3.AltAddress = Convert.ToInt32(Model.AltAddress);
                                        mobj3.ReasonCode = 0;
                                        ctxTFAT.StockMore.Add(mobj3);

                                        StockTax mobjtax = new StockTax();
                                        mobjtax.TableKey = Model.Type + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString();;
                                        mobjtax.Code = li.Code;
                                        mobjtax.ParentKey = Model.ParentKey;
                                        mobjtax.AUTHIDS = muserid;
                                        mobjtax.AUTHORISE = Model.AUTHORISE;
                                        mobjtax.Branch = mbranchcode;
                                        mobjtax.Cess = Convert.ToDecimal(0.00) * posneg;
                                        mobjtax.Taxable = li.Taxable * posneg;
                                        mobjtax.TaxAmt = Convert.ToDecimal(0.00) * posneg;
                                        mobjtax.TaxCode = li.GSTCode;
                                        mobjtax.ENTEREDBY = muserid;
                                        mobjtax.LASTUPDATEDATE = DateTime.Now;
                                        mobjtax.CGSTAmt = li.CGSTAmt * posneg;
                                        mobjtax.CGSTRate = li.CGSTRate;
                                        mobjtax.GSTNoITC = (Model.GSTNoITC == true) ? 1 : 0;
                                        mobjtax.GSTType = (Model.GSTType == null) ? 0 : Convert.ToInt32(Model.GSTType);
                                        mobjtax.HSNCode = li.HSN;
                                        mobjtax.IGSTAmt = li.IGSTAmt * posneg;
                                        mobjtax.IGSTRate = li.IGSTRate;
                                        mobjtax.SGSTAmt = li.SGSTAmt * posneg;
                                        mobjtax.SGSTRate = li.SGSTRate;
                                        mobjtax.CVDAmt = Convert.ToDecimal(li.CVDAmt) * posneg;
                                        mobjtax.CVDCessAmt = Convert.ToDecimal(li.CVDCessAmt) * posneg;
                                        mobjtax.CVDExtra = Convert.ToDecimal(li.CVDExtra) * posneg;
                                        mobjtax.CVDSCessAmt = Convert.ToDecimal(li.CVDCessAmt) * posneg;
                                        mobjtax.GSTNo = partyaddress.GSTNo;
                                        mobjtax.Party = Model.Account;
                                        mobjtax.AltAddress = Convert.ToInt32(Model.AltAddress);
                                        mobjtax.SubType = Model.SubType;
                                        mobjtax.DealerType = partyaddress.DealerType;
                                        mobjtax.PlaceOfSupply = Model.PlaceOfSupply;
                                        mobjtax.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                        mobjtax.ItemType = "I";
                                        ctxTFAT.StockTax.Add(mobjtax);
                                        xCnt2 = 1;
                                      
                                        xCnt2 = 1;
                                       
                                        

                                        var dlysch = li.DelyScheList;
                                        if (dlysch != null)
                                        {
                                            foreach (var item in dlysch.Where(x => x.tempIsDeleted != true))
                                            {
                                                DlySchedule mdly = new DlySchedule();
                                                mdly.AltAddress = Convert.ToInt16(item.AltAddress);
                                                mdly.AUTHIDS = muserid;
                                                mdly.AUTHORISE = Model.AUTHORISE;
                                                mdly.Branch = mbranchcode;
                                                mdly.Code = li.Code;
                                                mdly.DlyDate = ConvertDDMMYYTOYYMMDD(item.StrDlyDate);
                                                mdly.ExecutedQty = Convert.ToDecimal(item.ExecutedQty);
                                                mdly.ExecutedQty2 = 0;
                                                mdly.LocationCode = Model.LocationCode;
                                                mdly.MainType = Model.MainType;
                                                //mdly.Pending = item.Pending;
                                                mdly.Prefix = Model.Prefix;
                                                mdly.Qty = Convert.ToDecimal(item.Qty1);
                                                mdly.Qty2 = item.Qty2;
                                                mdly.RefNumber = "";
                                                mdly.Reserved = Convert.ToDecimal(0.00);
                                                mdly.Sno = item.SrNo.ToString();
                                                mdly.Srl =Convert.ToInt32(Model.Srl);
                                                mdly.SubType = Model.SubType;
                                                mdly.Type = Model.Type;
                                                mdly.ENTEREDBY = muserid;
                                                mdly.LASTUPDATEDATE = DateTime.Now;
                                                mdly.Days = 0;
                                                mdly.DelySno = item.tempId;
                                                mdly.NotInStock = ((li.ChlnKey != null && li.ChlnKey != "" && !(Model.SubType == "SX" || Model.SubType == "NS")) || Model.NonStock == true) ? true : false;
                                                mdly.ParentKey = Model.ParentKey;
                                                mdly.CompCode = mcompcode;
                                                mdly.TableKey = Model.Type + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString();;
                                                ctxTFAT.DlySchedule.Add(mdly);
                                            }
                                        }

                                      
                                        SaveItemAddons(li.PAddOnList, xCnt, li.Code, Model, (Model.Type + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl));
                                        
                                        if (Model.Mode == "Add")
                                        { ++xCnt; }
                                        mFirstCount = mFirstCount + 1;
                                    }

                                    if (Model.SubType == "RS" || Model.SubType == "CS" || Model.SubType == "XS" || Model.SubType == "NS")
                                    {
                                        foreach (var chg in Model.Charges)
                                        {
                                            if (Model.Mode == "Edit")
                                            { ++xCnt; }
                                            var trncharges = ctxTFAT.Charges.Where(x => x.Type == Model.Type && x.DontUse == false && x.Fld == chg.Fld).Select(x => new { x.TaxCode }).FirstOrDefault();
                                            if (chg.TaxAmt != 0)
                                            {
                                                var taxdet = ctxTFAT.TaxMaster.Where(x => x.Code == trncharges.TaxCode).Select(x => new { x.Pct, x.IGSTRate, x.SGSTRate, x.CGSTRate }).FirstOrDefault();

                                                StockTax mobjtax = new StockTax();
                                                mobjtax.TableKey = Model.Type + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString();;
                                                mobjtax.Code = chg.Fld;
                                                mobjtax.ParentKey = Model.ParentKey;
                                                mobjtax.AUTHIDS = muserid;
                                                mobjtax.AUTHORISE = Model.AUTHORISE;
                                                mobjtax.Branch = mbranchcode;
                                                mobjtax.Cess = 0;
                                                mobjtax.Taxable = chg.Amt;
                                                mobjtax.TaxAmt = chg.TaxAmt;
                                                mobjtax.TaxCode = trncharges.TaxCode;
                                                mobjtax.ENTEREDBY = muserid;
                                                mobjtax.LASTUPDATEDATE = DateTime.Now;
                                                mobjtax.CGSTAmt = (ourstatename == Model.PlaceOfSupply) ? (chg.TaxAmt / 2) : 0;
                                                mobjtax.CGSTRate = (ourstatename == Model.PlaceOfSupply) ? taxdet.CGSTRate : 0;
                                                mobjtax.GSTNoITC = (Model.GSTNoITC == true) ? 1 : 0;
                                                mobjtax.GSTType = (Model.GSTType == null) ? 0 : Convert.ToInt32(Model.GSTType);
                                                mobjtax.HSNCode = "";
                                                mobjtax.IGSTAmt = (ourstatename == Model.PlaceOfSupply) ? 0 : chg.TaxAmt;
                                                mobjtax.IGSTRate = (ourstatename == Model.PlaceOfSupply) ? 0 : taxdet.IGSTRate;
                                                mobjtax.SGSTAmt = (ourstatename == Model.PlaceOfSupply) ? (chg.TaxAmt / 2) : 0;
                                                mobjtax.SGSTRate = (ourstatename == Model.PlaceOfSupply) ? taxdet.SGSTRate : 0;
                                                mobjtax.CVDAmt = 0;
                                                mobjtax.CVDCessAmt = 0;
                                                mobjtax.CVDExtra = 0;
                                                mobjtax.CVDSCessAmt = 0;
                                                mobjtax.GSTNo = partyaddress.GSTNo;
                                                mobjtax.Party = Model.Account;
                                                mobjtax.AltAddress = Convert.ToInt32(Model.AltAddress);
                                                mobjtax.SubType = Model.SubType;
                                                mobjtax.DealerType = partyaddress.DealerType;
                                                mobjtax.PlaceOfSupply = Model.PlaceOfSupply;
                                                mobjtax.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                                mobjtax.ItemType = "C";
                                                ctxTFAT.StockTax.Add(mobjtax);
                                                if (Model.Mode == "Add")
                                                {
                                                    ++xCnt;
                                                }

                                            }

                                        }
                                    }
                                }
                                int lCnt = 1;
                                if (Model.SubType == "RS" || Model.SubType == "CS" || Model.SubType == "XS" || Model.SubType == "NS")
                                {
                                    var ledpost = Model.LedgerPostList;
                                    for (int u = 0; u < ledpost.Count; u++)
                                    {
                                        Ledger mobjL = new Ledger();
                                        mobjL.AltCode = ledpost[u].Code;
                                        mobjL.Audited = true;
                                        mobjL.AUTHIDS = muserid;
                                        mobjL.AUTHORISE = Model.AUTHORISE;
                                        mobjL.BillDate = ConvertDDMMYYTOYYMMDD(Model.OrdDate);
                                        mobjL.BillNumber = Model.BillNumber == null ? "" : Model.BillNumber;
                                        mobjL.Branch = mbranchcode;
                                        mobjL.Cheque = "";
                                        mobjL.ChequeReturn = false;
                                        mobjL.ChqCategory = 1;
                                        mobjL.ClearDate = DateTime.Now;
                                        mobjL.Code = ledpost[u].Code;
                                        mobjL.Credit = Convert.ToDecimal(ledpost[u].Credit);
                                        mobjL.CrPeriod = Model.CrPeriod;
                                        mobjL.CurrAmount = Convert.ToDecimal(ledpost[u].Credit + ledpost[u].Debit);
                                        mobjL.CurrName = Model.CurrName;
                                        mobjL.CurrRate = Model.CurrRate;
                                        mobjL.Debit = Convert.ToDecimal(ledpost[u].Debit);
                                        mobjL.Discounted = true;
                                        mobjL.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                        mobjL.DueDate = DateTime.Now;
                                        mobjL.LocationCode = Model.LocationCode;
                                        mobjL.MainType = Model.MainType;
                                        mobjL.Narr = Model.RichNote;
                                        mobjL.Party = ledpost[u].Code;
                                        mobjL.Prefix = Model.Prefix;
                                        mobjL.RecoFlag = "";
                                        mobjL.RefDoc = "";
                                        mobjL.Reminder = true;
                                        mobjL.Sno = ledpost[u].tempId;
                                        mobjL.Srl = Model.Srl.ToString();;
                                        mobjL.SubType = Model.SubType;
                                        mobjL.TaskID = Model.TaskID;
                                        mobjL.TDSChallanNumber = "";
                                        mobjL.TDSCode = Model.TDSCode;
                                        mobjL.TDSFlag = ledpost[u].TDSFlag;
                                        mobjL.Type = Model.Type;
                                        mobjL.ENTEREDBY = muserid;
                                        mobjL.LASTUPDATEDATE = DateTime.Now;
                                        mobjL.ChequeDate = DateTime.Now;
                                        mobjL.CompCode = mcompcode;
                                        mobjL.ParentKey = Model.ParentKey;
                                        mobjL.TableKey = Model.Type + Model.Prefix.Substring(0, 2) + lCnt.ToString("D3") + Model.Srl.ToString();;
                                        mobjL.PCCode = 100002;
                                        ctxTFAT.Ledger.Add(mobjL);


                                        //LedgerBranch mLB = new LedgerBranch();
                                        ////mLB. = ledpost[u].Code;
                                        //mLB.Audited = true;
                                        //mLB.AUTHIDS = muserid;
                                        //mLB.AUTHORISE = Model.AUTHORISE;
                                        //mLB.BillDate = ConvertDDMMYYTOYYMMDD(Model.OrdDate);
                                        //mLB.BillNumber = Model.BillNumber == null ? "" : Model.BillNumber;
                                        //mLB.Branch = mbranchcode;
                                        //mLB.Cheque = "";
                                        //mLB.ChequeReturn = false;
                                        //mLB.ChqCategory = 1;
                                        //mLB.ClearDate = DateTime.Now;
                                        //mLB.Code = ledpost[u].Code;
                                        //mLB.Credit = Convert.ToDecimal(ledpost[u].Credit);
                                        ////mLB.CrPeriod = Model.CrPeriod;
                                        //mLB.CurrAmount = Convert.ToDecimal(ledpost[u].Credit + ledpost[u].Debit);
                                        //mLB.CurrName = Model.CurrName;
                                        //mLB.CurrRate = Model.CurrRate;
                                        //mLB.Debit = Convert.ToDecimal(ledpost[u].Debit);
                                        ////mLB.Discounted = true;
                                        //mLB.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                        ////mLB.DueDate = DateTime.Now;
                                        //mLB.LocationCode = Model.LocationCode;
                                        //mLB.MainType = Model.MainType;
                                        //mLB.Narr = Model.RichNote;
                                        //mLB.Party = ledpost[u].Code;
                                        //mLB.Prefix = Model.Prefix;
                                        //mLB.ProjCode = Model.ProjCode;
                                        //mLB.ProjectStage = Model.ProjectStage;
                                        //mLB.ProjectUnit = Model.ProjectUnit;
                                        //mLB.RecoFlag = "";
                                        //mLB.RefDoc = "";
                                        ////mLB.Reminder = true;
                                        //mLB.Sno = ledpost[u].tempId;
                                        //mLB.Srl = Model.Srl.ToString();;
                                        //mLB.SubType = Model.SubType;
                                        ////mLB.TaskID = Model.TaskID;
                                        ////mLB.TDSChallanNumber = "";
                                        //mLB.TDSCode = Model.TDSCode;
                                        //mLB.TDSFlag = true;
                                        //mLB.Type = Model.Type;
                                        //mLB.ENTEREDBY = muserid;
                                        //mLB.LASTUPDATEDATE = DateTime.Now;
                                        //mLB.ChequeDate = DateTime.Now;
                                        //mLB.CompCode = mcompcode;
                                        //mLB.ParentKey = Model.ParentKey;
                                        //mLB.TableKey = Model.Type + Model.Prefix.Substring(0, 2) + lCnt.ToString("D3") + Model.Srl.ToString();;
                                        //mLB.PCCode = 100002;
                                        //mLB.xBranch = mbranchcode;
                                        //ctxTFAT.LedgerBranch.Add(mLB);
                                        ++lCnt;
                                    }
                                }
                                if (Model.SubType == "RS" || Model.SubType == "CS" || Model.SubType == "NS")
                                {
                                    SaveOutstandingDetails(Model);
                                }

                                #endregion
                          

                        }
                        #endregion
                        if (Model.MainType == "SL")
                        {
                            SaveTDSPayments(Model);
                        }
                        SaveTransportDetails(Model, Model.ParentKey);
                        SaveAddons(Model);
                        SaveNarration(Model, Model.ParentKey);
                        SaveAttachment(Model);
                        SaveTermsCondition(Model);




                        ctxTFAT.SaveChanges();
                        transaction.Commit();
                        transaction.Dispose();


                        UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, Model.ParentKey, DateTime.Now, Model.Amt, Model.Account, "", "A");
                        if (Model.AUTHORISE.Substring(0, 1) != "A")
                        {
                            if (Model.AUTHORISE.Substring(0, 1) != "X")
                            {
                                string mAuthUser;
                                if (Model.AUTHORISE.Substring(0, 1) == "D")
                                {
                                    //mAuthUser=SaveAuthoriseDOC( Model.ParentKey, 0, Model.Date, mcurrency, mbranchcode, muserid);
                                }
                                else
                                {
                                    mAuthUser = SaveAuthorise(Model.ParentKey, Model.Amt, Model.DocuDate, Model.CurrName, 1, DateTime.Now, Model.Account, mbranchcode, muserid, -1);
                                    SendAuthoriseMessage(mAuthUser, Model.ParentKey, Model.DocuDate, Model.AUTHORISE, Model.AccountName);
                                }
                            }

                        }
                        if (ctxTFAT.DocTypes.Where(z => z.Code == OriginalType).Select(x => x.SendAlert).FirstOrDefault() == true)
                        {
                            SendPartywiseSMS(OriginalSubType, OriginalParentCode, Model.Account, Model.AltAddress, true, mbranchcode);
                        }

                        if (Model.AUTHORISE != "X00")
                        {
                            //SendTrnsMsg(Model.Mode, Model.Amt, mbranchcode + OriginalParentCode, ConvertDDMMYYTOYYMMDD(Model.DocuDate), Model.Account);
                        }

                        if (Model.SaveAsDraft != "Y")
                        {
                            Session["NewItemlist"] = null;
                            Session["TempPurSaleAttach"] = null;
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
                NewSrl = (mbranchcode + OriginalParentCode),
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteData(PurchaseVM Model)
        {
            // Check for Active Documents
            string mactivestring = "";
            if (Model.SubType == "RS" || Model.SubType == "XS")
            {
                var TableKeys = ctxTFAT.Stock.Where(x => x.ParentKey == Model.ParentKey).Select(x => x.TableKey).ToList();
                var mactive1 = ctxTFAT.Stock.Where(x => TableKeys.Contains(x.InvKey)).Select(x => x.TableKey).FirstOrDefault();
                if (mactive1 != null)
                {
                    mactivestring = mactivestring + "\nInvoice: " + mactive1;
                }
            }
            else if (Model.SubType == "NP" || Model.SubType == "NS")
            {
                var TableKeys = ctxTFAT.Stock.Where(x => x.ParentKey == Model.ParentKey).Select(x => x.TableKey).ToList();
                var mactive1 = ctxTFAT.Stock.Where(x => TableKeys.Contains(x.InvKey)).Select(x => x.TableKey).FirstOrDefault();
                if (mactive1 != null)
                {
                    mactivestring = mactivestring + "\nInvoice: " + mactive1;
                }
            }
            else if (Model.SubType == "SX" || Model.SubType == "PX")
            {
                var TableKeys = ctxTFAT.Stock.Where(x => x.ParentKey == Model.ParentKey).Select(x => x.TableKey).ToList();
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

            using (var transaction = ctxTFAT.Database.BeginTransaction(System.Data.IsolationLevel.Unspecified))
            {
                try
                {
                    DeUpdate(Model);
                    UpdateAuditTrail(mbranchcode, "Delete", Model.Header, Model.ParentKey, DateTime.Now, 0, Model.ParentKey, "", "A");

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

        #endregion

        #region LedgerPost

        public ActionResult GetPostingNew(PurchaseVM Model)
        {
            string mpostcode;
            int n;
            string mIGSTCode = "";
            string mCGSTCode = "";
            string mSGSTCode = "";
            decimal mMultTax = 0;
            decimal mCVDAmt = 0;
            decimal mCVDCess = 0;
            decimal mCVDExtra = 0;
            decimal mCVDSCess = 0;
            decimal mvalue = 0;
            decimal mTaxable = 0;
            decimal mDebit = 0;
            decimal mCredit = 0;
            decimal mPostAmt = Model.Amt;
            decimal mWeightage = 0;
            decimal mtotalcost = 0;
            decimal mcostperitem = 0;


            string mCode = "";
            int xCnt = 0;
            string mTrx = "";
            if (Model.MainType == "SL" && "ES~OS~QS~PI~SG".Contains(Model.SubType) == false)
            {
                mTrx = "S";
            }
            else if (Model.MainType == "PR" && "EP~OP~QP~PG".Contains(Model.SubType) == false)
            {
                mTrx = "P";
            }

            string TaxAmtStr = "";
            foreach (var a in Model.Charges)
            {
                TaxAmtStr = TaxAmtStr + a.AddLess + a.TaxAmt;
            }
            decimal chggstamt = GetAmtValue(TaxAmtStr);
            var resultdoctype = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => x).FirstOrDefault();
            bool mProductPost = resultdoctype.ProductPost;
            bool mTradef = resultdoctype.CurrConv == "Y" ? true : false;
            List<PurchaseVM> mTaxPostCode2 = new List<PurchaseVM>();
            //Array mTaxAmt = Array.CreateInstance(typeof(decimal), 50);
            //bool mRCM = CheckRCM(Convert.ToInt32(Model.GSTType), Model.DocDate, mRCMDate);
            bool mRCM = false;
            if (Model.GSTType == "1")
            {
                mRCM = true;
            }

            List<PurchaseVM> mProdPostCode2 = new List<PurchaseVM>();

            // create item wise posting array
            var mresult = (List<PurchaseVM>)Session["NewItemlist"];
            if (mresult != null)
            {
                var mlist = mresult.Where(x => x.tempIsDeleted != true).ToList();
                //var mChargeTotal = PostCharges.Where(x => x.EqCost == "+" || x.EqCost == "-").Sum(x => (decimal?)x.Val1) ?? 0;
                //var mItemWithChargeTotal = mChargeTotal;
                var mItemValTotal = mlist.Where(x => x.ItemType != "S").Sum(x => (decimal?)x.Taxable) ?? 1;
                if (mItemValTotal == 0) mItemValTotal = 1;
                foreach (var mobj in mlist)
                {
                    mMultTax = mMultTax + mobj.SGSTAmt + mobj.CGSTAmt + mobj.IGSTAmt;// + mobj.CessAmt
                    mCVDAmt = mCVDAmt + mobj.CVDAmt;
                    mCVDCess = mCVDCess + mobj.CVDCessAmt;
                    mCVDExtra = mCVDExtra + mobj.CVDExtra;
                    mCVDSCess = mCVDSCess + mobj.CVDSCessAmt;
                    mTaxable = mTaxable + mobj.Taxable;

                    if (mProductPost == true)
                    {
                        //--------------- Product posting details
                        if (Model.MainType == "PR")
                        {
                            if (mTradef == false)
                                mpostcode = ctxTFAT.ItemDetail.Where(x => x.Code == mobj.Code && x.Branch == mbranchcode).Select(x => x.AcCode).FirstOrDefault();
                            else
                                mpostcode = ctxTFAT.ItemDetail.Where(x => x.Code == mobj.Code && x.Branch == mbranchcode).Select(x => x.AcCodeX).FirstOrDefault();
                        }
                        else
                        {
                            if (mTradef == false)
                                mpostcode = ctxTFAT.ItemDetail.Where(x => x.Code == mobj.Code && x.Branch == mbranchcode).Select(x => x.AcCodeS).FirstOrDefault();
                            else
                                mpostcode = ctxTFAT.ItemDetail.Where(x => x.Code == mobj.Code && x.Branch == mbranchcode).Select(x => x.AcCodeSX).FirstOrDefault();
                        }

                        if (mpostcode == "")
                            mpostcode = mTradef == true ? (Model.MainType == "PR" ? "000100017" : "000100025") : (Model.MainType == "PR" ? "000100004" : "000100001");


                        mvalue = mobj.Val1 * (mobj.Qty < 0 ? -1 : 1) /** Model.CurrRate*/;
                        //mvalue = Convert.ToDecimal(mProdPostAmt.GetValue(n)) + mvalue;    // add to existin array value
                        if (mRCM == false /*&& Model.CurrRate == 1*/)
                            mvalue = mvalue - (mobj.SGSTAmt + mobj.CGSTAmt + mobj.IGSTAmt) * (mobj.Qty < 0 ? -1 : 1);

                        // mProdPostAmt.SetValue(mvalue, n);

                        mProdPostCode2.Add(new PurchaseVM()
                        {
                            PostAccount = mpostcode,
                            AccAmt = mvalue
                        });
                    }
                    //=============== product post array ends here

                    if (Model.VATGSTApp == "G")
                    {
                        //--------------- GST posting starts
                        if ((mTrx == "P" && mRCM == false && mTradef == false) || mTrx == "S")
                        {
                            var mtaxs = ctxTFAT.TaxMaster.Select(x => new
                            {
                                x.Code,
                                x.SGSTCode,
                                x.CGSTCode,
                                x.IGSTCode,
                                x.CessCode,
                                x.Scope
                            }).Where(z => z.Scope == Model.MainType.Substring(0, 1) && z.Code == mobj.GSTCode).FirstOrDefault();
                            if (mtaxs != null)
                            {
                                // igst
                                if (mobj.IGSTAmt != 0)
                                {

                                    mTaxPostCode2.Add(new PurchaseVM()
                                    {
                                        TaxCode = mtaxs.IGSTCode,
                                        TaxAmt = mobj.IGSTAmt
                                    });
                                }
                                // cgst
                                if (mobj.CGSTAmt != 0)
                                {

                                    mTaxPostCode2.Add(new PurchaseVM()
                                    {
                                        TaxCode = mtaxs.CGSTCode,
                                        TaxAmt = mobj.CGSTAmt
                                    });
                                }
                                // sgst
                                if (mobj.SGSTAmt != 0)
                                {


                                    mTaxPostCode2.Add(new PurchaseVM()
                                    {
                                        TaxCode = mtaxs.SGSTCode,
                                        TaxAmt = mobj.SGSTAmt
                                    });
                                }
                                // cess
                                if (mobj.CessAmt != 0)
                                {

                                    mTaxPostCode2.Add(new PurchaseVM()
                                    {
                                        TaxCode = mtaxs.CessCode,
                                        TaxAmt = mobj.CessAmt
                                    });
                                }
                            }
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
                            if (mobj.IGSTAmt != 0)
                            {

                                mTaxPostCode2.Add(new PurchaseVM()
                                {
                                    TaxCode = mIGSTCode,
                                    TaxAmt = mobj.IGSTAmt * (mTrx == "P" ? -1 : 1)
                                });
                            }

                            // cgst
                            if (mobj.CGSTAmt != 0)
                            {

                                mTaxPostCode2.Add(new PurchaseVM()
                                {
                                    TaxCode = mCGSTCode,
                                    TaxAmt = mobj.CGSTAmt * (mTrx == "P" ? -1 : 1)
                                });
                            }
                            // sgst
                            if (mobj.SGSTAmt != 0)
                            {

                                mTaxPostCode2.Add(new PurchaseVM()
                                {
                                    TaxCode = mSGSTCode,
                                    TaxAmt = mobj.SGSTAmt * (mTrx == "P" ? -1 : 1)
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
                            if (mobj.IGSTAmt != 0)
                            {

                                mTaxPostCode2.Add(new PurchaseVM()
                                {
                                    TaxCode = mIGSTCode,
                                    TaxAmt = mobj.IGSTAmt
                                });
                            }
                            // cgst
                            if (mobj.CGSTAmt != 0)
                            {

                                mTaxPostCode2.Add(new PurchaseVM()
                                {
                                    TaxCode = mCGSTCode,
                                    TaxAmt = mobj.CGSTAmt
                                });
                            }
                            // sgst
                            if (mobj.SGSTAmt != 0)
                            {

                                mTaxPostCode2.Add(new PurchaseVM()
                                {
                                    TaxCode = mSGSTCode,
                                    TaxAmt = mobj.SGSTAmt
                                });
                            }
                        }
                    }
                    else // vat posting
                    {
                        // igstcode=taxamt, sgstcode=surcharge, cgstcode=addtax
                        var mtaxs = ctxTFAT.TaxMaster.Select(x => new { x.Code, x.SGSTCode, x.CGSTCode, x.IGSTCode, x.Scope }).Where(z => z.Scope == Model.MainType.Substring(0, 1) && z.Code == mobj.GSTCode).FirstOrDefault();
                        if (mtaxs != null)
                        {
                            // igst = taxamt
                            if (mobj.IGSTAmt != 0)
                            {
                                //n = Array.BinarySearch(mTaxPostCode, mtaxs.IGSTCode);
                                //if (n < 0)
                                //{
                                //    n = Array.BinarySearch(mTaxPostCode, null);      //find next blank array
                                //    mTaxPostCode.SetValue(mtaxs.IGSTCode, n);
                                //}
                                //mTaxAmt.SetValue(Convert.ToDecimal(mTaxAmt.GetValue(n)) + mobj.IGSTAmt, n);


                                mTaxPostCode2.Add(new PurchaseVM()
                                {
                                    TaxCode = mtaxs.IGSTCode,
                                    TaxAmt = mobj.IGSTAmt
                                });
                            }
                            // sgst = surcharge
                            if (mobj.SGSTAmt != 0)
                            {
                                //n = Array.BinarySearch(mTaxPostCode, mtaxs.SGSTCode);
                                //if (n < 0)
                                //{
                                //    n = Array.BinarySearch(mTaxPostCode, null);      //find next blank array
                                //    mTaxPostCode.SetValue(mtaxs.SGSTCode, n);
                                //}
                                //mTaxAmt.SetValue(Convert.ToDecimal(mTaxAmt.GetValue(n)) + mobj.SGSTAmt, n);

                                mTaxPostCode2.Add(new PurchaseVM()
                                {
                                    TaxCode = mtaxs.SGSTCode,
                                    TaxAmt = mobj.SGSTAmt
                                });
                            }
                            // cgst = addtax
                            if (mobj.CGSTAmt != 0)
                            {
                                //n = Array.BinarySearch(mTaxPostCode, mtaxs.CGSTCode);
                                //if (n < 0)
                                //{
                                //    n = Array.BinarySearch(mTaxPostCode, null);      //find next blank array
                                //    mTaxPostCode.SetValue(mtaxs.CGSTCode, n);
                                //}
                                //mTaxAmt.SetValue(Convert.ToDecimal(mTaxAmt.GetValue(n)) + mobj.CGSTAmt, n);

                                mTaxPostCode2.Add(new PurchaseVM()
                                {
                                    TaxCode = mtaxs.CGSTCode,
                                    TaxAmt = mobj.CGSTAmt
                                });
                            }
                        }
                    }
                }

                if (Model.VATGSTApp == "G")
                {
                    // refund due account for sez/exports
                    if ((Model.GSTType == "7" && mSEZChargeParty == false) || Model.GSTType == "9")
                    {

                        mTaxPostCode2.Add(new PurchaseVM()
                        {
                            TaxCode = "000100087",
                            TaxAmt = mMultTax * -1
                        });
                    }
                }
                //Session.Add("NewItemlist", mlist);
            }

            // ----- actual posting routine starts from here
            xCnt = 1;
            if (Model.VATGSTApp == "G")
            {
                mvalue = mTaxable - mCVDAmt - mCVDCess - mCVDExtra - mCVDSCess;
            }
            if (Model.VATGSTApp == "V")//10/sep/2019 darshan
            {
                mvalue = Model.Amt - mCVDAmt - mCVDCess - mCVDExtra - mCVDSCess;
            }

            List<PurchaseVM> LedPostList = new List<PurchaseVM>();

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

            if ((Model.GSTType == "7" && mSEZChargeParty == false) || Model.GSTType == "9")
            {
                decimal mroundcrdt = 0;
                if (Model.IsRoundOff == true && Model.RoundOff != 0)
                {
                    //if (Model.SubType != "NS" && Model.SubType != "NP")
                    //{
                    //    mroundcrdt = Model.RoundOff;// this time add 

                    //}
                    //else
                    //{
                    mroundcrdt = Model.RoundOff;// this time -ve
                    //}
                }

                mPostAmt = mvalue + mroundcrdt;   // inventory value


                decimal mTDSsez = Model.TDSAmt + Model.TDSSchg + Model.TDSCess + Model.TDSSHECess;
                if (Model.CutTDS == true && mTDSsez != 0)
                {
                    mPostAmt = mvalue + mroundcrdt + mTDSsez;
                }
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
                LedPostList.Add(new PurchaseVM()
                {
                    Code = Model.Account,
                    AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                    Debit = Math.Round(mDebit, 2),
                    Credit = Math.Round(mCredit, 2),
                    Branch = mbranchcode,
                    tempId = xCnt
                });

            }
            else
            {
                LedPostList.Add(new PurchaseVM()
                {
                    Code = Model.Account,
                    AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                    Debit = Math.Round(mCredit, 2),
                    Credit = Math.Round(mDebit, 2),
                    Branch = mbranchcode,
                    tempId = xCnt
                });
            }
            xCnt++;
            // --------------- tds Posting
            //bool bTDS = ctxTFAT.TaxDetails.Where(x => x.Code != Model.Account).Select(x => x.CutTDS).FirstOrDefault();
            bool bTDS = Model.CutTDS;
            decimal mTDS = Model.TDSAmt + Model.TDSSchg + Model.TDSCess + Model.TDSSHECess;
            if (bTDS == true && mTDS != 0)
            {
                mPostAmt = mTDS;
                if (Model.SubType == "NP" || Model.SubType == "PX")
                    mPostAmt = mPostAmt * -1;

                if (mPostAmt > 0)
                {
                    mDebit = mPostAmt;
                    mCredit = 0;
                }
                else
                {
                    mDebit = 0;
                    mCredit = mPostAmt * -1;
                }

                var mTdsPostCode = ctxTFAT.TDSMaster.Where(x => x.Code == Model.TDSCode).Select(x => x.PostCode).FirstOrDefault();
                if (mTdsPostCode == null || mTdsPostCode.Trim() == "")
                {
                    mTdsPostCode = "000009994";
                }
                if (Model.MainType == "PR")
                {
                    LedPostList.Add(new PurchaseVM()
                    {
                        Code = Model.Account,
                        AccountName = NameofAccount(Model.Account),
                        Debit = Math.Round(mDebit, 2),
                        Credit = Math.Round(mCredit, 2),
                        Branch = mbranchcode,
                        tempId = xCnt++
                    });
                }


                if (Model.SubType != "NP" && Model.SubType != "PX" && Model.SubType != "SX" && Model.SubType != "NS")
                {
                    mTDS = mTDS * -1;
                }

                if (mTDS < 0)
                {
                    mDebit = 0;
                    mCredit = mTDS * -1;
                }
                else
                {
                    mDebit = mTDS;
                    mCredit = 0;
                }
                LedPostList.Add(new PurchaseVM()
                {
                    Code = mTdsPostCode,
                    AccountName = NameofAccount(mTdsPostCode),
                    Debit = Math.Round(mDebit, 2),
                    Credit = Math.Round(mCredit, 2),
                    Branch = mbranchcode,
                    TDSFlag = true,
                    tempId = xCnt++
                });

            }
            //------ tds posting ends
            // posting CVD/Cess
            if (Model.MainType == "PR" && mTradef == true && mCVDAmt != 0)
            {
                if (mCVDAmt != 0)
                    LedPostList.Add(new PurchaseVM()
                    {
                        Code = "000100024",
                        AccountName = NameofAccount("000100024"),
                        Debit = 0,
                        Credit = Math.Round(mCVDAmt, 2),
                        Branch = mbranchcode,
                        tempId = xCnt++
                    });

                if (mCVDCess + mCVDSCess != 0)
                    LedPostList.Add(new PurchaseVM()
                    {
                        Code = "000100088",
                        AccountName = NameofAccount("000100088"),
                        Debit = 0,
                        Credit = Math.Round((mCVDCess + mCVDSCess), 2),
                        Branch = mbranchcode,
                        tempId = xCnt++
                    });

                if (mCVDExtra != 0)
                    LedPostList.Add(new PurchaseVM()
                    {
                        Code = "000100091",
                        AccountName = NameofAccount("000100091"),
                        Debit = 0,
                        Credit = Math.Round(mCVDExtra, 2),
                        Branch = mbranchcode,
                        tempId = xCnt++
                    });
            }

            // --------------- Product wise Account Posting
            if (mProductPost == true)
            {
                var mProdPostCode = mProdPostCode2.GroupBy(x => x.PostAccount).ToList();
                //for (int mCnt = 0; mCnt < 300; mCnt++)
                //{
                foreach (var apostcode in mProdPostCode)
                {
                    if (apostcode != null)
                    {
                        // decimal mAmt = (decimal)mProdPostAmt.GetValue(mCnt);
                        // mCode = mProdPostCode.GetValue(mCnt).ToString();

                        mCode = apostcode.Key;
                        decimal mAmt = mProdPostCode2.Where(x => x.PostAccount == apostcode.Key).Sum(x => (decimal?)x.AccAmt) ?? (decimal)0;
                        if (mCode == "")
                            break;

                        if (mAmt != 0)
                            mAmt = mAmt * (Model.SubType != "NS" && Model.SubType != "NP" ? -1 : 1);

                        if (mAmt < 0)
                        {
                            mDebit = 0;
                            mCredit = mAmt * -1;
                        }
                        else
                        {
                            mDebit = mAmt;
                            mCredit = 0;
                        }
                        if (Model.MainType == "SL")
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mCode,
                                AccountName = NameofAccount(mCode),
                                Debit = Math.Round(mDebit, 2),
                                Credit = Math.Round(mCredit, 2),
                                Branch = mbranchcode,
                                tempId = xCnt++
                            });
                        }
                        else
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mCode,
                                AccountName = NameofAccount(mCode),
                                Debit = Math.Round(mCredit, 2),
                                Credit = Math.Round(mDebit, 2),
                                Branch = mbranchcode,
                                tempId = xCnt++
                            });
                        }
                    }
                }

                //}
            }

            // Charges Updations
            n = 0;
            var mchg = Model.Charges.Where(x => x.AddLess == "+" || x.AddLess == "-").Select(x => x).ToList();
            foreach (PurchaseVM mc in mchg)
            {
                // if (product wise posting ) dont post net amount
                if (mc.ChgPostCode != "" && mc.Val1 != 0 && (mProductPost == false || (mProductPost == true && n != mNetSerl)))
                {
                    mPostAmt = mc.Val1;
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
                        LedPostList.Add(new PurchaseVM()
                        {
                            Code = mc.ChgPostCode,
                            AccountName = NameofAccount(mc.ChgPostCode),
                            Debit = Math.Round(mDebit, 2),
                            Credit = Math.Round(mCredit, 2),
                            Branch = mbranchcode,
                            tempId = xCnt++
                        });
                    }
                    else
                    {
                        if (mTradef == true && mTrx == "P" && n != mNetSerl && mProductPost == true)
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mc.ChgPostCode,
                                AccountName = NameofAccount(mc.ChgPostCode),
                                Debit = Math.Round(mDebit, 2),
                                Credit = Math.Round(mCredit, 2),
                                Branch = mbranchcode,
                                tempId = xCnt++
                            });
                        }
                        LedPostList.Add(new PurchaseVM()
                        {
                            Code = mc.ChgPostCode,
                            AccountName = NameofAccount(mc.ChgPostCode),
                            Debit = Math.Round(mCredit, 2),
                            Credit = Math.Round(mDebit, 2),
                            Branch = mbranchcode,
                            tempId = xCnt++
                        });
                    }
                }
                n++;
            }

            var mTaxPostCode = mTaxPostCode2.GroupBy(x => x.TaxCode).ToList();
            string ourstatename = ctxTFAT.Warehouse.Where(x => x.Code == Model.LocationCode).Select(x => x.State).FirstOrDefault();

            if (ourstatename == Model.PlaceOfSupply)
            {
                chggstamt = chggstamt / 2;
            }
            // --------------- tax code (GST) posting 
            //for (int mCnt3 = 0; mCnt3 < 50; mCnt3++)
            //{
            foreach (var a in mTaxPostCode)
            {
                if (a.Key != null)
                {
                    //mCode = mTaxPostCode.GetValue(mCnt3).ToString();
                    //mPostAmt = (decimal)mTaxAmt.GetValue(mCnt3);
                    mCode = a.Key;
                    mPostAmt = mTaxPostCode2.Where(x => x.TaxCode == a.Key).Sum(x => (decimal?)x.TaxAmt) + chggstamt ?? (decimal)0;

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
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mCode,
                                AccountName = NameofAccount(mCode),
                                Debit = Math.Round(mDebit, 2),
                                Credit = Math.Round(mCredit, 2),
                                Branch = mbranchcode,
                                tempId = xCnt++
                            });
                        }
                        else
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mCode,
                                AccountName = NameofAccount(mCode),
                                Debit = Math.Round(mCredit, 2),
                                Credit = Math.Round(mDebit, 2),
                                Branch = mbranchcode,
                                tempId = xCnt++
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
                    LedPostList.Add(new PurchaseVM() { Code = mCode, AccountName = NameofAccount(mCode), Debit = Math.Round(mDebit, 2), Credit = Math.Round(mCredit, 2), Branch = mbranchcode, tempId = xCnt++ });
                }
                else
                {
                    LedPostList.Add(new PurchaseVM() { Code = mCode, AccountName = NameofAccount(mCode), Debit = Math.Round(mCredit, 2), Credit = Math.Round(mDebit, 2), Branch = mbranchcode, tempId = xCnt++ });
                }
            }

            if (Model.Broker != 0 && Model.BrokerAmt > 0)
            {
                var mBro = ctxTFAT.Broker.Select(x => new
                {
                    x.PostAc,
                    x.DrCode,
                    x.CrCode,
                    x.Code
                }).Where(z => z.Code == Model.Broker).FirstOrDefault();
                if (mBro != null)
                {
                    if (/*mBro.PostAc != false &&*/ mBro.DrCode != "" && mBro.CrCode != "")
                    {
                        //mCode2 = TaxRs!DrCode
                        if (Model.SubType == "NS" || Model.SubType == "NP")
                        {
                            mDebit = 0;
                            mCredit = Model.BrokerAmt;
                        }
                        else
                        {
                            mDebit = Model.BrokerAmt;
                            mCredit = 0;
                        }
                        if (Model.MainType == "SL")
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mBro.DrCode,
                                AccountName = NameofAccount(mBro.DrCode),
                                Debit = Math.Round(mDebit, 2),
                                Credit = Math.Round(mCredit, 2),
                                Branch = mbranchcode,
                                tempId = xCnt++
                            });
                        }
                        else
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mBro.DrCode,
                                AccountName = NameofAccount(mBro.DrCode),
                                Debit = Math.Round(mCredit, 2),
                                Credit = Math.Round(mDebit, 2),
                                Branch = mbranchcode,
                                tempId = xCnt++
                            });
                        }
                        //mCode2 = TaxRs!CrCode
                        if (Model.SubType == "NS" || Model.SubType == "NP")
                        {
                            mDebit = Model.BrokerAmt;
                            mCredit = 0;
                        }
                        else
                        {
                            mDebit = 0;
                            mCredit = Model.BrokerAmt;
                        }

                        if (Model.MainType == "SL")
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mBro.CrCode,
                                AccountName = NameofAccount(mBro.CrCode),
                                Debit = Math.Round(mDebit, 2),
                                Credit = Math.Round(mCredit, 2),
                                Branch = mbranchcode,
                                tempId = xCnt++
                            });
                        }
                        else
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mBro.CrCode,
                                AccountName = NameofAccount(mBro.CrCode),
                                Debit = Math.Round(mCredit, 2),
                                Credit = Math.Round(mDebit, 2),
                                Branch = mbranchcode,
                                tempId = xCnt++
                            });
                        }
                    }
                }
            }

            Model.TotDebit = LedPostList.Sum(x => x.Debit);
            Model.TotCredit = LedPostList.Sum(x => x.Credit);
            Model.LedgerPostList = LedPostList;

            // display view form
            var html = ViewHelper.RenderPartialView(this, "LedgerPosting", new PurchaseVM() { LedgerPostList = Model.LedgerPostList, TotDebit = Math.Round(Model.TotDebit, 2), TotCredit = Math.Round(Model.TotCredit, 2), Mode = Model.Mode, Controller2 = Model.Controller2, Type = Model.Type, Module = Model.Module, ViewDataId = Model.ViewDataId, TableName = Model.TableName, Header = Model.Header, optiontype = Model.optiontype, OptionCode = Model.OptionCode, MainType = Model.MainType, SubType = Model.SubType });
            return Json(new
            {
                LedgerPostList = Model.LedgerPostList,
                TotDebit = Math.Round(Model.TotDebit, 2),
                TotCredit = Math.Round(Model.TotCredit, 2),
                Mode = Model.Mode,
                Html = html
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPostingforUpdate(PurchaseVM Model)
        {
            try
            {
                var result = Model.LedgerPostList;
                var result1 = result.Where(x => x.tempId == Model.tempId);
                foreach (var item in result1)
                {
                    Model.tempId = item.tempId;
                    Model.PostAccount = item.Code;
                    Model.PostAccountName = NameofAccount(item.Code);

                }
                var jsonResult = Json(new { Html = this.RenderPartialView("LedgerPostEditPopUp", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
            catch (Exception ex)
            {
                var jsonResult = Json(new { Status = "Error" }, JsonRequestBehavior.AllowGet);
                return jsonResult;
            }

        }

        public ActionResult UpdatePostingList(PurchaseVM Model)
        {

            var result = Model.LedgerPostList;
            foreach (var item in result.Where(x => x.tempId == Model.tempId))
            {

                item.tempId = Model.tempId;
                item.Code = Model.PostAccount;
                item.AccountName = NameofAccount(Model.PostAccount);
            }


            Model.LedgerPostList = result;
            Model.TotDebit = Model.LedgerPostList.Sum(x => x.Debit);
            Model.TotCredit = Model.LedgerPostList.Sum(x => x.Credit);
            // display view form
            var html = ViewHelper.RenderPartialView(this, "LedgerPosting", new PurchaseVM() { LedgerPostList = Model.LedgerPostList, TotDebit = Model.TotDebit, TotCredit = Model.TotCredit, Mode = Model.Mode, Controller2 = Model.Controller2, Type = Model.Type, Module = Model.Module, ViewDataId = Model.ViewDataId, TableName = Model.TableName, Header = Model.Header, optiontype = Model.optiontype, OptionCode = Model.OptionCode, MainType = Model.MainType, SubType = Model.SubType });
            return Json(new
            {
                LedgerPostList = Model.LedgerPostList,
                TotDebit = Model.TotDebit,
                TotCredit = Model.TotCredit,
                Mode = Model.Mode,
                Html = html
            }, JsonRequestBehavior.AllowGet);
        }

        public List<PurchaseVM> GetInterBranchPosting(PurchaseVM Model)
        {
            string mpostcode;
            int n;
            string mIGSTCode = "";
            string mCGSTCode = "";
            string mSGSTCode = "";
            decimal mMultTax = 0;
            decimal mCVDAmt = 0;
            decimal mCVDCess = 0;
            decimal mCVDExtra = 0;
            decimal mCVDSCess = 0;
            decimal mvalue = 0;
            decimal mTaxable = 0;
            decimal mDebit = 0;
            decimal mCredit = 0;
            decimal mPostAmt = Model.Amt;
            decimal mWeightage = 0;
            decimal mtotalcost = 0;
            decimal mcostperitem = 0;


            string mCode = "";
            int xCnt = 0;
            string mTrx = "";
            if (Model.MainType == "SL" && "ES~OS~QS~PI~SG".Contains(Model.SubType) == false)
            {
                mTrx = "S";
            }
            else if (Model.MainType == "PR" && "EP~OP~QP~PG".Contains(Model.SubType) == false)
            {
                mTrx = "P";
            }

            string TaxAmtStr = "";
            foreach (var a in Model.Charges)
            {
                TaxAmtStr = TaxAmtStr + a.AddLess + a.TaxAmt;
            }
            decimal chggstamt = GetAmtValue(TaxAmtStr);
            var resultdoctype = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => x).FirstOrDefault();
            bool mProductPost = resultdoctype.ProductPost;
            bool mTradef = resultdoctype.CurrConv == "Y" ? true : false;
            List<PurchaseVM> mTaxPostCode2 = new List<PurchaseVM>();
            //Array mTaxAmt = Array.CreateInstance(typeof(decimal), 50);
            //bool mRCM = CheckRCM(Convert.ToInt32(Model.GSTType), Model.DocDate, mRCMDate);
            bool mRCM = false;
            if (Model.GSTType == "1")
            {
                mRCM = true;
            }

            List<PurchaseVM> mProdPostCode2 = new List<PurchaseVM>();

            // create item wise posting array
            var mresult = (List<PurchaseVM>)Session["NewItemlist"];
            if (mresult != null)
            {
                var mlist = mresult.Where(x => x.tempIsDeleted != true).ToList();
                //var mChargeTotal = PostCharges.Where(x => x.EqCost == "+" || x.EqCost == "-").Sum(x => (decimal?)x.Val1) ?? 0;
                //var mItemWithChargeTotal = mChargeTotal;
                var mItemValTotal = mlist.Where(x => x.ItemType != "S").Sum(x => (decimal?)x.Taxable) ?? 1;
                if (mItemValTotal == 0) mItemValTotal = 1;
                foreach (var mobj in mlist)
                {
                    mMultTax = mMultTax + mobj.SGSTAmt + mobj.CGSTAmt + mobj.IGSTAmt;// + mobj.CessAmt
                    mCVDAmt = mCVDAmt + mobj.CVDAmt;
                    mCVDCess = mCVDCess + mobj.CVDCessAmt;
                    mCVDExtra = mCVDExtra + mobj.CVDExtra;
                    mCVDSCess = mCVDSCess + mobj.CVDSCessAmt;
                    mTaxable = mTaxable + mobj.Taxable;

                    if (mProductPost == true)
                    {
                        //--------------- Product posting details
                        if (Model.MainType == "PR")
                        {
                            if (mTradef == false)
                                mpostcode = ctxTFAT.ItemDetail.Where(x => x.Code == mobj.Code && x.Branch == Model.Branch).Select(x => x.AcCode).FirstOrDefault();
                            else
                                mpostcode = ctxTFAT.ItemDetail.Where(x => x.Code == mobj.Code && x.Branch == Model.Branch).Select(x => x.AcCodeX).FirstOrDefault();
                        }
                        else
                        {
                            if (mTradef == false)
                                mpostcode = ctxTFAT.ItemDetail.Where(x => x.Code == mobj.Code && x.Branch == Model.Branch).Select(x => x.AcCodeS).FirstOrDefault();
                            else
                                mpostcode = ctxTFAT.ItemDetail.Where(x => x.Code == mobj.Code && x.Branch == Model.Branch).Select(x => x.AcCodeSX).FirstOrDefault();
                        }

                        if (mpostcode == "")
                            mpostcode = mTradef == true ? (Model.MainType == "PR" ? "000100017" : "000100025") : (Model.MainType == "PR" ? "000100004" : "000100001");


                        mvalue = mobj.Val1 * (mobj.Qty < 0 ? -1 : 1) /** Model.CurrRate*/;
                        //mvalue = Convert.ToDecimal(mProdPostAmt.GetValue(n)) + mvalue;    // add to existin array value
                        if (mRCM == false /*&& Model.CurrRate == 1*/)
                            mvalue = mvalue - (mobj.SGSTAmt + mobj.CGSTAmt + mobj.IGSTAmt) * (mobj.Qty < 0 ? -1 : 1);

                        // mProdPostAmt.SetValue(mvalue, n);

                        mProdPostCode2.Add(new PurchaseVM()
                        {
                            PostAccount = mpostcode,
                            AccAmt = mvalue
                        });
                    }
                    //=============== product post array ends here

                    if (Model.VATGSTApp == "G")
                    {
                        var mproductGstCode = GetGSTItemCode(mobj.Code, Model.Branch);
                        //--------------- GST posting starts
                        if ((mTrx == "P" && mRCM == false && mTradef == false) || mTrx == "S")
                        {
                            var mtaxs = ctxTFAT.TaxMaster.Select(x => new
                            {
                                x.Code,
                                x.SGSTCode,
                                x.CGSTCode,
                                x.IGSTCode,
                                x.CessCode,
                                x.Scope
                            }).Where(z => z.Scope == Model.MainType.Substring(0, 1) && z.Code == mproductGstCode).FirstOrDefault();
                            if (mtaxs != null)
                            {
                                // igst
                                if (mobj.IGSTAmt != 0)
                                {

                                    mTaxPostCode2.Add(new PurchaseVM()
                                    {
                                        TaxCode = mtaxs.IGSTCode,
                                        TaxAmt = mobj.IGSTAmt
                                    });
                                }
                                // cgst
                                if (mobj.CGSTAmt != 0)
                                {

                                    mTaxPostCode2.Add(new PurchaseVM()
                                    {
                                        TaxCode = mtaxs.CGSTCode,
                                        TaxAmt = mobj.CGSTAmt
                                    });
                                }
                                // sgst
                                if (mobj.SGSTAmt != 0)
                                {


                                    mTaxPostCode2.Add(new PurchaseVM()
                                    {
                                        TaxCode = mtaxs.SGSTCode,
                                        TaxAmt = mobj.SGSTAmt
                                    });
                                }
                                // cess
                                if (mobj.CessAmt != 0)
                                {

                                    mTaxPostCode2.Add(new PurchaseVM()
                                    {
                                        TaxCode = mtaxs.CessCode,
                                        TaxAmt = mobj.CessAmt
                                    });
                                }
                            }
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
                            if (mobj.IGSTAmt != 0)
                            {

                                mTaxPostCode2.Add(new PurchaseVM()
                                {
                                    TaxCode = mIGSTCode,
                                    TaxAmt = mobj.IGSTAmt * (mTrx == "P" ? -1 : 1)
                                });
                            }

                            // cgst
                            if (mobj.CGSTAmt != 0)
                            {

                                mTaxPostCode2.Add(new PurchaseVM()
                                {
                                    TaxCode = mCGSTCode,
                                    TaxAmt = mobj.CGSTAmt * (mTrx == "P" ? -1 : 1)
                                });
                            }
                            // sgst
                            if (mobj.SGSTAmt != 0)
                            {

                                mTaxPostCode2.Add(new PurchaseVM()
                                {
                                    TaxCode = mSGSTCode,
                                    TaxAmt = mobj.SGSTAmt * (mTrx == "P" ? -1 : 1)
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
                            if (mobj.IGSTAmt != 0)
                            {

                                mTaxPostCode2.Add(new PurchaseVM()
                                {
                                    TaxCode = mIGSTCode,
                                    TaxAmt = mobj.IGSTAmt
                                });
                            }
                            // cgst
                            if (mobj.CGSTAmt != 0)
                            {

                                mTaxPostCode2.Add(new PurchaseVM()
                                {
                                    TaxCode = mCGSTCode,
                                    TaxAmt = mobj.CGSTAmt
                                });
                            }
                            // sgst
                            if (mobj.SGSTAmt != 0)
                            {

                                mTaxPostCode2.Add(new PurchaseVM()
                                {
                                    TaxCode = mSGSTCode,
                                    TaxAmt = mobj.SGSTAmt
                                });
                            }
                        }
                    }
                    else // vat posting
                    {
                        // igstcode=taxamt, sgstcode=surcharge, cgstcode=addtax
                        var mtaxs = ctxTFAT.TaxMaster.Select(x => new { x.Code, x.SGSTCode, x.CGSTCode, x.IGSTCode, x.Scope }).Where(z => z.Scope == Model.MainType.Substring(0, 1) && z.Code == mobj.GSTCode).FirstOrDefault();
                        if (mtaxs != null)
                        {
                            // igst = taxamt
                            if (mobj.IGSTAmt != 0)
                            {
                                //n = Array.BinarySearch(mTaxPostCode, mtaxs.IGSTCode);
                                //if (n < 0)
                                //{
                                //    n = Array.BinarySearch(mTaxPostCode, null);      //find next blank array
                                //    mTaxPostCode.SetValue(mtaxs.IGSTCode, n);
                                //}
                                //mTaxAmt.SetValue(Convert.ToDecimal(mTaxAmt.GetValue(n)) + mobj.IGSTAmt, n);


                                mTaxPostCode2.Add(new PurchaseVM()
                                {
                                    TaxCode = mtaxs.IGSTCode,
                                    TaxAmt = mobj.IGSTAmt
                                });
                            }
                            // sgst = surcharge
                            if (mobj.SGSTAmt != 0)
                            {
                                //n = Array.BinarySearch(mTaxPostCode, mtaxs.SGSTCode);
                                //if (n < 0)
                                //{
                                //    n = Array.BinarySearch(mTaxPostCode, null);      //find next blank array
                                //    mTaxPostCode.SetValue(mtaxs.SGSTCode, n);
                                //}
                                //mTaxAmt.SetValue(Convert.ToDecimal(mTaxAmt.GetValue(n)) + mobj.SGSTAmt, n);

                                mTaxPostCode2.Add(new PurchaseVM()
                                {
                                    TaxCode = mtaxs.SGSTCode,
                                    TaxAmt = mobj.SGSTAmt
                                });
                            }
                            // cgst = addtax
                            if (mobj.CGSTAmt != 0)
                            {
                                //n = Array.BinarySearch(mTaxPostCode, mtaxs.CGSTCode);
                                //if (n < 0)
                                //{
                                //    n = Array.BinarySearch(mTaxPostCode, null);      //find next blank array
                                //    mTaxPostCode.SetValue(mtaxs.CGSTCode, n);
                                //}
                                //mTaxAmt.SetValue(Convert.ToDecimal(mTaxAmt.GetValue(n)) + mobj.CGSTAmt, n);

                                mTaxPostCode2.Add(new PurchaseVM()
                                {
                                    TaxCode = mtaxs.CGSTCode,
                                    TaxAmt = mobj.CGSTAmt
                                });
                            }
                        }
                    }
                }

                if (Model.VATGSTApp == "G")
                {
                    // refund due account for sez/exports
                    if ((Model.GSTType == "7" && mSEZChargeParty == false) || Model.GSTType == "9")
                    {

                        mTaxPostCode2.Add(new PurchaseVM()
                        {
                            TaxCode = "000100087",
                            TaxAmt = mMultTax * -1
                        });
                    }
                }
                //Session.Add("NewItemlist", mlist);
            }

            // ----- actual posting routine starts from here
            xCnt = 1;
            if (Model.VATGSTApp == "G")
            {
                mvalue = mTaxable - mCVDAmt - mCVDCess - mCVDExtra - mCVDSCess;
            }
            if (Model.VATGSTApp == "V")//10/sep/2019 darshan
            {
                mvalue = Model.Amt - mCVDAmt - mCVDCess - mCVDExtra - mCVDSCess;
            }

            List<PurchaseVM> LedPostList = new List<PurchaseVM>();

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

            if ((Model.GSTType == "7" && mSEZChargeParty == false) || Model.GSTType == "9")
            {
                decimal mroundcrdt = 0;
                if (Model.IsRoundOff == true && Model.RoundOff != 0)
                {
                    //if (Model.SubType != "NS" && Model.SubType != "NP")
                    //{
                    //    mroundcrdt = Model.RoundOff;// this time add 

                    //}
                    //else
                    //{
                    mroundcrdt = Model.RoundOff;// this time -ve
                    //}
                }

                mPostAmt = mvalue + mroundcrdt;   // inventory value


                decimal mTDSsez = Model.TDSAmt + Model.TDSSchg + Model.TDSCess + Model.TDSSHECess;
                if (Model.CutTDS == true && mTDSsez != 0)
                {
                    mPostAmt = mvalue + mroundcrdt + mTDSsez;
                }
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
                LedPostList.Add(new PurchaseVM()
                {
                    Code = Model.Account,
                    AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                    Debit = Math.Round(mDebit, 2),
                    Credit = Math.Round(mCredit, 2),
                    Branch = Model.Branch,
                    tempId = xCnt
                });

            }
            else
            {
                LedPostList.Add(new PurchaseVM()
                {
                    Code = Model.Account,
                    AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                    Debit = Math.Round(mCredit, 2),
                    Credit = Math.Round(mDebit, 2),
                    Branch = Model.Branch,
                    tempId = xCnt
                });
            }
            xCnt++;
            // --------------- tds Posting
            //bool bTDS = ctxTFAT.TaxDetails.Where(x => x.Code != Model.Account).Select(x => x.CutTDS).FirstOrDefault();
            bool bTDS = Model.CutTDS;
            decimal mTDS = Model.TDSAmt + Model.TDSSchg + Model.TDSCess + Model.TDSSHECess;
            if (bTDS == true && mTDS != 0)
            {
                mPostAmt = mTDS;
                if (Model.SubType == "NP" || Model.SubType == "PX")
                    mPostAmt = mPostAmt * -1;

                if (mPostAmt > 0)
                {
                    mDebit = mPostAmt;
                    mCredit = 0;
                }
                else
                {
                    mDebit = 0;
                    mCredit = mPostAmt * -1;
                }

                var mTdsPostCode = ctxTFAT.TDSMaster.Where(x => x.Code == Model.TDSCode).Select(x => x.PostCode).FirstOrDefault();
                if (mTdsPostCode == null || mTdsPostCode.Trim() == "")
                {
                    mTdsPostCode = "000009994";
                }
                if (Model.MainType == "PR")
                {
                    LedPostList.Add(new PurchaseVM()
                    {
                        Code = Model.Account,
                        AccountName = NameofAccount(Model.Account),
                        Debit = Math.Round(mDebit, 2),
                        Credit = Math.Round(mCredit, 2),
                        Branch = Model.Branch,
                        tempId = xCnt++
                    });
                }


                if (Model.SubType != "NP" && Model.SubType != "PX" && Model.SubType != "SX" && Model.SubType != "NS")
                {
                    mTDS = mTDS * -1;
                }

                if (mTDS < 0)
                {
                    mDebit = 0;
                    mCredit = mTDS * -1;
                }
                else
                {
                    mDebit = mTDS;
                    mCredit = 0;
                }
                LedPostList.Add(new PurchaseVM()
                {
                    Code = mTdsPostCode,
                    AccountName = NameofAccount(mTdsPostCode),
                    Debit = Math.Round(mDebit, 2),
                    Credit = Math.Round(mCredit, 2),
                    Branch = Model.Branch,
                    TDSFlag = true,
                    tempId = xCnt++
                });

            }
            //------ tds posting ends
            // posting CVD/Cess
            if (Model.MainType == "PR" && mTradef == true && mCVDAmt != 0)
            {
                if (mCVDAmt != 0)
                    LedPostList.Add(new PurchaseVM()
                    {
                        Code = "000100024",
                        AccountName = NameofAccount("000100024"),
                        Debit = 0,
                        Credit = Math.Round(mCVDAmt, 2),
                        Branch = Model.Branch,
                        tempId = xCnt++
                    });

                if (mCVDCess + mCVDSCess != 0)
                    LedPostList.Add(new PurchaseVM()
                    {
                        Code = "000100088",
                        AccountName = NameofAccount("000100088"),
                        Debit = 0,
                        Credit = Math.Round((mCVDCess + mCVDSCess), 2),
                        Branch = Model.Branch,
                        tempId = xCnt++
                    });

                if (mCVDExtra != 0)
                    LedPostList.Add(new PurchaseVM()
                    {
                        Code = "000100091",
                        AccountName = NameofAccount("000100091"),
                        Debit = 0,
                        Credit = Math.Round(mCVDExtra, 2),
                        Branch = Model.Branch,
                        tempId = xCnt++
                    });
            }

            // --------------- Product wise Account Posting
            if (mProductPost == true)
            {
                var mProdPostCode = mProdPostCode2.GroupBy(x => x.PostAccount).ToList();
                //for (int mCnt = 0; mCnt < 300; mCnt++)
                //{
                foreach (var apostcode in mProdPostCode)
                {
                    if (apostcode != null)
                    {
                        // decimal mAmt = (decimal)mProdPostAmt.GetValue(mCnt);
                        // mCode = mProdPostCode.GetValue(mCnt).ToString();

                        mCode = apostcode.Key;
                        decimal mAmt = mProdPostCode2.Where(x => x.PostAccount == apostcode.Key).Sum(x => (decimal?)x.AccAmt) ?? (decimal)0;
                        if (mCode == "")
                            break;

                        if (mAmt != 0)
                            mAmt = mAmt * (Model.SubType != "NS" && Model.SubType != "NP" ? -1 : 1);

                        if (mAmt < 0)
                        {
                            mDebit = 0;
                            mCredit = mAmt * -1;
                        }
                        else
                        {
                            mDebit = mAmt;
                            mCredit = 0;
                        }
                        if (Model.MainType == "SL")
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mCode,
                                AccountName = NameofAccount(mCode),
                                Debit = Math.Round(mDebit, 2),
                                Credit = Math.Round(mCredit, 2),
                                Branch = Model.Branch,
                                tempId = xCnt++
                            });
                        }
                        else
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mCode,
                                AccountName = NameofAccount(mCode),
                                Debit = Math.Round(mCredit, 2),
                                Credit = Math.Round(mDebit, 2),
                                Branch = Model.Branch,
                                tempId = xCnt++
                            });
                        }
                    }
                }

                //}
            }

            // Charges Updations
            n = 0;
            var mchg = Model.Charges.Where(x => x.AddLess == "+" || x.AddLess == "-").Select(x => x).ToList();
            foreach (PurchaseVM mc in mchg)
            {
                // if (product wise posting ) dont post net amount
                if (mc.ChgPostCode != "" && mc.Val1 != 0 && (mProductPost == false || (mProductPost == true && n != mNetSerl)))
                {
                    mPostAmt = mc.Val1;
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
                        LedPostList.Add(new PurchaseVM()
                        {
                            Code = mc.ChgPostCode,
                            AccountName = NameofAccount(mc.ChgPostCode),
                            Debit = Math.Round(mDebit, 2),
                            Credit = Math.Round(mCredit, 2),
                            Branch = Model.Branch,
                            tempId = xCnt++
                        });
                    }
                    else
                    {
                        if (mTradef == true && mTrx == "P" && n != mNetSerl && mProductPost == true)
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mc.ChgPostCode,
                                AccountName = NameofAccount(mc.ChgPostCode),
                                Debit = Math.Round(mDebit, 2),
                                Credit = Math.Round(mCredit, 2),
                                Branch = Model.Branch,
                                tempId = xCnt++
                            });
                        }
                        LedPostList.Add(new PurchaseVM()
                        {
                            Code = mc.ChgPostCode,
                            AccountName = NameofAccount(mc.ChgPostCode),
                            Debit = Math.Round(mCredit, 2),
                            Credit = Math.Round(mDebit, 2),
                            Branch = Model.Branch,
                            tempId = xCnt++
                        });
                    }
                }
                n++;
            }

            var mTaxPostCode = mTaxPostCode2.GroupBy(x => x.TaxCode).ToList();
            string ourstatename = ctxTFAT.Warehouse.Where(x => x.Code == Model.LocationCode).Select(x => x.State).FirstOrDefault();

            if (ourstatename == Model.PlaceOfSupply)
            {
                chggstamt = chggstamt / 2;
            }
            // --------------- tax code (GST) posting 
            //for (int mCnt3 = 0; mCnt3 < 50; mCnt3++)
            //{
            foreach (var a in mTaxPostCode)
            {
                if (a.Key != null)
                {
                    //mCode = mTaxPostCode.GetValue(mCnt3).ToString();
                    //mPostAmt = (decimal)mTaxAmt.GetValue(mCnt3);
                    mCode = a.Key;
                    mPostAmt = mTaxPostCode2.Where(x => x.TaxCode == a.Key).Sum(x => (decimal?)x.TaxAmt) + chggstamt ?? (decimal)0;

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
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mCode,
                                AccountName = NameofAccount(mCode),
                                Debit = Math.Round(mDebit, 2),
                                Credit = Math.Round(mCredit, 2),
                                Branch = Model.Branch,
                                tempId = xCnt++
                            });
                        }
                        else
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mCode,
                                AccountName = NameofAccount(mCode),
                                Debit = Math.Round(mCredit, 2),
                                Credit = Math.Round(mDebit, 2),
                                Branch = Model.Branch,
                                tempId = xCnt++
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
                    LedPostList.Add(new PurchaseVM() { Code = mCode, AccountName = NameofAccount(mCode), Debit = Math.Round(mDebit, 2), Credit = Math.Round(mCredit, 2), Branch = Model.Branch, tempId = xCnt++ });
                }
                else
                {
                    LedPostList.Add(new PurchaseVM() { Code = mCode, AccountName = NameofAccount(mCode), Debit = Math.Round(mCredit, 2), Credit = Math.Round(mDebit, 2), Branch = Model.Branch, tempId = xCnt++ });
                }
            }

            if (Model.Broker != 0 && Model.BrokerAmt > 0)
            {
                var mBro = ctxTFAT.Broker.Select(x => new
                {
                    x.PostAc,
                    x.DrCode,
                    x.CrCode,
                    x.Code
                }).Where(z => z.Code == Model.Broker).FirstOrDefault();
                if (mBro != null)
                {
                    if (/*mBro.PostAc != false &&*/ mBro.DrCode != "" && mBro.CrCode != "")
                    {
                        //mCode2 = TaxRs!DrCode
                        if (Model.SubType == "NS" || Model.SubType == "NP")
                        {
                            mDebit = 0;
                            mCredit = Model.BrokerAmt;
                        }
                        else
                        {
                            mDebit = Model.BrokerAmt;
                            mCredit = 0;
                        }
                        if (Model.MainType == "SL")
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mBro.DrCode,
                                AccountName = NameofAccount(mBro.DrCode),
                                Debit = Math.Round(mDebit, 2),
                                Credit = Math.Round(mCredit, 2),
                                Branch = Model.Branch,
                                tempId = xCnt++
                            });
                        }
                        else
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mBro.DrCode,
                                AccountName = NameofAccount(mBro.DrCode),
                                Debit = Math.Round(mCredit, 2),
                                Credit = Math.Round(mDebit, 2),
                                Branch = Model.Branch,
                                tempId = xCnt++
                            });
                        }
                        //mCode2 = TaxRs!CrCode
                        if (Model.SubType == "NS" || Model.SubType == "NP")
                        {
                            mDebit = Model.BrokerAmt;
                            mCredit = 0;
                        }
                        else
                        {
                            mDebit = 0;
                            mCredit = Model.BrokerAmt;
                        }

                        if (Model.MainType == "SL")
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mBro.CrCode,
                                AccountName = NameofAccount(mBro.CrCode),
                                Debit = Math.Round(mDebit, 2),
                                Credit = Math.Round(mCredit, 2),
                                Branch = Model.Branch,
                                tempId = xCnt++
                            });
                        }
                        else
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mBro.CrCode,
                                AccountName = NameofAccount(mBro.CrCode),
                                Debit = Math.Round(mCredit, 2),
                                Credit = Math.Round(mDebit, 2),
                                Branch = Model.Branch,
                                tempId = xCnt++
                            });
                        }
                    }
                }
            }

            //Model.TotDebit = LedPostList.Sum(x => x.Debit);
            //Model.TotCredit = LedPostList.Sum(x => x.Credit);

            // display view form
            return LedPostList;
        }


        #endregion

        #region Delivery Schedule

        public ActionResult GetDelivaryScheduled(PurchaseVM Model)//coming from itemlist
        {
            List<PurchaseVM> delyschelist = new List<PurchaseVM>();

            var html = ViewHelper.RenderPartialView(this, "DelaySchedule", new PurchaseVM() { DelyScheList = delyschelist, Mode = Model.Mode, Code = Model.Code, ProductQty = Model.Qty, ProductQty2 = Model.Qty2, ProductRateOn2 = Model.RateOn2, SubType = Model.SubType, Store = Model.Store, Type = Model.Type });
            return Json(new
            {
                DelyScheList = delyschelist,
                Mode = Model.Mode,
                SubType = Model.SubType,
                Type = Model.Type,
                Code = Model.Code,
                Store = Model.Store,
                ProductQty = Model.Qty,
                ProductQty2 = Model.Qty2,
                ProductRateOn2 = Model.RateOn2,
                Html = html
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddSessionEditDelSche(PurchaseVM Model)
        {
            var result = (List<PurchaseVM>)Session["NewItemlist"];
            Model.Code = result.Where(x => x.tempId == Model.tempId).Select(x => x.Code).FirstOrDefault();
            Model.ProductQty = result.Where(x => x.tempId == Model.tempId).Select(x => x.Qty).FirstOrDefault();
            Model.ProductQty2 = result.Where(x => x.tempId == Model.tempId).Select(x => x.Qty2).FirstOrDefault();
            Model.ProductRateOn2 = result.Where(x => x.tempId == Model.tempId).Select(x => x.RateOn2).FirstOrDefault();
            List<PurchaseVM> delyschelist = new List<PurchaseVM>();
            List<PurchaseVM> delyschelist2 = new List<PurchaseVM>();
            delyschelist2 = (Model.DelyScheList == null) ? delyschelist2 : Model.DelyScheList;
            int Maxtempid = (delyschelist2.Count == 0) ? 0 : delyschelist2.Select(x => x.tempId).Max();
            delyschelist.AddRange(delyschelist2);

            delyschelist.Add(new PurchaseVM()
            {
                Days = Model.Days,
                StrDlyDate = Model.StrDlyDate,
                Qty1 = Model.Qty1,
                AltAddress = Model.AltAddress,
                ExecutedQty = Model.ExecutedQty,
                Pending = Model.Pending,
                SubType = Model.SubType,
                Type = Model.Type,
                Code = Model.Code,
                Store = Model.Store,
                Qty2 = Model.Qty2,
                SrNo = Maxtempid + 1,
                tempId = Maxtempid + 1,
                tempIsDeleted = false
            });


            Model.DelyScheList = delyschelist;
            foreach (var item in result.Where(x => x.tempId == Model.tempId))
            {
                item.DelyScheList = delyschelist;
            }
            Session.Add("NewItemlist", result);
            var html = ViewHelper.RenderPartialView(this, "EditDelaySchedule", new PurchaseVM() { DelyScheList = delyschelist, tempId = Model.tempId, Mode = Model.Mode, ProductQty = Model.ProductQty, ProductQty2 = Model.ProductQty2, ProductRateOn2 = Model.ProductRateOn2 });
            return Json(new
            {
                DelyScheList = Model.DelyScheList,
                tempId = Model.tempId,
                Mode = Model.Mode,
                ProductQty = Model.ProductQty,
                ProductQty2 = Model.ProductQty2,
                ProductRateOn2 = Model.ProductRateOn2,
                Html = html
            }, JsonRequestBehavior.AllowGet);

        }   //coming from add mode delayschedule

        public ActionResult AddDelivarySche(PurchaseVM Model)
        {

            List<PurchaseVM> delyschelist = new List<PurchaseVM>();
            List<PurchaseVM> delyschelist2 = new List<PurchaseVM>();
            delyschelist2 = (Model.DelyScheList == null) ? delyschelist2 : Model.DelyScheList;
            int Maxtempid = (delyschelist2.Count == 0) ? 0 : delyschelist2.Select(x => x.tempId).Max();
            delyschelist.AddRange(delyschelist2);

            delyschelist.Add(new PurchaseVM()
            {
                Days = Model.Days,
                StrDlyDate = Model.StrDlyDate,
                Qty1 = Model.Qty1,
                AltAddress = Model.AltAddress,
                ExecutedQty = Model.ExecutedQty,
                Pending = Model.Pending,
                SubType = Model.SubType,
                Type = Model.Type,
                Code = Model.Code,
                Store = Model.Store,
                Qty2 = Model.Qty2,
                SrNo = Maxtempid + 1,
                tempId = Maxtempid + 1,
                tempIsDeleted = false
            });

            var html = ViewHelper.RenderPartialView(this, "DelaySchedule", new PurchaseVM() { DelyScheList = delyschelist, Mode = Model.Mode, ProductQty = Model.ProductQty, ProductQty2 = Model.ProductQty2, ProductRateOn2 = Model.ProductRateOn2 });
            return Json(new
            {
                DelyScheList = delyschelist,
                Mode = Model.Mode,
                ProductQty = Model.ProductQty,
                ProductQty2 = Model.ProductQty2,
                ProductRateOn2 = Model.ProductRateOn2,
                Html = html
            }, JsonRequestBehavior.AllowGet);

        }//coming from add mode delayschedule

        public ActionResult GetAddSessSaveSDelSch(PurchaseVM Model)//coming from itemlist
        {
            List<PurchaseVM> delyschelist = new List<PurchaseVM>();
            List<PurchaseVM> delyschelist2 = new List<PurchaseVM>();
            if (Session["NewItemlist"] != null)
            {
                delyschelist = (List<PurchaseVM>)Session["NewItemlist"];

            }
            delyschelist2 = delyschelist.Where(x => x.tempId == Model.tempId).Select(x => x.DelyScheList).FirstOrDefault();
            Model.Code = delyschelist.Where(x => x.tempId == Model.tempId).Select(x => x.Code).FirstOrDefault();
            Model.Store = delyschelist.Where(x => x.tempId == Model.tempId).Select(x => x.Store).FirstOrDefault();
            Model.ProductQty = delyschelist.Where(x => x.tempId == Model.tempId).Select(x => x.Qty).FirstOrDefault();
            Model.ProductQty2 = delyschelist.Where(x => x.tempId == Model.tempId).Select(x => x.Qty2).FirstOrDefault();
            Model.ProductRateOn2 = delyschelist.Where(x => x.tempId == Model.tempId).Select(x => x.RateOn2).FirstOrDefault();
            var html = ViewHelper.RenderPartialView(this, "EditDelaySchedule", new PurchaseVM() { DelyScheList = delyschelist2, tempId = Model.tempId, Mode = Model.Mode, ProductQty = Model.ProductQty, ProductQty2 = Model.ProductQty2, ProductRateOn2 = Model.ProductRateOn2, SubType = Model.SubType, Code = Model.Code, Store = Model.Store });
            return Json(new
            {
                DelyScheList = delyschelist,
                Mode = Model.Mode,
                ProductQty = Model.ProductQty,
                ProductQty2 = Model.ProductQty2,
                ProductRateOn2 = Model.ProductRateOn2,
                SubType = Model.SubType,
                Code = Model.Code,
                Store = Model.Store,
                tempId = Model.tempId,
                Html = html
            }, JsonRequestBehavior.AllowGet);
        }

        public List<PurchaseVM> GetDeliveryScheduleList(string ParentKey, string TableKey, int Store)
        {
            List<PurchaseVM> delyschelist = new List<PurchaseVM>();
            var list = ctxTFAT.DlySchedule.Where(x => x.TableKey == TableKey).Select(x => x).ToList();
            foreach (var a in list)
            {
                delyschelist.Add(new PurchaseVM()
                {
                    Days = (a.Days == null) ? 0 : a.Days.Value,
                    StrDlyDate = (a.DlyDate == null) ? DateTime.Now.ToString("dd-MM-yyyy") : a.DlyDate.Value.ToString("dd-MM-yyyy"),
                    Qty1 = (a.Qty == null) ? 0 : Convert.ToDouble(a.Qty.Value),
                    AltAddress = (a.AltAddress == null) ? 0 : a.AltAddress.Value,
                    ExecutedQty = (a.ExecutedQty == null) ? 0 : Convert.ToDouble(a.ExecutedQty.Value),
                    Pending = 0,
                    SubType = a.SubType,
                    Type = a.Type,
                    Code = a.Code,
                    Store = Store,
                    Qty2 = a.Qty2,
                    SrNo = delyschelist.Count + 1,
                    tempId = delyschelist.Count + 1,
                    tempIsDeleted = false
                });
            }
            return delyschelist;
        }

        public ActionResult DeleteDeliverySchInAddMode(PurchaseVM Model)
        {

            //Model.DelyScheList.Where(x => x.tempId == Model.tempId).FirstOrDefault().tempIsDeleted = true;
            Model.DelyScheList = Model.DelyScheList.Where(x => x.tempId != Model.tempId).Select(x => x).ToList();

            string html;
            html = ViewHelper.RenderPartialView(this, "DelaySchedule", new PurchaseVM() { DelyScheList = Model.DelyScheList, Mode = Model.Mode, ProductQty = Model.ProductQty, ProductQty2 = Model.ProductQty2, ProductRateOn2 = Model.ProductRateOn2 });
            return Json(new
            {
                DelyScheList = Model.DelyScheList.Where(x => x.tempIsDeleted == false).Select(x => x).ToList(),
                Mode = Model.Mode,
                ProductQty = Model.ProductQty,
                ProductQty2 = Model.ProductQty2,
                ProductRateOn2 = Model.ProductRateOn2,
                Html = html
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteDeliverySchInEditMode(PurchaseVM Model)
        {

            //Model.DelyScheList.Where(x => x.tempId == Model.tempId).FirstOrDefault().tempIsDeleted = true;
            Model.DelyScheList = Model.DelyScheList.Where(x => x.tempId != Model.tempId).Select(x => x).ToList();
            string html;
            html = ViewHelper.RenderPartialView(this, "EditDelaySchedule", new PurchaseVM() { DelyScheList = Model.DelyScheList, Mode = Model.Mode, ProductQty = Model.ProductQty, ProductQty2 = Model.ProductQty2, ProductRateOn2 = Model.ProductRateOn2 });
            return Json(new
            {
                DelyScheList = Model.DelyScheList.Where(x => x.tempIsDeleted == false).Select(x => x).ToList(),
                Mode = Model.Mode,
                ProductQty = Model.ProductQty,
                ProductQty2 = Model.ProductQty2,
                ProductRateOn2 = Model.ProductRateOn2,
                Html = html
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveDelScheInEdit(PurchaseVM Model)
        {
            var result = (List<PurchaseVM>)Session["NewItemlist"];
            foreach (var item in result.Where(x => x.tempId == Model.tempId))
            {
                item.DelyScheList = Model.DelyScheList;
            }
            Session.Add("NewItemlist", result);


            return Json(JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region Addons

        [HttpGet]
        public ActionResult GetEditAddOnList(string Code, string ParentKey, string Type)
        {
            PurchaseVM Model = new PurchaseVM();
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
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/Purchase/AddOnGrid.cshtml", new PurchaseVM() { AddOnList = Model.AddOnList, Mode = "Edit" });
            var jsonResult = Json(new
            {
                AddOnList = Model.AddOnList,
                Mode = "Edit",
                Html = html
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }

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
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/Purchase/AddOnGrid.cshtml", new PurchaseVM() { AddOnList = Model.AddOnList, Mode = Model.Mode, Fld = Model.Fld });
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

        //public List<string> ConvertAddonString(List<AddOns> LastModel, string MainType, string Type)
        //{
        //    string j;
        //    string ab;
        //    string mamt = "";
        //    List<string> PopulateStr = new List<string>();
        //    var trnaddons = ctxTFAT.AddOns.Where(x => x.TableKey == "%Doc" + MainType && x.Hide == false && x.Types.Contains(Type)).Select(x => new { x.Eqsn, x.FldType, x.Fld, x.Sno }).OrderBy(x => new { x.Sno, x.Fld }).ToList();
        //    for (int i = 0; i < trnaddons.Count; i++)
        //    {

        //        var Eqn = trnaddons[i].Eqsn == null ? "" : trnaddons[i].Eqsn.Trim();
        //        if (Eqn.Contains("%F"))
        //        {
        //            for (int ai = 0; ai < trnaddons.Count; ai++)
        //            {
        //                j = trnaddons[ai].Fld;
        //                if (LastModel[ai].FldType == "N")
        //                {
        //                    Eqn = Eqn.Replace("%" + j, (LastModel[ai].ApplCode == "" || LastModel[ai].ApplCode == null) ? "0" : LastModel[ai].ApplCode);
        //                    if (!Eqn.Contains("%F"))
        //                    {
        //                        break;
        //                    }
        //                }
        //                else if (LastModel[ai].FldType == "T" || LastModel[ai].FldType == "M" || LastModel[ai].FldType == "C")
        //                {
        //                    Eqn = Eqn.Replace("%" + j, (LastModel[ai].ApplCode == "" || LastModel[ai].ApplCode == null) ? "''" : LastModel[ai].ApplCode);
        //                    if (!Eqn.Contains("%F"))
        //                    {
        //                        break;
        //                    }
        //                }
        //                else if (LastModel[ai].FldType == "D" || LastModel[ai].FldType == "L")
        //                {
        //                    Eqn = "";
        //                    break;

        //                }

        //            }
        //        }
        //        if (Eqn != "")
        //        {
        //            mamt = GetAmtValueAddon(Eqn);
        //        }
        //        else
        //        {
        //            mamt = LastModel[i].ApplCode;
        //        }

        //        PopulateStr.Add(mamt);
        //    }
        //    return PopulateStr;
        //}
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

        //public string GetQueryTextAddon(string sql)
        //{
        //    connstring = GetConnectionString();
        //    string bca = "";
        //    if (sql.Contains("^"))
        //    {
        //        bca = sql;
        //    }
        //    else
        //    {
        //        StringBuilder addonT = new StringBuilder();

        //        DataTable mDt2 = GetDataTable(sql, connstring);
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

        #region ProductAddons

        public ActionResult GetProductAddOnList(string Code, string Type)
        {
            PurchaseVM Model = new PurchaseVM();
            List<AddOns> objitemlist = new List<AddOns>();
            var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == Code && x.Hide == false && x.Types.Contains(Type))
                          select i).OrderBy(x => new { x.Sno, x.Fld }).ToList();
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
            Model.PAddOnList = objitemlist;
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/Purchase/PAddOnGrid.cshtml", new PurchaseVM() { PAddOnList = Model.PAddOnList, Mode = "Add" });
            var jsonResult = Json(new
            {
                PAddOnList = Model.PAddOnList,
                Mode = "Add",
                Html = html
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult GetProdEditAddOnList(PurchaseVM Model)
        {
            List<AddOns> objitemlist = new List<AddOns>();
            var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == Model.Code && x.Hide == false && x.Types.Contains(Model.Type))
                          select i).OrderBy(x => new { x.Sno, x.Fld }).ToList();
            int t = 1;
            int n = 0;
            foreach (var i in addons)
            {
                AddOns c = new AddOns();
                c.Fld = i.Fld;
                c.Head = i.Head;
                c.QueryText = i.QueryText == null ? "" : GetQueryTextAddon(i.QueryText);
                c.FldType = i.FldType;
                c.ApplCode = GetProdAddonValueNewSession(i.Fld, Model.tempId);
                c.PlaceValue = i.PlaceValue;
                c.Eqsn = i.Eqsn;
                objitemlist.Add(c);
                t = t + 1;
                n = n + 1;
            }

            Model.PEdtAddOnList = objitemlist;
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/Purchase/PEditOnGrid.cshtml", new PurchaseVM() { PEdtAddOnList = Model.PEdtAddOnList, PtempId = Model.tempId, Mode = "Add" });
            var jsonResult = Json(new
            {
                PEdtAddOnList = Model.PEdtAddOnList,
                PtempId = Model.tempId,
                Mode = Model.Mode,
                Html = html
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        [HttpPost]
        public ActionResult AddProductAddOnsVal(PurchaseVM Model)
        {
            if (Model.PAddOnList != null && Model.PAddOnList.Count > 0)
            {
                var ApplCode = Model.PAddOnList.Where(x => x.PlaceValue == "Rate").Select(x => x.ApplCode).FirstOrDefault();
                Model.Rate = Convert.ToDouble((ApplCode == "" || ApplCode == null) ? "0" : ApplCode);

                var ApplCode2 = Model.PAddOnList.Where(x => x.PlaceValue == "Qty").Select(x => x.ApplCode).FirstOrDefault();
                Model.Qty = Convert.ToDouble((ApplCode2 == "" || ApplCode2 == null) ? "0" : ApplCode2);
            }

            return Json(Model, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveProdShwAddVal(PurchaseVM Model)
        {

            var result = (List<PurchaseVM>)Session["NewItemlist"];
            var list = result.Where(x => x.tempIsDeleted != true && x.tempId == Model.tempId).FirstOrDefault();
            list.PAddOnList = Model.PAddOnList;
            if (Model.PAddOnList != null && Model.PAddOnList.Count > 0)
            {
                var ApplCode = Model.PAddOnList.Where(x => x.PlaceValue == "Rate").Select(x => x.ApplCode).FirstOrDefault();
                Model.Rate = Convert.ToDouble((ApplCode == "" || ApplCode == null) ? "0" : ApplCode);

                var ApplCode2 = Model.PAddOnList.Where(x => x.PlaceValue == "Qty").Select(x => x.ApplCode).FirstOrDefault();
                Model.Qty = Convert.ToDouble((ApplCode2 == "" || ApplCode2 == null) ? "0" : ApplCode2);
            }
            return Json(Model, JsonRequestBehavior.AllowGet);
        }

        public List<AddOns> GetPAddInEditView(string MainType, string TableKey, string Type)
        {
            string bca = "";
            List<AddOns> objitemlist = new List<AddOns>();
            var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%@D" + MainType && x.Hide == false && x.Types.Contains(Type))
                          select i).OrderBy(x => new { x.Sno, x.Fld }).ToList();
            DataTable mdt = GetProdAddonValTable(MainType, TableKey);
            int t = 1;
            int n = 0;
            foreach (var i in addons)
            {
                AddOns c = new AddOns();
                c.Fld = i.Fld;
                c.Head = i.Head;
                c.QueryText = string.IsNullOrEmpty(i.QueryText) ? "" : GetQueryTextAddon(i.QueryText);
                c.FldType = i.FldType;
                if (mdt != null && mdt.Rows.Count > 0)
                {
                    c.ApplCode = (string.IsNullOrEmpty(mdt.Rows[0][i.Fld].ToString())) ? "" : mdt.Rows[0][i.Fld].ToString();
                }
                else
                {
                    c.ApplCode = "";
                }
                c.PlaceValue = i.PlaceValue;
                c.Eqsn = i.Eqsn;
                objitemlist.Add(c);
                t = t + 1;
                n = n + 1;
            }

            return objitemlist;
        }

        public List<AddOns> GetPickPAddInEditView(string MainType, string PickMainType, string TableKey, string Type)
        {

            string bca = "";
            List<AddOns> objitemlist = new List<AddOns>();
            var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%@D" + MainType && x.Hide == false && x.Types.Contains(Type))
                          select i).OrderBy(x => new { x.Sno, x.Fld }).ToList();
            DataTable mdt = GetProdAddonValTable(PickMainType, TableKey);
            int t = 1;
            int n = 0;
            foreach (var i in addons)
            {
                AddOns c = new AddOns();
                c.Fld = i.Fld;
                c.Head = i.Head;
                c.QueryText = i.QueryText == null ? "" : GetQueryTextAddon(i.QueryText);
                c.FldType = i.FldType;
                if (mdt != null && mdt.Rows.Count > 0)
                {
                    c.ApplCode = (string.IsNullOrEmpty(mdt.Rows[0][i.Fld].ToString())) ? "" : mdt.Rows[0][i.Fld].ToString();
                }
                else
                {
                    c.ApplCode = "";
                }
                c.PlaceValue = i.PlaceValue;
                c.Eqsn = i.Eqsn;
                objitemlist.Add(c);
                t = t + 1;
                n = n + 1;
            }

            return objitemlist;
        }

        public DataTable GetProdAddonValTable(string MainType, string TableKey/*, string FLD*/)
        {
            string connstring = GetConnectionString();
            string bca = "";

            var loginQuery3 = @"Select * from addonitem" + MainType + " where TableKey = " + "'" + TableKey + "'" + "";
            DataTable mDt2 = GetDataTable(loginQuery3, connstring);
            //if (mDt2.Rows.Count > 0)
            //{
            //    bca = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? "" : mDt2.Rows[0][0].ToString();
            //}
            return mDt2;
        }

        public string GetProdAddonValueNewSession(string FLD, int tempid)
        {
            string bca = "";
            var result = (List<PurchaseVM>)Session["NewItemlist"];

            var list = result.Where(x => x.tempIsDeleted != true && x.tempId == tempid).FirstOrDefault();
            if (list != null)
            {
                if (list.PAddOnList != null)
                {
                    bca = list.PAddOnList.Where(x => x.Fld == FLD).Select(x => x.ApplCode).FirstOrDefault();

                }
            }
            return bca;
        }

        [HttpPost]
        public ActionResult CalByEquationProdAddonAddMode(PurchaseVM Model)
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

            declist = ConvertProdAddonString(declist2, Model.MainType, Model.Type, Model.Account, Model.Code);
            List<AddOns> objitemlist = new List<AddOns>();
            var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%@D" + Model.MainType && x.Hide == false && x.Types.Contains(Model.Type))
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
            Model.PAddOnList = objitemlist;
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/Purchase/PAddOnGrid.cshtml", new PurchaseVM() { PAddOnList = Model.PAddOnList, Mode = Model.Mode, Fld = Model.Fld });
            var jsonResult = Json(new
            {
                PAddOnList = Model.PAddOnList,
                Mode = Model.Mode,
                Fld = Model.Fld,
                Html = html
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [HttpPost]
        public ActionResult CalByEquationProdAddonEditMode(PurchaseVM Model)
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

            declist = ConvertProdAddonString(declist2, Model.MainType, Model.Type, Model.Account, Model.Code);
            List<AddOns> objitemlist = new List<AddOns>();
            var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%@D" + Model.MainType && x.Hide == false && x.Types.Contains(Model.Type))
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
            Model.PEdtAddOnList = objitemlist;
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/Purchase/PEditOnGrid.cshtml", new PurchaseVM()
            {
                PEdtAddOnList = Model.PEdtAddOnList,
                Mode = Model.Mode,
                Fld = Model.Fld,
                PtempId = Model.tempId,
            });
            var jsonResult = Json(new
            {
                PEdtAddOnList = Model.PEdtAddOnList,
                Mode = Model.Mode,
                Fld = Model.Fld,
                PtempId = Model.tempId,
                Html = html
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        //public List<string> ConvertProdAddonString(List<AddOns> LastModel, string MainType, string Type, string Account, string Code)
        //{
        //    string j;
        //    string ab;
        //    string mamt = "";
        //    List<string> PopulateStr = new List<string>();
        //    var trnaddons = ctxTFAT.AddOns.Where(x => x.TableKey == "%@D" + MainType && x.Hide == false && x.Types.Contains(Type)).Select(x => new { x.Eqsn, x.FldType, x.Fld }).ToList();
        //    for (int i = 0; i < trnaddons.Count; i++)
        //    {

        //        var Eqn = trnaddons[i].Eqsn == null ? "" : trnaddons[i].Eqsn.Trim();
        //        if (Eqn.Contains("%F"))
        //        {
        //            for (int ai = 0; ai < trnaddons.Count; ai++)
        //            {
        //                j = trnaddons[ai].Fld;
        //                if (LastModel[ai].FldType == "N")
        //                {
        //                    Eqn = Eqn.Replace("%" + j, (LastModel[ai].ApplCode == "" || LastModel[ai].ApplCode == null) ? "0" : LastModel[ai].ApplCode);

        //                    if (!Eqn.Contains("%F"))
        //                    {
        //                        break;
        //                    }
        //                }
        //                else if (LastModel[ai].FldType == "T" || LastModel[ai].FldType == "M" || LastModel[ai].FldType == "C")
        //                {
        //                    Eqn = Eqn.Replace("%" + j, (LastModel[ai].ApplCode == "" || LastModel[ai].ApplCode == null) ? "''" : LastModel[ai].ApplCode);

        //                    if (!Eqn.Contains("%F"))
        //                    {
        //                        break;
        //                    }
        //                }
        //                else if (LastModel[ai].FldType == "D" || LastModel[ai].FldType == "L")
        //                {
        //                    Eqn = "";
        //                    break;

        //                }

        //            }
        //        }
        //        if (Eqn != "")
        //        {
        //            Eqn = Eqn.Replace("%Party", Account);
        //            Eqn = Eqn.Replace("%Code", Code);
        //            mamt = GetAmtValueProdAddon(Eqn);
        //        }
        //        else
        //        {
        //            mamt = LastModel[i].ApplCode;
        //        }

        //        PopulateStr.Add(mamt);
        //    }
        //    return PopulateStr;
        //}

        public List<string> ConvertProdAddonString(List<AddOns> LastModel, string MainType, string Type, string Account, string Code)
        {
            string j;
            string ab;
            string mamt = "";
            List<string> PopulateStr = new List<string>();
            var trnaddons = ctxTFAT.AddOns.Where(x => x.TableKey == "%@D" + MainType && x.Hide == false && x.Types.Contains(Type)).Select(x => new { x.Eqsn, x.FldType, x.Fld, x.Sno }).OrderBy(x => new { x.Sno, x.Fld }).ToList();
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
                    Eqn = Eqn.Replace("%Party", Account);
                    Eqn = Eqn.Replace("%Code", Code);
                    mamt = GetAmtValueProdAddon(Eqn);
                }
                else
                {
                    mamt = LastModel[i].ApplCode;
                }

                PopulateStr.Add(mamt);
            }
            return PopulateStr;
        }

        public string GetAmtValueProdAddon(string finalvalue)
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
        public ActionResult AddSelectedCharges(PurchaseVM Model)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Fld", typeof(string));
            table.Columns.Add("Code", typeof(string));
            table.Columns.Add("Factor", typeof(string));
            table.Columns.Add("Equation", typeof(string));

            List<ChargesVM> objledgerdetail = new List<ChargesVM>();
            if (Session["chargesList"] != null)
            {
                objledgerdetail = (List<ChargesVM>)Session["chargesList"];
            }
            // Here we add five DataRows.
            table.Rows.Add(Model.Fld, Model.Code, Model.Factor, Model.Equation);
            for (int i = 0; i < table.Rows.Count; i++)
            {
                objledgerdetail.Add(new ChargesVM()
                {
                    Fld = table.Rows[i].Field<string>(0),
                    Code = table.Rows[i].Field<string>(1),
                    Factor = table.Rows[i].Field<string>(2),
                    Equation = table.Rows[i].Field<string>(3),
                    tEmpID = objledgerdetail.Count + 1,
                    tempIsDeleted1 = false
                }
                );
            }
            Session.Add("chargesList", objledgerdetail);
            var html = ViewHelper.RenderPartialView(this, "ChargeList", new ChargesVM() { Charges = objledgerdetail, Fld = "FLD" + (objledgerdetail.Count + 1).ToString() });
            return Json(new
            {
                Charges = objledgerdetail,
                Fld = "FLD" + (objledgerdetail.Count + 1).ToString(),
                Html = html
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CalByEquation(PurchaseVM Model)
        {
            List<string> declist = new List<string>();
            List<string> declist2 = new List<string>();
            declist = Model.Head.Split(',').ToList();
            if (Model.ValueLast == null || Model.ValueLast == "")
            {
                declist2.Add("");
            }
            else
            {
                declist2 = Model.ValueLast.Split(',').ToList();
            }
            string finaleqn;
            decimal mamt = 0;
            var chg = ctxTFAT.Charges.Where(x => x.Type == Model.Type && x.Fld == Model.Fld && x.DontUse == false).Select(x => new { x.Equation, x.TaxCode }).FirstOrDefault();
            finaleqn = ConvertString(declist, declist2, chg.Equation, Model.Type);
            mamt = GetAmtValue(finaleqn);
            decimal mpct = 0;
            if (Model.SubType == "RP" || Model.SubType == "CP" ||/* Model.SubType == "IM" ||*/ Model.SubType == "NP" || Model.SubType == "RS" || Model.SubType == "CS" || Model.SubType == "XS" || Model.SubType == "NS")
            {
                mpct = ctxTFAT.TaxMaster.Where(x => x.Code == chg.TaxCode).Select(x => (decimal?)x.Pct).FirstOrDefault() ?? 0;
            }
            var mtax = (mpct * Model.Amt) / 100;
            return Json(new
            {
                ALTNO = Model.Fld,
                Equation = mamt,
                TaxAmt = mtax,
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }

        //public string ConvertString(List<string> model, List<string> LastModel, string Equation, string Type)
        //{
        //    int j;
        //    string ab;
        //    var trncharges = ctxTFAT.Charges.Where(x => x.Type == Type && x.DontUse == false).Select(x => x).ToList().Count;
        //    for (int i = 1; i <= trncharges; i++)
        //    {
        //        j = i - 1;
        //        var Eqn = Equation.Replace("%", "");
        //        ab = Eqn.Replace("F" + i.ToString("D3"), model[j]).Replace("F" + i.ToString("D3"), LastModel[j]);

        //        Equation = ab;
        //    }
        //    return Equation;
        //}

        public string ConvertString(List<string> model, List<string> LastModel, string Equation, string Type)
        {
            int j;
            string ab;
            var trncharges = ctxTFAT.Charges.Where(x => x.Type == Type && x.DontUse == false).Select(x => new { x.Fld }).ToList();
            for (int i = 0; i < trncharges.Count; i++)
            {
                string abc = trncharges[i].Fld.Replace("F", "");
                j = Convert.ToInt32(abc);
                var Eqn = Equation.Replace("%", "");
                ab = Eqn.Replace("F" + j.ToString("D3"), model[i]).Replace("V" + j.ToString("D3"), LastModel[i]);

                Equation = ab;
            }
            return Equation;
        }


        public decimal GetAmtValue(string finalvalue)
        {
            string connstring = GetConnectionString();
            string sql = "";
            decimal mamtm = 0;
            sql = @"Select Top 1 " + finalvalue + " from TfatComp";
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

        [HttpPost]
        public ActionResult GetTotal(PurchaseVM Model)
        {
            decimal mamtm = 0;
            List<string> FinalHeadList = new List<string>();
            string abc = "0";
            string abc2 = "0";
            List<string> listhead = Model.Head.Split(',').ToList();
            for (int i = 0; i < listhead.Count; i++)
            {
                var a = listhead[i].Contains('+') || listhead[i].Contains('-') ? listhead[i] : "";
                if (a != "")
                {
                    abc = abc + a;
                }

            }
            List<string> listhead2 = Model.TaxAmtStr.Split(',').ToList();
            for (int i = 0; i < listhead2.Count; i++)
            {
                var a2 = listhead2[i].Contains('+') || listhead2[i].Contains('-') ? listhead2[i] : "";
                if (a2 != "")
                {
                    abc2 = abc2 + a2;
                }

            }
            List<PurchaseVM> objitemlist = new List<PurchaseVM>();
            if (Session["NewItemlist"] != null)
            {
                objitemlist = (List<PurchaseVM>)Session["NewItemlist"];
            }
            decimal gstamt = 0;
            if (Model.SubType == "RP" || Model.SubType == "CP" || /*Model.SubType == "IM" ||*/ Model.SubType == "NP" || Model.SubType == "RS" || Model.SubType == "CS" || Model.SubType == "XS" || Model.SubType == "NS")
            {
                gstamt = GetAmtValue(abc2);
            }


            if (objitemlist.Select(x => x.IGSTAmt).Sum() == 0)
            {
                Model.CGSTAmt = objitemlist.Select(x => x.CGSTAmt).Sum() + (gstamt / 2);
                Model.SGSTAmt = objitemlist.Select(x => x.SGSTAmt).Sum() + (gstamt / 2);
            }
            else
            {
                Model.IGSTAmt = objitemlist.Select(x => x.IGSTAmt).Sum() + gstamt;
            }

            if (Model.GSTType != "1")
            {
                mamtm = GetAmtValue(abc) + Model.IGSTAmt + Model.CGSTAmt + Model.SGSTAmt;
            }
            else
            {
                mamtm = GetAmtValue(abc);
            }
            if (Model.SubType == "RP")
            {
                Model.TDSAble = objitemlist.Select(x => x.Taxable).Sum();
            }
            if (Model.MainType == "SL")
            {
                Model.TDSAble = mamtm;
            }
            if (Model.SubType == "RP" || Model.MainType == "SL")
            {
                var datenow = DateTime.Now.Date;
                var TDSRATEtab = ctxTFAT.TDSRates.Where(x => x.Code == Model.TDSCode && x.EffDate <= datenow && x.LimitFrom <= Model.TDSAble).Select(x => new { x.TDSRate, x.Cess, x.SHECess, x.SurCharge }).FirstOrDefault();
                if (TDSRATEtab != null && Model.CutTDS == true)
                {
                    Model.TDSAmt = (Model.TDSAmt != 0) ? Model.TDSAmt : (TDSRATEtab.TDSRate == null ? 0 : (TDSRATEtab.TDSRate.Value * Model.TDSAble) / 100);
                    Model.TDSCess = (Model.TDSCess != 0) ? Model.TDSCess : (TDSRATEtab.Cess == null ? 0 : (TDSRATEtab.Cess.Value * Model.TDSAmt) / 100);
                    Model.TDSSchg = (Model.TDSSchg != 0) ? Model.TDSSchg : (TDSRATEtab.SurCharge == null ? 0 : (TDSRATEtab.SurCharge.Value * Model.TDSAmt) / 100);
                    Model.TDSSHECess = (Model.TDSSHECess != 0) ? Model.TDSSHECess : (TDSRATEtab.SHECess == null ? 0 : (TDSRATEtab.SHECess.Value * Model.TDSAmt) / 100);
                }
                else
                {
                    Model.TDSAmt = 0;
                    Model.TDSCess = 0;
                    Model.TDSSchg = 0;
                    Model.TDSSHECess = 0;
                }
            }

            if (Model.MainType == "SL" && Model.CutTDS == true)//for tcs
            {
                mamtm = mamtm + Model.TDSAmt + Model.TDSCess + Model.TDSSchg + Model.TDSSHECess;
            }


            var milamt = (mamtm * Model.MLSPercent) / 100;

            return Json(new
            {
                ALTNO = Model.Fld,
                Total = mamtm,
                IGSTAmt = Model.IGSTAmt,
                CGSTAmt = Model.CGSTAmt,
                SGSTAmt = Model.SGSTAmt,
                MLSAmount = milamt,
                TDSAble = Model.TDSAble,
                TDSAmt = Model.TDSAmt,
                TDSCess = Model.TDSCess,
                TDSSchg = Model.TDSSchg,
                TDSSHECess = Model.TDSSHECess,
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Disc Charges
        public ActionResult GetDiscountCharges(PurchaseVM Model)
        {

            try
            {
                PurchaseVM Model2 = new PurchaseVM();
                List<PriceListVM> objitemlist = new List<PriceListVM>();
                Model.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                bool allowdisccharges = false;

                if (Model.SubType == "NS" && Model.NonStock == true)
                {
                    allowdisccharges = false;
                }
                else if (Model.SubType == "RS" || Model.SubType == "CS" || Model.SubType == "XS" || Model.SubType == "OC" || Model.SubType == "SX" || Model.SubType == "OS" || Model.SubType == "QS" || Model.SubType == "ES" || Model.SubType == "DI" || Model.SubType == "PI" || (Model.SubType == "NS" && Model.NonStock == false))
                {
                    allowdisccharges = true;
                }
                //else if (Model.SubType == "EP" || Model.SubType == "QP" || Model.SubType == "OP" || Model.SubType == "CP" || Model.SubType == "IC" || Model.SubType == "RP" || Model.SubType == "NP" || Model.SubType == "IM" || Model.SubType == "PX" || Model.SubType == "GP")
                //{
                //    allowdisccharges = true;
                //}

                PurchaseVM Model3 = GetPriceListRate(Model.Code, Model.Account, Model.DocDate, Model.ClassValues1, Model.ClassValues2);
                Model.DiscNotAllowed = Model3.DiscNotAllowed;
                if (allowdisccharges == true)
                {
                    var PriceDiscList = GetPriceListDisc(Model.Code, Model.Account, Model.DocDate);
                    if (PriceDiscList != null && PriceDiscList.PriceListDiscCode != null && Model.DiscNotAllowed == false && Model.MainType == "SL")
                    {
                        for (int i = 1; i <= 6; i++)
                        {
                            PriceListVM pd = new PriceListVM();
                            switch (i)
                            {
                                case 1:
                                    pd.Disc = Convert.ToDouble(PriceDiscList.Disc1);
                                    pd.AddLess = (PriceDiscList.AddLess1 == "A") ? "+" : (PriceDiscList.AddLess1 == "L") ? "-" : "";
                                    pd.CalcOn = "";
                                    pd.DiscCaption = (PriceDiscList.DiscCaption1 == "" || PriceDiscList.DiscCaption1 == null) ? "Charge 1" : PriceDiscList.DiscCaption1;
                                    pd.PerValue = true;
                                    break;
                                case 2:
                                    pd.Disc = Convert.ToDouble(PriceDiscList.Disc2);
                                    pd.AddLess = (PriceDiscList.AddLess2 == "A") ? "+" : (PriceDiscList.AddLess2 == "L") ? "-" : "";
                                    pd.CalcOn = PriceDiscList.CalcOn2;
                                    pd.DiscCaption = (PriceDiscList.DiscCaption2 == "" || PriceDiscList.DiscCaption2 == null) ? "Charge 2" : PriceDiscList.DiscCaption2;
                                    pd.PerValue = true;
                                    break;
                                case 3:
                                    pd.Disc = Convert.ToDouble(PriceDiscList.Disc3);
                                    pd.AddLess = (PriceDiscList.AddLess3 == "A") ? "+" : (PriceDiscList.AddLess3 == "L") ? "-" : "";
                                    pd.CalcOn = PriceDiscList.CalcOn3;
                                    pd.DiscCaption = (PriceDiscList.DiscCaption3 == "" || PriceDiscList.DiscCaption3 == null) ? "Charge 3" : PriceDiscList.DiscCaption3;
                                    pd.PerValue = true;
                                    break;
                                case 4:
                                    pd.Disc = Convert.ToDouble(PriceDiscList.Disc4);
                                    pd.AddLess = (PriceDiscList.AddLess4 == "A") ? "+" : (PriceDiscList.AddLess4 == "L") ? "-" : "";
                                    pd.CalcOn = PriceDiscList.CalcOn4;
                                    pd.DiscCaption = (PriceDiscList.DiscCaption4 == "" || PriceDiscList.DiscCaption4 == null) ? "Charge 4" : PriceDiscList.DiscCaption4;
                                    pd.PerValue = true;
                                    break;
                                case 5:
                                    pd.Disc = Convert.ToDouble(PriceDiscList.Disc5);
                                    pd.AddLess = (PriceDiscList.AddLess5 == "A") ? "+" : (PriceDiscList.AddLess5 == "L") ? "-" : "";
                                    pd.CalcOn = PriceDiscList.CalcOn5;
                                    pd.DiscCaption = (PriceDiscList.DiscCaption5 == "" || PriceDiscList.DiscCaption5 == null) ? "Charge 5" : PriceDiscList.DiscCaption5;
                                    pd.PerValue = true;
                                    break;
                                case 6:
                                    pd.Disc = Convert.ToDouble(Model.DiscCharge6);
                                    pd.AddLess = "-";
                                    pd.CalcOn = "C";
                                    pd.DiscCaption = "Scheme Disc 1";
                                    pd.PerValue = true;
                                    break;
                            }
                            pd.tempid = i;
                            objitemlist.Add(pd);
                        }
                        Model2.PriceDiscCode = PriceDiscList.PriceListDiscCode;

                    }
                }


                if (objitemlist == null || objitemlist.Count() == 0)
                {
                    var ProductCharges = ctxTFAT.AddCharges.Where(x => x.Hide == false && x.Type == Model.MainType.Substring(0, 1)).Select(x => x).ToList();
                    int distemp = 1;
                    foreach (var a in ProductCharges)
                    {
                        objitemlist.Add(new PriceListVM()
                        {
                            Disc = Convert.ToDouble(a.Amount.Value),
                            AddLess = (a.CalOperater == "A") ? "+" : (a.CalOperater == "L") ? "-" : "",
                            CalcOn = "C",
                            DiscCaption = a.Name,
                            PerValue = a.PerOrValue,
                            tempid = distemp
                        });
                        distemp = distemp + 1;

                    }

                }

                Model2.DiscChargeList = objitemlist;

                var html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/Purchase/DiscountChargesAdd.cshtml", new PurchaseVM() { DiscChargeList = objitemlist });
                var jsonResult = Json(new
                {
                    Status = "Success",
                    Html = html,
                    PriceDiscCode = Model2.PriceDiscCode
                }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            catch (Exception ex)
            {
                var jsonResult = Json(new
                {
                    Status = "Error",
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
                return jsonResult;
            }


        }

        public ActionResult GetDiscountChargesEdit(PurchaseVM Model)
        {
            Model.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            PurchaseVM m3 = new PurchaseVM();
            m3 = GetRateByCircular(Model);
            if (m3.DiscNotAllowed == true)
            {
                return Json(new { Status = "DiscNoAllow" }, JsonRequestBehavior.AllowGet);
            }

            List<PurchaseVM> objitemlist = new List<PurchaseVM>();
            if (Session["NewItemlist"] != null)
            {
                objitemlist = (List<PurchaseVM>)Session["NewItemlist"];
            }
            bool allowdisccharges = false;

            if (Model.SubType == "NS" && Model.NonStock == true)
            {
                allowdisccharges = false;
            }
            else if (Model.SubType == "RS" || Model.SubType == "CS" || Model.SubType == "XS" || Model.SubType == "OC" || Model.SubType == "SX" || Model.SubType == "OS" || Model.SubType == "QS" || Model.SubType == "ES" || Model.SubType == "DI" || Model.SubType == "PI" || (Model.SubType == "NS" && Model.NonStock == false))
            {
                allowdisccharges = true;
            }
            //else if (Model.SubType == "EP" || Model.SubType == "QP" || Model.SubType == "OP" || Model.SubType == "CP" || Model.SubType == "IC" || Model.SubType == "RP" || Model.SubType == "NP" || Model.SubType == "IM" || Model.SubType == "PX" || Model.SubType == "GP")
            //{
            //    allowdisccharges = true;
            //}

            List<PriceListVM> DiscChargeList = new List<PriceListVM>();
            PurchaseVM Model2 = new PurchaseVM();
            var originalcode = objitemlist.Where(x => x.tempId == Model.tempId).Select(x => x.Code).FirstOrDefault();

            var PriceDiscList = objitemlist.Where(x => x.tempId == Model.tempId).Select(x => x.DiscChargeList).FirstOrDefault();

            if ((PriceDiscList == null || PriceDiscList.Count == 0 || (Model.Code != originalcode)) && (allowdisccharges == true))
            {
                PurchaseVM Model3 = GetPriceListRate(Model.Code, Model.Account, Model.DocDate, Model.ClassValues1, Model.ClassValues2);
                Model.DiscNotAllowed = Model3.DiscNotAllowed;
                var PriceDiscList2 = GetPriceListDisc(Model.Code, Model.Account, Model.DocDate);
                if (PriceDiscList2 != null && PriceDiscList2.PriceListDiscCode != null && Model.DiscNotAllowed == false)
                {
                    for (int ia = 1; ia <= 6; ia++)
                    {
                        PriceListVM pd = new PriceListVM();
                        switch (ia)
                        {
                            case 1:
                                pd.Disc = Convert.ToDouble(PriceDiscList2.Disc1);
                                pd.AddLess = (PriceDiscList2.AddLess1 == "A") ? "+" : (PriceDiscList2.AddLess1 == "L") ? "-" : "";
                                pd.CalcOn = "";
                                pd.DiscCaption = (PriceDiscList2.DiscCaption1 == "" || PriceDiscList2.DiscCaption1 == null) ? "Charge 1" : PriceDiscList2.DiscCaption1;
                                pd.PerValue = true;
                                break;
                            case 2:
                                pd.Disc = Convert.ToDouble(PriceDiscList2.Disc2);
                                pd.AddLess = (PriceDiscList2.AddLess2 == "A") ? "+" : (PriceDiscList2.AddLess2 == "L") ? "-" : "";
                                pd.CalcOn = PriceDiscList2.CalcOn2;
                                pd.DiscCaption = (PriceDiscList2.DiscCaption2 == "" || PriceDiscList2.DiscCaption2 == null) ? "Charge 2" : PriceDiscList2.DiscCaption2;
                                pd.PerValue = true;
                                break;
                            case 3:
                                pd.Disc = Convert.ToDouble(PriceDiscList2.Disc3);
                                pd.AddLess = (PriceDiscList2.AddLess3 == "A") ? "+" : (PriceDiscList2.AddLess3 == "L") ? "-" : "";
                                pd.CalcOn = PriceDiscList2.CalcOn3;
                                pd.DiscCaption = (PriceDiscList2.DiscCaption3 == "" || PriceDiscList2.DiscCaption3 == null) ? "Charge 3" : PriceDiscList2.DiscCaption3;
                                pd.PerValue = true;
                                break;
                            case 4:
                                pd.Disc = Convert.ToDouble(PriceDiscList2.Disc4);
                                pd.AddLess = (PriceDiscList2.AddLess4 == "A") ? "+" : (PriceDiscList2.AddLess4 == "L") ? "-" : "";
                                pd.CalcOn = PriceDiscList2.CalcOn4;
                                pd.DiscCaption = (PriceDiscList2.DiscCaption4 == "" || PriceDiscList2.DiscCaption4 == null) ? "Charge 4" : PriceDiscList2.DiscCaption4;
                                pd.PerValue = true;
                                break;
                            case 5:
                                pd.Disc = Convert.ToDouble(PriceDiscList2.Disc5);
                                pd.AddLess = (PriceDiscList2.AddLess5 == "A") ? "+" : (PriceDiscList2.AddLess5 == "L") ? "-" : "";
                                pd.CalcOn = PriceDiscList2.CalcOn5;
                                pd.DiscCaption = (PriceDiscList2.DiscCaption5 == "" || PriceDiscList2.DiscCaption5 == null) ? "Charge 5" : PriceDiscList2.DiscCaption5;
                                pd.PerValue = true;
                                break;
                            case 6:
                                pd.Disc = Convert.ToDouble(Model.DiscCharge6);
                                pd.AddLess = "-";
                                pd.CalcOn = "C";
                                pd.DiscCaption = "Scheme Disc 1";
                                pd.PerValue = true;
                                break;
                        }
                        pd.tempid = ia;
                        DiscChargeList.Add(pd);
                    }
                    Model.PriceDiscCode = PriceDiscList2.PriceListDiscCode;

                }
                if (DiscChargeList == null || DiscChargeList.Count() == 0)//Product Charges
                {
                    var Productcharges2 = ctxTFAT.AddCharges.Where(x => x.Hide == false && x.Type == Model.MainType.Substring(0, 1)).Select(x => x).ToList();
                    int distemp = 1;
                    foreach (var a in Productcharges2)
                    {
                        DiscChargeList.Add(new PriceListVM()
                        {
                            Disc = Convert.ToDouble(a.Amount.Value),
                            AddLess = (a.CalOperater == "A") ? "+" : (a.CalOperater == "L") ? "-" : "",
                            CalcOn = "C",
                            DiscCaption = a.Name,
                            PerValue = a.PerOrValue,
                            tempid = distemp
                        });
                        distemp = distemp + 1;

                    }
                }
            }
            else
            {
                int i = 1;
                if (PriceDiscList != null && PriceDiscList.Count > 0)
                {
                    foreach (var a in PriceDiscList)
                    {
                        PriceListVM pd = new PriceListVM();
                        pd.Disc = a.Disc;
                        pd.AddLess = a.AddLess;
                        pd.CalcOn = a.CalcOn;
                        pd.DiscAmt = a.DiscAmt;
                        pd.DiscCaption = a.DiscCaption;
                        pd.tempid = i;
                        pd.PerValue = a.PerValue;
                        DiscChargeList.Add(pd);
                        i = i + 1;
                    }
                }
                Model.PriceDiscCode = objitemlist.Where(x => x.tempId == Model.tempId).Select(x => x.PriceDiscCode).FirstOrDefault();

            }



            Model2.DiscChargeList = DiscChargeList;
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/Purchase/DiscountChargesEdit.cshtml", new PurchaseVM() { DiscChargeList = Model2.DiscChargeList });
            var jsonResult = Json(new
            {
                Html = html,
                PriceDiscCode = Model.PriceDiscCode
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }

        public ActionResult CalDiscountCharges(PurchaseVM Model)
        {
            double discstr2 = 0;
            decimal taxble = 0;
            List<PriceListVM> PriceList = new List<PriceListVM>();

            if (Model.RateOn2 == true)
            {
                taxble = Convert.ToDecimal(Model.Qty2) * Convert.ToDecimal(Model.Rate);
            }
            else
            {
                taxble = Convert.ToDecimal(Model.Qty) * Convert.ToDecimal(Model.Rate);
            }

            if (taxble == 0)
            {
                taxble = 1;
            }
            Model.Taxable = taxble;
            bool allowdisccharges = false;
            //if (Model.MainType == "SL")
            //{
            //    if (Model.SubType == "NS" && Model.NonStock == true)
            //    {
            //        allowdisccharges = false;
            //    }
            //    else if (Model.SubType == "RS" || Model.SubType == "CS" || Model.SubType == "XS" || Model.SubType == "OC" || Model.SubType == "SX" || Model.SubType == "OS" || Model.SubType == "QS" || Model.SubType == "ES" || Model.SubType == "DI" || Model.SubType == "PI" || (Model.SubType == "NS" && Model.NonStock == false))
            //    {
            //        allowdisccharges = true;
            //    }

            //}
            if (Model.DiscChargeList != null && Model.DiscChargeList.Count > 0 && Model.DiscNotAllowed == false /*&& (allowdisccharges == true)*/)
            {
                for (int a = 0; a < Model.DiscChargeList.Count(); a++)
                {
                    PriceListVM Model2 = new PriceListVM();
                    if (Model.DiscChargeList[a].AddLess == "D" || Model.DiscChargeList[a].AddLess == null || Model.DiscChargeList[a].AddLess == "")
                    {
                        Model.DiscChargeList[a].AddLess = "";

                    }
                    if (a == 0)
                    {
                        if (Model.DiscChargeList[a].PerValue == true)
                        {
                            discstr2 = GetValDiscCharge(Model.DiscChargeList[a].AddLess + "Cast(" + Model.DiscChargeList[a].Disc + "as float )" + "*" + "Cast(" + Model.Taxable + "as float )" + "/" + 100);
                        }
                        else
                        {
                            discstr2 = GetValDiscCharge(Model.DiscChargeList[a].AddLess + "Cast(" + Model.DiscChargeList[a].Disc + "as float )");
                        }
                        Model2.Disc = Model.DiscChargeList[a].Disc;
                        Model2.DiscAmt = discstr2;
                        Model2.AddLess = Model.DiscChargeList[a].AddLess;
                        Model2.CalcOn = Model.DiscChargeList[a].CalcOn;
                        Model2.Taxable = Model.Taxable + Convert.ToDecimal(discstr2);
                        Model2.tempid = a + 1;
                        Model2.DiscCaption = Model.DiscChargeList[a].DiscCaption;
                        Model2.PerValue = Model.DiscChargeList[a].PerValue;
                    }
                    else
                    {
                        if (Model.DiscChargeList[a].CalcOn == "P")
                        {
                            if (Model.DiscChargeList[a].PerValue == true)
                            {
                                discstr2 = GetValDiscCharge(Model.DiscChargeList[a].AddLess + "Cast(" + Model.DiscChargeList[a].Disc + "as float )" + "*" + "Cast(" + taxble + "as float )" + "/" + 100);
                            }
                            else
                            {
                                discstr2 = GetValDiscCharge(Model.DiscChargeList[a].AddLess + "Cast(" + Model.DiscChargeList[a].Disc + "as float )");
                            }
                            Model2.Disc = Model.DiscChargeList[a].Disc;
                            Model2.DiscAmt = discstr2;
                            Model2.AddLess = Model.DiscChargeList[a].AddLess;
                            Model2.CalcOn = Model.DiscChargeList[a].CalcOn;
                            Model2.Taxable = taxble + Convert.ToDecimal(discstr2);
                            Model2.tempid = a + 1;
                            Model2.DiscCaption = Model.DiscChargeList[a].DiscCaption;
                            Model2.PerValue = Model.DiscChargeList[a].PerValue;
                        }
                        else
                        {
                            for (int b = a - 1; b < a; b++)
                            {

                                if (Model.DiscChargeList[a].PerValue == true)
                                {
                                    discstr2 = GetValDiscCharge(Model.DiscChargeList[a].AddLess + "Cast(" + Model.DiscChargeList[a].Disc + "as float )" + "*" + "Cast(" + PriceList[b].Taxable + "as float )" + "/" + 100);
                                }
                                else
                                {
                                    discstr2 = GetValDiscCharge(Model.DiscChargeList[a].AddLess + "Cast(" + Model.DiscChargeList[a].Disc + "as float )");
                                }
                                Model2.Disc = Model.DiscChargeList[a].Disc;
                                Model2.DiscAmt = discstr2;
                                Model2.AddLess = Model.DiscChargeList[a].AddLess;
                                Model2.CalcOn = Model.DiscChargeList[a].CalcOn;
                                Model2.Taxable = PriceList[b].Taxable + Convert.ToDecimal(discstr2);
                                Model2.tempid = a + 1;
                                Model2.DiscCaption = Model.DiscChargeList[a].DiscCaption;
                                Model2.PerValue = Model.DiscChargeList[a].PerValue;
                            }
                        }

                    }
                    PriceList.Add(Model2);
                }

            }

            Model.DiscAmt = PriceList.Select(x => (decimal?)x.DiscAmt).Sum() ?? 0;
            Model.DiscAmt = Math.Round(Model.DiscAmt, 2);
            Model.Taxable = taxble + Model.DiscAmt;
            Model.Taxable = Math.Round(Model.Taxable, 2);
            Model.Disc = (Model.DiscAmt * 100) / taxble;
            Model.Disc = Math.Round(Model.Disc, 2);
            var html = ViewHelper.RenderPartialView(this, "DiscountChargesAdd", new PurchaseVM() { DiscChargeList = PriceList });

            return Json(new
            {
                Html = html,
                Taxable = Model.Taxable,
                DiscAmt = Model.DiscAmt,
                Disc = Model.Disc,
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CalDiscountChargesEdit(PurchaseVM Model)//pending
        {
            double discstr2 = 0;
            decimal taxble = 0;

            PurchaseVM m3 = new PurchaseVM();
            m3 = GetRateByCircular(Model);
            Model.DiscNotAllowed = m3.DiscNotAllowed;
            if (Model.DiscNotAllowed == true)
            {
                return Json(new { Status = "DiscNoAllow" }, JsonRequestBehavior.AllowGet);
            }

            List<PriceListVM> PriceList = new List<PriceListVM>();


            if (Model.RateOn2 == true)
            {
                taxble = Convert.ToDecimal(Model.Qty2) * Convert.ToDecimal(Model.Rate);
            }
            else
            {
                taxble = Convert.ToDecimal(Model.Qty) * Convert.ToDecimal(Model.Rate);
            }

            if (taxble == 0)
            {
                taxble = 1;
            }
            Model.Taxable = taxble;
            bool allowdisccharges = false;
            //if (Model.MainType == "SL")
            //{
            //    if (Model.SubType == "NS" && Model.NonStock == true)
            //    {
            //        allowdisccharges = false;
            //    }
            //    else if (Model.SubType == "RS" || Model.SubType == "CS" || Model.SubType == "XS" || Model.SubType == "OC" || Model.SubType == "SX" || Model.SubType == "OS" || Model.SubType == "QS" || Model.SubType == "ES" || Model.SubType == "DI" || Model.SubType == "PI" || (Model.SubType == "NS" && Model.NonStock == false))
            //    {
            //        allowdisccharges = true;
            //    }
            //}
            if (Model.DiscChargeList != null && Model.DiscChargeList.Count > 0 && Model.DiscNotAllowed == false /*&& (allowdisccharges == true)*/)
            {
                for (int a = 0; a < Model.DiscChargeList.Count(); a++)
                {
                    PriceListVM Model2 = new PriceListVM();
                    if (Model.DiscChargeList[a].AddLess == "D" || Model.DiscChargeList[a].AddLess == null || Model.DiscChargeList[a].AddLess == "")
                    {
                        Model.DiscChargeList[a].AddLess = "";

                    }
                    if (a == 0)
                    {
                        if (Model.DiscChargeList[a].PerValue == true)
                        {
                            discstr2 = GetValDiscCharge(Model.DiscChargeList[a].AddLess + "Cast(" + Model.DiscChargeList[a].Disc + "as float )" + "*" + "Cast(" + Model.Taxable + "as float )" + "/" + 100);
                        }
                        else
                        {
                            discstr2 = GetValDiscCharge(Model.DiscChargeList[a].AddLess + "Cast(" + Model.DiscChargeList[a].Disc + "as float )");
                        }
                        Model2.Disc = Model.DiscChargeList[a].Disc;
                        Model2.DiscAmt = discstr2;
                        Model2.AddLess = Model.DiscChargeList[a].AddLess;
                        Model2.CalcOn = Model.DiscChargeList[a].CalcOn;
                        Model2.Taxable = Model.Taxable + Convert.ToDecimal(discstr2);
                        Model2.tempid = a + 1;
                        Model2.DiscCaption = Model.DiscChargeList[a].DiscCaption;
                        Model2.PerValue = Model.DiscChargeList[a].PerValue;
                    }
                    else
                    {
                        if (Model.DiscChargeList[a].CalcOn == "P")
                        {
                            if (Model.DiscChargeList[a].PerValue == true)
                            {
                                discstr2 = GetValDiscCharge(Model.DiscChargeList[a].AddLess + "Cast(" + Model.DiscChargeList[a].Disc + "as float )" + "*" + "Cast(" + taxble + "as float )" + "/" + 100);
                            }
                            else
                            {
                                discstr2 = GetValDiscCharge(Model.DiscChargeList[a].AddLess + "Cast(" + Model.DiscChargeList[a].Disc + "as float )");
                            }

                            Model2.Disc = Model.DiscChargeList[a].Disc;
                            Model2.DiscAmt = discstr2;
                            Model2.AddLess = Model.DiscChargeList[a].AddLess;
                            Model2.CalcOn = Model.DiscChargeList[a].CalcOn;
                            Model2.Taxable = taxble + Convert.ToDecimal(discstr2);
                            Model2.tempid = a + 1;
                            Model2.DiscCaption = Model.DiscChargeList[a].DiscCaption;
                            Model2.PerValue = Model.DiscChargeList[a].PerValue;
                        }
                        else
                        {
                            for (int b = a - 1; b < a; b++)
                            {
                                if (Model.DiscChargeList[a].PerValue == true)
                                {
                                    discstr2 = GetValDiscCharge(Model.DiscChargeList[a].AddLess + "Cast(" + Model.DiscChargeList[a].Disc + "as float )" + "*" + "Cast(" + PriceList[b].Taxable + "as float )" + "/" + 100);
                                }
                                else
                                {
                                    discstr2 = GetValDiscCharge(Model.DiscChargeList[a].AddLess + "Cast(" + Model.DiscChargeList[a].Disc + "as float )");
                                }
                                Model2.Disc = Model.DiscChargeList[a].Disc;
                                Model2.DiscAmt = discstr2;
                                Model2.AddLess = Model.DiscChargeList[a].AddLess;
                                Model2.CalcOn = Model.DiscChargeList[a].CalcOn;
                                Model2.Taxable = PriceList[b].Taxable + Convert.ToDecimal(discstr2);
                                Model2.tempid = a + 1;
                                Model2.DiscCaption = Model.DiscChargeList[a].DiscCaption;
                                Model2.PerValue = Model.DiscChargeList[a].PerValue;
                            }
                        }

                    }
                    PriceList.Add(Model2);
                }
            }
            //commented if item change and if not discount charges structure in it and then didnt update
            //var result = (List<PurchaseVM>)Session["NewItemlist"];
            //var list = result.Where(x => x.tempId == Model.tempId).Select(x => x).ToList();
            //foreach (var a in list)
            //{
            //    a.DiscChargeList = PriceList;
            //}

            Model.DiscAmt = PriceList.Select(x => (decimal?)x.DiscAmt).Sum() ?? 0;
            Model.DiscAmt = Math.Round(Model.DiscAmt, 2);
            Model.Taxable = taxble + Model.DiscAmt;
            Model.Taxable = Math.Round(Model.Taxable, 2);
            Model.Disc = (Model.DiscAmt * 100) / taxble;
            Model.Disc = Math.Round(Model.Disc, 2);
            var html = ViewHelper.RenderPartialView(this, "DiscountChargesEdit", new PurchaseVM() { DiscChargeList = PriceList });

            return Json(new
            {
                Html = html,
                Taxable = Model.Taxable,
                DiscAmt = Model.DiscAmt,
                Disc = Model.Disc,
            }, JsonRequestBehavior.AllowGet);
        }

        public List<PriceListVM> GetDiscountChgListInPickUp(string TableKey, string SubType, string PickInMainType, string ProductCode, double ConvFactor, string mCurrentSubType, bool mCurrentNonStock)
        {
            PurchaseVM Model2 = new PurchaseVM();
            List<PriceListVM> objitemlist = new List<PriceListVM>();
            bool allowdisccharges = false;
            string mMaintype = ctxTFAT.DocTypes.Where(x => x.SubType == mCurrentSubType).Select(x => x.MainType).FirstOrDefault();

            if (mCurrentSubType == "NS" && mCurrentNonStock == true)
            {
                allowdisccharges = false;
            }
            else if (mCurrentSubType == "OS" || mCurrentSubType == "ES" || mCurrentSubType == "QS" || mCurrentSubType == "OC" || mCurrentSubType == "RS" || mCurrentSubType == "CS" || mCurrentSubType == "XS" || mCurrentSubType == "SX" || mCurrentSubType == "DI" || mCurrentSubType == "PI" || (mCurrentSubType == "NS" && mCurrentNonStock == false))
            {
                allowdisccharges = true;
            }
            //else if (mCurrentSubType == "EP" || mCurrentSubType == "QP" || mCurrentSubType == "OP" || mCurrentSubType == "CP" || mCurrentSubType == "IC" || mCurrentSubType == "RP" || mCurrentSubType == "NP" || mCurrentSubType == "IM" || mCurrentSubType == "PX" || mCurrentSubType == "GP")
            //{
            //    allowdisccharges = true;
            //}

           
             if ((SubType == "CP") || (SubType == "IC") || (SubType == "RP") || (SubType == "NP") || (SubType == "IM") || (SubType == "PX") || (SubType == "GP") || SubType == "OC" || SubType == "RS" || SubType == "CS" || SubType == "XS" || SubType == "SX" || SubType == "NS")
            {
                var PriceDiscList = (from i in ctxTFAT.Stock.Where(x => x.TableKey == TableKey)
                                     select i).FirstOrDefault();
                Model2.DiscCharge1 = PriceDiscList.ChgPer1 == null ? 0 : PriceDiscList.ChgPer1.Value;
                Model2.DiscChargeAmt1 = PriceDiscList.ChgAmt1 == null ? 0 : PriceDiscList.ChgAmt1.Value;
                Model2.DiscCharge2 = PriceDiscList.ChgPer2 == null ? 0 : PriceDiscList.ChgPer2.Value;
                Model2.DiscChargeAmt2 = PriceDiscList.ChgAmt2 == null ? 0 : PriceDiscList.ChgAmt2.Value;
                Model2.DiscCharge3 = PriceDiscList.ChgPer3 == null ? 0 : PriceDiscList.ChgPer3.Value;
                Model2.DiscChargeAmt3 = PriceDiscList.ChgAmt3 == null ? 0 : PriceDiscList.ChgAmt3.Value;
                Model2.DiscCharge4 = PriceDiscList.ChgPer4 == null ? 0 : PriceDiscList.ChgPer4.Value;
                Model2.DiscChargeAmt4 = PriceDiscList.ChgAmt4 == null ? 0 : PriceDiscList.ChgAmt4.Value;
                Model2.DiscCharge5 = PriceDiscList.ChgPer5 == null ? 0 : PriceDiscList.ChgPer5.Value;
                Model2.DiscChargeAmt5 = PriceDiscList.ChgAmt5 == null ? 0 : PriceDiscList.ChgAmt5.Value;
                Model2.DiscCharge6 = PriceDiscList.ChgPer6 == null ? 0 : PriceDiscList.ChgPer6.Value;
                Model2.DiscChargeAmt6 = PriceDiscList.ChgAmt6 == null ? 0 : PriceDiscList.ChgAmt6.Value;
                Model2.Code = PriceDiscList.Code;
                Model2.Account = PriceDiscList.Party;
                Model2.DocDate = PriceDiscList.DocDate;
                Model2.PriceDiscCode = PriceDiscList.PriceListDisc;
            }
           
            if (allowdisccharges == true)
            {
                //var PriceDiscList2 = ctxTFAT.PriceListsDisc.Where(x => x.Code == Model2.PriceDiscCode).Select(x => x).FirstOrDefault();
                //if (PriceDiscList2 != null)
                //{
                //    for (int i = 1; i <= 6; i++)
                //    {
                //        PriceListVM pd = new PriceListVM();
                //        switch (i)
                //        {
                //            case 1:
                //                pd.Disc = Convert.ToDouble(Model2.DiscCharge1);
                //                pd.AddLess = (PriceDiscList2.DiscAddLess1 == "A") ? "+" : (PriceDiscList2.DiscAddLess1 == "L") ? "-" : "";
                //                pd.CalcOn = "";
                //                pd.DiscAmt = Convert.ToDouble(Model2.DiscChargeAmt1) * Convert.ToDouble(ConvFactor);
                //                pd.DiscCaption = (PriceDiscList2.DiscCaption1 == "" || PriceDiscList2.DiscCaption1 == null) ? "Charge 1" : PriceDiscList2.DiscCaption1;
                //                pd.PerValue = true;
                //                break;
                //            case 2:
                //                pd.Disc = Convert.ToDouble(Model2.DiscCharge2);
                //                pd.AddLess = (PriceDiscList2.DiscAddLess2 == "A") ? "+" : (PriceDiscList2.DiscAddLess2 == "L") ? "-" : "";
                //                pd.CalcOn = PriceDiscList2.DiscApply2;
                //                pd.DiscAmt = Convert.ToDouble(Model2.DiscChargeAmt2) * Convert.ToDouble(ConvFactor);
                //                pd.DiscCaption = (PriceDiscList2.DiscCaption2 == "" || PriceDiscList2.DiscCaption2 == null) ? "Charge 2" : PriceDiscList2.DiscCaption2;
                //                pd.PerValue = true;
                //                break;
                //            case 3:
                //                pd.Disc = Convert.ToDouble(Model2.DiscCharge3);
                //                pd.AddLess = (PriceDiscList2.DiscAddLess3 == "A") ? "+" : (PriceDiscList2.DiscAddLess3 == "L") ? "-" : "";
                //                pd.CalcOn = PriceDiscList2.DiscApply3;
                //                pd.DiscAmt = Convert.ToDouble(Model2.DiscChargeAmt3) * Convert.ToDouble(ConvFactor);
                //                pd.DiscCaption = (PriceDiscList2.DiscCaption3 == "" || PriceDiscList2.DiscCaption3 == null) ? "Charge 3" : PriceDiscList2.DiscCaption3;
                //                pd.PerValue = true;
                //                break;
                //            case 4:
                //                pd.Disc = Convert.ToDouble(Model2.DiscCharge4);
                //                pd.AddLess = (PriceDiscList2.DiscAddLess4 == "A") ? "+" : (PriceDiscList2.DiscAddLess4 == "L") ? "-" : "";
                //                pd.CalcOn = PriceDiscList2.DiscApply4;
                //                pd.DiscAmt = Convert.ToDouble(Model2.DiscChargeAmt4) * Convert.ToDouble(ConvFactor);
                //                pd.DiscCaption = (PriceDiscList2.DiscCaption4 == "" || PriceDiscList2.DiscCaption4 == null) ? "Charge 4" : PriceDiscList2.DiscCaption4;
                //                pd.PerValue = true;
                //                break;
                //            case 5:
                //                pd.Disc = Convert.ToDouble(Model2.DiscCharge5);
                //                pd.AddLess = (PriceDiscList2.DiscAddLess5 == "A") ? "+" : (PriceDiscList2.DiscAddLess5 == "L") ? "-" : "";
                //                pd.CalcOn = PriceDiscList2.DiscApply5;
                //                pd.DiscAmt = Convert.ToDouble(Model2.DiscChargeAmt5) * Convert.ToDouble(ConvFactor);
                //                pd.DiscCaption = (PriceDiscList2.DiscCaption5 == "" || PriceDiscList2.DiscCaption5 == null) ? "Charge 5" : PriceDiscList2.DiscCaption5;
                //                pd.PerValue = true;
                //                break;
                //            case 6:
                //                pd.Disc = Convert.ToDouble(Model2.DiscCharge6);
                //                pd.AddLess = "-";
                //                pd.CalcOn = "C";
                //                pd.DiscAmt = Convert.ToDouble(Model2.DiscChargeAmt6) * Convert.ToDouble(ConvFactor);
                //                pd.DiscCaption = "Scheme Discount 1";
                //                pd.PerValue = true;
                //                break;

                //        }
                //        pd.tempid = i;
                //        objitemlist.Add(pd);
                //    }
                //}
            }

            if (objitemlist == null || objitemlist.Count() == 0)
            {
                var ProductCharges = ctxTFAT.AddCharges.Where(x => x.Hide == false && x.Type == PickInMainType.Substring(0, 1)).Select(x => x).ToList();
                int distemp = 1;
                foreach (var a in ProductCharges)
                {
                    double mdisc = 0;
                    double mdiscamt = 0;
                    if (distemp == 1)
                    {
                        mdisc = Convert.ToDouble(Model2.DiscCharge1);
                        mdiscamt = Convert.ToDouble(Model2.DiscChargeAmt1);
                    }
                    else if (distemp == 2)
                    {
                        mdisc = Convert.ToDouble(Model2.DiscCharge2);
                        mdiscamt = Convert.ToDouble(Model2.DiscChargeAmt2);
                    }
                    else if (distemp == 3)
                    {
                        mdisc = Convert.ToDouble(Model2.DiscCharge3);
                        mdiscamt = Convert.ToDouble(Model2.DiscChargeAmt3);
                    }
                    else if (distemp == 4)
                    {
                        mdisc = Convert.ToDouble(Model2.DiscCharge4);
                        mdiscamt = Convert.ToDouble(Model2.DiscChargeAmt4);
                    }
                    else if (distemp == 5)
                    {
                        mdisc = Convert.ToDouble(Model2.DiscCharge5);
                        mdiscamt = Convert.ToDouble(Model2.DiscChargeAmt5);
                    }
                    objitemlist.Add(new PriceListVM()
                    {
                        Disc = mdisc,
                        DiscAmt = mdiscamt,
                        AddLess = (a.CalOperater == "A") ? "+" : (a.CalOperater == "L") ? "-" : "",
                        CalcOn = "C",
                        DiscCaption = a.Name,
                        PerValue = a.PerOrValue,
                        tempid = distemp
                    });
                    distemp = distemp + 1;
                }
            }
            return objitemlist;
        }

        public double GetValDiscCharge(string finalvalue)
        {
            string connstring = GetConnectionString();
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

            DataTable smDt = GetDataTable(sql, connstring);

            if (smDt.Rows.Count > 0)
            {
                mamtm = (smDt.Rows[0][0].ToString() == "" || smDt.Rows[0][0].ToString() == null) ? "0" : smDt.Rows[0][0].ToString();
            }
            else
            {
                mamtm = "0";
            }
            return Convert.ToDouble(mamtm);
        }

        public List<PriceListVM> GetDiscountChgListInEdit(string TableKey, string SubType, string MainType, string ProductCode, bool NonStock)
        {
            PurchaseVM Model2 = new PurchaseVM();
            List<PriceListVM> objitemlist = new List<PriceListVM>();
            bool allowdisccharges = false;
            //string mMaintype = ctxTFAT.DocTypes.Where(x => x.SubType == SubType).Select(x => x.MainType).FirstOrDefault();
            //if (mMaintype == "SL")
            //{
            if (SubType == "NS" && NonStock == true)
            {
                allowdisccharges = false;
            }
            else if (SubType == "OS" || SubType == "ES" || SubType == "QS" || SubType == "OC" || SubType == "RS" || SubType == "CS" || SubType == "XS" || SubType == "SX" || SubType == "DI" || SubType == "PI" || (SubType == "NS" && NonStock == false))
            {
                allowdisccharges = true;
            }
            //else if (SubType == "EP" || SubType == "QP" || SubType == "OP" || SubType == "CP" || SubType == "IC" || SubType == "RP" || SubType == "NP" || SubType == "IM" || SubType == "PX" || SubType == "GP")
            //{
            //    allowdisccharges = true;
            //}

            //}
             if ((SubType == "CP") || (SubType == "IC") || (SubType == "RP") || (SubType == "NP") || (SubType == "IM") || (SubType == "PX") || (SubType == "GP") || SubType == "OC" || SubType == "RS" || SubType == "CS" || SubType == "XS" || SubType == "SX" || SubType == "NS")
            {
                var PriceDiscList = (from i in ctxTFAT.Stock.Where(x => x.TableKey == TableKey)
                                     select i).FirstOrDefault();
                Model2.DiscCharge1 = PriceDiscList.ChgPer1 == null ? 0 : PriceDiscList.ChgPer1.Value;
                Model2.DiscChargeAmt1 = PriceDiscList.ChgAmt1 == null ? 0 : PriceDiscList.ChgAmt1.Value;
                Model2.DiscCharge2 = PriceDiscList.ChgPer2 == null ? 0 : PriceDiscList.ChgPer2.Value;
                Model2.DiscChargeAmt2 = PriceDiscList.ChgAmt2 == null ? 0 : PriceDiscList.ChgAmt2.Value;
                Model2.DiscCharge3 = PriceDiscList.ChgPer3 == null ? 0 : PriceDiscList.ChgPer3.Value;
                Model2.DiscChargeAmt3 = PriceDiscList.ChgAmt3 == null ? 0 : PriceDiscList.ChgAmt3.Value;
                Model2.DiscCharge4 = PriceDiscList.ChgPer4 == null ? 0 : PriceDiscList.ChgPer4.Value;
                Model2.DiscChargeAmt4 = PriceDiscList.ChgAmt4 == null ? 0 : PriceDiscList.ChgAmt4.Value;
                Model2.DiscCharge5 = PriceDiscList.ChgPer5 == null ? 0 : PriceDiscList.ChgPer5.Value;
                Model2.DiscChargeAmt5 = PriceDiscList.ChgAmt5 == null ? 0 : PriceDiscList.ChgAmt5.Value;
                Model2.DiscCharge6 = PriceDiscList.ChgPer6 == null ? 0 : PriceDiscList.ChgPer6.Value;
                Model2.DiscChargeAmt6 = PriceDiscList.ChgAmt6 == null ? 0 : PriceDiscList.ChgAmt6.Value;
                Model2.Code = PriceDiscList.Code;
                Model2.Account = PriceDiscList.Party;
                Model2.DocDate = PriceDiscList.DocDate;
                Model2.PriceDiscCode = PriceDiscList.PriceListDisc;
            }
          
            if (allowdisccharges == true)
            {
                //var PriceDiscList2 = ctxTFAT.PriceListsDisc.Where(x => x.Code == Model2.PriceDiscCode).Select(x => x).FirstOrDefault();
                //if (PriceDiscList2 != null)
                //{
                //    for (int i = 1; i <= 6; i++)
                //    {
                //        PriceListVM pd = new PriceListVM();
                //        switch (i)
                //        {
                //            case 1:
                //                pd.Disc = Convert.ToDouble(Model2.DiscCharge1);
                //                pd.AddLess = (PriceDiscList2.DiscAddLess1 == "A") ? "+" : (PriceDiscList2.DiscAddLess1 == "L") ? "-" : "";
                //                pd.CalcOn = "";
                //                pd.DiscAmt = Convert.ToDouble(Model2.DiscChargeAmt1);
                //                pd.DiscCaption = (PriceDiscList2.DiscCaption1 == "" || PriceDiscList2.DiscCaption1 == null) ? "Charge 1" : PriceDiscList2.DiscCaption1;
                //                pd.PerValue = true;
                //                break;
                //            case 2:
                //                pd.Disc = Convert.ToDouble(Model2.DiscCharge2);
                //                pd.AddLess = (PriceDiscList2.DiscAddLess2 == "A") ? "+" : (PriceDiscList2.DiscAddLess2 == "L") ? "-" : "";
                //                pd.CalcOn = PriceDiscList2.DiscApply2;
                //                pd.DiscAmt = Convert.ToDouble(Model2.DiscChargeAmt2);
                //                pd.DiscCaption = (PriceDiscList2.DiscCaption2 == "" || PriceDiscList2.DiscCaption2 == null) ? "Charge 2" : PriceDiscList2.DiscCaption2;
                //                pd.PerValue = true;
                //                break;
                //            case 3:
                //                pd.Disc = Convert.ToDouble(Model2.DiscCharge3);
                //                pd.AddLess = (PriceDiscList2.DiscAddLess3 == "A") ? "+" : (PriceDiscList2.DiscAddLess3 == "L") ? "-" : "";
                //                pd.CalcOn = PriceDiscList2.DiscApply3;
                //                pd.DiscAmt = Convert.ToDouble(Model2.DiscChargeAmt3);
                //                pd.DiscCaption = (PriceDiscList2.DiscCaption3 == "" || PriceDiscList2.DiscCaption3 == null) ? "Charge 3" : PriceDiscList2.DiscCaption3;
                //                pd.PerValue = true;
                //                break;
                //            case 4:
                //                pd.Disc = Convert.ToDouble(Model2.DiscCharge4);
                //                pd.AddLess = (PriceDiscList2.DiscAddLess4 == "A") ? "+" : (PriceDiscList2.DiscAddLess4 == "L") ? "-" : "";
                //                pd.CalcOn = PriceDiscList2.DiscApply4;
                //                pd.DiscAmt = Convert.ToDouble(Model2.DiscChargeAmt4);
                //                pd.DiscCaption = (PriceDiscList2.DiscCaption4 == "" || PriceDiscList2.DiscCaption4 == null) ? "Charge 4" : PriceDiscList2.DiscCaption4;
                //                pd.PerValue = true;
                //                break;
                //            case 5:
                //                pd.Disc = Convert.ToDouble(Model2.DiscCharge5);
                //                pd.AddLess = (PriceDiscList2.DiscAddLess5 == "A") ? "+" : (PriceDiscList2.DiscAddLess5 == "L") ? "-" : "";
                //                pd.CalcOn = PriceDiscList2.DiscApply5;
                //                pd.DiscAmt = Convert.ToDouble(Model2.DiscChargeAmt5);
                //                pd.DiscCaption = (PriceDiscList2.DiscCaption5 == "" || PriceDiscList2.DiscCaption5 == null) ? "Charge 5" : PriceDiscList2.DiscCaption5;
                //                pd.PerValue = true;
                //                break;
                //            case 6:
                //                pd.Disc = Convert.ToDouble(Model2.DiscCharge6);
                //                pd.AddLess = "-";
                //                pd.CalcOn = "C";
                //                pd.DiscAmt = Convert.ToDouble(Model2.DiscChargeAmt6);
                //                pd.DiscCaption = "Scheme Discount 1";
                //                pd.PerValue = true;
                //                break;
                //        }
                //        pd.tempid = i;
                //        objitemlist.Add(pd);
                //    }
                //}

            }
            if (objitemlist == null || objitemlist.Count() == 0)
            {
                var ProductCharges = ctxTFAT.AddCharges.Where(x => x.Hide == false && x.Type == MainType.Substring(0, 1)).Select(x => x).ToList();
                int distemp = 1;
                foreach (var a in ProductCharges)
                {
                    double mdisc = 0;
                    double mdiscamt = 0;
                    if (distemp == 1)
                    {
                        mdisc = Convert.ToDouble(Model2.DiscCharge1);
                        mdiscamt = Convert.ToDouble(Model2.DiscChargeAmt1);
                    }
                    else if (distemp == 2)
                    {
                        mdisc = Convert.ToDouble(Model2.DiscCharge2);
                        mdiscamt = Convert.ToDouble(Model2.DiscChargeAmt2);
                    }
                    else if (distemp == 3)
                    {
                        mdisc = Convert.ToDouble(Model2.DiscCharge3);
                        mdiscamt = Convert.ToDouble(Model2.DiscChargeAmt3);
                    }
                    else if (distemp == 4)
                    {
                        mdisc = Convert.ToDouble(Model2.DiscCharge4);
                        mdiscamt = Convert.ToDouble(Model2.DiscChargeAmt4);
                    }
                    else if (distemp == 5)
                    {
                        mdisc = Convert.ToDouble(Model2.DiscCharge5);
                        mdiscamt = Convert.ToDouble(Model2.DiscChargeAmt5);
                    }
                    objitemlist.Add(new PriceListVM()
                    {
                        Disc = mdisc,
                        DiscAmt = mdiscamt,
                        AddLess = (a.CalOperater == "A") ? "+" : (a.CalOperater == "L") ? "-" : "",
                        CalcOn = "C",
                        DiscCaption = a.Name,
                        PerValue = a.PerOrValue,
                        tempid = distemp
                    });
                    distemp = distemp + 1;
                }
            }

            return objitemlist;
        }

       
        #endregion

        #region Pickup
        public List<PurchaseVM> Add2ItemForPickup(List<DataRow> ordersstk, string DiscAppl = "Y")
        {
            double PendFactor = 0;
            List<PurchaseVM> objitemlist = new List<PurchaseVM>();
            foreach (var item in ordersstk)
            {
                string munit = item["Unit"].ToString();
                int mqtydecimalpl = ctxTFAT.UnitMaster.Where(x => x.Code == munit).Select(x => (int?)x.NoOfDecimal).FirstOrDefault() ?? 0;
                if (mqtydecimalpl == 0)
                {
                    mqtydecimalpl = 2;
                }
                if (item["RateOn2"].ToString() == "1")
                {
                    if (Convert.ToDouble(item["Qty2"].ToString()) == 0)
                    {
                        item["Qty2"] = "1";
                    }
                    PendFactor = Convert.ToDouble(item["Pending2"].ToString()) / Convert.ToDouble(item["Qty2"].ToString());
                }
                else
                {
                    if (Convert.ToDouble(item["Qty"].ToString()) == 0)
                    {
                        item["Qty"] = "1";
                    }
                    PendFactor = Convert.ToDouble(item["Pending"].ToString()) / Convert.ToDouble(item["Qty"].ToString());
                }
                objitemlist.Add(new PurchaseVM()
                {
                    Code = item["Code"].ToString(),
                    Unit = item["Unit"].ToString(),
                    Pending = Math.Abs(Math.Round(Convert.ToDouble(item["Pending"].ToString()), mqtydecimalpl)),
                    Pending2 = Math.Abs(Math.Round(Convert.ToDouble(item["Pending2"].ToString()), mqtydecimalpl)),
                    Qty = Math.Abs(Convert.ToDouble(item["Qty"].ToString())),
                    Factor = Convert.ToDouble(item["Factor"].ToString()),
                    Qty2 = Math.Abs(Convert.ToDouble(item["Qty2"].ToString())),
                    Unit2 = item["Unit2"].ToString(),
                    RateOn2 = (item["RateOn2"].ToString() == "0" || item["RateOn2"].ToString() == "False" || item["RateOn2"].ToString() == "false") ? false : true,
                    Rate = Convert.ToDouble(item["Rate"].ToString()),
                    Disc = Math.Round(Math.Abs(Convert.ToDecimal(item["Disc"].ToString())), 2),
                    DiscAmt = Math.Round(Math.Abs(Convert.ToDecimal(item["DiscAmt"].ToString())) * Convert.ToDecimal(PendFactor), 2),
                    DiscPerAmt = Math.Round(Math.Abs(Convert.ToDecimal(item["DiscPerAmt"].ToString())) * Convert.ToDecimal(PendFactor), 2),
                    Taxable = Math.Round(Math.Abs(Convert.ToDecimal(item["Taxable"].ToString())) * Convert.ToDecimal(PendFactor), 2),
                    CGSTAmt = Math.Round(Math.Abs(Convert.ToDecimal(item["CGSTAmt"].ToString())) * Convert.ToDecimal(PendFactor), 2),
                    SGSTAmt = Math.Round(Math.Abs(Convert.ToDecimal(item["SGSTAmt"].ToString())) * Convert.ToDecimal(PendFactor), 2),
                    IGSTAmt = Math.Round(Math.Abs(Convert.ToDecimal(item["IGSTAmt"].ToString())) * Convert.ToDecimal(PendFactor), 2),
                    CVDAmt = Math.Round(Math.Abs(Convert.ToDecimal(item["CVDAmt"].ToString())) * Convert.ToDecimal(PendFactor), 2),
                    CVDCessAmt = Math.Round(Math.Abs(Convert.ToDecimal(item["CVDCessAmt"].ToString())) * Convert.ToDecimal(PendFactor), 2),
                    CVDExtra = Math.Round(Math.Abs(Convert.ToDecimal(item["CVDExtra"].ToString())) * Convert.ToDecimal(PendFactor), 2),
                    CVDSCessAmt = Math.Round(Math.Abs(Convert.ToDecimal(item["CVDSCessAmt"].ToString())) * Convert.ToDecimal(PendFactor), 2),
                    Val1 = Math.Round(Math.Abs(Convert.ToDecimal(item["Val1"].ToString())) * Convert.ToDecimal(PendFactor), 2),
                    Narr = item["Narr"].ToString(),
                    HSN = item["HSN"].ToString(),
                    IGSTRate = Convert.ToDecimal(item["IGSTRate"].ToString()),
                    CGSTRate = Convert.ToDecimal(item["CGSTRate"].ToString()),
                    SGSTRate = Convert.ToDecimal(item["SGSTRate"].ToString()),
                    Store = Convert.ToInt32(item["Store"].ToString()),
                    ItemName = item["ItemName"].ToString(),
                    ParentKey = item["ParentKey"].ToString(),
                    TableKey = item["TableKey"].ToString(),
                    GSTCode = item["GSTCode"].ToString(),
                    MainType = item["MainType"].ToString(),
                    BinNumber = Convert.ToInt32(item["BinNumber"].ToString()),
                    SubType = item["SubType"].ToString(),
                    IndKey = item["IndKey"].ToString(),
                    EnqKey = item["EnqKey"].ToString(),
                    QtnKey = item["QtnKey"].ToString(),
                    OrdKey = item["OrdKey"].ToString(),
                    ChlnKey = item["ChlnKey"].ToString(),
                    InvKey = item["InvKey"].ToString(),
                    PKSKey = item["PKSKey"].ToString(),
                    Stage = Convert.ToInt32(item["Stage"].ToString()),
                    RateType = item["RateType"].ToString(),
                    RateCalcType = item["RateCalcType"].ToString(),
                    StageName = item["Stage"].ToString(),//
                    BillDate = Convert.ToDateTime(string.IsNullOrEmpty(item["BillDate"].ToString()) ? "1900-01-01" : item["BillDate"].ToString()),
                    DocDate = Convert.ToDateTime(string.IsNullOrEmpty(item["DocDate"].ToString()) ? "1900-01-01" : item["DocDate"].ToString()),
                    PendingFactor = PendFactor,
                    BillNumber = item["BillNumber"].ToString(),
                    SrNo = objitemlist.Count + 1,
                    tempId = objitemlist.Count + 1,
                    PriceDiscCode = item["PriceDiscCode"].ToString(),
                    FreeQty = (item["FreeQty"].ToString() == "0" || item["FreeQty"].ToString() == "False" || item["FreeQty"].ToString() == "false") ? false : true,
                    ClassValues1 = item["ClassValues1"].ToString(),
                    ClassValues2 = item["ClassValues2"].ToString(),
                    PriceRateCode = item["PriceRateCode"].ToString(),
                    Type = item["TableKey"].ToString().Substring(0, 5),
                    Sno = Convert.ToInt32(item["TableKey"].ToString().Substring(5, 3)),
                    Srl = item["TableKey"].ToString().Substring(8),
                    ProjCode = item["ProjCode"].ToString(),
                    tempIsDeleted = false
                });
            }

            return objitemlist;
        }

        public ActionResult GetPickUp(PurchaseVM Model)
        {
            string mLink = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(z => z.OrdTypes.Trim()).FirstOrDefault();
            string mstr = "";
            string mstr2 = "";
            string mstr3 = "";
            string mtable = Model.MainType == "PR" ? "Purchase" : "Sales";
            if (mLink == null)
                mLink = "";
            bool gp_OrdIncludeRet = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(z => z.gp_OrdIncludeRet).FirstOrDefault();

            // common for all sources
            mstr2 = " Rate=r.Rate,Code = r.Code,ItemName = (select top 1 name from ItemMaster where code = r.Code),Unit = r.Unit,Qty=Abs(r.Qty),Qty2 = Abs(r.Qty2),Unit2 = r.Unit2,RateOn2 = r.RateOn," +
            " Sno = r.Sno,Factor = r.Factor,Disc = isnull(r.Disc,0),DiscAmt = isnull(r.DiscAmt,0),SGSTAmt = isnull(r.SGSTAmt,0),SGSTRate = isnull(r.SGSTRate, 0),CGSTAmt = isnull(r.cGSTAmt, 0),CGSTRate = isnull(r.cGSTRate, 0),IGSTAmt = isnull(r.iGSTAmt, 0),IGSTRate = isnull(r.iGSTRate, 0)," +
            " CVDAmt = isnull(r.CVDAmt, 0),CVDCessAmt = isnull(r.CVDCessAmt, 0),CVDExtra = isnull(r.CVDExtra, 0),CVDSCessAmt = isnull(r.CVDSCessAmt,0),Narr = r.Narr,HSN = r.HSNCode," +
            " Store = r.Store,MainType = r.MainType,SubType = r.SubType,Type = r.Type,RECORDKEY = r.RECORDKEY,Srl = r.Srl,Prefix = r.Prefix,TableKey = r.TableKey,ParentKey = r.ParentKey,Taxable = isnull(r.Taxable,0)," +
            " Val1 = r.Amt,GSTCode=r.TaxCode,BinNumber = 0,DiscPerAmt = r.Discount - r.DiscAmt,IndKey = isnull(r.IndKey,''),EnqKey = isnull(r.EnqKey,''),QtnKey =  isnull(r.QtnKey,''),OrdKey =  isnull(r.OrdKey,''),ChlnKey =  isnull(r.ChlnKey,''),InvKey =  isnull(r.InvKey,''),PKSKey =  isnull(r.PKSKey,''),PIKKey =  isnull(r.PIKKey,'')," +
            " RateType = r.RateType,RateCalcType = r.RateCalcType," +
            " PriceDiscCode = r.PriceListDisc,FreeQty = r.FreeQty,ClassValues1 = r.xValue1,ClassValues2 = r.xValue2,PriceRateCode = r.PriceListRate,Stage= 0";


            #region souce orders
            if (Model.SourceDoc == "Orders")
            {
                mstr2 = mstr2.Replace("r.ChlnKey", "''");
                mstr2 = mstr2.Replace("r.PKSKey", "''");

                mstr = "Select Pending = abs(r.Qty) - abs(isnull((Select sum(Qty) from OtherTable Where OtherTable.OrdKey = r.TableKey),0))," +
                   " Pending2 = abs(r.Qty2) - abs(isnull((Select sum(Qty2) from OtherTable Where OtherTable.OrdKey = r.TableKey),0))," +
                   " BillDate = (Select top 1 BillDate from Orders Where Orders.TableKey = r.ParentKey)," +
                    " BillNumber = (Select top 1 BillNumber from Orders Where Orders.TableKey = r.ParentKey),r.DocDate,ProjCode = (Select isnull(Name,'') from ProjectDetails Where ProjectDetails.code = (select ind.ProjCode from Orders ind where ind.TableKey = r.ParentKey)),";
                mstr3 = " from OrdersStk r where " +
                (string.IsNullOrEmpty(mLink) ? "" : "Charindex(r.Type,'" + mLink + "')<>0 and ") +
                " r.branch='" + mbranchcode + "' and r.SubType='O" + Model.MainType.Substring(0, 1) + "'" + " and r.Party='" + Model.Code + "' and r.LocationCode=" + Model.LocationCode + " and left(r.AUTHORISE,1)='A' and r.Status<>'C' and r.Status<>'F'" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(r.Type,'" + mDocString + "')=0") +
                " And (abs(r.Qty) -  abs(isnull((Select sum(Qty) from OtherTable Where OtherTable.OrdKey = r.TableKey),0))) >0 Order by r.docDate,r.Sno";

               
                 if (Model.SubType == "OS" || Model.SubType == "OP")
                {
                    mstr = mstr.Replace("OtherTable", "OrdersStk");
                    mstr3 = mstr3.Replace("OtherTable", "OrdersStk");
                    List<DataRow> ordersstk = GetDataTable(mstr + mstr2 + mstr3).AsEnumerable().ToList();
                    Model.NewItemList = Add2ItemForPickup(ordersstk);
                }
                else if (Model.SubType == "PK")
                {
                    mstr = mstr.Replace("OtherTable", "PackingListStk");
                    mstr3 = mstr3.Replace("OtherTable", "PackingListStk");
                    List<DataRow> ordersstk = GetDataTable(mstr + mstr2 + mstr3).AsEnumerable().ToList();
                    Model.NewItemList = Add2ItemForPickup(ordersstk);
                }
                else if (Model.SubType == "PL")
                {
                    mstr = mstr.Replace("OtherTable", "OrdersStk");
                    mstr = mstr.Replace("OrdKey", "PIKKey");
                    mstr3 = mstr3.Replace("OtherTable", "OrdersStk");
                    mstr3 = mstr3.Replace("OrdKey", "PIKKey");
                    List<DataRow> ordersstk = GetDataTable(mstr + mstr2 + mstr3).AsEnumerable().ToList();
                    Model.NewItemList = Add2ItemForPickup(ordersstk);
                }
                else if (Model.SubType == "GP")//no pending filter provide full 
                {
                    mstr = mstr.Replace("OtherTable", "OrdersStk");
                    mstr3 = mstr3.Replace(" And (abs(r.Qty) -  abs(isnull((Select sum(Qty) from OtherTable Where OtherTable.OrdKey = r.TableKey),0))) >0", "");
                    mstr3 = mstr3.Replace("OtherTable", "OrdersStk");

                    List<DataRow> ordersstk = GetDataTable(mstr + mstr2 + mstr3).AsEnumerable().ToList();
                    Model.NewItemList = Add2ItemForPickup(ordersstk);
                }
                else
                {
                    if (gp_OrdIncludeRet == false)
                    {
                        mstr = mstr.Replace("OtherTable.OrdKey = r.TableKey", "Stock.OrdKey = r.TableKey and Stock.NotInStock = 0");
                        mstr = mstr.Replace("OtherTable", "Stock");
                        mstr3 = mstr3.Replace("OtherTable.OrdKey = r.TableKey", "Stock.OrdKey = r.TableKey and Stock.NotInStock = 0");
                        mstr3 = mstr3.Replace("OtherTable", "Stock");
                        List<DataRow> ordersstk = GetDataTable(mstr + mstr2 + mstr3).AsEnumerable().ToList();
                        Model.NewItemList = Add2ItemForPickup(ordersstk);
                    }
                    else
                    {
                        mstr = mstr.Replace("OtherTable.OrdKey = r.TableKey", "Stock.OrdKey = r.TableKey and NotInStock = 0 and Subtype NOT IN ('NS','SX','NP','PX')");
                        mstr = mstr.Replace("OtherTable", "Stock");
                        mstr3 = mstr3.Replace("OtherTable.OrdKey = r.TableKey", "Stock.OrdKey = r.TableKey and NotInStock = 0 and Subtype NOT IN ('NS','SX','NP','PX')");
                        mstr3 = mstr3.Replace("OtherTable", "Stock");
                        List<DataRow> ordersstk = GetDataTable(mstr + mstr2 + mstr3).AsEnumerable().ToList();
                        Model.NewItemList = Add2ItemForPickup(ordersstk);
                    }
                }


            }
            #endregion souce orders

            #region souce invoice
            else if (Model.SourceDoc == "Invoice")
            {
                if (Model.SubType == "NP" || Model.SubType == "PX" || Model.SubType == "SX" || Model.SubType == "NS" || Model.SubType == "RP" || Model.SubType == "RS")
                {
                    mstr = "Select Pending = abs(r.Qty) - abs(isnull((Select sum(Qty) from Stock Where Stock.InvKey=r.TableKey),0))," +
                    " Pending2 = abs(r.Qty2) - abs(isnull((Select sum(Qty2) from Stock Where Stock.InvKey=r.TableKey),0))," +
                    " BillDate = (Select top 1 BillDate from " + mtable + " Where " + mtable + ".TableKey = r.ParentKey)," +
                    " BillNumber = (Select top 1 BillNumber from " + mtable + " Where " + mtable + ".TableKey = r.ParentKey),r.DocDate,ProjCode = (Select isnull(Name,'') from ProjectDetails Where ProjectDetails.code = (select ind.ProjCode from  " + mtable + "more ind where ind.TableKey = r.ParentKey)),";
                    mstr3 = " from Stock r left join Stocktax st on r.TableKey=st.TableKey where " +
                    (string.IsNullOrEmpty(mLink) ? "" : "Charindex(r.Type,'" + mLink + "')<>0 and ") +
                    " r.branch='" + mbranchcode + "' and r.SubType='R" + Model.MainType.Substring(0, 1) + "'" + " and r.Party='" + Model.Code + "' and r.LocationCode=" + Model.LocationCode + " and left(r.AUTHORISE,1)='A'" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(r.Type,'" + mDocString + "')=0") +
                    " And (abs(r.Qty) - abs(isnull((Select sum(Qty) from Stock Where Stock.InvKey=r.TableKey),0)))>0 Order by r.docDate,r.Sno";
                    mstr2 = " Rate= " + ((Model.CurrConv == "Y") ? "r.cRate" : "r.Rate") + ",Code = r.Code,ItemName = (select top 1 name from ItemMaster where code = r.Code),Unit = r.Unit,Qty=Abs(r.Qty),Qty2 = Abs(r.Qty2),Unit2 = r.Unit2,RateOn2 = r.RateOn," +
                    " Sno = r.Sno,Factor = r.Factor,Disc = isnull(r.Disc,0),DiscAmt = isnull(r.DiscAmt,0),SGSTAmt = isnull(st.SGSTAmt,0),SGSTRate = isnull(st.SGSTRate, 0),CGSTAmt = isnull(st.cGSTAmt, 0),CGSTRate = isnull(st.cGSTRate, 0),IGSTAmt = isnull(st.iGSTAmt, 0),IGSTRate = isnull(st.iGSTRate, 0)," +
                    " CVDAmt = isnull(st.CVDAmt, 0),CVDCessAmt = isnull(st.CVDCessAmt, 0),CVDExtra = isnull(st.CVDExtra, 0),CVDSCessAmt = isnull(st.CVDSCessAmt,0),Narr = r.Narr,HSN = st.HSNCode," +
                    " Store = r.Store,MainType = r.MainType,SubType = r.SubType,Type = r.Type,RECORDKEY = r.RECORDKEY,Srl = r.Srl,Prefix = r.Prefix,TableKey = r.TableKey,ParentKey = r.ParentKey,Taxable = isnull(st.Taxable,0)," +
                    " Val1 = r.Amt,GSTCode = st.TaxCode,BinNumber = 0,DiscPerAmt = r.Discount - r.DiscAmt,IndKey = r.IndKey,EnqKey = '',QtnKey = r.QtnKey,OrdKey = r.OrdKey,ChlnKey = r.ChlnKey,InvKey = r.InvKey,PKSKey =r.PKSKey,PIKKey = ''," +
                    " RateType = r.RateType,RateCalcType = r.RateCalcType," +
                    " PriceDiscCode = r.PriceListDisc,FreeQty = r.FreeQty,ClassValues1 = r.xValue1,ClassValues2 = r.xValue2,PriceRateCode = r.PriceListRate,Stage= 0";
                    List<DataRow> ordersstk = GetDataTable(mstr + mstr2 + mstr3).AsEnumerable().ToList();
                    Model.NewItemList = Add2ItemForPickup(ordersstk);
                }
            }
            #endregion

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            var html = ViewHelper.RenderPartialView(this, "PickUp", new PurchaseVM() { NewItemList = Model.NewItemList, MileStoneReqd = Model.MileStoneReqd });
            var jsonResult = Json(new { NewItemList = Model.NewItemList, MileStoneReqd = Model.MileStoneReqd, Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public ActionResult PostPickUp(PurchaseVM Model)
        {
            try
            {
                List<PurchaseVM> objitemlist = new List<PurchaseVM>();
               
                List<string> mtablekeys = new List<string>();
               
                if (Session["NewItemlist"] != null)
                {
                    objitemlist = (List<PurchaseVM>)Session["NewItemlist"];
                }
                var result = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => new
                {
                    x.CurrConv,
                    x.NonStock,
                    x.QCReqd,

                }).FirstOrDefault();
                if (objitemlist.Count > 0) { mtablekeys = objitemlist.Select(x => x.TableKey).ToList(); }
                foreach (var p in Model.PickUpList)
                {
                    if (mtablekeys.Contains(p.TableKey) == true)
                    {
                        return Json(new { Status = "Duplicate", Message = " Cant Pickup Duplicate Key that is " + p.TableKey }, JsonRequestBehavior.AllowGet);
                    }
                }
                if (objitemlist.Count > 0) { mtablekeys = objitemlist.Select(x => x.Code).ToList(); }

                foreach (var p in Model.PickUpList)
                {
                    if (mtablekeys.Contains(p.Code) == true)
                    {
                        return Json(new { Status = "Duplicate", Message = " Cant Pickup Duplicate Product that is " + p.Code }, JsonRequestBehavior.AllowGet);
                    }
                }

                int mMaxtempid = (objitemlist.Count == 0) ? 0 : objitemlist.Select(x => x.tempId).Max();

                Model.CurrConv = result.CurrConv;
                Model.NonStock = result.NonStock;

                foreach (var a in Model.PickUpList.OrderBy(x => x.tempId))
                {
                    mMaxtempid = mMaxtempid + 1;
                    var DiscCharges = GetDiscountChgListInPickUp(a.TableKey, a.SubType, Model.MainType, a.Code, a.PendingFactor, Model.SubType, Model.NonStock);
                    var mCurrQuery = @"select CurrRate from " + GetTableName(a.SubType) + " where tablekey = '" + a.ParentKey + "'";
                    DataTable mCurrDt = GetDataTable(mCurrQuery);
                    decimal CurrRate = 1;
                    if (mCurrDt.Rows.Count > 0)
                    {
                        CurrRate = (string.IsNullOrEmpty(mCurrDt.Rows[0][0].ToString()) == false) ? Convert.ToDecimal(mCurrDt.Rows[0][0].ToString()) : 1;
                    }
                    if (CurrRate == 0)
                    {
                        CurrRate = 1;
                    }
                   
                    PurchaseVM c = new PurchaseVM();
                    PurchaseVM b = new PurchaseVM();
                    b = a;
                    b.DiscChargeList = DiscCharges;
                    b.DiscChargeAmt = DiscCharges.Sum(x => (decimal?)x.DiscAmt) ?? 0;
                    b.VATGSTApp = Model.VATGSTApp;
                    b.GSTType = Model.GSTType;
                    b.LocationCode = Model.LocationCode;
                    b.DelyCode = Model.DelyCode;
                    b.DelyAltAdd = Model.DelyAltAdd;
                    b.CurrRate = CurrRate;
                    b.SubType = Model.SubType;
                    b.PlaceOfSupply = Model.PlaceOfSupply;
                    b.IsGstDocType = Model.IsGstDocType;
                    c = ServerSideTaxCalculation(b);
                    var mItemDetQC = ctxTFAT.ItemDetail.Where(x => x.Code == c.Code && x.Branch == mbranchcode).Select(x => new { x.QCReqd }).FirstOrDefault();
                    bool mQcreqd = (mItemDetQC.QCReqd == true && result.QCReqd == true) ? true : false;
                    bool mqcdone = (mQcreqd == true) ? false : true;
                    objitemlist.Add(new PurchaseVM()
                    {
                        Code = c.Code,
                        ItemName = c.ItemName,
                        Unit = c.Unit,
                        Qty = c.Qty,
                        Factor = c.Factor,
                        Qty2 = c.Qty2,
                        Unit2 = c.Unit2,
                        RateOn2 = c.RateOn2,
                        Rate = c.Rate,
                        Disc = c.Disc,
                        DiscAmt = c.DiscAmt,
                        Taxable = c.Taxable,
                        CGSTAmt = c.CGSTAmt,
                        SGSTAmt = c.SGSTAmt,
                        IGSTAmt = c.IGSTAmt,
                        Val1 = c.Val1,
                        Narr = c.Narr,
                        HSN = Model.SourceDoc == "Indents" ? "HSN0" : c.HSN,
                        GSTCode = Model.SourceDoc == "Indents" ? "PGS0" : c.GSTCode,
                        IGSTRate = c.IGSTRate,
                        CGSTRate = c.CGSTRate,
                        SGSTRate = c.SGSTRate,
                        Store = c.Store,
                        DelyScheList = GetDeliveryScheduleList(c.ParentKey, c.TableKey, c.Store),
                        PAddOnList = GetPickPAddInEditView(Model.MainType, c.MainType, c.TableKey, Model.Type),
                        
                        MainType = c.MainType,
                        ParentKey = c.ParentKey,
                        TableKey = c.TableKey,
                        IndKey = (Model.SourceDoc == "Indents") ? c.TableKey : c.IndKey,
                        EnqKey = (Model.SourceDoc == "Enquiry" || Model.SourceDoc == "Sales Enquiry" || Model.SourceDoc == "Purchase Enquiry") ? c.TableKey : c.EnqKey,
                        QtnKey = (Model.SourceDoc == "Quotation" || Model.SourceDoc == "Sales Quotation" || Model.SourceDoc == "Purchase Quotation") ? c.TableKey : c.QtnKey,
                        OrdKey = (Model.SourceDoc == "Orders" || Model.SourceDoc == "Blanket Order" || Model.SourceDoc == "Sales Order" || Model.SourceDoc == "Purchase Orders") ? c.TableKey : c.OrdKey,
                        ChlnKey = (Model.SourceDoc == "Challans" || Model.SourceDoc == "Inter Branch Doc") ? c.TableKey : c.ChlnKey,
                        InvKey = (Model.SourceDoc == "Invoice" || Model.SourceDoc == "Performa Invoice") ? c.TableKey : c.InvKey,
                        PKSKey = (Model.SourceDoc == "PackingList" || Model.SourceDoc == "DespatchInstructions") ? c.TableKey : c.PKSKey,
                        IssueKey = (Model.SourceDoc == "Sub Contract") ? c.TableKey : c.IssueKey,
                        BinNumber = c.BinNumber,
                        IsPickUp = true,
                        IsBlanketOrder = (Model.SubType == c.SubType && (c.SubType == "OS" || c.SubType == "OP")),
                        SubType = c.SubType,
                        StoreName = GetStoreName(c.Store),
                        DiscPerAmt = c.DiscPerAmt,
                        //ItemType = ctxTFAT.ItemMaster.Where(x => x.Code == c.Code).Select(x => x.ItemType).FirstOrDefault(),
                        SrNo = mMaxtempid,
                        tempId = mMaxtempid,
                        RateType = c.RateType,
                        RateCalcType = c.RateCalcType,
                        DiscChargeList = c.DiscChargeList,
                        DiscChargeAmt = c.DiscChargeAmt,
                        PriceDiscCode = c.PriceDiscCode,
                        FreeQty = c.FreeQty,
                        ClassValues1 = c.ClassValues1,
                        ClassValues2 = c.ClassValues2,
                        PriceRateCode = c.PriceRateCode,
                        tempIsDeleted = false,
                        ItemSchemeCode = c.ItemSchemeCode,
                        QCReqd = mQcreqd,
                        QCDone = mqcdone
                    });
                }


                Model.ParentKey = Model.PickUpList.Select(x => x.ParentKey).FirstOrDefault();//picked up parentkey
                var ObjMainType = Model.PickUpList.Select(x => x.MainType).FirstOrDefault();//picked up maintype
                var ObjSubType = Model.PickUpList.Select(x => x.SubType).FirstOrDefault();//picked up subtype

                Session.Add("NewItemlist", objitemlist);
                if (Model.Mode == "Add")
                {
                  
                    if (Model.SourceDoc == "Orders" || Model.SourceDoc == "Purchase Orders" || Model.SourceDoc == "Sales Orders" || Model.SourceDoc == "Blanket Order")
                    {
                        var Order = ctxTFAT.Orders.Where(x => x.TableKey == Model.ParentKey).Select(x => x).FirstOrDefault();
                        Model.SalesmanCode = Order.Salesman == null ? "100001" : Order.Salesman.Value.ToString();
                        //Model.SalesmanName = GetSalesManName(Order.Salesman);
                        Model.SCommOn = Order.SalemanOn == null ? 0 : Order.SalemanOn.Value;
                        Model.SCommission = Order.SalemanPer == null ? 0 : Order.SalemanPer.Value;
                        Model.SAmt = Order.SalemanAmt == null ? 0 : Order.SalemanAmt.Value;

                        Model.Broker = Order.Broker == null ? 0 : Order.Broker.Value;
                        Model.BrokerOn = Order.BrokerOn == null ? 0 : Order.BrokerOn.Value;
                        Model.Brokerage = Order.Brokerage == null ? 0 : Order.Brokerage.Value;
                        Model.BrokerAmt = Order.BrokerAmt == null ? 0 : Order.BrokerAmt.Value;
                        Model.PayTerms = Order.PayTerms == null ? 0 : Order.PayTerms.Value;
                        Model.WONumber = Order.WONumber == null ? "" : Order.WONumber;
                        //Model.Reference = Order.BillNumber;
                        Model.IncoTerms = Order.IncoTerms == null ? 0 : Order.IncoTerms.Value;
                        Model.IncoPlace = Order.IncoPlace;
                        Model.InsuranceNo = Order.InsuranceNo;
                        if (Model.SourceDoc == "Orders" && Model.SubType == "OS")
                        {
                            Model.BillNumber = Model.ParentKey.Substring(0, 5) + Order.BillNumber;
                        }
                        else
                        {
                            Model.BillNumber = Order.BillNumber;
                        }
                        Model.OrdDate = Order.BillDate == null ? DateTime.Now.ToString("dd-MM-yyyy") : Order.BillDate.ToString("dd-MM-yyyy");
                        Model.LCNo = Order.LCNo;
                        Model.ProjCode = Order.ProjCode;
                        if (Model.CurrConv == "Y")
                        {
                            Model.CurrName = Order.CurrName;
                            Model.CurrRate = Order.CurrRate == 0 ? 1 : Order.CurrRate;
                        }
                        else
                        {
                            Model.CurrName = 1;
                            Model.CurrRate = 1;
                        }
                        Model.CrPeriod = Order.CrPeriod == null ? 0 : Order.CrPeriod.Value;
                        Model.Account = Order.Code;
                        Model.DelyCode = Order.DelyCode == null ? Order.Code : Order.DelyCode;
                        Model.AltAddress = Order.AltAddress == null ? 0 : Order.AltAddress.Value;
                        Model.DelyAltAdd = Order.DelyAltAdd == null ? 0 : Order.DelyAltAdd.Value;
                        Model.BillContact = Order.BillContact == null ? 0 : Order.BillContact.Value;
                        Model.DelyContact = Order.DelyContact == null ? 0 : Order.DelyContact.Value;
                        Model.PlaceOfSupply = Order.PlaceOfSupply;
                        Model.ShipFrom = Order.ShipFrom;
                        Model.ShipFromName = ctxTFAT.Master.Where(x => x.Code == Model.ShipFrom).Select(x => x.Name).FirstOrDefault();
                        if (Model.SubType == "RS" || Model.SubType == "CS" || Model.SubType == "RP" || Model.SubType == "CP")
                        {
                            var morderkeys = Model.PickUpList.Select(x => x.ParentKey).ToList();
                            if (Model.OSAdjList != null && Model.OSAdjList.Count > 0)
                            {
                                foreach (var okey in morderkeys)
                                {
                                    foreach (var osa in Model.OSAdjList)
                                    {
                                        var mrefdoc = ctxTFAT.Ledger.Where(x => x.Code == Order.Code && x.ParentKey == osa.ParentKey).Select(x => x.RefDoc).FirstOrDefault();
                                        if (string.IsNullOrEmpty(mrefdoc) == false)
                                        {
                                            var mrefdockeys = mrefdoc.Split(',');
                                            if (mrefdockeys.Contains(okey))
                                            {
                                                osa.OSAdjFlag = true;
                                                osa.AdjustAmt = osa.BalanceAmt;
                                            }
                                        }

                                    }
                                }
                            }
                        }
                    }
                   

                    //var Tobj = ctxTFAT.TransportDetail.Where(x => x.TableKey == Model.ParentKey).Select(x => x).FirstOrDefault();
                    //if (Tobj != null)
                    //{
                    //    Model.ContactPerson = Tobj.ContactPerson;
                    //    Model.NoPkg = Tobj.NoPkg;
                    //    Model.NoteNo = Tobj.NoteNo;
                    //    Model.Remark = Tobj.Remark;
                    //    Model.TransMode = Tobj.TransMode;
                    //    Model.Transporter = Tobj.TransporterCode;
                    //    Model.TransporterN = GetTransporterName(Tobj.Transporter);
                    //    Model.VehicleNo = Tobj.VehicleNo;
                    //    Model.DeliveryType = Tobj.DeliveryType == null ? (byte)0 : Convert.ToByte(Tobj.DeliveryType.Value);
                    //    Model.FreightType = Tobj.FreightType == null ? (byte)0 : Convert.ToByte(Tobj.FreightType.Value);
                    //    if (Model.SubType == "NP" || Model.SubType == "NS")//for leebo
                    //    {
                    //        Model.EWBDate = Convert.ToDateTime("1900-01-01");
                    //    }
                    //    else
                    //    {
                    //        Model.EwBillNo = Tobj.EWBNo;
                    //        Model.EWBDate = Tobj.EWBDate == null ? DateTime.Now : Tobj.EWBDate.Value;
                    //    }
                    //    Model.TrxWeight = Tobj.weight.Value;
                    //    Model.TransportType = Convert.ToInt16(Tobj.TransType);

                    //    Model.NoteDate = Model.SourceDoc == "Orders" ? DateTime.Now : Tobj.NoteDate == null ? DateTime.Now : Tobj.NoteDate.Value;
                    //}
                    //else
                    //{
                        Model.ContactPerson = "";
                        Model.NoPkg = "";
                        Model.NoteDate = DateTime.Now;
                        Model.NoteNo = "";
                        Model.Remark = "";
                        Model.TransMode = "";
                        Model.Transporter = "";
                        Model.TransporterN = "";
                        Model.VehicleNo = "";
                        Model.DeliveryType = (byte)0;
                        Model.FreightType = (byte)0;
                        Model.EwBillNo = "";
                        Model.TrxWeight = 0;
                        Model.TransportType = Convert.ToInt16(0);
                        Model.EWBDate = DateTime.Now;
                        Model.NoteDate = DateTime.Now;
                    //}

                    var mnarr = ctxTFAT.Narration.Where(x => x.ParentKey == Model.ParentKey).FirstOrDefault();
                    if (mnarr != null)
                    {
                        Model.RichNote = mnarr.NarrRich;
                    }
                    else
                    {
                        Model.RichNote = "";
                    }
                    #region TDS Payments
                    var mobtdstax = ctxTFAT.TDSPayments.Where(x => x.ParentKey == Model.ParentKey).FirstOrDefault();
                    if (mobtdstax != null && Model.MainType == "SL")
                    {
                        Model.TDSCode = mobtdstax.TDSCode == null ? 0 : mobtdstax.TDSCode.Value;
                        Model.TDSAble = mobtdstax.TDSAble == null ? 0 : mobtdstax.TDSAble.Value;
                        Model.TDSAmt = mobtdstax.TDSAmt == null ? 0 : mobtdstax.TDSAmt.Value;
                        Model.TDSCess = mobtdstax.TDSCessAmt == null ? 0 : mobtdstax.TDSCessAmt.Value;
                        Model.CutTDS = true;
                        Model.TDSReason = "";
                        Model.TDSSchg = mobtdstax.TDSSurChargeAmt == null ? 0 : mobtdstax.TDSSurChargeAmt.Value;
                        Model.TDSSHECess = mobtdstax.TDSSheCessAmt == null ? 0 : mobtdstax.TDSSheCessAmt.Value;

                    }
                    #endregion
                    #region Charge
                    List<PurchaseVM> objledgerdetail = new List<PurchaseVM>();
                    var trncharges = (from c in ctxTFAT.Charges.Where(x => x.Type == Model.Type && x.DontUse == false)
                                      join st in ctxTFAT.StockTax.Where(x => x.Branch == mbranchcode && x.ParentKey == Model.ParentKey && x.ItemType == "C") on c.Fld equals st.Code into g
                                      from gg in g.DefaultIfEmpty()
                                      select new { c.Fld, c.Head, c.EqAmt, c.Equation, c.Code, TaxAmt = (gg.SGSTAmt + gg.IGSTAmt + gg.CGSTAmt) }).ToList();
                    DataTable mdt = GetChargeValValue(ObjSubType, Model.ParentKey);
                    int mfld;
                    foreach (var i in trncharges)
                    {
                        PurchaseVM c = new PurchaseVM();
                        c.Fld = i.Fld;
                        c.Code = i.Head;
                        c.AddLess = i.EqAmt;
                        c.Equation = i.Equation;
                        c.tempId = Convert.ToInt16(i.Fld.Substring(1, 3));
                        if (mdt != null && mdt.Rows.Count > 0)
                        {

                            mfld = Convert.ToInt32(i.Fld.Substring(1));
                            if (ObjSubType != "IP")
                            {
                                c.ColVal = mdt.Rows[0]["Amt" + mfld].ToString();
                                c.ValueLast = mdt.Rows[0]["Val" + mfld].ToString();
                                c.TaxAmt = (i.TaxAmt == null) ? 0 : i.TaxAmt.Value;
                            }
                            else
                            {
                                c.ColVal = "0";
                                c.ValueLast = "0";
                            }

                        }
                        c.Amt1 = Convert.ToDecimal((c.ValueLast == "" || c.ValueLast == null) ? "0" : c.ValueLast);

                        c.ChgPostCode = i.Code;
                        objledgerdetail.Add(c);

                    }
                    Model.Charges = objledgerdetail;
                    #endregion
                    #region Doc
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
                    Session["TempPurSaleAttach"] = Model.DocumentList;
                    #endregion
                    #region Addon

                    List<AddOns> objitemlist2 = new List<AddOns>();
                    var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%Doc" + Model.MainType && x.Hide == false && x.Types.Contains(Model.Type))
                                  select i).OrderBy(x => new { x.Sno, x.Fld }).ToList();
                    var loginQuery3 = @"select * from addondoc" + ObjMainType + " where tablekey=" + "'" + Model.ParentKey + "'" + "";
                    DataTable mDt = GetDataTable(loginQuery3, GetConnectionString());
                    int t = 1;
                    int n = 0;
                    foreach (var i in addons)
                    {
                        AddOns c = new AddOns();
                        c.Fld = i.Fld;
                        c.Head = i.Head;
                        if (mDt != null && mDt.Rows.Count > 0)
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
                        objitemlist2.Add(c);
                        t = t + 1;
                        n = n + 1;
                    }
                    Model.AddOnList = objitemlist2;

                    string bca = "";
                    #endregion Addon
                    #region Terms And Condition
                    var Terms = ctxTFAT.TermsDetails.Where(x => x.ParentKey == Model.ParentKey).Select(x => x).ToList();
                    if (Terms != null)
                    {
                        List<PurchaseVM> objtermlist = new List<PurchaseVM>();
                        foreach (var a in Terms)
                        {
                            objtermlist.Add(new PurchaseVM()
                            {
                                TermId = a.TermsTitle,
                                TermName = a.TermsConditions,
                                tempId = objtermlist.Count + 1,
                                tempIsDeleted = false
                            });
                        }
                        Model.TermList = objtermlist;
                    }

                    #endregion
                  
                    #region Bill Address
                    string AccAddress = "";
                    string DlyAddress = "";
                    if (Model.SourceDoc != "Indents")
                    {
                        Address partyaddress = new Address();
                        Address delyaddress = new Address();
                        partyaddress = (from Add in ctxTFAT.Address where Add.Code == Model.Account && Add.Sno == Model.AltAddress select Add).FirstOrDefault();
                        delyaddress = (from Add in ctxTFAT.Address where Add.Code == Model.DelyCode && Add.Sno == Model.DelyAltAdd select Add).FirstOrDefault();

                        if (partyaddress.Adrl1 != null)
                        {
                            AccAddress = partyaddress.Adrl1;
                        }
                        if (partyaddress.Adrl2 != null && partyaddress.Adrl2 != "")
                        {
                            if (AccAddress != "")
                            {
                                AccAddress = AccAddress + ",\n" + partyaddress.Adrl2;
                            }
                            else
                            {
                                AccAddress = partyaddress.Adrl2;
                            }
                        }
                        if (partyaddress.Adrl3 != null && partyaddress.Adrl3 != "")
                        {
                            if (AccAddress != "")
                            {
                                AccAddress = AccAddress + ",\n" + partyaddress.Adrl3;
                            }
                            else
                            {
                                AccAddress = partyaddress.Adrl3;
                            }
                        }
                        if (partyaddress.Adrl4 != "" && partyaddress.Adrl4 != "")
                        {
                            if (AccAddress != "")
                            {
                                AccAddress = AccAddress + ",\n" + partyaddress.Adrl4;
                            }
                            else
                            {
                                AccAddress = partyaddress.Adrl4;
                            }
                        }
                        if (partyaddress.City != null && partyaddress.City != "")
                        {
                            if (AccAddress != "")
                            {
                                AccAddress = AccAddress + ",\n" + partyaddress.City + (partyaddress.Pin == null || partyaddress.Pin == "" ? "" : "-" + partyaddress.Pin);
                            }
                            else
                            {
                                AccAddress = partyaddress.City;
                            }
                        }
                        if (partyaddress.State != null && partyaddress.State != "")
                        {
                            if (AccAddress != "")
                            {
                                AccAddress = AccAddress + ",\n" + partyaddress.State;
                            }
                            else
                            {
                                AccAddress = partyaddress.State;
                            }
                        }
                        if (partyaddress.Country != null && partyaddress.Country != "")
                        {
                            if (AccAddress != "")
                            {
                                AccAddress = AccAddress + ",\n" + partyaddress.Country;
                            }
                            else
                            {
                                AccAddress = partyaddress.Country;
                            }

                        }
                        if (partyaddress.Mobile != null && partyaddress.Mobile != "")
                        {
                            if (AccAddress != "")
                            {
                                AccAddress = AccAddress + ",\n" + partyaddress.Mobile;
                            }
                            else
                            {
                                AccAddress = partyaddress.Mobile;
                            }

                        }
                        if (partyaddress.Email != null)
                        {
                            if (AccAddress != "")
                            {
                                AccAddress = AccAddress + ",\n" + partyaddress.Email;
                            }
                            else
                            {
                                AccAddress = partyaddress.Email;
                            }
                        }

                        if (delyaddress.Adrl1 != null)
                        {
                            DlyAddress = delyaddress.Adrl1;
                        }
                        if (delyaddress.Adrl2 != null && delyaddress.Adrl2 != "")
                        {
                            if (DlyAddress != "")
                            {
                                DlyAddress = DlyAddress + ",\n" + delyaddress.Adrl2;
                            }
                            else
                            {
                                DlyAddress = delyaddress.Adrl2;
                            }
                        }
                        if (delyaddress.Adrl3 != null && delyaddress.Adrl3 != "")
                        {
                            if (DlyAddress != "")
                            {
                                DlyAddress = DlyAddress + ",\n" + delyaddress.Adrl3;
                            }
                            else
                            {
                                DlyAddress = delyaddress.Adrl3;
                            }
                        }
                        if (delyaddress.Adrl4 != "" && delyaddress.Adrl4 != "")
                        {
                            if (DlyAddress != "")
                            {
                                DlyAddress = DlyAddress + ",\n" + delyaddress.Adrl4;
                            }
                            else
                            {
                                DlyAddress = delyaddress.Adrl4;
                            }
                        }
                        if (delyaddress.City != null && delyaddress.City != "")
                        {
                            if (DlyAddress != "")
                            {
                                DlyAddress = DlyAddress + ",\n" + delyaddress.City + (delyaddress.Pin == null || delyaddress.Pin == "" ? "" : "-" + delyaddress.Pin);
                            }
                            else
                            {
                                DlyAddress = delyaddress.City;
                            }
                        }
                        if (delyaddress.State != null && delyaddress.State != "")
                        {
                            if (DlyAddress != "")
                            {
                                DlyAddress = DlyAddress + ",\n" + delyaddress.State;
                            }
                            else
                            {
                                DlyAddress = delyaddress.State;
                            }
                        }
                        if (delyaddress.Country != null && delyaddress.Country != "")
                        {
                            if (DlyAddress != "")
                            {
                                DlyAddress = DlyAddress + ",\n" + delyaddress.Country;
                            }
                            else
                            {
                                DlyAddress = delyaddress.Country;
                            }

                        }
                        if (delyaddress.Mobile != null && delyaddress.Mobile != "")
                        {
                            if (AccAddress != "")
                            {
                                AccAddress = AccAddress + ",\n" + delyaddress.Mobile;
                            }
                            else
                            {
                                AccAddress = delyaddress.Mobile;
                            }

                        }
                        if (delyaddress.Email != null)
                        {
                            if (DlyAddress != "")
                            {
                                DlyAddress = DlyAddress + ",\n" + delyaddress.Email;
                            }
                            else
                            {
                                DlyAddress = delyaddress.Email;
                            }
                        }
                        Model.AddressFrom = AccAddress;
                        Model.AddressTo = DlyAddress;
                        Model.DisplayGSTId = partyaddress.GSTNo;
                        Model.DisplayGSTId2 = delyaddress.GSTNo;
                        Model.AccountName = (ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault().ToString());
                        Model.DelyName = (ctxTFAT.Master.Where(x => x.Code == Model.DelyCode).Select(x => x.Name).FirstOrDefault().ToString());
                        var mastinfo = ctxTFAT.MasterInfo.Where(x => x.Code == Model.Account).Select(x => x.CrLimit).FirstOrDefault();
                        Model.CrLimit = mastinfo == null ? 0 : mastinfo.Value;
                        //Model.CrPeriod = mastinfo == null ? 0 : (mastinfo.CrPeriod == null) ? 0 : mastinfo.CrPeriod.Value;//since it shold
                        Model.Balance = Convert.ToDouble(GetBalance(Model.Account, DateTime.Now, mbranchcode));
                        Model.PlaceOfSupply = Model.PlaceOfSupply;
                    }
                    #endregion
                }
                Model.NewItemList = objitemlist;
                Model.TotalQty = objitemlist.Where(x => x.tempIsDeleted == false).Select(x => x.Qty).Sum();
                Model.Taxable = objitemlist.Where(x => x.tempIsDeleted == false).Select(x => x.Taxable).Sum();
                Model.Val1 = objitemlist.Where(x => x.tempIsDeleted == false).Select(x => x.Val1).Sum();
                Model.SGSTAmt = objitemlist.Where(x => x.tempIsDeleted == false).Select(x => x.SGSTAmt).Sum();
                Model.CGSTAmt = objitemlist.Where(x => x.tempIsDeleted == false).Select(x => x.CGSTAmt).Sum();
                Model.IGSTAmt = objitemlist.Where(x => x.tempIsDeleted == false).Select(x => x.IGSTAmt).Sum();
                Model.SubType = Model.SubType;
                Model.LocationCode = Model.LocationCode;
                string html = "";
                string saleshtml = "";
                if (Model.Mode == "Add")
                {
                    html = ViewHelper.RenderPartialView(this, "ItemChargeMoreView", Model);
                    Model.Mode = "Edit";
                    if (Model.SourceDoc != "Indents")
                    {
                        saleshtml = ViewHelper.RenderPartialView(this, "Sales", Model);
                    }
                }
                else
                {
                    html = ViewHelper.RenderPartialView(this, "ItemList", Model);
                }
                var jsonResult = Json(new
                {
                    TotalQty = objitemlist.Where(x => x.tempIsDeleted == false).Select(x => x.Qty).Sum(),
                    NewItemList = objitemlist.Where(x => x.tempIsDeleted == false).Select(x => x).ToList(),
                    Taxable = objitemlist.Where(x => x.tempIsDeleted == false).Select(x => x.Taxable).Sum(),
                    Val1 = objitemlist.Where(x => x.tempIsDeleted == false).Select(x => x.Val1).Sum(),
                    SGSTAmt = objitemlist.Where(x => x.tempIsDeleted == false).Select(x => x.SGSTAmt).Sum(),
                    CGSTAmt = objitemlist.Where(x => x.tempIsDeleted == false).Select(x => x.CGSTAmt).Sum(),
                    IGSTAmt = objitemlist.Where(x => x.tempIsDeleted == false).Select(x => x.IGSTAmt).Sum(),
                    SubType = Model.SubType,
                    BillNumber = Model.BillNumber,
                    OrdDate = Model.OrdDate,
                    SourceDoc = Model.SourceDoc,
                    CurrRate = Model.CurrRate,
                    CurrName = Model.CurrName,
                    Status = "Success",
                    MLSPercent = Model.MLSPercent,
                    CrLimit = Model.CrLimit,
                    Html = html,
                    SalesHtml = saleshtml
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
        #endregion

        #region Terms And Condition
        public ActionResult AddTermlist(PurchaseVM Model)
        {


            List<PurchaseVM> objtermlist = new List<PurchaseVM>();
            List<PurchaseVM> objtermlist2 = new List<PurchaseVM>();

            objtermlist2 = (Model.TermList == null) ? objtermlist2 : Model.TermList;
            int Maxtempid = (objtermlist2.Count == 0) ? 0 : objtermlist2.Select(x => x.tempId).Max();
            objtermlist.AddRange(objtermlist2);
            //if (Model.TermList != null && Model.TermList.Count > 0)
            //{
            //    objtermlist.AddRange(Model.TermList);
            //}

            objtermlist.Add(new PurchaseVM()
            {
                TermId = Model.TermId,
                TermName = Model.TermName,
                tempId = Maxtempid + 1,
                tempIsDeleted = false
            });
            var html = ViewHelper.RenderPartialView(this, "TermList", new PurchaseVM() { TermList = objtermlist });
            return Json(new
            {
                TermList = objtermlist,
                Html = html
            }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult AddTermlistByTemplate(PurchaseVM Model)
        {
            List<PurchaseVM> objtermlist = new List<PurchaseVM>();
            var templist = ctxTFAT.TermCondsTemplate.Where(x => x.Code == Model.TermTemplateId).Select(x => x).ToList();
            foreach (var a in templist)
            {
                var temTerm = Convert.ToInt32(a.TermsCode);
                objtermlist.Add(new PurchaseVM()
                {
                    TermId = ctxTFAT.TermsConditions.Where(x => x.Code == temTerm).Select(x => x.Name).FirstOrDefault(),
                    TermName = ctxTFAT.TermsConditions.Where(x => x.Code == temTerm).Select(x => x.Terms).FirstOrDefault(),
                    tempId = objtermlist.Count + 1,
                    tempIsDeleted = false
                });
            }


            var html = ViewHelper.RenderPartialView(this, "TermList", new PurchaseVM() { TermList = objtermlist });
            return Json(new
            {
                TermList = objtermlist,
                Html = html
            }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult DeleteTermlist(PurchaseVM Model)
        {

            var objtermlist = Model.TermList.Where(x => x.tempId != Model.tempId).Select(x => x).ToList();
            string html;
            html = ViewHelper.RenderPartialView(this, "TermList", new PurchaseVM() { TermList = objtermlist });
            return Json(new
            {
                Html = html,
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
        public ActionResult AttachDocument(HttpPostedFileBase files, string DocumentStr, string Mode)
        {
            string XYZ = "";
            string docstr = "";
            string FLN = "";
            int n = 0;
            List<PurchaseVM> DocList = new List<PurchaseVM>();
            if (Session["TempPurSaleAttach"] != null)
            {
                DocList = (List<PurchaseVM>)Session["TempPurSaleAttach"];
            }
            if (DocList != null && DocList.Count() > 0)
            {
                n = DocList.Select(x => x.tempId).Max();
            }



            byte[] fileData = null;

            for (int i = 0; i < Request.Files.Count; i++)
            {

                PurchaseVM Model = new PurchaseVM();

                var file = Request.Files[i];
                var fileName = Path.GetFileName(file.FileName);

                MemoryStream target = new MemoryStream();
                using (var binaryReader = new BinaryReader(Request.Files[i].InputStream))
                {
                    fileData = binaryReader.ReadBytes(Request.Files[i].ContentLength);
                }
                if (DocList.Select(x => x.FileName).Contains(fileName))
                {
                    return Json(new { Message = "File with same Name Already Uploaded Cant Upload", Status = "CancelError" }, JsonRequestBehavior.AllowGet);
                }

                Model.ImageData = fileData;

                string FileString = Convert.ToBase64String(fileData);
                file.InputStream.CopyTo(target);

                Model.FileContent = file.ContentType;
                Model.ImageStr = FileString;

                Model.FileName = fileName;
                Model.ContentType = file.ContentType;

                Model.tempId = n + 1;
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
            Session["TempPurSaleAttach"] = DocList;
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/Purchase/AttachmentDocument.cshtml", new PurchaseVM() { DocumentList = DocList, AllFileStr = XYZ, FileNameStr = FLN, Mode = Mode });
            var jsonResult = Json(new { DocumentList = DocList, AllFileStr = XYZ, FileNameStr = FLN, Html = html, Mode = Mode }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        //[HttpPost]
        //public ActionResult EditImage(HttpPostedFileBase files, string Document)
        //{
        //    string XYZ = "";

        //    string FLN = "";
        //    List<PurchaseVM> DocList = new List<PurchaseVM>();
        //    if (Session["TempPurSaleAttach"] != null)
        //    {
        //        DocList = (List<PurchaseVM>)Session["TempPurSaleAttach"];
        //    }
        //    int n = DocList.Count() + 1;

        //    byte[] fileData = null;
        //    string docstr = "";

        //    for (int i = 0; i < Request.Files.Count; i++)
        //    {
        //        PurchaseVM Model = new PurchaseVM();

        //        var file = Request.Files[i];
        //        var fileName = Path.GetFileName(file.FileName);
        //        MemoryStream target = new MemoryStream();
        //        using (var binaryReader = new BinaryReader(Request.Files[i].InputStream))
        //        {
        //            fileData = binaryReader.ReadBytes(Request.Files[i].ContentLength);
        //        }
        //        Model.ImageData = fileData;
        //        string FileString = Convert.ToBase64String(fileData);
        //        file.InputStream.CopyTo(target);
        //        Model.FileContent = file.ContentType;
        //        Model.ImageStr = FileString;
        //        Model.FileName = fileName;
        //        Model.ContentType = file.ContentType;
        //        Model.tempId = n;
        //        DocList.Add(Model);
        //        n = n + 1;

        //    }
        //    if (DocList.Count > 0)
        //    {
        //        foreach (var a in DocList.Where(x => x.tempIsDeleted == false))
        //        {
        //            docstr = docstr + a.ImageStr + ",";
        //        }
        //        if (docstr != "")
        //        {
        //            docstr = docstr.Remove(docstr.Length - 1);
        //        }
        //        string docfilnam = "";
        //        foreach (var b in DocList.Where(x => x.tempIsDeleted == false))
        //        {
        //            docfilnam = docfilnam + b.FileName + ",";
        //        }
        //        if (docfilnam != "")
        //        {
        //            docfilnam = docfilnam.Remove(docfilnam.Length - 1);
        //        }
        //        XYZ = docstr;
        //        FLN = docfilnam;
        //    }
        //    Session["TempPurSaleAttach"] = DocList;


        //    var html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/Purchase/AttachmentDocument.cshtml", new PurchaseVM() { DocumentList = DocList, AllFileStr = XYZ, FileNameStr = FLN, Mode = "Edit" });
        //    var jsonResult = Json(new { DocumentList = DocList, AllFileStr = XYZ, FileNameStr = FLN, Html = html, Mode = "Edit" }, JsonRequestBehavior.AllowGet);
        //    jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
        //    return jsonResult;
        //}
        [HttpPost]
        public ActionResult DeleteUploadFile(PurchaseVM Model)
        {
            string XYZ = "";

            string FLN = "";
            string docstr = "";
            List<PurchaseVM> DocList = new List<PurchaseVM>();
            if (Session["TempPurSaleAttach"] != null)
            {
                DocList = (List<PurchaseVM>)Session["TempPurSaleAttach"];
            }
            Model.DocumentList = DocList.Where(x => x.tempId != Model.tempId).Select(x => x).ToList();
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
            Session["TempPurSaleAttach"] = Model.DocumentList;

            var html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/Purchase/AttachmentDocument.cshtml", new PurchaseVM() { DocumentList = Model.DocumentList, AllFileStr = XYZ, FileNameStr = FLN, Mode = Model.Mode });
            var jsonResult = Json(new { DocumentList = Model.DocumentList, AllFileStr = XYZ, FileNameStr = FLN, Html = html, Mode = Model.Mode }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public FileResult Download(int tempId)
        {
            List<PurchaseVM> DocList = new List<PurchaseVM>();
            if (Session["TempPurSaleAttach"] != null)
            {
                DocList = (List<PurchaseVM>)Session["TempPurSaleAttach"];
            }

            var dwnfile = DocList.Where(x => x.tempId == tempId).FirstOrDefault();

            string filename = dwnfile.FileName;
            byte[] fileBytes = Convert.FromBase64String(dwnfile.ImageStr);

            return File(fileBytes, dwnfile.FileContent, filename);
        }

        public void SaveAttachment(PurchaseVM Model)
        {
            try
            {
                string attachmentPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentPath"].ToString()) == true ? "" : ConfigurationManager.AppSettings["AttachmentPath"].ToString();
                List<PurchaseVM> DocList = new List<PurchaseVM>();
                if (Session["TempPurSaleAttach"] != null)
                {
                    DocList = (List<PurchaseVM>)Session["TempPurSaleAttach"];
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
                        att.AUTHORISE = Model.AUTHORISE;
                        att.Branch = mbranchcode;
                        att.Code = "";
                        att.ENTEREDBY = muserid;
                        att.FilePath = directoryPath;
                        att.LASTUPDATEDATE = DateTime.Now;
                        att.LocationCode = Model.LocationCode;
                        att.Prefix = Model.Prefix;
                        att.Sno = an;
                        att.Srl = Model.Srl.ToString();;
                        att.SrNo = an;
                        att.TableKey = Model.Type + Model.Prefix.Substring(0, 2) + an.ToString("D3") + Model.Srl.ToString();;
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
            catch
            {

            }


        }

        public List<PurchaseVM> GetAttachmentListInEdit(PurchaseVM Model)
        {
            List<PurchaseVM> AttachmentList = new List<PurchaseVM>();
            try
            {
                var docdetail = ctxTFAT.Attachment.Where(x => x.ParentKey == Model.ParentKey).OrderBy(x => x.Sno).ToList();
                foreach (var item in docdetail)
                {
                    AttachmentList.Add(new PurchaseVM()
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
        #endregion

        #region MultiPrint

        [HttpPost]
        public ActionResult GetMultiPrint(PurchaseVM Model)
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
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Transactions/Views/Purchase/MultiPrint.cshtml", new PurchaseVM() { PrintGridList = Model.PrintGridList, Document = Model.Document });
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
        public List<LedgerVM> ShowAdjustListInSession(PurchaseVM Model)//while in session
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
        public ActionResult GetReferenceAdjustments(PurchaseVM Model)
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
            var html = ViewHelper.RenderPartialView(this, "ReferenceAdjustView", new PurchaseVM() { OSAdjList = OSAdjList });
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

        public ActionResult GetGSTCalculation(PurchaseVM Model)
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
                    return Json(Model, JsonRequestBehavior.AllowGet);
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

                    return Json(Model, JsonRequestBehavior.AllowGet);
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

                return Json(Model, JsonRequestBehavior.AllowGet);


            }
            else
            {
                Model.Inclusive = false;
                Model.IGSTRate = 0;
                Model.SGSTRate = 0;
                Model.CGSTRate = 0;
                return Json(Model, JsonRequestBehavior.AllowGet);
            }

        }

        public PurchaseVM GetGSTCalculationOnScan(PurchaseVM Model)
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

        private void SaveOutstandingDetails(PurchaseVM Model)
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
                        osobj1.Srl = Model.Srl.ToString();;
                        osobj1.Sno = mCnt;
                        osobj1.ParentKey = Model.ParentKey;
                        osobj1.TableKey = Model.Type + Model.Prefix.Substring(0, 2) + mCnt.ToString("D3") + Model.Srl.ToString();;
                        osobj1.aMaintype = item1.MainType;
                        osobj1.aSubType = item1.SubType;
                        osobj1.aType = item1.Type;
                        osobj1.aPrefix = item1.Prefix;
                        osobj1.aSrl = item1.Srl;
                        osobj1.aSno = item1.Sno;
                        osobj1.Amount = Convert.ToDecimal(item1.AdjustAmt);
                        osobj1.TableRefKey = item1.TableKey;
                        osobj1.AUTHIDS = muserid;
                        osobj1.AUTHORISE = Model.AUTHORISE;
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
                        osobj1.CurrName = Model.CurrName;
                        osobj1.CurrRate = Model.CurrRate;
                        osobj1.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
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
                        osobj2.aSrl = Model.Srl.ToString();;
                        osobj2.aSno = mCnt;
                        osobj2.ParentKey = Model.ParentKey;
                        osobj2.TableRefKey = Model.Type + Model.Prefix.Substring(0, 2) + mCnt.ToString("D3") + Model.Srl.ToString();;
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
                        osobj2.AUTHORISE = Model.AUTHORISE;
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
                        osobj2.CurrName = Model.CurrName;
                        osobj2.CurrRate = Model.CurrRate;
                        osobj2.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
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
                        osobj2.CurrAmount = item1.AdjustAmt;
                        osobj2.ValueDate = DateTime.Now;
                        osobj2.LocationCode = Model.LocationCode;

                        ctxTFAT.Outstanding.Add(osobj2);

                    }
                    mCnt = mCnt + 1;
                }
            }
        }

        public void SaveItemAddons(List<AddOns> AddOnList, int mSno, string Code, PurchaseVM Model, string TableKey)
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

        private void SaveAddons(PurchaseVM Model)
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

        private void SaveTermsCondition(PurchaseVM Model)
        {
            string addo1, addo2;
            if (Model.TermList != null && Model.TermList.Count > 0)
            {
                var addoni = ctxTFAT.TermsDetails.Where(x => x.TableKey == Model.ParentKey).Select(x => x).ToList();
                if (addoni != null && addoni.Count < 1)
                {
                    ctxTFAT.TermsDetails.RemoveRange(addoni);
                }
                int mCnt = 1;
                foreach (var a in Model.TermList)
                {
                    TermsDetails aip = new TermsDetails();
                    aip.AUTHIDS = muserid;
                    aip.AUTHORISE = "A00";
                    aip.Branch = mbranchcode;
                    aip.CompCode = mcompcode;
                    aip.LocationCode = Model.LocationCode;
                    aip.ProcessCode = 100001;
                    aip.Sno = mCnt;
                    aip.ENTEREDBY = muserid;
                    aip.LASTUPDATEDATE = DateTime.Now;
                    aip.Srl = Model.Srl.ToString();;
                    aip.SubType = Model.SubType;
                    aip.TermsConditions = a.TermName;
                    aip.TermsTitle = a.TermId;
                    aip.TableKey = Model.Type + Model.Prefix.Substring(0, 2) + mCnt.ToString("D3") + Model.Srl.ToString();;
                    aip.ParentKey = Model.ParentKey;
                    aip.Type = Model.Type;
                    aip.MainType = Model.MainType;
                    aip.Prefix = Model.Prefix;
                    ctxTFAT.TermsDetails.Add(aip);
                    mCnt = mCnt + 1;
                }
            }
        }

        private void SaveTransportDetails(PurchaseVM Model, string ParentKey)
        {
            //TransportDetail Tobj = new TransportDetail();
            //Tobj.AUTHIDS = muserid;
            //Tobj.AUTHORISE = Model.AUTHORISE;
            //Tobj.Branch = mbranchcode;
            //Tobj.ContactPerson = (Model.ContactPerson == "" || Model.ContactPerson == null) ? "" : Model.ContactPerson;
            //Tobj.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            //Tobj.LocationCode = Model.LocationCode;
            //Tobj.MainType = Model.MainType;
            //Tobj.NoPkg = (Model.NoPkg == "" || Model.NoPkg == null) ? "" : Model.NoPkg;
            //Tobj.BillDate = ConvertDDMMYYTOYYMMDD(Model.OrdDate);
            //Tobj.NoteNo = (Model.NoteNo == "" || Model.NoteNo == null) ? "" : Model.NoteNo;
            //Tobj.Prefix = mperiod;
            //Tobj.Remark = (Model.Remark == "" || Model.Remark == null) ? "" : Model.Remark;
            //Tobj.Srl = Model.Srl.ToString();;
            //Tobj.SubType = Model.SubType;
            //Tobj.TransMode = (Model.TransMode == "" || Model.TransMode == null) ? "" : Model.TransMode;
            //Tobj.Transporter = 0;//Changed now getting id from master
            //Tobj.Type = Model.Type;
            //Tobj.VehicleNo = (Model.VehicleNo == "" || Model.VehicleNo == null) ? "" : Model.VehicleNo;
            //Tobj.ENTEREDBY = muserid;
            //Tobj.LASTUPDATEDATE = DateTime.Now;
            //Tobj.DeliveryType = Model.DeliveryType;
            //Tobj.FreightType = Model.FreightType;
            //Tobj.weight = Model.TrxWeight;
            //Tobj.EWBNo = Model.EwBillNo;
            //Tobj.EWBDate = (Model.StrEWBDate == "01-01-0001" || Model.StrEWBDate == "" || Model.StrEWBDate == null) ? DateTime.Now.Date : ConvertDDMMYYTOYYMMDD(Model.StrEWBDate);
            //Tobj.TransType = Convert.ToByte(Model.TransportType);
            //Tobj.CompCode = mcompcode;
            //Tobj.TableKey = ParentKey;
            //Tobj.NoteDate = (Model.NotuDate == null || Model.NotuDate == "" || Model.NotuDate == "01-01-0001") ? DateTime.Now.Date : ConvertDDMMYYTOYYMMDD(Model.NotuDate);
            //Tobj.LASTUPDATEDATE = DateTime.Now;
            //Tobj.TransporterCode = Model.Transporter;
            //ctxTFAT.TransportDetail.Add(Tobj);
        }

        private void SaveTDSPayments(PurchaseVM Model)
        {
            var tdsrates = ctxTFAT.TDSRates.Where(x => x.Code == Model.TDSCode).Select(x => new { x.Tax, x.Cess, x.SHECess, x.SurCharge }).FirstOrDefault();
            if (Model.CutTDS == true)
            {
                TDSPayments tspay = new TDSPayments();
                tspay.aMainType = Model.MainType;
                tspay.Amount = Model.Amt;
                tspay.aPrefix = Model.Prefix;
                tspay.aSno = 1;
                tspay.aSrl = Model.Srl.ToString();;
                tspay.SubType = Model.SubType;
                tspay.aSubType = Model.SubType;
                tspay.aType = Model.Type;
                tspay.BankCode = "";
                tspay.BillNumber = (Model.BillNumber == null || Model.BillNumber == "") ? "" : Model.BillNumber;
                tspay.Branch = mbranchcode;
                tspay.CertDate = DateTime.Now;
                tspay.CertNumber = "";
                tspay.ChallanDate = ConvertDDMMYYTOYYMMDD(Model.OrdDate);
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
                tspay.Srl = Model.Srl.ToString();;
                tspay.SubType = Model.SubType;
                tspay.TableKey = Model.Type + Model.Prefix.Substring(0, 2) + 1.ToString("D3") + Model.Srl.ToString();;
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
                tspay.TDSTax = tdsrates == null ? 0 : tdsrates.Tax;
                tspay.TDSTaxAmt = 0;
                tspay.TotalTDSAmt = Model.TDSAmt + Model.TDSSchg + Model.TDSCess + Model.TDSSHECess;
                tspay.Type = Model.Type;
                tspay.ENTEREDBY = muserid;
                tspay.LASTUPDATEDATE = DateTime.Now;
                tspay.AUTHORISE = Model.AUTHORISE;
                tspay.AUTHIDS = muserid;
                ctxTFAT.TDSPayments.Add(tspay);
            }

        }

        private void SaveNarration(PurchaseVM Model, string ParentKey)
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
                narr.AUTHORISE = Model.AUTHORISE;
                narr.AUTHIDS = muserid;
                narr.LocationCode = Model.LocationCode;
                narr.TableKey = ParentKey;
                narr.CompCode = mcompcode;
                narr.ParentKey = ParentKey;
                ctxTFAT.Narration.Add(narr);
            }
        }

        public string GetBrokerName(string Code)
        {
            string name = "";
            var sales = ctxTFAT.Broker.Where(x => x.Code.ToString() == Code).Select(x => x).FirstOrDefault();
            if (sales != null)
            {
                name = sales.Name;
            }
            else
            {
                name = "";
            }
            return name;
        }

        public string GetTransporterName(int Code)
        {
            string name = "";
            string strcode = Code.ToString();
            var sales = ctxTFAT.Master.Where(x => x.Code == strcode).Select(x => x).FirstOrDefault();
            if (sales != null)
            {
                name = sales.Name;
            }
            else
            {
                name = "";
            }
            return name;
        }

        public decimal GetChargesVal(List<PurchaseVM> Charges, string FToken)
        {
            string connstring = GetConnectionString();
            string sql;
            decimal mamtm;
            var Val = Charges.Where(x => x.Fld == FToken).Select(x => x.Val1).FirstOrDefault();
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

        public ActionResult GetCreditLimit(string Party)
        {
            PurchaseVM Model = new PurchaseVM();
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

        public ActionResult GetBalanceOnPurchase(string Party)
        {
            PurchaseVM Model = new PurchaseVM();
            var Balance = GetBalance(Party, DateTime.Now, mbranchcode);
            Model.Balance = Convert.ToDouble(Balance);
            return Json(Model, JsonRequestBehavior.AllowGet);
        }

        public decimal GetCreditPendingPO(string Party, string SubType)
        {
            string connstring = GetConnectionString();
            decimal bca;
            var loginQuery3 = @"select SUM(dbo.fn_GetPendingOrder(subtype,tablekey) as pending from ordersstk where party=" + "'" + Party + "'" + " and Subtype = " + "'" + SubType + "'" + "";
            DataTable mDt2 = GetDataTable(loginQuery3, connstring);
            if (mDt2.Rows.Count > 0)
            {
                bca = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? 0 : Convert.ToDecimal(mDt2.Rows[0][0].ToString());
            }
            else
            {
                bca = 0;
            }
            return bca;
        }

        public string GetContainerCode(string Name)
        {
            string code;
            var name = ctxTFAT.Container.Where(x => x.Name == Name).Select(x => x.Code).FirstOrDefault();
            if (name != null)
            {
                code = "0";
            }
            else
            {
                code = name.ToString();
            }
            return code;
        }

        public string GetContainerName(string Code)
        {
            var cod = Convert.ToInt32(Code);
            var name = ctxTFAT.Container.Where(x => x.Code == cod).Select(x => x.Name).FirstOrDefault();
            return name;
        }

        public List<string> GetContainerList()
        {
            var name = ctxTFAT.Container.Select(x => x.Name).ToList();
            return name;
        }

        public string GetAssociateCode(string Account)
        {
            string Code = "";
            var abc = ctxTFAT.TfatBranch.Where(x => x.Account == Account).Select(x => x).FirstOrDefault();
            if (abc != null)
            {
                Code = abc.Code;
            }

            return Code;

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

        public string GetAssociateType(string Type)
        {
            string Code = "";
            var abc = ctxTFAT.DocTypes.Where(x => x.Code == Type).Select(x => x).FirstOrDefault();
            if (abc != null)
            {
                Code = abc.RefType;
            }

            return Code;
        }

        public PurchaseVM GetRateByCircular(PurchaseVM Model)
        {
            PurchaseVM Model2 = new PurchaseVM();
            var mDocTypes = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => new { x.Circulars, x.Category }).FirstOrDefault();
            if (mDocTypes.Circulars == 1 || mDocTypes.Circulars == 3)
            {
                Model2 = GetPriceListRate(Model.Code, Model.Account, Model.DocDate, Model.ClassValues1, Model.ClassValues2);
            }
            if (mDocTypes.Circulars != 3)
            {
                if (Model2.Rate == 0)
                {
                    var categ = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Category).FirstOrDefault();
                    if (mDocTypes.Category != null && mDocTypes.Category != 0)
                    {
                        var prodcatlink = ctxTFAT.ProductCatLink.Where(x => x.CatNo == mDocTypes.Category && x.Code == Model.Code).Select(X => new { X.Rate, X.Disc, X.DiscAmt }).FirstOrDefault();
                        Model2.Rate = prodcatlink == null ? 0 : prodcatlink.Rate == null ? 0 : prodcatlink.Rate.Value;
                        Model2.DiscNotAllowed = false;
                        Model2.Disc = prodcatlink == null ? 0 : prodcatlink.Disc == null ? 0 : prodcatlink.Disc.Value;
                        Model2.DiscAmt = prodcatlink == null ? 0 : prodcatlink.DiscAmt == null ? 0 : prodcatlink.DiscAmt.Value;
                    }
                    else
                    {
                        var prodcatlink = ctxTFAT.ProductCatLink.Where(x => x.CatNo == categ && x.Code == Model.Code).Select(X => new { X.Rate, X.Disc, X.DiscAmt }).FirstOrDefault();
                        Model2.Rate = prodcatlink == null ? 0 : prodcatlink.Rate == null ? 0 : prodcatlink.Rate.Value;
                        Model2.DiscNotAllowed = false;
                        Model2.Disc = prodcatlink == null ? 0 : prodcatlink.Disc == null ? 0 : prodcatlink.Disc.Value;
                        Model2.DiscAmt = prodcatlink == null ? 0 : prodcatlink.DiscAmt == null ? 0 : prodcatlink.DiscAmt.Value;
                    }
                }
                if (Model2.Rate == 0)
                {
                    if (mDocTypes.Circulars == 2)
                    {
                        Model2 = GetPriceListRate(Model.Code, Model.Account, Model.DocDate, Model.ClassValues1, Model.ClassValues2);
                    }
                    else
                    {
                        if (Model.MainType == "PR")
                        {
                            Model2.Rate = GetRate(Model.Code, Model.Store, Model.LocationCode);
                            Model2.Disc = ctxTFAT.ItemDetail.Where(x => x.Code == Model.Code && x.Branch == mbranchcode).Select(x => (decimal?)x.PurchDisc ?? 0).FirstOrDefault();
                            Model2.DiscAmt = ctxTFAT.ItemDetail.Where(x => x.Code == Model.Code && x.Branch == mbranchcode).Select(x => (decimal?)x.PurchDiscAmt ?? 0).FirstOrDefault();
                            Model2.DiscNotAllowed = false;
                        }
                        else
                        {
                            Model2.Rate = ctxTFAT.ItemDetail.Where(x => x.Code == Model.Code && x.Branch == mbranchcode).Select(x => (double?)x.SalesRate ?? 0).FirstOrDefault();
                            Model2.Disc = ctxTFAT.ItemDetail.Where(x => x.Code == Model.Code && x.Branch == mbranchcode).Select(x => (decimal?)x.SalesDisc ?? 0).FirstOrDefault();
                            Model2.DiscAmt = ctxTFAT.ItemDetail.Where(x => x.Code == Model.Code && x.Branch == mbranchcode).Select(x => (decimal?)x.SalesDiscAmt ?? 0).FirstOrDefault();
                            Model2.DiscNotAllowed = false;
                        }

                    }
                }
            }

            return Model2;
        }

        public PurchaseVM GetPriceListRate(string mItem, string mParty, DateTime mDate, string ClassValue1, string ClassValue2)
        {
            string mPartyValue = "";
            string mItemValue = "";
            string mItemGroup = "";
            int? mPartyCategory = 0;
            string mPartyGroup = "";
            string mPriceLCode = "";
            string mItemGroupTree = "";
            string mState = "";
            string mStr = "";
            List<string> mPriceList = new List<string>();
            mPartyValue = mParty;
            mItemValue = mItem;
            mDate = mDate.Date;
            mPartyCategory = ctxTFAT.Master.Where(z => z.Code == mParty).Select(x => x.Category).FirstOrDefault();
            mPriceLCode = ctxTFAT.MasterInfo.Where(x => x.Code == mParty).Select(x => x.PriceList).FirstOrDefault() ?? "";
            //mItemGroupTree = ctxTFAT.ItemMaster.Where(z => z.Code == mItem).Select(x => x.GroupTree).FirstOrDefault();
            mPriceList = mPriceLCode.Split(',').ToList();
            List<string> mGroupTreeLst = new List<string>();
            if (mItemGroupTree != null)
            {
                mGroupTreeLst = mItemGroupTree.Split('@').ToList();
            }
            bool mIsItemClass = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.gp_ItemClass).FirstOrDefault();
            //1. Parties+items
           
            double? mRate = 0;
            double? mMRP = 0;
            decimal? mSalesMargin = 0;
            double? mMinrate = 0;
            bool mDiscNotAllowed = false;
            double mDisc = 0;
            double mDiscAmt = 0;
            string mCode = "";
          
            PurchaseVM Model2 = new PurchaseVM();
            Model2.Rate = mRate.Value;
            Model2.DiscNotAllowed = mDiscNotAllowed;
            Model2.Disc = Convert.ToDecimal(mDisc);
            Model2.DiscAmt = Convert.ToDecimal(mDiscAmt);
            Model2.PriceRateCode = mCode;
            return Model2;
        }

        public PriceListVM GetPriceListDisc(string mItem, string mParty, DateTime mDate)
        {
            string mPartyValue = "";
            string mItemValue = "";
            string mItemGroup = "";
            int? mPartyCategory = 0;
            string mPartyGroup = "";
            string mState = "";
            string mItemGroupTree = "";
            string mStr = "";
            string mPriceDiscLCode = "";
            List<string> mPriceList = new List<string>();
            mPartyValue = mParty;
            mItemValue = mItem;
            //mItemGroupTree = ctxTFAT.ItemMaster.Where(x => x.Code == mItem).Select(x => x.GroupTree).FirstOrDefault();
            var mMaster = ctxTFAT.Master.Where(z => z.Code == mParty).Select(x => new { x.Category, x.Grp }).FirstOrDefault();
            mPriceDiscLCode = ctxTFAT.MasterInfo.Where(x => x.Code == mParty).Select(x => x.PriceDiscList).FirstOrDefault() ?? "";
            mPartyCategory = mMaster.Category;
            mPartyGroup = mMaster.Grp;
            mDate = mDate.Date;
            mPriceList = mPriceDiscLCode.Split(',').ToList();
            List<string> mGroupTreeLst = new List<string>();
            if (mItemGroupTree != null)
            {
                mGroupTreeLst = mItemGroupTree.Split('@').ToList();
            }

           
            double? mRate = 0;
            double? mMRP = 0;
            decimal? mSalesMargin = 0;
            double? mMinrate = 0;
            PriceListVM mModel = new PriceListVM();
          
            return mModel;
        }

       

        

        public DataTable GetPurSaleAddonValue(string MainType, string ParentKey)
        {
            string connstring = GetConnectionString();
            string bca = "";
            var loginQuery3 = @"select * from addondoc" + MainType + " where tablekey=" + "'" + ParentKey + "'" + "";
            DataTable mDt2 = GetDataTable(loginQuery3, connstring);
            //if (mDt2.Rows.Count > 0)
            //{
            //    bca = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? "" : mDt2.Rows[0][0].ToString();
            //}
            //mDt2.Dispose();
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

        public ActionResult GetStoreWiseStock(PurchaseVM Model)
        {
            ViewBag.StoreWiseStock = null;
            var mStores = ctxTFAT.Stores.Select(x => x).ToList();
            var mLocations = ctxTFAT.Warehouse.Select(x => x).ToList();
            var mBranches = ctxTFAT.TfatBranch.Select(x => x).ToList();
            List<PurchaseVM> mStoreStk = new List<PurchaseVM>();
            List<PurchaseVM> mLocaStk = new List<PurchaseVM>();
            List<PurchaseVM> mBranchStk = new List<PurchaseVM>();
            foreach (var a in mStores)
            {
                PurchaseVM mModel = new PurchaseVM();
                mModel.Store = a.Code;
                mModel.StoreName = a.Name;
                double mvalue = 0;
                string mstr = "Select Sum(Qty) as Stock from Stock where NotinStock = 0 and Code='" + Model.Code + "' and Store='" + a.Code + "'";
                DataTable mDt = GetDataTable(mstr);
                if (mDt.Rows.Count > 0)
                {
                    mvalue = (mDt.Rows[0][0].ToString() == null || mDt.Rows[0][0].ToString() == "") ? 0 : Convert.ToDouble(mDt.Rows[0][0].ToString());
                    mModel.Stock = mvalue;
                }
                else
                {
                    mvalue = 0;
                    mModel.Stock = mvalue;
                }
                mDt.Dispose();

                double mvalue2 = 0;
                string mstr2 = "Select Pending = abs(r.Qty) - abs(isnull((Select sum(Qty) from OrdersStk Where Subtype = 'OP' and OrdersStk.OrdKey=r.TableKey),0)) from Stock r where r.Code='" + Model.Code + "' and r.Store = '" + a.Code + "'";
                DataTable mDt2 = GetDataTable(mstr2);
                if (mDt2.Rows.Count > 0)
                {
                    mvalue2 = (mDt2.Rows[0][0].ToString() == null || mDt2.Rows[0][0].ToString() == "") ? 0 : Convert.ToDouble(mDt2.Rows[0][0].ToString());
                    mModel.Pending = mvalue2;
                }
                else
                {
                    mvalue2 = 0;
                    mModel.Pending = mvalue2;
                }
                mDt2.Dispose();


                double mvalue3 = 0;
                string mstr3 = "Select Pending = abs(r.Qty) - abs(isnull((Select sum(Qty) from OrdersStk Where Subtype = 'OS' and OrdersStk.OrdKey=r.TableKey),0)) from Stock r where r.Code='" + Model.Code + "' and r.Store = '" + a.Code + "'";
                DataTable mDt3 = GetDataTable(mstr3);
                if (mDt3.Rows.Count > 0)
                {
                    mvalue3 = (mDt3.Rows[0][0].ToString() == null || mDt3.Rows[0][0].ToString() == "") ? 0 : Convert.ToDouble(mDt3.Rows[0][0].ToString());
                    mModel.Pending2 = Math.Abs(mvalue3);
                }
                else
                {
                    mvalue3 = 0;
                    mModel.Pending2 = mvalue3;
                }
                mDt3.Dispose();

                double mvalue4 = 0;
                string mstr4 = "Select [dbo].[fn_GetReservationStore]('" + Model.Code + "','" + mbranchcode + "','" + a.Code + "')";
                DataTable mDt4 = GetDataTable(mstr4);
                if (mDt4.Rows.Count > 0)
                {
                    mvalue4 = (mDt4.Rows[0][0].ToString() == null || mDt4.Rows[0][0].ToString() == "") ? 0 : Convert.ToDouble(mDt4.Rows[0][0].ToString());
                    mModel.TotalQty = Math.Abs(mvalue4);
                }
                else
                {
                    mvalue4 = 0;
                    mModel.TotalQty = mvalue4;
                }
                mDt4.Dispose();

                mModel.Balance = mModel.Stock + mModel.Pending - mModel.Pending2 - mModel.TotalQty;
                mStoreStk.Add(mModel);
            }
            ViewBag.StoreWiseStock = mStoreStk;
            foreach (var a in mLocations)
            {
                PurchaseVM mModel = new PurchaseVM();
                mModel.LocationCode = a.Code;
                mModel.Header = a.Name;
                double mvalue = 0;
                string mstr = "Select Sum(Qty) as Stock from Stock where NotinStock = 0 and Code='" + Model.Code + "' and LocationCode='" + a.Code + "'";
                DataTable mDt = GetDataTable(mstr);
                if (mDt.Rows.Count > 0)
                {
                    mvalue = (mDt.Rows[0][0].ToString() == null || mDt.Rows[0][0].ToString() == "") ? 0 : Convert.ToDouble(mDt.Rows[0][0].ToString());
                    mModel.Stock = mvalue;
                }
                else
                {
                    mvalue = 0;
                    mModel.Stock = mvalue;
                }
                mDt.Dispose();

                double mvalue2 = 0;
                string mstr2 = "Select Pending = abs(r.Qty) - abs(isnull((Select sum(Qty) from OrdersStk Where Subtype = 'OP' and OrdersStk.OrdKey=r.TableKey),0)) from Stock r where r.Code='" + Model.Code + "' and r.LocationCode = '" + a.Code + "'";
                DataTable mDt2 = GetDataTable(mstr2);
                if (mDt2.Rows.Count > 0)
                {
                    mvalue2 = (mDt2.Rows[0][0].ToString() == null || mDt2.Rows[0][0].ToString() == "") ? 0 : Convert.ToDouble(mDt2.Rows[0][0].ToString());
                    mModel.Pending = mvalue2;
                }
                else
                {
                    mvalue2 = 0;
                    mModel.Pending = mvalue2;
                }
                mDt2.Dispose();


                double mvalue3 = 0;
                string mstr3 = "Select Pending = abs(r.Qty) - abs(isnull((Select sum(Qty) from OrdersStk Where Subtype = 'OS' and OrdersStk.OrdKey=r.TableKey),0)) from Stock r where r.Code='" + Model.Code + "' and r.LocationCode = '" + a.Code + "'";
                DataTable mDt3 = GetDataTable(mstr3);
                if (mDt3.Rows.Count > 0)
                {
                    mvalue3 = (mDt3.Rows[0][0].ToString() == null || mDt3.Rows[0][0].ToString() == "") ? 0 : Convert.ToDouble(mDt3.Rows[0][0].ToString());
                    mModel.Pending2 = Math.Abs(mvalue3);
                }
                else
                {
                    mvalue3 = 0;
                    mModel.Pending2 = mvalue3;
                }
                mDt3.Dispose();

                double mvalue4 = 0;
                string mstr4 = "Select [dbo].[fn_GetReservation]('" + Model.Code + "','" + mbranchcode + "')";
                DataTable mDt4 = GetDataTable(mstr4);
                if (mDt4.Rows.Count > 0)
                {
                    mvalue4 = (mDt4.Rows[0][0].ToString() == null || mDt4.Rows[0][0].ToString() == "") ? 0 : Convert.ToDouble(mDt4.Rows[0][0].ToString());
                    mModel.TotalQty = Math.Abs(mvalue4);
                }
                else
                {
                    mvalue4 = 0;
                    mModel.TotalQty = mvalue4;
                }
                mDt4.Dispose();

                mModel.Balance = mModel.Stock + mModel.Pending - mModel.Pending2 - mModel.TotalQty;
                mLocaStk.Add(mModel);
            }
            ViewBag.LocationWiseStock = mLocaStk;
            foreach (var a in mBranches)
            {
                PurchaseVM mModel = new PurchaseVM();
                mModel.Branch = a.Code;
                mModel.Header = a.Name;
                double mvalue = 0;
                string mstr = "Select Sum(Qty) as Stock from Stock where NotinStock = 0 and Code='" + Model.Code + "' and Branch='" + a.Code + "'";
                DataTable mDt = GetDataTable(mstr);
                if (mDt.Rows.Count > 0)
                {
                    mvalue = (mDt.Rows[0][0].ToString() == null || mDt.Rows[0][0].ToString() == "") ? 0 : Convert.ToDouble(mDt.Rows[0][0].ToString());
                    mModel.Stock = mvalue;
                }
                else
                {
                    mvalue = 0;
                    mModel.Stock = mvalue;
                }
                mDt.Dispose();

                double mvalue2 = 0;
                string mstr2 = "Select Pending = abs(r.Qty) - abs(isnull((Select sum(Qty) from OrdersStk Where Subtype = 'OP' and OrdersStk.OrdKey=r.TableKey),0)) from Stock r where r.Code='" + Model.Code + "' and r.Branch = '" + a.Code + "'";
                DataTable mDt2 = GetDataTable(mstr2);
                if (mDt2.Rows.Count > 0)
                {
                    mvalue2 = (mDt2.Rows[0][0].ToString() == null || mDt2.Rows[0][0].ToString() == "") ? 0 : Convert.ToDouble(mDt2.Rows[0][0].ToString());
                    mModel.Pending = mvalue2;
                }
                else
                {
                    mvalue2 = 0;
                    mModel.Pending = mvalue2;
                }
                mDt2.Dispose();


                double mvalue3 = 0;
                string mstr3 = "Select Pending = abs(r.Qty) - abs(isnull((Select sum(Qty) from OrdersStk Where Subtype = 'OS' and OrdersStk.OrdKey=r.TableKey),0)) from Stock r where r.Code='" + Model.Code + "' and r.Branch = '" + a.Code + "'";
                DataTable mDt3 = GetDataTable(mstr3);
                if (mDt3.Rows.Count > 0)
                {
                    mvalue3 = (mDt3.Rows[0][0].ToString() == null || mDt3.Rows[0][0].ToString() == "") ? 0 : Convert.ToDouble(mDt3.Rows[0][0].ToString());
                    mModel.Pending2 = Math.Abs(mvalue3);
                }
                else
                {
                    mvalue3 = 0;
                    mModel.Pending2 = mvalue3;
                }
                mDt3.Dispose();

                double mvalue4 = 0;
                string mstr4 = "Select [dbo].[fn_GetReservation]('" + Model.Code + "','" + mbranchcode + "')";
                DataTable mDt4 = GetDataTable(mstr4);
                if (mDt4.Rows.Count > 0)
                {
                    mvalue4 = (mDt4.Rows[0][0].ToString() == null || mDt4.Rows[0][0].ToString() == "") ? 0 : Convert.ToDouble(mDt4.Rows[0][0].ToString());
                    mModel.TotalQty = Math.Abs(mvalue4);
                }
                else
                {
                    mvalue4 = 0;
                    mModel.TotalQty = mvalue4;
                }
                mDt4.Dispose();

                mModel.Balance = mModel.Stock + mModel.Pending - mModel.Pending2 - mModel.TotalQty;
                mBranchStk.Add(mModel);
            }
            ViewBag.BranchWiseStock = mBranchStk;
            var html = ViewHelper.RenderPartialView(this, "StoreWiseStock", new { ViewBag.StoreWiseStock, ViewBag.LocationWiseStock, ViewBag.BranchWiseStock });
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public ActionResult AddImage(HttpPostedFileBase files)
        {
            string XYZ = "";
            string ZYX = "";
            string FLN = "";
            byte[] fileData = null;
            for (int i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];
                var fileName = Path.GetFileName(file.FileName);

                MemoryStream target = new MemoryStream();
                using (var binaryReader = new BinaryReader(Request.Files[i].InputStream))
                {
                    fileData = binaryReader.ReadBytes(Request.Files[i].ContentLength);
                }

                string FileString = Convert.ToBase64String(fileData);
                file.InputStream.CopyTo(target);
                //if (file.ContentType.Contains("image"))
                //{
                //    Model.FileContent = "image";
                //    Model.ImageStr = string.Format("data:image/png;base64,{0}", FileString);
                //}
                //else
                //{
                //    Model.FileContent = file.ContentType;
                //    Model.ImageStr = string.Format("data:" + file.ContentType + ";base64,{0}", FileString);
                //}

                XYZ += FileString + ",";
                FLN += fileName + ",";
            }
            XYZ = XYZ.Remove(XYZ.Length - 1, 1);
            FLN = FLN.Remove(FLN.Length - 1, 1);
            var jsonResult = Json(new { AllFileStr = XYZ, AllFileName = FLN }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

    }
}