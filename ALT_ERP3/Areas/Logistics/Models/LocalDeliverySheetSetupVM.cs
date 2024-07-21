﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class LocalDeliverySheetSetupVM
    {
        public AscDescLDS SelectColumn { get; set; }
        public bool Asc { get; set; }
        public bool Desc { get; set; }
        public int Days { get; set; }

        public bool LDSAutomatic { get; set; }
        public bool LDSManual { get; set; }
        public bool Both { get; set; }

        public bool LDS_Date { get; set; }
        public int Before_LDS_Date { get; set; }
        public int After_LDS_Date { get; set; }

        public int EditHours { get; set; }
        public int DeleteHours { get; set; }
        

        public bool FreightCalculate { get; set; }

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
    public enum AscDescLDS
    {
        DocumentDate, LDSNo
    }
}