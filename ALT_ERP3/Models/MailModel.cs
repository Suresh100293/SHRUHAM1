using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Models
{
    public class MailModel
    {
        public string Body { get; set; }
        public string Subject { get; set; }
        public string From { get; set; }
        public string SelfEmailID { get; set; }
        public string To { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
        public string LogHeader { get; set; }
        public string LogNarr { get; set; }
    }
}