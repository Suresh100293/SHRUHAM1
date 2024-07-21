using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class NotificationController : BaseController
    {
        private static string mauthorise = "A00";

        #region GetLists
        public JsonResult AutoCompleteToIDs(string term)
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
        #endregion

        // GET: Logistics/Notification
        public ActionResult Index(NotificationVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.tfatNotification.Where(x => (x.RECORDKEY == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    mModel.Srl = mList.Srl;
                    mModel.CreateOn = mList.CreateOn;
                    mModel.Clear = mList.Clear;
                    mModel.Priority = mList.Priority;
                    mModel.DocType = mList.DocType;
                    mModel.DocNo = mList.DocNo;
                    mModel.Parentkey = mList.Parentkey;
                    mModel.Tablekey = mList.Tablekey;
                    mModel.Describe = mList.Describe;
                    mModel.Msg = mList.Msg.Replace("<br>","\n");
                    mModel.HtmlString = mList.HtmlString;
                    mModel.ToUser = mList.ToUser;
                    mModel.ToUserName =ctxTFAT.TfatPass.Where(x=>x.Code==mList.ToUser).Select(x=>x.Name).FirstOrDefault();
                    mModel.ENTEREDBY = mList.ENTEREDBY;
                    mModel.AUTHIDS = ctxTFAT.TfatPass.Where(x=>x.Code==mList.ENTEREDBY).Select(x=>x.Name).FirstOrDefault();
                }
            }
            else
            {

            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(NotificationVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                if (mModel.Mode == "Delete")
                {
                    var Json = DeleteMessageLog(mModel);
                    transaction.Commit();
                    transaction.Dispose();
                    return Json;
                }
                try
                {
                    tfatNotification mobj = new tfatNotification();
                    bool mAdd = true;
                    if (ctxTFAT.tfatNotification.Where(x => (x.RECORDKEY == mModel.Document)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.tfatNotification.Where(x => x.RECORDKEY == mModel.Document).FirstOrDefault();
                        mAdd = false;
                    }

                    mobj.Clear = mModel.Clear;
                    mobj.Priority = mModel.Priority;

                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mAdd == true)
                    {
                        //mobj.MessageID = GetNextCode();
                        ctxTFAT.tfatNotification.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
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


        public ActionResult DeleteMessageLog(NotificationVM mModel)
        {
            // iX9: Check for Active Master Task
            string mactivestring = "";
            var mList = ctxTFAT.tfatNotification.Where(x => (x.RECORDKEY == mModel.Document)).FirstOrDefault();
            mactivestring = mList.Srl;
            ctxTFAT.tfatNotification.Remove(mList);
            ctxTFAT.SaveChanges();
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mactivestring.ToString(), DateTime.Now, 0, "", "Delete Notification", "NA");

            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }


        #endregion SaveData

       

    }
}