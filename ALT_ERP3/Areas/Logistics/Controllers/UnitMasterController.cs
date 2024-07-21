using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using EntitiModel;
using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class UnitMasterController : BaseController
    {
        ////private IBusinessCommon mBuss = new BusinessCommon();
        private static string mdocument = "";
        private static string mauthorise = "A00";
        private int mnewrecordkey = 0;


        // GET: Logistics/UnitMaster
        public ActionResult Index(UnitofMeasurementMasterVM mModel)
        {

            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());

            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "UNM");

            mdocument = mModel.Document;
            mModel.UnitMaster_Code = mModel.Document;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.UnitMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    mModel.UnitMaster_Code = mList.Code;
                    mModel.UnitMaster_Name = mList.Name;
                    mModel.UnitMaster_NoOfDecimal = (byte)(mList.NoOfDecimal != null ? mList.NoOfDecimal.Value : 0);
                    mModel.UnitMaster_Hide = mList.Hide;
                }
            }
            else
            {
                mModel.UnitMaster_Code = "";
                mModel.UnitMaster_Factor1 = 0;
                mModel.UnitMaster_Factor2 = 0;
                mModel.UnitMaster_Hide = false;
                mModel.UnitMaster_Lvl = 0;
                mModel.UnitMaster_Name = "";
                mModel.UnitMaster_NoOfDecimal = 0;
                mModel.UnitMaster_Operator1 = "";
                mModel.UnitMaster_Operator2 = "";
                mModel.UnitMaster_Type = false;
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(UnitofMeasurementMasterVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        var Msg = DeleteUnitofMeasurementMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.UnitMaster_Code, DateTime.Now, 0, mModel.UnitMaster_Code, "Delete Unit Master", "UNM");
                        return Msg;
                    }
                    UnitMaster mobj = new UnitMaster();
                    bool mAdd = true;
                    if (ctxTFAT.UnitMaster.Where(x => (x.Code == mModel.UnitMaster_Code)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.UnitMaster.Where(x => (x.Code == mModel.UnitMaster_Code)).FirstOrDefault();
                        mAdd = false;
                    }
                    mobj.Code = mModel.UnitMaster_Code;
                    mobj.Name = mModel.UnitMaster_Name;
                    mobj.NoOfDecimal = mModel.UnitMaster_NoOfDecimal;
                    mobj.Hide = mModel.UnitMaster_Hide;
                    // iX9: default values for the fields not used @Form
                    mobj.Factor1 = 0;
                    mobj.Factor2 = 0;
                    mobj.Lvl = 0;
                    mobj.Operator1 = "";
                    mobj.Operator2 = "";
                    mobj.Type = false;
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mAdd == true)
                    {
                        ctxTFAT.UnitMaster.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.Code, DateTime.Now, 0, mobj.Code, "Save Unit Master", "UNM");

                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "UnitofMeasurementMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "UnitofMeasurementMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "UnitofMeasurementMaster" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "UnitofMeasurementMaster" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteUnitofMeasurementMaster(UnitofMeasurementMasterVM mModel)
        {
            if (mModel.UnitMaster_Code == null || mModel.UnitMaster_Code == "")
            {
                return Json(new
                {
                    Message = "Code Not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            if (mModel.UnitMaster_Code == "KGS")
            {
                return Json(new
                {
                    Message = "Cannot Delete This..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            string mactivestring = "";
            var mactive1 = ctxTFAT.Stock.Where(x => (x.Unit == mModel.UnitMaster_Code)).Select(x => x.TableKey).FirstOrDefault();
            if (mactive1 != null)
            {
                mactivestring = mactivestring + "\nStock: " + mactive1;
            }
            //var mactive2 = ctxTFAT.ItemMaster.Where(x => (x.Unit == mModel.UnitMaster_Code || x.UnitM == mModel.UnitMaster_Code || x.UnitP == mModel.UnitMaster_Code || x.UnitS == mModel.UnitMaster_Code)).Select(x => x.Name).FirstOrDefault();
            //if (mactive2 != null)
            //{
            //    mactivestring = mactivestring + "\nItemMaster: " + mactive2;
            //}
            var mactive3 = ctxTFAT.LRMaster.Where(x => x.UnitCode == mModel.UnitMaster_Code).Select(x => x.LrNo).FirstOrDefault();
            if (mactive3 != 0)
            {
                mactivestring = mactivestring + "\nLrNo: " + mactive3;
            }


            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "Active Account, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }





            var mList = ctxTFAT.UnitMaster.Where(x => (x.Code == mModel.UnitMaster_Code)).FirstOrDefault();
            ctxTFAT.UnitMaster.Remove(mList);

            ctxTFAT.SaveChanges();




            return Json(new
            {
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}