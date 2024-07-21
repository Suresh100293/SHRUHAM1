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
    public class ItemStockTransferController : BaseController
    {
        private static string mauthorise = "A00";
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
            var result = ctxTFAT.TfatMenu.Where(z => z.OptionCode == moptioncode && z.ModuleName == mmodule).Select(x => new { x.AllowEdit, x.AllowDelete }).FirstOrDefault();
            string mopt = "EDVX";
            if (result != null)
            {
                mopt = (result.AllowEdit == false ? "X" : "E") + (result.AllowDelete == false ? "X" : "D") + "XX";
            }
            return GetGridDataColumns(id, "L", "XXXX");
        }

        public ActionResult GetMasterGridData(GridOption Model)
        {
            if (Model.IsFormatSelected == true)
            {
                int noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=0 Where SubCodeOf='" + msubcodeof + "'");
                noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=-1 Where Code='" + Model.ViewDataId + "'");
            }

            Model.Code = "1=1";
            var result = (List<SparePartTransferVM>)Session["ItemStockGeneralDetails"];
            if (result == null)
            {
                result = new List<SparePartTransferVM>();
            }
            if (result.Count() > 0)
            {
                Model.Code = "I.Tablekey not in (";
                foreach (var item in result)
                {
                    Model.Code += "'" + item.Tablekey + "',";
                }
                Model.Code = Model.Code.Substring(0, Model.Code.Length - 1);
                Model.Code += ")";
            }

            return GetGridReport(Model, "M", "MainType^" + Model.MainType + "~Code^" + Model.Code, false, 0);
        }

        #region Functions

        public string GenerateCode()
        {
            var LastCode = ctxTFAT.ExtraItemStock.Where(x => x.Type == "ITEMG").OrderByDescending(x => x.Srl).Select(x => x.Srl).FirstOrDefault();
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

        public ActionResult GetAcType(string term)
        {
            List<SelectListItem> maclist = new List<SelectListItem>();
            var result = ctxTFAT.Master.Where(x => x.Hide == false ).Select(x => new { x.Code, x.Name }).ToList();
            foreach (var item in result)
            {
                maclist.Add(new SelectListItem { Value = item.Code, Text = item.Name });
            }
            return Json(maclist, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetVehicle(string term)
        {
            var list = ctxTFAT.Master.Where(x => x.Hide == false).Select(x => new { x.Name, x.Code }).ToList();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.Name.ToLower().Trim().Contains(term.ToLower().Trim())).ToList();
            }
            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name 
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        #endregion

        // GET: Vehicles/ItemStockTransfer
        public ActionResult Index(SparePartTransferVM mModel)
        {
            mModel.FromDate = StartDate;
            mModel.ToDate = EndDate;
            Session["ItemStockGeneralDetails"] = null;
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.ExtraItemStock.Where(x => x.Tablekey == mModel.Document).FirstOrDefault();
                if (mList != null)
                {
                    mModel.DocDate = mList.DocDate.ToShortDateString();
                    var ItemList = ctxTFAT.UseItemStockDetail.Where(x => x.Srl == mList.Srl && x.Type==mList.Type).OrderBy(x => x.Sno).ToList();
                    int I = 1;
                    List<SparePartTransferVM> List = new List<SparePartTransferVM>();
                    foreach (var item in ItemList)
                    {
                        SparePartTransferVM sparePart = new SparePartTransferVM();
                        sparePart.VehicleNo = item.VehicleNo;
                        sparePart.VehicleNoN = ctxTFAT.Master.Where(x => x.Code == item.VehicleNo).Select(x => x.Name).FirstOrDefault();
                        ItemStock itemMaster = ctxTFAT.ItemStock.Where(x => x.Tablekey == item.Parentkey).FirstOrDefault();
                        sparePart.ProductCode = itemMaster.ProductGroup;
                        sparePart.ProductName = ctxTFAT.ItemGroups.Where(x => x.Code == itemMaster.ProductGroup).Select(x => x.Name).FirstOrDefault();
                        sparePart.ItemCode = itemMaster.Name;
                        sparePart.ItemName = ctxTFAT.ItemMaster.Where(x => x.Code == itemMaster.Name).Select(x => x.Name).FirstOrDefault();
                        sparePart.TotalQty = itemMaster.Qty;
                        sparePart.BalQty = itemMaster.Qty - (ctxTFAT.UseItemStockDetail.Where(x => x.Parentkey == itemMaster.Tablekey).Sum(x => (int?)x.Qty) ?? 0);
                        sparePart.Qty = item.Qty;
                        sparePart.BalQty += item.Qty;
                        sparePart.Tablekey = itemMaster.Tablekey;
                        sparePart.Srno = I++;
                        List.Add(sparePart);
                    }
                    mModel.ItemList = List;
                    Session["ItemStockGeneralDetails"] = mModel.ItemList;
                }
            }

            else
            {
                mModel.DocDate = DateTime.Now.ToShortDateString();
            }

            return View(mModel);
        }

        #region Add LEDGER ITEM

        public ActionResult GetBreakLedger(SparePartTransferVM Model)
        {
            var result = (List<SparePartTransferVM>)Session["ItemStockGeneralDetails"];
            var result1 = result.Where(x => x.Srno == Model.Srno);
            foreach (var item in result1)
            {
                Model.ProductCode = item.ProductCode;
                Model.ProductName = ctxTFAT.ItemGroups.Where(x => x.Code == item.ProductCode).Select(x => x.Name.ToString().ToUpper()).FirstOrDefault();
                Model.ItemCode = item.ItemCode;
                Model.ItemName = ctxTFAT.ItemMaster.Where(x => x.Code == item.ItemCode).Select(x => x.Name.ToString().ToUpper()).FirstOrDefault();
                Model.VehicleNo = item.VehicleNo;
                Model.VehicleNoN = ctxTFAT.Master.Where(x => x.Code == item.VehicleNo).Select(x => x.Name).FirstOrDefault();
                Model.TotalQty = item.TotalQty;
                Model.BalQty = item.BalQty;
                Model.Qty = item.Qty;

            }
            var jsonResult = Json(new { Html = this.RenderPartialView("EditSingleItemStock", Model) }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;


        }

        [HttpPost]
        public ActionResult AddEditSelectedLedger(SparePartTransferVM Model)
        {
            if (Model.Mode == "Add")
            {
                List<SparePartTransferVM> objledgerdetail = new List<SparePartTransferVM>();
                if (Session["ItemStockGeneralDetails"] != null)
                {
                    objledgerdetail = (List<SparePartTransferVM>)Session["ItemStockGeneralDetails"];
                }
                List<SparePartTransferVM> NewList = new List<SparePartTransferVM>();
                NewList = Model.ItemList;
                int I = objledgerdetail.Count() + 1;
                foreach (var item in NewList)
                {
                    item.VehicleNoN = ctxTFAT.Master.Where(x => x.Code == item.VehicleNo).Select(x => x.Name).FirstOrDefault();
                    item.Srno = I++;
                }
                objledgerdetail.AddRange(NewList);
                Session.Add("ItemStockGeneralDetails", objledgerdetail);
                Model.ItemList = objledgerdetail;
                var html = ViewHelper.RenderPartialView(this, "GridDetails", Model);
                return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = (List<SparePartTransferVM>)Session["ItemStockGeneralDetails"];
                var Item = result.Where(x => x.Srno == Model.Srno).FirstOrDefault();
                Item.Qty = Model.Qty;
                Item.VehicleNo = Model.VehicleNo;
                Item.VehicleNoN = ctxTFAT.Master.Where(x => x.Code == Model.VehicleNo).Select(x => x.Name).FirstOrDefault();

                Model.ItemList = result;
                var html = ViewHelper.RenderPartialView(this, "GridDetails", Model);
                return Json(new
                {
                    Html = html
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteLedger(SparePartTransferVM Model)
        {
            var result2 = (List<SparePartTransferVM>)Session["ItemStockGeneralDetails"];
            var result = result2.Where(x => x.Srno != Model.Srno).ToList();
            int I = 1;
            foreach (var item in result)
            {
                item.Srno = I++;
            }
            Session.Add("ItemStockGeneralDetails", result);
            Model.ItemList = result;
            var html = ViewHelper.RenderPartialView(this, "GridDetails", Model);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public ActionResult GetPickUpStock(SparePartTransferVM Model)
        {
            var jsonResult = Json(new { Html = this.RenderPartialView("PendingStocks", Model) }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public void DeUpdate(SparePartTransferVM Model)
        {
            ExtraItemStock tyreStockTransfer = ctxTFAT.ExtraItemStock.Where(x => x.Tablekey == Model.Document).FirstOrDefault();

            var mobj12 = ctxTFAT.UseItemStockDetail.Where(x => x.Srl == tyreStockTransfer.Srl && x.Type== tyreStockTransfer.Type).ToList();
            if (mobj12 != null)
            {
                ctxTFAT.UseItemStockDetail.RemoveRange(mobj12);
            }

            if (Model.Mode == "Delete")
            {
                ctxTFAT.ExtraItemStock.Remove(tyreStockTransfer);
            }
            ctxTFAT.SaveChanges();
        }

        [HttpPost]
        public ActionResult SaveData(SparePartTransferVM Model)
        {
            string mTable = "";
            string brMessage = "";
            string Parentkey = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction(System.Data.IsolationLevel.Unspecified))
            {
                try
                {
                    var result2 = (List<SparePartTransferVM>)Session["ItemStockGeneralDetails"];
                    if (result2 == null)
                    {
                        return Json(new
                        {
                            Message = "Item Stock Not Found In List...!",
                            Status = "Error"
                        }, JsonRequestBehavior.AllowGet);

                    }
                    bool mAdd = true;
                    ExtraItemStock tyreStock = new ExtraItemStock();
                    if (ctxTFAT.ExtraItemStock.Where(x => x.Tablekey == Model.Document).FirstOrDefault() != null)
                    {
                        tyreStock = ctxTFAT.ExtraItemStock.Where(x => x.Tablekey == Model.Document).FirstOrDefault();
                        mAdd = false;
                        DeUpdate(Model);
                        Model.ParentKey = tyreStock.Tablekey;
                        Parentkey = tyreStock.Parentkey;
                    }

                    if (mAdd)
                    {
                        tyreStock.Srl = GenerateCode();
                        tyreStock.Tablekey = "ITEMG" + mperiod.Substring(0, 2) + "001" + tyreStock.Srl;
                        tyreStock.Parentkey = "ITEMG" + mperiod.Substring(0, 2) + tyreStock.Srl;
                        tyreStock.Type = "ITEMG";
                        Model.ParentKey = tyreStock.Tablekey;
                        tyreStock.Prefix = mperiod;
                        tyreStock.ENTEREDBY = muserid;
                        tyreStock.ProductGroup = "000";
                        tyreStock.Item = "000";
                        Parentkey = tyreStock.Parentkey;
                    }

                    tyreStock.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocDate);
                    tyreStock.Branch = mbranchcode;
                    tyreStock.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    tyreStock.AUTHORISE = mauthorise;
                    tyreStock.AUTHIDS = muserid;


                    int Xcnt = 1;
                    foreach (var item in result2)
                    {
                        UseItemStockDetail useItem = new UseItemStockDetail();
                        useItem.Branch = mbranchcode;
                        useItem.CreateDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                        useItem.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocDate);
                        useItem.Qty = item.Qty;
                        useItem.VehicleNo = item.VehicleNo;
                        useItem.Parentkey = item.Tablekey;
                        useItem.Tablekey = "ITEMG" + mperiod.Substring(0, 2) + Xcnt.ToString("D3") + tyreStock.Srl;
                        useItem.Type = "ITEMG";
                        useItem.Srl = tyreStock.Srl;
                        useItem.Sno = Xcnt;
                        useItem.Prefix = mperiod;
                        useItem.ENTEREDBY = muserid;
                        useItem.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                        useItem.AUTHORISE = mauthorise;
                        useItem.AUTHIDS = muserid;
                        ctxTFAT.UseItemStockDetail.Add(useItem);
                    }

                    if (mAdd)
                    {
                        ctxTFAT.ExtraItemStock.Add(tyreStock);
                    }
                    else
                    {
                        ctxTFAT.Entry(tyreStock).State = EntityState.Modified;
                    }

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();

                    UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, Parentkey, ConvertDDMMYYTOYYMMDD(Model.DocDate), 0, "", "Save General Stock Transfer", "NA");

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
                NewSrl = (mbranchcode + Model.ParentKey),
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteData(SparePartTransferVM Model)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction(System.Data.IsolationLevel.Unspecified))
            {
                try
                {
                    var mlist = ctxTFAT.ExtraItemStock.Where(x => x.Tablekey == Model.Document).FirstOrDefault();
                    DeUpdate(Model);
                    UpdateAuditTrail(mbranchcode, "Delete", Model.Header, mlist.Parentkey, mlist.DocDate, 0, "", "Delete General Stock Transfer", "NA");

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