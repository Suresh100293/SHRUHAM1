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
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class BackUpLRController : BaseController
    {

        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;
        private string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        private string LastDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        //ITransactionGridOperation mIlst = new TransactionGridOperation();

        #region Function List

        public ActionResult CheckLRDate(string DocDate, string DocTime)
        {
            string message = "";
            var Date = DocDate.Split('/');
            string[] Time = DocTime.Split(':');
            DateTime Lrdate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Time[0]), Convert.ToInt32(Time[1]), 00);

            var OpeningLrDate = Lrdate.ToShortDateString();
            if (ConvertDDMMYYTOYYMMDD(StartDate) >= ConvertDDMMYYTOYYMMDD(OpeningLrDate))
            {
                message = "T";
            }
            else
            {
                message = "F";
            }

            return Json(new { Message = message, JsonRequestBehavior.AllowGet });
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

        public JsonResult GetConsigner(string term, bool Branch)//Consigner
        {
            List<Consigner> consigners = new List<Consigner>();
            if (Branch)
            {
                var Areas = GetChildGrp(mbranchcode);
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

        public JsonResult GetConsignee(string term, bool Branch)
        {
            List<Consigner> consigners = new List<Consigner>();
            if (Branch)
            {
                var Areas = GetChildGrp(mbranchcode);

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
            //var GetParentCode = ctxTFAT.TfatBranch.Where(x => x.Code == mcompcode).Select(x => x.Code).FirstOrDefault();
            List<TfatBranch> list = GetBranch(mbranchcode);

            list = list.Where(x => x.Category != "0" && x.BranchType == "G" && x.Status == true).ToList();
            var GeneralArea = ctxTFAT.TfatBranch.Where(x => x.Grp == "G00000" && x.Category == "Area" && x.Status == true).ToList();
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




            //return Json(Modified, JsonRequestBehavior.AllowGet);


        }

        public JsonResult To(string term)
        {
            List<TfatBranch> list = ctxTFAT.TfatBranch.Where(x => x.Category != "Zone" && x.Category != "0" && x.Status == true).ToList();

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

        public JsonResult BillatBranch(string term)
        {
            //List<TfatBranch> list = GetBranch(BranchCode);
            List<TfatBranch> list = ctxTFAT.TfatBranch.Where(x => x.Category != "0" && x.Category != "Area" && x.Category != "Zone").ToList();
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
            }
            var Modified = treeTables.Select(x => new
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
                Master master = ctxTFAT.Master.Where(x => x.Code == BillingParty).FirstOrDefault();
                if (master != null)
                {
                    POnumver = master.PORequired;
                    BEnumber = master.BERequired;
                    PartyChallan = master.ChlnRequired;
                    PartyInvoice = master.InvRequired;
                    Coln = master.Collection;
                    Deli = master.Delivery;
                }

            }

            return Json(new { POnumver = POnumver, BEnumber = BEnumber, PartyChallan = PartyChallan, PartyInvoice = PartyInvoice, Coln = Coln, Deli = Deli, JsonRequestBehavior.AllowGet });
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
                return Json(new { Status = "Success", id = master.Code, name = master.Name, JsonRequestBehavior.AllowGet });
            }
            else if (LrtypeName.Trim().ToLower() == "paid")
            {
                Master master = ctxTFAT.Master.Where(x => x.Code == LRSetup.DefaultPaidCustomer).FirstOrDefault();
                return Json(new { Status = "Success", id = master.Code, name = master.Name, JsonRequestBehavior.AllowGet });
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
                    Master master = ctxTFAT.Master.Where(x => x.Code == consigner.Customer).FirstOrDefault();
                    return Json(new { Status = "Success", id = master.Code, name = master.Name, JsonRequestBehavior.AllowGet });
                }
                else if (LRSetup.ToPayCustomer == "Consignee" && (!String.IsNullOrEmpty(Consignee)))
                {
                    Master master = ctxTFAT.Master.Where(x => x.Code == consignee.Customer).FirstOrDefault();
                    return Json(new { Status = "Success", id = master.Code, name = master.Name, JsonRequestBehavior.AllowGet });
                }
                return Json(new { Status = "Error", id = "", name = "", JsonRequestBehavior.AllowGet });

            }
            else if (lRTypeMaster.LRType.Trim().ToLower() == "paid")
            {
                if (LRSetup.PaidCustomer == "Consignor" && (!String.IsNullOrEmpty(Consignor)))
                {
                    Master master = ctxTFAT.Master.Where(x => x.Code == consigner.Customer).FirstOrDefault();
                    return Json(new { Status = "Success", id = master.Code, name = master.Name, JsonRequestBehavior.AllowGet });
                }
                else if (LRSetup.PaidCustomer == "Consignee" && (!String.IsNullOrEmpty(Consignee)))
                {
                    Master master = ctxTFAT.Master.Where(x => x.Code == consignee.Customer).FirstOrDefault();
                    return Json(new { Status = "Success", id = master.Code, name = master.Name, JsonRequestBehavior.AllowGet });
                }
                return Json(new { Status = "Error", id = "", name = "", JsonRequestBehavior.AllowGet });

            }
            return Json(new { Status = "Error", id = "", name = "", JsonRequestBehavior.AllowGet });
        }

        public int GetNewCode()
        {
            int Code = ctxTFAT.LRMaster.Where(x => x.LrGenerate == "A").OrderByDescending(x => x.RECORDKEY).Select(x => x.LrNo).Take(1).FirstOrDefault();
            if (Code == 0)
            {
                LRSetup LRSetup = ctxTFAT.LRSetup.FirstOrDefault();
                return Convert.ToInt32(LRSetup.Srl);
            }
            else
            {
                return (Convert.ToInt32(Code) + 1);
            }
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

        public ActionResult CheckManualLR(int lrno, string document)
        {
            string Flag = "F";
            string Msg = "";

            LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.LrNo == lrno && x.LrNo.ToString() != document).FirstOrDefault();
            if (lRMaster != null)
            {
                Flag = "T";
                Msg = "This LRNo Exist \nSo,Please Change Lr No....!";
            }

            return Json(new { Message = Flag, Msg = Msg, JsonRequestBehavior.AllowGet });
        }

        public ActionResult CheckAutoLR(int lrno, string document)
        {
            string Flag = "F";
            string Msg = "";

            LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.LrNo == lrno && x.LrNo.ToString() != document).FirstOrDefault();
            if (lRMaster != null)
            {
                Flag = "T";
                Msg = "Please Change Lr No....!";
            }
            return Json(new { Message = Flag, Msg = Msg, JsonRequestBehavior.AllowGet });
        }
        #endregion

        #region Index(LIst) 

        public ActionResult Index(GridOption Model)
        {
            TempData.Remove("StoreGridModalForDraft");

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
            TempData["StoreGridModalForDraft"] = Model;

            //var GetActivityPage = ctxTFAT.TfatMenu.Where(x => x.FormatCode.ToLower() == Model.ViewDataId.ToLower() && x.ModuleName == "Transactions" && x.ParentMenu == "Logistics").Select(x => new { x.AllowAdd, x.AllowDelete, x.AllowEdit, x.AllowPrint }).FirstOrDefault();
            //Model.xAdd = GetActivityPage.AllowAdd;
            //Model.xDelete = GetActivityPage.AllowDelete;
            //Model.xEdit = GetActivityPage.AllowEdit;
            //Model.xPrint = GetActivityPage.AllowPrint;

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
            string mopt = "EDVX";
            if (result != null)
            {
                mopt = (result.AllowEdit == false ? "X" : "E") + (result.AllowDelete == false ? "X" : "D") + "XX";
            }

            //return GetGridDataColumns(id, "L", mopt);
            return GetGridDataColumns(id, "L", "XXXX");
        }

        //[HttpPost]
        public ActionResult GetMasterGridData1(GridOption Model)
        {
            //var mList = ctxTFAT.LRSetup.Where(x => (x.RECORDKEY == 1)).Select(x => x.DaysList).FirstOrDefault();
            //int Days = Convert.ToInt32(mList) * (-1);
            //Model.FromDate = DateTime.Now.AddDays(Days).ToShortDateString();
            //Model.ToDate = DateTime.Now.ToShortDateString();

            if (Model.IsFormatSelected == true)
            {
                int noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=0 Where SubCodeOf='" + msubcodeof + "'");
                noOfRowUpdated = ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=-1 Where Code='" + Model.ViewDataId + "'");
            }
            return GetGridReport(Model, "M", "MainType^" + Model.MainType, false, 0);
        }

        #endregion

        #region Lr Index

        public ActionResult Index1(LRVM mModel)
        {
            TempData.Remove("LRAttachmentList");
            TempData.Remove("ConsignerList");

            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "A");
            mdocument = mModel.Document;


            #region LrSetUp
            mModel.LRSetup = ctxTFAT.LRSetup.Where(x => (x.RECORDKEY == 1)).FirstOrDefault();
            #endregion

            #region Restriction Of Consignor And Decrription
            mModel.ConsignorRestrict = true;
            mModel.DescriptionRestrict = true;
            if (muserid.ToUpper() != "SUPER")
            {
                //1563 description
                //1462 Consignor
                var Restrictdata = ctxTFAT.UserRights.Where(x => x.MenuID == 1563 && x.Code == muserid).Select(x => x.xAdd).FirstOrDefault();
                mModel.ConsignorRestrict = Restrictdata;
                Restrictdata = ctxTFAT.UserRights.Where(x => x.MenuID == 1462 && x.Code == muserid).Select(x => x.xAdd).FirstOrDefault();
                mModel.DescriptionRestrict = Restrictdata;

            }


            #endregion


            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete") || (mModel.getRecentLR == true))
            {

                var mlist = new LRMaster();
                if (mModel.getRecentLR)
                {
                    var Area = GetChildGrp(mbranchcode);
                    mlist = ctxTFAT.LRMaster.Where(x => x.AUTHIDS == muserid && Area.Contains(x.Branch)).OrderByDescending(x => x.RECORDKEY).FirstOrDefault();
                }
                else
                {
                    mlist = ctxTFAT.LRMaster.Where(x => (x.LrNo.ToString() == mModel.Document)).FirstOrDefault();
                }
                if (mlist != null)
                {
                    if (mModel.getRecentLR == false)
                    {
                        LRStock openingLr = ctxTFAT.LRStock.Where(x => (x.LrNo.ToString() == mModel.Document) && x.Type == "LR").FirstOrDefault();
                        if (openingLr != null)
                        {
                            mModel.StockQty = openingLr.TotalQty;
                            mModel.Branch = openingLr.Branch;
                            mModel.Branch_Name = ctxTFAT.TfatBranch.Where(x => x.Code == openingLr.Branch).Select(x => x.Name).FirstOrDefault();
                        }

                    }




                    mModel.Branch = mlist.Branch;

                    mModel.DocNo = mlist.DocNo;
                    if (mModel.getRecentLR)
                    {
                        mModel.DocDate = DateTime.Now;
                        //mModel.BookDate = DateTime.Now.ToShortDateString().ToString();
                        mModel.Time = DateTime.Now.ToString("HH:mm");

                    }
                    else
                    {
                        mModel.LrNo = mlist.LrNo;
                        mModel.BookDate = mlist.BookDate.ToShortDateString();
                        mModel.Time = mlist.Time;
                        mModel.DocDate = mlist.CreateDate;
                    }
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
                    mModel.ActWt = mlist.ActWt;
                    mModel.ChgWt = mlist.ChgWt;
                    mModel.Amt = Convert.ToDecimal(mlist.Amt.ToString("F"));

                    #region Charges

                    var getCharList = ctxTFAT.Charges.Where(x => x.MainType == "SL" && x.SubType == "RS" && x.Type == "LR" && x.DontUse == false).Select(x => new { x.Head, x.Fld }).ToArray();
                    var GetValue = ctxTFAT.LRMaster.Where(x => x.LrNo == mlist.LrNo).FirstOrDefault();
                    mModel.ChargeName = getCharList.Select(x => x.Head).ToArray();
                    mModel.Fld = getCharList.Select(x => x.Fld).ToArray();
                    int ArraySize = getCharList.Count();
                    string[] ChargeValue = new string[ArraySize];
                    for (int i = 0; i < ChargeValue.Length; i++)
                    {
                        var Fld = mModel.Fld[i].Substring(2, mModel.Fld[i].Length - 2);
                        var value = "Val" + Convert.ToInt32(Fld);
                        var area = GetValue.GetType().GetProperty(value).GetValue(GetValue, null);
                        ChargeValue[i] = area.ToString();
                    }
                    mModel.ChargeValue = ChargeValue;

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

                    #region Attachment
                    //var AttachmentList = ctxTFAT.Attachment.Where(x => (x.ParentCode.ToString() == mModel.LrNo.ToString()) && x.Module == "LR").ToList();
                    //if (AttachmentList != null)
                    //{
                    //    List<LRAttachment> attachmentDocumentVMlist = new List<LRAttachment>();
                    //    //byte[] array = Encoding.ASCII.GetBytes(input);
                    //    foreach (var item in AttachmentList)
                    //    {
                    //        LRAttachment attachmentDocumentVM = new LRAttachment
                    //        {
                    //            AttachLrNo = item.Code.ToString(),
                    //            FileName = item.FileName,
                    //            DocumentString = item.DocumentString,
                    //            ContentType = item.ContentType,
                    //            Image = Convert.FromBase64String(item.DocumentString)
                    //        };
                    //        attachmentDocumentVMlist.Add(attachmentDocumentVM);
                    //    }
                    //    TempData["LRAttachmentList"] = attachmentDocumentVMlist;
                    //    mModel.attachments = attachmentDocumentVMlist;
                    //}
                    #endregion

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
                    //mModel.BookDate = DateTime.Now.ToShortDateString().ToString();
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



                        mModel.BillBran = mModel.LRSetup.BillBranch;
                        mModel.BillBran_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.BillBran).Select(x => x.Name).FirstOrDefault();

                        mModel.Colln = mModel.LRSetup.Colln;
                        mModel.Delivery = mModel.LRSetup.Del;
                    }
                    TempData["LRAttachmentList"] = mModel.attachments;
                    mModel.LRMode = "G";
                    #region Fresh Charges
                    var getCharList = ctxTFAT.Charges.Where(x => x.MainType == "SL" && x.SubType == "RS" && x.Type == "LR" && x.DontUse == false).Select(x => new { x.Head, x.Fld }).ToArray();
                    int ArraySize = getCharList.Count();
                    string[] ChargeValue = new string[ArraySize];
                    for (int i = 0; i < ChargeValue.Length; i++)
                    {
                        ChargeValue[i] = 0.ToString();
                    }
                    mModel.ChargeName = getCharList.Select(x => x.Head).ToArray();
                    mModel.Fld = getCharList.Select(x => x.Fld).ToArray();
                    mModel.ChargeValue = ChargeValue;

                    #endregion
                }
            }
            else if ((mModel.Mode == "Select"))
            {
                var mlist1 = ctxTFAT.LRMasterDraft.Where(x => (x.LrNo.ToString() == mModel.Document)).FirstOrDefault();
                if (mlist1 != null)
                {
                    mModel.PONumber = mlist1.PONumber;
                    mModel.BENumber = mlist1.BENumber;
                    mModel.Draft_Name = mlist1.DraftName;
                    mModel.Branch = mlist1.Branch;
                    //mModel.BookDate = DateTime.Now.ToShortDateString();
                    mModel.Time = DateTime.Now.ToString("HH:mm");
                    mModel.DocDate = DateTime.Now;
                    mModel.DocNo = mlist1.DocNo;
                    mModel.TotQty = Convert.ToInt32(mlist1.TotQty);
                    mModel.BillBran = mlist1.BillBran;
                    mModel.BillBran_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mlist1.BillBran).Select(x => x.Name).FirstOrDefault();
                    mModel.BillParty = mlist1.BillParty;
                    mModel.BillParty_Name = ctxTFAT.CustomerMaster.Where(x => x.Code == mlist1.BillParty).Select(x => x.Name).FirstOrDefault();
                    mModel.LRtype = mlist1.LRtype;
                    mModel.LRtype_Name = ctxTFAT.LRTypeMaster.Where(x => x.Code == mlist1.LRtype).Select(x => x.LRType).FirstOrDefault();
                    mModel.ServiceType = mlist1.ServiceType;
                    mModel.ServiceType_Name = ctxTFAT.ServiceTypeMaster.Where(x => x.Code == mlist1.ServiceType).Select(x => x.ServiceType).FirstOrDefault();
                    mModel.RecCode = mlist1.RecCode;
                    mModel.RecCode_Name = ctxTFAT.Consigner.Where(x => x.Code == mlist1.RecCode).Select(x => x.Name).FirstOrDefault();
                    mModel.SendCode = mlist1.SendCode;
                    mModel.SendCode_Name = ctxTFAT.Consigner.Where(x => x.Code == mlist1.SendCode).Select(x => x.Name).FirstOrDefault();
                    mModel.Source = mlist1.Source;
                    mModel.Source_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mlist1.Source).Select(x => x.Name).FirstOrDefault();
                    mModel.Dest = mlist1.Dest;
                    mModel.Dest_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mlist1.Dest).Select(x => x.Name).FirstOrDefault();
                    mModel.PartyRef = mlist1.PartyRef;
                    mModel.PartyInvoice = mlist1.PartyInvoice;
                    mModel.GSTNO = mlist1.GSTNO;
                    mModel.EwayBill = mlist1.EwayBill;
                    mModel.VehicleNo = mlist1.VehicleNo;
                    mModel.ActWt = Convert.ToDouble(mlist1.ActWt);
                    mModel.ChgWt = Convert.ToDouble(mlist1.ChgWt);
                    mModel.Amt = Convert.ToDecimal(Convert.ToDecimal(mlist1.Amt).ToString("F"));
                    mModel.LRMode = "G";
                    #region Charges

                    var getCharList = ctxTFAT.Charges.Where(x => x.MainType == "SL" && x.SubType == "RS" && x.Type == "LR" && x.DontUse == false).Select(x => new { x.Head, x.Fld }).ToArray();
                    var GetValue = ctxTFAT.LRMasterDraft.Where(x => x.LrNo.ToString() == mModel.Document).FirstOrDefault();
                    mModel.ChargeName = getCharList.Select(x => x.Head).ToArray();
                    mModel.Fld = getCharList.Select(x => x.Fld).ToArray();
                    int ArraySize = getCharList.Count();
                    string[] ChargeValue = new string[ArraySize];
                    for (int i = 0; i < ChargeValue.Length; i++)
                    {
                        var Fld = mModel.Fld[i].Substring(2, mModel.Fld[i].Length - 2);
                        var value = "Val" + Convert.ToInt32(Fld);
                        var area = GetValue.GetType().GetProperty(value).GetValue(GetValue, null) == null ? "0" : GetValue.GetType().GetProperty(value).GetValue(GetValue, null);
                        ChargeValue[i] = area.ToString();
                    }
                    mModel.ChargeValue = ChargeValue;

                    #endregion

                    mModel.DecVal = (decimal)(mlist1.DecVal);
                    mModel.DescrType = mlist1.DescrType;
                    mModel.DescrType_Name = ctxTFAT.DescriptionMaster.Where(x => x.Code == mlist1.DescrType).Select(x => x.Description).FirstOrDefault();
                    mModel.ChgType = mlist1.ChgType;
                    mModel.ChgType_Name = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == mlist1.ChgType).Select(x => x.ChargeType).FirstOrDefault();
                    mModel.UnitCode = mlist1.UnitCode;
                    mModel.UnitCode_Name = ctxTFAT.UnitMaster.Where(x => x.Code == mlist1.UnitCode).Select(x => x.Name).FirstOrDefault();
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

            }
            else
            {

                var getdetailsOfCurrentBRanch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).FirstOrDefault();
                if (getdetailsOfCurrentBRanch.Category != "0" || getdetailsOfCurrentBRanch.Category != "Zone")
                {
                    mModel.Source = getdetailsOfCurrentBRanch.Code;
                    mModel.Source_Name = getdetailsOfCurrentBRanch.Name;
                }


                //mModel.BookDate = DateTime.Now.ToShortDateString().ToString();
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



                    mModel.BillBran = mModel.LRSetup.BillBranch;
                    mModel.BillBran_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.BillBran).Select(x => x.Name).FirstOrDefault();

                    mModel.Colln = mModel.LRSetup.Colln;
                    mModel.Delivery = mModel.LRSetup.Del;

                    if (mModel.LRSetup.LRBoth == true || mModel.LRSetup.LRGenerate == true)
                    {
                        //mModel.LrNo = GetNewCode();
                    }


                }
                TempData["LRAttachmentList"] = mModel.attachments;
                mModel.LRMode = "G";
                #region Fresh Charges
                var getCharList = ctxTFAT.Charges.Where(x => x.MainType == "SL" && x.SubType == "RS" && x.Type == "LR" && x.DontUse == false).Select(x => new { x.Head, x.Fld }).ToArray();
                int ArraySize = getCharList.Count();
                string[] ChargeValue = new string[ArraySize];
                for (int i = 0; i < ChargeValue.Length; i++)
                {
                    ChargeValue[i] = 0.ToString();
                }
                mModel.ChargeName = getCharList.Select(x => x.Head).ToArray();
                mModel.Fld = getCharList.Select(x => x.Fld).ToArray();
                mModel.ChargeValue = ChargeValue;

                #endregion
            }

            if (mModel.LRSetup == null)
            {
                mModel.LRSetup = new LRSetup();
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
            mmodule = "ControlPanel";
            ViewBag.id = "LorryDraft";
            ViewBag.ViewDataId = "LorryDraft";
            ViewBag.Header = "LorryDraft";
            ViewBag.Table = "LRMaster_Draft";
            ViewBag.Controller = "LorryReceipt";
            ViewBag.MainType = "M";
            ViewBag.Controller2 = "LorryReceipt";
            ViewBag.OptionType = "";
            ViewBag.OptionCode = "LorryDraft";
            ViewBag.Module = "ControlPanel";
            ViewBag.ViewName = "";
            ViewBag.ViewDataId1 = "LorryDraft";

            var html = ViewHelper.RenderPartialView(this, "LorryDraftIndex", Model);
            var jsonResult = Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public ActionResult DraftIndex(LRVM mModel)
        {
            //LogisticsGetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "A");
            mdocument = mModel.Document;


            #region LrSetUp
            mModel.LRSetup = ctxTFAT.LRSetup.Where(x => (x.RECORDKEY == 1)).FirstOrDefault();
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
                    //mModel.BookDate = DateTime.Now.ToShortDateString();
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
                    mModel.ActWt = Convert.ToDouble(mlist.ActWt);
                    mModel.ChgWt = Convert.ToDouble(mlist.ChgWt);
                    mModel.Amt = Convert.ToDecimal(Convert.ToDecimal(mlist.Amt).ToString("F"));
                    mModel.LRMode = mlist.LRMode;
                    #region Charges

                    var getCharList = ctxTFAT.Charges.Where(x => x.MainType == "SL" && x.SubType == "RS" && x.Type == "LR" && x.DontUse == false).Select(x => new { x.Head, x.Fld }).ToArray();
                    var GetValue = ctxTFAT.LRMasterDraft.Where(x => x.LrNo.ToString() == mModel.Document).FirstOrDefault();
                    mModel.ChargeName = getCharList.Select(x => x.Head).ToArray();
                    mModel.Fld = getCharList.Select(x => x.Fld).ToArray();
                    int ArraySize = getCharList.Count();
                    string[] ChargeValue = new string[ArraySize];
                    for (int i = 0; i < ChargeValue.Length; i++)
                    {
                        var Fld = mModel.Fld[i].Substring(2, mModel.Fld[i].Length - 2);
                        var value = "Val" + Convert.ToInt32(Fld);
                        var area = GetValue.GetType().GetProperty(value).GetValue(GetValue, null) == null ? "0" : GetValue.GetType().GetProperty(value).GetValue(GetValue, null);
                        ChargeValue[i] = area.ToString();
                    }
                    mModel.ChargeValue = ChargeValue;

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
                    //mModel.DeliveryAt = mlist.DeliveryAt;
                    //mModel.DeliveryAt_Name = ctxTFAT.TfatBranch.Where(x => x.Code == mlist.DeliveryAt).Select(x => x.Name).FirstOrDefault();
                    //mModel.DeliveryTxt = mlist.DeliveryTxt;
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
                    //mModel.MaxDate = ((ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString()).AddDays(mModel.LRSetup.After_LR_Date)).ToShortDateString().ToString());
                    //mModel.MinDate = ((ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString()).AddDays((mModel.LRSetup.Before_LR_Date) * (-1))).ToShortDateString().ToString());
                }



            }
            else
            {
                mModel.BookDate = DateTime.Now.ToShortDateString().ToString();
                mModel.Time = DateTime.Now.ToString("HH:mm");


                mModel.LRtype = mModel.LRSetup.LRType;
                mModel.LRtype_Name = ctxTFAT.LRTypeMaster.Where(x => x.Code == mModel.LRSetup.LRType).Select(x => x.LRType).FirstOrDefault();

                mModel.ServiceType = mModel.LRSetup.ServiceType;
                mModel.ServiceType_Name = ctxTFAT.ServiceTypeMaster.Where(x => x.Code == mModel.LRSetup.ServiceType).Select(x => x.ServiceType).FirstOrDefault();
                TempData["LRAttachmentList"] = mModel.attachments;
                mModel.LRMode = "G";
                #region Fresh Charges
                var getCharList = ctxTFAT.Charges.Where(x => x.MainType == "SL" && x.SubType == "RS" && x.Type == "LR" && x.DontUse == false).Select(x => new { x.Head, x.Fld }).ToArray();
                int ArraySize = getCharList.Count();
                string[] ChargeValue = new string[ArraySize];
                for (int i = 0; i < ChargeValue.Length; i++)
                {
                    ChargeValue[i] = 0.ToString();
                }
                mModel.ChargeName = getCharList.Select(x => x.Head).ToArray();
                mModel.Fld = getCharList.Select(x => x.Fld).ToArray();
                mModel.ChargeValue = ChargeValue;
                // mModel.LrNo = GetNewCodeDraft();
                #endregion
            }

            return View(mModel);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
            return GetGridDataColumns(id, "L", "EDVX");
        }

        #endregion


        #region SaveData And Delete

        public ActionResult SaveData(LRVM mModel)
        {

            LRVM Model = mModel;
            var getCharList = ctxTFAT.Charges.Where(x => x.MainType == "SL" && x.SubType == "RS" && x.Type == "LR" && x.DontUse == false).Select(x => new { x.Head, x.Fld }).ToArray();
            var GetValue = ctxTFAT.LRMaster.Where(x => x.LrNo == Model.LrNo).FirstOrDefault();
            Model.ChargeName = getCharList.Select(x => x.Head).ToArray();
            Model.Fld = getCharList.Select(x => x.Fld).ToArray();
            Model.LRSetup = ctxTFAT.LRSetup.Where(x => (x.RECORDKEY == 1)).FirstOrDefault();

            string[] ChargeValue1 = Model.Val1.ToString().Split(',');
            string[] FldValue1 = Model.FldValue.ToString().Split(',');
            Model.ChargeValue = ChargeValue1;

            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    if (mModel.Document != null)
                    {
                        var FmRelation = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == mModel.Document).FirstOrDefault();
                        if (FmRelation != null && FmRelation.DispachLC != 0)
                        {
                            transaction.Rollback();
                            return Json(new { Message = "Cannot Change LR Details Because Of In This LR  Load It In LC.", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                        }
                        if (FmRelation != null && FmRelation.UnbillQty != FmRelation.TotQty)
                        {
                            return Json(new { Message = "Cannot Change LR Details Because Of In This LR  Bill Generated.", Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                        }
                    }



                    if (mModel.Mode == "Delete")
                    {
                        string Msg = DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    bool mAdd = true, mStkAdd = true;
                    LRMaster lRMaster = new LRMaster();
                    LRStock lRStock = new LRStock();
                    OpeningLrMaster openingLrMaster = new OpeningLrMaster();

                    if (ctxTFAT.LRMaster.Where(x => (x.LrNo.ToString() == mModel.Document)).FirstOrDefault() != null)
                    {
                        lRMaster = ctxTFAT.LRMaster.Where(x => (x.LrNo.ToString() == mModel.Document)).FirstOrDefault();
                        openingLrMaster = ctxTFAT.OpeningLrMaster.Where(x => (x.LRNO.ToString() == mModel.Document)).FirstOrDefault();
                        mAdd = false;
                    }
                    if (ctxTFAT.LRStock.Where(x => (x.TableKey.ToString() == mModel.Document)).FirstOrDefault() != null)
                    {
                        lRStock = ctxTFAT.LRStock.Where(x => (x.TableKey.ToString() == mModel.Document)).FirstOrDefault();
                        mStkAdd = false;
                    }




                    if (mAdd)
                    {
                        lRMaster.LrNo = Convert.ToInt32(mModel.LrNo);
                        lRMaster.CreateDate = mModel.DocDate;
                        lRMaster.LoginBranch = mbranchcode;
                        openingLrMaster.LRNO = mModel.LrNo;
                        lRMaster.LrGenerate = mModel.LrGenerate;
                        lRMaster.ParentKey = mbranchcode + "LR000" + mperiod.Substring(0, 2) + mModel.LrNo;
                        lRMaster.TableKey = mbranchcode + "LR000" + mperiod.Substring(0, 2) + 1.ToString("D3") + mModel.LrNo;
                    }
                    if (mStkAdd)
                    {
                        lRStock.LoginBranch = mbranchcode;
                        lRStock.LrNo = mModel.LrNo;
                        lRStock.ParentKey = lRMaster.TableKey;
                        lRStock.TableKey = mbranchcode + "STK00" + mperiod.Substring(0, 2) + 1.ToString("D3") + lRMaster.LrNo;
                    }

                    //openingLrMaster
                    {
                        openingLrMaster.Qty = mModel.TotQty;
                        openingLrMaster.Branch = mModel.Branch;
                        openingLrMaster.ENTEREDBY = muserid;
                        openingLrMaster.AUTHIDS = muserid;
                        openingLrMaster.AUTHORISE = mauthorise;
                        openingLrMaster.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());

                    }

                    //LRMaster
                    {
                        TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mModel.Source).FirstOrDefault();
                        string Parent = "";
                        if (tfatBranch.Category == "Area")
                        {
                            var child = GetChildGrp(mModel.Source);
                            if (ctxTFAT.TfatBranch.Where(x => child.Contains(x.Code) && x.Category == "SubBranch").FirstOrDefault() != null)
                            {
                                Parent = ctxTFAT.TfatBranch.Where(x => child.Contains(x.Code) && x.Category == "SubBranch").Select(x => x.Code).FirstOrDefault();
                            }
                            else
                            {
                                Parent = ctxTFAT.TfatBranch.Where(x => child.Contains(x.Code) && x.Category == "Branch").Select(x => x.Code).FirstOrDefault();
                            }
                        }
                        else
                        {
                            Parent = mModel.Source;
                        }
                        lRMaster.Branch = Parent;
                        lRMaster.BookDate = ConvertDDMMYYTOYYMMDD(mModel.BookDate);
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

                        string[] ChargeValue = mModel.Val1.ToString().Split(',');
                        string[] FldValue = mModel.FldValue.ToString().Split(',');
                        for (int i = 0; i < ChargeValue.Length; i++)
                        {
                            var Fld = FldValue[i].Substring(2, FldValue[i].Length - 2);
                            var value = "Val" + Convert.ToInt32(Fld);
                            var area = Convert.ToDecimal(ChargeValue[i]);
                            typeof(LRMaster).GetProperty(value).SetValue(lRMaster, area, null);
                        }

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
                        lRMaster.Prefix = mModel.Prefix;
                        lRMaster.TrType = "SLR";
                        lRMaster.ConsignerInfo = mModel.ConsignerEXTRAInfo == null ? "" : mModel.ConsignerEXTRAInfo;
                        lRMaster.ConsigneeInfo = mModel.ConsigneeEXTRAInfo == null ? "" : mModel.ConsigneeEXTRAInfo;
                        lRMaster.BillingPartyInfo = mModel.BillingPartyEXTRAInfo == null ? "" : mModel.BillingPartyEXTRAInfo;
                        lRMaster.DispachLC = mModel.DispachLC;
                        lRMaster.DispachFM = mModel.DispachFM;
                        lRMaster.ENTEREDBY = muserid;
                        lRMaster.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                        lRMaster.AUTHORISE = muserid.Substring(0, 3);
                        lRMaster.AUTHIDS = muserid;
                        lRMaster.POD = true;
                        lRMaster.Crossing = mModel.Crossing;
                        //lRMaster.ParentKey = "LR_" + mModel.LrNo.ToString();
                        //lRMaster.TableKey = "LR_" + mModel.LrNo.ToString();
                        //lRMaster.ParentKey = "LR000" + mperiod.Substring(0, 2) + mModel.LrNo;
                        //lRMaster.TableKey = "LR000" + mperiod.Substring(0, 2) + 1.ToString("D3") + mModel.LrNo;

                        if (mModel.LRtype.Trim() != "100003")
                        {
                            lRMaster.UnbillQty = lRMaster.TotQty;
                        }
                        else
                        {
                            lRMaster.UnbillQty = 0;
                        }



                    }

                    //LRStock
                    //if (!String.IsNullOrEmpty(mModel.Branch))
                    if (Model.StockQty > 0)
                    {
                        //lRStock.ParentKey = lRMaster.TableKey;
                        //lRStock.TableKey = "LRSTK_" + lRMaster.LrNo.ToString("D6");
                        //lRStock.TableKey = "LRSTK000" + mperiod.Substring(0, 2) + mModel.LrNo;
                        //lRStock.TableKey = "STK00" + mperiod.Substring(0, 2) + 1.ToString("D3") + mModel.LrNo;
                        lRStock.Type = "LR";
                        lRStock.Branch = mModel.Branch;
                        lRStock.Date = ConvertDDMMYYTOYYMMDD(mModel.BookDate);
                        lRStock.Time = mModel.Time;
                        lRStock.TotalQty = Convert.ToInt32(mModel.StockQty);
                        lRStock.BalQty = Convert.ToInt32(mModel.StockQty);
                        lRStock.AllocatBalQty = Convert.ToInt32(mModel.StockQty);
                        double Weight = Convert.ToDouble(Math.Round((Convert.ToDecimal(mModel.StockQty)) / ((decimal)mModel.TotQty) * ((decimal)mModel.ActWt)));

                        lRStock.AllocatBalWght = Weight;
                        lRStock.BalWeight = Weight;
                        lRStock.ChrgWeight = Weight;
                        lRStock.ActWeight = Weight;

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
                        lRStock.AUTHORISE = mauthorise;
                        lRStock.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());

                        if (mStkAdd)
                        {
                            lRStock.LRRefTablekey = lRMaster.TableKey;
                            ctxTFAT.LRStock.Add(lRStock);
                        }
                        else
                        {
                            ctxTFAT.Entry(lRStock).State = EntityState.Modified;
                        }

                    }


                    //AttachmentList
                    {
                        //var AttachmentList = ctxTFAT.Attachment.Where(x => (x.ParentCode.ToString() == lRMaster.LrNo.ToString()) && x.Module == "LR").ToList();
                        //foreach (var item in AttachmentList)
                        //{
                        //    ctxTFAT.Attachment.Remove(item);
                        //}
                        List<LRAttachment> SessionAttachList = TempData.Peek("LRAttachmentList") as List<LRAttachment>;

                        if (SessionAttachList != null)
                        {
                            //var LastCode = ctxTFAT.Attachment.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).Take(1).FirstOrDefault();
                            //var Code = Convert.ToInt32(LastCode) + 1;
                            //foreach (var item in SessionAttachList)
                            //{

                            //    Attachment LR_Attachment = new Attachment
                            //    {
                            //        Branch = mbranchcode,
                            //        Module = "LR",
                            //        ParentCode = mModel.LrNo.ToString(),
                            //        Code = Code.ToString(),
                            //        ContentType = item.ContentType,
                            //        DocumentString = item.DocumentString,
                            //        FileName = item.FileName,
                            //        AUTHIDS = muserid,
                            //        AUTHORISE = mAUTHORISE,
                            //        ENTEREDBY = muserid,
                            //        LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString())
                            //    };

                            //    ctxTFAT.Attachment.Add(LR_Attachment);
                            //    ++Code;
                            //}
                        }
                    }



                    if (mAdd == true)
                    {
                        //ctxTFAT.LRMasterDraft.Add(lRMaster_Draft);
                        ctxTFAT.LRMaster.Add(lRMaster);
                        ctxTFAT.OpeningLrMaster.Add(openingLrMaster);
                    }
                    else
                    {
                        ctxTFAT.Entry(lRMaster).State = EntityState.Modified;
                        ctxTFAT.Entry(openingLrMaster).State = EntityState.Modified;
                    }

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, "openingLrMaster ", "", DateTime.Now, 0, "", "", "A");
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

            return Json(new { Status = "Success", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
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
                        lRMaster_Draft.Branch = mModel.Source;
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

                        string[] ChargeValue = mModel.Val1.ToString().Split(',');
                        string[] FldValue = mModel.FldValue.ToString().Split(',');
                        for (int i = 0; i < ChargeValue.Length; i++)
                        {
                            var Fld = FldValue[i].Substring(2, FldValue[i].Length - 2);
                            var value = "Val" + Convert.ToInt32(Fld);
                            var area = Convert.ToDecimal(ChargeValue[i]);
                            typeof(LRMasterDraft).GetProperty(value).SetValue(lRMaster_Draft, area, null);
                        }
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
                    UpdateAuditTrail(mbranchcode, mModel.Mode, "LRMaster_Draft", "", DateTime.Now, 0, "", "", "A");
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

        #endregion SaveData

        #region Delete

        public string DeleteStateMaster(LRVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return "Code not Entered..";
            }
            var mList = ctxTFAT.LRMaster.Where(x => (x.LrNo.ToString() == mModel.Document)).FirstOrDefault();
            var lRStock = ctxTFAT.LRStock.Where(x => (x.LrNo.ToString() == mModel.Document) && x.Branch == mList.Branch).FirstOrDefault();
            var OpenLr = ctxTFAT.OpeningLrMaster.Where(x => x.LRNO.ToString() == mModel.Document).FirstOrDefault();
            //var mList3 = ctxTFAT.LRLockSystem.Where(x => (x.LrNo.ToString() == mModel.Document)).FirstOrDefault();
            //var mList4 = ctxTFAT.Attachment.Where(x => (x.ParentCode.ToString() == mList.AttachmentCode)).ToList();

            //if (mList3.Billing == true || mList3.Delivery == true || mList3.Dispach == true)
            //{
            //    return "PLease Remove Lock Credential First.";
            //}

            //if (mList4.Count > 0)
            //{
            //    return "PLease Remove Attachment First.";

            //}

            if (mModel.Document != null)
            {
                var FmRelation = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == mModel.Document).FirstOrDefault();
                if (FmRelation != null && FmRelation.DispachLC != 0)
                {
                    return "Cannot Delete LR  Because Of In This LR  Load It In LC.";
                }
                if (mList.TotQty != mList.UnbillQty)
                {
                    return "Cannot Delete LR  Because Of In This LR  Bill Generated..!";
                }
            }

            //if (ctxTFAT.LR_PickOrder_Rel.Where(x => (x.LRNo == mModel.Document)).FirstOrDefault() != null)
            //{
            //    var pickorderrelation = ctxTFAT.LR_PickOrder_Rel.Where(x => (x.LRNo == mModel.Document)).FirstOrDefault();

            //    //var LR_PickOrder_Rel = ctxTFAT.LR_PickOrder_Rel.Where(x => (x.LRNo == mModel.Document)).FirstOrDefault();
            //    int orderno = Convert.ToInt32(pickorderrelation.OrderNo);
            //    PickOrder pickOrder1 = ctxTFAT.PickOrder.Where(x => x.OrderNo == orderno).FirstOrDefault();

            //    var BalQty = 0;
            //    var qty = pickOrder1.BalQty + pickorderrelation.Qty;

            //    pickOrder1.BalQty = qty;
            //    if (pickOrder1.BalQty == 0)
            //    {
            //        pickOrder1.OrderFlag = false;
            //    }
            //    else
            //    {
            //        pickOrder1.OrderFlag = true;
            //    }

            //    ctxTFAT.Entry(pickOrder1).State = EntityState.Modified;
            //    ctxTFAT.LR_PickOrder_Rel.Remove(pickorderrelation);
            //}

            ctxTFAT.LRMaster.Remove(mList);
            if (lRStock != null)
            {
                ctxTFAT.LRStock.Remove(lRStock);
            }

            ctxTFAT.OpeningLrMaster.Remove(OpenLr);
            //ctxTFAT.LRLockSystem.Remove(mList3);
            ctxTFAT.SaveChanges();

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

        #region Attachment (Download,View,Delete,Save)

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

        private readonly Random _random = new Random();
        public string RandomString(int size, bool lowerCase = false)
        {
            var builder = new StringBuilder(size);
            // Unicode/ASCII Letters are divided into two blocks
            // (Letters 65–90 / 97–122):     

            // char is a single Unicode character  
            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26; // A...Z or a..z: length = 26  
            for (var i = 0; i < size; i++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }
            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }
        [HttpPost]
        public ActionResult WebCamCaptureImg(string DocumentStr)
        {
            string ContentType = "data:image/png;base64,";
            DocumentStr = DocumentStr.Replace(DocumentStr, "data:image/png;base64,");
            List<LRAttachment> AttachList = new List<LRAttachment>();
            List<LRAttachment> SessionAttachList = TempData.Peek("LRAttachmentList") as List<LRAttachment>;
            if (SessionAttachList != null)
            {
                AttachList = SessionAttachList;
            }
            string FileName = RandomString(10, true);
            LRAttachment attachmentDocumentVM = new LRAttachment
            {
                AttachLrNo = FileName,
                FileName = FileName,
                DocumentString = DocumentStr,
                ContentType = ContentType,
                Image = Convert.FromBase64String(DocumentStr)
            };
            AttachList.Add(attachmentDocumentVM);

            TempData["LRAttachmentList"] = AttachList;
            var html = ViewHelper.RenderPartialView(this, "LRAttachmentView", new LRVM { attachments = AttachList });
            var jsonResult = Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        [HttpPost]
        public ActionResult AttachDocument(LRAttachment model, string DocumentStr, string FileNameStr)
        {
            List<LRAttachment> AttachList = new List<LRAttachment>();
            List<LRAttachment> SessionAttachList = TempData.Peek("LRAttachmentList") as List<LRAttachment>;
            if (SessionAttachList != null)
            {
                AttachList = SessionAttachList;
            }
            MemoryStream memory = new MemoryStream();
            if (AttachList.Where(x => x.FileName == model.UploadFile.FileName).FirstOrDefault() == null)
            {
                model.UploadFile.InputStream.CopyTo(memory);
                byte[] bytes = memory.ToArray();

                LRAttachment PersonalDocument = new LRAttachment
                {
                    AttachLrNo = model.UploadFile.FileName.ToString(),
                    DocumentString = Convert.ToBase64String(bytes),
                    ContentType = model.UploadFile.ContentType,
                    FileName = model.UploadFile.FileName,
                    Image = bytes
                };
                AttachList.Add(PersonalDocument);
            }
            TempData["LRAttachmentList"] = AttachList;
            var html = ViewHelper.RenderPartialView(this, "LRAttachmentView", new LRVM { attachments = AttachList });
            var jsonResult = Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public FileResult Download(string tempId)
        {
            List<LRAttachment> attachlist = new List<LRAttachment>();
            if (TempData["LRAttachmentList"] != null)
            {
                attachlist = TempData.Peek("LRAttachmentList") as List<LRAttachment>;
            }

            var dwnfile = attachlist.Where(x => x.AttachLrNo == tempId).FirstOrDefault();

            string filename = dwnfile.FileName;
            byte[] fileBytes = Convert.FromBase64String(dwnfile.DocumentString);

            return File(fileBytes, dwnfile.ContentType, filename);
        }

        [HttpPost]
        public ActionResult Delete(string tempId)
        {
            string message = "False";

            List<LRAttachment> attachlist = new List<LRAttachment>();
            if (TempData["LRAttachmentList"] != null)
            {
                attachlist = TempData.Peek("LRAttachmentList") as List<LRAttachment>;
            }

            var dwnfile = attachlist.Where(x => x.AttachLrNo == tempId).FirstOrDefault();

            attachlist.Remove(dwnfile);
            message = "True";
            TempData["LRAttachmentList"] = attachlist;
            var html = ViewHelper.RenderPartialView(this, "LRAttachmentView", new LRVM { attachments = attachlist });
            var jsonResult = Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        [HttpPost]
        public ActionResult ViewImage(LRAttachment mModel)
        {
            List<LRAttachment> attachmentDocumentVMs = new List<LRAttachment>();
            attachmentDocumentVMs = TempData.Peek("LRAttachmentList") as List<LRAttachment>;
            LRAttachment attachmentDocument = attachmentDocumentVMs.Where(x => x.AttachLrNo == mModel.AttachLrNo).FirstOrDefault();

            byte[] Image;
            byte[] byteArray = Encoding.UTF8.GetBytes(attachmentDocument.DocumentString);
            MemoryStream stream = new MemoryStream(byteArray);
            using (var binaryReader = new BinaryReader(stream))
            {
                Image = binaryReader.ReadBytes(attachmentDocument.DocumentString.Length);
            }
            byte[] fileBinary = Convert.FromBase64String(attachmentDocument.DocumentString);
            attachmentDocument.Image = fileBinary;
            var html = ViewHelper.RenderPartialView(this, "LRImageView", attachmentDocument);
            var jsonResult = Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        #endregion

        #region Update Lock,Attachment,Note

        public ActionResult SaveNote(LRVM mModel)
        {
            string Status = "Success";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {

                    //LRLockSystem lockSystem = ctxTFAT.LRLockSystem.Where(x => x.LrNo.ToString() == mModel.Document).FirstOrDefault();
                    //lockSystem.Note = mModel.MNote;
                    //ctxTFAT.Entry(lockSystem).State = EntityState.Modified;
                    //ctxTFAT.SaveChanges();
                    //transaction.Commit();
                    //transaction.Dispose();

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
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Status = Status, JsonRequestBehavior.AllowGet });

        }

        public ActionResult SaveAttachment(LRVM mModel)
        {
            string Status = "Success";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == mModel.Document).FirstOrDefault();
                    //if (lRMaster != null)
                    //{
                    //    var AttachmentList = ctxTFAT.Attachment.Where(x => (x.ParentCode.ToString() == lRMaster.AttachmentCode)).ToList();
                    //    foreach (var item in AttachmentList)
                    //    {
                    //        ctxTFAT.Attachment.Remove(item);
                    //    }
                    //    List<LRAttachment> SessionAttachList = TempData.Peek("LRAttachmentList") as List<LRAttachment>;

                    //    if (SessionAttachList != null)
                    //    {
                    //        var LastCode = ctxTFAT.Attachment.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).Take(1).FirstOrDefault();
                    //        var code = Convert.ToInt32(LastCode) + 1;
                    //        foreach (var item in SessionAttachList)
                    //        {

                    //            Attachment LR_Attachment = new Attachment
                    //            {
                    //                ParentCode = lRMaster.AttachmentCode,
                    //                Code = code.ToString(),
                    //                ContentType = item.ContentType,
                    //                DocumentString = item.DocumentString,
                    //                FileName = item.FileName,
                    //                AUTHIDS = muserid,
                    //                AUTHORISE = mAUTHORISE,
                    //                ENTEREDBY = muserid,
                    //                LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString())
                    //            };

                    //            ctxTFAT.Attachment.Add(LR_Attachment);
                    //            ctxTFAT.SaveChanges();
                    //            transaction.Commit();
                    //            transaction.Dispose();
                    //            ++code;
                    //        }
                    //    }
                    //}

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
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Status = Status, JsonRequestBehavior.AllowGet });

        }

        public ActionResult SaveLock(LRVM mModel)
        {
            string Status = "Success";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {


                    //LRLockSystem lockSystem = ctxTFAT.LRLockSystem.Where(x => x.LrNo.ToString() == mModel.Document).FirstOrDefault();

                    //lockSystem.Dispach = mModel.MDispach;
                    //lockSystem.Delivery = mModel.MDelivery;
                    //lockSystem.Billing = mModel.MBilling;
                    //lockSystem.Remark = mModel.MRemark;

                    //ctxTFAT.Entry(lockSystem).State = EntityState.Modified;
                    //ctxTFAT.SaveChanges();
                    //transaction.Commit();
                    //transaction.Dispose();
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
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Status = Status, JsonRequestBehavior.AllowGet });
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
                consignerList.CPhoneNO = item.ContactNO.ToString();
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

        public FileResult Export(LRVM mModel)
        {
            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                StringReader sr = new StringReader(mModel.GridHtml);
                Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 100f, 0f);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();
                //XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                pdfDoc.Close();
                return File(stream.ToArray(), "application/pdf", "Grid.pdf");
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
























    }
}