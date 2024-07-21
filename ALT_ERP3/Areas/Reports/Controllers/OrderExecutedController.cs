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
    public class OrderExecutedController : BaseController
    {
        //nEntities context = new nEntities();
        tfatEntities ctxTFAT = new tfatEntities();
        private string BranchCode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private string CompCode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        private string FinPeriod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        //IReportGridOperation mIlst = new ListViewGridOperationreport();
        List<SelectListItem> PartyName = new List<SelectListItem>();
        List<SelectListItem> OrderName = new List<SelectListItem>();
        // GET: Reports/OrderExecuted
        public ActionResult Index(GridOption Model)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            if (FinPeriod != "null")
            {
                var perd = ctxTFAT.TfatPerd.Where(x => x.Code == CompCode && x.PerdCode == FinPeriod).Select(b => new { b.StartDate, b.LastDate }).FirstOrDefault();
                Model.FromDate = JQGridHelper.ConvertDate(perd.StartDate.ToShortDateString()).Replace("-", "/");
                Model.ToDate = JQGridHelper.ConvertDate(perd.LastDate.ToShortDateString()).Replace("-", "/");

            }
            return View(Model);
        }

        public ActionResult GetPartyName(GridOption Model)
        {
            if (Model.MainType == "PR")
            {
                var result = ctxTFAT.Master.Where(x =>  x.BaseGr == "S" || x.BaseGr == "U" ).Select(m => new { m.Code, m.Name }).Distinct().ToList();
                foreach (var item in result)
                {
                    PartyName.Add(new SelectListItem { Text = item.Name, Value = item.Code });
                }
            }
            else
            {
                var result = ctxTFAT.Master.Where(x => x.BaseGr == "D" || x.BaseGr == "U" ).Select(m => new { m.Code, m.Name }).Distinct().ToList();
                foreach (var item in result)
                {
                    PartyName.Add(new SelectListItem { Text = item.Name, Value = item.Code });
                }
            }
            return Json(PartyName, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetOrderName(GridOption Model)
        {
            var result = ctxTFAT.Orders.Where(x => x.Code == Model.Party && x.Branch == BranchCode).Select(m => new { m.Code, m.OrdNumber }).Distinct().ToList();
            foreach (var item in result)
            {
                OrderName.Add(new SelectListItem { Text = item.OrdNumber, Value = item.OrdNumber });
            }

            return Json(OrderName, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id, GridOption Model)
        {
            Model.id = "OrderInvoice";
            id = Model.id;

            return mIlst.getGridDataColumns(id, "", "", "", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {

            Model.Code = "OrderInvoice";

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
            if (Model.Party != null)
            {
                string list1 = Model.Party;
                string newList1 = string.Join(",", list1.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Party = "And Master.Code in (" + "" + newList1 + "" + @")";
            }
            if (Model.Order != null)
            {
                string list1 = Model.Order;
                string newList2 = string.Join(",", list1.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Order = "And Orders.OrdNumber in (" + "" + newList2 + "" + @")";
            }
            if (Model.Value1 == "SL")
            {
                Model.Value2 = "And Stock.MainType In('SL','IV')";
            }
            if (Model.Value1 == "PR")
            {
                Model.Value2 = "And Stock.MainType In('PR','IV')";
            }
            Model.query = @"WITH PAGED_Stock  AS
                        (
                         SELECT " + Model.SelectContent + @",
                        ROW_NUMBER() OVER (ORDER BY " + "Master.Code" + @" " + Model.sord + @") AS RowNumber
                         From Stock,ItemMaster, Master Master 
                         Where Master.Code=Stock.Party  " + Model.Party + @" 
                         And ItemMaster.Code=Stock.Code  " + Model.Value2 + @"  
                         and Master.Branch in (" + "'" + BranchCode + "'" + @")
                         and DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                         and DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103) 
                         and Stock.Branch in (" + "'" + BranchCode + "'" + @")
                         And Left(Stock.AUTHORISE,1) = 'A' And Stock.OrdNumber<>'' 
                        )
                        SELECT " + Model.HeaderContent + @"
                        FROM PAGED_Stock
                        WHERE RowNumber BETWEEN " + startIndex + @" AND " + endIndex + @";";

            Model.queryforcount = @"SELECT Count(*)                         
                        From Stock,ItemMaster, Master Master 
                         Where Master.Code=Stock.Party  " + Model.Party + @" 
                         And ItemMaster.Code=Stock.Code  " + Model.Value2 + @" 
                         and Master.Branch in (" + "'" + BranchCode + "'" + @")
                         and DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                         and DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103) 
                         and Stock.Branch in (" + "'" + BranchCode + "'" + @")
                         And Left(Stock.AUTHORISE,1) = 'A' And Stock.OrdNumber<>'')";

            return Content(JQGridHelper.JsonForJqgrid(JQGridHelper.GetDataTable(Model), Model.rows, JQGridHelper.GetTotalCount(Model), Model.page), "application/json");
        }
    }
}