using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ALT_ERP3.DynamicBusinessLayer.Repository
{
    interface IListViewGridOperation
    {
        JsonResult getGridDataColumns(string id, string mFlag = "L");

        object Get(string sidx, string sord, int page, int rows, bool _search, string searchField, string searchOper, string searchString, string mFlag = "L");
    }
}