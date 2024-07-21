using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class DashboardObjectsVM
    {
        // iX9: Field Structure of ActiveObjects
        public int ActiveObjects_RECORDKEY { get; set; }
        public string ActiveObjects_Code { get; set; }
        public double ActiveObjects_Height { get; set; }
        public string ActiveObjects_Modules { get; set; }
        public string ActiveObjects_Name { get; set; }
        public string ActiveObjects_ObjectType { get; set; }
        public string ActiveObjects_Query { get; set; }
        public string ActiveObjects_ReportCode { get; set; }
        public string ActiveObjects_SizeType { get; set; }
        public bool ActiveObjects_Status { get; set; }
        public string ActiveObjects_Users { get; set; }
        public double ActiveObjects_Width { get; set; }
        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public List<SelectListItem> ObjectTypeList { get; set; }
        public List<SelectListItem> ModulesList { get; set; }
        public List<SelectListItem> SizeTypeList { get; set; }
        public List<SelectListItem> UsersMultiX { get; set; }
        public string UsersItemX { get; set; }

        // iX9: Common default Fields
        public string Document { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Mode { get; set; }

        // iX9: used while passing parameters from MasterGrid to Option and Back 
        public string MainType { get; set; }
        public string Controller2 { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
    }
}