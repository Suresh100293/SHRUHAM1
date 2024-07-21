using Common;
using EntitiModel;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data;
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
using System.Web.Script.Serialization;
using System.Drawing;
using Font = System.Drawing.Font;
using System.Configuration;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Web;
using Microsoft.Reporting.WebForms;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class LorryReceiptController : BaseController
    {
        string attachmentPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentPath"].ToString()) == true ? "" : ConfigurationManager.AppSettings["AttachmentPath"].ToString();

        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;
        private new string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private new string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        public new string muserid = System.Web.HttpContext.Current.Session["UserId"].ToString();
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        private static string mAUTHORISE = "A00";
        //ITransactionGridOperation mIlst = new TransactionGridOperation();
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

        #region Function List

        public ActionResult GetTicklrOfDeclareValue(string DeclareValue)
        {

            string Show = "N", GST_TicklrString = "GST-Info : ", EWB_TicklrString = "Eway Bill-Info : ";
            var Setup = ctxTFAT.LRSetup.FirstOrDefault();
            if (Setup != null)
            {
                if (Setup.DeclareValue <= Convert.ToInt32(DeclareValue))
                {
                    if (!String.IsNullOrEmpty(Setup.GST_Ticklr))
                    {
                        Show = "Y";
                        GST_TicklrString += Setup.GST_Ticklr;
                    }
                    if (!String.IsNullOrEmpty(Setup.EWB_Ticklr))
                    {
                        Show = "Y";
                        EWB_TicklrString += Setup.EWB_Ticklr;
                    }

                }
            }

            return Json(new { Show = Show, GST_TicklrString = GST_TicklrString, EWB_TicklrString = EWB_TicklrString, JsonRequestBehavior.AllowGet });
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
            }).Take(10);
            return Json(Modified, JsonRequestBehavior.AllowGet);

        }

        public ActionResult CheckLRDate(string DocDate, string DocTime)
        {
            string message = "", Status = "T";
            var Date = DocDate.Split('/');
            string[] Time = DocTime.Split(':');
            DateTime Lrdate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Time[0]), Convert.ToInt32(Time[1]), 00);

            LRSetup lRSetup = ctxTFAT.LRSetup.FirstOrDefault();
            //if (lRSetup != null)
            //{
            //    if (lRSetup.LRDate == false)
            //    {
            //        var MinDate = DateTime.Now.AddHours(lRSetup.BeforeLRDate * (-1));
            //        var MaxDate = DateTime.Now.AddHours(lRSetup.AfterLRDate);

            //        if (MinDate <= Lrdate && Lrdate <= MaxDate)
            //        {
            //            Status = "T";
            //        }
            //        else
            //        {
            //            Status = "F";
            //            message = "LRDATE And TIME NOT ALLOW AS PER THE SETUP RULE...!";
            //        }
            //    }
            //    else
            //    {
            //        if (DateTime.Now.ToShortDateString()!= Lrdate.ToShortDateString())
            //        {
            //            Status = "F";
            //            message = "Consignment Booking Date Allow Only Todays Date AS PER THE SETUP RULE...!";
            //        }
            //    }
            //}
            if (Status == "T")
            {
                var NewDocDate = ConvertDDMMYYTOYYMMDD(Lrdate.ToShortDateString());
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
                var Datew = ConvertDDMMYYTOYYMMDD(DocDate);
                if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.Type == "LR000" && x.LockDate == Datew).FirstOrDefault() != null)
                {
                    Status = "F";
                    message = "LR Date is Locked By Period Lock System...!";
                }
            }

            return Json(new { Status = Status, Message = message, JsonRequestBehavior.AllowGet });
        }

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

        public string ConvertDDMMYYTOYYMMDD1(string da)
        {
            string abc = da.Substring(6, 4) + "/" + da.Substring(3, 2) + "/" + da.Substring(0, 2);
            return abc;
        }

        public JsonResult GetConsigner(string term)//Consigner
        {
            List<Consigner> consigners = new List<Consigner>();
            bool Branch = true;
            Branch = ctxTFAT.LRSetup.Select(x => x.ConsignerList).FirstOrDefault();
            if (Branch)
            {
                var Areas = GetChildGrp(mbranchcode);
                var list = ctxTFAT.Consigner.Where(x => x.Acitve == true).ToList();
                foreach (var item in list)
                {
                    var itemArea = item.Branch.Split(',').ToList();
                    if (itemArea.Any(x => Areas.Contains(x)))
                    {
                        consigners.Add(item);
                    }
                }
            }
            else
            {
                consigners = ctxTFAT.Consigner.Where(x => x.Acitve == true).ToList();
            }

            if (!(String.IsNullOrEmpty(term)))
            {
                consigners = consigners.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }

            var Modified = consigners.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            }).Take(10);
            return Json(Modified, JsonRequestBehavior.AllowGet);

            //return Json((ctxTFAT.KilometerMaster.Join(ctxTFAT.TreeTable, p => p.From, pc => pc.Code, (p, pc) => new { p, pc })
            //             .Where(x => x.pc.Name.ToLower().Contains(term.ToLower()) && (!(x.pc.Code.ToLower().Contains("h"))) &&
            //             (!(x.pc.Code.ToLower().Contains("z")))).Select(m => new
            //             {
            //                 Name = m.pc.Name, // or m.ppc.pc.ProdId
            //                 Code = m.pc.Code
            //             })).ToArray().Distinct(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetConsignee(string term)
        {
            bool Branch = true;
            Branch = ctxTFAT.LRSetup.Select(x => x.ConsignerList).FirstOrDefault();
            List<Consigner> consigners = new List<Consigner>();
            if (Branch)
            {
                var Areas = GetChildGrp(mbranchcode);

                var list = ctxTFAT.Consigner.Where(x => x.Acitve == true).ToList();
                foreach (var item in list)
                {
                    var itemArea = item.Branch.Split(',').ToList();
                    if (itemArea.Any(x => Areas.Contains(x)))
                    {
                        consigners.Add(item);
                    }
                }
            }
            else
            {
                consigners = ctxTFAT.Consigner.Where(x => x.Acitve == true).ToList();
            }


            if (!(String.IsNullOrEmpty(term)))
            {
                consigners = consigners.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }

            var Modified = consigners.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            }).Take(10);
            return Json(Modified, JsonRequestBehavior.AllowGet);

            //return Json((ctxTFAT.KilometerMaster.Join(ctxTFAT.TreeTable, p => p.From, pc => pc.Code, (p, pc) => new { p, pc })
            //             .Where(x => x.pc.Name.ToLower().Contains(term.ToLower()) && (!(x.pc.Code.ToLower().Contains("h"))) &&
            //             (!(x.pc.Code.ToLower().Contains("z")))).Select(m => new
            //             {
            //                 Name = m.pc.Name, // or m.ppc.pc.ProdId
            //                 Code = m.pc.Code
            //             })).ToArray().Distinct(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBillingParty(string term)//BillingParty
        {
            var list = ctxTFAT.CustomerMaster.Where(x => x.AcType == "D").ToList();

            //    var UserInRole = ctxTFAT.Master.
            //Join(ctxTFAT.MasterGroups, u => u.Grp, uir => uir.Code,
            //(u, uir) => new { u, uir })
            //.Where(m => m.u.BaseGr == "D" || m.u.BaseGr == "U").Select(x => x.u).ToList();
            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            }).Take(10);
            return Json(Modified, JsonRequestBehavior.AllowGet);

        }
        public JsonResult GetExpeAcc(string term)//BillingParty
        {
            var list = ctxTFAT.Master.Where(x => x.Hide == false).ToList();

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

            var FillFromCombo = ctxTFAT.LRSetup.Select(x => (bool?)x.FillFromCurr ?? false).FirstOrDefault();
            List<TfatBranch> list = new List<TfatBranch>();

            if (FillFromCombo)
            {
                ////HOD Zone Branch SubBranch
                list = ctxTFAT.TfatBranch.Where(x => (x.Status == true && x.Code != "G00000" && x.Grp != "G00000") && (x.Code == mbranchcode || x.Grp == mbranchcode)).ToList();

                var ZoneList = list.Where(x => x.Category == "Zone").Select(x => x.Code).ToList();
                list.AddRange(ctxTFAT.TfatBranch.Where(x => ZoneList.Contains(x.Code) || ZoneList.Contains(x.Grp)).ToList());

                var BranchList = list.Where(x => x.Category == "Branch").Select(x => x.Code).ToList();
                list.AddRange(ctxTFAT.TfatBranch.Where(x => BranchList.Contains(x.Code) || BranchList.Contains(x.Grp)).ToList());

                var SubBranchList = list.Where(x => x.Category == "SubBranch").Select(x => x.Code).ToList();
                list.AddRange(ctxTFAT.TfatBranch.Where(x => SubBranchList.Contains(x.Code) || SubBranchList.Contains(x.Grp)).ToList());

                //list = ctxTFAT.TfatBranch.Where(x => (x.Status == true && x.Code != "G00000") && (x.Code == mbranchcode || x.Grp == mbranchcode || x.Grp== "G00000")).ToList();



                //list = GetBranch(mbranchcode);

                var GeneralArea = ctxTFAT.TfatBranch.Where(x => x.Grp == "G00000" && x.Category == "Area" && x.Status == true).OrderBy(x => x.Name).ToList();
                list.AddRange(GeneralArea);
            }
            else
            {
                list = ctxTFAT.TfatBranch.Where(x => x.Status == true && x.Code != "G00000").ToList();
            }
            var Newlist = list.Distinct().ToList();
            if (!(String.IsNullOrEmpty(term)))
            {
                Newlist = Newlist.Where(x => x.Name.ToLower().Contains(term.ToLower())).OrderBy(x => x.Name).Take(10).ToList();
            }
            else
            {
                Newlist = list.Distinct().Take(10).ToList();
            }

            Newlist.Where(w => w.Category == "Zone").ToList().ForEach(w => w.Name = w.Name + " - Z");
            Newlist.Where(w => w.Category == "0").ToList().ForEach(w => w.Name = w.Name + " - HO");
            Newlist.Where(w => w.Category == "Branch").ToList().ForEach(w => w.Name = w.Name + " - B");
            Newlist.Where(w => w.Category == "SubBranch").ToList().ForEach(w => w.Name = w.Name + " - SB");
            Newlist.Where(w => w.Category == "Area").ToList().ForEach(w => w.Name = w.Name + " - A");


            var Modified = Newlist.Select(x => new
            {
                Code = x.Code,
                Name = x.Name + " [" + ctxTFAT.TfatBranch.Where(y => y.Code == x.Grp).Select(y => y.Name).FirstOrDefault() + "]"
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public JsonResult To(string term)
        {
            List<TfatBranch> list = ctxTFAT.TfatBranch.Where(x => x.Status == true && x.Code != "G00000").OrderBy(x => x.Name).ToList();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).OrderBy(x => x.Name).ToList();
            }
            list = list.Distinct().Take(10).ToList();
            list.Where(w => w.Category == "Zone").ToList().ForEach(w => w.Name = w.Name + " - Z");
            list.Where(w => w.Category == "0").ToList().ForEach(w => w.Name = w.Name + " - HO");
            list.Where(w => w.Category == "Branch").ToList().ForEach(w => w.Name = w.Name + " - B");
            list.Where(w => w.Category == "SubBranch").ToList().ForEach(w => w.Name = w.Name + " - SB");
            list.Where(w => w.Category == "Area").ToList().ForEach(w => w.Name = w.Name + " - A");

            var Modified = list.ToList().Select(x => new
            {
                Code = x.Code,
                Name = x.Name + " [" + ctxTFAT.TfatBranch.Where(y => y.Code == x.Grp).Select(y => y.Name).FirstOrDefault() + "]"
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BillatBranch(string term)
        {
            //List<TfatBranch> list = GetBranch(BranchCode);
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
                list = ctxTFAT.ChargeTypeMaster.Where(x => x.ChargeType.ToLower().Contains(term.ToLower())).ToList().Distinct();
            }

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.ChargeType
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);

        }

        public JsonResult LoadVehicleNo(string term)//VehicleNO
        {
            var list = ctxTFAT.VehicleMaster.Where(x => x.Acitve == true).Select(x => new { x.TruckNo, x.Code }).ToList();
            list.AddRange(ctxTFAT.HireVehicleMaster.Where(x => x.Acitve == true).Select(x => new { x.TruckNo, x.Code }).ToList());
            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.TruckNo.Contains(term.ToLower())).ToList();
            }
            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.TruckNo
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);

        }

        public ActionResult FetchDriver(string VehicleNo)
        {
            string DriverCode = "", DriverNCombo = "";
            var Vehicle = ctxTFAT.VehicleMaster.Where(x => x.Code == VehicleNo).FirstOrDefault();
            if (Vehicle != null)
            {
                var Driver = ctxTFAT.VehicleDri_Hist.Where(x => x.TruckNo == Vehicle.Code).OrderByDescending(x => new { x.FromPeriod, x.FromTime }).Select(x => x.Driver).FirstOrDefault();
                DriverMaster driverMaster = ctxTFAT.DriverMaster.Where(x => x.Code == Driver).FirstOrDefault();
                if (driverMaster != null)
                {
                    DriverNCombo = driverMaster.Name;
                    DriverCode = driverMaster.Code;
                }
            }

            return Json(new
            {
                DriverNCombo = DriverNCombo,
                DriverCode = DriverCode,
                JsonRequestBehavior.AllowGet
            });
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

        public JsonResult GetUnit(string term)//Unit
        {
            //Descr
            var list = ctxTFAT.UnitMaster.Where(x => x.Hide == false).ToList().Distinct();

            //var list = ctxTFAT.KilometerMaster.Join(ctxTFAT.TreeTable, p => p.From, pc => pc.Code, (p, pc) => new { p, pc }).Where(x =>  (!(x.pc.Code.ToLower().Contains("h"))) && (!(x.pc.Code.ToLower().Contains("z")))).Select(X=>X.pc).ToList().Distinct();
            if (!(String.IsNullOrEmpty(term)))
            {
                list = ctxTFAT.UnitMaster.Where(x => x.Name.ToLower().Contains(term.ToLower()) && x.Hide == false).ToList().Distinct();
            }

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);

        }

        public JsonResult CollArea(string term)
        {
            List<TfatBranch> list = GetBranch(mbranchcode);
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

        public JsonResult LoadDeliveryAtSearch(string term, string From)
        {
            List<TfatBranch> list = ctxTFAT.TfatBranch.Where(x => x.Code != From && x.Category != "0" && x.Category != "Area" && x.Category != "Zone").ToList();


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

        public ActionResult GetAddress(string Consigner, string Consignee, string BillingParty)
        {

            string Addr = "", Addr1 = "", Addr2 = "", RecGST = "", SendGST = "", BillGST = "";
            if (!(string.IsNullOrEmpty(Consigner)))
            {
                Addr = ctxTFAT.Consigner.Where(x => x.Code == Consigner).Select(x => x.Addr1).FirstOrDefault();
                Addr = Addr + " " + ctxTFAT.Consigner.Where(x => x.Code == Consigner).Select(x => x.Addr2).FirstOrDefault();

                RecGST = ctxTFAT.Consigner.Where(x => x.Code == Consigner).Select(x => x.GST).FirstOrDefault();
                if (String.IsNullOrEmpty(RecGST))
                {
                    RecGST = "";
                }
            }

            if (!(string.IsNullOrEmpty(Consignee)))
            {
                Addr1 = ctxTFAT.Consigner.Where(x => x.Code == Consignee).Select(x => x.Addr1).FirstOrDefault();
                Addr1 = Addr1 + " " + ctxTFAT.Consigner.Where(x => x.Code == Consignee).Select(x => x.Addr2).FirstOrDefault();
                SendGST = ctxTFAT.Consigner.Where(x => x.Code == Consignee).Select(x => x.GST).FirstOrDefault();
                if (String.IsNullOrEmpty(SendGST))
                {
                    SendGST = "";
                }
            }
            if (!(string.IsNullOrEmpty(BillingParty)))
            {
                Caddress caddress = ctxTFAT.Caddress.Where(x => x.Code == BillingParty).FirstOrDefault();
                if (caddress != null)
                {
                    if (String.IsNullOrEmpty(caddress.Adrl1))
                    {
                        Addr2 += caddress.Adrl1;
                    }
                    if (String.IsNullOrEmpty(caddress.Adrl2))
                    {
                        Addr2 += caddress.Adrl2;
                    }
                    if (String.IsNullOrEmpty(caddress.Adrl3))
                    {
                        Addr2 += caddress.Adrl3;
                    }
                    if (String.IsNullOrEmpty(caddress.Adrl4))
                    {
                        Addr2 += caddress.Adrl4;
                    }
                    if (String.IsNullOrEmpty(caddress.GSTNo))
                    {
                        BillGST = caddress.GSTNo;
                    }
                }
            }

            return Json(new { RecGST = RecGST, SendGST = SendGST, BillGST = BillGST, Message = Addr, Message1 = Addr1, Message2 = Addr2, JsonRequestBehavior.AllowGet });
        }

        public ActionResult GetCustomerDetails(string BillingParty)
        {
            bool POnumver = false, BEnumber = false, PartyChallan = false, PartyInvoice = false;
            string Coln = "", Deli = "", HitContract = "C", Addr = "", AddrSno = "";
            if (!(string.IsNullOrEmpty(BillingParty)))
            {
                var master = ctxTFAT.CustomerMaster.Where(x => x.Code == BillingParty).FirstOrDefault();
                if (master != null)
                {
                    POnumver = master.PONumber;
                    BEnumber = master.BENumber;
                    PartyChallan = master.PartyChallan;
                    PartyInvoice = master.PartyInvoice;
                    Coln = master.Collection;
                    Deli = master.Delivery;
                    HitContract = master.ContractHitby;


                    Caddress caddress = ctxTFAT.Caddress.OrderBy(x => x.Sno).Where(x => x.Code == BillingParty).FirstOrDefault();
                    if (caddress != null)
                    {
                        if (!String.IsNullOrEmpty(caddress.Adrl1))
                        {
                            if (String.IsNullOrEmpty(Addr))
                            {
                                Addr = caddress.Adrl1;
                            }
                            else
                            {
                                Addr += ",\n" + caddress.Adrl1;
                            }
                        }
                        if (!String.IsNullOrEmpty(caddress.Adrl2))
                        {
                            if (String.IsNullOrEmpty(Addr))
                            {
                                Addr = caddress.Adrl2;
                            }
                            else
                            {
                                Addr += ",\n" + caddress.Adrl2;
                            }
                        }
                        if (!String.IsNullOrEmpty(caddress.Adrl3))
                        {
                            if (String.IsNullOrEmpty(Addr))
                            {
                                Addr = caddress.Adrl3;
                            }
                            else
                            {
                                Addr += ",\n" + caddress.Adrl3;
                            }
                        }
                        if (!String.IsNullOrEmpty(caddress.Adrl4))
                        {
                            if (String.IsNullOrEmpty(Addr))
                            {
                                Addr = caddress.Adrl4;
                            }
                            else
                            {
                                Addr += ",\n" + caddress.Adrl4;
                            }
                        }
                        if (!String.IsNullOrEmpty(caddress.Country))
                        {
                            if (String.IsNullOrEmpty(Addr))
                            {
                                Addr = ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == caddress.Country).Select(x => x.Name).FirstOrDefault();
                            }
                            else
                            {
                                Addr += ",\n" + ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == caddress.Country).Select(x => x.Name).FirstOrDefault();
                            }
                        }
                        if (!String.IsNullOrEmpty(caddress.State))
                        {
                            if (String.IsNullOrEmpty(Addr))
                            {
                                Addr = ctxTFAT.TfatState.Where(x => x.Code.ToString() == caddress.State).Select(x => x.Name).FirstOrDefault();
                            }
                            else
                            {
                                Addr += ",\n" + ctxTFAT.TfatState.Where(x => x.Code.ToString() == caddress.State).Select(x => x.Name).FirstOrDefault();
                            }
                        }
                        if (!String.IsNullOrEmpty(caddress.City))
                        {
                            if (String.IsNullOrEmpty(Addr))
                            {
                                Addr = ctxTFAT.TfatCity.Where(x => x.Code.ToString() == caddress.City).Select(x => x.Name).FirstOrDefault();
                            }
                            else
                            {
                                Addr += ",\n" + ctxTFAT.TfatCity.Where(x => x.Code.ToString() == caddress.City).Select(x => x.Name).FirstOrDefault();
                            }
                        }
                        if (!String.IsNullOrEmpty(caddress.Email))
                        {
                            if (String.IsNullOrEmpty(Addr))
                            {
                                Addr = caddress.Email;
                            }
                            else
                            {
                                Addr += ",\n" + caddress.Email;
                            }
                        }
                        if (!String.IsNullOrEmpty(caddress.Mobile))
                        {
                            if (String.IsNullOrEmpty(Addr))
                            {
                                Addr = caddress.Mobile;
                            }
                            else
                            {
                                Addr += ",\n" + caddress.Mobile;
                            }
                        }
                        AddrSno = caddress.Sno.ToString();
                    }
                }

            }

            return Json(new { Addr = Addr, AddrSno = AddrSno, HitContract = HitContract, POnumver = POnumver, BEnumber = BEnumber, PartyChallan = PartyChallan, PartyInvoice = PartyInvoice, Coln = Coln, Deli = Deli, JsonRequestBehavior.AllowGet });
        }

        public ActionResult CheckExistLR(string TableName, string Colfield, string Value, string SkipColumnName, string PKValue, string ExtraColumn, string ExtraValue, string ExtraColumn2, string ExtraValue2)
        {
            int count = 0;
            string message = "";
            DataTable dt = new DataTable();
            SqlDataAdapter sda = new SqlDataAdapter();
            string connstring = GetConnectionString();
            SqlConnection con = new SqlConnection(connstring);
            if (String.IsNullOrEmpty(SkipColumnName))
            {
                SqlCommand cmd = new SqlCommand("Select * from " + TableName + " where " + Colfield + " = '" + Value + "' ", con);
                sda = new SqlDataAdapter("Select * from " + TableName + " where " + Colfield + " = '" + Value + "' ", con);
            }
            else
            {
                SqlCommand cmd = new SqlCommand("Select * from " + TableName + " where " + Colfield + " = '" + Value + "' and " + SkipColumnName + "!='" + PKValue + "'", con);
                sda = new SqlDataAdapter("Select * from " + TableName + " where " + Colfield + " = '" + Value + "' and " + SkipColumnName + "!='" + PKValue + "'", con);

            }

            sda.Fill(dt);
            con.Close();
            count = dt.Rows.Count;
            if (count == 0)
            {
                message = "T";
            }
            else
            {
                message = "F";
            }
            return Json(new { Message = message, JsonRequestBehavior.AllowGet });
        }

        public ActionResult LoadCustomeThroughLRType(string LRType)
        {
            var LrtypeName = ctxTFAT.LRTypeMaster.Where(x => x.Code == LRType).Select(x => x.LRType).FirstOrDefault();
            LRSetup LRSetup = ctxTFAT.LRSetup.FirstOrDefault();

            if (LrtypeName.Trim().ToLower() == "to pay")
            {
                Master master = ctxTFAT.Master.Where(x => x.Code == LRSetup.DefaultToPayCustomer).FirstOrDefault();
                if (master == null)
                {
                    return Json(new { Status = "Error", id = "", name = "", JsonRequestBehavior.AllowGet });
                }
                else
                {
                    return Json(new { Status = "Success", id = master.Code, name = master.Name, JsonRequestBehavior.AllowGet });
                }

            }
            else if (LrtypeName.Trim().ToLower() == "paid")
            {
                Master master = ctxTFAT.Master.Where(x => x.Code == LRSetup.DefaultPaidCustomer).FirstOrDefault();
                if (master == null)
                {
                    return Json(new { Status = "Error", id = "", name = "", JsonRequestBehavior.AllowGet });
                }
                else
                {
                    return Json(new { Status = "Success", id = master.Code, name = master.Name, JsonRequestBehavior.AllowGet });
                }
            }
            return Json(new { Status = "Error", id = "", name = "", JsonRequestBehavior.AllowGet });
        }

        public ActionResult LoadCustomeThroughConsignorOrConsignee(string LRType, string Consignor, string Consignee)
        {
            var lRTypeMaster = ctxTFAT.LRTypeMaster.Where(x => x.Code == LRType).FirstOrDefault();
            LRSetup LRSetup = ctxTFAT.LRSetup.Where(x => (x.RECORDKEY == 1)).FirstOrDefault();
            Consigner consigner = ctxTFAT.Consigner.Where(x => x.Code == Consignor).FirstOrDefault();
            Consigner consignee = ctxTFAT.Consigner.Where(x => x.Code == Consignee).FirstOrDefault();

            if (lRTypeMaster.LRType.Trim().ToLower() == "to pay")
            {
                if (LRSetup.ToPayCustomer == "Consignor" && (!String.IsNullOrEmpty(Consignor)))
                {
                    CustomerMaster master = ctxTFAT.CustomerMaster.Where(x => x.Code == consigner.Customer).FirstOrDefault();
                    return Json(new { Status = "Success", id = master.Code, name = master.Name, JsonRequestBehavior.AllowGet });
                }
                else if (LRSetup.ToPayCustomer == "Consignee" && (!String.IsNullOrEmpty(Consignee)))
                {
                    CustomerMaster master = ctxTFAT.CustomerMaster.Where(x => x.Code == consignee.Customer).FirstOrDefault();
                    return Json(new { Status = "Success", id = master.Code, name = master.Name, JsonRequestBehavior.AllowGet });
                }
                return Json(new { Status = "Error", id = "", name = "", JsonRequestBehavior.AllowGet });

            }
            else if (lRTypeMaster.LRType.Trim().ToLower() == "paid")
            {
                if (LRSetup.PaidCustomer == "Consignor" && (!String.IsNullOrEmpty(Consignor)))
                {
                    CustomerMaster master = ctxTFAT.CustomerMaster.Where(x => x.Code == consigner.Customer).FirstOrDefault();
                    return Json(new { Status = "Success", id = master.Code, name = master.Name, JsonRequestBehavior.AllowGet });
                }
                else if (LRSetup.PaidCustomer == "Consignee" && (!String.IsNullOrEmpty(Consignee)))
                {
                    CustomerMaster master = ctxTFAT.CustomerMaster.Where(x => x.Code == consignee.Customer).FirstOrDefault();
                    return Json(new { Status = "Success", id = master.Code, name = master.Name, JsonRequestBehavior.AllowGet });
                }
                return Json(new { Status = "Error", id = "", name = "", JsonRequestBehavior.AllowGet });

            }
            return Json(new { Status = "Error", id = "", name = "", JsonRequestBehavior.AllowGet });
        }

        public int GetNewCode()
        {
            var mPrevSrl = GetLastSerial("LRMaster", mbranchcode, "LR000", mperiod, "RS", DateTime.Now.Date);
            //var mPrevSrl = GetLastSerialUseSetup("LRMaster", mbranchcode, "LR000", mperiod, "RS", DateTime.Now.Date, " from LRSetup", "select CetralisedSrlReq,YearwiseSrlReq,10 as Width,isnull(Srl,0) as Srl  ");
            return Convert.ToInt32(mPrevSrl);


        }

        public string GetNewCodeDraft()
        {
            string Code = ctxTFAT.LRMasterDraft.OrderByDescending(x => x.RECORDKEY).Select(x => x.LrNo).Take(1).FirstOrDefault();
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

        public ActionResult CheckManualLR(int lrno, string document)
        {
            string Flag = "F";
            string Msg = "";
            bool checkalloctionFound = false;

            LRSetup lRSetup = ctxTFAT.LRSetup.FirstOrDefault();
            if (lRSetup.ManualLRCheck)
            {
                List<TblBranchAllocation> tblBranchAllocations = new List<TblBranchAllocation>();
                if (lRSetup.CetralisedManualSrlReq)
                {
                    tblBranchAllocations = ctxTFAT.TblBranchAllocation.Where(x => x.Branch == mbranchcode && x.TRN_Type == "LR000").ToList();
                }
                else
                {
                    tblBranchAllocations = ctxTFAT.TblBranchAllocation.Where(x => x.Prefix == mperiod && x.Branch == mbranchcode && x.TRN_Type == "LR000").ToList();
                }
                foreach (var item in tblBranchAllocations)
                {
                    if (item.ManualFrom <= lrno && lrno <= item.ManualTo)
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
                LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.LrNo == lrno && x.Prefix == mperiod && x.TableKey.ToString() != document).FirstOrDefault();
                if (lRMaster != null)
                {
                    Flag = "T";
                    Msg = "This LRNo Exist \nSo,Please Change Lr No....!";
                }
                else
                {
                    var result = ctxTFAT.DocTypes.Where(x => x.Code == "LR000").Select(x => x).FirstOrDefault();
                    if (lrno.ToString().Length > (result.DocWidth))
                    {
                        Flag = "T";
                        Msg = "Lr NO Allow " + result.DocWidth + " Digit Only....!";
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

        public ActionResult CheckAutoLR(int lrno, string document,string Mode)
        {
            string Flag = "F";
            string Msg = "";
            if (Mode=="Add")
            {
                LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.LrNo == lrno && x.Prefix == mperiod && x.TableKey.ToString() != document).FirstOrDefault();
                if (lRMaster != null)
                {
                    Flag = "T";
                    Msg = "Please Change Lr No....!";
                }
            }
            
            return Json(new { Message = Flag, Msg = Msg, JsonRequestBehavior.AllowGet });
        }

        public BroadCastMaster GetBroadCast()
        {
            BroadCastMaster broadCast = new BroadCastMaster();
            var Date = DateTime.Now.ToShortDateString().Split('/');
            var Date1 = DateTime.Now.ToString("HH:mm").Split(':');
            DateTime DocDate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Date1[0]), Convert.ToInt32(Date1[1]), 00);
            broadCast = ctxTFAT.BroadCastMaster.Where(x => x.StartDate <= DocDate && DocDate <= x.EndDate && x.Active == true).FirstOrDefault();
            if (broadCast == null)
            {
                broadCast = ctxTFAT.BroadCastMaster.Where(x => x.DocNo == "99999" && x.Active == true).FirstOrDefault();
            }
            if (broadCast == null)
            {
                broadCast = new BroadCastMaster();
                broadCast.Narr = "T.LAT ERP-3.0, Shruham Software";
                broadCast.Color = "White";
            }
            return broadCast;
        }

        public ActionResult GetTickler(string Consigner, bool ConsignorH)
        {
            string SpclRemark = "", BlackListRemark = "", AddressSno = "", AddressSnoText = "";
            bool HireSpcl = false, HireBlackList = false;

            Consigner consigner = ctxTFAT.Consigner.Where(x => x.Code == Consigner).FirstOrDefault();

            if (ConsignorH)
            {
                if (consigner != null)
                {
                    if (consigner.RemarkReq && consigner.TickLrConsignor)
                    {
                        if (!String.IsNullOrEmpty(consigner.Remark))
                        {
                            HireSpcl = true;
                            SpclRemark = consigner.Remark;
                        }
                    }
                    if (consigner.HoldReq && consigner.HoldTickLrConsignor)
                    {
                        if (!String.IsNullOrEmpty(consigner.HoldRemark))
                        {
                            HireBlackList = true;
                            BlackListRemark = consigner.HoldRemark;
                        }
                    }
                    var result = ctxTFAT.ConsignerAddress.OrderBy(x => x.Sno).Where(x => x.Code == Consigner).FirstOrDefault();
                    if (result != null)
                    {
                        AddressSno = result.Sno.ToString();
                        AddressSnoText = string.IsNullOrEmpty(result.Addr1) == false ? result.Addr1 + ",\n" + (string.IsNullOrEmpty(result.Addr2) == false ? result.Addr2 + ",\n" + (string.IsNullOrEmpty(result.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == result.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == result.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(result.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == result.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(result.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == result.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == result.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(result.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == result.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : ""))))
                                                                                : (string.IsNullOrEmpty(result.Addr2) == false ? result.Addr2 + ",\n" + (string.IsNullOrEmpty(result.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == result.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == result.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(result.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == result.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(result.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == result.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == result.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(result.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == result.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : ""))));
                    }
                }
            }
            else
            {
                if (consigner != null)
                {
                    if (consigner.RemarkReq && consigner.TickLrConsignee)
                    {
                        if (!String.IsNullOrEmpty(consigner.Remark))
                        {
                            HireSpcl = true;
                            SpclRemark = consigner.Remark;
                        }
                    }
                    if (consigner.HoldReq && consigner.HoldTickLrConsignee)
                    {
                        if (!String.IsNullOrEmpty(consigner.HoldRemark))
                        {
                            HireBlackList = true;
                            BlackListRemark = consigner.HoldRemark;
                        }
                    }
                    var result = ctxTFAT.ConsignerAddress.OrderBy(x => x.Sno).Where(x => x.Code == Consigner).FirstOrDefault();
                    if (result != null)
                    {
                        AddressSno = result.Sno.ToString();
                        AddressSnoText = string.IsNullOrEmpty(result.Addr1) == false ? result.Addr1 + ",\n" + (string.IsNullOrEmpty(result.Addr2) == false ? result.Addr2 + ",\n" + (string.IsNullOrEmpty(result.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == result.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == result.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(result.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == result.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(result.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == result.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == result.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(result.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == result.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : ""))))
                                                                                : (string.IsNullOrEmpty(result.Addr2) == false ? result.Addr2 + ",\n" + (string.IsNullOrEmpty(result.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == result.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == result.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(result.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == result.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(result.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == result.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == result.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(result.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == result.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(result.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == result.City).Select(y => y.Name).FirstOrDefault()) : ""))));
                    }
                }
            }

            return Json(new
            {
                HireSpcl = HireSpcl,
                HireSpclRemark = SpclRemark,
                HireBlackList = HireBlackList,
                HireBlackListRemark = BlackListRemark,
                AddressSno = AddressSno,
                AddressSnoText = AddressSnoText,
                JsonRequestBehavior.AllowGet
            });
        }

        public ActionResult GetDriverTickler(string Driver)
        {
            string SpclRemark = "", BlackListRemark = "";
            bool HireSpcl = false, HireBlackList = false;

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
            }
            return Json(new
            {
                HireSpcl = HireSpcl,
                HireSpclRemark = SpclRemark,
                HireBlackList = HireBlackList,
                HireBlackListRemark = BlackListRemark,
                JsonRequestBehavior.AllowGet
            });
        }

        public List<SelectListItem> PopulateChargeTypeList()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            //string constr = ConfigurationManager.ConnectionStrings["ALT_ERP21EntitiesConnectionString"].ConnectionString;
            items.Add(new SelectListItem
            {
                Text = "Per KG",
                Value = "1"
            });
            items.Add(new SelectListItem
            {
                Text = "Per Qty",
                Value = "3"
            });
            items.Add(new SelectListItem
            {
                Text = "Fixed",
                Value = "4"
            });

            return items;
        }

        public List<SelectListItem> PopulateChargeOnWeightList()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            //string constr = ConfigurationManager.ConnectionStrings["ALT_ERP21EntitiesConnectionString"].ConnectionString;
            items.Add(new SelectListItem
            {
                Text = "Charge On ActWt",
                Value = "A"
            });
            items.Add(new SelectListItem
            {
                Text = "Charge On ChrgWt",
                Value = "C"
            });
            return items;
        }

        public JsonResult GetConsignerAddressSrno(string term)
        {
            var result = ctxTFAT.ConsignerAddress.Where(x => x.Code == "S").ToList();
            var split = term.Split('^');
            if (!String.IsNullOrEmpty(split[1]))
            {
                var NewCode = split[1];
                result = ctxTFAT.ConsignerAddress.Where(x => x.Code == NewCode).ToList();
            }

            var Modified = result.Select(x => new
            {
                Code = x.Sno,
                Name = string.IsNullOrEmpty(x.Addr1) == false ? x.Addr1 + ",\n" + (string.IsNullOrEmpty(x.Addr2) == false ? x.Addr2 + ",\n" + (string.IsNullOrEmpty(x.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == x.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == x.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(x.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == x.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(x.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == x.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == x.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(x.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == x.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : ""))))
                                                                                : (string.IsNullOrEmpty(x.Addr2) == false ? x.Addr2 + ",\n" + (string.IsNullOrEmpty(x.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == x.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == x.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(x.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == x.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(x.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == x.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == x.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(x.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == x.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : ""))))
            }).ToList();
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetBillPartyAddressSrno(string term)
        {
            var result = ctxTFAT.Caddress.Where(x => x.Code == "S").ToList();
            var split = term.Split('^');
            if (!String.IsNullOrEmpty(split[1]))
            {
                var NewCode = split[1];
                result = ctxTFAT.Caddress.Where(x => x.Code == NewCode).ToList();
            }
            var Modified = result.Select(x => new
            {
                Code = x.Sno,
                Name = string.IsNullOrEmpty(x.Adrl1) == false ? x.Adrl1 + ",\n" + (string.IsNullOrEmpty(x.Adrl2) == false ? x.Adrl2 + ",\n" + (string.IsNullOrEmpty(x.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == x.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == x.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(x.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == x.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(x.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == x.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == x.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(x.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == x.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : ""))))
                                                                                : (string.IsNullOrEmpty(x.Adrl2) == false ? x.Adrl2 + ",\n" + (string.IsNullOrEmpty(x.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == x.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == x.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(x.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == x.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(x.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == x.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == x.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(x.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == x.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(x.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == x.City).Select(y => y.Name).FirstOrDefault()) : ""))))

            }).ToList();
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetConsignerAddress(string Consigner, string addressno)
        {
            string Addr = "", RecGST = "";
            if (!(string.IsNullOrEmpty(Consigner)))
            {
                var ConsignerAddress = ctxTFAT.ConsignerAddress.Where(x => x.Code == Consigner && x.Sno.ToString() == addressno).FirstOrDefault();
                if (ConsignerAddress != null)
                {
                    if (!String.IsNullOrEmpty(ConsignerAddress.Addr1))
                    {
                        if (String.IsNullOrEmpty(Addr))
                        {
                            Addr = ConsignerAddress.Addr1;
                        }
                        else
                        {
                            Addr += ",\n" + ConsignerAddress.Addr1;
                        }
                    }
                    if (!String.IsNullOrEmpty(ConsignerAddress.Addr2))
                    {
                        if (String.IsNullOrEmpty(Addr))
                        {
                            Addr = ConsignerAddress.Addr2;
                        }
                        else
                        {
                            Addr += ",\n" + ConsignerAddress.Addr2;
                        }
                    }
                    if (!String.IsNullOrEmpty(ConsignerAddress.Country))
                    {
                        if (String.IsNullOrEmpty(Addr))
                        {
                            Addr = ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == ConsignerAddress.Country).Select(x => x.Name).FirstOrDefault();
                        }
                        else
                        {
                            Addr += ",\n" + ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == ConsignerAddress.Country).Select(x => x.Name).FirstOrDefault();
                        }
                    }
                    if (!String.IsNullOrEmpty(ConsignerAddress.State))
                    {
                        if (String.IsNullOrEmpty(Addr))
                        {
                            Addr = ctxTFAT.TfatState.Where(x => x.Code.ToString() == ConsignerAddress.State).Select(x => x.Name).FirstOrDefault();
                        }
                        else
                        {
                            Addr += ",\n" + ctxTFAT.TfatState.Where(x => x.Code.ToString() == ConsignerAddress.State).Select(x => x.Name).FirstOrDefault();
                        }
                    }
                    if (!String.IsNullOrEmpty(ConsignerAddress.City))
                    {
                        if (String.IsNullOrEmpty(Addr))
                        {
                            Addr = ctxTFAT.TfatCity.Where(x => x.Code.ToString() == ConsignerAddress.City).Select(x => x.Name).FirstOrDefault();
                        }
                        else
                        {
                            Addr += ",\n" + ctxTFAT.TfatCity.Where(x => x.Code.ToString() == ConsignerAddress.City).Select(x => x.Name).FirstOrDefault();
                        }
                    }
                    if (!String.IsNullOrEmpty(ConsignerAddress.Email))
                    {
                        if (String.IsNullOrEmpty(Addr))
                        {
                            Addr = ConsignerAddress.Email;
                        }
                        else
                        {
                            Addr += ",\n" + ConsignerAddress.Email;
                        }
                    }
                    if (!String.IsNullOrEmpty(ConsignerAddress.Mobile))
                    {
                        if (String.IsNullOrEmpty(Addr))
                        {
                            Addr = ConsignerAddress.Mobile;
                        }
                        else
                        {
                            Addr += ",\n" + ConsignerAddress.Mobile;
                        }
                    }
                    if (!String.IsNullOrEmpty(ConsignerAddress.GSTNo))
                    {
                        RecGST = ConsignerAddress.GSTNo;
                    }
                }
            }

            return Json(new { RecGST = RecGST, Message = Addr, JsonRequestBehavior.AllowGet });
        }
        public ActionResult GetConsigneeAddress(string Consigner, string addressno)
        {
            string Addr = "", RecGST = "";
            if (!(string.IsNullOrEmpty(Consigner)))
            {
                var ConsignerAddress = ctxTFAT.ConsignerAddress.Where(x => x.Code == Consigner && x.Sno.ToString() == addressno).FirstOrDefault();
                if (ConsignerAddress != null)
                {
                    if (!String.IsNullOrEmpty(ConsignerAddress.Addr1))
                    {
                        if (String.IsNullOrEmpty(Addr))
                        {
                            Addr = ConsignerAddress.Addr1;
                        }
                        else
                        {
                            Addr += ",\n" + ConsignerAddress.Addr1;
                        }
                    }
                    if (!String.IsNullOrEmpty(ConsignerAddress.Addr2))
                    {
                        if (String.IsNullOrEmpty(Addr))
                        {
                            Addr = ConsignerAddress.Addr2;
                        }
                        else
                        {
                            Addr += ",\n" + ConsignerAddress.Addr2;
                        }
                    }
                    if (!String.IsNullOrEmpty(ConsignerAddress.Country))
                    {
                        if (String.IsNullOrEmpty(Addr))
                        {
                            Addr = ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == ConsignerAddress.Country).Select(x => x.Name).FirstOrDefault();
                        }
                        else
                        {
                            Addr += ",\n" + ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == ConsignerAddress.Country).Select(x => x.Name).FirstOrDefault();
                        }
                    }
                    if (!String.IsNullOrEmpty(ConsignerAddress.State))
                    {
                        if (String.IsNullOrEmpty(Addr))
                        {
                            Addr = ctxTFAT.TfatState.Where(x => x.Code.ToString() == ConsignerAddress.State).Select(x => x.Name).FirstOrDefault();
                        }
                        else
                        {
                            Addr += ",\n" + ctxTFAT.TfatState.Where(x => x.Code.ToString() == ConsignerAddress.State).Select(x => x.Name).FirstOrDefault();
                        }
                    }
                    if (!String.IsNullOrEmpty(ConsignerAddress.City))
                    {
                        if (String.IsNullOrEmpty(Addr))
                        {
                            Addr = ctxTFAT.TfatCity.Where(x => x.Code.ToString() == ConsignerAddress.City).Select(x => x.Name).FirstOrDefault();
                        }
                        else
                        {
                            Addr += ",\n" + ctxTFAT.TfatCity.Where(x => x.Code.ToString() == ConsignerAddress.City).Select(x => x.Name).FirstOrDefault();
                        }
                    }
                    if (!String.IsNullOrEmpty(ConsignerAddress.Email))
                    {
                        if (String.IsNullOrEmpty(Addr))
                        {
                            Addr = ConsignerAddress.Email;
                        }
                        else
                        {
                            Addr += ",\n" + ConsignerAddress.Email;
                        }
                    }
                    if (!String.IsNullOrEmpty(ConsignerAddress.Mobile))
                    {
                        if (String.IsNullOrEmpty(Addr))
                        {
                            Addr = ConsignerAddress.Mobile;
                        }
                        else
                        {
                            Addr += ",\n" + ConsignerAddress.Mobile;
                        }
                    }
                    if (!String.IsNullOrEmpty(ConsignerAddress.GSTNo))
                    {
                        RecGST = ConsignerAddress.GSTNo;
                    }
                }
            }

            return Json(new { RecGST = RecGST, Message = Addr, JsonRequestBehavior.AllowGet });
        }
        public ActionResult GetBillPartyAddress(string Consigner, string addressno)
        {
            string Addr = "", RecGST = "";

            if (!(string.IsNullOrEmpty(Consigner)))
            {
                Caddress caddress = ctxTFAT.Caddress.Where(x => x.Code == Consigner && x.Sno.ToString() == addressno).FirstOrDefault();
                if (caddress != null)
                {
                    if (!String.IsNullOrEmpty(caddress.Adrl1))
                    {
                        if (String.IsNullOrEmpty(Addr))
                        {
                            Addr = caddress.Adrl1;
                        }
                        else
                        {
                            Addr += ",\n" + caddress.Adrl1;
                        }
                    }
                    if (!String.IsNullOrEmpty(caddress.Adrl2))
                    {
                        if (String.IsNullOrEmpty(Addr))
                        {
                            Addr = caddress.Adrl2;
                        }
                        else
                        {
                            Addr += ",\n" + caddress.Adrl2;
                        }
                    }
                    if (!String.IsNullOrEmpty(caddress.Adrl3))
                    {
                        if (String.IsNullOrEmpty(Addr))
                        {
                            Addr = caddress.Adrl3;
                        }
                        else
                        {
                            Addr += ",\n" + caddress.Adrl3;
                        }
                    }
                    if (!String.IsNullOrEmpty(caddress.Adrl4))
                    {
                        if (String.IsNullOrEmpty(Addr))
                        {
                            Addr = caddress.Adrl4;
                        }
                        else
                        {
                            Addr += ",\n" + caddress.Adrl4;
                        }
                    }
                    if (!String.IsNullOrEmpty(caddress.Country))
                    {
                        if (String.IsNullOrEmpty(Addr))
                        {
                            Addr = ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == caddress.Country).Select(x => x.Name).FirstOrDefault();
                        }
                        else
                        {
                            Addr += ",\n" + ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == caddress.Country).Select(x => x.Name).FirstOrDefault();
                        }
                    }
                    if (!String.IsNullOrEmpty(caddress.State))
                    {
                        if (String.IsNullOrEmpty(Addr))
                        {
                            Addr = ctxTFAT.TfatState.Where(x => x.Code.ToString() == caddress.State).Select(x => x.Name).FirstOrDefault();
                        }
                        else
                        {
                            Addr += ",\n" + ctxTFAT.TfatState.Where(x => x.Code.ToString() == caddress.State).Select(x => x.Name).FirstOrDefault();
                        }
                    }
                    if (!String.IsNullOrEmpty(caddress.City))
                    {
                        if (String.IsNullOrEmpty(Addr))
                        {
                            Addr = ctxTFAT.TfatCity.Where(x => x.Code.ToString() == caddress.City).Select(x => x.Name).FirstOrDefault();
                        }
                        else
                        {
                            Addr += ",\n" + ctxTFAT.TfatCity.Where(x => x.Code.ToString() == caddress.City).Select(x => x.Name).FirstOrDefault();
                        }
                    }
                    if (!String.IsNullOrEmpty(caddress.Email))
                    {
                        if (String.IsNullOrEmpty(Addr))
                        {
                            Addr = caddress.Email;
                        }
                        else
                        {
                            Addr += ",\n" + caddress.Email;
                        }
                    }
                    if (!String.IsNullOrEmpty(caddress.Mobile))
                    {
                        if (String.IsNullOrEmpty(Addr))
                        {
                            Addr = caddress.Mobile;
                        }
                        else
                        {
                            Addr += ",\n" + caddress.Mobile;
                        }
                    }
                    if (!String.IsNullOrEmpty(caddress.GSTNo))
                    {
                        RecGST = caddress.GSTNo;
                    }
                }
            }

            return Json(new { RecGST = RecGST, Message = Addr, JsonRequestBehavior.AllowGet });
        }
        #endregion

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

        #region List && Index

        public ActionResult Index(LRVM mModel)
        {

            //ConsignmentNotification1(ctxTFAT.LRMaster.OrderBy(x => x.BookDate).FirstOrDefault());


            Session["CommnNarrlist"] = null;
            Session["TempAttach"] = null;
            TempData.Remove("LRAttachmentList");
            TempData.Remove("ConsignerList");

            GetAllMenu(Session["ModuleName"].ToString());
            
            if (mModel.getRecentLR == true || mModel.Mode == "Select" || mModel.Mode == "Pick" || mModel.Mode == "Add")
            {
                UpdateAuditTrail(mbranchcode, "Add", mModel.Header, null, DateTime.Now, 0, "", "", "NA");
            }
            else
            {
                var Document = ctxTFAT.LRMaster.Where(x => (x.TableKey.ToString() == mModel.Document)).FirstOrDefault();
                UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, Document.ParentKey, Document.BookDate, 0, Document.BillParty, "", "CA");
            }


            mdocument = mModel.Document;
            mModel.ChargeTypeList = PopulateChargeTypeList();
            mModel.ChargeOnWeightList = PopulateChargeOnWeightList();
            mModel.BroadCastMaster = GetBroadCast();
            mModel.RpoertViewData = GetSubCodeoflist(mModel.ViewDataId);

            // LrSetUp
            mModel.LRSetup = ctxTFAT.LRSetup.FirstOrDefault();
            if (mModel.LRSetup == null)
            {
                mModel.LRSetup = new LRSetup();
            }
            if (mModel.LRSetup.CurrDatetOnlyreq == false && mModel.LRSetup.BackDateAllow == false && mModel.LRSetup.ForwardDateAllow == false)
            {
                mModel.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
                mModel.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
            }
            if (mModel.LRSetup.CurrDatetOnlyreq == true)
            {
                mModel.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
                mModel.EndDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            if (mModel.LRSetup.BackDateAllow == true)
            {
                mModel.StartDate = (DateTime.Now.AddDays(-mModel.LRSetup.BackDaysUpto)).ToString("yyyy-MM-dd");
            }
            if (mModel.LRSetup.ForwardDateAllow == true)
            {
                mModel.EndDate = (DateTime.Now.AddDays(mModel.LRSetup.ForwardDaysUpto)).ToString("yyyy-MM-dd");
            }
            mModel.ExtraInfoTab = false;
            if (!String.IsNullOrEmpty(mModel.LRSetup.ExtraInfoTabAllowTo))
            {
                var List = mModel.LRSetup.ExtraInfoTabAllowTo.Split(',').ToList();
                if (List.Where(x => x.Trim().ToLower() == muserid.Trim().ToLower()).FirstOrDefault() != null)
                {
                    mModel.ExtraInfoTab = true;
                }
            }
            // Restriction Of Consignor And Decrription
            mModel.ConsignorRestrict = true;
            mModel.DescriptionRestrict = true;
            if (muserid.ToUpper() != "SUPER")
            {
                var Tfatmenu = ctxTFAT.TfatMenu.Where(x => x.FormatCode == "ConsignerOrConsignee").FirstOrDefault();
                var Restrictdata = ctxTFAT.UserRights.Where(x => x.MenuID == Tfatmenu.ID && x.Code == muserid).Select(x => x.xAdd).FirstOrDefault();
                mModel.ConsignorRestrict = Restrictdata;
                Tfatmenu = ctxTFAT.TfatMenu.Where(x => x.FormatCode == "DescriptionMaster").FirstOrDefault();
                Restrictdata = ctxTFAT.UserRights.Where(x => x.MenuID == Tfatmenu.ID && x.Code == muserid).Select(x => x.xAdd).FirstOrDefault();
                mModel.DescriptionRestrict = Restrictdata;
            }

            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete") || (mModel.getRecentLR == true))
            {
                var mlist = new LRMaster();
                if (mModel.getRecentLR)
                {
                    var Area = GetChildGrp(mbranchcode);
                    mlist = ctxTFAT.LRMaster.Where(x => x.AUTHIDS == muserid && Area.Contains(x.Branch)).OrderByDescending(x => x.RECORDKEY).FirstOrDefault();
                    if (mlist != null)
                    {
                        mModel.LrContactPerson = mlist.InfoName;
                        mModel.LrContactPersonNo = mlist.InfoContactNO;
                        mModel.LrContactPersonEmailId = mlist.InfoEmailId;
                        mModel.HitContractType = ctxTFAT.CustomerMaster.Where(x => x.Code == mlist.BillParty).Select(x => x.ContractHitby).FirstOrDefault();
                        mModel.ConsignerAddNo = String.IsNullOrEmpty(mlist.ConsignerAddNo) == true ? "" : mlist.ConsignerAddNo;
                        mModel.ConsigneeAddNo = String.IsNullOrEmpty(mlist.ConsigneeAddNo) == true ? "" : mlist.ConsigneeAddNo;
                        mModel.BillPartyAddNo = String.IsNullOrEmpty(mlist.BillPartyAddNo) == true ? "" : mlist.BillPartyAddNo;
                        if (!String.IsNullOrEmpty(mModel.ConsignerAddNo))
                        {
                            var ConsignerAddres = ctxTFAT.ConsignerAddress.Where(x => x.Code == mlist.RecCode && x.Sno.ToString() == mlist.ConsignerAddNo).FirstOrDefault();
                            if (ConsignerAddres != null)
                            {
                                mModel.ConsignerAddNoName = string.IsNullOrEmpty(ConsignerAddres.Addr1) == false ? ConsignerAddres.Addr1 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Addr2) == false ? ConsignerAddres.Addr2 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))))
                                                                                                                                                : (string.IsNullOrEmpty(ConsignerAddres.Addr2) == false ? ConsignerAddres.Addr2 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))));
                                if (!String.IsNullOrEmpty(mModel.ConsignerAddNoName))
                                {
                                    mModel.ConsignerAddNoName = mModel.ConsignerAddNoName.Replace("\n", " ");
                                }

                            }
                        }
                        if (!String.IsNullOrEmpty(mModel.ConsigneeAddNo))
                        {
                            var ConsignerAddres = ctxTFAT.ConsignerAddress.Where(x => x.Code == mlist.SendCode && x.Sno.ToString() == mlist.ConsigneeAddNo).FirstOrDefault();
                            if (ConsignerAddres != null)
                            {
                                mModel.ConsigneeAddNoName = string.IsNullOrEmpty(ConsignerAddres.Addr1) == false ? ConsignerAddres.Addr1 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Addr2) == false ? ConsignerAddres.Addr2 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))))
                                                                                                                                                : (string.IsNullOrEmpty(ConsignerAddres.Addr2) == false ? ConsignerAddres.Addr2 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))));
                                if (!String.IsNullOrEmpty(mModel.ConsigneeAddNoName))
                                {
                                    mModel.ConsigneeAddNoName = mModel.ConsigneeAddNoName.Replace("\n", " ");
                                }

                            }
                        }
                        if (!String.IsNullOrEmpty(mModel.BillPartyAddNo))
                        {
                            var ConsignerAddres = ctxTFAT.Caddress.Where(x => x.Code == mlist.BillParty && x.Sno.ToString() == mlist.BillPartyAddNo).FirstOrDefault();
                            if (ConsignerAddres != null)
                            {
                                mModel.BillPartyAddNoName = string.IsNullOrEmpty(ConsignerAddres.Adrl1) == false ? ConsignerAddres.Adrl1 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Adrl2) == false ? ConsignerAddres.Adrl2 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))))
                                                                                                                                                : (string.IsNullOrEmpty(ConsignerAddres.Adrl2) == false ? ConsignerAddres.Adrl2 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))));
                                if (!String.IsNullOrEmpty(mModel.BillPartyAddNoName))
                                {
                                    mModel.BillPartyAddNoName = mModel.BillPartyAddNoName.Replace("\n", " ");
                                }
                            }
                        }
                    }
                    List<LRVM> lrdetaillist = new List<LRVM>();
                    mModel.LRDetailList = lrdetaillist;

                }
                else
                {
                    mlist = ctxTFAT.LRMaster.Where(x => (x.TableKey.ToString() == mModel.Document)).FirstOrDefault();
                    if (ctxTFAT.LRBill.Where(x => x.LRRefTablekey == mlist.TableKey.ToString()).ToList().Count() > 0)
                    {
                        var Bill = ctxTFAT.LRBill.Where(x => x.LRRefTablekey == mlist.TableKey.ToString()).FirstOrDefault();
                        mModel.BillGenerate = true;
                        mModel.BillDetails += "\nThis Consignment Bill Generate On " + Bill.DocDate.Value.ToShortDateString() + " through Document No is : " + Bill.Srl;
                    }
                    if (ctxTFAT.LCDetail.Where(x => x.LRRefTablekey == mlist.TableKey).ToList().Count() > 0)
                    {
                        mModel.DispatchLR = true;
                        mModel.BillDetails += "\nThis Consignment Dispatchd....!";
                    }
                    if (ctxTFAT.LRStock.Where(x => x.LRRefTablekey == mlist.TableKey).ToList().Count() > 1)
                    {
                        mModel.DispatchLR = true;
                        mModel.BillDetails += "\nThis Consignment Stock Used It....!";
                    }

                    if (ctxTFAT.DeliveryMaster.Where(x => x.ParentKey == mlist.TableKey).ToList().Count() > 0)
                    {
                        mModel.DeliveryLR = true;
                    }
                    if (ctxTFAT.TripFmList.Where(x => x.RefTablekey == mlist.TableKey.ToString()).ToList().Count() > 0)
                    {
                        mModel.TripLock = true;
                        mModel.TripMsg = "Trip Generated Of This LR So We Can Edit Delete .........";
                    }


                    mModel.PeriodLock = PeriodLock(mlist.Branch, "LR000", mlist.BookDate);
                    if (mlist.AUTHORISE.Substring(0, 1) == "A")
                    {
                        mModel.LockAuthorise = LockAuthorise("LR000", mModel.Mode, mlist.TableKey, mlist.ParentKey);
                    }
                    mModel.ConsignerAddNo = String.IsNullOrEmpty(mlist.ConsignerAddNo) == true ? "" : mlist.ConsignerAddNo;
                    mModel.ConsigneeAddNo = String.IsNullOrEmpty(mlist.ConsigneeAddNo) == true ? "" : mlist.ConsigneeAddNo;
                    mModel.BillPartyAddNo = String.IsNullOrEmpty(mlist.BillPartyAddNo) == true ? "" : mlist.BillPartyAddNo;
                    if (!String.IsNullOrEmpty(mModel.ConsignerAddNo))
                    {
                        var ConsignerAddres = ctxTFAT.ConsignerAddress.Where(x => x.Code == mlist.RecCode && x.Sno.ToString() == mlist.ConsignerAddNo).FirstOrDefault();
                        if (ConsignerAddres != null)
                        {
                            mModel.ConsignerAddNoName = string.IsNullOrEmpty(ConsignerAddres.Addr1) == false ? ConsignerAddres.Addr1 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Addr2) == false ? ConsignerAddres.Addr2 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))))
                                                                                                                                              : (string.IsNullOrEmpty(ConsignerAddres.Addr2) == false ? ConsignerAddres.Addr2 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))));
                            if (!String.IsNullOrEmpty(mModel.ConsignerAddNoName))
                            {
                                mModel.ConsignerAddNoName = mModel.ConsignerAddNoName.Replace("\n", " ");
                            }

                        }
                    }
                    if (!String.IsNullOrEmpty(mModel.ConsigneeAddNo))
                    {
                        var ConsignerAddres = ctxTFAT.ConsignerAddress.Where(x => x.Code == mlist.SendCode && x.Sno.ToString() == mlist.ConsigneeAddNo).FirstOrDefault();
                        if (ConsignerAddres != null)
                        {
                            mModel.ConsigneeAddNoName = string.IsNullOrEmpty(ConsignerAddres.Addr1) == false ? ConsignerAddres.Addr1 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Addr2) == false ? ConsignerAddres.Addr2 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))))
                                                                                                                                            : (string.IsNullOrEmpty(ConsignerAddres.Addr2) == false ? ConsignerAddres.Addr2 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))));
                            if (!String.IsNullOrEmpty(mModel.ConsigneeAddNoName))
                            {
                                mModel.ConsigneeAddNoName = mModel.ConsigneeAddNoName.Replace("\n", " ");
                            }

                        }
                    }
                    if (!String.IsNullOrEmpty(mModel.BillPartyAddNo))
                    {
                        var ConsignerAddres = ctxTFAT.Caddress.Where(x => x.Code == mlist.BillParty && x.Sno.ToString() == mlist.BillPartyAddNo).FirstOrDefault();
                        if (ConsignerAddres != null)
                        {
                            mModel.BillPartyAddNoName = string.IsNullOrEmpty(ConsignerAddres.Adrl1) == false ? ConsignerAddres.Adrl1 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Adrl2) == false ? ConsignerAddres.Adrl2 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))))
                                                                                                                                            : (string.IsNullOrEmpty(ConsignerAddres.Adrl2) == false ? ConsignerAddres.Adrl2 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))));
                            if (!String.IsNullOrEmpty(mModel.BillPartyAddNoName))
                            {
                                mModel.BillPartyAddNoName = mModel.BillPartyAddNoName.Replace("\n", " ");
                            }
                        }
                    }

                    List<LRVM> lrdetaillist = new List<LRVM>();
                    var LrRelatedExplist = ctxTFAT.LrRelatedExp.Where(x => x.Parentkey == mlist.ParentKey).ToList();
                    foreach (var item in LrRelatedExplist)
                    {
                        lrdetaillist.Add(new LRVM
                        {
                            ExpAcount = item.ExpAcount,
                            ExpAcountName = ctxTFAT.Master.Where(x => x.Code == item.ExpAcount).Select(x => x.Name).FirstOrDefault(),
                            SubHeadAcc = item.SubHeadAcc,
                            ExpAmount = item.Amount,
                            tempId = lrdetaillist.Count() + 1,
                        });
                    }
                    mModel.LRDetailList = lrdetaillist;

                    mModel.OrderReceivedDate = mlist.OrderReceivedDate == null ? "" : mlist.OrderReceivedDate.Value.ToShortDateString();
                    mModel.DateOfOrder = mlist.DateOfOrder == null ? "" : mlist.DateOfOrder.Value.ToShortDateString();
                    mModel.ScheduleDate = mlist.ScheduleDate == null ? "" : mlist.ScheduleDate.Value.ToShortDateString();

                }
                if (mlist != null)
                {




                    mModel.Rate = mlist.Rate;
                    mModel.RateChrgOn = mlist.RateChrgOn;
                    mModel.RateType = mlist.RateType;
                    mModel.DriverTripExp = mlist.DriverTripExp == null ? 0 : mlist.DriverTripExp;

                    //mModel.Branch = mlist.Branch;
                    mModel.StockAt = mlist.StockAt;

                    mModel.DocNo = mlist.DocNo;

                    mModel.LrContactPerson = mlist.InfoName;
                    mModel.LrContactPersonNo = mlist.InfoContactNO;
                    mModel.LrContactPersonEmailId = mlist.InfoEmailId;

                    mModel.ActualExp = (decimal)mlist.ActualExp;
                    mModel.ApprovedDiesel = mlist.ApprovedDiesel;
                    mModel.PendingDiesel = mlist.PendingDiesel;
                    mModel.AdvanceDiesel = mlist.AdvanceDiesel;
                    mModel.TripNarr = mlist.TripNarr;
                    mModel.TripNo = mlist.TripNo;
                    mModel.DieselLtr = mlist.DieselLtr;
                    mModel.DieselAmt = mlist.DieselAmt;
                    mModel.Driver = mlist.Driver;
                    mModel.DriverN = ctxTFAT.DriverMaster.Where(x => x.Code == mlist.Driver).Select(x => x.Name).FirstOrDefault();

                    if (!String.IsNullOrEmpty(mlist.DocNo))
                    {
                        PickOrder pickOrder = ctxTFAT.PickOrder.Where(x => x.TableKey.ToString().Trim() == mlist.DocNo.Trim()).FirstOrDefault();
                        if (pickOrder != null)
                        {
                            mModel.PickOrderBalQty = mlist.TotQty + pickOrder.BalQty;
                        }
                    }


                    if (mModel.getRecentLR)
                    {
                        mModel.DocDate = DateTime.Now;
                        mModel.BookDate = DateTime.Now.ToShortDateString().ToString();
                        mModel.Time = DateTime.Now.ToString("HH:mm");
                    }
                    else
                    {
                        //Get Attachment
                        AttachmentVM Att = new AttachmentVM();
                        Att.Type = "LR000";
                        Att.Srl = mlist.LrNo.ToString();

                        AttachmentController attachmentC = new AttachmentController();
                        List<AttachmentVM> attachments = attachmentC.GetAttachmentListInEdit(Att);
                        Session["TempAttach"] = attachments;

                        mModel.LrNo = mlist.LrNo;
                        mModel.DocDate = mlist.BookDate;
                        mModel.BookDate = mlist.BookDate.ToShortDateString();
                        mModel.Time = mlist.Time;
                    }
                    mModel.DocNo = mlist.DocNo;
                    mModel.BENumber = mlist.BENumber;
                    mModel.PONumber = mlist.PONumber;
                    mModel.LRMode = mlist.LRMode;
                    mModel.TotQty = mlist.TotQty;
                    mModel.BillBran = mlist.BillBran;
                    mModel.BillBran_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.BillBran).Select(x => x.Name).FirstOrDefault();
                    mModel.BillParty = mlist.BillParty;
                    mModel.BillParty_Name = ctxTFAT.CustomerMaster.Where(x => x.Code == mlist.BillParty).Select(x => x.Name).FirstOrDefault();
                    mModel.LRtype = mlist.LRtype;
                    mModel.LRtype_Name = ctxTFAT.LRTypeMaster.Where(x => x.Code == mlist.LRtype).Select(x => x.LRType).FirstOrDefault();
                    mModel.ServiceType = mlist.ServiceType;
                    mModel.ServiceType_Name = ctxTFAT.ServiceTypeMaster.Where(x => x.Code == mlist.ServiceType).Select(x => x.ServiceType).FirstOrDefault();
                    mModel.RecCode = mlist.RecCode;
                    mModel.RecCode_Name = ctxTFAT.Consigner.Where(x => x.Code == mlist.RecCode).Select(x => x.Name).FirstOrDefault();
                    mModel.SendCode = mlist.SendCode;
                    mModel.SendCode_Name = ctxTFAT.Consigner.Where(x => x.Code == mlist.SendCode).Select(x => x.Name).FirstOrDefault();
                    mModel.Source = mlist.Source;
                    mModel.Source_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Source).Select(x => x.Name).FirstOrDefault();
                    mModel.Dest = mlist.Dest;
                    mModel.Dest_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Dest).Select(x => x.Name).FirstOrDefault();
                    mModel.PartyRef = mlist.PartyRef;
                    mModel.PartyInvoice = mlist.PartyInvoice;
                    mModel.GSTNO = mlist.GSTNO;
                    mModel.EwayBill = mlist.EwayBill;
                    mModel.VehicleNo = mlist.VehicleNo;
                    mModel.RecGST = mlist.RecGST;
                    mModel.SendGST = mlist.SendGST;
                    mModel.BillGST = mlist.BillGST;
                    mModel.TrnMode = mlist.TrnMode;
                    mModel.HSNCODE = mlist.HSNCODE;
                    if (!String.IsNullOrEmpty(mModel.VehicleNo))
                    {
                        if (mModel.VehicleNo.Contains("H"))
                        {
                            mModel.VehicleNoName = ctxTFAT.HireVehicleMaster.Where(x => x.Code == mlist.VehicleNo).Select(x => x.TruckNo).FirstOrDefault();
                        }
                        else
                        {
                            mModel.VehicleNoName = ctxTFAT.VehicleMaster.Where(x => x.Code == mlist.VehicleNo).Select(x => x.TruckNo).FirstOrDefault();
                        }
                    }
                    mModel.ActWt = mlist.ActWt;
                    mModel.ChgWt = mlist.ChgWt;
                    mModel.Amt = Convert.ToDecimal(mlist.Amt.ToString("F"));

                    #region Charges

                    List<PurchaseVM> objledgerdetail = new List<PurchaseVM>();
                    var trncharges = ctxTFAT.Charges.Where(x => x.MainType == "SL" && x.SubType == "RS" && x.Type == "LR000" && x.DontUse == false).Select(x => new { x.Fld, x.Head, x.EqAmt, x.Equation, x.Code }).ToList();
                    //int fmno = Convert.ToInt32(mlist.LrNo);
                    foreach (var i in trncharges)
                    {
                        PurchaseVM c = new PurchaseVM();
                        c.Fld = i.Fld;
                        c.Code = i.Head;
                        c.AddLess = i.EqAmt;
                        c.Equation = i.Equation;
                        c.tempId = Convert.ToInt16(i.Fld.Substring(1, 3));
                        c.Val1 = GetChargeValValue(c.tempId, "Val", mlist.TableKey);
                        c.ChgPostCode = i.Code;
                        objledgerdetail.Add(c);
                    }
                    mModel.Charges = objledgerdetail;

                    #endregion

                    mModel.DecVal = (mlist.DecVal);
                    mModel.DescrType = mlist.DescrType;
                    mModel.DescrType_Name = ctxTFAT.DescriptionMaster.Where(x => x.Code == mlist.DescrType).Select(x => x.Description).FirstOrDefault();
                    mModel.ChgType = mlist.ChgType;
                    mModel.ChgType_Name = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == mlist.ChgType).Select(x => x.ChargeType).FirstOrDefault();
                    mModel.UnitCode = mlist.UnitCode;
                    mModel.UnitCode_Name = ctxTFAT.UnitMaster.Where(x => x.Code == mlist.UnitCode).Select(x => x.Name).FirstOrDefault();
                    mModel.Colln = mlist.Colln == null ? "G" : mlist.Colln;
                    mModel.Delivery = mlist.Delivery == null ? "G" : mlist.Delivery;
                    mModel.FormNo = mlist.FormNo;
                    mModel.TransactionAt = mlist.TransactionAt;
                    mModel.GstLiable = mlist.GstLiable;
                    mModel.Narr = mlist.Narr;
                    mModel.Prefix = mlist.Prefix;
                    mModel.TrType = mlist.TrType;
                    mModel.ConsignerEXTRAInfo = mlist.ConsignerInfo;
                    mModel.ConsigneeEXTRAInfo = mlist.ConsigneeInfo;
                    mModel.BillingPartyEXTRAInfo = mlist.BillingPartyInfo;
                    mModel.DispachLC = mlist.DispachLC;
                    mModel.DispachFM = mlist.DispachFM;
                    mModel.Crossing = mlist.Crossing;
                }
                else
                {
                    var getdetailsOfCurrentBRanch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).FirstOrDefault();
                    if (getdetailsOfCurrentBRanch.Category != "0" || getdetailsOfCurrentBRanch.Category != "Zone")
                    {
                        mModel.Source = getdetailsOfCurrentBRanch.Code;
                        mModel.Source_Name = getdetailsOfCurrentBRanch.Name;
                    }

                    //mModel.LrNo = GetNewCode();
                    mModel.BookDate = DateTime.Now.ToShortDateString().ToString();
                    mModel.Time = DateTime.Now.ToString("HH:mm");
                    mModel.DocDate = DateTime.Now;
                    if (mModel.LRSetup != null)
                    {

                        mModel.LRtype = mModel.LRSetup.LRType;
                        mModel.LRtype_Name = ctxTFAT.LRTypeMaster.Where(x => x.Code == mModel.LRSetup.LRType).Select(x => x.LRType).FirstOrDefault();

                        mModel.Source = mbranchcode;
                        mModel.Source_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault();

                        mModel.ServiceType = mModel.LRSetup.ServiceType;
                        mModel.ServiceType_Name = ctxTFAT.ServiceTypeMaster.Where(x => x.Code == mModel.LRSetup.ServiceType).Select(x => x.ServiceType).FirstOrDefault();

                        mModel.ChgType = mModel.LRSetup.ChrType;
                        mModel.ChgType_Name = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == mModel.LRSetup.ChrType).Select(x => x.ChargeType).FirstOrDefault();

                        mModel.UnitCode = mModel.LRSetup.Unit;
                        mModel.UnitCode_Name = ctxTFAT.UnitMaster.Where(x => x.Code == mModel.LRSetup.Unit).Select(x => x.Name).FirstOrDefault();

                        mModel.TotQty = mModel.LRSetup.defaultQty;

                        mModel.BillBran = mModel.LRSetup.BillBranch;
                        mModel.BillBran_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.BillBran).Select(x => x.Name).FirstOrDefault();

                        mModel.Colln = mModel.LRSetup.Colln;
                        mModel.Delivery = mModel.LRSetup.Del;
                    }
                    TempData["LRAttachmentList"] = mModel.attachments;
                    mModel.LRMode = "G";
                    #region Fresh Charges
                    //var getCharList = ctxTFAT.Charges.Where(x => x.MainType == "SL" && x.SubType == "RS" && x.Type == "LR" && x.DontUse == false).Select(x => new { x.Head, x.Fld }).ToArray();
                    //int ArraySize = getCharList.Count();
                    //string[] ChargeValue = new string[ArraySize];
                    //for (int i = 0; i < ChargeValue.Length; i++)
                    //{
                    //    ChargeValue[i] = 0.ToString();
                    //}
                    //mModel.ChargeName = getCharList.Select(x => x.Head).ToArray();
                    //mModel.Fld = getCharList.Select(x => x.Fld).ToArray();
                    //mModel.ChargeValue = ChargeValue;
                    List<PurchaseVM> objledgerdetail = new List<PurchaseVM>();
                    var trncharges = ctxTFAT.Charges.Where(x => x.MainType == "SL" && x.SubType == "RS" && x.Type == "LR000" && x.DontUse == false).Select(x => new { x.Fld, x.Head, x.EqAmt, x.Equation, x.Code }).ToList();
                    int fmno = Convert.ToInt32(mlist.LrNo);
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
                    mModel.Charges = objledgerdetail;
                    #endregion

                    mModel.StockAt = "Godown";
                }
            }
            else if ((mModel.Mode == "Select"))
            {
                mModel.Mode = "Add";
                var mlist1 = ctxTFAT.LRMasterDraft.Where(x => (x.LrNo.ToString() == mModel.Document)).FirstOrDefault();
                if (mlist1 != null)
                {
                    mModel.StockAt = "Godown";
                    mModel.ConsignerAddNo = "";
                    mModel.ConsigneeAddNo = "";
                    mModel.BillPartyAddNo = "";
                    mModel.PONumber = mlist1.PONumber;
                    mModel.BENumber = mlist1.BENumber;
                    mModel.Draft_Name = mlist1.DraftName;
                    //mModel.Branch = mlist1.Branch;
                    mModel.BookDate = DateTime.Now.ToShortDateString();
                    mModel.Time = DateTime.Now.ToString("HH:mm");
                    mModel.DocDate = DateTime.Now;
                    mModel.DocNo = mlist1.DocNo;
                    mModel.TotQty = Convert.ToInt32(mlist1.TotQty);

                    mModel.BillBran_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mlist1.BillBran).Select(x => x.Name).FirstOrDefault();
                    if (!String.IsNullOrEmpty(mModel.BillBran_Name))
                    {
                        mModel.BillBran = mlist1.BillBran;
                    }
                    mModel.BillParty_Name = ctxTFAT.CustomerMaster.Where(x => x.Code == mlist1.BillParty).Select(x => x.Name).FirstOrDefault();
                    if (!String.IsNullOrEmpty(mModel.BillParty_Name))
                    {
                        mModel.BillParty = mlist1.BillParty;
                        mModel.HitContractType = ctxTFAT.CustomerMaster.Where(x => x.Code == mModel.BillParty).Select(x => x.ContractHitby).FirstOrDefault();
                    }
                    mModel.LRtype_Name = ctxTFAT.LRTypeMaster.Where(x => x.Code == mlist1.LRtype).Select(x => x.LRType).FirstOrDefault();
                    if (!String.IsNullOrEmpty(mModel.LRtype_Name))
                    {
                        mModel.LRtype = mlist1.LRtype;
                    }
                    mModel.ServiceType_Name = ctxTFAT.ServiceTypeMaster.Where(x => x.Code == mlist1.ServiceType).Select(x => x.ServiceType).FirstOrDefault();
                    if (!String.IsNullOrEmpty(mModel.ServiceType_Name))
                    {
                        mModel.ServiceType = mlist1.ServiceType;
                    }

                    mModel.RecCode_Name = ctxTFAT.Consigner.Where(x => x.Code == mlist1.RecCode).Select(x => x.Name).FirstOrDefault();
                    if (!String.IsNullOrEmpty(mModel.RecCode_Name))
                    {
                        mModel.RecCode = mlist1.RecCode;
                    }

                    mModel.SendCode_Name = ctxTFAT.Consigner.Where(x => x.Code == mlist1.SendCode).Select(x => x.Name).FirstOrDefault();
                    if (!String.IsNullOrEmpty(mModel.SendCode_Name))
                    {
                        mModel.SendCode = mlist1.SendCode;
                    }

                    mModel.Source_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mlist1.Source).Select(x => x.Name).FirstOrDefault();
                    if (!String.IsNullOrEmpty(mModel.Source_Name))
                    {
                        mModel.Source = mlist1.Source;
                    }

                    mModel.Dest_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mlist1.Dest).Select(x => x.Name).FirstOrDefault();
                    if (!String.IsNullOrEmpty(mModel.Dest_Name))
                    {
                        mModel.Dest = mlist1.Dest;
                    }
                    mModel.PartyRef = mlist1.PartyRef;
                    mModel.PartyInvoice = mlist1.PartyInvoice;
                    mModel.GSTNO = mlist1.GSTNO;
                    mModel.EwayBill = mlist1.EwayBill;
                    mModel.VehicleNo = mlist1.VehicleNo;
                    if (!String.IsNullOrEmpty(mModel.VehicleNo))
                    {
                        if (mModel.VehicleNo.Contains("H"))
                        {
                            mModel.VehicleNoName = ctxTFAT.HireVehicleMaster.Where(x => x.Code == mlist1.VehicleNo).Select(x => x.TruckNo).FirstOrDefault();
                        }
                        else
                        {
                            mModel.VehicleNoName = ctxTFAT.VehicleMaster.Where(x => x.Code == mlist1.VehicleNo).Select(x => x.TruckNo).FirstOrDefault();
                        }
                    }
                    mModel.ActWt = Convert.ToDouble(mlist1.ActWt);
                    mModel.ChgWt = Convert.ToDouble(mlist1.ChgWt);
                    mModel.Amt = Convert.ToDecimal(Convert.ToDecimal(mlist1.Amt).ToString("F"));
                    mModel.LRMode = "G";
                    #region Charges

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
                        c.Val1 = GetChargeValValueOFDraft(c.tempId, "Val", mlist1.LrNo);
                        c.ChgPostCode = i.Code;
                        objledgerdetail.Add(c);
                    }
                    mModel.Charges = objledgerdetail;
                    #endregion

                    mModel.DecVal = (decimal)(mlist1.DecVal);
                    mModel.DescrType_Name = ctxTFAT.DescriptionMaster.Where(x => x.Code == mlist1.DescrType).Select(x => x.Description).FirstOrDefault();
                    if (!String.IsNullOrEmpty(mModel.DescrType_Name))
                    {
                        mModel.DescrType = mlist1.DescrType;
                    }
                    mModel.ChgType_Name = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == mlist1.ChgType).Select(x => x.ChargeType).FirstOrDefault();
                    if (!String.IsNullOrEmpty(mModel.ChgType_Name))
                    {
                        mModel.ChgType = mlist1.ChgType;
                    }
                    mModel.UnitCode_Name = ctxTFAT.UnitMaster.Where(x => x.Code == mlist1.UnitCode).Select(x => x.Name).FirstOrDefault();
                    if (!String.IsNullOrEmpty(mModel.UnitCode_Name))
                    {
                        mModel.UnitCode = mlist1.UnitCode;
                    }
                    mModel.Colln = mlist1.Colln == null ? "G" : mlist1.Colln;
                    mModel.Delivery = mlist1.Delivery == null ? "G" : mlist1.Delivery;
                    mModel.FormNo = mlist1.FormNo;
                    mModel.TransactionAt = mlist1.TransactionAt;
                    mModel.GstLiable = mlist1.GstLiable;
                    mModel.Narr = mlist1.Narr;
                    mModel.Prefix = mlist1.Prefix;
                    mModel.TrType = mlist1.TrType;
                    mModel.ConsignerEXTRAInfo = mlist1.ConsignerInfo;
                    mModel.ConsigneeEXTRAInfo = mlist1.ConsigneeInfo;
                    mModel.BillingPartyEXTRAInfo = mlist1.BillingPartyInfo;
                    //mModel.LrNo = GetNewCode();
                    TempData["LRAttachmentList"] = mModel.attachments;
                }
                List<LRVM> lrdetaillist = new List<LRVM>();
                mModel.LRDetailList = lrdetaillist;
            }
            else if ((mModel.Mode == "Pick"))
            {
                mModel.Mode = "Add";
                var mlist1 = ctxTFAT.PickOrder.Where(x => (x.TableKey.ToString() == mModel.Document)).FirstOrDefault();
                if (mlist1 != null)
                {
                    mModel.StockAt = "Godown";
                    mModel.ConsignerAddNo = "";
                    mModel.ConsigneeAddNo = "";
                    mModel.BillPartyAddNo = "";
                    mModel.PONumber = mlist1.PONumber;
                    mModel.BENumber = mlist1.BENumber;
                    //mModel.Branch = mlist1.Branch;
                    mModel.BookDate = DateTime.Now.ToShortDateString();
                    mModel.Time = DateTime.Now.ToString("HH:mm");
                    mModel.DocDate = DateTime.Now;
                    mModel.DocNo = mlist1.TableKey.ToString();
                    mModel.TotQty = Convert.ToInt32(mlist1.BalQty);

                    mModel.BillBran_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mlist1.BillBran).Select(x => x.Name).FirstOrDefault();
                    if (!String.IsNullOrEmpty(mModel.BillBran_Name))
                    {
                        mModel.BillBran = mlist1.BillBran;
                    }
                    mModel.BillParty_Name = ctxTFAT.CustomerMaster.Where(x => x.Code == mlist1.BillParty).Select(x => x.Name).FirstOrDefault();
                    if (!String.IsNullOrEmpty(mModel.BillParty_Name))
                    {
                        mModel.BillParty = mlist1.BillParty;
                        mModel.HitContractType = ctxTFAT.CustomerMaster.Where(x => x.Code == mModel.BillParty).Select(x => x.ContractHitby).FirstOrDefault();
                    }
                    mModel.LRtype_Name = ctxTFAT.LRTypeMaster.Where(x => x.Code == mlist1.LRtype).Select(x => x.LRType).FirstOrDefault();
                    if (!String.IsNullOrEmpty(mModel.LRtype_Name))
                    {
                        mModel.LRtype = mlist1.LRtype;
                    }
                    mModel.ServiceType_Name = ctxTFAT.ServiceTypeMaster.Where(x => x.Code == mlist1.ServiceType).Select(x => x.ServiceType).FirstOrDefault();
                    if (!String.IsNullOrEmpty(mModel.ServiceType_Name))
                    {
                        mModel.ServiceType = mlist1.ServiceType;
                    }

                    mModel.RecCode_Name = ctxTFAT.Consigner.Where(x => x.Code == mlist1.RecCode).Select(x => x.Name).FirstOrDefault();
                    if (!String.IsNullOrEmpty(mModel.RecCode_Name))
                    {
                        mModel.RecCode = mlist1.RecCode;
                    }

                    mModel.SendCode_Name = ctxTFAT.Consigner.Where(x => x.Code == mlist1.SendCode).Select(x => x.Name).FirstOrDefault();
                    if (!String.IsNullOrEmpty(mModel.SendCode_Name))
                    {
                        mModel.SendCode = mlist1.SendCode;
                    }

                    mModel.Source_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mlist1.Source).Select(x => x.Name).FirstOrDefault();
                    if (!String.IsNullOrEmpty(mModel.Source_Name))
                    {
                        mModel.Source = mlist1.Source;
                    }

                    mModel.Dest_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mlist1.Dest).Select(x => x.Name).FirstOrDefault();
                    if (!String.IsNullOrEmpty(mModel.Dest_Name))
                    {
                        mModel.Dest = mlist1.Dest;
                    }
                    mModel.PartyRef = mlist1.PartyRef;
                    mModel.PartyInvoice = mlist1.PartyInvoice;
                    mModel.GSTNO = mlist1.GSTNO;
                    mModel.EwayBill = mlist1.EwayBill;
                    mModel.VehicleNo = mlist1.VehicleNo;
                    if (!String.IsNullOrEmpty(mModel.VehicleNo))
                    {
                        if (mModel.VehicleNo.Contains("H"))
                        {
                            mModel.VehicleNoName = ctxTFAT.HireVehicleMaster.Where(x => x.Code == mlist1.VehicleNo).Select(x => x.TruckNo).FirstOrDefault();
                        }
                        else
                        {
                            mModel.VehicleNoName = ctxTFAT.VehicleMaster.Where(x => x.Code == mlist1.VehicleNo).Select(x => x.TruckNo).FirstOrDefault();
                        }
                    }
                    mModel.ActWt = Convert.ToDouble(mlist1.ActWt);
                    mModel.ChgWt = Convert.ToDouble(mlist1.ChgWt);
                    mModel.Amt = Convert.ToDecimal(Convert.ToDecimal(mlist1.Amt).ToString("F"));
                    mModel.LRMode = "G";
                    #region Charges

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
                        c.Val1 = GetChargeValValueOFPick(c.tempId, "Val", mlist1.TableKey.ToString());
                        c.ChgPostCode = i.Code;
                        objledgerdetail.Add(c);
                    }
                    mModel.Charges = objledgerdetail;
                    #endregion

                    mModel.DecVal = (decimal)(mlist1.DecVal == null ? 0 : mlist1.DecVal);
                    mModel.DescrType_Name = ctxTFAT.DescriptionMaster.Where(x => x.Code == mlist1.DescrType).Select(x => x.Description).FirstOrDefault();
                    if (!String.IsNullOrEmpty(mModel.DescrType_Name))
                    {
                        mModel.DescrType = mlist1.DescrType;
                    }
                    mModel.ChgType_Name = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == mlist1.ChgType).Select(x => x.ChargeType).FirstOrDefault();
                    if (!String.IsNullOrEmpty(mModel.ChgType_Name))
                    {
                        mModel.ChgType = mlist1.ChgType;
                    }
                    mModel.UnitCode_Name = ctxTFAT.UnitMaster.Where(x => x.Code == mlist1.UnitCode).Select(x => x.Name).FirstOrDefault();
                    if (!String.IsNullOrEmpty(mModel.UnitCode_Name))
                    {
                        mModel.UnitCode = mlist1.UnitCode;
                    }
                    mModel.Colln = mlist1.Colln == null ? "G" : mlist1.Colln;
                    mModel.Delivery = mlist1.Delivery == null ? "G" : mlist1.Delivery;
                    mModel.FormNo = mlist1.FormNo;
                    mModel.TransactionAt = mlist1.TransactionAt;
                    mModel.GstLiable = mlist1.GstLiable;
                    mModel.Narr = mlist1.Narr;
                    mModel.Prefix = mlist1.Prefix;
                    mModel.TrType = mlist1.TrType;
                    mModel.ConsignerEXTRAInfo = mlist1.ConsignerInfo;
                    mModel.ConsigneeEXTRAInfo = mlist1.ConsigneeInfo;
                    mModel.BillingPartyEXTRAInfo = mlist1.BillingPartyInfo;

                    mModel.LrContactPersonNo = mlist1.InfoContactNO;
                    mModel.LrContactPersonEmailId = mlist1.InfoEmailId;
                    mModel.LrContactPerson = mlist1.InfoName;
                    mModel.DocNo = mlist1.TableKey.ToString();
                    mModel.PickOrderBalQty = mlist1.BalQty;

                    //mModel.LrNo = GetNewCode();
                    TempData["LRAttachmentList"] = mModel.attachments;
                }
                List<LRVM> lrdetaillist = new List<LRVM>();
                mModel.LRDetailList = lrdetaillist;
            }
            else
            {

                mModel.StockAt = "Godown";
                var RateChrgOn = Convert.ToString(TempData.Peek("LR000PreviousRateChrgOn"));
                var RateType = Convert.ToString(TempData.Peek("LR000PreviousRateType"));
                if (!String.IsNullOrEmpty(RateChrgOn))
                {
                    mModel.RateChrgOn = RateChrgOn;
                }
                if (!String.IsNullOrEmpty(RateType))
                {
                    mModel.RateType = RateType;
                }
                var getdetailsOfCurrentBRanch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).FirstOrDefault();
                if (getdetailsOfCurrentBRanch.Category != "0" || getdetailsOfCurrentBRanch.Category != "Zone")
                {
                    mModel.Source = getdetailsOfCurrentBRanch.Code;
                    mModel.Source_Name = getdetailsOfCurrentBRanch.Name;
                }

                mModel.BookDate = DateTime.Now.ToShortDateString().ToString();
                mModel.Time = DateTime.Now.ToString("HH:mm");
                mModel.DocDate = DateTime.Now;
                if (mModel.LRSetup != null)
                {
                    mModel.TrnMode = mModel.LRSetup.TrnMode;

                    mModel.LRtype = mModel.LRSetup.LRType;
                    mModel.LRtype_Name = ctxTFAT.LRTypeMaster.Where(x => x.Code == mModel.LRSetup.LRType).Select(x => x.LRType).FirstOrDefault();

                    mModel.Source = mbranchcode;
                    mModel.Source_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault();

                    mModel.ServiceType = mModel.LRSetup.ServiceType;
                    mModel.ServiceType_Name = ctxTFAT.ServiceTypeMaster.Where(x => x.Code == mModel.LRSetup.ServiceType).Select(x => x.ServiceType).FirstOrDefault();

                    mModel.ChgType = mModel.LRSetup.ChrType;
                    mModel.ChgType_Name = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == mModel.LRSetup.ChrType).Select(x => x.ChargeType).FirstOrDefault();

                    mModel.UnitCode = mModel.LRSetup.Unit;
                    mModel.UnitCode_Name = ctxTFAT.UnitMaster.Where(x => x.Code == mModel.LRSetup.Unit).Select(x => x.Name).FirstOrDefault();

                    mModel.TotQty = mModel.LRSetup.defaultQty;

                    mModel.BillBran = mModel.LRSetup.BillBranch;
                    mModel.BillBran_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.BillBran).Select(x => x.Name).FirstOrDefault();

                    mModel.Colln = mModel.LRSetup.Colln;
                    mModel.Delivery = mModel.LRSetup.Del;
                }
                TempData["LRAttachmentList"] = mModel.attachments;
                mModel.LRMode = "G";
                #region Fresh Charges
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
                mModel.Charges = objledgerdetail;
                #endregion

                mModel.ConsignerAddNo = "";
                mModel.ConsigneeAddNo = "";
                mModel.BillPartyAddNo = "";

                List<LRVM> lrdetaillist = new List<LRVM>();
                mModel.LRDetailList = lrdetaillist;
            }

            if (mModel.LRSetup == null)
            {
                mModel.LRSetup = new LRSetup();
            }

            List<GridOption> Grlist = new List<GridOption>();
            var list = ctxTFAT.DocFormats.Where(x => x.Type == "LR000").Select(x => x).ToList();
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

            if (mModel.Mode == "Add" && mModel.LRSetup.GetAutoTripNo == true)
            {
                mModel.TripNo = GetLastSerial("TripSheetMaster", mbranchcode, "Trip0", mperiod, "JV", DateTime.Now.Date);
            }

            return View(mModel);
        }

        [HttpPost]
        public ActionResult LorryDraftIndex(GridOption Model)
        {


            Model.Document = "";
            Model.AccountName = "";
            moptioncode = "LorryDraft";
            msubcodeof = "LorryDraft";
            mmodule = "Transactions";
            ViewBag.id = "LorryDraft";
            ViewBag.ViewDataId = "LorryDraft";
            ViewBag.Header = "LorryDraft";
            ViewBag.Table = "LRMaster_Draft";
            ViewBag.Controller = "LorryReceipt";
            ViewBag.MainType = "M";
            ViewBag.Controller2 = "LorryReceipt";
            ViewBag.OptionType = "";
            ViewBag.OptionCode = "LorryDraft";
            ViewBag.Module = "Transactions";
            ViewBag.ViewName = "";
            ViewBag.ViewDataId1 = "LorryDraft";

            var html = ViewHelper.RenderPartialView(this, "LorryDraftIndex", Model);
            var jsonResult = Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }
        [HttpPost]
        public ActionResult PickUpList(GridOption Model)
        {
            Model.Document = "";
            Model.AccountName = "";
            moptioncode = "LROrderRequest";
            msubcodeof = "LROrderRequest";
            mmodule = "Transactions";
            ViewBag.id = "OrderRequest";
            ViewBag.ViewDataId = "OrderRequest";
            ViewBag.Header = "OrderRequest";
            ViewBag.Table = "PickOrder";
            ViewBag.Controller = "LorryReceipt";
            ViewBag.MainType = "M";
            ViewBag.Controller2 = "LorryReceipt";
            ViewBag.OptionType = "";
            ViewBag.OptionCode = "OrderRequest";
            ViewBag.Module = "Transactions";
            ViewBag.ViewName = "";
            ViewBag.ViewDataId1 = "OrderRequest";

            var html = ViewHelper.RenderPartialView(this, "OrderRequestList", Model);
            var jsonResult = Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public ActionResult DraftIndex(LRVM mModel)
        {
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;


            #region LrSetUp
            mModel.LRSetup = ctxTFAT.LRSetup.FirstOrDefault();
            if (mModel.LRSetup == null)
            {
                mModel.LRSetup = new LRSetup();
            }
            if (mModel.LRSetup.CurrDatetOnlyreq == false && mModel.LRSetup.BackDateAllow == false && mModel.LRSetup.ForwardDateAllow == false)
            {
                mModel.StartDate = ConvertDDMMYYTOYYMMDD(StartDate).ToString("yyyy-MM-dd");
                mModel.EndDate = ConvertDDMMYYTOYYMMDD(EndDate).ToString("yyyy-MM-dd");
            }
            if (mModel.LRSetup.CurrDatetOnlyreq == true)
            {
                mModel.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
                mModel.EndDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            if (mModel.LRSetup.BackDateAllow == true)
            {
                mModel.StartDate = (DateTime.Now.AddDays(-mModel.LRSetup.BackDaysUpto)).ToString("yyyy-MM-dd");
            }
            if (mModel.LRSetup.ForwardDateAllow == true)
            {
                mModel.EndDate = (DateTime.Now.AddDays(mModel.LRSetup.ForwardDaysUpto)).ToString("yyyy-MM-dd");
            }
            #endregion


            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete") || (mModel.Mode == "Select") || (mModel.getRecentLR == true))
            {

                var mlist = ctxTFAT.LRMasterDraft.Where(x => (x.LrNo.ToString() == mModel.Document)).FirstOrDefault();
                if (mlist != null)
                {
                    mModel.PONumber = mlist.PONumber;
                    mModel.BENumber = mlist.BENumber;
                    mModel.Draft_Name = mlist.DraftName;
                    mModel.Branch = mlist.Branch;
                    mModel.DocDate = DateTime.Now;
                    mModel.BookDate = DateTime.Now.ToShortDateString();
                    mModel.Time = DateTime.Now.ToString("HH:mm");
                    mModel.DocNo = mlist.DocNo;
                    //mModel.LrNo = Convert.ToInt32(mlist.LrNo);
                    mModel.TotQty = Convert.ToInt32(mlist.TotQty);
                    mModel.BillBran = mlist.BillBran;
                    mModel.BillBran_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.BillBran).Select(x => x.Name).FirstOrDefault();
                    mModel.BillParty = mlist.BillParty;
                    mModel.BillParty_Name = ctxTFAT.Master.Where(x => x.Code == mlist.BillParty).Select(x => x.Name).FirstOrDefault();
                    mModel.LRtype = mlist.LRtype;
                    mModel.LRtype_Name = ctxTFAT.LRTypeMaster.Where(x => x.Code == mlist.LRtype).Select(x => x.LRType).FirstOrDefault();
                    mModel.ServiceType = mlist.ServiceType;
                    mModel.ServiceType_Name = ctxTFAT.ServiceTypeMaster.Where(x => x.Code == mlist.ServiceType).Select(x => x.ServiceType).FirstOrDefault();
                    mModel.RecCode = mlist.RecCode;
                    mModel.RecCode_Name = ctxTFAT.Consigner.Where(x => x.Code == mlist.RecCode).Select(x => x.Name).FirstOrDefault();
                    mModel.SendCode = mlist.SendCode;
                    mModel.SendCode_Name = ctxTFAT.Consigner.Where(x => x.Code == mlist.SendCode).Select(x => x.Name).FirstOrDefault();
                    mModel.Source = mlist.Source;
                    mModel.Source_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Source).Select(x => x.Name).FirstOrDefault();
                    mModel.Dest = mlist.Dest;
                    mModel.Dest_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.Dest).Select(x => x.Name).FirstOrDefault();
                    mModel.PartyRef = mlist.PartyRef;
                    mModel.PartyInvoice = mlist.PartyInvoice;
                    mModel.GSTNO = mlist.GSTNO;
                    mModel.EwayBill = mlist.EwayBill;
                    mModel.VehicleNo = mlist.VehicleNo;
                    if (!String.IsNullOrEmpty(mModel.VehicleNo))
                    {
                        if (mModel.VehicleNo.Contains("H"))
                        {
                            mModel.VehicleNoName = ctxTFAT.HireVehicleMaster.Where(x => x.Code == mlist.VehicleNo).Select(x => x.TruckNo).FirstOrDefault();
                        }
                        else
                        {
                            mModel.VehicleNoName = ctxTFAT.VehicleMaster.Where(x => x.Code == mlist.VehicleNo).Select(x => x.TruckNo).FirstOrDefault();
                        }
                    }
                    mModel.ActWt = Convert.ToDouble(mlist.ActWt);
                    mModel.ChgWt = Convert.ToDouble(mlist.ChgWt);
                    mModel.Amt = Convert.ToDecimal(Convert.ToDecimal(mlist.Amt).ToString("F"));
                    mModel.LRMode = mlist.LRMode;
                    #region Charges
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
                        c.Val1 = GetChargeValValueOFDraft(c.tempId, "Val", mlist.LrNo);
                        c.ChgPostCode = i.Code;
                        objledgerdetail.Add(c);
                    }
                    mModel.Charges = objledgerdetail;
                    #endregion

                    mModel.DecVal = (decimal)(mlist.DecVal);
                    mModel.DescrType = mlist.DescrType;
                    mModel.DescrType_Name = ctxTFAT.DescriptionMaster.Where(x => x.Code == mlist.DescrType).Select(x => x.Description).FirstOrDefault();
                    mModel.ChgType = mlist.ChgType;
                    mModel.ChgType_Name = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == mlist.ChgType).Select(x => x.ChargeType).FirstOrDefault();
                    mModel.UnitCode = mlist.UnitCode;
                    mModel.UnitCode_Name = ctxTFAT.UnitMaster.Where(x => x.Code == mlist.UnitCode).Select(x => x.Name).FirstOrDefault();
                    mModel.Colln = mlist.Colln;
                    mModel.Delivery = mlist.Delivery;
                    mModel.FormNo = mlist.FormNo;
                    mModel.TransactionAt = mlist.TransactionAt;
                    mModel.GstLiable = mlist.GstLiable;
                    mModel.Narr = mlist.Narr;
                    mModel.Prefix = mlist.Prefix;
                    mModel.TrType = mlist.TrType;
                    mModel.ConsignerEXTRAInfo = mlist.ConsignerInfo;
                    mModel.ConsigneeEXTRAInfo = mlist.ConsigneeInfo;
                    mModel.BillingPartyEXTRAInfo = mlist.BillingPartyInfo;
                    TempData["LRAttachmentList"] = mModel.attachments;
                }
            }
            else
            {
                mModel.BookDate = DateTime.Now.ToShortDateString().ToString();
                mModel.Time = DateTime.Now.ToString("HH:mm");
                mModel.DocDate = DateTime.Now;

                mModel.LRtype = mModel.LRSetup.LRType;
                mModel.LRtype_Name = ctxTFAT.LRTypeMaster.Where(x => x.Code == mModel.LRSetup.LRType).Select(x => x.LRType).FirstOrDefault();

                mModel.ServiceType = mModel.LRSetup.ServiceType;
                mModel.ServiceType_Name = ctxTFAT.ServiceTypeMaster.Where(x => x.Code == mModel.LRSetup.ServiceType).Select(x => x.ServiceType).FirstOrDefault();
                TempData["LRAttachmentList"] = mModel.attachments;
                mModel.LRMode = "G";
                #region Fresh Charges
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
                mModel.Charges = objledgerdetail;
                #endregion
            }

            return View(mModel);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
            //var result = ctxTFAT.TfatMenu.Where(z => z.OptionCode == moptioncode && z.ModuleName == mmodule).Select(x => new { x.AllowEdit, x.AllowDelete }).FirstOrDefault();
            //string mopt = "EDVX";
            //if (result != null)
            //{
            //    mopt = (result.AllowEdit == false ? "X" : "E") + (result.AllowDelete == false ? "X" : "D") + "XX";
            //}
            //return GetGridDataColumns(id, "L", mopt);
            return GetGridDataColumns(id, "L", "EDVX");
        }

        public decimal GetChargeValValue(int i, string mfield, string Tablekey)
        {
            string connstring = GetConnectionString();
            string abc;
            var loginQuery3 = @"select " + mfield + i + " from LRMaster where Tablekey='" + Tablekey + "' ";

            DataTable mDt2 = GetDataTable(loginQuery3, connstring);
            if (mDt2.Rows.Count > 0)
            {
                abc = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? "0" : mDt2.Rows[0][0].ToString();
            }
            else
            {
                abc = "0";
            }
            //mModel.Amt = Convert.ToDecimal(mlist.Amt.ToString("F"));
            return Convert.ToDecimal(abc);
        }
        public decimal GetChargeValValueOFDraft(int i, string mfield, String LRNO)
        {
            string connstring = GetConnectionString();
            string abc;
            var loginQuery3 = @"select " + mfield + i + " from lrmasterdraft where LRno='" + LRNO + "' ";

            DataTable mDt2 = GetDataTable(loginQuery3, connstring);
            if (mDt2.Rows.Count > 0)
            {
                abc = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? "0" : mDt2.Rows[0][0].ToString();
            }
            else
            {
                abc = "0";
            }
            return Convert.ToDecimal(abc);
        }
        public decimal GetChargeValValueOFPick(int i, string mfield, String LRNO)
        {
            string connstring = GetConnectionString();
            string abc;
            var loginQuery3 = @"select " + mfield + i + " from PickOrder where Tablekey='" + LRNO + "' ";

            DataTable mDt2 = GetDataTable(loginQuery3, connstring);
            if (mDt2.Rows.Count > 0)
            {
                abc = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? "0" : mDt2.Rows[0][0].ToString();
            }
            else
            {
                abc = "0";
            }
            return Convert.ToDecimal(abc);
        }
        #endregion


        #region ADD LR details

        public ActionResult AddLRDetails(LRVM Model)
        {
            List<LRVM> lrdetaillist = new List<LRVM>();
            if (Model.Mode == "Add")
            {
                if (Model.LRDetailList != null)
                {
                    lrdetaillist = Model.LRDetailList;
                }

                if (String.IsNullOrEmpty(Model.ExpAcount))
                {
                    Model.Status = "ValidError";
                    Model.Message = "Please Select The Expenses Account...";
                    return Json(Model, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (lrdetaillist.Where(x => x.ExpAcount == Model.ExpAcount).FirstOrDefault() != null)
                    {
                        Model.Status = "ValidError";
                        Model.Message = "Duplicate Expenses Account Not Allow...";
                        return Json(Model, JsonRequestBehavior.AllowGet);
                    }
                }

                if (Model.ExpAmount == 0)
                {
                    Model.Status = "ValidError";
                    Model.Message = "Please Enter The Amount...";
                    return Json(Model, JsonRequestBehavior.AllowGet);
                }
                lrdetaillist.Add(new LRVM()
                {
                    ExpAcount = Model.ExpAcount,
                    ExpAcountName = ctxTFAT.Master.Where(x => x.Code.Trim() == Model.ExpAcount.Trim()).Select(x => x.Name).FirstOrDefault(),
                    SubHeadAcc = Model.SubHeadAcc,
                    ExpAmount = Model.ExpAmount,
                    tempId = lrdetaillist.Count + 1,

                });
            }
            else
            {
                if (String.IsNullOrEmpty(Model.ExpAcount))
                {
                    Model.Status = "ValidError";
                    Model.Message = "Please Select The Expenses Account...";
                    return Json(Model, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (lrdetaillist.Where(x => x.ExpAcount == Model.ExpAcount && x.tempId != Model.tempId).FirstOrDefault() != null)
                    {
                        Model.Status = "ValidError";
                        Model.Message = "Duplicate Expenses Account Not Allow...";
                        return Json(Model, JsonRequestBehavior.AllowGet);
                    }
                }
                if (Model.ExpAmount == 0)
                {
                    Model.Status = "ValidError";
                    Model.Message = "Please Enter The Amount..";
                    return Json(Model, JsonRequestBehavior.AllowGet);
                }
                if (Model.LRDetailList != null)
                {
                    lrdetaillist = Model.LRDetailList;
                }
                foreach (var item in lrdetaillist.Where(x => x.tempId == Model.tempId))
                {
                    item.ExpAcount = Model.ExpAcount;
                    item.ExpAcountName = ctxTFAT.Master.Where(x => x.Code.Trim() == Model.ExpAcount.Trim()).Select(x => x.Name).FirstOrDefault();
                    item.SubHeadAcc = Model.SubHeadAcc;
                    item.ExpAmount = Model.ExpAmount;
                    item.tempId = Model.tempId;
                }
            }

            var html = ViewHelper.RenderPartialView(this, "ConsignmentExpGridList", new LRVM() { LRDetailList = lrdetaillist });
            return Json(new { LRDetailList = lrdetaillist, Html = html, Amt = lrdetaillist.Sum(x => x.ExpAmount) }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult DeleteLRDetails(LRVM Model)
        {
            var result2 = Model.LRDetailList.Where(x => x.tempId != Model.tempId).Select(x => x).ToList();
            var html = ViewHelper.RenderPartialView(this, "ConsignmentExpGridList", new LRVM() { LRDetailList = result2 });
            return Json(new { LRDetailList = result2, Html = html, Amt = result2.Sum(x => x.ExpAmount) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetLRDetails(LRVM Model)
        {
            if (Model.LRDetailList != null && Model.LRDetailList.Count() > 0)
            {
                foreach (var a in Model.LRDetailList.Where(x => x.tempId == Model.tempId))
                {
                    Model.ExpAcount = a.ExpAcount;
                    Model.ExpAcountName = a.ExpAcountName;
                    Model.tempId = a.tempId;
                    Model.ExpAmount = a.ExpAmount;
                    Model.SubHeadAcc = a.SubHeadAcc;
                }
            }

            var html = ViewHelper.RenderPartialView(this, "ConsignmentExpGridList", Model);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        #endregion



        #region SaveData And Delete
        public void DeUpdate(LRVM Model)
        {
            LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == Model.Document).FirstOrDefault();
            if (lRMaster != null)
            {
                var mobj = ctxTFAT.LrRelatedExp.Where(x => x.Parentkey == lRMaster.ParentKey).ToList();
                if (mobj != null)
                {
                    ctxTFAT.LrRelatedExp.RemoveRange(mobj);
                }
                ctxTFAT.SaveChanges();
            }
        }

        public ActionResult SaveData(LRVM mModel)
        {

            LRVM Model = mModel;
            Model.LRSetup = ctxTFAT.LRSetup.Where(x => (x.RECORDKEY == 1)).FirstOrDefault();
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var MaxDecValue = 999999999999;
                    if (MaxDecValue < mModel.DecVal)
                    {
                        transaction.Rollback();
                        return Json(new { Message = "Declare value not allow Greater than 999999999999.", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                    }
                    if (mModel.Document != null)
                    {
                        var OpenLR = ctxTFAT.OpeningLrMaster.Where(x => x.LRNO.ToString() == mModel.Document).FirstOrDefault();
                        if (OpenLR != null)
                        {
                            transaction.Rollback();
                            return Json(new { Message = "Cannot Change LR Details Here Because Of  This Is Opening LR.", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (mModel.Mode == "Delete")
                    {
                        var mlist = ctxTFAT.LRMaster.Where(x => (x.TableKey.ToString() == mModel.Document)).FirstOrDefault();
                        if (mlist != null)
                        {
                            if (ctxTFAT.TripFmList.Where(x => x.RefTablekey == mlist.TableKey.ToString()).ToList().Count() > 0)
                            {
                                transaction.Rollback();
                                return Json(new { Message = "Trip Generated Of This LR So We Can Not Delete .........", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                            }
                        }

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
                    else
                    {
                        if (mModel.LRtype.Trim() == "100004")
                        {
                            var GetLcList = ctxTFAT.LCDetail.Where(x => x.LRRefTablekey == mModel.Document).ToList();
                            if (GetLcList != null)
                            {
                                if (GetLcList.Count() > 0)
                                {
                                    return Json(new { Status = "Error", Message = "Not Allow To Cancel Consignment.\n This Consignment Lorry Challan Generated...!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                        if (mModel.TotQty == 0 && mModel.LRtype.Trim() != "100004")
                        {
                            transaction.Rollback();
                            return Json(new { Message = "Package Required........", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                        }
                        if (mModel.ChgType == "100000")
                        {
                            if (mModel.ActWt == 0)
                            {
                                transaction.Rollback();
                                return Json(new { Message = "Actual Weight Required........", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                            }
                            if (mModel.ChgWt == 0)
                            {
                                transaction.Rollback();
                                return Json(new { Message = "Charge Weight Required........", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                            }
                            if (mModel.ActWt > mModel.ChgWt)
                            {
                                transaction.Rollback();
                                return Json(new { Message = "Actual_Weight Should Be Less Than Or Equal To Charged_Weight .........", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                            }
                        }

                        DeliveryMaster delivery = ctxTFAT.DeliveryMaster.Where(x => x.ParentKey.ToString() == mModel.Document).FirstOrDefault();
                        if (delivery != null)
                        {
                            if (!(delivery.DeliveryDate >= ConvertDDMMYYTOYYMMDD(mModel.BookDate)))
                            {
                                transaction.Rollback();
                                return Json(new { Message = "This Consignment Delivery Date Is :-" + delivery.DeliveryDate.ToShortDateString() + " .\n So Your Consignment Booking Date Should Be Less Than Delivery Date. ", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    bool mAdd = true;
                    LRMaster lRMaster = new LRMaster();
                    LRStock lRStock = new LRStock();

                    //LocalPickUpSheet localPickUpSheet = new LocalPickUpSheet();
                    if (ctxTFAT.LRMaster.Where(x => (x.TableKey.ToString() == mModel.Document)).FirstOrDefault() != null)
                    {
                        lRMaster = ctxTFAT.LRMaster.Where(x => (x.TableKey.ToString() == mModel.Document)).FirstOrDefault();
                        lRStock = ctxTFAT.LRStock.Where(x => (x.ParentKey.ToString() == mModel.Document)).FirstOrDefault();
                        mAdd = false;
                        mdocument = lRMaster.TableKey.ToString();
                        DeUpdate(mModel);
                    }

                    if (mAdd == false)
                    {
                        if (mbranchcode != lRMaster.Branch)
                        {
                            transaction.Rollback();
                            return Json(new { Message = "Login Branch Does Not Match With Documnet Branch.", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    if (mAdd)
                    {
                        lRMaster.LrNo = mModel.LrGenerate == "A" ? GetNewCode() : Convert.ToInt32(mModel.LrNo);
                        lRMaster.CreateDate = mModel.DocDate;
                        lRMaster.LoginBranch = mbranchcode;
                        lRStock.LoginBranch = mbranchcode;
                        lRStock.LrNo = lRMaster.LrNo;
                        lRMaster.LrGenerate = mModel.LrGenerate;
                        lRMaster.Branch = mbranchcode;
                        lRStock.Branch = mbranchcode;
                        lRMaster.ParentKey = mbranchcode + "LR000" + mperiod.Substring(0, 2) + lRMaster.LrNo;
                        lRMaster.TableKey = mbranchcode + "LR000" + mperiod.Substring(0, 2) + 1.ToString("D3") + lRMaster.LrNo;
                        lRStock.ParentKey = lRMaster.TableKey;
                        lRStock.TableKey = mbranchcode + "STK00" + mperiod.Substring(0, 2) + 1.ToString("D3") + lRMaster.LrNo;
                        mdocument = lRMaster.TableKey.ToString();
                    }

                    //PickOrder
                    if (!String.IsNullOrEmpty(mModel.DocNo))
                    {
                        var Qty = 0;
                        PickOrder pickOrder = ctxTFAT.PickOrder.Where(x => x.TableKey.ToString().Trim() == mModel.DocNo.Trim()).FirstOrDefault();
                        if (pickOrder != null)
                        {
                            List<string> PickLrno = new List<string>();
                            List<string> PickQty = new List<string>();

                            if (lRMaster.TotQty != mModel.TotQty)
                            {
                                if (lRMaster.TotQty > mModel.TotQty)
                                {
                                    //means We Reduce The Qty
                                    Qty = lRMaster.TotQty - mModel.TotQty;
                                    pickOrder.BalQty += Qty;
                                }
                                else
                                {
                                    //means We Increase The Qty
                                    Qty = mModel.TotQty - lRMaster.TotQty;
                                    pickOrder.BalQty -= Qty;
                                }

                                if (String.IsNullOrEmpty(pickOrder.ReferLR))
                                {
                                    pickOrder.ReferLR = lRMaster.LrNo.ToString().Trim() + ":" + mModel.TotQty;
                                }
                                else
                                {
                                    var SpliData = pickOrder.ReferLR.Split('^');
                                    foreach (var item in SpliData)
                                    {
                                        var SPlitLR = item.Split(':');
                                        PickLrno.Add(SPlitLR[0].ToString().Trim());
                                        PickQty.Add(SPlitLR[1].ToString().Trim());
                                    }

                                    var Index = PickLrno.IndexOf(mModel.LrNo.ToString().Trim());
                                    if (Index == -1)
                                    {
                                        PickLrno.Add(mModel.LrNo.ToString().Trim());
                                        PickQty.Add(mModel.TotQty.ToString().Trim());
                                    }
                                    else
                                    {
                                        PickQty[Index] = mModel.TotQty.ToString();
                                    }
                                    string NreRelLR = "";
                                    for (int i = 0; i < PickLrno.Count(); i++)
                                    {
                                        NreRelLR += PickLrno[i] + ":" + PickQty[i] + "^";
                                    }
                                    NreRelLR = NreRelLR.Substring(0, NreRelLR.Length - 1);
                                    pickOrder.ReferLR = NreRelLR;
                                }
                            }

                            ctxTFAT.Entry(pickOrder).State = EntityState.Modified;

                        }
                    }

                    //LRMaster
                    {
                        TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.Source).FirstOrDefault();

                        lRMaster.BookDate = ConvertDDMMYYTOYYMMDD(mModel.BookDate);
                        if (string.IsNullOrEmpty(mModel.OrderReceivedDate))
                        {
                            lRMaster.OrderReceivedDate = null;
                        }
                        else
                        {
                            lRMaster.OrderReceivedDate = ConvertDDMMYYTOYYMMDD(mModel.OrderReceivedDate);
                        }

                        if (string.IsNullOrEmpty(mModel.DateOfOrder))
                        {
                            lRMaster.DateOfOrder = null;
                        }
                        else
                        {
                            lRMaster.DateOfOrder = ConvertDDMMYYTOYYMMDD(mModel.DateOfOrder);
                        }

                        if (string.IsNullOrEmpty(mModel.ScheduleDate))
                        {
                            lRMaster.ScheduleDate = null;
                        }
                        else
                        {
                            lRMaster.ScheduleDate = ConvertDDMMYYTOYYMMDD(mModel.ScheduleDate);
                        }

                        lRMaster.Time = mModel.Time;
                        lRMaster.DocNo = mModel.DocNo;
                        lRMaster.TotQty = mModel.TotQty;
                        lRMaster.BillBran = mModel.BillBran;
                        lRMaster.BillParty = mModel.BillParty;
                        lRMaster.LRtype = mModel.LRtype;
                        lRMaster.ServiceType = mModel.ServiceType;
                        lRMaster.RecCode = mModel.RecCode;
                        lRMaster.SendCode = mModel.SendCode;
                        lRMaster.Source = mModel.Source;
                        lRMaster.Dest = mModel.Dest;
                        lRMaster.PartyRef = mModel.PartyRef;
                        lRMaster.PartyInvoice = mModel.PartyInvoice;
                        lRMaster.GSTNO = mModel.GSTNO;
                        lRMaster.EwayBill = mModel.EwayBill;
                        lRMaster.VehicleNo = mModel.VehicleNo;
                        lRMaster.ActWt = mModel.ActWt;
                        lRMaster.ChgWt = mModel.ChgWt;
                        lRMaster.Amt = mModel.Amt;
                        lRMaster.LRMode = mModel.LRMode;
                        lRMaster.RecGST = mModel.RecGST;
                        lRMaster.SendGST = mModel.SendGST;
                        lRMaster.BillGST = mModel.BillGST;
                        lRMaster.TrnMode = mModel.TrnMode;
                        lRMaster.HSNCODE = mModel.HSNCODE;

                        lRMaster.Val1 = mModel.Charges.Where(x => x.Fld == "F001").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F001") : 0;
                        lRMaster.Val2 = mModel.Charges.Where(x => x.Fld == "F002").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F002") : 0;
                        lRMaster.Val3 = mModel.Charges.Where(x => x.Fld == "F003").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F003") : 0;
                        lRMaster.Val4 = mModel.Charges.Where(x => x.Fld == "F004").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F004") : 0;
                        lRMaster.Val5 = mModel.Charges.Where(x => x.Fld == "F005").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F005") : 0;
                        lRMaster.Val6 = mModel.Charges.Where(x => x.Fld == "F006").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F006") : 0;
                        lRMaster.Val7 = mModel.Charges.Where(x => x.Fld == "F007").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F007") : 0;
                        lRMaster.Val8 = mModel.Charges.Where(x => x.Fld == "F008").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F008") : 0;
                        lRMaster.Val9 = mModel.Charges.Where(x => x.Fld == "F009").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F009") : 0;
                        lRMaster.Val10 = mModel.Charges.Where(x => x.Fld == "F010").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F010") : 0;
                        lRMaster.Val11 = mModel.Charges.Where(x => x.Fld == "F011").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F011") : 0;
                        lRMaster.Val12 = mModel.Charges.Where(x => x.Fld == "F012").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F012") : 0;
                        lRMaster.Val13 = mModel.Charges.Where(x => x.Fld == "F013").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F013") : 0;
                        lRMaster.Val14 = mModel.Charges.Where(x => x.Fld == "F014").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F014") : 0;
                        lRMaster.Val15 = mModel.Charges.Where(x => x.Fld == "F015").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F015") : 0;
                        lRMaster.Val16 = mModel.Charges.Where(x => x.Fld == "F016").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F016") : 0;
                        lRMaster.Val17 = mModel.Charges.Where(x => x.Fld == "F017").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F017") : 0;
                        lRMaster.Val18 = mModel.Charges.Where(x => x.Fld == "F018").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F018") : 0;
                        lRMaster.Val19 = mModel.Charges.Where(x => x.Fld == "F019").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F019") : 0;
                        lRMaster.Val20 = mModel.Charges.Where(x => x.Fld == "F020").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F020") : 0;
                        lRMaster.Val21 = mModel.Charges.Where(x => x.Fld == "F021").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F021") : 0;
                        lRMaster.Val22 = mModel.Charges.Where(x => x.Fld == "F022").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F022") : 0;
                        lRMaster.Val23 = mModel.Charges.Where(x => x.Fld == "F023").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F023") : 0;
                        lRMaster.Val24 = mModel.Charges.Where(x => x.Fld == "F024").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F024") : 0;
                        lRMaster.Val25 = mModel.Charges.Where(x => x.Fld == "F025").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F025") : 0;

                        lRMaster.DecVal = mModel.DecVal;
                        lRMaster.DescrType = mModel.DescrType;
                        lRMaster.ChgType = mModel.ChgType;
                        lRMaster.UnitCode = mModel.UnitCode;
                        lRMaster.Colln = mModel.Colln;
                        lRMaster.Delivery = mModel.Delivery;
                        lRMaster.FormNo = mModel.FormNo;
                        lRMaster.TransactionAt = mModel.TransactionAt;
                        lRMaster.GstLiable = mModel.GstLiable;
                        lRMaster.Narr = mModel.Narr;

                        lRMaster.TrType = "SLR";
                        lRMaster.ConsignerInfo = mModel.ConsignerEXTRAInfo == null ? "" : mModel.ConsignerEXTRAInfo;
                        lRMaster.ConsigneeInfo = mModel.ConsigneeEXTRAInfo == null ? "" : mModel.ConsigneeEXTRAInfo;
                        lRMaster.BillingPartyInfo = mModel.BillingPartyEXTRAInfo == null ? "" : mModel.BillingPartyEXTRAInfo;
                        lRMaster.DispachLC = mModel.DispachLC;
                        lRMaster.DispachFM = mModel.DispachFM;
                        lRMaster.ENTEREDBY = muserid;
                        lRMaster.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                        lRMaster.AUTHORISE = mauthorise;
                        lRMaster.AUTHIDS = muserid;
                        lRMaster.POD = true;
                        lRMaster.Crossing = mModel.Crossing;

                        lRMaster.BENumber = mModel.BENumber;
                        lRMaster.PONumber = mModel.PONumber;

                        TempData["LR000PreviousRateChrgOn"] = Model.RateChrgOn;
                        TempData["LR000PreviousRateType"] = Model.RateType;
                        lRMaster.RateChrgOn = Model.RateChrgOn;
                        lRMaster.Rate = Model.Rate;
                        lRMaster.RateType = Model.RateType;

                        lRMaster.ConsignerAddNo = Model.ConsignerAddNo;
                        lRMaster.ConsigneeAddNo = Model.ConsigneeAddNo;
                        lRMaster.BillPartyAddNo = Model.BillPartyAddNo;

                        if (mModel.LRtype.Trim() != "100003")
                        {
                            lRMaster.UnbillQty = lRMaster.TotQty;
                        }
                        else
                        {
                            lRMaster.UnbillQty = 0;
                        }

                        lRMaster.InfoContactNO = mModel.LrContactPersonNo;
                        lRMaster.InfoEmailId = mModel.LrContactPersonEmailId;
                        lRMaster.InfoName = mModel.LrContactPerson;
                        lRMaster.DieselLtr = Model.DieselLtr;
                        lRMaster.ApprovedDiesel = Model.ApprovedDiesel;
                        lRMaster.PendingDiesel = Model.PendingDiesel;
                        lRMaster.AdvanceDiesel = Model.AdvanceDiesel;
                        lRMaster.TripNarr = Model.TripNarr;
                        lRMaster.TripNo = Model.TripNo;
                        lRMaster.DieselAmt = Model.DieselAmt;

                        lRMaster.Driver = Model.Driver;
                        lRMaster.DriverTripExp = Model.DriverTripExp == null ? 0 : Model.DriverTripExp;
                        lRMaster.ActualExp = Model.ActualExp;
                        lRMaster.StockAt = Model.StockAt;

                        if (mModel.LRDetailList != null)
                        {
                            foreach (var item in mModel.LRDetailList)
                            {
                                LrRelatedExp lrRelatedExp = new LrRelatedExp();
                                lrRelatedExp.LRNO = lRMaster.LrNo;
                                lrRelatedExp.ExpAcount = item.ExpAcount;
                                lrRelatedExp.SubHeadAcc = item.SubHeadAcc;
                                lrRelatedExp.Amount = item.ExpAmount;
                                lrRelatedExp.Parentkey = lRMaster.ParentKey;
                                lrRelatedExp.Tablekey = mbranchcode + "LR000" + mperiod.Substring(0, 2) + item.tempId.ToString("D3") + lRMaster.LrNo;
                                lrRelatedExp.ENTEREDBY = muserid;
                                lrRelatedExp.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                                lrRelatedExp.AUTHORISE = "A00";
                                lrRelatedExp.AUTHIDS = muserid;
                                ctxTFAT.LrRelatedExp.Add(lrRelatedExp);
                            }
                        }
                    }
                    //LRStock
                    {


                        lRStock.Date = ConvertDDMMYYTOYYMMDD(mModel.BookDate);
                        lRStock.Time = mModel.Time;
                        lRStock.Type = lRMaster.StockAt == "Godown" ? "LR" : "TRN";
                        lRStock.TotalQty = mModel.TotQty;
                        lRStock.BalQty = mModel.TotQty;
                        lRStock.AllocatBalQty = mModel.TotQty;
                        lRStock.AllocatBalWght = mModel.ActWt;
                        lRStock.BalWeight = mModel.ActWt;
                        lRStock.ChrgWeight = mModel.ChgWt;
                        lRStock.ActWeight = mModel.ActWt;
                        lRStock.ChrgType = mModel.ChgType;
                        lRStock.Description = mModel.DescrType;
                        lRStock.Unit = mModel.UnitCode;
                        lRStock.FromBranch = mModel.Source;
                        lRStock.ToBranch = mModel.Dest;
                        lRStock.Consigner = mModel.RecCode;
                        lRStock.Consignee = mModel.SendCode;
                        lRStock.LrType = mModel.LRtype;
                        lRStock.Coln = lRMaster.Colln;
                        lRStock.LRMode = lRMaster.LRMode;
                        lRStock.Delivery = lRMaster.Delivery;

                        if (mModel.Colln == "G")
                        {
                            lRStock.StockAt = "Godown";
                            lRStock.StockStatus = "G";
                        }
                        else
                        {
                            lRStock.StockAt = "Pick";
                            lRStock.StockStatus = "P";
                        }

                        lRStock.Crossing = mModel.Crossing;
                        lRStock.Remark = mModel.Narr;
                        lRStock.ENTEREDBY = muserid;
                        lRStock.AUTHIDS = muserid;
                        lRStock.AUTHORISE = mAUTHORISE;
                        lRStock.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    }

                    #region Authorisation
                    string Athorise = "A00";
                    TfatUserAuditHeader authorisation = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == "LR000").FirstOrDefault();
                    if (authorisation != null)
                    {
                        Athorise = SetAuthorisationLogistics(authorisation, lRMaster.TableKey, lRMaster.LrNo.ToString(), 0, lRMaster.BookDate.ToShortDateString(), lRMaster.Amt, lRMaster.BillParty, mbranchcode);
                        lRStock.AUTHORISE = Athorise;
                        lRMaster.AUTHORISE = Athorise;
                    }
                    #endregion

                    if (mModel.Mode == "Add")
                    {
                        lRMaster.Prefix = mperiod;
                        lRStock.Prefix = mperiod;
                        lRStock.LRRefTablekey = lRMaster.TableKey;
                        ctxTFAT.LRMaster.Add(lRMaster);
                        ctxTFAT.LRStock.Add(lRStock);

                        AttachmentVM vM = new AttachmentVM();
                        vM.ParentKey = lRMaster.TableKey;
                        vM.Srl = lRMaster.LrNo.ToString();
                        vM.Type = "LR000";
                        SaveAttachment(vM);

                        SaveNarrationAdd(lRMaster.LrNo.ToString(), lRMaster.TableKey);
                    }
                    else
                    {
                        ctxTFAT.Entry(lRMaster).State = EntityState.Modified;
                        ctxTFAT.Entry(lRStock).State = EntityState.Modified;
                    }

                    //SavetFatEWB(lRMaster);
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    //ShruhamSendUsersMsg(mModel.Mode, lRMaster.Val1, lRMaster.Branch + lRMaster.ParentKey, lRMaster.BookDate, lRMaster.BillParty, "CA");
                    //ShruhamSendPartywiseSMS(mModel.Mode, lRMaster.Val1, lRMaster.Branch + lRMaster.ParentKey, lRMaster.BookDate, lRMaster.BillParty, "CA");
                    SendSMS_MSG_Email(mModel.Mode, lRMaster.Val1, lRMaster.Branch + lRMaster.ParentKey, lRMaster.BookDate, lRMaster.BillParty, "CA");
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, lRMaster.ParentKey, lRMaster.BookDate, 0, lRMaster.BillParty, "Save Lorry Receipt :" + lRMaster.LrNo, "CA");
                    ConsignmentNotification(lRMaster);
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.InnerException.Message, Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { SerialNo = mdocument, Status = "Success", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveDraft(LRVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    if (mModel.Mode == "Delete")
                    {
                        DeleteDraft(mModel.Document);
                        transaction.Commit();
                        transaction.Dispose();
                        //GridOption Model = TempData.Peek("StoreGridModalForDraft") as GridOption;
                        //return RedirectToAction("LorryDraft",Model);
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    bool mAdd = true;

                    LRMasterDraft lRMaster_Draft = new LRMasterDraft();
                    if (ctxTFAT.LRMasterDraft.Where(x => (x.LrNo.ToString() == mModel.Document)).FirstOrDefault() != null)
                    {
                        lRMaster_Draft = ctxTFAT.LRMasterDraft.Where(x => (x.LrNo.ToString() == mModel.Document)).FirstOrDefault();
                        mAdd = false;
                    }

                    if (mAdd)
                    {
                        lRMaster_Draft.LrNo = GetNewCodeDraft();
                    }

                    //LRMaster
                    {

                        lRMaster_Draft.DraftName = mModel.Draft_Name;
                        lRMaster_Draft.LoginBranch = mbranchcode;
                        lRMaster_Draft.Branch = mbranchcode;
                        lRMaster_Draft.BookDate = ConvertDDMMYYTOYYMMDD(mModel.BookDate.ToString());
                        lRMaster_Draft.CreateDate = DateTime.Now;
                        lRMaster_Draft.Time = mModel.Time;
                        lRMaster_Draft.DocNo = mModel.DocNo;
                        lRMaster_Draft.TotQty = mModel.TotQty;
                        lRMaster_Draft.BillBran = mModel.BillBran;
                        lRMaster_Draft.BillParty = mModel.BillParty;
                        lRMaster_Draft.LRtype = mModel.LRtype;
                        lRMaster_Draft.ServiceType = mModel.ServiceType;
                        lRMaster_Draft.RecCode = mModel.RecCode;
                        lRMaster_Draft.SendCode = mModel.SendCode;
                        lRMaster_Draft.Source = mModel.Source;
                        lRMaster_Draft.Dest = mModel.Dest;
                        lRMaster_Draft.PartyRef = mModel.PartyRef;
                        lRMaster_Draft.PartyInvoice = mModel.PartyInvoice;
                        lRMaster_Draft.GSTNO = mModel.GSTNO;
                        lRMaster_Draft.EwayBill = mModel.EwayBill;
                        lRMaster_Draft.VehicleNo = mModel.VehicleNo;
                        lRMaster_Draft.ActWt = mModel.ActWt;
                        lRMaster_Draft.ChgWt = mModel.ChgWt;
                        lRMaster_Draft.Amt = mModel.Amt;
                        lRMaster_Draft.DecVal = mModel.DecVal == null ? 0 : mModel.DecVal;
                        lRMaster_Draft.DescrType = mModel.DescrType;
                        lRMaster_Draft.ChgType = mModel.ChgType;
                        lRMaster_Draft.UnitCode = mModel.UnitCode;
                        lRMaster_Draft.Colln = mModel.Colln;
                        lRMaster_Draft.Delivery = mModel.Delivery;
                        //lRMaster_Draft.DeliveryAt = mModel.DeliveryAt;
                        //lRMaster_Draft.DeliveryTxt = mModel.DeliveryTxt;
                        lRMaster_Draft.FormNo = mModel.FormNo;
                        lRMaster_Draft.TransactionAt = mModel.TransactionAt;
                        lRMaster_Draft.GstLiable = mModel.GstLiable;
                        lRMaster_Draft.Narr = mModel.Narr;
                        lRMaster_Draft.Prefix = mModel.Prefix;
                        lRMaster_Draft.TrType = "SLR";
                        lRMaster_Draft.ConsignerInfo = mModel.ConsignerEXTRAInfo;
                        lRMaster_Draft.ConsigneeInfo = mModel.ConsigneeEXTRAInfo;
                        lRMaster_Draft.BillingPartyInfo = mModel.BillingPartyEXTRAInfo;
                        lRMaster_Draft.ENTEREDBY = muserid;
                        lRMaster_Draft.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                        lRMaster_Draft.AUTHORISE = muserid.Substring(0, 3);
                        lRMaster_Draft.AUTHIDS = muserid;
                        lRMaster_Draft.LRMode = mModel.LRMode;
                        lRMaster_Draft.BENumber = mModel.BENumber;
                        lRMaster_Draft.PONumber = mModel.PONumber;

                        //string[] ChargeValue = mModel.Val1.ToString().Split(',');
                        //string[] FldValue = mModel.FldValue.ToString().Split(',');
                        //for (int i = 0; i < ChargeValue.Length; i++)
                        //{
                        //    var Fld = FldValue[i].Substring(2, FldValue[i].Length - 2);
                        //    var value = "Val" + Convert.ToInt32(Fld);
                        //    var area = Convert.ToDecimal(ChargeValue[i]);
                        //    typeof(LRMasterDraft).GetProperty(value).SetValue(lRMaster_Draft, area, null);
                        //}
                        lRMaster_Draft.Val1 = mModel.Charges.Where(x => x.Fld == "F001").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F001") : 0;
                        lRMaster_Draft.Val2 = mModel.Charges.Where(x => x.Fld == "F002").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F002") : 0;
                        lRMaster_Draft.Val3 = mModel.Charges.Where(x => x.Fld == "F003").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F003") : 0;
                        lRMaster_Draft.Val4 = mModel.Charges.Where(x => x.Fld == "F004").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F004") : 0;
                        lRMaster_Draft.Val5 = mModel.Charges.Where(x => x.Fld == "F005").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F005") : 0;
                        lRMaster_Draft.Val6 = mModel.Charges.Where(x => x.Fld == "F006").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F006") : 0;
                        lRMaster_Draft.Val7 = mModel.Charges.Where(x => x.Fld == "F007").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F007") : 0;
                        lRMaster_Draft.Val8 = mModel.Charges.Where(x => x.Fld == "F008").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F008") : 0;
                        lRMaster_Draft.Val9 = mModel.Charges.Where(x => x.Fld == "F009").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F009") : 0;
                        lRMaster_Draft.Val10 = mModel.Charges.Where(x => x.Fld == "F010").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F010") : 0;
                        lRMaster_Draft.Val11 = mModel.Charges.Where(x => x.Fld == "F011").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F011") : 0;
                        lRMaster_Draft.Val12 = mModel.Charges.Where(x => x.Fld == "F012").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F012") : 0;
                        lRMaster_Draft.Val13 = mModel.Charges.Where(x => x.Fld == "F013").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F013") : 0;
                        lRMaster_Draft.Val14 = mModel.Charges.Where(x => x.Fld == "F014").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F014") : 0;
                        lRMaster_Draft.Val15 = mModel.Charges.Where(x => x.Fld == "F015").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F015") : 0;
                        lRMaster_Draft.Val16 = mModel.Charges.Where(x => x.Fld == "F016").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F016") : 0;
                        lRMaster_Draft.Val17 = mModel.Charges.Where(x => x.Fld == "F017").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F017") : 0;
                        lRMaster_Draft.Val18 = mModel.Charges.Where(x => x.Fld == "F018").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F018") : 0;
                        lRMaster_Draft.Val19 = mModel.Charges.Where(x => x.Fld == "F019").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F019") : 0;
                        lRMaster_Draft.Val20 = mModel.Charges.Where(x => x.Fld == "F020").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F020") : 0;
                        lRMaster_Draft.Val21 = mModel.Charges.Where(x => x.Fld == "F021").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F021") : 0;
                        lRMaster_Draft.Val22 = mModel.Charges.Where(x => x.Fld == "F022").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F022") : 0;
                        lRMaster_Draft.Val23 = mModel.Charges.Where(x => x.Fld == "F023").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F023") : 0;
                        lRMaster_Draft.Val24 = mModel.Charges.Where(x => x.Fld == "F024").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F024") : 0;
                        lRMaster_Draft.Val25 = mModel.Charges.Where(x => x.Fld == "F025").Select(x => x) != null ? GetChargesVal(mModel.Charges, " F025") : 0;

                    }
                    if (mAdd == true)
                    {
                        ctxTFAT.LRMasterDraft.Add(lRMaster_Draft);
                    }
                    else
                    {
                        ctxTFAT.Entry(lRMaster_Draft).State = EntityState.Modified;
                    }

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, "LRMaster_Draft", "", DateTime.Now, 0, "", "", "NA");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "LRMaster_Draft" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "LRMaster_Draft" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "LRMaster_Draft" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Status = "Success", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
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
                item.Prefix = mperiod;
                ctxTFAT.AlertNoteMaster.Add(item);
                ++Sno;
            }
        }

        public void SavetFatEWB(LRMaster mModel)
        {
            if (!String.IsNullOrEmpty(mModel.EwayBill))
            {
                tfatEWB tfatEWB = ctxTFAT.tfatEWB.Where(x => x.DocType == "LR000" && (x.LrTablekey == mModel.TableKey || x.EWBNO == mModel.EwayBill)).FirstOrDefault();
                if (tfatEWB != null)
                {
                    if (!String.IsNullOrEmpty(tfatEWB.Consignment))
                    {
                        tfatEWB.Consignment = mModel.LrNo.ToString();
                    }
                    if (!String.IsNullOrEmpty(tfatEWB.DocNo))
                    {
                        if (!String.IsNullOrEmpty(mModel.PartyInvoice))
                        {
                            tfatEWB.DocNo = mModel.PartyInvoice.ToString();
                        }
                        else if (!String.IsNullOrEmpty(mModel.BENumber))
                        {
                            tfatEWB.DocNo = mModel.BENumber.ToString();
                        }
                    }
                    if (!String.IsNullOrEmpty(tfatEWB.EWBNO))
                    {
                        if (!String.IsNullOrEmpty(mModel.EwayBill))
                        {
                            tfatEWB.EWBNO = mModel.EwayBill.ToString();
                        }
                    }
                    if (!String.IsNullOrEmpty(tfatEWB.LrTablekey))
                    {
                        tfatEWB.LrTablekey = mModel.TableKey.ToString();
                    }
                    ctxTFAT.Entry(tfatEWB).State = EntityState.Modified;
                }
            }
            else if (!String.IsNullOrEmpty(mModel.PartyInvoice))
            {
                tfatEWB tfatEWB = ctxTFAT.tfatEWB.Where(x => x.DocType == "LR000" && x.DocNo == mModel.PartyInvoice).FirstOrDefault();
                if (tfatEWB != null)
                {
                    if (!String.IsNullOrEmpty(tfatEWB.Consignment))
                    {
                        tfatEWB.Consignment = mModel.LrNo.ToString();
                    }
                    if (!String.IsNullOrEmpty(tfatEWB.DocNo))
                    {
                        if (!String.IsNullOrEmpty(mModel.PartyInvoice))
                        {
                            tfatEWB.DocNo = mModel.PartyInvoice.ToString();
                        }
                        else if (!String.IsNullOrEmpty(mModel.BENumber))
                        {
                            tfatEWB.DocNo = mModel.BENumber.ToString();
                        }
                    }
                    if (!String.IsNullOrEmpty(tfatEWB.EWBNO))
                    {
                        if (!String.IsNullOrEmpty(mModel.EwayBill))
                        {
                            tfatEWB.EWBNO = mModel.EwayBill.ToString();
                        }
                    }
                    if (!String.IsNullOrEmpty(tfatEWB.LrTablekey))
                    {
                        tfatEWB.LrTablekey = mModel.TableKey.ToString();
                    }
                    ctxTFAT.Entry(tfatEWB).State = EntityState.Modified;
                }
            }
            else if (!String.IsNullOrEmpty(mModel.BENumber))
            {
                tfatEWB tfatEWB = ctxTFAT.tfatEWB.Where(x => x.DocType == "LR000" && x.DocNo == mModel.BENumber).FirstOrDefault();
                if (tfatEWB != null)
                {
                    if (!String.IsNullOrEmpty(tfatEWB.Consignment))
                    {
                        tfatEWB.Consignment = mModel.LrNo.ToString();
                    }
                    if (!String.IsNullOrEmpty(tfatEWB.DocNo))
                    {
                        if (!String.IsNullOrEmpty(mModel.PartyInvoice))
                        {
                            tfatEWB.DocNo = mModel.PartyInvoice.ToString();
                        }
                        else if (!String.IsNullOrEmpty(mModel.BENumber))
                        {
                            tfatEWB.DocNo = mModel.BENumber.ToString();
                        }
                    }
                    if (!String.IsNullOrEmpty(tfatEWB.EWBNO))
                    {
                        if (!String.IsNullOrEmpty(mModel.EwayBill))
                        {
                            tfatEWB.EWBNO = mModel.EwayBill.ToString();
                        }
                    }
                    if (!String.IsNullOrEmpty(tfatEWB.LrTablekey))
                    {
                        tfatEWB.LrTablekey = mModel.TableKey.ToString();
                    }
                    ctxTFAT.Entry(tfatEWB).State = EntityState.Modified;
                }
            }
        }

        #endregion SaveData

        #region Delete

        public string DeleteStateMaster(LRVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return "Code not Entered..";
            }
            var mList = ctxTFAT.LRMaster.Where(x => (x.TableKey.ToString() == mModel.Document)).FirstOrDefault();
            var lRStock = ctxTFAT.LRStock.Where(x => (x.ParentKey.ToString() == mModel.Document) && x.Branch == mList.Branch).FirstOrDefault();

            if (mModel.Document != null)
            {
                var FmRelation = ctxTFAT.LCDetail.Where(x => x.LRRefTablekey.ToString() == mModel.Document).FirstOrDefault();
                if (FmRelation != null)
                {
                    return "Cannot Delete LR Details Because Of In This LR  Load It In LC.";
                }
                if (ctxTFAT.LRStock.Where(x => x.LRRefTablekey == mModel.Document).ToList().Count() > 1)
                {
                    return "\nThis Consignment Stock Used It....!";
                }
                if (ctxTFAT.PeriodLock.Where(x => x.Branch == mList.Branch && x.LockDate == mList.BookDate && x.Type == "LR000").FirstOrDefault() != null)
                {
                    return "Cannot Delete LR  Because Of In This LR  Date Lock By Period Lock System..!";
                }
            }
            var Delivery = ctxTFAT.DeliveryMaster.Where(x => x.ParentKey.ToString() == mModel.Document).FirstOrDefault();
            if (Delivery != null)
            {
                return "Found Delivery Please Remove Delivery First....";
            }
            var POD = ctxTFAT.PODRel.Where(x => x.LRRefTablekey.ToString() == mModel.Document).FirstOrDefault();
            if (POD != null)
            {
                return "Found POD Received Please Remove POD Received First....";
            }
            var Lrbill = ctxTFAT.LRBill.Where(x => x.LRRefTablekey.ToString() == mModel.Document).FirstOrDefault();
            if (Lrbill != null)
            {
                var Billno = ctxTFAT.Sales.Where(x => x.TableKey == Lrbill.ParentKey).FirstOrDefault();
                var Type = ctxTFAT.DocTypes.Where(x => x.Code == Billno.Type).Select(x => x.Name).FirstOrDefault();
                var Branch = ctxTFAT.TfatBranch.Where(x => x.Code == Billno.Branch).Select(x => x.Name).FirstOrDefault();
                return "This Consignment Found In " + Type + "  Entry Branch : " + Branch.ToUpper() + " And Document NO : " + Billno.Srl + " . \nPlease Remove Consignment From The " + Type + " First....";
            }
            var LRExp = ctxTFAT.RelLr.Where(x => x.LRRefTablekey.ToString() == mModel.Document).FirstOrDefault();
            if (LRExp != null)
            {
                var Billno = ctxTFAT.RelateData.Where(x => x.ParentKey == LRExp.ParentKey).FirstOrDefault();
                var Type = ctxTFAT.DocTypes.Where(x => x.Code == Billno.Type).Select(x => x.Name).FirstOrDefault();
                var Branch = ctxTFAT.TfatBranch.Where(x => x.Code == Billno.Branch).Select(x => x.Name).FirstOrDefault();
                return "This Consignment Found In " + Type + "  Entry Branch : " + Branch.ToUpper() + " And " + Billno.Srl + " . \nPlease Remove Consignment From The " + Type + " First....";
            }

            //PickOrder
            if (!String.IsNullOrEmpty(mList.DocNo))
            {
                var Qty = 0;
                PickOrder pickOrder = ctxTFAT.PickOrder.Where(x => x.TableKey.ToString().Trim() == mList.DocNo.Trim()).FirstOrDefault();
                if (pickOrder != null)
                {
                    List<string> PickLrno = new List<string>();
                    List<string> PickQty = new List<string>();


                    pickOrder.BalQty += mList.TotQty;
                    var SpliData = pickOrder.ReferLR.Split('^');
                    foreach (var item in SpliData)
                    {
                        var SPlitLR = item.Split(':');
                        PickLrno.Add(SPlitLR[0].ToString().Trim());
                        PickQty.Add(SPlitLR[1].ToString().Trim());
                    }
                    var Index = PickLrno.IndexOf(mList.LrNo.ToString().Trim());

                    string NreRelLR = "";
                    for (int i = 0; i < PickLrno.Count(); i++)
                    {
                        if (i != Index)
                        {
                            NreRelLR += PickLrno[i] + ":" + PickQty[i] + "^";
                        }

                    }
                    NreRelLR = NreRelLR == "" ? "" : NreRelLR.Substring(0, NreRelLR.Length - 1);
                    pickOrder.ReferLR = NreRelLR;

                    ctxTFAT.Entry(pickOrder).State = EntityState.Modified;

                }
            }


            var LrAttachment = ctxTFAT.Attachment.Where(x => x.Type == "LR000" && x.Srl == mList.LrNo.ToString()).ToList();
            foreach (var item in LrAttachment)
            {
                if (System.IO.File.Exists(item.FilePath))
                {
                    System.IO.File.Delete(item.FilePath);
                }
            }
            ctxTFAT.Attachment.RemoveRange(LrAttachment);

            var GetRemarkDocList = ctxTFAT.Narration.Where(x => x.Srl == mList.LrNo.ToString() && x.Type == "LR000").ToList();
            if (GetRemarkDocList != null)
            {
                ctxTFAT.Narration.RemoveRange(GetRemarkDocList);
            }

            var AuthorisationEntry = ctxTFAT.Authorisation.Where(x => x.Prefix == mperiod && x.Type == "LR000" && x.Srl == mList.LrNo.ToString()).FirstOrDefault();
            if (AuthorisationEntry != null)
            {
                ctxTFAT.Authorisation.Remove(AuthorisationEntry);
            }
            var mobj = ctxTFAT.LrRelatedExp.Where(x => x.Parentkey == mList.ParentKey).ToList();
            if (mobj != null)
            {
                ctxTFAT.LrRelatedExp.RemoveRange(mobj);
            }
            ctxTFAT.LRMaster.Remove(mList);
            ctxTFAT.LRStock.Remove(lRStock);
            ctxTFAT.SaveChanges();
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, mList.ParentKey, mList.BookDate, 0, mList.BillParty, " Delete Lorry Receipt :" + mList.LrNo, "CA");

            return "Success";
        }

        public ActionResult DeleteDraft(string DraftLrNO)
        {
            LRMasterDraft lRMaster_Draft = ctxTFAT.LRMasterDraft.Where(x => x.LrNo.ToString() == DraftLrNO).FirstOrDefault();
            ctxTFAT.LRMasterDraft.Remove(lRMaster_Draft);
            ctxTFAT.SaveChanges();

            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public ActionResult GetConsinorList(bool Consignor)
        {

            List<ConsignerList> consignerLists = new List<ConsignerList>();
            var consignorList = ctxTFAT.Consigner.ToList();
            foreach (var item in consignorList)
            {
                ConsignerList consignerList = new ConsignerList();
                consignerList.BrnachWiseConsigner = false;
                consignerList.CCode = item.Code;
                consignerList.CName = item.Name;
                consignerList.CContactPerson = item.ContactName;
                consignerList.CAddress = item.Addr1 + " " + item.Addr2;
                consignerList.CCity = item.City;
                consignerList.CPincode = item.Pincode.ToString();
                consignerList.CDistirict = item.District;
                consignerList.CPhoneNO = item.ContactNO;
                consignerList.CFax = item.Fax;
                consignerList.Consigner = Consignor;

                consignerLists.Add(consignerList);
            }




            var html = ViewHelper.RenderPartialView(this, "_AllConsignerList", consignerLists);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetInCurrentBranch(string ConsignerCode)
        {
            if (!String.IsNullOrEmpty(ConsignerCode))
            {

                Consigner consigner = ctxTFAT.Consigner.Where(x => x.Code == ConsignerCode).FirstOrDefault();
                consigner.Branch += "," + mbranchcode;

                ctxTFAT.Entry(consigner).State = EntityState.Modified;
                ctxTFAT.SaveChanges();

                return Json(new { Message = "", Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Message = "Not Found", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);

            }
        }

        public void Create()
        {

            string html = TempData.Peek("Design") as string;

            StringReader sr = new StringReader(html.ToString());

            Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
            HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
            MemoryStream pdfStream = new MemoryStream();


            PdfWriter writer = PdfWriter.GetInstance(pdfDoc, pdfStream);
            pdfDoc.Open();

            htmlparser.Parse(sr);
            pdfDoc.Close();
            writer.Close();

            //byte[] bytes = pdfStream.ToArray();
            //pdfStream.Close();

            Response.ClearContent();
            Response.ClearHeaders();

            Response.ContentType = "application/pdf";
            string mfilename = "LorryReceipt" + ".pdf";
            //Response.AppendHeader("Content-Disposition", "attachment; filename=" + mfilename);
            Response.AppendHeader("Content-Disposition", "inline; filename=" + mfilename);
            Response.BinaryWrite(pdfStream.ToArray());
            Response.End();

            //var converter = new HtmlToPdf();
            //converter.Options.DrawBackground = true;
            //converter.Options.EmbedFonts = true;
            //converter.Options.PdfPageSize = PdfPageSize.A4;
            //converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
            //converter.Options.WebPageWidth = 1024;
            //converter.Options.WebPageHeight = 0;
            //converter.Options.CssMediaType = HtmlToPdfCssMediaType.Screen;
            //converter.Options.DisplayFooter = true;

            //var footer = new PdfTextSection(0, 10, "{page_number} / {total_pages}",
            //                                new Font("Arial", 8))
            //{
            //    HorizontalAlign = PdfTextHorizontalAlign.Right
            //};
            //converter.Footer.Add(footer);

            //// Save PDF document (plus optional document information)
            //var doc = converter.ConvertHtmlString(html, baseUrl);
            //doc.DocumentInformation.Author = "Your name";
            //doc.DocumentInformation.CreationDate = DateTime.Now;
            //doc.Save(filename);
            //doc.Close();
        }

        #region Contract Methods

        // ChargeType Wise Contract Methods
        public ActionResult CustomerCharges(LRVM Model)
        {
            bool GeneralContract = false;

            var FromDt = ConvertDDMMYYTOYYMMDD(Model.BookDate);
            ContractList SingleContract = new ContractList();
            List<ContractList> WeightCartoon = new List<ContractList>();
            ConMaster conMaster = new ConMaster();
            if (Model.ChgType == "100000" || Model.ChgType == "100001")
            {
                conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();
                if (conMaster != null)
                {
                    WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                     where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                     select new ContractList()
                                     {
                                         Sno = ConDetail.SrNo,
                                         Service = ConDetail.Services,
                                         TypeOfChrg = ConDetail.WtType,
                                         FromWT = ConDetail.Wtfrom.Value,
                                         ToWT = ConDetail.WtTo.Value,
                                         Rate = (float)ConDetail.Rate,
                                         ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                         ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                         ConDetilsCode = ConDetail.Code,
                                     }).ToList();
                }
                else
                {
                    GeneralContract = true;
                }
                if (WeightCartoon == null || WeightCartoon.Count() == 0)
                {
                    string FromBranch = Model.Source, Tobranch = Model.Dest;
                    TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        FromBranch = tfatBranch.Grp;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }

                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                    if (conMaster != null)
                    {
                        WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                         where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                         select new ContractList()
                                         {
                                             Sno = ConDetail.SrNo,
                                             Service = ConDetail.Services,
                                             TypeOfChrg = ConDetail.WtType,
                                             FromWT = ConDetail.Wtfrom.Value,
                                             ToWT = ConDetail.WtTo.Value,
                                             Rate = (float)ConDetail.Rate,
                                             ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                             ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                             ConDetilsCode = ConDetail.Code,
                                         }).ToList();
                    }

                }

                foreach (var item in WeightCartoon)
                {
                    if (item.Service == "100000")
                    {
                        if (item.ChargeOfChrgWT)
                        {
                            SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ChgWt && x.ToWT >= Model.ChgWt).FirstOrDefault();
                            if (SingleContract != null)
                            {
                                item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                                SingleContract.Charges = item.Charges;
                                break;
                            }
                        }
                        else
                        {
                            SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                            if (SingleContract != null)
                            {
                                item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                                SingleContract.Charges = item.Charges;
                                break;
                            }
                        }
                    }
                    else
                    {
                        SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.TotQty && x.ToWT >= Model.TotQty).FirstOrDefault();
                        if (SingleContract != null)
                        {
                            item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }

                }

                if (SingleContract != null)
                {
                    if (SingleContract.Service != null)
                    {
                        var Rate = SingleContract.Rate;
                        if (SingleContract.ChargeOfChrgWT)
                        {
                            foreach (var item in SingleContract.Charges.ToList())
                            {
                                var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                            }
                        }
                        else
                        {
                            foreach (var item in SingleContract.Charges.ToList())
                            {
                                var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                            }
                        }
                        Model.Charges = SingleContract.Charges;


                    }
                }
            }
            else
            {
                conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();
                if (conMaster != null)
                {
                    SingleContract = (from ConDetail in ctxTFAT.ConDetail
                                      where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                      select new ContractList()
                                      {
                                          Sno = ConDetail.SrNo,
                                          Service = ConDetail.Services,
                                          TypeOfChrg = ConDetail.WtType,
                                          FromWT = ConDetail.Wtfrom.Value,
                                          ToWT = ConDetail.WtTo.Value,
                                          Rate = (float)ConDetail.Rate,
                                          ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                          ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                          ConDetilsCode = ConDetail.Code,
                                      }).FirstOrDefault();
                }

                if (SingleContract.Service == null)
                {
                    string FromBranch = Model.Source, Tobranch = Model.Dest;
                    TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        FromBranch = tfatBranch.Grp;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }

                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                    if (conMaster != null)
                    {
                        SingleContract = (from ConDetail in ctxTFAT.ConDetail
                                          where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                          select new ContractList()
                                          {
                                              Sno = ConDetail.SrNo,
                                              Service = ConDetail.Services,
                                              TypeOfChrg = ConDetail.WtType,
                                              FromWT = ConDetail.Wtfrom.Value,
                                              ToWT = ConDetail.WtTo.Value,
                                              Rate = (float)ConDetail.Rate,
                                              ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                              ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                              ConDetilsCode = ConDetail.Code,
                                          }).FirstOrDefault();
                    }

                }
                if (SingleContract != null)
                {
                    if (SingleContract.Service != null)
                    {
                        SingleContract.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);

                        if (SingleContract.ChargeOfChrgWT)
                        {
                            foreach (var item in SingleContract.Charges.ToList())
                            {
                                var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                            }
                        }
                        else
                        {
                            foreach (var item in SingleContract.Charges.ToList())
                            {
                                var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                            }
                        }
                        Model.Charges = SingleContract.Charges;
                    }
                }
            }


            string Status = "", Message = "", RateChrgOn = "", MRate = "", RateType = "";

            if (Model.ChgType == "100000" || Model.ChgType == "100001")
            {
                if (SingleContract != null)
                {
                    if (SingleContract.Service == null)
                    {
                        if (GeneralContract)
                        {
                            Status = "Error";
                            Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";
                        }
                        else if (Model.ChgType == "100000")
                        {
                            var ConDetail = ctxTFAT.ConDetail.Where(x => x.CustCode == conMaster.Code && x.Services == "100001").ToList();
                            if (ConDetail.Count() > 0)
                            {
                                Status = "OtherFunction";
                                Message = "Not Found Weight Contract...\n Find Carton Rate Accept ?";
                            }
                            else
                            {
                                Status = "Error";
                                Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";
                            }

                        }
                        else
                        {
                            var ConDetail = ctxTFAT.ConDetail.Where(x => x.CustCode == conMaster.Code && x.Services == "100000").ToList();
                            if (ConDetail.Count() > 0)
                            {
                                Status = "OtherFunction";
                                Message = "Not Found Carton Contract...\n Find Weight Rate Accept ?";
                            }
                            else
                            {
                                Status = "Error";
                                Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";
                            }
                        }
                    }
                    else
                    {
                        Status = "Success";
                        RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                        MRate = SingleContract.Rate.ToString();
                        RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

                    }
                }
                else
                {
                    if (GeneralContract)
                    {
                        Status = "Error";
                        Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";

                    }

                    else if (Model.ChgType == "100000")
                    {
                        var ConDetail = ctxTFAT.ConDetail.Where(x => x.CustCode == conMaster.Code && x.Services == "100001").ToList();
                        if (ConDetail.Count() > 0)
                        {
                            Status = "OtherFunction";
                            Message = "Not Found Weight Contract...\n Find Carton Rate Accept ?";
                        }
                        else
                        {
                            Status = "Error";
                            Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";
                        }

                    }
                    else
                    {
                        var ConDetail = ctxTFAT.ConDetail.Where(x => x.CustCode == conMaster.Code && x.Services == "100000").ToList();
                        if (ConDetail.Count() > 0)
                        {
                            Status = "OtherFunction";
                            Message = "Not Found Carton Contract...\n Find Weight Rate Accept ?";
                        }
                        else
                        {
                            Status = "Error";
                            Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";
                        }
                    }
                }

            }
            else
            {
                if (SingleContract != null)
                {
                    if (SingleContract.Service == null)
                    {
                        Status = "Error";
                        Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";
                    }
                    else
                    {
                        Status = "Success";
                        RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                        MRate = SingleContract.Rate.ToString();
                        RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

                    }
                }
                else
                {
                    Status = "Error";
                    Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";
                }
            }

            var html = ViewHelper.RenderPartialView(this, "_Charges", Model);
            var jsonResult = Json(new
            {
                Status = Status,
                Message = Message,
                Html = html,
                RateChrgOn = RateChrgOn,
                MRate = MRate,
                RateType = RateType,

            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public ActionResult CustomerOTherContract(LRVM Model)
        {
            var FromDt = ConvertDDMMYYTOYYMMDD(Model.BookDate);
            ContractList SingleContract = new ContractList();
            List<ContractList> WeightCartoon = new List<ContractList>();
            bool Weight = false; string OthCode = "", OthName = "";
            if (Model.ChgType == "100000")
            {
                Weight = true;
            }
            else
            {
                Weight = false;
            }

            string FromBranch = Model.Source, Tobranch = Model.Dest;
            TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
            {
                FromBranch = tfatBranch.Grp;
            }
            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
            {
                Tobranch = tfatBranch.Grp;
            }

            if (Weight)
            {
                OthCode = "100001";
                OthName = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == OthCode).Select(x => x.ChargeType).FirstOrDefault();
                ConMaster conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();
                if (conMaster != null)
                {
                    WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                     where ConDetail.CustCode == conMaster.Code && ConDetail.Services == "100001"
                                     select new ContractList()
                                     {
                                         Sno = ConDetail.SrNo,
                                         Service = ConDetail.Services,
                                         TypeOfChrg = ConDetail.WtType,
                                         FromWT = ConDetail.Wtfrom.Value,
                                         ToWT = ConDetail.WtTo.Value,
                                         Rate = (float)ConDetail.Rate,
                                         ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                         ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                         ConDetilsCode = ConDetail.Code,
                                     }).ToList();
                }
                if (WeightCartoon == null || WeightCartoon.Count() == 0)
                {
                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                    if (conMaster != null)
                    {
                        WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                         where ConDetail.CustCode == conMaster.Code && ConDetail.Services == "100001"
                                         select new ContractList()
                                         {
                                             Sno = ConDetail.SrNo,
                                             Service = ConDetail.Services,
                                             TypeOfChrg = ConDetail.WtType,
                                             FromWT = ConDetail.Wtfrom.Value,
                                             ToWT = ConDetail.WtTo.Value,
                                             Rate = (float)ConDetail.Rate,
                                             ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                             ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                             ConDetilsCode = ConDetail.Code,
                                         }).ToList();
                    }

                }
            }
            else
            {
                OthCode = "100000";
                OthName = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == OthCode).Select(x => x.ChargeType).FirstOrDefault();
                ConMaster conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();
                if (conMaster != null)
                {
                    WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                     where ConDetail.CustCode == conMaster.Code && ConDetail.Services == "100000"
                                     select new ContractList()
                                     {
                                         Sno = ConDetail.SrNo,
                                         Service = ConDetail.Services,
                                         TypeOfChrg = ConDetail.WtType,
                                         FromWT = ConDetail.Wtfrom.Value,
                                         ToWT = ConDetail.WtTo.Value,
                                         Rate = (float)ConDetail.Rate,
                                         ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                         ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                         ConDetilsCode = ConDetail.Code,
                                     }).ToList();
                }

                if (WeightCartoon == null || WeightCartoon.Count() == 0)
                {
                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                    if (conMaster != null)
                    {
                        WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                         where ConDetail.CustCode == conMaster.Code && ConDetail.Services == "100000"
                                         select new ContractList()
                                         {
                                             Sno = ConDetail.SrNo,
                                             Service = ConDetail.Services,
                                             TypeOfChrg = ConDetail.WtType,
                                             FromWT = ConDetail.Wtfrom.Value,
                                             ToWT = ConDetail.WtTo.Value,
                                             Rate = (float)ConDetail.Rate,
                                             ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                             ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                             ConDetilsCode = ConDetail.Code,
                                         }).ToList();
                    }

                }
            }

            foreach (var item in WeightCartoon)
            {
                if (item.Service == "100000")
                {
                    if (item.ChargeOfChrgWT)
                    {
                        SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ChgWt && x.ToWT >= Model.ChgWt).FirstOrDefault();
                        if (SingleContract != null)
                        {
                            item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }
                    else
                    {
                        SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                        if (SingleContract != null)
                        {
                            item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }
                }
                else
                {
                    SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.TotQty && x.ToWT >= Model.TotQty).FirstOrDefault();
                    if (SingleContract != null)
                    {
                        item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                        SingleContract.Charges = item.Charges;
                        break;
                    }
                }

            }

            if (SingleContract != null)
            {
                if (SingleContract.Service != null)
                {

                    var Rate = SingleContract.Rate;
                    if (SingleContract.ChargeOfChrgWT)
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                        }
                    }
                    else
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                        }
                    }
                    Model.Charges = SingleContract.Charges;


                }
            }


            string Status = "", Message = "", RateChrgOn = "", MRate = "", RateType = "";
            if (SingleContract != null)
            {
                if (SingleContract.Service == null)
                {
                    Status = "Error";
                    Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";
                }
                else
                {
                    Status = "Success";
                    RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                    MRate = SingleContract.Rate.ToString();
                    RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

                }
            }
            else
            {
                Status = "Error";
                Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";
            }


            var html = ViewHelper.RenderPartialView(this, "_Charges", Model);
            var jsonResult = Json(new
            {
                Status = Status,
                Message = Message,
                OthCode = OthCode,
                OthName = OthName,
                Html = html,
                RateChrgOn = RateChrgOn,
                MRate = MRate,
                RateType = RateType,


            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public ActionResult Charges(LRVM Model)
        {
            bool GeneralContract = false;

            var FromDt = ConvertDDMMYYTOYYMMDD(Model.BookDate);
            Model.BillParty = ctxTFAT.CustomerMaster.Where(x => x.Code == Model.BillParty).Select(x => x.AccountParentGroup).FirstOrDefault();
            ContractList SingleContract = new ContractList();
            List<ContractList> WeightCartoon = new List<ContractList>();
            ConMaster conMaster = new ConMaster();
            if (Model.ChgType == "100000" || Model.ChgType == "100001")
            {
                conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();
                if (conMaster != null)
                {
                    WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                     where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                     select new ContractList()
                                     {
                                         Sno = ConDetail.SrNo,
                                         Service = ConDetail.Services,
                                         TypeOfChrg = ConDetail.WtType,
                                         FromWT = ConDetail.Wtfrom.Value,
                                         ToWT = ConDetail.WtTo.Value,
                                         Rate = (float)ConDetail.Rate,
                                         ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                         ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                         ConDetilsCode = ConDetail.Code,
                                     }).ToList();
                }
                else
                {
                    GeneralContract = true;
                }
                if (WeightCartoon == null || WeightCartoon.Count() == 0)
                {
                    string FromBranch = Model.Source, Tobranch = Model.Dest;
                    TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        FromBranch = tfatBranch.Grp;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }

                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                    if (conMaster != null)
                    {
                        WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                         where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                         select new ContractList()
                                         {
                                             Sno = ConDetail.SrNo,
                                             Service = ConDetail.Services,
                                             TypeOfChrg = ConDetail.WtType,
                                             FromWT = ConDetail.Wtfrom.Value,
                                             ToWT = ConDetail.WtTo.Value,
                                             Rate = (float)ConDetail.Rate,
                                             ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                             ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                             ConDetilsCode = ConDetail.Code,
                                         }).ToList();
                    }

                }

                foreach (var item in WeightCartoon)
                {
                    if (item.Service == "100000")
                    {
                        if (item.ChargeOfChrgWT)
                        {
                            SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ChgWt && x.ToWT >= Model.ChgWt).FirstOrDefault();
                            if (SingleContract != null)
                            {
                                item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                                SingleContract.Charges = item.Charges;
                                break;
                            }
                        }
                        else
                        {
                            SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                            if (SingleContract != null)
                            {
                                item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                                SingleContract.Charges = item.Charges;
                                break;
                            }
                        }
                    }
                    else
                    {
                        SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.TotQty && x.ToWT >= Model.TotQty).FirstOrDefault();
                        if (SingleContract != null)
                        {
                            item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }

                }

                if (SingleContract != null)
                {
                    if (SingleContract.Service != null)
                    {

                        var Rate = SingleContract.Rate;

                        if (SingleContract.ChargeOfChrgWT)
                        {
                            foreach (var item in SingleContract.Charges.ToList())
                            {
                                var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                            }
                        }
                        else
                        {
                            foreach (var item in SingleContract.Charges.ToList())
                            {
                                var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                            }
                        }
                        Model.Charges = SingleContract.Charges;
                    }
                }
            }
            else
            {
                conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();
                if (conMaster != null)
                {
                    SingleContract = (from ConDetail in ctxTFAT.ConDetail
                                      where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                      select new ContractList()
                                      {
                                          Sno = ConDetail.SrNo,
                                          Service = ConDetail.Services,
                                          TypeOfChrg = ConDetail.WtType,
                                          FromWT = ConDetail.Wtfrom.Value,
                                          ToWT = ConDetail.WtTo.Value,
                                          Rate = (float)ConDetail.Rate,
                                          ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                          ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                          ConDetilsCode = ConDetail.Code,
                                      }).FirstOrDefault();
                }

                if (SingleContract.Service == null)
                {
                    string FromBranch = Model.Source, Tobranch = Model.Dest;
                    TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        FromBranch = tfatBranch.Grp;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }

                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                    if (conMaster != null)
                    {
                        SingleContract = (from ConDetail in ctxTFAT.ConDetail
                                          where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                          select new ContractList()
                                          {
                                              Sno = ConDetail.SrNo,
                                              Service = ConDetail.Services,
                                              TypeOfChrg = ConDetail.WtType,
                                              FromWT = ConDetail.Wtfrom.Value,
                                              ToWT = ConDetail.WtTo.Value,
                                              Rate = (float)ConDetail.Rate,
                                              ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                              ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                              ConDetilsCode = ConDetail.Code,
                                          }).FirstOrDefault();
                    }

                }
                if (SingleContract != null)
                {
                    if (SingleContract.Service != null)
                    {
                        SingleContract.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);

                        if (SingleContract.ChargeOfChrgWT)
                        {
                            foreach (var item in SingleContract.Charges.ToList())
                            {
                                var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                            }
                        }
                        else
                        {
                            foreach (var item in SingleContract.Charges.ToList())
                            {
                                var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                            }
                        }
                        Model.Charges = SingleContract.Charges;
                    }
                }
            }


            string Status = "", Message = "", RateChrgOn = "", MRate = "", RateType = "";
            if (Model.ChgType == "100000" || Model.ChgType == "100001")
            {
                if (SingleContract != null)
                {
                    if (SingleContract.Service == null)
                    {

                        if (GeneralContract)
                        {
                            Status = "Error";
                            Message = "Contract Not Found";
                        }

                        else if (Model.ChgType == "100000")
                        {
                            var ConDetail = ctxTFAT.ConDetail.Where(x => x.CustCode == conMaster.Code && x.Services == "100001").ToList();
                            if (ConDetail.Count() > 0)
                            {
                                Status = "OtherFunction";
                                Message = "Not Found Weight Contract...\n Find Carton Rate Accept ?";
                            }
                            else
                            {
                                Status = "Error";
                                Message = "Contract Not Found";
                            }

                        }
                        else
                        {
                            var ConDetail = ctxTFAT.ConDetail.Where(x => x.CustCode == conMaster.Code && x.Services == "100000").ToList();
                            if (ConDetail.Count() > 0)
                            {
                                Status = "OtherFunction";
                                Message = "Not Found Carton Contract...\n Find Weight Rate Accept ?";
                            }
                            else
                            {
                                Status = "Error";
                                Message = "Contract Not Found";
                            }
                        }
                    }
                    else
                    {
                        Status = "Success";
                        RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                        MRate = SingleContract.Rate.ToString();
                        RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

                    }
                }
                else
                {
                    if (GeneralContract)
                    {
                        Status = "Error";
                        Message = "Contract Not Found";
                    }

                    else if (Model.ChgType == "100000")
                    {
                        var ConDetail = ctxTFAT.ConDetail.Where(x => x.CustCode == conMaster.Code && x.Services == "100001").ToList();
                        if (ConDetail.Count() > 0)
                        {
                            Status = "OtherFunction";
                            Message = "Not Found Weight Contract...\n Find Carton Rate Accept ?";
                        }
                        else
                        {
                            Status = "Error";
                            Message = "Contract Not Found";
                        }

                    }
                    else
                    {
                        var ConDetail = ctxTFAT.ConDetail.Where(x => x.CustCode == conMaster.Code && x.Services == "100000").ToList();
                        if (ConDetail.Count() > 0)
                        {
                            Status = "OtherFunction";
                            Message = "Not Found Carton Contract...\n Find Weight Rate Accept ?";
                        }
                        else
                        {
                            Status = "Error";
                            Message = "Contract Not Found";
                        }
                    }
                }

            }
            else
            {
                if (SingleContract != null)
                {
                    if (SingleContract.Service == null)
                    {
                        Status = "Error";
                        Message = "Contract Not Found";
                    }
                    else
                    {
                        Status = "Success";
                        RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                        MRate = SingleContract.Rate.ToString();
                        RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

                    }
                }
                else
                {
                    Status = "Error";
                    Message = "Contract Not Found";
                }
            }

            var html = ViewHelper.RenderPartialView(this, "_Charges", Model);
            var jsonResult = Json(new
            {
                Status = Status,
                Message = Message,
                Html = html,
                RateChrgOn = RateChrgOn,
                MRate = MRate,
                RateType = RateType,

            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public ActionResult OTherContract(LRVM Model)
        {
            var FromDt = ConvertDDMMYYTOYYMMDD(Model.BookDate);
            Model.BillParty = ctxTFAT.CustomerMaster.Where(x => x.Code == Model.BillParty).Select(x => x.AccountParentGroup).FirstOrDefault();
            ContractList SingleContract = new ContractList();
            List<ContractList> WeightCartoon = new List<ContractList>();
            bool Weight = false; string OthCode = "", OthName = "";
            if (Model.ChgType == "100000")
            {
                Weight = true;
            }
            else
            {
                Weight = false;
            }

            string FromBranch = Model.Source, Tobranch = Model.Dest;
            TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
            {
                FromBranch = tfatBranch.Grp;
            }
            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
            {
                Tobranch = tfatBranch.Grp;
            }

            if (Weight)
            {
                OthCode = "100001";
                OthName = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == OthCode).Select(x => x.ChargeType).FirstOrDefault();
                ConMaster conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();
                if (conMaster != null)
                {
                    WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                     where ConDetail.CustCode == conMaster.Code && ConDetail.Services == "100001"
                                     select new ContractList()
                                     {
                                         Sno = ConDetail.SrNo,
                                         Service = ConDetail.Services,
                                         TypeOfChrg = ConDetail.WtType,
                                         FromWT = ConDetail.Wtfrom.Value,
                                         ToWT = ConDetail.WtTo.Value,
                                         Rate = (float)ConDetail.Rate,
                                         ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                         ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                         ConDetilsCode = ConDetail.Code,
                                     }).ToList();
                }
                if (WeightCartoon == null || WeightCartoon.Count() == 0)
                {
                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                    if (conMaster != null)
                    {
                        WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                         where ConDetail.CustCode == conMaster.Code && ConDetail.Services == "100001"
                                         select new ContractList()
                                         {
                                             Sno = ConDetail.SrNo,
                                             Service = ConDetail.Services,
                                             TypeOfChrg = ConDetail.WtType,
                                             FromWT = ConDetail.Wtfrom.Value,
                                             ToWT = ConDetail.WtTo.Value,
                                             Rate = (float)ConDetail.Rate,
                                             ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                             ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                             ConDetilsCode = ConDetail.Code,
                                         }).ToList();
                    }

                }
            }
            else
            {
                OthCode = "100000";
                OthName = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == OthCode).Select(x => x.ChargeType).FirstOrDefault();
                ConMaster conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();
                if (conMaster != null)
                {
                    WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                     where ConDetail.CustCode == conMaster.Code && ConDetail.Services == "100000"
                                     select new ContractList()
                                     {
                                         Sno = ConDetail.SrNo,
                                         Service = ConDetail.Services,
                                         TypeOfChrg = ConDetail.WtType,
                                         FromWT = ConDetail.Wtfrom.Value,
                                         ToWT = ConDetail.WtTo.Value,
                                         Rate = (float)ConDetail.Rate,
                                         ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                         ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                         ConDetilsCode = ConDetail.Code,
                                     }).ToList();
                }

                if (WeightCartoon == null || WeightCartoon.Count() == 0)
                {
                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                    if (conMaster != null)
                    {
                        WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                         where ConDetail.CustCode == conMaster.Code && ConDetail.Services == "100000"
                                         select new ContractList()
                                         {
                                             Sno = ConDetail.SrNo,
                                             Service = ConDetail.Services,
                                             TypeOfChrg = ConDetail.WtType,
                                             FromWT = ConDetail.Wtfrom.Value,
                                             ToWT = ConDetail.WtTo.Value,
                                             Rate = (float)ConDetail.Rate,
                                             ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                             ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                             ConDetilsCode = ConDetail.Code,
                                         }).ToList();
                    }

                }
            }

            foreach (var item in WeightCartoon)
            {
                if (item.Service == "100000")
                {
                    if (item.ChargeOfChrgWT)
                    {
                        SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ChgWt && x.ToWT >= Model.ChgWt).FirstOrDefault();
                        if (SingleContract != null)
                        {
                            item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }
                    else
                    {
                        SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                        if (SingleContract != null)
                        {
                            item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }
                }
                else
                {
                    SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.TotQty && x.ToWT >= Model.TotQty).FirstOrDefault();
                    if (SingleContract != null)
                    {
                        item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                        SingleContract.Charges = item.Charges;
                        break;
                    }
                }

            }

            if (SingleContract != null)
            {
                if (SingleContract.Service != null)
                {

                    var Rate = SingleContract.Rate;
                    if (SingleContract.ChargeOfChrgWT)
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                        }
                    }
                    else
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                        }
                    }
                    Model.Charges = SingleContract.Charges;


                }
            }


            string Status = "", Message = "", RateChrgOn = "", MRate = "", RateType = "";
            if (SingleContract != null)
            {
                if (SingleContract.Service == null)
                {
                    Status = "Error";
                    Message = "Contract Not Found";
                }
                else
                {
                    Status = "Success";
                    RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                    MRate = SingleContract.Rate.ToString();
                    RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

                }
            }
            else
            {
                Status = "Error";
                Message = "Contract Not Found";
            }


            var html = ViewHelper.RenderPartialView(this, "_Charges", Model);
            var jsonResult = Json(new
            {
                Status = Status,
                Message = Message,
                OthCode = OthCode,
                OthName = OthName,
                Html = html,
                RateChrgOn = RateChrgOn,
                MRate = MRate,
                RateType = RateType,

            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public ActionResult GeneralCharges(LRVM Model)
        {
            bool GeneralContract = false;

            var FromDt = ConvertDDMMYYTOYYMMDD(Model.BookDate);
            ContractList SingleContract = new ContractList();
            List<ContractList> WeightCartoon = new List<ContractList>();
            ConMaster conMaster = new ConMaster();
            if (Model.ChgType == "100000" || Model.ChgType == "100001")
            {
                #region Customer Check
                conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();

                if (conMaster != null)
                {
                    WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                     where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                     select new ContractList()
                                     {
                                         Sno = ConDetail.SrNo,
                                         Service = ConDetail.Services,
                                         TypeOfChrg = ConDetail.WtType,
                                         FromWT = ConDetail.Wtfrom.Value,
                                         ToWT = ConDetail.WtTo.Value,
                                         Rate = (float)ConDetail.Rate,
                                         ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                         ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                         ConDetilsCode = ConDetail.Code,
                                     }).ToList();
                }
                else
                {
                    GeneralContract = true;
                }
                if (WeightCartoon == null || WeightCartoon.Count() == 0)
                {
                    string FromBranch = Model.Source, Tobranch = Model.Dest;
                    TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        FromBranch = tfatBranch.Grp;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }

                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
                    if (conMaster != null)
                    {
                        WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                         where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                         select new ContractList()
                                         {
                                             Sno = ConDetail.SrNo,
                                             Service = ConDetail.Services,
                                             TypeOfChrg = ConDetail.WtType,
                                             FromWT = ConDetail.Wtfrom.Value,
                                             ToWT = ConDetail.WtTo.Value,
                                             Rate = (float)ConDetail.Rate,
                                             ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                             ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                             ConDetilsCode = ConDetail.Code,
                                         }).ToList();
                    }

                }

                foreach (var item in WeightCartoon)
                {
                    if (item.Service == "100000")
                    {
                        if (item.ChargeOfChrgWT)
                        {
                            SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ChgWt && x.ToWT >= Model.ChgWt).FirstOrDefault();
                            if (SingleContract != null)
                            {
                                item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                                SingleContract.Charges = item.Charges;
                                break;
                            }
                        }
                        else
                        {
                            SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                            if (SingleContract != null)
                            {
                                item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                                SingleContract.Charges = item.Charges;
                                break;
                            }
                        }
                    }
                    else
                    {
                        SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.TotQty && x.ToWT >= Model.TotQty).FirstOrDefault();
                        if (SingleContract != null)
                        {
                            item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }

                }

                #endregion

                #region Master Check
                if (SingleContract == null)
                {
                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();

                    if (conMaster != null)
                    {
                        WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                         where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                         select new ContractList()
                                         {
                                             Sno = ConDetail.SrNo,
                                             Service = ConDetail.Services,
                                             TypeOfChrg = ConDetail.WtType,
                                             FromWT = ConDetail.Wtfrom.Value,
                                             ToWT = ConDetail.WtTo.Value,
                                             Rate = (float)ConDetail.Rate,
                                             ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                             ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                             ConDetilsCode = ConDetail.Code,
                                         }).ToList();
                    }
                    else
                    {
                        GeneralContract = true;
                    }
                    if (WeightCartoon == null || WeightCartoon.Count() == 0)
                    {
                        string FromBranch = Model.Source, Tobranch = Model.Dest;
                        TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            FromBranch = tfatBranch.Grp;
                        }
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            Tobranch = tfatBranch.Grp;
                        }

                        conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
                        if (conMaster != null)
                        {
                            WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                             where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                             select new ContractList()
                                             {
                                                 Sno = ConDetail.SrNo,
                                                 Service = ConDetail.Services,
                                                 TypeOfChrg = ConDetail.WtType,
                                                 FromWT = ConDetail.Wtfrom.Value,
                                                 ToWT = ConDetail.WtTo.Value,
                                                 Rate = (float)ConDetail.Rate,
                                                 ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                                 ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                                 ConDetilsCode = ConDetail.Code,
                                             }).ToList();
                        }

                    }

                    foreach (var item in WeightCartoon)
                    {
                        if (item.Service == "100000")
                        {
                            if (item.ChargeOfChrgWT)
                            {
                                SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ChgWt && x.ToWT >= Model.ChgWt).FirstOrDefault();
                                if (SingleContract != null)
                                {
                                    item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                                    SingleContract.Charges = item.Charges;
                                    break;
                                }
                            }
                            else
                            {
                                SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                                if (SingleContract != null)
                                {
                                    item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                                    SingleContract.Charges = item.Charges;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.TotQty && x.ToWT >= Model.TotQty).FirstOrDefault();
                            if (SingleContract != null)
                            {
                                item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                                SingleContract.Charges = item.Charges;
                                break;
                            }
                        }

                    }
                }
                #endregion

                if (SingleContract != null)
                {
                    if (SingleContract.Service != null)
                    {
                        var Rate = SingleContract.Rate;
                        if (SingleContract.ChargeOfChrgWT)
                        {
                            foreach (var item in SingleContract.Charges.ToList())
                            {
                                var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                            }
                        }
                        else
                        {
                            foreach (var item in SingleContract.Charges.ToList())
                            {
                                var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                            }
                        }
                        Model.Charges = SingleContract.Charges;


                    }
                }
            }
            else
            {
                #region Customer 
                conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).FirstOrDefault();
                if (conMaster != null)
                {
                    SingleContract = (from ConDetail in ctxTFAT.ConDetail
                                      where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                      select new ContractList()
                                      {
                                          Sno = ConDetail.SrNo,
                                          Service = ConDetail.Services,
                                          TypeOfChrg = ConDetail.WtType,
                                          FromWT = ConDetail.Wtfrom.Value,
                                          ToWT = ConDetail.WtTo.Value,
                                          Rate = (float)ConDetail.Rate,
                                          ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                          ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                          ConDetilsCode = ConDetail.Code,
                                      }).FirstOrDefault();
                }

                if (SingleContract.Service == null)
                {
                    string FromBranch = Model.Source, Tobranch = Model.Dest;
                    TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        FromBranch = tfatBranch.Grp;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }

                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
                    if (conMaster != null)
                    {
                        SingleContract = (from ConDetail in ctxTFAT.ConDetail
                                          where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                          select new ContractList()
                                          {
                                              Sno = ConDetail.SrNo,
                                              Service = ConDetail.Services,
                                              TypeOfChrg = ConDetail.WtType,
                                              FromWT = ConDetail.Wtfrom.Value,
                                              ToWT = ConDetail.WtTo.Value,
                                              Rate = (float)ConDetail.Rate,
                                              ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                              ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                              ConDetilsCode = ConDetail.Code,
                                          }).FirstOrDefault();
                    }

                }

                if (SingleContract != null)
                {
                    if (SingleContract.Service != null)
                    {
                        SingleContract.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                        if (SingleContract.ChargeOfChrgWT)
                        {
                            foreach (var item in SingleContract.Charges.ToList())
                            {
                                var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                            }
                        }
                        else
                        {
                            foreach (var item in SingleContract.Charges.ToList())
                            {
                                var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                            }
                        }
                        Model.Charges = SingleContract.Charges;
                    }
                }
                #endregion

                #region Master
                if (conMaster == null)
                {
                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).FirstOrDefault();
                    if (conMaster != null)
                    {
                        SingleContract = (from ConDetail in ctxTFAT.ConDetail
                                          where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                          select new ContractList()
                                          {
                                              Sno = ConDetail.SrNo,
                                              Service = ConDetail.Services,
                                              TypeOfChrg = ConDetail.WtType,
                                              FromWT = ConDetail.Wtfrom.Value,
                                              ToWT = ConDetail.WtTo.Value,
                                              Rate = (float)ConDetail.Rate,
                                              ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                              ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                              ConDetilsCode = ConDetail.Code,
                                          }).FirstOrDefault();
                    }

                    if (SingleContract.Service == null)
                    {
                        string FromBranch = Model.Source, Tobranch = Model.Dest;
                        TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            FromBranch = tfatBranch.Grp;
                        }
                        tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                        if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                        {
                            Tobranch = tfatBranch.Grp;
                        }

                        conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
                        if (conMaster != null)
                        {
                            SingleContract = (from ConDetail in ctxTFAT.ConDetail
                                              where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.ChgType
                                              select new ContractList()
                                              {
                                                  Sno = ConDetail.SrNo,
                                                  Service = ConDetail.Services,
                                                  TypeOfChrg = ConDetail.WtType,
                                                  FromWT = ConDetail.Wtfrom.Value,
                                                  ToWT = ConDetail.WtTo.Value,
                                                  Rate = (float)ConDetail.Rate,
                                                  ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                                  ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                                  ConDetilsCode = ConDetail.Code,
                                              }).FirstOrDefault();
                        }
                    }

                    #endregion

                    if (SingleContract != null)
                    {
                        if (SingleContract.Service != null)
                        {
                            SingleContract.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                            if (SingleContract.ChargeOfChrgWT)
                            {
                                foreach (var item in SingleContract.Charges.ToList())
                                {
                                    var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                    SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                                }
                            }
                            else
                            {
                                foreach (var item in SingleContract.Charges.ToList())
                                {
                                    var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                                    SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                                }
                            }
                            Model.Charges = SingleContract.Charges;
                        }
                    }

                }

            }




            string Status = "", Message = "", RateChrgOn = "", MRate = "", RateType = "";
            if (Model.ChgType == "100000" || Model.ChgType == "100001")
            {
                if (SingleContract != null)
                {
                    if (SingleContract.Service == null)
                    {

                        if (GeneralContract)
                        {
                            Status = "Error";
                            Message = "Contract Not Found";
                        }

                        else if (Model.ChgType == "100000")
                        {
                            var ConDetail = ctxTFAT.ConDetail.Where(x => x.CustCode == conMaster.Code && x.Services == "100001").ToList();
                            if (ConDetail.Count() > 0)
                            {
                                Status = "OtherFunction";
                                Message = "Not Found Weight Contract...\n Find Carton Rate Accept ?";
                            }
                            else
                            {
                                Status = "Error";
                                Message = "Contract Not Found";
                            }

                        }
                        else
                        {
                            var ConDetail = ctxTFAT.ConDetail.Where(x => x.CustCode == conMaster.Code && x.Services == "100000").ToList();
                            if (ConDetail.Count() > 0)
                            {
                                Status = "OtherFunction";
                                Message = "Not Found Carton Contract...\n Find Weight Rate Accept ?";
                            }
                            else
                            {
                                Status = "Error";
                                Message = "Contract Not Found";
                            }
                        }
                    }
                    else
                    {
                        Status = "Success";
                        RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                        MRate = SingleContract.Rate.ToString();
                        RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

                    }
                }
                else
                {
                    if (GeneralContract)
                    {
                        Status = "Error";
                        Message = "Contract Not Found";
                    }

                    else if (Model.ChgType == "100000")
                    {
                        var ConDetail = ctxTFAT.ConDetail.Where(x => x.CustCode == conMaster.Code && x.Services == "100001").ToList();
                        if (ConDetail.Count() > 0)
                        {
                            Status = "OtherFunction";
                            Message = "Not Found Weight Contract...\n Find Carton Rate Accept ?";
                        }
                        else
                        {
                            Status = "Error";
                            Message = "Contract Not Found";
                        }

                    }
                    else
                    {
                        var ConDetail = ctxTFAT.ConDetail.Where(x => x.CustCode == conMaster.Code && x.Services == "100000").ToList();
                        if (ConDetail.Count() > 0)
                        {
                            Status = "OtherFunction";
                            Message = "Not Found Carton Contract...\n Find Weight Rate Accept ?";
                        }
                        else
                        {
                            Status = "Error";
                            Message = "Contract Not Found";
                        }
                    }
                }

            }
            else
            {
                if (SingleContract != null)
                {
                    if (SingleContract.Service == null)
                    {
                        Status = "Error";
                        Message = "Contract Not Found";
                    }
                    else
                    {
                        Status = "Success";
                        RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                        MRate = SingleContract.Rate.ToString();
                        RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

                    }
                }
                else
                {
                    Status = "Error";
                    Message = "Contract Not Found";
                }

            }

            var html = ViewHelper.RenderPartialView(this, "_Charges", Model);
            var jsonResult = Json(new
            {
                Status = Status,
                Message = Message,
                Html = html,
                RateChrgOn = RateChrgOn,
                MRate = MRate,
                RateType = RateType,

            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public ActionResult GeneralOTherContract(LRVM Model)
        {
            var FromDt = ConvertDDMMYYTOYYMMDD(Model.BookDate);
            ContractList SingleContract = new ContractList();
            List<ContractList> WeightCartoon = new List<ContractList>();
            bool Weight = false; string OthCode = "", OthName = "";
            if (Model.ChgType == "100000")
            {
                Weight = true;
            }
            else
            {
                Weight = false;
            }

            string FromBranch = Model.Source, Tobranch = Model.Dest;
            TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
            {
                FromBranch = tfatBranch.Grp;
            }
            tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
            if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
            {
                Tobranch = tfatBranch.Grp;
            }

            if (Weight)
            {
                OthCode = "100001";
                OthName = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == OthCode).Select(x => x.ChargeType).FirstOrDefault();
                ConMaster conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).FirstOrDefault();
                if (conMaster != null)
                {
                    WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                     where ConDetail.CustCode == conMaster.Code && ConDetail.Services == "100001"
                                     select new ContractList()
                                     {
                                         Sno = ConDetail.SrNo,
                                         Service = ConDetail.Services,
                                         TypeOfChrg = ConDetail.WtType,
                                         FromWT = ConDetail.Wtfrom.Value,
                                         ToWT = ConDetail.WtTo.Value,
                                         Rate = (float)ConDetail.Rate,
                                         ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                         ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                         ConDetilsCode = ConDetail.Code,
                                     }).ToList();
                }
                if (WeightCartoon == null || WeightCartoon.Count() == 0)
                {
                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
                    if (conMaster != null)
                    {
                        WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                         where ConDetail.CustCode == conMaster.Code && ConDetail.Services == "100001"
                                         select new ContractList()
                                         {
                                             Sno = ConDetail.SrNo,
                                             Service = ConDetail.Services,
                                             TypeOfChrg = ConDetail.WtType,
                                             FromWT = ConDetail.Wtfrom.Value,
                                             ToWT = ConDetail.WtTo.Value,
                                             Rate = (float)ConDetail.Rate,
                                             ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                             ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                             ConDetilsCode = ConDetail.Code,
                                         }).ToList();
                    }

                }
            }
            else
            {
                OthCode = "100000";
                OthName = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == OthCode).Select(x => x.ChargeType).FirstOrDefault();
                ConMaster conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).FirstOrDefault();
                if (conMaster != null)
                {
                    WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                     where ConDetail.CustCode == conMaster.Code && ConDetail.Services == "100000"
                                     select new ContractList()
                                     {
                                         Sno = ConDetail.SrNo,
                                         Service = ConDetail.Services,
                                         TypeOfChrg = ConDetail.WtType,
                                         FromWT = ConDetail.Wtfrom.Value,
                                         ToWT = ConDetail.WtTo.Value,
                                         Rate = (float)ConDetail.Rate,
                                         ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                         ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                         ConDetilsCode = ConDetail.Code,
                                     }).ToList();
                }

                if (WeightCartoon == null || WeightCartoon.Count() == 0)
                {
                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();
                    if (conMaster != null)
                    {
                        WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                         where ConDetail.CustCode == conMaster.Code && ConDetail.Services == "100000"
                                         select new ContractList()
                                         {
                                             Sno = ConDetail.SrNo,
                                             Service = ConDetail.Services,
                                             TypeOfChrg = ConDetail.WtType,
                                             FromWT = ConDetail.Wtfrom.Value,
                                             ToWT = ConDetail.WtTo.Value,
                                             Rate = (float)ConDetail.Rate,
                                             ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                             ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                             ConDetilsCode = ConDetail.Code,
                                         }).ToList();
                    }

                }
            }

            foreach (var item in WeightCartoon)
            {
                if (item.Service == "100000")
                {
                    if (item.ChargeOfChrgWT)
                    {
                        SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ChgWt && x.ToWT >= Model.ChgWt).FirstOrDefault();
                        if (SingleContract != null)
                        {
                            item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }
                    else
                    {
                        SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                        if (SingleContract != null)
                        {
                            item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }
                }
                else
                {
                    SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.TotQty && x.ToWT >= Model.TotQty).FirstOrDefault();
                    if (SingleContract != null)
                    {
                        item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                        SingleContract.Charges = item.Charges;
                        break;
                    }
                }

            }

            if (SingleContract != null)
            {
                if (SingleContract.Service != null)
                {
                    var Rate = SingleContract.Rate;
                    if (SingleContract.ChargeOfChrgWT)
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                        }
                    }
                    else
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                        }
                    }
                    Model.Charges = SingleContract.Charges;
                }
            }

            string Status = "", Message = "", RateChrgOn = "", MRate = "", RateType = "";
            if (SingleContract != null)
            {
                if (SingleContract.Service == null)
                {
                    Status = "Error";
                    Message = "Contract Not Found";
                }
                else
                {
                    Status = "Success";
                    RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                    MRate = SingleContract.Rate.ToString();
                    RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

                }
            }
            else
            {
                Status = "Error";
                Message = "Contract Not Found";
            }


            var html = ViewHelper.RenderPartialView(this, "_Charges", Model);
            var jsonResult = Json(new
            {
                Status = Status,
                Message = Message,
                OthCode = OthCode,
                OthName = OthName,
                Html = html,
                RateChrgOn = RateChrgOn,
                MRate = MRate,
                RateType = RateType,


            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        // ChargeType Wise Contract Methods

        // Item Wise Contract Methods

        public ActionResult ItemWiseCustomerCharges(LRVM Model)
        {
            bool GeneralContract = false;

            var FromDt = ConvertDDMMYYTOYYMMDD(Model.BookDate);
            ContractList SingleContract = new ContractList();
            List<ContractList> WeightCartoon = new List<ContractList>();
            ConMaster conMaster = new ConMaster();

            conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();
            if (conMaster != null)
            {
                WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                 where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
                                 select new ContractList()
                                 {
                                     Sno = ConDetail.SrNo,
                                     Service = ConDetail.Services,
                                     TypeOfChrg = ConDetail.WtType,
                                     FromWT = ConDetail.Wtfrom.Value,
                                     ToWT = ConDetail.WtTo.Value,
                                     Rate = (float)ConDetail.Rate,
                                     ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                     ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                     ConDetilsCode = ConDetail.Code,
                                 }).ToList();
            }
            else
            {
                GeneralContract = true;
            }

            if (WeightCartoon == null || WeightCartoon.Count() == 0)
            {
                string FromBranch = Model.Source, Tobranch = Model.Dest;
                TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                {
                    FromBranch = tfatBranch.Grp;
                }
                tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                {
                    Tobranch = tfatBranch.Grp;
                }

                conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                if (conMaster != null)
                {
                    WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                     where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
                                     select new ContractList()
                                     {
                                         Sno = ConDetail.SrNo,
                                         Service = ConDetail.Services,
                                         TypeOfChrg = ConDetail.WtType,
                                         FromWT = ConDetail.Wtfrom.Value,
                                         ToWT = ConDetail.WtTo.Value,
                                         Rate = (float)ConDetail.Rate,
                                         ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                         ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                         ConDetilsCode = ConDetail.Code,
                                     }).ToList();
                }

            }

            foreach (var item in WeightCartoon)
            {

                if (item.ChargeOfChrgWT)
                {
                    SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ChgWt && x.ToWT >= Model.ChgWt).FirstOrDefault();
                    if (SingleContract != null)
                    {
                        item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                        SingleContract.Charges = item.Charges;
                        break;
                    }
                }
                else
                {
                    SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                    if (SingleContract != null)
                    {
                        item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                        SingleContract.Charges = item.Charges;
                        break;
                    }
                }
            }

            if (SingleContract != null)
            {
                if (SingleContract.Service != null)
                {
                    var Rate = SingleContract.Rate;

                    if (SingleContract.ChargeOfChrgWT)
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                        }
                    }
                    else
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                        }
                    }
                    Model.Charges = SingleContract.Charges;

                }
            }


            string Status = "", Message = "", RateChrgOn = "", MRate = "", RateType = "";

            if (SingleContract != null)
            {
                if (SingleContract.Service == null)
                {


                    Status = "Error";
                    Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";

                }
                else
                {
                    Status = "Success";
                    RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                    MRate = SingleContract.Rate.ToString();
                    RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

                }
            }
            else
            {

                Status = "Error";
                Message = "Customer Contract Not Found...\n Please Allow To Find The Master Contranct? ";


            }
            var html = ViewHelper.RenderPartialView(this, "_Charges", Model);
            var jsonResult = Json(new
            {
                Status = Status,
                Message = Message,
                Html = html,
                RateChrgOn = RateChrgOn,
                MRate = MRate,
                RateType = RateType,

            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public ActionResult ItemWiseMasterCharges(LRVM Model)
        {
            bool GeneralContract = false;

            var FromDt = ConvertDDMMYYTOYYMMDD(Model.BookDate);
            ContractList SingleContract = new ContractList();
            List<ContractList> WeightCartoon = new List<ContractList>();
            ConMaster conMaster = new ConMaster();

            conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();
            if (conMaster != null)
            {
                WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                 where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
                                 select new ContractList()
                                 {
                                     Sno = ConDetail.SrNo,
                                     Service = ConDetail.Services,
                                     TypeOfChrg = ConDetail.WtType,
                                     FromWT = ConDetail.Wtfrom.Value,
                                     ToWT = ConDetail.WtTo.Value,
                                     Rate = (float)ConDetail.Rate,
                                     ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                     ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                     ConDetilsCode = ConDetail.Code,
                                 }).ToList();
            }
            else
            {
                GeneralContract = true;
            }

            if (WeightCartoon == null || WeightCartoon.Count() == 0)
            {
                string FromBranch = Model.Source, Tobranch = Model.Dest;
                TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                {
                    FromBranch = tfatBranch.Grp;
                }
                tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                {
                    Tobranch = tfatBranch.Grp;
                }

                conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == Model.BillParty && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).OrderByDescending(x => x.FromDt).FirstOrDefault();
                if (conMaster != null)
                {
                    WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                     where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
                                     select new ContractList()
                                     {
                                         Sno = ConDetail.SrNo,
                                         Service = ConDetail.Services,
                                         TypeOfChrg = ConDetail.WtType,
                                         FromWT = ConDetail.Wtfrom.Value,
                                         ToWT = ConDetail.WtTo.Value,
                                         Rate = (float)ConDetail.Rate,
                                         ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                         ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                         ConDetilsCode = ConDetail.Code,
                                     }).ToList();
                }

            }

            foreach (var item in WeightCartoon)
            {

                if (item.ChargeOfChrgWT)
                {
                    SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ChgWt && x.ToWT >= Model.ChgWt).FirstOrDefault();
                    if (SingleContract != null)
                    {
                        item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                        SingleContract.Charges = item.Charges;
                        break;
                    }
                }
                else
                {
                    SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                    if (SingleContract != null)
                    {
                        item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                        SingleContract.Charges = item.Charges;
                        break;
                    }
                }
            }

            if (SingleContract != null)
            {
                if (SingleContract.Service != null)
                {
                    var Rate = SingleContract.Rate;

                    if (SingleContract.ChargeOfChrgWT)
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                        }
                    }
                    else
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                        }
                    }
                    Model.Charges = SingleContract.Charges;

                }
            }

            string Status = "", Message = "", RateChrgOn = "", MRate = "", RateType = "";

            if (SingleContract != null)
            {
                if (SingleContract.Service == null)
                {
                    Status = "Error";
                    Message = "Contract Not Found";
                }
                else
                {
                    Status = "Success";
                    RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                    MRate = SingleContract.Rate.ToString();
                    RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

                }
            }
            else
            {
                Status = "Error";
                Message = "Contract Not Found";
            }


            var html = ViewHelper.RenderPartialView(this, "_Charges", Model);
            var jsonResult = Json(new
            {
                Status = Status,
                Message = Message,
                Html = html,
                RateChrgOn = RateChrgOn,
                MRate = MRate,
                RateType = RateType,

            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public ActionResult GeneralItemWiseCharges(LRVM Model)
        {
            bool GeneralContract = false;

            var FromDt = ConvertDDMMYYTOYYMMDD(Model.BookDate);
            ContractList SingleContract = new ContractList();
            List<ContractList> WeightCartoon = new List<ContractList>();
            ConMaster conMaster = new ConMaster();

            #region Customer Check
            conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();

            if (conMaster != null)
            {
                WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                 where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
                                 select new ContractList()
                                 {
                                     Sno = ConDetail.SrNo,
                                     Service = ConDetail.Services,
                                     TypeOfChrg = ConDetail.WtType,
                                     FromWT = ConDetail.Wtfrom.Value,
                                     ToWT = ConDetail.WtTo.Value,
                                     Rate = (float)ConDetail.Rate,
                                     ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                     ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                     ConDetilsCode = ConDetail.Code,
                                 }).ToList();
            }
            else
            {
                GeneralContract = true;
            }
            if (WeightCartoon == null || WeightCartoon.Count() == 0)
            {
                string FromBranch = Model.Source, Tobranch = Model.Dest;
                TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                {
                    FromBranch = tfatBranch.Grp;
                }
                tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                {
                    Tobranch = tfatBranch.Grp;
                }

                conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "CU" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();

                if (conMaster != null)
                {
                    WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                     where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
                                     select new ContractList()
                                     {
                                         Sno = ConDetail.SrNo,
                                         Service = ConDetail.Services,
                                         TypeOfChrg = ConDetail.WtType,
                                         FromWT = ConDetail.Wtfrom.Value,
                                         ToWT = ConDetail.WtTo.Value,
                                         Rate = (float)ConDetail.Rate,
                                         ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                         ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                         ConDetilsCode = ConDetail.Code,
                                     }).ToList();
                }

            }
            foreach (var item in WeightCartoon)
            {

                if (item.ChargeOfChrgWT)
                {
                    SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ChgWt && x.ToWT >= Model.ChgWt).FirstOrDefault();
                    if (SingleContract != null)
                    {
                        item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                        SingleContract.Charges = item.Charges;
                        break;
                    }
                }
                else
                {
                    SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                    if (SingleContract != null)
                    {
                        item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                        SingleContract.Charges = item.Charges;
                        break;
                    }
                }
            }

            #endregion

            #region Master Check
            if (SingleContract == null)
            {
                conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == Model.Source && x.ToBranch == Model.Dest).OrderByDescending(x => x.FromDt).FirstOrDefault();

                if (conMaster != null)
                {
                    WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                     where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
                                     select new ContractList()
                                     {
                                         Sno = ConDetail.SrNo,
                                         Service = ConDetail.Services,
                                         TypeOfChrg = ConDetail.WtType,
                                         FromWT = ConDetail.Wtfrom.Value,
                                         ToWT = ConDetail.WtTo.Value,
                                         Rate = (float)ConDetail.Rate,
                                         ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                         ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                         ConDetilsCode = ConDetail.Code,
                                     }).ToList();
                }
                else
                {
                    GeneralContract = true;
                }
                if (WeightCartoon == null || WeightCartoon.Count() == 0)
                {
                    string FromBranch = Model.Source, Tobranch = Model.Dest;
                    TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Source).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        FromBranch = tfatBranch.Grp;
                    }
                    tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Model.Dest).FirstOrDefault();
                    if (tfatBranch.Category == "Area" || tfatBranch.Category == "SubBranch")
                    {
                        Tobranch = tfatBranch.Grp;
                    }

                    conMaster = ctxTFAT.ConMaster.Where(x => x.Cust == "General" && x.CustType == "MA" && x.FromDt <= FromDt && x.FromBranch == FromBranch && x.ToBranch == Tobranch).FirstOrDefault();

                    if (conMaster != null)
                    {
                        WeightCartoon = (from ConDetail in ctxTFAT.ConDetail
                                         where ConDetail.CustCode == conMaster.Code && ConDetail.Services == Model.DescrType
                                         select new ContractList()
                                         {
                                             Sno = ConDetail.SrNo,
                                             Service = ConDetail.Services,
                                             TypeOfChrg = ConDetail.WtType,
                                             FromWT = ConDetail.Wtfrom.Value,
                                             ToWT = ConDetail.WtTo.Value,
                                             Rate = (float)ConDetail.Rate,
                                             ChargeOfActWT = ConDetail.ChgOn == true ? false : true,
                                             ChargeOfChrgWT = ConDetail.ChgOn == true ? true : false,
                                             ConDetilsCode = ConDetail.Code,
                                         }).ToList();
                    }

                }
                foreach (var item in WeightCartoon)
                {

                    if (item.ChargeOfChrgWT)
                    {
                        SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ChgWt && x.ToWT >= Model.ChgWt).FirstOrDefault();
                        if (SingleContract != null)
                        {
                            item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }
                    else
                    {
                        SingleContract = WeightCartoon.Where(x => x.FromWT <= Model.ActWt && x.ToWT >= Model.ActWt).FirstOrDefault();
                        if (SingleContract != null)
                        {
                            item.Charges = GetChargesOfService(SingleContract.ConDetilsCode, SingleContract.Service, SingleContract.Sno);
                            SingleContract.Charges = item.Charges;
                            break;
                        }
                    }
                }
            }
            #endregion





            if (SingleContract != null)
            {
                if (SingleContract.Service != null)
                {

                    var Rate = SingleContract.Rate;
                    if (SingleContract.ChargeOfChrgWT)
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ChgWt)));
                        }
                    }
                    else
                    {
                        foreach (var item in SingleContract.Charges.ToList())
                        {
                            var purchase = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).FirstOrDefault();
                            SingleContract.Charges.FirstOrDefault(c => c.Fld.ToLower().Trim() == item.Fld.ToLower().Trim()).Val1 = Convert.ToDecimal((GetSingleChargeCalculate(purchase, Model.TotQty, Model.ActWt)));
                        }
                    }
                    Model.Charges = SingleContract.Charges;


                }
            }

            string Status = "", Message = "", RateChrgOn = "", MRate = "", RateType = "";
            if (SingleContract != null)
            {
                if (SingleContract.Service == null)
                {
                    Status = "Error";
                    Message = "Contract Not Found";
                }
                else
                {
                    Status = "Success";
                    RateChrgOn = SingleContract.ChargeOfChrgWT == true ? "C" : "A";
                    MRate = SingleContract.Rate.ToString();
                    RateType = SingleContract.Charges.Where(x => x.Fld.ToLower().Trim() == "f001").Select(x => x.Type).FirstOrDefault().ToString();

                }
            }
            else
            {
                Status = "Error";
                Message = "Contract Not Found";
            }



            var html = ViewHelper.RenderPartialView(this, "_Charges", Model);
            var jsonResult = Json(new
            {
                Status = Status,
                Message = Message,
                Html = html,
                RateChrgOn = RateChrgOn,
                MRate = MRate,
                RateType = RateType,


            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        // Item Wise Contract Methods

        public ActionResult RefreshCharges(LRVM Model)
        {
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
            Model.Charges = objledgerdetail;

            var html = ViewHelper.RenderPartialView(this, "_Charges", Model);
            var jsonResult = Json(new
            {
                Status = "Success",
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public double GetSingleChargeCalculate(PurchaseVM SingleCharge, int Qty, double Weight)
        {
            double ChrgAmt = 0;

            if (SingleCharge != null)
            {
                if (SingleCharge.Type == "1")
                {
                    ChrgAmt = Convert.ToDouble((Convert.ToDecimal(Convert.ToDouble(SingleCharge.Val1) * Weight)));
                }
                else if (SingleCharge.Type == "2")
                {
                    var Tone = Weight / 10000;
                    ChrgAmt = Math.Round(Convert.ToDouble(SingleCharge.Val1) * Tone);
                }
                else if (SingleCharge.Type == "3")
                {
                    ChrgAmt = Math.Round(Convert.ToDouble(SingleCharge.Val1 * Qty));
                }
                else if (SingleCharge.Type == "4")
                {
                    ChrgAmt = Math.Round(Convert.ToDouble(SingleCharge.Val1));
                }

            }


            return ChrgAmt;
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
                c.Val1 = GetChargeValValue(code, Service, Sno, "Val", c.tempId);
                c.Type = GetChargeValType(code, Service, Sno, "Flg", c.tempId);
                purchases.Add(c);
            }
            return purchases;
        }

        public decimal GetChargeValValue(string code, string Service, int Sno, string Val, int i)
        {
            string connstring = GetConnectionString();
            string abc;

            var loginQuery3 = @"select " + Val + i + " from ConDetail where Code='" + code + "' and Services='" + Service + "' and SrNo=" + Sno + " ";
            DataTable mDt2 = GetDataTable(loginQuery3, connstring);
            if (mDt2.Rows.Count > 0)
            {
                abc = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? "0" : mDt2.Rows[0][0].ToString();
            }
            else
            {
                abc = "0";
            }
            return Convert.ToDecimal(abc);
        }

        public string GetChargeValType(string code, string Service, int Sno, string Val, int i)
        {
            string connstring = GetConnectionString();
            string abc;

            var loginQuery3 = @"select " + Val + i + " from ConDetail where Code='" + code + "' and Services='" + Service + "' and SrNo=" + Sno + " ";
            DataTable mDt2 = GetDataTable(loginQuery3, connstring);
            if (mDt2.Rows.Count > 0)
            {
                abc = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? "" : mDt2.Rows[0][0].ToString();
            }
            else
            {
                abc = "";
            }
            return abc;
        }

        public decimal GetChargesVal(List<PurchaseVM> Charges, string FToken)
        {
            string connstring = GetConnectionString();
            string sql;
            decimal mamtm;
            var Val = Charges.Where(x => x.Fld.Trim() == FToken.Trim()).Select(x => x.Val1).FirstOrDefault();
            var PosNeg = Charges.Where(x => x.Fld.Trim() == FToken.Trim()).Select(x => x.AddLess).FirstOrDefault();
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

        #endregion

        public ActionResult GenerateBarCode(string Code)
        {
            LRMaster lR = new LRMaster();
            string barCode = Code;
            System.Web.UI.WebControls.Image imgBarCode = new System.Web.UI.WebControls.Image();
            using (Bitmap bitMap = new Bitmap(barCode.Length * 40, 80))
            {
                using (Graphics graphics = Graphics.FromImage(bitMap))
                {
                    Font oFont = new Font("IDAutomationHC39M", 16);
                    PointF point = new PointF(2f, 2f);
                    SolidBrush blackBrush = new SolidBrush(Color.Black);
                    SolidBrush whiteBrush = new SolidBrush(Color.White);
                    graphics.FillRectangle(whiteBrush, 0, 0, bitMap.Width, bitMap.Height);
                    graphics.DrawString("*" + barCode + "*", oFont, blackBrush, point);

                }
                using (MemoryStream ms = new MemoryStream())
                {
                    bitMap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] byteImage = ms.ToArray();

                    Convert.ToBase64String(byteImage);
                    imgBarCode.ImageUrl = "data:image/png;base64," + Convert.ToBase64String(byteImage);

                }
                ViewBag.BarcodeImage = imgBarCode.ImageUrl;
                //bitMap.Save(@"~/Barcode/code.png", ImageFormat.Png);

            }

            var html = ViewHelper.RenderPartialView(this, "BarcodeView", lR);
            var jsonResult = Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
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
                    att.AUTHORISE = mAUTHORISE;
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
                    att.Type = String.IsNullOrEmpty(item.Type) == true ? "LR000" : item.Type;
                    att.ParentKey = Model.ParentKey;

                    att.CompCode = mcompcode;
                    att.ExternalAttach = item.ExternalAttach;
                    att.RefDocNo = item.RefCode;
                    att.RefType = item.RefType;
                    ctxTFAT.Attachment.Add(att);
                    ctxTFAT.SaveChanges();
                    System.IO.File.WriteAllBytes(directoryPath, IData);

                    ++J;
                    ++AttachCode;
                }
            }

        }

        public ActionResult AddToFavorite(GridOption mModel)
        {
            int Status = 0;
            if (String.IsNullOrEmpty(mModel.Controller))
            {
                mModel.Controller = "";
            }
            var tfatMenu = ctxTFAT.TfatMenu.Where(x => x.FormatCode.Trim().ToUpper() == mModel.ViewDataId.Trim().ToUpper() && x.OptionCode.ToString() == mModel.OptionCode).FirstOrDefault();
            if (tfatMenu == null && String.IsNullOrEmpty(mModel.OptionCode))
            {
                tfatMenu = ctxTFAT.TfatMenu.Where(x => x.FormatCode.Trim().ToUpper() == mModel.ViewDataId.Trim().ToUpper()).FirstOrDefault();
            }

            if (tfatMenu != null)
            {
                var USerFavourite = ctxTFAT.tfatUserFavourite.Where(x => x.ID == tfatMenu.ID && x.UserCode == muserid).FirstOrDefault();
                if (USerFavourite == null)
                {
                    USerFavourite = new tfatUserFavourite();
                    USerFavourite.ID = tfatMenu.ID;
                    USerFavourite.UserCode = muserid;
                    USerFavourite.Favourite = true;
                    USerFavourite.AUTHIDS = muserid;
                    USerFavourite.AUTHORISE = mauthorise;
                    USerFavourite.ENTEREDBY = muserid;
                    USerFavourite.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                    ctxTFAT.tfatUserFavourite.Add(USerFavourite);
                    Status = 1;
                }
                else
                {
                    ctxTFAT.tfatUserFavourite.Remove(USerFavourite);
                    Status = 0;
                }


                //if (tfatMenu.QuickMaster)
                //{
                //    tfatMenu.QuickMaster = false;
                //    tfatMenu.QuickMenu = false;
                //    Status = 0;
                //}
                //else
                //{
                //    tfatMenu.QuickMaster = true;
                //    tfatMenu.QuickMenu = true;
                //    Status = 1;
                //}

                //ctxTFAT.Entry(tfatMenu).State = EntityState.Modified;
                ctxTFAT.SaveChanges();

            }
            return Json(new { Status = Status }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMyFavorites(GridOption mModel)
        {
            int Status = 0;
            TfatMenu tfatMenu = ctxTFAT.TfatMenu.Where(x => x.FormatCode.Trim().ToUpper() == mModel.ViewDataId.Trim().ToUpper() && x.OptionCode.ToString() == mModel.OptionCode).FirstOrDefault();
            if (tfatMenu != null)
            {
                var USerFavourite = ctxTFAT.tfatUserFavourite.Where(x => x.ID == tfatMenu.ID && x.UserCode == muserid).FirstOrDefault();
                if (USerFavourite == null)
                {
                    Status = 0;
                }
                else
                {
                    Status = 1;
                }
                //if (tfatMenu.QuickMaster)
                //{
                //    Status = 1;
                //}
                //else
                //{
                //    Status = 0;
                //}
            }
            return Json(new { Status = Status }, JsonRequestBehavior.AllowGet);
        }

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

            var VerifiedKey = dtreport.Rows[0]["VerifiedKey"].ToString();
            if (VerifiedKey.ToString().ToLower() == mParentKey.ToString().ToLower())
            {
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
            else
            {
                rd.Close();
                rd.Dispose();
                return Json(new { Status = "Something Wrong" }, JsonRequestBehavior.AllowGet);
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
            bool CheckDocument = true;
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
                    var VerifiedKey = dtreport.Rows[0]["VerifiedKey"].ToString();
                    if (VerifiedKey.ToString().ToLower() == mParentKey.ToString().ToLower())
                    {
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
                    else
                    {
                        CheckDocument = false;
                        rd.Close();
                        rd.Dispose();
                        break;
                    }
                }
            }
            document.Close();
            if (CheckDocument)
            {
                if (String.IsNullOrEmpty(PDFName))
                {
                    return File(ms.ToArray(), "application/PDF");
                }
                else
                {
                    return File(ms.ToArray(), "application/PDF", PDFName + ".pdf");
                }
            }
            else
            {
                return Json(new { Status = "Something Wrong" }, JsonRequestBehavior.AllowGet);
            }
            //return File(ms.ToArray(), "application/PDF");
        }

        #region Not Required Functions

        public ActionResult GetAllRemarkOfDocument(string LRNO)
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
                            LRMaster fM = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == Model.FMNO).FirstOrDefault();
                            var LastSno = ctxTFAT.Narration.Where(x => x.ParentKey == fM.ParentKey).ToList().Count();
                            ++LastSno;

                            Narration narr = new Narration();
                            narr.Branch = mbranchcode;
                            narr.Narr = Model.NarrStr;
                            narr.NarrRich = Model.Header;
                            narr.Prefix = mperiod;
                            narr.Sno = LastSno;
                            narr.Srl = fM.LrNo.ToString();
                            narr.Type = "LR000";
                            narr.ENTEREDBY = muserid;
                            narr.LASTUPDATEDATE = DateTime.Now;
                            narr.AUTHORISE = mauthorise;
                            narr.AUTHIDS = muserid;
                            narr.LocationCode = 0;
                            narr.TableKey = "LR000" + mperiod.Substring(0, 2) + LastSno.ToString("D3") + fM.LrNo.ToString();
                            narr.CompCode = mcompcode;
                            narr.ParentKey = fM.ParentKey;
                            ctxTFAT.Narration.Add(narr);
                            ctxTFAT.SaveChanges();
                            transaction.Commit();
                            transaction.Dispose();

                            NewNarr.FMNO = fM.LrNo.ToString();
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
                        Narration narration = ctxTFAT.Narration.Where(x => x.Sno.ToString() == mModel.NarrSno.ToString() && x.Type == "LR000").FirstOrDefault();
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