using Common;
//using EntitiModel;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using ALT_ERP3.Controllers;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class GSTR3BController : BaseController
    {
        // GET: Reports/GSTR1
        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "A");
            if (Model.Document == "" || Model.Document == null)
            {
                GetAllMenu(Session["ModuleName"].ToString());
                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            }
            ViewBag.ViewDataId = Model.ViewDataId;
            ViewBag.Header = Model.Header;
            ViewBag.Controller = Model.Controller;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;
            return View(Model);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            return GetGridDataColumns("GSTR3B-" + Model.ViewDataId.ToUpper(), "X", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            //chkRCM = Include Reverse Charge for Eligible ITC
            //chkAdvance = Include Advance Receipts for Output
            //chkReturnS = Treat Sales Return as Purchase (Inward)
            //chkReturnP = Treat Purchase Return as Sales (Outward)
            //chkNOITC = Dont Include 'NOT ELEGIBLE FOR ITC' transactions in 4 - A table
            //chkReverse = Include Reverse Charge Transactions in 3.1 - D Table 
            bool chkRCM = Model.xAdd;
            //bool chkAdvance = Model.xEdit;
            bool chkReturnS = Model.xDelete;
            bool chkReturnP = Model.xView;
            bool chkNOITC = Model.IsFormatSelected;
            bool chkReverse = Model.ComController;
            int mcnt = 1;
            string mcode = Model.ViewDataId;
            string mSQL = "";
            string mSQL2 = "";
            string morderby = "t.Type,t.Srl";
            string mCurrState = "";
            var date = Model.Date.Replace("-", "/").Split(':');
            Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            string mWhere = " And s.Branch = '" + mbranchcode + "' And s.DocDate Between '" + MMDDYY(Convert.ToDateTime(Model.FromDate)) + "' And '" + MMDDYY(Convert.ToDateTime(Model.ToDate)) + "'" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(s.Type,'" + mDocString + "')=0");

            DateTime mDate1 = Convert.ToDateTime(Model.FromDate);
            DateTime mDate2 = Convert.ToDateTime(Model.ToDate);

            // get select fields from tfatsearch
            //string connstring = string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings[""].ConnectionString) == true ? "" : ConfigurationManager.ConnectionStrings[""].ConnectionString;
            string mconnstr = GetConnectionString();
            //SqlConnection con = new SqlConnection(mconnstr);
            decimal mtCol1 = 0;
            decimal mtCol2 = 0;
            decimal mtCol3 = 0;
            decimal mtCol4 = 0;
            decimal mtCol5 = 0;
            decimal mtCol6 = 0;

            // Create a new DataTable.
            DataTable mdt = new DataTable("gstr3b");
            DataColumn dtColumn;
            DataRow myDataRow;
            dtColumn = new DataColumn();
            dtColumn.DataType = typeof(Int32);
            dtColumn.ColumnName = "RecordKey";
            dtColumn.Unique = true;
            mdt.Columns.Add(dtColumn);
            DataColumn[] PrimaryKeyColumns = new DataColumn[1];
            PrimaryKeyColumns[0] = mdt.Columns["RecordKey"];
            mdt.PrimaryKey = PrimaryKeyColumns;                      // Create a new DataSet and table to DS
            switch (mcode)
            {
                case "outward":
                    // 3.1 Details of Outward Supplies and Inward supplies liable to Reverse Charges"
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "Nature of Supplies";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "Total Taxable";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "Integrated Tax";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "Central Tax";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "State Tax";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "Cess";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    mcnt = 1;
                    //Table header
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Nature of Supplies"] = "3.1 Details of Outward Supplies and Inward Supplies liable to Reverse Charges";
                    myDataRow["Total Taxable"] = "";
                    myDataRow["Integrated Tax"] = "";
                    myDataRow["Central Tax"] = "";
                    myDataRow["State Tax"] = "";
                    myDataRow["Cess"] = "";
                    mdt.Rows.Add(myDataRow);
                    // row 1
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Nature of Supplies"] = "1";
                    myDataRow["Total Taxable"] = "2";
                    myDataRow["Integrated Tax"] = "3";
                    myDataRow["Central Tax"] = "4";
                    myDataRow["State Tax"] = "5";
                    myDataRow["Cess"] = "6";
                    mdt.Rows.Add(myDataRow);
                    //
                    //(a) Outward Taxable Supplies (other than Zero rated, NIL rated and Exempted)"
                    //And i.ItemType='I' 
                    mSQL2 = "Select isnull(Sum(x.iGSTAmt),0) as mIGST,isnull(Sum(x.cGSTAmt),0) as mCGST,isnull(Sum(x.sGSTAmt),0) as mSGST,0.00 as mCess,isnull(Sum(x.Taxable),0) as mTaxable from StockTax x,Stock s,ItemMaster i Where i.Code=s.Code And x.TableKey=s.TableKey And (x.GSTType=0 or x.GSTType=7 or x.GSTType=13 or x.GSTType=15) And (x.SubType='XS' or x.SubType='RS' or x.SubType='CS' or x.SubType='US' " + (chkReturnS == false ? " or x.SubType='NS' " : "") + (chkReturnP == true ? " or x.SubType='NP' " : "") + ") And (x.IGSTAmt+x.CGSTAmt+x.SGSTAmt)<>0 " + mWhere;
                    DataTable mdata = GetDataTable(mSQL2, mconnstr);
                    //SqlCommand cmd = new SqlCommand("Select dbo.GetGridQuery(@mFormatCode,@mAlias,@mCurrDec,@mBranch,@mUserID,@mReportStartDate,@mReportEndDate,@mReturn)", con);
                    //cmd.Parameters.Add("@mFormatCode", SqlDbType.VarChar).Value = "GSTR1-" + Model.ViewDataId.ToUpper();
                    //cmd.Parameters.Add("@mAlias", SqlDbType.VarChar).Value = "";
                    //cmd.Parameters.Add("@mCurrDec", SqlDbType.TinyInt).Value = 2;
                    //cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
                    //cmd.Parameters.Add("@mUserID", SqlDbType.VarChar).Value = muserid;
                    //cmd.Parameters.Add("@mReportStartDate", SqlDbType.VarChar).Value = Model.FromDate;
                    //cmd.Parameters.Add("@mReportEndDate", SqlDbType.VarChar).Value = Model.ToDate;
                    //cmd.Parameters.Add("@mReturn", SqlDbType.VarChar).Value = "B";
                    //con.Open();
                    //mSQL = "Select " + cmd.ExecuteScalar().ToString();
                    decimal mCol1 = 0;
                    decimal mCol2 = 0;
                    decimal mCol3 = 0;
                    decimal mCol4 = 0;
                    decimal mCol5 = 0;
                    if (mdata != null)
                    {
                        mCol1 = Math.Abs((decimal)mdata.Rows[0]["mIGST"]);
                        mCol2 = Math.Abs((decimal)mdata.Rows[0]["mCGST"]);
                        mCol3 = Math.Abs((decimal)mdata.Rows[0]["mSGST"]);
                        mCol4 = Math.Abs((decimal)mdata.Rows[0]["mCess"]);
                        mCol5 = Math.Abs((decimal)mdata.Rows[0]["mTaxable"]);
                    }
                    mdata.Dispose();
                    mSQL2 = RecToString("Select Distinct IGSTCode from TaxMaster where Scope='S' and IGSTCode<>''", "^");
                    mCol1 -= FieldoftableNumber("Ledger s", "isnull(Sum(Debit*100/IgstRate),0)", "(SubType='DN' or SubType='CN') And CharIndex(Code,'" + mSQL2 + "')<>0 And Branch='" + mbranchcode + "'" + mWhere);
                    mCol2 -= FieldoftableNumber("Ledger s", "isnull(Sum(Debit-Credit),0)", "(SubType='DN' or SubType='CN') And CharIndex(Code,'" + mSQL2 + "')<>0 And Branch='" + mbranchcode + "'" + mWhere);
                    mSQL2 = RecToString("Select Distinct CGSTCode from TaxMaster where Scope='S' and CGSTCode<>''", "^");
                    //mSQL2 = RecToString("Select Distinct cGSTOut from HSNMaster where cGSTOut<>''", "^");
                    mCol1 -= FieldoftableNumber("Ledger s", "isnull(Sum(Debit*100/IgstRate),0)", "(SubType='DN' or SubType='CN') And CharIndex(Code,'" + mSQL2 + "')<>0 And Branch='" + mbranchcode + "'" + mWhere);
                    mCol3 -= FieldoftableNumber("Ledger s", "isnull(Sum(Debit-Credit),0)", "(SubType='DN' or SubType='CN') And CharIndex(Code,'" + mSQL2 + "')<>0 And Branch='" + mbranchcode + "'" + mWhere);
                    mSQL2 = RecToString("Select Distinct SGSTCode from TaxMaster where Scope='S' and SGSTCode<>''", "^");
                    mCol1 -= FieldoftableNumber("Ledger s", "isnull(Sum(Debit*100/IgstRate),0)", "(SubType='DN' or SubType='CN') And CharIndex(Code,'" + mSQL2 + "')<>0 And Branch='" + mbranchcode + "'" + mWhere);
                    mCol4 -= FieldoftableNumber("Ledger s", "isnull(Sum(Debit-Credit),0)", "(SubType='DN' or SubType='CN') And CharIndex(Code,'" + mSQL2 + "')<>0 And Branch='" + mbranchcode + "'" + mWhere);
                    //
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Nature of Supplies"] = "   (a) Outward Taxable Supplies (other than Zero rated, NIL rated and Exempted)";
                    myDataRow["Total Taxable"] = mCol5;
                    myDataRow["Integrated Tax"] = mCol1;
                    myDataRow["Central Tax"] = mCol2;
                    myDataRow["State Tax"] = mCol3;
                    myDataRow["Cess"] = mCol4;
                    mtCol1 += mCol1;
                    mtCol2 += mCol2;
                    mtCol3 += mCol3;
                    mtCol4 += mCol4;
                    mtCol5 += mCol5;
                    mdt.Rows.Add(myDataRow);
                    //(b) Outward Taxable Supplies (Zero rated)
                    mSQL2 = "Select isnull(Sum(x.Taxable),0) as mTaxable,isnull(Sum(x.iGSTAmt),0) as mIGST,isnull(Sum(x.cGSTAmt),0) as mCGST,isnull(Sum(x.sGSTAmt),0) as mSGST,0.00 as mCess from StockTax x,ItemMaster i,Stock s ";
                    mSQL2 += "Where x.TableKey=s.TableKey and i.Code=s.Code and i.ItemType='I' And (x.GSTType=5 or x.GSTType=8 or x.GSTType=9) And (x.SubType='XS' or x.SubType='RS' or x.SubType='CS' or x.SubType='NS' or x.SubType='EX' or x.SubType='US') " + mWhere;
                    mdata = GetDataTable(mSQL2, mconnstr);
                    mCol1 = 0;
                    mCol2 = 0;
                    mCol3 = 0;
                    mCol4 = 0;
                    mCol5 = 0;
                    if (mdata != null)
                    {
                        mCol1 = Math.Abs((decimal)mdata.Rows[0]["mIGST"]);
                        mCol2 = Math.Abs((decimal)mdata.Rows[0]["mCGST"]);
                        mCol3 = Math.Abs((decimal)mdata.Rows[0]["mSGST"]);
                        mCol4 = Math.Abs((decimal)mdata.Rows[0]["mCess"]);
                        mCol5 = Math.Abs((decimal)mdata.Rows[0]["mTaxable"]);
                    }
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Nature of Supplies"] = "   (b) Outward Taxable Supplies (Zero rated)";
                    myDataRow["Total Taxable"] = mCol5;
                    myDataRow["Integrated Tax"] = mCol1;
                    myDataRow["Central Tax"] = mCol2;
                    myDataRow["State Tax"] = mCol3;
                    myDataRow["Cess"] = mCol4;
                    mtCol1 += mCol1;
                    mtCol2 += mCol2;
                    mtCol3 += mCol3;
                    mtCol4 += mCol4;
                    mtCol5 += mCol5;
                    mdt.Rows.Add(myDataRow);
                    // (c) Other Outward Taxable Supplies (NIL rated and Exempted)
                    mSQL2 = "Select isnull(Sum(x.Taxable),0) as mTaxable,isnull(Sum(x.iGSTAmt),0) as mIGST,isnull(Sum(x.cGSTAmt),0) as mCGST,isnull(Sum(x.sGSTAmt),0) as mSGST,0.00 as mCess from StockTax x,Stock s,ItemMaster i Where i.Code=s.Code And x.TableKey=s.TableKey and ";
                    mSQL2 += "i.ItemType='I' And ((x.GSTType=0 And (x.IGSTRate+x.CGSTRate+x.SGSTRate)=0) or x.GSTType=10) And (x.SubType='XS' or x.SubType='RS' or x.SubType='CS' or x.SubType='NS' or x.SubType='US') " + mWhere;
                    mdata = GetDataTable(mSQL2, mconnstr);
                    mCol1 = 0;
                    mCol2 = 0;
                    mCol3 = 0;
                    mCol4 = 0;
                    mCol5 = 0;
                    if (mdata != null)
                    {
                        mCol1 = Math.Abs((decimal)mdata.Rows[0]["mIGST"]);
                        mCol2 = Math.Abs((decimal)mdata.Rows[0]["mCGST"]);
                        mCol3 = Math.Abs((decimal)mdata.Rows[0]["mSGST"]);
                        mCol4 = Math.Abs((decimal)mdata.Rows[0]["mCess"]);
                        mCol5 = Math.Abs((decimal)mdata.Rows[0]["mTaxable"]);
                    }
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Nature of Supplies"] = "   (c) Other Outward Taxable Supplies (NIL rated and Exempted)";
                    myDataRow["Total Taxable"] = mCol5;
                    myDataRow["Integrated Tax"] = mCol1;
                    myDataRow["Central Tax"] = mCol2;
                    myDataRow["State Tax"] = mCol3;
                    myDataRow["Cess"] = mCol4;
                    mtCol1 += mCol1;
                    mtCol2 += mCol2;
                    mtCol3 += mCol3;
                    mtCol4 += mCol4;
                    mtCol5 += mCol5;
                    mdt.Rows.Add(myDataRow);
                    //(d) Inward Supplies (liable to reverse charge)
                    if (chkReverse == true)
                    {
                        //i.ItemType='I' And 
                        mSQL2 = "Select isnull(Sum(x.Taxable),0) as mTaxable,isnull(Sum(x.iGSTAmt),0) as mIGST,isnull(Sum(x.cGSTAmt),0) as mCGST,isnull(Sum(x.sGSTAmt),0) as mSGST,0.00 as mCess from StockTax x,Stock s,ItemMaster i Where i.Code=s.Code And x.TableKey=s.TableKey and ";
                        mSQL2 += " " + (chkNOITC == true ? " x.GSTNoITC=0 And " : "") + " (x.GSTType=1 or x.GSTType=6) And (s.MainType='PM' or s.MainType='JV' or s.SubType='RP' or s.SubType='CP' or s.SubType='NP') " + mWhere;//And (x.IGSTAmt+x.CGSTAmt+x.SGSTAmt)<>0
                        mdata = GetDataTable(mSQL2, mconnstr);
                        mCol1 = 0;
                        mCol2 = 0;
                        mCol3 = 0;
                        mCol4 = 0;
                        mCol5 = 0;
                        if (mdata != null)
                        {
                            mCol1 = (decimal)mdata.Rows[0]["mIGST"];
                            mCol2 = (decimal)mdata.Rows[0]["mCGST"];
                            mCol3 = (decimal)mdata.Rows[0]["mSGST"];
                            mCol4 = (decimal)mdata.Rows[0]["mCess"];
                            mCol5 = (decimal)mdata.Rows[0]["mTaxable"];
                        }
                        myDataRow = mdt.NewRow();
                        myDataRow["RecordKey"] = mcnt++;
                        myDataRow["Nature of Supplies"] = "   (d) Inward Supplies (liable to reverse charge)";
                        myDataRow["Total Taxable"] = mCol5;
                        myDataRow["Integrated Tax"] = mCol1;
                        myDataRow["Central Tax"] = mCol2;
                        myDataRow["State Tax"] = mCol3;
                        myDataRow["Cess"] = mCol4;
                        mtCol1 += mCol1;
                        mtCol2 += mCol2;
                        mtCol3 += mCol3;
                        mtCol4 += mCol4;
                        mtCol5 += mCol5;
                        mdt.Rows.Add(myDataRow);
                    }
                    //(e) Non-GST Outward Supplies
                    mSQL2 = "Select isnull(Sum(x.Taxable),0) as mTaxable,isnull(Sum(x.iGSTAmt),0) as mIGST,isnull(Sum(x.cGSTAmt),0) as mCGST,isnull(Sum(x.sGSTAmt),0) as mSGST,0.00 as mCess from StockTax x,Stock s,ItemMaster i Where i.Code=s.Code And x.TableKey=s.TableKey and ";
                    mSQL2 += "i.ItemType='I' And x.GSTType=11 And (x.SubType='XS' or s.SubType='RS' or s.SubType='CS' or s.SubType='NS' or s.SubType='US') " + mWhere;
                    mdata = GetDataTable(mSQL2, mconnstr);
                    mCol1 = 0;
                    mCol2 = 0;
                    mCol3 = 0;
                    mCol4 = 0;
                    mCol5 = 0;
                    if (mdata != null)
                    {
                        mCol1 = Math.Abs((decimal)mdata.Rows[0]["mIGST"]);
                        mCol2 = Math.Abs((decimal)mdata.Rows[0]["mCGST"]);
                        mCol3 = Math.Abs((decimal)mdata.Rows[0]["mSGST"]);
                        mCol4 = Math.Abs((decimal)mdata.Rows[0]["mCess"]);
                        mCol5 = Math.Abs((decimal)mdata.Rows[0]["mTaxable"]);
                    }
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Nature of Supplies"] = "   (e) Non-GST Outward Supplies";
                    myDataRow["Total Taxable"] = mCol5;
                    myDataRow["Integrated Tax"] = mCol1;
                    myDataRow["Central Tax"] = mCol2;
                    myDataRow["State Tax"] = mCol3;
                    myDataRow["Cess"] = mCol4;
                    mtCol1 += mCol1;
                    mtCol2 += mCol2;
                    mtCol3 += mCol3;
                    mtCol4 += mCol4;
                    mtCol5 += mCol5;
                    mdt.Rows.Add(myDataRow);
                    //Total
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Nature of Supplies"] = "Total";
                    myDataRow["Total Taxable"] = mtCol5;
                    myDataRow["Integrated Tax"] = mtCol1;
                    myDataRow["Central Tax"] = mtCol2;
                    myDataRow["State Tax"] = mtCol3;
                    myDataRow["Cess"] = mtCol4;
                    mdt.Rows.Add(myDataRow);
                    break;
                case "urd":
                    // 3.2  Of the supplies shown in 3.1 (a), details of inter-state supplies made to unregistered persons, composition taxable person and UIN holders
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "Place of Supply";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "URD Taxable";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "URD IGST";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "Composition Taxable";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "Composition IGST";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "UIN Taxable";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "UIN IGST";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    mcnt = 1;
                    //Table header
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Place of Supply"] = "3.2 Of the supplies shown in 3.1 (a), details of inter-state supplies made to unregistered persons, composition taxable person and UIN holders";
                    myDataRow["URD Taxable"] = "";
                    myDataRow["URD IGST"] = "";
                    myDataRow["Composition Taxable"] = "";
                    myDataRow["Composition IGST"] = "";
                    myDataRow["UIN Taxable"] = "";
                    myDataRow["UIN IGST"] = "";
                    mdt.Rows.Add(myDataRow);
                    // row 1
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Place of Supply"] = "1";
                    myDataRow["URD Taxable"] = "2";
                    myDataRow["URD IGST"] = "3";
                    myDataRow["Composition Taxable"] = "4";
                    myDataRow["Composition IGST"] = "5";
                    myDataRow["UIN Taxable"] = "6";
                    myDataRow["UIN IGST"] = "7";
                    mdt.Rows.Add(myDataRow);
                    //
                    mSQL2 = "Select a.StateCode + '-' + a.State as State, ";
                    mSQL2 += "isnull(Sum(Case When a.DealerType = 1 Then x.Taxable Else 0 End),0) as mtxbl1, isnull(Sum(Case When a.DealerType = 1 Then x.iGSTAmt Else 0 End),0) as migst1, isnull(Sum(Case When a.DealerType = 2 Then x.Taxable Else 0 End),0) as mtxbl2, isnull(Sum(Case When a.DealerType = 2 Then x.iGSTAmt Else 0 End),0) as migst2, isnull(Sum(Case When a.DealerType = 3 Then x.Taxable Else 0 End),0) as mtxbl2, isnull(Sum(Case When a.DealerType = 3 Then x.iGSTAmt Else 0 End),0) as migst3";
                    mSQL2 += " from StockTax x,Address a,ItemMaster i,Stock s Where i.Code=s.Code and s.TableKey=x.TableKey and i.ItemType='I' And a.Code=x.Party and a.Sno=x.AltAddress And (s.SubType='JS' or s.SubType='XS' or s.SubType='RS' or s.SubType='CS' or s.SubType='NS' or s.SubType='US') And x.IGSTAmt<>0 And (a.DealerType=1 or a.DealerType=2 or a.DealerType=3)" + mWhere;
                    mSQL2 += " Group by a.StateCode + '-' + a.State";
                    mdata = GetDataTable(mSQL2, mconnstr);
                    if (mdata != null)
                    {
                        foreach (DataRow mrow in mdata.Rows)
                        {
                            myDataRow = mdt.NewRow();
                            myDataRow["RecordKey"] = mcnt++;
                            myDataRow["Place of Supply"] = mrow["State"].ToString();
                            myDataRow["URD Taxable"] = (decimal)mrow["mtxbl1"];
                            myDataRow["URD IGST"] = (decimal)mrow["migst1"];
                            myDataRow["Composition Taxable"] = (decimal)mrow["mtxbl2"];
                            myDataRow["Composition IGST"] = (decimal)mrow["migst2"];
                            myDataRow["UIN Taxable"] = (decimal)mrow["mtxbl3"];
                            myDataRow["UIN IGST"] = (decimal)mrow["migst3"];
                            mdt.Rows.Add(myDataRow);
                            mtCol1 += (decimal)mdata.Rows[0]["mtxbl1"]; ;
                            mtCol2 += (decimal)mdata.Rows[0]["migst1"]; ;
                            mtCol3 += (decimal)mdata.Rows[0]["mtxbl2"]; ;
                            mtCol4 += (decimal)mdata.Rows[0]["migst2"]; ;
                            mtCol5 += (decimal)mdata.Rows[0]["mtxbl3"]; ;
                            mtCol6 += (decimal)mdata.Rows[0]["migst3"]; ;
                        }
                        myDataRow = mdt.NewRow();
                        myDataRow["RecordKey"] = mcnt++;
                        myDataRow["Place of Supply"] = "Total";
                        myDataRow["URD Taxable"] = mtCol1;
                        myDataRow["URD IGST"] = mtCol2;
                        myDataRow["Composition Taxable"] = mtCol3;
                        myDataRow["Composition IGST"] = mtCol4;
                        myDataRow["UIN Taxable"] = mtCol5;
                        myDataRow["UIN IGST"] = mtCol6;
                        mdt.Rows.Add(myDataRow);
                    }
                    break;
                case "itc":
                    // Eligible ITC
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "Details";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "Integrated Tax";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "Central Tax";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "State Tax";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "Cess";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    mcnt = 1;
                    //Table header
                    //myDataRow = mdt.NewRow();
                    //myDataRow["RecordKey"] = mcnt++;
                    //myDataRow["Details"] = "4. Eligible ITC";
                    //myDataRow["Integrated Tax"] = "";
                    //myDataRow["Central Tax"] = "";
                    //myDataRow["State Tax"] = "";
                    //myDataRow["Cess"] = "";
                    //mdt.Rows.Add(myDataRow);
                    //// row 1
                    //myDataRow = mdt.NewRow();
                    //myDataRow["RecordKey"] = mcnt++;
                    //myDataRow["Details"] = "Details";
                    //myDataRow["Integrated Tax"] = "Integrated Tax";
                    //myDataRow["Central Tax"] = "Central Tax";
                    //myDataRow["State Tax"] = "State/UT Tax";
                    //myDataRow["Cess"] = "Cess";
                    //mdt.Rows.Add(myDataRow);
                    // row 1
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Details"] = "1";
                    myDataRow["Integrated Tax"] = "2";
                    myDataRow["Central Tax"] = "3";
                    myDataRow["State Tax"] = "4";
                    myDataRow["Cess"] = "5";
                    mdt.Rows.Add(myDataRow);
                    //(A) ITC Available (Whether in full or part)
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Details"] = "(A) ITC Available (Whether in full or part)";
                    myDataRow["Integrated Tax"] = "";
                    myDataRow["Central Tax"] = "";
                    myDataRow["State Tax"] = "";
                    myDataRow["Cess"] = "";
                    mdt.Rows.Add(myDataRow);
                    //(1)   Import of goods 
                    mSQL2 = "Select isnull(Sum(x.iGSTAmt),0) as mIGST,isnull(Sum(x.cGSTAmt),0) as mCGST,isnull(Sum(x.sGSTAmt),0) as mSGST,0.00 as mCess from StockTax x,DocTypes d,ItemMaster i,Stock s Where i.Code=s.Code And x.TableKey=s.TableKey and ";
                    mSQL2 += (chkNOITC == true ? " x.GSTNoITC=0 and " : "") + " s.Type=d.Code and d.CurrConv='Y' And i.ItemType='I' and " + (chkRCM == false ? " x.GSTType=0 and " : "") + " (x.SubType='RP' or x.SubType='IM') And (x.IGSTAmt)<>0 " + mWhere;
                    mdata = GetDataTable(mSQL2, mconnstr);
                    mCol1 = 0;
                    mCol2 = 0;
                    mCol3 = 0;
                    mCol4 = 0;
                    if (mdata != null)
                    {
                        mCol1 = (decimal)mdata.Rows[0]["mIGST"];
                        mCol2 = (decimal)mdata.Rows[0]["mCGST"];
                        mCol3 = (decimal)mdata.Rows[0]["mSGST"];
                        mCol4 = (decimal)mdata.Rows[0]["mCess"];
                    }
                    mtCol1 = mCol1;
                    mtCol2 = mCol2;
                    mtCol3 = mCol3;
                    mtCol4 = mCol4;
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Details"] = "   (1) Import of goods";
                    myDataRow["Integrated Tax"] = mCol1;
                    myDataRow["Central Tax"] = mCol2;
                    myDataRow["State Tax"] = mCol3;
                    myDataRow["Cess"] = mCol4;
                    mdt.Rows.Add(myDataRow);
                    //(1)   Import of Services 
                    mSQL2 = "Select isnull(Sum(x.iGSTAmt),0) as mIGST,isnull(Sum(x.cGSTAmt),0) as mCGST,isnull(Sum(x.sGSTAmt),0) as mSGST,0.00 as mCess from StockTax x,DocTypes d,ItemMaster i,Stock s Where i.Code=s.Code And x.TableKey=s.TableKey and ";
                    mSQL2 += (chkNOITC == true ? " x.GSTNoITC=0 and " : "") + " s.Type=d.Code and d.CurrConv='Y' And i.ItemType='S' and " + (chkRCM == false ? " x.GSTType=0 and " : "") + " (x.SubType='RP' or x.SubType='IM') And (x.IGSTAmt)<>0 " + mWhere;
                    mdata = GetDataTable(mSQL2, mconnstr);
                    mCol1 = 0;
                    mCol2 = 0;
                    mCol3 = 0;
                    mCol4 = 0;
                    if (mdata != null)
                    {
                        mCol1 = (decimal)mdata.Rows[0]["mIGST"];
                        mCol2 = (decimal)mdata.Rows[0]["mCGST"];
                        mCol3 = (decimal)mdata.Rows[0]["mSGST"];
                        mCol4 = (decimal)mdata.Rows[0]["mCess"];
                    }
                    mtCol1 += mCol1;
                    mtCol2 += mCol2;
                    mtCol3 += mCol3;
                    mtCol4 += mCol4;
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Details"] = "   (2) Import of Services";
                    myDataRow["Integrated Tax"] = mCol1;
                    myDataRow["Central Tax"] = mCol2;
                    myDataRow["State Tax"] = mCol3;
                    myDataRow["Cess"] = mCol4;
                    mdt.Rows.Add(myDataRow);
                    //(3) Inward supplies liable to reverse charge (other than 1 & 2 above)
                    mSQL2 = "Select isnull(Sum(x.iGSTAmt),0) as mIGST,isnull(Sum(x.cGSTAmt),0) as mCGST,isnull(Sum(x.sGSTAmt),0) as mSGST,0.00 as mCess from StockTax x,ItemMaster i,Stock s Where i.Code=s.Code and x.TableKey=s.TableKey and ";
                    mSQL2 += (chkNOITC == true ? " x.GSTNoITC=0 and " : "") + " (x.GSTType=1 or x.GSTType=6) And (s.MainType='PM' or s.MainType='JV' or x.SubType='RP' or x.SubType='CP' or x.SubType='NP') " + mWhere;//And (x.IGSTAmt+x.CGSTAmt+x.SGSTAmt)<>0
                    mdata = GetDataTable(mSQL2, mconnstr);
                    mCol1 = 0;
                    mCol2 = 0;
                    mCol3 = 0;
                    mCol4 = 0;
                    if (mdata != null)
                    {
                        mCol1 = (decimal)mdata.Rows[0]["mIGST"];
                        mCol2 = (decimal)mdata.Rows[0]["mCGST"];
                        mCol3 = (decimal)mdata.Rows[0]["mSGST"];
                        mCol4 = (decimal)mdata.Rows[0]["mCess"];
                    }
                    mtCol1 += mCol1;
                    mtCol2 += mCol2;
                    mtCol3 += mCol3;
                    mtCol4 += mCol4;
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Details"] = "   (3) Inward supplies liable to reverse charge (other than 1 & 2 above)";
                    myDataRow["Integrated Tax"] = mCol1;
                    myDataRow["Central Tax"] = mCol2;
                    myDataRow["State Tax"] = mCol3;
                    myDataRow["Cess"] = mCol4;
                    mdt.Rows.Add(myDataRow);
                    //(4) Inward supplies from ISD
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Details"] = "   (4) Inward supplies from ISD";
                    myDataRow["Integrated Tax"] = "0";
                    myDataRow["Central Tax"] = "0";
                    myDataRow["State Tax"] = "0";
                    myDataRow["Cess"] = "0";
                    mdt.Rows.Add(myDataRow);
                    //(4) All Other ITC
                    //mSQL2 = "Select isnull(Sum(x.iGSTAmt),0) as mIGST,isnull(Sum(x.cGSTAmt),0) as mCGST,isnull(Sum(x.sGSTAmt),0) as mSGST,0.00 as mCess from StockTax x,DocTypes d,ItemMaster i,Stock s Where i.Code=s.Code And x.TableKey=s.TableKey and ";
                    //mSQL2 += (chkNOITC == true ? " x.GSTNoITC=0 and " : "") + " s.Type=d.Code And d.CurrConv<>'Y' And i.ItemType='I' and x.GSTType=0 And (((x.SubType='XS' or x.SubType='RS' or x.SubType='US') And x.GSTType=9) or s.MainType='PM' or s.MainType='JV' or x.SubType='RP' or x.SubType='CP'" + (chkReturnS == true ? " or x.SubType='NS'" : "") + (chkReturnP == false ? " or x.SubType='NP'" : "") + ") and (x.IGSTAmt+x.CGSTAmt+x.SGSTAmt)<>0" + mWhere;
                    mSQL2 = "Select isnull(Sum(x.iGSTAmt),0) as mIGST,isnull(Sum(x.cGSTAmt),0) as mCGST,isnull(Sum(x.sGSTAmt),0) as mSGST,0.00 as mCess from StockTax x,DocTypes d,Stock s Where x.TableKey=s.TableKey and ";
                    mSQL2 += "x.GSTNoITC=0 and s.Type=d.Code And d.CurrConv<>'Y' and x.GSTType=0 And (((x.SubType='XS' or x.SubType='RS' or x.SubType='US') And x.GSTType=9) or s.MainType='PM' or s.MainType='JV' or x.SubType='RP' or x.SubType='CP'" + (chkReturnS == true ? " or x.SubType='NS'" : "") + (chkReturnP == false ? " or x.SubType='NP'" : "") + ") and (x.IGSTAmt+x.CGSTAmt+x.SGSTAmt)<>0" + mWhere;
                    //mSQL2 += (chkNOITC == true ? " x.GSTNoITC=0 and " : "") + " s.Type=d.Code And d.CurrConv<>'Y' and x.GSTType=0 And (((x.SubType='XS' or x.SubType='RS' or x.SubType='US') And x.GSTType=9) or s.MainType='PM' or s.MainType='JV' or x.SubType='RP' or x.SubType='CP'" + (chkReturnS == true ? " or x.SubType='NS'" : "") + (chkReturnP == false ? " or x.SubType='NP'" : "") + ") and (x.IGSTAmt+x.CGSTAmt+x.SGSTAmt)<>0" + mWhere;
                    mdata = GetDataTable(mSQL2, mconnstr);
                    mCol1 = 0;
                    mCol2 = 0;
                    mCol3 = 0;
                    mCol4 = 0;
                    if (mdata != null)
                    {
                        mCol1 = (decimal)mdata.Rows[0]["mIGST"];
                        mCol2 = (decimal)mdata.Rows[0]["mCGST"];
                        mCol3 = (decimal)mdata.Rows[0]["mSGST"];
                        mCol4 = (decimal)mdata.Rows[0]["mCess"];
                    }
                    mSQL2 = RecToString("Select Distinct iGSTIn from HSNMaster where iGSTIn<>''", "^");
                    mCol1 += FieldoftableNumber("Ledger", "isnull(Sum(Debit-Credit),0)", "(SubType='DN') and CharIndex(Code,'" + mSQL2 + "')<>0 and Branch='" + mbranchcode + "' and DocDate Between '" + MMDDYY(mDate1) + "' and '" + MMDDYY(mDate2) + "'");
                    mSQL2 = RecToString("Select Distinct cGSTIn from HSNMaster where cGSTIn<>''", "^");
                    mCol2 += FieldoftableNumber("Ledger", "isnull(Sum(Debit-Credit),0)", "(SubType='DN') And CharIndex(Code,'" + mSQL2 + "')<>0 and Branch ='" + mbranchcode + "' and DocDate Between '" + MMDDYY(mDate1) + "' and '" + MMDDYY(mDate2) + "'");
                    mSQL2 = RecToString("Select Distinct SGSTIn from HSNMaster where SGSTIn<>''", "^");
                    mCol3 += FieldoftableNumber("Ledger", "isnull(Sum(Debit-Credit),0)", "(SubType='DN') And CharIndex(Code,'" + mSQL2 + "')<>0  And Branch ='" + mbranchcode + "' And DocDate Between '" + MMDDYY(mDate1) + "' and '" + MMDDYY(mDate2) + "'");
                    mtCol1 += mCol1;
                    mtCol2 += mCol2;
                    mtCol3 += mCol3;
                    mtCol4 += mCol4;
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Details"] = "   (5) All Other ITC";
                    myDataRow["Integrated Tax"] = mCol1;
                    myDataRow["Central Tax"] = mCol2;
                    myDataRow["State Tax"] = mCol3;
                    myDataRow["Cess"] = mCol4;
                    mdt.Rows.Add(myDataRow);
                    //(B)ITC Reversed
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Details"] = "(B) ITC Reversed";
                    myDataRow["Integrated Tax"] = "";
                    myDataRow["Central Tax"] = "";
                    myDataRow["State Tax"] = "";
                    myDataRow["Cess"] = "";
                    mdt.Rows.Add(myDataRow);
                    //(1) As per Rule 42 & 43 of SGST/CGST rules 
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Details"] = "   (1) As per Rule 42 & 43 of SGST/CGST rules ";
                    myDataRow["Integrated Tax"] = "";
                    myDataRow["Central Tax"] = "";
                    myDataRow["State Tax"] = "";
                    myDataRow["Cess"] = "";
                    mdt.Rows.Add(myDataRow);
                    //(2) Others
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Details"] = "   (2) Others";
                    myDataRow["Integrated Tax"] = "";
                    myDataRow["Central Tax"] = "";
                    myDataRow["State Tax"] = "";
                    myDataRow["Cess"] = "";
                    mdt.Rows.Add(myDataRow);
                    //(C) Net ITC Available (A)-(B)
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Details"] = "(C) Net ITC Available (A)-(B)";
                    myDataRow["Integrated Tax"] = mtCol1;
                    myDataRow["Central Tax"] = mtCol2;
                    myDataRow["State Tax"] = mtCol3;
                    myDataRow["Cess"] = mtCol4;
                    mdt.Rows.Add(myDataRow);
                    //(D) Ineligible ITC
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Details"] = "(D) Ineligible ITC";
                    myDataRow["Integrated Tax"] = "";
                    myDataRow["Central Tax"] = "";
                    myDataRow["State Tax"] = "";
                    myDataRow["Cess"] = "";
                    mdt.Rows.Add(myDataRow);
                    //(1) As per section 17(5) of CGST//SGST Act
                    //i.ItemType='I' and 
                    mSQL2 = "Select isnull(Sum(x.iGSTAmt),0) as mIGST,isnull(Sum(x.cGSTAmt),0) as mCGST,isnull(Sum(x.sGSTAmt),0) as mSGST,0.00 as mCess from StockTax x, Stock s,ItemMaster i Where i.Code=s.Code and s.TableKey=x.TableKey and ";
                    mSQL2 += "x.GSTNoITC<>0 and (s.MainType='PM' or s.MainType='JV' or x.SubType='RP' or x.SubType='CP') and (x.IGSTAmt+x.CGSTAmt+x.SGSTAmt)<>0" + mWhere;
                    mdata = GetDataTable(mSQL2, mconnstr);
                    mCol1 = 0;
                    mCol2 = 0;
                    mCol3 = 0;
                    mCol4 = 0;
                    if (mdata != null)
                    {
                        mCol1 = (decimal)mdata.Rows[0]["mIGST"];
                        mCol2 = (decimal)mdata.Rows[0]["mCGST"];
                        mCol3 = (decimal)mdata.Rows[0]["mSGST"];
                        mCol4 = (decimal)mdata.Rows[0]["mCess"];
                    }
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Details"] = "   (1) As per section 17(5) of CGST//SGST Act";
                    myDataRow["Integrated Tax"] = mCol1;
                    myDataRow["Central Tax"] = mCol2;
                    myDataRow["State Tax"] = mCol3;
                    myDataRow["Cess"] = mCol4;
                    mdt.Rows.Add(myDataRow);
                    //(2) Others
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Details"] = "   (2) Others";
                    myDataRow["Integrated Tax"] = "";
                    myDataRow["Central Tax"] = "";
                    myDataRow["State Tax"] = "";
                    myDataRow["Cess"] = "";
                    mdt.Rows.Add(myDataRow);
                    break;
                case "exempted":
                    // 5. Values of exempt, Nil-rated and non-GST inward supplies
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "Nature of Supplies";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "Inter-State Supplies";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "Intra-State Supplies";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    mcnt = 1;
                    //Table header
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Nature of Supplies"] = "5. Values of exempt, Nil-rated and non-GST inward supplies";
                    myDataRow["Inter-State Supplies"] = "";
                    myDataRow["Intra-State Supplies"] = "";
                    mdt.Rows.Add(myDataRow);
                    //// row 1
                    //myDataRow = mdt.NewRow();
                    //myDataRow["RecordKey"] = mcnt++;
                    //myDataRow["Nature of Supplies"] = "Nature of Supplies";
                    //myDataRow["Inter-State Supplies"] = "Inter-State Supplies";
                    //myDataRow["Intra-State Supplies"] = "Intra-State Supplies";
                    //mdt.Rows.Add(myDataRow);
                    // row 1
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Nature of Supplies"] = "1";
                    myDataRow["Inter-State Supplies"] = "2";
                    myDataRow["Intra-State Supplies"] = "3";
                    mdt.Rows.Add(myDataRow);
                    //
                    //From a supplier under composition scheme, Exempt  and Nil rated supply
                    mSQL2 = "Select isnull(Sum(Case When a.State<>'" + mCurrState + "' then x.Taxable else 0 End),0) as mIGST,isnull(Sum(Case When a.State='" + mCurrState + "' then x.Taxable else 0 End),0) as mSGST from StockTax x,Address a,Stock s,ItemMaster i where i.Code=s.Code And x.TableKey=s.TableKey and ";
                    mSQL2 += "a.Code=s.Party and a.Sno=x.AltAddress and " + (chkNOITC == true ? " s.GSTNoITC=0 and " : "") + " i.ItemType='I' and (x.GSTType=12 or x.GSTType=10) and (s.MainType='PM' or s.MainType='JV' or x.SubType='RP' or x.SubType='CP' or x.SubType='NP') " + mWhere;
                    mdata = GetDataTable(mSQL2, mconnstr);
                    mCol1 = 0;
                    mCol2 = 0;
                    if (mdata != null)
                    {
                        mCol1 = (decimal)mdata.Rows[0]["mIGST"];
                        mCol2 = (decimal)mdata.Rows[0]["mSGST"];
                    }
                    mdata.Dispose();
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Nature of Supplies"] = "From a supplier under composition scheme, Exempt  and Nil rated supply";
                    myDataRow["Inter-State Supplies"] = mCol1;
                    myDataRow["Intra-State Supplies"] = mCol2;
                    mtCol1 = mCol1;
                    mtCol2 = mCol2;
                    mdt.Rows.Add(myDataRow);
                    //Non GST supply	
                    mSQL2 = "Select isnull(Sum(Case When a.State<>'" + mCurrState + "' then x.Taxable else 0 End),0) as mIGST,isnull(Sum(Case When a.State='" + mCurrState + "' then x.Taxable else 0 End),0) as mSGST from StockTax x,ItemMaster i,Stock s, Address a where s.TableKey=x.TableKey and i.Code=s.Code and ";
                    mSQL2 += "a.Code=s.Party And a.Sno=x.AltAddress And " + (chkNOITC == true ? " s.GSTNoITC=0 and " : "") + " i.ItemType='I' And x.GSTType=11 And (s.MainType='PM' or s.MainType='JV' or x.SubType='RP' or x.SubType='CP' or x.SubType='NP') " + mWhere;
                    mdata = GetDataTable(mSQL2, mconnstr);
                    mCol1 = 0;
                    mCol2 = 0;
                    if (mdata != null)
                    {
                        mCol1 = (decimal)mdata.Rows[0]["mIGST"];
                        mCol2 = (decimal)mdata.Rows[0]["mSGST"];
                    }
                    mdata.Dispose();
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Nature of Supplies"] = "Non GST supply";
                    myDataRow["Inter-State Supplies"] = mCol1;
                    myDataRow["Intra-State Supplies"] = mCol2;
                    mtCol1 += mCol1;
                    mtCol2 += mCol2;
                    mdt.Rows.Add(myDataRow);
                    //Total
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Nature of Supplies"] = "Total";
                    myDataRow["Inter-State Supplies"] = mtCol1;
                    myDataRow["Intra-State Supplies"] = mtCol2;
                    mdt.Rows.Add(myDataRow);
                    break;
                //case "payment":
                //    break;
            }

            if (Model.mWhat == "XLS")
            {
                string attachment = "attachment; filename=" + Model.ViewDataId + @".xls";
                Response.ClearContent();
                Response.AddHeader("content-disposition", attachment);
                Response.ContentType = "application/vnd.ms-excel";
                string tab = "";
                foreach (DataColumn dc in mdt.Columns)
                {
                    Response.Write(tab + dc.ColumnName);
                    tab = "\t";
                }
                Response.Write("\n");
                int i;
                foreach (DataRow dr in mdt.Rows)
                {
                    tab = "";
                    for (i = 0; i < mdt.Columns.Count; i++)
                    {
                        Response.Write(tab + dr[i].ToString());
                        tab = "\t";
                    }
                    Response.Write("\n");
                }
                Response.End();
                return null;
            }
            else if (Model.mWhat == "PDF")
            {
                GridView GridView1 = new GridView();
                GridView1.AllowPaging = false;
                GridView1.DataSource = mdt;
                GridView1.DataBind();
                Response.ContentType = "application /pdf";
                Response.AddHeader("content-disposition", "attachment;filename=" + Model.ViewDataId + @".pdf");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                StringWriter sw = new StringWriter();
                HtmlTextWriter hw = new HtmlTextWriter(sw);
                GridView1.RenderControl(hw);
                StringReader sr = new StringReader(sw.ToString());
                Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                pdfDoc.SetPageSize(PageSize.LETTER.Rotate());
                HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
                pdfDoc.Open();
                htmlparser.Parse(sr);
                pdfDoc.Close();
                Response.Write(pdfDoc);
                Response.End();
                return null;
                // use in place of htmlworker since its obsolete
                //using (StringWriter sw = new StringWriter())
                //{
                //    using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                //    {
                //        GridView1.RenderControl(hw);
                //        StringReader sr = new StringReader(sw.ToString());
                //        Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                //        PdfWriter writer = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
                //        pdfDoc.Open();
                //        XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                //        pdfDoc.Close();
                //        Response.ContentType = "application/pdf";
                //        Response.AddHeader("content-disposition", "attachment;filename=GridViewExport.pdf");
                //        Response.Cache.SetCacheability(HttpCacheability.NoCache);
                //        Response.Write(pdfDoc);
                //        Response.End();
                //    }
                //}
            }
            return Content(JQGridHelper.JsonForJqgrid(mdt, Model.rows, mdt.Rows.Count, Model.page), "application/json");
        }

        public ActionResult GetExcelGSTR(GridOption Model)
        {
            Model.mWhat = "XLS";
            return GetGridData(Model);
        }

        public ActionResult GetPDFGSTR(GridOption Model)
        {
            Model.mWhat = "PDF";
            return GetGridData(Model);
        }

    }
}