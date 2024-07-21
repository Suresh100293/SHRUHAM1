using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using System.Data.SqlClient;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class LRSetupController : BaseController
    {
         
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;

        #region Functions

        public JsonResult GetLRType(string term)//LRType
        {
            var list = ctxTFAT.LRTypeMaster.ToList().Distinct();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = ctxTFAT.LRTypeMaster.Where(x => x.LRType.ToLower().Contains(term.ToLower())).ToList().Distinct();
            }

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.LRType
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadToPay(string term)//LRType
        {
            var list = ctxTFAT.Master.ToList();

            var UserInRole = ctxTFAT.Master.
        Join(ctxTFAT.MasterGroups, u => u.Grp, uir => uir.Code,
        (u, uir) => new { u, uir })
        .Where(m => m.u.BaseGr == "D" || m.u.BaseGr == "U").Select(x => x.u).ToList();
            if (!(String.IsNullOrEmpty(term)))
            {
                UserInRole = UserInRole.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }

            var Modified = UserInRole.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetServiceType(string term)//ServiceType
        {
            var list = ctxTFAT.ServiceTypeMaster.ToList().Distinct();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = ctxTFAT.ServiceTypeMaster.Where(x => x.ServiceType.ToLower().Contains(term.ToLower())).ToList().Distinct();
            }

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.ServiceType
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public DateTime ConvertDDMMYYTOYYMMDD(string da)
        {
            string abc = da.Substring(6, 4) + "/" + da.Substring(3, 2) + "/" + da.Substring(0, 2);
            return Convert.ToDateTime(abc);
        }

        public JsonResult BillatBranch(string term)
        {
            List<TfatBranch> list = ctxTFAT.TfatBranch.Where(x => x.Code != "G00000" && x.Grp != "G00000" && x.Category != "Area").OrderBy(x => x.Name).ToList();


            list.Where(w => w.Category == "Zone").ToList().ForEach(w => w.Name = w.Name + " - Z");
            list.Where(w => w.Category == "0").ToList().ForEach(w => w.Name = w.Name + " - HO");
            list.Where(w => w.Category == "Branch").ToList().ForEach(w => w.Name = w.Name + " - B");
            list.Where(w => w.Category == "SubBranch").ToList().ForEach(w => w.Name = w.Name + " - SB");
            list.Where(w => w.Category == "Area").ToList().ForEach(w => w.Name = w.Name + " - A");

            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).OrderBy(x => x.Name).ToList();
            }
            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ChargeType(string term)//ChargeType
        {
            var list = ctxTFAT.ChargeTypeMaster.ToList().Distinct();


            //var list = ctxTFAT.KilometerMaster.Join(ctxTFAT.TreeTable, p => p.From, pc => pc.Code, (p, pc) => new { p, pc }).Where(x =>  (!(x.pc.Code.ToLower().Contains("h"))) && (!(x.pc.Code.ToLower().Contains("z")))).Select(X=>X.pc).ToList().Distinct();
            if (!(String.IsNullOrEmpty(term)))
            {
                list = ctxTFAT.ChargeTypeMaster.Where(x => x.Code.ToLower().Contains(term.ToLower())).ToList().Distinct();
            }

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.ChargeType
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetUnit(string term)//Unit
        {
            //Descr
            var list = ctxTFAT.UnitMaster.ToList().Distinct();


            //var list = ctxTFAT.KilometerMaster.Join(ctxTFAT.TreeTable, p => p.From, pc => pc.Code, (p, pc) => new { p, pc }).Where(x =>  (!(x.pc.Code.ToLower().Contains("h"))) && (!(x.pc.Code.ToLower().Contains("z")))).Select(X=>X.pc).ToList().Distinct();
            if (!(String.IsNullOrEmpty(term)))
            {
                list = ctxTFAT.UnitMaster.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList().Distinct();
            }

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetParticulars(string term)//Particular
        {
            //Descr
            var list = ctxTFAT.DescriptionMaster.ToList().Distinct();


            //var list = ctxTFAT.KilometerMaster.Join(ctxTFAT.TreeTable, p => p.From, pc => pc.Code, (p, pc) => new { p, pc }).Where(x =>  (!(x.pc.Code.ToLower().Contains("h"))) && (!(x.pc.Code.ToLower().Contains("z")))).Select(X=>X.pc).ToList().Distinct();
            if (!(String.IsNullOrEmpty(term)))
            {
                list = ctxTFAT.DescriptionMaster.Where(x => x.Description.ToLower().Contains(term.ToLower())).ToList().Distinct();
            }

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Description
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);

        }

        private List<SelectListItem> PopulatePrintFormats()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            var list = ctxTFAT.DocFormats.Where(x => x.Type == "LR000").OrderBy(x => x.FormatCode).ToList();
            foreach (var item in list)
            {
                items.Add(new SelectListItem
                {
                    Text = item.FormatCode.ToString(),
                    Value = item.FormatCode.ToString()
                });
            }
            return items;
        }

        private List<SelectListItem> PopulateUsers()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "select Code,Name from TfatPass where Locked='false' or Hide='false' order by Name ";
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


        #region EwayBill Functions

        public ActionResult GetTrasactMode()
        {
            List<SelectListItem> mList = new List<SelectListItem>();

            mList.Add(new SelectListItem { Text = "Road", Value = "1" });
            mList.Add(new SelectListItem { Text = "Rail", Value = "2" });
            mList.Add(new SelectListItem { Text = "Air", Value = "3" });
            mList.Add(new SelectListItem { Text = "Ship", Value = "4" });

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion

        // GET: Logistics/LRSetup
        public ActionResult Index(LRSetupVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, "Setup", mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;
            mModel.PrintFormats = PopulatePrintFormats();
            mModel.Users = PopulateUsers();

            var id = Convert.ToInt64(1);
            var mList = ctxTFAT.LRSetup.FirstOrDefault();
            if (mList != null)
            {
                if (mList.LRBoth == true)
                {
                    mModel.Both = true;
                }
                else if (mList.LRGenerate == true)
                {
                    mModel.LrAutomatic = true;
                }
                else
                {
                    mModel.LrManual = true;
                }

                mModel.FooterDetails1 = mList.FooterDetails1;
                mModel.FooterDetails2 = mList.FooterDetails2;
                mModel.FooterDetails3 = mList.FooterDetails3;
                mModel.FooterDetails4 = mList.FooterDetails4;
                mModel.TrnMode = mList.TrnMode;
                
                mModel.EditReq = mList.EditReq;
                mModel.DeleteReq = mList.DeleteReq;
                mModel.EditHours = Convert.ToInt32(mList.EditUptoHours);
                mModel.DeleteHours = Convert.ToInt32(mList.DeleteUptoHours);
                mModel.DispachLREdit = mList.DispatchLREditReq;
                mModel.BillLREdit = mList.LRBillEditReq;
                mModel.defaultQty = mList.defaultQty;
                mModel.GetAutoTripNo = mList.GetAutoTripNo;



                mModel.LRType = mList.LRType;
                mModel.LRType_Name = ctxTFAT.LRTypeMaster.Where(x => x.Code == mList.LRType).Select(x => x.LRType).FirstOrDefault();

                mModel.ServiceType = mList.ServiceType;
                mModel.ServiceType_Name = ctxTFAT.ServiceTypeMaster.Where(x => x.Code == mList.ServiceType).Select(x => x.ServiceType).FirstOrDefault();

                mModel.BillBranch = mList.BillBranch;
                mModel.BillBranch_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.BillBranch).Select(x => x.Name).FirstOrDefault();

                mModel.Unit = mList.Unit;
                mModel.Unit_Name = ctxTFAT.UnitMaster.Where(x => x.Code == mModel.Unit).Select(x => x.Name).FirstOrDefault();

                mModel.ChrType = mList.ChrType;
                mModel.ChrType_Name = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == mModel.ChrType).Select(x => x.ChargeType).FirstOrDefault();


                mModel.Declare_Value = mList.DeclareValue;
                mModel.GST = mList.GST;
                mModel.Eway_Bill = mList.EwayBill;

                mModel.GST_Ticklr = mList.GST_Ticklr;
                mModel.EWB_Ticklr = mList.EWB_Ticklr;

                mModel.Vehicle = mList.Vehicle;
                mModel.Party_Challan = mList.PartyChallan;
                mModel.Party_Invoice = mList.PartyInvoice;

                mModel.ParticularFlag = mList.ParticularFlag;

                mModel.DeclareValueZero = mList.DeclareValueZero;
                mModel.Charges = mList.Charges;

                mModel.Colln = mList.Colln;
                mModel.Del = mList.Del;

                mModel.FetchContract = mList.FetchContractbtn;
                mModel.GenralContract = mList.FetchGeneralContrct;
                mModel.PrintFormat = mList.DefaultPrint;
                mModel.User = mList.ExtraInfoTabAllowTo;

                mModel.Topay = mList.Topay;
                mModel.Paid = mList.Paid;
                mModel.TopayCustomer = (Consignor_Consignee)Enum.Parse(typeof(Consignor_Consignee), string.IsNullOrEmpty(mList.ToPayCustomer)==true?"0": mList.ToPayCustomer);
                mModel.PaidCustomer = (Consignor_Consignee)Enum.Parse(typeof(Consignor_Consignee), string.IsNullOrEmpty(mList.PaidCustomer)== true ? "0" : mList.PaidCustomer) ;
                mModel.DefaultToPay = mList.DefaultToPayCustomer;
                mModel.DefaultToPay_Name = ctxTFAT.Master.Where(x => x.Code == mModel.DefaultToPay).Select(x => x.Name).FirstOrDefault();
                mModel.DefaultPaid = mList.DefaultPaidCustomer;
                mModel.DefaultPaid_Name = ctxTFAT.Master.Where(x => x.Code == mModel.DefaultPaid).Select(x => x.Name).FirstOrDefault();
                mModel.ActWeightReq = mList.ActWeightReq;
                if (mList.ConsignerList == null)
                {
                    mList.ConsignerList = true;
                }

                if (Convert.ToBoolean(mList.ConsignerList))
                {
                    mModel.CurrentBranch = true;
                }
                else
                {
                    mModel.AllBranch = true;
                }

                mModel.CheckManualLR = mList.ManualLRCheck;
                mModel.FillFromCurr = mList.FillFromCurr==null?false: (bool)mList.FillFromCurr;

                mModel.Class_CurrDatetOnlyreq = mList.CurrDatetOnlyreq;
                mModel.Class_BackDateAllow = mList.BackDateAllow;
                mModel.Class_BackDaysUpto = mList.BackDaysUpto;
                mModel.Class_ForwardDateAllow = mList.ForwardDateAllow;
                mModel.Class_ForwardDaysUpto = mList.ForwardDaysUpto;
                mModel.Class_BranchwiseSrlReq = mList.BranchwiseSrlReq;
                mModel.Class_YearwiseSrlReq = mList.YearwiseSrlReq;
                mModel.Class_CetralisedSrlReq = mList.CetralisedSrlReq;
                mModel.Class_Srl = mList.Srl;

                mModel.YearwiseManualSrlReq = mList.YearwiseManualSrlReq;
                mModel.CetralisedManualSrlReq = mList.CetralisedManualSrlReq;
            }
            else
            {
                mModel.Party_Challan = true;
                mModel.Party_Invoice = true;
                mModel.FetchContract = true;
            }

            return View(mModel);
        }


        public ActionResult SaveData(LRSetupVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    LRSetup mobj = new LRSetup();
                    bool mAdd = true;
                    if (ctxTFAT.LRSetup.FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.LRSetup.FirstOrDefault();
                        mAdd = false;
                    }
                    if (mModel.Both == true)
                    {
                        mobj.LRBoth = true;
                        mobj.LRGenerate = false;
                    }
                    else if (mModel.LrAutomatic == true)
                    {
                        mobj.LRBoth = false;
                        mobj.LRGenerate = true;
                    }
                    else
                    {
                        mobj.LRBoth = false;
                        mobj.LRGenerate = false;
                    }

                    mobj.TrnMode = mModel.TrnMode;
                    mobj.EditUptoHours = mModel.EditHours;
                    mobj.DeleteUptoHours = mModel.DeleteHours;
                    mobj.EditReq = mModel.EditReq;
                    mobj.DeleteReq = mModel.DeleteReq;
                    mobj.DispatchLREditReq = mModel.DispachLREdit;
                    mobj.LRBillEditReq = mModel.BillLREdit;
                    mobj.defaultQty = mModel.defaultQty;

                    mobj.FooterDetails1 = mModel.FooterDetails1;
                    mobj.FooterDetails2 = mModel.FooterDetails2;
                    mobj.FooterDetails3 = mModel.FooterDetails3;
                    mobj.FooterDetails4 = mModel.FooterDetails4;

                    mobj.CurrDatetOnlyreq = mModel.Class_CurrDatetOnlyreq;
                    mobj.BackDateAllow = mModel.Class_BackDateAllow;
                    mobj.BackDaysUpto = mModel.Class_BackDaysUpto;
                    mobj.ForwardDateAllow = mModel.Class_ForwardDateAllow;
                    mobj.ForwardDaysUpto = mModel.Class_ForwardDaysUpto;
                    mobj.BranchwiseSrlReq = mModel.Class_BranchwiseSrlReq;
                    mobj.YearwiseSrlReq = mModel.Class_YearwiseSrlReq;
                    mobj.CetralisedSrlReq = mModel.Class_CetralisedSrlReq;
                    mobj.Srl = mModel.Class_Srl;

                    mobj.YearwiseManualSrlReq = mModel.YearwiseManualSrlReq;
                    mobj.CetralisedManualSrlReq = mModel.CetralisedManualSrlReq;

                    mobj.LRType = mModel.LRType;
                    mobj.ServiceType = mModel.ServiceType;
                    mobj.BillBranch = mModel.BillBranch;
                    mobj.Unit = mModel.Unit;
                    mobj.ChrType = mModel.ChrType;
                    mobj.ActWeightReq = mModel.ActWeightReq;

                    mobj.GetAutoTripNo = mModel.GetAutoTripNo;

                    mobj.FetchContractbtn = mModel.FetchContract;
                    mobj.FetchGeneralContrct = mModel.GenralContract;

                    mobj.DeclareValue = mModel.Declare_Value;
                    mobj.GST = mModel.GST;
                    mobj.EwayBill = mModel.Eway_Bill;

                    mobj.GST_Ticklr = mModel.GST_Ticklr;
                    mobj.EWB_Ticklr = mModel.EWB_Ticklr;

                    if (mModel.Vehicle == true)
                    {
                        mobj.Vehicle = true;
                    }
                    else
                    {
                        mobj.Vehicle = false;
                    }

                    mobj.ParticularFlag = mModel.ParticularFlag;

                    mobj.PartyChallan = mModel.Party_Challan;
                    mobj.PartyInvoice = mModel.Party_Invoice;

                    mobj.DeclareValueZero = mModel.DeclareValueZero;

                    mobj.Charges = mModel.Charges;

                    mobj.Colln = mModel.Colln;
                    mobj.Del = mModel.Del;

                    mobj.ToPayCustomer = mModel.TopayCustomer.ToString();
                    mobj.PaidCustomer = mModel.PaidCustomer.ToString();
                    mobj.DefaultToPayCustomer = mModel.DefaultToPay;
                    mobj.DefaultPaidCustomer = mModel.DefaultPaid;
                    mobj.Topay = mModel.Topay;
                    mobj.Paid = mModel.Paid;
                    if (mModel.CurrentBranch)
                    {
                        mobj.ConsignerList = true;
                    }
                    else
                    {
                        mobj.ConsignerList = false;
                    }
                    mobj.ManualLRCheck = mModel.CheckManualLR;
                    mobj.FillFromCurr = mModel.FillFromCurr;
                    mobj.ExtraInfoTabAllowTo = mModel.User;

                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    mobj.AUTHORISE = mauthorise;
                    mobj.AUTHIDS = muserid;
                    mobj.DefaultPrint = "";
                    ExecuteStoredProc("Update DocFormats set Selected='false' where type='LR000'");
                    if (!String.IsNullOrEmpty(mModel.PrintFormat))
                    {
                        mobj.DefaultPrint = mModel.PrintFormat;
                        var DefaultList = mModel.PrintFormat.Split(',').ToList();
                        var Formatlist = ctxTFAT.DocFormats.Where(x => DefaultList.Contains(x.FormatCode)).ToList();
                        foreach (var item in Formatlist)
                        {
                            item.Selected = true;
                            ctxTFAT.Entry(item).State = EntityState.Modified;
                        }
                    }

                    if (mAdd == true)
                    {
                        ctxTFAT.LRSetup.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }

                    DocTypes docTypes = ctxTFAT.DocTypes.Where(x => x.Code == "LR000").FirstOrDefault();
                    if (docTypes != null)
                    {
                        docTypes.GSTNoCommon = mModel.Class_BranchwiseSrlReq;
                        docTypes.CommonSeries = mModel.Class_YearwiseSrlReq;
                        docTypes.Centralised = mModel.Class_CetralisedSrlReq;
                        docTypes.LimitFrom = mModel.Class_Srl.ToString().Trim();
                        docTypes.LimitTo = "999999".PadLeft(docTypes.LimitFrom.Length, '9');
                        docTypes.DocWidth = docTypes.LimitFrom.Length;
                        ctxTFAT.Entry(docTypes).State = EntityState.Modified;
                    }


                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, "Setup", mModel.Header, "", DateTime.Now, 0, "", "Save Lorry Receipt Setup", "NA");
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

        public ActionResult DeleteStateMaster(LRSetupVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Charge Type not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var id = Convert.ToInt64(mModel.Document);
            var mList = ctxTFAT.LRSetup.Where(x => (x.RECORDKEY == id)).FirstOrDefault();
            ctxTFAT.LRSetup.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }
    }
}