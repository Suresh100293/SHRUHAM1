using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class AttachmentController : BaseController
    {
        string attachmentPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentPath"].ToString()) == true ? "" : ConfigurationManager.AppSettings["AttachmentPath"].ToString();
        private static string mAUTHORISE = "A00";


        public int GetNewAttachCode()
        {
            string Code = ctxTFAT.Attachment.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).Take(1).FirstOrDefault();
            if (String.IsNullOrEmpty(Code))
            {
                return 100000;
            }
            else
            {
                return Convert.ToInt32(Code) + 1;
            }
        }

        // GET: Logistics/Attachment
        public ActionResult Index()
        {
            return View();
        }

        #region attachment In Modal USe
        //Modal Show With Attachment Of Document
        public ActionResult UploadFile(AttachmentVM Model)
        {
            List<AttachmentVM> attachments = new List<AttachmentVM>();
            if (Model.Type == "Vehic" || Model.Type == "Drive")
            {
                if (Model.Mode != "Add")
                {
                    attachments = GetAttachmentListInEditWithoutModal(Model);
                }
                else
                {
                    attachments = (List<AttachmentVM>)Session["TempAttach"];
                }
            }
            else
            {
                if (Session["TempAttach"] != null)
                {
                    attachments = (List<AttachmentVM>)Session["TempAttach"];
                    if (attachments.Count() == 0)
                    {
                        attachments = GetAttachmentListInEdit(Model);
                    }
                }
                else
                {
                    attachments = GetAttachmentListInEdit(Model);
                }
            }

            if (attachments == null)
            {
                attachments = new List<AttachmentVM>();
            }
            Session["TempAttach"] = attachments;
            Model.DocumentList = attachments.OrderBy(x => x.tempId).ToList();

            var html = ViewHelper.RenderPartialView(this, "~/Areas/Logistics/Views/Attachment/AttachmentDocument.cshtml", Model);
            var jsonResult = Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public List<AttachmentVM> GetAttachmentListInEdit(AttachmentVM Model)
        {

            List<AttachmentVM> AttachmentList = new List<AttachmentVM>();
            List<Attachment> AttchList = new List<Attachment>();
            if (Model.Type == "Alert")
            {
                AttchList = ctxTFAT.Attachment.Where(x => x.Srl == Model.Srl && x.Type == Model.Type).ToList();
            }
            else if (Model.Type == "BLSMT")
            {
                AttchList = ctxTFAT.Attachment.Where(x => x.ParentKey == Model.Srl && x.Prefix == mperiod && x.Type == Model.Type).ToList();
            }
            else
            {
                AttchList = ctxTFAT.Attachment.Where(x => x.ParentKey == Model.Srl && x.Type != "Alert").ToList();
            }
            //var docdetail = ctxTFAT.Attachment.Where(x => x.Srl == Model.Srl && (x.Type == Model.Type) && x.Type != "Alert").ToList();
            bool DeleteEveyOne = false;
            bool DeleteEnteredOnly = false;
            var Setup = ctxTFAT.AttachmentSetup.FirstOrDefault();
            if (Setup != null)
            {
                if (Setup.DeleteEveryOne)
                {
                    DeleteEveyOne = true;
                }
                else if (Setup.DeleteEnteredOnly)
                {
                    DeleteEnteredOnly = true;
                }
            }
            int K = 1;
            foreach (var item in AttchList)
            {
                bool FoundAttachment = false;
                AttachmentVM AttachmentVM = new AttachmentVM();
                if (System.IO.File.Exists(item.FilePath))
                {
                    FoundAttachment = true;
                }
                if (FoundAttachment)
                {
                    AttachmentVM.DocDate = item.DocDate.ToShortDateString();
                    AttachmentVM.FileName = Path.GetFileName(item.FilePath);
                    AttachmentVM.Srl = item.Srl;
                    AttachmentVM.Code = item.Code;
                    AttachmentVM.TableKey = item.TableKey;
                    AttachmentVM.ParentKey = item.ParentKey;
                    AttachmentVM.Type = item.Type;
                    AttachmentVM.tempId = K++;
                    AttachmentVM.SrNo = item.Sno;
                    AttachmentVM.Path = item.FilePath;
                    AttachmentVM.FileContent = Path.GetExtension(item.FilePath);
                    AttachmentVM.ImageStr = Convert.ToBase64String(System.IO.File.ReadAllBytes(item.FilePath));
                    AttachmentVM.ImageData = Convert.FromBase64String(AttachmentVM.ImageStr);
                    AttachmentVM.tempIsDeleted = false;
                    AttachmentVM.HideDelete = Model.HideDelete;
                    AttachmentVM.ExternalAttach = item.ExternalAttach;
                    AttachmentVM.RefCode = item.RefDocNo;
                    AttachmentVM.RefType = item.RefType;
                    AttachmentVM.DocType = item.DocType;
                    AttachmentVM.ENTEREDBY = item.ENTEREDBY;
                    if (DeleteEveyOne)
                    {
                        AttachmentVM.AUTHIDS = "Y";
                    }
                    else
                    {
                        AttachmentVM.AUTHIDS = item.ENTEREDBY.Trim().ToLower() == muserid.Trim().ToLower() ? "Y" : "N";
                    }
                    AttachmentList.Add(AttachmentVM);
                }
            }


            return AttachmentList;
        }

        [HttpPost]//upload Attachement
        public ActionResult AttachDocument(AttachmentVM mModel)
        {
            List<AttachmentVM> DocList = new List<AttachmentVM>();
            if (Session["TempAttach"] != null)
            {
                DocList = (List<AttachmentVM>)Session["TempAttach"];
            }
            //int n = DocList.Count() + 1;
            byte[] fileData = null;
            for (int i = 0; i < Request.Files.Count; i++)
            {
                AttachmentVM Model = new AttachmentVM();
                var file = Request.Files[i];
                var fileName = Path.GetFileName(file.FileName);

                MemoryStream target = new MemoryStream();
                using (var binaryReader = new BinaryReader(Request.Files[i].InputStream))
                {
                    fileData = binaryReader.ReadBytes(Request.Files[i].ContentLength);
                }

                Model.ImageData = fileData;
                string FileString = Convert.ToBase64String(fileData);
                file.InputStream.CopyTo(target);
                Model.DocDate = DateTime.Now.ToShortDateString();
                Model.FileContent = file.ContentType;
                Model.ImageStr = FileString;
                Model.FileName = fileName;
                Model.ContentType = file.ContentType;
                Model.tempId = DocList.OrderByDescending(x => x.tempId).Select(x => x.tempId).FirstOrDefault() + 1;
                Model.Srl = Model.Srl;
                Model.Type = mModel.Type;
                Model.RefType = mModel.RefType;
                Model.Mode = mModel.Mode;
                Model.ENTEREDBY = muserid;
                Model.DocType = "Other";

                if (mModel.Mode != "Add")
                {
                    if (mModel.Type == "LR000")
                    {
                        var mlist = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString().Trim() == mModel.Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.Srl = mlist.LrNo.ToString();
                            Model.ParentKey = mlist.TableKey;
                        }
                    }
                    if (mModel.Type == "PCK00")
                    {
                        var mlist = ctxTFAT.PickOrder.Where(x => x.OrderNo.ToString().Trim() == mModel.Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.ParentKey = mlist.OrderNo.ToString();
                        }
                    }
                    else if (mModel.Type == "FM000" || mModel.Type == "FMH00")
                    {
                        var mlist = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString().Trim() == mModel.Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.Srl = mlist.FmNo.ToString();
                            Model.ParentKey = mlist.TableKey;
                        }
                    }
                    else if (mModel.Type == "Vehic")
                    {
                        var mlist = ctxTFAT.VehicleMaster.Where(x => x.Code.ToString().Trim() == mModel.Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.Srl = mlist.Code.ToString();
                            Model.ParentKey = mlist.Code;
                        }
                    }
                    else if (mModel.Type == "Maste")
                    {
                        var mlist = ctxTFAT.Master.Where(x => x.Code.ToString().Trim() == mModel.Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.Srl = mlist.Code;
                            Model.ParentKey = mlist.Code;
                        }
                    }
                    else if (mModel.Type == "CUSMA")
                    {
                        var mlist = ctxTFAT.CustomerMaster.Where(x => x.Code.ToString().Trim() == mModel.Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.Srl = mlist.Code;
                            Model.ParentKey = mlist.Code;
                        }
                    }
                    else if (mModel.Type == "Drive")
                    {
                        var mlist = ctxTFAT.DriverMaster.Where(x => x.Code.ToString().Trim() == mModel.Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.Srl = mlist.Code;
                            Model.ParentKey = mlist.Code;
                        }
                    }
                    else if (mModel.Type == "PUR00" || mModel.Type == "CPH00")
                    {
                        var mlist = ctxTFAT.Purchase.Where(x => x.TableKey.ToString().Trim() == mModel.Srl && x.Type == mModel.Type).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.Srl = mlist.Srl.ToString();
                            Model.ParentKey = mlist.TableKey;
                        }
                    }
                    else if (mModel.Type == "BLSMT")
                    {
                        var mlist = ctxTFAT.BillSubmission.Where(x => x.DocNo.ToString().Trim() == mModel.Srl && x.Prefix == mperiod).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.Srl = mlist.DocNo.ToString();
                            Model.ParentKey = mlist.DocNo;
                        }
                    }
                    else if (mModel.Type == "SLR00" || mModel.Type == "SLW00" || mModel.Type == "CMM00")
                    {
                        var mlist = ctxTFAT.Sales.Where(x => x.TableKey.ToString().Trim() == mModel.Srl && x.Type == mModel.Type).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.Srl = mlist.Srl;
                            Model.ParentKey = mlist.TableKey;
                        }
                    }
                    else if (mModel.Type == "Alert")
                    {
                        var mlist = ctxTFAT.AlertNoteMaster.Where(x => x.DocNo.ToString().Trim() == mModel.Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.Srl = mlist.DocNo;
                            Model.ParentKey = mlist.TableKey;
                        }
                    }
                    else if (mModel.Type == "CPO00" || mModel.Type == "COT00")
                    {
                        var mlist = ctxTFAT.RelateData.Where(x => x.ParentKey.ToString().Trim() == mModel.Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.Srl = mlist.Srl.ToString();
                            Model.ParentKey = mlist.ParentKey;
                        }
                    }
                    var AttachCode = GetNewAttachCode();
                    string directoryPath = attachmentPath + Model.ParentKey + "/" + Model.FileName;
                    Directory.CreateDirectory(attachmentPath + Model.ParentKey);
                    var Count = ctxTFAT.Attachment.Where(x => x.ParentKey == Model.ParentKey).ToList().Count() + 1;
                    Attachment att = new Attachment();
                    att.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                    att.AUTHIDS = muserid;
                    att.AUTHORISE = mAUTHORISE;
                    att.Branch = mbranchcode;
                    att.Code = AttachCode.ToString();
                    att.ENTEREDBY = muserid;
                    att.FilePath = directoryPath;
                    att.LASTUPDATEDATE = DateTime.Now;
                    att.LocationCode = Model.LocationCode;
                    att.Prefix = mperiod;
                    att.Sno = Count;
                    Model.tempId = Count;
                    att.Srl = Model.Srl;
                    att.SrNo = Count;
                    att.TableKey = mModel.Type + mperiod.Substring(0, 2) + Count.ToString("D3") + Model.Srl;
                    Model.TableKey = mModel.Type + mperiod.Substring(0, 2) + Count.ToString("D3") + Model.Srl;
                    att.ParentKey = Model.ParentKey;
                    att.Type = mModel.Type;
                    att.RefType = mModel.RefType;
                    att.CompCode = mcompcode;
                    att.DocType = "Other";
                    att.ExternalAttach = false;
                    ctxTFAT.Attachment.Add(att);
                    ctxTFAT.SaveChanges();
                    System.IO.File.WriteAllBytes(directoryPath, fileData);
                }



                DocList.Add(Model);
            }
            Session["TempAttach"] = DocList;

            DocList = DocList.ToList();

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Logistics/Views/Attachment/AttachmentDocument.cshtml", new AttachmentVM() { RefType = mModel.RefType, Type = mModel.Type, Srl = mModel.TableKey, Mode = mModel.Mode, DocumentList = DocList });
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        [HttpPost]//Delete Attachement
        public ActionResult DeleteUploadFile(AttachmentVM Model)
        {
            List<AttachmentVM> DocList = new List<AttachmentVM>();
            if (Session["TempAttach"] != null)
            {
                DocList = (List<AttachmentVM>)Session["TempAttach"];
            }

            Model.DocumentList = DocList.Where(x => x.tempId != Model.tempId).Select(x => x).ToList();
            Session["TempAttach"] = Model.DocumentList;
            if (Model.Mode != "Add")
            {
                var Delete = DocList.Where(x => x.tempId == Model.tempId).Select(x => x).FirstOrDefault();
                var attachment = ctxTFAT.Attachment.Where(x => x.Type == Model.Type && x.TableKey == Delete.TableKey).ToList();
                //var TempPath = attachment.Select(x => x.FilePath).FirstOrDefault();
                //var count = ctxTFAT.Attachment.Where(x => x.Type == Model.Type && x.Srl == Model.Srl && x.FilePath == TempPath && x.TableKey != Delete.TableKey).Select(x => x).FirstOrDefault();
                //if (count == null)
                //{
                //    foreach (var item in attachment)
                //    {
                //        if (System.IO.File.Exists(item.FilePath))
                //        {
                //            System.IO.File.Delete(item.FilePath);
                //        }
                //    }
                //}

                ctxTFAT.Attachment.RemoveRange(attachment);
                ctxTFAT.SaveChanges();
            }
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Logistics/Views/Attachment/AttachmentDocument.cshtml", Model);
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        //Dowbload Attachement
        public FileResult Download(string tempId)
        {
            List<AttachmentVM> DocList = new List<AttachmentVM>();
            if (Session["TempAttach"] != null)
            {
                DocList = (List<AttachmentVM>)Session["TempAttach"];
            }
            var dwnfile = DocList.Where(x => x.tempId.ToString() == tempId).FirstOrDefault();

            string filename = dwnfile.FileName;
            byte[] fileBytes = Convert.FromBase64String(dwnfile.ImageStr);

            return File(fileBytes, ".jpg", filename);
        }
        #endregion


        #region attachment Without Modal (Partial View Specially For Vehicle && Driver)

        public List<AttachmentVM> GetAttachmentListInEditWithoutModal(AttachmentVM Model)
        {

            List<AttachmentVM> AttachmentList = new List<AttachmentVM>();
            List<Attachment> AttchList = new List<Attachment>();
            if (Model.Type == "Alert")
            {
                AttchList = ctxTFAT.Attachment.Where(x => x.Srl == Model.Srl && (x.Type == Model.Type)).ToList();
            }
            else
            {
                if (String.IsNullOrEmpty(Model.DocType))
                {
                    AttchList = ctxTFAT.Attachment.Where(x => x.ParentKey == Model.Srl && (x.Type == Model.Type) && x.Type != "Alert").ToList();
                }
                else
                {
                    AttchList = ctxTFAT.Attachment.Where(x => x.ParentKey == Model.Srl && x.DocType == Model.DocType && (x.Type == Model.Type) && x.Type != "Alert").ToList();
                }
            }
            //var docdetail = ctxTFAT.Attachment.Where(x => x.Srl == Model.Srl && (x.Type == Model.Type) && x.Type != "Alert").ToList();
            bool DeleteEveyOne = false;
            bool DeleteEnteredOnly = false;
            var Setup = ctxTFAT.AttachmentSetup.FirstOrDefault();
            if (Setup != null)
            {
                if (Setup.DeleteEveryOne)
                {
                    DeleteEveyOne = true;
                }
                else if (Setup.DeleteEnteredOnly)
                {
                    DeleteEnteredOnly = true;
                }
            }
            int K = 1;
            foreach (var item in AttchList)
            {
                bool FoundAttachment = false;
                AttachmentVM AttachmentVM = new AttachmentVM();
                if (System.IO.File.Exists(item.FilePath))
                {
                    FoundAttachment = true;
                }
                if (FoundAttachment)
                {
                    AttachmentVM.DocDate = item.DocDate.ToShortDateString();
                    AttachmentVM.FileName = Path.GetFileName(item.FilePath);
                    AttachmentVM.Srl = item.Srl;
                    AttachmentVM.Code = item.Code;
                    AttachmentVM.TableKey = item.TableKey;
                    AttachmentVM.ParentKey = item.ParentKey;
                    AttachmentVM.Type = item.Type;
                    AttachmentVM.tempId = K++;
                    AttachmentVM.SrNo = item.Sno;
                    AttachmentVM.Path = item.FilePath;
                    AttachmentVM.FileContent = Path.GetExtension(item.FilePath);
                    AttachmentVM.ImageStr = Convert.ToBase64String(System.IO.File.ReadAllBytes(item.FilePath));
                    AttachmentVM.ImageData = Convert.FromBase64String(AttachmentVM.ImageStr);
                    AttachmentVM.tempIsDeleted = false;
                    AttachmentVM.HideDelete = Model.HideDelete;
                    AttachmentVM.ExternalAttach = item.ExternalAttach;
                    AttachmentVM.RefCode = item.RefDocNo;
                    AttachmentVM.RefType = item.RefType;
                    AttachmentVM.DocType = item.DocType;
                    AttachmentVM.ENTEREDBY = item.ENTEREDBY;
                    if (DeleteEveyOne)
                    {
                        AttachmentVM.AUTHIDS = "Y";
                    }
                    else
                    {
                        AttachmentVM.AUTHIDS = item.ENTEREDBY.Trim().ToLower() == muserid.Trim().ToLower() ? "Y" : "N";
                    }
                    AttachmentList.Add(AttachmentVM);
                }
            }


            return AttachmentList;
        }

        //Without Modal Attachment View
        public ActionResult UploadFileWithoutModal(AttachmentVM Model)
        {
            List<AttachmentVM> attachments = new List<AttachmentVM>();
            if (Model.Mode != "Add")
            {
                attachments = GetAttachmentListInEditWithoutModal(Model);
            }
            else
            {
                attachments = (List<AttachmentVM>)Session["TempAttach"];
            }
            if (attachments == null)
            {
                attachments = new List<AttachmentVM>();
            }


            Session["TempAttach"] = attachments;
            attachments = attachments.Where(x => x.DocType == Model.DocType).ToList();
            Model.DocumentList = attachments.OrderBy(x => x.tempId).ToList();

            var html = ViewHelper.RenderPartialView(this, "~/Areas/Logistics/Views/Attachment/AttachmentDocumentWithoutModal.cshtml", Model);
            var jsonResult = Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        [HttpPost]//upload Attachement
        public ActionResult AttachDocumentWithoutModal(AttachmentVM mModel)
        {
            List<AttachmentVM> DocList = new List<AttachmentVM>();
            if (Session["TempAttach"] != null)
            {
                DocList = (List<AttachmentVM>)Session["TempAttach"];
            }
            int n = DocList.Count() + 1;
            byte[] fileData = null;
            for (int i = 0; i < Request.Files.Count; i++)
            {
                AttachmentVM Model = new AttachmentVM();
                var file = Request.Files[i];
                var fileName = Path.GetFileName(file.FileName);

                MemoryStream target = new MemoryStream();
                using (var binaryReader = new BinaryReader(Request.Files[i].InputStream))
                {
                    fileData = binaryReader.ReadBytes(Request.Files[i].ContentLength);
                }


                Model.ImageData = fileData;
                string FileString = Convert.ToBase64String(fileData);
                file.InputStream.CopyTo(target);
                Model.DocDate = DateTime.Now.ToShortDateString();
                Model.FileContent = file.ContentType;
                Model.ImageStr = FileString;
                Model.FileName = fileName;
                Model.ContentType = file.ContentType;
                Model.tempId = DocList.OrderByDescending(x => x.tempId).Select(x => x.tempId).FirstOrDefault() + 1;
                Model.Srl = mModel.Srl;
                Model.Type = mModel.Type;
                Model.RefType = mModel.RefType;
                Model.Mode = mModel.Mode;
                Model.ENTEREDBY = muserid;
                Model.DocType = mModel.DocType;
                if (mModel.Mode != "Add")
                {
                    if (mModel.Type == "LR000")
                    {
                        var mlist = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString().Trim() == mModel.Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.Srl = mlist.LrNo.ToString();
                            Model.ParentKey = mlist.TableKey;
                        }
                    }
                    if (mModel.Type == "PCK00")
                    {
                        var mlist = ctxTFAT.PickOrder.Where(x => x.OrderNo.ToString().Trim() == mModel.Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.ParentKey = mlist.OrderNo.ToString();
                        }
                    }
                    else if (mModel.Type == "FM000" || mModel.Type == "FMH00")
                    {
                        var mlist = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString().Trim() == mModel.Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.Srl = mlist.FmNo.ToString();
                            Model.ParentKey = mlist.TableKey;
                        }
                    }
                    else if (mModel.Type == "Vehic")
                    {
                        var mlist = ctxTFAT.VehicleMaster.Where(x => x.Code.ToString().Trim() == mModel.Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.Srl = mlist.Code.ToString();
                            Model.ParentKey = mlist.Code;
                        }
                    }
                    else if (mModel.Type == "Maste")
                    {
                        var mlist = ctxTFAT.Master.Where(x => x.Code.ToString().Trim() == mModel.Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.Srl = mlist.Code;
                            Model.ParentKey = mlist.Code;
                        }
                    }
                    else if (mModel.Type == "CUSMA")
                    {
                        var mlist = ctxTFAT.CustomerMaster.Where(x => x.Code.ToString().Trim() == mModel.Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.Srl = mlist.Code;
                            Model.ParentKey = mlist.Code;
                        }
                    }
                    else if (mModel.Type == "Drive")
                    {
                        var mlist = ctxTFAT.DriverMaster.Where(x => x.Code.ToString().Trim() == mModel.Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.Srl = mlist.Code;
                            Model.ParentKey = mlist.Code;
                        }
                    }
                    else if (mModel.Type == "PUR00" || mModel.Type == "CPH00")
                    {
                        var mlist = ctxTFAT.Purchase.Where(x => x.TableKey.ToString().Trim() == mModel.Srl && x.Type == mModel.Type).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.Srl = mlist.Srl.ToString();
                            Model.ParentKey = mlist.TableKey;
                        }
                    }
                    else if (mModel.Type == "SLR00" || mModel.Type == "SLW00" || mModel.Type == "CMM00")
                    {
                        var mlist = ctxTFAT.Sales.Where(x => x.TableKey.ToString().Trim() == Model.Srl && x.Type == mModel.Type).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.Srl = mlist.Srl;
                            Model.ParentKey = mlist.TableKey;
                        }
                    }
                    else if (mModel.Type == "Alert")
                    {
                        var mlist = ctxTFAT.AlertNoteMaster.Where(x => x.DocNo.ToString().Trim() == Model.Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.Srl = mlist.DocNo;
                            Model.ParentKey = mlist.TableKey;
                        }
                    }

                    var AttachCode = GetNewAttachCode();
                    string directoryPath = attachmentPath + Model.ParentKey + "/" + Model.FileName;
                    Directory.CreateDirectory(attachmentPath + Model.ParentKey);
                    var Count = ctxTFAT.Attachment.Where(x => x.ParentKey == Model.ParentKey).ToList().Count() + 1;
                    Attachment att = new Attachment();
                    att.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                    att.AUTHIDS = muserid;
                    att.AUTHORISE = mAUTHORISE;
                    att.Branch = mbranchcode;
                    att.Code = AttachCode.ToString();
                    att.ENTEREDBY = muserid;
                    att.FilePath = directoryPath;
                    att.LASTUPDATEDATE = DateTime.Now;
                    att.LocationCode = Model.LocationCode;
                    att.Prefix = mperiod;
                    att.Sno = Count;
                    Model.tempId = Count;
                    att.Srl = Model.Srl;
                    att.SrNo = Count;
                    att.TableKey = mModel.Type + mperiod.Substring(0, 2) + Count.ToString("D3") + Model.Srl;
                    Model.TableKey = mModel.Type + mperiod.Substring(0, 2) + Count.ToString("D3") + Model.Srl;
                    att.ParentKey = Model.ParentKey;
                    att.Type = mModel.Type;
                    att.RefType = mModel.RefType;
                    att.CompCode = mcompcode;
                    att.DocType = mModel.DocType;
                    att.ExternalAttach = false;
                    ctxTFAT.Attachment.Add(att);
                    ctxTFAT.SaveChanges();
                    System.IO.File.WriteAllBytes(directoryPath, fileData);
                }
                DocList.Add(Model);
            }
            Session["TempAttach"] = DocList;

            DocList = DocList.ToList();
            DocList = DocList.Where(x => x.DocType == mModel.DocType).ToList();
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Logistics/Views/Attachment/AttachmentDocumentWithoutModal.cshtml", new AttachmentVM() { RefType = mModel.RefType, Type = mModel.Type, Srl = mModel.Srl, Mode = mModel.Mode, DocumentList = DocList });
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        [HttpPost]//Delete Attachement
        public ActionResult DeleteUploadFileWithoutModal(AttachmentVM Model)
        {
            List<AttachmentVM> DocList = new List<AttachmentVM>();
            if (Session["TempAttach"] != null)
            {
                DocList = (List<AttachmentVM>)Session["TempAttach"];
            }

            Model.DocumentList = DocList.Where(x => x.tempId != Model.tempId).Select(x => x).ToList();
            Session["TempAttach"] = Model.DocumentList;
            if (Model.Mode != "Add")
            {
                var Delete = DocList.Where(x => x.tempId == Model.tempId).Select(x => x).FirstOrDefault();
                var attachment = ctxTFAT.Attachment.Where(x => x.Type == Model.Type && x.TableKey == Delete.TableKey).ToList();
                //var TempPath = attachment.Select(x => x.FilePath).FirstOrDefault();
                //var count = ctxTFAT.Attachment.Where(x => x.Type == Model.Type && x.Srl == Model.Srl && x.FilePath == TempPath && x.TableKey != Delete.TableKey).Select(x => x).FirstOrDefault();
                //if (count == null)
                //{
                //    foreach (var item in attachment)
                //    {
                //        if (System.IO.File.Exists(item.FilePath))
                //        {
                //            System.IO.File.Delete(item.FilePath);
                //        }
                //    }
                //}

                ctxTFAT.Attachment.RemoveRange(attachment);
                ctxTFAT.SaveChanges();
            }
            Model.DocumentList = Model.DocumentList.Where(x => x.DocType == Model.DocType).ToList();

            var html = ViewHelper.RenderPartialView(this, "~/Areas/Logistics/Views/Attachment/AttachmentDocumentWithoutModal.cshtml", Model);
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public ActionResult WebCamCaptureImgWithoutModal(string DocumentStr, string Mode, string Type, string Srl, string RefType, string DocType)
        {
            string ContentType = "data:image/jpeg;base64,";
            List<AttachmentVM> DocList = new List<AttachmentVM>();
            if (Session["TempAttach"] != null)
            {
                DocList = (List<AttachmentVM>)Session["TempAttach"];
            }
            AttachmentVM AttachmentVM = new AttachmentVM();
            if (!String.IsNullOrEmpty(DocumentStr))
            {
                //DocumentStr = DocumentStr.Replace(DocumentStr, "data:image/png;base64,");
                DocumentStr = DocumentStr.Replace("data:image/jpeg;base64,", String.Empty);
                string FileName = RandomString(10, true) + ".jpg";
                AttachmentVM.DocDate = DateTime.Now.ToShortDateString();
                AttachmentVM.FileName = FileName;
                AttachmentVM.tempId = DocList.OrderByDescending(x => x.tempId).Select(x => x.tempId).FirstOrDefault() + 1;
                AttachmentVM.Path = "";
                AttachmentVM.FileContent = ContentType;
                AttachmentVM.ImageStr = DocumentStr;
                AttachmentVM.ImageData = Convert.FromBase64String(DocumentStr);
                AttachmentVM.RefType = RefType;
                AttachmentVM.Type = Type;
                AttachmentVM.Srl = Srl;
                AttachmentVM.Mode = Mode;
                AttachmentVM.ENTEREDBY = muserid;
                AttachmentVM.DocType = DocType;

                string TableKey = "", DocSrl="";
                if (Mode != "Add")
                {
                    if (Type == "LR000")
                    {
                        var mlist = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString().Trim() == Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.LrNo.ToString();
                            TableKey = mlist.TableKey;
                        }
                    }
                    if (Type == "PCK00")
                    {
                        var mlist = ctxTFAT.PickOrder.Where(x => x.OrderNo.ToString().Trim() == Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.OrderNo.ToString();
                            TableKey = mlist.OrderNo.ToString();
                        }
                    }
                    else if (Type == "FM000" || Type == "FMH00")
                    {
                        var mlist = ctxTFAT.FMMaster.Where(x => x.FmNo.ToString().Trim() == Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.FmNo.ToString();
                            TableKey = mlist.TableKey;
                        }
                    }
                    else if (Type == "Vehic")
                    {
                        var mlist = ctxTFAT.VehicleMaster.Where(x => x.Code.ToString().Trim() == Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.Code.ToString();
                            TableKey = mlist.Code;
                        }
                    }
                    else if (Type == "Maste")
                    {
                        var mlist = ctxTFAT.Master.Where(x => x.Code.ToString().Trim() == Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.Code;
                            TableKey = mlist.Code;
                        }
                    }
                    else if (Type == "CUSMA")
                    {
                        var mlist = ctxTFAT.CustomerMaster.Where(x => x.Code.ToString().Trim() == Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.Code;
                            TableKey = mlist.Code;
                        }
                    }
                    else if (Type == "Drive")
                    {
                        var mlist = ctxTFAT.DriverMaster.Where(x => x.Code.ToString().Trim() == Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.Code;
                            TableKey = mlist.Code;
                        }
                    }
                    else if (Type == "PUR00" || Type == "CPH00")
                    {
                        var mlist = ctxTFAT.Purchase.Where(x => x.Srl.ToString().Trim() == Srl && x.Type == Type).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.Srl.ToString();
                            TableKey = mlist.TableKey;
                        }
                    }
                    else if (Type == "BLSMT")
                    {
                        var mlist = ctxTFAT.BillSubmission.Where(x => x.DocNo.ToString().Trim() == Srl && x.Prefix == mperiod).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.DocNo.ToString();
                            TableKey = mlist.DocNo;
                        }
                    }
                    else if (Type == "SLR00" || Type == "SLW00" || Type == "CMM00")
                    {
                        var mlist = ctxTFAT.Sales.Where(x => x.BillNumber.ToString().Trim() == Srl && x.Type == Type).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.Srl;
                            TableKey = mlist.TableKey;
                        }
                    }
                    else if (Type == "Alert")
                    {
                        var mlist = ctxTFAT.AlertNoteMaster.Where(x => x.DocNo.ToString().Trim() == Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.DocNo;
                            TableKey = mlist.TableKey;
                        }
                    }
                    else if (Type == "CPO00" || Type == "COT00")
                    {
                        var mlist = ctxTFAT.RelateData.Where(x => x.ParentKey.ToString().Trim() == Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.Srl.ToString();
                            TableKey = mlist.ParentKey;
                        }
                    }

                    var AttachCode = GetNewAttachCode();
                    string directoryPath = attachmentPath + TableKey + "/" + FileName;
                    Directory.CreateDirectory(attachmentPath + TableKey);

                    Attachment att = new Attachment();
                    att.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                    att.AUTHIDS = muserid;
                    att.AUTHORISE = mAUTHORISE;
                    att.Branch = mbranchcode;
                    att.Code = AttachCode.ToString();
                    att.ENTEREDBY = muserid;
                    att.FilePath = directoryPath;
                    att.LASTUPDATEDATE = DateTime.Now;
                    att.LocationCode = mlocationcode;
                    att.Prefix = mperiod;
                    att.Sno = AttachmentVM.tempId;
                    att.Srl = DocSrl;
                    att.SrNo = AttachmentVM.tempId;
                    att.TableKey = Type + mperiod.Substring(0, 2) + AttachmentVM.tempId.ToString("D3") + DocSrl;
                    AttachmentVM.TableKey = Type + mperiod.Substring(0, 2) + AttachmentVM.tempId.ToString("D3") + DocSrl;
                    att.ParentKey = TableKey;
                    att.Type = AttachmentVM.Type;
                    att.RefType = AttachmentVM.RefType;
                    att.CompCode = mcompcode;
                    att.DocType = DocType;
                    att.ExternalAttach = false;
                    ctxTFAT.Attachment.Add(att);
                    ctxTFAT.SaveChanges();
                    System.IO.File.WriteAllBytes(directoryPath, AttachmentVM.ImageData);

                }
                DocList.Add(AttachmentVM);
            }

            TempData["TempAttach"] = DocList;
            DocList = DocList.Where(x => x.DocType == DocType).ToList();

            AttachmentVM.DocumentList = DocList;
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Logistics/Views/Attachment/AttachmentDocumentWithoutModal.cshtml", AttachmentVM);
            var jsonResult = Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }
        #endregion

        #region Camera 

        private readonly Random _random = new Random();
        public string RandomString(int size, bool lowerCase = false)
        {
            var builder = new StringBuilder(size);
            // Unicode/ASCII Letters are divided into two blocks
            // (Letters 65–90 / 97–122):     

            // char is a single Unicode character  
            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26; // A...Z or a..z: length = 26  
            for (var i = 0; i < size; i++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }
            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }
        [HttpPost]//Upload Camera Attachement
        public ActionResult WebCamCaptureImg(string DocumentStr, string Mode, string Type, string Srl, string RefType, string DocType)
        {
            string ContentType = "data:image/jpeg;base64,";
            List<AttachmentVM> DocList = new List<AttachmentVM>();
            if (Session["TempAttach"] != null)
            {
                DocList = (List<AttachmentVM>)Session["TempAttach"];
            }
            AttachmentVM AttachmentVM = new AttachmentVM();
            if (!String.IsNullOrEmpty(DocumentStr))
            {
                //DocumentStr = DocumentStr.Replace(DocumentStr, "data:image/png;base64,");
                DocumentStr = DocumentStr.Replace("data:image/jpeg;base64,", String.Empty);
                string FileName = RandomString(10, true) + ".jpg";
                AttachmentVM.DocDate = DateTime.Now.ToShortDateString();
                AttachmentVM.FileName = FileName;
                AttachmentVM.tempId = DocList.OrderByDescending(x => x.tempId).Select(x => x.tempId).FirstOrDefault() + 1;
                AttachmentVM.Path = "";
                AttachmentVM.FileContent = ContentType;
                AttachmentVM.ImageStr = DocumentStr;
                AttachmentVM.ImageData = Convert.FromBase64String(DocumentStr);
                AttachmentVM.RefType = RefType;
                AttachmentVM.Type = Type;
                AttachmentVM.Srl = Srl;
                AttachmentVM.Mode = Mode;
                AttachmentVM.ENTEREDBY = muserid;
                AttachmentVM.DocType = DocType;

                string TableKey = "", DocSrl = "";
                if (Mode != "Add")
                {
                    if (Type == "LR000")
                    {
                        var mlist = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString().Trim() == Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.LrNo.ToString();
                            TableKey = mlist.TableKey;
                        }
                    }
                    if (Type == "PCK00")
                    {
                        var mlist = ctxTFAT.PickOrder.Where(x => x.OrderNo.ToString().Trim() == Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.OrderNo.ToString();
                            TableKey = mlist.OrderNo.ToString();
                        }
                    }
                    else if (Type == "FM000" || Type == "FMH00")
                    {
                        var mlist = ctxTFAT.FMMaster.Where(x => x.FmNo.ToString().Trim() == Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.FmNo.ToString();
                            TableKey = mlist.TableKey;
                        }
                    }
                    else if (Type == "Vehic")
                    {
                        var mlist = ctxTFAT.VehicleMaster.Where(x => x.Code.ToString().Trim() == Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.Code.ToString();
                            TableKey = mlist.Code;
                        }
                    }
                    else if (Type == "Maste")
                    {
                        var mlist = ctxTFAT.Master.Where(x => x.Code.ToString().Trim() == Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.Code;
                            TableKey = mlist.Code;
                        }
                    }
                    else if (Type == "CUSMA")
                    {
                        var mlist = ctxTFAT.CustomerMaster.Where(x => x.Code.ToString().Trim() == Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.Code;
                            TableKey = mlist.Code;
                        }
                    }
                    else if (Type == "Drive")
                    {
                        var mlist = ctxTFAT.DriverMaster.Where(x => x.Code.ToString().Trim() == Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.Code;
                            TableKey = mlist.Code;
                        }
                    }
                    else if (Type == "PUR00" || Type == "CPH00")
                    {
                        var mlist = ctxTFAT.Purchase.Where(x => x.Srl.ToString().Trim() == Srl && x.Type == Type).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.Srl.ToString();
                            TableKey = mlist.TableKey;
                        }
                    }
                    else if (Type == "BLSMT")
                    {
                        var mlist = ctxTFAT.BillSubmission.Where(x => x.DocNo.ToString().Trim() == Srl && x.Prefix == mperiod).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.DocNo.ToString();
                            TableKey = mlist.DocNo;
                        }
                    }
                    else if (Type == "SLR00" || Type == "SLW00" || Type == "CMM00")
                    {
                        var mlist = ctxTFAT.Sales.Where(x => x.BillNumber.ToString().Trim() == Srl && x.Type == Type).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.Srl;
                            TableKey = mlist.TableKey;
                        }
                    }
                    else if (Type == "Alert")
                    {
                        var mlist = ctxTFAT.AlertNoteMaster.Where(x => x.DocNo.ToString().Trim() == Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.DocNo;
                            TableKey = mlist.TableKey;
                        }
                    }
                    else if (Type == "CPO00" || Type == "COT00")
                    {
                        var mlist = ctxTFAT.RelateData.Where(x => x.ParentKey.ToString().Trim() == Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            DocSrl = mlist.Srl.ToString();
                            TableKey = mlist.ParentKey;
                        }
                    }

                    var AttachCode = GetNewAttachCode();
                    string directoryPath = attachmentPath + TableKey + "/" + FileName;
                    Directory.CreateDirectory(attachmentPath + TableKey);

                    Attachment att = new Attachment();
                    att.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                    att.AUTHIDS = muserid;
                    att.AUTHORISE = mAUTHORISE;
                    att.Branch = mbranchcode;
                    att.Code = AttachCode.ToString();
                    att.ENTEREDBY = muserid;
                    att.FilePath = directoryPath;
                    att.LASTUPDATEDATE = DateTime.Now;
                    att.LocationCode = mlocationcode;
                    att.Prefix = mperiod;
                    att.Sno = AttachmentVM.tempId;
                    att.Srl = DocSrl;
                    att.SrNo = AttachmentVM.tempId;
                    att.TableKey = Type + mperiod.Substring(0, 2) + AttachmentVM.tempId.ToString("D3") + DocSrl;
                    AttachmentVM.TableKey = Type + mperiod.Substring(0, 2) + AttachmentVM.tempId.ToString("D3") + DocSrl;
                    att.ParentKey = TableKey;
                    att.Type = AttachmentVM.Type;
                    att.RefType = AttachmentVM.RefType;
                    att.DocType = AttachmentVM.DocType;
                    att.CompCode = mcompcode;
                    att.ExternalAttach = false;
                    ctxTFAT.Attachment.Add(att);
                    ctxTFAT.SaveChanges();
                    System.IO.File.WriteAllBytes(directoryPath, AttachmentVM.ImageData);

                }
                DocList.Add(AttachmentVM);
            }

            TempData["TempAttach"] = DocList;
            AttachmentVM.DocumentList = DocList;
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Logistics/Views/Attachment/AttachmentDocument.cshtml", AttachmentVM);
            var jsonResult = Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        #endregion


        public ActionResult GetAllAttachmentsOnlyView(AttachmentVM Model)
        {
            List<AttachmentVM> attachments = new List<AttachmentVM>();
            attachments = GetAttachmentListInEdit(Model);
            Model.DocumentList = attachments.OrderBy(x => x.tempId).ToList();

            var html = ViewHelper.RenderPartialView(this, "~/Areas/Logistics/Views/Attachment/AttachmentDocument.cshtml", Model);
            var jsonResult = Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }



    }
}