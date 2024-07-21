using ALT_ERP3.Controllers;
using Common;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class GSTR1Controller : BaseController
    {
        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            UpdateAuditTrail(mbranchcode, "Report", Model.Header, "", DateTime.Now, 0, "", "", "NA");
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
            return GetGridDataColumns(Model.ViewDataId.ToUpper(), "X", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            string mcode = Model.ViewDataId.Substring(6);
            string mSQL;
            string mSQL2;
            string msumstring;
            string morderby = "l.Type,lr.Srl";
            string mfrom = "";
            //chkReturnP = Consider Purchase Return in CDNR
            //chkReturnPR = Don't Consider Purchase Return in HSN Summary
            //chkShowPurRet = Consider Purchase Return in B2B
            //chkReturnSR = Don't Consider Sales Return in GSTR1
            //chkDontSRinCS = Don't Consider Sales Return in B2CS
            //chkPRRetDoc = Don't Consider Purchase Return in Docs
            //chkSEZinExp = Consider SEZ without Payment In Export 
            //chkSRRetDoc = Don't Consider Sales Return in DOCs

            bool chkShowPurRet = Model.SelectContent.ToString().Substring(0, 1) == "1" ? true : false;
            bool chkReturnSR = Model.SelectContent.ToString().Substring(1, 1) == "1" ? true : false;
            bool chkReturnPR = Model.SelectContent.ToString().Substring(2, 1) == "1" ? true : false;
            bool chkDontSRinCS = Model.SelectContent.ToString().Substring(3, 1) == "1" ? true : false;
            bool chkReturnP = Model.SelectContent.ToString().Substring(4, 1) == "1" ? true : false;
            bool chkPRRetDoc = Model.SelectContent.ToString().Substring(5, 1) == "1" ? true : false;
            bool chkSEZinExp = Model.SelectContent.ToString().Substring(6, 1) == "1" ? true : false;
            bool chkSRRetDoc = Model.SelectContent.ToString().Substring(7, 1) == "1" ? true : false;
            bool chkPrefixType = Model.SelectContent.ToString().Substring(8, 1) == "1" ? true : false;

            string mCurrState = "";
            var date = Model.Date.Replace("-", "/").Split(':');
            Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            DataTable mdt = new DataTable("gstr1docs");
            DateTime mDate1 = Convert.ToDateTime(Model.FromDate);
            DateTime mDate2 = Convert.ToDateTime(Model.ToDate);

            // get select fields from tfatsearch
            //string connstring = string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings[""].ConnectionString) == true ? "" : ConfigurationManager.ConnectionStrings[""].ConnectionString;
            SqlConnection con = new SqlConnection(GetConnectionString());
            msumstring = "Select" + GetSumString(con, Model.ViewDataId, Model.FromDate, Model.ToDate);

            SqlCommand cmd = new SqlCommand("Select dbo.GetGridQuery(@mFormatCode,@mAlias,@mCurrDec,@mBranch,@mUserID,@mReportStartDate,@mReportEndDate,@mReturn)", con);
            //SqlParameter code1 = new SqlParameter("@code", SqlDbType.Int);
            cmd.Parameters.Add("@mFormatCode", SqlDbType.VarChar).Value = Model.ViewDataId.ToUpper();
            cmd.Parameters.Add("@mAlias", SqlDbType.VarChar).Value = "";
            cmd.Parameters.Add("@mCurrDec", SqlDbType.TinyInt).Value = 2;
            cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
            cmd.Parameters.Add("@mUserID", SqlDbType.VarChar).Value = muserid;
            cmd.Parameters.Add("@mReportStartDate", SqlDbType.VarChar).Value = Model.FromDate;
            cmd.Parameters.Add("@mReportEndDate", SqlDbType.VarChar).Value = Model.ToDate;
            cmd.Parameters.Add("@mReturn", SqlDbType.VarChar).Value = "B";
            con.Open();
            mSQL = "Select " + cmd.ExecuteScalar().ToString();
            cmd.Dispose();

            if (mcode != "b2b" && mcode != "AT" && mcode != "atadj" && mcode != "hsn")
            {
                //mfrom += " from StockTax s";
                //mfrom += " left join Stock t on s.tablekey = t.tablekey";
                //mfrom += " left join Sales l on s.ParentKey = l.TableKey";
                mfrom += " from Sales l";
                mfrom += " left join Ledger lr on l.TableKey = lr.Parentkey and lr.sno=1 ";
                mfrom += " left join SalesMore r on l.TableKey = r.TableKey";
                mfrom += " left join Purchase p on p.TableKey = l.TableKey";
                mfrom += " Left Join CAddress a on l.Code = a.Code And l.AltAddress = a.Sno";
                mfrom += " Left Join TfatState x on x.Name = l.PlaceOfSupply ";
                //mSQL += " StockTax s left join Sales l on s.ParentKey=l.TableKey left join Purchase p on p.ParentKey=l.TableKey Left Join Address a on t.Party=a.Code And t.AltAddress=a.Sno";
                mfrom += " where " + (string.IsNullOrEmpty(mDocString) ? "" : "CharIndex(l.Type,'" + mDocString + "')=0 And ");
                msumstring += mfrom;
                mSQL += mfrom;
                //+ Replace(gsDocString, "Ledger.", "s.") + "s.ItemType='I' "
                //If gpWareHouse And strLocation <> "" Then mSQL += " And S.Locationcode In (" + strLocation + ")"
            }
            switch (mcode)
            {
                case "b2b":
                    // Details of invoices of Taxable Supplies made to other Registered Taxpayers
                    mSQL = "With mExecQuery as (";
                    mSQL += "Select(Case when l.SubType <> 'CS' then a.GSTNo else (Select m.GSTNo from CAddress m where m.Code = r.RefParty And m.Sno = 0) End) as [GSTIN / UIN of Recipient], ";
                    mSQL += "l.Branch+l.Tablekey as [Document], ";
                    mSQL += "a.Name as [Receiver Name], ";
                    mSQL += (chkPrefixType == true ? "l.Type+'-'+" : "") + "lr.Srl as [Invoice Number], ";
                    mSQL += "Replace(Convert(nVARCHAR, l.DocDate, 106), ' ', '-') as [Invoice date], ";
                    mSQL += "Cast(l.Amt as Decimal(15, 2)) as [Invoice Value], ";
                    //mSQL += "a.State as [Place Of Supply], ";
                    mSQL += "x.StateCode+'-'+l.PlaceOfSupply as [Place Of Supply], ";
                    mSQL += "(Case When l.GSTType = 1 or l.GSTType = 6 then 'Y' Else 'N' End) as [Reverse Charge], ";
                    mSQL += "0 as [Applicalble %of TaxRate], ";   // abatement from taxmaster goes here
                    mSQL += "(Case l.GSTType When 7 then 'SEZ supplies with payment' When 8 then 'SEZ supplies without payment' When 5 then 'Deemed Exp' Else 'Regular' End) as [Invoice Type], ";
                    mSQL += "'' as [E-Commerce GSTIN], ";
                    mSQL += "Cast(lr.IGSTRate + lr.cGSTRate + lr.sGSTRate as Decimal(15, 2)) as [Rate], ";
                    mSQL += "Cast(l.Taxable*-1 as Decimal(15, 2)) as [Taxable Value], ";
                    mSQL += "Cast(0 as Decimal(15, 2)) as [Cess Amount], ";
                    mSQL += "Cast(abs(lr.IGSTAmt + lr.cGSTAmt + lr.sGSTAmt) as Decimal(15, 2)) as [GST Amount] ";
                    mSQL += "from Sales l ";
                    //mSQL += "Left join Stock t on s.tablekey = t.tablekey ";
                    mSQL += "Left join Ledger lr on l.TableKey = lr.TableKey ";
                    mSQL += "Left join SalesMore r on l.TableKey = r.TableKey ";
                    mSQL += "Left join Purchase p on p.TableKey = l.TableKey ";
                    mSQL += "Left Join CAddress a on l.Code = a.Code And l.AltAddress = a.Sno ";
                    mSQL += "Left Join TfatState x on x.Name = l.PlaceOfSupply ";
                    mSQL += "Where (l.SubType = 'JS' or l.SubType = 'RS' or l.SubType = 'CS' ";
                    mSQL += (chkShowPurRet == true ? " or l.SubType='NP'" : "");
                    mSQL += ") And (Case when l.SubType<>'CS' then a.GSTNo else (Select Top 1 m.GSTNo from CAddress m where m.Code=r.RefParty And m.Sno=0) End)<>''  And (l.GSTType=0 or l.GSTType=7 or l.GSTType=8 or l.GSTType=13 or l.GSTType=15) And ((l.maintype='SL' And (l.REASONCODE=0 or l.REASONCODE is null)) OR (l.MAINTYPE='PR' AND P.REASONCODE<>0)) ";
                    //morderby = "(Case when t.SubType<>'CS' then a.GSTNo else (Select m.GSTNo from Address m where m.Code=r.RefParty And m.Sno=0) End),t.Type,t.Srl ";
                    mSQL += " And l.Branch in ('" + mbranchcode + "') " + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(l.Type,'" + mDocString + "')=0");
                    mSQL += " And l.DocDate Between '" + MMDDYY(Convert.ToDateTime(Model.FromDate)) + "' And '" + MMDDYY(Convert.ToDateTime(Model.ToDate)) + "' ";
                    mSQL += ")";
                    mSQL += "Select Document,[GSTIN / UIN of Recipient], [Receiver Name], [Invoice Number], [Invoice date], [Invoice Value], [Place Of Supply], [Reverse Charge],[Applicalble %of TaxRate], [Invoice Type], [E-Commerce GSTIN], [Rate], ";
                    mSQL += "Sum([Taxable Value]) as [Taxable Value],Sum([Cess Amount]) as [Cess Amount],Sum([GST Amount]) as [GST Amount] from mExecQuery ";
                    mSQL += "Group By Document,[GSTIN / UIN of Recipient], [Receiver Name], [Invoice Number], [Invoice date], [Invoice Value], [Place Of Supply], [Reverse Charge], [Applicalble %of TaxRate],[Invoice Type], [E-Commerce GSTIN], [Rate] ";
                    mSQL += "Order by [Receiver Name],[Invoice date],[Invoice Number]";
                    morderby = "";
                    break;
                case "b2cl":
                    // Invoices for Taxable Supplies to Consumers where a) The Place of Supply is outside State b) The Invoice value is > Rs 2,50,000
                    //mSQL += " (s.SubType='XS' or s.SubType='JS' or s.SubType='RS' or s.SubType='CS') And (Case when s.SubType<>'CS' then a.GSTNo when s.SubType='CS' And r.refparty<>'' then (Select Top 1 m.GSTNo from Address m where m.Code=r.RefParty And m.Sno=0) Else '' End)='' And s.iGSTAmt<>0 And l.Amt>250000 And (l.ReasonCode=0 or l.ReasonCode is null)";
                    //morderby = "t.DocDate,t.Srl";

                    mSQL += " (l.SubType='XS' or l.SubType='JS' or l.SubType='RS' or l.SubType='CS') And (Case when l.SubType<>'CS' then a.GSTNo when l.SubType='CS' And r.refparty<>'' then (Select Top 1 m.GSTNo from CAddress m where m.Code=r.RefParty And m.Sno=0) Else '' End)='' And lr.iGSTAmt<>0 And l.Amt>250000 And (l.ReasonCode=0 or l.ReasonCode is null)";
                    morderby = "l.DocDate,lr.Srl";
                    break;
                case "b2cs":
                    //mSQL += " (s.SubType='JS' or s.SubType='XS' or s.SubType='RS' or s.SubType='CS' or s.SubType='US' ";
                    //mSQL += (chkReturnSR == true || chkDontSRinCS == true ? "" : " or s.SubType='NS'");
                    //mSQL += ") And ((s.DealerType=5) or (Case when s.SubType<>'CS' and s.SubType<>'US' then a.GSTNo when (s.SubType='CS' or s.SubType='US') And r.refparty<>'' then (Select Top 1 m.GSTNo from Address m where m.Code=r.RefParty And m.Sno=0) Else '' End)='') And (upper(a.State)='" + mCurrState + "' or (upper(a.State)<>'" + mCurrState + "' And (abs(l.Amt)<=250000)" + (chkReturnSR == false ? "" : " or s.SubType='NS'") + ")) And Not (s.GSTType=5 or s.GSTType=7 or s.GSTType=8 or s.GSTType=9 or s.GSTType=13)";

                    mSQL += " (l.SubType='JS' or l.SubType='XS' or l.SubType='RS' or l.SubType='CS' or l.SubType='US' ";
                    mSQL += (chkReturnSR == true || chkDontSRinCS == true ? "" : " or l.SubType='NS'");
                    mSQL += ") And ((a.DealerType=5) or (Case when l.SubType<>'CS' and l.SubType<>'US' then a.GSTNo when (l.SubType='CS' or l.SubType='US') And r.refparty<>'' then (Select Top 1 m.GSTNo from CAddress m where m.Code=r.RefParty And m.Sno=0) Else '' End)='') And (upper(a.State)='" + mCurrState + "' or (upper(a.State)<>'" + mCurrState + "' And (abs(l.Amt)<=250000)" + (chkReturnSR == false ? "" : " or l.SubType='NS'") + ")) And Not (l.GSTType=5 or l.GSTType=7 or l.GSTType=8 or l.GSTType=9 or l.GSTType=13)";



                    morderby = "";// "t.DocDate,t.Srl";
                    break;
                case "cdnr":
                    //mSQL += " ((s.SubType='RS' And l.ReasonCode<>0 And l.ReasonCode<>8) " + (chkReturnSR == true ? "" : " or s.SubType='NS'") + " or s.SubType='AP' " + (chkReturnP == true ? "or s.SubType='NP'" : "") + ") And a.GSTNo<>''";
                    //morderby = "t.DocDate,t.Srl";

                    mSQL += " ((l.SubType='RS' And l.ReasonCode<>0 And l.ReasonCode<>8) " + (chkReturnSR == true ? "" : " or l.SubType='NS'") + " or l.SubType='AP' " + (chkReturnP == true ? "or l.SubType='NP'" : "") + ") And a.GSTNo<>''";
                    morderby = "l.DocDate,lr.Srl";

                    break;
                case "cdnur":
                    //mSQL += " ((s.SubType='RS' And l.ReasonCode<>0 And l.ReasonCode<>8) " + (chkReturnSR == true ? "" : " or s.SubType='NS'") + " or s.SubType='AP' " + (chkReturnP == true ? "or s.SubType='NP'" : "") + ") And (a.GSTNo='' or a.DealerType=1) And (s.cGSTAmt+s.sGSTAmt<>0 or (s.iGSTAmt<>0 and L.Amt>250000)) ";
                    //morderby = "t.DocDate,t.Srl";

                    mSQL += " ((l.SubType='RS' And l.ReasonCode<>0 And l.ReasonCode<>8) " + (chkReturnSR == true ? "" : " or l.SubType='NS'") + " or l.SubType='AP' " + (chkReturnP == true ? "or l.SubType='NP'" : "") + ") And (a.GSTNo='' or a.DealerType=1) And (lr.cGSTAmt+lr.sGSTAmt<>0 or (lr.iGSTAmt<>0 and L.Amt>250000)) ";
                    morderby = "l.DocDate,lr.Srl";
                    break;
                case "exp":
                    //mSQL += " (s.SubType='RS' or s.SubType='JS' or s.SubType='CS' or s.SubType='XS') And (s.GSTType=5 or s.GSTType=9 or s.GSTType=13)";
                    //morderby = "t.DocDate,t.Srl";

                    mSQL += " (l.SubType='RS' or l.SubType='JS' or l.SubType='CS' or l.SubType='XS') And (l.GSTType=5 or l.GSTType=9 or l.GSTType=13)";
                    morderby = "l.DocDate,lr.Srl";
                    break;
                case "at":
                    mSQL2 = "Select t.DocDate,s.CessAmt,s.IGSTAmt,s.CGSTAmt,s.SGSTAmt,s.iGSTRate,s.cGSTRate,s.sGSTRate,.Party,s.Branch,s.AltAddress,s.Taxable-(Select isnull(Sum(((o.Amount*100)/e.Amt)*e.Taxable/100),0) from Outstanding o,Sales e where ltrim(rtrim(o.flag))='' And o.DocDate Between '" + MMDDYY(mDate1) + "' And '" + MMDDYY(mDate2) + "' And o.Branch=s.Branch and o.aType=s.Type and o.aPrefix=s.Prefix and o.aSerial=s.Srl and o.aSno=s.Sno And o.Branch=e.Branch and o.Type=e.Type and o.Prefix=e.Prefix and o.Srl=e.Srl) as Amt into xTemp ";
                    mSQL2 = mSQL2 + " from StockTax s,stock t Where s.tablekey=t.tablekey and s.SubType='AR' And s.Branch in ('" + mbranchcode + "')";
                    mSQL2 = mSQL2 + " And t.DocDate Between '" + MMDDYY(mDate1) + "' And '" + MMDDYY(mDate2) + "'";
                    //If gpWareHouse And strLocation <> "" Then mSQL2 = mSQL2 + " And s.Locationcode In (" + strLocation + ")"
                    //gsdbDbf.Execute mSQL2
                    mSQL += " from xTemp s, Address a Where s.Party=a.Code And s.AltAddress=a.Sno ";
                    morderby = "";
                    break;
                case "atadj":
                    mSQL2 = "Select t.DocDate,s.CessAmt,s.IGSTAmt,s.CGSTAmt,s.SGSTAmt,s.iGSTRate,s.cGSTRate,s.sGSTRate,s.Party,s.Branch,s.AltAddress,s.Taxable,(Select isnull(Sum(((o.Amount*100)/e.Amt)*e.Taxable/100),0) from Outstanding o,Sales e where ltrim(rtrim(o.flag))='' And o.DocDate Between '" + MMDDYY(mDate1) + "' And '" + MMDDYY(mDate2) + "' And o.Branch=s.Branch and o.aType=s.Type and o.aPrefix=s.Prefix and o.aSerial=s.Srl and o.aSno=s.Sno And o.Branch=e.Branch and o.Type=e.Type and o.Prefix=e.Prefix and o.Srl=e.Srl) as Amt into xTemp ";
                    mSQL2 = mSQL2 + " from StockTax s,stock t Where s.tablekey=t.tablekey and s.SubType='AR' And s.Branch in ('" + mbranchcode + "')";
                    mSQL2 = mSQL2 + " And t.DocDate<'" + MMDDYY(mDate1) + "'";
                    //If gpWareHouse And strLocation <> "" Then mSQL2 = mSQL2 + " And s.Locationcode In (" + strLocation + ")"
                    //DelTbl gsdbDbf, "xTemp"
                    //gsdbDbf.Execute mSQL2
                    mSQL += " from xTemp s, Address a Where s.Party=a.Code And s.AltAddress=a.Sno And s.Amt<>0";
                    morderby = "";
                    break;
                case "exemp":
                    //' Details of Nil Rated, Exempted and Non GST Supplies made during the tax period
                    //.FixedRows = 1
                    //.Rows = 2
                    //mStr = Replace(gsDocString, "Ledger.", "s.") + " (s.GSTType=10 or s.GSTType=11 or s.GSTType=14) And a.Code=s.Code And a.Branch=s.Branch And a.Sno=s.AltAddress And (SubType='JS' or SubType='RS' or SubType='CS') And a.DealerType=0 And upper(a.State)<>'" + mCurrState + "' And s.Branch='" + gsCurBranch + "' And DocDate Between " + MmDdYy(mDate1) + " And " + MmDdYy(mDate2)
                    //If gpWareHouse And strLocation <> "" Then mStr = mStr + " And s.Locationcode In (" + strLocation + ")"
                    //.AddItem "Inter-State supplies to registered persons" + vbTab + fieldoftable("Sales s,Address a", "Sum(s.Amt)", "(s.GSTType<>10 and s.GSTType<>11) And " + mStr, "N") + vbTab + fieldoftable("Sales s,Address a", "Sum(s.Amt)", "(s.GSTType=10) And " + mStr, "N") + vbTab + fieldoftable("Sales s,Address a", "Sum(s.Amt)", "(s.GSTType=11) And " + mStr, "N")
                    //mStr = Replace(gsDocString, "Ledger.", "s.") + " (s.GSTType=10 or s.GSTType=11 or s.GSTType=14) And a.Code=s.Code And a.Branch=s.Branch And a.Sno=s.AltAddress And (SubType='JS' or SubType='RS' or SubType='CS') And a.DealerType=0 And upper(a.State)='" + mCurrState + "' And s.Branch='" + gsCurBranch + "' And DocDate Between " + MmDdYy(mDate1) + " And " + MmDdYy(mDate2)
                    //If gpWareHouse And strLocation <> "" Then mStr = mStr + " And s.Locationcode In (" + strLocation + ")"
                    //.AddItem "Intra-State supplies to registered persons" + vbTab + fieldoftable("Sales s,Address a", "Sum(s.Amt)", "(s.GSTType=14) And " + mStr, "N") + vbTab + fieldoftable("Sales s,Address a", "Sum(s.Amt)", "(s.GSTType=10) And " + mStr, "N") + vbTab + fieldoftable("Sales s,Address a", "Sum(s.Amt)", "(s.GSTType=11) And " + mStr, "N")
                    //mStr = Replace(gsDocString, "Ledger.", "s.") + " (s.GSTType=10 or s.GSTType=11 or s.GSTType=14) And a.Code=s.Code And a.Branch=s.Branch And a.Sno=s.AltAddress And (SubType='JS' or SubType='RS' or SubType='CS') And a.DealerType=1 And upper(a.State)<>'" + mCurrState + "' And s.Branch='" + gsCurBranch + "' And DocDate Between " + MmDdYy(mDate1) + " And " + MmDdYy(mDate2)
                    //If gpWareHouse And strLocation <> "" Then mStr = mStr + " And s.Locationcode In (" + strLocation + ")"
                    //.AddItem "Inter-State supplies to unregistered persons" + vbTab + fieldoftable("Sales s,Address a", "Sum(s.Amt)", "(s.GSTType=14) And " + mStr, "N") + vbTab + fieldoftable("Sales s,Address a", "Sum(s.Amt)", "(s.GSTType=10) And " + mStr, "N") + vbTab + fieldoftable("Sales s,Address a", "Sum(s.Amt)", "(s.GSTType=11) And " + mStr, "N")
                    //mStr = Replace(gsDocString, "Ledger.", "s.") + " (s.GSTType=10 or s.GSTType=11 or s.GSTType=14) And a.Code=s.Code And a.Branch=s.Branch And a.Sno=s.AltAddress And (SubType='JS' or SubType='RS' or SubType='CS') And a.DealerType=1 And upper(a.State)='" + mCurrState + "' And s.Branch='" + gsCurBranch + "' And DocDate Between " + MmDdYy(mDate1) + " And " + MmDdYy(mDate2)
                    //If gpWareHouse And strLocation <> "" Then mStr = mStr + " And s.Locationcode In (" + strLocation + ")"
                    //.AddItem "Intra-State supplies to unregistered persons" + vbTab + fieldoftable("Sales s,Address a", "Sum(s.Amt)", "(s.GSTType=14) And " + mStr, "N") + vbTab + fieldoftable("Sales s,Address a", "Sum(s.Amt)", "(s.GSTType=10) And " + mStr, "N") + vbTab + fieldoftable("Sales s,Address a", "Sum(s.Amt)", "(s.GSTType=11) And " + mStr, "N")
                    //.RemoveItem 1
                    morderby = "";
                    break;
                case "hsn":
                    // HSN wise summary of GOODS / SERVICES supplied during the tax period
                    // HSN|Description|UQC|Total Quantity|Total Value|Taxable Value|Integrated Tax Amount|Central Tax Amount|State/UT Tax Amount|Cess Amount
                    mSQL = "Select s.hsncode as hsn,h.name,h.unit,sum(x.qty*-1) as [Total Quantity],sum(x.amt*-1) as [Total Value],sum(s.taxable*-1) as [Taxable Value],sum(s.igstamt*-1) as [Integrated Tax Amount],sum(s.cgstamt*-1) as [Central Tax Amount],sum(s.sgstamt*-1) as [State/UT Tax Amount], 0 as [Cess Amount]";
                    mSQL += " from StockTax s, Stock x, Sales l, hsnmaster h Where s.hsncode=h.code and s.parentkey=l.tablekey and s.tablekey=x.tablekey";
                    mSQL += " and s.GSTType<>4 and (x.SubType='JS' or x.SubType='RS' or x.SubType='CS' or x.SubType='XS' " + (chkReturnSR == false ? " Or x.SubType='NS'" : "") + " " + (chkReturnPR == false ? "or x.SubType='NP'" : "") + ") ";
                    //If gpWareHouse And strLocation <> "" Then
                    //mSQL = ReplaceCase(mSQL, "%LocationFilter", " And LocationCode In (" + strLocation + ")")
                    //mSQL += " And s.Locationcode In (" + strLocation + ")"
                    //Else
                    mSQL += " and s.Branch in ('" + mbranchcode + "')";
                    mSQL += " and x.DocDate Between '" + MMDDYY(Convert.ToDateTime(Model.FromDate)) + "' and '" + MMDDYY(Convert.ToDateTime(Model.ToDate)) + "'";
                    mSQL += " Group by s.HSNCode,h.Name,h.Unit order by s.HSNCode";
                    morderby = "";
                    //End If
                    break;
                case "docs":
                    // Create a new DataTable.    
                    DataColumn dtColumn;
                    DataRow myDataRow;
                    // Create Description column.    
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(Int32);
                    dtColumn.ColumnName = "RecordKey";
                    dtColumn.Unique = true;
                    mdt.Columns.Add(dtColumn);
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "Description";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "MinNumber";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(String);
                    dtColumn.ColumnName = "MaxNumber";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(Int32);
                    dtColumn.ColumnName = "Count1";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);
                    dtColumn = new DataColumn();
                    dtColumn.DataType = typeof(Int32);
                    dtColumn.ColumnName = "Count2";
                    dtColumn.Unique = false;
                    mdt.Columns.Add(dtColumn);

                    DataColumn[] PrimaryKeyColumns = new DataColumn[1];
                    PrimaryKeyColumns[0] = mdt.Columns["RecordKey"];
                    mdt.PrimaryKey = PrimaryKeyColumns;                      // Create a new DataSet and table to DS
                    DataSet dtSet = new DataSet();
                    dtSet.Tables.Add(mdt);
                    // add row data
                    mSQL = " (SubType='JS' or SubType='RS' or SubType='CS' or Subtype='XS') And Branch in ('" + mbranchcode + "') And DocDate Between '" + MMDDYY(mDate1) + "' And '" + MMDDYY(mDate2) + "'";
                    //If gpWareHouse And strLocation <> "" Then mStr = mStr + " And Locationcode In (" + strLocation + ")"
                    //common series types
                    //RS
                    //GSTFlag<>0 And  SDS
                    string mTypeCond = RecToString("Select Code from DocTypes Where DocBackward=0 And SubType='RS' And GSTNoCommon=0 And left(Code,1)<>'%'");
                    mTypeCond = " And CharIndex(Type,'" + mTypeCond + "')<>0";
                    //.AddItem "Invoice for Outward Supply" + vbTab + fieldoftable("Sales", "Min(Srl)", mStr + mTypeCond) + vbTab + fieldoftable("Sales", "Max(Srl)", mStr + mTypeCond) + vbTab + fieldoftable("Sales", "Count(*)", mStr + mTypeCond) + vbTab + fieldoftable("Sales", "Count(*)", mStr + mTypeCond + " And Amt=0");
                    int mcnt = 1;
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Description"] = "Invoice for Outward Supply (Credit Sales)";
                    myDataRow["MinNumber"] = Fieldoftable("Sales", "Min(Srl)", mSQL + mTypeCond);
                    myDataRow["MaxNumber"] = Fieldoftable("Sales", "Max(Srl)", mSQL + mTypeCond);
                    myDataRow["Count1"] = FieldoftableNumber("Sales", "Count(*)", mSQL + mTypeCond);
                    myDataRow["Count2"] = FieldoftableNumber("Sales", "Count(*)", mSQL + mTypeCond + " And Amt=0");
                    mdt.Rows.Add(myDataRow);
                    //CS
                    mTypeCond = RecToString("Select Code from DocTypes Where DocBackward=0 And SubType='CS' And GSTNoCommon=0 And GSTFlag<>0 And left(Code,1)<>'%'");
                    mTypeCond = "And CharIndex(Type,'" + mTypeCond + "')<>0";
                    if (Convert.ToInt32(Fieldoftable("Sales", "Count(*)", mSQL + mTypeCond, "N")) != 0)
                    {
                        //.AddItem "Invoice for Outward Supply" + vbTab + fieldoftable("Sales", "Min(Srl)", mStr + mTypeCond) + vbTab + fieldoftable("Sales", "Max(Srl)", mStr + mTypeCond) + vbTab + fieldoftable("Sales", "Count(*)", mStr + mTypeCond) + vbTab + fieldoftable("Sales", "Count(*)", mStr + mTypeCond + " And Amt=0");
                        myDataRow = mdt.NewRow();
                        myDataRow["RecordKey"] = mcnt++;
                        myDataRow["Description"] = "Invoice for Outward Supply (Cash Sale)";
                        myDataRow["MinNumber"] = Fieldoftable("Sales", "Min(Srl)", mSQL + mTypeCond);
                        myDataRow["MaxNumber"] = Fieldoftable("Sales", "Max(Srl)", mSQL + mTypeCond);
                        myDataRow["Count1"] = FieldoftableNumber("Sales", "Count(*)", mSQL + mTypeCond);
                        myDataRow["Count2"] = FieldoftableNumber("Sales", "Count(*)", mSQL + mTypeCond + " And Amt=0");
                        mdt.Rows.Add(myDataRow);
                    }
                    //XS
                    mTypeCond = RecToString("Select Code from DocTypes Where DocBackward=0 And SubType='XS' And GSTNoCommon=0 And GSTFlag<>0 And left(Code,1)<>'%'");
                    mTypeCond = "And CharIndex(Type,'" + mTypeCond + "')<>0";
                    if (Convert.ToInt32(Fieldoftable("Sales", "Count(*)", mSQL + mTypeCond, "N")) != 0)
                    {
                        //.AddItem "Invoice for Outward Supply" + vbTab + fieldoftable("Sales", "Min(Srl)", mStr + mTypeCond) + vbTab + fieldoftable("Sales", "Max(Srl)", mStr + mTypeCond) + vbTab + fieldoftable("Sales", "Count(*)", mStr + mTypeCond) + vbTab + fieldoftable("Sales", "Count(*)", mStr + mTypeCond + " And Amt=0");
                        myDataRow = mdt.NewRow();
                        myDataRow["RecordKey"] = mcnt++;
                        myDataRow["Description"] = "Invoice for Outward Supply (Export)";
                        myDataRow["MinNumber"] = Fieldoftable("Sales", "Min(Srl)", mSQL + mTypeCond);
                        myDataRow["MaxNumber"] = Fieldoftable("Sales", "Max(Srl)", mSQL + mTypeCond);
                        myDataRow["Count1"] = FieldoftableNumber("Sales", "Count(*)", mSQL + mTypeCond);
                        myDataRow["Count2"] = FieldoftableNumber("Sales", "Count(*)", mSQL + mTypeCond + " And Amt=0");
                        mdt.Rows.Add(myDataRow);
                    }
                    //individual series;
                    //var mdoclist = ctxTFAT.DocTypes.Where(z => z.DocBackward == false && (z.SubType == "RS" || z.SubType == "CS" || z.SubType == "XS") && z.GSTNoCommon == true && z.GSTFlag == true && z.MainType != z.SubType && z.SubType != z.Code).Select(x => x.Code).ToList();
                    DataTable mdoclist = GetDataTable("Select Code from DocTypes where DocBackward=0 and (SubType='RS' or SubType='CS' or SubType='XS') and GSTNoCommon<>0 and GSTFlag<>0 and MainType<>SubType and SubType<>Code");
                    foreach (DataRow mrow in mdoclist.Rows)
                    {
                        string mvar = mrow["Code"].ToString();
                        if (Convert.ToInt32(Fieldoftable("Sales", "Count(*)", mSQL + " And Type='" + mvar + "'", "N")) != 0)
                        {
                            //.AddItem "Invoice for Outward Supply" + vbTab + fieldoftable("Sales", "Min(Type+'-'+Srl)", mStr + " And Type='" + mRs!Code + "'") + vbTab + fieldoftable("Sales", "Max(Type+'-'+Srl)", mStr + " And Type='" + mRs!Code + "'") + vbTab + fieldoftable("Sales", "Count(*)", mStr + " And Type='" + mRs!Code + "'") + vbTab + fieldoftable("Sales", "Count(*)", mStr + " And Type='" + mRs!Code + "' And Amt=0");
                            myDataRow = mdt.NewRow();
                            myDataRow["RecordKey"] = mcnt++;
                            myDataRow["Description"] = "Invoice for Outward Supply (" + mvar + ")";
                            myDataRow["MinNumber"] = Fieldoftable("Sales", "Min(Type+'-'+Srl)", mSQL + " And Type='" + mvar + "'");
                            myDataRow["MaxNumber"] = Fieldoftable("Sales", "Max(Type+'-'+Srl)", mSQL + " And Type='" + mvar + "'");
                            myDataRow["Count1"] = FieldoftableNumber("Sales", "Count(*)", mSQL + " And Type='" + mvar + "'");
                            myDataRow["Count2"] = FieldoftableNumber("Sales", "Count(*)", mSQL + " And Type='" + mvar + "' And Amt=0");
                            mdt.Rows.Add(myDataRow);
                        }
                    }
                    mSQL = "s.GSTType=6 And (s.SubType='RP' or s.SubType='CP') And s.Branch in ('" + mbranchcode + "') And s.DocDate Between '" + MMDDYY(mDate1) + "' And '" + MMDDYY(mDate2) + "'" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(s.Type,'" + mDocString + "')=0");
                    //if gpWareHouse And strLocation <> "" Then mStr = mStr + " And s.Locationcode In (" + strLocation + ")";
                    //.AddItem "Invoice for Inward Supply from Unregistered Person" + vbTab + fieldoftable("[Purchase] s", "Min(s.Type+'-'+s.Srl)", mStr) + vbTab + fieldoftable("[Purchase] s", "Max(s.Type+'-'+s.Srl)", mStr) + vbTab + fieldoftable("[Purchase] s", "Count(*)", mStr) + vbTab + fieldoftable("[Purchase] s", "Count(*)", mStr + " And s.Amt=0");
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Description"] = "Invoice for Inward Supply from Unregistered Person.";
                    myDataRow["MinNumber"] = Fieldoftable("[Purchase] s", "Min(s.Type+'-'+s.Srl)", mSQL);
                    myDataRow["MaxNumber"] = Fieldoftable("[Purchase] s", "Max(s.Type+'-'+s.Srl)", mSQL);
                    myDataRow["Count1"] = FieldoftableNumber("[Purchase] s", "Count(*)", mSQL);
                    myDataRow["Count2"] = FieldoftableNumber("[Purchase] s", "Count(*)", mSQL + " And s.Amt=0");
                    mdt.Rows.Add(myDataRow);
                    //.AddItem "Revised Invoice";
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Description"] = "Revised Invoice";
                    myDataRow["MinNumber"] = 0;
                    myDataRow["MaxNumber"] = 0;
                    myDataRow["Count1"] = 0;
                    myDataRow["Count2"] = 0;
                    mdt.Rows.Add(myDataRow);
                    mSQL = " s.GSTType=0 And (s.SubType='DN' " + (chkPRRetDoc == false ? " or s.SubType='NP'" : "") + ") And s.Branch in ('" + mbranchcode + "') And s.DocDate Between '" + MMDDYY(mDate1) + "' And '" + MMDDYY(mDate2) + "'" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(s.Type,'" + mDocString + "')=0");
                    if (Fieldoftable("[Purchase] s", "Min(Type+'-'+Srl)", mSQL) != "")
                    {
                        //.AddItem "Debit Note" + vbTab + fieldoftable("[Purchase] s", "Min(s.Type+'-'+s.Srl)", mStr) + vbTab + fieldoftable("[Purchase] s", "Max(s.Type+'-'+s.Srl)", mStr) + vbTab + fieldoftable("[Purchase] s", "Count(*)", mStr) + vbTab + fieldoftable("[Purchase] s", "Count(*)", mStr + " And s.Amt=0");
                        myDataRow = mdt.NewRow();
                        myDataRow["RecordKey"] = mcnt++;
                        myDataRow["Description"] = "Debit Note";
                        myDataRow["MinNumber"] = Fieldoftable("[Purchase] s", "Min(s.Type+'-'+s.Srl)", mSQL);
                        myDataRow["MaxNumber"] = Fieldoftable("[Purchase] s", "Max(s.Type+'-'+s.Srl)", mSQL);
                        myDataRow["Count1"] = FieldoftableNumber("[Purchase] s", "Count(*)", mSQL);
                        myDataRow["Count2"] = FieldoftableNumber("[Purchase] s", "Count(*)", mSQL + " And s.Amt=0");
                        mdt.Rows.Add(myDataRow);
                    }
                    mSQL = " s.GSTType=0 And (s.SubType='CN' " + (chkSRRetDoc == false ? " Or s.SubType='NS'" : "") + ") And s.Branch in ('" + mbranchcode + "') And s.DocDate Between '" + MMDDYY(mDate1) + "' And '" + MMDDYY(mDate2) + "'" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(s.Type,'" + mDocString + "')=0");
                    //.AddItem "Credit Note" + vbTab + fieldoftable("[Sales] s", "Min(s.Type+'-'+s.Srl)", mStr) + vbTab + fieldoftable("[Sales] s", "Max(s.Type+'-'+s.Srl)", mStr) + vbTab + fieldoftable("[Sales] s", "Count(*)", mStr) + vbTab + fieldoftable("[Sales] s", "Count(*)", mStr + " And s.Amt=0"); ;
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Description"] = "Credit Note";
                    myDataRow["MinNumber"] = Fieldoftable("[Sales] s", "Min(s.Type+'-'+s.Srl)", mSQL);
                    myDataRow["MaxNumber"] = Fieldoftable("[Sales] s", "Max(s.Type+'-'+s.Srl)", mSQL);
                    myDataRow["Count1"] = FieldoftableNumber("[Sales] s", "Count(*)", mSQL);
                    myDataRow["Count2"] = FieldoftableNumber("[Sales] s", "Count(*)", mSQL + " And s.Amt=0");
                    mdt.Rows.Add(myDataRow);
                    mSQL = " (SubType='CN') And Branch in ('" + mbranchcode + "') And DocDate Between '" + MMDDYY(mDate1) + "' And '" + MMDDYY(mDate2) + "'" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(Type,'" + mDocString + "')=0");
                    //.AddItem "Credit Note" + vbTab + fieldoftable("Ledger", "Min(Type+'-'+Srl)", mStr) + vbTab + fieldoftable("Ledger", "Max(Type+'-'+Srl)", mStr) + vbTab + fieldoftable("Ledger", "Count(Distinct Type+Prefix+Srl)", mStr);
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Description"] = "Credit Note";
                    myDataRow["MinNumber"] = Fieldoftable("Ledger", "Min(Type+'-'+Srl)", mSQL);
                    myDataRow["MaxNumber"] = Fieldoftable("Ledger", "Max(Type+'-'+Srl)", mSQL);
                    myDataRow["Count1"] = FieldoftableNumber("Ledger", "Count(Distinct Type+Prefix+Srl)", mSQL);
                    myDataRow["Count2"] = 0;
                    mdt.Rows.Add(myDataRow);
                    mSQL = " (MainType='AR') And Branch in ('" + mbranchcode + "') And DocDate Between '" + MMDDYY(mDate1) + "' And '" + MMDDYY(mDate2) + "'" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(Type,'" + mDocString + "')=0");
                    //.AddItem "Receipt Voucher" + vbTab + fieldoftable("Ledger", "Min(Type+'-'+Srl)", mStr) + vbTab + fieldoftable("Ledger", "Max(Type+'-'+Srl)", mStr) + vbTab + fieldoftable("Ledger", "Count(*)", mStr + " And Debit+Credit<>0");
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Description"] = "Receipt Voucher";
                    myDataRow["MinNumber"] = Fieldoftable("Ledger", "Min(Type+'-'+Srl)", mSQL);
                    myDataRow["MaxNumber"] = Fieldoftable("[Sales] s", "Max(s.Type+'-'+s.Srl)", mSQL);
                    myDataRow["Count1"] = FieldoftableNumber("Ledger", "Max(Type+'-'+Srl)", mSQL);
                    myDataRow["Count2"] = FieldoftableNumber("Ledger", "Count(*)", mSQL + " And Debit+Credit<>0");
                    mdt.Rows.Add(myDataRow);
                    mSQL = " (MainType='AP') And Branch in ('" + mbranchcode + "') And DocDate Between '" + MMDDYY(mDate1) + "' And '" + MMDDYY(mDate2) + "'" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(Type,'" + mDocString + "')=0");
                    //.AddItem "Payment Voucher" + vbTab + fieldoftable("Ledger", "Min(Type+'-'+Srl)", mStr) + vbTab + fieldoftable("Ledger", "Max(Type+'-'+Srl)", mStr) + vbTab + fieldoftable("Ledger", "Count(*)", mStr + " And Debit+Credit<>0");
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Description"] = "Payment Voucher";
                    myDataRow["MinNumber"] = Fieldoftable("Ledger", "Min(Type+'-'+Srl)", mSQL);
                    myDataRow["MaxNumber"] = Fieldoftable("Ledger", "Max(Type+'-'+Srl)", mSQL);
                    myDataRow["Count1"] = FieldoftableNumber("Ledger", "Count(*)", mSQL + " And Debit+Credit<>0");
                    myDataRow["Count2"] = 0;
                    mdt.Rows.Add(myDataRow);
                    mSQL = " (s.SubType='IS') And s.Branch in ('" + mbranchcode + "') And s.DocDate Between '" + MMDDYY(mDate1) + "' And '" + MMDDYY(mDate2) + "'" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(s.Type,'" + mDocString + "')=0");
                    //.AddItem "Delivery Challan for Job work" + vbTab + fieldoftable("[Stock] s", "Min(s.Type+'-'+s.Srl)", mStr) + vbTab + fieldoftable("[Stock] s", "Max(s.Type+'-'+s.Srl)", mStr) ' + vbTab + fieldoftable("[Stock] s", "Count(*)", mStr + " And s.Amt=0");
                    myDataRow = mdt.NewRow();
                    myDataRow["RecordKey"] = mcnt++;
                    myDataRow["Description"] = "Delivery Challan for Job work";
                    myDataRow["MinNumber"] = Fieldoftable("[Stock] s", "Min(s.Type+'-'+s.Srl)", mSQL);
                    myDataRow["MaxNumber"] = Fieldoftable("[Stock] s", "Max(s.Type+'-'+s.Srl)", mSQL);
                    myDataRow["Count1"] = FieldoftableNumber("[Stock] s", "Count(*)", mSQL + " And s.Amt=0");
                    myDataRow["Count2"] = 0;
                    mdt.Rows.Add(myDataRow);
                    morderby = "";
                    //.AddItem "Delivery Challan for Supply on Approval";
                    //.AddItem "Delivery Challan in case of Liquid Gas";
                    //.AddItem "Delivery Challan in case other than by way of Supply (excluding at S no. 9 to 11)";
                    break;
            }

            //if (mcode != "exemp" ++ mcode != "docs")
            //{

            if (mcode == "b2cs" || mcode == "at" || mcode == "atadj")
            {
                //mSQL += " And s.Branch in ('" + mbranchcode + "')";
                //mSQL += " And t.DocDate Between '" + MMDDYY(Convert.ToDateTime(Model.FromDate)) + "' And '" + MMDDYY(Convert.ToDateTime(Model.ToDate)) + "'";
                //mSQL += " Group by x.StateCode+'-'+l.PlaceOfSupply,(s.iGSTRate+s.cGSTRate+s.sGSTRate)";

                mSQL += " And l.Branch in ('" + mbranchcode + "')";
                mSQL += " And l.DocDate Between '" + MMDDYY(Convert.ToDateTime(Model.FromDate)) + "' And '" + MMDDYY(Convert.ToDateTime(Model.ToDate)) + "'";
                mSQL += " Group by x.StateCode+'-'+l.PlaceOfSupply,(lr.iGSTRate+lr.cGSTRate+lr.sGSTRate)";
            }
            if (mcode == "b2cl" || mcode == "cdnr" || mcode == "cdnur" || mcode == "exp")
            {
                //mSQL += " And s.Branch in ('" + mbranchcode + "')";
                //mSQL += " And t.DocDate Between '" + MMDDYY(Convert.ToDateTime(Model.FromDate)) + "' And '" + MMDDYY(Convert.ToDateTime(Model.ToDate)) + "'";

                mSQL += " And l.Branch in ('" + mbranchcode + "')";
                mSQL += " And l.DocDate Between '" + MMDDYY(Convert.ToDateTime(Model.FromDate)) + "' And '" + MMDDYY(Convert.ToDateTime(Model.ToDate)) + "'";


            }
            if (morderby != "")
            {
                mSQL += " Order by " + morderby;
            }
            string mtotalstring = "";
            if (mcode != "docs")
            {
                var mcmd = @mSQL;
                mdt = GetDataTable(mcmd, GetConnectionString());
                // merge taxable amounts for repeated invoice numbers
                if (mcode == "cdnr")
                {
                    mdt = MergeRowData(mdt, "4", "9");
                }
                else if (mcode == "b2b")
                {
                    mdt = MergeRowData(mdt, "2,3,4", "6");
                    mdt = InsertTotalRow(mdt, "6,13,14,15");
                }
                else if (mcode == "hsn")
                {
                    mdt = InsertTotalRow(mdt, "4,5,6,7,8,9,10");
                }
                mtotalstring = GetTotalRowData(msumstring);
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
                        Response.Write(tab + dr[i].ToString().Replace("\"", "").Replace(@"\", "").Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace("\b", ""));
                        //Response.Write(tab + dr[i].ToString());
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
                Response.AddHeader("content-disposition",
                "attachment;filename=" + Model.ViewDataId + @".pdf");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                StringWriter sw = new StringWriter();
                HtmlTextWriter hw = new HtmlTextWriter(sw);
                GridView1.RenderControl(hw);
                StringReader sr = new StringReader(sw.ToString());
                Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
                pdfDoc.Open();
                htmlparser.Parse(sr);
                pdfDoc.Close();
                Response.Write(pdfDoc);
                Response.End();
                return null;
            }
            return Content(JQGridHelper.JsonForJqgrid(mdt, Model.rows, mdt.Rows.Count, Model.page, mtotalstring), "application/json");
        }

        public string GetSumString(SqlConnection con, string ViewDataId, string FromDate, string ToDate)
        {
            SqlCommand cmdx = new SqlCommand("Select dbo.fn_GetGridSumQuery(@mFormatCode,@mAlias,@mCurrDec,@mBranch,@mUserID,@mReportStartDate,@mReportEndDate,@mReturn)", con);
            cmdx.Parameters.Add("@mFormatCode", SqlDbType.VarChar).Value = ViewDataId.ToUpper();
            cmdx.Parameters.Add("@mAlias", SqlDbType.VarChar).Value = "";
            cmdx.Parameters.Add("@mCurrDec", SqlDbType.TinyInt).Value = 2;
            cmdx.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
            cmdx.Parameters.Add("@mUserID", SqlDbType.VarChar).Value = muserid;
            cmdx.Parameters.Add("@mReportStartDate", SqlDbType.VarChar).Value = FromDate;
            cmdx.Parameters.Add("@mReportEndDate", SqlDbType.VarChar).Value = ToDate;
            cmdx.Parameters.Add("@mReturn", SqlDbType.VarChar).Value = "B";
            con.Open();
            cmdx.Dispose();
            string mstr = "Select " + cmdx.ExecuteScalar().ToString();
            con.Close();
            return mstr;
        }

        public string GetTotalRowData(string @mSumString)
        {
            StringBuilder jsonBuilder = new StringBuilder();
            DataTable msumdt = GetDataTable(@mSumString);
            if (msumdt.Rows.Count > 0)
            {
                jsonBuilder.Append(",\"userdata\":{");
                foreach (DataRow row in msumdt.Rows)
                {
                    foreach (DataColumn column in msumdt.Columns)
                    {
                        jsonBuilder.Append("\"" + column.ColumnName + "\":");
                        jsonBuilder.Append(row[column].ToString() + ",");
                    }
                }
                if (jsonBuilder.ToString().EndsWith(","))
                {
                    jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                }

                jsonBuilder.Append("}");
            }
            msumdt.Dispose();
            return jsonBuilder.ToString();
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