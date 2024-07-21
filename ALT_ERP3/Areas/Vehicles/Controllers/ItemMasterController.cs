using ALT_ERP3.Areas.Vehicles.Models;
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

namespace ALT_ERP3.Areas.Vehicles.Controllers
{
    public class ItemMasterController : BaseController
    {
        private static string mdocument = "";
        private static string mauthorise = "A00";
        private int mnewrecordkey = 0;

        public ActionResult GetHSNList(string term)
        {
            if (term != "")
            {
                var result = ctxTFAT.HSNMaster.Where(X => X.Name.Contains(term)).Select(m => new { Code = m.Code, Name = m.Name }).OrderBy(n => n.Name).ToList().Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.HSNMaster.Select(m => new { Code = m.Code, Name = m.Name }).OrderBy(n => n.Name).ToList().Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetProductGroupList(string term)
        {
            if (term != "")
            {
                var result = ctxTFAT.ItemGroups.Where(X => X.Name.Contains(term)).Select(m => new { Code = m.Code, Name = m.Name }).OrderBy(n => n.Name).ToList().Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.ItemGroups.Select(m => new { Code = m.Code, Name = m.Name }).OrderBy(n => n.Name).ToList().Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetPostingList(string term)
        {
            if (term != "")
            {
                var result = ctxTFAT.Master.Where(X => X.Name.Contains(term)).Select(m => new { Code = m.Code, Name = m.Name }).OrderBy(n => n.Name).ToList().Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.Master.Select(m => new { Code = m.Code, Name = m.Name }).OrderBy(n => n.Name).ToList().Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetGSTList(string term)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.TaxMaster.Where(x => x.VATGST == true).Select(x => new { x.Code, x.Name, x.Scope }).ToList().Take(10);
                var Modified = result.Select(x => new
                {
                    Code = x.Code,
                    Name = "[" + x.Name + "][" + x.Scope + "]"
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.TaxMaster.Where(x => x.VATGST == true && x.Name.Contains(term)).Select(m => new { m.Code, m.Name, m.Scope }).ToList().Take(10);
                var Modified = result.Select(x => new
                {
                    Code = x.Code,
                    Name = "[" + x.Name + "][" + x.Scope + "]"
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
        }
        private List<SelectListItem> PopulateBranchesOnly()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM TfatBranch where Category<>'Area' and Code<>'G00000' and Grp <>'G00000'  order by Recordkey ";
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

        // GET: Vehicles/ItemMaster
        public ActionResult Index(ItemMasterVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "I");
            mdocument = mModel.Document;
            mModel.BranchList = PopulateBranchesOnly();
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.ItemMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    mModel.Code = mList.Code;
                    mModel.Name = mList.Name;
                    mModel.BaseGr = mList.BaseGr;
                    mModel.BaseGrName = ctxTFAT.ItemGroups.Where(x => x.Code == mList.BaseGr).Select(x => x.Name).FirstOrDefault();
                    mModel.Rate = mList.Rate;
                    mModel.Posting = mList.Posting;
                    mModel.PostingName = ctxTFAT.Master.Where(x => x.Code == mList.Posting).Select(x => x.Name).FirstOrDefault();
                    mModel.GSTCode = mList.GSTCode;
                    mModel.GSTCodeName = "[" + ctxTFAT.TaxMaster.Where(x => x.Code == mList.GSTCode).Select(x => x.Name).FirstOrDefault() + "][" + ctxTFAT.TaxMaster.Where(x => x.Code == mList.GSTCode).Select(x => x.Scope).FirstOrDefault() + "]";
                    mModel.HSNCode = mList.HSNCode;
                    mModel.HSNCodeName = ctxTFAT.HSNMaster.Where(x => x.Code == mList.HSNCode).Select(x => x.Name).FirstOrDefault();
                    mModel.StockMaintain = mList.StockMaintain;
                    mModel.ExpiryDays = mList.ExpiryDays;
                    mModel.ExpiryKm = mList.ExpiryKm;
                    mModel.Active = mList.Active;
                    mModel.Narr = mList.Narr;
                    mModel.AppBranch = mList.AppBranch;
                }
            }
            else
            {
                mModel.Active = true;
                mModel.ExpiryDays = 0;
                mModel.ExpiryKm = 0;
                mModel.Rate = 0;
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(ItemMasterVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteUnitofMeasurementMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    ItemMaster mobj = new ItemMaster();
                    bool mAdd = true;
                    if (ctxTFAT.ItemMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.ItemMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                        mAdd = false;
                    }
                    if (mAdd)
                    {
                        if (ctxTFAT.ItemMaster.Where(x => x.Name.Trim().ToLower() == mModel.Name.Trim().ToLower()).FirstOrDefault() != null)
                        {
                            return Json(new { Message = "already Exist Item in Itemmaster...!" , Status = "Error", id = "UnitofMeasurementMaster" }, JsonRequestBehavior.AllowGet);
                        }


                        if (ctxTFAT.ItemMaster.ToList().Count() == 0)
                        {
                            mobj.Code = "000001";
                        }
                        else
                        {
                            var NewCode = ctxTFAT.ItemMaster.OrderByDescending(x => x.Code).Take(1).Select(x => x.Code).FirstOrDefault();
                            mobj.Code = (Convert.ToInt32(NewCode) + 1).ToString("D6");
                        }
                    }


                    mobj.Name = mModel.Name;
                    mobj.BaseGr = mModel.BaseGr;
                    mobj.Rate = mModel.Rate;
                    mobj.Posting = mModel.Posting;
                    mobj.GSTCode = mModel.GSTCode;
                    mobj.HSNCode = mModel.HSNCode;
                    mobj.StockMaintain = mModel.StockMaintain;
                    mobj.ExpiryDays = mModel.ExpiryDays;
                    mobj.ExpiryKm = mModel.ExpiryKm;
                    mobj.Active = mModel.Active;
                    mobj.Narr = mModel.Narr;
                    mobj.AppBranch = mModel.AppBranch;

                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mAdd == true)
                    {
                        ctxTFAT.ItemMaster.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.Code, DateTime.Now, 0, mobj.Code, "Save Item Master", "I");

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

        public ActionResult DeleteUnitofMeasurementMaster(ItemMasterVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Code Not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var Realtedata = ctxTFAT.RelateDataItem.Where(x => x.Item == mModel.Document).FirstOrDefault();
            if (Realtedata != null)
            {
                var GetLedger = ctxTFAT.RelateData.Where(x => x.ParentKey == Realtedata.Parentkey).FirstOrDefault();
                if (GetLedger != null)
                {
                    var Message = "Item Use IN: " + ctxTFAT.DocTypes.Where(x => x.Code == GetLedger.Type).Select(x => x.Name).FirstOrDefault() + " ,Branch Name :" + ctxTFAT.TfatBranch.Where(x => x.Code == GetLedger.Branch).Select(x => x.Name).FirstOrDefault() + " And Document No Is :" + GetLedger.Srl + " , Cant " + mModel.Mode;
                    return Json(new
                    {
                        Message = Message,
                        Status = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            var mList = ctxTFAT.ItemMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
            ctxTFAT.ItemMaster.Remove(mList);
            ctxTFAT.SaveChanges();
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mList.Code, DateTime.Now, 0, mList.Code, "Delete Item Master", "I");

            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData
    }
}