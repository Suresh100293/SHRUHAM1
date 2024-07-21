using ALT_ERP3.Areas.Logistics.Models;
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

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class BillQueryController : BaseController
    {
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;
        private new string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private new string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        public new string muserid = System.Web.HttpContext.Current.Session["UserId"].ToString();
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        //ITransactionGridOperation mIlst = new TransactionGridOperation();
        private static string mbasegr = "";

        public ActionResult GetAcType(string term)
        {
            List<SelectListItem> maclist = new List<SelectListItem>();

            maclist.Add(new SelectListItem { Value = "SLR00", Text = "Freight Bill" });
            maclist.Add(new SelectListItem { Value = "SLW00", Text = "Freight Bill (No LR)" });
            maclist.Add(new SelectListItem { Value = "CMM00", Text = "Cash Sale" });

            return Json(maclist, JsonRequestBehavior.AllowGet);
        }


        // GET: Logistics/BillQuery
        public ActionResult Index(BillQueryVM Model)
        {
            UpdateAuditTrail(mbranchcode, "Report", Model.Header, "", DateTime.Now, 0, "", "", "NA");
            if (Model.Document == "" || Model.Document == null)
            {
                GetAllMenu(Session["ModuleName"].ToString());

            }
            if (Model.Shortcut)
            {
                Model.BillNo = ctxTFAT.Sales.Where(x => x.TableKey == Model.Tablekey.Trim()).Select(x => x.Srl.ToString()).FirstOrDefault();
                Model.BillType = ctxTFAT.Sales.Where(x => x.TableKey == Model.Tablekey.Trim()).Select(x => x.Type.ToString()).FirstOrDefault();
                Model.Tablekey = Model.Tablekey.Trim();
            }
            else
            {
                Model.BillType = "SLR00";
            }
            ViewBag.ViewDataId = Model.ViewDataId;
            msubcodeof = Model.ViewDataId;
            mbasegr = Model.MainType;
            ViewBag.Header = Model.Header;
            ViewBag.Controller = Model.Controller;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;
            
            return View(Model);
        }

        [HttpPost]
        public ActionResult FetchFreightBillDocumentList(BillQueryVM Model)
        {
            List<BillQueryVM> ValueList = new List<BillQueryVM>();

            var Date = ctxTFAT.TfatPerd.OrderBy(x => x.StartDate).Select(x => x.StartDate).FirstOrDefault();
            var ledgerlist = ctxTFAT.Ledger.Where(x => x.Srl.Trim() == Model.BillNo.Trim() && x.Type == Model.BillType && x.Sno == 1 && x.DocDate < Date).ToList();
            var mlrmaster = ctxTFAT.Sales.Where(x => x.Srl.Trim() == Model.BillNo.Trim() && x.Type == Model.BillType).ToList();

            List<string> TotalDocumentFount = new List<string>();
            TotalDocumentFount.AddRange(mlrmaster.Select(x => x.TableKey.Replace(Model.BillNo,"001"+Model.BillNo)));
            foreach (var item in ledgerlist)
            {
                if (TotalDocumentFount.Where(x => x == item.TableKey).FirstOrDefault() == null)
                {
                    TotalDocumentFount.Add(item.TableKey);
                }
            }

            if (TotalDocumentFount == null)
            {
                Model.Status = "ValidError";
                Model.Message = "Bill Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            if (TotalDocumentFount.Count() == 0)
            {
                Model.Status = "ValidError";
                Model.Message = "Bill Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            else if (TotalDocumentFount.Count() == 1)
            {
                Model.Status = "Processed";
                if (mlrmaster.Count() == 0)
                {
                    Model.Message = ledgerlist.Select(x => x.TableKey).FirstOrDefault();
                    Model.Parentkey = ledgerlist.Select(x => x.ParentKey).FirstOrDefault();
                    Model.FindOpening = true;
                }
                else
                {
                    Model.Message = mlrmaster.Select(x => x.TableKey.Replace(Model.BillNo, "001" + Model.BillNo)).FirstOrDefault();
                    Model.Parentkey = mlrmaster.Select(x => x.TableKey).FirstOrDefault();
                    Model.FindOpening = false;
                }

                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            foreach (var item in mlrmaster)
            {
                BillQueryVM otherTransact = new BillQueryVM();
                otherTransact.BillBranch = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault();
                otherTransact.BMaster = ctxTFAT.Master.Where(x => x.Code == item.CustGroup).Select(x => x.Name).FirstOrDefault();
                otherTransact.Customer = ctxTFAT.CustomerMaster.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault();
                otherTransact.TotAmt = item.Amt.Value.ToString("0.00");
                otherTransact.BillDate = item.DocDate.Value;
                otherTransact.ENTEREDBY = item.ENTEREDBY.ToUpper();
                otherTransact.FindOpening = false;
                otherTransact.Tablekey = item.TableKey.Replace(Model.BillNo, "001" + Model.BillNo);
                otherTransact.Parentkey = item.TableKey;

                ValueList.Add(otherTransact);
            }
            foreach (var item in ledgerlist)
            {
                if (ValueList.Where(x => x.Tablekey == item.TableKey).FirstOrDefault() == null)
                {
                    BillQueryVM otherTransact = new BillQueryVM();
                    otherTransact.BillBranch = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault();
                    otherTransact.BMaster = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault();
                    otherTransact.Customer = ctxTFAT.CustomerMaster.Where(x => x.Code == item.Party).Select(x => x.Name).FirstOrDefault();
                    otherTransact.TotAmt = item.Debit.Value.ToString("0.00");
                    otherTransact.BillDate = item.DocDate;
                    otherTransact.ENTEREDBY = item.ENTEREDBY.ToUpper();
                    otherTransact.FindOpening = true;
                    otherTransact.Tablekey = item.TableKey;
                    otherTransact.Parentkey = item.ParentKey;
                    ValueList.Add(otherTransact);
                }
            }

            Model.ValueList = ValueList;
            var html = ViewHelper.RenderPartialView(this, "MultiBillList", Model);
            return Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetReport(BillQueryVM Model)
        {
            Session["TempAttach"] = null;
            Session["CommnNarrlist"] = null;
            string Status = "Success";
            int docdetail = 0, docdetail1 = 0;
            if (Model.FindOpening)
            {
                string Query = "select SL.Code as CODE,SL.Branch as BillBranch,SL.Tablekey as Tablekey, SL.BillDate as BillDate," +
                    "SL.ENTEREDBY as BillENTEREDBY,SL.LASTUPDATEDATE as BillLASTUPDATEDATE," +
                    " case when SL.Type ='CMM00' then (select C.Name From Master C where C.code = SL.CashBankCode) else (select C.Name From CustomerMaster C where C.code = SL.Party) end as Customer," +
                    "(Select M.Name From Master M Where M.code =  SL.Code) as BMaster,Cast(SL.Debit as Decimal(14, 2)) as TotAmt," +
                    "SL.Narr as Remark,SL.Srl as Srl,SL.Type as Type " +
                    "from Ledger SL where SL.Tablekey = '"+Model.Tablekey+"' ";
                List<DataRow> ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                if (ordersstk.Count() > 0)
                {
                    Model = BillDetails(ordersstk, Model);

                    Query = "select BLS.Party as PartyName,BLS.Branch as SBBranch ,BLS.Doctype as SBType,BLS.DocDate as SBDate,BLS.SubDt as SBSubDt," +
                        "BLS.DocNo as SBDocNo,BLS.Through as SBThrough,BLS.Remark as SBRemark," +
                        "BLS.EnteredBy as SBENTEREDBY,BLS.Lastupdatedate as SBLASTUPDATEDATE " +
                        "from BillSubRef BLR left join BillSubmission BLS on BLS.DocNo = BLR.DocNo " +
                        "where BLR.BillBranch = '"+Model.BillBranch+"' and BLR.BillTableKey = '"+Model.Tablekey+"' ";
                    ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                    Model.billsubmissinDetails = billsubmissinDetails(ordersstk);

                    Query = "select BLS.Branch as SBBranch,BLS.Doctype as SBType,BLS.DocDate as SBDate,BLS.DocNo as  SBDocNo," +
                        "BLS.Remark as SBRemark,BLS.EnteredBy as SBENTEREDBY,BLS.Lastupdatedate  as SBLASTUPDATEDATE," +
                        "case when BLS.DocType='Send' then BLS.FTBranch else '' end as ToBranch," +
                        "case when BLS.DocType='Received' then BLS.FTBranch else '' end as FromBranch " +
                        "from SendReceBillRef BLR left join SendReceBill BLS on BLS.DocNo = BLR.DocNo " +
                        "where BLR.BillBranch = '" + Model.BillBranch + "' and BLR.BillTableKey = '" + Model.Tablekey + "' ";
                    ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                    Model.sendbillsubmissinDetails = ExtrabillsubmissinDetails(ordersstk);

                    //var SaleBill = ctxTFAT.Ledger.Where(x => x.Type.Trim() == Model.BillType.Trim() && x.Srl.Trim() == Model.BillNo.Trim() && x.Prefix!=mperiod).FirstOrDefault();
                    Query = "select D.Branch as Branch,L.DocDate as Date,D.Srl as DocNo,D.Narr as Remark,D.ENTEREDBY as ENTEREDBY,D.LASTUPDATEDATE as LASTUPDATEDATE,D.Type as Type,"
                            + " (select DD.Name From DocTypes DD where DD.code = D.Type) as TypeName ,(select M.Name From Master M where M.code = L.Code) as Code,(select M.Name From Master M where M.code = L.Altcode) as Bank,D.Tablekey + D.Branch as TablekeyBranch"
                            + " from DebtorRecVouDetail D left"
                            + " join Ledger L on L.Parentkey + L.Branch = D.Parentkey + D.Branch"
                            + " where D.InvTableKey = '" + Model.Tablekey + "' and D.InvBranch = '" + Model.BillBranch + "' and L.Sno=1 ";
                    ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                    Model.PaymentReceivedDetails = PaymentReceivedDetails(ordersstk);

                    #region Bill Balance Amount 

                    

                    #endregion
                }
                else
                {
                    Status = "Error";
                }

                //#region Attachment

                //docdetail = ctxTFAT.Attachment.Where(x => x.Srl == Model.BillNo && (x.Type == Model.BillType) && x.Type != "Alert").ToList().Count();
                //docdetail1 = ctxTFAT.AlertNoteMaster.Where(x => x.TypeCode == Model.BillNo && x.Type == Model.BillType).ToList().Count();
                //#endregion
            }
            else
            {
                string Query = "select SL.CustGroup as CODE,SL.Branch as BillBranch,SL.Tablekey as Tablekey, SL.BillDate as BillDate,SL.ENTEREDBY as BillENTEREDBY,SL.LASTUPDATEDATE as BillLASTUPDATEDATE, case when SL.Type ='CMM00' then (select C.Name From Master C where C.code = SL.CashBankCode) else (select C.Name From CustomerMaster C where C.code = SL.Code) end as Customer,(Select M.Name From Master M Where M.code = (select C.AccountParentGroup From CustomerMaster C where C.code = SL.Code)) as BMaster,Cast(SL.Amt as Decimal(14, 2)) as TotAmt,SL.Narr as Remark,SL.Srl as Srl,SL.Type as Type "
                            + " from Sales SL where SL.Tablekey = '" + Model.Parentkey + "' ";
                List<DataRow> ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                if (ordersstk.Count() > 0)
                {
                    Model = BillDetails(ordersstk, Model);

                    Query = "Select LR.Tablekey as Tablekey,LR.Lrno as Lrno,LR.BookDate as LrDate,(CONVERT(VARCHAR(15),CAST(LR.Time AS TIME),100) ) as LrTime,(select T.Name From TfatBranch T where T.code=LR.Source) as LRFrom,(select T.Name From TfatBranch T where T.code=LR.Dest) as LRTo,(select C.Name from Consigner C where C.code=LR.RecCode) as Consignor,(select C.Name from Consigner C where C.code=LR.SendCode) as Consignee,LB.TotQty as LRQty,(select U.Name from unitmaster U where U.code=LR.UnitCode) as Unit,LR.ActWt as ActWt,LR.ChgWt as ChrgWt,(select J.ChargeType From ChargeTypeMaster J where J.code= LR.ChgType )as ChrgType,Cast(LB.Amt as Decimal(14, 2)) as LrAmt,LR.PartyRef as PartyChallan,LR.PartyInvoice as PartyInvoice,LR.PONumber as PONumber,LR.BENumber as BENumber,LR.EwayBill as EawayBill"
                            + " from lrmaster LR"
                            + " left join lrbill LB on LB.LRRefTablekey = LR.Tablekey"
                            + " where LB.ParentKey = '" + Model.Parentkey + "' ";
                    ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                    Model.LorryReceiptList = LorryReceiptDetails(ordersstk, Model.Parentkey);

                    Query = "select BLS.Party as PartyName,BLS.Branch as SBBranch ,BLS.Doctype as SBType,BLS.DocDate as SBDate,BLS.SubDt as SBSubDt,BLS.DocNo as SBDocNo,BLS.Through as SBThrough,BLS.Remark as SBRemark,BLS.EnteredBy as SBENTEREDBY,BLS.Lastupdatedate as SBLASTUPDATEDATE"
                            + " from BillSubRef BLR left"
                            + " join BillSubmission BLS on BLS.DocNo = BLR.DocNo"
                            + " where BLR.BillBranch = '" + Model.BillBranch + "' and BLR.BillTableKey = '" + Model.Tablekey + "'";
                    ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                    Model.billsubmissinDetails = billsubmissinDetails(ordersstk);

                    Query = "select BLS.Branch as SBBranch,BLS.Doctype as SBType,BLS.DocDate as SBDate,BLS.DocNo as  SBDocNo,BLS.Remark as SBRemark,BLS.EnteredBy as SBENTEREDBY,BLS.Lastupdatedate  as SBLASTUPDATEDATE,case when BLS.DocType='Send' then BLS.FTBranch else '' end as ToBranch,case when BLS.DocType='Received' then BLS.FTBranch else '' end as FromBranch"
                            + " from SendReceBillRef BLR left"
                            + " join SendReceBill BLS on BLS.DocNo = BLR.DocNo"
                            + " where BLR.BillBranch = '" + Model.BillBranch + "' and BLR.BillTableKey = '" + Model.Tablekey + "' ";
                    ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                    Model.sendbillsubmissinDetails = ExtrabillsubmissinDetails(ordersstk);

                    //var SaleBill = ctxTFAT.Sales.Where(x => x.Type.Trim() == Model.BillType.Trim() && x.Srl.Trim() == Model.BillNo.Trim()).FirstOrDefault();
                    var SaleBill = ctxTFAT.Ledger.Where(x => x.ParentKey.Trim() == Model.Parentkey.Trim() && x.Sno == 1).Select(x=>x.TableKey ).FirstOrDefault();


                    Query = "select D.Branch as Branch,L.DocDate as Date,D.Srl as DocNo,D.Narr as Remark,D.ENTEREDBY as ENTEREDBY,D.LASTUPDATEDATE as LASTUPDATEDATE,D.Type as Type,"
                            + " (select DD.Name From DocTypes DD where DD.code = D.Type) as TypeName ,(select M.Name From Master M where M.code = L.Code) as Code,(select M.Name From Master M where M.code = L.Altcode) as Bank,D.Tablekey + D.Branch as TablekeyBranch"
                            + " from DebtorRecVouDetail D left"
                            + " join Ledger L on L.Parentkey + L.Branch = D.Parentkey + D.Branch"
                            + " where D.InvTableKey = '" + SaleBill + "' and D.InvBranch = '" + Model.BillBranch + "' and L.Sno=1 ";
                    ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                    Model.PaymentReceivedDetails = PaymentReceivedDetails(ordersstk);

                    #region Bill Balance Amount 

                    var TotalCharges = Model.LorryReceiptList.Select(x => x.Charges).ToList();
                    var TotalBillAmt = TotalCharges.Sum(x => x.Sum(y => y.Amt));
                    if (Model.PaymentReceivedDetails != null)
                    {
                        var AllPaymentList = Model.PaymentReceivedDetails.Select(x => x.GetAmountDetails).ToList();
                        var TotalReceAmt = AllPaymentList.Sum(x => x.Sum(y => y.Amt));
                        Model.BillBalanceAmt = TotalBillAmt - TotalReceAmt;
                    }
                    else
                    {
                        Model.BillBalanceAmt = TotalBillAmt;
                    }

                    #endregion
                }
                else
                {
                    Status = "Error";
                }

                #region Attachment

                docdetail = ctxTFAT.Attachment.Where(x => x.ParentKey == Model.Parentkey && (x.Type == Model.BillType) && x.Type != "Alert").ToList().Count();
                docdetail1 = ctxTFAT.AlertNoteMaster.Where(x => x.ParentKey == Model.Parentkey && x.Type == Model.BillType).ToList().Count();
                #endregion
            }
            var html = ViewHelper.RenderPartialView(this, "_ReportDetails", Model);
            var jsonResult = Json(new
            {
                Status = Status,
                Html = html,
                AttachC = docdetail,
                AlertC = docdetail1,
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public BillQueryVM BillDetails(List<DataRow> ordersstk, BillQueryVM objitemlist)
        {
            foreach (var item in ordersstk)
            {
                var BillBranch = item["BillBranch"].ToString();
                objitemlist.BillDate = ConvertDDMMYYTOYYMMDD(item["BillDate"].ToString());
                objitemlist.BillENTEREDBY = item["BillENTEREDBY"].ToString();
                objitemlist.BillLASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(item["BillLASTUPDATEDATE"].ToString());
                objitemlist.Customer = item["Customer"].ToString();
                objitemlist.CustomerCode = item["CODE"].ToString();
                objitemlist.BMaster = item["BMaster"].ToString();
                objitemlist.TotAmt = item["TotAmt"].ToString();
                objitemlist.Remark = item["Remark"].ToString();
                //objitemlist.Tablekey = item["Tablekey"].ToString();
                objitemlist.BillBranch = item["BillBranch"].ToString();
                objitemlist.BillBranchN = ctxTFAT.TfatBranch.Where(x=>x.Code==BillBranch).Select(x=>x.Name).FirstOrDefault();
                objitemlist.BillType = item["Type"].ToString();
                objitemlist.BillNo = item["Srl"].ToString();



            }

            return objitemlist;
        }

        public List<BillRelatedLorryReceiptDetailsVM> LorryReceiptDetails(List<DataRow> ordersstk, string ParentKey)
        {
            List<BillRelatedLorryReceiptDetailsVM> objitemlist = new List<BillRelatedLorryReceiptDetailsVM>();
            int i = 1;
            foreach (var item in ordersstk)
            {

                objitemlist.Add(new BillRelatedLorryReceiptDetailsVM()
                {
                    Serial = i,
                    Lrno = item["Lrno"].ToString(),
                    LrDate = ConvertDDMMYYTOYYMMDD(item["LrDate"].ToString()),
                    LrTime = item["LrTime"].ToString().Trim(),
                    Consignor = item["Consignor"].ToString().Trim(),
                    Consignee = item["Consignee"].ToString().Trim(),
                    LRFrom = item["LRFrom"].ToString().Trim(),
                    LRTo = item["LRTo"].ToString(),
                    LRQty = item["LRQty"].ToString(),
                    Unit = item["Unit"].ToString(),
                    ActWt = item["ActWt"].ToString(),
                    ChrgWt = item["ChrgWt"].ToString(),
                    ChrgType = item["ChrgType"].ToString(),
                    LrAmt = Convert.ToDecimal(item["LrAmt"].ToString()),
                    PartyChallan = item["PartyChallan"].ToString(),
                    PartyInvoice = item["PartyInvoice"].ToString(),
                    PONumber = item["PONumber"].ToString(),
                    BENumber = item["BENumber"].ToString(),
                    EawayBill = item["EawayBill"].ToString(),
                    Charges = GetLRCharges(item["Tablekey"].ToString(), ParentKey.ToString())
                });
                i = i + 1;
            }

            return objitemlist;
        }
        public List<LRInvoiceVM> GetLRCharges(string LRNO, string Parentkey)
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
                c.Amt = GetLrWiseChargeValValue(c.tempId, LRNO, Parentkey);
                if (c.Amt > 0)
                {
                    objledgerdetail.Add(c);
                }

            }
            return objledgerdetail;
        }
        public decimal GetLrWiseChargeValValue(int i, string TableKey, string Parentkey)
        {
            string connstring = GetConnectionString();
            decimal abc;

            var loginQuery3 = @"select Val" + i + " from LRBill where  LRRefTablekey = '" + TableKey + "' and Parentkey = '" + Parentkey + "'";
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

        public List<SendBillsubmissinDetailsVM> ExtrabillsubmissinDetails(List<DataRow> ordersstk)
        {
            List<SendBillsubmissinDetailsVM> objitemlist = new List<SendBillsubmissinDetailsVM>();
            foreach (var item in ordersstk)
            {
                var TBranch = item["ToBranch"].ToString();
                var FBranch = item["FromBranch"].ToString();
                var SBranch = item["SBBranch"].ToString();
                objitemlist.Add(new SendBillsubmissinDetailsVM()
                {
                    SBBranch = ctxTFAT.TfatBranch.Where(x => x.Code == SBranch).Select(x => x.Name).FirstOrDefault(),
                    SBType = item["SBType"].ToString(),
                    SBDate = ConvertDDMMYYTOYYMMDD(item["SBDate"].ToString()),
                    SBDocNo = item["SBDocNo"].ToString(),
                    SBRemark = item["SBRemark"].ToString(),
                    SBENTEREDBY = item["SBENTEREDBY"].ToString(),
                    SBLASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(item["SBLASTUPDATEDATE"].ToString()),
                    ToBranch = ctxTFAT.TfatBranch.Where(x=>x.Code==TBranch).Select(x=>x.Name).FirstOrDefault(),
                    FromBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FBranch).Select(x => x.Name).FirstOrDefault(),

                });
            }

            return objitemlist;
        }

        public List<BillsubmissinDetailsVM> billsubmissinDetails(List<DataRow> ordersstk)
        {
            List<BillsubmissinDetailsVM> objitemlist = new List<BillsubmissinDetailsVM>();
            foreach (var item in ordersstk)
            {
                var PartyCode = item["PartyName"].ToString();
                var SBranch = item["SBBranch"].ToString();
                objitemlist.Add(new BillsubmissinDetailsVM()
                {
                    SBBranch = ctxTFAT.TfatBranch.Where(x => x.Code == SBranch).Select(x => x.Name).FirstOrDefault(),
                    SBDate = ConvertDDMMYYTOYYMMDD(item["SBDate"].ToString()),
                    SBSubDt = ConvertDDMMYYTOYYMMDD(item["SBSubDt"].ToString()),
                    SBDocNo = item["SBDocNo"].ToString(),
                    SBThrough = item["SBThrough"].ToString(),
                    SBRemark = item["SBRemark"].ToString(),
                    SBENTEREDBY = item["SBENTEREDBY"].ToString(),
                    SBLASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(item["SBLASTUPDATEDATE"].ToString()),
                    SBType = item["SBType"].ToString(),
                    SBPartyName = ctxTFAT.CustomerMaster.Where(x=>x.Code==PartyCode).Select(x=>x.Name).FirstOrDefault(),
                });
            }

            return objitemlist;
        }


        public List<PaymentReceivedDetailsVM> PaymentReceivedDetails(List<DataRow> ordersstk)
        {
            List<PaymentReceivedDetailsVM> objitemlist = new List<PaymentReceivedDetailsVM>();
            foreach (var item in ordersstk)
            {
                var OBranch = item["Branch"].ToString();
                objitemlist.Add(new PaymentReceivedDetailsVM()
                {
                    Branch = ctxTFAT.TfatBranch.Where(x=>x.Code==OBranch).Select(x=>x.Name).FirstOrDefault(),
                    Date = ConvertDDMMYYTOYYMMDD(item["Date"].ToString()),
                    DocNo = item["DocNo"].ToString(),
                    Remark = item["Remark"].ToString(),
                    ENTEREDBY = item["ENTEREDBY"].ToString(),
                    LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(item["LASTUPDATEDATE"].ToString()),
                    Type = item["Type"].ToString(),
                    TypeName = item["TypeName"].ToString(),
                    Bank = item["Bank"].ToString(),
                    Code = item["Code"].ToString(),
                    TablekeyBranch = item["TablekeyBranch"].ToString(),
                    GetAmountDetails = GetFMWiseCharges(item["TablekeyBranch"].ToString())
                });
            }

            return objitemlist;
        }

        public List<DebtorReceiptVM> GetFMWiseCharges(string tablekeybranch)
        {
            List<DebtorReceiptVM> objledgerdetail = new List<DebtorReceiptVM>();

            DebtorReceiptVM c1 = new DebtorReceiptVM();
            c1.Fld = "Amt";
            c1.Header = "Amt";
            c1.AddLess = "";
            c1.tempid = 1;
            c1.PostCode = "";
            //c.Amt = (mStrArray[a] == null) ? (decimal)0 : Convert.ToDecimal(mStrArray[a]);
            c1.Amt = (decimal)ctxTFAT.DebtorRecVouDetail.Where(x => x.TableKey + x.Branch == tablekeybranch).Select(x => x.Amount).FirstOrDefault();
            if (c1.Amt > 0)
            {
                c1.Amt = Math.Round(c1.Amt, 2);
                objledgerdetail.Add(c1);
            }

            DebtorReceiptVM c2 = new DebtorReceiptVM();
            c2.Fld = "TDS";
            c2.Header = "TDS";
            c2.AddLess = "-";
            c2.PostCode = "";
            c1.tempid = 1;
            //c.Amt = (mStrArray[a] == null) ? (decimal)0 : Convert.ToDecimal(mStrArray[a]);
            c2.Amt = (decimal)ctxTFAT.DebtorRecVouDetail.Where(x => x.TableKey + x.Branch == tablekeybranch).Select(x => x.TdsAmout).FirstOrDefault();
            if (c2.Amt > 0)
            {
                c2.Amt = Math.Round(c2.Amt, 2);
                objledgerdetail.Add(c2);
            }





            var trncharges = ctxTFAT.Charges.Where(x => x.Type == "BRC00" && x.DontUse == false).Select(x => new { x.Fld, x.Head, x.EqAmt, x.Equation, x.Code }).ToList();
            foreach (var i in trncharges)
            {
                DebtorReceiptVM c = new DebtorReceiptVM();
                c.Fld = i.Fld;
                c.Header = i.Head;
                c.AddLess = i.EqAmt;
                c.PostCode = i.Code;
                c.tempid = Convert.ToInt16(i.Fld.Substring(1, 3));
                c.Amt = GetChargeValValue(c.tempid, tablekeybranch);
                if (c.Amt > 0)
                {
                    objledgerdetail.Add(c);
                }

            }
            return objledgerdetail;
        }
        public decimal GetChargeValValue(int i, string tablekeyBranch)
        {
            string connstring = GetConnectionString();
            decimal abc;

            var loginQuery3 = @"select Val" + i + " from DebtorRecVouDetail where tablekey+Branch = '" + tablekeyBranch + "'";
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

        #region Not Required Functions

        public ActionResult GetAllRemarkOfDocument(string BillNo, string BillType)
        {
            Sales sales = ctxTFAT.Sales.Where(x => x.Srl.ToString() == BillNo && x.Type == BillType).FirstOrDefault();

            List<LoadingToDispatchVM> loadingTos = new List<LoadingToDispatchVM>();

            var FMList = (from FreightMemo in ctxTFAT.Sales
                          where FreightMemo.Srl.ToString().Trim() == BillNo.ToString().Trim() && FreightMemo.Type.ToString().Trim() == BillType.ToString().Trim() && String.IsNullOrEmpty(FreightMemo.Narr) == false
                          orderby FreightMemo.Srl
                          select new LoadingToDispatchVM()
                          {
                              FMNO = FreightMemo.Srl.ToString(),
                              AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Branch).Select(x => x.Name).FirstOrDefault(),
                              NarrStr = FreightMemo.Narr,
                              ENTEREDBY = FreightMemo.ENTEREDBY,
                              AUTHIDS = "N",
                              NarrSno = 0,
                              PayLoadL = FreightMemo.Type.Trim() == "SLW00" ? "Sale Bill (Without LR)" : "Credit Sales (Invoice)",
                          }).ToList();
            loadingTos.AddRange(FMList);

            var GetAlertNote = (from FreightMemo in ctxTFAT.AlertNoteMaster
                                where FreightMemo.TypeCode.ToString().Trim() == BillNo.ToString().Trim() && FreightMemo.Type == BillType
                                orderby FreightMemo.DocNo
                                select new LoadingToDispatchVM()
                                {
                                    FMNO = FreightMemo.TypeCode.ToString(),
                                    AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Branch).Select(x => x.Name).FirstOrDefault(),
                                    NarrStr = FreightMemo.Note,
                                    ENTEREDBY = FreightMemo.CreateBy,
                                    AUTHIDS = "N",
                                    NarrSno = 0,
                                    PayLoadL = "Alert Note",
                                }).ToList();
            loadingTos.AddRange(GetAlertNote);

            var GetRemarkDocList = ctxTFAT.Narration.Where(x => x.Srl == BillNo.ToString().Trim() && x.Type == BillType.ToString().Trim()).ToList();
            foreach (var item in GetRemarkDocList)
            {
                loadingTos.Add(new LoadingToDispatchVM
                {
                    FMNO = item.Srl,
                    AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault(),
                    NarrStr = item.Narr,
                    ENTEREDBY = item.ENTEREDBY,
                    AUTHIDS = item.ENTEREDBY.Trim().ToLower() == muserid.Trim().ToLower() ? "Y" : "N",
                    NarrSno = item.Sno,
                    PayLoadL = item.NarrRich
                });
            }
            var html = ViewHelper.RenderPartialView(this, "_DocumentNarrList", loadingTos);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveNarration(LoadingToDispatchVM Model)
        {
            string html = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    if (Model.NarrStr != null)
                    {
                        Sales sales = ctxTFAT.Sales.Where(x => x.Srl.ToString() == Model.FMNO && x.Type == Model.MainType).FirstOrDefault();

                        var LastSno = ctxTFAT.Narration.Where(x => x.ParentKey == sales.TableKey).ToList().Count();
                        ++LastSno;
                        Narration narr = new Narration();
                        narr.Branch = mbranchcode;
                        narr.Narr = Model.NarrStr;
                        narr.NarrRich = Model.Header;
                        narr.Prefix = mperiod;
                        narr.Sno = LastSno;
                        narr.Srl = sales.Srl.ToString();
                        narr.Type = Model.MainType;
                        narr.ENTEREDBY = muserid;
                        narr.LASTUPDATEDATE = DateTime.Now;
                        narr.AUTHORISE = mauthorise;
                        narr.AUTHIDS = muserid;
                        narr.LocationCode = 0;
                        narr.TableKey = Model.MainType + mperiod.Substring(0, 2) + LastSno.ToString("D3") + sales.Srl.ToString();
                        narr.CompCode = mcompcode;
                        narr.ParentKey = sales.TableKey;
                        ctxTFAT.Narration.Add(narr);
                        ctxTFAT.SaveChanges();
                        transaction.Commit();
                        transaction.Dispose();

                        List<LoadingToDispatchVM> loadingTos = new List<LoadingToDispatchVM>();


                        var FMList = (from FreightMemo in ctxTFAT.Sales
                                      where FreightMemo.Srl.ToString().Trim() == sales.Srl.ToString().Trim() && FreightMemo.Type.ToString().Trim() == Model.MainType.ToString().Trim() && String.IsNullOrEmpty(FreightMemo.Narr) == false
                                      orderby FreightMemo.Srl
                                      select new LoadingToDispatchVM()
                                      {
                                          FMNO = FreightMemo.Srl.ToString(),
                                          AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Branch).Select(x => x.Name).FirstOrDefault(),
                                          NarrStr = FreightMemo.Narr,
                                          ENTEREDBY = FreightMemo.ENTEREDBY,
                                          AUTHIDS = "N",
                                          NarrSno = 0,
                                          PayLoadL = FreightMemo.Type.Trim() == "SLW00" ? "Sale Bill (Without LR)" : "Credit Sales (Invoice)",
                                      }).ToList();
                        loadingTos.AddRange(FMList);
                        var GetAlertNote = (from FreightMemo in ctxTFAT.AlertNoteMaster
                                            where FreightMemo.TypeCode.ToString().Trim() == sales.Srl.ToString().Trim() && FreightMemo.Type == sales.Type
                                            orderby FreightMemo.DocNo
                                            select new LoadingToDispatchVM()
                                            {
                                                FMNO = FreightMemo.TypeCode.ToString(),
                                                AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Branch).Select(x => x.Name).FirstOrDefault(),
                                                NarrStr = FreightMemo.Note,
                                                ENTEREDBY = FreightMemo.CreateBy,
                                                AUTHIDS = "N",
                                                NarrSno = 0,
                                                PayLoadL = "Alert Note",
                                            }).ToList();
                        loadingTos.AddRange(GetAlertNote);
                        var GetRemarkDocList = ctxTFAT.Narration.Where(x => x.Srl == sales.Srl.ToString().Trim() && x.Type == Model.MainType.ToString().Trim()).ToList();
                        foreach (var item in GetRemarkDocList)
                        {
                            loadingTos.Add(new LoadingToDispatchVM
                            {
                                FMNO = item.Srl,
                                AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault(),
                                NarrStr = item.Narr,
                                ENTEREDBY = item.ENTEREDBY,
                                NarrSno = item.Sno,
                                PayLoadL = item.NarrRich,
                                AUTHIDS = item.ENTEREDBY.Trim().ToLower() == muserid.Trim().ToLower() ? "Y" : "N",
                            });
                        }
                        html = ViewHelper.RenderPartialView(this, "_DocumentNarrList", loadingTos);
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
                    Narration narration = ctxTFAT.Narration.Where(x => x.Sno.ToString() == mModel.NarrSno.ToString() && x.Type == mModel.MainType).FirstOrDefault();
                    if (narration != null)
                    {
                        ctxTFAT.Narration.Remove(narration);
                        ctxTFAT.SaveChanges();
                        transaction.Commit();
                        transaction.Dispose();
                    }
                    List<LoadingToDispatchVM> loadingTos = new List<LoadingToDispatchVM>();
                    Sales sales = ctxTFAT.Sales.Where(x => x.Srl.ToString() == mModel.FMNO && x.Type == mModel.MainType).FirstOrDefault();
                    if (sales != null)
                    {

                        var FMList = (from FreightMemo in ctxTFAT.Sales
                                      where FreightMemo.Srl.ToString().Trim() == sales.Srl.ToString().Trim() && FreightMemo.Type.ToString().Trim() == mModel.MainType.ToString().Trim() && String.IsNullOrEmpty(FreightMemo.Narr) == false
                                      orderby FreightMemo.Srl
                                      select new LoadingToDispatchVM()
                                      {
                                          FMNO = FreightMemo.Srl.ToString(),
                                          AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Branch).Select(x => x.Name).FirstOrDefault(),
                                          NarrStr = FreightMemo.Narr,
                                          ENTEREDBY = FreightMemo.ENTEREDBY,
                                          AUTHIDS = "N",
                                          NarrSno = 0,
                                          PayLoadL = FreightMemo.Type.Trim() == "SLW00" ? "Sale Bill (Without LR)" : "Credit Sales (Invoice)",
                                      }).ToList();
                        loadingTos.AddRange(FMList);
                        var GetAlertNote = (from FreightMemo in ctxTFAT.AlertNoteMaster
                                            where FreightMemo.TypeCode.ToString().Trim() == sales.Srl.ToString().Trim() && FreightMemo.Type == sales.Type
                                            orderby FreightMemo.DocNo
                                            select new LoadingToDispatchVM()
                                            {
                                                FMNO = FreightMemo.TypeCode.ToString(),
                                                AppBranch = "",
                                                NarrStr = FreightMemo.Note,
                                                ENTEREDBY = FreightMemo.CreateBy,
                                                AUTHIDS = "N",
                                                NarrSno = 0,
                                                PayLoadL = "Alert Note",
                                            }).ToList();
                        loadingTos.AddRange(GetAlertNote);
                        var GetRemarkDocList = ctxTFAT.Narration.Where(x => x.Srl == sales.Srl.ToString().Trim() && x.Type == mModel.MainType.ToString().Trim()).ToList();
                        foreach (var item in GetRemarkDocList)
                        {
                            loadingTos.Add(new LoadingToDispatchVM
                            {
                                FMNO = item.Srl,
                                AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault(),
                                NarrStr = item.Narr,
                                ENTEREDBY = item.ENTEREDBY,
                                NarrSno = item.Sno,
                                PayLoadL = item.NarrRich,
                                AUTHIDS = item.ENTEREDBY.Trim().ToLower() == muserid.Trim().ToLower() ? "Y" : "N",
                            });
                        }
                    }

                    html = ViewHelper.RenderPartialView(this, "_DocumentNarrList", loadingTos);
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