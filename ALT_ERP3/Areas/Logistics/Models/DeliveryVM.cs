using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class DeliveryVM
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public bool PeriodLock { get; set; }
        #region Basic Details Variables
        public string DeliveryNo { get; set; }
        public string Time { get; set; }
        public string Date { get; set; }

        public string DelGenerate { get; set; }


        public string ConsignerCode { get; set; }
        public string ConsignerName { get; set; }
        public string ConsigneeName { get; set; }
        public string ConsigneeCode { get; set; }
        public string From { get; set; }
        public string FromName { get; set; }
        public string To { get; set; }
        public string ToName { get; set; }
        public int LRQty { get; set; }
        public int LRBalQty { get; set; }
        public double LRWeight { get; set; }
        public double LRBalWeight { get; set; }
        public int DeliverQty { get; set; }
        public double DeliverWeight { get; set; }
        public string Lrno { get; set; }
        public string DeliveryTime { get; set; }
        public string DeliveryDate { get; set; }
        public string DeliveryRemark { get; set; }
        public string PODNo { get; set; }
        public string PODTime { get; set; }
        public string PODDate { get; set; }
        public string PODRemark { get; set; }
        public string DeliveryGoodStatus { get; set; }
        public int ShortQty { get; set; }
        public int BillQty { get; set; }
        public string VehicleNo { get; set; }
        public string VehicleNoName { get; set; }
        public string PersonName { get; set; }
        public string MobileNO { get; set; }
        public string Remark { get; set; }

        

        #endregion


        public string Parentkey { get; set; }
        public DateTime DocDate { get; set; }
        public Nullable<System.DateTime> MinDate { get; set; }
        public Nullable<System.DateTime> MaxDate { get; set; }
        public string RecordKey { get; set; }
        public string AttachmentCode { get; set; }
        public string POD { get; set; }

        public DeliverySetup DeliverySetup { get; set; }
        public FMAttachment Attachment { get; set; }
        public List<FMAttachment> attachments { get; set; }
        public LCVM All { get; set; }
        public LCVM CurrentBranch { get; set; }
        
        public List<DelRetion> DelRetions { get; set; }

        public bool OtherBranchLr { get; set; }
        public bool CurrentBranchLR { get; set; }
        public bool LockAuthorise { get; set; }
        //public bool PODCreateAtDeliveryTime { get; set; }
        //public bool DeletePODWithDelivery { get; set; }

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



    public class DelRetion
    {
        public bool BlockDelivery { get; set; }
        public string ParentKey { get; set; }
        public string StkBranch { get; set; }
        public string StkBranchN { get; set; }
        public string Type { get; set; }
        public int DelQty { get; set; }
        public int DelBalQty { get; set; }
        public double DelWeight { get; set; }
        public double DelBalWeight { get; set; }
        public string Authorise { get; set; }

        //public List<SelectListItem> Branches { get; set; }
        //public string ReverseStkArea { get; set; }
    }
}