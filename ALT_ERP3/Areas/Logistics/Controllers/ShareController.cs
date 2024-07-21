using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using Common;
using CrystalDecisions.CrystalReports.Engine;
using EntitiModel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class ShareController : BaseController
    {
        public new string EmialLogReportName = "";
        public new string EmialLogPersonName = "";
        public new string EmialLogHeader = "";
        public new string EmialLogAutoRemark = "Share Document";

        //Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        private List<SelectListItem> PopulatePrints(string Type)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT FormatCode as Code FROM DocFormats where type='" + Type + "'  order by Recordkey ";
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
                                Text = sdr["Code"].ToString(),
                                Value = sdr["Code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return items;
        }


        // GET: Logistics/Share
        public ActionResult Index(ShareVM mModel)
        {
            if (!String.IsNullOrEmpty(mModel.Parentkey) && mModel.Type!="SLR00" && mModel.Type != "BPM00")
            {
                string Branch = mModel.Parentkey.Substring(0, 6);
                if (ctxTFAT.TfatBranch.Where(x => x.Code == Branch).FirstOrDefault() != null)
                {
                    var Parentkey = mModel.Parentkey.Substring(6, mModel.Parentkey.Length - 6);
                    mModel.Type = Parentkey.Substring(0, 5);
                }
                else
                {
                    mModel.Type = mModel.Parentkey.Substring(0, 5);
                }
            }
            else
            {
                mModel.Parentkey = mModel.Parentkey.Substring(6, mModel.Parentkey.Length - 6);
            }
            
            var ShareSetup = ctxTFAT.ShareSetup.FirstOrDefault();
            if (ShareSetup != null)
            {
                if (mModel.Type == "LR000")
                {
                    mModel.Attachment = ShareSetup.LRAttachReq;
                    mModel.ExtraInfo = ShareSetup.LRExtra;
                    if (ShareSetup.LRAttachReq)
                    {
                        mModel.ConsignorFormat = ShareSetup.LRFormat;
                        mModel.ConsignoeeFormat = ShareSetup.LRFormat;
                        mModel.BillPartyFormat = ShareSetup.LRFormat;
                        mModel.ExtraFormat = ShareSetup.LRFormat;
                    }
                }
                else if (mModel.Type == "FM000")
                {
                    mModel.Attachment = ShareSetup.FMAttachReq;
                    mModel.ExtraInfo = ShareSetup.FmExtra;

                }
                else if (mModel.Type == "SLR00" || mModel.Type == "SLW00")
                {
                    mModel.Attachment = ShareSetup.BillAttachReq;
                    mModel.ExtraInfo = ShareSetup.BillExtra;
                    mModel.CustomerFormat = ShareSetup.BillFormat;
                    mModel.CustomerGroupFormat = ShareSetup.BillFormat;
                    mModel.ExtraFormat = ShareSetup.BillFormat;
                }
                else if (mModel.Type == "BPM00")
                {
                    mModel.Attachment = ShareSetup.PaymentAttachReq;
                    mModel.ExtraInfo = ShareSetup.Payment;
                }

            }

            mModel.PrintFormats = PopulatePrints(mModel.Type);

            if (mModel.Type == "LR000")
            {
                var LrMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == mModel.Parentkey).FirstOrDefault();
                if (LrMaster != null)
                {
                    //mModel.Parentkey = LrMaster.ParentKey;
                    mModel.Branch = LrMaster.Branch;

                    if (!(String.IsNullOrEmpty(LrMaster.BillParty)))
                    {
                        mModel.BillPartyName = ctxTFAT.CustomerMaster.Where(x => x.Code == LrMaster.BillParty).Select(x => x.Name).FirstOrDefault();
                        var CustomerDetails = ctxTFAT.Caddress.Where(x => x.Code == LrMaster.BillParty).FirstOrDefault();
                        if (CustomerDetails != null)
                        {
                            mModel.BillPartyMobileNo = CustomerDetails.Mobile.Trim();
                            mModel.BillPartyEmailId = CustomerDetails.Email.Trim();
                        }

                    }

                    var Consignor = ctxTFAT.Consigner.Where(x => x.Code == LrMaster.RecCode && x.Acitve == true).FirstOrDefault();
                    if (Consignor != null)
                    {
                        mModel.ConsignorName = Consignor.Name;
                        var ConsignerAddress = ctxTFAT.ConsignerAddress.Where(x => x.Code == LrMaster.RecCode).ToList();
                        if (ConsignerAddress != null)
                        {
                            foreach (var item in ConsignerAddress)
                            {
                                if (item.AllSendEmail)
                                {
                                    if (!(String.IsNullOrEmpty(item.Email)))
                                    {
                                        mModel.ConsignorEmailId += item.Email.Trim() + ",";
                                    }
                                }
                                if (item.AllSendSMS)
                                {
                                    if (!(String.IsNullOrEmpty(item.Mobile)))
                                    {
                                        mModel.ConsignorMobileNo += item.Mobile.Trim() + ",";
                                    }
                                }
                            }
                            if (!String.IsNullOrEmpty(mModel.ConsignorEmailId))
                            {
                                mModel.ConsignorEmailId = mModel.ConsignorEmailId.Substring(0, mModel.ConsignorEmailId.Length - 1);
                            }
                            if (!String.IsNullOrEmpty(mModel.ConsignorMobileNo))
                            {
                                mModel.ConsignorMobileNo = mModel.ConsignorMobileNo.Substring(0, mModel.ConsignorMobileNo.Length - 1);
                            }
                        }
                    }

                    var Consignee = ctxTFAT.Consigner.Where(x => x.Code == LrMaster.SendCode && x.Acitve == true).FirstOrDefault();
                    if (Consignee != null)
                    {
                        mModel.ConsignoeeName = Consignee.Name;
                        var ConsignerAddress = ctxTFAT.ConsignerAddress.Where(x => x.Code == LrMaster.SendCode).ToList();
                        if (ConsignerAddress != null)
                        {
                            foreach (var item in ConsignerAddress)
                            {
                                if (item.AllSendEmail)
                                {
                                    if (!(String.IsNullOrEmpty(item.Email)))
                                    {
                                        mModel.ConsignoeeEmailId += item.Email.Trim() + ",";
                                    }
                                }
                                if (item.AllSendSMS)
                                {
                                    if (!(String.IsNullOrEmpty(item.Mobile)))
                                    {
                                        mModel.ConsignoeeMobileNo += item.Mobile.Trim() + ",";
                                    }
                                }
                            }
                            if (!String.IsNullOrEmpty(mModel.ConsignoeeEmailId))
                            {
                                mModel.ConsignoeeEmailId = mModel.ConsignoeeEmailId.Substring(0, mModel.ConsignoeeEmailId.Length - 1);
                            }
                            if (!String.IsNullOrEmpty(mModel.ConsignoeeMobileNo))
                            {
                                mModel.ConsignoeeMobileNo = mModel.ConsignoeeMobileNo.Substring(0, mModel.ConsignoeeMobileNo.Length - 1);
                            }
                        }
                    }
                }

                var html = ViewHelper.RenderPartialView(this, "_ShareDetails", mModel);
                var jsonResult = Json(new
                {
                    Html = html
                }, JsonRequestBehavior.AllowGet);
                return jsonResult;
            }
            else if (mModel.Type == "FM000")
            {
                var FmMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == mModel.Parentkey).FirstOrDefault();
                if (FmMaster != null)
                {
                    mModel.Parentkey = FmMaster.ParentKey;
                    mModel.Branch = FmMaster.Branch;

                    mModel.BrokerName = ctxTFAT.Master.Where(x => x.Code == FmMaster.BroCode).Select(x => x.Name).FirstOrDefault();
                    var BrokerDetails = ctxTFAT.Address.Where(x => x.Code == FmMaster.BroCode).FirstOrDefault();
                    if (BrokerDetails != null)
                    {
                        mModel.BrokerMobileNo = BrokerDetails.Mobile.Trim();
                        mModel.BrokerEmailId = BrokerDetails.Email.Trim();
                    }
                }

                var html = ViewHelper.RenderPartialView(this, "_ShareDetails", mModel);
                var jsonResult = Json(new
                {
                    Html = html
                }, JsonRequestBehavior.AllowGet);
                return jsonResult;
            }
            else if (mModel.Type == "SLR00" || mModel.Type == "SLW00")
            {
                var SaleBill = ctxTFAT.Sales.Where(x => x.TableKey.ToString() == mModel.Parentkey).FirstOrDefault();
                if (SaleBill != null)
                {
                    mModel.CustomerName = ctxTFAT.CustomerMaster.Where(x => x.Code == SaleBill.Code).Select(x => x.Name).FirstOrDefault();
                    var CustomerDetails = ctxTFAT.Caddress.Where(x => x.Code == SaleBill.Code).FirstOrDefault();
                    if (CustomerDetails != null)
                    {
                        mModel.CustomerMobileNo = CustomerDetails.Mobile.Trim();
                        mModel.CustomerEmailId = CustomerDetails.Email.Trim();

                        var Customermaster = ctxTFAT.CustomerMaster.Where(x => x.Code == SaleBill.Code).FirstOrDefault();
                        mModel.CustomerGroupName = ctxTFAT.Master.Where(x => x.Code == Customermaster.AccountParentGroup).Select(x => x.Name).FirstOrDefault();
                        var BrokerDetails = ctxTFAT.Address.Where(x => x.Code == Customermaster.AccountParentGroup).FirstOrDefault();
                        if (BrokerDetails != null)
                        {
                            mModel.CustomerGroupMobileNo = BrokerDetails.Mobile.Trim();
                            mModel.CustomerGroupEmailId = BrokerDetails.Email.Trim();
                        }
                    }
                }

                var html = ViewHelper.RenderPartialView(this, "_ShareDetails", mModel);
                var jsonResult = Json(new
                {
                    Html = html
                }, JsonRequestBehavior.AllowGet);
                return jsonResult;
            }
            else if (mModel.Type == "BPM00")
            {
                var CreditorPayment = ctxTFAT.Ledger.Where(x => x.Branch + x.ParentKey == mModel.Parentkey && x.Sno == 1).FirstOrDefault();
                if (CreditorPayment==null)
                {
                    CreditorPayment = ctxTFAT.Ledger.Where(x => x.ParentKey == mModel.Parentkey && x.Sno == 1).FirstOrDefault();
                }
                if (CreditorPayment != null)
                {
                    mModel.CreditorName = ctxTFAT.Master.Where(x => x.Code == CreditorPayment.Code).Select(x => x.Name).FirstOrDefault();
                    var BrokerDetails = ctxTFAT.Address.Where(x => x.Code == CreditorPayment.Code).FirstOrDefault();
                    if (BrokerDetails != null)
                    {
                        mModel.CreditorMobileNo = BrokerDetails.Mobile.Trim();
                        mModel.CreditorEmailId = BrokerDetails.Email.Trim();
                    }
                }
                var html = ViewHelper.RenderPartialView(this, "_ShareDetails", mModel);
                var jsonResult = Json(new
                {
                    Html = html
                }, JsonRequestBehavior.AllowGet);
                return jsonResult;
            }

            var jsonResult1 = Json(new
            {
                Status = "Error",
                Html = ""
            }, JsonRequestBehavior.AllowGet);
            return jsonResult1;
        }

        public ActionResult ShareDoc(ShareVM mModel)
        {
            var MailGlobal = ctxTFAT.TfatComp.Select(x => x.GlobalMail).FirstOrDefault();
            EmialLogHeader = mModel.Header;
            if (mModel.Type == "LR000")
            {
                string mCC = "";
                if (ctxTFAT.ShareSetup.FirstOrDefault() != null)
                {
                    if (!string.IsNullOrEmpty(ctxTFAT.ShareSetup.Select(x => x.CCEmail).FirstOrDefault()))
                    {
                        mCC = ctxTFAT.ShareSetup.Select(x => x.CCEmail).FirstOrDefault();
                    }
                }


                LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == mModel.Parentkey.ToString().Trim()).FirstOrDefault();
                if (mModel.ConsignorSmsReq)
                {
                    if (String.IsNullOrEmpty(mModel.ConsignorMobileNo) == false && mModel.ConsignorMobileNo.ToUpper().Trim() != "NULL")
                    {
                        EmialLogPersonName = "Consignor Name : " + mModel.ConsignorName;
                        SendSMS(lRMaster.Branch + lRMaster.ParentKey, "Consignment Booked", lRMaster.BookDate, mModel.ConsignorMobileNo, mModel.ConsignorName);
                    }

                }
                if (mModel.ConsignoeeSmsReq && mModel.ConsignorMobileNo != mModel.ConsignoeeMobileNo)
                {
                    if (String.IsNullOrEmpty(mModel.ConsignoeeMobileNo) == false && mModel.ConsignoeeMobileNo.ToUpper().Trim() != "NULL")
                    {
                        EmialLogPersonName = "Consignee Name : " + mModel.ConsignoeeName;
                        SendSMS(lRMaster.Branch + lRMaster.ParentKey, "Consignment Booked", lRMaster.BookDate, mModel.ConsignoeeMobileNo, mModel.ConsignoeeName);
                    }
                }
                if (mModel.BillPartySmsReq && mModel.ConsignorMobileNo != mModel.BillPartyMobileNo && mModel.ConsignoeeMobileNo != mModel.BillPartyMobileNo)
                {
                    if (String.IsNullOrEmpty(mModel.BillPartyMobileNo) == false && mModel.BillPartyMobileNo.ToUpper().Trim() != "NULL")
                    {
                        EmialLogPersonName = "Customer Name : " + mModel.BillPartyName;
                        SendSMS(lRMaster.Branch + lRMaster.ParentKey, "Consignment Booked", lRMaster.BookDate, mModel.BillPartyMobileNo, mModel.BillPartyName);
                    }
                }
                if (String.IsNullOrEmpty(mModel.ExtraMobileNo) == false)
                {
                    EmialLogPersonName = " Extra Mobile No ";
                    SendSMS(lRMaster.Branch + lRMaster.ParentKey, "Consignment Booked", lRMaster.BookDate, mModel.ExtraMobileNo, "N/A");

                }
                string EmailmStr = "";
                EmailmStr = "<html>";
                EmailmStr += "<p><span style=\"font-family:Tahoma;font-size:17pt;\"><b>" + ctxTFAT.TfatComp.Select(x => x.Name.ToUpper()).FirstOrDefault() + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma;font-size:16pt;\"><b>" + "Document Detail" + "</b></span></p>";
                EmailmStr += "<br/>";
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

                if (mModel.ConsignorEmailReq)
                {
                    if (String.IsNullOrEmpty(mModel.ConsignorFormat))
                    {
                        mModel.ConsignorEmailAttachReq = false;
                    }
                    else
                    {
                        mModel.ConsignorEmailAttachReq = true;
                    }

                    var mstream = GetStream(mModel.ConsignorFormat, lRMaster.TableKey.ToString(), lRMaster.Branch);

                    //Match match = regex.Match(mModel.ConsignorEmailId);
                    //if (match.Success)
                    {
                        EmialLogPersonName = "Consignor Name : " + mModel.ConsignorName;
                        SendSendEMail(mModel.ConsignorEmailAttachReq, mstream, mModel.ConsignorEmailId, "Consignmnet Booking Details", EmailmStr + " ", false, mCC, "", lRMaster.ParentKey, mModel.ConsignorName, MailGlobal);
                    }

                }
                if (mModel.ConsignoeeEamilReq && mModel.ConsignorEmailId != mModel.ConsignoeeEmailId)
                {
                    var mstream = GetStream(mModel.ConsignoeeFormat, lRMaster.TableKey.ToString(), lRMaster.Branch);
                    //Match match = regex.Match(mModel.ConsignoeeEmailId);
                    //if (match.Success)
                    {
                        EmialLogPersonName = "Consignee Name : " + mModel.ConsignoeeName;
                        SendSendEMail(mModel.ConsignoeeEamilAttachReq, mstream, mModel.ConsignoeeEmailId, "Consignmnet Booking Details", EmailmStr + " ", false, mCC, "", lRMaster.ParentKey, mModel.ConsignoeeName, MailGlobal);
                    }
                }
                if (mModel.BillPartyEamilReq && mModel.ConsignorEmailId != mModel.BillPartyEmailId && mModel.ConsignoeeEmailId != mModel.BillPartyEmailId)
                {
                    var mstream = GetStream(mModel.BillPartyFormat, lRMaster.TableKey.ToString(), lRMaster.Branch);
                    //Match match = regex.Match(mModel.BillPartyEmailId);
                    //if (match.Success)
                    {
                        EmialLogPersonName = "Customer Name : " + mModel.BillPartyName;
                        SendSendEMail(mModel.BillPartyEamilAttachReq, mstream, mModel.BillPartyEmailId, "Consignmnet Booking Details", EmailmStr + " ", false, mCC, "", lRMaster.ParentKey, mModel.BillPartyName, MailGlobal);
                    }
                }
                if (!String.IsNullOrEmpty(mModel.ExtraEmailId))
                {
                    var mstream = GetStream(mModel.ExtraFormat, lRMaster.TableKey.ToString(), lRMaster.Branch);
                    //Match match = regex.Match(mModel.ExtraEmailId);
                    //if (match.Success)
                    {
                        EmialLogPersonName = " Extra Mail ID ";
                        SendSendEMail(String.IsNullOrEmpty(mModel.ExtraFormat) == true ? false : true, mstream, mModel.ExtraEmailId, "Consignmnet Booking Details", EmailmStr + " ", false, mCC, "", lRMaster.ParentKey, "N/A", MailGlobal);
                    }
                }

            }
            else if (mModel.Type == "SLR00" || mModel.Type == "SLW00")
            {
                string mCC = "";
                if (ctxTFAT.ShareSetup.FirstOrDefault() != null)
                {
                    if (!string.IsNullOrEmpty(ctxTFAT.ShareSetup.Select(x => x.CCEmail).FirstOrDefault()))
                    {
                        mCC = ctxTFAT.ShareSetup.Select(x => x.CCEmail).FirstOrDefault();
                    }
                }

                Sales sales = ctxTFAT.Sales.Where(x => x.TableKey.ToString() == mModel.Parentkey.ToString().Trim()).FirstOrDefault();
                if (mModel.CustomerSmsReq)
                {
                    if (String.IsNullOrEmpty(mModel.CustomerMobileNo) == false && mModel.CustomerMobileNo.ToUpper().Trim() != "NULL")
                    {
                        EmialLogPersonName = "Customer Name : " + mModel.CustomerName;
                        //SendSMS(lRMaster.Branch + lRMaster.ParentKey, "Consignment Booked", lRMaster.BookDate, mModel.ConsignorMobileNo, mModel.ConsignorName);
                    }
                }
                if (mModel.CustomerGroupSmsReq && mModel.CustomerGroupMobileNo != mModel.CustomerMobileNo)
                {
                    if (String.IsNullOrEmpty(mModel.CustomerGroupMobileNo) == false && mModel.CustomerGroupMobileNo.ToUpper().Trim() != "NULL")
                    {
                        EmialLogPersonName = "Ledger Name : " + mModel.CustomerGroupName;
                        //SendSMS(lRMaster.Branch + lRMaster.ParentKey, "Consignment Booked", lRMaster.BookDate, mModel.ConsignoeeMobileNo, mModel.ConsignoeeName);
                    }
                }
                if (String.IsNullOrEmpty(mModel.ExtraMobileNo) == false)
                {
                    EmialLogPersonName = " Extra Mobile No ";
                    //SendSMS(lRMaster.Branch + lRMaster.ParentKey, "Consignment Booked", lRMaster.BookDate, mModel.ExtraMobileNo, "N/A");
                }
                string EmailmStr = "";
                EmailmStr = "<html>";
                EmailmStr += "<p><span style=\"font-family:Tahoma;font-size:17pt;\"><b>" + ctxTFAT.TfatComp.Select(x => x.Name.ToUpper()).FirstOrDefault() + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma;font-size:16pt;\"><b>" + "Invoice Detail" + "</b></span></p>";
                EmailmStr += "<br/>";
                EmailmStr += "<p><span style=\"font-family:Tahoma\">Invoice No: <b>" + sales.Srl + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma\">Invoice Date: <b>" + sales.DocDate.Value.ToShortDateString() + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma\">Customer: <b>" + (ctxTFAT.CustomerMaster.Where(x => x.Code == sales.Code).Select(x => x.Name.ToUpper()).FirstOrDefault()) + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma\">Amount: <b>" + sales.Amt.Value.ToString() + "</b></span></p>";
                EmailmStr += "</html>";

                if (mModel.CustomerEamilReq)
                {
                    if (String.IsNullOrEmpty(mModel.CustomerFormat))
                    {
                        mModel.CustomerEamilAttachReq = false;
                    }
                    else
                    {
                        mModel.CustomerEamilAttachReq = true;
                    }

                    var mstream = GetStream(mModel.CustomerFormat, sales.TableKey.ToString(), sales.Branch);

                    //Match match = regex.Match(mModel.ConsignorEmailId);
                    //if (match.Success)
                    {
                        EmialLogPersonName = "Customer Name : " + mModel.CustomerName;
                        SendSendEMail(mModel.CustomerEamilAttachReq, mstream, mModel.CustomerEmailId, "Invoice Details", EmailmStr + " ", false, mCC, "", sales.TableKey, mModel.CustomerName, MailGlobal);
                    }

                }
                if (mModel.CustomerGroupEamilReq && mModel.CustomerEmailId != mModel.CustomerGroupEmailId)
                {
                    var mstream = GetStream(mModel.CustomerGroupFormat, sales.TableKey.ToString(), sales.Branch);
                    //Match match = regex.Match(mModel.ConsignoeeEmailId);
                    //if (match.Success)
                    {
                        EmialLogPersonName = "Ledger Name : " + mModel.CustomerGroupName;
                        SendSendEMail(mModel.CustomerGroupEamilReq, mstream, mModel.CustomerGroupEmailId, "Invoice Details", EmailmStr + " ", false, mCC, "", sales.TableKey, mModel.CustomerGroupName, MailGlobal);
                    }
                }
                
                if (!String.IsNullOrEmpty(mModel.ExtraEmailId))
                {
                    var mstream = GetStream(mModel.ExtraFormat, sales.TableKey.ToString(), sales.Branch);
                    //Match match = regex.Match(mModel.ExtraEmailId);
                    //if (match.Success)
                    {
                        EmialLogPersonName = " Extra Mail ID ";
                        SendSendEMail(String.IsNullOrEmpty(mModel.ExtraFormat) == true ? false : true, mstream, mModel.ExtraEmailId, "Invoice Details", EmailmStr + " ", false, mCC, "", sales.TableKey, "N/A", MailGlobal);
                    }
                }

            }
            else if (mModel.Type == "BPM00" )
            {
                string mCC = "";
                if (ctxTFAT.ShareSetup.FirstOrDefault() != null)
                {
                    if (!string.IsNullOrEmpty(ctxTFAT.ShareSetup.Select(x => x.CCEmail).FirstOrDefault()))
                    {
                        mCC = ctxTFAT.ShareSetup.Select(x => x.CCEmail).FirstOrDefault();
                    }
                }

                Ledger ledger = ctxTFAT.Ledger.Where(x => x.ParentKey.ToString() == mModel.Parentkey.ToString().Trim() && x.Sno==1).FirstOrDefault();
                if (mModel.CreditorSmsReq)
                {
                    if (String.IsNullOrEmpty(mModel.CreditorMobileNo) == false && mModel.CreditorMobileNo.ToUpper().Trim() != "NULL")
                    {
                        EmialLogPersonName = "Broker Name : " + mModel.CreditorName;
                        //SendSMS(lRMaster.Branch + lRMaster.ParentKey, "Consignment Booked", lRMaster.BookDate, mModel.ConsignorMobileNo, mModel.ConsignorName);
                    }
                }
                if (String.IsNullOrEmpty(mModel.ExtraMobileNo) == false)
                {
                    EmialLogPersonName = " Extra Mobile No ";
                    //SendSMS(lRMaster.Branch + lRMaster.ParentKey, "Consignment Booked", lRMaster.BookDate, mModel.ExtraMobileNo, "N/A");
                }
                string EmailmStr = "";
                EmailmStr = "<html>";
                EmailmStr += "<p><span style=\"font-family:Tahoma;font-size:17pt;\"><b>" + ctxTFAT.TfatComp.Select(x => x.Name.ToUpper()).FirstOrDefault() + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma;font-size:16pt;\"><b>" + "Payment Detail" + "</b></span></p>";
                EmailmStr += "<br/>";
                EmailmStr += "<p><span style=\"font-family:Tahoma\">Invoice No: <b>" + ledger.Srl + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma\">Invoice Date: <b>" + ledger.DocDate.ToShortDateString() + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma\">Broker: <b>" + (ctxTFAT.Master.Where(x => x.Code == ledger.Code).Select(x => x.Name.ToUpper()).FirstOrDefault()) + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma\">Bank: <b>" + (ctxTFAT.Master.Where(x => x.Code == ledger.AltCode).Select(x => x.Name.ToUpper()).FirstOrDefault()) + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma\">Amount: <b>" + ledger.Debit.ToString() + "</b></span></p>";
                EmailmStr += "<p><span style=\"font-family:Tahoma\">Cheque No: <b>" + ledger.Cheque + "</b></span></p>";
                EmailmStr += "</html>";

                if (mModel.CreditorEamilReq)
                {
                    if (String.IsNullOrEmpty(mModel.CreditorFormat))
                    {
                        mModel.CreditorEamilAttachReq = false;
                    }
                    else
                    {
                        mModel.CreditorEamilAttachReq = true;
                    }

                    var mstream = GetStream(mModel.CreditorFormat, ledger.ParentKey.ToString(), ledger.Branch);

                    //Match match = regex.Match(mModel.ConsignorEmailId);
                    //if (match.Success)
                    {
                        EmialLogPersonName = "Broker Name : " + mModel.CreditorName;
                        SendSendEMail(mModel.CreditorEamilAttachReq, mstream, mModel.CreditorEmailId, "Payment Details", EmailmStr + " ", false, mCC, "", ledger.TableKey, mModel.CreditorName, MailGlobal);
                    }

                }
                if (!String.IsNullOrEmpty(mModel.ExtraEmailId))
                {
                    var mstream = GetStream(mModel.ExtraFormat, ledger.ParentKey.ToString(), ledger.Branch);
                    //Match match = regex.Match(mModel.ExtraEmailId);
                    //if (match.Success)
                    {
                        EmialLogPersonName = " Extra Mail ID ";
                        SendSendEMail(String.IsNullOrEmpty(mModel.ExtraFormat) == true ? false : true, mstream, mModel.ExtraEmailId, "Payment Details", EmailmStr + " ", false, mCC, "", ledger.TableKey, "N/A", MailGlobal);
                    }
                }

            }
            return null;
        }



        public byte[] GetStream(string PrintDormats, string Parentkey, string Branch)
        {
            Stream streamR;
            Document document = new Document(PageSize.A4, 10, 10, 10, 10);
            var ms = new MemoryStream();
            if (!String.IsNullOrEmpty(PrintDormats))
            {
                {
                    PdfCopy pdf = new PdfCopy(document, ms);
                    document.Open();
                    pdf.Open();
                    List<string> mSrls = PrintDormats.Split(',').ToList();
                    foreach (var msrl in mSrls.OrderByDescending(x => x).ToList())//Model.list)
                    {
                        //mcode = msrl.Code;
                        var mcode = msrl;
                        if (string.IsNullOrEmpty(mcode) == false)
                        {

                            var StoreProcedure = ctxTFAT.DocFormats.Where(x => x.FormatCode == msrl).Select(x => x.StoredProc).FirstOrDefault();
                            DataTable dtreport = new DataTable();
                            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                            SqlCommand cmd = new SqlCommand("SPDoc_" + StoreProcedure, tfat_conx); //name of the storedprocedure
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandTimeout = 120;
                            cmd.Parameters.AddWithValue("@mParentKey", Parentkey);
                            cmd.Parameters.AddWithValue("@mBranch", Branch);
                            SqlDataAdapter adp = new SqlDataAdapter(cmd);
                            adp.Fill(dtreport);

                            ReportDocument rd = new ReportDocument();
                            rd.Load(Path.Combine(Server.MapPath("~/Reports"), msrl + ".rpt"));
                            rd.SetDataSource(dtreport);
                            //rd.PrintToPrinter(1, true, 0, 0);
                            try
                            {
                                Stream mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                                var mWhat = "PDF";
                                switch (mWhat)
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
                    document.Close();
                }
            }

            return ms.ToArray();
        }

        public void SendSMS(string mParentKey, string Temp, DateTime mDate, string MobileNo, string BillParty)//MObileNO is String By Comma
        {
            var mid = 0;
            try
            {
                WebRequest request = null;
                HttpWebResponse response = null;
                string mUID = "";
                string mPass = "";
                string mCaption = "";
                string mURL = "";
                string mProxy = "";
                bool mSMSPrefix = false;
                var mType = mParentKey.Substring(6, 5);
                var mMsg = (ctxTFAT.MsgTemplate.Where(x => x.Code == Temp).Select(x => x.MsgText).FirstOrDefault() ?? "").Trim();

                if (mMsg.Contains("%"))
                {
                    var Lrmaster = ctxTFAT.LRMaster.Where(x => x.Branch + x.ParentKey == mParentKey).FirstOrDefault();
                    if (Temp == "Consignment Booked")
                    {
                        var LR = ctxTFAT.LRMaster.Where(x => x.Branch + x.ParentKey == mParentKey).FirstOrDefault();
                        mMsg = mMsg.Replace("{%DocNo%}", Lrmaster.LrNo.ToString());
                        mMsg = mMsg.Replace("{%DocDate%}", mDate.ToShortDateString());
                        mMsg = mMsg.Replace("{%From%}", ctxTFAT.TfatBranch.Where(x => x.Code == LR.Source).Select(x => x.Name).FirstOrDefault());
                        mMsg = mMsg.Replace("{%TO%}", ctxTFAT.TfatBranch.Where(x => x.Code == LR.Dest).Select(x => x.Name).FirstOrDefault());
                        mMsg = mMsg.Replace("{%Consignor%}", ctxTFAT.Consigner.Where(x => x.Code == LR.RecCode).Select(x => x.Name).FirstOrDefault());
                        mMsg = mMsg.Replace("{%Consignee%}", ctxTFAT.Consigner.Where(x => x.Code == LR.SendCode).Select(x => x.Name).FirstOrDefault());
                        mMsg = mMsg.Replace("{%Qty%}", LR.TotQty.ToString());
                        mMsg = mMsg.Replace("{%QtyDesc%}", ctxTFAT.UnitMaster.Where(x => x.Code == LR.UnitCode).Select(x => x.Name).FirstOrDefault());
                        mMsg = mMsg.Replace("{%Weight%}", LR.ActWt.ToString());

                    }
                    else if (Temp == "Consignment Delivered")
                    {
                        var DeliveryMaster = ctxTFAT.DeliveryMaster.Where(x => x.Branch + x.ParentKey == mParentKey).FirstOrDefault();
                        var LR = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == DeliveryMaster.ParentKey.ToString()).FirstOrDefault();

                        mMsg = mMsg.Replace("{%DocNo%}", LR.LrNo.ToString());
                        mMsg = mMsg.Replace("{%DocDate%}", LR.BookDate.ToShortDateString());
                        mMsg = mMsg.Replace("{%From%}", ctxTFAT.TfatBranch.Where(x => x.Code == LR.Source).Select(x => x.Name).FirstOrDefault());
                        mMsg = mMsg.Replace("{%TO%}", ctxTFAT.TfatBranch.Where(x => x.Code == LR.Dest).Select(x => x.Name).FirstOrDefault());
                        mMsg = mMsg.Replace("{%DelDate%}", DeliveryMaster.DeliveryDate.ToShortDateString());
                        mMsg = mMsg.Replace("{%Qty%}", DeliveryMaster.Qty.ToString());
                        mMsg = mMsg.Replace("{%QtyDesc%}", ctxTFAT.UnitMaster.Where(x => x.Code == LR.UnitCode).Select(x => x.Name).FirstOrDefault());
                    }
                    else if (Temp == "Freight Memo Booked")
                    {
                        var fMMaster = ctxTFAT.FMMaster.Where(x => x.Branch + x.ParentKey == mParentKey).FirstOrDefault();

                        mMsg = mMsg.Replace("{%DocNo%}", fMMaster.FmNo.ToString());
                        mMsg = mMsg.Replace("{%VehicleNo%}", ctxTFAT.HireVehicleMaster.Where(x => x.Code == fMMaster.TruckNo).Select(x => x.TruckNo).FirstOrDefault());
                        mMsg = mMsg.Replace("{%Broker%}", ctxTFAT.Master.Where(x => x.Code == fMMaster.BroCode).Select(x => x.Name).FirstOrDefault());
                        mMsg = mMsg.Replace("{%Driver%}", fMMaster.Driver);
                        mMsg = mMsg.Replace("{%ContactNo%}", fMMaster.ContactNo);
                        mMsg = mMsg.Replace("{%DocDate%}", fMMaster.Date.ToShortDateString());
                        mMsg = mMsg.Replace("{%From%}", ctxTFAT.TfatBranch.Where(x => x.Code == fMMaster.FromBranch).Select(x => x.Name).FirstOrDefault());
                        mMsg = mMsg.Replace("{%Via%}", fMMaster.RouteViaName);
                        mMsg = mMsg.Replace("{%TO%}", ctxTFAT.TfatBranch.Where(x => x.Code == fMMaster.ToBranch).Select(x => x.Name).FirstOrDefault());
                        mMsg = mMsg.Replace("{%Freight%}", fMMaster.Freight.ToString());
                        mMsg = mMsg.Replace("{%Advance%}", fMMaster.Adv.ToString());
                        mMsg = mMsg.Replace("{%Balance %}", fMMaster.Balance.ToString());
                    }
                    //update for Pending Value 

                }
                var mMobile = MobileNo.ToString();
                try
                {
                    TfatComp tfatComp = ctxTFAT.TfatComp.FirstOrDefault();
                    if (!String.IsNullOrEmpty(tfatComp.SMSURL))
                    {
                        // You can convert a string into a byte array
                        byte[] asciiBytes = Encoding.ASCII.GetBytes(mMsg);

                        // You can convert a byte array into a char array
                        char[] asciiChars = Encoding.ASCII.GetChars(asciiBytes);
                        string asciiString = new string(asciiChars);

                        string GenerateUrl = "";
                        GenerateUrl += tfatComp.SMSURL + "?";
                        if (!String.IsNullOrEmpty(tfatComp.SmsUername))
                        {
                            GenerateUrl += "username=" + tfatComp.SmsUername;
                        }
                        if (!String.IsNullOrEmpty(tfatComp.Smspassword))
                        {
                            GenerateUrl += "&password=" + tfatComp.Smspassword;
                        }
                        if (!String.IsNullOrEmpty(tfatComp.SmsID))
                        {
                            GenerateUrl += "&" + tfatComp.SmsID;
                        }
                        if (!String.IsNullOrEmpty(tfatComp.SmsPara1))
                        {
                            GenerateUrl += "&" + tfatComp.SmsPara1 + asciiString;
                        }
                        if (!String.IsNullOrEmpty(tfatComp.SmsPara2))
                        {
                            GenerateUrl += "&" + tfatComp.SmsPara2 + mMobile;
                        }
                        if (!String.IsNullOrEmpty(tfatComp.SmsPara3))
                        {
                            GenerateUrl += "&" + tfatComp.SmsPara3;
                        }
                        if (!String.IsNullOrEmpty(tfatComp.SmsPara4))
                        {
                            GenerateUrl += "&" + tfatComp.SmsPara4;
                        }
                        if (!String.IsNullOrEmpty(tfatComp.SmsPara5))
                        {
                            GenerateUrl += "&" + tfatComp.SmsPara5;
                        }





                        WebClient client = new WebClient();
                        string jsonstring = client.DownloadString(GenerateUrl);

                        //using (WebClient client = new WebClient())
                        //{
                        //    var GenerateUrl = "https://web.insignsms.com/api/sendsms?username=laqshya&password=laqshya&senderid=LAQSYA&message=" + asciiString + "&numbers=" + mMobile + "&dndrefund=1";
                        //    var content = client.DownloadString(GenerateUrl);
                        //}

                        mid = SaveSMSLog(MobileNo.ToString(), mMsg, mParentKey, BillParty.ToString(), EmialLogPersonName, EmialLogHeader, EmialLogAutoRemark, muserid);
                    }
                }
                catch (Exception ex)
                {
                    ExecuteStoredProc("Update SMSLog Set sendstatus=0 where Recordkey=" + mid);
                    //return Json(new { Status = "Error", Message = ex.InnerException }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (SmtpFailedRecipientException smtex)
            {
                ExecuteStoredProc("Update SMSLog Set sendstatus=0 where Recordkey=" + mid);
                //return "Error!\n" + smtex.InnerException;
            }

        }

        public void SendSendEMail(bool AttachmentReq, byte[] bytes, string mEmail, string mSubject, string mMsg, bool UseSuchanURL = false, string mCC = "", string mBCC = "", string mParentKey = "", string mParty = "", bool GlbalMail = false)
        {
            int mid = 0;
            try
            {
                bool SendMail = false;
                string msmtppassword = "";
                string msmtphost = "";
                int msmtpport = 25;
                string msmtpuser = "";
                string mFromEmail = "";
                mEmail = mEmail.Trim();
                mCC = mCC.Trim();
                mBCC = mBCC.Trim();
                if (GlbalMail == false)
                {
                    var UserBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).FirstOrDefault();
                    if (Convert.ToBoolean(UserBranch.BranchMail))
                    {
                        if (Convert.ToBoolean(UserBranch.LocalMail))
                        {
                            SendMail = true;
                        }
                    }

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
                        mCC = (mCC != "" ? mCC + "," : "");
                        mCC += mEmailInfo.CCTo == null ? "" : mEmailInfo.CCTo.Trim();
                        mBCC = (mBCC != "" ? mBCC + "," : "");
                        mBCC += mEmailInfo.BCCTo == null ? "" : mEmailInfo.BCCTo.Trim();
                        msmtpuser = mEmailInfo.SMTPUser == null ? "" : mEmailInfo.SMTPUser.Trim();
                        msmtppassword = mEmailInfo.SMTPPassword == null ? "" : mEmailInfo.SMTPPassword.Trim();
                        msmtphost = mEmailInfo.SMTPServer == null ? "" : mEmailInfo.SMTPServer.Trim();
                        msmtpport = mEmailInfo.SMTPPort == null ? 25 : mEmailInfo.SMTPPort.Value;
                        mFromEmail = (mEmailInfo.SMTPUser ?? "").Trim();
                    }
                }
                else
                {
                    var mEmailInfo = ctxTFAT.TfatComp.Where(z => z.Code == mcompcode).Select(x => new
                    {
                        x.SMTPUser,
                        x.SMTPServer,
                        x.SMTPPassword,
                        x.SMTPPort,
                        x.CCTo,
                        x.BCCTo,
                        x.Email,
                        x.GlobalMail
                    }).FirstOrDefault();
                    if (mEmailInfo != null)
                    {
                        mCC = (mCC != "" ? mCC + "," : "");
                        mCC += mEmailInfo.CCTo == null ? "" : mEmailInfo.CCTo.Trim();
                        mBCC = (mBCC != "" ? mBCC + "," : "");
                        mBCC += mEmailInfo.BCCTo == null ? "" : mEmailInfo.BCCTo.Trim();
                        msmtpuser = mEmailInfo.SMTPUser == null ? "" : mEmailInfo.SMTPUser.Trim();
                        msmtppassword = mEmailInfo.SMTPPassword == null ? "" : mEmailInfo.SMTPPassword.Trim();
                        msmtphost = mEmailInfo.SMTPServer == null ? "" : mEmailInfo.SMTPServer.Trim();
                        msmtpport = mEmailInfo.SMTPPort == null ? 25 : mEmailInfo.SMTPPort.Value;
                        mFromEmail = (mEmailInfo.Email ?? "").Trim();
                        if (mEmailInfo.GlobalMail)
                        {
                            SendMail = true;
                        }
                    }
                }

                mCC = CutRightString(mCC, 1, ",");
                mBCC = CutRightString(mBCC, 1, ",");

                if (UseSuchanURL == true || (msmtphost == null || msmtphost == "") || mFromEmail == "")
                {
                    //msmtphost = "smtp.hostedemail.com";
                    //msmtpuser = "laqshya@laqshyalogistics.com";
                    //msmtppassword = "04MfRMo22twL";
                    //msmtpport = 587;
                    //mFromEmail = msmtpuser;

                }
                if (msmtpport != 587)
                {
                    msmtpport = 587;
                }
                MailMessage message = new MailMessage();
                if (AttachmentReq)
                {
                    //System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(mstream.ToString(),"DEmo","pdf");
                    //message.Attachments.Add(attachment);


                    MemoryStream memoryStream = new MemoryStream(bytes);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    //MailMessage message = new MailMessage();
                    System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(memoryStream, "Attachment.pdf");
                    message.Attachments.Add(attachment);

                }

                message.From = new MailAddress(mFromEmail);
                mEmail = CutRightString(mEmail, 1, ";");
                mEmail = CutRightString(mEmail, 1, ",");
                message.To.Add(mEmail);
                if (mCC != "")
                {
                    message.CC.Add(mCC);
                }

                if (mBCC != "")
                {
                    message.Bcc.Add(mBCC);
                }
                message.Subject = mSubject;
                message.IsBodyHtml = true;
                message.BodyEncoding = System.Text.Encoding.UTF8;
                mMsg = mMsg.Replace("^~|", "<br>");
                if (mMsg.Contains("<html>") == false)
                {
                    mMsg = TextToHtml(mMsg);
                }
                message.Body = mMsg;
                message.Priority = MailPriority.High;
                message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = msmtphost;
                smtp.Port = msmtpport;
                smtp.Credentials = new System.Net.NetworkCredential(msmtpuser, msmtppassword);

                smtp.EnableSsl = true;
                if (mParentKey != "" && mParty == "")
                {
                    string mmain = GetMainType(mParentKey.Substring(0, 5));
                    if ("SL~PR".Contains(mmain))
                    {
                        mParty = Fieldoftable(mmain == "SL" ? "Sales" : "Purchase", "Code", "TableKey='" + mParentKey + "'");
                    }
                }
                if (SendMail)
                {
                    smtp.Send(message);
                    mid = SaveEmailLog(mEmail, mCC, mBCC, mSubject, mMsg, mParentKey, mParty, EmialLogReportName, EmialLogPersonName, EmialLogHeader, EmialLogAutoRemark, muserid);
                }

                //return Json(new { Status = "Success", Message = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (SmtpFailedRecipientException smtex)
            {
                ExecuteStoredProc("Update Emaillog Set sentStatus=0 where RecordKey=" + mid);
                //return Json(new { Status = "Error", Message = smtex.InnerException }, JsonRequestBehavior.AllowGet);
            }
        }


        private string TextToHtml(string text)
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

        //public ActionResult SendReportMail(GridOption Model)
        //{
        //    ReportViewer reportViewer = new Microsoft.Reporting.WebForms.ReportViewer();
        //    reportViewer.ProcessingMode = ProcessingMode.Local;
        //    reportViewer.SizeToReportContent = true;
        //    reportViewer.ZoomMode = ZoomMode.PageWidth;
        //    reportViewer.ShowToolBar = true;
        //    reportViewer.AsyncRendering = true;
        //    reportViewer.Reset();

        //    var connectionString = ConfigurationManager.ConnectionStrings["BillingConnectionString"].ConnectionString;

        //    SqlConnection conx = new SqlConnection(connectionString); SqlDataAdapter adp = new SqlDataAdapter("SELECT COUNT(RecordKey)as Totalflag, ASF.ExcelDataFlag,CAST(Sum(Fees) AS INT)As TotalFees,Isnull(sum(convert(int,OPE)),0)As TotalOPE,((Isnull(CAST(Sum(Fees) AS INT),0) + Isnull(sum(convert(int,OPE)),0)) + CAST(Sum(ASF.SGST) AS INT) + CAST(Sum(ASF.CGST) AS INT) + CAST(Sum(ASF.IGST) AS INT))As Total,(Isnull(CAST(Sum(Fees) AS INT),0) + Isnull(sum(convert(int,OPE)),0))As TotalWTGst,dbo.udf_Num_ToWords(((Isnull(CAST(Sum(Fees) AS INT),0) + Isnull(sum(convert(int,OPE)),0)) + CAST(Sum(ASF.SGST) AS INT) + CAST(Sum(ASF.CGST) AS INT) + CAST(Sum(ASF.IGST) AS INT))) as WordTotal ,CAST(Sum(ASF.SGST) AS INT)As SGST,CAST(Sum(ASF.CGST) AS INT)As CGST,CAST(Sum(ASF.IGST) AS INT)As IGST, ASF.InvoiceNo,ASF.Remark1,ASF.InvoiceDate,cust.Billing_Name, cust.State, cust.PaymentTerms, cust.Statecode, cust.Adrl1,cust.Adrl2,  Cust.Name, Cust.ContactPerson,cust.GST ,bnk.Bank_Name,bnk.Branch_Adrs,bnk.Branch_Name,bnk.Accno,bnk.IFSC FROM NhbsAssurance ASF, Custmas cust ,BankInfo bnk WHERE ExcelDataFlag IS NOT NULL  and ExcelDataFlag =(" + "'" + Model.ExcelDataFlag + "'" + @") and cust.code = ASF.CustCode  and ASF.BankName = bnk.Bank_Name and ASF.AccountNo = bnk.Accno  GROUP BY ASF.ExcelDataFlag,ASF.InvoiceNo,ASF.InvoiceDate,ASF.Remark1,cust.Billing_Name, cust.State, cust.PaymentTerms,cust.Statecode, cust.Adrl1,cust.Adrl2, Cust.Name, Cust.ContactPerson,cust.GST,bnk.Bank_Name,bnk.Branch_Adrs,bnk.Branch_Name,bnk.Accno,bnk.IFSC; ", conx);
        //    adp.Fill(ds, ds.AssuranceBill.TableName);
        //    //reportViewer.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"Reports\AssuranceReport.rdlc";
        //    reportViewer.LocalReport.ReportPath = Server.MapPath("/Reports/NHBSAssuranceReport.rdlc");
        //    //reportViewer.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", ds.Tables[0]));
        //    ReportDataSource rds = new ReportDataSource("DataSet1", ds.Tables[0]);
        //    reportViewer.LocalReport.DataSources.Clear();
        //    reportViewer.LocalReport.DataSources.Add(rds);
        //    reportViewer.LocalReport.Refresh();
        //    byte[] Bytes = reportViewer.LocalReport.Render(format: "PDF", deviceInfo: "");
        //    var Invoice = nhbsctx.NhbsAssurances.Where(x => x.ExcelDataFlag == Model.ExcelDataFlag).Select(x => new { x.InvoiceNo }).FirstOrDefault();

        //    ViewBag.ReportViewer = reportViewer;

        //    try
        //    {
        //        var cbillname = nhbsctx.NhbsAssurances.Where(x => x.ExcelDataFlag == Model.ExcelDataFlag).Select(x => x.BillingName).FirstOrDefault();
        //        var customemail = nhbsctx.CustMas.Where(x => x.Billing_Name == cbillname).Select(x => x.CustEmail).FirstOrDefault();
        //        string nhbscompemail = nhbsctx.EmailProviders.Select(x => x.EmailId).FirstOrDefault();
        //        string nhbspwd = nhbsctx.EmailProviders.Select(x => x.Password).FirstOrDefault();
        //        string nhbshost = nhbsctx.EmailProviders.Select(x => x.Host).FirstOrDefault();
        //        int nhbsport = nhbsctx.EmailProviders.Select(x => x.Port).FirstOrDefault();
        //        Warning[] warnings;
        //        string[] streamids;
        //        string mimeType;
        //        string encoding;
        //        string extension;

        //        byte[] bytes = reportViewer.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamids, out warnings);

        //        MemoryStream memoryStream = new MemoryStream(bytes);
        //        memoryStream.Seek(0, SeekOrigin.Begin);

        //        MailMessage message = new MailMessage();
        //        System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(memoryStream, "NhbsAnnexureReport.pdf");
        //        message.Attachments.Add(attachment);

        //        message.From = new MailAddress("abc@gmail.com");
        //        message.To.Add(customemail);

        //        message.Subject = "Nhbs Annexure Report";
        //        message.IsBodyHtml = true;

        //        message.Body = "Please find Attached Report here.";
        //        message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess;
        //        SmtpClient smtp = new SmtpClient();
        //        smtp.Host = nhbshost;
        //        smtp.Port = nhbsport;
        //        smtp.Credentials = new System.Net.NetworkCredential(nhbscompemail, nhbspwd);
        //        smtp.EnableSsl = true;
        //        smtp.Send(message);

        //        memoryStream.Close();
        //        memoryStream.Dispose();
        //        return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (SmtpFailedRecipientException smtex)
        //    {
        //        return Json(new { Status = "Fail" }, JsonRequestBehavior.AllowGet);
        //    }
        //}
    }
}