using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Models
{
    public class AddOnValueVM
    {
        public string Branch { get; set; }
        public string MainType { get; set; }
        public string SubType { get; set; }
        public string TableKey { get; set; }
        public string AccPeriod { get; set; }
        public string PeriodCode { get; set; }
        public string Value { get; set; }
        public int RECORDKEY { get; set; }
        public string Fld { get; set; }
        public string CompCode { get; set; }
        public IList<AddOnValueVM> AddonList { get; set; }
        public IList<AddOnValueVM> EditonList { get; set; }

        public string Addoncode { get; set; }

        public string FldType { get; set; }

        public string Head { get; set; }

    }
}