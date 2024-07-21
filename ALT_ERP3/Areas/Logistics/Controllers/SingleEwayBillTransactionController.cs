using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using ClosedXML.Excel;
using Common;
using EntitiModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class SingleEwayBillTransactionController : BaseController
    {
        private string mdocument = "";
        private static string mauthorise = "A00";

        private static string weburl = "https://api.mastergst.com/";
        private static string authenticateurl = "ewaybillapi/v1.03/authenticate";

        private static string genewaybillurl = "ewaybillapi/v1.03/ewayapi/genewaybill";
        private static string genewayCosoleurl = "ewaybillapi/v1.03/ewayapi/gencewb";
        private static string initmultiurl = "ewaybillapi/v1.03/ewayapi/initmulti";
        private static string addmultiurl = "ewaybillapi/v1.03/ewayapi/addmulti";
        private static string genewayBParturl = "ewaybillapi/v1.03/ewayapi/vehewb";
        private static string extendewayurl = "ewaybillapi/v1.03/ewayapi/extendvalidity";
        private static string cancelewayurl = "ewaybillapi/v1.03/ewayapi/canewb";
        private static string getewaybillurl = "ewaybillapi/v1.03/ewayapi/getewaybill";
        private static string getGSTDetailsurl = "ewaybillapi/v1.03/ewayapi/getgstindetails";
        private static string getEWaybillDetailsurl = "ewaybillapi/v1.03/ewayapi/getewaybillsfortransporter";
        private static string getEWaybillDetailConsignersurl = "ewaybillapi/v1.03/ewayapi/getewaybillsfortransporterbygstin";
        //private static string memail = "laqshya@laqshyalogistics.com";
        //private static string musername = "laqshya_API_ewy";
        //private static string mpassword = "Laqshya@314";
        //private static string mclientid = "97281bee-6db3-4dd6-85e8-84178a06200a";
        //private static string mclientsecret = "71045052-0ca9-4497-85de-62eeb408361c";
        //private static string mgstin = "27AAGFB6750L1Z5";
        //private static string mipaddress = "223.177.54.203";
        private static string memail = "";
        private static string musername = "";
        private static string mpassword = "";
        private static string mclientid = "";
        private static string mclientsecret = "";
        private static string mgstin = "";
        private static string mipaddress = "223.177.54.203";

        #region EwayBill Functions



        public ActionResult GetSuppltType()
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            mList.Add(new SelectListItem { Text = "Inward", Value = "I" });
            mList.Add(new SelectListItem { Text = "Outward", Value = "O" });
            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSubSuppltType(string SupplyType)
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            if (SupplyType == "I")
            {
                mList.Add(new SelectListItem { Text = "Supply", Value = "1" });
                mList.Add(new SelectListItem { Text = "Import", Value = "2" });
                mList.Add(new SelectListItem { Text = "For Own Use", Value = "5" });
                mList.Add(new SelectListItem { Text = "Job work Returns", Value = "6" });
                mList.Add(new SelectListItem { Text = "Sales Return", Value = "7" });
                mList.Add(new SelectListItem { Text = "Others", Value = "8" });
                mList.Add(new SelectListItem { Text = "SKD/CKD/Lots", Value = "9" });
                mList.Add(new SelectListItem { Text = "Exhibition or Fairs", Value = "12" });
            }
            else
            {
                mList.Add(new SelectListItem { Text = "Supply", Value = "1" });
                mList.Add(new SelectListItem { Text = "Export", Value = "3" });
                mList.Add(new SelectListItem { Text = "Job Work", Value = "4" });
                mList.Add(new SelectListItem { Text = "For Own Use", Value = "5" });
                mList.Add(new SelectListItem { Text = "Others", Value = "8" });
                mList.Add(new SelectListItem { Text = "SKD/CKD/Lots", Value = "9" });
                mList.Add(new SelectListItem { Text = "Line Sales", Value = "10" });
                mList.Add(new SelectListItem { Text = "Recipient  Not Known", Value = "11" });
                mList.Add(new SelectListItem { Text = "Exhibition or Fairs", Value = "12" });

            }

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDocType()
        {
            List<SelectListItem> mList = new List<SelectListItem>();

            mList.Add(new SelectListItem { Text = "Tax Invoice", Value = "INV" });
            mList.Add(new SelectListItem { Text = "Bill of Supply", Value = "BIL" });
            mList.Add(new SelectListItem { Text = "Bill of Entry", Value = "BOE" });
            mList.Add(new SelectListItem { Text = "Delivery Challan", Value = "CHL" });
            mList.Add(new SelectListItem { Text = "Others", Value = "OTH" });
            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTrasactType()
        {
            List<SelectListItem> mList = new List<SelectListItem>();

            mList.Add(new SelectListItem { Text = "Regular", Value = "1" });
            mList.Add(new SelectListItem { Text = "Bill To-Ship To", Value = "2" });
            mList.Add(new SelectListItem { Text = "Bill From-Dispatch From", Value = "3" });
            mList.Add(new SelectListItem { Text = "Combination of 2 and 3", Value = "4" });

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTrasactMode()
        {
            List<SelectListItem> mList = new List<SelectListItem>();

            mList.Add(new SelectListItem { Text = "Road", Value = "1" });
            mList.Add(new SelectListItem { Text = "Rail", Value = "2" });
            mList.Add(new SelectListItem { Text = "Air", Value = "3" });
            mList.Add(new SelectListItem { Text = "Ship", Value = "4" });

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetVehicleType()
        {
            List<SelectListItem> mList = new List<SelectListItem>();

            mList.Add(new SelectListItem { Text = "Regular", Value = "R" });
            mList.Add(new SelectListItem { Text = "ODC", Value = "O" });

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetVehicle(string term)
        {
            var list = ctxTFAT.VehicleMaster.Where(x => x.Acitve == true).Select(x => new { x.TruckNo, x.Code, x.TruckStatus }).ToList();
            list.AddRange(ctxTFAT.HireVehicleMaster.Where(x => x.Acitve == true).Select(x => new { x.TruckNo, x.Code, x.TruckStatus }).ToList());

            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.TruckNo.Contains(term.ToLower())).ToList();
            }
            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.TruckNo + (x.TruckStatus == "100001" ? " -H " : x.TruckStatus == "100000" ? " -A" : " -O")
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetReasonCode()
        {
            List<SelectListItem> mList = new List<SelectListItem>();

            mList.Add(new SelectListItem { Text = "Due To Break Down", Value = "1" });
            mList.Add(new SelectListItem { Text = "Due To Transhipment", Value = "2" });
            mList.Add(new SelectListItem { Text = "Others", Value = "3" });
            mList.Add(new SelectListItem { Text = "First Time", Value = "4" });

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult FromPlace(string term)
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

        public JsonResult LoadState(string term)
        {
            string Msg = "";

            var list = ctxTFAT.TfatState.Where(x => x.Code == 1).ToList();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = ctxTFAT.TfatState.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }
            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetExtendReasonCode()
        {
            List<SelectListItem> mList = new List<SelectListItem>();

            mList.Add(new SelectListItem { Text = "Natural Calamity", Value = "1" });
            mList.Add(new SelectListItem { Text = "Law & Order", Value = "2" });
            mList.Add(new SelectListItem { Text = "Transhipment", Value = "3" });
            mList.Add(new SelectListItem { Text = "Accident", Value = "4" });
            mList.Add(new SelectListItem { Text = "Others", Value = "5" });

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetConsignmentStatus()
        {
            List<SelectListItem> mList = new List<SelectListItem>();

            mList.Add(new SelectListItem { Text = "In Transit", Value = "T" });
            mList.Add(new SelectListItem { Text = "In Movement", Value = "M" });

            return Json(mList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCancelReasonCode()
        {
            List<SelectListItem> mList = new List<SelectListItem>();

            mList.Add(new SelectListItem { Text = "Duplicate", Value = "1" });
            mList.Add(new SelectListItem { Text = "Order Cancelled", Value = "2" });
            mList.Add(new SelectListItem { Text = "Data Entry Mistake", Value = "3" });
            mList.Add(new SelectListItem { Text = "Others", Value = "4" });

            return Json(mList, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetConsignerAddress(string Consigner, string addressno)
        {
            string Addr1 = "", Addr2 = "", GSTNo = "", PinCode = "0", StateCode = "", StateName = "";
            if (!(string.IsNullOrEmpty(Consigner)))
            {
                var ConsignerAddress = ctxTFAT.ConsignerAddress.Where(x => x.Code == Consigner && x.Sno.ToString() == addressno).FirstOrDefault();
                if (ConsignerAddress != null)
                {
                    if (!String.IsNullOrEmpty(ConsignerAddress.Addr1))
                    {
                        Addr1 = ConsignerAddress.Addr1;
                    }
                    if (!String.IsNullOrEmpty(ConsignerAddress.Addr2))
                    {
                        Addr2 = ConsignerAddress.Addr2;
                    }
                    if (!String.IsNullOrEmpty(ConsignerAddress.Pin))
                    {
                        PinCode = ConsignerAddress.Pin;
                    }
                    if (!String.IsNullOrEmpty(ConsignerAddress.GSTNo))
                    {
                        GSTNo = ConsignerAddress.GSTNo;
                    }
                    if (!String.IsNullOrEmpty(ConsignerAddress.State))
                    {
                        StateCode = ConsignerAddress.State;
                        StateName = ctxTFAT.TfatState.Where(x => x.Code.ToString() == ConsignerAddress.State).Select(x => x.Name).FirstOrDefault();
                    }
                }
            }

            return Json(new { Addr1 = Addr1, Addr2 = Addr2, PinCode = PinCode, GSTNo = GSTNo, StateCode = StateCode, StateName = StateName, JsonRequestBehavior.AllowGet });
        }
        public ActionResult GetConsigneeAddress(string Consigner, string addressno)
        {
            string Addr1 = "", Addr2 = "", GSTNo = "", PinCode = "0", StateCode = "", StateName = "";
            if (!(string.IsNullOrEmpty(Consigner)))
            {
                var ConsignerAddress = ctxTFAT.ConsignerAddress.Where(x => x.Code == Consigner && x.Sno.ToString() == addressno).FirstOrDefault();
                if (ConsignerAddress != null)
                {
                    if (!String.IsNullOrEmpty(ConsignerAddress.Addr1))
                    {
                        Addr1 = ConsignerAddress.Addr1;
                    }
                    if (!String.IsNullOrEmpty(ConsignerAddress.Addr2))
                    {
                        Addr2 = ConsignerAddress.Addr2;
                    }
                    if (!String.IsNullOrEmpty(ConsignerAddress.Pin))
                    {
                        PinCode = ConsignerAddress.Pin;
                    }
                    if (!String.IsNullOrEmpty(ConsignerAddress.GSTNo))
                    {
                        GSTNo = ConsignerAddress.GSTNo;
                    }
                    if (!String.IsNullOrEmpty(ConsignerAddress.State))
                    {
                        StateCode = ConsignerAddress.State;
                        StateName = ctxTFAT.TfatState.Where(x => x.Code.ToString() == ConsignerAddress.State).Select(x => x.Name).FirstOrDefault();
                    }
                }
            }

            return Json(new { Addr1 = Addr1, Addr2 = Addr2, PinCode = PinCode, GSTNo = GSTNo, StateCode = StateCode, StateName = StateName, JsonRequestBehavior.AllowGet });

        }

        #endregion

        #region EwayBill  Bill GST

        public ActionResult Get_CGST_SGST()
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            mList.Add(new SelectListItem { Text = "0.500 + 0.500", Value = "0.500 + 0.500" });
            mList.Add(new SelectListItem { Text = "0.750 + 0.750", Value = "0.750 + 0.750" });
            mList.Add(new SelectListItem { Text = "3.750 + 3.750", Value = "3.750 + 3.750" });
            mList.Add(new SelectListItem { Text = "0 + 0", Value = "0 + 0" });
            mList.Add(new SelectListItem { Text = "2.500 + 2.500", Value = "2.500 + 2.500" });
            mList.Add(new SelectListItem { Text = "6 + 6", Value = "6 + 6" });
            mList.Add(new SelectListItem { Text = "9 + 9", Value = "9 + 9" });
            mList.Add(new SelectListItem { Text = "14 + 14", Value = "14 + 14" });
            mList.Add(new SelectListItem { Text = "1.500 + 1.500", Value = "1.500 + 1.500" });
            mList.Add(new SelectListItem { Text = "0.050 + 0.050", Value = "0.050 + 0.050" });
            mList.Add(new SelectListItem { Text = "0.125 + 0.125", Value = "0.125 + 0.125" });
            mList.Add(new SelectListItem { Text = "3 + 3", Value = "3 + 3" });
            mList.Add(new SelectListItem { Text = "-Not Appl-", Value = "NA" });
            return Json(mList, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Get_IGST()
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            mList.Add(new SelectListItem { Text = "1", Value = "1" });
            mList.Add(new SelectListItem { Text = "1.500", Value = "1.500" });
            mList.Add(new SelectListItem { Text = "7.500", Value = "7.500" });
            mList.Add(new SelectListItem { Text = "0", Value = "0" });
            mList.Add(new SelectListItem { Text = "5", Value = "5" });
            mList.Add(new SelectListItem { Text = "12", Value = "12" });
            mList.Add(new SelectListItem { Text = "18", Value = "18" });
            mList.Add(new SelectListItem { Text = "28", Value = "28" });
            mList.Add(new SelectListItem { Text = "3", Value = "3" });
            mList.Add(new SelectListItem { Text = "0.100", Value = "0.100" });
            mList.Add(new SelectListItem { Text = "0.250", Value = "0.250" });
            mList.Add(new SelectListItem { Text = "6", Value = "6" });
            mList.Add(new SelectListItem { Text = "-Not Appl-", Value = "NA" });
            return Json(mList, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Get_CESS()
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            mList.Add(new SelectListItem { Text = "0", Value = "0" });
            mList.Add(new SelectListItem { Text = "1", Value = "1" });
            mList.Add(new SelectListItem { Text = "3", Value = "3" });
            mList.Add(new SelectListItem { Text = "5", Value = "5" });
            mList.Add(new SelectListItem { Text = "11", Value = "11" });
            mList.Add(new SelectListItem { Text = "12", Value = "12" });
            mList.Add(new SelectListItem { Text = "12.500", Value = "12.500" });
            mList.Add(new SelectListItem { Text = "15", Value = "15" });
            mList.Add(new SelectListItem { Text = "17", Value = "17" });
            mList.Add(new SelectListItem { Text = "20", Value = "20" });
            mList.Add(new SelectListItem { Text = "21", Value = "21" });
            mList.Add(new SelectListItem { Text = "22", Value = "22" });
            mList.Add(new SelectListItem { Text = "36", Value = "36" });
            mList.Add(new SelectListItem { Text = "49", Value = "49" });
            mList.Add(new SelectListItem { Text = "60", Value = "60" });
            mList.Add(new SelectListItem { Text = "61", Value = "61" });
            mList.Add(new SelectListItem { Text = "65", Value = "65" });
            mList.Add(new SelectListItem { Text = "71", Value = "71" });
            mList.Add(new SelectListItem { Text = "72", Value = "72" });
            mList.Add(new SelectListItem { Text = "89", Value = "89" });
            mList.Add(new SelectListItem { Text = "96", Value = "96" });
            mList.Add(new SelectListItem { Text = "142", Value = "142" });
            mList.Add(new SelectListItem { Text = "160", Value = "160" });
            mList.Add(new SelectListItem { Text = "204", Value = "204" });
            mList.Add(new SelectListItem { Text = "290", Value = "290" });

            mList.Add(new SelectListItem { Text = "-Not Appl-", Value = "NA" });
            return Json(mList, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Get_CESSAdvol()
        {
            List<SelectListItem> mList = new List<SelectListItem>();
            mList.Add(new SelectListItem { Text = "0", Value = "0" });
            mList.Add(new SelectListItem { Text = "400", Value = "400" });
            mList.Add(new SelectListItem { Text = "2076", Value = "2076" });
            mList.Add(new SelectListItem { Text = "2747", Value = "2747" });
            mList.Add(new SelectListItem { Text = "3668", Value = "3668" });
            mList.Add(new SelectListItem { Text = "4006", Value = "4006" });
            mList.Add(new SelectListItem { Text = "4170", Value = "4170" });

            mList.Add(new SelectListItem { Text = "-Not Appl-", Value = "NA" });
            return Json(mList, JsonRequestBehavior.AllowGet);
        }
        #endregion

        private List<SelectListItem> PopulateUsersBranches()
        {
            string muserid = (System.Web.HttpContext.Current.Session["UserId"] ?? "Super").ToString();
            List<SelectListItem> items = new List<SelectListItem>();
            //string constr = ConfigurationManager.ConnectionStrings["tfatEntitiesConnectionString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = " SELECT Code, Name FROM TfatBranch where Code <> 'G00000' and Grp <> 'G00000' and Category <> 'Area' and Users Like '%" + muserid + "%'";
                if (muserid == "Super")
                {
                    query = " SELECT Code, Name FROM TfatBranch where Code <> 'G00000' and Grp <> 'G00000' and Category <> 'Area' ";
                }
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

            items.Add(new SelectListItem
            {
                Text = "General",
                Value = "G00000"
            });
            return items;
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            try
            {
                string Branch = "";
                if (Model.Type == "Active")
                {
                    Model.FromDate = "01/01/1900";
                }
                if (Model.ViewDataId == "ConsoleTFATEWBEway")
                {
                    Model.FromDate = "";
                    Model.ToDate = "";
                }
                bool Empty = false;
                if (Model.ComController)//Zoom Through
                {
                    if (Model.ViewDataId == "ConsoleTFATEWBEway")
                    {
                        if (Model.Customer)
                        {
                            var GetSideBar = ctxTFAT.ActiveSideBarObjects.Where(x => x.Users.Trim() == muserid.Trim() && x.Code == "EwayBillDetails").FirstOrDefault();
                            if (GetSideBar != null)
                            {
                                if (!String.IsNullOrEmpty(GetSideBar.Para10))
                                {
                                    Branch = GetSideBar.Para10;
                                }
                                if (GetSideBar.Para12 == "T")
                                {
                                    Empty = true;
                                }
                            }
                        }
                        else
                        {
                            var GetSideBar = ctxTFAT.ActiveSideBarObjects.Where(x => x.Users.Trim() == muserid.Trim() && x.Code == "EwayBillDetails").FirstOrDefault();
                            if (GetSideBar != null)
                            {
                                if (GetSideBar.Para12 == "T")
                                {
                                    Empty = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        var GetSideBar = ctxTFAT.ActiveSideBarObjects.Where(x => x.Users.Trim() == muserid.Trim() && x.Code == "EwayBillDetails").FirstOrDefault();
                        if (GetSideBar != null)
                        {
                            if (!String.IsNullOrEmpty(GetSideBar.Para10))
                            {
                                Branch = GetSideBar.Para10;
                            }
                            if (GetSideBar.Para12 == "T")
                            {
                                Empty = true;
                            }
                        }
                    }

                }
                else
                {
                    if (Model.ViewDataId == "ConsoleTFATEWBEway")
                    {
                        if (Model.Customer)
                        {
                            var USersBranch = PopulateUsersBranches();
                            Branch = String.Join(",", USersBranch.Select(x => x.Value).ToList());
                            Empty = true;
                        }
                        else
                        {
                            Empty = true;
                        }
                    }
                    else
                    {
                        if (ctxTFAT.tfatEwaySetup.Select(x => x.UserBranch).FirstOrDefault())
                        {
                            var USersBranch = PopulateUsersBranches();
                            Branch = String.Join(",", USersBranch.Select(x => x.Value).ToList());
                            Empty = true;
                        }
                    }

                }

                string ExistEwyBillForConsole = "";
                if (Model.ViewDataId == "ConsoleTFATEWBEway")
                {
                    List<EwayBillVM> ExistEWAY = (List<EwayBillVM>)Session["EwayBillVMCon"];
                    if (ExistEWAY != null)
                    {
                        foreach (var item in ExistEWAY)
                        {
                            ExistEwyBillForConsole += "'" + item.LREwayNo + "',";
                        }
                        ExistEwyBillForConsole = ExistEwyBillForConsole.Substring(0, ExistEwyBillForConsole.Length - 1);
                    }
                }


                ExecuteStoredProc("Drop Table ztmp_tfatEWB");
                SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                SqlCommand cmd = new SqlCommand("SPTFAT_EwatBillList", tfat_conx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;
                cmd.Parameters.Add("@Status", SqlDbType.VarChar).Value = Model.Type;
                cmd.Parameters.Add("@Branch", SqlDbType.VarChar).Value = Branch;
                cmd.Parameters.Add("@ExistEwyBillForConsole", SqlDbType.VarChar).Value = ExistEwyBillForConsole;
                cmd.Parameters.Add("@FromDate", SqlDbType.VarChar).Value = Model.FromDate == "" ? "" : (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                cmd.Parameters.Add("@ToDate", SqlDbType.VarChar).Value = Model.ToDate == "" ? "" : (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                cmd.Parameters.Add("@CurrDate", SqlDbType.VarChar).Value = (Convert.ToDateTime(DateTime.Now.ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                cmd.Parameters.Add("@mSelectQuery", SqlDbType.VarChar, 5000).Value = "";
                cmd.Parameters.Add("@mUserQuery", SqlDbType.VarChar, 5000).Value = "";
                cmd.Parameters.Add("@mTableQuery", SqlDbType.VarChar, 5000).Value = "";
                cmd.Parameters.Add("@mReturnTableQuery", SqlDbType.VarChar, 5000).Value = "";
                cmd.Parameters["@mSelectQuery"].Direction = ParameterDirection.Output;
                cmd.Parameters["@mUserQuery"].Direction = ParameterDirection.Output;
                cmd.Parameters["@mTableQuery"].Direction = ParameterDirection.Output;
                cmd.Parameters["@mReturnTableQuery"].Direction = ParameterDirection.Output;
                tfat_conx.Open();
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
                string mSelectQuery = (string)(cmd.Parameters["@mSelectQuery"].Value ?? "");
                string mUserQuery = (string)(cmd.Parameters["@mUserQuery"].Value ?? "");
                string mTableQuery = (string)(cmd.Parameters["@mTableQuery"].Value ?? "");
                //string mReturnTableQuery = (string)(cmd.Parameters["@mReturnTableQuery"].Value ?? "");

                tfat_conx.Close();
                var Count = 0;
                if (Model.Type != "ALL" && Model.Type != "Clear" && Model.Type != "Expired")
                {
                    ExecuteStoredProc("delete from ztmp_tfatEWB where [Stock Type]='DEL'");
                }
                if (!String.IsNullOrEmpty(Branch))
                {
                    if (Empty)
                    {
                        ExecuteStoredProc("delete from ztmp_tfatEWB where ConsignmentNO is not null and  charindex(StockBranchCode,'" + Branch + "')=0");
                    }
                    else
                    {
                        ExecuteStoredProc("delete from ztmp_tfatEWB where  charindex(StockBranchCode,'" + Branch + "')=0");
                    }
                }

                //ExecuteStoredProc("update ztmp_tfatEWB set [Stock Branch]='' where Stockkey is null");

                string mSQLQuery = "select  count(distinct [EwayBill No]) from ztmp_tfatEWB";
                tfat_conx = new SqlConnection(GetConnectionString());
                cmd = new SqlCommand(mSQLQuery, tfat_conx);
                try
                {
                    tfat_conx.Open();
                    cmd.CommandTimeout = 0;
                    Count = (Int32)cmd.ExecuteScalar();
                }
                catch (Exception mex)
                {
                }
                finally
                {
                    cmd.Dispose();
                    tfat_conx.Close();
                    tfat_conx.Dispose();
                }

                return GetGridDataColumns(Model.ViewDataId, "R", "", Count.ToString(), GetMonthString());
            }
            catch (Exception ex)
            {
                return Json(new { Status = "Error", Message = "Error! While Generating Report..\n" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetGridData(GridOption Model)
        {

            return GetGridReport(Model, "X", "", true, 0);
        }


        // GET: Logistics/SingleEwayBillTransaction
        public ActionResult Index(EwayBillVM Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            Session["EwayBillVMCon"] = null;
            mdocument = Model.Document;
            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            //Model.FromDate = DateTime.Now.ToShortDateString();
            //Model.ToDate = DateTime.Now.ToShortDateString();
            Model.SyncDate = DateTime.Now.ToShortDateString();
            Model.SyncToDate = DateTime.Now.ToShortDateString();
            ViewBag.ViewDataId = Model.ViewDataId;
            ViewBag.Header = Model.Header;
            ViewBag.Controller = Model.Controller;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;

            var setup = ctxTFAT.tfatEwaySetup.FirstOrDefault();
            if (setup == null)
            {
                setup = new tfatEwaySetup();
            }
            //AutoExtendEwayBillsList(Model);
            Model.tfatEwaySetup = setup;
            Model.TotalQty = 0;
            Model.TaxableAmt = 0;

            //mpara = (string.IsNullOrEmpty(Model.Document) == true) ? "" : "para01^" + Model.Document.Substring(6, Model.Document.Length - 6);
            return View(Model);
        }

        #region Common Methods

        public ActionResult FetBranchState(string Branch)
        {
            string StateCode = "", StateName = "", Pin = "";
            TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Branch.Trim()).FirstOrDefault();
            if (tfatBranch != null)
            {
                if (!String.IsNullOrEmpty(tfatBranch.State))
                {
                    TfatState tfatState = ctxTFAT.TfatState.Where(x => x.Code.ToString() == tfatBranch.State.Trim()).FirstOrDefault();
                    if (tfatState != null)
                    {
                        StateCode = tfatState.Code.ToString();
                        StateName = tfatState.Name;
                    }
                }
                Pin = tfatBranch.aPin;
            }

            return Json(new
            {
                StateCode = StateCode,
                StateName = StateName,
                Pin = Pin,
                JsonRequestBehavior.AllowGet
            });
        }

        public List<UnitList> GetUsints()
        {
            var students = new List<UnitList>() {
                new UnitList(){ Code = "BAG", Name="BAGS"},
                new UnitList(){ Code = "BAL", Name="BALE"},
                new UnitList(){ Code = "BDL", Name="BUNDLES"},
                new UnitList(){ Code = "BKL", Name="BUCKLES"},
                new UnitList(){ Code = "BOU", Name="BILLION OF UNITS"},
                new UnitList(){ Code = "BOX", Name="BOX"},
                new UnitList(){ Code = "BTL", Name="BOTTLES"},
                new UnitList(){ Code = "BUN", Name="BUNCHES"},
                new UnitList(){ Code = "CAN", Name="CANS"},
                new UnitList(){ Code = "CBM", Name="CUBIC METERS"},
                new UnitList(){ Code = "CCM", Name="CUBIC CENTIMETERS"},
                new UnitList(){ Code = "CMS", Name="CENTIMETERS"},
                new UnitList(){ Code = "CTN", Name="CARTONS"},
                new UnitList(){ Code = "DOZ", Name="DOZENS"},
                new UnitList(){ Code = "DRM", Name="DRUMS"},
                new UnitList(){ Code = "GGK", Name="GREAT GROSS"},
                new UnitList(){ Code = "GMS", Name="GRAMMES"},
                new UnitList(){ Code = "GRS", Name="GROSS"},
                new UnitList(){ Code = "GYD", Name="GROSS YARDS"},
                new UnitList(){ Code = "KGS", Name="KILOGRAMS"},
                new UnitList(){ Code = "KLR", Name="KILOLITRE"},
                new UnitList(){ Code = "KME", Name="KILOMETRE"},
                new UnitList(){ Code = "LTR", Name="LITRES"},
                new UnitList(){ Code = "MLT", Name="MILILITRE"},
                new UnitList(){ Code = "MTR", Name="METERS"},
                new UnitList(){ Code = "MTS", Name="METRIC TON"},
                new UnitList(){ Code = "NOS", Name="NUMBERS"},
                new UnitList(){ Code = "OTH", Name="OTHERS"},
                new UnitList(){ Code = "PAC", Name="PACKS"},
                new UnitList(){ Code = "PCS", Name="PIECES"},
                new UnitList(){ Code = "PRS", Name="PAIRS"},
                new UnitList(){ Code = "QTL", Name="QUINTAL"},
                new UnitList(){ Code = "ROL", Name="ROLLS"},
                new UnitList(){ Code = "SET", Name="SETS"},
                new UnitList(){ Code = "SQF", Name="SQUARE FEET"},
                new UnitList(){ Code = "SQM", Name="SQUARE METERS"},
                new UnitList(){ Code = "SQY", Name="SQUARE YARDS"},
                new UnitList(){ Code = "TBS", Name="TABLETS"},
                new UnitList(){ Code = "TGM", Name="TEN GROSS"},
                new UnitList(){ Code = "THD", Name="THOUSANDS"},
                new UnitList(){ Code = "TON", Name="TONNES"},
                new UnitList(){ Code = "TUB", Name="TUBES"},
                new UnitList(){ Code = "UGS", Name="US GALLONS"},
                new UnitList(){ Code = "UNT", Name="UNITS"},
                new UnitList(){ Code = "YDS", Name="YARDS"}
            };
            return students;
        }

        public JsonResult GetUnit(string term)//Unit
        {
            var list = GetUsints();

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


        public string SetApiParameters()
        {
            string Message = "";
            TfatComp tfatComp = ctxTFAT.TfatComp.FirstOrDefault();
            if (tfatComp != null)
            {
                if (String.IsNullOrEmpty(tfatComp.EwayEmail))
                {
                    Message += "Email Missing Of Eway Bill...!\n";
                }
                else
                {
                    memail = tfatComp.EwayEmail.Trim();
                }

                if (String.IsNullOrEmpty(tfatComp.EwayUsername))
                {
                    Message += "Username Missing Of Eway Bill...!\n";
                }
                else
                {
                    musername = tfatComp.EwayUsername.Trim();
                }

                if (String.IsNullOrEmpty(tfatComp.EwayPass))
                {
                    Message += "Password Missing Of Eway Bill...!\n";
                }
                else
                {
                    mpassword = tfatComp.EwayPass.Trim();
                }

                if (String.IsNullOrEmpty(tfatComp.EwayClientID))
                {
                    Message += "Client ID Missing Of Eway Bill...!\n";
                }
                else
                {
                    mclientid = tfatComp.EwayClientID.Trim();
                }

                if (String.IsNullOrEmpty(tfatComp.EwayClientSecret))
                {
                    Message += "Client Secret Missing Of Eway Bill...!\n";
                }
                else
                {
                    mclientsecret = tfatComp.EwayClientSecret.Trim();
                }

                if (String.IsNullOrEmpty(tfatComp.EwayGSTIn))
                {
                    Message += "GSTIn Missing Of Eway Bill...!\n";
                }
                else
                {
                    mgstin = tfatComp.EwayGSTIn.Trim();
                }

            }
            else
            {
                Message += "Please Update Ewaybill Details In Company Profile...!\n";
            }


            return Message;
        }

        public string GetToken()
        {
            string mtoken = "";
            try
            {
                bool GetAuthentication = false;
                tfatEWB tfatEWB = ctxTFAT.tfatEWB.Where(x => x.DocType == "AUTH0").FirstOrDefault();
                if (tfatEWB == null)
                {
                    GetAuthentication = true;
                }
                else
                {
                    DateTime CurrentDate1 = DateTime.Now;
                    DateTime authValid = tfatEWB.EWBValid.Value;
                    if (CurrentDate1 >= authValid)
                    {
                        GetAuthentication = true;

                    }
                }
                if (GetAuthentication)
                {
                    var URL = new UriBuilder(weburl + authenticateurl);
                    var queryString = HttpUtility.ParseQueryString(string.Empty);
                    queryString["email"] = memail.Trim();
                    queryString["username"] = musername.Trim();
                    queryString["password"] = mpassword.Trim();
                    URL.Query = queryString.ToString();
                    var client = new WebClient();
                    client.Headers.Add("ip_address", mipaddress.Trim());
                    client.Headers.Add("client_id", mclientid.Trim());
                    client.Headers.Add("client_secret", mclientsecret.Trim());
                    client.Headers.Add("gstin", mgstin.Trim());
                    string mstr = client.DownloadString(URL.ToString());
                    dynamic mjson = Newtonsoft.Json.Linq.JValue.Parse(Newtonsoft.Json.JsonConvert.DeserializeObject(mstr).ToString());
                    mtoken = mjson.GetValue("status_cd").ToString();

                    DateTime dateTime = DateTime.Now.AddHours(6);
                    SaveEWBAuth("AUTH0", "000000", dateTime, "Authe");
                }
                else
                {
                    mtoken = "1";
                }

            }
            catch (Exception mex)
            {
                mtoken = "";
            }
            return mtoken;
        }

        public string GetErrorString(string ErrorCode)
        {
            string Msg = "Get An Error.\n";

            var ErrArray = ErrorCode.Split(',').ToList();
            foreach (var item in ErrArray)
            {
                switch (item)
                {
                    case "100": Msg += "Invalid json\n"; break;
                    case "101": Msg += "Invalid Username\n"; break;
                    case "102": Msg += "Invalid Password\n"; break;
                    case "103": Msg += "Invalid Client -Id\n"; break;
                    case "104": Msg += "Invalid Client -Id\n"; break;
                    case "105": Msg += "Invalid Token\n"; break;
                    case "106": Msg += "Token Expired\n"; break;
                    case "107": Msg += "Authentication failed. Pls. inform the helpdesk\n"; break;
                    case "108": Msg += "Invalid login credentials.\n"; break;
                    case "109": Msg += "Decryption of data failed\n"; break;
                    case "110": Msg += "Invalid Client-ID/Client-Secret\n"; break;
                    case "111": Msg += "GSTIN is not registerd to this GSP\n"; break;
                    case "112": Msg += "IMEI does not belong to the user\n"; break;
                    case "113": Msg += "os-type is mandatory in header\n"; break;
                    case "114": Msg += "Invalid os-type parameter value\n"; break;
                    case "201": Msg += "Invalid Supply Type\n"; break;
                    case "202": Msg += "Invalid Sub-supply Type\n"; break;
                    case "203": Msg += "Sub-transaction type does not belongs to transaction type\n"; break;
                    case "204": Msg += "Invalid Document type\n"; break;
                    case "205": Msg += "Document type does not match with transaction & Sub trans type\n"; break;
                    case "206": Msg += "Invaild Invoice Number\n"; break;
                    case "207": Msg += "Invalid Invoice Date\n"; break;
                    case "208": Msg += "Invalid Supplier GSTIN\n"; break;
                    case "209": Msg += "Blank Supplier Address\n"; break;
                    case "210": Msg += "Invalid or Blank Supplier PIN Code\n"; break;
                    case "211": Msg += "Invalid or Blank Supplier state Code\n"; break;
                    case "212": Msg += "Invalid Consignee GSTIN\n"; break;
                    case "213": Msg += "Invalid Consignee Address\n"; break;
                    case "214": Msg += "Invalid Consignee PIN Code\n"; break;
                    case "215": Msg += "Invalid Consignee State Code\n"; break;
                    case "216": Msg += "Invalid HSN Code\n"; break;
                    case "217": Msg += "Invalid UQC Code\n"; break;
                    case "218": Msg += "Invalid Tax Rate for Intra State Transaction\n"; break;
                    case "219": Msg += "Invalid Tax Rate for Inter State Transaction\n"; break;
                    case "220": Msg += "Invalid Trans mode\n"; break;
                    case "221": Msg += "Invalid Approximate Distance\n"; break;
                    case "222": Msg += "Invalid Transporter Id\n"; break;
                    case "223": Msg += "Invalid Transaction Document Number\n"; break;
                    case "224": Msg += "Invalid Transaction Date\n"; break;
                    case "225": Msg += "Invalid Vehicle Number Format\n"; break;
                    case "226": Msg += "Both Transaction and Vehicle Number Blank\n"; break;
                    case "227": Msg += "User Gstin cannot be blank\n"; break;
                    case "228": Msg += "User id cannot be blank\n"; break;
                    case "229": Msg += "Supplier name is required\n"; break;
                    case "230": Msg += "Supplier place is required\n"; break;
                    case "231": Msg += "Consignee name is required\n"; break;
                    case "232": Msg += "Consignee place is required\n"; break;
                    case "233": Msg += "Eway bill does not contains any items\n"; break;
                    case "234": Msg += "Total amount/Taxable amout is mandatory\n"; break;
                    case "235": Msg += "Tax rates for Intra state transaction is blank\n"; break;
                    case "236": Msg += "Tax rates for Inter state transaction is blank\n"; break;
                    case "237": Msg += "Invalid client -Id/client-secret\n"; break;
                    case "238": Msg += "Invalid auth token\n"; break;
                    case "239": Msg += "Invalid action\n"; break;
                    case "240": Msg += "Could not generate eway bill, pls contact helpdesk\n"; break;
                    case "242": Msg += "Invalid or Blank Officer StateCode\n"; break;
                    case "243": Msg += "Invalid or Blank IR Number\n"; break;
                    case "244": Msg += "Invalid or Blank Actual Vehicle Number Format\n"; break;
                    case "245": Msg += "Invalid Verification Date Format\n"; break;
                    case "246": Msg += "Invalid Vehicle Release Date Format\n"; break;
                    case "247": Msg += "Invalid Verification Time Format\n"; break;
                    case "248": Msg += "Invalid Vehicle Release Date Format\n"; break;
                    case "249": Msg += "Actual Value cannot be less than or equal to zero\n"; break;
                    case "250": Msg += "Invalid Vehicle Release Date Format\n"; break;
                    case "251": Msg += "CGST nad SGST TaxRate should be same\n"; break;
                    case "252": Msg += "Invalid CGST Tax Rate\n"; break;
                    case "253": Msg += "Invalid SGST Tax Rate\n"; break;
                    case "254": Msg += "Invalid IGST Tax Rate\n"; break;
                    case "255": Msg += "Invalid CESS Rate\n"; break;
                    case "256": Msg += "Invalid Cess Non Advol value\n"; break;
                    case "278": Msg += "User Gstin does not match with Transporter Id\n"; break;
                    case "280": Msg += "Status is not ACTIVE\n"; break;
                    case "281": Msg += "Eway Bill is already expired hence update transporter is not allowed.\n"; break;
                    case "301": Msg += "Invalid eway bill number\n"; break;
                    case "302": Msg += "Invalid transporter mode\n"; break;
                    case "303": Msg += "Vehicle number is required\n"; break;
                    case "304": Msg += "Invalid vehicle format\n"; break;
                    case "305": Msg += "Place from is required\n"; break;
                    case "306": Msg += "Invalid from state\n"; break;
                    case "307": Msg += "Invalid reason\n"; break;
                    case "308": Msg += "Invalid remarks\n"; break;
                    case "309": Msg += "Could not update vehicle details, pl contact helpdesk\n"; break;
                    case "311": Msg += "Validity period lapsed, you cannot update vehicle details\n"; break;
                    case "312": Msg += "This eway bill is either not generated by you or cancelled\n"; break;
                    case "313": Msg += "Error in validating ewaybill for vehicle updation\n"; break;
                    case "315": Msg += "Validity period lapsed, you cannot cancel this eway bill\n"; break;
                    case "316": Msg += "Eway bill is already verified, you cannot cancel it\n"; break;
                    case "317": Msg += "Could not cancel eway bill, please contact helpdesk\n"; break;
                    case "320": Msg += "Invalid state to\n"; break;
                    case "321": Msg += "Invalid place to\n"; break;
                    case "322": Msg += "Could not generate consolidated eway bill\n"; break;
                    case "325": Msg += "Could not retrieve data\n"; break;
                    case "326": Msg += "Could not retrieve GSTIN details for the given GSTIN number\n"; break;
                    case "327": Msg += "Could not retrieve data from hsn\n"; break;
                    case "328": Msg += "Could not retrieve transporter details from gstin\n"; break;
                    case "329": Msg += "Could not retrieve States List\n"; break;
                    case "330": Msg += "Could not retrieve UQC list\n"; break;
                    case "331": Msg += "Could not retrieve Error code\n"; break;
                    case "334": Msg += "Could not retrieve user details by userid \n"; break;
                    case "336": Msg += "Could not retrieve transporter data by gstin \n"; break;
                    case "337": Msg += "Could not retrieve HSN details for the given HSN number\n"; break;
                    case "338": Msg += "You cannot update transporter details, as the current tranporter is already entered Part B details of the eway bill\n"; break;
                    case "339": Msg += "You are not assigned to update the tranporter details of this eway bill\n"; break;
                    case "341": Msg += "This e-way bill is generated by you and hence you cannot reject it\n"; break;
                    case "342": Msg += "You cannot reject this e-way bill as you are not the other party to do so\n"; break;
                    case "343": Msg += "This e-way bill is cancelled\n"; break;
                    case "344": Msg += "Invalid eway bill number\n"; break;
                    case "345": Msg += "Validity period lapsed, you cannot reject the e-way bill\n"; break;
                    case "346": Msg += "You can reject the e-way bill only within 72 hours from generated time\n"; break;
                    case "347": Msg += "Validation of eway bill number failed, while rejecting ewaybill\n"; break;
                    case "348": Msg += "Part-B is not generated for this e-way bill, hence rejection is not allowed.\n"; break;
                    case "350": Msg += "Could not generate consolidated eway bill\n"; break;
                    case "351": Msg += "Invalid state code\n"; break;
                    case "352": Msg += "Invalid rfid date\n"; break;
                    case "353": Msg += "Invalid location code\n"; break;
                    case "354": Msg += "Invalid rfid number\n"; break;
                    case "355": Msg += "Invalid Vehicle Number Format\n"; break;
                    case "356": Msg += "Invalid wt on bridge\n"; break;
                    case "357": Msg += "Could not retrieve eway bill details, pl. contact helpdesk\n"; break;
                    case "358": Msg += "GSTIN passed in request header is not matching with the user gstin mentioned in payload JSON\n"; break;
                    case "359": Msg += "User GSTIN should match to GSTIN(from) for outward transactions\n"; break;
                    case "360": Msg += "User GSTIN should match to GSTIN(to) for inward transactions\n"; break;
                    case "361": Msg += "Invalid Vehicle Type\n"; break;
                    case "362": Msg += "Transporter document date cannot be earlier than the invoice date\n"; break;
                    case "363": Msg += "E-way bill is not enabled for intra state movement for you state\n"; break;
                    case "364": Msg += "Error in verifying eway bill\n"; break;
                    case "365": Msg += "Error in verifying consolidated eway bill\n"; break;
                    case "366": Msg += "You will not get the ewaybills generated today, howerver you cann access the ewaybills of yester days\n"; break;
                    case "367": Msg += "Could not retrieve data for officer login\n"; break;
                    case "368": Msg += "Could not update transporter\n"; break;
                    case "369": Msg += "GSTIN/Transin passed in request header should match with the transported Id mentioned in payload JSON\n"; break;
                    case "370": Msg += "GSTIN/Transin passed in request header should not be the same as supplier(fromGSTIN) or recepient(toGSTIN)\n"; break;
                    case "371": Msg += "Invalid or Blank Supplier Ship-to State Code\n"; break;
                    case "372": Msg += "Invalid or Blank Consignee Ship-to State Code\n"; break;
                    case "373": Msg += "The Supplier ship-to state code should be Other Country for Sub Supply Type- Export\n"; break;
                    case "374": Msg += "The Consignee pin code should be 999999 for Sub Supply Type- Export\n"; break;
                    case "375": Msg += "The Supplier ship-from state code should be Other Country for Sub Supply Type- Import\n"; break;
                    case "376": Msg += "The Supplier pin code should be 999999 for Sub Supply Type- Import\n"; break;
                    case "377": Msg += "Sub Supply Type is mentioned as Others, the description for that is mandatory\n"; break;
                    case "378": Msg += "The supplier or conginee belong to SEZ, Inter state tax rates are applicable here\n"; break;
                    case "379": Msg += "Eway Bill can not be extended.. Already Cancelled\n"; break;
                    case "380": Msg += "Eway Bill Can not be Extended. Not in Active State\n"; break;
                    case "381": Msg += "There is No PART-B/Vehicle Entry.. So Please Update Vehicle Information..\n"; break;
                    case "382": Msg += "You Cannot Extend as EWB can be Extended only 8 hour before or after w.r.t Validity of EWB..!!\n"; break;
                    case "383": Msg += "Error While Extending..Please Contact Helpdesk. \n"; break;
                    case "384": Msg += "You are not current transporter or Generator of the ewayBill, with no transporter details.\n"; break;
                    case "385": Msg += "For Rail/Ship/Air transDocDate is mandatory\n"; break;
                    case "386": Msg += "Reason Code, Remarks is mandatory.\n"; break;
                    case "387": Msg += "No Record Found for Entered consolidated eWay bill.\n"; break;
                    case "388": Msg += "Exception in regenration of consolidated eWayBill!!Please Contact helpdesk\n"; break;
                    case "389": Msg += "Remaining Distance Required\n"; break;
                    case "390": Msg += "Remaining Distance Can not be greater than Actual Distance.\n"; break;
                    case "391": Msg += "No eway bill of specified tripsheet, neither  ACTIVE nor not Valid.\n"; break;
                    case "392": Msg += "Tripsheet is already cancelled, Hence Regeration is not possible\n"; break;
                    case "393": Msg += "Invalid GSTIN\n"; break;
                    case "394": Msg += "For other than Road Transport, TransDoc number is required\n"; break;
                    case "395": Msg += "Eway Bill Number should be numeric only\n"; break;
                    case "396": Msg += "Either Eway Bill Number Or Consolidated Eway Bill Number is required for Verification\n"; break;
                    case "397": Msg += "Error in Multi Vehicle Movement Initiation\n"; break;
                    case "398": Msg += "Eway Bill Item List is Empty\n"; break;
                    case "399": Msg += "Unit Code is not matching with any of the Unit Code from eway bill ItemList\n"; break;
                    case "400": Msg += "total quantity is exceeding from multi vehicle movement initiation quantity\n"; break;
                    case "401": Msg += "Error in inserting multi vehicle details\n"; break;
                    case "402": Msg += "total quantity can not be less than or equal to zero\n"; break;
                    case "403": Msg += "Error in multi vehicle details\n"; break;
                    case "405": Msg += "No record found for multi vehicle update with specified ewbNo groupNo and old vehicleNo/transDocNo with status as ACT\n"; break;
                    case "406": Msg += "Group number cannot be empty or zero\n"; break;
                    case "407": Msg += "Invalid old vehicle number format\n"; break;
                    case "408": Msg += "Invalid new vehicle number format\n"; break;
                    case "409": Msg += "Invalid old transDoc number\n"; break;
                    case "410": Msg += "Invalid new transDoc number\n"; break;
                    case "411": Msg += "Multi Vehicle Initiation data is not there for specified ewayBill and group No\n"; break;
                    case "412": Msg += "Multi Vehicle movement is already Initiated,hence PART B updation not allowed\n"; break;
                    case "413": Msg += "Unit Code is not matching with unit code of first initiaton\n"; break;
                    case "415": Msg += "Error in fetching in verification data for officer\n"; break;
                    case "416": Msg += "Date range is exceeding allowed date range \n"; break;
                    case "417": Msg += "No verification data found for officer \n"; break;
                    case "418": Msg += "No record found\n"; break;
                    case "419": Msg += "Error in fetching search result for taxpayer/transporter\n"; break;
                    case "420": Msg += "Minimum six character required for Tradename/legalname search\n"; break;
                    case "421": Msg += "Invalid pincode\n"; break;
                    case "422": Msg += "Invalid mobile number\n"; break;
                    case "423": Msg += "Error in fetching ewaybill list by vehicle number\n"; break;
                    case "424": Msg += "Invalid PAN number\n"; break;
                    case "425": Msg += "Error in fetching Part A data by IR Number\n"; break;
                    case "426": Msg += "For Vehicle Released vehicle release date and time is mandatory\n"; break;
                    case "427": Msg += "Error in saving Part-A verification Report\n"; break;
                    case "428": Msg += "For Goods Detained,Vehicle Released feild is mandatory\n"; break;
                    case "429": Msg += "Error in saving Part-B verification Report\n"; break;
                    case "430": Msg += "Goods Detained Field required.\n"; break;
                    case "431": Msg += "Part-A for this ewaybill is already generated by you.\n"; break;
                    case "432": Msg += "invalid vehicle released value\n"; break;
                    case "433": Msg += "invalid goods detained parameter value\n"; break;
                    case "434": Msg += "invalid ewbNoAvailable parameter value\n"; break;
                    case "435": Msg += "Part B is already updated,hence updation is not allowed\n"; break;
                    case "436": Msg += "Invalid Consignee ship to State Code for the given pincode\n"; break;
                    case "437": Msg += "Invalid Supplier ship from State Code for the given pincode\n"; break;
                    case "438": Msg += "Invalid Latitude\n"; break;
                    case "439": Msg += "Invalid Longitude\n"; break;
                    case "440": Msg += "Error in inserting in verification data\n"; break;
                    case "441": Msg += "Invalid verification type\n"; break;
                    case "442": Msg += "Error in inserting verification details\n"; break;
                    case "443": Msg += "invalid invoice available value\n"; break;
                    case "600": Msg += "Invalid category\n"; break;
                    case "601": Msg += "Invalid date format\n"; break;
                    case "602": Msg += "Invalid File Number\n"; break;
                    case "603": Msg += "For file details file number is required\n"; break;
                    case "604": Msg += "E-way bill(s) are already generated for the same document number, you cannot generate again on same document number\n"; break;
                    case "607": Msg += "dispatch from gstin is mandatary \n"; break;
                    case "608": Msg += "ship to from gstin is mandatary\n"; break;
                    case "609": Msg += " invalid ship to from gstin \n"; break;
                    case "610": Msg += "invalid dispatch from gstin \n"; break;
                    case "611": Msg += "invalid document type for the given supply type \n"; break;
                    case "612": Msg += "Invalid transaction type\n"; break;
                    case "613": Msg += "Exception in getting Officer Role\n"; break;
                    case "614": Msg += "Transaction type is mandatory\n"; break;
                    case "615": Msg += "Dispatch From GSTIN cannot be sent as the transaction type selected is Regular\n"; break;
                    case "616": Msg += "Ship to GSTIN cannot be sent as the transaction type selected is Regular\n"; break;
                    case "617": Msg += "Bill-from and dispatch-from gstin should not be same for this transaction type\n"; break;
                    case "618": Msg += "Bill-to and ship-to gstin should not be same for this transaction type\n"; break;
                    case "619": Msg += "Transporter Id is mandatory for generation of Part A slip\n"; break;
                    case "620": Msg += "Total invoice value cannot be less than the sum of total assessible value and tax values\n"; break;
                    case "621": Msg += "Transport mode is mandatory since vehicle number is present\n"; break;
                    case "622": Msg += "Transport mode is mandatory since transport document number is present\n"; break;
                    case "623": Msg += "IGST value is not applicable for Intra State Transaction\n"; break;
                    case "624": Msg += "CGST/SGST value is not applicable for Inter State Transaction\n"; break;
                    case "627": Msg += "Total value should not be negative\n"; break;
                    case "628": Msg += "Total invoice value should not be negative\n"; break;
                    case "629": Msg += "IGST value should not be negative\n"; break;
                    case "630": Msg += "CGST value should not be negative\n"; break;
                    case "631": Msg += "SGST value should not be negative\n"; break;
                    case "632": Msg += "Cess value should not be negative\n"; break;
                    case "633": Msg += "Cess non advol should not be negative\n"; break;
                    case "634": Msg += "Vehicle type should not be ODC when transmode is other than road\n"; break;
                    case "635": Msg += "You cannot update part B, as the current tranporter is already entered Part B details of the eway bill\n"; break;
                    case "636": Msg += "You are not assigned to update part B\n"; break;
                    case "637": Msg += "You cannot extend ewaybill, as the current tranporter is already entered Part B details of the ewaybill\n"; break;
                    case "638": Msg += "Transport mode is mandatory as Vehicle Number/Transport Document Number is given\n"; break;
                    case "640": Msg += "Tolal Invoice value is mandatory\n"; break;
                    case "641": Msg += "For outward CKD/SKD/Lots supply type, Bill To state should be as Other Country, since the  Bill To GSTIN given is of SEZ unit\n"; break;
                    case "642": Msg += "For inward CKD/SKD/Lots supply type, Bill From state should be as Other Country, since the  Bill From GSTIN given is of SEZ unit\n"; break;
                    case "643": Msg += "For regular transaction, Bill from state code and Dispatch from state code should be same\n"; break;
                    case "644": Msg += "For regular transaction, Bill to state code and Ship to state code should be same\n"; break;
                    case "645": Msg += "You cannot do multivehicle movement, as the current tranporter is already entered Part B details of the ewaybill\n"; break;
                    case "646": Msg += "You are not assigned to do MultiVehicle Movement\n"; break;
                    case "647": Msg += "Could not insert RFID data, pl. contact helpdisk\n"; break;
                    case "648": Msg += "Multi Vehicle movement is already Initiated,hence generation of consolidated eway bill is not allowed\n"; break;
                    case "649": Msg += "You cannot generate consolidated eway bill , as the current tranporter is already entered Part B details of the eway bill\n"; break;
                    case "650": Msg += "You are not assigned to generate consolidated ewaybill\n"; break;
                    case "651": Msg += "For Category Part-A or Part-B ewbdt is mandatory\n"; break;
                    case "652": Msg += "For Category EWB03 procdt is mandatory\n"; break;
                    case "654": Msg += "This GSTIN has generated a common Enrolment Number. Hence you are not allowed to generate Eway bill\n"; break;
                    case "655": Msg += "This GSTIN has generated a common Enrolment Number. Hence you cannot mention it as a tranporter\n"; break;
                    case "656": Msg += "This Eway Bill does not belongs to your state\n"; break;
                    case "657": Msg += "Eway Bill Category wise details will be available after 4 days only\n"; break;
                    case "658": Msg += "You are blocked for accesing this API as the allowed number of requests has been exceeded for this duration\n"; break;
                    case "659": Msg += "Remarks is mandatory\n"; break;
                    case "670": Msg += "Invalid Month Parameter\n"; break;
                    case "671": Msg += "Invalid Year Parameter\n"; break;
                    case "672": Msg += "User Id is mandatory\n"; break;
                    case "673": Msg += "Error in getting officer dashboard\n"; break;
                    case "675": Msg += "Error in getting EWB03 details by acknowledgement date range\n"; break;
                    case "676": Msg += "Error in getting EWB Not Available List by entered date range\n"; break;
                    case "677": Msg += "Error in getting EWB Not Available List by closed date range\n"; break;
                    case "678": Msg += "Invalid Uniq No\n"; break;
                    case "679": Msg += "Invalid EWB03 Ack No\n"; break;
                    case "680": Msg += "Invalid Close Reason\n"; break;
                    case "681": Msg += "Error in Closing EWB  Verification Data\n"; break;
                    case "682": Msg += "No Record available to Close\n"; break;
                    case "683": Msg += "Error in fetching WatchList Data\n"; break;
                    case "685": Msg += "Exception in fetching dashboard data\n"; break;
                    case "700": Msg += "You are not assigned to extend e-waybill\n"; break;
                    case "701": Msg += "Invalid Vehicle Direction\n"; break;
                    case "702": Msg += "The distance between the pincodes given is too high\n"; break;
                    case "703": Msg += "Since the consignor is Composite Taxpayer, inter state transactions are not allowed\n"; break;
                    case "704": Msg += "Since the consignor is Composite Taxpayer, Tax rates should be zero\n"; break;
                    case "705": Msg += "Invalid transit type\n"; break;
                    case "706": Msg += "Address Line1 is mandatory\n"; break;
                    case "707": Msg += "Address Line2 is mandatory\n"; break;
                    case "708": Msg += "Address Line3 is mandatory\n"; break;
                    case "709": Msg += "Pin to pin distance is not available for the given pin codes\n"; break;
                    case "710": Msg += "Invalid state code for the given pincode\n"; break;
                    case "711": Msg += "Invalid consignment status for the given transmode\n"; break;
                    case "712": Msg += "Transit Type is not required as the goods are in movement\n"; break;
                    case "713": Msg += "Transit Address is not required as the goods are in movement\n"; break;
                    case "714": Msg += "Document type - Tax Invoice is not allowed for composite tax payer\n"; break;
                    case "715": Msg += "The Consignor GSTIN is blocked from e-waybill generation as Return is not filed for past 2 months\n"; break;
                    case "716": Msg += "The Consignee GSTIN is blocked from e-waybill generation as Return is not filed for past 2 months\n"; break;
                    case "717": Msg += "The Transporter GSTIN is blocked from e-waybill generation as Return is not filed for past 2 months\n"; break;
                    case "718": Msg += "The User GSTIN is blocked from Transporter Updation as Return is not filed for past 2 months\n"; break;
                    case "719": Msg += "The Transporter GSTIN is blocked from Transporter Updation as Return is not filed for past 2 months\n"; break;
                    case "720": Msg += "E Way Bill should be generated as part of IRN generation or with reference to IRN in E Invoice System, Since Supplier is enabled for E Invoice.\n"; break;
                    case "721": Msg += "The distance between the given pincodes are not available in the system. Please provide distance.\n"; break;
                    case "722": Msg += "Consignee GSTIN is cancelled and document date is later than the  De-Registration date\n"; break;
                    case "724": Msg += "HSN code of at least one item should be of goods to generate e-Way Bill\n"; break;
                    case "726": Msg += "Vehicle type can not be regular when transportation mode is ship\n"; break;
                    case "727": Msg += "This e-Way Bill does not have Oxygen items\n"; break;
                    case "728": Msg += "You can cancel the ewaybill within 24 hours from Part B entry\n"; break;
                    case "801": Msg += "Transporter id is not required for ewaybill for gold\n"; break;
                    case "802": Msg += "Transporter name is not required for ewaybill for gold\n"; break;
                    case "803": Msg += "TransDocNo is not required for ewaybill for gold\n"; break;
                    case "804": Msg += "TransDocDate is not required for ewaybill for gold\n"; break;
                    case "805": Msg += "Vehicle No is not required for ewaybill for gold\n"; break;
                    case "806": Msg += "Vehicle Type is not required for ewaybill for gold\n"; break;
                    case "807": Msg += "Transmode is mandatory for ewaybill for gold\n"; break;
                    case "808": Msg += "Inter-State ewaybill is not allowed for gold\n"; break;
                    case "809": Msg += "Other items are not allowed with eway bill for gold\n"; break;
                    case "810": Msg += "Transport can not be updated for EwayBill For Gold\n"; break;
                    case "811": Msg += "Vehicle can not be updated for EwayBill For Gold\n"; break;
                    case "812": Msg += "ConsolidatedEWB cannot be generated for EwayBill For Gold \n"; break;
                    case "813": Msg += "Duplicate request at the same time\n"; break;
                    case "814": Msg += "MultiVehicleMovement cannot be initiated for EWay Bill For Gold\n"; break;
                    case "815": Msg += "Only trans mode road is allowed for Eway Bill For Gold\n"; break;
                    case "816": Msg += "Only transmode road is allowed for extending ewaybill for gold\n"; break;
                    case "817": Msg += "MultiVehicleMovement cannot be initiated.Eway Bill is not in Active State\n"; break;
                    case "818": Msg += "Validity period lapsed.Cannot generate consolidated Eway Bill\n"; break;
                }
            }

            return Msg;
        }

        #endregion

        #region MultiDocument

        [HttpPost]
        public ActionResult FetchDocumentList(LorryReceiptQueryVM Model)
        {
            List<LorryReceiptQueryVM> ValueList = new List<LorryReceiptQueryVM>();

            var mlrmaster = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == Model.Lrno).Select(x => x).ToList();
            if (mlrmaster == null)
            {
                Model.Status = "ValidError";
                Model.Message = "Consignment Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            if (mlrmaster.Count() == 0)
            {
                Model.Status = "ValidError";
                Model.Message = "Consignment Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            else if (mlrmaster.Count() == 1)
            {
                Model.Status = "Processed";
                Model.Message = mlrmaster.Select(x => x.TableKey).FirstOrDefault();
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            foreach (var item in mlrmaster)
            {
                LorryReceiptQueryVM otherTransact = new LorryReceiptQueryVM();
                otherTransact.ConsignmentKey = item.TableKey.ToString();
                otherTransact.Lrno = item.LrNo.ToString();
                otherTransact.LrDate = item.BookDate;
                otherTransact.LrQty = item.TotQty.ToString();
                otherTransact.LrActWt = item.ActWt.ToString("0.00");
                otherTransact.LrChrgWt = item.ChgWt.ToString("0.00");
                otherTransact.LRBranch = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault();
                otherTransact.LrFrom = ctxTFAT.TfatBranch.Where(x => x.Code == item.Source).Select(x => x.Name).FirstOrDefault();
                otherTransact.LrTo = ctxTFAT.TfatBranch.Where(x => x.Code == item.Dest).Select(x => x.Name).FirstOrDefault();
                otherTransact.LrConsignor = ctxTFAT.Consigner.Where(x => x.Code == item.SendCode).Select(x => x.Name).FirstOrDefault();
                otherTransact.LrConsignee = ctxTFAT.Consigner.Where(x => x.Code == item.RecCode).Select(x => x.Name).FirstOrDefault();
                ValueList.Add(otherTransact);
            }
            Model.ValueList = ValueList;
            var html = ViewHelper.RenderPartialView(this, "ConsignmentList", Model);
            return Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Generate EwayBill

        //Get Respenctive Data
        public ActionResult GetEwayBill(EwayBillVM Model)
        {
            string Errmsg = "", Status = "Success", TfatCompErr = "";
            TfatCompErr = SetApiParameters();
            if (String.IsNullOrEmpty(TfatCompErr))
            {
                LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == Model.Document.Trim()).FirstOrDefault();
                if (lRMaster != null)
                {
                    if (!String.IsNullOrEmpty(lRMaster.EwayBill))
                    {
                        Status = "Error";
                        Errmsg += "Not Allow To Create Eway Bill Of This Consignment Because Eway Bill Already Generated ...!\n";
                        Errmsg += "This Consignment Eway Bill No is : " + lRMaster.EwayBill + "\n";
                    }
                    if (!(lRMaster.DecVal >= 50000))
                    {
                        Status = "Error";
                        Errmsg += "Not Allow To Create Eway Bill Of This Consignment Because Value Need Minimum 50000 ...!\n";
                        Errmsg += "This Consignment Value Is : " + Convert.ToInt32(lRMaster.DecVal) + "\n"; ;
                    }
                    if ((String.IsNullOrEmpty(lRMaster.EwayBill)) && lRMaster.DecVal >= 50000 && Status == "Success")
                    {
                        tfatEwaySetup ewaySetup = ctxTFAT.tfatEwaySetup.FirstOrDefault();

                        if (ewaySetup != null)
                        {
                            Model.EWB_SupplyType = ewaySetup.GenSupplyType;
                            Model.EWB_SubSupplyType = ewaySetup.GenSubType;
                            Model.EWB_DocType = ewaySetup.GenDoctype;
                            Model.EWB_TRNType = ewaySetup.GenTranType;
                            Model.EWB_VehicleType = ewaySetup.GenVehicleType;
                        }
                        if (!String.IsNullOrEmpty(lRMaster.DescrType))
                        {
                            Model.EWB_productName = ctxTFAT.DescriptionMaster.Where(x => x.Code == lRMaster.DescrType).Select(x => x.Description).FirstOrDefault();
                        }

                        Model.TotalQty = lRMaster.TotQty;
                        Model.DocDate = lRMaster.BookDate.ToShortDateString();
                        Model.EWBDocument = String.IsNullOrEmpty(lRMaster.PartyInvoice) == true ? lRMaster.BENumber : lRMaster.PartyInvoice;
                        Model.EWB_TRNMode = lRMaster.TrnMode;
                        Model.EWB_hsnCode = lRMaster.HSNCODE;
                        Model.TaxableAmt = (decimal)lRMaster.DecVal;
                        Model.TotalInvAmt = (decimal)lRMaster.DecVal;
                        Model.EWB_ConsignorName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.RecCode).Select(x => x.Name).FirstOrDefault();
                        Model.EWB_ConsigneeName = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.SendCode).Select(x => x.Name).FirstOrDefault();

                        var Unts = GetUsints();
                        var GetElements = Unts.Where(x => x.Code == lRMaster.UnitCode).FirstOrDefault();
                        if (GetElements != null)
                        {
                            Model.UnitCode = GetElements.Code;
                            Model.UnitName = GetElements.Name;
                        }

                        //Model.EWB_ConsignorCode = lRMaster.RecCode;
                        //Model.EWB_ConsigneeCode = lRMaster.SendCode;
                        //Model.EWB_ConsignerAddNo = lRMaster.ConsignerAddNo;
                        //Model.EWB_ConsigneeAddNo = lRMaster.ConsigneeAddNo;
                        //var ConsignerAddres = ctxTFAT.ConsignerAddress.Where(x => x.Code == lRMaster.RecCode && x.Sno.ToString() == lRMaster.ConsignerAddNo).FirstOrDefault();
                        //if (ConsignerAddres != null)
                        //{
                        //    Model.EWB_ConsignerAddNoName = string.IsNullOrEmpty(ConsignerAddres.Addr1) == false ? ConsignerAddres.Addr1 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Addr2) == false ? ConsignerAddres.Addr2 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))))
                        //                                                                                                                    : (string.IsNullOrEmpty(ConsignerAddres.Addr2) == false ? ConsignerAddres.Addr2 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))));
                        //    Model.EWB_ConsignerAddNoName = Model.EWB_ConsignerAddNoName.Replace("\n", " ");
                        //}
                        //ConsignerAddres = ctxTFAT.ConsignerAddress.Where(x => x.Code == lRMaster.SendCode && x.Sno.ToString() == lRMaster.ConsigneeAddNo).FirstOrDefault();
                        //if (ConsignerAddres != null)
                        //{
                        //    Model.EWB_ConsigneeAddNoName = string.IsNullOrEmpty(ConsignerAddres.Addr1) == false ? ConsignerAddres.Addr1 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Addr2) == false ? ConsignerAddres.Addr2 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))))
                        //                                                                                                                    : (string.IsNullOrEmpty(ConsignerAddres.Addr2) == false ? ConsignerAddres.Addr2 + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))) : (string.IsNullOrEmpty(ConsignerAddres.Country) == false ? (ctxTFAT.TfatCountry.Where(y => y.Code.ToString() == ConsignerAddres.Country).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "")) : (string.IsNullOrEmpty(ConsignerAddres.State) == false ? (ctxTFAT.TfatState.Where(y => y.Code.ToString() == ConsignerAddres.State).Select(y => y.Name).FirstOrDefault()) + ",\n" + (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : "") : (string.IsNullOrEmpty(ConsignerAddres.City) == false ? (ctxTFAT.TfatCity.Where(y => y.Code == ConsignerAddres.City).Select(y => y.Name).FirstOrDefault()) : ""))));
                        //    Model.EWB_ConsigneeAddNoName = Model.EWB_ConsigneeAddNoName.Replace("\n", " ");
                        //}
                        var consigner = ctxTFAT.ConsignerAddress.Where(x => x.Code == lRMaster.RecCode && x.Sno.ToString() == lRMaster.ConsignerAddNo).FirstOrDefault();
                        var consignee = ctxTFAT.ConsignerAddress.Where(x => x.Code == lRMaster.SendCode && x.Sno.ToString() == lRMaster.ConsigneeAddNo).FirstOrDefault();


                        if (String.IsNullOrEmpty(lRMaster.RecGST))
                        {
                            Model.EWB_ConsignorGST = consigner == null ? "" : consigner.GSTNo;
                        }
                        else
                        {
                            Model.EWB_ConsignorGST = lRMaster.RecGST.Trim();
                        }
                        Model.EWB_ConsignorPincode = consigner == null ? "" : consigner.Pin.ToString();
                        if (consigner != null)
                        {
                            if (!String.IsNullOrEmpty(consigner.State))
                            {
                                Model.EWB_ConsignorState = consigner.State;
                                Model.EWB_ConsignorStateName = ctxTFAT.TfatState.Where(x => x.Code.ToString() == consigner.State).Select(x => x.Name).FirstOrDefault();
                            }
                        }

                        Model.EWB_ConsignorAddr1 = consigner == null ? "" : consigner.Addr1;
                        Model.EWB_ConsignorAddr2 = consigner == null ? "" : consigner.Addr2;


                        if (String.IsNullOrEmpty(lRMaster.SendGST))
                        {
                            Model.EWB_ConsigneeGST = consignee == null ? "" : consignee.GSTNo;
                        }
                        else
                        {
                            Model.EWB_ConsigneeGST = lRMaster.SendGST.Trim();
                        }

                        Model.EWB_ConsigneePincode = consignee == null ? "" : consignee.Pin.ToString();

                        if (consignee != null)
                        {
                            if (!String.IsNullOrEmpty(consignee.State))
                            {
                                Model.EWB_ConsigneeState = consignee.State;
                                Model.EWB_ConsigneeStateName = ctxTFAT.TfatState.Where(x => x.Code.ToString() == consignee.State).Select(x => x.Name).FirstOrDefault();
                            }
                        }

                        Model.EWB_ConsigneeAddr1 = consignee == null ? "" : consignee.Addr1;
                        Model.EWB_ConsigneeAddr2 = consignee == null ? "" : consignee.Addr2;

                        Model.EWB_FromPlace = lRMaster.Source;
                        Model.EWB_FromPlaceName = ctxTFAT.TfatBranch.Where(x => x.Code.ToString() == lRMaster.Source).Select(x => x.Name).FirstOrDefault();
                        Model.EWB_ToPlace = lRMaster.Dest;
                        Model.EWB_ToPlaceName = ctxTFAT.TfatBranch.Where(x => x.Code.ToString() == lRMaster.Dest).Select(x => x.Name).FirstOrDefault();

                    }
                }
                else
                {
                    Status = "Error";
                    Errmsg = "Consignment Not Found...!";
                }
            }
            else
            {
                Status = "Error";
                Errmsg = TfatCompErr;
            }

            var html = ViewHelper.RenderPartialView(this, "GenerateEway", Model);

            var jsonResult = Json(new
            {
                Status = Status,
                Errmsg = Errmsg,
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        //Generate Eway-Bill
        public ActionResult GenerateEWayBill(EwayBillVM Model)
        {
            //Model.Type = "1";//Generate New EwayBill
            string Status = "Success", EwaybillNo = "", EwatValidUpto = "", errormsg = "", TfatCompErr = "";
            JsonResult jsonResult = new JsonResult();

            //if (Model.Document == null || Model.Document == "")
            //{
            //    jsonResult = Json(new { Status = "Error", errormsg = "Document Missing...!", EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
            //    jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            //    return jsonResult;
            //}
            //if (String.IsNullOrEmpty(Model.EWBDocument))
            //{
            //    jsonResult = Json(new { Status = "Error", errormsg = "Please Enter EWB-Document No...!" }, JsonRequestBehavior.AllowGet);
            //    jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            //    return jsonResult;
            //}
            TfatCompErr = SetApiParameters();
            if (!String.IsNullOrEmpty(TfatCompErr))
            {
                jsonResult = Json(new { Status = "Error", errormsg = TfatCompErr }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }

            LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == Model.Document.Trim()).FirstOrDefault();
            //if (lRMaster == null)
            //{
            //    jsonResult = Json(new { Status = "Error", errormsg = "Consignment Not Found...!" }, JsonRequestBehavior.AllowGet);
            //    jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            //    return jsonResult;
            //}

            StringBuilder jsonBuilder = new StringBuilder();
            string VehicleNo = "";
            if (!String.IsNullOrEmpty(Model.EWB_VehicleNo))
            {
                HireVehicleMaster hireVehicle = ctxTFAT.HireVehicleMaster.Where(x => x.Code == Model.EWB_VehicleNo).FirstOrDefault();
                VehicleMaster vehicle = ctxTFAT.VehicleMaster.Where(x => x.Code == Model.EWB_VehicleNo).FirstOrDefault();

                if (hireVehicle != null)
                {
                    VehicleNo = Regex.Replace(hireVehicle.TruckNo, @"\s", "");
                    Model.EWB_LastVehicleNo = VehicleNo;
                }
                else if (vehicle != null)
                {
                    VehicleNo = Regex.Replace(vehicle.TruckNo, @"\s", "");
                    Model.EWB_LastVehicleNo = VehicleNo;
                }
                else
                {
                    VehicleNo = Regex.Replace(Model.EWB_VehicleNo, @"\s", "");
                    Model.EWB_LastVehicleNo = VehicleNo;
                }
            }


            if (Model.EWB_SupplyType == "I" && Model.EWB_SubSupplyType == "2")
            {
                Model.EWB_ConsignorPincode = "999999";
            }
            else if (Model.EWB_SupplyType == "O" && Model.EWB_SubSupplyType == "3")
            {
                Model.EWB_ConsigneePincode = "999999";
            }


            jsonBuilder.Append("{");
            jsonBuilder.Append("\"supplyType\":\"" + Model.EWB_SupplyType + "\",");
            jsonBuilder.Append("\"subSupplyType\":\"" + Model.EWB_SubSupplyType + "\",");
            jsonBuilder.Append("\"subSupplyDesc\":\"" + Model.EWB_SubSupplyDesc + "\",");
            jsonBuilder.Append("\"docType\":\"" + Model.EWB_DocType + "\",");
            jsonBuilder.Append("\"docNo\":\"" + Model.EWBDocument + "\",");
            jsonBuilder.Append("\"docDate\":\"" + Model.DocDate + "\",");

            //Consignor Details Fill
            if (Model.EWB_SupplyType == "I" && Model.EWB_SubSupplyType == "2")
            {
                jsonBuilder.Append("\"fromGstin\":\"" + "URP" + "\",");
            }
            else
            {
                jsonBuilder.Append("\"fromGstin\":\"" + Model.EWB_ConsignorGST.Trim() + "\",");
            }
            jsonBuilder.Append("\"fromTrdName\":\"" + Model.EWB_ConsignorName.ToUpper().Trim() + "\",");
            jsonBuilder.Append("\"fromAddr1\":\"" + Model.EWB_ConsignorAddr1 + "\",");
            jsonBuilder.Append("\"fromAddr2\":\"" + Model.EWB_ConsignorAddr2 + "\",");

            jsonBuilder.Append("\"fromPlace\":\"" + (ctxTFAT.TfatBranch.Where(x => x.Code == Model.EWB_FromPlace).Select(x => x.Name).FirstOrDefault()) + "\",");

            jsonBuilder.Append("\"fromPincode\":" + Model.EWB_ConsignorPincode.ToString() + ",");
            if (Model.EWB_SupplyType == "I" && Model.EWB_SubSupplyType == "2")
            {
                jsonBuilder.Append("\"actFromStateCode\":" + 97 + ",");
                jsonBuilder.Append("\"fromStateCode\":" + 99 + ",");
            }
            else
            {
                jsonBuilder.Append("\"actFromStateCode\":" + (ctxTFAT.TfatState.Where(x => x.Code.ToString() == Model.EWB_ConsignorState).Select(x => x.StateCode).FirstOrDefault()) + ",");
                jsonBuilder.Append("\"fromStateCode\":" + (ctxTFAT.TfatState.Where(x => x.Code.ToString() == Model.EWB_ConsignorState).Select(x => x.StateCode).FirstOrDefault()) + ",");
            }



            //Consignee Details Fill
            if (Model.EWB_SupplyType == "O" && Model.EWB_SubSupplyType == "3")
            {
                jsonBuilder.Append("\"toGstin\":\"" + "URP" + "\",");
            }
            else
            {
                jsonBuilder.Append("\"toGstin\":\"" + Model.EWB_ConsigneeGST.Trim() + "\",");
            }

            jsonBuilder.Append("\"toTrdName\":\"" + Model.EWB_ConsigneeName.ToUpper().Trim() + "\",");
            jsonBuilder.Append("\"toAddr1\":\"" + Model.EWB_ConsigneeAddr1 + "\",");
            jsonBuilder.Append("\"toAddr2\":\"" + Model.EWB_ConsigneeAddr2 + "\",");
            jsonBuilder.Append("\"toPlace\":\"" + (ctxTFAT.TfatBranch.Where(x => x.Code == Model.EWB_ToPlace).Select(x => x.Name).FirstOrDefault()) + "\",");
            jsonBuilder.Append("\"toPincode\":" + Model.EWB_ConsigneePincode.ToString() + ",");

            if (Model.EWB_SupplyType == "O" && Model.EWB_SubSupplyType == "3")
            {
                jsonBuilder.Append("\"actToStateCode\":" + 97 + ",");
                jsonBuilder.Append("\"toStateCode\":" + 99 + ",");
            }
            else
            {
                jsonBuilder.Append("\"actToStateCode\":" + (ctxTFAT.TfatState.Where(x => x.Code.ToString() == Model.EWB_ConsigneeState).Select(x => x.StateCode).FirstOrDefault()) + ",");
                jsonBuilder.Append("\"toStateCode\":" + (ctxTFAT.TfatState.Where(x => x.Code.ToString() == Model.EWB_ConsigneeState).Select(x => x.StateCode).FirstOrDefault()) + ",");
            }


            //Transaction Details
            jsonBuilder.Append("\"transactionType\":" + Model.EWB_TRNType + ",");
            if (Model.EWB_SupplyType == "I" && Model.EWB_SubSupplyType == "2")
            {
            }
            else
            {
                jsonBuilder.Append("\"dispatchFromGSTIN\":\"" + Model.EWB_ConsignorGST.Trim() + "\",");
            }
            jsonBuilder.Append("\"dispatchFromTradeName\":\"" + Model.EWB_ConsignorName.ToUpper().Trim() + "\",");

            if (Model.EWB_SupplyType == "O" && Model.EWB_SubSupplyType == "3")
            {

            }
            else
            {
                jsonBuilder.Append("\"shipToGSTIN\":\"" + Model.EWB_ConsigneeGST.Trim() + "\",");
            }

            jsonBuilder.Append("\"shipToTradeName\":\"" + Model.EWB_ConsigneeName.ToUpper().Trim() + "\",");
            jsonBuilder.Append("\"otherValue\":" + Convert.ToDecimal(Model.OtherAmt).ToString("F2") + ",");
            jsonBuilder.Append("\"totalValue\":" + Convert.ToDecimal(Model.TaxableAmt).ToString("F2") + ",");
            jsonBuilder.Append("\"cgstValue\":" + Convert.ToDecimal(Model.CGSTAmt).ToString("F2") + ",");
            jsonBuilder.Append("\"sgstValue\":" + Convert.ToDecimal(Model.SGSTAmt).ToString("F2") + ",");
            jsonBuilder.Append("\"igstValue\":" + Convert.ToDecimal(Model.IGSTAmt).ToString("F2") + ",");
            jsonBuilder.Append("\"cessValue\":" + Convert.ToDecimal(Model.CessAmt).ToString("F2") + ",");
            jsonBuilder.Append("\"cessNonAdvolValue\":" + Convert.ToDecimal(Model.CessAdvolAmt).ToString("F2") + ",");
            jsonBuilder.Append("\"totInvValue\":" + Convert.ToDecimal(Model.TotalInvAmt).ToString("F2") + ",");
            jsonBuilder.Append("\"transporterId\":\"" + (ctxTFAT.TfatComp.Select(x => x.TransporteID).FirstOrDefault()) + "\",");
            jsonBuilder.Append("\"transporterName\":\"" + (ctxTFAT.TfatComp.Select(x => x.Name).FirstOrDefault()) + "\",");
            if (!String.IsNullOrEmpty(VehicleNo))
            {
                if (lRMaster != null)
                {
                    jsonBuilder.Append("\"transDocNo\":\"" + lRMaster.LrNo.ToString() + "\",");
                }
                else
                {
                    jsonBuilder.Append("\"transDocNo\":\"" + "" + "\",");
                }
                jsonBuilder.Append("\"transMode\":\"" + Model.EWB_TRNMode + "\",");
            }
            jsonBuilder.Append("\"transDistance\":\"" + Model.EWB_Distance + "\",");
            if (!String.IsNullOrEmpty(VehicleNo))
            {
                jsonBuilder.Append("\"transDocDate\":\"" + Model.DocDate + "\",");
                jsonBuilder.Append("\"vehicleNo\":\"" + VehicleNo.ToUpper() + "\",");
                jsonBuilder.Append("\"vehicleType\":\"" + Model.EWB_VehicleType + "\",");
            }

            jsonBuilder.Append("\"itemList\":[");

            #region ItemList
            jsonBuilder.Append("{");
            jsonBuilder.Append("\"productName\":\"" + Model.EWB_productName + "\",");
            if (lRMaster != null)
            {
                jsonBuilder.Append("\"productDesc\":\"" + (ctxTFAT.DescriptionMaster.Where(x => x.Code == lRMaster.DescrType).Select(x => x.Description).FirstOrDefault()) + "\",");
            }
            else
            {
                jsonBuilder.Append("\"productDesc\":\"" + "" + "\",");
            }
            jsonBuilder.Append("\"hsnCode\":\"" + Model.EWB_hsnCode + "\",");
            jsonBuilder.Append("\"quantity\":" + Model.TotalQty + ",");
            jsonBuilder.Append("\"qtyUnit\":\"" + Model.UnitCode + "\",");
            jsonBuilder.Append("\"cgstRate\":" + Convert.ToDecimal(Model.CGSTRate).ToString("F3") + ",");
            jsonBuilder.Append("\"sgstRate\":" + Convert.ToDecimal(Model.SGSTRate).ToString("F3") + ",");
            jsonBuilder.Append("\"igstRate\":" + Convert.ToDecimal(Model.IGSTRate).ToString("F3") + ",");
            jsonBuilder.Append("\"cessRate\":" + Convert.ToDecimal(Model.CessRate).ToString("F3") + ",");
            jsonBuilder.Append("\"cessNonAdvol\":" + Convert.ToDecimal(Model.CessAdvolRate).ToString("F3") + ",");
            jsonBuilder.Append("\"taxableAmount\":" + Convert.ToDecimal(Model.TaxableAmt).ToString("F2") + "");
            jsonBuilder.Append("},");
            #endregion

            jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            jsonBuilder.Append("]},");

            jsonBuilder.Remove(jsonBuilder.Length - 1, 1);

            JObject mjson = JObject.Parse(jsonBuilder.ToString());

            string mtoken = GetToken();
            if (mtoken != "0")
            {
                RestClient client = new RestClient(weburl + genewaybillurl);
                RestRequest request = new RestRequest(Method.POST);
                request.AddParameter("email", memail, ParameterType.QueryString);
                request.AddHeader("ip_address", mipaddress);
                request.AddHeader("client_id", mclientid);
                request.AddHeader("client_secret", mclientsecret);
                request.AddHeader("gstin", mgstin);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/json");
                request.AddParameter("application/json", mjson, ParameterType.RequestBody);
                var response = client.Execute(request);
                string message = response.Content;
                dynamic irnresponseorder = JObject.Parse(message);
                string mreturn = JValue.Parse(irnresponseorder.GetValue("status_cd").ToString());
                if (mreturn != "0")
                {
                    dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("data").ToString());

                    EwaybillNo = irnresponseData.GetValue("ewayBillNo").ToString();
                    EwatValidUpto = irnresponseData.GetValue("validUpto").ToString();
                    errormsg += "Generate EwayBill Sucessfully.\n Eway BillNo:" + EwaybillNo + " \n Valid Upto : " + EwatValidUpto + "";
                    Model.Document = lRMaster == null ? "" : lRMaster.LrNo.ToString();
                    SaveEWB(Model, "LR000", irnresponseData.GetValue("ewayBillNo").ToString(), irnresponseData.GetValue("validUpto").ToString(), "GEN", lRMaster, lRMaster == null ? "" : lRMaster.TableKey);
                    SaveEWBLog("LR000", lRMaster == null ? "" : lRMaster.LrNo.ToString(), "Generate Eway Bill", "Sucess", irnresponseData.GetValue("ewayBillNo").ToString());
                    if (!String.IsNullOrEmpty(Model.Document))
                    {
                        ExecuteStoredProc("Update LRMaster set EwayBill='" + irnresponseData.GetValue("ewayBillNo").ToString() + "' where Tablekey='" + lRMaster.TableKey + "'");
                    }
                }
                else
                {
                    Status = "Error";
                    dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("error").ToString());
                    dynamic ErrorMsh = JValue.Parse(irnresponseData.GetValue("message").ToString());
                    var ErrorCodeList = ErrorMsh.GetValue("errorCodes").ToString();
                    errormsg = GetErrorString(ErrorCodeList);
                    SaveEWBLog("LR000", lRMaster == null ? "" : lRMaster.LrNo.ToString(), "Generate Eway Bill", errormsg, "");
                }
            }
            else
            {
                jsonResult = Json(new { Status = "Error", errormsg = "Authentication Issue...!", EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }

            jsonResult = Json(new { Status = Status, errormsg = errormsg, EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        public ActionResult FetchPendingConsignmentList(EwayBillVM mModel)
        {
            var html = ViewHelper.RenderPartialView(this, "PendingEwayList", mModel);
            var jsonResult = Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        //Clear Document Does Not Need To Pending EwatBill List So On....
        public ActionResult ClearEwayDocument(EwayBillVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == mModel.Document).FirstOrDefault();

                    if (lRMaster != null)
                    {
                        tfatSkipEwayBillDoc tfatSkipEwayBillDoc = new tfatSkipEwayBillDoc();
                        tfatSkipEwayBillDoc.DocNo = mModel.Document.Trim();
                        tfatSkipEwayBillDoc.DocDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                        tfatSkipEwayBillDoc.Branch = mbranchcode;
                        tfatSkipEwayBillDoc.ENTEREDBY = muserid;
                        tfatSkipEwayBillDoc.AUTHIDS = muserid;
                        tfatSkipEwayBillDoc.AUTHORISE = "A00";
                        tfatSkipEwayBillDoc.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                        ctxTFAT.tfatSkipEwayBillDoc.Add(tfatSkipEwayBillDoc);
                        ctxTFAT.SaveChanges();
                        transaction.Commit();
                        transaction.Dispose();
                    }
                    else
                    {
                        return Json(new { Message = "Document Not Found...\n ", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
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
            return Json(new { Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);

        }

        #endregion

        #region Multivehicle EwayBill

        //Get Respenctive Data
        public ActionResult GetMultiVehicleEway(EwayBillVM Model)
        {
            string Errmsg = "", Status = "Success", TfatCompErr = "";
            TfatCompErr = SetApiParameters();
            if (String.IsNullOrEmpty(TfatCompErr))
            {
                LRMaster lRMaster = new LRMaster();
                if (String.IsNullOrEmpty(Model.Document))
                {
                    lRMaster = ctxTFAT.LRMaster.Where(x => x.EwayBill.ToString() == Model.SearchEway.Trim()).FirstOrDefault();
                }
                else
                {
                    lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == Model.Document.Trim()).FirstOrDefault();
                }
                if (lRMaster != null)
                {
                    if ((!String.IsNullOrEmpty(lRMaster.EwayBill)))
                    {
                        tfatEwaySetup ewaySetup = ctxTFAT.tfatEwaySetup.FirstOrDefault();
                        if (ewaySetup != null)
                        {
                            Model.EWB_ReasonCode = ewaySetup.MulReason;
                        }

                        Model.DocDate = lRMaster.BookDate.ToShortDateString();
                        Model.EWBDocument = String.IsNullOrEmpty(lRMaster.PartyInvoice) == true ? lRMaster.BENumber : lRMaster.PartyInvoice.ToString();
                        Model.EWB_EwayNo = lRMaster.EwayBill.Trim();
                        Model.EWB_TotalQty = lRMaster.TotQty.ToString();
                        Model.EWB_TRNMode = lRMaster.TrnMode == null ? "" : lRMaster.TrnMode.ToString();

                        Model.EWB_FromPlace = lRMaster.Source;
                        Model.EWB_FromPlaceName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Source).Select(x => x.Name).FirstOrDefault();

                        Model.EWB_ToPlace = lRMaster.Dest;
                        Model.EWB_ToPlaceName = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name).FirstOrDefault();

                        Consigner consigner = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.RecCode).FirstOrDefault();
                        Consigner consignee = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.SendCode).FirstOrDefault();
                        if (!String.IsNullOrEmpty(consigner.State))
                        {
                            Model.EWB_ConsignorState = consigner.State;
                            Model.EWB_ConsignorStateName = ctxTFAT.TfatState.Where(x => x.Code.ToString() == consigner.State).Select(x => x.Name).FirstOrDefault();
                        }
                        if (!String.IsNullOrEmpty(consignee.State))
                        {
                            Model.EWB_ConsigneeState = consignee.State;
                            Model.EWB_ConsigneeStateName = ctxTFAT.TfatState.Where(x => x.Code.ToString() == consignee.State).Select(x => x.Name).FirstOrDefault();
                        }

                        var Unts = GetUsints();
                        var GetElements = Unts.Where(x => x.Code == lRMaster.UnitCode).FirstOrDefault();
                        if (GetElements != null)
                        {
                            Model.UnitCode = GetElements.Code;
                            Model.UnitName = GetElements.Name;
                        }
                    }
                    else
                    {
                        Status = "Error";
                        Errmsg = "This Consignment EwayBill Missing.\n Please Check It Consignment Details...!";
                    }
                }
                else
                {
                    tfatEwaySetup ewaySetup = ctxTFAT.tfatEwaySetup.FirstOrDefault();
                    if (ewaySetup != null)
                    {
                        Model.EWB_ReasonCode = ewaySetup.MulReason;
                    }
                    Model.EWB_EwayNo = Model.SearchEway.Trim();
                    Model.EWB_TotalQty = "0";
                }

                if (!String.IsNullOrEmpty(Model.Document))
                {
                    if (lRMaster == null)
                    {
                        Status = "Error";
                        Errmsg = "Consignment Not Found...!";
                    }
                }
            }
            else
            {
                Status = "Error";
                Errmsg = TfatCompErr;
            }
            var html = ViewHelper.RenderPartialView(this, "MultiVehicleEway", Model);
            var jsonResult = Json(new
            {
                Status = Status,
                Errmsg = Errmsg,
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        //Generate Multivehicle Eway-Bill
        public ActionResult GenerateMultiVehicleEWayBill(EwayBillVM Model)
        {
            //Model.Type = "1";//Generate New EwayBill
            string Status = "Success", EwaybillNo = "", EwatValidUpto = "", errormsg = "", TfatCompErr = "";
            JsonResult jsonResult = new JsonResult();

            if (String.IsNullOrEmpty(Model.Document) && String.IsNullOrEmpty(Model.SearchEway))
            {
                jsonResult = Json(new { Status = "Error", errormsg = "Document Missing...!", EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
            TfatCompErr = SetApiParameters();
            if (!String.IsNullOrEmpty(TfatCompErr))
            {
                jsonResult = Json(new { Status = "Error", errormsg = TfatCompErr }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }

            LRMaster lRMaster = new LRMaster();
            if (String.IsNullOrEmpty(Model.Document))
            {
                lRMaster = ctxTFAT.LRMaster.Where(x => x.EwayBill.ToString() == Model.SearchEway.Trim()).FirstOrDefault();
            }
            else
            {
                lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == Model.Document.Trim()).FirstOrDefault();
            }
            if (!String.IsNullOrEmpty(Model.Document))
            {
                if (lRMaster == null)
                {
                    jsonResult = Json(new { Status = "Error", errormsg = "Consignment Not Found...!" }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                    return jsonResult;
                }
            }
            bool Expired = false;
            if (lRMaster != null)
            {
                tfatEWB tfatEWB = ctxTFAT.tfatEWB.Where(x => x.LrTablekey == lRMaster.TableKey.ToString().Trim() && x.DocType == "LR000").FirstOrDefault();
                if (tfatEWB != null)
                {
                    if (tfatEWB.EWBValid != null)
                    {
                        DateTime EwayValidDate = tfatEWB.EWBValid.Value;
                        DateTime CurrentDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                        if (!(EwayValidDate >= CurrentDate))
                        {
                            Expired = true;
                        }
                    }
                }
            }

            if (Expired)
            {
                jsonResult = Json(new { Status = "Error", errormsg = "This Consignment Expired...!", EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
            else
            {
                StringBuilder jsonBuilder = new StringBuilder();
                string VehicleNo = "";
                HireVehicleMaster hireVehicle = ctxTFAT.HireVehicleMaster.Where(x => x.Code == Model.EWB_VehicleNo).FirstOrDefault();
                VehicleMaster vehicle = ctxTFAT.VehicleMaster.Where(x => x.Code == Model.EWB_VehicleNo).FirstOrDefault();

                if (hireVehicle != null)
                {
                    VehicleNo = Regex.Replace(hireVehicle.TruckNo, @"\s", "");
                    Model.EWB_LastVehicleNo = VehicleNo;
                }
                else if (vehicle != null)
                {
                    VehicleNo = Regex.Replace(vehicle.TruckNo, @"\s", "");
                    Model.EWB_LastVehicleNo = VehicleNo;
                }
                else
                {
                    VehicleNo = Regex.Replace(Model.EWB_VehicleNo, @"\s", "");
                    Model.EWB_LastVehicleNo = VehicleNo;
                }

                jsonBuilder.Append("{");
                jsonBuilder.Append("\"ewbNo\":" + Model.EWB_EwayNo.Trim() + ",");
                jsonBuilder.Append("\"fromPlace\":\"" + ctxTFAT.TfatBranch.Where(x => x.Code == Model.EWB_FromPlace).Select(x => x.Name).FirstOrDefault() + "\",");
                jsonBuilder.Append("\"fromState\":" + ctxTFAT.TfatState.Where(x => x.Code.ToString() == Model.EWB_ConsignorState.Trim()).Select(x => x.StateCode).FirstOrDefault() + ",");
                jsonBuilder.Append("\"toPlace\":\"" + ctxTFAT.TfatBranch.Where(x => x.Code == Model.EWB_ToPlace).Select(x => x.Name).FirstOrDefault() + "\",");
                jsonBuilder.Append("\"toState\":" + ctxTFAT.TfatState.Where(x => x.Code.ToString() == Model.EWB_ConsigneeState.Trim()).Select(x => x.StateCode).FirstOrDefault() + ",");
                jsonBuilder.Append("\"reasonCode\":\"" + Model.EWB_ReasonCode.Trim() + "\",");
                jsonBuilder.Append("\"reasonRem\":\"" + Model.EWB_ReasonRemark + "\",");
                jsonBuilder.Append("\"totalQuantity\":" + Model.EWB_TotalQty + ",");
                jsonBuilder.Append("\"unitCode\":\"" + Model.UnitCode + "\",");
                jsonBuilder.Append("\"transMode\":\"" + Model.EWB_TRNMode.Trim() + "\",");
                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                jsonBuilder.Append("},");
                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);

                JObject mjson = JObject.Parse(jsonBuilder.ToString());

                string mtoken = GetToken();
                if (mtoken != "0")
                {
                    RestClient client = new RestClient(weburl + initmultiurl);
                    RestRequest request = new RestRequest(Method.POST);
                    request.AddParameter("email", memail, ParameterType.QueryString);
                    request.AddHeader("ip_address", mipaddress);
                    request.AddHeader("client_id", mclientid);
                    request.AddHeader("client_secret", mclientsecret);
                    request.AddHeader("gstin", mgstin);
                    request.AddHeader("cache-control", "no-cache");
                    request.AddHeader("content-type", "application/json");
                    request.AddParameter("application/json", mjson, ParameterType.RequestBody);
                    var response = client.Execute(request);
                    string message = response.Content;
                    dynamic irnresponseorder = JObject.Parse(message);
                    string mreturn = JValue.Parse(irnresponseorder.GetValue("status_cd").ToString());

                    if (mreturn != "0")
                    {
                        jsonBuilder = new StringBuilder();
                        jsonBuilder.Append("{");
                        jsonBuilder.Append("\"ewbNo\":" + Model.EWB_EwayNo.Trim() + ",");
                        jsonBuilder.Append("\"vehicleNo\":\"" + VehicleNo.ToUpper() + "\",");
                        jsonBuilder.Append("\"groupNo\":\"" + 1 + "\",");
                        jsonBuilder.Append("\"transDocNo\":\"" + (lRMaster == null ? Model.EWBDocument.ToString().Trim() : lRMaster.LrNo.ToString()) + "\",");
                        jsonBuilder.Append("\"transDocDate\":\"" + Model.DocDate.Trim() + "\",");
                        jsonBuilder.Append("\"quantity\":" + Model.EWB_VehicleQty + ",");
                        jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                        jsonBuilder.Append("},");
                        jsonBuilder.Remove(jsonBuilder.Length - 1, 1);

                        mjson = JObject.Parse(jsonBuilder.ToString());

                        client = new RestClient(weburl + addmultiurl);
                        request = new RestRequest(Method.POST);
                        request.AddParameter("email", memail, ParameterType.QueryString);
                        request.AddHeader("ip_address", mipaddress);
                        request.AddHeader("client_id", mclientid);
                        request.AddHeader("client_secret", mclientsecret);
                        request.AddHeader("gstin", mgstin);
                        request.AddHeader("cache-control", "no-cache");
                        request.AddHeader("content-type", "application/json");
                        request.AddParameter("application/json", mjson, ParameterType.RequestBody);
                        response = client.Execute(request);
                        message = response.Content;
                        irnresponseorder = JObject.Parse(message);
                        mreturn = JValue.Parse(irnresponseorder.GetValue("status_cd").ToString());

                        if (mreturn != "0")
                        {
                            dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("data").ToString());

                            EwaybillNo = Model.EWB_EwayNo.Trim();
                            var Date = ctxTFAT.tfatEWB.Where(x => x.EWBNO == EwaybillNo).Select(x => x.EWBValid).FirstOrDefault();
                            if (Date != null)
                            {
                                EwatValidUpto = Date.Value.ToShortDateString();
                            }
                            errormsg += "Update Multi-Vehicle Sucessfully.\n Eway BillNo:" + EwaybillNo;
                            //Model.EWBDocument = String.IsNullOrEmpty(lRMaster.PartyInvoice) == true ? lRMaster.BENumber : lRMaster.PartyInvoice.ToString();
                            Model.Document = lRMaster == null ? null : lRMaster.LrNo.ToString();
                            SaveEWB(Model, "LR000", EwaybillNo.Trim(), EwatValidUpto, "Multi", lRMaster, lRMaster == null ? "" : lRMaster.TableKey);
                            SaveEWBLog("LR000", lRMaster == null ? " " : lRMaster.LrNo.ToString(), "Update Multi-Vehicle", "Sucess", EwaybillNo.Trim());
                        }
                        else
                        {
                            Status = "Error";
                            dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("error").ToString());
                            dynamic ErrorMsh = JValue.Parse(irnresponseData.GetValue("message").ToString());
                            var ErrorCodeList = ErrorMsh.GetValue("errorCodes").ToString();
                            errormsg = GetErrorString(ErrorCodeList);
                            SaveEWBLog("LR000", lRMaster == null ? " " : lRMaster.LrNo.ToString(), "Update Multi-Vehicle", errormsg, Model.EWB_EwayNo.Trim());
                        }
                    }
                    else
                    {
                        jsonResult = Json(new { Status = "Error", errormsg = "MultiVehicle Initialize Failed...!", EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
                        jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                        return jsonResult;
                    }

                }
                else
                {
                    jsonResult = Json(new { Status = "Error", errormsg = "Authentication Issue...!", EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                    return jsonResult;
                }
            }

            jsonResult = Json(new { Status = Status, errormsg = errormsg, EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        #endregion

        #region B-Part EwayBill

        //Get Respenctive Data
        public ActionResult GetBPartData(EwayBillVM Model)
        {
            string Errmsg = "", Status = "Success", TfatCompErr = "";

            TfatCompErr = SetApiParameters();

            if (String.IsNullOrEmpty(TfatCompErr))
            {
                LRMaster lRMaster = new LRMaster();
                if (String.IsNullOrEmpty(Model.Document))
                {
                    lRMaster = ctxTFAT.LRMaster.Where(x => x.EwayBill.ToString() == Model.SearchEway.Trim()).FirstOrDefault();
                }
                else
                {
                    lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == Model.Document.Trim()).FirstOrDefault();
                }
                if (lRMaster != null)
                {
                    if ((!String.IsNullOrEmpty(lRMaster.EwayBill)) && lRMaster.DecVal >= 50000)
                    {
                        tfatEwaySetup ewaySetup = ctxTFAT.tfatEwaySetup.FirstOrDefault();
                        if (ewaySetup != null)
                        {
                            Model.EWB_ReasonCode = ewaySetup.BPartReason;
                        }
                        Model.EWB_TRNMode = lRMaster.TrnMode;
                        Model.DocDate = lRMaster.BookDate.ToShortDateString();
                        Model.EWBDocument = String.IsNullOrEmpty(lRMaster.PartyInvoice) == true ? lRMaster.BENumber : lRMaster.PartyInvoice.ToString();
                        Model.EWB_EwayNo = lRMaster.EwayBill.Trim();

                        LCDetail lCDetail = ctxTFAT.LCDetail.Where(x => x.LRRefTablekey == lRMaster.TableKey).FirstOrDefault();
                        if (lCDetail != null)
                        {
                            LCMaster lCMaster = ctxTFAT.LCMaster.Where(x => x.TableKey == lCDetail.LCRefTablekey).FirstOrDefault();
                            if (lCMaster != null)
                            {
                                TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == lCMaster.FromBranch).FirstOrDefault();
                                if (tfatBranch != null)
                                {
                                    Model.EWB_Branch = tfatBranch.Code;
                                    Model.EWB_BranchName = tfatBranch.Name;
                                    if (!String.IsNullOrEmpty(tfatBranch.State))
                                    {
                                        TfatState tfatState = ctxTFAT.TfatState.Where(x => x.Code.ToString() == tfatBranch.State).FirstOrDefault();
                                        if (tfatState != null)
                                        {
                                            Model.EWB_ConsignorState = tfatState.Code.ToString();
                                            Model.EWB_ConsignorStateName = tfatState.Name;
                                        }
                                    }
                                }
                                if (lCMaster.DispachFM != 0)
                                {
                                    Model.EWB_VehicleNo = ctxTFAT.FMMaster.Where(x => x.TableKey == lCMaster.FMRefTablekey).Select(x => x.TruckNo).FirstOrDefault();
                                    if (Model.EWB_VehicleNo.Contains("H"))
                                    {
                                        Model.EWB_VehicleName = ctxTFAT.HireVehicleMaster.Where(x => x.Code == Model.EWB_VehicleNo).Select(x => x.TruckNo).FirstOrDefault();
                                    }
                                    else
                                    {
                                        Model.EWB_VehicleName = ctxTFAT.VehicleMaster.Where(x => x.Code == Model.EWB_VehicleNo).Select(x => x.TruckNo).FirstOrDefault();
                                    }
                                }
                            }
                            else
                            {
                                TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).FirstOrDefault();
                                if (tfatBranch != null)
                                {
                                    Model.EWB_Branch = tfatBranch.Code;
                                    Model.EWB_BranchName = tfatBranch.Name;
                                    if (!String.IsNullOrEmpty(tfatBranch.State))
                                    {
                                        TfatState tfatState = ctxTFAT.TfatState.Where(x => x.Code.ToString() == tfatBranch.State).FirstOrDefault();
                                        if (tfatState != null)
                                        {
                                            Model.EWB_ConsignorState = tfatState.Code.ToString();
                                            Model.EWB_ConsignorStateName = tfatState.Name;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).FirstOrDefault();
                            if (tfatBranch != null)
                            {
                                Model.EWB_Branch = tfatBranch.Code;
                                Model.EWB_BranchName = tfatBranch.Name;
                                if (!String.IsNullOrEmpty(tfatBranch.State))
                                {
                                    TfatState tfatState = ctxTFAT.TfatState.Where(x => x.Code.ToString() == tfatBranch.State).FirstOrDefault();
                                    if (tfatState != null)
                                    {
                                        Model.EWB_ConsignorState = tfatState.Code.ToString();
                                        Model.EWB_ConsignorStateName = tfatState.Name;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Status = "Error";
                        Errmsg = "This Consignment EwayBillno and Declare Value Does Not Match Required Criteria.\n Please Check It Consignment Details...!";
                    }
                }
                else
                {
                    Model.EWB_EwayNo = Model.SearchEway.Trim();
                }
                if (!String.IsNullOrEmpty(Model.Document))
                {
                    if (lRMaster == null)
                    {
                        Status = "Error";
                        Errmsg = "Consignment Not Found...!";
                    }
                }
            }
            else
            {
                Status = "Error";
                Errmsg = TfatCompErr;
            }
            if (String.IsNullOrEmpty(Errmsg))
            {
                #region GetBpart Details (Vehicle History)
                string mtoken = GetToken();
                if (mtoken != "0")
                {
                    var URL = new UriBuilder(weburl + getewaybillurl);
                    var queryString = HttpUtility.ParseQueryString(string.Empty);
                    queryString["email"] = memail;
                    queryString["ewbNo"] = Model.EWB_EwayNo.Trim();
                    URL.Query = queryString.ToString();
                    var client = new WebClient();
                    client.Headers.Add("ip_address", mipaddress);
                    client.Headers.Add("client_id", mclientid);
                    client.Headers.Add("client_secret", mclientsecret);
                    client.Headers.Add("gstin", mgstin);
                    string mstr = client.DownloadString(URL.ToString());
                    dynamic mjson = Newtonsoft.Json.Linq.JValue.Parse(Newtonsoft.Json.JsonConvert.DeserializeObject(mstr).ToString());
                    string mreturn = mjson.GetValue("status_cd").ToString();

                    if (mreturn != "0")
                    {
                        //var result = JsonConvert.DeserializeObject<Root>(mjson.GetValue("data").ToString());
                        dynamic result = JsonConvert.DeserializeObject(mstr);
                        var VehicleDetails = result.data.VehiclListDetails;
                        List<VehiclListDetail> VehiclListHistoryDetails = new List<VehiclListDetail>();
                        foreach (var item in VehicleDetails)
                        {
                            VehiclListDetail vehiclList = new VehiclListDetail();
                            vehiclList.updMode = item.updMode;
                            vehiclList.vehicleNo = item.vehicleNo;
                            vehiclList.fromPlace = item.fromPlace;
                            vehiclList.fromState = Convert.ToInt32(item.fromState);
                            vehiclList.tripshtNo = Convert.ToInt32(item.tripshtNo);
                            vehiclList.userGSTINTransin = item.userGSTINTransin;
                            var enteredDate = item.enteredDate.ToString();
                            var enteredDate1 = ConvertDDMMYYTOYYMMDD(enteredDate);
                            vehiclList.enteredDate = enteredDate1.ToShortDateString();
                            var transMode = item.transMode.ToString();
                            vehiclList.transMode = transMode == "1" ? "Road" : transMode == "2" ? "Rail" : transMode == "3" ? "Air" : "Ship";
                            vehiclList.transDocNo = item.transDocNo.ToString();
                            var transDocDate = item.transDocDate.ToString();
                            vehiclList.transDocDate = transDocDate;
                            vehiclList.groupNo = item.groupNo.ToString();
                            VehiclListHistoryDetails.Add(vehiclList);
                        }
                        Model.VehiclListHistoryDetails = VehiclListHistoryDetails;
                    }
                }
                #endregion
            }

            var html = ViewHelper.RenderPartialView(this, "BPartEway", Model);
            var jsonResult = Json(new
            {
                Status = Status,
                Errmsg = Errmsg,
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        //Update B-Part Eway-Bill
        public ActionResult GenerateBpartEWayBill(EwayBillVM Model)
        {
            //Model.Type = "1";//Generate New EwayBill
            string Status = "Success", EwaybillNo = "", EwatValidUpto = "", errormsg = "", TfatCompErr = "";

            JsonResult jsonResult = new JsonResult();

            if (String.IsNullOrEmpty(Model.Document) && String.IsNullOrEmpty(Model.SearchEway))
            {
                jsonResult = Json(new { Status = "Error", errormsg = "Document Missing...!", EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
            TfatCompErr = SetApiParameters();
            if (!String.IsNullOrEmpty(TfatCompErr))
            {
                jsonResult = Json(new { Status = "Error", errormsg = TfatCompErr }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }

            LRMaster lRMaster = new LRMaster();
            if (String.IsNullOrEmpty(Model.Document))
            {
                lRMaster = ctxTFAT.LRMaster.Where(x => x.EwayBill.ToString() == Model.SearchEway.Trim()).FirstOrDefault();
            }
            else
            {
                lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == Model.Document.Trim()).FirstOrDefault();
            }
            if (!String.IsNullOrEmpty(Model.Document))
            {
                if (lRMaster == null)
                {
                    jsonResult = Json(new { Status = "Error", errormsg = "Consignment Not Found...!" }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                    return jsonResult;
                }
            }
            bool Expired = false;
            if (lRMaster != null)
            {
                tfatEWB tfatEWB = ctxTFAT.tfatEWB.Where(x => x.LrTablekey == lRMaster.TableKey.ToString().Trim() && x.DocType == "LR000").FirstOrDefault();
                if (tfatEWB != null)
                {
                    if (tfatEWB.EWBValid != null)
                    {
                        DateTime EwayValidDate = tfatEWB.EWBValid.Value;
                        DateTime CurrentDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                        if (!(EwayValidDate >= CurrentDate))
                        {
                            Expired = true;
                        }
                    }
                }
            }

            if (Expired)
            {
                jsonResult = Json(new { Status = "Error", errormsg = "This Consignment Expired...!", EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
            else
            {
                StringBuilder jsonBuilder = new StringBuilder();
                string VehicleNo = "";
                HireVehicleMaster hireVehicle = ctxTFAT.HireVehicleMaster.Where(x => x.Code == Model.EWB_VehicleNo).FirstOrDefault();
                VehicleMaster vehicle = ctxTFAT.VehicleMaster.Where(x => x.Code == Model.EWB_VehicleNo).FirstOrDefault();

                if (hireVehicle != null)
                {
                    VehicleNo = Regex.Replace(hireVehicle.TruckNo, @"\s", "");
                    Model.EWB_LastVehicleNo = VehicleNo;
                }
                else if (vehicle != null)
                {
                    VehicleNo = Regex.Replace(vehicle.TruckNo, @"\s", "");
                    Model.EWB_LastVehicleNo = VehicleNo;
                }
                else
                {
                    VehicleNo = Regex.Replace(Model.EWB_VehicleNo, @"\s", "");
                    Model.EWB_LastVehicleNo = VehicleNo;
                }

                jsonBuilder.Append("{");
                jsonBuilder.Append("\"ewbNo\":" + Model.EWB_EwayNo.Trim() + ",");
                jsonBuilder.Append("\"vehicleNo\":\"" + VehicleNo.ToUpper() + "\",");
                jsonBuilder.Append("\"fromPlace\":\"" + ctxTFAT.TfatBranch.Where(x => x.Code == Model.EWB_Branch).Select(x => x.Name).FirstOrDefault() + "\",");
                jsonBuilder.Append("\"fromState\":" + ctxTFAT.TfatState.Where(x => x.Code.ToString() == Model.EWB_ConsignorState.Trim()).Select(x => x.StateCode).FirstOrDefault() + ",");
                jsonBuilder.Append("\"reasonCode\":\"" + Model.EWB_ReasonCode.Trim() + "\",");
                jsonBuilder.Append("\"reasonRem\":\"" + " " + "\",");
                jsonBuilder.Append("\"transDocNo\":\"" + (lRMaster == null ? Model.EWBDocument : lRMaster.LrNo.ToString()) + "\",");
                jsonBuilder.Append("\"transDocDate\":\"" + Model.DocDate.Trim() + "\",");
                jsonBuilder.Append("\"transMode\":\"" + Model.EWB_TRNMode.Trim() + "\",");
                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                jsonBuilder.Append("},");
                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);


                JObject mjson = JObject.Parse(jsonBuilder.ToString());

                string mtoken = GetToken();
                if (mtoken == "0")
                {
                    jsonResult = Json(new { Status = "Error", errormsg = "Authentication Not Sucessed...!" }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                    return jsonResult;
                }
                else
                {
                    RestClient client = new RestClient(weburl + genewayBParturl);
                    RestRequest request = new RestRequest(Method.POST);
                    request.AddParameter("email", memail, ParameterType.QueryString);
                    request.AddHeader("ip_address", mipaddress);
                    request.AddHeader("client_id", mclientid);
                    request.AddHeader("client_secret", mclientsecret);
                    request.AddHeader("gstin", mgstin);
                    request.AddHeader("cache-control", "no-cache");
                    request.AddHeader("content-type", "application/json");
                    request.AddParameter("application/json", mjson, ParameterType.RequestBody);
                    var response = client.Execute(request);
                    string message = response.Content;
                    dynamic irnresponseorder = JObject.Parse(message);
                    string mreturn = JValue.Parse(irnresponseorder.GetValue("status_cd").ToString());

                    if (mreturn != "0")
                    {
                        dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("data").ToString());

                        EwaybillNo = Model.EWB_EwayNo.Trim();
                        EwatValidUpto = irnresponseData.GetValue("validUpto").ToString();
                        errormsg += "Update B-Part Sucessfully.\n Eway BillNo:" + EwaybillNo + " \n Valid Upto : " + EwatValidUpto + "";
                        Model.Document = lRMaster == null ? null : lRMaster.LrNo.ToString();
                        SaveEWB(Model, "LR000", EwaybillNo.Trim(), irnresponseData.GetValue("validUpto").ToString(), "BPart", lRMaster, lRMaster == null ? "" : lRMaster.TableKey);
                        SaveEWBLog("LR000", lRMaster == null ? " " : lRMaster.LrNo.ToString(), "Update B-Part", "Sucess", EwaybillNo.Trim());
                    }
                    else
                    {
                        Status = "Error";
                        dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("error").ToString());
                        dynamic ErrorMsh = JValue.Parse(irnresponseData.GetValue("message").ToString());
                        var ErrorCodeList = ErrorMsh.GetValue("errorCodes").ToString();
                        errormsg = GetErrorString(ErrorCodeList);
                        SaveEWBLog("LR000", lRMaster == null ? " " : lRMaster.LrNo.ToString(), "Update B-Part", errormsg, Model.EWB_EwayNo.Trim());
                    }
                }
            }
            jsonResult = Json(new { Status = Status, errormsg = errormsg, EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        #endregion

        #region Console EwayBill

        //Get DataIntoGrid
        public ActionResult ConsoleGrid(EwayBillVM Model)
        {
            List<EwayBillVM> ConsoleList = new List<EwayBillVM>();
            List<EwayBillVM> ExistEWAY = (List<EwayBillVM>)Session["EwayBillVMCon"];
            if (ExistEWAY == null)
            {
                ExistEWAY = new List<EwayBillVM>();
            }
            ExistEWAY.AddRange(Model.ConsoleList);
            Session["EwayBillVMCon"] = ExistEWAY;
            var html = ViewHelper.RenderPartialView(this, "ConsoleGrid", ExistEWAY);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        //Delete Eway Bill From The Session
        [HttpPost]
        public ActionResult DeleteConsoleEway(EwayBillVM Model)
        {
            var result2 = (List<EwayBillVM>)Session["EwayBillVMCon"];
            if (result2 == null)
            {
                result2 = new List<EwayBillVM>();
            }
            var result = result2.Where(x => x.LREwayNo != Model.LREwayNo).ToList();
            Session["EwayBillVMCon"] = result;
            return Json(new
            {
                Status = "Sucess"

            }, JsonRequestBehavior.AllowGet);
        }

        //Check Mult Document OF Lorry Challan In System
        [HttpPost]
        public ActionResult FetchLCDocumentList(LorryReceiptQueryVM Model)
        {
            List<LorryReceiptQueryVM> ValueList = new List<LorryReceiptQueryVM>();

            var mlrmaster = ctxTFAT.LCMaster.Where(x => x.LCno.ToString() == Model.Lrno).Select(x => x).ToList();
            if (mlrmaster == null)
            {
                Model.Status = "ValidError";
                Model.Message = "Lorry Challan Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            if (mlrmaster.Count() == 0)
            {
                Model.Status = "ValidError";
                Model.Message = "Lorry Challan Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            else if (mlrmaster.Count() == 1)
            {
                Model.Status = "Processed";
                Model.Message = mlrmaster.Select(x => x.TableKey).FirstOrDefault();
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            foreach (var item in mlrmaster)
            {
                LorryReceiptQueryVM otherTransact = new LorryReceiptQueryVM();
                otherTransact.ConsignmentKey = item.TableKey.ToString();
                otherTransact.Lrno = item.LCno.ToString();
                otherTransact.LrDate = item.Date;
                otherTransact.LrQty = item.TotalQty.ToString();
                otherTransact.LRBranch = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault();
                otherTransact.LrFrom = ctxTFAT.TfatBranch.Where(x => x.Code == item.FromBranch).Select(x => x.Name).FirstOrDefault();
                otherTransact.LrTo = ctxTFAT.TfatBranch.Where(x => x.Code == item.ToBranch).Select(x => x.Name).FirstOrDefault();
                ValueList.Add(otherTransact);
            }
            Model.ValueList = ValueList;
            var html = ViewHelper.RenderPartialView(this, "LorryChallanList", Model);
            return Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
        }

        //Get Consignmnet Whos Have The Ewaybill In Lorry Challan. 
        public ActionResult GetLorryChallanEwayBillsOfConsignment(EwayBillVM Model)
        {
            List<EwayBillVM> ConsoleList = new List<EwayBillVM>();
            List<EwayBillVM> ExistEWAY = new List<EwayBillVM>();

            var Lcdetails = ctxTFAT.LCDetail.Where(x => x.LCRefTablekey == Model.LRrefKey).ToList();
            if (Lcdetails != null)
            {
                foreach (var item in Lcdetails)
                {
                    LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey == item.LRRefTablekey).FirstOrDefault();
                    if (lRMaster != null)
                    {
                        var EwayStock = ctxTFAT.tfatEWB.Where(x => x.LrTablekey == item.LRRefTablekey).FirstOrDefault();
                        if (EwayStock != null)
                        {
                            EwayBillVM ewayBill = new EwayBillVM();
                            ewayBill.LREwayNo = EwayStock.EWBNO;
                            if (EwayStock.EWBValid != null)
                            {
                                ewayBill.LREwayValid = EwayStock.EWBValid.Value.ToShortDateString();
                            }
                            ewayBill.LRno = lRMaster.LrNo.ToString();
                            ewayBill.LRDate = lRMaster.BookDate.ToShortDateString();
                            ewayBill.Consingor = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.RecCode).Select(x => x.Name).FirstOrDefault();
                            ewayBill.Consignee = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.SendCode).Select(x => x.Name).FirstOrDefault();
                            ewayBill.From = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Source).Select(x => x.Name).FirstOrDefault();
                            ewayBill.To = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name).FirstOrDefault();
                            ewayBill.LRrefKey = lRMaster.TableKey;
                            if (ExistEWAY.Where(x => x.LREwayNo == EwayStock.EWBNO).FirstOrDefault() == null)
                            {
                                ExistEWAY.Add(ewayBill);
                            }
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(lRMaster.EwayBill))
                            {
                                EwayBillVM ewayBill = new EwayBillVM();
                                ewayBill.LREwayNo = lRMaster.EwayBill;
                                ewayBill.LREwayValid = "";
                                ewayBill.LRno = lRMaster.LrNo.ToString();
                                ewayBill.LRDate = lRMaster.BookDate.ToShortDateString();
                                ewayBill.Consingor = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.RecCode).Select(x => x.Name).FirstOrDefault();
                                ewayBill.Consignee = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.SendCode).Select(x => x.Name).FirstOrDefault();
                                ewayBill.From = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Source).Select(x => x.Name).FirstOrDefault();
                                ewayBill.To = ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name).FirstOrDefault();
                                ewayBill.LRrefKey = lRMaster.TableKey;
                                if (ExistEWAY.Where(x => x.LREwayNo == lRMaster.EwayBill).FirstOrDefault() == null)
                                {
                                    ExistEWAY.Add(ewayBill);
                                }

                            }
                        }
                    }
                }
            }
            Session["EwayBillVMCon"] = ExistEWAY;

            string FromPLace = "", FromPLaceCode = "", VehicleNo = "", VehicleNoCode = "", StateName = "", StateCode = "";
            LCMaster lCMaster = ctxTFAT.LCMaster.Where(x => x.TableKey == Model.LRrefKey).FirstOrDefault();
            if (lCMaster != null)
            {
                FromPLaceCode = lCMaster.FromBranch;
                FromPLace = ctxTFAT.TfatBranch.Where(x => x.Code == lCMaster.FromBranch).Select(x => x.Name).FirstOrDefault();
                VehicleNoCode = ctxTFAT.FMMaster.Where(x => x.TableKey == lCMaster.FMRefTablekey).Select(x => x.TruckNo).FirstOrDefault();
                if (VehicleNoCode != null)
                {
                    VehicleNo = ctxTFAT.VehicleMaster.Where(x => x.Code == VehicleNoCode).Select(x => x.TruckNo).FirstOrDefault();
                    if (String.IsNullOrEmpty(VehicleNo))
                    {
                        VehicleNo = ctxTFAT.HireVehicleMaster.Where(x => x.Code == VehicleNoCode).Select(x => x.TruckNo).FirstOrDefault();
                    }
                }
                StateCode = ctxTFAT.TfatBranch.Where(x => x.Code == FromPLaceCode).Select(x => x.State).FirstOrDefault();
                if (!String.IsNullOrEmpty(StateCode))
                {
                    StateName = ctxTFAT.TfatState.Where(x => x.Code.ToString() == StateCode).Select(x => x.Name).FirstOrDefault();
                }
            }

            var html = ViewHelper.RenderPartialView(this, "ConsoleGrid", ExistEWAY);
            return Json(new { Html = html, FromPLace = FromPLace, FromPLaceCode = FromPLaceCode, VehicleNo = VehicleNo, VehicleNoCode = VehicleNoCode, StateName = StateName, StateCode = StateCode }, JsonRequestBehavior.AllowGet);
        }

        //Set Empty Console Session
        public ActionResult SetEmptySession(EwayBillVM Model)
        {
            Session["EwayBillVMCon"] = null;
            return Json(new
            {
                Status = "Sucess"

            }, JsonRequestBehavior.AllowGet);
        }

        //Generate Console EwyBill
        public ActionResult GenerateConsoleBill(EwayBillVM Model)
        {
            //Model.Type = "1";//Generate New EwayBill
            string Status = "Success", EwaybillNo = "", EwatValidUpto = "", errormsg = "", TfatCompErr = "";
            JsonResult jsonResult = new JsonResult();

            TfatCompErr = SetApiParameters();
            if (!String.IsNullOrEmpty(TfatCompErr))
            {
                jsonResult = Json(new { Status = "Error", errormsg = TfatCompErr }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }

            StringBuilder jsonBuilder = new StringBuilder();
            string VehicleNo = "";
            HireVehicleMaster hireVehicle = ctxTFAT.HireVehicleMaster.Where(x => x.Code == Model.EWB_VehicleNo).FirstOrDefault();
            VehicleMaster vehicle = ctxTFAT.VehicleMaster.Where(x => x.Code == Model.EWB_VehicleNo).FirstOrDefault();

            if (hireVehicle != null)
            {
                VehicleNo = Regex.Replace(hireVehicle.TruckNo, @"\s", "");
                Model.EWB_LastVehicleNo = VehicleNo;
            }
            else if (vehicle != null)
            {
                VehicleNo = Regex.Replace(vehicle.TruckNo, @"\s", "");
                Model.EWB_LastVehicleNo = VehicleNo;
            }
            else
            {
                VehicleNo = Regex.Replace(Model.EWB_VehicleNo, @"\s", "");
                Model.EWB_LastVehicleNo = VehicleNo;
            }

            jsonBuilder.Append("{");

            jsonBuilder.Append("\"fromPlace\":\"" + (ctxTFAT.TfatBranch.Where(x => x.Code == Model.EWB_FromPlace).Select(x => x.Name).FirstOrDefault()) + "\",");
            jsonBuilder.Append("\"fromState\":" + ctxTFAT.TfatState.Where(x => x.Code.ToString() == Model.EWB_ConsignorState.Trim()).Select(x => x.StateCode).FirstOrDefault() + ",");
            jsonBuilder.Append("\"vehicleNo\":\"" + VehicleNo.ToUpper() + "\",");
            jsonBuilder.Append("\"transMode\":\"" + Model.EWB_TRNMode + "\",");
            jsonBuilder.Append("\"transDocNo\":\"" + Model.EWBDocument + "\",");
            jsonBuilder.Append("\"transDocDate\":\"" + Model.DocDate + "\",");
            jsonBuilder.Append("\"tripSheetEwbBills\":[");

            #region ItemList

            foreach (var item in Model.ConsoleList)
            {
                jsonBuilder.Append("{");
                jsonBuilder.Append("\"ewbNo\":" + item.LREwayNo + ",");
                jsonBuilder.Append("},");
            }


            #endregion

            jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            jsonBuilder.Append("]},");

            jsonBuilder.Remove(jsonBuilder.Length - 1, 1);

            JObject mjson = JObject.Parse(jsonBuilder.ToString());

            string mtoken = GetToken();
            if (mtoken != "0")
            {
                RestClient client = new RestClient(weburl + genewayCosoleurl);
                RestRequest request = new RestRequest(Method.POST);
                request.AddParameter("email", memail, ParameterType.QueryString);
                request.AddHeader("ip_address", mipaddress);
                request.AddHeader("client_id", mclientid);
                request.AddHeader("client_secret", mclientsecret);
                request.AddHeader("gstin", mgstin);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/json");
                request.AddParameter("application/json", mjson, ParameterType.RequestBody);
                var response = client.Execute(request);
                string message = response.Content;
                dynamic irnresponseorder = JObject.Parse(message);
                string mreturn = JValue.Parse(irnresponseorder.GetValue("status_cd").ToString());
                if (mreturn != "0")
                {
                    dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("data").ToString());

                    EwaybillNo = irnresponseData.GetValue("cEwbNo").ToString();
                    EwatValidUpto = irnresponseData.GetValue("cEwbDate").ToString();
                    errormsg += "Generate Console EwayBill Sucessfully.\n Console Eway BillNo:" + EwaybillNo;

                    var EwayList = string.Join(",", Model.ConsoleList.Select(x => x.LREwayNo).ToList());
                    UpdateConsoleEWB(Model.ConsoleList.Select(x => x.LREwayNo).ToList(), irnresponseData.GetValue("cEwbNo").ToString(), irnresponseData.GetValue("cEwbDate").ToString());
                    SaveEWBLog("LR000", "", "Generate Console Eway Bill Of :" + EwayList, "Sucess", irnresponseData.GetValue("cEwbNo").ToString());
                }
                else
                {
                    Status = "Error";
                    dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("error").ToString());
                    dynamic ErrorMsh = JValue.Parse(irnresponseData.GetValue("message").ToString());
                    var ErrorCodeList = ErrorMsh.GetValue("errorCodes").ToString();
                    errormsg = GetErrorString(ErrorCodeList);
                    SaveEWBLog("LR000", "     ", "Generate Console Eway Bill", errormsg, "");
                }
            }
            else
            {
                jsonResult = Json(new { Status = "Error", errormsg = "Authentication Issue...!", EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }

            jsonResult = Json(new { Status = Status, errormsg = errormsg, EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        #endregion

        #region Extend EwayBill

        //Get Single EwayBill Respenctive Data
        public ActionResult GetExtendEway(EwayBillVM Model)
        {
            string Errmsg = "", Status = "Success", TfatCompErr = "";
            TfatCompErr = SetApiParameters();

            if (String.IsNullOrEmpty(TfatCompErr))
            {
                LRMaster lRMaster = new LRMaster();
                if (String.IsNullOrEmpty(Model.Document))
                {
                    lRMaster = ctxTFAT.LRMaster.Where(x => x.EwayBill.ToString() == Model.SearchEway.Trim()).FirstOrDefault();
                }
                else
                {
                    lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == Model.Document.Trim()).FirstOrDefault();
                }
                if (lRMaster != null)
                {
                    if ((!String.IsNullOrEmpty(lRMaster.EwayBill)) && lRMaster.DecVal >= 50000)
                    {
                        bool ProcessedOrNot = false;
                        tfatEWB tfatEWB = ctxTFAT.tfatEWB.Where(x => x.LrTablekey == lRMaster.TableKey.ToString() && x.DocType == "LR000").FirstOrDefault();
                        if (tfatEWB != null)
                        {
                            if (tfatEWB.EWBValid == null)
                            {
                                ProcessedOrNot = true;
                            }
                            else
                            {
                                DateTime CurrentDate = DateTime.Now;
                                CurrentDate = ConvertDDMMYYTOYYMMDD(CurrentDate.ToShortDateString());
                                CurrentDate = CurrentDate.AddHours(11);
                                CurrentDate = CurrentDate.AddMinutes(59);
                                DateTime validUpto = ConvertDDMMYYTOYYMMDD(tfatEWB.EWBValid.Value.ToShortDateString());
                                validUpto = validUpto.AddHours(11);
                                validUpto = validUpto.AddMinutes(59);
                                DateTime Before = validUpto.AddHours(-8);
                                DateTime After = validUpto.AddHours(8);
                                if (Before <= CurrentDate && CurrentDate <= After)
                                {
                                    ProcessedOrNot = true;
                                }
                                else
                                {
                                    Status = "Error";
                                    Errmsg = "Eway Bill Extend Only 8 hours Before and 8 hours After Expiry Date not Too Early...!\nEway Bill Expired On :" + validUpto + "\nUser Extend This EwayBill :" + Before + " Onwards...!";

                                }
                            }
                        }
                        else
                        {
                            ProcessedOrNot = true;
                        }

                        if (ProcessedOrNot)
                        {
                            tfatEwaySetup ewaySetup = ctxTFAT.tfatEwaySetup.FirstOrDefault();
                            if (ewaySetup != null)
                            {
                                Model.EWB_ConsignmentCode = ewaySetup.ExtConsignIs;
                                Model.EWB_TRNType = ewaySetup.ExtTranType;
                                Model.EWB_ExtendReasonCode = ewaySetup.ExtReason;
                            }

                            var VehicleCode = Fieldoftable("VehicleMaster", "Code", "REPLACE(Truckno, ' ', '')='" + tfatEWB.VehicleNo.Trim() + "'", "V");
                            if (String.IsNullOrEmpty(VehicleCode))
                            {
                                VehicleCode = Fieldoftable("HireVehicleMaster", "Code", "REPLACE(Truckno, ' ', '')='" + tfatEWB.VehicleNo.Trim() + "'", "V");
                            }

                            HireVehicleMaster hireVehicle = ctxTFAT.HireVehicleMaster.Where(x => x.Code == VehicleCode).FirstOrDefault();
                            VehicleMaster vehicle = ctxTFAT.VehicleMaster.Where(x => x.Code == VehicleCode).FirstOrDefault();
                            if (hireVehicle != null)
                            {
                                Model.EWB_VehicleNo = hireVehicle.Code;
                                Model.EWB_VehicleName = hireVehicle.TruckNo.Trim();
                            }
                            else if (vehicle != null)
                            {
                                Model.EWB_VehicleNo = vehicle.Code;
                                Model.EWB_VehicleName = vehicle.TruckNo.Trim();
                            }
                            else
                            {
                                Model.EWB_VehicleNoTxt = tfatEWB.VehicleNo.Trim();
                            }

                            Model.EWB_EwayNo = lRMaster.EwayBill;
                            Model.EWB_TRNMode = lRMaster.TrnMode;
                            Model.DocDate = lRMaster.BookDate.ToShortDateString();
                            Model.EWBDocument = String.IsNullOrEmpty(lRMaster.PartyInvoice) == true ? lRMaster.BENumber : lRMaster.PartyInvoice.ToString();
                            Model.EWB_EwayNo = lRMaster.EwayBill.Trim();


                            TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).FirstOrDefault();
                            if (tfatBranch != null)
                            {
                                Model.EWB_FromPlace = tfatBranch.Code;
                                Model.EWB_FromPlaceName = tfatBranch.Name;
                                if (!String.IsNullOrEmpty(tfatBranch.State))
                                {
                                    TfatState tfatState = ctxTFAT.TfatState.Where(x => x.Code.ToString() == tfatBranch.State).FirstOrDefault();
                                    if (tfatState != null)
                                    {
                                        Model.EWB_ConsignorState = tfatState.Code.ToString();
                                        Model.EWB_ConsignorStateName = tfatState.Name;
                                    }
                                }
                                Model.EWB_ConsignorPincode = tfatBranch.aPin;
                            }
                        }
                    }
                    else
                    {
                        Status = "Error";
                        Errmsg = "This Consignment EwayBillno and Declare Value Does Not Match Required Criteria.\n Please Check It Consignment Details...!";
                    }
                }
                else
                {
                    Model.EWB_EwayNo = Model.SearchEway.Trim();
                }
                if (!String.IsNullOrEmpty(Model.Document))
                {
                    if (lRMaster == null)
                    {
                        Status = "Error";
                        Errmsg = "Consignment Not Found...!";
                    }
                }
            }
            else
            {
                Status = "Error";
                Errmsg = TfatCompErr;
            }
            var html = ViewHelper.RenderPartialView(this, "ExtendEway", Model);
            var jsonResult = Json(new
            {
                Status = Status,
                Errmsg = Errmsg,
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        //Extend Single Eway-Bill
        public ActionResult ExtendEWayBill(EwayBillVM Model)
        {
            //Model.Type = "1";//Generate New EwayBill
            string Status = "Success", EwaybillNo = "", EwatValidUpto = "", errormsg = "", TfatCompErr = "";
            JsonResult jsonResult = new JsonResult();

            if (String.IsNullOrEmpty(Model.Document) && String.IsNullOrEmpty(Model.SearchEway))
            {
                jsonResult = Json(new { Status = "Error", errormsg = "Document Missing...!", EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
            TfatCompErr = SetApiParameters();
            if (!String.IsNullOrEmpty(TfatCompErr))
            {
                jsonResult = Json(new { Status = "Error", errormsg = TfatCompErr }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }

            LRMaster lRMaster = new LRMaster();
            if (String.IsNullOrEmpty(Model.Document))
            {
                lRMaster = ctxTFAT.LRMaster.Where(x => x.EwayBill.ToString() == Model.SearchEway.Trim()).FirstOrDefault();
            }
            else
            {
                lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == Model.Document.Trim()).FirstOrDefault();
            }
            if (!String.IsNullOrEmpty(Model.Document))
            {
                if (lRMaster == null)
                {
                    jsonResult = Json(new { Status = "Error", errormsg = "Consignment Not Found...!" }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                    return jsonResult;
                }
            }

            bool Expired = false;
            if (lRMaster != null)
            {
                tfatEWB tfatEWB = ctxTFAT.tfatEWB.Where(x => x.LrTablekey == lRMaster.TableKey.ToString().Trim() && x.DocType == "LR000").FirstOrDefault();
                if (tfatEWB != null)
                {
                    if (tfatEWB.EWBValid != null)
                    {
                        DateTime EwayValidDate = tfatEWB.EWBValid.Value;
                        DateTime CurrentDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                        if (!(EwayValidDate >= CurrentDate))
                        {
                            Expired = true;
                        }
                    }
                }
            }


            if (Expired)
            {
                jsonResult = Json(new { Status = "Error", errormsg = "This Consignment Expired...!", EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
            else
            {
                StringBuilder jsonBuilder = new StringBuilder();
                string VehicleNo = "";
                HireVehicleMaster hireVehicle = ctxTFAT.HireVehicleMaster.Where(x => x.Code == Model.EWB_VehicleNo).FirstOrDefault();
                VehicleMaster vehicle = ctxTFAT.VehicleMaster.Where(x => x.Code == Model.EWB_VehicleNo).FirstOrDefault();

                if (hireVehicle != null)
                {
                    VehicleNo = Regex.Replace(hireVehicle.TruckNo, @"\s", "");
                    Model.EWB_LastVehicleNo = VehicleNo;
                }
                else if (vehicle != null)
                {
                    VehicleNo = Regex.Replace(vehicle.TruckNo, @"\s", "");
                    Model.EWB_LastVehicleNo = VehicleNo;
                }
                else
                {
                    VehicleNo = Regex.Replace(Model.EWB_VehicleNo, @"\s", "");
                    Model.EWB_LastVehicleNo = VehicleNo;
                }

                jsonBuilder.Append("{");
                jsonBuilder.Append("\"ewbNo\":" + Model.EWB_EwayNo.Trim() + ",");
                jsonBuilder.Append("\"vehicleNo\":\"" + VehicleNo + "\",");
                jsonBuilder.Append("\"fromPlace\":\"" + ctxTFAT.TfatBranch.Where(x => x.Code == Model.EWB_FromPlace).Select(x => x.Name).FirstOrDefault() + "\",");
                jsonBuilder.Append("\"fromState\":" + ctxTFAT.TfatState.Where(x => x.Code.ToString() == Model.EWB_ConsignorState.Trim()).Select(x => x.StateCode).FirstOrDefault() + ",");
                jsonBuilder.Append("\"remainingDistance\":" + Model.EWB_Distance.Trim() + ",");
                jsonBuilder.Append("\"transDocNo\":\"" + (lRMaster == null ? Model.EWBDocument : lRMaster.LrNo.ToString()) + "\",");
                jsonBuilder.Append("\"transDocDate\":\"" + Model.DocDate + "\",");
                jsonBuilder.Append("\"transMode\":\"" + 5 + "\",");
                jsonBuilder.Append("\"extnRsnCode\":" + Model.EWB_ExtendReasonCode.Trim() + ",");
                jsonBuilder.Append("\"extnRemarks\":\"" + Model.EWB_ExtendReasonRemark + "\",");
                jsonBuilder.Append("\"fromPincode\":" + Model.EWB_ConsignorPincode.Trim() + ",");
                jsonBuilder.Append("\"consignmentStatus\":\"" + Model.EWB_ConsignmentCode.Trim() + "\",");
                jsonBuilder.Append("\"transitType\":\"" + "R" + "\",");
                jsonBuilder.Append("\"addressLine1\":\"" + ctxTFAT.TfatBranch.Where(x => x.Code == Model.EWB_FromPlace).Select(x => x.Name).FirstOrDefault() + "\",");
                jsonBuilder.Append("\"addressLine2\":\"" + ctxTFAT.TfatBranch.Where(x => x.Code == Model.EWB_FromPlace).Select(x => x.Name).FirstOrDefault() + "\",");
                jsonBuilder.Append("\"addressLine3\":\"" + ctxTFAT.TfatBranch.Where(x => x.Code == Model.EWB_FromPlace).Select(x => x.Name).FirstOrDefault() + "\",");

                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                jsonBuilder.Append("},");
                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);

                JObject mjson = JObject.Parse(jsonBuilder.ToString());

                string mtoken = GetToken();
                if (mtoken == "0")
                {
                    jsonResult = Json(new { Status = "Error", errormsg = "Authentication Not Sucessed...!" }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                    return jsonResult;
                }
                else
                {
                    RestClient client = new RestClient(weburl + extendewayurl);
                    RestRequest request = new RestRequest(Method.POST);
                    request.AddParameter("email", memail, ParameterType.QueryString);
                    request.AddHeader("ip_address", mipaddress);
                    request.AddHeader("client_id", mclientid);
                    request.AddHeader("client_secret", mclientsecret);
                    request.AddHeader("gstin", mgstin);
                    request.AddHeader("cache-control", "no-cache");
                    request.AddHeader("content-type", "application/json");
                    request.AddParameter("application/json", mjson, ParameterType.RequestBody);
                    var response = client.Execute(request);
                    string message = response.Content;
                    dynamic irnresponseorder = JObject.Parse(message);
                    string mreturn = JValue.Parse(irnresponseorder.GetValue("status_cd").ToString());

                    if (mreturn != "0")
                    {
                        dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("data").ToString());
                        EwaybillNo = Model.EWB_EwayNo.Trim();
                        EwatValidUpto = irnresponseData.GetValue("validUpto").ToString();
                        errormsg += "Extend Eway Bill Sucessfully.\n Eway BillNo:" + EwaybillNo + " \n Valid Upto : " + EwatValidUpto + "";
                        Model.Document = lRMaster == null ? null : lRMaster.LrNo.ToString();
                        SaveEWB(Model, "LR000", EwaybillNo.Trim(), EwatValidUpto, "Exten", lRMaster, lRMaster == null ? "" : lRMaster.TableKey);
                        SaveEWBLog("LR000", lRMaster == null ? " " : lRMaster.LrNo.ToString(), "Extend Eway Bill", "Sucess", EwaybillNo.Trim());
                    }
                    else
                    {
                        Status = "Error";
                        dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("error").ToString());
                        dynamic ErrorMsh = JValue.Parse(irnresponseData.GetValue("message").ToString());
                        var ErrorCodeList = ErrorMsh.GetValue("errorCodes").ToString();
                        errormsg = GetErrorString(ErrorCodeList);
                        SaveEWBLog("LR000", lRMaster == null ? " " : lRMaster.LrNo.ToString(), "Extend Eway Bill", errormsg, Model.EWB_EwayNo.Trim());
                    }
                }

            }
            jsonResult = Json(new { Status = Status, errormsg = errormsg, EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        //Get Multi EwayBill List Whos Avalable To Extend
        public ActionResult GetGownEwayBillList()
        {
            var ToDate = (Convert.ToDateTime(DateTime.Now.ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            JsonResult jsonResult = new JsonResult();
            ExecuteStoredProc("Drop Table TempExtEwayBillList");
            string Query = " with  demo as " +
                " (select Convert(char(10),DocDate, 103) as DocDate, DocNo, EWBNO, LrTablekey from tfatewb  " +
                " where DATEADD(hour, -8, DATEADD(minute, 59, DATEADD(hour, 11, ewbvalid))) <= DATEADD(minute, DATEPART(minute, getdate()),DATEADD(hour, DATEPART(hour, getdate()),cast('" + ToDate + "' as datetime))) and DATEADD(minute, DATEPART(minute, getdate()),DATEADD(hour, DATEPART(hour, getdate()),cast('" + ToDate + "' as datetime))) <= DATEADD(hour, 8, DATEADD(minute, 59, DATEADD(hour, 11, ewbvalid))) " +
                " and EWBValid is not null and EWBType <> 'Authe' and LrTablekey is not null" +
                " ), newDemo as ( select D.*,LRSTK.StockAt,case when LRSTK.Type='LR' then LRSTK.Branch  else (select Y.Branch from Lrstock Y where Y.tablekey=LRSTK.Parentkey) end  as Branch,LRSTK.LrNo,LRSTK.TableKey,LRSTK.RECORDKEY,LRSTK.Type,case when LRSTK.Type='LR' then 'GODOWN' else 'TRANSIT' end as TypeIN    " +
                " from demo D join Lrstock LRSTK on LRSTK.LRRefTablekey = D.LrTablekey and LRSTK.Type <> 'DEL'" +
                " where(case when LRSTK.TotalQty = 0 then 0 else " +
                " (LRSTK.TotalQty - (((select  ISNULL(SUM(LRT.TotalQty), 0) from LRStock LRT where LRT.ParentKey = LRSTK.TableKey))   ) ) end ) <> 0 )" +
                " ,FinalData as (select*, ROW_NUMBER() OVER(Partition by TableKey ORDER BY RECORDKEY desc) AS Rn  from newDemo ) " +
                " select case when  FD.TypeIN='GODOWN' then T.Name else (CASE WHEN EXISTS (select V.TruckNo from VehicleMaster V where V.code= FD.StockAt) THEN(select V.TruckNo from VehicleMaster V where V.code = FD.StockAt) ELSE(select V.TruckNo from HireVehicleMaster V where V.code = FD.StockAt) END) end as StockIN,case when  FD.TypeIN='GODOWN' then '' else FD.StockAt end as VehicleCode  ,FD.*,T.Code As BranchCode,T.Name As BranchName,T.aPin,S.Code as StateCodeO,S.Name As StateName,S.StateCode,T.Addrl1,T.Addrl2,T.Addrl3+' '+ T.Addrl4 as Addrl3 Into TempExtEwayBillList from FinalData FD,TfatBranch T,tfatState S where   FD.rn = 1 and FD.Branch=T.Code  and S.Code=T.State ";//FD.Type='LR' and

            //Query = "  with  demo as  ( select Convert(char(10),DocDate, 103) as DocDate, DocNo, EWBNO, LrTablekey from tfatewb where EWBValid is not null and EWBType<> 'Authe' and LrTablekey is not null ), newDemo as ( select D.*,LRSTK.StockAt,case when LRSTK.Type='LR' then LRSTK.Branch  else (select Y.Branch from Lrstock Y where Y.tablekey=LRSTK.Parentkey) end  as Branch,LRSTK.LrNo,LRSTK.TableKey,LRSTK.RECORDKEY,LRSTK.Type,case when LRSTK.Type='LR' then 'GODOWN' else 'TRANSIT' end as TypeIN  from demo D join Lrstock LRSTK on LRSTK.LRRefTablekey = D.LrTablekey and LRSTK.Type <> 'DEL' where(case when LRSTK.TotalQty = 0 then 0 else  (LRSTK.TotalQty - (((select  ISNULL(SUM(LRT.TotalQty), 0) from LRStock LRT where LRT.ParentKey = LRSTK.TableKey))   ) ) end ) <> 0 ) ,FinalData as ( select*, ROW_NUMBER() OVER(Partition by TableKey ORDER BY RECORDKEY desc) AS Rn  from newDemo )  select case when  FD.TypeIN='GODOWN' then T.Name else (CASE WHEN EXISTS (select V.TruckNo from VehicleMaster V where V.code= FD.StockAt) THEN(select V.TruckNo from VehicleMaster V where V.code = FD.StockAt) ELSE(select V.TruckNo from HireVehicleMaster V where V.code = FD.StockAt) END) end as StockIN,case when  FD.TypeIN='GODOWN' then '' else FD.StockAt end as VehicleCode  ,FD.*,T.Code As BranchCode,T.Name As BranchName,T.aPin,S.Code as StateCodeO,S.Name As StateName,S.StateCode,T.Addrl1,T.Addrl2,T.Addrl3 + ' ' + T.Addrl4 as Addrl3 Into TempExtEwayBillList from FinalData FD,TfatBranch T, tfatState S where  FD.rn = 1 and FD.Branch = T.Code  and S.Code = T.State  ";//FD.Type = 'LR' and

            using (SqlConnection sqlConnection = new SqlConnection(GetConnectionString()))
            {
                SqlCommand sqlCommand = new SqlCommand(Query, sqlConnection);
                sqlCommand.CommandType = CommandType.Text;
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                DataTable datatable = new DataTable();
                sqlDataAdapter.Fill(datatable);
            }

            jsonResult = Json(new { Status = "Sucess" }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        //Extend EwayBill Through User Decide 
        public ActionResult UpdateExtendEwayBill(EwayBillVM Model)
        {
            List<EwayBillVM> FreshEwayBill = new List<EwayBillVM>();

            #region DataTable
            // Create a new DataTable.
            DataTable SucussExtendEwayBill = new DataTable("SucussExtendEwayBill");
            DataColumn dtColumn;
            DataRow myDataRow;
            // Create Name column.
            dtColumn = new DataColumn();
            dtColumn.DataType = typeof(String);
            dtColumn.ColumnName = "EWB NO";
            /// Add column to the DataColumnCollection.
            SucussExtendEwayBill.Columns.Add(dtColumn);
            // Create Name column.
            dtColumn = new DataColumn();
            dtColumn.DataType = typeof(String);
            dtColumn.ColumnName = "EWB Valid Upto";
            /// Add column to the DataColumnCollection.
            SucussExtendEwayBill.Columns.Add(dtColumn);

            // Create a new DataTable.
            DataTable ErrorExtendEwayBill = new DataTable("ErrorExtendEwayBill");
            // Create Name column.
            dtColumn = new DataColumn();
            dtColumn.DataType = typeof(String);
            dtColumn.ColumnName = "EWB NO";
            /// Add column to the DataColumnCollection.
            ErrorExtendEwayBill.Columns.Add(dtColumn);
            // Create Name column.
            dtColumn = new DataColumn();
            dtColumn.DataType = typeof(String);
            dtColumn.ColumnName = "Error Msg";
            /// Add column to the DataColumnCollection.
            ErrorExtendEwayBill.Columns.Add(dtColumn);
            #endregion

            if (Model.ConsoleList != null)
            {
                FreshEwayBill = Model.ConsoleList;
            }

            


            string Status = "Success", EwaybillNo = "", EwatValidUpto = "", errormsg = "", TfatCompErr = "";
            JsonResult jsonResult = new JsonResult();
            if (FreshEwayBill.Count() == 0)
            {
                jsonResult = Json(new { Status = "Error", errormsg = "Document Not Found To Extend ...!", EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
            TfatCompErr = SetApiParameters();
            if (!String.IsNullOrEmpty(TfatCompErr))
            {
                jsonResult = Json(new { Status = "Error", errormsg = TfatCompErr }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
            foreach (var item in FreshEwayBill)
            {
                StringBuilder jsonBuilder = new StringBuilder();
                string VehicleNo = "";
                HireVehicleMaster hireVehicle = ctxTFAT.HireVehicleMaster.Where(x => x.Code == item.EWB_VehicleNo).FirstOrDefault();
                VehicleMaster vehicle = ctxTFAT.VehicleMaster.Where(x => x.Code == item.EWB_VehicleNo).FirstOrDefault();

                if (hireVehicle != null)
                {
                    VehicleNo = Regex.Replace(hireVehicle.TruckNo, @"\s", "");
                    Model.EWB_LastVehicleNo = VehicleNo;
                }
                else if (vehicle != null)
                {
                    VehicleNo = Regex.Replace(vehicle.TruckNo, @"\s", "");
                    Model.EWB_LastVehicleNo = VehicleNo;
                }
                else
                {
                    VehicleNo = Regex.Replace(item.EWB_VehicleNo, @"\s", "");
                    Model.EWB_LastVehicleNo = VehicleNo;
                }
                TfatBranch tfatBranch= ctxTFAT.TfatBranch.Where(x => x.Code.ToString() == item.EWB_FromPlace.Trim()).FirstOrDefault();
                TfatState tfatState = ctxTFAT.TfatState.Where(x => x.Code.ToString() == item.EWB_ConsignorState.Trim()).FirstOrDefault();
                jsonBuilder.Append("{");
                jsonBuilder.Append("\"ewbNo\":" + item.EWB_EwayNo.Trim() + ",");
                jsonBuilder.Append("\"vehicleNo\":\"" + VehicleNo + "\",");
                jsonBuilder.Append("\"fromPlace\":\"" + tfatBranch.Name + "\",");
                jsonBuilder.Append("\"fromState\":" + tfatState.StateCode.Trim() + ",");
                jsonBuilder.Append("\"remainingDistance\":" + item.EWB_Distance + ",");
                jsonBuilder.Append("\"transDocNo\":\"" + item.Document + "\",");
                jsonBuilder.Append("\"transDocDate\":\"" + item.DocDate + "\",");
                jsonBuilder.Append("\"transMode\":\"" + 5 + "\",");
                jsonBuilder.Append("\"extnRsnCode\":" + item.EWB_ExtendReasonCode + ",");
                jsonBuilder.Append("\"extnRemarks\":\"" + item.EWB_ExtendReasonRemark + "\",");
                jsonBuilder.Append("\"fromPincode\":" + item.EWB_ConsignorPincode.Trim() + ",");
                jsonBuilder.Append("\"consignmentStatus\":\"" + "T" + "\",");
                jsonBuilder.Append("\"transitType\":\"" + "W" + "\",");
                jsonBuilder.Append("\"addressLine1\":\"" + item.EWB_ConsignorAddr1 + "\",");
                jsonBuilder.Append("\"addressLine2\":\"" + item.EWB_ConsignorAddr2 + "\",");
                jsonBuilder.Append("\"addressLine3\":\"" + item.EWB_ConsignorAddr3 + "\",");

                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                jsonBuilder.Append("},");
                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);

                JObject mjson = JObject.Parse(jsonBuilder.ToString());

                string mtoken = GetToken();
                if (mtoken == "0")
                {
                    jsonResult = Json(new { Status = "Error", errormsg = "Authentication Not Sucessed...!" }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                    return jsonResult;
                }
                else
                {
                    RestClient client = new RestClient(weburl + extendewayurl);
                    RestRequest request = new RestRequest(Method.POST);
                    request.AddParameter("email", memail, ParameterType.QueryString);
                    request.AddHeader("ip_address", mipaddress);
                    request.AddHeader("client_id", mclientid);
                    request.AddHeader("client_secret", mclientsecret);
                    request.AddHeader("gstin", mgstin);
                    request.AddHeader("cache-control", "no-cache");
                    request.AddHeader("content-type", "application/json");
                    request.AddParameter("application/json", mjson, ParameterType.RequestBody);
                    var response = client.Execute(request);
                    string message = response.Content;
                    dynamic irnresponseorder = JObject.Parse(message);
                    string mreturn = JValue.Parse(irnresponseorder.GetValue("status_cd").ToString());

                    if (mreturn != "0")
                    {

                        dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("data").ToString());
                        EwaybillNo = item.EWB_EwayNo.Trim();
                        EwatValidUpto = irnresponseData.GetValue("validUpto").ToString();
                        errormsg += "Extend Eway Bill Sucessfully.\n Eway BillNo:" + EwaybillNo + " \n Valid Upto : " + EwatValidUpto + "";
                        item.EWB_LastVehicleNo = Model.EWB_LastVehicleNo;

                        myDataRow = SucussExtendEwayBill.NewRow();
                        myDataRow["EWB NO"] = item.EWB_EwayNo.Trim();
                        myDataRow["EWB Valid Upto"] = EwatValidUpto;
                        SucussExtendEwayBill.Rows.Add(myDataRow);

                        SaveEWB(item, "LR000", EwaybillNo.Trim(), EwatValidUpto, "Exten", null, item.LRrefKey);
                        SaveEWBLog("LR000", item.Document.ToString(), "Extend Eway Bill", "Sucess", EwaybillNo.Trim());
                    }
                    else
                    {
                        Status = "Error";
                        dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("error").ToString());
                        dynamic ErrorMsh = JValue.Parse(irnresponseData.GetValue("message").ToString());
                        var ErrorCodeList = ErrorMsh.GetValue("errorCodes").ToString();
                        errormsg = GetErrorString(ErrorCodeList);
                        myDataRow = ErrorExtendEwayBill.NewRow();
                        myDataRow["EWB NO"] = item.EWB_EwayNo.Trim();
                        myDataRow["Error Msg"] = errormsg;
                        ErrorExtendEwayBill.Rows.Add(myDataRow);
                        SaveEWBLog("LR000", item.Document.ToString(), "Extend Eway Bill", errormsg, item.EWB_EwayNo.Trim());
                    }
                }

            }
            if (SucussExtendEwayBill.Rows.Count > 0 || ErrorExtendEwayBill.Rows.Count > 0)
            {
                SendAutoMail(SucussExtendEwayBill, ErrorExtendEwayBill);
            }

            jsonResult = Json(new { Status = Status, errormsg = "Extend Eway Bill Process Completed Please Check Your Mail.\n All Details Attached In The Mail." }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        //Auto Extend EwayBill(AutoMatically)
        public ActionResult AutoExtendEwayBillsList(EwayBillVM Model)
        {
            List<EwayBillVM> FreshEwayBill = new List<EwayBillVM>();

            // Create a new DataTable.
            DataTable SucussExtendEwayBill = new DataTable("SucussExtendEwayBill");
            DataColumn dtColumn;
            DataRow myDataRow;
            // Create Name column.
            dtColumn = new DataColumn();
            dtColumn.DataType = typeof(String);
            dtColumn.ColumnName = "EWB NO";
            /// Add column to the DataColumnCollection.
            SucussExtendEwayBill.Columns.Add(dtColumn);
            // Create Name column.
            dtColumn = new DataColumn();
            dtColumn.DataType = typeof(String);
            dtColumn.ColumnName = "EWB Valid Upto";
            /// Add column to the DataColumnCollection.
            SucussExtendEwayBill.Columns.Add(dtColumn);

            // Create a new DataTable.
            DataTable ErrorExtendEwayBill = new DataTable("ErrorExtendEwayBill");
            // Create Name column.
            dtColumn = new DataColumn();
            dtColumn.DataType = typeof(String);
            dtColumn.ColumnName = "EWB NO";
            /// Add column to the DataColumnCollection.
            ErrorExtendEwayBill.Columns.Add(dtColumn);
            // Create Name column.
            dtColumn = new DataColumn();
            dtColumn.DataType = typeof(String);
            dtColumn.ColumnName = "Error Msg";
            /// Add column to the DataColumnCollection.
            ErrorExtendEwayBill.Columns.Add(dtColumn);

            var ToDate = (Convert.ToDateTime(DateTime.Now.ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            //string Query = " with  demo as " +
            //    " (select Convert(char(10),DocDate, 103) as DocDate, DocNo, EWBNO, LrTablekey from tfatewb  " +
            //    " where DATEADD(hour, -8, DATEADD(minute, 59, DATEADD(hour, 11, ewbvalid))) <= DATEADD(minute, " + DateTime.Now.Minute + ",DATEADD(hour, " + DateTime.Now.Hour + ",cast('" + ToDate + "' as datetime))) and DATEADD(minute, " + DateTime.Now.Minute + ",DATEADD(hour, " + DateTime.Now.Hour + ",cast('" + ToDate + "' as datetime))) <= DATEADD(hour, 8, DATEADD(minute, 59, DATEADD(hour, 11, ewbvalid))) " +
            //    " and EWBValid is not null and EWBType <> 'Authe' and LrTablekey is not null" +
            //    " ), newDemo as ( select D.*,LRSTK.StockAt,LRSTK.Branch,LRSTK.LrNo,LRSTK.TableKey,LRSTK.RECORDKEY  " +
            //    " from demo D join Lrstock LRSTK on LRSTK.LRRefTablekey = D.LrTablekey and LRSTK.Type <> 'DEL'" +
            //    " where(case when LRSTK.TotalQty = 0 then 0 else " +
            //    " (LRSTK.TotalQty - (((select  ISNULL(SUM(LRT.TotalQty), 0) from LRStock LRT where LRT.ParentKey = LRSTK.TableKey))   ) ) end ) <> 0 )" +
            //    " ,FinalData as (select*, ROW_NUMBER() OVER(Partition by TableKey ORDER BY RECORDKEY desc) AS Rn  from newDemo ) " +
            //    " select* from FinalData where rn = 1 ";

            string Query = " with  demo as " +
                " (select Convert(char(10),DocDate, 103) as DocDate, DocNo, EWBNO, LrTablekey from tfatewb  " +
                " where DATEADD(hour, -8, DATEADD(minute, 59, DATEADD(hour, 11, ewbvalid))) <= DATEADD(minute, DATEPART(minute, getdate()),DATEADD(hour, DATEPART(hour, getdate()),cast('" + ToDate + "' as datetime))) and DATEADD(minute, DATEPART(minute, getdate()),DATEADD(hour, DATEPART(hour, getdate()),cast('" + ToDate + "' as datetime))) <= DATEADD(hour, 8, DATEADD(minute, 59, DATEADD(hour, 11, ewbvalid))) " +
                " and EWBValid is not null and EWBType <> 'Authe' and LrTablekey is not null" +
                " ), newDemo as ( select D.*,LRSTK.StockAt,LRSTK.Branch,LRSTK.LrNo,LRSTK.TableKey,LRSTK.RECORDKEY  " +
                " from demo D join Lrstock LRSTK on LRSTK.LRRefTablekey = D.LrTablekey and LRSTK.Type <> 'DEL'" +
                " where(case when LRSTK.TotalQty = 0 then 0 else " +
                " (LRSTK.TotalQty - (((select  ISNULL(SUM(LRT.TotalQty), 0) from LRStock LRT where LRT.ParentKey = LRSTK.TableKey))   ) ) end ) <> 0 )" +
                " ,FinalData as (select*, ROW_NUMBER() OVER(Partition by TableKey ORDER BY RECORDKEY desc) AS Rn  from newDemo ) " +
                " select* from FinalData where rn = 1 ";
            using (SqlConnection sqlConnection = new SqlConnection(GetConnectionString()))
            {
                SqlCommand sqlCommand = new SqlCommand(Query, sqlConnection);
                sqlCommand.CommandType = CommandType.Text;
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                DataTable datatable = new DataTable();
                sqlDataAdapter.Fill(datatable);

                foreach (DataRow item in datatable.Rows)
                {
                    var Branch = item["Branch"].ToString();
                    TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == Branch).FirstOrDefault();
                    TfatState tfatState = ctxTFAT.TfatState.Where(x => x.Code.ToString() == (tfatBranch == null ? "" : tfatBranch.State)).FirstOrDefault();
                    FreshEwayBill.Add(new EwayBillVM
                    {
                        EWB_EwayNo = item["EWBNO"].ToString(),
                        EWB_FromPlace = tfatBranch == null ? " " : tfatBranch.Name,
                        EWB_ConsignorState = tfatState == null ? "0" : tfatState.StateCode,
                        Document = item["LrNo"].ToString(),
                        EWBDocument = item["DocNo"].ToString(),
                        DocDate = item["DocDate"].ToString(),
                        EWB_ConsignorPincode = tfatBranch == null ? "0" : tfatBranch.aPin,
                        EWB_VehicleNo = item["StockAt"].ToString(),
                        LRrefKey = item["TableKey"].ToString(),
                        EWB_ConsignorAddr1 = tfatBranch == null ? "" : tfatBranch.Addrl1,
                        EWB_ConsignorAddr2 = tfatBranch == null ? "" : tfatBranch.Addrl2,
                        EWB_ConsignorAddr3 = tfatBranch == null ? "" : tfatBranch.Addrl3 + " " + tfatBranch.Addrl4,
                    });
                }
            }

            string Status = "Success", EwaybillNo = "", EwatValidUpto = "", errormsg = "", TfatCompErr = "";
            JsonResult jsonResult = new JsonResult();
            if (FreshEwayBill.Count() == 0)
            {
                jsonResult = Json(new { Status = "Error", errormsg = "Document Not Found To Extend ...!", EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
            TfatCompErr = SetApiParameters();
            if (!String.IsNullOrEmpty(TfatCompErr))
            {
                jsonResult = Json(new { Status = "Error", errormsg = TfatCompErr }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
            foreach (var item in FreshEwayBill)
            {
                StringBuilder jsonBuilder = new StringBuilder();
                string VehicleNo = "";
                HireVehicleMaster hireVehicle = ctxTFAT.HireVehicleMaster.Where(x => x.Code == item.EWB_VehicleNo).FirstOrDefault();
                VehicleMaster vehicle = ctxTFAT.VehicleMaster.Where(x => x.Code == item.EWB_VehicleNo).FirstOrDefault();

                if (hireVehicle != null)
                {
                    VehicleNo = Regex.Replace(hireVehicle.TruckNo, @"\s", "");
                    Model.EWB_LastVehicleNo = VehicleNo;
                }
                else if (vehicle != null)
                {
                    VehicleNo = Regex.Replace(vehicle.TruckNo, @"\s", "");
                    Model.EWB_LastVehicleNo = VehicleNo;
                }
                else
                {
                    Model.EWB_LastVehicleNo = "";
                }

                jsonBuilder.Append("{");
                jsonBuilder.Append("\"ewbNo\":" + item.EWB_EwayNo.Trim() + ",");
                jsonBuilder.Append("\"vehicleNo\":\"" + VehicleNo + "\",");
                jsonBuilder.Append("\"fromPlace\":\"" + item.EWB_FromPlace + "\",");
                jsonBuilder.Append("\"fromState\":" + item.EWB_ConsignorState.Trim() + ",");
                jsonBuilder.Append("\"remainingDistance\":" + 0 + ",");
                jsonBuilder.Append("\"transDocNo\":\"" + item.Document + "\",");
                jsonBuilder.Append("\"transDocDate\":\"" + item.DocDate + "\",");
                jsonBuilder.Append("\"transMode\":\"" + 5 + "\",");
                jsonBuilder.Append("\"extnRsnCode\":" + 5 + ",");
                jsonBuilder.Append("\"extnRemarks\":\"" + "Auto Extend Eway Bill" + "\",");
                jsonBuilder.Append("\"fromPincode\":" + item.EWB_ConsignorPincode.Trim() + ",");
                jsonBuilder.Append("\"consignmentStatus\":\"" + "T" + "\",");
                jsonBuilder.Append("\"transitType\":\"" + "W" + "\",");
                jsonBuilder.Append("\"addressLine1\":\"" + item.EWB_ConsignorAddr1 + "\",");
                jsonBuilder.Append("\"addressLine2\":\"" + item.EWB_ConsignorAddr2 + "\",");
                jsonBuilder.Append("\"addressLine3\":\"" + item.EWB_ConsignorAddr3 + "\",");

                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                jsonBuilder.Append("},");
                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);

                JObject mjson = JObject.Parse(jsonBuilder.ToString());

                string mtoken = GetToken();
                if (mtoken == "0")
                {
                    jsonResult = Json(new { Status = "Error", errormsg = "Authentication Not Sucessed...!" }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                    return jsonResult;
                }
                else
                {
                    RestClient client = new RestClient(weburl + extendewayurl);
                    RestRequest request = new RestRequest(Method.POST);
                    request.AddParameter("email", memail, ParameterType.QueryString);
                    request.AddHeader("ip_address", mipaddress);
                    request.AddHeader("client_id", mclientid);
                    request.AddHeader("client_secret", mclientsecret);
                    request.AddHeader("gstin", mgstin);
                    request.AddHeader("cache-control", "no-cache");
                    request.AddHeader("content-type", "application/json");
                    request.AddParameter("application/json", mjson, ParameterType.RequestBody);
                    var response = client.Execute(request);
                    string message = response.Content;
                    dynamic irnresponseorder = JObject.Parse(message);
                    string mreturn = JValue.Parse(irnresponseorder.GetValue("status_cd").ToString());

                    if (mreturn != "0")
                    {

                        dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("data").ToString());
                        EwaybillNo = item.EWB_EwayNo.Trim();
                        EwatValidUpto = irnresponseData.GetValue("validUpto").ToString();
                        errormsg += "Extend Eway Bill Sucessfully.\n Eway BillNo:" + EwaybillNo + " \n Valid Upto : " + EwatValidUpto + "";
                        item.EWB_LastVehicleNo = Model.EWB_LastVehicleNo;

                        myDataRow = SucussExtendEwayBill.NewRow();
                        myDataRow["EWB NO"] = item.EWB_EwayNo.Trim();
                        myDataRow["EWB Valid Upto"] = EwatValidUpto;
                        SucussExtendEwayBill.Rows.Add(myDataRow);

                        SaveEWB(item, "LR000", EwaybillNo.Trim(), EwatValidUpto, "Exten", null, item.LRrefKey);
                        SaveEWBLog("LR000", item.Document.ToString(), "Extend Eway Bill", "Sucess", EwaybillNo.Trim());
                    }
                    else
                    {
                        Status = "Error";
                        dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("error").ToString());
                        dynamic ErrorMsh = JValue.Parse(irnresponseData.GetValue("message").ToString());
                        var ErrorCodeList = ErrorMsh.GetValue("errorCodes").ToString();
                        errormsg = GetErrorString(ErrorCodeList);
                        myDataRow = ErrorExtendEwayBill.NewRow();
                        myDataRow["EWB NO"] = item.EWB_EwayNo.Trim();
                        myDataRow["Error Msg"] = errormsg;
                        ErrorExtendEwayBill.Rows.Add(myDataRow);
                        SaveEWBLog("LR000", item.Document.ToString(), "Extend Eway Bill", errormsg, item.EWB_EwayNo.Trim());
                    }
                }

            }
            if (SucussExtendEwayBill.Rows.Count > 0 || ErrorExtendEwayBill.Rows.Count > 0)
            {
                SendAutoMail(SucussExtendEwayBill, ErrorExtendEwayBill);
            }

            jsonResult = Json(new { Status = Status, errormsg = "Extend Eway Bill Process Completed Please Check Your Mail.\n All Details Attached In The Mail." }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;

        }

        //Auto Extend EwayBill Result Send To Email And Attach Excel File
        public void SendAutoMail(DataTable dt, DataTable dt2)
        {
            int mid = 0;
            try
            {
                string EmailmStr = "";
                EmailmStr = "<html>";
                EmailmStr += "<p><span style=\"font-family:Tahoma;font-size:16pt;\"><b>" + "Dear Sir/Madam,<br>Please Find Attached With This Mail." + "</b></span></p>";
                EmailmStr += "<br/>";
                EmailmStr += "</html>";
                EmailmStr += "\n\n\n\n:Auto Generated Report";
                string msmtppassword = "";
                string msmtphost = "";
                int msmtpport = 25;
                string msmtpuser = "";
                string mFromEmail = "";
                string mEmail = "".Trim();
                string mCC = "";
                string mBCC = "";
                string mSubject = "Auto Extend Eway Bill";
                string mMsg = EmailmStr;
                DataTable Compdt = new DataTable();
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string sql = string.Format(@"Select top 1 * from TfatComp");
                    SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                    da.Fill(Compdt);
                }
                //var lst = dt.AsEnumerable().ToList();  
                var lst = Compdt.AsEnumerable()
                             .Select(r => r.Table.Columns.Cast<DataColumn>()
                             .Select(c => new KeyValuePair<string, object>(c.ColumnName, r[c.Ordinal])
                          ).ToDictionary(z => z.Key, z => z.Value)
                       ).ToList();


                foreach (var item in lst)
                {
                    mCC = (mCC != "" ? mCC + "," : "");
                    mCC += item["CCTo"] == null ? "" : item["CCTo"].ToString().Trim();
                    mBCC = (mBCC != "" ? mBCC + "," : "");
                    mBCC += item["BCCTo"] == null ? "" : item["BCCTo"].ToString().Trim();
                    msmtpuser = item["SMTPUser"] == null ? "" : item["SMTPUser"].ToString().Trim();
                    msmtppassword = item["SMTPPassword"] == null ? "" : item["SMTPPassword"].ToString().Trim();
                    msmtphost = item["SMTPServer"] == null ? "" : item["SMTPServer"].ToString().Trim();
                    msmtpport = item["SMTPPort"] == null ? 25 : Convert.ToInt32(item["SMTPPort"]);
                    mFromEmail = (item["Email"] ?? "").ToString().Trim();
                }

                mCC = CutRightString(mCC, 1, ",");
                mBCC = CutRightString(mBCC, 1, ",");

                if (msmtpport != 587)
                {
                    msmtpport = 587;
                }

                MailMessage message = new MailMessage();
                if (dt.Rows.Count > 0)
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var worksheet = wb.Worksheets.Add(dt, dt.TableName);
                        worksheet.CellsUsed().Style.Border.BottomBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                        worksheet.CellsUsed().Style.Border.TopBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                        worksheet.CellsUsed().Style.Border.LeftBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                        worksheet.CellsUsed().Style.Border.RightBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                        worksheet.Tables.FirstOrDefault().ShowAutoFilter = false;

                        //worksheet.CellsUsed().Style.Border.TopBorderColor = ClosedXML.Excel.XLColor.Red;
                        using (MemoryStream stream = new MemoryStream())
                        {
                            wb.SaveAs(stream);
                            //Convert MemoryStream to Byte array.
                            byte[] bytes1 = stream.ToArray();
                            var TypeEx = "xlsx";
                            System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(new MemoryStream(bytes1), dt.TableName.ToUpper() + "." + TypeEx);
                            message.Attachments.Add(attachment);
                            //return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Grid.xlsx");
                        }
                    }
                }
                if (dt2.Rows.Count > 0)
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var worksheet = wb.Worksheets.Add(dt2, dt2.TableName);
                        worksheet.CellsUsed().Style.Border.BottomBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                        worksheet.CellsUsed().Style.Border.TopBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                        worksheet.CellsUsed().Style.Border.LeftBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                        worksheet.CellsUsed().Style.Border.RightBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                        //worksheet.CellsUsed().Style.Border.TopBorderColor = ClosedXML.Excel.XLColor.Red;
                        using (MemoryStream stream = new MemoryStream())
                        {
                            wb.SaveAs(stream);
                            //Convert MemoryStream to Byte array.
                            byte[] bytes1 = stream.ToArray();
                            var TypeEx = "xlsx";
                            System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(new MemoryStream(bytes1), dt2.TableName.ToUpper() + "." + TypeEx);
                            message.Attachments.Add(attachment);
                            //return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Grid.xlsx");
                        }
                    }
                }

                message.From = new MailAddress(mFromEmail);
                mEmail = CutRightString(mEmail, 1, ";");
                mEmail = CutRightString(mEmail, 1, ",");
                if (mEmail != "")
                {
                    string[] ccid = mEmail.Split(',');
                    foreach (var item in ccid)
                    {
                        message.To.Add(new MailAddress(item.Trim()));
                    }
                }
                if (mCC != "")
                {
                    string[] ccid = mCC.Split(',');
                    foreach (var item in ccid)
                    {
                        message.CC.Add(new MailAddress(item.Trim()));
                    }
                }
                if (mBCC != "")
                {
                    string[] bccid = mBCC.Split(',');
                    foreach (var item in bccid)
                    {
                        message.Bcc.Add(new MailAddress(item.Trim()));
                    }
                }
                message.Subject = mSubject;
                message.IsBodyHtml = true;
                message.BodyEncoding = System.Text.Encoding.UTF8;
                mMsg = mMsg.Replace("^~|", "<br>");
                //if (mMsg.Contains("<html>") == false)
                //{
                //    mMsg = TextToHtml(mMsg);
                //}
                message.Body = mMsg;
                message.Priority = MailPriority.High;
                message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = msmtphost;
                smtp.Port = msmtpport;
                smtp.Credentials = new System.Net.NetworkCredential(msmtpuser, msmtppassword);

                smtp.EnableSsl = true;

                smtp.Send(message);
                mid = SaveEmailLog(mEmail, mCC, mBCC, mSubject, mMsg, "", "", mSubject, "", "Auto Extend Ewaybill Send Mail", "", "", "");
            }
            catch (SmtpFailedRecipientException smtex)
            {
                ExecuteStoredProc("Update Emaillog Set sentStatus=0 where RecordKey=" + mid);
                //return Json(new { Status = "Error", Message = smtex.InnerException }, JsonRequestBehavior.AllowGet);
            }
        }



        #endregion

        #region Cancel EwayBill

        //Get Respenctive Data
        public ActionResult GetCancelEway(EwayBillVM Model)
        {
            string Errmsg = "", Status = "Success", TfatCompErr = "";
            TfatCompErr = SetApiParameters();
            if (String.IsNullOrEmpty(TfatCompErr))
            {
                LRMaster lRMaster = new LRMaster();
                if (String.IsNullOrEmpty(Model.Document))
                {
                    lRMaster = ctxTFAT.LRMaster.Where(x => x.EwayBill.ToString() == Model.SearchEway.Trim()).FirstOrDefault();
                }
                else
                {
                    lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == Model.Document.Trim()).FirstOrDefault();
                }

                if (lRMaster != null)
                {
                    if ((!String.IsNullOrEmpty(lRMaster.EwayBill)) && lRMaster.DecVal >= 50000)
                    {
                        tfatEwaySetup ewaySetup = ctxTFAT.tfatEwaySetup.FirstOrDefault();
                        if (ewaySetup != null)
                        {
                            Model.EWB_CancelReasonCode = ewaySetup.CanReason;
                        }
                        Model.EWB_EwayNo = lRMaster.EwayBill;
                    }
                    else
                    {
                        Status = "Error";
                        Errmsg = "This Consignment EwayBillno and Declare Value Does Not Match Required Criteria.\n Please Check It Consignment Details...!";
                    }
                }
                else
                {
                    Model.EWB_EwayNo = Model.SearchEway.Trim();
                }

                if (!String.IsNullOrEmpty(Model.Document))
                {
                    if (lRMaster == null)
                    {
                        Status = "Error";
                        Errmsg = "Consignment Not Found...!";
                    }
                }
            }
            else
            {
                Status = "Error";
                Errmsg = TfatCompErr;
            }

            var html = ViewHelper.RenderPartialView(this, "CancelEway", Model);
            var jsonResult = Json(new
            {
                Status = Status,
                Errmsg = Errmsg,
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        //Cancel Eway-Bill
        public ActionResult CancelEWayBill(EwayBillVM Model)
        {
            //Model.Type = "1";//Generate New EwayBill
            string Status = "Success", EwaybillNo = "", EwatValidUpto = "", errormsg = "", TfatCompErr = "";
            JsonResult jsonResult = new JsonResult();

            if (String.IsNullOrEmpty(Model.Document) && String.IsNullOrEmpty(Model.SearchEway))
            {
                jsonResult = Json(new { Status = "Error", errormsg = "Document Missing...!", EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
            TfatCompErr = SetApiParameters();
            if (!String.IsNullOrEmpty(TfatCompErr))
            {
                jsonResult = Json(new { Status = "Error", errormsg = TfatCompErr }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }

            LRMaster lRMaster = new LRMaster();
            if (String.IsNullOrEmpty(Model.Document))
            {
                lRMaster = ctxTFAT.LRMaster.Where(x => x.EwayBill.ToString() == Model.SearchEway.Trim()).FirstOrDefault();
            }
            else
            {
                lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == Model.Document.Trim()).FirstOrDefault();
            }
            if (!String.IsNullOrEmpty(Model.Document))
            {
                if (lRMaster == null)
                {
                    jsonResult = Json(new { Status = "Error", errormsg = "Consignment Not Found...!" }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                    return jsonResult;
                }
            }

            bool Expired = false;
            if (lRMaster != null)
            {
                tfatEWB tfatEWB = ctxTFAT.tfatEWB.Where(x => x.LrTablekey == lRMaster.TableKey.ToString().Trim() && x.DocType == "LR000").FirstOrDefault();
                if (tfatEWB != null)
                {
                    if (tfatEWB.EWBValid != null)
                    {
                        DateTime EwayValidDate = tfatEWB.EWBValid.Value;
                        DateTime CurrentDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                        if (!(EwayValidDate >= CurrentDate))
                        {
                            Expired = true;
                        }
                    }
                }
            }

            if (Expired)
            {
                jsonResult = Json(new { Status = "Error", errormsg = "This Consignment Expired...!", EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
            else
            {
                StringBuilder jsonBuilder = new StringBuilder();

                jsonBuilder.Append("{");
                jsonBuilder.Append("\"ewbNo\":" + Model.EWB_EwayNo.Trim() + ",");
                jsonBuilder.Append("\"cancelRsnCode\":" + Model.EWB_CancelReasonCode.Trim() + ",");
                jsonBuilder.Append("\"cancelRmrk\":\"" + Model.EWB_CancelReasonRemark + "\",");

                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                jsonBuilder.Append("},");
                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);

                JObject mjson = JObject.Parse(jsonBuilder.ToString());
                string mtoken = GetToken();
                if (mtoken == "0")
                {
                    jsonResult = Json(new { Status = "Error", errormsg = "Authentication Not Sucessed...!" }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                    return jsonResult;
                }
                else
                {
                    RestClient client = new RestClient(weburl + cancelewayurl);
                    RestRequest request = new RestRequest(Method.POST);
                    request.AddParameter("email", memail, ParameterType.QueryString);
                    request.AddHeader("ip_address", mipaddress);
                    request.AddHeader("client_id", mclientid);
                    request.AddHeader("client_secret", mclientsecret);
                    request.AddHeader("gstin", mgstin);
                    request.AddHeader("cache-control", "no-cache");
                    request.AddHeader("content-type", "application/json");
                    request.AddParameter("application/json", mjson, ParameterType.RequestBody);
                    var response = client.Execute(request);
                    string message = response.Content;
                    dynamic irnresponseorder = JObject.Parse(message);
                    string mreturn = JValue.Parse(irnresponseorder.GetValue("status_cd").ToString());

                    if (mreturn != "0")
                    {
                        dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("data").ToString());

                        EwaybillNo = Model.EWB_EwayNo.Trim();
                        errormsg += "Cancel Eway Bill Sucessfully.\n Eway BillNo:" + EwaybillNo;
                        Model.Document = lRMaster == null ? null : lRMaster.LrNo.ToString();
                        Model.EWBDocument = lRMaster == null ? "" : String.IsNullOrEmpty(lRMaster.PartyInvoice) == true ? lRMaster.BENumber : lRMaster.PartyInvoice.ToString();
                        SaveEWB(Model, "LR000", EwaybillNo.Trim(), "", "Cance", lRMaster, lRMaster == null ? "" : lRMaster.TableKey);
                        SaveEWBLog("LR000", lRMaster == null ? " " : lRMaster.LrNo.ToString(), "Cancel Eway Bill", "Sucess", EwaybillNo.Trim());
                        EwaybillNo = "";
                        if (lRMaster != null)
                        {
                            ExecuteStoredProc("Update LRMaster set EwayBill='" + EwaybillNo + "' where Tablekey='" + lRMaster.TableKey + "'");
                            ExecuteStoredProc("delete From tfatEWB  where LrTablekey='" + lRMaster.TableKey + "'");
                        }
                        else
                        {
                            ExecuteStoredProc("delete From tfatEWB  where EWBNO='" + EwaybillNo + "'");
                        }

                    }
                    else
                    {
                        Status = "Error";
                        dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("error").ToString());
                        dynamic ErrorMsh = JValue.Parse(irnresponseData.GetValue("message").ToString());
                        var ErrorCodeList = ErrorMsh.GetValue("errorCodes").ToString();
                        errormsg = GetErrorString(ErrorCodeList);
                        SaveEWBLog("LR000", lRMaster == null ? " " : lRMaster.LrNo.ToString(), "Cancel Eway Bill", errormsg, Model.EWB_EwayNo.Trim());
                    }
                }
            }

            jsonResult = Json(new { Status = Status, errormsg = errormsg, EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        #endregion

        #region Synchronise Eway Bill

        public ActionResult SyncEwayBill(EwayBillVM Model)
        {
            string Status = "Success", errormsg = "", TfatCompErr = "";
            JsonResult jsonResult = new JsonResult();

            TfatCompErr = SetApiParameters();
            if (!String.IsNullOrEmpty(TfatCompErr))
            {
                jsonResult = Json(new { Status = "Error", errormsg = TfatCompErr }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
            if (ConvertDDMMYYTOYYMMDD(Model.SyncDate) > ConvertDDMMYYTOYYMMDD(Model.SyncToDate))
            {
                jsonResult = Json(new { Status = "Error", errormsg = "Sync From Date Always Less Than Sync To Date...!" }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
            var StartDate = ConvertDDMMYYTOYYMMDD(Model.SyncDate);
            var EndDate = ConvertDDMMYYTOYYMMDD(Model.SyncToDate);
            bool Add = false;
            string mtoken = GetToken();
            foreach (DateTime day in EachDay(StartDate, EndDate))
            {
                Model.SyncDate = day.ToShortDateString();
                Add = true;
                if (mtoken != "0")
                {
                    RestClient client = new RestClient(weburl + getEWaybillDetailsurl);
                    RestRequest request = new RestRequest(Method.GET);
                    request.AddParameter("email", memail, ParameterType.QueryString);
                    request.AddParameter("date", Model.SyncDate, ParameterType.QueryString);
                    request.AddHeader("ip_address", mipaddress);
                    request.AddHeader("client_id", mclientid);
                    request.AddHeader("client_secret", mclientsecret);
                    request.AddHeader("gstin", mgstin);
                    request.AddHeader("cache-control", "no-cache");
                    request.AddHeader("content-type", "application/json");
                    var response = client.Execute(request);
                    string message = response.Content;
                    dynamic irnresponseorder = JObject.Parse(message);
                    string mreturn = JValue.Parse(irnresponseorder.GetValue("status_cd").ToString());
                    if (mreturn != "0")
                    {
                        var ExistWebNO = ctxTFAT.tfatEWB.Select(x => x.EWBNO).ToList();
                        dynamic stuff = JsonConvert.DeserializeObject(message);
                        var Data = stuff.data;
                        foreach (var item in Data)
                        {
                            Model.Document = null;
                            //only eway bill Not Found In DataBase
                            if (ExistWebNO.Where(x => x.ToString().Trim() == item.ewbNo.ToString().trim()) == null)
                            {
                                string TempEWB = item.ewbNo.ToString().trim();
                                string DocNO = item.docNo.ToString();
                                tfatEWB tfatEWB = new tfatEWB();
                                tfatEWB.Narr = "Eway Bill Not Found In Our Database At Synchronized Time.";
                                LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.EwayBill == TempEWB).FirstOrDefault();
                                if (lRMaster != null)
                                {
                                    Model.Document = lRMaster.LrNo.ToString();
                                    tfatEWB.Narr += "And Connect With Consignment Through Eway Bil NO...";
                                    tfatEWB.LREwaybill = lRMaster.EwayBill;
                                }
                                else
                                {
                                    lRMaster = ctxTFAT.LRMaster.Where(x => x.PartyInvoice == DocNO).FirstOrDefault();
                                    if (lRMaster != null)
                                    {
                                        Model.Document = lRMaster.LrNo.ToString();
                                        tfatEWB.Narr += "And Connect With Consignment Through PartyInvoice...";
                                        tfatEWB.PartyInvoice = lRMaster.PartyInvoice;
                                    }
                                    else
                                    {
                                        lRMaster = ctxTFAT.LRMaster.Where(x => x.BENumber == DocNO).FirstOrDefault();
                                        if (lRMaster != null)
                                        {
                                            Model.Document = lRMaster.LrNo.ToString();
                                            tfatEWB.Narr += "And Connect With Consignment Through BE-NUMBER...";
                                            tfatEWB.BENUMBER = lRMaster.BENumber;
                                        }
                                    }

                                }

                                tfatEWB.Branch = mbranchcode;
                                tfatEWB.DocType = "LR000";
                                tfatEWB.DocNo = item.docNo.ToString().Trim();
                                tfatEWB.Consignment = Model.Document;
                                tfatEWB.LrTablekey = lRMaster == null ? null : lRMaster.TableKey;
                                var NewDate = item.docDate.ToString();
                                tfatEWB.DocDate = ConvertDDMMYYTOYYMMDD(NewDate);
                                tfatEWB.DocTime = DateTime.Now.ToShortTimeString();
                                tfatEWB.Prefix = mperiod;
                                tfatEWB.EWBNO = item.ewbNo.ToString();
                                var NewValidDate = item.validUpto.ToString();
                                if (String.IsNullOrEmpty(NewValidDate))
                                {
                                    //var NewDate1 = item.ewbDate.ToString(); 
                                    tfatEWB.EWBValid = null;
                                }
                                else
                                {
                                    tfatEWB.EWBValid = ConvertDDMMYYTOYYMMDD(NewValidDate);
                                }
                                tfatEWB.EWBType = "GEN";
                                tfatEWB.VehicleNo = Model.EWB_LastVehicleNo;
                                tfatEWB.AUTHIDS = muserid;
                                tfatEWB.AUTHORISE = "A00";
                                tfatEWB.ENTEREDBY = muserid;
                                tfatEWB.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                                tfatEWB.SyncDate = ConvertDDMMYYTOYYMMDD(Model.SyncDate);
                                tfatEWB.GenMode = "Portal";
                                ctxTFAT.tfatEWB.Add(tfatEWB);
                            }
                            else
                            {
                                //check eway bill && DocNo Not Found In DataBase
                                string CheckEWBNO = item.ewbNo.ToString();
                                string DocNO = item.docNo.ToString();
                                if (ctxTFAT.tfatEWB.Where(x => x.EWBNO.Trim() == CheckEWBNO.Trim() && x.DocNo.Trim() == DocNO.Trim()).FirstOrDefault() == null)
                                {
                                    tfatEWB tfatEWB = new tfatEWB();
                                    tfatEWB.Narr = "Eway Bill And Doc NO Not Found In Our Database At Synchronized Time.";
                                    LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.EwayBill == CheckEWBNO).FirstOrDefault();
                                    if (lRMaster != null)
                                    {
                                        Model.Document = lRMaster.LrNo.ToString();
                                        tfatEWB.Narr += "And Connect With Consignment Through Eway Bil NO...";
                                        tfatEWB.LREwaybill = lRMaster.EwayBill;
                                    }
                                    else
                                    {
                                        lRMaster = ctxTFAT.LRMaster.Where(x => x.PartyInvoice == DocNO).FirstOrDefault();
                                        if (lRMaster != null)
                                        {
                                            Model.Document = lRMaster.LrNo.ToString();
                                            tfatEWB.Narr += "And Connect With Consignment Through PartyInvoice...";
                                            tfatEWB.PartyInvoice = lRMaster.PartyInvoice;

                                        }
                                        else
                                        {
                                            lRMaster = ctxTFAT.LRMaster.Where(x => x.BENumber == DocNO).FirstOrDefault();
                                            if (lRMaster != null)
                                            {
                                                Model.Document = lRMaster.LrNo.ToString();
                                                tfatEWB.Narr += "And Connect With Consignment Through BE-NUMBER...";
                                                tfatEWB.BENUMBER = lRMaster.BENumber;

                                            }
                                        }

                                    }

                                    tfatEWB.Branch = mbranchcode;
                                    tfatEWB.DocType = "LR000";
                                    tfatEWB.DocNo = item.docNo.ToString().Trim();
                                    tfatEWB.Consignment = Model.Document;
                                    tfatEWB.LrTablekey = lRMaster == null ? null : lRMaster.TableKey;
                                    var NewDate = item.docDate.ToString();
                                    tfatEWB.DocDate = ConvertDDMMYYTOYYMMDD(NewDate);
                                    tfatEWB.DocTime = DateTime.Now.ToShortTimeString();
                                    tfatEWB.Prefix = mperiod;
                                    tfatEWB.EWBNO = item.ewbNo.ToString();
                                    var NewValidDate = item.validUpto.ToString();
                                    if (String.IsNullOrEmpty(NewValidDate))
                                    {
                                        //var NewDate1 = item.ewbDate.ToString();
                                        tfatEWB.EWBValid = null;
                                    }
                                    else
                                    {
                                        tfatEWB.EWBValid = ConvertDDMMYYTOYYMMDD(NewValidDate);
                                    }
                                    tfatEWB.EWBType = "GEN";
                                    tfatEWB.VehicleNo = Model.EWB_LastVehicleNo;
                                    tfatEWB.AUTHIDS = muserid;
                                    tfatEWB.AUTHORISE = "A00";
                                    tfatEWB.ENTEREDBY = muserid;
                                    tfatEWB.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                                    tfatEWB.SyncDate = ConvertDDMMYYTOYYMMDD(Model.SyncDate);
                                    tfatEWB.GenMode = "Portal";
                                    ctxTFAT.tfatEWB.Add(tfatEWB);
                                }
                                else
                                {
                                    tfatEWB tfatEWB = ctxTFAT.tfatEWB.Where(x => x.EWBNO.Trim() == CheckEWBNO.Trim() && x.DocNo.Trim() == DocNO.Trim()).FirstOrDefault();
                                    if (String.IsNullOrEmpty(tfatEWB.LrTablekey))
                                    {
                                        tfatEWB.Narr = "Eway Bill Found In Our Database...";
                                        LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.EwayBill == CheckEWBNO).FirstOrDefault();
                                        if (lRMaster != null)
                                        {
                                            Model.Document = lRMaster.LrNo.ToString();
                                            tfatEWB.Narr += "And Connect With Consignment Through Eway Bil NO...";
                                            tfatEWB.LREwaybill = lRMaster.EwayBill;
                                        }
                                        else
                                        {
                                            lRMaster = ctxTFAT.LRMaster.Where(x => x.PartyInvoice == DocNO).FirstOrDefault();
                                            if (lRMaster != null)
                                            {
                                                Model.Document = lRMaster.LrNo.ToString();
                                                tfatEWB.Narr += "Connect With Consignment Through Party Invoice...";
                                                tfatEWB.PartyInvoice = lRMaster.PartyInvoice;
                                            }
                                            else
                                            {
                                                lRMaster = ctxTFAT.LRMaster.Where(x => x.BENumber == DocNO).FirstOrDefault();
                                                if (lRMaster != null)
                                                {
                                                    Model.Document = lRMaster.LrNo.ToString();
                                                    tfatEWB.Narr += "And Connect With Consignment Through BE-NUMBER...";
                                                    tfatEWB.BENUMBER = lRMaster.BENumber;

                                                }
                                            }

                                        }

                                        tfatEWB.Consignment = Model.Document;
                                        tfatEWB.LrTablekey = lRMaster == null ? null : lRMaster.TableKey;
                                        var NewValidDate = item.validUpto.ToString();
                                        if (String.IsNullOrEmpty(NewValidDate))
                                        {
                                            //var NewDate1 = item.ewbDate.ToString();
                                            tfatEWB.EWBValid = null;
                                        }
                                        else
                                        {
                                            tfatEWB.EWBValid = ConvertDDMMYYTOYYMMDD(NewValidDate);
                                        }

                                        ctxTFAT.Entry(tfatEWB).State = EntityState.Modified;
                                    }


                                }
                            }
                        }

                        ctxTFAT.SaveChanges();


                        errormsg += Model.SyncDate + " : Sucessfully Update...!\n";
                        //SaveEWBLog("LR000", Model.Document, "Cancel Eway Bill", "Sucess", lRMaster.EwayBill.Trim());
                        //ExecuteStoredProc("Update LRMaster set EwayBill='" + EwaybillNo + "' where LRno=" + Model.Document + "");
                    }
                    else
                    {
                        Status = "Error";
                        dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("error").ToString());
                        dynamic ErrorMsh = JValue.Parse(irnresponseData.GetValue("message").ToString());
                        var ErrorCodeList = ErrorMsh.GetValue("errorCodes").ToString();
                        errormsg += Model.SyncDate + " : " + GetErrorString(ErrorCodeList);
                        //SaveEWBLog("LR000", Model.Document, "Cancel Eway Bill", errormsg, lRMaster.EwayBill.Trim());
                    }
                }
                else
                {
                    jsonResult = Json(new { Status = "Error", errormsg = "Authentication Not Sucessed...!" }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                    return jsonResult;
                }
            }

            if (Add)
            {
                TfatSyncHistory tfatSyncHistory = new TfatSyncHistory();
                tfatSyncHistory.CreateDate = DateTime.Now;
                tfatSyncHistory.FromDate = ConvertDDMMYYTOYYMMDD(StartDate.ToShortDateString());
                tfatSyncHistory.ToDate = ConvertDDMMYYTOYYMMDD(EndDate.ToShortDateString());
                tfatSyncHistory.ENTEREDBY = muserid;
                tfatSyncHistory.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                tfatSyncHistory.AUTHORISE = mauthorise;
                tfatSyncHistory.AUTHIDS = muserid;
                ctxTFAT.TfatSyncHistory.Add(tfatSyncHistory);
                ctxTFAT.SaveChanges();
            }



            jsonResult = Json(new { Status = Status, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;

        }

        public ActionResult SyncConsignerEwayBill(EwayBillVM Model)
        {
            string Status = "Success", errormsg = "", TfatCompErr = "";
            JsonResult jsonResult = new JsonResult();

            TfatCompErr = SetApiParameters();
            if (!String.IsNullOrEmpty(TfatCompErr))
            {
                jsonResult = Json(new { Status = "Error", errormsg = TfatCompErr }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }

            string mtoken = GetToken();
            if (mtoken != "0")
            {
                RestClient client = new RestClient(weburl + getEWaybillDetailConsignersurl);
                RestRequest request = new RestRequest(Method.GET);
                request.AddParameter("email", memail, ParameterType.QueryString);
                request.AddParameter("Gen_gstin", Model.EWB_ConsignorGST, ParameterType.QueryString);
                request.AddParameter("date", Model.SyncDate, ParameterType.QueryString);
                request.AddHeader("ip_address", mipaddress);
                request.AddHeader("client_id", mclientid);
                request.AddHeader("client_secret", mclientsecret);
                request.AddHeader("gstin", mgstin);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/json");
                var response = client.Execute(request);
                string message = response.Content;
                dynamic irnresponseorder = JObject.Parse(message);
                string mreturn = JValue.Parse(irnresponseorder.GetValue("status_cd").ToString());
                if (mreturn != "0")
                {
                    var ExistWebNO = ctxTFAT.tfatEWB.Select(x => x.EWBNO).ToList();
                    dynamic stuff = JsonConvert.DeserializeObject(message);
                    var Data = stuff.data;
                    foreach (var item in Data)
                    {
                        //only eway bill Not Found In DataBase
                        if (!ExistWebNO.Contains(item.ewbNo.ToString()))
                        {
                            string TempEWB = item.ewbNo.ToString();
                            string DocNO = item.docNo.ToString();
                            tfatEWB tfatEWB = new tfatEWB();
                            LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.EwayBill == TempEWB).FirstOrDefault();
                            if (lRMaster != null)
                            {
                                Model.Document = lRMaster.LrNo.ToString();
                            }
                            else
                            {
                                lRMaster = ctxTFAT.LRMaster.Where(x => x.PartyInvoice == DocNO).FirstOrDefault();
                                if (lRMaster != null)
                                {
                                    Model.Document = lRMaster.LrNo.ToString();
                                }
                                else
                                {
                                    lRMaster = ctxTFAT.LRMaster.Where(x => x.BENumber == DocNO).FirstOrDefault();
                                    if (lRMaster != null)
                                    {
                                        Model.Document = lRMaster.LrNo.ToString();
                                    }
                                }

                            }

                            tfatEWB.Branch = mbranchcode;
                            tfatEWB.DocType = "LR000";
                            tfatEWB.DocNo = item.docNo.ToString().Trim();
                            tfatEWB.Consignment = Model.Document;
                            tfatEWB.LrTablekey = lRMaster == null ? null : lRMaster.TableKey;
                            var NewDate = item.docDate.ToString();
                            tfatEWB.DocDate = ConvertDDMMYYTOYYMMDD(NewDate);
                            tfatEWB.DocTime = DateTime.Now.ToShortTimeString();
                            tfatEWB.Prefix = mperiod;
                            tfatEWB.EWBNO = item.ewbNo.ToString();
                            var NewValidDate = item.validUpto.ToString();
                            if (String.IsNullOrEmpty(NewValidDate))
                            {
                                tfatEWB.EWBValid = null;
                            }
                            else
                            {
                                tfatEWB.EWBValid = ConvertDDMMYYTOYYMMDD(NewValidDate);
                            }
                            tfatEWB.EWBType = "GEN";
                            tfatEWB.VehicleNo = Model.EWB_LastVehicleNo;
                            tfatEWB.AUTHIDS = muserid;
                            tfatEWB.AUTHORISE = "A00";
                            tfatEWB.ENTEREDBY = muserid;
                            tfatEWB.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                            tfatEWB.Narr = "Eway Bill Not Found In Our Database At Synchronized Time.";
                            tfatEWB.SyncDate = ConvertDDMMYYTOYYMMDD(Model.SyncDate);
                            ctxTFAT.tfatEWB.Add(tfatEWB);
                        }
                        else
                        {
                            //check eway bill && DocNo Not Found In DataBase
                            string CheckEWBNO = item.ewbNo.ToString();
                            string DocNO = item.docNo.ToString();
                            if (ctxTFAT.tfatEWB.Where(x => x.EWBNO.Trim() == CheckEWBNO.Trim() && x.DocNo.Trim() == DocNO.Trim()).FirstOrDefault() == null)
                            {
                                tfatEWB tfatEWB = new tfatEWB();
                                LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.EwayBill == CheckEWBNO).FirstOrDefault();
                                if (lRMaster != null)
                                {
                                    Model.Document = lRMaster.LrNo.ToString();
                                }
                                else
                                {
                                    lRMaster = ctxTFAT.LRMaster.Where(x => x.PartyInvoice == DocNO).FirstOrDefault();
                                    if (lRMaster != null)
                                    {
                                        Model.Document = lRMaster.LrNo.ToString();
                                    }
                                    else
                                    {
                                        lRMaster = ctxTFAT.LRMaster.Where(x => x.BENumber == DocNO).FirstOrDefault();
                                        if (lRMaster != null)
                                        {
                                            Model.Document = lRMaster.LrNo.ToString();
                                        }
                                    }

                                }

                                tfatEWB.Branch = mbranchcode;
                                tfatEWB.DocType = "LR000";
                                tfatEWB.DocNo = item.docNo.ToString().Trim();
                                tfatEWB.Consignment = Model.Document;
                                tfatEWB.LrTablekey = lRMaster == null ? null : lRMaster.TableKey;
                                var NewDate = item.docDate.ToString();
                                tfatEWB.DocDate = ConvertDDMMYYTOYYMMDD(NewDate);
                                tfatEWB.DocTime = DateTime.Now.ToShortTimeString();
                                tfatEWB.Prefix = mperiod;
                                tfatEWB.EWBNO = item.ewbNo.ToString();
                                var NewValidDate = item.validUpto.ToString();
                                if (String.IsNullOrEmpty(NewValidDate))
                                {
                                    tfatEWB.EWBValid = null;
                                }
                                else
                                {
                                    tfatEWB.EWBValid = ConvertDDMMYYTOYYMMDD(NewValidDate);
                                }
                                tfatEWB.EWBType = "GEN";
                                tfatEWB.VehicleNo = Model.EWB_LastVehicleNo;
                                tfatEWB.AUTHIDS = muserid;
                                tfatEWB.AUTHORISE = "A00";
                                tfatEWB.ENTEREDBY = muserid;
                                tfatEWB.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                                tfatEWB.Narr = "Eway Bill And Doc NO Not Found In Our Database At Synchronized Time.";
                                tfatEWB.SyncDate = ConvertDDMMYYTOYYMMDD(Model.SyncDate);
                                ctxTFAT.tfatEWB.Add(tfatEWB);
                            }
                            else
                            {
                                tfatEWB tfatEWB = ctxTFAT.tfatEWB.Where(x => x.EWBNO.Trim() == CheckEWBNO.Trim() && x.DocNo.Trim() == DocNO.Trim()).FirstOrDefault();
                                if (String.IsNullOrEmpty(tfatEWB.LrTablekey))
                                {
                                    LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.EwayBill == CheckEWBNO).FirstOrDefault();
                                    if (lRMaster != null)
                                    {
                                        Model.Document = lRMaster.LrNo.ToString();
                                    }
                                    else
                                    {
                                        lRMaster = ctxTFAT.LRMaster.Where(x => x.PartyInvoice == DocNO).FirstOrDefault();
                                        if (lRMaster != null)
                                        {
                                            Model.Document = lRMaster.LrNo.ToString();
                                        }
                                        else
                                        {
                                            lRMaster = ctxTFAT.LRMaster.Where(x => x.BENumber == DocNO).FirstOrDefault();
                                            if (lRMaster != null)
                                            {
                                                Model.Document = lRMaster.LrNo.ToString();
                                            }
                                        }

                                    }

                                    tfatEWB.Consignment = Model.Document;
                                    tfatEWB.LrTablekey = lRMaster == null ? null : lRMaster.TableKey;
                                    var NewValidDate = item.validUpto.ToString();
                                    if (String.IsNullOrEmpty(NewValidDate))
                                    {
                                        tfatEWB.EWBValid = null;
                                    }
                                    else
                                    {
                                        tfatEWB.EWBValid = ConvertDDMMYYTOYYMMDD(NewValidDate);
                                    }
                                    tfatEWB.Narr = "Eway Bill Found In Our Database And Connect With Consignment.";
                                    ctxTFAT.Entry(tfatEWB).State = EntityState.Modified;
                                }


                            }
                        }
                    }

                    ctxTFAT.SaveChanges();
                    errormsg += "Sucessfully Update...!";
                    //SaveEWBLog("LR000", Model.Document, "Cancel Eway Bill", "Sucess", lRMaster.EwayBill.Trim());
                    //ExecuteStoredProc("Update LRMaster set EwayBill='" + EwaybillNo + "' where LRno=" + Model.Document + "");
                }
                else
                {
                    Status = "Error";
                    dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("error").ToString());
                    dynamic ErrorMsh = JValue.Parse(irnresponseData.GetValue("message").ToString());
                    var ErrorCodeList = ErrorMsh.GetValue("errorCodes").ToString();
                    errormsg = GetErrorString(ErrorCodeList);
                    //SaveEWBLog("LR000", Model.Document, "Cancel Eway Bill", errormsg, lRMaster.EwayBill.Trim());
                }
            }
            else
            {
                jsonResult = Json(new { Status = "Error", errormsg = "Authentication Not Sucessed...!" }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;

            }




            jsonResult = Json(new { Status = Status, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;

        }

        public IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }

        #endregion

        public ActionResult GetGSTDetails(ConsignerMasterVM Model)
        {
            string Status = "Error", errormsg = "";
            errormsg = SetApiParameters();
            if (String.IsNullOrEmpty(errormsg))
            {
                string mtoken = GetToken();
                if (mtoken != "0")
                {
                    var URL = new UriBuilder(weburl + getGSTDetailsurl);
                    weburl = "https://api.mastergst.com/ewaybillapi/v1.03/ewayapi/getgstindetails";
                    URL = new UriBuilder(weburl);
                    var queryString = HttpUtility.ParseQueryString(string.Empty);
                    queryString["email"] = memail;
                    queryString["GSTIN"] = Model.GSTNo.ToString().Trim();
                    URL.Query = queryString.ToString();
                    var client = new WebClient();
                    client.Headers.Add("ip_address", mipaddress);
                    client.Headers.Add("client_id", mclientid);
                    client.Headers.Add("client_secret", mclientsecret);
                    client.Headers.Add("gstin", mgstin);
                    string mstr = client.DownloadString(URL.ToString());
                    dynamic mjson = Newtonsoft.Json.Linq.JValue.Parse(Newtonsoft.Json.JsonConvert.DeserializeObject(mstr).ToString());
                    string mreturn = mjson.GetValue("status_cd").ToString();
                    if (mreturn != "0")
                    {
                        Status = "Sucess";
                        dynamic irnresponseData = JValue.Parse(mjson.GetValue("data").ToString());
                        Model.Name = irnresponseData.GetValue("tradeName").ToString();
                        Model.Address1 = irnresponseData.GetValue("address1").ToString();
                        Model.Address2 = irnresponseData.GetValue("address2").ToString();
                        Model.State = irnresponseData.GetValue("stateCode").ToString();
                        TfatState tfatState = ctxTFAT.TfatState.Where(x => x.StateCode == Model.State).FirstOrDefault();
                        if (tfatState != null)
                        {
                            Model.State = tfatState.Code.ToString();
                            Model.StateName = tfatState.Name;
                        }
                        Model.Pin = irnresponseData.GetValue("pinCode").ToString();
                    }
                    else
                    {
                        Status = "Error";
                        dynamic irnresponseData = JValue.Parse(mjson.GetValue("error").ToString());
                        dynamic ErrorMsh = JValue.Parse(irnresponseData.GetValue("message").ToString());
                        var ErrorCodeList = ErrorMsh.GetValue("errorCodes").ToString();
                        errormsg = GetErrorString(ErrorCodeList);
                    }
                }
            }

            return Json(new { Status = Status, errormsg = errormsg, Model = Model, id = "StateMaster" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetEayBillToConsignmnet(EwayBillVM mModel)
        {
            string Status = "Sucess", errormsg = "";
            if (mModel.ExtendEwayBillList != null)
            {
                foreach (var item in mModel.ExtendEwayBillList)
                {
                    LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey == item.Consignment).FirstOrDefault();
                    if (lRMaster == null)
                    {

                    }
                }
            }


            return Json(new { Status = Status, errormsg = errormsg, id = "StateMaster" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FetchEwayBillList(EwayBillVM mModel)
        {
            mModel.SyncDate = DateTime.Now.ToShortDateString();
            var html = ViewHelper.RenderPartialView(this, "EwayBillList", mModel);
            var jsonResult = Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public ActionResult FetchMisEwayBillList(EwayBillVM mModel)
        {
            mModel.SyncDate = DateTime.Now.ToShortDateString();
            var html = ViewHelper.RenderPartialView(this, "MisMatchEway", mModel);
            var jsonResult = Json(new
            {
                Html = html
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public ActionResult UnClearEwayBill(EwayBillVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    if (mModel.ExtendEwayBillList != null)
                    {
                        foreach (var item in mModel.ExtendEwayBillList)
                        {
                            tfatEWB tfatEWB = ctxTFAT.tfatEWB.Where(x => x.EWBNO.ToString().Trim() == item.EWBNO.ToString().Trim()).FirstOrDefault();
                            if (tfatEWB == null)
                            {
                                return Json(new { Message = "Eway Bill Not Found...!", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                tfatEWB.Clear = false;
                                ctxTFAT.Entry(tfatEWB).State = EntityState.Modified;
                            }
                        }
                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();

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

        public ActionResult ClearEwayBill(EwayBillVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    if (mModel.ExtendEwayBillList != null)
                    {
                        foreach (var item in mModel.ExtendEwayBillList)
                        {
                            tfatEWB tfatEWB = ctxTFAT.tfatEWB.Where(x => x.EWBNO.ToString().Trim() == item.EWBNO.ToString().Trim()).FirstOrDefault();
                            if (tfatEWB == null)
                            {
                                return Json(new { Message = "Eway Bill Not Found...!", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                tfatEWB.Clear = true;
                                ctxTFAT.Entry(tfatEWB).State = EntityState.Modified;
                            }
                        }
                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
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
    }

}