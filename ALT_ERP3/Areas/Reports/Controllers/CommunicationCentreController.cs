//using EntitiModel;
//using System.Web.Razor.Generator;
//using System.Web.UI.WebControls;
using ALT_ERP3.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Reports.Controllers
{
    public class CommunicationCentreController : BaseController
    {
        private string msubcodeof = "";

        // GET: Reports/AccountLedger
        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "A");
            if (Model.Document == "" || Model.Document == null)
            {
                GetAllMenu(Session["ModuleName"].ToString());
                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                Model.ToDate = DateTime.Today.ToShortDateString();
                //System.Web.HttpContext.Current.Session["LastDate"].ToString();
            }
            if (Model.FromDate == null)
            {
                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                Model.ToDate = DateTime.Today.ToShortDateString();
            }
            ViewBag.ViewDataId = Model.ViewDataId;
            msubcodeof = Model.ViewDataId;
            Model.AccountName = NameofAccount(Model.Document);
            Model.ViewCode = GetDefaultCode(Model.ViewDataId);
            ViewBag.Header = Model.Header;
            ViewBag.Controller = Model.Controller;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;
            return View(Model);
        }

        public ActionResult GetDisplayType()
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "Leads", Text = "Leads" });
            GSt.Add(new SelectListItem { Value = "Contacts", Text = "Contacts" });
            GSt.Add(new SelectListItem { Value = "Enquiry", Text = "Opportunity" });
            GSt.Add(new SelectListItem { Value = "Quotes", Text = "Quotes" });
            GSt.Add(new SelectListItem { Value = "Orders", Text = "Orders" });
            GSt.Add(new SelectListItem { Value = "Team", Text = "Team" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTaskTypes()
        {
            return Json(GetDataTableList("Select Code,Name from ActivityType Order by Name"), JsonRequestBehavior.AllowGet);
            //return Json(GetDataTable("Select Code,Name from ActivityType").AsEnumerable().ToArray(), JsonRequestBehavior.AllowGet);
            //return Json((from m in ctxTFAT.ActivityType
            //             select new { m.Code, m.Name }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetInOut()
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "All", Text = "Show All" });
            GSt.Add(new SelectListItem { Value = "Inbound", Text = "Inbound" });
            GSt.Add(new SelectListItem { Value = "Outbound", Text = "Outbound" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTaskStatus()
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "All", Text = "Show All" });
            GSt.Add(new SelectListItem { Value = "Pending", Text = "Pending" });
            GSt.Add(new SelectListItem { Value = "Complete", Text = "Complete" });
            GSt.Add(new SelectListItem { Value = "OnHold", Text = "On Hold" });
            GSt.Add(new SelectListItem { Value = "Cancelled", Text = "Cancelled" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTrxType()
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "All", Text = "Show All" });
            GSt.Add(new SelectListItem { Value = "Opportunity", Text = "Opportunity" });
            GSt.Add(new SelectListItem { Value = "Quotes", Text = "Quotes Sent" });
            GSt.Add(new SelectListItem { Value = "Orders", Text = "Sales Orders" });
            GSt.Add(new SelectListItem { Value = "Delivery", Text = "Goods Delivery Notes" });
            GSt.Add(new SelectListItem { Value = "Sales", Text = "Sales" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCommType()
        {
            List<SelectListItem> GSt = new List<SelectListItem>();
            GSt.Add(new SelectListItem { Value = "All", Text = "Show All" });
            GSt.Add(new SelectListItem { Value = "Email", Text = "Email" });
            GSt.Add(new SelectListItem { Value = "SMS", Text = "SMS" });
            GSt.Add(new SelectListItem { Value = "Letter", Text = "Letter" });
            GSt.Add(new SelectListItem { Value = "Circular", Text = "Circular" });
            GSt.Add(new SelectListItem { Value = "Phone-Call", Text = "Phone-Call" });
            GSt.Add(new SelectListItem { Value = "Video-Conf", Text = "Video-Conf" });
            GSt.Add(new SelectListItem { Value = "Website", Text = "Website" });
            GSt.Add(new SelectListItem { Value = "Verbal", Text = "Verbal" });
            GSt.Add(new SelectListItem { Value = "Meeting", Text = "Meeting" });
            GSt.Add(new SelectListItem { Value = "Others", Text = "Others" });
            return Json(GSt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMobile(string Document)
        {
            string mcode = "";
            if (Document != "")
            {
                string mTable = GetTableName(Fieldoftable("DocTypes", "SubType", "Code = '" + Document.Substring(6, 5) + "'"));
                //string mTable = GetTableName(ctxTFAT.DocTypes.Where(x => x.Code == Document.Substring(6, 5)).Select(x => x.SubType).FirstOrDefault() ?? "");
                if (mTable != "")
                {
                    mcode = Fieldoftable(mTable, "Top 1 Code", "TableKey='" + Document.Substring(6) + "'");
                    int msno = Convert.ToInt32(FieldoftableNumber(mTable, "Top 1 AltAddress", "TableKey='" + Document.Substring(6) + "'"));
                    mcode = Fieldoftable("Address", "Mobile", "Code='" + mcode + "' and Sno=" + msno);
                    //ctxTFAT.Address.Where(x => x.Code == mcode && x.Sno == msno).Select(x => x.Mobile).FirstOrDefault() ?? "";
                }
            }
            return Json(mcode, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetEmailId(string Document)
        {
            string mcode = "";
            if (Document != "")
            {
                string mTable = GetTableName(Fieldoftable("DocTypes", "SubType", "Code = '" + Document.Substring(6, 5) + "'"));
                //string mTable = GetTableName(ctxTFAT.DocTypes.Where(x => x.Code == Document.Substring(6, 5)).Select(x => x.SubType).FirstOrDefault() ?? "");
                if (mTable != "")
                {
                    mcode = Fieldoftable(mTable, "Top 1 Code", "TableKey='" + Document.Substring(6) + "'");
                    int msno = Convert.ToInt32(FieldoftableNumber(mTable, "Top 1 AltAddress", "TableKey='" + Document.Substring(6) + "'"));
                    mcode = Fieldoftable("Address", "Email", "Code = '" + mcode + "' and Sno=" + msno);
                    //mcode = ctxTFAT.Address.Where(x => x.Code == mcode && x.Sno == msno).Select(x => x.Email).FirstOrDefault() ?? "";
                }
            }
            return Json(mcode, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetPartyInformation(GridOption Model)
        {
            string mstr = "";
            bool mok = true;
            int msno = Convert.ToInt32(Model.Sno ?? "0");
            if (Model.Type == "Leads")
            {
                int mcodeint = Convert.ToInt32(Model.Code);
                //var mas = ctxTFAT.Leads.Where(z => z.Code == mcodeint).Select(x => x).FirstOrDefault();
                DataTable mas = GetDataTable("Select * from Leads Where Code = '" + mcodeint + "'");
                if (mas != null)
                {
                    mstr += mas.Rows[0]["Name"] + "|";
                    if (mas.Rows[0]["Category"] != null && mas.Rows[0]["Category"].ToString() != "")
                    {
                        mstr += Fieldoftable("PartyCategory", "Name", "Code=" + Convert.ToInt32(mas.Rows[0]["Category"])) + "|";
                    }
                    else
                    {
                        mstr += "|";
                    }
                    mstr += (mas.Rows[0]["Contact"] ?? "") + " | ";
                    mstr += ((mas.Rows[0]["Adrl1"] ?? "") + " " + (mas.Rows[0]["Adrl2"] ?? "") + " " + (mas.Rows[0]["Adrl3"] ?? "") + ", " + (mas.Rows[0]["City"] ?? "") + " " + (mas.Rows[0]["State"] ?? "") + " " + (mas.Rows[0]["Country"] ?? "")) + "|";
                    mstr += "" + "|"; //area
                    mstr += "" + "|"; //gst
                    mstr += "" + "|"; //pan
                    mstr += (mas.Rows[0]["Mobile"] ?? "") + " | ";
                    mstr += (mas.Rows[0]["Email"] ?? "") + " | ";
                    mstr += ((mas.Rows[0]["Tel1"] ?? "") + " " + (mas.Rows[0]["Tel2"] ?? "") + " " + (mas.Rows[0]["Tel3"] ?? "") + " " + (mas.Rows[0]["Tel4"] ?? "")).Trim() + "|";
                    mstr += (mas.Rows[0]["Homepage"] ?? "") + " | ";
                    //mstr += mas.Name + "|";
                    //mstr += (ctxTFAT.PartyCategory.Where(z => z.Code == mas.Category).Select(x => x.Name).FirstOrDefault() ?? "") + "|";
                    //mstr += (mas.Contact ?? "") + "|";
                    //mstr += ((mas.Adrl1 ?? "") + " " + (mas.Adrl2 ?? "") + " " + (mas.Adrl3 ?? "") + ", " + (mas.City ?? "") + " " + (mas.State ?? "") + " " + (mas.Country ?? "")) + "|";
                    //mstr += "" + "|"; //area
                    //mstr += "" + "|"; //gst
                    //mstr += "" + "|"; //pan
                    //mstr += (mas.Mobile ?? "") + "|";
                    //mstr += (mas.Email ?? "") + "|";
                    //mstr += ((mas.Tel1 ?? "") + " " + (mas.Tel2 ?? "") + " " + (mas.Tel3 ?? "") + " " + (mas.Tel4 ?? "")).Trim() + "|";
                    //mstr += (mas.Homepage ?? "") + "|";
                }
                else
                {
                    mok = false;
                }
            }
            else if (Model.Type == "Team")
            {
                int mcodeint = Convert.ToInt32(Model.Code);
                //var mlead = ctxTFAT.SalesMan.Where(z => z.Code == mcodeint).Select(x => x).FirstOrDefault();
                DataTable mlead = GetDataTable("Select * from SalesMan Where Code = '" + mcodeint + "'");
                if (mlead != null)
                {
                    mstr += mlead.Rows[0]["Name"] + " | ";
                    mstr += (Fieldoftable("SalesmanCategory", "Name", "Code=" + Convert.ToInt32(mlead.Rows[0]["Category"])) ?? "") + " | ";
                    mstr += "" + "|"; //contact
                    mstr += ((mlead.Rows[0]["Adrl1"] ?? "") + " " + (mlead.Rows[0]["Adrl2"] ?? "") + " " + (mlead.Rows[0]["Adrl3"] ?? "") + ", " + (mlead.Rows[0]["City"] ?? "") + " " + (mlead.Rows[0]["State"] ?? "") + " " + (mlead.Rows[0]["Country"] ?? "")) + " |";
                    mstr += "" + "|"; //area
                    mstr += "" + "|"; //gst
                    mstr += "" + "|"; //pan
                    mstr += (mlead.Rows[0]["Mobile"] ?? "").ToString().Trim() + " | ";
                    mstr += (mlead.Rows[0]["Email"] ?? "").ToString().Trim() + " | ";
                    mstr += (mlead.Rows[0]["Telephone"] ?? "").ToString().Trim() + " | ";
                    mstr += "" + "|"; //www
                    //mstr += mlead.Name + "|";
                    //mstr += (ctxTFAT.SalesmanCategory.Where(z => z.Code == mlead.Category).Select(x => x.Name).FirstOrDefault() ?? "") + "|";
                    //mstr += "" + "|"; //contact
                    //mstr += ((mlead.Adrl1 ?? "") + " " + (mlead.Adrl2 ?? "") + " " + (mlead.Adrl3 ?? "") + ", " + (mlead.City ?? "") + " " + (mlead.State ?? "") + " " + (mlead.Country ?? "")) + "|";
                    //mstr += "" + "|"; //area
                    //mstr += "" + "|"; //gst
                    //mstr += "" + "|"; //pan
                    //mstr += (mlead.Mobile ?? "").Trim() + "|";
                    //mstr += (mlead.Email ?? "").Trim() + "|";
                    //mstr += (mlead.Telephone ?? "").Trim() + "|";
                    //mstr += "" + "|"; //www
                }
                else
                {
                    mok = false;
                }
            }
            else
            {
                //DataTable mas = GetDataTable("Select Name, Category from Master Where Code = '" + Model.Code + "'");
                mstr += NameofAccount(Model.Code, "A") + "|";
                mstr += (Fieldoftable("PartyCategory", "Name", "Code=" + FieldoftableNumber("Master", "Category", "Code='" + Model.Code + "'")) ?? "") + "|";
                //var mvar = ctxooroo.Address.Where(z => z.Code == Model.Code && z.Sno == msno).Select(x => x).FirstOrDefault();
                DataTable mvar = GetDataTable("Select * from Address Where Code = '" + Model.Code + "' and Sno = '" + msno + "'");
                //var mas = ctxTFAT.Master.Where(z => z.Code == Model.Code).Select(x => new { x.Name, x.City, x.BaseGr, x.Grp, x.Category }).FirstOrDefault();
                //mstr += mas.Name + "|";
                //mstr += (ctxTFAT.PartyCategory.Where(z => z.Code == mas.Category).Select(x => x.Name).FirstOrDefault() ?? "") + "|";
                //var mvar = ctxTFAT.Address.Where(z => z.Code == Model.Code && z.Sno == msno).Select(x => x).FirstOrDefault();
                if (mvar != null)
                {
                    mstr += (mvar.Rows[0]["Person"] ?? "") + " | ";
                    mstr += ((mvar.Rows[0]["Adrl1"] ?? "") + " " + (mvar.Rows[0]["Adrl2"] ?? "") + " " + (mvar.Rows[0]["Adrl3"] ?? "") + " " + (mvar.Rows[0]["Adrl4"] ?? "") + ", " + (mvar.Rows[0]["City"] ?? "") + " " + (mvar.Rows[0]["State"] ?? "") + " " + (mvar.Rows[0]["Country"] ?? "")) + " | ";
                    mstr += (Fieldoftable("AreaMaster", "Name", "Code = '" + mvar.Rows[0]["Area"] + "'") ?? "") + " | ";
                    mstr += (mvar.Rows[0]["GSTNo"] ?? "") + " | ";
                    mstr += (mvar.Rows[0]["PanNo"] ?? "") + " | ";
                    mstr += (mvar.Rows[0]["Mobile"] ?? "") + " | ";
                    mstr += (mvar.Rows[0]["Email"] ?? "") + " | ";
                    mstr += ((mvar.Rows[0]["Tel1"] ?? "") + " " + (mvar.Rows[0]["Tel2"] ?? "") + " " + (mvar.Rows[0]["Tel3"] ?? "") + " " + (mvar.Rows[0]["Tel4"] ?? "")).Trim() + "|";
                    mstr += (mvar.Rows[0]["www"] ?? "") + " | ";
                    //mstr += (mvar.Person ?? "") + "|";
                    //mstr += ((mvar.Adrl1 ?? "") + " " + (mvar.Adrl2 ?? "") + " " + (mvar.Adrl3 ?? "") + " " + (mvar.Adrl4 ?? "") + ", " + (mvar.City ?? "") + " " + (mvar.State ?? "") + " " + (mvar.Country ?? "")) + "|";
                    //mstr += (ctxTFAT.AreaMaster.Where(z => z.Code == mvar.Area).Select(x => x.Name).FirstOrDefault() ?? "") + "|";
                    //mstr += (mvar.GSTNo ?? "") + "|";
                    //mstr += (mvar.PanNo ?? "") + "|";
                    //mstr += (mvar.Mobile ?? "") + "|";
                    //mstr += (mvar.Email ?? "") + "|";
                    //mstr += ((mvar.Tel1 ?? "") + " " + (mvar.Tel2 ?? "") + " " + (mvar.Tel3 ?? "") + " " + (mvar.Tel4 ?? "")).Trim() + "|";
                    //mstr += (mvar.www ?? "") + "|";
                }
                else
                {
                    mok = false;
                }
            }
            if (mok == false)
            {
                mstr += "|";
                mstr += "|";
                mstr += "|";
                mstr += "|";
                mstr += "|";
                mstr += "|";
                mstr += "|";
                mstr += "|";
                mstr += "|";

            }
            //var mvar2 = ctxTFAT.MasterInfo.Where(z => z.Code == Model.Code).Select(x => x).FirstOrDefault();
            //if (mvar2 != null)
            //{
            //    mstr += (mvar2.Rank ?? 0) + "|";
            //    mstr += (mvar2.CrLimit ?? 0) + "|";
            //    mstr += (mvar2.CrPeriod ?? 0) + "|";
            DataTable mvar2 = GetDataTable("Select Rank,CrLimit,CrPeriod from MasterInfo Where Code = '" + Model.Code + "'");
            if (mvar2.Rows.Count > 0)
            {
                mstr += (mvar2.Rows[0]["Rank"] ?? 0) + " | ";
                mstr += (mvar2.Rows[0]["CrLimit"] ?? 0) + " | ";
                mstr += (mvar2.Rows[0]["CrPeriod"] ?? 0) + " | ";
            }
            else
            {
                mstr += 0 + "|";
                mstr += 0 + "|";
                mstr += 0 + "|";
            }
            // Monthly balances
            string mquery = "Select FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4)) as MonthName, Sum(Debit-Credit) as Amt from Ledger where Code='" + Model.Code + "' and docdate>='%RepStartDate' and docdate<='%RepEndDate' Group by Code,FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4))" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(Type,'" + mDocString + "')=0");
            string[] mArr = GetMonthlyBalance(mquery);
            mstr += string.Join(",", mArr) + "|";
            // sales
            if (Model.Type == "Team")
            {
                mquery = "Select FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4)) as MonthName, Sum(Amt) as Amt from Orders where docdate>='%RepStartDate' and docdate<='%RepEndDate' and Salesman=" + Model.Code + " Group by Salesman,FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4))" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(Type,'" + mDocString + "')=0");
            }
            else
            {
                mquery = "Select FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4)) as MonthName, Sum(Amt) as Amt from Orders where Code='" + Model.Code + "' and docdate>='%RepStartDate' and docdate<='%RepEndDate' Group by Code,FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4))" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(Type,'" + mDocString + "')=0");
            }
            mArr = GetMonthlyBalance(mquery);
            mstr += string.Join(",", mArr) + "|";
            //receipts
            mquery = "Select FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4)) as MonthName, Sum(Credit) as Amt from Ledger where MainType='RC' and Credit>0 and Code='" + Model.Code + "' and Docdate>='%RepStartDate' and Docdate<='%RepEndDate' Group by Code,FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4))" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(Type,'" + mDocString + "')=0");
            mArr = GetMonthlyBalance(mquery);
            mstr += string.Join(",", mArr) + "|";
            return Json(mstr, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            Model.ViewDataId = "CRM-" + Model.Type + "-List";
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        public ActionResult GetGridData(GridOption Model)
        {
            Model.ViewDataId = "CRM-" + Model.Type + "-List";
            return GetGridReport(Model, "R");
        }

        #region subreport
        [HttpPost]
        public ActionResult GetGridStructureSub(GridOption Model)
        {
            switch (Model.Type)
            {
                case "Leads":
                    break;
                case "Contacts":
                    break;
                case "Opportunity":
                    break;
                case "Quotes":
                    break;
                case "Orders":
                    break;
                case "Team":
                    break;
            }
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        [HttpPost]
        public ActionResult GetGridDataSub(GridOption Model)
        {
            if (Model.ViewDataId == "CRM-TransactionList")
            {
                SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                SqlCommand cmd = new SqlCommand("SPTFAT_CRM_GenerateTrxData", tfat_conx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;
                cmd.Parameters.Add("@mCode", SqlDbType.VarChar).Value = Model.Code ?? "";
                cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
                cmd.Parameters.Add("@mShow", SqlDbType.VarChar).Value = Model.Type == null || Model.Type == "0" ? "All" : Model.Type;
                cmd.Parameters.Add("@mDocString", SqlDbType.VarChar).Value = mDocString;
                tfat_conx.Open();
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
                tfat_conx.Close();
            }
            // type = msgtype or commtype
            //if (Model.Type == "All") Model.Type = "";
            return GetGridReport(Model, "R", "Document^" + (Model.Code ?? "") + "~Code^" + (Model.Code ?? "") + "~Type^" + (Model.Type ?? ""), false, 0);
        }
        #endregion subreport
    }
}