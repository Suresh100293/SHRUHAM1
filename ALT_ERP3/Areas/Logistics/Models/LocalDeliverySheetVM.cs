using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class LocalDeliverySheetVM
    {
        public LDSsetup LDSsetup { get; set; }
        public  LCVM LCVM { get; set; }
        public  List<LcDetailsVM> lcDetailsVMs { get; set; }
        public  List<LR_LC_Combine_VM> AllRoute { get; set; }
        public  FMVM fMVM { get; set; }
        public FMAttachment Attachment { get; set; }
        public List<FMAttachment> attachments { get; set; }

        public int LDSNo { get; set; }
        public string Time { get; set; }
        public string Date { get; set; }
        public string VehicleType { get; set; }
        public string VehicleTypeName { get; set; }
        public string VehicleNo { get; set; }
        public string Broker { get; set; }
        public string BrokerName { get; set; }
        public string KM { get; set; }
        public string From { get; set; }
        public string FromName { get; set; }
        public string DestCombo { get; set; }
        public string DestCombo_Name { get; set; }
        public string DestCombo_Sequence { get; set; }
        public string To { get; set; }
        public string ToName { get; set; }
        public List<SelectListItem> Branches { get; set; }
        public string AppBranch { get; set; }
        public string PayLoadL { get; set; }
        public string AppBranchL { get; set; }
        public string VehicleCategory { get; set; }
        public string VehicleCategoryName { get; set; }
        public string ReceiptNo { get; set; }
        public string DriverName { get; set; }
        public string LicenceNo { get; set; }
        public string LicenceExpDate { get; set; }
        public string Owner { get; set; }
        public string ChallanNo { get; set; }
        public string ContactNo { get; set; }
        public string Freight { get; set; }
        public string Advance { get; set; }
        public string Balance { get; set; }
        public string Remark { get; set; }
        public string AttachmentCode { get; set; }
        public string PayableAt { get; set; }
        public string PayableAtName { get; set; }
        public string PayLoad { get; set; }
        public string Loaded { get; set; }
        public string AvailablePayload { get; set; }
        public string OverLoadLoad { get; set; }

        public string LRNoList { get; set; }
        public string LoadQuantity { get; set; }
        public string LoadWeight { get; set; }
        public string Amount { get; set; }
        public string PickType { get; set; }
        public string RecordKey { get; set; }


        public DateTime DocDate { get; set; }
        public Nullable<System.DateTime> MinDate { get; set; }
        public Nullable<System.DateTime> MaxDate { get; set; }

        public bool VehicleUpdateFlag { get; set; }

        public string Note { get; set; }
        public bool All { get; set; }
        public bool Dispach { get; set; }
        public bool Delivery { get; set; }
        public bool Billing { get; set; }
        public string LockRemark { get; set; }




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


        public string NewFromName { get; set; }
        public string NewToName { get; set; }
        public List<SelectListItem> BranchList { get; set; }
        public string ParentOfTo { get; set; }
        public string Generate { get; set; }
    }

    
}