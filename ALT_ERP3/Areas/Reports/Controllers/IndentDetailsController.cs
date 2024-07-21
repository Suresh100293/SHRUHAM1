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

namespace ALT_ERP3.Areas.Reports.Controllers
{
    public class IndentDetailsController : BaseController
    {
        //nEntities context = new nEntities();
        tfatEntities ctxTFAT = new tfatEntities();
        private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private string CompCode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        private string FinPeriod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        //IReportGridOperation mIlst = new ListViewGridOperationreport();
        List<SelectListItem> ItemName = new List<SelectListItem>();
        List<SelectListItem> ItemNumber = new List<SelectListItem>();
        List<SelectListItem> StoreName = new List<SelectListItem>();
        // GET: Reports/IndentDetails
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
            Model.ViewDataId = "IndentRep";
            return View(Model);
        }

        public ActionResult GetItemList(string term)
        {
            if (term == "")
            {
                var result = ctxTFAT.ItemMaster.Where(x => x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).Take(10).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.ItemMaster.Where(x => x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetStoreName(GridOption Model)
        {
            var result = ctxTFAT.Stores.Where(x => x.Flag == "L").Select(c => new { c.Code, c.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                StoreName.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }

            return Json(StoreName, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetIndentNumber(GridOption Model)
        {
            var result = ctxTFAT.IndentStk.Select(m => new { m.Srl }).Distinct().ToList();
            foreach (var item in result)
            {
                ItemNumber.Add(new SelectListItem { Text = item.Srl, Value = item.Srl });
            }

            return Json(ItemNumber, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id, GridOption Model)
        {
            Model.ViewDataId = id;
            return mIlst.getGridDataColumns(id, "", "", "", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            Model.Code = Model.ViewDataId;
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
            if (Model.Value1 == "PR")
            {
                Model.Value2 = " and IndentStk.Pending<> IndentStk.Qty";
            }
            if (Model.Value1 == "PE")
            {
                Model.Value2 = " and IndentStk.Pending<> 0";
            }
            if (Model.IndentNumber != "0")
            {
                Model.Value3 = "and IndentStk.IndNumber = " + "'" + Model.IndentNumber + "'" + @"";
            }
            if (Model.Item != null)
            {
                string list = Model.Item;
                string newList = string.Join(",", list.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Item = "And IndentStk.Code in (" + "" + newList + "" + @")";
            }
            if (Model.Store != null)
            {
                string list1 = Model.Store;
                string newList1 = string.Join(",", list1.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Store = "And IndentStk.Store in (" + "" + newList1 + "" + @")";
            }
            if (Model.Value1=="RE")
            {
            }
            Model.query = @"WITH PAGED_Stock  AS
                        (
                         SELECT " + Model.SelectContent + @",
                         ROW_NUMBER() OVER (ORDER BY " + "IndentStk.Type,IndentStk.Prefix,IndentStk.Srl,IndentStk.Sno,IndentStk.Docdate" + @" " + Model.sord + @") AS RowNumber
                         FROM IndentStk, ItemMaster, Stores, Stores Stores1 WHERE IndentStk.Code=ItemMaster.Code AND
                         IndentStk.Store=Stores.Code AND IndentStk.Branch=Stores.Branch AND IndentStk.IndTo= Stores1.Code AND
                         IndentStk.Branch=Stores1.Branch AND IndentStk.Branch in (" + "'" + mbranchcode + "'" + @") And Left(IndentStk.AUTHORISE,1) = 'A'
                         " + Model.Value2 + @" " + Model.Value3 + @" " + Model.Store + @" " + Model.Item + @"
                         and  IndentStk.DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                         and IndentStk.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103)
                        )
                        SELECT " + Model.HeaderContent + @"
                        FROM PAGED_Stock
                        WHERE RowNumber BETWEEN " + startIndex + @" AND " + endIndex + @";";

            Model.queryforcount = @"SELECT Count(*)                         
                         FROM IndentStk, ItemMaster, Stores, Stores Stores1 WHERE IndentStk.Code=ItemMaster.Code AND
                         IndentStk.Store=Stores.Code AND IndentStk.Branch=Stores.Branch AND IndentStk.IndTo= Stores1.Code AND
                         IndentStk.Branch=Stores1.Branch AND IndentStk.Branch in (" + "'" + mbranchcode + "'" + @")   And Left(IndentStk.AUTHORISE,1) = 'A'
                         and  IndentStk.DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                         and IndentStk.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103)
                         and " + Model.Value2 + @" " + Model.Value3 + @" " + Model.Store + @" " + Model.Item + @"
                        )";
            return Content(JQGridHelper.JsonForJqgrid(JQGridHelper.GetDataTable(Model), Model.rows, JQGridHelper.GetTotalCount(Model), Model.page), "application/json");
        }


        public ActionResult GetExcel(GridOption Model)
        {
            if (Model.Value1 == "ER" || Model.Value1 == "QNS")
            {
                Model.Code = "EnquiryRep";

            }
            if (Model.Value1 == "ES" || Model.Value1 == "QNR")
            {
                Model.Code = "EnquiryRepPur";

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
            if (Model.Value1 == "PR")
            {
                Model.Value2 = " and IndentStk.Pending<> IndentStk.Qty";
            }
            if (Model.Value1 == "PE")
            {
                Model.Value2 = " and IndentStk.Pending<> 0";
            }
            if (Model.IndentNumber != "0")
            {
                Model.Value3 = "and IndentStk.IndNumber = " + "'" + Model.IndentNumber + "'" + @"";
            }
            if (Model.Item != null)
            {
                string list = Model.Item;
                string newList = string.Join(",", list.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Item = "And IndentStk.Code in (" + "" + newList + "" + @")";
            }
            if (Model.Store != null)
            {
                string list1 = Model.Store;
                string newList1 = string.Join(",", list1.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Store = "And IndentStk.Store in (" + "" + newList1 + "" + @")";
            }
            Model.query = @"WITH PAGED_Stock  AS
                        (
                         SELECT " + Model.SelectContent + @",
                         ROW_NUMBER() OVER (ORDER BY " + "IndentStk.Type,IndentStk.Prefix,IndentStk.Srl,IndentStk.Sno,IndentStk.Docdate" + @" " + Model.sord + @") AS RowNumber
                         FROM IndentStk, ItemMaster, Stores, Stores Stores1 WHERE IndentStk.Code=ItemMaster.Code AND
                         IndentStk.Store=Stores.Code AND IndentStk.Branch=Stores.Branch AND IndentStk.IndTo= Stores1.Code AND
                         IndentStk.Branch=Stores1.Branch AND IndentStk.Branch in (" + "'" + mbranchcode + "'" + @") And Left(IndentStk.AUTHORISE,1) = 'A'
                         " + Model.Value2 + @" " + Model.Value3 + @" " + Model.Store + @" " + Model.Item + @"
                         and  IndentStk.DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                         and IndentStk.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103)
                        )
                        SELECT " + Model.HeaderContent + @"
                        FROM PAGED_Stock
                        ";

            DataTable datatab = new DataTable();
            datatab = JQGridHelper.GetDataTable(Model);//your datatable
            string attachment = "attachment; filename=" + Model.Code + @".xls";
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
            if (Model.Value1 == "ER" || Model.Value1 == "QNS")
            {
                Model.Code = "EnquiryRep";

            }
            if (Model.Value1 == "ES" || Model.Value1 == "QNR")
            {
                Model.Code = "EnquiryRepPur";

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

            if (Model.Value1 == "PR")
            {
                Model.Value2 = " and IndentStk.Pending<> IndentStk.Qty";
            }
            if (Model.Value1 == "PE")
            {
                Model.Value2 = " and IndentStk.Pending<> 0";
            }
            if (Model.IndentNumber != "0")
            {
                Model.Value3 = "and IndentStk.IndNumber = " + "'" + Model.IndentNumber + "'" + @"";
            }
            if (Model.Item != null)
            {
                string list = Model.Item;
                string newList = string.Join(",", list.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Item = "And IndentStk.Code in (" + "" + newList + "" + @")";
            }
            if (Model.Store != null)
            {
                string list1 = Model.Store;
                string newList1 = string.Join(",", list1.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Store = "And IndentStk.Store in (" + "" + newList1 + "" + @")";
            }
            Model.query = @"WITH PAGED_Stock  AS
                        (
                         SELECT " + Model.SelectContent + @",
                         ROW_NUMBER() OVER (ORDER BY " + "IndentStk.Type,IndentStk.Prefix,IndentStk.Srl,IndentStk.Sno,IndentStk.Docdate" + @" " + Model.sord + @") AS RowNumber
                         FROM IndentStk, ItemMaster, Stores, Stores Stores1 WHERE IndentStk.Code=ItemMaster.Code AND
                         IndentStk.Store=Stores.Code AND IndentStk.Branch=Stores.Branch AND IndentStk.IndTo= Stores1.Code AND
                         IndentStk.Branch=Stores1.Branch AND IndentStk.Branch in (" + "'" + mbranchcode + "'" + @") And Left(IndentStk.AUTHORISE,1) = 'A'
                         " + Model.Value2 + @" " + Model.Value3 + @" " + Model.Store + @" " + Model.Item + @"
                         and  IndentStk.DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                         and IndentStk.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103)
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
            "attachment;filename=" + Model.Code + @".pdf");
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