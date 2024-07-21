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
using ALT_ERP3.Areas.Vehicles.Models;
using ALT_ERP3.Controllers;
using VehicleReportingst = ALT_ERP3.Areas.Vehicles.Models.VehicleReportingst;
using ALT_ERP3.Areas.Accounts.Models;
using System.Configuration;
using System.IO;
using ALT_ERP3.Areas.Logistics.Controllers;
using System.Net;
using System.Data.SqlClient;
using System.Data;

namespace ALT_ERP3.Areas.Vehicles.Controllers
{
    public class VehicleMasterController : BaseController
    {
        string attachmentPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentPath"].ToString()) == true ? "" : ConfigurationManager.AppSettings["AttachmentPath"].ToString();
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;

        #region GetFunction
        public string GetNewCode()
        {
            var NewLcNo = ctxTFAT.VehicleMaster.Where(x => x.Code != "99999" && x.Code != "99998").OrderByDescending(x => x.Code).Select(x => x.Code).Take(1).FirstOrDefault();
            int LcNo;
            if (String.IsNullOrEmpty(NewLcNo))
            {

                LcNo = 100000;
            }
            else
            {
                LcNo = Convert.ToInt32(NewLcNo) + 1;
            }

            return LcNo.ToString();
        }

        public string GetNewCode_VehiHistory()
        {
            string Code = ctxTFAT.tfatVehicleStatusHistory.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).FirstOrDefault();
            if (String.IsNullOrEmpty(Code))
            {

                return "100000";
            }
            else
            {
                return (Convert.ToInt32(Code) + 1).ToString();
            }
        }

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
        public JsonResult VehiclePostAc(string term)
        {
            var result = ctxTFAT.Master.Where(x => x.Hide == false /*&& x.IsLast== true*/ /*&& x.AcType == "S"*/ && x.OthPostType.Contains("V")).Select(m => new { m.Code, m.Name }).ToList();
            if (!(String.IsNullOrEmpty(term)))
            {
                result = ctxTFAT.Master.Where(x => x.Hide == false /*&& x.IsLast== true*/ /*&& x.AcType == "S"*/ && x.OthPostType.Contains("V") && x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).ToList();
            }
            var Modified = result.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetDriver(string term)
        {
            var list = ctxTFAT.DriverMaster.Where(x => x.VehicleNo == null || x.VehicleNo == "").ToList();
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
        public string GetNewCode_Vehi_Driver()
        {
            string Code = ctxTFAT.VehicleDri_Hist.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).FirstOrDefault();
            if (String.IsNullOrEmpty(Code))
            {

                return "100000";
            }
            else
            {
                return (Convert.ToInt32(Code) + 1).ToString();
            }
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
        public JsonResult DriCreditAccount(string term)//FMType
        {
            var list = ctxTFAT.Master.Where(x => x.AcType == "S").ToList().Distinct();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = ctxTFAT.Master.Where(x => x.Name.ToLower().Contains(term.ToLower()) /*&& x.AcType == "S"*/).ToList().Distinct();
            }

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }
        #endregion

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
            mpara = "";
            //return GetGridDataColumns(id, "L", mopt);
            return GetGridDataColumns(id, "L", "XXXX");
        }

        //[HttpPost]
        public ActionResult GetMasterGridData(GridOption Model)
        {
            ExecuteStoredProc("Drop Table ztmp_TfatVehicleHistory");
            string Query = "with CTE_RN as (    select  t.*,        ROW_NUMBER() OVER(ORDER BY fromperiod,FromTime) as RN  from tfatVehicleStatusHistory   as t  where truckno = '" + Model.Code + "') select   DocDate, FromPeriod, FromTime, Status,DATEDIFF(Day, FromPeriod, (select FromPeriod from CTE_RN G where G.RN = C.RN + 1)) as [Days], ENTEREDBY , Narr into ztmp_TfatVehicleHistory from CTE_RN as c";
            ExecuteStoredProc(Query);

            mpara = "para09^" + Model.Code;
            return GetGridReport(Model, "M", mpara, false, 0);
        }


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
                var Branchlist = ctxTFAT.VehicleMaster.Where(x => x.Code == Document).Select(x => x.Branch).FirstOrDefault();
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

        // GET: Vehicles/VehicleMaster
        public ActionResult Index(VehicleMasterVM mModel)
        {

            //SendSMS("9819260363", "msg", false, "", false, "");
            GetAllMenu(Session["ModuleName"].ToString());
            Session["TempAttach"] = null;
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");

            mdocument = mModel.Document;
            bool TrackReq = false;
            string Msg = "Tracking Not Avalable...!";
            //mModel.Branches = PopulateBranches();
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                //long Recordkey = Convert.ToInt64(mModel.Document);
                var mList = ctxTFAT.VehicleMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                //var PartyCode = Convert.ToInt32(mList.Code);

                if (mList != null)
                {
                    //var VehicleCategoryName = ctxTFAT.VehicleCategory.Where(x => x.Code == mList.Vehicle_Category).Select(x => x.vehicle_Category).FirstOrDefault();

                    //Get Attachment
                    AttachmentVM Att = new AttachmentVM();
                    Att.Type = "Vehic";
                    Att.Srl = mModel.Document.ToString();

                    AttachmentController attachmentC = new AttachmentController();
                    List<AttachmentVM> attachments = attachmentC.GetAttachmentListInEdit(Att);
                    Session["TempAttach"] = attachments;

                    mModel.Vehicle_No = mList.TruckNo;
                    mModel.Chassis_No = mList.ChassisNo;
                    var Driver = ctxTFAT.VehicleDri_Hist.Where(x => x.TruckNo == mList.Code).OrderByDescending(x => new { x.FromPeriod, x.FromTime }).Select(x => x.Driver).FirstOrDefault();
                    mModel.Driver_Name = Driver;
                    mModel.Driver = ctxTFAT.DriverMaster.Where(x => x.Code == Driver).Select(x => x.Name).FirstOrDefault();
                    if (mList.PurDt != null)
                    {
                        mModel.Purchase_Date = mList.PurDt.Value;
                    }

                    mModel.NoofTyres = mList.NoOfTyres;
                    mModel.NoOfSpepni = mList.Stepney;

                    mModel.BillOrInvoice_No = mList.BillOrInvoiceNo;
                    mModel.Chassis_Cost = mList.ChassisNo;
                    mModel.Financer_Name = mList.Fin;
                    mModel.Agreement_Amount = mList.AgAmt ?? 0;
                    mModel.Intrest_Rate = mList.IntRate ?? 0;
                    mModel.Insurance_Co = mList.InsCo;
                    mModel.Insured_Amount = mList.InsAmt ?? 0;
                    mModel.VehicleGroup = mList.TruckStatus;
                    mModel.VehicleGroup_Name = ctxTFAT.VehicleGrpStatusMas.Where(x => x.Code == mList.TruckStatus).Select(x => x.VehicleGroupStatus).FirstOrDefault();
                    if (String.IsNullOrEmpty(mModel.VehicleGroup_Name))
                    {
                        mModel.VehicleGroup = "";
                    }

                    mModel.Vehicle_Category = mList.VCategory;
                    mModel.Vehicle_Category_Name = ctxTFAT.VehicleCategory.Where(x => x.Code == mList.VCategory).Select(x => x.VehicleCategory1).FirstOrDefault();
                    if (String.IsNullOrEmpty(mModel.Vehicle_Category_Name))
                    {
                        mModel.Vehicle_Category = "";
                    }
                    mModel.Broker_Name = mList.Broker;
                    mModel.PayLoad = Convert.ToInt32(mList.PayLoad);
                    mModel.MaintainDriverAc = mList.MaintainDriverAC;
                    //mModel.PostingSplitAsperChargeMaster = mList.SplitPosting;
                    mModel.Broker = ctxTFAT.Master.Where(x => x.Code.ToString() == mList.Broker).Select(x => x.Name).FirstOrDefault();
                    if (String.IsNullOrEmpty(mModel.Broker))
                    {
                        mModel.Broker_Name = "";
                    }
                    mModel.Permit_No = mList.PermitNo;
                    mModel.Engine_No = mList.EngineNo;
                    mModel.Dealer_Name = mList.Dealer;
                    mModel.Invoice_Amount = mList.InvAmt ?? 0;
                    mModel.Body_Cost = mList.BCost ?? 0;
                    mModel.Agreement_nature = mList.AgNature;
                    mModel.Financed_Amount = mList.FinAmt ?? 0;
                    if (mList.Date != null)
                    {
                        mModel.Date = mList.Date.Value;
                    }
                    mModel.Intrest_Amount = mList.IntAmt ?? 0;
                    mModel.Policy_No = mList.PolicyNo;
                    mModel.Premium_Amount = mList.PreAmt ?? 0;
                    if (mList.LastEmiDate != null)
                    {
                        mModel.Last_Emi_Date = mList.LastEmiDate.Value;
                    }
                    mModel.Owner_Name = mList.Owner;
                    mModel.Acitve = mList.Acitve;
                    mModel.KM = mList.KM;
                    mModel.ScheduleDate_Time = mList.ScheduleDateTime;
                    mModel.ScheduleKM = mList.ScheduleKM;

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
                    var Status = ctxTFAT.tfatVehicleStatusHistory.Where(x => x.TruckNo == mList.Code).OrderByDescending(x => new { x.FromPeriod, x.FromTime }).Select(x => x.Status).FirstOrDefault();
                    if (String.IsNullOrEmpty(Status))
                    {
                        Status = "Ready";
                    }

                    mModel.vehicleReportingSt = (VehicleReportingst)Enum.Parse(typeof(VehicleReportingst), Status);
                    mModel.VehiPostAc = mList.PostAc;
                    mModel.VehiPostAcName = ctxTFAT.Master.Where(x => x.Code == mList.PostAc).Select(x => x.Name).FirstOrDefault();
                    if (String.IsNullOrEmpty(mModel.VehiPostAcName))
                    {
                        mModel.VehiPostAc = "";
                    }
                    mModel.ShortName = mList.ShortName;
                    mModel.SpecialRemarkReq = mList.RemarkReq;
                    mModel.Remark = mList.Remark;
                    mModel.BalckListReq = mList.HoldActivityReq;
                    mModel.BalckListRemark = mList.HoldRemark;

                    mModel.MaintainCreditorRecord = mList.ARAP;
                    mModel.PostingReq = mList.PostReq;
                    mModel.DebitAcCode = mList.DrAc;
                    mModel.DebitAcName = ctxTFAT.Master.Where(x => x.Code == mList.DrAc).Select(x => x.Name).FirstOrDefault();
                    mModel.CreditAccount = mList.CrAc;
                    mModel.GetParentAlso = mList.GetParentRateAlso;
                    mModel.DriCrAc = mList.DriCrAc;
                    mModel.DriCrAcN = ctxTFAT.Master.Where(x => x.Code == mList.DriCrAc).Select(x => x.Name).FirstOrDefault();
                    mModel.FMVOUREL = mList.FMVOURELReq;
                    mModel.DriverAdvancePayable = mList.DriverAdvancePayable;


                    #region Vehicle Tracking Avalable OR NOT

                    var CurrDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());

                    TfatVehicleTrackingSetup vehicleTrackingSetup = ctxTFAT.TfatVehicleTrackingSetup.FirstOrDefault();

                    if (vehicleTrackingSetup != null)
                    {
                        if (vehicleTrackingSetup.VM_AllTime)
                        {
                            TrackReq = true;
                        }
                    }

                    mModel.TrackButtonReq = TrackReq;
                    mModel.TrackErrorMsg = Msg;

                    var GetVehicleTrackId = ctxTFAT.TfatVehicleTrackApiList.Where(x => x.VehicleList.Contains(mList.Code)).FirstOrDefault();

                    if (GetVehicleTrackId == null)
                    {
                        mModel.TrackButtonReq = false;
                        mModel.TrackErrorMsg = "This Vehicle Not Fount To Any VehicleTracking List.\nPlease Check Tracking Details In Company Profile...!";
                    }

                    #endregion

                    List<ExpenseseOfVehicle> expeselist = new List<ExpenseseOfVehicle>();
                    ExecuteStoredProc("Drop Table Ztemp_VehiclemasterDue");
                    ExecuteStoredProc("select R.Code as ExpensesAc,R.date2 as TODT into Ztemp_VehiclemasterDue from RelateData R  where  R.value8 in  ( '" + mList.PostAc + "')	 and   R.Code in ('000100326','000100653','000100736','000100781','000100789','000100953','000101135')");
                    ExecuteStoredProc("insert into Ztemp_VehiclemasterDue select R.combo1 as ExpensesAc,R.date2 as TODT from RelateData R  where  R.code in  ( '" + mList.PostAc + "')	 and   R.combo1 in ('000100326','000100653','000100736','000100781','000100789','000100953','000101135')");
                    string Query = "WITH ranked_messages AS (SELECT m.*, ROW_NUMBER() OVER (PARTITION BY ExpensesAc ORDER BY Todt DESC) AS rn FROM Ztemp_VehiclemasterDue AS m ) SELECT *  FROM ranked_messages WHERE rn = 1;";
                    DataTable dt = new DataTable();
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();
                    var Date = Convert.ToDateTime("1900-01-01");
                    expeselist = (from DataRow dr in dt.Rows
                                  select new ExpenseseOfVehicle()
                                  {
                                      Code = dr["ExpensesAc"].ToString(),
                                      ToDt = string.IsNullOrEmpty(dr["TODT"].ToString()) == true ? Date : Convert.ToDateTime(dr["TODT"].ToString()),
                                  }).ToList();

                    expeselist = expeselist.Where(x => x.ToDt != Date).ToList();
                    ExecuteStoredProc("Drop Table Ztemp_VehiclemasterDue");
                    mModel.FitnessExp = expeselist.Where(x => x.Code == "000100736").Select(x => x.ToDt.ToShortDateString()).FirstOrDefault();
                    mModel.InsuranceExp = expeselist.Where(x => x.Code == "000100326").Select(x => x.ToDt.ToShortDateString()).FirstOrDefault();
                    mModel.PUCExp = expeselist.Where(x => x.Code == "000100789").Select(x => x.ToDt.ToShortDateString()).FirstOrDefault();
                    mModel.AIPExp = expeselist.Where(x => x.Code == "000100781").Select(x => x.ToDt.ToShortDateString()).FirstOrDefault();
                    mModel.StateTaxExp = expeselist.Where(x => x.Code == "000100953").Select(x => x.ToDt.ToShortDateString()).FirstOrDefault();
                    mModel.TPStateExp = expeselist.Where(x => x.Code == "000101135").Select(x => x.ToDt.ToShortDateString()).FirstOrDefault();
                    mModel.GreenTaxExp = expeselist.Where(x => x.Code == "000100653").Select(x => x.ToDt.ToShortDateString()).FirstOrDefault();
                }
            }
            else
            {
                mModel.Vehicle_No = "";
                mModel.Owner_Name = "";
                mModel.Chassis_No = "";
                mModel.Driver_Name = "";
                //mModel.Purchase_Date = "";
                mModel.BillOrInvoice_No = "";
                mModel.Chassis_Cost = "";
                mModel.Financer_Name = "";
                mModel.Agreement_Amount = 0;
                mModel.Intrest_Rate = 0;
                mModel.Insurance_Co = "";
                mModel.Insured_Amount = 0;
                //mModel.Truck_Status = "";
                mModel.Vehicle_Category = "";
                mModel.Broker_Name = "";
                mModel.Permit_No = "";
                mModel.Engine_No = "";
                mModel.Dealer_Name = "";
                mModel.Invoice_Amount = 0;
                mModel.Body_Cost = 0;
                mModel.Agreement_nature = "";
                mModel.Financed_Amount = 0;
                //mModel.Date = "";
                mModel.Intrest_Amount = 0;
                mModel.Policy_No = "";
                mModel.Premium_Amount = 0;
                //mModel.Last_Emi_Date = 0;
                mModel.Acitve = true;
                mModel.PayLoad = 0;
                mModel.Pick_Vehicle_Rate = true;
                mModel.Category_Rate = true;
                mModel.NoOfSpepni = 0;
                mModel.NoofTyres = 0;
                mModel.DriCrAc = "000100452";
                mModel.DriCrAcN = ctxTFAT.Master.Where(x => x.Code == mModel.DriCrAc).Select(x => x.Name).FirstOrDefault();


                //mModel.Pick_Vehicle_Rate_From_Category = false;
                if (mModel.ShortCutKey)
                {
                    VehicleGrpStatusMas VehicleGrpStatusMas = ctxTFAT.VehicleGrpStatusMas.Where(x => x.VehicleGroupStatus.ToLower().Contains("hire")).FirstOrDefault();
                    mModel.VehicleGroup = VehicleGrpStatusMas.Code;
                    mModel.VehicleGroup_Name = VehicleGrpStatusMas.VehicleGroupStatus;
                }

                mModel.DebitAcCode = "000100103";
                mModel.DebitAcName = ctxTFAT.Master.Where(x => x.Code == mModel.DebitAcCode).Select(x => x.Name).FirstOrDefault();
                mModel.CreditAccount = "V";
                mModel.MaintainCreditorRecord = true;
                mModel.MaintainDriverAc = true;
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(VehicleMasterVM mModel)
        {
            //string OldVehicleStatus = "";
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
                    VehicleMaster mobj = new VehicleMaster();
                    //VehicleDri_Hist vehicleDriver = new VehicleDri_Hist();

                    bool mAdd = true;
                    long Recordkey = Convert.ToInt64(mModel.Document);
                    if (ctxTFAT.VehicleMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.VehicleMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                        //OldVehicleStatus = mobj.Status;
                        mAdd = false;
                    }

                    if (mAdd == true)
                    {
                        mobj.Code = GetNewCode();
                        //if (mModel.Driver_Name.Trim() != "99999")
                        //{
                        //    DriverMaster Newdriver = ctxTFAT.DriverMaster.Where(x => x.Code.Trim().ToLower() == mModel.Driver_Name.Trim().ToLower()).FirstOrDefault();
                        //    if (Newdriver != null)
                        //    {
                        //        Newdriver.VehicleNo = mobj.Code.Trim();
                        //        ctxTFAT.Entry(Newdriver).State = EntityState.Modified;
                        //    }

                        //    vehicleDriver = new VehicleDri_Hist();
                        //    vehicleDriver.Code = GetNewCode_Vehi_Driver();
                        //    vehicleDriver.TruckNo = mobj.Code.ToUpper().Trim();
                        //    vehicleDriver.Driver = mModel.Driver_Name.Trim();
                        //    vehicleDriver.FromPeriod = DateTime.Now;
                        //    vehicleDriver.ToPeriod = null;
                        //    vehicleDriver.AUTHIDS = muserid;
                        //    vehicleDriver.AUTHORISE = mauthorise;
                        //    vehicleDriver.ENTEREDBY = muserid;
                        //    vehicleDriver.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                        //    ctxTFAT.VehicleDri_Hist.Add(vehicleDriver);
                        //}


                    }

                    #region Vehicle Master
                    mobj.TruckNo = mModel.Vehicle_No.ToUpper().Trim();
                    mobj.ChassisNo = mModel.Chassis_No;
                    mobj.Driver = " "; //mModel.Driver_Name;
                    if (mModel.Purchase_Date.ToShortDateString() == "01/01/0001")
                    {
                        mobj.PurDt = null;
                    }
                    else
                    {
                        mobj.PurDt = ConvertDDMMYYTOYYMMDD(mModel.Purchase_Date.ToShortDateString());
                    }
                    mobj.BillOrInvoiceNo = mModel.BillOrInvoice_No;
                    mobj.ChassisNo = mModel.Chassis_No;
                    mobj.Fin = mModel.Financer_Name;
                    mobj.AgAmt = mModel.Agreement_Amount;
                    mobj.IntRate = mModel.Intrest_Rate;
                    mobj.InsCo = mModel.Insurance_Co;
                    mobj.InsAmt = mModel.Insured_Amount;
                    mobj.TruckStatus = mModel.VehicleGroup;
                    mobj.VCategory = mModel.Vehicle_Category;
                    mobj.Broker = mModel.Broker_Name;
                    mobj.PermitNo = mModel.Permit_No;
                    mobj.EngineNo = mModel.Engine_No;
                    mobj.Dealer = mModel.Dealer_Name;
                    mobj.InvAmt = mModel.Invoice_Amount;
                    mobj.BCost = mModel.Body_Cost;
                    mobj.AgNature = mModel.Agreement_nature;
                    mobj.FinAmt = mModel.Financed_Amount;
                    if (mModel.Date.ToShortDateString() == "01/01/0001")
                    {
                        mobj.Date = null;
                    }
                    else
                    {
                        mobj.Date = ConvertDDMMYYTOYYMMDD(mModel.Date.ToShortDateString());
                    }
                    mobj.IntAmt = mModel.Intrest_Amount;
                    mobj.PolicyNo = mModel.Policy_No;
                    mobj.PreAmt = mModel.Premium_Amount;
                    if (mModel.Last_Emi_Date.ToShortDateString() == "01/01/0001")
                    {
                        mobj.LastEmiDate = null;
                    }
                    else
                    {
                        mobj.LastEmiDate = ConvertDDMMYYTOYYMMDD(mModel.Last_Emi_Date.ToShortDateString());
                    }
                    mobj.Owner = mModel.Owner_Name;
                    mobj.Acitve = mModel.Acitve;
                    mobj.PayLoad = mModel.PayLoad;
                    mobj.PickVehicleRate = mModel.Pick_Vehicle_Rate;
                    mobj.PickDriverTripRate = mModel.PickDriverTripRate;
                    mobj.ChangeDriverFreight_Advance = mModel.ChangeDriverFreight_Advance;
                    mobj.ChangeVehicleFreight_Advance = mModel.ChangeVehicleFreight_Advance;
                    mobj.RateType = mModel.Category;
                    mobj.Branch = mModel.Branch;
                    mobj.Status = mModel.vehicleReportingSt.ToString().Trim();
                    mobj.KM = mModel.KM;
                    mobj.ScheduleDateTime = mModel.ScheduleDate_Time;
                    mobj.ScheduleKM = mModel.ScheduleKM;
                    mobj.MaintainDriverAC = mModel.MaintainDriverAc;
                    //mobj.SplitPosting = mModel.PostingSplitAsperChargeMaster;
                    mobj.PostAc = mModel.VehiPostAc;
                    mobj.ShortName = mModel.ShortName;
                    mobj.Remark = mModel.Remark;
                    mobj.RemarkReq = mModel.SpecialRemarkReq;
                    mobj.HoldActivityReq = mModel.BalckListReq;
                    mobj.HoldRemark = mModel.BalckListRemark;
                    mobj.GetParentRateAlso = mModel.GetParentAlso;

                    mobj.PostReq = mModel.PostingReq;
                    mobj.ARAP = mModel.MaintainCreditorRecord;
                    mobj.DrAc = mModel.DebitAcCode;
                    mobj.CrAc = mModel.CreditAccount;
                    mobj.DriCrAc = mModel.DriCrAc;

                    mobj.NoOfTyres = mModel.NoofTyres;
                    mobj.Stepney = mModel.NoOfSpepni;

                    mobj.FMVOURELReq = mModel.FMVOUREL;
                    mobj.DriverAdvancePayable = mModel.DriverAdvancePayable;

                    //iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                    #endregion
                    #region Maintain History
                    //var statusHistory = ctxTFAT.tfatVehicleStatusHistory.Where(x => x.TruckNo == mobj.Code).FirstOrDefault();
                    //if (statusHistory == null)
                    //{
                    //    tfatVehicleStatusHistory history = new tfatVehicleStatusHistory();
                    //    history.Code = GetNewCode_VehiHistory();
                    //    history.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                    //    history.Status = mobj.Status;
                    //    history.FromPeriod = DateTime.Now;
                    //    history.TruckNo = mobj.Code;
                    //    history.Narr = mModel.VehicleStatusChangeNarr;
                    //    history.AUTHIDS = muserid;
                    //    history.AUTHORISE = mauthorise;
                    //    history.ENTEREDBY = muserid;
                    //    history.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                    //    ctxTFAT.tfatVehicleStatusHistory.Add(history);
                    //}
                    //else
                    //{
                    //    if (mobj.Status.Trim() != OldVehicleStatus.Trim())
                    //    {
                    //        var OldvehicleHistory = ctxTFAT.tfatVehicleStatusHistory.Where(x => x.TruckNo == mobj.Code.Trim() && x.ToPeriod == null).FirstOrDefault();
                    //        if (OldvehicleHistory != null)
                    //        {
                    //            OldvehicleHistory.ToPeriod = DateTime.Now;
                    //            ctxTFAT.Entry(OldvehicleHistory).State = EntityState.Modified;
                    //        }
                    //        tfatVehicleStatusHistory history = new tfatVehicleStatusHistory();
                    //        history.Code = GetNewCode_VehiHistory();
                    //        history.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                    //        history.Status = mobj.Status;
                    //        history.FromPeriod = DateTime.Now;
                    //        history.TruckNo = mobj.Code;
                    //        history.Narr = mModel.VehicleStatusChangeNarr;
                    //        history.AUTHIDS = muserid;
                    //        history.AUTHORISE = mauthorise;
                    //        history.ENTEREDBY = muserid;
                    //        history.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                    //        ctxTFAT.tfatVehicleStatusHistory.Add(history);
                    //    }
                    //}
                    #endregion
                    //#region DriverMaster Update

                    //DriverMaster Olddriver = ctxTFAT.DriverMaster.Where(x => x.VehicleNo.Trim().ToLower() == mobj.Code.Trim().ToLower()).FirstOrDefault();
                    //if (Olddriver != null)
                    //{
                    //    Olddriver.VehicleNo = null;
                    //    ctxTFAT.Entry(Olddriver).State = EntityState.Modified;
                    //}
                    //if (mobj.Driver != "99999")
                    //{
                    //    DriverMaster Newdriver = ctxTFAT.DriverMaster.Where(x => x.Code.Trim().ToLower() == mModel.Driver_Name.Trim().ToLower()).FirstOrDefault();
                    //    if (Newdriver != null)
                    //    {
                    //        Newdriver.VehicleNo = mModel.Vehicle_No.ToUpper().Trim();
                    //        ctxTFAT.Entry(Newdriver).State = EntityState.Modified;
                    //    }
                    //}

                    //#endregion

                    //#region Driver And Vehicle History

                    //vehicleDriver = ctxTFAT.VehicleDri_Hist.Where(x => x.TruckNo.Trim().ToLower() == mobj.Code.Trim().ToLower() && x.ToPeriod == null).FirstOrDefault();
                    //if (vehicleDriver == null)
                    //{
                    //    if (mobj.Driver != "99999")
                    //    {
                    //        vehicleDriver = new VehicleDri_Hist();
                    //        vehicleDriver.Code = GetNewCode_Vehi_Driver();
                    //        vehicleDriver.TruckNo = mobj.Code.ToUpper().Trim();
                    //        vehicleDriver.Driver = mobj.Driver;
                    //        vehicleDriver.FromPeriod = DateTime.Now;
                    //        vehicleDriver.ToPeriod = null;
                    //        vehicleDriver.AUTHIDS = muserid;
                    //        vehicleDriver.AUTHORISE = mauthorise;
                    //        vehicleDriver.ENTEREDBY = muserid;
                    //        vehicleDriver.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                    //        ctxTFAT.VehicleDri_Hist.Add(vehicleDriver);
                    //    }
                    //}
                    //else
                    //{
                    //    #region Update
                    //    vehicleDriver.ToPeriod = DateTime.Now;
                    //    ctxTFAT.Entry(vehicleDriver).State = EntityState.Modified;
                    //    #endregion

                    //    if (mobj.Driver != "99999")
                    //    {
                    //        vehicleDriver = new VehicleDri_Hist();
                    //        vehicleDriver.Code = GetNewCode_Vehi_Driver();
                    //        vehicleDriver.TruckNo = mobj.Code.ToUpper().Trim();
                    //        vehicleDriver.Driver = mobj.Driver;
                    //        vehicleDriver.FromPeriod = DateTime.Now;
                    //        vehicleDriver.ToPeriod = null;
                    //        vehicleDriver.AUTHIDS = muserid;
                    //        vehicleDriver.AUTHORISE = mauthorise;
                    //        vehicleDriver.ENTEREDBY = muserid;
                    //        vehicleDriver.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                    //        ctxTFAT.VehicleDri_Hist.Add(vehicleDriver);
                    //    }

                    //}

                    //#endregion


                    if (mAdd == true)
                    {
                        mobj.Code = GetNewCode();
                        ctxTFAT.VehicleMaster.Add(mobj);
                        AttachmentVM vM = new AttachmentVM();
                        vM.Srl = mobj.Code.ToString();
                        vM.ParentKey = mobj.Code.ToString();
                        vM.Type = "Vehic";
                        SaveAttachment(vM);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }

                    ctxTFAT.SaveChanges();
                    mnewrecordkey = Convert.ToInt32(mobj.RECORDKEY);
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.Code, DateTime.Now, 0, mobj.Code, "Save Vehicle Master", "VM");

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
        public ActionResult GenerareCreditors(string TruckNo)
        {
            Session["MailInfo"] = null;
            MasterVM Model = new MasterVM();
            List<MasterVM> AddressList1 = new List<MasterVM>();

            var Company = ctxTFAT.TfatComp.Where(x => x.Code.ToString() == mcompcode).FirstOrDefault();
            var countryname = ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == Company.Country).FirstOrDefault();
            var statename = ctxTFAT.TfatState.Where(x => x.Code.ToString() == Company.State).FirstOrDefault();
            var cityname = ctxTFAT.TfatCity.Where(x => x.Code.ToString() == Company.City).FirstOrDefault();

            AddressList1.Add(new MasterVM
            {
                Ifexist = true,
                SrNo = 0,
                AName = "",
                CorpID = mcompcode,
                Person = "",
                Adrl1 = "",
                Adrl2 = "",
                Adrl3 = "",
                Country = countryname.Code.ToString(),
                CountryName = countryname.Name,
                State = statename.Code.ToString(),
                StateName = statename.Name,
                City = cityname.Code.ToString(),
                CityName = cityname.Name,
                Pin = "",
                Area = 0,
                AreaName = "",
                Tel1 = "",
                Fax = "",
                Mobile = "",
                www = "",
                Email = "",
                MailingCategory = 0,
                UserID = muserid,
                CorrespondenceType = 0,
                Password = "",
                Source = 0,
                Segment = "",
                STaxCode = "",
                PTaxCode = "",
                Licence1 = "",
                Licence2 = "",
                PanNo = "",
                TINNo = "",
                Designation = 0,
                Language = 0,
                Dept = 0,
                Religion = 0,
                Division = 0,
                Bdate = DateTime.Now,
                Anndate = DateTime.Now,
                SpouseName = "",
                SpouseBdate = DateTime.Now,
                ChildName = "",
                ChildBdate = DateTime.Now,
                Code = "",
                ContactType = "1",
                AssistEmail = "",
                AssistMobile = "",
                AssistTel = "",
                AssistName = "",
                DefaultIGst = 0,
                DefaultSGst = 0,
                DefaultCGst = 0,
                VATReg = "",
                AadharNo = "",
                GSTNo = "",
                GSTType = "0",
                PoisonLicense = "",
                ReraNo = "",
                DealerType = "0"
            });
            Model.AddressList = AddressList1;

            if (AddressList1 != null)
            {
                Model.Ifexist = true;
                Model.SrNo = 0;
                Model.AName = "";
                Model.CorpID = mcompcode;
                Model.Person = "";
                Model.Adrl1 = "";
                Model.Adrl2 = "";
                Model.Adrl3 = "";
                Model.Country = countryname.Code.ToString();
                Model.CountryName = countryname.Name;
                Model.State = statename.Code.ToString();
                Model.StateName = statename.Name;
                Model.City = cityname.Code.ToString();
                Model.CityName = cityname.Name;
                Model.Pin = "";
                Model.Area = 0;
                Model.AreaName = "";
                Model.Tel1 = "";
                Model.Fax = "";
                Model.Mobile = "";
                Model.www = "";
                Model.Email = "";
                Model.MailingCategory = 0;
                Model.UserID = muserid;
                Model.CorrespondenceType = 0;
                Model.Password = "";
                Model.Source = 0;
                Model.Segment = "";
                Model.STaxCode = "";
                Model.PTaxCode = "";
                Model.Licence1 = "";
                Model.Licence2 = "";
                Model.PanNo = "";
                Model.TINNo = "";
                Model.Designation = 0;
                Model.Language = 0;
                Model.Dept = 0;
                Model.Religion = 0;
                Model.Division = 0;
                Model.Bdate = DateTime.Now;
                Model.Anndate = DateTime.Now;
                Model.SpouseName = "";
                Model.SpouseBdate = DateTime.Now;
                Model.ChildName = "";
                Model.ChildBdate = DateTime.Now;
                Model.Code = "";
                Model.ContactType = "1";
                Model.AssistEmail = "";
                Model.AssistMobile = "";
                Model.AssistTel = "";
                Model.AssistName = "";
                Model.DefaultIGst = 0;
                Model.DefaultSGst = 0;
                Model.DefaultCGst = 0;
                Model.VATReg = "";
                Model.AadharNo = "";
                Model.GSTNo = "";
                Model.GSTType = "0";
                Model.PoisonLicense = "";
                Model.ReraNo = "";
                Model.DealerType = "0";
                Model.Tel2 = "";
                Model.Tel3 = "";
            }

            Model.MailList = AddressList1;
            Session.Add("MailInfo", AddressList1);

            Model.AName = TruckNo;
            Model.AcType = "S";
            Model.AppBranch = mbranchcode;
            Model.City = "411111";
            Model.ContactType = "1";
            Model.CorpID = "100";
            Model.Country = "1";
            Model.DealerType = "0";
            Model.EmailTemplate = "False";
            Model.GSTType = "0";
            Model.Grp = "000000044";
            Model.Header = "Suppliers (Vendors/Creditors)";
            Model.Name = TruckNo;
            Model.OptionGstType = "0";
            Model.SMSTemplate = "False";
            //Model.ShortName = TruckNo.Length >= 25 ? TruckNo.Substring(0, 25) : TruckNo;
            Model.ShortName = null;
            Model.State = "19";


            var Result = VehiclePostIngAc(Model);
            return Result;
        }

        public ActionResult VehiclePostIngAc(MasterVM Model)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {

                    Model.Code = GetCode(0, 9, Model.Grp);

                    var delmasterinfo = ctxTFAT.MasterInfo.Where(x => x.Code == Model.Document).FirstOrDefault();
                    var deladdress = ctxTFAT.Address.Where(x => x.Code == Model.Document).ToList();
                    var delholdtrx = ctxTFAT.HoldTransactions.Where(x => x.Code == Model.Document).FirstOrDefault();
                    var deltaxdet = ctxTFAT.TaxDetails.Where(x => x.Code == Model.Document).FirstOrDefault();
                    var deltaddons = ctxTFAT.AddonMas.Where(x => x.TableKey == Model.Document).ToList();
                    //SAttach var deltdoc = ctxTFAT.Attachment.Where(x => x.ParentKey == "Master_" + Model.Document).ToList();
                    if (delmasterinfo != null)
                    {
                        ctxTFAT.MasterInfo.Remove(delmasterinfo);
                    }
                    if (deladdress != null)
                    {
                        ctxTFAT.Address.RemoveRange(deladdress);
                    }
                    if (delholdtrx != null)
                    {
                        ctxTFAT.HoldTransactions.Remove(delholdtrx);
                    }
                    if (deltaxdet != null)
                    {
                        ctxTFAT.TaxDetails.Remove(deltaxdet);
                    }
                    if (deltaddons.Count > 0)
                    {
                        ctxTFAT.AddonMas.RemoveRange(deltaddons);
                    }

                    // SAttach if (deltdoc.Count > 0)
                    //{
                    //    ctxTFAT.Attachment.RemoveRange(deltdoc);
                    //}
                    ctxTFAT.SaveChanges();

                    Master mobj = new Master();
                    if (Model.Mode == "Edit")
                    {
                        mobj = ctxTFAT.Master.Where(x => x.Code == Model.Code).Select(x => x).FirstOrDefault();
                    }


                    //Add Driver
                    mobj.OthPostType = "V";
                    mobj.Code = Model.Code;
                    mobj.Grp = Model.Grp;
                    mobj.Name = Model.Name.ToUpper().Trim();
                    mobj.ForceCC = Model.CCReqd;
                    mobj.AcHeadCode = (Model.AcHeadCode == null) ? "" : Model.AcHeadCode;
                    mobj.AcType = (Model.AcType == null) ? "" : Model.AcType;
                    mobj.ARAP = Model.ARAP;
                    mobj.AUTHIDS = muserid;
                    mobj.AppBranch = Model.AppBranch;
                    mobj.AUTHORISE = "A00";
                    mobj.BaseGr = (Model.AcType == null) ? "" : Model.AcType;
                    mobj.Category = (Model.Category == null) ? 0 : Model.Category;
                    mobj.CCBudget = Model.CCBudget;
                    mobj.Hide = Model.Hide;
                    mobj.IsPublic = Model.IsPublic;
                    mobj.NonActive = Model.NonActive;
                    mobj.ShortName = (Model.ShortName == null) ? "" : Model.ShortName;
                    mobj.SalesMan = Model.SalesMan;
                    mobj.Broker = Model.Broker;
                    mobj.ENTEREDBY = muserid;
                    mobj.GroupTree = GetGroupTree(Model.Grp);
                    mobj.IsSubLedger = Model.IsSubLedger;
                    mobj.LASTUPDATEDATE = DateTime.Now;
                    mobj.UserID = (Model.AdminUser == null) ? "" : Model.AdminUser;
                    mobj.CreateDate = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                    if (Model.Mode == "Edit")
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    else
                    {
                        ctxTFAT.Master.Add(mobj);
                    }

                    MasterInfo mobj2 = new MasterInfo();
                    mobj2.AppProduct = "";
                    mobj2.Area = null;
                    mobj2.AUTHIDS = muserid;
                    mobj2.AUTHORISE = "A00";
                    mobj2.BaseGr = (Model.AcType == null) ? "" : Model.AcType;
                    mobj2.Brokerage = 0;
                    mobj2.CashDisc = Model.CashDisc;
                    mobj2.CheckCRDays = Model.CheckCRDays;
                    mobj2.CheckCRLimit = Model.CheckCRLimit;
                    mobj2.Code = Model.Code;
                    mobj2.CompanyType = "";
                    mobj2.CostCentre = null;
                    mobj2.CRDaysWarn = (Model.CRDaysWarn == 0) ? false : true;
                    mobj2.CreatedOn = DateTime.Now;
                    mobj2.CrLimit = Model.CrLimit;
                    mobj2.CRLimitTole = Model.CRLimitTole;
                    mobj2.CRLimitWarn = (Model.CRLimitWarn == 0) ? false : true;
                    mobj2.CRLimitWithPO = Model.CRLimitWithPO;
                    mobj2.CRLimitWithTrx = Model.CRLimitWithTrx;
                    mobj2.CrPeriod = Convert.ToInt32(Model.CRPeriod);
                    mobj2.CurrName = Convert.ToInt32(Model.CurrCode);
                    mobj2.CutTDS = Model.CutTDS;
                    mobj2.DepricAC = "";
                    mobj2.DiscDays = Model.DiscDays;
                    mobj2.DiscPerc = Model.DiscPerc;
                    mobj2.EmailParty = Model.EmailParty;
                    mobj2.EmailPartyAlert = Model.EmailPartyAlert;
                    mobj2.EmailSalesman = Model.EmailSalesman;
                    mobj2.EmailTemplate = Model.EmailTemplate;
                    mobj2.EmailUsers = "";
                    mobj2.PriceList = Model.PriceList;
                    mobj2.FreqOS = (Model.FreqOS == null) ? 0 : Convert.ToInt32(Model.FreqOS);
                    mobj2.SGSTRate = Model.SGST;
                    mobj2.IGSTRate = Model.IGST;
                    mobj2.CGSTRate = Model.CGST;
                    mobj2.FreqForm = (Model.FreqForm == null) ? 0 : Convert.ToInt32(Model.FreqForm);
                    mobj2.Grp = Model.Grp;
                    mobj2.IntAmt = Convert.ToDecimal(0.00);
                    mobj2.IntRate = Model.IntRate;
                    mobj2.LastUpdateBy = muserid;

                    mobj2.LeadCode = "";
                    mobj2.LeadConvertDt = DateTime.Now;
                    mobj2.Name = Model.Name;
                    mobj2.Narr = (Model.Narr == null) ? "" : Model.Narr;
                    mobj2.PaymentTerms = (Model.PaymentTerms == null) ? "" : Model.PaymentTerms;
                    mobj2.Rank = (Model.Rank == null) ? 0 : Convert.ToInt32(Model.Rank);
                    mobj2.ReminderFormat = (Model.ReminderFormat == null) ? "" : Model.ReminderFormat;
                    mobj2.SMSTemplate = (Model.SMSTemplate == null) ? "" : Model.SMSTemplate;
                    mobj2.SMSUsers = "";
                    mobj2.SMSParty = Model.SMSParty;
                    mobj2.SMSSalesman = Model.SMSSalesman;
                    mobj2.IncoPlace = (Model.IncoPlace == null) ? "" : Model.IncoPlace;
                    mobj2.IncoTerms = (Model.IncoTerms == null) ? 0 : Model.IncoTerms;
                    mobj2.CurrName = Model.CurrCode;
                    mobj2.HSNCode = (Model.HSN == null) ? "" : Model.HSN;
                    mobj2.ReminderFormat = (Model.ReminderFormat == null) ? "" : Model.ReminderFormat;
                    mobj2.RTGS = (Model.RTGS == null) ? "" : Model.RTGS;
                    mobj2.TDSCode = (Model.TDSCode == null) ? 0 : Model.TDSCode;
                    mobj2.Transporter = (Model.Transporter == null) ? "" : Model.Transporter;
                    mobj2.xBranch = mbranchcode;
                    mobj2.ENTEREDBY = muserid;
                    mobj2.LastSent = DateTime.Now;
                    mobj2.ItemType = (Model.ItemType == null) ? "" : Model.ItemType;
                    mobj2.LocationCode = 100001;
                    mobj2.LASTUPDATEDATE = DateTime.Now;
                    mobj2.GSTType = (Model.OptionGstType == null) ? 0 : Convert.ToInt32(Model.OptionGstType);
                    mobj2.GSTFlag = Model.GstApplicable;
                    mobj2.ODLImit = Model.ODLimit;
                    mobj2.DrAcNo = (Model.AcCode == null) ? "" : Model.AcCode;
                    mobj2.SMSPartyAlert = Model.SMSPartyAlert;
                    mobj2.PriceDiscList = Model.PDiscList;
                    mobj2.SchemeList = Model.SchemeList;
                    ctxTFAT.MasterInfo.Add(mobj2);

                    HoldTransactions mobj4 = new HoldTransactions();
                    mobj4.AUTHIDS = muserid;
                    mobj4.AUTHORISE = "A00";
                    mobj4.CheckCRDays = Model.CheckCRDays;
                    mobj4.CheckCRLimit = Model.CheckCRLimit;
                    mobj4.ChkTempCRDays = false;
                    mobj4.ChkTempCRLimit = false;
                    mobj4.Code = Model.Code;
                    mobj4.CRDaysWarn = (Model.CRDaysWarn == 0) ? false : true;
                    mobj4.CRLimitWarn = (Model.CRLimitWarn == 0) ? false : true;
                    mobj4.CRLimitWithTrx = Model.CRLimitWithTrx;
                    mobj4.CRLimitWithPO = Model.CRLimitWithPO;
                    mobj4.CrPeriod = Convert.ToInt32(Model.CRPeriod);
                    mobj4.ENTEREDBY = muserid;
                    mobj4.HoldDespatch = Model.HoldDespatch;
                    mobj4.HoldDespatchDt1 = (Model.StrHoldDespatchDt == null || Model.StrHoldDespatchDt == "01-01-0001" || Model.StrHoldDespatchDt == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrHoldDespatchDt);
                    mobj4.HoldDespatchDt2 = DateTime.Now;
                    mobj4.HoldEnquiry = Model.HoldEnquiry;
                    mobj4.HoldEnquiryDt1 = (Model.StrHoldEnquiryDt == null || Model.StrHoldEnquiryDt == "01-01-0001" || Model.StrHoldEnquiryDt == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrHoldEnquiryDt);
                    mobj4.HoldEnquiryDt2 = DateTime.Now;
                    mobj4.HoldInvoice = Model.HoldInvoice;
                    mobj4.HoldInvoiceDt1 = (Model.StrHoldInvoiceDt == null || Model.StrHoldInvoiceDt == "01-01-0001" || Model.StrHoldInvoiceDt == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrHoldInvoiceDt);
                    mobj4.DocDate = DateTime.Now;
                    mobj4.HoldInvoiceDt2 = DateTime.Now;
                    mobj4.HoldNarr = Model.HoldNarr;
                    mobj4.HoldOrder = false;
                    mobj4.HoldOrderDt1 = DateTime.Now;
                    mobj4.HoldOrderDt2 = DateTime.Now;
                    mobj4.HoldPayment = false;
                    mobj4.HoldQuote = false;
                    mobj4.HoldQuoteDt1 = DateTime.Now;
                    mobj4.HoldQuoteDt2 = DateTime.Now; ;
                    mobj4.LASTUPDATEDATE = DateTime.Now;
                    mobj4.TempCrDayDt1 = DateTime.Now;
                    mobj4.TempCrDayDt2 = DateTime.Now;
                    mobj4.TempCrLimit = 0;
                    mobj4.TempCrLimitDt1 = DateTime.Now;
                    mobj4.TempCrLimitDt2 = DateTime.Now;
                    mobj4.TempCrPeriod = 0;
                    mobj4.TempRemark = "";
                    mobj4.Ticklers = Model.Ticklers;
                    ctxTFAT.HoldTransactions.Add(mobj4);

                    TaxDetails mobj3 = new TaxDetails();
                    mobj3.AUTHIDS = muserid;
                    mobj3.AUTHORISE = "A00";
                    mobj3.Code = Model.Code;
                    mobj3.CutTCS = Model.CutTCS;
                    mobj3.CutTDS = Model.CutTDS;
                    mobj3.Deductee = "";
                    mobj3.DifferRate = Model.DifferRate;
                    mobj3.DifferRateCertNo = (Model.DifferRateCertNo == null) ? "" : Model.DifferRateCertNo;
                    mobj3.ENTEREDBY = muserid;
                    mobj3.Form15HCITDate = (Model.StrForm15HCITDate == null || Model.StrForm15HCITDate == "01-01-0001" || Model.StrForm15HCITDate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrForm15HCITDate);
                    mobj3.Form15HDate = (Model.StrForm15HDate == null || Model.StrForm15HDate == "01-01-0001" || Model.StrForm15HDate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrForm15HDate);
                    mobj3.IsDifferRate = Model.IsDifferRate;
                    mobj3.IsForm15H = Model.IsForm15H;
                    mobj3.LASTUPDATEDATE = DateTime.Now;
                    mobj3.LocationCode = 100001;

                    mobj3.TDSCode = (Model.TDSCode == null) ? 0 : Convert.ToInt32(Model.TDSCode);
                    ctxTFAT.TaxDetails.Add(mobj3);

                    if (Session["MailInfo"] != null)
                    {
                        var mailinformation = (List<MasterVM>)Session["MailInfo"];
                        if (mailinformation.Count == 1)
                        {
                            Address mobj1 = new Address();
                            mobj1.AddOrContact = (Model.ContactType == null || Model.ContactType.Trim() == "") ? 0 : Convert.ToInt32(Model.ContactType);
                            mobj1.Adrl1 = (Model.Adrl1 == null) ? "" : Model.Adrl1;
                            mobj1.Adrl2 = (Model.Adrl2 == null) ? "" : Model.Adrl2;
                            mobj1.Adrl3 = (Model.Adrl3 == null) ? "" : Model.Adrl3;
                            mobj1.Adrl4 = "";
                            mobj1.AnnDate = (Model.StrAnndate == null || Model.StrAnndate == "01-01-0001" || Model.StrAnndate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrAnndate);
                            mobj1.Area = Model.Area;
                            mobj1.AssistEmail = (Model.AssistEmail == null) ? "" : Model.AssistEmail;
                            mobj1.AssistMobile = (Model.AssistMobile == null) ? "" : Model.AssistMobile;
                            mobj1.AssistName = (Model.AssistName == null) ? "" : Model.AssistName;
                            mobj1.AssistTel = (Model.AssistTel == null) ? "" : Model.AssistTel;
                            mobj1.AUTHIDS = muserid;
                            mobj1.AUTHORISE = "A00";
                            mobj1.BaseGr = (Model.AcType == null) ? "" : Model.AcType;
                            mobj1.BDate = (Model.Budate == null || Model.Budate == "01-01-0001" || Model.Budate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.Budate);
                            mobj1.ChildBdate = (Model.StrChildBdate == null || Model.StrChildBdate == "01-01-0001" || Model.StrChildBdate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrChildBdate);
                            mobj1.ChildName = (Model.ChildName == null) ? "" : Model.ChildName;
                            mobj1.City = (Model.City == null) ? "" : Model.City;
                            mobj1.Code = Model.Code;
                            mobj1.CorpID = (Model.CorpID == null) ? "" : Model.CorpID;
                            mobj1.CorrespondenceType = Model.CorrespondenceType;
                            mobj1.Country = (Model.Country == null) ? "" : Model.Country;
                            mobj1.Dept = Model.Dept;
                            mobj1.Designation = Model.Designation;
                            mobj1.Division = Model.Division;
                            mobj1.DraweeBank = Model.DraweeBank;
                            mobj1.Email = (Model.Email == null) ? "" : Model.Email;
                            mobj1.ENTEREDBY = muserid;
                            mobj1.Fax = (Model.Fax == null) ? "" : Model.Fax;
                            mobj1.Language = Model.Language;
                            mobj1.LASTUPDATEDATE = DateTime.Now;
                            mobj1.Licence1 = (Model.Licence1 == null) ? "" : Model.Licence1;
                            mobj1.Licence2 = (Model.Licence2 == null) ? "" : Model.Licence2;
                            mobj1.LocationCode = mlocationcode;
                            mobj1.MailingCategory = Model.MailingCategory;
                            mobj1.Mobile = (Model.Mobile == null) ? "" : Model.Mobile;
                            mobj1.Name = (Model.AName == null) ? "" : Model.AName;
                            mobj1.PanNo = (Model.PanNo == null) ? "" : Model.PanNo;
                            mobj1.Password = (Model.Password == null) ? "" : Model.Password;
                            mobj1.Person = (Model.Person == null) ? "" : Model.Person;
                            mobj1.PhotoPath = "";
                            mobj1.Pin = (Model.Pin == null) ? "" : Model.Pin;
                            mobj1.PTaxCode = (Model.PTaxCode == null) ? "" : Model.PTaxCode;
                            mobj1.STaxCode = (Model.STaxCode == null) ? "" : Model.STaxCode;
                            mobj1.Religion = Model.Religion;
                            mobj1.Segment = (Model.Segment == null) ? "" : Model.Segment;
                            mobj1.Sno = Convert.ToInt32(Model.SrNo);
                            mobj1.Source = Model.Source;
                            mobj1.SpouseBdate = (Model.SpouseBudate == null || Model.SpouseBudate == "01-01-0001" || Model.SpouseBudate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.SpouseBudate);
                            mobj1.SpouseName = (Model.SpouseName == null) ? "" : Model.SpouseName;
                            mobj1.State = (Model.State == null) ? "" : Model.State;
                            mobj1.Tel1 = (Model.Tel1 == null) ? "" : Model.Tel1;
                            mobj1.Tel2 = (Model.Tel2 == null) ? "" : Model.Tel2;
                            mobj1.Tel3 = (Model.Tel3 == null) ? "" : Model.Tel3;
                            mobj1.Tel4 = "";
                            mobj1.TINNo = (Model.TINNo == null) ? "" : Model.TINNo;
                            mobj1.UserID = (Model.UserID == null) ? "" : Model.UserID;
                            mobj1.www = (Model.www == null) ? "" : Model.www;
                            mobj1.AadharNo = (Model.AadharNo == null) ? "" : Model.AadharNo;
                            mobj1.GSTNo = (Model.GSTNo == null) ? "" : Model.GSTNo;
                            mobj1.IGSTRate = Model.DefaultIGst;
                            mobj1.CGSTRate = Model.DefaultCGst;
                            mobj1.SGSTRate = Model.DefaultSGst;
                            mobj1.GSTType = (Model.GSTType == null || Model.GSTType.Trim() == "") ? 0 : Convert.ToInt32(Model.GSTType);
                            mobj1.DealerType = (Model.DealerType == null || Model.DealerType.Trim() == "") ? (byte)0 : Convert.ToByte(Model.DealerType);
                            mobj1.VATReg = (Model.VATReg == null) ? "" : Model.VATReg;
                            mobj1.ReraRegNo = (Model.ReraNo == null) ? "" : Model.ReraNo;
                            ctxTFAT.Address.Add(mobj1);
                        }
                        else
                        {
                            foreach (var item in mailinformation)
                            {
                                Address mobj1 = new Address();
                                mobj1.AddOrContact = (item.ContactType == null || item.ContactType.Trim() == "") ? 0 : Convert.ToInt32(item.ContactType);
                                mobj1.Adrl1 = (item.Adrl1 == null) ? "" : item.Adrl1;
                                mobj1.Adrl2 = (item.Adrl2 == null) ? "" : item.Adrl2;
                                mobj1.Adrl3 = (item.Adrl3 == null) ? "" : item.Adrl3;
                                mobj1.Adrl4 = "";
                                mobj1.AnnDate = (item.Anndate == null) ? DateTime.Now : item.Anndate;
                                mobj1.Area = item.Area;
                                mobj1.AssistEmail = (item.AssistEmail == null) ? "" : item.AssistEmail;
                                mobj1.AssistMobile = (item.AssistMobile == null) ? "" : item.AssistMobile;
                                mobj1.AssistName = (item.AssistName == null) ? "" : item.AssistName;
                                mobj1.AssistTel = (item.AssistTel == null) ? "" : item.AssistTel;
                                mobj1.AUTHIDS = muserid;
                                mobj1.AUTHORISE = "A00";
                                mobj1.BaseGr = (Model.AcType == null) ? "" : Model.AcType;
                                mobj1.BDate = (item.Bdate == null) ? DateTime.Now : item.Bdate;
                                mobj1.ChildBdate = (item.ChildBdate == null) ? DateTime.Now : item.ChildBdate;
                                mobj1.ChildName = (item.ChildName == null) ? "" : item.ChildName;
                                mobj1.City = (item.City == null) ? "" : item.City;
                                mobj1.Code = Model.Code;
                                mobj1.CorpID = (item.CorpID == null) ? "" : item.CorpID;
                                mobj1.CorrespondenceType = item.CorrespondenceType;
                                mobj1.Country = (item.Country == null) ? "" : item.Country;
                                mobj1.Dept = item.Dept;
                                mobj1.Designation = item.Designation;
                                mobj1.Division = item.Division;
                                mobj1.DraweeBank = Model.DraweeBank;
                                mobj1.Email = (item.Email == null) ? "" : item.Email;
                                mobj1.ENTEREDBY = muserid;
                                mobj1.Fax = (item.Fax == null) ? "" : item.Fax;
                                mobj1.Language = item.Language;
                                mobj1.LASTUPDATEDATE = DateTime.Now;
                                mobj1.Licence1 = (item.Licence1 == null) ? "" : item.Licence1;
                                mobj1.Licence2 = (item.Licence2 == null) ? "" : item.Licence2;
                                mobj1.LocationCode = mlocationcode;
                                mobj1.MailingCategory = item.MailingCategory;
                                mobj1.Mobile = (item.Mobile == null) ? "" : item.Mobile;
                                mobj1.Name = (item.AName == null) ? "" : item.AName;
                                mobj1.PanNo = (item.PanNo == null) ? "" : item.PanNo;
                                mobj1.Password = (item.Password == null) ? "" : item.Password;
                                mobj1.Person = (item.Person == null) ? "" : item.Person;
                                mobj1.PhotoPath = "";
                                mobj1.Pin = (item.Pin == null) ? "" : item.Pin;
                                mobj1.PTaxCode = (item.PTaxCode == null) ? "" : item.PTaxCode;
                                mobj1.STaxCode = (item.STaxCode == null) ? "" : item.STaxCode;
                                mobj1.Religion = item.Religion;
                                mobj1.Segment = (item.Segment == null) ? "" : item.Segment;
                                mobj1.Sno = Convert.ToInt32(item.SrNo);
                                mobj1.Source = item.Source;
                                mobj1.SpouseBdate = (item.SpouseBdate == null) ? DateTime.Now : item.SpouseBdate;
                                mobj1.SpouseName = (item.SpouseName == null) ? "" : item.SpouseName;
                                mobj1.State = (item.State == null) ? "" : item.State;
                                mobj1.Tel1 = (item.Tel1 == null) ? "" : item.Tel1;
                                mobj1.Tel2 = (item.Tel2 == null) ? "" : item.Tel2;
                                mobj1.Tel3 = (item.Tel3 == null) ? "" : item.Tel3;
                                mobj1.Tel4 = "";
                                mobj1.TINNo = (item.TINNo == null) ? "" : item.TINNo;
                                mobj1.UserID = (item.UserID == null) ? "" : item.UserID;
                                mobj1.www = (item.www == null) ? "" : item.www;
                                mobj1.AadharNo = (item.AadharNo == null) ? "" : item.AadharNo;
                                mobj1.GSTNo = (item.GSTNo == null) ? "" : item.GSTNo;
                                mobj1.IGSTRate = item.DefaultIGst;
                                mobj1.CGSTRate = item.DefaultCGst;
                                mobj1.SGSTRate = item.DefaultSGst;
                                mobj1.GSTType = (item.GSTType == null || item.GSTType.Trim() == "") ? 0 : Convert.ToInt32(item.GSTType);
                                mobj1.DealerType = (item.DealerType == null || item.DealerType.Trim() == "") ? (byte)0 : Convert.ToByte(item.DealerType);
                                mobj1.VATReg = (item.VATReg == null) ? "" : item.VATReg;
                                mobj1.ReraRegNo = (item.ReraNo == null) ? "" : item.ReraNo;
                                ctxTFAT.Address.Add(mobj1);
                            }
                        }
                    }

                    if (Session["FixedAssets"] != null)
                    {
                        var FixedAssets = (List<MasterVM>)Session["FixedAssets"];
                        if (FixedAssets.Count != 0)
                        {
                            foreach (var item in FixedAssets)
                            {
                                Assets mobj1 = new Assets();
                                mobj1.AUTHORISE = "A00";
                                mobj1.Code = Model.Code;
                                mobj1.Branch = mbranchcode;
                                mobj1.AUTHIDS = muserid;
                                mobj1.Store = 100001;
                                mobj1.LocationCode = mlocationcode;
                                mobj1.AcDep = "";
                                mobj1.Method = item.Method;
                                mobj1.Rate = 1;
                                mobj1.AcCode = item.AcCode;
                                mobj1.BookValue = item.BookValue;
                                mobj1.CostPrice = item.CostPrice;
                                mobj1.PurchDate = item.PurchDate;
                                mobj1.UseDate = item.UseDate;
                                mobj1.LASTUPDATEDATE = DateTime.Now;
                                mobj1.ENTEREDBY = muserid;
                                ctxTFAT.Assets.Add(mobj1);
                            }
                        }
                    }

                    //SaveAddons(Model);
                    //SaveAttachment(Model);
                    ctxTFAT.SaveChanges();
                    //UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "");

                    //int n = ctxTFAT.Database.ExecuteSqlCommand("Update Master Set GroupTree = dbo.fn_GetGroupTree(Grp)");
                    transaction.Commit();
                    transaction.Dispose();
                    Session["TempAccMasterAttach"] = null;
                    Session["MailInfo"] = null;
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();

                    return Json(new { Message = ex1.EntityValidationErrors.Select(x => x.ValidationErrors.Select(xa => xa.ErrorMessage).FirstOrDefault()), Status = "Error" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.InnerException.Message, Status = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", Code = Model.Code, Name = Model.Name }, JsonRequestBehavior.AllowGet);
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

            if (mModel.Document == "99998")
            {
                return Json(new
                {
                    Message = "Not Allowed To Delete..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            if (mModel.Document == "99999")
            {
                return Json(new
                {
                    Message = "Not Allowed To Delete..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }


            //long recordkey = Convert.ToInt64(mModel.Document);
            var mList = ctxTFAT.VehicleMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
            string mactivestring = "";
            var mactive1 = ctxTFAT.FMMaster.Where(x => (x.TruckNo.ToUpper().Trim() == mList.Code.ToUpper().Trim())).Select(x => x.FmNo).FirstOrDefault();
            if (mactive1 != 0)
            {
                mactivestring = mactivestring + "\nThis Vehicle Connected To " + mactive1 + " FM ";
            }
            //var mactive2 = ctxTFAT.DriverMaster.Where(x => (x.VehicleNo.ToUpper().Trim() == mList.TruckNo.ToUpper().Trim())).Select(x => x.Name).FirstOrDefault();
            //if (mactive2 != null)
            //{
            //    mactivestring = mactivestring + "\nThis Vehicle Connected To " + mactive2 + " In Driver Master ";
            //}
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

                    //var vehicleDriver = ctxTFAT.VehicleDri_Hist.Where(x => x.TruckNo.Trim().ToLower() == mList.TruckNo.Trim().ToLower() && x.ToPeriod == null).FirstOrDefault();
                    //if (vehicleDriver != null)
                    //{
                    //    vehicleDriver.ToPeriod = DateTime.Now;
                    //    ctxTFAT.Entry(vehicleDriver).State = EntityState.Modified;
                    //}
                    //var DriverMaster = ctxTFAT.DriverMaster.Where(x => (x.VehicleNo.ToUpper().Trim() == mList.TruckNo.ToUpper().Trim())).FirstOrDefault();
                    //if (DriverMaster != null)
                    //{
                    //    DriverMaster.VehicleNo = "";
                    //    ctxTFAT.Entry(DriverMaster).State = EntityState.Modified;
                    //}

                    ctxTFAT.VehicleMaster.Remove(mList);
                    var VehicleAttachmentList = ctxTFAT.Attachment.Where(x => x.Type == "Vehic" && x.Srl == mList.Code).ToList();
                    foreach (var item in VehicleAttachmentList)
                    {
                        if (System.IO.File.Exists(item.FilePath))
                        {
                            System.IO.File.Delete(item.FilePath);
                        }
                    }
                    ctxTFAT.Attachment.RemoveRange(VehicleAttachmentList);
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mList.Code, DateTime.Now, 0, mList.Code, "Delete Vehicle Master", "VM");

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

        public ActionResult Insert_Lr_List_IN_LC(VehicleMasterVM mModel)
        {
            var html = ViewHelper.RenderPartialView(this, "_VehicleMasterPartialView", mModel);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }


        #region Attachment (Download,View,Delete,Save)
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
                    att.Type = String.IsNullOrEmpty(item.Type) == true ? "Vehic" : item.Type;
                    att.ParentKey = Model.ParentKey;
                    att.DocType = item.DocType;

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



        #region Tracking Vehicle

        public ActionResult TrackID(string VehicleCode)
        {
            string Status = "Sucess", Latitude = "", Longitude = "", Vehicle = "";

            TfatVehicleTrackApiList tfatVehicleTrackApi = ctxTFAT.TfatVehicleTrackApiList.Where(x => x.VehicleList.Contains(VehicleCode)).FirstOrDefault();
            var SetUrl = "";
            if (!String.IsNullOrEmpty(tfatVehicleTrackApi.TrackApi))
            {
                SetUrl += tfatVehicleTrackApi.TrackApi;
            }
            if (!String.IsNullOrEmpty(tfatVehicleTrackApi.Username))
            {
                SetUrl += tfatVehicleTrackApi.Username;
            }
            if (!String.IsNullOrEmpty(tfatVehicleTrackApi.Password))
            {
                SetUrl += tfatVehicleTrackApi.Password;
            }
            if (!String.IsNullOrEmpty(tfatVehicleTrackApi.Para1))
            {
                SetUrl += tfatVehicleTrackApi.Para1;
            }
            if (!String.IsNullOrEmpty(tfatVehicleTrackApi.Para2))
            {
                SetUrl += tfatVehicleTrackApi.Para2;
            }
            if (!String.IsNullOrEmpty(tfatVehicleTrackApi.Para3))
            {
                SetUrl += tfatVehicleTrackApi.Para3;
            }
            if (!String.IsNullOrEmpty(tfatVehicleTrackApi.Para4))
            {
                SetUrl += tfatVehicleTrackApi.Para4;
            }
            if (!String.IsNullOrEmpty(tfatVehicleTrackApi.Para5))
            {
                SetUrl += tfatVehicleTrackApi.Para5;
            }
            if (VehicleCode.Contains("H"))
            {
                SetUrl = SetUrl.Replace("@vehiclePara", ctxTFAT.HireVehicleMaster.Where(x => x.Code == VehicleCode).Select(x => x.TruckNo).FirstOrDefault().Replace(" ", ""));
            }
            else
            {
                SetUrl = SetUrl.Replace("@vehiclePara", ctxTFAT.VehicleMaster.Where(x => x.Code == VehicleCode).Select(x => x.TruckNo).FirstOrDefault().Replace(" ", ""));
            }





            var GenerateUrl = "http://api.ilogistek.com:6049/tracking?AuthKey=PV7q60SurjHlacX398hbQ4SiWfYktoL&vehicle_no=" + VehicleCode;
            WebClient client = new WebClient();
            string jsonstring = client.DownloadString(SetUrl);
            dynamic dynObj = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonstring);
            var Check = dynObj.VehicleNo;
            if (SetUrl.Contains("ilogistek"))
            {
                foreach (var item in dynObj)
                {
                    Check = item["VehicleNo"];
                }
            }
            else if (SetUrl.Contains("elixiatech"))
            {
                var status = dynObj.Status;

                if (status.Value == "0")
                {
                    Check = null;
                }
                else
                {
                    Check = "Success";
                }
            }
            if (Check == null)
            {
                Status = "Error";
            }
            else
            {
                if (SetUrl.Contains("ilogistek"))
                {
                    foreach (var item in dynObj)
                    {
                        Latitude = item["Latitude"];
                        Longitude = item["Longitude"];
                        Vehicle = item["VehicleNo"];
                    }
                    //Latitude = dynObj.Latitude.Value;
                    //Longitude = dynObj.Longitude.Value;
                    //Vehicle = dynObj.VehicleNo.Value;
                }
                else if (SetUrl.Contains("elixiatech"))
                {

                    Latitude = dynObj.Result.data[0]["lat"];
                    Longitude = dynObj.Result.data[0]["lng"];
                    Vehicle = dynObj.Result.data[0]["vehicleno"];
                }
            }


            var jsonResult = Json(new
            {
                Status = Status,
                Latitude = Status == "Error" ? "" : Latitude,
                Longitude = Status == "Error" ? "" : Longitude,
                Vehicle = Status == "Error" ? "" : Vehicle,
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        #endregion
    }
}