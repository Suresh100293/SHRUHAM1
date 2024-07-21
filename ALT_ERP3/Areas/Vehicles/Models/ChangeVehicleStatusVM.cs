using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Vehicles.Models
{
    public class ChangeVehicleStatusVM
    {
        public bool BlockChangeStatus { get; set; }
        public string EffDate { get; set; }
        public string EFFTime { get; set; }
        public string DocDate { get; set; }
        public string Vehicle { get; set; }
        public string VehicleCode { get; set; }
        public VehicleStatus vehicleReportingSt { get; set; }
        public string VehicleStatusChangeNarr { get; set; }
        
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
    public enum VehicleStatus
    {
        [Display(Name = "Ready For Loading")]
        Ready = 0,
        [Display(Name = "Under The Maintaince")]
        Maintaince = 1,
        [Display(Name = "Driver Not Available")]
        NODriver = 2,
        [Display(Name = "Transit")]
        Transit = 3,
        [Display(Name = "Accident")]
        Accident = 4,
        [Display(Name = "Sale")]
        Sale = 5,

    }
}