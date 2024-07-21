using ALT_ERP3.Controllers;
//using ALT_ERP3.DynamicBusinessLayer.Repository;
using Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Reports.Controllers
{
    public class AccCostCentreMatrixController : BaseController
    {
        private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private string CompCode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        private string FinPeriod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        //IReportGridOperation mIlst = new ListViewGridOperationreport();
        // GET: Reports/AccCostCentreMatrix
        public ActionResult Index(GridOption Model)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "");
            if (FinPeriod != "null")
            {
                var perd = ctxTFAT.TfatPerd.Where(x => x.Code == CompCode && x.PerdCode == FinPeriod).Select(b => new { b.StartDate, b.LastDate }).FirstOrDefault();
                Model.FromDate = JQGridHelper.ConvertDate(perd.StartDate.ToShortDateString()).Replace("-", "/");
                Model.ToDate = JQGridHelper.ConvertDate(perd.LastDate.Value.ToShortDateString()).Replace("-", "/");
            }
            return View(Model);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id, GridOption Model)
        {
            var Code = ctxTFAT.CostCentre.Where(x => x.Flag == "L" && x.CCType == "C").Select(m => new { m.Code, m.Name }).Distinct().ToList();
            var HeaderList = (from X in ctxTFAT.CostCentre
                              where X.Flag == "L" && X.CCType == "C"
                              select new { X.Name }).ToList();
            List<GridOption> newlist = new List<GridOption>();
            List<string> colname = new List<string>();
            List<GridColumn> colModal = new List<GridColumn>();
            List<object> result = new List<object>();

            colname.Add("Name");
            GridColumn gc1 = new GridColumn();
            gc1.name = "Name";
            gc1.index = "Name";
            gc1.width = "40px";
            gc1.frozen = true;
            colModal.Add(gc1);

            foreach (var Fld in HeaderList)
            {
                colname.Add(Fld.Name);
                GridColumn gc = new GridColumn();
                gc.name = Fld.Name;
                gc.editable = false;
                colModal.Add(gc);
            }
            result.Add(Core.CoreCommon.GetString(colname.ToArray()));
            result.Add(colModal);

            JsonResult JR = new JsonResult();
            JR.Data = Newtonsoft.Json.JsonConvert.SerializeObject(result);
            return JR;
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {

            if (Model.Date != null)
            {
                var date = Model.Date.Split('-');
                Model.FromDate = JQGridHelper.ConvertDate(date[0]);
                Model.ToDate = JQGridHelper.ConvertDate(date[1]);

            }
            var SDate = Convert.ToDateTime(Model.FromDate);
            var LDate = Convert.ToDateTime(Model.ToDate);

            int startIndex = (Model.page - 1) * Model.rows;
            int endIndex = Model.page * Model.rows;

            var Code = ctxTFAT.CostCentre.Where(x => x.Flag == "L" && x.CCType == "C").Select(m => new { m.Code, m.Name }).Distinct().ToList();
            var HeaderList = (from X in ctxTFAT.CostCentre
                              where X.Flag == "L" && X.CCType == "C"
                              select X.Name).ToList();
            List<string> header = new List<string>();
            List<string> Select = new List<string>();
            for (int i = 0; i < Code.Count; i++)
            {
                for (int j = 0; j < Code.Count; j++)
                {
                    if (i == j)
                    {
                        header.Add(HeaderList[i].Replace(".", " ").Replace(" ", "").Replace("-", "").Replace("%", "").Replace("(", "").Replace(")", ""));
                        Select.Add("Sum(Case Costcentre.Name When " + "'" + Code[i].Name + "'" + @" Then Debit - Credit Else 0 End) as " + Code[i].Name.Replace(".", " ").Replace(" ", "").Replace("-", "").Replace("%", "").Replace("(", "").Replace(")", "") + @"");
                    }

                }
            }
            Model.HeaderContent = string.Join(",", header).ToString();
            Model.SelectContent = string.Join(",", Select).ToString();


            Model.query = @"WITH PAGED_Stock  AS
                        (
                         Select Master.Name as Name," + Model.SelectContent + @",
                         ROW_NUMBER() OVER (ORDER BY " + "Master.Name" + @" " + Model.sord + @") AS RowNumber
                         From CostLedger Ledger,CostCentre,Master Where  
                         Ledger.MainType<>'MV' And Ledger.MainType<>'PV' And  
                         Ledger.Code = Master.Code And  Ledger.CostCode= Costcentre.Code And  
                         Ledger.branch in (" + "'" + mbranchcode + "'" + @")  And  costcentre.Branch in (" + "'" + mbranchcode + "'" + @") And  
                         Costcentre.CCType = 'C' 
                         And Ledger.DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                         And Ledger.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103)
                         And  Left(Ledger.AUTHORISE,1) = 'A' And  Master.branch = 'HO0000'  
                         Group by Master.Code,Master.name  
                        ) 
                        SELECT Name," + Model.HeaderContent + @"
                         FROM PAGED_Stock
                        WHERE RowNumber BETWEEN " + startIndex + @" AND " + endIndex + @";";

            Model.queryforcount = @"SELECT Count(*)                         
                         From CostLedger Ledger,CostCentre,Master Where  
                         Ledger.MainType<>'MV' And Ledger.MainType<>'PV' And  
                         Ledger.Code = Master.Code And  Ledger.CostCode= Costcentre.Code And  
                         Ledger.branch in (" + "'" + mbranchcode + "'" + @")  And  costcentre.Branch in (" + "'" + mbranchcode + "'" + @") And  
                         Costcentre.CCType = 'C' 
                         And Ledger.DocDate >= Convert(Datetime, " + "'" + SDate + "'" + @" , 103) 
                         And Ledger.DocDate <= Convert(Datetime ," + "'" + LDate + "'" + @" , 103)
                         And  Left(Ledger.AUTHORISE,1) = 'A' And  Master.branch = 'HO0000'  
                         Group by Master.Code,Master.name";

            return Content(JQGridHelper.JsonForJqgrid(GetDataTable(Model.query), Model.rows, GetTotalCount(Model), Model.page), "application/json");
        }
    }
}