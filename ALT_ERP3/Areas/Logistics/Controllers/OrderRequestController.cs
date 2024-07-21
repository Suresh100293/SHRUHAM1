using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using System.Configuration;
using System.IO;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class OrderRequestController : BaseController
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

        public ActionResult CheckLRDate(string DocDate, string DocTime)
        {
            string message = "", Status = "T";
            var Date = DocDate.Split('/');
            string[] Time = DocTime.Split(':');
            DateTime Lrdate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Time[0]), Convert.ToInt32(Time[1]), 00);
            if (ConvertDDMMYYTOYYMMDD(StartDate) <= Lrdate && Lrdate <= ConvertDDMMYYTOYYMMDD(EndDate))
            {
                Status = "T";
            }
            else
            {
                Status = "F";
                message = "Financial Date Range Allow Only...!";
            }
            var Datew = ConvertDDMMYYTOYYMMDD(DocDate);
            if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.Type == "PCK00" && x.LockDate == Datew).FirstOrDefault() != null)
            {
                Status = "F";
                message = "Order Date is Locked By Period Lock System...!";
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
                var Areas = GetChildGrp( mbranchcode);
                var list = ctxTFAT.Consigner.ToList();
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
                consigners = ctxTFAT.Consigner.ToList();
            }

            if (!(String.IsNullOrEmpty(term)))
            {
                consigners = consigners.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }

            var Modified = consigners.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
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
                var Areas = GetChildGrp( mbranchcode);

                var list = ctxTFAT.Consigner.ToList();
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
                consigners = ctxTFAT.Consigner.ToList();
            }


            if (!(String.IsNullOrEmpty(term)))
            {
                consigners = consigners.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }

            var Modified = consigners.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
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
            var list = ctxTFAT.UnitMaster.ToList().Distinct();


            //var list = ctxTFAT.KilometerMaster.Join(ctxTFAT.TreeTable, p => p.From, pc => pc.Code, (p, pc) => new { p, pc }).Where(x =>  (!(x.pc.Code.ToLower().Contains("h"))) && (!(x.pc.Code.ToLower().Contains("z")))).Select(X=>X.pc).ToList().Distinct();
            if (!(String.IsNullOrEmpty(term)))
            {
                list = ctxTFAT.UnitMaster.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList().Distinct();
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

            string Addr = "", Addr1 = "", Addr2 = "";
            if (!(string.IsNullOrEmpty(Consigner)))
            {
                Addr = ctxTFAT.Consigner.Where(x => x.Code == Consigner).Select(x => x.Addr1).FirstOrDefault();
                Addr = Addr + " " + ctxTFAT.Consigner.Where(x => x.Code == Consigner).Select(x => x.Addr2).FirstOrDefault();
            }
            if (!(string.IsNullOrEmpty(Consignee)))
            {
                Addr1 = ctxTFAT.Consigner.Where(x => x.Code == Consignee).Select(x => x.Addr1).FirstOrDefault();
                Addr1 = Addr1 + " " + ctxTFAT.Consigner.Where(x => x.Code == Consignee).Select(x => x.Addr2).FirstOrDefault();
            }
            if (!(string.IsNullOrEmpty(BillingParty)))
            {
                Addr2 = ctxTFAT.Consigner.Where(x => x.Code == BillingParty).Select(x => x.Addr1).FirstOrDefault();
            }

            return Json(new { Message = Addr, Message1 = Addr1, Message2 = Addr2, JsonRequestBehavior.AllowGet });
        }

        public ActionResult GetCustomerDetails(string BillingParty)
        {
            bool POnumver = false, BEnumber = false, PartyChallan = false, PartyInvoice = false;
            string Coln = "", Deli = "";
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
                }

            }

            return Json(new { POnumver = POnumver, BEnumber = BEnumber, PartyChallan = PartyChallan, PartyInvoice = PartyInvoice, Coln = Coln, Deli = Deli, JsonRequestBehavior.AllowGet });
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
            var mPrevSrl = GetLastSerial("PickOrder", mbranchcode, "PCK00", mperiod, "RS", DateTime.Now.Date);
            return Convert.ToInt32(mPrevSrl);

            //var newLrNo = ctxTFAT.PickOrder.OrderByDescending(x => x.OrderNo).Select(x => x.OrderNo).Take(1).FirstOrDefault();
            //if (newLrNo == 0)
            //{
            //    return 1;
            //}
            //else
            //{
            //    return (Convert.ToInt32(newLrNo) + 1);
            //}
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

        public ActionResult GetTickler(string Consigner)
        {
            string SpclRemark = "", BlackListRemark = "";
            bool HireSpcl = false, HireBlackList = false;

            Consigner consigner = ctxTFAT.Consigner.Where(x => x.Code == Consigner).FirstOrDefault();

            if (consigner != null)
            {
                if (consigner.RemarkReq)
                {
                    if (!String.IsNullOrEmpty(consigner.Remark))
                    {
                        HireSpcl = true;
                        SpclRemark = consigner.Remark;
                    }
                }
                if (consigner.HoldReq)
                {
                    if (!String.IsNullOrEmpty(consigner.HoldRemark))
                    {
                        HireBlackList = true;
                        BlackListRemark = consigner.HoldRemark;
                    }
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


        public ActionResult CheckManualLR(int lrno, string document)
        {
            string Flag = "F";
            string Msg = "";
            bool checkalloctionFound = false;

            LRSetup lRSetup = ctxTFAT.LRSetup.FirstOrDefault();
            if (lRSetup.ManualLRCheck)
            {
                List<TblBranchAllocation> tblBranchAllocations = new List<TblBranchAllocation>();
                tblBranchAllocations = ctxTFAT.TblBranchAllocation.Where(x => x.Branch == mbranchcode && x.TRN_Type == "LR000").ToList();

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
                LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.LrNo == lrno && x.LrNo.ToString() != document).FirstOrDefault();
                if (lRMaster != null)
                {
                    Flag = "T";
                    Msg = "This LRNo Exist \nSo,Please Change Lr No....!";
                }
            }
            else
            {
                Flag = "T";
                Msg = "Manual Range Not Found....!";
            }

            return Json(new { Message = Flag, Msg = Msg, JsonRequestBehavior.AllowGet });
        }

        public ActionResult CheckAutoLR(int lrno, string document)
        {
            string Flag = "F";
            string Msg = "";

            var lRMaster = ctxTFAT.PickOrder.Where(x => x.OrderNo == lrno && x.OrderNo.ToString() != document).FirstOrDefault();
            if (lRMaster != null)
            {
                Flag = "T";
                Msg = "Please Change Order No....!";
            }
            return Json(new { Message = Flag, Msg = Msg, JsonRequestBehavior.AllowGet });
        }

        public decimal GetChargeValValue(int i, string mfield, int OrderNo)
        {
            string connstring = GetConnectionString();
            string abc;
            var loginQuery3 = @"select " + mfield + i + " from PickOrder where OrderNo=" + OrderNo + " ";

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

        public ActionResult Index(OrderRequestVM mModel)
        {

            Session["TempAttach"] = null;
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;


            #region PickSetUp

            #endregion

            #region Restriction Of Consignor And Decrription
            mModel.ConsignorRestrict = true;
            mModel.DescriptionRestrict = true;
            if (muserid.ToUpper() != "SUPER")
            {
                //1563 description
                //1462 Consignor
                var Tfatmenu = ctxTFAT.TfatMenu.Where(x => x.FormatCode == "ConsignerOrConsignee").FirstOrDefault();
                var Restrictdata = ctxTFAT.UserRights.Where(x => x.MenuID == Tfatmenu.ID && x.Code == muserid).Select(x => x.xAdd).FirstOrDefault();
                mModel.ConsignorRestrict = Restrictdata;
                Tfatmenu = ctxTFAT.TfatMenu.Where(x => x.FormatCode == "DescriptionMaster").FirstOrDefault();
                Restrictdata = ctxTFAT.UserRights.Where(x => x.MenuID == Tfatmenu.ID && x.Code == muserid).Select(x => x.xAdd).FirstOrDefault();
                mModel.DescriptionRestrict = Restrictdata;

            }

            #endregion


            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {

                var mlist = new PickOrder();
                mlist = ctxTFAT.PickOrder.Where(x => (x.TableKey.ToString() == mModel.Document)).FirstOrDefault();
                if (mlist != null)
                {

                    AttachmentVM Att = new AttachmentVM();
                    Att.Type = "PCK00";
                    Att.Srl = mlist.OrderNo.ToString();

                    AttachmentController attachmentC = new AttachmentController();
                    List<AttachmentVM> attachments = attachmentC.GetAttachmentListInEdit(Att);
                    Session["TempAttach"] = attachments;

                    mModel.OrderFor = mlist.OrderFor;
                    mModel.OrderForN =ctxTFAT.TfatBranch.Where(x=>x.Code== mlist.OrderFor).Select(x=>x.Name).FirstOrDefault();

                    mModel.LrContactPerson = mlist.InfoName;
                    mModel.LrContactPersonNo = mlist.InfoContactNO;
                    mModel.LrContactPersonEmailId = mlist.InfoEmailId;

                    if (mlist.AUTHORISE.Substring(0, 1) == "A")
                    {
                        mModel.LockAuthorise = LockAuthorise("LR000", mModel.Mode, mlist.TableKey, mlist.ParentKey);
                    }

                    mModel.PeriodLock = PeriodLock(mlist.Branch, "LR000", mlist.CreateDate);

                    mModel.Branch = mlist.Branch;

                    mModel.DocNo = mlist.DocNo;

                    mModel.OrderNO = mlist.OrderNo;
                    mModel.BookDate = mlist.BookDate.Value.ToShortDateString();
                    mModel.Time = mlist.Time;
                    mModel.DocDate = mlist.CreateDate;

                    mModel.BENumber = mlist.BENumber;
                    mModel.PONumber = mlist.PONumber;
                    mModel.LRMode = mlist.LRMode;
                    mModel.TotQty = mlist.TotQty;
                    mModel.BalQty = mlist.BalQty;
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
                    mModel.ActWt = mlist.ActWt.Value;
                    mModel.ChgWt = mlist.ChgWt.Value;
                    mModel.Amt = Convert.ToDecimal(mlist.Amt.Value.ToString("F"));

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
                        c.Val1 = GetChargeValValue(c.tempId, "Val", mlist.OrderNo);
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
                    mModel.Crossing = mlist.Crossing;
                }
                else
                {
                    mModel.OrderFor = mbranchcode;
                    mModel.OrderForN = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault();
                    var getdetailsOfCurrentBRanch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).FirstOrDefault();

                    mModel.Source = getdetailsOfCurrentBRanch.Code;
                    mModel.Source_Name = getdetailsOfCurrentBRanch.Name;

                    mModel.OrderNO = GetNewCode();
                    mModel.BookDate = DateTime.Now.ToShortDateString().ToString();
                    mModel.Time = DateTime.Now.ToString("HH:mm");
                    mModel.DocDate = DateTime.Now;

                    mModel.LRMode = "G";
                    #region Fresh Charges

                    List<PurchaseVM> objledgerdetail = new List<PurchaseVM>();
                    var trncharges = ctxTFAT.Charges.Where(x => x.MainType == "SL" && x.SubType == "RS" && x.Type == "LR000" && x.DontUse == false).Select(x => new { x.Fld, x.Head, x.EqAmt, x.Equation, x.Code }).ToList();
                    //int fmno = Convert.ToInt32(mlist.OrderNo);
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
            }
            else
            {
                mModel.OrderFor = mbranchcode;
                mModel.OrderForN = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault();

                mModel.OrderNO = GetNewCode();
                var getdetailsOfCurrentBRanch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).FirstOrDefault();
                mModel.Source = getdetailsOfCurrentBRanch.Code;
                mModel.Source_Name = getdetailsOfCurrentBRanch.Name;

                mModel.BookDate = DateTime.Now.ToShortDateString().ToString();
                mModel.Time = DateTime.Now.ToString("HH:mm");
                mModel.DocDate = DateTime.Now;
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

        #region SaveData And Delete

        public ActionResult SaveData(OrderRequestVM mModel)
        {

            OrderRequestVM Model = mModel;
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    if (mModel.Mode == "Delete")
                    {
                        string Msg = DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        if (Msg == "Success")
                        {
                            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, mModel.Document,  DateTime.Now , 0, "", "Delete PickUp / Order Request", "NA");

                            return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { Status = "Error", Message = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    bool mAdd = true;
                    PickOrder pickOrder = new PickOrder();
                    if (ctxTFAT.PickOrder.Where(x => (x.TableKey.ToString() == mModel.Document)).FirstOrDefault() != null)
                    {
                        pickOrder = ctxTFAT.PickOrder.Where(x => (x.TableKey.ToString() == mModel.Document)).FirstOrDefault();
                        mAdd = false;
                    }
                    if (mAdd)
                    {
                        mModel.OrderNO = GetNewCode();
                        pickOrder.OrderNo = Convert.ToInt32(mModel.OrderNO);
                        pickOrder.CreateDate = mModel.DocDate;
                        pickOrder.LoginBranch = mbranchcode;
                        pickOrder.Prefix = mperiod;
                        pickOrder.ParentKey = "PCK00" + mperiod.Substring(0, 2) + mModel.OrderNO;
                        pickOrder.TableKey = "PCK00" + mperiod.Substring(0, 2) + 1.ToString("D3") + mModel.OrderNO;
                        //mModel.Srl = GetLastSerial("Ledger", mbranchcode, mModel.Type, mModel.Prefix, mModel.SubType, Model.DocDate);
                    }
                    //LRMaster
                    {
                        TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.Source).FirstOrDefault();
                        string Parent = "";
                        if (tfatBranch.Category == "Area")
                        {
                            Parent = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.Source).Select(x => x.Grp).FirstOrDefault();
                            if (Parent == "G00000")
                            {
                                Parent = mbranchcode;
                            }
                        }
                        else
                        {
                            Parent = mbranchcode;
                        }
                        pickOrder.Branch = Parent;
                        pickOrder.BookDate = ConvertDDMMYYTOYYMMDD(mModel.BookDate);
                        pickOrder.Time = mModel.Time;
                        pickOrder.DocNo = mModel.DocNo;
                        pickOrder.TotQty = mModel.TotQty;
                        pickOrder.BillBran = mModel.BillBran;
                        pickOrder.BillParty = mModel.BillParty;
                        pickOrder.LRtype = mModel.LRtype;
                        pickOrder.ServiceType = mModel.ServiceType;
                        pickOrder.RecCode = mModel.RecCode;
                        pickOrder.SendCode = mModel.SendCode;
                        pickOrder.Source = mModel.Source;
                        pickOrder.Dest = mModel.Dest;
                        pickOrder.PartyRef = mModel.PartyRef;
                        pickOrder.PartyInvoice = mModel.PartyInvoice;
                        pickOrder.GSTNO = mModel.GSTNO;
                        pickOrder.EwayBill = mModel.EwayBill;
                        pickOrder.VehicleNo = mModel.VehicleNo;
                        pickOrder.ActWt = mModel.ActWt;
                        pickOrder.ChgWt = mModel.ChgWt;
                        pickOrder.Amt = mModel.Amt;
                        pickOrder.LRMode = mModel.LRMode;
                        pickOrder.OrderFor = mModel.OrderFor;

                        pickOrder.Val1 = mModel.Charges.Where(x => x.Fld == "F001").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F001") : 0;
                        pickOrder.Val2 = mModel.Charges.Where(x => x.Fld == "F002").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F002") : 0;
                        pickOrder.Val3 = mModel.Charges.Where(x => x.Fld == "F003").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F003") : 0;
                        pickOrder.Val4 = mModel.Charges.Where(x => x.Fld == "F004").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F004") : 0;
                        pickOrder.Val5 = mModel.Charges.Where(x => x.Fld == "F005").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F005") : 0;
                        pickOrder.Val6 = mModel.Charges.Where(x => x.Fld == "F006").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F006") : 0;
                        pickOrder.Val7 = mModel.Charges.Where(x => x.Fld == "F007").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F007") : 0;
                        pickOrder.Val8 = mModel.Charges.Where(x => x.Fld == "F008").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F008") : 0;
                        pickOrder.Val9 = mModel.Charges.Where(x => x.Fld == "F009").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F009") : 0;
                        pickOrder.Val10 = mModel.Charges.Where(x => x.Fld == "F010").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F010") : 0;
                        pickOrder.Val11 = mModel.Charges.Where(x => x.Fld == "F011").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F011") : 0;
                        pickOrder.Val12 = mModel.Charges.Where(x => x.Fld == "F012").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F012") : 0;
                        pickOrder.Val13 = mModel.Charges.Where(x => x.Fld == "F013").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F013") : 0;
                        pickOrder.Val14 = mModel.Charges.Where(x => x.Fld == "F014").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F014") : 0;
                        pickOrder.Val15 = mModel.Charges.Where(x => x.Fld == "F015").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F015") : 0;
                        pickOrder.Val16 = mModel.Charges.Where(x => x.Fld == "F016").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F016") : 0;
                        pickOrder.Val17 = mModel.Charges.Where(x => x.Fld == "F017").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F017") : 0;
                        pickOrder.Val18 = mModel.Charges.Where(x => x.Fld == "F018").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F018") : 0;
                        pickOrder.Val19 = mModel.Charges.Where(x => x.Fld == "F019").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F019") : 0;
                        pickOrder.Val20 = mModel.Charges.Where(x => x.Fld == "F020").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F020") : 0;
                        pickOrder.Val21 = mModel.Charges.Where(x => x.Fld == "F021").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F021") : 0;
                        pickOrder.Val22 = mModel.Charges.Where(x => x.Fld == "F022").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F022") : 0;
                        pickOrder.Val23 = mModel.Charges.Where(x => x.Fld == "F023").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F023") : 0;
                        pickOrder.Val24 = mModel.Charges.Where(x => x.Fld == "F024").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F024") : 0;
                        pickOrder.Val25 = mModel.Charges.Where(x => x.Fld == "F025").Select(x => x) != null ? GetChargesVal(mModel.Charges, "F025") : 0;

                        pickOrder.DecVal = mModel.DecVal;
                        pickOrder.DescrType = mModel.DescrType;
                        pickOrder.ChgType = mModel.ChgType;
                        pickOrder.UnitCode = mModel.UnitCode;
                        pickOrder.Colln = mModel.Colln;
                        pickOrder.Delivery = mModel.Delivery;
                        pickOrder.FormNo = mModel.FormNo;
                        pickOrder.TransactionAt = mModel.TransactionAt;
                        pickOrder.GstLiable = mModel.GstLiable;
                        pickOrder.Narr = mModel.Narr;
                        pickOrder.TrType = "SLR";
                        pickOrder.ConsignerInfo = mModel.ConsignerEXTRAInfo == null ? "" : mModel.ConsignerEXTRAInfo;
                        pickOrder.ConsigneeInfo = mModel.ConsigneeEXTRAInfo == null ? "" : mModel.ConsigneeEXTRAInfo;
                        pickOrder.BillingPartyInfo = mModel.BillingPartyEXTRAInfo == null ? "" : mModel.BillingPartyEXTRAInfo;
                        pickOrder.ENTEREDBY = muserid;
                        pickOrder.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                        pickOrder.AUTHORISE = mauthorise;
                        pickOrder.AUTHIDS = muserid;
                        pickOrder.Crossing = mModel.Crossing;
                        
                        pickOrder.BENumber = mModel.BENumber;
                        pickOrder.PONumber = mModel.PONumber;

                        pickOrder.InfoContactNO = mModel.LrContactPersonNo;
                        pickOrder.InfoEmailId = mModel.LrContactPersonEmailId;
                        pickOrder.InfoName = mModel.LrContactPerson;


                    }


                    #region Authorisation
                    TfatUserAuditHeader authorisation = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == "PCK00").FirstOrDefault();
                    if (authorisation != null)
                    {
                        var Athorise = SetAuthorisationLogistics(authorisation, pickOrder.TableKey, pickOrder.OrderNo.ToString(), 0, pickOrder.BookDate.Value.ToShortDateString(), pickOrder.Val1, pickOrder.BillParty, mbranchcode);
                        pickOrder.AUTHORISE = Athorise;
                    }
                    #endregion

                    if (mAdd == true)
                    {
                        pickOrder.BalQty = mModel.TotQty;
                        ctxTFAT.PickOrder.Add(pickOrder);
                        AttachmentVM vM = new AttachmentVM();
                        vM.ParentKey = pickOrder.ParentKey;
                        vM.Srl = pickOrder.OrderNo.ToString();
                        SaveAttachment(vM);
                    }
                    else
                    {
                        pickOrder.BalQty = mModel.TotQty;
                        ctxTFAT.Entry(pickOrder).State = EntityState.Modified;
                    }
                    

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, pickOrder.ParentKey, pickOrder.BookDate==null?DateTime.Now: pickOrder.BookDate.Value, 0, pickOrder.BillParty, "Saved PickUp / Order Request", "CA");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Status = "Success", id = "OrderRequest" }, JsonRequestBehavior.AllowGet);
        }

        public void SaveAttachment(AttachmentVM Model)
        {
            var RemoveAttach = ctxTFAT.Attachment.Where(x => x.Srl == Model.Srl && x.Type == "PCK00" && x.Type != "Alert").ToList();
            foreach (var item in RemoveAttach)
            {
                if (System.IO.File.Exists(item.FilePath))
                {
                    System.IO.File.Delete(item.FilePath);
                }
            }
            ctxTFAT.Attachment.RemoveRange(RemoveAttach);
            
            List<AttachmentVM> DocList = new List<AttachmentVM>();
            if (Session["TempAttach"] != null)
            {
                DocList = (List<AttachmentVM>)Session["TempAttach"];
            }

            if (DocList != null && DocList.Count != 0)
            {
                var AttachCode = GetNewAttachCode();

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
                    att.Sno = item.tempId;
                    att.Srl = Model.Srl;
                    att.SrNo = item.tempId;
                    att.TableKey = "PCK00" + mperiod.Substring(0, 2) + item.tempId.ToString("D3") + Model.Srl;
                    att.Type = String.IsNullOrEmpty(item.Type) == true ? "PCK00" : item.Type;
                    att.ParentKey = Model.ParentKey;

                    att.CompCode = mcompcode;
                    att.ExternalAttach = item.ExternalAttach;
                    att.RefDocNo = item.RefCode;
                    att.RefType = item.RefType;
                    ctxTFAT.Attachment.Add(att);
                    ctxTFAT.SaveChanges();
                    System.IO.File.WriteAllBytes(directoryPath, IData);

                    ++AttachCode;
                }
            }

        }
        public string DeleteStateMaster(OrderRequestVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return "Code not Entered..";
            }
            var mList = ctxTFAT.PickOrder.Where(x => x.TableKey.ToString() == mModel.Document).FirstOrDefault();

            if (mModel.Document != null)
            {
                LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.DocNo == mModel.Document).FirstOrDefault();
                if (lRMaster != null)
                {
                    return "Cannot Delete This Pick Order Some Qty Use IN Consignment No : "+lRMaster.LrNo+"......!";
                }

                if (ctxTFAT.PeriodLock.Where(x => x.Branch == mList.Branch && x.LockDate == mList.BookDate && x.Type == "PCK00").FirstOrDefault() != null)
                {
                    return "Cannot Delete LR  Because Of In This LR  Date Lock By Period Lock System..!";
                }


            }

            var AlertLR = ctxTFAT.AlertNoteMaster.Where(x => x.Type.ToString() == "PCK00" && x.TypeCode.Trim() == mModel.Document).FirstOrDefault();
            if (AlertLR != null)
            {
                return "Found AlertNote Please Remove AlertNote First....";
            }

            var RemoveAttach = ctxTFAT.Attachment.Where(x => x.Srl == mModel.Document && x.Type == "PCK00" && x.Type != "Alert").ToList();
            foreach (var item in RemoveAttach)
            {
                if (System.IO.File.Exists(item.FilePath))
                {
                    System.IO.File.Delete(item.FilePath);
                }
            }
            ctxTFAT.Attachment.RemoveRange(RemoveAttach);

            var AuthorisationEntry = ctxTFAT.Authorisation.Where(x => x.Type == "PCK00" && x.Srl == mList.OrderNo.ToString()).FirstOrDefault();
            if (AuthorisationEntry != null)
            {
                ctxTFAT.Authorisation.Remove(AuthorisationEntry);
            }

            ctxTFAT.PickOrder.Remove(mList);
            ctxTFAT.SaveChanges();
            UpdateAuditTrail(mbranchcode, mModel.Mode, "OrderRequest", mList.ParentKey, mList.BookDate == null ? DateTime.Now : mList.BookDate.Value, 0, mList.BillParty, "Saved", "A");

            return "Success";
        }
        

        #endregion SaveData


    }
}