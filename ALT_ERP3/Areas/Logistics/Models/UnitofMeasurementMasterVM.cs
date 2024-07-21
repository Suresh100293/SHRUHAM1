using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class UnitofMeasurementMasterVM
    {
        // iX9: Field Structure of UnitMaster
        public int UnitMaster_RECORDKEY { get; set; }
        public string UnitMaster_Code { get; set; }
        public decimal UnitMaster_Factor1 { get; set; }
        public decimal UnitMaster_Factor2 { get; set; }
        public bool UnitMaster_Hide { get; set; }
        public decimal UnitMaster_Lvl { get; set; }
        public string UnitMaster_Name { get; set; }
        public int UnitMaster_NoOfDecimal { get; set; }
        public string UnitMaster_Operator1 { get; set; }
        public string UnitMaster_Operator2 { get; set; }
        public bool UnitMaster_Type { get; set; }

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