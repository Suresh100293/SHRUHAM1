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
    
    public partial class VehicleMaster
    {
        public int RECORDKEY { get; set; }
        public bool Acitve { get; set; }
        public Nullable<int> AgAmt { get; set; }
        public Nullable<System.DateTime> AgDt { get; set; }
        public string AgNature { get; set; }
        public Nullable<int> BCost { get; set; }
        public string BillOrInvoiceNo { get; set; }
        public string Branch { get; set; }
        public string Broker { get; set; }
        public string PostAc { get; set; }
        public Nullable<int> CCost { get; set; }
        public string ChassisNo { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public string Dealer { get; set; }
        public string Driver { get; set; }
        public Nullable<System.DateTime> EMIDate { get; set; }
        public string EngineNo { get; set; }
        public string Fin { get; set; }
        public Nullable<int> FinAmt { get; set; }
        public string FMonth { get; set; }
        public Nullable<int> InsAmt { get; set; }
        public string InsCo { get; set; }
        public Nullable<int> IntAmt { get; set; }
        public Nullable<int> IntRate { get; set; }
        public string Inv { get; set; }
        public Nullable<int> InvAmt { get; set; }
        public int KM { get; set; }
        public Nullable<System.DateTime> LastEmiDate { get; set; }
        public bool MaintainDriverAC { get; set; }
        public string MasCode { get; set; }
        public Nullable<int> MonIns { get; set; }
        public string Owner { get; set; }
        public int PayLoad { get; set; }
        public string PermitNo { get; set; }
        public bool PickVehicleRate { get; set; }
        public bool GetParentRateAlso { get; set; }
        public string PolicyNo { get; set; }
        public Nullable<int> PreAmt { get; set; }
        public Nullable<System.DateTime> PurDt { get; set; }
        public string RateType { get; set; }
        public bool ScheduleDateTime { get; set; }
        public bool ScheduleKM { get; set; }
        public string Status { get; set; }
        public string TMonth { get; set; }
        public Nullable<int> TripChg { get; set; }
        public string TruckNo { get; set; }
        public string TruckStatus { get; set; }
        public string VCategory { get; set; }
        public string ShortName { get; set; }
        public bool RemarkReq { get; set; }
        public string Remark { get; set; }
        public bool HoldActivityReq { get; set; }
        public string HoldRemark { get; set; }
        public bool ARAP { get; set; }
        public string CrAc { get; set; }
        public string DrAc { get; set; }
        public bool PostReq { get; set; }
        public string DriCrAc { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public bool SplitPosting { get; set; }
        public int NoOfTyres { get; set; }
        public int Stepney { get; set; }
        public bool FMVOURELReq { get; set; }
        public bool DriverAdvancePayable { get; set; }
        public string Code { get; set; }
        public Nullable<System.DateTime> FitnessExp { get; set; }
        public Nullable<System.DateTime> InsuranceExp { get; set; }
        public Nullable<System.DateTime> PUCExp { get; set; }
        public Nullable<System.DateTime> AIPExp { get; set; }
        public Nullable<System.DateTime> StateTaxExp { get; set; }
        public Nullable<System.DateTime> TPStateExp { get; set; }
        public Nullable<System.DateTime> GreenTaxExp { get; set; }
        public bool PickDriverTripRate { get; set; }
        public bool ChangeVehicleFreight_Advance { get; set; }
        public bool ChangeDriverFreight_Advance { get; set; }
    }
}
