using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Vehicles.Models
{
    public class NewVehiclesRatesVM
    {
        public string CopyBranch { get; set; }
        public string CopyBranchName { get; set; }
        public string typeofRate { get; set; }
        public string VehicleNo_Category { get; set; }
        public string From { get; set; }
        public string FromName { get; set; }
        public List<VehicleRateAndCategory> VehiclesList { get; set; }
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