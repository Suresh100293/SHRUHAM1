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
using ZXing.QrCode.Internal;
using iTextSharp.text;
using iTextSharp.text.pdf;
using CrystalDecisions.CrystalReports.Engine;
using System.Globalization;
using System.Windows.Shapes;
using iTextSharp.text.pdf.security;
using System.Security.Cryptography;
using com.itextpdf.text.pdf.security;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using SelectPdf;
using ALT_ERP3.Areas.Logistics.Controllers;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class LRInvoiceController : BaseController
    {
        string attachmentPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentPath"].ToString()) == true ? "" : ConfigurationManager.AppSettings["AttachmentPath"].ToString();
        private static string mauthorise = "A00";

        private static string gpHoliday1 = "Saturday";
        private static string gpHoliday2 = "Sunday";
        private static int mGSTStyle = 0;
        public static object[,] objarray = null;
        private static string msearchstyle = "";
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
            return GetGridDataColumns(id, "L", "XXXX");
        }

        public ActionResult GetMasterGridData(GridOption Model)
        {
            if (Model.CustomerF)
            {
                Model.Code = "r.BillParty ='" + Model.Code + "'";
            }
            else
            {
                var List = ctxTFAT.CustomerMaster.Where(x => x.AccountParentGroup.Trim() == Model.Code.Trim()).Select(x => x.Code).ToList();
                string Customers = "";
                foreach (var item in List)
                {
                    Customers += "'" + item + "',";
                }
                Customers = Customers.Substring(0, (Customers.Length - 1));
                Model.Code = "r.BillParty in (" + Customers + ")";
            }

            List<LRInvoiceVM> objitemlist = new List<LRInvoiceVM>();
            if (Session["LONewItemlist"] != null)
            {
                objitemlist = (List<LRInvoiceVM>)Session["LONewItemlist"];
            }
            string LRList = "";
            if (objitemlist != null)
            {
                foreach (var item in objitemlist)
                {
                    LRList += item.Code + ",";
                }
                if (!String.IsNullOrEmpty(LRList))
                {
                    LRList = LRList.Substring(0, LRList.Length - 1);
                    Model.Code += " And r.lrno not in (" + LRList + ")";
                }
            }


            if (Model.Branch == "All")
            {
                var List = ctxTFAT.TfatBranch.Where(x => x.Category.Trim() == "Branch" || x.Category.Trim() == "SubBranch").Select(x => x.Code).ToList();
                string Branches = "'HO0000',";
                foreach (var item in List)
                {
                    Branches += "'" + item + "',";
                }
                Branches = Branches.Substring(0, (Branches.Length - 1));
                Model.Branch = "r.Branch in (" + Branches + ")";
            }
            else
            {
                Model.Branch = "r.Branch ='" + mbranchcode + "'";
            }

            return GetGridReport(Model, "M", "Code^" + Model.Code + "~para01^" + Model.Branch + (mpara != "" ? "~" + mpara : ""), false, 0);
        }

        #region index

        public ActionResult Index(LRInvoiceVM Model)
        {
            Session["CommnNarrlist"] = null;
            Session["TempAttach"] = null;
            Model.Message = "";
            GetAllMenu(Session["ModuleName"].ToString());
            string connstring = GetConnectionString();
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            ViewBag.list = Session["MenuList"];
            ViewBag.Modules = Session["ModulesList"];
            //to-do remove updateitemlist. UPurchSerialStkList
            Session["LONewItemlist"] = null;
            Model.RpoertViewData = GetSubCodeoflist(Model.ViewDataId);

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
                Model.Type = "SLR00";
                UpdateAuditTrail(Model.Branch, Model.Mode, Model.Header, Model.ParentKey, DateTime.Now, 0, "", "", "NA");
            }
            else
            {
                Model.Branch = mbranchcode;
                Model.Type = "SLR00";
                UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "NA");
            }
            //var LRBillQuery = @"select isnull(BillBoth,0) as BillBoth,BillAuto,OthLRShow,CutTDS,ShowLedgerPost,LRReqd from LRBillSetup";
            //DataTable imDt = GetDataTable(LRBillQuery, connstring);
            var LRBillSetup = ctxTFAT.LRBillSetup.FirstOrDefault();
            if (LRBillSetup == null)
            {
                LRBillSetup = new LRBillSetup();
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
            //Model.IsRoundOff = result.RoundOff;
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
                var PreviousDate = Convert.ToString(TempData.Peek("PreviousDate"));
                if (String.IsNullOrEmpty(PreviousDate))
                {
                    Model.DocDate = DateTime.Now;
                }
                else
                {
                    Model.DocDate = ConvertDDMMYYTOYYMMDD(PreviousDate);
                }
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

                    Model.GSTType = mobj1.GSTType.ToString();
                    Model.GstTypeName = GetGSTTypeName(Model.GSTType);

                    Model.Authorise = mobj1.AUTHORISE;
                    var mdoctype = ctxTFAT.DocTypes.Where(x => x.Code == mobj1.Type).Select(x => new { x.DocWidth }).FirstOrDefault();

                    Model.Srl = mobj1.Srl.PadLeft(mdoctype.DocWidth, '0');
                    Model.Prefix = mobj1.Prefix;
                    Model.DocDate = mobj1.DocDate.Value;
                    Model.Branch = mobj1.Branch;
                    Model.LocationCode = mobj1.LocationCode;
                    Model.Account = mobj1.Code;
                    Model.HitContractType = (ctxTFAT.CustomerMaster.Where(x => x.Code == mobj1.Code).Select(x => x.ContractHitby).FirstOrDefault().ToString());
                    Model.AccountName = (ctxTFAT.CustomerMaster.Where(x => x.Code == mobj1.Code).Select(x => x.Name).FirstOrDefault().ToString());
                    Model.AltAddress = Convert.ToByte(mobj1.AltAddress);

                    string Address = "";
                    var Caddress = (from Add in ctxTFAT.Caddress where Add.Code == mobj1.Code && Add.Sno == mobj1.AltAddress select Add).FirstOrDefault();
                    if (Caddress != null)
                    {
                        if (Caddress.Adrl1 != null)
                        {
                            Address = Caddress.Adrl1;
                        }
                        if (Caddress.Adrl2 != null && Caddress.Adrl2 != "")
                        {
                            if (Address != "")
                            {
                                Address = Address + ",\n" + Caddress.Adrl2;
                            }
                            else
                            {
                                Address = Caddress.Adrl2;
                            }
                        }
                        if (Caddress.Adrl3 != null && Caddress.Adrl3 != "")
                        {
                            if (Address != "")
                            {
                                Address = Address + ",\n" + Caddress.Adrl3;
                            }
                            else
                            {
                                Address = Caddress.Adrl3;
                            }
                        }
                        if (Caddress.Adrl4 != "" && Caddress.Adrl4 != "")
                        {
                            if (Address != "")
                            {
                                Address = Address + ",\n" + Caddress.Adrl4;
                            }
                            else
                            {
                                Address = Caddress.Adrl4;
                            }
                        }
                        if (Caddress.City != null && Caddress.City != "")
                        {
                            if (Address != "")
                            {
                                Address = Address + ",\n" + ctxTFAT.TfatCity.Where(x => x.Code == Caddress.City).Select(x => x.Name).FirstOrDefault() + (Caddress.Pin == null || Caddress.Pin == "" ? "" : "-" + Caddress.Pin);
                            }
                            else
                            {
                                Address = ctxTFAT.TfatCity.Where(x => x.Code == Caddress.City).Select(x => x.Name).FirstOrDefault();
                            }
                        }
                        if (Caddress.State != null && Caddress.State != "")
                        {
                            if (Address != "")
                            {
                                Address = Address + ",\n" + ctxTFAT.TfatState.Where(x => x.Code.ToString() == Caddress.State).Select(x => x.Name).FirstOrDefault();
                            }
                            else
                            {
                                Address = ctxTFAT.TfatState.Where(x => x.Code.ToString() == Caddress.State).Select(x => x.Name).FirstOrDefault();
                            }
                        }
                        if (Caddress.Country != null && Caddress.Country != "")
                        {
                            if (Address != "")
                            {
                                Address = Address + ",\n" + ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == Caddress.Country).Select(x => x.Name).FirstOrDefault();
                            }
                            else
                            {
                                Address = ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == Caddress.Country).Select(x => x.Name).FirstOrDefault();
                            }

                        }
                        if (Caddress.Mobile != null && Caddress.Mobile != "")
                        {
                            if (Address != "")
                            {
                                Address = Address + ",\n" + Caddress.Mobile;
                            }
                            else
                            {
                                Address = Caddress.Mobile;
                            }

                        }
                        if (Caddress.Email != null)
                        {
                            if (Address != "")
                            {
                                Address = Address + ",\n" + Caddress.Email;
                            }
                            else
                            {
                                Address = Caddress.Email;
                            }
                        }
                    }
                    Model.AddressFrom = Address;
                    Model.Document = Model.Document;
                    Model.CrPeriod = mobj1.CrPeriod == null ? 0 : mobj1.CrPeriod.Value;
                    Model.AccParentGrp = ctxTFAT.CustomerMaster.Where(x => x.Code == mobj1.Code).Select(x => x.AccountParentGroup).FirstOrDefault();
                    Model.AccParentGrpName = ctxTFAT.Master.Where(x => x.Code == Model.AccParentGrp).Select(x => x.Name).FirstOrDefault();

                    //Model.IntegerValue = ctxTFAT.Caddress.Where(x => x.Code == mobj1.Code).ToList().Count;
                    //Model.SelectedIntegerValue = Model.AltAddress;

                    Model.BillNarr = mobj1.Narr;
                    Model.CPerson = String.IsNullOrEmpty(mobj1.CPerson) == true ? ctxTFAT.Caddress.Where(x => x.Code == mobj1.Code && x.Sno == 0).Select(x => x.Name).FirstOrDefault() : mobj1.CPerson;
                    Model.Remark = mobj1.Remark;
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
                    int mCnt = 1;
                    List<LRInvoiceVM> Upobjitemdetail = new List<LRInvoiceVM>();
                    foreach (var item in mobjtax)
                    {
                        //int mcode = Convert.ToInt32(item.LrNo);
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
                            Account = ctxTFAT.CustomerMaster.Where(X => X.Code == lrmaster.BillParty).Select(x => x.Name).FirstOrDefault(),
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
                            LRPartyInvoice = lrmaster.PartyInvoice,
                            LRPONumber = lrmaster.PONumber,
                            LRRefTableKey = lrmaster.TableKey
                        });
                        ++mCnt;
                    }
                    Session.Add("LONewItemlist", Upobjitemdetail);
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
                Att.Type = "SLR00";
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

                        Model.Message += " Document is Already Adjusted In Cash Bank Against : " + CheckDependency2.TableRefKey.ToString() + ", Cant " + Model.Mode;
                    }

                }
                if (Model.Mode != "Add" && Model.AuthReq == true && Model.Authorise.Substring(0, 1) == "A" && Model.AuthLock)
                {
                    Model.CheckMode = true;
                    //Model.Mode = "View";
                    Model.Message += " Document is Already Authorised Cant Edit...";
                }
                if (Model.Mode != "Add")
                {
                    if (!String.IsNullOrEmpty(ctxTFAT.Sales.Where(x => x.TableKey == Model.ParentKey && x.Branch == Model.Branch).Select(x => x.GSTIRNNumber).FirstOrDefault()))
                    {
                        Model.CheckIRNGENMode = true;
                        //Model.Mode = "View";
                        //Model.Message += " Documents IRN Generated Cant Edit/Delete...!";
                    }

                }
                if (Model.Authorise.Substring(0, 1) == "X")
                {
                    Model.IsDraftSave = true;
                    Model.Message = " Document is saved As Draft...!";
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
                            c.Debit = Convert.ToDecimal(mdt.Rows[0]["Amt" + mfld]);
                        }
                        else
                        {
                            c.ColVal = "0";
                            c.Debit = 0;
                        }
                    }
                    c.ChgPostCode = i.Code;
                    objledgerdetail.Add(c);
                }
                Model.Charges = objledgerdetail;
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
                if (!String.IsNullOrEmpty(mLedger.TaxCode))
                {
                    Model.GSTCode = mLedger.TaxCode == null ? "" : mLedger.TaxCode.ToString();
                    Model.GSTCodeName = ctxTFAT.TaxMaster.Where(x => x.Code.ToString() == Model.GSTCode).Select(x => x.Name).FirstOrDefault();
                    if (String.IsNullOrEmpty(Model.GSTCodeName))
                    {
                        Model.GSTCodeName = ctxTFAT.HSNMaster.Where(x => x.Code.ToString() == Model.GSTCode).Select(x => x.Name).FirstOrDefault();
                    }
                }

                Model.PeriodLock = PeriodLock(mLedger.Branch, mLedger.Type, mLedger.DocDate);
                if (mLedger.AUTHORISE.Substring(0, 1) == "A")
                {
                    Model.LockAuthorise = LockAuthorise(mLedger.Type, Model.Mode, mLedger.TableKey, mLedger.ParentKey);
                }
            }



            Model.Taxable = Model.TotalChgPickupList == null ? 0 : Model.TotalChgPickupList.Sum();
            Model.InvoiceAmt = Math.Round(Model.Taxable + Model.IGSTAmt + Model.CGSTAmt + Model.SGSTAmt, 2);
            //Model.RoundOff = Model.InvoiceAmt - (Model.Taxable + Model.IGSTAmt + Model.CGSTAmt + Model.SGSTAmt);
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
            List<GridOption> Grlist = new List<GridOption>();
            var list = ctxTFAT.DocFormats.Where(x => x.Type == "SLR00").Select(x => x).ToList();
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
            Model.AllHeaderList = ctxTFAT.Charges.Where(x => x.Type == "LR000").Select(x => x.Head).ToList();
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
        public ActionResult GetPartyDetails(LRInvoiceVM Model)
        {
            var CurrDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            var result = ctxTFAT.CustomerMaster.Where(x => x.Code == Model.Code).Select(x => new { x.Code, x.Name, x.SalesMan, x.Broker, x.AccountParentGroup }).FirstOrDefault();

            //Model.IntegerValue = ctxTFAT.Caddress.Where(x => x.Code == Model.Code).ToList().Count;
            //Model.SelectedIntegerValue = Model.AltAddress;
            //var html = ViewHelper.RenderPartialView(this, "_CustomerAddressList", Model);
            Model.AccParentGrp = result.AccountParentGroup;
            var msterinf = ctxTFAT.CMasterInfo.Where(x => x.Code == Model.Code).Select(x => new { x.CurrName, x.TDSCode, x.CutTDS, x.CrPeriod, x.Brokerage, x.Transporter, x.IncoTerms, x.IncoPlace, x.PaymentTerms, x.Rank }).FirstOrDefault();
            var addrl = ctxTFAT.Caddress.Where(x => x.Code == Model.Code && x.Sno == 0).FirstOrDefault();
            var taxdetails = ctxTFAT.CTaxDetails.Where(x => x.Code == Model.Code).Select(x => new { x.TDSCode, x.CutTDS }).FirstOrDefault();
            Model.CrPeriod = msterinf == null ? 0 : (msterinf.CrPeriod == null) ? 0 : msterinf.CrPeriod.Value;
            Model.AccParentGrpName = ctxTFAT.Master.Where(x => x.Code == Model.AccParentGrp).Select(x => x.Name).FirstOrDefault();
            Model.CPerson = addrl == null ? "" : addrl.Person;
            Model.PlaceOfSupply = addrl == null ? "" : addrl.State;
            Model.CutTDS = taxdetails == null ? false : taxdetails.CutTDS;
            Model.TDSCode = taxdetails == null ? "0" : taxdetails.TDSCode == null ? "0" : taxdetails.TDSCode.Value.ToString();
            Model.TDSCodeName = ctxTFAT.TDSMaster.Where(x => x.Code.ToString() == Model.TDSCode).Select(x => x.Name).FirstOrDefault();
            var mDocDate = DateTime.Now.Date;
            //var TDSRATEtab = ctxTFAT.TDSRates.Where(x => x.Code.ToString() == Model.TDSCode && x.EffDate <= mDocDate).OrderByDescending(x => x.EffDate).Select(x => new { x.TDSRate, x.Cess, x.SHECess, x.SurCharge }).FirstOrDefault();
            decimal TDSRate = 0;
            TDSRate = ctxTFAT.TDSRates.Where(x => x.Code.ToString() == Model.TDSCode && x.EffDate <= CurrDate).OrderByDescending(x => x.EffDate).Select(x => x.TDSRate ?? 0).FirstOrDefault();
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

            var TDSFlagSetup = ctxTFAT.LRBillSetup.Select(x => x.CutTDS).FirstOrDefault();
            if (TDSFlagSetup == false)
            {
                Model.CutTDS = false;
            }

            #region GST
            string GSTCode = "0", GSTName = "";
            bool GstFlag = false;
            decimal IGST = 0, CGST = 0, SGST = 0;
            var setup = ctxTFAT.LRBillSetup.FirstOrDefault();
            if (setup == null)
            {
                setup = new LRBillSetup();
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
            //if (Model.CutTDS==false)
            //{
            //    Model.CutTDS = ctxTFAT.LRBillSetup.Select(x => x.CutTDS).FirstOrDefault();
            //}

            var HitContract = ctxTFAT.CustomerMaster.Where(x => x.Code == Model.Code).Select(x => x.ContractHitby).FirstOrDefault();

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
                AccParentGrpName = Model.AccParentGrpName,
                CPerson = Model.CPerson,
                Balance = Model.Balance,
                HoldNarr = Model.HoldNarr,
                HoldInvoice = Model.HoldInvoice,
                AlertHoldInvoice = Model.AlertHoldInvoice,
                Tickler = Model.Tickler,
                TDSCodeName = Model.TDSCodeName,
                //html = html,
                HitContract = HitContract
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

        public ActionResult GetAddressList(string term,string Code)
        {
            var addrsnolist = ctxTFAT.Caddress.Where(x => x.Code == Code).ToList().Select(b => new 
            {
                Code = b.Sno.ToString(),
                Name = b.Sno.ToString()
            }).ToList();

            return Json(addrsnolist, JsonRequestBehavior.AllowGet);
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

                if (ctxTFAT.LRBillSetup.Select(x => x.PODReq).FirstOrDefault())
                {
                    Model.POD = true;
                }
                else
                {
                    Model.POD = false;
                }

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
                if (lrdetails.BillBran != mbranchcode)
                {
                    Model.Status = "ConfirmError";
                    Model.Message += "LR Bill Branch is Not of Current Branch Please Confirm Do You Want to Continue..\n";
                }
                if (mPendingQty <= 0)
                {
                    Model.Status = "ConfirmError";
                    Model.Message += "No Pending Quantity of Selected LR " + Model.Code + ".. Do You want to Continue\n";
                    Model.LockQty = true;
                }
                if (mAccount != Model.Account)
                {
                    Model.Status = "ConfirmError";
                    Model.Message += "" + Model.Code + " is of " + Model.Account + " Do you want to continue..\n";
                }
                if (Model.Account == null)
                {
                    Model.Status = "ConfirmError";
                    Model.Message += "LR Bill to Party Not Found Please Confirm Do You Want to Continue..\n";
                }
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

        [HttpPost]
        public ActionResult GetLRDetails(LRInvoiceVM Model)
        {
            //Change
            LRInvoiceVM Model2 = new LRInvoiceVM();
            var mCode = (string.IsNullOrEmpty(Model.Code) == true) ? 0 : Convert.ToInt32(Model.Code);
            var lrdetails = ctxTFAT.LRMaster.Where(x => x.TableKey == Model.Code).Select(x => x).FirstOrDefault();
            if (lrdetails == null)
            {
                Model2.Status = "ValidtnError";
                Model2.Message = "Document Not Found..";
                return Json(Model2, JsonRequestBehavior.AllowGet);
            }

            Model2.TotalQty = lrdetails.TotQty;
            Model2.LRDocDate = lrdetails.BookDate;
            Model2.LRPartyInvoice = lrdetails.PartyInvoice;
            Model2.LRPONumber = lrdetails.PONumber;

            Model2.LRDocuDate = lrdetails.BookDate.ToString("dd-MM-yyyy");
            Model2.Branch = ctxTFAT.TfatBranch.Where(X => X.Code == lrdetails.BillBran).Select(X => X.Name).FirstOrDefault();
            Model2.Consignor = ctxTFAT.Consigner.Where(X => X.Code == lrdetails.RecCode).Select(x => x.Name).FirstOrDefault();
            Model2.Consignee = ctxTFAT.Consigner.Where(X => X.Code == lrdetails.SendCode).Select(x => x.Name).FirstOrDefault();
            Model2.Narr = lrdetails.Narr;
            Model2.Weightage = Convert.ToDecimal(lrdetails.ActWt);
            Model2.FromLocation = ctxTFAT.TfatBranch.Where(X => X.Code == lrdetails.Source).Select(X => X.Name).FirstOrDefault();
            Model2.ToLocation = ctxTFAT.TfatBranch.Where(X => X.Code == lrdetails.Dest).Select(X => X.Name).FirstOrDefault();
            Model2.Account = ctxTFAT.CustomerMaster.Where(X => X.Code == lrdetails.BillParty).Select(x => x.Name).FirstOrDefault();
            var mAccount = ctxTFAT.CustomerMaster.Where(X => X.Code == Model.Account).Select(x => x.Name).FirstOrDefault();
            var mPendingQty = (ctxTFAT.LRMaster.Where(x => x.LrNo == mCode).Sum(x => (int?)x.TotQty ?? 0) - Math.Abs(ctxTFAT.LRBill.Where(x => x.LrNo == Model.Code).Sum(x => (int?)x.TotQty) ?? 0));
            Model2.Qty = mPendingQty;
            Model2.Pending = mPendingQty;
            Model2.ActWt = lrdetails.ActWt;
            Model2.ChgWt = lrdetails.ChgWt;
            if (!String.IsNullOrEmpty(lrdetails.DescrType))
            {
                Model2.ChargeType = ctxTFAT.DescriptionMaster.Where(x => x.Code == lrdetails.DescrType).Select(x => x.Description).FirstOrDefault();
            }
            if (lrdetails.BillBran != mbranchcode)
            {
                Model2.Status = "ConfirmError";
                Model2.Message = "LR Bill Branch is Not of Current Branch Please Confirm Do You Want to Continue..";
            }
            if (mPendingQty <= 0)
            {
                Model2.Status = "ConfirmError";
                Model2.Message = "No Pending Quantity of Selected LR " + Model.Code + ".. Do You want to Continue";
                Model2.LockQty = true;
            }
            if (lrdetails.BillParty != Model.Account)
            {
                Model2.Status = "ConfirmError";
                Model2.Message = "" + Model.Code + " is of " + Model2.Account + " Do you want to continue..";
            }
            if (Model2.Account == null)
            {
                Model2.Status = "ConfirmError";
                Model2.Message = "LR Bill to Party Not Found Please Confirm Do You Want to Continue..";
            }
            List<string> mLRCodes = new List<string>();
            var result = (List<LRInvoiceVM>)Session["LONewItemlist"];
            if (result != null && result.Count > 0)
            {
                mLRCodes = result.Select(x => x.LRRefTableKey).ToList();
            }
            if (mLRCodes.Where(x => x == lrdetails.TableKey.ToString()).FirstOrDefault() != null)
            {
                Model2.Status = "ValidtnError";
                Model2.Message = "Already Selected";
            }
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
                string connstring = GetConnectionString();
                string sql = "";
                decimal FLD = 0;
                sql = @"select isnull(val" + c.tempId + ",0) from LRMaster where Tablekey='" + lrdetails.TableKey + "' ";
                DataTable smDt = GetDataTable(sql, connstring);
                if (smDt.Rows.Count > 0)
                {
                    c.Amt = (smDt.Rows[0][0].ToString() == "" || smDt.Rows[0][0].ToString() == null) ? 0 : Convert.ToDecimal(smDt.Rows[0][0]);
                }
                else
                {
                    c.Amt = 0;
                }
                newlist1.Add(c);
                a = a + 1;
                b = b + 1;
            }
            Model2.LRChargeList = newlist1;
            List<decimal> ChatgesAmt = new List<decimal>();
            foreach (var item in newlist1)
            {
                ChatgesAmt.Add(item.Amt);
            }
            Model2.ChgPickupList = ChatgesAmt;
            return Json(Model2, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAddressBySno(string Code, int Sno)
        {
            string Address = "";
            Caddress result;
            LRInvoiceVM Model = new LRInvoiceVM();
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
                        Address = Address + ",\n" + ctxTFAT.TfatCity.Where(x => x.Code == result.City).Select(x => x.Name).FirstOrDefault() + (result.Pin == null || result.Pin == "" ? "" : "-" + result.Pin);
                    }
                    else
                    {
                        Address = ctxTFAT.TfatCity.Where(x => x.Code == result.City).Select(x => x.Name).FirstOrDefault();
                    }
                }
                if (result.State != null && result.State != "")
                {
                    if (Address != "")
                    {
                        Address = Address + ",\n" + ctxTFAT.TfatState.Where(x => x.Code.ToString() == result.State).Select(x => x.Name).FirstOrDefault();
                    }
                    else
                    {
                        Address = ctxTFAT.TfatState.Where(x => x.Code.ToString() == result.State).Select(x => x.Name).FirstOrDefault();
                    }
                }
                if (result.Country != null && result.Country != "")
                {
                    if (Address != "")
                    {
                        Address = Address + ",\n" + ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == result.Country).Select(x => x.Name).FirstOrDefault();
                    }
                    else
                    {
                        Address = ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == result.Country).Select(x => x.Name).FirstOrDefault();
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
            //Model.IntegerValue = ctxTFAT.Caddress.Where(x => x.Code == Code).ToList().Count;
            //Model.SelectedIntegerValue = Sno;
            //var html = ViewHelper.RenderPartialView(this, "_CustomerAddressList", Model);

            return Json(new { Address = Address }, JsonRequestBehavior.AllowGet);
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
        public ActionResult LRValidation(LRInvoiceVM Model)
        {
            if (Model.POD == true)
            {
                var mLrMaster = ctxTFAT.PODMaster.Where(x => x.ParentKey == Model.ParentKey && x.SendReceive == "C").Select(x => x).FirstOrDefault();
                if (mLrMaster != null)
                {
                    var PODRel = ctxTFAT.PODRel.Where(x => x.LRRefTablekey == Model.Code && x.SendReceive == "C" && x.ParentKey != mLrMaster.TableKey).FirstOrDefault();
                    if (PODRel != null && Model.POD == true)
                    {
                        Model.Status = "ConfirmError";
                        Model.Message = "LRNo " + Model.Code + " POD is already sended To Customer.";
                        //Model.POD = true;
                        return Json(Model, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        Model.POD = false;
                    }
                }
                else
                {
                    Model.POD = false;
                }
            }

            return Json(Model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetItemListforUpdate(LRInvoiceVM Model)
        {
            try
            {
                var result = (List<LRInvoiceVM>)Session["LONewItemlist"];
                if (result == null)
                {
                    result = new List<LRInvoiceVM>();
                }
                if (Model.SessionFlag == "Edit")
                {
                    var result1 = result.Where(x => x.tempId == Model.tempId).ToList();
                    foreach (var item in result1)
                    {
                        var mPendingQty = (ctxTFAT.LRMaster.Where(x => x.TableKey == item.LRRefTableKey).Sum(x => (int?)x.TotQty ?? 0) - Math.Abs(ctxTFAT.LRBill.Where(x => x.LRRefTablekey == item.LRRefTableKey).Sum(x => (int?)x.TotQty) ?? 0));
                        Model.tempId = item.tempId;
                        Model.Code = item.Code;
                        Model.LRRefTableKey = item.LRRefTableKey;
                        Model.LRName = item.Code;
                        var mLrMaster = ctxTFAT.LRMaster.Where(x => x.TableKey == item.LRRefTableKey).Select(x => x).FirstOrDefault();
                        Model.Qty = item.Qty;
                        Model.Val1 = decimal.Round(item.Val1, 2, MidpointRounding.AwayFromZero);
                        Model.Narr = item.Narr;
                        Model.SubType = Model.SubType;
                        Model.MainType = Model.MainType;
                        Model.Mode = Model.Mode;
                        Model.Account = ctxTFAT.CustomerMaster.Where(X => X.Code == mLrMaster.BillParty).Select(X => X.Name).FirstOrDefault();
                        Model.LRDocDate = mLrMaster.BookDate;
                        Model.IsPickUp = item.IsPickUp;
                        Model.PickedUpIn = item.PickedUpIn;
                        Model.ChgWt = mLrMaster.ChgWt;
                        Model.ActWt = mLrMaster.ActWt;
                        if (!String.IsNullOrEmpty(mLrMaster.DescrType))
                        {
                            Model.ChargeType = ctxTFAT.DescriptionMaster.Where(x => x.Code == mLrMaster.DescrType).Select(x => x.Description).FirstOrDefault();
                        }
                        Model.TotalQty = mLrMaster.TotQty;
                        Model.Pending = (mPendingQty - item.Qty) + item.Qty;
                        Model.Branch = ctxTFAT.TfatBranch.Where(X => X.Code == mLrMaster.BillBran).Select(X => X.Name).FirstOrDefault();
                        Model.Consignor = ctxTFAT.Consigner.Where(x => x.Code == mLrMaster.RecCode).Select(x => x.Name).FirstOrDefault();
                        Model.Consignee = ctxTFAT.Consigner.Where(x => x.Code == mLrMaster.SendCode).Select(x => x.Name).FirstOrDefault();
                        Model.ToLocation = ctxTFAT.TfatBranch.Where(X => X.Code == mLrMaster.Dest).Select(X => X.Name).FirstOrDefault();
                        Model.FromLocation = ctxTFAT.TfatBranch.Where(X => X.Code == mLrMaster.Source).Select(X => X.Name).FirstOrDefault();
                        Model.BookNarr = item.BookNarr;
                        Model.BillNarr = item.BillNarr;
                        Model.LRPartyInvoice = item.LRPartyInvoice;
                        Model.LRPONumber = item.LRPONumber;
                        Model.POD = item.POD;
                        Model.LockQty = item.LockQty;
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
            var result = (List<LRInvoiceVM>)Session["LONewItemlist"];
            AddLRList2 = (result == null) ? AddLRList2 : result;
            int Maxtempid = (AddLRList2.Count == 0) ? 0 : AddLRList2.Select(x => x.tempId).Max();
            AddLRList.AddRange(AddLRList2);
            if (Model.SessionFlag == "Add")
            {
                Maxtempid = Maxtempid + 1;
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
                    LRPartyInvoice = mLrMaster.PartyInvoice,
                    LRPONumber = mLrMaster.PONumber,
                    ToLocation = ctxTFAT.TfatBranch.Where(X => X.Code == mLrMaster.Dest).Select(X => X.Name).FirstOrDefault(),
                    FromLocation = ctxTFAT.TfatBranch.Where(X => X.Code == mLrMaster.Source).Select(X => X.Name).FirstOrDefault(),
                    TotalQty = mLrMaster.TotQty,
                    BookNarr = Model.BookNarr,
                    BillNarr = Model.BillNarr,
                    ChgWt = mLrMaster.ChgWt,
                    ActWt = mLrMaster.ActWt,
                    ChargeType = mLrMaster.ChgType,
                    //Freight = Model.Freight,
                    LRDocDate = ConvertDDMMYYTOYYMMDD(mLrMaster.BookDate.ToShortDateString()),
                    POD = Model.POD,
                    LockQty = Model.LockQty,
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
                    item.LRDocDate = ConvertDDMMYYTOYYMMDD(mLrMaster.BookDate.ToShortDateString());
                    item.POD = Model.POD;
                    item.LRRefTableKey = mLrMaster.TableKey;
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
                    mchgamt += i.ChgPickupList[ai];
                }
                mCharges2[ai] = mchgamt;
            }
            Model.TotalChgPickupList = mCharges2.ToList();
            Session.Add("LONewItemlist", AddLRList);
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
            var result2 = (List<LRInvoiceVM>)Session["LONewItemlist"];
            var result = result2.Where(x => x.tempId != Model.tempId).ToList();
            Session.Add("LONewItemlist", result);
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

        #endregion

        #region Save Update

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
            var PODNO = GetNewCode();
            string mTable = "";
            string brMessage = "";
            var Setup = ctxTFAT.LRBillSetup.FirstOrDefault();
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
                            var result1 = ctxTFAT.DocTypes.Where(x => x.Code == "SLR00").Select(x => x).FirstOrDefault();
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
                        Model.ParentKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + Model.Srl.ToString(); ;
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
                        AlertNoteMaster alertNoteMaster = ctxTFAT.AlertNoteMaster.Where(x => x.Type == "SLR00" && x.ParentKey.Trim() == Model.ParentKey.Trim() && x.Stop.Contains("SLR00")).FirstOrDefault();
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
                        vM.Type = "SLR00";
                        SaveAttachment(vM);

                        SaveNarrationAdd(Model.Srl.ToString(), Model.ParentKey);

                    }

                    //if (Model.Authorise.Substring(0, 1) == "X" || Model.Authorise.Substring(0, 1) == "R" || Model.Mode == "Add" || (Model.Mode == "Edit" && Model.AuthAgain))
                    //{
                    //    if (Model.Authorise.Substring(0, 1) != "N") //if authorised then check for the RateDiff Auth. Rule
                    //        Model.Authorise = (Model.AuthReq == true ? GetAuthorise(Model.Type, 0, Model.Branch) : Model.Authorise = "A00");
                    //}

                    #region Authorisation
                    TfatUserAuditHeader authorisation = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == "SLR00").FirstOrDefault();
                    if (authorisation != null)
                    {
                        mauthorise = SetAuthorisationLogistics(authorisation, Model.ParentKey, Model.Srl.ToString(), 0, Model.DocuDate, Model.Amt, Model.Account, mbranchcode);
                    }
                    #endregion

                    List<LRInvoiceVM> result = new List<LRInvoiceVM>();
                    result = (List<LRInvoiceVM>)Session["LONewItemlist"];
                    if (Model.Mode == "Add" || Model.Mode == "Edit")
                    {
                        TempData["PreviousDate"] = Model.DocuDate;
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
                                mobj1.Remark = Model.Remark;
                                mobj1.CPerson = Model.CPerson;
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
                                xCnt = 1;
                                List<PODMaster> pODs = new List<PODMaster>();
                                foreach (var li in list)
                                {
                                    LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.Trim() == li.LRRefTableKey.Trim()).FirstOrDefault();
                                    if (lRMaster != null)
                                    {
                                        if (lRMaster.BillParty != Model.Account)
                                        {
                                            lRMaster.BillParty = Model.Account;
                                            ctxTFAT.Entry(lRMaster).State = EntityState.Modified;
                                        }
                                    }
                                    LRBill lb = new LRBill();
                                    lb.LRRefTablekey = li.LRRefTableKey;
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
                                    lb.AUTHORISE = mauthorise;
                                    lb.AUTHIDS = muserid;
                                    lb.TableKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString();
                                    lb.POD = li.POD;
                                    ctxTFAT.LRBill.Add(lb);

                                    if (li.POD == true)
                                    {
                                        var mlrmaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == li.LRRefTableKey).Select(x => x).FirstOrDefault();
                                        if (pODs.Count() == 0)
                                        {
                                            PODMaster pod = new PODMaster();
                                            pod.CreateDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                            //pod.PODNo = Convert.ToInt32(GetNewCode()); 
                                            pod.PODNo = Convert.ToInt32(PODNO);
                                            pod.CurrentBranch = Model.Branch;
                                            pod.FromBranch = Model.Branch;
                                            pod.Task = "Customer";
                                            pod.SendReceive = "C";
                                            pod.ModuleName = "POD Send Customer";
                                            pod.Prefix = Model.Prefix;
                                            //pod.TableKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString();
                                            pod.TableKey = mbranchcode + "PODS0" + mperiod.Substring(0, 2) + "001" + pod.PODNo;
                                            pod.ParentKey = Model.ParentKey;
                                            pod.PODDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                            pod.PODTime = DateTime.Now.ToString("HH:mm");
                                            pod.PODRemark = "Send POD At Time Bill Generate...! " + Model.Type + Model.Prefix.Substring(0, 2) + Model.Srl.ToString();
                                            pod.AUTHIDS = muserid;
                                            pod.AUTHORISE = "A00";
                                            pod.CustCode = Model.Account;
                                            pod.DeliveryDate = null;
                                            pod.DeliveryNo = null;
                                            pod.DeliveryRemark = null;
                                            pod.DeliveryTime = null;
                                            pod.ENTEREDBY = muserid;
                                            pod.LASTUPDATEDATE = DateTime.Now;
                                            pODs.Add(pod);
                                        }
                                        var GetPODmaster = pODs.FirstOrDefault();
                                        //ListOfPod listOfPod = PODList.FirstOrDefault();
                                        PODRel podrel = new PODRel();
                                        podrel.Task = "Customer";
                                        podrel.SendReceive = "C";
                                        podrel.PODNo = GetPODmaster.PODNo.Value;
                                        podrel.Sno = xCnt;
                                        podrel.LrNo = Convert.ToInt32(li.Code);
                                        podrel.FromBranch = Model.Branch;
                                        podrel.ToBranch = Model.Branch;
                                        podrel.TableKey = mbranchcode + "PODRL" + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + GetPODmaster.PODNo;
                                        podrel.ParentKey = GetPODmaster.TableKey;
                                        podrel.LRRefTablekey = mlrmaster.TableKey;
                                        //var GetLastPOD = ctxTFAT.PODRel.OrderByDescending(x => x.TableKey).Where(x => x.LRRefTablekey == mlrmaster.TableKey).FirstOrDefault();
                                        var GetLastPOD = ctxTFAT.PODRel.OrderByDescending(x => x.RECORDKEY).Where(x => x.LRRefTablekey == mlrmaster.TableKey && (x.SendReceive == "R" || x.SendReceive == "S")).FirstOrDefault();
                                        if (GetLastPOD != null)
                                        {
                                            podrel.PODRefTablekey = GetLastPOD.TableKey;
                                        }
                                        podrel.AUTHIDS = muserid;
                                        podrel.AUTHORISE = "A00";
                                        podrel.ENTEREDBY = muserid;
                                        podrel.LASTUPDATEDATE = DateTime.Now;
                                        podrel.Prefix = Model.Prefix;
                                        ctxTFAT.PODRel.Add(podrel);
                                    }
                                    ++xCnt;
                                }
                                if (pODs.Count() > 0)
                                {
                                    ctxTFAT.PODMaster.AddRange(pODs);
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
                            Model.Authorise = mauthorise;
                            SaveTDSPayments(Model);
                        }

                        ctxTFAT.SaveChanges();
                        transaction.Commit();
                        transaction.Dispose();
                        //var mpods = ctxTFAT.PODMaster.Where(x => x.ParentKey == (Model.Type + Model.Prefix.Substring(0, 2) + Model.Srl)).Select(x => x).ToList();
                        //if (mpods != null && mpods.Count > 0)
                        //{
                        //    foreach (var p in mpods)
                        //    {
                        //        var msno = p.TableKey.Substring(7, 3);
                        //        p.TableKey = "POS00" + Model.Prefix.Substring(0, 2) + msno + p.RECORDKEY.ToString("D6");
                        //        p.ParentKey = "POS00" + Model.Prefix.Substring(0, 2) + p.RECORDKEY.ToString("D6");
                        //        ctxTFAT.Entry(p).State = EntityState.Modified;
                        //        ctxTFAT.SaveChanges();
                        //    }
                        //}
                        //var mpodrels = ctxTFAT.PODRel.Where(x => x.ParentKey == (Model.Type + Model.Prefix.Substring(0, 2) + Model.Srl)).Select(x => x).ToList();
                        //if (mpodrels != null && mpodrels.Count > 0)
                        //{
                        //    foreach (var pr in mpodrels)
                        //    {
                        //        var msno = pr.TableKey.Substring(7, 3);
                        //        pr.TableKey = "POS00" + Model.Prefix.Substring(0, 2) + msno + pr.PODNo.ToString("D6");
                        //        ctxTFAT.Entry(pr).State = EntityState.Modified;
                        //        ctxTFAT.SaveChanges();
                        //    }
                        //}
                        UpdateAuditTrail(Model.Branch, Model.Mode, Model.Header, Model.ParentKey, ConvertDDMMYYTOYYMMDD(Model.DocuDate), Model.Amt, Model.Account, "Save Freight Bill", "CA");
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
                        //if (Model.Authorise != "X00")
                        //{
                        //    SendTrnsMsg(Model.Mode, Model.Amt, Model.Branch + Model.ParentKey, ConvertDDMMYYTOYYMMDD(Model.DocuDate), Model.Account);
                        //}

                        if (Model.SaveAsDraft != "Y")
                        {
                            Session["LONewItemlist"] = null;
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

        public ActionResult DeleteData(LRInvoiceVM Model)
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
            if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.LockDate == Date && x.Type == "SLR00").FirstOrDefault() != null)
            {
                return Json(new
                {
                    Status = "Error",
                    Message = "Selected Document Date is Lock by Period Locking System.."
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
                        var RemoveAttach = ctxTFAT.Attachment.Where(x => x.Type == "SLR00" && x.Srl == mobj1.Srl).ToList();
                        foreach (var item in RemoveAttach)
                        {
                            if (System.IO.File.Exists(item.FilePath))
                            {
                                System.IO.File.Delete(item.FilePath);
                            }
                        }
                        ctxTFAT.Attachment.RemoveRange(RemoveAttach);
                        var GetRemarkDocList = ctxTFAT.Narration.Where(x => x.Srl == mobj1.Srl.ToString() && x.Type == "SLR00").ToList();
                        if (GetRemarkDocList != null)
                        {
                            ctxTFAT.Narration.RemoveRange(GetRemarkDocList);
                        }


                    }
                    DeUpdate(Model);
                    UpdateAuditTrail(mbranchcode, "Delete", Model.Header, Model.ParentKey, ConvertDDMMYYTOYYMMDD(Model.DocuDate), Model.Amt, Model.Account, "Delete Freight Bill", "CA");

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
                    att.Type = String.IsNullOrEmpty(item.Type) == true ? "SLR00" : item.Type;
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
            var resultlrlist = (List<LRInvoiceVM>)Session["LONewItemlist"];
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
                var resultw = ctxTFAT.DocTypes.Where(x => x.Code == "SLR00").Select(x => x).FirstOrDefault();
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
            if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.LockDate == Date && x.Type == Model.Type).FirstOrDefault() != null)
            {
                return Json(new
                {

                    Status = "ErrorValid",
                    Message = "Selected Document Date is Lock by Period Locking System.."
                }, JsonRequestBehavior.AllowGet); ;

            }

            if (!(ConvertDDMMYYTOYYMMDD(StartDate) <= Date && Date <= ConvertDDMMYYTOYYMMDD(EndDate)))
            {
                return Json(new
                {

                    Status = "ErrorValid",
                    Message = "Document Date Should Be In  Selected Financial Year..."
                }, JsonRequestBehavior.AllowGet); ;
            }


            var LRNos = resultlrlist.Select(x => x.LRRefTableKey).ToList();

            string Status2 = "Success", Message2 = "";
            var Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == "LR000" && LRNos.Contains(AlertMater.ParentKey) && AlertMater.RefType.Contains("SLR00")
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
                    if (stp.Trim().Contains("SLR00"))
                    {
                        var ConsignmentBookDate = ctxTFAT.LRMaster.Where(x => x.TableKey == item.DocReceivedN).Select(x => x.BookDate).FirstOrDefault();
                        Status2 = "Error";
                        Message2 += item.TypeCode + " Consignment Stopped For The Freight Bill And This Consignment Booked Date Was " + ConsignmentBookDate.ToShortDateString() + " .\nSo We Cannot Allow This Consignment To Freight Bill Please Remove It....!\n";
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
                //    HSNMaster hSNMaster=ctxTFAT.HSNMaster.Where(x=>x.Code== Model.GSTCode).FirstOrDefault();
                //    if (hSNMaster!=null)
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
                LedPostList.Add(new LRInvoiceVM()
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
                            DelyCode = Model.AccParentGrp
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
                            DelyCode = Model.AccParentGrp
                        });
                    }
                }
                n++;
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
                                DelyCode = Model.AccParentGrp
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
                    LedPostList.Add(new LRInvoiceVM() { Code = mCode, AccountName = NameofAccount(mCode), Debit = Math.Round(mDebit, 2), Credit = Math.Round(mCredit, 2), Branch = mbranchcode, FilBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(), tempId = xCnt++ });
                }
                else
                {
                    LedPostList.Add(new LRInvoiceVM() { Code = mCode, AccountName = NameofAccount(mCode), Debit = Math.Round(mCredit, 2), Credit = Math.Round(mDebit, 2), Branch = mbranchcode, FilBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(), tempId = xCnt++ });
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
                //if (mTdsPostCode == null || mTdsPostCode.Trim() == "")
                //{
                //    mTdsPostCode = "";
                //}

                LedPostList.Add(new LRInvoiceVM()
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
                    DelyCode = Model.AccParentGrp
                });

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
            Model.Taxable = Model.Taxable;
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
            decimal mamtm = Math.Round(Model.Taxable + Model.IGSTAmt + Model.SGSTAmt + Model.CGSTAmt, 2);
            //decimal mamtm = Model.Taxable + Model.IGSTAmt + Model.CGSTAmt + Model.SGSTAmt;
            //if (Model.CutTDS == true && Model.LRCutTDS == true)//for tcs
            //{
            //    mamtm = mamtm + Model.TDSAmt;
            //}
            //decimal RoundOff = mamtm - (Model.Taxable + Model.IGSTAmt + Model.SGSTAmt + Model.CGSTAmt);
            return Json(new
            {
                Total = mamtm,
                IGSTAmt = Model.IGSTAmt,
                CGSTAmt = Model.CGSTAmt,
                SGSTAmt = Model.SGSTAmt,
                TDSAmt = Model.TDSAmt,
                //RoundOff= RoundOff,
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

            return decimal.Round(abc, 2, MidpointRounding.AwayFromZero);
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

        public ActionResult GetPickUp(LRInvoiceVM Model)
        {
            List<LRInvoiceVM> objitemlist = new List<LRInvoiceVM>();
            if (Session["LONewItemlist"] != null)
            {
                objitemlist = (List<LRInvoiceVM>)Session["LONewItemlist"];
            }
            var mcharges = ctxTFAT.Charges.Where(x => x.Type == "LR000").Select(x => x.Head).ToArray();
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            var html = ViewHelper.RenderPartialView(this, "PickUp", new LRInvoiceVM() { AllHeaderList = mcharges.ToList(), FilCustomer = Model.FilCustomer, FilBranch = Model.FilBranch });
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
                if (Session["LONewItemlist"] != null)
                {
                    objitemlist = (List<LRInvoiceVM>)Session["LONewItemlist"];
                }
                int mMaxtempid = (objitemlist.Count == 0) ? 0 : objitemlist.Select(x => x.tempId).Max();
                foreach (var c in Model.PickUpList.OrderBy(x => x.tempId))
                {
                    LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey == c.TableKey).FirstOrDefault();
                    var mPrevQty = objitemlist.Where(x => x.TableKey == c.TableKey).Sum(x => (decimal?)x.Qty) ?? 0;
                    var mPendingQty = (ctxTFAT.LRMaster.Where(x => x.TableKey == c.TableKey).Sum(x => (int?)x.TotQty ?? 0) - Math.Abs(ctxTFAT.LRBill.Where(x => x.LRRefTablekey == c.TableKey).Sum(x => (int?)x.TotQty) ?? 0));
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
                        });
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
                        ActWt = lRMaster.ActWt,
                        ChargeType = lRMaster.ChgType,
                        ChgWt = lRMaster.ChgWt,
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
                        POD = c.POD,
                        LRRefTableKey = c.TableKey
                    });
                }
                Session.Add("LONewItemlist", objitemlist);
                Model.Taxable = objitemlist.Sum(x => x.Val1);
                Model.TotalQty = objitemlist.Sum(x => x.Qty);
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
            List<LRInvoiceVM> objitemlist = new List<LRInvoiceVM>();
            if (Session["LONewItemlist"] != null)
            {
                objitemlist = (List<LRInvoiceVM>)Session["LONewItemlist"];
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
            List<LRInvoiceVM> newlist1 = new List<LRInvoiceVM>();
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
            //foreach (var a in Model.Code.Split(','))
            //{
            //    var mLrMaster = ctxTFAT.PODRel.Where(x => x.LRRefTablekey == a && x.SendReceive == "C").Select(x => x).FirstOrDefault();
            //    if (mLrMaster != null)
            //    {
            //        mStatus = "ConfirmError";
            //        mMessage += "LRNo " + mLrMaster.LrNo + " POD is already sended";
            //    }
            //}
            //if (mMessage != "")
            //{
            //    Model.POD = true;
            //}
            Model.POD = false;
            Model.Message = mMessage;
            return Json(Model, JsonRequestBehavior.AllowGet);
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
            var html = ViewHelper.RenderPartialView(this, "MultiPrint", new LRInvoiceVM() { PrintGridList = Model.PrintGridList, Document = Model.Document });
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


        #region Contract RAte

        // ChargeType Wise Contract Methods
        public ActionResult CustomerCharges(LRVM Model)
        {
            bool GeneralContract = false;
            var mLrmaster = ctxTFAT.LRMaster.Where(x => x.TableKey == Model.LRRefTableKey).Select(x => x).FirstOrDefault();
            Model.BillParty = mLrmaster.BillParty;
            Model.Source = mLrmaster.Source;
            Model.Dest = mLrmaster.Dest;
            Model.ChgType = mLrmaster.ChgType;
            Model.ActWt = mLrmaster.ActWt;
            Model.ChgWt = mLrmaster.ChgWt;
            Model.TotQty = mLrmaster.TotQty;
            Model.DescrType = mLrmaster.DescrType;
            Model.BookDate = mLrmaster.BookDate.ToShortDateString();

            var FromDt = ConvertDDMMYYTOYYMMDD(Model.BookDate);
            ContractList SingleContract = new ContractList();
            List<ContractList> WeightCartoon = new List<ContractList>();
            ConMaster conMaster = new ConMaster();
            if (Model.ChgType == "100000" || Model.ChgType == "100001")
            {
                //Extact Match From And TO
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
                        FromBranch = Model.Source;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }
                    //Extact Match From And TO(Parent)
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

                    if (WeightCartoon == null || WeightCartoon.Count() == 0)
                    {
                        FromBranch = Model.Source; Tobranch = Model.Dest;
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            FromBranch = tfatBranch.Grp;
                        }
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            Tobranch = Model.Dest;
                        }
                        //Extact Match From(Parent) And TO
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

                        if (WeightCartoon == null || WeightCartoon.Count() == 0)
                        {
                            FromBranch = Model.Source; Tobranch = Model.Dest;
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                FromBranch = tfatBranch.Grp;
                            }
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                Tobranch = tfatBranch.Grp;
                            }
                            //Extact Match From(Parent) And TO(Parent)
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
                                item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                                SingleContract.Charges = item.Charges;
                                break;
                            }
                        }
                        else
                        {
                            SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                            if (SingleContract != null)
                            {
                                item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
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
                            item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }

                }

                if (SingleContract != null)
                {
                    if (SingleContract.Service != null)
                    {
                        var Rate = SingleContract.Rate;
                        if (SingleContract.ChargeOfChrgWT)
                        {
                            foreach (var item in SingleContract.Charges.ToList())
                            {
                                var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                            }
                        }
                        else
                        {
                            foreach (var item in SingleContract.Charges.ToList())
                            {
                                var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                            }
                        }
                        Model.Charges = SingleContract.Charges;


                    }
                }
            }
            else
            {   //Extact Match From And TO
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
                        FromBranch = Model.Source;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }
                    //Extact Match From And TO(Parent)
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

                    if (SingleContract.Service == null)
                    {
                        FromBranch = Model.Source; Tobranch = Model.Dest;
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            FromBranch = tfatBranch.Grp;
                        }
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            Tobranch = Model.Dest;
                        }
                        //Extact Match From(Parent) And TO
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

                        if (SingleContract.Service == null)
                        {
                            FromBranch = Model.Source; Tobranch = Model.Dest;
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                FromBranch = tfatBranch.Grp;
                            }
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                Tobranch = tfatBranch.Grp;
                            }
                            //Extact Match From(Parent) And TO(Parent)
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
                    }
                }
                if (SingleContract != null)
                {
                    if (SingleContract.Service != null)
                    {
                        SingleContract.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);

                        if (SingleContract.ChargeOfChrgWT)
                        {
                            foreach (var item in SingleContract.Charges.ToList())
                            {
                                var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                            }
                        }
                        else
                        {
                            foreach (var item in SingleContract.Charges.ToList())
                            {
                                var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                            }
                        }
                        Model.Charges = SingleContract.Charges;
                    }
                }
            }


            string Status = "", Message = "", RateChrgOn = "", MRate = "", RateType = "";

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
                        RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                        MRate = SingleContract.Rate.ToString();
                        RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

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
                        RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                        MRate = SingleContract.Rate.ToString();
                        RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

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
            var jsonResult = Json(new
            {
                Status = Status,
                Message = Message,
                ChargesListSelect = ChargesListSelect

            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public ActionResult Charges(LRVM Model)
        {
            bool GeneralContract = false;
            var mLrmaster = ctxTFAT.LRMaster.Where(x => x.TableKey == Model.LRRefTableKey).Select(x => x).FirstOrDefault();
            Model.BillParty = mLrmaster.BillParty;
            Model.Source = mLrmaster.Source;
            Model.Dest = mLrmaster.Dest;
            Model.ChgType = mLrmaster.ChgType;
            Model.ActWt = mLrmaster.ActWt;
            Model.ChgWt = mLrmaster.ChgWt;
            Model.TotQty = mLrmaster.TotQty;
            Model.DescrType = mLrmaster.DescrType;
            Model.BookDate = mLrmaster.BookDate.ToShortDateString();

            var FromDt = ConvertDDMMYYTOYYMMDD(Model.BookDate);
            Model.BillParty = ctxTFAT.CustomerMaster.Where(x => x.Code == Model.BillParty).Select(x => x.AccountParentGroup).FirstOrDefault();
            ContractList SingleContract = new ContractList();
            List<ContractList> WeightCartoon = new List<ContractList>();
            ConMaster conMaster = new ConMaster();
            if (Model.ChgType == "100000" || Model.ChgType == "100001")
            {
                //Extact Match From And TO
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
                        FromBranch = Model.Source;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }
                    //Extact Match From And TO(P)
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

                    if (WeightCartoon == null || WeightCartoon.Count() == 0)
                    {
                        FromBranch = Model.Source; Tobranch = Model.Dest;
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            FromBranch = tfatBranch.Grp;
                        }
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            Tobranch = Model.Dest;
                        }
                        //Extact Match From(P) And TO
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

                        if (WeightCartoon == null || WeightCartoon.Count() == 0)
                        {
                            FromBranch = Model.Source; Tobranch = Model.Dest;
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                FromBranch = tfatBranch.Grp;
                            }
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                Tobranch = tfatBranch.Grp;
                            }
                            //Extact Match From(P) And TO(P)
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
                                item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                                SingleContract.Charges = item.Charges;
                                break;
                            }
                        }
                        else
                        {
                            SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                            if (SingleContract != null)
                            {
                                item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
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
                            item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }

                }

                if (SingleContract != null)
                {
                    if (SingleContract.Service != null)
                    {

                        var Rate = SingleContract.Rate;

                        if (SingleContract.ChargeOfChrgWT)
                        {
                            foreach (var item in SingleContract.Charges.ToList())
                            {
                                var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                            }
                        }
                        else
                        {
                            foreach (var item in SingleContract.Charges.ToList())
                            {
                                var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                            }
                        }
                        Model.Charges = SingleContract.Charges;
                    }
                }
            }
            else
            {
                //Extact Match From And TO
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
                        FromBranch = Model.Source;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }
                    //Extact Match From And TO(P)
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

                    if (SingleContract.Service == null)
                    {
                        FromBranch = Model.Source; Tobranch = Model.Dest;
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            FromBranch = tfatBranch.Grp;
                        }
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            Tobranch = Model.Dest;
                        }
                        //Extact Match From(P) And TO
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

                        if (SingleContract.Service == null)
                        {
                            FromBranch = Model.Source; Tobranch = Model.Dest;
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                FromBranch = tfatBranch.Grp;
                            }
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                Tobranch = tfatBranch.Grp;
                            }
                            //Extact Match From(P) And TO(P)
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
                    }

                }
                if (SingleContract != null)
                {
                    if (SingleContract.Service != null)
                    {
                        SingleContract.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);

                        if (SingleContract.ChargeOfChrgWT)
                        {
                            foreach (var item in SingleContract.Charges.ToList())
                            {
                                var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                            }
                        }
                        else
                        {
                            foreach (var item in SingleContract.Charges.ToList())
                            {
                                var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                            }
                        }
                        Model.Charges = SingleContract.Charges;
                    }
                }
            }


            string Status = "", Message = "", RateChrgOn = "", MRate = "", RateType = "";
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
                        RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                        MRate = SingleContract.Rate.ToString();
                        RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

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
                        RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                        MRate = SingleContract.Rate.ToString();
                        RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

                    }
                }
                else
                {
                    Status = "Error";
                    Message = "Contract Not Found";
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
            var jsonResult = Json(new
            {
                Status = Status,
                Message = Message,
                ChargesListSelect = ChargesListSelect

            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public ActionResult CustomerOTherContract(LRVM Model)
        {
            var mLrmaster = ctxTFAT.LRMaster.Where(x => x.TableKey == Model.LRRefTableKey).Select(x => x).FirstOrDefault();
            Model.BillParty = mLrmaster.BillParty;
            Model.Source = mLrmaster.Source;
            Model.Dest = mLrmaster.Dest;
            Model.ChgType = mLrmaster.ChgType;
            Model.ActWt = mLrmaster.ActWt;
            Model.ChgWt = mLrmaster.ChgWt;
            Model.TotQty = mLrmaster.TotQty;
            Model.DescrType = mLrmaster.DescrType;
            Model.BookDate = mLrmaster.BookDate.ToShortDateString();

            var FromDt = ConvertDDMMYYTOYYMMDD(Model.BookDate);
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



            if (Weight)
            {
                OthCode = "100001";
                OthName = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == OthCode).Select(x => x.ChargeType).FirstOrDefault();

                //Extact Match From And TO
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
                    string FromBranch = Model.Source, Tobranch = Model.Dest;
                    TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        FromBranch = Model.Source;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }
                    //Extact Match From And TO(P)
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

                    if (WeightCartoon == null || WeightCartoon.Count() == 0)
                    {
                        FromBranch = Model.Source; Tobranch = Model.Dest;
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            FromBranch = tfatBranch.Grp;
                        }
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            Tobranch = Model.Dest;
                        }
                        //Extact Match From(P) And TO
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

                        if (WeightCartoon == null || WeightCartoon.Count() == 0)
                        {
                            FromBranch = Model.Source; Tobranch = Model.Dest;
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                FromBranch = tfatBranch.Grp;
                            }
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                Tobranch = tfatBranch.Grp;
                            }
                            //Extact Match From(P) And TO(P)
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
                }
            }
            else
            {
                OthCode = "100000";
                OthName = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == OthCode).Select(x => x.ChargeType).FirstOrDefault();
                //Extact Match From And TO
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
                    string FromBranch = Model.Source, Tobranch = Model.Dest;
                    TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        FromBranch = Model.Source;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }
                    //Extact Match From And TO(P)
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

                    if (WeightCartoon == null || WeightCartoon.Count() == 0)
                    {
                        FromBranch = Model.Source; Tobranch = Model.Dest;
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            FromBranch = tfatBranch.Grp;
                        }
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            Tobranch = Model.Dest;
                        }
                        //Extact Match From(P) And TO
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

                        if (WeightCartoon == null || WeightCartoon.Count() == 0)
                        {
                            FromBranch = Model.Source; Tobranch = Model.Dest;
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                FromBranch = tfatBranch.Grp;
                            }
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                Tobranch = tfatBranch.Grp;
                            }
                            //Extact Match From(P) And TO(P)
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
                            item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }
                    else
                    {
                        SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                        if (SingleContract != null)
                        {
                            item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
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
                        item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                        SingleContract.Charges = item.Charges;
                        break;
                    }
                }

            }

            if (SingleContract != null)
            {
                if (SingleContract.Service != null)
                {

                    var Rate = SingleContract.Rate;
                    if (SingleContract.ChargeOfChrgWT)
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                        }
                    }
                    else
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                        }
                    }
                    Model.Charges = SingleContract.Charges;


                }
            }


            string Status = "", Message = "", RateChrgOn = "", MRate = "", RateType = "";
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
                    RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                    MRate = SingleContract.Rate.ToString();
                    RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

                }
            }
            else
            {
                Status = "Error";
                Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";
            }


            List<decimal> ChargesListSelect = new List<decimal>();
            if (Model.Charges != null)
            {
                foreach (var item in Model.Charges)
                {
                    ChargesListSelect.Add(item.Val1);
                }
            }
            var jsonResult = Json(new
            {
                Status = Status,
                Message = Message,
                ChargesListSelect = ChargesListSelect

            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public ActionResult OTherContract(LRVM Model)
        {
            var mLrmaster = ctxTFAT.LRMaster.Where(x => x.TableKey == Model.LRRefTableKey).Select(x => x).FirstOrDefault();
            Model.BillParty = mLrmaster.BillParty;
            Model.Source = mLrmaster.Source;
            Model.Dest = mLrmaster.Dest;
            Model.ChgType = mLrmaster.ChgType;
            Model.ActWt = mLrmaster.ActWt;
            Model.ChgWt = mLrmaster.ChgWt;
            Model.TotQty = mLrmaster.TotQty;
            Model.DescrType = mLrmaster.DescrType;
            Model.BookDate = mLrmaster.BookDate.ToShortDateString();

            var FromDt = ConvertDDMMYYTOYYMMDD(Model.BookDate);
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



            if (Weight)
            {
                OthCode = "100001";
                OthName = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == OthCode).Select(x => x.ChargeType).FirstOrDefault();
                //Extact Match From And TO
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
                    string FromBranch = Model.Source, Tobranch = Model.Dest;
                    TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        FromBranch = Model.Source;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }
                    //Extact Match From And TO(P)
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

                    if (WeightCartoon == null || WeightCartoon.Count() == 0)
                    {
                        FromBranch = Model.Source; Tobranch = Model.Dest;
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            FromBranch = tfatBranch.Grp;
                        }
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            Tobranch = Model.Dest;
                        }
                        //Extact Match From(P) And TO
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

                        if (WeightCartoon == null || WeightCartoon.Count() == 0)
                        {
                            FromBranch = Model.Source; Tobranch = Model.Dest;
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                FromBranch = tfatBranch.Grp;
                            }
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                Tobranch = tfatBranch.Grp;
                            }
                            //Extact Match From(P) And TO(P)
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
                }
            }
            else
            {
                OthCode = "100000";
                OthName = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == OthCode).Select(x => x.ChargeType).FirstOrDefault();
                //Extact Match From And TO
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
                    string FromBranch = Model.Source, Tobranch = Model.Dest;
                    TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        FromBranch = Model.Source;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }
                    //Extact Match From And TO(P)
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

                    if (WeightCartoon == null || WeightCartoon.Count() == 0)
                    {
                        FromBranch = Model.Source; Tobranch = Model.Dest;
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            FromBranch = tfatBranch.Grp;
                        }
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            Tobranch = Model.Dest;
                        }
                        //Extact Match From(P) And TO
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

                        if (WeightCartoon == null || WeightCartoon.Count() == 0)
                        {
                            FromBranch = Model.Source; Tobranch = Model.Dest;
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                FromBranch = tfatBranch.Grp;
                            }
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                Tobranch = tfatBranch.Grp;
                            }
                            //Extact Match From(P) And TO(P)
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
                            item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }
                    else
                    {
                        SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                        if (SingleContract != null)
                        {
                            item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
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
                        item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                        SingleContract.Charges = item.Charges;
                        break;
                    }
                }

            }

            if (SingleContract != null)
            {
                if (SingleContract.Service != null)
                {

                    var Rate = SingleContract.Rate;
                    if (SingleContract.ChargeOfChrgWT)
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                        }
                    }
                    else
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                        }
                    }
                    Model.Charges = SingleContract.Charges;


                }
            }


            string Status = "", Message = "", RateChrgOn = "", MRate = "", RateType = "";
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
                    RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                    MRate = SingleContract.Rate.ToString();
                    RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

                }
            }
            else
            {
                Status = "Error";
                Message = "Contract Not Found";
            }


            List<decimal> ChargesListSelect = new List<decimal>();
            if (Model.Charges != null)
            {
                foreach (var item in Model.Charges)
                {
                    ChargesListSelect.Add(item.Val1);
                }
            }
            var jsonResult = Json(new
            {
                Status = Status,
                Message = Message,
                ChargesListSelect = ChargesListSelect

            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public ActionResult GeneralCharges(LRVM Model)
        {
            bool GeneralContract = false;
            var mLrmaster = ctxTFAT.LRMaster.Where(x => x.TableKey == Model.LRRefTableKey).Select(x => x).FirstOrDefault();
            Model.BillParty = mLrmaster.BillParty;
            Model.Source = mLrmaster.Source;
            Model.Dest = mLrmaster.Dest;
            Model.ChgType = mLrmaster.ChgType;
            Model.ActWt = mLrmaster.ActWt;
            Model.ChgWt = mLrmaster.ChgWt;
            Model.TotQty = mLrmaster.TotQty;
            Model.DescrType = mLrmaster.DescrType;
            Model.BookDate = mLrmaster.BookDate.ToShortDateString();

            var FromDt = ConvertDDMMYYTOYYMMDD(Model.BookDate);
            ContractList SingleContract = new ContractList();
            List<ContractList> WeightCartoon = new List<ContractList>();
            ConMaster conMaster = new ConMaster();
            if (Model.ChgType == "100000" || Model.ChgType == "100001")
            {
                #region Customer Check
                //Extact Match From And TO
                conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();

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
                        FromBranch = Model.Source;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }
                    //Extact Match From And TO(P)
                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
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

                    if (WeightCartoon == null || WeightCartoon.Count() == 0)
                    {
                        FromBranch = Model.Source; Tobranch = Model.Dest;
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            FromBranch = tfatBranch.Grp;
                        }
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            Tobranch = Model.Dest;
                        }
                        //Extact Match From(P) And TO
                        conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
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

                        if (WeightCartoon == null || WeightCartoon.Count() == 0)
                        {
                            FromBranch = Model.Source; Tobranch = Model.Dest;
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                FromBranch = tfatBranch.Grp;
                            }
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                Tobranch = tfatBranch.Grp;
                            }
                            //Extact Match From(P) And TO(P)
                            conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
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
                                item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                                SingleContract.Charges = item.Charges;
                                break;
                            }
                        }
                        else
                        {
                            SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                            if (SingleContract != null)
                            {
                                item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
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
                            item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }

                }

                #endregion

                #region Master Check
                if (SingleContract == null)
                {
                    //Extact Match From And TO
                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();
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
                            FromBranch = Model.Source;
                        }
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            Tobranch = tfatBranch.Grp;
                        }
                        //Extact Match From And TO(P)
                        conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
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

                        if (WeightCartoon == null || WeightCartoon.Count() == 0)
                        {
                            FromBranch = Model.Source; Tobranch = Model.Dest;
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                FromBranch = tfatBranch.Grp;
                            }
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                Tobranch = Model.Dest;
                            }
                            //Extact Match From(P) And TO
                            conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
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

                            if (WeightCartoon == null || WeightCartoon.Count() == 0)
                            {
                                FromBranch = Model.Source; Tobranch = Model.Dest;
                                tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                                if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                                {
                                    FromBranch = tfatBranch.Grp;
                                }
                                tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                                if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                                {
                                    Tobranch = tfatBranch.Grp;
                                }
                                //Extact Match From(P) And TO(P)
                                conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
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
                                    item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                                    SingleContract.Charges = item.Charges;
                                    break;
                                }
                            }
                            else
                            {
                                SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                                if (SingleContract != null)
                                {
                                    item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
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
                                item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                                SingleContract.Charges = item.Charges;
                                break;
                            }
                        }

                    }
                }
                #endregion

                if (SingleContract != null)
                {
                    if (SingleContract.Service != null)
                    {
                        var Rate = SingleContract.Rate;
                        if (SingleContract.ChargeOfChrgWT)
                        {
                            foreach (var item in SingleContract.Charges.ToList())
                            {
                                var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                            }
                        }
                        else
                        {
                            foreach (var item in SingleContract.Charges.ToList())
                            {
                                var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                            }
                        }
                        Model.Charges = SingleContract.Charges;


                    }
                }
            }
            else
            {
                #region Customer 
                //Extact Match From And TO
                conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).FirstOrDefault();
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
                        FromBranch = Model.Source;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }
                    //Extact Match From And TO(P)
                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
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
                        FromBranch = Model.Source; Tobranch = Model.Dest;
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            FromBranch = tfatBranch.Grp;
                        }
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            Tobranch = Model.Dest;
                        }
                        //Extact Match From(P) And TO
                        conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
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
                            FromBranch = Model.Source; Tobranch = Model.Dest;
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                FromBranch = tfatBranch.Grp;
                            }
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                Tobranch = tfatBranch.Grp;
                            }
                            //Extact Match From(P) And TO(P)
                            conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
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
                    }
                }
                #endregion

                #region Master
                if (conMaster == null)
                {
                    //Extact Match From And TO
                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).FirstOrDefault();
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
                            FromBranch = Model.Source;
                        }
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            Tobranch = tfatBranch.Grp;
                        }
                        //Extact Match From And TO(P)
                        conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
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
                            FromBranch = Model.Source; Tobranch = Model.Dest;
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                FromBranch = tfatBranch.Grp;
                            }
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                Tobranch = Model.Dest;
                            }
                            //Extact Match From(P) And TO
                            conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
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
                                FromBranch = Model.Source; Tobranch = Model.Dest;
                                tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                                if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                                {
                                    FromBranch = tfatBranch.Grp;
                                }
                                tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                                if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                                {
                                    Tobranch = tfatBranch.Grp;
                                }
                                //Extact Match From(P) And TO(P)
                                conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
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
                        }
                    }

                    #endregion

                    if (SingleContract != null)
                    {
                        if (SingleContract.Service != null)
                        {
                            SingleContract.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                            if (SingleContract.ChargeOfChrgWT)
                            {
                                foreach (var item in SingleContract.Charges.ToList())
                                {
                                    var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                    SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                                }
                            }
                            else
                            {
                                foreach (var item in SingleContract.Charges.ToList())
                                {
                                    var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                    SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                                }
                            }
                            Model.Charges = SingleContract.Charges;
                        }
                    }

                }

            }

            string Status = "", Message = "", RateChrgOn = "", MRate = "", RateType = "";
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
                        RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                        MRate = SingleContract.Rate.ToString();
                        RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

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
                        RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                        MRate = SingleContract.Rate.ToString();
                        RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

                    }
                }
                else
                {
                    Status = "Error";
                    Message = "Contract Not Found";
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
            var jsonResult = Json(new
            {
                Status = Status,
                Message = Message,
                ChargesListSelect = ChargesListSelect

            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public ActionResult GeneralOTherContract(LRVM Model)
        {
            var mLrmaster = ctxTFAT.LRMaster.Where(x => x.TableKey == Model.LRRefTableKey).Select(x => x).FirstOrDefault();
            Model.BillParty = mLrmaster.BillParty;
            Model.Source = mLrmaster.Source;
            Model.Dest = mLrmaster.Dest;
            Model.ChgType = mLrmaster.ChgType;
            Model.ActWt = mLrmaster.ActWt;
            Model.ChgWt = mLrmaster.ChgWt;
            Model.TotQty = mLrmaster.TotQty;
            Model.DescrType = mLrmaster.DescrType;
            Model.BookDate = mLrmaster.BookDate.ToShortDateString();

            var FromDt = ConvertDDMMYYTOYYMMDD(Model.BookDate);
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



            if (Weight)
            {
                OthCode = "100001";
                OthName = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == OthCode).Select(x => x.ChargeType).FirstOrDefault();
                //Extact Match From And TO
                ConMaster conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).FirstOrDefault();
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
                    string FromBranch = Model.Source, Tobranch = Model.Dest;
                    TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        FromBranch = Model.Source;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }
                    //Extact Match From And TO(P)
                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
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
                        FromBranch = Model.Source; Tobranch = Model.Dest;
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            FromBranch = tfatBranch.Grp;
                        }
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            Tobranch = Model.Dest;
                        }
                        //Extact Match From(P) And TO
                        conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
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
                            FromBranch = Model.Source; Tobranch = Model.Dest;
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                FromBranch = tfatBranch.Grp;
                            }
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                Tobranch = tfatBranch.Grp;
                            }
                            //Extact Match From(P) And TO(P)
                            conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
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
                }
            }
            else
            {
                OthCode = "100000";
                OthName = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == OthCode).Select(x => x.ChargeType).FirstOrDefault();
                //Extact Match From And TO
                ConMaster conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).FirstOrDefault();
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
                    string FromBranch = Model.Source, Tobranch = Model.Dest;
                    TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        FromBranch = Model.Source;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }
                    //Extact Match From And TO(P)
                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
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
                        FromBranch = Model.Source; Tobranch = Model.Dest;
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            FromBranch = tfatBranch.Grp;
                        }
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            Tobranch = Model.Dest;
                        }
                        //Extact Match From(P) And TO
                        conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
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
                            FromBranch = Model.Source; Tobranch = Model.Dest;
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                FromBranch = tfatBranch.Grp;
                            }
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                Tobranch = tfatBranch.Grp;
                            }
                            //Extact Match From(P) And TO(P)
                            conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
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
                            item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }
                    else
                    {
                        SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                        if (SingleContract != null)
                        {
                            item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
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
                        item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                        SingleContract.Charges = item.Charges;
                        break;
                    }
                }

            }

            if (SingleContract != null)
            {
                if (SingleContract.Service != null)
                {
                    var Rate = SingleContract.Rate;
                    if (SingleContract.ChargeOfChrgWT)
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                        }
                    }
                    else
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                        }
                    }
                    Model.Charges = SingleContract.Charges;
                }
            }

            string Status = "", Message = "", RateChrgOn = "", MRate = "", RateType = "";
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
                    RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                    MRate = SingleContract.Rate.ToString();
                    RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

                }
            }
            else
            {
                Status = "Error";
                Message = "Contract Not Found";
            }


            List<decimal> ChargesListSelect = new List<decimal>();
            if (Model.Charges != null)
            {
                foreach (var item in Model.Charges)
                {
                    ChargesListSelect.Add(item.Val1);
                }
            }
            var jsonResult = Json(new
            {
                Status = Status,
                Message = Message,
                ChargesListSelect = ChargesListSelect

            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        // Item/Description Wise Contract Methods
        public ActionResult ItemWiseCustomerCharges(LRVM Model)
        {
            bool GeneralContract = false;
            var mLrmaster = ctxTFAT.LRMaster.Where(x => x.TableKey == Model.LRRefTableKey).Select(x => x).FirstOrDefault();
            Model.BillParty = mLrmaster.BillParty;
            Model.Source = mLrmaster.Source;
            Model.Dest = mLrmaster.Dest;
            Model.ChgType = mLrmaster.ChgType;
            Model.ActWt = mLrmaster.ActWt;
            Model.ChgWt = mLrmaster.ChgWt;
            Model.TotQty = mLrmaster.TotQty;
            Model.DescrType = mLrmaster.DescrType;
            Model.BookDate = mLrmaster.BookDate.ToShortDateString();

            var FromDt = ConvertDDMMYYTOYYMMDD(Model.BookDate);
            ContractList SingleContract = new ContractList();
            List<ContractList> WeightCartoon = new List<ContractList>();
            ConMaster conMaster = new ConMaster();
            //Extact Match From And TO
            conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();
            if (conMaster != null)
            {
                WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                 where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
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
                    FromBranch = Model.Source;
                }
                tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                {
                    Tobranch = tfatBranch.Grp;
                }
                //Extact Match From And TO(P)
                conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                if (conMaster != null)
                {
                    WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                     where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
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
                    FromBranch = Model.Source; Tobranch = Model.Dest;
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        FromBranch = tfatBranch.Grp;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = Model.Dest;
                    }
                    //Extact Match From(P) And TO
                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                    if (conMaster != null)
                    {
                        WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                         where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
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
                        FromBranch = Model.Source; Tobranch = Model.Dest;
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            FromBranch = tfatBranch.Grp;
                        }
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            Tobranch = tfatBranch.Grp;
                        }
                        //Extact Match From(P) And TO(P)
                        conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                        if (conMaster != null)
                        {
                            WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                             where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
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
            }

            foreach (var item in WeightCartoon)
            {

                if (item.ChargeOfChrgWT)
                {
                    SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ChgWt && x.ToWT >= Model.ChgWt).FirstOrDefault();
                    if (SingleContract != null)
                    {
                        item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                        SingleContract.Charges = item.Charges;
                        break;
                    }
                }
                else
                {
                    SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                    if (SingleContract != null)
                    {
                        item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                        SingleContract.Charges = item.Charges;
                        break;
                    }
                }
            }

            if (SingleContract != null)
            {
                if (SingleContract.Service != null)
                {
                    var Rate = SingleContract.Rate;

                    if (SingleContract.ChargeOfChrgWT)
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                        }
                    }
                    else
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                        }
                    }
                    Model.Charges = SingleContract.Charges;

                }
            }


            string Status = "", Message = "", RateChrgOn = "", MRate = "", RateType = "";

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
                    RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                    MRate = SingleContract.Rate.ToString();
                    RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

                }
            }
            else
            {

                Status = "Error";
                Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";


            }

            List<decimal> ChargesListSelect = new List<decimal>();
            if (Model.Charges != null)
            {
                foreach (var item in Model.Charges)
                {
                    ChargesListSelect.Add(item.Val1);
                }
            }
            var jsonResult = Json(new
            {
                Status = Status,
                Message = Message,
                ChargesListSelect = ChargesListSelect

            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public ActionResult ItemWiseMasterCharges(LRVM Model)
        {
            bool GeneralContract = false;
            var mLrmaster = ctxTFAT.LRMaster.Where(x => x.TableKey == Model.LRRefTableKey).Select(x => x).FirstOrDefault();
            Model.BillParty = mLrmaster.BillParty;
            Model.Source = mLrmaster.Source;
            Model.Dest = mLrmaster.Dest;
            Model.ChgType = mLrmaster.ChgType;
            Model.ActWt = mLrmaster.ActWt;
            Model.ChgWt = mLrmaster.ChgWt;
            Model.TotQty = mLrmaster.TotQty;
            Model.DescrType = mLrmaster.DescrType;
            Model.BookDate = mLrmaster.BookDate.ToShortDateString();

            var FromDt = ConvertDDMMYYTOYYMMDD(Model.BookDate);
            ContractList SingleContract = new ContractList();
            List<ContractList> WeightCartoon = new List<ContractList>();
            ConMaster conMaster = new ConMaster();
            //Extact Match From And TO
            conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();
            if (conMaster != null)
            {
                WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                 where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
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
                    FromBranch = Model.Source;
                }
                tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                {
                    Tobranch = tfatBranch.Grp;
                }
                //Extact Match From And TO(P)
                conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                if (conMaster != null)
                {
                    WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                     where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
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
                    FromBranch = Model.Source; Tobranch = Model.Dest;
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        FromBranch = tfatBranch.Grp;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = Model.Dest;
                    }
                    //Extact Match From(P) And TO
                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                    if (conMaster != null)
                    {
                        WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                         where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
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
                        FromBranch = Model.Source; Tobranch = Model.Dest;
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            FromBranch = tfatBranch.Grp;
                        }
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            Tobranch = tfatBranch.Grp;
                        }
                        //Extact Match From(P) And TO(P)
                        conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                        if (conMaster != null)
                        {
                            WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                             where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
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
            }

            foreach (var item in WeightCartoon)
            {

                if (item.ChargeOfChrgWT)
                {
                    SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ChgWt && x.ToWT >= Model.ChgWt).FirstOrDefault();
                    if (SingleContract != null)
                    {
                        item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                        SingleContract.Charges = item.Charges;
                        break;
                    }
                }
                else
                {
                    SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                    if (SingleContract != null)
                    {
                        item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                        SingleContract.Charges = item.Charges;
                        break;
                    }
                }
            }

            if (SingleContract != null)
            {
                if (SingleContract.Service != null)
                {
                    var Rate = SingleContract.Rate;

                    if (SingleContract.ChargeOfChrgWT)
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                        }
                    }
                    else
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                        }
                    }
                    Model.Charges = SingleContract.Charges;

                }
            }

            string Status = "", Message = "", RateChrgOn = "", MRate = "", RateType = "";

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
                    RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                    MRate = SingleContract.Rate.ToString();
                    RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

                }
            }
            else
            {
                Status = "Error";
                Message = "Contract Not Found";
            }


            List<decimal> ChargesListSelect = new List<decimal>();
            if (Model.Charges != null)
            {
                foreach (var item in Model.Charges)
                {
                    ChargesListSelect.Add(item.Val1);
                }
            }
            var jsonResult = Json(new
            {
                Status = Status,
                Message = Message,
                ChargesListSelect = ChargesListSelect

            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public ActionResult GeneralItemWiseCharges(LRVM Model)
        {
            bool GeneralContract = false;
            var mLrmaster = ctxTFAT.LRMaster.Where(x => x.TableKey == Model.LRRefTableKey).Select(x => x).FirstOrDefault();
            Model.BillParty = mLrmaster.BillParty;
            Model.Source = mLrmaster.Source;
            Model.Dest = mLrmaster.Dest;
            Model.ChgType = mLrmaster.ChgType;
            Model.ActWt = mLrmaster.ActWt;
            Model.ChgWt = mLrmaster.ChgWt;
            Model.TotQty = mLrmaster.TotQty;
            Model.DescrType = mLrmaster.DescrType;
            Model.BookDate = mLrmaster.BookDate.ToShortDateString();

            var FromDt = ConvertDDMMYYTOYYMMDD(Model.BookDate);
            ContractList SingleContract = new ContractList();
            List<ContractList> WeightCartoon = new List<ContractList>();
            ConMaster conMaster = new ConMaster();

            #region Customer Check
            //Extact Match From And TO
            conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();

            if (conMaster != null)
            {
                WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                 where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
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
                    FromBranch = Model.Source;
                }
                tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                {
                    Tobranch = tfatBranch.Grp;
                }
                //Extact Match From And TO(P)
                conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();

                if (conMaster != null)
                {
                    WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                     where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
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
                    FromBranch = Model.Source; Tobranch = Model.Dest;
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        FromBranch = tfatBranch.Grp;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = Model.Dest;
                    }
                    //Extact Match From(P) And TO
                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
                    if (conMaster != null)
                    {
                        WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                         where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
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
                        FromBranch = Model.Source; Tobranch = Model.Dest;
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            FromBranch = tfatBranch.Grp;
                        }
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            Tobranch = tfatBranch.Grp;
                        }
                        //Extact Match From(P) And TO(P)
                        conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
                        if (conMaster != null)
                        {
                            WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                             where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
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
            }
            foreach (var item in WeightCartoon)
            {

                if (item.ChargeOfChrgWT)
                {
                    SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ChgWt && x.ToWT >= Model.ChgWt).FirstOrDefault();
                    if (SingleContract != null)
                    {
                        item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                        SingleContract.Charges = item.Charges;
                        break;
                    }
                }
                else
                {
                    SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                    if (SingleContract != null)
                    {
                        item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                        SingleContract.Charges = item.Charges;
                        break;
                    }
                }
            }

            #endregion

            #region Master Check
            if (SingleContract == null)
            {
                //Extact Match From And TO
                conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();

                if (conMaster != null)
                {
                    WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                     where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
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
                        FromBranch = Model.Source;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }
                    //Extact Match From And TO(P)
                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();

                    if (conMaster != null)
                    {
                        WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                         where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
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
                        FromBranch = Model.Source; Tobranch = Model.Dest;
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            FromBranch = tfatBranch.Grp;
                        }
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            Tobranch = Model.Dest;
                        }
                        //Extact Match From(P) And TO
                        conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();

                        if (conMaster != null)
                        {
                            WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                             where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
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
                            FromBranch = Model.Source; Tobranch = Model.Dest;
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                FromBranch = tfatBranch.Grp;
                            }
                            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                            {
                                Tobranch = tfatBranch.Grp;
                            }
                            //Extact Match From(P) And TO(P)
                            conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();

                            if (conMaster != null)
                            {
                                WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                                 where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
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
                }
                foreach (var item in WeightCartoon)
                {

                    if (item.ChargeOfChrgWT)
                    {
                        SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ChgWt && x.ToWT >= Model.ChgWt).FirstOrDefault();
                        if (SingleContract != null)
                        {
                            item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }
                    else
                    {
                        SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                        if (SingleContract != null)
                        {
                            item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }
                }
            }
            #endregion


            if (SingleContract != null)
            {
                if (SingleContract.Service != null)
                {

                    var Rate = SingleContract.Rate;
                    if (SingleContract.ChargeOfChrgWT)
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                        }
                    }
                    else
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                        }
                    }
                    Model.Charges = SingleContract.Charges;


                }
            }

            string Status = "", Message = "", RateChrgOn = "", MRate = "", RateType = "";
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
                    RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                    MRate = SingleContract.Rate.ToString();
                    RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

                }
            }
            else
            {
                Status = "Error";
                Message = "Contract Not Found";
            }



            List<decimal> ChargesListSelect = new List<decimal>();
            if (Model.Charges != null)
            {
                foreach (var item in Model.Charges)
                {
                    ChargesListSelect.Add(item.Val1);
                }
            }
            var jsonResult = Json(new
            {
                Status = Status,
                Message = Message,
                ChargesListSelect = ChargesListSelect

            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        //Common Function Use
        public double GetSingleChargeCalculate(PurchaseVM SingleCharge, int Qty, double Weight)
        {
            double ChrgAmt = 0;

            if (SingleCharge != null)
            {
                if (SingleCharge.Type == "1")
                {
                    ChrgAmt = Convert.ToDouble((Convert.ToDecimal(Convert.ToDouble(SingleCharge.Val1) * Weight)));
                }
                else if (SingleCharge.Type == "2")
                {
                    var Tone = Weight / 10000;
                    ChrgAmt = Math.Round(Convert.ToDouble(SingleCharge.Val1) * Tone);
                }
                else if (SingleCharge.Type == "3")
                {
                    ChrgAmt = Math.Round(Convert.ToDouble(SingleCharge.Val1 * Qty));
                }
                else if (SingleCharge.Type == "4")
                {
                    ChrgAmt = Math.Round(Convert.ToDouble(SingleCharge.Val1));
                }

            }


            return ChrgAmt;
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
                c.Val1 = GetChargeValValue(code, Service, Sno, "Val", c.tempId);
                c.Type = GetChargeValType(code, Service, Sno, "Flg", c.tempId);
                purchases.Add(c);
            }
            return purchases;
        }

        public decimal GetChargeValValue(string code, string Service, int Sno, string Val, int i)
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

        public string GetChargeValType(string code, string Service, int Sno, string Val, int i)
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
                Model.InvoiceAmt = Math.Round(Model.Taxable + Model.CGSTAmt + Model.SGSTAmt + Model.IGSTAmt, 2);
                //Model.RoundOff = Model.InvoiceAmt-(Model.Taxable + Model.CGSTAmt + Model.SGSTAmt + Model.IGSTAmt);

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
                Model.InvoiceAmt = mamtm;
                return Json(Model, JsonRequestBehavior.AllowGet);
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

        #region AlertNote Stop CHeck

        public ActionResult DocumentStopAlertNote(string Type, List<string> TypeCode)
        {
            string Status = "Success", Message = "", Document = "";
            if (TypeCode != null)
            {
                Document = TypeCode[0].ToString();
                var result1 = ctxTFAT.DocTypes.Where(x => x.Code == "SLR00").Select(x => x).FirstOrDefault();
                Document = Document.PadLeft(result1.DocWidth, '0');
            }
            var Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && Document == AlertMater.TypeCode && String.IsNullOrEmpty(AlertMater.Stop) == false
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
                    if (Type.Trim() == stp.Trim())
                    {
                        Status = "Error";
                        Message += item.TypeCode;
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
                            Sales fM = ctxTFAT.Sales.Where(x => x.Srl.ToString() == Model.FMNO && x.Type == "SLR00").FirstOrDefault();
                            var LastSno = ctxTFAT.Narration.Where(x => x.ParentKey == fM.TableKey).ToList().Count();
                            ++LastSno;

                            Narration narr = new Narration();
                            narr.Branch = mbranchcode;
                            narr.Narr = Model.NarrStr;
                            narr.NarrRich = Model.Header;
                            narr.Prefix = mperiod;
                            narr.Sno = LastSno;
                            narr.Srl = fM.Srl.ToString();
                            narr.Type = "SLR00";
                            narr.ENTEREDBY = muserid;
                            narr.LASTUPDATEDATE = DateTime.Now;
                            narr.AUTHORISE = "A00";
                            narr.AUTHIDS = muserid;
                            narr.LocationCode = 0;
                            narr.TableKey = mbranchcode + "SLR00" + mperiod.Substring(0, 2) + LastSno.ToString("D3") + fM.Srl.ToString();
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
                        Narration narration = ctxTFAT.Narration.Where(x => x.Sno.ToString() == mModel.NarrSno.ToString() && x.Type == "SLR00").FirstOrDefault();
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

        public ActionResult dgsg(string No)
        {

            string sourceDocument = "C:\\Users\\Shruham\\Desktop\\TripSheet.pdf";
            string destinationPath = sourceDocument.Replace(".pdf", "_signed.pdf");

            FileStream fileStream = new FileStream("C:\\Users\\Shruham\\Desktop\\Bhanushali\\SUresh.pfx", FileMode.Open);
            //FileStream fileStream = new FileStream("C:\\Users\\Shruham\\Desktop\\TFATDigitalSigner_TemporaryKey.pfx", FileMode.Open);

            SignPdfFile(sourceDocument, destinationPath, fileStream, "Suresh", "reason", "location");

            return null;


        }

        public void SignPdfFile(string sourceDocument, string destinationPath, Stream privateKeyStream, string password, string reason, string location)
        {


            Pkcs12Store pk12 = new Pkcs12Store(privateKeyStream, password.ToCharArray());
            privateKeyStream.Dispose();
            string alias = null;
            foreach (string tAlias in pk12.Aliases)
            {
                if (pk12.IsKeyEntry(tAlias))
                {
                    alias = tAlias;
                    break;
                }
            }
            var pk = pk12.GetKey(alias).Key;
            iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(sourceDocument);
            using (FileStream fout = new FileStream(destinationPath, FileMode.Create, FileAccess.ReadWrite))
            {
                using (iTextSharp.text.pdf.PdfStamper stamper = iTextSharp.text.pdf.PdfStamper.CreateSignature(reader, fout, '\0'))
                {
                    iTextSharp.text.pdf.PdfSignatureAppearance appearance = stamper.SignatureAppearance;
                    iTextSharp.text.pdf.BaseFont bf = iTextSharp.text.pdf.BaseFont.CreateFont(System.Web.HttpContext.Current.Server.MapPath("~/fonts/arial.ttf"), iTextSharp.text.pdf.BaseFont.IDENTITY_H, iTextSharp.text.pdf.BaseFont.EMBEDDED);
                    iTextSharp.text.Font font = new iTextSharp.text.Font(bf, 11);
                    appearance.Layer2Font = font;
                    //appearance.Image = new iTextSharp.text.pdf.PdfImage();
                    appearance.Reason = reason;
                    appearance.Location = location;
                    appearance.SetVisibleSignature(new iTextSharp.text.Rectangle(20, 10, 170, 60), 1, "Icsi-Vendor");
                    iTextSharp.text.pdf.security.IExternalSignature es = new iTextSharp.text.pdf.security.PrivateKeySignature(pk, "SHA-256");
                    iTextSharp.text.pdf.security.MakeSignature.SignDetached(appearance, es, new X509Certificate[] { pk12.GetCertificate(alias).Certificate }, null, null, null, 0, iTextSharp.text.pdf.security.CryptoStandard.CMS);
                    stamper.Close();
                }
            }
        }

        public void Digital()
        {
            // the initial file
            string file = Server.MapPath("~\\Areas\\Accounts\\Views\\LRInvoice\\TripSheet.pdf");

            // the certificate
            string certFile = Server.MapPath("~\\Areas\\Accounts\\Views\\LRInvoice\\selectpdf.pfx");

            // load the pdf document using the advanced security manager
            PdfSecurityManager security = new PdfSecurityManager();
            security.Load(file);

            // encryption algorithm and key length
            security.EncryptionAlgorithm = PdfEncryptionAlgorithm.AES;
            security.EncryptionKeySize = PdfEncryptionKeySize.EncryptKey256Bit;

            // set document passwords
            security.OwnerPassword = "test1";
            security.UserPassword = "test2";

            //set document permissions
            security.CanAssembleDocument = false;
            security.CanCopyContent = true;
            security.CanEditAnnotations = true;
            security.CanEditContent = true;
            security.CanFillFields = true;
            security.CanPrint = true;

            // add the digital signature
            security.Sign(certFile, "selectpdf");

            // save pdf document
            //security.Save("Sample.pdf");
            security.Save(System.Web.HttpContext.Current.Response, false, "Sample.pdf");

            // close pdf document
            security.Close();
            //return "";
        }

        #endregion
    }
}