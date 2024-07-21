using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Vehicles.Models
{
    public class ListOfRatesModelVM
    {
        public string ToD { get; set; }
        public string ToDName { get; set; }
        public decimal KM { get; set; }
        public decimal Rate { get; set; }
        public decimal AdvRate { get; set; }
        public decimal Reporting { get; set; }
    }
}