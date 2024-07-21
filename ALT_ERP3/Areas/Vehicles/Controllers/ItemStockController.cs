using ALT_ERP3.Areas.Accounts.Models;
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
    public class ItemStockController : BaseController
    {
        private static string mauthorise = "A00";
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

        #region Functions

        public string GenerateCode()
        {
            var LastCode = ctxTFAT.ExtraItemStock.Where(x => x.Type == "ITEMX").OrderByDescending(x => x.Srl).Select(x => x.Srl).FirstOrDefault();
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

        // GET: Vehicles/ItemStock
        public ActionResult Index(OtherTransactModel mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.ExtraItemStock.Where(x => x.Tablekey == mModel.Document).FirstOrDefault();
                if (mList != null)
                {
                    var item = ctxTFAT.RelateData.Where(x => x.TableKey == mList.Tablekey).FirstOrDefault();
                    mModel.ItemList = GetItemWiseData(item.TableKey);
                    mModel.TableKey = item.TableKey;
                }
            }
            else
            {
                List<AddOns> truckaddonlist = new List<AddOns>();
                //mModel.ProductGroupType = "000009";
                var mItem = ctxTFAT.ItemMaster.Where(x => x.BaseGr == mModel.ProductGroupType).Select(x => new { x.Name, x.Code }).Distinct().ToList();
                var mItemGroup = ctxTFAT.ItemGroups.Where(x => x.Hide == false && x.Code != "000009").Select(x => new { x.Name, x.Code }).Distinct().ToList();
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
            }
            return View(mModel);
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

        public void DeUpdate(OtherTransactModel Model)
        {
            ExtraItemStock tyreStockTransfer = ctxTFAT.ExtraItemStock.Where(x => x.Tablekey == Model.Document).FirstOrDefault();

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
                ctxTFAT.ExtraItemStock.Remove(tyreStockTransfer);
            }
            ctxTFAT.SaveChanges();
        }

        [HttpPost]
        public ActionResult SaveData(OtherTransactModel Model)
        {
            string mTable = "";
            string brMessage = "", Parentkey = "";
            Model.Type = "ITEMX";
            Model.Prefix = mperiod;
            Model.Branch = mbranchcode;
            using (var transaction = ctxTFAT.Database.BeginTransaction(System.Data.IsolationLevel.Unspecified))
            {
                try
                {
                    bool mAdd = true;
                    ExtraItemStock tyreStock = new ExtraItemStock();
                    if (ctxTFAT.ExtraItemStock.Where(x => x.Tablekey == Model.Document).FirstOrDefault() != null)
                    {
                        tyreStock = ctxTFAT.ExtraItemStock.Where(x => x.Tablekey == Model.Document).FirstOrDefault();
                        mAdd = false;
                        DeUpdate(Model);
                        Model.Srl = tyreStock.Srl;
                        Model.ParentKey = tyreStock.Tablekey;
                        Parentkey = tyreStock.Parentkey;

                    }

                    if (mAdd)
                    {
                        tyreStock.Srl = GenerateCode();
                        tyreStock.Tablekey = "ITEMX" + mperiod.Substring(0, 2) + "001" + tyreStock.Srl;
                        tyreStock.Parentkey = "ITEMX" + mperiod.Substring(0, 2) + tyreStock.Srl;
                        Model.Srl = tyreStock.Srl;
                        tyreStock.Type = "ITEMX";
                        Model.ParentKey = tyreStock.Tablekey;
                        tyreStock.Prefix = mperiod;
                        tyreStock.ENTEREDBY = muserid;
                        Parentkey = tyreStock.Parentkey;

                    }

                    tyreStock.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                    tyreStock.Branch = mbranchcode;
                    tyreStock.ProductGroup = Model.ItemList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                    tyreStock.Item = Model.ItemList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();


                    tyreStock.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    tyreStock.AUTHORISE = mauthorise;
                    tyreStock.AUTHIDS = muserid;

                    if (mAdd)
                    {
                        ctxTFAT.ExtraItemStock.Add(tyreStock);
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
                    reldt.Type = "ITEMX";
                    reldt.Srl = Convert.ToInt32(tyreStock.Srl);
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
                            if (String.IsNullOrEmpty(mDATE2))
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
                                        tyreStock.Item = FinalCode;
                                        ctxTFAT.ItemMaster.Add(tyreMaster);
                                        TyreMasterAdd.Add(reldt.Value2);
                                        ++NewCode1;
                                    }
                                }
                            }
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

                    UpdateAuditTrail(Model.Branch, Model.Mode, Model.Header, Parentkey, ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString()), Model.Amt, "", "Save New Item Stock", "NA");

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
                    if (Model.Document == null || Model.Document == "")
                    {
                        return Json(new
                        {
                            Message = "Code not Entered..",
                            Status = "Error"
                        }, JsonRequestBehavior.AllowGet);
                    }
                    var ml = ctxTFAT.ItemStock.Where(x => x.Parentkey == Model.Document).FirstOrDefault();
                    if (ml != null)
                    {
                        var List = ctxTFAT.UseItemStockDetail.Where(x => x.Parentkey == ml.Tablekey).FirstOrDefault();
                        if (List != null)
                        {
                            return Json(new
                            {
                                Message = "\nThis Item Stock Transfer IN Document No : " + List.Srl,
                                Status = "Error"
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }


                    Model.Branch = mbranchcode;


                    var mlist = ctxTFAT.ExtraItemStock.Where(x => x.Tablekey == Model.Document).FirstOrDefault();


                    DeUpdate(Model);
                    UpdateAuditTrail(mbranchcode, "Delete", Model.Header, mlist.Parentkey, mlist.DocDate, 0, "", "Delete New Item Stock", "NA");

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