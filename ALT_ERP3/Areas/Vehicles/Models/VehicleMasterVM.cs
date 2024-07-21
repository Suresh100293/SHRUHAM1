using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Vehicles.Models
{
    public class VehicleMasterVM
    {
        public string VehicleStatusChangeNarr { get; set; }

        public bool TrackButtonReq { get; set; }
        public string TrackErrorMsg { get; set; }

        public string Code { get; set; }
        public string Vehicle_No { get; set; }
        public string Owner_Name { get; set; }

        public int NoofTyres { get; set; }
        public int NoOfSpepni { get; set; }

        public string Chassis_No { get; set; }
        public string Driver { get; set; }
        public string Driver_Name { get; set; }
        public DateTime Purchase_Date { get; set; }
        public string BillOrInvoice_No { get; set; }
        public string Chassis_Cost { get; set; }
        public string Financer_Name { get; set; }
        public int Agreement_Amount { get; set; }
        public int Intrest_Rate { get; set; }
        public string Insurance_Co { get; set; }
        public int Insured_Amount { get; set; }
        public string VehicleGroup { get; set; }
        public string VehicleGroup_Name { get; set; }
        public bool Acitve { get; set; }
        //public List<string> Truck_Status_List { get; set; }
        public string Vehicle_Category { get; set; }
        public int PayLoad { get; set; }
        public string Vehicle_Category_Name { get; set; }
        public string Broker { get; set; }
        public string Broker_Name { get; set; }
        public string Permit_No { get; set; }
        public string Engine_No { get; set; }
        public string Dealer_Name { get; set; }
        public int Invoice_Amount { get; set; }
        public int Body_Cost { get; set; }
        public string Agreement_nature { get; set; }
        public int Financed_Amount { get; set; }
        public DateTime Date { get; set; }
        public int Intrest_Amount { get; set; }
        public string Policy_No { get; set; }
        public int Premium_Amount { get; set; }
        public DateTime Last_Emi_Date { get; set; }
        public int KM { get; set; }
        public PickVehicleRate Pick_Vehicle_Rate_From_Category { get; set; }
        public bool Pick_Vehicle_Rate { get; set; }
        public bool PickDriverTripRate { get; set; }
        public bool ChangeVehicleFreight_Advance { get; set; }
        public bool ChangeDriverFreight_Advance { get; set; }
        public bool Vehicle_Rate { get; set; }
        public bool Category_Rate { get; set; }
        public bool GetParentAlso { get; set; }
        public string Category { get; set; }
        public VehicleReportingst vehicleReportingSt { get; set; }

        public string Branch { get; set; }
        public bool ShortCutKey { get; set; }
        public bool ScheduleDate_Time { get; set; }
        public bool ScheduleKM { get; set; }

        public bool MaintainDriverAc { get; set; }
        public bool PostingSplitAsperChargeMaster { get; set; }

        public string VehiPostAc { get; set; }
        public string VehiPostAcName { get; set; }

        public bool DriverAdvancePayable { get; set; }
        public bool FMVOUREL { get; set; }

        public string Remark { get; set; }
        public bool SpecialRemarkReq { get; set; }
        public string SpecialRemark { get; set; }
        public bool BalckListReq { get; set; }
        public string BalckListRemark { get; set; }
        public string DriverContactNo { get; set; }

        public string ShortName { get; set; }

        public bool PostingReq { get; set; }
        public string DebitAcName { get; set; }
        public string DebitAcCode { get; set; }
        public string CreditAccount { get; set; }
        public bool MaintainCreditorRecord { get; set; }
        //public string BranchL { get; set; }
        public List<ExpenseseOfVehicle> expeselist { get; set; }
        public string DriCrAc { get; set; }
        public string DriCrAcN { get; set; }


        public string FitnessExp { get; set; }
        public string InsuranceExp { get; set; }
        public string PUCExp { get; set; }
        public string AIPExp { get; set; }
        public string StateTaxExp { get; set; }
        public string TPStateExp { get; set; }
        public string GreenTaxExp { get; set; }






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
    public enum PickVehicleRate
    {
        Yes = 0, No = 1
    }
    public enum VehicleReportingst
    {
        [Display(Name = "Ready For Loading")]
        Ready = 0,
        [Display(Name = "Under The Maintaince")]
        Maintaince = 1,
        [Display(Name = "Driver Not Available")]
        NODriver = 2,
        [Display(Name = "Transit")]
        Transit = 3,
        [Display(Name = "Accident")]
        Accident = 4,
        [Display(Name = "Sale")]
        Sale = 5,
    }
    public class ExpenseseOfVehicle
    {
        public string Code { get; set; }
        public DateTime ToDt { get; set; }
    }
}