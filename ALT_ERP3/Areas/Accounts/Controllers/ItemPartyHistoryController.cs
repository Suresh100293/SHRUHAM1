using EntitiModel;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using System.Web.UI.WebControls;
using ALT_ERP3.Controllers;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class ItemPartyHistoryController : BaseController
    {
        decimal mOpeningBalance = 0;

        // GET: Reports/ItemPartyHistory
        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = DateTime.Today.ToShortDateString();
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "A");
            if (Model.Document == "" || Model.Document == null)
            {
                GetAllMenu(Session["ModuleName"].ToString());

            }
            else
            {
                Model.Item = Model.Item;    // just dummy to know that they are passed from calling form
                Model.Code = Model.Document;    // just dummy to know that they are passed from calling form
                Model.AccountName = NameofAccount(Model.Document);
            }
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
                return Json(GetDataTableList("Select Top 15 Code,Name from Master where (BaseGr='D' or BaseGr='S' or BaseGr='U') and Hide=0 Order by Name"), JsonRequestBehavior.AllowGet);
                //var result = ctxTFAT.Master.Where(z => (z.BaseGr == "D" || z.BaseGr == "S" || z.BaseGr == "U") && z.Hide == false).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).Take(15).ToList();
                //return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(GetDataTableList("Select Code,Name from Master where Name like '%" + term + "%' and (BaseGr='D' or BaseGr='S' or BaseGr='U') and Hide=0 Order by Name"), JsonRequestBehavior.AllowGet);
                //var result = ctxTFAT.Master.Where(x => x.Name.Contains(term) && (x.BaseGr == "D" || x.BaseGr == "S" || x.BaseGr == "U") && x.Hide == false).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).ToList();
                //return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetItemList(string term)
        {
            if (term == "")
            {
                return Json(GetDataTableList("Select Top 15 Code,Name='[' + Code + '] '+Name from ItemMaster Order by Name"), JsonRequestBehavior.AllowGet);
                //var result = ctxTFAT.ItemMaster.Select(m => new { m.Code, Name = "[" + m.Code + "] " + m.Name }).OrderBy(n => n.Name).Take(15).ToList();
                //return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(GetDataTableList("Select Code,Name='[' + Code + '] '+Name from ItemMaster where (Code like '%" + term + "%' or Name like '%" + term + "%') Order by Name"), JsonRequestBehavior.AllowGet);
                //var result = ctxTFAT.ItemMaster.Where(x => x.Name.Contains(term) || x.Code.Contains(term)).Select(m => new { m.Code, Name = "[" + m.Code + "] " + m.Name }).OrderBy(n => n.Name).ToList();
                //return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        //[HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            //if (Model.Code == null && Model.Item == null) return null;
            return GetGridReport(Model, "R", (Model.Code != null && Model.Code.Trim() != "" ? "Code^" + Model.Code + "~" : "") + (Model.Item != null && Model.Item != "" ? "Item^" + Model.Item + "~" : "") + (mpara != "" ? "~" + mpara : ""));
        }

        public ActionResult GetExcel(GridOption Model)
        {
            Model.mWhat = "XLS";
            return GetGridData(Model);
        }

        public ActionResult PrintReport(GridOption Model)
        {
            Model.mWhat = "PDF";
            return GetGridData(Model);
        }

        #region tab grids
        [HttpPost]
        public ActionResult GetGridStructureSales(GridOption Model)
        {
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        [HttpPost]
        public ActionResult GetGridDataSales(GridOption Model)
        {
            return GetGridReport(Model, "R", (Model.Code != null && Model.Code.Trim() != "" ? "Code^" + Model.Code + "~" : "") + (Model.Item != null && Model.Item != "" ? "Item^" + Model.Item + "~" : "") + (mpara != "" ? "~" + mpara : ""));
            //return GetGridReport(Model, "R", "Code^" + Model.Code + "~Item^" + Model.Item + "~" + (mpara != "" ? "~" + mpara : ""), false, 0);
        }
        #endregion tab grids
    }
}