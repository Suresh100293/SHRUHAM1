using Common;
using EntitiModel;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using ALT_ERP3.Controllers;
using ALT_ERP3.DynamicBusinessLayer;
using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Reports.Controllers
{
    public class PurchSalesRegStockController : ReportController
    {
        private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private static string mPara = "";
        private static string mformat = "";

        // GET: Reports/ReportCentre
        public ActionResult Index(GridOption Model)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "");
            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            //Model.ViewDataId = Model.ViewDataId;
            return View(Model);
        }

        public ActionResult GetViewCodes(string term)
        {
            return rep_GetViewCodes(term);
        }

        public ActionResult GetItems(string ItemGroups)
        {
            return rep_GetItems(ItemGroups);
        }

        [HttpPost]
        public string GetItemGroupsTree(string ItemGroups)
        {
            return rep_GetItemGroupsTree(ItemGroups);
        }

        [HttpPost]
        public string GetAccGroupsTree()
        {
            return rep_GetAccGroupsTree("");
        }

        [HttpPost]
        public string GetStoresTree()
        {
            return rep_GetStoresTree();
        }

        public ActionResult GetAccounts(string mGroups)
        {
            return rep_GetAccounts(mGroups);
        }

        [HttpPost]
        public ActionResult GetSalesmanTree()
        {
            return rep_GetSalesmanTree();
        }

        [HttpPost]
        public ActionResult GetProcessLists()
        {
            return rep_GetProcessLists();
        }

        [HttpPost]
        public ActionResult GetAreaLists()
        {
            return rep_GetAreaLists();
        }

        [HttpPost]
        public ActionResult GetItemCategoryLists()
        {
            return rep_GetItemCategoryLists();
        }

        [HttpPost]
        public ActionResult GetBrokerLists()
        {
            return rep_GetBrokerLists();
        }

        [HttpPost]
        public string GetTypesTree(string msub)
        {
            return rep_GetTypesTree(msub);
        }

        #region executereport
        [HttpPost]
        public ActionResult SaveParameters(GridOption Model)
        {
            mformat = Model.ViewDataId;
            string mPara = rep_SaveParameters(Model.SelectContent.Trim());
            return Json(new
            {
                Status = "Success",
                Message = ""
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            //IReportGridOperation mIlst = new ListViewGridOperationreport();
            Model.ViewDataId = mformat;
            return mIlst.getGridDataColumns(Model.ViewDataId, "", "", "", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            return GetGridReport(Model, "R", mPara, false, 0);
        }

        public ActionResult GetExcel(GridOption Model)
        {
            Model.mWhat = "XLS";
            return GetGridReport(Model, "R", mPara, false, 0);
        }

        public ActionResult GetPDF(GridOption Model)
        {
            Model.mWhat = "PDF";
            return GetGridReport(Model, "R", mPara, false, 0);
        }

        public ActionResult PrintReport(GridOption Model)
        {
            Model.mWhat = "PDF";
            return PrintReportsCrystal(Model, "REP_AcLedgerPrint", "SPREP_LedgerPrint", "PDF", false,mPara);
        }
        #endregion executereport
    }
}