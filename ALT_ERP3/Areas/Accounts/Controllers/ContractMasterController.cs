using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class ContractMasterController : BaseController
    {
        private static string mauthorise = "A00";
        private static string mdocument = "";
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
        // GET: Accounts/ContractMaster

        public JsonResult LoadBranch(string term)
        {
            //List<TfatBranch> list = ctxTFAT.TfatBranch.Where(x => x.Category != "Zone" && x.Code != "G00000" && x.Grp != "G00000" && x.Status == true).OrderBy(x => x.Name).ToList();
            List<TfatBranch> list = ctxTFAT.TfatBranch.Where(x => x.Category != "Zone"  && x.Status == true).OrderBy(x => x.Name).ToList();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }

            List<TfatBranch> treeTables = new List<TfatBranch>();

            foreach (var item in list)
            {
                if (item.Category == "Branch")
                {
                    item.Name += " - B";
                    treeTables.Add(item);
                }
                else if (item.Category == "SubBranch")
                {
                    item.Name += " - SB";
                    treeTables.Add(item);
                }else if (item.Category == "0")
                {
                    if (item.Code.Contains("G000"))
                    {
                        item.Name += " - G0";
                        treeTables.Add(item);
                    }
                    else
                    {
                        item.Name += " - HO";
                        treeTables.Add(item);
                    }
                    
                }
                else
                {
                    item.Name = item.Name + " - A";
                    treeTables.Add(item);
                }
            }
            var Modified = treeTables.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadCustomer(string term, bool MasterAccount)
        {

            if (MasterAccount)
            {
                List<Master> list = new List<Master>();
                list.Add(new Master
                {
                    Code = "General",
                    Name = "General"
                });
                list.AddRange(ctxTFAT.Master.Where(x => x.Hide == false && x.AcType == "D" && x.BaseGr == "D").ToList());

                //list = ctxTFAT.Master.Where(x => x.Hide == false && x.AcType == "D" && x.BaseGr == "D").ToList();

                if (!(String.IsNullOrEmpty(term)))
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
            else
            {
                List<CustomerMaster> list = new List<CustomerMaster>();
                list.Add(new CustomerMaster
                {
                    Code = "General",
                    Name = "General"
                });
                list.AddRange(ctxTFAT.CustomerMaster.Where(x => x.Hide == false && x.AcType == "D" && x.BaseGr == "D").ToList());

                //list = ctxTFAT.Master.Where(x => x.Hide == false && x.AcType == "D" && x.BaseGr == "D").ToList();

                if (!(String.IsNullOrEmpty(term)))
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



        }

        public ActionResult Index(ContractMasterVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");

            mdocument = mModel.Document;


            #region Default Charges
            List<PurchaseVM> objledgerdetail = new List<PurchaseVM>();
            var trncharges = ctxTFAT.Charges.Where(x => x.MainType == "SL" && x.SubType == "RS" && x.Type == "LR000" && x.DontUse == false).Select(x => new { x.Fld, x.Head, x.EqAmt, x.Equation, x.Code }).ToList();
            foreach (var i in trncharges)
            {
                PurchaseVM c = new PurchaseVM();
                c.Fld = i.Fld;
                c.Code = i.Head;
                c.AddLess = i.EqAmt;
                c.Equation = i.Equation;
                c.tempId = Convert.ToInt16(i.Fld.Substring(1, 3));
                c.Val1 = 0;
                c.ChgPostCode = i.Code;
                objledgerdetail.Add(c);
            }

            #endregion



            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.ConMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    mModel.Customer = mList.Cust;
                    mModel.CustomerType = mList.CustType;
                    if (mList.Cust == "General")
                    {
                        mModel.CustomerN = "General";
                    }
                    else
                    {
                        if (mList.CustType=="CU")
                        {
                            mModel.CustomerN = ctxTFAT.CustomerMaster.Where(x => x.Code == mList.Cust).Select(x => x.Name).FirstOrDefault();
                        }
                        else
                        {
                            mModel.CustomerN = ctxTFAT.Master.Where(x => x.Code == mList.Cust).Select(x => x.Name).FirstOrDefault();
                        }
                    }
                    mModel.FromBranch = mList.FromBranch;
                    mModel.FromBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == mList.FromBranch).Select(x => x.Name).FirstOrDefault();
                    mModel.ToBranch = mList.ToBranch;
                    mModel.ToBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == mList.ToBranch).Select(x => x.Name).FirstOrDefault();
                    mModel.DocDate = mList.ContractDate.ToShortDateString();
                    mModel.FromDate = mList.FromDt.ToShortDateString();
                    mModel.ToDate = mList.ToDt.ToShortDateString();
                    mModel.PaymentTerms = mList.PaymentTerms;
                    mModel.Remark = mList.Remark;

                    var mobj = (from ConDetail in ctxTFAT.ConDetail
                                where ConDetail.CustCode == mModel.Document
                                orderby ConDetail.Services
                                select new ContractList()
                                {
                                    ContractType=ConDetail.ContrType,
                                    Service = ConDetail.Services,
                                    ServiceN = ConDetail.ContrType== "ChargeType" ? ctxTFAT.ChargeTypeMaster.Where(x => x.Code.Trim() == ConDetail.Services.Trim()).Select(x => x.ChargeType).FirstOrDefault(): ctxTFAT.DescriptionMaster.Where(x => x.Code.Trim() == ConDetail.Services.Trim()).Select(x => x.Description).FirstOrDefault(),
                                    Sno = ConDetail.SrNo,
                                    TypeOfChrg = ConDetail.WtType,
                                    FromWT = ConDetail.Wtfrom.Value,
                                    ToWT = ConDetail.WtTo.Value,
                                    Rate = (float)ConDetail.Rate,
                                    ChargeONN = ConDetail.ChgOn == true ? "Charge On Chargeble WT " : "Charge On Actual WT",
                                    ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                    ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                    UniqueKey = ConDetail.Services.Trim() + ConDetail.SrNo.ToString().Trim(),
                                    ConDetilsCode = ConDetail.Code,
                                }).ToList();

                    foreach (var item in mobj)
                    {
                        item.Charges = GetChargesOfService(item.ConDetilsCode, item.Service, item.Sno);
                    }

                    mModel.ContractList = mobj;

                }
            }
            else
            {



                mModel.DocDate = DateTime.Now.ToShortDateString();
            }
            ContractList contract = new ContractList();
            contract.ServiceList = GetServiceList();
            contract.ItemListList = GetItemList();
            mModel.contract = contract;
            //List<PurchaseVM> objledgerdetail = new List<PurchaseVM>();
            //var trncharges = ctxTFAT.Charges.Where(x => x.MainType == "SL" && x.SubType == "RS" && x.Type == "LR" && x.DontUse == false).Select(x => new { x.Fld, x.Head, x.EqAmt, x.Equation, x.Code }).ToList();
            //foreach (var i in trncharges)
            //{
            //    PurchaseVM c = new PurchaseVM();
            //    c.Fld = i.Fld;
            //    c.Code = i.Head;
            //    c.AddLess = i.EqAmt;
            //    c.Equation = i.Equation;
            //    c.tempId = Convert.ToInt16(i.Fld.Substring(1, 3));
            //    c.Val1 = 0;
            //    c.ChgPostCode = i.Code;
            //    objledgerdetail.Add(c);
            //}
            mModel.ChargesHead = objledgerdetail;


            Session.Add("ContractMasterVM", mModel);
            return View(mModel);
        }

        public List<PurchaseVM> GetChargesOfService(string code, string Service, int Sno)
        {
            List<PurchaseVM> purchases = new List<PurchaseVM>();

            var trncharges = ctxTFAT.Charges.Where(x => x.MainType == "SL" && x.SubType == "RS" && x.Type == "LR000" && x.DontUse == false).Select(x => new { x.Fld, x.Head, x.EqAmt, x.Equation, x.Code }).ToList();
            foreach (var i in trncharges)
            {
                PurchaseVM c = new PurchaseVM();
                c.Fld = i.Fld;
                c.Code = i.Head;
                c.AddLess = i.EqAmt;
                c.Equation = i.Equation;
                c.tempId = Convert.ToInt16(i.Fld.Substring(1, 3));
                var ValAndFlg = GetChargeValValueAndFlg(code, Service, Sno, "Val", "Flg", c.tempId);
                c.Val1 = Convert.ToDecimal(ValAndFlg[0]);
                c.Type = ValAndFlg[1];
                purchases.Add(c);
            }

            return purchases;
        }

        public string[] GetChargeValValueAndFlg(string code, string Service, int Sno, string Val, string Flg, int i)
        {
            string connstring = GetConnectionString();
            string[] abc = new string[2];

            var loginQuery3 = @"select " + Val + i + "," + Flg + i + " from ConDetail where Code='" + code + "' and Services='" + Service + "' and SrNo=" + Sno + " ";
            DataTable mDt2 = GetDataTable(loginQuery3, connstring);
            if (mDt2.Rows.Count > 0)
            {
                abc[0] = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? "0" : mDt2.Rows[0][0].ToString();
                abc[1] = (mDt2.Rows[0][1].ToString() == "" || mDt2.Rows[0][1].ToString() == null) ? "" : mDt2.Rows[0][1].ToString();
                //abc = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? "0" : mDt2.Rows[0][0].ToString();
            }
            else
            {
                abc[0] = "0";
                abc[1] = "";
            }
            return abc;
            //return Convert.ToDecimal(abc);
        }


        //public decimal GetChargeValValue(string code, string Service, int Sno, string Val, int i)
        //{
        //    string connstring = GetConnectionString();
        //    string abc;

        //    var loginQuery3 = @"select " + Val + i + " from ConDetail where Code='" + code + "' and Services='" + Service + "' and SrNo=" + Sno + " ";
        //    DataTable mDt2 = GetDataTable(loginQuery3, connstring);
        //    if (mDt2.Rows.Count > 0)
        //    {
        //        abc = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? "0" : mDt2.Rows[0][0].ToString();
        //    }
        //    else
        //    {
        //        abc = "0";
        //    }
        //    return Convert.ToDecimal(abc);
        //}

        //public string GetChargeValType(string code, string Service, int Sno, string Val, int i)
        //{
        //    string connstring = GetConnectionString();
        //    string abc;

        //    var loginQuery3 = @"select " + Val + i + " from ConDetail where Code='" + code + "' and Services='" + Service + "' and SrNo=" + Sno + " ";
        //    DataTable mDt2 = GetDataTable(loginQuery3, connstring);
        //    if (mDt2.Rows.Count > 0)
        //    {
        //        abc = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? "" : mDt2.Rows[0][0].ToString();
        //    }
        //    else
        //    {
        //        abc = "";
        //    }
        //    return abc;
        //}

        public List<SelectListItem> GetServiceList()
        {
            List<SelectListItem> CallServiceList = new List<SelectListItem>();
            var BranchList = ctxTFAT.ChargeTypeMaster.Where(x => x.Acitve == true).Select(x => new { x.Code, x.ChargeType }).ToList();
            foreach (var item in BranchList)
            {
                CallServiceList.Add(new SelectListItem
                {
                    Value = item.Code,
                    Text = item.ChargeType
                });
            }
            return CallServiceList;
        }
        public List<SelectListItem> GetItemList()
        {
            List<SelectListItem> CallServiceList = new List<SelectListItem>();
            var BranchList = ctxTFAT.DescriptionMaster.Where(x => x.Acitve == true).Select(x => new { x.Code, x.Description }).ToList();
            foreach (var item in BranchList)
            {
                CallServiceList.Add(new SelectListItem
                {
                    Value = item.Code,
                    Text = item.Description
                });
            }
            return CallServiceList;
        }

        public string GenerateCode()
        {
            var LastCode = ctxTFAT.ConMaster.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).FirstOrDefault();
            if (String.IsNullOrEmpty(LastCode))
            {
                LastCode = "100000";
            }
            else
            {
                int NewCode = Convert.ToInt32(LastCode) + 1;
                LastCode = NewCode.ToString("D6");
            }
            return LastCode;
        }

        public string GenerateCodeConMasterRel()
        {
            var LastCode = ctxTFAT.ConMasterRel.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).FirstOrDefault();
            if (String.IsNullOrEmpty(LastCode))
            {
                LastCode = "100000";
            }
            else
            {
                int NewCode = Convert.ToInt32(LastCode) + 1;
                LastCode = NewCode.ToString("D6");
            }
            return LastCode;
        }

        public ActionResult CheckDate(string DocDate, bool DOCDT, string FromDT, string ToDT)
        {
            bool status = true;
            string message = "";
            if (DOCDT)
            {
                //DateTime Date = ConvertDDMMYYTOYYMMDD(DocDate);
                //if (!(ConvertDDMMYYTOYYMMDD(StartDate) <= Date && Date <= ConvertDDMMYYTOYYMMDD(EndDate)))
                //{
                //    status = false;
                //    message = "Document Date Should Be Between Financial Year...!";
                //}
            }
            else
            {
                DateTime FDate = ConvertDDMMYYTOYYMMDD(FromDT);
                DateTime TDate = ConvertDDMMYYTOYYMMDD(ToDT);
                //if (!((ConvertDDMMYYTOYYMMDD(StartDate) <= FDate && FDate <= ConvertDDMMYYTOYYMMDD(EndDate)) && (ConvertDDMMYYTOYYMMDD(StartDate) <= TDate && TDate <= ConvertDDMMYYTOYYMMDD(EndDate))))
                //{
                //    status = false;
                //    message = "From Date and To Date Should Be Between Financial Year...!";
                //}

            }
            return Json(new { Status = status, Message = message, JsonRequestBehavior.AllowGet });
        }

        #region Sub-Functions
        public ActionResult GetBreakLedger(ContractMasterVM Model)
        {
            if (Model.Mode == "Add")
            {
                var FromDt = ConvertDDMMYYTOYYMMDD(Model.FromDate);
                var ToDt = ConvertDDMMYYTOYYMMDD(Model.ToDate);
                var ConMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.Customer && x.CustType == Model.CustomerType && x.FromBranch == Model.FromBranch && x.ToBranch == Model.ToBranch && ( ( x.FromDt <= FromDt && FromDt <= x.ToDt ) || ( x.FromDt <= ToDt && ToDt <= x.ToDt ) ) && x.Code != Model.Document).FirstOrDefault();
                if (ConMaster != null)
                {
                    return Json(new
                    {
                        Message = "Based On Basic Data We Found Contract So We Cant Processed ....!",
                        Status = "CancelError"
                    }, JsonRequestBehavior.AllowGet);
                }
            }



            if (Model.Mode == "Edit")
            {
                var result = (ContractMasterVM)Session["ContractMasterVM"];
                var result1 = result.ContractList.Where(x => x.UniqueKey == Model.contract.UniqueKey).FirstOrDefault();

                Model.contract = result1;
                Model.contract.ServiceList = GetServiceList();
                Model.contract.ItemListList = GetItemList();

                var jsonResult = Json(new { Html = this.RenderPartialView("AddNewContract", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            else
            {
                ContractList contract = new ContractList();
                contract.ServiceList = GetServiceList();
                contract.ItemListList = GetItemList();

                var result = (ContractMasterVM)Session["ContractMasterVM"];
                if (result == null)
                {
                    contract.Sno = 1;
                }
                else
                {
                    contract.Sno = result.ContractList == null ? 1 : result.ContractList.Count + 1;
                }

                List<PurchaseVM> objledgerdetail = new List<PurchaseVM>();
                var trncharges = ctxTFAT.Charges.Where(x => x.MainType == "SL" && x.SubType == "RS" && x.Type == "LR000" && x.DontUse == false).Select(x => new { x.Fld, x.Head, x.EqAmt, x.Equation, x.Code }).ToList();
                foreach (var i in trncharges)
                {
                    PurchaseVM c = new PurchaseVM();
                    c.Fld = i.Fld;
                    c.Code = i.Head;
                    c.AddLess = i.EqAmt;
                    c.Equation = i.Equation;
                    c.tempId = Convert.ToInt16(i.Fld.Substring(1, 3));
                    c.ColVal = "0";
                    c.ChgPostCode = i.Code;
                    objledgerdetail.Add(c);
                }
                contract.Charges = objledgerdetail;

                Model.contract = contract;

                var jsonResult = Json(new { Html = this.RenderPartialView("AddNewContract", Model) }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }
        [HttpPost]
        public ActionResult AddEditSelectedLedger(ContractMasterVM Model)
        {



            if (Model.Mode == "Add")
            {
                ContractMasterVM masterVM = new ContractMasterVM();
                List<ContractList> objledgerdetail = new List<ContractList>();
                if (Session["ContractMasterVM"] != null)
                {
                    masterVM = (ContractMasterVM)Session["ContractMasterVM"];
                    if (masterVM.ContractList != null)
                    {
                        objledgerdetail = masterVM.ContractList;
                    }
                }

                if (Model.contract.FromWT != 0 )
                {
                    var UserRange = Enumerable.Range(Model.contract.FromWT, (Model.contract.ToWT - Model.contract.FromWT) + 1);
                    var OtherRageContractlist = objledgerdetail.Where(x => x.UniqueKey != Model.contract.UniqueKey && x.Service == Model.contract.Service && x.ContractType==Model.contract.ContractType).ToList();
                    foreach (var item in OtherRageContractlist)
                    {
                        var ExistingRange = Enumerable.Range(item.FromWT, (item.ToWT - item.FromWT) + 1);
                        var commonItems = UserRange.Intersect(ExistingRange);
                        if (commonItems.Count() >= 1)
                        {
                            return Json(new
                            {
                                Message = "This Contact FromWT And ToWT Range Already Use So We Cant Allow Duplicate Range.",
                                Status = "CancelError"
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    var Service = objledgerdetail.Where(x => x.Service == Model.contract.Service && x.ContractType == Model.contract.ContractType).FirstOrDefault();
                    if (Service != null)
                    {
                        return Json(new
                        {
                            Message = "This Contact Not Allow To Add " + Service.ServiceN + " Service Because Of This Service Already Created....!",
                            Status = "CancelError"
                        }, JsonRequestBehavior.AllowGet);
                    }
                }



                ContractList Singlecontract = new ContractList();
                Singlecontract.ContractType = Model.contract.ContractType;
                Singlecontract.Service = Model.contract.Service;
                Singlecontract.ServiceN = Model.contract.ContractType== "ChargeType" ? ctxTFAT.ChargeTypeMaster.Where(x => x.Code.Trim() == Model.contract.Service.Trim()).Select(x => x.ChargeType).FirstOrDefault() : ctxTFAT.DescriptionMaster.Where(x => x.Code.Trim() == Model.contract.Service.Trim()).Select(x => x.Description).FirstOrDefault();
                Singlecontract.Sno = Model.contract.Sno;
                Singlecontract.TypeOfChrg = Model.contract.TypeOfChrg;
                Singlecontract.FromWT = Model.contract.FromWT;
                Singlecontract.ToWT = Model.contract.ToWT;
                Singlecontract.Rate = Model.contract.Rate;
                Singlecontract.ChargeOfActWT = Model.contract.ChargeOfActWT;
                Singlecontract.ChargeOfChrgWT = Model.contract.ChargeOfChrgWT;
                Singlecontract.ChargeONN = Model.contract.ChargeOfChrgWT == true ? "Charge On Chargeble WT " : "Charge On Actual WT";
                Singlecontract.Charges = Model.contract.Charges;
                Singlecontract.UniqueKey = (Model.contract.Service.Trim() + Model.contract.Sno.ToString().Trim()).Trim();


                objledgerdetail.Add(Singlecontract);
                masterVM.ContractList = objledgerdetail;
                Session.Add("ContractMasterVM", masterVM);
                var html = ViewHelper.RenderPartialView(this, "ContractList", masterVM);
                return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ContractMasterVM masterVM = new ContractMasterVM();
                List<ContractList> objledgerdetail = new List<ContractList>();
                if (Session["ContractMasterVM"] != null)
                {
                    masterVM = (ContractMasterVM)Session["ContractMasterVM"];
                    if (masterVM.ContractList != null)
                    {
                        objledgerdetail = masterVM.ContractList;
                    }
                }

                if (Model.contract.Service == "100000" || Model.contract.Service == "100001")
                {
                    var UserRange = Enumerable.Range(Model.contract.FromWT, (Model.contract.ToWT - Model.contract.FromWT) + 1);
                    var OtherRageContractlist = objledgerdetail.Where(x => x.UniqueKey != Model.contract.UniqueKey && x.Service == Model.contract.Service).ToList();
                    foreach (var item in OtherRageContractlist)
                    {
                        var ExistingRange = Enumerable.Range(item.FromWT, (item.ToWT - item.FromWT) + 1);
                        var commonItems = UserRange.Intersect(ExistingRange);
                        if (commonItems.Count() >= 1)
                        {
                            return Json(new
                            {
                                Message = "This Contact FromWT And ToWT Range Already Use So We Cant Allow Duplicate Range.",
                                Status = "CancelError"
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    var Service = objledgerdetail.Where(x => x.Service == Model.contract.Service && x.UniqueKey != Model.contract.UniqueKey).FirstOrDefault();
                    if (Service != null)
                    {
                        return Json(new
                        {
                            Message = "This Contact Not Allow To Add " + Service.ServiceN + " Service Because Of This Service Already Created....!",
                            Status = "CancelError"
                        }, JsonRequestBehavior.AllowGet);
                    }
                }

                var result = objledgerdetail.Where(x => x.UniqueKey == Model.contract.UniqueKey).FirstOrDefault();

                result.ContractType = Model.contract.ContractType;
                result.Service = Model.contract.Service;
                result.ServiceN = Model.contract.ContractType == "ChargeType" ? ctxTFAT.ChargeTypeMaster.Where(x => x.Code.Trim() == Model.contract.Service.Trim()).Select(x => x.ChargeType).FirstOrDefault() : ctxTFAT.DescriptionMaster.Where(x => x.Code.Trim() == Model.contract.Service.Trim()).Select(x => x.Description).FirstOrDefault();
                result.Sno = Model.contract.Sno;
                result.TypeOfChrg = Model.contract.TypeOfChrg;
                result.FromWT = Model.contract.FromWT;
                result.ToWT = Model.contract.ToWT;
                result.Rate = Model.contract.Rate;
                result.ChargeOfActWT = Model.contract.ChargeOfActWT;
                result.ChargeOfChrgWT = Model.contract.ChargeOfChrgWT;
                result.ChargeONN = Model.contract.ChargeOfChrgWT == true ? "Charge On Chargeble WT " : "Charge On Actual WT";
                result.Charges = Model.contract.Charges;

                Session.Add("ContractMasterVM", masterVM);
                var html = ViewHelper.RenderPartialView(this, "ContractList", masterVM);
                return Json(new
                {
                    Html = html
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult DeleteLedger(ContractMasterVM Model)
        {
            ContractMasterVM masterVM = new ContractMasterVM();
            List<ContractList> objledgerdetail = new List<ContractList>();
            if (Session["ContractMasterVM"] != null)
            {
                masterVM = (ContractMasterVM)Session["ContractMasterVM"];
                if (masterVM.ContractList != null)
                {
                    objledgerdetail = masterVM.ContractList;
                }
            }

            var result = objledgerdetail.Where(x => x.UniqueKey != Model.contract.UniqueKey).ToList();
            objledgerdetail = result;
            masterVM.ContractList = objledgerdetail;
            Session.Add("ContractMasterVM", masterVM);

            var html = ViewHelper.RenderPartialView(this, "ContractList", masterVM);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        public ActionResult SaveData(ContractMasterVM Model)
        {
            var result = (ContractMasterVM)Session["ContractMasterVM"];
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    bool mAdd = true;
                    if (Model.Mode == "Delete")
                    {
                        var mli = ctxTFAT.ConMaster.Where(x => x.Code == Model.Document).FirstOrDefault();
                        DeleteStateMaster(Model);
                        transaction.Commit();
                        transaction.Dispose();
                        UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, mli.ParentKey, DateTime.Now, 0, "", "Delete Contract Master", "NA");

                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    ConMaster conMaster = new ConMaster();
                    if (ctxTFAT.ConMaster.Where(x => x.Code == Model.Document).FirstOrDefault() != null)
                    {
                        conMaster = ctxTFAT.ConMaster.Where(x => x.Code == Model.Document).FirstOrDefault();
                        mAdd = false;
                        DeUpdate(Model);
                    }


                    var FromDt = ConvertDDMMYYTOYYMMDD(Model.FromDate);
                    var ToDt = ConvertDDMMYYTOYYMMDD(Model.ToDate);
                    var ConMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.Customer && x.CustType == Model.CustomerType && x.FromBranch == Model.FromBranch && x.ToBranch == Model.ToBranch && ( ( x.FromDt <= FromDt && FromDt <= x.ToDt ) ||( x.FromDt <= ToDt && ToDt <= x.ToDt ) ) && x.Code != Model.Document).FirstOrDefault();
                    if (ConMaster != null)
                    {
                        return Json(new
                        {
                            Message = "Based On Basic Data We Found Contract So We Cant Processed ....!",
                            Status = "Error"
                        }, JsonRequestBehavior.AllowGet);
                    }



                    #region ConMaster

                    if (mAdd)
                    {
                        conMaster.Code = GenerateCode();
                        
                        conMaster.TableKey = "CONMT" + mperiod.Substring(0, 2) + "001" + conMaster.Code;
                        conMaster.ParentKey = "CONMT" + mperiod.Substring(0, 2) + conMaster.Code;
                        conMaster.DocDt = DateTime.Now;
                    }
                    conMaster.Branch = mbranchcode;
                    conMaster.Cust = Model.Customer;
                    conMaster.CustType = Model.CustomerType;
                    conMaster.ContractDate = ConvertDDMMYYTOYYMMDD(Model.DocDate);
                    conMaster.ToBranch = Model.ToBranch;
                    conMaster.FromDt = ConvertDDMMYYTOYYMMDD(Model.FromDate);
                    conMaster.PaymentTerms = Model.PaymentTerms;
                    conMaster.Remark = Model.Remark;
                    conMaster.FromBranch = Model.FromBranch;
                    conMaster.ToDt = ConvertDDMMYYTOYYMMDD(Model.ToDate);

                    conMaster.ENTEREDBY = muserid;
                    conMaster.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    conMaster.AUTHORISE = mauthorise;
                    conMaster.AUTHIDS = muserid;

                    #endregion

                    #region ConMasterRel,ConDetail

                    var Contractlist = result.ContractList;
                    var GetDistinctServiceList = Contractlist.Select(x => x.Service).ToList().Distinct();
                    var ConMstCode = ctxTFAT.ConMasterRel.OrderByDescending(x => x.RECORDKEY).Select(x => x.ChargCode).FirstOrDefault();
                    int MasterCode = 0;
                    if (String.IsNullOrEmpty(ConMstCode))
                    {
                        MasterCode = 100000;
                    }
                    else
                    {
                        MasterCode = Convert.ToInt32(ConMstCode) + 1;
                    }
                    foreach (var item in GetDistinctServiceList)
                    {
                        #region conMasterRel

                        ConMasterRel conMasterRel = new ConMasterRel();
                        conMasterRel.Code = conMaster.Code;
                        conMasterRel.ChargCode = MasterCode.ToString("D6");

                        conMasterRel.ENTEREDBY = muserid;
                        conMasterRel.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                        conMasterRel.AUTHORISE = mauthorise;
                        conMasterRel.AUTHIDS = muserid;

                        ctxTFAT.ConMasterRel.Add(conMasterRel);

                        #endregion

                        #region ConDetail

                        var ServiceList = Contractlist.Where(x => x.Service == item).ToList();
                        int SNO = 1;
                        foreach (var contract in ServiceList)
                        {
                            ConDetail conDetail = new ConDetail();
                            conDetail.ContrType = contract.ContractType;
                            conDetail.CustCode = conMaster.Code;
                            conDetail.Code = conMasterRel.ChargCode;
                            conDetail.Services = contract.Service;
                            conDetail.SrNo = SNO;
                            conDetail.WtType = contract.TypeOfChrg;
                            conDetail.Wtfrom = contract.FromWT;
                            conDetail.WtTo = contract.ToWT;
                            conDetail.Rate = Convert.ToDecimal(contract.Rate);
                            conDetail.ChgOn = contract.ChargeOfChrgWT;
                            conDetail.Val1 = contract.Charges.Where(x => x.Fld == "F001").Select(x => x) != null ? GetChargesVal(contract.Charges, "F001") : 0;
                            conDetail.Val2 = contract.Charges.Where(x => x.Fld == "F002").Select(x => x) != null ? GetChargesVal(contract.Charges, "F002") : 0;
                            conDetail.Val3 = contract.Charges.Where(x => x.Fld == "F003").Select(x => x) != null ? GetChargesVal(contract.Charges, "F003") : 0;
                            conDetail.Val4 = contract.Charges.Where(x => x.Fld == "F004").Select(x => x) != null ? GetChargesVal(contract.Charges, "F004") : 0;
                            conDetail.Val5 = contract.Charges.Where(x => x.Fld == "F005").Select(x => x) != null ? GetChargesVal(contract.Charges, "F005") : 0;
                            conDetail.Val6 = contract.Charges.Where(x => x.Fld == "F006").Select(x => x) != null ? GetChargesVal(contract.Charges, "F006") : 0;
                            conDetail.Val7 = contract.Charges.Where(x => x.Fld == "F007").Select(x => x) != null ? GetChargesVal(contract.Charges, "F007") : 0;
                            conDetail.Val8 = contract.Charges.Where(x => x.Fld == "F008").Select(x => x) != null ? GetChargesVal(contract.Charges, "F008") : 0;
                            conDetail.Val9 = contract.Charges.Where(x => x.Fld == "F009").Select(x => x) != null ? GetChargesVal(contract.Charges, "F009") : 0;
                            conDetail.Val10 = contract.Charges.Where(x => x.Fld == "F010").Select(x => x) != null ? GetChargesVal(contract.Charges, "F010") : 0;
                            conDetail.Val11 = contract.Charges.Where(x => x.Fld == "F011").Select(x => x) != null ? GetChargesVal(contract.Charges, "F011") : 0;
                            conDetail.Val12 = contract.Charges.Where(x => x.Fld == "F012").Select(x => x) != null ? GetChargesVal(contract.Charges, "F012") : 0;
                            conDetail.Val13 = contract.Charges.Where(x => x.Fld == "F013").Select(x => x) != null ? GetChargesVal(contract.Charges, "F013") : 0;
                            conDetail.Val14 = contract.Charges.Where(x => x.Fld == "F014").Select(x => x) != null ? GetChargesVal(contract.Charges, "F014") : 0;
                            conDetail.Val15 = contract.Charges.Where(x => x.Fld == "F015").Select(x => x) != null ? GetChargesVal(contract.Charges, "F015") : 0;
                            conDetail.Val16 = contract.Charges.Where(x => x.Fld == "F016").Select(x => x) != null ? GetChargesVal(contract.Charges, "F016") : 0;
                            conDetail.Val17 = contract.Charges.Where(x => x.Fld == "F017").Select(x => x) != null ? GetChargesVal(contract.Charges, "F017") : 0;
                            conDetail.Val18 = contract.Charges.Where(x => x.Fld == "F018").Select(x => x) != null ? GetChargesVal(contract.Charges, "F018") : 0;
                            conDetail.Val19 = contract.Charges.Where(x => x.Fld == "F019").Select(x => x) != null ? GetChargesVal(contract.Charges, "F019") : 0;
                            conDetail.Val20 = contract.Charges.Where(x => x.Fld == "F020").Select(x => x) != null ? GetChargesVal(contract.Charges, "F020") : 0;
                            conDetail.Val21 = contract.Charges.Where(x => x.Fld == "F021").Select(x => x) != null ? GetChargesVal(contract.Charges, "F021") : 0;
                            conDetail.Val22 = contract.Charges.Where(x => x.Fld == "F022").Select(x => x) != null ? GetChargesVal(contract.Charges, "F022") : 0;
                            conDetail.Val23 = contract.Charges.Where(x => x.Fld == "F023").Select(x => x) != null ? GetChargesVal(contract.Charges, "F023") : 0;
                            conDetail.Val24 = contract.Charges.Where(x => x.Fld == "F024").Select(x => x) != null ? GetChargesVal(contract.Charges, "F024") : 0;
                            conDetail.Val25 = contract.Charges.Where(x => x.Fld == "F025").Select(x => x) != null ? GetChargesVal(contract.Charges, "F025") : 0;
                            conDetail.Flg1 = contract.Charges.Where(x => x.Fld == "F001").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg2 = contract.Charges.Where(x => x.Fld == "F002").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg3 = contract.Charges.Where(x => x.Fld == "F003").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg4 = contract.Charges.Where(x => x.Fld == "F004").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg5 = contract.Charges.Where(x => x.Fld == "F005").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg6 = contract.Charges.Where(x => x.Fld == "F006").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg7 = contract.Charges.Where(x => x.Fld == "F007").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg8 = contract.Charges.Where(x => x.Fld == "F008").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg9 = contract.Charges.Where(x => x.Fld == "F009").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg10 = contract.Charges.Where(x => x.Fld == "F010").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg11 = contract.Charges.Where(x => x.Fld == "F011").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg12 = contract.Charges.Where(x => x.Fld == "F012").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg13 = contract.Charges.Where(x => x.Fld == "F013").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg14 = contract.Charges.Where(x => x.Fld == "F014").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg15 = contract.Charges.Where(x => x.Fld == "F015").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg16 = contract.Charges.Where(x => x.Fld == "F016").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg17 = contract.Charges.Where(x => x.Fld == "F017").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg18 = contract.Charges.Where(x => x.Fld == "F018").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg19 = contract.Charges.Where(x => x.Fld == "F019").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg20 = contract.Charges.Where(x => x.Fld == "F020").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg21 = contract.Charges.Where(x => x.Fld == "F021").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg22 = contract.Charges.Where(x => x.Fld == "F022").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg23 = contract.Charges.Where(x => x.Fld == "F023").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg24 = contract.Charges.Where(x => x.Fld == "F024").Select(x => x.Type).FirstOrDefault();
                            conDetail.Flg25 = contract.Charges.Where(x => x.Fld == "F025").Select(x => x.Type).FirstOrDefault();

                            conDetail.ENTEREDBY = muserid;
                            conDetail.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                            conDetail.AUTHORISE = mauthorise;
                            conDetail.AUTHIDS = muserid;

                            ctxTFAT.ConDetail.Add(conDetail);
                            ++SNO;
                        }
                        #endregion

                        ++MasterCode;
                    }
                    #endregion

                    if (mAdd)
                    {
                        ctxTFAT.ConMaster.Add(conMaster);
                    }
                    else
                    {
                        ctxTFAT.Entry(conMaster).State = EntityState.Modified;
                    }

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    Session["ContractMasterVM"] = null;
                    UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, conMaster.ParentKey, DateTime.Now, 0, "", "Save Contract Master", "NA");

                    return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
                }

                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    string dd1 = ex1.InnerException.Message;
                    return Json(new
                    {
                        Status = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    string dd = ex.InnerException.InnerException.Message;
                    return Json(new
                    {
                        Status = "Error",
                        Message = dd
                    }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        private void DeUpdate(ContractMasterVM Model)
        {
            var mDeleteConMasterRel = ctxTFAT.ConMasterRel.Where(x => x.Code == Model.Document);
            var ChargeCodeList = mDeleteConMasterRel.Select(x => x.ChargCode).ToList();
            var mDeleteConDetail = ctxTFAT.ConDetail.Where(x => ChargeCodeList.Contains(x.Code)).ToList();
            ctxTFAT.ConMasterRel.RemoveRange(mDeleteConMasterRel);
            ctxTFAT.ConDetail.RemoveRange(mDeleteConDetail);
            ctxTFAT.SaveChanges();
        }

        public double GetChargesVal(List<PurchaseVM> Charges, string FToken)
        {
            string connstring = GetConnectionString();
            string sql;
            double mamtm;
            var Val = Charges.Where(x => x.Fld == FToken).Select(x => x.Val1).FirstOrDefault();
            var PosNeg = Charges.Where(x => x.Fld == FToken).Select(x => x.AddLess).FirstOrDefault();
            sql = @"Select Top 1 " + PosNeg + Val + " from TfatComp";
            DataTable smDt = GetDataTable(sql, connstring);
            if (smDt.Rows.Count > 0)
            {
                mamtm = (smDt.Rows[0][0].ToString() == "" || smDt.Rows[0][0].ToString() == null) ? 0 : Convert.ToDouble(smDt.Rows[0][0]);
            }
            else
            {
                mamtm = 0;
            }
            return mamtm;
        }

        public ActionResult DeleteStateMaster(ContractMasterVM model)
        {
            if (model.Document == null || model.Document == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var conMaster = ctxTFAT.ConMaster.Where(x => x.Code == model.Document).FirstOrDefault();
            var mDeleteConMasterRel = ctxTFAT.ConMasterRel.Where(x => x.Code == model.Document).ToList();
            var ChargeCodeList = mDeleteConMasterRel.Select(x => x.ChargCode).ToList();
            var mDeleteConDetail = ctxTFAT.ConDetail.Where(x => ChargeCodeList.Contains(x.Code)).ToList();
            ctxTFAT.ConMaster.Remove(conMaster);
            ctxTFAT.ConMasterRel.RemoveRange(mDeleteConMasterRel);
            ctxTFAT.ConDetail.RemoveRange(mDeleteConDetail);
            ctxTFAT.SaveChanges();
            return Json(new
            {
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Copy(string DocNo,string CustType,string CustCode)
        {
            List<CopyContractVM> List = new List<CopyContractVM>();

            string Parent = "";
            //string Child = "";
            List<string> Child = new List<string>();
            if (CustType=="CU")
            {
                Parent = ctxTFAT.CustomerMaster.Where(x => x.Code == CustCode).Select(x => x.AccountParentGroup).FirstOrDefault();
                foreach (var item in ctxTFAT.CustomerMaster.Where(x=>x.AccountParentGroup==Parent).Select(x=>x.Code).ToList())
                {
                    //Child += "'" + item + "',";
                    Child.Add(item);
                }
                //Child = Child.Substring(0, Child.Length - 1);
            }
            else
            {
                Parent = CustCode;
                foreach (var item in ctxTFAT.CustomerMaster.Where(x => x.AccountParentGroup == Parent).Select(x => x.Code).ToList())
                {
                    //Child += "'" + item + "',";
                    Child.Add(item);
                }
                //Child = Child.Substring(0, Child.Length - 1);
            }

            var Mobj = (from ConMaster in ctxTFAT.ConMaster
                        where ConMaster.Code != DocNo && ((ConMaster.CustType=="CU" && Child.Contains(ConMaster.Cust)) || (ConMaster.CustType == "MA" && ConMaster.Cust==Parent) || (ConMaster.Cust== "General"))
                        select new CopyContractVM
                        {
                            DocumentNo = ConMaster.Code,
                            FromDate = ConMaster.FromDt,
                            TODate = ConMaster.ToDt,
                            CustomerType = ConMaster.CustType == "MA" ? "Master" : "Customer",
                            CustomerName = ConMaster.Cust== "General"? "General": ConMaster.CustType == "MA" ? ctxTFAT.Master.Where(x => x.Code == ConMaster.Cust).Select(x => x.Name).FirstOrDefault() : ctxTFAT.CustomerMaster.Where(x=>x.Code==ConMaster.Cust).Select(x=>x.Name).FirstOrDefault(),
                        }).ToList();
            if (Mobj == null)
            {
                Mobj = new List<CopyContractVM>();
            }
            var html = ViewHelper.RenderPartialView(this, "ListOFContractMaster", Mobj);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult CopyContractMaster(ContractMasterVM Model)
        {
            ContractMasterVM masterVM = new ContractMasterVM();
            List<ContractList> objledgerdetail = new List<ContractList>();
            if (Session["ContractMasterVM"] != null)
            {
                masterVM = (ContractMasterVM)Session["ContractMasterVM"];
                if (masterVM.ContractList != null)
                {
                    objledgerdetail = masterVM.ContractList;
                }
            }

            var mobj = (from ConDetail in ctxTFAT.ConDetail
                        where ConDetail.CustCode == Model.copyDocument
                        orderby ConDetail.Services
                        select new ContractList()
                        {
                            ContractType = ConDetail.ContrType,
                            Service = ConDetail.Services,
                            ServiceN = ConDetail.ContrType == "ChargeType" ? ctxTFAT.ChargeTypeMaster.Where(x => x.Code.Trim() == ConDetail.Services.Trim()).Select(x => x.ChargeType).FirstOrDefault() : ctxTFAT.DescriptionMaster.Where(x => x.Code.Trim() == ConDetail.Services.Trim()).Select(x => x.Description).FirstOrDefault(),
                            Sno = ConDetail.SrNo,
                            TypeOfChrg = ConDetail.WtType,
                            FromWT = ConDetail.Wtfrom.Value,
                            ToWT = ConDetail.WtTo.Value,
                            Rate = (float)ConDetail.Rate,
                            ChargeONN = ConDetail.ChgOn == true ? "Charge On Chargeble WT " : "Charge On Actual WT",
                            ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                            ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                            UniqueKey = ConDetail.Services.Trim() + ConDetail.SrNo.ToString().Trim(),
                            ConDetilsCode = ConDetail.Code,
                        }).ToList();

            foreach (var item in mobj)
            {
                item.Charges = GetChargesOfService(item.ConDetilsCode, item.Service, item.Sno);
            }

            masterVM.ContractList = mobj;

            Session.Add("ContractMasterVM", masterVM);
            var html = ViewHelper.RenderPartialView(this, "ContractList", masterVM);
            return Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
        }
    }
}