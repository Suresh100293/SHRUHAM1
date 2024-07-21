using Common;
using EntitiModel;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using ALT_ERP3.Controllers;
using ALT_ERP3.DynamicBusinessLayer;
using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Reports
{
    public class PurchaseSalesSchedulesController : BaseController
    {
        //nEntities context = new nEntities();
        tfatEntities ctxTFAT = new tfatEntities();
        private string BranchCode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private string CompCode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        private string FinPeriod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        //IReportGridOperation mIlst = new ListViewGridOperationreport();
        List<SelectListItem> Itemgrp = new List<SelectListItem>();
        List<SelectListItem> ItemName = new List<SelectListItem>();
        List<SelectListItem> ProductName = new List<SelectListItem>();
        List<SelectListItem> productsgrp = new List<SelectListItem>();
        List<SelectListItem> OrderName = new List<SelectListItem>();
        // GET: Reports/PurchaseSalesSchedules
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
            if (Model.MainType=="PR")
            {
                var result = ctxTFAT.Master.Where(x => x.BaseGr == "S" || x.BaseGr == "U" ).Select(m => new { m.Code, m.Name }).Distinct().ToList();
                foreach (var item in result)
                {
                    ProductName.Add(new SelectListItem { Text = item.Name, Value = item.Code });
                }
            }
            else
            {
                var result = ctxTFAT.Master.Where(x => x.BaseGr=="D" ||x.BaseGr=="U" ).Select(m => new { m.Code, m.Name }).Distinct().ToList();
                foreach (var item in result)
                {
                    ProductName.Add(new SelectListItem { Text = item.Name, Value = item.Code });
                }
            }
            return Json(ProductName, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetOrderName(GridOption Model)
        {
            var result = ctxTFAT.Orders.Where(x => x.MainType == Model.MainType && x.Branch == BranchCode).Select(m => new { m.Code, m.OrdNumber }).Distinct().ToList();
            foreach (var item in result)
            {
                OrderName.Add(new SelectListItem { Text = item.OrdNumber, Value = item.OrdNumber });
            }

            return Json(OrderName, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult GetGridStructureRecords(string id, GridOption Model)
        {
            Model.id = "MultiPartyOrder";
            id = Model.id;

            return mIlst.getGridDataColumns(id, "", "", "", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {

            Model.Code = "MultiPartyOrder";

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
            if (Model.Order != null)
            {
                string list1 = Model.Order;
                string newList2 = string.Join(",", list1.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Order = "And Orders.OrdNumber in (" + "" + newList2 + "" + @")";
            }
            if (Model.Value1 == "PEN")
            {
                Model.Value2 = "And OrdersStk.Pending > 0 ";
            }
            if (Model.Value1 == "EXE")
            {
                Model.Value2 = "And (OrdersStk.Pending<OrdersStk.Qty Or OrdersStk.Cancflag='Y')  ";
            }
            Model.query = @"WITH PAGED_Stock  AS
                        (
                         SELECT " + Model.SelectContent + @",
                        ROW_NUMBER() OVER (ORDER BY " + "OrdersStk.Code" + @" " + Model.sord + @") AS RowNumber
                        FROM (Orders LEFT JOIN (((OrdersStk LEFT JOIN ItemMaster ON OrdersStk.Code = itemmaster.Code) 
                        LEFT JOIN Master ON Master.Code = OrdersStk.Party) 
                        LEFT JOIN [Addon@D] ON ordersStk.Branch=[Addon@D].Branch And OrdersStk.Type=[Addon@D].Type 
                        And OrdersStk.Prefix=[Addon@D].Prefix And OrdersStk.Sno=[Addon@D].Sno And OrdersStk.Srl=[Addon@D].Srl) 
                        ON Orders.Type=OrdersStk.Type And Orders.Prefix=OrdersStk.Prefix And Orders.Srl=OrdersStk.Srl) 
                        Where Master.Branch in(" + "'" + BranchCode + "'" + @") " + Model.Value2 + @"
                        and Orders.DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                        and Orders.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103) 
                        And ordersStk.Branch in(" + "'" + BranchCode + "'" + @") And Orders.Branch in(" + "'" + BranchCode + "'" + @")
                        And Left(Orders.AUTHORISE,1) = 'A'  And Orders.MainType= " + "'" + Model.MainType + "'" + @"   And OrdersStk.CancFlag <> 'Y' 
                        " + Model.Item + @" " + Model.Party + @" " + Model.Order + @"
                        )
                        SELECT " + Model.HeaderContent + @"
                        FROM PAGED_Stock
                        WHERE RowNumber BETWEEN " + startIndex + @" AND " + endIndex + @";";

            Model.queryforcount = @"SELECT Count(*)                         
                        FROM (Orders LEFT JOIN (((OrdersStk LEFT JOIN ItemMaster ON OrdersStk.Code = itemmaster.Code) 
                        LEFT JOIN Master ON Master.Code = OrdersStk.Party) 
                        LEFT JOIN [Addon@D] ON ordersStk.Branch=[Addon@D].Branch And OrdersStk.Type=[Addon@D].Type 
                        And OrdersStk.Prefix=[Addon@D].Prefix And OrdersStk.Sno=[Addon@D].Sno And OrdersStk.Srl=[Addon@D].Srl) 
                        ON Orders.Type=OrdersStk.Type And Orders.Prefix=OrdersStk.Prefix And Orders.Srl=OrdersStk.Srl) 
                        Where Master.Branch in(" + "'" + BranchCode + "'" + @") " + Model.Value2 + @" 
                        and Orders.DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                        and Orders.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103) 
                        And ordersStk.Branch in(" + "'" + BranchCode + "'" + @") And Orders.Branch in(" + "'" + BranchCode + "'" + @")
                        And Left(Orders.AUTHORISE,1) = 'A'  And Orders.MainType=" + "'" + Model.MainType + "'" + @"   And OrdersStk.CancFlag <> 'Y' 
                        " + Model.Item + @" " + Model.Party + @" " + Model.Order + @")";

            return Content(JQGridHelper.JsonForJqgrid(JQGridHelper.GetDataTable(Model), Model.rows, JQGridHelper.GetTotalCount(Model), Model.page), "application/json");
        }

        public ActionResult GetExcel(GridOption Model)
        {
            Model.Code = "MultiPartyOrder";

            if (Model.MainType == "PR")
            {
                Model.Name = "PurchaseSchedule";
            }

            if (Model.MainType == "SL")
            {
                Model.Name = "SalesSchedule";
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

            for (int k = 0; k < SplitContent.Length; k++)
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
            if (Model.Order != null)
            {
                string list1 = Model.Order;
                string newList2 = string.Join(",", list1.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Order = "And Orders.OrdNumber in (" + "" + newList2 + "" + @")";
            }
            if (Model.Value1 == "PEN")
            {
                Model.Value2 = "And OrdersStk.Pending > 0 ";
            }
            if (Model.Value1 == "EXE")
            {
                Model.Value2 = "And (OrdersStk.Pending<OrdersStk.Qty Or OrdersStk.Cancflag='Y')  ";
            }
            Model.query = @"WITH PAGED_Stock  AS
                        (
                         SELECT " + Model.SelectContent + @",
                        ROW_NUMBER() OVER (ORDER BY " + "OrdersStk.Code" + @" " + Model.sord + @") AS RowNumber
                        FROM (Orders LEFT JOIN (((OrdersStk LEFT JOIN ItemMaster ON OrdersStk.Code = itemmaster.Code) 
                        LEFT JOIN Master ON Master.Code = OrdersStk.Party) 
                        LEFT JOIN [Addon@D] ON ordersStk.Branch=[Addon@D].Branch And OrdersStk.Type=[Addon@D].Type 
                        And OrdersStk.Prefix=[Addon@D].Prefix And OrdersStk.Sno=[Addon@D].Sno And OrdersStk.Srl=[Addon@D].Srl) 
                        ON Orders.Type=OrdersStk.Type And Orders.Prefix=OrdersStk.Prefix And Orders.Srl=OrdersStk.Srl) 
                        Where Master.Branch in(" + "'" + BranchCode + "'" + @") " + Model.Value2 + @"
                        and Orders.DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                        and Orders.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103) 
                        And ordersStk.Branch in(" + "'" + BranchCode + "'" + @") And Orders.Branch in(" + "'" + BranchCode + "'" + @")
                        And Left(Orders.AUTHORISE,1) = 'A'  And Orders.MainType= " + "'" + Model.MainType + "'" + @"   And OrdersStk.CancFlag <> 'Y' 
                        " + Model.Item + @" " + Model.Party + @" " + Model.Order + @"
                        )
                        SELECT " + Model.HeaderContent + @"
                        FROM PAGED_Stock
                        ";

            DataTable datatab = new DataTable();
            datatab = JQGridHelper.GetDataTable(Model);//your datatable
            string attachment = "attachment; filename=" + Model.Name + @".xls";
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
            Model.Code = "MultiPartyOrder";
            if (Model.MainType == "PR")
            {
                Model.Name = "PurchaseSchedule";
            }
            if (Model.MainType == "SL")
            {
                Model.Name = "SalesSchedule";
            }
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
            if (Model.Order != null)
            {
                string list1 = Model.Order;
                string newList2 = string.Join(",", list1.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Order = "And Orders.OrdNumber in (" + "" + newList2 + "" + @")";
            }
            if (Model.Value1 == "PEN")
            {
                Model.Value2 = "And OrdersStk.Pending > 0 ";
            }
            if (Model.Value1 == "EXE")
            {
                Model.Value2 = "And (OrdersStk.Pending<OrdersStk.Qty Or OrdersStk.Cancflag='Y')  ";
            }
            Model.query = @"WITH PAGED_Stock  AS
                        (
                         SELECT " + Model.SelectContent + @",
                        ROW_NUMBER() OVER (ORDER BY " + "OrdersStk.Code" + @" " + Model.sord + @") AS RowNumber
                        FROM (Orders LEFT JOIN (((OrdersStk LEFT JOIN ItemMaster ON OrdersStk.Code = itemmaster.Code) 
                        LEFT JOIN Master ON Master.Code = OrdersStk.Party) 
                        LEFT JOIN [Addon@D] ON ordersStk.Branch=[Addon@D].Branch And OrdersStk.Type=[Addon@D].Type 
                        And OrdersStk.Prefix=[Addon@D].Prefix And OrdersStk.Sno=[Addon@D].Sno And OrdersStk.Srl=[Addon@D].Srl) 
                        ON Orders.Type=OrdersStk.Type And Orders.Prefix=OrdersStk.Prefix And Orders.Srl=OrdersStk.Srl) 
                        Where Master.Branch in(" + "'" + BranchCode + "'" + @") " + Model.Value2 + @"
                        and Orders.DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                        and Orders.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103) 
                        And ordersStk.Branch in(" + "'" + BranchCode + "'" + @") And Orders.Branch in(" + "'" + BranchCode + "'" + @")
                        And Left(Orders.AUTHORISE,1) = 'A'  And Orders.MainType= " + "'" + Model.MainType + "'" + @"   And OrdersStk.CancFlag <> 'Y' 
                        " + Model.Item + @" " + Model.Party + @" " + Model.Order + @"
                        )
                        SELECT " + Model.HeaderContent + @"
                        FROM PAGED_Stock
                        ";
            DataTable datatab = new DataTable();
            datatab = JQGridHelper.GetDataTable(Model);//your datatable

            GridView GridView1 = new GridView();
            GridView1.AllowPaging = false;
            GridView1.DataSource = datatab;
            GridView1.DataBind();

            Response.ContentType = "application /pdf";
            Response.AddHeader("content-disposition",
            "attachment;filename=" + Model.Name + @".pdf");
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