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
    public class RegisterController : BaseController
    {
        //nEntities context = new nEntities();
        tfatEntities ctxTFAT = new tfatEntities();
        private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private string CompCode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        private string FinPeriod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        //IReportGridOperation mIlst = new ListViewGridOperationreport();
        // GET: Reports/Register
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
            if (Model.Value1 == "Both")
            {
                Model.query = @"WITH PAGED_Stock  AS
                    (
                    Select Master.Name as AccountDescription,CostCentre.name as CostCentre,
                    ledger.Type as Type,ledger.Srl as Serial,Convert(char(10),
                    ledger.DocDate, 103) as DocDate,Cast(ledger.Debit as Decimal(14,2)) as Debit,
                    Cast(ledger.Credit as Decimal(14,2)) as Credit from CostLedger Ledger,
                    Costcentre,Master  Where  Ledger.MainType<>'MV' And Ledger.MainType<>'PV' 
                    And  CostCentre.Code=Ledger.CostCode and Master.Code=Ledger.Code 
                    And Master.Branch='HO0000'  And DocDate>='01-Apr-2017' 
                    And DocDate <= '31-Mar-2018'  And CostCentre.Branch='HO0000' 
                    And Ledger.Branch='HO0000' And Master.Branch='HO0000'  
                    And Left(Ledger.AUTHORISE,1) = 'A'
                    ) 
                SELECT AccountDescription,CostCentre,Type,Serial,DocDate,Debit,Credit
                FROM PAGED_Stock";
            }

            if (Model.Value1 == "Debit")
            {
                Model.query = @"WITH PAGED_Stock  AS
                    (
                    Select Master.Name as AccountDescription,CostCentre.name as CostCentre,
                    ledger.Type as Type,ledger.Srl as Serial,Convert(char(10),
                    ledger.DocDate, 103) as DocDate,Cast(ledger.Debit as Decimal(14,2)) as Debit,
                    Cast(ledger.Credit as Decimal(14,2)) as Credit from CostLedger Ledger,
                    Costcentre,Master  Where  Ledger.MainType<>'MV' And Ledger.MainType<>'PV'
                    And  CostCentre.Code=Ledger.CostCode and Master.Code=Ledger.Code 
                    And Master.Branch='HO0000'  and Debit <>0  
                    And Ledger.CostCode In ('B2C','C03','C18','C20','C30','C23','C22','C21','C25','C29','C31','C24','C19','C27','C26','C28','C16','C15','C17','C14','C02','C04','C06','C09','C08','C07','C11','C10','C05','C13','C12') 
                    And DocDate>='01-Apr-2017' And DocDate <= '31-Mar-2018'  
                    And CostCentre.Branch='HO0000' And Ledger.Branch='HO0000' 
                    And Master.Branch='HO0000'  And Left(Ledger.AUTHORISE,1) = 'A' 
                  
                    ) 
                SELECT AccountDescription,CostCentre,Type,Serial,DocDate,Debit,Credit
                FROM PAGED_Stock";
            }
            if (Model.Value1 == "Credit")
            {
                Model.query = @"WITH PAGED_Stock  AS
                    (
                    Select Master.Name as AccountDescription,CostCentre.name as CostCentre,
                    ledger.Type as Type,ledger.Srl as Serial,Convert(char(10),
                    ledger.DocDate, 103) as DocDate,Cast(ledger.Debit as Decimal(14,2)) as Debit,
                    Cast(ledger.Credit as Decimal(14,2)) as Credit from CostLedger Ledger,
                    Costcentre,Master  Where  Ledger.MainType<>'MV' And Ledger.MainType<>'PV'
                    And  CostCentre.Code=Ledger.CostCode and Master.Code=Ledger.Code 
                    And Master.Branch='HO0000'  and Credit <>0  
                    And Ledger.CostCode In ('B2C','C03','C18','C20','C30','C23','C22','C21','C25','C29','C31','C24','C19','C27','C26','C28','C16','C15','C17','C14','C02','C04','C06','C09','C08','C07','C11','C10','C05','C13','C12') 
                    And DocDate>='01-Apr-2017' And DocDate <= '31-Mar-2018'  
                    And CostCentre.Branch='HO0000' And Ledger.Branch='HO0000' 
                    And Master.Branch='HO0000'  And Left(Ledger.AUTHORISE,1) = 'A' 
                    ) 
                SELECT AccountDescription,CostCentre,Type,Serial,DocDate,Debit,Credit
                FROM PAGED_Stock";
            }
            if (Model.query != null)
            {
                DataTable dt = new DataTable();
                DataSet ds = new DataSet();
                SqlConnection conn = new SqlConnection(GetConnectionString());
                SqlDataAdapter adap = new SqlDataAdapter(Model.query, conn);

                adap.Fill(ds);
                dt = ds.Tables[0];
                conn.Close();
                List<GridOption> newlist = dt.DataTableToList<GridOption>();
                Model.list = newlist;
            }

            return View(Model);
        }
    }
}