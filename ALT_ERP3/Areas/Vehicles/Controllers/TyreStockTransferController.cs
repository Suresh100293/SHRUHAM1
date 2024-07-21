using ALT_ERP3.Areas.Vehicles.Models;
using ALT_ERP3.Controllers;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ALT_ERP3.Areas.Vehicles.Controllers
{
    public class TyreStockTransferController : BaseController
    {
        // GET: Vehicles/TyreStockTransfer
        private static string mauthorise = "A00";
        private static string mdocument = "";
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

        #region Functions
        public ActionResult PopulateStock()
        {
            List<SelectListItem> items = new List<SelectListItem>();


            items.Add(new SelectListItem
            {
                Text = "All",
                Value = "All"
            });
            items.Add(new SelectListItem
            {
                Text = "Vehicle",
                Value = "Vehicle"
            });
            items.Add(new SelectListItem
            {
                Text = "Remould",
                Value = "Remould"
            });
            items.Add(new SelectListItem
            {
                Text = "Scrap",
                Value = "Scrap"
            });
            items.Add(new SelectListItem
            {
                Text = "Sale",
                Value = "Sale"
            });
            items.Add(new SelectListItem
            {
                Text = "OutOfStock",
                Value = "OutOfStock"
            });


            return Json(items, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PopulateUpdateStock()
        {
            List<SelectListItem> items = new List<SelectListItem>();


            items.Add(new SelectListItem
            {
                Text = "Vehicle",
                Value = "Vehicle"
            });
            items.Add(new SelectListItem
            {
                Text = "Remould",
                Value = "Remould"
            });
            items.Add(new SelectListItem
            {
                Text = "Scrap",
                Value = "Scrap"
            });
            items.Add(new SelectListItem
            {
                Text = "Sale",
                Value = "Sale"
            });
            items.Add(new SelectListItem
            {
                Text = "OutOfStock",
                Value = "OutOfStock"
            });
            items.Add(new SelectListItem
            {
                Text = "Stock",
                Value = "Stock"
            });
            return Json(items, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PopulateUpdateOldVehicleStock()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            items.Add(new SelectListItem
            {
                Text = "Remould",
                Value = "Remould"
            });
            items.Add(new SelectListItem
            {
                Text = "Scrap",
                Value = "Scrap"
            });
            items.Add(new SelectListItem
            {
                Text = "Sale",
                Value = "Sale"
            });
            items.Add(new SelectListItem
            {
                Text = "OutOfStock",
                Value = "OutOfStock"
            });
            items.Add(new SelectListItem
            {
                Text = "Stock",
                Value = "Stock"
            });
            return Json(items, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PopulateVehicle()
        {

            List<SelectListItem> GSt = new List<SelectListItem>();
            //var mVehicles = ctxTFAT.Master.Where(x => x.RelatedTo == "1").Select(m => new
            //{
            //    Value = m.Code,
            //    Text = m.Name
            //}).OrderBy(n => n.Text).ToList();

            var mVehicles = ctxTFAT.VehicleMaster.Where(x => x.Acitve == true).Select(m => new
            {
                Value = m.Code,
                Text = m.TruckNo
            }).OrderBy(n => n.Text).ToList();


            foreach (var a in mVehicles)
            {
                GSt.Add(new SelectListItem { Value = a.Value, Text = a.Text });
            }
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PopulateVehicleNew()
        {

            List<SelectListItem> GSt = new List<SelectListItem>();
            //var mVehicles = ctxTFAT.Master.Where(x => x.RelatedTo == "1").Select(m => new
            //{
            //    Value = m.Code,
            //    Text = m.Name
            //}).OrderBy(n => n.Text).ToList();

            var mVehicles = ctxTFAT.VehicleMaster.Where(x => x.Acitve == true).Select(m => new
            {
                Value = m.Code,
                Text = m.TruckNo
            }).OrderBy(n => n.Text).ToList();


            foreach (var a in mVehicles)
            {
                GSt.Add(new SelectListItem { Value = a.Value, Text = a.Text });
            }
            GSt.Add(new SelectListItem { Value = "Stock", Text = "Stock" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PopulateInstallFor()
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem
            {
                Text = "Tyre",
                Value = "Tyre"
            });
            GSt.Add(new SelectListItem
            {
                Text = "Stepnee",
                Value = "Stepnee"
            });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }
        public string GenerateCode()
        {
            var LastCode = ctxTFAT.TyreStockTransfer.OrderByDescending(x => x.RECORDKEY).Select(x => x.DocNo).FirstOrDefault();
            if (String.IsNullOrEmpty(LastCode))
            {
                LastCode = "100000";
            }
            else
            {
                int NewCode = Convert.ToInt32(LastCode) + 1;
                LastCode = NewCode.ToString("D6");
            }
            return LastCode;
        }

        public DateTime ConvertYYMMDDTODDMMYY(string da)
        {
            string abc = da.Substring(8, 2) + "/" + da.Substring(5, 2) + "/" + da.Substring(0, 4);
            return Convert.ToDateTime(abc);
        }

        #endregion


        // GET: Accounts/TyreStockTransfer
        public ActionResult Index(TyreStockTransferVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "","NA");
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                #region Default

                #endregion

                var mList = ctxTFAT.TyreStockTransfer.Where(x => (x.DocNo == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    if (mList.TransferToStock == "New")
                    {
                        mModel.AddOnList = GetTruckWiseData(mList.Tablekey, mList.Branch);
                        var CurrentStock = ctxTFAT.TyreStockSerial.Where(x => x.Branch == mList.Branch && x.TableKey == mList.Tablekey).FirstOrDefault();
                        var FirstStockHistory = ctxTFAT.TyreStockSerial.Where(x => x.TableKey.ToString() == CurrentStock.ParentKey.Trim()).OrderBy(x => x.RECORDKEY).FirstOrDefault();

                        if (FirstStockHistory != null)
                        {
                            var ActiveStock = ctxTFAT.TyreStockSerial.Where(x => x.TableKey.ToString() == CurrentStock.ParentKey.Trim()).OrderByDescending(x => x.RECORDKEY).FirstOrDefault();

                            mModel.NPreviousTransferToStock = ActiveStock.Status;
                            mModel.NPreviousStockSerial = FirstStockHistory.SerialNo;
                            mModel.NPreviousStockAt = ActiveStock.StockAt;
                            mModel.NPreviousInstallDate = FirstStockHistory.Value1;
                            mModel.NPreviousInstallKM = FirstStockHistory.Value2.Value.ToString();
                            mModel.NPreviousExpirtDate = FirstStockHistory.Value3;
                            mModel.NPreviousExpiryKM = FirstStockHistory.Value4.Value.ToString();
                            mModel.NPreviousIsActive = FirstStockHistory.IsActive == true ? "Active" : "Not Active";
                            mModel.NPreviousTablekey = FirstStockHistory.TableKey;
                            mModel.NPreviousBranch = FirstStockHistory.Branch;
                            mModel.NPreviousblockDocument = ActiveStock.IsActive == true ? false : true;
                        }

                        mModel.Document = mList.DocNo;

                        mModel.TransferToStock = mList.TransferToStock;
                        mModel.VehicleCode = mList.VehicleCode;
                        mModel.InstallFor = mList.InstallFor == "T" ? "Tyre" : "Stepnee";
                        mModel.TyrePlaceNo = mList.TyrePlaceNo;
                        mModel.TyreSerialNo = mList.TyreSerialNo;
                        mModel.InstallDate = mList.InstallDate.ToShortDateString();
                        mModel.InstallKM = mList.InstallKM;
                        mModel.VehicleList = mList.VehicleCode;
                        mModel.RelatedChoice = mList.VehicleCode;
                        var Status = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == mList.Tablekey && x.Branch == mList.Branch).Select(x => x.IsActive).FirstOrDefault();
                        mModel.NStockblockDocument = Status == true ? false : true;

                    }
                    else
                    {
                        //Stock Details
                        var CurrentStock = ctxTFAT.TyreStockSerial.Where(x => x.Branch == mList.Branch && x.TableKey == mList.Tablekey).FirstOrDefault();
                        var FirstStockHistory = ctxTFAT.TyreStockSerial.Where(x => x.TableKey.ToString() == CurrentStock.ParentKey.Trim()).OrderBy(x => x.RECORDKEY).FirstOrDefault();
                        var FirstStockHistoryRelateData = ctxTFAT.RelateData.Where(x => x.TableKey == FirstStockHistory.TableKey && x.Branch == FirstStockHistory.Branch).Select(x => x).FirstOrDefault();
                        var ActiveStock = ctxTFAT.TyreStockSerial.Where(x => x.TableKey.ToString() == CurrentStock.ParentKey.Trim() && x.ParentKey.ToString() != CurrentStock.TableKey.Trim()).OrderByDescending(x => x.RECORDKEY).FirstOrDefault();
                        if (FirstStockHistoryRelateData != null)
                        {

                            mModel.OldStkTyreSerialNo = FirstStockHistory.SerialNo;
                            mModel.OldStkStatus = FirstStockHistory.Status;
                            mModel.OldStkAt = FirstStockHistory.StockAt == "Vehicle" ? ctxTFAT.Master.Where(x => x.Code == FirstStockHistory.Vehicle).Select(x => x.Name).FirstOrDefault() : "Godown";
                            mModel.OldStkTyrePlcaseNo = FirstStockHistory.StockAt == "Vehicle" ? String.IsNullOrEmpty(FirstStockHistory.StepneeNo) == true ? FirstStockHistory.TyreNo : FirstStockHistory.StepneeNo : "";
                            mModel.OldStkTypeName = ctxTFAT.TyreMaster.Where(x => x.TyreType == FirstStockHistoryRelateData.Value2).Select(x => x.Name).FirstOrDefault();
                            mModel.OldStkTypeNameCode = FirstStockHistoryRelateData.Value2;
                            mModel.OldStkCost = FirstStockHistoryRelateData.Num1.Value.ToString();
                            mModel.OldStkWarrantyKM = FirstStockHistoryRelateData.Num4.Value.ToString();
                            mModel.OldStkDays = FirstStockHistoryRelateData.Value3.ToString();
                            mModel.OldStkInstallFor = FirstStockHistory.StockAt == "Vehicle" ? FirstStockHistoryRelateData.Value4 : "";
                            mModel.OldStkInstallDate = FirstStockHistoryRelateData.Date2.Value.ToShortDateString();
                            mModel.OldStkInstallKM = FirstStockHistory.StockAt == "Vehicle" ? FirstStockHistoryRelateData.Num2.ToString() : "";
                            mModel.OldStkExpirtDate = FirstStockHistoryRelateData.Date3 == null ? null : FirstStockHistoryRelateData.Date3.Value.ToShortDateString();
                            mModel.OldStkExpiryKM = FirstStockHistory.StockAt == "Vehicle" ? FirstStockHistoryRelateData.Num3.Value.ToString() : "";
                            mModel.OldStkTablekey = ActiveStock.TableKey;
                            mModel.OldStkBranch = ActiveStock.Branch;
                            //Model.OldStkParentkey = TyreStockSerial.ParentKey;
                        }

                        //Previous Stock Details
                        if (mList.TransferToStock == "Vehicle")
                        {
                            var CurrVehicleStock = ctxTFAT.TyreStockSerial.Where(x => x.Branch == mList.Branch && x.ParentKey == mList.Tablekey).OrderByDescending(x => x.RECORDKEY).FirstOrDefault();
                            var StockDetails = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == CurrVehicleStock.TableKey && x.ParentKey != mList.Tablekey).OrderByDescending(x => x.RECORDKEY).FirstOrDefault();

                            if (StockDetails != null)
                            {
                                mModel.PreviousVehicleblockDocument = CurrVehicleStock.IsActive == true ? false : true;
                                mModel.PreviousVehicleStatusTransferToStock = CurrVehicleStock.Status;
                                mModel.PreviousVehicleStatusStockSerial = StockDetails.SerialNo;
                                mModel.PreviousVehicleStatusStockAt = StockDetails.StockAt;
                                mModel.PreviousVehicleStatusInstallDate = StockDetails.Value1;
                                mModel.PreviousVehicleStatusInstallKM = StockDetails.Value2.Value.ToString();
                                mModel.PreviousVehicleStatusExpirtDate = StockDetails.Value3;
                                mModel.PreviousVehicleStatusExpiryKM = StockDetails.Value4.Value.ToString();
                                mModel.PreviousVehicleStatusIsActive = "Active";
                                mModel.PreviousVehicleStatusTablekey = StockDetails.TableKey;
                                mModel.PreviousVehicleStatusBranch = StockDetails.Branch;
                                //Model.PreviousVehicleStatusParentkey = StockDetails.ParentKey;
                            }
                        }


                        if (ctxTFAT.TyreStockSerial.Where(x => x.TableKey == mList.Tablekey && x.Branch == mList.Branch).Select(x => x.IsActive).FirstOrDefault())
                        {
                            mModel.blockDocument = false;
                        }
                        else
                        {
                            mModel.blockDocument = true;
                        }

                        mModel.Document = mList.DocNo;

                        mModel.TransferToStock = mList.TransferToStock;
                        mModel.VehicleCode = mList.VehicleCode;
                        mModel.InstallFor = mList.InstallFor == "T" ? "Tyre" : "Stepnee";
                        mModel.TyrePlaceNo = mList.TyrePlaceNo;
                        mModel.TyreSerialNo = mList.TyreSerialNo;
                        mModel.InstallDate = mList.InstallDate.ToShortDateString();
                        mModel.InstallKM = mList.InstallKM;
                        mModel.VehicleList = mList.VehicleCode;
                    }


                }
            }
            else
            {
                mModel.TransferToStock = "0";
                mModel.VehicleList = "0";
                mModel.StockStatusList = "0";
                mModel.InstallFor = "0";
                mModel.InstallDate = DateTime.Now.ToShortDateString();
                mModel.AddOnList = AddNewStock(mModel);
            }


            return View(mModel);
        }

        public ActionResult GetPickUp(TyreStockTransferVM Model)
        {
            string mstr = "";
            string abc = "";

            string Status = "";
            if (Model.StockStatusList == "All")
            {
                Status = "  Status <> 'OutOfStock' and StockAt  <> 'OutOfStock'";
            }
            else
            {
                Status = "  Status = '" + Model.StockStatusList + "'  or  StockAt = '" + Model.StockStatusList + "'              ";
            }


            mstr = "Select RECORDKEY,SerialNo,Status,case when StockAt='Vehicle' then TyreNo else '' end as VehicleTyreNo,case when StockAt='Vehicle' then StepneeNo else '' end as VehicleStepneeNo, ( case when StockAt='Vehicle' then (select m.Name from Master m where m.Code=Vehicle) else (case when  StockAt='Stock' then 'Godown' else StockAt end ) end) as StockAt,ParentKey,TableKey from TyreStockSerial WHERE IsActive='true' and  " + Status + " Order by RecordKey";


            List<DataRow> ordersstk = GetDataTable(mstr).AsEnumerable().ToList();
            Model.PickupList = Add2ItemForPickup(ordersstk, abc);

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            var html = ViewHelper.RenderPartialView(this, "PickUp", new TyreStockTransferVM() { PickupList = Model.PickupList });
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public List<TyreStockSerial> Add2ItemForPickup(List<DataRow> ordersstk, string query)
        {
            List<TyreStockSerial> objitemlist = new List<TyreStockSerial>();
            int i = 1;
            foreach (var item in ordersstk)
            {

                objitemlist.Add(new TyreStockSerial()
                {
                    RECORDKEY = Convert.ToInt32(item["RECORDKEY"].ToString()),
                    SerialNo = item["SerialNo"].ToString(),
                    Status = item["Status"].ToString(),
                    Vehicle = i.ToString(),
                    TyreNo = (item["VehicleTyreNo"].ToString()),
                    StepneeNo = item["VehicleStepneeNo"].ToString(),
                    StockAt = item["StockAt"].ToString(),
                    ParentKey = item["ParentKey"].ToString(),
                    TableKey = item["TableKey"].ToString(),
                });
                ++i;

            }
            return objitemlist;
        }

        public ActionResult OldStockDetails(TyreStockTransferVM Model)
        {
            var ActiveStock = ctxTFAT.TyreStockSerial.Where(x => x.RECORDKEY.ToString() == Model.DoubleClickDocument.Trim()).FirstOrDefault();

            var FirstStockHistory = ctxTFAT.TyreStockSerial.Where(x => x.TableKey.ToString() == ActiveStock.TableKey.Trim()).OrderBy(x => x.RECORDKEY).FirstOrDefault();
            var FirstStockHistoryRelateData = ctxTFAT.RelateData.Where(x => x.TableKey == FirstStockHistory.TableKey && x.Branch == FirstStockHistory.Branch).Select(x => x).FirstOrDefault();

            if (FirstStockHistoryRelateData != null)
            {

                Model.OldStkTyreSerialNo = FirstStockHistory.SerialNo;
                Model.OldStkStatus = FirstStockHistory.Status;
                Model.OldStkAt = FirstStockHistory.StockAt == "Vehicle" ? ctxTFAT.Master.Where(x => x.Code == FirstStockHistory.Vehicle).Select(x => x.Name).FirstOrDefault() : "Godown";
                Model.OldStkTyrePlcaseNo = FirstStockHistory.StockAt == "Vehicle" ? String.IsNullOrEmpty(FirstStockHistory.StepneeNo) == true ? FirstStockHistory.TyreNo : FirstStockHistory.StepneeNo : "";
                Model.OldStkTypeName = ctxTFAT.TyreMaster.Where(x => x.TyreType == FirstStockHistoryRelateData.Value2).Select(x => x.Name).FirstOrDefault();
                Model.OldStkTypeNameCode = FirstStockHistoryRelateData.Value2;
                Model.OldStkCost = FirstStockHistoryRelateData.Num1.Value.ToString();
                Model.OldStkWarrantyKM = FirstStockHistoryRelateData.Num4.Value.ToString();
                Model.OldStkDays = FirstStockHistoryRelateData.Value3.ToString();
                Model.OldStkInstallFor = FirstStockHistory.StockAt == "Vehicle" ? FirstStockHistoryRelateData.Value4 : "";
                Model.OldStkInstallDate = FirstStockHistoryRelateData.Date2.Value.ToShortDateString();
                Model.OldStkInstallKM = FirstStockHistory.StockAt == "Vehicle" ? FirstStockHistoryRelateData.Num2.ToString() : "";
                Model.OldStkExpirtDate = FirstStockHistoryRelateData.Date3 == null ? null : FirstStockHistoryRelateData.Date3.Value.ToShortDateString();
                Model.OldStkExpiryKM = FirstStockHistory.StockAt == "Vehicle" ? FirstStockHistoryRelateData.Num3.Value.ToString() : "";
                Model.OldStkTablekey = ActiveStock.TableKey;
                Model.OldStkBranch = ActiveStock.Branch;
                //Model.OldStkParentkey = TyreStockSerial.ParentKey;
            }
            //else
            //{
            //    var TyreStockTransfer = ctxTFAT.TyreStockTransfer.Where(x => x.Tablekey == TyreStockSerial.TableKey).Select(x => x).FirstOrDefault();
            //    Model.OldStkTypeName = ctxTFAT.TyreMaster.Where(x => x.TyreType == TyreStockTransfer.OldStkTypeNameCode).Select(x => x.Name).FirstOrDefault();
            //    Model.OldStkTypeNameCode = TyreStockTransfer.OldStkTypeNameCode;
            //    Model.OldStkCost = TyreStockTransfer.OldStkCost;
            //    Model.OldStkWarrantyKM = TyreStockTransfer.OldStkWarrantyKM;
            //    Model.OldStkDays = TyreStockTransfer.OldStkDays;
            //    Model.OldStkInstallFor = TyreStockTransfer.OldStkInstallFor;
            //    Model.OldStkTyrePlcaseNo = TyreStockTransfer.OldStkTyrePlcaseNo;
            //    Model.OldStkTyreSerialNo = TyreStockTransfer.OldStkTyreSerialNo;
            //    Model.OldStkInstallDate = TyreStockTransfer.OldStkInstallDate.ToShortDateString();
            //    Model.OldStkInstallKM = TyreStockTransfer.OldStkInstallKM;
            //    Model.OldStkExpirtDate = TyreStockTransfer.OldStkExpirtDate == null ? "" : TyreStockTransfer.OldStkExpirtDate.Value.ToShortDateString();
            //    Model.OldStkExpiryKM = TyreStockTransfer.OldStkExpiryKM;
            //    Model.OldStkAt = TyreStockTransfer.OldStkAt;
            //    Model.OldStkStatus = TyreStockTransfer.OldStkStatus;
            //    Model.OldStkTablekey = TyreStockTransfer.OldStkTablekey;
            //    Model.OldStkParentkey = TyreStockTransfer.OldStkParentkey;
            //}

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            var html = ViewHelper.RenderPartialView(this, "_StockDetailsPartialView", Model);
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public ActionResult NewStockDetails(TyreStockTransferVM Model)
        {
            string Status = "Success";
            string Message = "";

            var Vehiclemaster = ctxTFAT.VehicleMaster.Where(x => x.Code == Model.VehicleList).FirstOrDefault();
            if (Vehiclemaster != null)
            {
                if (Model.InstallFor == "Tyre")
                {
                    if (!(Convert.ToInt32(Model.TyrePlaceNo) <= Vehiclemaster.NoOfTyres))
                    {
                        Status = "Error";
                        Message = "Vehicle Have Only " + Vehiclemaster.NoOfTyres + " Tyres....";
                    }
                }
                else
                {
                    if (!(Convert.ToInt32(Model.TyrePlaceNo) <= Vehiclemaster.Stepney))
                    {
                        Status = "Error";
                        Message = "Vehicle Have Only " + Vehiclemaster.Stepney + " Stepnees....";
                    }
                }
            }

            if (Status != "Error")
            {
                TyreStockSerial StockDetails = new TyreStockSerial();
                if (Model.InstallFor == "Tyre" && String.IsNullOrEmpty(Model.TyrePlaceNo) == false && Vehiclemaster != null)
                {
                    StockDetails = ctxTFAT.TyreStockSerial.Where(x => x.TyreNo == Model.TyrePlaceNo && x.Vehicle == Vehiclemaster.PostAc && x.StockAt == "Vehicle" && x.IsActive == true).OrderByDescending(x => x.RECORDKEY).FirstOrDefault();
                }
                else if (Model.InstallFor == "Stepnee" && String.IsNullOrEmpty(Model.TyrePlaceNo) == false && Vehiclemaster != null)
                {
                    StockDetails = ctxTFAT.TyreStockSerial.Where(x => x.StepneeNo == Model.TyrePlaceNo && x.Vehicle == Vehiclemaster.PostAc && x.StockAt == "Vehicle" && x.IsActive == true).OrderByDescending(x => x.RECORDKEY).FirstOrDefault();
                }
                else
                {
                    StockDetails = null;
                }

                if (StockDetails != null)
                {
                    Model.PreviousVehicleStatusTransferToStock = StockDetails.Status;
                    Model.PreviousVehicleStatusStockSerial = StockDetails.SerialNo;
                    Model.PreviousVehicleStatusStockAt = StockDetails.StockAt;
                    Model.PreviousVehicleStatusInstallDate = StockDetails.Value1;
                    Model.PreviousVehicleStatusInstallKM = StockDetails.Value2.Value.ToString();
                    Model.PreviousVehicleStatusExpirtDate = StockDetails.Value3;
                    Model.PreviousVehicleStatusExpiryKM = StockDetails.Value4.Value.ToString();
                    Model.PreviousVehicleStatusIsActive = StockDetails.IsActive == true ? "Active" : "Not Active";
                    Model.PreviousVehicleStatusTablekey = StockDetails.TableKey;
                    Model.PreviousVehicleStatusBranch = StockDetails.Branch;
                    //Model.PreviousVehicleStatusParentkey = StockDetails.ParentKey;
                }
                else
                {
                    Model.PreviousVehicleStatusStockAt = null;
                }
            }
            else
            {
                Model.PreviousVehicleStatusStockAt = null;
            }

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            var html = ViewHelper.RenderPartialView(this, "_TransferPartialView", Model);
            var jsonResult = Json(new { status = Status, HTML = html, Message = Message }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public ActionResult HistoryDetails(TyreStockTransferVM Model)
        {
            var tyrestock = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == Model.OldStkTablekey).ToList();
            List<TyreStockTransferVM> list = new List<TyreStockTransferVM>();
            int i = 1;
            foreach (var item in tyrestock)
            {
                TyreStockTransferVM tyreStock = new TyreStockTransferVM();
                RelateData mRelateData = new RelateData();
                if (i == 1)
                {
                    mRelateData = ctxTFAT.RelateData.Where(x => x.TableKey == item.TableKey && x.Branch == item.Branch).Select(x => x).FirstOrDefault();
                }
                if (mRelateData != null)
                {
                    tyreStock.OldStkTypeName = ctxTFAT.TyreMaster.Where(x => x.TyreType == mRelateData.Value2).Select(x => x.Name).FirstOrDefault();
                    tyreStock.OldStkTypeNameCode = mRelateData.Value2 == null ? null : mRelateData.Value2;
                    tyreStock.OldStkCost = mRelateData.Num1 == null ? null : mRelateData.Num1.Value.ToString();
                    tyreStock.OldStkWarrantyKM = mRelateData.Num4 == null ? null : mRelateData.Num4.Value.ToString();
                    tyreStock.OldStkDays = mRelateData.Value3 == null ? null : mRelateData.Value3.ToString();
                    tyreStock.OldStkInstallFor = String.IsNullOrEmpty(item.StepneeNo) == false ? "Stepnee" : String.IsNullOrEmpty(item.TyreNo) == false ? "Tyre" : "";
                    tyreStock.OldStkTyrePlcaseNo = String.IsNullOrEmpty(item.StepneeNo) == false ? item.StepneeNo : String.IsNullOrEmpty(item.TyreNo) == false ? item.TyreNo : "";
                    tyreStock.OldStkTyreSerialNo = mRelateData.Value6 == null ? null : mRelateData.Value6;
                    tyreStock.OldStkInstallDate = item.Value1;
                    tyreStock.OldStkInstallKM = item.Value2.ToString();
                    tyreStock.OldStkExpirtDate = mRelateData.Date3 == null ? null : mRelateData.Date3.Value.ToShortDateString();
                    tyreStock.OldStkExpiryKM = mRelateData.Num3 == null ? null : mRelateData.Num3.Value.ToString();
                    tyreStock.OldStkAt = item.StockAt == "Vehicle" ? ctxTFAT.Master.Where(x => x.Code == item.Vehicle).Select(x => x.Name).FirstOrDefault() : "Godown";
                    tyreStock.OldStkStatus = item.Status;
                    tyreStock.DocDate = item.DocDate.ToShortDateString();

                }
                list.Add(tyreStock);
                ++i;
            }
            Model.HistoryDetails = list;
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            var html = ViewHelper.RenderPartialView(this, "_HistoryPartialView", Model);
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public void DeUpdate(TyreStockTransferVM Model)
        {
            TyreStockTransfer tyreStockTransfer = ctxTFAT.TyreStockTransfer.Where(x => x.DocNo == Model.Document).FirstOrDefault();

            RelateData relateData = ctxTFAT.RelateData.Where(x => x.TableKey == tyreStockTransfer.Tablekey && x.Branch == tyreStockTransfer.Branch).FirstOrDefault();
            ctxTFAT.RelateData.Remove(relateData);

            TyreStockSerial NewStock = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == tyreStockTransfer.Tablekey && x.Branch == tyreStockTransfer.Branch).FirstOrDefault();
            ctxTFAT.TyreStockSerial.Remove(NewStock);

            //OldStock And VihiclePreviousStock
            var List = ctxTFAT.TyreStockSerial.Where(x => x.ParentKey == tyreStockTransfer.Tablekey && x.Branch == tyreStockTransfer.Branch).ToList();
            ctxTFAT.TyreStockSerial.RemoveRange(List);

            if (Model.Mode == "Delete")
            {
                ctxTFAT.TyreStockTransfer.Remove(tyreStockTransfer);

                //update Stock
                foreach (var item in List)
                {
                    TyreStockSerial TSS = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == item.TableKey && x.ParentKey != tyreStockTransfer.Tablekey).OrderByDescending(x => x.RECORDKEY).Select(x => x).FirstOrDefault();
                    TSS.IsActive = true;
                    ctxTFAT.Entry(TSS).State = EntityState.Modified;
                }
            }

            ctxTFAT.SaveChanges();

        }

        public ActionResult SaveData(TyreStockTransferVM Model)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    bool mAdd = true;
                    if (Model.Mode == "Delete")
                    {
                        DeleteData(Model);
                        transaction.Commit();
                        transaction.Dispose();
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    TyreStockTransfer tyreStock = new TyreStockTransfer();
                    if (ctxTFAT.TyreStockTransfer.Where(x => x.DocNo == Model.Document).FirstOrDefault() != null)
                    {
                        tyreStock = ctxTFAT.TyreStockTransfer.Where(x => x.DocNo == Model.Document).FirstOrDefault();
                        mAdd = false;
                        DeUpdate(Model);
                    }

                    if (String.IsNullOrEmpty(Model.RelatedChoice))
                    {
                        #region tyreStock Transfer

                        if (mAdd)
                        {
                            tyreStock.DocNo = GenerateCode();
                            tyreStock.Tablekey = "TYRTN" + mperiod.Substring(0, 2) + "001" + tyreStock.DocNo;
                            tyreStock.Parentkey = "TYRTN" + mperiod.Substring(0, 2) + tyreStock.DocNo;
                        }

                        tyreStock.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                        tyreStock.Branch = mbranchcode;
                        tyreStock.TransferToStock = Model.TransferToStock;
                        tyreStock.VehicleCode = Model.VehicleCode;
                        tyreStock.InstallFor = Model.InstallFor == "Tyre" ? "T" : "S";
                        tyreStock.TyrePlaceNo = Model.TyrePlaceNo;
                        tyreStock.TyreSerialNo = Model.TyreSerialNo;
                        tyreStock.InstallDate = ConvertDDMMYYTOYYMMDD(Model.InstallDate);
                        tyreStock.InstallKM = Model.InstallKM;

                        tyreStock.ENTEREDBY = muserid;
                        tyreStock.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                        tyreStock.AUTHORISE = mauthorise;
                        tyreStock.AUTHIDS = muserid;

                        #endregion


                        #region Update OldStock Stock 
                        //Old Stock 
                        var TyreStockSerial1 = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == Model.OldStkTablekey && x.ParentKey != tyreStock.Tablekey).OrderByDescending(x => x.RECORDKEY).FirstOrDefault();
                        TyreStockSerial1.IsActive = false;
                        ctxTFAT.Entry(TyreStockSerial1).State = EntityState.Modified;



                        #endregion

                        #region Update VehicleStock Vehicle To Stock
                        //If Previous Vehicle Stock Found
                        TyreStockSerial StockDetails = new TyreStockSerial();
                        if (Model.TransferToStock == "Vehicle")
                        {
                            StockDetails = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == Model.PreviousVehicleStatusTablekey && x.ParentKey != tyreStock.Tablekey && x.StockAt == "Vehicle").OrderByDescending(x => x.RECORDKEY).FirstOrDefault();

                            if (StockDetails != null)
                            {
                                StockDetails.IsActive = false;
                                ctxTFAT.Entry(StockDetails).State = EntityState.Modified;
                            }

                        }
                        else
                        {
                            StockDetails = null;
                        }
                        #endregion

                        var Vehiclemaster = ctxTFAT.VehicleMaster.Where(x => x.Code == Model.VehicleCode).FirstOrDefault();

                        #region Add New Stock


                        TyreStockSerial tyreStockSerial = new TyreStockSerial();
                        tyreStockSerial.Branch = mbranchcode;
                        tyreStockSerial.DocDate = tyreStock.DocDate;
                        tyreStockSerial.SerialNo = Model.TyreSerialNo;
                        tyreStockSerial.Status = Model.TransferToStock == "Vehicle" ? "Remould" : Model.TransferToStock;
                        tyreStockSerial.Value1 = (Model.InstallDate);
                        tyreStockSerial.Value2 = Convert.ToInt32(Model.InstallKM);
                        tyreStockSerial.Value3 = (Model.InstallDate);
                        tyreStockSerial.Value4 = Convert.ToInt32(Model.InstallKM);
                        tyreStockSerial.Vehicle = Vehiclemaster.PostAc;
                        tyreStockSerial.StepneeNo = Model.InstallFor == "Tyre" ? null : Model.TyrePlaceNo;
                        tyreStockSerial.TyreNo = Model.InstallFor == "Tyre" ? Model.TyrePlaceNo : null;
                        tyreStockSerial.StockAt = Model.TransferToStock == "Vehicle" ? "Vehicle" : "Stock";
                        tyreStockSerial.TableKey = tyreStock.Tablekey;
                        tyreStockSerial.ParentKey = TyreStockSerial1.TableKey;
                        tyreStockSerial.Sno = 1;
                        tyreStockSerial.ENTEREDBY = muserid;
                        tyreStockSerial.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                        tyreStockSerial.AUTHORISE = mauthorise;
                        tyreStockSerial.AUTHIDS = muserid;
                        tyreStockSerial.IsActive = true;
                        ctxTFAT.TyreStockSerial.Add(tyreStockSerial);

                        //Old Stock 
                        if (TyreStockSerial1 != null)
                        {
                            TyreStockSerial TSS3 = new TyreStockSerial();
                            TSS3.DocDate = tyreStock.DocDate;
                            TSS3.Branch = tyreStockSerial.Branch;
                            TSS3.SerialNo = tyreStockSerial.SerialNo;
                            TSS3.Status = "OutOfStock";
                            TSS3.Value1 = tyreStockSerial.Value1;
                            TSS3.Value2 = tyreStockSerial.Value2;
                            TSS3.Value3 = tyreStockSerial.Value3;
                            TSS3.Value4 = tyreStockSerial.Value4;
                            TSS3.Vehicle = TyreStockSerial1.Vehicle;
                            TSS3.TyreNo = TyreStockSerial1.TyreNo;
                            TSS3.StepneeNo = TyreStockSerial1.StepneeNo;
                            TSS3.StockAt = TyreStockSerial1.StockAt;
                            TSS3.TableKey = TyreStockSerial1.TableKey;
                            TSS3.ParentKey = tyreStockSerial.TableKey;
                            TSS3.Sno = tyreStockSerial.Sno;
                            TSS3.ENTEREDBY = muserid;
                            TSS3.AUTHIDS = muserid;
                            TSS3.AUTHORISE = "A00";
                            TSS3.LASTUPDATEDATE = DateTime.Now;
                            TSS3.IsActive = true;
                            ctxTFAT.TyreStockSerial.Add(TSS3);
                        }

                        //If Previous Vehicle Stock Found
                        if (StockDetails != null)
                        {
                            TyreStockSerial TSS3 = new TyreStockSerial();
                            TSS3.DocDate = tyreStock.DocDate;
                            TSS3.Branch = tyreStockSerial.Branch;
                            TSS3.SerialNo = StockDetails.SerialNo;
                            TSS3.Status = Model.PreviousVehicleStatusTransferToStock;
                            TSS3.Value1 = StockDetails.Value1;
                            TSS3.Value2 = StockDetails.Value2;
                            TSS3.Value3 = StockDetails.Value3;
                            TSS3.Value4 = StockDetails.Value4;
                            TSS3.Vehicle = StockDetails.Vehicle;
                            TSS3.TyreNo = StockDetails.TyreNo;
                            TSS3.StepneeNo = StockDetails.StepneeNo;
                            TSS3.StockAt = "Stock";
                            TSS3.TableKey = StockDetails.TableKey;
                            TSS3.ParentKey = tyreStockSerial.TableKey;
                            TSS3.Sno = tyreStockSerial.Sno;
                            TSS3.ENTEREDBY = muserid;
                            TSS3.AUTHIDS = muserid;
                            TSS3.AUTHORISE = "A00";
                            TSS3.LASTUPDATEDATE = DateTime.Now;
                            TSS3.IsActive = true;
                            ctxTFAT.TyreStockSerial.Add(TSS3);
                        }



                        #endregion

                        RelateData reldt1 = new RelateData();
                        reldt1.Amount = 0;
                        reldt1.AUTHIDS = muserid;
                        reldt1.AUTHORISE = mauthorise;
                        reldt1.Branch = mbranchcode;
                        reldt1.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                        reldt1.ENTEREDBY = muserid;
                        reldt1.Deleted = false;
                        reldt1.Type = "TYRTN";
                        reldt1.Srl = Convert.ToInt32(tyreStock.DocNo);
                        reldt1.Sno = "001";
                        reldt1.SubType = "TN";
                        reldt1.LASTUPDATEDATE = DateTime.Now;
                        reldt1.MainType = "TY";
                        reldt1.Code = " ";
                        reldt1.Narr = "";
                        reldt1.RelateTo = (byte)(4);
                        reldt1.Value1 = "Master";
                        reldt1.Value2 = Model.OldStkTypeName;
                        reldt1.Num1 = Convert.ToDecimal(Model.OldStkCost);
                        reldt1.Num4 = Convert.ToDecimal(Model.OldStkWarrantyKM);
                        reldt1.Value3 = Model.OldStkDays;
                        reldt1.Value4 = Model.InstallFor;
                        reldt1.Value5 = Model.TyrePlaceNo;
                        reldt1.Value6 = Model.TyreSerialNo;
                        reldt1.Value7 = Model.TransferToStock == "Vehicle" ? "Remould" : Model.TransferToStock;
                        reldt1.Date1 = null;
                        reldt1.Date2 = ConvertDDMMYYTOYYMMDD(Model.InstallDate);
                        reldt1.Num2 = Convert.ToDecimal(Model.InstallKM);
                        reldt1.Date3 = null;
                        reldt1.Num3 = Convert.ToDecimal(Model.InstallKM);
                        reldt1.Value8 = Vehiclemaster == null ? "" : Vehiclemaster.PostAc;
                        reldt1.Combo1 = null;
                        reldt1.TableKey = tyreStock.Tablekey;
                        reldt1.ParentKey = tyreStock.Parentkey;
                        reldt1.AmtType = false;
                        reldt1.ReqRelated = true;
                        reldt1.Status = false;
                        reldt1.Clear = false;
                        ctxTFAT.RelateData.Add(reldt1);

                    }
                    else
                    {

                        #region tyreStock Transfer

                        if (mAdd)
                        {
                            tyreStock.DocNo = GenerateCode();
                            tyreStock.Tablekey = "TYRTN" + mperiod.Substring(0, 2) + "001" + tyreStock.DocNo;
                            tyreStock.Parentkey = "TYRTN" + mperiod.Substring(0, 2) + tyreStock.DocNo;
                            tyreStock.Prefix = mperiod;
                        }

                        tyreStock.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                        tyreStock.Branch = mbranchcode;
                        tyreStock.TransferToStock = "New";
                        tyreStock.VehicleCode = Model.RelatedChoice;
                        tyreStock.InstallFor = Model.AddOnList.Where(x => x.Fld == "F006").Select(x => x.ApplCode).FirstOrDefault() == null ? "" : Model.AddOnList.Where(x => x.Fld == "F006").Select(x => x.ApplCode).FirstOrDefault() == "Tyre" ? "T" : "S";
                        tyreStock.TyrePlaceNo = Model.AddOnList.Where(x => x.Fld == "F007").Select(x => x.ApplCode).FirstOrDefault();
                        tyreStock.TyreSerialNo = Model.AddOnList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault();
                        tyreStock.InstallDate = ConvertYYMMDDTODDMMYY(Model.AddOnList.Where(x => x.Fld == "F010").Select(x => x.ApplCode).FirstOrDefault());
                        tyreStock.InstallKM = Model.AddOnList.Where(x => x.Fld == "F011").Select(x => x.ApplCode).FirstOrDefault();

                        tyreStock.ENTEREDBY = muserid;
                        tyreStock.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                        tyreStock.AUTHORISE = mauthorise;
                        tyreStock.AUTHIDS = muserid;

                        #endregion

                        if (Model.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault() == "Direct")
                        {
                            var Code = Model.AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault();
                            TyreMaster tyreMaster = ctxTFAT.TyreMaster.Where(x => x.TyreType == Code).FirstOrDefault();
                            if (tyreMaster == null)
                            {
                                tyreMaster = new TyreMaster();
                                tyreMaster.Name = Model.AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault();

                                var mdecnum42 = Model.AddOnList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault();
                                tyreMaster.KM = Convert.ToDouble((string.IsNullOrEmpty(mdecnum42) == true) ? 0 : Convert.ToDecimal(mdecnum42));

                                tyreMaster.ExpiryDays = Convert.ToInt32(Model.AddOnList.Where(x => x.Fld == "F005").Select(x => x.ApplCode).FirstOrDefault());
                                tyreMaster.Active = true;
                                tyreMaster.TyreType = Model.AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault();

                                var mdecnum12 = Model.AddOnList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                                tyreMaster.Cost = (string.IsNullOrEmpty(mdecnum12) == true) ? 0 : Convert.ToDecimal(mdecnum12);

                                var mDATE32 = Model.AddOnList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                                tyreMaster.ExpiryDate = (string.IsNullOrEmpty(mDATE32) == true) ? Convert.ToDateTime("1900-01-01") : Convert.ToDateTime(mDATE32);

                                tyreMaster.AUTHIDS = muserid;
                                tyreMaster.AUTHORISE = mauthorise;
                                tyreMaster.ENTEREDBY = muserid;
                                tyreMaster.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                                int NewCode1;
                                var NewCode = ctxTFAT.TyreMaster.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).Take(1).FirstOrDefault();
                                if (NewCode == null || NewCode == "")
                                {
                                    NewCode1 = 100000;
                                }
                                else
                                {
                                    NewCode1 = Convert.ToInt32(NewCode) + 1;
                                }
                                string FinalCode = NewCode1.ToString("D6");
                                tyreMaster.Code = FinalCode;
                                ctxTFAT.TyreMaster.Add(tyreMaster);
                            }
                        }


                        #region Update VehicleStock Vehicle To Stock
                        //If Previous Vehicle Stock Found
                        TyreStockSerial StockDetails = new TyreStockSerial();
                        if (!(String.IsNullOrEmpty(Model.NPreviousTablekey)))
                        {
                            StockDetails = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == Model.NPreviousTablekey && x.ParentKey != tyreStock.Tablekey && x.StockAt == "Vehicle").OrderByDescending(x => x.RECORDKEY).FirstOrDefault();

                            if (StockDetails != null)
                            {
                                StockDetails.IsActive = false;
                                ctxTFAT.Entry(StockDetails).State = EntityState.Modified;
                            }

                        }
                        else
                        {
                            StockDetails = null;
                        }
                        #endregion

                        var Vehiclemaster = ctxTFAT.VehicleMaster.Where(x => x.Code == Model.RelatedChoice).FirstOrDefault();

                        #region Add New Stock


                        #region New Stock Entry
                        var mInstDATE = Model.AddOnList.Where(x => x.Fld == "F010").Select(x => x.ApplCode).FirstOrDefault();
                        TyreStockSerial TSS2 = new TyreStockSerial();
                        TSS2.Branch = mbranchcode;
                        TSS2.DocDate = tyreStock.DocDate;
                        TSS2.SerialNo = Model.AddOnList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault() ?? "";
                        TSS2.Status = Model.AddOnList.Where(x => x.Fld == "F014").Select(x => x.ApplCode).FirstOrDefault();
                        TSS2.Value1 = Model.AddOnList.Where(x => x.Fld == "F010").Select(x => x.ApplCode).FirstOrDefault();
                        TSS2.Value2 = Model.AddOnList.Where(x => x.Fld == "F011").Select(x => x.ApplCode).FirstOrDefault() == null ? 0 : Convert.ToDecimal(Model.AddOnList.Where(x => x.Fld == "F011").Select(x => x.ApplCode).FirstOrDefault());
                        TSS2.Value3 = Model.AddOnList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                        TSS2.Value4 = Model.AddOnList.Where(x => x.Fld == "F013").Select(x => x.ApplCode).FirstOrDefault() == null ? 0 : Convert.ToDecimal(Model.AddOnList.Where(x => x.Fld == "F013").Select(x => x.ApplCode).FirstOrDefault());
                        TSS2.Vehicle = Vehiclemaster == null ? "" : Vehiclemaster.PostAc;
                        if (Model.AddOnList.Where(x => x.Fld == "F006").Select(x => x.ApplCode).FirstOrDefault() == "Tyre")
                        {
                            TSS2.TyreNo = Model.AddOnList.Where(x => x.Fld == "F007").Select(x => x.ApplCode).FirstOrDefault();
                        }
                        else
                        {
                            TSS2.StepneeNo = Model.AddOnList.Where(x => x.Fld == "F007").Select(x => x.ApplCode).FirstOrDefault();
                        }
                        TSS2.StockAt = (Model.RelatedChoice == "Tyrestock" || Model.RelatedChoice == "Stock") ? "Stock" : "Vehicle";
                        TSS2.TableKey = tyreStock.Tablekey;
                        TSS2.ParentKey = StockDetails == null ? tyreStock.Parentkey : StockDetails.TableKey;
                        TSS2.Sno = 1;
                        TSS2.ENTEREDBY = muserid;
                        TSS2.AUTHIDS = muserid;
                        TSS2.AUTHORISE = "A00";
                        TSS2.LASTUPDATEDATE = DateTime.Now;
                        TSS2.IsActive = true;
                        ctxTFAT.TyreStockSerial.Add(TSS2);
                        #endregion

                        #region Update New Stock
                        if (!(String.IsNullOrEmpty(Model.NPreviousTablekey)))
                        {
                            TyreStockSerial TSS3 = new TyreStockSerial();
                            TSS3.DocDate = (tyreStock.DocDate);
                            TSS3.Branch = TSS2.Branch;
                            TSS3.SerialNo = TSS2.SerialNo;
                            TSS3.Status = Model.NPreviousTransferToStock;
                            TSS3.Value1 = TSS2.Value1;
                            TSS3.Value2 = TSS2.Value2;
                            TSS3.Value3 = TSS2.Value3;
                            TSS3.Value4 = TSS2.Value4;
                            TSS3.Vehicle = TSS2.Vehicle;
                            TSS3.TyreNo = TSS2.TyreNo;
                            TSS3.StepneeNo = TSS2.StepneeNo;
                            TSS3.StockAt = "Stock";
                            TSS3.TableKey = Model.NPreviousTablekey;
                            TSS3.ParentKey = TSS2.TableKey;
                            TSS3.Sno = TSS2.Sno;
                            TSS3.ENTEREDBY = muserid;
                            TSS3.AUTHIDS = muserid;
                            TSS3.AUTHORISE = "A00";
                            TSS3.LASTUPDATEDATE = DateTime.Now;
                            TSS3.IsActive = true;
                            ctxTFAT.TyreStockSerial.Add(TSS3);
                        }

                        #endregion


                        #endregion

                    }
                    if (Model.AddOnList != null)
                    {
                        RelateData reldt = new RelateData();
                        reldt.Amount = 0;
                        reldt.AUTHIDS = muserid;
                        reldt.AUTHORISE = mauthorise;
                        reldt.Branch = tyreStock.Branch;
                        reldt.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                        reldt.ENTEREDBY = muserid;
                        reldt.Deleted = false;
                        reldt.Type = Model.Type;
                        reldt.Srl = Convert.ToInt32(tyreStock.DocNo);
                        reldt.Sno = 1.ToString("D3");
                        reldt.SubType = Model.SubType;
                        reldt.LASTUPDATEDATE = DateTime.Now;
                        reldt.MainType = Model.MainType;
                        reldt.Code = " ";
                        reldt.Narr = " ";
                        reldt.RelateTo = (byte)(4);
                        reldt.Value1 = Model.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                        reldt.Value2 = Model.AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault();

                        var mdecnum1 = Model.AddOnList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                        reldt.Num1 = (string.IsNullOrEmpty(mdecnum1) == true) ? 0 : Convert.ToDecimal(mdecnum1);
                        var mdecnum4 = Model.AddOnList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault();
                        reldt.Num4 = (string.IsNullOrEmpty(mdecnum4) == true) ? 0 : Convert.ToDecimal(mdecnum4);

                        reldt.Value3 = Model.AddOnList.Where(x => x.Fld == "F005").Select(x => x.ApplCode).FirstOrDefault();
                        reldt.Value4 = Model.AddOnList.Where(x => x.Fld == "F006").Select(x => x.ApplCode).FirstOrDefault();
                        reldt.Value5 = Model.AddOnList.Where(x => x.Fld == "F007").Select(x => x.ApplCode).FirstOrDefault();
                        reldt.Value6 = Model.AddOnList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault();
                        reldt.Value7 = Model.AddOnList.Where(x => x.Fld == "F014").Select(x => x.ApplCode).FirstOrDefault();

                        var mDATE = Model.AddOnList.Where(x => x.Fld == "F009").Select(x => x.ApplCode).FirstOrDefault();
                        reldt.Date1 = (string.IsNullOrEmpty(mDATE) == true) ? Convert.ToDateTime("1900-01-01") : Convert.ToDateTime(mDATE);
                        var mDATE2 = Model.AddOnList.Where(x => x.Fld == "F010").Select(x => x.ApplCode).FirstOrDefault();
                        reldt.Date2 = (string.IsNullOrEmpty(mDATE2) == true) ? Convert.ToDateTime("1900-01-01") : Convert.ToDateTime(mDATE2);

                        var mdecnum2 = Model.AddOnList.Where(x => x.Fld == "F011").Select(x => x.ApplCode).FirstOrDefault();
                        reldt.Num2 = (string.IsNullOrEmpty(mdecnum2) == true) ? 0 : Convert.ToDecimal(mdecnum2);

                        var mDATE3 = Model.AddOnList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                        reldt.Date3 = (string.IsNullOrEmpty(mDATE3) == true) ? Convert.ToDateTime("1900-01-01") : Convert.ToDateTime(mDATE3);

                        var mdecnum3 = Model.AddOnList.Where(x => x.Fld == "F013").Select(x => x.ApplCode).FirstOrDefault();
                        reldt.Num3 = (string.IsNullOrEmpty(mdecnum3) == true) ? 0 : Convert.ToDecimal(mdecnum3);
                        reldt.Value8 = Model.RelatedChoice;//Vehicle NO
                        reldt.Combo1 = null;//OTher Cost Account
                        reldt.TableKey = tyreStock.Tablekey;
                        reldt.ParentKey = tyreStock.Parentkey;
                        reldt.AmtType = true;
                        reldt.ReqRelated = true;
                        reldt.Status = false;
                        reldt.Clear = false;

                        ctxTFAT.RelateData.Add(reldt);
                    }




                    if (mAdd)
                    {
                        ctxTFAT.TyreStockTransfer.Add(tyreStock);
                    }
                    else
                    {
                        ctxTFAT.Entry(tyreStock).State = EntityState.Modified;
                    }

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, tyreStock.Parentkey, DateTime.Now, 0,"", "Save TyreStock Transfer", "NA");

                    return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
                }

                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    string dd1 = ex1.InnerException.Message;
                    return Json(new
                    {
                        Status = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    string dd = ex.InnerException.InnerException.Message;
                    return Json(new
                    {
                        Status = "Error",
                        Message = dd
                    }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult DeleteData(TyreStockTransferVM Model)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction(System.Data.IsolationLevel.Unspecified))
            {
                try
                {

                    TyreStockTransfer tyreStockTransfer = ctxTFAT.TyreStockTransfer.Where(x => x.DocNo == Model.Document).FirstOrDefault();

                    DeUpdate(Model);

                    UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, tyreStockTransfer.Parentkey, DateTime.Now, 0, "", "Delete TyreStock Transfer", "NA");

                    //UpdateAuditTrail(mbranchcode, "Delete", Model.Header, Model.ParentKey, DateTime.Now, 0, Model.ParentKey, "");

                    transaction.Commit();
                    transaction.Dispose();
                    //SendTrnsMsg("Delete", Model.Amt, mbranchcode + Model.ParentKey, ConvertDDMMYYTOYYMMDD(Model.DocuDate), "");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    return Json(new
                    {
                        Status = "Error",
                        Message = ex1.InnerException.InnerException.Message
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    return Json(new
                    {
                        Status = "Error",
                        Message = ex.InnerException.InnerException.Message
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new
            {
                Status = "Success",
                Message = "The Document is Deleted."
            }, JsonRequestBehavior.AllowGet);
        }

        public List<AddOns> AddNewStock(TyreStockTransferVM Model)
        {
            List<AddOns> truckaddonlist = new List<AddOns>();
            //Tyre Details

            List<string> tyretypeList = new List<string>();
            tyretypeList.Add("New");
            tyretypeList.Add("Remould");
            tyretypeList.Add("Scrap");
            tyretypeList.Add("Sale");
            tyretypeList.Add("OutOfStock");
            tyretypeList.Add("Stock");

            var mTyreTypes = ctxTFAT.TyreMaster.Select(x => x.TyreType).Distinct().ToList();
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F001",
                Head = "From",
                ApplCode = Model.FromType,
                QueryText = "Master^Direct",
                FldType = "C"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F002",
                Head = "Product",
                ApplCode = "",
                QueryText = "Select^" + String.Join("^", mTyreTypes),
                FldType = Model.FromType == "Direct" ? "T" : "C",
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F003",
                Head = "Cost",
                ApplCode = "",
                QueryText = "",
                FldType = "T"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F014",
                Head = "Tyre Type",
                ApplCode = "",
                QueryText = "Select^" + String.Join("^", tyretypeList),
                FldType = "C",
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F004",
                Head = "KM",
                ApplCode = "",
                QueryText = "",
                FldType = "T"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F005",
                Head = "Expiry Days",
                ApplCode = "",
                QueryText = "",
                FldType = "T"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F006",
                Head = "Install For",
                ApplCode = "",
                QueryText = "Tyre^Stepnee",
                FldType = "R"

            });


            truckaddonlist.Add(new AddOns()
            {
                Fld = "F007",
                Head = "Tyre Placed No",
                ApplCode = "",
                QueryText = "",
                FldType = "T"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F008",
                Head = "Tyre SerialNo",
                ApplCode = "",
                QueryText = "",
                FldType = "T"

            });



            truckaddonlist.Add(new AddOns()
            {
                Fld = "F009",
                Head = "Mfg Date",
                ApplCode = DateTime.Now.ToString("yyyy-MM-dd"),
                QueryText = "",
                FldType = "D"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F010",
                Head = "Install Date",
                ApplCode = DateTime.Now.ToString("yyyy-MM-dd"),
                QueryText = "",
                FldType = "D"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F011",
                Head = "Install KM",
                ApplCode = "0",
                QueryText = "",
                FldType = "N"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F012",
                Head = "Expiry Date",
                ApplCode = DateTime.Now.ToString("yyyy-MM-dd"),
                QueryText = "",
                FldType = "D"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F013",
                Head = "Expiry KM",
                ApplCode = "0",
                QueryText = "",
                FldType = "N"

            });
            return truckaddonlist;
        }

        [HttpPost]
        public ActionResult GetTruckTyreSingleViewList(TyreStockTransferVM Model)
        {
            List<AddOns> truckaddonlist = new List<AddOns>();
            //Tyre Details

            List<string> tyretypeList = new List<string>();
            tyretypeList.Add("New");
            tyretypeList.Add("Remould");
            tyretypeList.Add("Scrap");
            tyretypeList.Add("Sale");
            tyretypeList.Add("OutOfStock");
            tyretypeList.Add("Stock");

            var mTyreTypes = ctxTFAT.TyreMaster.Select(x => x.TyreType).Distinct().ToList();
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F001",
                Head = "From",
                ApplCode = Model.FromType,
                QueryText = "Master^Direct",
                FldType = "C"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F002",
                Head = "Product",
                ApplCode = "",
                QueryText = "Select^" + String.Join("^", mTyreTypes),
                FldType = Model.FromType == "Direct" ? "T" : "C",
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F003",
                Head = "Cost",
                ApplCode = "",
                QueryText = "",
                FldType = "T"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F014",
                Head = "Tyre Type",
                ApplCode = "",
                QueryText = "Select^" + String.Join("^", tyretypeList),
                FldType = "C",
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F004",
                Head = "KM",
                ApplCode = "",
                QueryText = "",
                FldType = "T"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F005",
                Head = "Expiry Days",
                ApplCode = "",
                QueryText = "",
                FldType = "T"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F006",
                Head = "Install For",
                ApplCode = "",
                QueryText = "Tyre^Stepnee",
                FldType = "R"

            });


            truckaddonlist.Add(new AddOns()
            {
                Fld = "F007",
                Head = "Tyre Placed No",
                ApplCode = "",
                QueryText = "",
                FldType = "T"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F008",
                Head = "Tyre SerialNo",
                ApplCode = "",
                QueryText = "",
                FldType = "T"

            });

            truckaddonlist.Add(new AddOns()
            {
                Fld = "F009",
                Head = "Mfg Date",
                ApplCode = Model.DocDateA.ToString("yyyy-MM-dd"),
                QueryText = "",
                FldType = "D"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F010",
                Head = "Install Date",
                ApplCode = Model.DocDateA.ToString("yyyy-MM-dd"),
                QueryText = "",
                FldType = "D"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F011",
                Head = "Install KM",
                ApplCode = "0",
                QueryText = "",
                FldType = "N"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F012",
                Head = "Expiry Date",
                ApplCode = "",
                QueryText = "",
                FldType = "D"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F013",
                Head = "Expiry KM",
                ApplCode = "0",
                QueryText = "",
                FldType = "N"

            });

            var html = ViewHelper.RenderPartialView(this, "_NewStockPartialView", new TyreStockTransferVM() { AddOnList = truckaddonlist });
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetTyreDetail(TyreStockTransferVM Model)
        {
            var mSpare = ctxTFAT.TyreMaster.Where(x => x.TyreType == Model.Type).Select(x => x).FirstOrDefault();
            var mExpdate = DateTime.Now.Date;
            var mDate = Convert.ToDateTime(Model.FromDate);
            decimal McOST = 0;
            double mKm = 0;
            int mexpdays = 0;
            if (mSpare != null)
            {
                mexpdays = mSpare.ExpiryDays == null ? 0 : mSpare.ExpiryDays.Value;
                mKm = mSpare.KM == null ? 0 : mSpare.KM.Value;
                McOST = mSpare.Cost == null ? 0 : mSpare.Cost.Value;
            }

            mExpdate = mDate.AddDays(mexpdays);

            return Json(new
            {

                KM = mKm,
                Days = mexpdays,
                Cost = McOST,
                ExpDays = mExpdate.ToString("yyyy-MM-dd"),

            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetCalcTyreDetail(TyreStockTransferVM Model)
        {
            var mExpdate = DateTime.Now.Date;
            var mDate = Convert.ToDateTime(Model.FromDate);
            var mexpdays = Convert.ToInt32(Model.ExpDays);
            var mKm = Model.KM;

            mExpdate = mDate.AddDays(mexpdays);

            return Json(new
            {

                KM = mKm,
                Days = mexpdays,
                ExpDays = mExpdate.ToString("yyyy-MM-dd"),

            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetTyreStockDetail(TyreStockTransferVM Model)
        {

            string Status = "Success";
            string Message = "";

            var Vehiclemaster = ctxTFAT.VehicleMaster.Where(x => x.Code == Model.VehicleList).FirstOrDefault();
            if (Vehiclemaster != null)
            {
                if (Model.InstallFor == "Tyre")
                {
                    if (!(Convert.ToInt32(Model.TyrePlaceNo) <= Vehiclemaster.NoOfTyres))
                    {
                        Status = "Error";
                        Message = "Vehicle Have Only " + Vehiclemaster.NoOfTyres + " Tyres....";
                    }
                }
                else
                {
                    if (!(Convert.ToInt32(Model.TyrePlaceNo) <= Vehiclemaster.Stepney))
                    {
                        Status = "Error";
                        Message = "Vehicle Have Only " + Vehiclemaster.Stepney + " Stepnees....";
                    }
                }
            }

            if (Status != "Error")
            {
                TyreStockSerial StockDetails = new TyreStockSerial();
                if (Model.InstallFor == "Tyre" && String.IsNullOrEmpty(Model.TyrePlaceNo) == false && Vehiclemaster != null)
                {
                    StockDetails = ctxTFAT.TyreStockSerial.Where(x => x.TyreNo == Model.TyrePlaceNo && x.Vehicle == Vehiclemaster.PostAc && x.StockAt == "Vehicle" && x.IsActive == true).OrderByDescending(x => x.RECORDKEY).FirstOrDefault();
                }
                else if (Model.InstallFor == "Stepnee" && String.IsNullOrEmpty(Model.TyrePlaceNo) == false && Vehiclemaster != null)
                {
                    StockDetails = ctxTFAT.TyreStockSerial.Where(x => x.StepneeNo == Model.TyrePlaceNo && x.Vehicle == Vehiclemaster.PostAc && x.StockAt == "Vehicle" && x.IsActive == true).OrderByDescending(x => x.RECORDKEY).FirstOrDefault();
                }
                else
                {
                    StockDetails = null;
                }

                if (StockDetails != null)
                {
                    Model.NPreviousTransferToStock = StockDetails.Status;
                    Model.NPreviousStockSerial = StockDetails.SerialNo;
                    Model.NPreviousStockAt = StockDetails.StockAt;
                    Model.NPreviousInstallDate = StockDetails.Value1;
                    Model.NPreviousInstallKM = StockDetails.Value2.Value.ToString();
                    Model.NPreviousExpirtDate = StockDetails.Value3;
                    Model.NPreviousExpiryKM = StockDetails.Value4.Value.ToString();
                    Model.NPreviousIsActive = StockDetails.IsActive == true ? "Active" : "Not Active";
                    Model.NPreviousTablekey = StockDetails.TableKey;
                    Model.NPreviousBranch = StockDetails.Branch;
                    //Model.PreviousVehicleStatusParentkey = StockDetails.ParentKey;
                }
                else
                {
                    Model.PreviousVehicleStatusStockAt = null;
                }
            }
            else
            {
                Model.PreviousVehicleStatusStockAt = null;
            }
            var html = ViewHelper.RenderPartialView(this, "_NewTyreStockPartialView", Model);

            return Json(new
            {
                Status = Status,
                Html = html,
                Message = Message
            }, JsonRequestBehavior.AllowGet);
        }

        public List<AddOns> GetTruckWiseData(string TableKey, string Branch)
        {
            var mRelateData = ctxTFAT.RelateData.Where(x => x.TableKey == TableKey && x.Branch == Branch).Select(x => x).FirstOrDefault();
            List<AddOns> truckaddonlist = new List<AddOns>();
            //Tyre Details
            var mTyreTypes = ctxTFAT.TyreMaster.Select(x => x.TyreType).Distinct().ToList();

            List<string> tyretypeList = new List<string>();
            tyretypeList.Add("New");
            tyretypeList.Add("Remould");
            tyretypeList.Add("Scrap");
            tyretypeList.Add("Sale");
            tyretypeList.Add("OutOfStock");
            tyretypeList.Add("Stock");

            truckaddonlist.Add(new AddOns()
            {
                Fld = "F001",
                Head = "From",
                ApplCode = mRelateData.Value1,
                QueryText = "Master^Direct",
                FldType = "C"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F002",
                Head = "Product",
                ApplCode = mRelateData.Value2,
                QueryText = "Select^" + String.Join("^", mTyreTypes),
                FldType = mRelateData.Value1 == "Master" ? "C" : "T",
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F003",
                Head = "Cost",
                ApplCode = mRelateData.Num1.Value.ToString(),
                QueryText = "",
                FldType = "N"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F014",
                Head = "Tyre Type",
                ApplCode = mRelateData.Value7,
                QueryText = "Select^" + String.Join("^", tyretypeList),
                FldType = "C",
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F004",
                Head = "Warranty KM",
                ApplCode = mRelateData.Num4 == null ? "0" : mRelateData.Num4.Value.ToString(),
                QueryText = "",
                FldType = "N"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F005",
                Head = "Warranty Days",
                ApplCode = mRelateData.Value3,
                QueryText = "",
                FldType = "N"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F006",
                Head = "Install For",
                ApplCode = mRelateData.Value4,
                QueryText = "Tyre^Stepnee",
                FldType = "R"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F007",
                Head = "Tyre Placed No",
                ApplCode = mRelateData.Value5,
                QueryText = "",
                FldType = "N"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F008",
                Head = "Tyre SerialNo",
                ApplCode = mRelateData.Value6,
                QueryText = "",
                FldType = "T"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F009",
                Head = "Mfg Date",
                ApplCode = mRelateData.Date1.Value.ToString("yyyy-MM-dd"),
                QueryText = "",
                FldType = "D"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F010",
                Head = "Install Date",
                ApplCode = mRelateData.Date2.Value.ToString("yyyy-MM-dd"),
                QueryText = "",
                FldType = "D"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F011",
                Head = "Install KM",
                ApplCode = mRelateData.Num2.Value.ToString(),
                QueryText = "",
                FldType = "N"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F012",
                Head = "Expiry Date",
                ApplCode = mRelateData.Date3.Value.ToString("yyyy-MM-dd"),
                QueryText = "",
                FldType = "D"

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F013",
                Head = "Expiry KM",
                ApplCode = mRelateData.Num3 == null ? "0" : mRelateData.Num3.Value.ToString(),
                QueryText = "",
                FldType = "N"
            });


            return truckaddonlist;
        }

    }
}