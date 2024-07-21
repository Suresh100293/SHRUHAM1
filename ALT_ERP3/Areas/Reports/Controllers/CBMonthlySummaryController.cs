using Comman;
using EntitiModel;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
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
using ALT_ERP3.DynamicBusinessLayer;
using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Reports.Controllers
{
    public class CBMonthlySummaryController : Controller
    {
        nEntities context = new nEntities();
        tfatEntities context1 = new tfatEntities();
        private string BranchCode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private string CompCode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        private string FinPeriod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        //IReportGridOperation mIlst = new ListViewGridOperationreport();
        private decimal TOpening;
        // GET: Reports/CBMonthlySummary
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

        public ActionResult GetMonthlySummary(GridOption Model)
        {
            if (FinPeriod != "null")
            {
                var perd = context.TfatPerds.Where(x => x.Code == CompCode && x.PerdCode == FinPeriod).Select(b => new { b.StartDate, b.LastDate }).FirstOrDefault();
                Model.FromDate = JQGridHelper.ConvertDate(perd.StartDate.ToShortDateString()).Replace("-", "/");
                Model.ToDate = JQGridHelper.ConvertDate(perd.LastDate.ToShortDateString()).Replace("-", "/");
            }
            var SDate = Convert.ToDateTime(Model.FromDate);
            var LDate = Convert.ToDateTime(Model.ToDate);


            Model.query = @"WITH PAGED_Stock  AS
             (
             Select Month(Docdate) as Month, Sum(Debit) as Debit, sum(Credit) as Credit
             from Ledger where Ledger.MainType <> 'MV' And Ledger.MainType <> 'PV' And  Code =" + "'" + Model.AccountName + "'" + @"
             And DocDate Between Convert(Datetime, " + "'" + SDate + "'" + @" , 103)
			 And Convert(Datetime ," + "'" + LDate + "'" + @" , 103)
             And Branch in (" + "'" + BranchCode + "'" + @") And Left(Ledger.Authorise, 1) = 'A'
             Group by  Month(Docdate)
             ),
            D as
            (Select Sum(Debit - Credit) as mBal from Ledger
              where Ledger.MainType <> 'MV' And Ledger.MainType <> 'PV' And Branch in(" + "'" + BranchCode + "'" + @")
            And Code =" + "'" + Model.AccountName + "'" + @" And DocDate<= '31-Dec-2015'
            And Left(Ledger.Authorise, 1) = 'A' )
            SELECT Month,
            Opening =
            CASE
            WHEN Month = '1' THEN(select Isnull(D.mBal, 0) where Month = 1)
            END,
            Debit,Credit,Balance =
            CASE Month
            WHEN '1' THEN(select((select Isnull(D.mBal, 0) where Month = 1) + (Debit - Credit)) where Month = 1)
            END
            FROM PAGED_Stock,D
            ";
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tfatEntitiesConnectionString"].ConnectionString);
            SqlDataAdapter adap = new SqlDataAdapter(Model.query, conn);
            adap.Fill(ds);
            dt = ds.Tables[0];
            conn.Close();
            List<GridOption> newlist = dt.DataTableToList<GridOption>();
            List<GridOption> newlist1 = new List<GridOption>();
            for (int i = 0; i < newlist.Count; i++)
            {
                if (i == 0)
                {
                    TOpening = newlist[i].Opening;
                    Session.Add("bal", newlist[i].Balance);
                }
                else
                {
                    TOpening = Convert.ToDecimal(Session["bal"].ToString());
                    newlist[i].Balance = (Convert.ToDecimal(Session["bal"].ToString()) + (newlist[i].Debit - newlist[i].Credit));
                    Session.Add("bal", newlist[i].Balance);
                }
                newlist1.Add(new GridOption
                {
                    Month = newlist[i].Month,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(newlist[i].Month)),
                    Opening = TOpening,
                    Debit = newlist[i].Debit,
                    Credit = newlist[i].Credit,
                    Balance = newlist[i].Balance,
                });
            }
            Model.SumDebit = newlist.Sum(x => x.Debit);
            Model.SumCredit = newlist.Sum(x => x.Credit);
            Model.list = newlist1.OrderBy(x => x.Month).ToList(); 
            if (Model.AccountName != null)
            {
                return Json(new { Status = "Success", Html = this.RenderPartialView("MonthlySummary", Model) }, JsonRequestBehavior.AllowGet);
            }
            return View(Model);
        }
      
        public ActionResult GetAccountList(string term)
        {
            if (term == "")
            {
                var result = context1.Masters.Where(x => x.Flag == "L" && x.BaseGr == "B" || x.BaseGr == "C" && x.Branch == BranchCode && x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).Take(10).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = context1.Masters.Where(x => x.Flag == "L" && x.Branch == BranchCode && x.Name.Contains(term) && ((x.BaseGr == "B") || (x.BaseGr == "C"))).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetExcel(GridOption Model)
        {
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
            Model.query = @"WITH PAGED_Stock  AS
             (
             Select Month(Docdate) as Month, Sum(Debit) as Debit, sum(Credit) as Credit
             from Ledger where Ledger.MainType <> 'MV' And Ledger.MainType <> 'PV' And  Code =" + "'" + Model.AccountName + "'" + @"
             And DocDate Between Convert(Datetime, " + "'" + SDate + "'" + @" , 103)
			 And Convert(Datetime ," + "'" + LDate + "'" + @" , 103)
             And Branch in(" + "'" + BranchCode + "'" + @") And Left(Ledger.Authorise, 1) = 'A'
             Group by  Month(Docdate)
             ),
            D as
            (Select Sum(Debit - Credit) as mBal from Ledger
              where Ledger.MainType <> 'MV' And Ledger.MainType <> 'PV' And Branch in(" + "'" + BranchCode + "'" + @")
            And Code =" + "'" + Model.AccountName + "'" + @" And DocDate<= '31-Dec-2015'
            And Left(Ledger.Authorise, 1) = 'A' )
            SELECT Month,
            Opening =
            CASE
            WHEN Month = '1' THEN(select Isnull(D.mBal, 0) where Month = 1)
            END,
            Debit,Credit,Balance =
            CASE Month
            WHEN '1' THEN(select((select Isnull(D.mBal, 0) where Month = 1) + (Debit - Credit)) where Month = 1)
            END
            FROM PAGED_Stock,D
            ";

            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tfatEntitiesConnectionString"].ConnectionString);
            SqlDataAdapter adap = new SqlDataAdapter(Model.query, conn);
            adap.Fill(ds);
            dt = ds.Tables[0];
            conn.Close();
            List<GridOption> newlist = dt.DataTableToList<GridOption>();
            List<GridOption> newlist1 = new List<GridOption>();
            for (int i = 0; i < newlist.Count; i++)
            {
                if (i == 0)
                {
                    TOpening = newlist[i].Opening;
                    Session.Add("bal", newlist[i].Balance);
                }
                else
                {
                    TOpening = Convert.ToDecimal(Session["bal"].ToString());
                    newlist[i].Balance = (Convert.ToDecimal(Session["bal"].ToString()) + (newlist[i].Debit - newlist[i].Credit));
                    Session.Add("bal", newlist[i].Balance);
                }
                newlist1.Add(new GridOption
                {
                    Month = newlist[i].Month,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(newlist[i].Month)),
                    Opening = TOpening,
                    Debit = newlist[i].Debit,
                    Credit = newlist[i].Credit,
                    Balance = newlist[i].Balance,
                });
            }
            Model.SumDebit = newlist.Sum(x => x.Debit);
            Model.SumCredit = newlist.Sum(x => x.Credit);
            Model.list = newlist1.OrderBy(x => x.Month).ToList();

            var products = Model.list.OrderBy(x => x.Month).ToList();

            var grid = new GridView();
            grid.DataSource = from p in products
                              select new
                              {
                                  p.Month,
                                  p.MonthName,
                                  p.Opening,
                                  p.Debit,
                                  p.Credit,
                                  p.Balance,
                                 
                              };
            grid.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=CashBankMonthlySummary.xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);
            grid.RenderControl(htw);
            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();
            return null;
        }

        public ActionResult GetPDF(GridOption Model)
        {
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
            Model.query = @"WITH PAGED_Stock  AS
             (
             Select Month(Docdate) as Month, Sum(Debit) as Debit, sum(Credit) as Credit
             from Ledger where Ledger.MainType <> 'MV' And Ledger.MainType <> 'PV' And  Code =" + "'" + Model.AccountName + "'" + @"
             And DocDate Between Convert(Datetime, " + "'" + SDate + "'" + @" , 103)
			 And Convert(Datetime ," + "'" + LDate + "'" + @" , 103)
             And Branch in(" + "'" + BranchCode + "'" + @") And Left(Ledger.Authorise, 1) = 'A'
             Group by  Month(Docdate)
             ),
            D as
            (Select Sum(Debit - Credit) as mBal from Ledger
              where Ledger.MainType <> 'MV' And Ledger.MainType <> 'PV' And Branch in(" + "'" + BranchCode + "'" + @")
            And Code =" + "'" + Model.AccountName + "'" + @" And DocDate<= '31-Dec-2015'
            And Left(Ledger.Authorise, 1) = 'A' )
            SELECT Month,
            Opening =
            CASE
            WHEN Month = '1' THEN(select Isnull(D.mBal, 0) where Month = 1)
            END,
            Debit,Credit,Balance =
            CASE Month
            WHEN '1' THEN(select((select Isnull(D.mBal, 0) where Month = 1) + (Debit - Credit)) where Month = 1)
            END
            FROM PAGED_Stock,D
            ";

            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["tfatEntitiesConnectionString"].ConnectionString);
            SqlDataAdapter adap = new SqlDataAdapter(Model.query, conn);
            adap.Fill(ds);
            dt = ds.Tables[0];
            conn.Close();
            List<GridOption> newlist = dt.DataTableToList<GridOption>();
            List<GridOption> newlist1 = new List<GridOption>();
            for (int i = 0; i < newlist.Count; i++)
            {
                if (i == 0)
                {
                    TOpening = newlist[i].Opening;
                    Session.Add("bal", newlist[i].Balance);
                }
                else
                {
                    TOpening = Convert.ToDecimal(Session["bal"].ToString());
                    newlist[i].Balance = (Convert.ToDecimal(Session["bal"].ToString()) + (newlist[i].Debit - newlist[i].Credit));
                    Session.Add("bal", newlist[i].Balance);
                }
                newlist1.Add(new GridOption
                {
                    Month = newlist[i].Month,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(newlist[i].Month)),
                    Opening = TOpening,
                    Debit = newlist[i].Debit,
                    Credit = newlist[i].Credit,
                    Balance = newlist[i].Balance,
                });
            }
            Model.SumDebit = newlist.Sum(x => x.Debit);
            Model.SumCredit = newlist.Sum(x => x.Credit);
            Model.list = newlist1.OrderBy(x => x.Month).ToList();

            var products = newlist1.OrderBy(x => x.Month).ToList();

            var grid = new GridView();
            grid.DataSource = from p in products
                              select new
                              {
                                  p.Month,
                                  p.MonthName,
                                  p.Opening,
                                  p.Debit,
                                  p.Credit,
                                  p.Balance,

                              };
            grid.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.ContentType = "application / pdf";
            Response.AddHeader("content-disposition",
            "attachment;filename=CashBankMonthlySummary.pdf");
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            StringWriter sw = new StringWriter();
            HtmlTextWriter hw = new HtmlTextWriter(sw);
            grid.RenderControl(hw);
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