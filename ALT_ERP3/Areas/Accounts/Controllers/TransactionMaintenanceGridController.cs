using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
////using ALT_ERP3.DynamicBusinessLayer;
////using ALT_ERP3.DynamicBusinessLayer.Repository;
using Common;
using CrystalDecisions.CrystalReports.Engine;
using EntitiModel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WebForms;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class TransactionMaintenanceGridController : BaseController
    {
        //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        private static string msubcodeof = "";

        ////ITransactionGridOperation mIlst = new TransactionGridOperation();
        //IBusinessCommon mIBuss = new BusinessCommon();
        private DataTable table = new DataTable();
        List<SelectListItem> typelist = new List<SelectListItem>();
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
        public ActionResult GetTypes(LedgerVM Model)
        {
            if (muserid.ToLower() == "super")
            {
                var result = (from d in ctxTFAT.DocTypes
                              where d.SubType == Model.SubType && d.Code != d.SubType && d.TypeLock != true /*&& d.AppBranch.Contains(mbranchcode) */
                              select new { Value = d.Code, Text = "[" + d.MainType + "-" + d.SubType + "] " + d.Name, Name = d.Name }).OrderBy(n => n.Name).ToList();
                result = result.Where(x => x.Value != "CPO00" && x.Value != "COT00").ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = (from d in ctxTFAT.DocTypes
                              where d.SubType == Model.SubType && d.Code != d.SubType && d.TypeLock != true /*&& d.AppBranch.Contains(mbranchcode)*/
                              //join ur in ctxTFAT.UserRightsTrx on d.Code equals ur.Type
                              //where ur.Code == muserid && ur.xCess == true
                              select new { Value = d.Code, Text = "[" + d.MainType + "-" + d.SubType + "] " + d.Name, Name = d.Name }).OrderBy(n => n.Name).ToList();
                result = result.Where(x => x.Value != "CPO00" && x.Value != "COT00").ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Accounts/TransactionMaintenanceGrid
        public ActionResult Index(GridOption Model)
        {
            //GetAllMenu(Session["ModuleName"].ToString());
            ////AccountingGetAllMenu(Session["ModuleName"].ToString());
            ////VehicleGetAllMenu(Session["ModuleName"].ToString());
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", System.DateTime.Now, 0, "", "", "A");
            Model.FromDate = ConvertToLocaleDate(System.Web.HttpContext.Current.Session["StartDate"].ToString());
            Model.ToDate = ConvertToLocaleDate(System.Web.HttpContext.Current.Session["LastDate"].ToString());
            Model.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
            Model.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
            Model.MenuName = ctxTFAT.TfatMenu.Where(x => x.OptionCode == Model.OptionCode).Select(x => x.Menu).FirstOrDefault();
            msubcodeof = Model.ViewDataId;
            ViewBag.ViewDataId = Model.ViewDataId;
            var mresult = ctxTFAT.ReportHeader.Where(x => x.SubCodeOf == msubcodeof && !x.Code.EndsWith(".bak") && x.DefaultReport == true).Select(x => x.Code).FirstOrDefault() ?? "";
            if (mresult != "")
            {
                ViewBag.id = mresult;
                ViewBag.ViewDataId = mresult;
            }
            if (Model.Type != null)
            {
                var result = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => new
                {
                    x.MainType,
                    x.SubType,
                    x.Name,
                    x.ConstantMode,
                    x.Code,
                    x.CurrConv
                }).FirstOrDefault();

                Model.MainType = result.MainType;
                Model.SubType = result.SubType;
                Model.Header = result.Name;
                Model.Mode = result.ConstantMode.ToString();
                Model.Type = result.Code;
                Model.CurrConv = result.CurrConv;
                Model.ComController = true;

                if (Request.IsAjaxRequest())
                {
                    return Json(Model, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var result1 = ctxTFAT.DocTypes.Where(x => x.MainType == Model.MainType && x.SubType == Model.SubType && x.SubType != x.Code && x.TypeLock != true && x.AppBranch.Contains(mbranchcode)).Select(x => new
                {
                    x.Code,
                    x.ConstantMode,
                    x.Name,
                    x.CurrConv
                }).Take(1).FirstOrDefault();

                if (result1 != null)
                {
                    Model.Mode = result1.ConstantMode.ToString();
                    Model.Type = result1.Code;
                    Model.CurrConv = result1.CurrConv;
                    Model.Header = result1.Name;
                }

                if (Model.SubType == "BP")
                {
                    Model.Type = "BPMM0";
                }
                else if (Model.SubType == "BR")
                {
                    Model.Type = "BRCC0";
                }
                else if (Model.SubType == "HP")
                {
                    Model.Type = "CPM00";
                }
                else if (Model.SubType == "CP")
                {
                    Model.Type = "CPH00";
                }
                else if (Model.SubType == "CR")
                {
                    Model.Type = "CRC00";
                }
                else if (Model.SubType == "CN")
                {
                    Model.Type = "CRN00";
                }
                else if (Model.SubType == "DN")
                {
                    Model.Type = "DRN00";
                }
                else if (Model.SubType == "TP")
                {
                    Model.Type = "TPM00";
                }
                else if (Model.SubType == "QD")
                {
                    Model.Type = "CHQDP";
                }




            }
            TfatMenu tfatMenu = ctxTFAT.TfatMenu.Where(z => z.OptionCode == Model.OptionCode && z.ModuleName == Model.Module).FirstOrDefault();

            if (muserid.ToLower() == "super")
            {
                Model.xAdd = true;
                Model.xEdit = true;
                Model.xDelete = true;
                Model.xView = true;
            }
            else
            {
                var result = ctxTFAT.UserRights.Where(z => z.MenuID == tfatMenu.ID && z.ModuleName == Model.Module && z.Code == muserid).FirstOrDefault();
                if (result != null)
                {
                    Model.xAdd = result.xAdd;
                    Model.xDelete = result.xDelete;
                    Model.xEdit = result.xEdit;
                    Model.xPrint = result.xPrint;
                }
            }
            return View(Model);
        }

        public ActionResult GetFormats()
        {
            var result = ctxTFAT.ReportHeader.Where(x => x.SubCodeOf == msubcodeof && !x.Code.EndsWith(".bak")).Select(m => new
            {
                Code = m.Code,
                Name = m.Code
            }).OrderBy(n => n.Code).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RefreshReport(GridOption Model)
        {
            if (Model.Type == null || Model.Type == "" || !(Request.IsAjaxRequest()))
            {
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            var result = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => new
            {
                x.MainType,
                x.SubType,
                x.Name,
                x.ConstantMode,
                x.Code,
                x.CurrConv
            }).FirstOrDefault();

            Model.MainType = result.MainType;
            Model.SubType = result.SubType;
            Model.Header = result.Name;
            Model.Mode = result.ConstantMode.ToString();
            Model.Type = result.Code;
            //Model.MenuName = result.Name;
            Model.CurrConv = result.CurrConv;
            Model.ComController = true;

            TfatMenu tfatMenu = ctxTFAT.TfatMenu.Where(z => z.OptionCode == Model.OptionCode && z.ModuleName == Model.Module).FirstOrDefault();
            if (muserid.ToLower() == "super")
            {
                Model.xAdd = true;
                Model.xEdit = true;
                Model.xDelete = true;
                Model.xView = true;
            }
            else
            {
                var result1 = ctxTFAT.UserRights.Where(z => z.MenuID == tfatMenu.ID && z.ModuleName == Model.Module && z.Code == muserid).FirstOrDefault();
                if (result1 != null)
                {
                    Model.xAdd = result1.xAdd;
                    Model.xDelete = result1.xDelete;
                    Model.xEdit = result1.xEdit;
                    Model.xPrint = result1.xPrint;
                }
            }


            //var userrightstrx = ctxTFAT.UserRightsTrx.Where(x => x.Code == muserid && x.Type == Model.Type).Select(x => x).FirstOrDefault();
            //if (userrightstrx != null)
            //{
            //    Model.xAdd = userrightstrx.xAdd;
            //    Model.xEdit = userrightstrx.xEdit;
            //    Model.xDelete = userrightstrx.xDelete;
            //    Model.xView = userrightstrx.xCess;
            //}
            if (muserid.ToLower() == "super")
            {
                Model.xAdd = true;
                Model.xEdit = true;
                Model.xDelete = true;
                Model.xView = true;
            }
            return Json(Model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            return GetGridDataColumns(Model.ViewDataId, "L", "EDVP");
        }

        //[HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            if (Model.IsFormatSelected == true)
            {
                int noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=0 Where SubCodeOf='" + msubcodeof + "'");
                noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=-1 Where Code='" + Model.ViewDataId + "'");
            }
            return GetGridReport(Model, "T", "Type^" + Model.Type, false, 0);
        }

        //[HttpPost]
        //public ActionResult GetSubGridStructure(GridOption Model)
        //{
        //    string msubgrid = "";
        //    msubgrid = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).Select(z => z.DrillQuery.Trim()).FirstOrDefault() ?? "";
        //    if (msubgrid == "")
        //    {
        //        return Json(new { Status = "Error", Message = "Sub-Grid format not found.." }, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        return GetGridDataColumns(msubgrid, "X");
        //    }
        //}

        //[HttpPost]
        //public ActionResult GetSubGridData(GridOption Model)
        //{
        //    string msubgrid = "";
        //    msubgrid = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).Select(z => z.DrillQuery.Trim()).FirstOrDefault() ?? "";
        //    if (msubgrid.Trim() == "")
        //    {
        //        return Json(new
        //        {
        //            Status = "Error",
        //            Message = "Sub-Grid format not found.."
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        Model.ViewDataId = msubgrid.Trim();
        //        Model.Document = Model.Document.Substring(6);
        //        return GetGridReport(Model, "R", "Document^" + Model.Document, false, 0);
        //    }
        //}

        [HttpPost]
        public ActionResult GetMultiPrint(GridOption Model)
        {
            List<GridOption> Grlist = new List<GridOption>();
            var list = ctxTFAT.DocFormats.Where(x => x.Type == Model.Type).Select(x => x).ToList();
            if (list != null)
            {
                foreach (var a in list)
                {
                    Grlist.Add(new GridOption
                    {
                        Format = a.FormatCode,
                        IsFormatSelected = a.Selected,
                        StoreProcedure = a.StoredProc
                    });
                }
            }
            Model.list = Grlist;
            var html = ViewHelper.RenderPartialView(this, "MultiPrint", new GridOption() { list = Model.list });
            var jsonResult = Json(new
            {
                list = Model.list,
                Html = html
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult PrintDataSSRS(GridOption Model)
        {
            ReportViewer reportViewer = new ReportViewer();
            reportViewer.ProcessingMode = ProcessingMode.Local;
            reportViewer.SizeToReportContent = true;
            reportViewer.ZoomMode = ZoomMode.PageWidth;
            reportViewer.ShowToolBar = true;
            reportViewer.AsyncRendering = true;
            reportViewer.Reset();

            string msubtypeType = "";
            string mSPSource = "";
            string mMainType = "";
            string mParentKey = Model.Document.Substring(6, Model.Document.Length - 6);
            var mDoc = ctxTFAT.DocTypes.Where(x => x.Code == mParentKey.Substring(0, 5)).Select(x => new { x.SubType, x.DataSource, x.MainType }).FirstOrDefault();
            if (mDoc != null)
            {
                msubtypeType = mDoc.SubType;
                //mSPSource = mDoc.DataSource;
                mMainType = mDoc.MainType;
            }

            DataTable dtreport = new DataTable();
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("SPDoc_" + Model.StoreProcedure, tfat_conx); //name of the storedprocedure
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@mTableKey", mParentKey);
            //cmd.Parameters.AddWithValue("@msubtypeType", msubtypeType);
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            adp.Fill(dtreport);

            string mFormat = Model.Format.Trim();
            //ctxTFAT.DocTypes.Where(x => x.Code == mParentKey.Substring(0, 5)).Select(x => x.Formats).FirstOrDefault().Trim();
            if (mFormat != "")
            {
                mFormat = "NoFormatDefined";
            }
            if (mFormat.ToLower().EndsWith(".rdlc") == false) mFormat = mFormat + ".rdlc";
            reportViewer.LocalReport.ReportPath = Server.MapPath("/Reports/" + mFormat);
            reportViewer.LocalReport.DataSources.Clear();
            reportViewer.Visible = true;

            string mDataSource = "";
            if (msubtypeType == "EP")
            {
                mDataSource = "DS_PurchaseEnquiry";
            }
            else if (msubtypeType == "ES")
            {
                mDataSource = "DS_SalesEnquiry";
            }
            else if (msubtypeType == "CP")
            {
                mDataSource = "DS_CashPurchase";
            }
            else if (msubtypeType == "IC")
            {
                mDataSource = "DS_GoodsInward";
            }
            else if (msubtypeType == "IM")
            {
                mDataSource = "DS_ImportPurchase";
            }
            else if (msubtypeType == "NP")
            {
                mDataSource = "DS_PurchaseReturns";
            }
            else if (msubtypeType == "PX")
            {
                mDataSource = "DS_PurchaseReturnChallan";
            }
            else if (msubtypeType == "RP")
            {
                mDataSource = "DS_CreditPurchase";
            }
            else if (msubtypeType == "CS")
            {
                mDataSource = "DS_CashSales";
            }
            else if (msubtypeType == "GO")
            {
                mDataSource = "DS_GoodsDelivery";
            }
            else if (msubtypeType == "NS")
            {
                mDataSource = "DS_SalesReturn";
            }
            else if (msubtypeType == "SX")
            {
                mDataSource = "DS_SalesReturnChallan";
            }
            else if (msubtypeType == "OC")
            {
                mDataSource = "DS_GoodsDeliveryNote";
            }
            else if (msubtypeType == "RS")
            {
                mDataSource = "DS_CreditSales";
            }
            else if (msubtypeType == "US")
            {
                mDataSource = "DS_CounterSales";
            }
            else if (msubtypeType == "XS")
            {
                mDataSource = "DS_ExportInvoice";
            }
            else if (msubtypeType == "QP")
            {
                mDataSource = "DS_PurchaseQuotes";
            }
            else if (msubtypeType == "QS")
            {
                mDataSource = "DS_SalesQuotes";
            }
            else if (msubtypeType == "OP")
            {
                mDataSource = "DS_PurchaseOrders";
            }
            else if (msubtypeType == "OS")
            {
                mDataSource = "DS_SalesOrders";
            }
            else if (msubtypeType == "PI")
            {
                mDataSource = "DS_ProformaInvoice";
            }
            else if (msubtypeType == "IP")
            {
                mDataSource = "DS_PurchaseIndents";
            }
            else if (mMainType == "JV" || mMainType == "MV" || mMainType == "PV" || mMainType == "RC" || mMainType == "PM")
            {
                mDataSource = "DS_CashBank";
            }
            else if (msubtypeType == "FS")
            {
                mDataSource = "DS_SubContReceived";
            }
            else if (msubtypeType == "IS")
            {
                mDataSource = "DS_SubContIssued";
            }
            else if (msubtypeType == "GT" || msubtypeType == "GI")
            {
                mDataSource = "DS_GoodsTransfer";
            }
            else if (msubtypeType == "IA")
            {
                mDataSource = "DS_InventoryAdj";
            }
            else if (msubtypeType == "MB")
            {
                mDataSource = "DS_MfgByBOM";
            }
            else if (msubtypeType == "MP")
            {
                mDataSource = "DS_DirectMfg";
            }
            else if (msubtypeType == "WO")
            {
                mDataSource = "DS_WorkOrders";
            }
            else if (msubtypeType == "PN")
            {
                mDataSource = "DS_PhysicalStock";
            }
            else if (msubtypeType == "PP")
            {
                mDataSource = "DS_ProductionPlan";
            }
            else if (msubtypeType == "PS")
            {
                mDataSource = "DS_PSP";
            }
            else if (msubtypeType == "PY")
            {
                mDataSource = "DS_MaterialPutAway";
            }
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource(mDataSource, dtreport));
            reportViewer.LocalReport.Refresh();
            ViewBag.ReportViewer = reportViewer;
            return View();
        }

        public ActionResult PrintSingleDocumentCrystal(GridOption Model)
        {
            //var relativePath = "~/Reports/" + Model.Format + ".rpt";
            if (Model.Format == null)
            {
                return Json(new { Status = "filenotfound" }, JsonRequestBehavior.AllowGet);
            }
            if (Model.Format.ToLower().EndsWith(".rdlc") == true)
            {
                return PrintDataSSRS(Model);
            }
            if (FileExists("~/Reports/" + Model.Format.Trim() + ".rpt") == false)
            {
                return Json(new { Status = "filenotfound" }, JsonRequestBehavior.AllowGet);
            }

            string mParentKey = Model.Document.Substring(6, Model.Document.Length - 6);
            DataTable dtreport = new DataTable();
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("SPDoc_" + Model.StoreProcedure, tfat_conx); //name of the storedprocedure
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@mParentKey", mParentKey);
            cmd.Parameters.AddWithValue("@mBranch", Model.Document.Substring(0, 6));
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            adp.Fill(dtreport);

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), Model.Format.Trim() + ".rpt"));
            rd.SetDataSource(dtreport);
            try
            {
                Stream mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                mstream.Seek(0, SeekOrigin.Begin);
                rd.Close();
                rd.Dispose();
                return File(mstream, "application/PDF");
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

        [HttpPost]
        public ActionResult SaveMultiPrint(GridOption Model)
        {
            var FormatList = Model.Format.Split(',');
            //var delformat = ctxTFAT.DocFormats.Where(x => x.Type == Model.Type).Select(x => x).ToList();
            ctxTFAT.Database.ExecuteSqlCommand("Update DocFormats Set Selected=0 Where Type='" + Model.Type + "'");
            foreach (var a in FormatList)
            {
                var docformat = ctxTFAT.DocFormats.Where(x => x.Type == Model.Type && x.FormatCode.Trim() == a.Trim()).Select(x => x).FirstOrDefault();
                docformat.Selected = true;
                ctxTFAT.Entry(docformat).State = EntityState.Modified;
                ctxTFAT.SaveChanges();
            }
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SendMultiReport(GridOption Model)
        {
            if (Model.Format == null)
            {
                return null;
            }

            string mParentKey = "";
            Document document = new Document(PageSize.A4, 10, 10, 10, 10);
            var ms = new MemoryStream();
            {
                PdfCopy pdf = new PdfCopy(document, ms);
                document.Open();
                pdf.Open();

                List<string> mformats = Model.Format.Split(',').ToList();
                foreach (var mformat in mformats)
                {
                    mParentKey = Model.Document.Substring(6, Model.Document.Length - 6);
                    Model.StoreProcedure = ctxTFAT.DocFormats.Where(x => x.FormatCode.Trim() == mformat.Trim()).Select(x => x.StoredProc).FirstOrDefault();
                    DataTable dtreport = new DataTable();
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("SPDoc_" + Model.StoreProcedure, tfat_conx); //name of the storedprocedure
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@mParentKey", mParentKey);
                    cmd.Parameters.AddWithValue("@mBranch", Model.Document.Substring(0, 6));
                    SqlDataAdapter adp = new SqlDataAdapter(cmd);
                    adp.Fill(dtreport);

                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/Reports"), mformat.Trim() + ".rpt"));
                    rd.SetDataSource(dtreport);
                    try
                    {
                        Stream mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
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
            document.Close();
            ////Response.Clear();
            ////Response.ContentType = "application/pdf";
            ////Response.AddHeader("Content-Disposition", "attachment; filename=" + mParentKey + ".pdf");
            ////Response.ContentType = "application/pdf";
            ////Response.Buffer = true;
            ////Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            ////Response.BinaryWrite(ms.ToArray());
            ////Response.End();
            ////Response.Close();
            //if (mParentKey == "" || mParentKey == null) mParentKey = "MultiFormat Document";
            return File(ms.ToArray(), "application/PDF");
            //return null;
        }
    }
}