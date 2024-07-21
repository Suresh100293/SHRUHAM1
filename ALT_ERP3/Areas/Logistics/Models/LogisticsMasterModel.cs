using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TFATERPWebApplication.Areas.Logistics.Models
{
    public class LogisticsMasterModel
    {
        // dataField
        public string Name { get; set; }
        public string Language { get; set; }
        public string DialCode { get; set; }
        public string CurrentcyCode { get; set; }


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
        public string AccountName { get; internal set; }
        public dynamic Controller { get; internal set; }
        public dynamic ViewName { get; internal set; }
        public dynamic AcType { get; internal set; }
    }
}