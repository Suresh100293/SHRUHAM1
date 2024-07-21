using ALT_ERP3.Controllers;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.DashBoard.Controllers
{
    public class ReportsDashBoardController : BaseController
    {
        // GET: DashBoard/ReportsDashBoard
        public ActionResult Index()
        {
            Session["ModuleName"] = "Reports";
            //GetAllMenu(Session["ModuleName"].ToString());
            ////AccountingGetAllMenu(Session["ModuleName"].ToString());
            ////VehicleGetAllMenu(Session["ModuleName"].ToString());
            GetAllMenu(Session["ModuleName"].ToString());
            return View();
        }
    }
}