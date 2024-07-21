using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class MyMenuVM
    {
        // iX9: Field Structure of TfatMenu
        public int TfatMenu_RECORDKEY { get; set; }
        public bool TfatMenu_AllowClick { get; set; }
        public bool TfatMenu_AutoGenerate { get; set; }
        public string TfatMenu_Controller { get; set; }
        public int TfatMenu_DisplayOrder { get; set; }
        public string TfatMenu_FormatCode { get; set; }
        public bool TfatMenu_Hide { get; set; }
        public int TfatMenu_ID { get; set; }
        public bool TfatMenu_IsDone { get; set; }
        public byte TfatMenu_Level { get; set; }
        public string TfatMenu_MainType { get; set; }
        public string TfatMenu_Menu { get; set; }
        public string TfatMenu_ModuleName { get; set; }
        public string TfatMenu_OptionCode { get; set; }
        public string TfatMenu_OptionType { get; set; }
        public string TfatMenu_ParentMenu { get; set; }
        public bool TfatMenu_QuickMaster { get; set; }
        public bool TfatMenu_QuickMenu { get; set; }
        public string TfatMenu_SubType { get; set; }
        public bool TfatMenu_SystemDefault { get; set; }
        public string TfatMenu_TableName { get; set; }
        public string TfatMenu_ZoomURL { get; set; }
        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public List<SelectListItem> ModuleNameList { get; set; }
        public List<SelectListItem> OptionTypeList { get; set; }
        public List<SelectListItem> ParentMenuList { get; set; }

        // iX9: Common default Fields
        public int Document { get; set; }
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