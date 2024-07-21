using ALT_ERP3.Controllers;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.DashBoard.Controllers
{
    public class ControlPanelController : BaseController
    {
        // GET: DashBoard/ControlPanel
        public ActionResult Index()
        {
            Session["ModuleName"] = "ControlPanel";
            //GetAllMenu(Session["ModuleName"].ToString());
            ////AccountingGetAllMenu(Session["ModuleName"].ToString());
            ////VehicleGetAllMenu(Session["ModuleName"].ToString());
            GetAllMenu(Session["ModuleName"].ToString());
            return View();
        }
    }
}