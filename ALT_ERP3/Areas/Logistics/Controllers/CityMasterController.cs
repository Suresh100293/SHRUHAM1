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
    public class CityMasterController : BaseController
    {
        private string mauthorise = "A00";

        #region GetLists
        public JsonResult AutoCompleteState(string term)
        {
            if (term != null && term != "")
            {
                return Json((from m in ctxTFAT.TfatState
                             where m.Name.ToLower().Contains(term.ToLower()) || m.Name.ToLower().Contains(term.ToLower())
                             select new { Name = "[" + m.Name + "] " + m.Name, Code = m.Code.ToString() }).ToArray(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json((from m in ctxTFAT.TfatState
                             select new { Name = "[" + m.Name + "] " + m.Name, Code = m.Code.ToString() }).Take(10).ToArray(), JsonRequestBehavior.AllowGet);
            }
        }


        public string GetCode()
        {
            string Code = "";
            var LastCode = ctxTFAT.TfatCity.OrderByDescending(x => x.Code).FirstOrDefault();
            Code = (Convert.ToInt32(LastCode.Code) + 1).ToString();
            return Code;
        }

        #endregion GetLists

        // GET: Logistics/CityMaster
        public ActionResult Index(CityMasterVM Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, "City Master", "", DateTime.Now, 0, Model.Document==null?"": Model.Document.ToUpper().Trim(), "", "CITY");
            Model.TfatCity_Name = Model.Document;
            if ((Model.Mode == "Edit") || (Model.Mode == "View") || (Model.Mode == "Delete"))
            {
                var mList = ctxTFAT.TfatCity.Where(x => (x.Name == Model.Document)).FirstOrDefault();
                if (mList != null)
                {
                    var mState = ctxTFAT.TfatState.Where(x => x.Code.ToString() == mList.State).Select(x => new { Name = x.Name, Code = x.Code.ToString() }).FirstOrDefault();
                    Model.TfatCity_State = mState != null ? mState.Code.ToString() : "";
                    Model.StateName = mState != null ? mState.Name : "";
                    Model.TfatCity_Name = mList.Name;
                    Model.TfatCity_Code =  Convert.ToInt32 (mList.Code);
                    Model.AUTHORISE = mList.AUTHORISE ?? "A00";
                    mauthorise = Model.AUTHORISE;
                }
            }
            else
            {
                Model.Mode = "Add";
                Model.AUTHORISE = "A00";
                Model.TfatCity_Code = 0;
                Model.TfatCity_Name = "";
                Model.TfatCity_State = "";
            }
            return View(Model);
        }

        public ActionResult MoveNextPrevious(string Mode, string mdocument)
        {
            string mvar;
            if (Mode == "N")
            {
                mvar = Fieldoftable("TfatCity", "Top 1 Name", "Name>'" + mdocument + "' order by Name", "T") ?? "";
            }
            else
            {
                mvar = Fieldoftable("TfatCity", "Top 1 Name", "Name<'" + mdocument + "' order by Name desc", "T") ?? "";
            }
            return Json(new { Status = "Success", data = mvar }, JsonRequestBehavior.AllowGet);
        }

        #region SaveData
        public ActionResult SaveData(CityMasterVM Model)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (Model.Mode == "Delete")
                    {
                        var mli = ctxTFAT.TfatCity.Where(x => (x.Name.ToLower().Trim() == Model.TfatCity_Name.ToLower().Trim())).FirstOrDefault();

                        DeleteData(Model);
                        transaction.Commit();
                        transaction.Dispose();
                        UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "     " + mperiod.Substring(0, 2) + mli.Code, DateTime.Now, 0, Model.TfatCity_Name.ToUpper().Trim(), "Delete  City Master", "CITY");

                        return Json(new { Status = "Success", Message = "Data is Deleted.", NewSrl = "" }, JsonRequestBehavior.AllowGet);
                    }
                    TfatCity mobj = new TfatCity();
                    bool mAdd = true;
                    if (ctxTFAT.TfatCity.Where(x => (x.Name.ToLower().Trim() == Model.TfatCity_Name.ToLower().Trim())).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.TfatCity.Where(x => (x.Name.ToLower().Trim() == Model.TfatCity_Name.ToLower().Trim())).FirstOrDefault();
                        mAdd = false;
                    }
                    
                    //mobj.Code = Model.TfatCity_Code.ToString().Trim();
                    mobj.State = Model.TfatCity_State ?? "";
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = DateTime.Now;
                    mobj.AUTHORISE = "A00";
                    if (mAdd == true)
                    {
                        mobj.Name = Model.TfatCity_Name;
                        mobj.Code = GetCode();
                        ctxTFAT.TfatCity.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    string mNewCode = mobj.Name.ToString();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "     " + mperiod.Substring(0, 2) + mNewCode, DateTime.Now, 0, mobj.Name.ToUpper().Trim(), "Save City Master", "CITY");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    return Json(new { Message = ex1.EntityValidationErrors.Select(x => x.ValidationErrors.Select(xa => xa.ErrorMessage).FirstOrDefault()), Status = "Error" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    return Json(new { ex.InnerException.InnerException.Message, Status = "Error", id = "CityMaster" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", NewSrl = Model.TfatCity_Name, id = "CityMaster" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteData(CityMasterVM Model)
        {
            if (Model.TfatCity_Name == null || Model.TfatCity_Name == "")
            {
                return Json(new
                {
                    Message = "Name not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            DeUpdate(Model);
            return Json(new { Status = "Success", Message = "The Document is Deleted." }, JsonRequestBehavior.AllowGet);
        }

        private void DeUpdate(CityMasterVM Model)
        {
            var mList = ctxTFAT.TfatCity.Where(x => (x.Name == Model.TfatCity_Name)).ToList();
            if (mList != null)
            {
                ctxTFAT.TfatCity.RemoveRange(mList);
            }
            ctxTFAT.SaveChanges();
        }
        #endregion SaveData
    }
}