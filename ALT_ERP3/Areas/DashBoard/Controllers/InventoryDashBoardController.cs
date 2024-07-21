using ALT_ERP3.Controllers;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.DashBoard.Controllers
{
    public class InventoryDashBoardController : BaseController
    {
        // GET: DashBoard/InventoryDashBoard
        //nEntities context = new nEntities();
        //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        public ActionResult Index()
        {
            Session["ModuleName"] = "Inventory";
            GetAllMenu(Session["ModuleName"].ToString());
            return View();
        }
    }
}