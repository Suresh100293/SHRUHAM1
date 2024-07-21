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
    public class QuotationsDetailsController : BaseController
    {
        //nEntities context = new nEntities();
        tfatEntities ctxTFAT = new tfatEntities();
        private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private string CompCode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        private string FinPeriod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        //IReportGridOperation mIlst = new ListViewGridOperationreport();
        List<SelectListItem> ItemName = new List<SelectListItem>();
        List<SelectListItem> ProductName = new List<SelectListItem>();
        List<SelectListItem> SalesManName = new List<SelectListItem>();
        // GET: Reports/QuotationsDetails
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
                ProductName.Add(new SelectListItem { Text = item.Name, Value = item.Code });
            }

            return Json(ProductName, JsonRequestBehavior.AllowGet);
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

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id, GridOption Model)
        {
            if (Model.Value1 == "QS" || Model.Value1 == "ONR")
            {
                Model.ViewDataId = "QuotationSalesmanWise";
                id = Model.ViewDataId;
            }
            if (Model.Value1 == "QR" || Model.Value1 == "ONP")
            {
                Model.ViewDataId = "QuotationRep";
                id = Model.ViewDataId;
            }
            return mIlst.getGridDataColumns(id, "", "", "", "");
        }


        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            if (Model.Value1 == "QS" || Model.Value1 == "ONR")
            {
                Model.Code = "QuotationSalesmanWise";

            }
            if (Model.Value1 == "QR" || Model.Value1 == "ONP")
            {
                Model.Code = "QuotationRep";

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
            if (Model.Party != null)
            {
                string list1 = Model.Party;
                string newList1 = string.Join(",", list1.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Party = "And QuoteStk.Party in (" + "" + newList1 + "" + @")";
            }
            if (Model.Salesman != null)
            {
                string list1 = Model.Salesman;
                string newList2 = string.Join(",", list1.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Salesman = "And Salesman.Code in (" + "" + newList2 + "" + @")";
            }

            if (Model.Value1 == "ONP")
            {
                Model.Value2 = "And QuoteStk.Type+QuoteStk.Prefix+QuoteStk.Sno+QuoteStk.Srl NOT IN (SELECT QtnNumber FROM OrdersStk WHERE Branch='HO0000' AND Subtype='OP')";
            }
            if (Model.Value1 == "ONR")
            {
                Model.Value2 = "And QuoteStk.Type+QuoteStk.Prefix+QuoteStk.Sno+QuoteStk.Srl NOT IN (SELECT QtnNumber FROM OrdersStk WHERE Branch='HO0000' AND Subtype='OS')";
            }
            if (Model.Value1 == "QR" || Model.Value1 == "ONP")
            {
                Model.query = @"WITH PAGED_Stock  AS
                        (
                         SELECT " + Model.SelectContent + @",
                         ROW_NUMBER() OVER (ORDER BY " + "QuoteStk.DocDate,QuoteStk.Prefix,QuoteStk.Srl,QuoteStk.Sno" + @" " + Model.sord + @") AS RowNumber
                         FROM ItemMaster, Master, Quote, QuoteStk  where QuoteStk.MainType  = Quote.MainType and 
                         QuoteStk.SubType  = Quote.SubType and QuoteStk.Type  = Quote.Type and QuoteStk.Prefix  = Quote.Prefix
                         and QuoteStk.srl = Quote.srl and  ItemMaster.Code  = QuoteStk.Code and Master.Code  = QuoteStk.Party 
                         and Quote.DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                         and Quote.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103) and QuoteStk.SubType = 'QP'     
                         And QuoteStk.Branch in(" + "'" + mbranchcode + "'" + @")  And Quote.Branch in(" + "'" + mbranchcode + "'" + @") And Left(Quote.AUTHORISE,1) = 'A'  
                         And Master.Branch in(" + "'" + mbranchcode + "'" + @") " + Model.Item + @" " + Model.Party + @" " + Model.Salesman + @" " + Model.Value2 + @"
                        )
                        SELECT " + Model.HeaderContent + @"
                        FROM PAGED_Stock
                        WHERE RowNumber BETWEEN " + startIndex + @" AND " + endIndex + @";";

                Model.queryforcount = @"SELECT Count(*)                         
                         FROM ItemMaster, Master, Quote, QuoteStk  where QuoteStk.MainType  = Quote.MainType and 
                         QuoteStk.SubType  = Quote.SubType and QuoteStk.Type  = Quote.Type and QuoteStk.Prefix  = Quote.Prefix
                         and QuoteStk.srl = Quote.srl and  ItemMaster.Code  = QuoteStk.Code and Master.Code  = QuoteStk.Party 
                         and Quote.DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                         and Quote.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103) and QuoteStk.SubType = 'QP'     
                         And QuoteStk.Branch in(" + "'" + mbranchcode + "'" + @")  And Quote.Branch in(" + "'" + mbranchcode + "'" + @") And Left(Quote.AUTHORISE,1) = 'A'  
                         And Master.Branch in(" + "'" + mbranchcode + "'" + @") " + Model.Item + @" " + Model.Party + @" " + Model.Salesman + @" " + Model.Value2 + @")";
            }
            if (Model.Value1 == "QS" || Model.Value1 == "ONR")
            {
                Model.query = @"WITH PAGED_Stock  AS
                        (
                         SELECT " + Model.SelectContent + @",
                        ROW_NUMBER() OVER (ORDER BY " + " QuoteStk.DocDate,QuoteStk.Prefix,QuoteStk.Srl,QuoteStk.Sno" + @" " + Model.sord + @") AS RowNumber
                        FROM ItemMaster, Master, Salesman, Quote, QuoteStk  
                        where QuoteStk.MainType=Quote.MainType and QuoteStk.SubType=Quote.SubType  and QuoteStk.Type=Quote.Type and 
                        QuoteStk.Prefix=Quote.Prefix and QuoteStk.srl=Quote.srl and  ItemMaster.Code= QuoteStk.Code and 
                        Master.Code  = QuoteStk.Party 
                        and Quote.DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                        and Quote.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103) 
                        And Quote.Salesman=Salesman.Code  and QuoteStk.SubType = 'QS'      
                        And QuoteStk.Branch in(" + "'" + mbranchcode + "'" + @")  And Quote.Branch in(" + "'" + mbranchcode + "'" + @") And Left(Quote.AUTHORISE,1) = 'A'  
                        And Master.Branch in(" + "'" + mbranchcode + "'" + @") And Salesman.Locked=0 And Salesman.Branch in(" + "'" + mbranchcode + "'" + @")
                        " + Model.Item + @" " + Model.Party + @" " + Model.Salesman + @" " + Model.Value2 + @"
                        )
                        SELECT " + Model.HeaderContent + @"
                        FROM PAGED_Stock
                        WHERE RowNumber BETWEEN " + startIndex + @" AND " + endIndex + @";";

                Model.queryforcount = @"SELECT Count(*)                         
                        FROM ItemMaster, Master, Salesman, Quote, QuoteStk  
                        where QuoteStk.MainType=Quote.MainType and QuoteStk.SubType=Quote.SubType  and QuoteStk.Type=Quote.Type and 
                        QuoteStk.Prefix=Quote.Prefix and QuoteStk.srl=Quote.srl and  ItemMaster.Code= QuoteStk.Code and 
                        Master.Code  = QuoteStk.Party 
                        and Quote.DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                        and Quote.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103) 
                        And Quote.Salesman=Salesman.Code  and QuoteStk.SubType = 'QS'      
                        And QuoteStk.Branch in(" + "'" + mbranchcode + "'" + @")  And Quote.Branch in(" + "'" + mbranchcode + "'" + @") And Left(Quote.AUTHORISE,1) = 'A'  
                        And Master.Branch in(" + "'" + mbranchcode + "'" + @") And Salesman.Locked=0 And Salesman.Branch in(" + "'" + mbranchcode + "'" + @")
                        " + Model.Item + @" " + Model.Party + @" " + Model.Salesman + @" " + Model.Value2 + @")";
            }

            return Content(JQGridHelper.JsonForJqgrid(JQGridHelper.GetDataTable(Model), Model.rows, JQGridHelper.GetTotalCount(Model), Model.page), "application/json");
        }

        public ActionResult GetExcel(GridOption Model)
        {
            if (Model.Value1 == "QS" || Model.Value1 == "ONR")
            {
                Model.Code = "QuotationSalesmanWise";

            }
            if (Model.Value1 == "QR" || Model.Value1 == "ONP")
            {
                Model.Code = "QuotationRep";

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
                Model.Item = "And QuoteStk.Code in (" + "" + newList + "" + @")";
            }
            if (Model.Party != null)
            {
                string list1 = Model.Party;
                string newList1 = string.Join(",", list1.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Party = "And QuoteStk.Party in (" + "" + newList1 + "" + @")";
            }
            if (Model.Salesman != null)
            {
                string list1 = Model.Salesman;
                string newList2 = string.Join(",", list1.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Salesman = "And Salesman.Code in (" + "" + newList2 + "" + @")";
            }

            if (Model.Value1 == "ONP")
            {
                Model.Value2 = "And QuoteStk.Type+QuoteStk.Prefix+QuoteStk.Sno+QuoteStk.Srl NOT IN (SELECT QtnNumber FROM OrdersStk WHERE Branch='HO0000' AND Subtype='OP')";
            }
            if (Model.Value1 == "ONR")
            {
                Model.Value2 = "And QuoteStk.Type+QuoteStk.Prefix+QuoteStk.Sno+QuoteStk.Srl NOT IN (SELECT QtnNumber FROM OrdersStk WHERE Branch='HO0000' AND Subtype='OS')";
            }
            if (Model.Value1 == "QR" || Model.Value1 == "ONP")
            {
                Model.query = @"WITH PAGED_Stock  AS
                        (
                         SELECT " + Model.SelectContent + @",
                         ROW_NUMBER() OVER (ORDER BY " + "QuoteStk.DocDate,QuoteStk.Prefix,QuoteStk.Srl,QuoteStk.Sno" + @" " + Model.sord + @") AS RowNumber
                         FROM ItemMaster, Master, Quote, QuoteStk  where QuoteStk.MainType  = Quote.MainType and 
                         QuoteStk.SubType  = Quote.SubType and QuoteStk.Type  = Quote.Type and QuoteStk.Prefix  = Quote.Prefix
                         and QuoteStk.srl = Quote.srl and  ItemMaster.Code  = QuoteStk.Code and Master.Code  = QuoteStk.Party 
                         and Quote.DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                         and Quote.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103) and QuoteStk.SubType = 'QP'     
                         And QuoteStk.Branch in(" + "'" + mbranchcode + "'" + @")  And Quote.Branch in(" + "'" + mbranchcode + "'" + @") And Left(Quote.AUTHORISE,1) = 'A'  
                         And Master.Branch in(" + "'" + mbranchcode + "'" + @") " + Model.Item + @" " + Model.Party + @" " + Model.Salesman + @" " + Model.Value2 + @"
                        )
                        SELECT " + Model.HeaderContent + @"
                        FROM PAGED_Stock
                        ";
            }
            if (Model.Value1 == "QS" || Model.Value1 == "ONR")
            {
                Model.query = @"WITH PAGED_Stock  AS
                        (
                         SELECT " + Model.SelectContent + @",
                        ROW_NUMBER() OVER (ORDER BY " + " QuoteStk.DocDate,QuoteStk.Prefix,QuoteStk.Srl,QuoteStk.Sno" + @" " + Model.sord + @") AS RowNumber
                        FROM ItemMaster, Master, Salesman, Quote, QuoteStk  
                        where QuoteStk.MainType=Quote.MainType and QuoteStk.SubType=Quote.SubType  and QuoteStk.Type=Quote.Type and 
                        QuoteStk.Prefix=Quote.Prefix and QuoteStk.srl=Quote.srl and  ItemMaster.Code= QuoteStk.Code and 
                        Master.Code  = QuoteStk.Party 
                        and Quote.DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                        and Quote.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103) 
                        And Quote.Salesman=Salesman.Code  and QuoteStk.SubType = 'QS'      
                        And QuoteStk.Branch in(" + "'" + mbranchcode + "'" + @")  And Quote.Branch in(" + "'" + mbranchcode + "'" + @") And Left(Quote.AUTHORISE,1) = 'A'  
                        And Master.Branch in(" + "'" + mbranchcode + "'" + @") And Salesman.Locked=0 And Salesman.Branch in(" + "'" + mbranchcode + "'" + @")
                        " + Model.Item + @" " + Model.Party + @" " + Model.Salesman + @" " + Model.Value2 + @"
                        )
                        SELECT " + Model.HeaderContent + @"
                        FROM PAGED_Stock
                        ";
            }


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
            if (Model.Value1 == "QS" || Model.Value1 == "ONR")
            {
                Model.Code = "QuotationSalesmanWise";

            }
            if (Model.Value1 == "QR" || Model.Value1 == "ONP")
            {
                Model.Code = "QuotationRep";

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
                Model.Item = "And QuoteStk.Code in (" + "" + newList + "" + @")";
            }
            if (Model.Party != null)
            {
                string list1 = Model.Party;
                string newList1 = string.Join(",", list1.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Party = "And QuoteStk.Party in (" + "" + newList1 + "" + @")";
            }
            if (Model.Salesman != null)
            {
                string list1 = Model.Salesman;
                string newList2 = string.Join(",", list1.Split(',').Select(x => string.Format("'{0}'", x)).ToList());
                Model.Salesman = "And Salesman.Code in (" + "" + newList2 + "" + @")";
            }

            if (Model.Value1 == "ONP")
            {
                Model.Value2 = "And QuoteStk.Type+QuoteStk.Prefix+QuoteStk.Sno+QuoteStk.Srl NOT IN (SELECT QtnNumber FROM OrdersStk WHERE Branch='HO0000' AND Subtype='OP')";
            }
            if (Model.Value1 == "ONR")
            {
                Model.Value2 = "And QuoteStk.Type+QuoteStk.Prefix+QuoteStk.Sno+QuoteStk.Srl NOT IN (SELECT QtnNumber FROM OrdersStk WHERE Branch='HO0000' AND Subtype='OS')";
            }
            if (Model.Value1 == "QR" || Model.Value1 == "ONP")
            {
                Model.query = @"WITH PAGED_Stock  AS
                        (
                         SELECT " + Model.SelectContent + @",
                         ROW_NUMBER() OVER (ORDER BY " + "QuoteStk.DocDate,QuoteStk.Prefix,QuoteStk.Srl,QuoteStk.Sno" + @" " + Model.sord + @") AS RowNumber
                         FROM ItemMaster, Master, Quote, QuoteStk  where QuoteStk.MainType  = Quote.MainType and 
                         QuoteStk.SubType  = Quote.SubType and QuoteStk.Type  = Quote.Type and QuoteStk.Prefix  = Quote.Prefix
                         and QuoteStk.srl = Quote.srl and  ItemMaster.Code  = QuoteStk.Code and Master.Code  = QuoteStk.Party 
                         and Quote.DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                         and Quote.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103) and QuoteStk.SubType = 'QP'     
                         And QuoteStk.Branch in(" + "'" + mbranchcode + "'" + @")  And Quote.Branch in(" + "'" + mbranchcode + "'" + @") And Left(Quote.AUTHORISE,1) = 'A'  
                         And Master.Branch in(" + "'" + mbranchcode + "'" + @") " + Model.Item + @" " + Model.Party + @" " + Model.Salesman + @" " + Model.Value2 + @"
                        )
                        SELECT " + Model.HeaderContent + @"
                        FROM PAGED_Stock
                        ";
            }
            if (Model.Value1 == "QS" || Model.Value1 == "ONR")
            {
                Model.query = @"WITH PAGED_Stock  AS
                        (
                         SELECT " + Model.SelectContent + @",
                        ROW_NUMBER() OVER (ORDER BY " + " QuoteStk.DocDate,QuoteStk.Prefix,QuoteStk.Srl,QuoteStk.Sno" + @" " + Model.sord + @") AS RowNumber
                        FROM ItemMaster, Master, Salesman, Quote, QuoteStk  
                        where QuoteStk.MainType=Quote.MainType and QuoteStk.SubType=Quote.SubType  and QuoteStk.Type=Quote.Type and 
                        QuoteStk.Prefix=Quote.Prefix and QuoteStk.srl=Quote.srl and  ItemMaster.Code= QuoteStk.Code and 
                        Master.Code  = QuoteStk.Party 
                        and Quote.DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                        and Quote.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103) 
                        And Quote.Salesman=Salesman.Code  and QuoteStk.SubType = 'QS'      
                        And QuoteStk.Branch in(" + "'" + mbranchcode + "'" + @")  And Quote.Branch in(" + "'" + mbranchcode + "'" + @") And Left(Quote.AUTHORISE,1) = 'A'  
                        And Master.Branch in(" + "'" + mbranchcode + "'" + @") And Salesman.Locked=0 And Salesman.Branch in(" + "'" + mbranchcode + "'" + @")
                        " + Model.Item + @" " + Model.Party + @" " + Model.Salesman + @" " + Model.Value2 + @"
                        )
                        SELECT " + Model.HeaderContent + @"
                        FROM PAGED_Stock
                        ";
            }
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