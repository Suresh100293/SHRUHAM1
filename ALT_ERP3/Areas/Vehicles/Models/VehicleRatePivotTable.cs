using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Vehicles.Models
{
    public class VehicleRatePivotTable
    {
        public string ColHead { get; set; }
        public string ColField { get; set; }
        public string ColType { get; set; }
        public int ColWidth { get; set; }
        public int Decs { get; set; }
        public bool AllowEdit { get; set; }
        public bool YesTotal { get; set; }
        public bool IsHidden { get; set; }
    }
}