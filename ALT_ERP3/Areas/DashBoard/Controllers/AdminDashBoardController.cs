using ALT_ERP3.Controllers;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.DashBoard.Controllers
{
    public class AdminDashBoardController : BaseController
    {
        ////nEntities context = new nEntities();
        //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        // GET: Transaction/SalesDashBoard

        public ActionResult Index()
        {
            Session["ModuleName"] = "Admin";
            GetAllMenu(Session["ModuleName"].ToString());
            return View();
        }
    }
}