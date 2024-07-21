using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace ALT_ERP3.Areas.Vehicles.Models
{
    public class DriverMasterVM
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Nick_Name { get; set; }
        public string MobileNo1 { get; set; }
        public string MobileNo2 { get; set; }
        public string Guaranter { get; set; }
        public string Reference { get; set; }
        public string LicenceNo { get; set; }
        public string LicenceExpDate { get; set; }
        public string ZoneName { get; set; }
        public string ZoneCode { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string Posting { get; set; }
        public string PostingName { get; set; }
        public string Ticklers { get; set; }
        public string HoldTicklers { get; set; }
        public IEnumerable<SelectListItem> ValidProoflist { get; set; }

        public string DriverStatusChangeNarr { get; set; }



        //public IEnumerable<SelectListItem> ZoneList { get; set; }
        //public IEnumerable<SelectListItem> BranchList { get; set; }
        public string ValidProof { get; set; }
        public string ProofNo { get; set; }
        public string VehicleNo { get; set; }
        public string VehicleNoN { get; set; }
        public bool Active { get; set; }
        // Attachment File
        public AttachmentDocumentVM attachmentDocuments { get; set; }
        public List<AttachmentDocumentVM> PersonalDocumentslist { get; set; }
        public List<AttachmentDocumentVM> LicencedocumentList { get; set; }
        public List<AttachmentDocumentVM> AttachmentList { get; set; }

        //public DriverAttachment driverAttachment { get; set; }
        //public List<DriverAttachment> driverAttachments { get; set; }


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