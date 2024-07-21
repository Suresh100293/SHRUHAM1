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
    public class BroadcastMasterController : BaseController
    {
        private static string mauthorise = "A00";

        public JsonResult GetFontFamily(string term)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Value = "Arial", Text = "Arial" });
            items.Add(new SelectListItem { Value = "Arial Black", Text = "Arial Black" });
            items.Add(new SelectListItem { Value = "Helvetica", Text = "Helvetica" });
            items.Add(new SelectListItem { Value = "Calibri", Text = "Calibri" });
            items.Add(new SelectListItem { Value = "Cambria", Text = "Cambria" });


            if (!String.IsNullOrEmpty(term))
            {
                items = items.Where(x => term.Contains(x.Text)).ToList();
            }

            var Modified = items.Select(x => new
            {
                Code = x.Value,
                Name = x.Text
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public string GetCode()
        {
            string Code = ctxTFAT.BroadCastMaster.OrderByDescending(x => x.RECORDKEY).Select(x => x.DocNo).FirstOrDefault();
            if (String.IsNullOrEmpty(Code))
            {
                Code = "100000";
            }
            else
            {
                Code = (Convert.ToInt32(Code) + 1).ToString();
            }

            return Code;
        }

        // GET: Logistics/BroadcastMaster
        public ActionResult Index(BroadCastVM mModel)
        {
            Session["TempAttach"] = null;
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0,"", "", "NA");

            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mlist = ctxTFAT.BroadCastMaster.Where(x => x.DocNo == mModel.Document).FirstOrDefault();
                if (mlist != null)
                {
                    mModel.DocNo = mlist.DocNo;
                    mModel.StartDate = mlist.StartDate.Value.ToShortDateString();
                    mModel.StartTime = mlist.StartTime;
                    mModel.EndDate = mlist.EndDate.Value.ToShortDateString();
                    mModel.EndTime = mlist.EndTime;
                    mModel.FontFamily = mlist.FontFamily;
                    mModel.Blink = mlist.Blink;
                    mModel.Scroll = mlist.Scroll;
                    mModel.Bold = mlist.Bold;
                    mModel.Color = mlist.Color;
                    mModel.Narr = mlist.Narr;
                    mModel.Active = mlist.Active;

                }
            }
            else
            {
                mModel.StartDate = DateTime.Now.ToShortDateString();
                mModel.StartTime = DateTime.Now.ToString("HH:mm");
                mModel.EndDate = DateTime.Now.AddDays(2).ToShortDateString();
                mModel.EndTime = DateTime.Now.AddDays(2).ToString("HH:mm");
                mModel.Active = true;
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(BroadCastVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        var Jsomn = DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Jsomn;
                    }

                    #region Validation BroadCast
                    DateTime StartDate = new DateTime();
                    DateTime EndDate = new DateTime();
                    if (mModel.Document != "99999")
                    {

                        var Date = mModel.StartDate.Split('/');
                        var Date1 = mModel.StartTime.Split(':');
                        StartDate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Date1[0]), Convert.ToInt32(Date1[1]), 00);
                        Date = mModel.EndDate.Split('/');
                        Date1 = mModel.EndTime.Split(':');
                        EndDate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Date1[0]), Convert.ToInt32(Date1[1]), 00);

                        if (!(StartDate < EndDate))
                        {
                            return Json(new
                            {
                                Message = "StartDate Always Greater Than EndDate..",
                                Status = "Error"
                            }, JsonRequestBehavior.AllowGet);
                        }



                        var CheckExist = ctxTFAT.BroadCastMaster.Where(x => x.DocNo != mModel.Document && x.DocNo != "999999" && x.StartDate <= StartDate && StartDate <= x.EndDate).ToList();
                        if (CheckExist.Count() > 0)
                        {
                            return Json(new
                            {
                                Message = "Cannot Create Duplicate BroadCast",
                                Status = "Error"
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    #endregion

                    BroadCastMaster mobj = new BroadCastMaster();
                    bool mAdd = true;
                    if (ctxTFAT.BroadCastMaster.Where(x => (x.DocNo == mModel.Document)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.BroadCastMaster.Where(x => (x.DocNo == mModel.Document)).FirstOrDefault();

                        mAdd = false;
                    }
                    if (mAdd)
                    {
                        mobj.EntryDate = DateTime.Now;
                    }

                    if (mModel.Document != "99999")
                    {
                        mobj.StartDate = StartDate;
                        mobj.EndDate = EndDate;
                    }


                    mobj.StartTime = mModel.StartTime;
                    mobj.EndTime = mModel.EndTime;
                    mobj.Blink = mModel.Blink;
                    mobj.Scroll = mModel.Scroll;
                    mobj.Color = mModel.Color;
                    mobj.FontFamily = mModel.FontFamily;
                    mobj.Bold = mModel.Bold;
                    mobj.Narr = mModel.Narr;
                    mobj.Active = mModel.Active;

                    //// iX9: default values for the fields not used @Form
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                    if (mAdd == true)
                    {
                        mobj.DocNo = GetCode();
                        ctxTFAT.BroadCastMaster.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    string mNewCode = "";
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.DocNo, DateTime.Now, 0, "", "Save BroadCast No :" + mobj.DocNo + "", "NA");

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

        public ActionResult DeleteStateMaster(BroadCastVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            string mactivestring = "";
            if (mModel.Document == "99999")
            {
                var mactive1 = ctxTFAT.BroadCastMaster.Where(x => x.DocNo == mModel.Document).FirstOrDefault();
                if (mactive1 != null)
                {
                    mactivestring = mactivestring + "\nNot Allowed To Delete Broadcast No : " + mactive1.DocNo;
                }
            }

            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "Active Account, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }



            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var mList = ctxTFAT.BroadCastMaster.Where(x => (x.DocNo == mModel.Document)).FirstOrDefault();
                    ctxTFAT.BroadCastMaster.Remove(mList);

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.Document, DateTime.Now, 0, "", "Delete BroadCast No :" + mList.DocNo + "", "NA");

                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new
                    {
                        Message = ex1.InnerException.Message,
                        Status = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new
                    {
                        Message = ex.InnerException.Message,
                        Status = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new
            {
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion SaveData


        public ActionResult GetBroadCast(BroadCastMaster broadCast)
        {

            string  Color = "", Narr = "", FontFamily = "";
            bool Blink = false, Scroll = false, Bold = false;
            broadCast = new BroadCastMaster();
            var Date = DateTime.Now.ToShortDateString().Split('/');
            var Date1 = DateTime.Now.ToString("HH:mm").Split(':');
            DateTime DocDate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Date1[0]), Convert.ToInt32(Date1[1]), 00);
            broadCast = ctxTFAT.BroadCastMaster.Where(x => x.StartDate <= DocDate && DocDate <= x.EndDate && x.Active == true).FirstOrDefault();
            if (broadCast == null)
            {
                broadCast = ctxTFAT.BroadCastMaster.Where(x => x.DocNo == "99999" && x.Active == true).FirstOrDefault();
            }
            if (broadCast == null)
            {
                broadCast = new BroadCastMaster();
                broadCast.Narr = "ALT AIR-0.3, Shruham Software";
                broadCast.Color = "White";
                broadCast.FontFamily = "Arial";
            }
            Color = broadCast.Color;
            Narr = broadCast.Narr;
            FontFamily = broadCast.FontFamily;
            Blink = broadCast.Blink;
            Scroll = broadCast.Scroll;
            Bold = broadCast.Bold;

            return Json(new { Status= "Success", Color = Color, Narr= Narr, FontFamily= FontFamily, Blink= Blink, Scroll= Scroll, Bold= Bold, id = "StateMaster" }, JsonRequestBehavior.AllowGet);
        }

    }
}