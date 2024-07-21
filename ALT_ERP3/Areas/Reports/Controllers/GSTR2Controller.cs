using Common;
using EntitiModel;
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

namespace ALT_ERP3.Areas.Reports.Controllers
{
    public class GSTR2Controller : BaseController
    {
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
            return GetGridDataColumns(Model.ViewDataId.ToUpper(), "X", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            string mcode = Model.ViewDataId.Substring(6);
            string mSQL = "";
            string mSQL2 = "";
            string morderby = "t.Type,t.Srl";
            //chkReturnP = Consider Purchase Return in CDNR
            //chkReturnPR = Don't Consider Purchase Return in HSN Summary
            //chkShowPurRet = Consider Purchase Return in B2B
            //chkReturnSR = Don't Consider Sales Return in GSTR1
            //chkDontSRinCS = Don't Consider Sales Return in B2CS
            //chkPRRetDoc = Don't Consider Purchase Return in Docs
            bool chkShowPurRet = Model.xAdd;
            bool chkReturnSR = Model.xEdit;
            bool chkReturnPR = Model.xDelete;
            bool chkDontSRinCS = Model.xView;
            bool chkReturnP = Model.IsFormatSelected;
            bool chkPRRetDoc = Model.ComController;
            string mCurrState = "";
            var date = Model.Date.Replace("-", "/").Split(':');
            Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            DataTable mdt = new DataTable("gstr1docs");
            string mDate1 = "'" + MMDDYY(Convert.ToDateTime(Model.FromDate)) + "'";
            string mDate2 = "'" + MMDDYY(Convert.ToDateTime(Model.ToDate)) + "'"; ;

            // get select fields from tfatsearch
            //string connstring = string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings[""].ConnectionString) == true ? "" : ConfigurationManager.ConnectionStrings[""].ConnectionString;
            SqlConnection con = new SqlConnection(GetConnectionString());

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
            //  2     3      4     5     6      7     8    9       10     11    12
            // b2b, b2bur, imps, impg, cdnr,  cdnur, at, atadj,  exemp, itcr, hsnsum
            if (mcode != "atadj" && mcode != "itcr" && mcode != "hsnsum")
            {
                mSQL = mSQL + " from StockTax s";
                mSQL = mSQL + " left join Stock t on s.tablekey = t.tablekey";
                mSQL = mSQL + " left join Purchase l on s.ParentKey = l.TableKey";
                mSQL = mSQL + " left join PurchaseMore r on s.ParentKey = r.TableKey";
                mSQL = mSQL + " Left Join Address a on l.Code = a.Code and l.AltAddress = a.Sno";
                mSQL = mSQL + " Left Join TfatState x on x.Name = l.PlaceOfSupply ";
                mSQL = mSQL + " where " + (string.IsNullOrEmpty(mDocString) ? "" : "CharIndex(t.Type,'" + mDocString + "')=0 And ");
            }
            switch (mcode)
            {
                case "b2b":
                    // 1. Summary Of Supplies From Registered Suppliers B2B(3)
                    mSQL = mSQL + " (t.MainType='PM' or t.MainType='JV' or t.SubType='RP' or t.SubType='CP' or t.SubType='NP' or t.SubType='HP' or t.SubType='JI') And a.GSTNo<>'' And (s.iGSTAmt+s.cGSTAmt+s.sGSTAmt)<>0 And (s.GSTType=0 or s.GSTType=13 or s.GSTType=1)";
                    morderby = "a.GSTNo,t.Type,t.Srl";
                    break;
                case "b2bur":
                    // 2. Summary Of Supplies From Unregistered Suppliers B2BUR(4B)
                    mSQL = mSQL + " (t.MainType='PM' or t.MainType='JV' or t.SubType='JI' or t.SubType='RP' or t.SubType='CP' or t.SubType='NP' or t.SubType='HP') And (a.GSTNo='') And (s.GSTType=0 or s.GSTType=6 or s.GSTType=1)";
                    morderby = "a.GSTNo,t.Type,t.Srl";
                    break;
                case "imps":
                    // 3. Summary For IMPS (4C) - import of services
                    mSQL = mSQL + " t.SubType='IM' And s.iGSTAmt<>0 And (s.GSTType=0 And l.CurrRate<>1) And (Select Top 1 HSNMaster.Flag from HSNMaster Where HSNMaster.Code=s.HSNCode)='S'";
                    break;
                case "impg":
                    // 4. Summary For IMPG (5) - import of goods from overseas or SEZ
                    mSQL = mSQL + " (t.SubType='IM' or t.SubType='RP' or t.SubType='JI') And a.GSTNo<>'' And (s.GSTType=7 or s.GSTType=8 or (s.GSTType=0 And l.CurrRate<>1)) And (Select Top 1 HSNMaster.Flag from HSNMaster Where HSNMaster.Code=s.HSNCode)='I'";
                    break;
                case "cdnr":
                    // 5. Summary For CDNR (6C)
                    mSQL = mSQL + " ((t.subtype='RP' and l.Reasoncode<>0) or t.SubType='NP' or t.SubType='AP') and a.GSTNo<>'' And (s.GSTType=0)";
                    break;
                case "cdnur":
                    // 6. Summary For CDNUR (6C)
                    mSQL = mSQL + " ((t.subtype='RP' and l.Reasoncode<>0) or t.SubType='NP' or t.SubType='AP') and a.GSTNo='' And (a.DealerType=1 or s.GSTType=6)";
                    break;
                case "at":
                    // 7. Summary For Tax Liability on Advance Paid under Reverse Charge (10 A)
                    mSQL = mSQL + " t.SubType='AP' And a.GSTNo<>'' And (s.GSTType=1)";
                    break;
                case "atadj":
                    // 8. Summary For Adjustment of advance tax paid earlier for reverse charge supplies (10 B)
                    ExecuteStoredProc("SPTFAT_DeleteTempTable xTemp");
                    mSQL2 = "Select t.DocDate,s.Cess,s.IGSTAmt,s.CGSTAmt,s.SGSTAmt,s.iGSTRate,s.cGSTRate,s.sGSTRate,t.Party,s.Branch,s.AltAddress,s.Taxable,";
                    mSQL2 = mSQL2 + " (Select isnull(Sum(((o.Amount*100)/e.Amt)*e.Taxable/100),0) from Outstanding o,Sales e where o.DocDate Between " + mDate1 + " and " + mDate2;
                    mSQL2 = mSQL2 + " and o.TableRefKey=s.TableKey and o.ParentKey=e.TableKey) as Amt into xTemp ";
                    mSQL2 = mSQL2 + " from StockTax s left Join Stock t on s.TableKey=t.TableKey Where s.ItemType='I' and t.SubType='AR' And s.Branch='" + mbranchcode + "'";
                    mSQL2 = mSQL2 + " and t.DocDate<" + mDate1;
                    ExecuteStoredProc(mSQL2);
                    //DelTbl gsdbDbf, "xTemp";
                    //gsdbDbf.Execute mSql2;
                    mSQL = mSQL + " from xTemp s, Address a Where s.Party=a.Code And s.AltAddress=a.Sno And s.Amt<>0";
                    break;
                case "exemp":
                    // 9.Summary For Composition, Nil rated, exempted and non GST inward supplies(7)
                    // Composition Taxable Person,   Exempt Supply,  NIL Rated Supply,   Non-GST Supply
                    // .FixedRows = 1
                    // .Rows = 2
                    // mStr = "s.CurrRate<>0 And s.iGSTAmt+s.cGSTAmt+s.sGSTAmt=0 And a.Code=s.Code And a.Branch=s.Branch And a.Sno=s.AltAddress And (s.SubType='RP' or s.SubType='HP' or s.SubType='IM' or s.SubType='JI') And upper(a.State)<>'" + mCurrState + "' And s.Branch in (" + mBranchCode + ") And DocDate Between " + MmDdYy(mDate1) + " And " + MmDdYy(mDate2)
                    // If gpWarehouse And strLocation<> "" Then mStr = mStr & " And s.Locationcode In (" & strLocation & ")"
                    //.AddItem "Inter-State supplies" & vbTab & FieldOfTable("Purchase s,Address a", "Sum(s.Taxable)", "(s.GSTType=12 And a.DealerType=2) And " + mStr, "N") & vbTab & FieldOfTable("Purchase s,Address a", "Sum(s.Taxable)", "(s.GSTType=0 And a.DealerType=0) And " + mStr, "N") & vbTab & FieldOfTable("Purchase s,Address a", "Sum(s.Taxable)", "(s.GSTType=10 And s.iGSTAmt+s.cGSTAmt+s.sGSTAmt=0) And " + mStr, "N") & vbTab & FieldOfTable("Purchase s,Address a", "Sum(s.Taxable)", "(s.GSTType=11 And s.iGSTAmt+s.cGSTAmt+s.sGSTAmt=0) And " + mStr, "N")
                    // mStr = "s.CurrRate<>0 And s.iGSTAmt+s.cGSTAmt+s.sGSTAmt=0 And a.Code=s.Code And a.Branch=s.Branch And a.Sno=s.AltAddress And (s.SubType='RP' or s.SubType='HP' or s.SubType='IM' or s.SubType='JI') And upper(a.State)='" + mCurrState + "' And s.Branch in (" + mBranchCode + ") And DocDate Between " + MmDdYy(mDate1) + " And " + MmDdYy(mDate2)
                    // If gpWarehouse And strLocation<> "" Then mStr = mStr & " And s.Locationcode In (" & strLocation & ")"
                    //.AddItem "Intra-State supplies" & vbTab & FieldOfTable("Purchase s,Address a", "Sum(s.Taxable)", "(s.GSTType=12 And a.DealerType=2) And " + mStr, "N") & vbTab & FieldOfTable("Purchase s,Address a", "Sum(s.Taxable)", "(s.GSTType=0 And a.DealerType=0) And " + mStr, "N") & vbTab & FieldOfTable("Purchase s,Address a", "Sum(s.Taxable)", "(s.GSTType=10 And s.iGSTAmt+s.cGSTAmt+s.sGSTAmt=0) And " + mStr, "N") & vbTab & FieldOfTable("Purchase s,Address a", "Sum(s.Taxable)", "(s.GSTType=11 And s.iGSTAmt+s.cGSTAmt+s.sGSTAmt=0) And " + mStr, "N")
                    //.RemoveItem 1
                    break;
                case "itcr":
                    // 10. Summary Input Tax credit Reversal/Reclaim (11)
                    //.FixedRows = 1
                    //.Rows = 2
                    //.AddItem "(a) Amount in terms of rule 37(2)" '& vbTab & FieldOfTable("Sales s,Address a", "Sum(s.Amt)", "(s.GSTType<>10 and s.GSTType<>11) And " + mStr, "N") & vbTab & FieldOfTable("Sales s,Address a", "Sum(s.Amt)", "(s.GSTType=10) And " + mStr, "N") & vbTab & FieldOfTable("Sales s,Address a", "Sum(s.Amt)", "(s.GSTType=11) And " + mStr, "N")
                    //.AddItem "(b) Amount in terms of rule 42(1)(m)"
                    //.AddItem "(c) Amount in terms of rule 43(1)(h)"
                    //.AddItem "(d) Amount in terms of rule 42(2)(a)"
                    //.AddItem "(e) Amount in terms of rule 42(2)(b)"
                    //.AddItem "(f) On account of amount paid subsequent to reversal of ITC"
                    //.AddItem "(g) Any other liability (Specify)"
                    //.RemoveItem 1
                    break;
                case "hsnsum":
                    // 11. Summary For HSN (13)
                    // HSN|Description|UQC|Total Quantity|Total Value|Taxable Value|Integrated Tax Amount|Central Tax Amount|State/UT Tax Amount|Cess Amount
                    mSQL = mSQL + " from Stock t Left join stockTax s on s.TableKey=t.TableKey left join itemmaster i on i.code = t.code left join hsnmaster h on h.code = i.hsncode, Purchase l Where t.ParentKey=l.TableKey And (t.SubType='RP' or t.SubType='HP' or t.SubType='JI' or t.MainType='PM' or t.MainType='JV' or t.Subtype='NP' or t.subtype='IM')";
                    break;
            }

            if (mcode != "itcr" || mcode != "exemp")
            {
                mSQL = mSQL + " and s.Branch='" + mbranchcode + "'";
                if (mcode != "at")
                {
                    mSQL = mSQL + " And t.DocDate Between " + mDate1 + " and " + mDate2;
                }

                switch (mcode)
                {
                    case "at":
                        mSQL = mSQL + " Group by a.StateCode+'-'+a.State,(s.iGSTRate+s.cGSTRate+s.sGSTRate)";
                        break;
                    case "atadj":
                        mSQL = mSQL + " Group by a.StateCode+'-'+a.State,(s.iGSTRate+s.cGSTRate+s.sGSTRate)";
                        break;
                    case "hsnsum":
                        mSQL = mSQL + " Group by h.Code, h.Name, h.Unit";
                        break;
                    default:
                        mSQL = mSQL + " Order by " + morderby;
                        break;
                }
            }
            var mcmd = @mSQL;
            mdt = GetDataTable(mcmd, GetConnectionString());

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