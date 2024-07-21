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
    public class TransactionRulesforSMSEmailController : BaseController
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

        public ActionResult GetSMSTemplates()
        {
            List<SelectListItem> StoreCodelst = new List<SelectListItem>();
            var result = ctxTFAT.MsgTemplate.Select(m => new
            {
                m.Code,
                m.Subject
            }).ToList();
            foreach (var item in result)
            {
                StoreCodelst.Add(new SelectListItem { Text = item.Subject, Value = item.Code.ToString() });
            }
            return Json(StoreCodelst, JsonRequestBehavior.AllowGet);
        }
        

        public ActionResult GetEmailMsgTemplates()
        {
            List<SelectListItem> StoreCodelst = new List<SelectListItem>();
            var result = ctxTFAT.MsgTemplate.Select(m => new
            {
                m.Code,
                m.Subject
            }).ToList();
            foreach (var item in result)
            {
                StoreCodelst.Add(new SelectListItem { Text = item.Subject, Value = item.Code.ToString() });
            }
            return Json(StoreCodelst, JsonRequestBehavior.AllowGet);
        }

        private List<SelectListItem> PopulateUsers()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM TfatPass order by Recordkey ";
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
        private List<SelectListItem> PopulateTemplates()
        {
            List<SelectListItem> StoreCodelst = new List<SelectListItem>();
            var result = ctxTFAT.MsgTemplate.Select(m => new
            {
                m.Code,
                m.Subject
            }).ToList();
            foreach (var item in result)
            {
                StoreCodelst.Add(new SelectListItem { Text = item.Subject, Value = item.Code.ToString() });
            }
            return StoreCodelst;
        }
        private List<SelectListItem> PopulateBranch()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM TfatBranch where Code<>'G00000' and Category <> 'Area'  order by Name ";
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
        #endregion GetLists

        // GET: Logistics/TransactionRulesforSMSEmail
        public ActionResult Index(TransactionSMSModel mModel)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, "Partywise Transaction Rules for SMS/E-mail", "", DateTime.Now, 0, "", "","NA");
            List<TransactionSMSModel> mLeftList = new List<TransactionSMSModel>();
            var mlist = ctxTFAT.DocTypes.Where(x => x.MainType != x.SubType && x.Code != x.SubType).Select(x => x).OrderBy(x => x.Name).ToList();
            foreach (var i in mlist)
            {
                mLeftList.Add(new TransactionSMSModel()
                {
                    DocTypes_Code = i.Code,
                    DocTypes_Name = i.Name,
                    DocTypes_MainType = i.MainType,
                    DocTypes_SubType = i.SubType,
                });
            }
            mModel.SMSTemplates = PopulateTemplates();

            mModel.Userss = PopulateUsers();

            mModel.PartyBranchs = PopulateBranch();
            mModel.UserBranchs = PopulateBranch();
            mModel.PrintFormats = PopulateFormats("");

            mModel.mLeftList = mLeftList;
            return View(mModel);
        }

        [HttpPost]
        public ActionResult GetLeftGrid(TransactionSMSModel mModel)
        {
            List<TransactionSMSModel> mLeftList = new List<TransactionSMSModel>();
            List<DocTypes> mlist = new List<DocTypes>();
            if (string.IsNullOrEmpty(mModel.SearchContent) == false)
            {
                mlist = ctxTFAT.DocTypes.Where(x => x.MainType!="MM" && x.MainType != x.SubType && x.Code != x.SubType && x.Name.ToLower().Contains(mModel.SearchContent)).Select(x => x).OrderBy(x => x.Name).ToList();
            }
            else
            {
                mlist = ctxTFAT.DocTypes.Where(x => x.MainType != "MM" && x.MainType != x.SubType && x.Code != x.SubType).Select(x => x).OrderBy(x => x.Name).ToList();
            }
            foreach (var i in mlist)
            {
                mLeftList.Add(new TransactionSMSModel()
                {
                    DocTypes_Code = i.Code,
                    DocTypes_Name = i.Name,
                    DocTypes_MainType = i.MainType,
                    DocTypes_SubType = i.SubType,
                });
            }
            
            var html = ViewHelper.RenderPartialView(this, "PartialTypeView", new TransactionSMSModel { mLeftList = mLeftList });
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ClickLeftGrid(TransactionSMSModel mModel)
        {
            List<TransactionSMSModel> mlist = new List<TransactionSMSModel>();
            mModel.SMSTemplates = PopulateTemplates();
            mModel.Userss = PopulateUsers();
            mModel.PartyBranchs = PopulateBranch();
            mModel.UserBranchs = PopulateBranch();
            mModel.PrintFormats = PopulateFormats(mModel.DocTypes_Code);
            if (mModel.DocTypes_Code.Trim().ToUpper()=="LR000" || mModel.DocTypes_Code.Trim().ToUpper() == "DELV0")
            {
                var result = (from mrightlink in ctxTFAT.CustomerMaster
                                  //where (mrightlink.AppBranch.Contains(mbranchcode) || mrightlink.AppBranch == "All") 
                              join xMessageRules in ctxTFAT.SMSRules.Where(x => x.Type == mModel.DocTypes_Code) on mrightlink.Code equals xMessageRules.Code into ljointresult
                              from finalresult in ljointresult.DefaultIfEmpty()
                              select new
                              {
                                  mrightlink.Code,
                                  mrightlink.Name,

                                  PartyBranch = finalresult != null ? finalresult.PartyBranch.ToString() : null,
                                  PartyEmail = finalresult != null ? finalresult.PartyEmail : false,
                                  PartySMS = finalresult != null ? finalresult.PartySMS : false,
                                  PartySMSTemp = finalresult != null ? finalresult.PartySMSTemp.ToString() : null,
                                  PartyAttach = finalresult != null ? finalresult.PartyEmailAttachments.ToString() : null,
                                  UserAttach = finalresult != null ? finalresult.UserEmailAttachments.ToString() : null,


                                  UserBranch = finalresult != null ? finalresult.UserBranch.ToString() : null,
                                  UsersList = finalresult != null ? finalresult.UsersList.ToString() : null,
                                  UserEmail = finalresult != null ? finalresult.UserEmail : false,
                                  UserMsg = finalresult != null ? finalresult.UserMsg : false,
                                  UserSMS = finalresult != null ? finalresult.UserSMS : false,

                              }).OrderBy(x => x.Name).ToList();

                foreach (var i in result)
                {
                    mlist.Add(new TransactionSMSModel()
                    {
                        PartyId = i.Code,
                        PartyName = i.Name,
                        SMSTemplate = i.PartySMSTemp,

                        PartySendEmail = i.PartyEmail,
                        PartySendSMS = i.PartySMS,
                        PartyBranchL = i.PartyBranch,

                        UserBranchL = i.UserBranch,
                        UsersL = i.UsersList,
                        UserSendEmail = i.UserEmail,
                        UserSendMSG = i.UserMsg,
                        UserSendSMS = i.UserSMS,
                        PartyPrintFormatL = i.PartyAttach,
                        UserPrintFormatL = i.UserAttach,

                    });
                }
            }
            else if (mModel.DocTypes_Code.Trim().ToUpper() == "FM000" || mModel.DocTypes_Code.Trim().ToUpper() == "FMH00")
            {
                var result = (from mrightlink in ctxTFAT.Master
                                  where (mrightlink.OthPostType.Contains("B")) 
                              join xMessageRules in ctxTFAT.SMSRules.Where(x => x.Type == mModel.DocTypes_Code) on mrightlink.Code equals xMessageRules.Code into ljointresult
                              from finalresult in ljointresult.DefaultIfEmpty()
                              select new
                              {
                                  mrightlink.Code,
                                  mrightlink.Name,

                                  PartyBranch = finalresult != null ? finalresult.PartyBranch.ToString() : null,
                                  PartyEmail = finalresult != null ? finalresult.PartyEmail : false,
                                  PartySMS = finalresult != null ? finalresult.PartySMS : false,
                                  PartySMSTemp = finalresult != null ? finalresult.PartySMSTemp.ToString() : null,
                                  PartyAttach = finalresult != null ? finalresult.PartyEmailAttachments.ToString() : null,
                                  UserAttach = finalresult != null ? finalresult.UserEmailAttachments.ToString() : null,


                                  UserBranch = finalresult != null ? finalresult.UserBranch.ToString() : null,
                                  UsersList = finalresult != null ? finalresult.UsersList.ToString() : null,
                                  UserEmail = finalresult != null ? finalresult.UserEmail : false,
                                  UserMsg = finalresult != null ? finalresult.UserMsg : false,
                                  UserSMS = finalresult != null ? finalresult.UserSMS : false,

                              }).OrderBy(x => x.Name).ToList();

                
                foreach (var i in result)
                {
                    mlist.Add(new TransactionSMSModel()
                    {
                        PartyId = i.Code,
                        PartyName = i.Name,
                        SMSTemplate = i.PartySMSTemp,

                        PartySendEmail = i.PartyEmail,
                        PartySendSMS = i.PartySMS,
                        PartyBranchL = i.PartyBranch,

                        UserBranchL = i.UserBranch,
                        UsersL = i.UsersList,
                        UserSendEmail = i.UserEmail,
                        UserSendMSG = i.UserMsg,
                        UserSendSMS = i.UserSMS,
                        PartyPrintFormatL = i.PartyAttach,
                        UserPrintFormatL = i.UserAttach,

                    });
                }
            }
            
            var html = ViewHelper.RenderPartialView(this, "TransactionMessageRulesPartial", new TransactionSMSModel { mRightList = mlist, DocTypes_Code = mModel.DocTypes_Code, SMSTemplates = mModel.SMSTemplates, Userss = mModel.Userss, PartyBranchs = mModel.PartyBranchs, UserBranchs = mModel.UserBranchs, PrintFormats = mModel.PrintFormats });
            var jsonResult = Json(new { mRightList = mlist, Type = mModel.DocTypes_Code, Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        #region SaveData
        public ActionResult SaveData(TransactionSMSModel mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var mobjstk1 = ctxTFAT.SMSRules.Where(x => x.Type == mModel.Type).ToList();
                    ctxTFAT.SMSRules.RemoveRange(mobjstk1);
                    ctxTFAT.SaveChanges();
                    foreach (var eachvalue in mModel.mRightList)
                    {
                        if (eachvalue.PartySendEmail == true || eachvalue.PartySendSMS == true || eachvalue.UserSendEmail == true || eachvalue.UserSendMSG == true || eachvalue.UserSendSMS == true)
                        {
                            SMSRules mobj = new SMSRules();
                            mobj.CompCode = mcompcode;
                            mobj.Type = mModel.Type;
                            mobj.Code = eachvalue.PartyId;
                            mobj.PartyBranch = eachvalue.PartyBranchL;
                            mobj.PartyEmail = eachvalue.PartySendEmail;
                            mobj.PartyEmailTemp = eachvalue.SMSTemplate;
                            mobj.PartySMS = eachvalue.PartySendSMS;
                            mobj.PartySMSTemp = eachvalue.SMSTemplate;
                            mobj.UsersList = eachvalue.UsersL;
                            mobj.UserBranch = eachvalue.UserBranchL;
                            mobj.UserEmail = eachvalue.UserSendEmail;
                            mobj.UserSMS = eachvalue.UserSendSMS;
                            mobj.UserMsg = eachvalue.UserSendMSG;
                            mobj.PartyEmailAttachments = eachvalue.PartyPrintFormatL;
                            mobj.UserEmailAttachments = eachvalue.UserPrintFormatL;
                            mobj.CheckBlank = false;
                            mobj.LocationCode = 100001;
                            mobj.OtherContact = " ";
                            mobj.SendBroker = false;
                            mobj.SendSalesman = false;
                            mobj.ENTEREDBY = muserid;
                            mobj.LASTUPDATEDATE = DateTime.Now;
                            mobj.AUTHORISE = mauthorise;
                            mobj.AUTHIDS = muserid;

                            ctxTFAT.SMSRules.Add(mobj);
                            ctxTFAT.SaveChanges();
                        }

                    }
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, "Partywise Transaction Rules for SMS/E-mail", "", DateTime.Now, 0, mModel.Type, "Save Partywise Transaction Rules for SMS/E-mail", "D");

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