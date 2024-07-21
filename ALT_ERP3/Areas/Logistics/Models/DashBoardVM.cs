using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class DashBoardVM
    {

        public string UserCode { get; set; }
        public string Branch { get; set; }
        public string DocDate { get; set; }
        public string Val1 { get; set; }
        public string Val1N { get; set; }
        public string Val2 { get; set; }
        public string Val2N { get; set; }
        public string Val3 { get; set; }
        public string Val3N { get; set; }
        public string Val4 { get; set; }
        public string Val4N { get; set; }
        public string Val5 { get; set; }
        public string Val5N { get; set; }
        public string Val6 { get; set; }
        public string Val6N { get; set; }
        public string Val7 { get; set; }
        public string Val7N { get; set; }
        public string Val8 { get; set; }
        public string Val8N { get; set; }
        public string Val9 { get; set; }
        public string Val9N { get; set; }
        public string Val10 { get; set; }
        public string Val10N { get; set; }
        public string Val11 { get; set; }
        public string Val11N { get; set; }
        public string Val12 { get; set; }
        public string Val12N { get; set; }
        public string Val13 { get; set; }
        public string Val13N { get; set; }
        public string Val14 { get; set; }
        public string Val14N { get; set; }
        public string Val15 { get; set; }
        public string Val15N { get; set; }

        // iX9: used while passing parameters from MasterGrid to Option and Back 
        public string MainType { get; set; }
        public string Controller2 { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
        public string Document { get; set; }
        public dynamic Controller { get;  set; }
        public string Date { get;  set; }
        public string Mode { get;  set; }
    }
}