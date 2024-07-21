using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class LcDetailsVM
    {
        public string Branch { get;  set; }
        public bool OtherBranch { get;  set; }
        public int Lrno { get;  set; }
        public string LRKey { get;  set; }
        public int Lcno { get;  set; }
        public string From { get;  set; }
        public string To { get;  set; }
        public string Consignor { get;  set; }
        public string Consignee { get;  set; }
        public string ChrgeType { get;  set; }
        public string Description { get;  set; }
        public string Unit { get;  set; }
        public string LrType { get;  set; }
        public string LRDelivery { get;  set; }
        public string LRColln { get;  set; }
        public string EditLDC { get;  set; }
        public double ChrWeight { get;  set; }
        public double ActWeight { get;  set; }

        public int Qty { get;  set; }
        public int TransitQty { get;  set; }
        public int LoadGQty { get;  set; }
        public int LoadDQty { get;  set; }
        public double LRActWeight { get;  set; }
        public int Amount { get;  set; }
        public string PickType { get;  set; }
        public string recordekey { get;  set; }
        public bool EditLDSNo { get; set; }
        public string Date { get;  set; }
        public string Time { get;  set; }
        public string remark { get;  set; }
        public bool ShowAmountColumn { get; set; }
        public bool ShowLoadQtyANdWeight { get; set; }
        public string LRMode { get; set; }
        public DateTime LRDATESTK { get; set; }

        public string Authenticate { get; set; }
    }
}