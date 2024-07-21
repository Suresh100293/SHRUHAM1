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
    public class StoreWiseStockDetailController : Controller
    {
        //nEntities context = new nEntities();
        tfatEntities ctxTFAT = new tfatEntities();
        private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private string CompCode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        private string FinPeriod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        //IReportGridOperation mIlst = new ListViewGridOperationreport();
        List<SelectListItem> PartyName = new List<SelectListItem>();
        List<SelectListItem> Process = new List<SelectListItem>();
        List<SelectListItem> ProductCat = new List<SelectListItem>();
        List<SelectListItem> Store = new List<SelectListItem>();
        List<SelectListItem> ItemName = new List<SelectListItem>();
        List<SelectListItem> Itemgrp = new List<SelectListItem>();
        List<SelectListItem> SalesManName = new List<SelectListItem>();
        List<SelectListItem> Brokername = new List<SelectListItem>();
        // GET: Reports/StoreWiseStockDetail
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

        public ActionResult GetItemGroups(GridOption Model)
        {
            var result = ctxTFAT.ItemGroups.Select(m => new { m.Code, m.Name }).ToList();
            foreach (var item in result)
            {
                Itemgrp.Add(new SelectListItem { Text = item.Name, Value = item.Code });
            }

            return Json(Itemgrp, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetItemName(GridOption Model)
        {
            if (Model.Code != null)
            {
                var List = Model.Code.Split(',').ToList();
                var SubType = List.ToString();
                if (List.Count > 1)
                {
                    foreach (var item in List)
                    {
                        var result = ctxTFAT.ItemMaster.Where(m => m.Grp == Model.Code).Select(m => new { m.Code, m.Name }).Distinct().ToList();
                        foreach (var item1 in result)
                        {
                            ItemName.Add(new SelectListItem { Text = item1.Name, Value = item1.Code });
                        }
                    }
                }
                else
                {
                    var result = ctxTFAT.ItemMaster.Where(m => m.Grp == Model.Code).Select(m => new { m.Code, m.Name }).Distinct().ToList();
                    foreach (var item1 in result)
                    {
                        ItemName.Add(new SelectListItem { Text = item1.Name, Value = item1.Code });
                    }
                }
            }
            return Json(ItemName, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetProcess(GridOption Model)
        {
            var result = ctxTFAT.ProcessMas.Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                Process.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }
            return Json(Process, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetProductCategory(GridOption Model)
        {
            var result = ctxTFAT.ProcessMas.Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                ProductCat.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }
            return Json(ProductCat, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Getstore(GridOption Model)
        {
            var result = ctxTFAT.Stores.Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                Store.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }
            return Json(Store, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSalesMan(GridOption Model)
        {
            var result = ctxTFAT.SalesMan.Where(x => x.Flag == "L").Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                SalesManName.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }
            return Json(SalesManName, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBroker(GridOption Model)
        {
            var result = ctxTFAT.Broker.Where(x => x.Locked == false ).Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                Brokername.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }
            return Json(Brokername, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id, GridOption Model)
        {
            Model.ViewDataId = "StkStoreProcReport";
            id = Model.ViewDataId;
            return mIlst.getGridDataColumns(id, "", "", "", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            Model.Code = "StkStoreProcReport";
            if (Model.Date != null)
            {
                var date = Model.Date.Split('-');
                Model.FromDate = JQGridHelper.ConvertDate(date[0]);
                Model.ToDate = JQGridHelper.ConvertDate(date[1]);

            }
            var SDate = Convert.ToDateTime(Model.FromDate);
            var LDate = Convert.ToDateTime(Model.ToDate);
            var Asondate = DateTime.Now;
            var GridContent = JQGridHelper.GetGridContent(Model);
           
            string[] SplitContent = GridContent.Split('&');
            for (int i = 0; i < SplitContent.Length; i++)
            {
                Model.SelectContent = SplitContent[0];
                Model.HeaderContent = SplitContent[1];
            }

            int startIndex = (Model.page - 1) * Model.rows;
            int endIndex = Model.page * Model.rows;
            if (Model.Item != "0")
            {
                string list = Model.Item;
                string newList = string.Join(",", list.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Value1 = "And ItemMaster.Code in (" + "" + newList + "" + @")";
            }
            Model.Value2 = "dbo.GetStock(Stock.Code,Stock.Store,Stock.Branch,Convert(Datetime, " + "'" + SDate + "'" + @" , 103))";
            Model.query = @"WITH PAGED_Stock  AS
                        (
                         SELECT " + Model.SelectContent + @",Cast(dbo.GetStock(Stock.Code,Stock.Store,Stock.Branch,Convert(Datetime, " + "'" + LDate + "'" + @" , 103)) as Decimal(12,0)) as Qty,
                        ROW_NUMBER() OVER (ORDER BY " + "Stock.Code" + @" " + Model.sord + @") AS RowNumber
                        From Stock, ItemMaster, Stores  
                        Where ItemMaster.Code = Stock.Code  and Stock.Store = Stores.Code 
                        And Stores.Branch = Stock.Branch  and Stock.DocDate <= Convert(Datetime, " + "'" + Asondate + "'" + @" , 103) 
                        And Stock.NotInStock=0 And stock.Branch in(" + "'" + mbranchcode + "'" + @") 
                        " + Model.Value1 + @" And Left(Stock.AUTHORISE,1) = 'A'  
                        Group By Stores.Name,Stock.code,ItemMaster.Name,Stock.Unit,Stock.Store,Stock.Branch
                        )
                        SELECT " + Model.HeaderContent + @"
                        FROM PAGED_Stock
                        WHERE RowNumber BETWEEN " + startIndex + @" AND " + endIndex + @";";

            Model.queryforcount = @"SELECT Count(*) From (
                        SELECT  Stores.Name,Stock.code,Stock.Unit,Stock.Store,Stock.Branch  From Stock, ItemMaster, Stores 
                        Where ItemMaster.Code = Stock.Code  and Stock.Store = Stores.Code 
                        And Stores.Branch = Stock.Branch  and Stock.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103)  
                        And Stock.NotInStock=0 And stock.Branch in(" + "'" + mbranchcode + "'" + @") And Left(Stock.AUTHORISE,1) = 'A'  
                        Group By Stores.Name,Stock.code,Stock.Unit,Stock.Store,Stock.Branch
                        )d1";

            return Content(JQGridHelper.JsonForJqgrid(JQGridHelper.GetDataTable(Model), Model.rows, JQGridHelper.GetTotalCount(Model), Model.page), "application/json");
        }

    }
}