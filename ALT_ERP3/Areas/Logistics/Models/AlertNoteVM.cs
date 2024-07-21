using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class AlertNoteVM
    {
        public bool AllowDelete { get; set; }
        public string RefType { get; set; }
        public string DocumentKey { get; set; }

        public bool ShortCutKey { get; set; }
        public string NextAlertFound { get; set; }
        public List<AttachmentVM> attachments { get; set; }

        public LRMaster lRMaster { get; set; }
        public List<LRMaster> LRMasterslist { get; set; }

        public LCMaster lCMaster { get; set; }
        public List<LCMaster> LCMasterslist { get; set; }

        public FMMaster fMMaster { get; set; }
        public List<FMMaster> FMMasterslist { get; set; }

        public Sales sales { get; set; }
        public List<Sales> Saleslist { get; set; }

        public Purchase purchase { get; set; }
        public List<Purchase> Purchaseslist { get; set; }

        public bool PartyNarr { get; set; }
        public string Type { get; set; }
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public string Branch { get; set; }
        public string AType { get; set; }
        public string ATypeN { get; set; }
        public string TypeCode { get; set; }
        public string Remark { get; set; }
        public bool Bling { get; set; }
        public bool Stop { get; set; }
        public string DocReceived { get; set; }
        public string DocReceivedN { get; set; }
        public string LoadingMessage { get; set; }

        public List<AlertNoteSubVM> RefersType { get; set; }
        //public List<AlertNoteSubVM> StopActivity { get; set; }

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
        public string Code { get; set; }
        
    }

    public class AlertNoteSubVM
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public bool select { get; set; }
        public bool stop { get; set; }
    }
}