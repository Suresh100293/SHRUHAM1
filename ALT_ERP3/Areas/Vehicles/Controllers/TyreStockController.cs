using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Vehicles.Models;
using ALT_ERP3.Controllers;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Vehicles.Controllers
{
    public class TyreStockController : BaseController
    {
        private static string mauthorise = "A00";


        #region Functions

        public ActionResult PopulateVehicleNew(string term)
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            if (term == "" || term == null)
            {
                var result = ctxTFAT.VehicleMaster.Where(x => x.Acitve == true && x.Code != "99998" && x.Code != "99999").Select(x => new { x.Code, x.TruckNo }).ToList().Take(10);
                foreach (var item in result)
                {
                    GSt.Add(new SelectListItem { Value = item.Code, Text = item.TruckNo });
                }
                GSt.Add(new SelectListItem { Value = "Tyrestock", Text = "Tyre Stock" });
                GSt.Add(new SelectListItem { Value = "Remould", Text = "Remould" });
                var Modified = GSt.Select(x => new
                {
                    Code = x.Value,
                    Name = x.Text
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.VehicleMaster.Where(x => x.Acitve == true && x.TruckNo.Contains(term) && x.Code != "99998" && x.Code != "99999").Select(x => new { x.Code, x.TruckNo }).ToList().Take(10);
                foreach (var item in result)
                {
                    GSt.Add(new SelectListItem { Value = item.Code, Text = item.TruckNo });
                }
                GSt.Add(new SelectListItem { Value = "Tyrestock", Text = "Tyre Stock" });
                GSt.Add(new SelectListItem { Value = "Remould", Text = "Remould" });


                var Modified = GSt.Select(x => new
                {
                    Code = x.Value,
                    Name = x.Text
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
        }

        public string GenerateCode()
        {
            var LastCode = ctxTFAT.TyreStockTransfer.OrderByDescending(x => x.DocNo).Select(x => x.DocNo).FirstOrDefault();
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

        // GET: Vehicles/TyreStock
        public ActionResult Index(OtherTransactModel mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.TyreStockTransfer.Where(x => x.Tablekey == mModel.Document).FirstOrDefault();
                if (mList != null)
                {
                    var item = ctxTFAT.RelateData.Where(x => x.TableKey == mList.Tablekey).FirstOrDefault();
                    mModel.AddOnList = GetTruckWiseData(item.TableKey);
                    mModel.ItemList = GetItemWiseData(item.TableKey);
                    mModel.TyreStockList = GetTyreStockSerialList(item.TableKey, item.Branch);
                    mModel.RelatedChoice = item.Code;
                    mModel.TableKey = item.TableKey;
                    mModel.RelatedChoice = mList.VehicleCode;
                    mModel.RelatedChoiceN = ctxTFAT.VehicleMaster.Where(x => x.Code == mList.VehicleCode).Select(x => x.TruckNo).FirstOrDefault();
                    if (mModel.RelatedChoice == "Tyrestock")
                    {
                        mModel.RelatedChoiceN = "Tyre Stock";
                    }
                    else if (mModel.RelatedChoice == "Remould")
                    {
                        mModel.RelatedChoiceN = "Remould";
                    }
                }
            }
            else
            {
                List<AddOns> truckaddonlist = new List<AddOns>();
                mModel.ProductGroupType = "000009";
                var mItem = ctxTFAT.ItemMaster.Where(x => x.BaseGr == mModel.ProductGroupType).Select(x => new { x.Name, x.Code }).Distinct().ToList();
                var mItemGroup = ctxTFAT.ItemGroups.Where(x => x.Hide == false).Select(x => new { x.Name, x.Code }).Distinct().ToList();
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "Product Group",
                    ApplCode = mModel.ProductGroupType,
                    QueryText = "Select^" + String.Join("^", mItemGroup.Select(x => x.Name).ToList()),
                    QueryCode = "Select^" + String.Join("^", mItemGroup.Select(x => x.Code).ToList()),
                    FldType = "C",
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F002",
                    Head = "Pick From",
                    ApplCode = mModel.FromType,
                    QueryText = "Master^Direct",
                    QueryCode = "Master^Direct",
                    FldType = "C"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F003",
                    Head = "Item",
                    ApplCode = "",
                    QueryText = "Select^" + String.Join("^", mItem.Select(x => x.Name).ToList()),
                    QueryCode = "Select^" + String.Join("^", mItem.Select(x => x.Code).ToList()),
                    FldType = mModel.FromType == "Direct" ? "T" : "C",
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F004",
                    Head = "Cost",
                    ApplCode = "0",
                    QueryText = "",
                    QueryCode = "",
                    FldType = "N",
                    InActive = true
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F005",
                    Head = "Qty",
                    ApplCode = "1",
                    QueryText = "",
                    QueryCode = "",
                    FldType = "N",
                    InActive = true
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F006",
                    Head = "Total Amount",
                    ApplCode = "0",
                    QueryText = "",
                    QueryCode = "",
                    FldType = "N",
                    InActive = true
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F007",
                    Head = "Warranty KM",
                    ApplCode = "0",
                    QueryText = "",
                    FldType = "N"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F008",
                    Head = "Current KM",
                    ApplCode = "0",
                    QueryText = "",
                    FldType = "N",

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F009",
                    Head = "Due km",
                    ApplCode = "0",
                    QueryText = "",
                    FldType = "N"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F010",
                    Head = "Warranty Days",
                    ApplCode = "0",
                    QueryText = "",
                    FldType = "N",

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F011",
                    Head = "MFG Date",
                    ApplCode = DateTime.Now.ToString("yyyy-MM-dd"),
                    QueryText = "",
                    FldType = "D",
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F012",
                    Head = "Install / Received Date",
                    ApplCode = DateTime.Now.ToString("yyyy-MM-dd"),
                    QueryText = "",
                    FldType = "D"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F013",
                    Head = "Due Date",
                    ApplCode = DateTime.Now.ToString("yyyy-MM-dd"),
                    QueryText = "",
                    FldType = "D",

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F014",
                    Head = "HSN CODE",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "X",
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F015",
                    Head = "Description",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "M",
                });
                mModel.ItemList = truckaddonlist;

                truckaddonlist = new List<AddOns>();
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
                    Head = "Tyre Type",
                    ApplCode = "",
                    QueryText = "Select^" + String.Join("^", tyretypeList),
                    QueryCode = "Select^" + String.Join("^", tyretypeList),
                    FldType = "C",
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F002",
                    Head = "Install For",
                    ApplCode = "",
                    QueryText = "Tyre^Stepnee",
                    FldType = "R"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F003",
                    Head = "Tyre Placed No",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "N"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F004",
                    Head = "Tyre SerialNo",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"
                });
                mModel.AddOnList = truckaddonlist;
            }
            return View(mModel);
        }

        public List<AddOns> GetTruckWiseData(string TableKey)
        {
            var mRelateData = ctxTFAT.RelateData.Where(x => x.TableKey == TableKey).Select(x => x).FirstOrDefault();
            List<AddOns> truckaddonlist = new List<AddOns>();

            //Tyre Details
            var mTyreTypes = ctxTFAT.ItemMaster.Where(x => x.BaseGr == mRelateData.Char1).Select(x => new { x.Name, x.Code }).Distinct().ToList();
            var mItemGroup = ctxTFAT.ItemGroups.Where(x => x.Hide == false).Select(x => new { x.Name, x.Code }).Distinct().ToList();

            List<string> tyretypeList = new List<string>();
            tyretypeList.Add("New");
            tyretypeList.Add("Remould");
            tyretypeList.Add("Scrap");
            tyretypeList.Add("Sale");
            tyretypeList.Add("OutOfStock");
            tyretypeList.Add("Stock");
            if (!String.IsNullOrEmpty(mRelateData.Value4))
            {
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "Tyre Type",
                    ApplCode = mRelateData.Value7,
                    QueryText = "Select^" + String.Join("^", tyretypeList),
                    QueryCode = "Select^" + String.Join("^", tyretypeList),
                    FldType = "C",
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F002",
                    Head = "Install For",
                    ApplCode = mRelateData.Value4,
                    QueryText = "Tyre^Stepnee",
                    FldType = "R"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F003",
                    Head = "Tyre Placed No",
                    ApplCode = mRelateData.Value5,
                    QueryText = "",
                    FldType = "N"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F004",
                    Head = "Tyre SerialNo",
                    ApplCode = mRelateData.Value6,
                    QueryText = "",
                    FldType = "T"
                });
            }

            return truckaddonlist;
        }

        public List<AddOns> GetItemWiseData(string TableKey)
        {
            var mRelateData = ctxTFAT.RelateDataItem.Where(x => x.TableKey == TableKey).Select(x => x).FirstOrDefault();
            List<AddOns> truckaddonlist = new List<AddOns>();

            if (mRelateData != null)
            {
                var mItem = ctxTFAT.ItemMaster.Where(x => x.BaseGr == mRelateData.ProductGroup).Select(x => new { x.Name, x.Code }).Distinct().ToList();
                var mItemGroup = ctxTFAT.ItemGroups.Where(x => x.Hide == false).Select(x => new { x.Name, x.Code }).Distinct().ToList();
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "Product Group",
                    ApplCode = mRelateData.ProductGroup,
                    QueryText = "Select^" + String.Join("^", mItemGroup.Select(x => x.Name).ToList()),
                    QueryCode = "Select^" + String.Join("^", mItemGroup.Select(x => x.Code).ToList()),
                    FldType = "C",
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F002",
                    Head = "Pick From",
                    ApplCode = "Master",
                    QueryText = "Master^Direct",
                    QueryCode = "Master^Direct",
                    FldType = "C"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F003",
                    Head = "Item",
                    ApplCode = mRelateData.Item,
                    QueryText = "Select^" + String.Join("^", mItem.Select(x => x.Name).ToList()),
                    QueryCode = "Select^" + String.Join("^", mItem.Select(x => x.Code).ToList()),
                    FldType = "C"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F004",
                    Head = "Cost",
                    ApplCode = mRelateData.Cost.ToString(),
                    QueryText = "",
                    QueryCode = "",
                    FldType = "N",
                    InActive = true
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F005",
                    Head = "Qty",
                    ApplCode = mRelateData.Qty,
                    QueryText = "",
                    QueryCode = "",
                    FldType = "N",
                    InActive = true
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F006",
                    Head = "Total Amount",
                    ApplCode = mRelateData.TotalAmout.ToString(),
                    QueryText = "",
                    QueryCode = "",
                    FldType = "N",
                    InActive = true
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F007",
                    Head = "Warranty KM",
                    ApplCode = mRelateData.WarrantyKm,
                    QueryText = "",
                    FldType = "N"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F008",
                    Head = "Current KM",
                    ApplCode = mRelateData.CurrentKM.ToString(),
                    QueryText = "",
                    FldType = "N"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F009",
                    Head = "Due km",
                    ApplCode = mRelateData.DueKM.ToString(),
                    QueryText = "",
                    FldType = "N"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F010",
                    Head = "Warranty Days",
                    ApplCode = mRelateData.WarrantyDays.ToString(),
                    QueryText = "",
                    FldType = "N",

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F011",
                    Head = "MFG Date",
                    ApplCode = (string.IsNullOrEmpty(mRelateData.MFGDate) == true) ? Convert.ToDateTime("1900-01-01").ToString("yyyy-MM-dd") : mRelateData.MFGDate.ToString(),
                    QueryText = "",
                    FldType = "D",
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F012",
                    Head = "Install / Received Date",
                    ApplCode = (string.IsNullOrEmpty(mRelateData.InstallDate) == true) ? Convert.ToDateTime("1900-01-01").ToString("yyyy-MM-dd") : mRelateData.InstallDate.ToString(),
                    QueryText = "",
                    FldType = "D"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F013",
                    Head = "Due Date",
                    ApplCode = (string.IsNullOrEmpty(mRelateData.DueDate) == true) ? Convert.ToDateTime("1900-01-01").ToString("yyyy-MM-dd") : mRelateData.DueDate.ToString(),
                    QueryText = "",
                    FldType = "D",

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F014",
                    Head = "HSN CODE",
                    ApplCode = mRelateData.HSNCode,
                    QueryText = "",
                    FldType = "X",
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F015",
                    Head = "Description",
                    ApplCode = mRelateData.Description,
                    QueryText = "",
                    FldType = "M",
                });
            }
            return truckaddonlist;
        }

        public List<OtherTransactModel> GetTyreStockSerialList(string TableKey, string Branch)
        {
            List<OtherTransactModel> mTyredetails = new List<OtherTransactModel>();
            var GetParentkey = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == TableKey && x.Branch == Branch).Select(x => x.ParentKey).FirstOrDefault();

            var mTyrestocks = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == GetParentkey).Select(x => x).OrderBy(x => x.RECORDKEY).FirstOrDefault();

            var Count = ctxTFAT.TyreStockSerial.Where(x => x.ParentKey == GetParentkey && x.TableKey != TableKey).Select(x => x).ToList().Count();

            var LatestmTyrestocks = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == GetParentkey && x.ParentKey == TableKey && x.Branch == Branch).Select(x => x).OrderByDescending(x => x.RECORDKEY).FirstOrDefault();

            if (mTyrestocks != null)
            {
                var a = mTyrestocks;
                mTyredetails.Add(new OtherTransactModel()
                {
                    Name = ctxTFAT.VehicleMaster.Where(x => x.PostAc == a.Vehicle).Select(x => x.TruckNo).FirstOrDefault(),
                    Branch = LatestmTyrestocks.Branch,
                    DocuDate = a.Value1,
                    ActWt = Convert.ToDouble(a.Value2),
                    FEndDate = a.Value3,
                    ChgWt = Convert.ToDouble(a.Value4),
                    RECORDKEY = a.RECORDKEY,
                    ApplCode = LatestmTyrestocks.Status,
                    Srl = a.SerialNo,
                    Code = a.Vehicle,
                    TableKey = LatestmTyrestocks.TableKey,
                    ParentKey = LatestmTyrestocks.ParentKey,
                    StockAt = LatestmTyrestocks.StockAt,
                    StepneeNo = a.StepneeNo,
                    TyreNo = a.TyreNo,
                    IsActive = a.IsActive,
                    AuthLock = LatestmTyrestocks.IsActive == false ? true : false,
                });


            }
            return mTyredetails;
        }

        [HttpPost]
        public ActionResult ConfirmRelationAddEdit(OtherTransactModel Model)
        {
            string mMessage = "";
            Master master = ctxTFAT.Master.Where(x => x.Code == Model.Code).FirstOrDefault();
            //Branch Validation
            if (Model.DuplExpDtConfirm == true && Model.RelatedTo == "000100345" || Model.Code == "000100345")
            {
                if (Model.AddOnList != null && Model.ItemList != null)
                {
                    var mtype = Model.ItemList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                    var mInstalldT = Model.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                    var mtyreserialno = Model.AddOnList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault();
                    DateTime mInstalldate = Convert.ToDateTime(mInstalldT);
                    var iMTableKey = ctxTFAT.RelateData.Where(x => x.Combo1 == Model.RelatedTo && x.Code == Model.Code && (x.Value2 == mtype && x.Value6 == mtyreserialno && mInstalldate <= x.Date3) && x.ParentKey != Model.ParentKey).Select(x => x.TableKey).FirstOrDefault();
                    var mdecnum2 = Model.ItemList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault();
                    var mNum2 = (string.IsNullOrEmpty(mdecnum2) == true) ? 0 : Convert.ToDecimal(mdecnum2);

                    var iMTableKey2 = ctxTFAT.RelateData.Where(x => x.Combo1 == Model.RelatedTo && x.Code == Model.Code && (x.Value2 == mtype && x.Value6 == mtyreserialno && mNum2 <= x.Num3) && x.ParentKey != Model.ParentKey).Select(x => x.TableKey).FirstOrDefault();

                    if (iMTableKey != null || iMTableKey2 != null)
                    {
                        Model.Status = "ConfirmError";
                        string mmessage = "";
                        if (iMTableKey != null)
                        {
                            mmessage = mmessage + "Entered Install date before Due date Confirm Do You want to continue";
                        }
                        if (iMTableKey2 != null)
                        {
                            mmessage = mmessage + " Entered Current KM date before Due KM Confirm Do You want to continue";
                        }
                        Model.Message = mmessage;


                        return Json(Model, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            else
            {
                Model.Status = "Success";
                Model.Message = "";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetItemPartDetail(OtherTransactModel Model, string KM, string Days)
        {
            var mSpare = ctxTFAT.ItemMaster.Where(x => x.Code == Model.Type).Select(x => x).FirstOrDefault();
            var mExpdate = DateTime.Now.Date;
            var mDate = Convert.ToDateTime(Model.FromDate);
            double McOST = 0;
            double mKm = 0;
            int mexpdays = 0;
            string Account = "", AccountName = "";

            if (mSpare != null)
            {
                mexpdays = mSpare.ExpiryDays;
                mKm = mSpare.ExpiryKm;
                McOST = mSpare.Rate;
            }
            if (!String.IsNullOrEmpty(Days) && Days != "0")
            {
                mexpdays = Convert.ToInt32(Days);
            }
            if (!String.IsNullOrEmpty(KM) && KM != "0")
            {
                mKm = Convert.ToInt32(KM);
            }
            if (mexpdays == 0)
            {
                mExpdate = mDate.AddDays(mexpdays);
            }
            else
            {
                mExpdate = mDate.AddDays(mexpdays);
            }
            if (mKm == 0)
            {
                mKm = Convert.ToInt32(KM == "" ? "0" : KM);
            }
            string GSTNAME = "", HSNCODE = "", Descr = "";
            decimal IGSTRATE = 0, CGSTRATE = 0, SGSTRATE = 0;
            if (mSpare != null)
            {
                HSNCODE = mSpare.HSNCode;
                Descr = mSpare.Narr;
            }
            string HSNCODENAme = "";
            if (!String.IsNullOrEmpty(HSNCODE))
            {
                HSNCODENAme = ctxTFAT.HSNMaster.Where(x => x.Code == HSNCODE).Select(x => x.Name).FirstOrDefault();
            }
            return Json(new
            {
                KM = mKm,
                Days = mexpdays,
                Cost = McOST,
                ExpDays = mexpdays == 0 ? "" : mExpdate.ToString("yyyy-MM-dd"),
                Account = Account,
                AccountName = AccountName,
                GSTCODE = mSpare == null ? "" : mSpare.GSTCode,
                GSTNAME = GSTNAME,
                IGSTRATE = IGSTRATE,
                CGSTRATE = CGSTRATE,
                SGSTRATE = SGSTRATE,
                HSNCODE = HSNCODE,
                HSNCODENAme = HSNCODENAme,
                Descr = Descr,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetItemSingleViewList(OtherTransactModel Model)
        {
            Model.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
            List<AddOns> truckaddonlist = new List<AddOns>();
            // Item Details
            var mItem = ctxTFAT.ItemMaster.Where(x => x.BaseGr == Model.ProductGroupType).Select(x => new { x.Name, x.Code }).Distinct().ToList();
            var mItemGroup = ctxTFAT.ItemGroups.Where(x => x.Hide == false).Select(x => new { x.Name, x.Code }).Distinct().ToList();
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F001",
                Head = "Product Group",
                ApplCode = Model.ProductGroupType,
                QueryText = "Select^" + String.Join("^", mItemGroup.Select(x => x.Name).ToList()),
                QueryCode = "Select^" + String.Join("^", mItemGroup.Select(x => x.Code).ToList()),
                FldType = "C",
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F002",
                Head = "Pick From",
                ApplCode = Model.FromType,
                QueryText = "Master^Direct",
                QueryCode = "Master^Direct",
                FldType = "C"
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F003",
                Head = "Item",
                ApplCode = "",
                QueryText = "Select^" + String.Join("^", mItem.Select(x => x.Name).ToList()),
                QueryCode = "Select^" + String.Join("^", mItem.Select(x => x.Code).ToList()),
                FldType = Model.FromType == "Direct" ? "T" : "C",
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F004",
                Head = "Cost",
                ApplCode = "0",
                QueryText = "",
                QueryCode = "",
                FldType = "N",
                InActive = true
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F005",
                Head = "Qty",
                ApplCode = "1",
                QueryText = "",
                QueryCode = "",
                FldType = "N",
                InActive = true
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F006",
                Head = "Total Amount",
                ApplCode = "0",
                QueryText = "",
                QueryCode = "",
                FldType = "N",
                InActive = true
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F007",
                Head = "Warranty KM",
                ApplCode = "0",
                QueryText = "",
                FldType = "N"
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F008",
                Head = "Current KM",
                ApplCode = "0",
                QueryText = "",
                FldType = "N",

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F009",
                Head = "Due km",
                ApplCode = "0",
                QueryText = "",
                FldType = "N"
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F010",
                Head = "Warranty Days",
                ApplCode = "0",
                QueryText = "",
                FldType = "N",

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F011",
                Head = "MFG Date",
                ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                QueryText = "",
                FldType = "D",
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F012",
                Head = "Install / Received Date",
                ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                QueryText = "",
                FldType = "D"
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F013",
                Head = "Due Date",
                ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                QueryText = "",
                FldType = "D",

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F014",
                Head = "HSN CODE",
                ApplCode = "",
                QueryText = "",
                FldType = "X",
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F015",
                Head = "Description",
                ApplCode = "",
                QueryText = "",
                FldType = "M",
            });
            var html = ViewHelper.RenderPartialView(this, "ItemDetails", new OtherTransactModel() { ItemList = truckaddonlist });
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetTyreStockDetail(OtherTransactModel Model)
        {
            var MaintainStock = false;
            var ItemMaster = ctxTFAT.ItemMaster.Where(x => x.Code == Model.Type).FirstOrDefault();
            if (ItemMaster != null)
            {
                MaintainStock = ItemMaster.StockMaintain;
            }
            List<TyreStockSerial> mTyrestocks = new List<TyreStockSerial>();
            List<OtherTransactModel> mTyredetails = new List<OtherTransactModel>();
            if (MaintainStock)
            {
                string OldStkTableKey = "";
                var GetStock = ctxTFAT.TyreStockSerial.Where(x => x.TableKey.Trim() == Model.TableKey).FirstOrDefault();
                if (GetStock != null)
                {
                    OldStkTableKey = GetStock.ParentKey;
                }

                if (Model.Fld == "Tyre")
                {
                    var mtyreno = (string.IsNullOrEmpty(Model.TyreNo) == false) ? Convert.ToInt32(Model.TyreNo) : 0;
                    var mtyrep = ctxTFAT.VehicleMaster.Where(x => x.Code == Model.VehicleNo).Select(x => x.NoOfTyres).FirstOrDefault();
                    if (((mtyreno > mtyrep)) && mtyreno != 0)
                    {
                        return Json(new { Status = "Valid", Message = "Tyre Place On No " + Model.TyreNo + " Not exist in Selected Vehicle" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (Model.Fld == "Stepnee")
                {
                    var mtyreno = (string.IsNullOrEmpty(Model.TyreNo) == false) ? Convert.ToInt32(Model.TyreNo) : 0;
                    var mtyrep = ctxTFAT.VehicleMaster.Where(x => x.Code == Model.VehicleNo).Select(x => x.Stepney).FirstOrDefault();
                    if (((mtyreno > mtyrep)) && mtyreno != 0)
                    {
                        return Json(new { Status = "Valid", Message = "Stepnee No " + Model.TyreNo + " Not exist in Selected Vehicle" }, JsonRequestBehavior.AllowGet);
                    }
                }
                var VehiclePOstAc = ctxTFAT.VehicleMaster.Where(x => x.Code == Model.VehicleNo).Select(x => x.PostAc).FirstOrDefault();
                if (String.IsNullOrEmpty(Model.TableKey))
                {
                    mTyrestocks = ctxTFAT.TyreStockSerial.Where(x => ((x.TyreNo == Model.TyreNo && Model.Fld == "Tyre") || (x.StepneeNo == Model.TyreNo && Model.Fld == "Stepnee")) && x.Vehicle == VehiclePOstAc && x.StockAt == "Vehicle" && x.IsActive == true).Select(x => x).ToList();
                }
                else
                {
                    mTyrestocks = ctxTFAT.TyreStockSerial.Where(x => ((x.TyreNo == Model.TyreNo && Model.Fld == "Tyre") || (x.StepneeNo == Model.TyreNo && Model.Fld == "Stepnee")) && x.Vehicle == VehiclePOstAc /*&& x.StockAt == "Vehicle"*/ && x.TableKey == OldStkTableKey).Select(x => x).ToList();
                }
                if (mTyrestocks != null && mTyrestocks.Count > 0)
                {
                    foreach (var a in mTyrestocks)
                    {
                        mTyredetails.Add(new OtherTransactModel()
                        {
                            Name = ctxTFAT.VehicleMaster.Where(x => x.PostAc == a.Vehicle).Select(x => x.TruckNo).FirstOrDefault(),
                            DocuDate = a.Value1,
                            ActWt = Convert.ToDouble(a.Value2),
                            FEndDate = a.Value3,
                            ChgWt = Convert.ToDouble(a.Value4),
                            RECORDKEY = a.RECORDKEY,
                            ApplCode = a.Status,
                            Srl = a.SerialNo,
                            Code = a.Vehicle,
                            TableKey = a.TableKey,
                            StepneeNo = a.StepneeNo,
                            TyreNo = a.TyreNo,
                            StockAt = a.StockAt,
                            IsActive = a.IsActive
                        });
                    }

                }
            }
            var html = ViewHelper.RenderPartialView(this, "TyreStockSerial", new OtherTransactModel() { TyreStockList = mTyredetails });
            return Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
        }

        public void DeUpdate(OtherTransactModel Model)
        {
            TyreStockTransfer tyreStockTransfer = ctxTFAT.TyreStockTransfer.Where(x => x.Tablekey == Model.Document).FirstOrDefault();

            var mobj12 = ctxTFAT.RelateData.Where(x => x.ParentKey == tyreStockTransfer.Parentkey && x.Branch == tyreStockTransfer.Branch).ToList();
            if (mobj12 != null)
            {
                ctxTFAT.RelateData.RemoveRange(mobj12);
            }

            var ItemStock = ctxTFAT.ItemStock.Where(x => x.Parentkey == tyreStockTransfer.Tablekey).FirstOrDefault();
            if (ItemStock != null)
            {
                ctxTFAT.ItemStock.Remove(ItemStock);
            }
            var ItemStockRel = ctxTFAT.RelateDataItem.Where(x => x.TableKey == tyreStockTransfer.Tablekey).FirstOrDefault();
            if (ItemStockRel != null)
            {
                ctxTFAT.RelateDataItem.Remove(ItemStockRel);
            }

            if (Model.Mode == "Delete")
            {
                ctxTFAT.TyreStockTransfer.Remove(tyreStockTransfer);

                var GetParentkey = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == tyreStockTransfer.Tablekey && x.Branch == tyreStockTransfer.Branch).Select(x => x.ParentKey).FirstOrDefault();
                TyreStockSerial TSS = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == GetParentkey && x.StockAt == "Vehicle").OrderBy(x => x.RECORDKEY).Select(x => x).FirstOrDefault();
                if (TSS != null)
                {
                    TSS.IsActive = true;
                    ctxTFAT.Entry(TSS).State = EntityState.Modified;
                }
                var mobj134 = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == tyreStockTransfer.Tablekey && x.Branch == tyreStockTransfer.Branch).FirstOrDefault();
                if (mobj134 != null)
                {
                    ctxTFAT.TyreStockSerial.Remove(mobj134);
                }
                mobj134 = ctxTFAT.TyreStockSerial.Where(x => x.ParentKey == tyreStockTransfer.Tablekey && x.Branch == tyreStockTransfer.Branch).FirstOrDefault();
                if (mobj134 != null)
                {
                    ctxTFAT.TyreStockSerial.Remove(mobj134);
                }

            }

            ctxTFAT.SaveChanges();

        }

        [HttpPost]
        public ActionResult SaveData(OtherTransactModel Model)
        {
            string mTable = "";
            string brMessage = "";
            Model.Type = "TYRTN";
            Model.Prefix = mperiod;
            Model.Branch = mbranchcode;
            string Parentkey = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction(System.Data.IsolationLevel.Unspecified))
            {
                try
                {
                    bool mAdd = true;
                    TyreStockTransfer tyreStock = new TyreStockTransfer();
                    if (ctxTFAT.TyreStockTransfer.Where(x => x.Tablekey == Model.Document).FirstOrDefault() != null)
                    {
                        tyreStock = ctxTFAT.TyreStockTransfer.Where(x => x.Tablekey == Model.Document).FirstOrDefault();
                        mAdd = false;
                        DeUpdate(Model);
                        Model.Srl = tyreStock.DocNo;
                        Model.ParentKey = tyreStock.Tablekey;
                        Parentkey = tyreStock.Parentkey;
                    }

                    if (mAdd)
                    {
                        tyreStock.DocNo = GenerateCode();
                        tyreStock.Tablekey = "TYRTN" + mperiod.Substring(0, 2) + "001" + tyreStock.DocNo;
                        tyreStock.Parentkey = "TYRTN" + mperiod.Substring(0, 2) + tyreStock.DocNo;
                        Model.Srl = tyreStock.DocNo;
                        Model.ParentKey = tyreStock.Tablekey;
                        tyreStock.Prefix = mperiod;
                        tyreStock.ENTEREDBY = muserid;
                        Parentkey = tyreStock.Parentkey;
                    }

                    tyreStock.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                    tyreStock.Branch = mbranchcode;
                    tyreStock.TransferToStock = "New";
                    tyreStock.VehicleCode = Model.RelatedChoice;
                    tyreStock.InstallFor = Model.AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault() == null ? "" : Model.AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault() == "Tyre" ? "T" : "S";
                    tyreStock.TyrePlaceNo = Model.AddOnList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                    tyreStock.TyreSerialNo = Model.AddOnList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault();
                    tyreStock.InstallDate = ConvertYYMMDDTODDMMYY(Model.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault());
                    tyreStock.InstallKM = Model.ItemList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault();


                    tyreStock.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    tyreStock.AUTHORISE = mauthorise;
                    tyreStock.AUTHIDS = muserid;

                    if (mAdd)
                    {
                        ctxTFAT.TyreStockTransfer.Add(tyreStock);
                    }
                    else
                    {
                        ctxTFAT.Entry(tyreStock).State = EntityState.Modified;
                    }






                    int xCnt = 1; // used for counts

                    List<string> TyreMasterAdd = new List<string>();
                    int NewCode1;
                    var NewCode = ctxTFAT.ItemMaster.OrderByDescending(x => x.Code).Select(x => x.Code).Take(1).FirstOrDefault();
                    if (NewCode == null || NewCode == "")
                    {
                        NewCode1 = 000001;
                    }
                    else
                    {
                        NewCode1 = Convert.ToInt32(NewCode) + 1;
                    }
                    RelateData reldt = new RelateData();
                    reldt.Amount = 0;
                    reldt.AUTHIDS = muserid;
                    reldt.AUTHORISE = mauthorise;
                    reldt.Branch = mbranchcode;
                    reldt.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                    reldt.ENTEREDBY = muserid;
                    reldt.Deleted = false;
                    reldt.Type = "TYRTN";
                    reldt.Srl = Convert.ToInt32(tyreStock.DocNo);
                    reldt.Sno = "001";
                    reldt.SubType = "TN";
                    reldt.LASTUPDATEDATE = DateTime.Now;
                    reldt.MainType = "TY";
                    reldt.Code = "";
                    reldt.Narr = "";

                    reldt.TableKey = tyreStock.Tablekey;
                    reldt.ParentKey = tyreStock.Parentkey;
                    reldt.RelateTo = (byte)(0);
                    reldt.ItemReq = false;
                    RelateDataItem relateDataItem = new RelateDataItem();
                    if (Model.ItemList != null)
                    {
                        if (Model.ItemList.Count() > 0)
                        {
                            reldt.Char1 = Model.ItemList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                            reldt.Value1 = Model.ItemList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault();
                            reldt.Value2 = Model.ItemList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                            var mdecnum1 = Model.ItemList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault();
                            reldt.Num1 = (string.IsNullOrEmpty(mdecnum1) == true) ? 0 : Convert.ToDecimal(mdecnum1);
                            var mdecnum4 = Model.ItemList.Where(x => x.Fld == "F007").Select(x => x.ApplCode).FirstOrDefault();
                            reldt.Num4 = (string.IsNullOrEmpty(mdecnum4) == true) ? 0 : Convert.ToDecimal(mdecnum4);
                            reldt.Value3 = Model.ItemList.Where(x => x.Fld == "F010").Select(x => x.ApplCode).FirstOrDefault();
                            var mDATE = Model.ItemList.Where(x => x.Fld == "F011").Select(x => x.ApplCode).FirstOrDefault();
                            if (!string.IsNullOrEmpty(mDATE))
                            {
                                if (mDATE.ToString().Trim() != "0001-01-01")
                                {
                                    reldt.Date1 = Convert.ToDateTime(mDATE);
                                }
                            }
                            var mDATE2 = Model.ItemList.Where(x => x.Fld == "F013").Select(x => x.ApplCode).FirstOrDefault();
                            if (String.IsNullOrEmpty(reldt.Value3))
                            {

                                reldt.Date2 = null;
                            }
                            else
                            {

                                if (!string.IsNullOrEmpty(mDATE2))
                                {
                                    if (mDATE2.ToString().Trim() != "0001-01-01" && mDATE2.ToString().Trim() != "1900-01-01")
                                    {
                                        reldt.Date2 = Convert.ToDateTime(mDATE2);
                                    }
                                    else
                                    {
                                        reldt.Date2 = null;
                                    }
                                }
                                else
                                {
                                    reldt.Date2 = null;
                                }

                            }
                            var mdecnum2 = Model.ItemList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault();
                            reldt.Num2 = (string.IsNullOrEmpty(mdecnum2) == true) ? 0 : Convert.ToDecimal(mdecnum2);
                            var mDATE3 = Model.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                            if (!string.IsNullOrEmpty(mDATE3))
                            {
                                if (mDATE3.ToString().Trim() != "0001-01-01")
                                {
                                    reldt.Date3 = Convert.ToDateTime(mDATE3);
                                }
                            }
                            var mdecnum3 = Model.ItemList.Where(x => x.Fld == "F009").Select(x => x.ApplCode).FirstOrDefault();
                            reldt.Num3 = (string.IsNullOrEmpty(mdecnum3) == true) ? 0 : Convert.ToDecimal(mdecnum3);

                            relateDataItem.ProductGroup = Model.ItemList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                            relateDataItem.Item = Model.ItemList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                            relateDataItem.Cost = Convert.ToDecimal(Model.ItemList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault());
                            relateDataItem.Qty = Model.ItemList.Where(x => x.Fld == "F005").Select(x => x.ApplCode).FirstOrDefault();
                            relateDataItem.TotalAmout = Convert.ToDecimal(Model.ItemList.Where(x => x.Fld == "F006").Select(x => x.ApplCode).FirstOrDefault());
                            relateDataItem.WarrantyKm = Model.ItemList.Where(x => x.Fld == "F007").Select(x => x.ApplCode).FirstOrDefault();
                            relateDataItem.CurrentKM = Model.ItemList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault();
                            relateDataItem.DueKM = Model.ItemList.Where(x => x.Fld == "F009").Select(x => x.ApplCode).FirstOrDefault();
                            relateDataItem.WarrantyDays = Model.ItemList.Where(x => x.Fld == "F010").Select(x => x.ApplCode).FirstOrDefault();
                            relateDataItem.MFGDate = Model.ItemList.Where(x => x.Fld == "F011").Select(x => x.ApplCode).FirstOrDefault();
                            relateDataItem.InstallDate = Model.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                            relateDataItem.DueDate = Model.ItemList.Where(x => x.Fld == "F013").Select(x => x.ApplCode).FirstOrDefault();
                            relateDataItem.HSNCode = Model.HSNCODE;
                            relateDataItem.Description = Model.ItemList.Where(x => x.Fld == "F015").Select(x => x.ApplCode).FirstOrDefault();
                            relateDataItem.TableKey = reldt.TableKey;
                            relateDataItem.Parentkey = reldt.ParentKey;
                            relateDataItem.AUTHIDS = muserid;
                            relateDataItem.AUTHORISE = mauthorise;
                            relateDataItem.ENTEREDBY = muserid;
                            relateDataItem.LASTUPDATEDATE = DateTime.Now;
                            ctxTFAT.RelateDataItem.Add(relateDataItem);
                            reldt.ItemReq = true;
                            if (Model.ItemList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault() == "Direct")
                            {
                                ItemMaster tyreMaster = ctxTFAT.ItemMaster.Where(x => x.Name == relateDataItem.Item).FirstOrDefault();
                                if (tyreMaster == null)
                                {
                                    if (!(TyreMasterAdd.Contains(relateDataItem.Item)))
                                    {
                                        tyreMaster = new ItemMaster();
                                        tyreMaster.Name = relateDataItem.Item;
                                        tyreMaster.BaseGr = relateDataItem.ProductGroup;
                                        tyreMaster.Rate = Convert.ToDouble(relateDataItem.Cost);
                                        tyreMaster.Posting = "";
                                        tyreMaster.GSTCode = reldt.GSTCode;
                                        tyreMaster.HSNCode = Model.HSNCODE;
                                        tyreMaster.StockMaintain = true;
                                        tyreMaster.ExpiryDays = Convert.ToInt32(relateDataItem.WarrantyDays);
                                        tyreMaster.ExpiryKm = Convert.ToInt32(relateDataItem.WarrantyKm);
                                        tyreMaster.Active = true;
                                        tyreMaster.Narr = "";
                                        tyreMaster.AppBranch = mbranchcode;

                                        tyreMaster.AUTHIDS = muserid;
                                        tyreMaster.AUTHORISE = "A00";
                                        tyreMaster.ENTEREDBY = muserid;
                                        tyreMaster.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                                        string FinalCode = NewCode1.ToString("D6");
                                        tyreMaster.Code = FinalCode;
                                        reldt.Value2 = FinalCode;
                                        relateDataItem.Item = FinalCode;
                                        ctxTFAT.ItemMaster.Add(tyreMaster);
                                        TyreMasterAdd.Add(reldt.Value2);
                                        ++NewCode1;
                                    }
                                }
                            }
                        }
                    }
                    if (Model.AddOnList != null)
                    {
                        if (Model.AddOnList.Count() > 0)
                        {
                            //Specially For Tyres Expenses Save ( Value1,Value2 Num1,Num4,Value3,Value4,Value5,Value6,Date1,Date2,Num2,Date3,Num3  )
                            reldt.Value4 = Model.AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault();
                            reldt.Value5 = Model.AddOnList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                            reldt.Value6 = Model.AddOnList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault();
                            reldt.Value7 = Model.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();

                        }
                    }
                    reldt.Value8 = Model.RelatedChoice;//Vehicle NO
                    reldt.Combo1 = null;//OTher Cost Account
                    reldt.AmtType = true;
                    reldt.ReqRelated = true;
                    reldt.Status = false;
                    reldt.Clear = false;
                    reldt.GSTFlag = false;
                    ctxTFAT.RelateData.Add(reldt);
                    int xrellrCnt = 1;

                    if (Model.ItemList != null)
                    {
                        if (Model.ItemList.Count() > 0)
                        {
                            bool MaintainStock = false;
                            var PickFrom = Model.ItemList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault();
                            if (PickFrom == "Direct")
                            {
                                MaintainStock = true;
                            }
                            else if (PickFrom == "Master")
                            {
                                var itemmaster = ctxTFAT.ItemMaster.Where(x => x.Code == relateDataItem.Item.Trim()).FirstOrDefault();
                                if (itemmaster != null)
                                {
                                    MaintainStock = itemmaster.StockMaintain;
                                }
                            }
                            if (MaintainStock)
                            {

                                if (Model.Mode == "Add")
                                {
                                    int mTy = 1;
                                    string PreviousTablekeyOfStk = Model.Type + Model.Prefix.Substring(0, 2) + Model.Srl.ToString();
                                    OtherTransactModel TyreStock = new OtherTransactModel();
                                    if (Model.TyreStockList != null && Model.TyreStockList.Count > 0)
                                    {
                                        TyreStock = Model.TyreStockList.FirstOrDefault();
                                        TyreStockSerial TSS = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == TyreStock.TableKey).OrderBy(x => x.RECORDKEY).Select(x => x).FirstOrDefault();
                                        TSS.IsActive = false;
                                        ctxTFAT.Entry(TSS).State = EntityState.Modified;
                                        PreviousTablekeyOfStk = TyreStock.TableKey;
                                    }
                                    if (Model.AddOnList != null)
                                    {
                                        if (Model.AddOnList.Count() > 0 && Model.ItemList.Count() > 0)
                                        {
                                            #region New Stock Entry
                                            var mInstDATE = Model.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                                            TyreStockSerial TSS2 = new TyreStockSerial();
                                            TSS2.Branch = mbranchcode;
                                            TSS2.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                                            TSS2.SerialNo = Model.AddOnList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault() ?? "";
                                            TSS2.Status = Model.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                                            TSS2.Value1 = Model.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                                            TSS2.Value2 = Model.ItemList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault() == null ? 0 : Convert.ToDecimal(Model.ItemList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault());
                                            TSS2.Value3 = Model.ItemList.Where(x => x.Fld == "F013").Select(x => x.ApplCode).FirstOrDefault();
                                            TSS2.Value4 = Model.ItemList.Where(x => x.Fld == "F009").Select(x => x.ApplCode).FirstOrDefault() == null ? 0 : Convert.ToDecimal(Model.ItemList.Where(x => x.Fld == "F009").Select(x => x.ApplCode).FirstOrDefault());
                                            TSS2.Vehicle = Model.RelatedChoice;
                                            if (Model.AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault() == "Tyre")
                                            {
                                                TSS2.TyreNo = Model.AddOnList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                                            }
                                            else
                                            {
                                                TSS2.StepneeNo = Model.AddOnList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                                            }
                                            TSS2.StockAt = (Model.RelatedChoice == "Tyrestock" || Model.RelatedChoice == "Remould") ? "Stock" : "Vehicle";
                                            TSS2.TableKey = Model.Type + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString();
                                            TSS2.ParentKey = PreviousTablekeyOfStk;
                                            TSS2.Sno = mTy;
                                            TSS2.ENTEREDBY = muserid;
                                            TSS2.AUTHIDS = muserid;
                                            TSS2.AUTHORISE = "A00";
                                            TSS2.LASTUPDATEDATE = DateTime.Now;
                                            TSS2.IsActive = true;
                                            ctxTFAT.TyreStockSerial.Add(TSS2);
                                            #endregion

                                            #region Update New Stock
                                            if (Model.TyreStockList != null && Model.TyreStockList.Count > 0)
                                            {
                                                TyreStockSerial OTSS = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == TyreStock.TableKey).OrderBy(x => x.RECORDKEY).Select(x => x).FirstOrDefault();
                                                if (TyreStock != null)
                                                {
                                                    TyreStockSerial TSS3 = new TyreStockSerial();
                                                    TSS3.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                                                    TSS3.Branch = TSS2.Branch;
                                                    TSS3.SerialNo = OTSS.SerialNo;
                                                    TSS3.Status = TyreStock.ApplCode;
                                                    TSS3.Value1 = OTSS.Value1;
                                                    TSS3.Value2 = OTSS.Value2;
                                                    TSS3.Value3 = OTSS.Value3;
                                                    TSS3.Value4 = OTSS.Value4;
                                                    TSS3.Vehicle = TSS2.Vehicle;
                                                    TSS3.TyreNo = "";
                                                    TSS3.StepneeNo = "";
                                                    TSS3.StockAt = "Stock";
                                                    TSS3.TableKey = TyreStock.TableKey;
                                                    TSS3.ParentKey = TSS2.TableKey;
                                                    TSS3.Sno = TSS2.Sno;
                                                    TSS3.ENTEREDBY = muserid;
                                                    TSS3.AUTHIDS = muserid;
                                                    TSS3.AUTHORISE = "A00";
                                                    TSS3.LASTUPDATEDATE = DateTime.Now;
                                                    TSS3.IsActive = true;
                                                    ctxTFAT.TyreStockSerial.Add(TSS3);
                                                }
                                            }
                                            #endregion
                                        }
                                    }
                                }
                                else
                                {
                                    int mTy = 1;
                                    string PreviousTablekeyOfStk = Model.Type + Model.Prefix.Substring(0, 2) + Model.Srl.ToString();
                                    OtherTransactModel TyreStock = new OtherTransactModel();
                                    if (Model.TyreStockList != null && Model.TyreStockList.Count > 0)
                                    {
                                        TyreStock = Model.TyreStockList.FirstOrDefault();
                                        TyreStockSerial TSS = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == TyreStock.TableKey).OrderBy(x => x.RECORDKEY).Select(x => x).FirstOrDefault();
                                        TSS.IsActive = false;
                                        ctxTFAT.Entry(TSS).State = EntityState.Modified;
                                        PreviousTablekeyOfStk = TyreStock.TableKey;
                                    }
                                    var tablekey = Model.Type + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString();
                                    var mInstDATE = Model.ItemList == null ? "" : Model.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                                    TyreStockSerial TSS2 = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == tablekey && x.Branch == Model.Branch).FirstOrDefault();
                                    if (TSS2 != null && Model.AddOnList.Count() > 0 && Model.ItemList.Count() > 0)
                                    {
                                        TSS2.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                                        TSS2.SerialNo = Model.AddOnList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault();
                                        TSS2.Value1 = Model.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                                        TSS2.Value2 = Model.ItemList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault() == null ? 0 : Convert.ToDecimal(Model.ItemList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault());
                                        TSS2.Value3 = Model.ItemList.Where(x => x.Fld == "F013").Select(x => x.ApplCode).FirstOrDefault();
                                        TSS2.Value4 = Model.ItemList.Where(x => x.Fld == "F009").Select(x => x.ApplCode).FirstOrDefault() == null ? 0 : Convert.ToDecimal(Model.ItemList.Where(x => x.Fld == "F009").Select(x => x.ApplCode).FirstOrDefault());
                                        TSS2.Status = Model.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                                        TSS2.Vehicle = Model.RelatedChoice;
                                        TSS2.ParentKey = PreviousTablekeyOfStk;
                                        TSS2.Sno = mTy;
                                        TSS2.ENTEREDBY = muserid;
                                        TSS2.AUTHIDS = muserid;
                                        TSS2.AUTHORISE = "A00";
                                        TSS2.LASTUPDATEDATE = DateTime.Now;
                                        ctxTFAT.Entry(TSS2).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        if (Model.AddOnList != null && Model.ItemList != null)
                                        {
                                            if (Model.AddOnList.Count() > 0 && Model.ItemList.Count() > 0)
                                            {

                                                #region New Stock Entry
                                                mInstDATE = Model.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                                                TSS2 = new TyreStockSerial();
                                                TSS2.Branch = mbranchcode;
                                                TSS2.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                                                TSS2.SerialNo = Model.AddOnList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault() ?? "";
                                                TSS2.Status = Model.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                                                TSS2.Value1 = Model.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                                                TSS2.Value2 = Model.ItemList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault() == null ? 0 : Convert.ToDecimal(Model.ItemList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault());
                                                TSS2.Value3 = Model.ItemList.Where(x => x.Fld == "F013").Select(x => x.ApplCode).FirstOrDefault();
                                                TSS2.Value4 = Model.ItemList.Where(x => x.Fld == "F009").Select(x => x.ApplCode).FirstOrDefault() == null ? 0 : Convert.ToDecimal(Model.ItemList.Where(x => x.Fld == "F009").Select(x => x.ApplCode).FirstOrDefault());
                                                TSS2.Vehicle = Model.RelatedChoice;
                                                if (Model.AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault() == "Tyre")
                                                {
                                                    TSS2.TyreNo = Model.AddOnList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                                                }
                                                else
                                                {
                                                    TSS2.StepneeNo = Model.AddOnList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                                                }
                                                TSS2.StockAt = (Model.RelatedChoice == "Tyrestock" || Model.RelatedChoice == "Remould") ? "Stock" : "Vehicle";
                                                TSS2.TableKey = Model.Type + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString();
                                                TSS2.ParentKey = PreviousTablekeyOfStk;
                                                TSS2.Sno = mTy;
                                                TSS2.ENTEREDBY = muserid;
                                                TSS2.AUTHIDS = muserid;
                                                TSS2.AUTHORISE = "A00";
                                                TSS2.LASTUPDATEDATE = DateTime.Now;
                                                TSS2.IsActive = true;
                                                ctxTFAT.TyreStockSerial.Add(TSS2);
                                                #endregion

                                                #region Update New Stock
                                                if (Model.TyreStockList != null && Model.TyreStockList.Count > 0)
                                                {
                                                    if (TyreStock != null)
                                                    {
                                                        TyreStockSerial TSS3 = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == TyreStock.TableKey && x.ParentKey == TyreStock.ParentKey && x.Branch == TyreStock.Branch).Select(x => x).FirstOrDefault();
                                                        TSS3.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                                                        //TSS3.Branch = TSS2.Branch;
                                                        //TSS3.SerialNo = TSS2.SerialNo;
                                                        TSS3.Status = TyreStock.ApplCode;
                                                        //TSS3.Value1 = TSS2.Value1;
                                                        //TSS3.Value2 = TSS2.Value2;
                                                        //TSS3.Value3 = TSS2.Value3;
                                                        //TSS3.Value4 = TSS2.Value4;
                                                        TSS3.Vehicle = TSS2.Vehicle;
                                                        //TSS3.TyreNo = TSS2.TyreNo;
                                                        //TSS3.StepneeNo = TSS2.StepneeNo;
                                                        TSS3.Sno = TSS2.Sno;
                                                        TSS3.ENTEREDBY = muserid;
                                                        TSS3.AUTHIDS = muserid;
                                                        TSS3.AUTHORISE = "A00";
                                                        TSS3.LASTUPDATEDATE = DateTime.Now;
                                                        TSS3.IsActive = true;
                                                        ctxTFAT.Entry(TSS3).State = EntityState.Modified;
                                                    }
                                                }
                                                #endregion
                                            }
                                        }
                                    }
                                }

                                //Item Stock Maintain  
                                if (Model.ItemList != null && Model.ItemList.Count() > 0)
                                {
                                    ItemStock itemStock = new ItemStock();
                                    itemStock.ProductGroup = relateDataItem.ProductGroup;
                                    itemStock.Name = relateDataItem.Item;
                                    itemStock.Rate = Convert.ToDouble(relateDataItem.Cost);
                                    itemStock.HSNCode = Model.HSNCODE;
                                    itemStock.Qty = Convert.ToInt32(relateDataItem.Qty);
                                    itemStock.Parentkey = reldt.TableKey;
                                    itemStock.Tablekey = "ITEM0" + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString(); ;
                                    itemStock.AUTHIDS = muserid;
                                    itemStock.AUTHORISE = "A00";
                                    itemStock.ENTEREDBY = muserid;
                                    itemStock.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                                    ctxTFAT.ItemStock.Add(itemStock);
                                }
                            }
                        }
                    }
                    ++xCnt;

                    //SaveNarration(Model, Model.ParentKey);
                    Model.Authorise = mauthorise;
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();

                    UpdateAuditTrail(Model.Branch, Model.Mode, Model.Header, Parentkey, DateTime.Now, Model.Amt, "", "Save New Tyre Stock", "NA");

                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    return Json(new
                    {
                        Message = ex1.EntityValidationErrors.Select(x => x.ValidationErrors.Select(xa => xa.ErrorMessage).FirstOrDefault()),
                        Status = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    return Json(new
                    {
                        Message = ex.InnerException.InnerException.Message,
                        Status = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new
            {
                NewSrl = (Model.Branch + Model.ParentKey),
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteData(OtherTransactModel Model)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction(System.Data.IsolationLevel.Unspecified))
            {
                try
                {
                    Model.Branch = mbranchcode;
                    //Check Current TyreStock Available Or Transfer It Or NOt.
                    var CurrentStock = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == Model.Document && x.Branch == Model.Branch).FirstOrDefault();
                    if (CurrentStock != null)
                    {
                        if (!CurrentStock.IsActive)
                        {
                            return Json(new
                            {
                                Status = "Error",
                                Message = "This Document Not Allow To Delete Because OF This Tyre Stock Will Transfer It SomeWhere....!"
                            }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            //Previous Tyre Stock Status
                            var PrevioustyreStock = ctxTFAT.TyreStockSerial.Where(x => x.ParentKey == Model.Document && x.Branch == Model.Branch && x.TableKey == CurrentStock.ParentKey).FirstOrDefault();
                            if (PrevioustyreStock != null)
                            {
                                if (!PrevioustyreStock.IsActive)
                                {
                                    return Json(new
                                    {
                                        Status = "Error",
                                        Message = "This Document Not Allow To Delete Because OF Old Tyre Stock Will Transfer It SomeWhere....!"
                                    }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                    }
                    var mlist = ctxTFAT.TyreStockTransfer.Where(x => x.Tablekey == Model.Document).FirstOrDefault();
                    //ctxTFAT.TyreStockTransfer.Remove(mlist);

                    DeUpdate(Model);
                    UpdateAuditTrail(mbranchcode, "Delete", Model.Header, mlist.Parentkey, DateTime.Now, 0, "", "Delete New Tyre Stock", "NA");

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

    }
}