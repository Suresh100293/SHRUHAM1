using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class PODSendVM
    {
        public bool GlobalSearch { get; set; }

        public List<ListOfPod> PODSendList { get; set; }
        public string SearchLrnoForPodSend { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string CustoCode { get; set; }
        public string CustoName { get; set; }
        public string Task { get; set; }
        public string RecordKEy { get; set; }
        public int PODNo { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public bool BlockPOD { get; set; }

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
    }
    public class ListOfPod
    {
        public int Sno { get; set; }
        public int PODNO { get; set; }
        public string Lrno { get; set; }
        public string LRTime { get; set; }
        public string LRDate { get; set; }
        public string ConsignerName { get; set; }
        public string ConsigneeName { get; set; }
        public string FromName { get; set; }
        public string ToName { get; set; }
        public string Remark { get; set; }
        public string RecePODRemark { get; set; }
        public bool CheckBox { get; set; }
        public string Authorise { get; set; }
        public string LRRefTablekey { get; set; }
        public string PODRefTablekey { get; set; }
        public string CurrentBranch { get; set; }
        public string SendReceive { get; set; }
        public string FromBranch { get; set; }
        public string ToBranch { get; set; }
    }
}