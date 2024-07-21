using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class LocalDeliverySheetUnloadController : BaseController
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
            var BranchChild = GetChildGrp(mbranchcode);
            var list = ctxTFAT.VehicleMaster.Where(x => x.Acitve == true && BranchChild.Contains(x.Branch) && x.Status == "Ready").ToList();



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

        public JsonResult GetBroker(string term)
        {
            var list = ctxTFAT.Broker.ToList();


            //var list = ctxTFAT.KilometerMaster.Join(ctxTFAT.TfatBranch, p => p.From, pc => pc.Code, (p, pc) => new { p, pc }).Where(x =>  (!(x.pc.Code.ToLower().Contains("h"))) && (!(x.pc.Code.ToLower().Contains("z")))).Select(X=>X.pc).ToList().Distinct();
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

        public DateTime ConvertDDMMYYTOYYMMDD(string da)
        {
            string abc = da.Substring(6, 4) + "/" + da.Substring(3, 2) + "/" + da.Substring(0, 2);
            return Convert.ToDateTime(abc);
        }

        #region Connected

        public JsonResult From(string term)
        {
                string BranchCode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();

                //var GetParentCode = ctxTFAT.TfatBranch.Where(x => x.Code == mcompcode).Select(x => x.Code).FirstOrDefault();
                List<TfatBranch> list = GetBranch(BranchCode);

                list = list.Where(x => x.Category != "0").ToList();
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
            List<TfatBranch> list = ctxTFAT.TfatBranch.Where(x => x.Category != "Zone" && x.Category != "0" ).ToList();



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

        #endregion

        #region Connected

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

        #endregion

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

        private static List<SelectListItem> PopulateBranches()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            //string constr = ConfigurationManager.ConnectionStrings["tfatEntitiesConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(LocalDeliverySheetUnloadController.connstring))
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
                BroNAme = ctxTFAT.Broker.Where(x => x.Code.ToString() == vehicleMaster.Broker).Select(x => x.Name).FirstOrDefault();
                Payload = (Convert.ToInt32(vehicleMaster.PayLoad) * 1000).ToString();
                DriverMaster driverMaster = ctxTFAT.DriverMaster.Where(x => x.Code == vehicleMaster.Driver).FirstOrDefault();
                DriverName = driverMaster.Name;
                LicenNo = driverMaster.LicenceNo;
                LicenceExpDate = driverMaster.LicenceExpDate == null ? null : driverMaster.LicenceExpDate.Value.ToShortDateString();
                KM = vehicleMaster.KM;
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

        public ActionResult CheckLDSDate(string DocDate, string EndDate, string StartDate)
        {
            string message = "";
            DateTime Date = ConvertDDMMYYTOYYMMDD(DocDate);
            DateTime Statdate = ConvertDDMMYYTOYYMMDD(StartDate);
            DateTime Enddate = ConvertDDMMYYTOYYMMDD(EndDate);

            if (Statdate <= Date && Date <= Enddate)
            {
                message = "T";
            }
            else
            {
                message = "F";
            }

            return Json(new { Message = message, JsonRequestBehavior.AllowGet });
        }

        #endregion


        // GET: Logistics/LocalDeliverySheetUnload
        #region Index(List)

        public ActionResult Index(GridOption Model)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());

            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "A");

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
            return View(Model);
        }

        public ActionResult GetFormats1()
        {
            var result = ctxTFAT.ReportHeader.Where(x => x.SubCodeOf == msubcodeof && !x.Code.EndsWith(".bak")).Select(m => new
            {
                Code = m.Code,
                Name = m.Code
            }).OrderBy(n => n.Code).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords1(string id)
        {
            var result = ctxTFAT.TfatMenu.Where(z => z.OptionCode == moptioncode && z.ModuleName == mmodule).Select(x => new { x.AllowEdit, x.AllowDelete }).FirstOrDefault();
            string mopt = "XXXX";
            if (result != null)
            {
                mopt = (result.AllowEdit == false ? "X" : "E") + (result.AllowDelete == false ? "X" : "D") + "XX";
            }

            //return GetGridDataColumns(id, "L", mopt);
            return GetGridDataColumns(id, "L", mopt);
        }

        //[HttpPost]
        public ActionResult GetMasterGridData1(GridOption Model)
        {

            if (Model.IsFormatSelected == true)
            {
                int noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=0 Where SubCodeOf='" + msubcodeof + "'");
                noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=-1 Where Code='" + Model.ViewDataId + "'");
            }
            return GetGridReport(Model, "M", "MainType^" + Model.MainType, false, 0);
        }

        #endregion

        public ActionResult Index1(LocalDeliverySheetVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "A");
            mdocument = mModel.Document;

            List<LcDetailsVM> lCDetails = new List<LcDetailsVM>();

            mModel.LDSsetup = ctxTFAT.LDSsetup.Where(x => (x.RECORDKEY == 1)).FirstOrDefault();

            connstring = GetConnectionString();
            mModel.Branches = PopulateBranches();

            LocalDeliverySheet localDeliverySheet = ctxTFAT.LocalDeliverySheet.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();

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
                        mModel.ToName = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.To).Select(x => x.Name).FirstOrDefault();

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

                mModel.AllRoute = lR_LC_Combine_VMs;
            }
            #endregion



            mModel.VehicleCategory = localDeliverySheet.VehicleCategory;
            mModel.VehicleCategoryName = ctxTFAT.VehicleCategory.Where(x => x.Code.ToString() == mModel.VehicleCategory).Select(x => x.VehicleCategory1).FirstOrDefault();
            mModel.ReceiptNo = localDeliverySheet.ReceiptNo;
            mModel.DriverName = localDeliverySheet.Driver;
            mModel.LicenceNo = localDeliverySheet.LicenceNo;
            mModel.LicenceExpDate = localDeliverySheet.LicenceExpDate.Value.ToShortDateString();
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


            List<LocalDeliveryRel> LocalDeliveryRels = ctxTFAT.LocalDeliveryRel.Where(x => x.LDSNO == localDeliverySheet.LDSNo).ToList();
            //List<LRStock> lRStocks = ctxTFAT.LRStock.Where(x => x.LCNO == localDeliverySheet.LDSNo && x.StockStatus == "L" && x.StockAt == "LDS").ToList();
            foreach (var item in LocalDeliveryRels)
            {
                if (item.Qty > 0)
                {
                    LRStock lRStock = ctxTFAT.LRStock.Where(x => x.RECORDKEY.ToString() == item.ParentKey).FirstOrDefault();
                    LcDetailsVM lcDetailsVM = new LcDetailsVM
                    {
                        recordekey = (item.ParentKey.ToString()),
                        Qty = Convert.ToInt32(item.Qty),
                        LRActWeight = Convert.ToInt32(item.UnloadWeight),
                        ChrWeight = lRStock.ChrgWeight,
                        ActWeight = Convert.ToInt32(item.Weight),
                        Lrno = item.LRno,
                        PickType = item.PickType,
                        ChrgeType = ctxTFAT.ChargeTypeMaster.Where(x => x.Code.ToLower().Trim() == lRStock.ChrgType.ToLower().Trim()).Select(x => x.ChargeType).FirstOrDefault(),
                        Description = ctxTFAT.DescriptionMaster.Where(x => x.Code == lRStock.Description).Select(x => x.Description).FirstOrDefault(),
                        Unit = ctxTFAT.UnitMaster.Where(x => x.Code == lRStock.Unit).Select(x => x.Name).FirstOrDefault(),
                        From = ctxTFAT.TfatBranch.Where(x => x.Code == lRStock.FromBranch).Select(x => x.Name).FirstOrDefault(),
                        To = ctxTFAT.TfatBranch.Where(x => x.Code == lRStock.ToBranch).Select(x => x.Name).FirstOrDefault(),
                        Consignor = ctxTFAT.Consigner.Where(x => x.Code == lRStock.Consigner).Select(x => x.Name).FirstOrDefault(),
                        Consignee = ctxTFAT.Consigner.Where(x => x.Code == lRStock.Consignee).Select(x => x.Name).FirstOrDefault(),
                        LrType = ctxTFAT.LRTypeMaster.Where(x => x.Code == lRStock.LrType).Select(x => x.LRType).FirstOrDefault(),
                        LRDelivery = lRStock.Delivery == "Godown" ? "Godown" : ctxTFAT.TfatBranch.Where(x => x.Code == lRStock.Delivery).Select(x => x.Name).FirstOrDefault(),
                        LRColln = lRStock.Coln == "G" ? "Godown" : lRStock.Coln == "D" ? "Door" : "Crossing",
                        LoadGQty = Convert.ToInt32(item.UnloadQty),
                        ShowAmountColumn = true,
                        ShowLoadQtyANdWeight = true,
                        Amount = Convert.ToInt32(item.Amount)
                    };
                    lCDetails.Add(lcDetailsVM);
                }
            }

            mModel.lcDetailsVMs = lCDetails;

            return View(mModel);
        }


        public ActionResult SaveData(LocalDeliverySheetVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var Child = GetChildGrp(mbranchcode);

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

                    LocalDeliverySheet localDeliverySheet = ctxTFAT.LocalDeliverySheet.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
                    if (localDeliverySheet != null)
                    {
                        //List<LocalDeliveryRel> LocalDeliveryRels = ctxTFAT.LocalDeliveryRel.Where(x => x.LDSNo == localDeliverySheet.LDSNo).ToList();
                        //List<LRStock> lRStocks = ctxTFAT.LRStock.Where(x => x.LCNO == localDeliverySheet.LDSNo && x.StockStatus == "L" && x.StockAt == "LDS").ToList();
                        //VehicleMaster vehicleMaster = ctxTFAT.VehicleMaster.Where(x => x.Vehicle_No == localDeliverySheet.VehicleNo).FirstOrDefault(); ;

                        if (!String.IsNullOrEmpty(mModel.RecordKey))
                        {
                            var lrno = mModel.LRNoList.Split(',');
                            var loadQty = mModel.LoadQuantity.Split(',');
                            var loadWeight = mModel.LoadWeight.Split(',');
                            var RecordKey = mModel.RecordKey.Split(',');

                            for (int i = 0; i < RecordKey.Length; i++)
                            {
                                var reco = RecordKey[i].ToString();
                                //LRStock
                                LRStock lRStock = ctxTFAT.LRStock.Where(x => x.RECORDKEY.ToString() == reco).FirstOrDefault();
                                //LDS LR Relation
                                LocalDeliveryRel LocalDeliveryRel = ctxTFAT.LocalDeliveryRel.Where(x => x.LDSNO == localDeliverySheet.LDSNo && x.ParentKey == reco && x.LRno == lRStock.LrNo).FirstOrDefault();
                                //LDS LrStock
                                LRStock lRStockLDS = ctxTFAT.LRStock.Where(x => x.LCNO == localDeliverySheet.LDSNo && x.StockAt == "LDS" && x.StockStatus == "L" && x.LrNo == lRStock.LrNo ).FirstOrDefault();

                                if (LocalDeliveryRel.UnloadQty!= Convert.ToInt32(loadQty[i]))
                                {
                                    int Qty = 0;
                                    double Weight = 0;

                                    if (LocalDeliveryRel.UnloadQty < Convert.ToInt32(loadQty[i]))
                                    {
                                        Qty = (Convert.ToInt32(loadQty[i]) - Convert.ToInt32(LocalDeliveryRel.UnloadQty));
                                        Weight = Convert.ToDouble(Math.Round((Convert.ToDecimal(Qty)) / ((decimal)LocalDeliveryRel.Qty) * ((decimal)(Convert.ToInt32(LocalDeliveryRel.Weight)))));


                                        
                                        lRStockLDS.TotalQty -= Qty;
                                        lRStockLDS.AllocatBalQty -= Qty;
                                        lRStockLDS.BalQty -= Qty;
                                        lRStockLDS.ActWeight -= Weight;
                                        lRStockLDS.AllocatBalWght -= Weight;
                                        lRStockLDS.BalWeight -= Weight;

                                        LocalDeliveryRel.UnloadQty += Qty;
                                        LocalDeliveryRel.UnloadWeight = (Convert.ToDouble(LocalDeliveryRel.UnloadWeight) + Weight);

                                        lRStock.AllocatBalQty += Qty;
                                        lRStock.BalQty += Qty;
                                        lRStock.AllocatBalWght += Weight;
                                        lRStock.BalWeight += Weight;
                                    }
                                    else
                                    {
                                        Qty = Convert.ToInt32(LocalDeliveryRel.Qty) - (Convert.ToInt32(loadQty[i]));
                                        Weight = Convert.ToDouble(Math.Round((Convert.ToDecimal(Qty)) / ((decimal)LocalDeliveryRel.Qty) * ((decimal)(Convert.ToInt32(LocalDeliveryRel.Weight)))));


                                        lRStockLDS.TotalQty += Qty;
                                        lRStockLDS.AllocatBalQty += Qty;
                                        lRStockLDS.BalQty += Qty;
                                        lRStockLDS.ActWeight = Weight;
                                        lRStockLDS.AllocatBalWght += Weight;
                                        lRStockLDS.BalWeight += Weight;

                                        LocalDeliveryRel.UnloadQty -= Qty;
                                        LocalDeliveryRel.UnloadWeight = (Convert.ToDouble(LocalDeliveryRel.UnloadWeight) -Weight);

                                        lRStock.AllocatBalQty -= Qty;
                                        lRStock.BalQty -= Qty;
                                        lRStock.AllocatBalWght -= Weight;
                                        lRStock.BalWeight -= Weight;
                                    }
                                }
                                

                                ctxTFAT.Entry(lRStockLDS).State = EntityState.Modified;
                                ctxTFAT.Entry(LocalDeliveryRel).State = EntityState.Modified;
                                ctxTFAT.Entry(lRStock).State = EntityState.Modified;

                            }


                        }
                    }

                    ctxTFAT.SaveChanges();

                    //mnewrecordkey = Convert.ToInt32(mobj.RecordKey);
                    //string mNewCode = "";
                    //mNewCode = mobj.Code;
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

        public void Unload()
        {

        }
        public void Rev_Unload()
        {

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

    }
}