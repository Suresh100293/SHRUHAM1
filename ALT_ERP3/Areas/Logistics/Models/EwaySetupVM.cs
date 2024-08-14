using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class EwaySetupVM
    {
        public bool AllBranch { get; set; }
        public bool UserBranch { get; set; }
        public bool ConsoleForAllBranch { get; set; }
        public bool ConsoleForUserBranch { get; set; }
        public bool AutoConsole { get; set; }
        public string AutoExtendMailID { get; set; }

        public string GenSupplyType { get; set; }
        public string GenSubType { get; set; }
        public string GenDoctype { get; set; }
        public string GenTranType { get; set; }
        public string GenVehicleType { get; set; }
        public string MulReason { get; set; }
        public string BPartReason { get; set; }
        public string ExtConsignIs { get; set; }
        public string ExtTranType { get; set; }
        public string ExtReason { get; set; }
        public string CanReason { get; set; }


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