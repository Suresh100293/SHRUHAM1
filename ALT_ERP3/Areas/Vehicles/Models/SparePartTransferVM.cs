using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Vehicles.Models
{
    public class SparePartTransferVM
    {
        public List<SparePartTransferVM> ItemList { get; set; }
        public string DocDate { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string VehicleNo { get; set; }
        public string VehicleNoN { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }

        public int TotalQty { get; set; }
        public int BalQty { get; set; }
        public int Qty { get; set; }
        public int Srno { get; set; }
        public string Tablekey { get; set; }
        public string ParentKey { get; set; }

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
        public string Type { get; set; }
        public string SubType { get; set; }
    }
}