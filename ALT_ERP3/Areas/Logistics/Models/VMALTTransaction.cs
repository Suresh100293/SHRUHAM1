using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class VMALTTransaction
    {
        public long RECORDKEY { get; set; }

        public List<SelectListItem> Users { get; set; }
        public List<SelectListItem> Descriptions { get; set; }
        public List<SelectListItem> Brokers { get; set; }
        public List<SelectListItem> Vehicles { get; set; }
        public List<SelectListItem> Statuss { get; set; }
        public List<SelectListItem> LRExpenses { get; set; }
        public List<SelectListItem> ABExpenses { get; set; }
        public List<SelectListItem> Expenses { get; set; }
        public List<SelectListItem> Parties { get; set; }
        public List<SelectListItem> Vendors { get; set; }
        public List<SelectListItem> Branches { get; set; }

        public ConsignmentNotification consignmentNotification { get; set; }
        public LorryChallanNotification lorryChallanNotification { get; set; }
        public FreightMemoNotification freightMemoNotification { get; set; }
        public VehicleActivityNotification vehicleActivityNotification { get; set; }
        public DeliveryNotification deliveryNotification { get; set; }
        public PODNotification pODNotification { get; set; }
        public BillSubmissionNotification billSubmissionNotification { get; set; }
        public DocAuthenticateNotification docAuthenticateNotification { get; set; }

        //Transaction About Money
        public AdvBalPaymentNotification advBalPaymentNotification { get; set; }
        public BillNotification billNotification { get; set; }
        public CashBankPaymentNotification cashBankPaymentNotification { get; set; }
        public CashBankJVPaymentNotification cashBankJVPaymentNotification { get; set; }
        public CreditPurchaseNotification creditPurchaseNotification { get; set; }
        public CreditPaymentNotification creditPaymentNotification { get; set; }
        public BankReceiptNotification bankReceiptNotification { get; set; }
        public TripSheetNotification tripSheetNotification { get; set; }


        // iX9: Common default Fields
        public string Document { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Mode { get; set; }
        // iX9: used while passing parameters from MasterGrid to Option and Back 
        public string MainType { get; set; }
        public string Controller2 { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
    }

    #region Consignment Notification Models
    public class ConsignmentNotification
    {
        public TrnBackDaysConsignment trnBackDaysConsignment { get; set; }
        public TrnForwardDaysConsignment trnForwardDaysConsignment { get; set; }
        public TrnDeclareValueConsignment trnDeclareValueConsignment { get; set; }
        public TrnDescriptionConsignment trnDescriptionConsignment { get; set; }
        public TrnDeclareValueEwaybillConsignment trnDeclareValueEwaybillConsignment { get; set; }
    }
    public class TrnBackDaysConsignment
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int BackDays { get; set; }
        public string Description { get; } = "Send Notification if Consignment Book Date is less Than No Of Days.";
    }
    public class TrnForwardDaysConsignment
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int ForwardDays { get; set; }
        public string Description { get; } = "Send Notification if Consignment Book Date is more Than No Of Days.";
    }
    public class TrnDeclareValueConsignment
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public decimal DeclareValue { get; set; }
        public string Description { get; } = "Send Notification if Consignment Declare Value More Than Enterd Declare Value.";
    }
    public class TrnDescriptionConsignment
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string MDescription { get; set; }
        public string MDescriptionL { get; set; }
        public string Description { get; } = "Send Notification if Consignment inlude Particular selected Description.";
    }
    public class TrnDeclareValueEwaybillConsignment
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public decimal DeclareValue { get; set; }
        public string Description { get; } = "Send Notification if Consignment Declare Value More Than Enterd Declare Value and Eway Bill No Not Entered.";
    }
    #endregion

    #region Lorry Challan Notification Models
    public class LorryChallanNotification
    {
        public TrnBackDaysLorryChallan trnBackDaysLorryChallan { get; set; }
        public TrnForwardDaysLorryChallan trnForwardDaysLorryChallan { get; set; }
    }
    public class TrnBackDaysLorryChallan
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int BackDays { get; set; }
        public string Description { get; } = "Send Notification if Lorry Challan Date is less Than No Of Days.";
    }
    public class TrnForwardDaysLorryChallan
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int ForwardDays { get; set; }
        public string Description { get; } = "Send Notification if Lorry Challan Date is more Than No Of Days.";
    }
    #endregion

    #region Freight Memo Notification Models
    public class FreightMemoNotification
    {
        public TrnBackDaysFreightMemo trnBackDaysFreightMemo { get; set; }
        public TrnForwardDaysFreightMemo trnForwardDaysFreightMemo { get; set; }
        public TrnDriverLicExpFreightMemo trnDriverLicExpFreightMemo { get; set; }
        public TrnDocAmtFreightMemo trnDocAmtFreightMemo { get; set; }
        public TrnBrokerFreightMemo trnBrokerFreightMemo { get; set; }
        public TrnVehicleFreightMemo trnVehicleFreightMemo { get; set; }
    }
    public class TrnBackDaysFreightMemo
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int BackDays { get; set; }
        public string Description { get; } = "Send Notification if Freight Memo Date is less Than No Of Days.";        
    }
    public class TrnForwardDaysFreightMemo
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int ForwardDays { get; set; }
        public string Description { get; } = "Send Notification if Freight Memo Date is more Than No Of Days.";
    }
    public class TrnDriverLicExpFreightMemo
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Description { get; } = "Send Notification if Drivers License Exired In Freight Memo.";
    }
    public class TrnDocAmtFreightMemo
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public decimal DocAmount { get; set; }
        public string Description { get; } = "Send Notification if Freight Memo Freight More Than Enterd Freight Value.";
    }
    public class TrnBrokerFreightMemo
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Broker { get; set; }
        public string BrokerL { get; set; }
        public string Description { get; } = "Send Notification if Freight Memo inlude Particular selected Broker."; 
    }
    public class TrnVehicleFreightMemo
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Vehicle { get; set; }
        public string VehicleL { get; set; }
        public string Description { get; } = "Send Notification if Freight Memo inlude Particular selected Vehicle.";
    }
    #endregion

    #region VehicleActivity Notification Models
    public class VehicleActivityNotification
    {
        public TrnArrivalDaysVehicleActivity trnArrivalDaysVehicleActivity { get; set; }
        public TrnDispatchDaysVehicleActivity trnDispatchDaysVehicleActivity { get; set; }
        public TrnOverloadVehicleActivity trnOverloadVehicleActivity { get; set; }
        public TrnClearVehicleActivity trnClearVehicleActivity { get; set; }
        public TrnUnloadVehicleActivity trnUnloadVehicleActivity { get; set; }

    }
    public class TrnArrivalDaysVehicleActivity
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string ArrivalDays { get; set; }
        public string Description { get; } = "Send Notification if Vehicle Reached In Branch Late Upto Entered HH:MM.";
    }
    public class TrnDispatchDaysVehicleActivity
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string DispatchDays { get; set; }
        public string Description { get; } = "Send Notification if Vehicle Out From Branch Late Upto Entered HH:MM.";
    }
    public class TrnOverloadVehicleActivity
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int OverloadinKg { get; set; }
        public string Description { get; } = "Send Notification if Loading Weight Is Greter Than Entered Overload Weight."; 
    }
    public class TrnClearVehicleActivity
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Description { get; } = "Send Notification if Own Branch Material Not Unloading.";
    }
    public class TrnUnloadVehicleActivity
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Description { get; } = "Send Notification if Unload Other Branch Material.";
    }
    #endregion

    #region Delivery Notification Models
    public class DeliveryNotification
    {
        public TrnStatusDelivery trnStatusDelivery { get; set; }
        public TrnAnotherBranchDelivery trnAnotherBranchDelivery { get; set; }
    }
    public class TrnStatusDelivery
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Status { get; set; }
        public string StatusL { get; set; }
        public string Description { get; } = "Send Notification if Delivery inlude Particular selected Status.";
    }
    public class TrnAnotherBranchDelivery
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Description { get; } = "Send Notification if Delivered Another Branch Material.";
    }
    #endregion

    #region POD Notification Models
    public class PODNotification
    {
        public TrnBackDaysPOD trnBackDaysPOD { get; set; }
        public TrnForwardDaysPOD trnForwardDaysPOD { get; set; }
        public TrnReceivedDaysPOD trnReceivedDaysPOD { get; set; }
        public TrnSendDaysPOD trnSendDaysPOD { get; set; }
        public TrnSelectNoDeliveryPOD trnSelectNoDeliveryPOD { get; set; }
    }
    public class TrnBackDaysPOD
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int BackDays { get; set; }
        public string Description { get; } = "Send Notification if POD Date is less Than No Of Days.";
    }
    public class TrnForwardDaysPOD
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int ForwardDays { get; set; }
        public string Description { get; } = "Send Notification if POD Date is more Than No Of Days.";  
    }
    public class TrnReceivedDaysPOD
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int ReceivedDays { get; set; }
        public string Description { get; } = "Send Notification if POD Received Late As Compared To Entered Days."; 
    }
    public class TrnSendDaysPOD
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int SendDays { get; set; }
        public string Description { get; } = "Send Notification if POD Send Late As Compared To Entered Days."; 
    }
    public class TrnSelectNoDeliveryPOD
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Description { get; } = "Send Notification if Without Delivery POD Received.";
    }
    #endregion

    #region BillSubmission Notification Models
    public class BillSubmissionNotification
    {
        public TrnBackDaysBillSubmission trnBackDaysBillSubmission { get; set; }
        public TrnForwardDaysBillSubmission trnForwardDaysBillSubmission { get; set; }
        public TrnLateDaysBillSubmission trnLateDaysBillSubmission { get; set; }
    }
    public class TrnBackDaysBillSubmission
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int BackDays { get; set; }
        public string Description { get; } = "Send Notification if Bill Submission Date is less Than No Of Days."; 
    }
    public class TrnForwardDaysBillSubmission
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int ForwardDays { get; set; }
        public string Description { get; } = "Send Notification if Bill Submission Date is more Than No Of Days."; 
    }
    public class TrnLateDaysBillSubmission
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int LateDays { get; set; }
        public string Description { get; } = "Send Notification if Bill Submission Late As Compared (Bill Docdate) To Entered Days."; 
    }
    #endregion

    #region DocAuthenticate Notification Models
    public class DocAuthenticateNotification
    {
        public TrnAuthenticateDaysDocAuthenticate trnAuthenticateDaysDocAuthenticate { get; set; }
    }
    public class TrnAuthenticateDaysDocAuthenticate
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int AuthDays { get; set; }
        public string Description { get; } = "Send Notification if Document Authenticate Late As Compared To Entered Days.";
    }
    #endregion

    #region Advance-BalancePayment Notification Models
    public class AdvBalPaymentNotification
    {
        public TrnBackDaysAdvBalPayment trnBackDaysAdvBalPayment { get; set; }
        public TrnForwardDaysAdvBalPayment trnForwardDaysAdvBalPayment { get; set; }
        public TrnExpensesAdvBalPayment trnExpensesAdvBalPayment { get; set; }
        public TrnDoubleExpAdvBalPayment trnDoubleExpAdvBalPayment { get; set; }
        public TrnParticularDoubleExpAdvBalPayment trnParticularDoubleExpAdvBalPayment { get; set; }
    }
    public class TrnBackDaysAdvBalPayment
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int BackDays { get; set; }
        public string Description { get; } = "Send Notification if Advance-Balance Payment Date is less Than No Of Days.";
    }
    public class TrnForwardDaysAdvBalPayment
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int ForwardDays { get; set; }
        public string Description { get; } = "Send Notification if Advance-Balance Payment Date is more Than No Of Days."; 
    }
    public class TrnExpensesAdvBalPayment
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Expense { get; set; }
        public string ExpenseL { get; set; }
        public string Description { get; } = "Send Notification if Advance-Balance Payment inlude Particular selected Charges.";  
    }
    public class TrnDoubleExpAdvBalPayment
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Description { get; } = "Send Notification if Double Expenses Found ON Consigmnet.";
    }
    public class TrnParticularDoubleExpAdvBalPayment
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Expense { get; set; }
        public string ExpenseL { get; set; }
        public string Description { get; } = "Send Notification if Double Expenses Found On Particular selected Expenses.";  
    }
    #endregion

    #region Bill Notification Models
    public class BillNotification
    {
        public TrnBackDaysBill trnBackDaysBill { get; set; }
        public TrnForwardDaysBill trnForwardDaysBill { get; set; }
        public TrnDocAmountBill trnDocAmountBill { get; set; }
        public TrnZeroAmountBill trnZeroAmountBill { get; set; }
        public TrnOtherPartyBill trnOtherPartyBill { get; set; }
        public TrnConsignmentBill trnConsignmentBill { get; set; }
        public TrnPartyBill trnPartyBill { get; set; }
        public TrnDoubleExpBill trnDoubleExpBill { get; set; }
        public TrnParticularDoubleExpBill trnParticularDoubleExpBill { get; set; }
    }
    public class TrnBackDaysBill
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int BackDays { get; set; }
        public string Description { get; } = "Send Notification if Invoice Date is less Than No Of Days."; 
    }
    public class TrnForwardDaysBill
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int ForwardDays { get; set; }
        public string Description { get; } = "Send Notification if Invoice Date is more Than No Of Days.";  
    }
    public class TrnDocAmountBill
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public decimal DocAmount { get; set; }
        public string Description { get; } = "Send Notification if Invoice Amount More Than Enterd Doc Amount."; 
    }
    public class TrnZeroAmountBill
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Description { get; } = "Send Notification if Invoice Amount Is Zero."; 
    }
    public class TrnOtherPartyBill
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Description { get; } = "Send Notification if Invoice Generate On Another Party.";
    }
    public class TrnConsignmentBill
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int Days { get; set; }
        public string Description { get; } = "Send Notification if Difference Between Invoice Date And Consignment Date Is Greater Than Entered Days."; 
    }
    public class TrnPartyBill
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Party { get; set; }
        public string PartyL { get; set; }
        public string Description { get; } = "Send Notification if Invoice inlude Particular selected Party."; 
    }
    public class TrnDoubleExpBill
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Description { get; } = "Send Notification if Double Expenses Found ON Consigmnet.";
    }
    public class TrnParticularDoubleExpBill
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Expense { get; set; }
        public string ExpenseL { get; set; }
        public string Description { get; } = "Send Notification if Double Expenses Found On Particular selected Expenses.";
    }
    #endregion

    #region Cash-Bank Transaction Notification Models
    public class CashBankPaymentNotification
    {
        public TrnBackDaysCashBank trnBackDaysCashBank { get; set; }
        public TrnForwardDaysCashBank trnForwardDaysCashBank { get; set; }
        public TrnDocAmountCashBank trnDocAmountCashBank { get; set; }
        public TrnDoubleExpCashBank trnDoubleExpCashBank { get; set; }
        public TrnParticularDoubleExpCashBank trnParticularDoubleExpCashBank { get; set; }
    }
    public class TrnBackDaysCashBank
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int BackDays { get; set; }
        public string Description { get; } = "Send Notification if CashBank Transaction Date is less Than No Of Days.";
    }
    public class TrnForwardDaysCashBank
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int ForwardDays { get; set; }
        public string Description { get; } = "Send Notification if CashBank Transaction Date is more Than No Of Days.";  
    }
    public class TrnDocAmountCashBank
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public decimal DocAmount { get; set; }
        public string Description { get; } = "Send Notification if CashBank Transaction More Than Enterd Doc Amount.";
    }
    public class TrnDoubleExpCashBank
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Description { get; } = "Send Notification if Double Expenses Found ON Consigmnet.";
    }
    public class TrnParticularDoubleExpCashBank
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Expense { get; set; }
        public string ExpenseL { get; set; }
        public string Description { get; } = "Send Notification if Double Expenses Found On Particular selected Expenses.";
    }
    #endregion

    #region Cash-Bank JV Transaction Notification Models
    public class CashBankJVPaymentNotification
    {
        public TrnBackDaysCashBankJV trnBackDaysCashBankJV { get; set; }
        public TrnForwardDaysCashBankJV trnForwardDaysCashBankJV { get; set; }
        public TrnDocAmountCashBankJV trnDocAmountCashBankJV { get; set; }
        public TrnDoubleExpCashBankJV trnDoubleExpCashBankJV { get; set; }
        public TrnParticularDoubleExpCashBankJV trnParticularDoubleExpCashBankJV { get; set; }
    }
    public class TrnBackDaysCashBankJV
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int BackDays { get; set; }
        public string Description { get; } = "Send Notification if CashBank Transaction JV Date is less Than No Of Days."; 
    }
    public class TrnForwardDaysCashBankJV
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int ForwardDays { get; set; }
        public string Description { get; } = "Send Notification if CashBank JV Transaction Date is more Than No Of Days.";
    }
    public class TrnDocAmountCashBankJV
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public decimal DocAmount { get; set; }
        public string Description { get; } = "Send Notification if CashBank JV Transaction Amount More Than Enterd Doc Amount.";
    }
    public class TrnDoubleExpCashBankJV
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Description { get; } = "Send Notification if Double Expenses Found ON Consigmnet.";
    }
    public class TrnParticularDoubleExpCashBankJV
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Expense { get; set; }
        public string ExpenseL { get; set; }
        public string Description { get; } = "Send Notification if Double Expenses Found On Particular selected Expenses.";
    }
    #endregion

    #region Credit Purchase Transaction Notification Models
    public class CreditPurchaseNotification
    {
        public TrnBackDaysCreditPurchase trnBackDaysCreditPurchase { get; set; }
        public TrnForwardDaysCreditPurchase trnForwardDaysCreditPurchase { get; set; }
        public TrnDocAmountCreditPurchase trnDocAmountCreditPurchase { get; set; }
        public TrnVendorCreditPurchase trnVendorCreditPurchase { get; set; }
        public TrnDoubleExpPurchase trnDoubleExpPurchase { get; set; }
        public TrnParticularDoubleExpPurchase trnParticularDoubleExpPurchase { get; set; }
        
    }
    public class TrnBackDaysCreditPurchase
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int BackDays { get; set; }
        public string Description { get; } = "Send Notification if Credit Purchase Date is less Than No Of Days.";
    }
    public class TrnForwardDaysCreditPurchase
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int ForwardDays { get; set; }
        public string Description { get; } = "Send Notification if Credit Purchase Date is more Than No Of Days.";
    }
    public class TrnDocAmountCreditPurchase
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public decimal DocAmount { get; set; }
        public string Description { get; } = "Send Notification if Credit Purchase Amount More Than Enterd Doc Amount.";
    }
    public class TrnVendorCreditPurchase
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Vendor { get; set; }
        public string VendorL { get; set; }
        public string Description { get; } = "Send Notification if Credit Purchase inlude Particular selected Vendors.";
    }
    public class TrnDoubleExpPurchase
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Description { get; } = "Send Notification if Double Expenses Found ON Consigmnet.";
    }
    public class TrnParticularDoubleExpPurchase
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Expense { get; set; }
        public string ExpenseL { get; set; }
        public string Description { get; } = "Send Notification if Double Expenses Found On Particular selected Expenses.";
    }
    #endregion

    #region Credit Payment Transaction Notification Models
    public class CreditPaymentNotification
    {
        public TrnBackDaysCreditPayment trnBackDaysCreditPayment { get; set; }
        public TrnForwardDaysCreditPayment trnForwardDaysCreditPayment { get; set; }
        public TrnDocAmountCreditPayment trnDocAmountCreditPayment { get; set; }
        public TrnVendorCreditPayment trnVendorCreditPayment { get; set; }
    }
    public class TrnBackDaysCreditPayment
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int BackDays { get; set; }
        public string Description { get; } = "Send Notification if Creditor Payment Date is less Than No Of Days.";
    }
    public class TrnForwardDaysCreditPayment
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int ForwardDays { get; set; }
        public string Description { get; } = "Send Notification if Creditor Payment Date is more Than No Of Days.";
    }
    public class TrnDocAmountCreditPayment
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public decimal DocAmount { get; set; }
        public string Description { get; } = "Send Notification if Creditor Payment Amount More Than Enterd Doc Amount.";
    }
    public class TrnVendorCreditPayment
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Vendor { get; set; }
        public string VendorL { get; set; }
        public string Description { get; } = "Send Notification if Creditor Payment inlude Particular selected Vendors.";
    }
    #endregion

    #region Bank Receipt Transaction Notification Models
    public class BankReceiptNotification
    {
        public TrnBackDaysBankReceipt trnBackDaysBankReceipt { get; set; }
        public TrnForwardDaysBankReceipt trnForwardDaysBankReceipt { get; set; }
        public TrnDocAmountBankReceipt trnDocAmountBankReceipt { get; set; }
        public TrnPendingAdjustBankReceipt trnPendingAdjustBankReceipt { get; set; }
        public TrnFreightRebateAmountBankReceipt trnFreightRebateAmountBankReceipt { get; set; }
    }
    public class TrnBackDaysBankReceipt
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int BackDays { get; set; }
        public string Description { get; } = "Send Notification if Bank Receipt Date is less Than No Of Days."; 
    }
    public class TrnForwardDaysBankReceipt
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int ForwardDays { get; set; }
        public string Description { get; } = "Send Notification if Bank Receipt Date is more Than No Of Days.";
    }
    public class TrnDocAmountBankReceipt
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public decimal DocAmount { get; set; }
        public string Description { get; } = "Send Notification if Bank Receipt Amount More Than Enterd Doc Amount.";
    }
    public class TrnPendingAdjustBankReceipt
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Description { get; } = "Send Notification if Bank Receipt Amount Not Adjust Properly.";
    }
    public class TrnFreightRebateAmountBankReceipt
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public decimal FreightRebateAmount { get; set; }
        public string Description { get; } = "Send Notification if Bank Receipt FreightRebate Amount More Than Enterd Freight Rebate Amount.";
    }
    #endregion

    #region Trip Sheet Transaction Notification Models
    public class TripSheetNotification
    {
        public TrnBackDaysTripSheet trnBackDaysTripSheet { get; set; }
        public TrnForwardDaysTripSheet trnForwardDaysTripSheet { get; set; }
        public TrnDocAmountTripSheet trnDocAmountTripSheet { get; set; }
        public TrnEtraExpTripSheet trnEtraExpTripSheet { get; set; }
        public TrnFmDateRangeTripSheet trnFmDateRangeTripSheet { get; set; }
        public TrnADVDateRangeTripSheet trnADVDateRangeTripSheet { get; set; }
        public TrnCCDateRangeTripSheet trnCCDateRangeTripSheet { get; set; }
        public TrnDoubleExpTripSheet trnDoubleExpTripSheet { get; set; }
        public TrnParticularDoubleExpTripSheet trnParticularDoubleExpTripSheet { get; set; }
    }
    public class TrnBackDaysTripSheet
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int BackDays { get; set; }
        public string Description { get; } = "Send Notification if Trip Sheet Date is less Than No Of Days.";
    }
    public class TrnForwardDaysTripSheet
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public int ForwardDays { get; set; }
        public string Description { get; } = "Send Notification if Trip Sheet Date is more Than No Of Days.";
    }
    public class TrnDocAmountTripSheet
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public decimal DocAmount { get; set; }
        public string Description { get; } = "Send Notification if Trip Sheet Amount More Than Enterd Doc Amount.";
    }
    public class TrnEtraExpTripSheet
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public decimal ExtraExp { get; set; }
        public string Description { get; } = "Send Notification if Trip Sheet Other Expenses Amount More Than Enterd ExtraExp Amount.";
    }
    public class TrnFmDateRangeTripSheet
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Description { get; } = "Send Notification if Trip Sheet Docdate Is Less Than Freight Memo Filter Date Range.";
    }
    public class TrnADVDateRangeTripSheet
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Description { get; } = "Send Notification if Trip Sheet Docdate Is Less Than Advance Adjust Date Range.";
    }
    public class TrnCCDateRangeTripSheet
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Description { get; } = "Send Notification if Trip Sheet Docdate Is Less Than Cost Center Adjust Date Range.";
    }
    public class TrnDoubleExpTripSheet
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Description { get; } = "Send Notification if Double Expenses Found ON Consigmnet.";
    }
    public class TrnParticularDoubleExpTripSheet
    {
        //Common Properties
        public bool Nofitication { get; set; }
        public bool Email { get; set; }
        public bool Priority { get; set; }
        public bool EnteredBy { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public string Expense { get; set; }
        public string ExpenseL { get; set; }
        public string Description { get; } = "Send Notification if Double Expenses Found On Particular selected Expenses.";
    }
    #endregion
}





