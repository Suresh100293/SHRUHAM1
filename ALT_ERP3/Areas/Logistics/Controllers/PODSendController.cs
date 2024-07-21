using Common;
using EntitiModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WebForms;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class PODSendController : BaseController
    {

        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        public static string connstring = "";
        //ITransactionGridOperation mIlst = new TransactionGridOperation();
        private string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

        #region Function List

        public DateTime ConvertDDMMYYTOYYMMDD(string da)
        {
            string abc = da.Substring(6, 4) + "/" + da.Substring(3, 2) + "/" + da.Substring(0, 2);
            return Convert.ToDateTime(abc);
        }

        public JsonResult BranchList(string term)
        {
            List<TfatBranch> list = new List<TfatBranch>();

            list = ctxTFAT.TfatBranch.Where(x => x.Code != mbranchcode && (x.Status == true && x.Code != "G00000" && x.Grp != "G00000" && x.Category != "Area")).ToList();

            var Newlist = list.Distinct();
            if (!(String.IsNullOrEmpty(term)))
            {
                Newlist = Newlist.Where(x => x.Name.ToLower().Contains(term.ToLower())).OrderBy(x => x.Name).Take(10).ToList();
            }

            Newlist = Newlist.Where(x => x.Category != "Area").ToList();

            Newlist.Where(w => w.Category == "Zone").ToList().ForEach(w => w.Name = w.Name + " - Z");
            Newlist.Where(w => w.Category == "0").ToList().ForEach(w => w.Name = w.Name + " - HO");
            Newlist.Where(w => w.Category == "Branch").ToList().ForEach(w => w.Name = w.Name + " - B");
            Newlist.Where(w => w.Category == "SubBranch").ToList().ForEach(w => w.Name = w.Name + " - SB");


            var Modified = Newlist.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }


        public JsonResult CustmoerList(string term)
        {

            var list = ctxTFAT.CustomerMaster.Where(x => x.NonActive == false).ToList();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public string GetNewCode()
        {
            var mPrevSrl = GetLastSerial("PODMaster", mbranchcode, "POD00", mperiod, "RS", DateTime.Now.Date);

            //var NewLcNo = ctxTFAT.PODMaster.Where(x => x.Prefix == mperiod).OrderByDescending(x => x.PODNo).Select(x => x.PODNo).Take(1).FirstOrDefault();
            //int LcNo;
            //if (NewLcNo == 0 || NewLcNo == null)
            //{
            //    var DocType = ctxTFAT.DocTypes.Where(x => x.Code == "POD00").Select(x => new { x.DocWidth, x.LimitFrom }).FirstOrDefault();
            //    LcNo = Convert.ToInt32(DocType.LimitFrom);
            //}
            //else
            //{
            //    LcNo = Convert.ToInt32(NewLcNo) + 1;
            //}

            return mPrevSrl.ToString();
        }

        #endregion
        #region Index(List)

        public ActionResult Index(GridOption Model)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "NA");
            Model.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
            Model.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
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

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
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
            return GetGridReport(Model, "M", "MainType^" + Model.MainType, false, 0);
        }

        public ActionResult GetPODBranchSendGridData(GridOption Model)
        {
            if (Model.IsFormatSelected == true)
            {
                int noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=0 Where SubCodeOf='" + msubcodeof + "'");
                noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=-1 Where Code='" + Model.ViewDataId + "'");
            }

            List<ListOfPod> ExistingPOD = TempData.Peek("ExistPOD") as List<ListOfPod>;
            var ExistKey = "";
            if (ExistingPOD != null)
            {
                foreach (var item in ExistingPOD)
                {
                    ExistKey += "'" + item.PODRefTablekey + "',";
                }
            }
            if (!String.IsNullOrEmpty(ExistKey))
            {
                ExistKey = ExistKey.Substring(0, ExistKey.Length - 1);
            }
            else
            {
                ExistKey = "''";
            }

            ExecuteStoredProc("Drop Table Ztemp_PODReceivedSendBranch");
            ExecuteStoredProc("WITH ranked_messages AS(	select ROW_NUMBER() OVER (PARTITION BY PODRel.LRRefTablekey ORDER BY PODREL.recordkey DESC) AS rn,PODM.PODNo as PODNO,LR.LrNo as Lrno,LR.Time as LRTime,Convert(char(10),LR.BookDate, 103) as LRDate, (select C.Name from Consigner C where C.code= LR.RecCode) as ConsignerName,(select C.Name from Consigner C where C.code=LR.SendCode) as ConsigneeName,(select T.Name from TfatBranch T where T.code=LR.Source) as FromName,(select T.Name from TfatBranch T where T.code=LR.Dest) as ToName,PODREL.RecePODRemark as Remark,PODREL.TableKey as PODRefTablekey,PODM.AUTHORISE as Authorise,PODRel.LRRefTablekey as ConsignmentKey,PODM.CurrentBranch,PODM.SendReceive,PODREL.FromBranch,PODREL.ToBranch 	from PODMaster PODM	join PODRel PODREL on PODM.TableKey=PODREL.ParentKey join LRMaster LR on PODRel.LRRefTablekey=LR.TableKey where PODREL.TableKey not in (select distinct POSD.PODRefTablekey from PODRel POSD where POSD.PODRefTablekey is not null)   ) SELECT * into Ztemp_PODReceivedSendBranch FROM ranked_messages WHERE  SendReceive='R' and CurrentBranch='" + mbranchcode + "' and PODRefTablekey not in (" + ExistKey + ")");

            return GetGridReport(Model, "M", "Code^" + Model.Code + "~MainType^" + Model.MainType, false, 0);
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
            var html = ViewHelper.RenderPartialView(this, "ReportPrintOptions", new GridOption() { PrintGridList = Model.PrintGridList, Document = Model.Document, ViewDataId = Model.ViewDataId });
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
            //var PDFName = Model.Document.Substring(20);
            var PDFName = "";
            //var ViewId = Model.ViewDataId.Trim();
            //if (ViewId== "LREntry")
            //{
            //    var DocUment = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == Model.Document).FirstOrDefault();
            //    if (DocUment!=null)
            //    {
            //        PDFName += "LR :" + DocUment.LrNo;
            //    }
            //}

            string mParentKey = Model.Document;
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
                //Response.Headers.Add("Content-Disposition", "DemoLR");
                if (String.IsNullOrEmpty(PDFName))
                {
                    return File(mstream, "application/PDF");
                }
                else
                {
                    return File(mstream, "application/PDF", PDFName + ".pdf");
                }

                //return File(mstream,"application/PDF","Demo"); //Automatically Download With Demo Name File But Some Extension Problem Over There.


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
            //var PDFName = Model.Document.Substring(20);
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
                    mParentKey = Model.Document;
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
            //return File(ms.ToArray(), "application/PDF");

        }



        #endregion
        // GET: Logistics/PODSend
        public ActionResult Index1(PODSendVM mModel)
        {
            TempData.Remove("ExistPOD");
            GetAllMenu(Session["ModuleName"].ToString());

            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;
            var Setup = ctxTFAT.PODSetup.FirstOrDefault();
            if (Setup != null)
            {
                mModel.GlobalSearch = Setup.GlobalSearch;
            }

            //var branchlist = GetChildGrp(mbranchcode);

            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                PODMaster pODMaster = ctxTFAT.PODMaster.Where(x => x.TableKey.ToString() == mModel.Document).FirstOrDefault();
                if (pODMaster.Task == "Branch")
                {
                    List<PODRel> pODDels = ctxTFAT.PODRel.Where(x => x.ParentKey == pODMaster.TableKey).ToList();
                    mModel.Task = pODMaster.Task;
                    mModel.Date = pODMaster.PODDate.ToShortDateString();
                    mModel.Time = pODMaster.PODTime;
                    mModel.BranchCode = pODDels.Select(x => x.ToBranch).FirstOrDefault();
                    mModel.BranchName = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.BranchCode).Select(x => x.Name).FirstOrDefault();
                    mModel.PODNo = pODMaster.PODNo.Value;

                    var ListOfTablekey = pODDels.Select(x => x.TableKey).ToList();
                    var podCheck = ctxTFAT.PODRel.Where(x => ListOfTablekey.Contains(x.PODRefTablekey)).FirstOrDefault();
                    if (podCheck != null)
                    {
                        mModel.BlockPOD = true;
                    }

                    List<ListOfPod> listOfPods = new List<ListOfPod>();
                    foreach (var item in pODDels)
                    {
                        LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey == item.LRRefTablekey).FirstOrDefault();
                        ListOfPod listOfPod = new ListOfPod
                        {
                            //PODNO = item.ChildNo,
                            Lrno = item.LrNo.ToString(),
                            LRTime = lRMaster.Time,
                            LRDate = lRMaster.BookDate.ToShortDateString(),
                            ConsignerName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.RecCode).Select(x => x.Name).FirstOrDefault(),
                            ConsigneeName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.SendCode).Select(x => x.Name).FirstOrDefault(),
                            FromName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Source).Select(x => x.Name).FirstOrDefault(),
                            ToName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name).FirstOrDefault(),
                            //Remark = item.PODRemark,
                            PODRefTablekey = item.PODRefTablekey,
                            LRRefTablekey = lRMaster.TableKey.ToString(),
                            Authorise = item.AUTHORISE.Substring(0, 1)
                        };
                        listOfPods.Add(listOfPod);
                    }
                    TempData["ExistPOD"] = listOfPods;
                    mModel.PODSendList = listOfPods;

                }
                else if (pODMaster.Task == "File")
                {
                    List<PODRel> pODDels = ctxTFAT.PODRel.Where(x => x.ParentKey == pODMaster.TableKey).ToList();
                    mModel.Task = pODMaster.Task;
                    mModel.Date = pODMaster.PODDate.ToShortDateString();
                    mModel.Time = pODMaster.PODTime;
                    mModel.BranchCode = pODDels.Select(x => x.ToBranch).FirstOrDefault();
                    mModel.BranchName = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.BranchCode).Select(x => x.Name).FirstOrDefault();
                    mModel.PODNo = pODMaster.PODNo.Value;

                    var ListOfTablekey = pODDels.Select(x => x.TableKey).ToList();
                    var podCheck = ctxTFAT.PODRel.Where(x => ListOfTablekey.Contains(x.PODRefTablekey)).FirstOrDefault();
                    if (podCheck != null)
                    {
                        mModel.BlockPOD = true;
                    }

                    List<ListOfPod> listOfPods = new List<ListOfPod>();
                    foreach (var item in pODDels)
                    {
                        LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey == item.LRRefTablekey).FirstOrDefault();
                        ListOfPod listOfPod = new ListOfPod
                        {
                            //PODNO = item.ChildNo,
                            Lrno = item.LrNo.ToString(),
                            LRTime = lRMaster.Time,
                            LRDate = lRMaster.BookDate.ToShortDateString(),
                            ConsignerName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.RecCode).Select(x => x.Name).FirstOrDefault(),
                            ConsigneeName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.SendCode).Select(x => x.Name).FirstOrDefault(),
                            FromName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Source).Select(x => x.Name).FirstOrDefault(),
                            ToName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name).FirstOrDefault(),
                            //Remark = item.PODRemark,
                            PODRefTablekey = item.PODRefTablekey,
                            LRRefTablekey = lRMaster.TableKey.ToString(),
                            Authorise = item.AUTHORISE.Substring(0, 1)
                        };
                        listOfPods.Add(listOfPod);
                    }
                    TempData["ExistPOD"] = listOfPods;
                    mModel.PODSendList = listOfPods;
                }
                else if (pODMaster.Task == "Customer")
                {
                    List<PODRel> pODDels = ctxTFAT.PODRel.Where(x => x.ParentKey == pODMaster.TableKey).ToList();
                    mModel.Task = pODMaster.Task;
                    mModel.Date = pODMaster.PODDate.ToShortDateString();
                    mModel.Time = pODMaster.PODTime;
                    mModel.BranchCode = pODDels.Select(x => x.ToBranch).FirstOrDefault();
                    mModel.BranchName = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.BranchCode).Select(x => x.Name).FirstOrDefault();
                    mModel.PODNo = pODMaster.PODNo.Value;
                    mModel.CustoCode = pODMaster.CustCode;
                    mModel.CustoName = ctxTFAT.CustomerMaster.Where(x => x.Code == pODMaster.CustCode).Select(x => x.Name).FirstOrDefault();

                    var ListOfTablekey = pODDels.Select(x => x.TableKey).ToList();
                    var podCheck = ctxTFAT.PODRel.Where(x => ListOfTablekey.Contains(x.PODRefTablekey)).FirstOrDefault();
                    if (podCheck != null)
                    {
                        mModel.BlockPOD = true;
                    }

                    List<ListOfPod> listOfPods = new List<ListOfPod>();
                    foreach (var item in pODDels)
                    {
                        LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.LrNo == item.LrNo).FirstOrDefault();
                        ListOfPod listOfPod = new ListOfPod
                        {
                            //PODNO = item.ChildNo,
                            Lrno = item.LrNo.ToString(),
                            LRTime = lRMaster.Time,
                            LRDate = lRMaster.BookDate.ToShortDateString(),
                            ConsignerName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.RecCode).Select(x => x.Name).FirstOrDefault(),
                            ConsigneeName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.SendCode).Select(x => x.Name).FirstOrDefault(),
                            FromName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Source).Select(x => x.Name).FirstOrDefault(),
                            ToName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name).FirstOrDefault(),
                            //Remark = item.PODRemark,
                            PODRefTablekey = item.PODRefTablekey,
                            LRRefTablekey = lRMaster.TableKey.ToString(),
                            Authorise = item.AUTHORISE.Substring(0, 1)
                        };
                        listOfPods.Add(listOfPod);
                    }
                    TempData["ExistPOD"] = listOfPods;
                    mModel.PODSendList = listOfPods;
                }
            }
            else
            {
                mModel.Date = DateTime.Now.ToShortDateString();
                mModel.Time = DateTime.Now.ToString("HH:mm");
                mModel.Task = "Branch";

                #region POD LIst
                List<ListOfPod> listOfPods = new List<ListOfPod>();
                mModel.PODSendList = listOfPods;
                #endregion
            }
            return View(mModel);
        }
        public void DeUpdate(PODSendVM mModel)
        {
            var PODMaster = ctxTFAT.PODMaster.Where(x => x.TableKey.ToString() == mModel.Document).FirstOrDefault();
            var PODRels = ctxTFAT.PODRel.Where(x => x.ParentKey == PODMaster.TableKey).ToList();
            ctxTFAT.PODRel.RemoveRange(PODRels);
            ctxTFAT.SaveChanges();
        }
        public ActionResult SaveData(PODSendVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        var Msg = DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Msg;
                    }
                    if (mModel.Mode == "Edit")
                    {
                        var Demo = ctxTFAT.PODMaster.Where(x => x.TableKey.ToString() == mModel.Document).FirstOrDefault();
                        if (mbranchcode != Demo.CurrentBranch)
                        {
                            transaction.Rollback();
                            return Json(new { Message = "Login Branch Does Not Match With Documnet Branch.", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    PODMaster pODMaster = new PODMaster();
                    if (mModel.Task == "Branch")
                    {
                        bool mAdd = true;
                        if (ctxTFAT.PODMaster.Where(x => x.TableKey.ToString() == mModel.Document).FirstOrDefault() != null)
                        {
                            pODMaster = ctxTFAT.PODMaster.Where(x => x.TableKey.ToString() == mModel.Document).FirstOrDefault();
                            mAdd = false;
                            DeUpdate(mModel);
                        }

                        if (mAdd)
                        {
                            pODMaster.CreateDate = DateTime.Now;
                            pODMaster.PODNo = Convert.ToInt32(GetNewCode());
                            pODMaster.CurrentBranch = mbranchcode;
                            pODMaster.FromBranch = mbranchcode;
                            pODMaster.Task = "Branch";
                            pODMaster.SendReceive = "S";
                            pODMaster.ModuleName = "POD Send";
                            pODMaster.Prefix = mperiod;
                            pODMaster.TableKey = mbranchcode + "PODS0" + mperiod.Substring(0, 2) + "001" + pODMaster.PODNo;
                            pODMaster.ParentKey = mbranchcode + "PODS0" + mperiod.Substring(0, 2) + pODMaster.PODNo;
                        }
                        pODMaster.PODDate = ConvertDDMMYYTOYYMMDD(mModel.Date);
                        pODMaster.PODTime = mModel.Time;
                        pODMaster.AUTHIDS = muserid;
                        pODMaster.AUTHORISE = mauthorise;
                        pODMaster.ENTEREDBY = muserid;
                        pODMaster.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                        if (mAdd)
                        {
                            ctxTFAT.PODMaster.Add(pODMaster);
                        }
                        else
                        {
                            ctxTFAT.Entry(pODMaster).State = EntityState.Modified;
                        }
                        int Sno = 1;
                        foreach (var item in mModel.PODSendList)
                        {
                            PODRel pODRel = new PODRel();
                            pODRel.Task = "Branch";
                            pODRel.SendReceive = "S";
                            pODRel.PODNo = pODMaster.PODNo.Value;
                            //pODRel.ChildNo = item.PODNO;
                            pODRel.LrNo = Convert.ToInt32(item.Lrno);
                            pODRel.FromBranch = mbranchcode;
                            pODRel.ToBranch = mModel.BranchCode;
                            pODRel.Sno = Sno;
                            pODRel.TableKey = mbranchcode + "PODRL" + mperiod.Substring(0, 2) + Sno.ToString("D3") + pODMaster.PODNo;
                            pODRel.ParentKey = pODMaster.TableKey;
                            pODRel.LRRefTablekey = item.LRRefTablekey;
                            pODRel.PODRefTablekey = item.PODRefTablekey;
                            if (String.IsNullOrEmpty(item.PODRefTablekey))
                            {
                                var GetLastPOD = ctxTFAT.PODRel.OrderByDescending(x => x.RECORDKEY).Where(x => x.LRRefTablekey == item.LRRefTablekey && (x.SendReceive == "R" || x.SendReceive == "S")).FirstOrDefault();
                                if (GetLastPOD != null )
                                {
                                    pODRel.PODRefTablekey = GetLastPOD.TableKey;
                                }
                            }
                            //pODRel.PODRefTablekey = item.PODRefTablekey;
                            pODRel.AUTHIDS = muserid;
                            pODRel.AUTHORISE = mauthorise;
                            pODRel.ENTEREDBY = muserid;
                            pODRel.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                            pODRel.Prefix = mperiod;
                            ctxTFAT.PODRel.Add(pODRel);
                            PODNotification(pODMaster, pODRel, false);
                            ++Sno;
                        }
                        UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header + "-Branch", pODMaster.ParentKey, pODMaster.PODDate, 0, "", "Save PODNO :" + pODMaster.PODNo, "NA");
                    }
                    else if (mModel.Task == "File")
                    {
                        bool mAdd = true;
                        if (ctxTFAT.PODMaster.Where(x => x.PODNo.ToString() == mModel.Document).FirstOrDefault() != null)
                        {
                            pODMaster = ctxTFAT.PODMaster.Where(x => x.TableKey.ToString() == mModel.Document).FirstOrDefault();
                            mAdd = false;
                            DeUpdate(mModel);
                        }

                        if (mAdd)
                        {
                            pODMaster.CreateDate = DateTime.Now;
                            pODMaster.PODNo = Convert.ToInt32(GetNewCode());
                            pODMaster.Prefix = mperiod;
                            pODMaster.TableKey = mbranchcode + "PODS0" + mperiod.Substring(0, 2) + "001" + pODMaster.PODNo;
                            pODMaster.ParentKey = mbranchcode + "PODS0" + mperiod.Substring(0, 2) + pODMaster.PODNo;
                        }
                        pODMaster.PODDate = ConvertDDMMYYTOYYMMDD(mModel.Date);
                        pODMaster.PODTime = mModel.Time;
                        pODMaster.CurrentBranch = mbranchcode;
                        pODMaster.FromBranch = mbranchcode;
                        pODMaster.Task = "File";
                        pODMaster.SendReceive = "F";
                        pODMaster.ModuleName = "POD File";

                        pODMaster.AUTHIDS = muserid;
                        pODMaster.AUTHORISE = mauthorise;
                        pODMaster.ENTEREDBY = muserid;
                        pODMaster.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                        if (mAdd)
                        {
                            ctxTFAT.PODMaster.Add(pODMaster);
                        }
                        else
                        {
                            ctxTFAT.Entry(pODMaster).State = EntityState.Modified;
                        }

                        int Sno = 0;
                        //var DistinctPOd = mModel.PODSendList.Select(x => x.PODNO).Distinct().ToList();
                        foreach (var item1 in mModel.PODSendList)
                        {
                            //PODRel OldPodRel = ctxTFAT.PODRel.Where(x => x.TableKey == item1.PODRefTablekey).FirstOrDefault();
                            Sno += 1;
                            PODRel pODRel = new PODRel();
                            pODRel.Sno = Sno;
                            pODRel.Task = "File";
                            pODRel.SendReceive = "F";
                            pODRel.PODNo = pODMaster.PODNo.Value;
                            //pODRel.ChildNo = item.PODNO;
                            pODRel.LrNo = Convert.ToInt32(item1.Lrno);
                            pODRel.FromBranch = mbranchcode;
                            pODRel.ToBranch = mbranchcode;
                            pODRel.TableKey = mbranchcode + "PODRL" + mperiod.Substring(0, 2) + Sno.ToString("D3") + pODMaster.PODNo.Value;
                            pODRel.ParentKey = pODMaster.TableKey;
                            pODRel.LRRefTablekey = item1.LRRefTablekey;
                            pODRel.PODRefTablekey = item1.PODRefTablekey;
                            if (String.IsNullOrEmpty(item1.PODRefTablekey))
                            {
                                var GetLastPOD = ctxTFAT.PODRel.OrderByDescending(x => x.RECORDKEY).Where(x => x.LRRefTablekey == item1.LRRefTablekey && (x.SendReceive == "R" || x.SendReceive == "S")).FirstOrDefault();
                                if (GetLastPOD != null )
                                {
                                    pODRel.PODRefTablekey = GetLastPOD.TableKey;
                                }
                            }
                            pODRel.AUTHIDS = muserid;
                            pODRel.AUTHORISE = mauthorise;
                            pODRel.ENTEREDBY = muserid;
                            pODRel.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                            pODRel.Prefix = mperiod;
                            ctxTFAT.PODRel.Add(pODRel);
                        }

                        UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header + "-File", pODMaster.ParentKey, pODMaster.PODDate, 0, "", "Save PODNO :" + pODMaster.PODNo, "NA");
                    }
                    else
                    {
                        bool mAdd = true;
                        if (ctxTFAT.PODMaster.Where(x => x.PODNo.ToString() == mModel.Document).FirstOrDefault() != null)
                        {
                            pODMaster = ctxTFAT.PODMaster.Where(x => x.PODNo.ToString() == mModel.Document).FirstOrDefault();
                            mAdd = false;
                            DeUpdate(mModel);
                        }

                        if (mAdd)
                        {
                            pODMaster.CreateDate = DateTime.Now;
                            pODMaster.PODNo = Convert.ToInt32(GetNewCode());
                            pODMaster.Prefix = mperiod;
                            pODMaster.TableKey = mbranchcode + "PODS0" + mperiod.Substring(0, 2) + "001" + pODMaster.PODNo;
                            pODMaster.ParentKey = mbranchcode + "PODS0" + mperiod.Substring(0, 2) + pODMaster.PODNo;
                        }
                        pODMaster.PODDate = ConvertDDMMYYTOYYMMDD(mModel.Date);
                        pODMaster.PODTime = mModel.Time;
                        pODMaster.CurrentBranch = mbranchcode;
                        pODMaster.FromBranch = mbranchcode;
                        pODMaster.CustCode = mModel.CustoCode;
                        pODMaster.Task = "Customer";
                        pODMaster.SendReceive = "C";
                        pODMaster.ModuleName = "POD Send Customer   ";
                        pODMaster.AUTHIDS = muserid;
                        pODMaster.AUTHORISE = mauthorise;
                        pODMaster.ENTEREDBY = muserid;
                        pODMaster.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                        if (mAdd)
                        {
                            ctxTFAT.PODMaster.Add(pODMaster);
                        }
                        else
                        {
                            ctxTFAT.Entry(pODMaster).State = EntityState.Modified;
                        }
                        int Sno = 0;
                        //var DistinctPOd = mModel.PODSendList.Select(x => x.PODNO).Distinct().ToList();
                        foreach (var item1 in mModel.PODSendList)
                        {
                            //var item = mModel.PODSendList.Where(x => x.PODNO == item1).FirstOrDefault();
                            //PODRel OldPodRel = ctxTFAT.PODRel.Where(x => x.TableKey == item1.PODRefTablekey).FirstOrDefault();
                            Sno += 1;
                            PODRel pODRel = new PODRel();
                            pODRel.Sno = Sno;
                            pODRel.Task = "Customer";
                            pODRel.SendReceive = "C";
                            pODRel.PODNo = pODMaster.PODNo.Value;
                            //pODRel.ChildNo = item.PODNO;
                            pODRel.LrNo = Convert.ToInt32(item1.Lrno);
                            pODRel.FromBranch = mbranchcode;
                            pODRel.ToBranch = mbranchcode;
                            pODRel.TableKey = mbranchcode + "PODRL" + mperiod.Substring(0, 2) + Sno.ToString("D3") + pODMaster.PODNo;
                            pODRel.ParentKey = pODMaster.TableKey;
                            pODRel.LRRefTablekey = item1.LRRefTablekey;
                            pODRel.PODRefTablekey = item1.PODRefTablekey;
                            if (String.IsNullOrEmpty(item1.PODRefTablekey))
                            {
                                var GetLastPOD = ctxTFAT.PODRel.OrderByDescending(x => x.RECORDKEY).Where(x => x.LRRefTablekey == item1.LRRefTablekey && (x.SendReceive == "R" || x.SendReceive == "S")).FirstOrDefault();
                                if (GetLastPOD != null)
                                {
                                    pODRel.PODRefTablekey = GetLastPOD.TableKey;
                                }
                            }
                            pODRel.AUTHIDS = muserid;
                            pODRel.AUTHORISE = mauthorise;
                            pODRel.ENTEREDBY = muserid;
                            pODRel.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                            pODRel.Prefix = mperiod;
                            ctxTFAT.PODRel.Add(pODRel);
                        }

                        UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header + "-Customer", pODMaster.ParentKey, pODMaster.PODDate, 0, "", "Save PODNO :" + pODMaster.PODNo, "NA");
                    }

                    ctxTFAT.SaveChanges();

                    //mnewrecordkey = Convert.ToInt32(mobj.RecordKey);
                    //string mNewCode = "";
                    //mNewCode = mobj.Code;
                    transaction.Commit();
                    transaction.Dispose();
                    //UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "");
                }

                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
        }
        private ActionResult DeleteStateMaster(PODSendVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new { Message = "Code not Entered..", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
            }

            PODMaster pODMaster = ctxTFAT.PODMaster.Where(x => x.TableKey.ToString() == mModel.Document).FirstOrDefault();
            var POdRel = ctxTFAT.PODRel.Where(x => x.ParentKey == pODMaster.TableKey).ToList();

            //foreach (var item in POdRel)
            //{
            //    ctxTFAT.PODRel.Remove(item);
            //    PODRel OldPOdREl = ctxTFAT.PODRel.Where(x => x.TableKey.ToString() == item.ParentKey).FirstOrDefault();
            //    if (OldPOdREl != null)
            //    {
            //        ctxTFAT.Entry(OldPOdREl).State = EntityState.Modified;
            //    }

            //}

            ctxTFAT.PODMaster.Remove(pODMaster);
            ctxTFAT.PODRel.RemoveRange(POdRel);
            ctxTFAT.SaveChanges();
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header + "-Branch", pODMaster.ParentKey, pODMaster.PODDate, 0, "", "Delete PODNO :" + pODMaster.RECORDKEY, "NA");
            return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
        }

        //Search IN Branch Tab
        public ActionResult SearchLRNOInBranch(PODSendVM mModel)
        {
            ListOfPod listOf = new ListOfPod();
            List<ListOfPod> ExistingPOD = TempData.Peek("ExistPOD") as List<ListOfPod>;
            if (ExistingPOD == null)
            {
                ExistingPOD = new List<ListOfPod>();
            }
            if (mModel.PODSendList == null)
            {
                mModel.PODSendList = new List<ListOfPod>();
            }

            if (ExistingPOD.Where(x => x.LRRefTablekey == mModel.SearchLrnoForPodSend).FirstOrDefault() == null)
            {
                LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == mModel.SearchLrnoForPodSend).FirstOrDefault();
                if (lRMaster != null)
                {
                    var GetLastPOD = ctxTFAT.PODRel.OrderByDescending(x => x.RECORDKEY).Where(x => x.LRRefTablekey == lRMaster.TableKey && (x.SendReceive=="R" || x.SendReceive=="S")).FirstOrDefault();
                    if (GetLastPOD != null )
                    {
                        listOf.PODRefTablekey = GetLastPOD.TableKey;
                        listOf.PODNO = GetLastPOD.PODNo;
                        listOf.Lrno = GetLastPOD.LrNo.ToString();
                        listOf.LRRefTablekey = GetLastPOD.LRRefTablekey;
                        listOf.LRDate = lRMaster.BookDate.ToShortDateString();
                        listOf.LRTime = lRMaster.Time;
                        listOf.ConsignerName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.RecCode).Select(x => x.Name).FirstOrDefault();
                        listOf.ConsigneeName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.SendCode).Select(x => x.Name).FirstOrDefault();
                        listOf.FromName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Source).Select(x => x.Name).FirstOrDefault();
                        listOf.ToName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name).FirstOrDefault();
                        listOf.Remark = GetLastPOD.RecePODRemark;
                        ExistingPOD.Add(listOf);
                    }
                    else
                    {
                        listOf.PODRefTablekey = "";
                        listOf.PODNO = 0;
                        listOf.Lrno = lRMaster.LrNo.ToString();
                        listOf.LRRefTablekey = lRMaster.TableKey;
                        listOf.LRDate = lRMaster.BookDate.ToShortDateString();
                        listOf.LRTime = lRMaster.Time;
                        listOf.ConsignerName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.RecCode).Select(x => x.Name).FirstOrDefault();
                        listOf.ConsigneeName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.SendCode).Select(x => x.Name).FirstOrDefault();
                        listOf.FromName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Source).Select(x => x.Name).FirstOrDefault();
                        listOf.ToName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name).FirstOrDefault();
                        listOf.Remark = lRMaster.Narr;
                        ExistingPOD.Add(listOf);
                    }
                }
            }
            
            TempData["ExistPOD"] = ExistingPOD;
            var html = ViewHelper.RenderPartialView(this, "PODGrid", ExistingPOD);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GridView(PODSendVM mModel)
        {
            List<ListOfPod> sendVMs = new List<ListOfPod>();
            List<ListOfPod> ExistPOD = TempData.Peek("ExistPOD") as List<ListOfPod>;
            if (ExistPOD == null)
            {
                ExistPOD = new List<ListOfPod>();
            }
            ExistPOD.AddRange(mModel.PODSendList);
            TempData["ExistPOD"] = ExistPOD;
            var html = ViewHelper.RenderPartialView(this, "PODGrid", ExistPOD);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteLRFromTempData(string TableKey)
        {
            string Msg = "Sucess";
            List<ListOfPod> ExistPOD = TempData.Peek("ExistPOD") as List<ListOfPod>;
            if (ExistPOD == null)
            {
                ExistPOD = new List<ListOfPod>();
            }
            var DeletePOD1 = ExistPOD.Where(x => x.PODRefTablekey == TableKey).FirstOrDefault();
            if (DeletePOD1 != null)
            {
                ExistPOD.Remove(DeletePOD1);
            }
            TempData["ExistPOD"] = ExistPOD;
            return Json(new { Msg = Msg }, JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        public ActionResult FetchDocumentList(LorryReceiptQueryVM Model)
        {
            var mlrmaster = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == Model.Lrno).Select(x => x).ToList();
            if (mlrmaster == null)
            {
                Model.Status = "ValidError";
                Model.Message = "Consignment Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            if (mlrmaster.Count() == 0)
            {
                Model.Status = "ValidError";
                Model.Message = "Consignment Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            else if (mlrmaster.Count() == 1)
            {
                Model.Status = "Processed";
                Model.Message = mlrmaster.Select(x => x.TableKey).FirstOrDefault();
                return Json(Model, JsonRequestBehavior.AllowGet);
            }

            var html = ViewHelper.RenderPartialView(this, "ConsignmentList", Model);
            return Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
        }

    }
}