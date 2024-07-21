//using System.Web.UI.WebControls;
using ALT_ERP3.Controllers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class AccountSchedulesController : BaseController
    {
        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            if (Model.Document == "" || Model.Document == null)
            {
                
                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                Model.ToDate = DateTime.Today.ToShortDateString();
            }
            else
            {
                Model.Code = Model.Document;
                Model.AccountName = NameofAccount(Model.Document, "G");
                //Model.AccountName = ctxTFAT.MasterGroups.Where(z => z.Code == Model.Document).Select(x => x.Name).FirstOrDefault();
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, "Report", Model.Header, "", DateTime.Now, 0, Model.Document, "", "G");
            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            // AccountSchedules
            ViewBag.ViewDataId = Model.ViewDataId;
            Model.ViewCode = GetDefaultCode(Model.ViewDataId);
            ViewBag.Header = Model.Header;
            ViewBag.Controller = Model.Controller;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;
            return View(Model);
        }

        public ActionResult GetAccountList(string term)
        {
            if (term == "")
            {
                return Json(GetDataTableList("Select Top 15 Code,Name from MasterGroups Order by Name"), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(GetDataTableList("Select Code,Name from MasterGroups Where Name like '%" + term + "%' Order by Name"), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            //////IReportGridOperation mIlst = new ListViewGridOperationreport();
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        //[HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            GenerateGrpWithBalance(Model.Date, Model.Branch, "ACL", Convert.ToInt32(ppara01 == null || ppara01 == "" ? "0" : ppara01), "", false, 0,true);
            SqlConnection conTFAT = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("dbo.SPTFAT_GetAccountSchedule", conTFAT);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@mGroup", SqlDbType.VarChar).Value = Model.Code ?? "";
            conTFAT.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            conTFAT.Dispose();
            return GetGridReport(Model, "R", "Code^" + Model.Code, true, 0);
        }

        public ActionResult GetExcel(GridOption Model)
        {
            Model.mWhat = "XLS";
            return GetGridData(Model);
        }

        
    }
}