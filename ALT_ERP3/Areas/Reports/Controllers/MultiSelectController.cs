using ALT_ERP3.Controllers;
using CrystalDecisions.CrystalReports.Engine;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Reports.Controllers
{
    public class MultiSelectController : BaseController
    {
        private static string mmaintype = "";
        private static string msubtype = "";
        private static string mtype = "";
        private static string mtable = "";
        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "A");
            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            Model.BaseGr = Model.MainType;
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
                foreach (var msrl in mSrls)//Model.list)
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
                            mParentKey = mcode.Substring(6, mcode.Length - 6);
                            var docFormats = ctxTFAT.DocFormats.Where(x => x.Type == mParentKey.Substring(0, 5) && x.Selected == true).Select(x => x).ToList();
                            foreach (var d in docFormats)
                            {
                                Model.StoreProcedure = ctxTFAT.DocFormats.Where(x => x.FormatCode == d.FormatCode).Select(x => x.StoredProc).FirstOrDefault();
                                DataTable dtreport = new DataTable();
                                SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                                SqlCommand cmd = new SqlCommand("SPDoc_" + Model.StoreProcedure, tfat_conx); //name of the storedprocedure
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.CommandTimeout = 120;
                                cmd.Parameters.AddWithValue("@mTableKey", mParentKey);
                                SqlDataAdapter adp = new SqlDataAdapter(cmd);
                                adp.Fill(dtreport);

                                ReportDocument rd = new ReportDocument();
                                rd.Load(Path.Combine(Server.MapPath("~/Reports"), d.FormatCode + ".rpt"));
                                rd.SetDataSource(dtreport);
                                try
                                {
                                    //rd.PrintToPrinter(1, true, 0, 0);
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
            try
            {
                document.Close();
            }
            catch (Exception e) { }
            return File(ms.ToArray(), "application/" + Model.mWhat);
        }

        [HttpPost]
        public ActionResult GetGridStructureML(GridOption Model)
        {
            //string mstr = "^" + string.Join("^", Model.list);
            if (Model.list == null) return null;
            string mstr = "^";
            foreach (var m in Model.list)
            {
                mstr += m.Code + "^";
            }
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
            cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
            cmd.Parameters.Add("@mDate1", SqlDbType.Date).Value = Model.FromDate;
            cmd.Parameters.Add("@mDate2", SqlDbType.Date).Value = Model.ToDate;
            cmd.Parameters.Add("@mLocationCode", SqlDbType.Int).Value = 0;
            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();
            tfat_conx.Close();
            cmd.Dispose();
            tfat_conx.Dispose();
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        [HttpPost]
        public ActionResult GetGridDataML(GridOption Model)
        {
            return GetGridReport(Model, "R", "BaseGr^" + Model.MainType + "~SubType^" + Model.SubType + "~Type^" + Model.Type);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            return GetGridReport(Model, "R", "BaseGr^" + Model.MainType + "~SubType^" + Model.SubType + "~Type^" + Model.Type);
        }

        public ActionResult GetExcel(GridOption Model)
        {
            Model.mWhat = "XLS";
            return GetGridReport(Model, "R", "", false, 0);
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