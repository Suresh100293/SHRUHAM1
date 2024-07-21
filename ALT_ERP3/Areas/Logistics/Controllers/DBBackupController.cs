using ALT_ERP3.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class DBBackupController : BaseController
    {
        // GET: Logistics/DBBackup
        public ActionResult Index(GridOption Model)
        {
            Session["TempAttach"] = null;
            GetAllMenu(Session["ModuleName"].ToString());
            
            if (Model.Document == null)
            {
                Model.Document = "";
                Model.AccountName = "";
            }
            
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
            return View(Model);
        }




    }
}