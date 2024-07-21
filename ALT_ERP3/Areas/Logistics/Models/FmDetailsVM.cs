using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class FmDetailsVM
    {
        public string Branch { get; set; }
        public List<ArrivalDetailsVM> ArrivalDetails { get; set; }
        public List<DispatchDetailsVM> DispatchDetails { get; set; }
        public List<LorryChallanVM> LoadingDetails { get; set; }
        public List<LorryChallanVM> UnLoadingDetails { get; set; }

    }

    public class ArrivalDetailsVM
    {
        public string Area { get; set; }
        public string Date { get; set; }
        public string KM { get; set; }
        public string Remark { get; set; }
    }
    public class DispatchDetailsVM
    {
        public string Area { get; set; }
        public string Date { get; set; }
        public string KM { get; set; }
        public string Remark { get; set; }
    }
    public class LorryChallanVM
    {
        public string LcNo { get; set; }
        public string Date { get; set; }
        public string From { get; set; }
        public string TO { get; set; }
        public string Qty { get; set; }
        public string Weight { get; set; }
        public List<LorryReVM> LorryReVMs { get; set; }
    }
    public class LorryReVM
    {
        public string Srno { get; set; }
        public string LRNo { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Qty { get; set; }
        public string Weight { get; set; }
        public string Particular { get; set; }
    }
}