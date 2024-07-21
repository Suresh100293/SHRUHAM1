using ALT_ERP3.Controllers;
using CrystalDecisions.CrystalReports.Engine;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WebForms;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using ZXing;

namespace ALT_ERP3.Areas.Reports.Controllers
{
    public class PrintBarCodeController : BaseController
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
            GenerateBarCodeTable(Model.Document, mtable);
            return View(Model);
        }

        public ActionResult dummyurl()  // required for jqgrid inline edit (don't remove) - SDS
        {
            return null;
        }

        private void GenerateBarCodeTable(string mdocument, string mtable)
        {
            string mmtable;
            if (mtable == "Sales" || mtable == "Purchase" || mtable == "Stock")
            {
                mmtable = "Stock";
            }
            else
            {
                mmtable = mtable + "Stk";
            }
            string mconn = GetConnectionString();
            ExecuteStoredProc("Delete from BarCodePrint", mconn);
            DataTable mdt = GetDataTable("Select ParentKey,TableKey,Qty,Rate,Amt,Code,Authorise,Authids,Lastupdatedate,Enteredby from " + mmtable + " Where ParentKey='" + mdocument.Substring(6) + "'", mconn);
            foreach (DataRow mrow in mdt.Rows)
            {
                string mstr = "Insert into BarCodePrint (";
                mstr += "Sno,Branch,ParentKey,TableKey,Code,Qty,Rate,Amt,SQty,Authorise,Authids,Lastupdatedate,Enteredby";
                mstr += ") values (";
                mstr += 0 + ",'" + mbranchcode + "','" + mrow["ParentKey"].ToString() + "','" + mrow["TableKey"].ToString() + "','" + mrow["Code"].ToString() + "',";
                mstr += Convert.ToDouble(mrow["Qty"]) + "," + Convert.ToDouble(mrow["Rate"]) + "," + Convert.ToDouble(mrow["Amt"]) + ",1,";
                mstr += "'" + mrow["Authorise"].ToString() + "','" + mrow["Authids"].ToString() + "','" + MMDDYY(Convert.ToDateTime(mrow["Lastupdatedate"])) + "','" + mrow["Enteredby"].ToString() + "'";
                mstr += ")";
                ExecuteStoredProc(mstr, mconn);
            }
        }

        private byte[] GetBarCodeImage(string mcode)
        {
            MemoryStream ms = new MemoryStream();
            BarcodeWriter writer = new ZXing.BarcodeWriter() { Format = BarcodeFormat.CODE_128 };
            writer.Options.Height = 60;
            writer.Options.Width = 400;
            writer.Options.PureBarcode = true;
            System.Drawing.Image img = writer.Write(mcode);
            img.Save(ms, ImageFormat.Png);
            //Model.BarCodeImage = Convert.ToBase64String(ms.ToArray());
            //string filename = "BarCode.bmp";
            byte[] fileBytes = Convert.FromBase64String(Convert.ToBase64String(ms.ToArray()));
            ms.Dispose();
            return fileBytes;
        }

        private byte[] GetQRCodeImage(string mcode)
        {
            //MemoryStream ms = new MemoryStream();
            //QRCodeGenerator qrGenerator = new QRCodeGenerator();
            //QRCodeGenerator.QRCode qrCode = qrGenerator.CreateQrCode(mcode, QRCodeGenerator.ECCLevel.Q);
            //Bitmap bitMap = qrCode.GetGraphic(20);
            //bitMap.Save(ms, ImageFormat.Png);
            ////Model.QRCodeImage = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
            //byte[] fileBytes = Convert.FromBase64String(Convert.ToBase64String(ms.ToArray()));
            //bitMap.Dispose();
            //ms.Dispose();

            MemoryStream ms2 = new MemoryStream();
            QRCodeGenerator qrGenerator2 = new QRCodeGenerator();
            QRCodeData qrCodeData2 = qrGenerator2.CreateQrCode(mcode, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode2 = new QRCode(qrCodeData2);
            Bitmap bitMap2 = qrCode2.GetGraphic(20);
            bitMap2.Save(ms2, System.Drawing.Imaging.ImageFormat.Jpeg);//ImageFormat.Bmp
            byte[] fileBytes2 = Convert.FromBase64String(Convert.ToBase64String(ms2.ToArray()));
            bitMap2.Dispose();
            qrCodeData2.Dispose();
            qrGenerator2.Dispose();
            ms2.Dispose();

            return fileBytes2;
        }

        private string PrintBarCode(string mdocument)
        {
            string mmtable;
            if (mtable == "Sales" || mtable == "Purchase" || mtable == "Stock")
            {
                mmtable = "Stock";
            }
            else
            {
                mmtable = mtable + "Stk";
            }
            string mconn = GetConnectionString();
            string mdoc = "";
            ExecuteStoredProc("Delete from BarCodePrint", mconn);
            List<string> mSrls = mdocument.Split('^').ToList();
            foreach (var msrl in mSrls)//Model.list)
            {
                if (msrl != "")
                {
                    var mvar = msrl.Split('~');
                    DataTable mdt = GetDataTable("Select ParentKey,TableKey,Qty,Rate,Amt,Code,Authorise,Authids,Lastupdatedate,Enteredby from " + mmtable + " Where TableKey='" + mvar[0].Substring(6) + "'", mconn);
                    foreach (DataRow mrow in mdt.Rows)
                    {
                        double mqty = Convert.ToDouble(mvar[1]);
                        //Math.Abs((double)mrow["Qty"]);
                        for (int x = 1; x <= mqty; x++)
                        {
                            string mstr = "Insert into BarCodePrint (";
                            mstr += "Sno,Branch,ParentKey,TableKey,Code,Qty,Rate,Amt,SQty,Authorise,Authids,Lastupdatedate,Enteredby";
                            mstr += ") values (";
                            mstr += (x - 1) + ",'" + mbranchcode + "','" + mrow["ParentKey"].ToString() + "','" + mrow["TableKey"].ToString() + "','" + mrow["Code"].ToString() + "',";
                            mstr += Convert.ToDouble(mrow["Qty"]) + "," + Convert.ToDouble(mrow["Rate"]) + "," + Convert.ToDouble(mrow["Amt"]) + ",1,";
                            mstr += "'" + mrow["Authorise"].ToString() + "','" + mrow["Authids"].ToString() + "','" + MMDDYY(Convert.ToDateTime(mrow["Lastupdatedate"])) + "','" + mrow["Enteredby"].ToString() + "'";
                            mstr += ")";
                            ExecuteStoredProc(mstr, mconn);
                            mdoc += mbranchcode + mrow["TableKey"].ToString() + "^";
                        }
                    }
                }
            }
            mdoc = CutRightString(mdoc, 1, "^");
            return mdoc;
        }

        public ActionResult BulkPrint(GridOption Model)
        {
            Model.SelectContent = PrintBarCode(Model.SelectContent);
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
            string mParentKey = "";
            int msno = 0;
            Document document = new Document(PageSize.A4, 10, 10, 10, 10);
            var ms = new MemoryStream();
            {
                PdfCopy pdf = new PdfCopy(document, ms);
                document.Open();
                pdf.Open();
                List<string> mSrls = Model.SelectContent.Split('^').ToList();
                foreach (var msrl in mSrls)
                {
                    if (string.IsNullOrEmpty(msrl) == false)
                    {
                        mParentKey = msrl.Substring(6, msrl.Length - 6);
                        Model.StoreProcedure = ctxTFAT.DocFormats.Where(x => x.FormatCode == Model.Format).Select(x => x.StoredProc).FirstOrDefault();
                        DataTable dtreport = new DataTable();
                        SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                        SqlCommand cmd = new SqlCommand("SPDoc_" + Model.StoreProcedure, tfat_conx); //name of the storedprocedure
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 120;
                        cmd.Parameters.AddWithValue("@mTableKey", mParentKey);
                        cmd.Parameters.AddWithValue("@mSno", msno++);
                        SqlDataAdapter adp = new SqlDataAdapter(cmd);
                        adp.Fill(dtreport);
                        int mbccol = -1;
                        int mbcicol = -1;
                        for (int i = 0; i < dtreport.Columns.Count; i++)
                        {
                            string mcolname = dtreport.Columns[i].ColumnName.Trim().ToLower();
                            if (mcolname == "barcode")
                            {
                                mbccol = i;
                            }
                            if (mcolname == "barcodeimage")
                            {
                                mbcicol = i;
                            }
                        }
                        if (mbccol != -1 && mbcicol != -1)
                        {
                            foreach (DataRow dr in dtreport.Rows)
                            {
                                dr[mbcicol] = GetBarCodeImage(dr[mbccol].ToString());
                            }
                        }

                        ReportDocument rd = new ReportDocument();
                        rd.Load(Path.Combine(Server.MapPath("~/Reports"), Model.Format + ".rpt"));
                        rd.SetDataSource(dtreport);
                        //rd.PrintToPrinter(1, true, 0, 0);
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
            }
            try
            {
                document.Close();
            }
            catch (Exception e) { }
            return File(ms.ToArray(), "application/PDF");
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

        public ActionResult GetFormats(GridOption Model)
        {
            var docFormats = (from d in ctxTFAT.DocFormats
                              where d.Type == Model.Type
                              select new
                              {
                                  Value = d.FormatCode,
                                  Text = d.FormatCode
                              }).ToList();
            return Json(docFormats, JsonRequestBehavior.AllowGet);
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