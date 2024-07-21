using ALT_ERP3.Controllers;
using CrystalDecisions.CrystalReports.Engine;
using EntitiModel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class MultiSelectController : BaseController
    {
        private static string mmaintype = "";
        private static string msubtype = "";
        private static string mtype = "";
        private static string mtable = "";

        public ActionResult GetAccountGrps(string term)
        {
            if (term == "")
            {
                List<SelectListItem> items = new List<SelectListItem>();
                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    string query = "select k.Code as Code,k.Name as Name from mastergroups k where  k.Code not in (select kk.grp from MasterGroups kk) order by k.Name ";
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
                var Modified = items.Select(x => new
                {
                    Code = x.Value,
                    Name = x.Text
                });
                //var result = ctxTFAT.MasterGroups.Where(x => x.Hide == false /*&& x.IsLast== true*/ && x.Code != x.Grp && x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).Take(10).ToList();
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<SelectListItem> items = new List<SelectListItem>();
                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    string query = "select k.Code as Code,k.Name as Name from mastergroups k where K.Name like '%" + term + "%' and  k.Code not in (select kk.grp from MasterGroups kk) order by k.Name ";
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

                var Modified = items.Select(x => new
                {
                    Code = x.Value,
                    Name = x.Text
                });
                //var result = ctxTFAT.MasterGroups.Where(x => x.Hide == false /*&& x.IsLast == true*/ && x.Code != x.Grp && x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).OrderBy(n => n.Name).ToList();
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetPrintFormats(string term)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.DocFormats.Where(x => x.Type == mtype).Select(m => new
                {
                    Code = m.FormatCode,
                    Name = m.FormatCode
                }).OrderBy(n => n.Name).ToList().Take(10);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.DocFormats.Where(x => x.Type == mtype && x.FormatCode.ToLower().Trim().Contains(term.ToLower().Trim())).Select(m => new
                {
                    Code = m.FormatCode,
                    Name = m.FormatCode
                }).OrderBy(n => n.Name).ToList().Take(10);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        private List<SelectListItem> PopulateBranchesOnly()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM TfatBranch where Category<>'Area' and BranchType='G' order by Recordkey ";
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
            return items;
        }

        private List<SelectListItem> PopulateAccountGrps()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "select k.Code as Code,k.Name as Name from mastergroups k where  k.Code not in (select kk.grp from MasterGroups kk) order by k.Name";
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
            return items;
        }

        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }

            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, "Report", Model.Header, "", DateTime.Now, 0, "", "", "NA");
            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            Model.BaseGr = Model.MainType;
            Model.Type = ctxTFAT.DocFormats.Where(x => x.FormatCode == Model.ViewDataId).Select(x => x.Type).FirstOrDefault();
            Model.Value1 = "1";
            if (Model.ViewDataId == "Invoice")
            {
                TfatSearch tfatSearch = ctxTFAT.TfatSearch.Where(x => x.ColHead.Contains("Select") && x.Code == Model.ViewDataId).FirstOrDefault();
                tfatSearch.IsHidden = false;
                ctxTFAT.Entry(tfatSearch).State = EntityState.Modified;
                ctxTFAT.SaveChanges();
            }

            mtype = Model.Type;
            if (Model.MainType == null)
            {
                Model.MainType = ctxTFAT.DocTypes.Where(z => z.Code == Model.Type).Select(x => x.MainType).FirstOrDefault();
            }
            Model.SubType = Model.SubType != null ? Model.SubType : ctxTFAT.DocTypes.Where(z => z.Code == Model.Type).Select(x => x.SubType).FirstOrDefault();
            msubtype = Model.SubType;
            Model.ViewCode = Model.ViewDataId;
            ViewBag.ViewDataId = Model.ViewDataId;
            ViewBag.PeriodType = ctxTFAT.TfatMenu.Where(z => z.OptionCode == Model.OptionCode).Select(x => x.PeriodType).FirstOrDefault() ?? "X";
            ViewBag.Header = Model.Header;
            ViewBag.Controller = Model.Controller;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;
            mtable = Model.TableName;
            Model.Branches = PopulateBranchesOnly();
            Model.Accounts = PopulateAccountGrps();
            Model.Branch = mbranchcode;
            return View(Model);
        }

        public ActionResult BulkPrint(GridOption Model)
        {
            if (Model.Date != null)
            {
                Model.Date = Model.Date.Replace("undefined:", "01-01-2000:");
                var date = Model.Date.Replace("-", "/").Split(':');
                Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                Model.ToDate = Convert.ToDateTime(date[1]).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            }
            else
            {
                Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
                Model.FromDate = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                Model.ToDate = (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            }
            var SDate = Convert.ToDateTime(Model.FromDate);
            switch (msubtype)
            {
                case "RE":  // payment reminder
                    Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                    Model.FromDate = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    Model.ViewDataId = "PaymentReminder";// "PaymentReminder";
                    break;
                case "ML":  // multi-ledger printing
                    Model.ViewDataId = "AccountLedgerPrintData";
                    break;
                case "BC":  // balance confirmation
                    Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                    Model.FromDate = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    Model.ViewDataId = "BalanceConfirmation";
                    break;
            }

            string mParentKey = "";
            string mcode = "";
            int msno = 0;
            Document document = new Document(PageSize.A4, 10, 10, 10, 10);
            var ms = new MemoryStream();
            {
                PdfCopy pdf = new PdfCopy(document, ms);
                document.Open();
                pdf.Open();
                List<string> mSrls = Model.SelectContent.Split('^').ToList();
                var docFormats = ctxTFAT.DocFormats.Where(x => x.FormatCode.Trim() == Model.Format.Trim()).Select(x => x).FirstOrDefault();

                foreach (var msrl in mSrls.OrderByDescending(x => x).ToList())//Model.list)
                {
                    //mcode = msrl.Code;
                    mcode = msrl;
                    if (string.IsNullOrEmpty(mcode) == false)
                    {
                        if (msubtype == "RE" || msubtype == "BC" || msubtype == "ML")
                        {
                            DataTable dtreport = new DataTable();
                            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                            SqlCommand cmd = new SqlCommand("SPREP_" + Model.ViewDataId, tfat_conx); //name of the storedprocedure
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandTimeout = 120;
                            cmd.Parameters.Add("@mCode", SqlDbType.VarChar).Value = mcode;
                            cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
                            cmd.Parameters.Add("@mDate1", SqlDbType.Date).Value = Model.FromDate;
                            cmd.Parameters.Add("@mDate2", SqlDbType.Date).Value = Model.ToDate;
                            cmd.Parameters.Add("@mLocationCode", SqlDbType.Int).Value = mlocationcode;
                            SqlDataAdapter adp = new SqlDataAdapter(cmd);
                            adp.Fill(dtreport);

                            ReportDocument rd = new ReportDocument();
                            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "REP_" + Model.ViewDataId + ".rpt"));
                            rd.SetDataSource(dtreport);
                            try
                            {
                                Stream mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                                switch (Model.mWhat)
                                {
                                    case "PDF":
                                        mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                                        break;
                                    case "XLS":
                                        mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.Excel);
                                        break;
                                    case "WORD":
                                        mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.WordForWindows);
                                        break;
                                    case "CSV":
                                        mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.CharacterSeparatedValues);
                                        break;
                                }
                                mstream.Seek(0, SeekOrigin.Begin);

                                Warning[] warnings;
                                string[] streamids;
                                string mimeType;
                                string encoding;
                                string extension;
                                MemoryStream memory1 = new MemoryStream();
                                mstream.CopyTo(memory1);
                                byte[] bytes = memory1.ToArray();
                                MemoryStream memoryStream = new MemoryStream(bytes);
                                PdfReader imageDocumentReader = new PdfReader(memoryStream.ToArray());
                                int ab = imageDocumentReader.NumberOfPages;
                                for (int a = 1; a <= ab; a++)
                                {
                                    var page = pdf.GetImportedPage(imageDocumentReader, a);
                                    pdf.AddPage(page);
                                }
                                imageDocumentReader.Close();
                            }
                            catch
                            {
                                rd.Close();
                                rd.Dispose();
                                throw;
                            }
                            finally
                            {
                                rd.Close();
                                rd.Dispose();
                            }
                        }
                        else
                        {
                            if (docFormats != null)
                            {
                                int NoOfCopies = 1;
                                if (!(String.IsNullOrEmpty(Model.Value1)))
                                {
                                    NoOfCopies = Convert.ToInt32(Model.Value1);
                                }
                                mParentKey = mcode.Substring(6, mcode.Length - 6);
                                Model.Branch = mcode.Substring(0, 6);
                                for (int i = 0; i < NoOfCopies; i++)
                                {
                                    Model.StoreProcedure = docFormats.StoredProc;
                                    DataTable dtreport = new DataTable();
                                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                                    SqlCommand cmd = new SqlCommand("SPDoc_" + Model.StoreProcedure, tfat_conx); //name of the storedprocedure
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.CommandTimeout = 120;
                                    cmd.Parameters.AddWithValue("@mParentKey", mParentKey);
                                    cmd.Parameters.AddWithValue("@mBranch", Model.Branch);
                                    SqlDataAdapter adp = new SqlDataAdapter(cmd);
                                    adp.Fill(dtreport);

                                    ReportDocument rd = new ReportDocument();
                                    rd.Load(Path.Combine(Server.MapPath("~/Reports"), docFormats.FormatCode + ".rpt"));
                                    rd.SetDataSource(dtreport);
                                    //rd.PrintToPrinter(1, true, 0, 0);
                                    try
                                    {
                                        Stream mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                                        switch (Model.mWhat)
                                        {
                                            case "PDF":
                                                mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                                                break;
                                            case "XLS":
                                                mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.Excel);
                                                break;
                                            case "WORD":
                                                mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.WordForWindows);
                                                break;
                                            case "CSV":
                                                mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.CharacterSeparatedValues);
                                                break;
                                        }
                                        mstream.Seek(0, SeekOrigin.Begin);
                                        Warning[] warnings;
                                        string[] streamids;
                                        string mimeType;
                                        string encoding;
                                        string extension;
                                        MemoryStream memory1 = new MemoryStream();
                                        mstream.CopyTo(memory1);
                                        byte[] bytes = memory1.ToArray();
                                        MemoryStream memoryStream = new MemoryStream(bytes);
                                        PdfReader imageDocumentReader = new PdfReader(memoryStream.ToArray());
                                        int ab = imageDocumentReader.NumberOfPages;
                                        for (int a = 1; a <= ab; a++)
                                        {
                                            var page = pdf.GetImportedPage(imageDocumentReader, a);
                                            pdf.AddPage(page);
                                        }
                                        imageDocumentReader.Close();
                                    }
                                    catch
                                    {
                                        rd.Close();
                                        rd.Dispose();
                                        throw;
                                    }
                                    finally
                                    {
                                        rd.Close();
                                        rd.Dispose();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            try
            {
                document.Close();
            }
            catch (Exception e)
            {

            }
            return File(ms.ToArray(), "application/" + Model.mWhat);
        }

        [HttpPost]
        public ActionResult GetGridStructureML(GridOption Model)
        {
            string mstr = "";
            if (Model.list != null)
            {
                mstr = "" + string.Join(",", Model.list.Select(x => x.Code).ToList());
            }

            string Str = "";


            //if (Model.list == null) return null;
            //string mstr = "^";
            //foreach (var m in Model.list)
            //{
            //    if (!String.IsNullOrEmpty(m.Code))
            //    {
            //        mstr += m.Code + "^";
            //    }
            //}
            ExecuteStoredProc("Delete From LedgerPrintData");
            if (Model.Date != null)
            {
                Model.Date = Model.Date.Replace("undefined:", "01-01-2000:");
                var date = Model.Date.Replace("-", "/").Split(':');
                Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                Model.ToDate = Convert.ToDateTime(date[1]).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            }
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("SPTFAT_AccountLedgerPrintMulti", tfat_conx); //name of the storedprocedure
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@mCode", SqlDbType.VarChar).Value = mstr;// "^" + Model.SelectContent;
            cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = String.IsNullOrEmpty(Model.Branch) == true ? mbranchcode : Model.Branch;
            cmd.Parameters.Add("@mDate1", SqlDbType.Date).Value = Model.FromDate;
            cmd.Parameters.Add("@mDate2", SqlDbType.Date).Value = Model.ToDate;
            cmd.Parameters.Add("@mLocationCode", SqlDbType.Int).Value = 0;
            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();
            tfat_conx.Close();
            cmd.Dispose();
            tfat_conx.Dispose();
            decimal ClosingBal = 0, mOpeningBalance = 0;
            var SDate = Convert.ToDateTime(Model.FromDate);
            var LDate = Convert.ToDateTime(Model.ToDate);
            mOpeningBalance = GetMulAccountBalance(mstr, SDate.AddDays(-1), String.IsNullOrEmpty(Model.Branch) == true ? mbranchcode : Model.Branch, 0, true, false, false);
            ClosingBal = GetMulAccountBalance(mstr, LDate.Date, String.IsNullOrEmpty(Model.Branch) == true ? mbranchcode : Model.Branch, 0, true, false, false);

            decimal? TCredit = GetMulBalanceDateRange(mstr, SDate.Date, LDate.Date, "C", String.IsNullOrEmpty(Model.Branch) == true ? mbranchcode : Model.Branch, 0, true, false, false);
            decimal? TDebit = GetMulBalanceDateRange(mstr, SDate.Date, LDate.Date, "D", String.IsNullOrEmpty(Model.Branch) == true ? mbranchcode : Model.Branch, 0, true, false, false);

            string mOpening = Math.Abs(Math.Round(mOpeningBalance, 2)) + (mOpeningBalance > 0 ? " Dr" : " Cr");
            string mClosing = Math.Abs(Math.Round((decimal)ClosingBal, 2)) + (ClosingBal > 0 ? " Dr" : " Cr");
            string mDebit = string.Format("{0,0:N2}", TDebit != null ? TDebit : 0);
            string mCredit = string.Format("{0,0:N2}", TCredit != null ? TCredit : 0);

            return GetGridDataColumns(Model.ViewDataId, "X", "", mOpening + "|" + mClosing + "|" + mDebit + "|" + mCredit);
        }

        [HttpPost]
        public ActionResult GetGridDataML(GridOption Model)
        {
            if (String.IsNullOrEmpty(Model.Branch))
            {
                Model.Branch = "'" + mbranchcode + "'";
                mpara = "para01" + "^" + Model.Branch + "~";
            }
            else
            {
                mpara = "para01" + "^" + Model.Branch + "~";
            }
            if (Model.Supress)
            {
                mpara = "para02" + "^ (Debit+Credit)<>0~";
                mpara = "para02" + "^Code in (select Distinct Code from LedgerPrintData where (Debit+Credit)<>0)~";
            }
            else
            {
                mpara = "para02" + "^1=1~";
            }

            return GetGridReport(Model, "X", "BaseGr^" + Model.MainType + "~SubType^" + Model.SubType + "~Type^" + Model.Type + (mpara != "" ? "~" + mpara : ""));
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            mpara = "";
            if (String.IsNullOrEmpty(Model.Branch))
            {
                Model.Branch = "'" + mbranchcode + "'";
                mpara = "para01" + "^" + Model.Branch + "~";
            }
            else
            {
                mpara = "para01" + "^" + Model.Branch + "~";
            }
            if (!String.IsNullOrEmpty(Model.TempType))
            {
                mpara += "para02" + "^Charindex(Master.Grp,'" + Model.TempType + "')<>0~";
            }
            else
            {
                mpara += "para02" + "^1=1~";
            }

            return GetGridReport(Model, "R", "BaseGr^" + Model.MainType + "~SubType^" + Model.SubType + "~Type^" + Model.Type + "~Type^" + Model.Type + (mpara != "" ? "~" + mpara : ""));
        }

        public ActionResult GetExcel(GridOption Model)
        {
            mpara = "";
            Model.mWhat = "XLS";
            mpara = "para02" + "^1=1~";
            return GetGridReport(Model, "R", mpara, false, 0);
        }

        #region subreport
        [HttpPost]
        public ActionResult GetGridStructureSub(GridOption Model)
        {
            string msubgrid = "";
            msubgrid = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).Select(z => z.DrillQuery).FirstOrDefault() ?? "";
            if (msubgrid.Trim() == "")
            {
                return Json(new { Status = "Error", Message = "Sub-Grid format not found.." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (msubgrid.Contains("~")) // name of the column to take else Document column e.g.SubGrid-OrdersExecuted~TableKey
                {
                    msubgrid = msubgrid.Substring(0, msubgrid.LastIndexOf("~"));
                }
                string mstr = string.Join(",", GetMonthlyBalance("", Model.Document));
                return GetGridDataColumns(msubgrid, "X", "", mstr);
            }
        }

        [HttpPost]
        public ActionResult GetGridDataSub(GridOption Model)
        {
            string msubgrid = "";
            msubgrid = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).Select(z => z.DrillQuery).FirstOrDefault() ?? "";
            if (msubgrid.Trim() == "")
            {
                return Json(new { Status = "Error", Message = "Sub-Grid format not found.." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (msubgrid.Contains("~")) // name of the column to take else Document column e.g.SubGrid-OrdersExecuted~TableKey
                {
                    msubgrid = msubgrid.Substring(0, msubgrid.LastIndexOf("~"));
                }
                Model.ViewDataId = msubgrid.Trim();
                return GetGridReport(Model, "R", "Code^" + Model.Document);
            }
        }
        #endregion subreport
    }
}