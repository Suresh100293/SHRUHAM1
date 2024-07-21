using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class ConsignorRulesforSMSEmailController : BaseController
    {
        private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mAUTHORISE = "A00";
        private static string mdocument = "";
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        private DataTable table = new DataTable();

        public JsonResult AutoCompleteType(string term)
        {

            var TypeList = ctxTFAT.ConsignorRule.Select(x => x.Type).ToList();
            return Json((from m in ctxTFAT.DocTypes
                         where m.Name.ToLower().Contains(term.ToLower()) && !TypeList.Contains(m.Code) && m.Code.Length == 5
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSMSTemplates(string term)
        {

            return Json((from m in ctxTFAT.MsgTemplate
                         select new { Name = m.Subject, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteUserID(string term)
        {
            var list = ctxTFAT.Consigner.Where(x => x.Acitve == true).ToList().Distinct();
            var ExistConsignor = ctxTFAT.ConsignorRule.Select(x => x.Code).ToList();
            list = list.Where(x => !(ExistConsignor.Contains(x.Code))).ToList();
            if (!(String.IsNullOrEmpty(term)))
            {
                list = ctxTFAT.Consigner.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList().Distinct();
            }

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        private List<SelectListItem> PopulateConsigners()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "select S.Code as Code,S.Name from Consigner S where S.Acitve='true' and S.Code Not In (select C.Code from ConsignorRule C) ";
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
                                Text = sdr["Name"].ToString(),
                                Value = sdr["Code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return items;
        }

        private List<SelectListItem> PopulateBranches()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM TfatBranch where Code='HO0000' or Category='Branch' or Category='SubBranch' ";
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
                                Text = sdr["Name"].ToString(),
                                Value = sdr["Code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return items;
        }

        public List<SelectListItem> PopulateFormats(string mType)
        {
            List<SelectListItem> StoreCodelst = new List<SelectListItem>();
            var result = ctxTFAT.DocFormats.Where(x => x.Type == mType && x.OutputDevice != "H").Select(x => new
            {
                x.FormatCode,

            }).ToList();
            foreach (var item in result)
            {
                StoreCodelst.Add(new SelectListItem { Text = item.FormatCode, Value = item.FormatCode.ToString() });
            }
            return StoreCodelst;
        }

        public JsonResult GetEmailAttachment(string term)
        {

            return Json((from m in ctxTFAT.DocFormats
                         where m.Type == "LR000"
                         select new { Name = m.FormatCode, Code = m.FormatCode }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        // GET: Logistics/ConsignorRulesforSMSEmail
        public ActionResult Index(ConsignorRulesforSMSEmailVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;
            Session["GridDataSession"] = null;

            var mType = ctxTFAT.DocTypes.Where(x => x.Code == "LR000").Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
            mModel.Type = mType != null ? mType.Code.ToString() : "";
            mModel.TypeName = mType != null ? mType.Name : "";

            mModel.Branches = PopulateBranches();
            mModel.Consignors = PopulateConsigners();
            mModel.EmailFormats = PopulateFormats("LR000");

            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.ConsignorRule.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    var consigner = ctxTFAT.Consigner.Where(x => x.Code == mModel.Document).FirstOrDefault();
                    mModel.Code = consigner.Code;
                    mModel.CodeName = consigner.Name;
                    mModel.AppBranch = mList.Branch;
                    mModel.SendEmail = mList.EmailReq;
                    mModel.EmailFormat = mList.PartyEmail;
                    mModel.EmailFormatL = mList.PartyEmail;
                    mModel.SendSMS = mList.SMSReq;
                    mModel.SMSTemplate = mList.SMSTemp;
                    mModel.SMSTemplateName = mList.SMSTemp;
                }
            }

            return View(mModel);
        }

        #region GridOperations
        // following action is used when row is added to grid in add mode
        [HttpPost]
        public ActionResult AddToTable(ConsignorRulesforSMSEmailVM Model)
        {
            string Status = "Success", Message = "";
            Model.Branches = PopulateBranches();
            Model.EmailFormats = PopulateFormats(Model.Type);
            List<SelectListItem> SelectedBranch = new List<SelectListItem>();
            if (!String.IsNullOrEmpty(Model.AppBranch))
            {
                var ListBtanchCode = Model.AppBranch.Split(',').ToList();
                SelectedBranch = Model.Branches.Where(x => ListBtanchCode.Contains(x.Value)).ToList();
            }
            List<SelectListItem> SelectedEmail = new List<SelectListItem>();
            if (!String.IsNullOrEmpty(Model.EmailFormat))
            {
                var ListBtanchCode = Model.EmailFormat.Split(',').ToList();
                SelectedEmail = Model.EmailFormats.Where(x => ListBtanchCode.Contains(x.Value)).ToList();
            }

            List<ConsignorRulesforSMSEmailVM> objgriddetail = new List<ConsignorRulesforSMSEmailVM>();
            if (Session["GridDataSession"] != null)
            {
                objgriddetail = (List<ConsignorRulesforSMSEmailVM>)Session["GridDataSession"];
            }

            if (objgriddetail.Where(x => x.Code == Model.Code).FirstOrDefault() == null)
            {
                objgriddetail.Add(new ConsignorRulesforSMSEmailVM()
                {
                    Code = Model.Code,
                    CodeName = Model.CodeName,

                    SendEmail = Model.SendEmail,
                    EmailFormat = Model.EmailFormat,
                    EmailFormats = SelectedEmail,
                    SendSMS = Model.SendSMS,
                    SMSTemplate = Model.SMSTemplate,
                    SMSTemplateName = Model.SMSTemplateName,
                    tEmpID = objgriddetail.Count + 1,
                    AppBranch = Model.AppBranch,
                    Branches = SelectedBranch,
                });
            }
            else
            {
                Status = "Error";
                Message = "Same User Created So Cant Create Duplicate ..!";
            }

            Session.Add("GridDataSession", objgriddetail);
            Model.AppBranch = "";
            var html = ViewHelper.RenderPartialView(this, "GridDataView", new ConsignorRulesforSMSEmailVM() { EmailFormats = Model.EmailFormats, Branches = Model.Branches, GridDataVM = objgriddetail, Mode = "Add" });
            return Json(new
            {
                GridDataVM = objgriddetail,
                Html = html,
                Mode = "Add",
                Status = Status,
                Message = Message
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PickFromTable(ConsignorRulesforSMSEmailVM Model)
        {
            Model.Branches = PopulateBranches();
            Model.EmailFormats = PopulateFormats(Model.Type);
            var result = (List<ConsignorRulesforSMSEmailVM>)Session["GridDataSession"];
            var result1 = result.Where(x => x.tEmpID == Model.tEmpID);
            foreach (var item in result1)
            {
                Model.Code = item.Code;
                Model.CodeName = item.CodeName;
                Model.SendEmail = item.SendEmail;
                Model.EmailFormat = item.EmailFormat;
                Model.SendSMS = item.SendSMS;
                Model.SMSTemplate = item.SMSTemplate;
                Model.SMSTemplateName = item.SMSTemplateName;
                Model.tEmpID = item.tEmpID;
                Model.GridDataVM = result;
                Model.AppBranch = item.AppBranch;
            }
            return Json(new
            {
                Html = this.RenderPartialView("GridDataView", Model),
                AppBranch = Model.AppBranch,
                EmailFormats = Model.EmailFormats
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult AddToTableEdit(ConsignorRulesforSMSEmailVM Model)
        {
            Model.Branches = PopulateBranches();
            Model.EmailFormats = PopulateFormats(Model.Type);

            List<SelectListItem> SelectedBranch = new List<SelectListItem>();
            if (!String.IsNullOrEmpty(Model.AppBranch))
            {
                var ListBtanchCode = Model.AppBranch.Split(',').ToList();
                SelectedBranch = Model.Branches.Where(x => ListBtanchCode.Contains(x.Value)).ToList();
            }
            List<SelectListItem> SelectedEmail = new List<SelectListItem>();
            if (!String.IsNullOrEmpty(Model.EmailFormat))
            {
                var ListBtanchCode = Model.EmailFormat.Split(',').ToList();
                SelectedEmail = Model.EmailFormats.Where(x => ListBtanchCode.Contains(x.Value)).ToList();
            }
            var result = (List<ConsignorRulesforSMSEmailVM>)Session["GridDataSession"];
            foreach (var item in result.Where(x => x.tEmpID == Model.tEmpID))
            {
                item.Code = Model.Code;
                item.CodeName = Model.CodeName;
                item.SendEmail = Model.SendEmail;
                item.EmailFormat = Model.EmailFormat;
                item.EmailFormats = SelectedEmail;
                item.SendSMS = Model.SendSMS;
                item.SMSTemplate = Model.SMSTemplate;
                item.SMSTemplateName = Model.SMSTemplateName;
                item.tEmpID = Model.tEmpID;
                item.AppBranch = Model.AppBranch;
                item.Branches = SelectedBranch;
            }
            Session.Add("GridDataSession", result);
            var html = ViewHelper.RenderPartialView(this, "GridDataView", new ConsignorRulesforSMSEmailVM() { EmailFormats = Model.EmailFormats, Branches = Model.Branches, GridDataVM = result, Mode = "Add" });
            return Json(new
            {
                GridDataVM = result,
                Html = html,
                Mode = "Add"
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteTableRow(int tEmpID, string Type)
        {
            ConsignorRulesforSMSEmailVM Model = new ConsignorRulesforSMSEmailVM();
            Model.Type = Type;
            Model.EmailFormats = PopulateFormats(Model.Type);
            var result = (List<ConsignorRulesforSMSEmailVM>)Session["GridDataSession"];
            result.Where(x => x.tEmpID == tEmpID).FirstOrDefault();
            result = result.Where(x => x.tEmpID != tEmpID).ToList();
            int i = 1;
            foreach (var item in result)
            {
                item.tEmpID = i++;
            }
            Session.Add("GridDataSession", result);
            var html = ViewHelper.RenderPartialView(this, "GridDataView", new ConsignorRulesforSMSEmailVM() { EmailFormats = Model.EmailFormats, Branches = PopulateBranches(), GridDataVM = result });
            return Json(new { SelectedIndent = result, Html = html }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region SaveData
        public ActionResult SaveData(ConsignorRulesforSMSEmailVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteTransactionAuthorisationRules(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }

                    if (mModel.Mode == "Add")
                    {
                        foreach (var item in mModel.Code.Split(',').ToList())
                        {
                            ConsignorRule consignorRule = new ConsignorRule();
                            consignorRule.DocBranch = mbranchcode;
                            consignorRule.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                            consignorRule.CompCode = mcompcode;
                            consignorRule.Type = mModel.Type;
                            consignorRule.Code = item;
                            consignorRule.Branch = mModel.AppBranch;
                            consignorRule.EmailReq = mModel.SendEmail;
                            consignorRule.PartyEmail = mModel.SendEmail == true ? mModel.EmailFormat : "";
                            consignorRule.SMSReq = mModel.SendSMS;
                            consignorRule.SMSTemp = mModel.SendSMS == true ? mModel.SMSTemplate : "";
                            consignorRule.ENTEREDBY = muserid;
                            consignorRule.AUTHIDS = muserid;
                            consignorRule.AUTHORISE = mAUTHORISE;
                            consignorRule.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                            ctxTFAT.ConsignorRule.Add(consignorRule);
                        }
                    }
                    else
                    {
                        ConsignorRule consignorRule = ctxTFAT.ConsignorRule.Where(x => x.Code == mModel.Document).FirstOrDefault();
                        consignorRule.Branch = mModel.AppBranch;
                        consignorRule.EmailReq = mModel.SendEmail;
                        consignorRule.PartyEmail = mModel.SendEmail == true ? mModel.EmailFormat : "";
                        consignorRule.SMSReq = mModel.SendSMS;
                        consignorRule.SMSTemp = mModel.SendSMS == true ? mModel.SMSTemplate : "";
                        consignorRule.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                        ctxTFAT.Entry(consignorRule).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "Save Consignor / Consignee Rule For SMS/EMAIL", "NA");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "TransactionAuthorisationRules" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "TransactionAuthorisationRules" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "TransactionAuthorisationRules" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "TransactionAuthorisationRules" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteTransactionAuthorisationRules(ConsignorRulesforSMSEmailVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Document Missing..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var mList2 = ctxTFAT.ConsignorRule.Where(x => x.Code == mModel.Document).ToList();
            ctxTFAT.ConsignorRule.RemoveRange(mList2);
            ctxTFAT.SaveChanges();
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, mModel.Document, "Delete Consignor / Consignee Rule For SMS/EMAIL", "CONS");

            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData


    }
}