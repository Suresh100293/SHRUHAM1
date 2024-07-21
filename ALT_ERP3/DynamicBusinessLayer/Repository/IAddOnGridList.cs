using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ALT_ERP3.DynamicBusinessLayer.Repository
{
    interface IAddOnGridList
    {
        object getAddonlist(string sidx, string sord, int page, int rows, string Code);
    }
}
