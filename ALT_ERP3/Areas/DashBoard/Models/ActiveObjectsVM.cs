using ALT_ERP3.Models;
using System;
using System.Collections.Generic;

namespace ALT_ERP3
{
    public class ActiveObjectsVM
    {
        #region Outstanding Variables

        public decimal OSTotal { get; set; }
        public decimal OSUnAdj { get; set; }
        public decimal OS30 { get; set; }
        public decimal OS60 { get; set; }
        public decimal OS90 { get; set; }
        public decimal OS120 { get; set; }
        public decimal OS150 { get; set; }
        public decimal OS180 { get; set; }
        public decimal OS180M { get; set; }

        public decimal PayOSTotal { get; set; }
        public decimal PayOSUnAdj { get; set; }
        public decimal PayOS30 { get; set; }
        public decimal PayOS60 { get; set; }
        public decimal PayOS90 { get; set; }
        public decimal PayOS120 { get; set; }
        public decimal PayOS150 { get; set; }
        public decimal PayOS180 { get; set; }
        public decimal PayOS180M { get; set; }
        #endregion

        #region Unbill Consignment Variables

        public int UnBillTotalConsignment { get; set; }
        public decimal UnBillTotalAmtConsignment { get; set; }

        public int UnBillLr30 { get; set; }
        public int UnBillLr60 { get; set; }
        public int UnBillLr90 { get; set; }
        public int UnBillLr120 { get; set; }
        public int UnBillLr120M { get; set; }
        public decimal UnBillLrAmt30 { get; set; }
        public decimal UnBillLrAmt60 { get; set; }
        public decimal UnBillLrAmt90 { get; set; }
        public decimal UnBillLrAmt120 { get; set; }
        public decimal UnBillLrAmt120M { get; set; }

        #endregion

        #region Consignment Stock Variables

        public int StockTotalConsignment { get; set; }
        public int StockTotalQtyConsignment { get; set; }
        public decimal StockTotalWeightConsignment { get; set; }
        public decimal StockTotalValueConsignment { get; set; }

        public int StockLr30 { get; set; }
        public int StockLr60 { get; set; }
        public int StockLr90 { get; set; }
        public int StockLr120 { get; set; }
        public int StockLr120M { get; set; }
        public int StockLrQty30 { get; set; }
        public int StockLrQty60 { get; set; }
        public int StockLrQty90 { get; set; }
        public int StockLrQty120 { get; set; }
        public int StockLrQty120M { get; set; }
        public decimal StockLrWT30 { get; set; }
        public decimal StockLrWT60 { get; set; }
        public decimal StockLrWT90 { get; set; }
        public decimal StockLrWT120 { get; set; }
        public decimal StockLrWT120M { get; set; }
        public decimal StockLrDeclar30 { get; set; }
        public decimal StockLrDeclar60 { get; set; }
        public decimal StockLrDeclar90 { get; set; }
        public decimal StockLrDeclar120 { get; set; }
        public decimal StockLrDeclar120M { get; set; }
        #endregion

        #region Consignment TRN Stock Variables

        public int StockTotalConsignmentTRN { get; set; }
        public int StockTotalQtyConsignmentTRN { get; set; }
        public decimal StockTotalWeightConsignmentTRN { get; set; }
        public decimal StockTotalValueConsignmentTRN { get; set; }

        public int StockLr30TRN { get; set; }
        public int StockLr60TRN { get; set; }
        public int StockLr90TRN { get; set; }
        public int StockLr120TRN { get; set; }
        public int StockLr120MTRN { get; set; }
        public int StockLrQty30TRN { get; set; }
        public int StockLrQty60TRN { get; set; }
        public int StockLrQty90TRN { get; set; }
        public int StockLrQty120TRN { get; set; }
        public int StockLrQty120MTRN { get; set; }
        public decimal StockLrWT30TRN { get; set; }
        public decimal StockLrWT60TRN { get; set; }
        public decimal StockLrWT90TRN { get; set; }
        public decimal StockLrWT120TRN { get; set; }
        public decimal StockLrWT120MTRN { get; set; }
        public decimal StockLrDeclar30TRN { get; set; }
        public decimal StockLrDeclar60TRN { get; set; }
        public decimal StockLrDeclar90TRN { get; set; }
        public decimal StockLrDeclar120TRN { get; set; }
        public decimal StockLrDeclar120MTRN { get; set; }
        #endregion

        #region Booking Variables

        public decimal BKTotal { get; set; }
        public decimal BKApr { get; set; }
        public decimal BKMay { get; set; }
        public decimal BKJun { get; set; }
        public decimal BKJuy { get; set; }
        public decimal BKAug { get; set; }
        public decimal BKSept { get; set; }
        public decimal BKOct { get; set; }
        public decimal BKNov { get; set; }
        public decimal BKDec { get; set; }
        public decimal BKJan { get; set; }
        public decimal BKFeb { get; set; }
        public decimal BKMar { get; set; }

        #endregion

        #region TOP Customers

        public List<string> TopCustomerName { get; set; }
        public List<decimal> TopCustomerAmt { get; set; }

        #endregion

        #region TOP Group-Customers

        public List<string> TopGroupCustomerName { get; set; }
        public List<decimal> TopGroupCustomerAmt { get; set; }

        #endregion

        #region TOP Vendors

        public List<string> TopVendorsName { get; set; }
        public List<decimal> TopVendorsAmt { get; set; }

        #endregion

        #region TOP Expenses

        public List<string> TopExpenseName { get; set; }
        public List<decimal> TopExpenseAmt { get; set; }

        #endregion

        #region Vehicle Status
        public int VCountVehicle { get; set; }
        public int VActiveVehicle { get; set; }
        public int VMaintain { get; set; }
        public int VNoDriver { get; set; }
        public int VReady { get; set; }
        public int VTransit { get; set; }
        public int VAccident { get; set; }
        public int VSale { get; set; }

        #endregion

        #region Driver Status
        public int ONVehicle { get; set; }
        public int NOVehicle { get; set; }
        public int Bhatta { get; set; }
        public int DriverCount { get; set; }
        //public List<string> DriverStatusName { get; set; }
        //public List<int> DriverStatusAvg { get; set; }
        public int ActiverDrivers { get; set; }

        #endregion

        #region Vehicle Tracking

        public List<VehicleTrackinModel> VehicleTrackList { get; set; }
        public List<VehicleTrackinModel> BranchTrackList { get; set; }
        public bool BranchReq { get; set; }
        public string SearchVehicle { get; set; }
        public string CountVehicle { get; set; }
        public string CountBranch { get; set; }
        #endregion


        #region DriverTripBalance
        public string DriverBalTotal { get; set; }
        public List<string> DriverName { get; set; }
        public List<string> DriverLastTripDate { get; set; }
        public List<string> DriverBal { get; set; }

        #endregion

        #region VehicleTripBalance

        public List<string> VehicleName { get; set; }
        public List<string> VehicleLastTripDate { get; set; }
        public List<string> VehicleKM { get; set; }

        #endregion

        #region VehicleExpDue

        public List<string> VehicleExpName { get; set; }
        public List<string> VehicleExpName0 { get; set; }
        public List<string> VehicleExpName5 { get; set; }
        public List<string> VehicleExpName15 { get; set; }
        public List<string> VehicleExpName30 { get; set; }

        #endregion

        #region EwayBillDetails

        public List<string> EWBActive { get; set; }
        public List<string> EWBActiveToday { get; set; }
        public List<string> EWBActiveTomorrow { get; set; }
        public List<string> EWBActiveExpired { get; set; }

        #endregion


        public bool DefaultMap { get; set; }

        public string Branch { get; set; }
        public string User { get; set; }
        public string ID { get; set; }

        public decimal? EnqAmt { get; set; }
        public int? EnqCount { get; set; }
        public decimal? QtnAmt { get; set; }
        public int? QtnCount { get; set; }
        public decimal? OrdAmt { get; set; }
        public int? OrdCount { get; set; }
        public decimal? InvAmt { get; set; }
        public int? InvCount { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string SizeType { get; set; }
        public string ObjectType { get; internal set; }
        public bool DashboardActive { get; set; }
        public string ReportCode { get; set; }
        public List<ActiveObjectsVM> codes { get; set; }
    }
    public class VehicleTrackinModel
    {
        public string title { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public string description { get; set; }

    }
}