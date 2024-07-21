using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
////using ALT_ERP3.DynamicBusinessLayer;
////using ALT_ERP3.DynamicBusinessLayer.Repository;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    // GET: Accounts/DocumentFormats
    public class DocumentFormatsController : BaseController
    {
        //IBusinessCommon mIBuss = new BusinessCommon();
        //private IBusinessCommon mIBusi = new BusinessCommon();
        //private string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        //private string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        //private string muserid = System.Web.HttpContext.Current.Session["UserId"].ToString();
        //private string mperiod = System.Web.HttpContext.Current.Session["FPerd"].ToString();
        //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        private static string connstring = "";
        private static string mtype = "";


        // GET: Transactions/PackingSlip
        public ActionResult Index(DocFormatVM Model)
        {
            connstring = GetConnectionString();
            var Doc = Model.Document;

            Model.Type = Model.Document;

            var result = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => new
            {
                x.MainType,
                x.SubType,
                x.Name
            }).FirstOrDefault();
            Model.MainType = result.MainType;
            Model.SubType = result.SubType;
            Model.Header = result.Name;
            List<DocFormatVM> c = new List<DocFormatVM>();
            var DocFormatList = ctxTFAT.DocFormats.Where(x => x.Type == Model.Type).Select(x => x).ToList();
            int asc = 1;
            foreach (var a in DocFormatList)
            {

                c.Add(new DocFormatVM()
                {
                    FormatCode = a.FormatCode,
                    StoredProc = a.StoredProc,
                    SendEmail = a.SendEmail,
                    OutputDevice = a.OutputDevice,
                    ItemAttach = a.ItemAttach,
                    DocHandle = a.DocHandle,
                    AttachDocs = a.AttachDocs,
                    EmailTemplate = a.EmailTemplate,
                    tEmpID = asc,
                    tempIsDeleted = false
                });
                asc = asc + 1;
            }
            Model.DocFormatList = c;
            Session.Add("DocumentFormatSession", c);
            return View(Model);
        }

        #region GridOperations
        // following action is used when row is added to grid in add mode
        [HttpPost]
        public ActionResult AddToTable(DocFormatVM Model)
        {
            List<DocFormatVM> c = new List<DocFormatVM>();
            List<DocFormatVM> ca = new List<DocFormatVM>();

            if (Session["DocumentFormatSession"] != null)
            {
                c = (List<DocFormatVM>)Session["DocumentFormatSession"];
            }
            ca = (c == null) ? ca : c;
            int MaxtEmpID = (ca.Count == 0) ? 0 : ca.Select(x => x.tEmpID).Max();

            c.Add(new DocFormatVM()
            {
                FormatCode = Model.FormatCode,
                StoredProc = Model.StoredProc,
                SendEmail = Model.SendEmail,
                OutputDevice = Model.OutputDevice,
                ItemAttach = Model.ItemAttach,
                DocHandle = Model.DocHandle,
                AttachDocs = Model.AttachDocs,
                EmailTemplate = Model.EmailTemplate,
                tEmpID = MaxtEmpID + 1,
                tempIsDeleted = false
            });
            Session.Add("DocumentFormatSession", c);
            Model.DocFormatList = c;

            var html = ViewHelper.RenderPartialView(this, "AddFormats", new DocFormatVM() { DocFormatList = Model.DocFormatList, Mode = "Add" });
            return Json(new
            {
                DocFormatList = Model.DocFormatList,
                Html = html,

                Mode = "Add"
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFormatforUpdate(DocFormatVM Model)
        {
            var result = (List<DocFormatVM>)Session["DocumentFormatSession"];
            var result1 = result.Where(x => x.tEmpID == Model.tEmpID);
            foreach (var item in result1)
            {
                Model.tEmpID = item.tEmpID;

                Model.FormatCode = item.FormatCode;
                Model.StoredProc = item.StoredProc;
                Model.SendEmail = item.SendEmail;
                Model.OutputDevice = item.OutputDevice;
                Model.ItemAttach = item.ItemAttach;
                Model.DocHandle = item.DocHandle;
                Model.AttachDocs = item.AttachDocs;
                Model.EmailTemplate = item.EmailTemplate;
            }
            return Json(new
            {
                Html = this.RenderPartialView("UpdateFormat", Model)
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddToTableEdit(DocFormatVM Model)
        {
            var result = (List<DocFormatVM>)Session["DocumentFormatSession"];
            foreach (var item in result.Where(x => x.tEmpID == Model.tEmpID))
            {
                item.FormatCode = Model.FormatCode;
                item.StoredProc = Model.StoredProc;
                item.SendEmail = Model.SendEmail;
                item.OutputDevice = Model.OutputDevice;
                item.ItemAttach = Model.ItemAttach;
                item.DocHandle = Model.DocHandle;
                item.AttachDocs = Model.AttachDocs;
                item.EmailTemplate = Model.EmailTemplate;
            }
            Session.Add("DocumentFormatSession", result);
            var html = ViewHelper.RenderPartialView(this, "AddFormats", new DocFormatVM() { DocFormatList = result, Mode = "Add" });
            return Json(new
            {
                DocFormatList = result,
                Html = html,
                Mode = "Add"
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DeleteTableRow(DocFormatVM Model)
        {
            var result = (List<DocFormatVM>)Session["DocumentFormatSession"];
            var result2 = result.Where(x => x.tEmpID != Model.tEmpID).ToList();
            Session.Add("DocumentFormatSession", result2);
            var html = ViewHelper.RenderPartialView(this, "AddFormats", new DocFormatVM() { DocFormatList = result2 });
            return Json(new { DocFormatList = result2, Html = html }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        public ActionResult SaveAddons(DocFormatVM Model)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var mactive1 = ctxTFAT.DocFormats.Where(x => (x.Type == Model.Type)).Select(x => x).ToList();
                    ctxTFAT.DocFormats.RemoveRange(mactive1);

                    var result = (List<DocFormatVM>)Session["DocumentFormatSession"];
                    if (result != null && result.Count > 0)
                    {
                        int asc = 1;
                        foreach (var i in result)
                        {
                            DocFormats c = new DocFormats();
                            c.AttachDocs = i.AttachDocs;
                            c.DocHandle = i.DocHandle;
                            c.FormatCode = i.FormatCode;
                            c.ItemAttach = i.ItemAttach;
                            c.OutputDevice = i.OutputDevice;
                            c.Selected = i.Selected;
                            c.SendEmail = i.SendEmail;
                            c.Sno = asc;
                            c.StoredProc = i.StoredProc;
                            c.Type = Model.Type;
                            c.EmailTemplate = i.EmailTemplate == null ? "" : i.EmailTemplate;
                            c.ENTEREDBY = muserid;
                            c.LASTUPDATEDATE = DateTime.Now;
                            c.AUTHORISE = "A00";
                            c.AUTHIDS = muserid;
                            ctxTFAT.DocFormats.Add(c);
                            asc = asc + 1;
                        }
                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    Session["DocumentFormatSession"] = null;
                }

                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    return Json(new
                    {
                        Message = ex1.InnerException.InnerException.Message,
                        Status = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    return Json(new
                    {
                        Message = ex.InnerException.InnerException.Message,
                        Status = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new
            {
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }
    }
}