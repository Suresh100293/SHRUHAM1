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


namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class TripSheetController : BaseController
    {
        private static string mdocument = "";
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        private static string mauthorise = "A00";
        readonly //ITransactionGridOperation mIlst = new TransactionGridOperation();
        private static string connstring;


        #region ALL GET 
        public ActionResult GetLoanTrxTypes()
        {

            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "A", Text = "Advance" });
            GSt.Add(new SelectListItem { Value = "B", Text = "Balance" });

            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBankList(string term)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.Master.Where(x => (x.BaseGr == "B" || x.BaseGr == "C") && x.Hide == false).Select(c => new { c.Code, c.Name }).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var result = ctxTFAT.Master.Where(x => x.Name.Contains(term) && (x.BaseGr == "B" || x.BaseGr == "C") && x.Hide == false).Select(m => new { m.Code, m.Name }).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }



        public ActionResult GetBranchList()
        {
            List<SelectListItem> branch = new List<SelectListItem>();
            var warehouselist = ctxTFAT.TfatBranch.Select(b => new
            {
                b.Code,
                b.Name
            }).ToList();
            foreach (var item in warehouselist)
            {
                branch.Add(new SelectListItem { Text = item.Name, Value = item.Code });
            }
            return Json(branch, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetAreaList()
        {
            List<SelectListItem> branch = new List<SelectListItem>();
            var warehouselist = ctxTFAT.AreaMaster.Select(b => new
            {
                b.Code,
                b.Name
            }).ToList();
            foreach (var item in warehouselist)
            {
                branch.Add(new SelectListItem { Text = item.Name, Value = item.Code.ToString() });
            }
            return Json(branch, JsonRequestBehavior.AllowGet);

        }


        public ActionResult GetAccountList(string term)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.Master.Where(x => x.BaseGr == "V").Select(c => new { c.Code, c.Name }).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var result = ctxTFAT.Master.Where(x => x.Name.Contains(term) && x.BaseGr == "V").Select(m => new { m.Code, m.Name }).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetExpenseList(string term)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.Master.Where(x => x.BaseGr == "X").Select(c => new { c.Code, c.Name }).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var result = ctxTFAT.Master.Where(x => x.Name.Contains(term) && x.BaseGr == "X").Select(m => new { m.Code, m.Name }).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }



        #endregion
        public ActionResult Index(TripSheetModel Model)
        {

            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "A");

            mdocument = Model.Document;

            if (Model.Mode == "Add")
            {
                Model.DocDate = DateTime.Now.Date;
                Session["tripsheetlist"] = null;
                Model.Type = Model.Type;
                //var mdoctypes = ctxTFAT.DocTypes.Where(X => X.Code == Model.Type).Select(X => X).FirstOrDefault();
                //Model.MainType = mdoctypes.MainType;
                //Model.SubType = mdoctypes.SubType;

            }
            else
            {
                Model.Type = Model.Type;
                var mdoctypes = ctxTFAT.DocTypes.Where(X => X.Code == Model.Type).Select(X => X).FirstOrDefault();
                Model.MainType = mdoctypes.MainType;
                Model.SubType = mdoctypes.SubType;

            }
            return View(Model);
        }


        public ActionResult GetRelateToList()
        {

            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "1", Text = "Trip" });

            return Json(GSt, JsonRequestBehavior.AllowGet);
        }


        #region Ledger add edit
        public ActionResult AddEditLedgerDetails(TripSheetModel Model)
        {
            List<TripSheetModel> objledgerdetail = new List<TripSheetModel>();

            if (Session["tripsheetlist"] != null)
            {
                objledgerdetail = (List<TripSheetModel>)Session["tripsheetlist"];
            }
            if (Model.SessionFlag == "Edit")
            {

                foreach (var item in objledgerdetail.Where(x => x.tempid == Model.tempid))
                {
                    item.RelateTo = Model.RelateTo;
                    item.ChargeTrip = Model.ChargeTrip;
                    item.Rate = Model.Rate;
                    item.ChargeKM = Model.ChargeKM;
                    item.StartDateStr = Model.StartDateStr;
                    item.StartKM = Model.StartKM;
                    item.EndDateStr = Model.EndDateStr;
                    item.EndKM = Model.EndKM;
                    item.LCDetailList = Model.LCDetailList;
                    item.AddNarrList = Model.AddNarrList;
                    item.LessNarrList = Model.LessNarrList;
                    item.Account = Model.Account;
                    item.AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault();
                    item.Debit = Model.Debit;
                    item.Credit = Model.Credit;
                    item.Remark = Model.Remark;
                }

            }
            else
            {
                objledgerdetail.Add(new TripSheetModel()
                {
                    RelateTo = Model.RelateTo,
                    ChargeTrip = Model.ChargeTrip,
                    Rate = Model.Rate,
                    ChargeKM = Model.ChargeKM,
                    StartDateStr = Model.StartDateStr,
                    StartKM = Model.StartKM,
                    EndDateStr = Model.EndDateStr,
                    EndKM = Model.EndKM,
                    LCDetailList = Model.LCDetailList,
                    AddNarrList = Model.AddNarrList,
                    LessNarrList = Model.LessNarrList,
                    Account = Model.Account,
                    AccountName = ctxTFAT.Master.Where(x => x.Code == Model.Account).Select(x => x.Name).FirstOrDefault(),
                    Debit = Model.Debit,
                    Credit = Model.Credit,
                    Remark = Model.Remark,
                    tempid = objledgerdetail.Count + 1,

                }); ;

            }
            Session.Add("tripsheetlist", objledgerdetail);

            var html = ViewHelper.RenderPartialView(this, "LedgerList", new TripSheetModel() { SelectedLedger = objledgerdetail });
            var jsonResult = Json(new
            {
                Status = "Success",
                Html = html
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }

        [HttpPost]
        public ActionResult DeleteLedgerDetails(TripSheetModel Model)
        {

            var result = (List<TripSheetModel>)Session["tripsheetlist"];

            var result2 = result.Where(x => x.tempid != Model.tempid).ToList();

            Session["tripsheetlist"] = result2;
            var html = ViewHelper.RenderPartialView(this, "LedgerList", new TripSheetModel() { SelectedLedger = result2 });
            return Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetLedgerDetailsInEdit(TripSheetModel Model)
        {
            List<TripSheetModel> objledgerdetail = new List<TripSheetModel>();
            if (Session["tripsheetlist"] != null)
            {
                objledgerdetail = (List<TripSheetModel>)Session["tripsheetlist"];
            }
            foreach (var item in objledgerdetail.Where(x => x.tempid == Model.tempid))
            {
                Model.RelateTo = item.RelateTo;
                Model.ChargeTrip = item.ChargeTrip;
                Model.Rate = item.Rate;
                Model.ChargeKM = item.ChargeKM;
                Model.StartDateStr = item.StartDateStr;
                Model.StartKM = item.StartKM;
                Model.EndDateStr = item.EndDateStr;
                Model.EndKM = item.EndKM;
                Model.LCDetailList = item.LCDetailList;
                Model.AddNarrList = item.AddNarrList;
                Model.LessNarrList = item.LessNarrList;
                Model.Account = item.Account;
                Model.AccountName = ctxTFAT.Master.Where(x => x.Code == item.Account).Select(x => x.Name).FirstOrDefault();
                Model.Debit = item.Debit;
                Model.Credit = item.Credit;
                Model.Remark = item.Remark;
            }
            Model.SelectedLedger = objledgerdetail;
            Model.SessionFlag = "Edit";
            var jsonResult = Json(new { Html = this.RenderPartialView("LedgerList", Model) }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }


        #endregion

        #region Relate Truck Details


        public ActionResult GetTripSheetDetail(TripSheetModel Model)
        {
            if (Model.SessionFlag == "Edit")
            {
                List<TripSheetModel> objledgerdetail = new List<TripSheetModel>();

                if (Session["tripsheetlist"] != null)
                {
                    objledgerdetail = (List<TripSheetModel>)Session["tripsheetlist"];
                }

                foreach (var item in objledgerdetail.Where(x => x.tempid == Model.tempid))
                {
                    Model.RelateTo = item.RelateTo;
                    Model.ChargeTrip = item.ChargeTrip;
                    Model.Rate = item.Rate;
                    Model.ChargeKM = item.ChargeKM;
                    Model.StartDateStr = item.StartDateStr;
                    Model.StartKM = item.StartKM;
                    Model.EndDateStr = item.EndDateStr;
                    Model.EndKM = item.EndKM;
                    Model.LCDetailList = item.LCDetailList;
                    Model.AddNarrList = item.AddNarrList;
                    Model.LessNarrList = item.LessNarrList;
                    Model.EndDate = ConvertDDMMYYTOYYMMDD(item.EndDateStr);
                    Model.StartDate = ConvertDDMMYYTOYYMMDD(item.StartDateStr);
                    Model.TripFinalAmt = item.TripFinalAmt;
                }


                var jsonResult = Json(new { Html = this.RenderPartialView("TripPopUpAddEdit", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            else
            {
                Model.StartDate = DateTime.Now.Date;
                Model.EndDate = DateTime.Now.Date;
                var jsonResult = Json(new { Html = this.RenderPartialView("TripPopUpAddEdit", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }


        #endregion



        #region LCDetails add edit
        public ActionResult AddEditLCDetails(TripSheetModel Model)
        {
            List<TripSheetModel> objledgerdetail = new List<TripSheetModel>();

            if (Model.LCDetailList != null)
            {
                objledgerdetail = Model.LCDetailList;
            }
            if (Model.SessionFlag == "Edit")
            {

                foreach (var item in objledgerdetail.Where(x => x.tempid == Model.tempid))
                {
                    item.LCNo = Model.LCNo;
                    item.LCDate = Model.LCDate;
                    item.FromLocation = Model.FromLocation;
                    item.FromName = ctxTFAT.AreaMaster.Where(x => x.Code == Model.FromLocation).Select(x => x.Name).FirstOrDefault();
                    item.ToName = ctxTFAT.AreaMaster.Where(x => x.Code == Model.ToLocation).Select(x => x.Name).FirstOrDefault();
                    item.ToLocation = Model.ToLocation;
                    item.ChargesAcc = Model.ChargesAcc;
                    item.ChargesAccName = Model.ChargesAccName;
                    item.Amt = Model.Amt;
                    item.Descr = Model.Descr;
                }

            }
            else
            {
                objledgerdetail.Add(new TripSheetModel()
                {
                    LCNo = Model.LCNo,
                    LCDate = Model.LCDate,
                    FromLocation = Model.FromLocation,
                    ToLocation = Model.ToLocation,
                    FromName = ctxTFAT.AreaMaster.Where(x => x.Code == Model.FromLocation).Select(x => x.Name).FirstOrDefault(),
                    ToName = ctxTFAT.AreaMaster.Where(x => x.Code == Model.ToLocation).Select(x => x.Name).FirstOrDefault(),
                    ChargesAcc = Model.ChargesAcc,
                    ChargesAccName = Model.ChargesAccName,
                    Amt = Model.Amt,
                    Descr = Model.Descr,
                    tempid = objledgerdetail.Count + 1,

                });

            }
            Model.TripFinalAmt = (objledgerdetail.Sum(x => (decimal?)x.Amt) ?? 0) - Model.LessAmt + Model.AddAmt;

            var html = ViewHelper.RenderPartialView(this, "LCDetails", new TripSheetModel() { LCDetailList = objledgerdetail });
            var jsonResult = Json(new
            {
                Status = "Success",
                Html = html,
                TripFinalAmt = Model.TripFinalAmt
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }

        [HttpPost]
        public ActionResult DeleteLCDetails(TripSheetModel Model)
        {
            var result = Model.LCDetailList;
            var result2 = result.Where(x => x.tempid != Model.tempid).ToList();
            var html = ViewHelper.RenderPartialView(this, "LCDetails", new TripSheetModel() { LCDetailList = result2 });
            return Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
        }


        #endregion


        #region  add narr list
        public ActionResult AddEditAddNarrList(TripSheetModel Model)
        {
            List<TripSheetModel> objledgerdetail = new List<TripSheetModel>();

            if (Model.AddNarrList != null)
            {
                objledgerdetail = Model.AddNarrList;
            }
            if (Model.SessionFlag == "Edit")
            {

                foreach (var item in objledgerdetail.Where(x => x.tempid == Model.tempid))
                {
                    item.AddNarr = Model.AddNarr;

                    item.AddAmt = Model.AddAmt;
                }

            }
            else
            {
                objledgerdetail.Add(new TripSheetModel()
                {

                    AddAmt = Model.AddAmt,
                    AddNarr = Model.AddNarr,
                    tempid = objledgerdetail.Count + 1,

                });

            }

            Model.TripFinalAmt = (objledgerdetail.Sum(x => (decimal?)x.AddAmt) ?? 0) - Model.LessAmt + Model.LCAmt;
            var html = ViewHelper.RenderPartialView(this, "AddNarrList", new TripSheetModel() { AddNarrList = objledgerdetail });
            var jsonResult = Json(new
            {
                Status = "Success",
                Html = html,
                TripFinalAmt = Model.TripFinalAmt
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }

        [HttpPost]
        public ActionResult DeleteAddNarrList(TripSheetModel Model)
        {
            var result = Model.AddNarrList;
            var result2 = result.Where(x => x.tempid != Model.tempid).ToList();
            var html = ViewHelper.RenderPartialView(this, "AddNarrList", new TripSheetModel() { AddNarrList = result2 });
            return Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
        }


        #endregion


        #region less narr list
        public ActionResult AddEditLessNarrList(TripSheetModel Model)
        {
            List<TripSheetModel> objledgerdetail = new List<TripSheetModel>();

            if (Model.LessNarrList != null)
            {
                objledgerdetail = Model.LessNarrList;
            }
            if (Model.SessionFlag == "Edit")
            {

                foreach (var item in objledgerdetail.Where(x => x.tempid == Model.tempid))
                {
                    item.LessNarr = Model.LessNarr;

                    item.LessAmt = Model.LessAmt;
                }

            }
            else
            {
                objledgerdetail.Add(new TripSheetModel()
                {

                    LessAmt = Model.LessAmt,
                    LessNarr = Model.LessNarr,
                    tempid = objledgerdetail.Count + 1,

                });

            }
            Model.TripFinalAmt =  Model.AddAmt-(objledgerdetail.Sum(x => (decimal?)x.LessAmt) ?? 0)  + Model.LCAmt;

            var html = ViewHelper.RenderPartialView(this, "LessNarrList", new TripSheetModel() { LessNarrList = objledgerdetail });
            var jsonResult = Json(new
            {
                Status = "Success",
                Html = html,
                TripFinalAmt = Model.TripFinalAmt
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }

        [HttpPost]
        public ActionResult DeleteLessNarrList(TripSheetModel Model)
        {
            var result = Model.LessNarrList;
            var result2 = result.Where(x => x.tempid != Model.tempid).ToList();
            var html = ViewHelper.RenderPartialView(this, "LessNarrList", new TripSheetModel() { LessNarrList = result2 });
            return Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
        }


        #endregion

        //#region SaveData
        //public ActionResult SaveData(TripSheetModel mModel)
        //{
        //    using (var transaction = ctxTFAT.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            // iX9: Remove Existing Data for Delete Mode
        //            if (mModel.Mode == "Delete")
        //            {
        //                DeUpdate(mModel);
        //                if (mModel.Mode == "Delete")
        //                {
        //                    transaction.Commit();
        //                    transaction.Dispose();
        //                    return Json(new
        //                    {
        //                        Status = "success",
        //                        Message = "Data is Deleted."
        //                    }, JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //            TDSMaster mobj = new TDSMaster();
        //            if (mModel.Mode == "Edit")
        //            {
        //                mobj = ctxTFAT.TDSMaster.Where(x => (x.Code == mModel.TDSMaster_Code)).FirstOrDefault();
        //            }
                  
        //            mobj.LASTUPDATEDATE = System.DateTime.Now;
        //            if (mModel.Mode == "Add")
        //            {
        //                mobj.Code = GetNextCode();
        //            }
        //            if (mModel.Mode == "Add")
        //            {
        //                ctxTFAT.TDSMaster.Add(mobj);
        //            }
        //            else
        //            {
        //                ctxTFAT.Entry(mobj).State = EntityState.Modified;
        //            }
        //            ctxTFAT.SaveChanges();
                   
                   
        //            transaction.Commit();
        //            transaction.Dispose();
        //        }
        //        catch (DbEntityValidationException ex1)
        //        {
        //            transaction.Rollback();
        //            string dd1 = ex1.InnerException.Message;
        //        }
        //        catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
        //        {
        //            transaction.Rollback();
        //            string dd = ex.InnerException.Message;
        //        }
        //        catch (Exception e)
        //        {
        //            ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
        //            ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
        //        }
        //    }
        //    return Json(new { Status = "Success", id = "TDSMaster" }, JsonRequestBehavior.AllowGet);
        //}
      
        //public ActionResult DeleteData(TripSheetModel mModel)
        //{
        //    // iX9: Check for Active Master TDSMaster
        //    string mactivestring = "";
        //    var mactive1 = ctxTFAT.TDSChallan.Where(x => (x.TDSCode == mModel.TDSMaster_Code)).Select(x => x.TDSCode).FirstOrDefault();
        //    if (mactive1 != null) { mactivestring = mactivestring + "\nTDSChallan: " + mactive1; }
        //    var mactive2 = ctxTFAT.TDSPayments.Where(x => (x.TDSCode == mModel.TDSMaster_Code)).Select(x => x.TDSCode).FirstOrDefault();
        //    if (mactive2 != null) { mactivestring = mactivestring + "\nTDSPayments: " + mactive2; }
        //    var mactive3 = ctxTFAT.TDSRates.Where(x => (x.Code == mModel.TDSMaster_Code)).Select(x => x.Code).FirstOrDefault();
        //    if (mactive3 != null) { mactivestring = mactivestring + "\nTDSRates: " + mactive3; }
        //    if (mactivestring != "")
        //    {
        //        return Json(new
        //        {
        //            Message = "Active Item, Can't Delete..\nUsed with:\n" + mactivestring,
        //            Status = "Error"
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    var mList = ctxTFAT.TDSMaster.Where(x => (x.Code == mModel.TDSMaster_Code)).FirstOrDefault();
        //    ctxTFAT.TDSMaster.Remove(mList);
        //    var mList2 = ctxTFAT.TDSRates.Where(x => x.Code == mModel.TDSMaster_Code).ToList();
        //    ctxTFAT.TDSRates.RemoveRange(mList2);
        //    ctxTFAT.SaveChanges();
        //    return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        //}

        //private void DeUpdate(TripSheetModel Model)
        //{

        //}
        //#endregion SaveData

    }
}