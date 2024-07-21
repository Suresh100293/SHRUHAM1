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

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class DescriptionMasterController : BaseController
    {

        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;

        #region Functions

        //public string ConvertDDMMYYTOYYMMDD(string da)
        //{
        //    string abc = da.Substring(6, 4) + "/" + da.Substring(3, 2) + "/" + da.Substring(0, 2);
        //    return abc;
        //}

        #endregion


        // GET: Logistics/DescriptionMaster
        public ActionResult Index(DescriptionMasterVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());

            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");

            mdocument = mModel.Document;
            //mModel.TfatState_Name = mModel.Document;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var id = Convert.ToInt64(mModel.Document);
                var mList = ctxTFAT.DescriptionMaster.Where(x => (x.RECORDKEY == id)).FirstOrDefault();
                if (mList != null)
                {
                    mModel.Code = mList.Code;
                    mModel.Description = mList.Description;
                    mModel.Acitve = mList.Acitve;
                }
            }
            else
            {
                mModel.Code = "";
                mModel.Description = "";
                mModel.Acitve = true;
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(DescriptionMasterVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {

                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        var mli = ctxTFAT.DescriptionMaster.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
                        var MSG = DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mli.Code, DateTime.Now, 0, mli.Code, "Delete Description Master ", "DSR");
                        return MSG;
                    }
                    DescriptionMaster mobj = new DescriptionMaster();
                    bool mAdd = true;
                    var id = Convert.ToInt64(mModel.Document);
                    if (ctxTFAT.DescriptionMaster.Where(x => (x.RECORDKEY == id)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.DescriptionMaster.Where(x => (x.RECORDKEY == id)).FirstOrDefault();
                        mAdd = false;
                    }

                    mobj.Code = mModel.Code;
                    mobj.Description = mModel.Description;
                    mobj.Acitve = mModel.Acitve;

                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                    if (mAdd == true)
                    {
                        int NewCode1;
                        var NewCode = ctxTFAT.DescriptionMaster.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).Take(1).FirstOrDefault();
                        if (NewCode == null || NewCode == "")
                        {
                            NewCode1 = 100000;
                        }
                        else
                        {
                            NewCode1 = Convert.ToInt32(NewCode) + 1;
                        }
                        string FinalCode = NewCode1.ToString("D6");
                        mobj.Code = FinalCode;
                        ctxTFAT.DescriptionMaster.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = Convert.ToInt32(mobj.RECORDKEY);
                    string mNewCode = "";
                    mNewCode = mobj.Code;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.Code, DateTime.Now, 0, mobj.Code, "Save Description Master ", "DSR");

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

        public ActionResult DeleteStateMaster(DescriptionMasterVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Not Geting DesctionMaster Data",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var mList = ctxTFAT.DescriptionMaster.Where(x => (x.RECORDKEY.ToString() == mModel.Document)).FirstOrDefault();

            string mactivestring = "";
            var mactive1 = ctxTFAT.LRMaster.Where(x => (x.DescrType == mList.Code)).Select(x => x.LrNo).FirstOrDefault();
            if (mactive1 != 0)
            {
                mactivestring = mactivestring + "\nLrNo: " + mactive1;
            }
            var Condetail = ctxTFAT.ConDetail.Where(x => x.ContrType == "ItemType" && x.Services == mList.Code).FirstOrDefault();
            if (Condetail != null)
            {
                mactivestring = mactivestring + "\nContract Master Document No: " + Condetail.Code;
            }
            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "Active Account, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }


            ctxTFAT.DescriptionMaster.Remove(mList);
            ctxTFAT.SaveChanges();

            return Json(new
            {
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}