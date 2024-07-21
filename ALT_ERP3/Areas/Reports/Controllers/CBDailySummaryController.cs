using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.DynamicBusinessLayer;
using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Reports.Controllers
{
    public class CBDailySummaryController : Controller
    {
        //nEntities context = new nEntities();
        tfatEntities ctxTFAT = new tfatEntities();
        private string BranchCode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private int CompCode = (int) System.Web.HttpContext.Current.Session["CompCode"];
        private string FinPeriod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        //IReportGridOperation mIlst = new ListViewGridOperationreport();
        // GET: Reports/CBDailySummary
        public ActionResult Index(GridOption Model)
        {
            if (FinPeriod != "null")
            {
                var perd = ctxTFAT.TfatPerd.Where(x => x.Code == CompCode && x.PerdCode == FinPeriod).Select(b => new { b.StartDate, b.LastDate }).FirstOrDefault();
                Model.FromDate = JQGridHelper.ConvertDate(perd.StartDate.ToShortDateString()).Replace("-", "/");
                Model.ToDate = JQGridHelper.ConvertDate(perd.LastDate.ToShortDateString()).Replace("-", "/");

            }
            return View(Model);
        }
        public ActionResult GetAccountList(string term)
        {
            if (term == "")
            {
                var result = ctxTFAT.Master.Where(x => x.Flag == "L" && x.BaseGr == "B" || x.BaseGr == "C" && x.Branch == BranchCode && x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).Take(10).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.Master.Where(x => x.Flag == "L" && x.BaseGr == "B" || x.BaseGr == "C" && x.Branch == BranchCode && x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id, GridOption Model)
        {

            Model.id = "DlyAccSummary";
            id = Model.id;
            return mIlst.getGridDataColumns(id, "", "", "", "");

        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {

            Model.Code = "DlyAccSummary";

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
                         SELECT Month(Docdate) as Month,Sum(Debit) as Debit, Sum(Credit) as Credit,
                        ROW_NUMBER() OVER (ORDER BY " + "Ledger.DocDate" + @" " + Model.sord + @") AS RowNumber
                        from Ledger 
                        where  Ledger.MainType<>'MV' And Ledger.MainType<>'PV' And  Code =" + "'" + Model.AccountName + "'" + @"
                        and Ledger.DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                        and Ledger.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103)
                        And Branch = 'HO0000' And Left(Ledger.AUTHORISE,1) = 'A' 
                         Group by Ledger.DocDate
                        )
                        SELECT Month,Debit,Credit
                        FROM PAGED_Stock
                        WHERE RowNumber BETWEEN " + startIndex + @" AND " + endIndex + @";";

            Model.queryforcount = @"SELECT Count(*)                         
                        from Ledger 
                        where  Ledger.MainType<>'MV' And Ledger.MainType<>'PV' And  Code =" + "'" + Model.AccountName + "'" + @"
                        and Ledger.DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                        and Ledger.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103)
                        And Branch = 'HO0000' And Left(Ledger.AUTHORISE,1) = 'A' 
                        ";

            return Content(JQGridHelper.JsonForJqgrid(JQGridHelper.GetDataTable(Model), Model.rows, JQGridHelper.GetTotalCount(Model), Model.page), "application/json");
        }
    }
}