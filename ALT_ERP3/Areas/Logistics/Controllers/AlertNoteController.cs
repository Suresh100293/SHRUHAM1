using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class AlertNoteController : BaseController
    {
        string attachmentPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentPath"].ToString()) == true ? "" : ConfigurationManager.AppSettings["AttachmentPath"].ToString();
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        private static string mauthorise = "A00";

        #region Common Function

        public JsonResult GetType(string term)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Value = "LR000", Text = "Lorry Receipt" });
            //items.Add(new SelectListItem { Value = "LC000", Text = "Lorry Challan" });
            items.Add(new SelectListItem { Value = "FM000", Text = "Freight Memo" });
            items.Add(new SelectListItem { Value = "FM000", Text = "Freight Memo Hire" });
            items.Add(new SelectListItem { Value = "SLR00", Text = "Freight Bill" });
            items.Add(new SelectListItem { Value = "SLW00", Text = "Freight Bill Without Consignment" });
            //items.Add(new SelectListItem { Value = "CMM00", Text = "Cash Sale" });
            //items.Add(new SelectListItem { Value = "PUR00", Text = "Purchase Bill" });

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
            string DocNo = "";

            DocNo = ctxTFAT.AlertNoteMaster.OrderByDescending(x => x.RECORDKEY).Select(x => x.DocNo).FirstOrDefault();

            if (String.IsNullOrEmpty(DocNo))
            {
                DocNo = "100000";
            }
            else
            {
                var Integer = Convert.ToInt32(DocNo) + 1;
                DocNo = Integer.ToString("D6");
            }

            return DocNo;
        }

        public ActionResult SetToOtherType(AlertNoteVM mModel)
        {
            List<AlertNoteSubVM> items = new List<AlertNoteSubVM>();
            if (mModel.AType == "LR000")
            {
                items.Add(new AlertNoteSubVM { Code = "LC000", Name = "Lorry Challan" });
                items.Add(new AlertNoteSubVM { Code = "LOD00", Name = "Loading" });
                items.Add(new AlertNoteSubVM { Code = "UNLOD", Name = "Un-Loading" });
                items.Add(new AlertNoteSubVM { Code = "DELV0", Name = "Delivery" });
                items.Add(new AlertNoteSubVM { Code = "SLR00", Name = "Freight Bill" });
                items.Add(new AlertNoteSubVM { Code = "CMM00", Name = "Cash Sale" });
                items.Add(new AlertNoteSubVM { Code = "POD00", Name = "POD" });
            }
            //else if (mModel.AType == "LC000")
            //{
            //    items.Add(new AlertNoteSubVM { Code = "LC000", Name = "Lorry Challan" });
            //}
            else if (mModel.AType == "FM000" || mModel.AType == "FM000")
            {
                items.Add(new AlertNoteSubVM { Code = "FMP00", Name = "Advance Balance" });
                items.Add(new AlertNoteSubVM { Code = "ACTVT", Name = "Activity" });
            }
            else if (mModel.AType == "SLR00")
            {
                //items.Add(new AlertNoteSubVM { Code = "SLR00", Name = "Freight Bill" });
                items.Add(new AlertNoteSubVM { Code = "BLSMT", Name = "Bill Submission" });
            }
            else if (mModel.AType == "SLW00")
            {
                //items.Add(new AlertNoteSubVM { Code = "SLW00", Name = "Freight Bill Without Consignment" });
                items.Add(new AlertNoteSubVM { Code = "BLSMT", Name = "Bill Submission" });
            }
            //else if (mModel.AType == "CMM00")
            //{
            //    items.Add(new AlertNoteSubVM { Code = "CMM00", Name = "Cash Sale" });
            //}
            //else if (mModel.AType == "PUR00")
            //{
            //    items.Add(new AlertNoteSubVM { Code = "PUR00", Name = "Purchase Bill" });
            //}


            mModel.RefersType = items;

            var html = ViewHelper.RenderPartialView(this, "SetAlertPartialView", mModel);
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public List<AlertNoteSubVM> GetSelectedItem(string Type)
        {
            List<AlertNoteSubVM> items = new List<AlertNoteSubVM>();
            if (Type == "LR000")
            {
                items.Add(new AlertNoteSubVM { Code = "LC000", Name = "Lorry Challan" });
                items.Add(new AlertNoteSubVM { Code = "LOD00", Name = "Loading" });
                items.Add(new AlertNoteSubVM { Code = "UNLOD", Name = "Un-Loading" });
                items.Add(new AlertNoteSubVM { Code = "DELV0", Name = "Delivery" });
                items.Add(new AlertNoteSubVM { Code = "SLR00", Name = "Freight Bill" });
                items.Add(new AlertNoteSubVM { Code = "CMM00", Name = "Cash Sale" });
                items.Add(new AlertNoteSubVM { Code = "POD00", Name = "POD" });
            }
            //else if (Type == "LC000")
            //{
            //    items.Add(new AlertNoteSubVM { Code = "LC000", Name = "Lorry Challan" });
            //}
            else if (Type == "FM000" || Type == "FM000")
            {
                items.Add(new AlertNoteSubVM { Code = "FMP00", Name = "Advance Balance" });
                items.Add(new AlertNoteSubVM { Code = "ACTVT", Name = "Activity" });
            }
            else if (Type == "SLR00")
            {
                //items.Add(new AlertNoteSubVM { Code = "SLR00", Name = "Freight Bill" });
                items.Add(new AlertNoteSubVM { Code = "BLSMT", Name = "Bill Submission" });
            }
            else if (Type == "SLW00")
            {
                //items.Add(new AlertNoteSubVM { Code = "SLW00", Name = "Freight Bill Without Consignment" });
                items.Add(new AlertNoteSubVM { Code = "BLSMT", Name = "Bill Submission" });
            }
            //else if (Type == "CMM00")
            //{
            //    items.Add(new AlertNoteSubVM { Code = "CMM00", Name = "Cash Sale" });
            //}
            //else if (Type == "PUR00")
            //{
            //    items.Add(new AlertNoteSubVM { Code = "PUR00", Name = "Purchase Bill" });
            //}

            return items;
        }

        public int GetNewAttachCode()
        {
            string Code = ctxTFAT.Attachment.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).Take(1).FirstOrDefault();
            if (String.IsNullOrEmpty(Code))
            {
                return 100000;
            }
            else
            {
                return Convert.ToInt32(Code) + 1;
            }
        }

        #endregion

        // GET: Logistics/AlertNote
        #region Alert Note Own Pages Handles
        public ActionResult Index(AlertNoteVM mModel)
        {
            Session["TempAttach"] = null;
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, mModel.Document, "", "NA");
            if (mModel.ShortCutKey)
            {
                //mModel.DocNo = GetCode();
                if (mModel.AType == "LR000")
                {
                    mModel.ATypeN = "Lorry Receipt";
                    List<AlertNoteSubVM> items = new List<AlertNoteSubVM>();
                    items = GetSelectedItem(mModel.AType);
                    LRMaster lR = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString().Trim() == mModel.DocReceived).FirstOrDefault();
                    if (lR == null)
                    {
                        mModel.TypeCode = mModel.DocReceived;
                        mModel.RefersType = items;
                    }
                    else
                    {
                        mModel.TypeCode = lR.LrNo.ToString();
                        mModel.RefersType = items;
                        lR.Source = ctxTFAT.TfatBranch.Where(x => x.Code == lR.Source).Select(x => x.Name).FirstOrDefault();
                        lR.Dest = ctxTFAT.TfatBranch.Where(x => x.Code == lR.Dest).Select(x => x.Name).FirstOrDefault();
                        lR.RecCode = ctxTFAT.Consigner.Where(x => x.Code == lR.RecCode).Select(x => x.Name).FirstOrDefault();
                        lR.SendCode = ctxTFAT.Consigner.Where(x => x.Code == lR.SendCode).Select(x => x.Name).FirstOrDefault();
                        mModel.lRMaster = lR;
                        mModel.DocumentKey = lR.TableKey;
                    }

                }
                else
                {
                    mModel.ATypeN = "Freigh Memo";
                    List<AlertNoteSubVM> items = new List<AlertNoteSubVM>();
                    items = GetSelectedItem(mModel.AType);
                    FMMaster fM = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString().Trim() == mModel.DocReceived).FirstOrDefault();
                    if (fM == null)
                    {
                        mModel.TypeCode = mModel.DocReceived;
                        mModel.RefersType = items;
                    }
                    else
                    {
                        mModel.TypeCode = fM.FmNo.ToString();
                        mModel.RefersType = items;
                        fM.FromBranch = ctxTFAT.TfatBranch.Where(x => x.Code == fM.FromBranch).Select(x => x.Name).FirstOrDefault();
                        fM.ToBranch = ctxTFAT.TfatBranch.Where(x => x.Code == fM.ToBranch).Select(x => x.Name).FirstOrDefault();
                        fM.BroCode = ctxTFAT.Master.Where(x => x.Code == fM.BroCode).Select(x => x.Name).FirstOrDefault();
                        mModel.fMMaster = fM;
                        mModel.DocumentKey = fM.TableKey;
                    }

                }
            }
            else if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                List<AlertNoteSubVM> items = new List<AlertNoteSubVM>();
                AlertNoteMaster alertNote = ctxTFAT.AlertNoteMaster.Where(x => x.DocNo.Trim() == mModel.Document.Trim()).FirstOrDefault();

                mModel.DocNo = alertNote.DocNo;
                mModel.PartyNarr = alertNote.PartyNarr;


                //Get Attachment
                AttachmentVM Att = new AttachmentVM();
                Att.Type = "Alert";
                Att.Srl = alertNote.DocNo.ToString();

                AttachmentController attachmentC = new AttachmentController();
                List<AttachmentVM> attachments = attachmentC.GetAttachmentListInEdit(Att);
                Session["TempAttach"] = attachments;


                items = GetSelectedItem(alertNote.Type);
                if (!String.IsNullOrEmpty(alertNote.RefType))
                {
                    var SelecedItemList = alertNote.RefType.Split(',').ToList();
                    items.Where(x => SelecedItemList.Contains(x.Code)).ToList().ForEach(i => i.select = true);
                }
                if (!String.IsNullOrEmpty(alertNote.Stop))
                {
                    var SelecedItemList = alertNote.Stop.Split(',').ToList();
                    items.Where(x => SelecedItemList.Contains(x.Code)).ToList().ForEach(i => i.stop = true);
                }



                mModel.AType = alertNote.Type;
                mModel.TypeCode = alertNote.TypeCode;
                if (alertNote.Type == "LR000")
                {
                    mModel.ATypeN = "Lorry Receipt";
                    mModel.DocReceived = alertNote.ParentKey;
                    LRMaster lR = ctxTFAT.LRMaster.Where(x => x.TableKey == alertNote.ParentKey).FirstOrDefault();
                    if (lR != null)
                    {
                        lR.Source = ctxTFAT.TfatBranch.Where(x => x.Code == lR.Source).Select(x => x.Name).FirstOrDefault();
                        lR.Dest = ctxTFAT.TfatBranch.Where(x => x.Code == lR.Dest).Select(x => x.Name).FirstOrDefault();
                        lR.RecCode = ctxTFAT.Consigner.Where(x => x.Code == lR.RecCode).Select(x => x.Name).FirstOrDefault();
                        lR.SendCode = ctxTFAT.Consigner.Where(x => x.Code == lR.SendCode).Select(x => x.Name).FirstOrDefault();
                        mModel.lRMaster = lR;
                        mModel.DocumentKey = lR.TableKey;
                    }

                }
                else if (alertNote.Type == "FM000" || alertNote.Type == "FM000")
                {
                    mModel.ATypeN = "Freigh Memo";
                    mModel.DocReceived = alertNote.ParentKey;
                    FMMaster fM = ctxTFAT.FMMaster.Where(x => x.TableKey == alertNote.ParentKey).FirstOrDefault();
                    if (fM != null)
                    {
                        fM.FromBranch = ctxTFAT.TfatBranch.Where(x => x.Code == fM.FromBranch).Select(x => x.Name).FirstOrDefault();
                        fM.ToBranch = ctxTFAT.TfatBranch.Where(x => x.Code == fM.ToBranch).Select(x => x.Name).FirstOrDefault();
                        fM.BroCode = ctxTFAT.Master.Where(x => x.Code == fM.BroCode).Select(x => x.Name).FirstOrDefault();
                        mModel.fMMaster = fM;
                        mModel.DocumentKey = fM.TableKey;
                    }

                }
                else if (alertNote.Type == "LC000")
                {
                    mModel.ATypeN = "Lorry Challan";
                    mModel.DocReceived = alertNote.ParentKey;
                    LCMaster fM = ctxTFAT.LCMaster.Where(x => x.TableKey == alertNote.ParentKey).FirstOrDefault();
                    if (fM != null)
                    {
                        fM.FromBranch = ctxTFAT.TfatBranch.Where(x => x.Code == fM.FromBranch).Select(x => x.Name).FirstOrDefault();
                        fM.ToBranch = ctxTFAT.TfatBranch.Where(x => x.Code == fM.ToBranch).Select(x => x.Name).FirstOrDefault();
                        fM.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == fM.Branch).Select(x => x.Name).FirstOrDefault();
                        mModel.lCMaster = fM;
                        mModel.DocumentKey = fM.TableKey;
                    }

                }
                else if (alertNote.Type == "SLR00")
                {
                    mModel.ATypeN = "Freight Bill";
                    mModel.DocReceived = alertNote.ParentKey;
                    var mlist = ctxTFAT.Sales.Where(x => x.TableKey == alertNote.ParentKey).FirstOrDefault();
                    if (mlist != null)
                    {
                        mlist.Code = ctxTFAT.CustomerMaster.Where(x => x.Code == mlist.Code).Select(x => x.Name).FirstOrDefault();
                        mlist.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Branch).Select(x => x.Name).FirstOrDefault();
                        mModel.sales = mlist;
                        mModel.DocumentKey = mlist.TableKey;
                    }
                }
                else if (alertNote.Type == "SLW00")
                {
                    mModel.ATypeN = "Freight Bill Without Consignment";
                    mModel.DocReceived = alertNote.ParentKey;
                    var mlist = ctxTFAT.Sales.Where(x => x.TableKey == alertNote.ParentKey).FirstOrDefault();
                    if (mlist != null)
                    {
                        mlist.Code = ctxTFAT.CustomerMaster.Where(x => x.Code == mlist.Code).Select(x => x.Name).FirstOrDefault();
                        mlist.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Branch).Select(x => x.Name).FirstOrDefault();
                        mModel.sales = mlist;
                        mModel.DocumentKey = mlist.TableKey;
                    }
                }
                else if (alertNote.Type == "CMM00")
                {
                    mModel.ATypeN = "Cash Sale";
                    mModel.DocReceived = alertNote.ParentKey;
                    var mlist = ctxTFAT.Sales.Where(x => x.TableKey == alertNote.ParentKey).FirstOrDefault();
                    if (mlist != null)
                    {
                        mlist.Code = ctxTFAT.Master.Where(x => x.Code == mlist.CashBankCode).Select(x => x.Name).FirstOrDefault();
                        mlist.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Branch).Select(x => x.Name).FirstOrDefault();
                        mModel.sales = mlist;
                        mModel.DocumentKey = mlist.TableKey;
                    }
                }
                else if (alertNote.Type == "PUR00")
                {
                    mModel.ATypeN = "Purchase Bill";
                    mModel.DocReceived = alertNote.ParentKey;
                    var mlist = ctxTFAT.Purchase.Where(x => x.TableKey == alertNote.ParentKey).FirstOrDefault();
                    if (mlist != null)
                    {
                        mlist.Code = ctxTFAT.Master.Where(x => x.Code == mlist.Code).Select(x => x.Name).FirstOrDefault();
                        mlist.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Branch).Select(x => x.Name).FirstOrDefault();
                        mModel.purchase = mlist;
                        mModel.DocumentKey = mlist.TableKey;
                    }
                }

                mModel.RefersType = items;
                mModel.Remark = alertNote.Note;
                mModel.Bling = alertNote.Bling;
                //mModel.Stop = alertNote.Stop;
            }
            else
            {
                //mModel.DocNo = GetCode();
            }
            return View(mModel);
        }
        public ActionResult SaveData(AlertNoteVM mModel)
        {

            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        var Msg = DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();


                        if (Msg == "Success")
                        {
                            return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { Status = "Error", Message = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    AlertNoteMaster mobj = new AlertNoteMaster();
                    bool mAdd = true;
                    if (ctxTFAT.AlertNoteMaster.Where(x => x.DocNo.ToString() == mModel.Document.ToString()).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.AlertNoteMaster.Where(x => x.DocNo.ToString() == mModel.Document.ToString()).FirstOrDefault();
                        mAdd = false;
                    }
                    if (mAdd)
                    {
                        mobj.DocDate = DateTime.Now;
                        mobj.DocNo = GetCode();
                        mobj.CreateBy = muserid;
                        mobj.Branch = mbranchcode;
                        mobj.Prefix = mperiod;
                    }

                    string RefType = ""; string RefTypestop = "";
                    if (mModel.RefersType != null)
                    {
                        foreach (var item in mModel.RefersType.Where(x => x.select == true).ToList())
                        {
                            RefType += item.Code + ",";
                        }
                        if (!String.IsNullOrEmpty(RefType))
                        {
                            RefType = RefType.Substring(0, RefType.Length - 1);
                        }

                        foreach (var item in mModel.RefersType.Where(x => x.stop == true).ToList())
                        {
                            RefTypestop += item.Code + ",";
                        }
                        if (!String.IsNullOrEmpty(RefTypestop))
                        {
                            RefTypestop = RefTypestop.Substring(0, RefTypestop.Length - 1);
                        }
                    }

                    mobj.PartyNarr = mModel.PartyNarr;
                    mobj.Type = mModel.AType;
                    mobj.TypeCode = mModel.TypeCode;
                    mobj.RefType = RefType;
                    mobj.Note = mModel.Remark;
                    mobj.Bling = mModel.Bling;
                    mobj.Stop = RefTypestop;
                    if (mobj.Type == "LR000")
                    {
                        LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == mModel.DocumentKey ).FirstOrDefault();
                        if (lRMaster != null)
                        {
                            mobj.ParentKey = lRMaster.TableKey;
                        }
                        else
                        {
                            mobj.ParentKey =mbranchcode+ "LR000" + mperiod.Substring(0, 2) + "001" + mobj.TypeCode;
                        }

                    }
                    else if (mobj.Type == "LC000")
                    {
                        LCMaster lCMaster = ctxTFAT.LCMaster.Where(x => x.TableKey.ToString() == mModel.DocumentKey ).FirstOrDefault();
                        if (lCMaster != null)
                        {
                            mobj.ParentKey = lCMaster.TableKey;
                        }
                        else
                        {
                            mobj.ParentKey = mbranchcode + "LC000" + mperiod.Substring(0, 2) + "001" + mobj.TypeCode;
                        }

                    }
                    else if (mobj.Type == "FM000")
                    {

                        FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == mModel.DocumentKey ).FirstOrDefault();
                        if (fMMaster != null)
                        {
                            mobj.ParentKey = fMMaster.TableKey;
                        }
                        else
                        {
                            mobj.ParentKey = mbranchcode + "FM000" + mperiod.Substring(0, 2) + "001" + mobj.TypeCode;
                        }
                    }
                    else if (mobj.Type == "FM000")
                    {

                        FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == mModel.DocumentKey ).FirstOrDefault();
                        if (fMMaster != null)
                        {
                            mobj.ParentKey = fMMaster.TableKey;
                        }
                        else
                        {
                            mobj.ParentKey = mbranchcode + "FM000" + mperiod.Substring(0, 2) + "001" + mobj.TypeCode;
                        }
                    }
                    else if (mobj.Type == "SLR00" || mobj.Type == "SLW00" || mobj.Type == "CMM00")
                    {

                        string Srl = mobj.TypeCode;

                        var result1 = ctxTFAT.DocTypes.Where(x => x.Code.ToLower() == mobj.Type.ToLower().Trim()).Select(x => x).FirstOrDefault();
                        Srl = Srl.PadLeft(result1.DocWidth, '0');

                        mobj.ParentKey = ctxTFAT.Sales.Where(x => x.TableKey.ToString() == mModel.DocumentKey && x.Type == mobj.Type.Trim() ).Select(x => x.TableKey).FirstOrDefault();
                        if (mobj.ParentKey == null)
                        {
                            mobj.ParentKey = mbranchcode + mobj.Type + mperiod.Substring(0, 2) + Srl;
                        }
                        mobj.TypeCode = Srl;

                    }
                    else if (mobj.Type == "PUR00")
                    {

                        string Srl = mobj.TypeCode;
                        var result1 = ctxTFAT.DocTypes.Where(x => x.Code.ToLower() == mobj.Type.ToLower().Trim()).Select(x => x).FirstOrDefault();
                        Srl = Srl.PadLeft(result1.DocWidth, '0');
                        var NewInteger = Convert.ToInt32(Srl).ToString();
                        mobj.ParentKey = ctxTFAT.Purchase.Where(x => x.TableKey.ToString() == mModel.DocumentKey && x.Type == mobj.Type.Trim() ).Select(x => x.TableKey).FirstOrDefault();
                        if (mobj.ParentKey == null)
                        {
                            mobj.ParentKey = mbranchcode + mobj.Type + mperiod.Substring(0, 2) + Srl;
                        }
                        mobj.TypeCode = Srl;

                    }
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;

                    AttachmentVM vM = new AttachmentVM();
                    vM.ParentKey = "ALERT" + mperiod.Substring(0, 2) + mobj.DocNo;
                    vM.Srl = mobj.DocNo.ToString();
                    vM.Type = "Alert";
                    SaveAttachment(vM);

                    if (mAdd == true)
                    {
                        mobj.TableKey = "ALERT" + mperiod.Substring(0, 2) + 1.ToString("D3") + mobj.DocNo;
                        ctxTFAT.AlertNoteMaster.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "ARTNT" + mperiod.Substring(0, 2) + mobj.DocNo, DateTime.Now, 0, "", mobj.Type + " : " + mobj.TypeCode, "NA");
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

            return Json(new { Status = "Success", id = "AlertMaster" }, JsonRequestBehavior.AllowGet);
        }
        public string DeleteStateMaster(AlertNoteVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return "Code not Entered..";
            }


            var Alert = ctxTFAT.AlertNoteMaster.Where(x => x.DocNo.Trim() == mModel.Document.Trim()).FirstOrDefault();

            var AlertAttachMent = ctxTFAT.Attachment.Where(x => x.Type == "Alert" && x.Srl == Alert.DocNo).ToList();
            foreach (var item in AlertAttachMent)
            {
                if (System.IO.File.Exists(item.FilePath))
                {
                    System.IO.File.Delete(item.FilePath);
                }
            }
            ctxTFAT.Attachment.RemoveRange(AlertAttachMent);
            ctxTFAT.AlertNoteMaster.Remove(Alert);
            ctxTFAT.SaveChanges();
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "ARTNT" + mperiod.Substring(0, 2) + mModel.Document, DateTime.Now, 0, "", " Delete Alert Note OF " + Alert.Type + " : " + Alert.TypeCode, "NA");
            return "Success";
        }
        #endregion

        #region Common Pages To Use Another Multiple Pages
        public ActionResult PartialView(AlertNoteVM mModel)
        {
            Session["TempAttach"] = null;
            //mModel.DocNo = GetCode();
            if (mModel.AType == "LR000")
            {
                mModel.ATypeN = "Lorry Receipt";
                List<AlertNoteSubVM> items = new List<AlertNoteSubVM>();
                items = GetSelectedItem(mModel.AType);
                LRMaster lR = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString().Trim() == mModel.DocumentKey).FirstOrDefault();
                if (lR == null)
                {
                    mModel.TypeCode = mModel.DocReceived;
                    mModel.RefersType = items;
                }
                else
                {
                    mModel.TypeCode = lR.LrNo.ToString();
                    mModel.RefersType = items;
                    lR.Source = ctxTFAT.TfatBranch.Where(x => x.Code == lR.Source).Select(x => x.Name).FirstOrDefault();
                    lR.Dest = ctxTFAT.TfatBranch.Where(x => x.Code == lR.Dest).Select(x => x.Name).FirstOrDefault();
                    lR.RecCode = ctxTFAT.Consigner.Where(x => x.Code == lR.RecCode).Select(x => x.Name).FirstOrDefault();
                    lR.SendCode = ctxTFAT.Consigner.Where(x => x.Code == lR.SendCode).Select(x => x.Name).FirstOrDefault();
                    mModel.lRMaster = lR;
                    mModel.DocumentKey = lR.TableKey;
                }

            }
            else if (mModel.AType == "LC000")
            {
                mModel.ATypeN = "Lorry Challan";
                List<AlertNoteSubVM> items = new List<AlertNoteSubVM>();
                items = GetSelectedItem(mModel.AType);
                LCMaster fM = ctxTFAT.LCMaster.Where(x => x.TableKey.ToString().Trim() == mModel.DocumentKey).FirstOrDefault();
                if (fM == null)
                {
                    mModel.TypeCode = mModel.DocReceived;
                    mModel.RefersType = items;
                }
                else
                {
                    mModel.TypeCode = mModel.DocReceived;
                    mModel.RefersType = items;
                    fM.FromBranch = ctxTFAT.TfatBranch.Where(x => x.Code == fM.FromBranch).Select(x => x.Name).FirstOrDefault();
                    fM.ToBranch = ctxTFAT.TfatBranch.Where(x => x.Code == fM.ToBranch).Select(x => x.Name).FirstOrDefault();
                    fM.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == fM.Branch).Select(x => x.Name).FirstOrDefault();
                    mModel.lCMaster = fM;
                    mModel.DocumentKey = fM.TableKey;

                }

            }
            else if (mModel.AType == "FM000" || mModel.AType == "FM000")
            {
                mModel.ATypeN = "Freigh Memo";
                List<AlertNoteSubVM> items = new List<AlertNoteSubVM>();
                items = GetSelectedItem(mModel.AType);
                FMMaster fM = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString().Trim() == mModel.DocumentKey).FirstOrDefault();
                if (fM == null)
                {
                    mModel.TypeCode = mModel.DocReceived;
                    mModel.RefersType = items;
                }
                else
                {
                    mModel.TypeCode = fM.FmNo.ToString();
                    mModel.RefersType = items;
                    fM.FromBranch = ctxTFAT.TfatBranch.Where(x => x.Code == fM.FromBranch).Select(x => x.Name).FirstOrDefault();
                    fM.ToBranch = ctxTFAT.TfatBranch.Where(x => x.Code == fM.ToBranch).Select(x => x.Name).FirstOrDefault();
                    fM.BroCode = ctxTFAT.Master.Where(x => x.Code == fM.BroCode).Select(x => x.Name).FirstOrDefault();
                    mModel.fMMaster = fM;
                    mModel.DocumentKey = fM.TableKey;

                }
            }
            else if (mModel.AType == "SLR00")
            {
                mModel.ATypeN = "Freight Bill";
                List<AlertNoteSubVM> items = new List<AlertNoteSubVM>();
                items = GetSelectedItem(mModel.AType);

                var result1 = ctxTFAT.DocTypes.Where(x => x.Code == "SLR00").Select(x => x).FirstOrDefault();
                var Srl = mModel.DocReceived.PadLeft(result1.DocWidth, '0');

                Sales mlist = ctxTFAT.Sales.Where(x => x.TableKey.ToString().Trim() == mModel.DocumentKey && x.Type == "SLR00").FirstOrDefault();
                if (mlist == null)
                {
                    mModel.TypeCode = Srl;
                    mModel.RefersType = items;
                }
                else
                {
                    mModel.TypeCode = Srl.ToString();
                    mModel.RefersType = items;
                    mlist.Code = ctxTFAT.CustomerMaster.Where(x => x.Code == mlist.Code).Select(x => x.Name).FirstOrDefault();
                    mlist.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Branch).Select(x => x.Name).FirstOrDefault();
                    mModel.sales = mlist;
                    mModel.DocumentKey = mlist.TableKey;

                }
            }
            else if (mModel.AType == "SLW00")
            {
                mModel.ATypeN = "Freight Bill Without Consignment";
                List<AlertNoteSubVM> items = new List<AlertNoteSubVM>();
                items = GetSelectedItem(mModel.AType);

                var result1 = ctxTFAT.DocTypes.Where(x => x.Code == "SLW00").Select(x => x).FirstOrDefault();
                var Srl = mModel.DocReceived.PadLeft(result1.DocWidth, '0');

                Sales mlist = ctxTFAT.Sales.Where(x => x.TableKey.ToString().Trim() == mModel.DocumentKey && x.Type == "SLW00").FirstOrDefault();
                if (mlist == null)
                {
                    mModel.TypeCode = Srl;
                    mModel.RefersType = items;
                }
                else
                {
                    mModel.TypeCode = Srl.ToString();
                    mModel.RefersType = items;
                    mlist.Code = ctxTFAT.CustomerMaster.Where(x => x.Code == mlist.Code).Select(x => x.Name).FirstOrDefault();
                    mlist.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Branch).Select(x => x.Name).FirstOrDefault();
                    mModel.sales = mlist;
                    mModel.DocumentKey = mlist.TableKey;

                }
            }
            else if (mModel.AType == "CMM00")
            {
                mModel.ATypeN = "Cash Sale";
                List<AlertNoteSubVM> items = new List<AlertNoteSubVM>();
                items = GetSelectedItem(mModel.AType);

                var result1 = ctxTFAT.DocTypes.Where(x => x.Code == "CMM00").Select(x => x).FirstOrDefault();
                var Srl = mModel.DocReceived.PadLeft(result1.DocWidth, '0');

                Sales mlist = ctxTFAT.Sales.Where(x => x.Srl.ToString().Trim() == mModel.DocumentKey && x.Type == "CMM00").FirstOrDefault();
                if (mlist == null)
                {
                    mModel.TypeCode = Srl;
                    mModel.RefersType = items;
                }
                else
                {
                    mModel.TypeCode = Srl.ToString();
                    mModel.RefersType = items;
                    mlist.Code = ctxTFAT.CustomerMaster.Where(x => x.Code == mlist.Code).Select(x => x.Name).FirstOrDefault();
                    mlist.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Branch).Select(x => x.Name).FirstOrDefault();
                    mModel.sales = mlist;
                    mModel.DocumentKey = mlist.TableKey;
                }
            }
            else if (mModel.AType == "PUR00")
            {
                mModel.ATypeN = "Purchase Bill";
                List<AlertNoteSubVM> items = new List<AlertNoteSubVM>();
                items = GetSelectedItem(mModel.AType);

                var result1 = ctxTFAT.DocTypes.Where(x => x.Code == "PUR00").Select(x => x).FirstOrDefault();
                var Srl = mModel.DocReceived.PadLeft(result1.DocWidth, '0');
                Srl = Convert.ToInt32(Srl).ToString();
                Purchase mlist = ctxTFAT.Purchase.Where(x => x.Srl.Value.ToString().Trim() == mModel.DocumentKey && x.Type == "PUR00").FirstOrDefault();
                if (mlist == null)
                {
                    mModel.TypeCode = mModel.DocReceived.PadLeft(result1.DocWidth, '0');
                    mModel.RefersType = items;
                }
                else
                {
                    mModel.TypeCode = mModel.DocReceived.PadLeft(result1.DocWidth, '0');
                    mModel.RefersType = items;
                    mlist.Code = ctxTFAT.CustomerMaster.Where(x => x.Code == mlist.Code).Select(x => x.Name).FirstOrDefault();
                    mlist.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Branch).Select(x => x.Name).FirstOrDefault();
                    mModel.purchase = mlist;
                    mModel.DocumentKey = mlist.TableKey;
                }
            }

            var html = ViewHelper.RenderPartialView(this, "CommonView", mModel);
            var jsonResult = Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public ActionResult SaveDataGetList(AlertNoteVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        var Msg = DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();

                        if (Msg == "Success")
                        {
                            return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { Status = "Error", Message = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    AlertNoteMaster mobj = new AlertNoteMaster();
                    bool mAdd = true;
                    if (ctxTFAT.AlertNoteMaster.Where(x => x.DocNo.ToString() == mModel.Document.ToString()).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.AlertNoteMaster.Where(x => x.DocNo.ToString() == mModel.Document.ToString()).FirstOrDefault();
                        mAdd = false;
                    }
                    if (mAdd)
                    {
                        mobj.DocDate = DateTime.Now;
                        mobj.DocNo = GetCode();
                        mobj.CreateBy = muserid;
                        mobj.Branch = mbranchcode;
                        mobj.Prefix = mperiod;
                    }

                    string RefType = ""; string RefTypestop = "";
                    if (mModel.RefersType != null)
                    {
                        foreach (var item in mModel.RefersType.Where(x => x.select == true).ToList())
                        {
                            RefType += item.Code + ",";
                        }
                        if (!String.IsNullOrEmpty(RefType))
                        {
                            RefType = RefType.Substring(0, RefType.Length - 1);
                        }

                        foreach (var item in mModel.RefersType.Where(x => x.stop == true).ToList())
                        {
                            RefTypestop += item.Code + ",";
                        }
                        if (!String.IsNullOrEmpty(RefTypestop))
                        {
                            RefTypestop = RefTypestop.Substring(0, RefTypestop.Length - 1);
                        }
                    }

                    mobj.Type = mModel.AType;
                    mobj.PartyNarr = mModel.PartyNarr;
                    mobj.TypeCode = mModel.TypeCode;
                    mobj.RefType = RefType;
                    mobj.Note = mModel.Remark;
                    mobj.Bling = mModel.Bling;
                    mobj.Stop = RefTypestop;
                    mobj.TableKey = "ALERT" + mperiod.Substring(0, 2) + 1.ToString("D3") + mobj.DocNo;
                    if (mobj.Type == "LR000")
                    {
                        LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == mModel.DocumentKey).FirstOrDefault();
                        if (lRMaster != null)
                        {
                            mobj.ParentKey = lRMaster.TableKey;
                        }
                        else
                        {
                            mobj.ParentKey = mbranchcode + "LR000" + mperiod.Substring(0, 2) + "001" + mobj.TypeCode;
                        }

                    }
                    else if (mobj.Type == "LC000")
                    {
                        LCMaster lRMaster = ctxTFAT.LCMaster.Where(x => x.TableKey.ToString() == mModel.DocumentKey).FirstOrDefault();
                        if (lRMaster != null)
                        {
                            mobj.ParentKey = lRMaster.TableKey;
                        }
                        else
                        {
                            mobj.ParentKey = mbranchcode + "LC000" + mperiod.Substring(0, 2) + "001" + mobj.TypeCode;
                        }

                    }
                    else if (mobj.Type == "FM000")
                    {

                        FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == mModel.DocumentKey).FirstOrDefault();
                        if (fMMaster != null)
                        {
                            mobj.ParentKey = fMMaster.TableKey;
                        }
                        else
                        {
                            mobj.ParentKey = mbranchcode + "FM000" + mperiod.Substring(0, 2) + "001" + mobj.TypeCode;
                        }
                    }
                    else if (mobj.Type == "FM000")
                    {

                        FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == mModel.DocumentKey).FirstOrDefault();
                        if (fMMaster != null)
                        {
                            mobj.ParentKey = fMMaster.TableKey;
                        }
                        else
                        {
                            mobj.ParentKey = mbranchcode + "FM000" + mperiod.Substring(0, 2) + "001" + mobj.TypeCode;
                        }
                    }
                    else if (mobj.Type == "SLR00" || mobj.Type == "SLW00" || mobj.Type == "CMM00")
                    {
                        string Srl = mobj.TypeCode;
                        var result1 = ctxTFAT.DocTypes.Where(x => x.Code.ToLower() == mobj.Type.ToLower().Trim()).Select(x => x).FirstOrDefault();
                        Srl = Srl.PadLeft(result1.DocWidth, '0');


                        mobj.ParentKey = ctxTFAT.Sales.Where(x => x.TableKey.ToString() == mModel.DocumentKey && x.Type == mobj.Type.Trim()).Select(x => x.TableKey).FirstOrDefault();
                        if (mobj.ParentKey == null)
                        {
                            mobj.ParentKey = mbranchcode + mobj.Type + mperiod.Substring(0, 2) + Srl;
                        }
                        mobj.TypeCode = Srl;
                    }
                    else if (mobj.Type == "PUR00")
                    {
                        string Srl = mobj.TypeCode;
                        var result1 = ctxTFAT.DocTypes.Where(x => x.Code.ToLower() == mobj.Type.ToLower().Trim()).Select(x => x).FirstOrDefault();
                        Srl = Srl.PadLeft(result1.DocWidth, '0');

                        var NewSrlInteger = Convert.ToInt32(Srl).ToString();
                        mobj.ParentKey = ctxTFAT.Purchase.Where(x => x.TableKey.ToString() == mModel.DocumentKey && x.Type == mobj.Type.Trim()).Select(x => x.TableKey).FirstOrDefault();
                        if (mobj.ParentKey == null)
                        {
                            mobj.ParentKey = mbranchcode + mobj.Type + mperiod.Substring(0, 2) + Srl;
                        }
                        mobj.TypeCode = Srl;
                    }
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;



                    if (mModel.Mode != "Add")
                    {
                        AttachmentVM vM = new AttachmentVM();
                        vM.ParentKey = "ALERT" + mperiod.Substring(0, 2) + mobj.DocNo;
                        vM.Srl = mobj.DocNo.ToString();
                        vM.Type = "Alert";
                        SaveAttachment(vM);

                        if (mAdd == true)
                        {
                            ctxTFAT.AlertNoteMaster.Add(mobj);
                        }
                        else
                        {
                            ctxTFAT.Entry(mobj).State = EntityState.Modified;
                        }

                        ctxTFAT.SaveChanges();
                        transaction.Commit();
                        transaction.Dispose();
                    }
                    else
                    {
                        List<AlertNoteMaster> objledgerdetail = new List<AlertNoteMaster>();

                        if (Session["CommnNarrlist"] != null)
                        {
                            objledgerdetail = (List<AlertNoteMaster>)Session["CommnNarrlist"];
                        }
                        mobj.TableKey = (objledgerdetail.Count() + 1).ToString();
                        objledgerdetail.Add(mobj);
                        Session["CommnNarrlist"] = objledgerdetail;
                    }

                    return RedirectToAction("ShoWAlertNoteList", new { Type = mobj.Type, TypeCode = mobj.ParentKey ?? "", DocTpe = mobj.Type });
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

            //return Json(new { Status = "Success", id = "AlertMaster" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteAlertNote(AlertNoteVM mModel)
        {
            string html = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    List<AlertNoteMaster> objledgerdetail = new List<AlertNoteMaster>();
                    if (Session["CommnNarrlist"] != null)
                    {
                        objledgerdetail = (List<AlertNoteMaster>)Session["CommnNarrlist"];
                    }
                    if (objledgerdetail == null)
                    {
                        objledgerdetail = new List<AlertNoteMaster>();
                    }

                    AlertNoteMaster alertNote = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == mModel.DocReceived).FirstOrDefault();
                    if (alertNote != null)
                    {
                        ctxTFAT.AlertNoteMaster.Remove(alertNote);
                    }
                    else
                    {
                        objledgerdetail = objledgerdetail.Where(x => x.TableKey != mModel.DocReceived).ToList();
                        Session["CommnNarrlist"] = objledgerdetail;
                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();


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
            return Json(new { Html = html, Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Common Function Use Default
        public ActionResult CheckDocumentYear(string Type, string DocNo)
        {
            AlertNoteVM mModel = new AlertNoteVM();
            mModel.AType = Type;
            string Single = "Yes",Tablekey="";
            if (Type == "LR000")
            {
                var GetLR = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString().Trim() == DocNo.ToString().Trim()).ToList();
                if (GetLR.Count()>1)
                {
                    Single = "No";
                    List<LRMaster> lRMasters = new List<LRMaster>();
                    foreach (var item in GetLR)
                    {
                        item.Source = ctxTFAT.TfatBranch.Where(x => x.Code == item.Source).Select(x => x.Name).FirstOrDefault();
                        item.Dest = ctxTFAT.TfatBranch.Where(x => x.Code == item.Dest).Select(x => x.Name).FirstOrDefault();
                        item.RecCode = ctxTFAT.Consigner.Where(x => x.Code == item.RecCode).Select(x => x.Name).FirstOrDefault();
                        item.SendCode = ctxTFAT.Consigner.Where(x => x.Code == item.SendCode).Select(x => x.Name).FirstOrDefault();
                        lRMasters.Add(item);
                    }
                    mModel.LRMasterslist = lRMasters;
                }
                else
                {
                    Tablekey = GetLR.Select(x => x.TableKey).FirstOrDefault();
                }
            }
            else if (Type == "LC000")
            {
                var GetLC = ctxTFAT.LCMaster.Where(x => x.LCno.ToString().Trim() == DocNo.ToString().Trim()).ToList();
                if (GetLC.Count() > 1)
                {
                    Single = "No";
                    List<LCMaster> lCMasters = new List<LCMaster>();
                    foreach (var item in GetLC)
                    {
                        item.FromBranch = ctxTFAT.TfatBranch.Where(x => x.Code == item.FromBranch).Select(x => x.Name).FirstOrDefault();
                        item.ToBranch = ctxTFAT.TfatBranch.Where(x => x.Code == item.ToBranch).Select(x => x.Name).FirstOrDefault();
                        item.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault();
                        lCMasters.Add(item);
                    }
                    mModel.LCMasterslist = lCMasters;
                }
                else
                {
                    Tablekey = GetLC.Select(x => x.TableKey).FirstOrDefault();
                }
            }
            else if (Type == "FM000" || Type == "FM000")
            {
                var GetFM = ctxTFAT.FMMaster.Where(x => x.FmNo.ToString().Trim() == DocNo.ToString().Trim()).ToList();
                if (GetFM.Count() > 1)
                {
                    Single = "No";
                    List<FMMaster> fMMasters = new List<FMMaster>();
                    foreach (var item in GetFM)
                    {
                        item.FromBranch = ctxTFAT.TfatBranch.Where(x => x.Code == item.FromBranch).Select(x => x.Name).FirstOrDefault();
                        item.ToBranch = ctxTFAT.TfatBranch.Where(x => x.Code == item.ToBranch).Select(x => x.Name).FirstOrDefault();
                        item.BroCode = ctxTFAT.Master.Where(x => x.Code == item.BroCode).Select(x => x.Name).FirstOrDefault();
                        fMMasters.Add(item);
                    }
                    mModel.FMMasterslist = fMMasters;
                }
                else
                {
                    Tablekey = GetFM.Select(x => x.TableKey).FirstOrDefault();
                }
            }
            else if (Type == "SLR00" || Type == "SLW00" || Type == "CMM00")
            {
                string Srl = DocNo;
                var result1 = ctxTFAT.DocTypes.Where(x => x.Code.ToLower() == Type.ToLower().Trim()).Select(x => x).FirstOrDefault();
                Srl = Srl.PadLeft(result1.DocWidth, '0');

                var mlist = ctxTFAT.Sales.Where(x => x.Srl.ToString().Trim() == Srl.Trim() && x.Type.Trim() == Type.Trim()).ToList();
                if (mlist.Count()>1)
                {
                    Single = "No";
                    List<Sales> sales = new List<Sales>();
                    foreach (var item in mlist)
                    {
                        if (Type == "CMM00")
                        {
                            item.Code = ctxTFAT.Master.Where(x => x.Code == item.CashBankCode).Select(x => x.Name).FirstOrDefault();
                        }
                        else
                        {
                            item.Code = ctxTFAT.CustomerMaster.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault();
                        }
                        item.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault();
                        sales.Add(item);
                    }
                    mModel.Saleslist = sales;
                }
                else
                {
                    Tablekey = mlist.Select(x => x.TableKey).FirstOrDefault();
                }
            }
            else if (Type == "PUR00")
            {
                string Srl = DocNo;
                var result1 = ctxTFAT.DocTypes.Where(x => x.Code.ToLower() == Type.ToLower().Trim()).Select(x => x).FirstOrDefault();
                Srl = Srl.PadLeft(result1.DocWidth, '0');
                var NewInteger = Convert.ToInt32(Srl).ToString();
                var mlist = ctxTFAT.Purchase.Where(x => x.Srl.Value.ToString() == NewInteger && x.Type.Trim() == Type.Trim()).ToList();
                if (mlist.Count() > 1)
                {
                    Single = "No";
                    List<Purchase> purchases = new List<Purchase>();
                    foreach (var item in mlist)
                    {
                        item.Code = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault();
                        item.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault();
                        purchases.Add(item);
                    }
                    mModel.Purchaseslist = purchases;
                }
                else
                {
                    Tablekey = mlist.Select(x => x.TableKey).FirstOrDefault();
                }

            }

            string Html = "";
            if (Single != "Yes")
            {
                Html = ViewHelper.RenderPartialView(this, "MultipleDocumentList", mModel);
            }
            var jsonResult = Json(new { Status = Single, Html = Html, Tablekey= Tablekey }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public ActionResult CheckDocument(string Type, string DocNo, string DocumentKey)
        {

            AlertNoteVM mModel = new AlertNoteVM();
            mModel.AType = Type;
            mModel.TypeCode = DocNo;
            string Status = "Success", Message = "";
            string BasicHtml = "";
            if (Type == "LR000")
            {
                var GetLR = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString().Trim() == DocumentKey.ToString().Trim() ).FirstOrDefault();
                if (GetLR == null)
                {
                    Status = "Error";
                    Message = "Consignment Not Found...!";
                }
                else
                {
                    GetLR.Source = ctxTFAT.TfatBranch.Where(x => x.Code == GetLR.Source).Select(x => x.Name).FirstOrDefault();
                    GetLR.Dest = ctxTFAT.TfatBranch.Where(x => x.Code == GetLR.Dest).Select(x => x.Name).FirstOrDefault();
                    GetLR.RecCode = ctxTFAT.Consigner.Where(x => x.Code == GetLR.RecCode).Select(x => x.Name).FirstOrDefault();
                    GetLR.SendCode = ctxTFAT.Consigner.Where(x => x.Code == GetLR.SendCode).Select(x => x.Name).FirstOrDefault();
                    mModel.lRMaster = GetLR;
                    BasicHtml = ViewHelper.RenderPartialView(this, "BasicDetailsDocView", mModel);
                }
            }
            else if (Type == "LC000")
            {
                var GetLC = ctxTFAT.LCMaster.Where(x => x.TableKey.ToString().Trim() == DocumentKey.ToString().Trim() ).FirstOrDefault();
                if (GetLC == null)
                {
                    Status = "Error";
                    Message = "Lorry Challan Not Found...!";
                }
                else
                {
                    GetLC.FromBranch = ctxTFAT.TfatBranch.Where(x => x.Code == GetLC.FromBranch).Select(x => x.Name).FirstOrDefault();
                    GetLC.ToBranch = ctxTFAT.TfatBranch.Where(x => x.Code == GetLC.ToBranch).Select(x => x.Name).FirstOrDefault();
                    GetLC.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == GetLC.Branch).Select(x => x.Name).FirstOrDefault();
                    mModel.lCMaster = GetLC;
                    BasicHtml = ViewHelper.RenderPartialView(this, "BasicDetailsDocView", mModel);
                }
            }
            else if (Type == "FM000" || Type == "FM000")
            {
                var GetFM = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString().Trim() == DocumentKey.ToString().Trim() ).FirstOrDefault();
                if (GetFM == null)
                {
                    Status = "Error";
                    Message = "Freight Memo Not Found...!";
                }
                else
                {
                    GetFM.FromBranch = ctxTFAT.TfatBranch.Where(x => x.Code == GetFM.FromBranch).Select(x => x.Name).FirstOrDefault();
                    GetFM.ToBranch = ctxTFAT.TfatBranch.Where(x => x.Code == GetFM.ToBranch).Select(x => x.Name).FirstOrDefault();
                    GetFM.BroCode = ctxTFAT.Master.Where(x => x.Code == GetFM.BroCode).Select(x => x.Name).FirstOrDefault();
                    mModel.fMMaster = GetFM;
                    BasicHtml = ViewHelper.RenderPartialView(this, "BasicDetailsDocView", mModel);
                }
            }
            else if (Type == "SLR00" || Type == "SLW00" || Type == "CMM00")
            {
                string Srl = DocNo;
                var result1 = ctxTFAT.DocTypes.Where(x => x.Code.ToLower() == Type.ToLower().Trim()).Select(x => x).FirstOrDefault();
                Srl = Srl.PadLeft(result1.DocWidth, '0');

                var mlist = ctxTFAT.Sales.Where(x => x.TableKey.ToString().Trim() == DocumentKey.Trim() && x.Type.Trim() == Type.Trim() ).FirstOrDefault();
                if (mlist != null)
                {
                    if (Type == "CMM00")
                    {
                        mlist.Code = ctxTFAT.Master.Where(x => x.Code == mlist.CashBankCode).Select(x => x.Name).FirstOrDefault();
                    }
                    else
                    {
                        mlist.Code = ctxTFAT.CustomerMaster.Where(x => x.Code == mlist.Code).Select(x => x.Name).FirstOrDefault();
                    }
                    mlist.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Branch).Select(x => x.Name).FirstOrDefault();
                    mModel.sales = mlist;
                    BasicHtml = ViewHelper.RenderPartialView(this, "BasicDetailsDocView", mModel);
                }
                else
                {
                    Status = "Error";
                    Message = "Bill Not Found...!";
                }
            }
            else if (Type == "PUR00")
            {
                string Srl = DocNo;
                var result1 = ctxTFAT.DocTypes.Where(x => x.Code.ToLower() == Type.ToLower().Trim()).Select(x => x).FirstOrDefault();
                Srl = Srl.PadLeft(result1.DocWidth, '0');
                var NewInteger = Convert.ToInt32(Srl).ToString();
                var mlist = ctxTFAT.Purchase.Where(x => x.TableKey.ToString().Trim() == DocumentKey.Trim() && x.Type.Trim() == Type.Trim() ).FirstOrDefault();
                if (mlist != null)
                {
                    mlist.Code = ctxTFAT.Master.Where(x => x.Code == mlist.Code).Select(x => x.Name).FirstOrDefault();
                    mlist.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Branch).Select(x => x.Name).FirstOrDefault();
                    mModel.purchase = mlist;
                    BasicHtml = ViewHelper.RenderPartialView(this, "BasicDetailsDocView", mModel);
                }
                else
                {
                    Status = "Error";
                    Message = "Bill Not Found...!";
                }
            }

            var jsonResult = Json(new { Status = Status, Message = Message, BasicHtml = BasicHtml }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public void SaveAttachment(AttachmentVM Model)
        {
            var RemoveAttach = ctxTFAT.Attachment.Where(x => x.Type == "Alert" && x.Srl == Model.Srl).ToList();
            foreach (var item in RemoveAttach)
            {
                if (System.IO.File.Exists(item.FilePath))
                {
                    System.IO.File.Delete(item.FilePath);
                }
            }
            ctxTFAT.Attachment.RemoveRange(RemoveAttach);

            List<AttachmentVM> DocList = new List<AttachmentVM>();
            if (Session["TempAttach"] != null)
            {
                DocList = (List<AttachmentVM>)Session["TempAttach"];
            }

            if (DocList != null && DocList.Count != 0)
            {
                var AttachCode = GetNewAttachCode();

                foreach (var item in DocList.ToList())
                {
                    Directory.CreateDirectory(attachmentPath + Model.ParentKey);
                    string directoryPath = attachmentPath + Model.ParentKey + "/" + item.FileName;
                    byte[] IData = Convert.FromBase64String(item.ImageStr);
                    Attachment att = new Attachment();
                    att.AUTHIDS = String.IsNullOrEmpty(item.ENTEREDBY) == true ? muserid : item.ENTEREDBY;
                    att.DocDate = ConvertDDMMYYTOYYMMDD(item.DocDate);
                    att.AUTHORISE = mauthorise;
                    att.Branch = mbranchcode;
                    att.Code = AttachCode.ToString();
                    att.ENTEREDBY = String.IsNullOrEmpty(item.ENTEREDBY) == true ? muserid : item.ENTEREDBY;
                    att.FilePath = directoryPath;
                    att.LASTUPDATEDATE = DateTime.Now;
                    att.LocationCode = Model.LocationCode;
                    att.Prefix = mperiod;
                    att.Sno = item.tempId;
                    att.Srl = Model.Srl;
                    att.SrNo = item.tempId;
                    att.TableKey = "Alert" + mperiod.Substring(0, 2) + item.tempId.ToString("D3") + Model.Srl;
                    att.Type = String.IsNullOrEmpty(item.Type) == true ? "Alert" : item.Type;
                    att.ParentKey = Model.ParentKey;

                    att.CompCode = mcompcode;
                    att.ExternalAttach = item.ExternalAttach;
                    att.RefDocNo = item.RefCode;
                    att.RefType = item.RefType;
                    ctxTFAT.Attachment.Add(att);
                    ctxTFAT.SaveChanges();
                    System.IO.File.WriteAllBytes(directoryPath, IData);

                    ++AttachCode;
                }
            }
        }
        #endregion

        #region Common Show Alert
        public ActionResult ShoWAlertNoteList(string Type, List<string> TypeCode, string DocTpe)
        {
            string LoadingMessage = "", Status = "Error";
            bool Stop = false;

            var AlertSetup = ctxTFAT.tfatAlertNoteSetup.FirstOrDefault();
            List<AlertNoteVM> Mobj = new List<AlertNoteVM>();
            List<AlertNoteMaster> objledgerdetail = new List<AlertNoteMaster>();
            if (Session["CommnNarrlist"] != null)
            {
                objledgerdetail = (List<AlertNoteMaster>)Session["CommnNarrlist"];
            }


            if (TypeCode == null)
            {
                TypeCode = new List<string>();
                TypeCode.Add("");
            }
            
            if (Type == "LR000" && DocTpe == "LR000")
            {
                Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && TypeCode.Contains(AlertMater.ParentKey)
                        orderby AlertMater.DocNo
                        select new AlertNoteVM()
                        {
                            Type = DocTpe,
                            ENTEREDBY = AlertMater.CreateBy,
                            RefType = AlertMater.RefType,
                            Branch = ctxTFAT.TfatBranch.Where(x => x.Code == AlertMater.Branch).Select(x => x.Name).FirstOrDefault(),
                            TypeCode = AlertMater.TypeCode,
                            Remark = AlertMater.Note,
                            DocNo = AlertMater.DocNo,
                            DocDate = AlertMater.DocDate,
                            DocReceived = AlertMater.TableKey,
                            DocReceivedN = AlertMater.ParentKey,
                            Bling = AlertMater.Bling,
                        }).ToList();
                foreach (var item1 in Mobj)
                {
                    Status = "Success";
                    item1.attachments = GetAttachmentListInEdit(item1.DocNo);
                }

            }
            else if (Type == "LR000" && DocTpe == "LC000")
            {
                Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && TypeCode.Contains(AlertMater.ParentKey) && AlertMater.RefType.Contains(DocTpe)
                        orderby AlertMater.DocNo
                        select new AlertNoteVM()
                        {
                            Type = DocTpe,
                            ENTEREDBY = AlertMater.CreateBy,
                            RefType = AlertMater.RefType,
                            Branch = ctxTFAT.TfatBranch.Where(x => x.Code == AlertMater.Branch).Select(x => x.Name).FirstOrDefault(),
                            TypeCode = AlertMater.TypeCode,
                            Remark = AlertMater.Note,
                            DocNo = AlertMater.DocNo,
                            DocDate = AlertMater.DocDate,
                            DocReceived = AlertMater.TableKey,
                            DocReceivedN = AlertMater.ParentKey,
                            Bling = AlertMater.Bling,
                        }).ToList();
                foreach (var item1 in Mobj)
                {
                    Status = "Success";
                    var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item1.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                    foreach (var stp in Activirty)
                    {
                        if (DocTpe.Trim() == stp.Trim())
                        {
                            var ConsignmentBookDate = ctxTFAT.LRMaster.Where(x => x.TableKey == item1.DocReceivedN).Select(x => x.BookDate).FirstOrDefault();
                            item1.Stop = true;
                            item1.LoadingMessage += item1.TypeCode + " Consignment Stopped For The Lorry Challan And Consignment Book Date Was "+ConsignmentBookDate.ToShortDateString()+" .\nSo We Cannot Allow This Consignment In Current Lorry Challan Please Remove It....!\n";
                            break;
                        }
                    }
                    item1.attachments = GetAttachmentListInEdit(item1.DocNo);
                }
            }
            else if (Type == "LR000" && DocTpe == "LOD00")
            {
                List<AlertNoteVM> NewMobj = new List<AlertNoteVM>();
                foreach (var item in TypeCode)
                {
                    var LRListOfLC = ctxTFAT.LCDetail.Where(x => x.LCRefTablekey.ToString() == item).ToList();
                    foreach (var LR in LRListOfLC)
                    {
                        LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey == LR.LRRefTablekey).FirstOrDefault();
                        if (lRMaster != null)
                        {
                            Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                                    where AlertMater.Type == Type && AlertMater.ParentKey == lRMaster.TableKey && AlertMater.RefType.Contains(DocTpe)
                                    orderby AlertMater.DocNo
                                    select new AlertNoteVM()
                                    {
                                        Type = DocTpe,
                                        ENTEREDBY = AlertMater.CreateBy,
                                        RefType = AlertMater.RefType,
                                        Branch = ctxTFAT.TfatBranch.Where(x => x.Code == AlertMater.Branch).Select(x => x.Name).FirstOrDefault(),
                                        TypeCode = AlertMater.TypeCode,
                                        Remark = AlertMater.Note,
                                        DocNo = AlertMater.DocNo,
                                        DocDate = AlertMater.DocDate,
                                        DocReceived = AlertMater.TableKey,
                                        DocReceivedN = AlertMater.ParentKey,
                                        Bling = AlertMater.Bling,
                                    }).ToList();
                            foreach (var item1 in Mobj)
                            {
                                Status = "Success";
                                var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item1.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                                foreach (var stp in Activirty)
                                {
                                    if (DocTpe.Trim() == stp.Trim())
                                    {
                                        item1.Stop = true;
                                        item1.LoadingMessage += item1.TypeCode + " Consignment Stopped For Loading And This Consignment Include In Lorry Challan No " + LR.LCno + " And Respective Date Was : "+LR.Date.ToShortDateString()+". So U Cannot Load This Lorry Challan....!\n";
                                        break;
                                    }
                                }
                                item1.attachments = GetAttachmentListInEdit(item1.DocNo);
                            }

                            NewMobj.AddRange(Mobj);
                        }
                    }
                }
                Mobj = NewMobj;
            }
            else if (Type == "LR000" && DocTpe == "UNLOD")
            {
                List<AlertNoteVM> NewMobj = new List<AlertNoteVM>();
                foreach (var item in TypeCode)
                {
                    LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey == item).FirstOrDefault();
                    LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey == lRStock.LRRefTablekey).FirstOrDefault();

                    Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                            where AlertMater.Type == Type && AlertMater.ParentKey== lRStock.LRRefTablekey && AlertMater.RefType.Contains(DocTpe)
                            orderby AlertMater.DocNo
                            select new AlertNoteVM()
                            {
                                Type = DocTpe,
                                ENTEREDBY = AlertMater.CreateBy,
                                RefType = AlertMater.RefType,
                                Branch = ctxTFAT.TfatBranch.Where(x => x.Code == AlertMater.Branch).Select(x => x.Name).FirstOrDefault(),
                                TypeCode = AlertMater.TypeCode,
                                Remark = AlertMater.Note,
                                DocNo = AlertMater.DocNo,
                                DocDate = AlertMater.DocDate,
                                DocReceived = AlertMater.TableKey,
                                DocReceivedN = AlertMater.ParentKey,
                                Bling = AlertMater.Bling,
                            }).ToList();
                    foreach (var item1 in Mobj)
                    {
                        Status = "Success";
                        var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item1.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                        foreach (var stp in Activirty)
                        {
                            if (DocTpe.Trim() == stp.Trim())
                            {
                                item1.Stop = true;
                                item1.LoadingMessage += item1.TypeCode + " Consignment Stopped For The Unloading And This Consignment Booked Date Was  "+lRMaster.BookDate.ToShortDateString()+" .\nSo We Cannot Allow This Consignment To Unload....!\n";
                                break;
                            }
                        }
                        item1.attachments = GetAttachmentListInEdit(item1.DocNo);
                    }
                    NewMobj.AddRange(Mobj);
                }
                Mobj = NewMobj;
            }
            else if (Type == "LR000" && DocTpe == "DELV0")
            {
                var GetConsignmentKey = ctxTFAT.LRStock.Where(x => TypeCode.Contains(x.TableKey)).Select(x => x.LRRefTablekey).ToList();

                Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && GetConsignmentKey.Contains(AlertMater.ParentKey) && AlertMater.RefType.Contains(DocTpe)
                        orderby AlertMater.DocNo
                        select new AlertNoteVM()
                        {
                            Type = DocTpe,
                            ENTEREDBY = AlertMater.CreateBy,
                            RefType = AlertMater.RefType,
                            Branch = ctxTFAT.TfatBranch.Where(x => x.Code == AlertMater.Branch).Select(x => x.Name).FirstOrDefault(),
                            TypeCode = AlertMater.TypeCode,
                            Remark = AlertMater.Note,
                            DocNo = AlertMater.DocNo,
                            DocDate = AlertMater.DocDate,
                            DocReceived = AlertMater.TableKey,
                            DocReceivedN = AlertMater.ParentKey,
                            Bling = AlertMater.Bling,
                        }).ToList();
                foreach (var item1 in Mobj)
                {
                    Status = "Success";
                    var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item1.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                    foreach (var stp in Activirty)
                    {
                        if (DocTpe.Trim() == stp.Trim())
                        {
                            var ConsignmentBookDate = ctxTFAT.LRMaster.Where(x => x.TableKey == item1.DocReceivedN).Select(x => x.BookDate).FirstOrDefault();
                            item1.Stop = true;
                            item1.LoadingMessage += item1.TypeCode + " Consignment Stopped For The Delivery And This Consignment Booked Date Was  "+ ConsignmentBookDate.ToShortDateString() + " .\nSo We Cannot Allow This Consignment To Deliverd....!\n";
                            break;
                        }
                    }
                    item1.attachments = GetAttachmentListInEdit(item1.DocNo);
                }
            }
            else if (Type == "LR000" && DocTpe == "SLR00")
            {
                Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && TypeCode.Contains(AlertMater.ParentKey) && AlertMater.RefType.Contains(DocTpe)
                        orderby AlertMater.DocNo
                        select new AlertNoteVM()
                        {
                            Type = DocTpe,
                            ENTEREDBY = AlertMater.CreateBy,
                            RefType = AlertMater.RefType,
                            Branch = ctxTFAT.TfatBranch.Where(x => x.Code == AlertMater.Branch).Select(x => x.Name).FirstOrDefault(),
                            TypeCode = AlertMater.TypeCode,
                            Remark = AlertMater.Note,
                            DocNo = AlertMater.DocNo,
                            DocDate = AlertMater.DocDate,
                            DocReceived = AlertMater.TableKey,
                            DocReceivedN = AlertMater.ParentKey,
                            Bling = AlertMater.Bling,
                        }).ToList();
                foreach (var item1 in Mobj)
                {
                    Status = "Success";
                    var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item1.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                    foreach (var stp in Activirty)
                    {
                        if (DocTpe.Trim() == stp.Trim())
                        {
                            var ConsignmentBookDate = ctxTFAT.LRMaster.Where(x => x.TableKey == item1.DocReceivedN).Select(x => x.BookDate).FirstOrDefault();
                            item1.Stop = true;
                            item1.LoadingMessage += item1.TypeCode + " Consignment Stopped For The Freight Bill And This Consignment Booked Date Was "+ ConsignmentBookDate.ToShortDateString() + " .\nSo We Cannot Allow This Consignment To Freight Bill Please Remove It....!\n";
                            break;
                        }
                    }
                    item1.attachments = GetAttachmentListInEdit(item1.DocNo);
                }
            }
            else if (Type == "LR000" && DocTpe == "CMM00")
            {
                Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && TypeCode.Contains(AlertMater.ParentKey) && AlertMater.RefType.Contains(DocTpe)
                        orderby AlertMater.DocNo
                        select new AlertNoteVM()
                        {
                            Type = DocTpe,
                            ENTEREDBY = AlertMater.CreateBy,
                            RefType = AlertMater.RefType,
                            Branch = ctxTFAT.TfatBranch.Where(x => x.Code == AlertMater.Branch).Select(x => x.Name).FirstOrDefault(),
                            TypeCode = AlertMater.TypeCode,
                            Remark = AlertMater.Note,
                            DocNo = AlertMater.DocNo,
                            DocDate = AlertMater.DocDate,
                            DocReceived = AlertMater.TableKey,
                            DocReceivedN = AlertMater.ParentKey,
                            Bling = AlertMater.Bling,
                        }).ToList();
                foreach (var item1 in Mobj)
                {
                    Status = "Success";
                    var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item1.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                    foreach (var stp in Activirty)
                    {
                        if (DocTpe.Trim() == stp.Trim())
                        {
                            var ConsignmentBookDate = ctxTFAT.LRMaster.Where(x => x.TableKey == item1.DocReceivedN).Select(x => x.BookDate).FirstOrDefault();
                            item1.Stop = true;
                            item1.LoadingMessage += item1.TypeCode + " Consignment Stopped For The Cash Sale And This Consignment Booked Date Was"+ ConsignmentBookDate.ToShortDateString() + ".\nSo We Cannot Allow This Consignment To Cash Sale Please Remove It....!\n";
                            break;
                        }
                    }
                    item1.attachments = GetAttachmentListInEdit(item1.DocNo);
                }
            }
            else if (Type == "LR000" && DocTpe == "POD00")
            {
                var GetConsignmentKey = ctxTFAT.LRStock.Where(x => TypeCode.Contains(x.TableKey)).Select(x => x.LRRefTablekey).ToList();

                Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && GetConsignmentKey.Contains(AlertMater.ParentKey) && AlertMater.RefType.Contains(DocTpe)
                        orderby AlertMater.DocNo
                        select new AlertNoteVM()
                        {
                            Type = DocTpe,
                            ENTEREDBY = AlertMater.CreateBy,
                            RefType = AlertMater.RefType,
                            Branch = ctxTFAT.TfatBranch.Where(x => x.Code == AlertMater.Branch).Select(x => x.Name).FirstOrDefault(),
                            TypeCode = AlertMater.TypeCode,
                            Remark = AlertMater.Note,
                            DocNo = AlertMater.DocNo,
                            DocDate = AlertMater.DocDate,
                            DocReceived = AlertMater.TableKey,
                            DocReceivedN = AlertMater.ParentKey,
                            Bling = AlertMater.Bling,
                        }).ToList();
                foreach (var item1 in Mobj)
                {
                    Status = "Success";
                    var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item1.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                    foreach (var stp in Activirty)
                    {
                        if (DocTpe.Trim() == stp.Trim())
                        {
                            var ConsignmentBookDate = ctxTFAT.LRMaster.Where(x => x.TableKey == item1.DocReceivedN).Select(x => x.BookDate).FirstOrDefault();
                            item1.Stop = true;
                            item1.LoadingMessage += item1.TypeCode + " Consignment Stopped For The POD And This Consignment Booked Date Was" + ConsignmentBookDate.ToShortDateString() + " .\nSo We Cannot Allow This Consignment To POD Please Remove It....!\n";
                            break;
                        }
                    }
                    item1.attachments = GetAttachmentListInEdit(item1.DocNo);
                }
            }
            else if (Type == "LC000" && DocTpe == "LC000")
            {
                Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && TypeCode.Contains(AlertMater.ParentKey)
                        orderby AlertMater.DocNo
                        select new AlertNoteVM()
                        {
                            Type = DocTpe,
                            ENTEREDBY = AlertMater.CreateBy,
                            RefType = AlertMater.RefType,
                            Branch = ctxTFAT.TfatBranch.Where(x => x.Code == AlertMater.Branch).Select(x => x.Name).FirstOrDefault(),
                            TypeCode = AlertMater.TypeCode,
                            Remark = AlertMater.Note,
                            DocNo = AlertMater.DocNo,
                            DocDate = AlertMater.DocDate,
                            DocReceived = AlertMater.TableKey,
                            DocReceivedN = AlertMater.ParentKey,
                            Bling = AlertMater.Bling,
                        }).ToList();
                foreach (var item1 in Mobj)
                {
                    Status = "Success";
                    var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item1.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                    foreach (var stp in Activirty)
                    {
                        if (DocTpe.Trim() == stp.Trim())
                        {
                            item1.Stop = true;
                            item1.LoadingMessage += item1.TypeCode + " Lorry Challan Stopped .\nSo We Cannot Allow This Lorry Challan....!\n";
                            break;
                        }
                    }
                    item1.attachments = GetAttachmentListInEdit(item1.DocNo);
                }
            }
            else if ((Type == "FM000" && DocTpe == "FM000") || (Type == "FM000" && DocTpe == "FM000"))
            {
                Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && TypeCode.Contains(AlertMater.ParentKey)
                        orderby AlertMater.DocNo
                        select new AlertNoteVM()
                        {
                            Type = DocTpe,
                            ENTEREDBY = AlertMater.CreateBy,
                            RefType = AlertMater.RefType,
                            Branch = ctxTFAT.TfatBranch.Where(x => x.Code == AlertMater.Branch).Select(x => x.Name).FirstOrDefault(),
                            TypeCode = AlertMater.TypeCode,
                            Remark = AlertMater.Note,
                            DocNo = AlertMater.DocNo,
                            DocDate = AlertMater.DocDate,
                            DocReceived = AlertMater.TableKey,
                            DocReceivedN = AlertMater.ParentKey,
                            Bling = AlertMater.Bling,
                        }).ToList();
                foreach (var item1 in Mobj)
                {
                    Status = "Success";
                    item1.attachments = GetAttachmentListInEdit(item1.DocNo);
                }
            }
            else if ((Type == "FM000" && DocTpe == "FMP00") || (Type == "FM000" && DocTpe == "FMP00"))
            {
                var ledger = ctxTFAT.Ledger.Where(x => TypeCode.Contains(x.TableKey)).FirstOrDefault();
                var LedgerKeyList = ctxTFAT.Ledger.Where(x => x.ParentKey == ledger.ParentKey).Select(x => x.TableKey).ToList();


                Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && LedgerKeyList.Contains(AlertMater.ParentKey) && AlertMater.RefType.Contains(DocTpe)
                        orderby AlertMater.DocNo
                        select new AlertNoteVM()
                        {
                            Type = DocTpe,
                            ENTEREDBY = AlertMater.CreateBy,
                            RefType = AlertMater.RefType,
                            Branch = ctxTFAT.TfatBranch.Where(x => x.Code == AlertMater.Branch).Select(x => x.Name).FirstOrDefault(),
                            TypeCode = AlertMater.TypeCode,
                            Remark = AlertMater.Note,
                            DocNo = AlertMater.DocNo,
                            DocDate = AlertMater.DocDate,
                            DocReceived = AlertMater.TableKey,
                            DocReceivedN = AlertMater.ParentKey,
                            Bling = AlertMater.Bling,
                        }).ToList();
                foreach (var item1 in Mobj)
                {
                    Status = "Success";
                    var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item1.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                    foreach (var stp in Activirty)
                    {
                        if (DocTpe.Trim() == stp.Trim())
                        {
                            var ConsignmentBookDate = ctxTFAT.FMMaster.Where(x => x.TableKey == item1.DocReceivedN).Select(x => x.Date).FirstOrDefault();
                            item1.Stop = true;
                            item1.LoadingMessage += item1.TypeCode + " Freight Memo Stopped For Payment And This Freight Memo Booked Date Was" + ConsignmentBookDate.ToShortDateString() + ".\nSo We Cannot Allow This Freight Memo Please Remove It...!\n";
                            break;
                        }
                    }
                    item1.attachments = GetAttachmentListInEdit(item1.DocNo);
                }

            }
            else if ((Type == "FM000" && DocTpe == "ACTVT") || (Type == "FM000" && DocTpe == "ACTVT"))
            {

                FMROUTETable fMROUTETable = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == TypeCode.FirstOrDefault()).FirstOrDefault();

                Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && AlertMater.ParentKey== fMROUTETable.Parentkey && AlertMater.RefType.Contains(DocTpe)
                        orderby AlertMater.DocNo
                        select new AlertNoteVM()
                        {
                            Type = DocTpe,
                            ENTEREDBY = AlertMater.CreateBy,
                            RefType = AlertMater.RefType,
                            Branch = ctxTFAT.TfatBranch.Where(x => x.Code == AlertMater.Branch).Select(x => x.Name).FirstOrDefault(),
                            TypeCode = AlertMater.TypeCode,
                            Remark = AlertMater.Note,
                            DocNo = AlertMater.DocNo,
                            DocDate = AlertMater.DocDate,
                            DocReceived = AlertMater.TableKey,
                            DocReceivedN = AlertMater.ParentKey,
                            Bling = AlertMater.Bling,
                        }).ToList();
                foreach (var item1 in Mobj)
                {
                    Status = "Success";
                    var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item1.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                    foreach (var stp in Activirty)
                    {
                        if (DocTpe.Trim() == stp.Trim())
                        {
                            item1.Stop = true;
                            item1.LoadingMessage += item1.TypeCode + " Freight Memo Activity Stopped.\nSo We Cannot Allow Any Activity Against This Freight Memo...!\n";
                            break;
                        }
                    }
                    item1.attachments = GetAttachmentListInEdit(item1.DocNo);
                }
            }
            else if (Type == "SLR00" && DocTpe == "SLR00")
            {
                Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && TypeCode.Contains(AlertMater.ParentKey)
                        orderby AlertMater.DocNo
                        select new AlertNoteVM()
                        {
                            Type = DocTpe,
                            ENTEREDBY = AlertMater.CreateBy,
                            RefType = AlertMater.RefType,
                            Branch = ctxTFAT.TfatBranch.Where(x => x.Code == AlertMater.Branch).Select(x => x.Name).FirstOrDefault(),
                            TypeCode = AlertMater.TypeCode,
                            Remark = AlertMater.Note,
                            DocNo = AlertMater.DocNo,
                            DocDate = AlertMater.DocDate,
                            DocReceived = AlertMater.TableKey,
                            DocReceivedN = AlertMater.ParentKey,
                            Bling = AlertMater.Bling,
                        }).ToList();
                foreach (var item1 in Mobj)
                {
                    Status = "Success";
                    var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item1.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                    foreach (var stp in Activirty)
                    {
                        if (DocTpe.Trim() == stp.Trim())
                        {
                            item1.Stop = true;
                            item1.LoadingMessage += item1.TypeCode + " Freight Bill Stopped .\nSo We Cannot Allow This Freight Bill....!\n";
                            break;
                        }
                    }
                    item1.attachments = GetAttachmentListInEdit(item1.DocNo);
                }
            }
            else if (Type == "SLR00" && DocTpe == "BLSMT")
            {
                Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && TypeCode.Contains(AlertMater.ParentKey)
                        orderby AlertMater.DocNo
                        select new AlertNoteVM()
                        {
                            Type = DocTpe,
                            ENTEREDBY = AlertMater.CreateBy,
                            RefType = AlertMater.RefType,
                            Branch = ctxTFAT.TfatBranch.Where(x => x.Code == AlertMater.Branch).Select(x => x.Name).FirstOrDefault(),
                            TypeCode = AlertMater.TypeCode,
                            Remark = AlertMater.Note,
                            DocNo = AlertMater.DocNo,
                            DocDate = AlertMater.DocDate,
                            DocReceived = AlertMater.TableKey,
                            DocReceivedN = AlertMater.ParentKey,
                            Bling = AlertMater.Bling,
                        }).ToList();
                foreach (var item1 in Mobj)
                {
                    Status = "Success";
                    var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item1.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                    foreach (var stp in Activirty)
                    {
                        if (DocTpe.Trim() == stp.Trim())
                        {
                            item1.Stop = true;
                            item1.LoadingMessage += item1.TypeCode + " Freight Bill Stopped For The Submission .\nSo We Cannot Allow This Freight Bill In Submission....!\n";
                            break;
                        }
                    }
                    item1.attachments = GetAttachmentListInEdit(item1.DocNo);
                }
            }
            else if (Type == "SLW00" && DocTpe == "SLW00")
            {
                Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && TypeCode.Contains(AlertMater.ParentKey)
                        orderby AlertMater.DocNo
                        select new AlertNoteVM()
                        {
                            Type = DocTpe,
                            ENTEREDBY = AlertMater.CreateBy,
                            RefType = AlertMater.RefType,
                            Branch = ctxTFAT.TfatBranch.Where(x => x.Code == AlertMater.Branch).Select(x => x.Name).FirstOrDefault(),
                            TypeCode = AlertMater.TypeCode,
                            Remark = AlertMater.Note,
                            DocNo = AlertMater.DocNo,
                            DocDate = AlertMater.DocDate,
                            DocReceived = AlertMater.TableKey,
                            DocReceivedN = AlertMater.ParentKey,
                            Bling = AlertMater.Bling,
                        }).ToList();
                foreach (var item1 in Mobj)
                {
                    Status = "Success";
                    var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item1.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                    foreach (var stp in Activirty)
                    {
                        if (DocTpe.Trim() == stp.Trim())
                        {
                            item1.Stop = true;
                            item1.LoadingMessage += item1.TypeCode + " Freight Bill Without Consignment Stopped .\nSo We Cannot Allow This Freight Bill Without Consignment....!\n";
                            break;
                        }
                    }
                    item1.attachments = GetAttachmentListInEdit(item1.DocNo);
                }
            }
            else if (Type == "SLW00" && DocTpe == "BLSMT")
            {
                Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && TypeCode.Contains(AlertMater.ParentKey)
                        orderby AlertMater.DocNo
                        select new AlertNoteVM()
                        {
                            Type = DocTpe,
                            ENTEREDBY = AlertMater.CreateBy,
                            RefType = AlertMater.RefType,
                            Branch = ctxTFAT.TfatBranch.Where(x => x.Code == AlertMater.Branch).Select(x => x.Name).FirstOrDefault(),
                            TypeCode = AlertMater.TypeCode,
                            Remark = AlertMater.Note,
                            DocNo = AlertMater.DocNo,
                            DocDate = AlertMater.DocDate,
                            DocReceived = AlertMater.TableKey,
                            DocReceivedN = AlertMater.ParentKey,
                            Bling = AlertMater.Bling,
                        }).ToList();
                foreach (var item1 in Mobj)
                {
                    Status = "Success";
                    var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item1.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                    foreach (var stp in Activirty)
                    {
                        if (DocTpe.Trim() == stp.Trim())
                        {
                            item1.Stop = true;
                            item1.LoadingMessage += item1.TypeCode + " Freight Bill  Without Consignment  Stopped For The Submission .\nSo We Cannot Allow This Freight Bill Without Consignment  In Submission....!\n";
                            break;
                        }
                    }
                    item1.attachments = GetAttachmentListInEdit(item1.DocNo);
                }
            }
            else if (Type == "CMM00" && DocTpe == "CMM00")
            {
                Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && TypeCode.Contains(AlertMater.ParentKey)
                        orderby AlertMater.DocNo
                        select new AlertNoteVM()
                        {
                            Type = DocTpe,
                            ENTEREDBY = AlertMater.CreateBy,
                            RefType = AlertMater.RefType,
                            Branch = ctxTFAT.TfatBranch.Where(x => x.Code == AlertMater.Branch).Select(x => x.Name).FirstOrDefault(),
                            TypeCode = AlertMater.TypeCode,
                            Remark = AlertMater.Note,
                            DocNo = AlertMater.DocNo,
                            DocDate = AlertMater.DocDate,
                            DocReceived = AlertMater.TableKey,
                            DocReceivedN = AlertMater.ParentKey,
                            Bling = AlertMater.Bling,
                        }).ToList();
                foreach (var item1 in Mobj)
                {
                    Status = "Success";
                    var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item1.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                    foreach (var stp in Activirty)
                    {
                        if (DocTpe.Trim() == stp.Trim())
                        {
                            item1.Stop = true;
                            item1.LoadingMessage += item1.TypeCode + " Cash Sale Stopped .\nSo We Cannot Allow This Cash Sale Document....!\n";
                            break;
                        }
                    }
                    item1.attachments = GetAttachmentListInEdit(item1.DocNo);
                }
            }
            else if (Type == "PUR00" && DocTpe == "PUR00")
            {
                Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && TypeCode.Contains(AlertMater.ParentKey)
                        orderby AlertMater.DocNo
                        select new AlertNoteVM()
                        {
                            Type = DocTpe,
                            ENTEREDBY = AlertMater.CreateBy,
                            RefType = AlertMater.RefType,
                            Branch = ctxTFAT.TfatBranch.Where(x => x.Code == AlertMater.Branch).Select(x => x.Name).FirstOrDefault(),
                            TypeCode = AlertMater.TypeCode,
                            Remark = AlertMater.Note,
                            DocNo = AlertMater.DocNo,
                            DocDate = AlertMater.DocDate,
                            DocReceived = AlertMater.TableKey,
                            DocReceivedN = AlertMater.ParentKey,
                            Bling = AlertMater.Bling,
                        }).ToList();
                foreach (var item1 in Mobj)
                {
                    Status = "Success";
                    var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item1.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                    foreach (var stp in Activirty)
                    {
                        if (DocTpe.Trim() == stp.Trim())
                        {
                            item1.Stop = true;
                            item1.LoadingMessage += item1.TypeCode + " Credit Purchase Stopped .\nSo We Cannot Allow This Credit Purchase Document....!\n";
                            break;
                        }
                    }
                    item1.attachments = GetAttachmentListInEdit(item1.DocNo);
                }
            }

            foreach (var item in objledgerdetail)
            {
                AlertNoteVM alertNote = new AlertNoteVM();
                alertNote.Type = item.Type;
                alertNote.ENTEREDBY = item.CreateBy;
                alertNote.RefType = item.RefType;
                alertNote.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault();
                alertNote.TypeCode = item.TypeCode;
                alertNote.Remark = item.Note;
                alertNote.DocNo = item.DocNo;
                alertNote.DocDate = item.DocDate;
                alertNote.DocReceived = item.TableKey;
                alertNote.DocReceivedN = item.ParentKey;
                alertNote.Bling = item.Bling;
                Mobj.Add(alertNote);
            }

            foreach (var item in Mobj)
            {
                Status = "Success";
                if (item.attachments == null)
                {
                    item.attachments = new List<AttachmentVM>();
                }

                if (AlertSetup != null)
                {
                    if (AlertSetup.DeleteOwnUser)
                    {
                        if (item.ENTEREDBY.Trim().ToLower() == muserid.Trim().ToLower() || "super" == muserid.Trim().ToLower())
                        {
                            item.AllowDelete = true;
                        }
                    }
                    else if (AlertSetup.DeleteAllUser)
                    {
                        item.AllowDelete = true;
                    }
                }
            }

            var html = ViewHelper.RenderPartialView(this, "ListOfAlertNoteView", Mobj);
            var jsonResult = Json(new { Status = Status, Html = html, Stop = Stop }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public List<AttachmentVM> GetAttachmentListInEdit(string TableKey)
        {
            List<AttachmentVM> AttachmentList = new List<AttachmentVM>();
            var docdetail = ctxTFAT.Attachment.Where(x => x.Type == "Alert" && x.Srl.ToLower().Trim() == TableKey.Trim().ToLower()).Take(3);
            foreach (var item in docdetail)
            {
                AttachmentVM AttachmentVM = new AttachmentVM();

                bool FoundAttachment = false;
                if (System.IO.File.Exists(item.FilePath))
                {
                    FoundAttachment = true;
                }

                if (FoundAttachment)
                {
                    AttachmentVM.FileName = Path.GetFileName(item.FilePath);
                    AttachmentVM.Srl = item.Srl;
                    AttachmentVM.Code = item.Code;
                    AttachmentVM.TableKey = item.TableKey;
                    AttachmentVM.ParentKey = item.ParentKey;
                    AttachmentVM.Type = item.Type;
                    AttachmentVM.tempId = item.Sno;
                    AttachmentVM.SrNo = item.Sno;
                    AttachmentVM.Path = item.FilePath;
                    AttachmentVM.FileContent = Path.GetExtension(item.FilePath);
                    AttachmentVM.ImageStr = Convert.ToBase64String(System.IO.File.ReadAllBytes(item.FilePath));
                    AttachmentVM.ImageData = Convert.FromBase64String(AttachmentVM.ImageStr);
                    AttachmentVM.tempIsDeleted = false;
                    AttachmentList.Add(AttachmentVM);
                }

            }


            return AttachmentList;
        }
        #endregion
    }
}