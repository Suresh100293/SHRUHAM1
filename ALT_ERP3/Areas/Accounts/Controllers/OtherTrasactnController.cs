using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class OtherTrasactnController : BaseController
    {
        private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mAUTHORISE = "A00";
        private static string mdocument = "";

        // GET: Accounts/OtherTrasactn
        public ActionResult Index(OtherTransactModel Model)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "A");
            mdocument = Model.Document;
            if (Model.Mode == "Add")
            {
                Model.DocDate = DateTime.Now;
                Session["othertrxlist"] = null;
            }
            return View(Model);
        }

        public ActionResult GetCashBankList(string term)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.Master.Where(x => x.BaseGr == "C" || x.BaseGr == "B").Select(c => new { c.Code, c.Name }).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            else
            {

                var result = ctxTFAT.Master.Where(x => x.Name.Contains(term) && (x.BaseGr == "C" || x.BaseGr == "B")).Select(m => new { m.Code, m.Name }).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetAccountList(string term/*, string BaseGr*/)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.Master.Select(m => new
                {
                    m.Code,
                    m.Name
                }).OrderBy(n => n.Name).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.Master.Where(x => x.Name.Contains(term)).Select(m => new
                {
                    m.Code,
                    m.Name
                }).OrderBy(n => n.Name).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetRelatedToList()
        {

            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "Accident", Text = "Accident" });
            GSt.Add(new SelectListItem { Value = "Body Building", Text = "Body Building" });
            GSt.Add(new SelectListItem { Value = "Miscellaneous Charges", Text = "Miscellaneous Charges" });
            GSt.Add(new SelectListItem { Value = "Others", Text = "Others" });
            GSt.Add(new SelectListItem { Value = "Principal Amount", Text = "Principal Amount" });
            GSt.Add(new SelectListItem { Value = "Diesel", Text = "Diesel" });
            GSt.Add(new SelectListItem { Value = "Fitness", Text = "Fitness" });
            GSt.Add(new SelectListItem { Value = "Green Tax", Text = "Green Tax" });
            GSt.Add(new SelectListItem { Value = "Permit 1 Year", Text = "Permit 1 Year" });
            GSt.Add(new SelectListItem { Value = "Permit 5 Year", Text = "Permit 5 Year" });
            GSt.Add(new SelectListItem { Value = "PT", Text = "PT" });
            GSt.Add(new SelectListItem { Value = "PUC", Text = "PUC" });

            GSt.Add(new SelectListItem { Value = "Insurance", Text = "Insurance" });
            GSt.Add(new SelectListItem { Value = "Loan", Text = "Loan" });
            GSt.Add(new SelectListItem { Value = "Spare Parts & Repairs", Text = "Spare Parts & Repairs" });
            GSt.Add(new SelectListItem { Value = "Trip", Text = "Trip" });
            GSt.Add(new SelectListItem { Value = "Tyre", Text = "Tyre" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        #region ADD LEDGER ITEM

        public ActionResult AddCashLedger(OtherTransactModel Model)
        {
            //if (Model.SessionFlag == "Add")
            //{
            List<OtherTransactModel> objledgerdetail = new List<OtherTransactModel>();

            List<OtherTransactModel> lrdetaillist = new List<OtherTransactModel>();


            if (Session["othertrxlist"] != null)
            {
                objledgerdetail = (List<OtherTransactModel>)Session["othertrxlist"];
            }

            //if (Model.OSAdjList != null && Model.OSAdjList.Count > 0)
            //{
            //    var item3 = Model.OSAdjList.Select(x => x.Srl).ToList();
            //    string osadjnarr = String.Join(",", item3);

            //    Model.Narr = Model.Narr + "\n" + "Adjusted Bill No(s): " + osadjnarr;
            //}
            objledgerdetail.Add(new OtherTransactModel()
            {

                Code = Model.Code,
                AccountName = Model.AccountName,
                LRDetailList = Model.LRDetailList,
                AddOnList = Model.AddOnList,

                Debit = (Model.AmtType == "Payment") ? Model.Amt : 0,
                Credit = (Model.AmtType == "Receipt") ? Model.Amt : 0,
                Narr = Model.Narr,
                Amt = Model.Amt,
                AmtType = Model.AmtType,
                tempId = objledgerdetail.Count + 1,

            });

            Session.Add("othertrxlist", objledgerdetail);
            decimal sumdebit = objledgerdetail.Sum(x => x.Debit);
            decimal sumcredit = objledgerdetail.Sum(x => x.Credit);
            var html = ViewHelper.RenderPartialView(this, "LedgerList", new OtherTransactModel() { Selectedleger = objledgerdetail });
            return Json(new { Selectedleger = objledgerdetail, Html = html, Sumdebit = sumdebit, Sumcredit = sumcredit }, JsonRequestBehavior.AllowGet);
            //}

        }
        [HttpPost]
        public ActionResult Deleteledger(OtherTransactModel Model)
        {
            var result = (List<OtherTransactModel>)Session["othertrxlist"];

            var result2 = result.Where(x => x.tempId != Model.tempId).Select(x => x).ToList();
            Session.Add("othertrxlist", result2);
            decimal sumdebit = result2.Sum(x => x.Debit);
            decimal sumcredit = result2.Sum(x => x.Credit);

            var html = ViewHelper.RenderPartialView(this, "LedgerList", new OtherTransactModel() { Selectedleger = result2 });
            return Json(new
            {
                Selectedleger = result2,
                Html = html,
                Sumdebit = sumdebit,
                Sumcredit = sumcredit
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCashLedger(OtherTransactModel Model)
        {


            var result = (List<OtherTransactModel>)Session["othertrxlist"];
            var result1 = result.Where(x => x.tempId == Model.tempId);
            foreach (var item in result1)
            {
                Model.Code = item.Code;
                Model.AccountName = item.AccountName;
                Model.LRDetailList = item.LRDetailList;

                Model.AddOnList = item.AddOnList;
                Model.Debit = (item.AmtType == "Payment") ? Model.Amt : 0;
                Model.Credit = (item.AmtType == "Receipt") ? Model.Amt : 0;
                Model.Narr = item.Narr;
                Model.Amt = item.Amt;
                Model.AmtType = item.AmtType;
            }
            Model.Selectedleger = result;

            var jsonResult = Json(new { Html = this.RenderPartialView("LedgerList", Model), Code = Model.Code, AccountName = Model.AccountName }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;



        }

        #endregion
        #region ADD LR details
        public ActionResult AddLRDetails(OtherTransactModel Model)
        {
            List<OtherTransactModel> lrdetaillist = new List<OtherTransactModel>();
            if (Model.LRDetailList != null)
            {
                lrdetaillist = Model.LRDetailList;
            }
            lrdetaillist.Add(new OtherTransactModel()
            {
                LRNumber = Model.LRNumber,
                Amt = Model.Amt,
                tempId = lrdetaillist.Count + 1,

            });

            var html = ViewHelper.RenderPartialView(this, "LRDetails", new OtherTransactModel() { LRDetailList = lrdetaillist });
            return Json(new { LRDetailList = lrdetaillist, Html = html }, JsonRequestBehavior.AllowGet);
            //}

        }
        [HttpPost]
        public ActionResult DeleteLRDetails(OtherTransactModel Model)
        {


            var result2 = Model.LRDetailList.Where(x => x.tempId != Model.tempId).Select(x => x).ToList();

            var html = ViewHelper.RenderPartialView(this, "LRDetails", new OtherTransactModel() { LRDetailList = result2 });
            return Json(new { LRDetailList = result2, Html = html }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        [HttpPost]
        public ActionResult GetTruckTypeWiseViewList(OtherTransactModel Model)
        {
            List<AddOns> truckaddonlist = new List<AddOns>();
            if (Model.RelatedTo == "Accident")
            {
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "Accident Date",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "D"

                });
            }

            if (Model.RelatedTo == "Body Building" || Model.RelatedTo == "Miscellaneous Charges" || Model.RelatedTo == "Others" || Model.RelatedTo == "Principal Amount")
            {
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "Amount",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"

                });
            }
            if (Model.RelatedTo == "Diesel")
            {
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "Diesel Receipt No",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F002",
                    Head = "Litres",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"

                });
            }
            if (Model.RelatedTo == "Fitness" || Model.RelatedTo == "Green Tax" || Model.RelatedTo == "Permit 1 Year" || Model.RelatedTo == "Permit 5 Year" || Model.RelatedTo == "PT" || Model.RelatedTo == "PUC")
            {
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "From",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "D"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F002",
                    Head = "To",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "D"

                });
            }

            if (Model.RelatedTo == "Insurance" || Model.RelatedTo == "Loan")
            {
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "Insurance Company",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "D"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F002",
                    Head = "From",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "D"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F003",
                    Head = "To",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "D"

                });
            }
            if (Model.RelatedTo == "Spare Parts & Repairs")
            {

                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "Spare Part",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F002",
                    Head = "Cost",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F003",
                    Head = "Receipt No",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F004",
                    Head = "Receipt Dt",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "D"

                });

                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F005",
                    Head = "KMS",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F006",
                    Head = "Due Date",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "D"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F007",
                    Head = "Due km",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"

                });
            }

            if (Model.RelatedTo == "Trip")
            {

                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "Trip Start Date",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "D"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F002",
                    Head = "Trip End Date",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "D"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F003",
                    Head = "Starting kms",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F004",
                    Head = "Ending kms",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"

                });

                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F005",
                    Head = "Charge KMS",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F006",
                    Head = "Rate",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F007",
                    Head = "Trip Charges",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"

                });
            }

            if (Model.RelatedTo == "Tyre")
            {

                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "Manufacturing",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F002",
                    Head = "Type",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "D"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F003",
                    Head = "Posting",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F004",
                    Head = "Side",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"

                });

                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F005",
                    Head = "No",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F006",
                    Head = "Mfg Date",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "D"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F007",
                    Head = "Install Date",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "D"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F007",
                    Head = "Expiry Date",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "D"

                });
            }
            var html = ViewHelper.RenderPartialView(this, "TruckDetails", new OtherTransactModel() { AddOnList = truckaddonlist, RelatedTo = Model.RelatedTo });
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }
    }
}