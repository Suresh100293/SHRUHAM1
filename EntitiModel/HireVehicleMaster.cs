//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EntitiModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class HireVehicleMaster
    {
        public int RECORDKEY { get; set; }
        public bool Acitve { get; set; }
        public string Branch { get; set; }
        public string Broker { get; set; }
        public string Driver { get; set; }
        public string ENTEREDBY { get; set; }
        public string ContactNo { get; set; }
        public int KM { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public int PayLoad { get; set; }
        public string Status { get; set; }
        public string TruckNo { get; set; }
        public string TruckStatus { get; set; }
        public string VCategory { get; set; }
        public bool SpclRemarkReq { get; set; }
        public string SpclRemark { get; set; }
        public bool BlackListReq { get; set; }
        public string BlackListRemark { get; set; }
        public string Remark { get; set; }
        public string ShortName { get; set; }
        public string CrAc { get; set; }
        public string DrAc { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Code { get; set; }
        public bool PickVehicleRate { get; set; }
        public bool GetParentRateAlso { get; set; }
        public string RateType { get; set; }
        public bool PickDriverTripRate { get; set; }
        public bool ChangeVehicleFreight_Advance { get; set; }
        public bool ChangeDriverFreight_Advance { get; set; }
    }
}
