using ALT_ERP3.Controllers;
using Common;
using CrystalDecisions.CrystalReports.Engine;
using EntitiModel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WebForms;
////using ALT_ERP3.DynamicBusinessLayer;
////using ALT_ERP3.DynamicBusinessLayer.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class MasterGridController : BaseController
    {
        // GET: Accounts/MasterGridd

        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
        // GET: Accounts/MasterGrid
        public ActionResult Index(GridOption Model)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            Model.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
            Model.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
            //UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "A");

            if (Model.Document == null)
            {
                Model.Document = "";
                Model.AccountName = "";
            }
            moptioncode = Model.OptionCode;
            msubcodeof = Model.ViewDataId;
            mmodule = Model.Module;
            ViewBag.id = Model.ViewDataId;
            ViewBag.ViewDataId = Model.ViewDataId;
            ViewBag.Header = Model.Header;
            ViewBag.Table = Model.TableName;
            ViewBag.Controller = Model.Controller;
            ViewBag.MainType = Model.MainType;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;
            ViewBag.ViewName = Model.ViewName;
            if (Model.AcType != null)
            {
                ViewBag.MainType = Model.AcType;
            }

            if (Model.ViewDataId == "Invoice")
            {
                TfatSearch tfatSearch = ctxTFAT.TfatSearch.Where(x => x.ColHead.Contains("Select") && x.Code == Model.ViewDataId).FirstOrDefault();
                tfatSearch.IsHidden = true;
                ctxTFAT.Entry(tfatSearch).State = EntityState.Modified;
                ctxTFAT.SaveChanges();
            }


            TfatMenu tfatMenu = ctxTFAT.TfatMenu.Where(z => z.OptionCode == moptioncode && z.ModuleName == mmodule).FirstOrDefault();
            if (muserid.ToUpper() == "SUPER")
            {
                Model.xAdd = true;
                Model.xDelete = true;
                Model.xEdit = true;
                Model.xPrint = true;
            }
            else
            {
                var result = ctxTFAT.UserRights.Where(z => z.MenuID == tfatMenu.ID && z.ModuleName == mmodule && z.Code == muserid).FirstOrDefault();

                if (result != null)
                {
                    Model.xAdd = result.xAdd;
                    Model.xDelete = result.xDelete;
                    Model.xEdit = result.xEdit;
                    Model.xPrint = result.xPrint;
                }
            }
            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

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
        public ActionResult GetFormats1()
        {
            var result = ctxTFAT.ReportHeader.Where(x => x.SubCodeOf == "LorryDraft" && !x.Code.EndsWith(".bak")).Select(m => new
            {
                Code = m.Code,
                Name = m.Code
            }).OrderBy(n => n.Code).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
            var result = ctxTFAT.TfatMenu.Where(z => z.OptionCode == moptioncode && z.ModuleName == mmodule).Select(x => new { x.AllowEdit, x.AllowDelete }).FirstOrDefault();
            string mopt = "EDVX";
            if (result != null)
            {
                mopt = (result.AllowEdit == false ? "X" : "E") + (result.AllowDelete == false ? "X" : "D") + "XX";
            }
            //return GetGridDataColumns(id, "L", mopt);
            return GetGridDataColumns(id, "L", "XXXX");
        }

        //[HttpPost]
        public ActionResult GetMasterGridData(GridOption Model)
        {
            if (Model.IsFormatSelected == true)
            {
                int noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=0 Where SubCodeOf='" + msubcodeof + "'");
                noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=-1 Where Code='" + Model.ViewDataId + "'");
            }
            if (!String.IsNullOrEmpty(Model.searchString) && Model.searchString.Contains("OP:"))
            {
                var Array = Model.searchString.Replace("OP:", "");
                Model.Code = "Ledger.Branch+Ledger.ParentKey in ( select D.Branch+D.ParentKey  from DebtorRecVouDetail D where InvPrefix<>'" + mperiod + "' and InvSrl ='" + Array + "')";
                Model.searchString = "";
            }
            else
            {
                Model.Code = "1=1";
            }
            if (string.IsNullOrEmpty(Model.searchField))
            {
                if (!String.IsNullOrEmpty(Model.searchString) && Model.searchString != "undefined")
                {
                    Model.searchOper = "cn";
                    if (Model.ViewDataId == "TransactionAuthorisationRules")
                    {
                        Model.searchField = " Type ";
                    }
                    else if (Model.ViewDataId == "ReceivablesCustomers" || Model.ViewDataId == "CreditorPayment" || Model.ViewDataId == "DebtorReceipt" || Model.ViewDataId == "PayablesSuppliers" || Model.ViewDataId == "ViewData-CashSales" || Model.ViewDataId == "ViewData-Purchase" || Model.ViewDataId == "OtherTransaction" || Model.ViewDataId == "OtherTransactionJV")
                    {
                        Model.searchField = "   Master.Name    ";
                    }
                    else if (Model.ViewDataId == "AccountOpeningManual")
                    {
                        Model.searchField = " (select Name from Master where Code=Ledger.Code) ";
                    }
                    else if (Model.ViewDataId == "ContractMaster")
                    {
                        Model.searchField = "    CASE    WHEN Cust='General' THEN Cust  ELSE (select Name from Master where Code=Cust) END      ";
                    }
                    else if (Model.ViewDataId == "CustomerMaster")
                    {
                        Model.searchField = " CustomerMaster.Name ";
                    }
                    else if (Model.ViewDataId == "AdvancePay")
                    {
                        Model.searchField = " (select Name From Master where code=Account) ";
                    }
                    else if (Model.ViewDataId == "ViewData-Sales" || Model.ViewDataId == "ViewData-SalesNOLR" || Model.ViewDataId == "Invoice")
                    {
                        Model.searchField = " Sales.Srl ";
                    }
                    else
                    {
                        Model.searchField = " Name ";
                    }

                }
            }


            var Reporttype = "M";
            if (Model.ViewDataId == "AccountOpeningManual")
            {
                Reporttype = "T";
            }
            if (Model.ViewDataId == "ViewData-Purchase")
            {
                Model.Type = "PUR00";
            }
            if (Model.ViewDataId == "CashPurchase")
            {
                Model.Type = "CPH00";
            }


            return GetGridReport(Model, Reporttype, "Code^" + Model.Code + "~Type^" + Model.Type, false, 0);
            //return GetGridReport(Model, Reporttype, "MainType^" + Model.MainType, false, 0);
        }

        [HttpPost]
        public ActionResult GetMultiPrint(GridOption Model)
        {
            List<GridOption> Grlist = new List<GridOption>();
            //if (Model.ViewDataId== "CustomerLabelPrinting")
            //{
            //    return RedirectToAction("AccountLabelPrint", Model);
            //}
            Model.Branch = Model.Document.Substring(0, 6);
            var ParentKey = Model.Document.Substring(6);
            var Type = ctxTFAT.Ledger.Where(x => x.ParentKey == ParentKey && x.Branch == Model.Branch).Select(x => x.Type).FirstOrDefault();
            Model.Type = Type;


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
            Model.PrintGridList = Grlist.OrderBy(x => x.Format).ToList();
            var html = ViewHelper.RenderPartialView(this, "ReportPrintOptions", new GridOption() { PrintGridList = Model.PrintGridList, Document = Model.Document });
            var jsonResult = Json(new
            {
                Document = Model.Document,
                PrintGridList = Model.PrintGridList,
                Html = html
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }

        public ActionResult AccountLabelPrint(GridOption Model)
        {
            //var PDFName = Model.Document.Substring(19);
            var PDFName = "";
            //var relativePath = "~/Reports/" + Model.Format + ".rpt";
            

            if (FileExists("~/Reports/AccountLabelPrint.rpt") == false)
            {
                return Json(new { Status = "filenotfound" }, JsonRequestBehavior.AllowGet);
            }
            #region Remove Comment
            var absolutePath = HttpContext.Server.MapPath("~/Reports/AccountLabelPrint.rpt");
            if (System.IO.File.Exists(absolutePath) == false)
            {
                return Json(new { Status = "filenotfound" }, JsonRequestBehavior.AllowGet);
            }
            #endregion

            //string mParentKey = Model.Document.Substring(6, Model.Document.Length - 6);
            //Model.Branch = Model.Document.Substring(0, 6);

            DataTable dtreport = new DataTable();
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            string Query = "select C.Name, isnull(CA.Adrl1,'') as Adrl1 , isnull(CA.Adrl2,'') as Adrl2,isnull(CA.Adrl3,'') as Adrl3,(select A.Name from tfatcity A where A.code=CA.City) as City,CA.Pin,CA.Person " +
                "from customermaster C right join Caddress CA on C.Code = CA.Code where CA.RECORDKEY in (" + Model.Document + ")";
            SqlCommand cmd = new SqlCommand(Query, tfat_conx); //name of the storedprocedure
            cmd.CommandType = CommandType.Text;
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            adp.Fill(dtreport);

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "AccountLabelPrint.rpt"));
            rd.SetDataSource(dtreport);

            try
            {
                Stream mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                mstream.Seek(0, SeekOrigin.Begin);
                rd.Close();
                rd.Dispose();
                if (String.IsNullOrEmpty(PDFName))
                {
                    return File(mstream, "application/PDF");
                }
                else
                {
                    return File(mstream, "application/PDF", PDFName + ".pdf");
                }
                //return File(mstream, "application/PDF");
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

        public ActionResult PrintSingleDocumentCrystal(GridOption Model)
        {
            //var PDFName = Model.Document.Substring(19);
            var PDFName = "";
            //var relativePath = "~/Reports/" + Model.Format + ".rpt";
            if (Model.Format == null)
            {
                return Json(new { Status = "filenotfound" }, JsonRequestBehavior.AllowGet);
            }

            if (FileExists("~/Reports/" + Model.Format.Trim() + ".rpt") == false)
            {
                return Json(new { Status = "filenotfound" }, JsonRequestBehavior.AllowGet);
            }
            #region Remove Comment
            var absolutePath = HttpContext.Server.MapPath("~/Reports/" + Model.Format.Trim() + ".rpt");
            if (System.IO.File.Exists(absolutePath) == false)
            {
                return Json(new { Status = "filenotfound" }, JsonRequestBehavior.AllowGet);
            }
            #endregion

            string mParentKey = Model.Document.Substring(6, Model.Document.Length - 6);
            Model.Branch = Model.Document.Substring(0, 6);

            DataTable dtreport = new DataTable();
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("SPDoc_" + Model.StoreProcedure, tfat_conx); //name of the storedprocedure
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@mParentKey", mParentKey);
            cmd.Parameters.AddWithValue("@mBranch", Model.Branch);
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
                if (String.IsNullOrEmpty(PDFName))
                {
                    return File(mstream, "application/PDF");
                }
                else
                {
                    return File(mstream, "application/PDF", PDFName + ".pdf");
                }
                //return File(mstream, "application/PDF");
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

        public ActionResult SendMultiReport(GridOption Model)
        {
            //var PDFName = Model.Document.Substring(19);
            var PDFName = "";
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
                    Model.Branch = Model.Document.Substring(0, 6);
                    Model.StoreProcedure = ctxTFAT.DocFormats.Where(x => x.FormatCode.Trim() == mformat.Trim()).Select(x => x.StoredProc).FirstOrDefault();
                    DataTable dtreport = new DataTable();
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("SPDoc_" + Model.StoreProcedure, tfat_conx); //name of the storedprocedure
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@mParentKey", mParentKey);
                    cmd.Parameters.AddWithValue("@mBranch", Model.Branch);
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

            if (String.IsNullOrEmpty(PDFName))
            {
                return File(ms.ToArray(), "application/PDF");
            }
            else
            {
                return File(ms.ToArray(), "application/PDF", PDFName + ".pdf");
            }
            //return File(ms.ToArray(), "application/PDF");

        }

        //IRN
        public ActionResult GSTeInvoice(string mparentkey)
        {
            //HO0000HO0000SLR00231000169
            string mirn;
            mparentkey = mparentkey.Substring(6);
            string mtable = "";
            var tmpBranchCode = mparentkey.Substring(0, 6);
            if (ctxTFAT.TfatBranch.Where(x => x.Code == tmpBranchCode).FirstOrDefault() != null)
            {
                mtable = GetTableName(GetSubType(mparentkey.Substring(6, 5)));
            }
            else
            {
                mtable = GetTableName(GetSubType(mparentkey.Substring(0, 5)));
            }

            //if (Fieldoftable(mtable, "GSTIRNNumber", "Tablekey='" + mparentkey + "'").Trim() != "")
            //{
            //    mirn = "The IRN is already Generated..";
            //}
            if (string.IsNullOrEmpty(Fieldoftable(mtable, "QRCodeImage", "Tablekey='" + mparentkey + "'")))
            {
                GSTOnlineController mcls = new GSTOnlineController();
                mcls.GenerateQRImage(mparentkey, Session["CustomerID"].ToString().ToUpper());
                mcls.Dispose();
            }
            if (!Fieldoftable(mtable, "Authorise", "Tablekey='" + mparentkey + "'").StartsWith("A"))
            {
                mirn = "The Document is not yet Authorised (Approved)..";
            }
            else
            {
                GSTOnlineController mcls = new GSTOnlineController();
                mirn = mcls.GetIRNNumber(mparentkey, Session["CustomerID"].ToString().ToUpper());
                mcls.Dispose();
            }
            return Json(new { result = mirn }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GSTCancelIRN(string mparentkey)
        {
            string mirn;
            //mparentkey = mparentkey.Substring(6);
            //string mtable = GetTableName(GetSubType(mparentkey.Substring(0, 5)));

            mparentkey = mparentkey.Substring(6);
            string mtable = "";
            var tmpBranchCode = mparentkey.Substring(0, 6);
            if (ctxTFAT.TfatBranch.Where(x => x.Code == tmpBranchCode).FirstOrDefault() != null)
            {
                mtable = GetTableName(GetSubType(mparentkey.Substring(6, 5)));
            }
            else
            {
                mtable = GetTableName(GetSubType(mparentkey.Substring(0, 5)));
            }
            if (!Fieldoftable(mtable, "Authorise", "Tablekey='" + mparentkey + "'").StartsWith("A"))
            {
                mirn = "The Document is not yet Authorised (Approved)..";
            }
            else if (Fieldoftable(mtable, "GSTIRNNumber", "Tablekey='" + mparentkey + "'").Trim() == "")
            {
                mirn = "IRN is not Generated..";
            }
            //else if (Fieldoftable(mtable, "EWBNo", "Tablekey='" + mparentkey + "'").Trim() != "")
            //{
            //    mirn = "eWay Bill Number is Generated..Can't Cancel..";
            //}
            else
            {
                GSTOnlineController mcls = new GSTOnlineController();
                mirn = mcls.CancelIRN(mparentkey);
                mcls.Dispose();
            }
            return Json(new { result = mirn }, JsonRequestBehavior.AllowGet);
        }

        //Eway Bill
        public ActionResult GSTIRNEWay(string mparentkey)
        {
            string mirn;
            mparentkey = mparentkey.Substring(6);
            string mtable = GetTableName(GetSubType(mparentkey.Substring(0, 5)));
            if (!Fieldoftable(mtable, "Authorise", "Tablekey='" + mparentkey + "'").StartsWith("A"))
            {
                mirn = "The Document is not yet Authorised (Approved)..";
            }
            else if (Fieldoftable(mtable, "GSTIRNNumber", "Tablekey='" + mparentkey + "'").Trim() == "")
            {
                mirn = "IRN is not Generated yet..";
            }
            else if (FieldoftableBool(mtable, "CancelFlagGST", "Tablekey='" + mparentkey + "'") == true)
            {
                mirn = "IRN is Cancelled..Can't Genetate EWB..";
            }
            else if (Fieldoftable(mtable, "EWBNo", "Tablekey='" + mparentkey + "'").Trim() != "")
            {
                mirn = "EWB Number is already Generated..";
            }
            else
            {
                GSTOnlineController mcls = new GSTOnlineController();
                mirn = mcls.GenerateEWay(mparentkey);
                mcls.Dispose();
            }
            return Json(new { result = mirn }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GSTCancelEWB(string mparentkey)
        {
            string mirn;
            mparentkey = mparentkey.Substring(6);
            string mtable = GetTableName(GetSubType(mparentkey.Substring(0, 5)));
            if (Fieldoftable(mtable, "EWBNo", "Tablekey='" + mparentkey + "'") == "")
            {
                mirn = "eWay Bill Number is not Generated..Nothing to Cancel..";
            }
            else
            {
                GSTOnlineController mcls = new GSTOnlineController();
                mirn = mcls.CancelEWB(mparentkey);
                mcls.Dispose();
            }
            return Json(new { result = mirn }, JsonRequestBehavior.AllowGet);
        }
    }
}