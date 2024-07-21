using System.Web.Mvc;

namespace ALT_ERP3.DynamicBusinessLayer.Repository
{
    interface IReportGridOperation
    {
        JsonResult getGridDataColumns(string id, string open="0", string close = "0", string TCredit = "0", string TDebit = "0", string mFlag = "L",string mVar1= "_", string mVar2 = "_", string mVar3 = "_", string mVar4 = "_");

        //object Get(string sidx, string sord, int page, int rows, bool _search, string searchField, string searchOper, string searchString, string mFlag = "L");
    }
}
