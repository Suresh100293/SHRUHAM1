using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Areas.Vehicles.Models;
using ALT_ERP3.Controllers;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using VehicleReportingst = ALT_ERP3.Areas.Vehicles.Models.VehicleReportingst;

namespace ALT_ERP3.Areas.Vehicles.Controllers
{
    public class HireVehicleController : BaseController
    {

        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;

        #region Function

        public JsonResult AutoCompleteVehicleCategory(string term)
        {
            var vehicleCatagorylist = ctxTFAT.VehicleCategory.Where(x => x.Acitve == true).ToList();
            if (!(String.IsNullOrEmpty(term)))
            {
                vehicleCatagorylist = vehicleCatagorylist.Where(x => x.VehicleCategory1.ToLower().Contains(term.ToLower())).ToList();
            }

            var Modified = vehicleCatagorylist.Select(x => new
            {
                Code = x.Code,
                Name = x.VehicleCategory1
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetBroker(string term)
        {
            var result = ctxTFAT.Master.Where(x => x.Hide == false /*&& x.IsLast== true*/ /*&& x.AcType == "S"*/ && x.OthPostType.Contains("B")).Select(m => new { m.Code, m.Name }).ToList();
            if (!(String.IsNullOrEmpty(term)))
            {
                result = ctxTFAT.Master.Where(x => x.Hide == false /*&& x.IsLast== true*/ /*&& x.AcType == "S"*/ && x.OthPostType.Contains("B") && x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).ToList();
            }
            var Modified = result.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetVehicleGroupStatus(string term)
        {
            var list = ctxTFAT.VehicleGrpStatusMas.Where(x => x.Acitve == true && x.Code != "100001").ToList();
            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.VehicleGroupStatus.ToLower().Contains(term.ToLower())).ToList();
            }
            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.VehicleGroupStatus
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DebitAccount(string term)//FMType
        {
            var list = ctxTFAT.Master.Where(x => x.AcType == "X").ToList().Distinct();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = ctxTFAT.Master.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList().Distinct();
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
                //long id = Convert.ToInt64(Document);
                var Branchlist = ctxTFAT.HireVehicleMaster.Where(x => x.Code == Document).Select(x => x.Branch).FirstOrDefault();
                BranchArray = Branchlist.ToString().Split(',');
            }

            var mTreeList = ctxTFAT.TfatBranch.Where(x => x.Category != "Area" && x.Code != "G00000").Select(x => new { x.Name, x.Grp, x.Code, x.Category }).ToList();

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
            var mTreeList = ctxTFAT.TfatBranch.Where(x => x.Category != "Area" && x.Code != "G00000").Select(x => new { x.Name, x.Grp, x.Code, x.Category }).ToList();
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

        // GET: Vehicles/HireVehicle
        public ActionResult Index(VehicleMasterVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());

            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");

            mdocument = mModel.Document;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                //long Recordkey = Convert.ToInt64(mModel.Document);
                var mList = ctxTFAT.HireVehicleMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    mModel.Vehicle_No = mList.TruckNo;
                    mModel.Driver_Name = mList.Driver;
                    mModel.Driver = ctxTFAT.DriverMaster.Where(x => x.Code == mList.Driver).Select(x => x.Name).FirstOrDefault();

                    mModel.VehicleGroup = mList.TruckStatus;
                    mModel.VehicleGroup_Name = ctxTFAT.VehicleGrpStatusMas.Where(x => x.Code == mList.TruckStatus).Select(x => x.VehicleGroupStatus).FirstOrDefault();
                    mModel.Vehicle_Category = mList.VCategory;
                    mModel.Vehicle_Category_Name = ctxTFAT.VehicleCategory.Where(x => x.Code == mList.VCategory).Select(x => x.VehicleCategory1).FirstOrDefault();
                    mModel.Broker_Name = mList.Broker;
                    mModel.PayLoad = Convert.ToInt32(mList.PayLoad);
                    mModel.Broker = ctxTFAT.Master.Where(x => x.Code.ToString() == mList.Broker).Select(x => x.Name).FirstOrDefault();
                    mModel.Acitve = mList.Acitve;
                    mModel.KM = mList.KM;
                    mModel.vehicleReportingSt = (VehicleReportingst)Enum.Parse(typeof(VehicleReportingst), mList.Status);
                    mModel.DriverContactNo = mList.ContactNo;

                    mModel.SpecialRemarkReq = mList.SpclRemarkReq;
                    mModel.SpecialRemark = mList.SpclRemark;
                    mModel.BalckListReq = mList.BlackListReq;
                    mModel.BalckListRemark = mList.BlackListRemark;
                    mModel.Remark = mList.Remark;
                    mModel.ShortName = mList.ShortName;
                    mModel.DebitAcCode = mList.DrAc;
                    mModel.DebitAcName = ctxTFAT.Master.Where(x => x.Code == mList.DrAc).Select(x => x.Name).FirstOrDefault();
                    mModel.CreditAccount = mList.CrAc;

                    mModel.Pick_Vehicle_Rate = mList.PickVehicleRate;
                    mModel.ChangeVehicleFreight_Advance = mList.ChangeVehicleFreight_Advance;

                    mModel.PickDriverTripRate = mList.PickDriverTripRate;
                    mModel.ChangeDriverFreight_Advance = mList.ChangeDriverFreight_Advance;

                    if (mList.PickVehicleRate)
                    {
                        var category = mList.RateType.Split(',');
                        foreach (var item in category)
                        {
                            if (item.Trim() == "V")
                            {
                                mModel.Vehicle_Rate = true;
                            }
                            else if (item.Trim() == "C")
                            {
                                mModel.Category_Rate = true;
                            }
                        }
                    }
                    mModel.Category = mList.RateType;
                    mModel.GetParentAlso = mList.GetParentRateAlso;

                }
            }
            else
            {
                mModel.Pick_Vehicle_Rate = false;
                mModel.Acitve = true;
                mModel.PayLoad = 0;
                mModel.vehicleReportingSt = (VehicleReportingst)Enum.Parse(typeof(VehicleReportingst), "Ready");
                mModel.VehicleGroup = "100001";
                mModel.VehicleGroup_Name = ctxTFAT.VehicleGrpStatusMas.Where(x => x.Code == mModel.VehicleGroup).Select(x => x.VehicleGroupStatus).FirstOrDefault();
                mModel.DebitAcCode = "000100103";
                mModel.DebitAcName = ctxTFAT.Master.Where(x => x.Code == mModel.DebitAcCode).Select(x => x.Name).FirstOrDefault();
                mModel.CreditAccount = "V";
            }
            return View(mModel);
        }
        public string GetNewCode()
        {
            var NewLcNo = ctxTFAT.HireVehicleMaster.OrderByDescending(x => x.Code).Select(x => x.Code).Take(1).FirstOrDefault();
            int LcNo;
            if (String.IsNullOrEmpty(NewLcNo))
            {

                LcNo = 100000;
            }
            else
            {
                NewLcNo = NewLcNo.Replace("H", "");
                LcNo = Convert.ToInt32(NewLcNo) + 1;
            }

            return "H"+LcNo.ToString();
        }
        public ActionResult SaveData(VehicleMasterVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    if (mModel.Mode == "Delete")
                    {
                        DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    HireVehicleMaster mobj = new HireVehicleMaster();
                    bool mAdd = true;
                    //long Recordkey = Convert.ToInt64(mModel.Document);
                    if (ctxTFAT.HireVehicleMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.HireVehicleMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                        mAdd = false;
                    }

                    #region Hire Vehicle Master
                    mobj.TruckNo = mModel.Vehicle_No.ToUpper().Trim();
                    mobj.TruckStatus = mModel.VehicleGroup;
                    mobj.VCategory = mModel.Vehicle_Category;
                    mobj.Broker = mModel.Broker_Name;
                    mobj.Driver = mModel.Driver_Name;
                    mobj.ContactNo = mModel.DriverContactNo;
                    mobj.Acitve = mModel.Acitve;
                    mobj.PayLoad = mModel.PayLoad;
                    mobj.Branch = mModel.Branch;
                    mobj.Status = mModel.vehicleReportingSt.ToString().Trim();
                    mobj.KM = mModel.KM;
                    mobj.SpclRemarkReq = mModel.SpecialRemarkReq;
                    mobj.SpclRemark = mModel.SpecialRemark;
                    mobj.BlackListReq = mModel.BalckListReq;
                    mobj.BlackListRemark = mModel.BalckListRemark;
                    mobj.Remark = mModel.Remark;
                    mobj.ShortName = mModel.ShortName;

                    mobj.DrAc = mModel.DebitAcCode;
                    mobj.CrAc = mModel.CreditAccount;

                    mobj.PickVehicleRate = mModel.Pick_Vehicle_Rate;
                    mobj.PickDriverTripRate = mModel.PickDriverTripRate;
                    mobj.ChangeDriverFreight_Advance = mModel.ChangeDriverFreight_Advance;
                    mobj.ChangeVehicleFreight_Advance = mModel.ChangeVehicleFreight_Advance;
                    mobj.RateType = mModel.Category;
                    mobj.GetParentRateAlso = mModel.GetParentAlso;




                    //iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                    #endregion

                    if (mAdd == true)
                    {
                        mobj.Code = GetNewCode();
                        ctxTFAT.HireVehicleMaster.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();
                    mnewrecordkey = Convert.ToInt32(mobj.RECORDKEY);
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.Code, DateTime.Now, 0, mobj.Code, "Save Hire Vehicle Master", "HVM");

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

        public ActionResult DeleteStateMaster(VehicleMasterVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            //long recordkey = Convert.ToInt64(mModel.Document);
            var mList = ctxTFAT.HireVehicleMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
            string mactivestring = "";
            var mactive1 = ctxTFAT.FMMaster.Where(x => (x.TruckNo.ToUpper().Trim() == mList.Code.ToUpper().Trim())).Select(x => x.FmNo).FirstOrDefault();
            if (mactive1 != 0)
            {
                mactivestring = mactivestring + "\nThis Vehicle Connected To " + mactive1 + " FM ";
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
                    ctxTFAT.HireVehicleMaster.Remove(mList);
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mList.Code, DateTime.Now, 0, mList.Code, "Delete Hire Vehicle Master", "HVM");

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
    }
}