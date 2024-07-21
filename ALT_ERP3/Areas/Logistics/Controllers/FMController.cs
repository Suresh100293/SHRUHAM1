using Common;
using EntitiModel;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using System.Configuration;
using CrystalDecisions.CrystalReports.Engine;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WebForms;
using ALT_ERP3.Areas.Vehicles.Models;
using System.Globalization;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class FMController : BaseController
    {
        string attachmentPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentPath"].ToString()) == true ? "" : ConfigurationManager.AppSettings["AttachmentPath"].ToString();

        private static string mauthorise = "A00";
        private static string mdocument = "";
        private string mnewrecordkey = "";
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        public static string connstring = "";
        //ITransactionGridOperation mIlst = new TransactionGridOperation();
        private new string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private new string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        public new string muserid = System.Web.HttpContext.Current.Session["UserId"].ToString();
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

        List<FmCatchChargesInfo> fmCatchCharges = new List<FmCatchChargesInfo>();
        List<FmCatchChargesInfo> TripfmCatchCharges = new List<FmCatchChargesInfo>();

        #region Function List

        public ActionResult GetDriverTickler(string Driver)
        {
            string SpclRemark = "", BlackListRemark = "", DriverNofiticationMeg = "";
            bool HireSpcl = false, HireBlackList = false, DriverNofitication = false;

            FMSetup fMSetup = ctxTFAT.FMSetup.FirstOrDefault();
            if (fMSetup == null)
            {
                fMSetup = new FMSetup();
            }


            DriverMaster consigner = ctxTFAT.DriverMaster.Where(x => x.Code == Driver).FirstOrDefault();
            if (consigner != null)
            {

                if (!String.IsNullOrEmpty(consigner.Ticklers))
                {
                    HireSpcl = true;
                    SpclRemark = consigner.Ticklers;
                }
                if (!String.IsNullOrEmpty(consigner.HoldTicklers))
                {
                    HireBlackList = true;
                    BlackListRemark = consigner.HoldTicklers;
                }

                var LicenceExpDate = consigner.LicenceExpDate == null ? null : consigner.LicenceExpDate.Value.ToShortDateString();
                if (!String.IsNullOrEmpty(LicenceExpDate) && fMSetup.DriverExp == true)
                {
                    if (consigner.LicenceExpDate.Value < DateTime.Now)
                    {
                        DriverNofitication = true;
                        DriverNofiticationMeg += "Driver License Expired Date :" + LicenceExpDate + ".<br> Current Driver License Expired.<br>";
                    }
                }
            }
            return Json(new
            {
                HireSpcl = HireSpcl,
                HireSpclRemark = SpclRemark,
                HireBlackList = HireBlackList,
                HireBlackListRemark = BlackListRemark,
                DriverNofitication = DriverNofitication,
                DriverNofiticationMeg = DriverNofiticationMeg,
                JsonRequestBehavior.AllowGet
            });
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

        public JsonResult GetVehicleGroupStatus(string term)
        {
            FMSetup fatfmsetup = ctxTFAT.FMSetup.FirstOrDefault();
            if (fatfmsetup == null)
            {
                fatfmsetup = new FMSetup();
            }
            List<string> GetGropCodeList = new List<string>();
            List<VehicleMaster> vehicleMasters = new List<VehicleMaster>();
            var Child = GetChildGrp(mbranchcode);
            if (fatfmsetup.VehiclesBranchWise)
            {
                vehicleMasters = ctxTFAT.VehicleMaster.Where(x => Child.Contains(x.Branch) && x.Acitve == true).ToList();
                //GetGropCodeList = ctxTFAT.VehicleMaster.Where(x => x.Branch.Contains(mbranchcode) && x.Acitve == true && x.VehicleReportingSt == "Ready").Select(x => x.Truck_Status).ToList();
            }
            else
            {
                vehicleMasters = ctxTFAT.VehicleMaster.Where(x => x.Acitve == true).ToList();
            }
            if (fatfmsetup.VehicleReadyStsOnly)
            {
                vehicleMasters = vehicleMasters.Where(x => x.Status == "Ready").ToList();
            }
            GetGropCodeList = vehicleMasters.Select(x => x.TruckStatus).ToList();

            var list = ctxTFAT.VehicleGrpStatusMas.Where(x => x.Acitve == true && GetGropCodeList.Contains(x.Code) || x.Code == "100001").ToList();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.VehicleGroupStatus.ToLower().Contains(term.ToLower())).ToList();
            }

            bool ExcludeHire = false;
            FMSetup fMSetup = ctxTFAT.FMSetup.FirstOrDefault();
            if (fMSetup != null)
            {
                ExcludeHire = fMSetup.ExcludeHire;
            }
            if (ExcludeHire)
            {
                list = list.Where(x => x.Code != "100001").ToList();
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
                    Code = x.Code,
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
                //if (!String.IsNullOrEmpty(TruckStatus) && TruckStatus!= "000000")
                //{
                //    list = list.Where(x => x.TruckStatus.ToLower() == TruckStatus.ToLower()).ToList();
                //}

                if (!(String.IsNullOrEmpty(term)))
                {
                    list = list.Where(x => x.TruckNo.ToLower().Contains(term.ToLower())).ToList();
                }
                var Modified = list.Select(x => new
                {
                    Code = x.Code,
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
            using (SqlConnection con = new SqlConnection(FMController.connstring))
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

        public ActionResult CheckFMDate(string DocDate, string DocTime)
        {
            string message = "", Status = "T";
            var Date = DocDate.Split('/');
            string[] Time = DocTime.Split(':');
            DateTime Fmdate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Time[0]), Convert.ToInt32(Time[1]), 00);

            FMSetup fMSetup = ctxTFAT.FMSetup.FirstOrDefault();
            //if (fMSetup != null)
            //{
            //    if (fMSetup.FMDate==false)
            //    {
            //        var MinDate = DateTime.Now.AddHours(fMSetup.BeforeFMDate * (-1));
            //        var MaxDate = DateTime.Now.AddHours(fMSetup.AfterFMDate);

            //        if (MinDate <= Fmdate && Fmdate <= MaxDate)
            //        {
            //            Status = "T";
            //        }
            //        else
            //        {
            //            Status = "F";
            //            message = "Freight Memo DATE And TIME NOT ALLOW AS PER THE SETUP RULE...!";
            //        }
            //    }
            //    else
            //    {
            //        if (DateTime.Now.ToShortDateString() != Fmdate.ToShortDateString())
            //        {
            //            Status = "F";
            //            message = "Freight Memo Date Allow Only Todays Date AS PER THE SETUP RULE...!";
            //        }
            //    }
            //}

            if (Status == "T")
            {
                var NewDocDate = ConvertDDMMYYTOYYMMDD(Fmdate.ToShortDateString());
                if (ConvertDDMMYYTOYYMMDD(StartDate) <= NewDocDate && NewDocDate <= ConvertDDMMYYTOYYMMDD(EndDate))
                {

                    Status = "T";
                }
                else
                {
                    Status = "F";
                    message = "Financial Date Range Allow Only...!";
                }
            }
            if (Status == "T")
            {
                var NewDate = ConvertDDMMYYTOYYMMDD(DocDate);
                if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.Type == "FM000" && x.LockDate == NewDate).FirstOrDefault() != null)
                {
                    Status = "F";
                    message = "FM Date is Locked By Period Lock System...!";
                }
            }


            return Json(new { Status = Status, Message = message, JsonRequestBehavior.AllowGet });
        }

        public JsonResult From(string term)
        {
            var FillFromCombo = ctxTFAT.FMSetup.Select(x => (bool?)x.FillFromCurr ?? false).FirstOrDefault();
            List<TfatBranch> list = new List<TfatBranch>();
            if (FillFromCombo)
            {
                //HOD Zone Branch SubBranch
                list = ctxTFAT.TfatBranch.Where(x => (x.Status == true && x.Code != "G00000" && x.Grp != "G00000") && (x.Code == mbranchcode || x.Grp == mbranchcode)).ToList();

                var ZoneList = list.Where(x => x.Category == "Zone").Select(x => x.Code).ToList();
                list.AddRange(ctxTFAT.TfatBranch.Where(x => ZoneList.Contains(x.Code) || ZoneList.Contains(x.Grp)).ToList());

                var BranchList = list.Where(x => x.Category == "Branch").Select(x => x.Code).ToList();
                list.AddRange(ctxTFAT.TfatBranch.Where(x => BranchList.Contains(x.Code) || BranchList.Contains(x.Grp)).ToList());

                var SubBranchList = list.Where(x => x.Category == "SubBranch").Select(x => x.Code).ToList();
                list.AddRange(ctxTFAT.TfatBranch.Where(x => SubBranchList.Contains(x.Code) || SubBranchList.Contains(x.Grp)).ToList());

                //list = GetBranch(mbranchcode);

                var GeneralArea = ctxTFAT.TfatBranch.Where(x => x.Grp == "G00000" && x.Category == "Area" && x.Status == true).OrderBy(x => x.Name).ToList();
                list.AddRange(GeneralArea);
            }
            else
            {
                list = ctxTFAT.TfatBranch.Where(x => x.Status == true && x.Code != "G00000").ToList();
            }
            var Newlist = list.Distinct();
            if (!(String.IsNullOrEmpty(term)))
            {
                Newlist = Newlist.Where(x => x.Name.ToLower().Contains(term.ToLower())).OrderBy(x => x.Name).Take(10).ToList();
            }

            Newlist.Where(w => w.Category == "Zone").ToList().ForEach(w => w.Name = w.Name + " - Z");
            Newlist.Where(w => w.Category == "0").ToList().ForEach(w => w.Name = w.Name + " - HO");
            Newlist.Where(w => w.Category == "Branch").ToList().ForEach(w => w.Name = w.Name + " - B");
            Newlist.Where(w => w.Category == "SubBranch").ToList().ForEach(w => w.Name = w.Name + " - SB");
            Newlist.Where(w => w.Category == "Area").ToList().ForEach(w => w.Name = w.Name + " - A");


            var Modified = Newlist.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);

        }

        public JsonResult To(string term)
        {
            List<TfatBranch> list = ctxTFAT.TfatBranch.Where(x => x.Status == true).OrderBy(x => x.Name).ToList();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).OrderBy(x => x.Name).ToList();
            }

            list.Where(w => w.Category == "Zone").ToList().ForEach(w => w.Name = w.Name + " - Z");
            list.Where(w => w.Category == "0").ToList().ForEach(w => w.Name = w.Name + " - HO");
            list.Where(w => w.Category == "Branch").ToList().ForEach(w => w.Name = w.Name + " - B");
            list.Where(w => w.Category == "SubBranch").ToList().ForEach(w => w.Name = w.Name + " - SB");
            list.Where(w => w.Category == "Area").ToList().ForEach(w => w.Name = w.Name + " - A");

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AddDestination(string term)
        {
            List<TfatBranch> list = ctxTFAT.TfatBranch.Where(x => x.Status == true).OrderBy(x => x.Name).ToList();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).OrderBy(x => x.Name).ToList();
            }

            list.Where(w => w.Category == "Zone").ToList().ForEach(w => w.Name = w.Name + " - Z");
            list.Where(w => w.Category == "0").ToList().ForEach(w => w.Name = w.Name + " - HO");
            list.Where(w => w.Category == "Branch").ToList().ForEach(w => w.Name = w.Name + " - B");
            list.Where(w => w.Category == "SubBranch").ToList().ForEach(w => w.Name = w.Name + " - SB");
            list.Where(w => w.Category == "Area").ToList().ForEach(w => w.Name = w.Name + " - A");

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteRouteBranch(string term, string Branch)
        {
            List<TfatBranch> list = ctxTFAT.TfatBranch.Where(x => x.Status == true && x.Grp != "G00000" && x.Code != "G00000").OrderBy(x => x.Name).ToList();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.Name.ToLower().Contains(term.ToLower()) && (!Branch.Contains(x.Code.ToLower()))).OrderBy(x => x.Name).ToList();
            }

            list.Where(w => w.Category == "Zone").ToList().ForEach(w => w.Name = w.Name + " - Z");
            list.Where(w => w.Category == "0").ToList().ForEach(w => w.Name = w.Name + " - HO");
            list.Where(w => w.Category == "Branch").ToList().ForEach(w => w.Name = w.Name + " - B");
            list.Where(w => w.Category == "SubBranch").ToList().ForEach(w => w.Name = w.Name + " - SB");
            list.Where(w => w.Category == "Area").ToList().ForEach(w => w.Name = w.Name + " - A");

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);

        }

        public JsonResult PayableAt(string term)
        {

            List<TfatBranch> list = ctxTFAT.TfatBranch.Where(x => x.Code != "G00000" && x.Grp != "G00000" && x.Category != "Area").OrderBy(x => x.Name).ToList();


            list.Where(w => w.Category == "Zone").ToList().ForEach(w => w.Name = w.Name + " - Z");
            list.Where(w => w.Category == "0").ToList().ForEach(w => w.Name = w.Name + " - HO");
            list.Where(w => w.Category == "Branch").ToList().ForEach(w => w.Name = w.Name + " - B");
            list.Where(w => w.Category == "SubBranch").ToList().ForEach(w => w.Name = w.Name + " - SB");
            list.Where(w => w.Category == "Area").ToList().ForEach(w => w.Name = w.Name + " - A");



            var Newlist = list.Distinct();
            if (!(String.IsNullOrEmpty(term)))
            {
                Newlist = Newlist.Where(x => x.Name.ToLower().Contains(term.ToLower())).OrderBy(x => x.Category).Take(10).ToList();
            }

            Newlist.Where(w => w.Category == "Zone").ToList().ForEach(w => w.Name = w.Name + " - Z");
            Newlist.Where(w => w.Category == "0").ToList().ForEach(w => w.Name = w.Name + " - HO");
            Newlist.Where(w => w.Category == "Branch").ToList().ForEach(w => w.Name = w.Name + " - B");
            Newlist.Where(w => w.Category == "SubBranch").ToList().ForEach(w => w.Name = w.Name + " - SB");
            Newlist.Where(w => w.Category == "Area").ToList().ForEach(w => w.Name = w.Name + " - A");


            var Modified = Newlist.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FetChGrupStatus(string VehicleNo)
        {
            string DriverSpclRemark = "", DriverBlackListRemark = "", DriverNofiticationMeg = "";
            bool VehiclePosting = false, DriverPosting = false, ChangeDriverCharges = false, ChangeVehicleCharges = false, DriverHireSpcl = false, DriverHireBlackList = false, DriverNofitication = false;
            string VehicleNoName = "";
            string NofiticationMeg = "", HireSpclRemark = "", HireBlackListRemark = "", MaintainDriverAc = "F", FMVouRel = "F", CatchFreight = "F", DriverNCombo = "", DriverContact = "", DriverCode = "", Payload = "", BroCode = "", BroNAme = "", VehicleCategoryCode = "", VehicleCategoryName = "", OwnerName = "", DriverName = "", LicenNo = "", LicenceExpDate = "", VehicleGroupStatusCode = "", VehicleGroupStatusName = "";
            int KM = 0;
            bool CutTDS = false, Nofitication = false, HireSpcl = false, HireBlackList = false, AllowToChange = false;
            decimal TDSRate = 0;
            VehicleMaster vehicleMaster = ctxTFAT.VehicleMaster.Where(x => x.Code == VehicleNo).FirstOrDefault();
            FMSetup fMSetup = ctxTFAT.FMSetup.FirstOrDefault();

            string VehicleGroupCode = "", VehicleGroupName = "";


            if (fMSetup == null)
            {
                fMSetup = new FMSetup();
            }
            if (vehicleMaster != null)
            {
                VehicleGroupCode = vehicleMaster.TruckStatus;
                if (!String.IsNullOrEmpty(VehicleGroupCode))
                {
                    VehicleGroupName = ctxTFAT.VehicleGrpStatusMas.Where(x => x.Code == VehicleGroupCode).Select(x => x.VehicleGroupStatus).FirstOrDefault();
                }
                VehicleNoName = vehicleMaster.TruckNo;
                OwnerName = vehicleMaster.Owner;
                VehicleGroupStatusCode = vehicleMaster.TruckStatus;
                VehicleGroupStatusName = ctxTFAT.VehicleGrpStatusMas.Where(x => x.Code == vehicleMaster.TruckStatus).Select(x => x.VehicleGroupStatus).FirstOrDefault();
                VehicleCategoryCode = vehicleMaster.VCategory;
                VehicleCategoryName = ctxTFAT.VehicleCategory.Where(x => x.Code == vehicleMaster.VCategory).Select(x => x.VehicleCategory1).FirstOrDefault();
                BroCode = vehicleMaster.Broker;
                BroNAme = ctxTFAT.Master.Where(x => x.Code.ToString() == vehicleMaster.Broker).Select(x => x.Name).FirstOrDefault();
                ChangeDriverCharges = vehicleMaster.ChangeDriverFreight_Advance;
                ChangeVehicleCharges = vehicleMaster.ChangeVehicleFreight_Advance;
                DriverPosting = vehicleMaster.MaintainDriverAC;
                VehiclePosting = vehicleMaster.PostReq;
                string mBroPostCode = BroCode;
                string VehicleLedPostCode = vehicleMaster.PostAc;
                Payload = vehicleMaster.PayLoad.ToString();
                var mCreditAcc = (vehicleMaster.CrAc == "B") ? mBroPostCode : VehicleLedPostCode;
                var tdsdetails = ctxTFAT.TaxDetails.Where(x => x.Code == mCreditAcc).Select(x => new { x.CutTDS, x.TDSCode }).FirstOrDefault();

                var tdscode = tdsdetails == null ? 0 : (tdsdetails.TDSCode == null) ? 0 : tdsdetails.TDSCode.Value;
                var mDocDate = DateTime.Now.Date;
                var TDSRATEtab = ctxTFAT.TDSRates.Where(x => x.Code == tdscode && x.EffDate <= mDocDate).OrderByDescending(x => x.EffDate).Select(x => new { x.TDSRate, x.Cess, x.SHECess, x.SurCharge }).FirstOrDefault();
                //var TDSRATEtab = ctxTFAT.Master.Where(x=>x.Code== mCreditAcc).Select(x=>x.ra).FirstOrDefault();

                if (vehicleMaster.TruckStatus == "100000" && fMSetup.ATDSAdjReq == true)
                {
                    CutTDS = true;
                    AllowToChange = true;
                    TDSRate = (TDSRATEtab != null && CutTDS == true) ? ((TDSRATEtab.TDSRate == null) ? 0 : TDSRATEtab.TDSRate.Value) : 0;
                }
                else if (vehicleMaster.TruckStatus == "100002" && fMSetup.OTDSAdjReq == true)
                {
                    CutTDS = true;
                    AllowToChange = true;
                    TDSRate = (TDSRATEtab != null && CutTDS == true) ? ((TDSRATEtab.TDSRate == null) ? 0 : TDSRATEtab.TDSRate.Value) : 0;
                }


                if (!String.IsNullOrEmpty(vehicleMaster.RateType))
                {
                    CatchFreight = "T";
                }


                if (vehicleMaster.TruckStatus != "100001")
                {
                    MaintainDriverAc = "T";
                }
                //if (vehicleMaster.FMVOURELReq)
                //{
                //    FMVouRel = "T";
                //}
                var Driver = ctxTFAT.VehicleDri_Hist.Where(x => x.TruckNo == vehicleMaster.Code).OrderByDescending(x => new { x.FromPeriod, x.FromTime }).Select(x => x.Driver).FirstOrDefault();
                DriverMaster driverMaster = ctxTFAT.DriverMaster.Where(x => x.Code == Driver).FirstOrDefault();
                if (driverMaster != null)
                {
                    DriverNCombo = driverMaster.Name;
                    DriverContact = driverMaster.MobileNo1;
                    DriverCode = driverMaster.Code;
                    LicenNo = driverMaster.LicenceNo;
                    LicenceExpDate = driverMaster.LicenceExpDate == null ? null : driverMaster.LicenceExpDate.Value.ToShortDateString();

                    if (!String.IsNullOrEmpty(LicenceExpDate) && fMSetup.DriverExp == true)
                    {
                        if (driverMaster.LicenceExpDate.Value < DateTime.Now)
                        {
                            DriverNofitication = true;
                            DriverNofiticationMeg += "Driver License Expired Date :" + LicenceExpDate + ".<br> Current Driver License Expired.<br>";

                        }
                    }
                    if (!String.IsNullOrEmpty(driverMaster.Ticklers))
                    {
                        DriverHireSpcl = true;
                        DriverSpclRemark = driverMaster.Ticklers;
                    }
                    if (!String.IsNullOrEmpty(driverMaster.HoldTicklers))
                    {
                        DriverHireBlackList = true;
                        DriverBlackListRemark = driverMaster.HoldTicklers;
                    }
                }
                //KM = ctxTFAT.VehicleKmMaintainMa.OrderByDescending(x => x.Date).Where(x => x.VehicleNo == vehicleMaster.TruckNo).Select(x => x.KM).FirstOrDefault();
                KM = vehicleMaster.KM;

                if (vehicleMaster.RemarkReq)
                {
                    if (!String.IsNullOrEmpty(vehicleMaster.Remark))
                    {
                        HireSpcl = true;
                        HireSpclRemark = vehicleMaster.Remark;
                    }
                }
                if (vehicleMaster.HoldActivityReq)
                {
                    if (!String.IsNullOrEmpty(vehicleMaster.HoldRemark))
                    {
                        HireBlackList = true;
                        HireBlackListRemark = vehicleMaster.HoldRemark;
                    }
                }
                if (fMSetup.FitnessExp == true || fMSetup.InsuranceExp == true || fMSetup.PUCExp == true || fMSetup.AIPExp == true || fMSetup.StateTaxExp == true || fMSetup.TPStateExp == true || fMSetup.GreenTaxExp == true)
                {
                    List<ExpenseseOfVehicle> expeselist = new List<ExpenseseOfVehicle>();
                    ExecuteStoredProc("Drop Table Ztemp_FMVehiclemasterDue");
                    ExecuteStoredProc("select R.Code as ExpensesAc,R.date2 as TODT into Ztemp_FMVehiclemasterDue from RelateData R  where  R.value8 in  ( '" + vehicleMaster.PostAc + "')	 and   R.Code in ('000100326','000100653','000100736','000100781','000100789','000100953','000101135')");
                    ExecuteStoredProc("insert into Ztemp_FMVehiclemasterDue select R.combo1 as ExpensesAc,R.date2 as TODT from RelateData R  where  R.code in  ( '" + vehicleMaster.PostAc + "')	 and   R.combo1 in ('000100326','000100653','000100736','000100781','000100789','000100953','000101135')");
                    ExecuteStoredProc("Drop Table Ztemp_FMVehiclemasterDue1");
                    ExecuteStoredProc("WITH ranked_messages AS (SELECT m.*, ROW_NUMBER() OVER (PARTITION BY ExpensesAc ORDER BY Todt DESC) AS rn FROM Ztemp_FMVehiclemasterDue AS m ) SELECT *  into Ztemp_FMVehiclemasterDue1 FROM ranked_messages WHERE rn = 1;");
                    var ToDate = (Convert.ToDateTime(DateTime.Now.ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

                    string Query = "select  *from Ztemp_FMVehiclemasterDue1 where TODT < '" + ToDate + "'";
                    DataTable dt = new DataTable();
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();
                    expeselist = (from DataRow dr in dt.Rows
                                  select new ExpenseseOfVehicle()
                                  {
                                      Code = dr["ExpensesAc"].ToString(),
                                      ToDt = Convert.ToDateTime(dr["TODT"].ToString()),
                                  }).ToList();

                    ExecuteStoredProc("Drop Table Ztemp_FMVehiclemasterDue");
                    ExecuteStoredProc("Drop Table Ztemp_FMVehiclemasterDue1");

                    var FitnessExp = expeselist.Where(x => x.Code == "000100736").Select(x => x.ToDt.ToShortDateString()).FirstOrDefault();
                    var InsuranceExp = expeselist.Where(x => x.Code == "000100326").Select(x => x.ToDt.ToShortDateString()).FirstOrDefault();
                    var PUCExp = expeselist.Where(x => x.Code == "000100789").Select(x => x.ToDt.ToShortDateString()).FirstOrDefault();
                    var AIPExp = expeselist.Where(x => x.Code == "000100781").Select(x => x.ToDt.ToShortDateString()).FirstOrDefault();
                    var StateTaxExp = expeselist.Where(x => x.Code == "000100953").Select(x => x.ToDt.ToShortDateString()).FirstOrDefault();
                    var TPStateExp = expeselist.Where(x => x.Code == "000101135").Select(x => x.ToDt.ToShortDateString()).FirstOrDefault();
                    var GreenTaxExp = expeselist.Where(x => x.Code == "000100653").Select(x => x.ToDt.ToShortDateString()).FirstOrDefault();
                    if (!String.IsNullOrEmpty(FitnessExp) && fMSetup.FitnessExp == true)
                    {
                        Nofitication = true;
                        NofiticationMeg += "Fitness Expired Date :" + FitnessExp + ".<br> Current Vehicle Fitness Expired Please Renew IT.<br>";
                    }
                    if (!String.IsNullOrEmpty(InsuranceExp) && fMSetup.InsuranceExp == true)
                    {
                        Nofitication = true;
                        NofiticationMeg += "Insurance Expired Date :" + InsuranceExp + ".<br> Current Vehicle Insurance Expired Please Renew IT.<br>";
                    }
                    if (!String.IsNullOrEmpty(PUCExp) && fMSetup.PUCExp == true)
                    {
                        Nofitication = true;
                        NofiticationMeg += "PUC Expired Date :" + PUCExp + ".<br> Current Vehicle PUC Expired Please Renew IT.<br>";
                    }
                    if (!String.IsNullOrEmpty(AIPExp) && fMSetup.AIPExp == true)
                    {
                        Nofitication = true;
                        NofiticationMeg += "AIP(1 Year) Expired Date :" + AIPExp + ".<br> Current Vehicle AIP(1 Year) Expired Please Renew IT.<br>";
                    }
                    if (!String.IsNullOrEmpty(StateTaxExp) && fMSetup.StateTaxExp == true)
                    {
                        Nofitication = true;
                        NofiticationMeg += "StateTax(5 Year) Expired Date :" + StateTaxExp + ".<br> Current Vehicle StateTax(5 Year) Expired Please Renew IT.<br>";
                    }
                    if (!String.IsNullOrEmpty(TPStateExp) && fMSetup.TPStateExp == true)
                    {
                        Nofitication = true;
                        NofiticationMeg += "TPState Expired Date :" + TPStateExp + ".<br> Current Vehicle TPState Expired Please Renew IT.<br>";
                    }
                    if (!String.IsNullOrEmpty(GreenTaxExp) && fMSetup.GreenTaxExp == true)
                    {
                        Nofitication = true;
                        NofiticationMeg += "GreenTax Expired Date :" + GreenTaxExp + ".<br> Current Vehicle GreenTax Expired Please Renew IT.<br>";
                    }
                }
            }
            else
            {
                HireVehicleMaster hireVehicle = ctxTFAT.HireVehicleMaster.Where(x => x.Code.Trim() == VehicleNo.Trim()).FirstOrDefault();
                if (hireVehicle != null)
                {
                    VehicleNoName = hireVehicle.TruckNo;

                    VehicleGroupStatusCode = hireVehicle.TruckStatus;
                    VehicleGroupStatusName = ctxTFAT.VehicleGrpStatusMas.Where(x => x.Code == hireVehicle.TruckStatus).Select(x => x.VehicleGroupStatus).FirstOrDefault();
                    VehicleCategoryCode = hireVehicle.VCategory;
                    VehicleCategoryName = ctxTFAT.VehicleCategory.Where(x => x.Code == hireVehicle.VCategory).Select(x => x.VehicleCategory1).FirstOrDefault();
                    BroCode = hireVehicle.Broker;
                    BroNAme = ctxTFAT.Master.Where(x => x.Code.ToString() == hireVehicle.Broker).Select(x => x.Name).FirstOrDefault();
                    Payload = hireVehicle.PayLoad.ToString();
                    DriverName = hireVehicle.Driver;

                    //KM = ctxTFAT.VehicleKmMaintainMa.OrderByDescending(x => x.Date).Where(x => x.VehicleNo == hireVehicle.TruckNo).Select(x => x.KM).FirstOrDefault();
                    KM = hireVehicle.KM;

                    ChangeDriverCharges = hireVehicle.ChangeDriverFreight_Advance;
                    ChangeVehicleCharges = hireVehicle.ChangeVehicleFreight_Advance;
                    DriverPosting = false;
                    VehiclePosting = true;

                    string mBroPostCode = BroCode;
                    var mCreditAcc = BroCode;
                    var tdsdetails = ctxTFAT.TaxDetails.Where(x => x.Code == mCreditAcc).Select(x => new { x.CutTDS, x.TDSCode }).FirstOrDefault();
                    var tdscode = tdsdetails == null ? 0 : (tdsdetails.TDSCode == null) ? 0 : tdsdetails.TDSCode.Value;
                    var mDocDate = DateTime.Now.Date;
                    var TDSRATEtab = ctxTFAT.TDSRates.Where(x => x.Code == tdscode && x.EffDate <= mDocDate).OrderByDescending(x => x.EffDate).Select(x => new { x.TDSRate, x.Cess, x.SHECess, x.SurCharge }).FirstOrDefault();




                    if (hireVehicle.SpclRemarkReq)
                    {
                        if (!String.IsNullOrEmpty(hireVehicle.SpclRemark))
                        {
                            HireSpcl = true;
                            HireSpclRemark = hireVehicle.SpclRemark;
                        }
                    }
                    if (hireVehicle.BlackListReq)
                    {
                        if (!String.IsNullOrEmpty(hireVehicle.BlackListRemark))
                        {
                            HireBlackList = true;
                            HireBlackListRemark = hireVehicle.BlackListRemark;
                        }
                    }

                    if (fMSetup.HTDSAdjReq == true)
                    {
                        CutTDS = true;
                        AllowToChange = true;
                        TDSRate = (TDSRATEtab != null && CutTDS == true) ? ((TDSRATEtab.TDSRate == null) ? 0 : TDSRATEtab.TDSRate.Value) : 0;
                    }
                }
            }
            return Json(new
            {
                VehicleGroupCode= VehicleGroupCode,
                VehicleGroupName= VehicleGroupName,
                DriverPosting = DriverPosting,
                VehiclePosting = VehiclePosting,
                ChangeVehicleCharges = ChangeVehicleCharges,
                ChangeDriverCharges = ChangeDriverCharges,
                HireSpcl = HireSpcl,
                HireSpclRemark = HireSpclRemark,
                HireBlackList = HireBlackList,
                HireBlackListRemark = HireBlackListRemark,
                DriverNCombo = DriverNCombo,
                DriverCode = DriverCode,
                VehicleNo = VehicleNo,
                VehicleNoName = VehicleNoName,
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
                MaintainDriverAc = MaintainDriverAc,
                FMVouRel = FMVouRel,
                CutTDS = CutTDS,
                TDSRate = TDSRate,
                CatchFreight = CatchFreight,
                AllowToChange = AllowToChange,
                DriverContact = DriverContact,
                DriverHireSpcl = DriverHireSpcl,
                DriverSpclRemark = DriverSpclRemark,
                DriverHireBlackList = DriverHireBlackList,
                DriverBlackListRemark = DriverBlackListRemark,
                Nofitication = Nofitication,
                NofiticationMeg = NofiticationMeg,
                DriverNofitication = DriverNofitication,
                DriverNofiticationMeg = DriverNofiticationMeg,
                JsonRequestBehavior.AllowGet
            });
        }

        #region Fetching Freight Advance ViaFreight ViaAdvance LocalFreight LocalAdvance

        public ActionResult GetChargesOfFM(string FromBranch, string RouteVia, string ToBranch, string Vehicle, string VehicleCategory, string FMDate, string SFMno)
        {
            int FMno = String.IsNullOrEmpty(SFMno) == true ? 0 : Convert.ToInt32(SFMno);
            Session["FMFreightAdvance"] = null;
            Session["TripFMFreightAdvance"] = null;
            fmCatchCharges = new List<FmCatchChargesInfo>();
            TripfmCatchCharges = new List<FmCatchChargesInfo>();
            decimal Freight = 0, Advance = 0, ViaAdvance = 0, ViaFreight = 0;
            decimal TripFreight = 0, TripAdvance = 0, TripViaAdvance = 0, TripViaFreight = 0;
            VehicleMaster vehicleMaster = new VehicleMaster();
            if (!(String.IsNullOrEmpty(Vehicle)) && !(String.IsNullOrEmpty(FromBranch)) && !(String.IsNullOrEmpty(ToBranch)))
            {
                vehicleMaster = ctxTFAT.VehicleMaster.Where(x => x.Code.Trim() == Vehicle.Trim()).FirstOrDefault();
                if (vehicleMaster != null)
                {
                    DateTime CurrentDate = ConvertDDMMYYTOYYMMDD(FMDate);
                    TfatBranch tfatBranchFrom = new TfatBranch();
                    TfatBranch tfatBranchTo = new TfatBranch();
                    bool CheckParent = vehicleMaster.GetParentRateAlso;
                    if (CheckParent)
                    {
                        tfatBranchFrom = ctxTFAT.TfatBranch.Where(x => x.Code == FromBranch).FirstOrDefault();
                        if (tfatBranchFrom.Category == "Area" || tfatBranchFrom.Category == "SubBranch")
                        {
                            tfatBranchFrom = ctxTFAT.TfatBranch.Where(x => x.Code == tfatBranchFrom.Grp).FirstOrDefault();
                        }
                        tfatBranchTo = ctxTFAT.TfatBranch.Where(x => x.Code == ToBranch).FirstOrDefault();
                        if (tfatBranchTo.Category == "Area" || tfatBranchTo.Category == "SubBranch")
                        {
                            tfatBranchTo = ctxTFAT.TfatBranch.Where(x => x.Code == tfatBranchTo.Grp).FirstOrDefault();
                        }
                    }
                    var RateType = vehicleMaster.RateType.Split(',').ToList();

                    if (vehicleMaster.PickVehicleRate)
                    {
                        #region VEHICLE CHARGES
                        //Catch Freight,LocalFreight,Advance,LocalAdvance Separately
                        
                        #region Freight && Advance

                        if (RateType.Contains("V"))
                        {
                            //From TO
                            var FreightAdvance = CatchFreightAdvance("VNo", Vehicle, FromBranch, CurrentDate, ToBranch, FMno);
                            Freight = FreightAdvance[0];
                            Advance = FreightAdvance[1];
                            if (FreightAdvance[0] == 0 && FreightAdvance[1] == 0)
                            {
                                //From-Parent => TO
                                FreightAdvance = CatchFreightAdvance("VNo", Vehicle, tfatBranchFrom.Code, CurrentDate, ToBranch, FMno);
                                Freight = FreightAdvance[0];
                                Advance = FreightAdvance[1];
                                if (FreightAdvance[0] == 0 && FreightAdvance[1] == 0)
                                {
                                    //From => TO-Parent
                                    FreightAdvance = CatchFreightAdvance("VNo", Vehicle, FromBranch, CurrentDate, tfatBranchTo.Code, FMno);
                                    Freight = FreightAdvance[0];
                                    Advance = FreightAdvance[1];
                                    if (FreightAdvance[0] == 0 && FreightAdvance[1] == 0)
                                    {
                                        //From-Parent => TO-Parent
                                        FreightAdvance = CatchFreightAdvance("VNo", Vehicle, FromBranch, CurrentDate, tfatBranchTo.Code, FMno);
                                        Freight = FreightAdvance[0];
                                        Advance = FreightAdvance[1];
                                        if (FreightAdvance[0] == 0 && FreightAdvance[1] == 0)
                                        {
                                            //Category Check
                                            if (RateType.Contains("C"))
                                            {
                                                //From TO
                                                FreightAdvance = CatchFreightAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, ToBranch, FMno);
                                                Freight = FreightAdvance[0];
                                                Advance = FreightAdvance[1];
                                                if (FreightAdvance[0] == 0 && FreightAdvance[1] == 0)
                                                {
                                                    //From-Parent => TO
                                                    FreightAdvance = CatchFreightAdvance("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, ToBranch, FMno);
                                                    Freight = FreightAdvance[0];
                                                    Advance = FreightAdvance[1];
                                                    if (FreightAdvance[0] == 0 && FreightAdvance[1] == 0)
                                                    {
                                                        //From => TO-Parent
                                                        FreightAdvance = CatchFreightAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, tfatBranchTo.Code, FMno);
                                                        Freight = FreightAdvance[0];
                                                        Advance = FreightAdvance[1];
                                                        if (FreightAdvance[0] == 0 && FreightAdvance[1] == 0)
                                                        {
                                                            //From-Parent => TO-Parent
                                                            FreightAdvance = CatchFreightAdvance("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, tfatBranchTo.Code, FMno);
                                                            Freight = FreightAdvance[0];
                                                            Advance = FreightAdvance[1];
                                                            if (FromBranch == ToBranch)
                                                            {
                                                                var LocalFreight = CatchLocalFreightAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, FMno);
                                                                Freight += LocalFreight[0];
                                                                Advance += LocalFreight[1];
                                                            }
                                                            else
                                                            {
                                                                var LocalFreight = CatchLocalFreightAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, FMno);
                                                                Freight += LocalFreight[0];
                                                                Advance += LocalFreight[1];
                                                                LocalFreight = CatchLocalFreightAdvance("VCategory", VehicleCategory, ToBranch, CurrentDate, FMno);
                                                                Freight += LocalFreight[0];
                                                                Advance += LocalFreight[1];
                                                            }
                                                        }
                                                        else
                                                        {
                                                            var LocalFreight = CatchLocalFreightAdvance("VCategory", VehicleCategory, ToBranch, CurrentDate, FMno);
                                                            Freight += LocalFreight[0];
                                                            Advance += LocalFreight[1];
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var LocalFreight = CatchLocalFreightAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, FMno);
                                                        Freight += LocalFreight[0];
                                                        Advance += LocalFreight[1];
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (FromBranch == ToBranch)
                                                {
                                                    var LocalFreight = CatchLocalFreightAdvance("VNo", Vehicle, FromBranch, CurrentDate, FMno);
                                                    Freight += LocalFreight[0];
                                                    Advance += LocalFreight[1];
                                                }
                                                else
                                                {
                                                    var LocalFreight = CatchLocalFreightAdvance("VNo", Vehicle, FromBranch, CurrentDate, FMno);
                                                    Freight += LocalFreight[0];
                                                    Advance += LocalFreight[1];
                                                    LocalFreight = CatchLocalFreightAdvance("VNo", Vehicle, ToBranch, CurrentDate, FMno);
                                                    Freight += LocalFreight[0];
                                                    Advance += LocalFreight[1];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (FromBranch == ToBranch)
                                            {
                                                var LocalFreight = CatchLocalFreightAdvance("VNo", Vehicle, FromBranch, CurrentDate, FMno);
                                                Freight += LocalFreight[0];
                                                Advance += LocalFreight[1];
                                            }
                                            else
                                            {
                                                var LocalFreight = CatchLocalFreightAdvance("VNo", Vehicle, FromBranch, CurrentDate, FMno);
                                                Freight += LocalFreight[0];
                                                Advance += LocalFreight[1];
                                                LocalFreight = CatchLocalFreightAdvance("VNo", Vehicle, ToBranch, CurrentDate, FMno);
                                                Freight += LocalFreight[0];
                                                Advance += LocalFreight[1];
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var LocalFreight = CatchLocalFreightAdvance("VNo", Vehicle, ToBranch, CurrentDate, FMno);
                                        Freight += LocalFreight[0];
                                        Advance += LocalFreight[1];
                                    }
                                }
                                else
                                {
                                    var LocalFreight = CatchLocalFreightAdvance("VNo", Vehicle, FromBranch, CurrentDate, FMno);
                                    Freight += LocalFreight[0];
                                    Advance += LocalFreight[1];
                                }
                            }
                        }
                        else
                        {
                            if (RateType.Contains("C"))
                            {
                                //From TO
                                var FreightAdvance = CatchFreightAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, ToBranch, FMno);
                                Freight = FreightAdvance[0];
                                Advance = FreightAdvance[1];
                                if (FreightAdvance[0] == 0 && FreightAdvance[1] == 0)
                                {
                                    //From-Parent => TO
                                    FreightAdvance = CatchFreightAdvance("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, ToBranch, FMno);
                                    Freight = FreightAdvance[0];
                                    Advance = FreightAdvance[1];
                                    if (FreightAdvance[0] == 0 && FreightAdvance[1] == 0)
                                    {
                                        //From => TO-Parent
                                        FreightAdvance = CatchFreightAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, tfatBranchTo.Code, FMno);
                                        Freight = FreightAdvance[0];
                                        Advance = FreightAdvance[1];
                                        if (FreightAdvance[0] == 0 && FreightAdvance[1] == 0)
                                        {
                                            //From-Parent => TO-Parent
                                            FreightAdvance = CatchFreightAdvance("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, tfatBranchTo.Code, FMno);
                                            Freight = FreightAdvance[0];
                                            Advance = FreightAdvance[1];
                                            if (FromBranch == ToBranch)
                                            {
                                                var LocalFreight = CatchLocalFreightAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, FMno);
                                                Freight += LocalFreight[0];
                                                Advance += LocalFreight[1];
                                            }
                                            else
                                            {
                                                var LocalFreight = CatchLocalFreightAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, FMno);
                                                Freight += LocalFreight[0];
                                                Advance += LocalFreight[1];
                                                LocalFreight = CatchLocalFreightAdvance("VCategory", VehicleCategory, ToBranch, CurrentDate, FMno);
                                                Freight += LocalFreight[0];
                                                Advance += LocalFreight[1];
                                            }
                                        }
                                        else
                                        {
                                            var LocalFreight = CatchLocalFreightAdvance("VCategory", VehicleCategory, ToBranch, CurrentDate, FMno);
                                            Freight += LocalFreight[0];
                                            Advance += LocalFreight[1];
                                        }
                                    }
                                    else
                                    {
                                        var LocalFreight = CatchLocalFreightAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, FMno);
                                        Freight += LocalFreight[0];
                                        Advance += LocalFreight[1];
                                    }
                                }
                            }
                        }

                        #endregion

                        #region Get ViaFreight && ViaAdvance

                        if (!String.IsNullOrEmpty(RouteVia))
                        {
                            var RouteList = RouteVia.Split(',');

                            foreach (var item in RouteList)
                            {
                                var ViaFreightlist = CatchViaFreightAdvance("VNo", Vehicle, RouteVia, CurrentDate, FMno);
                                ViaFreight = ViaFreightlist[0];
                                ViaAdvance = ViaFreightlist[1];
                                if (ViaFreight == 0 && ViaAdvance == 0)
                                {
                                    ViaFreightlist = CatchViaFreightAdvance("VCategory", VehicleCategory, RouteVia, CurrentDate, FMno);
                                    ViaFreight = ViaFreightlist[0];
                                    ViaAdvance = ViaFreightlist[1];
                                }
                                Freight += ViaFreight;
                                Advance += ViaAdvance;
                            }
                        }

                        #endregion

                        #endregion

                    }
                    if (vehicleMaster.PickDriverTripRate)
                    {
                        #region TRIP CHARGES
                        //Catch Freight,LocalFreight,Advance,LocalAdvance Separately
                        #region Freight && Advance

                        if (RateType.Contains("V"))
                        {
                            //From TO
                            var FreightAdvance = CatchTripFreightAdvance("VNo", Vehicle, FromBranch, CurrentDate, ToBranch, FMno);
                            TripFreight = FreightAdvance[0];
                            TripAdvance = FreightAdvance[1];
                            if (FreightAdvance[0] == 0 && FreightAdvance[1] == 0)
                            {
                                //From-Parent => TO
                                FreightAdvance = CatchTripFreightAdvance("VNo", Vehicle, tfatBranchFrom.Code, CurrentDate, ToBranch, FMno);
                                TripFreight = FreightAdvance[0];
                                TripAdvance = FreightAdvance[1];
                                if (FreightAdvance[0] == 0 && FreightAdvance[1] == 0)
                                {
                                    //From => TO-Parent
                                    FreightAdvance = CatchTripFreightAdvance("VNo", Vehicle, FromBranch, CurrentDate, tfatBranchTo.Code, FMno);
                                    TripFreight = FreightAdvance[0];
                                    TripAdvance = FreightAdvance[1];
                                    if (FreightAdvance[0] == 0 && FreightAdvance[1] == 0)
                                    {
                                        //From-Parent => TO-Parent
                                        FreightAdvance = CatchTripFreightAdvance("VNo", Vehicle, FromBranch, CurrentDate, tfatBranchTo.Code, FMno);
                                        TripFreight = FreightAdvance[0];
                                        TripAdvance = FreightAdvance[1];
                                        if (FreightAdvance[0] == 0 && FreightAdvance[1] == 0)
                                        {
                                            //Category Check
                                            if (RateType.Contains("C"))
                                            {
                                                //From TO
                                                FreightAdvance = CatchTripFreightAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, ToBranch, FMno);
                                                TripFreight = FreightAdvance[0];
                                                TripAdvance = FreightAdvance[1];
                                                if (FreightAdvance[0] == 0 && FreightAdvance[1] == 0)
                                                {
                                                    //From-Parent => TO
                                                    FreightAdvance = CatchTripFreightAdvance("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, ToBranch, FMno);
                                                    TripFreight = FreightAdvance[0];
                                                    TripAdvance = FreightAdvance[1];
                                                    if (FreightAdvance[0] == 0 && FreightAdvance[1] == 0)
                                                    {
                                                        //From => TO-Parent
                                                        FreightAdvance = CatchTripFreightAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, tfatBranchTo.Code, FMno);
                                                        TripFreight = FreightAdvance[0];
                                                        TripAdvance = FreightAdvance[1];
                                                        if (FreightAdvance[0] == 0 && FreightAdvance[1] == 0)
                                                        {
                                                            //From-Parent => TO-Parent
                                                            FreightAdvance = CatchTripFreightAdvance("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, tfatBranchTo.Code, FMno);
                                                            TripFreight = FreightAdvance[0];
                                                            TripAdvance = FreightAdvance[1];
                                                            if (FromBranch == ToBranch)
                                                            {
                                                                var LocalFreight = CatchTripLocalFreightAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, FMno);
                                                                TripFreight += LocalFreight[0];
                                                                TripAdvance += LocalFreight[1];
                                                            }
                                                            else
                                                            {
                                                                var LocalFreight = CatchTripLocalFreightAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, FMno);
                                                                TripFreight += LocalFreight[0];
                                                                TripAdvance += LocalFreight[1];
                                                                LocalFreight = CatchTripLocalFreightAdvance("VCategory", VehicleCategory, ToBranch, CurrentDate, FMno);
                                                                TripFreight += LocalFreight[0];
                                                                TripAdvance += LocalFreight[1];
                                                            }
                                                        }
                                                        else
                                                        {
                                                            var LocalFreight = CatchTripLocalFreightAdvance("VCategory", VehicleCategory, ToBranch, CurrentDate, FMno);
                                                            TripFreight += LocalFreight[0];
                                                            TripAdvance += LocalFreight[1];
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var LocalFreight = CatchTripLocalFreightAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, FMno);
                                                        TripFreight += LocalFreight[0];
                                                        TripAdvance += LocalFreight[1];
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (FromBranch == ToBranch)
                                                {
                                                    var LocalFreight = CatchTripLocalFreightAdvance("VNo", Vehicle, FromBranch, CurrentDate, FMno);
                                                    TripFreight += LocalFreight[0];
                                                    TripAdvance += LocalFreight[1];
                                                }
                                                else
                                                {
                                                    var LocalFreight = CatchTripLocalFreightAdvance("VNo", Vehicle, FromBranch, CurrentDate, FMno);
                                                    TripFreight += LocalFreight[0];
                                                    TripAdvance += LocalFreight[1];
                                                    LocalFreight = CatchTripLocalFreightAdvance("VNo", Vehicle, ToBranch, CurrentDate, FMno);
                                                    TripFreight += LocalFreight[0];
                                                    TripAdvance += LocalFreight[1];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (FromBranch == ToBranch)
                                            {
                                                var LocalFreight = CatchTripLocalFreightAdvance("VNo", Vehicle, FromBranch, CurrentDate, FMno);
                                                TripFreight += LocalFreight[0];
                                                TripAdvance += LocalFreight[1];
                                            }
                                            else
                                            {
                                                var LocalFreight = CatchTripLocalFreightAdvance("VNo", Vehicle, FromBranch, CurrentDate, FMno);
                                                TripFreight += LocalFreight[0];
                                                TripAdvance += LocalFreight[1];
                                                LocalFreight = CatchTripLocalFreightAdvance("VNo", Vehicle, ToBranch, CurrentDate, FMno);
                                                TripFreight += LocalFreight[0];
                                                TripAdvance += LocalFreight[1];
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var LocalFreight = CatchTripLocalFreightAdvance("VNo", Vehicle, ToBranch, CurrentDate, FMno);
                                        TripFreight += LocalFreight[0];
                                        TripAdvance += LocalFreight[1];
                                    }
                                }
                                else
                                {
                                    var LocalFreight = CatchTripLocalFreightAdvance("VNo", Vehicle, FromBranch, CurrentDate, FMno);
                                    TripFreight += LocalFreight[0];
                                    TripAdvance += LocalFreight[1];
                                }
                            }
                        }
                        else
                        {
                            if (RateType.Contains("C"))
                            {
                                //From TO
                                var FreightAdvance = CatchTripFreightAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, ToBranch, FMno);
                                TripFreight = FreightAdvance[0];
                                TripAdvance = FreightAdvance[1];
                                if (FreightAdvance[0] == 0 && FreightAdvance[1] == 0)
                                {
                                    //From-Parent => TO
                                    FreightAdvance = CatchTripFreightAdvance("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, ToBranch, FMno);
                                    TripFreight = FreightAdvance[0];
                                    TripAdvance = FreightAdvance[1];
                                    if (FreightAdvance[0] == 0 && FreightAdvance[1] == 0)
                                    {
                                        //From => TO-Parent
                                        FreightAdvance = CatchTripFreightAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, tfatBranchTo.Code, FMno);
                                        TripFreight = FreightAdvance[0];
                                        TripAdvance = FreightAdvance[1];
                                        if (FreightAdvance[0] == 0 && FreightAdvance[1] == 0)
                                        {
                                            //From-Parent => TO-Parent
                                            FreightAdvance = CatchTripFreightAdvance("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, tfatBranchTo.Code, FMno);
                                            TripFreight = FreightAdvance[0];
                                            TripAdvance = FreightAdvance[1];
                                            if (FromBranch == ToBranch)
                                            {
                                                var LocalFreight = CatchTripLocalFreightAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, FMno);
                                                TripFreight += LocalFreight[0];
                                                TripAdvance += LocalFreight[1];
                                            }
                                            else
                                            {
                                                var LocalFreight = CatchTripLocalFreightAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, FMno);
                                                TripFreight += LocalFreight[0];
                                                TripAdvance += LocalFreight[1];
                                                LocalFreight = CatchTripLocalFreightAdvance("VCategory", VehicleCategory, ToBranch, CurrentDate, FMno);
                                                TripFreight += LocalFreight[0];
                                                TripAdvance += LocalFreight[1];
                                            }
                                        }
                                        else
                                        {
                                            var LocalFreight = CatchTripLocalFreightAdvance("VCategory", VehicleCategory, ToBranch, CurrentDate, FMno);
                                            TripFreight += LocalFreight[0];
                                            TripAdvance += LocalFreight[1];
                                        }
                                    }
                                    else
                                    {
                                        var LocalFreight = CatchTripLocalFreightAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, FMno);
                                        TripFreight += LocalFreight[0];
                                        TripAdvance += LocalFreight[1];
                                    }
                                }
                            }
                        }

                        #endregion

                        #region Get TripViaFreight && TripViaAdvance

                        if (!String.IsNullOrEmpty(RouteVia))
                        {
                            var RouteList = RouteVia.Split(',');

                            foreach (var item in RouteList)
                            {
                                var ViaFreightlist = CatchTripViaFreightAdvance("VNo", Vehicle, RouteVia, CurrentDate, FMno);
                                TripViaFreight = ViaFreightlist[0];
                                TripViaAdvance = ViaFreightlist[1];
                                if (TripViaFreight == 0 && TripViaAdvance == 0)
                                {
                                    ViaFreightlist = CatchTripViaFreightAdvance("VCategory", VehicleCategory, RouteVia, CurrentDate, FMno);
                                    TripViaFreight = ViaFreightlist[0];
                                    TripViaAdvance = ViaFreightlist[1];
                                }
                                TripFreight += TripViaFreight;
                                TripAdvance += TripViaAdvance;
                            }
                        }

                        #endregion

                        #endregion
                    }





                    Session["FMFreightAdvance"] = fmCatchCharges;
                    Session["TripFMFreightAdvance"] = TripfmCatchCharges;

                    //var html = ViewHelper.RenderPartialView(this, "FMChargesDescription", fmCatchCharges);
                    return Json(new
                    {
                        Status = "Success",
                        Freight = Freight > 0 ? Freight : 0,
                        Advance = Advance > 0 ? Advance : 0,
                        TripFreight = TripFreight > 0 ? TripFreight : 0,
                        TripAdvance = TripAdvance > 0 ? TripAdvance : 0,
                    }, JsonRequestBehavior.AllowGet);
                }

            }

            return Json(new
            {
                Status = "Error",
                Freight = 0,
                Advance = 0,
                TripFreight = 0,
                TripAdvance = 0,
            },
                JsonRequestBehavior.AllowGet
            );
        }

        public decimal CatchFreight(string VehicleType, string Vehicle, string FromBranch, DateTime CurrentDate, string ToBranch, int FMNO)
        {

            //int FMNO=Convert.ToInt32(sFMNO)
            decimal Freight = 0;
            FreightChargeMa freightCharge = ctxTFAT.FreightChargeMa.Where(x => x.VehicleType == VehicleType && x.Vehicle == Vehicle.Trim() && x.FromBranch == FromBranch && x.FromPeriod <= CurrentDate).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
            if (freightCharge != null)
            {
                FreightChargeMaRef freightChargeMaRef = ctxTFAT.FreightChargeMaRef.Where(x => x.DocNo == freightCharge.DocNo && x.FromBranch == FromBranch && x.ToBranch == ToBranch).FirstOrDefault();
                if (freightChargeMaRef != null)
                {
                    Freight = freightChargeMaRef.Freight.Value;
                    FmCatchChargesInfo chargesInfo = new FmCatchChargesInfo
                    {
                        Fmno = FMNO,
                        MenuHeader = "FreightChargeMaster",
                        Sno = fmCatchCharges.Count() + 1,
                        FromBranch = FromBranch,
                        ToBranch = ToBranch,
                        Category = VehicleType,
                        Type = "F",
                        Amt = freightChargeMaRef.Freight.Value
                    };
                    fmCatchCharges.Add(chargesInfo);
                }
            }
            return Freight;
        }

        public decimal CatchLocalFreight(string VehicleType, string Vehicle, string FromBranch, DateTime CurrentDate, int FMNO)
        {
            decimal Freight = 0;
            FreightLocalChargesMa freightCharge = ctxTFAT.FreightLocalChargesMa.Where(x => x.VehicleType == VehicleType && x.Vehicle == Vehicle.Trim() && x.FromPeriod <= CurrentDate).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
            if (freightCharge != null)
            {
                FreightLocalChargesMaRef freightChargeMaRef = ctxTFAT.FreightLocalChargesMaRef.Where(x => x.DocNo == freightCharge.DocNo && x.Area == FromBranch).FirstOrDefault();
                if (freightChargeMaRef != null)
                {
                    Freight = freightChargeMaRef.Freight.Value;
                    FmCatchChargesInfo chargesInfo = new FmCatchChargesInfo
                    {
                        Fmno = FMNO,
                        MenuHeader = "FreightLocalCharges",
                        Sno = fmCatchCharges.Count() + 1,
                        FromBranch = FromBranch,
                        Category = VehicleType,
                        Type = "LF",
                        Amt = freightChargeMaRef.Freight.Value
                    };
                    fmCatchCharges.Add(chargesInfo);
                }
            }
            return Freight;
        }

        public decimal CatchViaFreight(string VehicleType, string Vehicle, string RouteVia, DateTime CurrentDate, int FMNO)
        {
            //var ListRouteVia = RouteVia.Split(',');
            decimal ViaFreight = 0;
            ViaFreightMa viaFreight = ctxTFAT.ViaFreightMa.Where(x => x.VehicleType == VehicleType && x.Vehicle == Vehicle.Trim() && x.FromPeriod <= CurrentDate).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
            if (viaFreight != null)
            {
                ViaFreightMaRef viaFreightMaRef = ctxTFAT.ViaFreightMaRef.Where(x => x.DocNo == viaFreight.DocNo && x.Area == RouteVia).FirstOrDefault();
                if (viaFreightMaRef != null)
                {
                    ViaFreight += viaFreightMaRef.ViaFreight.Value;
                    FmCatchChargesInfo chargesInfo = new FmCatchChargesInfo
                    {
                        Fmno = FMNO,
                        MenuHeader = "ViaFreightMaster",
                        Sno = fmCatchCharges.Count() + 1,
                        FromBranch = RouteVia,
                        Category = VehicleType,
                        Type = "VF",
                        Amt = viaFreightMaRef.ViaFreight.Value
                    };
                    fmCatchCharges.Add(chargesInfo);
                }

            }
            return ViaFreight;
        }

        public decimal CatchAdvance(string VehicleType, string Vehicle, string FromBranch, DateTime CurrentDate, string ToBranch, int FMNO)
        {
            decimal Freight = 0;
            TripChargesMa freightCharge = ctxTFAT.TripChargesMa.Where(x => x.VehicleType == VehicleType && x.Vehicle == Vehicle.Trim() && x.FromBranch == FromBranch && x.FromPeriod <= CurrentDate).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
            if (freightCharge != null)
            {
                TripChargesMaRef freightChargeMaRef = ctxTFAT.TripChargesMaRef.Where(x => x.DocNo == freightCharge.DocNo && x.FromBranch == FromBranch && x.ToBranch == ToBranch).FirstOrDefault();
                if (freightChargeMaRef != null)
                {
                    Freight = freightChargeMaRef.TripAdvance.Value;
                    FmCatchChargesInfo chargesInfo = new FmCatchChargesInfo
                    {
                        Fmno = FMNO,
                        MenuHeader = "TripChargesMaster",
                        Sno = fmCatchCharges.Count() + 1,
                        FromBranch = FromBranch,
                        ToBranch = ToBranch,
                        Category = VehicleType,
                        Type = "A",
                        Amt1 = freightChargeMaRef.TripAdvance.Value
                    };
                    fmCatchCharges.Add(chargesInfo);
                }
            }
            return Freight;
        }

        public decimal CatchLocalAdvance(string VehicleType, string Vehicle, string FromBranch, DateTime CurrentDate, int FMNO)
        {
            decimal Freight = 0;
            LocalChargesMa freightCharge = ctxTFAT.LocalChargesMa.Where(x => x.VehicleType == VehicleType && x.Vehicle == Vehicle.Trim() && x.FromPeriod <= CurrentDate).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
            if (freightCharge != null)
            {
                LocalChargesMaRef freightChargeMaRef = ctxTFAT.LocalChargesMaRef.Where(x => x.DocNo == freightCharge.DocNo && x.Area == FromBranch).FirstOrDefault();
                if (freightChargeMaRef != null)
                {
                    Freight = freightChargeMaRef.LocalAdvance.Value;
                    FmCatchChargesInfo chargesInfo = new FmCatchChargesInfo
                    {
                        Fmno = FMNO,
                        MenuHeader = "LocalChargesMaster",
                        Sno = fmCatchCharges.Count() + 1,
                        FromBranch = FromBranch,
                        Category = VehicleType,
                        Type = "LA",
                        Amt1 = freightChargeMaRef.LocalAdvance.Value
                    };
                    fmCatchCharges.Add(chargesInfo);
                }
            }
            return Freight;
        }

        public decimal CatchViaAdvance(string VehicleType, string Vehicle, string RouteVia, DateTime CurrentDate, int FMNO)
        {
            //var ListRouteVia = RouteVia.Split(',');
            decimal ViaAdvance = 0;
            ViaChargesMa freightCharge = ctxTFAT.ViaChargesMa.Where(x => x.VehicleType == VehicleType && x.Vehicle == Vehicle.Trim() && x.FromPeriod <= CurrentDate).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
            if (freightCharge != null)
            {
                ViaChargesMaRef freightChargeMaRef = ctxTFAT.ViaChargesMaRef.Where(x => x.DocNo == freightCharge.DocNo && x.Area == RouteVia).FirstOrDefault();
                if (freightChargeMaRef != null)
                {
                    ViaAdvance += freightChargeMaRef.ViaAdvance.Value;
                    FmCatchChargesInfo chargesInfo = new FmCatchChargesInfo
                    {
                        Fmno = FMNO,
                        MenuHeader = "ViaChargesMaster",
                        Sno = fmCatchCharges.Count() + 1,
                        FromBranch = RouteVia,
                        Category = VehicleType,
                        Type = "VA",
                        Amt1 = freightChargeMaRef.ViaAdvance.Value
                    };
                    fmCatchCharges.Add(chargesInfo);
                }

            }
            return ViaAdvance;
        }

        public List<decimal> CatchFreightAdvance(string VehicleType, string Vehicle, string FromBranch, DateTime CurrentDate, string ToBranch, int FMNO)
        {
            List<decimal> FreightAdvance = new List<decimal>();
            FreightChargeMa freightCharge = ctxTFAT.FreightChargeMa.Where(x => x.VehicleType == VehicleType && x.Vehicle == Vehicle.Trim() && x.FromBranch == FromBranch && x.FromPeriod <= CurrentDate).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
            if (freightCharge != null)
            {
                FreightChargeMaRef freightChargeMaRef = ctxTFAT.FreightChargeMaRef.Where(x => x.DocNo == freightCharge.DocNo && x.FromBranch == FromBranch && x.ToBranch == ToBranch).FirstOrDefault();
                if (freightChargeMaRef != null)
                {
                    FreightAdvance.Add(freightChargeMaRef.Freight ?? 0);
                    FreightAdvance.Add(freightChargeMaRef.Advance ?? 0);
                    FmCatchChargesInfo chargesInfo = new FmCatchChargesInfo
                    {
                        Fmno = FMNO,
                        MenuHeader = "FreightChargeMaster",
                        Sno = fmCatchCharges.Count() + 1,
                        FromBranch = FromBranch,
                        ToBranch = ToBranch,
                        Category = VehicleType,
                        Type = "FA",
                        Amt = freightChargeMaRef.Freight.Value,
                        Amt1 = freightChargeMaRef.Advance.Value
                    };
                    fmCatchCharges.Add(chargesInfo);
                }
                else
                {
                    FreightAdvance.Add(0);
                    FreightAdvance.Add(0);
                }
            }
            else
            {
                FreightAdvance.Add(0);
                FreightAdvance.Add(0);
            }
            return FreightAdvance;
        }

        public List<decimal> CatchLocalFreightAdvance(string VehicleType, string Vehicle, string FromBranch, DateTime CurrentDate, int FMNO)
        {
            List<decimal> LocalFreightAdvance = new List<decimal>();
            FreightLocalChargesMa freightCharge = ctxTFAT.FreightLocalChargesMa.Where(x => x.VehicleType == VehicleType && x.Vehicle == Vehicle.Trim() && x.FromPeriod <= CurrentDate).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
            if (freightCharge != null)
            {
                FreightLocalChargesMaRef freightChargeMaRef = ctxTFAT.FreightLocalChargesMaRef.Where(x => x.DocNo == freightCharge.DocNo && x.Area == FromBranch).FirstOrDefault();
                if (freightChargeMaRef != null)
                {
                    LocalFreightAdvance.Add(freightChargeMaRef.Freight ?? 0);
                    LocalFreightAdvance.Add(freightChargeMaRef.Advance ?? 0);
                    FmCatchChargesInfo chargesInfo = new FmCatchChargesInfo
                    {
                        Fmno = FMNO,
                        MenuHeader = "FreightLocalCharges",
                        Sno = fmCatchCharges.Count() + 1,
                        FromBranch = FromBranch,
                        Category = VehicleType,
                        Type = "LFA",
                        Amt = freightChargeMaRef.Freight.Value,
                        Amt1 = freightChargeMaRef.Advance.Value
                    };
                    fmCatchCharges.Add(chargesInfo);
                }
                else
                {
                    LocalFreightAdvance.Add(0);
                    LocalFreightAdvance.Add(0);
                }
            }
            else
            {
                LocalFreightAdvance.Add(0);
                LocalFreightAdvance.Add(0);
            }
            return LocalFreightAdvance;
        }

        public List<decimal> CatchViaFreightAdvance(string VehicleType, string Vehicle, string RouteVia, DateTime CurrentDate, int FMNO)
        {
            //var ListRouteVia = RouteVia.Split(',');
            List<decimal> ViaFreight_Advance = new List<decimal>();

            ViaFreightMa viaFreight = ctxTFAT.ViaFreightMa.Where(x => x.VehicleType == VehicleType && x.Vehicle == Vehicle.Trim() && x.FromPeriod <= CurrentDate).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
            if (viaFreight != null)
            {
                ViaFreightMaRef viaFreightMaRef = ctxTFAT.ViaFreightMaRef.Where(x => x.DocNo == viaFreight.DocNo && x.Area == RouteVia).FirstOrDefault();
                if (viaFreightMaRef != null)
                {
                    ViaFreight_Advance.Add(viaFreightMaRef.ViaFreight ?? 0);
                    ViaFreight_Advance.Add(viaFreightMaRef.ViaAdvance ?? 0);
                    FmCatchChargesInfo chargesInfo = new FmCatchChargesInfo
                    {
                        Fmno = FMNO,
                        MenuHeader = "ViaFreightMaster",
                        Sno = fmCatchCharges.Count() + 1,
                        FromBranch = RouteVia,
                        Category = VehicleType,
                        Type = "VFA",
                        Amt = viaFreightMaRef.ViaFreight.Value,
                        Amt1 = viaFreightMaRef.ViaAdvance.Value
                    };
                    fmCatchCharges.Add(chargesInfo);
                }
                else
                {
                    ViaFreight_Advance.Add(0);
                    ViaFreight_Advance.Add(0);
                }

            }
            else
            {
                ViaFreight_Advance.Add(0);
                ViaFreight_Advance.Add(0);
            }

            return ViaFreight_Advance;
        }

        public List<decimal> CatchTripFreightAdvance(string VehicleType, string Vehicle, string FromBranch, DateTime CurrentDate, string ToBranch, int FMNO)
        {
            List<decimal> FreightAdvance = new List<decimal>();
            TripChargesMa freightCharge = ctxTFAT.TripChargesMa.Where(x => x.VehicleType == VehicleType && x.Vehicle == Vehicle.Trim() && x.FromBranch == FromBranch && x.FromPeriod <= CurrentDate).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
            if (freightCharge != null)
            {
                TripChargesMaRef freightChargeMaRef = ctxTFAT.TripChargesMaRef.Where(x => x.DocNo == freightCharge.DocNo && x.FromBranch == FromBranch && x.ToBranch == ToBranch).FirstOrDefault();
                if (freightChargeMaRef != null)
                {
                    FreightAdvance.Add(freightChargeMaRef.TripCharge ?? 0);
                    FreightAdvance.Add(freightChargeMaRef.TripAdvance ?? 0);
                    FmCatchChargesInfo chargesInfo = new FmCatchChargesInfo
                    {
                        Fmno = FMNO,
                        MenuHeader = "Driver Trip Charges Master",
                        Sno = fmCatchCharges.Count() + 1,
                        FromBranch = FromBranch,
                        ToBranch = ToBranch,
                        Category = VehicleType,
                        Type = "TFA",
                        Amt = freightChargeMaRef.TripCharge.Value,
                        Amt1 = freightChargeMaRef.TripAdvance.Value
                    };
                    TripfmCatchCharges.Add(chargesInfo);
                }
                else
                {
                    FreightAdvance.Add(0);
                    FreightAdvance.Add(0);
                }
            }
            else
            {
                FreightAdvance.Add(0);
                FreightAdvance.Add(0);
            }
            return FreightAdvance;
        }

        public List<decimal> CatchTripLocalFreightAdvance(string VehicleType, string Vehicle, string FromBranch, DateTime CurrentDate, int FMNO)
        {
            List<decimal> LocalFreightAdvance = new List<decimal>();
            LocalChargesMa freightCharge = ctxTFAT.LocalChargesMa.Where(x => x.VehicleType == VehicleType && x.Vehicle == Vehicle.Trim() && x.FromPeriod <= CurrentDate).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
            if (freightCharge != null)
            {
                LocalChargesMaRef freightChargeMaRef = ctxTFAT.LocalChargesMaRef.Where(x => x.DocNo == freightCharge.DocNo && x.Area == FromBranch).FirstOrDefault();
                if (freightChargeMaRef != null)
                {
                    LocalFreightAdvance.Add(freightChargeMaRef.LocalCharges ?? 0);
                    LocalFreightAdvance.Add(freightChargeMaRef.LocalAdvance ?? 0);
                    FmCatchChargesInfo chargesInfo = new FmCatchChargesInfo
                    {
                        Fmno = FMNO,
                        MenuHeader = "Driver Trip Local Charges",
                        Sno = fmCatchCharges.Count() + 1,
                        FromBranch = FromBranch,
                        Category = VehicleType,
                        Type = "TLFA",
                        Amt = freightChargeMaRef.LocalCharges.Value,
                        Amt1 = freightChargeMaRef.LocalAdvance.Value
                    };
                    TripfmCatchCharges.Add(chargesInfo);
                }
                else
                {
                    LocalFreightAdvance.Add(0);
                    LocalFreightAdvance.Add(0);
                }
            }
            else
            {
                LocalFreightAdvance.Add(0);
                LocalFreightAdvance.Add(0);
            }
            return LocalFreightAdvance;
        }

        public List<decimal> CatchTripViaFreightAdvance(string VehicleType, string Vehicle, string RouteVia, DateTime CurrentDate, int FMNO)
        {
            //var ListRouteVia = RouteVia.Split(',');
            List<decimal> ViaFreight_Advance = new List<decimal>();

            ViaChargesMa viaFreight = ctxTFAT.ViaChargesMa.Where(x => x.VehicleType == VehicleType && x.Vehicle == Vehicle.Trim() && x.FromPeriod <= CurrentDate).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
            if (viaFreight != null)
            {
                ViaChargesMaRef viaFreightMaRef = ctxTFAT.ViaChargesMaRef.Where(x => x.DocNo == viaFreight.DocNo && x.Area == RouteVia).FirstOrDefault();
                if (viaFreightMaRef != null)
                {
                    ViaFreight_Advance.Add(viaFreightMaRef.ViaCharges ?? 0);
                    ViaFreight_Advance.Add(viaFreightMaRef.ViaAdvance ?? 0);
                    FmCatchChargesInfo chargesInfo = new FmCatchChargesInfo
                    {
                        Fmno = FMNO,
                        MenuHeader = "Driver Via Charges Master",
                        Sno = fmCatchCharges.Count() + 1,
                        FromBranch = RouteVia,
                        Category = VehicleType,
                        Type = "TVFA",
                        Amt = viaFreightMaRef.ViaCharges.Value,
                        Amt1 = viaFreightMaRef.ViaAdvance.Value
                    };
                    TripfmCatchCharges.Add(chargesInfo);
                }
                else
                {
                    ViaFreight_Advance.Add(0);
                    ViaFreight_Advance.Add(0);
                }

            }
            else
            {
                ViaFreight_Advance.Add(0);
                ViaFreight_Advance.Add(0);
            }

            return ViaFreight_Advance;
        }



        #endregion

        #region Old Fetching Freight Advance And ViaCharges

        public ActionResult FetchFreight_Advance(string FromBranch, string RouteVia, string ToBranch, string Vehicle, string VehicleCategory, string FMDate)
        {
            decimal Freight = 0, Advance = 0;

            if (!(String.IsNullOrEmpty(FMDate) || String.IsNullOrEmpty(Vehicle) || String.IsNullOrEmpty(VehicleCategory)))
            {
                VehicleMaster vehicleMaster = ctxTFAT.VehicleMaster.Where(x => x.Code.Trim() == Vehicle.Trim()).FirstOrDefault();
                if (vehicleMaster != null)
                {


                    DateTime CurrentDate = ConvertDDMMYYTOYYMMDD(FMDate);
                    TfatBranch tfatBranchFrom = new TfatBranch();
                    TfatBranch tfatBranchTo = new TfatBranch();
                    bool CheckParent = false;
                    if (vehicleMaster != null)
                    {
                        CheckParent = vehicleMaster.GetParentRateAlso;
                    }
                    if (CheckParent)
                    {
                        tfatBranchFrom = ctxTFAT.TfatBranch.Where(x => x.Code == FromBranch).FirstOrDefault();
                        if (tfatBranchFrom.Category == "Area" || tfatBranchFrom.Category == "SubBranch")
                        {
                            tfatBranchFrom = ctxTFAT.TfatBranch.Where(x => x.Code == tfatBranchFrom.Grp).FirstOrDefault();
                        }
                        tfatBranchTo = ctxTFAT.TfatBranch.Where(x => x.Code == ToBranch).FirstOrDefault();
                        if (tfatBranchTo.Category == "Area" || tfatBranchTo.Category == "SubBranch")
                        {
                            tfatBranchTo = ctxTFAT.TfatBranch.Where(x => x.Code == tfatBranchTo.Grp).FirstOrDefault();
                        }
                    }
                    bool GetLocalAdvance = true;

                    #region Get Freight
                    if (!(String.IsNullOrEmpty(FromBranch) && String.IsNullOrEmpty(ToBranch)))
                    {
                        var RateType = vehicleMaster.RateType.Split(',').ToList();
                        if (RateType.Contains("V"))
                        {
                            //From TO
                            Freight = FetchFreight("VNo", Vehicle, FromBranch, CurrentDate, ToBranch);
                            if (Freight == 0)
                            {
                                if (CheckParent)
                                {
                                    //From To-Parent
                                    Freight = FetchFreight("VNo", Vehicle, FromBranch, CurrentDate, tfatBranchTo.Code);
                                    if (Freight == 0)
                                    {
                                        //From-Parent TO
                                        Freight = FetchFreight("VNo", Vehicle, tfatBranchFrom.Code, CurrentDate, ToBranch);
                                        if (Freight == 0)
                                        {
                                            //From-Parent To-Parent
                                            Freight = FetchFreight("VNo", Vehicle, tfatBranchFrom.Code, CurrentDate, tfatBranchTo.Code);
                                            if (Freight == 0)
                                            {
                                                //Check Category
                                                if (RateType.Contains("C"))
                                                {
                                                    //From To
                                                    Freight = FetchFreight("VCategory", VehicleCategory, FromBranch, CurrentDate, ToBranch);
                                                    if (Freight == 0)
                                                    {
                                                        //From To-Parent
                                                        Freight = FetchFreight("VCategory", VehicleCategory, FromBranch, CurrentDate, tfatBranchTo.Code);
                                                        if (Freight == 0)
                                                        {
                                                            //From-Parent TO
                                                            Freight = FetchFreight("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, ToBranch);
                                                            if (Freight == 0)
                                                            {
                                                                //From-Parent To-Parent
                                                                Freight = FetchFreight("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, tfatBranchTo.Code);
                                                            }
                                                        }
                                                    }

                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //Check Category
                                    if (RateType.Contains("C"))
                                    {
                                        //From To
                                        Freight = FetchFreight("VCategory", VehicleCategory, FromBranch, CurrentDate, ToBranch);
                                        if (Freight == 0)
                                        {
                                            if (CheckParent)
                                            {
                                                //From To-Parent
                                                Freight = FetchFreight("VCategory", VehicleCategory, FromBranch, CurrentDate, tfatBranchTo.Code);
                                                if (Freight == 0)
                                                {
                                                    //From-Parent TO
                                                    Freight = FetchFreight("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, ToBranch);
                                                    if (Freight == 0)
                                                    {
                                                        //From-Parent To-Parent
                                                        Freight = FetchFreight("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, tfatBranchTo.Code);
                                                    }
                                                }
                                            }
                                        }

                                    }
                                }
                            }
                        }
                        else
                        {
                            //Check Category
                            if (RateType.Contains("C"))
                            {
                                //From To
                                Freight = FetchFreight("VCategory", VehicleCategory, FromBranch, CurrentDate, ToBranch);
                                if (Freight == 0)
                                {
                                    if (CheckParent)
                                    {
                                        //From To-Parent
                                        Freight = FetchFreight("VCategory", VehicleCategory, FromBranch, CurrentDate, tfatBranchTo.Code);
                                        if (Freight == 0)
                                        {
                                            //From-Parent TO
                                            Freight = FetchFreight("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, ToBranch);
                                            if (Freight == 0)
                                            {
                                                //From-Parent To-Parent
                                                Freight = FetchFreight("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, tfatBranchTo.Code);
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }
                    #endregion

                    #region Get Advance
                    if (!(String.IsNullOrEmpty(FromBranch) && String.IsNullOrEmpty(ToBranch)))
                    {
                        var RateType = vehicleMaster.RateType.Split(',').ToList();
                        if (RateType.Contains("V"))
                        {
                            //From TO
                            Advance = FetchAdvance("VNo", Vehicle, FromBranch, CurrentDate, ToBranch);
                            if (Advance == 0)
                            {
                                if (CheckParent)
                                {
                                    //From To-Parent
                                    Advance = FetchAdvance("VNo", Vehicle, FromBranch, CurrentDate, tfatBranchTo.Code);
                                    if (Advance == 0)
                                    {
                                        //From-Parent TO
                                        Advance = FetchAdvance("VNo", Vehicle, tfatBranchFrom.Code, CurrentDate, ToBranch);
                                        if (Advance == 0)
                                        {
                                            //From-Parent To-Parent
                                            Advance = FetchAdvance("VNo", Vehicle, tfatBranchFrom.Code, CurrentDate, tfatBranchTo.Code);
                                            if (Advance == 0)
                                            {
                                                //Check Category
                                                if (RateType.Contains("C"))
                                                {
                                                    //From To
                                                    Advance = FetchAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, ToBranch);
                                                    if (Advance == 0)
                                                    {
                                                        //From To-Parent
                                                        Advance = FetchAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, tfatBranchTo.Code);
                                                        if (Advance == 0)
                                                        {
                                                            //From-Parent TO
                                                            Advance = FetchAdvance("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, ToBranch);
                                                            if (Advance == 0)
                                                            {
                                                                //From-Parent To-Parent
                                                                Advance = FetchAdvance("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, tfatBranchTo.Code);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        GetLocalAdvance = false;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //Check Category
                                    if (RateType.Contains("C"))
                                    {
                                        //From To
                                        Advance = FetchAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, ToBranch);
                                        if (Advance == 0)
                                        {
                                            if (CheckParent)
                                            {
                                                //From To-Parent
                                                Advance = FetchAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, tfatBranchTo.Code);
                                                if (Advance == 0)
                                                {
                                                    //From-Parent TO
                                                    Advance = FetchAdvance("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, ToBranch);
                                                    if (Advance == 0)
                                                    {
                                                        //From-Parent To-Parent
                                                        Advance = FetchAdvance("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, tfatBranchTo.Code);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            GetLocalAdvance = false;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                GetLocalAdvance = false;
                            }
                        }
                        else
                        {
                            //Check Category
                            if (RateType.Contains("C"))
                            {
                                //From To
                                Advance = FetchAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, ToBranch);
                                if (Advance == 0)
                                {
                                    if (CheckParent)
                                    {
                                        //From To-Parent
                                        Advance = FetchAdvance("VCategory", VehicleCategory, FromBranch, CurrentDate, tfatBranchTo.Code);
                                        if (Advance == 0)
                                        {
                                            //From-Parent TO
                                            Advance = FetchAdvance("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, ToBranch);
                                            if (Advance == 0)
                                            {
                                                //From-Parent To-Parent
                                                Advance = FetchAdvance("VCategory", VehicleCategory, tfatBranchFrom.Code, CurrentDate, tfatBranchTo.Code);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    GetLocalAdvance = false;
                                }

                            }
                        }
                    }
                    #endregion

                    if (!String.IsNullOrEmpty(RouteVia))
                    {
                        var ViaRouteList = RouteVia.Split(',').ToList();
                        ViaAndLocalExpMas viaAndLocal = ctxTFAT.ViaAndLocalExpMas.Where(x => x.FromPeriod <= CurrentDate && x.VehicleType == "VNo" && x.Vehicle == Vehicle).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
                        if (viaAndLocal != null)
                        {
                            var GetViaAdvance = ctxTFAT.ViaAndLocalExpMasRef.Where(x => x.DocNo == viaAndLocal.DocNo && ViaRouteList.Contains(x.Area)).Select(x => x.ViaAdvance.Value).ToList();
                            if (GetViaAdvance.Sum() != 0)
                            {
                                Advance += GetViaAdvance.Sum();
                            }
                            else
                            {
                                var RateType = vehicleMaster.RateType.Split(',').ToList();
                                if (RateType.Contains("C"))
                                {
                                    viaAndLocal = ctxTFAT.ViaAndLocalExpMas.Where(x => x.FromPeriod <= CurrentDate && x.VehicleType == "VCategory" && x.Vehicle == VehicleCategory).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
                                    if (viaAndLocal != null)
                                    {
                                        GetViaAdvance = ctxTFAT.ViaAndLocalExpMasRef.Where(x => x.DocNo == viaAndLocal.DocNo && ViaRouteList.Contains(x.Area)).Select(x => x.ViaAdvance.Value).ToList();
                                        if (GetViaAdvance.Sum() != 0)
                                        {
                                            Advance += GetViaAdvance.Sum();
                                        }
                                    }
                                }
                            }

                        }
                        else
                        {
                            var RateType = vehicleMaster.RateType.Split(',').ToList();
                            if (RateType.Contains("C"))
                            {
                                viaAndLocal = ctxTFAT.ViaAndLocalExpMas.Where(x => x.FromPeriod <= CurrentDate && x.VehicleType == "VCategory" && x.Vehicle == VehicleCategory).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
                                if (viaAndLocal != null)
                                {
                                    var GetViaAdvance = ctxTFAT.ViaAndLocalExpMasRef.Where(x => x.DocNo == viaAndLocal.DocNo && ViaRouteList.Contains(x.Area)).Select(x => x.ViaAdvance.Value).ToList();
                                    if (GetViaAdvance.Sum() != 0)
                                    {
                                        Advance += GetViaAdvance.Sum();
                                    }
                                }
                            }
                        }
                    }

                    if (GetLocalAdvance)
                    {
                        if (!String.IsNullOrEmpty(ToBranch))
                        {
                            var tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == ToBranch).Select(x => x.Category).FirstOrDefault();
                            if (!(tfatBranch == "Branch" || tfatBranch == "SubBranch"))
                            {
                                ViaAndLocalExpMas viaAndLocal = ctxTFAT.ViaAndLocalExpMas.Where(x => x.FromPeriod <= CurrentDate && x.VehicleType == "VNo" && x.Vehicle == Vehicle).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
                                if (viaAndLocal != null)
                                {
                                    var GetViaAdvance = ctxTFAT.ViaAndLocalExpMasRef.Where(x => x.Area == ToBranch && x.DocNo == viaAndLocal.DocNo).Select(x => x.LocalAdvance.Value).FirstOrDefault();
                                    if (GetViaAdvance != 0)
                                    {
                                        Advance += GetViaAdvance;
                                    }
                                    else
                                    {
                                        var RateType = vehicleMaster.RateType.Split(',').ToList();
                                        if (RateType.Contains("C"))
                                        {
                                            viaAndLocal = ctxTFAT.ViaAndLocalExpMas.Where(x => x.FromPeriod <= CurrentDate && x.VehicleType == "VCategory" && x.Vehicle == VehicleCategory).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
                                            if (viaAndLocal != null)
                                            {
                                                GetViaAdvance = ctxTFAT.ViaAndLocalExpMasRef.Where(x => x.Area == ToBranch && x.DocNo == viaAndLocal.DocNo).Select(x => x.LocalAdvance.Value).FirstOrDefault();
                                                if (GetViaAdvance != 0)
                                                {
                                                    Advance += GetViaAdvance;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var RateType = vehicleMaster.RateType.Split(',').ToList();
                                    if (RateType.Contains("C"))
                                    {
                                        viaAndLocal = ctxTFAT.ViaAndLocalExpMas.Where(x => x.FromPeriod <= CurrentDate && x.VehicleType == "VCategory" && x.Vehicle == VehicleCategory).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
                                        if (viaAndLocal != null)
                                        {
                                            var GetViaAdvance = ctxTFAT.ViaAndLocalExpMasRef.Where(x => x.Area == ToBranch && x.DocNo == viaAndLocal.DocNo).Select(x => x.LocalAdvance.Value).FirstOrDefault();
                                            if (GetViaAdvance != 0)
                                            {
                                                Advance += GetViaAdvance;
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
                return Json(new
                {
                    Status = "Success",
                    Freight = Freight,
                    Advance = Advance,
                },
                    JsonRequestBehavior.AllowGet
                );
            }
            else
            {
                return Json(new
                {
                    Status = "Error",
                    Freight = Freight,
                    Advance = Advance,
                },
                    JsonRequestBehavior.AllowGet
                );
            }
        }

        public decimal FetchFreight(string VehicleType, string Vehicle, string FromBranch, DateTime CurrentDate, string ToBranch)
        {
            decimal Freight = 0;
            Freight_Trip_Adv freight_Trip_ = ctxTFAT.Freight_Trip_Adv.Where(x => x.VehicleType == VehicleType && x.Vehicle == Vehicle.Trim() && x.FromBranch == FromBranch && x.FromPeriod <= CurrentDate).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
            if (freight_Trip_ != null)
            {
                Freight_Trip_AdvRef freight_Trip_Adv = ctxTFAT.Freight_Trip_AdvRef.Where(x => x.DocNo == freight_Trip_.DocNo && x.FromBranch == FromBranch && x.ToBranch == ToBranch).FirstOrDefault();
                if (freight_Trip_Adv != null)
                {
                    Freight = freight_Trip_Adv.Freight.Value;
                }
            }

            return Freight;
        }

        public decimal FetchAdvance(string VehicleType, string Vehicle, string FromBranch, DateTime CurrentDate, string ToBranch)
        {
            decimal Advance = 0;
            Freight_Trip_Adv freight_Trip_ = ctxTFAT.Freight_Trip_Adv.Where(x => x.VehicleType == VehicleType && x.Vehicle == Vehicle.Trim() && x.FromBranch == FromBranch && x.FromPeriod <= CurrentDate).OrderByDescending(x => x.FromPeriod).FirstOrDefault();
            if (freight_Trip_ != null)
            {
                Freight_Trip_AdvRef freight_Trip_Adv = ctxTFAT.Freight_Trip_AdvRef.Where(x => x.DocNo == freight_Trip_.DocNo && x.FromBranch == FromBranch && x.ToBranch == ToBranch).FirstOrDefault();
                if (freight_Trip_Adv != null)
                {
                    Advance = freight_Trip_Adv.Advance.Value;
                }
            }

            return Advance;
        }

        #endregion

        public string GetNewCode_VehiHistory()
        {
            string Code = ctxTFAT.tfatVehicleStatusHistory.OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefault();
            if (String.IsNullOrEmpty(Code))
            {

                return "100000";
            }
            else
            {
                return (Convert.ToInt32(Code) + 1).ToString();
            }
        }

        public string GetNewCodeHistory()
        {
            var NewLcNo = ctxTFAT.TfatVehicleStatus.OrderByDescending(x => x.DocNo).Select(x => x.DocNo).Take(1).FirstOrDefault();
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

        public ActionResult ShowCharges(bool Freight)
        {
            List<FmCatchChargesInfo> FMCharges = Session["FMFreightAdvance"] as List<FmCatchChargesInfo>;
            List<FmCatchChargesInfo> FMCharges1 = new List<FmCatchChargesInfo>();
            if (FMCharges == null)
            {
                FMCharges = new List<FmCatchChargesInfo>();
            }
            if (Freight)
            {

                FMCharges1.AddRange(FMCharges.Where(x => x.Type == "FA" || x.Type == "LFA" || x.Type == "VFA").ToList());
                //Session["FMFreightAdvance"] = FMCharges;
                FMCharges1.ForEach(x =>
                {
                    x.AUTHORISE = ctxTFAT.TfatBranch.Where(c => c.Code == x.FromBranch).Select(c => c.Name).FirstOrDefault();
                    x.ENTEREDBY = ctxTFAT.TfatBranch.Where(c => c.Code == x.ToBranch).Select(c => c.Name).FirstOrDefault();
                    x.AUTHIDS = x.Type == "FA" ? "Freight" : x.Type == "LFA" ? "Local Freight" : x.Type == "VFA" ? "Via Freight" : "";

                });
            }
            else
            {

                FMCharges1.AddRange(FMCharges.Where(x => x.Type == "FA" || x.Type == "LFA" || x.Type == "VFA").ToList());
                //Session["FMFreightAdvance"] = FMCharges;
                FMCharges1.ForEach(x =>
                {
                    x.Amt = x.Amt1 ?? 0;
                    x.AUTHORISE = ctxTFAT.TfatBranch.Where(c => c.Code == x.FromBranch).Select(c => c.Name).FirstOrDefault();
                    x.ENTEREDBY = ctxTFAT.TfatBranch.Where(c => c.Code == x.ToBranch).Select(c => c.Name).FirstOrDefault();
                    x.AUTHIDS = x.Type == "FA" ? "Advance" : x.Type == "LFA" ? "Local Advance" : x.Type == "VFA" ? "Via Advance" : "";
                });
            }

            var html = ViewHelper.RenderPartialView(this, "FMChargesDescription", FMCharges1);
            return Json(new
            {
                Html = html,
                JsonRequestBehavior.AllowGet
            });
        }

        public ActionResult TripShowCharges(bool Freight)
        {
            List<FmCatchChargesInfo> FMCharges = Session["TripFMFreightAdvance"] as List<FmCatchChargesInfo>;
            List<FmCatchChargesInfo> FMCharges1 = new List<FmCatchChargesInfo>();
            if (FMCharges == null)
            {
                FMCharges = new List<FmCatchChargesInfo>();
            }
            if (Freight)
            {

                FMCharges1.AddRange(FMCharges.Where(x => x.Type == "TFA" || x.Type == "TLFA" || x.Type == "TVFA").ToList());
                //Session["FMFreightAdvance"] = FMCharges;
                FMCharges1.ForEach(x =>
                {
                    x.AUTHORISE = ctxTFAT.TfatBranch.Where(c => c.Code == x.FromBranch).Select(c => c.Name).FirstOrDefault();
                    x.ENTEREDBY = ctxTFAT.TfatBranch.Where(c => c.Code == x.ToBranch).Select(c => c.Name).FirstOrDefault();
                    x.AUTHIDS = x.Type == "TFA" ? "Trip Freight" : x.Type == "TLFA" ? "Local Trip Freight" : x.Type == "TVFA" ? "Via Trip Freight" : "";

                });
            }
            else
            {

                FMCharges1.AddRange(FMCharges.Where(x => x.Type == "TFA" || x.Type == "TLFA" || x.Type == "TVFA").ToList());
                //Session["FMFreightAdvance"] = FMCharges;
                FMCharges1.ForEach(x =>
                {
                    x.Amt = x.Amt1 ?? 0;
                    x.AUTHORISE = ctxTFAT.TfatBranch.Where(c => c.Code == x.FromBranch).Select(c => c.Name).FirstOrDefault();
                    x.ENTEREDBY = ctxTFAT.TfatBranch.Where(c => c.Code == x.ToBranch).Select(c => c.Name).FirstOrDefault();
                    x.AUTHIDS = x.Type == "TFA" ? "Trip Advance" : x.Type == "TLFA" ? "Local Trip Advance" : x.Type == "TVFA" ? "Via Trip Advance" : "";
                });
            }

            var html = ViewHelper.RenderPartialView(this, "FMChargesDescription", FMCharges1);
            return Json(new
            {
                Html = html,
                JsonRequestBehavior.AllowGet
            });
        }

        public JsonResult GetBroker(string term)
        {
            var result = ctxTFAT.Master.Where(x => x.Hide == false /*&& x.IsLast== true*/ && x.AcType == "S" && x.OthPostType.Contains("B")).Select(m => new { m.Code, m.Name }).ToList();
            if (!(String.IsNullOrEmpty(term)))
            {
                result = ctxTFAT.Master.Where(x => x.Hide == false /*&& x.IsLast== true*/ && x.AcType == "S" && x.OthPostType.Contains("B") && x.Name.ToLower().Trim().Contains(term.ToLower().Trim())).Select(m => new { m.Code, m.Name }).ToList();
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
            var result = ctxTFAT.DriverMaster.Where(x => x.Status == true).Select(m => new { m.Code, m.Name }).ToList();
            if (!(String.IsNullOrEmpty(term)))
            {
                result = result.Where(x => x.Name.ToLower().Trim().Contains(term.ToLower().Trim())).Select(m => new { m.Code, m.Name }).ToList();
            }
            var Modified = result.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetPostAc(string term)
        {
            var result = ctxTFAT.Master.Where(x => x.Hide == false).Select(m => new { m.Code, m.Name }).ToList();

            var Modified = result.Select(x => new
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

        public ActionResult CheckManual(int No, string document)
        {
            string Flag = "F";
            string Msg = "";
            bool checkalloctionFound = false;

            FMSetup fMSetup = ctxTFAT.FMSetup.FirstOrDefault();
            if (fMSetup.ManualFMCheck)
            {
                List<TblBranchAllocation> tblBranchAllocations = new List<TblBranchAllocation>();
                if (fMSetup.CetralisedManualSrlReq)
                {
                    tblBranchAllocations = ctxTFAT.TblBranchAllocation.Where(x => x.Branch == mbranchcode && x.TRN_Type == "FM000").ToList();
                }
                else
                {
                    tblBranchAllocations = ctxTFAT.TblBranchAllocation.Where(x => x.Prefix == mperiod && x.Branch == mbranchcode && x.TRN_Type == "FM000").ToList();
                }
                foreach (var item in tblBranchAllocations)
                {
                    if (item.ManualFrom <= No && No <= item.ManualTo)
                    {
                        checkalloctionFound = true;
                        break;
                    }
                }
            }
            else
            {
                checkalloctionFound = true;
            }

            if (checkalloctionFound)
            {
                FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.FmNo == No && x.Prefix == mperiod && x.TableKey.ToString() != document).FirstOrDefault();
                if (fMMaster != null)
                {
                    Flag = "T";
                    Msg = "This FMNo Exist \nSo,Please Change FM No....!";
                }
                else
                {
                    var result = ctxTFAT.DocTypes.Where(x => x.Code == "FM000").Select(x => x).FirstOrDefault();
                    if (No.ToString().Length > (result.DocWidth))
                    {
                        Flag = "T";
                        Msg = "FM NO Allow " + result.DocWidth + " Digit Only....!";
                    }
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

            FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.FmNo == No && x.Prefix == mperiod && x.TableKey.ToString() != document).FirstOrDefault();
            if (fMMaster != null)
            {
                Flag = "T";
                Msg = "Please Change FM No....!";
            }
            return Json(new { Message = Flag, Msg = Msg, JsonRequestBehavior.AllowGet });
        }

        public int GetNewCode()
        {
            var mPrevSrl = GetLastSerial("FMMaster", mbranchcode, "FM000", mperiod, "RP", DateTime.Now.Date);

            return Convert.ToInt32(mPrevSrl);
            //int Code = ctxTFAT.FMMaster.Where(x => x.Prefix == mperiod).OrderByDescending(x => x.FmNo).Select(x => x.FmNo).Take(1).FirstOrDefault();
            //var DocType = ctxTFAT.DocTypes.Where(x => x.Code == "FM000").Select(x => new { x.DocWidth, x.LimitFrom }).FirstOrDefault();
            //if (Code == 0)
            //{
            //    return Convert.ToInt32(DocType.LimitFrom);
            //}
            //else
            //{
            //    return (Convert.ToInt32(Code) + 1);
            //}
        }

        public string GetNewCodeDraft()
        {
            string Code = ctxTFAT.FMMasterDraft.OrderByDescending(x => x.RECORDKEY).Select(x => x.FmNo).Take(1).FirstOrDefault();
            if (Code == null)
            {
                return "D1";
            }
            else
            {
                int newCode = Convert.ToInt32(Code.Substring(1, Code.Length - 1));
                return "D" + ++newCode;
            }
        }

        public string GetAccName(string Code)
        {
            var mName = ctxTFAT.Master.Where(X => X.Code == Code).Select(X => X.Name).FirstOrDefault();
            return mName;
        }
        #endregion

        #region Posting Functions And Posting Validation

        public string GetChargeValValue(int i, string mfield, string Tablekey)
        {
            string connstring = GetConnectionString();
            string abc;

            var loginQuery3 = @"select " + mfield + i + " from FmMaster where Tablekey='" + Tablekey + "' ";
            DataTable mDt2 = GetDataTable(loginQuery3, connstring);
            if (mDt2.Rows.Count > 0)
            {
                abc = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? "0" : mDt2.Rows[0][0].ToString();
            }
            else
            {
                abc = "0";
            }
            return abc;
        }

        public string GetChargeValValueDraft(int i, string mfield, string FMNO)
        {
            string connstring = GetConnectionString();
            string abc;

            var loginQuery3 = @"select " + mfield + i + " from FMMasterDraft where fmno='" + FMNO + "' ";
            DataTable mDt2 = GetDataTable(loginQuery3, connstring);
            if (mDt2.Rows.Count > 0)
            {
                abc = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? "0" : mDt2.Rows[0][0].ToString();
            }
            else
            {
                abc = "0";
            }
            return abc;
        }

        public ActionResult GetPostingNew(FMVM Model)
        {
            if (Model.Mode != "Delete")
            {
                string mStr = CheckValidations(Model);
                if (mStr != "")
                {
                    return Json(new
                    {
                        Message = mStr,
                        Status = "ValidError"
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            string mValidation = GetValidationPosting(Model);
            if (mValidation != "")
            {
                return Json(new { Status = "ValidError", Message = mValidation }, JsonRequestBehavior.AllowGet);
            }

            List<PurchaseVM> LedPostList = new List<PurchaseVM>();
            int n;
            VehicleMaster vehicleMaster = new VehicleMaster();
            HireVehicleMaster hireVehicleMaster = new HireVehicleMaster();

            if (Model.VehicleGroup == "100001")
            {
                hireVehicleMaster = ctxTFAT.HireVehicleMaster.Where(x => x.Code == Model.VehicleNo).FirstOrDefault();
            }
            else
            {
                vehicleMaster = ctxTFAT.VehicleMaster.Where(x => x.Code == Model.VehicleNo).FirstOrDefault();
            }

            string mBroPostCode = ctxTFAT.Master.Where(x => x.Code.ToString() == Model.Broker).Select(x => x.Code).FirstOrDefault();
            string VehicleLedPostCode = vehicleMaster.PostAc;

            string mDebitAcc = "";
            string mCreditAcc = "";
            if (Model.VehicleGroup == "100001")
            {
                mDebitAcc = ctxTFAT.FMSetup.Select(x => x.HireDrAc).FirstOrDefault();
                mCreditAcc = mBroPostCode;
            }
            else
            {
                mDebitAcc = vehicleMaster.DrAc;
                mCreditAcc = (vehicleMaster.CrAc == "B") ? mBroPostCode : VehicleLedPostCode;
            }

            bool mMaintainDriverac = Model.VehicleGroup == "100001" ? false : vehicleMaster.MaintainDriverAC;
            //bool FmVourel = Model.VehicleGroup == "100001" ? false : vehicleMaster.FMVOURELReq;

            Model.Balance = Model.Freight - Model.Advance;
            decimal mDebit = 0;
            decimal mCredit = 0;
            int xCnt = 1;

            List<PurchaseVM> PostCharges = new List<PurchaseVM>();

            #region Only FM000 POsting
            if (vehicleMaster.PostReq == true || Model.VehicleGroup == "100001")
            {
                foreach (var a in Model.Charges)
                {
                    string EqCost = "";
                    decimal vals = 0;
                    EqCost = ctxTFAT.Charges.Where(x => x.Fld == a.Fld && x.Type == "FM000").Select(x => x.EqCost).FirstOrDefault();
                    if (EqCost == "+")
                    {
                        vals = +a.Val1;
                    }
                    if (EqCost == "-")
                    {
                        vals = -a.Val1;
                    }

                    PostCharges.Add(new PurchaseVM()
                    {
                        Code = a.Code,
                        AddLess = a.AddLess,
                        Amt = a.Amt,
                        Val1 = vals,
                        ChgPostCode = a.ChgPostCode,
                        Fld = a.Fld,
                        EqCost = EqCost
                    });
                }
                //Freight Posting Normal
                if (Model.Freight > 0)
                {
                    LedPostList.Add(new PurchaseVM()
                    {
                        Code = mDebitAcc,
                        AccountName = GetAccName(mDebitAcc),
                        Debit = Model.Freight,
                        Credit = 0,
                        Branch = mbranchcode,
                        tempId = xCnt++,
                        RefDoc = "",
                        DelyCode = mCreditAcc
                    });
                }
                if (Model.VehicleGroup == "100001")
                {
                    if (Model.Advance > 0)
                    {
                        LedPostList.Add(new PurchaseVM()
                        {
                            Code = mCreditAcc,
                            AccountName = GetAccName(mCreditAcc),
                            Debit = 0,
                            Credit = Model.Advance,
                            Branch = mbranchcode,
                            tempId = xCnt++,
                            RefDoc = "A",
                            DelyCode = mDebitAcc
                        });
                    }
                    if (Model.Balance > 0)
                    {
                        LedPostList.Add(new PurchaseVM()
                        {
                            Code = mCreditAcc,
                            AccountName = GetAccName(mCreditAcc),
                            Debit = 0,
                            Credit = Model.Balance,
                            Branch = mbranchcode,
                            tempId = xCnt++,
                            RefDoc = "B",
                            DelyCode = mDebitAcc
                        });
                    }
                }
                else
                {
                    if (Model.Freight > 0)
                    {
                        LedPostList.Add(new PurchaseVM()
                        {
                            Code = mCreditAcc,
                            AccountName = GetAccName(mCreditAcc),
                            Debit = 0,
                            Credit = Model.Freight,
                            Branch = mbranchcode,
                            tempId = xCnt++,
                            RefDoc = "B",
                            DelyCode = mDebitAcc
                        });
                    }
                }


                if (Model.TDSAmt != 0)
                {
                    LedPostList.Add(new PurchaseVM()
                    {
                        Code = mCreditAcc,
                        AccountName = GetAccName(mCreditAcc),
                        Debit = Model.TDSAmt,
                        Credit = 0,
                        Branch = mbranchcode,
                        tempId = xCnt++,
                        RefDoc = "B",
                        TDSFlag = true,
                        DelyCode = "000009994"
                    });


                    LedPostList.Add(new PurchaseVM()
                    {
                        Code = "000009994",
                        AccountName = GetAccName("000009994"),
                        Debit = 0,
                        Credit = Model.TDSAmt,
                        Branch = mbranchcode,
                        tempId = xCnt++,
                        RefDoc = "B",
                        DelyCode = mCreditAcc
                    });
                }
            }
            #endregion

            #region If  Driver Posting If DriverMaintain AC ON
            string mDriverAc = "";
            string mDriverPostCode = "";
            string mDriverAdvancePayable = "";
            if (mMaintainDriverac == true)
            {
                mDriverAc = Model.DriverCode;
                mDriverPostCode = ctxTFAT.DriverMaster.Where(x => x.Code == mDriverAc).Select(x => x.Posting).FirstOrDefault();
                mDriverAdvancePayable = vehicleMaster.DriCrAc;

                if (Model.TripAdvance > 0)
                {
                    LedPostList.Add(new PurchaseVM()
                    {
                        Code = mDriverPostCode,
                        AccountName = GetAccName(mDriverPostCode),
                        Debit = Model.TripAdvance,
                        Credit = 0,
                        Branch = mbranchcode,
                        tempId = xCnt++,
                        Party = mDriverAdvancePayable,
                        RefDoc = "A",
                        DelyCode = mDriverAdvancePayable
                    });
                    LedPostList.Add(new PurchaseVM()
                    {
                        Code = mDriverAdvancePayable,
                        AccountName = GetAccName(mDriverAdvancePayable),
                        Debit = 0,
                        Credit = Model.TripAdvance,
                        Branch = mbranchcode,
                        tempId = xCnt++,
                        RefDoc = "A",
                        DelyCode = mDriverPostCode
                    });
                }

            }
            #endregion

            var mchg = Model.Charges.Where(x => x.AddLess == "+" || x.AddLess == "-").Select(x => x).ToList();
            decimal mPostAmt = 0;
            decimal mDebitchargeside = 0;
            decimal mcrDebit = 0;
            decimal mcrCredit = 0;

            #region If Driver Posting False (Both) Then Charges Ledger Posting  Against Broker Account
            if (mMaintainDriverac == false)
            {
                foreach (PurchaseVM mc in mchg)//for creditor account
                {
                    if (mc.ChgPostCode != "" && mc.Val1 != 0)
                    {
                        mPostAmt = mc.Val1;
                        if (mc.AddLess == "+")
                        {
                            mcrDebit = 0;
                            mcrCredit = mPostAmt;
                        }
                        else
                        {
                            mcrDebit = mPostAmt;
                            mcrCredit = 0;
                        }

                        LedPostList.Add(new PurchaseVM()
                        {
                            Code = mCreditAcc,
                            AccountName = GetAccName(mCreditAcc),
                            Debit = mcrDebit,
                            Credit = mcrCredit,
                            Branch = mbranchcode,
                            tempId = xCnt++,
                            RefDoc = "A",
                            Party = mDriverAdvancePayable,
                            Reference = mc.ChgPostCode,
                            RelatedTo = GetAccName(mc.ChgPostCode),
                            DelyCode = mc.ChgPostCode
                        });
                    }
                }
                foreach (PurchaseVM mc in mchg)
                {
                    // if (product wise posting ) dont post net amount
                    if (mc.ChgPostCode != "" && mc.Val1 != 0)
                    {
                        mPostAmt = mc.Val1;
                        if (mc.AddLess == "+")
                        {
                            mDebit = mPostAmt;
                            mCredit = 0;
                        }
                        else
                        {
                            mDebit = 0;
                            mCredit = mPostAmt;
                        }
                        //mchargelist.Add(new PurchaseVM()
                        //{
                        //    Debit = mDebit,
                        //    Credit = mCredit,

                        //});



                        LedPostList.Add(new PurchaseVM()
                        {
                            Code = mc.ChgPostCode,
                            AccountName = GetAccName(mc.ChgPostCode),
                            Debit = mDebit,
                            Credit = mCredit,
                            Branch = mbranchcode,
                            tempId = xCnt++,
                            RefDoc = "A",
                            Party = mDriverAdvancePayable,
                            Reference = mc.ChgPostCode,
                            RelatedTo = GetAccName(mc.ChgPostCode),
                            DelyCode = mCreditAcc
                        });

                    }
                }
            }
            #endregion

            #region If Driver Posting True Then Ledger Posting Against Driver Payable Account
            if (mMaintainDriverac == true)
            {
                foreach (PurchaseVM mc in mchg)//for creditor account
                {
                    if (mc.ChgPostCode != "" && mc.Val1 != 0)

                    {
                        mPostAmt = mc.Val1;
                        if (mc.AddLess == "+")
                        {
                            mcrDebit = 0;
                            mcrCredit = mPostAmt;
                        }
                        else
                        {
                            mcrDebit = mPostAmt;
                            mcrCredit = 0;
                        }

                        LedPostList.Add(new PurchaseVM()
                        {
                            Code = mDriverAdvancePayable,
                            AccountName = GetAccName(mDriverAdvancePayable),
                            Debit = mcrDebit,
                            Credit = mcrCredit,
                            Branch = mbranchcode,
                            tempId = xCnt++,
                            RefDoc = "A",
                            Party = mDriverAdvancePayable,
                            Reference = mc.ChgPostCode,
                            RelatedTo = GetAccName(mc.ChgPostCode),
                            DelyCode = mDriverPostCode
                        });
                    }

                }
                foreach (PurchaseVM mc in mchg)
                {
                    if (mc.ChgPostCode != "" && mc.Val1 != 0)
                    {
                        mPostAmt = mc.Val1;
                        if (mc.AddLess == "+")
                        {
                            mDebit = mPostAmt;
                            mCredit = 0;
                        }
                        else
                        {
                            mDebit = 0;
                            mCredit = mPostAmt;
                        }


                        LedPostList.Add(new PurchaseVM()
                        {
                            Code = mDriverPostCode,
                            AccountName = GetAccName(mDriverPostCode),
                            Debit = mDebit,
                            Credit = mCredit,
                            Branch = mbranchcode,
                            tempId = xCnt++,
                            RefDoc = "A",
                            Party = mDriverAdvancePayable,
                            Reference = mc.ChgPostCode,
                            RelatedTo = GetAccName(mc.ChgPostCode),
                            DelyCode = mDriverAdvancePayable
                        });

                    }
                }
            }
            #endregion

            Model.TotDebit = LedPostList.Sum(x => x.Debit);
            Model.TotCredit = LedPostList.Sum(x => x.Credit);
            Model.LedgerPostList = LedPostList.Where(x => (x.Debit + x.Credit) > 0).ToList();

            // display view form
            var html = ViewHelper.RenderPartialView(this, "LedgerPosting", new FMVM() { LedgerPostList = Model.LedgerPostList, TotDebit = Math.Round(Model.TotDebit, 2), TotCredit = Math.Round(Model.TotCredit, 2), Mode = Model.Mode, Controller2 = Model.Controller2, Module = Model.Module, ViewDataId = Model.ViewDataId, TableName = Model.TableName, Header = Model.Header, OptionType = Model.OptionType, OptionCode = Model.OptionCode, MainType = Model.MainType });
            return Json(new
            {
                LedgerPostList = Model.LedgerPostList,
                TotDebit = Math.Round(Model.TotDebit, 2),
                TotCredit = Math.Round(Model.TotCredit, 2),
                Mode = Model.Mode,
                Html = html,
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPostingNew1(FMVM Model)
        {
            if (Model.Mode != "Delete")
            {
                string mStr = CheckValidations(Model);
                if (mStr != "")
                {
                    return Json(new
                    {
                        Message = mStr,
                        Status = "ValidError"
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            string mValidation = GetValidationPosting(Model);
            if (mValidation != "")
            {
                return Json(new { Status = "ValidError", Message = mValidation }, JsonRequestBehavior.AllowGet);
            }

            List<PurchaseVM> LedPostList = new List<PurchaseVM>();
            int n;
            VehicleMaster vehicleMaster = new VehicleMaster();
            HireVehicleMaster hireVehicleMaster = new HireVehicleMaster();

            if (Model.VehicleGroup == "100001")
            {
                hireVehicleMaster = ctxTFAT.HireVehicleMaster.Where(x => x.Code == Model.VehicleNo).FirstOrDefault();
            }
            else
            {
                vehicleMaster = ctxTFAT.VehicleMaster.Where(x => x.Code == Model.VehicleNo).FirstOrDefault();
            }

            string mBroPostCode = ctxTFAT.Master.Where(x => x.Code.ToString() == Model.Broker).Select(x => x.Code).FirstOrDefault();
            string VehicleLedPostCode = vehicleMaster.PostAc;

            string mDebitAcc = "";
            string mCreditAcc = "";
            if (Model.VehicleGroup == "100001")
            {
                mDebitAcc = ctxTFAT.FMSetup.Select(x => x.HireDrAc).FirstOrDefault();
                mCreditAcc = mBroPostCode;
            }
            else
            {
                mDebitAcc = vehicleMaster.DrAc;
                mCreditAcc = (vehicleMaster.CrAc == "B") ? mBroPostCode : VehicleLedPostCode;
            }

            bool mMaintainDriverac = Model.VehicleGroup == "100001" ? false : vehicleMaster.MaintainDriverAC;
            bool FmVourel = Model.VehicleGroup == "100001" ? false : vehicleMaster.FMVOURELReq;

            Model.Balance = Model.Freight - Model.Advance;
            decimal mDebit = 0;
            decimal mCredit = 0;
            int xCnt = 1;

            List<PurchaseVM> PostCharges = new List<PurchaseVM>();

            #region Only FM000 POsting
            if ((Model.VehicleGroup != "100001" && vehicleMaster.PostReq == true) || Model.VehicleGroup == "100001")
            {
                foreach (var a in Model.Charges)
                {
                    string EqCost = "";
                    decimal vals = 0;
                    EqCost = ctxTFAT.Charges.Where(x => x.Fld == a.Fld && x.Type == "FM000").Select(x => x.EqCost).FirstOrDefault();
                    if (EqCost == "+")
                    {
                        vals = +a.Val1;
                    }
                    if (EqCost == "-")
                    {
                        vals = -a.Val1;
                    }

                    PostCharges.Add(new PurchaseVM()
                    {
                        Code = a.Code,
                        AddLess = a.AddLess,
                        Amt = a.Amt,
                        Val1 = vals,
                        ChgPostCode = a.ChgPostCode,
                        Fld = a.Fld,
                        EqCost = EqCost
                    });
                }

                if ((mMaintainDriverac == false) || Model.VehicleGroup == "100001")//freight adv and bal 3 entries 
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        if (i == 1)
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mDebitAcc,
                                AccountName = GetAccName(mDebitAcc),
                                Debit = Model.Freight,
                                Credit = 0,
                                Branch = mbranchcode,
                                tempId = xCnt++,
                                RefDoc = "",
                                DelyCode = mCreditAcc
                            });
                        }

                        if (i == 2)
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mCreditAcc,
                                AccountName = GetAccName(mCreditAcc),
                                Debit = 0,
                                Credit = Model.Advance,
                                Branch = mbranchcode,
                                tempId = xCnt++,
                                RefDoc = "A",
                                DelyCode = mDebitAcc
                            });
                        }
                        if (i == 3)
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mCreditAcc,
                                AccountName = GetAccName(mCreditAcc),
                                Debit = 0,
                                Credit = Model.Balance,
                                Branch = mbranchcode,
                                tempId = xCnt++,
                                RefDoc = "B",
                                DelyCode = mDebitAcc
                            });
                        }


                    }
                }
                else if (mMaintainDriverac == true)
                {

                    for (int i = 1; i <= 2; i++)
                    {
                        if (i == 1)
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mDebitAcc,
                                AccountName = GetAccName(mDebitAcc),
                                Debit = Model.Freight,
                                Credit = 0,
                                Branch = mbranchcode,
                                tempId = xCnt++,
                                DelyCode = mCreditAcc

                            });
                        }

                        if (i == 2)
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mCreditAcc,
                                AccountName = GetAccName(mCreditAcc),
                                Debit = 0,
                                Credit = Model.Freight,
                                Branch = mbranchcode,
                                tempId = xCnt++,
                                RefDoc = "B",
                                DelyCode = mDebitAcc
                            });
                        }
                    }
                }
                else
                {
                    for (int i = 1; i <= 2; i++)
                    {
                        if (i == 1)
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mDebitAcc,
                                AccountName = GetAccName(mDebitAcc),
                                Debit = Model.Freight,
                                Credit = 0,
                                Branch = mbranchcode,
                                tempId = xCnt++,
                                DelyCode = mCreditAcc
                            });
                        }

                        if (i == 2)
                        {
                            LedPostList.Add(new PurchaseVM()
                            {
                                Code = mCreditAcc,
                                AccountName = GetAccName(mCreditAcc),
                                Debit = 0,
                                Credit = Model.Freight,
                                Branch = mbranchcode,
                                tempId = xCnt++,
                                RefDoc = "B",//earlier blank,
                                DelyCode = mDebitAcc
                            });
                        }
                    }
                }

                if (Model.TDSAmt != 0)
                {
                    LedPostList.Add(new PurchaseVM()
                    {
                        Code = mCreditAcc,
                        AccountName = GetAccName(mCreditAcc),
                        Debit = Model.TDSAmt,
                        Credit = 0,
                        Branch = mbranchcode,
                        tempId = xCnt++,
                        RefDoc = "B",
                        TDSFlag = true,
                        DelyCode = "000009994"
                    });


                    LedPostList.Add(new PurchaseVM()
                    {
                        Code = "000009994",
                        AccountName = GetAccName("000009994"),
                        Debit = 0,
                        Credit = Model.TDSAmt,
                        Branch = mbranchcode,
                        tempId = xCnt++,
                        RefDoc = "B",
                        DelyCode = mCreditAcc
                    });
                }
            }
            #endregion

            string mDriverAc = "";
            string mDriverPostCode = "";
            string mDriverAdvancePayable = "";
            if (mMaintainDriverac == true)
            {
                mDriverAc = Model.DriverCode;
                mDriverPostCode = ctxTFAT.DriverMaster.Where(x => x.Code == mDriverAc).Select(x => x.Posting).FirstOrDefault();
                mDriverAdvancePayable = vehicleMaster.DriCrAc;
            }

            #region If FmVourel Not Maintain and Driver Posting On Then Ledger Posting Below
            if (mMaintainDriverac == true && FmVourel == false)
            {
                LedPostList.Add(new PurchaseVM()
                {
                    Code = mDriverPostCode,
                    AccountName = GetAccName(mDriverPostCode),
                    Debit = Model.Advance,
                    Credit = 0,
                    Branch = mbranchcode,
                    tempId = xCnt++,
                    Party = mDriverAdvancePayable,
                    RefDoc = "A",
                    DelyCode = mDriverAdvancePayable
                });

                LedPostList.Add(new PurchaseVM()
                {
                    Code = mDriverAdvancePayable,
                    AccountName = GetAccName(mDriverAdvancePayable),
                    Debit = 0,
                    Credit = Model.Advance,
                    Branch = mbranchcode,
                    tempId = xCnt++,
                    RefDoc = "A",
                    DelyCode = mDriverPostCode
                });
            }
            #endregion

            var mchg = Model.Charges.Where(x => x.AddLess == "+" || x.AddLess == "-").Select(x => x).ToList();
            decimal mPostAmt = 0;
            decimal mDebitchargeside = 0;
            decimal mcrDebit = 0;
            decimal mcrCredit = 0;

            #region If Driver Posting False (Both) Then Charges Ledger Posting  Against Broker Account
            if (mMaintainDriverac == false)
            {
                foreach (PurchaseVM mc in mchg)//for creditor account
                {
                    if (mc.ChgPostCode != "" && mc.Val1 != 0)
                    {
                        mPostAmt = mc.Val1;
                        if (mc.AddLess == "+")
                        {
                            mcrDebit = 0;
                            mcrCredit = mPostAmt;
                        }
                        else
                        {
                            mcrDebit = mPostAmt;
                            mcrCredit = 0;
                        }

                        LedPostList.Add(new PurchaseVM()
                        {
                            Code = mCreditAcc,
                            AccountName = GetAccName(mCreditAcc),
                            Debit = mcrDebit,
                            Credit = mcrCredit,
                            Branch = mbranchcode,
                            tempId = xCnt++,
                            RefDoc = "A",
                            Party = mDriverAdvancePayable,
                            Reference = mc.ChgPostCode,
                            RelatedTo = GetAccName(mc.ChgPostCode),
                            DelyCode = mc.ChgPostCode
                        });
                    }
                }
                foreach (PurchaseVM mc in mchg)
                {
                    // if (product wise posting ) dont post net amount
                    if (mc.ChgPostCode != "" && mc.Val1 != 0)
                    {
                        mPostAmt = mc.Val1;
                        if (mc.AddLess == "+")
                        {
                            mDebit = mPostAmt;
                            mCredit = 0;
                        }
                        else
                        {
                            mDebit = 0;
                            mCredit = mPostAmt;
                        }
                        //mchargelist.Add(new PurchaseVM()
                        //{
                        //    Debit = mDebit,
                        //    Credit = mCredit,

                        //});



                        LedPostList.Add(new PurchaseVM()
                        {
                            Code = mc.ChgPostCode,
                            AccountName = GetAccName(mc.ChgPostCode),
                            Debit = mDebit,
                            Credit = mCredit,
                            Branch = mbranchcode,
                            tempId = xCnt++,
                            RefDoc = "A",
                            Party = mDriverAdvancePayable,
                            Reference = mc.ChgPostCode,
                            RelatedTo = GetAccName(mc.ChgPostCode),
                            DelyCode = mCreditAcc
                        });

                    }
                }
            }
            #endregion

            #region If Driver Posting True Then Ledger Posting Against Driver Payable Account
            if (mMaintainDriverac == true)
            {
                foreach (PurchaseVM mc in mchg)//for creditor account
                {
                    if (mc.ChgPostCode != "" && mc.Val1 != 0)

                    {
                        mPostAmt = mc.Val1;
                        if (mc.AddLess == "+")
                        {
                            mcrDebit = 0;
                            mcrCredit = mPostAmt;
                        }
                        else
                        {
                            mcrDebit = mPostAmt;
                            mcrCredit = 0;
                        }

                        LedPostList.Add(new PurchaseVM()
                        {
                            Code = mDriverAdvancePayable,
                            AccountName = GetAccName(mDriverAdvancePayable),
                            Debit = mcrDebit,
                            Credit = mcrCredit,
                            Branch = mbranchcode,
                            tempId = xCnt++,
                            RefDoc = "A",
                            Party = mDriverAdvancePayable,
                            Reference = mc.ChgPostCode,
                            RelatedTo = GetAccName(mc.ChgPostCode),
                            DelyCode = mDriverPostCode
                        });
                    }

                }
                foreach (PurchaseVM mc in mchg)
                {
                    if (mc.ChgPostCode != "" && mc.Val1 != 0)
                    {
                        mPostAmt = mc.Val1;
                        if (mc.AddLess == "+")
                        {
                            mDebit = mPostAmt;
                            mCredit = 0;
                        }
                        else
                        {
                            mDebit = 0;
                            mCredit = mPostAmt;
                        }


                        LedPostList.Add(new PurchaseVM()
                        {
                            Code = mDriverPostCode,
                            AccountName = GetAccName(mDriverPostCode),
                            Debit = mDebit,
                            Credit = mCredit,
                            Branch = mbranchcode,
                            tempId = xCnt++,
                            RefDoc = "A",
                            Party = mDriverAdvancePayable,
                            Reference = mc.ChgPostCode,
                            RelatedTo = GetAccName(mc.ChgPostCode),
                            DelyCode = mDriverAdvancePayable
                        });

                    }
                }
            }
            #endregion

            Model.TotDebit = LedPostList.Sum(x => x.Debit);
            Model.TotCredit = LedPostList.Sum(x => x.Credit);
            Model.LedgerPostList = LedPostList.Where(x => (x.Debit + x.Credit) > 0).ToList();

            // display view form
            var html = ViewHelper.RenderPartialView(this, "LedgerPosting", new FMVM() { LedgerPostList = Model.LedgerPostList, TotDebit = Math.Round(Model.TotDebit, 2), TotCredit = Math.Round(Model.TotCredit, 2), Mode = Model.Mode, Controller2 = Model.Controller2, Module = Model.Module, ViewDataId = Model.ViewDataId, TableName = Model.TableName, Header = Model.Header, OptionType = Model.OptionType, OptionCode = Model.OptionCode, MainType = Model.MainType });
            return Json(new
            {
                LedgerPostList = Model.LedgerPostList,
                TotDebit = Math.Round(Model.TotDebit, 2),
                TotCredit = Math.Round(Model.TotCredit, 2),
                Mode = Model.Mode,
                Html = html,
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);
        }

        public decimal GetChargesVal(List<PurchaseVM> Charges, string FToken)
        {
            string connstring = GetConnectionString();
            string sql;
            decimal mamtm;
            var Val = Charges.Where(x => x.Fld == FToken).Select(x => x.Val1).FirstOrDefault();
            var PosNeg = Charges.Where(x => x.Fld == FToken).Select(x => x.AddLess).FirstOrDefault();
            sql = @"Select Top 1 " + PosNeg + Val + " from TfatComp";
            DataTable smDt = GetDataTable(sql, connstring);
            if (smDt.Rows.Count > 0)
            {
                mamtm = (smDt.Rows[0][0].ToString() == "" || smDt.Rows[0][0].ToString() == null) ? 0 : Convert.ToDecimal(smDt.Rows[0][0]);
            }
            else
            {
                mamtm = 0;
            }
            return mamtm;
        }

        public string GetValidationPosting(FMVM mModel)
        {
            string mMessage = "";
            VehicleMaster mvehicleMaster = new VehicleMaster();
            HireVehicleMaster mhireVehicleMaster = new HireVehicleMaster();

            if (mModel.VehicleGroup == "100001")
            {
                mhireVehicleMaster = ctxTFAT.HireVehicleMaster.Where(x => x.Code == mModel.VehicleNo).FirstOrDefault();
                var HirePostAc = ctxTFAT.FMSetup.Select(x => x.HireDrAc).FirstOrDefault();
                if (string.IsNullOrEmpty(HirePostAc) == true)
                {
                    mMessage = mMessage + " Debit Posting Account needed to be set in Freight Memo Setup.";
                }
                string mBroPostCode = ctxTFAT.Master.Where(x => x.Code.ToString() == mModel.Broker).Select(x => x.Code).FirstOrDefault();
                if (string.IsNullOrEmpty(mBroPostCode))
                {
                    mMessage = mMessage + " Broker Posting Account needed to be set in Vehicle Master";
                }
            }
            else
            {
                string mBroPostCode = ctxTFAT.Master.Where(x => x.Code.ToString() == mModel.Broker).Select(x => x.Code).FirstOrDefault();
                mvehicleMaster = ctxTFAT.VehicleMaster.Where(x => x.Code == mModel.VehicleNo).FirstOrDefault();
                string mDriverPostCode = mModel.VehicleGroup == "100001" ? null : ctxTFAT.DriverMaster.Where(x => x.Code == mModel.DriverCode).Select(x => x.Posting).FirstOrDefault();
                string mVPostCode = (mvehicleMaster.MaintainDriverAC == true) ? mDriverPostCode : mBroPostCode;
                if (string.IsNullOrEmpty(mvehicleMaster.DrAc) == true)
                {
                    mMessage = mMessage + " Debit Posting Account needed to be set in Hire Vehicle Master";
                }
                if (mvehicleMaster.MaintainDriverAC == true && mvehicleMaster.FMVOURELReq == false)
                {
                    if (string.IsNullOrEmpty(mDriverPostCode))
                    {
                        mMessage = mMessage + " Driver Posting Account needed to be set in Vehicle Master";
                    }
                    if (mModel.Balance != 0)
                    {
                        mMessage = mMessage + " Balance should be nill or zero";
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(mBroPostCode))
                    {
                        mMessage = mMessage + " Broker Posting Account needed to be set in Vehicle Master";
                    }

                }
            }


            var DocDate = ConvertDDMMYYTOYYMMDD(mModel.FM_Date);
            if (!(ConvertDDMMYYTOYYMMDD(StartDate) <= DocDate && DocDate <= ConvertDDMMYYTOYYMMDD(EndDate)))
            {
                mMessage = mMessage + " Financial Date Range Allow Only...!";
            }


            return mMessage;
        }

        public string CheckValidations(FMVM Model)
        {
            int count = 0;
            string message = "";

            var Date = Model.FM_Date.Split('/');
            string[] Time = Model.FM_Time.Split(':');
            DateTime Fmdate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Time[0]), Convert.ToInt32(Time[1]), 00);

            if (ConvertDDMMYYTOYYMMDD(StartDate) >= Fmdate && Fmdate >= ConvertDDMMYYTOYYMMDD(EndDate))
            {
                message += " Date Allow Financial Range Only";
            }
            if (Model.Mode == "Add")
            {
                if (Model.FMGenerate == "A")
                {

                    FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.FmNo.ToString() == Model.FMNO && x.TableKey.ToString() != Model.Document).FirstOrDefault();
                    if (fMMaster != null)
                    {

                        message += " Please Change FM No....!";
                    }

                }
                else
                {
                    bool checkalloctionFound = false;

                    FMSetup fMSetup2 = ctxTFAT.FMSetup.FirstOrDefault();
                    if (fMSetup2.ManualFMCheck)
                    {
                        List<TblBranchAllocation> tblBranchAllocations = new List<TblBranchAllocation>();
                        if (fMSetup2.CetralisedManualSrlReq)
                        {
                            tblBranchAllocations = ctxTFAT.TblBranchAllocation.Where(x => x.Branch == mbranchcode && x.TRN_Type == "FM000").ToList();
                        }
                        else
                        {
                            tblBranchAllocations = ctxTFAT.TblBranchAllocation.Where(x => x.Prefix == mperiod && x.Branch == mbranchcode && x.TRN_Type == "FM000").ToList();
                        }
                        var mFmNo = (string.IsNullOrEmpty(Model.FMNO) == true) ? 0 : Convert.ToInt32(Model.FMNO);
                        foreach (var item in tblBranchAllocations)
                        {
                            if (item.ManualFrom <= mFmNo && mFmNo <= item.ManualTo)
                            {
                                checkalloctionFound = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        checkalloctionFound = true;
                    }

                    if (checkalloctionFound)
                    {
                        FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.FmNo.ToString() == Model.FMNO && x.TableKey.ToString() != Model.Document).FirstOrDefault();
                        if (fMMaster != null)
                        {

                            message += " This FMNo Exist \nSo,Please Change FM No....!";
                        }
                    }
                    else
                    {

                        message += " Manual Range Not Found....!";
                    }
                }
            }


            if (Model.ScheduleRequired)
            {
                DateTime ArrivalDate = new DateTime();
                if (!(String.IsNullOrEmpty(Model.ArrivalDate)))
                {
                    var Datee = Model.ArrivalDate.Split('/');
                    var Datee1 = Model.ArrivalTime.Split(':');

                    ArrivalDate = new DateTime(Convert.ToInt32(Datee[2]), Convert.ToInt32(Datee[1]), Convert.ToInt32(Datee[0]), Convert.ToInt32(Datee1[0]), Convert.ToInt32(Datee1[1]), 00);


                    if ((ArrivalDate > Fmdate))
                    {
                        message += "Arrival Date Not Allow Greater Than FMDate.....\n";
                    }

                    //if (!(Convert.ToInt32(Model.KM) <= Convert.ToInt32(Model.ArrivalKM)))
                    //{
                    //    message += "Arrival KM Not Allow less Than FM KM.....\n";
                    //}
                }
                if (!(String.IsNullOrEmpty(Model.DispatchDate)))
                {
                    var Datee = Model.DispatchDate.Split('/');
                    var Datee1 = Model.DispatchTime.Split(':');
                    DateTime DispatchDate = new DateTime(Convert.ToInt32(Datee[2]), Convert.ToInt32(Datee[1]), Convert.ToInt32(Datee[0]), Convert.ToInt32(Datee1[0]), Convert.ToInt32(Datee1[1]), 00);

                    if (!(DispatchDate >= ArrivalDate))
                    {
                        message += "Dispatch Date Not Allow less Than Arrival Date.....\n";
                    }

                    //if (!(Convert.ToInt32(Model.KM) <= Convert.ToInt32(Model.DispatchKM)))
                    //{
                    //    message += "Dispatch KM Not Allow less Than FM KM.....\n";
                    //}

                    //if (!(Convert.ToInt32(Model.ArrivalKM) <= Convert.ToInt32(Model.DispatchKM)))
                    //{
                    //    message += "Dispatch KM Not Allow less Than Arrival KM.....";
                    //}


                }



            }

            return message;
        }

        public ActionResult FetchBrokerTDS(FMVM Model)
        {
            string MaintainDriverAc = "F";
            bool CutTDS = false;
            decimal TDSRate = 0;
            string mCreditAcc = "";
            bool ApplTDS = false;
            VehicleMaster vehicleMaster = ctxTFAT.VehicleMaster.Where(x => x.Code == Model.VehicleNo).FirstOrDefault();
            FMSetup fMSetup = ctxTFAT.FMSetup.FirstOrDefault();
            if (fMSetup == null)
            {
                fMSetup = new FMSetup();
            }
            if (!String.IsNullOrEmpty(Model.VehicleNo))
            {
                if (vehicleMaster != null)
                {
                    string VehicleLedPostCode = vehicleMaster.PostAc;
                    if (vehicleMaster.MaintainDriverAC)
                    {
                        MaintainDriverAc = "T";
                    }
                    mCreditAcc = (vehicleMaster.CrAc == "B") ? Model.Broker : VehicleLedPostCode;


                    if (vehicleMaster.TruckStatus == "100000" && fMSetup.ATDSAdjReq == true)
                    {
                        ApplTDS = fMSetup.ATDSAdjReq;
                    }
                    else if (vehicleMaster.TruckStatus == "100002" && fMSetup.OTDSAdjReq == true)
                    {
                        ApplTDS = fMSetup.OTDSAdjReq;
                    }
                }
                else
                {
                    mCreditAcc = Model.Broker;
                    ApplTDS = fMSetup.HTDSAdjReq;
                }
                var tdsdetails = ctxTFAT.TaxDetails.Where(x => x.Code == mCreditAcc).Select(x => new { x.CutTDS, x.TDSCode }).FirstOrDefault();
                if (ApplTDS)
                {
                    CutTDS = true;
                }
                var tdscode = tdsdetails == null ? 0 : (tdsdetails.TDSCode == null) ? 0 : tdsdetails.TDSCode.Value;
                var mDocDate = DateTime.Now.Date;
                var TDSRATEtab = ctxTFAT.TDSRates.Where(x => x.Code == tdscode && x.EffDate <= mDocDate).OrderByDescending(x => x.EffDate).Select(x => new { x.TDSRate, x.Cess, x.SHECess, x.SurCharge }).FirstOrDefault();
                TDSRate = (TDSRATEtab != null && CutTDS == true) ? ((TDSRATEtab.TDSRate == null) ? 0 : TDSRATEtab.TDSRate.Value) : 0;

            }


            return Json(new
            {
                MaintainDriverAc = MaintainDriverAc,
                CutTDS = CutTDS,
                TDSRate = TDSRate,
                ApplTDS = ApplTDS,
                JsonRequestBehavior.AllowGet
            });
        }

        #endregion

        [HttpPost]       //Draft List
        public ActionResult FMDraft(GridOption Model)
        {
            Model.Document = "";
            Model.AccountName = "";
            moptioncode = "FMMasterDraft";
            msubcodeof = "FMMasterDraft";
            mmodule = "Transactions";
            ViewBag.id = "FMMasterDraft";
            ViewBag.ViewDataId = "FMMasterDraft";
            ViewBag.Header = "FMMasterDraft";
            ViewBag.Table = "FMMasterDraft";
            ViewBag.Controller = "FM";
            ViewBag.MainType = "M";
            ViewBag.Controller2 = "FM";
            ViewBag.OptionType = "";
            ViewBag.OptionCode = "FMMasterDraft";
            ViewBag.Module = "Transactions";
            ViewBag.ViewName = "";
            ViewBag.ViewDataId1 = "FMMasterDraft";

            var html = ViewHelper.RenderPartialView(this, "_FreightMemoDraftView", Model);
            var jsonResult = Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        #region Index Page

        // GET: Logistics/FM
        public ActionResult Index(FMVM mModel)
        {
            Session["CommnNarrlist"] = null;
            Session["TempAttach"] = null;
            Session["GridDataSession"] = null;
            TempData.Remove("ExistLC");
            TempData.Remove("Destination");
            TempData.Remove("FMAttachmentList");

            #region Defalut Load

            // Setup Logistics Flow
            mModel.LogisticsFlow = ctxTFAT.LogisticsFlow.FirstOrDefault();
            if (mModel.LogisticsFlow == null)
            {
                mModel.LogisticsFlow = new LogisticsFlow();
            }

            // Restriction Of VehicleMaster
            mModel.VehicleRestrict = true;
            if (muserid.ToUpper() != "SUPER")
            {
                //1464 VehicleMaster
                var Restrictdata = ctxTFAT.UserRights.Where(x => x.MenuID == 1464 && x.Code == muserid).Select(x => x.xAdd).FirstOrDefault();
                mModel.VehicleRestrict = Restrictdata;
            }

            // FMSetup
            FMSetup fMSetup = ctxTFAT.FMSetup.FirstOrDefault();
            if (fMSetup == null)
            {
                fMSetup = new FMSetup();
            }
            mModel.FMSetup = fMSetup;
            mModel.AllowToChangeTDS = fMSetup.AllowToChgTDS;
            if (mModel.FMSetup.CurrDatetOnlyreq == false && mModel.FMSetup.BackDateAllow == false && mModel.FMSetup.ForwardDateAllow == false)
            {
                mModel.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
                mModel.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
            }
            if (mModel.FMSetup.CurrDatetOnlyreq == true)
            {
                mModel.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
                mModel.EndDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            if (mModel.FMSetup.BackDateAllow == true)
            {
                mModel.StartDate = (DateTime.Now.AddDays(-mModel.FMSetup.BackDaysUpto)).ToString("yyyy-MM-dd");
            }
            if (mModel.FMSetup.ForwardDateAllow == true)
            {
                mModel.EndDate = (DateTime.Now.AddDays(mModel.FMSetup.ForwardDaysUpto)).ToString("yyyy-MM-dd");
            }

            #endregion

            mModel.Branch = mbranchcode;
            GetAllMenu(Session["ModuleName"].ToString());
            connstring = GetConnectionString();
            //UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");

            if (mModel.getRecentFM == true || mModel.Mode == "Select" || mModel.Mode == "Add")
            {
                UpdateAuditTrail(mbranchcode, "Add", mModel.Header, null, DateTime.Now, 0, "", "", "NA");
            }
            else
            {
                var Document = ctxTFAT.FMMaster.Where(x => (x.TableKey.ToString() == mModel.Document)).FirstOrDefault();
                UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, Document.ParentKey, Document.Date, Document.Freight, Document.BroCode, "", "A");
            }



            mdocument = mModel.Document;
            mModel.Branches = PopulateBranches();
            DifferenceFmParameters differenceFm = new DifferenceFmParameters();
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete") || (mModel.getRecentFM == true))
            {
                FMMaster fMMastera = new FMMaster();

                if (mModel.getRecentFM)
                {
                    var Area = GetChildGrp(mbranchcode);
                    fMMastera = ctxTFAT.FMMaster.Where(x => x.AUTHIDS == muserid && Area.Contains(x.Branch)).OrderByDescending(x => x.RECORDKEY).FirstOrDefault();
                    mModel.Mode = "Add";
                }
                else
                {
                    fMMastera = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == mModel.Document).FirstOrDefault();
                    var listfmroute = ctxTFAT.FMROUTETable.Where(x => x.Parentkey == fMMastera.TableKey).ToList();
                    if (listfmroute.Where(x => x.ArrivalDate != null && x.SequenceRoute != 0 && x.SubRoute != 0).FirstOrDefault() != null)
                    {
                        mModel.BlockFM = true;
                        mModel.BlockFMMessage += "This FM Arrived In Another Branch So U Cant Edit Route And Schedule...!<br>";
                    }
                    else if (listfmroute.Where(x => x.LCNO != null && x.LCNO != "").FirstOrDefault() != null)
                    {
                        mModel.BlockFM = true;
                        mModel.BlockFMMessage += "In This FM Material Loaded So U Cant Edit Route And Schedule...!<br>";
                    }
                    else if (listfmroute.Where(x => x.ConsignmentLoad != null && x.ConsignmentLoad != "").FirstOrDefault() != null)
                    {
                        mModel.BlockFM = true;
                        mModel.BlockFMMessage += "In This FM Material Loaded(Direct)   So U Cant Edit Route And Schedule...!<br>";
                    }
                    else if (listfmroute.Where(x => x.UnLoadLCNO != null && x.UnLoadLCNO != "").FirstOrDefault() != null)
                    {
                        mModel.BlockFM = true;
                        mModel.BlockFMMessage += "This FM Material UnLoaded So U Cant Edit Route And Schedule...!<br>";
                    }

                    if (ctxTFAT.TripFmList.Where(x => x.RefTablekey == fMMastera.TableKey).FirstOrDefault() != null)
                    {
                        mModel.CheckMode = true;
                        mModel.Message += "Trip Generated Of this Freight Memo So U Cant Edit/Delete...!<br>";
                    }
                }

                if (fMMastera != null)
                {
                    //mModel.FMNO = Lcmaster.FmNo.ToString();
                    if (mModel.getRecentFM)
                    {
                        if (mModel.FMSetup.FMGenerate == true || mModel.FMSetup.FMBoth == true)
                        {
                            mModel.FMNO = "0";
                            mModel.FMGenerate = "A";
                        }
                        else
                        {
                            mModel.FMGenerate = "M";
                        }
                        mModel.Branch = fMMastera.Branch;
                        mModel.VehicleCategory = "";
                        mModel.VehicleNo = "";
                        mModel.FM_Date = DateTime.Now.ToShortDateString();
                        mModel.ArrivalDate = DateTime.Now.ToShortDateString();
                        mModel.DispatchDate = DateTime.Now.ToShortDateString();
                        mModel.DocDate = DateTime.Now;
                        mModel.FM_Time = DateTime.Now.ToString("HH:mm");
                        mModel.ArrivalTime = DateTime.Now.ToString("HH:mm");
                        mModel.DispatchTime = DateTime.Now.ToString("HH:mm");
                        mModel.KM = 0;
                        mModel.ArrivalKM = "0";
                        mModel.DispatchKM = "0";
                        mModel.fmCatchCharges = new List<FmCatchChargesInfo>();
                        Session["FMFreightAdvance"] = mModel.fmCatchCharges;
                    }
                    else
                    {
                        var categoryList1 = ctxTFAT.FmCatchChargesInfo.Where(x => x.FMRefTablekey == fMMastera.TableKey).ToList();

                        mModel.fmCatchCharges = categoryList1;
                        Session["FMFreightAdvance"] = mModel.fmCatchCharges;

                        mModel.PeriodLock = PeriodLock(fMMastera.Branch, "FM000", fMMastera.Date);
                        if (fMMastera.AUTHORISE.Substring(0, 1) == "A")
                        {
                            mModel.LockAuthorise = LockAuthorise("FM000", mModel.Mode, fMMastera.TableKey, fMMastera.ParentKey);
                        }

                        //Get Attachment
                        AttachmentVM Att = new AttachmentVM();
                        Att.Type = "FM000";
                        Att.Srl = fMMastera.FmNo.ToString();

                        AttachmentController attachmentC = new AttachmentController();
                        List<AttachmentVM> attachments = attachmentC.GetAttachmentListInEdit(Att);
                        Session["TempAttach"] = attachments;


                        mModel.FMNO = fMMastera.FmNo.ToString();
                        mModel.VehicleCategory = fMMastera.VehicleCategory;
                        mModel.VehicleCategory_Name = ctxTFAT.VehicleCategory.Where(x => x.Code == mModel.VehicleCategory).Select(x => x.VehicleCategory1).FirstOrDefault();
                        mModel.VehicleNo = fMMastera.TruckNo;
                        mModel.VehicleNoName = fMMastera.VehicleStatus == "100001" ? ctxTFAT.HireVehicleMaster.Where(x => x.Code == fMMastera.TruckNo).Select(x => x.TruckNo).FirstOrDefault() : ctxTFAT.VehicleMaster.Where(x => x.Code == fMMastera.TruckNo).Select(x => x.TruckNo).FirstOrDefault();
                        mModel.FM_Time = fMMastera.Time;
                        mModel.FM_Date = fMMastera.Date.ToShortDateString();

                        mModel.AllowToChangeVehicleCharges = fMMastera.VehicleStatus == "100001" ? ctxTFAT.HireVehicleMaster.Where(x => x.Code == fMMastera.TruckNo).Select(x => x.ChangeVehicleFreight_Advance).FirstOrDefault() : ctxTFAT.VehicleMaster.Where(x => x.Code == fMMastera.TruckNo).Select(x => x.ChangeVehicleFreight_Advance).FirstOrDefault();
                        mModel.AllowToChangeDriverCharges = fMMastera.VehicleStatus == "100001" ? ctxTFAT.HireVehicleMaster.Where(x => x.Code == fMMastera.TruckNo).Select(x => x.ChangeDriverFreight_Advance).FirstOrDefault() : ctxTFAT.VehicleMaster.Where(x => x.Code == fMMastera.TruckNo).Select(x => x.ChangeDriverFreight_Advance).FirstOrDefault();

                        mModel.DocDate = fMMastera.Date;
                        mModel.KM = (fMMastera.KM);
                        mModel.FMGenerate = fMMastera.GenerateType;
                        mModel.DieselAmt = fMMastera.DieselAmt;
                        mModel.DieselLtr = fMMastera.DieselLtr;

                    }

                    if (fMMastera.VehicleStatus == "100001")
                    {
                        mModel.DriverName = fMMastera.Driver;
                    }
                    else
                    {
                        mModel.DriverCode = fMMastera.Driver;
                        mModel.DriverNCombo = ctxTFAT.DriverMaster.Where(x => x.Code == fMMastera.Driver).Select(x => x.Name).FirstOrDefault();
                        mModel.MaintainDriAc = fMMastera.VehicleStatus == "100001" ? false : true;
                        mModel.FMVouRelReq = ctxTFAT.VehicleMaster.Where(x => x.Code == mModel.VehicleNo).Select(x => x.FMVOURELReq).FirstOrDefault();
                        mModel.FMPosting = ctxTFAT.VehicleMaster.Where(x => x.Code == mModel.VehicleNo).Select(x => x.PostReq).FirstOrDefault();
                        if (!String.IsNullOrEmpty(ctxTFAT.VehicleMaster.Where(x => x.Code == mModel.VehicleNo).Select(x => x.RateType).FirstOrDefault()))
                        {
                            mModel.CatchFreight = true;
                        }
                    }

                    mModel.TDSAmt = fMMastera.TDSAmt.Value;
                    mModel.TotAdvExp = fMMastera.TotlAdvExp;
                    mModel.TotExp = fMMastera.TotlExp;
                    mModel.Balance = fMMastera.Balance;

                    mModel.VehicleGroup = fMMastera.VehicleStatus;
                    mModel.VehicleGroup_Name = ctxTFAT.VehicleGrpStatusMas.Where(x => x.Code == mModel.VehicleGroup).Select(x => x.VehicleGroupStatus).FirstOrDefault();
                    mModel.Broker = fMMastera.BroCode;
                    mModel.Broker_Name = ctxTFAT.Master.Where(x => x.Code.ToString() == mModel.Broker).Select(x => x.Name).FirstOrDefault();
                    mModel.From = fMMastera.FromBranch;
                    mModel.From_Name = ctxTFAT.TfatBranch.Where(x => x.Code == fMMastera.FromBranch).Select(x => x.Name).FirstOrDefault();
                    mModel.From = fMMastera.FromBranch;
                    mModel.From_Name = ctxTFAT.TfatBranch.Where(x => x.Code == fMMastera.FromBranch).Select(x => x.Name).FirstOrDefault();
                    mModel.To = fMMastera.ToBranch;
                    mModel.To_Name = ctxTFAT.TfatBranch.Where(x => x.Code == fMMastera.ToBranch).Select(x => x.Name).FirstOrDefault();

                    #region Payload Details
                    mModel.PayLoad = (Convert.ToInt32(fMMastera.PayLoad)).ToString();
                    #endregion

                    mModel.ReceiptNo = (fMMastera.ReceiptNo);
                    mModel.DriverName = fMMastera.Driver;
                    mModel.LicenceNo = fMMastera.LicenCeNo;
                    mModel.LicenceExpDate = Convert.ToDateTime(fMMastera.LicenceExpDate).ToShortDateString();
                    mModel.Owner = fMMastera.OwnerName;
                    mModel.ChallanNo = fMMastera.ChallanNo;
                    mModel.ContactNo = (fMMastera.ContactNo);
                    mModel.Freight = fMMastera.Freight;
                    mModel.Advance = fMMastera.Adv;
                    mModel.TripFreight = fMMastera.TripFreight;
                    mModel.TripAdvance = fMMastera.TripAdvance;
                    mModel.PayableAt = fMMastera.PayAt;
                    mModel.PayableAt_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.PayableAt).Select(x => x.Name).FirstOrDefault();
                    mModel.Remark = fMMastera.Remark;
                    mModel.Balance = fMMastera.Balance;
                    mModel.AppBranch = fMMastera.RouteVia;


                    List<FMROUTETable> FMROUTETables = ctxTFAT.FMROUTETable.Where(x => x.Parentkey == fMMastera.TableKey).OrderBy(x => x.SubRoute).ToList();
                    List<RouteDetails> routeDetails = new List<RouteDetails>();
                    foreach (var item in FMROUTETables)
                    {
                        RouteDetails routeDetail = new RouteDetails();
                        routeDetail.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == item.RouteVia).Select(x => x.Name).FirstOrDefault();
                        routeDetail.ArrivalSchDate = item.ArrivalSchDate == null ? "" : item.ArrivalSchDate.Value.ToShortDateString();
                        routeDetail.ArrivalSchTime = item.ArrivalSchTime == null ? "" : item.ArrivalSchTime;
                        routeDetail.ArrivalSchKm = item.ArrivalSchKm == null ? "" : item.ArrivalSchKm.Value.ToString();
                        routeDetail.ArrivalReSchDate = item.ArrivalReSchDate == null ? "" : item.ArrivalReSchDate.Value.ToShortDateString();
                        routeDetail.ArrivalReSchTime = item.ArrivalReSchTime == null ? "" : item.ArrivalReSchTime;
                        routeDetail.ArrivalReSchKm = item.ArrivalReSchKm == null ? "" : item.ArrivalReSchKm.Value.ToString();
                        routeDetail.ArrivalDate = item.ArrivalDate == null ? "" : item.ArrivalDate.Value.ToShortDateString();
                        routeDetail.ArrivalTime = item.ArrivalTime == null ? "" : item.ArrivalTime;
                        routeDetail.ArrivalKM = item.ArrivalKM == null ? "" : item.ArrivalKM.Value.ToString();
                        routeDetail.ArrivalLateTime = "0";
                        routeDetail.DispatchLateTime = "0";

                        if ((!String.IsNullOrEmpty(routeDetail.ArrivalSchDate)) && (!String.IsNullOrEmpty(routeDetail.ArrivalDate)))
                        {
                            var Date = routeDetail.ArrivalSchDate.Split('/');
                            var Date1 = routeDetail.ArrivalSchTime.Split(':');
                            DateTime arrivalScheduleDate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Date1[0]), Convert.ToInt32(Date1[1]), 00);

                            Date = routeDetail.ArrivalDate.Split('/');
                            Date1 = routeDetail.ArrivalTime.Split(':');
                            DateTime arrivalDate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Date1[0]), Convert.ToInt32(Date1[1]), 00);

                            if (arrivalScheduleDate >= arrivalDate)
                            {
                                TimeSpan ts = arrivalScheduleDate - arrivalDate;
                                var TotalMinutes = ts.TotalMinutes;

                                if (TotalMinutes == 0)
                                {
                                    //routeDetail.ArrivalLateTime = "Vehicle On Time. ";
                                    routeDetail.ArrivalLateTime = "0 ";
                                }
                                else
                                {
                                    //routeDetail.ArrivalLateTime = "Vehicle Reach Early " + Math.Round((TotalMinutes / 60)) + " Hours and " + (TotalMinutes % 60) + " Minutes.";
                                    routeDetail.ArrivalLateTime = "  " + Math.Round((TotalMinutes / 60)) + " : " + (TotalMinutes % 60);
                                }

                            }
                            else
                            {
                                TimeSpan ts = arrivalDate - arrivalScheduleDate;
                                var TotalMinutes = ts.TotalMinutes;
                                //routeDetail.ArrivalLateTime = "Vehicle Reachd Late " + Math.Round((TotalMinutes / 60)) + " Hours and " + (TotalMinutes % 60) + " Minutes.";
                                routeDetail.ArrivalLateTime = " - " + Math.Round((TotalMinutes / 60)) + " : " + (TotalMinutes % 60);
                            }
                        }

                        routeDetail.DispatchSchDate = item.DispatchSchDate == null ? "" : item.DispatchSchDate.Value.ToShortDateString();
                        routeDetail.DispatchSchTime = item.DispatchSchTime == null ? "" : item.DispatchSchTime;
                        routeDetail.DispatchReSchDate = item.DispatchReSchDate == null ? "" : item.DispatchReSchDate.Value.ToShortDateString();
                        routeDetail.DispatchReSchTime = item.DispatchReSchTime == null ? "" : item.DispatchReSchTime;
                        routeDetail.DispatchDate = item.DispatchDate == null ? "" : item.DispatchDate.Value.ToShortDateString();
                        routeDetail.DispatchTime = item.DispatchTime == null ? "" : item.DispatchTime;
                        routeDetail.DispatchKM = item.DispatchKM == null ? "" : item.DispatchKM.Value.ToString();




                        if ((!String.IsNullOrEmpty(routeDetail.DispatchSchDate)) && (!String.IsNullOrEmpty(routeDetail.DispatchDate)))
                        {
                            var Date = routeDetail.DispatchSchDate.Split('/');
                            var Date1 = routeDetail.DispatchSchTime.Split(':');
                            DateTime dispatchScheduleDate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Date1[0]), Convert.ToInt32(Date1[1]), 00);

                            Date = routeDetail.DispatchDate.Split('/');
                            Date1 = routeDetail.DispatchTime.Split(':');
                            DateTime dispachDate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Date1[0]), Convert.ToInt32(Date1[1]), 00);

                            if (dispatchScheduleDate >= dispachDate)
                            {
                                TimeSpan ts = dispatchScheduleDate - dispachDate;
                                var TotalMinutes = ts.TotalMinutes;

                                if (TotalMinutes == 0)
                                {
                                    //routeDetail.DispatchLateTime = "Vehicle Move On Time. ";
                                    routeDetail.DispatchLateTime = " 0 ";
                                }
                                else
                                {
                                    //routeDetail.DispatchLateTime = "Vehicle Move Early " + Math.Round((TotalMinutes / 60)) + " Hours and " + (TotalMinutes % 60) + " Minutes.";
                                    routeDetail.DispatchLateTime = "  " + Math.Round((TotalMinutes / 60)) + " : " + (TotalMinutes % 60);
                                }

                            }
                            else
                            {
                                TimeSpan ts = dispachDate - dispatchScheduleDate;
                                var TotalMinutes = ts.TotalMinutes;
                                var ff = ts.TotalHours;
                                //routeDetail.DispatchLateTime = "Vehicle Move Late " + Math.Round((TotalMinutes / 60)) + " Hours and " + (TotalMinutes % 60) + " Minutes.";
                                routeDetail.DispatchLateTime = " - " + Math.Round((TotalMinutes / 60)) + " : " + (TotalMinutes % 60);
                            }
                        }



                        routeDetails.Add(routeDetail);
                    }
                    mModel.ViewSchedule = routeDetails;


                    List<RouteDetails> routeDetails1 = new List<RouteDetails>();
                    var FinalRoute = FMROUTETables.Where(x => x.RouteType == "R").OrderByDescending(x => x.SequenceRoute).Select(x => x.SequenceRoute).FirstOrDefault();
                    int oi = 1;

                    RouteDetails route = new RouteDetails();
                    route.Tempid = oi;
                    route.Branch = fMMastera.FromBranch;
                    route.BranchN = ctxTFAT.TfatBranch.Where(x => x.Code == fMMastera.FromBranch).Select(x => x.Name).FirstOrDefault();
                    route.AllowToChange = "F";
                    ++oi;
                    routeDetails1.Add(route);

                    foreach (var item in FMROUTETables)
                    {
                        route = new RouteDetails();
                        if (item.RouteVia != fMMastera.FromBranch && item.RouteVia != fMMastera.ToBranch && item.SequenceRoute != null)
                        {
                            route.Tempid = oi;
                            route.SequenceRoute = item.SequenceRoute.Value;
                            route.Branch = item.RouteVia;
                            route.BranchN = ctxTFAT.TfatBranch.Where(x => x.Code == item.RouteVia).Select(x => x.Name).FirstOrDefault();
                            route.AllowToChange = "T";
                            ++oi;
                            routeDetails1.Add(route);
                        }

                    }
                    route = new RouteDetails();
                    route.Tempid = oi;
                    route.Branch = fMMastera.ToBranch;
                    route.BranchN = ctxTFAT.TfatBranch.Where(x => x.Code == fMMastera.ToBranch).Select(x => x.Name).FirstOrDefault();
                    route.AllowToChange = "F";
                    ++oi;
                    routeDetails1.Add(route);

                    mModel.AllroutelistChange = routeDetails1;
                    Session["GridDataSession"] = routeDetails1;

                    #region scheduleCalculators
                    List<ScheduleCalculator> scheduleCalculators = new List<ScheduleCalculator>();
                    for (int i = 0; i < FMROUTETables.Count(); i++)
                    {
                        var item = FMROUTETables[i];
                        ScheduleCalculator schedule = new ScheduleCalculator();

                        if (item.SequenceRoute != 0 || item.SubRoute != 0)
                        {
                            var item1 = FMROUTETables[i - 1];
                            schedule.FromName = ctxTFAT.TfatBranch.Where(x => x.Code == item1.RouteVia).Select(x => x.Name).FirstOrDefault();
                        }
                        schedule.ToName = ctxTFAT.TfatBranch.Where(x => x.Code == item.RouteVia).Select(x => x.Name).FirstOrDefault();
                        schedule.ActivityTime = item.VehicleActivity == null ? "0:0" : item.VehicleActivity;
                        schedule.RunningKM = item.Kilometers == null ? 0 : (int)item.Kilometers;
                        schedule.RunningTime = item.KilometersTime == null ? "0.0" : item.KilometersTime;
                        scheduleCalculators.Add(schedule);
                    }

                    List<int> RunnHours = new List<int>();
                    List<int> RunnMinutes = new List<int>();
                    List<int> ActivityHours = new List<int>();
                    List<int> ActivityMinutes = new List<int>();


                    foreach (var item in scheduleCalculators)
                    {
                        var Dummy = item.ActivityTime.Split(':');
                        ActivityHours.Add(Convert.ToInt32(Dummy[0]));
                        ActivityMinutes.Add(Convert.ToInt32(Dummy[1]));
                        Dummy = item.RunningTime.Split('.');
                        RunnHours.Add(Convert.ToInt32(Dummy[0]));
                        RunnMinutes.Add(Convert.ToInt32(Dummy[1]));
                    }
                    mModel.GrandKM = scheduleCalculators.Sum(x => x.RunningKM);
                    mModel.GrandActivity = (ActivityHours.Sum() + (ActivityMinutes.Sum() / 60)).ToString("D2") + ":" + ((ActivityMinutes.Sum() % 60)).ToString("D2");
                    mModel.GrandTime = (RunnHours.Sum() + (RunnMinutes.Sum() / 60)) + ":" + ((RunnMinutes.Sum() % 60));

                    mModel.scheduleCalculators = scheduleCalculators;
                    #endregion

                    #region GetAll RouteVia
                    if (!(String.IsNullOrEmpty(fMMastera.RouteVia)))
                    {
                        List<LR_LC_Combine_VM> lR_LC_Combine_VMs = new List<LR_LC_Combine_VM>();
                        var GetSourceArry = fMMastera.RouteVia.Split(',');

                        for (int i = 0; i < GetSourceArry.Length; i++)
                        {
                            var SourceCode = GetSourceArry[i];


                            var SourceName = ctxTFAT.TfatBranch.Where(x => x.Code == SourceCode).Select(x => x.Name).FirstOrDefault();
                            if (!(String.IsNullOrEmpty(SourceName)))
                            {
                                LR_LC_Combine_VM lR_LC_Combine_VM = new LR_LC_Combine_VM
                                {
                                    Consigner = SourceName,
                                    From = SourceCode,
                                };
                                lR_LC_Combine_VMs.Add(lR_LC_Combine_VM);
                            }

                        }
                        TempData["Destination"] = lR_LC_Combine_VMs;
                        mModel.AllDest = lR_LC_Combine_VMs;
                    }
                    #endregion

                    #region Charges

                    List<PurchaseVM> objledgerdetail = new List<PurchaseVM>();
                    var trncharges = ctxTFAT.Charges.Where(x => x.Type == "FM000" && x.DontUse == false).Select(x => new { x.Fld, x.Head, x.EqAmt, x.Equation, x.Code }).ToList();
                    //int fmno = Convert.ToInt32(fMMastera.FmNo);
                    foreach (var i in trncharges)
                    {
                        PurchaseVM c = new PurchaseVM();
                        c.Fld = i.Fld;
                        c.Code = i.Head;
                        c.AddLess = i.EqAmt;
                        c.Equation = i.Equation;
                        c.tempId = Convert.ToInt16(i.Fld.Substring(1, 3));
                        if (mModel.Mode != "Add")
                        {
                            c.ColVal = GetChargeValValue(c.tempId, "ChgAmt", fMMastera.TableKey);
                            c.ValueLast = GetChargeValValue(c.tempId, "ChgAmt", fMMastera.TableKey);
                            c.Amt1 = Convert.ToDecimal((c.ValueLast == "" || c.ValueLast == null) ? "0" : c.ValueLast);
                        }
                        else
                        {
                            c.ColVal = "0";
                        }
                        c.ChgPostCode = i.Code;
                        objledgerdetail.Add(c);
                    }
                    mModel.Charges = objledgerdetail;

                    #endregion

                    if (fMMastera.ScheduleRequired)
                    {
                        mModel.ScheduleRequired = fMMastera.ScheduleRequired;
                        FMROUTETable fM_ROUTE_ = ctxTFAT.FMROUTETable.Where(x => x.Parentkey == fMMastera.TableKey && x.SequenceRoute == 0).FirstOrDefault();
                        mModel.ArrivalDate = fM_ROUTE_.ArrivalDate == null ? "" : fM_ROUTE_.ArrivalDate.Value.ToShortDateString();
                        mModel.ArrivalTime = fM_ROUTE_.ArrivalTime == null ? "" : fM_ROUTE_.ArrivalTime;
                        mModel.ArrivalKM = fM_ROUTE_.ArrivalKM == null ? "0" : fM_ROUTE_.ArrivalKM.Value.ToString();
                        mModel.DispatchDate = fM_ROUTE_.DispatchDate == null ? "" : fM_ROUTE_.DispatchDate.Value.ToShortDateString();
                        mModel.DispatchTime = fM_ROUTE_.DispatchTime == null ? "" : fM_ROUTE_.DispatchTime;
                        mModel.DispatchKM = fM_ROUTE_.DispatchKM == null ? "0" : fM_ROUTE_.DispatchKM.Value.ToString();
                        mModel.ArrivalRemark = fM_ROUTE_.ArrivalRemark;
                    }
                    if (fMMastera.VehicleStatus != "100001")
                    {
                        differenceFm.OldFmPosting = fMMastera.VehicleFmPostReq;
                        differenceFm.OldMaintainCreditorPayRecord = fMMastera.VehicleArAp;
                        differenceFm.OldMaintainDriverAc = fMMastera.VehicleMaintainDriverAC;
                        differenceFm.OldDriverPosting = fMMastera.DriverPostAc;
                        differenceFm.OldDriverPostingN = ctxTFAT.Master.Where(x => x.Code == fMMastera.DriverPostAc).Select(x => x.Name).FirstOrDefault();
                        differenceFm.OldVehiclePosting = fMMastera.VehiclePostAc;
                        differenceFm.OldVehiclePostingN = ctxTFAT.Master.Where(x => x.Code == fMMastera.VehiclePostAc).Select(x => x.Name).FirstOrDefault();

                        VehicleMaster vehicle = ctxTFAT.VehicleMaster.Where(x => x.Code.Trim().ToLower() == fMMastera.TruckNo.Trim().ToLower()).FirstOrDefault();
                        differenceFm.NewFmPosting = vehicle.PostReq;
                        differenceFm.NewMaintainCreditorPayRecord = vehicle.ARAP;
                        differenceFm.NewMaintainDriverAc = vehicle.MaintainDriverAC;
                        differenceFm.NewVehiclePosting = vehicle.PostAc;
                        differenceFm.NEwVehiclePostingN = ctxTFAT.Master.Where(x => x.Code == vehicle.PostAc).Select(x => x.Name).FirstOrDefault();

                        DriverMaster driverMaster = ctxTFAT.DriverMaster.Where(x => x.Code == fMMastera.Driver).FirstOrDefault();
                        differenceFm.NewDriverPosting = driverMaster.Posting;
                        differenceFm.NEwDriverPostingN = ctxTFAT.Master.Where(x => x.Code == driverMaster.Posting).Select(x => x.Name).FirstOrDefault();
                    }

                }
                else
                {
                    mModel = FreshModal(mModel);
                }
                var tdsdetails = ctxTFAT.TDSPayments.Where(x => x.Type == "FM000" && x.TableKey == mModel.Document).Select(x => x).FirstOrDefault();
                if (fMMastera.VehicleStatus != "100001")
                {
                    if (fMSetup.ATDSAdjReq == true || fMSetup.OTDSAdjReq == true)
                    {
                        mModel.CutTDS = (tdsdetails != null) ? true : false;
                    }

                }
                else
                {
                    if (fMSetup.HTDSAdjReq)
                    {
                        mModel.CutTDS = (tdsdetails != null) ? true : false;
                    }
                }
                mModel.TDSAmt = (tdsdetails != null) ? tdsdetails.TDSAmt.Value : 0;
                mModel.TDSRate = (tdsdetails != null) ? tdsdetails.TDSTax.Value : 0;
                mModel.TDSCode = (tdsdetails != null) ? tdsdetails.TDSCode.Value : 0;

                var GetLedger = ctxTFAT.Ledger.Where(x => x.ParentKey == fMMastera.ParentKey).Select(x => x.TableKey).ToList();
                if (GetLedger != null)
                {
                    var advpaydetails = ctxTFAT.VoucherDetail.Where(x => GetLedger.Contains(x.FMTableKey.ToString())).Select(x => x).FirstOrDefault();
                    if (advpaydetails != null)
                    {
                        mModel.CheckMode = true;

                        mModel.Message += "Document is Already Adjusted Against Advance Payment: " + advpaydetails.ParentKey + ", Cant Edit or Delete.<br>";
                    }
                }

                var AdjustFMCashBannkTrn = ctxTFAT.RelFm.Where(x => x.FMRefTablekey == fMMastera.TableKey).FirstOrDefault();
                if (AdjustFMCashBannkTrn != null)
                {
                    mModel.CheckMode = true;

                    mModel.Message += " Document is Found In Expense File '" + (ctxTFAT.TfatBranch.Where(x => x.Code == AdjustFMCashBannkTrn.Branch).Select(x => x.Name).FirstOrDefault()) + " , Cant " + mModel.Mode;

                }

            }
            else if ((mModel.Mode == "Select"))
            {
                mModel.fmCatchCharges = fmCatchCharges;
                mModel.Branch = mbranchcode;
                var fMMaster_Draft = ctxTFAT.FMMasterDraft.Where(x => x.FmNo.ToString() == mModel.Document).FirstOrDefault();
                if (fMMaster_Draft != null)
                {
                    if (mModel.FMSetup.FMGenerate == true || mModel.FMSetup.FMBoth == true)
                    {
                        mModel.FMNO = "0";
                        mModel.FMGenerate = "A";
                    }
                    else
                    {
                        mModel.FMGenerate = "M";
                    }
                    mModel.Mode = "Add";
                    mModel.DocDate = DateTime.Now;
                    mModel.FM_Time = DateTime.Now.ToString("HH:mm");
                    mModel.ArrivalTime = DateTime.Now.ToString("HH:mm");
                    mModel.DispatchTime = DateTime.Now.ToString("HH:mm");
                    mModel.FM_Date = DateTime.Now.ToShortDateString();
                    mModel.ArrivalDate = DateTime.Now.ToShortDateString();
                    mModel.DispatchDate = DateTime.Now.ToShortDateString();
                    mModel.VehicleGroup = fMMaster_Draft.VehicleStatus;
                    mModel.VehicleGroup_Name = ctxTFAT.VehicleGrpStatusMas.Where(x => x.Code == mModel.VehicleGroup).Select(x => x.VehicleGroupStatus).FirstOrDefault();
                    if (fMMaster_Draft.VehicleStatus == "100001")
                    {
                        mModel.DriverName = fMMaster_Draft.Driver;
                    }
                    else
                    {
                        mModel.DriverCode = fMMaster_Draft.Driver;
                        mModel.DriverNCombo = ctxTFAT.DriverMaster.Where(x => x.Code == fMMaster_Draft.Driver).Select(x => x.Name).FirstOrDefault();
                    }
                    mModel.From = fMMaster_Draft.FromBranch;
                    mModel.From_Name = ctxTFAT.TfatBranch.Where(x => x.Code == fMMaster_Draft.FromBranch).Select(x => x.Name).FirstOrDefault();
                    mModel.To = fMMaster_Draft.ToBranch;
                    mModel.To_Name = ctxTFAT.TfatBranch.Where(x => x.Code == fMMaster_Draft.ToBranch).Select(x => x.Name).FirstOrDefault();
                    mModel.ReceiptNo = (fMMaster_Draft.ReceiptNo);
                    mModel.ChallanNo = (fMMaster_Draft.ChallanNo);
                    mModel.ContactNo = (fMMaster_Draft.ContactNo);
                    mModel.Freight = Convert.ToDecimal(fMMaster_Draft.Freight);
                    mModel.Advance = Convert.ToDecimal(fMMaster_Draft.Adv);
                    mModel.PayableAt = fMMaster_Draft.PayAt;
                    mModel.PayableAt_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.PayableAt).Select(x => x.Name).FirstOrDefault();
                    mModel.Remark = fMMaster_Draft.Remark;
                    mModel.Balance = Convert.ToDecimal(fMMaster_Draft.Balance);
                    mModel.TDSAmt = fMMaster_Draft.TDSAmt.Value;
                    mModel.TotAdvExp = fMMaster_Draft.TotlAdvExp;
                    mModel.TotExp = fMMaster_Draft.TotlExp;
                    mModel.Balance = fMMaster_Draft.Balance.Value;
                    #region GetAll RouteVia


                    if (!(String.IsNullOrEmpty(fMMaster_Draft.RouteVia)))
                    {
                        List<LR_LC_Combine_VM> lR_LC_Combine_VMs = new List<LR_LC_Combine_VM>();
                        var GetSourceArry = fMMaster_Draft.RouteVia.Split(',');

                        for (int i = 0; i < GetSourceArry.Length; i++)
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
                        TempData["Destination"] = lR_LC_Combine_VMs;
                        mModel.AllDest = lR_LC_Combine_VMs;
                    }


                    #endregion

                    #region Charges

                    List<PurchaseVM> objledgerdetail = new List<PurchaseVM>();
                    var trncharges = ctxTFAT.Charges.Where(x => x.Type == "FM000" && x.DontUse == false).Select(x => new { x.Fld, x.Head, x.EqAmt, x.Equation, x.Code }).ToList();
                    int fmno = Convert.ToInt32(mModel.FMNO);
                    foreach (var i in trncharges)
                    {
                        PurchaseVM c = new PurchaseVM();
                        c.Fld = i.Fld;
                        c.Code = i.Head;
                        c.AddLess = i.EqAmt;
                        c.Equation = i.Equation;
                        c.tempId = Convert.ToInt16(i.Fld.Substring(1, 3));
                        if (mModel.Mode != "Add")
                        {
                            c.ColVal = GetChargeValValue(c.tempId, "ChgAmt", mModel.Document);
                            c.ValueLast = GetChargeValValue(c.tempId, "ChgAmt", mModel.Document);
                            c.Amt1 = Convert.ToDecimal((c.ValueLast == "" || c.ValueLast == null) ? "0" : c.ValueLast);
                        }
                        else
                        {
                            c.ColVal = "0";
                        }
                        c.ChgPostCode = i.Code;
                        objledgerdetail.Add(c);
                    }
                    mModel.Charges = objledgerdetail;

                    #endregion
                }
            }
            else
            {
                mModel = FreshModal(mModel);
            }

            if (mModel.FMSetup == null)
            {
                mModel.FMSetup = new FMSetup();
            }

            mModel.differenceFm = differenceFm;

            List<GridOption> Grlist = new List<GridOption>();
            var list = ctxTFAT.DocFormats.Where(x => x.Type == "FM000" && x.OutputDevice != "H").Select(x => x).ToList();
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

        public ActionResult DraftIndex(FMVM mModel)
        {

            #region Defalut Load

            #region Setup Logistics Flow
            mModel.LogisticsFlow = ctxTFAT.LogisticsFlow.FirstOrDefault();
            if (mModel.LogisticsFlow == null)
            {
                mModel.LogisticsFlow = new LogisticsFlow();
            }
            #endregion
            #region Restriction Of VehicleMaster
            mModel.VehicleRestrict = true;
            if (muserid.ToUpper() != "SUPER")
            {
                //1464 VehicleMaster
                var Restrictdata = ctxTFAT.UserRights.Where(x => x.MenuID == 1464 && x.Code == muserid).Select(x => x.xAdd).FirstOrDefault();
                mModel.VehicleRestrict = Restrictdata;
            }
            #endregion
            #region FMSetup
            mModel.FMSetup = ctxTFAT.FMSetup.FirstOrDefault();
            #endregion
            #endregion

            GetAllMenu(Session["ModuleName"].ToString());
            connstring = GetConnectionString();
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;
            mModel.Branches = PopulateBranches();
            DifferenceFmParameters differenceFm = new DifferenceFmParameters();
            FMMasterDraft fMMastera = new FMMasterDraft();
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete") || (mModel.getRecentFM == true))
            {
                fMMastera = ctxTFAT.FMMasterDraft.Where(x => x.FmNo.ToString() == mModel.Document).FirstOrDefault();

                if (fMMastera != null)
                {

                    mModel.FMNO = fMMastera.FmNo.ToString();
                    mModel.VehicleCategory = fMMastera.VehicleCategory;
                    mModel.VehicleCategory_Name = ctxTFAT.VehicleCategory.Where(x => x.Code == mModel.VehicleCategory).Select(x => x.VehicleCategory1).FirstOrDefault();
                    mModel.VehicleNo = fMMastera.TruckNo;
                    mModel.VehicleNoName = fMMastera.VehicleStatus == "100001" ? ctxTFAT.HireVehicleMaster.Where(x => x.Code == fMMastera.TruckNo).Select(x => x.TruckNo).FirstOrDefault() : ctxTFAT.VehicleMaster.Where(x => x.Code == fMMastera.TruckNo).Select(x => x.TruckNo).FirstOrDefault();

                    mModel.FM_Time = fMMastera.Time;
                    mModel.FM_Date = fMMastera.Date.ToShortDateString();
                    mModel.KM = fMMastera.KM.Value;
                    mModel.DocDate = fMMastera.Date;
                    mModel.FMGenerate = "M";
                    if (fMMastera.VehicleStatus == "100001")
                    {
                        mModel.DriverName = fMMastera.Driver;
                    }
                    else
                    {
                        mModel.DriverCode = fMMastera.Driver;
                        mModel.DriverNCombo = ctxTFAT.DriverMaster.Where(x => x.Code == fMMastera.Driver).Select(x => x.Name).FirstOrDefault();
                        mModel.MaintainDriAc = fMMastera.VehicleStatus == "" ? false : true;
                        mModel.MaintainDriAc = fMMastera.VehicleStatus == "100001" ? false : true;
                    }

                    mModel.TDSAmt = fMMastera.TDSAmt.Value;
                    mModel.TotAdvExp = fMMastera.TotlAdvExp;
                    mModel.TotExp = fMMastera.TotlExp;
                    mModel.Balance = fMMastera.Balance.Value;

                    mModel.VehicleGroup = fMMastera.VehicleStatus;
                    mModel.VehicleGroup_Name = ctxTFAT.VehicleGrpStatusMas.Where(x => x.Code == mModel.VehicleGroup).Select(x => x.VehicleGroupStatus).FirstOrDefault();
                    mModel.Broker = fMMastera.BroCode;
                    mModel.Broker_Name = ctxTFAT.Master.Where(x => x.Code.ToString() == mModel.Broker).Select(x => x.Name).FirstOrDefault();
                    mModel.From = fMMastera.FromBranch;
                    mModel.From_Name = ctxTFAT.TfatBranch.Where(x => x.Code == fMMastera.FromBranch).Select(x => x.Name).FirstOrDefault();
                    mModel.From = fMMastera.FromBranch;
                    mModel.From_Name = ctxTFAT.TfatBranch.Where(x => x.Code == fMMastera.FromBranch).Select(x => x.Name).FirstOrDefault();
                    mModel.To = fMMastera.ToBranch;
                    mModel.To_Name = ctxTFAT.TfatBranch.Where(x => x.Code == fMMastera.ToBranch).Select(x => x.Name).FirstOrDefault();

                    #region Payload Details
                    mModel.PayLoad = (Convert.ToInt32(fMMastera.PayLoad)).ToString();
                    #endregion

                    mModel.ReceiptNo = (fMMastera.ReceiptNo);
                    mModel.Draft_Name = fMMastera.DraftName;
                    mModel.DriverName = fMMastera.Driver;
                    mModel.LicenceNo = fMMastera.LicenCeNo;
                    mModel.LicenceExpDate = Convert.ToDateTime(fMMastera.LicenceExpDate).ToShortDateString();
                    mModel.Owner = fMMastera.OwnerName;
                    mModel.ChallanNo = fMMastera.ChallanNo;
                    mModel.ContactNo = (fMMastera.ContactNo);
                    mModel.Freight = fMMastera.Freight.Value;
                    mModel.Advance = fMMastera.Adv.Value;
                    mModel.PayableAt = fMMastera.PayAt;
                    mModel.PayableAt_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.PayableAt).Select(x => x.Name).FirstOrDefault();
                    mModel.Remark = fMMastera.Remark;
                    mModel.Balance = fMMastera.Balance.Value;
                    mModel.AppBranch = fMMastera.RouteVia;

                    #region GetAll RouteVia
                    if (!(String.IsNullOrEmpty(fMMastera.RouteVia)))
                    {
                        List<LR_LC_Combine_VM> lR_LC_Combine_VMs = new List<LR_LC_Combine_VM>();
                        var GetSourceArry = fMMastera.RouteVia.Split(',');

                        for (int i = 0; i < GetSourceArry.Length; i++)
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
                        TempData["Destination"] = lR_LC_Combine_VMs;
                        mModel.AllDest = lR_LC_Combine_VMs;
                    }
                    #endregion

                    #region Charges

                    List<PurchaseVM> objledgerdetail = new List<PurchaseVM>();
                    var trncharges = ctxTFAT.Charges.Where(x => x.Type == "FM000" && x.DontUse == false).Select(x => new { x.Fld, x.Head, x.EqAmt, x.Equation, x.Code }).ToList();
                    //int fmno = Convert.ToInt32(fMMastera.FmNo);
                    foreach (var i in trncharges)
                    {
                        PurchaseVM c = new PurchaseVM();
                        c.Fld = i.Fld;
                        c.Code = i.Head;
                        c.AddLess = i.EqAmt;
                        c.Equation = i.Equation;
                        c.tempId = Convert.ToInt16(i.Fld.Substring(1, 3));
                        if (mModel.Mode != "Add")
                        {
                            c.ColVal = GetChargeValValueDraft(c.tempId, "ChgAmt", fMMastera.FmNo);
                            c.ValueLast = GetChargeValValueDraft(c.tempId, "ChgAmt", fMMastera.FmNo);
                            c.Amt1 = Convert.ToDecimal((c.ValueLast == "" || c.ValueLast == null) ? "0" : c.ValueLast);
                        }
                        else
                        {
                            c.ColVal = "0";
                        }
                        c.ChgPostCode = i.Code;
                        objledgerdetail.Add(c);
                    }
                    mModel.Charges = objledgerdetail;

                    #endregion
                }
            }
            else
            {
                mModel = FreshModal(mModel);
                mModel.FMNO = GetNewCodeDraft();
            }

            if (mModel.FMSetup == null)
            {
                mModel.FMSetup = new FMSetup();
            }

            return View(mModel);
        }

        public FMVM FreshModal(FMVM mModel)
        {
            List<LCModal> lCModals = new List<LCModal>();
            TempData["ExistLC"] = lCModals;
            mModel.LClist = lCModals;
            mModel.UnLClist = lCModals;

            mModel.fmCatchCharges = fmCatchCharges;
            var getdetailsOfCurrentBRanch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).FirstOrDefault();
            if (getdetailsOfCurrentBRanch.Category != "0" || getdetailsOfCurrentBRanch.Category != "Zone")
            {
                mModel.From = getdetailsOfCurrentBRanch.Code;
                mModel.From_Name = getdetailsOfCurrentBRanch.Name;
                mModel.PayableAt = getdetailsOfCurrentBRanch.Code;
                mModel.PayableAt_Name = getdetailsOfCurrentBRanch.Name;
            }
            if (mModel.FMSetup != null)
            {
                if (mModel.FMSetup.FMBoth == true || mModel.FMSetup.FMGenerate == true)
                {
                    mModel.FMNO = "0";
                    mModel.FMGenerate = "A";
                }
                else
                {
                    mModel.FMGenerate = "M";
                }

                if (mModel.FMSetup.ExcludeHire)
                {
                    if (mModel.FMSetup.FmType != "100001")
                    {
                        mModel.VehicleGroup = mModel.FMSetup.FmType;
                        mModel.VehicleGroup_Name = ctxTFAT.VehicleGrpStatusMas.Where(x => x.Code == mModel.VehicleGroup).Select(x => x.VehicleGroupStatus).FirstOrDefault();
                    }
                }

            }
            mModel.Branch = mbranchcode;
            mModel.FM_Date = DateTime.Now.ToShortDateString().ToString();
            mModel.ArrivalDate = DateTime.Now.ToShortDateString().ToString();
            mModel.DispatchDate = DateTime.Now.ToShortDateString().ToString();
            mModel.FM_Time = DateTime.Now.ToString("HH:mm");
            mModel.ArrivalTime = DateTime.Now.ToString("HH:mm");
            mModel.DispatchTime = DateTime.Now.ToString("HH:mm");
            //mModel.Time = DateTime.Now.ToString("HH:mm");
            mModel.KM = 0;
            mModel.ArrivalKM = "0";
            mModel.DispatchKM = "0";
            mModel.DocDate = DateTime.Now;

            #region Charges

            List<PurchaseVM> objledgerdetail = new List<PurchaseVM>();
            var trncharges = ctxTFAT.Charges.Where(x => x.Type == "FM000" && x.DontUse == false).Select(x => new { x.Fld, x.Head, x.EqAmt, x.Equation, x.Code }).ToList();
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
            mModel.Charges = objledgerdetail;

            #endregion





            return mModel;
        }

        #endregion

        #region Save && Delete FM

        public void DeUpdate(FMVM Model)
        {
            FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey == Model.Document).FirstOrDefault();
            var ledger = ctxTFAT.Ledger.Where(x => x.ParentKey == fMMaster.ParentKey).ToList();
            ctxTFAT.Ledger.RemoveRange(ledger);
            var BillSubRef = ctxTFAT.FmCatchChargesInfo.Where(x => x.FMRefTablekey.ToString() == Model.Document).ToList();
            ctxTFAT.FmCatchChargesInfo.RemoveRange(BillSubRef);
            var Fmvourel = ctxTFAT.FMVouRel.Where(x => x.FMNo == Model.FMNO).ToList();
            ctxTFAT.FMVouRel.RemoveRange(Fmvourel);
            var tdspayment = ctxTFAT.TDSPayments.Where(x => x.ParentKey == fMMaster.ParentKey).Select(x => x).FirstOrDefault();
            var Outstandingdel = ctxTFAT.Outstanding.Where(x => x.ParentKey == fMMaster.ParentKey).Select(x => x).ToList();
            if (tdspayment != null)
            {
                ctxTFAT.TDSPayments.Remove(tdspayment);
            }
            ctxTFAT.Outstanding.RemoveRange(Outstandingdel);
            ctxTFAT.SaveChanges();
        }

        public ActionResult SaveData(FMVM mModel)
        {

            //bool Status = false;
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
                    TfatVehicleStatus tfatVehicleStatus = new TfatVehicleStatus();
                    tfatVehicleStatusHistory vehicleDri_Hist = new tfatVehicleStatusHistory();
                    FMMaster fMMaster = new FMMaster();
                    VehicleMaster vehicleMaster = new VehicleMaster();
                    HireVehicleMaster hireVehicleMaster = new HireVehicleMaster();
                    if (mModel.VehicleGroup != "100001")
                    {
                        vehicleMaster = ctxTFAT.VehicleMaster.Where(x => x.Code == mModel.VehicleNo).FirstOrDefault();
                    }
                    else
                    {
                        hireVehicleMaster = ctxTFAT.HireVehicleMaster.Where(X => X.Code == mModel.VehicleNo).FirstOrDefault();
                    }

                    int FMNO = 0;
                    if (ctxTFAT.FMMaster.Where(x => x.TableKey == mModel.Document).FirstOrDefault() != null)
                    {
                        fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey == mModel.Document).FirstOrDefault();
                        vehicleDri_Hist = ctxTFAT.tfatVehicleStatusHistory.Where(x => x.refParentKey == fMMaster.TableKey).FirstOrDefault();
                        tfatVehicleStatus = ctxTFAT.TfatVehicleStatus.Where(x => x.refParentKey == fMMaster.TableKey).FirstOrDefault();
                        mAdd = false;
                        DeUpdate(mModel);
                    }
                    if (mAdd == false)
                    {
                        if (mbranchcode != fMMaster.Branch)
                        {
                            transaction.Rollback();
                            return Json(new { Message = "Login Branch Does Not Match With Documnet Branch.", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    if (mModel.Mode == "Add")
                    {
                        fMMaster.FmNo = mModel.FMGenerate == "A" ? GetNewCode() : Convert.ToInt32(mModel.FMNO);
                        fMMaster.CreateDate = DateTime.Now;
                        fMMaster.LoginBranch = mbranchcode;
                        fMMaster.Branch = mbranchcode;
                        fMMaster.GenerateType = mModel.FMGenerate;
                        fMMaster.Type = "FM000";

                        fMMaster.ParentKey = mbranchcode + "FM000" + mperiod.Substring(0, 2) + fMMaster.FmNo;
                        fMMaster.TableKey = mbranchcode + "FM000" + mperiod.Substring(0, 2) + 1.ToString("D3") + fMMaster.FmNo;
                        mnewrecordkey = fMMaster.TableKey.ToString();
                    }
                    else
                    {
                        mnewrecordkey = fMMaster.TableKey.ToString();
                    }

                    #region Update Vehicle Status

                    FMSetup fMSetup = ctxTFAT.FMSetup.FirstOrDefault();
                    if (fMSetup != null)
                    {
                        if (!String.IsNullOrEmpty(fMSetup.VehicleCateStsMain))
                        {
                            var CodeList = fMSetup.VehicleCateStsMain.Split(',');
                            foreach (var item in CodeList)
                            {
                                if (item.ToLower().Trim() == mModel.VehicleGroup.Trim())
                                {
                                    if (mModel.VehicleGroup != "100001")
                                    {
                                        vehicleMaster.Status = "Transit";
                                        ctxTFAT.Entry(vehicleMaster).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        hireVehicleMaster.Status = "Transit";
                                        ctxTFAT.Entry(hireVehicleMaster).State = EntityState.Modified;
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    #endregion

                    //TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.From).FirstOrDefault();

                    fMMaster.Date = ConvertDDMMYYTOYYMMDD(mModel.FM_Date.ToString());
                    fMMaster.Time = mModel.FM_Time;
                    fMMaster.CurrBranch = fMMaster.Branch == null ? mbranchcode : fMMaster.Branch;
                    fMMaster.FmStatus = "W";
                    fMMaster.VehicleStatus = mModel.VehicleGroup;
                    fMMaster.TruckNo = mModel.VehicleNo;
                    fMMaster.BroCode = mModel.Broker;
                    fMMaster.KM = Convert.ToDouble(mModel.KM);
                    fMMaster.FromBranch = mModel.From;
                    fMMaster.ToBranch = mModel.To;
                    fMMaster.VehicleCategory = mModel.VehicleCategory;
                    fMMaster.PayLoad = Convert.ToInt32(mModel.PayLoad.ToString());
                    fMMaster.ReceiptNo = mModel.ReceiptNo;
                    if (mModel.VehicleGroup == "100001")
                    {
                        fMMaster.Driver = mModel.DriverName;
                    }
                    else
                    {
                        fMMaster.Driver = mModel.DriverCode;
                        VehicleMaster vehicle = ctxTFAT.VehicleMaster.Where(x => x.Code == fMMaster.TruckNo).FirstOrDefault();
                        DriverMaster driver = ctxTFAT.DriverMaster.Where(x => x.Code == fMMaster.Driver).FirstOrDefault();

                        fMMaster.VehiclePostAc = vehicle.PostAc;
                        fMMaster.DriverPostAc = driver.Posting;
                        fMMaster.VehicleMaintainDriverAC = vehicle.MaintainDriverAC;
                        fMMaster.VehicleArAp = vehicle.ARAP;
                        fMMaster.VehicleFmPostReq = vehicle.PostReq;
                    }
                    fMMaster.DieselAmt = mModel.DieselAmt;
                    fMMaster.DieselLtr = mModel.DieselLtr;
                    fMMaster.LicenCeNo = mModel.LicenceNo;
                    if (mModel.LicenceExpDate == "01/01/0001" || mModel.LicenceExpDate == null)
                    {
                        fMMaster.LicenceExpDate = null;
                    }
                    else
                    {
                        fMMaster.LicenceExpDate = ConvertDDMMYYTOYYMMDD(mModel.LicenceExpDate);
                    }
                    fMMaster.OwnerName = mModel.Owner;
                    fMMaster.ChallanNo = mModel.ChallanNo;
                    fMMaster.ContactNo = mModel.ContactNo;
                    fMMaster.Freight = mModel.Freight;
                    fMMaster.Adv = mModel.Advance;
                    fMMaster.TripAdvance = mModel.TripAdvance;
                    fMMaster.TripFreight = mModel.TripFreight;
                    if (mModel.VehicleGroup != "100001")
                    {
                        if (vehicleMaster.MaintainDriverAC)
                        {
                            fMMaster.Balance = 0;
                        }
                        else
                        {
                            fMMaster.Balance = (mModel.Freight) - (mModel.Advance);
                        }
                    }
                    else
                    {
                        fMMaster.Balance = (mModel.Freight) - (mModel.Advance);
                    }

                    fMMaster.PayAt = mModel.PayableAt;
                    fMMaster.ChgAmt1 = mModel.Charges.Where(x => x.Fld == "F001").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F001") : 0;
                    fMMaster.ChgAmt2 = mModel.Charges.Where(x => x.Fld == "F002").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F002") : 0;
                    fMMaster.ChgAmt3 = mModel.Charges.Where(x => x.Fld == "F003").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F003") : 0;
                    fMMaster.ChgAmt4 = mModel.Charges.Where(x => x.Fld == "F004").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F004") : 0;
                    fMMaster.ChgAmt5 = mModel.Charges.Where(x => x.Fld == "F005").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F005") : 0;
                    fMMaster.ChgAmt6 = mModel.Charges.Where(x => x.Fld == "F006").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F006") : 0;
                    fMMaster.ChgAmt7 = mModel.Charges.Where(x => x.Fld == "F007").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F007") : 0;
                    fMMaster.ChgAmt8 = mModel.Charges.Where(x => x.Fld == "F008").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F008") : 0;
                    fMMaster.ChgAmt9 = mModel.Charges.Where(x => x.Fld == "F009").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F009") : 0;
                    fMMaster.ChgAmt10 = mModel.Charges.Where(x => x.Fld == "F010").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F010") : 0;

                    fMMaster.OldRate = mModel.Freight;
                    fMMaster.Opening = false;
                    fMMaster.status = true;
                    fMMaster.NetBalance = (mModel.Balance);
                    fMMaster.Balance = (mModel.Balance);
                    fMMaster.TDSAmt = (mModel.TDSAmt);
                    fMMaster.TotlExp = (mModel.TotExp);
                    fMMaster.TotlAdvExp = (mModel.TotAdvExp);
                    fMMaster.Remark = mModel.Remark;
                    fMMaster.Prefix = mperiod;
                    fMMaster.Payment = mModel.Payment;
                    fMMaster.ScheduleRequired = mModel.ScheduleRequired;
                    if (mModel.VehicleGroup != "100001")
                    {
                        fMMaster.ScheduleKMReq = vehicleMaster.ScheduleKM;
                    }
                    else
                    {
                        fMMaster.ScheduleKMReq = false;
                    }
                    if (!String.IsNullOrEmpty(mModel.DestCombo))
                    {
                        var GetRoute = mModel.DestCombo.Split(',');
                        var SelectedRoute = "";
                        foreach (var item in GetRoute)
                        {
                            if (item != mModel.From || item != mModel.To)
                            {
                                SelectedRoute += (ctxTFAT.TfatBranch.Where(x => x.Code == item).Select(x => x.Name).FirstOrDefault()) + ",";
                            }
                        }
                        fMMaster.SelectedRoute = SelectedRoute;
                        fMMaster.RouteVia = mModel.DestCombo + ",";
                    }
                    else
                    {
                        fMMaster.SelectedRoute = "";
                        fMMaster.RouteVia = mModel.DestCombo;
                    }
                    fMMaster.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    fMMaster.ENTEREDBY = muserid;
                    fMMaster.AUTHORISE = mauthorise;
                    fMMaster.AUTHIDS = muserid;

                    LogisticsFlow systemFlow = ctxTFAT.LogisticsFlow.FirstOrDefault();
                    if (systemFlow == null)
                    {
                        systemFlow = new LogisticsFlow();
                    }
                    fMMaster.ScheduleFollowup = Convert.ToBoolean(systemFlow.ScheduleFollowUp);

                    //List<FMAttachment> SessionAttachList = Session["FMAttachmentList"] as List<FMAttachment>;

                    #region Authorisation

                    mauthorise = "A00";
                    TfatUserAuditHeader authorisation = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == "FM000").FirstOrDefault();
                    if (authorisation != null)
                    {
                        mauthorise = SetAuthorisationLogistics(authorisation, fMMaster.TableKey, fMMaster.FmNo.ToString(), 0, fMMaster.Date.ToShortDateString(), fMMaster.Freight, fMMaster.BroCode, mbranchcode);
                        fMMaster.AUTHORISE = mauthorise;
                    }

                    #endregion

                    #region Maintain Driver And Vehicle History

                    if (vehicleDri_Hist == null)
                    {
                        vehicleDri_Hist = new tfatVehicleStatusHistory();
                        tfatVehicleStatus = new TfatVehicleStatus();
                    }
                    if (vehicleDri_Hist.RECORDKEY == 0)
                    {
                        vehicleDri_Hist.Code = GetNewCode_VehiHistory();
                        vehicleDri_Hist.ENTEREDBY = muserid;
                        vehicleDri_Hist.refParentKey = fMMaster.TableKey;
                        vehicleDri_Hist.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());

                        vehicleDri_Hist.Status = "Transit";
                        vehicleDri_Hist.FromPeriod = ConvertDDMMYYTOYYMMDD(mModel.FM_Date);
                        vehicleDri_Hist.FromTime = mModel.FM_Time;
                        vehicleDri_Hist.TruckNo = mModel.VehicleNo;
                        vehicleDri_Hist.Narr = "Generate Freight Memo";
                        vehicleDri_Hist.AUTHIDS = muserid;
                        vehicleDri_Hist.AUTHORISE = mauthorise;
                        vehicleDri_Hist.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                        ctxTFAT.tfatVehicleStatusHistory.Add(vehicleDri_Hist);

                        tfatVehicleStatus.DocNo = GetNewCodeHistory();
                        tfatVehicleStatus.AUTHIDS = muserid;
                        tfatVehicleStatus.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                        tfatVehicleStatus.ENTEREDBY = muserid;
                        tfatVehicleStatus.refParentKey = fMMaster.TableKey;

                        tfatVehicleStatus.Vehicle = mModel.VehicleNo;
                        tfatVehicleStatus.Branch = mbranchcode;
                        tfatVehicleStatus.Status = "Transit";
                        tfatVehicleStatus.Narr = "Generate Freight Memo";
                        tfatVehicleStatus.EffDate = ConvertDDMMYYTOYYMMDD(mModel.FM_Date);
                        tfatVehicleStatus.EffTime = mModel.FM_Time;
                        tfatVehicleStatus.AUTHORISE = mauthorise;
                        tfatVehicleStatus.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                        ctxTFAT.TfatVehicleStatus.Add(tfatVehicleStatus);

                    }
                    else
                    {
                        vehicleDri_Hist.Status = "Transit";
                        vehicleDri_Hist.FromPeriod = ConvertDDMMYYTOYYMMDD(mModel.FM_Date);
                        vehicleDri_Hist.FromTime = mModel.FM_Time;
                        vehicleDri_Hist.TruckNo = mModel.VehicleNo;
                        vehicleDri_Hist.Narr = "Generate Freight Memo";
                        vehicleDri_Hist.AUTHIDS = muserid;
                        vehicleDri_Hist.AUTHORISE = mauthorise;
                        vehicleDri_Hist.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                        ctxTFAT.Entry(vehicleDri_Hist).State = EntityState.Modified;

                        tfatVehicleStatus.Vehicle = mModel.VehicleNo;
                        tfatVehicleStatus.Branch = mbranchcode;
                        tfatVehicleStatus.Status = "Transit";
                        tfatVehicleStatus.Narr = "Generate Freight Memo";
                        tfatVehicleStatus.EffDate = ConvertDDMMYYTOYYMMDD(mModel.FM_Date);
                        tfatVehicleStatus.EffTime = mModel.FM_Time;
                        tfatVehicleStatus.AUTHORISE = mauthorise;
                        tfatVehicleStatus.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                        ctxTFAT.Entry(tfatVehicleStatus).State = EntityState.Modified;
                    }

                    #endregion

                    if (mModel.Mode == "Add")
                    {
                        ctxTFAT.FMMaster.Add(fMMaster);
                        AttachmentVM vM = new AttachmentVM();
                        vM.ParentKey = fMMaster.TableKey;
                        vM.Srl = fMMaster.FmNo.ToString();
                        vM.Type = "FM000";
                        SaveAttachment(vM);
                        SaveNarrationAdd(fMMaster.FmNo.ToString(), fMMaster.TableKey);
                        mnewrecordkey = fMMaster.TableKey;
                    }
                    else
                    {
                        ctxTFAT.Entry(fMMaster).State = EntityState.Modified;
                        mnewrecordkey = fMMaster.TableKey;
                    }

                    #region Check Freight Memo Some Point To Update Or Not New Data

                    var FMMast = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == mModel.Document.ToString()).FirstOrDefault();
                    if (FMMast != null)
                    {
                        var listfmroute = ctxTFAT.FMROUTETable.Where(x => x.Parentkey == mModel.Document).ToList();
                        if (listfmroute.Where(x => x.ArrivalDate != null && x.SequenceRoute != 0 && x.SubRoute != 0).FirstOrDefault() != null)
                        {
                            mModel.BlockFM = true;
                            mModel.BlockFMMessage = "This FM Arrived In Another Branch So U Cant Edit Route And Schedule...!";
                        }
                        else if (listfmroute.Where(x => x.LCNO != null && x.LCNO != "").FirstOrDefault() != null)
                        {
                            mModel.BlockFM = true;
                            mModel.BlockFMMessage = "In This FM Material Loaded So U Cant Edit Route And Schedule...!";
                        }
                        else if (listfmroute.Where(x => x.UnLoadLCNO != null && x.UnLoadLCNO != "").FirstOrDefault() != null)
                        {
                            mModel.BlockFM = true;
                            mModel.BlockFMMessage = "This FM Material UnLoaded So U Cant Edit Route And Schedule...!";
                        }
                        else if (listfmroute.Where(x => x.ConsignmentLoad != null && x.ConsignmentLoad != "").FirstOrDefault() != null)
                        {
                            mModel.BlockFM = true;
                            mModel.BlockFMMessage += "In This FM Material Loaded(Direct)   So U Cant Edit Route And Schedule...!<br>";
                        }
                    }

                    #endregion

                    #region Add New Routes

                    if (mModel.BlockFM == false)
                    {
                        AddFmRealtion(mModel, fMMaster);
                    }

                    #endregion

                    #region Freight And Advance Charges Brief

                    List<FmCatchChargesInfo> FMCharges = Session["FMFreightAdvance"] as List<FmCatchChargesInfo>;
                    if (FMCharges == null)
                    {
                        FMCharges = new List<FmCatchChargesInfo>();
                    }
                    foreach (var item in FMCharges)
                    {
                        FmCatchChargesInfo fmCatch = new FmCatchChargesInfo();
                        fmCatch.Amt = item.Amt;
                        fmCatch.Amt1 = item.Amt1;
                        fmCatch.AUTHIDS = muserid;
                        fmCatch.AUTHORISE = "A00";
                        fmCatch.Category = item.Category;
                        fmCatch.ENTEREDBY = muserid;
                        fmCatch.Fmno = fMMaster.FmNo;
                        fmCatch.FromBranch = item.FromBranch;
                        fmCatch.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                        fmCatch.ToBranch = item.ToBranch;
                        fmCatch.Type = item.Type;
                        fmCatch.MenuHeader = item.MenuHeader;
                        fmCatch.Sno = item.Sno;
                        fmCatch.FMRefTablekey = fMMaster.TableKey;

                        ctxTFAT.FmCatchChargesInfo.Add(fmCatch);
                    }

                    #endregion

                    #region Freight Memo Posting Darshan

                    List<PurchaseVM> LedgerPosting = mModel.LedgerPostList;
                    int lCnt = 1;
                    string mCreditAcc = "";
                    bool Posting = true;
                    if (mModel.VehicleGroup != "100001")
                    {
                        if (vehicleMaster.PostReq == false && vehicleMaster.MaintainDriverAC == false)
                        {
                            Posting = false;
                        }
                        else if (vehicleMaster.PostReq == false && vehicleMaster.MaintainDriverAC == true)
                        {
                            if (vehicleMaster.FMVOURELReq)
                            {
                                Posting = false;
                            }
                        }
                    }
                    List<Ledger> ledgers = new List<Ledger>();
                    string mDriverAc = "";
                    string mDriverPostCode = "";
                    bool mMaintainDriverac = mModel.VehicleGroup == "100001" ? false : vehicleMaster.MaintainDriverAC;
                    //bool mMaintainCredit = mModel.VehicleGroup == "100001" ? false : vehicleMaster.ARAP;
                    if (Posting)
                    {
                        var ledpost = LedgerPosting;
                        if (ledpost != null)
                        {
                            for (int u = 0; u < ledpost.Count; u++)
                            {
                                Ledger mobjL = new Ledger();
                                mobjL.AltCode = ledpost[u].DelyCode;
                                mobjL.Audited = true;
                                mobjL.AUTHIDS = muserid;
                                mobjL.AUTHORISE = fMMaster.AUTHORISE;
                                mobjL.BillDate = ConvertDDMMYYTOYYMMDD(mModel.FM_Date.ToString());
                                mobjL.BillNumber = "";
                                mobjL.Branch = mbranchcode;
                                mobjL.Cheque = "";
                                mobjL.ChequeReturn = false;
                                mobjL.ChqCategory = 1;
                                mobjL.ClearDate = DateTime.Now;
                                mobjL.Code = ledpost[u].Code;
                                mobjL.Credit = Convert.ToDecimal(ledpost[u].Credit);
                                mobjL.CrPeriod = 0;
                                mobjL.CurrAmount = Convert.ToDecimal(ledpost[u].Credit + ledpost[u].Debit);
                                mobjL.CurrName = 1;
                                mobjL.CurrRate = 1;
                                mobjL.Debit = Convert.ToDecimal(ledpost[u].Debit);
                                mobjL.Discounted = true;
                                mobjL.DocDate = ConvertDDMMYYTOYYMMDD(mModel.FM_Date.ToString());
                                mobjL.DueDate = DateTime.Now;
                                mobjL.LocationCode = 100001;
                                mobjL.MainType = "LO";
                                mobjL.Narr = ledpost[u].RelatedTo;
                                mobjL.Party = ledpost[u].Code;
                                mobjL.Prefix = mperiod;
                                mobjL.RecoFlag = "";
                                mobjL.RefDoc = ledpost[u].RefDoc;
                                mobjL.Reminder = true;
                                mobjL.Sno = lCnt;
                                mobjL.Srl = fMMaster.FmNo.ToString();
                                mobjL.SubType = "LC";
                                mobjL.TaskID = 0;
                                mobjL.TDSChallanNumber = "";
                                mobjL.TDSCode = 0;
                                mobjL.TDSFlag = false;
                                mobjL.Type = "FM000";
                                mobjL.ENTEREDBY = muserid;
                                mobjL.LASTUPDATEDATE = DateTime.Now;
                                mobjL.ChequeDate = DateTime.Now;
                                mobjL.CompCode = mcompcode;
                                mobjL.ParentKey = fMMaster.ParentKey;
                                mobjL.TableKey = mbranchcode + "FM000" + mperiod.Substring(0, 2) + lCnt.ToString("D3") + fMMaster.FmNo;
                                mobjL.PCCode = 100002;
                                mobjL.ProjCode = ledpost[u].Reference;
                                ctxTFAT.Ledger.Add(mobjL);

                                ledgers.Add(mobjL);
                                ++lCnt;
                            }
                        }

                        string mBroPostCode = ctxTFAT.Master.Where(x => x.Code.ToString() == mModel.Broker).Select(x => x.Code).FirstOrDefault();
                        string VehicleLedPostCode = vehicleMaster.PostAc;
                        mCreditAcc = (vehicleMaster.CrAc == "B") ? mBroPostCode : VehicleLedPostCode;
                    }
                    #endregion

                    #region  Below Code Not Execute At All (FMVOURELReq Always Return False) 
                    if (vehicleMaster.MaintainDriverAC == true && vehicleMaster.FMVOURELReq == true)
                    {
                        if (mMaintainDriverac == true)
                        {
                            mDriverAc = mModel.DriverCode;
                            mDriverPostCode = ctxTFAT.DriverMaster.Where(x => x.Code == mDriverAc).Select(x => x.Posting).FirstOrDefault();
                        }
                        if (mModel.Advance > 0)
                        {
                            FMVouRel mFmVRel = new FMVouRel();
                            mFmVRel.Adv = mModel.Advance;
                            mFmVRel.AdvPen = mModel.Advance;
                            mFmVRel.Bal = 0;
                            mFmVRel.BalPen = 0;
                            mFmVRel.Branch = mbranchcode;
                            mFmVRel.FMNo = fMMaster.FmNo.ToString();
                            mFmVRel.Freight = mModel.Freight;
                            mFmVRel.Pay = "A";
                            mFmVRel.SNo = "";
                            mFmVRel.Type = true;
                            mFmVRel.VouNo = "";
                            mFmVRel.ENTEREDBY = muserid;
                            mFmVRel.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                            mFmVRel.AUTHORISE = "A00";
                            mFmVRel.AUTHIDS = muserid;
                            mFmVRel.PostCode = mDriverPostCode;
                            ctxTFAT.FMVouRel.Add(mFmVRel);
                        }
                    }
                    #endregion

                    #region TDS Posting
                    if (Posting)
                    {
                        if (mModel.TDSAmt != 0)
                        {
                            var mAccount = mModel.VehicleGroup == "100001" ? mModel.Broker : mCreditAcc;
                            TDSPayments tspay = new TDSPayments();
                            tspay.aMainType = "LO";
                            tspay.Amount = mModel.Freight;
                            tspay.aPrefix = mperiod;
                            tspay.aSno = 1;
                            tspay.aSrl = fMMaster.FmNo.ToString();
                            tspay.SubType = "LC";
                            tspay.aSubType = "LC";
                            tspay.aType = "FM000";
                            tspay.BankCode = "";
                            tspay.BillNumber = "";
                            tspay.Branch = mbranchcode;
                            tspay.CertDate = DateTime.Now;
                            tspay.CertNumber = "";
                            tspay.ChallanDate = ConvertDDMMYYTOYYMMDD(mModel.FM_Date.ToString());
                            tspay.ChallanNumber = "";
                            tspay.CNO = "";
                            tspay.Code = mAccount;
                            tspay.CompCode = mcompcode;
                            tspay.DepositSerial = "";
                            tspay.DocDate = ConvertDDMMYYTOYYMMDD(mModel.FM_Date.ToString());
                            tspay.DueDate = DateTime.Now.Date;
                            tspay.EndCredit = false;

                            tspay.LocationCode = 100001;
                            tspay.MainType = "LO";
                            tspay.Narr = "";
                            tspay.PaidAmt = 0;
                            tspay.ParentKey = fMMaster.ParentKey;
                            tspay.Party = mAccount;
                            tspay.PaymentMode = 0;
                            tspay.Prefix = mperiod;
                            tspay.Sno = 1;
                            tspay.Srl = fMMaster.FmNo.ToString();
                            tspay.SubType = "LC";
                            tspay.TableKey = mbranchcode + "FM000" + mperiod.Substring(0, 2) + 1.ToString("D3") + fMMaster.FmNo;
                            tspay.TDSAble = mModel.Freight;
                            tspay.TDSAmt = mModel.TDSAmt;
                            tspay.TDSCess = 0;
                            tspay.TDSCessAmt = 0;
                            tspay.TDSCode = ctxTFAT.TaxDetails.Where(x => x.Code == mAccount).Select(x => (int?)x.TDSCode).FirstOrDefault() ?? 0;
                            tspay.TDSReason = 0;
                            tspay.TDSSheCess = 0;
                            tspay.TDSSheCessAmt = 0;
                            tspay.TDSSurCharge = 0;
                            tspay.TDSSurChargeAmt = 0;
                            tspay.TDSTax = mModel.TDSRate;
                            tspay.TDSTaxAmt = 0;
                            tspay.TotalTDSAmt = mModel.TDSAmt;
                            tspay.Type = "FM000";
                            tspay.ENTEREDBY = muserid;
                            tspay.LASTUPDATEDATE = DateTime.Now;
                            tspay.AUTHORISE = fMMaster.AUTHORISE;
                            tspay.AUTHIDS = muserid;
                            ctxTFAT.TDSPayments.Add(tspay);

                            var mBalCntNoCred = ledgers.Where(x => x.RefDoc == "B" && x.Code == mAccount).FirstOrDefault();
                            var BillDetails1 = ledgers.Where(x => x.Party == "000009994").Select(x => x.Sno).FirstOrDefault();
                            var BillDetails = ledgers.Where(x => x.Sno == (BillDetails1 - 1)).FirstOrDefault();

                            if (BillDetails != null)
                            {
                                Outstanding osobj1 = new Outstanding();

                                osobj1.Branch = mbranchcode;
                                osobj1.DocBranch = mbranchcode;
                                osobj1.MainType = "LO";
                                osobj1.SubType = "LC";
                                osobj1.Type = "FM000";
                                osobj1.Prefix = mperiod;
                                osobj1.Srl = fMMaster.FmNo.ToString();
                                osobj1.Sno = 1;
                                osobj1.ParentKey = BillDetails.ParentKey;
                                osobj1.TableKey = BillDetails.TableKey;
                                osobj1.aMaintype = "LO";
                                osobj1.aSubType = "LC";
                                osobj1.aType = "FM000";
                                osobj1.aPrefix = mperiod;
                                osobj1.aSrl = fMMaster.FmNo.ToString();
                                osobj1.aSno = 2;
                                osobj1.Amount = mModel.TDSAmt;
                                osobj1.TableRefKey = BillDetails.TableKey;
                                osobj1.AUTHIDS = muserid;
                                osobj1.AUTHORISE = fMMaster.AUTHORISE;
                                osobj1.BillDate = ConvertDDMMYYTOYYMMDD(mModel.FM_Date.ToString());
                                osobj1.BillNumber = "";
                                osobj1.CompCode = mcompcode;
                                osobj1.Broker = 100001;
                                osobj1.Brokerage = Convert.ToDecimal(0.00);
                                osobj1.BrokerAmt = Convert.ToDecimal(0.00);
                                osobj1.BrokerOn = Convert.ToDecimal(0.00);
                                osobj1.ChlnDate = DateTime.Now;
                                osobj1.ChlnNumber = "";
                                osobj1.Code = BillDetails.Party;
                                osobj1.CrPeriod = 0;
                                osobj1.CurrName = 1;
                                osobj1.CurrRate = 1;
                                osobj1.DocDate = ConvertDDMMYYTOYYMMDD(mModel.FM_Date.ToString());
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
                                osobj1.TDSAmt = 0;
                                osobj1.ENTEREDBY = muserid;
                                osobj1.LASTUPDATEDATE = DateTime.Now;
                                osobj1.CurrAmount = mModel.TDSAmt;
                                osobj1.ValueDate = DateTime.Now;
                                osobj1.LocationCode = 100001;

                                ctxTFAT.Outstanding.Add(osobj1);

                                // second effect
                                Outstanding osobj2 = new Outstanding();
                                osobj2.Branch = mbranchcode;
                                osobj2.DocBranch = mbranchcode;
                                osobj2.ParentKey = mBalCntNoCred.ParentKey;
                                osobj2.Type = "FM000";
                                osobj2.Prefix = mperiod;
                                osobj2.Srl = fMMaster.FmNo.ToString();
                                osobj2.Sno = 2;
                                osobj2.TableKey = mBalCntNoCred.TableKey;
                                osobj2.aType = "FM000";
                                osobj2.aPrefix = mperiod;
                                osobj2.aSrl = fMMaster.FmNo.ToString();
                                osobj2.aSno = 1;
                                osobj2.aMaintype = "LO";
                                osobj2.TableRefKey = mBalCntNoCred.TableKey;
                                osobj2.MainType = "LO";
                                osobj2.SubType = "LC";
                                osobj2.aSubType = "LC";
                                osobj2.Amount = mModel.TDSAmt;
                                osobj2.AUTHIDS = muserid;
                                osobj2.AUTHORISE = fMMaster.AUTHORISE;
                                osobj2.BillDate = ConvertDDMMYYTOYYMMDD(mModel.FM_Date.ToString());
                                osobj2.BillNumber = "";
                                osobj2.CompCode = mcompcode;
                                osobj2.Broker = 100001;
                                osobj2.Brokerage = Convert.ToDecimal(0.00);
                                osobj2.BrokerAmt = Convert.ToDecimal(0.00);
                                osobj2.BrokerOn = Convert.ToDecimal(0.00);
                                osobj2.ChlnDate = DateTime.Now;
                                osobj2.ChlnNumber = "";
                                osobj2.Code = mBalCntNoCred.Code;
                                osobj2.CrPeriod = 0;
                                osobj2.CurrName = 1;
                                osobj2.CurrRate = 1;
                                osobj2.DocDate = ConvertDDMMYYTOYYMMDD(mModel.FM_Date.ToString());
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
                                osobj2.TDSAmt = 0;
                                osobj2.ENTEREDBY = muserid;
                                osobj2.LASTUPDATEDATE = DateTime.Now;
                                osobj2.CurrAmount = mModel.TDSAmt;
                                osobj2.ValueDate = DateTime.Now;
                                osobj2.LocationCode = 100001;

                                ctxTFAT.Outstanding.Add(osobj2);
                            }
                        }
                    }
                    #endregion

                    ctxTFAT.SaveChanges();

                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, fMMaster.ParentKey, fMMaster.Date, fMMaster.Freight, fMMaster.BroCode, "Save Freight Memo :" + fMMaster.FmNo, "A");
                    FreightMemoNotification(fMMaster);
                }

                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.InnerException.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { SerialNo = mnewrecordkey, Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpDateFMRouteData(FMVM mModel)
        {
            //bool Status = false;
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    FMMaster fMMaster = new FMMaster();
                    if (ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == mModel.Document).FirstOrDefault() != null)
                    {
                        fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == mModel.Document).FirstOrDefault();
                    }
                    var result = (List<RouteDetails>)Session["GridDataSession"];
                    bool AddRoute = false;
                    var Parent = mbranchcode;
                    var LastRouteOfFM = ctxTFAT.FMROUTETable.Where(x => x.Parentkey.ToString() == mModel.Document && x.RouteType == "R").OrderByDescending(x => x.SequenceRoute).FirstOrDefault();
                    int LastRoute = LastRouteOfFM.SequenceRoute.Value;
                    var LastRouteOfFMList = ctxTFAT.FMROUTETable.Where(x => x.Parentkey.ToString() == mModel.Document && x.SubRoute == LastRoute).ToList();
                    var ExistingRout = ctxTFAT.FMROUTETable.Where(x => x.Parentkey.ToString() == mModel.Document && x.RouteType == "R").Select(x => x.Parent).ToList();
                    var Via = "";
                    var ViaName = "";
                    foreach (var item in result.OrderBy(x => x.Tempid).ToList())
                    {
                        if (!String.IsNullOrEmpty(item.Branch))
                        {
                            TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).FirstOrDefault();
                            if (tfatBranch.Category == "Area")
                            {
                                if (tfatBranch.Grp != "G00000")
                                {
                                    AddRoute = true;
                                    Parent = tfatBranch.Grp;
                                }
                            }
                            else
                            {
                                AddRoute = true;
                                Parent = tfatBranch.Code;
                            }
                            if (AddRoute)
                            {

                                if (!(ExistingRout.Contains(Parent)))
                                {
                                    Via += item.Branch + ",";
                                    ViaName += tfatBranch.Name + ",";
                                    FMROUTETable fMROUTE = new FMROUTETable();
                                    fMROUTE.ENTEREDBY = muserid;
                                    fMROUTE.FmNo = fMMaster.FmNo;
                                    fMROUTE.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                                    fMROUTE.Parent = Parent;
                                    fMROUTE.RouteClear = false;
                                    fMROUTE.RouteType = "R";
                                    fMROUTE.RouteVia = item.Branch;
                                    fMROUTE.SubRoute = LastRoute;
                                    fMROUTE.SequenceRoute = LastRoute;
                                    fMROUTE.VehicleActivity = "00:00";
                                    fMROUTE.AUTHORISE = mauthorise;
                                    fMROUTE.AUTHIDS = muserid;
                                    fMROUTE.Prefix = mperiod;
                                    fMROUTE.Parentkey = fMMaster.TableKey;

                                    ctxTFAT.FMROUTETable.Add(fMROUTE);
                                    ExistingRout.Add(Parent);
                                    ++LastRoute;
                                }
                            }
                        }

                    }


                    if (!(String.IsNullOrEmpty(Via)))
                    {
                        var MainRoute = LastRouteOfFMList.Where(x => x.RouteType == "R").FirstOrDefault();
                        MainRoute.SequenceRoute = (LastRoute);
                        MainRoute.SubRoute = (LastRoute);
                        ctxTFAT.Entry(MainRoute).State = EntityState.Modified;
                        var SubRouteList = LastRouteOfFMList.Where(x => x.RouteType != "R").ToList();
                        SubRouteList.ForEach(x => x.SubRoute = (LastRoute));

                        //Via = Via.Substring(0, Via.Length - 1);
                        //ViaName = ViaName.Substring(0, ViaName.Length - 1);

                    }
                    fMMaster.RouteVia += Via;
                    fMMaster.SelectedRoute += ViaName;

                    FMROUTETable LastRouteN = ctxTFAT.FMROUTETable.Where(x => x.Parentkey == mModel.Document && x.RouteType == "R").OrderByDescending(x => x.SequenceRoute).FirstOrDefault();
                    var LastBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == LastRouteN.Parent).Select(x => x.Name).FirstOrDefault() + ",";
                    string resdsult = fMMaster.RouteViaName.Replace(LastBranchN, "");
                    fMMaster.RouteViaName = resdsult + ViaName + LastBranchN;


                    ctxTFAT.Entry(fMMaster).State = EntityState.Modified;
                    ctxTFAT.SaveChanges();

                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, fMMaster.ParentKey, fMMaster.Date, fMMaster.Freight, fMMaster.BroCode, "Save Freight Memo :" + fMMaster.FmNo, "A");

                }

                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.InnerException.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
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

        public string DeleteStateMaster(FMVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return "Code not Entered..";
            }

            var fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == mModel.Document.ToString()).FirstOrDefault();
            var mLedgerList = ctxTFAT.Ledger.Where(x => x.ParentKey == fMMaster.ParentKey).ToList();
            var MfMVOUREL = ctxTFAT.FMVouRel.Where(x => (x.FMNo == mModel.Document) && x.Type == true).Select(x => x).ToList();
            var FMROUTETables = ctxTFAT.FMROUTETable.Where(x => x.Parentkey.ToString() == fMMaster.TableKey.ToString()).ToList();
            var lCMasters = ctxTFAT.LCMaster.Where(x => x.FMRefTablekey.ToString() == fMMaster.TableKey.ToString()).ToList();
            var FmCharges = ctxTFAT.FmCatchChargesInfo.Where(x => x.FMRefTablekey == fMMaster.TableKey).ToList();

            if (ctxTFAT.PeriodLock.Where(x => x.LockDate == fMMaster.Date && x.Branch == mbranchcode && x.Type == "FM000").FirstOrDefault() != null)
            {
                return "This FM Date Locked By Period Locking System...!";
            }
           
            if (!String.IsNullOrEmpty(fMMaster.LCno))
            {
                return "In This FM Material Loaded So U Cant Delete...!";
            }
            if (FMROUTETables.Where(x => x.ConsignmentLoad != null && x.ConsignmentLoad != "").FirstOrDefault() != null)
            {
                return "In This FM Material Loaded(Direct)   So U Cant Delete...!";
            }
            var AlertNoteFM = ctxTFAT.AlertNoteMaster.Where(x => x.Type == "FM000" && x.ParentKey.Trim() == fMMaster.TableKey.ToString().Trim()).FirstOrDefault();
            if (AlertNoteFM != null)
            {
                return "Found AlertNote Please Remove AlertNote First....";
            }

            var TripSheet = ctxTFAT.TripFmList.Where(x => x.RefTablekey == fMMaster.TableKey.ToString().Trim()).FirstOrDefault();
            if (AlertNoteFM != null)
            {
                return "Found TripSheet Please Remove TripSheet First....";
            }
            var GetLedger = ctxTFAT.Ledger.Where(x => x.ParentKey == fMMaster.ParentKey).Select(x => x.TableKey).ToList();
            if (GetLedger != null)
            {
                var advpaydetails = ctxTFAT.VoucherDetail.Where(x => GetLedger.Contains(x.FMTableKey.ToString())).Select(x => x).FirstOrDefault();
                if (advpaydetails != null)
                {

                    return "Document is Already Adjusted Against Advance Payment: " + advpaydetails.ParentKey + ", Cant Edit or Delete";
                }
            }

            var AdjustFMCashBannkTrn = ctxTFAT.RelFm.Where(x => x.FMRefTablekey == fMMaster.TableKey).FirstOrDefault();
            if (AdjustFMCashBannkTrn != null)
            {
                return " Document is Already Adjusted In '" + (ctxTFAT.TfatBranch.Where(x => x.Code == AdjustFMCashBannkTrn.Branch).Select(x => x.Name).FirstOrDefault()) + "'  Cash Bank Against : " + AdjustFMCashBannkTrn.ParentKey.ToString() + " , Cant " + mModel.Mode;
            }

            var LRExp = ctxTFAT.RelFm.Where(x => x.FMRefTablekey.ToString() == mModel.Document).FirstOrDefault();
            if (LRExp != null)
            {
                var Billno = ctxTFAT.RelateData.Where(x => x.ParentKey == LRExp.ParentKey).FirstOrDefault();
                var Type = ctxTFAT.DocTypes.Where(x => x.Code == Billno.Type).Select(x => x.Name).FirstOrDefault();
                var Branch = ctxTFAT.TfatBranch.Where(x => x.Code == Billno.Branch).Select(x => x.Name).FirstOrDefault();
                return "This Freight Memo Found In " + Type + " . Entry Branch : " + Branch.ToUpper() + " And " + Billno.Srl + " . \nPlease Remove Consignment From The " + Type + " First....";
            }

            var VehicleStatus = ctxTFAT.TfatVehicleStatus.Where(x => x.refParentKey == fMMaster.TableKey).FirstOrDefault();
            var VehicleStatusHistory = ctxTFAT.tfatVehicleStatusHistory.Where(x => x.refParentKey == fMMaster.TableKey).FirstOrDefault();
            if (VehicleStatus != null)
            {
                ctxTFAT.TfatVehicleStatus.Remove(VehicleStatus);
            }
            if (VehicleStatusHistory != null)
            {
                ctxTFAT.tfatVehicleStatusHistory.Remove(VehicleStatusHistory);
            }


            foreach (var item in mLedgerList)
            {
                var Outstanding = ctxTFAT.Outstanding.Where(x => x.Srl == item.Srl && x.TableRefKey == item.TableKey).ToList();
                ctxTFAT.Outstanding.RemoveRange(Outstanding);
            }
            ctxTFAT.Ledger.RemoveRange(mLedgerList);
            ctxTFAT.FMVouRel.RemoveRange(MfMVOUREL);
            ctxTFAT.FmCatchChargesInfo.RemoveRange(FmCharges);

            var GetRemarkDocList = ctxTFAT.Narration.Where(x => x.Srl == fMMaster.FmNo.ToString() && x.Type == "FM000").ToList();
            if (GetRemarkDocList != null)
            {
                ctxTFAT.Narration.RemoveRange(GetRemarkDocList);
            }

            var AuthorisationEntry = ctxTFAT.Authorisation.Where(x => x.TableKey == fMMaster.TableKey.ToString()).FirstOrDefault();
            if (AuthorisationEntry != null)
            {
                ctxTFAT.Authorisation.Remove(AuthorisationEntry);
            }

            ctxTFAT.FMMaster.Remove(fMMaster);
            ctxTFAT.FMROUTETable.RemoveRange(FMROUTETables);

            foreach (var item in lCMasters)
            {
                item.DispachFM = 0;
                ctxTFAT.Entry(item).State = EntityState.Modified;
            }

            var VehicleKm = ctxTFAT.VehicleKmMaintainMa.Where(x => x.FMRefTablekey.ToString() == mModel.Document).ToList();
            ctxTFAT.VehicleKmMaintainMa.RemoveRange(VehicleKm);

            var FMAttachMent = ctxTFAT.Attachment.Where(x => x.Type == "FM000" && x.Srl == fMMaster.FmNo.ToString()).ToList();
            foreach (var item in FMAttachMent)
            {
                if (System.IO.File.Exists(item.FilePath))
                {
                    System.IO.File.Delete(item.FilePath);
                }
            }
            ctxTFAT.Attachment.RemoveRange(FMAttachMent);


            ctxTFAT.SaveChanges();
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, fMMaster.ParentKey, fMMaster.Date, fMMaster.Freight, fMMaster.BroCode, "Delete Freight Memo :" + fMMaster.FmNo, "A");

            return "Success";
        }

        public ActionResult SaveDraftData(FMVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        string Msg = DeleteStateDraftMaster(mModel);
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

                    FMMasterDraft fMMaster = new FMMasterDraft();




                    if (ctxTFAT.FMMasterDraft.Where(x => (x.FmNo.ToString() == mModel.Document)).FirstOrDefault() != null)
                    {
                        fMMaster = ctxTFAT.FMMasterDraft.Where(x => (x.FmNo.ToString() == mModel.Document)).FirstOrDefault();
                        mAdd = false;
                    }



                    if (mModel.Mode == "Add")
                    {
                        fMMaster.FmNo = (mModel.FMNO);
                        fMMaster.CreateDate = DateTime.Now;
                        fMMaster.LoginBranch = mbranchcode;

                    }


                    TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.From).FirstOrDefault();
                    string Parent = "";
                    if (tfatBranch.Category == "Area")
                    {
                        var child = GetChildGrp(mModel.From);
                        if (ctxTFAT.TfatBranch.Where(x => child.Contains(x.Code) && x.Category == "SubBranch").FirstOrDefault() != null)
                        {
                            Parent = ctxTFAT.TfatBranch.Where(x => child.Contains(x.Code) && x.Category == "SubBranch").Select(x => x.Code).FirstOrDefault();
                        }
                        else
                        {
                            Parent = ctxTFAT.TfatBranch.Where(x => child.Contains(x.Code) && x.Category == "Branch").Select(x => x.Code).FirstOrDefault();
                        }

                        Parent = ctxTFAT.TfatBranch.Where(x => x.Code == tfatBranch.Grp).Select(x => x.Code).FirstOrDefault();


                    }
                    else
                    {
                        Parent = mModel.From;
                    }

                    fMMaster.DraftName = mModel.Draft_Name;
                    fMMaster.Branch = Parent;
                    fMMaster.Date = ConvertDDMMYYTOYYMMDD(mModel.FM_Date.ToString());
                    fMMaster.Time = mModel.FM_Time;
                    fMMaster.CurrBranch = fMMaster.Branch == null ? mbranchcode : fMMaster.Branch;
                    fMMaster.FmStatus = "W";
                    fMMaster.VehicleStatus = mModel.VehicleGroup;
                    fMMaster.TruckNo = mModel.VehicleNo;
                    fMMaster.BroCode = mModel.Broker;
                    fMMaster.KM = Convert.ToDouble(mModel.KM);
                    fMMaster.FromBranch = mModel.From;
                    fMMaster.ToBranch = mModel.To;
                    fMMaster.VehicleCategory = mModel.VehicleCategory;
                    fMMaster.PayLoad = Convert.ToInt32(mModel.PayLoad.ToString());
                    fMMaster.ReceiptNo = mModel.ReceiptNo;
                    if (mModel.VehicleGroup == "100001")
                    {
                        fMMaster.Driver = mModel.DriverName;
                    }
                    else
                    {
                        fMMaster.Driver = mModel.DriverCode;

                        VehicleMaster vehicle = ctxTFAT.VehicleMaster.Where(x => x.Code == fMMaster.TruckNo).FirstOrDefault();
                        DriverMaster driver = ctxTFAT.DriverMaster.Where(x => x.Code == fMMaster.Driver).FirstOrDefault();

                        fMMaster.VehiclePostAc = vehicle.PostAc;
                        fMMaster.DriverPostAc = driver.Posting;
                        fMMaster.VehicleMaintainDriverAC = vehicle.MaintainDriverAC;
                        fMMaster.VehicleArAp = vehicle.ARAP;
                        fMMaster.VehicleFmPostReq = vehicle.PostReq;

                    }
                    fMMaster.LicenCeNo = mModel.LicenceNo;
                    if (mModel.LicenceExpDate == "01/01/0001" || mModel.LicenceExpDate == null)
                    {
                        fMMaster.LicenceExpDate = null;
                    }
                    else
                    {
                        fMMaster.LicenceExpDate = ConvertDDMMYYTOYYMMDD(mModel.LicenceExpDate);
                    }
                    fMMaster.OwnerName = mModel.Owner;
                    fMMaster.ChallanNo = mModel.ChallanNo;
                    fMMaster.ContactNo = mModel.ContactNo;
                    fMMaster.Freight = mModel.Freight;
                    fMMaster.Adv = mModel.Advance;

                    fMMaster.PayAt = mModel.PayableAt;
                    fMMaster.ChgAmt1 = mModel.Charges.Where(x => x.Fld == "F001").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F001") : 0;
                    fMMaster.ChgAmt2 = mModel.Charges.Where(x => x.Fld == "F002").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F002") : 0;
                    fMMaster.ChgAmt3 = mModel.Charges.Where(x => x.Fld == "F003").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F003") : 0;
                    fMMaster.ChgAmt4 = mModel.Charges.Where(x => x.Fld == "F004").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F004") : 0;
                    fMMaster.ChgAmt5 = mModel.Charges.Where(x => x.Fld == "F005").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F005") : 0;
                    fMMaster.ChgAmt6 = mModel.Charges.Where(x => x.Fld == "F006").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F006") : 0;
                    fMMaster.ChgAmt7 = mModel.Charges.Where(x => x.Fld == "F007").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F007") : 0;
                    fMMaster.ChgAmt8 = mModel.Charges.Where(x => x.Fld == "F008").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F008") : 0;
                    fMMaster.ChgAmt9 = mModel.Charges.Where(x => x.Fld == "F009").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F009") : 0;
                    fMMaster.ChgAmt10 = mModel.Charges.Where(x => x.Fld == "F010").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F010") : 0;

                    fMMaster.OldRate = mModel.Freight;
                    fMMaster.Opening = false;
                    //lCMaster.Other = 0;
                    fMMaster.status = true;
                    fMMaster.NetBalance = (mModel.Balance);
                    fMMaster.Balance = (mModel.Balance);
                    fMMaster.TDSAmt = (mModel.TDSAmt);
                    fMMaster.TotlExp = (mModel.TotExp);
                    fMMaster.TotlAdvExp = (mModel.TotAdvExp);
                    fMMaster.Remark = mModel.Remark;
                    //lCMaster.PayLoad = "0";
                    fMMaster.Payment = mModel.Payment;
                    fMMaster.ScheduleRequired = mModel.ScheduleRequired;
                    //lCMaster.sch = vehicleMaster.ScheduleDateTime;

                    fMMaster.ScheduleKMReq = false;

                    //lCMaster.LoadPayload = mModel.PayLoad;
                    fMMaster.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    fMMaster.ENTEREDBY = muserid;
                    fMMaster.AUTHORISE = mauthorise;
                    fMMaster.AUTHIDS = muserid;
                    fMMaster.ParentKey = mbranchcode + "FM000" + mperiod.Substring(0, 2) + mModel.FMNO;
                    fMMaster.TableKey = mbranchcode + "FM000" + mperiod.Substring(0, 2) + 1.ToString("D3") + mModel.FMNO;

                    LogisticsFlow systemFlow = ctxTFAT.LogisticsFlow.FirstOrDefault();
                    if (systemFlow == null)
                    {
                        systemFlow = new LogisticsFlow();
                    }
                    fMMaster.ScheduleFollowup = Convert.ToBoolean(systemFlow.ScheduleFollowUp);

                    List<FMAttachment> SessionAttachList = Session["FMAttachmentList"] as List<FMAttachment>;


                    if (mAdd == true)
                    {
                        ctxTFAT.FMMasterDraft.Add(fMMaster);
                        //ctxTFAT.LockSystem.Add(lockSystem);
                    }
                    else
                    {
                        ctxTFAT.Entry(fMMaster).State = EntityState.Modified;
                        //ctxTFAT.Entry(lockSystem).State = EntityState.Modified;
                    }

                    ctxTFAT.SaveChanges();


                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
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

        public string DeleteStateDraftMaster(FMVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return "Code not Entered..";
            }

            FMMasterDraft fMMaster_Draft = ctxTFAT.FMMasterDraft.Where(x => x.FmNo.ToString() == mModel.Document).FirstOrDefault();
            if (fMMaster_Draft != null)
            {
                ctxTFAT.FMMasterDraft.Remove(fMMaster_Draft);
                ctxTFAT.SaveChanges();
                return "Success";
            }
            return "Success";
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

        #endregion

        #region Other Some Method To Daily Use In FM like (FmFullDetails,SetSchedule,Create Fm Routes,Destination(Route),ListBox,DeleteDestination)

        public ActionResult FmActivity(String Fmno)
        {
            List<FmDetailsVM> fmDetailsVMs = new List<FmDetailsVM>();

            var RouteTable = ctxTFAT.FMROUTETable.Where(x => x.Parentkey.ToString() == Fmno).ToList();
            if (RouteTable.Count() > 0)
            {
                foreach (var item in RouteTable.Where(x => x.RouteType == "R"))
                {
                    FmDetailsVM fmDetailsVM = new FmDetailsVM();
                    List<ArrivalDetailsVM> arrivalDetails = new List<ArrivalDetailsVM>();
                    List<DispatchDetailsVM> dispatchDetails = new List<DispatchDetailsVM>();
                    List<LorryChallanVM> loading = new List<LorryChallanVM>();
                    List<LorryChallanVM> Unloading = new List<LorryChallanVM>();
                    var GetArrivalList = RouteTable.Where(x => x.SubRoute == item.SequenceRoute).ToList();
                    foreach (var item1 in GetArrivalList)
                    {
                        if (String.IsNullOrEmpty(fmDetailsVM.Branch))
                        {
                            fmDetailsVM.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == item1.RouteVia).Select(x => x.Name).FirstOrDefault();
                        }

                        ArrivalDetailsVM arrival = new ArrivalDetailsVM();
                        arrival.Area = ctxTFAT.TfatBranch.Where(x => x.Code == item1.RouteVia).Select(x => x.Name).FirstOrDefault();
                        arrival.Date = item1.ArrivalDate == null ? null : item1.ArrivalDate.Value.ToShortDateString();
                        arrival.KM = item1.ArrivalKM == null ? null : item1.ArrivalKM.Value.ToString();
                        arrival.Remark = item1.ArrivalRemark == null ? null : item1.ArrivalRemark;
                        arrivalDetails.Add(arrival);

                        DispatchDetailsVM dispatch = new DispatchDetailsVM();
                        dispatch.Area = ctxTFAT.TfatBranch.Where(x => x.Code == item1.RouteVia).Select(x => x.Name).FirstOrDefault();
                        dispatch.Date = item1.DispatchDate == null ? null : item1.DispatchDate.Value.ToShortDateString();
                        dispatch.KM = item1.DispatchKM == null ? null : item1.DispatchKM.Value.ToString();
                        dispatch.Remark = item1.DispatchRemark == null ? null : item1.DispatchRemark;
                        dispatchDetails.Add(dispatch);

                        var LcmasterList = ctxTFAT.LCMaster.Where(x => x.FMRefTablekey == Fmno).ToList();
                        if (LcmasterList.Count() > 0)
                        {
                            foreach (var item2 in LcmasterList)
                            {
                                LorryChallanVM lorry = new LorryChallanVM();
                                var LrDetails = ctxTFAT.LCDetail.Where(x => x.LCRefTablekey == item2.TableKey).ToList();

                                lorry.LcNo = item2.LCno.ToString();
                                lorry.From = ctxTFAT.TfatBranch.Where(x => x.Code == item2.FromBranch).Select(x => x.Name).FirstOrDefault();
                                lorry.TO = ctxTFAT.TfatBranch.Where(x => x.Code == item2.ToBranch).Select(x => x.Name).FirstOrDefault();
                                lorry.Date = item2.LoadDate.Value.ToShortDateString();
                                lorry.Qty = LrDetails.Sum(x => x.LrQty).ToString();
                                lorry.Weight = LrDetails.Sum(x => x.LRActWeight).ToString();

                                List<LorryReVM> Loadin = new List<LorryReVM>();
                                int i = 1;
                                foreach (var item3 in LrDetails)
                                {
                                    LorryReVM lorryRe = new LorryReVM();
                                    lorryRe.Srno = i.ToString();
                                    lorryRe.LRNo = item3.LRno.ToString();
                                    lorryRe.From = ctxTFAT.TfatBranch.Where(x => x.Code == item3.FromBranch).Select(x => x.Name).FirstOrDefault();
                                    lorryRe.To = ctxTFAT.TfatBranch.Where(x => x.Code == item3.ToBranch).Select(x => x.Name).FirstOrDefault();
                                    lorryRe.Qty = item3.LrQty.ToString();
                                    lorryRe.Weight = item3.LRActWeight.ToString();
                                    ++i;
                                    Loadin.Add(lorryRe);
                                }
                                lorry.LorryReVMs = Loadin;
                                loading.Add(lorry);
                            }

                            foreach (var item2 in LcmasterList)
                            {
                                LorryChallanVM lorry = new LorryChallanVM();
                                var LrDetails = ctxTFAT.LCDetail.Where(x => x.LCRefTablekey == item2.TableKey).ToList();
                                var ParentkeyOfLR = LrDetails.Select(x => x.ParentKey).ToList();
                                var UnloadDetails = ctxTFAT.LRStock.Where(x => ParentkeyOfLR.Contains(x.TableKey) && x.Type == "LR").ToList();

                                lorry.LcNo = item2.LCno.ToString();
                                lorry.From = ctxTFAT.TfatBranch.Where(x => x.Code == item2.FromBranch).Select(x => x.Name).FirstOrDefault();
                                lorry.TO = ctxTFAT.TfatBranch.Where(x => x.Code == item2.ToBranch).Select(x => x.Name).FirstOrDefault();
                                lorry.Date = item2.LoadDate.Value.ToShortDateString();
                                lorry.Qty = LrDetails.Sum(x => x.LrQty).ToString();
                                lorry.Weight = LrDetails.Sum(x => x.LRActWeight).ToString();

                                List<LorryReVM> Loadin = new List<LorryReVM>();
                                int i = 1;
                                foreach (var item3 in LrDetails)
                                {
                                    LorryReVM lorryRe = new LorryReVM();
                                    lorryRe.Srno = i.ToString();
                                    lorryRe.LRNo = item3.LRno.ToString();
                                    lorryRe.From = ctxTFAT.TfatBranch.Where(x => x.Code == item3.FromBranch).Select(x => x.Name).FirstOrDefault();
                                    lorryRe.To = ctxTFAT.TfatBranch.Where(x => x.Code == item3.ToBranch).Select(x => x.Name).FirstOrDefault();
                                    lorryRe.Qty = UnloadDetails.Where(x => x.LrNo.ToString() == item3.LRno.ToString()).Select(x => x.UnloadGodwonQty).FirstOrDefault().ToString();
                                    lorryRe.Weight = item3.LRActWeight.ToString();
                                    ++i;
                                    Loadin.Add(lorryRe);
                                }
                                lorry.LorryReVMs = Loadin;
                                Unloading.Add(lorry);
                            }
                        }
                    }
                    fmDetailsVM.ArrivalDetails = arrivalDetails;
                    fmDetailsVM.DispatchDetails = dispatchDetails;
                    fmDetailsVM.LoadingDetails = loading;
                    fmDetailsVM.UnLoadingDetails = Unloading;
                    fmDetailsVMs.Add(fmDetailsVM);
                }
            }

            var html = ViewHelper.RenderPartialView(this, "FmWorkDetails", fmDetailsVMs);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetSchedule(string ArrivalDate, string ArrivalTime, string Fmno, string ArrivalKM, string ArrivalRemark, string RECORDKEY)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    int RECOR = Convert.ToInt32(RECORDKEY);
                    FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == Fmno).FirstOrDefault();
                    DateTime NewDate = new DateTime();
                    var Route = ctxTFAT.FMROUTETable.Where(x => x.Parentkey.ToString() == Fmno && x.RECORDKEY >= RECOR).OrderBy(x => x.SubRoute).ToList();
                    var Date = ArrivalDate.Split('/');
                    var Date1 = ArrivalTime.Split(':');

                    NewDate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Date1[0]), Convert.ToInt32(Date1[1]), 00);
                    int KMMaintain = 0;
                    {
                        //for (int i = 0; i < Route.Count(); i++)
                        //{
                        //    string Value = (i).ToString();
                        //    if (i == 0)
                        //    {
                        //        var VihicleActivity = ctxTFAT.TfatBranch.Where(x => x.Code == fMMaster.FromBranch).Select(x => x.VehicleWaitTime).FirstOrDefault();

                        //        FMROUTETable fM_ROUTE = Route[i];

                        //        fM_ROUTE.VehicleActivity = VihicleActivity;
                        //        NewDate = NewDate.AddHours(Convert.ToInt32(VihicleActivity) / 60).AddMinutes(Convert.ToInt32(VihicleActivity) % 60);
                        //        fM_ROUTE.DispatchSchDate = ConvertDDMMYYTOYYMMDD(NewDate.ToShortDateString());
                        //        fM_ROUTE.DispatchReSchDate = ConvertDDMMYYTOYYMMDD(NewDate.ToShortDateString());
                        //        fM_ROUTE.DispatchSchTime = NewDate.ToString("HH:mm");
                        //        fM_ROUTE.DispatchReSchTime = NewDate.ToString("HH:mm");
                        //        fM_ROUTE.ArrivalDate = ConvertDDMMYYTOYYMMDD(ArrivalDate);
                        //        fM_ROUTE.ArrivalTime = ArrivalTime;
                        //        fM_ROUTE.ArrivalKM = Convert.ToInt32(ArrivalKM);
                        //        fM_ROUTE.ArrivalRemark = ArrivalRemark;
                        //        KMMaintain = Convert.ToInt32(ArrivalKM);

                        //        fM_ROUTE.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                        //        fM_ROUTE.ENTEREDBY = muserid;
                        //        fM_ROUTE.AUTHORISE = mauthorise;
                        //        fM_ROUTE.AUTHIDS = muserid;
                        //        //ctxTFAT.FMROUTETable.Add(fM_ROUTE);
                        //        ctxTFAT.Entry(fM_ROUTE).State = EntityState.Modified;

                        //    }
                        //    else
                        //    {
                        //        FMROUTETable FMROUTETable = Route[i];

                        //        var VihicleActivity = ctxTFAT.TfatBranch.Where(x => x.Code == FMROUTETable.RouteVia).Select(x => x.VehicleWaitTime).FirstOrDefault();
                        //        var dd = Route[i - 1];
                        //        var kilometerMasters = ctxTFAT.KilometerMasterRef.Where(x => x.FromBranch == dd.RouteVia && x.ToBranch == FMROUTETable.RouteVia).FirstOrDefault();
                        //        VehicleRates allVehicle = ctxTFAT.VehicleRates.Where(x => x.VehicleNo == fMMaster.TruckNo).FirstOrDefault();

                        //        //var Hours_And_Time = kilometerMasters.Time.Split('.');
                        //        string[] Hours_And_Time = new string[2];
                        //        if (kilometerMasters == null)
                        //        {
                        //            Hours_And_Time[0] = "0";
                        //            Hours_And_Time[1] = "0";
                        //        }
                        //        else
                        //        {
                        //            if (allVehicle == null)
                        //            {
                        //                Hours_And_Time[0] = "0";
                        //                Hours_And_Time[1] = "0";
                        //            }
                        //            else
                        //            {
                        //                if (string.IsNullOrEmpty(allVehicle.Reporting) == false)
                        //                {
                        //                    Hours_And_Time = allVehicle.Reporting.Split('.');
                        //                }
                        //                else
                        //                {
                        //                    Hours_And_Time[0] = "0";
                        //                    Hours_And_Time[1] = "0";
                        //                }
                        //            }


                        //        }

                        //        FMROUTETable.VehicleActivity = VihicleActivity;
                        //        FMROUTETable.Kilometers = Convert.ToInt32(kilometerMasters == null ? 0 : Convert.ToInt32(kilometerMasters.KM));
                        //        FMROUTETable.KilometersTime = allVehicle == null ? "0.0" : allVehicle.Reporting;

                        //        var minute = 0;
                        //        if (Hours_And_Time.Count() == 1)
                        //        {
                        //            minute = 0;
                        //        }
                        //        else
                        //        {
                        //            minute = Convert.ToInt32(Hours_And_Time[1]);
                        //        }
                        //        NewDate = NewDate.AddHours(Convert.ToInt32(Hours_And_Time[0])).AddMinutes(Convert.ToInt32(minute));
                        //        FMROUTETable.ArrivalSchDate = ConvertDDMMYYTOYYMMDD(NewDate.ToShortDateString());
                        //        FMROUTETable.ArrivalSchTime = NewDate.ToString("HH:mm");
                        //        FMROUTETable.ArrivalSchKm = (Convert.ToInt32(KMMaintain) + Convert.ToInt32(kilometerMasters == null ? 0 : Convert.ToInt32(kilometerMasters.KM)));
                        //        FMROUTETable.ArrivalReSchDate = ConvertDDMMYYTOYYMMDD(NewDate.ToShortDateString());
                        //        FMROUTETable.ArrivalReSchTime = NewDate.ToString("HH:mm");
                        //        FMROUTETable.ArrivalReSchKm = (Convert.ToInt32(KMMaintain) + Convert.ToInt32(kilometerMasters == null ? 0 : Convert.ToInt32(kilometerMasters.KM)));
                        //        NewDate = NewDate.AddHours(Convert.ToInt32(VihicleActivity) / 60).AddMinutes(Convert.ToInt32(VihicleActivity) % 60);
                        //        FMROUTETable.DispatchSchDate = ConvertDDMMYYTOYYMMDD(NewDate.ToShortDateString());
                        //        FMROUTETable.DispatchSchTime = NewDate.ToString("HH:mm");
                        //        FMROUTETable.DispatchReSchDate = ConvertDDMMYYTOYYMMDD(NewDate.ToShortDateString());
                        //        FMROUTETable.DispatchReSchTime = NewDate.ToString("HH:mm");

                        //        FMROUTETable.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                        //        FMROUTETable.ENTEREDBY = muserid;
                        //        FMROUTETable.AUTHORISE = mauthorise;
                        //        FMROUTETable.AUTHIDS = muserid;

                        //        ctxTFAT.Entry(FMROUTETable).State = EntityState.Modified;

                        //    }
                        //}
                    }
                    for (int i = 0; i < Route.Count(); i++)
                    {
                        string Value = (i).ToString();
                        if (i == 0)
                        {
                            var VihicleActivity = ctxTFAT.TfatBranch.Where(x => x.Code == fMMaster.FromBranch).Select(x => x.VehicleWaitTime).FirstOrDefault();

                            FMROUTETable fM_ROUTE = Route[i];

                            fM_ROUTE.VehicleActivity = VihicleActivity;
                            int Hours = 0, minutes = 0;
                            if (!String.IsNullOrEmpty(VihicleActivity))
                            {
                                var GetTime = VihicleActivity.Split(':');
                                Hours = Convert.ToInt32(GetTime[0]);
                                if (GetTime.Length == 2)
                                {
                                    minutes = Convert.ToInt32(GetTime[1]);
                                }
                            }
                            NewDate = NewDate.AddHours(Convert.ToInt32(Hours)).AddMinutes(minutes);
                            fM_ROUTE.DispatchSchDate = ConvertDDMMYYTOYYMMDD(NewDate.ToShortDateString());
                            fM_ROUTE.DispatchReSchDate = ConvertDDMMYYTOYYMMDD(NewDate.ToShortDateString());
                            fM_ROUTE.DispatchSchTime = NewDate.ToString("HH:mm");
                            fM_ROUTE.DispatchReSchTime = NewDate.ToString("HH:mm");
                            fM_ROUTE.ArrivalDate = ConvertDDMMYYTOYYMMDD(ArrivalDate);
                            fM_ROUTE.ArrivalTime = ArrivalTime;
                            fM_ROUTE.ArrivalKM = Convert.ToInt32(ArrivalKM);

                            fM_ROUTE.ArrivalRemark = ArrivalRemark;


                            //fM_ROUTE.DispatchDate = null;
                            //fM_ROUTE.DispatchTime = null;
                            //fM_ROUTE.DispatchKM = null;

                            fM_ROUTE.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                            fM_ROUTE.ENTEREDBY = muserid;
                            fM_ROUTE.AUTHORISE = mauthorise;
                            fM_ROUTE.AUTHIDS = muserid;
                            //ctxTFAT.FMROUTETable.Add(fM_ROUTE);
                            ctxTFAT.Entry(fM_ROUTE).State = EntityState.Modified;
                        }
                        else
                        {
                            FMROUTETable FMROUTETable = Route[i];
                            var FetLocalKM = false;
                            var VihicleActivity = ctxTFAT.TfatBranch.Where(x => x.Code == FMROUTETable.RouteVia).Select(x => x.VehicleWaitTime).FirstOrDefault();
                            var dd = Route[i - 1];
                            var kilometerMasters = ctxTFAT.KilometerMasterRef.Where(x => x.FromBranch == dd.RouteVia && x.ToBranch == FMROUTETable.RouteVia).FirstOrDefault();

                            var StartPoint = ctxTFAT.TfatBranch.Where(x => x.Code == dd.RouteVia).FirstOrDefault();
                            var EndPoint = ctxTFAT.TfatBranch.Where(x => x.Code == FMROUTETable.RouteVia).FirstOrDefault();

                            if (kilometerMasters == null)
                            {
                                FetLocalKM = true;
                                kilometerMasters = ctxTFAT.KilometerMasterRef.Where(x => x.FromBranch == dd.RouteVia && x.ToBranch == EndPoint.Grp).FirstOrDefault();
                                if (kilometerMasters == null)
                                {
                                    kilometerMasters = ctxTFAT.KilometerMasterRef.Where(x => x.FromBranch == StartPoint.Grp && x.ToBranch == FMROUTETable.RouteVia).FirstOrDefault();
                                    if (kilometerMasters == null)
                                    {
                                        kilometerMasters = ctxTFAT.KilometerMasterRef.Where(x => x.FromBranch == StartPoint.Grp && x.ToBranch == EndPoint.Grp).FirstOrDefault();
                                    }
                                }
                            }
                            LocalKMandTimeMaRef localKMand = new LocalKMandTimeMaRef();
                            if (FetLocalKM)
                            {
                                localKMand = ctxTFAT.LocalKMandTimeMaRef.Where(x => x.FromBranch == kilometerMasters.FromBranch && x.ToBranch == kilometerMasters.ToBranch).FirstOrDefault();
                            }







                            //LocalKMandTimeMaRef localKMand = new LocalKMandTimeMaRef();
                            //if (StartPoint.Category == "Area" || EndPoint.Category == "Area")
                            //{
                            //    if (StartPoint.Category == "Area" && EndPoint.Category == "Area")
                            //    {
                            //        localKMand = ctxTFAT.LocalKMandTimeMaRef.Where(x => x.FromBranch == dd.RouteVia && x.ToBranch == FMROUTETable.RouteVia).FirstOrDefault();
                            //        if (localKMand == null)
                            //        {
                            //            localKMand = ctxTFAT.LocalKMandTimeMaRef.Where(x => x.FromBranch == StartPoint.Grp && x.ToBranch == EndPoint.Grp).FirstOrDefault();
                            //        }
                            //    }
                            //    else if (StartPoint.Category == "Area")
                            //    {
                            //        localKMand = ctxTFAT.LocalKMandTimeMaRef.Where(x => x.FromBranch == dd.RouteVia && x.ToBranch == FMROUTETable.RouteVia).FirstOrDefault();
                            //        if (localKMand == null)
                            //        {
                            //            localKMand = ctxTFAT.LocalKMandTimeMaRef.Where(x => x.FromBranch == StartPoint.Grp && x.ToBranch == FMROUTETable.RouteVia).FirstOrDefault();
                            //        }
                            //    }
                            //    else
                            //    {
                            //        localKMand = ctxTFAT.LocalKMandTimeMaRef.Where(x => x.FromBranch == dd.RouteVia && x.ToBranch == FMROUTETable.RouteVia).FirstOrDefault();
                            //        if (localKMand == null)
                            //        {
                            //            localKMand = ctxTFAT.LocalKMandTimeMaRef.Where(x => x.FromBranch == dd.RouteVia && x.ToBranch == EndPoint.Grp).FirstOrDefault();
                            //        }
                            //    }
                            //}


                            VehicleRates allVehicle = ctxTFAT.VehicleRates.Where(x => x.VehicleNo == fMMaster.TruckNo).FirstOrDefault();

                            //var Hours_And_Time = kilometerMasters.Time.Split('.');
                            string[] Hours_And_Time = new string[2];

                            string MainHr = "0", LocalHr = "0", MainMinute = "0", LocalMinute = "0";
                            int MainKM = 0, LocalKM = 0;
                            if (kilometerMasters != null)
                            {
                                if (!String.IsNullOrEmpty(kilometerMasters.ReportTime))
                                {
                                    var GetTime = kilometerMasters.ReportTime.Split(':');
                                    MainHr = GetTime[0];
                                    MainMinute = GetTime[1];
                                }

                                if (!String.IsNullOrEmpty(kilometerMasters.KM))
                                {
                                    MainKM = Convert.ToInt32(kilometerMasters.KM);
                                }


                            }

                            if (localKMand != null)
                            {
                                if (!String.IsNullOrEmpty(localKMand.ReportTime))
                                {
                                    var GetTime = localKMand.ReportTime.Split(':');
                                    LocalHr = GetTime[0];
                                    LocalMinute = GetTime[1];
                                }

                                if (!String.IsNullOrEmpty(localKMand.KM))
                                {
                                    LocalKM = Convert.ToInt32(localKMand.KM);
                                }
                            }

                            Hours_And_Time[0] = (Convert.ToInt32(MainHr) + Convert.ToInt32(LocalHr)).ToString();
                            Hours_And_Time[1] = (Convert.ToInt32(MainMinute) + Convert.ToInt32(LocalMinute)).ToString();
                            int TotalKM = MainKM + LocalKM;


                            FMROUTETable.VehicleActivity = VihicleActivity;
                            FMROUTETable.Kilometers = TotalKM;
                            FMROUTETable.KilometersTime = Hours_And_Time[0] + "." + Hours_And_Time[1];


                            var minute = Convert.ToInt32(Hours_And_Time[1]);

                            NewDate = NewDate.AddHours(Convert.ToInt32(Hours_And_Time[0])).AddMinutes(Convert.ToInt32(minute));
                            FMROUTETable.ArrivalSchDate = ConvertDDMMYYTOYYMMDD(NewDate.ToShortDateString());
                            FMROUTETable.ArrivalSchTime = NewDate.ToString("HH:mm");
                            FMROUTETable.ArrivalSchKm = (Convert.ToInt32(KMMaintain) + TotalKM);
                            FMROUTETable.ArrivalReSchDate = ConvertDDMMYYTOYYMMDD(NewDate.ToShortDateString());
                            FMROUTETable.ArrivalReSchTime = NewDate.ToString("HH:mm");
                            FMROUTETable.ArrivalReSchKm = (Convert.ToInt32(KMMaintain) + TotalKM);

                            int Hours = 0, minutes = 0;
                            if (!String.IsNullOrEmpty(VihicleActivity))
                            {
                                var GetTime = VihicleActivity.Split(':');
                                Hours = Convert.ToInt32(GetTime[0]);
                                if (GetTime.Length == 2)
                                {
                                    minutes = Convert.ToInt32(GetTime[1]);
                                }
                            }
                            NewDate = NewDate.AddHours(Hours).AddMinutes(minutes);
                            FMROUTETable.DispatchSchDate = ConvertDDMMYYTOYYMMDD(NewDate.ToShortDateString());
                            FMROUTETable.DispatchSchTime = NewDate.ToString("HH:mm");
                            FMROUTETable.DispatchReSchDate = ConvertDDMMYYTOYYMMDD(NewDate.ToShortDateString());
                            FMROUTETable.DispatchReSchTime = NewDate.ToString("HH:mm");
                            KMMaintain = KMMaintain + TotalKM;




                            FMROUTETable.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                            FMROUTETable.ENTEREDBY = muserid;
                            FMROUTETable.AUTHORISE = mauthorise;
                            FMROUTETable.AUTHIDS = muserid;
                            ctxTFAT.Entry(FMROUTETable).State = EntityState.Modified;

                        }
                    }

                    FMROUTETable table = Route[0];
                    var VehicleMainKM = ctxTFAT.VehicleKmMaintainMa.Where(x => x.FMRefTablekey == table.Parentkey && x.Parent == table.Parent && x.RouteVia == table.RouteVia && x.ComingFrom == "Arrival").FirstOrDefault();
                    if (VehicleMainKM == null)
                    {
                        VehicleMainKM = new VehicleKmMaintainMa();
                        VehicleMainKM.ComingFrom = "Arrival";
                        VehicleMainKM.Date = table.ArrivalDate.Value;
                        VehicleMainKM.EntryDate = DateTime.Now;
                        VehicleMainKM.KM = Convert.ToInt32(table.ArrivalKM);
                        VehicleMainKM.Time = table.ArrivalTime;
                        VehicleMainKM.VehicleNo = table.VehicleNo;
                        VehicleMainKM.FMno = table.FmNo;
                        VehicleMainKM.Parent = table.Parent;
                        VehicleMainKM.RouteVia = table.RouteVia;
                        VehicleMainKM.ENTEREDBY = muserid;
                        VehicleMainKM.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                        VehicleMainKM.AUTHORISE = mauthorise;
                        VehicleMainKM.AUTHIDS = muserid;
                        VehicleMainKM.FMRefTablekey = mbranchcode + "FM000" + mperiod.Substring(0, 2) + 1.ToString("D3") + fMMaster.FmNo.ToString();

                        ctxTFAT.VehicleKmMaintainMa.Add(VehicleMainKM);
                    }
                    else
                    {
                        VehicleMainKM.Date = ConvertDDMMYYTOYYMMDD(ArrivalDate);
                        VehicleMainKM.Time = ArrivalTime;
                        VehicleMainKM.KM = Convert.ToInt32(ArrivalKM);
                        ctxTFAT.Entry(VehicleMainKM).State = EntityState.Modified;
                    }

                    VehicleMaster vehicle = ctxTFAT.VehicleMaster.Where(X => X.Code.ToUpper().Trim() == fMMaster.TruckNo.ToUpper().Trim()).FirstOrDefault();
                    if (vehicle == null)
                    {
                        HireVehicleMaster hireVehicle = ctxTFAT.HireVehicleMaster.Where(X => X.Code.ToUpper().Trim() == fMMaster.TruckNo.ToUpper().Trim()).FirstOrDefault();
                        hireVehicle.KM = Convert.ToInt32(KMMaintain);
                        ctxTFAT.Entry(hireVehicle).State = EntityState.Modified;

                    }
                    else
                    {
                        vehicle.KM = Convert.ToInt32(KMMaintain);
                        ctxTFAT.Entry(vehicle).State = EntityState.Modified;

                    }

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    ctxTFAT.SaveChanges();
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

        private void AddFmRealtion(FMVM mModel, FMMaster fMMaster)
        {

            var FmRelList = ctxTFAT.FMROUTETable.Where(x => x.Parentkey.ToString() == mModel.Document.ToString()).ToList();
            ctxTFAT.FMROUTETable.RemoveRange(FmRelList);

            var VehicleKm = ctxTFAT.VehicleKmMaintainMa.Where(x => x.FMRefTablekey.ToString() == mModel.Document.ToString()).ToList();
            ctxTFAT.VehicleKmMaintainMa.RemoveRange(VehicleKm);



            string DestBrNAme = ""; int totalRoute = 0;

            String[] DestSequence = new String[] { };
            String[] DestCodearray1 = new String[] { };
            if (!String.IsNullOrEmpty(mModel.DestCombo_Sequence))
            {
                DestSequence = mModel.DestCombo_Sequence.Split(',');
                totalRoute = DestSequence.Length;
                DestCodearray1 = mModel.DestCombo.Split(',');
            }

            #region New Route Define

            List<string> Route = new List<string>();
            List<string> ParentRoot = new List<string>();

            #region From Destination
            var RouteCode = mModel.From;
            TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == RouteCode).FirstOrDefault();
            Route.Add(mbranchcode);
            ParentRoot.Add(mbranchcode);
            #endregion

            #region If From Area Select Any Other Branch Or Subbranch

            //var MbranchDetails = ctxTFAT.TfatBranch.Where(x => x.Code.Trim() == mbranchcode).FirstOrDefault();
            //if (MbranchDetails.Category == "Branch" || MbranchDetails.Category == "SubBranch" || MbranchDetails.Code == "HO0000")
            //{
            var GetFromAreaInfo = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.From).FirstOrDefault();
            if (GetFromAreaInfo.Category == "Area")
            {
                if (GetFromAreaInfo.Grp != "G00000")
                {
                    GetFromAreaInfo = ctxTFAT.TfatBranch.Where(x => x.Code == GetFromAreaInfo.Grp).FirstOrDefault();
                    if (GetFromAreaInfo.Category == "Branch")
                    {
                        //if (GetFromAreaInfo.Code.Trim() != mbranchcode)
                        {
                            Route.Add(mModel.From);
                            ParentRoot.Add(GetFromAreaInfo.Code);
                        }
                    }
                    else if (GetFromAreaInfo.Category == "SubBranch")
                    {
                        //GetFromAreaInfo = ctxTFAT.TfatBranch.Where(x => x.Code == GetFromAreaInfo.Grp).FirstOrDefault();
                        //if (GetFromAreaInfo.Code.Trim() != mbranchcode)
                        {
                            Route.Add(mModel.From);
                            ParentRoot.Add(GetFromAreaInfo.Code);
                        }
                    }
                    else
                    {
                        Route.Add(mModel.From);
                        ParentRoot.Add(mbranchcode);
                    }
                }
                else
                {
                    Route.Add(mModel.From);
                    ParentRoot.Add(mbranchcode);
                }
            }
            else
            {
                if (GetFromAreaInfo.Category == "Branch")
                {
                    //if (GetFromAreaInfo.Code.Trim() != mbranchcode)
                    {
                        Route.Add(mModel.From);
                        ParentRoot.Add(mModel.From);
                    }
                }
                else if (GetFromAreaInfo.Category == "SubBranch")
                {
                    GetFromAreaInfo = ctxTFAT.TfatBranch.Where(x => x.Code == GetFromAreaInfo.Grp).FirstOrDefault();
                    //if (GetFromAreaInfo.Code.Trim() != mbranchcode)
                    {
                        Route.Add(mModel.From);
                        ParentRoot.Add(mModel.From);
                    }
                }
                else
                {
                    Route.Add(mModel.From);
                    ParentRoot.Add(mbranchcode);
                }
            }

            //}


            #endregion

            #region Route List SortOut
            for (int i = 0; i < totalRoute; i++)
            {
                int index = Array.IndexOf(DestSequence, (i + 1).ToString());
                RouteCode = DestCodearray1[index];

                tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == RouteCode).FirstOrDefault();
                if (tfatBranch.Category == "Area")
                {
                    List<string> child = new List<string>();
                    if (RouteCode == "HO0000")
                    {
                        child.Add("HO0000");
                    }
                    else
                    {
                        child = ctxTFAT.TfatBranch.Where(x => x.Grp == tfatBranch.Grp || x.Code == tfatBranch.Grp).Select(x => x.Code).ToList();
                    }
                    if (ctxTFAT.TfatBranch.Where(x => child.Contains(x.Code) && x.Category == "SubBranch").FirstOrDefault() != null)
                    {
                        var Rout = ctxTFAT.TfatBranch.Where(x => child.Contains(x.Code) && x.Category == "SubBranch").Select(x => x.Code).FirstOrDefault();
                        Route.Add(tfatBranch.Code);
                        ParentRoot.Add(Rout);
                    }
                    else
                    {
                        var Rout = ctxTFAT.TfatBranch.Where(x => child.Contains(x.Code) && x.Category == "Branch").Select(x => x.Code).FirstOrDefault();
                        if (String.IsNullOrEmpty(Rout))
                        {
                            ParentRoot.Add(mbranchcode);
                        }
                        else
                        {
                            ParentRoot.Add(Rout);
                        }
                        Route.Add(tfatBranch.Code);
                    }
                }
                else
                {
                    Route.Add(RouteCode);
                    ParentRoot.Add(tfatBranch.Code);
                }
            }
            #endregion

            #region Last Destination
            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.To).FirstOrDefault();
            if (tfatBranch.Category == "Area")
            {
                List<string> child = new List<string>();
                if (mModel.To == "HO0000")
                {
                    child.Add("HO0000");
                }
                else
                {
                    child = ctxTFAT.TfatBranch.Where(x => x.Grp == tfatBranch.Grp || x.Code == tfatBranch.Grp).Select(x => x.Code).ToList();
                }
                if (ctxTFAT.TfatBranch.Where(x => child.Contains(x.Code) && x.Category == "SubBranch").FirstOrDefault() != null)
                {
                    var Rout = ctxTFAT.TfatBranch.Where(x => child.Contains(x.Code) && x.Category == "SubBranch").Select(x => x.Code).FirstOrDefault();
                    Route.Add(tfatBranch.Code);
                    ParentRoot.Add(Rout);
                }
                else
                {
                    var Rout = ctxTFAT.TfatBranch.Where(x => child.Contains(x.Code) && x.Category == "Branch").Select(x => x.Code).FirstOrDefault();
                    if (String.IsNullOrEmpty(Rout))
                    {
                        ParentRoot.Add(mbranchcode);
                    }
                    else
                    {
                        ParentRoot.Add(Rout);
                    }
                    Route.Add(tfatBranch.Code);
                }
            }
            else
            {
                Route.Add(tfatBranch.Code);
                ParentRoot.Add(tfatBranch.Code);
            }
            #endregion

            #endregion

            List<FMROUTETable> fM_ROUTE_s = new List<FMROUTETable>();
            List<bool> AddFlagOfFmRoute = new List<bool>();
            List<int> SeqList = new List<int>();
            int Inde = 0;
            foreach (var item in Route)
            {
                SeqList.Add(Inde++);
            }
            int[] SeqArray = SeqList.ToArray();
            DateTime NewDate = new DateTime();
            int SubRoot = 0;
            List<string> AddRouteCode = new List<string>();
            List<string> AddParentRouteCode = new List<string>();
            bool AddFmroute = true;

            if (mModel.ScheduleRequired && String.IsNullOrEmpty(mModel.ArrivalDate) == false)
            {
                var Date = mModel.ArrivalDate.Split('/');
                var Date1 = mModel.ArrivalTime.Split(':');

                NewDate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Date1[0]), Convert.ToInt32(Date1[1]), 00);
            }
            int KMMaintain = 0;
            for (int i = 0; i < Route.Count(); i++)
            {
                AddFmroute = true;
                string Value = (i).ToString();
                var Index = Array.IndexOf(SeqArray, i);
                if (i == 0)
                {
                    SubRoot = 0;
                    var VihicleActivity = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.From).Select(x => x.VehicleWaitTime).FirstOrDefault();

                    FMROUTETable fM_ROUTE = new FMROUTETable();
                    fM_ROUTE.FmNo = Convert.ToInt32(fMMaster.FmNo);
                    if (fMMaster.ScheduleFollowup)
                    {
                        fM_ROUTE.Parent = ParentRoot[Index];

                    }
                    else
                    {
                        var TempVar = Route[Index];

                        var Bracnh = ctxTFAT.TfatBranch.Where(x => x.Code == TempVar).FirstOrDefault();
                        if (Bracnh.Category == "Area")
                        {
                            var ParentList = ctxTFAT.TfatBranch.Where(x => x.Grp == TempVar).Select(x => x.Code).ToList();
                            if (ctxTFAT.TfatBranch.Where(x => ParentList.Contains(x.Code) && x.Category == "Branch").FirstOrDefault() != null)
                            {
                                fM_ROUTE.Parent = ctxTFAT.TfatBranch.Where(x => ParentList.Contains(x.Code) && x.Category == "Branch").Select(x => x.Code).FirstOrDefault();
                            }
                            else
                            {
                                fM_ROUTE.Parent = ctxTFAT.TfatBranch.Where(x => ParentList.Contains(x.Code) && x.Category == "SubBranch").Select(x => x.Code).FirstOrDefault();
                            }
                        }


                        if (String.IsNullOrEmpty(fM_ROUTE.Parent))
                        {
                            fM_ROUTE.Parent = ParentRoot[Index];
                        }

                    }
                    fM_ROUTE.RouteVia = Route[Index];
                    fM_ROUTE.SequenceRoute = Convert.ToInt32(SeqArray[Index].ToString());
                    fM_ROUTE.SubRoute = SeqArray[Index];
                    fM_ROUTE.RouteType = "R";
                    fM_ROUTE.VehicleActivity = VihicleActivity;
                    fM_ROUTE.Prefix = mperiod;
                    fM_ROUTE.Parentkey = mbranchcode + "FM000" + mperiod.Substring(0, 2) + 1.ToString("D3") + fMMaster.FmNo.ToString();
                    int Hours = 0, minutes = 0;
                    if (!String.IsNullOrEmpty(VihicleActivity))
                    {
                        var GetTime = VihicleActivity.Split(':');
                        Hours = Convert.ToInt32(GetTime[0]);
                        if (GetTime.Length == 2)
                        {
                            minutes = Convert.ToInt32(GetTime[1]);
                        }
                    }

                    if (mModel.ScheduleRequired && String.IsNullOrEmpty(mModel.ArrivalDate) == false)
                    {
                        NewDate = NewDate.AddHours(Convert.ToInt32(Hours)).AddMinutes(minutes);
                        var DDDFDF = NewDate.ToShortTimeString();

                        fM_ROUTE.DispatchSchDate = ConvertDDMMYYTOYYMMDD(NewDate.ToShortDateString());
                        fM_ROUTE.DispatchReSchDate = ConvertDDMMYYTOYYMMDD(NewDate.ToShortDateString());
                        fM_ROUTE.DispatchSchTime = NewDate.ToString("HH:mm");
                        fM_ROUTE.DispatchReSchTime = NewDate.ToString("HH:mm");
                        fM_ROUTE.ArrivalDate = ConvertDDMMYYTOYYMMDD(mModel.ArrivalDate);
                        fM_ROUTE.ArrivalTime = mModel.ArrivalTime;
                        fM_ROUTE.ArrivalKM = Convert.ToInt32(mModel.ArrivalKM);


                        fM_ROUTE.ArrivalRemark = mModel.ArrivalRemark;
                        KMMaintain = Convert.ToInt32(mModel.DispatchKM);
                    }
                    else
                    {
                        fM_ROUTE.DispatchSchDate = null;
                        fM_ROUTE.DispatchSchTime = null;
                        fM_ROUTE.DispatchReSchDate = null;
                        fM_ROUTE.DispatchReSchTime = null;
                        fM_ROUTE.ArrivalDate = null;
                        fM_ROUTE.ArrivalTime = null;
                        fM_ROUTE.ArrivalKM = null;
                        fM_ROUTE.ArrivalRemark = null;
                        if (!String.IsNullOrEmpty(mModel.DispatchDate))
                        {
                            fM_ROUTE.DispatchDate = ConvertDDMMYYTOYYMMDD(mModel.DispatchDate);
                            fM_ROUTE.DispatchTime = mModel.DispatchTime;
                            fM_ROUTE.DispatchKM = Convert.ToInt32(mModel.DispatchKM);
                            KMMaintain = Convert.ToInt32(mModel.DispatchKM);//Update On 23.8.22
                        }

                        fM_ROUTE.DispatchRemark = null;
                    }
                    fM_ROUTE.VehicleNo = mModel.VehicleNo;
                    fM_ROUTE.RouteClear = false;

                    fM_ROUTE.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    fM_ROUTE.ENTEREDBY = muserid;
                    fM_ROUTE.AUTHORISE = fMMaster.AUTHORISE;
                    fM_ROUTE.AUTHIDS = muserid;
                    fM_ROUTE_s.Add(fM_ROUTE);
                    AddFlagOfFmRoute.Add(true);
                    AddRouteCode.Add(fM_ROUTE.RouteVia);
                    AddParentRouteCode.Add(fM_ROUTE.Parent);
                }
                else
                {
                    if (i == 1 && String.IsNullOrEmpty(mModel.DispatchDate) == false)
                    {
                        var Date = mModel.DispatchDate.Split('/');
                        var Date1 = mModel.DispatchTime.Split(':');

                        NewDate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Date1[0]), Convert.ToInt32(Date1[1]), 00);
                    }



                    var DestCode = Route[Index];
                    var VihicleActivity = ctxTFAT.TfatBranch.Where(x => x.Code == DestCode).Select(x => x.VehicleWaitTime).FirstOrDefault();
                    var dd = Route[Index - 1];

                    var kilometerMasters = ctxTFAT.KilometerMasterRef.Where(x => x.FromBranch == dd && x.ToBranch == DestCode).FirstOrDefault();
                    var FetLocalKM = false;
                    var StartPoint = ctxTFAT.TfatBranch.Where(x => x.Code == dd).FirstOrDefault();
                    var EndPoint = ctxTFAT.TfatBranch.Where(x => x.Code == DestCode).FirstOrDefault();
                    if (kilometerMasters == null)
                    {
                        FetLocalKM = true;
                        kilometerMasters = ctxTFAT.KilometerMasterRef.Where(x => x.FromBranch == dd && x.ToBranch == EndPoint.Grp).FirstOrDefault();
                        if (kilometerMasters == null)
                        {
                            kilometerMasters = ctxTFAT.KilometerMasterRef.Where(x => x.FromBranch == StartPoint.Grp && x.ToBranch == DestCode).FirstOrDefault();
                            if (kilometerMasters == null)
                            {
                                kilometerMasters = ctxTFAT.KilometerMasterRef.Where(x => x.FromBranch == StartPoint.Grp && x.ToBranch == EndPoint.Grp).FirstOrDefault();
                            }
                        }
                    }
                    LocalKMandTimeMaRef localKMand = new LocalKMandTimeMaRef();
                    if (FetLocalKM == true && kilometerMasters != null)
                    {
                        localKMand = ctxTFAT.LocalKMandTimeMaRef.Where(x => x.FromBranch == kilometerMasters.FromBranch && x.ToBranch == kilometerMasters.ToBranch).FirstOrDefault();
                    }
                    VehicleRates allVehicle = ctxTFAT.VehicleRates.Where(x => x.VehicleNo == fMMaster.TruckNo).FirstOrDefault();
                    string[] Hours_And_Time = new string[2];
                    string MainHr = "0", LocalHr = "0", MainMinute = "0", LocalMinute = "0";
                    int MainKM = 0, LocalKM = 0;
                    if (kilometerMasters != null)
                    {
                        if (!String.IsNullOrEmpty(kilometerMasters.ReportTime))
                        {
                            if (kilometerMasters.ReportTime.Contains(":"))
                            {
                                var GetTime = kilometerMasters.ReportTime.Split(':');
                                MainHr = GetTime[0];
                                MainMinute = GetTime[1];
                            }

                        }

                        if (!String.IsNullOrEmpty(kilometerMasters.KM))
                        {
                            if (kilometerMasters.KM != "NA")
                            {
                                MainKM = Convert.ToInt32(kilometerMasters.KM);

                            }
                        }
                    }

                    if (localKMand != null)
                    {
                        if (!String.IsNullOrEmpty(localKMand.ReportTime))
                        {
                            if (localKMand.ReportTime != "NA")
                            {
                                var GetTime = localKMand.ReportTime.Split(':');
                                LocalHr = GetTime[0];
                                LocalMinute = GetTime[1];
                            }
                        }

                        if (!String.IsNullOrEmpty(localKMand.KM) && localKMand.KM != "NA")
                        {
                            LocalKM = Convert.ToInt32(localKMand.KM);
                        }
                    }

                    Hours_And_Time[0] = (Convert.ToInt32(MainHr) + Convert.ToInt32(LocalHr)).ToString();
                    Hours_And_Time[1] = (Convert.ToInt32(MainMinute) + Convert.ToInt32(LocalMinute)).ToString();
                    int TotalKM = MainKM + LocalKM;


                    FMROUTETable FMROUTETable = new FMROUTETable();
                    FMROUTETable.VehicleActivity = VihicleActivity;
                    FMROUTETable.Kilometers = TotalKM;
                    FMROUTETable.KilometersTime = Hours_And_Time[0] + "." + Hours_And_Time[1];

                    if (mModel.ScheduleRequired)
                    {
                        var minute = Convert.ToInt32(Hours_And_Time[1]);

                        NewDate = NewDate.AddHours(Convert.ToInt32(Hours_And_Time[0])).AddMinutes(Convert.ToInt32(minute));
                        FMROUTETable.ArrivalSchDate = ConvertDDMMYYTOYYMMDD(NewDate.ToShortDateString());
                        FMROUTETable.ArrivalSchTime = NewDate.ToString("HH:mm");
                        FMROUTETable.ArrivalSchKm = (Convert.ToInt32(KMMaintain) + TotalKM);
                        FMROUTETable.ArrivalReSchDate = ConvertDDMMYYTOYYMMDD(NewDate.ToShortDateString());
                        FMROUTETable.ArrivalReSchTime = NewDate.ToString("HH:mm");
                        FMROUTETable.ArrivalReSchKm = (Convert.ToInt32(KMMaintain) + TotalKM);

                        int Hours = 0, minutes = 0;
                        if (!String.IsNullOrEmpty(VihicleActivity))
                        {
                            var GetTime = VihicleActivity.Split(':');
                            Hours = Convert.ToInt32(GetTime[0]);
                            if (GetTime.Length == 2)
                            {
                                minutes = Convert.ToInt32(GetTime[1]);
                            }
                        }
                        NewDate = NewDate.AddHours(Hours).AddMinutes(minutes);
                        FMROUTETable.DispatchSchDate = ConvertDDMMYYTOYYMMDD(NewDate.ToShortDateString());
                        FMROUTETable.DispatchSchTime = NewDate.ToString("HH:mm");
                        FMROUTETable.DispatchReSchDate = ConvertDDMMYYTOYYMMDD(NewDate.ToShortDateString());
                        FMROUTETable.DispatchReSchTime = NewDate.ToString("HH:mm");
                        KMMaintain = KMMaintain + TotalKM;
                    }
                    else
                    {
                        FMROUTETable.ArrivalSchDate = null;
                        FMROUTETable.ArrivalSchTime = null;
                        FMROUTETable.ArrivalSchKm = null;
                        FMROUTETable.ArrivalReSchDate = null;
                        FMROUTETable.ArrivalReSchTime = null;
                        FMROUTETable.ArrivalReSchKm = null;
                        FMROUTETable.DispatchSchDate = null;
                        FMROUTETable.DispatchSchTime = null;
                        FMROUTETable.DispatchReSchDate = null;
                        FMROUTETable.DispatchReSchTime = null;
                    }


                    if (fMMaster.ScheduleFollowup)
                    {
                        FMROUTETable.Parent = ParentRoot[Index];
                        if (ParentRoot[Index - 1].ToLower().Trim() == ParentRoot[Index].ToLower().Trim())
                        {
                            //var SeqIndex= Array.IndexOf(ParentRoot.ToArray(), ParentRoot[Index]);
                            FMROUTETable.SubRoute = SubRoot;
                            FMROUTETable.RouteType = "";
                            FMROUTETable.RouteVia = DestCode;
                        }
                        else
                        {
                            FMROUTETable.SubRoute = ++SubRoot;
                            FMROUTETable.RouteType = "R";
                            FMROUTETable.SequenceRoute = SubRoot;
                            FMROUTETable.RouteVia = DestCode;
                        }
                    }
                    else
                    {
                        var ParentList = ctxTFAT.TfatBranch.Where(x => x.Grp == DestCode).Select(x => x.Code).ToList();
                        if (DestCode == "HO0000")
                        {
                            ParentList.Clear();
                            ParentList.Add("HO0000");
                        }


                        if (ParentList.Any(AddParentRouteCode.Contains) || AddParentRouteCode.Contains(ParentRoot[Index]))
                        {
                            if (AddRouteCode.Contains(DestCode))
                            {
                                AddFmroute = false;
                            }
                            FMROUTETable.RouteVia = DestCode;

                            FMROUTETable.RouteType = "";
                            if (DestCode == "HO0000")
                            {
                                FMROUTETable.Parent = DestCode;
                                var NewVal = fM_ROUTE_s.Where(x => x.Parent == FMROUTETable.Parent).Select(x => x.SubRoute).FirstOrDefault();
                                FMROUTETable.SubRoute = NewVal;
                            }
                            else if (ctxTFAT.TfatBranch.Where(x => ParentList.Contains(x.Code) && x.Category == "Branch").FirstOrDefault() != null)
                            {
                                FMROUTETable.Parent = ctxTFAT.TfatBranch.Where(x => ParentList.Contains(x.Code) && x.Category == "Branch").Select(x => x.Code).FirstOrDefault();
                                var NewVal = fM_ROUTE_s.Where(x => x.Parent == FMROUTETable.Parent).Select(x => x.SubRoute).FirstOrDefault();
                                FMROUTETable.SubRoute = NewVal;
                            }
                            else
                            {
                                FMROUTETable.Parent = ctxTFAT.TfatBranch.Where(x => ParentList.Contains(x.Code) && x.Category == "SubBranch").Select(x => x.Code).FirstOrDefault();
                                var NewVal = fM_ROUTE_s.Where(x => x.Parent == FMROUTETable.Parent).Select(x => x.SubRoute).FirstOrDefault();
                                FMROUTETable.SubRoute = NewVal;
                            }
                            if (String.IsNullOrEmpty(FMROUTETable.Parent))
                            {
                                FMROUTETable.Parent = ParentRoot[Index];
                                var NewVal = fM_ROUTE_s.Where(x => x.Parent == FMROUTETable.Parent).Select(x => x.SubRoute).FirstOrDefault();
                                FMROUTETable.SubRoute = NewVal;
                            }
                        }
                        else
                        {
                            FMROUTETable.Parent = ParentRoot[Index];
                            FMROUTETable.SubRoute = ++SubRoot;
                            FMROUTETable.RouteType = "R";
                            FMROUTETable.SequenceRoute = SubRoot;
                            FMROUTETable.RouteVia = DestCode;
                        }
                    }


                    FMROUTETable.FmNo = Convert.ToInt32(fMMaster.FmNo);
                    FMROUTETable.DispatchDate = null;
                    FMROUTETable.DispatchTime = null;
                    FMROUTETable.DispatchKM = null;
                    FMROUTETable.DispatchRemark = null;
                    FMROUTETable.ArrivalDate = null;
                    FMROUTETable.ArrivalTime = null;
                    FMROUTETable.ArrivalKM = null;
                    FMROUTETable.ArrivalRemark = null;
                    FMROUTETable.VehicleNo = mModel.VehicleNo;
                    FMROUTETable.RouteClear = false;
                    FMROUTETable.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    FMROUTETable.ENTEREDBY = muserid;
                    FMROUTETable.AUTHORISE = fMMaster.AUTHORISE;
                    FMROUTETable.AUTHIDS = muserid;
                    FMROUTETable.Prefix = mperiod;
                    FMROUTETable.Parentkey = mbranchcode + "FM000" + mperiod.Substring(0, 2) + 1.ToString("D3") + fMMaster.FmNo.ToString();
                    fM_ROUTE_s.Add(FMROUTETable);
                    AddFlagOfFmRoute.Add(AddFmroute);
                    AddRouteCode.Add(DestCode);
                    AddParentRouteCode.Add(ParentRoot[Index]);
                }
            }



            fM_ROUTE_s = fM_ROUTE_s.OrderBy(x => x.SubRoute).ToList();
            for (int i = 0; i < fM_ROUTE_s.Count; i++)
            {
                //if (AddFlagOfFmRoute[i])
                {

                    ctxTFAT.FMROUTETable.Add(fM_ROUTE_s[i]);


                }
            }

            VehicleKmMaintainMa maintainMa = new VehicleKmMaintainMa();
            //maintainMa=ctxTFAT.VehicleKmMaintainMa.Where()
            maintainMa.ComingFrom = "FM Through";
            maintainMa.Date = fMMaster.Date;
            maintainMa.EntryDate = fMMaster.CreateDate;
            maintainMa.KM = Convert.ToInt32(fMMaster.KM);
            maintainMa.Time = fMMaster.Time;
            maintainMa.VehicleNo = fMMaster.TruckNo;
            maintainMa.FMno = fMMaster.FmNo;
            maintainMa.ENTEREDBY = muserid;
            maintainMa.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
            maintainMa.AUTHORISE = "A00";
            maintainMa.AUTHIDS = muserid;
            maintainMa.FMRefTablekey = mbranchcode + "FM000" + mperiod.Substring(0, 2) + 1.ToString("D3") + fMMaster.FmNo.ToString();

            ctxTFAT.VehicleKmMaintainMa.Add(maintainMa);

            VehicleMaster vehicle = ctxTFAT.VehicleMaster.Where(X => X.Code.ToUpper().Trim() == fMMaster.TruckNo.ToUpper().Trim()).FirstOrDefault();
            if (vehicle == null)
            {
                HireVehicleMaster hireVehicle = ctxTFAT.HireVehicleMaster.Where(X => X.Code.ToUpper().Trim() == fMMaster.TruckNo.ToUpper().Trim()).FirstOrDefault();
                hireVehicle.KM = Convert.ToInt32(fMMaster.KM); ;
                ctxTFAT.Entry(hireVehicle).State = EntityState.Modified;
            }
            else
            {
                vehicle.KM = Convert.ToInt32(fMMaster.KM); ;
                ctxTFAT.Entry(vehicle).State = EntityState.Modified;

            }

            if (mModel.ScheduleRequired)
            {
                var GetFirstFmroute = fM_ROUTE_s.Where(x => x.SequenceRoute == 0 && x.SubRoute == 0).FirstOrDefault();

                if (String.IsNullOrEmpty(mModel.ArrivalDate) == false)
                {
                    VehicleKmMaintainMa maintain = new VehicleKmMaintainMa();
                    maintain.ComingFrom = "Arrival";
                    maintain.Date = ConvertDDMMYYTOYYMMDD(GetFirstFmroute.ArrivalDate.ToString());
                    maintain.EntryDate = fMMaster.CreateDate;
                    maintain.KM = Convert.ToInt32(GetFirstFmroute.ArrivalKM);
                    maintain.Time = GetFirstFmroute.ArrivalTime;
                    maintain.VehicleNo = GetFirstFmroute.VehicleNo;
                    maintain.FMno = GetFirstFmroute.FmNo;
                    maintain.Parent = GetFirstFmroute.Parent;
                    maintain.RouteVia = GetFirstFmroute.RouteVia;
                    maintain.ENTEREDBY = muserid;
                    maintain.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    maintain.AUTHORISE = "A00";
                    maintain.AUTHIDS = muserid;
                    maintain.FMRefTablekey = mbranchcode + "FM000" + mperiod.Substring(0, 2) + 1.ToString("D3") + fMMaster.FmNo.ToString();
                    ctxTFAT.VehicleKmMaintainMa.Add(maintain);
                }

                if (String.IsNullOrEmpty(mModel.DispatchDate) == false)
                {
                    VehicleKmMaintainMa maintain = new VehicleKmMaintainMa();
                    maintain.ComingFrom = "Dispatch";
                    maintain.Date = ConvertDDMMYYTOYYMMDD(GetFirstFmroute.DispatchDate.ToString());
                    maintain.EntryDate = fMMaster.CreateDate;
                    maintain.KM = Convert.ToInt32(GetFirstFmroute.DispatchKM);
                    maintain.Time = GetFirstFmroute.DispatchTime;
                    maintain.VehicleNo = GetFirstFmroute.VehicleNo;
                    maintain.FMno = GetFirstFmroute.FmNo;
                    maintain.Parent = GetFirstFmroute.Parent;
                    maintain.RouteVia = GetFirstFmroute.RouteVia;
                    maintain.ENTEREDBY = muserid;
                    maintain.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    maintain.AUTHORISE = "A00";
                    maintain.AUTHIDS = muserid;
                    maintain.FMRefTablekey = mbranchcode + "FM000" + mperiod.Substring(0, 2) + 1.ToString("D3") + fMMaster.FmNo.ToString();
                    ctxTFAT.VehicleKmMaintainMa.Add(maintain);
                }

                vehicle = ctxTFAT.VehicleMaster.Where(X => X.Code.ToUpper().Trim() == fMMaster.TruckNo.ToUpper().Trim()).FirstOrDefault();
                if (vehicle == null)
                {
                    HireVehicleMaster hireVehicle = ctxTFAT.HireVehicleMaster.Where(X => X.Code.ToUpper().Trim() == fMMaster.TruckNo.ToUpper().Trim()).FirstOrDefault();
                    hireVehicle.KM = Convert.ToInt32(KMMaintain); ;
                    ctxTFAT.Entry(hireVehicle).State = EntityState.Modified;

                }
                else
                {
                    vehicle.KM = Convert.ToInt32(KMMaintain); ;
                    ctxTFAT.Entry(vehicle).State = EntityState.Modified;

                }
            }

            string FinalRouteName = "", SameRoutName = "", RouteVia = "";

            foreach (var item in fM_ROUTE_s.Where(x => x.RouteType == "R"))
            {
                RouteVia += item.RouteVia + ",";
                FinalRouteName += ctxTFAT.TfatBranch.Where(x => x.Code == item.Parent).Select(x => x.Name).FirstOrDefault() + ",";
            }

            if (!String.IsNullOrEmpty(RouteVia))
            {
                //fMMaster.RouteViaName = FinalRouteName.Substring(0, FinalRouteName.Length - 1);
                fMMaster.RouteViaName = FinalRouteName;
                fMMaster.CurrBranch = ParentRoot[0];
                fMMaster.CurrRoute = 0;
                //ctxTFAT.Entry(fMMaster).State = EntityState.Modified;
            }
            else
            {
                fMMaster.RouteViaName = "";
            }

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

            var html = ViewHelper.RenderPartialView(this, "_Get_Destination_List", fMVM);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult ShowListBox()
        {
            FMVM fMVM = new FMVM();
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



            var html = ViewHelper.RenderPartialView(this, "PartialOfListBox", fMVM);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteDestination(string ViaRoute)
        {
            string Msg = "Sucess";
            List<LR_LC_Combine_VM> lR_LC_Combine_VMs = new List<LR_LC_Combine_VM>();
            List<LR_LC_Combine_VM> SessionDestList = TempData.Peek("Destination") as List<LR_LC_Combine_VM>;
            if (SessionDestList != null)
            {
                lR_LC_Combine_VMs = SessionDestList;
                var SingleLR_LC_Combine_VM = lR_LC_Combine_VMs.Where(x => x.From == ViaRoute).FirstOrDefault();
                if (SingleLR_LC_Combine_VM != null)
                {
                    lR_LC_Combine_VMs.Remove(SingleLR_LC_Combine_VM);
                }
            }
            TempData["Destination"] = lR_LC_Combine_VMs;
            FMVM fMVM = new FMVM();
            fMVM.AllDest = lR_LC_Combine_VMs;

            var html = ViewHelper.RenderPartialView(this, "_Get_Destination_List", fMVM);
            //var html = ViewHelper.RenderPartialView(this, "_Get_Destination_List", lR_LC_Combine_VMs);
            return Json(new { Msg = Msg, Html = html }, JsonRequestBehavior.AllowGet);

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
                    att.Type = String.IsNullOrEmpty(item.Type) == true ? "FM000" : item.Type;
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
            if (String.IsNullOrEmpty(Model.Branch))
            {
                Model.Branch = mbranchcode;
            }
            //var PDFName = Model.Document.Substring(20);
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

            //string mParentKey = Model.Document.Substring(6, Model.Document.Length - 6);
            string mParentKey = Model.Document;

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
            if (String.IsNullOrEmpty(Model.Branch))
            {
                Model.Branch = mbranchcode;
            }
            //var PDFName = Model.Document.Substring(20);
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
                    //mParentKey = Model.Document.Substring(6, Model.Document.Length - 6);
                    mParentKey = Model.Document;
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

        [HttpPost]
        public ActionResult AddToTable(FMVM Model)
        {
            string Status = "Success", Message = "";

            List<RouteDetails> objgriddetail = new List<RouteDetails>();
            if (Session["GridDataSession"] != null)
            {
                objgriddetail = (List<RouteDetails>)Session["GridDataSession"];
            }
            int I = objgriddetail.Count();
            var Count = objgriddetail.Count() - 2;
            var GRP = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Branch).FirstOrDefault();
            var CheckParentBranch = Model.Branch;
            if (GRP.Category == "Area")
            {
                CheckParentBranch = GRP.Grp;
            }
            if (CheckParentBranch == mbranchcode)
            {
                Status = "Error";
                Message = "Same Branch Not Allow To Add In Route List..!";
            }
            else
            {
                if (objgriddetail.Where(x => (x.Branch == Model.Branch || x.Branch == CheckParentBranch)).FirstOrDefault() == null)
                {
                    var Object = objgriddetail.Where(x => x.Tempid == I).FirstOrDefault();
                    objgriddetail.Add(new RouteDetails()
                    {
                        Branch = Model.Branch,
                        BranchN = Model.BranchN,
                        Tempid = I,
                        AllowToChange = "T"
                    });
                    Object.Tempid = I + 1;
                }
                else
                {
                    Status = "Error";
                    Message = "Same Branch Not Allow To Add In Route List..!";
                }
            }

            Model.Tempid = 0;
            Session.Add("GridDataSession", objgriddetail);
            Model.AllroutelistChange = objgriddetail;
            var html = ViewHelper.RenderPartialView(this, "ChangeRoueDetails", Model);
            return Json(new
            {
                GridDataVM = objgriddetail,
                Html = html,
                Mode = "Add",
                Status = Status,
                Message = Message
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddToTableEdit(FMVM Model)
        {

            var result = (List<RouteDetails>)Session["GridDataSession"];
            foreach (var item in result.Where(x => x.Tempid == Model.Tempid))
            {
                item.Branch = Model.Branch;
                item.BranchN = Model.BranchN;
            }

            Session.Add("GridDataSession", result);
            Model.AllroutelistChange = result.ToList();
            var html = ViewHelper.RenderPartialView(this, "ChangeRoueDetails", Model);
            return Json(new
            {
                GridDataVM = result,
                Html = html,
                Mode = "Add"
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PickFromTable(FMVM Model)
        {

            var result = (List<RouteDetails>)Session["GridDataSession"];
            var result1 = result.Where(x => x.Tempid == Model.Tempid);
            foreach (var item in result1)
            {

                Model.Tempid = item.Tempid;
                Model.RouteBranch = item.Branch;
                Model.BranchN = item.BranchN;
            }
            Model.AllroutelistChange = result.Where(x => x.Tempid != Model.Tempid).ToList();

            return Json(new
            {
                Html = this.RenderPartialView("ChangeRoueDetails", Model),
                AppBranch = Model.AppBranch
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteTableRow(int tEmpID, string FMNO)
        {
            FMVM Model = new FMVM();

            var result = (List<RouteDetails>)Session["GridDataSession"];
            var DeleteRoute = result.Where(x => x.Tempid == tEmpID).FirstOrDefault();
            result = result.Where(x => x.Tempid != tEmpID).ToList();
            if (!(String.IsNullOrEmpty(FMNO)))
            {
                if (DeleteRoute.SequenceRoute != 0)
                {
                    List<string> DeleteRouteCode = new List<string>();
                    var RouteTableList = ctxTFAT.FMROUTETable.Where(x => x.Parentkey.ToString() == FMNO && x.SubRoute == DeleteRoute.SequenceRoute).ToList();

                    if (RouteTableList.Count() > 0)
                    {
                        DeleteRouteCode = RouteTableList.Select(x => x.RouteVia).ToList();
                        ctxTFAT.FMROUTETable.RemoveRange(RouteTableList);
                        ctxTFAT.SaveChanges();

                        var Fmmaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == FMNO).FirstOrDefault();
                        if (!(String.IsNullOrEmpty(Fmmaster.RouteVia)))
                        {
                            string NewRouteDefine = "";
                            string NewRouteDefineName = "";
                            var SplitRote = Fmmaster.RouteVia.Split(',');
                            foreach (var item in SplitRote)
                            {
                                if (!(DeleteRouteCode.Contains(item)))
                                {
                                    TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == item).FirstOrDefault();
                                    if (tfatBranch != null)
                                    {
                                        NewRouteDefine += tfatBranch.Code + ",";
                                        NewRouteDefineName += tfatBranch.Name + ",";
                                    }
                                }
                            }
                            Fmmaster.RouteVia = NewRouteDefine;
                            Fmmaster.SelectedRoute = NewRouteDefineName;

                            int I = 0;
                            var GeneratedRout = "";
                            RouteTableList = ctxTFAT.FMROUTETable.Where(x => x.Parentkey.ToString() == FMNO).ToList();
                            foreach (var item in RouteTableList.OrderBy(x => x.SubRoute).ToList())
                            {
                                if (item.RouteType == "R")
                                {
                                    //if (Fmmaster.FromBranch!=item.RouteVia && Fmmaster.ToBranch!=item.RouteVia)
                                    {
                                        GeneratedRout += ctxTFAT.TfatBranch.Where(x => x.Code == item.Parent).Select(x => x.Name).FirstOrDefault() + ",";
                                    }

                                    item.SequenceRoute = I;
                                    item.SubRoute = I;
                                    ++I;
                                }
                                else
                                {
                                    item.SubRoute = I - 1;
                                }
                            }

                            Fmmaster.RouteViaName = GeneratedRout;
                            ctxTFAT.Entry(Fmmaster).State = EntityState.Modified;
                            ctxTFAT.SaveChanges();

                        }
                    }
                }
            }



            int i = 1;
            foreach (var item in result.OrderBy(x => x.Tempid).ToList())
            {
                item.Tempid = i++;
            }
            Model.Tempid = 0;
            Session.Add("GridDataSession", result);
            Model.AllroutelistChange = result;
            var html = ViewHelper.RenderPartialView(this, "ChangeRoueDetails", Model);
            return Json(new { SelectedIndent = result, Html = html }, JsonRequestBehavior.AllowGet);
        }

        #region Not Required Functions

        public ActionResult GetAllRemarkOfDocument(string Fmno)
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
                            FMMaster master = ctxTFAT.FMMaster.Where(x => x.FmNo.ToString() == Model.FMNO).FirstOrDefault();
                            var LastSno = ctxTFAT.Narration.Where(x => x.ParentKey == master.ParentKey).ToList().Count();
                            ++LastSno;

                            Narration narr = new Narration();
                            narr.Branch = mbranchcode;
                            narr.Narr = Model.NarrStr;
                            narr.NarrRich = Model.Header;
                            narr.Prefix = mperiod;
                            narr.Sno = LastSno;
                            narr.Srl = master.FmNo.ToString();
                            narr.Type = "FM000";
                            narr.ENTEREDBY = muserid;
                            narr.LASTUPDATEDATE = DateTime.Now;
                            narr.AUTHORISE = mauthorise;
                            narr.AUTHIDS = muserid;
                            narr.LocationCode = 0;
                            narr.TableKey = mbranchcode + "FM000" + mperiod.Substring(0, 2) + LastSno.ToString("D3") + master.FmNo.ToString();
                            narr.CompCode = mcompcode;
                            narr.ParentKey = master.ParentKey;
                            ctxTFAT.Narration.Add(narr);
                            ctxTFAT.SaveChanges();
                            transaction.Commit();
                            transaction.Dispose();

                            NewNarr.FMNO = master.FmNo.ToString();
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
                        Narration narration = ctxTFAT.Narration.Where(x => x.Sno.ToString() == mModel.NarrSno.ToString() && x.Type == "FM000").FirstOrDefault();
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