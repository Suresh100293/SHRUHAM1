using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class LogisticsMasterGridController : BaseController
    {
         
        //ITransactionGridOperation mIlst = new TransactionGridOperation();
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        public new string muserid = System.Web.HttpContext.Current.Session["UserId"].ToString();

        // GET: Logistics/LogisticsMasterGrid
        public ActionResult Index(GridOption Model)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());

            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "A");

            if (Model.Document == null)
            {
                Model.Document = "";
                Model.AccountName = "";
            }
            moptioncode = Model.OptionCode;
            msubcodeof = Model.ViewDataId;
            mmodule = Model.Module;
            ViewBag.id = Model.ViewDataId;
            ViewBag.ViewDataId = Model.ViewDataId;
            ViewBag.Header = Model.Header;
            ViewBag.Table = Model.TableName;
            ViewBag.Controller = Model.Controller;
            ViewBag.MainType = Model.MainType;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;
            ViewBag.ViewName = Model.ViewName;
            if (Model.AcType != null)
            {
                ViewBag.MainType = Model.AcType;
            }
            TfatMenu tfatMenu = ctxTFAT.TfatMenu.Where(z => z.OptionCode == moptioncode && z.ModuleName == mmodule).FirstOrDefault();
            if (muserid.ToUpper() == "SUPER")
            {
                Model.xAdd = true;
                Model.xDelete = true;
                Model.xEdit = true;
                Model.xPrint = true;
            }
            else
            {
                var result = ctxTFAT.UserRights.Where(z => z.MenuID == tfatMenu.ID && z.ModuleName == mmodule && z.Code == muserid).FirstOrDefault();
                
                if (result != null)
                {
                    Model.xAdd = result.xAdd;
                    Model.xDelete = result.xDelete;
                    Model.xEdit = result.xEdit;
                    Model.xPrint = result.xPrint;
                }
            }
            return View(Model);
        }

        public ActionResult GetFormats()
        {
            var result = ctxTFAT.ReportHeader.Where(x => x.SubCodeOf == msubcodeof && !x.Code.EndsWith(".bak")).Select(m => new
            {
                Code = m.Code,
                Name = m.Code
            }).OrderBy(n => n.Code).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LorryDraft()
        {
            var result = ctxTFAT.ReportHeader.Where(x => x.SubCodeOf == "LorryDraft" && !x.Code.EndsWith(".bak")).Select(m => new
            {
                Code = m.Code,
                Name = m.Code
            }).OrderBy(n => n.Code).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
            return GetGridDataColumns(id, "L", "XXXX");
        }

        //[HttpPost]
        public ActionResult GetMasterGridData(GridOption Model)
        {
            if (Model.IsFormatSelected == true)
            {
                int noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=0 Where SubCodeOf='" + msubcodeof + "'");
                noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=-1 Where Code='" + Model.ViewDataId + "'");
            }
            return GetGridReport(Model, "M", "MainType^" + Model.MainType, false, 0);
        }
    }
}