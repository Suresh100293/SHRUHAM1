using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class UserRightsVM
    {
        // iX9: Field Structure of UserRights
        public int UserRights_RECORDKEY { get; set; }
        public string UserRights_Branch { get; set; }
        public string UserRights_Code { get; set; }
        public string UserRights_CompCode { get; set; }
        public int UserRights_MenuID { get; set; }
        public bool UserRights_xAdd { get; set; }
        public bool UserRights_xBackDated { get; set; }
        public bool UserRights_xCess { get; set; }
        public bool UserRights_xDelete { get; set; }
        public bool UserRights_xEdit { get; set; }
        public decimal UserRights_xLimit { get; set; }
        public bool UserRights_xPrint { get; set; }
        public string Modulename { get; set; }
        public string ParentName { get; set; }
        public string ZoomUrl { get; set; }

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

        // iX9: special lists used for Grid2Grid relation type of interface
        public string TfatPass_Code { get; set; }
        public string TfatPass_Name { get; set; }
        public string TfatMenu_Menu { get; set; }

        public List<UserRightsVM> mLeftList { get; set; }
        public List<UserRightsVM> mRightList { get; set; }
        public string Account { get; set; }
        public string AccountName { get; set; }

        public string EnumModuleName { get; set; }
    }

}