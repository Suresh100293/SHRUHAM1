using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using ALT_ERP3.Areas.Accounts.Models;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class ConsignerOrConsigneeController : BaseController
    {

        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;

        #region GetLists
        public JsonResult AutoCompleteCountry(string term)
        {
            return Json((from m in ctxTFAT.TfatCountry
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code.ToString() }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteState(string term)
        {
            var mPara = term.Split('^');
            var mP1 = mPara[0].ToLower();
            var mP2 = mPara[1];
            // linq doesnt support array in query, so parameters are stored in var
            return Json((from m in ctxTFAT.TfatState
                         where m.Country == mP2 && m.Name.ToLower().Contains(mP1)
                         select new { Name = m.Name, Code = m.Code.ToString() }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteCity(string term)
        {
            var mPara = term.Split('^');
            var mP1 = mPara[0].ToLower();
            var mP2 = mPara[1];
            // linq doesnt support array in query, so parameters are stored in var
            return Json((from m in ctxTFAT.TfatCity
                         where m.State == mP2 && m.Name.ToLower().Contains(mP1)
                         select new { Name = m.Name, Code = m.Code.ToString() }).ToArray(), JsonRequestBehavior.AllowGet);
        }
        private List<SelectListItem> PopulateBranches()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            var list = ctxTFAT.TfatBranch.Where(x => x.Status == true && (x.Category == "Zone" || x.Category == "Branch" || x.Category == "SubBranch")).ToList();
            foreach (var item in list)
            {
                string Alias = item.Category == "Zone" ? "Z" : item.Category == "Branch" ? "B" : "SB";

                items.Add(new SelectListItem
                {
                    Text = item.Name.ToString() + " - " + Alias,
                    Value = item.Code.ToString()
                });
            }
            return items;
        }
        public JsonResult GetBillingParty(string term)//BillingParty
        {
            var list = ctxTFAT.CustomerMaster.Where(x => x.Hide == false).ToList();
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


        #endregion

        #region TreeView

        public string TreeView(string Mode, string Document)
        {
            string BranchCode = "";
            string[] BranchArray = new string[100];
            if (Mode == "Add")
            {
                BranchCode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
            }
            else
            {
                var Branchlist = ctxTFAT.Consigner.Where(x => x.Code == Document).Select(x => x.Branch).FirstOrDefault();
                BranchArray = Branchlist.ToString().Split(',');
            }

            var mTreeList = ctxTFAT.TfatBranch.Where(x => x.Category != "Area").Select(x => new { x.Name, x.Grp, x.Code, x.Category }).ToList();

            List<NFlatObject> flatObjects2 = new List<NFlatObject>();
            for (int n = 0; n < mTreeList.Count; n++)
            {
                string alias = "";
                NFlatObject abc = new NFlatObject();
                if (mTreeList[n].Category == "Zone")
                {
                    alias = mTreeList[n].Name + "-" + "Z";
                }
                else if (mTreeList[n].Category == "Branch")
                {
                    alias = mTreeList[n].Name + "-" + "B";
                }
                else if (mTreeList[n].Category == "SubBranch")
                {
                    alias = mTreeList[n].Name + "-" + "SB";
                }
                else
                {
                    alias = mTreeList[n].Name + "-" + "HO";
                }

                abc.data = alias;
                abc.Id = mTreeList[n].Code;
                if (Mode == "Add")
                {
                    if (BranchCode == abc.Id)
                    {
                        abc.isSelected = true;
                    }
                }
                else
                {
                    if (BranchArray.Contains(abc.Id))
                    {
                        abc.isSelected = true;
                    }
                }

                if (mTreeList[n].Code == mTreeList[n].Grp)
                {
                    abc.ParentId = "0";
                }
                else
                {
                    abc.ParentId = mTreeList[n].Grp;
                }
                flatObjects2.Add(abc);
            }
            var recursiveObjects = FillRecursive1(flatObjects2, "0");
            string myjsonmodel = new JavaScriptSerializer().Serialize(recursiveObjects);
            ViewBag.Tree = myjsonmodel;
            return myjsonmodel;

        }

        public string CheckUncheckTree(string Check)
        {
            var mTreeList = ctxTFAT.TfatBranch.Where(x => x.Category != "Area").Select(x => new { x.Name, x.Grp, x.Code, x.Category }).ToList();
            string alias = "";
            List<NFlatObject> flatObjects2 = new List<NFlatObject>();
            for (int n = 0; n < mTreeList.Count; n++)
            {
                NFlatObject abc = new NFlatObject();
                if (mTreeList[n].Category == "Zone")
                {
                    alias = mTreeList[n].Name + "-" + "Z";
                }
                else if (mTreeList[n].Category == "Branch")
                {
                    alias = mTreeList[n].Name + "-" + "B";
                }
                else if (mTreeList[n].Category == "SubBranch")
                {
                    alias = mTreeList[n].Name + "-" + "SB";
                }
                else
                {
                    alias = mTreeList[n].Name + "-" + "HO";
                }
                abc.data = alias;
                abc.Id = mTreeList[n].Code;
                if (Check == "Check")
                {
                    abc.isSelected = true;
                }
                if (mTreeList[n].Code == mTreeList[n].Grp)
                {
                    abc.ParentId = "0";
                }
                else
                {
                    abc.ParentId = mTreeList[n].Grp;
                }
                flatObjects2.Add(abc);
            }
            var recursiveObjects = FillRecursive1(flatObjects2, "0");
            string myjsonmodel = new JavaScriptSerializer().Serialize(recursiveObjects);
            ViewBag.Tree = myjsonmodel;
            return myjsonmodel;

        }

        public static List<NRecursiveObject> FillRecursive1(List<NFlatObject> flatObjects, string parentId)
        {
            bool mSelected = false;
            List<NRecursiveObject> recursiveObjects = new List<NRecursiveObject>();
            foreach (var item in flatObjects.Where(x => x.ParentId.Equals(parentId)))
            {
                mSelected = item.isSelected;
                recursiveObjects.Add(new NRecursiveObject
                {
                    data = item.data,
                    id = item.Id,
                    attr = new NFlatTreeAttribute { id = item.Id.ToString(), selected = item.isSelected },
                    children = FillRecursive1(flatObjects, item.Id)
                });
            }
            return recursiveObjects;
        }
        #endregion

        // GET: Logistics/ConsignerOrConsignee
        public ActionResult Index(ConsignerMasterVM mModel)
        {
            Session["AddInfo"] = null;
            List<ConsignerMasterVM> AddressList1 = new List<ConsignerMasterVM>();
            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());

            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");

            mdocument = mModel.Document;

            //mModel.Branches = PopulateBranches();

            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.Consigner.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                //var CityCode = Convert.ToInt32(mList.City);
                var City = ctxTFAT.TfatCity.Where(x => x.Code == mList.City).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                var State = ctxTFAT.TfatState.Where(x => x.Code.ToString() == mList.State).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                if (mList != null)
                {
                    mModel.SrNo = 0;
                    mModel.Code = mList.Code;
                    mModel.Name = mList.Name;
                    mModel.DuplicateName = mList.Name;
                    mModel.Address1 = mList.Addr1;
                    mModel.Address2 = mList.Addr2;
                    //mModel.Branch = mList.Branch;
                    mModel.CustomerCode = mList.Customer;
                    mModel.CustomerName = ctxTFAT.Master.Where(x => x.Code == mModel.CustomerCode).Select(x => x.Name).FirstOrDefault();


                    if (City != null)
                    {
                        mModel.City = City.Code.ToString();
                        mModel.CityName = City.Name.ToString();
                    }
                    if (State != null)
                    {
                        mModel.State = State.Code.ToString();
                        mModel.StateName = State.Name.ToString();
                    }

                    mModel.RemarkReq = mList.RemarkReq;
                    mModel.Remark = mList.Remark;
                    mModel.HoldReq = mList.HoldReq;
                    mModel.HoldRemark = mList.HoldRemark;


                    mModel.Fax = mList.Fax;

                    mModel.Acitve = mList.Acitve;

                    //mModel.Area = mList.Area;

                    mModel.TickLrConsignor = mList.TickLrConsignor;
                    mModel.TickLrConsignee = mList.TickLrConsignee;
                    mModel.HoldTickLrConsignor = mList.HoldTickLrConsignor;
                    mModel.HoldTickLrConsignee = mList.HoldTickLrConsignee;

                    var address = ctxTFAT.ConsignerAddress.Where(x => x.Code == mModel.Document).Select(x => x).ToList();
                    foreach (var mobj1 in address)
                    {
                        AddressList1.Add(new ConsignerMasterVM
                        {
                            SrNo = mobj1.Sno,
                            Address1 = mobj1.Addr1,
                            Address2 = mobj1.Addr2,
                            MobileNo = mobj1.Mobile,
                            Email = mobj1.Email,
                            Tel1 = mobj1.Tel1,
                            Tel2 = mobj1.Tel2,
                            Tel3 = mobj1.Tel3,
                            Country = mobj1.Country,
                            CountryName = ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == mobj1.Country).Select(x => x.Name).FirstOrDefault(),
                            State = mobj1.State,
                            StateName = ctxTFAT.TfatState.Where(x => x.Code.ToString() == mobj1.State).Select(x => x.Name).FirstOrDefault(),
                            City = mobj1.City,
                            CityName = ctxTFAT.TfatCity.Where(x => x.Code.ToString() == mobj1.City).Select(x => x.Name).FirstOrDefault(),
                            Pin = mobj1.Pin,
                            GSTNo = mobj1.GSTNo,
                            PanNo = mobj1.PanNo,
                            ContactPersonName = mobj1.ContPersonNmae,
                            AllSendEmail = mobj1.AllSendEmail,
                            AllSendSMS = mobj1.AllSendSMS,
                        });
                        mModel.AddressList = AddressList1;
                        Session.Add("AddInfo", AddressList1);
                    }
                }
            }
            else
            {
                TfatComp tfatComp = ctxTFAT.TfatComp.Where(x => x.Code == mcompcode).FirstOrDefault();
                var countryname = ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == tfatComp.Country).FirstOrDefault();
                var statename = ctxTFAT.TfatState.Where(x => x.Code.ToString() == tfatComp.State).FirstOrDefault();
                var cityname = ctxTFAT.TfatCity.Where(x => x.Code.ToString() == tfatComp.City).FirstOrDefault();

                mModel.Name = "";
                mModel.Acitve = true;
                mModel.SrNo = 0;
                int NewCode1;
                var NewCode = ctxTFAT.Consigner.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).Take(1).FirstOrDefault();
                if (NewCode == null || NewCode == "")
                {
                    NewCode1 = 100000;
                }
                else
                {
                    NewCode1 = Convert.ToInt32(NewCode) + 1;
                }
                string FinalCode = NewCode1.ToString("D6");
                mModel.Code = FinalCode;

                AddressList1.Add(new ConsignerMasterVM
                {
                    SrNo = 0,
                    ContactPersonName = "",
                    Address1 = "",
                    Address2 = "",
                    Email = "",
                    MobileNo = "",
                    Tel1 = "",
                    Tel2 = "",
                    Tel3 = "",
                    Country = countryname.Code.ToString(),
                    CountryName = countryname.Name,
                    State = statename.Code.ToString(),
                    StateName = statename.Name,
                    City = cityname.Code,
                    CityName = cityname.Name,
                    Pin = "0",
                    GSTNo = "",
                    PanNo = "",
                });
                mModel.AddressList = AddressList1;
                Session.Add("AddInfo", AddressList1);
            }
            return View(mModel);
        }

        private string GetPrefix(DateTime mDate)
        {
            string mstr = "";
            var perdstring = ctxTFAT.TfatPerd.Select(x => x).OrderBy(x => x.StartDate).FirstOrDefault();
            string p1 = perdstring.PerdCode.Substring(2, 2);
            string p2 = perdstring.PerdCode.Substring(6, 2);

            int d10 = Convert.ToInt16(perdstring.PerdCode.Substring(0, 2));
            d10 = d10 - 1;
            string d1 = d10.ToString();

            int d20 = Convert.ToInt16(perdstring.PerdCode.Substring(4, 2));
            d20 = d20 - 1;
            string d2 = d20.ToString();
            mstr = d1 + p1 + d2 + p2;
            return (mstr);
        }

        #region SaveData

        public void DeUpdate(ConsignerMasterVM Model)
        {
            var list = ctxTFAT.ConsignerAddress.Where(x => x.Code == Model.Document).ToList();
            if (list != null)
            {
                ctxTFAT.ConsignerAddress.RemoveRange(list);
            }
            ctxTFAT.SaveChanges();
        }

        public ActionResult SaveData(ConsignerMasterVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    bool Status = false;
                    if (mModel.AcitveorNot.ToString() == "True")
                    {
                        Status = true;
                    }
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        var MSG = DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return MSG;
                    }
                    Consigner mobj = new Consigner();
                    bool mAdd = true;
                    if (ctxTFAT.Consigner.Where(x => (x.Code == mModel.Document)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.Consigner.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                        DeUpdate(mModel);
                        mAdd = false;
                    }

                    //mobj.ShortName = mModel.ShortName;
                    mobj.Name = mModel.Name;
                    //mobj.ContactName = mModel.Contact_Person;
                    //mobj.ContactName2 = mModel.Contact_Person1;
                    //mobj.Addr1 = mModel.Address1;
                    //mobj.Addr2 = mModel.Address2;
                    //mobj.City = mModel.City;
                    //mobj.State = mModel.State;

                    //mobj.Pincode = mModel.Pincode == null ? 0 : (Int32)mModel.Pincode;
                    //mobj.District = mModel.District;
                    //mobj.ContactNO = (mModel.Contact_No);
                    //mobj.ContactNO2 = (mModel.Contact_No1);
                    //mobj.Fax = mModel.Fax;
                    //mobj.Email = mModel.Email_Id;
                    //mobj.Email2 = mModel.Email_Id1;
                    mobj.Acitve = Status;
                    //long recordkey = Convert.ToInt64(mModel.Area);
                    //mobj.Area = ctxTFAT.TfatBranch.Where(x => x.RECORDKEY == recordkey).Select(m => m.Code).FirstOrDefault();
                    mobj.Branch = mModel.Branch;
                    mobj.Customer = mModel.CustomerCode;
                    mobj.RemarkReq = mModel.RemarkReq;
                    mobj.Remark = mModel.Remark;
                    mobj.HoldReq = mModel.HoldReq;
                    mobj.HoldRemark = mModel.HoldRemark;
                    //mobj.PAN = mModel.PAN;
                    //mobj.GST = mModel.GST;
                    mobj.TickLrConsignor = mModel.TickLrConsignor;
                    mobj.TickLrConsignee = mModel.TickLrConsignee;
                    mobj.HoldTickLrConsignor = mModel.HoldTickLrConsignor;
                    mobj.HoldTickLrConsignee = mModel.HoldTickLrConsignee;




                    //// iX9: default values for the fields not used @Form
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                    if (mAdd == true)
                    {
                        mobj.CreateDate = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                        int NewCode1;
                        var NewCode = ctxTFAT.Consigner.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).Take(1).FirstOrDefault();
                        if (NewCode == null || NewCode == "")
                        {
                            NewCode1 = 100000;
                        }
                        else
                        {
                            NewCode1 = Convert.ToInt32(NewCode) + 1;
                        }
                        string FinalCode = NewCode1.ToString("D6");
                        mobj.Code = FinalCode;
                        ctxTFAT.Consigner.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }

                    var result = (List<ConsignerMasterVM>)Session["AddInfo"];
                    if (result == null)
                    {
                        result = new List<ConsignerMasterVM>();
                    }

                    foreach (var item in result)
                    {
                        ConsignerAddress consignerAddress = new ConsignerAddress();
                        consignerAddress.Code = mobj.Code;
                        consignerAddress.Name = mobj.Name;
                        consignerAddress.Addr1 = item.Address1;
                        consignerAddress.Addr2 = item.Address2;
                        consignerAddress.Email = item.Email;
                        consignerAddress.Mobile = item.MobileNo;
                        consignerAddress.Tel1 = item.Tel1;
                        consignerAddress.Tel2 = item.Tel2;
                        consignerAddress.Tel3 = item.Tel3;
                        consignerAddress.Country = item.Country;
                        consignerAddress.State = item.State;
                        consignerAddress.City = item.City;
                        consignerAddress.Pin = item.Pin;
                        consignerAddress.GSTNo = item.GSTNo;
                        consignerAddress.PanNo = item.PanNo;
                        consignerAddress.Sno = item.SrNo == null ? 0 : item.SrNo.Value;
                        consignerAddress.ContPersonNmae = String.IsNullOrEmpty(item.ContactPersonName) == true ? "" : item.ContactPersonName;
                        consignerAddress.AllSendEmail = item.AllSendEmail;
                        consignerAddress.AllSendSMS = item.AllSendSMS;

                        consignerAddress.AUTHIDS = muserid;
                        consignerAddress.AUTHORISE = mauthorise;
                        consignerAddress.ENTEREDBY = muserid;
                        consignerAddress.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                        ctxTFAT.ConsignerAddress.Add(consignerAddress);
                    }

                    ctxTFAT.SaveChanges();
                    mnewrecordkey = Convert.ToInt32(mobj.RECORDKEY);
                    string mNewCode = "";
                    mNewCode = mobj.Code;
                    transaction.Commit();
                    transaction.Dispose();
                    var doct = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                    var Prefix = GetPrefix(doct);
                    var parentkey = "Consi" + Prefix.Substring(0, 2) + mobj.Code;
                    SendSMS_MSG_EmailOfMaster(mModel.Mode, 0, mbranchcode + parentkey, DateTime.Now, mobj.Name, "NA");
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, parentkey, DateTime.Now, 0, mNewCode, "Save Consignor / Consignee Master", "CONS");

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

        public ActionResult DeleteStateMaster(ConsignerMasterVM mModel)
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
            var Default = ctxTFAT.Consigner.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
            if (Default != null)
            {
                if (Default.Code.Trim() == "100000" || Default.Code.Trim() == "100001")
                {
                    mactivestring = mactivestring + "\nCant  Allow To Delete This Is Default Value:: " + Default.Name;
                }
            }
            var mactive1 = ctxTFAT.LRMaster.Where(x => x.RecCode == mModel.Document || x.SendCode == mModel.Document).FirstOrDefault();
            if (mactive1 != null)
            {
                mactivestring = mactivestring + "\nLRNO: " + mactive1.LrNo;
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
                    var mList = ctxTFAT.Consigner.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                    ctxTFAT.Consigner.Remove(mList);

                    var mListlist = ctxTFAT.ConsignerAddress.Where(x => (x.Code == mModel.Document)).ToList();
                    if (mListlist != null)
                    {
                        ctxTFAT.ConsignerAddress.RemoveRange(mListlist);
                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.Document, DateTime.Now, 0, mModel.Document, "Delete Consignor / Consignee Master", "CONS");

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


        #region Multiple Address 

        public ActionResult GetAddInfo(ConsignerMasterVM Model)
        {
            var result = (List<ConsignerMasterVM>)Session["AddInfo"];
            if (result == null)
            {
                result = new List<ConsignerMasterVM>();
            }
            var result1 = result.Where(x => x.SrNo == Model.SrNo);
            foreach (var item in result1)
            {
                Model.ContactPersonName = item.ContactPersonName;
                Model.Address1 = item.Address1;
                Model.Address2 = item.Address2;
                Model.Email = item.Email;
                Model.MobileNo = item.MobileNo;
                Model.Tel1 = item.Tel1;
                Model.Tel2 = item.Tel2;
                Model.Tel3 = item.Tel3;
                Model.Country = item.Country;
                Model.CountryName = item.CountryName;
                Model.State = item.State;
                Model.StateName = item.StateName;
                Model.City = item.City;
                Model.CityName = item.CityName;
                Model.Pin = item.Pin;
                Model.GSTNo = item.GSTNo;
                Model.PanNo = item.PanNo;
                Model.AllSendEmail = item.AllSendEmail;
                Model.AllSendSMS = item.AllSendSMS;
            }
            Model.AddressList = result;
            return Json(new { Html = this.RenderPartialView("MailingInfo", Model) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveAddInfo(ConsignerMasterVM Model)
        {
            int srno;
            List<ConsignerMasterVM> MailInformation = new List<ConsignerMasterVM>();
            try
            {
                if (Session["AddInfo"] != null)
                {
                    MailInformation = (List<ConsignerMasterVM>)Session["AddInfo"];
                }
                if (MailInformation == null)
                {
                    MailInformation = new List<ConsignerMasterVM>();
                }
                var Comp = ctxTFAT.TfatComp.Where(x => x.Code == mcompcode).FirstOrDefault();
                var countryname = ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == Comp.Country).FirstOrDefault();
                var statename = ctxTFAT.TfatState.Where(x => x.Code.ToString() == Comp.State).FirstOrDefault();
                var cityname = ctxTFAT.TfatCity.Where(x => x.Code.ToString() == Comp.City).FirstOrDefault();

                srno = MailInformation.Count();
                MailInformation.Add(new ConsignerMasterVM()
                {

                    ContactPersonName = "",
                    SrNo = srno,
                    Address1 = "",
                    Address2 = "",
                    Country = countryname.Code.ToString(),
                    CountryName = countryname.Name.ToString(),
                    State = statename.Code.ToString(),
                    StateName = statename.Name.ToString(),
                    City = cityname.Code.ToString(),
                    CityName = cityname.Name.ToString(),
                    Email = "",
                    MobileNo = "",
                    Tel1 = "",
                    Tel2 = "",
                    Tel3 = "",
                    Pin = "0",
                    GSTNo = "",
                    PanNo = "",
                });
                Model.Country = countryname.Code.ToString();
                Model.CountryName = countryname.Name.ToString();
                Model.State = statename.Code.ToString();
                Model.StateName = statename.Name.ToString();
                Model.City = cityname.Code.ToString();
                Model.CityName = cityname.Name.ToString();
                Session.Add("AddInfo", MailInformation);
                Model.AddressList = MailInformation;
                Model.SrNo = srno;
            }
            catch (DbEntityValidationException ex1)
            {
                string dd1 = ex1.InnerException.Message;
            }
            var html = ViewHelper.RenderPartialView(this, "MailingInfo", Model);
            return Json(new { MailList = MailInformation, Html = html }, JsonRequestBehavior.AllowGet);
            //return Json(new { Html = this.RenderPartialView("MailingInfo", Model) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditAddData(ConsignerMasterVM Model)
        {
            List<ConsignerMasterVM> MailInformation1 = new List<ConsignerMasterVM>();
            List<ConsignerMasterVM> MailInformation = new List<ConsignerMasterVM>();
            try
            {
                if (Session["AddInfo"] != null)
                {
                    MailInformation = (List<ConsignerMasterVM>)Session["AddInfo"];
                }
                if (MailInformation == null)
                {
                    MailInformation = new List<ConsignerMasterVM>();
                }
                foreach (var item in MailInformation.Where(x => x.SrNo == Model.SrNo))
                {
                    item.ContactPersonName = Model.ContactPersonName;
                    item.Address1 = Model.Address1;
                    item.Address2 = Model.Address2;
                    item.Email = Model.Email;
                    item.MobileNo = Model.MobileNo;
                    item.Tel1 = Model.Tel1;
                    item.Tel2 = Model.Tel2;
                    item.Tel3 = Model.Tel3;
                    item.Pin = Model.Pin;
                    item.GSTNo = Model.GSTNo;
                    item.PanNo = Model.PanNo;
                    item.AllSendEmail = Model.AllSendEmail;
                    item.AllSendSMS = Model.AllSendSMS;


                    item.Country = Model.Country;
                    item.CountryName = ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == Model.Country).Select(x => x.Name).FirstOrDefault();
                    Model.Country = Model.Country;
                    Model.CountryName = item.CountryName;

                    item.State = Model.State;
                    item.StateName = ctxTFAT.TfatState.Where(x => x.Code.ToString() == Model.State).Select(x => x.Name).FirstOrDefault();
                    Model.State = Model.State;
                    Model.StateName = item.StateName;

                    item.City = Model.City;
                    item.CityName = ctxTFAT.TfatCity.Where(x => x.Code.ToString() == Model.City).Select(x => x.Name).FirstOrDefault();
                    Model.City = Model.City;
                    Model.CityName = item.CityName;
                };
                Session.Add("AddInfo", MailInformation);
                Model.AddressList = MailInformation;
            }
            catch (DbEntityValidationException ex1)
            {
                string dd1 = ex1.InnerException.Message;
            }
            var html = ViewHelper.RenderPartialView(this, "MailingInfo", Model);
            return Json(new { MailList = MailInformation, Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteAddData(ConsignerMasterVM Model)
        {
            List<ConsignerMasterVM> MailInformation1 = new List<ConsignerMasterVM>();
            List<ConsignerMasterVM> MailInformation = new List<ConsignerMasterVM>();
            try
            {
                if (Session["AddInfo"] != null)
                {
                    MailInformation = (List<ConsignerMasterVM>)Session["AddInfo"];
                }
                if (MailInformation == null)
                {
                    MailInformation = new List<ConsignerMasterVM>();
                }
                MailInformation1 = MailInformation.Where(x => x.SrNo != Model.SrNo).ToList();
                int I = 0;
                foreach (var item in MailInformation1)
                {
                    item.SrNo = I++;
                }
                Session.Add("AddInfo", MailInformation1);
                Model.AddressList = MailInformation1;
            }
            catch (DbEntityValidationException ex1)
            {
                string dd1 = ex1.InnerException.Message;
            }
            var html = ViewHelper.RenderPartialView(this, "MailingInfo", Model);
            return Json(new { MailList = MailInformation, Html = html }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        //public string ConvertDDMMYYTOYYMMDD(string da)
        //{
        //    string abc = da.Substring(6, 4) + "/" + da.Substring(3, 2) + "/" + da.Substring(0, 2);
        //    return abc;
        //}
    }
}