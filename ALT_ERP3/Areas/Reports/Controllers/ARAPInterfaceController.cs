using ALT_ERP3.Controllers;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Reports.Controllers
{
    public class ARAPInterfaceController : BaseController
    {
        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "A");
            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            //mbasegr = Model.MainType;
            ViewBag.Header = Model.Header;
            ViewBag.Controller = Model.Controller;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;
            Model.Branch = mbranchcode;
            Model.BranchName = NameofAccount(mbranchcode, "B");
            //ctxooroo.OorooBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault();
            Model.AccountName = NameofAccount(mbranchcode, "B");
            //Model.BranchName = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault();
            //Model.AccountName = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault();
            Model.Opening = 0;
            Model.Closing = 0;
            Model.ViewDataId = "ArApInvGrid";
            ViewBag.ViewDataId = Model.ViewDataId;
            //Model.TableName = "ledger";
            //Model.MainType = "SL";
            return View(Model);
        }

        [HttpGet]
        public ActionResult GetLocations()
        {
            return Json(GetDataTableList("Select Code, Name from Warehouse Where Branch = '" + mbranchcode + "' and Charindex('" + muserid + "',Users)<>0"), JsonRequestBehavior.AllowGet);
            //var result = ctxTFAT.Warehouse.Where(z => z.Branch == mbranchcode && (z.Users + ",").Contains(muserid + ",")).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).ToList();
            //return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBranches()
        {
            return Json(GetDataTableList("Select Code, Name = '[' + Code + '] ' + Name from TfatBranch Where Code = '" + mbranchcode + "'"), JsonRequestBehavior.AllowGet);
            //var result = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(m => new { m.Code, Name = "[" + m.Code + "] " + m.Name }).OrderBy(n => n.Name).ToList();
            //return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetParties(string term)
        {
            var mPara = term.Split('^');
            var mP1 = mPara[0];
            var mP2 = mPara[1];
            if (mP1 == "")
            {
                //var result = ctxTFAT.Master.Where(x => (x.BaseGr == mP2 || x.BaseGr=="U") && x.Hide == false && x.Name.Contains(term)).Select(m => new
                //{
                //    Code = m.Code,
                //    Name = m.Name
                //}).OrderBy(n => n.Name).ToList();
                //return Json(result, JsonRequestBehavior.AllowGet);
                return Json(GetDataTableList("Select Code, Name from Master Where (BaseGr = '" + mP2 + "' or BaseGr = 'U') and Hide=0 and Charindex('" + term + "',Name)<>0 Order by Name"), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(GetDataTableList("Select Code, Name from Master Where (BaseGr = '" + mP2 + "' or BaseGr='U') and Hide=0 and Charindex('" + mP1 + "',Name)<>0 Order by Name"), JsonRequestBehavior.AllowGet);
                //var result = ctxTFAT.Master.Where(x => (x.BaseGr == mP2 || x.BaseGr == "U") && x.Hide == false && x.Name.Contains(mP1)).Select(m => new
                //{
                //    Code = m.Code,
                //    Name = m.Name
                //}).OrderBy(n => n.Name).ToList();
                //return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Address(string PartyCode)
        {
            //var result = ctxTFAT.Address.Where(x => x.Code == PartyCode).Select(m => new
            //{
            //    Mobile = m.Mobile,
            //    Tel1 = m.Tel1,
            //    Tel2 = m.Tel2,
            //    Adrl1 = m.Adrl1,
            //}).ToList();
            //return Json(result, JsonRequestBehavior.AllowGet);
            return Json(GetDataTableList("Select Mobile, Tel1, Tel2, Adrl1 from Address Where Code = '" + PartyCode + "'"), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAccountOpenClose(GridOption Model)
        {
            decimal OpeningBal, ClosingBal;
            DateTime StartDate = ConvertDDMMYYTOYYMMDD(Model.FromDate);
            DateTime EndDate = ConvertDDMMYYTOYYMMDD(Model.ToDate);
            OpeningBal = FieldoftableNumber("Ledger", "Sum(Debit-Credit)", "MainType<>'MV' and MainType<>'PV' and Code='" + Model.Code + "' and DocDate<'" + MMDDYY(StartDate) + "' and Branch='" + mbranchcode + "' and left(AUTHORISE,1)='A'" + (string.IsNullOrEmpty(mDocString) ? "" : " and Charindex(Type,'" + mDocString + "')=0") + (Model.LocationCode != 0 ? " and LocationCode=" + Model.LocationCode : ""));
            //if (Model.LocationCode != 0)
            //{
            //    OpeningBal = (from L in ctxTFAT.Ledger
            //                  where L.MainType != "MV" && L.MainType != "PV" && L.Code == Model.Code && L.DocDate < StartDate && L.Branch == mbranchcode && L.LocationCode == Model.LocationCode && L.AUTHORISE.StartsWith("A") && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
            //                  select (L.Debit - L.Credit)).DefaultIfEmpty().Sum() ?? 0;
            //}
            //else
            //{
            //    OpeningBal = (from L in ctxTFAT.Ledger
            //                  where L.MainType != "MV" && L.MainType != "PV" && L.Code == Model.Code && L.DocDate < StartDate && L.Branch == mbranchcode && L.AUTHORISE.StartsWith("A") && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
            //                  select (L.Debit - L.Credit)).DefaultIfEmpty().Sum() ?? 0;
            //}

            string mOpening = string.Format("{0:0.00}", Math.Abs(OpeningBal)) + (OpeningBal > 0 ? " Dr" : " Cr");
            //if (Model.LocationCode != 0)
            //{
            //    ClosingBal = (from L in ctxTFAT.Ledger
            //                  where L.MainType != "MV" && L.MainType != "PV" && L.Code == Model.Code && L.DocDate <= EndDate && L.Branch == mbranchcode && L.LocationCode == Model.LocationCode && L.AUTHORISE.StartsWith("A") && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
            //                  select L.Debit - L.Credit).DefaultIfEmpty().Sum() ?? 0;
            //}
            //else
            //{
            //    ClosingBal = (from L in ctxTFAT.Ledger
            //                  where L.MainType != "MV" && L.MainType != "PV" && L.Code == Model.Code && L.DocDate <= EndDate && L.Branch == mbranchcode && L.AUTHORISE.StartsWith("A") && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
            //                  select L.Debit - L.Credit).DefaultIfEmpty().Sum() ?? 0;
            //}
            ClosingBal = FieldoftableNumber("Ledger", "Sum(Debit-Credit)", "MainType<>'MV' and MainType<>'PV' and Code='" + Model.Code + "' and DocDate<='" + MMDDYY(EndDate) + "' and Branch='" + mbranchcode + "' and left(AUTHORISE,1)='A'" + (string.IsNullOrEmpty(mDocString) ? "" : " and Charindex(Type,'" + mDocString + "')=0") + (Model.LocationCode != 0 ? " and LocationCode=" + Model.LocationCode : ""));
            string mClosing = string.Format("{0:0.00}", Math.Abs((decimal)ClosingBal)) + (ClosingBal > 0 ? " Dr" : " Cr");
            return Json(new { mOpening = mOpening, mClosing = mClosing }, JsonRequestBehavior.AllowGet);
        }

        #region DebitGrid
        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
            //////ITransactionGridOperation mIlst = new TransactionGridOperation();
            return GetGridDataColumns(id, "X", "");
        }

        [HttpPost]
        public ActionResult GetGridDataDebit(GridOption Model)
        {
            return GetGridReport(Model, "R", "Code^" + Model.Code + "~Warehouse^" + Model.LocationCode + "~DrCrFlag^D~IncludeZero^" + Model.IsFormatSelected);
            //return Content(JQGridHelper.JsonForJqgrid(GetDebitLedgerReport(Model), Model.rows, GetTotalCount(Model), Model.page), "application/json");
        }

        public int GetTotalCount(GridOption Model)
        {
            DataTable dt = TempData.Peek("DebitDt") as DataTable;
            return dt.Rows.Count;
        }
        #endregion

        #region CreditGrid
        [HttpPost]
        public ActionResult GetGridDataCredit(GridOption Model)
        {
            return GetGridReport(Model, "R", "Code^" + Model.Code + "~Warehouse^" + Model.LocationCode + "~DrCrFlag^C~IncludeZero^" + Model.IsFormatSelected);
        }

        public int CreditGetTotalCount(GridOption Model)
        {
            DataTable dt = TempData.Peek("CreditDt") as DataTable;
            return dt.Rows.Count;
        }
        #endregion

        #region SubGrid
        List<string> ColumnList = new List<string>();
        public ActionResult SubGrid(GridOption Model)
        {
            ColumnList.Add("type");
            ColumnList.Add("prefix");
            ColumnList.Add("serial");
            ColumnList.Add("amount");
            ColumnList.Add("doc.date");
            TempData["ColumnLIST"] = ColumnList;
            var mTfatSearch = (from TS in ctxTFAT.TfatSearch
                               where TS.Code == "ArApInvGrid" && TS.CalculatedCol == false && TS.ColHead != ""
                               orderby TS.Sno
                               select new { TS.ColHead, TS.ColWidth, TS.ColField, TS.ColType, TS.YesTotal, TS.AllowEdit, TS.Decs, TS.IsHidden }).ToList();

            List<string> colname = new List<string>();
            List<GridColumn> colModal = new List<GridColumn>();
            List<object> result = new List<object>();

            foreach (var Fld in mTfatSearch)
            {
                if (ColumnList.Contains(Fld.ColHead.Trim().ToLower()))
                {
                    colname.Add(Fld.ColHead.Trim());
                    GridColumn gc = new GridColumn();
                    gc.name = Fld.ColHead.Trim();
                    gc.index = Fld.ColField;
                    gc.editable = Fld.AllowEdit;
                    if (Fld.ColType == "Num" || Fld.ColType == "Qty" || Fld.ColType == "Rte")
                    {
                        gc.align = "right";
                        gc.formatter = "number : { decimalSeparator: \".\", thousandsSeparator: \"\", decimalPlaces: " + Fld.Decs + "}";
                        if (Fld.YesTotal == true)
                        {
                            gc.summaryTpl = "<b>{0}</b>";
                            gc.summaryType = "sum";
                        }
                    }
                    if (Fld.ColType == "Dte" || Fld.ColType == "Dtm")
                    {
                        gc.align = "center";
                        if (gc.editable == true)
                            gc.editoptions = "dataInit:function(el){$(el).datepicker({ dateFormat: 'yy-mm-dd'});}";
                    }
                    gc.sortable = true;
                    gc.search = true;
                    gc.width = Fld.IsHidden == true ? "0" : (Fld.ColWidth).ToString();   // removed /15
                    if (Fld.ColHead == "Prefix")
                    {
                        gc.width = "50";
                    }
                    colModal.Add(gc);
                }
            }
            result.Add(GetString(colname.ToArray()));
            result.Add(colModal);
            JsonResult JR = new JsonResult();
            JR.Data = Newtonsoft.Json.JsonConvert.SerializeObject(result);
            return JR;
        }

        [HttpPost]
        public ActionResult GetTransactionGridData1(GridOption Model)
        {
            return Content(JQGridHelper.JsonForJqgrid(GetSubDebitLedgerReport(Model), Model.rows, GetTotalCount1(Model), Model.page), "application/json");
        }

        public DataTable GetSubDebitLedgerReport(GridOption Model)
        {
            var query = "Select Type,Prefix,Srl as Serial,Amount,DocDate as [Doc.Date] from Outstanding where TableRefKey='" + Model.Document + "'" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(Type,'" + mDocString + "')=0");
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            SqlConnection conn = new SqlConnection(GetConnectionString());
            SqlDataAdapter adap = new SqlDataAdapter(query, conn);
            adap.Fill(ds);
            dt = ds.Tables[0];
            TempData["SubGridDT"] = dt;
            return dt;
        }

        public int GetTotalCount1(GridOption Model)
        {
            DataTable dt = TempData.Peek("SubGridDT") as DataTable;
            return dt.Rows.Count;
        }
        #endregion

        #region Adjustment
        public ActionResult FIFOAdjust(GridOption Model)
        {
            var result = "";
            DataTable griddebitDt = new DataTable();
            DataTable gridcreditDt = new DataTable();
            string mStr = "Select MainType,SubType,Code,TableKey,Sno,Type,Prefix,Srl,(Outstanding.Debit + Outstanding.Credit) - isnull((Select Sum(o.Amount) from Outstanding o Where o.Code=Outstanding.Code And o.TableRefKey = Outstanding.TableKey And o.Branch = Outstanding.Branch),0) as Pending " +
                            "from Ledger Outstanding Where Outstanding.Code='" + Model.Code + "' And " +
                            "Outstanding.Debit <> 0 And Outstanding.MainType<>'MV' And Outstanding.MainType <> 'PV' And " +
                            "(Outstanding.Debit + Outstanding.Credit) - isnull((Select Sum(o.Amount) from Outstanding o " +
                            "where o.Code = Outstanding.Code And o.TableRefKey = Outstanding.TableKey And " +
                            " o.Branch = Outstanding.Branch),0)<> 0 " + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(Outstanding.Type,'" + mDocString + "')=0") +
                            "And OutStanding.DocDate <= '" + MMDDYY(Convert.ToDateTime(Model.ToDate)) + "' And Outstanding.Branch = '" + mbranchcode + "' And Left(Authorise,1) = 'A' And " +
                            "OutStanding.Debit<>0 And OutStanding.Locationcode = '" + Model.LocationCode + "' Order by Outstanding.DocDate";
            SqlConnection conn = new SqlConnection(GetConnectionString());
            SqlDataAdapter dadr = new SqlDataAdapter(mStr, conn);
            dadr.Fill(griddebitDt);

            mStr = "Select MainType,SubType,Code,TableKey,Sno,Type,Prefix,Srl,(Outstanding.Debit + Outstanding.Credit) - isnull((Select Sum(o.Amount) from Outstanding o Where o.Code=Outstanding.Code And o.TableRefKey = Outstanding.TableKey And o.Branch = Outstanding.Branch),0) as Pending " +
                "from Ledger Outstanding Where Outstanding.Code = '" + Model.Code + "' And Outstanding.Credit <> 0 And Outstanding.MainType <> 'MV' " +
                "And Outstanding.MainType <> 'PV'  And(Outstanding.Debit + Outstanding.Credit) - isnull((Select Sum(o.Amount) from Outstanding o " +
                "where o.Code = Outstanding.Code And o.TableRefKey = Outstanding.TableKey " + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(Type,'" + mDocString + "')=0 ") +
                "And o.Branch = Outstanding.Branch),0)<> 0 And OutStanding.DocDate <= '" + MMDDYY(Convert.ToDateTime(Model.ToDate)) + "' And " +
                "OutStanding.Credit<>0 And Outstanding.Branch = '" + mbranchcode + "' And Left(Authorise,1) = 'A' And Locationcode = '" + Model.LocationCode + "' " +
                "Order by Outstanding.DocDate";
            //SqlConnection conn1 = new SqlConnection(GetConnectionString());
            SqlDataAdapter dacr = new SqlDataAdapter(mStr, conn);
            dacr.Fill(gridcreditDt);
            using (var trxfifo = ctxTFAT.Database.BeginTransaction())
                try
                {
                    foreach (DataRow mdrow in griddebitDt.Rows)
                    {
                        decimal mdamt = Convert.ToDecimal(mdrow["Pending"].ToString());
                        foreach (DataRow mcrow in gridcreditDt.Rows)
                        {
                            if (mdamt <= 0) break;
                            decimal mcamt = Convert.ToDecimal(mcrow["Pending"].ToString());
                            if (mcamt != 0 && mcamt > 0)
                            {
                                decimal mamt = 0;
                                if (mdamt >= mcamt) { mamt = mcamt; mdamt -= mcamt; mcamt = 0; } else { mamt = mdamt; mcamt = mcamt - mdamt; mdamt = 0; }
                                mcrow["Pending"] = mcamt;
                                var partycode = mdrow["Code"].ToString();
                                var tablekey = mdrow["TableKey"].ToString();
                                Ledger ledger = ctxTFAT.Ledger.Where(x => x.Code == partycode && x.TableKey == tablekey).FirstOrDefault();
                                Outstanding os = new Outstanding();
                                os.Branch = ledger.Branch;
                                os.DocBranch = ledger.Branch;
                                os.Code = partycode;
                                os.ParentKey = ledger.ParentKey;
                                //mcrow["ParentKey"].ToString();
                                os.TableKey = tablekey; //ledger.Type + ledger.Prefix.Substring(0, 2) + ledger.Sno.ToString("D3") + ledger.Srl;
                                os.TableRefKey = mcrow["tablekey"].ToString();
                                os.MainType = ledger.MainType;
                                os.SubType = ledger.SubType;
                                os.Type = ledger.Type;
                                os.Prefix = ledger.Prefix;
                                os.Srl = ledger.Srl;
                                os.Sno = ledger.Sno;
                                os.aMaintype = mcrow["maintype"].ToString();
                                os.aSubType = mcrow["subtype"].ToString();
                                os.aType = mcrow["Type"].ToString();
                                os.aPrefix = mcrow["Prefix"].ToString();
                                os.aSrl = mcrow["Srl"].ToString();
                                os.aSno = Convert.ToInt32(mcrow["Sno"]);
                                os.Amount = mamt;
                                os.AUTHIDS = muserid;
                                os.AUTHORISE = ledger.AUTHORISE;
                                os.BillDate = ledger.BillDate == null ? Convert.ToDateTime("1900-01-01") : ledger.BillDate;
                                os.BillNumber = ledger.BillNumber == null ? "" : ledger.BillNumber;
                                os.CompCode = mcompcode;
                                os.Broker = 100001;
                                os.Brokerage = 0;// Convert.ToDecimal(0.00);
                                os.BrokerAmt = 0;// Convert.ToDecimal(0.00);
                                os.BrokerOn = 0;// Convert.ToDecimal(0.00);
                                os.ChlnDate = DateTime.Now;
                                os.ChlnNumber = "";
                                os.CrPeriod = 0;
                                os.CurrName = (ledger.CurrName.ToString() == "") ? 0 : Convert.ToInt32(ledger.CurrName);
                                os.CurrRate = 1;
                                os.DocDate = ledger.DocDate;
                                os.Narr = "";
                                os.OrdDate = DateTime.Now;
                                os.OrdNumber = "";
                                //os.ProjCode = ledger.ProjCode;
                                //os.ProjectStage = ledger.ProjectStage;
                                //os.ProjectUnit = ledger.ProjectUnit;
                                os.RefParty = "";
                                os.SalemanAmt = 0;
                                os.SalemanOn = 0;
                                os.SalemanPer = 0;
                                os.Salesman = 100001;
                                os.TDSAmt = 0;
                                os.ENTEREDBY = ledger.ENTEREDBY;
                                os.LASTUPDATEDATE = DateTime.Now;
                                os.CurrAmount = ledger.CurrAmount;
                                os.ValueDate = DateTime.Now;
                                os.LocationCode = ledger.LocationCode;
                                ctxTFAT.Outstanding.Add(os);

                                Outstanding os2 = new Outstanding();
                                partycode = mcrow["Code"].ToString();
                                tablekey = mcrow["TableKey"].ToString();
                                ledger = ctxTFAT.Ledger.Where(x => x.Code == partycode && x.TableKey == tablekey).FirstOrDefault();
                                os2.Branch = ledger.Branch;
                                os2.DocBranch = ledger.Branch;
                                os2.Code = partycode;
                                os2.ParentKey = ledger.ParentKey;
                                os2.TableKey = tablekey;// ledger.Type + ledger.Prefix.Substring(0, 2) + ledger.Sno.ToString("D3") + ledger.Srl;
                                os2.TableRefKey = mdrow["tablekey"].ToString();
                                os2.MainType = ledger.MainType;
                                os2.SubType = ledger.SubType;
                                os2.Type = ledger.Type;
                                os2.Prefix = ledger.Prefix;
                                os2.Srl = ledger.Srl;
                                os2.Sno = ledger.Sno;
                                os2.aMaintype = mdrow["maintype"].ToString();
                                os2.aSubType = mdrow["subtype"].ToString();
                                os2.aType = mdrow["Type"].ToString();
                                os2.aPrefix = mdrow["Prefix"].ToString();
                                os2.aSrl = mdrow["Srl"].ToString();
                                os2.aSno = Convert.ToInt32(mdrow["Sno"]);
                                os2.Amount = mamt;
                                os2.AUTHIDS = muserid;
                                os2.AUTHORISE = ledger.AUTHORISE;
                                os2.BillDate = ledger.BillDate == null ? Convert.ToDateTime("1900-01-01") : ledger.BillDate;
                                os2.BillNumber = ledger.BillNumber == null ? "" : ledger.BillNumber;
                                os2.CompCode = mcompcode;
                                os2.Broker = 100001;
                                os2.Brokerage = 0;
                                os2.BrokerAmt = 0;
                                os2.BrokerOn = 0;
                                os2.ChlnDate = DateTime.Now;
                                os2.ChlnNumber = "";
                                os2.CrPeriod = 0;
                                os2.CurrName = (ledger.CurrName.ToString() == "") ? 0 : Convert.ToInt32(ledger.CurrName);
                                os2.CurrRate = 1;
                                os2.DocDate = ConvertDDMMYYTOYYMMDD(ledger.DocDate.ToString());
                                os2.Narr = "";
                                os2.OrdDate = DateTime.Now;
                                os2.OrdNumber = "";
                                //os2.ProjCode = ledger.ProjCode;
                                //os2.ProjectStage = ledger.ProjectStage;
                                //os2.ProjectUnit = ledger.ProjectUnit;
                                os2.RefParty = "";
                                os2.SalemanAmt = 0;
                                os2.SalemanOn = 0;
                                os2.SalemanPer = 0;
                                os2.Salesman = 100001;
                                os2.TDSAmt = 0;
                                os2.ENTEREDBY = ledger.ENTEREDBY;
                                os2.LASTUPDATEDATE = DateTime.Now;
                                os2.CurrAmount = ledger.CurrAmount;
                                os2.ValueDate = DateTime.Now;
                                os2.LocationCode = ledger.LocationCode;
                                ctxTFAT.Outstanding.Add(os2);
                            }
                        }
                    }
                    ctxTFAT.SaveChanges();
                    result = "Success";
                    trxfifo.Commit();
                }
                catch (Exception ex)
                {
                    trxfifo.Rollback();
                    result = ex.Message.ToString();
                    return Json(result, JsonRequestBehavior.AllowGet);
                    throw;
                }
                finally
                {
                    dacr.Dispose();
                    dadr.Dispose();
                    conn.Dispose();
                }
            //return null;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReleaseAdjust(List<string> CTablekey, List<string> DTablekey, string Code, string LocationCode)
        {
            if (DTablekey != null)
            {
                if (DTablekey.Count > 0)
                {
                    foreach (var item in DTablekey)
                    {
                        ExecuteStoredProc("Delete from Outstanding Where TableKey='" + item + "' or TableRefKey='" + item + "'");
                        //var mosdel = ctxTFAT.Outstanding.Where(z => z.TableKey == item || z.TableRefKey == item).ToList();
                        //ctxTFAT.Outstanding.RemoveRange(mosdel);
                        //ctxTFAT.SaveChanges();
                    }
                }
            }

            if (CTablekey != null)
            {
                if (CTablekey.Count > 0)
                {
                    foreach (var item in CTablekey)
                    {
                        ExecuteStoredProc("Delete from Outstanding Where TableKey='" + item + "' or TableRefKey='" + item + "'");
                        //var mosdel = ctxTFAT.Outstanding.Where(z => z.TableKey == item || z.TableRefKey == item).ToList();
                        //if (mosdel != null)
                        //{
                        //    ctxTFAT.Outstanding.RemoveRange(mosdel);
                        //    ctxTFAT.SaveChanges();
                        //}
                    }
                }
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReleaseAll(string Code, int LocationCode)
        {
            ExecuteStoredProc("Delete from Outstanding Where Code='" + Code + "' and LocationCode=" + LocationCode);
            //var mosdel = ctxTFAT.Outstanding.Where(z => z.Code == Code && z.LocationCode == LocationCode).ToList();
            //if (mosdel != null)
            //{
            //    ctxTFAT.Outstanding.RemoveRange(mosdel);
            //    ctxTFAT.SaveChanges();
            //}
            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult Adjust(List<string> CTablekey, List<string> DTablekey, string PartyCode, string EndDate, string LocationCode)
        {
            string TabKey = ""; var result = "";
            DataTable griddebitDt = new DataTable();
            DataTable gridcreditDt = new DataTable();
            EndDate = ConvertDDMMYYTOYYMMDDinstr(EndDate);
            if (DTablekey.Count > 0)
            {
                foreach (var item in DTablekey)
                {
                    TabKey += "'" + item + "'" + ",";
                }
                TabKey = CutRightString(TabKey, 1, ",");
                //TabKey.Substring(0, TabKey.Length - 1);
                var query = "Select Code,TableKey,MainType,SubType,Sno,Type,Prefix,Srl,BillNumber,Convert(char(10),DocDate, 103) as [Doc.Date],Cast(Debit as Decimal(14,2)) as Amount, " +
               "Outstanding.Debit + Outstanding.Credit - isnull((Select Sum(o.Amount) from Outstanding o where " +
               "o.Code = Outstanding.Code And o.TableRefKey = Outstanding.TableKey),0) as Pending," +
               " Convert(char(10), CrPeriod + BillDate,103) as [Due Date], isnull((Select Sum(o.Amount) from Outstanding o " +
               "where o.Code = Outstanding.Code And o.TableRefKey=Outstanding.TableKey),0) as [Adj.Amt],'' as [Select],TableKey," +
               "Abs(CurrAmount) as [Curr.Amt],CurrName as Curr,Narr as Narration from Ledger Outstanding Where Outstanding.Code = '" + PartyCode + "' and " +
               "Outstanding.Debit <> 0 And Outstanding.MainType <> 'MV' And Outstanding.MainType <> 'PV' And " +
               "(Outstanding.Debit + Outstanding.Credit) - isnull((Select Sum(o.Amount) from Outstanding o " +
               "where o.Code = Outstanding.Code And o.TableRefKey = Outstanding.TableKey),0)<> 0 And " +
               "OutStanding.DocDate <= '" + EndDate + "' And Outstanding.Branch = '" + mbranchcode + "' And Left(Authorise,1) = 'A' And " +
               "Locationcode = '" + LocationCode + "' and TableKey in (" + TabKey + ") Order by Outstanding.DocDate";
                SqlConnection conn = new SqlConnection(GetConnectionString());
                SqlDataAdapter adap = new SqlDataAdapter(query.ToString(), conn);
                adap.Fill(griddebitDt);
            }
            else
            {
                result = "Data Not Found";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            if (CTablekey.Count > 0)
            {
                TabKey = "";
                foreach (var item in CTablekey)
                {
                    TabKey += "'" + item + "'" + ",";
                }
                TabKey = CutRightString(TabKey, 1, ",");
                var CreditQuery = "Select ParentKey,Code,TableKey,MainType,SubType,Sno , Type, Prefix, Srl, BillNumber, Convert(char(10), DocDate, 103) as [Doc.Date],Cast(Credit as Decimal(14, 2)) as Amount," +
               " (Outstanding.Debit + Outstanding.Credit - isnull((Select Sum(o.Amount) from Outstanding o " +
               "where o.Code = Outstanding.Code And o.TableRefKey = Outstanding.TableKey),0)) as Pending, " +
               "Convert(char(10), CrPeriod + BillDate, 103) as [Due Date], isnull((Select Sum(o.Amount) from Outstanding o " +
               "where o.Code = Outstanding.Code And o.TableRefKey = Outstanding.TableKey),0) as Bal,'' as [Select],Abs(CurrAmount) as [Curr.Amt]," +
               "CurrName,Narr from Ledger Outstanding Where Outstanding.Code = '" + PartyCode + "' and Outstanding.Credit <> 0 And Outstanding.MainType <> 'MV' " +
               "And Outstanding.MainType <> 'PV'  And(Outstanding.Debit + Outstanding.Credit) - isnull((Select Sum(o.Amount) from Outstanding o " +
               "where o.Code = Outstanding.Code And o.TableRefKey = Outstanding.TableKey),0)<> 0 And OutStanding.DocDate <= '" + EndDate + "'  And " +
               "Outstanding.Branch = '" + mbranchcode + "' And Left(Authorise,1) = 'A' And DocDate<= '" + EndDate + "' And Locationcode = '" + LocationCode + "' " +
               "and TableKey in (" + TabKey + ") Order by Outstanding.DocDate";

                SqlConnection conn1 = new SqlConnection(GetConnectionString());
                SqlDataAdapter adap1 = new SqlDataAdapter(CreditQuery.ToString(), conn1);
                adap1.Fill(gridcreditDt);
            }
            else
            {
                result = "Data Not Found";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            if (griddebitDt.Rows.Count > 0 && gridcreditDt.Rows.Count > 0)
            {
                using (var trxfifo = ctxTFAT.Database.BeginTransaction())
                    try
                    {
                        foreach (DataRow mdrow in griddebitDt.Rows)
                        {
                            decimal mdamt = Convert.ToDecimal(mdrow["Pending"].ToString());
                            foreach (DataRow mcrow in gridcreditDt.Rows)
                            {
                                if (mdamt <= 0) break;
                                decimal mcamt = Convert.ToDecimal(mcrow["Pending"].ToString());
                                if (mcamt != 0 && mcamt > 0)
                                {
                                    decimal mamt = 0;
                                    if (mdamt >= mcamt) { mamt = mcamt; mdamt -= mcamt; mcamt = 0; } else { mamt = mdamt; mcamt = mcamt - mdamt; mdamt = 0; }
                                    mcrow["Pending"] = mcamt;
                                    var partycode = mdrow["Code"].ToString();
                                    var tablekey = mdrow["TableKey"].ToString();
                                    Ledger ledger = ctxTFAT.Ledger.Where(x => x.Code == partycode && x.TableKey == tablekey).FirstOrDefault();
                                    Outstanding os = new Outstanding();
                                    os.Branch = ledger.Branch;
                                    os.DocBranch = ledger.Branch;
                                    os.Code = partycode;
                                    os.ParentKey = mcrow["ParentKey"].ToString();//ledger.ParentKey;
                                    os.TableKey = ledger.TableKey;
                                    //ledger.Type + ledger.Prefix.Substring(0, 2) + ledger.Sno.ToString("D3") + ledger.Srl;
                                    os.TableRefKey = mcrow["tablekey"].ToString();
                                    os.MainType = ledger.MainType;
                                    os.SubType = ledger.SubType;
                                    os.Type = ledger.Type;
                                    os.Prefix = ledger.Prefix;
                                    os.Srl = ledger.Srl;
                                    os.Sno = ledger.Sno;
                                    os.aMaintype = mcrow["maintype"].ToString();
                                    os.aSubType = mcrow["subtype"].ToString();
                                    os.aType = mcrow["Type"].ToString();
                                    os.aPrefix = mcrow["Prefix"].ToString();
                                    os.aSrl = mcrow["Srl"].ToString();
                                    os.aSno = Convert.ToInt32(mcrow["Sno"]);
                                    os.Amount = mamt;
                                    os.AUTHIDS = muserid;
                                    os.AUTHORISE = ledger.AUTHORISE;
                                    os.BillDate = ledger.BillDate == null ? Convert.ToDateTime("1900-01-01") : ledger.BillDate;
                                    os.BillNumber = ledger.BillNumber == null ? "" : ledger.BillNumber;
                                    os.CompCode = mcompcode;
                                    os.Broker = 100001;
                                    os.Brokerage = 0;// Convert.ToDecimal(0.00);
                                    os.BrokerAmt = 0;// Convert.ToDecimal(0.00);
                                    os.BrokerOn = 0;// Convert.ToDecimal(0.00);
                                    os.ChlnDate = DateTime.Now;
                                    os.ChlnNumber = "";
                                    os.CrPeriod = 0;
                                    os.CurrName = (ledger.CurrName.ToString() == "") ? 0 : Convert.ToInt32(ledger.CurrName);
                                    os.CurrRate = 1;
                                    os.DocDate = ledger.DocDate;
                                    os.Narr = "";
                                    os.OrdDate = DateTime.Now;
                                    os.OrdNumber = "";
                                    os.RefParty = "";
                                    os.SalemanAmt = 0;
                                    os.SalemanOn = 0;
                                    os.SalemanPer = 0;
                                    os.Salesman = 100001;
                                    os.TDSAmt = 0;
                                    os.ENTEREDBY = ledger.ENTEREDBY;
                                    os.LASTUPDATEDATE = DateTime.Now;
                                    os.CurrAmount = ledger.CurrAmount;
                                    os.ValueDate = DateTime.Now;
                                    os.LocationCode = ledger.LocationCode;
                                    ctxTFAT.Outstanding.Add(os);

                                    Outstanding os2 = new Outstanding();
                                    partycode = mcrow["Code"].ToString();
                                    tablekey = mcrow["TableKey"].ToString();
                                    ledger = ctxTFAT.Ledger.Where(x => x.Code == partycode && x.TableKey == tablekey).FirstOrDefault();
                                    os2.Branch = ledger.Branch;
                                    os2.DocBranch = ledger.Branch;
                                    os2.Code = partycode;
                                    os2.ParentKey = ledger.ParentKey;
                                    os2.TableKey = ledger.TableKey;// ledger.Type + ledger.Prefix.Substring(0, 2) + ledger.Sno.ToString("D3") + ledger.Srl;
                                    os2.TableRefKey = mdrow["tablekey"].ToString();
                                    os2.MainType = ledger.MainType;
                                    os2.SubType = ledger.SubType;
                                    os2.Type = ledger.Type;
                                    os2.Prefix = ledger.Prefix;
                                    os2.Srl = ledger.Srl;
                                    os2.Sno = ledger.Sno;
                                    os2.aMaintype = mdrow["maintype"].ToString();
                                    os2.aSubType = mdrow["subtype"].ToString();
                                    os2.aType = mdrow["Type"].ToString();
                                    os2.aPrefix = mdrow["Prefix"].ToString();
                                    os2.aSrl = mdrow["Srl"].ToString();
                                    os2.aSno = Convert.ToInt32(mdrow["Sno"]);
                                    os2.Amount = mamt;
                                    os2.AUTHIDS = muserid;
                                    os2.AUTHORISE = ledger.AUTHORISE;
                                    os2.BillDate = ledger.BillDate == null ? Convert.ToDateTime("1900-01-01") : ledger.BillDate;
                                    os2.BillNumber = ledger.BillNumber == null ? "" : ledger.BillNumber;
                                    os2.CompCode = mcompcode;
                                    os2.Broker = 100001;
                                    os2.Brokerage = 0;// Convert.ToDecimal(0.00);
                                    os2.BrokerAmt = 0;// Convert.ToDecimal(0.00);
                                    os2.BrokerOn = 0;// Convert.ToDecimal(0.00);
                                    os2.ChlnDate = DateTime.Now;
                                    os2.ChlnNumber = "";
                                    os2.CrPeriod = 0;
                                    os2.CurrName = (ledger.CurrName.ToString() == "") ? 0 : Convert.ToInt32(ledger.CurrName);
                                    os2.CurrRate = 1;
                                    os2.DocDate = ConvertDDMMYYTOYYMMDD(ledger.DocDate.ToString());
                                    os2.Narr = "";
                                    os2.OrdDate = DateTime.Now;
                                    os2.OrdNumber = "";
                                    //os2.ProjCode = ledger.ProjCode;
                                    //os2.ProjectStage = ledger.ProjectStage;
                                    //os2.ProjectUnit = ledger.ProjectUnit;
                                    os2.RefParty = "";
                                    os2.SalemanAmt = 0;
                                    os2.SalemanOn = 0;
                                    os2.SalemanPer = 0;
                                    os2.Salesman = 100001;
                                    os2.TDSAmt = 0;
                                    os2.ENTEREDBY = ledger.ENTEREDBY;
                                    os2.LASTUPDATEDATE = DateTime.Now;
                                    os2.CurrAmount = ledger.CurrAmount;
                                    os2.ValueDate = DateTime.Now;
                                    os2.LocationCode = ledger.LocationCode;
                                    ctxTFAT.Outstanding.Add(os2);
                                }
                            }
                        }
                        ctxTFAT.SaveChanges();
                        trxfifo.Commit();
                        result = "Success";
                    }
                    catch (Exception ex)
                    {
                        trxfifo.Rollback();
                        result = ex.Message.ToString();
                        return Json(result, JsonRequestBehavior.AllowGet);
                        throw;
                    }
            }
            else
            {
                result = "No Data Found";
            }

            //return null;
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}