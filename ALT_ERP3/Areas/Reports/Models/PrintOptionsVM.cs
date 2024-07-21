using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3
{
    public class PrintOptionsVM
    {
        public string ReportType { get; set; }
        public string ReportLayout { get; set; }
        public string ReportPaperSize { get; set; }
        public string Reportoutput { get; set; }
        public bool EmailOnly { get; set; }
        public string EmailID { get; set; }
    }
}