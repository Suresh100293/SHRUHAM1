using ALT_ERP3.Areas.Vehicles.Models;
using ALT_ERP3.Controllers;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Vehicles.Controllers
{
    public class ItemGroupController : BaseController
    {
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;

        // GET: Vehicles/ItemGroup
        public ActionResult Index(ItemGroupVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "IG");

            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.ItemGroups.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    mModel.Name = mList.Name;
                    mModel.Hide = mList.Hide;
                }
            }
            else
            {
            }
            return View(mModel);
        }
        public ActionResult SaveData(ItemGroupVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        string MSG = DeleteStateMaster(mModel);
                        
                        if (MSG == "Success")
                        {
                            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.Document, DateTime.Now, 0, mModel.Document, "Delete Item Group", "IG");
                            transaction.Commit();
                            transaction.Dispose();
                            ctxTFAT.SaveChanges();
                            return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { Status = "Error", Message = MSG }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    ItemGroups tfatBranch = new ItemGroups();
                    bool mAdd = true;
                    if (ctxTFAT.ItemGroups.Where(x => x.Code == mModel.Document).FirstOrDefault() != null)
                    {
                        tfatBranch = ctxTFAT.ItemGroups.Where(x => x.Code == mModel.Document).FirstOrDefault();
                        mAdd = false;
                    }
                    if (mAdd)
                    {
                        if (ctxTFAT.ItemGroups.ToList().Count() == 0)
                        {
                            tfatBranch.Code = "000001";
                        }
                        else
                        {
                            var NewCode = ctxTFAT.ItemGroups.OrderByDescending(x => x.Code).Take(1).Select(x => x.Code).FirstOrDefault();
                            tfatBranch.Code = (Convert.ToInt32(NewCode) + 1).ToString("D6");
                        }
                    }

                    tfatBranch.Name = mModel.Name;
                    tfatBranch.Hide = mModel.Hide;

                    tfatBranch.AUTHIDS = muserid;
                    tfatBranch.AUTHORISE = mauthorise;
                    tfatBranch.ENTEREDBY = muserid;
                    tfatBranch.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                    if (mAdd == false)
                    {
                        ctxTFAT.Entry(tfatBranch).State = EntityState.Modified;
                    }
                    else
                    {
                        ctxTFAT.ItemGroups.Add(tfatBranch);
                    }
                    ctxTFAT.SaveChanges();
                    string mNewCode = "";
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + tfatBranch.Code, DateTime.Now, 0, tfatBranch.Code, "Save Item Group", "IG");

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

        public string DeleteStateMaster(ItemGroupVM mModel)
        {
            string Message = "";
            if (mModel.Document == null)
            {
                return Message = "Code not Entered..";
            }

            // iX9: Check for Active Master MasterGroups
            var Delete = true;
            string mactivestring = "";
            var mactive1 = ctxTFAT.ItemMaster.Where(x => (x.BaseGr == mModel.Document)).FirstOrDefault();
            if (mactive1 != null)
            {
                mactivestring = mactivestring + "\nName: " + mactive1.Name + " In Item Master.";
            }

            var mlis = ctxTFAT.RelateDataItem.Where(x => x.ProductGroup == mModel.Document).FirstOrDefault();
            if (mlis!=null)
            {
                mactivestring = mactivestring + "\nName: " + mactive1.Name + " In Relate Data Item Master.";
            }


            if (mactivestring != "")
            {
                return Message = "Active Item, Can't Delete..\nUsed with:\n" + mactivestring;
            }

            var mList = ctxTFAT.ItemGroups.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
            ctxTFAT.ItemGroups.Remove(mList);
            

            return Message = "Success";
        }
    }
}