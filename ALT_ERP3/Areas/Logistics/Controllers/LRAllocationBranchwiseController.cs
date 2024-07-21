using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Data;
using Common;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class LRAllocationBranchwiseController : BaseController
    {

        private static string mAUTHORISE = "A00";
        private static string mdocument = "";
        private int mnewRECORDKEY = 0;
        //private string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        private new string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private new string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        public new string muserid = System.Web.HttpContext.Current.Session["UserId"].ToString();

        public JsonResult Branch(string term)
        {
            List<TfatBranch> list = ctxTFAT.TfatBranch.Where(x => x.Category != "Zone" && x.Code != "G00000" && x.Category != "Area" && x.Status == true && x.Grp != "G00000").ToList();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }

            List<TfatBranch> treeTables = new List<TfatBranch>();

            foreach (var item in list)
            {
                if (item.Category == "Zone")
                {
                    item.Name += " - Z";
                    treeTables.Add(item);
                }
                else if (item.Category == "Branch")
                {
                    item.Name += " - B";
                    treeTables.Add(item);
                }
                else if (item.Category == "SubBranch")
                {
                    item.Name += " - SB";
                    treeTables.Add(item);
                }
                else if (item.Category == "0")
                {
                    item.Name += " - HO";
                    treeTables.Add(item);
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

        public int GetNewCode()
        {
            int Code = ctxTFAT.TblBranchAllocation.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).Take(1).FirstOrDefault();
            if (Code == 0)
            {
                return 1;
            }
            else
            {
                return (Convert.ToInt32(Code) + 1);
            }
        }

        public ActionResult CheckRange(int From, int To, string Branch, string Document,string TRN)
        {
            string Flag = "F";
            string Msg = "";
            var UserRange = Enumerable.Range(From, (To - From) + 1);
            bool checkalloctionFound = false;
            bool SetupValid = false;
            var Setuptype = "";
            if (TRN == "LR000")
            {
                if (ctxTFAT.LRSetup.FirstOrDefault() != null)
                {
                    SetupValid = true;
                    if (ctxTFAT.LRSetup.Select(x => x.CetralisedManualSrlReq).FirstOrDefault())
                    {
                        Setuptype = "C";
                    }
                    else
                    {
                        Setuptype = "Y";
                    }
                }
            }
            else if (TRN == "LC000")
            {
                if (ctxTFAT.LCSetup.FirstOrDefault() != null)
                {
                    SetupValid = true;
                    if (ctxTFAT.LCSetup.Select(x => x.CetralisedManualSrlReq).FirstOrDefault())
                    {
                        Setuptype = "C";
                    }
                    else
                    {
                        Setuptype = "Y";
                    }
                }
            }
            else if (TRN == "FM000")
            {
                if (ctxTFAT.FMSetup.FirstOrDefault() != null)
                {
                    SetupValid = true;
                    if (ctxTFAT.FMSetup.Select(x => x.CetralisedManualSrlReq).FirstOrDefault())
                    {
                        Setuptype = "C";
                    }
                    else
                    {
                        Setuptype = "Y";
                    }
                }
            }

            if (SetupValid)
            {
                List<TblLrAllocation> tblLrAllocations = new List<TblLrAllocation>();
                if (Setuptype == "C")
                {
                    tblLrAllocations = ctxTFAT.TblLrAllocation.Where(x => x.Code.ToString() != Document && x.TRN_Type == TRN).ToList();
                }
                else
                {
                    tblLrAllocations = ctxTFAT.TblLrAllocation.Where(x => x.Prefix == mperiod && x.Code.ToString() != Document && x.TRN_Type == TRN).ToList();
                }
                foreach (var item in tblLrAllocations)
                {
                    var ExistingRange = Enumerable.Range(item.AllocateFrom, (item.AllocateTo - item.AllocateFrom) + 1);
                    if ((item.AllocateFrom <= From && To <= item.AllocateTo))
                    {
                        checkalloctionFound = true;
                        break;
                    }
                }

                if (checkalloctionFound)
                {
                    List<TblBranchAllocation> tblBranchAllocations = ctxTFAT.TblBranchAllocation.Where(x => x.Prefix == mperiod && x.Code.ToString() != Document && x.TRN_Type == TRN).ToList();
                    if (Setuptype == "C")
                    {
                        tblBranchAllocations = ctxTFAT.TblBranchAllocation.Where(x => x.Code.ToString() != Document && x.TRN_Type == TRN).ToList();
                    }
                    else
                    {
                        tblBranchAllocations = ctxTFAT.TblBranchAllocation.Where(x => x.Prefix == mperiod && x.Code.ToString() != Document && x.TRN_Type == TRN).ToList();
                    }

                    foreach (var item in tblBranchAllocations)
                    {
                        var ExistingRange = Enumerable.Range(item.ManualFrom, (item.ManualTo - item.ManualFrom) + 1);
                        var commonItems = UserRange.Intersect(ExistingRange);
                        if (commonItems.Count() >= 1)
                        {
                            Flag = "T";
                            Msg = "This Range Already Use It........!";
                            break;
                        }
                    }
                }
                else
                {
                    Flag = "T";
                    Msg = "Range Not Found....!";
                }
            }
            else
            {
                Flag = "T";
                Msg = "Please Create SetUp First...!";
            }

            return Json(new { Message = Flag, Msg = Msg, JsonRequestBehavior.AllowGet });
        }



        // GET: Logistics/LRAllocationBranchwise
        public ActionResult Index(GridOption Model)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());

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
            Model.ToDate = DateTime.Today.ToShortDateString();

            return View(Model);
        }
        
        public ActionResult GetFormats()
        {
            var result = ctxTFAT.ReportHeader.Where(x => x.SubCodeOf == msubcodeof && !x.Code.EndsWith(".bak")).Select(m => new
            {
                Code = m.Code,
                Name = m.Code
            }).OrderBy(n => n.Code).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
            return GetGridDataColumns(id, "L", "XXXX");
        }

        public ActionResult GetMasterGridData(GridOption Model)
        {
            Model.Date = System.Web.HttpContext.Current.Session["StartDate"].ToString() + ":" + System.Web.HttpContext.Current.Session["LastDate"].ToString(); ;

            if (Model.IsFormatSelected == true)
            {
                int noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=0 Where SubCodeOf='" + msubcodeof + "'");
                noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=-1 Where Code='" + Model.ViewDataId + "'");
            }
            return GetGridReport(Model, "M", "MainType^" + Model.MainType, false, 0);
        }

        public ActionResult Index1(LrAllocateBranchVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;

            mModel.Transaction = (TransactionType)Enum.Parse(typeof(TransactionType), mModel.TransactionType);
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                TblBranchAllocation tblLr = ctxTFAT.TblBranchAllocation.Where(x => x.Code.ToString() == mModel.Document).FirstOrDefault();
                mModel.Code = tblLr.Code.ToString("D6");
                mModel.Branch = tblLr.Branch;
                mModel.BranchName = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.Branch).Select(x => x.Name).FirstOrDefault();
                mModel.ManualFrom = tblLr.ManualFrom;
                mModel.ManualTo = tblLr.ManualTo;
                mModel.Transaction = (TransactionType)Enum.Parse(typeof(TransactionType), tblLr.TRN_Type);

            }
            else
            {
                mModel.Code = GetNewCode().ToString("D6");
            }

            return View(mModel);
        }
        

        public ActionResult SaveData(LrAllocateBranchVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    #region Check Able To Modify Records

                    if (mModel.Mode != "Add")
                    {
                        TblBranchAllocation tblLr = ctxTFAT.TblBranchAllocation.Where(x => x.Code.ToString() == mModel.Document).FirstOrDefault();
                        

                        string tablename = tblLr.TRN_Type == "LR000" ? "LRMaster" : tblLr.TRN_Type == "LC000" ? "LCMaster" : tblLr.TRN_Type == "FM000" ? "FMMaster" : tblLr.TRN_Type == "LDS" ? "LocalDeliverySheet" : "DeliveryMaster";
                        string ColField = tblLr.TRN_Type == "LR000" ? "LrNo" : tblLr.TRN_Type == "LC000" ? "LCno" : tblLr.TRN_Type == "FM000" ? "FmNo" : tblLr.TRN_Type == "LDS" ? "LDSNo" : "DeliveryNo";

                        string selectquery = "select " + ColField + " as No From " + tablename + " where " + ColField + " between " + tblLr.ManualFrom + " and " + tblLr.ManualTo + "";
                        SqlConnection mConn = new SqlConnection(GetConnectionString());
                        mConn.Open();
                        SqlCommand mcmd = new SqlCommand(selectquery, mConn);
                        SqlDataAdapter sda = new SqlDataAdapter(mcmd);
                        DataTable dt = new DataTable();
                        sda.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            var claimNos = dt.AsEnumerable().Select(s => s.Field<int>("No")).Distinct().ToList();
                            var NewRange = Enumerable.Range(mModel.ManualFrom, (mModel.ManualTo - mModel.ManualFrom) + 1);
                            var commonItems = claimNos.Intersect(NewRange);
                            if (dt.Rows.Count != commonItems.Count())
                            {
                                return Json(new
                                {
                                    Message = "Some Records Generated So Cant Modify....!",
                                    Status = "Error"
                                }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }

                    #endregion
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.Document, DateTime.Now, 0, "", "Delete Document Allocation Branch", "NA");
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    TblBranchAllocation mobj = new TblBranchAllocation();
                    bool mAdd = true;
                    if (ctxTFAT.TblBranchAllocation.Where(x => x.Code.ToString() == mModel.Document).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.TblBranchAllocation.Where(x => x.Code.ToString() == mModel.Document).FirstOrDefault();
                        mAdd = false;
                    }
                    mobj.Code = Convert.ToInt32(mModel.Code);
                    mobj.ManualFrom = mModel.ManualFrom;
                    mobj.ManualTo = mModel.ManualTo;
                    mobj.Branch = mModel.Branch;
                    mobj.TRN_Type = mModel.Transaction.ToString();
                    mobj.Prefix = mperiod;
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mAUTHORISE;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                    if (mAdd == true)
                    {
                        mobj.Date = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                        ctxTFAT.TblBranchAllocation.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mModel.Document, DateTime.Now, 0, "", "Save Document Allocation Branch", "NA");

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
        public ActionResult DeleteStateMaster(LrAllocateBranchVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Not Geting ServiceTypeMaster Data",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var mList = ctxTFAT.TblBranchAllocation.Where(x => x.Code.ToString() == mModel.Document).FirstOrDefault();
            ctxTFAT.TblBranchAllocation.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }

    }
}