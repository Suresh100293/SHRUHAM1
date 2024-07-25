using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class ItemTrackingController : BaseController
    {

        public ActionResult GetGroups(string term)
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            if (term == "" || term == null)
            {
                var result = ctxTFAT.ItemGroups.ToList().Take(10);
                var Modified = result.Select(x => new
                {
                    Code = x.Code,
                    Name = x.Name
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.ItemGroups.Where(x => x.Name.Contains(term)).ToList().Take(10);
                var Modified = result.Select(x => new
                {
                    Code = x.Code,
                    Name = x.Name
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetItemMasters(string term, string ItemGroup)
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            if (term == "" || term == null)
            {
                var result = ctxTFAT.ItemMaster.Where(x => x.BaseGr == ItemGroup).ToList().Take(10);
                var Modified = result.Select(x => new
                {
                    Code = x.Code,
                    Name = x.Name
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.ItemMaster.Where(x => x.BaseGr == ItemGroup && x.Name.Contains(term)).ToList().Take(10);
                var Modified = result.Select(x => new
                {
                    Code = x.Code,
                    Name = x.Name
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Logistics/ItemTracking
        public ActionResult Index(ItemTrackVM Model)
        {
            UpdateAuditTrail(mbranchcode, "Report", Model.Header, "", DateTime.Now, 0, "", "", "NA");

            if (Model.Document == "" || Model.Document == null)
            {
                GetAllMenu(Session["ModuleName"].ToString());

            }
            ViewBag.ViewDataId = Model.ViewDataId;
            ViewBag.Header = Model.Header;
            ViewBag.Controller = Model.Controller;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;

            return View(Model);
        }

        #region Item History 

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
            return GetGridDataColumns(id, "L", "XXXX");
        }
        public ActionResult GetGridData(GridOption Model)
        {
            return GetGridReport(Model, "M", "Code^" + Model.Code, false, 0);
        }

        #endregion
    }
}