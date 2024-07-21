using Common;
using EntitiModel;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using ALT_ERP3.Controllers;
using ALT_ERP3.DynamicBusinessLayer;
using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Accounts.Controllers
namespace ALT_ERP3.Areas.Reports.Controllers
{
    public class BankSlipPrintingController : BaseController
    {
        //IAddOnGridList AOlst = new AddonGridlist();
        //nEntities context = new nEntities();
        tfatEntities ctxTFAT = new tfatEntities();
        private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        private string FinPeriod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        //IReportGridOperation mIlst = new ListViewGridOperationreport();
        List<SelectListItem> Bank = new List<SelectListItem>();
        List<SelectListItem> BankName = new List<SelectListItem>();
        // GET: Reports/BankSlipPrinting
        public ActionResult Index(GridOption Model)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "");
            if (FinPeriod != "null")
            {
                var perd = ctxTFAT.TfatPerd.Where(x => x.Code == mcompcode && x.PerdCode == FinPeriod).Select(b => new { b.StartDate, b.LastDate }).FirstOrDefault();
                Model.FromDate = JQGridHelper.ConvertDate(perd.StartDate.ToShortDateString()).Replace("-", "/");
                Model.ToDate = JQGridHelper.ConvertDate(perd.LastDate.ToShortDateString()).Replace("-", "/");

            }
            return View(Model);
        }

        public ActionResult GetBank(GridOption Model)
        {
            var result = ctxTFAT.DocTypes.Where(x => x.SubType == "BR" && !x.Code.StartsWith("%")).Select(c => new { c.Code, c.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                Bank.Add(new SelectListItem { Text = item.Name, Value = item.Code });
            }

            return Json(Bank, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetBankName(GridOption Model)
        {
            var result = ctxTFAT.Master.Where(x => x.BaseGr == "B").Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                BankName.Add(new SelectListItem { Text = item.Name, Value = item.Code });
            }

            return Json(BankName, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id, GridOption Model)
        {
            Model.ViewDataId = "BankSlip";
            //id = Model.ViewDataId;
            return mIlst.getGridDataColumns(id, "", "", "", "");
        }
        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            Model.Code = "BankSlip";
            if (Model.Date != null)
            {
                var date = Model.Date.Split('-');
                Model.FromDate = JQGridHelper.ConvertDate(date[0]);
                Model.ToDate = JQGridHelper.ConvertDate(date[1]);

            }
            var SDate = Convert.ToDateTime(Model.FromDate);
            var LDate = Convert.ToDateTime(Model.ToDate);
            var GridContent = JQGridHelper.GetGridContent(Model);
            string[] SplitContent = GridContent.Split('&');
            for (int i = 0; i < SplitContent.Length; i++)
            {
                Model.SelectContent = SplitContent[0];
                Model.HeaderContent = SplitContent[1];
            }

            int startIndex = (Model.page - 1) * Model.rows;
            int endIndex = Model.page * Model.rows;

            Model.query = @"WITH PAGED_Stock  AS
                       (
                        select Top 10 '' as [Select],Ledger.DocDate as Date,Master.Name AS Particulars, Ledger.BanKCode AS Drawee Bank,Ledger.Cheque as Cheque No, Ledger.Narr AS Narration ,( Ledger.Debit+Ledger.Credit )as Amount
                        From LedgerBranch Ledger, 
                        Master WHERE Ledger.Code = Master.Code  And Ledger.Type IN (" + "'" + Model.AccountName + "'" + @") 
                        And Ledger.DocDate Between Convert(Datetime, " + "'" + SDate + "'" + @" , 103)
			            And Convert(Datetime ," + "'" + LDate + "'" + @" , 103)
                        AND Ledger.branch in (" + "'" + mbranchcode + "'" + @")
                        AND Ledger.Subtype = 'BR' And  Ledger.Branch+Type+Prefix+Srl 
                        In (Select Branch+Type+Prefix+Srl From Ledger Where Ledger.Branch in (" + "'" + mbranchcode + "'" + @")  And Ledger.Type IN (" + "'" + Model.AccountName + "'" + @")  And Ledger.Code = " + "'" + Model.BankName + "'" + @")
                        And  Master.Branch in (" + "'" + mbranchcode + "'" + @")
                        )
                        SELECT [Select],Date,Particulars,Drawee Bank,Cheque No,Narration,Amount
                        FROM PAGED_Stock";

            Model.queryforcount = @"SELECT Count(*)                         
                        From LedgerBranch Ledger, 
                        Master WHERE Ledger.Code = Master.Code  And Ledger.Type IN (" + "'" + Model.AccountName + "'" + @")  
                        And Ledger.DocDate Between Convert(Datetime, " + "'" + SDate + "'" + @" , 103)
			            And Convert(Datetime ," + "'" + LDate + "'" + @" , 103) AND Ledger.branch in (" + "'" + mbranchcode + "'" + @") 
                        AND Ledger.Subtype = 'BR' And  Ledger.Branch+Type+Prefix+Srl 
                        In (Select Branch+Type+Prefix+Srl From Ledger Where Ledger.Branch in (" + "'" + mbranchcode + "'" + @")  And Ledger.Type IN (" + "'" + Model.AccountName + "'" + @")  And Ledger.Code = " + "'" + Model.BankName + "'" + @")
                        And  Master.Branch in (" + "'" + mbranchcode + "'" + @")
                         ";

            return Content(JQGridHelper.JsonForJqgrid(JQGridHelper.GetDataTable(Model), Model.rows, JQGridHelper.GetTotalCount(Model), Model.page), "application/json");
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
            
                string list = Model.AccountName;
                string newList = string.Join(",", list.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.query = @"WITH PAGED_Ledger  AS
                        (
                          select Top 10 '' as [select],Ledger.DocDate as DocDate,Master.Name AS Particulars, Ledger.BanKCode AS DraweeBank,Ledger.Cheque as Cheque , Ledger.Narr AS Narration ,( Ledger.Debit+Ledger.Credit )as Amount
                        From LedgerBranch Ledger, 
                        Master WHERE Ledger.Code = Master.Code  And Ledger.Type IN (" + "'" + Model.AccountName + "'" + @") 
                        And Ledger.DocDate Between Convert(Datetime, " + "'" + SDate + "'" + @" , 103)
			            And Convert(Datetime ," + "'" + LDate + "'" + @" , 103)
                        AND Ledger.branch in (" + "'" + mbranchcode + "'" + @")
                        AND Ledger.Subtype = 'BR' And  Ledger.Branch+Type+Prefix+Srl 
                        In (Select Branch+Type+Prefix+Srl From Ledger Where Ledger.Branch in (" + "'" + mbranchcode + "'" + @")  And Ledger.Type IN (" + "'" + Model.AccountName + "'" + @")  And Ledger.Code = " + "'" + Model.BankName + "'" + @")
                        And  Master.Branch in (" + "'" + mbranchcode + "'" + @")
                        )
                        SELECT [select],DocDate,Particulars,DraweeBank,Cheque,Narration,Amount
                        FROM PAGED_Ledger
                        ";
            
          
            DataTable datatab = new DataTable();
            datatab = JQGridHelper.GetDataTable(Model);//your datatable
            string attachment = "attachment; filename=BankSlipPrinting.xls";
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
           
                string list = Model.AccountName;
                string newList = string.Join(",", list.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.query = @"WITH PAGED_Ledger  AS
                        (
                          select Top 10 '' as [select],Ledger.DocDate as DocDate,Master.Name AS Particulars, Ledger.BanKCode AS DraweeBank,Ledger.Cheque as Cheque , Ledger.Narr AS Narration ,( Ledger.Debit+Ledger.Credit )as Amount
                        From LedgerBranch Ledger, 
                        Master WHERE Ledger.Code = Master.Code  And Ledger.Type IN (" + "'" + Model.AccountName + "'" + @") 
                        And Ledger.DocDate Between Convert(Datetime, " + "'" + SDate + "'" + @" , 103)
			            And Convert(Datetime ," + "'" + LDate + "'" + @" , 103)
                        AND Ledger.branch in (" + "'" + mbranchcode + "'" + @")
                        AND Ledger.Subtype = 'BR' And  Ledger.Branch+Type+Prefix+Srl 
                        In (Select Branch+Type+Prefix+Srl From Ledger Where Ledger.Branch in (" + "'" + mbranchcode + "'" + @")  And Ledger.Type IN (" + "'" + Model.AccountName + "'" + @")  And Ledger.Code = " + "'" + Model.BankName + "'" + @")
                        And  Master.Branch in (" + "'" + mbranchcode + "'" + @")
                        )
                        SELECT [select],DocDate,Particulars,DraweeBank,Cheque,Narration,Amount
                        FROM PAGED_Ledger
                        ";
          
            DataTable datatab = new DataTable();
            datatab = JQGridHelper.GetDataTable(Model);//your datatable

            GridView GridView1 = new GridView();
            GridView1.AllowPaging = false;
            GridView1.DataSource = datatab;
            GridView1.DataBind();

            Response.ContentType = "application /pdf";
            Response.AddHeader("content-disposition",
            "attachment;filename=BankSlipPrinting.pdf");
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