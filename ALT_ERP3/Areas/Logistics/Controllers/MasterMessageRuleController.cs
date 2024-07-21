using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class MasterMessageRuleController : BaseController
    {
        private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mauthorise = "A00";
        private static int mdocument = 0;

        #region GetLists
        public ActionResult GetSearchList()
        {
            List<SelectListItem> CallItemOptionList = new List<SelectListItem>();

            CallItemOptionList.Add(new SelectListItem { Value = "MainType", Text = "MainType" });//2
            CallItemOptionList.Add(new SelectListItem { Value = "SubType", Text = "SubType" });//3
            CallItemOptionList.Add(new SelectListItem { Value = "Code", Text = "Code" });//3
            CallItemOptionList.Add(new SelectListItem { Value = "Name", Text = "Description" });//3
            return Json(CallItemOptionList, JsonRequestBehavior.AllowGet);
        }

        private List<SelectListItem> PopulateBranch()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM TfatBranch where Code<>'G00000'  and Category <> 'Area' order by Name ";
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

        #endregion GetLists

        // GET: Logistics/MasterMessageRule
        public ActionResult Index(MasterMessageRuleVM mModel)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, "Master Message Rules", "", DateTime.Now, 0, "", "", "NA");
            List<MasterMessageRuleVM> mLeftList = new List<MasterMessageRuleVM>();
            var mlist = ctxTFAT.DocTypes.Where(x => x.MainType == "MM" && x.MainType != x.SubType && x.Code != x.SubType).Select(x => x).OrderBy(x => x.Name).ToList();
            foreach (var i in mlist)
            {
                mLeftList.Add(new MasterMessageRuleVM()
                {
                    DocTypes_Code = i.Code,
                    DocTypes_Name = i.Name,
                    DocTypes_MainType = i.MainType,
                    DocTypes_SubType = i.SubType,
                });
            }
            mModel.MSGBranchs = PopulateBranch();
            mModel.EmailBranchs = PopulateBranch();

            mModel.mLeftList = mLeftList;
            return View(mModel);
        }

        [HttpPost]
        public ActionResult GetLeftGrid(MasterMessageRuleVM mModel)
        {
            List<MasterMessageRuleVM> mLeftList = new List<MasterMessageRuleVM>();
            List<DocTypes> mlist = new List<DocTypes>();
            if (string.IsNullOrEmpty(mModel.SearchContent) == false)
            {
                mlist = ctxTFAT.DocTypes.Where(x => x.MainType == "MM" && x.MainType != x.SubType && x.Code != x.SubType && x.Name.ToLower().Contains(mModel.SearchContent)).Select(x => x).OrderBy(x => x.Name).ToList();
            }
            else
            {
                mlist = ctxTFAT.DocTypes.Where(x => x.MainType == "MM" && x.MainType != x.SubType && x.Code != x.SubType && x.AppBranch == mbranchcode).Select(x => x).OrderBy(x => x.Name).ToList();
            }
            foreach (var i in mlist.Where(x => x.Code.Length >= 3))
            {
                mLeftList.Add(new MasterMessageRuleVM()
                {
                    DocTypes_Code = i.Code,
                    DocTypes_Name = i.Name,
                    DocTypes_MainType = i.MainType,
                    DocTypes_SubType = i.SubType,
                });
            }
            var html = ViewHelper.RenderPartialView(this, "PartialTypeView", new MasterMessageRuleVM { mLeftList = mLeftList });
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ClickLeftGrid(MasterMessageRuleVM mModel)
        {
            mModel.MSGBranchs = PopulateBranch();
            mModel.EmailBranchs = PopulateBranch();

            var result = (from mrightlink in ctxTFAT.TfatPass
                          where mrightlink.AppBranch.Contains(mbranchcode)
                          join xMessageRules in ctxTFAT.MasterMessageRules.Where(x => x.Type == mModel.DocTypes_Code) on mrightlink.Code equals xMessageRules.Code into ljointresult
                          from finalresult in ljointresult.DefaultIfEmpty()
                          select new
                          {
                              mrightlink.Code,
                              SMSBranchL = finalresult != null ? finalresult.SmsBranch.ToString() : null,
                              MessageRules_SendSMS = finalresult != null ? finalresult.SendSMS : false,
                              SMSTemplate = finalresult != null ? finalresult.SMSTemp : null,

                              EmailBranchL = finalresult != null ? finalresult.EmailBranch.ToString() : null,
                              EmailMessageRules_xAdd = finalresult != null ? finalresult.EmailAdd : false,
                              EmailMessageRules_xEdit = finalresult != null ? finalresult.EmaiEdit : false,
                              EmailMessageRules_xDelete = finalresult != null ? finalresult.EmaiDelete : false,
                              EmailMessageRules_xPrint = finalresult != null ? finalresult.EmaiPrint : false,
                              EmailMessageRules_xBackDated = finalresult != null ? finalresult.EmaiBackDated : false,

                              MSGBranchL = finalresult != null ? finalresult.MsgBranch.ToString() : null,
                              MSGMessageRules_xAdd = finalresult != null ? finalresult.MsgAdd : false,
                              MSGMessageRules_xEdit = finalresult != null ? finalresult.MsgEdit : false,
                              MSGMessageRules_xDelete = finalresult != null ? finalresult.MsgDelete : false,
                              MSGMessageRules_xPrint = finalresult != null ? finalresult.MsgPrint : false,
                              MSGMessageRules_xBackDated = finalresult != null ? finalresult.MsgBackDated : false,

                              LimitAmount = finalresult != null ? finalresult.LimitAmount.Value : 0,

                          }).OrderBy(x => x.Code).ToList();

            List<MasterMessageRuleVM> mlist = new List<MasterMessageRuleVM>();
            foreach (var i in result)
            {
                mlist.Add(new MasterMessageRuleVM()
                {
                    MessageRules_Code = i.Code,

                    //SMSBranchL = i.SMSBranchL,
                    //MessageRules_SendSMS = i.MessageRules_SendSMS,
                    //SMSTemplate = i.SMSTemplate,

                    EmailBranchL = i.EmailBranchL,
                    EmailMessageRules_xAdd = i.EmailMessageRules_xAdd,
                    EmailMessageRules_xEdit = i.EmailMessageRules_xEdit,
                    EmailMessageRules_xDelete = i.EmailMessageRules_xDelete,
                    EmailMessageRules_xPrint = i.EmailMessageRules_xPrint,
                    EmailMessageRules_xBackDated = i.EmailMessageRules_xBackDated,

                    MSGBranchL = i.MSGBranchL,
                    MSGMessageRules_xAdd = i.MSGMessageRules_xAdd,
                    MSGMessageRules_xEdit = i.MSGMessageRules_xEdit,
                    MSGMessageRules_xDelete = i.MSGMessageRules_xDelete,
                    MSGMessageRules_xPrint = i.MSGMessageRules_xPrint,
                    MSGMessageRules_xBackDated = i.MSGMessageRules_xBackDated,

                    MessageRules_LimitAmount = i.LimitAmount,

                });
            }
            var html = ViewHelper.RenderPartialView(this, "TransactionMessageRulesPartial", new MasterMessageRuleVM { EmailBranchs = mModel.EmailBranchs,  MSGBranchs = mModel.MSGBranchs,  mRightList = mlist, MessageRules_Type = mModel.DocTypes_Code });
            return Json(new { mRightList = mlist, MessageRules_Type = mModel.DocTypes_Code, Html = html }, JsonRequestBehavior.AllowGet);
        }

        #region SaveData
        public ActionResult SaveData(MasterMessageRuleVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var mobjstk1 = ctxTFAT.MasterMessageRules.Where(x => x.Type == mModel.MessageRules_Type).ToList();
                    ctxTFAT.MasterMessageRules.RemoveRange(mobjstk1);
                    ctxTFAT.SaveChanges();
                    foreach (var eachvalue in mModel.mRightList)
                    {
                        MasterMessageRules mobj = new MasterMessageRules();
                        mobj.Type = mModel.MessageRules_Type;
                        mobj.Code = eachvalue.MessageRules_Code;
                        mobj.LimitAmount = eachvalue.MessageRules_LimitAmount;

                        mobj.SendSMS = false;// eachvalue.MessageRules_SendSMS;

                        //if (eachvalue.MessageRules_SendSMS)
                        //{
                        //    mobj.SMSTemp = eachvalue.SMSTemplate;
                        //    mobj.SmsBranch = eachvalue.SMSBranchL;
                        //}

                        mobj.EmailBranch = eachvalue.EmailBranchL;
                        mobj.EmailAdd = eachvalue.EmailMessageRules_xAdd;
                        mobj.EmaiEdit = eachvalue.EmailMessageRules_xEdit;
                        mobj.EmaiDelete = eachvalue.EmailMessageRules_xDelete;
                        mobj.EmaiPrint = eachvalue.EmailMessageRules_xPrint;
                        mobj.EmaiBackDated = eachvalue.EmailMessageRules_xBackDated;

                        mobj.MsgBranch = eachvalue.MSGBranchL;
                        mobj.MsgAdd = eachvalue.MSGMessageRules_xAdd;
                        mobj.MsgEdit = eachvalue.MSGMessageRules_xEdit;
                        mobj.MsgDelete = eachvalue.MSGMessageRules_xDelete;
                        mobj.MsgPrint = eachvalue.MSGMessageRules_xPrint;
                        mobj.MsgBackDated = eachvalue.MSGMessageRules_xBackDated;




                        mobj.AUTHIDS = muserid;
                        mobj.AUTHORISE = mauthorise;
                        mobj.CompCode = mcompcode;
                        mobj.ENTEREDBY = muserid;
                        mobj.LASTUPDATEDATE = DateTime.Now;
                        ctxTFAT.MasterMessageRules.Add(mobj);
                        ctxTFAT.SaveChanges();
                    }
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, "Master Message Rules", "", DateTime.Now, 0, mModel.MessageRules_Type, "Save Master Message Rules", "D");

                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    string dd1 = ex1.InnerException.Message;
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    string dd = ex.InnerException.Message;
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                }
            }
            return Json(new { Status = "Success", id = "MessageRules" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData


    }
}