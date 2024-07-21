using ALT_ERP3.Areas.Accounts.Models;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Vehicles.Models
{
    public class TyreStockTransferVM
    {


        #region Stock Details

        public string OldStkTyreSerialNo { get; set; }
        public string OldStkStatus { get; set; }
        public string OldStkAt { get; set; }
        public string OldStkTyrePlcaseNo { get; set; }
        public string OldStkTypeName { get; set; }
        public string OldStkTypeNameCode { get; set; }
        public string OldStkCost { get; set; }
        public string OldStkWarrantyKM { get; set; }
        public string OldStkDays { get; set; }
        public string OldStkInstallFor { get; set; }
        public string OldStkInstallDate { get; set; }
        public string OldStkInstallKM { get; set; }
        public string OldStkExpirtDate { get; set; }
        public string OldStkExpiryKM { get; set; }
        public string OldStkTablekey { get; set; }
        public string OldStkBranch { get; set; }

        #endregion

        #region Previous Vehicle Details
        public bool PreviousVehicleblockDocument { get; set; }
        public string PreviousVehicleStatusTransferToStock { get; set; }
        public string PreviousVehicleStatusStockAt { get; set; }
        public string PreviousVehicleStatusStockSerial { get; set; }
        public string PreviousVehicleStatusInstallDate { get; set; }
        public string PreviousVehicleStatusInstallKM { get; set; }
        public string PreviousVehicleStatusExpirtDate { get; set; }
        public string PreviousVehicleStatusExpiryKM { get; set; }
        public string PreviousVehicleStatusIsActive { get; set; }
        public string PreviousVehicleStatusTablekey { get; set; }
        public string PreviousVehicleStatusBranch { get; set; }
        #endregion


        #region New Stock Entity
        public bool NStockblockDocument { get; set; }

        public List<OtherTransactModel> TyreStockList { get; set; }
        public List<AddOns> AddOnList { get; set; }

        public string RelatedChoice { get; set; }
        public string FromType { get; set; }
        public string FromDate { get; set; }
        public string Type { get; set; }
        public string ExpDays { get; set; }
        public string KM { get; set; }
        public DateTime DocDateA { get; set; }

        #region Previous Vehicle Details
        public bool NPreviousblockDocument { get; set; }
        public string NPreviousTransferToStock { get; set; }
        public string NPreviousStockAt { get; set; }
        public string NPreviousStockSerial { get; set; }
        public string NPreviousInstallDate { get; set; }
        public string NPreviousInstallKM { get; set; }
        public string NPreviousExpirtDate { get; set; }
        public string NPreviousExpiryKM { get; set; }
        public string NPreviousIsActive { get; set; }
        public string NPreviousTablekey { get; set; }
        public string NPreviousBranch { get; set; }
        #endregion



        #endregion

        public string StockStatusList { get; set; }
        public string DoubleClickDocument { get; set; }
        public string DocDate { get; set; }
        public bool blockDocument { get; set; }

        public string TransferToStock { get; set; }
        public string VehicleList { get; set; }
        public string VehicleName { get; set; }
        public string VehicleCode { get; set; }

        public string InstallFor { get; set; }
        public string TyrePlaceNo { get; set; }
        public string TyreSerialNo { get; set; }
        public string InstallDate { get; set; }
        public string InstallKM { get; set; }


        public List<TyreStockSerial> PickupList { get; set; }
        public List<TyreStockTransferVM> HistoryDetails { get; set; }


        // iX9: used while passing parameters from MasterGrid to Option and Back 
        public string Document { get; set; }
        public string ViewDataId { get; set; }
        public string Header { get; set; }
        public string Mode { get; set; }

        public string MainType { get; set; }
        public string SubType { get; set; }
        public string Controller2 { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }

    }
}