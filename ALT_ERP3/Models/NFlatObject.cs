using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Models
{
    public class NFlatObject
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
        public string data { get; set; }
        public bool isSelected { get; set; }
    }
}