using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3
{
    public class GridColumn
    {
        public string name { get; set; }
        public string index { get; set; }
        public string width { get; set; }
        //public string align { get; set; }
        public bool key { get; set; }
        public bool sortable { get; set; }
        public bool frozen { get; set; }
        public bool editable { get; set; }
        public string edittype { get; set; }
        public bool hidden { get; set; }
        public string editoptions { get; set; }
        public string align { get; set; }
        public string formatoptions { get; set; }
        public string formatter { get; set; }
        public string summaryTpl { get; set; }
        public string summaryType { get; set; }
        public bool filterable { get; set; }
        public bool search { get; set; }
    }
}