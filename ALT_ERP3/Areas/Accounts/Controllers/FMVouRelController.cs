using ALT_ERP3.Controllers;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class FMVouRelController : BaseController
    {

        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";


        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
            var result = ctxTFAT.TfatMenu.Where(z => z.OptionCode == moptioncode && z.ModuleName == mmodule).Select(x => new { x.AllowEdit, x.AllowDelete }).FirstOrDefault();
            string mopt = "EDVX";
            if (result != null)
            {
                mopt = (result.AllowEdit == false ? "X" : "E") + (result.AllowDelete == false ? "X" : "D") + "XX";
            }
            //return GetGridDataColumns(id, "L", mopt);
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


            if (!String.IsNullOrEmpty(Model.searchString) && Model.searchString != "undefined")
            {
                Model.searchOper = "cn";

                Model.searchField = " FMNO ";

            }


            var Reporttype = "M";
            if (Model.ViewDataId == "AccountOpeningManual")
            {
                Reporttype = "T";
            }


            return GetGridReport(Model, Reporttype, "MainType^" + Model.MainType, false, 0);
        }


        // GET: Accounts/FMVouRel

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

            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = DateTime.Today.ToShortDateString();

            return View(Model);
        }

        public ActionResult DeleteStateMaster(string FMNO)
        {
            string ErroMsg = "N";
            if (FMNO == null || FMNO == "")
            {
                return Json(new
                {
                    Message = "Document Not  Found..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var FMVOUREL = ctxTFAT.FMVouRel.Where(x => x.RECORDKEY.ToString().Trim() == FMNO).FirstOrDefault();
            var VoucherDetails = ctxTFAT.VoucherDetail.Where(x => x.FMNo.ToString().Trim() == FMVOUREL.FMNo.Trim()).ToList();

            if (VoucherDetails.Count() > 0)
            {
                return Json(new
                {
                    Message = "Already Payment Done Of " + FMVOUREL.FMNo + "  FM...",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }


            
            if (FMVOUREL != null)
            {
                ctxTFAT.FMVouRel.Remove(FMVOUREL);
            }

            ctxTFAT.SaveChanges();
            return Json(new { Msg = ErroMsg, Status = "Success" }, JsonRequestBehavior.AllowGet);


        }
    }
}