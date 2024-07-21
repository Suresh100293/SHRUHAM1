using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class MessageLogVM
    {
        // iX9: Field Structure of MessageLog
        public int MessageLog_RECORDKEY { get; set; }
        public int MessageLog_Category { get; set; }
        public string MessageLog_Code { get; set; }
        public string MessageLog_FromIDs { get; set; }
        public System.DateTime MessageLog_mDate { get; set; }
        public string MessageLog_mDateVM { get; set; }
        public string MessageLog_Message { get; set; }
        public bool MessageLog_MessageDelete { get; set; }
        public int MessageLog_MessageID { get; set; }
        public bool MessageLog_MessageRead { get; set; }
        public System.DateTime MessageLog_mTime { get; set; }
        public string MessageLog_mTimeVM { get; set; }
        public string MessageLog_Prefix { get; set; }
        public bool MessageLog_ReplyRequest { get; set; }
        public bool MessageLog_SendNow { get; set; }
        public string MessageLog_Srl { get; set; }
        public string MessageLog_TableKey { get; set; }
        public string MessageLog_Type { get; set; }
        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public string FromIDsName { get; set; }
        public string CodeName { get; set; }
        public string CategoryName { get; set; }

        // iX9: Common default Fields
        public int Document { get; set; }
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