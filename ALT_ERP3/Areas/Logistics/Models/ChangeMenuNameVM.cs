using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class ChangeMenuNameVM
    {
        public string ModuleName { get; set; }
        public string ID { get; set; }
        public string NewName { get; set; }
        public List<TfatMenu> Logistics { get; set; }
        public List<TfatMenu> Accounts { get; set; }
        public List<TfatMenu> Vehicles { get; set; }
    }
}