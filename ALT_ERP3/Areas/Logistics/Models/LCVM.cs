using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class LCVM
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public DateTime DocDate { get; set; }
        public string LcDate { get; set; }

        public string Lcno { get; set; }
        public string Branch { get; set; }
        public string Time { get; set; }
        public string LcFromSource { get; set; }
        public string LcFromSource_Name { get; set; }
        public string LcTODest { get; set; }
        public string LcToDest_Name { get; set; }
        public bool DispachFM { get; set; }
        public string Remark { get; set; }
        public bool PeriodLock { get; set; }
        public bool LedgerThrough { get; set; }
        public bool LockAuthorise { get; set; }
        public bool DispatchLc { get; set; }
        
        public LCSetup LCSetup { get; set; }
        public List<StockDetails> Consignments { get; set; }
        public List<LcDetailsVM> lCDetails { get; set; }
        public List<GridOption> PrintGridList { get; set; }
        public string SearchLrnO { get; set; }

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
        public string LcGenerate { get; set; }

    }
    public class StockDetails
    {
        //public string lcno { get; set; }
        public string LrNo { get; set; }
        public string LrBookDate { get; set; }
        public string StockAvlIn { get; set; }
        public int AvlQty { get; set; }
        public int LoadQty { get; set; }
        public double LoadWeight { get; set; }
        public double ActWeight { get; set; }
        public double ChrWeight { get; set; }
        public string ChgType { get; set; }
        public string UnitCode { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Consigner { get; set; }
        public string Consignee { get; set; }
        public string LRType { get; set; }
        public string DeliveryOfLR { get; set; }
        public string Collection { get; set; }
        public string Mode { get; set; }
        public string StockTableky { get; set; }
        public string ConsignmentTableky { get; set; }
        public string Authenticate { get; set; }
    }
}