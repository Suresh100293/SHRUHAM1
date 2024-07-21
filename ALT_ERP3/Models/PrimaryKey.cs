using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3
{
    public class PrimaryKey
    {
        public string TABLE_QUALIFIER { get; set; }
        public string TABLE_OWNER { get; set; }
        public string TABLE_NAME { get; set; }
        public string COLUMN_NAME { get; set; }
        public Int16 KEY_SEQ { get; set; }
        public string PK_NAME { get; set; }
    }
}