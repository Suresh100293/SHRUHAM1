using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class MultiDeliveryVM
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public string DeliveryDays { get; set; }

        public string Lrno { get; set; }
        public DateTime BookDate { get; set; }
        public double ActWt { get; set; }
        public double ChgWt { get; set; }
        public string Unit { get; set; }
        public string UnitName { get; set; }
        public string ChgType { get; set; }
        public string ChgTypeName { get; set; }
        public string From { get; set; }
        public string FromName { get; set; }
        public string To { get; set; }
        public string ToName { get; set; }
        public string Delivery { get; set; }
        public string Colln { get; set; }
        public string ConsignerCode { get; set; }
        public string ConsignerName { get; set; }
        public string ConsigneeCode { get; set; }
        public string ConsigneeName { get; set; }
        public string LRtype { get; set; }
        public string LRtypeName { get; set; }
        public int TotQty { get; set; }
        public string Tablekey { get; set; }

        public string StockBranch { get; set; }
        public string StockType { get; set; }
        public int StockQty { get; set; }
        public string StockTablekey { get; set; }

        public string DeliveryTime { get; set; }
        public string DeliveryDate { get; set; }
        public string DeliveryRemark { get; set; }
        public string DeliveryGoodStatus { get; set; }

        public List<MultiDeliveryVM> GridDetails { get; set; }

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