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
    public class BranchLocationController : BaseController
    {
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;

        // GET: Logistics/BranchLocation
        public ActionResult Index(BranchLocationVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;

            var result = (from mrightlink in ctxTFAT.TfatBranch.Where(x => x.Code != "G00000" && x.Grp != "G00000" && x.Category != "Area" && x.Status == true)
                          join xUserRights in ctxTFAT.TfatBranchLocation on mrightlink.Code equals xUserRights.Code into ljointresult
                          from finalresult in ljointresult.DefaultIfEmpty()
                          orderby (mrightlink.RECORDKEY)
                          select new BranchLocationVM
                          {
                              ID = mrightlink.Code,
                              Area = mrightlink.Name,
                              Location = finalresult != null ? finalresult.Location : "",
                              Title = finalresult != null ? finalresult.Title : "",
                          }).ToList();
            mModel.list = result;
            return View(mModel);
        }

        public ActionResult SaveData(BranchLocationVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.list!=null)
                    {
                        foreach (var item in mModel.list)
                        {
                            if (!(String.IsNullOrEmpty(item.Location) && String.IsNullOrEmpty(item.Title)))
                            {
                                TfatBranchLocation mobj = new TfatBranchLocation();
                                bool mAdd = true;
                                if (ctxTFAT.TfatBranchLocation.Where(x => x.Code.Trim() == item.ID.Trim()).FirstOrDefault() != null)
                                {
                                    mobj = ctxTFAT.TfatBranchLocation.Where(x => x.Code.Trim() == item.ID.Trim()).FirstOrDefault();
                                    mAdd = false;
                                }
                                mobj.Code = item.ID;
                                mobj.Location = item.Location;
                                mobj.Title = item.Title;
                                // iX9: Save default values to Std fields
                                mobj.AUTHIDS = muserid;
                                mobj.AUTHORISE = mauthorise;
                                mobj.LASTUPDATEDATE = System.DateTime.Now;
                                if (mAdd == true)
                                {
                                    mobj.ENTEREDBY = muserid;
                                    ctxTFAT.TfatBranchLocation.Add(mobj);
                                }
                                else
                                {
                                    ctxTFAT.Entry(mobj).State = EntityState.Modified;
                                }
                            }
                        }
                    }
                    
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "Save Branch Location", "NA");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "CountryMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "CountryMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "CountryMaster" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "CountryMaster" }, JsonRequestBehavior.AllowGet);
        }
    }
}