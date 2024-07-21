using ALT_ERP3.Controllers;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.DashBoard.Controllers
{
    public class PlantDashBoardController : BaseController
    {
        // GET: DashBoard/PlantDashBoard
        //nEntities context = new nEntities();
        //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        public ActionResult Index()
        {
            Session["ModuleName"] = "Plant";
            GetAllMenu(Session["ModuleName"].ToString());
            return View();
        }
    }
}