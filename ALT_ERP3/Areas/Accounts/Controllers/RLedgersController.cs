using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Linq;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class RLedgersController : BaseController
    {
        private static decimal mOpeningBalance = 0;
        private static string msubcodeof = "";
        private static string mbasegr = "";
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
        private List<SelectListItem> PopulateBranchesOnly()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            if (muserid.Trim().ToUpper() == "SUPER")
            {
                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    string query = "SELECT Code, Name FROM TfatBranch where Category='Branch' or Category='SubBranch' order by Recordkey ";
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                items.Add(new SelectListItem
                                {
                                    Text = sdr["Name"].ToString(),
                                    Value = sdr["Code"].ToString()
                                });
                            }
                        }
                        con.Close();
                    }
                }
            }
            else
            {
                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    string query = "SELECT Code, Name FROM TfatBranch where Category='Branch' or Category='SubBranch' and Users like'%" + muserid + "%' order by Recordkey ";
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                items.Add(new SelectListItem
                                {
                                    Text = sdr["Name"].ToString(),
                                    Value = sdr["Code"].ToString()
                                });
                            }
                        }
                        con.Close();
                    }
                }
            }
            return items;
        }

        

        #region TreeView
        public string TreeView(string Mode, string Document)
        {
            string BranchCode = "";
            string[] BranchArray = new string[100];
            if (Mode == "Add")
            {
                BranchCode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
            }
            else
            {
                long id = Convert.ToInt64(Document);
                var Branchlist = ctxTFAT.VehicleMaster.Where(x => x.RECORDKEY == id).Select(x => x.Branch).FirstOrDefault();
                BranchArray = Branchlist.ToString().Split(',');
            }

            var mTreeList = ctxTFAT.TfatBranch.Where(x => x.Category != "Area" && x.Code != "G00000").OrderBy(x=>x.RECORDKEY).Select(x => new { x.Name, x.Grp, x.Code, x.Category }).ToList();

            if (muserid.Trim().ToUpper() != "SUPER")
            {
                mTreeList = ctxTFAT.TfatBranch.Where(x => x.Category != "Area" && x.Code != "G00000" && x.Users.Contains(muserid)).OrderBy(x => x.RECORDKEY).Select(x => new { x.Name, x.Grp, x.Code, x.Category }).ToList();
            }

                List<NFlatObject> flatObjects2 = new List<NFlatObject>();
            for (int n = 0; n < mTreeList.Count; n++)
            {
                string alias = "";
                NFlatObject abc = new NFlatObject();
                if (mTreeList[n].Category == "Zone")
                {
                    alias = mTreeList[n].Name + "-" + "Z";
                }
                else if (mTreeList[n].Category == "Branch")
                {
                    alias = mTreeList[n].Name + "-" + "B";
                }
                else if (mTreeList[n].Category == "SubBranch")
                {
                    alias = mTreeList[n].Name + "-" + "SB";
                }
                else
                {
                    alias = mTreeList[n].Name + "-" + "HO";
                }

                abc.data = alias;
                abc.Id = mTreeList[n].Code;
                if (Mode == "Add")
                {
                    if (BranchCode == abc.Id)
                    {
                        abc.isSelected = true;
                    }
                }
                else
                {
                    if (BranchArray.Contains(abc.Id))
                    {
                        abc.isSelected = true;
                    }
                }

                if (mTreeList[n].Code == mTreeList[n].Grp)
                {
                    abc.ParentId = "0";
                }
                else
                {
                    abc.ParentId = mTreeList[n].Code;
                }
                flatObjects2.Add(abc);
            }
            var recursiveObjects = FillRecursive1(flatObjects2, "0");
            string myjsonmodel = new JavaScriptSerializer().Serialize(recursiveObjects);
            ViewBag.Tree = myjsonmodel;
            return myjsonmodel;

        }
        public string CheckUncheckTree(string Check)
        {
            var mTreeList = ctxTFAT.TfatBranch.Where(x => x.Category != "Area" && x.Code != "G00000").Select(x => new { x.Name, x.Grp, x.Code, x.Category }).ToList();

            if (muserid.Trim().ToUpper() != "SUPER")
            {
                mTreeList = ctxTFAT.TfatBranch.Where(x => x.Category != "Area" && x.Code != "G00000" && x.Users.Contains(muserid)).OrderBy(x => x.RECORDKEY).Select(x => new { x.Name, x.Grp, x.Code, x.Category }).ToList();
            }


            string alias = "";
            List<NFlatObject> flatObjects2 = new List<NFlatObject>();
            for (int n = 0; n < mTreeList.Count; n++)
            {
                NFlatObject abc = new NFlatObject();
                if (mTreeList[n].Category == "Zone")
                {
                    alias = mTreeList[n].Name + "-" + "Z";
                }
                else if (mTreeList[n].Category == "Branch")
                {
                    alias = mTreeList[n].Name + "-" + "B";
                }
                else if (mTreeList[n].Category == "SubBranch")
                {
                    alias = mTreeList[n].Name + "-" + "SB";
                }
                else
                {
                    alias = mTreeList[n].Name + "-" + "HO";
                }
                abc.data = alias;
                abc.Id = mTreeList[n].Code;
                if (Check == "Check")
                {
                    abc.isSelected = true;
                }
                if (mTreeList[n].Code == mTreeList[n].Grp)
                {
                    abc.ParentId = "0";
                }
                else
                {
                    abc.ParentId = mTreeList[n].Grp;
                }
                flatObjects2.Add(abc);
            }
            var recursiveObjects = FillRecursive1(flatObjects2, "0");
            string myjsonmodel = new JavaScriptSerializer().Serialize(recursiveObjects);
            ViewBag.Tree = myjsonmodel;
            return myjsonmodel;

        }
        public static List<NRecursiveObject> FillRecursive1(List<NFlatObject> flatObjects, string parentId)
        {
            bool mSelected = false;
            List<NRecursiveObject> recursiveObjects = new List<NRecursiveObject>();
            foreach (var item in flatObjects)
            {
                mSelected = item.isSelected;
                recursiveObjects.Add(new NRecursiveObject
                {
                    data = item.data,
                    id = item.Id,
                    attr = new NFlatTreeAttribute { id = item.Id.ToString(), selected = item.isSelected },
                    //children = FillRecursive1(flatObjects, item.Id)
                });
            }
            return recursiveObjects;
        }
        #endregion


        // GET: Reports/AccountLedger
        public ActionResult Index(GridOption Model)
        {
            UpdateAuditTrail(mbranchcode, "Report", Model.Header, "", DateTime.Now, 0, "", "", "NA");
            Model.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
            Model.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
            if (Model.Document == "" || Model.Document == null)
            {
                GetAllMenu(Session["ModuleName"].ToString());
                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
                //System.Web.HttpContext.Current.Session["LastDate"].ToString();
            }
            if (Model.FromDate == null)
            {
                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            }
            Model.Branches = PopulateBranchesOnly();
            Model.Branch = mbranchcode;

            ViewBag.ViewDataId = Model.ViewDataId;
            msubcodeof = Model.ViewDataId;
            Model.AccountName = NameofAccount(Model.Document);
            Model.ViewCode = GetDefaultCode(Model.ViewDataId);
            mbasegr = Model.MainType;
            ViewBag.Header = Model.Header;
            ViewBag.Controller = Model.Controller;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;

            string[] HideColmn = new string[] { "BalanceField","Reco","Sno","Document" };
            var tfatSearch = ctxTFAT.TfatSearch.Where(x => x.Code == Model.ViewDataId).ToList();
            tfatSearch.ForEach(x => x.IsHidden = false);
            tfatSearch.Where(x => HideColmn.Contains(x.ColHead)).ToList().ForEach(x => x.IsHidden = true);
            ctxTFAT.SaveChanges();

            return View(Model);
        }

        public ActionResult GetAccountList(string term)
        {
            if (mbasegr == "CB")
            {
                if (term == "")
                {
                    return Json(GetDataTableList("Select Top 10 Code,Name from Master where (BaseGr='B' or BaseGr='C') and Hide=0 and (AppBranch like '%" + mbranchcode + "%' or AppBranch='All') Order by Name"), JsonRequestBehavior.AllowGet);
                    //var result = ctxTFAT.Master.Where(z => (z.BaseGr == "C" || z.BaseGr == "B") && z.Hide == false).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).Take(15).ToList();
                    //return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(GetDataTableList("Select Top 10 Code,Name from Master where Name like '%" + term + "%' and (BaseGr='B' or BaseGr='C') and Hide=0 and (AppBranch like '%" + mbranchcode + "%' or AppBranch='All') Order by Name"), JsonRequestBehavior.AllowGet);
                    //var result = ctxTFAT.Master.Where(z => (z.BaseGr == "C" || z.BaseGr == "B") && z.Hide == false).Where(x => x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).ToList();
                    //return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                if (term == "")
                {
                    return Json(GetDataTableList("Select Top 10 Code, Name =  isnull(ShortName,'') + ' ' + Name + ' ' + ' ('+isnull(BaseGr,'')+') ' + (case when basegr='D' or Basegr='S' then isnull(City,'') else '' end) from Master Where BaseGr<>'C' and BaseGr <> 'B' and Hide=0 "), JsonRequestBehavior.AllowGet);
                    //return Json(GetDataTableList("Select Top 15 Code,Name from Master where BaseGr<>'B' and BaseGr<>'C' and Hide=0 Order by Name"), JsonRequestBehavior.AllowGet);
                    //var result = ctxTFAT.Master.Where(x => x.BaseGr != "C" && x.BaseGr != "B" && x.Hide == false).Select(m => new { m.Code, Name = "[" + m.Code + "] " + m.ShortName + " " + m.Name + " " + m.City }).OrderBy(n => n.Name).Take(15).ToList();
                    //return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(GetDataTableList("Select Top 10 Code, Name =  isnull(ShortName,'') + ' ' + Name + ' ' + ' ('+isnull(BaseGr,'')+') ' + (Case when basegr='D' or Basegr='S' then isnull(City,'') else '' end) from Master Where BaseGr <> 'C' and BaseGr <> 'B' and Hide=0 and Charindex('" + term + "', Name)<>0 "), JsonRequestBehavior.AllowGet);
                    //return Json(GetDataTableList("Select Code,Name from Master where Name like '%" + term + "%' and BaseGr<>'B' and BaseGr<>'C' and Hide=0 Order by Name"), JsonRequestBehavior.AllowGet);
                    //var result = ctxTFAT.Master.Where(x => x.BaseGr != "C" && x.BaseGr != "B" && x.Hide == false && x.Name.Contains(term)).Select(m => new { m.Code, Name = "[" + m.Code + "] " + m.ShortName + " " + m.Name + " " + m.City }).OrderBy(n => n.Name).ToList();
                    //return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            try
            {
                #region Set Parameters

                ppara01 = "";
                ppara02 = "";
                ppara03 = "";
                ppara04 = "";
                ppara05 = "";
                ppara06 = "";
                ppara07 = "";
                ppara08 = "";
                ppara09 = "";
                ppara10 = "";
                ppara11 = "";
                ppara12 = "";
                ppara13 = "";
                ppara14 = "";
                ppara15 = "";
                ppara16 = "";
                ppara17 = "";
                ppara18 = "";
                ppara19 = "";
                ppara20 = "";
                ppara21 = "";
                ppara22 = "";
                ppara23 = "";
                ppara24 = "";

                if (!String.IsNullOrEmpty(Model.SelectContent))
                {
                    mpara = "para05" + "^" + Model.SelectContent + "~para02^'Yes'~";

                    ppara05 = Model.SelectContent;
                }

                #endregion

                if (Model.Code == null || Model.Code == "") return null;
                if (Model.Date == ":")
                {
                    Model.Date = System.Web.HttpContext.Current.Session["StartDate"].ToString() + ":" + DateTime.Today.ToShortDateString();
                }
                if (IsValidDate(Model.Date) == false)
                {
                    return Json(new { Message = "Invalid Date..", Status = "Error" }, JsonRequestBehavior.AllowGet);
                }
                var date = Model.Date.Replace("-", "/").Split(':');
                Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                decimal? OpeningBal = 0;
                var SDate = Convert.ToDateTime(Model.FromDate);
                var LDate = Convert.ToDateTime(Model.ToDate);
                int mlocation = Convert.ToInt32(ppara01 == null || ppara01 == "" ? "0" : ppara01);
                //string mpal = (ppara02 == null || ppara02 == "" ? "no" : ppara02).ToLower();
                //string mauth = (ppara04 == null || ppara04 == "" ? "no" : ppara04).ToLower();
                bool mpal = ppara02 == null || ppara02 == "" ? false : true;
                bool mauth = ppara04 == null || ppara04 == "" ? false : true;
                bool mmvpv = false;
                string mbranch = ppara05 == null || ppara05 == "" ? "'" + mbranchcode + "'" : ppara05;
                //if (mlocation != 0)
                //{
                mpal = true;
                OpeningBal = GetBalance(Model.Code, SDate.AddDays(-1), mbranch, mlocation, mpal, mauth, mmvpv);
                //(from L in ctxTFAT.Ledger
                //          where L.MainType != "MV" && L.MainType != "PV" && L.Code == Model.Code && L.DocDate < SDate.Date && L.Branch == mbranchcode && L.LocationCode == mlocation && (L.AUTHORISE.StartsWith("A") || mauth == "yes" ? L.AUTHORISE.StartsWith("N") : true) && (mpal != "no" ? true : L.Type.StartsWith("PAL") == false) && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                //          select L.Debit - L.Credit).DefaultIfEmpty(0).Sum();
                //}
                //else
                //{
                //OpeningBal = (from L in ctxTFAT.Ledger
                //where L.MainType != "MV" && L.MainType != "PV" && L.Code == Model.Code && L.DocDate < SDate.Date && L.Branch == mbranchcode && (L.AUTHORISE.StartsWith("A") || mauth == "yes" ? L.AUTHORISE.StartsWith("N") : true) && (mpal != "no" ? true : L.Type.StartsWith("PAL") == false) && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                //select L.Debit - L.Credit).DefaultIfEmpty(0).Sum();
                //}
                mOpeningBalance = OpeningBal != null ? OpeningBal.Value : 0;
                string mOpening = Math.Abs(mOpeningBalance) + (mOpeningBalance > 0 ? " Dr" : " Cr");
                //string.Format("{0,0:N2}", Math.Abs(mOpeningBalance)) + (mOpeningBalance > 0 ? " Dr" : " Cr");
                decimal? ClosingBal = 0;
                //if (mlocation != 0)
                //{
                //ClosingBal = (from L in ctxTFAT.Ledger
                //where L.MainType != "MV" && L.MainType != "PV" && L.Code == Model.Code && L.DocDate <= LDate.Date && L.Branch == mbranchcode && L.LocationCode == mlocation && (L.AUTHORISE.StartsWith("A") || mauth == "yes" ? L.AUTHORISE.StartsWith("N") : true) && (mpal != "no" ? true : L.Type.StartsWith("PAL") == false) && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                //select L.Debit - L.Credit).DefaultIfEmpty(0).Sum();
                //}
                //else
                //{
                ClosingBal = GetBalance(Model.Code, LDate.Date, mbranch, mlocation, false, mauth, mmvpv);

                //(from L in ctxTFAT.Ledger
                //where L.MainType != "MV" && L.MainType != "PV" && L.Code == Model.Code && L.DocDate <= LDate.Date && L.Branch == mbranchcode && (L.AUTHORISE.StartsWith("A") || mauth == "yes" ? L.AUTHORISE.StartsWith("N") : true) && (mpal != "no" ? true : L.Type.StartsWith("PAL") == false) && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                //select L.Debit - L.Credit).DefaultIfEmpty(0).Sum();
                //}
                ClosingBal = ClosingBal ?? 0;
                string mClosing = Math.Abs((decimal)ClosingBal) + (ClosingBal > 0 ? " Dr" : " Cr");






                //string.Format("{0,0:N2}", Math.Abs((decimal)ClosingBal)) + (ClosingBal > 0 ? " Dr" : " Cr");
                decimal? TCredit = GetBalanceDateRange(Model.Code, SDate.Date, LDate.Date, "C", mbranch, mlocation, false, mauth, mmvpv);
                //if (mlocation != 0)
                //{
                //    TCredit = (from L in ctxTFAT.Ledger
                //               where L.MainType != "MV" && L.MainType != "PV" && L.Code == Model.Code && L.DocDate >= SDate.Date && L.DocDate <= LDate.Date && L.Branch == mbranchcode && L.LocationCode == mlocation && (L.AUTHORISE.StartsWith("A") || mauth == "yes" ? L.AUTHORISE.StartsWith("N") : true) && (mpal != "no" ? true : L.Type.StartsWith("PAL") == false) && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                //               select L.Credit).DefaultIfEmpty(0).Sum();
                //}
                //else
                //{
                //    TCredit = (from L in ctxTFAT.Ledger
                //               where L.MainType != "MV" && L.MainType != "PV" && L.Code == Model.Code && L.DocDate >= SDate.Date && L.DocDate <= LDate.Date && L.Branch == mbranchcode && (L.AUTHORISE.StartsWith("A") || mauth == "yes" ? L.AUTHORISE.StartsWith("N") : true) && (mpal != "no" ? true : L.Type.StartsWith("PAL") == false) && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                //               select L.Credit).DefaultIfEmpty(0).Sum();
                //}
                string mCredit = string.Format("{0,0:N2}", TCredit != null ? TCredit : 0);
                decimal? TDebit = GetBalanceDateRange(Model.Code, SDate.Date, LDate.Date, "D", mbranch, mlocation, false, mauth, mmvpv);

                #region Calculate Opening In Acoount By Suresh 02.11.2021

                decimal TotalDebit = TDebit??0, TotalCredit = TCredit??0, FinalClosing = 0;
                if (mOpeningBalance > 0)
                {
                    TotalDebit += mOpeningBalance ;
                }
                else
                {
                    TotalCredit += (mOpeningBalance)*(-1) ;
                }

                FinalClosing = TotalDebit - TotalCredit;

                mClosing = Math.Abs((decimal)FinalClosing) + (FinalClosing > 0 ? " Dr" : " Cr");
                #endregion

                //if (mlocation != 0)
                //{
                //    TDebit = (from L in ctxTFAT.Ledger
                //              where L.MainType != "MV" && L.MainType != "PV" && L.Code == Model.Code && L.DocDate >= SDate.Date && L.DocDate <= LDate.Date && L.Branch == mbranchcode && L.LocationCode == mlocation && L.AUTHORISE.StartsWith("A") && (mpal != "no" ? true : L.Type.StartsWith("PAL") == false) && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                //              select L.Debit).DefaultIfEmpty(0).Sum();
                //}
                //else
                //{
                //    TDebit = (from L in ctxTFAT.Ledger
                //              where L.MainType != "MV" && L.MainType != "PV" && L.Code == Model.Code && L.DocDate >= SDate.Date && L.DocDate <= LDate.Date && L.Branch == mbranchcode && (L.AUTHORISE.StartsWith("A") || mauth == "yes" ? L.AUTHORISE.StartsWith("N") : true) && (mpal != "no" ? true : L.Type.StartsWith("PAL") == false) && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(L.Type))
                //              select L.Debit).DefaultIfEmpty(0).Sum();
                //}
                string mDebit = string.Format("{0,0:N2}", TDebit != null ? TDebit : 0);
                //IReportGridOperation mIlst = new ListViewGridOperationreport();
                //string mstr = "";
                //var mas = ctxTFAT.Master.Where(z => z.Code == Model.Code).Select(x => new { x.Name, x.City, x.BaseGr, x.Grp }).FirstOrDefault();
                //mstr += mas.Name + " " + mas.City.Trim() + "|";
                //mstr += (ctxTFAT.MasterGroups.Where(z => z.Code == mas.Grp).Select(x => x.Name).FirstOrDefault() ?? "") + "|";
                //var mvar = ctxTFAT.Address.Where(z => z.Code == Model.Code).Select(x => x).FirstOrDefault();
                //if (mvar != null)
                //{
                //    mstr += (mvar.Person ?? "") + "|";
                //    mstr += ((mvar.Adrl1 ?? "") + " " + (mvar.Adrl2 ?? "") + (mvar.Adrl3 ?? "") + " " + (mvar.Adrl4 ?? "") + ", " + (mvar.City ?? "") + " " + (mvar.State ?? "") + " " + (mvar.Country ?? "")) + "|";
                //    mstr += (mvar.GSTNo ?? "") + "|";
                //    mstr += (mvar.Mobile ?? "") + "|";
                //    mstr += (mvar.Email ?? "") + "|";
                //}
                //else
                //{
                //    mstr += "" + "|";
                //    mstr += "" + "|";
                //    mstr += "" + "|";
                //    mstr += "" + "|";
                //    mstr += "" + "|";
                //}
                //var mvar2 = ctxTFAT.MasterInfo.Where(z => z.Code == Model.Code).Select(x => x).FirstOrDefault();
                //if (mvar2 != null)
                //{
                //    mstr += (mvar2.CrLimit ?? 0) + "|";
                //    mstr += (mvar2.CrPeriod ?? 0) + "|";
                //    mstr += (mvar2.Rank ?? 0) + "|";
                //}
                //else
                //{
                //    mstr += 0 + "|";
                //    mstr += 0 + "|";
                //    mstr += 0 + "|";
                //}
                string mstr = "";
                DataTable mas = GetDataTable("Select Name, City, BaseGr, Grp from Master Where Code = '" + Model.Code + "'");
                //var mas = ctxooroo.Master.Where(z => z.Code == Model.Code).Select(x => new { x.Name, x.City, x.BaseGr, x.Grp }).FirstOrDefault();
                mstr += mas.Rows[0]["Name"] + " " + mas.Rows[0]["City"].ToString().Trim() + " | ";
                mstr += (Fieldoftable("MasterGroups", "Name", "Code = '" + mas.Rows[0]["Grp"].ToString() + "'") ?? "") + " |";
                //var mvar = ctxooroo.Address.Where(z => z.Code == Model.Code).Select(x => x).FirstOrDefault();
                DataTable mvar = GetDataTable("Select * from Address Where Code = '" + Model.Code + "'");
                if (mvar.Rows.Count > 0)
                {
                    mstr += (mvar.Rows[0]["Person"] ?? "") + " | ";
                    mstr += (mvar.Rows[0]["Adrl1"] ?? "") + " " + (mvar.Rows[0]["Adrl2"] ?? "") + (mvar.Rows[0]["Adrl3"] ?? "") + " " + (mvar.Rows[0]["Adrl4"] ?? "") + ", " + (mvar.Rows[0]["City"] ?? "") + " " + (mvar.Rows[0]["State"] ?? "") + " " + (mvar.Rows[0]["Country"] ?? "") + " | ";
                    mstr += (mvar.Rows[0]["GSTNo"] ?? "") + " | ";
                    mstr += (mvar.Rows[0]["Mobile"] ?? "") + " | ";
                    mstr += (mvar.Rows[0]["Email"] ?? "") + " | ";
                    //mstr += (mvar.Person ?? "") + "|";
                    //mstr += ((mvar.Adrl1 ?? "") + " " + (mvar.Adrl2 ?? "") + (mvar.Adrl3 ?? "") + " " + (mvar.Adrl4 ?? "") + ", " + (mvar.City ?? "") + " " + (mvar.State ?? "") + " " + (mvar.Country ?? "")) + "|";
                    //mstr += (mvar.GSTNo ?? "") + "|";
                    //mstr += (mvar.Mobile ?? "") + "|";
                    //mstr += (mvar.Email ?? "") + "|";
                }
                else
                {
                    mstr += "" + "|";
                    mstr += "" + "|";
                    mstr += "" + "|";
                    mstr += "" + "|";
                    mstr += "" + "|";
                }
                //var mvar2 = ctxooroo.MasterInfo.Where(z => z.Code == Model.Code).Select(x => x).FirstOrDefault();
                DataTable mvar2 = GetDataTable("Select * from MasterInfo Where Code = '" + Model.Code + "'");
                if (mvar2.Rows.Count > 0)
                {
                    mstr += (mvar2.Rows[0]["CrLimit"] ?? 0) + "|";
                    mstr += (mvar2.Rows[0]["CrPeriod"] ?? 0) + " | ";
                    mstr += (mvar2.Rows[0]["Rank"] ?? 0) + " | ";
                }
                else
                {
                    mstr += 0 + "|";
                    mstr += 0 + "|";
                    mstr += 0 + "|";
                }

                string mquery = "Select FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4)) as MonthName, Sum(Debit-Credit) as Amt from Ledger where Code='" + Model.Code + "' and Branch in (" + mbranch + ") and docdate>='%RepStartDate' and docdate<='%RepEndDate' " + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(Type,'" + mDocString + "')=0") + " Group by Code,FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4))";
                string[] mArr = GetMonthlyBalance(mquery);
                mstr += string.Join(",", mArr) + "|";
                if (mas.Rows[0]["BaseGr"].ToString() == "D")
                //if (mas.BaseGr == "D")
                {
                    mquery = "Select FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4)) as MonthName, Sum(Amt) as Amt from Sales where Code='" + Model.Code + "' and Branch in (" + mbranch + ") and docdate>='%RepStartDate' and docdate<='%RepEndDate'" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(Type,'" + mDocString + "')=0") + " Group by Code,FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4))";
                    mArr = GetMonthlyBalance(mquery);
                    mstr += string.Join(",", mArr) + "|";
                    mquery = "Select FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4)) as MonthName, Sum(Credit) as Amt from Ledger where MainType='RC' and Credit>0 and Code='" + Model.Code + "' and Branch in (" + mbranch + ") and Docdate>='%RepStartDate' and Docdate<='%RepEndDate'" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(Type,'" + mDocString + "')= 0") + " Group by Code,FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4))";
                    mArr = GetMonthlyBalance(mquery);
                    mstr += string.Join(",", mArr) + "|";
                }
                else if (mas.Rows[0]["BaseGr"].ToString() == "S")
                //else if (mas.BaseGr == "S")
                {
                    mquery = "Select FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4)) as MonthName, Sum(Amt) as Amt from Purchase where Code='" + Model.Code + "' and Branch in (" + mbranch + ") and docdate>='%RepStartDate' and docdate<='%RepEndDate'" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(Type,'" + mDocString + "')=0") + " Group by Code,FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4))";
                    mArr = GetMonthlyBalance(mquery);
                    mstr += string.Join(",", mArr) + "|";
                    mquery = "Select FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4)) as MonthName, Sum(Debit) as Amt from Ledger where MainType='PM' and Debit>0 and Code='" + Model.Code + "' and Branch in (" + mbranch + ") and Docdate>='%RepStartDate' and Docdate<='%RepEndDate'" + (string.IsNullOrEmpty(mDocString) ? "" : " And CharIndex(Type,'" + mDocString + "')=0") + " Group by Code,FORMAT(DocDate, 'MMM') + ' ' + cast(year(docdate) as varchar(4))";
                    mArr = GetMonthlyBalance(mquery);
                    mstr += string.Join(",", mArr) + "|";
                }
                else // for General Ledger a/cs
                {
                    mstr += "" + "|";
                    mstr += "" + "|";
                }
                return GetGridDataColumns(Model.ViewDataId, "X", "", mOpening + "|" + mClosing + "|" + mCredit + "|" + mDebit, mstr, GetMonthString());
            }
            catch (Exception ex)
            {
                return Json(new { Status = "Error", Message = "Error! While Generating Report..\n" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetGridData(GridOption Model)
        {
            if (Model.Code == null || Model.Code == "") return null;
            //ppara05 = ppara05 == null || ppara05 == "" ? "'" + mbranchcode + "'" : ppara05;
            return GetGridReport(Model, "R", "Code^" + Model.Code + (mpara != "" ? "~" + mpara : ""), true, mOpeningBalance);
        }

        public ActionResult GetExcel(GridOption Model)
        {
            if (Model.SelectContent == null || Model.SelectContent == "") return null;
            Model.mWhat = "XLS";
            return GetGridReport(Model, "R", "Code^" + Model.SelectContent + (mpara != "" ? "~" + mpara : ""), true, mOpeningBalance, "", "A4");
        }

        #region subreport
        [HttpPost]
        public ActionResult GetGridStructureSales(GridOption Model)
        {

            #region Set Parameters

            ppara01 = "";
            ppara02 = "";
            ppara03 = "";
            ppara04 = "";
            ppara05 = "";
            ppara06 = "";
            ppara07 = "";
            ppara08 = "";
            ppara09 = "";
            ppara10 = "";
            ppara11 = "";
            ppara12 = "";
            ppara13 = "";
            ppara14 = "";
            ppara15 = "";
            ppara16 = "";
            ppara17 = "";
            ppara18 = "";
            ppara19 = "";
            ppara20 = "";
            ppara21 = "";
            ppara22 = "";
            ppara23 = "";
            ppara24 = "";

            if (!String.IsNullOrEmpty(Model.SelectContent))
            {
                mpara = "para05" + "^" + Model.SelectContent + "~";

                ppara05 = Model.SelectContent;
                Model.Branch = Model.SelectContent;
            }

            #endregion


            //LedSalesRegister,LedSalesOrders,LedSalesStockReg,LedOSAgeing,LedFollowupRegister
            //LedPurchRegister,LedPurchOrders,LedPurchStockReg,LedOSAgeing,MthlyAccSummary
            //IReportGridOperation mIlst = new ListViewGridOperationreport();
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        [HttpPost]
        public ActionResult GetGridDataSales(GridOption Model)
        {
            if (Model.Code == null || Model.Code == "") return null;
            if (Model.ViewDataId == "LedOSAgeing" || Model.ViewDataId == "LedgerDailySummary")
            {
                //DataTable dtreport = new DataTable();
                SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                SqlCommand cmd = new SqlCommand("SPTFAT_" + Model.ViewDataId, tfat_conx)
                {
                    CommandType = CommandType.StoredProcedure
                };
                var date = Model.Date.Replace("-", "/").Split(':');
                Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                cmd.Parameters.Add("@mCode", SqlDbType.VarChar).Value = Model.Code ?? "";
                if (Model.ViewDataId == "LedOSAgeing")
                {
                    cmd.Parameters.Add("@mDate", SqlDbType.VarChar).Value = Model.ToDate;
                }
                else
                {
                    cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = Model.Branch ?? mbranchcode;
                    cmd.Parameters.Add("@mStartDate", SqlDbType.Date).Value = Model.FromDate;
                    cmd.Parameters.Add("@mEndDate", SqlDbType.Date).Value = Model.ToDate;
                }
                cmd.Parameters.Add("@mDocString", SqlDbType.VarChar).Value = mDocString;
                tfat_conx.Open();
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                tfat_conx.Close();
            }
            else if (Model.ViewDataId == "LedgerMonthlySummary")
            {
                //DataTable dtreport = new DataTable();
                ExecuteStoredProc("Drop Table ztmp_TempMth");
                SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                SqlCommand cmd = new SqlCommand("SPTFAT_LedgerMthSummary", tfat_conx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;
                cmd.Parameters.Add("@mCode", SqlDbType.VarChar).Value = Model.Code ?? "";
                cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = Model.Branch ?? mbranchcode;
                cmd.Parameters.Add("@mStartDate", SqlDbType.Date).Value = Convert.ToDateTime(System.Web.HttpContext.Current.Session["StartDate"].ToString());
                cmd.Parameters.Add("@mDocString", SqlDbType.VarChar).Value = mDocString;
                tfat_conx.Open();
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
                tfat_conx.Close();
            }
            return GetGridReport(Model, "R", "Document^" + (Model.Document ?? "") + "~Code^" + (Model.Code ?? ""), false, 0);
        }
        #endregion subreport



        [HttpPost]
        public ActionResult SetBranchParameters(GridOption Model)
        {
            ppara01 = "";
            ppara02 = "";
            ppara03 = "";
            ppara04 = "";
            ppara05 = "";
            ppara06 = "";
            ppara07 = "";
            ppara08 = "";
            ppara09 = "";
            ppara10 = "";
            ppara11 = "";
            ppara12 = "";
            ppara13 = "";
            ppara14 = "";
            ppara15 = "";
            ppara16 = "";
            ppara17 = "";
            ppara18 = "";
            ppara19 = "";
            ppara20 = "";
            ppara21 = "";
            ppara22 = "";
            ppara23 = "";
            ppara24 = "";

            if (!String.IsNullOrEmpty(Model.SelectContent))
            {
                mpara = "para05" + "^" + Model.SelectContent + "~";

                ppara05 = Model.SelectContent;
            }
            
            if (Model.IsFormatSelected == true)
            {
                ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=0 Where SubCodeOf='" + Model.OptionCode + "'");
                ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=-1 Where Code='" + Model.ViewDataId + "'");
            }
            return Json(new
            {
                Status = "Success",
                Message = ""
            }, JsonRequestBehavior.AllowGet);
        }
    }
}