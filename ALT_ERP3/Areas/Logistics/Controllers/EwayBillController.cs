using ALT_ERP3.Controllers;
using EntitiModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class EwayBillController : BaseController
    {
        private static string memail = "";
        private static string musername = "";
        private static string mpassword = "";
        private static string mclientid = "";
        private static string mclientsecret = "";
        private static string mgstin = "";
        private static string mipaddress = "223.177.54.203";
        private static string mauthorise = "A00";

        private static string weburl = "https://api.mastergst.com/";
        private static string authenticateurl = "ewaybillapi/v1.03/authenticate";

        private static string GetEwayBillDetails = "ewaybillapi/v1.03/ewayapi/getewaybill";
        private static string GenerateConsoleeway = "ewaybillapi/v1.03/ewayapi/gencewb";

        public EwayBillController()
        {

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
                TfatEway tfatEWB = ctxTFAT.TfatEway.Where(x => x.Type == "AUTH0").FirstOrDefault();
                if (tfatEWB == null)
                {
                    GetAuthentication = true;
                }
                else
                {
                    DateTime CurrentDate1 = DateTime.Now;
                    DateTime authValid = tfatEWB.validUpto.Value;
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
                    SaveEWBAuth("AUTH0", "000000", dateTime);
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
        public void SaveEWBAuth(string DocType, string EWBNO, DateTime EWBValid)
        {
            bool mAdd = false;
            TfatEway tfatEWB = ctxTFAT.TfatEway.Where(x => x.Type == DocType).FirstOrDefault();
            if (tfatEWB == null)
            {
                tfatEWB = new TfatEway();
                mAdd = true;
            }
            if (mAdd)
            {
                tfatEWB.Branch = mbranchcode;
                tfatEWB.Type = DocType;
                tfatEWB.Prefix = mperiod;
                tfatEWB.ewbNo = Convert.ToInt64(EWBNO);
                tfatEWB.ewbDate = EWBValid;
                tfatEWB.status = "";
                tfatEWB.genGstin = "";
                tfatEWB.docNo = "";
                tfatEWB.docDate = DateTime.Now;
                tfatEWB.delPinCode = "";
                tfatEWB.delStateCode = "";
                tfatEWB.delPlace = "";
                tfatEWB.validUpto = EWBValid;
                tfatEWB.extendedTimes = "";
                tfatEWB.rejectStatus = "";
                tfatEWB.GenOnPortal = false;
                tfatEWB.RefTablekey = "";

                tfatEWB.AUTHIDS = muserid;
                tfatEWB.AUTHORISE = "A00";
                tfatEWB.ENTEREDBY = muserid;
                tfatEWB.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                ctxTFAT.TfatEway.Add(tfatEWB);
                ctxTFAT.SaveChanges();
            }
            else
            {
                tfatEWB.validUpto = EWBValid;
                ctxTFAT.Entry(tfatEWB).State = EntityState.Modified;
                ctxTFAT.SaveChanges();
            }
        }
        public void SaveEWBLog(string Type, string Description, string Msg, string EWBNO, string Remark, string Mode, string Docno)
        {
            EwayBillLog eWBLog = new EwayBillLog();
            eWBLog.Branch = mbranchcode;
            eWBLog.Docdate = DateTime.Now;
            eWBLog.Mode = Mode;
            eWBLog.Type = Type;
            eWBLog.DocNo = Docno;
            eWBLog.Description = Description;
            eWBLog.Msg = Msg;
            eWBLog.EWBNO = EWBNO;
            eWBLog.Remark = Remark;
            eWBLog.AUTHIDS = muserid;
            eWBLog.AUTHORISE = "A00";
            eWBLog.ENTEREDBY = muserid;
            eWBLog.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
            ctxTFAT.EwayBillLog.Add(eWBLog);
            ctxTFAT.SaveChanges();
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

        //Get Details EwayBill At Consingmnet Creatation.
        public string GetEwayBillDetail(string EwayNo, string ConsignmentKey, string Mode, string DocNo)
        {
            string Status = "Sucess";
            try
            {
                var ewayBills = ctxTFAT.TfatEway.Where(x => x.RefTablekey != null).ToList();
                var OldEwayBilllist = ewayBills.Where(x => x.RefTablekey.Split(',').Any(key => key.Trim() == ConsignmentKey)).ToList();
                foreach (var item in OldEwayBilllist)
                {
                    if (item.MultiEWB)
                    {
                        var List = item.RefTablekey.Split(',').ToList();
                        item.RefTablekey = "";
                        foreach (var key in List)
                        {
                            if (key != ConsignmentKey)
                            {
                                item.RefTablekey += key + ",";
                            }
                        }
                        item.RefTablekey = item.RefTablekey.Substring(0, item.RefTablekey.Length - 1);
                        if (item.RefTablekey.Split(',').ToList().Count() <= 1)
                        {
                            item.Stage = "";
                            item.MultiEWB = false;
                        }
                        ctxTFAT.Entry(item).State = EntityState.Modified;
                        SaveEWBLog("LR000", "Update MultVehicle Eway Bill", "Sucess", item.ewbNo.ToString(), "", Mode, DocNo);
                    }
                    else
                    {
                        SaveEWBLog("LR000", "Delete Eway Bill", "Sucess", item.ewbNo.ToString(), "", Mode, DocNo);
                        ctxTFAT.TfatEway.Remove(item);
                    }
                }


                if (!String.IsNullOrEmpty(EwayNo) && Mode != "Delete")
                {
                    var TfatCompErr = SetApiParameters();
                    if (String.IsNullOrEmpty(TfatCompErr))
                    {
                        string mtoken = GetToken();
                        if (mtoken != "0")
                        {
                            var URL = new UriBuilder(weburl + GetEwayBillDetails);
                            var queryString = HttpUtility.ParseQueryString(string.Empty);
                            queryString["email"] = memail;
                            queryString["ewbNo"] = EwayNo.Trim();
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
                                dynamic result = JsonConvert.DeserializeObject(mstr);
                                var VehicleDetails = result.data.VehiclListDetails;
                                TfatEway tfatEWB = ctxTFAT.TfatEway.Where(x => x.ewbNo.ToString() == EwayNo).FirstOrDefault();
                                if (tfatEWB == null)
                                {
                                    tfatEWB = new TfatEway();
                                    tfatEWB.Branch = mbranchcode;
                                    tfatEWB.Type = "Fetch";
                                    tfatEWB.Prefix = mperiod;
                                    tfatEWB.ewbNo = Convert.ToInt64(result.data.ewbNo);
                                    var Date = result.data.ewayBillDate.ToString();
                                    tfatEWB.ewbDate = DateTime.ParseExact(Date, "dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                                    tfatEWB.status = result.data.status;
                                    tfatEWB.genGstin = result.data.userGstin;
                                    tfatEWB.docNo = result.data.docNo;
                                    Date = result.data.docDate.ToString();
                                    tfatEWB.docDate = DateTime.ParseExact(Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                    tfatEWB.delPinCode = result.data.toPincode;
                                    tfatEWB.delStateCode = result.data.toStateCode;
                                    tfatEWB.delPlace = result.data.toPlace;
                                    Date = result.data.validUpto.ToString();
                                    tfatEWB.validUpto = DateTime.ParseExact(Date, "dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                                    tfatEWB.extendedTimes = result.data.extendedTimes;
                                    tfatEWB.rejectStatus = result.data.rejectStatus;
                                    tfatEWB.GenOnPortal = true;
                                    tfatEWB.RefTablekey = ConsignmentKey;
                                    tfatEWB.MultiEWB = false;
                                    tfatEWB.Stage = "";

                                    tfatEWB.AUTHIDS = muserid;
                                    tfatEWB.AUTHORISE = "A00";
                                    tfatEWB.ENTEREDBY = muserid;
                                    tfatEWB.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                                    ctxTFAT.TfatEway.Add(tfatEWB);
                                }
                                else
                                {
                                    tfatEWB.RefTablekey += "," + ConsignmentKey;
                                    tfatEWB.MultiEWB = true;
                                    tfatEWB.Stage = "B";
                                    tfatEWB.AUTHIDS = muserid;
                                    tfatEWB.AUTHORISE = "A00";
                                    tfatEWB.ENTEREDBY = muserid;
                                    tfatEWB.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                                    ctxTFAT.Entry(tfatEWB).State = EntityState.Modified;
                                }


                                SaveEWBLog("LR000", "Fetching Eway Bill", "Sucess", EwayNo.ToString(), "", Mode, DocNo);
                            }
                            else
                            {
                                dynamic irnresponseData = JValue.Parse(mjson.GetValue("error").ToString());
                                dynamic ErrorMsh = JValue.Parse(irnresponseData.GetValue("message").ToString());
                                var ErrorCodeList = ErrorMsh.GetValue("errorCodes").ToString();
                                var errormsg = GetErrorString(ErrorCodeList);
                                SaveEWBLog("LR000", "Fetching Eway Bill", errormsg, EwayNo.ToString(), "", Mode, DocNo);
                                Status = EwayNo + " Is Not Valid...!\n Please Enter Proper Eway Bill-No...!";
                            }
                        }
                        else
                        {
                            Status = "Authentication Issue...!";
                        }
                    }
                    else
                    {
                        Status = TfatCompErr;
                    }
                }
            }
            catch (Exception ex)
            {

                Status = "EwayBill Details Fetching Issue...!";
            }
            ctxTFAT.SaveChanges();
            return Status;

        }

        //Generate Console EWB Through Loading.
        public string GenerateConsole(List<string> Consignments, string Type, string Vehicle)
        {
            string Status = string.Empty;
            Status = SetApiParameters();
            if (!String.IsNullOrEmpty(Status))
            {
                return Status;
            }
            FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey == Vehicle).FirstOrDefault();
            TfatBranch fromplace = ctxTFAT.TfatBranch.Where(x => x.Code == fMMaster.FromBranch).FirstOrDefault();
            TfatState state = ctxTFAT.TfatState.Where(x => x.Code.ToString() == fromplace.State).FirstOrDefault();
            if (fMMaster != null && fromplace != null && state != null)
            {
                HireVehicleMaster hireVehicle = ctxTFAT.HireVehicleMaster.Where(x => x.Code == fMMaster.TruckNo).FirstOrDefault();
                VehicleMaster vehicle = ctxTFAT.VehicleMaster.Where(x => x.Code == fMMaster.TruckNo).FirstOrDefault();
                string VehicleNo = string.Empty;
                if (hireVehicle != null)
                {
                    VehicleNo = Regex.Replace(hireVehicle.TruckNo, @"\s", "");
                }
                else if (vehicle != null)
                {
                    VehicleNo = Regex.Replace(vehicle.TruckNo, @"\s", "");
                }

                foreach (var item in Consignments)
                {
                    StringBuilder jsonBuilder = new StringBuilder();
                    var lcdetails = ctxTFAT.LCDetail.Where(x => x.LCRefTablekey == item).ToList();
                    var Consignmentkeys = lcdetails.Select(x => x.LRRefTablekey).ToList();
                    var ewbs = ctxTFAT.TfatEway.Where(x => Consignmentkeys.Any(y => y == x.RefTablekey)).Select(x => x).ToList();
                    if (ewbs.Count() > 0)
                    {
                        jsonBuilder.Append("{");
                        jsonBuilder.Append("\"fromPlace\":\"" + fromplace.Name + "\",");
                        jsonBuilder.Append("\"fromState\":" + state.StateCode + ",");
                        jsonBuilder.Append("\"vehicleNo\":\"" + VehicleNo + "\",");
                        jsonBuilder.Append("\"transMode\":\"" + 1 + "\",");
                        jsonBuilder.Append("\"transDocNo\":\"" + fMMaster.FmNo + "\",");
                        jsonBuilder.Append("\"transDocDate\":\"" + fMMaster.Date.ToShortDateString() + "\",");
                        jsonBuilder.Append("\"tripSheetEwbBills\":[");
                        foreach (var ewb in ewbs)
                        {
                            jsonBuilder.Append("{");
                            jsonBuilder.Append("\"ewbNo\":" + ewb.ewbNo);
                            jsonBuilder.Append("},");
                        }
                        jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                        jsonBuilder.Append("]},");
                        jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                    }

                    Status = GetToken();
                    if (Status != "0")
                    {
                        JObject mjson = JObject.Parse(jsonBuilder.ToString());
                        RestClient client = new RestClient(weburl + GenerateConsoleeway);
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
                            var ConsoleEway = irnresponseData.GetValue("cEwbNo").ToString();
                            var EwatValidUpto = irnresponseData.GetValue("cEwbDate").ToString();
                            var Valid = DateTime.ParseExact(EwatValidUpto, "dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                            Status += "Generate Console using Challan No Is " + lcdetails.Select(x => x.LCno).FirstOrDefault() + ".Console Eway-No :" + ConsoleEway + "\n";
                            foreach (var Ewb in ewbs)
                            {
                                EWBVehicleDetails eWBVehicle = new EWBVehicleDetails();
                                eWBVehicle.Branch = mbranchcode;
                                eWBVehicle.Docdate = fMMaster.Date;
                                eWBVehicle.EWBNO = Ewb.ewbNo.ToString();
                                eWBVehicle.Description = "Generate Console";
                                eWBVehicle.LRTablekey = Ewb.RefTablekey;
                                eWBVehicle.LCTablekey = item;
                                eWBVehicle.FMTablekey = fMMaster.TableKey;
                                eWBVehicle.ValidUpto = Valid;
                                eWBVehicle.ConsoleEWBNo = ConsoleEway;
                                eWBVehicle.AUTHIDS = muserid;
                                eWBVehicle.AUTHORISE = "A00";
                                eWBVehicle.ENTEREDBY = muserid;
                                eWBVehicle.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                                ctxTFAT.EWBVehicleDetails.Add(eWBVehicle);
                                Ewb.validUpto = Valid;
                                ctxTFAT.Entry(Ewb).State = EntityState.Modified;
                                ctxTFAT.SaveChanges();

                                SaveEWBLog("LR000", "Generate Console", "Generate Console Eway Bill-No :" + ConsoleEway,
                                Ewb.ewbNo.ToString(), "Loading Challan Is :" + lcdetails.Select(x => x.LCno).FirstOrDefault(), "Loading", (ctxTFAT.LRMaster.Where(x => x.TableKey == Ewb.RefTablekey).Select(x => x.LrNo).FirstOrDefault().ToString()));
                            }
                        }
                        else
                        {
                            dynamic irnresponseData = JValue.Parse(irnresponseorder.GetValue("error").ToString());
                            dynamic ErrorMsh = JValue.Parse(irnresponseData.GetValue("message").ToString());
                            var ErrorCodeList = ErrorMsh.GetValue("errorCodes").ToString();
                            var errormsg = GetErrorString(ErrorCodeList);
                            Status += errormsg;
                            SaveEWBLog("LC000", "Generate Console", errormsg, "", "Loading Challan Is :" + lcdetails.Select(x => x.LCno).FirstOrDefault(), "Loading", "");
                        }
                    }
                    else
                    {
                        return "Authentication Issue...!";
                    }
                }
            }
            return Status;
        }

        // GET: Logistics/EwayBill
        public ActionResult Index()
        {
            TfatEway tfatEway = new TfatEway();
            return View();
        }




    }
}