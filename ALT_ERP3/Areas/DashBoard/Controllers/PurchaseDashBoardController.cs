using ALT_ERP3.Controllers;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.DashBoard.Controllers
{
    public class PurchaseDashBoardController : BaseController
    {
        // GET: DashBoard/PurchaseDashBoard
        //nEntities context = new nEntities();
        //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        public ActionResult Index(ActiveObjectsVM Model)
        {
            //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
            Session["ModuleName"] = "Purchase";
            GetAllMenu(Session["ModuleName"].ToString());
            return View(Model);
        }
    }
}