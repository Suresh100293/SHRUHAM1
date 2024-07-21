using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Models
{
    public class BarChartVM
    {
        public BarChartVM()
        {
            labels = new List<string>();
            datasets = new List<BarChartChildVM>();
        }
        public List<string> labels { get; set; }

        public List<BarChartChildVM> datasets { get; set; }
    }
    public class BarChartChildVM
    {

        public BarChartChildVM()
        {
            data = new List<decimal>();
        }

        public string label { get; set; }
        public string backgroundColor { get; set; }
        public string borderColor { get; set; }
        public int borderWidth { get; set; }
        public string hoverbackgroundColor { get; set; }
        public string hoverborderColor { get; set; }

        public List<decimal> data { get; set; }
    }
}