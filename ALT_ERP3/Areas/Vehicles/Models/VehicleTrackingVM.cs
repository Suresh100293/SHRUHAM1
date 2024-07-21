using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Vehicles.Models
{
    public class VehicleTrackingVM
    {
        #region FM Tracking
        public bool AllTime { get; set; }
        public int UptoDays { get; set; }
        public bool UptoDaysReq { get; set; }
        public bool OnlySchedule { get; set; }
        public int ScheduleAndUptoDays { get; set; }
        public bool ScheduleAndUptoDaysReq { get; set; }
        #endregion

        #region Vehicle Activity
        public bool VA_AllTime { get; set; }
        public int VA_UptoDays { get; set; }
        public bool VA_UptoDaysReq { get; set; }
        public bool VA_CompleteReq { get; set; }
        #endregion

        #region Consignment Tracking
        public bool CT_AllTime { get; set; }
        public int CT_UptoDays { get; set; }
        public bool CT_UptoDaysReq { get; set; }
        public bool CT_DeliveryReq { get; set; }
        #endregion

        #region Vehicle Master
        public bool VM_AllTime { get; set; }

        #endregion

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