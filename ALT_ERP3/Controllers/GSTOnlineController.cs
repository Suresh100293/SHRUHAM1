using ALT_ERP3.Areas.Accounts.Models;
using EntitiModel;
using Newtonsoft.Json.Linq;
using QRCoder;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Controllers
{
    public class GSTOnlineController : BaseController
    {
        // GET: GSTOnline

        // fixed values
        private static string mweburl = "https://api.mastergst.com/";
        private static string mip_address = "49.36.17.253";
        //trial
        //private static string mclient_id = "f7c66fe3-701e-432d-8eb2-4e825626dc01";
        //private static string mclient_secret = "0b6b4be2-b347-415f-b198-ab2e7b46025c";
        // prod
        private static string mclient_id = "adbd34ca-3f8b-401e-8a85-dc25ae5c3f74";
        private static string mclient_secret = "4eda80f5-8396-4af8-9a50-06db239d9437";
        private static string memail = "sds@suchansoftware.com";
        private string mlocation = "";
        //private static string mgstuserid = "mastergst";
        //private static string mgstpassword = "Malli#123";
        // list of urls
        //private static string mauthurl = "einvoice/authenticate";
        private static string mcancelurl = "einvoice/type/CANCEL/version/V1_03";
        private static string mgetirnurl = "einvoice/type/GENERATE/version/V1_03";
        private static string mewayurl = "einvoice/type/GENERATE_EWAYBILL/version/V1_03";
        private static string mewaycancelurl = "einvoice/type/CANCEL_EWAYBILL/version/V1_03";
        private static string mgetirndetails = "einvoice/type/GETIRNBYDOCDETAILS/version/V1_03";

        public string GetToken()
        {
            // user specific information: username, password, gstin
            // fixed/constant values: ip_address, client_id, client_secret, email
            string mtoken = "";
            try
            {
                DateTime mexpiry = DateTime.Now;
                DataRow mbranch = null;
                if (mlocation != "")
                {
                    mbranch = GetDataRow("Select EInvoiceID,EInvoicePassword,EInvoiceGSTNo=GSTNo,GSTAuthToken,GSTTokenExpiry from Warehouse where Code='" + mlocation + "'");
                }
                else
                {
                    mbranch = GetDataRow("Select EInvoiceID,EInvoicePassword,EInvoiceGSTNo,GSTAuthToken,GSTTokenExpiry from TfatComp");
                }
                //DataRow mbranch = GetDataRow("Select EInvoiceID,EInvoicePassword,GSTNo,GSTAuthToken,GSTTokenExpiry from TfatBranch where Code='" + mbranchcode + "'");
                if (!DBNull.Value.Equals(mbranch["GSTTokenExpiry"]))
                    mexpiry = Convert.ToDateTime(mbranch["GSTTokenExpiry"]);
                if (DateTime.Now.AddMinutes(3) <= mexpiry)
                {
                    if (mbranch["GSTAuthToken"].ToString() != "") mtoken = mbranch["GSTAuthToken"].ToString();
                }
                if (mtoken != "") return mtoken;
                // generate new token forcefully
                string musername = mbranch["EInvoiceID"].ToString();
                string mpassword = mbranch["EInvoicePassword"].ToString();
                string mgstin = mbranch["EInvoiceGSTNo"].ToString();
                var URL = new UriBuilder(mweburl + "einvoice/authenticate");

                var queryString = HttpUtility.ParseQueryString(string.Empty);
                queryString["email"] = memail;
                URL.Query = queryString.ToString();
                var client = new WebClient();
                client.Headers.Add("username", musername);
                client.Headers.Add("password", mpassword);
                client.Headers.Add("gstin", mgstin);
                client.Headers.Add("ip_address", mip_address);
                client.Headers.Add("client_id", mclient_id);
                client.Headers.Add("client_secret", mclient_secret);
                client.Headers.Add("Accept", "application/json");
                string mstr = client.DownloadString(URL.ToString());
                dynamic mjson = Newtonsoft.Json.Linq.JValue.Parse(Newtonsoft.Json.JsonConvert.DeserializeObject(mstr).ToString());
                mtoken = mjson.GetValue("data").ToString();
                mjson = Newtonsoft.Json.Linq.JValue.Parse(Newtonsoft.Json.JsonConvert.DeserializeObject(mtoken).ToString());
                mtoken = mjson.GetValue("AuthToken").ToString();
                string msek = mjson.GetValue("Sek").ToString();
                mexpiry = Convert.ToDateTime(mjson.GetValue("TokenExpiry").ToString());
                SaveLog(musername, mgstin, "Auth", "", mtoken, "Success");
                if (mlocation != "")
                {
                    ExecuteStoredProc("Update Warehouse Set GSTSEK='" + msek + "',GSTTokenExpiry='" + MMDDYYTime(mexpiry) + "',GSTAuthToken='" + mtoken + "' Where Code='" + mlocation + "'");
                }
                else
                {
                    ExecuteStoredProc("Update TfatComp Set GSTSEK='" + msek + "',GSTTokenExpiry='" + MMDDYYTime(mexpiry) + "',GSTAuthToken='" + mtoken + "' ");
                }
                //string msek = mjson.GetValue("sek").ToString();
            }
            catch (Exception mex)
            {
                mtoken = "";
            }
            return mtoken;
        }

        public string ValidateGSTNumber(string mgsttocheck)
        {
            string mname;
            // user specific information: username, password, gstin
            // fixed/constant values: ip_address, client_id, client_secret, email
            try
            {
                string mtoken = GetToken();
                if (mtoken == "")
                {
                    return "Error! Getting Token Details";
                }
                GetLocation("");
                DataRow mbranch = null;
                if (mlocation != "")
                {
                    mbranch = GetDataRow("Select EInvoiceID,EInvoicePassword,GSTNo from Warehouse where Code='" + mlocation + "'");
                }
                else
                {
                    mbranch = GetDataRow("Select EInvoiceID,EInvoicePassword,GSTNo from TfatBranch where Code='" + mbranchcode + "'");
                }
                string musername = mbranch["EInvoiceID"].ToString();
                string mpassword = mbranch["EInvoicePassword"].ToString();
                string mgstin = mbranch["GSTNo"].ToString();
                var URL = new UriBuilder(mweburl + "einvoice/type/GSTNDETAILS/version/V1_03");
                var queryString = HttpUtility.ParseQueryString(string.Empty);
                queryString["param1"] = mgsttocheck;
                queryString["email"] = memail;
                URL.Query = queryString.ToString();
                var client = new WebClient();
                client.Headers.Add("ip_address", mip_address);
                client.Headers.Add("client_id", mclient_id);
                client.Headers.Add("client_secret", mclient_secret);
                client.Headers.Add("username", musername);
                client.Headers.Add("auth-token", mtoken);
                client.Headers.Add("gstin", mgstin);
                client.Headers.Add("Accept", "application/json");// it worked without this parameter also
                string respx = client.DownloadString(URL.ToString());
                dynamic mjson = Newtonsoft.Json.Linq.JValue.Parse(Newtonsoft.Json.JsonConvert.DeserializeObject(respx).ToString());
                mname = mjson.GetValue("data").ToString();
                mjson = Newtonsoft.Json.Linq.JValue.Parse(Newtonsoft.Json.JsonConvert.DeserializeObject(mname).ToString());
                mname = mjson.GetValue("TradeName").ToString();
                mname += "|" + mjson.LegalName;
                mname += "|" + (mjson.AddrBno ?? "") + " " + (mjson.AddrBnm ?? "");
                mname += "|" + (mjson.AddrFlno ?? "") + " " + (mjson.AddrSt ?? "");
                mname += "|" + (mjson.AddrLoc ?? "");
                mname += "|" + (mjson.StateCode ?? "");
                mname += "|" + (mjson.AddrPncd ?? "");
                SaveLog(musername, mgstin, "ValidGST", mgsttocheck, mname, "Success");
            }
            catch (Exception mex)
            {
                mname = "Error! Fetching Party Details";
            }
            return mname;
        }

        public string FOT(string mQuery)
        {
            string mStr;
            DataTable mDt = GetDataTable(mQuery);
            if (mDt != null && mDt.Rows.Count > 0) { mStr = mDt.Rows[0][0].ToString(); } else { mStr = ""; }
            mDt.Dispose();
            return mStr;
        }

        public int FOTInt(string mQuery)
        {
            int mvalue;
            DataTable mDt = GetDataTable(mQuery);
            if (mDt != null && mDt.Rows.Count > 0)
            {
                mvalue = !DBNull.Value.Equals(mDt.Rows[0][0]) ? Convert.ToInt32(mDt.Rows[0][0]) : 0;
            }
            else { mvalue = -1; }
            mDt.Dispose();
            return mvalue;
        }

        public string CancelIRN(string mparentkey)
        {
            GetLocation(mparentkey);
            //string mtable = GetTableName(GetSubType(mparentkey.Substring(0, 5)));
            string mtable = "";
            var tmpBranchCode = mparentkey.Substring(0, 6);
            if (ctxTFAT.TfatBranch.Where(x => x.Code == tmpBranchCode).FirstOrDefault() != null)
            {
                mtable = GetTableName(GetSubType(mparentkey.Substring(6, 5)));
            }
            else
            {
                mtable = GetTableName(GetSubType(mparentkey.Substring(0, 5)));
            }

            string mirn = FOT("Select GSTIRNNumber from " + mtable + " Where TableKey='" + mparentkey + "'");
            string mremark = FOT("Select CancelReason from " + mtable + " Where TableKey='" + mparentkey + "'");
            if (mremark == "") mremark = "Wrong Entry";
            int mid = FOTInt("Select CancelID from " + mtable + " Where TableKey='" + mparentkey + "'");
            if (mid <= 0) mid = 4;

            JObject mjson = new JObject(
                                new JProperty("Irn", mirn),
                                new JProperty("CnlRsn", mid.ToString()),   //1- Duplicate, 2 - Data entry mistake, 3- Order Cancelled, 4 - Others
                                new JProperty("CnlRem", mremark));

            string mtoken = GetToken();
            if (mtoken == "")
            {
                return "Error! Getting Token Details";
            }
            DataRow mbranchs = null;
            if (mlocation != "")
            {
                mbranchs = GetDataRow("Select EInvoiceID,EInvoiceGSTNo=GSTNo from Warehouse Where Code='" + mlocation + "'");
            }
            else
            {
                mbranchs = GetDataRow("Select EInvoiceID,EInvoiceGSTNo from TfatComp ");
            }

            string musername = mbranchs["EInvoiceID"].ToString();
            //string mpassword = mbranchs["EInvoicePassword"].ToString();
            string mgstin = mbranchs["EInvoiceGSTNo"].ToString();

            //musername = mgstuserid;
            //mpassword = mgstpassword;

            RestClient client = new RestClient(mweburl + mcancelurl);
            RestRequest request = new RestRequest(Method.POST);
            request.AddParameter("email", memail, ParameterType.QueryString);
            request.AddHeader("ip_address", mip_address);
            request.AddHeader("client_id", mclient_id);
            request.AddHeader("client_secret", mclient_secret);
            request.AddHeader("username", musername);
            //request.AddHeader("password", mpassword);
            request.AddHeader("auth-token", mtoken);
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
                string mdate = irnresponseData.GetValue("CancelDate").ToString();
                ExecuteStoredProc("Update " + mtable + " Set Amt=0,Amt1=0,CGSTAMT=0,CurrAmount=0,IGSTAMT=0,SGSTAMT=0,QTY=0,QTY2=0,Roundoff=0,Taxable=0,Val1=0,Narr='-- Cancelled --',CancelFlag=0,CancelFlagGST=-1,CancelDate='" + MMDDYYTime(Convert.ToDateTime(mdate)) + "',EWBNo='' Where TableKey='" + mparentkey + "'");
                ExecuteStoredProc("Delete from Lrbill where Parentkey='" + mparentkey + "'");
                ExecuteStoredProc("Delete from Outstanding where Parentkey='" + mparentkey + "'");
                ExecuteStoredProc("Delete from Ledger where Parentkey='" + mparentkey + "'");
                ExecuteStoredProc("Delete from StockBatch where Parentkey='" + mparentkey + "'");
                ExecuteStoredProc("Delete from StockSerial where Parentkey='" + mparentkey + "'");
                ExecuteStoredProc("Delete from StockMore where Parentkey='" + mparentkey + "'");
                ExecuteStoredProc("Delete from StockTax where Parentkey='" + mparentkey + "'");
                ExecuteStoredProc("Delete from StockOS where Parentkey='" + mparentkey + "'");
                // update pending for the challans
                //string mstr = RecToString("Select ChlnKey from Stock Where Parentkey='" + mparentkey + "'");
                //ExecuteStoredProc("Update Stock Set Pending=abs(Qty) where Branch='" + mbranch + "' and Charindex(Type+Prefix+sno+Srl,'" + mstr + "')<>0");
                //ExecuteStoredProc("Update " + mtable + " Set ChlnKey='' where ChlnKey='" + mparentkey + "'");
                ExecuteStoredProc("Delete from Stock where Parentkey='" + mparentkey + "'");

                SaveLog(musername, mgstin, "CancelIRN", mparentkey, mirn, "Success");
                mirn = "IRN for the Document is Cancelled:\n" + mirn;
            }
            else
            {
                SaveLog(musername, mgstin, "CancelIRN", mparentkey, mirn, "Error");
                ExecuteStoredProc("Update " + mtable + " Set CancelFlag=0,CancelFlagGST=-1,EWBNo='' Where TableKey='" + mparentkey + "'");
                dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("status_desc").ToString());
                mreturn = irnresponseData.ToString().Replace("@", "");
                UpdateError(mreturn, mtable, mparentkey);
                mirn = "Error! Document Not found or already Cancelled..";
            }
            return mirn;
        }

        public void UpdateError(string merror, string mtablex, string mparentkey)
        {
            //merror = merror.ToString().Replace("@", "");
            //merror = merror.Substring(merror.IndexOf("ErrorMessage"));
            merror = GetErrorString(merror);
            SqlConnection conn = new SqlConnection(GetConnectionString());
            conn.Open();
            SqlCommand command = new SqlCommand(@"Update " + mtablex + " Set GSTError=@payload Where TableKey='" + mparentkey + "'", conn);
            IDataParameter par = command.CreateParameter();
            par.ParameterName = "@payload";
            par.DbType = DbType.String;
            par.Value = merror;
            command.Parameters.Add(par);
            command.ExecuteNonQuery();
        }

        public string GenerateEWay(string mparentkey)
        {
            GetLocation(mparentkey);
            string mstr;
            string mdataerror = "";
            string mtrxid = "";
            string mtrxname = "";
            string mtrxmode = "0";
            int mdistance = 0;
            string mtable = GetTableName(GetSubType(mparentkey.Substring(0, 5)));
            string mirn = FOT("Select GSTIRNNumber from " + mtable + " Where TableKey='" + mparentkey + "'");
            if (mirn == "") return "Error! IRN Number is not generated yet..";
            string mparty = FOT("Select DelyCode from " + mtable + " Where TableKey='" + mparentkey + "'");
            int msno = FOTInt("Select DelyAltAdd from " + mtable + " Where TableKey='" + mparentkey + "'");
            DataRow mtrx = GetDataRow("Select * from TransportDetail Where TableKey='" + mparentkey + "'");
            string mveh = null;
            string mdoc = null;
            string mdocdate = null;
            if (mtrx != null)
            {
                mveh = mtrx["vehicleno"].ToString().Trim();
                if (mveh == "")
                {
                    mveh = null;
                }
                else if (mveh.Length < 4 || mveh.Length > 20)
                {
                    mdataerror += "Transport Vehicle Number is not Proper. |";
                }
                mdistance = FOTInt("Select Distance from Address where Code='" + mparty + "' and Sno=" + msno);
                mtrxid = FOT("Select GSTNo from Address where Code='" + mtrx["TransporterCode"].ToString().Trim() + "'").Trim();
                if (mtrxid == "")
                {
                    mtrxid = null;
                }
                else if (mtrxid.Length != 15)
                {
                    mdataerror += "Transporter ID is not Proper. |";
                }
                mtrxname = FOT("Select Name from Master where Code='" + mtrx["TransporterCode"].ToString().Trim() + "'").Trim();
                if (mtrxname == "")
                {
                    mtrxname = null;
                }
                else if (mtrxname.Length < 3 || mtrxname.Length > 100)
                {
                    mdataerror += "Transporter Name is not Proper. |";
                }
                mtrxmode = mtrx["TransMode"] == null ? "1" : mtrx["TransMode"].ToString();
                switch (mtrxmode)
                {
                    case "Rail":
                        mtrxmode = "2";
                        break;
                    case "Air":
                        mtrxmode = "3";
                        break;
                    case "Ship":
                        mtrxmode = "4";
                        break;
                    default:
                        mtrxmode = "1";
                        break;
                }
                if (mtrxmode == "0") mtrxmode = "1";
                if (mtrxmode == "0") mtrxmode = "1";
                mdocdate = Convert.ToDateTime(mtrx["NoteDate"]).ToString("dd/MM/yyyy");
                mdoc = mtrx["NoteNo"].ToString().Trim();
                if (mdoc == "")
                {
                    mdoc = null;
                    mdocdate = null;
                }
                else if (mdoc.Length > 15)
                {
                    mdataerror += "Transport Document Number is not Proper. |";
                }
                if (mdocdate != null)
                {
                    DateTime msalesdate = Convert.ToDateTime(Fieldoftable(mtable, "DocDate", "TableKey='" + mparentkey + "'", "D"));
                    if (Convert.ToDateTime(mdocdate) < msalesdate)
                    {
                        mdataerror += "Transport Document Date can not be earlier to Sales Date. |";
                    }
                }
            }
            else
            {
                mdataerror = "Document Not found..";
            }
            if (mdataerror != "")
            {
                ExecuteStoredProc("Update " + mtable + " Set GSTError='" + mdataerror + "' Where TableKey='" + mparentkey + "'");
                return mdataerror;
            }

            JObject mjson = new JObject(
                                new JProperty("Irn", mirn),
                                new JProperty("Transid", mtrxid),
                                new JProperty("Transname", mtrxname),
                                new JProperty("Distance", mdistance),
                                new JProperty("Transdocno", mdoc),
                                new JProperty("TransdocDt", mdocdate),
                                new JProperty("Vehno", mveh),
                                new JProperty("Vehtype", "R"),
                                new JProperty("TransMode", mtrxmode.Substring(0, 1)));

            if (mveh == null)
            {
                mjson.Remove("Vehno");
                mjson.Remove("Transdocno");
                mjson.Remove("TransdocDt");
                mjson.Remove("TransMode");
            }

            string mtoken = GetToken();
            if (mtoken == "")
            {
                return "Error! Getting Token Details";
            }
            DataRow mbranchs = null;
            if (mlocation != "")
            {
                mbranchs = GetDataRow("Select * from Warehouse Where Code='" + mlocation + "'");
            }
            else
            {
                mbranchs = GetDataRow("Select * from TfatBranch Where Code='" + mbranchcode + "'");
            }

            string musername = mbranchs["EInvoiceID"].ToString();
            //string mpassword = mbranchs["EInvoicePassword"].ToString();
            string mgstin = mbranchs["GSTNo"].ToString();

            //musername = mgstuserid;
            //mpassword = mgstpassword;

            RestClient client = new RestClient(mweburl + mewayurl);
            RestRequest request = new RestRequest(Method.POST);
            request.AddParameter("email", memail, ParameterType.QueryString);
            request.AddHeader("ip_address", mip_address);
            request.AddHeader("client_id", mclient_id);
            request.AddHeader("client_secret", mclient_secret);
            request.AddHeader("username", musername);
            request.AddHeader("auth-token", mtoken);
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
                string mewno = irnresponseData.GetValue("EwbNo").ToString();
                string mewdate = irnresponseData.GetValue("EwbDt").ToString();
                string mewvaliddate = irnresponseData.GetValue("EwbValidTill").ToString();
                if (mewvaliddate == "") mewvaliddate = mewdate;
                ExecuteStoredProc("Update TransportDetail Set GSTError='',EWBValidDate='" + MMDDYYTime(Convert.ToDateTime(mewvaliddate)) + "',EWBDate='" + MMDDYYTime(Convert.ToDateTime(mewdate)) + "',EWBNo='" + mewno + "' Where TableKey='" + mparentkey + "'");
                ExecuteStoredProc("Update " + mtable + " Set GSTEWayFlag=0,EWBValidDate='" + MMDDYYTime(Convert.ToDateTime(mewvaliddate)) + "',EWBDate='" + MMDDYYTime(Convert.ToDateTime(mewdate)) + "',EWBNo='" + mewno + "',GSTError='' Where TableKey='" + mparentkey + "'");
                SaveLog(musername, mgstin, "CreateEWB", mparentkey, mewno, "Success");
                return "EWayBill Number for the Document is Generated:\n" + mirn + "\n" + mewno;
            }
            else
            {
                dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("status_desc").ToString());
                mreturn = irnresponseData.ToString().Replace("@", "");
                if (mreturn.Contains("4002"))//eway bill is already generated
                {
                    ExecuteStoredProc("Update TransportDetail Set GSTError='' Where TableKey='" + mparentkey + "'");
                    ExecuteStoredProc("Update " + mtable + " Set GSTEWayFlag=0,GSTError='' Where TableKey='" + mparentkey + "'");
                }
                else
                {
                    UpdateError(mreturn, mtable, mparentkey);
                    UpdateError(mreturn, "TransportDetail", mparentkey);
                    //mirn = irnresponseData.ToString().Replace("@", "");
                    //mirn = mirn.Substring(mirn.IndexOf("ErrorMessage"));
                    //ExecuteStoredProc("Update TransportDetail Set GSTError='" + mirn + "' Where TableKey='" + mparentkey + "'");
                    //ExecuteStoredProc("Update " + mtable + " Set GSTError='" + mirn + "' Where TableKey='" + mparentkey + "'");
                }
                SaveLog(musername, mgstin, "CreateEWB", mparentkey, "", "Error");
                return "Error!\n" + GetErrorString(irnresponseData.ToString()).Replace("@", "");
            }
        }

        public string CancelEWB(string mparentkey)
        {
            GetLocation(mparentkey);
            string mtable = GetTableName(GetSubType(mparentkey.Substring(0, 5)));
            string mewb = FOT("Select EWBNo from " + mtable + " Where TableKey='" + mparentkey + "'");
            string mremark = FOT("Select CancelEWBReason from " + mtable + " Where TableKey='" + mparentkey + "'");
            if (mremark == "") mremark = "Wrong Entry";
            int mid = FOTInt("Select CancelEWBID from " + mtable + " Where TableKey='" + mparentkey + "'");
            if (mid <= 0) mid = 2;
            JObject mjson = new JObject(
                                new JProperty("ewbNo", Convert.ToInt64(mewb)),
                                new JProperty("cancelRsnCode", mid),   //1- Duplicate, 2 - Data entry mistake, 3- Order Cancelled, 4 - Others
                                new JProperty("cancelRmrk", mremark));

            string mtoken = GetToken();
            if (mtoken == "")
            {
                return "Error! Getting Token Details";
            }
            DataRow mbranchs = null;
            if (mlocation != "")
            {
                mbranchs = GetDataRow("Select * from Warehouse Where Code='" + mlocation + "'");
            }
            else
            {
                mbranchs = GetDataRow("Select * from TfatBranch Where Code='" + mbranchcode + "'");
            }
            string mgstin;
            string musername = mbranchs["EInvoiceID"].ToString();
            string mpassword = mbranchs["EInvoicePassword"].ToString();
            mgstin = mbranchs["GSTNo"].ToString();
            RestClient client = new RestClient(mweburl + mewaycancelurl);
            RestRequest request = new RestRequest(Method.POST);
            request.AddParameter("email", memail, ParameterType.QueryString);
            request.AddHeader("ip_address", mip_address);
            request.AddHeader("client_id", mclient_id);
            request.AddHeader("client_secret", mclient_secret);
            request.AddHeader("username", musername);
            request.AddHeader("auth-token", mtoken);
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
                string mdate = irnresponseData.GetValue("cancelDate").ToString();
                ExecuteStoredProc("Update " + mtable + " Set EWBNo='',CancelFlagEWB=0,CancelEWBDate='" + MMDDYYTime(Convert.ToDateTime(mdate)) + "' Where TableKey='" + mparentkey + "'");
                ExecuteStoredProc("Update TransportDetail Set EWBNo='' Where TableKey='" + mparentkey + "'");
                SaveLog(musername, mgstin, "CancelEWB", mparentkey, mewb, "Success");
                return "EWB for the Document is Cancelled:\n" + mewb;
            }
            else
            {
                //mreturn = JValue.Parse(irnresponseorder.GetValue("status_desc").ToString());
                //SqlConnection conn = new SqlConnection(GetConnectionString());
                //conn.Open();
                //SqlCommand command = new SqlCommand(@"Update " + mtable + " Set GSTError=@payload Where TableKey='" + mparentkey + "'", conn);
                //IDataParameter par = command.CreateParameter();
                //par.ParameterName = "@payload";
                //par.DbType = DbType.String;
                //par.Value = irnresponseorder.ToString().Replace("@", "");
                //command.Parameters.Add(par);
                //command.ExecuteNonQuery();
                //command.Dispose();
                mreturn = "Cancel EWB: Error: " + message.Replace("@", "");
                UpdateError(mreturn, mtable, mparentkey);
                SaveLog(musername, mgstin, "CancelEWB", mparentkey, mewb, "Error");
                //ExecuteStoredProc("Update " + mtable + " Set CancelFlagEWB=0 Where TableKey='" + mparentkey + "'");
                return "Error! Document Not found or already Cancelled..";
            }
        }

        private void SaveLog(string musername, string mgstin, string mevent, string mdocument, string mdata, string mstatus)
        {
            ExecuteStoredProc("Insert into GSTLog (Status,GSTUserID,EntryDate,Event,Document,GSTNo,Data) values ('" + mstatus + "','" + musername + "','" + MMDDYYTime(DateTime.Now) + "','" + mevent + "','" + mdocument + "','" + mgstin + "','" + mdata + "')");
        }

        public string GetIRNData(string mparentkey, string mcustomerid)
        {
            GetLocation(mparentkey);
            if (string.IsNullOrEmpty(mparentkey)) return "";
            string mtable = GetTableName(GetSubType(mparentkey.Substring(0, 5)));
            DataRow msales = GetDataRow("Select Sales.*,SalesMore.TdsAmt from " + mtable + " Sales," + mtable + "More SalesMore Where Sales.TableKey='" + mparentkey + "' and Sales.TableKey=SalesMore.TableKey");
            if (msales == null) return "";
            string msubtype = msales["SubType"].ToString();
            //msubtype = msubtype == "RS" ? "INV" : (msubtype == "NP" ? "DBN" : "CRN");
            string minvno = msales["Srl"].ToString();
            if (minvno.StartsWith("0")) minvno = msales["Type"].ToString() + "-" + minvno;

            int n = FOTInt("Select GSTDocType from DocTypes where Code='" + msales["Type"].ToString() + "'");
            if (n == 1)
            {
                msubtype = "DBN";
            }
            else if (n == 2)
                msubtype = "CRN";
            else
            {
                msubtype = msubtype == "RS" || msubtype == "XS" ? "INV" : (msubtype == "NP" ? "DBN" : "CRN");
            }
            if (minvno.StartsWith("0")) minvno = msales["Type"].ToString() + "-" + minvno;

            string mirn = "";
            string mtoken = GetToken();
            if (mtoken == "")
            {
                ExecuteStoredProc("Update " + mtable + " Set GSTError='Invalid Token..Check UserID/Password' Where TableKey='" + mparentkey + "'");
                return "Error! Getting Token Details";
            }

            string mgstin;
            //if (mIsTrial == false)
            //{
            DataRow mbranchs = null;
            if (mlocation != "")
            {
                mbranchs = GetDataRow("Select * from Warehouse Where Code='" + mlocation + "'");
            }
            else
            {
                mbranchs = GetDataRow("Select * from TfatBranch Where Code='" + mbranchcode + "'");
            }
            string musername = mbranchs["EInvoiceID"].ToString();
            string mpassword = mbranchs["EInvoicePassword"].ToString();
            mgstin = mbranchs["GSTNo"].ToString();

            // method-1 doesnt return response : status = ok
            RestClient client = new RestClient(mweburl + mgetirndetails);
            RestRequest request = new RestRequest(Method.GET);
            request.AddParameter("email", memail, ParameterType.QueryString);
            request.AddParameter("param1", msubtype, ParameterType.QueryString);
            request.AddHeader("docnum", minvno);
            request.AddHeader("docdate", Convert.ToDateTime(msales["DocDate"]).ToString("dd/MM/yyyy"));
            request.AddHeader("ip_address", mip_address);
            request.AddHeader("client_id", mclient_id);
            request.AddHeader("client_secret", mclient_secret);
            request.AddHeader("username", musername);
            request.AddHeader("password", mpassword);
            request.AddHeader("auth-token", mtoken);
            request.AddHeader("gstin", mgstin);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/json");
            var response = client.Execute(request);
            string message = response.Content;
            dynamic irnresponseorder = JObject.Parse(message);
            string mStatuscd = JValue.Parse(irnresponseorder.GetValue("status_cd").ToString());
            SqlConnection conn = new SqlConnection(GetConnectionString());
            conn.Open();
            SqlCommand command;
            if (mStatuscd == "1")
            //                if (!message.Contains("status_cd"))
            {
                dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("data").ToString());
                string mAckNo = irnresponseData.GetValue("AckNo").ToString();
                string mAckDt = irnresponseData.GetValue("AckDt").ToString();
                string mSignedInvoice = irnresponseData.GetValue("SignedInvoice").ToString();
                mirn = irnresponseData.GetValue("Irn").ToString();
                string mSignedQRCode = irnresponseData.GetValue("SignedQRCode").ToString();
                //string mStatus = irnresponseData.GetValue("Status").ToString();
                // sometimes due to some error in execnonquery data is not saved so we save the critical infrmation f
                try
                {
                    ExecuteStoredProc(@"Update " + mtable + " Set GSTError='',GSTAckNo='" + mAckNo + "',GSTAckDate='" + MMDDYYTime(Convert.ToDateTime(mAckDt)) + "', GSTIRNNumber='" + mirn + "',GSTQRCode='" + mSignedQRCode + "',GSTeInoivce='" + mSignedInvoice + "' Where TableKey='" + mparentkey + "'");
                }
                catch { }
                try
                {
                    //string minv = Decode(mSignedQRCode); 
                    command = new SqlCommand(@"Update " + mtable + " Set GSTError='',GSTAckNo='" + mAckNo + "',GSTAckDate='" + MMDDYYTime(Convert.ToDateTime(mAckDt)) + "', GSTIRNNumber='" + mirn + "',GSTQRCode='" + mSignedQRCode + "',GSTeInoivce='" + mSignedInvoice + "',QRCodeImage=@payload Where TableKey='" + mparentkey + "'", conn);
                    IDataParameter par = command.CreateParameter();
                    par.ParameterName = "@payload";
                    par.DbType = DbType.Binary;
                    //par.Value = GetQRCodeImage(Decode(mSignedQRCode));
                    par.Value = GetQRCodeImage(mSignedQRCode);
                    command.Parameters.Add(par);
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                catch { }
                SaveLog(musername, mgstin, "GetIRNData", mparentkey, mirn, "S");
                mirn = "Sucess^" + mirn;
            }
            else
            {
                mirn = "Error IRN Data:\n" + message;
                dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("status_desc").ToString());
                command = new SqlCommand(@"Update " + mtable + " Set GSTError=@payload Where TableKey='" + mparentkey + "'", conn);
                IDataParameter par = command.CreateParameter();
                par.ParameterName = "@payload";
                par.DbType = DbType.String;
                mirn = irnresponseData.ToString().Replace("@", "");
                mirn = GetErrorString(mirn);
                par.Value = mirn;
                command.Parameters.Add(par);
                command.ExecuteNonQuery();
                command.Dispose();
                SaveLog(musername, mgstin, "GetIRNData", mparentkey, "", "E");
                mirn = "Error^" + mirn;
            }
            conn.Close();
            conn.Dispose();
            return mirn;
        }

        public void GetLocation(string mparentkey)
        {
            mlocation = "";
            //if (FieldoftableBool("TfatBranch", "gp_Warehouse", "Code='" + mbranchcode + "'") == true)
            //{
            //    if (mparentkey != "")
            //    {
            //        string mtype = mparentkey.Substring(0, 5);
            //        mlocation = Fieldoftable("DocTypes", "LocationCode", "Code='" + mtype + "'");
            //        if (mlocation == "") mlocation = "."; // dummy locaion code for gettoken etc;
            //    }

            //}
            //else
            //{
            //    mlocation = "";
            //}
        }

        public void GenerateQRImage(string mparentkey, string mcustomerid)
        {
            string mtable = "";
            var TempBranchCode = mparentkey.Substring(0, 6);
            if (ctxTFAT.TfatBranch.Where(x => x.Code == TempBranchCode).FirstOrDefault() != null)
            {
                mtable = GetTableName(GetSubType(mparentkey.Substring(6, 5)));
            }
            else
            {
                mtable = GetTableName(GetSubType(mparentkey.Substring(0, 5)));
            }
            SqlConnection conn = new SqlConnection(GetConnectionString());
            conn.Open();
            SqlCommand command;
            string mSignedQRCode = Fieldoftable(mtable, "GSTQRCode", "TableKey='" + mparentkey + "'");
            command = new SqlCommand(@"Update " + mtable + " Set QRCodeImage=@payload Where TableKey='" + mparentkey + "'", conn);
            IDataParameter par = command.CreateParameter();
            par.ParameterName = "@payload";
            par.DbType = DbType.Binary;
            par.Value = GetQRCodeImage(mSignedQRCode);
            command.Parameters.Add(par);
            command.ExecuteNonQuery();
            command.Dispose();
        }

        public string GetIRNNumber(string mparentkey, string mcustomerid)
        {
            GetLocation(mparentkey);
            if (string.IsNullOrEmpty(mparentkey)) return null;
            //string mtable = "Sales";// GetTableName(GetSubType(mparentkey.Substring(0, 5)));
            string mtable = "";
            var TempBranchCode = mparentkey.Substring(0, 6);
            if (ctxTFAT.TfatBranch.Where(x => x.Code == TempBranchCode).FirstOrDefault() != null)
            {
                mtable = GetTableName(GetSubType(mparentkey.Substring(6, 5)));
            }
            else
            {
                mtable = GetTableName(GetSubType(mparentkey.Substring(0, 5)));
            }

            DataRow msales = GetDataRow("Select Sales.*,SMTDSAmt=SalesMore.TdsAmt from " + mtable + " Sales," + mtable + "More SalesMore Where Sales.TableKey='" + mparentkey + "' and Sales.TableKey=SalesMore.TableKey");
            var TotalAMt = Math.Abs(Math.Round(Convert.ToDecimal(msales["Taxable"].ToString()) + Convert.ToDecimal(msales["cGSTAmt"].ToString()) + Convert.ToDecimal(msales["sGSTAmt"].ToString()) + Convert.ToDecimal(msales["iGSTAmt"].ToString()), 0));
            var RoundOff = Math.Abs(Math.Round((Convert.ToDecimal(TotalAMt.ToString())) - (Convert.ToDecimal(msales["Taxable"].ToString()) + Convert.ToDecimal(msales["cGSTAmt"].ToString()) + Convert.ToDecimal(msales["sGSTAmt"].ToString()) + Convert.ToDecimal(msales["iGSTAmt"].ToString())), 2));
            if (msales == null) return null;
            string mtype = msales["Type"].ToString();
            GetLocation(mtype);
            string msubtype = msales["SubType"].ToString();
            string mmsubtype = msubtype;
            string mshipfrom = msales["ShipFrom"].ToString().Trim();
            int n = FOTInt("Select GSTDocType from DocTypes where Code='" + mtype + "'");
            if (n == 1)
            {
                msubtype = "DBN";
            }
            else if (n == 2)
                msubtype = "CRN";
            else
            {
                msubtype = msubtype == "RS" || msubtype == "XS" || msubtype == "CS" ? "INV" : (msubtype == "NP" ? "DBN" : "CRN");
            }
            string minvno = msales["Srl"].ToString();
            string mdataerror = "";
            if (minvno.StartsWith("0")) minvno = mtype + "-" + minvno;
            if (minvno.StartsWith("0")) mdataerror += "Invoice Number Can't Start with 0|";
            string mgstsubtype = "";
            string mstr = "";
            decimal mdiscount = 0;
            decimal mother = 0;
            if (mtable == "Sales")
            {
                mdiscount = Math.Abs(Math.Round(Convert.ToDecimal(msales["Discount"].ToString()), 2));
            }
            mother = Math.Abs(Math.Round(Convert.ToDecimal(msales["SMTDSAmt"].ToString()), 2));
            List<DataRow> chgs = GetDataTable("Select EQAmt from Charges Where Fld<>'F001' and Type='" + mtype + "'").AsEnumerable().ToList();
            n = 2;
            foreach (DataRow mrow in chgs)
            {
                if (mrow[0].ToString().Trim() == "+")
                {
                    mother += Math.Round(Convert.ToDecimal(msales["VAL" + n].ToString()), 2);
                }
                else if (mrow[0].ToString().Trim() == "-")
                {
                    mdiscount += Math.Abs(Math.Round(Convert.ToDecimal(msales["VAL" + n].ToString()), 2));
                }
                ++n;
            }
            //B2B,SEZWP,SEZWOP,EXPWP,EXPWOP,DEXP
            //0,5,7,8,9,13,15,16
            mgstsubtype = "B2B";
            switch ((int)msales["GSTType"])
            {
                case 0:             //0 - Tax Invoice
                    mgstsubtype = "B2B";
                    break;
                case 1:             //1 - Reverse Charge
                    break;
                case 2:
                    break;
                case 3:
                    break;
                case 4:
                    break;
                case 5: //EXPWOP    5 - Export(under LUT)
                    mgstsubtype = "EXPWOP";
                    break;
                case 6:
                    break;
                case 7: //SEZWP     7 - SEZ with Payment
                    mgstsubtype = "SEZWP";
                    break;
                case 8: //SEZWOP    8 - SEZ w / o Payment
                    mgstsubtype = "SEZWOP";
                    break;
                case 9: //EXPWP     9 - Export(with Duty Payment)
                    mgstsubtype = "EXPWP";
                    break;
                case 10:
                    break;
                case 11:
                    break;
                case 12:
                    break;
                case 13: //DEXP     13 - Deemed Exports
                    mgstsubtype = "DEXP";
                    break;
                case 14:
                    break;
                case 15: //EXPWP    15 - Export @0.1%
                    mgstsubtype = (mmsubtype == "RS" ? "B2B" : "EXPWP");
                    break;
                case 16: //EXPWP    16 - Export @0.05%
                    mgstsubtype = (mmsubtype == "RS" ? "B2B" : "EXPWP");
                    break;
                case 17: //EXPWOP   17 - Export(Without Payment of Duty)
                    mgstsubtype = "EXPWOP";
                    break;
            }

            string mbranchtel = "";
            string mbranchemail = "";
            string mbranchadd1 = "";
            string mbranchadd2 = "";
            DataRow mbranch = null;
            if (mlocation != "")
            {
                mbranch = GetDataRow("Select *,aPin=Pin,Addrl1=Adrl1,Addrl2=Adrl2,Email='',Tel1=Telephone from Warehouse Where Code='" + mlocation + "'");
            }
            else
            {
                mbranch = GetDataRow("Select * from TfatBranch Where Code='" + msales["Branch"].ToString() + "'");
            }
            if (mbranch != null)
            {
                mstr = mbranch["State"].ToString().Trim();
                if (mstr.Length < 1)
                {
                    mdataerror += "Branch State is required. |";
                }
                mstr = mbranch["aPin"].ToString().Trim();
                if (mstr.Length < 1)
                {
                    mdataerror += "Branch PinCode is required. |";
                }
                mbranchadd1 = mbranch["Addrl1"].ToString().Trim();
                if (mbranchadd1.Length < 1)
                {
                    mdataerror += "Branch Address Line 1 is required. |";
                }
                mbranchadd2 = mbranch["Addrl2"].ToString().Trim();
                if (mbranchadd2.Length < 3) mbranchadd2 = null;
                if (mbranchadd2 == "") mbranchadd2 = null;
                mbranchemail = mbranch["Email"].ToString().Trim();
                if (mbranchemail.Length < 6) mbranchemail = null;
                if (mbranchemail == "") mbranchemail = null;
                mbranchtel = mbranch["Tel1"].ToString().Trim();
                if (mbranchtel.Length < 6) mbranchtel = null;
                if (mbranchtel == "") mbranchtel = null;
            }

            string mfromname = null;
            string mfromadrl1 = null;
            string mfromadrl2 = null;
            string mfromcity = null;
            string mfrompin = null;
            string mfromstatecode = null;
            if (mshipfrom != "")
            {
                DataRow mAddf = GetDataRow("Select * from CAddress Where Code='" + mshipfrom + "' and Sno=0");
                if (mAddf != null)
                {
                    mfromname = mAddf["Name"].ToString().Trim();
                    if (mfromname.Length < 1)
                    {
                        mdataerror += "Despatch from Name is required. |";
                    }
                    mfromadrl1 = mAddf["Adrl1"].ToString().Trim();
                    if (mfromadrl1.Length < 1)
                    {
                        mdataerror += "Despatch from Address is required. |";
                    }
                    mfromadrl2 = mAddf["Adrl2"].ToString().Trim();
                    if (mfromadrl2 == "") mfromadrl2 = null;
                    mfromcity = mAddf["City"].ToString().Trim();
                    if (mfromcity.Length < 3)
                    {
                        mdataerror += "Despatch from City is required. |";
                    }
                    mfrompin = mAddf["Pin"].ToString().Trim();
                    if (mfrompin.Length < 6)
                    {
                        mdataerror += "Despatch from PinCode is required. |";
                    }
                    mfromstatecode = FOT("Select StateCode from TfatState where Name='" + mAddf["State"].ToString().Trim() + "'").Trim();
                    if (mfromstatecode.Length < 1)
                    {
                        mdataerror += "Despatch from StateCode is required. |";
                    }
                }
            }
            string mbillgst = "";
            string mbillstate = "";
            string mbillpos = "";
            string mbilladd1 = "";
            string mbilladd2 = "";
            string mbillemail = "";
            string mbilltel = "";
            string mbilllegal = "";
            string mbillname = "";
            string mbillcity = "";
            string mbillpin = "";
            DataRow mAdd = GetDataRow("Select * from CAddress Where Code='" + msales["Code"].ToString() + "' and Sno=" + Convert.ToInt32(msales["AltAddress"]));
            if (mAdd != null)
            {
                mbillname = mAdd["Name"].ToString().Trim();
                if (mbillname.Length < 1)
                {
                    mdataerror += "Bill to Name is required. |";
                }
                mbilllegal = mAdd["LegalName"].ToString().Trim();
                if (mbilllegal.Length < 1)
                {
                    mbilllegal = mbillname;
                }
                if (mcustomerid != "OMSAI1")
                {
                    mbilladd1 = mAdd["Adrl1"].ToString().Trim();
                    if (mbilladd1.Length < 1)
                    {
                        mdataerror += "Bill to Address is required. |";
                    }
                    mbilladd2 = mAdd["Adrl2"].ToString().Trim();
                    if (mbilladd2 == "") mbilladd2 = null;
                    mbillcity = Fieldoftable("TfatCity", "Name", "Code='" + mAdd["City"].ToString().Trim() + "'");//mAdd["City"].ToString().Trim();
                    if (mbillcity.Length < 1)
                    {
                        mdataerror += "Bill to City is required. |";
                    }
                    mbillpin = mAdd["Pin"].ToString().Replace(" ", "").Trim();
                    if (mbillpin.Length < 1 && mgstsubtype == "B2B")
                    {
                        mdataerror += "Bill to PinCode is required. |";
                    }
                }
                mbillgst = mAdd["GSTNo"].ToString().Trim();
                if (mbillgst.Length < 15 && mgstsubtype == "B2B")
                {
                    mdataerror += "Bill to GST Number is required. |";
                }

                mbillemail = null;
                mbilltel = null;
                //mbillpos = Fieldoftable("TfatState", "StateCode", "Name='" + msales["PlaceOfSupply"].ToString().Trim() + "'");
                //var TempSaleBranch = msales["Branch"].ToString();
                //var BranchStateCode = ctxTFAT.TfatBranch.Where(x => x.Code == TempSaleBranch.ToString()).Select(x => x.State).FirstOrDefault();
                mbillpos = Fieldoftable("TfatState", "StateCode", "Code=" + mAdd["State"].ToString().Trim() + "");
                if (mbillpos.Length < 1)
                {
                    mdataerror += "Bill POS is required. |";
                }
                mbillstate = mbillpos;
            }

            string mshipname = "";
            string mshiplegal = "";
            string mshipadd1 = "";
            string mshipadd2 = "";
            string mshipcity = "";
            string mshippin = "0";
            string mshipgst = null;
            string mshipstate = "";
            //DataRow mAdd2 = GetDataRow("Select * from Address Where Code='" + msales["DelyCode"].ToString().Trim() + "' and Sno=" + (int)msales["DelyAltAdd"]);
            DataRow mAdd2 = GetDataRow("Select * from Address Where Code='0' and Sno=" + Convert.ToInt32(msales["DelyAltAdd"]));
            if (mAdd2 != null)
            {
                mshipname = mAdd["Name"].ToString().Trim();
                if (mshipname.Length < 1)
                {
                    mdataerror += "Ship to Name is required. |";
                }
                mshiplegal = mAdd["LegalName"].ToString().Trim();
                if (mshiplegal.Length < 1)
                {
                    mshiplegal = mshipname;
                }
                if (mcustomerid != "OMSAI1")
                {
                    mshipadd1 = mAdd2["Adrl1"].ToString().Trim();
                    if (mshipadd1.Length < 1)
                    {
                        mdataerror += "Ship to Address is required. |";
                    }
                    mshipadd2 = mAdd2["Adrl2"].ToString().Trim();
                    mshipadd2 = mshipadd2 == "" ? null : mshipadd2;
                    mshipcity = mAdd2["City"].ToString().Trim();
                    if (mshipcity.Length < 1)
                    {
                        mdataerror += "Ship to City is required. |";
                    }
                    mshippin = mAdd2["Pin"].ToString().Trim().Replace(" ", "");
                    if (mshippin.Length < 1 && mgstsubtype == "B2B")
                    {
                        mdataerror += "Ship to PinCode is required. |";
                    }
                }
                mshipgst = mAdd2["GSTNo"].ToString().Trim();

                mshipstate = FOT("Select StateCode from TfatState where Name='" + mAdd2["State"].ToString().Trim() + "'").Trim();

            }

            // omsai customisation
            if (mcustomerid == "OMSAI1")
            {
                // take mbilladd1,mbilladd2,mbillcity,mbillpin and mshipadd1,mshipadd2,mshipcity,mshippin from addons
                // F027-mbilladd1, F028-mshipadd1
                DataRow mAddx = GetDataRow("Select badd1=F033,badd2=F034,bcity=F035,bpin=F036,sadd1=F037,sadd2=F038,scity=F039,spin=F040 from AddonDocSL Where TableKey='" + mparentkey + "'");
                if (mAddx != null)
                {
                    mbilladd1 = mAddx["badd1"].ToString().Trim();
                    if (mbilladd1.Length < 1)
                    {
                        mdataerror += "Bill to Address is required. |";
                    }
                    mbilladd2 = mAddx["badd2"].ToString().Trim();
                    mbilladd2 = mbilladd2 == "" ? null : mbilladd2;
                    mbillcity = mAddx["bcity"].ToString().Trim();
                    if (mbillcity.Length < 1)
                    {
                        mdataerror += "Bill to City is required. |";
                    }
                    mbillpin = mAddx["bpin"].ToString().Trim().Replace(" ", "");
                    if (mbillpin.Length < 1)
                    {
                        mdataerror += "Bill to PinCode is required. |";
                    }
                    // shipping details
                    mshipadd1 = mAddx["sadd1"].ToString().Trim();
                    if (mshipadd1.Length < 1)
                    {
                        mdataerror += "Ship to Address is required. |";
                    }
                    mshipadd2 = mAddx["sadd2"].ToString().Trim();
                    mshipadd2 = mshipadd2 == "" ? null : mshipadd2;
                    mshipcity = mAddx["scity"].ToString().Trim();
                    if (mshipcity.Length < 1)
                    {
                        mdataerror += "Ship to City is required. |";
                    }
                    mshippin = mAddx["spin"].ToString().Trim().Replace(" ", "");
                    if (mshippin.Length < 1)
                    {
                        mdataerror += "Ship to PinCode is required. |";
                    }
                }
            }

            if (mdataerror != "")
            {
                ExecuteStoredProc("Update " + mtable + " Set GSTError='" + mdataerror + "' Where TableKey='" + mparentkey + "'");
                return mdataerror;
            }
            else
            {
                ExecuteStoredProc("Update " + mtable + " Set GSTError='' Where TableKey='" + mparentkey + "'");
            }
            DataRow mexp = mexp = GetDataRow("Select * from Expdet Where TableKey='" + mparentkey + "'");
            string mshipbill = null;
            DateTime mshipdate = DateTime.Now.Date;
            if (mexp != null)
            {
                mshipbill = mexp["SBNo"].ToString() ?? "";
                if (mshipbill == "")
                {
                    mshipbill = null;
                }
            }
            //List<DataRow> mstocks = GetDataTable("Select TableKey=s.TableKey,Sno=s.Sno,Rate=(case when s.Rate2<>0 then s.Rate2 else s.Rate end),Amt=abs(s.Amt),Qty=(case when s.Rateon<>0 then s.qty2 else s.Qty end),QtyRate=(case when (case when s.Rateon<>0 then s.qty2 else s.Qty end)*s.Rate<>0 then (case when s.Rateon<>0 then s.qty2 else s.Qty end)*(case when s.Rate2<>0 then s.Rate2 else s.Rate end) else cast(st.Taxable as Money) end),s.Code, Taxable=Abs(st.Taxable),sGSTAmt=abs(st.sGSTAmt),cGSTAmt=abs(st.cGSTAmt),iGSTAmt=abs(st.iGSTAmt),GSTRate=st.iGSTRate+st.sGSTRate+st.cGSTRate,st.Cess,* from Stock s join StockTax st on s.TableKey = st.TableKey Where s.ParentKey='" + mparentkey + "' order by s.Sno").AsEnumerable().ToList();
            //List<DataRow> mstocks = GetDataTable("select Type=SR.Type,TableKey=s.TableKey,Sno=s.Sno,HSNCode=lr.hsncode,Qty=s.TotQty,FreeQty='false',Unit=(select U.Name from alt_shakti.[dbo].unitmaster U where U.code=lr.unitcode),PrdDesc=(select U.Description from alt_shakti.[dbo].descriptionmaster U where U.code=lr.DescrType),Rate=case when s.TotQty=0 then 0 else ROUND((s.amt/s.TotQty),3) end,QtyRate= ROUND((case when s.TotQty=0 then 0 else ROUND((s.amt/s.TotQty),3) end)*(s.TotQty),3),Taxable=s.Amt,GSTRate=SR.IGSTRate+SR.CGSTRate+SR.SGSTRate,sGSTAmt=case when SR.SGSTRate=0 then 0 else  ROUND((s.amt*SR.SGSTRate/100),3) end,iGSTAmt=case when SR.IGSTRate=0 then 0 else ROUND((s.amt*SR.IGSTRate/100),3) end,cGSTAmt=case when SR.CGSTRate=0 then 0 else ROUND((s.amt*SR.CGSTRate/100),3) end,Cess=0,Amt=s.Amt+case when SR.SGSTRate=0 then 0 else  ROUND((s.amt*SR.SGSTRate/100),3) end+case when SR.IGSTRate=0 then 0 else ROUND((s.amt*SR.IGSTRate/100),3) end+case when SR.CGSTRate=0 then 0 else ROUND((s.amt*SR.CGSTRate/100),3) end from alt_shakti.[dbo].lrbill s left join alt_shakti.[dbo].lrmaster lr on s.lrreftablekey=lr.tablekey left join alt_shakti.[dbo].Ledger SR on SR.parentkey=s.parentkey where s.parentkey='" + mparentkey + "' and SR.sno=1 order by s.Sno").AsEnumerable().ToList();
            List<ChergesListWithHSN> mstocks = new List<ChergesListWithHSN>();
            Ledger ledger = ctxTFAT.Ledger.Where(x => x.Sno == 1 && x.ParentKey == mparentkey).FirstOrDefault();
            int mcnt = 1;
            foreach (var item in ctxTFAT.Charges.Where(x => x.Type == "LR000").ToList())
            {
                
                var tempId = Convert.ToInt16(item.Fld.Substring(1, 3));
                var Amount = GetLrWiseChargeValValue(tempId, mparentkey);
                if (Amount != 0)
                {
                    decimal IGSTAmt = 0,CGSTAmt = 0,SGSTAmt = 0;
                    if (ledger.IGSTRate.Value!=0)
                    {
                        IGSTAmt = Math.Round(Math.Abs(Convert.ToDecimal(((Amount * ledger.IGSTRate.Value) / 100).ToString())), 3);
                    }
                    if (ledger.CGSTRate.Value!=0)
                    {
                        CGSTAmt = Math.Round(Math.Abs(Convert.ToDecimal(((Amount * ledger.CGSTRate.Value) / 100).ToString())), 3);
                    }
                    if (ledger.SGSTRate.Value!=0)
                    {
                        SGSTAmt = Math.Round(Math.Abs(Convert.ToDecimal(((Amount * ledger.SGSTRate.Value) / 100).ToString())), 3);
                    }

                    mstocks.Add(new ChergesListWithHSN
                    {
                        Type = "SLR00",
                        TableKey = mparentkey,
                        Sno = mcnt,
                        HSNCode = string.IsNullOrEmpty(item.HSNCode)==true?"": item.HSNCode,
                        Qty = 1,
                        FreeQty = false,
                        Unit = item.Head,
                        PrdDesc = string.IsNullOrEmpty(item.HSNCode) == true ? "" : ctxTFAT.HSNMaster.Where(x => x.Code == item.HSNCode).Select(x => x.Name).FirstOrDefault(),
                        Rate = Amount,
                        QtyRate = Amount,
                        Taxable = Amount,
                        GSTRate = Convert.ToDecimal(ledger.IGSTRate + ledger.CGSTRate + ledger.SGSTRate),
                        sGSTAmt = SGSTAmt,
                        iGSTAmt = IGSTAmt,
                        cGSTAmt = CGSTAmt,
                        Cess =0,
                        Amt= Amount+ SGSTAmt+ IGSTAmt+ CGSTAmt
                    });
                    ++mcnt;
                }
            }


            List<AddOns> mAddonsList = new List<AddOns>();
            //foreach (var a in mstocks)
            //{
            //    mAddonsList.AddRange(GetPAddInEditView("SL", a["TableKey"].ToString(), a["Type"].ToString()));
            //    mAddonsList.AddRange(GetPAddInEditView("SL", a.TableKey.ToString(), a.Type.ToString()));
            //}
            // for exports etc.
            if (mgstsubtype != "B2B" && mgstsubtype != "SEZWP" && mgstsubtype != "SEZWOP")
            {
                mbillpin = "999999";
                mbillstate = "96";
                mbillgst = "URP";
                mbillpos = "96";

                mshipgst = "URP";
                mshippin = "999999";
                mshipstate = "96";
                // SDS 02.12.2022
                // the shipping address should be Port address for exports
                if (mgstsubtype == "EXPWP" || mgstsubtype == "EXPWOP")
                {
                    mshipadd1 = FOT("Select Add1 from Port where Code=" + mexp["PortLoad"].ToString().Trim());
                    mshipadd2 = FOT("Select Add2 from Port where Code=" + mexp["PortLoad"].ToString().Trim());
                    mshipcity = FOT("Select City from Port where Code=" + mexp["PortLoad"].ToString().Trim());
                    mshippin = FOT("Select Pin from Port where Code=" + mexp["PortLoad"].ToString().Trim());
                    mshipstate = FOT("Select State from Port where Code=" + mexp["PortLoad"].ToString().Trim());
                    mshipstate = FOT("Select StateCode from TfatState where Name='" + mshipstate + "'").Trim();
                }
            }

            JObject vendorobj = new JObject(
                                new JProperty("Version", "1.1"),
                                new JProperty("TranDtls",
                                new JObject(
                                    new JProperty("TaxSch", "GST"),
                                    new JProperty("SupTyp", mgstsubtype),   //B2B  ,SEZWP , SEZWOP, EXPWP, EXPWOP, DEXP
                                    new JProperty("RegRev", "N"),           // whether reverse charge
                                    new JProperty("EcmGstin", null),
                                    new JProperty("IgstOnIntra", "N")       // Y/N  Y-indicates the supply is intra state but chargeable to IGST
                                    )),
                                new JProperty("DocDtls",
                                new JObject(
                                    new JProperty("Typ", msubtype),         //INV-Invoice ,CRN-Credit Note, DBN-Debit Note
                                    new JProperty("No", minvno),
                                    new JProperty("Dt", Convert.ToDateTime(msales["DocDate"]).ToString("dd/MM/yyyy"))/*Convert.ToDateTime(msales["DocDate"]).ToString("dd/MM/yyyy")*/
                                    )),
                                new JProperty("SellerDtls",
                                new JObject(
                                    new JProperty("Gstin", mbranch["GSTNo"].ToString().Trim()),
                                    new JProperty("LglNm", mcompname.ToString().Trim()),//mbranch["Name"].ToString().Trim()
                                    new JProperty("TrdNm", mcompname.ToString().Trim()),//mbranch["Name"].ToString().Trim()
                                    new JProperty("Addr1", mbranchadd1),
                                    new JProperty("Addr2", mbranchadd2),
                                    new JProperty("Loc", mbranch["City"].ToString().Trim()),
                                    new JProperty("Pin", Convert.ToInt32(mbranch["aPin"].ToString().Replace(" ", "").Trim())),
                                    new JProperty("Stcd", Fieldoftable("TfatState", "StateCode", "Code='" + mbranch["State"].ToString().Trim() + "'")),
                                    new JProperty("Ph", mbranchtel),
                                    new JProperty("Em", mbranchemail)
                                    )),
                                new JProperty("BuyerDtls",
                                new JObject() {
                                    new JProperty("Gstin",  mbillgst),//mAdd["GSTNo"].ToString().Trim()
                                    new JProperty("LglNm",   mbilllegal),
                                    new JProperty("TrdNm",   mbillname),
                                    new JProperty("Pos",  mbillpos),//Fieldoftable("TfatState", "StateCode", "Name='" + msales["PlaceOfSupply"].ToString().Trim() + "'")
                                    new JProperty("Addr1",  mbilladd1),
                                    new JProperty("Addr2",   mbilladd2),
                                    new JProperty("Loc",  mbillcity),
                                    new JProperty("Pin",  Convert.ToInt32(string.IsNullOrEmpty(mbillpin)==true?"0":mbillpin)),
                                    new JProperty("Stcd", mbillstate.ToString().Trim() ),//Fieldoftable("TfatState", "StateCode", "Name='" + mAdd["State"].ToString().Trim() + "'")
                                    new JProperty("Ph",  mbilltel),
                                    new JProperty("Em",  mbillemail)
                                }),

                                    new JProperty("DispDtls", new JObject() {   // ship from details
                                    new JProperty("Nm", mfromname),
                                    new JProperty("Addr1",   mfromadrl1),
                                    new JProperty("Addr2",   mfromadrl2),
                                    new JProperty("Loc",   mfromcity),
                                    new JProperty("Pin",  Convert.ToInt32(string.IsNullOrEmpty(mfrompin)==true?"0":mfrompin)),
                                    new JProperty("Stcd", mfromstatecode)
                                    }),

                                    new JProperty("ShipDtls", mshipgst != "" ?
                                    (new JObject() {
                                    new JProperty("Gstin",  mshipgst),//mAdd2["GSTNo"].ToString().Trim()
                                    new JProperty("LglNm",   mshiplegal),
                                    new JProperty("TrdNm",   mshipname),
                                    new JProperty("Addr1",  mshipadd1),
                                    new JProperty("Addr2",  mshipadd2),
                                    new JProperty("Loc", mshipcity),
                                    new JProperty("Pin",  Convert.ToInt32(string.IsNullOrEmpty(mshippin)==true?"0":mshippin)),
                                    new JProperty("Stcd", mshipstate)//Fieldoftable("TfatState", "StateCode", "Name='" + mAdd2["State"].ToString().Trim() + "'")
                                    }) :
                                    (new JObject() {
                                    new JProperty("LglNm",   mshiplegal),
                                    new JProperty("TrdNm",   mshipname),
                                    new JProperty("Addr1",  mshipadd1),
                                    new JProperty("Addr2",  mshipadd2),
                                    new JProperty("Loc", mshipcity),
                                    new JProperty("Pin",  Convert.ToInt32(string.IsNullOrEmpty(mshippin)==true?"0":mshippin)),
                                    new JProperty("Stcd", mshipstate)//Fieldoftable("TfatState", "StateCode", "Name='" + mAdd2["State"].ToString().Trim() + "'")
                                    })
                                    ),
                                    new JProperty("ItemList",
                                    new JArray(
                                            from i in mstocks.AsEnumerable().ToList()
                                            select new JObject(
                                                new JProperty("SlNo", i.Sno.ToString()),
                                                new JProperty("IsServc", "Y"),//(Fieldoftable("ItemMaster", "ItemType", "Code='" + i["Code"].ToString().Trim() + "'") == "S" ? "Y" : "N")
                                                new JProperty("PrdDesc", i.Unit.ToString()),//GetItemName(i["Code"].ToString(), i["TableKey"].ToString(), mcustomerid)
                                                new JProperty("HsnCd", i.HSNCode.ToString().Replace(" ", "")),
                                                new JProperty("Qty", Math.Round(Math.Abs(Convert.ToDouble(i.Qty.ToString())), 3)),
                                                new JProperty("FreeQty", (Convert.ToBoolean(i.FreeQty) == true ? Math.Round(Convert.ToDouble(i.Qty.ToString()), 3) : 0)),
                                                //new JProperty("Unit", ""),//i.Unit.ToString().Trim().Substring(0,8)
                                                new JProperty("UnitPrice", Math.Round(Convert.ToDecimal(i.Rate.ToString()), 3)),
                                                new JProperty("TotAmt", Math.Abs(Math.Round(Convert.ToDecimal(i.QtyRate.ToString()), 2))),
                                                new JProperty("Discount", GetItemDisc(i.TableKey.ToString())),
                                                new JProperty("PreTaxVal", Math.Abs(Math.Round(Convert.ToDecimal(i.Taxable.ToString()), 2))),
                                                new JProperty("AssAmt", Math.Abs(Math.Round(Convert.ToDecimal(i.Taxable.ToString()), 2))),
                                                new JProperty("GstRt", Convert.ToDouble(i.GSTRate.ToString())),
                                                new JProperty("SgstAmt", Math.Round(Math.Abs(Convert.ToDecimal(i.sGSTAmt.ToString())), 2)),
                                                new JProperty("IgstAmt", Math.Round(Math.Abs(Convert.ToDecimal(i.iGSTAmt.ToString())), 2)),
                                                new JProperty("CgstAmt", Math.Round(Math.Abs(Convert.ToDecimal(i.cGSTAmt.ToString())), 2)),
                                                new JProperty("CesRt", Convert.ToDouble(i.Cess.ToString())),
                                                new JProperty("CesAmt", 0),
                                                new JProperty("CesNonAdvlAmt", 0),
                                                new JProperty("StateCesRt", 0),
                                                new JProperty("StateCesNonAdvlAmt", 0),
                                                new JProperty("OthChrg", 0),
                                                new JProperty("TotItemVal", Math.Round(Convert.ToDouble(i.Amt.ToString()), 2)),
                                                new JProperty("OrdLineRef", " "),   //(string.IsNullOrEmpty(i["OrdKey"].ToString()) == true) ? "  " : i["OrdKey"].ToString()),
                                                new JProperty("OrgCntry", "IN"),
                                                new JProperty("AttribDtls", new JArray(
                                                    from adl in mAddonsList.Where(x => x.TableKey == (string)i.TableKey).AsEnumerable().ToList()
                                                    select new JObject(
                                                        new JProperty("Nm", adl.Head),
                                                        new JProperty("Val", adl.ApplCode)
                                                    )))
                                        ))),
                                    new JProperty("ValDtls",
                                    new JObject() {
                                        new JProperty("AssVal",Math.Abs(Math.Round(Convert.ToDecimal(msales["Taxable"].ToString()),2))),
                                        new JProperty("CgstVal",Math.Abs(Math.Round(Convert.ToDecimal(msales["cGSTAmt"].ToString()),2))),
                                        new JProperty("SgstVal",Math.Abs(Math.Round(Convert.ToDecimal(msales["sGSTAmt"].ToString()),2))),
                                        new JProperty("IgstVal",Math.Abs(Math.Round(Convert.ToDecimal(msales["iGSTAmt"].ToString()),2))),
                                        new JProperty("CesVal",0),
                                        new JProperty("StCesVal",0),
                                        new JProperty("Discount",mdiscount),
                                        new JProperty("OthChrg",mother),
                                        //new JProperty("RndOffAmt",Math.Abs(Math.Round(Convert.ToDecimal(msales["RoundOff"].ToString()),2))),
                                        new JProperty("RndOffAmt",RoundOff),
                                        //new JProperty("TotInvVal",Math.Abs(Math.Round( Convert.ToDecimal(msales["Amt"].ToString()),2))),
                                        new JProperty("TotInvVal",TotalAMt),
                                        //new JProperty("TotInvValFc", Math.Abs(Math.Round(Convert.ToDecimal(msales["CurrAmount"].ToString()),2)))
                                        new JProperty("TotInvValFc", TotalAMt)
                                    }),

                                    new JProperty("AddlDocDtls",
                                    new JArray(
                                        new JObject()
                                        {
                                            new JProperty("Url","www.suchansoftware.com"),
                                            new JProperty("Info" ,"Generated using TFAT ERP, info@suchansoftware.com")
                                        })),
                                    new JProperty("ExpDtls",
                                    new JObject()
                                    {
                                        new JProperty("ShipBNo",mshipbill ),//(mexp != null) ? mexp["SBNo"].ToString().Trim():""
                                        new JProperty("ShipBDt",(mshipbill != null) ? Convert.ToDateTime(mexp["SBDate"]).ToString("dd/MM/yyyy"):null ),
                                        new JProperty("Port" ,(mexp != null) ? FOT("Select Name from Port where Code=" + mexp["PortLoad"].ToString().Trim()):""),
                                        new JProperty("RefClm" ,"N"),
                                        new JProperty("ForCur" ,""),
                                        new JProperty("CntCode" ,(mexp != null) ? mexp["CountryDest"].ToString().Trim():"IN"),
                                        new JProperty("ExpDuty" ,0)
                                    })

                                    );

            if (Convert.ToDecimal(msales["CurrRate"]) <= 1 || mshipbill == null)
            {
                vendorobj.Remove("ExpDtls");
            }
            // remove dispatch from if self dispatch
            if (mshipfrom == "")
            {
                vendorobj.Remove("DispDtls");
            }
            // remove ship to details if bill to and ship to are same
            if (mgstsubtype == "B2B" && msales["Code"].ToString() == msales["DelyCode"].ToString() && Convert.ToInt32(msales["AltAddress"]) == Convert.ToInt32(msales["DelyAltAdd"]))
            {
                vendorobj.Remove("ShipDtls");
            }
            //if (mtrxname.Trim() == "")
            //{
            //    mjson.Remove("EwbDtls");
            //}
            //if (mtrx["NoteNo"].ToString().Trim() == "")
            //{
            //    mjson.Remove("EwbDtls");
            //}
            //if (msales["Narr"].ToString().Trim() == "")
            //{
            //    vendorobj.Remove("RefDtls");
            //}

            string musername = "";
            string mpassword = "";
            string mgstin = "";
            DataRow mcomp = null;
            mcomp = GetDataRow("Select EInvoiceID,EInvoicePassword,EInvoiceGSTNo from TfatComp ");
            if (mcomp != null)
            {
                musername = mcomp["EInvoiceID"].ToString();
                mpassword = mcomp["EInvoicePassword"].ToString();
                mgstin = mcomp["EInvoiceGSTNo"].ToString();
            }
            else
            {
                return "Error! Please Check Company Details To Fill EInvoice Credentials...!";
            }


            string mvend = vendorobj.ToString();
            string mirn = "";
            string mtoken = GetToken();
            if (mtoken == "")
            {
                return "Error! Getting Token Details";
            }





            //musername = mgstuserid;
            //mpassword = mgstpassword;

            // method-1 doesnt return response : status = ok
            RestClient client = new RestClient(mweburl + mgetirnurl);
            RestRequest request = new RestRequest(Method.POST);
            request.AddParameter("email", memail, ParameterType.QueryString);
            request.AddHeader("ip_address", mip_address);
            request.AddHeader("client_id", mclient_id);
            request.AddHeader("client_secret", mclient_secret);
            request.AddHeader("username", musername);
            request.AddHeader("password", mpassword);
            request.AddHeader("auth-token", mtoken);
            request.AddHeader("gstin", mgstin);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", vendorobj, ParameterType.RequestBody);
            var response = client.Execute(request);
            string message = response.Content;
            dynamic irnresponseorder = JObject.Parse(message);
            string mStatuscd = JValue.Parse(irnresponseorder.GetValue("status_cd").ToString());
            SqlConnection conn = new SqlConnection(GetConnectionString());
            conn.Open();
            SqlCommand command;
            if (mStatuscd == "1")
            {
                dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("data").ToString());
                string mAckNo = irnresponseData.GetValue("AckNo").ToString();
                string mAckDt = irnresponseData.GetValue("AckDt").ToString();
                string mSignedInvoice = irnresponseData.GetValue("SignedInvoice").ToString();
                mirn = irnresponseData.GetValue("Irn").ToString();
                string mSignedQRCode = irnresponseData.GetValue("SignedQRCode").ToString();
                string mStatus = irnresponseData.GetValue("Status").ToString();
                //string mStatusdesc = JValue.Parse(irnresponseorder.GetValue("status_desc").ToString());
                //ExecuteStoredProc("Update Sales Set GSTAckNo='" + mAckNo + "',GSTAckDate='" + MMDDYY(Convert.ToDateTime(mAckDt)) + "', GSTIRNNumber='" + mirn + "',GSTQRCode='" + mSignedQRCode + "',GSTeInoivce='" + mSignedInvoice + "' Where TableKey='" + mparentkey + "'");//,QRCodeImage='" + GetQRCodeImage(Decode(mSignedQRCode)).ToString() + "'
                string minv = DecodeBase64(mSignedInvoice);
                command = new SqlCommand(@"Update " + mtable + " Set GSTError='',GSTAckNo='" + mAckNo + "',GSTAckDate='" + MMDDYY(Convert.ToDateTime(mAckDt)) + "', GSTIRNNumber='" + mirn + "',GSTQRCode='" + mSignedQRCode + "',GSTeInoivce='" + mSignedInvoice + "',QRCodeImage=@payload Where TableKey='" + mparentkey + "'", conn);
                IDataParameter par = command.CreateParameter();
                par.ParameterName = "@payload";
                par.DbType = DbType.Binary;
                //par.Value = GetQRCodeImage(Decode(mSignedQRCode));
                par.Value = GetQRCodeImage(mSignedQRCode);
                command.Parameters.Add(par);
                command.ExecuteNonQuery();
                command.Dispose();
                SaveLog(musername, mgstin, "CreateIRN", mparentkey, mirn, "Success");
                mirn = "IRN Number Successfully Generated:\n" + mirn;
            }
            else
            {
                mirn = "Error Generating IRN:\n" + message;
                dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("status_desc").ToString());
                if (irnresponseData.ToString().Contains("Duplicate IRN") == false)
                {
                    command = new SqlCommand(@"Update " + mtable + " Set GSTError=@payload Where TableKey='" + mparentkey + "'", conn);
                    IDataParameter par = command.CreateParameter();
                    par.ParameterName = "@payload";
                    par.DbType = DbType.String;
                    mirn = irnresponseData.ToString().Replace("@", "");
                    mirn = GetErrorString(mirn);
                    par.Value = mirn;// irnresponseData.ToString().Replace("@", "");
                    command.Parameters.Add(par);
                    command.ExecuteNonQuery();
                    command.Dispose();
                    SaveLog(musername, mgstin, "CreateIRN", mparentkey, "", "Error");
                    mirn = "Error Generating IRN:\n" + GetErrorString(message);
                }
                else
                {
                    mirn = GetIRNData(mparentkey, mcustomerid);
                    //SaveLog(musername, mgstin, "CreateIRN", mparentkey, "duplicateIRN", "Error");
                }
            }
            conn.Close();
            conn.Dispose();
            return mirn;
        }

        public decimal GetLrWiseChargeValValue(int i, string TableKey)
        {
            string connstring = GetConnectionString();
            decimal abc;

            var loginQuery3 = @"select sum(Val" + i + ") from LRBill where  Parentkey = '" + TableKey + "'";
            DataTable mDt2 = GetDataTable(loginQuery3, connstring);
            if (mDt2.Rows.Count > 0)
            {
                abc = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? 0 : Convert.ToDecimal(mDt2.Rows[0][0].ToString());
            }
            else
            {
                abc = 0;
            }

            return decimal.Round(abc, 2, MidpointRounding.AwayFromZero);
        }

        private double GetItemDisc(string mtablekey)
        {
            double mamt = 0;
            DataRow mrow = GetDataRow("Select Discount=isnull(Discount,0),ChgAmt1=isnull(ChgAmt1,0),ChgAmt2=isnull(ChgAmt2,0),ChgAmt3=isnull(ChgAmt3,0),ChgAmt4=isnull(ChgAmt4,0),ChgAmt5=isnull(ChgAmt5,0),ChgAmt6=isnull(ChgAmt6,0) from Stock where TableKey='" + mtablekey + "'");
            if (mrow != null)
            {
                mamt = Convert.ToDouble(mrow["Discount"]);
                try { mamt += Convert.ToDouble(mrow["ChgAmt1"]); } catch (Exception mex) { }
                try { mamt += Convert.ToDouble(mrow["ChgAmt2"]); } catch { }
                try { mamt += Convert.ToDouble(mrow["ChgAmt3"]); } catch { }
                try { mamt += Convert.ToDouble(mrow["ChgAmt4"]); } catch { }
                try { mamt += Convert.ToDouble(mrow["ChgAmt5"]); } catch { }
            }
            return Math.Round(Math.Abs(mamt), 2);
            //return Math.Abs(Math.Round(mamt, 2));
        }

        public string GetErrorString(string merror)
        {
            if (merror.IndexOf("ErrorMessage") > 0)
            {
                merror = merror.Substring(merror.IndexOf("ErrorMessage"));
            }
            merror = merror.ToString().Replace("@", "").Replace("{", "").Replace("}", "").Replace("]", "").Replace("[", "");
            return merror.Replace("{", "").Replace("}", "").Replace("[", "").Replace("]", "").Replace("\"", "");
        }

        public string GetItemName(string mcode, string mtablekey, string mcustomerid)
        {
            string mname;
            if (mcustomerid == "LB1234")       // leebo
            {
                mname = FOT("Select top 1 F002 from AddonItemMas where TableKey='" + mcode + "'");
                //mname = FOT("Select top 1 F002 from AddonItemSL where TableKey='" + mtablekey + "'");
            }
            else
            {
                mname = FOT("Select Name from ItemMaster where Code='" + mcode + "'");
            }
            mname = mname.Replace("\"", "").Trim();
            if (mname.Length < 3) mname.PadRight(3);
            if (mname.Length > 300) mname = mname.Substring(0, 300);
            return mname;
        }

        public List<AddOns> GetPAddInEditView(string MainType, string TableKey, string Type)
        {
            List<AddOns> objitemlist = new List<AddOns>();
            var addons = (from i in ctxTFAT.AddOns.Where(x => x.TableKey == "%@DSL" && x.Hide == false /*&& x.Types.Contains(Type)*/)
                          select i).OrderBy(x => new { x.Sno, x.Fld }).ToList();
            DataTable mdt = GetDataTable(@"Select  * from AddonItemSL where Tablekey = " + "'" + TableKey + "'" + "");
            foreach (var i in addons)
            {
                AddOns c = new AddOns();
                c.Fld = i.Fld;
                c.Head = i.Head;
                c.QueryText = string.IsNullOrEmpty(i.QueryText) ? "" : GetQueryTextAddon(i.QueryText);
                c.FldType = i.FldType;
                if (mdt != null && mdt.Rows.Count > 0)
                {
                    c.ApplCode = (string.IsNullOrEmpty(mdt.Rows[0][i.Fld].ToString())) ? " " : mdt.Rows[0][i.Fld].ToString();
                }
                else
                {
                    c.ApplCode = " ";
                }
                if (c.ApplCode == "") c.ApplCode = " ";// since blank is not allowed on gst portal
                c.PlaceValue = i.PlaceValue;
                c.Eqsn = i.Eqsn;
                c.TableKey = TableKey;
                objitemlist.Add(c);
            }
            return objitemlist;
        }

        public string DecodeBase64(string token)
        {
            var parts = token.Split('.');
            string header = parts[0];
            string payload = parts[1];
            var headerData = JObject.Parse(Base64Decode(header));
            var payloadData = JObject.Parse(Base64Decode(payload));
            return headerData.ToString() + payloadData.ToString();
        }

        public string Base64Decode(string data)
        {
            try
            {
                data = data.Replace('-', '+'); // 62nd char of encoding
                data = data.Replace('_', '/'); // 63rd char of encoding
                switch (data.Length % 4) // Pad with trailing '='s
                {
                    case 0: break; // No pad chars in this case
                    case 2: data += "=="; break; // Two pad chars
                    case 3: data += "="; break; // One pad char
                    default:
                        throw new System.Exception("Illegal base64url string!");
                }
                byte[] crypto = Convert.FromBase64String(data);
                return Encoding.UTF8.GetString(crypto);
            }
            catch (Exception e)
            {
                throw new Exception("Error in base64Decode" + e.Message);
            }
        }

        //private void DecodeQRCode(string mparentkey)
        //{
        //    string mqr = FOT("Select GSTQRCode from Sales Where Parentkey='" + mparentkey + "'");
        //    if (mqr == "") return;
        //    byte[] bitmap = GetQRCodeImage(mqr);
        //    string mpath = @"C:\TFATGSTeInvoice\QRCoded_" + mparentkey.Replace("\\", "").Replace("/", "") + ".jpeg";
        //    using (Image image = Image.FromStream(new MemoryStream(bitmap)))
        //    {
        //        image.Save(mpath, ImageFormat.Jpeg);
        //    }
        //    mqr = Decode(mqr);
        //    bitmap = GetQRCodeImage(mqr, ImageFormat.Jpeg);
        //    mpath = @"C:\TFATGSTeInvoice\QRSimple_" + mparentkey.Replace("\\", "").Replace("/", "") + ".jpeg";
        //    using (Image image = Image.FromStream(new MemoryStream(bitmap)))
        //    {
        //        image.Save(mpath, ImageFormat.Jpeg);
        //    }
        //    mpath = @"C:\TFATGSTeInvoice\QR_" + mparentkey.Replace("\\", "").Replace("/", "") + ".txt";
        //    if (File.Exists(mpath))
        //    {
        //        File.Delete(mpath);
        //    }
        //    File.WriteAllText(mpath, mqr);
        //}

        private byte[] GetQRCodeImage(string mcode)
        {
            //MemoryStream ms = new MemoryStream();
            //QRCodeGenerator qrGenerator = new QRCodeGenerator();
            //QRCodeGenerator.QRCode qrCode = qrGenerator.CreateQrCode(mcode, QRCodeGenerator.ECCLevel.Q);
            //Bitmap bitMap = qrCode.GetGraphic(20);
            //bitMap.Save(ms, ImageFormat.Jpeg);
            //byte[] fileBytes = Convert.FromBase64String(Convert.ToBase64String(ms.ToArray()));
            //bitMap.Dispose();
            //ms.Dispose();
            //return fileBytes;

            MemoryStream ms2 = new MemoryStream();
            QRCodeGenerator qrGenerator2 = new QRCodeGenerator();
            QRCodeData qrCodeData2 = qrGenerator2.CreateQrCode(mcode, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode2 = new QRCode(qrCodeData2);
            Bitmap bitMap2 = qrCode2.GetGraphic(20);
            bitMap2.Save(ms2, System.Drawing.Imaging.ImageFormat.Jpeg);//ImageFormat.Bmp
            byte[] fileBytes2 = Convert.FromBase64String(Convert.ToBase64String(ms2.ToArray()));
            bitMap2.Dispose();
            qrCodeData2.Dispose();
            qrGenerator2.Dispose();
            ms2.Dispose();
            return fileBytes2;
        }
    }
}