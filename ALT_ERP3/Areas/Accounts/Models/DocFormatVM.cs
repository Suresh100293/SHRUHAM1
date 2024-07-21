using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class DocFormatVM
    {
        public bool AttachDocs { get; set; }
        public string DocHandle { get; set; }
        public string FormatCode { get; set; }
        public bool ItemAttach { get; set; }
        public string OutputDevice { get; set; }
        public bool SendEmail { get; set; }
        public int Sno { get; set; }
        public string StoredProc { get; set; }
        public bool Selected { get; set; }
        // iX9: Common default Fields
        public string Document { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Mode { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }
        // iX9: used while passing parameters from MasterGrid to Option and Back 
        public string MainType { get; set; }
        public string Controller2 { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
        public string EmailTemplate { get; set; }

        // iX9: Fields for GridView
        public bool tempIsDeleted { get; set; }
        public int tEmpID { get; set; }
        public List<DocFormatVM> DocFormatList { get; set; }
        public int LocationCode { get; set; }
        public string Prefix { get; set; }
        public string Srl { get; set; }

    }
}