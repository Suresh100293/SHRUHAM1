using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using Common;
using CrystalDecisions.Shared.Json;
using EntitiModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class EwayBillController : BaseController
    {
        private string mdocument = "";
        private static string mauthorise = "A00";

        private static string memail = "Sureshpandule7@gmail.com";
        private static string musername = "05AAACH6188F1ZM";
        private static string mpassword = "abc123@@";

        private static string weburl = "https://api.mastergst.com/";
        private static string authenticateurl = "ewaybillapi/v1.03/authenticate";
        private static string genewaybillurl = "ewaybillapi/v1.03/ewayapi/genewaybill";
        private static string initmultiurl = "ewaybillapi/v1.03/ewayapi/initmulti";
        private static string addmultiurl = "ewaybillapi/v1.03/ewayapi/addmulti";
        private static string getewaybillurl = "ewaybillapi/v1.03/ewayapi/getewaybill";

        #region EwayBill Functions

        public ActionResult GetSuppltType(LRVM Model)
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

        public JsonResult GetVehicle(string term, bool Hire)
        {
            if (Hire == false)
            {
                var list = ctxTFAT.VehicleMaster.Where(x => x.Acitve == true).Select(x => new { x.TruckNo, x.Code }).ToList();
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
            else
            {
                var list = ctxTFAT.HireVehicleMaster.Where(x => x.Acitve == true).Select(x => new { x.TruckNo, x.Code }).ToList();
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

        #endregion

        // GET: Logistics/EwayBill
        #region Grid 

        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, "", "", "A");
            mdocument = Model.Document;
            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            ViewBag.ViewDataId = Model.ViewDataId;
            ViewBag.Header = Model.Header;
            ViewBag.Controller = Model.Controller;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;
            //mpara = (string.IsNullOrEmpty(Model.Document) == true) ? "" : "para01^" + Model.Document.Substring(6, Model.Document.Length - 6);
            return View(Model);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            return GetGridReport(Model, "R", "", false, 0);
        }

        #endregion

        public ActionResult EwayBillReParam(EwayBillVM Model)
        {

            LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == Model.Document.Trim()).FirstOrDefault();
            if (lRMaster!=null)
            {
                if (!String.IsNullOrEmpty(lRMaster.DescrType))
                {
                    Model.EWB_productName = ctxTFAT.DescriptionMaster.Where(x => x.Code == lRMaster.DescrType).Select(x => x.Description).FirstOrDefault();
                }
                Model.EWB_TRNMode = lRMaster.TrnMode;
                Model.EWB_hsnCode = lRMaster.HSNCODE;

                Consigner consigner = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.RecCode).FirstOrDefault();
                Consigner consignee = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.SendCode).FirstOrDefault();

                if (String.IsNullOrEmpty(lRMaster.RecGST))
                {
                    Model.EWB_ConsignorGST = consigner.GST;
                }
                else
                {
                    Model.EWB_ConsignorGST = lRMaster.RecGST.Trim();
                }
                if (String.IsNullOrEmpty(lRMaster.SendGST))
                {
                    Model.EWB_ConsigneeGST = consignee.GST;
                }
                else
                {
                    Model.EWB_ConsigneeGST = lRMaster.SendGST.Trim();
                }
                Model.EWB_ConsignorPincode = consigner.Pincode.Value.ToString();
                Model.EWB_ConsigneePincode = consignee.Pincode.Value.ToString();

                if (!String.IsNullOrEmpty(consigner.State))
                {
                    Model.EWB_ConsignorState = consigner.State;
                    Model.EWB_ConsignorStateName =ctxTFAT.TfatState.Where(x=>x.Code.ToString()==consigner.State).Select(x=>x.Name).FirstOrDefault();
                }
                if (!String.IsNullOrEmpty(consignee.State))
                {
                    Model.EWB_ConsigneeState = consignee.State;
                    Model.EWB_ConsigneeStateName = ctxTFAT.TfatState.Where(x=>x.Code.ToString()== consignee.State).Select(x=>x.Name).FirstOrDefault();
                }

            }


            var html = ViewHelper.RenderPartialView(this, "ShowEayBillReqPara", Model);
            var jsonResult = Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }
        public ActionResult GenerateEWayBill(EwayBillVM Model)
        {
            //Model.Type = "1";//Generate New EwayBill
            string Status = "Success", EwaybillNo = "", EwatValidUpto = "", errormsg = "";

            JsonResult jsonResult = new JsonResult();

            if (Model.Document == null || Model.Document == "")
            {
                jsonResult = Json(new { Status = "Error", errormsg = "Document Missing...!", EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }
            LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == Model.Document.Trim()).FirstOrDefault();
            if (lRMaster!=null)
            {
                Consigner consigner = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.RecCode).FirstOrDefault();
                if (consigner!=null)
                {
                    if (String.IsNullOrEmpty(consigner.GST))
                    {
                        errormsg = "Consignor Gst Missing\n";
                    }
                    if (consigner.Pincode==0)
                    {
                        errormsg = "Consignor PinCode Missing\n";
                    }
                    if (String.IsNullOrEmpty(consigner.State))
                    {
                        errormsg = "Consignor State Missing\n";
                    }
                }
                Consigner consignee = ctxTFAT.Consigner.Where(x => x.Code == lRMaster.SendCode).FirstOrDefault();
                if (consignee != null)
                {
                    if (String.IsNullOrEmpty(consignee.GST))
                    {
                        errormsg = "Consignee Gst Missing\n";
                    }
                    if (consignee.Pincode == 0)
                    {
                        errormsg = "Consignee PinCode Missing\n";
                    }
                    if (String.IsNullOrEmpty(consignee.State))
                    {
                        errormsg = "Consignee State Missing\n";
                    }
                }
                if (!String.IsNullOrEmpty(errormsg))
                {
                    jsonResult = Json(new { Status = "Error", errormsg = errormsg, EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                    return jsonResult;
                }
                StringBuilder stringBuilder = GetJSON(Model, lRMaster, consigner, consignee);
                var GetFinalJson = HitApi(Model,stringBuilder);
                return GetFinalJson;
            }
            else
            {
                jsonResult = Json(new { Status = "Error", errormsg = "Document Missing...!", EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;

            }

        }


        public StringBuilder GetJSON(EwayBillVM Model, LRMaster lRMaster, Consigner consigner, Consigner consignee)
        {
            StringBuilder jsonBuilder = new StringBuilder();
            //if (Model.Type=="1")
            {
                string VehicleNo = "";
                HireVehicleMaster hireVehicle = ctxTFAT.HireVehicleMaster.Where(x => x.Code == Model.EWB_VehicleNo).FirstOrDefault();
                VehicleMaster vehicle = ctxTFAT.VehicleMaster.Where(x => x.Code == Model.EWB_VehicleNo).FirstOrDefault();

                if (hireVehicle != null)
                {
                    VehicleNo = Regex.Replace(hireVehicle.TruckNo, @"\s", "");
                    Model.EWB_LastVehicleNo = hireVehicle.Code;
                }
                else if (vehicle != null)
                {
                    VehicleNo = Regex.Replace(vehicle.TruckNo, @"\s", "");
                    Model.EWB_LastVehicleNo = vehicle.Code;
                }
                else
                {
                    VehicleNo = Regex.Replace(Model.EWB_VehicleNo, @"\s", "");
                    Model.EWB_LastVehicleNo = VehicleNo;
                }

                jsonBuilder.Append("{");
                jsonBuilder.Append("\"supplyType\":\"" + Model.EWB_SupplyType + "\",");
                jsonBuilder.Append("\"subSupplyType\":\"" + Model.EWB_SubSupplyType + "\",");
                jsonBuilder.Append("\"subSupplyDesc\":\"" + Model.EWB_SubSupplyDesc + "\",");
                jsonBuilder.Append("\"docType\":\"" + Model.EWB_DocType + "\",");
                jsonBuilder.Append("\"docNo\":\"" + Model.Document + "\",");
                jsonBuilder.Append("\"docDate\":\"" + lRMaster.BookDate.ToShortDateString() + "\",");

                //Consignor Details Fill
                jsonBuilder.Append("\"fromGstin\":\"" + Model.EWB_ConsignorGST.Trim() + "\",");
                jsonBuilder.Append("\"fromTrdName\":\"" + consigner.Name.ToUpper().Trim() + "\",");
                jsonBuilder.Append("\"fromAddr1\":\"" + "2ND CROSS NO 59  19  A" + "\",");
                jsonBuilder.Append("\"fromAddr2\":\"" + "GROUND FLOOR OSBORNE ROAD" + "\",");
                jsonBuilder.Append("\"fromPlace\":\"" + (ctxTFAT.TfatBranch.Where(x=>x.Code==lRMaster.Source).Select(x=>x.Name).FirstOrDefault()) + "\",");
                jsonBuilder.Append("\"fromPincode\":" + Model.EWB_ConsignorPincode.ToString() + ",");
                jsonBuilder.Append("\"actFromStateCode\":"+(ctxTFAT.TfatState.Where(x=>x.Code.ToString()==Model.EWB_ConsignorState).Select(x=>x.StateCode).FirstOrDefault() )+ ",");
                jsonBuilder.Append("\"fromStateCode\":" + (ctxTFAT.TfatState.Where(x => x.Code.ToString() == Model.EWB_ConsignorState).Select(x => x.StateCode).FirstOrDefault()) + ",");

                //Consignee Details Fill
                jsonBuilder.Append("\"toGstin\":\"" + Model.EWB_ConsigneeGST.Trim() + "\",");
                jsonBuilder.Append("\"toTrdName\":\"" + consignee.Name.ToUpper().Trim() + "\",");
                jsonBuilder.Append("\"toAddr1\":\"" + "Shree Nilaya" + "\",");
                jsonBuilder.Append("\"toAddr2\":\"" + "Dasarahosahalli" + "\",");
                jsonBuilder.Append("\"toPlace\":\"" + (ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name).FirstOrDefault()) + "\",");
                jsonBuilder.Append("\"toPincode\":" + Model.EWB_ConsigneePincode.ToString() + ",");
                jsonBuilder.Append("\"actToStateCode\":" + (ctxTFAT.TfatState.Where(x => x.Code.ToString() == Model.EWB_ConsigneeState).Select(x => x.StateCode).FirstOrDefault()) + ",");
                jsonBuilder.Append("\"toStateCode\":" + (ctxTFAT.TfatState.Where(x => x.Code.ToString() == Model.EWB_ConsigneeState).Select(x => x.StateCode).FirstOrDefault()) + ",");

                //Transaction Details
                jsonBuilder.Append("\"transactionType\":" + Model.EWB_TRNType + ",");
                jsonBuilder.Append("\"dispatchFromGSTIN\":\"" + Model.EWB_ConsignorGST.Trim()  + "\",");
                jsonBuilder.Append("\"dispatchFromTradeName\":\"" + consigner.Name.ToUpper().Trim() + "\",");
                jsonBuilder.Append("\"shipToGSTIN\":\"" + Model.EWB_ConsigneeGST.Trim() + "\",");
                jsonBuilder.Append("\"shipToTradeName\":\"" + consignee.Name.ToUpper().Trim() + "\",");
                jsonBuilder.Append("\"otherValue\":0,");
                jsonBuilder.Append("\"totalValue\":"+Convert.ToInt32(lRMaster.DecVal)+",");
                jsonBuilder.Append("\"cgstValue\":0,");
                jsonBuilder.Append("\"sgstValue\":0,");
                jsonBuilder.Append("\"igstValue\":0,");
                jsonBuilder.Append("\"cessValue\":0,");
                jsonBuilder.Append("\"cessNonAdvolValue\":0,");
                jsonBuilder.Append("\"totInvValue\":"+Convert.ToInt32(lRMaster.DecVal)+",");
                jsonBuilder.Append("\"transporterId\":\"" + (musername) + "\",");
                jsonBuilder.Append("\"transporterName\":\"" + (ctxTFAT.TfatComp.Select(x => x.Name).FirstOrDefault()) + "\",");
                jsonBuilder.Append("\"transDocNo\":\"" + Model.Document + "\",");
                jsonBuilder.Append("\"transMode\":\"" + Model.EWB_TRNMode + "\",");
                jsonBuilder.Append("\"transDistance\":\"" + Model.EWB_Distance + "\",");
                jsonBuilder.Append("\"transDocDate\":\"" + lRMaster.BookDate.ToShortDateString() + "\",");
                jsonBuilder.Append("\"vehicleNo\":\"" + VehicleNo.ToUpper() + "\",");
                jsonBuilder.Append("\"vehicleType\":\"" + Model.EWB_VehicleType + "\",");
                jsonBuilder.Append("\"itemList\":[");

                #region ItemList
                jsonBuilder.Append("{");
                jsonBuilder.Append("\"productName\":\"" + Model.EWB_productName + "\",");
                jsonBuilder.Append("\"productDesc\":\"" + (ctxTFAT.DescriptionMaster.Where(x=>x.Code==lRMaster.DescrType).Select(x=>x.Description).FirstOrDefault()) + "\",");
                jsonBuilder.Append("\"hsnCode\":\"" + Model.EWB_hsnCode + "\",");
                jsonBuilder.Append("\"quantity\":"+lRMaster.TotQty+",");
                jsonBuilder.Append("\"qtyUnit\":\"" + lRMaster.UnitCode + "\",");
                jsonBuilder.Append("\"cgstRate\":" + 0 + ",");
                jsonBuilder.Append("\"sgstRate\":" + 0 + ",");
                jsonBuilder.Append("\"igstRate\":" + 0 + ",");
                jsonBuilder.Append("\"cessRate\":" + 0 + ",");
                jsonBuilder.Append("\"cessNonAdvol\":" + 0 + ",");
                jsonBuilder.Append("\"taxableAmount\":"+Convert.ToInt32(lRMaster.DecVal)+"");
                jsonBuilder.Append("},");
                #endregion

                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                jsonBuilder.Append("]},");

                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            }
            var Date = DateTime.Now.ToShortDateString();

            
            //jsonBuilder.Append("\"dispatchFromGSTIN\":\"" + "29AAAAA1303P1ZV" + "\",");
            //jsonBuilder.Append("\"dispatchFromTradeName\":\"" + "ABC Traders" + "\",");
            //jsonBuilder.Append("\"shipToGSTIN\":\"" + "29ALSPR1722R1Z3" + "\",");
            //jsonBuilder.Append("\"shipToTradeName\":\"" + "XYZ Traders" + "\",");
            

            return jsonBuilder;
        }
        public string GetToken()
        {
            // user specific information: username, password, gstin
            // fixed/constant values: ip_address, client_id, client_secret, email
            string mtoken = "";
            try
            {
                var URL = new UriBuilder(weburl+ authenticateurl);

                var queryString = HttpUtility.ParseQueryString(string.Empty);
                queryString["email"] = memail;
                queryString["username"] = musername;
                queryString["password"] = mpassword;
                URL.Query = queryString.ToString();
                var client = new WebClient();
                client.Headers.Add("ip_address", "223.177.54.203");
                client.Headers.Add("client_id", "9bc17437-eeae-405a-b5b3-47f373a3fcaa");
                client.Headers.Add("client_secret", "43286fa2-59d1-4a2e-91c5-72548bf6c336");
                client.Headers.Add("gstin", "05AAACH6188F1ZM");
                string mstr = client.DownloadString(URL.ToString());
                dynamic mjson = Newtonsoft.Json.Linq.JValue.Parse(Newtonsoft.Json.JsonConvert.DeserializeObject(mstr).ToString());
                mtoken = mjson.GetValue("status_cd").ToString();

                //mexpiry = Convert.ToDateTime(mjson.GetValue("TokenExpiry").ToString());
                //SaveLog(musername, mgstin, "Auth", "", mtoken, "Success");

            }
            catch (Exception mex)
            {
                mtoken = "";
            }
            return mtoken;
        }
        

        #region using RestClient && RestRequest

        public ActionResult HitApi(EwayBillVM Model, StringBuilder json)
        {
            string Status = "Success", EwaybillNo = "", EwatValidUpto = "", errormsg = "";
            JObject mjson = JObject.Parse(json.ToString());
            JsonResult jsonResult = new JsonResult();

            string mtoken = GetToken();
            if (mtoken == "0")
            {
                jsonResult = Json(new { Status = "Error", errormsg = "Authentication Not Sucessed...!" }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
                return jsonResult;
            }

            weburl = weburl + genewaybillurl;
            
            RestClient client = new RestClient(weburl);
            RestRequest request = new RestRequest(Method.POST);
            
            request.AddParameter("email", memail, ParameterType.QueryString);
            request.AddHeader("ip_address", "49.36.17.253");
            request.AddHeader("client_id", "9bc17437-eeae-405a-b5b3-47f373a3fcaa");
            request.AddHeader("client_secret", "43286fa2-59d1-4a2e-91c5-72548bf6c336");
            request.AddHeader("gstin", "05AAACH6188F1ZM");
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
                LRMaster lRMaster = new LRMaster();
                SaveEWB(Model,"LR000", irnresponseData.GetValue("ewayBillNo").ToString(),irnresponseData.GetValue("validUpto").ToString(),"GEN", lRMaster);
                SaveEWBLog("LR000", Model.Document,"Generate Eway Bill","Sucess", irnresponseData.GetValue("ewayBillNo").ToString());
                ExecuteStoredProc("Update LRMaster set EwayBill='"+ irnresponseData.GetValue("ewayBillNo").ToString() + "' where LRno=" + Model.Document + "");
            }
            else
            {
                Status = "Error";
                dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("error").ToString());
                errormsg += "Get An Error.\n Error Is:" + irnresponseData.GetValue("message").ToString() + "";
                SaveEWBLog("LR000", Model.Document, "Generate Eway Bill", irnresponseData.GetValue("message").ToString(),"");
            }
            jsonResult = Json(new { Status = Status, errormsg = errormsg, EwaybillNo = EwaybillNo, EwatValidUpto = EwatValidUpto }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        #endregion

        




        #region using WebRequest 
        public string GenerateEwayBill1(StringBuilder json)
        {
            string mtoken = GetToken();
            //StringBuilder json = GetJSON();
            string uri = "https://api.mastergst.com/ewaybillapi/v1.03/ewayapi/genewaybill?email=Sureshpandule7%40gmail.com";
            Uri ourUri = new Uri(uri);
            WebRequest request = WebRequest.Create(ourUri);

            StreamReader reader;
            string rawresponse;
            request.ContentType = "application/json";
            request.Method = "POST";

            request.Headers.Add("ip_address", "49.36.17.253");
            request.Headers.Add("client_id", "9bc17437-eeae-405a-b5b3-47f373a3fcaa");
            request.Headers.Add("client_secret", "43286fa2-59d1-4a2e-91c5-72548bf6c336");
            request.Headers.Add("gstin", "05AAACH6188F1ZM");

            string postdata = json.ToString();
            request.ContentLength = postdata.Length;
            StreamWriter requestWriter = new StreamWriter(request.GetRequestStream());
            requestWriter.Write(postdata);
            requestWriter.Close();
            WebResponse response = (request.GetResponse());
            reader = new StreamReader(response.GetResponseStream());
            rawresponse = reader.ReadToEnd();
            var read = Newtonsoft.Json.Linq.JObject.Parse(rawresponse);
            return "";
        }
        #endregion
    }

}