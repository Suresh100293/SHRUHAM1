using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Vehicles.Models
{
    public class ItemMasterVM
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public double Rate { get; set; }

        public string BaseGr { get; set; }
        public string BaseGrName { get; set; }

        public string Posting { get; set; }
        public string PostingName { get; set; }

        public string GSTCode { get; set; }
        public string GSTCodeName { get; set; }

        public string HSNCode { get; set; }
        public string HSNCodeName { get; set; }

        public List<SelectListItem> BranchList { get; set; }
        public string AppBranch { get; set; }
        public string AppBranchL { get; set; }

        public bool StockMaintain { get; set; }
        public int ExpiryDays { get; set; }
        public int ExpiryKm { get; set; }
        public bool Active { get; set; }
        public string Narr { get; set; }


        public bool ShortCutKey { get; set; }



        // iX9: Common default Fields
        public string Document { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Mode { get; set; }

        // iX9: used while passing parameters from MasterGrid to Option and Back 
        public string MainType { get; set; }
        public string Controller2 { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
    }
}