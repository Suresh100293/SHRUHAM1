using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.DynamicBusinessLayer
{
    public class AddonGridlist : IAddOnGridList
    {
        //nEntities context = new nEntities();
        //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
       //private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();

        public object getAddonlist(string sidx, string sord, int page, int rows, string Code)
        {
            int pageindex = Convert.ToInt32(page) - 1;
            int pagesize = rows;
            //var cntName = this.ControllerContext.RouteData.Values["controller"].ToString();
            var addonListResult = ctxTFAT.AddOns.Where(s => s.TableKey == Code).Select(
                                 a => new
                                 {
                                     a.Fld,
                                     a.FldType,
                                     a.Head,
                                     a.RECORDKEY
                                 });
            int totalRecords = addonListResult.Count();
            //int totaladdons = addonvalues.Count();
            var totalPages = (int)Math.Ceiling((float)totalRecords / (float)rows);
            if (sord.ToUpper() == "DESC")
            {
                addonListResult = addonListResult.OrderByDescending(s => s.Fld);
                addonListResult = addonListResult.Skip(pageindex * pagesize).Take(pagesize);
            }
            else
            {
                addonListResult = addonListResult.OrderBy(s => s.Fld);
                addonListResult = addonListResult.Skip(pageindex * pagesize).Take(pagesize);
            }
            var jsonData = new
            {
                total = totalPages,
                page,
                records = totalRecords,
                rows = addonListResult,
            };

            return jsonData;
        }

        
    }
}