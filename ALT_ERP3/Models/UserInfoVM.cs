using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3
{
    public partial class UserInfoVM
    {
        public int RECORDKEY { get; set; }
        public bool AllowItem { get; set; }
        public bool AllowParty { get; set; }
        public string DeviceID { get; set; }
        public bool DeviceStatus { get; set; }
        public string EmailID { get; set; }
        public string IDs { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Prefix { get; set; }
        public bool Remember { get; set; }
    }
}