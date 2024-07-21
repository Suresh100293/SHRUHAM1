using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Controllers;
using ALT_ERP3.DynamicBusinessLayer;
using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Reports.Controllers
{
    public class StockMovementStatusController : BaseController
    {
        //nEntities context = new nEntities();
        tfatEntities ctxTFAT = new tfatEntities();
        private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private string CompCode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        private string FinPeriod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        //IReportGridOperation mIlst = new ListViewGridOperationreport();
        List<SelectListItem> Store = new List<SelectListItem>();
        List<SelectListItem> ItemName = new List<SelectListItem>();
        List<SelectListItem> Itemgrp = new List<SelectListItem>();
     
        // GET: Reports/StockMovementStatus
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

        public ActionResult Getstore(GridOption Model)
        {

            var result = ctxTFAT.Stores.Select(m => new { m.Code, m.Name }).Distinct().ToList();
            foreach (var item in result)
            {
                Store.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }
            return Json(Store, JsonRequestBehavior.AllowGet);
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
        [HttpPost]
        public ActionResult GetGridStructureRecords(string id, GridOption Model)
        {
            Model.ViewDataId = "StkMinMax";
            id = Model.ViewDataId;
            return mIlst.getGridDataColumns(id, "", "", "", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            Model.Code = "StkMinMax";
            SqlConnection conntable2 = null;
            conntable2 = new SqlConnection(GetConnectionString());
            string droptable1 = @"select Code from  Tmp85451";
            string droptable2 = @"select Code from  Tmp88378";
            SqlCommand comm21 = new SqlCommand(droptable1, conntable2);
            SqlCommand comm22 = new SqlCommand(droptable2, conntable2);
            conntable2.Open();
            comm21.ExecuteNonQuery();
            comm22.ExecuteNonQuery();
            conntable2.Close();
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            SqlDataAdapter adap = new SqlDataAdapter(droptable1, conntable2);
            adap.Fill(ds);
            dt = ds.Tables[0];
            List<GridOption> newlist = dt.DataTableToList<GridOption>();
          
            if (newlist.Count!=0)
            {
                string query3 = @"drop table [Tmp85451]";
                string query4 = @"drop table [Tmp88378]";
                SqlCommand comm3 = new SqlCommand(query3, conntable2);
                SqlCommand comm4 = new SqlCommand(query4, conntable2);
                conntable2.Open();
                comm3.ExecuteNonQuery();
                comm4.ExecuteNonQuery();
                conntable2.Close();
            }
            var aa= newlist.Sum(x => x.Debit);

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

            var query1= @" SELECT ItemMaster.Code,  IsNull(sum(Stock.Qty), 0) as TQty,  
                           Count(Stock.Qty) as NoOfTrans INTO Tmp85451 
                           from Stock RIGHT OUTER JOIN ItemMaster ON Stock.Code = ItemMaster.Code
                           Where Stock.NotInStock=0 And Stock.Branch = 'HO0000' And Left(Stock.AUTHORISE,1) = 'A' 
                           And ItemMaster.Flag<>'G' And Stock.DocDate<='31-Mar-2018' Group By ItemMaster.Code
                           ";

            var query2 = @"Select Code,isnull(Sum(TQty), 0) as TotQty,isnull(Sum(nooftrans), 0) as Trans into Tmp88378
                         from Tmp85451 Group by Code";

            SqlConnection conntable = null;
           
            conntable = new SqlConnection(GetConnectionString());
            SqlCommand comm = new SqlCommand(query1, conntable);
            SqlCommand comm1 = new SqlCommand(query2, conntable);
            conntable.Open();
            comm.ExecuteNonQuery();
            comm1.ExecuteNonQuery();
            conntable.Close();
            Model.query = @"WITH PAGED_Stock  AS
                        (
                            Select i.Code,i.Name, CAST(Sum(s.TotQty) as Decimal(18,3)) as [Total Qty],
                            sum(dbo.GetPendingOrderItem('OS',i.Code,'HO0000','31-Mar-2018')) as SOQty,
                            sum(dbo.GetPendingOrderItem('OP',i.Code,'HO0000','31-Mar-2018')) as POQty,
                            Sum(dbo.GetReservation(i.Code,'HO0000')) as ResQty,0 as FreeQty,
                            CAST(d.ReordLevel as Decimal(18,3)) as [Re Ord Qty] , 
                            CAST(d.MinQty as Decimal(18,3)) as [MinQty] ,
                            CAST(d.MaxQty as Decimal(18,3)) as [MaxQty] ,
                            CAST(d.LeadTime as Decimal(18,3)) as [Lead Time], Sum(Trans) as [No.of Txn],
                            Case When Sum(Trans)>3 Then 'Fast' When Sum(Trans) > 1 Then 'Slow' Else 'Non-Moving' End as FSN, 
                            ROW_NUMBER() OVER (ORDER BY " + "i.Name" + @" " + Model.sord + @") AS RowNumber
                            From (Itemmaster i left join Tmp88378 s on i.Code=s.Code) left join 
                            ItemDetail d on d.code=i.Code and d.Branch='HO0000'  
                            Where i.NonStock=0  Group by i.Code,i.name,d.ReordLevel,d.minQty,d.MaxQty,d.LeadTime
                        )
                        SELECT Code,Name,[Total Qty],SOQty,POQty,ResQty,[Re Ord Qty],MinQty,MaxQty,[No.of Txn],FSN
                        FROM PAGED_Stock
                        WHERE RowNumber BETWEEN " + startIndex + @" AND " + endIndex + @";";

            Model.queryforcount = @"SELECT Count(*)                         
                       From (Itemmaster i left join Tmp88378 s on i.Code=s.Code) left join 
                            ItemDetail d on d.code=i.Code and d.Branch='HO0000'  
                            Where i.NonStock=0";
            

            return Content(JQGridHelper.JsonForJqgrid(JQGridHelper.GetDataTable(Model), Model.rows, JQGridHelper.GetTotalCount(Model), Model.page), "application/json");
        }

    }
}