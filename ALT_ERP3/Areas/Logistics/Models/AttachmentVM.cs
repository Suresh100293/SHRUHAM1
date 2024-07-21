using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class AttachmentVM
    {
        public string FileName { get; set; }
        public byte[] ImageData { get; set; }
        public string FileContent { get; set; }
        public string ContentType { get; set; }
        public string ImageStr { get; set; }
        public int tempId { get; set; }
        public bool tempIsDeleted { get; set; }
        public string AllFileStr { get; set; }
        public string FileNameStr { get; set; }
        public string ChangeLog { get; set; }
        public string ParentKey { get; set; }
        public int LocationCode { get; set; }
        public string Prefix { get; set; }
        public string Srl { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public string TableKey { get; set; }
        public int SrNo { get; set; }
        public string Path { get; set; }
        public string Mode { get; set; }
        public bool HideDelete { get; set; }
        public string DocDate { get; set; }
        public string DocType { get; set; }

        public List<AttachmentVM> DocumentList { get; set; }
        public List<AttachmentVM> AllDocumentList { get; set; }
        public HttpPostedFileBase fileBase { get; set; }
        public string TypeName { get; set; }
        public bool ExternalAttach { get; set; }
        public string RefCode { get; set; }
        public string RefType { get; set; }

        public string Document { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        // iX9: used while passing parameters from MasterGrid to Option and Back 
        public string MainType { get; set; }
        public string Controller2 { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
        public string GridHtml { get; set; }
        public string LrGenerate { get; set; }
        
    }
}