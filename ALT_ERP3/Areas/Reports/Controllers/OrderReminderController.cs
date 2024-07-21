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
    public class OrderReminderController : BaseController
    {
        //nEntities context = new nEntities();
        tfatEntities ctxTFAT = new tfatEntities();
        private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private string CompCode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        private string FinPeriod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        //IReportGridOperation mIlst = new ListViewGridOperationreport();
        List<SelectListItem> PartyName = new List<SelectListItem>();
        List<SelectListItem> ItemName = new List<SelectListItem>();
        List<SelectListItem> OrderName = new List<SelectListItem>();
        // GET: Reports/OrderReminder
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

        public ActionResult GetItemName(GridOption Model)
        {
            var result = ctxTFAT.ItemMaster.Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                ItemName.Add(new SelectListItem { Text = item.Name, Value = item.Code });
            }

            return Json(ItemName, JsonRequestBehavior.AllowGet);
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


        [HttpPost]
        public ActionResult GetGridStructureRecords(string id, GridOption Model)
        {

            Model.ViewDataId = "OrderReminder";
            id = Model.ViewDataId;

            return mIlst.getGridDataColumns(id, "", "", "", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {

            Model.Code = "OrderReminder";

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
                         FROM Orders,OrdersStk,Master,ItemMaster 
                         WHERE Orders.MainType= " + Model.MainType + @" AND Orders.SubType='OP' 
                         AND Orders.Type+Orders.Prefix+Orders.Srl=OrdersStk.Type+OrdersStk.Prefix+OrdersStk.Srl 
                         AND Orders.Code=OrdersStk.Party AND Orders.Branch=OrdersStk.Branch 
                         AND Orders.Code=Master.Code AND Master.Branch=Orders.Branch 
                         AND ItemMaster.Code=OrdersStk.Code  
                         AND Orders.Docdate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                         and Orders.Docdate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103)
                         And Orders.Branch in (" + "'" + mbranchcode + "'" + @") And Left(Orders.AUTHORISE,1) = 'A'
                         " + Model.Item + @" " + Model.Party + @"
                        )
                        SELECT " + Model.HeaderContent + @"
                        FROM PAGED_Stock
                        WHERE RowNumber BETWEEN " + startIndex + @" AND " + endIndex + @";";

            Model.queryforcount = @"SELECT Count(*)                         
                         FROM Orders,OrdersStk,Master,ItemMaster 
                         WHERE Orders.MainType='PR' AND Orders.SubType='OP' 
                         AND Orders.Type+Orders.Prefix+Orders.Srl=OrdersStk.Type+OrdersStk.Prefix+OrdersStk.Srl 
                         AND Orders.Code=OrdersStk.Party AND Orders.Branch=OrdersStk.Branch 
                         AND Orders.Code=Master.Code AND Master.Branch=Orders.Branch 
                         AND ItemMaster.Code=OrdersStk.Code  
                         AND Orders.Docdate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                         and Orders.Docdate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103)
                         And Orders.Branch in (" + "'" + mbranchcode + "'" + @") And Left(Orders.AUTHORISE,1) = 'A'
                         " + Model.Item + @" " + Model.Party + @"
                         ";


            return Content(JQGridHelper.JsonForJqgrid(JQGridHelper.GetDataTable(Model), Model.rows, JQGridHelper.GetTotalCount(Model), Model.page), "application/json");
        }
    }
}