using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Vehicles.Models
{
    public class ChangeDriverStatusVM
    {
        public string EffDate { get; set; }
        public string refParentKey { get; set; }
        public string EFFTime { get; set; }
        public string DocDate { get; set; }

        public string Driver { get; set; }
        public string DriverCode { get; set; }
        public string VehicleNo { get; set; }
        public string VehicleNoCode { get; set; }
        public string MobileNo1 { get; set; }
        public string MobileNo2 { get; set; }
        public string Guaranter { get; set; }
        public string LicenceNo { get; set; }
        public string LicenceExpDate { get; set; }
        public string DriverStatusChangeNarr { get; set; }

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