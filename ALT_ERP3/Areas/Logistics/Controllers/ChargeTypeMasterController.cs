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
    public class ChargeTypeMasterController : BaseController
    {

        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;

        // GET: Logistics/ServiceMaster
        public ActionResult Index(ChargeTypeMasterVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header,"", DateTime.Now, 0, mModel.Document, "", "CHRG");

            mdocument = mModel.Document;
            //mModel.TfatState_Name = mModel.Document;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.ChargeTypeMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    //var mCountry = ctxTFAT.TfatCountry.Where(x => x.Name == mList.Country).Select(x => new { Name = x.Name, Code = x.Name }).FirstOrDefault();
                    //mModel.TfatState_Country = mCountry != null ? mCountry.Code.ToString() : "";
                    //mModel.CountryName = mCountry != null ? mCountry.Name : "";
                    //mModel.TfatState_Name = mList.Name;
                    //mModel.TfatState_Code = mList.Code;
                    mModel.ChargeType = mList.ChargeType;
                    mModel.ShortName = mList.ShortName;
                    mModel.Code = mList.Code;
                    mModel.Acitve = mList.Acitve;
                }
            }
            else
            {
                //mModel.TfatState_Code = 0;
                //mModel.TfatState_Country = "";
                //mModel.TfatState_Name = "";
                //mModel.TfatState_StateCode = "";
                mModel.ChargeType = "";
                mModel.ShortName = "";
                mModel.Code = "";
                mModel.Acitve = true;
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(ChargeTypeMasterVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    bool Status = false;
                    if (mModel.AcitveorNot.ToString() == "True")
                    {
                        Status = true;
                    }
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        var MSG = DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.Document, DateTime.Now, 0, mModel.Document, "Delete Charge Type Master", "CHRG");
                        return MSG;
                    }
                    ChargeTypeMaster mobj = new ChargeTypeMaster();
                    bool mAdd = true;
                    if (ctxTFAT.ChargeTypeMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.ChargeTypeMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                        mAdd = false;
                    }

                    mobj.ChargeType = mModel.ChargeType;
                    mobj.ShortName = mModel.ShortName;
                    mobj.Acitve = Status;
                    //mobj.Country = mModel.TfatState_Country;
                    //// iX9: default values for the fields not used @Form
                    //mobj.StateCode = "";
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                    if (mAdd == true)
                    {
                        int NewCode1;
                        var NewCode = ctxTFAT.ChargeTypeMaster.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).Take(1).FirstOrDefault();
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
                        ctxTFAT.ChargeTypeMaster.Add(mobj);
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
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.Code, DateTime.Now, 0, mobj.Code, "Save Charge Type Master", "CHRG");

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

        public JsonResult DeleteStateMaster(ChargeTypeMasterVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Charge Type not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            if (mModel.Document == "100000")
            {
                return Json(new
                {
                    Message = "Cannot Delete This..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            string mactivestring = "";
            var mactive1 = ctxTFAT.LRMaster.Where(x => (x.ChgType == mModel.Document)).Select(x => x.LrNo).FirstOrDefault();
            if (mactive1 != 0)
            {
                mactivestring = mactivestring + "\nLrNo: " + mactive1;
            }

            var Condetail = ctxTFAT.ConDetail.Where(x => x.ContrType == "ChargeType" && x.Services == mModel.Document).FirstOrDefault();
            if (Condetail!=null)
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

            var mList = ctxTFAT.ChargeTypeMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
            ctxTFAT.ChargeTypeMaster.Remove(mList);

            ctxTFAT.SaveChanges();

            return Json(new
            {
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData

        //public string ConvertDDMMYYTOYYMMDD(string da)
        //{
        //    string abc = da.Substring(6, 4) + "/" + da.Substring(3, 2) + "/" + da.Substring(0, 2);
        //    return abc;
        //}
    }
}