using Comman;
using EntitiModel;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using ALT_ERP3.DynamicBusinessLayer;
using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Reports.Controllers
{
    public class CBBookController : Controller
    {
        nEntities context = new nEntities();
        tfatEntities context1 = new tfatEntities();
        private string BranchCode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private string CompCode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        private string FinPeriod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        //IReportGridOperation mIlst = new ListViewGridOperationreport();
        // GET: Reports/CBBook
        public ActionResult Index(GridOption Model)
        {
            if (FinPeriod != "null")
            {
                var perd = context.TfatPerds.Where(x => x.Code == CompCode && x.PerdCode == FinPeriod).Select(b => new { b.StartDate, b.LastDate }).FirstOrDefault();
                Model.FromDate = JQGridHelper.ConvertDate(perd.StartDate.ToShortDateString()).Replace("-", "/");
                Model.ToDate = JQGridHelper.ConvertDate(perd.LastDate.ToShortDateString()).Replace("-", "/");

            }
            return View(Model);
        }

        public ActionResult GetAccountList(string term)
        {
            if (term == "")
            {
                var result = context1.Masters.Where(x => x.Flag == "L" && x.Branch == BranchCode && x.Name.Contains(term) && ((x.BaseGr == "B") || (x.BaseGr == "C"))).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).Take(10).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = context1.Masters.Where(x => x.Flag == "L"  && x.Branch == BranchCode && x.Name.Contains(term) && ((x.BaseGr == "B") || (x.BaseGr == "C"))).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id, GridOption Model)
        {
            if (FinPeriod != "null")
            {
                var perd = context.TfatPerds.Where(x => x.Code == CompCode && x.PerdCode == FinPeriod).Select(b => new { b.StartDate, b.LastDate }).FirstOrDefault();
                Model.FromDate = JQGridHelper.ConvertDate(perd.StartDate.ToShortDateString()).Replace("-", "/");
                Model.ToDate = JQGridHelper.ConvertDate(perd.LastDate.ToShortDateString()).Replace("-", "/");

            }
            var SDate = Convert.ToDateTime(Model.FromDate);
            var LDate = Convert.ToDateTime(Model.ToDate);

            var result1 = (from L in context1.Ledgers
                           where L.Code == Model.AccountName && L.DocDate < SDate.Date
                           select new { L.Credit, L.Debit }).ToList();

            var result2 = (from L in context1.Ledgers
                           where L.Code == Model.AccountName && L.DocDate <= LDate.Date
                           select new { L.Credit, L.Debit }).ToList();


            if (Model.Date != "-")
            {
                var date = Model.Date.Split('-');
                Model.FromDate1 = JQGridHelper.ConvertDate(date[0]).ToString();
                Model.ToDate1 = JQGridHelper.ConvertDate(date[1]).ToString();

            }

            DateTime TSDate = Convert.ToDateTime(Model.FromDate1);
            DateTime TLDate = Convert.ToDateTime(Model.ToDate1);


            var result3 = (from L in context1.Ledgers
                           where L.Code == Model.AccountName && L.DocDate >= TSDate.Date && L.DocDate <= TLDate
                           select new { L.Credit }).ToList();

            var result4 = (from L in context1.Ledgers
                           where L.Code == Model.AccountName && L.DocDate >= TSDate.Date && L.DocDate <= TLDate
                           select new { L.Debit }).ToList();

            var OpeningBal = Convert.ToString(result1.ToList().Select(X => X.Debit - X.Credit).Sum());
            var ClosingBal = Convert.ToString(result2.ToList().Select(X => X.Debit - X.Credit).Sum());
            var TCredit = Convert.ToString(result3.ToList().Select(X => X.Credit).Sum());
            var TDebit = Convert.ToString(result4.ToList().Select(X => X.Debit).Sum());

            id = "AcMultLedgerScreen";
            return mIlst.getGridDataColumns(id, OpeningBal, ClosingBal, TCredit, TDebit);
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            Model.Code = "AcMultLedgerScreen";
            if (Model.Date != null)
            {
                var date = Model.Date.Split('-');
                Model.FromDate = JQGridHelper.ConvertDate(date[0]);
                Model.ToDate = JQGridHelper.ConvertDate(date[1]);

            }
            var SDate = Convert.ToDateTime(Model.FromDate).Date;
            var LDate = Convert.ToDateTime(Model.ToDate).Date;

            var GridContent = JQGridHelper.GetGridContent(Model);
            string[] SplitContent = GridContent.Split('&');
            for (int i = 0; i < SplitContent.Length; i++)
            {
                Model.SelectContent = SplitContent[0];
                Model.HeaderContent = SplitContent[1];
            }
            int startIndex = (Model.page - 1) * Model.rows;
            int endIndex = Model.page * Model.rows;
            Model.query = @"WITH PAGED_Ledger  AS
                        (
                         Select " + Model.SelectContent + @",
                         ROW_NUMBER() OVER (ORDER BY " + Model.sidx + @" " + Model.sord + @") AS RowNumber
                         from Ledger 
                         inner join Master Master on Master.Code = Ledger.AltCode
                         LEFT JOIN Purchase ON Ledger.Type=Purchase.Type And Ledger.Prefix=Purchase.Prefix And Ledger.Srl=Purchase.Srl And Ledger.Branch=Purchase.Branch  
                         where Ledger.Code = " + "'" + Model.AccountName + "'" + @" and Ledger.Docdate >= Convert(Datetime, " + "'" + SDate + "'" + @", 103) 
                         and Ledger.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @", 103) 
                        )
                        SELECT " + Model.HeaderContent + @"
                        FROM PAGED_Ledger
                        WHERE RowNumber BETWEEN " + startIndex + @" AND " + endIndex + @";";

            Model.queryforcount = @"SELECT Count(*)                         
                                  from Ledger 
                                  inner join Master Master on Master.Code = Ledger.AltCode
                                  LEFT JOIN Purchase ON Ledger.Type=Purchase.Type And Ledger.Prefix=Purchase.Prefix And Ledger.Srl=Purchase.Srl And Ledger.Branch=Purchase.Branch  
                                  where Ledger.Code = " + "'" + Model.AccountName + "'" + @" and Ledger.Docdate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                                  and Ledger.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103)";

            return Content(JQGridHelper.JsonForJqgrid(JQGridHelper.GetDataTable(Model), Model.rows, JQGridHelper.GetTotalCount(Model), Model.page), "application/json");
        }

        public ActionResult GetExcel(GridOption Model)
        {
            Model.Code = "AcMultLedgerScreen";
            if (Model.Date != null)
            {
                var date = Model.Date.Split('-');
                Model.FromDate = JQGridHelper.ConvertDate(date[0]);
                Model.ToDate = JQGridHelper.ConvertDate(date[1]);

            }
            var SDate = Convert.ToDateTime(Model.FromDate).Date;
            var LDate = Convert.ToDateTime(Model.ToDate).Date;

            var GridContent = JQGridHelper.GetGridContent(Model);
            string[] SplitContent = GridContent.Split('&');
            for (int k = 0; k < SplitContent.Length; k++)
            {
                Model.SelectContent = SplitContent[0];
                Model.HeaderContent = SplitContent[1];
            }
            int startIndex = (Model.page - 1) * Model.rows;
            int endIndex = Model.page * Model.rows;
            Model.query = @"WITH PAGED_Ledger  AS
                        (
                         Select " + Model.SelectContent + @",
                         ROW_NUMBER() OVER (ORDER BY " + "Ledger.Type" + @" " + Model.sord + @") AS RowNumber
                         from Ledger 
                         inner join Master Master on Master.Code = Ledger.AltCode
                         LEFT JOIN Purchase ON Ledger.Type=Purchase.Type And Ledger.Prefix=Purchase.Prefix And Ledger.Srl=Purchase.Srl And Ledger.Branch=Purchase.Branch  
                         where Ledger.Code = " + "'" + Model.AccountName + "'" + @" and Ledger.Docdate >= Convert(Datetime, " + "'" + SDate + "'" + @", 103) 
                         and Ledger.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @", 103) 
                        )
                        SELECT " + Model.HeaderContent + @"
                        FROM PAGED_Ledger
                        ";
            DataTable datatab = new DataTable();
            datatab = JQGridHelper.GetDataTable(Model);//your datatable
            string attachment = "attachment; filename=CASHBANKBOOK.xls";
            Response.ClearContent();
            Response.AddHeader("content-disposition", attachment);
            Response.ContentType = "application/vnd.ms-excel";
            string tab = "";
            foreach (DataColumn dc in datatab.Columns)
            {
                Response.Write(tab + dc.ColumnName);
                tab = "\t";
            }
            Response.Write("\n");
            int i;
            foreach (DataRow dr in datatab.Rows)
            {
                tab = "";
                for (i = 0; i < datatab.Columns.Count; i++)
                {
                    Response.Write(tab + dr[i].ToString());
                    tab = "\t";
                }
                Response.Write("\n");
            }
            Response.End();
            return null;

        }

        public ActionResult GetPDF(GridOption Model)
        {
            Model.Code = "AcMultLedgerScreen";
            if (Model.Date != null)
            {
                var date = Model.Date.Split('-');
                Model.FromDate = JQGridHelper.ConvertDate(date[0]);
                Model.ToDate = JQGridHelper.ConvertDate(date[1]);

            }
            var SDate = Convert.ToDateTime(Model.FromDate).Date;
            var LDate = Convert.ToDateTime(Model.ToDate).Date;

            var GridContent = JQGridHelper.GetGridContent(Model);
            string[] SplitContent = GridContent.Split('&');
            for (int k = 0; k < SplitContent.Length; k++)
            {
                Model.SelectContent = SplitContent[0];
                Model.HeaderContent = SplitContent[1];
            }
            int startIndex = (Model.page - 1) * Model.rows;
            int endIndex = Model.page * Model.rows;
            
            Model.query = @"WITH PAGED_Ledger  AS
                        (
                        Select " + Model.SelectContent + @",
                         ROW_NUMBER() OVER (ORDER BY " + "Ledger.Type" + @" " + Model.sord + @") AS RowNumber
                         from Ledger 
                         inner join Master Master on Master.Code = Ledger.AltCode
                         LEFT JOIN Purchase ON Ledger.Type=Purchase.Type And Ledger.Prefix=Purchase.Prefix And Ledger.Srl=Purchase.Srl And Ledger.Branch=Purchase.Branch  
                         where Ledger.Code = " + "'" + Model.AccountName + "'" + @" and Ledger.Docdate >= Convert(Datetime, " + "'" + SDate + "'" + @", 103) 
                         and Ledger.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @", 103) 
                        )
                        SELECT " + Model.HeaderContent + @"
                        FROM PAGED_Ledger
                        ";

            DataTable datatab = new DataTable();
            datatab = JQGridHelper.GetDataTable(Model);//your datatable

            GridView GridView1 = new GridView();
            GridView1.AllowPaging = false;
            GridView1.DataSource = datatab;
            GridView1.DataBind();

            Response.ContentType = "application / pdf";
            Response.AddHeader("content-disposition",
            "attachment;filename=CASHBANKBOOK.pdf");
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
    }
}