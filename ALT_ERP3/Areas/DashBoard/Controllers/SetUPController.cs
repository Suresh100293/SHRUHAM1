using ALT_ERP3.Controllers;
using Common;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.DashBoard.Controllers
{
    public class SetUPController : BaseController
    {
        // GET: DashBoard/SetUP
        public ActionResult Index()
        {
            Session["ModuleName"] = "SetUP";
            //GetAllMenu(Session["ModuleName"].ToString());
            //GetAllMenu(Session["ModuleName"].ToString());
            ////AccountingGetAllMenu(Session["ModuleName"].ToString());
            ////VehicleGetAllMenu(Session["ModuleName"].ToString());
            GetAllMenu(Session["ModuleName"].ToString());
            //GridOption gridOption = new GridOption();

            //var html = ViewHelper.RenderPartialView(this, "MenuList", gridOption);
            //return Json(new { Html = html }, JsonRequestBehavior.AllowGet);

            return View();
        }

    }
}