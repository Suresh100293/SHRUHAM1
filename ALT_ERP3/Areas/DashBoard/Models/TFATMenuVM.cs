using System;

namespace ALT_ERP3.Areas.DashBoard.Models
{
    public class TFATMenuVM
    {
        public int IID { get; set; }
        public int RECORDKEY { get; set; }
        public short ID { get; set; }
        public string Menu { get; set; }
        public bool AllowClick { get; set; }
        public int DisplayOrder { get; set; }
        public string ParentMenu { get; set; }
        public string SubType { get; set; }
        public string MainType { get; set; }
        public bool AutoGenerate { get; set; }
        public bool Hide { get; set; }
        public string FormatCode { get; set; }
        public string Controller { get; set; }
        public byte Level { get; set; }
        public string TableName { get; set; }
        public string ModuleName { get; set; }
        public bool IsDone { get; set; }
        public bool QuickMenu { get; set; }
        public bool QuickMaster { get; set; }
        public string OptionType { get; set; }
        public string OptionCode { get; set; }
        public string Controller2 { get; set; }
        public bool AllowAdd { get; set; }
        public bool AllowEdit { get; set; }
        public bool AllowDelete { get; set; }
        public bool AllowPrint { get; set; }

        public string Message { get; set; }
        public string FromIDs { get; set; }
        public DateTime mDate { get; set; }
        public string ZoomURL { get; set; }

    }
}