using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Vehicles.Models
{
    public class AttachmentDocumentVM
    {
        public string AttachmentCode { get; set; }
        public string TypeOfAttachment { get; set; }
        public HttpPostedFileBase PersonalPhoto { get; set; }
        public HttpPostedFileBase LicencePhoto { get; set; }
        public HttpPostedFileBase DocumentPhoto { get; set; }
        public byte[] Image { get; set; }

        public long RecordKey { get; set; }
        public string Code { get; set; }
        public string FileName { get; set; }
        public string ParentCode { get; set; }
        public string DocumentString { get; set; }
        public string ContentType { get; set; }
        public string ENTEREDBY { get; set; }
        public string LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
    }
}