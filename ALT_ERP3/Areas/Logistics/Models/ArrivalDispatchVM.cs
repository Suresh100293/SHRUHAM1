using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class ArrivalDispatchVM
    {
        public string Fmno { get; set; }
        public string VehicleNo { get; set; }
        public string From { get; set; }
        public string Route { get; set; }
        public string To { get; set; }
        public string FMDate { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string KM { get; set; }
        public string Remark { get; set; }
        public string LCno { get; set; }
        public string TotalWeight { get; set; }
        public string TotalQty { get; set; }
        public string NoofLr { get; set; }
        public List<LCModal> lCModals { get; set; }
        public List<LRModal> lRMOdals { get; set; }
        public string ScheduleDate { get; set; }
        public string ScheduleTime { get; set; }
        public string ScheduleKM { get; set; }
    }
}