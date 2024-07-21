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
    public class PurchSalesAnalysisController : BaseController
    {

       //nEntities context = new nEntities();
        tfatEntities ctxTFAT = new tfatEntities();
        private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private string CompCode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        private string FinPeriod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        //IReportGridOperation mIlst = new ListViewGridOperationreport();
        List<SelectListItem> Itemgrp = new List<SelectListItem>();
        List<SelectListItem> Area = new List<SelectListItem>();
        List<SelectListItem> Types = new List<SelectListItem>();
        List<SelectListItem> ItemName = new List<SelectListItem>();
        List<SelectListItem> ProductName = new List<SelectListItem>();
        List<SelectListItem> productsgrp = new List<SelectListItem>();
        List<SelectListItem> SalesManName = new List<SelectListItem>();
        // GET: Reports/PurchSalesAnalysis
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
        public ActionResult GetPartyName(GridOption Model)
        {
            if (Model.MainType == "PR")
            {
                var result = ctxTFAT.Master.Where(x => x.BaseGr == "S" || x.BaseGr == "U" ).Select(m => new { m.Code, m.Name }).Distinct().ToList();
                foreach (var item in result)
                {
                    ProductName.Add(new SelectListItem { Text = item.Name, Value = item.Code });
                }
            }
            else
            {
                var result = ctxTFAT.Master.Where(x => x.BaseGr == "D" || x.BaseGr == "U" ).Select(m => new { m.Code, m.Name }).Distinct().ToList();
                foreach (var item in result)
                {
                    ProductName.Add(new SelectListItem { Text = item.Name, Value = item.Code });
                }
            }
            return Json(ProductName, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetSalesMan(GridOption Model)
        {
            var result = ctxTFAT.SalesMan.Where(x => x.Locked ==false).Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                SalesManName.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(SalesManName, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAreas(GridOption Model)
        {
            var result = ctxTFAT.AreaMaster.Where(x => x.Locked == false).Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                Area.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(Area, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetDoctypes(GridOption Model)
        {
            var result = ctxTFAT.DocTypes.Where(x => x.SubType == "OP" && !x.Code.StartsWith("%")).Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                Types.Add(new SelectListItem { Text = item.Name, Value = item.Code });
            }
            return Json(Types, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetGridStructureRecords(string id, GridOption Model)
        {
            var status = ctxTFAT.ControlPara.Where(x => x.MemoryVar == "gpsalesman").Select(m => new {m.Valu}).ToList();
           
            foreach (var item in status)
            {
                if (item.Valu==1)
                {
                    Model.ViewDataId = "OrdersRep-Salesman wise";
                }
                else
                {
                    Model.ViewDataId = "OrdersRep";
                }
            }
            id = Model.ViewDataId;
            return mIlst.getGridDataColumns(id, "", "", "", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            var status = ctxTFAT.ControlPara.Where(x => x.MemoryVar == "gpsalesman").Select(m => new { m.Valu }).ToList();

            foreach (var item in status)
            {
                if (item.Valu == 1)
                {
                    Model.Code = "OrdersRep-Salesman wise";
                }
                else
                {
                    Model.Code = "OrdersRep";
                }
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
            if (Model.Value1 == "OE")
            {
                Model.Value2 = "And OrdersStk.pending <= 0";
            }
            if (Model.Value1 == "PO")
            {
                Model.Value2 = "And OrdersStk.pending > 0";
            }

            if (Model.Value3 == "ODW")
            {
                Model.Value3 = "and Orders.OrdDate";
            }
            if (Model.Value3 == "DDW")
            {
                Model.Value3 = "and Orders.DocDate";
            }

            if (Model.Item != null)
            {
                string list2 = Model.Item;
                string newList2 = string.Join(",", list2.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Item = "And ItemMaster.Code in (" + "" + newList2 + "" + @")";
            }
            if (Model.Party != null)
            {
                string list1 = Model.Party;
                string newList1 = string.Join(",", list1.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Party = "And Master.Code in (" + "" + newList1 + "" + @")";
            }

            if (Model.Salesman != null)
            {
                string list3 = Model.Party;
                string newList3 = string.Join(",", list3.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Salesman = "And Master.Code in (" + "" + newList3 + "" + @")";
            }
            if (Model.Area != null)
            {
                string list4 = Model.Party;
                string newList4 = string.Join(",", list4.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Area = "And Master.Code in (" + "" + newList4 + "" + @")";
            }
            if (Model.Type != null)
            {
                string list5 = Model.Party;
                string newList5 = string.Join(",", list5.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Type = "And Master.Code in (" + "" + newList5 + "" + @")";
            }


            Model.query = @"WITH PAGED_Stock  AS
                        (
                         SELECT " + Model.SelectContent + @",
                         ROW_NUMBER() OVER (ORDER BY " + "ItemMaster.Code" + @" " + Model.sord + @") AS RowNumber
                         FROM ItemMaster,Master,Orders,OrdersStk  WHERE OrdersStk.Type=Orders.Type 
                         AND OrdersStk.Prefix=Orders.Prefix AND OrdersStk.Srl=Orders.Srl 
                         AND ItemMaster.Code=OrdersStk.Code AND Master.Code=OrdersStk.Party 
                         " + Model.Value3 + @"  >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                         " + Model.Value3 + @"  <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103)
                         And Orders.Branch in (" + "'" + mbranchcode + "'" + @") And (Orders.OrdNumber <> '' And Orders.OrdNumber is Not Null) " + Model.Value2 + @" 
                         And OrdersStk.Branch in (" + "'" + mbranchcode + "'" + @")
                         AND Master.Branch in (" + "'" + mbranchcode + "'" + @") And Orders.Branch in (" + "'" + mbranchcode + "'" + @")
                         And Orders.MainType = (" + "'" + Model.MainType + "'" + @") 
                        " + Model.Item + @" " + Model.Party + @" " + Model.Salesman + @" " + Model.Area + @" " + Model.Type + @"
                        )
                        SELECT " + Model.HeaderContent + @"
                        FROM PAGED_Stock
                        WHERE RowNumber BETWEEN " + startIndex + @" AND " + endIndex + @";";

                Model.queryforcount = @"SELECT Count(*)                         
                        FROM ItemMaster,Master,Orders,OrdersStk  WHERE OrdersStk.Type=Orders.Type 
                         AND OrdersStk.Prefix=Orders.Prefix AND OrdersStk.Srl=Orders.Srl 
                         AND ItemMaster.Code=OrdersStk.Code AND Master.Code=OrdersStk.Party 
                         and Orders.DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                         and Orders.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103)
                         And Orders.Branch (" + "'" + mbranchcode + "'" + @") And (Orders.OrdNumber <> '' And Orders.OrdNumber is Not Null) " + Model.Value2 + @"
                         And OrdersStk.Branch in (" + "'" + mbranchcode + "'" + @")
                         AND Master.Branch in (" + "'" + mbranchcode + "'" + @") And Orders.Branch in (" + "'" + mbranchcode + "'" + @")
                         And Orders.MainType = (" + "'" + Model.MainType + "'" + @")
                         " + Model.Item + @" " + Model.Party + @" " + Model.Salesman + @" " + Model.Area + @" " + Model.Type + @" ";
          
            return Content(JQGridHelper.JsonForJqgrid(JQGridHelper.GetDataTable(Model), Model.rows, JQGridHelper.GetTotalCount(Model), Model.page), "application/json");
        }
    }
}