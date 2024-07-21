using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TFATERPWebApplication.Areas.Logistics.Models
{
    public class DraftVM
    {
        public string Document { get; set; }
        //public string Mode { get; set; }
        public string Name { get; set; }
        public bool DraftFlag { get; set; }

        public string DraftName { get; set; }
        public string Mode { get; set; }
        public string DocDate { get; set; }
        public string RecCode { get; set; }
        public string SendCode { get; set; }
        public string Source { get; set; }
        public string Dest { get; set; }
        public int PackQty { get; set; }
    }
}