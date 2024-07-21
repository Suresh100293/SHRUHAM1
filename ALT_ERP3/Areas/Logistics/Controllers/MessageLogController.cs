using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class MessageLogController : BaseController
    {
         
        ////private IBusinessCommon mBuss = new BusinessCommon();
        private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mauthorise = "A00";
        private static int mdocument = 0;

        #region GetLists
        public JsonResult AutoCompleteFromIDs(string term)
        {
            if (String.IsNullOrEmpty(term))
            {
                return Json((from m in ctxTFAT.TfatPass
                             select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json((from m in ctxTFAT.TfatPass
                             where m.Name.ToLower().Contains(term.ToLower())
                             select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult AutoCompleteCode(string term)
        {
            if (String.IsNullOrEmpty(term))
            {
                return Json((from m in ctxTFAT.TfatPass
                             select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json((from m in ctxTFAT.TfatPass
                             where m.Name.ToLower().Contains(term.ToLower())
                             select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
            }
            
        }

        public JsonResult AutoCompleteCategory(string term)
        {
            if (String.IsNullOrEmpty(term))
            {
                return Json((from m in ctxTFAT.MessageCategory
                             select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json((from m in ctxTFAT.MessageCategory
                             where m.Name.ToLower().Contains(term.ToLower())
                             select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
            }
            
        }

        #endregion GetLists


        // GET: Logistics/MessageLog
        public ActionResult Index(MessageLogVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;
            mModel.MessageLog_mDate = DateTime.Now;
            mModel.MessageLog_mTime = DateTime.Now;
            mModel.MessageLog_RECORDKEY = mModel.Document;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.MessageLog.Where(x => (x.RECORDKEY == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    var mFromIDs = ctxTFAT.TfatPass.Where(x => x.Code == mList.FromIDs).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mCode = ctxTFAT.TfatPass.Where(x => x.Code == mList.Code).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mCategory = ctxTFAT.MessageCategory.Where(x => x.Code == mList.Category).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    mModel.MessageLog_FromIDs = mFromIDs != null ? mFromIDs.Code.ToString() : "";
                    mModel.FromIDsName = mFromIDs != null ? mFromIDs.Name : "";
                    mModel.MessageLog_Code = mCode != null ? mCode.Code.ToString() : "";
                    mModel.CodeName = mCode != null ? mCode.Name : "";
                    mModel.MessageLog_Category = mCategory != null ? mCategory.Code.Value : 0;
                    mModel.CategoryName = mCategory != null ? mCategory.Name : "";
                    mModel.MessageLog_MessageID = mList.MessageID != null ? mList.MessageID.Value : 0;
                    mModel.MessageLog_mDate = mList.mDate != null ? mList.mDate.Value : DateTime.Now;
                    mModel.MessageLog_mTime = mList.mTime != null ? mList.mTime.Value : DateTime.Now;
                    mModel.MessageLog_MessageRead = mList.MessageRead;
                    mModel.MessageLog_MessageDelete = mList.MessageDelete;
                    mModel.MessageLog_ReplyRequest = mList.ReplyRequest;
                    mModel.MessageLog_SendNow = mList.SendNow;
                    mModel.MessageLog_Message = mList.Message;
                }
            }
            else
            {
                var mCategory = ctxTFAT.MessageCategory.Where(x => x.Code == 100001).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                mModel.CategoryName = mCategory != null ? mCategory.Name : "";
                mModel.MessageLog_Category = mCategory != null ? mCategory.Code.Value : 0;
                mModel.MessageLog_Code = "";
                var mFromIDs = ctxTFAT.TfatPass.Where(x => x.Code == muserid).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                mModel.FromIDsName = mFromIDs != null ? mFromIDs.Name : "";
                mModel.MessageLog_mDate = System.DateTime.Now;
                mModel.MessageLog_Message = "";
                mModel.MessageLog_MessageDelete = false;
                mModel.MessageLog_MessageID = 0;
                mModel.MessageLog_MessageRead = false;
                mModel.MessageLog_mTime = System.DateTime.Now;
                mModel.MessageLog_Prefix = "";
                mModel.MessageLog_ReplyRequest = false;
                mModel.MessageLog_SendNow = false;
                mModel.MessageLog_Srl = "";
                mModel.MessageLog_TableKey = "";
                mModel.MessageLog_Type = "";
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(MessageLogVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                if (mModel.Mode == "Delete")
                {
                   var Json= DeleteMessageLog(mModel);
                    transaction.Commit();
                    transaction.Dispose();
                    return Json;
                }
                try
                {
                    MessageLog mobj = new MessageLog();
                    bool mAdd = true;
                    if (ctxTFAT.MessageLog.Where(x => (x.RECORDKEY == mdocument)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.MessageLog.Where(x => (x.RECORDKEY == mdocument)).FirstOrDefault();
                        mAdd = false;
                    }
                    mobj.MessageID = mModel.MessageLog_MessageID;
                    mobj.mDate = ConvertDDMMYYTOYYMMDD(mModel.MessageLog_mDateVM);
                    mobj.mTime = ConvertDDMMYYTOYYMMDD(mModel.MessageLog_mTimeVM);
                    mobj.FromIDs = muserid;
                    mobj.Code = mModel.MessageLog_Code;
                    mobj.Category = mModel.MessageLog_Category;
                    mobj.MessageRead = mModel.MessageLog_MessageRead;
                    mobj.MessageDelete = mModel.MessageLog_MessageDelete;
                    mobj.ReplyRequest = mModel.MessageLog_ReplyRequest;
                    mobj.SendNow = mModel.MessageLog_SendNow;
                    mobj.Message = mModel.MessageLog_Message;
                    // iX9: default values for the fields not used @Form
                    mobj.Prefix = "";
                    mobj.Srl = "";
                    mobj.TableKey = "";
                    mobj.Type = "";
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mAdd == true)
                    {
                        //mobj.MessageID = GetNextCode();
                        ctxTFAT.MessageLog.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    int mNewCode = 0;
                    mNewCode = mobj.RECORDKEY;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mNewCode, DateTime.Now, 0, "", "Save Messenger", "NA");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "MessageLog" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "MessageLog" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "MessageLog" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "MessageLog" }, JsonRequestBehavior.AllowGet);
        }

        public int GetNextCode()
        {
            var nextcode = (from x in ctxTFAT.MessageLog select (int?)x.RECORDKEY).Max() ?? 0;
            return (++nextcode);
        }


        public ActionResult DeleteMessageLog(MessageLogVM mModel)
        {
            // iX9: Check for Active Master Task
            string mactivestring = "";
            var mList = ctxTFAT.MessageLog.Where(x => (x.RECORDKEY == mdocument)).FirstOrDefault();
            ctxTFAT.MessageLog.Remove(mList);
            ctxTFAT.SaveChanges();
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mdocument.ToString(), DateTime.Now, 0, "", "Delete Messenger", "NA");

            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }


        #endregion SaveData
    }
}