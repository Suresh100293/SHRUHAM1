using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Vehicles.Models
{
    public class TyreVM
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Tyre_Type { get; set; }
        public string Expiry_Date { get; set; }
        public Nullable<int> Expiry_Days { get; set; }
        public Nullable<int> KM { get; set; }
        public bool Active { get; set; }
        public string AcitveorNot { get; set; }
        public Nullable<decimal> Cost { get; set; }


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