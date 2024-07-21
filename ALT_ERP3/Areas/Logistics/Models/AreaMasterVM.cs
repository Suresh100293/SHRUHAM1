using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class AreaMasterVM
    {
        public string Name { get; set; }
        public string Grp { get; set; }
        public string Grp_Name { get; set; }
        public bool Active { get; set; }
        public string State { get; set; }
        public string StateName { get; set; }
        public string Country { get; set; }
        public string CountryName { get; set; }
        public string City { get; set; }
        public string CityName { get; set; }
        public string Pincode { get; set; }


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
        public string Code { get; set; }
        public bool BranchMail { get; set; }
        public bool LocalMail { get; set; }
    }
}