using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using Common;
using CrystalDecisions.CrystalReports.Engine;
using EntitiModel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class BillSubmissionController : BaseController
    {
        string attachmentPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentPath"].ToString()) == true ? "" : ConfigurationManager.AppSettings["AttachmentPath"].ToString();

        private static string mauthorise = "A00";
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        public string muserid = System.Web.HttpContext.Current.Session["UserId"].ToString();
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

        public JsonResult GetBillType(string term)
        {
            var list = ctxTFAT.DocTypes.Where(x => x.Code == "SLR00" || x.Code == "SLW00").ToList();

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetParty(string term)
        {
            var list = ctxTFAT.CustomerMaster.ToList();

            if (!String.IsNullOrEmpty(term))
            {
                list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }
            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Getbranch(string term)
        {
            var list = ctxTFAT.TfatBranch.Where(x => (x.Category != "Area" && x.Code != "G00000")).ToList();

            if (!String.IsNullOrEmpty(term))
            {
                list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }
            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }
        private static string mdocument = "";
        public string GetCode()
        {
            var DocNo = ctxTFAT.BillSubmission.OrderByDescending(x => x.RECORDKEY).Select(x => x.DocNo).FirstOrDefault();
            if (String.IsNullOrEmpty(DocNo))
            {
                return "100000";
            }
            else
            {
                var NewCode = Convert.ToInt32(DocNo) + 1;
                return NewCode.ToString("D6");
            }
        }
        public string GetCodeOFSend()
        {
            var DocNo = ctxTFAT.SendReceBill.OrderByDescending(x => x.RECORDKEY).Select(x => x.DocNo).FirstOrDefault();
            if (String.IsNullOrEmpty(DocNo))
            {
                DocNo = ctxTFAT.BillSubmission.OrderByDescending(x => x.RECORDKEY).Select(x => x.DocNo).FirstOrDefault();
                if (String.IsNullOrEmpty(DocNo))
                {
                    return "100000";
                }
                else
                {
                    var NewCode = Convert.ToInt32(DocNo) + 1;
                    return NewCode.ToString("D6");
                }
            }
            else
            {
                var NewCode = Convert.ToInt32(DocNo) + 1;
                return NewCode.ToString("D6");
            }
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
            return GetGridDataColumns(id, "L", "XXXX");
        }
        //All Stock
        public ActionResult GetGridData(GridOption Model)
        {
            mpara = "";
            if (ctxTFAT.TfatBranch.Where(x => x.Code == Model.Code).FirstOrDefault() != null)
            {
                List<BillDetails> objledgerdetail = new List<BillDetails>();
                if (Session["BillDetails"] != null)
                {
                    objledgerdetail = (List<BillDetails>)Session["BillDetails"];
                }
                var ExistBillList = objledgerdetail.Select(x => x.BillTableKey).ToList();
                var Para1 = string.Join(",", ExistBillList);
                mpara += "para01" + "^" + Para1 + "~";

                var Receivedbranchlist = GetChildGrp(mbranchcode);
                var Sendbranchlist = GetChildGrp(Model.Code);

                var GetDocumentlist = ctxTFAT.SendReceBill.Where(x => x.DocType == "Send" && Receivedbranchlist.Contains(x.FTBranch) && Sendbranchlist.Contains(x.Branch)).Select(x => x.DocNo).ToList();
                var Para3 = string.Join(",", GetDocumentlist);
                mpara += "para03" + "^" + Para3 + "~";
                var GetReceivedBillNoList = (from SendReceBill in ctxTFAT.SendReceBill
                                             join SendReceBillRef in ctxTFAT.SendReceBillRef on SendReceBill.DocNo equals SendReceBillRef.DocNo
                                             where Receivedbranchlist.Contains(SendReceBill.Branch) && SendReceBill.DocType == "Received"
                                             select new { SendReceBill.DocNo, SendReceBillRef.BillTableKey }).ToList();
                var ReceivedBillNo = GetReceivedBillNoList.Select(x => x.BillTableKey).ToList();
                var Para2 = string.Join(",", ReceivedBillNo);
                mpara += "para02" + "^" + Para2 + "~";


            }
            else
            {
                List<BillDetails> objledgerdetail = new List<BillDetails>();
                if (Session["BillDetails"] != null)
                {
                    objledgerdetail = (List<BillDetails>)Session["BillDetails"];
                }
                var ExistBillList = objledgerdetail.Select(x => x.BillTableKey).ToList();
                var Para1 = string.Join(",", ExistBillList);
                mpara = "para01" + "^" + Para1 + "~";
                Model.Code = ctxTFAT.CustomerMaster.Where(x => x.Code == Model.Code).Select(x => x.AccountParentGroup).FirstOrDefault();
            }
            return GetGridReport(Model, "L", "Code^" + Model.Code + (mpara != "" ? "~" + mpara : ""), false, 0);
        }

        // GET: Logistics/BillSubmission
        public ActionResult Index(GridOption Model)
        {


            Model.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
            Model.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "NA");

            if (Model.Document == null)
            {
                Model.Document = "";
                Model.AccountName = "";
            }
            moptioncode = Model.OptionCode;
            msubcodeof = Model.ViewDataId;
            mmodule = Model.Module;
            ViewBag.id = Model.ViewDataId;
            ViewBag.ViewDataId = Model.ViewDataId;
            ViewBag.Header = Model.Header;
            ViewBag.Table = Model.TableName;
            ViewBag.Controller = Model.Controller;
            ViewBag.MainType = Model.MainType;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;
            ViewBag.ViewName = Model.ViewName;
            if (Model.AcType != null)
            {
                ViewBag.MainType = Model.AcType;
            }
            TfatMenu tfatMenu = ctxTFAT.TfatMenu.Where(z => z.OptionCode == moptioncode && z.ModuleName == mmodule).FirstOrDefault();
            if (muserid.ToUpper() == "SUPER")
            {
                Model.xAdd = true;
                Model.xDelete = true;
                Model.xEdit = true;
                Model.xPrint = true;
            }
            else
            {
                var result = ctxTFAT.UserRights.Where(z => z.MenuID == tfatMenu.ID && z.ModuleName == mmodule && z.Code == muserid).FirstOrDefault();

                if (result != null)
                {
                    Model.xAdd = result.xAdd;
                    Model.xDelete = result.xDelete;
                    Model.xEdit = result.xEdit;
                    Model.xPrint = result.xPrint;
                }
            }

            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            return View(Model);
        }
        public ActionResult DirectSubMission(BillSubmissionVM mModel)
        {
            Session["TempAttach"] = null;
            Session["BillDetails"] = null;
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;
            mModel.Methods = "Direct";
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var BillSubmission = ctxTFAT.BillSubmission.Where(x => x.DocNo == mModel.Document).FirstOrDefault();
                var billSubRef = ctxTFAT.BillSubRef.Where(x => x.DocNo == mModel.Document).FirstOrDefault();
                mModel.DocumentNo = mModel.Document;
                mModel.BillDetail = new BillDetails();
                mModel.BillType = "Direct";
                mModel.BillDetail.BillType = billSubRef.BillType;
                mModel.BillDetail.BillTableKey = billSubRef.BillTableKey;
                mModel.BillDetail.BillParentKey = ctxTFAT.Ledger.Where(x => x.TableKey == billSubRef.BillTableKey).Select(x => x.ParentKey).FirstOrDefault();
                mModel.BillDetail.BillBranch = billSubRef.BillBranch;
                mModel.BillDetail.BillBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == billSubRef.BillBranch).Select(x => x.Name).FirstOrDefault();
                mModel.BillDetail.BillTypeName = ctxTFAT.DocTypes.Where(x => x.Code == billSubRef.BillType).Select(x => x.Name).FirstOrDefault();
                mModel.BillDetail.BillNo = billSubRef.BillNo;
                mModel.SubmitDate = BillSubmission.SubDt.ToShortDateString();
                mModel.DocDate = BillSubmission.DocDate.ToShortDateString();
                mModel.Through = BillSubmission.Through;
                mModel.Remark = BillSubmission.Remark;
                mModel.Party = BillSubmission.Party;
                mModel.PartyName = ctxTFAT.CustomerMaster.Where(x => x.Code == BillSubmission.Party).Select(x => x.Name).FirstOrDefault();
                mModel.BillDetail.Date = billSubRef.BillDate;
                mModel.BillDetail.Amount = billSubRef.Amount.ToString();
                mModel.PartyParent = billSubRef.PartyGroup;
                mModel.PartyParentName = ctxTFAT.Master.Where(x => x.Code == billSubRef.PartyGroup).Select(x => x.Name).FirstOrDefault();
                mModel.Branch = BillSubmission.Branch;
            }
            else
            {
                mModel.BillDetail = new BillDetails();
                mModel.DocumentNo = GetCode();
                mModel.BillType = "Direct";
                mModel.SubmitDate = DateTime.Now.ToShortDateString();
                mModel.DocDate = DateTime.Now.ToShortDateString();
                mModel.Through = "By Hand";
                mModel.BillDetail.BillType = "SLR00";
                mModel.BillDetail.BillTypeName = ctxTFAT.DocTypes.Where(x => x.Code == "SLR00").Select(x => x.Name).FirstOrDefault();
                mModel.Branch = mbranchcode;
            }
            return View(mModel);
        }
        public ActionResult PartySubmission(BillSubmissionVM mModel)
        {
            Session["TempAttach"] = null;
            Session["BillDetails"] = null;
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;
            var Setup = ctxTFAT.BillSubmissionSetup.FirstOrDefault();
            if (Setup != null)
            {
                mModel.GlobalSearch = Setup.GlobalSearch;
            }
            mModel.Methods = "Party";
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var BillSubmission = ctxTFAT.BillSubmission.Where(x => x.DocNo == mModel.Document).FirstOrDefault();
                mModel.Branch = BillSubmission.Branch;
                mModel.DocumentNo = mModel.Document;
                mModel.Party = BillSubmission.Party;
                mModel.PartyName = ctxTFAT.CustomerMaster.Where(x => x.Code == BillSubmission.Party).Select(x => x.Name).FirstOrDefault();
                mModel.SubmitDate = BillSubmission.SubDt.ToShortDateString();
                mModel.DocDate = BillSubmission.DocDate.ToShortDateString();
                mModel.Through = BillSubmission.Through;
                mModel.Remark = BillSubmission.Remark;
                mModel.BillDetail = new BillDetails();
                mModel.BillDetail.BillType = "";
                mModel.BillDetail.BillTypeName = ctxTFAT.SubTypes.Where(x => x.Code == "").Select(x => x.Name).FirstOrDefault();
                mModel.BillDetails = new List<BillDetails>();
                var mobj = (from BillSubRef in ctxTFAT.BillSubRef
                            where BillSubRef.DocNo == mModel.Document
                            orderby BillSubRef.BillNo
                            select new BillDetails()
                            {
                                BillNo = BillSubRef.BillNo,
                                BillType = BillSubRef.BillType,
                                BillTypeName = ctxTFAT.DocTypes.Where(x => x.Code == BillSubRef.BillType).Select(x => x.Name).FirstOrDefault(),
                                PartyParent = BillSubRef.PartyGroup,
                                PartyParentName = ctxTFAT.Master.Where(x => x.Code == BillSubRef.PartyGroup).Select(x => x.Name).FirstOrDefault(),
                                Amount = BillSubRef.Amount.ToString(),
                                Date = BillSubRef.BillDate,
                                BillParty = mModel.Party,
                                BillBranchN = mModel.PartyName,
                                BillBranch = BillSubRef.BillBranch,
                                BillTableKey = BillSubRef.BillTableKey,
                                BillPartyName = ctxTFAT.TfatBranch.Where(x => x.Code == BillSubRef.BillBranch).Select(x => x.Name).FirstOrDefault()
                            }).ToList();
                mModel.BillDetails = mobj;
                Session["BillDetails"] = mobj;
            }
            else
            {
                mModel.BillDetail = new BillDetails();
                mModel.DocumentNo = GetCode();
                mModel.BillType = "Direct";
                mModel.SubmitDate = DateTime.Now.ToShortDateString();
                mModel.DocDate = DateTime.Now.ToShortDateString();
                mModel.Through = "By Hand";
                mModel.BillDetail.BillType = "";
                mModel.BillDetail.BillTypeName = ctxTFAT.SubTypes.Where(x => x.Code == "").Select(x => x.Name).FirstOrDefault();
                mModel.Branch = mbranchcode;
            }


            List<GridOption> Grlist = new List<GridOption>();
            var list = ctxTFAT.DocFormats.Where(x => x.StoredProc == "PartySubmission").Select(x => x).ToList();
            if (list != null)
            {
                foreach (var a in list)
                {
                    Grlist.Add(new GridOption
                    {
                        Format = a.FormatCode,
                        IsFormatSelected = a.Selected,
                        StoreProcedure = a.StoredProc
                    });
                }

            }
            mModel.PrintGridList = Grlist;


            return View(mModel);
        }
        public ActionResult SendBill(BillSubmissionVM mModel)
        {
            Session["BillDetails"] = null;
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;
            mModel.Methods = "Send";
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var BillSubmission = ctxTFAT.SendReceBill.Where(x => x.DocNo == mModel.Document).FirstOrDefault();
                mModel.Branch = BillSubmission.Branch;
                mModel.DocumentNo = mModel.Document;
                mModel.FromBranch = BillSubmission.FTBranch;
                mModel.FromBranchName = ctxTFAT.TfatBranch.Where(x => x.Code == BillSubmission.FTBranch).Select(x => x.Name).FirstOrDefault();
                mModel.Remark = BillSubmission.Remark;
                mModel.BillDetail = new BillDetails();
                mModel.BillDetail.BillType = " ";
                mModel.BillDetail.BillTypeName = ctxTFAT.SubTypes.Where(x => x.Code == " ").Select(x => x.Name).FirstOrDefault();
                mModel.DocDate = BillSubmission.DocDate.ToShortDateString();
                mModel.BillDetails = new List<BillDetails>();
                var mobj = (from BillSubRef in ctxTFAT.SendReceBillRef
                            where BillSubRef.DocNo == mModel.Document
                            orderby BillSubRef.BillNo
                            select new BillDetails()
                            {
                                BillNo = BillSubRef.BillNo,
                                BillTableKey = BillSubRef.BillTableKey,
                                BillParty = BillSubRef.Party,
                                BillPartyName = ctxTFAT.CustomerMaster.Where(x => x.Code == BillSubRef.Party).Select(x => x.Name).FirstOrDefault(),
                                PartyParent = BillSubRef.PartyGroup,
                                PartyParentName = ctxTFAT.Master.Where(x => x.Code == BillSubRef.PartyGroup).Select(x => x.Name).FirstOrDefault(),
                                Amount = BillSubRef.Amount.ToString(),
                                Date = BillSubRef.BillDate,
                                BillType = BillSubRef.BillType,
                                BillTypeName = ctxTFAT.DocTypes.Where(x => x.Code == BillSubRef.BillType).Select(x => x.Name).FirstOrDefault(),
                                BillBranch = BillSubRef.BillBranch,
                                BillBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == BillSubRef.BillBranch).Select(x => x.Name).FirstOrDefault()
                            }).ToList();


                var ReceivedDocList = ctxTFAT.SendReceBill.Where(x => x.DocType == "Received").Select(x => x.DocNo).ToList();
                var ReceivedBillList = ctxTFAT.SendReceBillRef.Where(x => ReceivedDocList.Contains(x.DocNo)).ToList();

                var BillNoList = ReceivedBillList.Select(x => x.BillTableKey).ToList();
                var PartyList = ReceivedBillList.Select(x => x.Party).ToList();

                foreach (var item in mobj.Where(w => BillNoList.Contains(w.BillTableKey) && PartyList.Contains(w.BillParty)))
                {
                    item.AllowToChange = true;
                }

                mModel.BillDetails = mobj;

                Session["BillDetails"] = mobj;
            }
            else
            {
                mModel.BillDetail = new BillDetails();
                mModel.DocumentNo = GetCodeOFSend();
                mModel.BillDetail.BillType = "RS";
                mModel.BillDetail.BillTypeName = ctxTFAT.SubTypes.Where(x => x.Code == "RS").Select(x => x.Name).FirstOrDefault();
                mModel.DocDate = DateTime.Now.ToShortDateString();
                mModel.Branch = mbranchcode;
            }
            mModel.SendBillController = true;

            List<GridOption> Grlist = new List<GridOption>();
            var list = ctxTFAT.DocFormats.Where(x => x.StoredProc == "SendBill").Select(x => x).ToList();
            if (list != null)
            {
                foreach (var a in list)
                {
                    Grlist.Add(new GridOption
                    {
                        Format = a.FormatCode,
                        IsFormatSelected = a.Selected,
                        StoreProcedure = a.StoredProc
                    });
                }

            }
            mModel.PrintGridList = Grlist;

            return View(mModel);
        }
        public ActionResult ReceiveBill(BillSubmissionVM mModel)
        {
            Session["BillDetails"] = null;
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;
            var Setup = ctxTFAT.BillSubmissionSetup.FirstOrDefault();
            if (Setup != null)
            {
                mModel.GlobalSearch = Setup.GlobalSearch;
            }
            mModel.Methods = "Received";
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var BillSubmission = ctxTFAT.SendReceBill.Where(x => x.DocNo == mModel.Document).FirstOrDefault();
                mModel.Branch = BillSubmission.Branch;
                mModel.DocumentNo = mModel.Document;
                mModel.FromBranch = BillSubmission.FTBranch;
                mModel.FromBranchName = ctxTFAT.TfatBranch.Where(x => x.Code == BillSubmission.FTBranch).Select(x => x.Name).FirstOrDefault();
                mModel.Remark = BillSubmission.Remark;
                mModel.BillDetail = new BillDetails();
                mModel.BillDetail.BillType = "RS";
                mModel.BillDetail.BillTypeName = ctxTFAT.SubTypes.Where(x => x.Code == "RS").Select(x => x.Name).FirstOrDefault();
                mModel.BillDetails = new List<BillDetails>();
                var mobj = (from BillSubRef in ctxTFAT.SendReceBillRef
                            where BillSubRef.DocNo == mModel.Document
                            orderby BillSubRef.BillNo
                            select new BillDetails()
                            {
                                BillNo = BillSubRef.BillNo,
                                BillTableKey = BillSubRef.BillTableKey,
                                BillParty = BillSubRef.Party,
                                BillPartyName = ctxTFAT.CustomerMaster.Where(x => x.Code == BillSubRef.Party).Select(x => x.Name).FirstOrDefault(),
                                PartyParent = BillSubRef.PartyGroup,
                                PartyParentName = ctxTFAT.Master.Where(x => x.Code == BillSubRef.PartyGroup).Select(x => x.Name).FirstOrDefault(),
                                Amount = BillSubRef.Amount.ToString(),
                                Date = BillSubRef.BillDate,
                                BillType = BillSubRef.BillType,
                                BillTypeName = ctxTFAT.DocTypes.Where(x => x.Code == BillSubRef.BillType).Select(x => x.Name).FirstOrDefault(),
                                BillBranch = BillSubRef.BillBranch,
                                BillBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == BillSubRef.BillBranch).Select(x => x.Name).FirstOrDefault()
                            }).ToList();
                mModel.BillDetails = mobj;
                Session["BillDetails"] = mobj;
            }
            else
            {
                mModel.BillDetail = new BillDetails();
                mModel.DocumentNo = GetCodeOFSend();
                mModel.BillDetail.BillType = "RS";
                mModel.BillDetail.BillTypeName = ctxTFAT.SubTypes.Where(x => x.Code == "RS").Select(x => x.Name).FirstOrDefault();
                mModel.DocDate = DateTime.Now.ToShortDateString();
                mModel.Branch = mbranchcode;
            }
            return View(mModel);
        }


        #region Add LEDGER ITEM
        public ActionResult GetBreakLedger(BillSubmissionVM Model)
        {
            if (Model.Mode == "Edit")
            {
                var result = (List<BillDetails>)Session["BillDetails"];
                var result1 = result.Where(x => x.BillNo == Model.BillDetail.BillNo);
                foreach (var item in result1)
                {
                    Model.BillDetail.BillType = item.BillType;
                    Model.BillDetail.BillTypeName = ctxTFAT.DocTypes.Where(x => x.Code == item.BillType).Select(x => x.Name).FirstOrDefault();
                    Model.BillDetail.PartyParent = item.PartyParent;
                    Model.BillDetail.BillParty = item.BillParty;
                    Model.BillDetail.BillPartyName = item.BillPartyName;
                    Model.BillDetail.PartyParentName = item.PartyParentName;
                    Model.BillDetail.Date = item.Date;
                    Model.BillDetail.Amount = item.Amount;
                }
                var jsonResult = Json(new { Html = this.RenderPartialView("AddNewBill", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            else
            {
                Model.BillDetail = new BillDetails();
                Model.BillDetail.BillType = "RS";
                Model.BillDetail.BillTypeName = ctxTFAT.SubTypes.Where(x => x.Code == "RS").Select(x => x.Name).FirstOrDefault();

                var jsonResult = Json(new { Html = this.RenderPartialView("AddNewBill", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }
        [HttpPost]
        public ActionResult AddEditSelectedLedger(BillSubmissionVM Model)
        {
            if (Model.Mode == "Add")
            {
                List<BillDetails> objledgerdetail = new List<BillDetails>();
                BillDetails SingleBill = new BillDetails();
                if (String.IsNullOrEmpty(Model.Party))
                {
                    SingleBill = (from Sale in ctxTFAT.Ledger
                                  where Sale.TableKey == Model.BillDetail.BillTableKey && Sale.Sno == 1 && (Sale.Type == "SLR00" || Sale.Type == "SLW00")
                                  orderby Sale.Srl
                                  select new BillDetails()
                                  {
                                      BillNo = Sale.Srl,
                                      PartyParent = Sale.Code,
                                      PartyParentName = ctxTFAT.Master.Where(x => x.Code == Sale.Code).Select(x => x.Name).FirstOrDefault(),
                                      Amount = Sale.Debit.ToString(),
                                      Date = Sale.DocDate,
                                      BillParty = Sale.Party,
                                      BillType = Sale.Type,
                                      BillTypeName = ctxTFAT.DocTypes.Where(x => x.Code == Sale.Type).Select(x => x.Name).FirstOrDefault(),
                                      BillBranch = Sale.Branch,
                                      BillTableKey = Sale.TableKey,
                                      BillBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == Sale.Branch).Select(x => x.Name).FirstOrDefault()
                                  }).FirstOrDefault();
                }
                else
                {
                    Model.Party = ctxTFAT.CustomerMaster.Where(x => x.Code == Model.Party).Select(x => x.AccountParentGroup).FirstOrDefault();
                    SingleBill = (from Sale in ctxTFAT.Ledger
                                  where Sale.Srl == Model.BillDetail.BillNo && Sale.Code == Model.Party && Sale.Sno == 1 && (Sale.Type == "SLR00" || Sale.Type == "SLW00")
                                  orderby Sale.Srl
                                  select new BillDetails()
                                  {
                                      BillNo = Sale.Srl,
                                      BillTableKey = Sale.TableKey,
                                      PartyParent = Sale.Code,
                                      PartyParentName = ctxTFAT.Master.Where(x => x.Code == Sale.Code).Select(x => x.Name).FirstOrDefault(),
                                      Amount = Sale.Debit.ToString(),
                                      Date = Sale.DocDate,
                                      BillParty = Sale.Party,
                                      BillType = Sale.Type,
                                      BillTypeName = ctxTFAT.DocTypes.Where(x => x.Code == Sale.Type).Select(x => x.Name).FirstOrDefault(),
                                      BillBranch = Sale.Branch,
                                      BillBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == Sale.Branch).Select(x => x.Name).FirstOrDefault()
                                  }).FirstOrDefault();
                }


                if (Session["BillDetails"] != null)
                {
                    objledgerdetail = (List<BillDetails>)Session["BillDetails"];
                }
                if (SingleBill != null)
                {
                    objledgerdetail.Add(SingleBill);
                }

                Session.Add("BillDetails", objledgerdetail);
                Model.BillDetails = objledgerdetail;
                var html = ViewHelper.RenderPartialView(this, "BreakUpListParty", Model);
                return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = (List<BillDetails>)Session["BillDetails"];

                var html = ViewHelper.RenderPartialView(this, "BreakUpListParty", Model);
                return Json(new
                {
                    Html = html
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult DeleteLedger(BillSubmissionVM Model)
        {
            var result2 = (List<BillDetails>)Session["BillDetails"];
            var result = result2.Where(x => x.BillNo != Model.BillDetail.BillNo).ToList();
            Session.Add("BillDetails", result);
            Model.BillDetails = result;
            var html = ViewHelper.RenderPartialView(this, "BreakUpListParty", Model);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Fetching Bill 

        #region Direct
        public ActionResult FetBillDeatilsDirect(BillSubmissionVM Model)
        {
            //Search Only BillNo Without Ant Criteria And Return Partial View
            string Status = "Success", Party = "", Parent = "", PartyN = "", Amount = "", Branch = "", BranchN = "", BillKey = "", PBillKey = "";
            DateTime Date = new DateTime();
            var CustomerList = ctxTFAT.CustomerMaster.Select(x => x.Code).ToList();
            var SingleBill = (from Sale in ctxTFAT.Ledger
                              where Sale.Srl == Model.BillDetail.BillNo && Sale.Sno == 1 && Sale.Type == Model.BillDetail.BillType
                              orderby Sale.Srl
                              select new BillDetails()
                              {
                                  BillNo = Sale.Srl,
                                  BillTableKey = Sale.TableKey,
                                  BillParentKey = Sale.ParentKey,
                                  PartyParent = Sale.Code,
                                  PartyParentName = ctxTFAT.Master.Where(x => x.Code == Sale.Code).Select(x => x.Name).FirstOrDefault(),
                                  BillParty = Sale.Party,
                                  BillPartyName = ctxTFAT.CustomerMaster.Where(x => x.Code == Sale.Party).Select(x => x.Name).FirstOrDefault(),
                                  Amount = Sale.Debit.ToString(),
                                  Date = Sale.DocDate,
                                  BillBranch = Sale.Branch,
                                  BillBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == Sale.Branch).Select(x => x.Name).FirstOrDefault(),
                              }).FirstOrDefault();
            if (SingleBill == null)
            {
                Status = "Error";
            }
            else
            {
                var SubmitBillList = ctxTFAT.BillSubRef.Where(x => x.DocNo != Model.Document).Select(x => x.BillTableKey).ToList();
                if (SubmitBillList.Contains(SingleBill.BillTableKey))
                {
                    Status = "OptionalError";
                }
                Parent = SingleBill.PartyParent;
                Party = SingleBill.BillParty;
                PartyN = SingleBill.BillPartyName;
                Date = SingleBill.Date;
                Amount = SingleBill.Amount;
                Branch = SingleBill.BillBranch;
                BranchN = SingleBill.BillBranchN;
                BillKey = SingleBill.BillTableKey;
                PBillKey = SingleBill.BillParentKey;

            }

            return Json(new { Status = Status, BillKey = BillKey, PBillKey = PBillKey, Parent = Parent, Party = Party, PartyN = PartyN, Date = Date.ToShortDateString(), Amount = Amount, Branch = Branch, BranchN = BranchN }, JsonRequestBehavior.AllowGet);
            //return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            //var html = ViewHelper.RenderPartialView(this, "DirectSubmission", Model);
        }
        #endregion

        #region Party

        public ActionResult FetBillDeatilsPartyCombo(BillSubmissionVM Model)
        {
            //Search Only BillNo Without Ant Criteria And Return Partial View
            //Model.BillDetails = new List<BillDetails>();
            List<BillDetails> objledgerdetail = new List<BillDetails>();
            if (Session["BillDetails"] != null)
            {
                objledgerdetail = (List<BillDetails>)Session["BillDetails"];
            }

            var ExistBillList = objledgerdetail.Select(x => x.BillTableKey).ToList();
            var SubmitBillList = ctxTFAT.BillSubRef.Select(x => x.BillTableKey).Distinct().ToList();

            string Status = "Success", Message = "";
            var html = "";
            var mobj = (from Sale in ctxTFAT.Ledger
                        where Sale.Party == Model.Party && (!ExistBillList.Contains(Sale.TableKey)) && (!SubmitBillList.Contains(Sale.TableKey)) && (Sale.Type == "SLR00" || Sale.Type == "SLW00")
                        orderby Sale.Srl
                        select new BillDetails()
                        {
                            BillNo = Sale.Srl,
                            BillTableKey = Sale.TableKey,
                            PartyParent = Sale.Code,
                            BillParty = Sale.Party,
                            BillType = Sale.Type,
                            BillTypeName = (ctxTFAT.DocTypes.Where(x => x.Code == Sale.Type).Select(x => x.Name).FirstOrDefault()),
                            PartyParentName = ctxTFAT.Master.Where(x => x.Code == Sale.Code).Select(x => x.Name).FirstOrDefault(),
                            BillPartyName = ctxTFAT.CustomerMaster.Where(x => x.Code == Sale.Party).Select(x => x.Name).FirstOrDefault(),
                            Amount = Sale.Debit.Value.ToString(),
                            Date = Sale.DocDate,
                            BillBranch = Sale.Branch,
                            BillBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == Sale.Branch).Select(x => x.Name).FirstOrDefault()
                        }).ToList();

            if (mobj.Count() == 0)
            {
                Status = "Error";
                Message = ctxTFAT.CustomerMaster.Where(x => x.Code == Model.Party).Select(x => x.Name).FirstOrDefault() + " This Customer Not Found Any Bill...";
            }
            else
            {
                Model.BillDetails = mobj;
                Status = "Success";
                html = ViewHelper.RenderPartialView(this, "FetBillDeatilsPartyCombo", Model);
            }
            return Json(new { Html = html, Status = Status, Message = Message }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddBillToList(BillSubmissionVM Model)
        {
            //Search Only BillNo Without Ant Criteria And Return Partial View
            //Model.BillDetails = new List<BillDetails>();
            string Status = "Success", SearchBillGet = ""; ;
            List<BillDetails> objledgerdetail = new List<BillDetails>();
            var SelectedBillNo = Model.BillDetails.Select(x => x.BillNo).ToList();
            var SelectedBillTablekey = Model.BillDetails.Select(x => x.BillTableKey).ToList();
            if (SelectedBillTablekey.Count() > 0)
            {
                SearchBillGet = "Yes";
            }
            List<BillDetails> mobj = new List<BillDetails>();

            if (String.IsNullOrEmpty(Model.Party))
            {
                if (!String.IsNullOrEmpty(Model.FromBranch))
                {
                    mobj = (from Sale in ctxTFAT.SendReceBillRef
                            join SendReceBill in ctxTFAT.SendReceBill on Sale.DocNo equals SendReceBill.DocNo
                            where SelectedBillTablekey.Contains(Sale.BillTableKey) && SendReceBill.FTBranch == Model.FromBranch
                            orderby Sale.BillNo
                            select new BillDetails()
                            {
                                BillNo = Sale.BillNo,
                                BillTableKey = Sale.BillTableKey,
                                PartyParent = Sale.PartyGroup,
                                BillParty = Sale.Party,
                                BillType = Sale.BillType,
                                BillTypeName = (ctxTFAT.DocTypes.Where(x => x.Code == Sale.BillType).Select(x => x.Name).FirstOrDefault()),
                                PartyParentName = ctxTFAT.Master.Where(x => x.Code == Sale.PartyGroup).Select(x => x.Name).FirstOrDefault(),
                                BillPartyName = ctxTFAT.CustomerMaster.Where(x => x.Code == Sale.Party).Select(x => x.Name).FirstOrDefault(),
                                Amount = Sale.Amount.ToString(),
                                Date = Sale.BillDate,
                                BillBranch = Sale.BillBranch,
                                BillBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == Sale.BillBranch).Select(x => x.Name).FirstOrDefault()
                            }).ToList();
                }
                else
                {
                    if (String.IsNullOrEmpty(SearchBillGet))
                    {
                        mobj = (from Sale in ctxTFAT.SendReceBillRef
                                where SelectedBillNo.Contains(Sale.BillNo)
                                orderby Sale.BillNo
                                select new BillDetails()
                                {
                                    BillNo = Sale.BillNo,
                                    BillTableKey = Sale.BillTableKey,
                                    PartyParent = Sale.PartyGroup,
                                    BillParty = Sale.Party,
                                    BillType = Sale.BillType,
                                    BillTypeName = (ctxTFAT.DocTypes.Where(x => x.Code == Sale.BillType).Select(x => x.Name).FirstOrDefault()),
                                    PartyParentName = ctxTFAT.Master.Where(x => x.Code == Sale.PartyGroup).Select(x => x.Name).FirstOrDefault(),
                                    BillPartyName = ctxTFAT.CustomerMaster.Where(x => x.Code == Sale.Party).Select(x => x.Name).FirstOrDefault(),
                                    Amount = Sale.Amount.ToString(),
                                    Date = Sale.BillDate,
                                    BillBranch = Sale.BillBranch,
                                    BillBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == Sale.BillBranch).Select(x => x.Name).FirstOrDefault()
                                }).ToList();
                    }
                    else
                    {
                        mobj = (from Sale in ctxTFAT.SendReceBillRef
                                where SelectedBillTablekey.Contains(Sale.BillTableKey)
                                orderby Sale.BillNo
                                select new BillDetails()
                                {
                                    BillNo = Sale.BillNo,
                                    BillTableKey = Sale.BillTableKey,
                                    PartyParent = Sale.PartyGroup,
                                    BillParty = Sale.Party,
                                    BillType = Sale.BillType,
                                    BillTypeName = (ctxTFAT.DocTypes.Where(x => x.Code == Sale.BillType).Select(x => x.Name).FirstOrDefault()),
                                    PartyParentName = ctxTFAT.Master.Where(x => x.Code == Sale.PartyGroup).Select(x => x.Name).FirstOrDefault(),
                                    BillPartyName = ctxTFAT.CustomerMaster.Where(x => x.Code == Sale.Party).Select(x => x.Name).FirstOrDefault(),
                                    Amount = Sale.Amount.ToString(),
                                    Date = Sale.BillDate,
                                    BillBranch = Sale.BillBranch,
                                    BillBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == Sale.BillBranch).Select(x => x.Name).FirstOrDefault()
                                }).ToList();
                    }

                }

            }
            else
            {
                if (String.IsNullOrEmpty(SearchBillGet))
                {
                    mobj = (from Sale in ctxTFAT.Ledger
                            where SelectedBillNo.Contains(Sale.Srl) && Sale.Party == Model.Party && Sale.Sno == 1 && (Sale.Type == "SLR00" || Sale.Type == "SLW00")
                            orderby Sale.Srl
                            select new BillDetails()
                            {
                                BillNo = Sale.Srl,
                                BillTableKey = Sale.TableKey,
                                PartyParent = Sale.Code,
                                BillParty = Sale.Party,
                                BillType = Sale.Type,
                                BillTypeName = (ctxTFAT.DocTypes.Where(x => x.Code == Sale.Type).Select(x => x.Name).FirstOrDefault()),
                                PartyParentName = ctxTFAT.Master.Where(x => x.Code == Sale.Code).Select(x => x.Name).FirstOrDefault(),
                                BillPartyName = ctxTFAT.CustomerMaster.Where(x => x.Code == Sale.Party).Select(x => x.Name).FirstOrDefault(),
                                Amount = Sale.Debit.Value.ToString(),
                                Date = Sale.DocDate,
                                BillBranch = Sale.Branch,
                                BillBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == Sale.Branch).Select(x => x.Name).FirstOrDefault()
                            }).ToList();
                }
                else
                {
                    mobj = (from Sale in ctxTFAT.Ledger
                            where SelectedBillTablekey.Contains(Sale.TableKey) && Sale.Sno == 1 && (Sale.Type == "SLR00" || Sale.Type == "SLW00")
                            orderby Sale.Srl
                            select new BillDetails()
                            {
                                BillNo = Sale.Srl,
                                BillTableKey = Sale.TableKey,
                                PartyParent = Sale.Code,
                                BillParty = Sale.Party,
                                BillType = Sale.Type,
                                BillTypeName = (ctxTFAT.DocTypes.Where(x => x.Code == Sale.Type).Select(x => x.Name).FirstOrDefault()),
                                PartyParentName = ctxTFAT.Master.Where(x => x.Code == Sale.Code).Select(x => x.Name).FirstOrDefault(),
                                BillPartyName = ctxTFAT.CustomerMaster.Where(x => x.Code == Sale.Party).Select(x => x.Name).FirstOrDefault(),
                                Amount = Sale.Debit.Value.ToString(),
                                Date = Sale.DocDate,
                                BillBranch = Sale.Branch,
                                BillBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == Sale.Branch).Select(x => x.Name).FirstOrDefault()
                            }).ToList();
                }
            }

            if (Session["BillDetails"] != null)
            {
                objledgerdetail = (List<BillDetails>)Session["BillDetails"];
            }

            objledgerdetail.AddRange(mobj);

            Session.Add("BillDetails", objledgerdetail);
            Model.BillDetails = objledgerdetail;
            Model.BillDetail = new BillDetails();
            Model.BillDetail.BillType = "";
            Model.BillDetail.BillTypeName = "";

            var html = ViewHelper.RenderPartialView(this, "BreakUpListParty", Model);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ResetList(BillSubmissionVM Model)
        {
            Session["BillDetails"] = null;
            List<BillDetails> objledgerdetail = new List<BillDetails>();

            Model.BillDetails = objledgerdetail;
            Model.BillDetail = new BillDetails();
            Model.BillDetail.BillType = "";
            Model.BillDetail.BillTypeName = "";

            var html = ViewHelper.RenderPartialView(this, "BreakUpListParty", Model);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public ActionResult FetBillDeatilsBranchCombo(BillSubmissionVM Model)
        {
            //Search Only BillNo Without Ant Criteria And Return Partial View
            Model.BillDetails = new List<BillDetails>();
            string Status = "Success", Message = "";
            var html = "";

            var Receivedbranchlist = GetChildGrp(mbranchcode);
            var Sendbranchlist = GetChildGrp(Model.FromBranch);

            var GetDocumentlist = ctxTFAT.SendReceBill.Where(x => x.DocType == "Send" && Receivedbranchlist.Contains(x.FTBranch) && Sendbranchlist.Contains(x.Branch)).Select(x => x.DocNo).ToList();

            var GetReceivedBillNoList = (from SendReceBill in ctxTFAT.SendReceBill
                                         join SendReceBillRef in ctxTFAT.SendReceBillRef on SendReceBill.DocNo equals SendReceBillRef.DocNo
                                         where Receivedbranchlist.Contains(SendReceBill.Branch) && SendReceBill.DocType == "Received"
                                         select new { SendReceBill.DocNo, SendReceBillRef.BillTableKey }).ToList();
            var ReceivedBillNo = GetReceivedBillNoList.Select(x => x.BillTableKey).ToList();

            var ReceivedDocNo = GetReceivedBillNoList.Select(x => x.DocNo).ToList();


            var mobj = (from SendReceBillRef in ctxTFAT.SendReceBillRef
                        where GetDocumentlist.Contains(SendReceBillRef.DocNo) && ((!ReceivedDocNo.Contains(SendReceBillRef.DocNo)) && (!ReceivedBillNo.Contains(SendReceBillRef.BillTableKey)))
                        orderby SendReceBillRef.BillNo
                        select new BillDetails()
                        {
                            BillNo = SendReceBillRef.BillNo,
                            BillTableKey = SendReceBillRef.BillTableKey,
                            PartyParent = SendReceBillRef.PartyGroup,
                            PartyParentName = ctxTFAT.Master.Where(x => x.Code == SendReceBillRef.PartyGroup).Select(x => x.Name).FirstOrDefault(),
                            Amount = SendReceBillRef.Amount.ToString(),
                            Date = SendReceBillRef.BillDate,
                            BillType = SendReceBillRef.BillType,
                            BillTypeName = ctxTFAT.DocTypes.Where(x => x.Code == SendReceBillRef.BillType).Select(x => x.Name).FirstOrDefault(),
                            BillParty = SendReceBillRef.Party,
                            BillPartyName = ctxTFAT.CustomerMaster.Where(x => x.Code == SendReceBillRef.Party).Select(x => x.Name).FirstOrDefault(),
                            BillBranch = SendReceBillRef.BillBranch,
                            BillBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == SendReceBillRef.BillBranch).Select(x => x.Name).FirstOrDefault()
                        }).ToList();

            if (mobj.Count() > 0)
            {
                Status = "Success";
                Model.BillDetails = mobj;
                html = ViewHelper.RenderPartialView(this, "FetBillDeatilsPartyCombo", Model);
            }
            else
            {
                Status = "Error";
                Message = "Not Found Any Bill...";
            }

            return Json(new { Status = Status, Message = Message, Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SearchBillNo(BillSubmissionVM Model)
        {
            string Status = "", Message = "", Html = "";
            Model.Mode = "Add";
            var result = (List<BillDetails>)Session["BillDetails"];
            if (result == null)
            {
                result = new List<BillDetails>();
            }
            var bill = new List<BillDetails>();
            var result1 = result.Where(x => x.BillNo == Model.BillDetail.BillNo).FirstOrDefault();
            if (result1 == null)
            {

                if (String.IsNullOrEmpty(Model.Party))
                {
                    bill = (from Sale in ctxTFAT.Ledger
                            where Sale.Srl == Model.BillDetail.BillNo && Sale.Sno == 1 && (Sale.Type == "SLR00" || Sale.Type == "SLW00")
                            orderby Sale.Srl
                            select new BillDetails()
                            {
                                BillNo = Sale.Srl,
                                BillTableKey = Sale.TableKey,
                                PartyParent = Sale.Code,
                                PartyParentName = ctxTFAT.Master.Where(x => x.Code == Sale.Code).Select(x => x.Name).FirstOrDefault(),
                                Amount = Sale.Debit.Value.ToString(),
                                Date = Sale.DocDate,
                                BillType = Sale.Type,
                                BillTypeName = ctxTFAT.DocTypes.Where(x => x.Code == Sale.Type).Select(x => x.Name).FirstOrDefault(),
                                BillParty = Sale.Party,
                                BillPartyName = ctxTFAT.CustomerMaster.Where(x => x.Code == Sale.Party).Select(x => x.Name).FirstOrDefault(),
                                BillBranch = Sale.Branch,
                                BillBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == Sale.Branch).Select(x => x.Name).FirstOrDefault()
                            }).ToList();
                }
                else
                {
                    Model.Party = ctxTFAT.CustomerMaster.Where(x => x.Code == Model.Party).Select(x => x.AccountParentGroup).FirstOrDefault();
                    bill = (from Ledger in ctxTFAT.Ledger
                            where Ledger.Srl.Trim() == Model.BillDetail.BillNo.Trim() && Ledger.Code == Model.Party && Ledger.Sno == 1 && (Ledger.Type == "SLR00" || Ledger.Type == "SLW00")
                            orderby Ledger.Srl
                            select new BillDetails()
                            {
                                BillNo = Ledger.Srl,
                                BillTableKey = Ledger.TableKey,
                                PartyParent = Ledger.Code,
                                PartyParentName = ctxTFAT.Master.Where(x => x.Code == Ledger.Code).Select(x => x.Name).FirstOrDefault(),
                                Amount = Ledger.Debit.Value.ToString(),
                                Date = Ledger.DocDate,
                                BillType = Ledger.Type,
                                BillTypeName = ctxTFAT.DocTypes.Where(x => x.Code == Ledger.Type).Select(x => x.Name).FirstOrDefault(),
                                BillParty = Ledger.Party,
                                BillPartyName = ctxTFAT.CustomerMaster.Where(x => x.Code == Ledger.Party).Select(x => x.Name).FirstOrDefault(),
                                BillBranch = Ledger.Branch,
                                BillBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == Ledger.Branch).Select(x => x.Name).FirstOrDefault()
                            }).ToList();
                }

                if (bill.Count() == 0)
                {
                    Status = "Error";
                    Message = "Not Found...!";
                }
                else
                {
                    if (bill.Count() == 1)
                    {
                        var SubmitBillList = ctxTFAT.BillSubRef.Select(x => x.BillTableKey).ToList();
                        if (SubmitBillList.Contains(bill.Select(x => x.BillTableKey).FirstOrDefault()) && Model.Methods != "Received" && Model.Methods != "Send")
                        {
                            Status = "Optional";
                            Message = "This Bill Submited Already R U Sure To Submit Bill?";
                            Model.BillDetail = bill.FirstOrDefault();
                        }
                        else
                        {
                            Status = "Success";
                            Model.BillDetail = bill.FirstOrDefault();
                        }
                    }
                    else
                    {
                        Model.BillDetails = bill;
                    }
                }
            }
            else
            {

                Status = "Error";
                Message = "Already Selected This Bill No...!";
            }

            var SearchCombo = "";
            if (Status != "Error" && bill.Count() == 1)
            {
                Html = ViewHelper.RenderPartialView(this, "AddNewBill", Model);
            }
            else if (Status != "Error" && bill.Count() > 1)
            {
                SearchCombo = "Y";
                Status = "Success";
                Html = ViewHelper.RenderPartialView(this, "SearchList", Model);

            }

            return Json(new { SearchCombo = SearchCombo, Status = Status, Message = Message, Html = Html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AnySearchBillNo(BillSubmissionVM Model)
        {
            string Status = "", Message = "", Html = "";
            Model.Mode = "Add";
            var result = (List<BillDetails>)Session["BillDetails"];
            if (result == null)
            {
                result = new List<BillDetails>();
            }
            var bill = new List<BillDetails>();
            var result1 = result.Where(x => x.BillNo == Model.BillDetail.BillNo).FirstOrDefault();
            if (result1 == null)
            {
                bill = (from Sale in ctxTFAT.Ledger
                        where Sale.Srl == Model.BillDetail.BillNo && Sale.Sno == 1 && (Sale.Type == "SLR00" || Sale.Type == "SLW00")
                        orderby Sale.Srl
                        select new BillDetails()
                        {
                            BillNo = Sale.Srl,
                            BillTableKey = Sale.TableKey,
                            PartyParent = Sale.Code,
                            PartyParentName = ctxTFAT.Master.Where(x => x.Code == Sale.Code).Select(x => x.Name).FirstOrDefault(),
                            Amount = Sale.Debit.Value.ToString(),
                            Date = Sale.DocDate,
                            BillType = Sale.Type,
                            BillTypeName = ctxTFAT.DocTypes.Where(x => x.Code == Sale.Type).Select(x => x.Name).FirstOrDefault(),
                            BillParty = Sale.Party,
                            BillPartyName = ctxTFAT.CustomerMaster.Where(x => x.Code == Sale.Party).Select(x => x.Name).FirstOrDefault(),
                            BillBranch = Sale.Branch,
                            BillBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == Sale.Branch).Select(x => x.Name).FirstOrDefault()
                        }).ToList();


                if (bill.Count() == 0)
                {
                    Status = "Error";
                    Message = "Not Found...!";
                }
                else
                {
                    if (bill.Count() == 1)
                    {
                        var SubmitBillList = ctxTFAT.BillSubRef.Select(x => x.BillTableKey).ToList();
                        if (SubmitBillList.Contains(bill.Select(x => x.BillTableKey).FirstOrDefault()) && Model.Methods != "Received" && Model.Methods != "Send")
                        {
                            Status = "Optional";
                            Message = "This Bill Submited Already R U Sure To Submit Bill?";
                            Model.BillDetail = bill.FirstOrDefault();
                        }
                        else
                        {
                            Status = "Success";
                            Model.BillDetail = bill.FirstOrDefault();
                        }
                    }
                    else
                    {
                        Model.BillDetails = bill;
                    }

                }
            }
            else
            {

                Status = "Error";
                Message = "Already Selected This Bill No...!";
            }

            var SearchCombo = "";
            if (Status != "Error" && bill.Count() == 1)
            {
                Html = ViewHelper.RenderPartialView(this, "AddAnyNewBill", Model);
            }
            else if (Status != "Error" && bill.Count() > 1)
            {
                SearchCombo = "Y";
                Status = "Success";
                Html = ViewHelper.RenderPartialView(this, "SearchList", Model);

            }

            return Json(new { SearchCombo = SearchCombo, Status = Status, Message = Message, Html = Html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FetBillDeatilsParty(BillSubmissionVM Model)
        {
            //Search  BillNo and Party Both then Return Partial View
            string Status = "Success", Party = "", PartyN = "", Date = "", Amount = "";

            return Json(new { Status = Status, Party = Party, PartyN = PartyN, Date = Date, Amount = Amount, }, JsonRequestBehavior.AllowGet);
        }
        #endregion



        #region SaveData

        public ActionResult SaveData(BillSubmissionVM mModel)
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
                        return Msg;
                    }

                    var Date = ConvertDDMMYYTOYYMMDD(mModel.DocDate);
                    var mtfatperd = ctxTFAT.TfatPerd.Where(x => x.PerdCode == mperiod).Select(x => new { x.StartDate, x.LastDate }).FirstOrDefault();
                    DateTime mStartDate = Convert.ToDateTime(System.Web.HttpContext.Current.Session["StartDate"]);
                    DateTime mLastDate = Convert.ToDateTime(System.Web.HttpContext.Current.Session["LastDate"]);

                    if (mtfatperd.StartDate > Date || Date > mtfatperd.LastDate)
                    {
                        return Json(new
                        {

                            Status = "ErrorValid",
                            Message = "Selected Document Date is not in Current Accounting Period..!"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    BillSubmission mobj = new BillSubmission();
                    bool mAdd = true;
                    if (ctxTFAT.BillSubmission.Where(x => (x.DocNo == mModel.Document)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.BillSubmission.Where(x => (x.DocNo == mModel.Document)).FirstOrDefault();
                        mAdd = false;
                        mdocument = mobj.DocNo.ToString();
                        DeUpdate(mModel);
                    }
                    if (mAdd)
                    {
                        mobj.DocNo = GetCode();
                        mobj.Prefix = mperiod;
                        mdocument = mobj.DocNo.ToString();
                    }
                    mobj.Branch = mbranchcode;
                    mobj.DocDate = ConvertDDMMYYTOYYMMDD(mModel.DocDate);
                    mobj.DocType = mModel.BillType;

                    List<BillDetails> objledgerdetail = new List<BillDetails>();
                    if (Session["BillDetails"] != null)
                    {
                        objledgerdetail = (List<BillDetails>)Session["BillDetails"];
                    }

                    if (objledgerdetail.Count() > 0)
                    {
                        mobj.NoOfBill = objledgerdetail.Count();
                    }
                    else
                    {
                        mobj.NoOfBill = 1;
                    }
                    mobj.SubDt = ConvertDDMMYYTOYYMMDD(mModel.SubmitDate);
                    mobj.Through = mModel.Through;
                    mobj.Party = mModel.BillDetail.BillParty;
                    mobj.Remark = mModel.Remark;

                    //// iX9: default values for the fields not used @Form
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                    if (mAdd == true)
                    {
                        ctxTFAT.BillSubmission.Add(mobj);
                        if (mobj.DocType != "Direct")
                        {
                            AttachmentVM vM = new AttachmentVM();
                            vM.ParentKey = mobj.DocNo;
                            vM.Srl = mobj.DocNo.ToString();
                            vM.Type = "BLSMT";
                            SaveAttachment(vM);
                        }

                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }


                    if (mobj.DocType == "Direct")
                    {
                        BillSubRef billSubRef = new BillSubRef();
                        billSubRef.DocNo = mobj.DocNo;
                        billSubRef.BillNo = mModel.BillDetail.BillNo;
                        billSubRef.BillTableKey = mModel.BillDetail.BillTableKey;
                        billSubRef.BillBranch = mModel.BillDetail.BillBranch;
                        billSubRef.BillType = mModel.BillDetail.BillType;
                        billSubRef.PartyGroup = mModel.BillDetail.PartyParent;
                        billSubRef.BillDate = ConvertDDMMYYTOYYMMDD(mModel.BillDetail.Date.ToString());
                        billSubRef.Amount = Convert.ToDecimal(mModel.BillDetail.Amount);
                        billSubRef.AUTHIDS = muserid;
                        billSubRef.AUTHORISE = mauthorise;
                        billSubRef.ENTEREDBY = muserid;
                        billSubRef.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                        ctxTFAT.BillSubRef.Add(billSubRef);
                        BillSubmissionNotification(mobj, billSubRef);
                        UpdateAuditTrail(mbranchcode, mModel.Mode, "Direct-Submission", "BLSMT" + mperiod.Substring(0, 2) + mobj.DocNo, mobj.SubDt, 0, "", "Save Bill NO:" + mobj.DocNo, "NA");
                    }
                    else
                    {
                        foreach (var item in objledgerdetail)
                        {
                            BillSubRef billSubRef = new BillSubRef();
                            billSubRef.DocNo = mobj.DocNo;
                            billSubRef.BillNo = item.BillNo;
                            billSubRef.BillTableKey = item.BillTableKey;
                            billSubRef.BillBranch = item.BillBranch;
                            billSubRef.BillType = item.BillType;
                            //billSubRef.PartyGroup = ctxTFAT.CustomerMaster.Where(x => x.Code == mobj.Party).Select(x => x.AccountParentGroup).FirstOrDefault(); ;
                            billSubRef.PartyGroup = item.PartyParent;
                            billSubRef.BillDate = ConvertDDMMYYTOYYMMDD(item.Date.ToString());
                            billSubRef.Amount = Convert.ToDecimal(item.Amount);
                            billSubRef.AUTHIDS = muserid;
                            billSubRef.AUTHORISE = mauthorise;
                            billSubRef.ENTEREDBY = muserid;
                            billSubRef.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                            ctxTFAT.BillSubRef.Add(billSubRef);
                            BillSubmissionNotification(mobj, billSubRef);
                            UpdateAuditTrail(mbranchcode, mModel.Mode, "Party-Submission", "BLSMT" + mperiod.Substring(0, 2) + mobj.DocNo, mobj.SubDt, 0, "", "Save Bill NO:" + mobj.DocNo, "NA");
                        }
                    }

                    ctxTFAT.SaveChanges();
                    string mNewCode = "";
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
            return Json(new { SerialNo = mdocument, Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SendReceiveSaveData(BillSubmissionVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        var Msg = SendReceiveDeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Msg;
                    }
                    var Date = ConvertDDMMYYTOYYMMDD(mModel.DocDate);
                    var mtfatperd = ctxTFAT.TfatPerd.Where(x => x.PerdCode == mperiod).Select(x => new { x.StartDate, x.LastDate }).FirstOrDefault();
                    DateTime mStartDate = Convert.ToDateTime(System.Web.HttpContext.Current.Session["StartDate"]);
                    DateTime mLastDate = Convert.ToDateTime(System.Web.HttpContext.Current.Session["LastDate"]);

                    if (mtfatperd.StartDate > Date || Date > mtfatperd.LastDate)
                    {
                        return Json(new
                        {

                            Status = "ErrorValid",
                            Message = "Selected Document Date is not in Current Accounting Period..!"
                        }, JsonRequestBehavior.AllowGet);
                    }




                    SendReceBill mobj = new SendReceBill();
                    bool mAdd = true;
                    if (ctxTFAT.SendReceBill.Where(x => (x.DocNo == mModel.Document)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.SendReceBill.Where(x => (x.DocNo == mModel.Document)).FirstOrDefault();
                        mAdd = false;
                        mdocument = mobj.DocNo.ToString();
                        SendReceiveDeUpdate(mModel);
                    }
                    if (mAdd)
                    {
                        mobj.DocNo = mModel.DocumentNo;
                        mdocument = mobj.DocNo.ToString();
                    }

                    List<BillDetails> objledgerdetail = new List<BillDetails>();
                    if (Session["BillDetails"] != null)
                    {
                        objledgerdetail = (List<BillDetails>)Session["BillDetails"];
                    }

                    mobj.Branch = mbranchcode;
                    mobj.DocDate = ConvertDDMMYYTOYYMMDD(mModel.DocDate);
                    mobj.DocType = mModel.BillType;
                    mobj.NoOfBill = objledgerdetail.Count();
                    mobj.FTBranch = mModel.FromBranch;
                    mobj.Remark = mModel.Remark;

                    //// iX9: default values for the fields not used @Form
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                    if (mAdd == true)
                    {
                        ctxTFAT.SendReceBill.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }


                    foreach (var item in objledgerdetail)
                    {
                        SendReceBillRef billSubRef = new SendReceBillRef();
                        billSubRef.DocNo = mobj.DocNo;
                        billSubRef.BillNo = item.BillNo;
                        billSubRef.BillTableKey = item.BillTableKey;
                        billSubRef.BillType = item.BillType;
                        billSubRef.BillBranch = item.BillBranch;
                        billSubRef.PartyGroup = item.PartyParent;
                        billSubRef.Party = item.BillParty;
                        billSubRef.BillDate = ConvertDDMMYYTOYYMMDD(item.Date.ToString());
                        billSubRef.Amount = Convert.ToDecimal(item.Amount);
                        billSubRef.AUTHIDS = muserid;
                        billSubRef.AUTHORISE = mauthorise;
                        billSubRef.ENTEREDBY = muserid;
                        billSubRef.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                        ctxTFAT.SendReceBillRef.Add(billSubRef);
                    }



                    ctxTFAT.SaveChanges();
                    string mNewCode = "";
                    transaction.Commit();
                    transaction.Dispose();

                    DateTime dailyTime = ConvertDDMMYYTOYYMMDD(StartDate.ToString());
                    var Prefix = ctxTFAT.TfatPerd.Where(x => x.StartDate == dailyTime).Select(x => x.PerdCode).FirstOrDefault();

                    UpdateAuditTrail(mbranchcode, mModel.Mode, "Send-Received-Submission", "BLSMT" + Prefix.Substring(0, 2) + mobj.DocNo, mobj.DocDate, 0, "", "Save Bill NO:" + mobj.DocNo, "NA");
                    //UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "");

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
            return Json(new { SerialNo = mdocument, Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
        }



        public void SendReceiveDeUpdate(BillSubmissionVM Model)
        {
            var BillSubRef = ctxTFAT.SendReceBillRef.Where(x => x.DocNo == Model.Document).ToList();
            ctxTFAT.SendReceBillRef.RemoveRange(BillSubRef);
            ctxTFAT.SaveChanges();
        }
        public void DeUpdate(BillSubmissionVM Model)
        {
            var BillSubRef = ctxTFAT.BillSubRef.Where(x => x.DocNo == Model.Document).ToList();
            ctxTFAT.BillSubRef.RemoveRange(BillSubRef);
            ctxTFAT.SaveChanges();
        }


        public ActionResult DeleteStateMaster(BillSubmissionVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var mList = ctxTFAT.BillSubmission.Where(x => (x.DocNo == mModel.Document)).FirstOrDefault();
            var mList1 = ctxTFAT.BillSubRef.Where(x => (x.DocNo == mModel.Document)).ToList();
            ctxTFAT.BillSubmission.Remove(mList);
            ctxTFAT.BillSubRef.RemoveRange(mList1);
            ctxTFAT.SaveChanges();
            UpdateAuditTrail(mbranchcode, mModel.Mode, "Party-Submission", "BLSMT" + mperiod.Substring(0, 2) + mList.DocNo, mList.SubDt, 0, "", "Save Bill NO:" + mList.DocNo, "NA");

            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SendReceiveDeleteStateMaster(BillSubmissionVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var mList = ctxTFAT.SendReceBill.Where(x => (x.DocNo == mModel.Document)).FirstOrDefault();
            var mList1 = ctxTFAT.SendReceBillRef.Where(x => (x.DocNo == mModel.Document)).ToList();

            if (mList.DocType == "Send")
            {
                var ReceivedDocList = ctxTFAT.SendReceBill.Where(x => x.DocType == "Received").Select(x => x.DocNo).ToList();
                var ReceivedBillList = ctxTFAT.SendReceBillRef.Where(x => ReceivedDocList.Contains(x.DocNo)).ToList();

                var BillNoList = ReceivedBillList.Select(x => x.BillTableKey).ToList();
                var PartyList = ReceivedBillList.Select(x => x.Party).ToList();

                var ReceivBill = mList1.Where(w => BillNoList.Contains(w.BillTableKey) && PartyList.Contains(w.Party)).ToList();
                if (ReceivBill.Count() > 0)
                {
                    return Json(new
                    {
                        Message = "Some Bill Received So We Can't Delete....",
                        Status = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ctxTFAT.SendReceBill.Remove(mList);
                    ctxTFAT.SendReceBillRef.RemoveRange(mList1);
                    ctxTFAT.SaveChanges();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, "Send-Received-Submission", "BLSMT" + mperiod.Substring(0, 2) + mList.DocNo, mList.DocDate, 0, "", "Delete Bill NO:" + mList.DocNo, "NA");

                    return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                ctxTFAT.SendReceBill.Remove(mList);
                ctxTFAT.SendReceBillRef.RemoveRange(mList1);
                ctxTFAT.SaveChanges();
                UpdateAuditTrail(mbranchcode, mModel.Mode, "Send-Received-Submission", "BLSMT" + mperiod.Substring(0, 2) + mList.DocNo, mList.DocDate, 0, "", "Delete Bill NO:" + mList.DocNo, "NA");
                return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region AlertNote Stop CHeck

        public ActionResult CheckStopAlertNote(string Type, List<string> TypeCode, string DocTpe)
        {
            string Status = "Success", Message = "";
            var Mobj = (from AlertMater in ctxTFAT.AlertNoteMaster
                        where AlertMater.Type == Type && TypeCode.Contains(AlertMater.TypeCode) && AlertMater.RefType.Contains(DocTpe)
                        orderby AlertMater.DocNo
                        select new AlertNoteVM()
                        {
                            TypeCode = AlertMater.TypeCode,
                            DocReceived = AlertMater.TableKey,
                        }).ToList();
            foreach (var item in Mobj)
            {
                var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.TableKey == item.DocReceived).Select(x => x.Stop).FirstOrDefault().Split(',').ToList();
                foreach (var stp in Activirty)
                {
                    if (DocTpe.Trim() == stp.Trim())
                    {
                        Status = "Error";
                        Message += "Lock Submission On Bill No: " + item.TypeCode + " As Per The AlertNote Rule...\n";
                        break;
                    }
                }
            }
            var jsonResult = Json(new { Status = Status, Message = Message }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public ActionResult CheckStopAlertNoteOfParty(string Type, List<string> TypeCode, string DocTpe)
        {
            string Status = "Success", Message = "";

            List<BillDetails> objledgerdetail = new List<BillDetails>();
            objledgerdetail = (List<BillDetails>)Session["BillDetails"];
            string BindDocNo = string.Join("','", TypeCode);
            BindDocNo = "'" + BindDocNo.Substring(0, BindDocNo.Length - 2);
            string Query = "select TypeCode from AlertNoteMaster where Type+TypeCode in (" + BindDocNo + ") and Stop='" + DocTpe + "' ";
            DataTable dt = GetDataTable(Query, "");
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    Status = "Error";
                    Message += "Lock Submission On Bill No: " + item["TypeCode"] + " As Per The AlertNote Rule...\n";
                    break;
                }
            }

            //foreach (var item in objledgerdetail)
            //{
            //    var Activirty = ctxTFAT.AlertNoteMaster.Where(x => x.Type == item.BillType && x.TypeCode == item.BillNo).FirstOrDefault();
            //    if (Activirty != null)
            //    {
            //        var StopTypeList = Activirty.Stop.Split(',').ToList();
            //        foreach (var stp in StopTypeList)
            //        {
            //            if (DocTpe.Trim() == stp.Trim())
            //            {
            //                Status = "Error";
            //                Message += "Lock Submission On Bill No: " + item.BillNo + " As Per The AlertNote Rule...\n";
            //                break;
            //            }
            //        }
            //    }
            //}




            var jsonResult = Json(new { Status = Status, Message = Message }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        #endregion




        [HttpPost]
        public ActionResult GetMultiPrint(GridOption Model)
        {
            List<GridOption> Grlist = new List<GridOption>();
            Model.Type = ctxTFAT.DocFormats.Where(x => x.FormatCode == Model.ViewDataId).Select(x => x.Type).FirstOrDefault();


            var list = ctxTFAT.DocFormats.Where(x => x.StoredProc == Model.ViewDataId).Select(x => x).ToList();
            if (list != null)
            {
                foreach (var a in list)
                {
                    Grlist.Add(new GridOption
                    {
                        Format = a.FormatCode,
                        IsFormatSelected = a.Selected,
                        StoreProcedure = a.StoredProc
                    });
                }

            }
            Model.PrintGridList = Grlist;
            var html = ViewHelper.RenderPartialView(this, "ReportPrintOptions", new GridOption() { PrintGridList = Model.PrintGridList, Document = Model.Document });
            var jsonResult = Json(new
            {
                Document = Model.Document,
                PrintGridList = Model.PrintGridList,
                Html = html
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }

        public ActionResult PrintSingleDocumentCrystal(GridOption Model)
        {
            var PDFName = Model.Document;
            //var relativePath = "~/Reports/" + Model.Format + ".rpt";
            if (Model.Format == null)
            {
                return Json(new { Status = "filenotfound" }, JsonRequestBehavior.AllowGet);
            }

            if (FileExists("~/Reports/" + Model.Format.Trim() + ".rpt") == false)
            {
                return Json(new { Status = "filenotfound" }, JsonRequestBehavior.AllowGet);
            }
            #region Remove Comment
            var absolutePath = HttpContext.Server.MapPath("~/Reports/" + Model.Format.Trim() + ".rpt");
            if (System.IO.File.Exists(absolutePath) == false)
            {
                return Json(new { Status = "filenotfound" }, JsonRequestBehavior.AllowGet);
            }
            #endregion

            string mParentKey = Model.Document;
            Model.Branch = mbranchcode;
            DataTable dtreport = new DataTable();
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("SPDoc_" + Model.StoreProcedure, tfat_conx); //name of the storedprocedure
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@mParentKey", mParentKey);
            cmd.Parameters.AddWithValue("@mBranch", Model.Branch);
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            adp.Fill(dtreport);

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), Model.Format.Trim() + ".rpt"));
            rd.SetDataSource(dtreport);

            try
            {
                Stream mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                mstream.Seek(0, SeekOrigin.Begin);
                rd.Close();
                rd.Dispose();
                if (String.IsNullOrEmpty(PDFName))
                {
                    return File(mstream, "application/PDF");
                }
                else
                {
                    return File(mstream, "application/PDF", PDFName + ".pdf");
                }
                //return File(mstream, "application/PDF");
            }
            catch
            {
                rd.Close();
                rd.Dispose();
                throw;
            }
            finally
            {
                rd.Close();
                rd.Dispose();
            }
        }

        public ActionResult SendMultiReport(GridOption Model)
        {
            var PDFName = Model.Document;
            if (Model.Format == null)
            {
                return null;
            }

            string mParentKey = "";
            Document document = new Document(PageSize.A4, 10, 10, 10, 10);
            var ms = new MemoryStream();
            {
                PdfCopy pdf = new PdfCopy(document, ms);
                document.Open();
                pdf.Open();

                List<string> mformats = Model.Format.Split(',').ToList();
                foreach (var mformat in mformats)
                {
                    mParentKey = Model.Document;
                    Model.Branch = mbranchcode;
                    Model.StoreProcedure = ctxTFAT.DocFormats.Where(x => x.FormatCode.Trim() == mformat.Trim()).Select(x => x.StoredProc).FirstOrDefault();
                    DataTable dtreport = new DataTable();
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("SPDoc_" + Model.StoreProcedure, tfat_conx); //name of the storedprocedure
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@mParentKey", mParentKey);
                    cmd.Parameters.AddWithValue("@mBranch", Model.Branch);
                    SqlDataAdapter adp = new SqlDataAdapter(cmd);
                    adp.Fill(dtreport);

                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/Reports"), mformat.Trim() + ".rpt"));
                    rd.SetDataSource(dtreport);
                    try
                    {
                        Stream mstream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        mstream.Seek(0, SeekOrigin.Begin);

                        Warning[] warnings;
                        string[] streamids;
                        string mimeType;
                        string encoding;
                        string extension;
                        MemoryStream memory1 = new MemoryStream();
                        mstream.CopyTo(memory1);
                        byte[] bytes = memory1.ToArray();
                        MemoryStream memoryStream = new MemoryStream(bytes);
                        PdfReader imageDocumentReader = new PdfReader(memoryStream.ToArray());
                        int ab = imageDocumentReader.NumberOfPages;
                        for (int a = 1; a <= ab; a++)
                        {
                            var page = pdf.GetImportedPage(imageDocumentReader, a);
                            pdf.AddPage(page);
                        }
                        imageDocumentReader.Close();
                    }
                    catch
                    {
                        rd.Close();
                        rd.Dispose();
                        throw;
                    }
                    finally
                    {
                        rd.Close();
                        rd.Dispose();
                    }
                }
            }
            document.Close();

            if (String.IsNullOrEmpty(PDFName))
            {
                return File(ms.ToArray(), "application/PDF");
            }
            else
            {
                return File(ms.ToArray(), "application/PDF", PDFName + ".pdf");
            }
            //return File(ms.ToArray(), "application/PDF");

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

        public void SaveAttachment(AttachmentVM Model)
        {

            List<AttachmentVM> DocList = new List<AttachmentVM>();
            if (Session["TempAttach"] != null)
            {
                DocList = (List<AttachmentVM>)Session["TempAttach"];
            }

            if (DocList != null && DocList.Count != 0)
            {
                var AttachCode = GetNewAttachCode();
                int J = 1;
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
                    att.Sno = J;
                    att.Srl = Model.Srl;
                    att.SrNo = J;
                    att.TableKey = Model.Type + mperiod.Substring(0, 2) + J.ToString("D3") + Model.Srl;
                    att.Type = String.IsNullOrEmpty(item.Type) == true ? "BLSMT" : item.Type;
                    att.ParentKey = Model.ParentKey;

                    att.CompCode = mcompcode;
                    att.ExternalAttach = item.ExternalAttach;
                    att.RefDocNo = item.RefCode;
                    att.RefType = item.RefType;
                    ctxTFAT.Attachment.Add(att);
                    ctxTFAT.SaveChanges();
                    System.IO.File.WriteAllBytes(directoryPath, IData);

                    ++J;
                    ++AttachCode;
                }
            }

        }

    }
}