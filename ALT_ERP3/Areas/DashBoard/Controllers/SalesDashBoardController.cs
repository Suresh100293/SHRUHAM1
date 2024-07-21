using ALT_ERP3.Controllers;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.DashBoard.Controllers
{
    public class SalesDashBoardController : BaseController
    {
        // GET: DashBoard/SalesDashBoard
        //private string muserid = System.Web.HttpContext.Current.Session["UserId"].ToString();

        public ActionResult Index(ActiveObjectsVM Model)
        {
            Session["ModuleName"] = "Sales";
            GetAllMenu(Session["ModuleName"].ToString());
            return View(Model);
        }

        public ActionResult GetBarDetail(string code)
        {
            GenerateChart(code);
            return null;
        }
    }
}