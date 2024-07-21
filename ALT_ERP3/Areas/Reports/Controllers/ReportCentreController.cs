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
using System.Text;
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
    public class ReportCentreController : ReportController
    {
        tfatEntities ctxTFAT = new tfatEntities();
        private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private string muserid = System.Web.HttpContext.Current.Session["UserId"].ToString();
        private static string mPara = "";
        private static string mparastring = "";
        private static string maccgroups = "";
        private static string mitemgroups = "";
        private static string msub = "";
        
        // GET: Reports/ReportCentre
        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "");
            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            var reportheader = ctxTFAT.ReportHeader.Where(z => z.SubCodeOf == Model.ViewDataId && z.DefaultReport == true).Select(x => x).FirstOrDefault();
            Model.ViewCode = reportheader.Code ?? "";
            Model.IsFormatSelected = (Model.ViewCode == null || Model.ViewCode == "") ?  false: true;
            List<string> inputlist = new List<string>();
            List<string> inputlist2 = new List<string>();
            inputlist = (reportheader.InputPara == "" || reportheader.InputPara == null) ? inputlist : reportheader.InputPara.Split('~').ToList();
            foreach (var ai in inputlist)
            {
                var a = ai.Split('^');
                string a1 = a[0];
                string a2 = GetQueryText(a[1]);
                inputlist2.Add(a1 + "^" + a2);
            }
            Model.AddOnParaList = inputlist2;
            return View(Model);
        }

        [HttpPost]
        public ActionResult GetParameterValues(string mformat)
        {
            List<string> inputlist = new List<string>();
            List<string> inputlist2 = new List<string>();
            string mparastring = "";
            string maccgroups = "";
            string mitemgroups = "";
            string msubtypes = "";
            string minputpara = "";
            var result = ctxTFAT.ReportHeader.Select(x=> new { x.ParaString, x.AccGroups, x.ItemGroups, x.SubTypes, x.Code, x.InputPara }).Where(z=>z.Code == mformat).FirstOrDefault();
            if (result != null)
            {
                mparastring = result.ParaString;
                maccgroups = result.AccGroups;
                mitemgroups = result.ItemGroups;
                msubtypes = result.SubTypes;
                minputpara = result.InputPara;
                inputlist = (result.InputPara == "" || result.InputPara == null)? inputlist: result.InputPara.Split('~').ToList();
            }
            GridOption Model = new GridOption();

            foreach(var ai in inputlist)
            {
                var a = ai.Split('^');
                string a1 = a[0];
                string a2 = GetQueryText(a[1]);
                inputlist2.Add(a1 + "^"  + a2);
            }
            Model.AddOnParaList = inputlist2;
            string html = ViewHelper.RenderPartialView(this, "ReportAddOnGrid", Model);
            return Json(new
            {
                parastring = mparastring,
                itemgroup = mitemgroups,
                accgroup = maccgroups,
                sub = msubtypes,
                inputpara = minputpara,
                inputlist = inputlist,
                Status = "Success",
                Html = html,
                Message = ""
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetViewCodes(string mSubCodeOf)
        {
            return rep_GetViewCodes(mSubCodeOf);
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
        public string GetAccGroupsTree(string maccgroup)
        {
            return rep_GetAccGroupsTree(maccgroup);
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
        public ActionResult GetCityLists()
        {
            return rep_GetCityLists();
        }

        [HttpPost]
        public ActionResult GetStateLists()
        {
            return rep_GetStateLists();
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
            mPara = rep_SaveParameters(Model.SelectContent.Trim());
            string mGrp = rep_GetGroupby(Model.ViewDataId);
            if (Model.IsFormatSelected == true)
            {
                int noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=0 Where SubCodeOf='" + Model.OptionCode + "'");
                noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=-1 Where Code='" + Model.ViewDataId + "'");
            }
            return Json(new
            {
                Group = mGrp,
                Status = "Success",
                Message = ""
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            //IReportGridOperation mIlst = new ListViewGridOperationreport();
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
            return PrintReportsCrystal(Model, "REP_AcLedgerPrint", "SPREP_LedgerPrint", "PDF", false);
        }

        public string GetQueryText(string sql)
        {
            string connstring = "";
            connstring = GetConnectionString();
            string bca = "";
            if (sql.Contains(","))
            {
                bca = sql;
            }
            else
            {
                StringBuilder addonT = new StringBuilder();

                DataTable mDt2 = GetDataTable(sql, connstring);
                if (mDt2.Rows.Count > 0)
                {
                    for (var i = 0; i < mDt2.Rows.Count; i++)
                    {
                        bca = (mDt2.Rows[i][0].ToString() == "" || mDt2.Rows[i][0].ToString() == null) ? "" : mDt2.Rows[i][0].ToString();
                        addonT.Append(bca + ",");
                    }

                }
               
                string addonVT = addonT.ToString();
                if (addonVT != "")
                {
                    bca = addonVT.Remove(addonVT.Length - 1);
                }
                else
                {
                    bca = sql;
                }


            }
            return bca;
        }
        #endregion executereport
    }
}