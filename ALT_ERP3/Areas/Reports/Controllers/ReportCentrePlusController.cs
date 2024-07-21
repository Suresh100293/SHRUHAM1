/* -----------------------------------------
   Copyright 2019, Suchan Software Pvt. Ltd.
   ----------------------------------------- */
using ALT_ERP3.Controllers;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Reports.Controllers
{
    public class ReportCentrePlusController : ReportController
    {
        // GET: Reports/ReportCentre
        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            //GetAllMenu(Session["ModuleName"].ToString());
            //string murl = Request.Url.ToString();
            ViewBag.list = Session["MenuList"];
            ViewBag.Modules = Session["ModulesList"];

            UpdateAuditTrail(mbranchcode, "Report", Model.Header, "", DateTime.Now, 0, "", "", "A");
            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            ViewBag.PeriodType = ctxTFAT.TfatMenu.Where(z => z.OptionCode == Model.OptionCode).Select(x => x.PeriodType).FirstOrDefault() ?? "X";
            moptioncode = Model.OptionCode;
            msubcodeof = Model.ViewDataId;
            mmaintype = Model.MainType;
            var reportheader = ctxTFAT.ReportHeader.Where(z => z.SubCodeOf == msubcodeof && z.DefaultReport == true).Select(x => new { x.Code, x.InputPara }).FirstOrDefault();
            if (reportheader == null)
            {
                reportheader = ctxTFAT.ReportHeader.Where(z => z.SubCodeOf == msubcodeof).Select(x => new { x.Code, x.InputPara }).FirstOrDefault();
            }
            if (reportheader != null)
            {
                Model.ViewCode = reportheader.Code == null ? "" : reportheader.Code;
                Model.IsFormatSelected = (Model.ViewCode == null || Model.ViewCode == "") ? false : true;
                List<string> inputlist = new List<string>();
                List<string> inputlist2 = new List<string>();
                inputlist = (reportheader.InputPara == "" || reportheader.InputPara == null) ? inputlist : reportheader.InputPara.Trim().Split('~').ToList();
                foreach (var ai in inputlist)
                {
                    if (ai != "" && ai != null)
                    {
                        var a = ai.Split('^');
                        string a1 = a[0];
                        string a2 = GetQueryText(a[1]);
                        inputlist2.Add(a1 + "^" + a2);
                    }
                }
                Model.AddOnParaList = inputlist2;
            }
            return View(Model);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            Model.MainType = mmaintype;
            ExecutePreQuery(msubcodeof, Model);
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        #region executestoredproc
        // option specific stored procedure to execute
        public void ExecutePreQuery(string msubcodeof, GridOption Model)
        {
            string mDate = Model.Date;
            string mformatcode = Model.ViewDataId;
            switch (msubcodeof)
            {
                case "StockStateBasicAdvanced":
                    ExecuteStockStatement(mDate);
                    break;
                case "StockAgeingAnalysis":
                    ExecuteStockAgeing(mDate);
                    break;
                case "DayBookItem":
                    ExecuteDayBookItem(mDate);
                    break;
                case "CashFlowStatement":
                    ExecuteCashFlow(mDate, Model.MainType, 0);
                    break;
                case "AcDisplay":
                default:
                    try
                    {
                        string mStr = ctxTFAT.ReportHeader.Where(z => z.Code == mformatcode).Select(x => x.PostLogic.Trim()).FirstOrDefault() ?? "";
                        if (mStr == "") break;

                        var mreplist = ctxTFAT.TfatSearch.Where(z => z.Code == mformatcode && z.ColHead == "%pivotcolumns").Select(x => new { x.Sno, x.ColWidth, x.ColType, x.ColHead, x.Decs }).FirstOrDefault();
                        int msno = mreplist.Sno;
                        var mreplistdel = ctxTFAT.TfatSearch.Where(z => z.Code == mformatcode && z.Sno > msno).Select(x => x).ToList();
                        if (mreplistdel != null) ctxTFAT.TfatSearch.RemoveRange(mreplistdel);

                        var mpara = mStr.Split(';');
                        try { ctxTFAT.Database.ExecuteSqlCommand("Drop Table ztmp_" + mformatcode); } catch { }
                        DataTable mcolsdt = new DataTable();
                        mcolsdt = GetDataTable(mpara[0].ToString());
                        mStr = "";
                        for (int x = 0; x <= mcolsdt.Rows.Count - 1; x++)
                        {
                            mStr = mStr + "[" + mcolsdt.Rows[x][0].ToString().Replace("[", "").Replace("]", "") + "],";

                            TfatSearch mobj = new TfatSearch();
                            mobj.Code = mformatcode;
                            mobj.Sno = ++msno;
                            mobj.AllowEdit = false;
                            mobj.AUTHIDS = muserid;
                            mobj.AUTHORISE = "A00";
                            mobj.BackColor = "";
                            mobj.CalculatedCol = false;
                            mobj.ChildOfCol = "";
                            mobj.ColChars = 0;
                            mobj.ColCondition = "";
                            mobj.ColField = "[" + mcolsdt.Rows[x][0].ToString().Replace("[", "").Replace("]", "") + "]";
                            mobj.ColHead = mcolsdt.Rows[x][0].ToString().Replace("[", "").Replace("]", "");
                            mobj.ColPosition = 0;
                            mobj.ColType = mreplist.ColType;
                            mobj.ColWidth = 75;
                            mobj.YesTotal = true;
                            mobj.Comma = false;
                            mobj.CondBCLR = "";
                            mobj.Decs = mreplist.Decs;
                            mobj.DisplayGrid = false;
                            mobj.ENTEREDBY = muserid;
                            mobj.FitToHeight = false;
                            mobj.FitToWidth = false;
                            mobj.FldAs = "PivotFld" + x;
                            mobj.FontBold = false;
                            mobj.FontItalics = false;
                            mobj.FontName = "Tahoma";
                            mobj.FontSize = 9;
                            mobj.FontStrike = false;
                            mobj.FontUnderLine = false;
                            mobj.ForeColor = "";
                            mobj.FormatString = "";
                            mobj.GraphCol = false;
                            mobj.GroupHead = "";
                            mobj.Idle = false;
                            mobj.IsHidden = false;
                            mobj.LASTUPDATEDATE = DateTime.Today;
                            mobj.Locked = false;
                            mobj.MergeCond = "";
                            mobj.MergeData = false;
                            mobj.Modules = "";
                            mobj.SplitColumn = "";
                            mobj.SystemColumn = "";
                            mobj.xCurrency = false;
                            ctxTFAT.TfatSearch.Add(mobj);
                        }
                        ctxTFAT.SaveChanges();
                        mStr = CutRightString(mStr, 1, ",");

                        string mQry = "SELECT " + mpara[2].ToString() + "," + mStr;
                        mQry = mQry + " into ztmp_" + mformatcode + " FROM ";
                        mQry = mQry + "(";
                        mQry = mQry + "  " + mpara[1].ToString();
                        mQry = mQry + ") AS firstquery";
                        mQry = mQry + " PIVOT";
                        mQry = mQry + "(";
                        mQry = mQry + " SUM(" + mpara[4].ToString() + ") FOR " + mpara[3].ToString() + " IN (" + mStr + ")";
                        mQry = mQry + ") AS secondquery;";
                        var date = mDate.Replace("-", "/").Split(':');
                        mQry = mQry.Replace("%RepStartDate", (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture));
                        mQry = mQry.Replace("%RepEndDate", (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture));
                        mQry = mQry.Replace("%Branch", mbranchcode);
                        mQry = mQry.Replace("%branch", mbranchcode);
                        if (pwarehouse != "")
                        {
                            mQry = mQry.Replace("%warehouse", pwarehouse.Replace("'", ""));
                            mQry = mQry.Replace("%locationcode", pwarehouse);
                        }
                        else
                        {
                            mQry = mQry.Replace("%warehouse", mlocationcode.ToString());
                            mQry = mQry.Replace("%locationcode", mlocationcode.ToString());
                        }
                        mQry = ReplaceVariables(mQry);
                        ctxTFAT.Database.ExecuteSqlCommand(mQry);
                        ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set SelectQuery='' Where Code='" + mformatcode + "'");
                    }
                    catch (Exception e) { }
                    break;
                    // example
                    //SELECT Code,' + @columns + '
                    //into ztmp_stockstatestore FROM
                    //(
                    //  SELECT Stock.Code, Stores.Name, Stock.Qty
                    //  FROM Stock, Stores
                    //  Where Stores.Code = Stock.Store and Stock.DocDate >= '''+@mDate1+''' and Stock.DocDate <= '''+@mDate2+''' and Stock.Branch = '''+@mBranch +'''
                    //) AS j
                    //PIVOT
                    //(
                    //  SUM(Qty) FOR Name IN('+ @columns+ ')
                    //) AS p; ';
            }
        }

        private void ExecuteCashFlow(string mDate, string mwhat, int mdaterange)
        {
            string mStr = "";
            var date = mDate.Replace("-", "/").Split(':');
            DateTime mDate1 = Convert.ToDateTime(date[0]);
            DateTime mDate2 = Convert.ToDateTime(date[1]);

            string mcashaccounts = RecToString("Select Code from Master Where BaseGr='B' or BaseGr='C'", ",");
            //decimal mOpening = FieldoftableNumber("Ledger", "Sum(Debit-Credit) from Ledger", "Code in ('" + mcashaccounts + "') and DocDate<'" + MMDDYY(mDate1) + "' And Branch='" + mbranchcode + "' And Left(Authorse,1)='A'" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(Type,'" + mDocString + "')=0"));

            if (mdaterange == 0)
            {
            }
            else if (mdaterange == 7 || mdaterange == 14 || mdaterange == 30)  // days
            {
                DateTime mdt;
                mDate1 = Convert.ToDateTime(System.Web.HttpContext.Current.Session["StartDate"].ToString() ?? DateTime.Now.Date.ToString());
                mDate2 = DateTime.Now.Date;
                for (int x = 1; x <= mdaterange; x++)
                {
                    mdt = mDate2.AddDays(1 + x);
                    mStr = mStr + "sum(Case l.DocDate+l.CrPeriod When '" + MMDDYY(mdt) + "' Then l.Credit Else 0 End) As [" + mdt.ToString("dd/mm/yyyy") + "],";
                }
            }
            else if (mdaterange == 3 || mdaterange == 6 || mdaterange == 12)  // months
            {
                DateTime mdt1;
                DateTime mdt2;
                for (int x = 1; x <= mdaterange; x++)
                {
                    mdt1 = mDate2.AddMonths(x - 1);
                    mdt2 = mDate2.AddMonths(x);
                    mStr = mStr + "sum(Case l.DocDate+l.CrPeriod When >='" + MMDDYY(mdt1) + "' And l.DocDate+l.CrPeriod <='" + MMDDYY(mdt2) + "' Then l.Credit Else 0 End) As [" + mdt1.ToString("dd/mm/yyyy") + " to " + mdt2.ToString("dd/mm/yyyy") + "],";
                }
            }
            mStr = CutRightString(mStr, 1, ",");
            ExecuteStoredProc("SPTFAT_DeleteTempTable ztmp_CashFlow");
            string mSQL = "";
            if (ppara02 == "Group wise")
            {
                mSQL = "Select '        ' as Type, a.Name as [Name],a.Code, sum(l.Debit) as [Debit], sum(l.Credit) as [Credit]" + (mStr != "" ? "," + mStr : "") + " into ztmp_CashFlow from ledger l, Master m, MasterGroups a " +
                    "where m.code=l.code and m.grp=a.code " + (mwhat == "F" ? " and (CharIndex('000000011',m.GroupTree)<>0 or CharIndex('000000019',m.GroupTree)<>0)" : "") +
                    " and CharIndex(l.Code,'" + mcashaccounts + "')=0 And l.DocDate Between '" + MMDDYY(mDate1) + "' And '" + MMDDYY(mDate2) + "' And l.Branch = '" + mbranchcode + "' and (l.MainType<>'MV' And l.MainType<>'PV') And Left(l.Authorise,1) = 'A' " +
                    " And l.Parentkey in (Select Parentkey from Ledger x where x.DocDate Between '" + MMDDYY(mDate1) + "' and '" + MMDDYY(mDate2) + "' And Charindex(Code,'" + mcashaccounts + "')<>0 And x.Branch = '" + mbranchcode + "' And (x.MainType<>'MV' And x.MainType<>'PV') And Left(x.Authorise,1) = 'A') Group by a.Name,a.Code";
            }
            else
            {
                mSQL = "Select '        ' as Type, m.Name as [Name],m.Code, sum(l.Debit) as [Debit], sum(l.Credit) as [Credit]" + (mStr != "" ? "," + mStr : "") + " into ztmp_CashFlow from ledger l, Master m " +
                    "where m.code=l.code " + (mwhat == "F" ? " and (CharIndex('000000011',m.GroupTree)<>0 or CharIndex('000000019',m.GroupTree)<>0)" : "") +
                    " and CharIndex(l.Code,'" + mcashaccounts + "')=0 And l.DocDate Between '" + MMDDYY(mDate1) + "' And '" + MMDDYY(mDate2) + "' And l.Branch = '" + mbranchcode + "' and (l.MainType<>'MV' And l.MainType<>'PV') And Left(l.Authorise,1) = 'A' " +
                    " And l.Parentkey in (Select Parentkey from Ledger x where x.DocDate Between '" + MMDDYY(mDate1) + "' and '" + MMDDYY(mDate2) + "' And Charindex(Code,'" + mcashaccounts + "')<>0 And x.Branch = '" + mbranchcode + "' And (x.MainType<>'MV' And x.MainType<>'PV') And Left(x.Authorise,1) = 'A') Group by m.Name,m.Code";
            }
            ExecuteStoredProc(mSQL);
            ExecuteStoredProc("Update ztmp_CashFlow set Type=(Case When Debit<>0 then 'Outflow' Else 'InFlow' End)");
        }

        private void ExecuteDayBookItem(string mDate)
        {
            //DataTable dtreport = new DataTable();
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("SPTFAT_ItemDayBooks", tfat_conx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            var date = mDate.Replace("-", "/").Split(':');
            cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
            cmd.Parameters.Add("@mDate1", SqlDbType.VarChar).Value = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            cmd.Parameters.Add("@mDate2", SqlDbType.VarChar).Value = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            cmd.Parameters.Add("@mItems", SqlDbType.VarChar).Value = pitems;
            cmd.Parameters.Add("@mLocations", SqlDbType.VarChar).Value = pwarehouse;
            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();
            tfat_conx.Close();
            cmd.Dispose();
            tfat_conx.Dispose();
        }

        private void ExecuteStockStatement(string mDate)
        {
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("SPTFAT_StockStatement", tfat_conx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            var date = mDate.Replace("-", "/").Split(':');
            ppara07 = (ppara07 ?? "") == "" ? "'" + mbranchcode + "'" : ppara07;
            if (!ppara07.StartsWith("'")) ppara07 = "'" + ppara07 + "'";
            cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = ppara07;
            cmd.Parameters.Add("@mDate1", SqlDbType.VarChar).Value = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            cmd.Parameters.Add("@mDate2", SqlDbType.VarChar).Value = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            cmd.Parameters.Add("@mStores", SqlDbType.VarChar).Value = pstores;
            cmd.Parameters.Add("@mTypes", SqlDbType.VarChar).Value = ptypes;
            cmd.Parameters.Add("@mCategory", SqlDbType.VarChar).Value = pitemcat;
            cmd.Parameters.Add("@mItemGroups", SqlDbType.VarChar).Value = pitemgroups;
            cmd.Parameters.Add("@mItems", SqlDbType.VarChar).Value = pitems;
            cmd.Parameters.Add("@mLocations", SqlDbType.VarChar).Value = pwarehouse;
            cmd.Parameters.Add("@mTransfer", SqlDbType.Bit).Value = (ppara01 == "Yes" ? -1 : 0);
            cmd.Parameters.Add("@mNonStock", SqlDbType.Bit).Value = (ppara02 == "Yes" ? -1 : 0);
            cmd.Parameters.Add("@mInActive", SqlDbType.Bit).Value = (ppara03 == "Yes" ? -1 : 0);
            cmd.Parameters.Add("@mFilter", SqlDbType.Int).Value = ppara04 == "" ? 0 : Convert.ToInt32(ppara04.Substring(0, 1));
            cmd.Parameters.Add("@mCostRate", SqlDbType.Bit).Value = (ppara05 == "Yes" ? -1 : 0);
            cmd.Parameters.Add("@mMethod", SqlDbType.VarChar).Value = (ppara06 == "" ? "X" : ppara06.Substring(0, 1));
            cmd.Parameters.Add("@mDocString", SqlDbType.VarChar).Value = mDocString;
            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();
            tfat_conx.Close();
            cmd.Dispose();
            tfat_conx.Dispose();
        }

        private void ExecuteStockAgeing(string mDate)
        {
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("SPTFAT_StockAgeing", tfat_conx);
            cmd.CommandTimeout = 0;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            var date = mDate.Replace("-", "/").Split(':');
            cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
            cmd.Parameters.Add("@mItemGroups", SqlDbType.VarChar).Value = pitemgroups.Replace("'", "");
            cmd.Parameters.Add("@mItems", SqlDbType.VarChar).Value = pitems.Replace("'", "");
            cmd.Parameters.Add("@mStore", SqlDbType.Int).Value = Convert.ToInt16(pstores == "" ? "0" : pstores);
            cmd.Parameters.Add("@mDate", SqlDbType.VarChar).Value = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();
            tfat_conx.Close();
            cmd.Dispose();
            tfat_conx.Dispose();
        }
        #endregion executestoredproc
    }
}