using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class LocalDeliverySheetController : BaseController
    {

        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        public static string connstring = "";
        //ITransactionGridOperation mIlst = new TransactionGridOperation();
        private string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();


        #region Function List

        public JsonResult GetVehicleGroup(string term)
        {

            var list = ctxTFAT.VehicleCategory.Where(x => x.Acitve == true).ToList();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.VehicleCategory1.ToLower().Contains(term.ToLower())).ToList();
            }

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.VehicleCategory1
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetVehicleGroupStatus(string term)
        {
            var Branch = System.Web.HttpContext.Current.Session["BranchCode"].ToString();

            var GetGropCodeList = ctxTFAT.VehicleMaster.Where(x => x.Branch.Contains(Branch) && x.Acitve == true && x.Status == "Ready").Select(x => x.TruckStatus).ToList();
            var list = ctxTFAT.VehicleGrpStatusMas.Where(x => x.Acitve == true && GetGropCodeList.Contains(x.Code)).ToList();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.VehicleGroupStatus.ToLower().Contains(term.ToLower())).ToList();
            }

            if (list.Count() == 0)
            {
                list = ctxTFAT.VehicleGrpStatusMas.Where(x => x.Code == "100001").ToList();
            }

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.VehicleGroupStatus
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetVehicle(string term, string TruckStatus)
        {
            FMSetup fatfmsetup = ctxTFAT.FMSetup.FirstOrDefault();
            if (fatfmsetup == null)
            {
                fatfmsetup = new FMSetup();
            }
            var BranchChild = GetChildGrp(mbranchcode);
            if (TruckStatus == "100001")
            {
                List<HireVehicleMaster> list = new List<HireVehicleMaster>();

                if (fatfmsetup.VehiclesBranchWise)
                {
                    list = ctxTFAT.HireVehicleMaster.Where(x => BranchChild.Contains(x.Branch) && x.Acitve == true).ToList();
                }
                else
                {
                    list = ctxTFAT.HireVehicleMaster.Where(x => x.Acitve == true).ToList();
                }
                if (fatfmsetup.VehicleReadyStsOnly)
                {
                    list = list.Where(x => x.Status == "Ready").ToList();
                }
                if (!String.IsNullOrEmpty(TruckStatus))
                {
                    list = list.Where(x => x.TruckStatus.ToLower() == TruckStatus.ToLower()).ToList();
                }

                if (!(String.IsNullOrEmpty(term)))
                {
                    list = list.Where(x => x.TruckNo.ToLower().Contains(term.ToLower())).ToList();
                }
                var Modified = list.Select(x => new
                {
                    Code = x.TruckNo,
                    Name = x.TruckNo
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<VehicleMaster> list = new List<VehicleMaster>();

                if (fatfmsetup.VehiclesBranchWise)
                {
                    list = ctxTFAT.VehicleMaster.Where(x => BranchChild.Contains(x.Branch) && x.Acitve == true).ToList();
                }
                else
                {
                    list = ctxTFAT.VehicleMaster.Where(x => x.Acitve == true).ToList();
                }
                if (fatfmsetup.VehicleReadyStsOnly)
                {
                    list = list.Where(x => x.Status == "Ready").ToList();
                }
                if (!String.IsNullOrEmpty(TruckStatus))
                {
                    list = list.Where(x => x.TruckStatus.ToLower() == TruckStatus.ToLower()).ToList();
                }

                if (!(String.IsNullOrEmpty(term)))
                {
                    list = list.Where(x => x.TruckNo.ToLower().Contains(term.ToLower())).ToList();
                }
                var Modified = list.Select(x => new
                {
                    Code = x.TruckNo,
                    Name = x.TruckNo
                });
                return Json(Modified, JsonRequestBehavior.AllowGet);
            }


            //list = ctxTFAT.VehicleMaster.Where(x => x.Acitve == true && x.Branch.Contains(mbranchcode) && x.VehicleReportingSt == "Ready").ToList();


        }

        private static List<SelectListItem> PopulateBranches()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            //string constr = ConfigurationManager.ConnectionStrings["tfatEntitiesConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(LocalDeliverySheetController.connstring))
            {
                string query = " SELECT Code, Name FROM TfatBranch";
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

        public List<SelectListItem> PopulateBranchesList()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            //string constr = ConfigurationManager.ConnectionStrings["tfatEntitiesConnectionString"].ConnectionString;

            var BranchList = ctxTFAT.TfatBranch.Where(x => x.Category == "Branch" || x.Category == "SubBranch").Select(x => new { x.Code, x.Name, x.Category }).ToList();

            foreach (var item in BranchList)
            {
                if (item.Category == "Branch")
                {
                    items.Add(new SelectListItem
                    {
                        Text = item.Name.ToString() + "-B",
                        Value = item.Code.ToString()
                    });
                }
                else
                {
                    items.Add(new SelectListItem
                    {
                        Text = item.Name.ToString() + "-SB",
                        Value = item.Code.ToString()
                    });
                }
            }
            return items;
        }

        #region Loginwise

        public List<TfatBranch> GetBranch(string BRanchCode)
        {


            var mTreeList = ctxTFAT.TfatBranch.Select(x => new { x.Name, x.Grp, x.Code }).ToList();

            List<NFlatObject> flatObjects2 = new List<NFlatObject>();
            List<TfatBranch> GEtArea = new List<TfatBranch>();
            for (int n = 0; n < mTreeList.Count; n++)
            {
                NFlatObject abc = new NFlatObject();
                abc.data = mTreeList[n].Name;
                abc.Id = mTreeList[n].Code;
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
            var recursiveObjects = FillRecursive1(flatObjects2, BRanchCode);

            var Currentbranch = ctxTFAT.TfatBranch.Where(x => x.Code == BRanchCode).FirstOrDefault();
            GEtArea.Add(Currentbranch);
            foreach (var item in recursiveObjects)
            {
                var branch = ctxTFAT.TfatBranch.Where(x => x.Code == item.id).FirstOrDefault();
                GEtArea.Add(branch);
                if (item.children.Count > 0)
                {
                    foreach (var item1 in item.children)
                    {
                        var branch1 = ctxTFAT.TfatBranch.Where(x => x.Code == item1.id).FirstOrDefault();
                        GEtArea.Add(branch1);
                    }
                }
            }
            return GEtArea;
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

        public JsonResult From(string term)
        {
            //var GetParentCode = ctxTFAT.TfatBranch.Where(x => x.Code == mcompcode).Select(x => x.Code).FirstOrDefault();
            List<TfatBranch> list = GetBranch(mbranchcode);

            list = list.Where(x => x.Category != "0").ToList();
            var GeneralArea = ctxTFAT.TfatBranch.Where(x => x.Grp == "G00000" && x.Category == "Area").ToList();
            list.AddRange(GeneralArea);
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

        public JsonResult To(string term)
        {
            List<TfatBranch> list = ctxTFAT.TfatBranch.Where(x => x.Category != "Zone" && x.Category != "0").OrderBy(x => x.RECORDKEY).ToList();

            //list = list.Where(x => x.Category != "Area" && x.Category != "0" && (!(x.Code.Contains(From)))).ToList();
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


        public JsonResult AddDestination(string term)
        {
            List<TfatBranch> list = ctxTFAT.TfatBranch.Where(x => x.Category != "Zone" && x.Category != "0").ToList();

            //list = list.Where(x => (!(x.Code.Contains(From))) && (!(x.Code.Contains(To))) && ((x.Category != "0"))).ToList();

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

        public JsonResult PayableAt(string term)
        {
            List<TfatBranch> list = ctxTFAT.TfatBranch.Where(x => x.Category != "0" && x.Category != "Area" && x.Category != "Zone").ToList();

            //list = list.Where(x => x.Category != "0").ToList();
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
                }
                else if (item.Category == "Area")
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

        public ActionResult StoreAllDestination(string ViaRoute)
        {
            bool Add = true;
            List<LR_LC_Combine_VM> lR_LC_Combine_VMs = new List<LR_LC_Combine_VM>();
            List<LR_LC_Combine_VM> SessionDestList = TempData.Peek("Destination") as List<LR_LC_Combine_VM>;
            if (SessionDestList != null)
            {
                lR_LC_Combine_VMs = SessionDestList;
                foreach (var item in lR_LC_Combine_VMs)
                {
                    if (item.From == ViaRoute)
                    {
                        Add = false;
                        break;
                    }
                }
            }
            if (Add)
            {
                var FromName = ctxTFAT.TfatBranch.Where(x => x.Code == ViaRoute).Select(x => x.Name).FirstOrDefault();

                LR_LC_Combine_VM lR_LC_Combine_VM = new LR_LC_Combine_VM
                {
                    Consigner = FromName,
                    From = ViaRoute,
                };
                lR_LC_Combine_VMs.Add(lR_LC_Combine_VM);
            }


            TempData["Destination"] = lR_LC_Combine_VMs;

            FMVM fMVM = new FMVM();
            fMVM.AllDest = lR_LC_Combine_VMs;
            LocalDeliverySheetVM localDeliverySheetVM = new LocalDeliverySheetVM();
            localDeliverySheetVM.fMVM = fMVM;

            var html = ViewHelper.RenderPartialView(this, "_Get_Destination_List", localDeliverySheetVM);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult ShowListBox()
        {
            LocalDeliverySheetVM fMVM = new LocalDeliverySheetVM();
            connstring = GetConnectionString();
            fMVM.Branches = PopulateBranches();

            List<LR_LC_Combine_VM> SessionDestList = TempData.Peek("Destination") as List<LR_LC_Combine_VM>;
            string Code = "";
            if (SessionDestList != null)
            {
                foreach (var item in SessionDestList)
                {
                    Code += item.From + ",";
                }
                if (!String.IsNullOrEmpty(Code))
                {
                    fMVM.AppBranch = Code.Substring(0, Code.Length - 1);
                }

            }



            var html = ViewHelper.RenderPartialView(this, "_PartialListBox", fMVM);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public ActionResult FetChGrupStatus(string VehicleNo)
        {
            string Payload = "", BroCode = "", BroNAme = "", VehicleCategoryCode = "", VehicleCategoryName = "", OwnerName = "", DriverName = "", LicenNo = "", LicenceExpDate = "", VehicleGroupStatusCode = "", VehicleGroupStatusName = "";
            int KM = 0;
            VehicleMaster vehicleMaster = ctxTFAT.VehicleMaster.Where(x => x.TruckNo == VehicleNo).FirstOrDefault();
            if (vehicleMaster != null)
            {
                OwnerName = vehicleMaster.Owner;
                VehicleGroupStatusCode = vehicleMaster.TruckStatus;
                VehicleGroupStatusName = ctxTFAT.VehicleGrpStatusMas.Where(x => x.Code == vehicleMaster.TruckStatus).Select(x => x.VehicleGroupStatus).FirstOrDefault();
                VehicleCategoryCode = vehicleMaster.VCategory;
                VehicleCategoryName = ctxTFAT.VehicleCategory.Where(x => x.Code == vehicleMaster.VCategory).Select(x => x.VehicleCategory1).FirstOrDefault();
                BroCode = vehicleMaster.Broker;
                BroNAme = ctxTFAT.Master.Where(x => x.Code.ToString() == vehicleMaster.Broker).Select(x => x.Name).FirstOrDefault();
                Payload = vehicleMaster.PayLoad.ToString();
                DriverMaster driverMaster = ctxTFAT.DriverMaster.Where(x => x.Code == vehicleMaster.Driver).FirstOrDefault();
                if (driverMaster == null)
                {
                    DriverName = "";
                }
                else
                {
                    DriverName = driverMaster.Name;
                    LicenNo = driverMaster.LicenceNo;
                    LicenceExpDate = driverMaster.LicenceExpDate == null ? null : driverMaster.LicenceExpDate.Value.ToShortDateString();
                }


                KM = vehicleMaster.KM;
            }
            else
            {
                HireVehicleMaster hireVehicle = ctxTFAT.HireVehicleMaster.Where(x => x.TruckNo.Trim() == VehicleNo.Trim()).FirstOrDefault();
                if (hireVehicle != null)
                {
                    VehicleGroupStatusCode = hireVehicle.TruckStatus;
                    VehicleGroupStatusName = ctxTFAT.VehicleGrpStatusMas.Where(x => x.Code == hireVehicle.TruckStatus).Select(x => x.VehicleGroupStatus).FirstOrDefault();
                    VehicleCategoryCode = hireVehicle.VCategory;
                    VehicleCategoryName = ctxTFAT.VehicleCategory.Where(x => x.Code == hireVehicle.VCategory).Select(x => x.VehicleCategory1).FirstOrDefault();
                    BroCode = hireVehicle.Broker;
                    BroNAme = ctxTFAT.Master.Where(x => x.Code.ToString() == hireVehicle.Broker).Select(x => x.Name).FirstOrDefault();
                    Payload = hireVehicle.PayLoad.ToString();
                    DriverName = hireVehicle.Driver;
                }
            }
            return Json(new
            {
                VehicleNo = VehicleNo,
                BroNAme = BroNAme,
                OwnerName = OwnerName,
                VehicleGroupStatusCode = VehicleGroupStatusCode,
                VehicleGroupStatusName = VehicleGroupStatusName,
                VehicleCategoryCode = VehicleCategoryCode,
                VehicleCategoryName = VehicleCategoryName,
                BroCode = BroCode,
                Payload = Payload,
                DriverName = DriverName,
                LicenNo = LicenNo,
                LicenceExpDate = LicenceExpDate,
                KM = KM,
                JsonRequestBehavior.AllowGet
            });
        }

        public ActionResult GetAllField(string VehicleNo, string From, string To)
        {
            string DriverCode = "", OwnerName = "", DriverName = "", LicenNo = "", KM = "", PayLoad = "";
            decimal Rate = 0, AdvRate = 0;
            DateTime LicenceExpDate = DateTime.Now;
            OwnerName = ctxTFAT.VehicleMaster.Where(x => x.TruckNo == VehicleNo).Select(x => x.Owner).FirstOrDefault();
            DriverCode = ctxTFAT.VehicleMaster.Where(x => x.TruckNo == VehicleNo).Select(x => x.Driver).FirstOrDefault();
            PayLoad = ctxTFAT.VehicleMaster.Where(x => x.TruckNo == VehicleNo).Select(x => x.PayLoad.ToString()).FirstOrDefault();

            DriverName = ctxTFAT.DriverMaster.Where(x => x.Code == DriverCode).Select(x => x.Name).FirstOrDefault();
            LicenNo = ctxTFAT.DriverMaster.Where(x => x.Code == DriverCode).Select(x => x.LicenceNo).FirstOrDefault();
            LicenceExpDate = ctxTFAT.DriverMaster.Where(x => x.Code == DriverCode).Select(x => x.LicenceExpDate.Value).FirstOrDefault();
            Rate = Convert.ToDecimal(ctxTFAT.VehicleRates.Where(x => x.VehicleNo == VehicleNo && x.FromBranch == From && x.ToBranch == To).Select(x => x.Rate).FirstOrDefault());
            AdvRate = Convert.ToDecimal(ctxTFAT.VehicleRates.Where(x => x.VehicleNo == VehicleNo && x.FromBranch == From && x.ToBranch == To).Select(x => x.AdvRate).FirstOrDefault());
            KM = ctxTFAT.VehicleMaster.Where(x => x.TruckNo == VehicleNo).Select(x => x.KM).FirstOrDefault().ToString();
            return Json(new { PayLoad = PayLoad, OwnerName = OwnerName, DriverName = DriverName, LicenNo = LicenNo, LicenceExpDate = LicenceExpDate, Rate = Rate, AdvRate = AdvRate, KM = KM, JsonRequestBehavior.AllowGet });
        }

        public JsonResult GetBroker(string term)
        {
            var result = ctxTFAT.Master.Where(x => x.Hide == false /*&& x.IsLast== true*/ && x.AcType == "S" && x.RelatedTo == "6").Select(m => new { m.Code, m.Name }).ToList();
            if (!(String.IsNullOrEmpty(term)))
            {
                result = ctxTFAT.Master.Where(x => x.Hide == false /*&& x.IsLast== true*/ && x.AcType == "S" && x.RelatedTo == "6" && x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).ToList();
            }
            var Modified = result.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);

        }

        public ActionResult CheckLDSNO(string LDSNO, string DocumentNO)
        {
            int docNo = Convert.ToInt32(DocumentNO);
            string message = "";

            LocalDeliverySheet localDeliverySheet = ctxTFAT.LocalDeliverySheet.Where(x => x.LDSNo.ToString() == LDSNO && x.RECORDKEY != docNo).FirstOrDefault();
            if (localDeliverySheet == null)
            {
                return Json(new { Message = "T", JsonRequestBehavior.AllowGet });
            }
            else
            {
                return Json(new { Message = "F", JsonRequestBehavior.AllowGet });
            }
        }

        public ActionResult CheckLDSDate(string DocDate, string DocTime)
        {
            string message = "";
            var Date = DocDate.Split('/');
            string[] Time = DocTime.Split(':');
            DateTime Ldsdate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Time[0]), Convert.ToInt32(Time[1]), 00);

            LDSsetup lDSsetup = ctxTFAT.LDSsetup.FirstOrDefault();
            if (lDSsetup == null)
            {
                var MinDate = DateTime.Now.AddHours(lDSsetup.BeforeLDSDate * (-1));
                var MaxDate = DateTime.Now.AddHours(lDSsetup.AfterLDSDate);

                if (MinDate <= Ldsdate && Ldsdate <= MaxDate)
                {
                    message = "T";
                }
                else
                {
                    message = "F";
                }
            }
            else
            {
                message = "T";
            }
            return Json(new { Message = message, JsonRequestBehavior.AllowGet });
        }


        public ActionResult CheckManual(int No, string document)
        {
            string Flag = "F";
            string Msg = "";
            bool checkalloctionFound = false;
            List<TblBranchAllocation> tblBranchAllocations = tblBranchAllocations = ctxTFAT.TblBranchAllocation.Where(x => x.Branch == mbranchcode).ToList();

            foreach (var item in tblBranchAllocations)
            {
                if (item.ManualFrom <= No && No <= item.ManualTo)
                {
                    checkalloctionFound = true;
                    break;
                }
            }

            if (checkalloctionFound)
            {
                LocalDeliverySheet local = ctxTFAT.LocalDeliverySheet.Where(x => x.LDSNo == No && x.LDSNo.ToString() != document).FirstOrDefault();
                if (local != null)
                {
                    Flag = "T";
                    Msg = "This LDSNo Exist \nSo,Please Change LDS No....!";
                }
            }
            else
            {
                Flag = "T";
                Msg = "Manual Range Not Found....!";
            }

            return Json(new { Message = Flag, Msg = Msg, JsonRequestBehavior.AllowGet });
        }

        public ActionResult CheckAuto(int No, string document)
        {
            string Flag = "F";
            string Msg = "";

            LocalDeliverySheet local = ctxTFAT.LocalDeliverySheet.Where(x => x.LDSNo == No && x.LDSNo.ToString() != document).FirstOrDefault();
            if (local != null)
            {
                Flag = "T";
                Msg = "Please Change LDS No....!";
            }
            return Json(new { Message = Flag, Msg = Msg, JsonRequestBehavior.AllowGet });
        }



        #endregion



        // GET: Logistics/LocalDeliverySheet

        public ActionResult Index(LocalDeliverySheetVM mModel)
        {

            TempData.Remove("Destination");
            TempData.Remove("FMAttachmentList");

            TempData.Remove("ExistingLRLIST");
            TempData.Remove("AllStockLRLIst");
            TempData.Remove("AllDoorStockLRLIst");
            TempData.Remove("AllTransitLRLIst");

            List<LcDetailsVM> lCDetails = new List<LcDetailsVM>();

            mModel.LDSsetup = ctxTFAT.LDSsetup.Where(x => (x.RECORDKEY == 1)).FirstOrDefault();
            if (mModel.LDSsetup == null)
            {
                mModel.LDSsetup = new LDSsetup();
            }
            string BranchCode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "A");
            mdocument = mModel.Document;

            connstring = GetConnectionString();
            mModel.Branches = PopulateBranches();
            mModel.BranchList = PopulateBranchesList();

            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {

                #region Attachment
                LocalDeliverySheet localDeliverySheet = ctxTFAT.LocalDeliverySheet.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
                if (!String.IsNullOrEmpty(localDeliverySheet.LDSNo.ToString()))
                {

                }
                #endregion

                mModel.LDSNo = localDeliverySheet.LDSNo;
                mModel.Time = localDeliverySheet.Time;
                mModel.Date = localDeliverySheet.Date.ToShortDateString();
                mModel.VehicleType = localDeliverySheet.VehicleType;
                mModel.VehicleTypeName = ctxTFAT.VehicleGrpStatusMas.Where(x => x.Code == mModel.VehicleType).Select(x => x.VehicleGroupStatus).FirstOrDefault();
                mModel.VehicleNo = localDeliverySheet.VehicleNo;
                mModel.Broker = mModel.Broker;
                mModel.BrokerName = ctxTFAT.Broker.Where(x => x.Code.ToString() == mModel.Broker).Select(x => x.Name).FirstOrDefault();
                mModel.KM = localDeliverySheet.KM == null ? "0" : Convert.ToInt32(localDeliverySheet.KM).ToString();
                mModel.From = localDeliverySheet.FromBranch;
                mModel.FromName = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.From).Select(x => x.Name).FirstOrDefault();

                #region GetAll RouteVia


                if (!(String.IsNullOrEmpty(localDeliverySheet.ToBranch)))
                {
                    List<LR_LC_Combine_VM> lR_LC_Combine_VMs = new List<LR_LC_Combine_VM>();
                    var GetSourceArry = localDeliverySheet.ToBranch.Split(',');

                    for (int i = 0; i < GetSourceArry.Length; i++)
                    {
                        if (i == GetSourceArry.Length - 1)
                        {
                            mModel.To = GetSourceArry[i];
                            //mModel.ToName = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.To).Select(x => x.Name).FirstOrDefault();
                            var TO = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.To).Select(x => new { x.Name, x.Category }).FirstOrDefault();
                            if (TO.Category == "Zone")
                            {
                                mModel.ToName = TO.Name + " - Z";
                            }
                            else if (TO.Category == "Branch")
                            {
                                mModel.ToName = TO.Name + " - B";

                            }
                            else if (TO.Category == "SubBranch")
                            {
                                mModel.ToName = TO.Name + " - SB";
                            }
                            else
                            {
                                mModel.ToName = TO.Name + " - A";
                            }
                        }
                        else
                        {
                            var SourceCode = GetSourceArry[i];


                            var SourceName = ctxTFAT.TfatBranch.Where(x => x.Code == SourceCode).Select(x => x.Name).FirstOrDefault();
                            LR_LC_Combine_VM lR_LC_Combine_VM = new LR_LC_Combine_VM
                            {
                                Consigner = SourceName,
                                From = SourceCode,
                            };
                            lR_LC_Combine_VMs.Add(lR_LC_Combine_VM);
                        }

                    }
                    TempData["Destination"] = lR_LC_Combine_VMs;
                    //mModel.AllDest = lR_LC_Combine_VMs;
                }


                #endregion


                mModel.VehicleCategory = localDeliverySheet.VehicleCategory;
                mModel.VehicleCategoryName = ctxTFAT.VehicleCategory.Where(x => x.Code.ToString() == mModel.VehicleCategory).Select(x => x.VehicleCategory1).FirstOrDefault();
                mModel.ReceiptNo = localDeliverySheet.ReceiptNo;
                mModel.DriverName = localDeliverySheet.Driver;
                mModel.LicenceNo = localDeliverySheet.LicenceNo;
                mModel.LicenceExpDate = localDeliverySheet.LicenceExpDate==null?"": localDeliverySheet.LicenceExpDate.Value.ToShortDateString();
                mModel.Owner = localDeliverySheet.Owner;
                mModel.ChallanNo = localDeliverySheet.ChallanNo;
                mModel.ContactNo = localDeliverySheet.ContactNo.ToString();
                mModel.Freight = localDeliverySheet.Freight.ToString();
                mModel.Advance = localDeliverySheet.Advance.ToString();
                mModel.Balance = localDeliverySheet.Balalnce.ToString();
                mModel.PayableAt = localDeliverySheet.PayableAt;
                mModel.PayableAtName = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.PayableAt).Select(x => x.Name).FirstOrDefault();
                mModel.Remark = localDeliverySheet.Remark;
                mModel.PayLoad = localDeliverySheet.PayLoad.ToString();

                #region LorryReceipt Expenses

                var mList = ctxTFAT.LorryReceiptExpenses.Where(x => x.DocNo == localDeliverySheet.LDSNo.ToString() && x.ParentKey==localDeliverySheet.TableKey).ToList().Count();
                if (mList != 0)
                {
                    var lCDetail = ctxTFAT.LorryReceiptExpenses.Where(x => x.DocNo == localDeliverySheet.LDSNo.ToString() && x.ParentKey == localDeliverySheet.TableKey).ToList();
                    foreach (var item in lCDetail)
                    {
                        LRStock lRStock = new LRStock();
                        var RecordKey = Convert.ToInt32(10);
                        lRStock = ctxTFAT.LRStock.Where(x => x.TableKey == item.ParentKey).FirstOrDefault();
                        LcDetailsVM lcDetailsVM = new LcDetailsVM();
                        lcDetailsVM.EditLDSNo = true;

                        var GetFmno = ctxTFAT.LCMaster.Where(x => x.LCno.ToString() == lRStock.LCNO.ToString()).Select(x => x.DispachFM).FirstOrDefault();
                        var CheckVehicleInCurrentBRanchOrNot = ctxTFAT.FMMaster.Where(x => x.FmNo == GetFmno).FirstOrDefault();
                        if (CheckVehicleInCurrentBRanchOrNot!=null)
                        {
                            if (CheckVehicleInCurrentBRanchOrNot.FmStatus == "W")
                            {
                                lcDetailsVM.EditLDSNo = true;
                            }
                            else
                            {
                                lcDetailsVM.EditLDSNo = false;
                            }
                        }
                        

                        lcDetailsVM.recordekey = (item.ParentKey == "" ? "0" : item.ParentKey);
                        lcDetailsVM.Qty = lRStock.AllocatBalQty + Convert.ToInt32(item.Qty);
                        lcDetailsVM.LRActWeight = Convert.ToInt32(item.Weight);
                        lcDetailsVM.ChrWeight = lRStock.ChrgWeight;
                        lcDetailsVM.ActWeight = Math.Abs(((lRStock.AllocatBalQty + Convert.ToInt32(item.Qty)) * (lRStock.ActWeight)) / (lRStock.TotalQty));
                        lcDetailsVM.Lrno = Convert.ToInt32(item.LRno);
                        //lcDetailsVM.PickType = item.PickType;
                        lcDetailsVM.ChrgeType = ctxTFAT.ChargeTypeMaster.Where(x => x.ChargeType.ToLower().Trim() == lRStock.ChrgType.ToLower().Trim()).Select(x => x.ChargeType).FirstOrDefault();
                        lcDetailsVM.Description = ctxTFAT.DescriptionMaster.Where(x => x.Code == lRStock.Description).Select(x => x.Description).FirstOrDefault();
                        lcDetailsVM.Unit = ctxTFAT.UnitMaster.Where(x => x.Code == lRStock.Unit).Select(x => x.Name).FirstOrDefault();
                        lcDetailsVM.From = ctxTFAT.TfatBranch.Where(x => x.Code == lRStock.FromBranch).Select(x => x.Name).FirstOrDefault();
                        lcDetailsVM.To = ctxTFAT.TfatBranch.Where(x => x.Code == lRStock.ToBranch).Select(x => x.Name).FirstOrDefault();
                        lcDetailsVM.Consignor = ctxTFAT.Consigner.Where(x => x.Code == lRStock.Consigner).Select(x => x.Name).FirstOrDefault();
                        lcDetailsVM.Consignee = ctxTFAT.Consigner.Where(x => x.Code == lRStock.Consignee).Select(x => x.Name).FirstOrDefault();
                        lcDetailsVM.LrType = ctxTFAT.LRTypeMaster.Where(x => x.Code == lRStock.LrType).Select(x => x.LRType).FirstOrDefault();
                        lcDetailsVM.LRDelivery = lRStock.Delivery == "Godown" ? "Godown" : ctxTFAT.TfatBranch.Where(x => x.Code == lRStock.Delivery).Select(x => x.Name).FirstOrDefault();
                        lcDetailsVM.LRColln = lRStock.Coln == "G" ? "Godown" : lRStock.Coln == "D" ? "Door" : "Crossing";
                        lcDetailsVM.LoadGQty = Convert.ToInt32(item.Qty);
                        lcDetailsVM.Amount = Convert.ToInt32(item.Amount);
                        lcDetailsVM.ShowAmountColumn = true;
                        lcDetailsVM.ShowLoadQtyANdWeight = true;
                        lCDetails.Add(lcDetailsVM);

                    }
                }

                #endregion

                TempData["ExistingLRLIST"] = lCDetails;
            }
            else
            {
                mModel.From = BranchCode;
                mModel.FromName = ctxTFAT.TfatBranch.Where(x => x.Code == BranchCode).Select(x => x.Name).FirstOrDefault();
                mModel.Date = DateTime.Now.ToShortDateString();
                mModel.Time = DateTime.Now.ToString("HH:mm");
                var newFMNo = ctxTFAT.LocalDeliverySheet.Where(x => x.GenerateType == "A").OrderByDescending(x => x.RECORDKEY).Select(x => x.LDSNo).Take(1).FirstOrDefault();
                if (newFMNo == null || newFMNo == 0)
                {
                    mModel.LDSNo = 1;
                }
                else
                {
                    mModel.LDSNo = (Convert.ToInt32(newFMNo) + 1);
                }
            }
            Godown_LR_Stock_List("");
            Door_LR_Stock_List("");
            Transit_LR_List("");
            mModel.lcDetailsVMs = lCDetails;
            return View(mModel);
        }

        public ActionResult SaveData(LocalDeliverySheetVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        string Msg = DeleteStateMaster(mModel);
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
                    bool mAdd = true;


                    LocalDeliverySheet localDeliverySheet = new LocalDeliverySheet();
                    List<LorryReceiptExpenses> lorryReceiptExpenses = new List<LorryReceiptExpenses>();
                    List<LocalDeliveryRel> LocalDeliveryRels = new List<LocalDeliveryRel>();
                    List<LRStock> lRStocks = new List<LRStock>();
                    VehicleMaster vehicleMaster = ctxTFAT.VehicleMaster.Where(x => x.TruckNo == mModel.VehicleNo).FirstOrDefault(); ;
                    var Child = GetChildGrp(mbranchcode);



                    int SheetCode = Convert.ToInt32(mModel.Document);
                    if (ctxTFAT.LocalDeliverySheet.Where(x => x.RECORDKEY == SheetCode).FirstOrDefault() != null)
                    {
                        localDeliverySheet = ctxTFAT.LocalDeliverySheet.Where(x => x.RECORDKEY == SheetCode).FirstOrDefault();
                        lorryReceiptExpenses = ctxTFAT.LorryReceiptExpenses.Where(x => x.DocNo == localDeliverySheet.LDSNo.ToString() && x.ParentKey==localDeliverySheet.TableKey).ToList();
                        LocalDeliveryRels = ctxTFAT.LocalDeliveryRel.Where(x => x.LDSNO == localDeliverySheet.LDSNo).ToList();
                        lRStocks = ctxTFAT.LRStock.Where(x => x.LCNO == localDeliverySheet.LDSNo && x.StockStatus == "L" && x.StockAt == "LDS").ToList();
                        mAdd = false;

                        #region Remove LorryReceiptExpenses

                        foreach (var item in lRStocks)
                        {
                            ctxTFAT.LRStock.Remove(item);
                        }
                        foreach (var item in lorryReceiptExpenses)
                        {
                                LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey.ToString() == item.ParentKey.ToString()).FirstOrDefault();

                                lRStock.AllocatBalQty += Convert.ToInt32(item.Qty);
                                lRStock.BalQty += Convert.ToInt32(item.Qty);
                                lRStock.AllocatBalWght += Convert.ToInt32(item.Weight);
                                lRStock.BalWeight += Convert.ToInt32(item.Weight);

                                ctxTFAT.Entry(lRStock).State = EntityState.Modified;
                                ctxTFAT.LorryReceiptExpenses.Remove(item); 
                        }

                        foreach (var item in LocalDeliveryRels)
                        {
                            ctxTFAT.LocalDeliveryRel.Remove(item);
                        }

                        #endregion

                        #region Remove LDS Entry From LRstock

                        var LDsEntryFromLRStock = ctxTFAT.LRStock.Where(x => x.StockStatus == "L" && x.LCNO == localDeliverySheet.LDSNo).ToList();
                        foreach (var item in LDsEntryFromLRStock)
                        {
                            ctxTFAT.LRStock.Remove(item);
                        }
                        #endregion

                        #region Remove Attachment
                        if (!String.IsNullOrEmpty(localDeliverySheet.LDSNo.ToString()))
                        {

                        }

                        #endregion
                    }


                    if (mAdd)
                    {
                        localDeliverySheet.LDSNo = mModel.LDSNo;
                        localDeliverySheet.GenerateType = mModel.Generate;
                    }

                    #region Update Vehicle Status

                    vehicleMaster.Status = "Transit";
                    ctxTFAT.Entry(vehicleMaster).State = EntityState.Modified;
                    #endregion

                    localDeliverySheet.Branch = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
                    localDeliverySheet.LoginBranch = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
                    localDeliverySheet.Date = ConvertDDMMYYTOYYMMDD(mModel.Date);
                    localDeliverySheet.CreateDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                    localDeliverySheet.Time = mModel.Time;
                    localDeliverySheet.VehicleType = mModel.VehicleType;
                    localDeliverySheet.VehicleCategory = mModel.VehicleCategory;
                    localDeliverySheet.VehicleNo = mModel.VehicleNo;
                    localDeliverySheet.FromBranch = mModel.From;
                    localDeliverySheet.ToBranch = mModel.DestCombo != null ? mModel.DestCombo + "," + mModel.To : mModel.To;
                    localDeliverySheet.Driver = mModel.DriverName;
                    localDeliverySheet.Freight = Convert.ToDouble(mModel.Freight);
                    localDeliverySheet.Advance = Convert.ToDouble(mModel.Advance);
                    localDeliverySheet.Balalnce = Convert.ToDouble(mModel.Freight) - Convert.ToDouble(mModel.Advance);
                    localDeliverySheet.Remark = mModel.Remark;
                    localDeliverySheet.KM = Convert.ToInt32(mModel.KM);
                    localDeliverySheet.ReceiptNo = mModel.ReceiptNo;
                    localDeliverySheet.LicenceNo = mModel.LicenceNo;
                    if (mModel.LicenceExpDate != null)
                    {
                        localDeliverySheet.LicenceExpDate = ConvertDDMMYYTOYYMMDD(mModel.LicenceExpDate);
                    }

                    localDeliverySheet.Owner = mModel.Owner;
                    localDeliverySheet.ChallanNo = mModel.ChallanNo;
                    localDeliverySheet.ContactNo = (mModel.ContactNo);
                    localDeliverySheet.PayableAt = mModel.PayableAt;
                    localDeliverySheet.PayLoad = Convert.ToInt32(mModel.PayLoad);

                    localDeliverySheet.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    localDeliverySheet.ENTEREDBY = muserid;
                    localDeliverySheet.AUTHORISE = mauthorise;
                    localDeliverySheet.AUTHIDS = muserid;
                    localDeliverySheet.TableKey = "LDS_" + Convert.ToInt32(localDeliverySheet.LDSNo).ToString("D6");
                    localDeliverySheet.ParentKey = "LDS_" + Convert.ToInt32(localDeliverySheet.LDSNo).ToString("D6");

                    List<FMAttachment> SessionAttachList = TempData.Peek("FMAttachmentList") as List<FMAttachment>;
                    if (SessionAttachList != null)
                    {
                        string AttachmentCodelist = "";
                        var LastCode = ctxTFAT.Attachment.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).Take(1).FirstOrDefault();
                        int Code;
                        if (LastCode == null)
                        {
                            Code = 1;
                        }
                        else
                        {
                            Code = Convert.ToInt32(LastCode) + 1;
                        }
                        foreach (var item in SessionAttachList)
                        {
                            AttachmentCodelist += Code + ",";
                            //Attachment FM_Attachment = new Attachment
                            //{
                            //    Branch = System.Web.HttpContext.Current.Session["BranchCode"].ToString(),
                            //    Module = "LDS",
                            //    ParentCode = localDeliverySheet.LDSNo,
                            //    Code = (Convert.ToInt32(Code)).ToString(),
                            //    ContentType = item.ContentType,
                            //    DocumentString = item.DocumentString,
                            //    FileName = item.FileName,
                            //    AUTHIDS = muserid,
                            //    AUTHORISE = mauthorise,
                            //    ENTEREDBY = muserid,
                            //    LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString()).ToString(),
                            //    Type_Of_Attachement = ""
                            //};

                            //ctxTFAT.Attachment.Add(FM_Attachment);
                            ++Code;
                        }
                        //localDeliverySheet.AttachmentCode = AttachmentCodelist.Substring(0, AttachmentCodelist.Length - 1);
                    }

                    List<LCDetail> lCDetails = TempData.Peek("ExistingLRLIST") as List<LCDetail>;
                    var lrno = mModel.LRNoList.Split(',');
                    var loadQty = mModel.LoadQuantity.Split(',');
                    var loadWeight = mModel.LoadWeight.Split(',');
                    var amount = mModel.Amount.Split(',');
                    var PickType = mModel.PickType.Split(',');
                    var RecordKey = mModel.RecordKey.Split(',');


                    //var LastRecorDkeyOfLrStock = ctxTFAT.LRStock.OrderByDescending(x => x.RECORDKEY).Select(x => x.RECORDKEY).FirstOrDefault();
                    //for (int i = 0; i < lrno.Length; i++)
                    //{
                    //    var Recordkey = RecordKey[i];
                    //    var LRno = lrno[i];
                    //    if (PickType[i] == "GDStock" || PickType[i] == "PickStock")
                    //    {
                    //        LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey.ToString() == Recordkey).FirstOrDefault();
                    //        LorryReceiptExpenses lorryReceipt = new LorryReceiptExpenses();
                    //        {
                    //            lorryReceipt.LDSNo = mModel.LDSNo;
                    //            lorryReceipt.LRno = Convert.ToInt32(lrno[i]);
                    //            lorryReceipt.Qty = Convert.ToInt32(loadQty[i]);
                    //            lorryReceipt.Amount = Convert.ToDouble(amount[i]);
                    //            lorryReceipt.ExpModule = "LDS";
                    //            lorryReceipt.ExpType = "Door";
                    //            lorryReceipt.Weight = loadWeight[i];
                    //            lorryReceipt.PickType = PickType[i];
                    //            lorryReceipt.ENTEREDBY = muserid;
                    //            lorryReceipt.AUTHORISE = mauthorise;
                    //            lorryReceipt.AUTHIDS = muserid;
                    //            lorryReceipt.TableKey = "LRExp_" + Convert.ToInt32(localDeliverySheet.LDSNo).ToString("D6") + "_" + i;
                    //            lorryReceipt.ParentKey = lRStock.TableKey;
                    //            lorryReceipt.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    //        }
                    //        LocalDeliveryRel LocalDeliveryRel = new LocalDeliveryRel();
                    //        {
                    //            LocalDeliveryRel.LDSNO = mModel.LDSNo;
                    //            LocalDeliveryRel.LRno = Convert.ToInt32(lrno[i]);
                    //            LocalDeliveryRel.Qty = Convert.ToInt32(loadQty[i]);
                    //            LocalDeliveryRel.Amount = Convert.ToDouble(amount[i]);
                    //            LocalDeliveryRel.ExpModule = "LDS";
                    //            LocalDeliveryRel.ExpType = "Door";
                    //            LocalDeliveryRel.Weight = Convert.ToDouble(loadWeight[i]);
                    //            LocalDeliveryRel.PickType = PickType[i];
                    //            LocalDeliveryRel.UnloadQty = 0;
                    //            LocalDeliveryRel.UnloadWeight = 0;
                    //            LocalDeliveryRel.ENTEREDBY = muserid;
                    //            LocalDeliveryRel.AUTHORISE = mauthorise;
                    //            LocalDeliveryRel.AUTHIDS = muserid;
                    //            LocalDeliveryRel.TableKey = "LDRel_" + Convert.ToInt32(localDeliverySheet.LDSNo).ToString("D6") + "_" + i;
                    //            LocalDeliveryRel.ParentKey = lRStock.TableKey;
                    //            LocalDeliveryRel.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    //        }
                    //        LRStock stock = new LRStock();
                    //        {
                    //            stock.Branch = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
                    //            stock.LoginBranch = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
                    //            stock.LrNo = Convert.ToInt32(lrno[i]);
                    //            stock.Date = ConvertDDMMYYTOYYMMDD(mModel.Date);
                    //            stock.Time = mModel.Time;
                    //            stock.TotalQty = Convert.ToInt32(loadQty[i]);
                    //            stock.AllocatBalQty = Convert.ToInt32(loadQty[i]);
                    //            stock.BalQty = Convert.ToInt32(loadQty[i]);
                    //            stock.ActWeight = Convert.ToDouble(loadWeight[i]);
                    //            stock.AllocatBalWght = Convert.ToDouble(loadWeight[i]);
                    //            stock.BalWeight = Convert.ToDouble(loadWeight[i]);
                    //            stock.ChrgWeight = lRStock.ChrgWeight;
                    //            stock.ChrgType = lRStock.ChrgType;
                    //            stock.Description = lRStock.Description;
                    //            stock.Unit = lRStock.Unit;
                    //            stock.FromBranch = lRStock.FromBranch;
                    //            stock.ToBranch = lRStock.ToBranch;
                    //            stock.Consigner = lRStock.Consigner;
                    //            stock.Consignee = lRStock.Consignee;
                    //            stock.LrType = lRStock.LrType;
                    //            stock.Coln = lRStock.Coln;
                    //            stock.Delivery = lRStock.Delivery;
                    //            stock.Remark = mModel.Remark;
                    //            stock.StockAt = "LDS";
                    //            stock.StockStatus = "L";
                    //            stock.LCNO = Convert.ToInt32(localDeliverySheet.LDSNo);
                    //            stock.ENTEREDBY = muserid;
                    //            stock.AUTHORISE = mauthorise;
                    //            stock.AUTHIDS = muserid;
                    //            stock.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    //            stock.TableKey = "LDSREF_" + Convert.ToInt32(localDeliverySheet.LDSNo).ToString("D6") + "_" + i;
                    //            stock.ParentKey = lRStock.TableKey;
                    //        }


                    //        lRStock.AllocatBalQty -= Convert.ToInt32(loadQty[i]);
                    //        lRStock.BalQty -= Convert.ToInt32(loadQty[i]);
                    //        lRStock.AllocatBalWght -= Convert.ToInt32(loadWeight[i]);
                    //        lRStock.BalWeight -= Convert.ToInt32(loadWeight[i]);

                    //        ctxTFAT.Entry(lRStock).State = EntityState.Modified;
                    //        ctxTFAT.LorryReceiptExpenses.Add(lorryReceipt);
                    //        ctxTFAT.LocalDeliveryRel.Add(LocalDeliveryRel);
                    //        ctxTFAT.LRStock.Add(stock);
                    //    }
                    //    else
                    //    {
                    //        var lCDetail = ctxTFAT.LRStock.Where(x => x.TableKey.ToString() == Recordkey).FirstOrDefault();

                    //        LorryReceiptExpenses lorryReceipt = new LorryReceiptExpenses
                    //        {
                    //            LDSNo = mModel.LDSNo,
                    //            LRno = Convert.ToInt32(lrno[i]),
                    //            Qty = Convert.ToInt32(loadQty[i]),
                    //            Amount = Convert.ToDouble(amount[i]),
                    //            ExpModule = "LDS",
                    //            ExpType = "Door",
                    //            Weight = loadWeight[i],
                    //            PickType = PickType[i],
                    //            ENTEREDBY = muserid,
                    //            AUTHORISE = mauthorise,
                    //            AUTHIDS = muserid,
                    //            LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString()),
                    //            TableKey = "LRExp_" + Convert.ToInt32(localDeliverySheet.LDSNo).ToString("D6") + "_" + i,
                    //            ParentKey = lCDetail.TableKey
                    //        };
                    //        LocalDeliveryRel LocalDeliveryRel = new LocalDeliveryRel();
                    //        {

                    //            LocalDeliveryRel.LDSNO = mModel.LDSNo;
                    //            LocalDeliveryRel.LRno = Convert.ToInt32(lrno[i]);
                    //            LocalDeliveryRel.Qty = Convert.ToInt32(loadQty[i]);
                    //            LocalDeliveryRel.Amount = Convert.ToDouble(amount[i]);
                    //            LocalDeliveryRel.ExpModule = "LDS";
                    //            LocalDeliveryRel.ExpType = "Door";
                    //            LocalDeliveryRel.Weight = Convert.ToDouble(loadWeight[i]);
                    //            LocalDeliveryRel.PickType = PickType[i];
                    //            LocalDeliveryRel.UnloadQty = 0;
                    //            LocalDeliveryRel.UnloadWeight = 0;
                    //            LocalDeliveryRel.ENTEREDBY = muserid;
                    //            LocalDeliveryRel.AUTHORISE = mauthorise;
                    //            LocalDeliveryRel.AUTHIDS = muserid;
                    //            LocalDeliveryRel.TableKey = "LDRel_" + Convert.ToInt32(localDeliverySheet.LDSNo).ToString("D6") + "_" + i;
                    //            LocalDeliveryRel.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    //            LocalDeliveryRel.ParentKey = lCDetail.TableKey;
                    //        }
                    //        LRStock stock = new LRStock();
                    //        {
                    //            stock.Branch = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
                    //            stock.LoginBranch = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
                    //            stock.LrNo = Convert.ToInt32(lrno[i]);
                    //            stock.Date = ConvertDDMMYYTOYYMMDD(mModel.Date);
                    //            stock.Time = mModel.Time;
                    //            stock.TotalQty = Convert.ToInt32(loadQty[i]);
                    //            stock.AllocatBalQty = Convert.ToInt32(loadQty[i]);
                    //            stock.BalQty = Convert.ToInt32(loadQty[i]);
                    //            stock.ActWeight = Convert.ToDouble(loadWeight[i]);
                    //            stock.AllocatBalWght = Convert.ToDouble(loadWeight[i]);
                    //            stock.BalWeight = Convert.ToDouble(loadWeight[i]);
                    //            stock.ChrgWeight = lCDetail.ChrgWeight;
                    //            stock.ChrgType = lCDetail.ChrgType;
                    //            stock.Description = lCDetail.Description;
                    //            stock.Unit = lCDetail.Unit;
                    //            stock.FromBranch = lCDetail.FromBranch;
                    //            stock.ToBranch = lCDetail.ToBranch;
                    //            stock.Consigner = lCDetail.Consigner;
                    //            stock.Consignee = lCDetail.Consignee;
                    //            stock.LrType = lCDetail.LrType;
                    //            stock.Coln = lCDetail.Coln;
                    //            stock.Delivery = lCDetail.Delivery;
                    //            stock.Remark = mModel.Remark;
                    //            stock.StockAt = "LDS";
                    //            stock.StockStatus = "L";
                    //            stock.LCNO = Convert.ToInt32(localDeliverySheet.LDSNo);
                    //            stock.ENTEREDBY = muserid;
                    //            stock.AUTHORISE = mauthorise;
                    //            stock.AUTHIDS = muserid;
                    //            stock.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    //            stock.TableKey = "LDSREF_" + Convert.ToInt32(localDeliverySheet.LDSNo).ToString("D6") + "_" + i;
                    //            stock.ParentKey = lCDetail.TableKey;
                    //        }
                    //        ctxTFAT.LorryReceiptExpenses.Add(lorryReceipt);
                    //        ctxTFAT.LocalDeliveryRel.Add(LocalDeliveryRel);
                    //        ctxTFAT.LRStock.Add(stock);
                    //        lCDetail.AllocatBalQty -= Convert.ToInt32(loadQty[i]);
                    //        lCDetail.BalQty -= Convert.ToInt32(loadQty[i]);
                    //        lCDetail.AllocatBalWght -= Convert.ToInt32(loadWeight[i]);
                    //        lCDetail.BalWeight -= Convert.ToInt32(loadWeight[i]);

                    //        ctxTFAT.Entry(lCDetail).State = EntityState.Modified;
                    //    }

                    //}

                    if (mAdd == true)
                    {
                        ctxTFAT.LocalDeliverySheet.Add(localDeliverySheet);
                    }
                    else
                    {
                        ctxTFAT.Entry(localDeliverySheet).State = EntityState.Modified;
                    }
                    ctxTFAT.SaveChanges();

                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "A");
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

        public string DeleteStateMaster(LocalDeliverySheetVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return "Code not Entered..";
            }

            var id = Convert.ToInt64(mModel.Document);

            var FmRelation = ctxTFAT.FMROUTETable.Where(x => x.FmNo.ToString() == mModel.Document && x.DispatchDate != null).ToList();
            if (FmRelation.Count() > 0)
            {
                return "Cannot Delete ........!";
            }

            var fMMaster = ctxTFAT.FMMaster.Where(x => (x.FmNo.ToString() == id.ToString())).FirstOrDefault();
            var FMROUTETables = ctxTFAT.FMROUTETable.Where(x => x.FmNo.ToString() == id.ToString()).ToList();
            var lCMasters = ctxTFAT.LCMaster.Where(x => (x.DispachFM.ToString() == fMMaster.FmNo.ToString())).ToList();
            //var fM_Attachments = ctxTFAT.Attachment.Where(x => x.ParentCode == fMMaster.AttachmentCode.ToString()).ToList();

            ctxTFAT.FMMaster.Remove(fMMaster);
            foreach (var item in FMROUTETables)
            {
                ctxTFAT.FMROUTETable.Remove(item);
            }
            foreach (var item in lCMasters)
            {
                item.DispachFM = 0;
                ctxTFAT.Entry(item).State = EntityState.Modified;
            }
            //foreach (var item in fM_Attachments)
            //{
            //    ctxTFAT.Attachment.Remove(item);
            //}
            ctxTFAT.SaveChanges();
            return "Success";
        }

        #region Attachment (Download,View,Delete,Save)

        [HttpPost]
        public ActionResult AttachDocument(FMAttachment model, string DocumentStr, string FileNameStr)
        {
            List<FMAttachment> AttachList = new List<FMAttachment>();
            List<FMAttachment> SessionAttachList = TempData.Peek("FMAttachmentList") as List<FMAttachment>;
            if (SessionAttachList != null)
            {
                AttachList = SessionAttachList;
            }
            MemoryStream memory = new MemoryStream();
            if (AttachList.Where(x => x.FileName == model.UploadFile.FileName).FirstOrDefault() == null)
            {
                model.UploadFile.InputStream.CopyTo(memory);
                byte[] bytes = memory.ToArray();

                FMAttachment PersonalDocument = new FMAttachment
                {
                    AttachFMNo = model.UploadFile.FileName.ToString(),
                    DocumentString = Convert.ToBase64String(bytes),
                    ContentType = model.UploadFile.ContentType,
                    FileName = model.UploadFile.FileName,
                    Image = bytes
                };
                AttachList.Add(PersonalDocument);
            }
            LocalDeliverySheetVM localDeliverySheetVM = new LocalDeliverySheetVM();
            localDeliverySheetVM.attachments = AttachList;

            TempData["FMAttachmentList"] = AttachList;
            var html = ViewHelper.RenderPartialView(this, "_Attachment", localDeliverySheetVM);
            var jsonResult = Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }
        [HttpPost]
        public ActionResult AttachDocumentList()
        {
            List<FMAttachment> AttachList = new List<FMAttachment>();
            List<FMAttachment> SessionAttachList = TempData.Peek("FMAttachmentList") as List<FMAttachment>;
            if (SessionAttachList != null)
            {
                AttachList = SessionAttachList;
            }

            TempData["FMAttachmentList"] = AttachList;
            var html = ViewHelper.RenderPartialView(this, "_AttachmentList", new LocalDeliverySheetVM { attachments = AttachList });
            var jsonResult = Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }
        public FileResult Download(string tempId)
        {
            List<FMAttachment> attachlist = new List<FMAttachment>();
            if (TempData["FMAttachmentList"] != null)
            {
                attachlist = TempData.Peek("FMAttachmentList") as List<FMAttachment>;
            }

            var dwnfile = attachlist.Where(x => x.AttachFMNo == tempId).FirstOrDefault();

            string filename = dwnfile.FileName;
            byte[] fileBytes = Convert.FromBase64String(dwnfile.DocumentString);

            return File(fileBytes, dwnfile.ContentType, filename);
        }

        [HttpPost]
        public ActionResult Delete(string tempId)
        {
            string message = "False";

            List<FMAttachment> attachlist = new List<FMAttachment>();
            if (TempData["FMAttachmentList"] != null)
            {
                attachlist = TempData.Peek("FMAttachmentList") as List<FMAttachment>;
            }

            var dwnfile = attachlist.Where(x => x.AttachFMNo == tempId).FirstOrDefault();

            attachlist.Remove(dwnfile);
            message = "True";
            TempData["FMAttachmentList"] = attachlist;
            var html = ViewHelper.RenderPartialView(this, "FMAttachmentView", new FMVM { attachments = attachlist });
            var jsonResult = Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        [HttpPost]
        public ActionResult ViewImage(FMAttachment mModel)
        {
            List<FMAttachment> attachmentDocumentVMs = new List<FMAttachment>();
            attachmentDocumentVMs = TempData.Peek("FMAttachmentList") as List<FMAttachment>;
            FMAttachment attachmentDocument = attachmentDocumentVMs.Where(x => x.AttachFMNo == mModel.AttachFMNo).FirstOrDefault();

            byte[] Image;
            byte[] byteArray = Encoding.UTF8.GetBytes(attachmentDocument.DocumentString);
            MemoryStream stream = new MemoryStream(byteArray);
            using (var binaryReader = new BinaryReader(stream))
            {
                Image = binaryReader.ReadBytes(attachmentDocument.DocumentString.Length);
            }
            byte[] fileBinary = Convert.FromBase64String(attachmentDocument.DocumentString);
            attachmentDocument.Image = fileBinary;
            var html = ViewHelper.RenderPartialView(this, "_ImageView", attachmentDocument);

            var jsonResult = Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        #endregion

        #region Update Lock,Attachment,Note

        public ActionResult SaveNote(FMVM mModel)
        {
            string Status = "Success";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {

                    //LockSystem lockSystem = ctxTFAT.LockSystem.Where(x => x.DocumentNo.ToString() == mModel.Document && x.Type=="FM").FirstOrDefault();
                    //lockSystem.Note = mModel.Note;
                    //ctxTFAT.Entry(lockSystem).State = EntityState.Modified;
                    //ctxTFAT.SaveChanges();
                    //transaction.Commit();
                    //transaction.Dispose();

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

            return Json(new { Status = Status, JsonRequestBehavior.AllowGet });

        }
        public ActionResult SaveAttachment(FMVM mModel)
        {
            string Status = "Success";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    {
                        //FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.FmNo.ToString() == mModel.FMNO).FirstOrDefault();
                        //if (fMMaster != null)
                        //{
                        //    var AttachmentList = ctxTFAT.Attachment.Where(x => (x.ParentCode.ToString() == fMMaster.AttachmentCode)).ToList();
                        //    foreach (var item in AttachmentList)
                        //    {
                        //        ctxTFAT.Attachment.Remove(item);
                        //    }
                        //}

                        List<Attachment> SessionAttachList = TempData.Peek("FMAttachmentList") as List<Attachment>;

                        if (SessionAttachList != null)
                        {
                            foreach (var item in SessionAttachList)
                            {
                                var LastCode = ctxTFAT.Attachment.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).Take(1).FirstOrDefault();

                                //Attachment LR_Attachment = new Attachment
                                //{
                                //    ParentCode = fMMaster.AttachmentCode,
                                //    Code = (Convert.ToInt32(LastCode) + 1).ToString(),
                                //    ContentType = item.ContentType,
                                //    DocumentString = item.DocumentString,
                                //    FileName = item.FileName,
                                //    AUTHIDS = muserid,
                                //    AUTHORISE = mauthorise,
                                //    ENTEREDBY = muserid,
                                //    LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString()).ToString()
                                //};

                                //ctxTFAT.Attachment.Add(LR_Attachment);
                                ctxTFAT.SaveChanges();
                                transaction.Commit();
                                transaction.Dispose();
                            }
                        }
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

            return Json(new { Status = Status, JsonRequestBehavior.AllowGet });

        }
        public ActionResult SaveLock(FMVM mModel)
        {
            string Status = "Success";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {


                    //LockSystem lockSystem = ctxTFAT.LockSystem.Where(x => x.DocumentNo == mModel.Document && x.Type=="FM").FirstOrDefault();

                    //lockSystem.Dispatch = mModel.Dispach;
                    //lockSystem.Delivery = mModel.Delivery;
                    //lockSystem.Billing = mModel.Billing;
                    //lockSystem.LockRemark = mModel.LockRemark;

                    //ctxTFAT.Entry(lockSystem).State = EntityState.Modified;
                    //ctxTFAT.SaveChanges();
                    //transaction.Commit();
                    //transaction.Dispose();
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

            return Json(new { Status = Status, JsonRequestBehavior.AllowGet });
        }

        #endregion

        public ActionResult Godown_LR_Stock_List(string To)
        {
            string[] ToNameArray = new string[0];
            if (!String.IsNullOrEmpty(To))
            {
                var ToCodeArray = To.Split(',');
                ToNameArray = new string[ToCodeArray.Length];
                for (int i = 0; i < ToCodeArray.Length; i++)
                {
                    string Too = ToCodeArray[i];
                    ToNameArray[i] = ctxTFAT.TfatBranch.Where(x => x.Code == Too).Select(x => x.Name).FirstOrDefault();
                }
            }

            bool AmountFlag = true, LoadQty_WeightFlag = true;
            var BranchChild = GetChildGrp(mbranchcode);
            LCVM lCVM = new LCVM();
            List<LcDetailsVM> StockOfLR = TempData.Peek("ExistingLRLIST") as List<LcDetailsVM>;
            if (StockOfLR == null)
            {
                StockOfLR = new List<LcDetailsVM>();
            }
            List<LcDetailsVM> AllStockLRLIst = new List<LcDetailsVM>();

            List<LRStock> GetLrList = new List<LRStock>();

            GetLrList = ctxTFAT.LRStock.Where(x => BranchChild.Contains(x.Branch) && x.AllocatBalQty > 0 && (x.StockStatus == "G")).ToList();
            foreach (var lRStock in GetLrList)
            {
                bool Add = true;
                if (StockOfLR != null)
                {
                    if (StockOfLR.Where(x => x.recordekey == lRStock.TableKey).FirstOrDefault() != null)
                    {
                        Add = false;
                    }
                }
                else
                {
                    StockOfLR = new List<LcDetailsVM>();
                }
                if (Add)
                {
                    LcDetailsVM item = new LcDetailsVM();
                    item.recordekey = lRStock.TableKey;
                    item.ChrgeType = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == lRStock.ChrgType).Select(x => x.ChargeType).FirstOrDefault();
                    item.Description = ctxTFAT.DescriptionMaster.Where(x => x.Code == lRStock.Description).Select(x => x.Description).FirstOrDefault();
                    item.Unit = ctxTFAT.UnitMaster.Where(x => x.Code == lRStock.Unit).Select(x => x.Name).FirstOrDefault();
                    item.From = ctxTFAT.TfatBranch.Where(x => x.Code == lRStock.FromBranch).Select(x => x.Name).FirstOrDefault();
                    item.To = ctxTFAT.TfatBranch.Where(x => x.Code == lRStock.ToBranch).Select(x => x.Name).FirstOrDefault();
                    item.Consignor = ctxTFAT.Consigner.Where(x => x.Code == lRStock.Consigner).Select(x => x.Name).FirstOrDefault();
                    item.Consignee = ctxTFAT.Consigner.Where(x => x.Code == lRStock.Consignee).Select(x => x.Name).FirstOrDefault();
                    item.LrType = ctxTFAT.LRTypeMaster.Where(x => x.Code == lRStock.LrType).Select(x => x.LRType).FirstOrDefault();
                    item.LRDelivery = lRStock.Delivery == "Godown" ? "Godown" : ctxTFAT.TfatBranch.Where(x => x.Code == lRStock.Delivery).Select(x => x.Name).FirstOrDefault();
                    item.LRColln = lRStock.Coln == "G" ? "Godown" : lRStock.Coln == "C" ? "Crossing" : ctxTFAT.TfatBranch.Where(x => x.Code == lRStock.Coln).Select(x => x.Name).FirstOrDefault();
                    //item.PickFrom = lRStock.Coln == "G" ? "Godown" : lRStock.Coln == "C" ? "Crossing" : ctxTFAT.LRMaster.Where(x => x.LrNo == lRStock.LrNo).Select(x => x.VehicleNo).FirstOrDefault();
                    //item.LrQty = lRStock.TotalQty;
                    item.Qty = lRStock.AllocatBalQty;
                    item.Lrno = lRStock.LrNo.Value;
                    item.ChrWeight = lRStock.ChrgWeight;
                    item.ActWeight = lRStock.AllocatBalWght;
                    item.LRActWeight = Convert.ToInt32(lRStock.AllocatBalWght);
                    item.PickType = "GDStock";
                    item.EditLDSNo = true;
                    item.ShowAmountColumn = AmountFlag;
                    item.ShowLoadQtyANdWeight = LoadQty_WeightFlag;

                    AllStockLRLIst.Add(item);
                }
            }
            List<LcDetailsVM> DeleteLRList = TempData.Peek("DeleteLRLIST") as List<LcDetailsVM>;
            if (DeleteLRList != null)
            {
                AllStockLRLIst.AddRange(DeleteLRList.Where(x => x.PickType == "GDStock").ToList());
            }
            TempData["AllStockLRLIst"] = AllStockLRLIst;
            TempData["ExistingLRLIST"] = StockOfLR;

            List<LcDetailsVM> lcDetailsVMs = new List<LcDetailsVM>();
            if (ToNameArray.Length > 0)
            {
                lcDetailsVMs.AddRange(AllStockLRLIst.Where(x => ToNameArray.Contains(x.To)));
                lcDetailsVMs.AddRange(AllStockLRLIst.Where(x => !(ToNameArray.Contains(x.To))));
            }
            else
            {
                lcDetailsVMs.AddRange(AllStockLRLIst.OrderBy(x => x.Lrno));
            }
            lCVM.lCDetails = lcDetailsVMs;
            var html = ViewHelper.RenderPartialView(this, "G_D_T_A_LrList", lCVM);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Door_LR_Stock_List(string To)
        {
            string[] ToNameArray = new string[0];
            if (!String.IsNullOrEmpty(To))
            {
                var ToCodeArray = To.Split(',');
                ToNameArray = new string[ToCodeArray.Length];
                for (int i = 0; i < ToCodeArray.Length; i++)
                {
                    string Too = ToCodeArray[i];
                    ToNameArray[i] = ctxTFAT.TfatBranch.Where(x => x.Code == Too).Select(x => x.Name).FirstOrDefault();
                }
            }

            bool AmountFlag = true, LoadQty_WeightFlag = true;
            var BranchChild = GetChildGrp(mbranchcode);
            LCVM lCVM = new LCVM();
            List<LcDetailsVM> StockOfDoorLR = TempData.Peek("ExistingLRLIST") as List<LcDetailsVM>;
            List<LcDetailsVM> AllDoorStockLRLIst = new List<LcDetailsVM>();
            List<LRStock> GetLrList = new List<LRStock>();
            GetLrList = ctxTFAT.LRStock.Where(x => BranchChild.Contains(x.Branch) && x.AllocatBalQty > 0 && (x.StockStatus == "P")).ToList();
            foreach (var lRStock in GetLrList)
            {
                bool Add = true;
                if (StockOfDoorLR != null)
                {
                    if (StockOfDoorLR.Where(x => x.recordekey == lRStock.TableKey).FirstOrDefault() != null)
                    {
                        Add = false;
                    }
                }
                else
                {
                    StockOfDoorLR = new List<LcDetailsVM>();
                }

                if (Add)
                {
                    LcDetailsVM item = new LcDetailsVM();
                    item.recordekey = lRStock.TableKey;
                    item.ChrgeType = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == lRStock.ChrgType).Select(x => x.ChargeType).FirstOrDefault();
                    item.Description = ctxTFAT.DescriptionMaster.Where(x => x.Code == lRStock.Description).Select(x => x.Description).FirstOrDefault();
                    item.Unit = ctxTFAT.UnitMaster.Where(x => x.Code == lRStock.Unit).Select(x => x.Name).FirstOrDefault();
                    item.From = ctxTFAT.TfatBranch.Where(x => x.Code == lRStock.FromBranch).Select(x => x.Name).FirstOrDefault();
                    item.To = ctxTFAT.TfatBranch.Where(x => x.Code == lRStock.ToBranch).Select(x => x.Name).FirstOrDefault();
                    item.Consignor = ctxTFAT.Consigner.Where(x => x.Code == lRStock.Consigner).Select(x => x.Name).FirstOrDefault();
                    item.Consignee = ctxTFAT.Consigner.Where(x => x.Code == lRStock.Consignee).Select(x => x.Name).FirstOrDefault();
                    item.LrType = ctxTFAT.LRTypeMaster.Where(x => x.Code == lRStock.LrType).Select(x => x.LRType).FirstOrDefault();
                    item.LRDelivery = lRStock.Delivery == "Godown" ? "Godown" : ctxTFAT.TfatBranch.Where(x => x.Code == lRStock.Delivery).Select(x => x.Name).FirstOrDefault();
                    item.LRColln = lRStock.Coln == "G" ? "Godown" : lRStock.Coln == "C" ? "Crossing" : ctxTFAT.TfatBranch.Where(x => x.Code == lRStock.Coln).Select(x => x.Name).FirstOrDefault();
                    //item.PickFrom = lRStock.Coln == "G" ? "Godown" : lRStock.Coln == "C" ? "Crossing" : ctxTFAT.LRMaster.Where(x => x.LrNo == lRStock.LrNo).Select(x => x.VehicleNo).FirstOrDefault();
                    //item.LrQty = lRStock.TotalQty;
                    item.Qty = lRStock.AllocatBalQty;
                    item.ChrWeight = lRStock.ChrgWeight;
                    item.ActWeight = lRStock.ActWeight;
                    item.LRActWeight = Convert.ToInt32(lRStock.AllocatBalWght);
                    item.Lrno = lRStock.LrNo.Value;
                    item.PickType = "PickStock";
                    item.EditLDSNo = true;
                    item.ShowAmountColumn = AmountFlag;
                    item.ShowLoadQtyANdWeight = LoadQty_WeightFlag;
                    AllDoorStockLRLIst.Add(item);
                }
            }
            List<LcDetailsVM> DeleteLRList = TempData.Peek("DeleteLRLIST") as List<LcDetailsVM>;
            if (DeleteLRList != null)
            {
                AllDoorStockLRLIst.AddRange(DeleteLRList.Where(x => x.PickType == "PickStock").ToList());
            }
            TempData["AllDoorStockLRLIst"] = AllDoorStockLRLIst;
            TempData["ExistingLRLIST"] = StockOfDoorLR;
            List<LcDetailsVM> lcDetailsVMs = new List<LcDetailsVM>();
            if (ToNameArray.Length > 0)
            {
                lcDetailsVMs.AddRange(AllDoorStockLRLIst.Where(x => ToNameArray.Contains(x.To)));
                lcDetailsVMs.AddRange(AllDoorStockLRLIst.Where(x => !(ToNameArray.Contains(x.To))));
            }
            else
            {
                lcDetailsVMs.AddRange(AllDoorStockLRLIst.OrderBy(x => x.Lrno));
            }
            lCVM.lCDetails = lcDetailsVMs;
            var html = ViewHelper.RenderPartialView(this, "G_D_T_A_LrList", lCVM);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Transit_LR_List(string To)
        {
            string[] ToNameArray = new string[0];
            if (!String.IsNullOrEmpty(To))
            {
                var ToCodeArray = To.Split(',');
                ToNameArray = new string[ToCodeArray.Length];
                for (int i = 0; i < ToCodeArray.Length; i++)
                {
                    string Too = ToCodeArray[i];
                    ToNameArray[i] = ctxTFAT.TfatBranch.Where(x => x.Code == Too).Select(x => x.Name).FirstOrDefault();
                }
            }
            bool AmountFlag = true, LoadQty_WeightFlag = true;
            var BranchChild = GetChildGrp(mbranchcode);
            LCVM lCVM = new LCVM();
            List<LcDetailsVM> TransitLRList = TempData.Peek("ExistingLRLIST") as List<LcDetailsVM>;
            if (TransitLRList == null)
            {
                TransitLRList = new List<LcDetailsVM>();
            }
            List<LcDetailsVM> AllTransitLRLIst = new List<LcDetailsVM>();
            //Current Branch Me Vehicle Unloading Ke Liye HE Uska List
            var Fmlist = ctxTFAT.FMMaster.Where(x => BranchChild.Contains(x.CurrBranch) && (x.FmStatus == "W")).ToList();
            if (Fmlist.Count() > 0)
            {
                //Create List Of All Fmno To Easy To Filter In LCdetails Tables
                var FmNoList = Fmlist.Select(x => x.FmNo).ToList();
                //Search In LCmaster Table Wo Jo Mumbai BRanch KE Liye Material Available He
                var Lcmasters = ctxTFAT.LCMaster.Where(x => FmNoList.Contains(x.DispachFM) && BranchChild.Contains(x.ToBranch)).ToList();
                //Create List Of All LcmasterNo To Easy To Filter In LCdetails Tables
                var LCno = Lcmasters.Select(x => x.LCno).ToList();
                //Finally Lr Ka List Milega Lcmaster Se
                var LrListFromVehicle = ctxTFAT.LRStock.Where(x => LCno.Contains(x.LCNO.Value) && x.BalQty > 0 && x.StockStatus == "T").ToList();
                foreach (var detail in LrListFromVehicle)
                {
                    var Add = true;
                    if (TransitLRList != null)
                    {
                        var CheckExistOrNot = TransitLRList.Where(x => x.recordekey == detail.TableKey).FirstOrDefault();
                        if (CheckExistOrNot != null)
                        {
                            Add = false;
                        }
                    }
                    if (Add)
                    {
                        LcDetailsVM item = new LcDetailsVM();
                        item.recordekey = detail.TableKey;
                        item.ChrgeType = ctxTFAT.ChargeTypeMaster.Where(x => x.ChargeType == detail.ChrgType).Select(x => x.ChargeType).FirstOrDefault();
                        item.Description = ctxTFAT.DescriptionMaster.Where(x => x.Code == detail.Description).Select(x => x.Description).FirstOrDefault();
                        item.Unit = ctxTFAT.UnitMaster.Where(x => x.Code == detail.Unit).Select(x => x.Name).FirstOrDefault();
                        item.From = ctxTFAT.TfatBranch.Where(x => x.Code == detail.FromBranch).Select(x => x.Name).FirstOrDefault();
                        item.To = ctxTFAT.TfatBranch.Where(x => x.Code == detail.ToBranch).Select(x => x.Name).FirstOrDefault();
                        item.Consignor = ctxTFAT.Consigner.Where(x => x.Code == detail.Consigner).Select(x => x.Name).FirstOrDefault();
                        item.Consignee = ctxTFAT.Consigner.Where(x => x.Code == detail.Consignee).Select(x => x.Name).FirstOrDefault();
                        item.LrType = ctxTFAT.LRTypeMaster.Where(x => x.Code == detail.LrType).Select(x => x.LRType).FirstOrDefault();
                        item.LRDelivery = detail.Delivery == "Godown" ? "Godown" : ctxTFAT.TfatBranch.Where(x => x.Code == detail.Delivery).Select(x => x.Name).FirstOrDefault();
                        item.LRColln = detail.Coln == "G" ? "Godown" : detail.Coln == "C" ? "Crossing" : ctxTFAT.TfatBranch.Where(x => x.Code == detail.Coln).Select(x => x.Name).FirstOrDefault();
                        //item.LrQty = detail.TotalQty;
                        item.Qty = detail.AllocatBalQty;
                        item.ChrWeight = detail.ChrgWeight;
                        item.ActWeight = detail.ActWeight;
                        item.LRActWeight = Convert.ToInt32(detail.AllocatBalWght);
                        item.Lrno = detail.LrNo.Value;
                        item.PickType = "Transit";
                        item.EditLDSNo = true;
                        item.ShowAmountColumn = AmountFlag;
                        item.ShowLoadQtyANdWeight = LoadQty_WeightFlag;
                        AllTransitLRLIst.Add(item);
                    }
                }
            }
            List<LcDetailsVM> DeleteLRList = TempData.Peek("DeleteLRLIST") as List<LcDetailsVM>;
            if (DeleteLRList != null)
            {
                AllTransitLRLIst.AddRange(DeleteLRList.Where(x => x.PickType == "Transit").ToList());
            }
            TempData["AllTransitLRLIst"] = AllTransitLRLIst;
            TempData["ExistingLRLIST"] = TransitLRList;

            List<LcDetailsVM> lcDetailsVMs = new List<LcDetailsVM>();
            if (ToNameArray.Length > 0)
            {
                lcDetailsVMs.AddRange(AllTransitLRLIst.Where(x => ToNameArray.Contains(x.To)));
                lcDetailsVMs.AddRange(AllTransitLRLIst.Where(x => !(ToNameArray.Contains(x.To))));
            }
            else
            {
                lcDetailsVMs.AddRange(AllTransitLRLIst.OrderBy(x => x.Lrno));
            }
            lCVM.lCDetails = lcDetailsVMs;
            var html = ViewHelper.RenderPartialView(this, "G_D_T_A_LrList", lCVM);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult All_LR_List(string To)
        {
            string[] ToNameArray = new string[0];
            if (!String.IsNullOrEmpty(To))
            {
                var ToCodeArray = To.Split(',');
                ToNameArray = new string[ToCodeArray.Length];
                for (int i = 0; i < ToCodeArray.Length; i++)
                {
                    string Too = ToCodeArray[i];
                    ToNameArray[i] = ctxTFAT.TfatBranch.Where(x => x.Code == Too).Select(x => x.Name).FirstOrDefault();
                }
            }
            Godown_LR_Stock_List(To);
            Door_LR_Stock_List(To);
            Transit_LR_List(To);

            LCVM lCVM = new LCVM();
            List<LcDetailsVM> lcDetailsVMs = new List<LcDetailsVM>();
            List<LcDetailsVM> AllStockLRLIst = TempData.Peek("AllStockLRLIst") as List<LcDetailsVM>;
            List<LcDetailsVM> AllDoorStockLRLIst = TempData.Peek("AllDoorStockLRLIst") as List<LcDetailsVM>;
            List<LcDetailsVM> AllTransitLRLIst = TempData.Peek("AllTransitLRLIst") as List<LcDetailsVM>;

            if (AllStockLRLIst == null)
            {
                AllStockLRLIst = new List<LcDetailsVM>();
            }

            if (AllDoorStockLRLIst == null)
            {
                AllDoorStockLRLIst = new List<LcDetailsVM>();
            }
            if (AllTransitLRLIst == null)
            {
                AllTransitLRLIst = new List<LcDetailsVM>();
            }

            if (ToNameArray.Length > 0)
            {
                lcDetailsVMs.AddRange(AllStockLRLIst.Where(x => ToNameArray.Contains(x.To)));
                lcDetailsVMs.AddRange(AllStockLRLIst.Where(x => !(ToNameArray.Contains(x.To))));
                lcDetailsVMs.AddRange(AllDoorStockLRLIst.Where(x => ToNameArray.Contains(x.To)));
                lcDetailsVMs.AddRange(AllDoorStockLRLIst.Where(x => !(ToNameArray.Contains(x.To))));
                lcDetailsVMs.AddRange(AllTransitLRLIst.Where(x => ToNameArray.Contains(x.To)));
                lcDetailsVMs.AddRange(AllTransitLRLIst.Where(x => !(ToNameArray.Contains(x.To))));
            }
            else
            {
                lcDetailsVMs.AddRange(AllStockLRLIst.OrderBy(x => x.Lrno));
                lcDetailsVMs.AddRange(AllDoorStockLRLIst.OrderBy(x => x.Lrno));
                lcDetailsVMs.AddRange(AllTransitLRLIst.OrderBy(x => x.Lrno));
            }
            lCVM.lCDetails = lcDetailsVMs;
            var html = ViewHelper.RenderPartialView(this, "G_D_T_A_LrList", lCVM);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult GridView(string AllLrList, string PickUp, string Delivery, string LoadGQty, string LoadDQty, string LoadWeight, string PickType, string Amount)
        {
            bool AmountFlag = true;
            bool LoadQty_WeightFlag = true;


            List<LcDetailsVM> AllStockLRLIst = TempData.Peek("AllStockLRLIst") as List<LcDetailsVM>;
            List<LcDetailsVM> AllDoorStockLRLIst = TempData.Peek("AllDoorStockLRLIst") as List<LcDetailsVM>;
            List<LcDetailsVM> AllTransitLRLIst = TempData.Peek("AllTransitLRLIst") as List<LcDetailsVM>;
            List<LcDetailsVM> InserList = TempData.Peek("ExistingLRLIST") as List<LcDetailsVM>;
            if (InserList == null)
            {
                InserList = new List<LcDetailsVM>();
            }
            var ids = AllLrList.Split(',');
            //var pickup = PickUp.Split(',');
            //var delivery = Delivery.Split(',');
            var loadqty = LoadGQty.Split(',');
            var loadqty1 = LoadDQty.Split(',');
            var PickTypee = PickType.Split(',');
            var loadweight = LoadWeight.Split(',');
            List<string> amount = new List<string>();
            var ArrayLength = 0;
            if (AmountFlag)
            {
                amount = Amount.Split(',').ToList();
            }

            for (int i = 0; i < ids.Length; i++)
            {
                if (InserList.Where(x => x.Lrno.ToString() == ids[i]).FirstOrDefault() == null)
                {
                    if (PickTypee[i] == "GDStock")
                    {
                        var GodownStk = AllStockLRLIst.Where(x => x.Lrno.ToString() == ids[i]).FirstOrDefault();
                        GodownStk.LoadGQty = Convert.ToInt32(loadqty[i]);
                        if (loadqty1[i] != "undefined")
                        {
                            GodownStk.LoadDQty = Convert.ToInt32(loadqty1[i]);
                        }
                        GodownStk.LRActWeight = Convert.ToInt32(loadweight[i]);
                        if (AmountFlag)
                        {
                            GodownStk.Amount = Convert.ToInt32(amount[i]);
                        }
                        InserList.Add(GodownStk);
                        AllStockLRLIst.Remove(GodownStk);
                        TempData["ExistingLRLIST"] = InserList;
                        TempData["AllStockLRLIst"] = AllStockLRLIst;
                    }
                    else if (PickTypee[i] == "PickStock")
                    {
                        var GodownStk = AllDoorStockLRLIst.Where(x => x.Lrno.ToString() == ids[i]).FirstOrDefault();
                        GodownStk.LoadGQty = Convert.ToInt32(loadqty[i]);
                        if (loadqty1[i] != "undefined")
                        {
                            GodownStk.LoadDQty = Convert.ToInt32(loadqty1[i]);
                        }
                        GodownStk.LRActWeight = Convert.ToInt32(loadweight[i]);
                        if (AmountFlag)
                        {
                            GodownStk.Amount = Convert.ToInt32(amount[i]);
                        }
                        InserList.Add(GodownStk);
                        AllDoorStockLRLIst.Remove(GodownStk);
                        TempData["ExistingLRLIST"] = InserList;
                        TempData["AllDoorStockLRLIst"] = AllDoorStockLRLIst;
                    }
                    else if (PickTypee[i] == "Transit")
                    {
                        var GodownStk = AllTransitLRLIst.Where(x => x.Lrno.ToString() == ids[i]).FirstOrDefault();
                        GodownStk.LoadGQty = Convert.ToInt32(loadqty[i]);
                        if (loadqty1[i] != "undefined")
                        {
                            GodownStk.LoadDQty = Convert.ToInt32(loadqty1[i]);
                        }
                        GodownStk.LRActWeight = Convert.ToInt32(loadweight[i]);
                        if (AmountFlag)
                        {
                            GodownStk.Amount = Convert.ToInt32(amount[i]);
                        }
                        InserList.Add(GodownStk);
                        AllTransitLRLIst.Remove(GodownStk);
                        TempData["ExistingLRLIST"] = InserList;
                        TempData["AllTransitLRLIst"] = AllTransitLRLIst;
                    }
                }
            }
            var html = ViewHelper.RenderPartialView(this, "GridView", InserList);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteLRFromTempData(string LRno, string PickType)
        {
            string Msg = "Sucess";
            List<LcDetailsVM> DeleteLRList = TempData.Peek("DeleteLRLIST") as List<LcDetailsVM>;
            if (DeleteLRList == null)
            {
                DeleteLRList = new List<LcDetailsVM>();
            }
            List<LcDetailsVM> ExistingLRLIST = TempData.Peek("ExistingLRLIST") as List<LcDetailsVM>;
            List<LcDetailsVM> AllStockLRLIst = TempData.Peek("AllStockLRLIst") as List<LcDetailsVM>;
            List<LcDetailsVM> AllDoorStockLRLIst = TempData.Peek("AllDoorStockLRLIst") as List<LcDetailsVM>;
            List<LcDetailsVM> AllTransitLRLIst = TempData.Peek("AllTransitLRLIst") as List<LcDetailsVM>;

            if (PickType == "GDStock")
            {
                if (AllStockLRLIst == null)
                {
                    AllStockLRLIst = new List<LcDetailsVM>();
                }
                var GetDeleteLR = ExistingLRLIST.Where(x => x.Lrno.ToString() == LRno).FirstOrDefault();
                if (GetDeleteLR != null)
                {
                    GetDeleteLR.LoadGQty = 0;
                    GetDeleteLR.LoadDQty = 0;
                    GetDeleteLR.LRActWeight = 0;
                    GetDeleteLR.Amount = 0;
                    AllStockLRLIst.Add(GetDeleteLR);
                    DeleteLRList.Add(GetDeleteLR);
                    ExistingLRLIST.Remove(GetDeleteLR);
                }

            }
            else if (PickType == "PickStock")
            {
                if (AllDoorStockLRLIst == null)
                {
                    AllDoorStockLRLIst = new List<LcDetailsVM>();
                }
                var GetDeleteLR = ExistingLRLIST.Where(x => x.Lrno.ToString() == LRno).FirstOrDefault();
                if (GetDeleteLR != null)
                {
                    GetDeleteLR.LoadGQty = 0;
                    GetDeleteLR.LoadDQty = 0;
                    GetDeleteLR.LRActWeight = 0;
                    GetDeleteLR.Amount = 0;
                    AllDoorStockLRLIst.Add(GetDeleteLR);
                    DeleteLRList.Add(GetDeleteLR);
                    ExistingLRLIST.Remove(GetDeleteLR);
                }
            }
            else
            {
                if (AllTransitLRLIst == null)
                {
                    AllTransitLRLIst = new List<LcDetailsVM>();
                }
                var GetDeleteLR = ExistingLRLIST.Where(x => x.Lrno.ToString() == LRno).FirstOrDefault();
                if (GetDeleteLR != null)
                {
                    GetDeleteLR.LoadGQty = 0;
                    GetDeleteLR.LoadDQty = 0;
                    GetDeleteLR.LRActWeight = 0;
                    GetDeleteLR.Amount = 0;
                    AllTransitLRLIst.Add(GetDeleteLR);
                    DeleteLRList.Add(GetDeleteLR);
                    ExistingLRLIST.Remove(GetDeleteLR);
                }
            }

            TempData["ExistingLRLIST"] = ExistingLRLIST;
            TempData["AllStockLRLIst"] = AllStockLRLIst;
            TempData["AllDoorStockLRLIst"] = AllDoorStockLRLIst;
            TempData["AllTransitLRLIst"] = AllTransitLRLIst;
            TempData["DeleteLRList"] = DeleteLRList;

            return Json(new { Msg = Msg }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult UpdateQtyWeight(string Reco, int GQty, int DQty, int Weight)
        {
            List<LcDetailsVM> lCDetails = TempData.Peek("ExistingLRLIST") as List<LcDetailsVM>;

            var GetLRDetails = lCDetails.Where(x => x.recordekey == Reco).FirstOrDefault();
            if (GetLRDetails != null)
            {
                GetLRDetails.LoadGQty = GQty;
                GetLRDetails.LoadDQty = DQty;
                GetLRDetails.LRActWeight = Weight;
                TempData["ExistingLRLIST"] = lCDetails;
            }

            return Json(new { Msg = "Sucess" }, JsonRequestBehavior.AllowGet);
        }



        #region Dump Code

        //public ActionResult Index1(LocalDeliverySheetVM mModel)
        //{

        //    TempData.Remove("Destination");
        //    TempData.Remove("FMAttachmentList");

        //    TempData.Remove("ExistingLRLIST");
        //    TempData.Remove("AllStockLRLIst");
        //    TempData.Remove("AllDoorStockLRLIst");
        //    TempData.Remove("AllTransitLRLIst");

        //    List<LcDetailsVM> lCDetails = new List<LcDetailsVM>();

        //    mModel.LDSsetup = ctxTFAT.LDSsetup.Where(x => (x.RECORDKEY == 1)).FirstOrDefault();
        //    if (mModel.LDSsetup == null)
        //    {
        //        mModel.LDSsetup = new LDSsetup();
        //    }
        //    string BranchCode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        //    GetAllMenu(Session["ModuleName"].ToString());
        //    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "");
        //    mdocument = mModel.Document;

        //    connstring = GetConnectionString();
        //    mModel.Branches = PopulateBranches();
        //    mModel.BranchList = PopulateBranchesList();

        //    if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
        //    {

        //        #region Attachment
        //        LocalDeliverySheet localDeliverySheet = ctxTFAT.LocalDeliverySheet.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
        //        if (!String.IsNullOrEmpty(localDeliverySheet.LDSNo.ToString()))
        //        {

        //        }
        //        #endregion

        //        mModel.LDSNo = localDeliverySheet.LDSNo;
        //        mModel.Time = localDeliverySheet.Time;
        //        mModel.Date = localDeliverySheet.Date.ToShortDateString();
        //        mModel.VehicleType = localDeliverySheet.VehicleType;
        //        mModel.VehicleTypeName = ctxTFAT.VehicleGrpStatusMas.Where(x => x.Code == mModel.VehicleType).Select(x => x.VehicleGroupStatus).FirstOrDefault();
        //        mModel.VehicleNo = localDeliverySheet.VehicleNo;
        //        mModel.Broker = mModel.Broker;
        //        mModel.BrokerName = ctxTFAT.Broker.Where(x => x.Code.ToString() == mModel.Broker).Select(x => x.Name).FirstOrDefault();
        //        mModel.KM = localDeliverySheet.KM == null ? "0" : Convert.ToInt32(localDeliverySheet.KM).ToString();
        //        mModel.From = localDeliverySheet.FromBranch;
        //        mModel.FromName = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.From).Select(x => x.Name).FirstOrDefault();

        //        #region GetAll RouteVia


        //        if (!(String.IsNullOrEmpty(localDeliverySheet.ToBranch)))
        //        {
        //            List<LR_LC_Combine_VM> lR_LC_Combine_VMs = new List<LR_LC_Combine_VM>();
        //            var GetSourceArry = localDeliverySheet.ToBranch.Split(',');

        //            for (int i = 0; i < GetSourceArry.Length; i++)
        //            {
        //                if (i == GetSourceArry.Length - 1)
        //                {
        //                    mModel.To = GetSourceArry[i];
        //                    //mModel.ToName = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.To).Select(x => x.Name).FirstOrDefault();
        //                    var TO = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.To).Select(x => new { x.Name, x.Category }).FirstOrDefault();
        //                    if (TO.Category == "Zone")
        //                    {
        //                        mModel.ToName = TO.Name + " - Z";
        //                    }
        //                    else if (TO.Category == "Branch")
        //                    {
        //                        mModel.ToName = TO.Name + " - B";

        //                    }
        //                    else if (TO.Category == "SubBranch")
        //                    {
        //                        mModel.ToName = TO.Name + " - SB";
        //                    }
        //                    else
        //                    {
        //                        mModel.ToName = TO.Name + " - A";
        //                    }
        //                }
        //                else
        //                {
        //                    var SourceCode = GetSourceArry[i];


        //                    var SourceName = ctxTFAT.TfatBranch.Where(x => x.Code == SourceCode).Select(x => x.Name).FirstOrDefault();
        //                    LR_LC_Combine_VM lR_LC_Combine_VM = new LR_LC_Combine_VM
        //                    {
        //                        Consigner = SourceName,
        //                        From = SourceCode,
        //                    };
        //                    lR_LC_Combine_VMs.Add(lR_LC_Combine_VM);
        //                }

        //            }
        //            TempData["Destination"] = lR_LC_Combine_VMs;
        //            //mModel.AllDest = lR_LC_Combine_VMs;
        //        }


        //        #endregion


        //        mModel.VehicleCategory = localDeliverySheet.VehicleCategory;
        //        mModel.VehicleCategoryName = ctxTFAT.VehicleCategory.Where(x => x.Code.ToString() == mModel.VehicleCategory).Select(x => x.VehicleCategory1).FirstOrDefault();
        //        mModel.ReceiptNo = localDeliverySheet.ReceiptNo;
        //        mModel.DriverName = localDeliverySheet.Driver;
        //        mModel.LicenceNo = localDeliverySheet.LicenceNo;
        //        mModel.LicenceExpDate = localDeliverySheet.LicenceExpDate.Value.ToShortDateString();
        //        mModel.Owner = localDeliverySheet.Owner;
        //        mModel.ChallanNo = localDeliverySheet.ChallanNo;
        //        mModel.ContactNo = localDeliverySheet.ContactNo.ToString();
        //        mModel.Freight = localDeliverySheet.Freight.ToString();
        //        mModel.Advance = localDeliverySheet.Advance.ToString();
        //        mModel.Balance = localDeliverySheet.Balalnce.ToString();
        //        mModel.PayableAt = localDeliverySheet.PayableAt;
        //        mModel.PayableAtName = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.PayableAt).Select(x => x.Name).FirstOrDefault();
        //        mModel.Remark = localDeliverySheet.Remark;
        //        mModel.PayLoad = localDeliverySheet.PayLoad.ToString();

        //        //List<LCDetail> lCDetails1 = TempData.Peek("ExistingLRLIST") as List<LCDetail>;
        //        var mList = ctxTFAT.LorryReceiptExpenses.Where(x => x.LDSNo == localDeliverySheet.LDSNo).ToList().Count();

        //        if (mList != 0)
        //        {
        //            //string Branch = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        //            //var Child = ctxTFAT.BranchChild.Where(x => x.Code == Branch).Select(x => x.Childs).ToList();
        //            var lCDetail = ctxTFAT.LorryReceiptExpenses.Where(x => x.LDSNo == localDeliverySheet.LDSNo).ToList();
        //            foreach (var item in lCDetail)
        //            {
        //                LRStock lRStock = new LRStock();
        //                //var RecordKey = Convert.ToInt32(item.ReferenceOfTransit);
        //                var RecordKey = Convert.ToInt32(10);
        //                lRStock = ctxTFAT.LRStock.Where(x => x.RECORDKEY == RecordKey).FirstOrDefault();

        //                LcDetailsVM lcDetailsVM = new LcDetailsVM();
        //                if (item.PickType == "GDStock" || item.PickType == "PickStock")
        //                {
        //                    lcDetailsVM.EditLDSNo = true;
        //                }
        //                else
        //                {
        //                    var GetFmno = ctxTFAT.LCMaster.Where(x => x.LCno.ToString() == lRStock.LCNO.ToString()).Select(x => x.DispachFM).FirstOrDefault();
        //                    var CheckVehicleInCurrentBRanchOrNot = ctxTFAT.FMMaster.Where(x => x.FmStatus == "U" && x.FmNo == GetFmno).FirstOrDefault();
        //                    if (CheckVehicleInCurrentBRanchOrNot == null)
        //                    {
        //                        lcDetailsVM.EditLDSNo = false;
        //                    }
        //                    else
        //                    {
        //                        lcDetailsVM.EditLDSNo = true;
        //                    }


        //                }
        //                //lcDetailsVM.recordekey = Convert.ToInt32(item.ReferenceOfTransit == "" ? "0" : item.ReferenceOfTransit);
        //                //lcDetailsVM.recordekey = Convert.ToInt32(item.ReferenceOfTransit == "" ? "0" : item.ReferenceOfTransit);
        //                lcDetailsVM.Qty = lRStock.AllocatBalQty + Convert.ToInt32(item.Qty);
        //                lcDetailsVM.LRActWeight = Convert.ToInt32(item.Weight);
        //                lcDetailsVM.ChrWeight = lRStock.ChrgWeight;
        //                lcDetailsVM.ActWeight = Math.Abs(((lRStock.AllocatBalQty + Convert.ToInt32(item.Qty)) * (lRStock.ActWeight)) / (lRStock.TotalQty));
        //                lcDetailsVM.Lrno = item.LRno;
        //                lcDetailsVM.PickType = item.PickType;
        //                lcDetailsVM.ChrgeType = ctxTFAT.ChargeTypeMaster.Where(x => x.ChargeType.ToLower().Trim() == lRStock.ChrgType.ToLower().Trim()).Select(x => x.ChargeType).FirstOrDefault();
        //                lcDetailsVM.Description = ctxTFAT.DescriptionMaster.Where(x => x.Code == lRStock.Description).Select(x => x.Description).FirstOrDefault();
        //                lcDetailsVM.Unit = ctxTFAT.UnitMaster.Where(x => x.Code == lRStock.Unit).Select(x => x.Name).FirstOrDefault();
        //                lcDetailsVM.From = ctxTFAT.TfatBranch.Where(x => x.Code == lRStock.FromBranch).Select(x => x.Name).FirstOrDefault();
        //                lcDetailsVM.To = ctxTFAT.TfatBranch.Where(x => x.Code == lRStock.ToBranch).Select(x => x.Name).FirstOrDefault();
        //                lcDetailsVM.Consignor = ctxTFAT.Consigner.Where(x => x.Code == lRStock.Consigner).Select(x => x.Name).FirstOrDefault();
        //                lcDetailsVM.Consignee = ctxTFAT.Consigner.Where(x => x.Code == lRStock.Consignee).Select(x => x.Name).FirstOrDefault();
        //                lcDetailsVM.LrType = ctxTFAT.LRTypeMaster.Where(x => x.Code == lRStock.LrType).Select(x => x.LRType).FirstOrDefault();
        //                lcDetailsVM.LRDelivery = lRStock.Delivery == "Godown" ? "Godown" : ctxTFAT.TfatBranch.Where(x => x.Code == lRStock.Delivery).Select(x => x.Name).FirstOrDefault();
        //                lcDetailsVM.LRColln = lRStock.Coln == "G" ? "Godown" : lRStock.Coln == "D" ? "Door" : "Crossing";
        //                lcDetailsVM.LoadGQty = Convert.ToInt32(item.Qty);
        //                lcDetailsVM.Amount = Convert.ToInt32(item.Amount);
        //                lcDetailsVM.ShowAmountColumn = true;
        //                lcDetailsVM.ShowLoadQtyANdWeight = true;
        //                lCDetails.Add(lcDetailsVM);

        //            }
        //        }

        //        TempData["ExistingLRLIST"] = lCDetails;
        //    }
        //    else
        //    {
        //        mModel.From = BranchCode;
        //        mModel.FromName = ctxTFAT.TfatBranch.Where(x => x.Code == BranchCode).Select(x => x.Name).FirstOrDefault();
        //        mModel.Date = DateTime.Now.ToShortDateString();
        //        mModel.Time = DateTime.Now.ToString("HH:mm");
        //        var newFMNo = ctxTFAT.LocalDeliverySheet.OrderByDescending(x => x.RECORDKEY).Select(x => x.LDSNo).Take(1).FirstOrDefault();
        //        if (newFMNo == null || newFMNo == 0)
        //        {
        //            mModel.LDSNo = 1;
        //        }
        //        else
        //        {
        //            mModel.LDSNo = (Convert.ToInt32(newFMNo) + 1);
        //        }
        //    }
        //    Godown_LR_Stock_List("");
        //    Door_LR_Stock_List("");
        //    Transit_LR_List("");
        //    mModel.lcDetailsVMs = lCDetails;
        //    return View(mModel);
        //}

        #region Index(List)

        //public ActionResult Indefffx(GridOption Model)
        //{
        //    GetAllMenu(Session["ModuleName"].ToString());
        //    //AccountingGetAllMenu(Session["ModuleName"].ToString());
        //    //VehicleGetAllMenu(Session["ModuleName"].ToString());
        //    //GeneralGetAllMenu(Session["ModuleName"].ToString());

        //    UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "");

        //    if (Model.Document == null)
        //    {
        //        Model.Document = "";
        //        Model.AccountName = "";
        //    }
        //    moptioncode = Model.OptionCode;
        //    msubcodeof = Model.ViewDataId;
        //    mmodule = Model.Module;
        //    ViewBag.id = Model.ViewDataId;
        //    ViewBag.ViewDataId = Model.ViewDataId;
        //    ViewBag.Header = Model.Header;
        //    ViewBag.Table = Model.TableName;
        //    ViewBag.Controller = Model.Controller;
        //    ViewBag.MainType = Model.MainType;
        //    ViewBag.Controller2 = Model.Controller2;
        //    ViewBag.OptionType = Model.OptionType;
        //    ViewBag.OptionCode = Model.OptionCode;
        //    ViewBag.Module = Model.Module;
        //    ViewBag.ViewName = Model.ViewName;
        //    if (Model.AcType != null)
        //    {
        //        ViewBag.MainType = Model.AcType;
        //    }
        //    var GetActivityPage = ctxTFAT.TfatMenu.Where(x => x.FormatCode.ToLower() == Model.ViewDataId.ToLower() && x.ModuleName == "Transactions" && x.ParentMenu == "Logistics").Select(x => new { x.AllowAdd, x.AllowDelete, x.AllowEdit, x.AllowPrint }).FirstOrDefault();
        //    Model.xAdd = GetActivityPage.AllowAdd;
        //    Model.xDelete = GetActivityPage.AllowDelete;
        //    Model.xEdit = GetActivityPage.AllowEdit;
        //    Model.xPrint = GetActivityPage.AllowPrint;
        //    return View(Model);
        //}

        //public ActionResult GetFormats1()
        //{
        //    var result = ctxTFAT.ReportHeader.Where(x => x.SubCodeOf == msubcodeof && !x.Code.EndsWith(".bak")).Select(m => new
        //    {
        //        Code = m.Code,
        //        Name = m.Code
        //    }).OrderBy(n => n.Code).ToList();
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}

        //[HttpPost]
        //public ActionResult GetGridStructureRecords1(string id)
        //{
        //    var result = ctxTFAT.TfatMenu.Where(z => z.OptionCode == moptioncode && z.ModuleName == mmodule).Select(x => new { x.AllowEdit, x.AllowDelete }).FirstOrDefault();
        //    string mopt = "XXXX";
        //    if (result != null)
        //    {
        //        mopt = (result.AllowEdit == false ? "X" : "E") + (result.AllowDelete == false ? "X" : "D") + "XX";
        //    }

        //    //return GetGridDataColumns(id, "L", mopt);
        //    return GetGridDataColumns(id, "L", "XXXX");
        //}

        //[HttpPost]
        //public ActionResult GetMasterGridData1(GridOption Model)
        //{

        //    if (Model.IsFormatSelected == true)
        //    {
        //        int noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=0 Where SubCodeOf='" + msubcodeof + "'");
        //        noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=-1 Where Code='" + Model.ViewDataId + "'");
        //    }
        //    return GetGridReport(Model, "M", "MainType^" + Model.MainType, false, 0);
        //}

        #endregion

        #endregion


    }
}