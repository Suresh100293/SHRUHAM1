using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class PickOrderSetupVM
    {
        public AscDescOrder SelectColumn { get; set; }
        public bool Asc { get; set; }
        public bool Desc { get; set; }




        public string OrderType { get; set; }
        public string OrderType_Name { get; set; }

        public string ServiceType { get; set; }
        public string ServiceType_Name { get; set; }

        public bool AutoSelctFromBasedOnOrderBranch { get; set; }

        public bool From { get; set; }
        

        public string Colln { get; set; }
        public string Del { get; set; }

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

    public enum AscDescOrder
    {
        OrderDate, OrderNo
    }
}