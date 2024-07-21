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
    public class ProductDespatchController : Controller
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
        // GET: Reports/ProductDespatch
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
            var result = ctxTFAT.ItemGroups.Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                Itemgrp.Add(new SelectListItem { Text = item.Name, Value = item.Code });
            }

            return Json(Itemgrp, JsonRequestBehavior.AllowGet);
        }

    
        public ActionResult GetItemName(GridOption Model)
        {
            var result = ctxTFAT.ItemMaster.Where(m => m.Grp == Model.Code).Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                ItemName.Add(new SelectListItem { Text = item.Name, Value = item.Code });
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
        public ActionResult GetPartyName(GridOption Model)
        {

            var result = ctxTFAT.Master.Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                PartyName.Add(new SelectListItem { Text = item.Name, Value = item.Code });
            }
            return Json(PartyName, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetSalesMan(GridOption Model)
        {
            var result = ctxTFAT.SalesMan.Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                SalesManName.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(SalesManName, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBroker(GridOption Model)
        {
            var result = ctxTFAT.Broker.Where(x => x.Locked ==false ).Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                Brokername.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(Brokername, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id, GridOption Model)
        {

            Model.ViewDataId = "DespatchPlan";
            id = Model.ViewDataId;

            return mIlst.getGridDataColumns(id, "", "", "", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {

            Model.Code = "DespatchPlan";

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
                Model.Item = "And ItemMaster.Code in (" + "" + newList + "" + @")";
            }
            if (Model.Party != null)
            {
                string list1 = Model.Party;
                string newList1 = string.Join(",", list1.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Party = "And Master.Code in (" + "" + newList1 + "" + @")";
            }
            Model.query = @"WITH PAGED_Stock  AS
                        (
                         SELECT " + Model.SelectContent + @",
                         ROW_NUMBER() OVER (ORDER BY " + "Master.Name,Orders.OrdNumber" + @" " + Model.sord + @") AS RowNumber
                         FROM ((((OrdersStk LEFT JOIN DlySchedule 
                         On OrdersStk.Branch+OrdersStk.Maintype+OrdersStk.Type+OrdersStk.Prefix+OrdersStk.Srl+OrdersStk.Sno= DlySchedule.Branch+DlySchedule.Maintype+DlySchedule.Type+DlySchedule.Prefix+DlySchedule.Srl+DlySchedule.Sno) 
                         LEFT JOIN Orders ON OrdersStk.Branch+OrdersStk.Maintype+OrdersStk.Type+OrdersStk.Prefix+OrdersStk.Srl= Orders.Branch+Orders.Maintype+Orders.Type+Orders.Prefix+Orders.Srl) 
                         LEFT JOIN ItemMaster ON ItemMaster.Code=OrdersStk.Code)
                         LEFT JOIN Master ON Master.Code=OrdersStk.Party And Master.Branch=OrdersStk.Branch)
                         WHERE OrdersStk.Branch in (" + "'" + mbranchcode + "'" + @")
                         AND Orders.Docdate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                         and Orders.Docdate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103) 
                        )
                        SELECT " + Model.HeaderContent + @"
                        FROM PAGED_Stock
                        WHERE RowNumber BETWEEN " + startIndex + @" AND " + endIndex + @";";

            Model.queryforcount = @"SELECT Count(*)                         
                        FROM ((((OrdersStk LEFT JOIN DlySchedule 
                         On OrdersStk.Branch+OrdersStk.Maintype+OrdersStk.Type+OrdersStk.Prefix+OrdersStk.Srl+OrdersStk.Sno= DlySchedule.Branch+DlySchedule.Maintype+DlySchedule.Type+DlySchedule.Prefix+DlySchedule.Srl+DlySchedule.Sno) 
                         LEFT JOIN Orders ON OrdersStk.Branch+OrdersStk.Maintype+OrdersStk.Type+OrdersStk.Prefix+OrdersStk.Srl= Orders.Branch+Orders.Maintype+Orders.Type+Orders.Prefix+Orders.Srl) 
                         LEFT JOIN ItemMaster ON ItemMaster.Code=OrdersStk.Code)
                         LEFT JOIN Master ON Master.Code=OrdersStk.Party And Master.Branch=OrdersStk.Branch)
                         WHERE OrdersStk.Branch in (" + "'" + mbranchcode + "'" + @")
                         AND Orders.Docdate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                         and Orders.Docdate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103) 
                         ";


            return Content(JQGridHelper.JsonForJqgrid(JQGridHelper.GetDataTable(Model), Model.rows, JQGridHelper.GetTotalCount(Model), Model.page), "application/json");
        }
    }
}