using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class AccountingPeriodController : BaseController
    {
        private static string mauthorise = "A00";

        public string GetNextCode(AccountPeriodVM mModel)
        {

            var d1 = mModel.StartDate;
            var d2 = mModel.LastDate;
            var m1 = d1.Substring(3, 2);
            var y1 = d1.Substring(8, 2);
            var m2 = d2.Substring(3, 2);
            var y2 = d2.Substring(8, 2);
            return y1 + m1 + y2 + m2;
        }
        private List<SelectListItem> PopulateUsersOnly()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "";
                query = "select trim(Code) as Code,Name from TfatPass order by Name ";
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
        // GET: Logistics/AccountingPeriod
        public ActionResult Index(AccountPeriodVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.TfatPerd.Where(x => (x.PerdCode == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    mModel.LastDate = mList.LastDate.Value.ToShortDateString();
                    mModel.StartDate = mList.StartDate.ToShortDateString();
                    mModel.Locked = mList.Locked;
                    mModel.User = mList.LockUsers;
                }
            }
            else
            {
                mModel.CompCode = mcompcode;
            }
            mModel.Users = PopulateUsersOnly();
            return View(mModel);
        }



        public ActionResult SaveData(AccountPeriodVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    if (mModel.Locked)
                    {
                        var CountNoofPerd = ctxTFAT.TfatPerd.Where(x=>x.Locked==false && x.PerdCode != mModel.Document).ToList().Count();
                        if (CountNoofPerd == 0)
                        {
                            return Json(new { Message = "Minimum Single Accounting Period Required...!", Status = "Error", id = "AssignTasks" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteAccountPerios(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.Document, DateTime.Now, 0, "", "Delete Period", "NA");
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    TfatPerd mobj = new TfatPerd();
                    bool mAdd = true;
                    if (ctxTFAT.TfatPerd.Where(x => (x.PerdCode == mModel.Document)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.TfatPerd.Where(x => (x.PerdCode == mModel.Document)).FirstOrDefault();
                        mAdd = false;
                    }

                    mobj.Code = mcompcode;
                    mobj.DatabaseName = "TRNX_ERPiX9_Blank";
                    mobj.DbOffLine = false;
                    mobj.LastDate = ConvertDDMMYYTOYYMMDD(mModel.LastDate);
                    mobj.Locked = mModel.Locked;
                    mobj.MasUpdate = false;
                    mobj.NewUpdate = false;
                    mobj.StartDate = ConvertDDMMYYTOYYMMDD(mModel.StartDate);
                    mobj.LockUsers = mModel.User;

                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mAdd == true)
                    {
                        mobj.PerdCode = GetNextCode(mModel);
                    }
                    if (mAdd == true)
                    {
                        ctxTFAT.TfatPerd.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    int mNewCode = 0;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.PerdCode, DateTime.Now, 0, "", "Save Period", "NA");

                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "AssignTasks" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "AssignTasks" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "AssignTasks" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "AssignTasks" }, JsonRequestBehavior.AllowGet);
        }

        private void DeleteAccountPerios(AccountPeriodVM mModel)
        {
            throw new NotImplementedException();
        }






    }
}