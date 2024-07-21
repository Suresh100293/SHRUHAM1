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
    public class ShareSetupController : BaseController
    {
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;


        private List<SelectListItem> PopulatePrints(string Type)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT FormatCode as Code FROM DocFormats where type='" + Type + "'  order by Recordkey ";
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
                                Text = sdr["Code"].ToString(),
                                Value = sdr["Code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return items;
        }

        // GET: Logistics/ShareSetup
        public ActionResult Index(ShareSetupVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, "Setup", mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;

            var id = Convert.ToInt64(1);
            var mList = ctxTFAT.ShareSetup.Where(x => (x.RECORDKEY == id)).FirstOrDefault();
            if (mList != null)
            {
                mModel.LRExtra=mList.LRExtra;
                mModel.LRAttachReq = mList.LRAttachReq;
                mModel.FmExtra = mList.FmExtra;
                mModel.FMAttachReq = mList.FMAttachReq;
                mModel.BillExtra = mList.BillExtra;
                mModel.BillAttachReq = mList.BillAttachReq;
                mModel.Payment = mList.Payment;
                mModel.PaymentAttachReq = mList.PaymentAttachReq;
                mModel.LRFormat = mList.LRFormat;
                mModel.FMFormat = mList.FMFormat;
                mModel.BillFormat = mList.BillFormat;
                mModel.PaymentFormat = mList.PaymentFormat;
                mModel.CCEmail = mList.CCEmail;
            }
            mModel.LRPrintFormats = PopulatePrints("LR000");
            mModel.FMPrintFormats = PopulatePrints("FM000");
            mModel.BillPrintFormats = PopulatePrints("SLR00");
            mModel.PaymentPrintFormats = PopulatePrints("BPM00");


            return View(mModel);
        }
        public ActionResult SaveData(ShareSetupVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    ////iX9: Remove Existing Data for Delete Mode
                    //if (mModel.Mode == "Delete")
                    //{
                    //    DeleteStateMaster(mModel);
                    //    transaction.Commit();
                    //    transaction.Dispose();
                    //    return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    //}

                    ShareSetup mobj = new ShareSetup();
                    bool mAdd = true;
                    var id = Convert.ToInt64(1);
                    if (ctxTFAT.ShareSetup.Where(x => (x.RECORDKEY == id)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.ShareSetup.Where(x => (x.RECORDKEY == id)).FirstOrDefault();
                        mAdd = false;
                    }

                    mobj.LRExtra = mModel.LRExtra;
                    mobj.LRAttachReq = mModel.LRAttachReq;
                    mobj.FmExtra = mModel.FmExtra;
                    mobj.FMAttachReq = mModel.FMAttachReq;
                    mobj.BillExtra = mModel.BillExtra;
                    mobj.BillAttachReq = mModel.BillAttachReq;
                    mobj.Payment = mModel.Payment;
                    mobj.PaymentAttachReq = mModel.PaymentAttachReq;
                    mobj.LRFormat = mModel.LRFormat;
                    mobj.FMFormat = mModel.FMFormat;
                    mobj.BillFormat = mModel.BillFormat;
                    mobj.PaymentFormat = mModel.PaymentFormat;
                    mobj.CCEmail = mModel.CCEmail;

                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    mobj.AUTHORISE = mauthorise;
                    mobj.AUTHIDS = muserid;

                    if (mAdd == true)
                    {
                        ctxTFAT.ShareSetup.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, "Setup", mModel.Header, "", DateTime.Now, 0, "", "Save Share Setup", "NA");
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

    }
}