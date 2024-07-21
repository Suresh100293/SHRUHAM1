using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class ExternalAttachmentController : BaseController
    {
        public string RefType = "";
        string attachmentPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentPath"].ToString()) == true ? "" : ConfigurationManager.AppSettings["AttachmentPath"].ToString();
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;

        #region Functions

        public JsonResult GetType(string term)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Value = "Vehic", Text = "Vehicle" });
            items.Add(new SelectListItem { Value = "Drive", Text = "Driver" });
            items.Add(new SelectListItem { Value = "Maste", Text = "Debtors/Creditors" });
            items.Add(new SelectListItem { Value = "CUSMA", Text = "Customer" });

            items.Add(new SelectListItem { Value = "LR000", Text = "Lorry Receipt" });
            items.Add(new SelectListItem { Value = "LR000", Text = "POD Received" });
            items.Add(new SelectListItem { Value = "LR000", Text = "Delivery" });

            items.Add(new SelectListItem { Value = "PCK00", Text = "Pick Order" });

            items.Add(new SelectListItem { Value = "FM000", Text = "Freight Memo" });
            items.Add(new SelectListItem { Value = "FM000", Text = "Loading" });
            items.Add(new SelectListItem { Value = "FM000", Text = "Unloading" });


            items.Add(new SelectListItem { Value = "SLR00", Text = "Sale Bill" });
            items.Add(new SelectListItem { Value = "SLW00", Text = "Sale Bill (WithOut Consignment)" });
            items.Add(new SelectListItem { Value = "CMM00", Text = "Cash Sale" });
            items.Add(new SelectListItem { Value = "BLSMT", Text = "Bill Submission(Party wise)" });

            items.Add(new SelectListItem { Value = "PUR00", Text = "Purchase Bill" });
            items.Add(new SelectListItem { Value = "CPH00", Text = "Cash Purchase" });

            items.Add(new SelectListItem { Value = "CPO00", Text = "Cash Bank Transaction" });
            items.Add(new SelectListItem { Value = "COT00", Text = "Journal Voucher With Cost " });


            if (!String.IsNullOrEmpty(term))
            {
                items = items.Where(x => term.Contains(x.Text)).ToList();
            }

            var Modified = items.Select(x => new
            {
                Code = x.Value,
                Name = x.Text
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public List<SelectListItem> FetList()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Value = "LR000", Text = "Lorry Receipt" });
            items.Add(new SelectListItem { Value = "FM000", Text = "Freight Memo" });
            items.Add(new SelectListItem { Value = "FM000", Text = "Loading" });
            items.Add(new SelectListItem { Value = "FM000", Text = "Unloading" });
            items.Add(new SelectListItem { Value = "LR000", Text = "Delivery" });
            items.Add(new SelectListItem { Value = "LR000", Text = "POD Received" });
            items.Add(new SelectListItem { Value = "Vehic", Text = "Vehicle" });
            items.Add(new SelectListItem { Value = "Drive", Text = "Driver" });
            items.Add(new SelectListItem { Value = "SLR00", Text = "Sale Bill" });
            items.Add(new SelectListItem { Value = "SLW00", Text = "Sale Bill (WithOut Consignment)" });
            items.Add(new SelectListItem { Value = "CMM00", Text = "Cash Sale" });
            items.Add(new SelectListItem { Value = "BLSMT", Text = "Bill Submission(Party wise)" });
            items.Add(new SelectListItem { Value = "PUR00", Text = "Purchase Bill" });
            items.Add(new SelectListItem { Value = "CPH00", Text = "Cash Purchase" });
            items.Add(new SelectListItem { Value = "Maste", Text = "Debtors/Creditors" });
            items.Add(new SelectListItem { Value = "CUSMA", Text = "Customer" });
            items.Add(new SelectListItem { Value = "PCK00", Text = "Pick Order" });
            items.Add(new SelectListItem { Value = "CPO00", Text = "Cash Bank Transaction" });
            items.Add(new SelectListItem { Value = "COT00", Text = "Journal Voucher With Cost " });
            //items.Add(new SelectListItem { Value = "Alert", Text = "Alert" });

            return items;
        }

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

        public JsonResult GetMaster(string term, string Type)
        {
            if (Type == "Vehic")
            {
                var result = ctxTFAT.VehicleMaster.Where(x => x.Code != "99998" && x.Code != "99999" && x.Acitve == true && x.TruckNo.Contains(term)).Select(m => new { m.Code, Name = m.TruckNo }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else if (Type == "Drive")
            {
                var result = ctxTFAT.DriverMaster.Where(x => x.Code != "99999" && x.Status == true && x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else if (Type == "Alert")
            {
                List<SelectListItem> items = new List<SelectListItem>();
                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    string query = "SELECT DocNo,Type,TypeCode FROM AlertNoteMaster ";
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
                                    Text = sdr["Type"].ToString() + " | " + sdr["TypeCode"].ToString(),
                                    Value = sdr["DocNo"].ToString()
                                });
                            }
                        }
                        con.Close();
                    }
                }
                if (!String.IsNullOrEmpty(term))
                {
                    items = items.Where(x => term.Contains(x.Text)).Take(10).ToList();
                }
                var Modified = items.Select(x => new
                {
                    Code = x.Value,
                    Name = x.Text
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
            else if (Type == "Maste")
            {
                var result = ctxTFAT.Master.Where(x => x.Hide == false && x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else if (Type == "CUSMA")
            {
                var result = ctxTFAT.CustomerMaster.Where(x => x.Hide == false && x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return null;
        }

        public string GetParameterName(string Type, string Parameter)
        {
            string ParameterName = "";
            if (Type == "Vehic")
            {
                List<SelectListItem> items = new List<SelectListItem>();
                items.Add(new SelectListItem { Value = "VehiclePhoto", Text = "VehiclePhoto" });
                items.Add(new SelectListItem { Value = "RCBook", Text = "RCBook" });
                items.Add(new SelectListItem { Value = "Fitness", Text = "Fitness" });
                items.Add(new SelectListItem { Value = "Insurance", Text = "Insurance" });
                items.Add(new SelectListItem { Value = "PUC", Text = "PUC" });
                items.Add(new SelectListItem { Value = "AIP_1_Year", Text = "AIP (1 Year)" });
                items.Add(new SelectListItem { Value = "StateTax_5_Year", Text = "State Tax (5 Year)" });
                items.Add(new SelectListItem { Value = "TPState", Text = "TP State" });
                items.Add(new SelectListItem { Value = "GreenTax", Text = "Green Tax" });
                items.Add(new SelectListItem { Value = "Other", Text = "Other" });
                ParameterName = items.Where(x => x.Value == Parameter).Select(x => x.Text).FirstOrDefault();
            }
            else if (Type == "Drive")
            {
                List<SelectListItem> items = new List<SelectListItem>();
                items.Add(new SelectListItem { Value = "DriverPhoto", Text = "Driver Photo" });
                items.Add(new SelectListItem { Value = "PanCard", Text = "Pan Card" });
                items.Add(new SelectListItem { Value = "Aadharcard", Text = "Aadhar card" });
                items.Add(new SelectListItem { Value = "License", Text = "License" });
                items.Add(new SelectListItem { Value = "BankDetails", Text = "Bank Details" });
                items.Add(new SelectListItem { Value = "Other", Text = "Other" });
                ParameterName = items.Where(x => x.Value == Parameter).Select(x => x.Text).FirstOrDefault();
            }


            return ParameterName;
        }

        public JsonResult GetParameters(string term, string Type)
        {
            if (Type == "Vehic")
            {
                List<SelectListItem> items = new List<SelectListItem>();
                items.Add(new SelectListItem { Value = "VehiclePhoto", Text = "VehiclePhoto" });
                items.Add(new SelectListItem { Value = "RCBook", Text = "RCBook" });
                items.Add(new SelectListItem { Value = "Fitness", Text = "Fitness" });
                items.Add(new SelectListItem { Value = "Insurance", Text = "Insurance" });
                items.Add(new SelectListItem { Value = "PUC", Text = "PUC" });
                items.Add(new SelectListItem { Value = "AIP_1_Year", Text = "AIP (1 Year)" });
                items.Add(new SelectListItem { Value = "StateTax_5_Year", Text = "State Tax (5 Year)" });
                items.Add(new SelectListItem { Value = "TPState", Text = "TP State" });
                items.Add(new SelectListItem { Value = "GreenTax", Text = "Green Tax" });
                items.Add(new SelectListItem { Value = "Other", Text = "Other" });
                var result = items.Select(m => new { Code = m.Value, Name = m.Text }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else if (Type == "Drive")
            {
                List<SelectListItem> items = new List<SelectListItem>();
                items.Add(new SelectListItem { Value = "DriverPhoto", Text = "Driver Photo" });
                items.Add(new SelectListItem { Value = "PanCard", Text = "Pan Card" });
                items.Add(new SelectListItem { Value = "Aadharcard", Text = "Aadhar card" });
                items.Add(new SelectListItem { Value = "License", Text = "License" });
                items.Add(new SelectListItem { Value = "BankDetails", Text = "Bank Details" });
                items.Add(new SelectListItem { Value = "Other", Text = "Other" });
                var result = items.Select(m => new { Code = m.Value, Name = m.Text }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return null;
        }

        #endregion
        // GET: Logistics/ExternalAttachment
        public ActionResult Index(ExternalAttachmentVM mModel)
        {
            Session["ETempAttach"] = null;
            Session["OTempAttach"] = null;
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");

            mdocument = mModel.Document;

            //mModel.Branches = PopulateBranches();

            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.Attachment.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    mModel.Type = mList.Type;
                    mModel.DocType = mList.DocType;
                    mModel.DocTypeN = GetParameterName(mModel.Type, mList.DocType);
                    mModel.ParentKey = mList.ParentKey;

                    var GetList = FetList();
                    mModel.TypeName = GetList.Where(x => x.Value == mList.Type).Select(x => x.Text).FirstOrDefault();
                    mModel.Srl = mList.Srl;
                    mModel.DocumentList = GetAttachmentListInEdit(mModel);
                    mModel.OLdDocumentList = GetOldAttachmentList(mModel);
                    Session["ETempAttach"] = mModel.DocumentList;
                    Session["OTempAttach"] = mModel.OLdDocumentList;
                    mModel.Srl = "";
                    if (mList.Type == "LR000")
                    {
                        var mlist = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString().Trim() == mList.ParentKey).FirstOrDefault();
                        if (mlist != null)
                        {
                            mlist.Source = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Source).Select(x => x.Name).FirstOrDefault();
                            mlist.Dest = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Dest).Select(x => x.Name).FirstOrDefault();
                            mlist.RecCode = ctxTFAT.Consigner.Where(x => x.Code == mlist.RecCode).Select(x => x.Name).FirstOrDefault();
                            mlist.SendCode = ctxTFAT.Consigner.Where(x => x.Code == mlist.SendCode).Select(x => x.Name).FirstOrDefault();
                            mModel.lrmater = mlist;
                            mModel.Srl = mList.Srl;

                        }
                    }
                    else if (mList.Type == "FM000")
                    {
                        var mlist = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString().Trim() == mList.ParentKey).FirstOrDefault();
                        if (mlist != null)
                        {
                            mlist.FromBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.FromBranch).Select(x => x.Name).FirstOrDefault();
                            mlist.ToBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.ToBranch).Select(x => x.Name).FirstOrDefault();
                            mlist.BroCode = ctxTFAT.Master.Where(x => x.Code == mlist.BroCode).Select(x => x.Name).FirstOrDefault();
                            mModel.FMMaster = mlist;
                            mModel.Srl = mList.Srl;

                        }
                    }
                    else if (mList.Type == "Vehic")
                    {
                        var mlist = ctxTFAT.VehicleMaster.Where(x => x.Code.ToString().Trim() == mList.ParentKey).FirstOrDefault();
                        if (mlist != null)
                        {
                            mlist.Driver = ctxTFAT.DriverMaster.Where(x => x.Code == mlist.Driver).Select(x => x.Name).FirstOrDefault();
                            mlist.Broker = ctxTFAT.Master.Where(x => x.Code == mlist.Broker).Select(x => x.Name).FirstOrDefault();
                            mModel.VehicleMaster = mlist;
                            mModel.extraSrl = mList.Srl;
                            mModel.extraSrlN = mlist.TruckNo;
                        }
                    }
                    else if (mList.Type == "Drive")
                    {
                        var mlist = ctxTFAT.DriverMaster.Where(x => x.Code.ToString().Trim() == mList.ParentKey).FirstOrDefault();
                        if (mlist != null)
                        {
                            mModel.DriverMaster = mlist;
                            mModel.extraSrl = mList.Srl;
                            mModel.extraSrlN = mlist.Name;
                        }
                    }
                    else if (mList.Type == "Maste")
                    {
                        var mlist = ctxTFAT.Master.Where(x => x.Code.ToString().Trim() == mList.ParentKey).FirstOrDefault();
                        if (mlist != null)
                        {
                            mlist.Code = mlist.AcType == "S" ? "Vendor/Creditor" : "Debtor";
                            mModel.Master = mlist;
                            mModel.extraSrl = mList.Srl;
                            mModel.extraSrlN = mlist.Name;
                        }
                    }
                    else if (mList.Type == "CUSMA")
                    {
                        var mlist = ctxTFAT.CustomerMaster.Where(x => x.Code.ToString().Trim() == mList.ParentKey).FirstOrDefault();
                        if (mlist != null)
                        {
                            mModel.customerMaster = mlist;
                            mModel.extraSrl = mList.Srl;
                            mModel.extraSrlN = mlist.Name;
                        }
                    }

                    else if (mList.Type == "SLR00" || mList.Type == "SLW00" || mList.Type == "CMM00")
                    {
                        var mlist = ctxTFAT.Sales.Where(x => x.TableKey.ToString().Trim() == mList.ParentKey && x.Type.Trim() == mModel.Type.Trim()).FirstOrDefault();
                        if (mlist != null)
                        {
                            if (mList.Type == "SLR00" || mList.Type == "SLW00")
                            {
                                mlist.Code = ctxTFAT.CustomerMaster.Where(x => x.Code == mlist.Code).Select(x => x.Name).FirstOrDefault();
                            }
                            else
                            {
                                mlist.Code = ctxTFAT.Master.Where(x => x.Code == mlist.Code).Select(x => x.Name).FirstOrDefault();
                            }
                            mlist.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Branch).Select(x => x.Name).FirstOrDefault();
                            mModel.sales = mlist;
                            mModel.Srl = mList.Srl;
                        }
                    }
                    else if (mList.Type == "BLSMT")
                    {
                        
                        var mlist = ctxTFAT.BillSubmission.Where(x => x.DocNo.ToString().Trim() == mList.ParentKey && x.DocType == "Party").FirstOrDefault();
                        if (mlist != null)
                        {
                            mlist.Party = ctxTFAT.CustomerMaster.Where(x => x.Code == mlist.Party).Select(x => x.Name).FirstOrDefault();
                            mlist.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Branch).Select(x => x.Name).FirstOrDefault();
                            mModel.billSubmission = mlist;
                            mModel.Srl = mList.Srl;
                        }
                    }
                    else if (mList.Type == "PUR00" || mList.Type == "CPH00")
                    {
                        
                        var mlist = ctxTFAT.Purchase.Where(x => x.TableKey.ToString().Trim() == mList.ParentKey && x.Type == mList.Type).FirstOrDefault();
                        if (mlist != null)
                        {
                            mlist.Code = ctxTFAT.Master.Where(x => x.Code == mlist.Account).Select(x => x.Name).FirstOrDefault();
                            mlist.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Branch).Select(x => x.Name).FirstOrDefault();
                            mModel.purchase = mlist;
                            mModel.Srl = mList.Srl;
                        }
                    }
                    else if (mList.Type == "CPO00" || mList.Type == "COT00")
                    {
                        
                        var mlist = ctxTFAT.RelateData.Where(x => x.ParentKey.ToString().Trim() == mList.ParentKey).FirstOrDefault();
                        if (mlist != null)
                        {
                            mlist.Code = ctxTFAT.Master.Where(x => x.Code == mlist.Code).Select(x => x.Name).FirstOrDefault();
                            mlist.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Branch).Select(x => x.Name).FirstOrDefault();
                            mModel.relateData = mlist;
                            mModel.Srl = mList.Srl;
                        }
                    }
                    else if (mList.Type == "PCK00")
                    {
                        var mlist = ctxTFAT.PickOrder.Where(x => x.OrderNo.ToString().Trim() == mModel.Srl).FirstOrDefault();
                        if (mlist != null)
                        {
                            mlist.Source = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Source).Select(x => x.Name).FirstOrDefault();
                            mlist.Dest = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Dest).Select(x => x.Name).FirstOrDefault();
                            mlist.RecCode = ctxTFAT.Consigner.Where(x => x.Code == mlist.RecCode).Select(x => x.Name).FirstOrDefault();
                            mlist.SendCode = ctxTFAT.Consigner.Where(x => x.Code == mlist.SendCode).Select(x => x.Name).FirstOrDefault();
                            mModel.pickOrder = mlist;
                            mModel.Srl = mList.Srl;
                        }
                    }

                }
            }
            else
            {

            }
            return View(mModel);
        }

        public ActionResult GetDocumentDetails(ExternalAttachmentVM mModel)
        {
            //BasicDetailsDocView
            List<ExternalAttachmentVM> vMs = new List<ExternalAttachmentVM>();
            var Status = "Error";
            string BasicHtml = "";
            if (mModel.Type == "LR000")
            {
                var mlist = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString().Trim() == mModel.Srl && x.Prefix == mperiod).FirstOrDefault();
                if (mlist != null)
                {
                    Status = "Success";
                    mlist.Source = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Source).Select(x => x.Name).FirstOrDefault();
                    mlist.Dest = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Dest).Select(x => x.Name).FirstOrDefault();
                    mlist.RecCode = ctxTFAT.Consigner.Where(x => x.Code == mlist.RecCode).Select(x => x.Name).FirstOrDefault();
                    mlist.SendCode = ctxTFAT.Consigner.Where(x => x.Code == mlist.SendCode).Select(x => x.Name).FirstOrDefault();
                    mModel.lrmater = mlist;
                    mModel.ParentKey = mlist.TableKey;
                    BasicHtml = ViewHelper.RenderPartialView(this, "BasicDetailsDocView", mModel);
                }
            }
            else if (mModel.Type == "FM000")
            {
                var mlist = ctxTFAT.FMMaster.Where(x => x.FmNo.ToString().Trim() == mModel.Srl && x.Prefix == mperiod).FirstOrDefault();
                if (mlist != null)
                {
                    Status = "Success";
                    mlist.FromBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.FromBranch).Select(x => x.Name).FirstOrDefault();
                    mlist.ToBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.ToBranch).Select(x => x.Name).FirstOrDefault();
                    mlist.BroCode = ctxTFAT.Master.Where(x => x.Code == mlist.BroCode).Select(x => x.Name).FirstOrDefault();
                    mModel.FMMaster = mlist;
                    mModel.ParentKey = mlist.TableKey;
                    BasicHtml = ViewHelper.RenderPartialView(this, "BasicDetailsDocView", mModel);
                }
            }
            else if (mModel.Type == "Vehic")
            {
                var mlist = ctxTFAT.VehicleMaster.Where(x => x.Code.ToString().Trim() == mModel.Srl).FirstOrDefault();
                if (mlist != null)
                {
                    Status = "Success";
                    mlist.Driver = ctxTFAT.DriverMaster.Where(x => x.Code == mlist.Driver).Select(x => x.Name).FirstOrDefault();
                    mlist.Broker = ctxTFAT.Master.Where(x => x.Code == mlist.Broker).Select(x => x.Name).FirstOrDefault();
                    mModel.VehicleMaster = mlist;
                    mModel.ParentKey = mlist.Code;
                    BasicHtml = ViewHelper.RenderPartialView(this, "BasicDetailsDocView", mModel);
                }
            }
            else if (mModel.Type == "Drive")
            {
                var mlist = ctxTFAT.DriverMaster.Where(x => x.Code.ToString().Trim() == mModel.Srl).FirstOrDefault();
                if (mlist != null)
                {
                    Status = "Success";
                    mModel.DriverMaster = mlist;
                    mModel.ParentKey = mlist.Code;
                    BasicHtml = ViewHelper.RenderPartialView(this, "BasicDetailsDocView", mModel);
                }
            }
            else if (mModel.Type == "SLR00" || mModel.Type == "SLW00" || mModel.Type == "CMM00")
            {
                var mlist = ctxTFAT.Sales.Where(x => x.Srl.ToString().Trim() == mModel.Srl && x.Type.Trim() == mModel.Type.Trim() && x.Prefix == mperiod).FirstOrDefault();
                if (mlist != null)
                {
                    Status = "Success";
                    if (mModel.Type == "SLR00" || mModel.Type == "SLW00")
                    {
                        mlist.Code = ctxTFAT.CustomerMaster.Where(x => x.Code == mlist.Code).Select(x => x.Name).FirstOrDefault();
                    }
                    else
                    {
                        mlist.Code = ctxTFAT.Master.Where(x => x.Code == mlist.CashBankCode).Select(x => x.Name).FirstOrDefault();
                    }
                    mlist.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Branch).Select(x => x.Name).FirstOrDefault();
                    mModel.sales = mlist;
                    mModel.ParentKey = mlist.TableKey;
                    BasicHtml = ViewHelper.RenderPartialView(this, "BasicDetailsDocView", mModel);

                }
            }
            else if (mModel.Type == "Maste")
            {
                var mlist = ctxTFAT.Master.Where(x => x.Code.ToString().Trim() == mModel.Srl).FirstOrDefault();
                if (mlist != null)
                {
                    Status = "Success";
                    mlist.Code = mlist.AcType == "S" ? "Vendor/Creditor" : "Debtor";
                    mModel.Master = mlist;
                    mModel.ParentKey = mModel.Srl;
                    BasicHtml = ViewHelper.RenderPartialView(this, "BasicDetailsDocView", mModel);
                }
            }
            else if (mModel.Type == "CUSMA")
            {
                var mlist = ctxTFAT.CustomerMaster.Where(x => x.Code.ToString().Trim() == mModel.Srl).FirstOrDefault();
                if (mlist != null)
                {
                    Status = "Success";
                    mModel.customerMaster = mlist;
                    mModel.ParentKey = mlist.Code;
                    BasicHtml = ViewHelper.RenderPartialView(this, "BasicDetailsDocView", mModel);
                }
            }
            else if (mModel.Type == "BLSMT")
            {
                var mlist = ctxTFAT.BillSubmission.Where(x => x.DocNo.ToString().Trim() == mModel.Srl && x.DocType== "Party" && x.Prefix == mperiod).FirstOrDefault();
                if (mlist != null)
                {
                    Status = "Success";
                    mlist.Party = ctxTFAT.CustomerMaster.Where(x => x.Code == mlist.Party).Select(x => x.Name).FirstOrDefault();
                    mModel.billSubmission = mlist;
                    mModel.ParentKey = mlist.DocNo;
                    BasicHtml = ViewHelper.RenderPartialView(this, "BasicDetailsDocView", mModel);
                }
            }
            else if (mModel.Type == "PUR00" || mModel.Type == "CPH00")
            {
                var Srl = Convert.ToInt32(mModel.Srl).ToString();
                var mlist = ctxTFAT.Purchase.Where(x => x.Srl.ToString().Trim() == Srl && x.Prefix == mperiod && x.Type == mModel.Type).FirstOrDefault();
                if (mlist != null)
                {
                    Status = "Success";
                    mlist.Code = ctxTFAT.Master.Where(x => x.Code == mlist.Account).Select(x => x.Name).FirstOrDefault();
                    mlist.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Branch).Select(x => x.Name).FirstOrDefault();
                    mModel.purchase = mlist;
                    mModel.ParentKey = mlist.TableKey;
                    BasicHtml = ViewHelper.RenderPartialView(this, "BasicDetailsDocView", mModel);
                }
            }
            else if (mModel.Type == "CPO00" || mModel.Type == "COT00")
            {
                var Srl = Convert.ToInt32(mModel.Srl).ToString();
                var mlist = ctxTFAT.RelateData.Where(x => x.Srl.ToString().Trim() == Srl && x.Prefix == mperiod).FirstOrDefault();
                if (mlist != null)
                {
                    Status = "Success";
                    mlist.Code = ctxTFAT.Master.Where(x => x.Code == mlist.Code).Select(x => x.Name).FirstOrDefault();
                    mlist.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Branch).Select(x => x.Name).FirstOrDefault();
                    mModel.relateData = mlist;
                    mModel.ParentKey = mlist.ParentKey;
                    BasicHtml = ViewHelper.RenderPartialView(this, "BasicDetailsDocView", mModel);
                }
            }
            else if (mModel.Type == "PCK00")
            {
                var mlist = ctxTFAT.PickOrder.Where(x => x.OrderNo.ToString().Trim() == mModel.Srl && x.Prefix == mperiod).FirstOrDefault();
                if (mlist != null)
                {
                    Status = "Success";
                    mlist.Source = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Source).Select(x => x.Name).FirstOrDefault();
                    mlist.Dest = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Dest).Select(x => x.Name).FirstOrDefault();
                    mlist.RecCode = ctxTFAT.Consigner.Where(x => x.Code == mlist.RecCode).Select(x => x.Name).FirstOrDefault();
                    mlist.SendCode = ctxTFAT.Consigner.Where(x => x.Code == mlist.SendCode).Select(x => x.Name).FirstOrDefault();
                    mModel.pickOrder = mlist;
                    mModel.ParentKey = mlist.OrderNo.ToString();
                    BasicHtml = ViewHelper.RenderPartialView(this, "BasicDetailsDocView", mModel);
                }
            }


            var jsonResult = Json(new { Status = Status, BasicHtml = BasicHtml }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult SaveData(ExternalAttachmentVM Model)
        {

            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {

                    if (Model.Type == "LR000")
                    {
                        var mlist = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString().Trim() == Model.ParentKey).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.ParentKey = mlist.TableKey;
                        }
                    }
                    else if (Model.Type == "PCK00")
                    {
                        var mlist = ctxTFAT.PickOrder.Where(x => x.OrderNo.ToString().Trim() == Model.Srl && x.Prefix == mperiod).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.ParentKey = mlist.OrderNo.ToString();
                        }
                    }
                    else if (Model.Type == "FM000")
                    {
                        var mlist = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString().Trim() == Model.ParentKey).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.ParentKey = mlist.TableKey;
                        }
                    }
                    else if (Model.Type == "Vehic")
                    {
                        var mlist = ctxTFAT.VehicleMaster.Where(x => x.Code.ToString().Trim() == Model.ParentKey).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.ParentKey = mlist.Code;
                        }
                    }
                    else if (Model.Type == "Drive")
                    {
                        var mlist = ctxTFAT.DriverMaster.Where(x => x.Code.ToString().Trim() == Model.ParentKey).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.ParentKey = mlist.Code;
                        }
                    }
                    else if (Model.Type == "CUSMA")
                    {
                        var mlist = ctxTFAT.CustomerMaster.Where(x => x.Code.ToString().Trim() == Model.ParentKey).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.ParentKey = mlist.Code;
                        }
                    }
                    else if (Model.Type == "Maste")
                    {
                        var mlist = ctxTFAT.Master.Where(x => x.Code.ToString().Trim() == Model.ParentKey).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.ParentKey = mlist.Code;
                        }
                    }
                    else if (Model.Type == "SLR00" || Model.Type == "SLW00" || Model.Type == "CMM00")
                    {
                        var mlist = ctxTFAT.Sales.Where(x => x.TableKey.ToString().Trim() == Model.ParentKey && x.Type == Model.Type).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.ParentKey = mlist.TableKey;
                        }
                    }
                    else if (Model.Type == "PUR00" || Model.Type=="CPH00")
                    {
                        var mlist = ctxTFAT.Purchase.Where(x => x.TableKey.ToString().Trim() == Model.ParentKey && x.Type == Model.Type).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.ParentKey = mlist.TableKey;
                        }
                    }
                    else if (Model.Type == "BLSMT" )
                    {
                        var mlist = ctxTFAT.BillSubmission.Where(x => x.DocNo.ToString().Trim() == Model.ParentKey && x.DocType == "Party").FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.ParentKey = mlist.DocNo;
                        }
                    }
                    else if (Model.Type == "CPO00" || Model.Type == "COT00")
                    {
                        var mlist = ctxTFAT.RelateData.Where(x => x.ParentKey.ToString().Trim() == Model.ParentKey).FirstOrDefault();
                        if (mlist != null)
                        {
                            Model.ParentKey = mlist.ParentKey;
                        }
                    }
                    if (Model.Mode == "Delete")
                    {
                        string Msg = DeleteStateMaster(Model);
                        transaction.Commit();
                        transaction.Dispose();
                        UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "EXTAT" + mperiod.Substring(0, 2) + Model.Srl, DateTime.Now, 0, "", " External Ataachment OF" + Model.Type + " : " + Model.Srl, "NA");
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }


                    var RemoveAttach = ctxTFAT.Attachment.Where(x => x.Code == Model.Document).ToList();
                    ctxTFAT.Attachment.RemoveRange(RemoveAttach);
                    //if (Directory.Exists(attachmentPath + Model.ParentKey))
                    //{
                    //    Directory.Delete(attachmentPath + Model.ParentKey, true);
                    //}
                    List<ExternalAttachmentVM> DocList = new List<ExternalAttachmentVM>();
                    if (Session["ETempAttach"] != null)
                    {
                        DocList = (List<ExternalAttachmentVM>)Session["ETempAttach"];
                    }

                    if (DocList != null && DocList.Count != 0)
                    {
                        var AttachCode = GetNewAttachCode();
                        foreach (var item in DocList.ToList())
                        {
                            var count = ctxTFAT.Attachment.Where(x => x.ParentKey == Model.ParentKey).ToList().Count + 1;
                            Directory.CreateDirectory(attachmentPath + Model.ParentKey);
                            string directoryPath = attachmentPath + Model.ParentKey + "/" + item.FileName;
                            byte[] IData = Convert.FromBase64String(item.ImageStr);
                            Attachment att = new Attachment();
                            att.AUTHIDS = String.IsNullOrEmpty(item.ENTEREDBY) == true ? muserid : item.ENTEREDBY;
                            att.DocDate = ConvertDDMMYYTOYYMMDD(item.DocDate);
                            att.AUTHORISE = mauthorise;
                            att.Branch = mbranchcode;
                            att.Code = AttachCode.ToString();
                            att.ENTEREDBY = String.IsNullOrEmpty(item.ENTEREDBY) == true ? muserid : item.ENTEREDBY;
                            att.FilePath = directoryPath;
                            att.LASTUPDATEDATE = DateTime.Now;
                            att.LocationCode = Model.LocationCode;
                            att.Prefix = mperiod;
                            att.Sno = count;
                            att.Srl = Model.Srl;
                            att.SrNo = count;
                            att.TableKey = Model.Type + mperiod.Substring(0, 2) + count.ToString("D3") + Model.Srl;
                            att.ParentKey = Model.ParentKey;
                            att.Type = Model.Type;
                            att.CompCode = mcompcode;
                            att.ExternalAttach = true;
                            att.RefDocNo = Model.Srl;
                            att.RefType = item.RefType;
                            att.DocType = item.DocType;
                            ctxTFAT.Attachment.Add(att);
                            ctxTFAT.SaveChanges();

                            System.IO.File.WriteAllBytes(directoryPath, IData);
                            ++AttachCode;
                            ++count;
                        }

                        ctxTFAT.SaveChanges();
                        transaction.Commit();
                        transaction.Dispose();
                        UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "EXTAT" + mperiod.Substring(0, 2) + Model.Srl, DateTime.Now, 0, "", " External Ataachment OF" + Model.Type + " : " + Model.Srl, "NA");

                    }
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Status = "Success", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
        }

        public string DeleteStateMaster(ExternalAttachmentVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return "Code not Entered..";
            }

            var RemoveAttach = ctxTFAT.Attachment.Where(x => x.Code == mModel.Document).ToList();
            ctxTFAT.Attachment.RemoveRange(RemoveAttach);
            ctxTFAT.SaveChanges();

            return "Success";

        }

        #region Attachment

        public List<ExternalAttachmentVM> GetAttachmentListInEdit(ExternalAttachmentVM Model)
        {

            List<ExternalAttachmentVM> AttachmentList = new List<ExternalAttachmentVM>();
            var docdetail = ctxTFAT.Attachment.Where(x => x.Code == Model.Document && x.Type != "Alert").ToList();
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
            foreach (var item in docdetail)
            {
                bool FoundAttachment = false;
                ExternalAttachmentVM AttachmentVM = new ExternalAttachmentVM();
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
                    AttachmentVM.tempId = item.Sno;
                    AttachmentVM.SrNo = item.Sno;
                    AttachmentVM.Path = item.FilePath;
                    AttachmentVM.FileContent = Path.GetExtension(item.FilePath);
                    AttachmentVM.ImageStr = Convert.ToBase64String(System.IO.File.ReadAllBytes(item.FilePath));
                    AttachmentVM.ImageData = Convert.FromBase64String(AttachmentVM.ImageStr);
                    AttachmentVM.tempIsDeleted = false;
                    AttachmentVM.HideDelete = Model.HideDelete;
                    AttachmentVM.ExternalAttach = item.ExternalAttach;
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

        public List<ExternalAttachmentVM> GetOldAttachmentList(ExternalAttachmentVM Model)
        {

            List<ExternalAttachmentVM> AttachmentList = new List<ExternalAttachmentVM>();
            var docdetail = ctxTFAT.Attachment.Where(x => x.Code != Model.Document && x.Srl == Model.Srl && (x.Type == Model.Type) && x.Type != "Alert").ToList();
            

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
            foreach (var item in docdetail)
            {
                bool FoundAttachment = false;
                ExternalAttachmentVM AttachmentVM = new ExternalAttachmentVM();
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
                    AttachmentVM.tempId = item.Sno;
                    AttachmentVM.SrNo = item.Sno;
                    AttachmentVM.Path = item.FilePath;
                    AttachmentVM.FileContent = Path.GetExtension(item.FilePath);
                    AttachmentVM.ImageStr = Convert.ToBase64String(System.IO.File.ReadAllBytes(item.FilePath));
                    AttachmentVM.ImageData = Convert.FromBase64String(AttachmentVM.ImageStr);
                    AttachmentVM.tempIsDeleted = false;
                    AttachmentVM.HideDelete = Model.HideDelete;
                    AttachmentVM.ExternalAttach = item.ExternalAttach;
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
        public ActionResult AttachDocument(ExternalAttachmentVM mModel)
        {
            List<ExternalAttachmentVM> DocList = new List<ExternalAttachmentVM>();
            if (Session["ETempAttach"] != null)
            {
                DocList = (List<ExternalAttachmentVM>)Session["ETempAttach"];
            }
            int n = DocList.Count() + 1;
            byte[] fileData = null;
            for (int i = 0; i < Request.Files.Count; i++)
            {
                ExternalAttachmentVM Model = new ExternalAttachmentVM();
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
                Model.RefType = "External Attachment";
                Model.Mode = mModel.Mode;
                Model.DocType = mModel.DocType;
                Model.ENTEREDBY = muserid;

                DocList.Add(Model);
            }
            Session["ETempAttach"] = DocList;

            DocList = DocList.ToList();

            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Logistics/Views/ExternalAttachment/AttatPartialView.cshtml", new ExternalAttachmentVM() { Srl = mModel.Srl, DocumentList = DocList });
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        [HttpPost]//Delete Attachement
        public ActionResult DeleteUploadFile(ExternalAttachmentVM Model)
        {
            List<ExternalAttachmentVM> DocList = new List<ExternalAttachmentVM>();
            if (Session["ETempAttach"] != null)
            {
                DocList = (List<ExternalAttachmentVM>)Session["ETempAttach"];
            }

            Model.DocumentList = DocList.Where(x => x.tempId != Model.tempId).Select(x => x).ToList();
            Session["ETempAttach"] = Model.DocumentList;
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
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Logistics/Views/ExternalAttachment/AttatPartialView.cshtml", Model);
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        //Dowbload Attachement
        public FileResult Download(string tempId)
        {
            List<ExternalAttachmentVM> DocList = new List<ExternalAttachmentVM>();
            if (Session["ETempAttach"] != null)
            {
                DocList = (List<ExternalAttachmentVM>)Session["ETempAttach"];
            }
            var dwnfile = DocList.Where(x => x.tempId.ToString() == tempId).FirstOrDefault();
            if (dwnfile==null)
            {
                if (Session["OTempAttach"] != null)
                {
                    DocList = (List<ExternalAttachmentVM>)Session["OTempAttach"];
                }
                dwnfile = DocList.Where(x => x.tempId.ToString() == tempId).FirstOrDefault();
            }
            string filename = dwnfile.FileName;
            byte[] fileBytes = Convert.FromBase64String(dwnfile.ImageStr);

            return File(fileBytes, ".jpg", filename);
        }

        [HttpPost]
        public ActionResult GetOldFile(ExternalAttachmentVM Model)
        {
            List<ExternalAttachmentVM> DocList = new List<ExternalAttachmentVM>();
            List<ExternalAttachmentVM> OldDocList = new List<ExternalAttachmentVM>();
            if (Session["ETempAttach"] != null)
            {
                DocList = (List<ExternalAttachmentVM>)Session["ETempAttach"];
            }
            OldDocList = GetOldAttachmentList(Model);
            foreach (var item in OldDocList)
            {
                item.tempId = DocList.OrderByDescending(x => x.tempId).Select(x => x.tempId).FirstOrDefault() + 1;
                OldDocList.Add(item);
            }
            Session["OTempAttach"] = OldDocList;
            Model.OLdDocumentList = OldDocList;

            var html = ViewHelper.RenderPartialView(this, "~/Areas/Logistics/Views/ExternalAttachment/OldAttatlistPartialView.cshtml", Model);
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
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
            List<ExternalAttachmentVM> DocList = new List<ExternalAttachmentVM>();
            if (Session["ETempAttach"] != null)
            {
                DocList = (List<ExternalAttachmentVM>)Session["ETempAttach"];
            }
            ExternalAttachmentVM AttachmentVM = new ExternalAttachmentVM();
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
                AttachmentVM.RefType = "External Attachment";
                AttachmentVM.Type = Type;
                AttachmentVM.DocType = DocType;
                AttachmentVM.Srl = Srl;
                AttachmentVM.Mode = Mode;
                AttachmentVM.ENTEREDBY = muserid;

                DocList.Add(AttachmentVM);
            }

            TempData["ETempAttach"] = DocList;
            AttachmentVM.DocumentList = DocList;
            var html = ViewHelper.RenderPartialView(this, "~/Areas/Logistics/Views/ExternalAttachment/AttatPartialView.cshtml", AttachmentVM);
            var jsonResult = Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }


        #endregion


    }
}