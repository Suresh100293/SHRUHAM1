using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class AutoConsignmentMailVM
    {
        public bool DailyBasis { get; set; }
        public bool YestDailyBasis { get; set; }
        public string Type { get; set; }

        public string Code { get; set; }
        public string ReportName { get; set; }

        public List<SelectListItem> Customers { get; set; }
        public string Account { get; set; }
        public string AccountL { get; set; }

        public bool Active { get; set; }

        public string EmailTo { get; set; }
        public string EmailCC { get; set; }
        public string EmailBCC { get; set; }

        public string Time { get; set; }

        public bool EveryDay { get; set; }
        public bool Day { get; set; }
        public string DayName { get; set; }
        public bool DateR { get; set; }
        public string Date { get; set; }

        public string UptoDate { get; set; }

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