using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Controllers;

namespace ALT_ERP3.Areas.DashBoard.Controllers
{
    public class MasterDashBoardController : BaseController
    {
        // GET: DashBoard/MasterDashBoard
        public ActionResult Index()
        {
            Session["ModuleName"] = "Master";
            GetAllMenu(Session["ModuleName"].ToString());
            return View();
        }
    }
}