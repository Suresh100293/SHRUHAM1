using ALT_ERP3.Areas.Accounts.Models;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ALT_ERP3.Controllers
{
    public class SendEmailController : BaseController
    {
        //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        public new string EmialLogReportName = "";
        public new string EmialLogPersonName = "";
        public new string EmialLogHeader = "";
        public new string EmialLogAutoRemark = "";
        //Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");

        public ActionResult Index(GridOption Model)
        {
            EmialLogHeader = Model.Header;
            Session["AttachFileTempAttach"] = null;
            if (Model.MenuName == "T")
            {
                EmialLogAutoRemark = "Alert Through Email";
                string Consignor = "", consignee = "", BillParty = "", EmailTo = "";
                LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey == Model.TableKey).FirstOrDefault();
                if (lRMaster != null)
                {
                    Consignor = lRMaster.RecCode;
                    consignee = lRMaster.SendCode;
                    BillParty = lRMaster.BillParty;
                    Model.Grp = "Consignment Alert Docket No:" + lRMaster.LrNo + " From : " + ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Source).Select(x => x.Name).FirstOrDefault() + "  To : " + ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name).FirstOrDefault() + "";
                }
                if (!String.IsNullOrEmpty(Consignor))
                {
                    var Consigner = ctxTFAT.ConsignerAddress.Where(x => x.Code == Consignor && x.AllSendEmail == true).ToList();
                    //Match match = regex.Match(Consigner.Email);
                    foreach (var item in Consigner)
                    {
                        if (!String.IsNullOrEmpty(item.Email))
                        {

                            EmailTo += item.Email + ",";
                        }
                    }
                    if (!String.IsNullOrEmpty(EmailTo))
                    {
                        EmialLogPersonName += "Consignor Name : " + Consigner.Select(x => x.Name).FirstOrDefault() + "    ";
                    }
                }
                if (!String.IsNullOrEmpty(consignee))
                {
                    var ConsigneeM = ctxTFAT.ConsignerAddress.Where(x => x.Code == consignee && x.AllSendEmail == true).ToList();
                    foreach (var item in ConsigneeM)
                    {
                        if (!String.IsNullOrEmpty(item.Email))
                        {

                            EmailTo += item.Email + ",";
                        }
                    }
                    if (!String.IsNullOrEmpty(EmailTo))
                    {
                        EmialLogPersonName += "Consignee Name : " + ConsigneeM.Select(x => x.Name).FirstOrDefault() + "    ";
                    }
                }
                if (!String.IsNullOrEmpty(BillParty))
                {
                    var Customer = ctxTFAT.Caddress.Where(x => x.Code == BillParty).ToList();
                    if (Customer.Count() > 0)
                    {
                        EmialLogPersonName += "Customer Name : " + ctxTFAT.CustomerMaster.Where(x => x.Code == BillParty).Select(x => x.Name).FirstOrDefault();

                        foreach (var item in Customer)
                        {
                            //Match match = regex.Match(item.Email);
                            //if (match.Success)
                            {
                                EmailTo += item.Email + ",";
                            }
                        }
                    }

                }
                Model.SelectContent = EmailTo;
            }
            if (String.IsNullOrEmpty(Model.SelectContent))
            {
                Model.SelectContent = "";
            }
            return View(Model);
        }

        public ActionResult SendEmailNow(string mTo, string mCC, string mBCC, string mSubject, string mMessage, string mAttachment, string Tablekey)
        {
            int mid = 0;
            string msmtppassword = "";
            string msmtphost = "";
            string msmtpuser = "";
            string FromMail = "";
            int msmtpport = 25;

            bool SendMail = false;
            bool Branch = false;
            string EmailmStr = "";



            LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey == Tablekey).FirstOrDefault();
            if (lRMaster != null)
            {
                string mNewCC = "";
                if (ctxTFAT.tfatAlertNoteSetup.FirstOrDefault() != null)
                {
                    if (!String.IsNullOrEmpty(ctxTFAT.tfatAlertNoteSetup.Select(x => x.CCEmail).FirstOrDefault()))
                    {
                        mNewCC = ctxTFAT.tfatAlertNoteSetup.Select(x => x.CCEmail).FirstOrDefault();
                    }
                }
                if (String.IsNullOrEmpty(mCC))
                {
                    mCC = mNewCC;
                }
                else
                {
                    mCC += "," + mNewCC;
                }
                EmailmStr = "<html>";
                EmailmStr += "<p><span style=\"font-family:Tahoma;font-size:17pt;\"><b>" + ctxTFAT.TfatComp.Select(x => x.Name.ToUpper()).FirstOrDefault() + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma;color:red;\" >Note: <b>" + mMessage + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma;font-size:16pt;\"><b>" + "Document Detail" + "</b></span></p>";
                //EmailmStr += "<br/>";
                EmailmStr += "<p><span style=\"font-family:Tahoma\">Docket No: <b>" + lRMaster.LrNo + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma\">Book Date: <b>" + lRMaster.BookDate.ToShortDateString() + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma\">From: <b>" + (ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Source).Select(x => x.Name.ToUpper()).FirstOrDefault()) + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma\">To: <b>" + (ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name.ToUpper()).FirstOrDefault()) + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma\">Consignor: <b>" + (ctxTFAT.Consigner.Where(x => x.Code == lRMaster.RecCode).Select(x => x.Name.ToUpper()).FirstOrDefault()) + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma\">Consignee: <b>" + (ctxTFAT.Consigner.Where(x => x.Code == lRMaster.SendCode).Select(x => x.Name.ToUpper()).FirstOrDefault()) + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma\">Bill Party: <b>" + (ctxTFAT.CustomerMaster.Where(x => x.Code == lRMaster.BillParty).Select(x => x.Name.ToUpper()).FirstOrDefault()) + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma\">Qty: <b>" + lRMaster.TotQty + " - " + ctxTFAT.UnitMaster.Where(x => x.Code == lRMaster.UnitCode).Select(x => x.Name).FirstOrDefault() + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma\">Description: <b>" + ctxTFAT.DescriptionMaster.Where(x => x.Code == lRMaster.DescrType).Select(x => x.Description).FirstOrDefault() + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma\">Weight: <b>" + lRMaster.ChgWt + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma\">Invoice No: <b>" + lRMaster.PartyInvoice + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma\">PONumber: <b>" + lRMaster.PONumber + "</b></span></p>";
                
                
                EmailmStr += "</html>";
            }
            else
            {
                EmailmStr = "<html>";
                EmailmStr += "<p><span style=\"font-family:Tahoma;color:red;\" >Note: <b>" + mMessage + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma;font-size:17pt;\"><b>" + ctxTFAT.TfatComp.Select(x => x.Name.ToUpper()).FirstOrDefault() + "</b></span></p>";
                EmailmStr += "<br/>";
                EmailmStr += "</html>";
            }
            mMessage = EmailmStr;

            var UserBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).FirstOrDefault();
            if (Convert.ToBoolean(UserBranch.BranchMail))
            {
                if (Convert.ToBoolean(UserBranch.LocalMail))
                {
                    var mEmailInfo = ctxTFAT.TfatPass.Where(z => z.Code == muserid).Select(x => new
                    {
                        x.SMTPUser,
                        x.SMTPServer,
                        x.SMTPPassword,
                        x.SMTPPort,
                        x.CCTo,
                        x.BCCTo,
                        x.Email,
                    }).FirstOrDefault();

                    if (mEmailInfo != null)
                    {
                        mCC = mCC == null ? "" : mCC.Trim();
                        mBCC = mBCC == null ? "" : mBCC.Trim();
                        msmtpuser = mEmailInfo.SMTPUser == null ? "" : mEmailInfo.SMTPUser.Trim();
                        msmtppassword = mEmailInfo.SMTPPassword == null ? "" : mEmailInfo.SMTPPassword.Trim();
                        msmtphost = mEmailInfo.SMTPServer == null ? "" : mEmailInfo.SMTPServer.Trim();
                        msmtpport = mEmailInfo.SMTPPort == null ? 25 : mEmailInfo.SMTPPort.Value;
                        FromMail = mEmailInfo.Email;
                        Branch = true;
                        if (FromMail.Contains("@"))
                        {
                            SendMail = true;
                        }
                    }
                }
                else
                {
                    var mEmailInfo = ctxTFAT.TfatComp.Select(x => new
                    {
                        x.SMTPUser,
                        x.SMTPServer,
                        x.SMTPPassword,
                        x.SMTPPort,
                        x.CCTo,
                        x.BCCTo,
                        x.GlobalMail,
                        x.Email
                    }).FirstOrDefault();

                    if (mEmailInfo != null)
                    {
                        mCC = mCC == null ? "" : mCC.Trim();
                        mBCC = mBCC == null ? "" : mBCC.Trim();
                        msmtpuser = mEmailInfo.SMTPUser == null ? "" : mEmailInfo.SMTPUser.Trim();
                        FromMail = mEmailInfo.Email == null ? "" : mEmailInfo.Email.Trim();
                        msmtppassword = mEmailInfo.SMTPPassword == null ? "" : mEmailInfo.SMTPPassword.Trim();
                        msmtphost = mEmailInfo.SMTPServer == null ? "" : mEmailInfo.SMTPServer.Trim();
                        msmtpport = mEmailInfo.SMTPPort == null ? 25 : mEmailInfo.SMTPPort.Value;
                        if (mEmailInfo.GlobalMail)
                        {
                            if (FromMail.Contains("@"))
                            {
                                SendMail = true;
                            }
                        }
                    }
                }
            }
            try
            {
                if (SendMail)
                {
                    if (Branch)
                    {
                        if (String.IsNullOrEmpty(msmtphost))
                        {
                            msmtphost = "smtp.gmail.com";
                        }

                        if (String.IsNullOrEmpty(msmtpuser))
                        {
                            if (String.IsNullOrEmpty(FromMail))
                            {
                                return Json(new
                                {
                                    Status = "Error",
                                    Message = "Please Check Your Profile...!\n Email ID Missing...!"
                                }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                msmtpuser = FromMail;
                            }
                        }
                        if (String.IsNullOrEmpty(msmtppassword))
                        {
                            return Json(new
                            {
                                Status = "Error",
                                Message = "Please Check Your Profile...!\n Password Missing...!"
                            }, JsonRequestBehavior.AllowGet);
                        }
                        if (msmtpport == 0)
                        {
                            msmtpport = 587;
                        }
                        try
                        {
                            MailMessage mail = new MailMessage();

                            List<PurchaseVM> DocList = new List<PurchaseVM>();
                            if (Session["AttachFileTempAttach"] != null)
                            {
                                DocList = (List<PurchaseVM>)Session["AttachFileTempAttach"];
                            }
                            foreach (var item in DocList)
                            {
                                MemoryStream mem = new MemoryStream(item.ImageData);
                                System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(mem, item.FileName, item.ContentType);
                                mail.Attachments.Add(attachment);
                            }

                            SmtpClient SmtpServer = new SmtpClient(msmtphost);

                            mail.From = new MailAddress(FromMail);
                            mail.To.Add(mTo);
                            if (mCC != "") mail.CC.Add(mCC);
                            if (mBCC != "") mail.Bcc.Add(mBCC);
                            mail.Subject = mSubject;
                            mail.IsBodyHtml = true;
                            mail.BodyEncoding = System.Text.Encoding.UTF8;
                            mMessage = mMessage.Replace("^~|", "<br>");
                            if (mMessage.Contains("<html>") == false)
                            {
                                mMessage = TextToHtmlSend(mMessage);
                            }
                            mail.Body = mMessage;

                            SmtpServer.Port = msmtpport;
                            SmtpServer.Credentials = new System.Net.NetworkCredential(msmtpuser, msmtppassword);
                            SmtpServer.EnableSsl = true;
                            SmtpServer.Send(mail);
                            mid = SaveEmailLog(mTo, mCC, mBCC, mSubject, mMessage, lRMaster == null ? "" : lRMaster.ParentKey, lRMaster == null ? "" : lRMaster.BillParty, EmialLogReportName, EmialLogPersonName, EmialLogHeader, EmialLogAutoRemark, muserid);

                        }
                        catch (SmtpFailedRecipientException smtex)
                        {
                            throw;
                        }
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(msmtphost))
                        {
                            msmtphost = "smtp.entpmail.com";
                        }

                        if (String.IsNullOrEmpty(msmtpuser))
                        {
                            if (String.IsNullOrEmpty(FromMail))
                            {
                                return Json(new
                                {
                                    Status = "Error",
                                    Message = "Please Check Your Company Profile...!\n Email ID Missing...!"
                                }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                msmtpuser = FromMail;
                            }
                        }
                        if (String.IsNullOrEmpty(msmtppassword))
                        {
                            return Json(new
                            {
                                Status = "Error",
                                Message = "Please Check Your Company Profile...!\n Password Missing...!"
                            }, JsonRequestBehavior.AllowGet);
                        }
                        if (msmtpport == 0)
                        {
                            msmtpport = 587;
                        }



                        MailMessage message = new MailMessage();
                        List<PurchaseVM> DocList = new List<PurchaseVM>();
                        if (Session["AttachFileTempAttach"] != null)
                        {
                            DocList = (List<PurchaseVM>)Session["AttachFileTempAttach"];
                        }
                        foreach (var item in DocList)
                        {
                            MemoryStream mem = new MemoryStream(item.ImageData);
                            System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(mem, item.FileName, item.ContentType);
                            message.Attachments.Add(attachment);
                        }

                        message.From = new MailAddress(FromMail);
                        message.To.Add(mTo);
                        if (mCC != "") message.CC.Add(mCC);
                        if (mBCC != "") message.Bcc.Add(mBCC);

                        message.Subject = mSubject;

                        message.IsBodyHtml = true;
                        message.BodyEncoding = System.Text.Encoding.UTF8;
                        mMessage = mMessage.Replace("^~|", "<br>");
                        if (mMessage.Contains("<html>") == false)
                        {
                            mMessage = TextToHtmlSend(mMessage);
                        }
                        message.Body = mMessage;
                        message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess;
                        SmtpClient smtp = new SmtpClient();

                        smtp.Host = msmtphost;
                        smtp.Port = msmtpport;
                        smtp.Credentials = new System.Net.NetworkCredential(msmtpuser, msmtppassword);
                        smtp.EnableSsl = true;
                        smtp.Send(message);
                        mid = SaveEmailLog(mTo, mCC, mBCC, mSubject, mMessage, lRMaster == null ? "" : lRMaster.ParentKey, lRMaster == null ? "" : lRMaster.BillParty, EmialLogReportName, EmialLogPersonName, EmialLogHeader, EmialLogAutoRemark, muserid);

                    }
                }

                return Json(new
                {
                    Status = "Success"
                }, JsonRequestBehavior.AllowGet);

            }

            catch (Exception ex)
            {
                return Json(new
                {
                    Status = "Error",
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost]
        public ActionResult SaveSelectedValues(GridOption Model)
        {
            string html = "";


            if (Model.ViewDataId == "MultiPartySelect")
            {
                Model.ViewCode = "#MainPartialDiv";
                html = ViewHelper.RenderPartialView(this, "SendEmailPartial", Model);
            }
            if (Model.ViewDataId == "MultiSalesmanSelect")
            {
                Model.ViewCode = "#MainPartialDiv2";
                html = ViewHelper.RenderPartialView(this, "SubjectPartialView", Model);
            }

            return Json(new { Html = html, View = Model.ViewCode }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult AttachDocument(HttpPostedFileBase files)
        {
            List<PurchaseVM> list = new List<PurchaseVM>();
            if (Session["AttachFileTempAttach"] != null)
            {
                list = (List<PurchaseVM>)Session["AttachFileTempAttach"];
            }
            string XYZ = "";
            string docstr = "";
            string FLN = "";
            int n = 0;
            byte[] fileData = null;
            for (int i = 0; i < Request.Files.Count; i++)
            {
                PurchaseVM Model = new PurchaseVM();
                var file = Request.Files[i];
                var fileName = Path.GetFileName(file.FileName);
                string path = Server.MapPath(fileName);
                Model.Path = path;
                MemoryStream target = new MemoryStream();
                using (var binaryReader = new BinaryReader(Request.Files[i].InputStream))
                {
                    fileData = binaryReader.ReadBytes(Request.Files[i].ContentLength);
                }
                Model.ImageData = fileData;
                string FileString = Convert.ToBase64String(fileData);
                file.InputStream.CopyTo(target);
                Model.FileContent = file.ContentType;
                Model.ImageStr = FileString;
                Model.FileName = fileName;
                Model.ContentType = file.ContentType;
                Model.tempId = list.Count() + 1;
                n = n + 1;
                list.Add(Model);
            }
            ViewBag.DocumentList = list;
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            var html = ViewHelper.RenderPartialView(this, "AttachmentDocument", ViewBag.DocumentList);
            Session["AttachFileTempAttach"] = list;
            return Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]//Delete Attachement
        public ActionResult DeleteUploadFile(PurchaseVM Model)
        {
            List<PurchaseVM> DocList = new List<PurchaseVM>();
            if (Session["AttachFileTempAttach"] != null)
            {
                DocList = (List<PurchaseVM>)Session["AttachFileTempAttach"];
            }

            Model.DocumentList = DocList.Where(x => x.tempId != Model.tempId).Select(x => x).ToList();
            Session["AttachFileTempAttach"] = Model.DocumentList;
            ViewBag.DocumentList = Model.DocumentList;
            var html = ViewHelper.RenderPartialView(this, "AttachmentDocument", ViewBag.DocumentList);
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        private string TextToHtmlSend(string text)
        {
            //text = HttpUtility.HtmlEncode(text);
            text = text.Replace("\r\n", "\r");
            text = text.Replace("\n", "\r");
            text = text.Replace("\r", "<br>\r\n");
            text = text.Replace("  ", " &nbsp;");
            string mstr = "<html>";

            if (ctxTFAT.TfatBranch.Where(z => z.Code == mbranchcode).Select(x => x.Logo).FirstOrDefault() != null)
                mstr += "<img src = \"data:image/png;base64," + Convert.ToBase64String(ctxTFAT.TfatBranch.Where(z => z.Code == mbranchcode).Select(x => x.Logo).FirstOrDefault()) + "\" width=\"50\" height=\"50\" alt=\"Branch Logo\"/>";
            mstr += "<p><span style=\"font-family:Tahoma;font-size:16pt;\"><b>" + (ctxTFAT.TfatBranch.Where(z => z.Code == mbranchcode).Select(x => x.Name).FirstOrDefault() ?? "") + "</b></span></p>";
            mstr += "<hr>";
            if (text.Contains("^b"))
            {
                text = text.Replace("^b", "<strong>").Replace("^eb", "</strong>");
            }
            mstr += text + "</html>";
            return mstr;
        }
    }
}