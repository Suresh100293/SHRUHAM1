using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3
{
    public class ForeignKey
    {
        public string PKTABLE_QUALIFIER { get; set; }
        public string PKTABLE_OWNER { get; set; }
        public string PKTABLE_NAME { get; set; }
        public string PKCOLUMN_NAME { get; set; }
        public string FKTABLE_QUALIFIER { get; set; }
        public string FKTABLE_OWNER { get; set; }
        public string FKTABLE_NAME { get; set; }
        public string FKCOLUMN_NAME { get; set; }
        public Int16 KEY_SEQ { get; set; }
        public Int16 UPDATE_RULE { get; set; }
        public Int16 DELETE_RULE { get; set; }
        public string FK_NAME { get; set; }
        public string PK_NAME { get; set; }
        public Int16 DEFERRABILITY { get; set; }
    }
}