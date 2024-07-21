using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class PODReceivedVM
    {
        public bool GlobalSearch { get; set; }

        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public DateTime DocDate { get; set; }

        public bool PeriodLock { get; set; }
        public int PODNO { get; set; }
        public string SearchLrnoForPodReceived { get; set; }
        public string Lrno { get; set; }
        public string ConsignmentKey { get; set; }
        public string LRTime { get; set; }
        public string LRDate { get; set; }
        public string ConsignerName { get; set; }
        public string ConsigneeName { get; set; }
        public string FromName { get; set; }
        public string ToName { get; set; }
        public int LRQty { get; set; }
        public double LRWeight { get; set; }
        public string PODTime { get; set; }
        public string PODDate { get; set; }
        public string PODRemark { get; set; }
        public int DeliveryNo { get; set; }
        public string DeliveryTime { get; set; }
        public string DeliveryDate { get; set; }
        public int ShortQty { get; set; }
        public string VehicleNo { get; set; }
        public string PersonName { get; set; }
        public string MobileNO { get; set; }
        public string DeliveryGoodStatus { get; set; }
        public string DeliveryRemark { get; set; }

        public string Parentkey { get; set; }
        public string Task { get; set; }
        public bool LockAuthorise { get; set; }
        public bool BlockPOD { get; set; }
        public bool NODeliveryPOD { get; set; }
        
        public LCVM All { get; set; }
        public LCVM CurrentBranch { get; set; }

        public List<ListOfPod> PODPendingReceiveList { get; set; }
        public List<PODDelRetion> pODDelRetions { get; set; }

        public int BranchPODNO { get; set; }
        public string ComingFromBranchPOD { get; set; }
        public string ComingFromBranchPODN { get; set; }


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

    public class PODDelRetion
    {
        public bool BlockDelivery { get; set; }
        public string ParentKey { get; set; }
        public string StkBranch { get; set; }
        public string StkBranchN { get; set; }
        public string Type { get; set; }
        public int DelQty { get; set; }
        public int DelBalQty { get; set; }
        public double DelWeight { get; set; }
        public double DelBalWeight { get; set; }
        public string Authorise { get; set; }
    }

}