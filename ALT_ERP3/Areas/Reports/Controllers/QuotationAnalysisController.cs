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
    public class QuotationAnalysisController : Controller
    {
        //nEntities context = new nEntities();
        tfatEntities ctxTFAT = new tfatEntities();
        private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private string CompCode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        private string FinPeriod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        //IReportGridOperation mIlst = new ListViewGridOperationreport();
        List<SelectListItem> ItemName = new List<SelectListItem>();
        List<SelectListItem> Type = new List<SelectListItem>();
        // GET: Reports/QuotationAnalysis
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

        public ActionResult GetItemName(GridOption Model)
        {
            var result = ctxTFAT.ItemMaster.Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                ItemName.Add(new SelectListItem { Text = item.Name, Value = item.Code });
            }

            return Json(ItemName, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetType(GridOption Model)
        {
            var result = ctxTFAT.DocTypes.Where(x => x.SubType == Model.SubType && !x.Code.StartsWith("%")).Select(c => new { c.Code, c.Name }).Distinct().ToList();
            foreach (var item1 in result)
            {
                Type.Add(new SelectListItem { Text = item1.Name, Value = item1.Code });
            }

            return Json(Type, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id, GridOption Model)
        {
            if (Model.Value1 == "IP")
            {
                Model.ViewDataId = "QtnAnalysisInd";
                id = Model.ViewDataId;
            }
            if (Model.Value1 == "EP")
            {
                Model.ViewDataId = "QtnAnalysisEnq";
                id = Model.ViewDataId;
            }
            if (Model.Value1 =="PW")
            {
                Model.ViewDataId = "QtnAnalysisItem";
                id = Model.ViewDataId;
            }
            return mIlst.getGridDataColumns(id, "", "", "", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {

            if (Model.Value1 == "IP")
            {
                Model.Code = "QtnAnalysisInd";

            }
            if (Model.Value1 == "EP")
            {
                Model.Code = "QtnAnalysisEnq";

            }
            if (Model.Value1 == "PW")
            {
                Model.Code = "QtnAnalysisItem";

            }

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
            if (Model.Item != null)
            {
                string list = Model.Item;
                string newList = string.Join(",", list.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Item = "And QuoteStk.Code in (" + "" + newList + "" + @")";
            }
            if (Model.Value1 == "PW")
            {
                Model.query = @"WITH PAGED_Stock  AS
                        (
                         SELECT " + Model.SelectContent + @",
                         ROW_NUMBER() OVER (ORDER BY " + "Master.Name,QuoteStk.Code,QuoteStk.Type,QuoteStk.Docdate,QuoteStk.Prefix,QuoteStk.Srl,QuoteStk.Sno" + @" " + Model.sord + @") AS RowNumber
                         FROM QuoteStk , Master, ItemMaster, Address WHERE 
                         QuoteStk.Code=ItemMaster.Code AND QuoteStk.Branch in(" + "'" + mbranchcode + "'" + @") And Left(QuoteStk.AUTHORISE,1) = 'A' AND 
                         Master.Branch in(" + "'" + mbranchcode + "'" + @") AND Master.Flag = 'L' AND Master.Code = QuoteStk.Party AND Address.Branch= Master.Branch 
                         AND Address.Code =  Master.Code AND QuoteStk.SubType = 'QP' AND QuoteStk.CancFlag='N' 
                         AND QuoteStk.DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                         and QuoteStk.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103) " + Model.Item + @"
                        )
                        SELECT " + Model.HeaderContent + @"
                        FROM PAGED_Stock
                        WHERE RowNumber BETWEEN " + startIndex + @" AND " + endIndex + @";";

                Model.queryforcount = @"SELECT Count(*)                         
                         FROM QuoteStk , Master, ItemMaster, Address WHERE 
                         QuoteStk.Code=ItemMaster.Code AND QuoteStk.Branch in(" + "'" + mbranchcode + "'" + @") And Left(QuoteStk.AUTHORISE,1) = 'A' AND 
                         Master.Branch in(" + "'" + mbranchcode + "'" + @") AND Master.Flag = 'L' AND Master.Code = QuoteStk.Party AND Address.Branch= Master.Branch 
                         AND Address.Code =  Master.Code AND QuoteStk.SubType = 'QP' AND QuoteStk.CancFlag='N' 
                         AND QuoteStk.DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                         and QuoteStk.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103) " + Model.Item + @")";
            }
           
            return Content(JQGridHelper.JsonForJqgrid(JQGridHelper.GetDataTable(Model), Model.rows, JQGridHelper.GetTotalCount(Model), Model.page), "application/json");
        }
    }
}