﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Vehicles.Models
{
    public class LocalKmAndTimeVM
    {
        public bool BrnachOnly { get; set; }
        public bool Supress { get; set; }
        public string Branch { get; set; }
        public string BranchName { get; set; }
        public string BranchType { get; set; }

        public string ID { get; set; }
        public string GRP { get; set; }
        public string Area { get; set; }
        public string KM { get; set; }
        public string Time { get; set; }
        public string Category { get; set; }
        public string ParentOfID { get; set; }

        public bool copy { get; set; }
        public string copyDocument { get; set; }

        public List<LocalKmAndTimeVM> list { get; set; }

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
        public dynamic Controller { get; internal set; }
        public dynamic ViewName { get; internal set; }
        public object AcType { get; internal set; }
    }
    public class LocalKMTimeCopyVM
    {
        public string DocumentNo { get; set; }
        public string FromBranch { get; set; }
    }
}