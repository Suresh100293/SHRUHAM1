using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class AccountGroupsVM
    {
        // iX9: Field Structure of MasterGroups
        public int MasterGroups_RECORDKEY { get; set; }
        public string MasterGroups_AcType { get; set; }
        public string MasterGroups_AliasPrefix { get; set; }
        public string MasterGroups_BaseGr { get; set; }
        public string MasterGroups_Code { get; set; }
        public int MasterGroups_DisplayOrder { get; set; }
        public bool MasterGroups_ForceCC { get; set; }
        public string MasterGroups_Grp { get; set; }
        public int MasterGroups_GrpKey { get; set; }
        public bool MasterGroups_Hide { get; set; }
        public bool MasterGroups_IsLast { get; set; }
        public int MasterGroups_Level { get; set; }
        public string MasterGroups_Name { get; set; }
        public bool MasterGroups_NoDetails { get; set; }
        public int MasterGroups_PCCode { get; set; }
        public string MasterGroups_Prefix { get; set; }
        public string MasterGroups_RevGroup { get; set; }
        public string MasterGroups_Sch { get; set; }
        public int MasterGroups_Seq { get; set; }
        public string MasterGroups_StructureCode { get; set; }
        public bool MasterGroups_SystemCode { get; set; }
        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public List<SelectListItem> BaseGrList { get; set; }
        public string GrpName { get; set; }
        public string RevGroupName { get; set; }

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