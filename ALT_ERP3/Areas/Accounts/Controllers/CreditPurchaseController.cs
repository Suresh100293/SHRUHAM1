using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Controllers;
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class CreditPurchaseController : BaseController
    {
        public static int mfilecount;
        string attachmentPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentPath"].ToString()) == true ? "" : ConfigurationManager.AppSettings["AttachmentPath"].ToString();
        private static string mauthorise = "A00";
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

        #region Item History 

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
            return GetGridDataColumns(id, "L", "XXXX");
        }
        public ActionResult GetGridData(GridOption Model)
        {
            return GetGridReport(Model, "M", "Code^" + Model.Code, false, 0);
        }

        #endregion

        #region Call Dependency Return Functions List

        public ActionResult GetItemMasters(string term, string ItemGroup)
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            if (term == "" || term == null)
            {
                var result = ctxTFAT.ItemMaster.Where(x => x.BaseGr == ItemGroup).ToList().Take(10);
                var Modified = result.Select(x => new
                {
                    Code = x.Code,
                    Name = x.Name
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.ItemMaster.Where(x => x.BaseGr == ItemGroup && x.Name.Contains(term)).ToList().Take(10);
                var Modified = result.Select(x => new
                {
                    Code = x.Code,
                    Name = x.Name
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetTDSList(string term)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.TDSMaster.Select(c => new { c.Code, c.Name }).Distinct().ToList().Take(10);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var result = ctxTFAT.TDSMaster.Where(x => x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).Distinct().ToList().Take(10);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetHSNList(string term)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.HSNMaster.Select(c => new { c.Code, c.Name }).Distinct().ToList().Take(10);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var result = ctxTFAT.HSNMaster.Where(x => x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).Distinct().ToList().Take(10);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetTDSRateDetail(CreditPurchaseVM Model)
        {
            var CurrDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            var TDSRate = ctxTFAT.TDSRates.Where(x => x.Code.ToString() == Model.TDSCode && x.EffDate <= CurrDate).OrderByDescending(x => x.EffDate).Select(x => new { x.TDSRate/*, x.Cess, x.SurCharge, x.SHECess*/ }).FirstOrDefault();
            if (TDSRate != null)
            {
                Model.TDSRate = (TDSRate.TDSRate == null) ? 0 : Convert.ToDecimal(TDSRate.TDSRate.Value);
            }
            else
            {
                Model.TDSRate = 0;
            }
            return Json(Model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetGSTList(string term)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.HSNMaster.Select(x => new { x.Code, x.Name, Scope = "" }).ToList().Take(10);
                var result1 = ctxTFAT.TaxMaster.Where(x => x.VATGST == true).Select(x => new { x.Code, x.Name, x.Scope }).ToList().Take(10);

                List<SelectListItem> selectListItems = new List<SelectListItem>();
                foreach (var item in result)
                {
                    selectListItems.Add(new SelectListItem { Text = "[" + item.Name + "]", Value = item.Code });
                }
                foreach (var item in result1)
                {
                    selectListItems.Add(new SelectListItem { Text = "[" + item.Name + "][" + item.Scope + "]", Value = item.Code });
                }

                var Modified = selectListItems.Select(x => new
                {
                    Code = x.Value,
                    Name = x.Text
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.HSNMaster.Where(x => x.Name.ToLower().Contains(term.ToLower())).Select(m => new { m.Code, m.Name }).ToList().Take(10);
                var result1 = ctxTFAT.TaxMaster.Where(x => x.VATGST == true && x.Name.ToLower().Contains(term.ToLower())).Select(x => new { x.Code, x.Name, x.Scope }).ToList().Take(10);

                List<SelectListItem> selectListItems = new List<SelectListItem>();
                foreach (var item in result)
                {
                    selectListItems.Add(new SelectListItem { Text = "[" + item.Name + "]", Value = item.Code });
                }
                foreach (var item in result1)
                {
                    selectListItems.Add(new SelectListItem { Text = "[" + item.Name + "][" + item.Scope + "]", Value = item.Code });
                }
                var Modified = selectListItems.Select(x => new
                {
                    Code = x.Value,
                    Name = x.Text
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetGSTRateDetail(string Code, string Party, string DocDate)
        {
            var CurrDate = ConvertDDMMYYTOYYMMDD(DocDate);
            var pstate = ctxTFAT.Address.Where(x => x.Code == Party).Select(x => x.State).FirstOrDefault();
            if (String.IsNullOrEmpty(pstate))
            {
                pstate = "19";
            }
            var branchstate = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.State).FirstOrDefault();
            if (pstate == branchstate)
            {
                if (ctxTFAT.HSNMaster.Where(x => x.Code == Code).FirstOrDefault() != null)
                {
                    var result = ctxTFAT.HSNRates.Where(x => x.Code == Code && x.EffDate <= CurrDate).OrderByDescending(x => x.EffDate).Select(x => new { IGSTRate = (decimal)0, SGSTRate = x.SGSTRate.Value, CGSTRate = x.CGSTRate.Value }).FirstOrDefault();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = ctxTFAT.TaxMaster.Where(x => x.Code == Code).Select(x => new { IGSTRate = (decimal)0, SGSTRate = x.SGSTRate.Value, CGSTRate = x.CGSTRate.Value }).FirstOrDefault();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                if (ctxTFAT.HSNMaster.Where(x => x.Code == Code).FirstOrDefault() != null)
                {
                    var result = ctxTFAT.HSNRates.Where(x => x.Code == Code && x.EffDate <= CurrDate).OrderByDescending(x => x.EffDate).Select(x => new { IGSTRate = x.IGSTRate.Value, SGSTRate = (decimal)0, CGSTRate = (decimal)0 }).FirstOrDefault();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = ctxTFAT.TaxMaster.Where(x => x.Code == Code).Select(x => new { IGSTRate = x.IGSTRate.Value, SGSTRate = (decimal)0, CGSTRate = (decimal)0 }).FirstOrDefault();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult GetAccountDetails(CreditPurchaseVM Model)
        {
            var CurrDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            var taxdetails = ctxTFAT.TaxDetails.Where(x => x.Code == Model.BankCashCode).Select(x => new { x.TDSCode, x.CutTDS, x.TDSRate }).FirstOrDefault();
            var CutTDS = taxdetails == null ? false : taxdetails.CutTDS;

            var TDSFlagSetup = ctxTFAT.OtherTransactSetup.Select(x => x.CutTDS).FirstOrDefault();
            if (TDSFlagSetup == false)
            {
                CutTDS = false;
            }
            var TDSCode = taxdetails == null ? 0 : taxdetails.TDSCode == null ? 0 : taxdetails.TDSCode.Value;
            decimal TDSRate = 0;
            TDSRate = ctxTFAT.TDSRates.Where(x => x.Code == TDSCode && x.EffDate <= CurrDate).OrderByDescending(x => x.EffDate).Select(x => x.TDSRate ?? 0).FirstOrDefault();
            var TDSName = ctxTFAT.TDSMaster.Where(x => x.Code == TDSCode).Select(x => x.Name).FirstOrDefault();
            return Json(new
            {
                CutTDS = CutTDS,
                TDSCode = TDSCode,
                TDSRate = TDSRate,
                TDSName = TDSName,


            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetRelatedChoiceToList(string term)
        {
            string[] DefaultCost = new string[] { "000100949", "000100950", "000100811", "000100343", "000100736", "000100653", "000100326", "000100951", "000100952", "000100404", "000100781", "000100953", "000100954", "000100788", "000101135", "000100789", "000100346", "000100341", "000100345" };
            DefaultCost = new string[2] { "##", "###" };

            List<SelectListItem> GSt = new List<SelectListItem>();
            if (term == "" || term == null)
            {
                var mVehicles = ctxTFAT.Master.Where(x => x.BaseGr != "D" && (!DefaultCost.Contains(x.Code))).OrderBy(n => n.Name).ToList().Take(10);
                foreach (var item in mVehicles)
                {
                    GSt.Add(new SelectListItem { Value = item.Code, Text = item.Name });
                }

                GSt.Add(new SelectListItem { Value = "Tyrestock", Text = "Tyre Stock" });
                GSt.Add(new SelectListItem { Value = "Remould", Text = "Remould" });

                var Modified = GSt.Select(x => new
                {
                    Code = x.Value,
                    Name = x.Text
                });

                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var mVehicles = ctxTFAT.Master.Where(x => x.BaseGr != "D" && (!DefaultCost.Contains(x.Code)) && x.Name.Contains(term)).OrderBy(n => n.Name).ToList().Take(10);

                foreach (var item in mVehicles)
                {
                    GSt.Add(new SelectListItem { Value = item.Code, Text = item.Name });
                }

                GSt.Add(new SelectListItem { Value = "Tyrestock", Text = "Tyre Stock" });
                GSt.Add(new SelectListItem { Value = "Remould", Text = "Remould" });

                var Modified = GSt.Select(x => new
                {
                    Code = x.Value,
                    Name = x.Text
                });

                return Json(Modified, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetRelatedToList(string term)
        {

            List<SelectListItem> GSt = new List<SelectListItem>();

            string[] DefaultCost = new string[] { "000100949", "000100950", "000100811", "000100343", "000100736", "000100653", "000100326", "000100951", "000100952", "000100404", "000100781", "000100953", "000100954", "000100788", "000101135", "000100789", "000100346", "000100341", "000100345" };
            if (term == "" || term == null)
            {
                var mVehicles = ctxTFAT.Master.Where(x => (DefaultCost.Contains(x.Code))).OrderBy(n => n.Name).ToList();
                foreach (var item in mVehicles)
                {
                    GSt.Add(new SelectListItem { Value = item.Code, Text = item.Name });
                }
                var Modified = GSt.Select(x => new
                {
                    Code = x.Value,
                    Name = x.Text
                });

                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var mVehicles = ctxTFAT.Master.Where(x => (DefaultCost.Contains(x.Code)) && x.Name.Contains(term)).OrderBy(n => n.Name).ToList();

                foreach (var item in mVehicles)
                {
                    GSt.Add(new SelectListItem { Value = item.Code, Text = item.Name });
                }
                var Modified = GSt.Select(x => new
                {
                    Code = x.Value,
                    Name = x.Text
                });

                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetCashBankList(string term)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.Master.Where(x => (x.BaseGr == "S") && x.Hide == false).Select(c => new { c.Code, c.Name }).Distinct().ToList().Take(10);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var result = ctxTFAT.Master.Where(x => x.Name.Contains(term) && (x.BaseGr == "S") && x.Hide == false).Select(m => new { m.Code, m.Name }).Distinct().ToList().Take(10);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetAccountList(string term/*, string BaseGr*/)
        {
            if (term == "" || term == null)
            {
                var result = ctxTFAT.Master.Where(x => x.BaseGr != "S" && x.BaseGr != "D").Select(m => new
                {
                    m.Code,
                    m.Name
                }).OrderBy(n => n.Name).ToList().Take(10);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.Master.Where(x => x.Name.Contains(term) /*&& (x.BaseGr != "S" && x.BaseGr != "D")*/).Select(m => new
                {
                    m.Code,
                    m.Name
                }).OrderBy(n => n.Name).ToList().Take(10);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public List<CreditPurchaseVM> GetAttachmentListInEdit(string TableKey)
        {
            List<CreditPurchaseVM> AttachmentList = new List<CreditPurchaseVM>();
            var docdetail = ctxTFAT.Attachment.Where(x => x.TableKey == TableKey).ToList();
            foreach (var item in docdetail)
            {
                AttachmentList.Add(new CreditPurchaseVM()
                {
                    FileName = Path.GetFileName(item.FilePath),
                    Srl = item.Srl,
                    Code = item.Code,
                    TableKey = item.TableKey,
                    ParentKey = item.ParentKey,
                    Type = item.Type,
                    tempId = item.Sno,
                    SrNo = item.Sno,
                    Path = item.FilePath,
                    FileContent = Path.GetExtension(item.FilePath),
                    ImageStr = Convert.ToBase64String(System.IO.File.ReadAllBytes(item.FilePath)),
                    tempIsDeleted = false
                });
            }
            return AttachmentList;
        }

        public List<CreditPurchaseVM> GetLRDetailList(string TableKey)
        {
            List<CreditPurchaseVM> objledgerdetail2 = new List<CreditPurchaseVM>();
            var LRDetailList = ctxTFAT.RelLr.Where(x => (x.Branch + x.TableKey).ToString().Trim() == TableKey).Select(x => x).ToList();
            foreach (var a in LRDetailList)
            {
                objledgerdetail2.Add(new CreditPurchaseVM()
                {
                    LRNumber = a.LrNo,
                    LRAmt = a.LrAmt.Value,
                    tempId = a.SrNo,
                    ConsignmentKey = a.LRRefTablekey,
                });

            }
            return objledgerdetail2;
        }

        public List<CreditPurchaseVM> GetFMDetailList(string TableKey)
        {
            List<CreditPurchaseVM> objledgerdetail2 = new List<CreditPurchaseVM>();
            var LRDetailList = ctxTFAT.RelFm.Where(x => x.TableKey == TableKey).Select(x => x).ToList();
            foreach (var a in LRDetailList)
            {
                objledgerdetail2.Add(new CreditPurchaseVM()
                {
                    FMNumber = a.FMNo,
                    FMAmt = a.FmAmt,
                    tempId = a.SrNo,
                    FreightMemoKey = a.FMRefTablekey,
                });

            }
            return objledgerdetail2;
        }

        public List<AddOns> GetTruckWiseData(string TableKey, string RelatedTo, bool AccRelatedAc, string Code)
        {
            var mRelateData = ctxTFAT.RelateData.Where(x => x.TableKey == TableKey).Select(x => x).FirstOrDefault();
            List<AddOns> truckaddonlist = new List<AddOns>();

            //if (mRelateData.ItemReq == false)
            {
                //Trip Details
                if (Code == "000100341" || RelatedTo == "000100341")
                {
                    if (mRelateData.Date1 != null)
                    {
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F001",
                            Head = "Trip Start Date",
                            ApplCode = mRelateData.Date1.Value.ToString("yyyy-MM-dd"),
                            QueryText = "",
                            QueryCode = "",
                            FldType = "D"

                        });
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F002",
                            Head = "Trip End Date",
                            ApplCode = mRelateData.Date2.Value.ToString("yyyy-MM-dd"),
                            QueryText = "",
                            QueryCode = "",
                            FldType = "D"

                        });
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F003",
                            Head = "Trip Days",
                            ApplCode = mRelateData.Num1.Value.ToString(),
                            QueryText = "",
                            QueryCode = "",
                            FldType = "N"

                        });
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F004",
                            Head = "Starting kms",
                            ApplCode = mRelateData.Value1,
                            QueryText = "",
                            QueryCode = "",
                            FldType = "N"

                        });
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F005",
                            Head = "Ending kms",
                            ApplCode = mRelateData.Value2,
                            QueryText = "",
                            QueryCode = "",
                            FldType = "N"

                        });
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F006",
                            Head = "Charge KMS",
                            ApplCode = mRelateData.Num2.Value.ToString(),
                            QueryText = "",
                            QueryCode = "",
                            FldType = "N"

                        });
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F007",
                            Head = "Per KM Rate",
                            ApplCode = mRelateData.Num3.Value.ToString(),
                            QueryText = "",
                            QueryCode = "",
                            FldType = "N"

                        });
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F008",
                            Head = "Trip Charges",
                            ApplCode = mRelateData.Num4.Value.ToString(),
                            QueryText = "",
                            QueryCode = "",
                            FldType = "N"

                        });
                    }
                }

                //Tyre Details
                else if (Code == "000100345" || RelatedTo == "000100345")
                {

                    var mTyreTypes = ctxTFAT.ItemMaster.Where(x => x.BaseGr == mRelateData.Char1).Select(x => new { x.Name, x.Code }).Distinct().ToList();
                    var mItemGroup = ctxTFAT.ItemGroups.Where(x => x.Hide == false).Select(x => new { x.Name, x.Code }).Distinct().ToList();

                    List<string> tyretypeList = new List<string>();
                    tyretypeList.Add("New");
                    tyretypeList.Add("Remould");
                    tyretypeList.Add("Scrap");
                    tyretypeList.Add("Sale");
                    tyretypeList.Add("OutOfStock");
                    tyretypeList.Add("Stock");
                    if (!String.IsNullOrEmpty(mRelateData.Value4))
                    {
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F015",
                        //    Head = "Product Group",
                        //    ApplCode = mRelateData.Char1,
                        //    QueryText = "Select^" + String.Join("^", mItemGroup.Select(x => x.Name).ToList()),
                        //    QueryCode = "Select^" + String.Join("^", mItemGroup.Select(x => x.Code).ToList()),
                        //    FldType = "C",
                        //});
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F001",
                        //    Head = "Pick From",
                        //    ApplCode = mRelateData.Value1,
                        //    QueryText = "Master^Direct",
                        //    QueryCode = "Master^Direct",
                        //    FldType = "C"

                        //});
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F002",
                        //    Head = "Product",
                        //    ApplCode = mRelateData.Value2,
                        //    QueryText = "Select^" + String.Join("^", mTyreTypes.Select(x => x.Name).ToList()),
                        //    QueryCode = "Select^" + String.Join("^", mTyreTypes.Select(x => x.Code).ToList()),
                        //    FldType = mRelateData.Value1 == "Master" ? "C" : "T",
                        //});
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F003",
                        //    Head = "Cost",
                        //    ApplCode = mRelateData.Num1.Value.ToString(),
                        //    QueryText = "",
                        //    QueryCode = "",
                        //    FldType = "N"

                        //});
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F001",
                            Head = "Tyre Type",
                            ApplCode = mRelateData.Value7,
                            QueryText = "Select^" + String.Join("^", tyretypeList),
                            QueryCode = "Select^" + String.Join("^", tyretypeList),
                            FldType = "C",
                        });
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F004",
                        //    Head = "Warranty KM",
                        //    ApplCode = mRelateData.Num4 == null ? "0" : mRelateData.Num4.Value.ToString(),
                        //    QueryText = "",
                        //    FldType = "N"

                        //});

                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F002",
                            Head = "Install For",
                            ApplCode = mRelateData.Value4,
                            QueryText = "Tyre^Stepnee",
                            FldType = "R"

                        });
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F003",
                            Head = "Tyre Placed No",
                            ApplCode = mRelateData.Value5,
                            QueryText = "",
                            FldType = "N"

                        });
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F004",
                            Head = "Tyre SerialNo",
                            ApplCode = mRelateData.Value6,
                            QueryText = "",
                            FldType = "T"

                        });
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F009",
                        //    Head = "Mfg Date",
                        //    ApplCode = mRelateData.Date1.Value.ToString("yyyy-MM-dd"),
                        //    QueryText = "",
                        //    FldType = "D"

                        //});
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F010",
                        //    Head = "Install / Received Date",
                        //    ApplCode = mRelateData.Date3.Value.ToString("yyyy-MM-dd"),
                        //    QueryText = "",
                        //    FldType = "D"

                        //});
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F011",
                        //    Head = "Install KM",
                        //    ApplCode = mRelateData.Num2.Value.ToString(),
                        //    QueryText = "",
                        //    FldType = "N"

                        //});
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F005",
                        //    Head = "Expiry Days",
                        //    ApplCode = mRelateData.Value3,
                        //    QueryText = "",
                        //    FldType = "N",
                        //    

                        //});
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F012",
                        //    Head = "Expiry Date",
                        //    ApplCode = mRelateData.Date2 == null ? "" : mRelateData.Date2.Value.ToString("yyyy-MM-dd"),
                        //    QueryText = "",
                        //    FldType = "D",
                        //    

                        //});
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F013",
                        //    Head = "Expiry KM",
                        //    ApplCode = mRelateData.Num3 == null ? "0" : mRelateData.Num3.Value.ToString(),
                        //    QueryText = "",
                        //    FldType = "N"
                        //});
                    }
                }

                //Diesel Details
                else if (Code == "000100343" || RelatedTo == "000100343")
                {
                    //if (!String.IsNullOrEmpty(mRelateData.Value1))
                    {
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F001",
                            Head = "Diesel Receipt No",
                            ApplCode = mRelateData.Value1,
                            QueryText = "",
                            FldType = "T"
                        });
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F002",
                            Head = "Litres",
                            ApplCode = mRelateData.Value2,
                            QueryText = "",
                            FldType = "N"
                        });
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F003",
                        //    Head = "KM",
                        //    ApplCode = mRelateData.Num1 == null ? "0" : mRelateData.Num1.Value.ToString(),
                        //    QueryText = "",
                        //    FldType = "N"
                        //});

                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F004",
                        //    Head = "Date",
                        //    ApplCode = mRelateData.Date1 == null ? DateTime.Now.ToString("yyyy-MM-dd") : mRelateData.Date1.Value.ToString("yyyy-MM-dd"),
                        //    QueryText = "",
                        //    FldType = "D"
                        //});
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F005",
                        //    Head = "Amount",
                        //    ApplCode = mRelateData.Num2 == null ? "0" : mRelateData.Num2.Value.ToString(),
                        //    QueryText = "",
                        //    FldType = "T"
                        //});
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F003",
                            Head = "Approved Diesel",
                            ApplCode = mRelateData.Value3,
                            QueryText = "",
                            FldType = "T"
                        });
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F004",
                            Head = "Approver Name",
                            ApplCode = mRelateData.Value4,
                            QueryText = "",
                            FldType = "T"
                        });
                    }
                }
                //Loan(Details) || Permit(1 Year ) || Permit(5 Year ) || Fitness || PUC ||  PT || Green Tax || T P (Transit Pass)
                else if (Code == "000100951" || RelatedTo == "000100951" || Code == "000100781" || RelatedTo == "000100781" || Code == "000100953" || RelatedTo == "000100953" || Code == "000100736" || RelatedTo == "000100736" || Code == "000100789" || RelatedTo == "000100789" || Code == "000100788" || RelatedTo == "000100788" || Code == "000100653" || RelatedTo == "000100653" || Code == "000100811" || RelatedTo == "000100811" || Code == "000101135" || RelatedTo == "000101135")
                {
                    if (mRelateData.Date1 != null)
                    {
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F001",
                            Head = "From Date",
                            ApplCode = mRelateData.Date1.Value.ToString("yyyy-MM-dd"),
                            QueryText = "",
                            FldType = "D"
                        });
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F002",
                            Head = "To Date",
                            ApplCode = mRelateData.Date2.Value.ToString("yyyy-MM-dd"),
                            QueryText = "",
                            FldType = "D"
                        });
                    }
                }

                //Insurance Details
                else if (Code == "000100326" || RelatedTo == "000100326")
                {
                    if (mRelateData.Date1 != null)
                    {
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F001",
                            Head = "From Date",
                            ApplCode = mRelateData.Date1.Value.ToString("yyyy-MM-dd"),
                            QueryText = "",
                            FldType = "D"
                        });
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F002",
                            Head = "To Date",
                            ApplCode = mRelateData.Date2.Value.ToString("yyyy-MM-dd"),
                            QueryText = "",
                            FldType = "D"
                        });
                    }
                }
                //Accident Details
                if (Code == "000100949" || RelatedTo == "000100949")
                {
                    if (mRelateData.Date1 != null)
                    {
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F001",
                            Head = "Accident Date",
                            ApplCode = mRelateData.Date1.Value.ToString("yyyy-MM-dd"),
                            QueryText = "",
                            FldType = "D"
                        });
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F002",
                            Head = "Amount",
                            ApplCode = mRelateData.Num1.Value.ToString(),
                            QueryText = "",
                            FldType = "T"
                        });
                    }

                }

                // Spare Parts & Repairs Details
                else if (Code == "000100346" || RelatedTo == "000100346")
                {
                    var mSparePart = ctxTFAT.ItemMaster.Where(x => x.BaseGr == mRelateData.Char1).Select(x => new { x.Name, x.Code }).Distinct().ToList();
                    var mItemGroup = ctxTFAT.ItemGroups.Where(x => x.Hide == false).Select(x => new { x.Name, x.Code }).Distinct().ToList();

                    if (!String.IsNullOrEmpty(mRelateData.Value5))
                    {
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F012",
                        //    Head = "Product Group",
                        //    ApplCode = mRelateData.Char1,
                        //    QueryText = "Select^" + String.Join("^", mItemGroup.Select(x => x.Name).ToList()),
                        //    QueryCode = "Select^" + String.Join("^", mItemGroup.Select(x => x.Code).ToList()),
                        //    FldType = "C",
                        //});
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F001",
                        //    Head = "Pick From",
                        //    ApplCode = mRelateData.Value1,
                        //    QueryText = "Select^" + String.Join("^", "Master^Direct"),
                        //    QueryCode = "Select^" + String.Join("^", "Master^Direct"),
                        //    FldType = "C"
                        //});
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F002",
                        //    Head = "Spare Part",
                        //    ApplCode = mRelateData.Value2,
                        //    QueryText = "Select^" + String.Join("^", mSparePart.Select(x => x.Name).ToList()),
                        //    QueryCode = "Select^" + String.Join("^", mSparePart.Select(x => x.Code).ToList()),
                        //    FldType = mRelateData.Value1 == "Direct" ? "T" : "C",
                        //});
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F003",
                        //    Head = "Cost",
                        //    ApplCode = mRelateData.Num1.Value.ToString(),
                        //    QueryText = "",
                        //    FldType = "N",
                        //    InActive = true
                        //});
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F013",
                        //    Head = "Qty",
                        //    ApplCode = mRelateData.Char2,
                        //    QueryText = "",
                        //    QueryCode = "",
                        //    FldType = "N",
                        //    InActive = true
                        //});
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F004",
                        //    Head = "Warranty KM",
                        //    ApplCode = mRelateData.Value3.ToString(),
                        //    QueryText = "",
                        //    FldType = "N"
                        //});

                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F001",
                            Head = "Receipt No",
                            ApplCode = mRelateData.Value5,
                            QueryText = "",
                            FldType = "T"
                        });
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F002",
                            Head = "Receipt Dt",
                            ApplCode = mRelateData.Date1.Value.ToString("yyyy-MM-dd"),
                            QueryText = "",
                            FldType = "D"
                        });
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F008",
                        //    Head = "Install / Received Date",
                        //    ApplCode = mRelateData.Date3.Value.ToString("yyyy-MM-dd"),
                        //    QueryText = "",
                        //    FldType = "D"
                        //});
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F009",
                        //    Head = "Current KM",
                        //    ApplCode = mRelateData.Num2.Value.ToString(),
                        //    QueryText = "",
                        //    FldType = "N"
                        //});
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F005",
                        //    Head = "Warranty Days",
                        //    ApplCode = mRelateData.Value4.ToString(),
                        //    QueryText = "",
                        //    FldType = "N",
                        //    
                        //});
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F010",
                        //    Head = "Due Date",
                        //    ApplCode = mRelateData.Date2 == null ? "" : mRelateData.Date2.Value.ToString("yyyy-MM-dd"),
                        //    QueryText = "",
                        //    FldType = "D",
                        //    
                        //});
                        //truckaddonlist.Add(new AddOns()
                        //{
                        //    Fld = "F011",
                        //    Head = "Due km",
                        //    ApplCode = mRelateData.Num3.Value.ToString(),
                        //    QueryText = "",
                        //    FldType = "N"
                        //});
                    }
                }

                //Body Building Details
                else if (Code == "000100950" || RelatedTo == "000100950")
                {
                    if (mRelateData.Date1 != null)
                    {
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F001",
                            Head = "Date",
                            ApplCode = mRelateData.Date1.Value.ToString("yyyy-MM-dd"),
                            QueryText = "",
                            FldType = "D"

                        });
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F002",
                            Head = "Amount",
                            ApplCode = mRelateData.Num1.Value.ToString(),
                            QueryText = "",
                            FldType = "T"

                        });
                    }
                }

                //Others || Miscellaneous Charges
                else if (Code == "000100404" || RelatedTo == "000100404" || Code == "000100952" || RelatedTo == "000100952")
                {
                    if (mRelateData.Num1 != null)
                    {
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F001",
                            Head = "Amount",
                            ApplCode = mRelateData.Num1.Value.ToString(),
                            QueryText = "",
                            FldType = "T"

                        });
                    }
                }

                //Principal Amount
                else if (Code == "000100954" || RelatedTo == "000100954")
                {
                    if (mRelateData.Date1 != null)
                    {
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F001",
                            Head = "Date",
                            ApplCode = mRelateData.Date1.Value.ToString("yyyy-MM-dd"),
                            QueryText = "",
                            FldType = "D"

                        });
                        truckaddonlist.Add(new AddOns()
                        {
                            Fld = "F002",
                            Head = "Amount",
                            ApplCode = mRelateData.Num1.Value.ToString(),
                            QueryText = "",
                            FldType = "T"

                        });
                    }
                }
            }
            return truckaddonlist;
        }

        public List<AddOns> GetItemWiseData(string TableKey)
        {
            var mRelateData = ctxTFAT.RelateDataItem.Where(x => x.TableKey == TableKey).Select(x => x).FirstOrDefault();
            List<AddOns> truckaddonlist = new List<AddOns>();

            if (mRelateData != null)
            {
                var mItem = ctxTFAT.ItemMaster.Where(x => x.BaseGr == mRelateData.ProductGroup).Select(x => new { x.Name, x.Code }).Distinct().OrderBy(x => x.Name).ToList();
                var mItemGroup = ctxTFAT.ItemGroups.Where(x => x.Hide == false).Select(x => new { x.Name, x.Code }).Distinct().ToList();
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "Product Group",
                    ApplCode = mRelateData.ProductGroup,
                    QueryText = "Select^" + String.Join("^", mItemGroup.Select(x => x.Name).ToList()),
                    QueryCode = "Select^" + String.Join("^", mItemGroup.Select(x => x.Code).ToList()),
                    FldType = "C",
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F002",
                    Head = "Pick From",
                    ApplCode = "Master",
                    QueryText = "Master^Direct",
                    QueryCode = "Master^Direct",
                    FldType = "C",
                    Hide = true
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F003",
                    Head = "Item",
                    ApplCode = mRelateData.Item,
                    QueryText = "Select^" + String.Join("^", mItem.Select(x => x.Name).ToList()),
                    QueryCode = "Select^" + String.Join("^", mItem.Select(x => x.Code).ToList()),
                    FldType = "C"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F004",
                    Head = "Cost",
                    ApplCode = mRelateData.Cost.ToString(),
                    QueryText = "",
                    QueryCode = "",
                    FldType = "N",
                    InActive = true
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F005",
                    Head = "Qty",
                    ApplCode = mRelateData.Qty,
                    QueryText = "",
                    QueryCode = "",
                    FldType = "N",
                    InActive = true
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F006",
                    Head = "Total Amount",
                    ApplCode = mRelateData.TotalAmout.ToString(),
                    QueryText = "",
                    QueryCode = "",
                    FldType = "N",
                    InActive = true
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F007",
                    Head = "Warranty KM",
                    ApplCode = mRelateData.WarrantyKm,
                    QueryText = "",
                    FldType = "N"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F008",
                    Head = "Install KM",
                    ApplCode = mRelateData.CurrentKM.ToString(),
                    QueryText = "",
                    FldType = "N",

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F009",
                    Head = "Due km",
                    ApplCode = mRelateData.DueKM.ToString(),
                    QueryText = "",
                    FldType = "N"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F010",
                    Head = "Warranty Days",
                    ApplCode = mRelateData.WarrantyDays.ToString(),
                    QueryText = "",
                    FldType = "N",

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F011",
                    Head = "MFG Date",
                    ApplCode = (string.IsNullOrEmpty(mRelateData.MFGDate) == true) ? Convert.ToDateTime("1900-01-01").ToString("yyyy-MM-dd") : mRelateData.MFGDate.ToString(),
                    QueryText = "",
                    FldType = "D",
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F012",
                    Head = "Install / Received Date",
                    ApplCode = (string.IsNullOrEmpty(mRelateData.InstallDate) == true) ? Convert.ToDateTime("1900-01-01").ToString("yyyy-MM-dd") : mRelateData.InstallDate.ToString(),
                    QueryText = "",
                    FldType = "D"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F013",
                    Head = "Due Date",
                    ApplCode = (string.IsNullOrEmpty(mRelateData.DueDate) == true) ? Convert.ToDateTime("1900-01-01").ToString("yyyy-MM-dd") : mRelateData.DueDate.ToString(),
                    QueryText = "",
                    FldType = "D",

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F014",
                    Head = "HSN CODE",
                    ApplCode = mRelateData.HSNCode,
                    QueryText = "",
                    FldType = "X",
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F015",
                    Head = "Description",
                    ApplCode = mRelateData.Description,
                    QueryText = "",
                    FldType = "M",
                });
            }
            return truckaddonlist;
        }

        public List<CreditPurchaseVM> GetTyreStockSerialList(string TableKey, string Branch)
        {
            List<CreditPurchaseVM> mTyredetails = new List<CreditPurchaseVM>();

            var GetParentkey = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == TableKey && x.Branch == Branch).Select(x => x.ParentKey).FirstOrDefault();

            var mTyrestocks = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == GetParentkey).Select(x => x).OrderBy(x => x.RECORDKEY).FirstOrDefault();

            var Count = ctxTFAT.TyreStockSerial.Where(x => x.ParentKey == GetParentkey && x.TableKey != TableKey).Select(x => x).ToList().Count();

            var LatestmTyrestocks = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == GetParentkey && x.ParentKey == TableKey && x.Branch == Branch).Select(x => x).OrderByDescending(x => x.RECORDKEY).FirstOrDefault();

            if (mTyrestocks != null)
            {
                var a = mTyrestocks;
                mTyredetails.Add(new CreditPurchaseVM()
                {
                    Name = ctxTFAT.VehicleMaster.Where(x => x.PostAc == a.Vehicle).Select(x => x.TruckNo).FirstOrDefault(),
                    Branch = LatestmTyrestocks.Branch,
                    DocuDate = a.Value1,
                    ActWt = Convert.ToDouble(a.Value2),
                    FEndDate = a.Value3,
                    ChgWt = Convert.ToDouble(a.Value4),
                    RECORDKEY = a.RECORDKEY,
                    ApplCode = LatestmTyrestocks.Status,
                    Srl = a.SerialNo,
                    Code = a.Vehicle,
                    TableKey = LatestmTyrestocks.TableKey,
                    ParentKey = LatestmTyrestocks.ParentKey,
                    StockAt = LatestmTyrestocks.StockAt,
                    StepneeNo = a.StepneeNo,
                    TyreNo = a.TyreNo,
                    IsActive = a.IsActive,
                    AuthLock = LatestmTyrestocks.IsActive == false ? true : false,
                });
            }
            return mTyredetails;
        }

        public string GetTripNewCode()
        {
            var mName = ctxTFAT.TripSheetMaster.Where(x => x.Prefix == mperiod).OrderByDescending(X => X.RECORDKEY).Select(X => X.DocNo).FirstOrDefault();
            if (String.IsNullOrEmpty(mName))
            {
                var DocType = ctxTFAT.DocTypes.Where(x => x.Code == "Trip0").Select(x => new { x.DocWidth, x.LimitFrom }).FirstOrDefault();
                mName = DocType.LimitFrom;
            }
            else
            {
                mName = (Convert.ToInt32(mName) + 1).ToString("D6");
            }
            return mName;
        }

        [HttpPost]
        public ActionResult GetTotal(CreditPurchaseVM Model)
        {
            decimal TdsAmt = 0, IGST = 0, SGST = 0, CGST = 0;
            Model.Taxable = Model.Taxable;
            if (Model.TDSFlag)
            {
                Model.TDSAmt = (Model.Taxable * Model.TDSRate) / 100;
            }
            else
            {
                Model.TDSAmt = 0;
            }
            if (Model.GSTFlag)
            {
                Model.IGSTAmt = Math.Round((Model.Taxable * Model.IGSTRate) / 100, 2);
                Model.SGSTAmt = Math.Round((Model.Taxable * Model.SGSTRate) / 100, 2);
                Model.CGSTAmt = Math.Round((Model.Taxable * Model.CGSTRate) / 100, 2);
            }
            else
            {
                Model.IGSTAmt = 0;
                Model.SGSTAmt = 0;
                Model.CGSTAmt = 0;
            }
            decimal mamtm = Math.Round(Model.Taxable + Model.IGSTAmt + Model.SGSTAmt + Model.CGSTAmt, 2);

            return Json(new
            {
                Total = mamtm,
                IGSTAmt = Model.IGSTAmt,
                CGSTAmt = Model.CGSTAmt,
                SGSTAmt = Model.SGSTAmt,
                TDSAmt = Model.TDSAmt,
                //RoundOff= RoundOff,
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        // GET: Accounts/CreditPurchase

        public ActionResult Index(CreditPurchaseVM Model)
        {
            Session["CommnNarrlist"] = null;
            Session["TempAttach"] = null;
            TempData["PURReferAccount"] = "";
            string connstring = GetConnectionString();
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());

            //ViewBag.list = Session["MenuList"];
            //ViewBag.Modules = Session["ModulesList"];


            var Setup = ctxTFAT.CreditPurchaseSetup.FirstOrDefault();
            if (Setup == null)
            {
                Setup = new CreditPurchaseSetup();
            }
            //var OtherBillQuery = @"select isnull(BillBoth,0) as BillBoth,BillAuto,CCAmtMatch,BackDays,ShowLedgerPost,BackDated,DuplExpLRFMConfirm,NoDuplExpLRFM,ForwardDated,ForwardDays,ShowDocSerial,AllowZeroAmt,AutoRemark,NoDuplExpDt,DuplExpDtConfirm from CreditPurchaseSetup";
            //DataTable imDt = GetDataTable(OtherBillQuery);
            //if (imDt.Rows.Count > 0)
            {
                Model.BillBoth = Setup.BillBoth;
                Model.BillAuto = Setup.BillAuto;
                //Model.CCAmtMatch = Setup.CCAmtMatch;
                var mBackdays = Convert.ToDecimal(Setup.BackDays);
                Model.BackDays = Convert.ToInt32(mBackdays);
                Model.ShowLedgerPost = Setup.ShowLedgerPost == true ? "Y" : "N";
                Model.BackDated = Setup.BackDated;
                Model.DuplExpLRFMConfirm = Setup.DuplExpLRFMConfirm;
                Model.NoDuplExpLRFM = Setup.NoDuplExpLRFM;
                Model.ForwardDated = Setup.ForwardDated;
                var mfORWdays = Convert.ToDecimal(Setup.ForwardDays);
                Model.ForwardDays = Convert.ToInt32(mfORWdays);
                Model.ShowDocSerial = Setup.ShowDocSerial;
                Model.AllowZeroAmt = Setup.AllowZeroAmt;
                Model.AllowAutoRemark = Setup.AutoRemark;
                Model.NoDuplExpDt = Setup.NoDuplExpDt;
                Model.DuplExpDtConfirm = Setup.DuplExpDtConfirm;
                if (Setup.CurrDatetOnlyreq == false && Setup.BackDated == false && Setup.ForwardDated == false)
                {
                    Model.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
                    Model.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
                }
                if (Setup.CurrDatetOnlyreq == true)
                {
                    Model.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
                    Model.EndDate = DateTime.Now.ToString("yyyy-MM-dd");
                }
                if (Setup.BackDated == true)
                {
                    Model.StartDate = (DateTime.Now.AddDays(-Model.BackDays)).ToString("yyyy-MM-dd");
                }
                if (Setup.ForwardDated == true)
                {
                    Model.EndDate = (DateTime.Now.AddDays(Model.ForwardDays)).ToString("yyyy-MM-dd");
                }
            }
            Model.TDSFlag = Setup.CutTDS;
            //Model.SetupReqRelatedAc = Setup.RelatedPosting;
            var mAuth = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == Model.Type).Select(x => new
            {
                x.AuthLock,
                x.AuthNoPrint,
                x.AuthReq,
                x.AuthAgain
            }).FirstOrDefault();
            if (mAuth != null)
            {
                Model.AuthLock = mAuth.AuthLock;
                Model.AuthNoPrint = mAuth.AuthNoPrint;
                Model.AuthReq = mAuth.AuthReq;
                Model.AuthAgain = mAuth.AuthAgain;
            }
            //Model.GSTTypeName = GetGSTTypeName(Model.GSTType);

            var mpara = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x).FirstOrDefault();
            if (Model.Mode != "Add")
            {
                Model.Document = Model.Document;
                Model.Branch = Model.Document.Substring(0, 6);
                Model.ParentKey = Model.Document.Substring(6, (Model.Document.Length - 6));
                Model.Type = "PUR00";
                var result = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => new
                {
                    x.MainType,
                    x.SubType,
                    x.Name,
                    x.ConstantMode,
                    x.Constant,
                    x.LockPosting,
                    x.GSTType
                }).FirstOrDefault();
                Model.MainType = result.MainType;
                Model.SubType = result.SubType;
                Model.GSTType = result.GSTType.ToString();
                UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, Model.ParentKey, DateTime.Now, 0, "", "", "NA");
            }
            else
            {
                Model.Branch = mbranchcode;
                UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "NA");
            }

            if (Model.BillBoth == true || Model.BillAuto == true)
            {
                Model.OTGenerate = "A";
            }
            else
            {
                Model.OTGenerate = "M";
            }
            if (Model.Mode == "Add")
            {
                Model.DocDate = DateTime.Now;
                Model.BillDate = DateTime.Now;
                Session["Lothertrxlist"] = null;
                Model.AmtType = "Payment";
                //Model.ShowLedgerPost = "Y";
                Model.Prefix = mperiod;
                //var mStartDate = DateTime.Now.Date.AddDays(-Model.BackDays);
                //Model.StartDate = mStartDate.ToString("yyyy-MM-dd");
                //var mEndDate = DateTime.Now.Date.AddDays(Model.ForwardDays);
                //Model.EndDate = mEndDate.ToString("yyyy-MM-dd");
                Model.Type = "PUR00";
                var result = ctxTFAT.DocTypes.Where(x => x.Code == Model.Type).Select(x => x).FirstOrDefault();
                Model.MainType = result.MainType;
                Model.SubType = result.SubType;
                Model.BankCashCode = ctxTFAT.Purchase.Where(x => x.Branch == mbranchcode && x.Type == "PUR00").OrderByDescending(x => x.RECORDKEY).Select(x => x.Account).FirstOrDefault();
                Model.BankCashName = ctxTFAT.Master.Where(x => x.Code == Model.BankCashCode).Select(x => x.Name).FirstOrDefault();
                string mvprefix = result.PrefixConst == null ? "" : result.PrefixConst;
                int mWidth = result.DocWidth;
                var mPrevSrl = GetLastSerial(Model.TableName, Model.Branch, Model.Type, mperiod, Model.SubType, DateTime.Now.Date);
                if (mvprefix != "")
                {
                    mPrevSrl = mPrevSrl.Replace(mvprefix, "");
                }

                int mPreIntSrl = Convert.ToInt32(mPrevSrl) - 1;
                ViewData["PrevSrl"] = mPreIntSrl.ToString("D" + mWidth);
                var mserial = mPreIntSrl + 1;

            }
            else
            {
                //Model.ShowLedgerPost = "Y";

                var mVouchList = ctxTFAT.RelateData.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).ToList();
                List<CreditPurchaseVM> objledgerdetail = new List<CreditPurchaseVM>();
                int mCnt = 1;
                foreach (var item in mVouchList)
                {
                    var PaymentSum = ctxTFAT.RelateData.Where(x => x.AmtType == true && x.Char1 == item.Char1.ToString() && item.Char1.ToString() != "0").FirstOrDefault();
                    var ReceivedSum = ctxTFAT.RelateData.Where(x => x.AmtType == false && x.Char1 == item.Char1.ToString() && item.Char1.ToString() != "0").Sum(x => x.Amount);
                    decimal? BalAMt = Model.Amount;
                    if (PaymentSum != null)
                    {
                        if (PaymentSum.Amount > Convert.ToDecimal(ReceivedSum))
                        {
                            BalAMt = PaymentSum.Amount - Convert.ToDecimal(ReceivedSum);
                            BalAMt = BalAMt + item.Amount.Value;
                        }
                    }
                    var TyreStock = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == item.TableKey && x.Branch == item.Branch).FirstOrDefault();

                    //var Count = ctxTFAT.TyreStockSerial.Where(x => x.ParentKey == item.TableKey).ToList();
                    string mReleatedTo = item.Combo1;
                    objledgerdetail.Add(new CreditPurchaseVM()
                    {
                        Code = item.Code,
                        AccountName = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault(),
                        LRDetailList = GetLRDetailList(item.Branch + item.TableKey),
                        FMDetailList = GetFMDetailList(item.TableKey),
                        AddOnList = GetTruckWiseData(item.TableKey, mReleatedTo, item.ReqRelated, item.Code),
                        ItemList = GetItemWiseData(item.TableKey),
                        TyreStockList = GetTyreStockSerialList(item.TableKey, item.Branch),
                        Debit = (item.AmtType == true) ? item.Amount.Value : 0,
                        Credit = (item.AmtType == false) ? item.Amount.Value : 0,
                        //Narr = item.Narr,
                        Amt = item.Amount.Value,
                        AmtType = (item.AmtType == true) ? "Payment" : "Receipt",
                        RelatedChoice = item.Value8,
                        RelatedTo = item.Combo1,
                        TableKey = item.TableKey,
                        //DocumentList = GetAttachmentListInEdit(item.TableKey),
                        tempId = mCnt,
                        Narr = item.Narr,
                        PartialDivName = item.RelateTo.Value.ToString(),
                        BTNNO = (item.AmtType == true) ? item.Char1 : "",
                        BTNNOCombo = (item.AmtType == false) ? item.Char1 : "",
                        BTNNOComboN = PaymentSum == null ? "" : (item.AmtType == false) ? " BTN No-:  " + item.Char1 + " / Amt:- " + PaymentSum.Amount.Value.ToString("F2") + " / Date:- " + PaymentSum.DocDate.Value.ToShortDateString() : "",
                        BTNTotalAmt = PaymentSum == null ? 0 : PaymentSum.Amount.Value,
                        BTNBalAmt = BalAMt.Value,
                        ReferAccReq = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.ReferAccReq).FirstOrDefault(),
                        CostCenterTally = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.CostCenterAmtTally).FirstOrDefault(),
                        GSTFlag = item.GSTFlag,
                        GSTCode = item.GSTCode,
                        GSTCodeName = String.IsNullOrEmpty(ctxTFAT.TaxMaster.Where(x => x.Code.ToString() == item.GSTCode).Select(x => x.Name).FirstOrDefault()) == true ? ctxTFAT.HSNMaster.Where(x => x.Code.ToString() == item.GSTCode).Select(x => x.Name).FirstOrDefault() : ctxTFAT.TaxMaster.Where(x => x.Code.ToString() == item.GSTCode).Select(x => x.Name).FirstOrDefault(),
                        Taxable = item.Amount.Value,
                        IGSTRate = item.IGSTRate,
                        CGSTRate = item.CGSTRate,
                        SGSTRate = item.SGSTRate,
                        CGSTAmt = item.CGSTAmt,
                        SGSTAmt = item.SGSTAmt,
                        IGSTAmt = item.IGSTAmt,
                        //AccReqRelated = item.ReqRelated,
                        AuthReq = TyreStock == null ? false : TyreStock.IsActive,
                    });
                    mCnt = mCnt + 1;
                }
                string[] DefaultCost = new string[] { "000100949", "000100950", "000100811", "000100343", "000100736", "000100653", "000100326", "000100951", "000100952", "000100404", "000100781", "000100953", "000100954", "000100788", "000101135", "000100789", "000100346", "000100341", "000100345" };
                foreach (var item in objledgerdetail)
                {
                    var mAcc = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => new { x.RelatedTo, x.Name }).FirstOrDefault();

                    item.HSNCODE = item.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                    item.HSNCODEName = ctxTFAT.HSNMaster.Where(x => x.Code == item.HSNCODE).Select(x => x.Name).FirstOrDefault();

                    if (DefaultCost.Contains(item.Code))
                    {
                        item.PartialDivName = "Y";
                    }
                    else if (mAcc.RelatedTo == "LR")
                    {
                        item.PartialDivName = "LR";
                    }
                    else if (mAcc.RelatedTo == "FM")
                    {
                        item.PartialDivName = "FM";
                    }
                    else if (mAcc.RelatedTo == "Branch")
                    {
                        item.PartialDivName = "Branch";
                    }
                    else
                    {
                        item.PartialDivName = "EY";
                    }
                }
                Session.Add("Lothertrxlist", objledgerdetail);







                Model.Selectedleger = objledgerdetail;
                Model.SumDebit = objledgerdetail.Sum(x => x.Debit);
                Model.SumCredit = objledgerdetail.Sum(x => x.Credit);
                Model.SumDebit = Math.Round((decimal)Model.SumDebit, 2);
                Model.SumCredit = Math.Round((decimal)Model.SumCredit, 2);
                var mLedger = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).OrderBy(x => x.Sno).FirstOrDefault();
                Model.DocDate = mLedger.DocDate;
                Model.Srl = mLedger.Srl;
                Model.Prefix = mLedger.Prefix;
                Model.BankCashCode = mLedger.Code;
                Model.BankCashName = ctxTFAT.Master.Where(x => x.Code == mLedger.Code).Select(x => x.Name).FirstOrDefault();
                Model.BillNumber = mLedger.BillNumber;
                Model.BillDate = mLedger.BillDate.Value;

                #region GstGet

                Model.IGSTAmt = objledgerdetail.Sum(x => x.IGSTAmt);
                Model.CGSTAmt = objledgerdetail.Sum(x => x.CGSTAmt);
                Model.SGSTAmt = objledgerdetail.Sum(x => x.SGSTAmt);

                //if (Model.IGSTAmt > 0 || Model.CGSTAmt > 0 || Model.SGSTAmt > 0)
                //{
                //    Model.GSTFlag = true;
                //}

                //Model.SGSTRate = mLedger.SGSTRate == null ? 0 : mLedger.SGSTRate.Value;
                //Model.CGSTRate = mLedger.CGSTRate == null ? 0 : mLedger.CGSTRate.Value;
                //Model.IGSTRate = mLedger.IGSTRate == null ? 0 : mLedger.IGSTRate.Value;

                //Model.GSTCode = mLedger.TaxCode == null ? "" : mLedger.TaxCode.ToString();
                //Model.GSTCodeName = ctxTFAT.TaxMaster.Where(x => x.Code.ToString() == Model.GSTCode).Select(x => x.Name).FirstOrDefault();

                Model.Taxable = objledgerdetail.Sum(x => x.Debit);
                Model.InvoiceAmt = Math.Abs(Model.Taxable + Model.IGSTAmt + Model.CGSTAmt + Model.SGSTAmt);

                #endregion


                Model.PeriodLock = PeriodLock(mLedger.Branch, mLedger.Type, mLedger.DocDate);
                if (mLedger.AUTHORISE.Substring(0, 1) == "A")
                {
                    Model.LockAuthorise = LockAuthorise(mLedger.Type, Model.Mode, mLedger.ParentKey, mLedger.ParentKey);
                }
                var LedgerList = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).Select(x => x.Branch + x.TableKey).ToList();
                string IntoSingleLine = "";
                IntoSingleLine = String.Join(",", LedgerList);
                //foreach (var item in LedgerList)
                //{
                //    IntoSingleLine += "" + item + ",";
                //}
                if (!String.IsNullOrEmpty(IntoSingleLine))
                {
                    //IntoSingleLine = IntoSingleLine.Substring(0, IntoSingleLine.Length - 1);
                    //string connstring = GetConnectionString();
                    string mSQLQuery = "with Demo as( SELECT  value  FROM TripSheetMaster CROSS APPLY STRING_SPLIT(AdjustLedgerRef, '^') where len(AdjustLedgerRef)>5) select count(*) from Demo where charindex( Value,'" + IntoSingleLine + "')<>0";
                    //var connstring = GetConnectionString();
                    SqlConnection conn = new SqlConnection(connstring);
                    SqlCommand cmd = new SqlCommand(mSQLQuery, conn);
                    try
                    {
                        conn.Open();
                        cmd.CommandTimeout = 0;
                        Int32 CustomerCnt = (Int32)cmd.ExecuteScalar();
                        if (CustomerCnt > 0)
                        {
                            Model.LockAdjustTrip = true;
                            Model.LockAdjustTripMessage = "This Document Adjust In TripSheet.. So U Cant Edit/Delete...!<br>";
                        }
                        else
                        {
                            mSQLQuery = "with Demo as( SELECT  value  FROM TripSheetMaster CROSS APPLY STRING_SPLIT(CCjustLedgerRef, '^') where len(CCjustLedgerRef)>5) select count(*) from Demo where charindex( Value,'" + IntoSingleLine + "')<>0";
                            conn = new SqlConnection(connstring);
                            cmd = new SqlCommand(mSQLQuery, conn);
                            try
                            {
                                conn.Open();
                                cmd.CommandTimeout = 0;
                                CustomerCnt = (Int32)cmd.ExecuteScalar();
                                if (CustomerCnt > 0)
                                {
                                    Model.LockAdjustTrip = true;
                                    Model.LockAdjustTripMessage = "This Document Adjust In TripSheet.. So U Cant Edit/Delete...!<br>";
                                }
                                else
                                {
                                    mSQLQuery = "with Demo as( SELECT  value  FROM TripSheetMaster CROSS APPLY STRING_SPLIT(AdjustBalLedgerRef, '^') where len(AdjustBalLedgerRef)>5) select count(*) from Demo where charindex( Value,'" + IntoSingleLine + "')<>0";
                                    conn = new SqlConnection(connstring);
                                    cmd = new SqlCommand(mSQLQuery, conn);
                                    try
                                    {
                                        conn.Open();
                                        cmd.CommandTimeout = 0;
                                        CustomerCnt = (Int32)cmd.ExecuteScalar();
                                        if (CustomerCnt > 0)
                                        {
                                            Model.LockAdjustTrip = true;
                                            Model.LockAdjustTripMessage = "This Document Adjust In TripSheet.. So U Cant Edit/Delete...!<br>";
                                        }
                                        else
                                        {

                                        }
                                    }
                                    catch (Exception mex)
                                    {
                                    }
                                    finally
                                    {
                                        cmd.Dispose();
                                        conn.Close();
                                        conn.Dispose();
                                    }
                                }
                            }
                            catch (Exception mex)
                            {
                            }
                            finally
                            {
                                cmd.Dispose();
                                conn.Close();
                                conn.Dispose();
                            }
                        }
                    }
                    catch (Exception mex)
                    {
                    }
                    finally
                    {
                        cmd.Dispose();
                        conn.Close();
                        conn.Dispose();
                    }
                }
                #region Purchase

                var mobj1 = ctxTFAT.Purchase.Where(x => (x.TableKey) == Model.ParentKey && x.Branch == Model.Branch).FirstOrDefault();

                Model.BillNumber = mobj1.BillNumber;
                Model.BillDate = mobj1.BillDate.Value;

                Model.Authorise = mobj1.AUTHORISE;
                var mdoctype = ctxTFAT.DocTypes.Where(x => x.Code == mobj1.Type).Select(x => new { x.DocWidth }).FirstOrDefault();
                Model.Srl = mobj1.Srl.Value.ToString("D" + mdoctype.DocWidth.ToString());
                Model.Prefix = mobj1.Prefix;
                Model.DocDate = mobj1.DocDate.Value;

                Model.Branch = mobj1.Branch;
                Model.LocationCode = mobj1.LocationCode;
                Model.BankCashCode = mobj1.Code;
                Model.Document = Model.Document;
                Model.GSTType = (mobj1.GSTType == null) ? "0" : mobj1.GSTType.Value.ToString();
                //Model.GSTTypeName = GetGSTTypeName(Model.GSTType);
                Model.CrPeriod = mobj1.CrPeriod == null ? 0 : mobj1.CrPeriod.Value;
                Model.Narr = mobj1.Narr;


                string mStr = @"select dbo.GetBalance('" + Model.BankCashCode + "','" + MMDDYY(DateTime.Now) + "','',0,0)";
                DataTable smDt = GetDataTable(mStr);
                double mBalance = 0;
                if (smDt.Rows.Count > 0)
                {
                    mBalance = Convert.ToDouble(smDt.Rows[0][0].ToString());
                }
                #endregion

                #region ATTACHMENT
                AttachmentVM Att = new AttachmentVM();
                Att.Type = "PUR00";
                Att.Srl = Model.Srl.ToString();

                AttachmentController attachmentC = new AttachmentController();
                List<AttachmentVM> attachments = attachmentC.GetAttachmentListInEdit(Att);
                Session["TempAttach"] = attachments;

                #endregion

                #region TDS

                var tdsdetails = ctxTFAT.TDSPayments.Where(x => x.ParentKey == Model.ParentKey && x.Branch == mobj1.Branch).Select(x => x).FirstOrDefault();
                Model.TDSAmt = (tdsdetails != null) ? tdsdetails.TDSAmt.Value : 0;
                Model.TDSRate = (tdsdetails != null) ? tdsdetails.TDSTax == null ? 0 : tdsdetails.TDSTax.Value : 0;
                Model.TDSCode = (tdsdetails != null) ? tdsdetails.TDSCode.Value.ToString() : "";
                Model.TDSCodeName = ctxTFAT.TDSMaster.Where(x => x.Code.ToString() == Model.TDSCode).Select(x => x.Name).FirstOrDefault();
                if (Model.TDSAmt > 0)
                {
                    Model.TDSFlag = true;
                }
                else
                {
                    Model.TDSFlag = Setup.CutTDS;
                }
                var CheckDependency2 = ctxTFAT.Outstanding.Where(x => x.aType == Model.Type && x.aSrl == Model.Srl && x.aPrefix == Model.Prefix && x.DocBranch == Model.Branch).Select(x => x).FirstOrDefault();
                if (CheckDependency2 != null)
                {
                    if (CheckDependency2.Type == Model.Type && CheckDependency2.Srl == Model.Srl && CheckDependency2.Prefix == Model.Prefix && CheckDependency2.DocBranch == Model.Branch)
                    {

                    }
                    else
                    {
                        Model.CheckMode = true;

                        Model.Message = " Document is Already Adjusted In Cash Bank Against : " + CheckDependency2.TableRefKey.ToString() + ", Cant " + Model.Mode;
                    }

                }
                if (Model.Mode != "Add" && Model.AuthReq == true && Model.Authorise.Substring(0, 1) == "A" && Model.AuthLock)
                {
                    Model.CheckMode = true;
                    //Model.Mode = "View";
                    Model.Message = "Document is Already Authorised Cant Edit";
                }

                #endregion


            }

            List<GridOption> Grlist = new List<GridOption>();
            var list = ctxTFAT.DocFormats.Where(x => x.Type == "PUR00").Select(x => x).ToList();
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



            return View(Model);
        }

        public ActionResult GetBTNCOMBOList(string term, string Account, string Srl)
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            if (Account != "" && Account != null)
            {
                var result = ctxTFAT.RelateData.Where(x => x.Code == "").Select(c => new { c.Char1, c.Char2 }).Distinct().ToList();
                var GetList = ctxTFAT.RelateData.Where(x => x.Code == Account && x.Srl.Value.ToString() != Srl).Select(x => x.Char1).ToList().Distinct();
                decimal Amt = 0;
                foreach (var item in GetList)
                {
                    //var NO = Convert.ToInt32(item);
                    if (item != "NULL" && item != null)
                    {
                        var PaymentSum = ctxTFAT.RelateData.Where(x => x.AmtType == true && x.Char1 == item.ToString() && item.ToString() != "0").FirstOrDefault();
                        var ReceivedSum = ctxTFAT.RelateData.Where(x => x.AmtType == false && x.Char1 == item.ToString() && item.ToString() != "0" && x.Srl.Value.ToString() != Srl).Sum(x => x.Amount);

                        if (PaymentSum.Amount > Convert.ToDecimal(ReceivedSum))
                        {
                            string Txt = " BTN No-:  " + item + " / Amt:- " + PaymentSum.Amount.Value.ToString("F2") + " / Date:- " + PaymentSum.DocDate.Value.ToShortDateString();
                            //Amt = PaymentSum - Convert.ToDecimal( ReceivedSum);
                            listItems.Add(new SelectListItem { Value = item, Text = Txt });
                            //listItems.Add(new SelectListItem { Value = item, Text = " BTN No-:  " +item  });
                        }
                    }

                }
                if (!String.IsNullOrEmpty(term))
                {
                    listItems = listItems.Where(x => x.Value.Contains(term)).ToList();
                }
            }
            var Modified = listItems.Select(x => new
            {
                Code = x.Value,
                Name = x.Text
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        #region ADD LEDGER ITEM

        public ActionResult AddCashLedger(CreditPurchaseVM Model)
        {

            //var Setup = ctxTFAT.CreditPurchaseSetup.FirstOrDefault();
            var Setup = ctxTFAT.CreditPurchaseSetup.FirstOrDefault();
            if (Setup == null)
            {
                //Setup = new CreditPurchaseSetup();
                Setup = new CreditPurchaseSetup();
            }

            //TP(Transit Pass) Trip Expenses ,Fitness,Green Tax,Permit(1 Year),Permit(5 Year),PT,PUC,Insurance,Loan Expenses Validation Of Date
            if ((Model.RelatedTo == "000101135" || Model.Code == "000101135" || Model.RelatedTo == "000100341" || Model.Code == "000100341" || Model.RelatedTo == "000100736" || Model.Code == "000100736" || Model.RelatedTo == "000100653" || Model.Code == "000100653" || Model.RelatedTo == "000100811" || Model.Code == "000100811" || Model.RelatedTo == "000100781" || Model.Code == "000100781" || Model.RelatedTo == "000100953" || Model.Code == "000100953" || Model.RelatedTo == "000100788" || Model.Code == "000100788" || Model.RelatedTo == "000100789" || Model.Code == "000100789" || Model.RelatedTo == "000100326" || Model.Code == "000100326" || Model.RelatedTo == "000100951" || Model.Code == "000100951"))
            {
                if (Model.AddOnList != null)
                {
                    var mstartdate = Model.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                    DateTime Startdate = Convert.ToDateTime(mstartdate);
                    var menddate = Model.AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault();
                    DateTime Enddate = Convert.ToDateTime(menddate);
                    if (Enddate < Startdate)
                    {
                        Model.Status = "ValidError";
                        Model.Message = "To Date Should Be greater than From Date";
                        return Json(Model, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            //Check Date Of Tyre Expenses
            if (Model.RelatedTo == "000100345" || Model.Code == "000100345")
            {
                if (Model.ItemList != null)
                {
                    var mInstalldT = Model.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                    DateTime mInstalldate = Convert.ToDateTime(mInstalldT);
                    var mInstalldT2 = Model.ItemList.Where(x => x.Fld == "F013").Select(x => x.ApplCode).FirstOrDefault();
                    if (mInstalldT2 != null)
                    {
                        DateTime mInstalldate2 = Convert.ToDateTime(mInstalldT2);
                        if (mInstalldate2 < mInstalldate)
                        {
                            Model.Status = "ValidError";
                            Model.Message = "ExpiryDate should be greater than Install Date Cant Save";
                            return Json(Model, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                int NoOfTyre = 0, NoOfStepne = 0;
                if (Model.Code == "000100345")
                {
                    NoOfTyre = ctxTFAT.VehicleMaster.Where(x => x.PostAc == Model.RelatedChoice).Select(x => x.NoOfTyres).FirstOrDefault();
                    NoOfStepne = ctxTFAT.VehicleMaster.Where(x => x.PostAc == Model.RelatedChoice).Select(x => x.Stepney).FirstOrDefault();

                }
                else
                {
                    NoOfTyre = ctxTFAT.VehicleMaster.Where(x => x.PostAc == Model.Code).Select(x => x.NoOfTyres).FirstOrDefault();
                    NoOfStepne = ctxTFAT.VehicleMaster.Where(x => x.PostAc == Model.Code).Select(x => x.Stepney).FirstOrDefault();
                }
                if (Model.AddOnList != null)
                {
                    var mstepneeortyre = Model.AddOnList.Where(x => x.Fld == "F002").Select(X => X.ApplCode).FirstOrDefault();
                    var ilnTableKey = Model.AddOnList.Where(x => x.Fld == "F003").Select(X => X.ApplCode).FirstOrDefault();
                    int mTyreno = Convert.ToInt32(ilnTableKey);
                    if (mTyreno > NoOfTyre && mstepneeortyre == "Tyre")
                    {
                        Model.Status = "ValidError";
                        Model.Message = "Vehicles Have " + NoOfTyre + " No Of Tyres. So Entered Tyre No not Valid Cant Save.";
                        return Json(Model, JsonRequestBehavior.AllowGet);
                    }
                    int mstepneeno = Convert.ToInt32(ilnTableKey);
                    if (mstepneeno > NoOfStepne && mstepneeortyre == "Stepnee")
                    {
                        Model.Status = "ValidError";
                        Model.Message = "Vehicles Have " + NoOfStepne + " No Of Stepnee. So Entered Stepnee No not Valid Cant Save.";
                        return Json(Model, JsonRequestBehavior.AllowGet);
                    }
                }

                if (Model.SessionFlag == "Add")
                {
                    List<CreditPurchaseVM> objledgerdetail1 = new List<CreditPurchaseVM>();
                    if (Session["Lothertrxlist"] != null)
                    {
                        objledgerdetail1 = (List<CreditPurchaseVM>)Session["Lothertrxlist"];
                    }
                    foreach (var item in objledgerdetail1)
                    {
                        var TyreOrStepni = Model.AddOnList == null ? "0" : Model.AddOnList.Where(x => x.Fld == "F002").Select(X => X.ApplCode).FirstOrDefault();
                        var TyreOrStepno = Model.AddOnList == null ? "0" : Model.AddOnList.Where(x => x.Fld == "F003").Select(X => X.ApplCode).FirstOrDefault();
                        var VehiclNo = Model.Code == "000100345" ? Model.RelatedChoice : Model.Code;
                        var CurrVehiclNo = item.Code == "000100345" ? item.RelatedChoice : item.Code;
                        CurrVehiclNo = CurrVehiclNo == "Tyrestock" ? "" : CurrVehiclNo;
                        CurrVehiclNo = CurrVehiclNo == "Remould" ? "" : CurrVehiclNo;

                        if (item.AddOnList != null)
                        {
                            if (item.AddOnList.Where(x => x.Fld == "F002").Select(X => X.ApplCode).FirstOrDefault() == TyreOrStepni && item.AddOnList.Where(x => x.Fld == "F003").Select(X => X.ApplCode).FirstOrDefault() == TyreOrStepno && VehiclNo == CurrVehiclNo)
                            {
                                Model.Status = "ValidError";
                                Model.Message = "You Have Enterd Already This Vehicle And Tyre/Stepni no  So U Cannot Enter Duplicate Data.....!";
                                return Json(Model, JsonRequestBehavior.AllowGet);
                            }
                        }

                    }
                }
            }

            //Check Date Of Spare Parts Expenses Expenses
            if (Model.RelatedTo == "000100346" || Model.Code == "000100346")
            {
                var mInstalldT = Model.ItemList == null ? "1900-01-01" : Model.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                DateTime mInstalldate = Convert.ToDateTime(mInstalldT);
                var mInstalldT2 = Model.ItemList == null ? "" : Model.ItemList.Where(x => x.Fld == "F013").Select(x => x.ApplCode).FirstOrDefault();
                if (!String.IsNullOrEmpty(mInstalldT2))
                {
                    DateTime mInstalldate2 = Convert.ToDateTime(mInstalldT2);
                    if (mInstalldate2 < mInstalldate)
                    {
                        Model.Status = "ValidError";
                        Model.Message = "ExpiryDate should be greater than Install Date Cant Save";
                        return Json(Model, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            if (Setup.NoDuplExpDt)
            {
                if (Model.SessionFlag == "Add")
                {
                    if ((Model.RelatedTo == "000100341" || Model.Code == "000100341" || Model.RelatedTo == "000100736" || Model.Code == "000100736" || Model.RelatedTo == "000100653" || Model.Code == "000100653" || Model.RelatedTo == "000100811" || Model.Code == "000100811" || Model.RelatedTo == "000100781" || Model.Code == "000100781" || Model.RelatedTo == "000100953" || Model.Code == "000100953" || Model.RelatedTo == "000100788" || Model.Code == "000100788" || Model.RelatedTo == "000100789" || Model.Code == "000100789" || Model.RelatedTo == "000100326" || Model.Code == "000100326" || Model.RelatedTo == "000100951" || Model.Code == "000100951"))
                    {
                        if (Model.AddOnList != null)
                        {
                            var mstartdate = Model.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                            DateTime Startdate = Convert.ToDateTime(mstartdate);

                            var menddate = Model.AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault();
                            DateTime Enddate = Convert.ToDateTime(menddate);

                            var FromDate = (Convert.ToDateTime(Startdate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            var TODate = (Convert.ToDateTime(Enddate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

                            string mStr = "", iMTableKey = "";
                            mStr = "select top 1 isnull(TableKey,'') from relatedata where value8='" + Model.RelatedChoice + "' and code='" + Model.Code + "' and  '" + FromDate + "' <= date2  ";
                            string connstring = GetConnectionString();
                            DataTable smDt = GetDataTable(mStr, connstring);
                            if (smDt.Rows.Count > 0)
                            {
                                iMTableKey = smDt.Rows[0][0].ToString();
                            }
                            if (!(String.IsNullOrEmpty(iMTableKey)))
                            {
                                string mMessage = "";
                                string Name = "";
                                if (String.IsNullOrEmpty(Model.RelatedChoice))
                                {
                                    Name = ctxTFAT.Master.Where(x => x.Code.Trim() == Model.Code).Select(x => x.Name).FirstOrDefault();
                                }
                                else
                                {
                                    Name = ctxTFAT.Master.Where(x => x.Code.Trim() == Model.RelatedChoice).Select(x => x.Name).FirstOrDefault();
                                }
                                mMessage = "Already " + Name + " Save Between Entered Days Cant Save.";
                                Model.Status = "ValidError";
                                Model.Message = mMessage;
                                return Json(Model, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
                else
                {
                    if ((Model.RelatedTo == "000100341" || Model.Code == "000100341" || Model.RelatedTo == "000100736" || Model.Code == "000100736" || Model.RelatedTo == "000100653" || Model.Code == "000100653" || Model.RelatedTo == "000100811" || Model.Code == "000100811" || Model.RelatedTo == "000100781" || Model.Code == "000100781" || Model.RelatedTo == "000100953" || Model.Code == "000100953" || Model.RelatedTo == "000100788" || Model.Code == "000100788" || Model.RelatedTo == "000100789" || Model.Code == "000100789" || Model.RelatedTo == "000100326" || Model.Code == "000100326" || Model.RelatedTo == "000100951" || Model.Code == "000100951"))
                    {
                        if (Model.AddOnList != null)
                        {
                            var mstartdate = Model.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                            DateTime Startdate = Convert.ToDateTime(mstartdate);

                            var menddate = Model.AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault();
                            DateTime Enddate = Convert.ToDateTime(menddate);

                            var FromDate = (Convert.ToDateTime(Startdate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            var TODate = (Convert.ToDateTime(Enddate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

                            string mStr = "", iMTableKey = "";
                            mStr = "select top 1 isnull(TableKey,'') from relatedata where ParentKey<>'" + Model.ParentKey + "' and value8='" + Model.RelatedChoice + "' and code='" + Model.Code + "' and  '" + FromDate + "' <= date2  ";
                            string connstring = GetConnectionString();
                            DataTable smDt = GetDataTable(mStr, connstring);
                            if (smDt.Rows.Count > 0)
                            {
                                iMTableKey = smDt.Rows[0][0].ToString();
                            }

                            if (!(String.IsNullOrEmpty(iMTableKey)))
                            {
                                string mMessage = "";
                                string Name = "";
                                if (String.IsNullOrEmpty(Model.RelatedChoice))
                                {
                                    Name = ctxTFAT.Master.Where(x => x.Code.Trim() == Model.Code).Select(x => x.Name).FirstOrDefault();
                                }
                                else
                                {
                                    Name = ctxTFAT.Master.Where(x => x.Code.Trim() == Model.RelatedChoice).Select(x => x.Name).FirstOrDefault();
                                }
                                mMessage = "Already " + Name + " Save Between Entered Days Cant Save.";
                                Model.Status = "ValidError";
                                Model.Message = mMessage;
                                return Json(Model, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }

                if (Model.RelatedTo == "000100346" || Model.Code == "000100346")
                {
                    var mpart = Model.ItemList == null ? "" : Model.ItemList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault();
                    if (!String.IsNullOrEmpty(mpart))
                    {
                        var mInstalldT = Model.ItemList == null ? "1900-01-01" : Model.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                        DateTime mInstalldate = Convert.ToDateTime(mInstalldT);
                        var iMTableKey = ctxTFAT.RelateData.Where(x => x.Combo1 == Model.RelatedTo && x.Code == Model.Code && (x.Value1 == mpart && mInstalldate <= x.Date3) && x.ParentKey != Model.ParentKey).Select(x => x.TableKey).FirstOrDefault();
                        if (iMTableKey != null)
                        {
                            Model.Status = "ValidError";
                            Model.Message = "Same Vehicle Same Part Entered Install date before Due date Cant Save";
                            return Json(Model, JsonRequestBehavior.AllowGet);
                        }
                    }
                    var mdecnum2 = Model.AddOnList == null ? "" : Model.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                    var mNum2 = (string.IsNullOrEmpty(mdecnum2) == true) ? 0 : Convert.ToDecimal(mdecnum2);

                    var iMTableKey2 = ctxTFAT.RelateData.Where(x => x.Combo1 == Model.RelatedTo && x.Code == Model.Code && (x.Value1 == mpart && mNum2 <= x.Num3) && x.ParentKey != Model.ParentKey).Select(x => x.TableKey).FirstOrDefault();
                    if (iMTableKey2 != null)
                    {
                        Model.Status = "ValidError";
                        Model.Message = "Same Vehicle Same Part Entered Current KM date before Due KM  Cant Save";
                        return Json(Model, JsonRequestBehavior.AllowGet);
                    }

                }
                else if (Model.RelatedTo == "000100345" || Model.Code == "000100345")
                {
                    var mtype = Model.ItemList == null ? "" : Model.ItemList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                    var mInstalldT = Model.ItemList == null ? "1900-01-01" : Model.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                    var mtyreserialno = Model.AddOnList == null ? "" : Model.AddOnList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault();
                    DateTime mInstalldate = Convert.ToDateTime(mInstalldT);
                    var iMTableKey = ctxTFAT.RelateData.Where(x => x.Combo1 == Model.RelatedTo && x.Code == Model.Code && (x.Value2 == mtype && x.Value6 == mtyreserialno && mInstalldate <= x.Date3) && x.ParentKey != Model.ParentKey).Select(x => x.TableKey).FirstOrDefault();
                    if (iMTableKey != null)
                    {
                        Model.Status = "ValidError";
                        Model.Message = "Entered Install date before Due date  Cant Save";
                        return Json(Model, JsonRequestBehavior.AllowGet);
                    }
                    var mdecnum2 = Model.ItemList == null ? "" : Model.ItemList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault();
                    var mNum2 = (string.IsNullOrEmpty(mdecnum2) == true) ? 0 : Convert.ToDecimal(mdecnum2);

                    var iMTableKey2 = ctxTFAT.RelateData.Where(x => x.Combo1 == Model.RelatedTo && x.Code == Model.Code && (x.Value2 == mtype && x.Value6 == mtyreserialno && mNum2 <= x.Num3) && x.ParentKey != Model.ParentKey).Select(x => x.TableKey).FirstOrDefault();
                    if (iMTableKey2 != null)
                    {
                        Model.Status = "ValidError";
                        Model.Message = "Entered Current KM date before Due KM  Cant Save";
                        return Json(Model, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            if (Model.SessionFlag == "Add")
            {
                List<CreditPurchaseVM> objledgerdetail = new List<CreditPurchaseVM>();

                List<CreditPurchaseVM> lrdetaillist = new List<CreditPurchaseVM>();

                if (Session["Lothertrxlist"] != null)
                {
                    objledgerdetail = (List<CreditPurchaseVM>)Session["Lothertrxlist"];
                }

                #region Add Default Narr 

                if (Setup.AutoRemark)
                {
                    if (Model.LRDetailList != null && Model.LRDetailList.Count > 0)
                    {
                        List<string> item3 = new List<string>();
                        item3 = Model.LRDetailList.Select(x => x.LRNumber + "-" + x.LRAmt).ToList();

                        string osadjnarr = String.Join(",", item3);
                        Model.Narr += "LR :-" + osadjnarr + "   \n";

                    }

                    if (Model.FMDetailList != null && Model.FMDetailList.Count > 0)
                    {
                        List<string> item3 = new List<string>();
                        item3 = Model.FMDetailList.Select(x => x.FMNumber + "-" + x.FMAmt).ToList();

                        string osadjnarr = String.Join(",", item3);
                        Model.Narr += "FM :-" + osadjnarr + "   \n";
                    }

                    if (Model.ItemList != null && Model.ItemList.Count() > 0 && (Model.RelatedTo != "000100343" || Model.Code != "000100343"))
                    {
                        var Prodctgroup = Model.ItemList.Where(x => x.Fld == "F001").Select(X => X.ApplCode).FirstOrDefault();
                        if ((!String.IsNullOrEmpty(Prodctgroup)) && Prodctgroup != "Select")
                        {
                            if (Model.ItemList.Where(x => x.Fld == "F002").Select(X => X.ApplCode).FirstOrDefault() == "Direct")
                            {
                                var Group = Model.ItemList.Where(x => x.Fld == "F001").Select(X => X.ApplCode).FirstOrDefault();
                                Model.Narr += "Item Details : Group Name: " + ctxTFAT.ItemGroups.Where(x => x.Code == Group).Select(X => X.Name).FirstOrDefault() + ", Name : " + Model.ItemList.Where(x => x.Fld == "F003").Select(X => X.ApplCode).FirstOrDefault() + ", Cost : " + Model.ItemList.Where(x => x.Fld == "F004").Select(X => X.ApplCode).FirstOrDefault() + ", Qty : " + Model.ItemList.Where(x => x.Fld == "F005").Select(X => X.ApplCode).FirstOrDefault() + ", Total Amount : " + Model.ItemList.Where(x => x.Fld == "F006").Select(X => X.ApplCode).FirstOrDefault() + ", Warranty KM : " + Model.ItemList.Where(x => x.Fld == "F007").Select(X => X.ApplCode).FirstOrDefault() + ", Current KM : " + Model.ItemList.Where(x => x.Fld == "F008").Select(X => X.ApplCode).FirstOrDefault() + ", Due KM : " + Model.ItemList.Where(x => x.Fld == "F009").Select(X => X.ApplCode).FirstOrDefault() + ", Warranty Days : " + Model.ItemList.Where(x => x.Fld == "F010").Select(X => X.ApplCode).FirstOrDefault() + ", MFG Date : " + Convert.ToDateTime(Model.ItemList.Where(x => x.Fld == "F011").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + ", Install / Received  Date : " + Convert.ToDateTime(Model.ItemList.Where(x => x.Fld == "F012").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + ", Due Date : " + Convert.ToDateTime(Model.ItemList.Where(x => x.Fld == "F013").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + ", HSN Name : " + ctxTFAT.HSNMaster.Where(x => x.Code == Model.HSNCODE).Select(X => X.Name).FirstOrDefault() + "   \n"; ;
                            }
                            else
                            {
                                var Code = Model.ItemList.Where(x => x.Fld == "F003").Select(X => X.ApplCode).FirstOrDefault();
                                var Group = Model.ItemList.Where(x => x.Fld == "F001").Select(X => X.ApplCode).FirstOrDefault();
                                Model.Narr += "Item Details : Group Name: " + ctxTFAT.ItemGroups.Where(x => x.Code == Group).Select(X => X.Name).FirstOrDefault() + ", Name : " + ctxTFAT.ItemMaster.Where(x => x.Code == Code).Select(X => X.Name).FirstOrDefault() + ", Cost : " + Model.ItemList.Where(x => x.Fld == "F004").Select(X => X.ApplCode).FirstOrDefault() + ", Qty : " + Model.ItemList.Where(x => x.Fld == "F005").Select(X => X.ApplCode).FirstOrDefault() + ", Total Amount : " + Model.ItemList.Where(x => x.Fld == "F006").Select(X => X.ApplCode).FirstOrDefault() + ", Warranty KM : " + Model.ItemList.Where(x => x.Fld == "F007").Select(X => X.ApplCode).FirstOrDefault() + ", Current KM : " + Model.ItemList.Where(x => x.Fld == "F008").Select(X => X.ApplCode).FirstOrDefault() + ", Due KM : " + Model.ItemList.Where(x => x.Fld == "F009").Select(X => X.ApplCode).FirstOrDefault() + ", Warranty Days : " + Model.ItemList.Where(x => x.Fld == "F010").Select(X => X.ApplCode).FirstOrDefault() + ", MFG Date : " + Convert.ToDateTime(Model.ItemList.Where(x => x.Fld == "F011").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + ", Install / Received  Date : " + Convert.ToDateTime(Model.ItemList.Where(x => x.Fld == "F012").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + ", Due Date : " + Convert.ToDateTime(Model.ItemList.Where(x => x.Fld == "F013").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + ", HSN Name : " + ctxTFAT.HSNMaster.Where(x => x.Code == Model.HSNCODE).Select(X => X.Name).FirstOrDefault() + "   \n"; ;
                            }
                        }

                    }

                    if (Model.AddOnList != null)
                    {
                        if (Model.RelatedTo == "000100949" || Model.Code == "000100949")
                        {
                            Model.Narr += "Accident Date: " + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F001").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + ", Amount: " + Model.AddOnList.Where(x => x.Fld == "F002").Select(X => X.ApplCode).FirstOrDefault();
                        }
                        else if (Model.RelatedTo == "000100950" || Model.Code == "000100950")
                        {
                            Model.Narr += "Body Building Date: " + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F001").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + ", Amount : " + Model.AddOnList.Where(x => x.Fld == "F002").Select(X => X.ApplCode).FirstOrDefault();
                        }
                        else if (Model.RelatedTo == "000100952" || Model.Code == "000100952")
                        {
                            Model.Narr += "Misc Charges Amount: " + Model.AddOnList.Where(x => x.Fld == "F001").Select(X => X.ApplCode).FirstOrDefault();
                        }
                        else if (Model.RelatedTo == "000100404" || Model.Code == "000100404")
                        {
                            Model.Narr += "Other Amount: " + Model.AddOnList.Where(x => x.Fld == "F001").Select(X => X.ApplCode).FirstOrDefault();
                        }
                        else if (Model.RelatedTo == "000100954" || Model.Code == "000100954")
                        {
                            Model.Narr += "Principal Amount Date: " + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F001").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + ", Amount: " + Model.AddOnList.Where(x => x.Fld == "F002").Select(X => X.ApplCode).FirstOrDefault();
                        }
                        else if (Model.RelatedTo == "000100343" || Model.Code == "000100343")
                        {
                            Model.Narr += "Diesel Receipt No:- " + Model.AddOnList.Where(x => x.Fld == "F001").Select(X => X.ApplCode).FirstOrDefault() + ", Litres : " + Model.AddOnList.Where(x => x.Fld == "F002").Select(X => X.ApplCode).FirstOrDefault() + ", Aprroved Diesel : " + Model.AddOnList.Where(x => x.Fld == "F003").Select(X => X.ApplCode).FirstOrDefault() + ", Aprrover Name : " + Model.AddOnList.Where(x => x.Fld == "F004").Select(X => X.ApplCode).FirstOrDefault();

                            if (Model.ItemList != null && Model.ItemList.Count() > 0)
                            {
                                Model.Narr += " ,Amount : " + Model.ItemList.Where(x => x.Fld == "F006").Select(X => X.ApplCode).FirstOrDefault() + ", Current KM : " + Model.ItemList.Where(x => x.Fld == "F008").Select(X => X.ApplCode).FirstOrDefault() + ", Install  / Received Date : " + Model.ItemList.Where(x => x.Fld == "F012").Select(X => X.ApplCode).FirstOrDefault();
                            }
                        }
                        else if (Model.RelatedTo == "000100736" || Model.Code == "000100736")
                        {
                            Model.Narr += "Fitness:- Year : [ " + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F001").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + " ] : [ " + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F002").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + " ]";
                        }
                        else if (Model.RelatedTo == "000100653" || Model.Code == "000100653")
                        {
                            Model.Narr += "Green Tax:- Year : [ " + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F001").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + " ] : [ " + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F002").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + " ]";
                        }
                        else if (Model.RelatedTo == "000100811" || Model.Code == "000100811")
                        {
                            Model.Narr += "State Tax:- Year : [ " + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F001").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + " ] : [ " + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F002").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + " ]";
                        }
                        else if (Model.RelatedTo == "000100781" || Model.Code == "000100781")
                        {
                            Model.Narr += "Permit 1 Year: [" + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F001").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + " ] : [ " + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F002").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + " ]";
                        }
                        else if (Model.RelatedTo == "000100953" || Model.Code == "000100953")
                        {
                            Model.Narr += "Permit 5 Year: [" + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F001").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + " ] : [ " + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F002").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + " ]";
                        }
                        else if (Model.RelatedTo == "000101135" || Model.Code == "000101135")
                        {
                            Model.Narr += "TP(Temporary Permit) Year: [" + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F001").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + "] - [" + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F002").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + " ]";
                        }
                        else if (Model.RelatedTo == "000100788" || Model.Code == "000100788")
                        {
                            Model.Narr += "PT :- Year: [" + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F001").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + "] - [" + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F002").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + " ]";
                        }
                        else if (Model.RelatedTo == "000100789" || Model.Code == "000100789")
                        {
                            Model.Narr += "PUC :- Year: [" + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F001").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + "] - [" + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F002").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + " ]";
                        }
                        else if (Model.RelatedTo == "000100326" || Model.Code == "000100326")
                        {
                            Model.Narr += "Insurance :- Year: [" + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F001").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + "] - [" + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F002").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + " ]";
                        }
                        else if (Model.RelatedTo == "000100951" || Model.Code == "000100951")
                        {
                            Model.Narr += "Loan :- Year: [" + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F001").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + "] - [" + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F002").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + " ]";
                        }
                        else if (Model.RelatedTo == "000100346" || Model.Code == "000100346")
                        {
                            Model.Narr += "\nSpare Parts Repairs : " + " Receipt No : " + Model.AddOnList.Where(x => x.Fld == "F001").Select(X => X.ApplCode).FirstOrDefault() + ", Receipt Date : " + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F002").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString();
                        }
                        else if (Model.RelatedTo == "000100341" || Model.Code == "000100341")
                        {
                            Model.Narr += "Trip :- Start Date : " + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F001").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + ",  End Date :" + Convert.ToDateTime(Model.AddOnList.Where(x => x.Fld == "F002").Select(X => X.ApplCode).FirstOrDefault()).ToShortDateString() + ", Days : " + Model.AddOnList.Where(x => x.Fld == "F003").Select(X => X.ApplCode).FirstOrDefault() + ", kms : " + Model.AddOnList.Where(x => x.Fld == "F004").Select(X => X.ApplCode).FirstOrDefault() + " - " + Model.AddOnList.Where(x => x.Fld == "F005").Select(X => X.ApplCode).FirstOrDefault();
                        }
                        else if (Model.RelatedTo == "000100345" || Model.Code == "000100345")
                        {
                            Model.Narr += "\nTyre :- Type : " + Model.AddOnList.Where(x => x.Fld == "F001").Select(X => X.ApplCode).FirstOrDefault() + ", Place NO : " + Model.AddOnList.Where(x => x.Fld == "F002").Select(X => X.ApplCode).FirstOrDefault() + ", Serial NO : " + Model.AddOnList.Where(x => x.Fld == "F003").Select(X => X.ApplCode).FirstOrDefault();
                        }
                        if (!String.IsNullOrEmpty(Model.RelatedChoice))
                        {
                            Model.Narr += ", Refer Account : " + ctxTFAT.Master.Where(x => x.Code.Trim() == Model.RelatedChoice.Trim()).Select(x => x.Name.ToUpper()).FirstOrDefault();
                        }
                    }

                }


                #endregion

                #region Set BTN Concept Of Branch Time Of Payment And Receipt

                var mAcc = ctxTFAT.Master.Where(x => x.Code == Model.Code).Select(x => new { x.RelatedTo, x.Name }).FirstOrDefault();
                if (mAcc.RelatedTo != "Branch")
                {
                    Model.BTNNO = null;
                    Model.BTNNOCombo = null;
                }
                else
                {
                    Model.BTNNOComboN = " BTN No-:  " + Model.BTNNOCombo + " / Amt:- " + Model.Amount.ToString("F2") + " / Date:- " + Model.DocDate.ToShortDateString();
                }

                #endregion


                objledgerdetail.Add(new CreditPurchaseVM()
                {

                    Code = Model.Code,
                    AccountName = Model.AccountName,
                    LRDetailList = Model.LRDetailList,
                    FMDetailList = Model.FMDetailList,
                    AddOnList = Model.AddOnList,
                    ItemList = Model.ItemList,
                    Debit = (Model.AmtType == "Payment") ? Model.Amt : 0,
                    Credit = (Model.AmtType == "Receipt") ? Model.Amt : 0,
                    Narr = Model.Narr,
                    Amt = Model.Amt,
                    AmtType = Model.AmtType,
                    tempId = objledgerdetail.Count + 1,
                    RelatedChoice = Model.RelatedChoice,
                    RelatedTo = Model.RelatedTo,
                    TyreStockList = Model.TyreStockList,
                    PartialDivName = Model.PartialDivName,
                    BTNNO = Model.BTNNO,
                    BTNNOCombo = Model.BTNNOCombo,
                    BTNNOComboN = Model.BTNNOComboN,
                    BTNBalAmt = Model.BTNBalAmt,
                    BTNTotalAmt = Model.BTNTotalAmt,
                    ReferAccReq = Model.ReferAccReq,
                    CostCenterTally = Model.CostCenterTally,
                    GSTFlag = Model.GSTFlag,
                    GSTCode = Model.GSTCode,
                    Taxable = Model.Taxable,
                    IGSTRate = Model.IGSTRate,
                    CGSTRate = Model.CGSTRate,
                    SGSTRate = Model.SGSTRate,
                    CGSTAmt = Model.CGSTAmt,
                    SGSTAmt = Model.SGSTAmt,
                    IGSTAmt = Model.IGSTAmt,
                    HSNCODE = Model.HSNCODE,
                    //AccReqRelated = Model.AccReqRelated,
                });
                TempData["PURReferAccount"] = string.IsNullOrEmpty(Model.RelatedChoice) == true ? "" : Model.RelatedChoice;

                Session.Add("Lothertrxlist", objledgerdetail);
                decimal sumdebit = objledgerdetail.Sum(x => x.Debit);
                decimal sumcredit = objledgerdetail.Sum(x => x.Credit);

                decimal TdsAmt = 0, IGST = 0, SGST = 0, CGST = 0;
                if (Model.TDSFlag)
                {
                    TdsAmt = (sumdebit * Model.TDSRate) / 100;
                }

                foreach (var item in objledgerdetail)
                {
                    if (item.GSTFlag)
                    {
                        IGST += (item.Taxable * item.IGSTRate) / 100;
                        SGST += (item.Taxable * item.SGSTRate) / 100;
                        CGST += (item.Taxable * item.CGSTRate) / 100;
                    }
                }

                var InvAmt = sumdebit + Math.Round(IGST, 2) + Math.Round(SGST, 2) + Math.Round(CGST, 2);



                var html = ViewHelper.RenderPartialView(this, "LedgerList", new CreditPurchaseVM() { Selectedleger = objledgerdetail });
                var jsonResult = Json(new { IGST = IGST, SGST = SGST, CGST = CGST, InvAmt = InvAmt, TdsAmt = TdsAmt, Selectedleger = objledgerdetail, Html = html, Sumdebit = sumdebit, Sumcredit = sumcredit }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
            else
            {
                var objledgerdetail = (List<CreditPurchaseVM>)Session["Lothertrxlist"];

                #region Set BTN Concept Of Branch Time Of Payment And Receipt

                var mAcc = ctxTFAT.Master.Where(x => x.Code == Model.Code).Select(x => new { x.RelatedTo, x.Name }).FirstOrDefault();
                if (mAcc.RelatedTo != "Branch")
                {
                    Model.BTNNO = null;
                    Model.BTNNOCombo = null;
                }
                else
                {
                    Model.BTNNOComboN = " BTN No-:  " + Model.BTNNOCombo + " / Amt:- " + Model.Amount.ToString("F2") + " / Date:- " + Model.DocDate.ToShortDateString();
                }

                #endregion

                foreach (var item in objledgerdetail.Where(x => x.tempId == Model.tempId))
                {
                    item.Code = Model.Code;
                    item.AccountName = Model.AccountName;
                    item.LRDetailList = Model.LRDetailList;
                    item.FMDetailList = Model.FMDetailList;
                    item.AddOnList = Model.AddOnList;
                    item.ItemList = Model.ItemList;
                    item.Debit = (Model.AmtType == "Payment") ? Model.Amt : 0;
                    item.Credit = (Model.AmtType == "Receipt") ? Model.Amt : 0;
                    item.Narr = Model.Narr;
                    item.Amt = Model.Amt;
                    item.AmtType = Model.AmtType;
                    item.RelatedChoice = Model.RelatedChoice;
                    item.RelatedTo = Model.RelatedTo;
                    item.Narr = Model.Narr;
                    item.TyreStockList = Model.TyreStockList;
                    item.PartialDivName = Model.PartialDivName;
                    item.BTNNO = Model.BTNNO;
                    item.BTNNOCombo = Model.BTNNOCombo;
                    item.BTNNOComboN = Model.BTNNOComboN;
                    item.BTNBalAmt = Model.BTNBalAmt;
                    item.BTNTotalAmt = Model.BTNTotalAmt;
                    item.ReferAccReq = Model.ReferAccReq;
                    item.CostCenterTally = Model.CostCenterTally;
                    item.GSTFlag = Model.GSTFlag;
                    item.GSTCode = Model.GSTCode;
                    item.Taxable = Model.Taxable;
                    item.IGSTRate = Model.IGSTRate;
                    item.CGSTRate = Model.CGSTRate;
                    item.SGSTRate = Model.SGSTRate;
                    item.CGSTAmt = Model.CGSTAmt;
                    item.SGSTAmt = Model.SGSTAmt;
                    item.IGSTAmt = Model.IGSTAmt;
                    item.HSNCODE = Model.HSNCODE;
                }
                Session.Add("Lothertrxlist", objledgerdetail);
                decimal sumdebit = objledgerdetail.Sum(x => x.Debit);
                decimal sumcredit = objledgerdetail.Sum(x => x.Credit);
                decimal TdsAmt = 0, IGST = 0, SGST = 0, CGST = 0;
                if (Model.TDSFlag)
                {
                    TdsAmt = (sumdebit * Model.TDSRate) / 100;
                }
                foreach (var item in objledgerdetail)
                {
                    if (item.GSTFlag)
                    {
                        IGST += (item.Taxable * item.IGSTRate) / 100;
                        SGST += (item.Taxable * item.SGSTRate) / 100;
                        CGST += (item.Taxable * item.CGSTRate) / 100;
                    }
                }

                var InvAmt = sumdebit + Math.Round(IGST, 2) + Math.Round(SGST, 2) + Math.Round(CGST, 2);

                var html = ViewHelper.RenderPartialView(this, "LedgerList", new CreditPurchaseVM() { Selectedleger = objledgerdetail });
                var jsonResult = Json(new { IGST = IGST, SGST = SGST, CGST = CGST, InvAmt = InvAmt, TdsAmt = TdsAmt, Selectedleger = objledgerdetail, Html = html, Sumdebit = sumdebit, Sumcredit = sumcredit }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
        }
        [HttpPost]
        public ActionResult Deleteledger(CreditPurchaseVM Model)
        {
            var result = (List<CreditPurchaseVM>)Session["Lothertrxlist"];

            var result2 = result.Where(x => x.tempId != Model.tempId).Select(x => x).ToList();
            Session.Add("Lothertrxlist", result2);
            decimal sumdebit = result2.Sum(x => x.Debit);
            decimal sumcredit = result2.Sum(x => x.Credit);
            int i = 1;
            decimal TdsAmt = 0, IGST = 0, SGST = 0, CGST = 0;
            foreach (var item in result2)
            {
                if (item.GSTFlag)
                {
                    IGST += (sumdebit * item.IGSTRate) / 100;
                    SGST += (sumdebit * item.SGSTRate) / 100;
                    CGST += (sumdebit * item.CGSTRate) / 100;
                }
                item.tempId = i++;
            }

            if (Model.TDSFlag)
            {
                TdsAmt = (sumdebit * Model.TDSRate) / 100;
            }

            var InvAmt = sumdebit + Math.Round(IGST, 2) + Math.Round(SGST, 2) + Math.Round(CGST, 2);




            var html = ViewHelper.RenderPartialView(this, "LedgerList", new CreditPurchaseVM() { Selectedleger = result2 });
            var jsonResult = Json(new
            {
                Selectedleger = result2,
                Html = html,
                Sumdebit = sumdebit,
                Sumcredit = sumcredit,
                IGST = IGST,
                SGST = SGST,
                CGST = CGST,
                InvAmt = InvAmt,
                TdsAmt = TdsAmt,

            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public ActionResult GetCashLedger(CreditPurchaseVM Model)
        {
            string Branch = "", Message = "", Status = "";
            if (Model.SessionFlag == "Add")
            {
                int Srl = Convert.ToInt32(Model.Srl);
                var Purchase = ctxTFAT.Purchase.Where(x => x.Code == Model.BankCashCode && x.BillNumber.Trim() == Model.BillNumber.Trim() && x.Srl != Srl).FirstOrDefault();
                if (Purchase != null)
                {
                    Status = "Error";
                    Message = "Bill Number Found Please Change The BillNumber...!";
                }
            }


            if (Model.SessionFlag == "Edit")
            {

                var result = (List<CreditPurchaseVM>)Session["Lothertrxlist"];
                var result1 = result.Where(x => x.tempId == Model.tempId);
                foreach (var item in result1)
                {
                    //var Count = ctxTFAT.TyreStockSerial.Where(x => x.ParentKey == item.TableKey).ToList().Count();

                    Model.Code = item.Code;
                    Model.AccountName = item.AccountName;
                    Model.LRDetailList = item.LRDetailList;
                    Model.FMDetailList = item.FMDetailList;
                    Model.tempId = Model.tempId;
                    Model.AddOnList = GetSessionEditTruckWiseData(item);
                    Model.ItemList = GetSessionEditItemWiseData(item);
                    Model.Debit = (item.AmtType == "Payment") ? Model.Amt : 0;
                    Model.Credit = (item.AmtType == "Receipt") ? Model.Amt : 0;
                    Model.Narr = item.Narr;
                    Model.Amt = item.Amt;
                    Model.AmtType = item.AmtType;
                    Model.RelatedTo = item.RelatedTo;
                    Model.RelatedChoice = item.RelatedChoice;
                    Model.TyreStockList = item.TyreStockList;
                    Model.DocumentList = item.DocumentList;
                    Model.BTNNO = item.BTNNO;
                    Model.BTNNOCombo = item.BTNNOCombo;
                    Model.BTNNOComboN = item.BTNNOComboN;
                    Model.BTNBalAmt = item.BTNBalAmt;
                    Model.BTNTotalAmt = item.BTNTotalAmt;
                    //Model.AccReqRelated = item.AccReqRelated;
                    Model.TableKey = item.TableKey;
                    Model.AuthReq = item.AuthReq == false ? true : false;
                    Model.RelatedChoiceN = ctxTFAT.Master.Where(x => x.Code == Model.RelatedChoice).Select(x => x.Name).FirstOrDefault();
                    Model.RelatedToN = ctxTFAT.Master.Where(x => x.Code == Model.RelatedTo).Select(x => x.Name).FirstOrDefault();
                    Model.ReferAccReq = item.ReferAccReq;
                    Model.CostCenterTally = item.CostCenterTally;
                    Model.SaveCostCenter = Model.AddOnList != null && Model.AddOnList.Count() > 0 ? true : false;
                    if (Model.RelatedTo == "000100345" || Model.Code == "000100345")
                    {
                        var TyreType = Model.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                        if (String.IsNullOrEmpty(TyreType))
                        {
                            Model.SaveCostCenter = false;
                        }
                    }
                    if (Model.RelatedChoice == "Tyrestock")
                    {
                        Model.RelatedChoiceN = "Tyre Stock";
                    }
                    else if (Model.RelatedChoice == "Remould")
                    {
                        Model.RelatedChoiceN = "Remould";
                    }


                    string[] DefaultCost = new string[] { "000100949", "000100950", "000100811", "000100343", "000100736", "000100653", "000100326", "000100951", "000100952", "000100404", "000100781", "000100953", "000100954", "000100788", "000101135", "000100789", "000100346", "000100341", "000100345" };
                    var mAcc = ctxTFAT.Master.Where(x => x.Code == Model.Code).Select(x => new { x.RelatedTo, x.Name }).FirstOrDefault();

                    if (DefaultCost.Contains(Model.Code))
                    {
                        Model.PartialDivName = "Y";
                    }
                    else if (mAcc.RelatedTo == "LR")
                    {
                        Model.PartialDivName = "LR";
                    }
                    else if (mAcc.RelatedTo == "FM")
                    {
                        Model.PartialDivName = "FM";
                    }
                    else if (mAcc.RelatedTo == "Branch")
                    {
                        Model.PartialDivName = "Branch";
                        Branch = ViewHelper.RenderPartialView(this, "BranchDetails", Model);
                    }
                    else
                    {
                        Model.PartialDivName = "EY";
                    }

                    Model.GSTCodeName = ctxTFAT.TaxMaster.Where(x => x.Code.ToString() == item.GSTCode).Select(x => x.Name).FirstOrDefault();
                    Model.GSTFlag = item.GSTFlag;
                    Model.GSTCode = item.GSTCode;
                    Model.Taxable = item.Taxable;
                    Model.IGSTRate = item.IGSTRate;
                    Model.CGSTRate = item.CGSTRate;
                    Model.SGSTRate = item.SGSTRate;
                    Model.CGSTAmt = item.CGSTAmt;
                    Model.SGSTAmt = item.SGSTAmt;
                    Model.IGSTAmt = item.IGSTAmt;
                    if (Model.ItemList.Count() > 0)
                    {
                        Model.HSNCODE = item.HSNCODE;
                        Model.HSNCODEName = ctxTFAT.HSNMaster.Where(x => x.Code == Model.HSNCODE).Select(x => x.Name).FirstOrDefault();

                        Model.Item = Model.ItemList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                        Model.ItemName = ctxTFAT.ItemMaster.Where(x => x.Code == Model.Item).Select(x => x.Name).FirstOrDefault();
                    }
                }
                Model.Selectedleger = result;
                Model.Mode = "Edit";
            }
            else
            {
                Model.AmtType = "Payment";
                Model.Mode = "Add";
                Model.SaveCostCenter = true;

                List<AddOns> truckaddonlist = new List<AddOns>();
                var mItem = ctxTFAT.ItemMaster.Where(x => x.BaseGr == Model.ProductGroupType).Select(x => new { x.Name, x.Code }).Distinct().OrderBy(x => x.Name).ToList();
                var mItemGroup = ctxTFAT.ItemGroups.Where(x => x.Hide == false).Select(x => new { x.Name, x.Code }).Distinct().ToList();
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "Product Group",
                    ApplCode = Model.ProductGroupType,
                    QueryText = "Select^" + String.Join("^", mItemGroup.Select(x => x.Name).ToList()),
                    QueryCode = "Select^" + String.Join("^", mItemGroup.Select(x => x.Code).ToList()),
                    FldType = "C",
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F002",
                    Head = "Pick From",
                    ApplCode = Model.FromType,
                    QueryText = "Master^Direct",
                    QueryCode = "Master^Direct",
                    FldType = "C",
                    Hide = true
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F003",
                    Head = "Item",
                    ApplCode = "",
                    QueryText = "Select^" + String.Join("^", mItem.Select(x => x.Name).ToList()),
                    QueryCode = "Select^" + String.Join("^", mItem.Select(x => x.Code).ToList()),
                    FldType = Model.FromType == "Direct" ? "T" : "C",
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F004",
                    Head = "Cost",
                    ApplCode = "0",
                    QueryText = "",
                    QueryCode = "",
                    FldType = "N",
                    InActive = true
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F005",
                    Head = "Qty",
                    ApplCode = "1",
                    QueryText = "",
                    QueryCode = "",
                    FldType = "N",
                    InActive = true
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F006",
                    Head = "Total Amount",
                    ApplCode = "0",
                    QueryText = "",
                    QueryCode = "",
                    FldType = "N",
                    InActive = true
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F007",
                    Head = "Warranty KM",
                    ApplCode = "0",
                    QueryText = "",
                    FldType = "N"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F008",
                    Head = "Install KM",
                    ApplCode = "0",
                    QueryText = "",
                    FldType = "N",

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F009",
                    Head = "Due km",
                    ApplCode = "0",
                    QueryText = "",
                    FldType = "N"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F010",
                    Head = "Warranty Days",
                    ApplCode = "0",
                    QueryText = "",
                    FldType = "N",

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F011",
                    Head = "MFG Date",
                    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                    QueryText = "",
                    FldType = "D",
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F012",
                    Head = "Install / Received Date",
                    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                    QueryText = "",
                    FldType = "D"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F013",
                    Head = "Due Date",
                    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                    QueryText = "",
                    FldType = "D",

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F014",
                    Head = "HSN CODE",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "X",
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F015",
                    Head = "Description",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "M",
                });
                Model.ItemList = truckaddonlist;


                Model.RelatedChoice = TempData.Peek("PURReferAccount").ToString();
                Model.RelatedChoiceN = ctxTFAT.Master.Where(x => x.Code == Model.RelatedChoice).Select(x => x.Name).FirstOrDefault();
            }


            var jsonResult = Json(new { Status = Status, Message = Message, Branch = Branch, Html = this.RenderPartialView("AddEditTransaction", Model) }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;



        }

        [HttpPost]
        public ActionResult ConfirmRelationAddEdit(CreditPurchaseVM Model)
        {
            string mMessage = "";
            Master master = ctxTFAT.Master.Where(x => x.Code == Model.Code).FirstOrDefault();
            //Branch Validation
            if (ctxTFAT.Master.Where(x => x.Code == Model.Code).Select(x => x.RelatedTo).FirstOrDefault() == "Branch")
            {
                string mmessage = "";
                if (Model.AmtType == "Receipt")
                {
                    if (!String.IsNullOrEmpty(Model.BTNNOCombo))
                    {
                        var Amt = Model.BTNBalAmt;
                        if (Convert.ToDecimal(Amt) < Model.Amt)
                        {
                            Model.Status = "ConfirmError1";
                            mmessage = mmessage + "Your Amount Not Greater Than Of BTN Bal Amount. Please Check It.....!";
                        }
                    }
                    else
                    {
                        Model.Status = "ConfirmError1";
                        mmessage = mmessage + "Please Select Settle BTN No.....!";
                    }
                }


                if (!String.IsNullOrEmpty(mmessage))
                {
                    Model.Message = mmessage;
                    return Json(Model, JsonRequestBehavior.AllowGet);
                }
            }
            if (master.RelatedTo == "LR" && master.ForceCC == true)
            {
                string mmessage = "";
                if (Model.LRDetailList == null)
                {
                    Model.LRDetailList = new List<CreditPurchaseVM>();
                }

                if (Model.LRDetailList.Count() == 0)
                {
                    Model.Status = "ConfirmError1";
                    mmessage = mmessage + "Please Enter The Lr Details In Grid...!";
                }

                if (!String.IsNullOrEmpty(mmessage))
                {
                    Model.Message = mmessage;
                    return Json(Model, JsonRequestBehavior.AllowGet);
                }
            }
            if (master.RelatedTo == "FM" && master.ForceCC == true)
            {
                string mmessage = "";
                if (Model.FMDetailList == null)
                {
                    Model.FMDetailList = new List<CreditPurchaseVM>();
                }

                if (Model.FMDetailList.Count() == 0)
                {
                    Model.Status = "ConfirmError1";
                    mmessage = mmessage + "Please Enter The FM Details In Grid...!";
                }

                if (!String.IsNullOrEmpty(mmessage))
                {
                    Model.Message = mmessage;
                    return Json(Model, JsonRequestBehavior.AllowGet);
                }
            }
            if (String.IsNullOrEmpty(master.RelatedTo) == false && master.RelatedTo != "NA")
            {
                string mmessage = "";
                if (master.ForceCC == true)
                {
                    if (Model.AddOnList == null)
                    {
                        Model.Status = "ConfirmError1";
                        mmessage = mmessage + "Please Enter The Cost Details...!";
                    }
                }
                if (!String.IsNullOrEmpty(mmessage))
                {
                    Model.Message = mmessage;
                    return Json(Model, JsonRequestBehavior.AllowGet);
                }
            }
            //Trip , Loan , Permit(1 Year) ,Insurance , Permit (5 year) , Fitness , PUC , PT , Green Tax Validation
            if (Model.DuplExpDtConfirm == true && (Model.RelatedTo == "000100341" || Model.Code == "000100341" || Model.RelatedTo == "000100951" || Model.Code == "000100951" || Model.RelatedTo == "000100781" || Model.Code == "000100781" || Model.RelatedTo == "000100326" || Model.Code == "000100326" || Model.RelatedTo == "000100953" || Model.Code == "000100953" || Model.RelatedTo == "000100736" || Model.Code == "000100736" || Model.RelatedTo == "000100789" || Model.Code == "000100789" || Model.RelatedTo == "000100788" || Model.Code == "000100788" || Model.RelatedTo == "000100653" || Model.Code == "000100653" || Model.RelatedTo == "000100811" || Model.Code == "000100811"))
            {
                if (Model.AddOnList != null)
                {
                    var mstartdate = Model.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                    DateTime Startdate = Convert.ToDateTime(mstartdate);

                    var menddate = Model.AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault();
                    DateTime Enddate = Convert.ToDateTime(menddate);
                    var FromDate = (Convert.ToDateTime(Startdate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    var TODate = (Convert.ToDateTime(Enddate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

                    string mStr = "", iMTableKey = "";
                    mStr = "select top 1 isnull(TableKey,'') from relatedata where ParentKey<>'" + Model.ParentKey + "' and value8='" + Model.RelatedChoice + "' and code='" + Model.Code + "' and  '" + FromDate + "' <= date2  ";
                    string connstring = GetConnectionString();
                    DataTable smDt = GetDataTable(mStr, connstring);
                    if (smDt.Rows.Count > 0)
                    {
                        iMTableKey = smDt.Rows[0][0].ToString();
                    }

                    //var iMTableKey = ctxTFAT.RelateData.Where(x => x.Combo1 == Model.RelatedTo && x.Code == Model.Code && (mstenddate >= x.Date1 && mstenddate <= x.Date2) && x.ParentKey != Model.ParentKey).Select(x => x.TableKey).FirstOrDefault();
                    if (!(String.IsNullOrEmpty(iMTableKey)))
                    {
                        //string mMessage = "";
                        string Name = "";
                        if (String.IsNullOrEmpty(Model.RelatedChoice))
                        {
                            Name = ctxTFAT.Master.Where(x => x.Code.Trim() == Model.Code).Select(x => x.Name).FirstOrDefault();
                        }
                        else
                        {
                            Name = ctxTFAT.Master.Where(x => x.Code.Trim() == Model.RelatedChoice).Select(x => x.Name).FirstOrDefault();
                        }

                        mMessage = "Already " + Name + " Save Between Entered Days Do you want to Continue.";
                        //mMessage = "Already Record Found  Between Entered Days Do you want to Continue.";
                    }
                }
                if (!String.IsNullOrEmpty(mMessage))
                {
                    Model.Status = "ConfirmError";
                    Model.Message = mMessage;
                    return Json(Model, JsonRequestBehavior.AllowGet);
                }
            }
            // Spare Parts Validation 
            if (Model.DuplExpDtConfirm == true && Model.RelatedTo == "000100346" || Model.Code == "000100346")
            {
                if (Model.ItemList != null)
                {
                    var mpart = Model.ItemList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                    var mInstalldT = Model.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                    DateTime mInstalldate = Convert.ToDateTime(mInstalldT);
                    var iMTableKey = ctxTFAT.RelateData.Where(x => x.Combo1 == Model.RelatedTo && x.Code == Model.Code && (x.Value1 == mpart && mInstalldate >= x.Date3) && x.ParentKey != Model.ParentKey).Select(x => x.TableKey).FirstOrDefault();
                    if (iMTableKey != null)
                    {
                        Model.Status = "ConfirmError";
                        Model.Message = "Same Vehicle Same Part Entered Install date before Due date Confirm Do You want to continue";
                        return Json(Model, JsonRequestBehavior.AllowGet);
                    }
                    var mdecnum2 = Model.ItemList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault();
                    var mNum2 = (string.IsNullOrEmpty(mdecnum2) == true) ? 0 : Convert.ToDecimal(mdecnum2);

                    var iMTableKey2 = ctxTFAT.RelateData.Where(x => x.Combo1 == Model.RelatedTo && x.Code == Model.Code && (x.Value1 == mpart && mNum2 >= x.Num3) && x.ParentKey != Model.ParentKey).Select(x => x.TableKey).FirstOrDefault();
                    if (iMTableKey2 != null)
                    {
                        Model.Status = "ConfirmError";
                        Model.Message = "Same Vehicle Same Part Entered Current KM date before Due KM Confirm Do You want to continue";
                        return Json(Model, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            // Tyre  Validation 
            if (Model.DuplExpDtConfirm == true && Model.RelatedTo == "000100345" || Model.Code == "000100345")
            {
                if (Model.AddOnList != null && Model.ItemList != null)
                {
                    var mtype = Model.ItemList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                    var mInstalldT = Model.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                    var mtyreserialno = Model.AddOnList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault();
                    DateTime mInstalldate = Convert.ToDateTime(mInstalldT);
                    var iMTableKey = ctxTFAT.RelateData.Where(x => x.Combo1 == Model.RelatedTo && x.Code == Model.Code && (x.Value2 == mtype && x.Value6 == mtyreserialno && mInstalldate <= x.Date3) && x.ParentKey != Model.ParentKey).Select(x => x.TableKey).FirstOrDefault();
                    var mdecnum2 = Model.ItemList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault();
                    var mNum2 = (string.IsNullOrEmpty(mdecnum2) == true) ? 0 : Convert.ToDecimal(mdecnum2);

                    var iMTableKey2 = ctxTFAT.RelateData.Where(x => x.Combo1 == Model.RelatedTo && x.Code == Model.Code && (x.Value2 == mtype && x.Value6 == mtyreserialno && mNum2 <= x.Num3) && x.ParentKey != Model.ParentKey).Select(x => x.TableKey).FirstOrDefault();

                    if (iMTableKey != null || iMTableKey2 != null)
                    {
                        Model.Status = "ConfirmError";
                        string mmessage = "";
                        if (iMTableKey != null)
                        {
                            mmessage = mmessage + "Entered Install date before Due date Confirm Do You want to continue";
                        }
                        if (iMTableKey2 != null)
                        {
                            mmessage = mmessage + " Entered Current KM date before Due KM Confirm Do You want to continue";
                        }
                        Model.Message = mmessage;

                        if (!String.IsNullOrEmpty(mmessage))
                        {
                            Model.Message = mmessage;
                            return Json(Model, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }

            Model.Status = "Success";
            Model.Message = "";
            return Json(Model, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region ADD LR details

        public ActionResult AddLRDetails(CreditPurchaseVM Model)
        {
            List<CreditPurchaseVM> lrdetaillist = new List<CreditPurchaseVM>();
            var mlrmaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == Model.ConsignmentKey).Select(x => x).FirstOrDefault();

            if (mlrmaster == null)
            {
                Model.Status = "ValidError";
                Model.Message = "Consignment Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            if (Model.SessionFlag == "Add")
            {
                var mRellR = (from r in ctxTFAT.RelLr
                              join rd in ctxTFAT.RelateData on r.TableKey equals rd.TableKey
                              where r.LRRefTablekey == Model.ConsignmentKey && rd.Code == Model.Code
                              select r.TableKey).FirstOrDefault();
                if (mRellR != null)
                {
                    if (Model.NoDuplExpLRFM == true)
                    {
                        Model.Status = "ValidError";
                        Model.Message = /*"LR Already Save Cant Save.."*/"Found Same Expenses on same Document No Cant Save";
                        return Json(Model, JsonRequestBehavior.AllowGet);
                    }
                }


                if (Model.LRDetailList != null)
                {
                    lrdetaillist = Model.LRDetailList;
                }

                if (lrdetaillist.Count() > 0 && (lrdetaillist.Select(x => x.ConsignmentKey).Contains(Model.ConsignmentKey)))
                {
                    Model.Status = "ValidError";
                    Model.Message = "Consignment Already in List Cant Save..";
                    return Json(Model, JsonRequestBehavior.AllowGet);
                }
                if (String.IsNullOrEmpty(Model.ConsignmentKey))
                {
                    Model.Status = "ConfirmError";
                    Model.Message = "Please Enter The Consignment No..";
                    return Json(Model, JsonRequestBehavior.AllowGet);
                }
                //if (Model.LRAmt == 0)
                //{
                //    Model.Status = "ValidError";
                //    Model.Message = "Please Enter The Amount..";
                //    return Json(Model, JsonRequestBehavior.AllowGet);
                //}
                lrdetaillist.Add(new CreditPurchaseVM()
                {
                    ConsignmentKey = Model.ConsignmentKey,
                    LRNumber = Model.LRNumber,
                    LRAmt = Model.LRAmt,
                    tempId = lrdetaillist.Count + 1,

                });

            }
            else
            {
                var mRellR = (from r in ctxTFAT.RelLr
                              join rd in ctxTFAT.RelateData on r.TableKey equals rd.TableKey
                              where r.LRRefTablekey == Model.ConsignmentKey && rd.Code == Model.Code && rd.ParentKey != Model.ParentKey
                              select r.TableKey).FirstOrDefault();
                if (mRellR != null)
                {
                    if (Model.NoDuplExpLRFM == true)
                    {
                        Model.Status = "ValidError";
                        Model.Message = /*"LR Already Save Cant Save.."*/"Found Same Expenses on same Document No Cant Save";
                        return Json(Model, JsonRequestBehavior.AllowGet);
                    }
                }
                //if (Model.LRAmt == 0)
                //{
                //    Model.Status = "ValidError";
                //    Model.Message = "Please Enter The Amount..";
                //    return Json(Model, JsonRequestBehavior.AllowGet);
                //}

                if (Model.LRDetailList != null)
                {
                    lrdetaillist = Model.LRDetailList;
                }

                foreach (var item in lrdetaillist.Where(x => x.tempId == Model.tempId))
                {

                    item.ConsignmentKey = Model.ConsignmentKey;
                    item.LRNumber = Model.LRNumber;
                    item.LRAmt = Model.LRAmt;
                    item.tempId = Model.tempId;
                }
            }


            var html = ViewHelper.RenderPartialView(this, "LRDetails", new CreditPurchaseVM() { LRDetailList = lrdetaillist });
            return Json(new { LRDetailList = lrdetaillist, Html = html, Amt = lrdetaillist.Sum(x => x.LRAmt) }, JsonRequestBehavior.AllowGet);


        }

        [HttpPost]
        public ActionResult DeleteLRDetails(CreditPurchaseVM Model)
        {


            var result2 = Model.LRDetailList.Where(x => x.tempId != Model.tempId).Select(x => x).ToList();

            var html = ViewHelper.RenderPartialView(this, "LRDetails", new CreditPurchaseVM() { LRDetailList = result2 });
            return Json(new { LRDetailList = result2, Html = html, Amt = result2.Sum(x => x.LRAmt) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetLRDetails(CreditPurchaseVM Model)
        {
            if (Model.LRDetailList != null && Model.LRDetailList.Count() > 0)
            {
                foreach (var a in Model.LRDetailList.Where(x => x.tempId == Model.tempId))
                {
                    Model.LRNumber = a.LRNumber;
                    Model.LRAmt = a.LRAmt;
                    Model.tempId = a.tempId;
                    Model.ConsignmentKey = a.ConsignmentKey;
                }
            }

            var html = ViewHelper.RenderPartialView(this, "LRDetails", Model);
            return Json(new { ConsignmentKey = Model.ConsignmentKey, Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetLrMasterDetails(CreditPurchaseVM Model)
        {
            var mlrmaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == Model.LRNumber).Select(x => x).FirstOrDefault();

            if (mlrmaster == null)
            {
                Model.Status = "ValidError";
                Model.Message = "Consignment Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            if (mlrmaster != null)
            {
                Model.ConsignmentKey = mlrmaster.TableKey.ToString();
                Model.LRNumber = mlrmaster.LrNo.ToString();
                Model.DocDate = mlrmaster.BookDate;
                Model.Qty = mlrmaster.TotQty;
                Model.ActWt = mlrmaster.ActWt;
                Model.ChgWt = mlrmaster.ChgWt;
                Model.LRFrom = ctxTFAT.TfatBranch.Where(x => x.Code == mlrmaster.Source).Select(x => x.Name).FirstOrDefault();
                Model.LRTo = ctxTFAT.TfatBranch.Where(x => x.Code == mlrmaster.Dest).Select(x => x.Name).FirstOrDefault();
                Model.LRConginer = ctxTFAT.Consigner.Where(x => x.Code == mlrmaster.SendCode).Select(x => x.Name).FirstOrDefault();
                Model.LRConsignee = ctxTFAT.Consigner.Where(x => x.Code == mlrmaster.RecCode).Select(x => x.Name).FirstOrDefault();
                var Setup = ctxTFAT.CreditPurchaseSetup.FirstOrDefault();
                if (Setup != null)
                {
                    if (Setup.ShowConsignmentExp)
                    {
                        var result = (from lrExp in ctxTFAT.RelLr
                                      where lrExp.LRRefTablekey == mlrmaster.TableKey
                                      join Relateda in ctxTFAT.RelateData.Where(x => x.AmtType == true)
                                      on lrExp.TableKey equals Relateda.TableKey
                                      select new CreditPurchaseVM()
                                      {
                                          Amt = lrExp.LrAmt.Value,
                                          AccountName = ctxTFAT.Master.Where(x => x.Code == Relateda.Code).Select(x => x.Name).FirstOrDefault(),
                                          DocDate = Relateda.DocDate.Value,
                                          EnteredBy = Relateda.ENTEREDBY,
                                      }).OrderBy(x => x.DocDate).ToList();
                        Model.ConsignmentExplist = result;
                    }
                }
            }
            else
            {
                Model.ConsignmentExplist = new List<CreditPurchaseVM>();
            }

            var html = ViewHelper.RenderPartialView(this, "LRMasterDetails", Model);
            return Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FetchDocumentList(CreditPurchaseVM Model)
        {
            List<CreditPurchaseVM> ValueList = new List<CreditPurchaseVM>();

            var mlrmaster = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == Model.LRNumber).Select(x => x).ToList();
            if (mlrmaster == null)
            {
                Model.Status = "ValidError";
                Model.Message = "Consignment Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            if (mlrmaster.Count() == 0)
            {
                Model.Status = "ValidError";
                Model.Message = "Consignment Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            else if (mlrmaster.Count() == 1)
            {
                Model.Status = "Processed";
                Model.Message = mlrmaster.Select(x => x.TableKey).FirstOrDefault();
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            foreach (var item in mlrmaster)
            {
                CreditPurchaseVM otherTransact = new CreditPurchaseVM();
                otherTransact.ConsignmentKey = item.TableKey.ToString();
                otherTransact.LRNumber = item.LrNo.ToString();
                otherTransact.DocDate = item.BookDate;
                otherTransact.Qty = item.TotQty;
                otherTransact.ActWt = item.ActWt;
                otherTransact.ChgWt = item.ChgWt;
                otherTransact.LRFrom = ctxTFAT.TfatBranch.Where(x => x.Code == item.Source).Select(x => x.Name).FirstOrDefault();
                otherTransact.LRTo = ctxTFAT.TfatBranch.Where(x => x.Code == item.Dest).Select(x => x.Name).FirstOrDefault();
                otherTransact.LRConginer = ctxTFAT.Consigner.Where(x => x.Code == item.SendCode).Select(x => x.Name).FirstOrDefault();
                otherTransact.LRConsignee = ctxTFAT.Consigner.Where(x => x.Code == item.RecCode).Select(x => x.Name).FirstOrDefault();
                ValueList.Add(otherTransact);
            }
            Model.ValueList = ValueList;
            var html = ViewHelper.RenderPartialView(this, "ConsignmentList", Model);
            return Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ConfirmLRSave(CreditPurchaseVM Model)
        {
            if (Model.SessionFlag == "Add")
            {
                var Setup = ctxTFAT.CreditPurchaseSetup.FirstOrDefault();
                if (Setup != null)
                {
                    if (Setup.RestrictLrDateExp)
                    {
                        var Days = String.IsNullOrEmpty(Setup.RestrictLrExpDays) == true ? 0 : Convert.ToInt32(Setup.RestrictLrExpDays);
                        var DocumentDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                        DocumentDate = DocumentDate.AddDays(-Days);

                        var ConsignemtDate = ctxTFAT.LRMaster.Where(x => x.TableKey == Model.ConsignmentKey).Select(x => x.BookDate).FirstOrDefault();
                        if (!(DocumentDate <= ConsignemtDate))
                        {
                            Model.Status = "ValidError";
                            Model.Message = "Consignemt Date Always Greater Than '" + DocumentDate.ToShortDateString() + "' Only...";
                        }

                    }
                }

                var mRellR = (from r in ctxTFAT.RelLr
                              join rd in ctxTFAT.RelateData on r.TableKey equals rd.TableKey
                              where r.LRRefTablekey == Model.ConsignmentKey && rd.Code == Model.Code
                              select r.TableKey).FirstOrDefault();

                if (mRellR != null)
                {
                    if (Model.DuplExpLRFMConfirm == true)
                    {
                        Model.Status = "ConfirmError";
                        Model.Message = "Document has already same Charges Do you want to Continue..";

                    }
                }
            }
            else
            {
                var mRellR = (from r in ctxTFAT.RelLr
                              join rd in ctxTFAT.RelateData on r.TableKey equals rd.TableKey
                              where r.LRRefTablekey == Model.ConsignmentKey && rd.Code == Model.Code && rd.ParentKey != Model.ParentKey
                              select r.TableKey).FirstOrDefault();

                if (mRellR != null)
                {
                    if (Model.DuplExpLRFMConfirm == true)
                    {
                        Model.Status = "ConfirmError";
                        Model.Message = "Document has already same Charges Do you want to Continue..";
                        return Json(new { Status = Model.Status, Message = Model.Message }, JsonRequestBehavior.AllowGet);
                    }
                }

            }


            return Json(new { Status = Model.Status, Message = Model.Message }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region ADD FM details

        public ActionResult AddFMDetails(CreditPurchaseVM Model)
        {
            List<CreditPurchaseVM> FMdetaillist = new List<CreditPurchaseVM>();
            if (Model.SessionFlag == "Add")
            {

                var mRellR = (from r in ctxTFAT.RelFm
                              join rd in ctxTFAT.RelateData on r.TableKey equals rd.TableKey
                              where r.FMRefTablekey == Model.FreightMemoKey && rd.Code == Model.Code
                              select r.TableKey).FirstOrDefault();
                if (mRellR != null)
                {
                    if (Model.NoDuplExpLRFM == true)
                    {
                        Model.Status = "ValidError";
                        Model.Message = /*"FM Already Save Cant Save.."*/"Found Same Expenses on same Document No Cant Save";
                        return Json(Model, JsonRequestBehavior.AllowGet);
                    }
                }

                if (Model.FMDetailList != null)
                {
                    FMdetaillist = Model.FMDetailList;
                }

                if (FMdetaillist.Count() > 0 && (FMdetaillist.Select(x => x.FreightMemoKey).Contains(Model.FreightMemoKey)))
                {
                    Model.Status = "ValidError";
                    Model.Message = "Freight Memo Already in List Cant Save..";
                    return Json(Model, JsonRequestBehavior.AllowGet);
                }
                if (String.IsNullOrEmpty(Model.FMNumber))
                {
                    Model.Status = "ConfirmError";
                    Model.Message = "Please Enter The Freight Memo No..";
                    return Json(Model, JsonRequestBehavior.AllowGet);
                }
                //if (Model.FMAmt == 0)
                //{
                //    Model.Status = "ValidError";
                //    Model.Message = "Please Enter The Amount..";
                //    return Json(Model, JsonRequestBehavior.AllowGet);
                //}
                FMdetaillist.Add(new CreditPurchaseVM()
                {
                    FMNumber = Model.FMNumber,
                    FMAmt = Model.FMAmt,
                    tempId = FMdetaillist.Count + 1,
                    FreightMemoKey = Model.FreightMemoKey,
                });
            }
            else
            {
                var mRellR = (from r in ctxTFAT.RelFm
                              join rd in ctxTFAT.RelateData on r.TableKey equals rd.TableKey
                              where r.FMRefTablekey == Model.FreightMemoKey && rd.Code == Model.Code && rd.ParentKey != Model.ParentKey
                              select r.TableKey).FirstOrDefault();
                if (mRellR != null)
                {
                    if (Model.NoDuplExpLRFM == true)
                    {
                        Model.Status = "ValidError";
                        Model.Message = /*"FM Already Save Cant Save.."*/"Found Same Expenses on same Document No Cant Save";
                        return Json(Model, JsonRequestBehavior.AllowGet);
                    }
                }
                //if (Model.FMAmt == 0)
                //{
                //    Model.Status = "ValidError";
                //    Model.Message = "Please Enter The Amount..";
                //    return Json(Model, JsonRequestBehavior.AllowGet);
                //}

                if (Model.FMDetailList != null)
                {
                    FMdetaillist = Model.FMDetailList;
                }

                foreach (var item in FMdetaillist.Where(x => x.tempId == Model.tempId))
                {

                    item.FMNumber = Model.FMNumber;
                    item.FMAmt = Model.FMAmt;
                    item.tempId = Model.tempId;
                    item.FreightMemoKey = Model.FreightMemoKey;
                }
            }

            var html = ViewHelper.RenderPartialView(this, "FMDetails", new CreditPurchaseVM() { FMDetailList = FMdetaillist });
            return Json(new { FMDetailList = FMdetaillist, Html = html, Amt = FMdetaillist.Sum(x => x.FMAmt) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteFMDetails(CreditPurchaseVM Model)
        {
            var result2 = Model.FMDetailList.Where(x => x.tempId != Model.tempId).Select(x => x).ToList();
            var html = ViewHelper.RenderPartialView(this, "FMDetails", new CreditPurchaseVM() { FMDetailList = result2 });
            return Json(new { FMDetailList = result2, Html = html, Amt = result2.Sum(x => x.FMAmt) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetFMDetails(CreditPurchaseVM Model)
        {
            if (Model.FMDetailList != null && Model.FMDetailList.Count() > 0)
            {
                foreach (var a in Model.FMDetailList.Where(x => x.tempId == Model.tempId))
                {
                    Model.FMNumber = a.FMNumber;
                    Model.FMAmt = a.FMAmt;
                    Model.tempId = a.tempId;
                    Model.FreightMemoKey = a.FreightMemoKey;
                }
            }

            var html = ViewHelper.RenderPartialView(this, "FMDetails", Model);
            return Json(new { FreightMemoKey = Model.FreightMemoKey, Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetFMMasterDetails(CreditPurchaseVM Model)
        {
            var mlrmaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == Model.FMNumber).Select(x => x).FirstOrDefault();

            if (mlrmaster == null)
            {
                Model.Status = "ValidError";
                Model.Message = "Freight Memo Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }

            if (mlrmaster != null)
            {
                Model.FMNumber = mlrmaster.FmNo.ToString();
                Model.FreightMemoKey = mlrmaster.TableKey.ToString();
                Model.DocDate = mlrmaster.Date;
                Model.Name = ctxTFAT.VehicleCategory.Where(x => x.Code == mlrmaster.VehicleCategory).Select(x => x.VehicleCategory1).FirstOrDefault();
                Model.LRFrom = ctxTFAT.TfatBranch.Where(x => x.Code == mlrmaster.FromBranch).Select(x => x.Name).FirstOrDefault();
                Model.LRTo = ctxTFAT.TfatBranch.Where(x => x.Code == mlrmaster.ToBranch).Select(x => x.Name).FirstOrDefault();
                Model.LRConginer = ctxTFAT.DriverMaster.Where(x => x.Code == mlrmaster.Driver).Select(x => x.Name).FirstOrDefault();
                Model.LRConsignee = mlrmaster.VehicleStatus == "100001" ? ctxTFAT.HireVehicleMaster.Where(x => x.Code == mlrmaster.TruckNo).Select(x => x.TruckNo).FirstOrDefault() : ctxTFAT.VehicleMaster.Where(x => x.Code == mlrmaster.TruckNo).Select(x => x.TruckNo).FirstOrDefault();
            }


            var html = ViewHelper.RenderPartialView(this, "FMMasterDetails", Model);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FetchFreightMemoDocumentList(CreditPurchaseVM Model)
        {
            List<CreditPurchaseVM> ValueList = new List<CreditPurchaseVM>();

            var mlrmaster = ctxTFAT.FMMaster.Where(x => x.FmNo.ToString() == Model.FMNumber).Select(x => x).ToList();
            if (mlrmaster == null)
            {
                Model.Status = "ValidError";
                Model.Message = "Freight Memo Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            if (mlrmaster.Count() == 0)
            {
                Model.Status = "ValidError";
                Model.Message = "Freight Memo Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            else if (mlrmaster.Count() == 1)
            {
                Model.Status = "Processed";
                Model.Message = mlrmaster.Select(x => x.TableKey).FirstOrDefault();
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            foreach (var item in mlrmaster)
            {
                CreditPurchaseVM otherTransact = new CreditPurchaseVM();
                otherTransact.FMNumber = item.FmNo.ToString();
                otherTransact.FreightMemoKey = item.TableKey.ToString();
                otherTransact.DocDate = item.Date;
                otherTransact.Name = ctxTFAT.VehicleCategory.Where(x => x.Code == item.VehicleCategory).Select(x => x.VehicleCategory1).FirstOrDefault();
                otherTransact.LRFrom = ctxTFAT.TfatBranch.Where(x => x.Code == item.FromBranch).Select(x => x.Name).FirstOrDefault();
                otherTransact.LRTo = ctxTFAT.TfatBranch.Where(x => x.Code == item.ToBranch).Select(x => x.Name).FirstOrDefault();
                otherTransact.LRConginer = ctxTFAT.DriverMaster.Where(x => x.Code == item.Driver).Select(x => x.Name).FirstOrDefault();
                otherTransact.LRConsignee = item.VehicleStatus == "100001" ? ctxTFAT.HireVehicleMaster.Where(x => x.Code == item.TruckNo).Select(x => x.TruckNo).FirstOrDefault() : ctxTFAT.VehicleMaster.Where(x => x.Code == item.TruckNo).Select(x => x.TruckNo).FirstOrDefault();
                ValueList.Add(otherTransact);
            }
            Model.ValueList = ValueList;
            var html = ViewHelper.RenderPartialView(this, "FreightMemoList", Model);
            return Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ConfirmFMSave(CreditPurchaseVM Model)
        {

            if (Model.SessionFlag == "Add")
            {
                var mRellR = (from r in ctxTFAT.RelFm
                              join rd in ctxTFAT.RelateData on r.TableKey equals rd.TableKey
                              where r.FMRefTablekey == Model.FreightMemoKey && rd.Code == Model.Code
                              select r.TableKey).FirstOrDefault();

                if (mRellR != null)
                {
                    if (Model.DuplExpLRFMConfirm == true)
                    {
                        Model.Status = "ConfirmError";
                        Model.Message = "Document has already same Charges Do you want to Continue..";

                    }
                }
            }
            else
            {
                var mRellR = (from r in ctxTFAT.RelFm
                              join rd in ctxTFAT.RelateData on r.TableKey equals rd.TableKey
                              where r.FMRefTablekey == Model.FreightMemoKey && rd.Code == Model.Code && rd.ParentKey != Model.ParentKey
                              select r.TableKey).FirstOrDefault();

                if (mRellR != null)
                {
                    if (Model.DuplExpLRFMConfirm == true)
                    {
                        Model.Status = "ConfirmError";
                        Model.Message = "Document has already same Charges Do you want to Continue..";
                        return Json(new { Status = Model.Status, Message = Model.Message }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(new { Status = Model.Status, Message = Model.Message }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public ActionResult RelatedToDecide(CreditPurchaseVM Model)
        {
            string PartialDivName = "", html = "";
            var mAcc = ctxTFAT.Master.Where(x => x.Code == Model.Code).FirstOrDefault();
            if (mAcc.OthPostType.Contains("D"))
            {
                Model.ShowDriverOrVehicle = true;
                Model.ShowDriverOrVehicleLabel = "Current Vehicle : ";
                if (ctxTFAT.DriverMaster.Where(x => x.Posting == mAcc.Code).FirstOrDefault() != null)
                {
                    var Driver = ctxTFAT.DriverMaster.Where(x => x.Posting == mAcc.Code).FirstOrDefault();
                    var GetVehicleCode = ctxTFAT.VehicleDri_Hist.Where(x => x.Driver == Driver.Code).OrderByDescending(x => new { x.FromPeriod, x.FromTime }).Select(x => x.TruckNo).FirstOrDefault();
                    if (!String.IsNullOrEmpty(GetVehicleCode))
                    {
                        Model.ShowDriverOrVehicleLabel += ctxTFAT.VehicleMaster.Where(x => x.Code == GetVehicleCode).Select(x => x.TruckNo).FirstOrDefault();
                    }
                }
            }
            else if (mAcc.OthPostType.Contains("V"))
            {
                Model.ShowDriverOrVehicle = true;
                Model.ShowDriverOrVehicleLabel = "Current Driver : ";
                if (ctxTFAT.VehicleMaster.Where(x => x.PostAc == mAcc.Code).FirstOrDefault() != null)
                {
                    var Vehicle = ctxTFAT.VehicleMaster.Where(x => x.PostAc == mAcc.Code).FirstOrDefault();
                    var GetDriverCode = ctxTFAT.VehicleDri_Hist.Where(x => x.TruckNo == Vehicle.Code).OrderByDescending(x => new { x.FromPeriod, x.FromTime }).Select(x => x.Driver).FirstOrDefault();
                    if (!String.IsNullOrEmpty(GetDriverCode))
                    {
                        Model.ShowDriverOrVehicleLabel += ctxTFAT.DriverMaster.Where(x => x.Code == GetDriverCode).Select(x => x.Name).FirstOrDefault();
                    }
                }
            }
            var mIsVehicle = ctxTFAT.Master.Where(x => x.Code == Model.Code).Select(x => x.OthPostType).FirstOrDefault();

            string[] DefaultCost = new string[] { "000100949", "000100950", "000100811", "000100343", "000100736", "000100653", "000100326", "000100951", "000100952", "000100404", "000100781", "000100953", "000100954", "000100788", "000101135", "000100789", "000100346", "000100341", "000100345" };

            if (DefaultCost.Contains(Model.Code))
            {
                PartialDivName = "Y";
            }
            else if (DefaultCost.Contains(mAcc.RelatedTo))
            {
                PartialDivName = "HY";
            }
            else if (mAcc.RelatedTo == "LR")
            {
                PartialDivName = "LR";
            }
            else if (mAcc.RelatedTo == "FM")
            {
                PartialDivName = "FM";
            }
            else if (mAcc.RelatedTo == "Branch")
            {
                string BTNLast = "";
                var result = (List<CreditPurchaseVM>)Session["Lothertrxlist"];
                if (result != null)
                {
                    BTNLast = result.Where(x => x.BTNNO != "" && x.BTNNO != null && x.AmtType == "Payment").OrderByDescending(x => x.tempId).Select(x => x.BTNNO).FirstOrDefault();
                }
                if (String.IsNullOrEmpty(BTNLast))
                {
                    var BranchLedgerList = ctxTFAT.Master.Where(x => x.RelatedTo.Trim() == "Branch").Select(x => x.Code).ToList();
                    var BTNONO = ctxTFAT.RelateData.Where(x => BranchLedgerList.Contains(x.Code) && x.AmtType == true && x.Char1.Trim() != "" && x.Char1.Trim() != null).OrderByDescending(x => x.RECORDKEY).Select(x => x.Char1).FirstOrDefault();
                    if (String.IsNullOrEmpty(BTNONO))
                    {
                        Model.BTNNO = "1";
                    }
                    else
                    {
                        Model.BTNNO = (Convert.ToInt32(BTNONO) + 1).ToString();
                    }
                }
                else
                {
                    Model.BTNNO = (Convert.ToInt32(BTNLast) + 1).ToString();
                }

                html = ViewHelper.RenderPartialView(this, "BranchDetails", Model);
                PartialDivName = "Branch";
            }
            else
            {
                PartialDivName = "N";
            }
            var ReferAccReq = mAcc.ReferAccReq;
            var CostCenterTally = mAcc.CostCenterAmtTally;
            var AllowToChange = mAcc.AllowToChanges;

            #region GST
            string GSTCode = "0", GSTName = "";
            bool GstFlag = false;
            decimal IGST = 0, CGST = 0, SGST = 0;
            MasterInfo Masteraddress = ctxTFAT.MasterInfo.Where(x => x.Code == Model.Code.Trim()).FirstOrDefault();
            GstFlag = Masteraddress == null ? false : Masteraddress.GSTFlag;

            if (String.IsNullOrEmpty(Model.HSNCODE))
            {
                if (ctxTFAT.HSNMaster.Where(x => x.Code == Masteraddress.HSNCode).Select(x => x.Name).FirstOrDefault() != null)
                {
                    GSTCode = Masteraddress.HSNCode;
                    GSTName = ctxTFAT.HSNMaster.Where(x => x.Code == GSTCode).Select(x => x.Name).FirstOrDefault();

                    var EffectDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                    var pstate = ctxTFAT.Address.Where(x => x.Code == Model.Code.Trim()).Select(x => x.State).FirstOrDefault();
                    if (String.IsNullOrEmpty(pstate))
                    {
                        pstate = "19";
                    }
                    var branchstate = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.State).FirstOrDefault();
                    if (pstate == branchstate)
                    {
                        var result = ctxTFAT.HSNRates.Where(x => x.Code == GSTCode && x.EffDate <= EffectDate).OrderByDescending(x => x.EffDate).Select(x => new { IGSTRate = (decimal)0, SGSTRate = x.SGSTRate.Value, CGSTRate = x.CGSTRate.Value }).FirstOrDefault();
                        IGST = result == null ? 0 : result.IGSTRate;
                        SGST = result == null ? 0 : result.SGSTRate;
                        CGST = result == null ? 0 : result.CGSTRate;
                    }
                    else
                    {
                        var result = ctxTFAT.HSNRates.Where(x => x.Code == GSTCode && x.EffDate <= EffectDate).OrderByDescending(x => x.EffDate).Select(x => new { IGSTRate = x.IGSTRate.Value, SGSTRate = (decimal)0, CGSTRate = (decimal)0 }).FirstOrDefault();
                        IGST = result == null ? 0 : result.IGSTRate;
                        SGST = result == null ? 0 : result.SGSTRate;
                        CGST = result == null ? 0 : result.CGSTRate;
                    }
                }
                else
                {
                    GSTCode = Masteraddress.HSNCode;
                    GSTName = ctxTFAT.TaxMaster.Where(x => x.Code == GSTCode).Select(x => x.Name).FirstOrDefault();

                    var pstate = ctxTFAT.Address.Where(x => x.Code == Model.Code.Trim()).Select(x => x.State).FirstOrDefault();
                    if (String.IsNullOrEmpty(pstate))
                    {
                        pstate = "19";
                    }
                    var branchstate = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.State).FirstOrDefault();
                    if (pstate == branchstate)
                    {
                        var result = ctxTFAT.TaxMaster.Where(x => x.Code == GSTCode).Select(x => new { IGSTRate = (decimal)0, SGSTRate = x.SGSTRate.Value, CGSTRate = x.CGSTRate.Value }).FirstOrDefault();
                        IGST = result == null ? 0 : result.IGSTRate;
                        SGST = result == null ? 0 : result.SGSTRate;
                        CGST = result == null ? 0 : result.CGSTRate;
                    }
                    else
                    {
                        var result = ctxTFAT.TaxMaster.Where(x => x.Code == GSTCode).Select(x => new { IGSTRate = x.IGSTRate.Value, SGSTRate = (decimal)0, CGSTRate = (decimal)0 }).FirstOrDefault();
                        IGST = result == null ? 0 : result.IGSTRate;
                        SGST = result == null ? 0 : result.SGSTRate;
                        CGST = result == null ? 0 : result.CGSTRate;
                    }
                }
            }
            else
            {
                if (ctxTFAT.HSNMaster.Where(x => x.Code == Model.HSNCODE).Select(x => x.Name).FirstOrDefault() != null)
                {
                    GSTCode = Model.HSNCODE;
                    GSTName = ctxTFAT.HSNMaster.Where(x => x.Code == GSTCode).Select(x => x.Name).FirstOrDefault();

                    var EffectDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                    var pstate = ctxTFAT.Address.Where(x => x.Code == Model.Code.Trim()).Select(x => x.State).FirstOrDefault();
                    if (String.IsNullOrEmpty(pstate))
                    {
                        pstate = "19";
                    }
                    var branchstate = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.State).FirstOrDefault();
                    if (pstate == branchstate)
                    {
                        var result = ctxTFAT.HSNRates.Where(x => x.Code == GSTCode && x.EffDate <= EffectDate).OrderByDescending(x => x.EffDate).Select(x => new { IGSTRate = (decimal)0, SGSTRate = x.SGSTRate.Value, CGSTRate = x.CGSTRate.Value }).FirstOrDefault();
                        IGST = result == null ? 0 : result.IGSTRate;
                        SGST = result == null ? 0 : result.SGSTRate;
                        CGST = result == null ? 0 : result.CGSTRate;
                    }
                    else
                    {
                        var result = ctxTFAT.HSNRates.Where(x => x.Code == GSTCode && x.EffDate <= EffectDate).OrderByDescending(x => x.EffDate).Select(x => new { IGSTRate = x.IGSTRate.Value, SGSTRate = (decimal)0, CGSTRate = (decimal)0 }).FirstOrDefault();
                        IGST = result == null ? 0 : result.IGSTRate;
                        SGST = result == null ? 0 : result.SGSTRate;
                        CGST = result == null ? 0 : result.CGSTRate;
                    }
                }
                else
                {
                    GSTCode = Model.HSNCODE;
                    GSTName = ctxTFAT.TaxMaster.Where(x => x.Code == GSTCode).Select(x => x.Name).FirstOrDefault();

                    var pstate = ctxTFAT.Address.Where(x => x.Code == Model.Code.Trim()).Select(x => x.State).FirstOrDefault();
                    if (String.IsNullOrEmpty(pstate))
                    {
                        pstate = "19";
                    }
                    var branchstate = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.State).FirstOrDefault();
                    if (pstate == branchstate)
                    {
                        var result = ctxTFAT.TaxMaster.Where(x => x.Code == GSTCode).Select(x => new { IGSTRate = (decimal)0, SGSTRate = x.SGSTRate.Value, CGSTRate = x.CGSTRate.Value }).FirstOrDefault();
                        IGST = result == null ? 0 : result.IGSTRate;
                        SGST = result == null ? 0 : result.SGSTRate;
                        CGST = result == null ? 0 : result.CGSTRate;
                    }
                    else
                    {
                        var result = ctxTFAT.TaxMaster.Where(x => x.Code == GSTCode).Select(x => new { IGSTRate = x.IGSTRate.Value, SGSTRate = (decimal)0, CGSTRate = (decimal)0 }).FirstOrDefault();
                        IGST = result == null ? 0 : result.IGSTRate;
                        SGST = result == null ? 0 : result.SGSTRate;
                        CGST = result == null ? 0 : result.CGSTRate;
                    }
                }
            }
            #endregion
            return Json(new { ShowDriverOrVehicle = Model.ShowDriverOrVehicle, ShowDriverOrVehicleLabel = Model.ShowDriverOrVehicleLabel, AllowToChange = AllowToChange, GSTCode = GSTCode, GSTName = GSTName, GstFlag = GstFlag, IGST = IGST, SGST = SGST, CGST = CGST, Html = html, PartialDivName = PartialDivName, ReferAccReq = ReferAccReq, CostCenterTally = CostCenterTally }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetTruckTypeWiseViewList(CreditPurchaseVM Model)
        {
            Model.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            List<AddOns> truckaddonlist = new List<AddOns>();
            bool ExistAddonOrNot = false;

            var result = (List<CreditPurchaseVM>)Session["Lothertrxlist"];
            var result1 = new CreditPurchaseVM();

            Model.RelatedToN = ctxTFAT.Master.Where(x => x.Code == Model.RelatedTo).Select(x => x.Name).FirstOrDefault();
            {

                //Trip Details
                if (Model.Code == "000100341")
                {

                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Trip Start Date",
                        ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                        QueryText = "",
                        FldType = "D"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "Trip End Date",
                        ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                        QueryText = "",
                        FldType = "D"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F003",
                        Head = "Trip Days",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F004",
                        Head = "Starting kms",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F005",
                        Head = "Ending kms",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"

                    });

                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F006",
                        Head = "Charge KMS",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F007",
                        Head = "Per KM Rate",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F008",
                        Head = "Trip Charges",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"

                    });

                }

                //Tyre Details
                else if (Model.Code == "000100345")
                {
                    List<string> tyretypeList = new List<string>();
                    tyretypeList.Add("New");
                    tyretypeList.Add("Remould");
                    tyretypeList.Add("Scrap");
                    tyretypeList.Add("Sale");
                    tyretypeList.Add("OutOfStock");
                    tyretypeList.Add("Stock");

                    //var mTyreTypes = ctxTFAT.TyreMaster.Select(x => x.TyreType).Distinct().ToList();
                    var mTyreTypes = ctxTFAT.ItemMaster.Where(x => x.BaseGr == Model.ProductGroupType).Select(x => new { x.Name, x.Code }).Distinct().OrderBy(x => x.Name).ToList();
                    var mItemGroup = ctxTFAT.ItemGroups.Where(x => x.Hide == false).Select(x => new { x.Name, x.Code }).Distinct().ToList();
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F015",
                    //    Head = "Product Group",
                    //    ApplCode = Model.ProductGroupType,
                    //    QueryText = "Select^" + String.Join("^", mItemGroup.Select(x => x.Name).ToList()),
                    //    QueryCode = "Select^" + String.Join("^", mItemGroup.Select(x => x.Code).ToList()),
                    //    FldType = "C",
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F001",
                    //    Head = "Pick From",
                    //    ApplCode = Model.FromType,
                    //    QueryText = "Master^Direct",
                    //    QueryCode = "Master^Direct",
                    //    FldType = "C"

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F002",
                    //    Head = "Product",
                    //    ApplCode = "",
                    //    QueryText = "Select^" + String.Join("^", mTyreTypes.Select(x => x.Name).ToList()),
                    //    QueryCode = "Select^" + String.Join("^", mTyreTypes.Select(x => x.Code).ToList()),
                    //    FldType = Model.FromType == "Direct" ? "T" : "C",
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F003",
                    //    Head = "Cost",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N"

                    //});

                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Tyre Type",
                        ApplCode = "",
                        QueryText = "Select^" + String.Join("^", tyretypeList),
                        QueryCode = "Select^" + String.Join("^", tyretypeList),
                        FldType = "C",
                    });
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F004",
                    //    Head = "Warranty KM",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N"

                    //});

                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "Install For",
                        ApplCode = "",
                        QueryText = "Tyre^Stepnee",
                        FldType = "R"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F003",
                        Head = "Tyre Placed No",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F004",
                        Head = "Tyre SerialNo",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "T"

                    });
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F009",
                    //    Head = "Mfg Date",
                    //    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                    //    QueryText = "",
                    //    FldType = "D"

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F010",
                    //    Head = "Install / Received Date",
                    //    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                    //    QueryText = "",
                    //    FldType = "D"

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F011",
                    //    Head = "Install KM",
                    //    ApplCode = "0",
                    //    QueryText = "",
                    //    FldType = "N"

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F005",
                    //    Head = "Expiry Days",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N",
                    //    

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F012",
                    //    Head = "Expiry Date",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "D",
                    //    

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F013",
                    //    Head = "Expiry KM",
                    //    ApplCode = "0",
                    //    QueryText = "",
                    //    FldType = "N"
                    //});
                }

                //Diesel Details
                else if (Model.Code == "000100343")
                {
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Diesel Receipt No",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "T"
                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "Litres",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"
                    });
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F003",
                    //    Head = "KM",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N"
                    //});

                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F004",
                    //    Head = "Date",
                    //    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                    //    QueryText = "",
                    //    FldType = "D"
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F005",
                    //    Head = "Amount",
                    //    ApplCode = Model.Amt.ToString(),
                    //    QueryText = "",
                    //    FldType = "T"
                    //});
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F003",
                        Head = "Approved Diesel",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "T"
                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F004",
                        Head = "Approver Name",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "T"
                    });
                }

                //Loan(Details) || Permit(1 Year ) || Permit(5 Year ) || Fitness || PUC ||  PT || Green Tax || T P (Transit Pass)
                else if (Model.Code == "000100951" || Model.Code == "000100781" || Model.Code == "000100953" || Model.Code == "000100736" || Model.Code == "000100789" || Model.Code == "000100788" || Model.Code == "000100653" || Model.Code == "000100811" || Model.Code == "000101135")
                {
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "From Date",
                        ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                        QueryText = "",
                        FldType = "D"
                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "To Date",
                        ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                        QueryText = "",
                        FldType = "D"
                    });
                }

                //Insurance Details
                else if (Model.Code == "000100326")
                {
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "From Date",
                        ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                        QueryText = "",
                        FldType = "D"
                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "To Date",
                        ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                        QueryText = "",
                        FldType = "D"
                    });
                }

                //Accident Details
                if (Model.Code == "000100949")
                {
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Accident Date",
                        ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                        QueryText = "",
                        FldType = "D"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "Amount",
                        ApplCode = Model.Amt.ToString(),
                        QueryText = "",
                        FldType = "T"

                    });
                }

                // Spare Parts & Repairs Details
                else if (Model.Code == "000100346")
                {
                    var mSparePart = ctxTFAT.ItemMaster.Where(x => x.BaseGr == Model.ProductGroupType).Select(x => new { x.Name, x.Code }).Distinct().OrderBy(x => x.Name).ToList();
                    var mItemGroup = ctxTFAT.ItemGroups.Where(x => x.Hide == false).Select(x => new { x.Name, x.Code }).Distinct().ToList();
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F012",
                    //    Head = "Product Group",
                    //    ApplCode = Model.ProductGroupType,
                    //    QueryText = "Select^" + String.Join("^", mItemGroup.Select(x => x.Name).ToList()),
                    //    QueryCode = "Select^" + String.Join("^", mItemGroup.Select(x => x.Code).ToList()),
                    //    FldType = "C",
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F001",
                    //    Head = "Pick From",
                    //    ApplCode = Model.FromType,
                    //    QueryText = "Master^Direct",
                    //    QueryCode = "Master^Direct",
                    //    FldType = "C"
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F002",
                    //    Head = "Spare Part",
                    //    ApplCode = "",
                    //    QueryText = "Select^" + String.Join("^", mSparePart.Select(x => x.Name).ToList()),
                    //    QueryCode = "Select^" + String.Join("^", mSparePart.Select(x => x.Code).ToList()),
                    //    FldType = Model.FromType == "Direct" ? "T" : "C",
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F003",
                    //    Head = "Cost",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    QueryCode = "",
                    //    FldType = "N",
                    //    InActive = true
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F013",
                    //    Head = "Qty",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    QueryCode = "",
                    //    FldType = "N",
                    //    InActive = true
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F004",
                    //    Head = "Warranty KM",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N"
                    //});

                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Receipt No",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "T"
                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "Receipt Dt",
                        ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                        QueryText = "",
                        FldType = "D"
                    });
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F008",
                    //    Head = "Install / Received Date",
                    //    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                    //    QueryText = "",
                    //    FldType = "D"
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F009",
                    //    Head = "Current KM",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N"
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F005",
                    //    Head = "Warranty Days",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N",
                    //    
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F010",
                    //    Head = "Due Date",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "D",
                    //    
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F011",
                    //    Head = "Due km",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N"
                    //});
                }

                //Body Building Details
                else if (Model.Code == "000100950")
                {
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Date",
                        ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                        QueryText = "",
                        FldType = "D"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "Amount",
                        ApplCode = Model.Amt.ToString(),
                        QueryText = "",
                        FldType = "T"

                    });


                }

                //Others || Miscellaneous Charges
                else if (Model.Code == "000100404" || Model.Code == "000100952")
                {
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Amount",
                        ApplCode = Model.Amt.ToString(),
                        QueryText = "",
                        FldType = "T"

                    });

                }

                //Principal Amount
                else if (Model.Code == "000100954")
                {
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Date",
                        ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                        QueryText = "",
                        FldType = "D"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "Amount",
                        ApplCode = Model.Amt.ToString(),
                        QueryText = "",
                        FldType = "T"

                    });


                }
            }

            var html = ViewHelper.RenderPartialView(this, "TruckDetails", new CreditPurchaseVM() { /*AccReqRelated = Model.AccReqRelated,*/ SaveCostCenter = true, AddOnList = truckaddonlist, RelatedTo = Model.RelatedTo, RelatedChoice = Model.RelatedChoice, PartialDivName = Model.PartialDivName, Code = Model.Code });
            return Json(new { Html = html, RelatedToN = Model.RelatedToN, RelatedTo = Model.RelatedTo }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult GetTruckTypeWiseViewListE(CreditPurchaseVM Model)
        {
            Model.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            List<AddOns> truckaddonlist = new List<AddOns>();
            bool ExistAddonOrNot = false;
            var result = (List<CreditPurchaseVM>)Session["Lothertrxlist"];
            var result1 = new CreditPurchaseVM();
            var Acc = ctxTFAT.Master.Where(x => x.Code == Model.Code).FirstOrDefault();
            if (Acc != null)
            {
                Model.RelatedTo = Acc.RelatedTo;
                Model.RelatedToN = ctxTFAT.Master.Where(x => x.Code == Model.RelatedTo).Select(x => x.Name).FirstOrDefault();
            }

            //Trip Details
            if (Model.RelatedTo == "000100341")
            {

                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "Trip Start Date",
                    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                    QueryText = "",
                    FldType = "D"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F002",
                    Head = "Trip End Date",
                    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                    QueryText = "",
                    FldType = "D"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F003",
                    Head = "Trip Days",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "N"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F004",
                    Head = "Starting kms",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "N"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F005",
                    Head = "Ending kms",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "N"

                });

                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F006",
                    Head = "Charge KMS",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "N"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F007",
                    Head = "Per KM Rate",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "N"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F008",
                    Head = "Trip Charges",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "N"
                });

            }

            //Tyre Details
            else if (Model.RelatedTo == "000100345")
            {
                List<string> tyretypeList = new List<string>();
                tyretypeList.Add("New");
                tyretypeList.Add("Remould");
                tyretypeList.Add("Scrap");
                tyretypeList.Add("Sale");
                tyretypeList.Add("OutOfStock");
                tyretypeList.Add("Stock");

                var mTyreTypes = ctxTFAT.ItemMaster.Where(x => x.BaseGr == Model.ProductGroupType).Select(x => new { x.Name, x.Code }).Distinct().OrderBy(x => x.Name).ToList();
                var mItemGroup = ctxTFAT.ItemGroups.Where(x => x.Hide == false).Select(x => new { x.Name, x.Code }).Distinct().ToList();
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F015",
                //    Head = "Product Group",
                //    ApplCode = Model.ProductGroupType,
                //    QueryText = "Select^" + String.Join("^", mItemGroup.Select(x => x.Name).ToList()),
                //    QueryCode = "Select^" + String.Join("^", mItemGroup.Select(x => x.Code).ToList()),
                //    FldType = "C",
                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F001",
                //    Head = "Pick From",
                //    ApplCode = Model.FromType,
                //    QueryText = "Master^Direct",
                //    QueryCode = "Master^Direct",
                //    FldType = "C"

                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F002",
                //    Head = "Product",
                //    ApplCode = "",
                //    QueryText = "Select^" + String.Join("^", mTyreTypes.Select(x => x.Name).ToList()),
                //    QueryCode = "Select^" + String.Join("^", mTyreTypes.Select(x => x.Code).ToList()),
                //    FldType = Model.FromType == "Direct" ? "T" : "C",
                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F003",
                //    Head = "Cost",
                //    ApplCode = "",
                //    QueryText = "",
                //    FldType = "N"

                //});

                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "Tyre Type",
                    ApplCode = "",
                    QueryText = "Select^" + String.Join("^", tyretypeList),
                    QueryCode = "Select^" + String.Join("^", tyretypeList),
                    FldType = "C",
                });
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F004",
                //    Head = "Warranty KM",
                //    ApplCode = "",
                //    QueryText = "",
                //    FldType = "N"

                //});

                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F002",
                    Head = "Install For",
                    ApplCode = "",
                    QueryText = "Tyre^Stepnee",
                    FldType = "R"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F003",
                    Head = "Tyre Placed No",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "N"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F004",
                    Head = "Tyre SerialNo",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"

                });
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F009",
                //    Head = "Mfg Date",
                //    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                //    QueryText = "",
                //    FldType = "D"

                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F010",
                //    Head = "Install / Received Date",
                //    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                //    QueryText = "",
                //    FldType = "D"

                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F011",
                //    Head = "Install KM",
                //    ApplCode = "0",
                //    QueryText = "",
                //    FldType = "N"

                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F005",
                //    Head = "Expiry Days",
                //    ApplCode = "",
                //    QueryText = "",
                //    FldType = "N",
                //    

                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F012",
                //    Head = "Expiry Date",
                //    ApplCode = "",
                //    QueryText = "",
                //    FldType = "D"

                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F013",
                //    Head = "Expiry KM",
                //    ApplCode = "0",
                //    QueryText = "",
                //    FldType = "N"
                //});
            }

            //Diesel Details
            else if (Model.RelatedTo == "000100343")
            {
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "Diesel Receipt No",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F002",
                    Head = "Litres",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "N"
                });
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F003",
                //    Head = "KM",
                //    ApplCode = "",
                //    QueryText = "",
                //    FldType = "N"
                //});

                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F004",
                //    Head = "Date",
                //    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                //    QueryText = "",
                //    FldType = "D"
                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F005",
                //    Head = "Amount",
                //    ApplCode = Model.Amt.ToString(),
                //    QueryText = "",
                //    FldType = "T"
                //});
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F003",
                    Head = "Approved Diesel",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F004",
                    Head = "Approver Name",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"
                });
            }

            //Loan(Details) || Permit(1 Year ) || Permit(5 Year ) || Fitness || PUC ||  PT || Green Tax || T P (Transit Pass)
            else if (Model.RelatedTo == "000100951" || Model.RelatedTo == "000100781" || Model.RelatedTo == "000100953" || Model.RelatedTo == "000100736" || Model.RelatedTo == "000100789" || Model.RelatedTo == "000100788" || Model.RelatedTo == "000100653" || Model.RelatedTo == "000100811" || Model.RelatedTo == "000101135")
            {
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "From Date",
                    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                    QueryText = "",
                    FldType = "D"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F002",
                    Head = "To Date",
                    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                    QueryText = "",
                    FldType = "D"
                });
            }

            //Insurance Details
            else if (Model.RelatedTo == "000100326")
            {
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "From Date",
                    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                    QueryText = "",
                    FldType = "D"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F002",
                    Head = "To Date",
                    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                    QueryText = "",
                    FldType = "D"
                });
            }

            //Accident Details
            if (Model.RelatedTo == "000100949")
            {
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "Accident Date",
                    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                    QueryText = "",
                    FldType = "D"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F002",
                    Head = "Amount",
                    ApplCode = Model.Amt.ToString(),
                    QueryText = "",
                    FldType = "T"

                });
            }

            // Spare Parts & Repairs Details
            else if (Model.RelatedTo == "000100346")
            {
                var mSparePart = ctxTFAT.ItemMaster.Where(x => x.BaseGr == Model.ProductGroupType).Select(x => new { x.Name, x.Code }).Distinct().OrderBy(x => x.Name).ToList();
                var mItemGroup = ctxTFAT.ItemGroups.Where(x => x.Hide == false).Select(x => new { x.Name, x.Code }).Distinct().ToList();

                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F012",
                //    Head = "Product Group",
                //    ApplCode = Model.ProductGroupType,
                //    QueryText = "Select^" + String.Join("^", mItemGroup.Select(x => x.Name).ToList()),
                //    QueryCode = "Select^" + String.Join("^", mItemGroup.Select(x => x.Code).ToList()),
                //    FldType = "C",
                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F001",
                //    Head = "Pick From",
                //    ApplCode = Model.FromType,
                //    QueryText = "Master^Direct",
                //    QueryCode = "Master^Direct",
                //    FldType = "C"
                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F002",
                //    Head = "Spare Part",
                //    ApplCode = "",
                //    QueryText = "Select^" + String.Join("^", mSparePart.Select(x => x.Name).ToList()),
                //    QueryCode = "Select^" + String.Join("^", mSparePart.Select(x => x.Code).ToList()),
                //    FldType = Model.FromType == "Direct" ? "T" : "C",
                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F003",
                //    Head = "Cost",
                //    ApplCode = "",
                //    QueryText = "",
                //    FldType = "N",
                //    InActive = true
                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F013",
                //    Head = "Qty",
                //    ApplCode = "",
                //    QueryText = "",
                //    QueryCode = "",
                //    FldType = "N",
                //    InActive = true
                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F004",
                //    Head = "Warranty KM",
                //    ApplCode = "",
                //    QueryText = "",
                //    FldType = "N"
                //});

                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "Receipt No",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"
                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F002",
                    Head = "Receipt Dt",
                    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                    QueryText = "",
                    FldType = "D"
                });
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F008",
                //    Head = "Install / Received Date",
                //    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                //    QueryText = "",
                //    FldType = "D"
                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F009",
                //    Head = "Current KM",
                //    ApplCode = "",
                //    QueryText = "",
                //    FldType = "N"
                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F005",
                //    Head = "Warranty Days",
                //    ApplCode = "",
                //    QueryText = "",
                //    FldType = "N",
                //    
                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F010",
                //    Head = "Due Date",
                //    ApplCode = "",
                //    QueryText = "",
                //    FldType = "D",
                //    
                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F011",
                //    Head = "Due km",
                //    ApplCode = "",
                //    QueryText = "",
                //    FldType = "N"
                //});
            }

            //Body Building Details
            else if (Model.RelatedTo == "000100950")
            {
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "Date",
                    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                    QueryText = "",
                    FldType = "D"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F002",
                    Head = "Amount",
                    ApplCode = Model.Amt.ToString(),
                    QueryText = "",
                    FldType = "T"

                });


            }

            //Others || Miscellaneous Charges
            else if (Model.RelatedTo == "000100404" || Model.RelatedTo == "000100952")
            {
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "Amount",
                    ApplCode = Model.Amt.ToString(),
                    QueryText = "",
                    FldType = "T"

                });

            }

            //Principal Amount
            else if (Model.RelatedTo == "000100954")
            {
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "Date",
                    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                    QueryText = "",
                    FldType = "D"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F002",
                    Head = "Amount",
                    ApplCode = Model.Amt.ToString(),
                    QueryText = "",
                    FldType = "T"

                });


            }




            var html = ViewHelper.RenderPartialView(this, "TruckDetails", new CreditPurchaseVM() { /*AccReqRelated = Model.AccReqRelated,*/ SaveCostCenter = true, AddOnList = truckaddonlist, RelatedTo = Model.RelatedTo, RelatedChoice = Model.RelatedChoice, PartialDivName = Model.PartialDivName, Code = Model.Code });
            return Json(new { Html = html, RelatedToN = Model.RelatedToN, RelatedTo = Model.RelatedTo }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetTruckTypeWiseViewListRelatedToE(CreditPurchaseVM Model)
        {
            Model.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            List<AddOns> truckaddonlist = new List<AddOns>();
            bool ExistAddonOrNot = false;


            var result = (List<CreditPurchaseVM>)Session["Lothertrxlist"];
            var result1 = new CreditPurchaseVM();

            //if (result != null)
            //{
            //    result1 = result.Where(x => x.Code == Model.Code && (x.RelatedChoice == Model.RelatedChoice || x.RelatedTo == Model.RelatedTo)).FirstOrDefault();
            //    if (result1 != null)
            //    {
            //        if (result1.AddOnList != null)
            //        {
            //            if (result1.AddOnList.Count() != 0)
            //            {
            //                ExistAddonOrNot = true;
            //            }
            //        }

            //    }
            //}

            Model.RelatedToN = ctxTFAT.Master.Where(x => x.Code == Model.RelatedTo).Select(x => x.Name).FirstOrDefault();



            //if (ExistAddonOrNot)
            //{
            //    truckaddonlist = result1.AddOnList;
            //}
            //else
            {

                //Trip Details
                if (Model.RelatedTo == "000100341")
                {

                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Trip Start Date",
                        ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                        QueryText = "",
                        FldType = "D"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "Trip End Date",
                        ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                        QueryText = "",
                        FldType = "D"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F003",
                        Head = "Trip Days",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F004",
                        Head = "Starting kms",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F005",
                        Head = "Ending kms",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"

                    });

                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F006",
                        Head = "Charge KMS",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F007",
                        Head = "Per KM Rate",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F008",
                        Head = "Trip Charges",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"

                    });

                }

                //Tyre Details
                else if (Model.RelatedTo == "000100345")
                {
                    List<string> tyretypeList = new List<string>();
                    tyretypeList.Add("New");
                    tyretypeList.Add("Remould");
                    tyretypeList.Add("Scrap");
                    tyretypeList.Add("Sale");
                    tyretypeList.Add("OutOfStock");
                    tyretypeList.Add("Stock");

                    //var mTyreTypes = ctxTFAT.TyreMaster.Select(x => x.TyreType).Distinct().ToList();
                    var mTyreTypes = ctxTFAT.ItemMaster.Where(x => x.BaseGr == Model.ProductGroupType).Select(x => new { x.Name, x.Code }).Distinct().OrderBy(x => x.Name).ToList();
                    var mItemGroup = ctxTFAT.ItemGroups.Where(x => x.Hide == false).Select(x => new { x.Name, x.Code }).Distinct().ToList();
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F015",
                    //    Head = "Product Group",
                    //    ApplCode = Model.ProductGroupType,
                    //    QueryText = "Select^" + String.Join("^", mItemGroup.Select(x => x.Name).ToList()),
                    //    QueryCode = "Select^" + String.Join("^", mItemGroup.Select(x => x.Code).ToList()),
                    //    FldType = "C",
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F001",
                    //    Head = "Pick From",
                    //    ApplCode = Model.FromType,
                    //    QueryText = "Master^Direct",
                    //    QueryCode = "Master^Direct",
                    //    FldType = "C"

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F002",
                    //    Head = "Product",
                    //    ApplCode = "",
                    //    QueryText = "Select^" + String.Join("^", mTyreTypes.Select(x => x.Name).ToList()),
                    //    QueryCode = "Select^" + String.Join("^", mTyreTypes.Select(x => x.Code).ToList()),
                    //    FldType = Model.FromType == "Direct" ? "T" : "C",
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F003",
                    //    Head = "Cost",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N"

                    //});

                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Tyre Type",
                        ApplCode = "",
                        QueryText = "Select^" + String.Join("^", tyretypeList),
                        QueryCode = "Select^" + String.Join("^", tyretypeList),
                        FldType = "C",
                    });
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F004",
                    //    Head = "Warranty KM",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N"

                    //});

                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "Install For",
                        ApplCode = "",
                        QueryText = "Tyre^Stepnee",
                        FldType = "R"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F003",
                        Head = "Tyre Placed No",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F004",
                        Head = "Tyre SerialNo",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "T"

                    });
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F009",
                    //    Head = "Mfg Date",
                    //    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                    //    QueryText = "",
                    //    FldType = "D"

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F010",
                    //    Head = "Install / Received Date",
                    //    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                    //    QueryText = "",
                    //    FldType = "D"

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F011",
                    //    Head = "Install KM",
                    //    ApplCode = "0",
                    //    QueryText = "",
                    //    FldType = "N"

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F005",
                    //    Head = "Expiry Days",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N",
                    //    

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F012",
                    //    Head = "Expiry Date",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "D",
                    //    

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F013",
                    //    Head = "Expiry KM",
                    //    ApplCode = "0",
                    //    QueryText = "",
                    //    FldType = "N"
                    //});
                }

                //Diesel Details
                else if (Model.RelatedTo == "000100343")
                {
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Diesel Receipt No",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "T"
                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "Litres",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"
                    });
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F003",
                    //    Head = "KM",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N"
                    //});

                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F004",
                    //    Head = "Date",
                    //    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                    //    QueryText = "",
                    //    FldType = "D"
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F005",
                    //    Head = "Amount",
                    //    ApplCode = Model.Amt.ToString(),
                    //    QueryText = "",
                    //    FldType = "T"
                    //});
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F003",
                        Head = "Approved Diesel",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "T"
                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F004",
                        Head = "Approver Name",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "T"
                    });
                }

                //Loan(Details) || Permit(1 Year ) || Permit(5 Year ) || Fitness || PUC ||  PT || Green Tax || T P (Transit Pass)
                else if (Model.RelatedTo == "000100951" || Model.RelatedTo == "000100781" || Model.RelatedTo == "000100953" || Model.RelatedTo == "000100736" || Model.RelatedTo == "000100789" || Model.RelatedTo == "000100788" || Model.RelatedTo == "000100653" || Model.RelatedTo == "000100811" || Model.RelatedTo == "000101135")
                {
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "From Date",
                        ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                        QueryText = "",
                        FldType = "D"
                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "To Date",
                        ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                        QueryText = "",
                        FldType = "D"
                    });
                }

                //Insurance Details
                else if (Model.RelatedTo == "000100326")
                {
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "From Date",
                        ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                        QueryText = "",
                        FldType = "D"
                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "To Date",
                        ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                        QueryText = "",
                        FldType = "D"
                    });
                }

                //Accident Details
                if (Model.RelatedTo == "000100949")
                {
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Accident Date",
                        ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                        QueryText = "",
                        FldType = "D"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "Amount",
                        ApplCode = Model.Amt.ToString(),
                        QueryText = "",
                        FldType = "T"

                    });
                }

                // Spare Parts & Repairs Details
                else if (Model.RelatedTo == "000100346")
                {
                    var mSparePart = ctxTFAT.ItemMaster.Where(x => x.BaseGr == Model.ProductGroupType).Select(x => new { x.Name, x.Code }).Distinct().OrderBy(x => x.Name).ToList();
                    var mItemGroup = ctxTFAT.ItemGroups.Where(x => x.Hide == false).Select(x => new { x.Name, x.Code }).Distinct().ToList();

                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F012",
                    //    Head = "Product Group",
                    //    ApplCode = Model.ProductGroupType,
                    //    QueryText = "Select^" + String.Join("^", mItemGroup.Select(x => x.Name).ToList()),
                    //    QueryCode = "Select^" + String.Join("^", mItemGroup.Select(x => x.Code).ToList()),
                    //    FldType = "C",
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F001",
                    //    Head = "Pick From",
                    //    ApplCode = Model.FromType,
                    //    QueryText = "Master^Direct",
                    //    QueryCode = "Master^Direct",
                    //    FldType = "C"
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F002",
                    //    Head = "Spare Part",
                    //    ApplCode = "",
                    //    QueryText = "Select^" + String.Join("^", mSparePart.Select(x => x.Name).ToList()),
                    //    QueryCode = "Select^" + String.Join("^", mSparePart.Select(x => x.Code).ToList()),
                    //    FldType = Model.FromType == "Direct" ? "T" : "C",
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F003",
                    //    Head = "Cost",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N",
                    //    InActive = true
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F013",
                    //    Head = "Qty",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    QueryCode = "",
                    //    FldType = "N",
                    //    InActive = true
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F004",
                    //    Head = "Warranty KM",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N"
                    //});

                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Receipt No",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "T"
                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "Receipt Dt",
                        ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                        QueryText = "",
                        FldType = "D"
                    });
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F008",
                    //    Head = "Install / Received Date",
                    //    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                    //    QueryText = "",
                    //    FldType = "D"
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F009",
                    //    Head = "Current KM",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N"
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F005",
                    //    Head = "Warranty Days",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N",
                    //    
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F010",
                    //    Head = "Due Date",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "D",
                    //    
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F011",
                    //    Head = "Due km",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N"
                    //});
                }

                //Body Building Details
                else if (Model.RelatedTo == "000100950")
                {
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Date",
                        ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                        QueryText = "",
                        FldType = "D"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "Amount",
                        ApplCode = Model.Amt.ToString(),
                        QueryText = "",
                        FldType = "T"

                    });


                }

                //Others || Miscellaneous Charges
                else if (Model.RelatedTo == "000100404" || Model.RelatedTo == "000100952")
                {
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Amount",
                        ApplCode = Model.Amt.ToString(),
                        QueryText = "",
                        FldType = "T"

                    });

                }

                //Principal Amount
                else if (Model.RelatedTo == "000100954")
                {
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Date",
                        ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                        QueryText = "",
                        FldType = "D"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "Amount",
                        ApplCode = Model.Amt.ToString(),
                        QueryText = "",
                        FldType = "T"

                    });


                }
            }



            var html = ViewHelper.RenderPartialView(this, "TruckDetails", new CreditPurchaseVM() { /*AccReqRelated = Model.AccReqRelated,*/ SaveCostCenter = true, AddOnList = truckaddonlist, RelatedTo = Model.RelatedTo, RelatedChoice = Model.RelatedChoice, PartialDivName = Model.PartialDivName });
            return Json(new { Html = html, RelatedToN = Model.RelatedToN, RelatedTo = Model.RelatedTo }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetTruckTyreSingleViewList(CreditPurchaseVM Model)
        {
            Model.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            List<AddOns> truckaddonlist = new List<AddOns>();

            //Tyre Details
            if (Model.Code == "000100345")
            {
                List<string> tyretypeList = new List<string>();
                tyretypeList.Add("New");
                tyretypeList.Add("Remould");
                tyretypeList.Add("Scrap");
                tyretypeList.Add("Sale");
                tyretypeList.Add("OutOfStock");
                tyretypeList.Add("Stock");

                //var mTyreTypes = ctxTFAT.TyreMaster.Select(x => x.TyreType).Distinct().ToList();
                var mTyreTypes = ctxTFAT.ItemMaster.Where(x => x.BaseGr == Model.ProductGroupType).Select(x => new { x.Name, x.Code }).Distinct().OrderBy(x => x.Name).ToList();
                var mItemGroup = ctxTFAT.ItemGroups.Where(x => x.Hide == false).Select(x => new { x.Name, x.Code }).Distinct().ToList();
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F015",
                //    Head = "Product Group",
                //    ApplCode = Model.ProductGroupType,
                //    QueryText = "Select^" + String.Join("^", mItemGroup.Select(x => x.Name).ToList()),
                //    QueryCode = "Select^" + String.Join("^", mItemGroup.Select(x => x.Code).ToList()),
                //    FldType = "C",
                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F001",
                //    Head = "Pick From",
                //    ApplCode = Model.FromType,
                //    QueryText = "Master^Direct",
                //    QueryCode = "Master^Direct",
                //    FldType = "C"

                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F002",
                //    Head = "Product",
                //    ApplCode = "",
                //    QueryText = "Select^" + String.Join("^", mTyreTypes.Select(x => x.Name).ToList()),
                //    QueryCode = "Select^" + String.Join("^", mTyreTypes.Select(x => x.Code).ToList()),
                //    FldType = Model.FromType == "Direct" ? "T" : "C",
                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F003",
                //    Head = "Cost",
                //    ApplCode = "",
                //    QueryText = "",
                //    FldType = "T"

                //});
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "Tyre Type",
                    ApplCode = "",
                    QueryText = "Select^" + String.Join("^", tyretypeList),
                    QueryCode = "Select^" + String.Join("^", tyretypeList),
                    FldType = "C",
                });
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F004",
                //    Head = "KM",
                //    ApplCode = "",
                //    QueryText = "",
                //    FldType = "T"

                //});

                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F002",
                    Head = "Install For",
                    ApplCode = "",
                    QueryText = "Tyre^Stepnee",
                    FldType = "R"

                });


                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F003",
                    Head = "Tyre Placed No",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F004",
                    Head = "Tyre SerialNo",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"

                });



                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F009",
                //    Head = "Mfg Date",
                //    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                //    QueryText = "",
                //    FldType = "D"

                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F010",
                //    Head = "Install / Received Date",
                //    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                //    QueryText = "",
                //    FldType = "D"

                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F011",
                //    Head = "Install KM",
                //    ApplCode = "0",
                //    QueryText = "",
                //    FldType = "N"

                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F005",
                //    Head = "Expiry Days",
                //    ApplCode = "",
                //    QueryText = "",
                //    FldType = "T",
                //    

                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F012",
                //    Head = "Expiry Date",
                //    ApplCode = "",
                //    QueryText = "",
                //    FldType = "D",
                //    

                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F013",
                //    Head = "Expiry KM",
                //    ApplCode = "0",
                //    QueryText = "",
                //    FldType = "N"

                //});

            }

            // Spare Parts & Repairs Details
            else if (Model.Code == "000100346")
            {
                var mSparePart = ctxTFAT.ItemMaster.Where(x => x.BaseGr == Model.ProductGroupType).Select(x => new { x.Name, x.Code }).Distinct().OrderBy(x => x.Name).ToList();
                var mItemGroup = ctxTFAT.ItemGroups.Where(x => x.Hide == false).Select(x => new { x.Name, x.Code }).Distinct().ToList();
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F012",
                //    Head = "Product Group",
                //    ApplCode = Model.ProductGroupType,
                //    QueryText = "Select^" + String.Join("^", mItemGroup.Select(x => x.Name).ToList()),
                //    QueryCode = "Select^" + String.Join("^", mItemGroup.Select(x => x.Code).ToList()),
                //    FldType = "C",
                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F001",
                //    Head = "Pick From",
                //    ApplCode = Model.FromType,
                //    QueryText = "Master^Direct",
                //    QueryCode = "Master^Direct",
                //    FldType = "C"

                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F002",
                //    Head = "Spare Part",
                //    ApplCode = "",
                //    QueryText = "Select^" + String.Join("^", mSparePart.Select(x => x.Name).ToList()),
                //    QueryCode = "Select^" + String.Join("^", mSparePart.Select(x => x.Code).ToList()),
                //    FldType = Model.FromType == "Direct" ? "T" : "C",

                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F003",
                //    Head = "Cost",
                //    ApplCode = "",
                //    QueryText = "",
                //    FldType = "N",
                //    InActive = true

                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F013",
                //    Head = "Qty",
                //    ApplCode = "",
                //    QueryText = "",
                //    QueryCode = "",
                //    FldType = "N",
                //    InActive = true
                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F004",
                //    Head = "KM",
                //    ApplCode = "",
                //    QueryText = "",
                //    FldType = "T"

                //});

                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F001",
                    Head = "Receipt No",
                    ApplCode = "",
                    QueryText = "",
                    FldType = "T"

                });
                truckaddonlist.Add(new AddOns()
                {
                    Fld = "F002",
                    Head = "Receipt Dt",
                    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                    QueryText = "",
                    FldType = "D"

                });
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F008",
                //    Head = "Install / Received Date",
                //    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                //    QueryText = "",
                //    FldType = "D"

                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F009",
                //    Head = "Current KM",
                //    ApplCode = "",
                //    QueryText = "",
                //    FldType = "T"

                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F005",
                //    Head = "Warranty Days",
                //    ApplCode = "",
                //    QueryText = "",
                //    FldType = "T",
                //    

                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F010",
                //    Head = "Due Date",
                //    ApplCode = "",
                //    QueryText = "",
                //    FldType = "D",
                //    

                //});
                //truckaddonlist.Add(new AddOns()
                //{
                //    Fld = "F011",
                //    Head = "Due km",
                //    ApplCode = "",
                //    QueryText = "",
                //    FldType = "T"

                //});
            }

            var html = ViewHelper.RenderPartialView(this, "TruckTableView", new CreditPurchaseVM() { AddOnList = truckaddonlist, RelatedTo = Model.RelatedTo, SaveCostCenter = true, PartialDivName = Model.PartialDivName });
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetItemSingleViewList(CreditPurchaseVM Model)
        {
            Model.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
            List<AddOns> truckaddonlist = new List<AddOns>();
            // Item Details
            var mItem = ctxTFAT.ItemMaster.Where(x => x.BaseGr == Model.ProductGroupType).Select(x => new { x.Name, x.Code }).Distinct().OrderBy(x => x.Name).ToList();
            var mItemGroup = ctxTFAT.ItemGroups.OrderBy(x => x.Name).Where(x => x.Hide == false).Select(x => new { x.Name, x.Code }).Distinct().ToList();
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F001",
                Head = "Product Group",
                ApplCode = Model.ProductGroupType,
                QueryText = "Select^" + String.Join("^", mItemGroup.Select(x => x.Name).ToList()),
                QueryCode = "Select^" + String.Join("^", mItemGroup.Select(x => x.Code).ToList()),
                FldType = "C",
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F002",
                Head = "Pick From",
                ApplCode = Model.FromType,
                QueryText = "Master^Direct",
                QueryCode = "Master^Direct",
                FldType = "C",
                Hide = true
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F003",
                Head = "Item",
                ApplCode = "",
                QueryText = "Select^" + String.Join("^", mItem.Select(x => x.Name).ToList()),
                QueryCode = "Select^" + String.Join("^", mItem.Select(x => x.Code).ToList()),
                FldType = Model.FromType == "Direct" ? "T" : "C",
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F004",
                Head = "Cost",
                ApplCode = "0",
                QueryText = "",
                QueryCode = "",
                FldType = "N",
                InActive = true
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F005",
                Head = "Qty",
                ApplCode = "1",
                QueryText = "",
                QueryCode = "",
                FldType = "N",
                InActive = true
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F006",
                Head = "Total Amount",
                ApplCode = "0",
                QueryText = "",
                QueryCode = "",
                FldType = "N",
                InActive = true
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F007",
                Head = "Warranty KM",
                ApplCode = "0",
                QueryText = "",
                FldType = "N"
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F008",
                Head = "Install KM",
                ApplCode = "0",
                QueryText = "",
                FldType = "N",

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F009",
                Head = "Due km",
                ApplCode = "0",
                QueryText = "",
                FldType = "N"
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F010",
                Head = "Warranty Days",
                ApplCode = "0",
                QueryText = "",
                FldType = "N",

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F011",
                Head = "MFG Date",
                ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                QueryText = "",
                FldType = "D",
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F012",
                Head = "Install / Received Date",
                ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                QueryText = "",
                FldType = "D"
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F013",
                Head = "Due Date",
                ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                QueryText = "",
                FldType = "D",

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F014",
                Head = "HSN CODE",
                ApplCode = "",
                QueryText = "",
                FldType = "X",
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F015",
                Head = "Description",
                ApplCode = "",
                QueryText = "",
                FldType = "M",
            });
            var html = ViewHelper.RenderPartialView(this, "ItemDetails", new CreditPurchaseVM() { ItemList = truckaddonlist });
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetBTNDetail(CreditPurchaseVM Model)
        {
            var GetMaxAmt = ctxTFAT.RelateData.Where(x => x.Code == Model.Code && x.Char1 == Model.BTNNOCombo && x.AmtType == true).Sum(x => x.Amount ?? 0);
            decimal GetReceAmt = 0;
            if (ctxTFAT.RelateData.Where(x => x.Code == Model.Code && x.Char1 == Model.BTNNOCombo && x.AmtType == false).ToList().Count() > 0)
            {
                GetReceAmt = ctxTFAT.RelateData.Where(x => x.Code == Model.Code && x.Char1 == Model.BTNNOCombo && x.AmtType == false).Sum(x => x.Amount ?? 0);
            }

            if (GetMaxAmt > GetReceAmt)
            {
                GetReceAmt = GetMaxAmt - GetReceAmt;
            }
            var result = (List<CreditPurchaseVM>)Session["Lothertrxlist"];
            if (result != null)
            {
                var GetList = result.Where(x => x.Code == Model.Code && x.AmtType == "Receipt" && x.BTNNOCombo == Model.BTNNOCombo).Sum(x => x.Amt);
                GetReceAmt = GetReceAmt - GetList;
            }
            return Json(new
            {

                Max = GetMaxAmt,
                Min = GetReceAmt,
            }, JsonRequestBehavior.AllowGet);
        }


        public List<AddOns> GetSessionEditTruckWiseData(CreditPurchaseVM Model)
        {

            List<AddOns> truckaddonlist = new List<AddOns>();
            //if (Model.AddOnList != null)
            {


                //Accident Details
                if (Model.RelatedTo == "000100949" || Model.Code == "000100949")
                {
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Accident Date",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "D"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "Amount",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "T"

                    });
                }

                //Body Building Details
                else if (Model.RelatedTo == "000100950" || Model.Code == "000100950")
                {
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Date",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "D"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "Amount",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "T"

                    });
                }

                //Others || Miscellaneous Charges
                else if (Model.RelatedTo == "000100404" || Model.Code == "000100404" || Model.RelatedTo == "000100952" || Model.Code == "000100952")
                {
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Amount",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "T"

                    });

                }

                //Principal Amount
                else if (Model.RelatedTo == "000100954" || Model.Code == "000100954")
                {
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Date",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "D"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "Amount",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "T"

                    });
                }
                //Diesel Details
                else if (Model.RelatedTo == "000100343" || Model.Code == "000100343")
                {
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Diesel Receipt No",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "T"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "Litres",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"

                    });
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F003",
                    //    Head = "KM",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N"

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F004",
                    //    Head = "Date",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "D"

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F005",
                    //    Head = "Amount",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "T"
                    //});
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F003",
                        Head = "Approved Diesel",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "T"
                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F004",
                        Head = "Approver Name",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "T"
                    });

                }

                //Loan(Details) || Permit(1 Year ) || Permit(5 Year ) || Fitness || PUC ||  PT || Green Tax || T P (Transit Pass)
                else if (Model.RelatedTo == "000100951" || Model.Code == "000100951" || Model.RelatedTo == "000100781" || Model.Code == "000100781" || Model.RelatedTo == "000100953" || Model.Code == "000100953" || Model.RelatedTo == "000100736" || Model.Code == "000100736" || Model.RelatedTo == "000100789" || Model.Code == "000100789" || Model.RelatedTo == "000100788" || Model.Code == "000100788" || Model.RelatedTo == "000100653" || Model.Code == "000100653" || Model.RelatedTo == "000100811" || Model.Code == "000100811" || Model.RelatedTo == "000101135" || Model.Code == "000101135")
                {
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "From Date",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "D"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "To Date",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "D"

                    });

                }

                //Insurance Details
                else if (Model.RelatedTo == "000100326" || Model.Code == "000100326")
                {
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F001",
                    //    Head = "Insurance Company",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "D"

                    //});
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "From Date",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "D"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "To Date",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "D"

                    });


                }

                // Spare Parts & Repairs Details
                else if (Model.RelatedTo == "000100346" || Model.Code == "000100346")
                {
                    //var mRelateData = Model.AddOnList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                    //var mSparePart = ctxTFAT.ItemMaster.Where(x => x.BaseGr == mRelateData).Select(x => new { x.Name, x.Code }).Distinct().ToList();
                    //var mItemGroup = ctxTFAT.ItemGroups.Where(x => x.Hide == false).Select(x => new { x.Name, x.Code }).Distinct().ToList();
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F012",
                    //    Head = "Product Group",
                    //    ApplCode = mRelateData,
                    //    QueryText = "Select^" + String.Join("^", mItemGroup.Select(x => x.Name).ToList()),
                    //    QueryCode = "Select^" + String.Join("^", mItemGroup.Select(x => x.Code).ToList()),
                    //    FldType = "C",
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F001",
                    //    Head = "Pick From",
                    //    ApplCode = Model.FromType,
                    //    QueryText = "Master^Direct",
                    //    QueryCode = "Master^Direct",
                    //    FldType = "C"

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F002",
                    //    Head = "Spare Part",
                    //    ApplCode = "",
                    //    QueryText = "Select^" + String.Join("^", mSparePart.Select(x => x.Name).ToList()),
                    //    QueryCode = "Select^" + String.Join("^", mSparePart.Select(x => x.Code).ToList()),
                    //    FldType = Model.FromType == "Direct" ? "T" : "C",

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F003",
                    //    Head = "Cost",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N",
                    //    InActive = true

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F013",
                    //    Head = "Qty",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    QueryCode = "",
                    //    FldType = "N",
                    //    InActive = true
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F004",
                    //    Head = "Warranty KM",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N"

                    //});

                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Receipt No",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "T"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "Receipt Dt",
                        ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                        QueryText = "",
                        FldType = "D"

                    });
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F008",
                    //    Head = "Install / Received Date",
                    //    ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                    //    QueryText = "",
                    //    FldType = "D"

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F009",
                    //    Head = "Current KM",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N"

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F005",
                    //    Head = "Warranty Days",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N",
                    //    

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F010",
                    //    Head = "Due Date",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "D",
                    //    

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F011",
                    //    Head = "Due km",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N"

                    //});

                }

                //Trip Details
                else if (Model.RelatedTo == "000100341" || Model.Code == "000100341")
                {

                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Trip Start Date",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "D"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "Trip End Date",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "D"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F003",
                        Head = "Trip Days",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F004",
                        Head = "Starting kms",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F005",
                        Head = "Ending kms",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"

                    });

                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F006",
                        Head = "Charge KMS",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F007",
                        Head = "Per KM Rate",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F008",
                        Head = "Trip Charges",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"

                    });

                }

                //Tyre Details
                else if (Model.RelatedTo == "000100345" || Model.Code == "000100345")
                {
                    List<string> tyretypeList = new List<string>();
                    tyretypeList.Add("New");
                    tyretypeList.Add("Remould");
                    tyretypeList.Add("Scrap");
                    tyretypeList.Add("Sale");
                    tyretypeList.Add("OutOfStock");
                    tyretypeList.Add("Stock");
                    //var mTyreTypes = ctxTFAT.TyreMaster.Select(x => x.TyreType).Distinct().ToList();
                    //var mRelateData = Model.AddOnList.Where(x => x.Fld == "F015").Select(x => x.ApplCode).FirstOrDefault();
                    //var mTyreTypes = ctxTFAT.ItemMaster.Where(x => x.BaseGr == mRelateData).Select(x => new { x.Name, x.Code }).Distinct().ToList();
                    //var mItemGroup = ctxTFAT.ItemGroups.Where(x => x.Hide == false).Select(x => new { x.Name, x.Code }).Distinct().ToList();
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F015",
                    //    Head = "Product Group",
                    //    ApplCode = mRelateData,
                    //    QueryText = "Select^" + String.Join("^", mItemGroup.Select(x => x.Name).ToList()),
                    //    QueryCode = "Select^" + String.Join("^", mItemGroup.Select(x => x.Code).ToList()),
                    //    FldType = "C",
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F001",
                    //    Head = "Pick From",
                    //    ApplCode = "",
                    //    QueryText = "Master^Direct",
                    //    QueryCode = "Master^Direct",
                    //    FldType = "C"

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F002",
                    //    Head = "Product",
                    //    ApplCode = "",
                    //    QueryText = "Select^" + String.Join("^", mTyreTypes.Select(x => x.Name).ToList()),
                    //    QueryCode = "Select^" + String.Join("^", mTyreTypes.Select(x => x.Code).ToList()),
                    //    FldType = "C",
                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F003",
                    //    Head = "Cost",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N"

                    //});
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F001",
                        Head = "Tyre Type",
                        ApplCode = "",
                        QueryText = "Select^" + String.Join("^", tyretypeList),
                        QueryCode = "Select^" + String.Join("^", tyretypeList),
                        FldType = "C",
                    });
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F004",
                    //    Head = "Warranty KM",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N"

                    //});

                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F002",
                        Head = "Install For",
                        ApplCode = "",
                        QueryText = "Tyre^Stepnee",
                        FldType = "R"

                    });


                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F003",
                        Head = "Tyre Placed No",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "N"

                    });
                    truckaddonlist.Add(new AddOns()
                    {
                        Fld = "F004",
                        Head = "Tyre SerialNo",
                        ApplCode = "",
                        QueryText = "",
                        FldType = "T"

                    });



                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F009",
                    //    Head = "Mfg Date",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "D"

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F010",
                    //    Head = "Install / Received Date",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "D"

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F011",
                    //    Head = "Install KM",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N"

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F005",
                    //    Head = "Expiry Days",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N",
                    //    

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F012",
                    //    Head = "Expiry Date",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "D",
                    //    

                    //});
                    //truckaddonlist.Add(new AddOns()
                    //{
                    //    Fld = "F013",
                    //    Head = "Expiry KM",
                    //    ApplCode = "",
                    //    QueryText = "",
                    //    FldType = "N"

                    //});

                }

                if (Model.AddOnList != null && Model.AddOnList.Count > 0)
                {
                    string FromType = "";
                    foreach (var i in Model.AddOnList)
                    {
                        //if ((Model.RelatedTo == "000100345" || Model.Code == "000100345") && i.Fld == "F001")
                        //{
                        //    FromType = i.ApplCode;
                        //}
                        //if ((Model.RelatedTo == "000100346" || Model.Code == "000100346") && i.Fld == "F001")
                        //{
                        //    FromType = i.ApplCode;
                        //}

                        foreach (var at in truckaddonlist.Where(x => x.Fld == i.Fld))
                        {
                            at.ApplCode = i.ApplCode;
                            //if ((Model.RelatedTo == "000100345" || Model.Code == "000100345") && FromType == "Direct" && at.Fld == "F002")
                            //{
                            //    at.FldType = "T";
                            //}
                            //if ((Model.RelatedTo == "000100346" || Model.Code == "000100346") && FromType == "Direct" && at.Fld == "F002")
                            //{
                            //    at.FldType = "T";
                            //}

                        }

                    }
                }
            }



            return truckaddonlist;
        }

        public List<AddOns> GetSessionEditItemWiseData(CreditPurchaseVM Model)
        {

            List<AddOns> truckaddonlist = new List<AddOns>();
            var mRelateData = Model.ItemList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
            var mItem = ctxTFAT.ItemMaster.Where(x => x.BaseGr == mRelateData).Select(x => new { x.Name, x.Code }).Distinct().OrderBy(x => x.Name).ToList();
            var mItemGroup = ctxTFAT.ItemGroups.Where(x => x.Hide == false).Select(x => new { x.Name, x.Code }).Distinct().ToList();
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F001",
                Head = "Product Group",
                ApplCode = mRelateData,
                QueryText = "Select^" + String.Join("^", mItemGroup.Select(x => x.Name).ToList()),
                QueryCode = "Select^" + String.Join("^", mItemGroup.Select(x => x.Code).ToList()),
                FldType = "C",
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F002",
                Head = "Pick From",
                ApplCode = Model.FromType,
                QueryText = "Master^Direct",
                QueryCode = "Master^Direct",
                FldType = "C",
                Hide = true
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F003",
                Head = "Item",
                ApplCode = "",
                QueryText = "Select^" + String.Join("^", mItem.Select(x => x.Name).ToList()),
                QueryCode = "Select^" + String.Join("^", mItem.Select(x => x.Code).ToList()),
                FldType = Model.FromType == "Direct" ? "T" : "C",
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F004",
                Head = "Cost",
                ApplCode = "0",
                QueryText = "",
                QueryCode = "",
                FldType = "N",
                InActive = true
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F005",
                Head = "Qty",
                ApplCode = "1",
                QueryText = "",
                QueryCode = "",
                FldType = "N",
                InActive = true
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F006",
                Head = "Total Amount",
                ApplCode = "0",
                QueryText = "",
                QueryCode = "",
                FldType = "N",
                InActive = true
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F007",
                Head = "Warranty KM",
                ApplCode = "0",
                QueryText = "",
                FldType = "N"
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F008",
                Head = "Install KM",
                ApplCode = "0",
                QueryText = "",
                FldType = "N",

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F009",
                Head = "Due km",
                ApplCode = "0",
                QueryText = "",
                FldType = "N"
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F010",
                Head = "Warranty Days",
                ApplCode = "0",
                QueryText = "",
                FldType = "N",

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F011",
                Head = "MFG Date",
                ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                QueryText = "",
                FldType = "D",
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F012",
                Head = "Install / Received Date",
                ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                QueryText = "",
                FldType = "D"
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F013",
                Head = "Due Date",
                ApplCode = Model.DocDate.ToString("yyyy-MM-dd"),
                QueryText = "",
                FldType = "D",

            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F014",
                Head = "HSN CODE",
                ApplCode = "",
                QueryText = "",
                FldType = "X",
            });
            truckaddonlist.Add(new AddOns()
            {
                Fld = "F015",
                Head = "Description",
                ApplCode = "",
                QueryText = "",
                FldType = "M",
            });

            if (Model.ItemList != null && Model.ItemList.Count > 0)
            {
                string FromType = "";
                foreach (var i in Model.ItemList)
                {
                    if (i.Fld == "F002")
                    {
                        FromType = i.ApplCode;
                    }

                    foreach (var at in truckaddonlist.Where(x => x.Fld == i.Fld))
                    {
                        at.ApplCode = i.ApplCode;
                        if (FromType == "Direct" && at.Fld == "F003")
                        {
                            at.FldType = "T";
                        }
                    }
                }
            }
            return truckaddonlist;
        }

        [HttpPost]
        public ActionResult GetSparePartDetail(CreditPurchaseVM Model, string KM, string Days)
        {
            var mSpare = ctxTFAT.ItemMaster.Where(x => x.Code == Model.Type).Select(x => x).FirstOrDefault();
            var mExpdate = DateTime.Now.Date;
            var mDate = Convert.ToDateTime(Model.FromDate);
            double McOST = 0;
            double mKm = 0;
            int mexpdays = 0;
            string Account = "", AccountName = "";
            if (String.IsNullOrEmpty(Model.Code))
            {
                Account = mSpare.Posting;
                AccountName = ctxTFAT.Master.Where(x => x.Code == mSpare.Posting).Select(x => x.Name).FirstOrDefault();

            }
            if (mSpare != null)
            {
                mexpdays = mSpare.ExpiryDays;
                mKm = mSpare.ExpiryKm;
                McOST = mSpare.Rate;
            }
            if (!String.IsNullOrEmpty(Days))
            {

                mexpdays = Convert.ToInt32(Days);
            }
            if (!String.IsNullOrEmpty(KM))
            {
                mKm = Convert.ToInt32(KM);
            }

            if (mexpdays == 0)
            {
                mExpdate = mDate.AddDays(mexpdays);
            }
            else
            {
                mExpdate = mDate.AddDays(mexpdays);
            }
            if (mKm == 0)
            {
                mKm = Convert.ToInt32(KM == "" ? "0" : KM);
            }
            string GSTNAME = "";
            decimal IGSTRATE = 0, CGSTRATE = 0, SGSTRATE = 0;
            if (!String.IsNullOrEmpty(mSpare.GSTCode))
            {
                var pstate = ctxTFAT.Address.Where(x => x.Code == Model.BankCashCode).Select(x => x.State).FirstOrDefault();
                if (String.IsNullOrEmpty(pstate))
                {
                    pstate = "19";
                }
                var branchstate = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.State).FirstOrDefault();
                if (pstate == branchstate)
                {
                    var result = ctxTFAT.TaxMaster.Where(x => x.Code == mSpare.GSTCode).Select(x => new { Name = x.Name, IGSTRate = (decimal)0, SGSTRate = x.SGSTRate.Value, CGSTRate = x.CGSTRate.Value }).FirstOrDefault();
                    GSTNAME = result.Name;
                    IGSTRATE = result.IGSTRate;
                    CGSTRATE = result.CGSTRate;
                    SGSTRATE = result.SGSTRate;
                }
                else
                {
                    var result = ctxTFAT.TaxMaster.Where(x => x.Code == mSpare.GSTCode).Select(x => new { Name = x.Name, IGSTRate = x.IGSTRate.Value, SGSTRate = (decimal)0, CGSTRate = (decimal)0 }).FirstOrDefault();
                    GSTNAME = result.Name;
                    IGSTRATE = result.IGSTRate;
                    CGSTRATE = result.CGSTRate;
                    SGSTRATE = result.SGSTRate;
                }
            }



            return Json(new
            {
                KM = mKm,
                Days = mexpdays,
                Cost = McOST,
                ExpDays = mexpdays == 0 ? "" : mExpdate.ToString("yyyy-MM-dd"),
                Account = Account,
                AccountName = AccountName,
                GSTCODE = mSpare.GSTCode,
                GSTNAME = GSTNAME,
                IGSTRATE = IGSTRATE,
                CGSTRATE = CGSTRATE,
                SGSTRATE = SGSTRATE,
            }, JsonRequestBehavior.AllowGet);
            //mExpdate = mDate.AddDays(mSpare.ExpiryDays);
            //var html = ViewHelper.RenderPartialView(this, "SpareDetails", new OtherTransactModel() { KM = mSpare.KM, Cost = mSpare.Cost, ExpDays = mSpare.ExpiryDays });

            //return Json(new
            //{
            //    Html = html,
            //    Cost = mSpare.Cost,
            //    ExpDays = mExpdate.ToString("yyyy-MM-dd"),
            //    KM = mSpare.KM,

            //}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetItemPartDetail(CreditPurchaseVM Model, string KM, string Days)
        {
            var mSpare = ctxTFAT.ItemMaster.Where(x => x.Code == Model.Type).Select(x => x).FirstOrDefault();
            var mExpdate = DateTime.Now.Date;
            var mDate = Convert.ToDateTime(Model.FromDate);
            double McOST = 0;
            double mKm = 0;
            int mexpdays = 0;
            string Account = "", AccountName = "";
            if (String.IsNullOrEmpty(Model.Code) && mSpare != null)
            {
                Account = mSpare.Posting;
                AccountName = ctxTFAT.Master.Where(x => x.Code == mSpare.Posting).Select(x => x.Name).FirstOrDefault();

            }
            if (mSpare != null)
            {
                mexpdays = mSpare.ExpiryDays;
                mKm = mSpare.ExpiryKm;
                McOST = mSpare.Rate;
            }
            if (!String.IsNullOrEmpty(Days) && Days != "0")
            {
                mexpdays = Convert.ToInt32(Days);
            }
            if (!String.IsNullOrEmpty(KM) && KM != "0")
            {
                mKm = Convert.ToInt32(KM);
            }
            if (mexpdays == 0)
            {
                mExpdate = mDate.AddDays(mexpdays);
            }
            else
            {
                mExpdate = mDate.AddDays(mexpdays);
            }
            if (mKm == 0)
            {
                mKm = Convert.ToInt32(KM == "" ? "0" : KM);
            }
            string GSTNAME = "", GSTCODE = "", HSNCODE = "", Descr = "";
            decimal IGSTRATE = 0, CGSTRATE = 0, SGSTRATE = 0;
            if (mSpare != null)
            {
                var EffectDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                if (ctxTFAT.TfatComp.Select(x => x.UseHSNMaster).FirstOrDefault())
                {
                    if (!String.IsNullOrEmpty(mSpare.HSNCode))
                    {
                        var pstate = ctxTFAT.Address.Where(x => x.Code == (String.IsNullOrEmpty(mSpare.Posting) == true ? Model.BankCashCode : mSpare.Posting)).Select(x => x.State).FirstOrDefault();
                        if (String.IsNullOrEmpty(pstate))
                        {
                            pstate = "19";
                        }
                        var branchstate = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.State).FirstOrDefault();
                        if (pstate == branchstate)
                        {
                            var result = ctxTFAT.HSNRates.Where(x => x.Code == mSpare.HSNCode && x.EffDate <= EffectDate).OrderByDescending(x => x.EffDate).Select(x => new { IGSTRate = (decimal)0, SGSTRate = x.SGSTRate.Value, CGSTRate = x.CGSTRate.Value }).FirstOrDefault();
                            GSTNAME = ctxTFAT.HSNMaster.Where(x => x.Code == mSpare.HSNCode).Select(x => x.Name).FirstOrDefault();
                            GSTCODE = mSpare.HSNCode;
                            IGSTRATE = result.IGSTRate;
                            CGSTRATE = result.CGSTRate;
                            SGSTRATE = result.SGSTRate;
                        }
                        else
                        {
                            var result = ctxTFAT.HSNRates.Where(x => x.Code == mSpare.HSNCode && x.EffDate <= EffectDate).OrderByDescending(x => x.EffDate).Select(x => new { IGSTRate = x.IGSTRate.Value, SGSTRate = (decimal)0, CGSTRate = (decimal)0 }).FirstOrDefault();
                            GSTNAME = ctxTFAT.HSNMaster.Where(x => x.Code == mSpare.HSNCode).Select(x => x.Name).FirstOrDefault();
                            GSTCODE = mSpare.HSNCode;
                            IGSTRATE = result.IGSTRate;
                            CGSTRATE = result.CGSTRate;
                            SGSTRATE = result.SGSTRate;
                        }
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(mSpare.GSTCode))
                    {
                        var pstate = ctxTFAT.Address.Where(x => x.Code == (String.IsNullOrEmpty(mSpare.Posting) == true ? Model.BankCashCode : mSpare.Posting)).Select(x => x.State).FirstOrDefault();
                        if (String.IsNullOrEmpty(pstate))
                        {
                            pstate = "19";
                        }
                        var branchstate = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.State).FirstOrDefault();
                        if (pstate == branchstate)
                        {
                            var result = ctxTFAT.TaxMaster.Where(x => x.Code == mSpare.GSTCode).Select(x => new { Name = x.Name, IGSTRate = (decimal)0, SGSTRate = x.SGSTRate.Value, CGSTRate = x.CGSTRate.Value }).FirstOrDefault();
                            GSTNAME = result.Name;
                            GSTCODE = mSpare.GSTCode;
                            IGSTRATE = result.IGSTRate;
                            CGSTRATE = result.CGSTRate;
                            SGSTRATE = result.SGSTRate;
                        }
                        else
                        {
                            var result = ctxTFAT.TaxMaster.Where(x => x.Code == mSpare.GSTCode).Select(x => new { Name = x.Name, IGSTRate = x.IGSTRate.Value, SGSTRate = (decimal)0, CGSTRate = (decimal)0 }).FirstOrDefault();
                            GSTNAME = result.Name;
                            GSTCODE = mSpare.GSTCode;
                            IGSTRATE = result.IGSTRate;
                            CGSTRATE = result.CGSTRate;
                            SGSTRATE = result.SGSTRate;
                        }
                    }
                }


                HSNCODE = mSpare.HSNCode;
                Descr = mSpare.Narr;
            }
            string HSNCODENAme = "";
            if (!String.IsNullOrEmpty(HSNCODE))
            {
                HSNCODENAme = ctxTFAT.HSNMaster.Where(x => x.Code == HSNCODE).Select(x => x.Name).FirstOrDefault();
            }
            return Json(new
            {
                KM = mKm,
                Days = mexpdays,
                Cost = McOST,
                ExpDays = mexpdays == 0 ? "" : mExpdate.ToString("yyyy-MM-dd"),
                Account = Account,
                AccountName = AccountName,
                GSTCODE = GSTCODE,
                GSTNAME = GSTNAME,
                IGSTRATE = IGSTRATE,
                CGSTRATE = CGSTRATE,
                SGSTRATE = SGSTRATE,
                HSNCODE = HSNCODE,
                HSNCODENAme = HSNCODENAme,
                Descr = Descr,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetTyreDetail(CreditPurchaseVM Model, string KM, string Days)
        {
            var mSpare = ctxTFAT.ItemMaster.Where(x => x.Code == Model.Type).Select(x => x).FirstOrDefault();
            var mExpdate = DateTime.Now.Date;
            var mDate = Convert.ToDateTime(Model.FromDate);
            double McOST = 0;
            double mKm = 0;
            int mexpdays = 0;
            string Account = "", AccountName = "";
            if (String.IsNullOrEmpty(Model.Code))
            {
                Account = mSpare.Posting;
                AccountName = ctxTFAT.Master.Where(x => x.Code == mSpare.Posting).Select(x => x.Name).FirstOrDefault();

            }
            if (mSpare != null)
            {
                mexpdays = mSpare.ExpiryDays;
                mKm = mSpare.ExpiryKm;
                McOST = mSpare.Rate;
            }

            if (!String.IsNullOrEmpty(Days))
            {
                mexpdays = Convert.ToInt32(Days);
            }
            if (!String.IsNullOrEmpty(KM))
            {
                mKm = Convert.ToInt32(KM);
            }

            if (mexpdays == 0)
            {
                mExpdate = mDate.AddDays(mexpdays);
            }
            else
            {
                mExpdate = mDate.AddDays(mexpdays);
            }
            if (mKm == 0)
            {
                mKm = Convert.ToInt32(KM == "" ? "0" : KM);
            }
            string GSTNAME = "", GSTCODE = "";
            decimal IGSTRATE = 0, CGSTRATE = 0, SGSTRATE = 0;
            if (mSpare != null)
            {
                if (!String.IsNullOrEmpty(mSpare.GSTCode))
                {
                    var pstate = ctxTFAT.Address.Where(x => x.Code == (String.IsNullOrEmpty(mSpare.Posting) == true ? Model.BankCashCode : mSpare.Posting)).Select(x => x.State).FirstOrDefault();
                    if (String.IsNullOrEmpty(pstate))
                    {
                        pstate = "19";
                    }
                    var branchstate = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.State).FirstOrDefault();
                    if (pstate == branchstate)
                    {
                        var result = ctxTFAT.TaxMaster.Where(x => x.Code == mSpare.GSTCode).Select(x => new { Name = x.Name, IGSTRate = (decimal)0, SGSTRate = x.SGSTRate.Value, CGSTRate = x.CGSTRate.Value }).FirstOrDefault();
                        GSTNAME = result.Name;
                        GSTCODE = mSpare.GSTCode;
                        IGSTRATE = result.IGSTRate;
                        CGSTRATE = result.CGSTRate;
                        SGSTRATE = result.SGSTRate;
                    }
                    else
                    {
                        var result = ctxTFAT.TaxMaster.Where(x => x.Code == mSpare.GSTCode).Select(x => new { Name = x.Name, IGSTRate = x.IGSTRate.Value, SGSTRate = (decimal)0, CGSTRate = (decimal)0 }).FirstOrDefault();
                        GSTNAME = result.Name;
                        GSTCODE = mSpare.GSTCode;
                        IGSTRATE = result.IGSTRate;
                        CGSTRATE = result.CGSTRate;
                        SGSTRATE = result.SGSTRate;
                    }
                }
                else if (!String.IsNullOrEmpty(mSpare.HSNCode))
                {
                    var pstate = ctxTFAT.Address.Where(x => x.Code == (String.IsNullOrEmpty(mSpare.Posting) == true ? Model.BankCashCode : mSpare.Posting)).Select(x => x.State).FirstOrDefault();
                    if (String.IsNullOrEmpty(pstate))
                    {
                        pstate = "19";
                    }
                    var branchstate = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.State).FirstOrDefault();
                    if (pstate == branchstate)
                    {
                        var result = ctxTFAT.HSNRates.Where(x => x.Code == mSpare.HSNCode).Select(x => new { IGSTRate = (decimal)0, SGSTRate = x.SGSTRate.Value, CGSTRate = x.CGSTRate.Value }).FirstOrDefault();
                        GSTNAME = ctxTFAT.HSNMaster.Where(x => x.Code == mSpare.HSNCode).Select(x => x.Name).FirstOrDefault();
                        GSTCODE = mSpare.HSNCode;
                        IGSTRATE = result.IGSTRate;
                        CGSTRATE = result.CGSTRate;
                        SGSTRATE = result.SGSTRate;
                    }
                    else
                    {
                        var result = ctxTFAT.HSNRates.Where(x => x.Code == mSpare.HSNCode).Select(x => new { IGSTRate = x.IGSTRate.Value, SGSTRate = (decimal)0, CGSTRate = (decimal)0 }).FirstOrDefault();
                        GSTNAME = ctxTFAT.HSNMaster.Where(x => x.Code == mSpare.HSNCode).Select(x => x.Name).FirstOrDefault();
                        GSTCODE = mSpare.HSNCode;
                        IGSTRATE = result.IGSTRate;
                        CGSTRATE = result.CGSTRate;
                        SGSTRATE = result.SGSTRate;
                    }
                }
            }

            return Json(new
            {

                KM = mKm,
                Days = mexpdays,
                Cost = McOST,
                ExpDays = mexpdays == 0 ? "" : mExpdate.ToString("yyyy-MM-dd"),
                Account = Account,
                AccountName = AccountName,
                GSTCODE = GSTCODE,
                GSTNAME = GSTNAME,
                IGSTRATE = IGSTRATE,
                CGSTRATE = CGSTRATE,
                SGSTRATE = SGSTRATE,
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult GetCalcTyreDetail(CreditPurchaseVM Model)
        {

            var mExpdate = DateTime.Now.Date;
            var mDate = Convert.ToDateTime(Model.FromDate);
            var mexpdays = Convert.ToInt32(Model.ExpDays);
            var mKm = Model.KM;

            mExpdate = mDate.AddDays(mexpdays);

            return Json(new
            {

                KM = mKm,
                Days = mexpdays,
                ExpDays = mExpdate.ToString("yyyy-MM-dd"),

            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetTyreStockDetail(CreditPurchaseVM Model)
        {
            var MaintainStock = false;
            var ItemMaster = ctxTFAT.ItemMaster.Where(x => x.Code == Model.Type).FirstOrDefault();
            if (ItemMaster != null)
            {
                MaintainStock = ItemMaster.StockMaintain;
            }
            List<TyreStockSerial> mTyrestocks = new List<TyreStockSerial>();
            List<CreditPurchaseVM> mTyredetails = new List<CreditPurchaseVM>();
            if (MaintainStock)
            {
                string OldStkTableKey = "";
                var GetStock = ctxTFAT.TyreStockSerial.Where(x => x.TableKey.Trim() == Model.TableKey).FirstOrDefault();
                if (GetStock != null)
                {
                    OldStkTableKey = GetStock.ParentKey;
                }

                if (Model.Fld == "Tyre")
                {
                    var mtyreno = (string.IsNullOrEmpty(Model.TyreNo) == false) ? Convert.ToInt32(Model.TyreNo) : 0;
                    var mtyrep = ctxTFAT.VehicleMaster.Where(x => x.PostAc == Model.VehicleNo).Select(x => x.NoOfTyres).FirstOrDefault();
                    if (((mtyreno > mtyrep)) && mtyreno != 0)
                    {
                        return Json(new { Status = "Valid", Message = "Tyre Place On No " + Model.TyreNo + " Not exist in Selected Vehicle" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (Model.Fld == "Stepnee")
                {
                    var mtyreno = (string.IsNullOrEmpty(Model.TyreNo) == false) ? Convert.ToInt32(Model.TyreNo) : 0;
                    var mtyrep = ctxTFAT.VehicleMaster.Where(x => x.PostAc == Model.VehicleNo).Select(x => x.Stepney).FirstOrDefault();
                    if (((mtyreno > mtyrep)) && mtyreno != 0)
                    {
                        return Json(new { Status = "Valid", Message = "Stepnee No " + Model.TyreNo + " Not exist in Selected Vehicle" }, JsonRequestBehavior.AllowGet);
                    }
                }
                if (String.IsNullOrEmpty(Model.TableKey))
                {
                    mTyrestocks = ctxTFAT.TyreStockSerial.Where(x => ((x.TyreNo == Model.TyreNo && Model.Fld == "Tyre") || (x.StepneeNo == Model.TyreNo && Model.Fld == "Stepnee")) && x.Vehicle == Model.VehicleNo && x.StockAt == "Vehicle" && x.IsActive == true).Select(x => x).ToList();
                }
                else
                {
                    mTyrestocks = ctxTFAT.TyreStockSerial.Where(x => ((x.TyreNo == Model.TyreNo && Model.Fld == "Tyre") || (x.StepneeNo == Model.TyreNo && Model.Fld == "Stepnee")) && x.Vehicle == Model.VehicleNo /*&& x.StockAt == "Vehicle"*/ && x.TableKey == OldStkTableKey).Select(x => x).ToList();
                }
                if (mTyrestocks != null && mTyrestocks.Count > 0)
                {
                    foreach (var a in mTyrestocks)
                    {
                        mTyredetails.Add(new CreditPurchaseVM()
                        {
                            Name = ctxTFAT.VehicleMaster.Where(x => x.PostAc == a.Vehicle).Select(x => x.TruckNo).FirstOrDefault(),
                            DocuDate = a.Value1,
                            ActWt = Convert.ToDouble(a.Value2),
                            FEndDate = a.Value3,
                            ChgWt = Convert.ToDouble(a.Value4),
                            RECORDKEY = a.RECORDKEY,
                            ApplCode = a.Status,
                            Srl = a.SerialNo,
                            Code = a.Vehicle,
                            TableKey = a.TableKey,
                            StepneeNo = a.StepneeNo,
                            TyreNo = a.TyreNo,
                            StockAt = a.StockAt,
                            IsActive = a.IsActive
                        });
                    }

                }
            }
            var html = ViewHelper.RenderPartialView(this, "TyreStockSerial", new CreditPurchaseVM() { TyreStockList = mTyredetails });
            return Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
        }

        #region LedgerPost

        public ActionResult GetPostingNew(CreditPurchaseVM Model)
        {
            List<CreditPurchaseVM> FirstLedPostList = new List<CreditPurchaseVM>();
            List<CreditPurchaseVM> LedPostList = new List<CreditPurchaseVM>();
            int mCnt = 1;
            var objledgerdetail = (List<CreditPurchaseVM>)Session["Lothertrxlist"];

            if (objledgerdetail == null || objledgerdetail.Count() == 0)
            {
                return Json(new
                {

                    Status = "ErrorValid",
                    Message = "No list Found Cant Save"
                }, JsonRequestBehavior.AllowGet); ;
            }

            if (!String.IsNullOrEmpty(Model.Srl))
            {
                var result = ctxTFAT.DocTypes.Where(x => x.Code == "PUR00").Select(x => x).FirstOrDefault();
                if (Model.Srl.Length > (result.DocWidth))
                {
                    return Json(new
                    {
                        Status = "ErrorValid",
                        Message = "Document NO. Allow " + result.DocWidth + " Digit Only....!"
                    }, JsonRequestBehavior.AllowGet); ;
                }
            }

            var Date = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
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
            var gDate = ConvertDDMMYYTOYYMMDD(Model.BillDate.ToShortDateString());
            if (mtfatperd.StartDate > gDate || gDate > mtfatperd.LastDate)
            {
                return Json(new
                {

                    Status = "ErrorValid",
                    Message = "Selected Bill Date is not in Current Accounting Period..!"
                }, JsonRequestBehavior.AllowGet);
            }

            if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.Type == "PUR00" && x.LockDate == Date).FirstOrDefault() != null)
            {
                return Json(new
                {
                    Status = "ErrorValid",
                    Message = "Date is Locked By Period Lock System...!"
                }, JsonRequestBehavior.AllowGet); ;
            }

            bool SplitPosting = false;
            CreditPurchaseSetup purchaseSetup = ctxTFAT.CreditPurchaseSetup.FirstOrDefault();
            if (purchaseSetup != null)
            {
                if (purchaseSetup.SplitPosting)
                {
                    SplitPosting = true;
                }
            }

            if (SplitPosting == false)
            {
                foreach (var item in objledgerdetail)
                {
                    FirstLedPostList.Add(new CreditPurchaseVM()
                    {
                        Code = Model.BankCashCode,
                        AccountName = ctxTFAT.Master.Where(x => x.Code == Model.BankCashCode).Select(x => x.Name).FirstOrDefault(),
                        Debit = 0,
                        Credit = Math.Round(item.Amt, 2),
                        Branch = mbranchcode,
                        BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                        tempId = mCnt,
                        Narr = item.Narr,
                        AltCode = item.Code,
                    });
                    mCnt = mCnt + 1;
                    FirstLedPostList.Add(new CreditPurchaseVM()
                    {
                        Code = item.Code,
                        AccountName = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault(),
                        Debit = Math.Round(item.Amt, 2),
                        Credit = 0,
                        Branch = mbranchcode,
                        BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                        tempId = mCnt,
                        Narr = item.Narr,
                        AltCode = Model.BankCashCode,
                    });
                }

                #region Merge Posting

                mCnt = 1;
                LedPostList.Add(new CreditPurchaseVM()
                {
                    Code = Model.BankCashCode,
                    AccountName = ctxTFAT.Master.Where(x => x.Code == Model.BankCashCode).Select(x => x.Name).FirstOrDefault(),
                    Debit = 0,
                    Credit = Math.Round(FirstLedPostList.Where(x => x.Code == Model.BankCashCode).Sum(x => x.Credit), 2),
                    Branch = mbranchcode,
                    BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                    tempId = mCnt,
                    Narr = "",
                    AltCode = String.Join(",", FirstLedPostList.Where(x => x.Code == Model.BankCashCode).Select(x => x.AltCode).ToList()),
                });
                mCnt = mCnt + 1;
                foreach (var item in FirstLedPostList.Where(x => x.Code != Model.BankCashCode).ToList())
                {
                    LedPostList.Add(new CreditPurchaseVM()
                    {
                        Code = item.Code,
                        AccountName = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault(),
                        Debit = Math.Round(item.Debit, 2),
                        Credit = 0,
                        Branch = mbranchcode,
                        BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                        tempId = mCnt,
                        Narr = item.Narr,
                        AltCode = Model.BankCashCode,
                    });
                    mCnt = mCnt + 1;
                }

                #endregion

                FirstLedPostList = new List<CreditPurchaseVM>();
                int Xvnt = 1;

                foreach (var item in objledgerdetail)
                {
                    if (item.GSTFlag)
                    {
                        if (!String.IsNullOrEmpty(item.GSTCode))
                        {
                            TaxMaster taxMaster = ctxTFAT.TaxMaster.Where(x => x.Code == item.GSTCode).FirstOrDefault();
                            if (taxMaster != null)
                            {
                                string SGSTInputAccount = "000100032", CGSTInputAccount = "000100033", IGSTInputAccount = "000100034";

                                if (item.IGSTAmt > 0)
                                {
                                    FirstLedPostList.Add(new CreditPurchaseVM()
                                    {
                                        Code = Model.BankCashCode,
                                        AccountName = ctxTFAT.Master.Where(x => x.Code == Model.BankCashCode).Select(x => x.Name).FirstOrDefault(),
                                        Debit = 0,
                                        Credit = Math.Round(item.IGSTAmt, 2),
                                        Branch = mbranchcode,
                                        BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                        tempId = Xvnt,
                                        Narr = "",
                                        AltCode = IGSTInputAccount
                                    });
                                    Xvnt = Xvnt + 1;
                                    FirstLedPostList.Add(new CreditPurchaseVM()
                                    {
                                        Code = IGSTInputAccount,
                                        AccountName = ctxTFAT.Master.Where(x => x.Code == IGSTInputAccount).Select(x => x.Name).FirstOrDefault(),
                                        Debit = Math.Round(item.IGSTAmt, 2),
                                        Credit = 0,
                                        Branch = mbranchcode,
                                        BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                        tempId = Xvnt,
                                        Narr = "",
                                        AltCode = Model.BankCashCode
                                    });
                                    Xvnt = Xvnt + 1;
                                }
                                else
                                {
                                    if (item.CGSTAmt > 0)
                                    {
                                        FirstLedPostList.Add(new CreditPurchaseVM()
                                        {
                                            Code = Model.BankCashCode,
                                            AccountName = ctxTFAT.Master.Where(x => x.Code == Model.BankCashCode).Select(x => x.Name).FirstOrDefault(),
                                            Debit = 0,
                                            Credit = Math.Round(item.CGSTAmt, 2),
                                            Branch = mbranchcode,
                                            BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                            tempId = Xvnt,
                                            Narr = "",
                                            AltCode = CGSTInputAccount
                                        });
                                        Xvnt = Xvnt + 1;
                                        FirstLedPostList.Add(new CreditPurchaseVM()
                                        {
                                            Code = CGSTInputAccount,
                                            AccountName = ctxTFAT.Master.Where(x => x.Code == CGSTInputAccount).Select(x => x.Name).FirstOrDefault(),
                                            Debit = Math.Round(item.CGSTAmt, 2),
                                            Credit = 0,
                                            Branch = mbranchcode,
                                            BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                            tempId = Xvnt,
                                            Narr = "",
                                            AltCode = Model.BankCashCode
                                        });
                                        Xvnt = Xvnt + 1;
                                    }
                                    if (item.SGSTAmt > 0)
                                    {
                                        FirstLedPostList.Add(new CreditPurchaseVM()
                                        {
                                            Code = Model.BankCashCode,
                                            AccountName = ctxTFAT.Master.Where(x => x.Code == Model.BankCashCode).Select(x => x.Name).FirstOrDefault(),
                                            Debit = 0,
                                            Credit = Math.Round(item.SGSTAmt, 2),
                                            Branch = mbranchcode,
                                            BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                            tempId = Xvnt,
                                            Narr = "",
                                            AltCode = SGSTInputAccount
                                        });
                                        Xvnt = Xvnt + 1;
                                        FirstLedPostList.Add(new CreditPurchaseVM()
                                        {
                                            Code = SGSTInputAccount,
                                            AccountName = ctxTFAT.Master.Where(x => x.Code == SGSTInputAccount).Select(x => x.Name).FirstOrDefault(),
                                            Debit = Math.Round(item.SGSTAmt, 2),
                                            Credit = 0,
                                            Branch = mbranchcode,
                                            BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                            tempId = Xvnt,
                                            Narr = "",
                                            AltCode = Model.BankCashCode
                                        });
                                        Xvnt = Xvnt + 1;
                                    }
                                }
                            }
                            else
                            {
                                HSNMaster hSNMaster = ctxTFAT.HSNMaster.Where(x => x.Code == item.GSTCode).FirstOrDefault();
                                if (hSNMaster != null)
                                {
                                    if (item.IGSTAmt > 0)
                                    {
                                        FirstLedPostList.Add(new CreditPurchaseVM()
                                        {
                                            Code = Model.BankCashCode,
                                            AccountName = ctxTFAT.Master.Where(x => x.Code == Model.BankCashCode).Select(x => x.Name).FirstOrDefault(),
                                            Debit = 0,
                                            Credit = Math.Round(item.IGSTAmt, 2),
                                            Branch = mbranchcode,
                                            BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                            tempId = Xvnt,
                                            Narr = "",
                                            AltCode = hSNMaster.IGSTIn
                                        });
                                        Xvnt = Xvnt + 1;
                                        FirstLedPostList.Add(new CreditPurchaseVM()
                                        {
                                            Code = hSNMaster.IGSTIn,
                                            AccountName = ctxTFAT.Master.Where(x => x.Code == hSNMaster.IGSTIn).Select(x => x.Name).FirstOrDefault(),
                                            Debit = Math.Round(item.IGSTAmt, 2),
                                            Credit = 0,
                                            Branch = mbranchcode,
                                            BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                            tempId = Xvnt,
                                            Narr = "",
                                            AltCode = Model.BankCashCode
                                        });
                                        Xvnt = Xvnt + 1;
                                    }
                                    else
                                    {
                                        if (item.CGSTAmt > 0)
                                        {
                                            FirstLedPostList.Add(new CreditPurchaseVM()
                                            {
                                                Code = Model.BankCashCode,
                                                AccountName = ctxTFAT.Master.Where(x => x.Code == Model.BankCashCode).Select(x => x.Name).FirstOrDefault(),
                                                Debit = 0,
                                                Credit = Math.Round(item.CGSTAmt, 2),
                                                Branch = mbranchcode,
                                                BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                                tempId = Xvnt,
                                                Narr = "",
                                                AltCode = hSNMaster.CGSTIn
                                            });
                                            Xvnt = Xvnt + 1;
                                            FirstLedPostList.Add(new CreditPurchaseVM()
                                            {
                                                Code = hSNMaster.CGSTIn,
                                                AccountName = ctxTFAT.Master.Where(x => x.Code == hSNMaster.CGSTIn).Select(x => x.Name).FirstOrDefault(),
                                                Debit = Math.Round(item.CGSTAmt, 2),
                                                Credit = 0,
                                                Branch = mbranchcode,
                                                BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                                tempId = Xvnt,
                                                Narr = "",
                                                AltCode = Model.BankCashCode
                                            });
                                            Xvnt = Xvnt + 1;
                                        }
                                        if (item.SGSTAmt > 0)
                                        {
                                            FirstLedPostList.Add(new CreditPurchaseVM()
                                            {
                                                Code = Model.BankCashCode,
                                                AccountName = ctxTFAT.Master.Where(x => x.Code == Model.BankCashCode).Select(x => x.Name).FirstOrDefault(),
                                                Debit = 0,
                                                Credit = Math.Round(item.SGSTAmt, 2),
                                                Branch = mbranchcode,
                                                BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                                tempId = Xvnt,
                                                Narr = "",
                                                AltCode = hSNMaster.SGSTIn
                                            });
                                            Xvnt = Xvnt + 1;
                                            FirstLedPostList.Add(new CreditPurchaseVM()
                                            {
                                                Code = hSNMaster.SGSTIn,
                                                AccountName = ctxTFAT.Master.Where(x => x.Code == hSNMaster.SGSTIn).Select(x => x.Name).FirstOrDefault(),
                                                Debit = Math.Round(item.SGSTAmt, 2),
                                                Credit = 0,
                                                Branch = mbranchcode,
                                                BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                                tempId = Xvnt,
                                                Narr = "",
                                                AltCode = Model.BankCashCode
                                            });
                                            Xvnt = Xvnt + 1;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                #region Merge Posting
                if (FirstLedPostList.Count() > 0)
                {
                    LedPostList.Add(new CreditPurchaseVM()
                    {
                        Code = Model.BankCashCode,
                        AccountName = ctxTFAT.Master.Where(x => x.Code == Model.BankCashCode).Select(x => x.Name).FirstOrDefault(),
                        Debit = 0,
                        Credit = Math.Round(FirstLedPostList.Where(x => x.Code == Model.BankCashCode).Sum(x => x.Credit), 2),
                        Branch = mbranchcode,
                        BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                        tempId = mCnt,
                        Narr = "",
                        AltCode = String.Join(",", FirstLedPostList.Where(x => x.Code == Model.BankCashCode).Select(x => x.AltCode).ToList()),
                    });
                    mCnt = mCnt + 1;
                    foreach (var item in FirstLedPostList.Where(x => x.Code != Model.BankCashCode).ToList())
                    {
                        LedPostList.Add(new CreditPurchaseVM()
                        {
                            Code = item.Code,
                            AccountName = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault(),
                            Debit = Math.Round(item.Debit, 2),
                            Credit = 0,
                            Branch = mbranchcode,
                            BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                            tempId = mCnt,
                            Narr = item.Narr,
                            AltCode = Model.BankCashCode,
                        });
                        mCnt = mCnt + 1;
                    }
                }
                #endregion

                if (Model.TDSFlag)
                {
                    if (Model.TDSAmt > 0)
                    {
                        //var TDSCODE = ctxTFAT.TaxDetails.Where(x => x.Code == Model.BankCashCode).Select(x => x.TDSCode).FirstOrDefault();
                        //var PostAC = ctxTFAT.TDSMaster.Where(x => x.Code.ToString() == Model.TDSCode).Select(x => x.PostCode).FirstOrDefault();

                        //if (!String.IsNullOrEmpty(PostAC))
                        {
                            LedPostList.Add(new CreditPurchaseVM()
                            {
                                Code = Model.BankCashCode,
                                AccountName = ctxTFAT.Master.Where(x => x.Code == Model.BankCashCode).Select(x => x.Name).FirstOrDefault(),
                                Debit = Math.Round(Model.TDSAmt, 2),
                                Credit = 0,
                                Branch = mbranchcode,
                                BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                tempId = mCnt,
                                Narr = "",
                                AltCode = "000009994"
                            });
                            mCnt = mCnt + 1;
                            LedPostList.Add(new CreditPurchaseVM()
                            {
                                Code = "000009994",
                                AccountName = ctxTFAT.Master.Where(x => x.Code == "000009994").Select(x => x.Name).FirstOrDefault(),
                                Debit = 0,
                                Credit = Math.Round(Model.TDSAmt, 2),
                                Branch = mbranchcode,
                                BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                tempId = mCnt,
                                Narr = "",
                                AltCode = Model.BankCashCode
                            });
                            mCnt = mCnt + 1;
                        }


                    }
                }

                int NewMergeNo = 1;
                var mCharges2 = LedPostList.GroupBy(x => x.Code).Select(x => new CreditPurchaseVM { Code = x.Select(x1 => x1.Code).First() }).ToList(); ;
                List<CreditPurchaseVM> MergeList = new List<CreditPurchaseVM>();
                foreach (var item in mCharges2)
                {
                    var AltCodeMerge = LedPostList.Where(x => x.Code == item.Code).Select(x => x.AltCode.Split(',')).ToList();
                    List<string> DistinctAltCode = new List<string>();
                    foreach (var ALT in AltCodeMerge)
                    {
                        DistinctAltCode.AddRange(ALT.Distinct().ToList());
                    }

                    CreditPurchaseVM Merge = new CreditPurchaseVM();
                    Merge.Code = item.Code;
                    Merge.AccountName = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault();
                    Merge.Debit = Math.Round(LedPostList.Where(x => x.Code == item.Code).Sum(x => x.Debit), 2);
                    Merge.Credit = Math.Round(LedPostList.Where(x => x.Code == item.Code).Sum(x => x.Credit), 2);
                    Merge.Branch = LedPostList.Where(x => x.Code == item.Code).Select(x => x.Branch).FirstOrDefault();
                    Merge.BranchName = LedPostList.Where(x => x.Code == item.Code).Select(x => x.BranchName).FirstOrDefault();
                    Merge.tempId = NewMergeNo;
                    Merge.Narr = "";
                    Merge.AltCode = String.Join(",", DistinctAltCode.Distinct().ToList());
                    MergeList.Add(Merge);
                    ++NewMergeNo;
                }

                LedPostList = new List<CreditPurchaseVM>();
                LedPostList = MergeList;
            }
            else
            {
                foreach (var item in objledgerdetail)
                {
                    LedPostList.Add(new CreditPurchaseVM()
                    {
                        Code = Model.BankCashCode,
                        AccountName = ctxTFAT.Master.Where(x => x.Code == Model.BankCashCode).Select(x => x.Name).FirstOrDefault(),
                        Debit = 0,
                        Credit = Math.Round(item.Amt, 2),
                        Branch = mbranchcode,
                        BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                        tempId = mCnt,
                        Narr = "",
                        AltCode = item.Code
                    });
                    mCnt = mCnt + 1;
                    LedPostList.Add(new CreditPurchaseVM()
                    {
                        Code = item.Code,
                        AccountName = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault(),
                        Debit = Math.Round(item.Amt, 2),
                        Credit = 0,
                        Branch = mbranchcode,
                        BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                        tempId = mCnt,
                        Narr = item.Narr,
                        AltCode = Model.BankCashCode
                    });
                    mCnt = mCnt + 1;
                    if (item.GSTFlag)
                    {
                        if (!String.IsNullOrEmpty(item.GSTCode))
                        {
                            TaxMaster taxMaster = ctxTFAT.TaxMaster.Where(x => x.Code == item.GSTCode).FirstOrDefault();
                            if (taxMaster != null)
                            {
                                string SGSTInputAccount = "000100032", CGSTInputAccount = "000100033", IGSTInputAccount = "000100034";

                                if (item.IGSTAmt > 0)
                                {
                                    LedPostList.Add(new CreditPurchaseVM()
                                    {
                                        Code = Model.BankCashCode,
                                        AccountName = ctxTFAT.Master.Where(x => x.Code == Model.BankCashCode).Select(x => x.Name).FirstOrDefault(),
                                        Debit = 0,
                                        Credit = Math.Round(item.IGSTAmt, 2),
                                        Branch = mbranchcode,
                                        BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                        tempId = mCnt,
                                        Narr = "",
                                        AltCode = IGSTInputAccount
                                    });
                                    mCnt = mCnt + 1;
                                    LedPostList.Add(new CreditPurchaseVM()
                                    {
                                        Code = IGSTInputAccount,
                                        AccountName = ctxTFAT.Master.Where(x => x.Code == IGSTInputAccount).Select(x => x.Name).FirstOrDefault(),
                                        Debit = Math.Round(item.IGSTAmt, 2),
                                        Credit = 0,
                                        Branch = mbranchcode,
                                        BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                        tempId = mCnt,
                                        Narr = "",
                                        AltCode = Model.BankCashCode
                                    });
                                    mCnt = mCnt + 1;
                                }
                                else
                                {
                                    if (item.CGSTAmt > 0)
                                    {
                                        LedPostList.Add(new CreditPurchaseVM()
                                        {
                                            Code = Model.BankCashCode,
                                            AccountName = ctxTFAT.Master.Where(x => x.Code == Model.BankCashCode).Select(x => x.Name).FirstOrDefault(),
                                            Debit = 0,
                                            Credit = Math.Round(item.CGSTAmt, 2),
                                            Branch = mbranchcode,
                                            BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                            tempId = mCnt,
                                            Narr = "",
                                            AltCode = CGSTInputAccount
                                        });
                                        mCnt = mCnt + 1;
                                        LedPostList.Add(new CreditPurchaseVM()
                                        {
                                            Code = CGSTInputAccount,
                                            AccountName = ctxTFAT.Master.Where(x => x.Code == CGSTInputAccount).Select(x => x.Name).FirstOrDefault(),
                                            Debit = Math.Round(item.CGSTAmt, 2),
                                            Credit = 0,
                                            Branch = mbranchcode,
                                            BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                            tempId = mCnt,
                                            Narr = "",
                                            AltCode = Model.BankCashCode
                                        });
                                        mCnt = mCnt + 1;
                                    }
                                    if (item.SGSTAmt > 0)
                                    {
                                        LedPostList.Add(new CreditPurchaseVM()
                                        {
                                            Code = Model.BankCashCode,
                                            AccountName = ctxTFAT.Master.Where(x => x.Code == Model.BankCashCode).Select(x => x.Name).FirstOrDefault(),
                                            Debit = 0,
                                            Credit = Math.Round(item.SGSTAmt, 2),
                                            Branch = mbranchcode,
                                            BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                            tempId = mCnt,
                                            Narr = "",
                                            AltCode = SGSTInputAccount
                                        });
                                        mCnt = mCnt + 1;
                                        LedPostList.Add(new CreditPurchaseVM()
                                        {
                                            Code = SGSTInputAccount,
                                            AccountName = ctxTFAT.Master.Where(x => x.Code == SGSTInputAccount).Select(x => x.Name).FirstOrDefault(),
                                            Debit = Math.Round(item.SGSTAmt, 2),
                                            Credit = 0,
                                            Branch = mbranchcode,
                                            BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                            tempId = mCnt,
                                            Narr = "",
                                            AltCode = Model.BankCashCode
                                        });
                                        mCnt = mCnt + 1;
                                    }
                                }
                            }
                            else
                            {
                                HSNMaster hSNMaster = ctxTFAT.HSNMaster.Where(x => x.Code == item.GSTCode).FirstOrDefault();
                                if (hSNMaster != null)
                                {
                                    if (item.IGSTAmt > 0)
                                    {
                                        LedPostList.Add(new CreditPurchaseVM()
                                        {
                                            Code = Model.BankCashCode,
                                            AccountName = ctxTFAT.Master.Where(x => x.Code == Model.BankCashCode).Select(x => x.Name).FirstOrDefault(),
                                            Debit = 0,
                                            Credit = Math.Round(item.IGSTAmt, 2),
                                            Branch = mbranchcode,
                                            BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                            tempId = mCnt,
                                            Narr = "",
                                            AltCode = hSNMaster.IGSTIn
                                        });
                                        mCnt = mCnt + 1;
                                        LedPostList.Add(new CreditPurchaseVM()
                                        {
                                            Code = hSNMaster.IGSTIn,
                                            AccountName = ctxTFAT.Master.Where(x => x.Code == hSNMaster.IGSTIn).Select(x => x.Name).FirstOrDefault(),
                                            Debit = Math.Round(item.IGSTAmt, 2),
                                            Credit = 0,
                                            Branch = mbranchcode,
                                            BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                            tempId = mCnt,
                                            Narr = "",
                                            AltCode = Model.BankCashCode
                                        });
                                        mCnt = mCnt + 1;
                                    }
                                    else
                                    {
                                        if (item.CGSTAmt > 0)
                                        {
                                            LedPostList.Add(new CreditPurchaseVM()
                                            {
                                                Code = Model.BankCashCode,
                                                AccountName = ctxTFAT.Master.Where(x => x.Code == Model.BankCashCode).Select(x => x.Name).FirstOrDefault(),
                                                Debit = 0,
                                                Credit = Math.Round(item.CGSTAmt, 2),
                                                Branch = mbranchcode,
                                                BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                                tempId = mCnt,
                                                Narr = "",
                                                AltCode = hSNMaster.CGSTIn
                                            });
                                            mCnt = mCnt + 1;
                                            LedPostList.Add(new CreditPurchaseVM()
                                            {
                                                Code = hSNMaster.CGSTIn,
                                                AccountName = ctxTFAT.Master.Where(x => x.Code == hSNMaster.CGSTIn).Select(x => x.Name).FirstOrDefault(),
                                                Debit = Math.Round(item.CGSTAmt, 2),
                                                Credit = 0,
                                                Branch = mbranchcode,
                                                BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                                tempId = mCnt,
                                                Narr = "",
                                                AltCode = Model.BankCashCode
                                            });
                                            mCnt = mCnt + 1;
                                        }
                                        if (item.SGSTAmt > 0)
                                        {
                                            LedPostList.Add(new CreditPurchaseVM()
                                            {
                                                Code = Model.BankCashCode,
                                                AccountName = ctxTFAT.Master.Where(x => x.Code == Model.BankCashCode).Select(x => x.Name).FirstOrDefault(),
                                                Debit = 0,
                                                Credit = Math.Round(item.SGSTAmt, 2),
                                                Branch = mbranchcode,
                                                BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                                tempId = mCnt,
                                                Narr = "",
                                                AltCode = hSNMaster.SGSTIn
                                            });
                                            mCnt = mCnt + 1;
                                            LedPostList.Add(new CreditPurchaseVM()
                                            {
                                                Code = hSNMaster.SGSTIn,
                                                AccountName = ctxTFAT.Master.Where(x => x.Code == hSNMaster.SGSTIn).Select(x => x.Name).FirstOrDefault(),
                                                Debit = Math.Round(item.SGSTAmt, 2),
                                                Credit = 0,
                                                Branch = mbranchcode,
                                                BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                                tempId = mCnt,
                                                Narr = "",
                                                AltCode = Model.BankCashCode
                                            });
                                            mCnt = mCnt + 1;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (Model.TDSFlag)
                {
                    if (Model.TDSAmt > 0)
                    {
                        //var TDSCODE = ctxTFAT.TaxDetails.Where(x => x.Code == Model.BankCashCode).Select(x => x.TDSCode).FirstOrDefault();
                        //var PostAC = ctxTFAT.TDSMaster.Where(x => x.Code.ToString() == Model.TDSCode).Select(x => x.PostCode).FirstOrDefault();

                        //if (!String.IsNullOrEmpty(PostAC))
                        {
                            LedPostList.Add(new CreditPurchaseVM()
                            {
                                Code = Model.BankCashCode,
                                AccountName = ctxTFAT.Master.Where(x => x.Code == Model.BankCashCode).Select(x => x.Name).FirstOrDefault(),
                                Debit = Math.Round(Model.TDSAmt, 2),
                                Credit = 0,
                                Branch = mbranchcode,
                                BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                tempId = mCnt,
                                Narr = "",
                                AltCode = "000009994"
                            });
                            mCnt = mCnt + 1;
                            LedPostList.Add(new CreditPurchaseVM()
                            {
                                Code = "000009994",
                                AccountName = ctxTFAT.Master.Where(x => x.Code == "000009994").Select(x => x.Name).FirstOrDefault(),
                                Debit = 0,
                                Credit = Math.Round(Model.TDSAmt, 2),
                                Branch = mbranchcode,
                                BranchName = ctxTFAT.TfatBranch.Where(X => X.Code == mbranchcode).Select(x => x.Name).FirstOrDefault(),
                                tempId = mCnt,
                                Narr = "",
                                AltCode = Model.BankCashCode
                            });
                            mCnt = mCnt + 1;
                        }


                    }
                }
            }

            Model.TotDebit = LedPostList.Sum(x => x.Debit);
            Model.TotCredit = LedPostList.Sum(x => x.Credit);
            Model.LedgerPostList = LedPostList;

            // display view form
            var html = ViewHelper.RenderPartialView(this, "LedgerPosting", new CreditPurchaseVM() { LedgerPostList = Model.LedgerPostList, TotDebit = Math.Round(Model.TotDebit, 2), TotCredit = Math.Round(Model.TotCredit, 2), Mode = Model.Mode, Controller2 = Model.Controller2, Type = Model.Type, Module = Model.Module, ViewDataId = Model.ViewDataId, TableName = Model.TableName, Header = Model.Header, OptionType = Model.OptionType, OptionCode = Model.OptionCode, MainType = Model.MainType, SubType = Model.SubType });
            return Json(new
            {
                LedgerPostList = Model.LedgerPostList,
                TotDebit = Math.Round(Model.TotDebit, 2),
                TotCredit = Math.Round(Model.TotCredit, 2),
                Mode = Model.Mode,
                Html = html
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public void DeUpdate(CreditPurchaseVM Model)
        {
            string connstring = GetConnectionString();
            var mobj2 = ctxTFAT.PurchaseMore.Where(x => x.TableKey == Model.ParentKey && x.Branch == Model.Branch).FirstOrDefault();
            if (mobj2 != null)
            {
                ctxTFAT.PurchaseMore.Remove(mobj2);
            }

            var mobj1 = ctxTFAT.Purchase.Where(x => x.TableKey == Model.ParentKey && x.Branch == Model.Branch).FirstOrDefault();
            if (mobj1 != null)
            {
                ctxTFAT.Purchase.Remove(mobj1);
            }
            var mobj11 = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).ToList();
            if (mobj11 != null)
            {
                ctxTFAT.Ledger.RemoveRange(mobj11);
            }

            var mobj12 = ctxTFAT.RelateData.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).ToList();
            if (mobj12 != null)
            {
                ctxTFAT.RelateData.RemoveRange(mobj12);
            }

            var mobj13 = ctxTFAT.RelLr.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).ToList();
            if (mobj13 != null)
            {
                ctxTFAT.RelLr.RemoveRange(mobj13);
            }

            var mobj14 = ctxTFAT.RelFm.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).ToList();
            if (mobj14 != null)
            {
                ctxTFAT.RelFm.RemoveRange(mobj14);
            }
            var result1 = (List<CreditPurchaseVM>)Session["Lothertrxlist"];
            foreach (var item in result1)
            {
                var ItemStock = ctxTFAT.ItemStock.Where(x => x.Parentkey == item.TableKey).FirstOrDefault();
                if (ItemStock != null)
                {
                    ctxTFAT.ItemStock.Remove(ItemStock);
                }

                var ItemStockRel = ctxTFAT.RelateDataItem.Where(x => x.TableKey == item.TableKey).FirstOrDefault();
                if (ItemStockRel != null)
                {
                    ctxTFAT.RelateDataItem.Remove(ItemStockRel);
                }
            }
            if (Model.Mode == "Delete")
            {
                List<string> tablekeylist = new List<string>();
                List<CreditPurchaseVM> result = new List<CreditPurchaseVM>();
                result = (List<CreditPurchaseVM>)Session["Lothertrxlist"];
                foreach (var item in result)
                {

                    var GetParentkey = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == item.TableKey && x.Branch == Model.Branch).Select(x => x.ParentKey).FirstOrDefault();

                    TyreStockSerial TSS = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == GetParentkey && x.StockAt == "Vehicle").OrderBy(x => x.RECORDKEY).Select(x => x).FirstOrDefault();
                    if (TSS != null)
                    {
                        TSS.IsActive = true;
                        ctxTFAT.Entry(TSS).State = EntityState.Modified;
                    }


                    var mobj134 = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == item.TableKey && x.Branch == Model.Branch).FirstOrDefault();
                    if (mobj134 != null)
                    {
                        ctxTFAT.TyreStockSerial.Remove(mobj134);
                    }
                    mobj134 = ctxTFAT.TyreStockSerial.Where(x => x.ParentKey == item.TableKey && x.Branch == Model.Branch).FirstOrDefault();
                    if (mobj134 != null)
                    {
                        ctxTFAT.TyreStockSerial.Remove(mobj134);
                    }
                }
            }

            var mobj15 = ctxTFAT.LR_FM_Expenses.Where(x => x.ParentKey == Model.ParentKey).ToList();
            if (mobj15 != null)
            {
                ctxTFAT.LR_FM_Expenses.RemoveRange(mobj15);
            }
            var mDeleteAuthorise = ctxTFAT.Authorisation.Where(x => x.ParentKey == Model.ParentKey).ToList();
            if (mDeleteAuthorise != null)
            {
                ctxTFAT.Authorisation.RemoveRange(mDeleteAuthorise);
            }
            var mAttachment = ctxTFAT.Attachment.Where(x => x.ParentKey == Model.ParentKey).ToList();
            if (mAttachment != null)
            {
                ctxTFAT.Attachment.RemoveRange(mAttachment);
            }
            var mtdspayment = ctxTFAT.TDSPayments.Where(x => x.ParentKey == Model.ParentKey).Select(x => x).FirstOrDefault();
            if (mtdspayment != null)
            {
                ctxTFAT.TDSPayments.Remove(mtdspayment);
            }

            var TripSheetmaster = ctxTFAT.TripSheetMaster.Where(x => x.ParentKey == Model.ParentKey).Select(x => x).FirstOrDefault();
            if (TripSheetmaster != null)
            {
                ctxTFAT.TripSheetMaster.Remove(TripSheetmaster);
            }

            var moutstanding = ctxTFAT.Outstanding.Where(x => x.ParentKey == Model.ParentKey && x.DocBranch == Model.Branch).Select(x => x).ToList();
            if (moutstanding != null)
            {
                ctxTFAT.Outstanding.RemoveRange(moutstanding);
            }
            ctxTFAT.SaveChanges();
        }

        [HttpPost]
        public ActionResult SaveData(CreditPurchaseVM Model)
        {
            string mTable = "";
            string brMessage = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction(System.Data.IsolationLevel.Unspecified))
            {
                try
                {
                    // REMOVE EXISTING DATA FOR EDIT && DELETE MODE
                    if (Model.Mode == "Edit")
                    {
                        if (mbranchcode != Model.Branch)
                        {
                            transaction.Rollback();
                            return Json(new { Message = "Login Branch Does Not Match With Documnet Branch.", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                        }
                        DeUpdate(Model);
                        Model.ParentKey = Model.ParentKey;

                    }
                    if (Model.Mode == "Add")
                    {
                        if (Model.OTGenerate == "M")
                        {
                            var result1 = ctxTFAT.DocTypes.Where(x => x.Code == "PUR00").Select(x => x).FirstOrDefault();
                            Model.Srl = Model.Srl.PadLeft(result1.DocWidth, '0');
                        }
                        else
                        {
                            Model.Srl = GetLastSerial(Model.TableName, Model.Branch, Model.Type, Model.Prefix, Model.SubType, ConvertDDMMYYTOYYMMDD(Model.DocuDate));
                        }
                        Model.ParentKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + Model.Srl.ToString();

                        AlertNoteMaster alertNoteMaster = ctxTFAT.AlertNoteMaster.Where(x => x.Type == "PUR00" && x.TypeCode.Trim() == Model.Srl.Trim() && x.Stop.Contains("PUR00")).FirstOrDefault();
                        if (alertNoteMaster != null)
                        {
                            return Json(new
                            {
                                Message = Model.Srl.Trim(),
                                Status = "AutoDocumentAlert"
                            }, JsonRequestBehavior.AllowGet);
                        }

                        string pkStr = "";
                        AttachmentVM vM = new AttachmentVM();
                        vM.ParentKey = Model.ParentKey;
                        vM.Srl = Model.Srl.ToString();
                        vM.Type = "PUR00";
                        SaveAttachment(vM);

                        SaveNarrationAdd(Model.Srl.ToString(), Model.ParentKey);
                    }
                    //if (Model.Authorise.Substring(0, 1) == "X" || Model.Authorise.Substring(0, 1) == "R" || Model.Mode == "Add" || (Model.Mode == "Edit" && Model.AuthAgain))
                    //{
                    //    if (Model.Authorise.Substring(0, 1) != "N") //if authorised then check for the RateDiff Auth. Rule
                    //        Model.Authorise = (Model.AuthReq == true ? GetAuthorise(Model.Type, 0, Model.Branch) : Model.Authorise = "A00");
                    //}

                    #region Authorisation
                    TfatUserAuditHeader authorisation = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == "PUR00").FirstOrDefault();
                    if (authorisation != null)
                    {
                        mauthorise = SetAuthorisationLogistics(authorisation, Model.ParentKey, Model.Srl.ToString(), 0, Model.DocuDate, Model.Amt, Model.BankCashCode, mbranchcode);
                    }
                    #endregion

                    string MergeNarr = "";
                    List<CreditPurchaseVM> result = new List<CreditPurchaseVM>();
                    result = (List<CreditPurchaseVM>)Session["Lothertrxlist"];

                    CreditPurchaseSetup setup = ctxTFAT.CreditPurchaseSetup.FirstOrDefault();
                    if (setup == null)
                    {
                        setup = new CreditPurchaseSetup();
                    }
                    if (Model.Mode == "Add" || Model.Mode == "Edit")
                    {
                        int xCnt = 1; // used for counts
                        int mFirstCount = 1;

                        #region Other Sales Menu

                        if (result != null)
                        {
                            var list = result.ToList();

                            //save data in table Quatetion
                            Purchase mobj1 = new Purchase();
                            mobj1.AccAmt = Model.Amt;//Calculate or Pass Parameter
                            mobj1.Account = Model.BankCashCode;
                            mobj1.AcHeadPerc = 0;
                            mobj1.AddTax = 0;
                            mobj1.AltAddress = 0;
                            mobj1.Amt = Model.Amt;//Calculate or Pass Parameter
                            mobj1.Amt1 = 0;
                            mobj1.Amt2 = 0;
                            mobj1.Amt3 = 0;
                            mobj1.Amt4 = 0;
                            mobj1.Amt5 = 0;
                            mobj1.Amt6 = 0;
                            mobj1.Amt7 = 0;
                            mobj1.Amt8 = 0;
                            mobj1.Amt9 = 0;
                            mobj1.Amt10 = 0;
                            mobj1.BillDate = (Model.BillDate);
                            mobj1.BillNumber = (Model.BillNumber == null || Model.BillNumber.Trim() == "") ? Model.Srl : Model.BillNumber;
                            mobj1.Branch = Model.Branch;
                            mobj1.Broker = "";
                            mobj1.Brokerage = 0;
                            mobj1.BrokerAmt = 0;
                            mobj1.BrokerOn = 0;
                            mobj1.Cess = 0;
                            mobj1.CGSTAmt = Convert.ToDecimal(Model.CGSTAmt);
                            mobj1.ChlnDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                            mobj1.Code = Model.BankCashCode;
                            mobj1.CompCode = mcompcode;
                            mobj1.CreatedOn = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                            mobj1.CrPeriod = Model.CrPeriod;
                            mobj1.CurrAmount = Model.Amt;
                            mobj1.CurrRate = 1;
                            mobj1.DCCode = 100001;
                            mobj1.DelyAltAdd = Convert.ToByte(0);
                            mobj1.DelyContact = 0;
                            mobj1.DelyDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                            mobj1.Disc = Convert.ToDecimal(0.00);
                            mobj1.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                            mobj1.EWBValidDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                            mobj1.ForceOrderS = false;
                            mobj1.GSTNoITC = 0;
                            mobj1.GSTType = (Model.GSTType == null) ? 0 : Convert.ToInt32(Model.GSTType);
                            mobj1.IGSTAmt = Convert.ToDecimal(Model.IGSTAmt);
                            mobj1.LocationCode = 0;
                            mobj1.MainType = Model.MainType;
                            mobj1.Narr = Model.Narr;
                            mobj1.PCCode = 100002;
                            mobj1.PlaceOfSupply = "Maharashtra";
                            mobj1.Prefix = Model.Prefix;
                            mobj1.Qty = Convert.ToDecimal(list.Select(x => x.Qty).Sum());
                            mobj1.Qty2 = Convert.ToDouble(list.Select(x => x.Qty).Sum());
                            mobj1.ReasonCode = 0;
                            mobj1.RoundOff = Model.RoundOff;//check sds
                            mobj1.SGSTAmt = Convert.ToDecimal(Model.SGSTAmt);
                            mobj1.Srl = Convert.ToInt32(Model.Srl);
                            mobj1.STAmt = 0;
                            mobj1.Staxable = 0;
                            mobj1.sTaxPaidBy = false;
                            mobj1.sTaxPay = false;
                            mobj1.STCess = 0;
                            mobj1.STSheCess = 0;
                            mobj1.SubType = Model.SubType;
                            mobj1.SurCharge = 0;
                            mobj1.TableKey = Model.ParentKey;
                            mobj1.Taxable = Convert.ToDecimal(Model.Taxable);
                            mobj1.TaxAmt = Model.IGSTAmt + Model.CGSTAmt + Model.SGSTAmt;
                            mobj1.TDSAble = 0;
                            mobj1.TDSAmt = Model.TDSAmt;
                            mobj1.TaxCode = Model.GSTCode;

                            mobj1.TDSCode = Model.TDSAmt > 0 ? ctxTFAT.TaxDetails.Where(x => x.Code == Model.BankCashCode).Select(x => x.TDSCode.Value).FirstOrDefault() : 0;
                            mobj1.TDSFlag = Model.TDSAmt > 0 ? true : false;

                            mobj1.TDSCess = 0;
                            mobj1.TDSSchg = 0;
                            mobj1.TDSTax = 0;
                            mobj1.Type = Model.Type;
                            mobj1.Val1 = 0;
                            mobj1.Val2 = 0;
                            mobj1.Val3 = 0;
                            mobj1.Val4 = 0;
                            mobj1.Val5 = 0;
                            mobj1.Val6 = 0;
                            mobj1.Val7 = 0;
                            mobj1.Val8 = 0;
                            mobj1.Val9 = 0;
                            mobj1.Val10 = 0;

                            mobj1.ENTEREDBY = muserid;
                            mobj1.AUTHIDS = muserid;
                            mobj1.AUTHORISE = mauthorise;
                            mobj1.LASTUPDATEDATE = DateTime.Now;

                            ctxTFAT.Purchase.Add(mobj1);






                            // sales more
                            PurchaseMore mobj2 = new PurchaseMore();
                            mobj2.Branch = mbranchcode;
                            mobj2.EWayBillRequired = false;
                            mobj2.LocationCode = 0;
                            mobj2.MainType = Model.MainType;
                            mobj2.Prefix = Model.Prefix;
                            mobj2.Srl = Model.Srl;
                            mobj2.SubType = Model.SubType;
                            mobj2.TableKey = Model.ParentKey;
                            mobj2.TDSFlag = false;
                            mobj2.Type = Model.Type;

                            mobj2.ENTEREDBY = muserid;
                            mobj2.AUTHIDS = muserid;
                            mobj2.AUTHORISE = mauthorise;
                            mobj2.LASTUPDATEDATE = DateTime.Now;

                            ctxTFAT.PurchaseMore.Add(mobj2);
                        }

                        #endregion
                        #region Other Transaction
                        if (result != null)
                        {
                            var list = result.ToList();
                            var mCodeSTR = ctxTFAT.Attachment.Max(X => X.Code);
                            var mcodeint = (string.IsNullOrEmpty(mCodeSTR) == true) ? 1 : Convert.ToInt32(mCodeSTR);
                            mfilecount = mcodeint;
                            xCnt = 1;

                            var GetLrID = ctxTFAT.RelLr.OrderByDescending(x => x.LrID).Select(x => x.LrID).FirstOrDefault();
                            int GetLrID1 = (Convert.ToInt32(GetLrID) + 1);

                            string CostCenter = "";
                            string ReferAccount = "";
                            string[] DefaultCost = new string[] { "000100949", "000100950", "000100811", "000100343", "000100736", "000100653", "000100326", "000100951", "000100952", "000100404", "000100781", "000100953", "000100954", "000100788", "000101135", "000100789", "000100346", "000100341", "000100345" };


                            List<string> TyreMasterAdd = new List<string>();
                            int NewCode1;
                            var NewCode = ctxTFAT.ItemMaster.OrderByDescending(x => x.Code).Select(x => x.Code).Take(1).FirstOrDefault();
                            if (NewCode == null || NewCode == "")
                            {
                                NewCode1 = 000001;
                            }
                            else
                            {
                                NewCode1 = Convert.ToInt32(NewCode) + 1;
                            }

                            var BranchLedgerList = ctxTFAT.Master.Where(x => x.RelatedTo.Trim() == "Branch").Select(x => x.Code).ToList();
                            var BTNONO = ctxTFAT.RelateData.Where(x => BranchLedgerList.Contains(x.Code) && x.AmtType == true && x.Char1.Trim() != "" && x.Char1.Trim() != null).OrderByDescending(x => x.RECORDKEY).Select(x => x.Char1).FirstOrDefault();
                            var NewBTNNO = 0;
                            if (String.IsNullOrEmpty(BTNONO))
                            {
                                NewBTNNO = 1;
                            }
                            else
                            {
                                NewBTNNO = (Convert.ToInt32(BTNONO) + 1);
                            }

                            foreach (var li in list)
                            {


                                GetLrID = GetLrID1.ToString();
                                if (GetLrID.Length > 6)
                                {
                                    GetLrID.PadLeft(6, '0');
                                }

                                if (DefaultCost.Contains(li.Code))
                                {
                                    CostCenter = li.Code;
                                    ReferAccount = li.RelatedChoice;
                                }
                                else
                                {
                                    CostCenter = li.RelatedTo;
                                    ReferAccount = (String.IsNullOrEmpty(li.RelatedChoice)) == true ? li.RelatedTo : li.RelatedChoice;
                                }

                                RelateData reldt = new RelateData();
                                reldt.Amount = li.Amt;
                                reldt.AUTHIDS = muserid;
                                reldt.AUTHORISE = mauthorise;
                                reldt.Branch = Model.Branch;
                                reldt.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                reldt.ENTEREDBY = muserid;
                                reldt.Deleted = false;
                                reldt.Type = Model.Type;
                                reldt.Srl = Convert.ToInt32(Model.Srl);
                                reldt.Sno = xCnt.ToString("D3");
                                reldt.SubType = Model.SubType;
                                reldt.LASTUPDATEDATE = DateTime.Now;
                                reldt.MainType = Model.MainType;
                                reldt.Code = li.Code;
                                if (!string.IsNullOrEmpty(li.Narr))
                                {
                                    MergeNarr += li.Narr + "\n";
                                }
                                reldt.Narr = li.Narr;
                                if (li.AmtType == "Payment")
                                {
                                    var Master = ctxTFAT.Master.Where(x => x.Code == li.Code).Select(x => x.RelatedTo).FirstOrDefault();
                                    if (Master == "Branch")
                                    {
                                        if (Model.Mode == "Add")
                                        {
                                            reldt.Char1 = NewBTNNO.ToString();
                                            ++NewBTNNO;
                                        }
                                        else
                                        {
                                            var Count = ctxTFAT.RelateData.Where(x => x.AmtType == true && x.Char1.Trim() == li.BTNNO.ToString().Trim()).ToList().Count;
                                            if (Count == 0)
                                            {
                                                reldt.Char1 = NewBTNNO.ToString();
                                                ++NewBTNNO;
                                            }
                                            else if (Count == 1)
                                            {
                                                reldt.Char1 = li.BTNNO.ToString();
                                            }
                                            else
                                            {
                                                reldt.Char1 = NewBTNNO.ToString();
                                                ++NewBTNNO;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (!String.IsNullOrEmpty(li.BTNNOCombo))
                                    {
                                        reldt.Char1 = li.BTNNOCombo.Split(',')[0].ToString();
                                    }
                                }

                                var RelatedTo = ctxTFAT.Master.Where(x => x.Code.Trim() == li.Code.Trim()).Select(x => x.RelatedTo).FirstOrDefault();
                                reldt.TableKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString();
                                reldt.ParentKey = Model.ParentKey;
                                //reldt.RelateTo = string.IsNullOrEmpty(RelatedTo) == true ? (byte)(0) : Convert.ToByte(RelatedTo);
                                reldt.RelateTo = (byte)(0);
                                reldt.ItemReq = false;
                                RelateDataItem relateDataItem = new RelateDataItem();
                                if (li.ItemList != null)
                                {
                                    if (li.ItemList.Count() > 0)
                                    {
                                        if (li.RelatedTo == "000100345" || li.Code == "000100345")
                                        {
                                            reldt.Char1 = li.ItemList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Value1 = li.ItemList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Value2 = li.ItemList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                                            var mdecnum1 = li.ItemList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Num1 = (string.IsNullOrEmpty(mdecnum1) == true) ? 0 : Convert.ToDecimal(mdecnum1);
                                            var mdecnum4 = li.ItemList.Where(x => x.Fld == "F007").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Num4 = (string.IsNullOrEmpty(mdecnum4) == true) ? 0 : Convert.ToDecimal(mdecnum4);
                                            reldt.Value3 = li.ItemList.Where(x => x.Fld == "F010").Select(x => x.ApplCode).FirstOrDefault();
                                            var mDATE = li.ItemList.Where(x => x.Fld == "F011").Select(x => x.ApplCode).FirstOrDefault();
                                            if (!string.IsNullOrEmpty(mDATE))
                                            {
                                                if (mDATE.ToString().Trim() != "0001-01-01")
                                                {
                                                    reldt.Date1 = Convert.ToDateTime(mDATE);
                                                }
                                            }
                                            //reldt.Date1 = (string.IsNullOrEmpty(mDATE) == true) ? Convert.ToDateTime("1900-01-01") : Convert.ToDateTime(mDATE);
                                            var mDATE2 = li.ItemList.Where(x => x.Fld == "F013").Select(x => x.ApplCode).FirstOrDefault();
                                            if (String.IsNullOrEmpty(reldt.Value3))
                                            {

                                                reldt.Date2 = null;
                                            }
                                            else
                                            {
                                                if (Convert.ToInt32(reldt.Value3) > 0)
                                                {
                                                    //reldt.Date2 = (string.IsNullOrEmpty(mDATE2) == true) ? Convert.ToDateTime("1900-01-01") : Convert.ToDateTime(mDATE2);
                                                    if (!string.IsNullOrEmpty(mDATE2))
                                                    {
                                                        if (mDATE2.ToString().Trim() != "0001-01-01" && mDATE2.ToString().Trim() != "1900-01-01")
                                                        {
                                                            reldt.Date2 = Convert.ToDateTime(mDATE2);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    reldt.Date2 = null;
                                                }
                                            }
                                            var mdecnum2 = li.ItemList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Num2 = (string.IsNullOrEmpty(mdecnum2) == true) ? 0 : Convert.ToDecimal(mdecnum2);
                                            var mDATE3 = li.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                                            if (!string.IsNullOrEmpty(mDATE3))
                                            {
                                                if (mDATE3.ToString().Trim() != "0001-01-01")
                                                {
                                                    reldt.Date3 = Convert.ToDateTime(mDATE3);
                                                }
                                            }
                                            //reldt.Date3 = (string.IsNullOrEmpty(mDATE3) == true) ? Convert.ToDateTime("1900-01-01") : Convert.ToDateTime(mDATE3);
                                            var mdecnum3 = li.ItemList.Where(x => x.Fld == "F009").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Num3 = (string.IsNullOrEmpty(mdecnum3) == true) ? 0 : Convert.ToDecimal(mdecnum3);
                                        }
                                        else if (li.RelatedTo == "000100346" || li.Code == "000100346")
                                        {
                                            reldt.Char1 = li.ItemList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Char2 = li.ItemList.Where(x => x.Fld == "F005").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Value1 = li.ItemList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Value2 = li.ItemList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();

                                            var mdecnum1 = li.ItemList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Num1 = (string.IsNullOrEmpty(mdecnum1) == true) ? 0 : Convert.ToDecimal(mdecnum1);

                                            reldt.Value3 = li.ItemList.Where(x => x.Fld == "F007").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Value4 = li.ItemList.Where(x => x.Fld == "F010").Select(x => x.ApplCode).FirstOrDefault();

                                            var mDATE2 = li.ItemList.Where(x => x.Fld == "F013").Select(x => x.ApplCode).FirstOrDefault();
                                            if (String.IsNullOrEmpty(reldt.Value4))
                                            {

                                                reldt.Date2 = null;
                                            }
                                            else
                                            {
                                                if (Convert.ToInt32(reldt.Value4) > 0)
                                                {
                                                    //reldt.Date2 = (string.IsNullOrEmpty(mDATE2) == true) ? Convert.ToDateTime("1900-01-01") : Convert.ToDateTime(mDATE2);
                                                    if (!string.IsNullOrEmpty(mDATE2))
                                                    {
                                                        if (mDATE2.ToString().Trim() != "0001-01-01" && mDATE2.ToString().Trim() != "1900-01-01")
                                                        {
                                                            reldt.Date2 = Convert.ToDateTime(mDATE2);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    reldt.Date2 = null;
                                                }
                                            }

                                            var mdecnum2 = li.ItemList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Num2 = (string.IsNullOrEmpty(mdecnum2) == true) ? 0 : Convert.ToDecimal(mdecnum2);

                                            var mDATE3 = li.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                                            if (!string.IsNullOrEmpty(mDATE3))
                                            {
                                                if (mDATE3.ToString().Trim() != "0001-01-01")
                                                {
                                                    reldt.Date3 = Convert.ToDateTime(mDATE3);
                                                }
                                            }
                                            //reldt.Date3 = (string.IsNullOrEmpty(mDATE3) == true) ? Convert.ToDateTime("1900-01-01") : Convert.ToDateTime(mDATE3);

                                            var vad = li.ItemList.Where(x => x.Fld == "F009").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Num3 = (string.IsNullOrEmpty(vad) == true) ? 0 : Convert.ToDecimal(vad);

                                        }
                                        else if (li.RelatedTo == "000100343" || li.Code == "000100343")
                                        {
                                            var km = li.ItemList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Num1 = (string.IsNullOrEmpty(km) == true) ? 0 : Convert.ToDecimal(km);
                                            var mDATE = li.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                                            //reldt.Date1 = (string.IsNullOrEmpty(mDATE) == true) ? Convert.ToDateTime("1900-01-01") : Convert.ToDateTime(mDATE);
                                            if (!string.IsNullOrEmpty(mDATE))
                                            {
                                                if (mDATE.ToString().Trim() != "0001-01-01")
                                                {
                                                    reldt.Date1 = Convert.ToDateTime(mDATE);
                                                }
                                            }
                                            var mdecnum = li.ItemList.Where(x => x.Fld == "F006").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Num2 = (string.IsNullOrEmpty(mdecnum) == true) ? 0 : Convert.ToDecimal(mdecnum);
                                        }
                                        relateDataItem.ProductGroup = li.ItemList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                                        relateDataItem.Item = li.ItemList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                                        relateDataItem.Cost = Convert.ToDecimal(li.ItemList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault());
                                        relateDataItem.Qty = li.ItemList.Where(x => x.Fld == "F005").Select(x => x.ApplCode).FirstOrDefault();
                                        relateDataItem.TotalAmout = Convert.ToDecimal(li.ItemList.Where(x => x.Fld == "F006").Select(x => x.ApplCode).FirstOrDefault());
                                        relateDataItem.WarrantyKm = li.ItemList.Where(x => x.Fld == "F007").Select(x => x.ApplCode).FirstOrDefault();
                                        relateDataItem.CurrentKM = li.ItemList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault();
                                        relateDataItem.DueKM = li.ItemList.Where(x => x.Fld == "F009").Select(x => x.ApplCode).FirstOrDefault();
                                        relateDataItem.WarrantyDays = li.ItemList.Where(x => x.Fld == "F010").Select(x => x.ApplCode).FirstOrDefault();
                                        relateDataItem.MFGDate = li.ItemList.Where(x => x.Fld == "F011").Select(x => x.ApplCode).FirstOrDefault();
                                        relateDataItem.InstallDate = li.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                                        relateDataItem.DueDate = li.ItemList.Where(x => x.Fld == "F013").Select(x => x.ApplCode).FirstOrDefault();
                                        relateDataItem.HSNCode = li.HSNCODE;
                                        relateDataItem.Description = li.ItemList.Where(x => x.Fld == "F015").Select(x => x.ApplCode).FirstOrDefault();
                                        relateDataItem.TableKey = reldt.TableKey;
                                        relateDataItem.Parentkey = reldt.ParentKey;
                                        relateDataItem.AUTHIDS = muserid;
                                        relateDataItem.AUTHORISE = mauthorise;
                                        relateDataItem.ENTEREDBY = muserid;
                                        relateDataItem.LASTUPDATEDATE = DateTime.Now;
                                        ctxTFAT.RelateDataItem.Add(relateDataItem);
                                        reldt.ItemReq = true;
                                        if (li.ItemList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault() == "Direct")
                                        {
                                            ItemMaster tyreMaster = ctxTFAT.ItemMaster.Where(x => x.Name == relateDataItem.Item).FirstOrDefault();
                                            if (tyreMaster == null)
                                            {
                                                if (!(TyreMasterAdd.Contains(relateDataItem.Item)))
                                                {
                                                    tyreMaster = new ItemMaster();
                                                    tyreMaster.Name = relateDataItem.Item;
                                                    tyreMaster.BaseGr = relateDataItem.ProductGroup;
                                                    tyreMaster.Rate = Convert.ToDouble(relateDataItem.Cost);
                                                    tyreMaster.Posting = "";
                                                    tyreMaster.GSTCode = reldt.GSTCode;
                                                    tyreMaster.HSNCode = li.HSNCODE;
                                                    tyreMaster.StockMaintain = true;
                                                    tyreMaster.ExpiryDays = Convert.ToInt32(relateDataItem.WarrantyDays);
                                                    tyreMaster.ExpiryKm = Convert.ToInt32(relateDataItem.WarrantyKm);
                                                    tyreMaster.Active = true;
                                                    tyreMaster.Narr = "";
                                                    tyreMaster.AppBranch = mbranchcode;

                                                    tyreMaster.AUTHIDS = muserid;
                                                    tyreMaster.AUTHORISE = "A00";
                                                    tyreMaster.ENTEREDBY = muserid;
                                                    tyreMaster.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                                                    string FinalCode = NewCode1.ToString("D6");
                                                    tyreMaster.Code = FinalCode;
                                                    reldt.Value2 = FinalCode;
                                                    relateDataItem.Item = FinalCode;
                                                    ctxTFAT.ItemMaster.Add(tyreMaster);
                                                    TyreMasterAdd.Add(reldt.Value2);
                                                    ++NewCode1;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (li.AddOnList != null)
                                {
                                    if (li.AddOnList.Count() > 0)
                                    {
                                        //Whos Have Date1 && Num1 Only   //Accident, Body Building,Principal Amount
                                        if (li.RelatedTo == "000100949" || li.Code == "000100949" || li.RelatedTo == "000100950" || li.Code == "000100950" || li.RelatedTo == "000100954" || li.Code == "000100954")
                                        {
                                            var mDATE = li.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                                            //reldt.Date1 = (string.IsNullOrEmpty(mDATE) == true) ? Convert.ToDateTime("1900-01-01") : Convert.ToDateTime(mDATE);
                                            if (!string.IsNullOrEmpty(mDATE))
                                            {
                                                if (mDATE.ToString().Trim() != "0001-01-01")
                                                {
                                                    reldt.Date1 = Convert.ToDateTime(mDATE);
                                                }
                                            }
                                            var mdecnum = li.AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Num1 = (string.IsNullOrEmpty(mdecnum) == true) ? 0 : Convert.ToDecimal(mdecnum);
                                        }
                                        //Whos Have Num1 Only  //Other Charges,Miscellaneous Charges
                                        else if (li.RelatedTo == "000100404" || li.Code == "000100404" || li.RelatedTo == "000100952" || li.Code == "000100952")
                                        {
                                            var mdecnum = li.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Num1 = (string.IsNullOrEmpty(mdecnum) == true) ? 0 : Convert.ToDecimal(mdecnum);
                                        }
                                        //Whos Have Date1 && Date2 Only  //Loan Expense,Permit (1 Year),Insurance,Permit 5 Year,Fitness,Puc,Pt,Green Tax,T P (Transit Pass)
                                        else if (li.RelatedTo == "000100951" || li.Code == "000100951" || li.RelatedTo == "000100781" || li.Code == "000100781" || li.RelatedTo == "000100326" || li.Code == "000100326" || li.RelatedTo == "000100953" || li.Code == "000100953" || li.RelatedTo == "000100736" || li.Code == "000100736" || li.RelatedTo == "000100789" || li.Code == "000100789" || li.RelatedTo == "000100788" || li.Code == "000100788" || li.RelatedTo == "000100653" || li.Code == "000100653" || li.RelatedTo == "000100811" || li.Code == "000100811" || li.RelatedTo == "000101135" || li.Code == "000101135")
                                        {
                                            var mDATE = li.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                                            //reldt.Date1 = (string.IsNullOrEmpty(mDATE) == true) ? Convert.ToDateTime("1900-01-01") : Convert.ToDateTime(mDATE);
                                            if (!string.IsNullOrEmpty(mDATE))
                                            {
                                                if (mDATE.ToString().Trim() != "0001-01-01")
                                                {
                                                    reldt.Date1 = Convert.ToDateTime(mDATE);
                                                }
                                            }
                                            var mDATE2 = li.AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault();
                                            //reldt.Date2 = (string.IsNullOrEmpty(mDATE2) == true) ? Convert.ToDateTime("1900-01-01") : Convert.ToDateTime(mDATE2);
                                            if (!string.IsNullOrEmpty(mDATE2))
                                            {
                                                if (mDATE2.ToString().Trim() != "0001-01-01" && mDATE2.ToString().Trim() != "1900-01-01")
                                                {
                                                    reldt.Date2 = Convert.ToDateTime(mDATE2);
                                                }
                                            }


                                        }
                                        //Specially For Diesel Expenses Save ( Value1,Value2,Num1,Date1,Num2  )
                                        else if (li.RelatedTo == "000100343" || li.Code == "000100343")
                                        {
                                            reldt.Value1 = li.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Value2 = li.AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Value3 = li.AddOnList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Value4 = li.AddOnList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault();
                                        }
                                        //Specially For Spare Parts Expenses Save ( Value1,Value2,Num1,Value3,Value4,Value5,Date1,Date2,Num2,Date3,Num3  )
                                        else if (li.RelatedTo == "000100346" || li.Code == "000100346")
                                        {

                                            reldt.Value5 = li.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();

                                            var mDATE = li.AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault();
                                            //reldt.Date1 = (string.IsNullOrEmpty(mDATE) == true) ? Convert.ToDateTime("1900-01-01") : Convert.ToDateTime(mDATE);
                                            if (!string.IsNullOrEmpty(mDATE))
                                            {
                                                if (mDATE.ToString().Trim() != "0001-01-01")
                                                {
                                                    reldt.Date1 = Convert.ToDateTime(mDATE);
                                                }
                                            }
                                        }
                                        //Specially For Trip Expenses Save ( Date1,Date2,Num1,Value1,Value2,Num2,Num3,Num4  )
                                        else if (li.RelatedTo == "000100341" || li.Code == "000100341")
                                        {
                                            var mDATE = li.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                                            //reldt.Date1 = (string.IsNullOrEmpty(mDATE) == true) ? Convert.ToDateTime("1900-01-01") : Convert.ToDateTime(mDATE);
                                            if (!string.IsNullOrEmpty(mDATE))
                                            {
                                                if (mDATE.ToString().Trim() != "0001-01-01")
                                                {
                                                    reldt.Date1 = Convert.ToDateTime(mDATE);
                                                }
                                            }
                                            var mDATE2 = li.AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault();
                                            //reldt.Date2 = (string.IsNullOrEmpty(mDATE2) == true) ? Convert.ToDateTime("1900-01-01") : Convert.ToDateTime(mDATE2);
                                            if (!string.IsNullOrEmpty(mDATE2))
                                            {
                                                if (mDATE2.ToString().Trim() != "0001-01-01" && mDATE2.ToString().Trim() != "1900-01-01")
                                                {
                                                    reldt.Date2 = Convert.ToDateTime(mDATE2);
                                                }
                                            }
                                            var mdecnum = li.AddOnList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Num1 = (string.IsNullOrEmpty(mdecnum) == true) ? 0 : Convert.ToDecimal(mdecnum);

                                            reldt.Value1 = li.AddOnList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Value2 = li.AddOnList.Where(x => x.Fld == "F005").Select(x => x.ApplCode).FirstOrDefault();

                                            var mdecnum2 = li.AddOnList.Where(x => x.Fld == "F006").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Num2 = (string.IsNullOrEmpty(mdecnum2) == true) ? 0 : Convert.ToDecimal(mdecnum2);
                                            var mdecnum3 = li.AddOnList.Where(x => x.Fld == "F007").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Num3 = (string.IsNullOrEmpty(mdecnum3) == true) ? 0 : Convert.ToDecimal(mdecnum3);
                                            var mdecnum4 = li.AddOnList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Num4 = (string.IsNullOrEmpty(mdecnum4) == true) ? 0 : Convert.ToDecimal(mdecnum4);

                                            #region Save TripSheetMaster

                                            TripSheetMaster tripExpensesMaster = new TripSheetMaster();
                                            var Driver = "";
                                            //Find Driver 
                                            var GetOtherPostAccount = ctxTFAT.Master.Where(x => x.Code == li.Code).Select(x => x.OthPostType).FirstOrDefault();
                                            if (GetOtherPostAccount.Contains("D"))
                                            {
                                                Driver = li.Code;
                                            }
                                            if (String.IsNullOrEmpty(Driver))
                                            {
                                                GetOtherPostAccount = ctxTFAT.Master.Where(x => x.Code == li.RelatedChoice).Select(x => x.OthPostType).FirstOrDefault();
                                                if (GetOtherPostAccount.Contains("D"))
                                                {
                                                    Driver = li.RelatedChoice;
                                                }
                                            }
                                            // End

                                            tripExpensesMaster.DocNo = GetTripNewCode();
                                            tripExpensesMaster.EntryDate = DateTime.Now;
                                            tripExpensesMaster.Prefix = mperiod;
                                            tripExpensesMaster.ParentKey = Model.ParentKey;
                                            tripExpensesMaster.TableKey = "Trip0" + mperiod.Substring(0, 2) + 1.ToString("D3") + tripExpensesMaster.DocNo;
                                            tripExpensesMaster.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                            tripExpensesMaster.Branch = mbranchcode;
                                            tripExpensesMaster.Driver = ctxTFAT.DriverMaster.Where(x => x.Posting == Driver).Select(x => x.Code).FirstOrDefault();
                                            tripExpensesMaster.DebitAc = "000100341";
                                            tripExpensesMaster.TDSAmt = 0;
                                            tripExpensesMaster.NetAmt = li.Amt;
                                            tripExpensesMaster.Narr = li.Narr;
                                            tripExpensesMaster.FromKM = reldt.Value1;
                                            tripExpensesMaster.ToKM = reldt.Value2;
                                            tripExpensesMaster.PerKMChrg = reldt.Num3;
                                            tripExpensesMaster.FromDT = ConvertDDMMYYTOYYMMDD(reldt.Date1.Value.ToShortDateString());
                                            tripExpensesMaster.TODT = ConvertDDMMYYTOYYMMDD(reldt.Date2.Value.ToShortDateString());
                                            tripExpensesMaster.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                                            tripExpensesMaster.ENTEREDBY = muserid;
                                            tripExpensesMaster.AUTHORISE = mauthorise;
                                            tripExpensesMaster.AUTHIDS = muserid;
                                            ctxTFAT.TripSheetMaster.Add(tripExpensesMaster);
                                            #endregion
                                        }
                                        //Specially For Tyres Expenses Save ( Value1,Value2 Num1,Num4,Value3,Value4,Value5,Value6,Date1,Date2,Num2,Date3,Num3  )
                                        else if (li.RelatedTo == "000100345" || li.Code == "000100345")
                                        {
                                            reldt.Value4 = li.AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Value5 = li.AddOnList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Value6 = li.AddOnList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault();
                                            reldt.Value7 = li.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                                        }
                                    }
                                }
                                reldt.Value8 = ReferAccount;//Vehicle NO
                                reldt.Combo1 = CostCenter;//OTher Cost Account

                                reldt.AmtType = (li.AmtType == "Payment") ? true : false;
                                reldt.ReqRelated = true;
                                reldt.Status = false;
                                reldt.Clear = false;
                                reldt.GSTFlag = li.GSTFlag;
                                reldt.GSTCode = li.GSTCode;
                                reldt.IGSTAmt = li.IGSTAmt;
                                reldt.CGSTAmt = li.CGSTAmt;
                                reldt.SGSTAmt = li.SGSTAmt;
                                reldt.IGSTRate = li.IGSTRate;
                                reldt.CGSTRate = li.CGSTRate;
                                reldt.SGSTRate = li.SGSTRate;
                                ctxTFAT.RelateData.Add(reldt);
                                int xrellrCnt = 1;
                                if (li.LRDetailList != null && li.LRDetailList.Count > 0)
                                {
                                    foreach (var l in li.LRDetailList)
                                    {
                                        RelLr rllr = new RelLr();
                                        rllr.AUTHIDS = muserid;
                                        rllr.AUTHORISE = mauthorise;
                                        rllr.Branch = Model.Branch;
                                        rllr.Deleted = false;
                                        rllr.ENTEREDBY = muserid;
                                        rllr.LASTUPDATEDATE = DateTime.Now;
                                        rllr.LrAmt = l.LRAmt;
                                        rllr.LrID = GetLrID;
                                        rllr.LrNo = l.LRNumber;
                                        rllr.TableKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString();
                                        rllr.SrNo = xrellrCnt;
                                        rllr.ParentKey = Model.ParentKey;
                                        rllr.LRRefTablekey = l.ConsignmentKey;
                                        rllr.Prefix = mperiod;
                                        ctxTFAT.RelLr.Add(rllr);

                                        xrellrCnt = xrellrCnt + 1;
                                    }
                                    ++GetLrID1;
                                }
                                if (li.FMDetailList != null && li.FMDetailList.Count > 0)
                                {
                                    foreach (var l in li.FMDetailList)
                                    {
                                        RelFm rllr = new RelFm();
                                        rllr.AUTHIDS = muserid;
                                        rllr.AUTHORISE = mauthorise;
                                        rllr.Branch = Model.Branch;
                                        rllr.ENTEREDBY = muserid;
                                        rllr.LASTUPDATEDATE = DateTime.Now;
                                        rllr.FmAmt = l.FMAmt;
                                        rllr.FMNo = l.FMNumber;

                                        rllr.TableKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString();
                                        rllr.SrNo = xrellrCnt;
                                        rllr.ParentKey = Model.ParentKey;
                                        rllr.FMRefTablekey = l.FreightMemoKey;
                                        rllr.Prefix = mperiod;
                                        ctxTFAT.RelFm.Add(rllr);



                                        xrellrCnt = xrellrCnt + 1;
                                    }
                                }
                                //if (setup.RelatedPosting || li.AccReqRelated)
                                {
                                    if (li.ItemList != null)
                                    {
                                        if (li.ItemList.Count() > 0)
                                        {
                                            bool MaintainStock = false;
                                            var PickFrom = li.ItemList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault();
                                            if (PickFrom == "Direct")
                                            {
                                                MaintainStock = true;
                                            }
                                            else if (PickFrom == "Master")
                                            {
                                                //var itemmaster = ctxTFAT.ItemMaster.Where(x => x.Code == (relateDataItem.Item ?? "").Trim())?.FirstOrDefault();

                                                var itemmaster = ctxTFAT.ItemMaster.Where(x => x.Code == relateDataItem.Item.Trim()).FirstOrDefault();
                                                if (itemmaster != null)
                                                {
                                                    MaintainStock = itemmaster.StockMaintain;
                                                }
                                            }
                                            if (MaintainStock)
                                            {
                                                if (li.RelatedTo == "000100345" || li.Code == "000100345")
                                                {
                                                    if (Model.Mode == "Add")
                                                    {
                                                        int mTy = 1;
                                                        string PreviousTablekeyOfStk = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + Model.Srl.ToString();
                                                        CreditPurchaseVM TyreStock = new CreditPurchaseVM();
                                                        if (li.TyreStockList != null && li.TyreStockList.Count > 0)
                                                        {
                                                            TyreStock = li.TyreStockList.FirstOrDefault();
                                                            TyreStockSerial TSS = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == TyreStock.TableKey).OrderBy(x => x.RECORDKEY).Select(x => x).FirstOrDefault();
                                                            TSS.IsActive = false;
                                                            ctxTFAT.Entry(TSS).State = EntityState.Modified;
                                                            PreviousTablekeyOfStk = TyreStock.TableKey;
                                                        }
                                                        if (li.AddOnList != null)
                                                        {
                                                            if (li.AddOnList.Count() > 0 && li.ItemList.Count() > 0)
                                                            {
                                                                #region New Stock Entry
                                                                var mInstDATE = li.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                                                                TyreStockSerial TSS2 = new TyreStockSerial();
                                                                TSS2.Branch = mbranchcode;
                                                                TSS2.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                                                TSS2.SerialNo = li.AddOnList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault() ?? "";
                                                                TSS2.Status = li.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                                                                TSS2.Value1 = li.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                                                                TSS2.Value2 = li.ItemList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault() == null ? 0 : Convert.ToDecimal(li.ItemList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault());
                                                                TSS2.Value3 = li.ItemList.Where(x => x.Fld == "F013").Select(x => x.ApplCode).FirstOrDefault();
                                                                TSS2.Value4 = li.ItemList.Where(x => x.Fld == "F009").Select(x => x.ApplCode).FirstOrDefault() == null ? 0 : Convert.ToDecimal(li.ItemList.Where(x => x.Fld == "F009").Select(x => x.ApplCode).FirstOrDefault());
                                                                TSS2.Vehicle = (DefaultCost.Contains(li.Code)) ? li.RelatedChoice : li.Code;
                                                                if (li.AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault() == "Tyre")
                                                                {
                                                                    TSS2.TyreNo = li.AddOnList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                                                                }
                                                                else
                                                                {
                                                                    TSS2.StepneeNo = li.AddOnList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                                                                }
                                                                TSS2.StockAt = (li.RelatedChoice == "Tyrestock" || li.RelatedChoice == "Remould") ? "Stock" : "Vehicle";
                                                                TSS2.TableKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString();
                                                                TSS2.ParentKey = PreviousTablekeyOfStk;
                                                                TSS2.Sno = mTy;
                                                                TSS2.ENTEREDBY = muserid;
                                                                TSS2.AUTHIDS = muserid;
                                                                TSS2.AUTHORISE = "A00";
                                                                TSS2.LASTUPDATEDATE = DateTime.Now;
                                                                TSS2.IsActive = true;
                                                                ctxTFAT.TyreStockSerial.Add(TSS2);
                                                                #endregion

                                                                #region Update New Stock
                                                                if (li.TyreStockList != null && li.TyreStockList.Count > 0)
                                                                {
                                                                    TyreStockSerial OTSS = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == TyreStock.TableKey).OrderBy(x => x.RECORDKEY).Select(x => x).FirstOrDefault();
                                                                    if (TyreStock != null)
                                                                    {
                                                                        TyreStockSerial TSS3 = new TyreStockSerial();
                                                                        TSS3.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                                                        TSS3.Branch = TSS2.Branch;
                                                                        TSS3.SerialNo = OTSS.SerialNo;
                                                                        TSS3.Status = TyreStock.ApplCode;
                                                                        TSS3.Value1 = OTSS.Value1;
                                                                        TSS3.Value2 = OTSS.Value2;
                                                                        TSS3.Value3 = OTSS.Value3;
                                                                        TSS3.Value4 = OTSS.Value4;
                                                                        TSS3.Vehicle = TSS2.Vehicle;
                                                                        TSS3.TyreNo = "";
                                                                        TSS3.StepneeNo = "";
                                                                        TSS3.StockAt = "Stock";
                                                                        TSS3.TableKey = TyreStock.TableKey;
                                                                        TSS3.ParentKey = TSS2.TableKey;
                                                                        TSS3.Sno = TSS2.Sno;
                                                                        TSS3.ENTEREDBY = muserid;
                                                                        TSS3.AUTHIDS = muserid;
                                                                        TSS3.AUTHORISE = "A00";
                                                                        TSS3.LASTUPDATEDATE = DateTime.Now;
                                                                        TSS3.IsActive = true;
                                                                        ctxTFAT.TyreStockSerial.Add(TSS3);
                                                                    }
                                                                }
                                                                #endregion
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        int mTy = 1;
                                                        string PreviousTablekeyOfStk = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + Model.Srl.ToString();
                                                        CreditPurchaseVM TyreStock = new CreditPurchaseVM();
                                                        if (li.TyreStockList != null && li.TyreStockList.Count > 0)
                                                        {
                                                            TyreStock = li.TyreStockList.FirstOrDefault();
                                                            TyreStockSerial TSS = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == TyreStock.TableKey).OrderBy(x => x.RECORDKEY).Select(x => x).FirstOrDefault();
                                                            TSS.IsActive = false;
                                                            ctxTFAT.Entry(TSS).State = EntityState.Modified;
                                                            PreviousTablekeyOfStk = TyreStock.TableKey;
                                                        }
                                                        var tablekey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString();
                                                        var mInstDATE = li.ItemList == null ? "" : li.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                                                        TyreStockSerial TSS2 = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == tablekey && x.Branch == Model.Branch).FirstOrDefault();
                                                        if (TSS2 != null && li.AddOnList.Count() > 0 && li.ItemList.Count() > 0)
                                                        {
                                                            TSS2.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                                            TSS2.SerialNo = li.AddOnList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault();
                                                            TSS2.Value1 = li.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                                                            TSS2.Value2 = li.ItemList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault() == null ? 0 : Convert.ToDecimal(li.ItemList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault());
                                                            TSS2.Value3 = li.ItemList.Where(x => x.Fld == "F013").Select(x => x.ApplCode).FirstOrDefault();
                                                            TSS2.Value4 = li.ItemList.Where(x => x.Fld == "F009").Select(x => x.ApplCode).FirstOrDefault() == null ? 0 : Convert.ToDecimal(li.ItemList.Where(x => x.Fld == "F009").Select(x => x.ApplCode).FirstOrDefault());
                                                            TSS2.Status = li.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                                                            TSS2.Vehicle = (DefaultCost.Contains(li.Code)) ? li.RelatedChoice : li.Code;
                                                            TSS2.ParentKey = PreviousTablekeyOfStk;
                                                            TSS2.Sno = mTy;
                                                            TSS2.ENTEREDBY = muserid;
                                                            TSS2.AUTHIDS = muserid;
                                                            TSS2.AUTHORISE = "A00";
                                                            TSS2.LASTUPDATEDATE = DateTime.Now;
                                                            //TSS2.IsActive = true;
                                                            ctxTFAT.Entry(TSS2).State = EntityState.Modified;
                                                        }
                                                        else
                                                        {
                                                            if (li.AddOnList != null && li.ItemList != null)
                                                            {
                                                                if (li.AddOnList.Count() > 0 && li.ItemList.Count() > 0)
                                                                {

                                                                    #region New Stock Entry
                                                                    mInstDATE = li.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                                                                    TSS2 = new TyreStockSerial();
                                                                    TSS2.Branch = mbranchcode;
                                                                    TSS2.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                                                    TSS2.SerialNo = li.AddOnList.Where(x => x.Fld == "F004").Select(x => x.ApplCode).FirstOrDefault() ?? "";
                                                                    TSS2.Status = li.AddOnList.Where(x => x.Fld == "F001").Select(x => x.ApplCode).FirstOrDefault();
                                                                    TSS2.Value1 = li.ItemList.Where(x => x.Fld == "F012").Select(x => x.ApplCode).FirstOrDefault();
                                                                    TSS2.Value2 = li.ItemList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault() == null ? 0 : Convert.ToDecimal(li.ItemList.Where(x => x.Fld == "F008").Select(x => x.ApplCode).FirstOrDefault());
                                                                    TSS2.Value3 = li.ItemList.Where(x => x.Fld == "F013").Select(x => x.ApplCode).FirstOrDefault();
                                                                    TSS2.Value4 = li.ItemList.Where(x => x.Fld == "F009").Select(x => x.ApplCode).FirstOrDefault() == null ? 0 : Convert.ToDecimal(li.ItemList.Where(x => x.Fld == "F009").Select(x => x.ApplCode).FirstOrDefault());
                                                                    TSS2.Vehicle = (DefaultCost.Contains(li.Code)) ? li.RelatedChoice : li.Code;
                                                                    if (li.AddOnList.Where(x => x.Fld == "F002").Select(x => x.ApplCode).FirstOrDefault() == "Tyre")
                                                                    {
                                                                        TSS2.TyreNo = li.AddOnList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                                                                    }
                                                                    else
                                                                    {
                                                                        TSS2.StepneeNo = li.AddOnList.Where(x => x.Fld == "F003").Select(x => x.ApplCode).FirstOrDefault();
                                                                    }
                                                                    TSS2.StockAt = (li.RelatedChoice == "Tyrestock" || li.RelatedChoice == "Remould") ? "Stock" : "Vehicle";
                                                                    TSS2.TableKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString();
                                                                    TSS2.ParentKey = PreviousTablekeyOfStk;
                                                                    TSS2.Sno = mTy;
                                                                    TSS2.ENTEREDBY = muserid;
                                                                    TSS2.AUTHIDS = muserid;
                                                                    TSS2.AUTHORISE = "A00";
                                                                    TSS2.LASTUPDATEDATE = DateTime.Now;
                                                                    TSS2.IsActive = true;
                                                                    ctxTFAT.TyreStockSerial.Add(TSS2);
                                                                    #endregion

                                                                    #region Update New Stock
                                                                    if (li.TyreStockList != null && li.TyreStockList.Count > 0)
                                                                    {
                                                                        if (TyreStock != null)
                                                                        {
                                                                            TyreStockSerial TSS3 = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == TyreStock.TableKey && x.ParentKey == TyreStock.ParentKey && x.Branch == TyreStock.Branch).Select(x => x).FirstOrDefault();
                                                                            TSS3.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                                                                            //TSS3.Branch = TSS2.Branch;
                                                                            //TSS3.SerialNo = TSS2.SerialNo;
                                                                            TSS3.Status = TyreStock.ApplCode;
                                                                            //TSS3.Value1 = TSS2.Value1;
                                                                            //TSS3.Value2 = TSS2.Value2;
                                                                            //TSS3.Value3 = TSS2.Value3;
                                                                            //TSS3.Value4 = TSS2.Value4;
                                                                            TSS3.Vehicle = TSS2.Vehicle;
                                                                            //TSS3.TyreNo = TSS2.TyreNo;
                                                                            //TSS3.StepneeNo = TSS2.StepneeNo;
                                                                            TSS3.Sno = TSS2.Sno;
                                                                            TSS3.ENTEREDBY = muserid;
                                                                            TSS3.AUTHIDS = muserid;
                                                                            TSS3.AUTHORISE = "A00";
                                                                            TSS3.LASTUPDATEDATE = DateTime.Now;
                                                                            TSS3.IsActive = true;
                                                                            ctxTFAT.Entry(TSS3).State = EntityState.Modified;
                                                                        }
                                                                    }
                                                                    #endregion
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                //Item Stock Maintain  
                                                if (li.ItemList != null && li.ItemList.Count() > 0)
                                                {
                                                    ItemStock itemStock = new ItemStock();
                                                    itemStock.ProductGroup = relateDataItem.ProductGroup;
                                                    itemStock.Name = relateDataItem.Item;
                                                    itemStock.Rate = Convert.ToDouble(relateDataItem.Cost);
                                                    itemStock.HSNCode = li.HSNCODE;
                                                    itemStock.Qty = Convert.ToInt32(relateDataItem.Qty);
                                                    itemStock.Parentkey = reldt.TableKey;
                                                    itemStock.Tablekey = mbranchcode + "ITEM0" + Model.Prefix.Substring(0, 2) + xCnt.ToString("D3") + Model.Srl.ToString(); ;
                                                    itemStock.AUTHIDS = muserid;
                                                    itemStock.AUTHORISE = "A00";
                                                    itemStock.ENTEREDBY = muserid;
                                                    itemStock.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                                                    ctxTFAT.ItemStock.Add(itemStock);
                                                }
                                            }
                                        }
                                    }

                                }
                                ++xCnt;
                            }
                        }
                        int lCnt = 1;
                        var ledpost = Model.LedgerPostList;
                        for (int u = 0; u < ledpost.Count; u++)
                        {
                            Ledger mobjL = new Ledger();
                            mobjL.AltCode = ledpost[u].AltCode;
                            mobjL.Audited = true;
                            mobjL.AUTHIDS = muserid;
                            mobjL.AUTHORISE = mauthorise;
                            mobjL.BillDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                            mobjL.BillNumber = (Model.BillNumber == null || Model.BillNumber.Trim() == "") ? Model.Srl : Model.BillNumber;
                            mobjL.Branch = Model.Branch;
                            mobjL.Cheque = Model.Cheque;
                            mobjL.ChequeReturn = false;
                            mobjL.ChqCategory = 1;
                            mobjL.ClearDate = DateTime.Now;
                            mobjL.Code = ledpost[u].Code;
                            mobjL.Credit = Convert.ToDecimal(ledpost[u].Credit);
                            mobjL.CrPeriod = Model.CrPeriod;
                            mobjL.CurrAmount = Convert.ToDecimal(ledpost[u].Credit + ledpost[u].Debit);
                            mobjL.CurrName = 1;
                            mobjL.CurrRate = 1;
                            mobjL.Debit = Convert.ToDecimal(ledpost[u].Debit);
                            mobjL.Discounted = true;
                            mobjL.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                            mobjL.DueDate = DateTime.Now;
                            mobjL.LocationCode = Model.LocationCode;
                            mobjL.MainType = Model.MainType;
                            mobjL.Narr = MergeNarr;
                            mobjL.Party = ledpost[u].Code;
                            mobjL.Prefix = Model.Prefix;
                            mobjL.RecoFlag = "";
                            mobjL.RefDoc = "";
                            mobjL.Reminder = true;
                            mobjL.Sno = ledpost[u].tempId;
                            mobjL.Srl = Model.Srl.ToString();
                            mobjL.SubType = Model.SubType;
                            mobjL.TaskID = 0;
                            mobjL.TDSChallanNumber = "";
                            mobjL.TDSCode = Model.TDSCode == null ? 0 : Convert.ToInt32(Model.TDSCode.ToString());
                            mobjL.TDSFlag = Model.TDSFlag;
                            mobjL.Type = Model.Type;
                            mobjL.ENTEREDBY = muserid;
                            mobjL.LASTUPDATEDATE = DateTime.Now;
                            mobjL.ChequeDate = DateTime.Now;
                            mobjL.CompCode = mcompcode;
                            mobjL.ParentKey = Model.ParentKey;
                            mobjL.TableKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + lCnt.ToString("D3") + Model.Srl.ToString(); ;
                            mobjL.PCCode = 100002;
                            mobjL.CGSTAmt = Model.CGSTAmt;
                            mobjL.CGSTRate = Model.CGSTRate;
                            mobjL.IGSTAmt = Model.IGSTAmt;
                            mobjL.IGSTRate = Model.IGSTRate;
                            mobjL.SGSTAmt = Model.SGSTAmt;
                            mobjL.SGSTRate = Model.SGSTRate;
                            mobjL.TaxCode = Model.GSTCode;
                            ctxTFAT.Ledger.Add(mobjL);
                            ++lCnt;
                        }

                        #endregion

                        //SaveNarration(Model, Model.ParentKey);
                        if (Model.TDSFlag == true && String.IsNullOrEmpty(Model.TDSCode) == false)
                        {
                            SaveTDSPayments(Model);
                        }

                        ctxTFAT.SaveChanges();
                        transaction.Commit();
                        transaction.Dispose();

                        UpdateAuditTrail(Model.Branch, Model.Mode, Model.Header, Model.ParentKey, ConvertDDMMYYTOYYMMDD(Model.DocuDate), Model.Amt, Model.BankCashCode, "Save Credit Purchase", "A");


                        //if (Model.Authorise.Substring(0, 1) != "A")
                        //{
                        //    if (Model.Authorise.Substring(0, 1) != "X")
                        //    {
                        //        string mAuthUser;
                        //        if (Model.Authorise.Substring(0, 1) == "D")
                        //        {
                        //            //mAuthUser=SaveAuthoriseDOC( Model.ParentKey, 0, Model.Date, mcurrency, Model.Branch, muserid);
                        //        }
                        //        else
                        //        {
                        //            mAuthUser = SaveAuthorise(Model.ParentKey, Model.Amt, Model.DocuDate, 1, 1, DateTime.Now, Model.Account, Model.Branch, muserid, -1);
                        //            SendAuthoriseMessage(mAuthUser, Model.ParentKey, Model.DocuDate, Model.Authorise, Model.AccountName);
                        //        }
                        //    }

                        //}


                        //if (Model.Authorise != "X00")
                        //{
                        //    SendTrnsMsg(Model.Mode, Model.Amt, Model.Branch + Model.ParentKey, ConvertDDMMYYTOYYMMDD(Model.DocuDate), Model.Account);
                        //}


                    }
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    return Json(new
                    {
                        Message = ex1.EntityValidationErrors.Select(x => x.ValidationErrors.Select(xa => xa.ErrorMessage).FirstOrDefault()),
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
                NewSrl = (Model.Branch + Model.ParentKey),
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
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
        public void SaveNarrationAdd(string DocNo, string Parentkey)
        {
            List<AlertNoteMaster> objledgerdetail = new List<AlertNoteMaster>();

            if (Session["CommnNarrlist"] != null)
            {
                objledgerdetail = (List<AlertNoteMaster>)Session["CommnNarrlist"];
            }
            int Sno = Convert.ToInt32(GetCode());
            foreach (var item in objledgerdetail)
            {
                item.DocNo = Sno.ToString("D6");
                item.TypeCode = DocNo;
                item.TableKey = "ALERT" + mperiod.Substring(0, 2) + 1.ToString("D3") + item.DocNo;
                item.ParentKey = Parentkey;
                ctxTFAT.AlertNoteMaster.Add(item);
                ++Sno;
            }
        }

        private void SaveTDSPayments(CreditPurchaseVM Model)
        {
            //var TDSCODE = ctxTFAT.TaxDetails.Where(x => x.Code == Model.BankCashCode).FirstOrDefault();
            var tdsrates = ctxTFAT.TDSRates.Where(x => x.Code.ToString() == Model.TDSCode).Select(x => new { x.Tax, x.Cess, x.SHECess, x.SurCharge, x.TDSRate }).FirstOrDefault();
            if (Model.TDSFlag == true && Model.TDSAmt > 0)
            {
                TDSPayments tspay = new TDSPayments();
                tspay.aMainType = Model.MainType;
                tspay.Amount = Model.Amt;
                tspay.aPrefix = Model.Prefix;
                tspay.aSno = 1;
                tspay.aSrl = Model.Srl.ToString(); ;
                tspay.SubType = Model.SubType;
                tspay.aSubType = Model.SubType;
                tspay.aType = Model.Type;
                tspay.BankCode = "";
                tspay.BillNumber = (Model.BillNumber == null || Model.BillNumber == "") ? "" : Model.BillNumber;
                tspay.Branch = mbranchcode;
                tspay.CertDate = DateTime.Now;
                tspay.CertNumber = "";
                tspay.ChallanDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                tspay.ChallanNumber = "";
                tspay.CNO = "";
                tspay.Code = Model.BankCashCode;
                tspay.CompCode = mcompcode;
                tspay.DepositSerial = "";
                tspay.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                tspay.DueDate = DateTime.Now.Date;
                tspay.EndCredit = false;

                tspay.LocationCode = Model.LocationCode;
                tspay.MainType = Model.MainType;
                tspay.Narr = "";
                tspay.PaidAmt = 0;
                tspay.ParentKey = Model.ParentKey;
                tspay.Party = Model.BankCashCode;
                tspay.PaymentMode = 0;
                tspay.Prefix = Model.Prefix;
                tspay.Sno = 1;
                tspay.Srl = Model.Srl.ToString(); ;
                tspay.SubType = Model.SubType;
                tspay.TableKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + 1.ToString("D3") + Model.Srl.ToString(); ;
                tspay.TDSAble = 0;
                tspay.TDSAmt = Model.TDSAmt;
                tspay.TDSCess = tdsrates == null ? 0 : tdsrates.Cess;
                tspay.TDSCessAmt = 0;
                tspay.TDSCode = Convert.ToInt32(Model.TDSCode);
                tspay.TDSReason = 0;
                tspay.TDSSheCess = tdsrates == null ? 0 : tdsrates.SHECess;
                tspay.TDSSheCessAmt = 0;
                tspay.TDSSurCharge = tdsrates == null ? 0 : tdsrates.SurCharge;
                tspay.TDSSurChargeAmt = 0;
                tspay.TDSTax = Model.TDSRate;
                tspay.TDSTaxAmt = 0;
                tspay.TotalTDSAmt = Model.TDSAmt;
                tspay.Type = Model.Type;
                tspay.ENTEREDBY = muserid;
                tspay.LASTUPDATEDATE = DateTime.Now;
                tspay.AUTHORISE = "A00";
                tspay.AUTHIDS = muserid;
                ctxTFAT.TDSPayments.Add(tspay);


                var mBalCntNoCred = Model.LedgerPostList.Where(x => x.Code == Model.BankCashCode && x.Credit == Model.TDSAmt).Select(x => x.tempId).FirstOrDefault();


                Outstanding osobj1 = new Outstanding();

                osobj1.Branch = mbranchcode;
                osobj1.DocBranch = mbranchcode;
                osobj1.MainType = Model.MainType;
                osobj1.SubType = Model.SubType;
                osobj1.Type = Model.Type;
                osobj1.Prefix = Model.Prefix;
                osobj1.Srl = Model.Srl;
                osobj1.Sno = 1;
                osobj1.ParentKey = Model.ParentKey;
                osobj1.TableKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + 1.ToString("D3") + Model.Srl;
                osobj1.aMaintype = Model.MainType;
                osobj1.aSubType = Model.SubType;
                osobj1.aType = Model.Type;
                osobj1.aPrefix = Model.Prefix;
                osobj1.aSrl = Model.Srl;
                osobj1.aSno = 2;
                osobj1.Amount = Model.TDSAmt;
                osobj1.TableRefKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + mBalCntNoCred.ToString("D3") + Model.Srl;
                osobj1.AUTHIDS = muserid;
                osobj1.AUTHORISE = "A00";
                osobj1.BillDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                osobj1.BillNumber = Model.BillNumber;
                osobj1.CompCode = mcompcode;
                osobj1.Broker = 100001;
                osobj1.Brokerage = Convert.ToDecimal(0.00);
                osobj1.BrokerAmt = Convert.ToDecimal(0.00);
                osobj1.BrokerOn = Convert.ToDecimal(0.00);
                osobj1.ChlnDate = DateTime.Now;
                osobj1.ChlnNumber = "";
                osobj1.Code = Model.BankCashCode;
                osobj1.CrPeriod = 0;
                osobj1.CurrName = 1;
                osobj1.CurrRate = 1;
                osobj1.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                osobj1.Narr = "";
                osobj1.OrdDate = DateTime.Now;
                osobj1.OrdNumber = "";
                osobj1.ProjCode = "";
                osobj1.ProjectStage = 0;
                osobj1.ProjectUnit = 0;
                osobj1.RefParty = "";
                osobj1.SalemanAmt = Convert.ToDecimal(0.00);
                osobj1.SalemanOn = Convert.ToDecimal(0.00);
                osobj1.SalemanPer = Convert.ToDecimal(0.00);
                osobj1.Salesman = 100001;
                osobj1.TDSAmt = Model.TDSAmt;
                osobj1.ENTEREDBY = muserid;
                osobj1.LASTUPDATEDATE = DateTime.Now;
                osobj1.CurrAmount = Model.TDSAmt;
                osobj1.ValueDate = DateTime.Now;
                osobj1.LocationCode = 100001;

                ctxTFAT.Outstanding.Add(osobj1);

                // second effect
                Outstanding osobj2 = new Outstanding();

                osobj2.Branch = mbranchcode;
                osobj2.DocBranch = mbranchcode;
                osobj2.ParentKey = Model.ParentKey;
                osobj2.Type = Model.Type;
                osobj2.Prefix = Model.Prefix;
                osobj2.Srl = Model.Srl;
                osobj2.Sno = 2;
                osobj2.TableKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + mBalCntNoCred.ToString("D3") + Model.Srl;
                osobj2.aType = Model.Type;
                osobj2.aPrefix = Model.Prefix;
                osobj2.aSrl = Model.Srl;
                osobj2.aSno = 1;
                osobj2.aMaintype = Model.MainType;
                osobj2.TableRefKey = mbranchcode + Model.Type + Model.Prefix.Substring(0, 2) + 1.ToString("D3") + Model.Srl;
                osobj2.MainType = Model.MainType;
                osobj2.SubType = Model.SubType;
                osobj2.aSubType = Model.SubType;
                osobj2.Amount = Model.TDSAmt;
                osobj2.AUTHIDS = muserid;
                osobj2.AUTHORISE = "A00";
                osobj2.BillDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                osobj2.BillNumber = "";
                osobj2.CompCode = mcompcode;
                osobj2.Broker = 100001;
                osobj2.Brokerage = Convert.ToDecimal(0.00);
                osobj2.BrokerAmt = Convert.ToDecimal(0.00);
                osobj2.BrokerOn = Convert.ToDecimal(0.00);
                osobj2.ChlnDate = DateTime.Now;
                osobj2.ChlnNumber = Model.BillNumber;
                osobj2.Code = Model.BankCashCode;
                osobj2.CrPeriod = 0;
                osobj2.CurrName = 1;
                osobj2.CurrRate = 1;
                osobj2.DocDate = ConvertDDMMYYTOYYMMDD(Model.DocuDate);
                osobj2.Narr = "";
                osobj2.OrdDate = DateTime.Now;
                osobj2.OrdNumber = "";
                osobj2.ProjCode = "";
                osobj2.ProjectStage = 0;
                osobj2.ProjectUnit = 0;
                osobj2.RefParty = "";
                osobj2.SalemanAmt = Convert.ToDecimal(0.00);
                osobj2.SalemanOn = Convert.ToDecimal(0.00);
                osobj2.SalemanPer = Convert.ToDecimal(0.00);
                osobj2.Salesman = 100001;
                osobj2.TDSAmt = Model.TDSAmt;
                osobj2.ENTEREDBY = muserid;
                osobj2.LASTUPDATEDATE = DateTime.Now;
                osobj2.CurrAmount = Model.TDSAmt;
                osobj2.ValueDate = DateTime.Now;
                osobj2.LocationCode = 100001;

                ctxTFAT.Outstanding.Add(osobj2);


            }

        }

        public ActionResult DeleteData(CreditPurchaseVM Model)
        {
            var Date = ConvertDDMMYYTOYYMMDD(Model.DocuDate);

            if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.Type == "PUR00" && x.LockDate == Date).FirstOrDefault() != null)
            {
                return Json(new
                {

                    Status = "Error",
                    Message = "Date is Locked By Period Lock System...!"
                }, JsonRequestBehavior.AllowGet); ;
            }

            var mobjk = ctxTFAT.Ledger.Where(x => x.ParentKey == Model.ParentKey && x.Branch == Model.Branch).Select(x => x.Branch + x.TableKey).ToList();
            if (mobjk != null)
            {
                string IntoSingleLine = "";
                IntoSingleLine = String.Join(",", mobjk);
                //foreach (var item in mobjk)
                //{
                //    IntoSingleLine += "" + item + ",";
                //}
                if (!String.IsNullOrEmpty(IntoSingleLine))
                {
                    //IntoSingleLine = IntoSingleLine.Substring(0, IntoSingleLine.Length - 1);
                    string mSQLQuery = "with Demo as( SELECT  value  FROM TripSheetMaster CROSS APPLY STRING_SPLIT(AdjustLedgerRef, '^') where len(AdjustLedgerRef)>5) select count(*) from Demo where charindex( Value,'" + IntoSingleLine + "')<>0";
                    var connstring = GetConnectionString();
                    SqlConnection conn = new SqlConnection(connstring);
                    SqlCommand cmd = new SqlCommand(mSQLQuery, conn);
                    try
                    {
                        conn.Open();
                        cmd.CommandTimeout = 0;
                        Int32 CustomerCnt = (Int32)cmd.ExecuteScalar();
                        if (CustomerCnt > 0)
                        {
                            return Json(new
                            {
                                Status = "Error",
                                Message = "This Document Locked Because Tripsheet Adjust Against This Document...!"
                            }, JsonRequestBehavior.AllowGet); ;
                        }
                        else
                        {
                            mSQLQuery = "with Demo as( SELECT  value  FROM TripSheetMaster CROSS APPLY STRING_SPLIT(CCjustLedgerRef, '^') where len(CCjustLedgerRef)>5) select count(*) from Demo where charindex( Value,'" + IntoSingleLine + "')<>0";
                            conn = new SqlConnection(connstring);
                            cmd = new SqlCommand(mSQLQuery, conn);
                            try
                            {
                                conn.Open();
                                cmd.CommandTimeout = 0;
                                CustomerCnt = (Int32)cmd.ExecuteScalar();
                                if (CustomerCnt > 0)
                                {
                                    return Json(new
                                    {
                                        Status = "Error",
                                        Message = "This Document Locked Because Tripsheet Adjust Against This Document...!"
                                    }, JsonRequestBehavior.AllowGet); ;
                                }
                                else
                                {
                                    mSQLQuery = "with Demo as( SELECT  value  FROM TripSheetMaster CROSS APPLY STRING_SPLIT(AdjustBalLedgerRef, '^') where len(AdjustBalLedgerRef)>5) select count(*) from Demo where charindex( Value,'" + IntoSingleLine + "')<>0";
                                    conn = new SqlConnection(connstring);
                                    cmd = new SqlCommand(mSQLQuery, conn);
                                    try
                                    {
                                        conn.Open();
                                        cmd.CommandTimeout = 0;
                                        CustomerCnt = (Int32)cmd.ExecuteScalar();
                                        if (CustomerCnt > 0)
                                        {
                                            Model.LockAdjustTrip = true;
                                            Model.LockAdjustTripMessage = "This Document Adjust In TripSheet.. So U Cant Edit/Delete...!<br>";
                                        }
                                        else
                                        {

                                        }
                                    }
                                    catch (Exception mex)
                                    {
                                    }
                                    finally
                                    {
                                        cmd.Dispose();
                                        conn.Close();
                                        conn.Dispose();
                                    }
                                }
                            }
                            catch (Exception mex)
                            {
                            }
                            finally
                            {
                                cmd.Dispose();
                                conn.Close();
                                conn.Dispose();
                            }
                        }
                    }
                    catch (Exception mex)
                    {
                    }
                    finally
                    {
                        cmd.Dispose();
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }


            var SetupTyreStockDelete = ctxTFAT.CreditPurchaseSetup.Select(x => x.TyreStockDelete).FirstOrDefault();
            if (SetupTyreStockDelete == false)
            {
                List<CreditPurchaseVM> result = new List<CreditPurchaseVM>();
                result = (List<CreditPurchaseVM>)Session["Lothertrxlist"];
                foreach (var item in result)
                {
                    //Check Current TyreStock Available Or Transfer It Or NOt.
                    var CurrentStock = ctxTFAT.TyreStockSerial.Where(x => x.TableKey == item.TableKey && x.Branch == Model.Branch).FirstOrDefault();
                    if (CurrentStock != null)
                    {
                        if (!CurrentStock.IsActive)
                        {
                            return Json(new
                            {
                                Status = "Error",
                                Message = "This Document Not Allow To Delete Because OF This Tyre Stock Will Transfer It SomeWhere....!"
                            }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            //Previous Tyre Stock Status
                            var PrevioustyreStock = ctxTFAT.TyreStockSerial.Where(x => x.ParentKey == item.TableKey && x.Branch == Model.Branch && x.TableKey == CurrentStock.ParentKey).FirstOrDefault();
                            if (PrevioustyreStock != null)
                            {
                                if (!PrevioustyreStock.IsActive)
                                {
                                    return Json(new
                                    {
                                        Status = "Error",
                                        Message = "This Document Not Allow To Delete Because OF Old Tyre Stock Will Transfer It SomeWhere....!"
                                    }, JsonRequestBehavior.AllowGet);
                                }
                            }


                        }

                    }

                }
            }


            var mobj1 = ctxTFAT.Purchase.Where(x => x.TableKey == Model.ParentKey && x.Branch == Model.Branch).FirstOrDefault();
            if (mobj1 != null)
            {

                var CheckDependency2 = ctxTFAT.Outstanding.Where(x => x.aType == mobj1.Type && x.aSrl == mobj1.Srl.Value.ToString() && x.aPrefix == mobj1.Prefix && x.DocBranch == mobj1.Branch).Select(x => x).FirstOrDefault();
                if (CheckDependency2 != null)
                {
                    if (CheckDependency2.Type == mobj1.Type && CheckDependency2.Srl == mobj1.Srl.Value.ToString() && CheckDependency2.Prefix == mobj1.Prefix && CheckDependency2.DocBranch == mobj1.Branch)
                    {

                    }
                    else
                    {
                        Model.CheckMode = true;

                        Model.Message = " Document is Already Adjusted In Cash Bank Against : " + CheckDependency2.TableRefKey.ToString() + ", Cant " + Model.Mode;
                    }

                }
            }

            using (var transaction = ctxTFAT.Database.BeginTransaction(System.Data.IsolationLevel.Unspecified))
            {
                try
                {
                    var mobj11 = ctxTFAT.Purchase.Where(x => x.TableKey == Model.ParentKey && x.Branch == Model.Branch).FirstOrDefault();
                    if (mobj11 != null)
                    {
                        var RemoveAttach = ctxTFAT.Attachment.Where(x => x.Type == "PUR00" && x.Srl == mobj11.Srl.ToString()).ToList();
                        foreach (var item in RemoveAttach)
                        {
                            if (System.IO.File.Exists(item.FilePath))
                            {
                                System.IO.File.Delete(item.FilePath);
                            }
                        }
                        ctxTFAT.Attachment.RemoveRange(RemoveAttach);

                        var GetRemarkDocList = ctxTFAT.Narration.Where(x => x.Srl == mobj1.Srl.ToString() && x.Type == "PUR00").ToList();
                        if (GetRemarkDocList != null)
                        {
                            ctxTFAT.Narration.RemoveRange(GetRemarkDocList);
                        }
                    }
                    DeUpdate(Model);
                    UpdateAuditTrail(Model.Branch, Model.Mode, Model.Header, Model.ParentKey, ConvertDDMMYYTOYYMMDD(Model.DocuDate), Model.Amt, Model.BankCashCode, "Delete Credit Purchase", "A");

                    //UpdateAuditTrail(mbranchcode, "Delete", Model.Header, Model.ParentKey, DateTime.Now, 0, Model.ParentKey, "");

                    transaction.Commit();
                    transaction.Dispose();
                    //SendTrnsMsg("Delete", Model.Amt, mbranchcode + Model.ParentKey, ConvertDDMMYYTOYYMMDD(Model.DocuDate), "");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    return Json(new
                    {
                        Status = "Error",
                        Message = ex1.InnerException.InnerException.Message
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    return Json(new
                    {
                        Status = "Error",
                        Message = ex.InnerException.InnerException.Message
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new
            {
                Status = "Success",
                Message = "The Document is Deleted."
            }, JsonRequestBehavior.AllowGet);
        }

        #region attachment
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
                    att.AUTHORISE = "A00";
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
                    att.Type = String.IsNullOrEmpty(item.Type) == true ? "PUR00" : item.Type;
                    att.ParentKey = Model.ParentKey;

                    att.CompCode = mcompcode;
                    att.ExternalAttach = item.ExternalAttach;
                    att.RefDocNo = item.RefCode;
                    att.RefType = item.RefType;
                    ctxTFAT.Attachment.Add(att);
                    ctxTFAT.SaveChanges();
                    System.IO.File.WriteAllBytes(directoryPath, IData);

                    ++AttachCode;
                    ++J;
                }
            }

        }
        #endregion

        public ActionResult PrintSingleDocumentCrystal(GridOption Model)
        {
            //var PDFName = Model.Document.Substring(19);
            var PDFName = "";
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

            string mParentKey = Model.Document.Substring(6, Model.Document.Length - 6);
            Model.Branch = Model.Document.Substring(0, 6);

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
            //var PDFName = Model.Document.Substring(19);
            var PDFName = "";
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
                    mParentKey = Model.Document.Substring(6, Model.Document.Length - 6);
                    Model.Branch = Model.Document.Substring(0, 6);
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

        #region Not Required Functions

        public ActionResult GetAllRemarkOfDocument(string BillNo)
        {
            List<LoadingToDispatchVM> objledgerdetail = new List<LoadingToDispatchVM>();

            if (Session["Narrlist"] != null)
            {
                objledgerdetail = (List<LoadingToDispatchVM>)Session["Narrlist"];
            }
            if (objledgerdetail == null)
            {
                objledgerdetail = new List<LoadingToDispatchVM>();
            }
            var html = ViewHelper.RenderPartialView(this, "_DocumentNarrList", objledgerdetail);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveNarration(LoadingToDispatchVM Model)
        {
            string html = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    List<LoadingToDispatchVM> objledgerdetail = new List<LoadingToDispatchVM>();
                    LoadingToDispatchVM NewNarr = new LoadingToDispatchVM();

                    if (Session["Narrlist"] != null)
                    {
                        objledgerdetail = (List<LoadingToDispatchVM>)Session["Narrlist"];
                    }
                    if (objledgerdetail == null)
                    {
                        objledgerdetail = new List<LoadingToDispatchVM>();
                    }

                    if (Model.NarrStr != null)
                    {

                        if (Model.Mode != "Add")
                        {
                            Purchase fM = ctxTFAT.Purchase.Where(x => x.Srl.ToString() == Model.FMNO && x.Type == "PUR00").FirstOrDefault();
                            var LastSno = ctxTFAT.Narration.Where(x => x.ParentKey == fM.TableKey).ToList().Count();
                            ++LastSno;

                            Narration narr = new Narration();
                            narr.Branch = mbranchcode;
                            narr.Narr = Model.NarrStr;
                            narr.NarrRich = Model.Header;
                            narr.Prefix = mperiod;
                            narr.Sno = LastSno;
                            narr.Srl = fM.Srl.ToString();
                            narr.Type = "PUR00";
                            narr.ENTEREDBY = muserid;
                            narr.LASTUPDATEDATE = DateTime.Now;
                            narr.AUTHORISE = "A00";
                            narr.AUTHIDS = muserid;
                            narr.LocationCode = 0;
                            narr.TableKey = mbranchcode + "PUR00" + mperiod.Substring(0, 2) + LastSno.ToString("D3") + fM.Srl.ToString();
                            narr.CompCode = mcompcode;
                            narr.ParentKey = fM.TableKey;
                            ctxTFAT.Narration.Add(narr);
                            ctxTFAT.SaveChanges();
                            transaction.Commit();
                            transaction.Dispose();

                            NewNarr.FMNO = fM.Srl.ToString();
                            NewNarr.AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault();
                            NewNarr.NarrStr = Model.NarrStr;
                            NewNarr.ENTEREDBY = muserid;
                            NewNarr.NarrSno = objledgerdetail.Count() + 1;
                            NewNarr.PayLoadL = Model.Header;
                            objledgerdetail.Add(NewNarr);
                        }
                        else
                        {
                            NewNarr.FMNO = Model.FMNO.ToString();
                            NewNarr.AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault();
                            NewNarr.NarrStr = Model.NarrStr;
                            NewNarr.ENTEREDBY = muserid;
                            NewNarr.NarrSno = objledgerdetail.Count() + 1;
                            NewNarr.PayLoadL = Model.Header;
                            objledgerdetail.Add(NewNarr);
                        }

                        Session["Narrlist"] = objledgerdetail;

                        html = ViewHelper.RenderPartialView(this, "_DocumentNarrList", objledgerdetail);
                    }
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

        public ActionResult DeleteNarr(LoadingToDispatchVM mModel)
        {
            string html = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    List<LoadingToDispatchVM> objledgerdetail = new List<LoadingToDispatchVM>();
                    if (Session["Narrlist"] != null)
                    {
                        objledgerdetail = (List<LoadingToDispatchVM>)Session["Narrlist"];
                    }
                    if (objledgerdetail == null)
                    {
                        objledgerdetail = new List<LoadingToDispatchVM>();
                    }

                    if (mModel.Mode != "Add")
                    {
                        Narration narration = ctxTFAT.Narration.Where(x => x.Sno.ToString() == mModel.NarrSno.ToString() && x.Type == "PUR00").FirstOrDefault();
                        if (narration != null)
                        {
                            ctxTFAT.Narration.Remove(narration);
                            ctxTFAT.SaveChanges();
                            transaction.Commit();
                            transaction.Dispose();
                        }
                    }
                    objledgerdetail = objledgerdetail.Where(x => x.NarrSno != mModel.NarrSno).ToList();
                    Session["Narrlist"] = objledgerdetail;
                    html = ViewHelper.RenderPartialView(this, "_DocumentNarrList", objledgerdetail);
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
    }
}