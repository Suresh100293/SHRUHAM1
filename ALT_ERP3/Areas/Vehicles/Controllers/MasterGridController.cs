using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
using Common;
using System.Data;
using System.Data.SqlClient;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WebForms;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Vehicles.Controllers
{
    public class MasterGridController : BaseController
    {
        // GET: Vehicles/MasterGridd

        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

        // GET: Vehicles/MasterGrid
        public ActionResult Index(GridOption Model)
        {
            Session["TempAttach"] = null;
            GetAllMenu(Session["ModuleName"].ToString());
            Model.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
            Model.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
            //UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "","NA");

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
                var result = ctxTFAT.UserRights.Where(z => z.MenuID == tfatMenu.ID && z.ModuleName == mmodule && z.Code==muserid).FirstOrDefault();

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
            Model.Code = "1=1";
            if (Model.IsFormatSelected == true)
            {
                int noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=0 Where SubCodeOf='" + msubcodeof + "'");
                noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=-1 Where Code='" + Model.ViewDataId + "'");
            }
            if (string.IsNullOrEmpty(Model.searchField))
            {
                if (!String.IsNullOrEmpty(Model.searchString) && Model.searchString != "undefined")
                {
                    if (String.IsNullOrEmpty(Model.searchOper))
                    {
                        Model.searchOper = "cn";
                        if (Model.ViewDataId == "TripSheet")
                        {
                            Model.searchField = " (Select Name From DriverMaster where code=Driver)  ";
                        }
                        else if (Model.ViewDataId == "VehicleGroupStatus")
                        {
                            Model.searchField = "   VehicleGroupStatus    ";
                        }
                        else if (Model.ViewDataId == "VehicleCategory")
                        {
                            Model.searchField = " vehicleCategory ";
                        }
                        else if (Model.ViewDataId == "VehicleMaster" || Model.ViewDataId == "HireVehicle")
                        {
                            Model.searchField = "    TruckNo      ";
                        }
                        else if (Model.ViewDataId == "KilometerMaster" || Model.ViewDataId == "LocalKmAndTime" || Model.ViewDataId == "FreightChargeMaster" || Model.ViewDataId == "TripChargesMaster")
                        {
                            Model.searchField = "  (select Name From TfatBranch where Code=FromBranch)  ";
                        }
                        else if (Model.ViewDataId == "FreightLocalCharges" || Model.ViewDataId == "ViaChargesMaster" || Model.ViewDataId == "ViaFreightMaster" || Model.ViewDataId == "LocalChargesMaster")
                        {
                            Model.searchField = " CASE    WHEN VehicleType='VNo' THEN Vehicle ELSE (select VehicleCategory From VehicleCategory where Code=Vehicle) END  ";
                        }
                        else
                        {
                            Model.searchField = " Name ";
                        }
                    }


                }
            }

            if (Model.ViewDataId == "DriverMaster")
            {
                if (Model.searchOper == "Active")
                {
                    //Model.searchField = "Status";
                    //Model.searchString = " = 'true'";
                    Model.Code = "Status = 'true'";
                }
                else if (Model.searchOper == "Running")
                {
                    //Model.searchField = "VehicleNo";
                    //Model.searchString = " is not null and VehicleNo <> '' ";
                    Model.Code = "(select top 1 G.TruckNo from VehicleDri_Hist G   where G.Driver= DriverMaster.Code order by FromPeriod desc,FromTime desc) is not null and ((select top 1 G.TruckNo from VehicleDri_Hist G   where G.Driver= DriverMaster.Code order by FromPeriod desc,FromTime desc))<>'99999'";
                }
            }
            else if (Model.ViewDataId == "VehicleMaster")
            {
                if (Model.searchOper == "Active")
                {
                    //Model.searchField = "Acitve";
                    //Model.searchString = " = 'true'";
                    Model.Code = "Acitve = 'true'";
                }
                else if (Model.searchOper == "Running")
                {
                    //Model.searchField = "Driver";
                    //Model.searchString = " <> '99999' ";
                    Model.Code = "(select top 1 G.Driver from VehicleDri_Hist G where G.TruckNo=VehicleMaster.Code Order by G.FromPeriod desc,G.FromTime desc) is not null and (select top 1 G.Driver from VehicleDri_Hist G where G.TruckNo=VehicleMaster.Code Order by G.FromPeriod desc,G.FromTime desc)<>'99999'";

                }
            }


            return GetGridReport(Model, "M", "Code^" + Model.Code, false, 0);
        }



        [HttpPost]
        public ActionResult GetMultiPrint(GridOption Model)
        {
            List<GridOption> Grlist = new List<GridOption>();
            Model.Type = ctxTFAT.DocFormats.Where(x => x.FormatCode == Model.ViewDataId).Select(x => x.Type).FirstOrDefault();


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
            Model.PrintGridList = Grlist;
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

        public ActionResult PrintSingleDocumentCrystal(GridOption Model)
        {
            var PDFName = Model.Document;
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

            string mParentKey = Model.Document + mperiod;
            Model.Branch = mbranchcode;
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
            var PDFName = Model.Document;
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
                    mParentKey = Model.Document + mperiod;
                    Model.Branch = mbranchcode;
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

        }

    }
}