using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Models
{
    public class AddNoteVM
    {
        public int RECORDKEY { get; set; }
        public string Branch { get; set; }
        public string Narr { get; set; }
        public string NarrRich { get; set; }
        public string Prefix { get; set; }
        public int Sno { get; set; }
        public string Srl { get; set; }
        public string Type { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public int LocationCode { get; set; }
        public string TableKey { get; set; }
        public IList<AddNoteVM> AddNote { get; set; }
    }
}