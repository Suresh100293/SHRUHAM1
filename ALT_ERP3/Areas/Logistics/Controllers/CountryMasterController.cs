using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class CountryMasterController : BaseController
    {
         
        //private IBusinessCommon mBuss = new BusinessCommon();
        private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mauthorise = "A00";
        private static string mdocument = "";

        #region GetLists
        public JsonResult AutoCompleteCurrName(string term)
        {
            return Json((from m in ctxTFAT.CurrencyMaster
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteLanguage(string term)
        {
            return Json((from m in ctxTFAT.Language
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        #endregion GetLists

        // GET: Logistics/CountryMaster
        public ActionResult Index(CountryMasterVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());

            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, mModel.Document==null?"":mModel.Document.ToUpper().Trim(), "", "COUNTRY");

            mdocument = mModel.Document;
            mModel.TfatCountry_Name = mModel.Document;

            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.TfatCountry.Where(x => (x.Name == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    var mCurrName = ctxTFAT.CurrencyMaster.Where(x => x.Code == mList.CurrName).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    var mLanguage = ctxTFAT.Language.Where(x => x.Code == mList.Language).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    mModel.TfatCountry_CurrName = mCurrName != null ? mCurrName.Code : 0;
                    mModel.CurrNameName = mCurrName != null ? mCurrName.Name : "";
                    mModel.TfatCountry_Language = mLanguage != null ? mLanguage.Code : 0;
                    mModel.LanguageName = mLanguage != null ? mLanguage.Name : "";
                    mModel.TfatCountry_Code = (Int32)mList.Code;
                    mModel.TfatCountry_Name = mList.Name;
                    mModel.TfatCountry_CountryCode = mList.CountryCode != null ? Convert.ToInt32(mList.CountryCode) : 0;
                    mModel.TfatCountry_CurCode = mList.CurCode;
                    mModel.TfatCountry_CurrDec = mList.CurrDec != null ? mList.CurrDec.Value : 0;
                    mModel.TfatCountry_CurDecName = mList.CurDecName;
                    mModel.TfatCountry_DialCode = mList.DialCode;
                    mModel.TfatCountry_CurrRate = mList.CurrRate != null ? mList.CurrRate.Value : 0;
                }
            }
            else
            {
                mModel.TfatCountry_Code = 0;
                mModel.TfatCountry_CountryCode = 0;
                mModel.TfatCountry_CurCode = "";
                mModel.TfatCountry_CurDecName = "";
                mModel.TfatCountry_CurrDec = 0;
                mModel.TfatCountry_CurrName = 0;
                mModel.TfatCountry_CurrRate = 0;
                mModel.TfatCountry_DialCode = "";
                mModel.TfatCountry_Language = 0;
                mModel.TfatCountry_LockCode = "";
                mModel.TfatCountry_Name = "";
                mModel.TfatCountry_ResourceOffset = 0;
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(CountryMasterVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        var mli = ctxTFAT.TfatCountry.Where(x => (x.Name.ToLower().Trim() == mModel.TfatCountry_Name.ToLower().Trim())).FirstOrDefault();

                        DeleteCountryMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mli.Code, DateTime.Now, 0, mModel.TfatCountry_Name.ToUpper().Trim(), "Delete Country Master", "COUNTRY");

                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    TfatCountry mobj = new TfatCountry();
                    

                    bool mAdd = true;
                    if (ctxTFAT.TfatCountry.Where(x => (x.Name.ToLower().Trim() == mModel.TfatCountry_Name.ToLower().Trim())).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.TfatCountry.Where(x => (x.Name.ToLower().Trim() == mModel.TfatCountry_Name.ToLower().Trim())).FirstOrDefault();
                        mAdd = false;
                    }
                    //mobj.Code = mModel.TfatCountry_Code;
                    
                    mobj.CountryCode = mModel.TfatCountry_CountryCode.ToString();
                    mobj.CurCode = mModel.TfatCountry_CurCode;
                    mobj.CurrName = mModel.TfatCountry_CurrName;
                    mobj.CurrDec = mModel.TfatCountry_CurrDec;
                    mobj.CurDecName = mModel.TfatCountry_CurDecName;
                    mobj.Language = mModel.TfatCountry_Language;
                    mobj.DialCode = mModel.TfatCountry_DialCode;
                    mobj.CurrRate = mModel.TfatCountry_CurrRate;
                    // iX9: default values for the fields not used @Form
                    mobj.LockCode = "";
                    mobj.ResourceOffset = 0;
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mAdd == true)
                    {
                        mobj.Name = mModel.TfatCountry_Name;
                        mobj.Code =Convert.ToInt64( GetNextCode());
                    }
                    if (mAdd == true)
                    {
                        ctxTFAT.TfatCountry.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    string mNewCode = "";
                    mNewCode = mobj.Name;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.Code, DateTime.Now, 0, mModel.TfatCountry_Name.ToUpper().Trim(), "Save Country Master", "COUNTRY");
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

        public string GetNextCode()
        {
            string Code = (from x in ctxTFAT.TfatCountry select x.Name).Max();
            string digits = new string(Code.Where(char.IsDigit).ToArray());
            int number;
            if (!int.TryParse(digits, out number)) //int.Parse would do the job since only digits are selected
            {
                number = 0;
            }
            return (++number).ToString("D35");
        }

        public ActionResult DeleteCountryMaster(CountryMasterVM mModel)
        {
            if (mModel.TfatCountry_Name == null || mModel.TfatCountry_Name == "")
            {
                return Json(new
                {
                    Message = "Name not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var mList = ctxTFAT.TfatCountry.Where(x => (x.Name == mModel.TfatCountry_Name)).FirstOrDefault();
            ctxTFAT.TfatCountry.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData

    }
}