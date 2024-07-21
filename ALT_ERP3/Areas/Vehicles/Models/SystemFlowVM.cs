using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Vehicles.Models
{
    public class SystemFlowVM
    {
        public int Days { get; set; }

        public bool GeneralUnloadReq { get; set; }
        public bool ScheduleFollowUp { get; set; }
        public bool ReworkFM { get; set; }
        public bool UnloadAllMaterialReq { get; set; }

        public List<VehiclCategoryList> categoryLists { get; set; }

        public string ArrivalAndDispatchReq { get; set; }
        public string ScheduleReq { get; set; }
        public bool ADCriteriaCheck { get; set; }

        public bool RouteCheckMaterial { get; set; }
        public bool RouteClearReq { get; set; }
        public bool DestCheckMaterial { get; set; }
        public bool DestClearReq { get; set; }
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
    public class VehiclCategoryList
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public bool ArrivalDispatchReq { get; set; }
        public bool SeheduleReq { get; set; }
    }
}