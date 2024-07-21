using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class ZoneBranchAreaCombineModel
    {
        
        public string Name { get; set; }
        public string Desc { get; set; }
        public int Id { get; set; }
        public string ParentId { get; set; }
        public string ParentName { get; set; }
        public string ShortName { get; set; }
        public string GstNo { get; set; }
        public string GstState { get; set; }
        public string MobileNo { get; set; }
        public string Address { get; set; }
        public EnumStatus Status { get; set; }
        public string ViewName { get; set; }
        public string Mode { get; set; }
        public string AllowButtons { get; set; }
        public List<string> OfficeTypeList { get; set; }
        public string OfficeType { get; set; }
    }
    public enum EnumStatus
    {
        Active=1, DeActive=2
    }

}