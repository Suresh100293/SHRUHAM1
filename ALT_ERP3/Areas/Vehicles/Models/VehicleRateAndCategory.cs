
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Vehicles.Models
{
    public class VehicleRateAndCategory
    {
        public string Code { get; set; }
        public string VehicleNO_Category { get; set; }
        public string To { get; set; }
        public string ToName { get; set; }
        public double KM { get; set; }
        public string Reporting { get; set; }
        //public string ReportingHH { get; set; }
        //public string ReportingMM { get; set; }
        public decimal? Rate { get; set; }
        public decimal? AdvRate { get; set; }
    }
}