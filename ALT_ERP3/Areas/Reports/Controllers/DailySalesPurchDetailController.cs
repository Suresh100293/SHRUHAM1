using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Controllers;
using ALT_ERP3.DynamicBusinessLayer;
using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Reports.Controllers
{
    public class DailySalesPurchDetailController : BaseController
    {
        //nEntities context = new nEntities();
        tfatEntities ctxTFAT = new tfatEntities();
        private int mlocationcode = 100001;
        private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private string CompCode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        private string FinPeriod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        //IReportGridOperation mIlst = new ListViewGridOperationreport();
        List<SelectListItem> Store = new List<SelectListItem>();
        List<SelectListItem> Sales = new List<SelectListItem>();
        List<SelectListItem> Purchases = new List<SelectListItem>();
        List<SelectListItem> ItemName = new List<SelectListItem>();
        List<SelectListItem> Itemgrp = new List<SelectListItem>();
        // GET: Reports/DailySalesPurchDetail
        public ActionResult Index(GridOption Model)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "");
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
        public ActionResult Getstore(GridOption Model)
        {

            var result = ctxTFAT.Stores.Where(x => x.LocationCode == mlocationcode ).Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                Store.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }
            return Json(Store, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSales(GridOption Model)
        {

            var result = ctxTFAT.DocTypes.Where(x => x.MainType == "SL" && !x.Code.StartsWith("%") && ((x.SubType != "ES" )|| (x.SubType != "QS") || (x.SubType != "OS") || (x.SubType != "PI") || (x.SubType != "OC") || (x.SubType != "SX"))).Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                Sales.Add(new SelectListItem { Text = item.Name, Value = item.Code });
            }
            return Json(Sales, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetPurchase(GridOption Model)
        {

            var result = ctxTFAT.DocTypes.Where(x => x.MainType == "PR" && !x.Code.StartsWith("%") && ((x.SubType != "IP") || (x.SubType != "EP") || (x.SubType != "QP") || (x.SubType != "OP") || (x.SubType != "IC") || (x.SubType != "PX"))).Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                Purchases.Add(new SelectListItem { Text = item.Name, Value = item.Code });
            }
            return Json(Purchases, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
            id = "DlySalPurDetails";
            return mIlst.getGridDataColumns(id, "", "", "", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            Model.Code = "DlySalPurDetails";

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
            if (Model.Sales != null)
            {
                string list = Model.Sales;
                string newList = string.Join(",", list.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Sales = "And Sales.Type in (" + "" + newList + "" + @")";
            }
            if (Model.Purchase != null)
            {
                string list = Model.Purchase;
                string newList = string.Join(",", list.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Purchase = "And Purchase.Type in (" + "" + newList + "" + @")";
            }

            int startIndex = (Model.page - 1) * Model.rows;
            int endIndex = Model.page * Model.rows;
            Model.query = @"WITH PAGED_Stock  AS
                        (
                         SELECT " + Model.SelectContent + @",
                         ROW_NUMBER() OVER (ORDER BY " + "Stock.Code" + @" " + Model.sord + @") AS RowNumber
                         From Stock Stock, Sales, ItemMaster ItemMaster, Stores Stores  
                         Where (stock.Type + stock.prefix + stock.srl + stock.branch ) = (Sales.Type + Sales.prefix + Sales.Srl + Sales.branch)  
                         and ItemMaster.Code = Stock.Code  and Stock.Store = Stores.Code   
                         and Stock.DocDate between  '01-Apr-2017' And '31-Mar-2018'   
                         and Stock.Branch = 'HO0000' And Left(Stock.AUTHORISE,1) = 'A' 
                         AND ItemMaster.NonStock=0  and Stores.Branch = 'HO0000'  and Sales.Branch = 'HO0000' 
                         GROUP BY stock.Type,stock.Code, ItemMaster.Name, stock.docdate
                        )
                        SELECT " + Model.HeaderContent + @"
                        FROM PAGED_Stock
                        WHERE RowNumber BETWEEN " + startIndex + @" AND " + endIndex + @";";

            Model.queryforcount = @"SELECT Count(*) From (                        
                        Select Stock.Type as COL1,Stock.Code as COL2,ItemMaster.Name as COL3,Convert(char(10),
						Stock.DocDate, 103) as COL4,Cast(ABS(Sum(Stock.Qty)) as Decimal(15,3)) as COL5,
						Cast(ABS(Sum(Stock.Amt)) as Decimal(14,2)) as COL6,Cast(Avg(Stock.Rate) as Money) as COL7 
						From Stock Stock, Sales, ItemMaster ItemMaster, Stores Stores  
						Where (stock.Type + stock.prefix + stock.srl + stock.branch ) = (Sales.Type + Sales.prefix + Sales.Srl + Sales.branch)  
						and ItemMaster.Code = Stock.Code  and Stock.Store = Stores.Code   
						 and Stock.DocDate between  '01-Apr-2017' And '31-Mar-2018'   
						 and Stock.Branch = 'HO0000' And Left(Stock.AUTHORISE,1) = 'A' 
						 AND ItemMaster.NonStock=0  and Stores.Branch = 'HO0000'  and Sales.Branch = 'HO0000'  
						 GROUP BY stock.Type,stock.Code, ItemMaster.Name, stock.docdate)d1";

            return Content(JQGridHelper.JsonForJqgrid(JQGridHelper.GetDataTable(Model), Model.rows, JQGridHelper.GetTotalCount(Model), Model.page), "application/json");
        }
    }
}