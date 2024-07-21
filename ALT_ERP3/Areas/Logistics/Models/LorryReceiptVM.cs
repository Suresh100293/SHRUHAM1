using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class LorryReceiptVM
    {
        public string LrNo { get; set; }
        public string DocDate { get; set; }
        public string LRtype { get; set; }
        public string LRtype_Name { get; set; }
        public string ServiceType { get; set; }
        public string ServiceType_Name { get; set; }
        public string RecCode { get; set; }
        public string RecCode_Name { get; set; }
        public string RecCodeAddr { get; set; }
        public string RecCodeInfo { get; set; }
        public string SendCode { get; set; }
        public string SendCode_Name { get; set; }
        public string SendCodeAddr { get; set; }
        public string SendCodeInfo { get; set; }
        public string BillParty { get; set; }
        public string BillParty_Name { get; set; }
        public string BillPartyAddr { get; set; }
        public string BillPartyInfo { get; set; }
        public string Source { get; set; }
        public string Source_Name { get; set; }
        public string Dest { get; set; }
        public string Dest_Name { get; set; }
        public string BillBran { get; set; }
        public string BillBran_Name { get; set; }
        public Collection Colln { get; set; }
        public string CollnArea { get; set; }
        public string CollnArea_Name { get; set; }
        public bool ColareaChk { get; set; }
        public string CollNAreaTxt { get; set; }
        public Delievery Del { get; set; }
        public string DelAt { get; set; }
        public string DelAt_Name { get; set; }
        public bool DeliveryChk { get; set; }
        public string DeliveryAtTxt { get; set; }
        public short PackQty { get; set; }
        public string UnitCode { get; set; }
        public string UnitCode_Name { get; set; }
        public double ActWt { get; set; }
        public double ChgWt { get; set; }
        public string ChgType { get; set; }
        public string ChgType_Name { get; set; }
        public string Descr { get; set; }
        public string Descr_Name { get; set; }
        public bool DescrChk { get; set; }
        public string DescrTxt { get; set; }
        public string PartyRef { get; set; }
        public string PartyInvoice { get; set; }
        public decimal DecVal { get; set; }
        public string GSTNO { get; set; }
        public string EwayBill { get; set; }
        public string VehicleNo { get; set; }
        public string Narr { get; set; }
        public string StartDate { get; set; }
        public string DocCurrentDate { get; set; }
        public string EndDate { get; set; }
        public string Val1 { get; set; }


        #region Calculation Part
        public string[] ChargeName { get; set; }
        public string[] ChargeValue { get; set; }
        public decimal Amt { get; set; }
        #endregion

        #region LrSetup
        public LRSetup LRSetup { get; set; }
        #endregion

        #region Draft Details
        //public List<DraftVM> DraftVM { get; set; }
        public bool Draft { get; set; }
        public string Draft_Name { get; set; }
        #endregion

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
    public enum Delievery
    {
        Godown, Door
    }

    public enum Collection
    {
        No, Yes
    }
}