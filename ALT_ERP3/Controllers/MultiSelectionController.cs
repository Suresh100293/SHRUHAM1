using ALT_ERP3.Areas.Accounts.Models;
////using ALT_ERP3.DynamicBusinessLayer;
////using ALT_ERP3.DynamicBusinessLayer.Repository;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Controllers
{
    public class MultiSelectionController : BaseController
    {
        //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();

        public ActionResult Index()
        {
            //Model.Header = ctxTFAT.ReportHeader.Where(z => z.Code == Model.ViewDataId).Select(x => x.FormatHead).FirstOrDefault() ?? "";
            return View();
        }

        [HttpPost]
        public ActionResult GetGridStructure(string id)
        {
            ////ITransactionGridOperation mIlst = new TransactionGridOperation();
            return GetGridDataColumns(id, "L", "XXXX");
        }

        public ActionResult GetGridData(GridOption Model, string textsearch, string filterfield)
        {
            string mFilter = "";
            if (filterfield != "" && textsearch != "")
            {
                mFilter = ctxTFAT.TfatSearch.Where(z => z.Code == Model.ViewDataId && z.ColHead == filterfield).Select(x => x.ColField).FirstOrDefault() ?? "";
                if (mFilter != "")
                {
                    mFilter = mFilter + " like '%" + textsearch + "%'";
                }
            }
            return GetGridReport(Model, "M", "MainType^" + Model.MainType, false, 0, mFilter);
        }
    }
}