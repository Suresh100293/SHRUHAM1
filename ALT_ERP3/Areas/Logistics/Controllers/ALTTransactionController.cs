using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class ALTTransactionController : BaseController
    {
        private static string mauthorise = "A00";

        private List<SelectListItem> PopulateUsers()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "select Code,Name from TfatPass where Locked='false' order by Recordkey ";
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

        private List<SelectListItem> PopulateDescriptions()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "select Code,Description from DescriptionMaster order by Description ";
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
                                Text = sdr["Description"].ToString(),
                                Value = sdr["Code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return items;
        }

        private List<SelectListItem> PopulateTruckNo()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code,TruckNo,TruckStatus FROM VehicleMaster order by Recordkey ";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            string Flag = sdr["TruckStatus"].ToString().Trim() == "100000" ? " - A" : " - O";
                            items.Add(new SelectListItem
                            {
                                Text = sdr["TruckNo"].ToString() + Flag,
                                Value = sdr["Code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code,TruckNo FROM HireVehicleMaster  order by Recordkey ";
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
                                Text = sdr["TruckNo"].ToString() + " - H",
                                Value = sdr["Code"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }

            return items;
        }

        private List<SelectListItem> PopulateBroker()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM Master where OthPostType like '%B%'  order by Recordkey ";
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

        private List<SelectListItem> PopulateDeliveryStatus()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem
            {
                Text = "OK",
                Value = "OK"
            });
            items.Add(new SelectListItem
            {
                Text = "Package Damage",
                Value = "Package Damage"
            });
            items.Add(new SelectListItem
            {
                Text = "Material Damage",
                Value = "Material Damage"
            });
            items.Add(new SelectListItem
            {
                Text = "Short",
                Value = "Short"
            });
            return items;
        }

        private List<SelectListItem> PopulateExpenses()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM Master order by Recordkey ";
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

        private List<SelectListItem> PopulateLRExpenses()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM Master where RelatedTo ='LR' order by Name ";
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

        private List<SelectListItem> PopulateABExpenses()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM Master where code in (select C.Code from Charges C where C.Type='fmp00' and C.DontUse=0) order by Name ";
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

        private List<SelectListItem> PopulateParties()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM CustomerMaster order by Recordkey ";
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

        private List<SelectListItem> PopulateVendor()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM Master where BaseGr ='U' or  BaseGr ='S'  order by Recordkey ";
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

        private List<SelectListItem> PopulateBranch()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT Code, Name FROM tfatBranch where Code != 'G00000' and Grp != 'G00000' and Category != 'Area'  order by Name ";
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

        // GET: Logistics/ALTTransaction
        public ActionResult Index(VMALTTransaction mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            
            var alt = ctxTFAT.tfatAltNotification.FirstOrDefault();
            if (alt == null)
            {
                mModel.RECORDKEY = 0;
                mModel.consignmentNotification = new ConsignmentNotification();
                mModel.consignmentNotification.trnBackDaysConsignment = new TrnBackDaysConsignment();
                mModel.consignmentNotification.trnForwardDaysConsignment = new TrnForwardDaysConsignment();
                mModel.consignmentNotification.trnDeclareValueConsignment = new TrnDeclareValueConsignment();
                mModel.consignmentNotification.trnDescriptionConsignment = new TrnDescriptionConsignment();
                mModel.consignmentNotification.trnDeclareValueEwaybillConsignment = new TrnDeclareValueEwaybillConsignment();

                mModel.lorryChallanNotification = new LorryChallanNotification();
                mModel.lorryChallanNotification.trnBackDaysLorryChallan = new TrnBackDaysLorryChallan();
                mModel.lorryChallanNotification.trnForwardDaysLorryChallan = new TrnForwardDaysLorryChallan();

                mModel.freightMemoNotification = new FreightMemoNotification();
                mModel.freightMemoNotification.trnBackDaysFreightMemo = new TrnBackDaysFreightMemo();
                mModel.freightMemoNotification.trnForwardDaysFreightMemo = new TrnForwardDaysFreightMemo();
                mModel.freightMemoNotification.trnDriverLicExpFreightMemo = new TrnDriverLicExpFreightMemo();
                mModel.freightMemoNotification.trnDocAmtFreightMemo = new TrnDocAmtFreightMemo();
                mModel.freightMemoNotification.trnBrokerFreightMemo = new TrnBrokerFreightMemo();
                mModel.freightMemoNotification.trnVehicleFreightMemo = new TrnVehicleFreightMemo();

                mModel.vehicleActivityNotification = new VehicleActivityNotification();
                mModel.vehicleActivityNotification.trnArrivalDaysVehicleActivity = new TrnArrivalDaysVehicleActivity();
                mModel.vehicleActivityNotification.trnDispatchDaysVehicleActivity = new TrnDispatchDaysVehicleActivity();
                mModel.vehicleActivityNotification.trnOverloadVehicleActivity = new TrnOverloadVehicleActivity();
                mModel.vehicleActivityNotification.trnClearVehicleActivity = new TrnClearVehicleActivity();
                mModel.vehicleActivityNotification.trnUnloadVehicleActivity = new TrnUnloadVehicleActivity();

                mModel.deliveryNotification = new DeliveryNotification();
                mModel.deliveryNotification.trnStatusDelivery = new TrnStatusDelivery();
                mModel.deliveryNotification.trnAnotherBranchDelivery = new TrnAnotherBranchDelivery();

                mModel.pODNotification = new PODNotification();
                mModel.pODNotification.trnBackDaysPOD = new TrnBackDaysPOD();
                mModel.pODNotification.trnForwardDaysPOD = new TrnForwardDaysPOD();
                mModel.pODNotification.trnReceivedDaysPOD = new TrnReceivedDaysPOD();
                mModel.pODNotification.trnSendDaysPOD = new TrnSendDaysPOD();
                mModel.pODNotification.trnSelectNoDeliveryPOD = new TrnSelectNoDeliveryPOD();

                mModel.billSubmissionNotification = new BillSubmissionNotification();
                mModel.billSubmissionNotification.trnBackDaysBillSubmission = new TrnBackDaysBillSubmission();
                mModel.billSubmissionNotification.trnForwardDaysBillSubmission = new TrnForwardDaysBillSubmission();
                mModel.billSubmissionNotification.trnLateDaysBillSubmission = new TrnLateDaysBillSubmission();

                mModel.docAuthenticateNotification = new DocAuthenticateNotification();
                mModel.docAuthenticateNotification.trnAuthenticateDaysDocAuthenticate = new TrnAuthenticateDaysDocAuthenticate();

                mModel.advBalPaymentNotification = new AdvBalPaymentNotification();
                mModel.advBalPaymentNotification.trnBackDaysAdvBalPayment = new TrnBackDaysAdvBalPayment();
                mModel.advBalPaymentNotification.trnForwardDaysAdvBalPayment = new TrnForwardDaysAdvBalPayment();
                mModel.advBalPaymentNotification.trnExpensesAdvBalPayment = new TrnExpensesAdvBalPayment();
                mModel.advBalPaymentNotification.trnDoubleExpAdvBalPayment = new TrnDoubleExpAdvBalPayment();
                mModel.advBalPaymentNotification.trnParticularDoubleExpAdvBalPayment = new TrnParticularDoubleExpAdvBalPayment();

                mModel.billNotification = new BillNotification();
                mModel.billNotification.trnBackDaysBill = new TrnBackDaysBill();
                mModel.billNotification.trnForwardDaysBill = new TrnForwardDaysBill();
                mModel.billNotification.trnDocAmountBill = new TrnDocAmountBill();
                mModel.billNotification.trnZeroAmountBill = new TrnZeroAmountBill();
                mModel.billNotification.trnOtherPartyBill = new TrnOtherPartyBill();
                mModel.billNotification.trnConsignmentBill = new TrnConsignmentBill();
                mModel.billNotification.trnPartyBill = new TrnPartyBill();
                mModel.billNotification.trnDoubleExpBill = new TrnDoubleExpBill();
                mModel.billNotification.trnParticularDoubleExpBill = new TrnParticularDoubleExpBill();

                mModel.cashBankPaymentNotification = new CashBankPaymentNotification();
                mModel.cashBankPaymentNotification.trnBackDaysCashBank = new TrnBackDaysCashBank();
                mModel.cashBankPaymentNotification.trnForwardDaysCashBank = new TrnForwardDaysCashBank();
                mModel.cashBankPaymentNotification.trnDocAmountCashBank = new TrnDocAmountCashBank();
                mModel.cashBankPaymentNotification.trnDoubleExpCashBank = new TrnDoubleExpCashBank();
                mModel.cashBankPaymentNotification.trnParticularDoubleExpCashBank = new TrnParticularDoubleExpCashBank();

                mModel.cashBankJVPaymentNotification = new CashBankJVPaymentNotification();
                mModel.cashBankJVPaymentNotification.trnBackDaysCashBankJV = new TrnBackDaysCashBankJV();
                mModel.cashBankJVPaymentNotification.trnForwardDaysCashBankJV = new TrnForwardDaysCashBankJV();
                mModel.cashBankJVPaymentNotification.trnDocAmountCashBankJV = new TrnDocAmountCashBankJV();
                mModel.cashBankJVPaymentNotification.trnDoubleExpCashBankJV = new TrnDoubleExpCashBankJV();
                mModel.cashBankJVPaymentNotification.trnParticularDoubleExpCashBankJV = new TrnParticularDoubleExpCashBankJV();

                mModel.creditPurchaseNotification = new CreditPurchaseNotification();
                mModel.creditPurchaseNotification.trnBackDaysCreditPurchase = new TrnBackDaysCreditPurchase();
                mModel.creditPurchaseNotification.trnForwardDaysCreditPurchase = new TrnForwardDaysCreditPurchase();
                mModel.creditPurchaseNotification.trnDocAmountCreditPurchase = new TrnDocAmountCreditPurchase();
                mModel.creditPurchaseNotification.trnVendorCreditPurchase = new TrnVendorCreditPurchase();
                mModel.creditPurchaseNotification.trnDoubleExpPurchase = new TrnDoubleExpPurchase();
                mModel.creditPurchaseNotification.trnParticularDoubleExpPurchase = new TrnParticularDoubleExpPurchase();

                mModel.creditPaymentNotification = new CreditPaymentNotification();
                mModel.creditPaymentNotification.trnBackDaysCreditPayment = new TrnBackDaysCreditPayment();
                mModel.creditPaymentNotification.trnForwardDaysCreditPayment = new TrnForwardDaysCreditPayment();
                mModel.creditPaymentNotification.trnDocAmountCreditPayment = new TrnDocAmountCreditPayment();
                mModel.creditPaymentNotification.trnVendorCreditPayment = new TrnVendorCreditPayment();

                mModel.bankReceiptNotification = new BankReceiptNotification();
                mModel.bankReceiptNotification.trnBackDaysBankReceipt = new TrnBackDaysBankReceipt();
                mModel.bankReceiptNotification.trnForwardDaysBankReceipt = new TrnForwardDaysBankReceipt();
                mModel.bankReceiptNotification.trnDocAmountBankReceipt = new TrnDocAmountBankReceipt();
                mModel.bankReceiptNotification.trnPendingAdjustBankReceipt = new TrnPendingAdjustBankReceipt();
                mModel.bankReceiptNotification.trnFreightRebateAmountBankReceipt = new TrnFreightRebateAmountBankReceipt();

                mModel.tripSheetNotification = new TripSheetNotification();
                mModel.tripSheetNotification.trnBackDaysTripSheet = new TrnBackDaysTripSheet();
                mModel.tripSheetNotification.trnForwardDaysTripSheet = new TrnForwardDaysTripSheet();
                mModel.tripSheetNotification.trnDocAmountTripSheet = new TrnDocAmountTripSheet();
                mModel.tripSheetNotification.trnEtraExpTripSheet = new TrnEtraExpTripSheet();
                mModel.tripSheetNotification.trnFmDateRangeTripSheet = new TrnFmDateRangeTripSheet();
                mModel.tripSheetNotification.trnADVDateRangeTripSheet = new TrnADVDateRangeTripSheet();
                mModel.tripSheetNotification.trnCCDateRangeTripSheet = new TrnCCDateRangeTripSheet();
                mModel.tripSheetNotification.trnDoubleExpTripSheet = new TrnDoubleExpTripSheet();
                mModel.tripSheetNotification.trnParticularDoubleExpTripSheet = new TrnParticularDoubleExpTripSheet();

            }
            else
            {
                mModel.RECORDKEY = alt.RECORDKEY;
                mModel.consignmentNotification = new ConsignmentNotification
                {
                    trnBackDaysConsignment = new TrnBackDaysConsignment
                    {
                        Nofitication = alt.CDBDN,
                        Email = alt.CDBDE,
                        Priority = alt.CDBDP,
                        EnteredBy = alt.CDBDSelf,
                        User = alt.CDBDU == null ? "" : alt.CDBDU,
                        Branch = alt.CDBDBranch == null ? "" : alt.CDBDBranch,
                        BackDays = alt.CDBDDays
                    },
                    trnForwardDaysConsignment = new TrnForwardDaysConsignment
                    {
                        Nofitication = alt.CDFDN,
                        Email = alt.CDFDE,
                        Priority = alt.CDFDP,
                        EnteredBy = alt.CDFDSelf,
                        User = alt.CDFDU == null ? "" : alt.CDFDU,
                        Branch = alt.CDFDBranch == null ? "" : alt.CDFDBranch,
                        ForwardDays = alt.CDFDDays
                    },
                    trnDeclareValueConsignment = new TrnDeclareValueConsignment
                    {
                        Nofitication = alt.CDDVN,
                        Email = alt.CDDVE,
                        Priority = alt.CDDVP,
                        EnteredBy = alt.CDDVSelf,
                        User = alt.CDDVU == null ? "" : alt.CDDVU,
                        Branch = alt.CDDVBranch == null ? "" : alt.CDDVBranch,
                        DeclareValue = alt.CDDVDeclareVal
                    },
                    trnDescriptionConsignment = new TrnDescriptionConsignment
                    {
                        Nofitication = alt.CDDSN,
                        Email = alt.CDDSE,
                        Priority = alt.CDDSP,
                        EnteredBy = alt.CDDSSelf,
                        User = alt.CDDSU == null ? "" : alt.CDDSU,
                        Branch = alt.CDDSBranch == null ? "" : alt.CDDSBranch,
                        MDescription = alt.CDDSDescription == null ? "" : alt.CDDSDescription
                    },
                    trnDeclareValueEwaybillConsignment=new TrnDeclareValueEwaybillConsignment
                    {
                        Nofitication = alt.CDDVEN,
                        Email = alt.CDDVEE,
                        Priority = alt.CDDVEP,
                        EnteredBy = alt.CDDVESelf,
                        User = alt.CDDVEU == null ? "" : alt.CDDVEU,
                        Branch = alt.CDDVEBranch == null ? "" : alt.CDDVEBranch,
                        DeclareValue = alt.CDDVEDeclareVal
                    },
                };
                mModel.lorryChallanNotification = new LorryChallanNotification
                {
                    trnBackDaysLorryChallan = new TrnBackDaysLorryChallan
                    {
                        Nofitication = alt.LCBDN,
                        Email = alt.LCBDE,
                        Priority = alt.LCBDP,
                        EnteredBy = alt.LCBDSelf,
                        User = alt.LCBDU == null ? "" : alt.LCBDU,
                        Branch = alt.LCBDBranch == null ? "" : alt.LCBDBranch,
                        BackDays = alt.LCBDDays
                    },
                    trnForwardDaysLorryChallan = new TrnForwardDaysLorryChallan
                    {
                        Nofitication = alt.LCFDN,
                        Email = alt.LCFDE,
                        Priority = alt.LCFDP,
                        EnteredBy = alt.LCFDSelf,
                        User = alt.LCFDU == null ? "" : alt.LCFDU,
                        Branch = alt.LCFDBranch == null ? "" : alt.LCFDBranch,
                        ForwardDays = alt.LCFDDays
                    },
                };
                mModel.freightMemoNotification = new FreightMemoNotification
                {
                    trnBackDaysFreightMemo = new TrnBackDaysFreightMemo
                    {
                        Nofitication = alt.FMBDN,
                        Email = alt.FMBDE,
                        Priority = alt.FMBDP,
                        EnteredBy = alt.FMBDSelf,
                        User = alt.FMBDU == null ? "" : alt.FMBDU,
                        Branch = alt.FMBDBranch == null ? "" : alt.FMBDBranch,
                        BackDays = alt.FMBDDays
                    },
                    trnForwardDaysFreightMemo = new TrnForwardDaysFreightMemo
                    {
                        Nofitication = alt.FMFDN,
                        Email = alt.FMFDE,
                        Priority = alt.FMFDP,
                        EnteredBy = alt.FMFDSelf,
                        User = alt.FMFDU == null ? "" : alt.FMFDU,
                        Branch = alt.FMFDBranch == null ? "" : alt.FMFDBranch,
                        ForwardDays = alt.FMFDDays
                    },
                    trnDriverLicExpFreightMemo = new TrnDriverLicExpFreightMemo
                    {
                        Nofitication = alt.FMDLN,
                        Email = alt.FMDLE,
                        Priority = alt.FMDLP,
                        EnteredBy = alt.FMDLSelf,
                        User = alt.FMDLU == null ? "" : alt.FMDLU,
                        Branch = alt.FMDLBranch == null ? "" : alt.FMDLBranch
                    },
                    trnDocAmtFreightMemo = new TrnDocAmtFreightMemo
                    {
                        Nofitication = alt.FMDAN,
                        Email = alt.FMDAE,
                        Priority = alt.FMDAP,
                        EnteredBy = alt.FMDASelf,
                        User = alt.FMDAU == null ? "" : alt.FMDAU,
                        Branch = alt.FMDABranch == null ? "" : alt.FMDABranch,
                        DocAmount = alt.FMDADocAmt
                    },
                    trnBrokerFreightMemo = new TrnBrokerFreightMemo
                    {
                        Nofitication = alt.FMBRN,
                        Email = alt.FMBRE,
                        Priority = alt.FMBRP,
                        EnteredBy = alt.FMBRSelf,
                        User = alt.FMBRU == null ? "" : alt.FMBRU,
                        Branch = alt.FMBRBranch == null ? "" : alt.FMBRBranch,
                        Broker = alt.FMBRBroker == null ? "" : alt.FMBRBroker
                    },
                    trnVehicleFreightMemo = new TrnVehicleFreightMemo
                    {
                        Nofitication = alt.FMVLN,
                        Email = alt.FMVLE,
                        Priority = alt.FMVLP,
                        EnteredBy = alt.FMVLSelf,
                        User = alt.FMVLU == null ? "" : alt.FMVLU,
                        Branch = alt.FMVLBranch == null ? "" : alt.FMVLBranch,
                        Vehicle = alt.FMVLVehicles == null ? "" : alt.FMVLVehicles
                    },
                };
                mModel.vehicleActivityNotification = new VehicleActivityNotification
                {
                    trnArrivalDaysVehicleActivity = new TrnArrivalDaysVehicleActivity
                    {
                        Nofitication = alt.VAARN,
                        Email = alt.VAARE,
                        Priority = alt.VAARP,
                        EnteredBy = alt.VAARSelf,
                        User = alt.VAARU == null ? "" : alt.VAARU,
                        Branch = alt.VAARBranch == null ? "" : alt.VAARBranch,
                        ArrivalDays = alt.VAARArrival == null ? "" : alt.VAARArrival
                    },
                    trnDispatchDaysVehicleActivity = new TrnDispatchDaysVehicleActivity
                    {
                        Nofitication = alt.VADSN,
                        Email = alt.VADSE,
                        Priority = alt.VADSP,
                        EnteredBy = alt.VADSSelf,
                        User = alt.VADSU == null ? "" : alt.VADSU,
                        Branch = alt.VADSBranch == null ? "" : alt.VADSBranch,
                        DispatchDays = alt.VADSDispatch == null ? "" : alt.VADSDispatch
                    },
                    trnOverloadVehicleActivity = new TrnOverloadVehicleActivity
                    {
                        Nofitication = alt.VAOLN,
                        Email = alt.VAOLE,
                        Priority = alt.VAOLP,
                        EnteredBy = alt.VAOLSelf,
                        User = alt.VAOLU == null ? "" : alt.VAOLU,
                        Branch = alt.VAOLBranch == null ? "" : alt.VAOLBranch,
                        OverloadinKg = alt.VAOLOvelloadKG
                    },
                    trnClearVehicleActivity = new TrnClearVehicleActivity
                    {
                        Nofitication = alt.VACLN,
                        Email = alt.VACLE,
                        Priority = alt.VACLP,
                        EnteredBy = alt.VACLSelf,
                        User = alt.VACLU == null ? "" : alt.VACLU,
                        Branch = alt.VACLBranch == null ? "" : alt.VACLBranch
                    },
                    trnUnloadVehicleActivity = new TrnUnloadVehicleActivity
                    {
                        Nofitication = alt.VAUNN,
                        Email = alt.VAUNE,
                        Priority = alt.VAUNP,
                        EnteredBy = alt.VAUNSelf,
                        User = alt.VAUNU == null ? "" : alt.VAUNU,
                        Branch = alt.VAUNBranch == null ? "" : alt.VAUNBranch
                    },
                };
                mModel.deliveryNotification = new DeliveryNotification
                {
                    trnStatusDelivery = new TrnStatusDelivery
                    {
                        Nofitication = alt.DLSTN,
                        Email = alt.DLSTE,
                        Priority = alt.DLSTP,
                        EnteredBy = alt.DLSTSelf,
                        User = alt.DLSTU == null ? "" : alt.DLSTU,
                        Branch = alt.DLSTBranch == null ? "" : alt.DLSTBranch,
                        Status = alt.DLSTStatus == null ? "" : alt.DLSTStatus,
                    },
                    trnAnotherBranchDelivery = new TrnAnotherBranchDelivery
                    {
                        Nofitication = alt.DLBRN,
                        Email = alt.DLBRE,
                        Priority = alt.DLBRP,
                        EnteredBy = alt.DLBRSelf,
                        User = alt.DLBRU == null ? "" : alt.DLBRU,
                        Branch = alt.DLBRBranch == null ? "" : alt.DLBRBranch,
                    },
                };
                mModel.pODNotification = new PODNotification
                {
                    trnBackDaysPOD = new TrnBackDaysPOD
                    {
                        Nofitication = alt.PODBDN,
                        Email = alt.PODBDE,
                        Priority = alt.PODBDP,
                        EnteredBy = alt.PODBDSelf,
                        User = alt.PODBDU == null ? "" : alt.PODBDU,
                        Branch = alt.PODBDBranch == null ? "" : alt.PODBDBranch,
                        BackDays = alt.PODBDDays
                    },
                    trnForwardDaysPOD = new TrnForwardDaysPOD
                    {
                        Nofitication = alt.PODFDN,
                        Email = alt.PODFDE,
                        Priority = alt.PODFDP,
                        EnteredBy = alt.PODFDSelf,
                        User = alt.PODFDU == null ? "" : alt.PODFDU,
                        Branch = alt.PODFDBranch == null ? "" : alt.PODFDBranch,
                        ForwardDays = alt.PODFDDays
                    },
                    trnReceivedDaysPOD = new TrnReceivedDaysPOD
                    {
                        Nofitication = alt.PODRCN,
                        Email = alt.PODRCE,
                        Priority = alt.PODRCP,
                        EnteredBy = alt.PODRCSelf,
                        User = alt.PODRCU == null ? "" : alt.PODRCU,
                        Branch = alt.PODRCBranch == null ? "" : alt.PODRCBranch,
                        ReceivedDays = alt.PODRCDays
                    },
                    trnSendDaysPOD = new TrnSendDaysPOD
                    {
                        Nofitication = alt.PODSDN,
                        Email = alt.PODSDE,
                        Priority = alt.PODSDP,
                        EnteredBy = alt.PODSDSelf,
                        User = alt.PODSDU == null ? "" : alt.PODSDU,
                        Branch = alt.PODSDBranch == null ? "" : alt.PODSDBranch,
                        SendDays = alt.PODSDDays
                    },
                    trnSelectNoDeliveryPOD = new TrnSelectNoDeliveryPOD
                    {
                        Nofitication = alt.PODNDLN,
                        Email = alt.PODNDLE,
                        Priority = alt.PODNDLP,
                        EnteredBy = alt.PODNDLSelf,
                        User = alt.PODNDLU == null ? "" : alt.PODNDLU,
                        Branch = alt.PODNDLBranch == null ? "" : alt.PODNDLBranch
                    },
                };
                mModel.billSubmissionNotification = new BillSubmissionNotification
                {
                    trnBackDaysBillSubmission = new TrnBackDaysBillSubmission
                    {
                        Nofitication = alt.BSBDN,
                        Email = alt.BSBDE,
                        Priority = alt.BSBDP,
                        EnteredBy = alt.BSBDSelf,
                        User = alt.BSBDU == null ? "" : alt.BSBDU,
                        Branch = alt.BSBDBranch == null ? "" : alt.BSBDBranch,
                        BackDays = alt.BSBDDays
                    },
                    trnForwardDaysBillSubmission = new TrnForwardDaysBillSubmission
                    {
                        Nofitication = alt.BSFDN,
                        Email = alt.BSFDE,
                        Priority = alt.BSFDP,
                        EnteredBy = alt.BSFDSelf,
                        User = alt.BSFDU == null ? "" : alt.BSFDU,
                        Branch = alt.BSFDBranch == null ? "" : alt.BSFDBranch,
                        ForwardDays = alt.BSFDDays
                    },
                    trnLateDaysBillSubmission = new TrnLateDaysBillSubmission
                    {
                        Nofitication = alt.BSLSN,
                        Email = alt.BSLSE,
                        Priority = alt.BSLSP,
                        EnteredBy = alt.BSLSSelf,
                        User = alt.BSLSU == null ? "" : alt.BSLSU,
                        Branch = alt.BSLSBranch == null ? "" : alt.BSLSBranch,
                        LateDays = alt.BSLSDays
                    },
                };
                mModel.docAuthenticateNotification = new DocAuthenticateNotification
                {
                    trnAuthenticateDaysDocAuthenticate = new TrnAuthenticateDaysDocAuthenticate
                    {
                        Nofitication = alt.DAAUTHN,
                        Email = alt.DAAUTHE,
                        Priority = alt.DAAUTHP,
                        EnteredBy = alt.DAAUTHSelf,
                        User = alt.DAAUTHU == null ? "" : alt.DAAUTHU,
                        Branch = alt.DAAUTHBranch == null ? "" : alt.DAAUTHBranch,
                        AuthDays = alt.DAAUTHDays
                    },
                };
                mModel.advBalPaymentNotification = new AdvBalPaymentNotification
                {
                    trnBackDaysAdvBalPayment = new TrnBackDaysAdvBalPayment
                    {
                        Nofitication = alt.FMPBDN,
                        Email = alt.FMPBDE,
                        Priority = alt.FMPBDP,
                        EnteredBy = alt.FMPBDSelf,
                        User = alt.FMPBDU == null ? "" : alt.FMPBDU,
                        Branch = alt.FMPBDBranch == null ? "" : alt.FMPBDBranch,
                        BackDays = alt.FMPBDDays
                    },
                    trnForwardDaysAdvBalPayment = new TrnForwardDaysAdvBalPayment
                    {
                        Nofitication = alt.FMPFDN,
                        Email = alt.FMPFDE,
                        Priority = alt.FMPFDP,
                        EnteredBy = alt.FMPFDSelf,
                        User = alt.FMPFDU == null ? "" : alt.FMPFDU,
                        Branch = alt.FMPFDBranch == null ? "" : alt.FMPFDBranch,
                        ForwardDays = alt.FMPFDDays
                    },
                    trnExpensesAdvBalPayment = new TrnExpensesAdvBalPayment
                    {
                        Nofitication = alt.FMPEXN,
                        Email = alt.FMPEXE,
                        Priority = alt.FMPEXP,
                        EnteredBy = alt.FMPEXSelf,
                        User = alt.FMPEXU == null ? "" : alt.FMPEXU,
                        Branch = alt.FMPEXBranch == null ? "" : alt.FMPEXBranch,
                        Expense = alt.FMPEXExpenses == null ? "" : alt.FMPEXExpenses,
                    },
                    trnDoubleExpAdvBalPayment = new TrnDoubleExpAdvBalPayment
                    {
                        Nofitication = alt.FMPCEXN,
                        Email = alt.FMPCEXE,
                        Priority = alt.FMPCEXP,
                        EnteredBy = alt.FMPCEXSelf,
                        User = alt.FMPCEXU == null ? "" : alt.FMPCEXU,
                        Branch = alt.FMPCEXBranch == null ? "" : alt.FMPCEXBranch,
                    },
                    trnParticularDoubleExpAdvBalPayment = new TrnParticularDoubleExpAdvBalPayment
                    {
                        Nofitication = alt.FMPCDEXN,
                        Email = alt.FMPCDEXE,
                        Priority = alt.FMPCDEXP,
                        EnteredBy = alt.FMPCDEXSelf,
                        User = alt.FMPCDEXU == null ? "" : alt.FMPCDEXU,
                        Branch = alt.FMPCDEXBranch == null ? "" : alt.FMPCDEXBranch,
                        Expense = alt.FMPCDEXExpenses == null ? "" : alt.FMPCDEXExpenses,
                    },
                };
                mModel.billNotification = new BillNotification
                {
                    trnBackDaysBill = new TrnBackDaysBill
                    {
                        Nofitication = alt.INVBDN,
                        Email = alt.INVBDE,
                        Priority = alt.INVBDP,
                        EnteredBy = alt.INVBDSelf,
                        User = alt.INVBDU == null ? "" : alt.INVBDU,
                        Branch = alt.INVBDBranch == null ? "" : alt.INVBDBranch,
                        BackDays = alt.INVBDDays
                    },
                    trnForwardDaysBill = new TrnForwardDaysBill
                    {
                        Nofitication = alt.INVFDN,
                        Email = alt.INVFDE,
                        Priority = alt.INVFDP,
                        EnteredBy = alt.INVFDSelf,
                        User = alt.INVFDU == null ? "" : alt.INVFDU,
                        Branch = alt.INVFDBranch == null ? "" : alt.INVFDBranch,
                        ForwardDays = alt.INVFDDays
                    },
                    trnDocAmountBill = new TrnDocAmountBill
                    {
                        Nofitication = alt.INVDAN,
                        Email = alt.INVDAE,
                        Priority = alt.INVDAP,
                        EnteredBy = alt.INVDASelf,
                        User = alt.INVDAU == null ? "" : alt.INVDAU,
                        Branch = alt.INVDABranch == null ? "" : alt.INVDABranch,
                        DocAmount = alt.INVDADocAmt
                    },
                    trnZeroAmountBill = new TrnZeroAmountBill
                    {
                        Nofitication = alt.INVZAN,
                        Email = alt.INVZAE,
                        Priority = alt.INVZAP,
                        EnteredBy = alt.INVZASelf,
                        User = alt.INVZAU == null ? "" : alt.INVZAU,
                        Branch = alt.INVZABranch == null ? "" : alt.INVZABranch
                    },
                    trnOtherPartyBill = new TrnOtherPartyBill
                    {
                        Nofitication = alt.INVOPN,
                        Email = alt.INVOPE,
                        Priority = alt.INVOPP,
                        EnteredBy = alt.INVOPSelf,
                        User = alt.INVOPU == null ? "" : alt.INVOPU,
                        Branch = alt.INVOPBranch == null ? "" : alt.INVOPBranch,
                    },
                    trnConsignmentBill = new TrnConsignmentBill
                    {
                        Nofitication = alt.INVCDN,
                        Email = alt.INVCDE,
                        Priority = alt.INVCDP,
                        EnteredBy = alt.INVCDSelf,
                        User = alt.INVCDU == null ? "" : alt.INVCDU,
                        Branch = alt.INVCDBranch == null ? "" : alt.INVCDBranch,
                        Days = alt.INVCDDays,
                    },
                    trnPartyBill = new TrnPartyBill
                    {
                        Nofitication = alt.INVPTN,
                        Email = alt.INVPTE,
                        Priority = alt.INVPTP,
                        EnteredBy = alt.INVPTSelf,
                        User = alt.INVPTU == null ? "" : alt.INVPTU,
                        Branch = alt.INVPTBranch== null ? "" : alt.INVPTBranch,
                        Party = alt.INVPTParty == null ? "" : alt.INVPTParty,
                    },
                    trnDoubleExpBill = new TrnDoubleExpBill
                    {
                        Nofitication = alt.INVCEXN,
                        Email = alt.INVCEXE,
                        Priority = alt.INVCEXP,
                        EnteredBy = alt.INVCEXSelf,
                        User = alt.INVCEXU == null ? "" : alt.INVCEXU,
                        Branch = alt.INVCEXBranch == null ? "" : alt.INVCEXBranch,
                    },
                    trnParticularDoubleExpBill = new TrnParticularDoubleExpBill
                    {
                        Nofitication = alt.INVCDEXN,
                        Email = alt.INVCDEXE,
                        Priority = alt.INVCDEXP,
                        EnteredBy = alt.INVCDEXSelf,
                        User = alt.INVCDEXU == null ? "" : alt.INVCDEXU,
                        Branch = alt.INVCDEXBranch == null ? "" : alt.INVCDEXBranch,
                        Expense = alt.INVCDEXExpenses == null ? "" : alt.INVCDEXExpenses,
                    },
                };
                mModel.cashBankPaymentNotification = new CashBankPaymentNotification
                {
                    trnBackDaysCashBank = new TrnBackDaysCashBank
                    {
                        Nofitication = alt.CPOBDN,
                        Email = alt.CPOBDE,
                        Priority = alt.CPOBDP,
                        EnteredBy = alt.CPOBDSelf,
                        User = alt.CPOBDU == null ? "" : alt.CPOBDU,
                        Branch = alt.CPOBDBranch == null ? "" : alt.CPOBDBranch,
                        BackDays = alt.CPOBDDays
                    },
                    trnForwardDaysCashBank = new TrnForwardDaysCashBank
                    {
                        Nofitication = alt.CPOFDN,
                        Email = alt.CPOFDE,
                        Priority = alt.CPOFDP,
                        EnteredBy = alt.CPOFDSelf,
                        User = alt.CPOFDU == null ? "" : alt.CPOFDU,
                        Branch = alt.CPOFDBranch == null ? "" : alt.CPOFDBranch,
                        ForwardDays = alt.CPOFDDays
                    },
                    trnDocAmountCashBank = new TrnDocAmountCashBank
                    {
                        Nofitication = alt.CPODAN,
                        Email = alt.CPODAE,
                        Priority = alt.CPODAP,
                        EnteredBy = alt.CPODASelf,
                        User = alt.CPODAU == null ? "" : alt.CPODAU,
                        Branch = alt.CPODABranch == null ? "" : alt.CPODABranch,
                        DocAmount = alt.CPODADocAmt
                    },
                    trnDoubleExpCashBank = new TrnDoubleExpCashBank
                    {
                        Nofitication = alt.CPOCEXN,
                        Email = alt.CPOCEXE,
                        Priority = alt.CPOCEXP,
                        EnteredBy = alt.CPOCEXSelf,
                        User = alt.CPOCEXU == null ? "" : alt.CPOCEXU,
                        Branch = alt.CPOCEXBranch == null ? "" : alt.CPOCEXBranch,
                    },
                    trnParticularDoubleExpCashBank = new TrnParticularDoubleExpCashBank
                    {
                        Nofitication = alt.CPOCDEXN,
                        Email = alt.CPOCDEXE,
                        Priority = alt.CPOCDEXP,
                        EnteredBy = alt.CPOCDEXSelf,
                        User = alt.CPOCDEXU == null ? "" : alt.CPOCDEXU,
                        Branch = alt.CPOCDEXBranch == null ? "" : alt.CPOCDEXBranch,
                        Expense = alt.CPOCDEXExpenses == null ? "" : alt.CPOCDEXExpenses,
                    },
                };
                mModel.cashBankJVPaymentNotification = new CashBankJVPaymentNotification
                {
                    trnBackDaysCashBankJV = new TrnBackDaysCashBankJV
                    {
                        Nofitication = alt.COTBDN,
                        Email = alt.COTBDE,
                        Priority = alt.COTBDP,
                        EnteredBy = alt.COTBDSelf,
                        User = alt.COTBDU == null ? "" : alt.COTBDU,
                        Branch = alt.COTBDBranch == null ? "" : alt.COTBDBranch,
                        BackDays = alt.COTBDDays
                    },
                    trnForwardDaysCashBankJV = new TrnForwardDaysCashBankJV
                    {
                        Nofitication = alt.COTFDN,
                        Email = alt.COTFDE,
                        Priority = alt.COTFDP,
                        EnteredBy = alt.COTFDSelf,
                        User = alt.COTFDU == null ? "" : alt.COTFDU,
                        Branch = alt.COTFDBranch == null ? "" : alt.COTFDBranch,
                        ForwardDays = alt.COTFDDays
                    },
                    trnDocAmountCashBankJV = new TrnDocAmountCashBankJV
                    {
                        Nofitication = alt.COTDAN,
                        Email = alt.COTDAE,
                        Priority = alt.COTDAP,
                        EnteredBy = alt.COTDASelf,
                        User = alt.COTDAU == null ? "" : alt.COTDAU,
                        Branch = alt.COTDABranch == null ? "" : alt.COTDABranch,
                        DocAmount = alt.COTDADocAmt
                    },
                    trnDoubleExpCashBankJV = new TrnDoubleExpCashBankJV
                    {
                        Nofitication = alt.COTCEXN,
                        Email = alt.COTCEXE,
                        Priority = alt.COTCEXP,
                        EnteredBy = alt.COTCEXSelf,
                        User = alt.COTCEXU == null ? "" : alt.COTCEXU,
                        Branch = alt.COTCEXBranch == null ? "" : alt.COTCEXBranch,
                    },
                    trnParticularDoubleExpCashBankJV = new TrnParticularDoubleExpCashBankJV
                    {
                        Nofitication = alt.COTCDEXN,
                        Email = alt.COTCDEXE,
                        Priority = alt.COTCDEXP,
                        EnteredBy = alt.COTCDEXSelf,
                        User = alt.COTCDEXU == null ? "" : alt.COTCDEXU,
                        Branch = alt.COTCDEXBranch == null ? "" : alt.COTCDEXBranch,
                        Expense = alt.COTCDEXExpenses == null ? "" : alt.COTCDEXExpenses,
                    },
                };
                mModel.creditPurchaseNotification = new CreditPurchaseNotification
                {
                    trnBackDaysCreditPurchase = new TrnBackDaysCreditPurchase
                    {
                        Nofitication = alt.PURBDN,
                        Email = alt.PURBDE,
                        Priority = alt.PURBDP,
                        EnteredBy = alt.PURBDSelf,
                        User = alt.PURBDU == null ? "" : alt.PURBDU,
                        Branch = alt.PURBDBranch == null ? "" : alt.PURBDBranch,
                        BackDays = alt.PURBDDays
                    },
                    trnForwardDaysCreditPurchase = new TrnForwardDaysCreditPurchase
                    {
                        Nofitication = alt.PURFDN,
                        Email = alt.PURFDE,
                        Priority = alt.PURFDP,
                        EnteredBy = alt.PURFDSelf,
                        User = alt.PURFDU == null ? "" : alt.PURFDU,
                        Branch = alt.PURFDBranch == null ? "" : alt.PURFDBranch,
                        ForwardDays = alt.PURFDDays
                    },
                    trnDocAmountCreditPurchase = new TrnDocAmountCreditPurchase
                    {
                        Nofitication = alt.PURDAN,
                        Email = alt.PURDAE,
                        Priority = alt.PURDAP,
                        EnteredBy = alt.PURDASelf,
                        User = alt.PURDAU == null ? "" : alt.PURDAU,
                        Branch = alt.PURDABranch == null ? "" : alt.PURDABranch,
                        DocAmount = alt.PURDADocAmt
                    },
                    trnVendorCreditPurchase = new TrnVendorCreditPurchase
                    {
                        Nofitication = alt.PURVRN,
                        Email = alt.PURVRE,
                        Priority = alt.PURVRP,
                        EnteredBy = alt.PURVRSelf,
                        User = alt.PURVRU == null ? "" : alt.PURVRU,
                        Branch = alt.PURVRBranch == null ? "" : alt.PURVRBranch,
                        Vendor = alt.PURVRVendor == null ? "" : alt.PURVRVendor
                    },
                    trnDoubleExpPurchase = new TrnDoubleExpPurchase
                    {
                        Nofitication = alt.PURCEXN,
                        Email = alt.PURCEXE,
                        Priority = alt.PURCEXP,
                        EnteredBy = alt.PURCEXSelf,
                        User = alt.PURCEXU == null ? "" : alt.PURCEXU,
                        Branch = alt.PURCEXBranch == null ? "" : alt.PURCEXBranch,
                    },
                    trnParticularDoubleExpPurchase = new TrnParticularDoubleExpPurchase
                    {
                        Nofitication = alt.PURCDEXN,
                        Email = alt.PURCDEXE,
                        Priority = alt.PURCDEXP,
                        EnteredBy = alt.PURCDEXSelf,
                        User = alt.PURCDEXU == null ? "" : alt.PURCDEXU,
                        Branch = alt.PURCDEXBranch == null ? "" : alt.PURCDEXBranch,
                        Expense = alt.PURCDEXExpenses == null ? "" : alt.PURCDEXExpenses,
                    },
                };
                mModel.creditPaymentNotification = new CreditPaymentNotification
                {
                    trnBackDaysCreditPayment = new TrnBackDaysCreditPayment
                    {
                        Nofitication = alt.BPMBDN,
                        Email = alt.BPMBDE,
                        Priority = alt.BPMBDP,
                        EnteredBy = alt.BPMBDSelf,
                        User = alt.BPMBDU == null ? "" : alt.BPMBDU,
                        Branch = alt.BPMBDBranch == null ? "" : alt.BPMBDBranch,
                        BackDays = alt.BPMBDDays
                    },
                    trnForwardDaysCreditPayment = new TrnForwardDaysCreditPayment
                    {
                        Nofitication = alt.BPMFDN,
                        Email = alt.BPMFDE,
                        Priority = alt.BPMFDP,
                        EnteredBy = alt.BPMFDSelf,
                        User = alt.BPMFDU == null ? "" : alt.BPMFDU,
                        Branch = alt.BPMFDBranch == null ? "" : alt.BPMFDBranch,
                        ForwardDays = alt.BPMFDDays
                    },
                    trnDocAmountCreditPayment = new TrnDocAmountCreditPayment
                    {
                        Nofitication = alt.BPMDAN,
                        Email = alt.BPMDAE,
                        Priority = alt.BPMDAP,
                        EnteredBy = alt.BPMDASelf,
                        User = alt.BPMDAU == null ? "" : alt.BPMDAU,
                        Branch = alt.BPMDABranch == null ? "" : alt.BPMDABranch,
                        DocAmount = alt.BPMDADocAmt
                    },
                    trnVendorCreditPayment = new TrnVendorCreditPayment
                    {
                        Nofitication = alt.BPMVRN,
                        Email = alt.BPMVRE,
                        Priority = alt.BPMVRP,
                        EnteredBy = alt.BPMVRSelf,
                        User = alt.BPMVRU == null ? "" : alt.BPMVRU,
                        Branch = alt.BPMVRBranch == null ? "" : alt.BPMVRBranch,
                        Vendor = alt.BPMVRVendor == null ? "" : alt.BPMVRVendor
                    },
                };
                mModel.bankReceiptNotification = new BankReceiptNotification
                {
                    trnBackDaysBankReceipt = new TrnBackDaysBankReceipt
                    {
                        Nofitication = alt.BRCBDN,
                        Email = alt.BRCBDE,
                        Priority = alt.BRCBDP,
                        EnteredBy = alt.BRCBDSelf,
                        User = alt.BRCBDU == null ? "" : alt.BRCBDU,
                        Branch = alt.BRCBDBranch == null ? "" : alt.BRCBDBranch,
                        BackDays = alt.BRCBDDays
                    },
                    trnForwardDaysBankReceipt = new TrnForwardDaysBankReceipt
                    {
                        Nofitication = alt.BRCFDN,
                        Email = alt.BRCFDE,
                        Priority = alt.BRCFDP,
                        EnteredBy = alt.BRCFDSelf,
                        User = alt.BRCFDU == null ? "" : alt.BRCFDU,
                        Branch = alt.BRCFDBranch == null ? "" : alt.BRCFDBranch,
                        ForwardDays = alt.BRCFDDays
                    },
                    trnDocAmountBankReceipt = new TrnDocAmountBankReceipt
                    {
                        Nofitication = alt.BRCDAN,
                        Email = alt.BRCDAE,
                        Priority = alt.BRCDAP,
                        EnteredBy = alt.BRCDASelf,
                        User = alt.BRCDAU == null ? "" : alt.BRCDAU,
                        Branch = alt.BRCDABranch == null ? "" : alt.BRCDABranch,
                        DocAmount = alt.BRCDADocAmt
                    },
                    trnPendingAdjustBankReceipt = new TrnPendingAdjustBankReceipt
                    {
                        Nofitication = alt.BRCUAN,
                        Email = alt.BRCUAE,
                        Priority = alt.BRCUAP,
                        EnteredBy = alt.BRCUASelf,
                        User = alt.BRCUAU == null ? "" : alt.BRCUAU,
                        Branch = alt.BRCUABranch == null ? "" : alt.BRCUABranch,
                    },
                    trnFreightRebateAmountBankReceipt = new TrnFreightRebateAmountBankReceipt
                    {
                        Nofitication = alt.BRCFRN,
                        Email = alt.BRCFRE,
                        Priority = alt.BRCFRP,
                        EnteredBy = alt.BRCFRSelf,
                        User = alt.BRCFRU == null ? "" : alt.BRCFRU,
                        Branch = alt.BRCFRBranch == null ? "" : alt.BRCFRBranch,
                        FreightRebateAmount = alt.BRCFRFreightRebate
                    },
                };
                mModel.tripSheetNotification = new TripSheetNotification
                {
                    trnBackDaysTripSheet = new TrnBackDaysTripSheet
                    {
                        Nofitication = alt.TRIPBDN,
                        Email = alt.TRIPBDE,
                        Priority = alt.TRIPBDP,
                        EnteredBy = alt.TRIPBDSelf,
                        User = alt.TRIPBDU == null ? "" : alt.TRIPBDU,
                        Branch = alt.TRIPBDBranch == null ? "" : alt.TRIPBDBranch,
                        BackDays = alt.TRIPBDDays
                    },
                    trnForwardDaysTripSheet = new TrnForwardDaysTripSheet
                    {
                        Nofitication = alt.TRIPFDN,
                        Email = alt.TRIPFDE,
                        Priority = alt.TRIPFDP,
                        EnteredBy = alt.TRIPFDSelf,
                        User = alt.TRIPFDU == null ? "" : alt.TRIPFDU,
                        Branch = alt.TRIPFDBranch == null ? "" : alt.TRIPFDBranch,
                        ForwardDays = alt.TRIPFDDays
                    },
                    trnDocAmountTripSheet = new TrnDocAmountTripSheet
                    {
                        Nofitication = alt.TRIPDAN,
                        Email = alt.TRIPDAE,
                        Priority = alt.TRIPDAP,
                        EnteredBy = alt.TRIPDASelf,
                        User = alt.TRIPDAU == null ? "" : alt.TRIPDAU,
                        Branch = alt.TRIPDABranch == null ? "" : alt.TRIPDABranch,
                        DocAmount = alt.TRIPDADocAmt
                    },
                    trnEtraExpTripSheet = new TrnEtraExpTripSheet
                    {
                        Nofitication = alt.TRIPEEN,
                        Email = alt.TRIPEEE,
                        Priority = alt.TRIPEEP,
                        EnteredBy = alt.TRIPEESelf,
                        User = alt.TRIPEEU == null ? "" : alt.TRIPEEU,
                        Branch = alt.TRIPEEBranch == null ? "" : alt.TRIPEEBranch,
                        ExtraExp = alt.TRIPEEExtraExpAmt
                    },
                    trnFmDateRangeTripSheet=new TrnFmDateRangeTripSheet {
                        Nofitication = alt.TRIPFMN,
                        Email = alt.TRIPFME,
                        Priority = alt.TRIPFMP,
                        EnteredBy = alt.TRIPFMSelf,
                        User = alt.TRIPFMU == null ? "" : alt.TRIPFMU,
                        Branch = alt.TRIPFMBranch == null ? "" : alt.TRIPFMBranch,
                    },
                    trnADVDateRangeTripSheet=new TrnADVDateRangeTripSheet {
                        Nofitication = alt.TRIPADVN,
                        Email = alt.TRIPADVE,
                        Priority = alt.TRIPADVP,
                        EnteredBy = alt.TRIPADVSelf,
                        User = alt.TRIPADVU == null ? "" : alt.TRIPADVU,
                        Branch = alt.TRIPADVBranch == null ? "" : alt.TRIPADVBranch,
                    },
                    trnCCDateRangeTripSheet=new TrnCCDateRangeTripSheet {
                        Nofitication = alt.TRIPCCN,
                        Email = alt.TRIPCCE,
                        Priority = alt.TRIPCCP,
                        EnteredBy = alt.TRIPCCSelf,
                        User = alt.TRIPCCU == null ? "" : alt.TRIPCCU,
                        Branch = alt.TRIPCCBranch == null ? "" : alt.TRIPCCBranch,
                    },
                    trnDoubleExpTripSheet = new TrnDoubleExpTripSheet
                    {
                        Nofitication = alt.TRIPCEXN,
                        Email = alt.TRIPCEXE,
                        Priority = alt.TRIPCEXP,
                        EnteredBy = alt.TRIPCEXSelf,
                        User = alt.TRIPCEXU == null ? "" : alt.TRIPCEXU,
                        Branch = alt.TRIPCEXBranch == null ? "" : alt.TRIPCEXBranch,
                    },
                    trnParticularDoubleExpTripSheet = new TrnParticularDoubleExpTripSheet
                    {
                        Nofitication = alt.TRIPCDEXN,
                        Email = alt.TRIPCDEXE,
                        Priority = alt.TRIPCDEXP,
                        EnteredBy = alt.TRIPCDEXSelf,
                        User = alt.TRIPCDEXU == null ? "" : alt.TRIPCDEXU,
                        Branch = alt.TRIPCDEXBranch == null ? "" : alt.TRIPCDEXBranch,
                        Expense = alt.TRIPCDEXExpenses == null ? "" : alt.TRIPCDEXExpenses,
                    },
                };
            }

            mModel.Users = PopulateUsers();
            mModel.Descriptions = PopulateDescriptions();
            mModel.Vehicles = PopulateTruckNo();
            mModel.Brokers = PopulateBroker();
            mModel.Statuss = PopulateDeliveryStatus();
            mModel.Expenses = PopulateExpenses();
            mModel.ABExpenses = PopulateABExpenses();
            mModel.LRExpenses = PopulateLRExpenses();
            mModel.Parties = PopulateParties();
            mModel.Vendors = PopulateVendor();
            mModel.Branches = PopulateBranch();

            UpdateAuditTrail(mbranchcode, mModel.RECORDKEY == 0 ? "Add" : "Edit", mModel.Header, "", DateTime.Now, 0, mModel.Document == null ? "" : mModel.Document.ToUpper().Trim(), "", "ALT-Trasaction");
            return View(mModel);
        }

        public ActionResult SaveData(VMALTTransaction mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    tfatAltNotification tfatAlt = new tfatAltNotification();
                    bool mAdd = true;
                    if (ctxTFAT.tfatAltNotification.FirstOrDefault() != null)
                    {
                        tfatAlt = ctxTFAT.tfatAltNotification.FirstOrDefault();
                        mAdd = false;
                    }
                    if (mAdd)
                    {
                        tfatAlt.CreateDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                    }
                    //Consignment
                    {
                        tfatAlt.CDBDN = mModel.consignmentNotification.trnBackDaysConsignment.Nofitication;
                        tfatAlt.CDBDE = mModel.consignmentNotification.trnBackDaysConsignment.Email;
                        tfatAlt.CDBDP = mModel.consignmentNotification.trnBackDaysConsignment.Priority;
                        tfatAlt.CDBDSelf = mModel.consignmentNotification.trnBackDaysConsignment.EnteredBy;
                        tfatAlt.CDBDU = mModel.consignmentNotification.trnBackDaysConsignment.User ?? "";
                        tfatAlt.CDBDBranch = mModel.consignmentNotification.trnBackDaysConsignment.Branch ?? "";
                        tfatAlt.CDBDDays = mModel.consignmentNotification.trnBackDaysConsignment.BackDays;
                        tfatAlt.CDFDN = mModel.consignmentNotification.trnForwardDaysConsignment.Nofitication;
                        tfatAlt.CDFDE = mModel.consignmentNotification.trnForwardDaysConsignment.Email;
                        tfatAlt.CDFDP = mModel.consignmentNotification.trnForwardDaysConsignment.Priority;
                        tfatAlt.CDFDSelf = mModel.consignmentNotification.trnForwardDaysConsignment.EnteredBy;
                        tfatAlt.CDFDU = mModel.consignmentNotification.trnForwardDaysConsignment.User ?? "";
                        tfatAlt.CDFDBranch = mModel.consignmentNotification.trnForwardDaysConsignment.Branch ?? "";
                        tfatAlt.CDFDDays = mModel.consignmentNotification.trnForwardDaysConsignment.ForwardDays;
                        tfatAlt.CDDVN = mModel.consignmentNotification.trnDeclareValueConsignment.Nofitication;
                        tfatAlt.CDDVE = mModel.consignmentNotification.trnDeclareValueConsignment.Email;
                        tfatAlt.CDDVP = mModel.consignmentNotification.trnDeclareValueConsignment.Priority;
                        tfatAlt.CDDVSelf = mModel.consignmentNotification.trnDeclareValueConsignment.EnteredBy;
                        tfatAlt.CDDVU = mModel.consignmentNotification.trnDeclareValueConsignment.User ?? "";
                        tfatAlt.CDDVBranch = mModel.consignmentNotification.trnDeclareValueConsignment.Branch ?? "";
                        tfatAlt.CDDVDeclareVal = mModel.consignmentNotification.trnDeclareValueConsignment.DeclareValue;
                        tfatAlt.CDDSN = mModel.consignmentNotification.trnDescriptionConsignment.Nofitication;
                        tfatAlt.CDDSE = mModel.consignmentNotification.trnDescriptionConsignment.Email;
                        tfatAlt.CDDSP = mModel.consignmentNotification.trnDescriptionConsignment.Priority;
                        tfatAlt.CDDSSelf = mModel.consignmentNotification.trnDescriptionConsignment.EnteredBy;
                        tfatAlt.CDDSU = mModel.consignmentNotification.trnDescriptionConsignment.User ?? "";
                        tfatAlt.CDDSBranch = mModel.consignmentNotification.trnDescriptionConsignment.Branch ?? "";
                        tfatAlt.CDDSDescription = mModel.consignmentNotification.trnDescriptionConsignment.MDescription ?? "";
                        tfatAlt.CDDVEN = mModel.consignmentNotification.trnDeclareValueEwaybillConsignment.Nofitication;
                        tfatAlt.CDDVEE = mModel.consignmentNotification.trnDeclareValueEwaybillConsignment.Email;
                        tfatAlt.CDDVEP = mModel.consignmentNotification.trnDeclareValueEwaybillConsignment.Priority;
                        tfatAlt.CDDVESelf = mModel.consignmentNotification.trnDeclareValueEwaybillConsignment.EnteredBy;
                        tfatAlt.CDDVEU = mModel.consignmentNotification.trnDeclareValueEwaybillConsignment.User ?? "";
                        tfatAlt.CDDVEBranch = mModel.consignmentNotification.trnDeclareValueEwaybillConsignment.Branch ?? "";
                        tfatAlt.CDDVEDeclareVal = mModel.consignmentNotification.trnDeclareValueEwaybillConsignment.DeclareValue;
                    }
                    //Lorry Challan
                    {
                        tfatAlt.LCBDN = mModel.lorryChallanNotification.trnBackDaysLorryChallan.Nofitication;
                        tfatAlt.LCBDE = mModel.lorryChallanNotification.trnBackDaysLorryChallan.Email;
                        tfatAlt.LCBDP = mModel.lorryChallanNotification.trnBackDaysLorryChallan.Priority;
                        tfatAlt.LCBDSelf = mModel.lorryChallanNotification.trnBackDaysLorryChallan.EnteredBy;
                        tfatAlt.LCBDU = mModel.lorryChallanNotification.trnBackDaysLorryChallan.User ?? "";
                        tfatAlt.LCBDBranch = mModel.lorryChallanNotification.trnBackDaysLorryChallan.Branch ?? "";
                        tfatAlt.LCBDDays = mModel.lorryChallanNotification.trnBackDaysLorryChallan.BackDays;
                        tfatAlt.LCFDN = mModel.lorryChallanNotification.trnForwardDaysLorryChallan.Nofitication;
                        tfatAlt.LCFDE = mModel.lorryChallanNotification.trnForwardDaysLorryChallan.Email;
                        tfatAlt.LCFDP = mModel.lorryChallanNotification.trnForwardDaysLorryChallan.Priority;
                        tfatAlt.LCFDSelf = mModel.lorryChallanNotification.trnForwardDaysLorryChallan.EnteredBy;
                        tfatAlt.LCFDU = mModel.lorryChallanNotification.trnForwardDaysLorryChallan.User ?? "";
                        tfatAlt.LCFDBranch = mModel.lorryChallanNotification.trnForwardDaysLorryChallan.Branch ?? "";
                        tfatAlt.LCFDDays = mModel.lorryChallanNotification.trnForwardDaysLorryChallan.ForwardDays;
                    }
                    //Freight Memo
                    {
                        tfatAlt.FMBDN = mModel.freightMemoNotification.trnBackDaysFreightMemo.Nofitication;
                        tfatAlt.FMBDE = mModel.freightMemoNotification.trnBackDaysFreightMemo.Email;
                        tfatAlt.FMBDP = mModel.freightMemoNotification.trnBackDaysFreightMemo.Priority;
                        tfatAlt.FMBDSelf = mModel.freightMemoNotification.trnBackDaysFreightMemo.EnteredBy;
                        tfatAlt.FMBDU = mModel.freightMemoNotification.trnBackDaysFreightMemo.User ?? "";
                        tfatAlt.FMBDBranch = mModel.freightMemoNotification.trnBackDaysFreightMemo.Branch ?? "";
                        tfatAlt.FMBDDays = mModel.freightMemoNotification.trnBackDaysFreightMemo.BackDays;
                        tfatAlt.FMFDN = mModel.freightMemoNotification.trnForwardDaysFreightMemo.Nofitication;
                        tfatAlt.FMFDE = mModel.freightMemoNotification.trnForwardDaysFreightMemo.Email;
                        tfatAlt.FMFDP = mModel.freightMemoNotification.trnForwardDaysFreightMemo.Priority;
                        tfatAlt.FMFDSelf = mModel.freightMemoNotification.trnForwardDaysFreightMemo.EnteredBy;
                        tfatAlt.FMFDU = mModel.freightMemoNotification.trnForwardDaysFreightMemo.User ?? "";
                        tfatAlt.FMFDBranch = mModel.freightMemoNotification.trnForwardDaysFreightMemo.Branch ?? "";
                        tfatAlt.FMFDDays = mModel.freightMemoNotification.trnForwardDaysFreightMemo.ForwardDays;
                        tfatAlt.FMDLN = mModel.freightMemoNotification.trnDriverLicExpFreightMemo.Nofitication;
                        tfatAlt.FMDLE = mModel.freightMemoNotification.trnDriverLicExpFreightMemo.Email;
                        tfatAlt.FMDLP = mModel.freightMemoNotification.trnDriverLicExpFreightMemo.Priority;
                        tfatAlt.FMDLSelf = mModel.freightMemoNotification.trnDriverLicExpFreightMemo.EnteredBy;
                        tfatAlt.FMDLU = mModel.freightMemoNotification.trnDriverLicExpFreightMemo.User ?? "";
                        tfatAlt.FMDLBranch = mModel.freightMemoNotification.trnDriverLicExpFreightMemo.Branch ?? "";
                        tfatAlt.FMDAN = mModel.freightMemoNotification.trnDocAmtFreightMemo.Nofitication;
                        tfatAlt.FMDAE = mModel.freightMemoNotification.trnDocAmtFreightMemo.Email;
                        tfatAlt.FMDAP = mModel.freightMemoNotification.trnDocAmtFreightMemo.Priority;
                        tfatAlt.FMDASelf = mModel.freightMemoNotification.trnDocAmtFreightMemo.EnteredBy;
                        tfatAlt.FMDAU = mModel.freightMemoNotification.trnDocAmtFreightMemo.User ?? "";
                        tfatAlt.FMDABranch = mModel.freightMemoNotification.trnDocAmtFreightMemo.Branch ?? "";
                        tfatAlt.FMDADocAmt = mModel.freightMemoNotification.trnDocAmtFreightMemo.DocAmount;
                        tfatAlt.FMBRN = mModel.freightMemoNotification.trnBrokerFreightMemo.Nofitication;
                        tfatAlt.FMBRE = mModel.freightMemoNotification.trnBrokerFreightMemo.Email;
                        tfatAlt.FMBRP = mModel.freightMemoNotification.trnBrokerFreightMemo.Priority;
                        tfatAlt.FMBRSelf = mModel.freightMemoNotification.trnBrokerFreightMemo.EnteredBy;
                        tfatAlt.FMBRU = mModel.freightMemoNotification.trnBrokerFreightMemo.User ?? "";
                        tfatAlt.FMBRBranch = mModel.freightMemoNotification.trnBrokerFreightMemo.Branch ?? "";
                        tfatAlt.FMBRBroker = mModel.freightMemoNotification.trnBrokerFreightMemo.Broker ?? "";
                        tfatAlt.FMVLN = mModel.freightMemoNotification.trnVehicleFreightMemo.Nofitication;
                        tfatAlt.FMVLE = mModel.freightMemoNotification.trnVehicleFreightMemo.Email;
                        tfatAlt.FMVLP = mModel.freightMemoNotification.trnVehicleFreightMemo.Priority;
                        tfatAlt.FMVLSelf = mModel.freightMemoNotification.trnVehicleFreightMemo.EnteredBy;
                        tfatAlt.FMVLU = mModel.freightMemoNotification.trnVehicleFreightMemo.User ?? "";
                        tfatAlt.FMVLBranch = mModel.freightMemoNotification.trnVehicleFreightMemo.Branch ?? "";
                        tfatAlt.FMVLVehicles = mModel.freightMemoNotification.trnVehicleFreightMemo.Vehicle ?? "";
                    }
                    //Vehicle Activity
                    {
                        tfatAlt.VAARN = mModel.vehicleActivityNotification.trnArrivalDaysVehicleActivity.Nofitication;
                        tfatAlt.VAARE = mModel.vehicleActivityNotification.trnArrivalDaysVehicleActivity.Email;
                        tfatAlt.VAARP = mModel.vehicleActivityNotification.trnArrivalDaysVehicleActivity.Priority;
                        tfatAlt.VAARSelf = mModel.vehicleActivityNotification.trnArrivalDaysVehicleActivity.EnteredBy;
                        tfatAlt.VAARU = mModel.vehicleActivityNotification.trnArrivalDaysVehicleActivity.User ?? "";
                        tfatAlt.VAARBranch = mModel.vehicleActivityNotification.trnArrivalDaysVehicleActivity.Branch ?? "";
                        tfatAlt.VAARArrival = mModel.vehicleActivityNotification.trnArrivalDaysVehicleActivity.ArrivalDays;
                        tfatAlt.VADSN = mModel.vehicleActivityNotification.trnDispatchDaysVehicleActivity.Nofitication;
                        tfatAlt.VADSE = mModel.vehicleActivityNotification.trnDispatchDaysVehicleActivity.Email;
                        tfatAlt.VADSP = mModel.vehicleActivityNotification.trnDispatchDaysVehicleActivity.Priority;
                        tfatAlt.VADSSelf = mModel.vehicleActivityNotification.trnDispatchDaysVehicleActivity.EnteredBy;
                        tfatAlt.VADSU = mModel.vehicleActivityNotification.trnDispatchDaysVehicleActivity.User ?? "";
                        tfatAlt.VADSBranch = mModel.vehicleActivityNotification.trnDispatchDaysVehicleActivity.Branch ?? "";
                        tfatAlt.VADSDispatch = mModel.vehicleActivityNotification.trnDispatchDaysVehicleActivity.DispatchDays;
                        tfatAlt.VAOLN = mModel.vehicleActivityNotification.trnOverloadVehicleActivity.Nofitication;
                        tfatAlt.VAOLE = mModel.vehicleActivityNotification.trnOverloadVehicleActivity.Email;
                        tfatAlt.VAOLP = mModel.vehicleActivityNotification.trnOverloadVehicleActivity.Priority;
                        tfatAlt.VAOLSelf = mModel.vehicleActivityNotification.trnOverloadVehicleActivity.EnteredBy;
                        tfatAlt.VAOLU = mModel.vehicleActivityNotification.trnOverloadVehicleActivity.User ?? "";
                        tfatAlt.VAOLBranch = mModel.vehicleActivityNotification.trnOverloadVehicleActivity.Branch ?? "";
                        tfatAlt.VAOLOvelloadKG = mModel.vehicleActivityNotification.trnOverloadVehicleActivity.OverloadinKg;
                        tfatAlt.VACLN = mModel.vehicleActivityNotification.trnClearVehicleActivity.Nofitication;
                        tfatAlt.VACLE = mModel.vehicleActivityNotification.trnClearVehicleActivity.Email;
                        tfatAlt.VACLP = mModel.vehicleActivityNotification.trnClearVehicleActivity.Priority;
                        tfatAlt.VACLSelf = mModel.vehicleActivityNotification.trnClearVehicleActivity.EnteredBy;
                        tfatAlt.VACLU = mModel.vehicleActivityNotification.trnClearVehicleActivity.User ?? "";
                        tfatAlt.VACLBranch = mModel.vehicleActivityNotification.trnClearVehicleActivity.Branch ?? "";
                        tfatAlt.VAUNN = mModel.vehicleActivityNotification.trnUnloadVehicleActivity.Nofitication;
                        tfatAlt.VAUNE = mModel.vehicleActivityNotification.trnUnloadVehicleActivity.Email;
                        tfatAlt.VAUNP = mModel.vehicleActivityNotification.trnUnloadVehicleActivity.Priority;
                        tfatAlt.VAUNSelf = mModel.vehicleActivityNotification.trnUnloadVehicleActivity.EnteredBy;
                        tfatAlt.VAUNU = mModel.vehicleActivityNotification.trnUnloadVehicleActivity.User ?? "";
                        tfatAlt.VAUNBranch = mModel.vehicleActivityNotification.trnUnloadVehicleActivity.Branch ?? "";
                    }
                    //Delivery
                    {
                        tfatAlt.DLSTN = mModel.deliveryNotification.trnStatusDelivery.Nofitication;
                        tfatAlt.DLSTE = mModel.deliveryNotification.trnStatusDelivery.Email;
                        tfatAlt.DLSTP = mModel.deliveryNotification.trnStatusDelivery.Priority;
                        tfatAlt.DLSTSelf = mModel.deliveryNotification.trnStatusDelivery.EnteredBy;
                        tfatAlt.DLSTU = mModel.deliveryNotification.trnStatusDelivery.User ?? "";
                        tfatAlt.DLSTBranch = mModel.deliveryNotification.trnStatusDelivery.Branch ?? "";
                        tfatAlt.DLSTStatus = mModel.deliveryNotification.trnStatusDelivery.Status ?? "";
                        tfatAlt.DLBRN = mModel.deliveryNotification.trnAnotherBranchDelivery.Nofitication;
                        tfatAlt.DLBRE = mModel.deliveryNotification.trnAnotherBranchDelivery.Email;
                        tfatAlt.DLBRP = mModel.deliveryNotification.trnAnotherBranchDelivery.Priority;
                        tfatAlt.DLBRSelf = mModel.deliveryNotification.trnAnotherBranchDelivery.EnteredBy;
                        tfatAlt.DLBRU = mModel.deliveryNotification.trnAnotherBranchDelivery.User ?? "";
                        tfatAlt.DLBRBranch = mModel.deliveryNotification.trnAnotherBranchDelivery.Branch ?? "";
                    }
                    //POD
                    {
                        tfatAlt.PODBDN = mModel.pODNotification.trnBackDaysPOD.Nofitication;
                        tfatAlt.PODBDE = mModel.pODNotification.trnBackDaysPOD.Email;
                        tfatAlt.PODBDP = mModel.pODNotification.trnBackDaysPOD.Priority;
                        tfatAlt.PODBDSelf = mModel.pODNotification.trnBackDaysPOD.EnteredBy;
                        tfatAlt.PODBDU = mModel.pODNotification.trnBackDaysPOD.User ?? "";
                        tfatAlt.PODBDBranch = mModel.pODNotification.trnBackDaysPOD.Branch ?? "";
                        tfatAlt.PODBDDays = mModel.pODNotification.trnBackDaysPOD.BackDays;
                        tfatAlt.PODFDN = mModel.pODNotification.trnForwardDaysPOD.Nofitication;
                        tfatAlt.PODFDE = mModel.pODNotification.trnForwardDaysPOD.Email;
                        tfatAlt.PODFDP = mModel.pODNotification.trnForwardDaysPOD.Priority;
                        tfatAlt.PODFDSelf = mModel.pODNotification.trnForwardDaysPOD.EnteredBy;
                        tfatAlt.PODFDU = mModel.pODNotification.trnForwardDaysPOD.User ?? "";
                        tfatAlt.PODFDBranch = mModel.pODNotification.trnForwardDaysPOD.Branch ?? "";
                        tfatAlt.PODFDDays = mModel.pODNotification.trnForwardDaysPOD.ForwardDays;
                        tfatAlt.PODRCN = mModel.pODNotification.trnReceivedDaysPOD.Nofitication;
                        tfatAlt.PODRCE = mModel.pODNotification.trnReceivedDaysPOD.Email;
                        tfatAlt.PODRCP = mModel.pODNotification.trnReceivedDaysPOD.Priority;
                        tfatAlt.PODRCSelf = mModel.pODNotification.trnReceivedDaysPOD.EnteredBy;
                        tfatAlt.PODRCU = mModel.pODNotification.trnReceivedDaysPOD.User ?? "";
                        tfatAlt.PODRCBranch = mModel.pODNotification.trnReceivedDaysPOD.Branch ?? "";
                        tfatAlt.PODRCDays = mModel.pODNotification.trnReceivedDaysPOD.ReceivedDays;
                        tfatAlt.PODSDN = mModel.pODNotification.trnSendDaysPOD.Nofitication;
                        tfatAlt.PODSDE = mModel.pODNotification.trnSendDaysPOD.Email;
                        tfatAlt.PODSDP = mModel.pODNotification.trnSendDaysPOD.Priority;
                        tfatAlt.PODSDSelf = mModel.pODNotification.trnSendDaysPOD.EnteredBy;
                        tfatAlt.PODSDU = mModel.pODNotification.trnSendDaysPOD.User ?? "";
                        tfatAlt.PODSDBranch = mModel.pODNotification.trnSendDaysPOD.Branch ?? "";
                        tfatAlt.PODSDDays = mModel.pODNotification.trnSendDaysPOD.SendDays;
                        tfatAlt.PODNDLN = mModel.pODNotification.trnSelectNoDeliveryPOD.Nofitication;
                        tfatAlt.PODNDLE = mModel.pODNotification.trnSelectNoDeliveryPOD.Email;
                        tfatAlt.PODNDLP = mModel.pODNotification.trnSelectNoDeliveryPOD.Priority;
                        tfatAlt.PODNDLSelf = mModel.pODNotification.trnSelectNoDeliveryPOD.EnteredBy;
                        tfatAlt.PODNDLU = mModel.pODNotification.trnSelectNoDeliveryPOD.User ?? "";
                        tfatAlt.PODNDLBranch = mModel.pODNotification.trnSelectNoDeliveryPOD.Branch ?? "";
                    }
                    //Bill Submisssion 
                    {
                        tfatAlt.BSBDN = mModel.billSubmissionNotification.trnBackDaysBillSubmission.Nofitication;
                        tfatAlt.BSBDE = mModel.billSubmissionNotification.trnBackDaysBillSubmission.Email;
                        tfatAlt.BSBDP = mModel.billSubmissionNotification.trnBackDaysBillSubmission.Priority;
                        tfatAlt.BSBDSelf = mModel.billSubmissionNotification.trnBackDaysBillSubmission.EnteredBy;
                        tfatAlt.BSBDU = mModel.billSubmissionNotification.trnBackDaysBillSubmission.User ?? "";
                        tfatAlt.BSBDBranch = mModel.billSubmissionNotification.trnBackDaysBillSubmission.Branch ?? "";
                        tfatAlt.BSBDDays = mModel.billSubmissionNotification.trnBackDaysBillSubmission.BackDays;
                        tfatAlt.BSFDN = mModel.billSubmissionNotification.trnForwardDaysBillSubmission.Nofitication;
                        tfatAlt.BSFDE = mModel.billSubmissionNotification.trnForwardDaysBillSubmission.Email;
                        tfatAlt.BSFDP = mModel.billSubmissionNotification.trnForwardDaysBillSubmission.Priority;
                        tfatAlt.BSFDSelf = mModel.billSubmissionNotification.trnForwardDaysBillSubmission.EnteredBy;
                        tfatAlt.BSFDU = mModel.billSubmissionNotification.trnForwardDaysBillSubmission.User ?? "";
                        tfatAlt.BSFDBranch = mModel.billSubmissionNotification.trnForwardDaysBillSubmission.Branch ?? "";
                        tfatAlt.BSFDDays = mModel.billSubmissionNotification.trnForwardDaysBillSubmission.ForwardDays;
                        tfatAlt.BSLSN = mModel.billSubmissionNotification.trnLateDaysBillSubmission.Nofitication;
                        tfatAlt.BSLSE = mModel.billSubmissionNotification.trnLateDaysBillSubmission.Email;
                        tfatAlt.BSLSP = mModel.billSubmissionNotification.trnLateDaysBillSubmission.Priority;
                        tfatAlt.BSLSSelf = mModel.billSubmissionNotification.trnLateDaysBillSubmission.EnteredBy;
                        tfatAlt.BSLSU = mModel.billSubmissionNotification.trnLateDaysBillSubmission.User ?? "";
                        tfatAlt.BSLSBranch = mModel.billSubmissionNotification.trnLateDaysBillSubmission.Branch ?? "";
                        tfatAlt.BSLSDays = mModel.billSubmissionNotification.trnLateDaysBillSubmission.LateDays;
                    }
                    //Doc Authentication
                    {
                        tfatAlt.DAAUTHN = mModel.docAuthenticateNotification.trnAuthenticateDaysDocAuthenticate.Nofitication;
                        tfatAlt.DAAUTHE = mModel.docAuthenticateNotification.trnAuthenticateDaysDocAuthenticate.Email;
                        tfatAlt.DAAUTHP = mModel.docAuthenticateNotification.trnAuthenticateDaysDocAuthenticate.Priority;
                        tfatAlt.DAAUTHSelf = mModel.docAuthenticateNotification.trnAuthenticateDaysDocAuthenticate.EnteredBy;
                        tfatAlt.DAAUTHU = mModel.docAuthenticateNotification.trnAuthenticateDaysDocAuthenticate.User ?? "";
                        tfatAlt.DAAUTHBranch = mModel.docAuthenticateNotification.trnAuthenticateDaysDocAuthenticate.Branch ?? "";
                        tfatAlt.DAAUTHDays = mModel.docAuthenticateNotification.trnAuthenticateDaysDocAuthenticate.AuthDays;
                    }
                    //Advance Balance Payment
                    tfatAlt.FMPBDN = mModel.advBalPaymentNotification.trnBackDaysAdvBalPayment.Nofitication;
                    tfatAlt.FMPBDE = mModel.advBalPaymentNotification.trnBackDaysAdvBalPayment.Email;
                    tfatAlt.FMPBDP = mModel.advBalPaymentNotification.trnBackDaysAdvBalPayment.Priority;
                    tfatAlt.FMPBDSelf = mModel.advBalPaymentNotification.trnBackDaysAdvBalPayment.EnteredBy;
                    tfatAlt.FMPBDU = mModel.advBalPaymentNotification.trnBackDaysAdvBalPayment.User ?? "";
                    tfatAlt.FMPBDBranch = mModel.advBalPaymentNotification.trnBackDaysAdvBalPayment.Branch ?? "";
                    tfatAlt.FMPBDDays = mModel.advBalPaymentNotification.trnBackDaysAdvBalPayment.BackDays;
                    tfatAlt.FMPFDN = mModel.advBalPaymentNotification.trnForwardDaysAdvBalPayment.Nofitication;
                    tfatAlt.FMPFDE = mModel.advBalPaymentNotification.trnForwardDaysAdvBalPayment.Email;
                    tfatAlt.FMPFDP = mModel.advBalPaymentNotification.trnForwardDaysAdvBalPayment.Priority;
                    tfatAlt.FMPFDSelf = mModel.advBalPaymentNotification.trnForwardDaysAdvBalPayment.EnteredBy;
                    tfatAlt.FMPFDU = mModel.advBalPaymentNotification.trnForwardDaysAdvBalPayment.User ?? "";
                    tfatAlt.FMPFDBranch = mModel.advBalPaymentNotification.trnForwardDaysAdvBalPayment.Branch ?? "";
                    tfatAlt.FMPFDDays = mModel.advBalPaymentNotification.trnForwardDaysAdvBalPayment.ForwardDays;
                    tfatAlt.FMPEXN = mModel.advBalPaymentNotification.trnExpensesAdvBalPayment.Nofitication;
                    tfatAlt.FMPEXE = mModel.advBalPaymentNotification.trnExpensesAdvBalPayment.Email;
                    tfatAlt.FMPEXP = mModel.advBalPaymentNotification.trnExpensesAdvBalPayment.Priority;
                    tfatAlt.FMPEXSelf = mModel.advBalPaymentNotification.trnExpensesAdvBalPayment.EnteredBy;
                    tfatAlt.FMPEXU = mModel.advBalPaymentNotification.trnExpensesAdvBalPayment.User ?? "";
                    tfatAlt.FMPEXBranch = mModel.advBalPaymentNotification.trnExpensesAdvBalPayment.Branch ?? "";
                    tfatAlt.FMPEXExpenses = mModel.advBalPaymentNotification.trnExpensesAdvBalPayment.Expense ?? "";
                    tfatAlt.FMPCEXN = mModel.advBalPaymentNotification.trnDoubleExpAdvBalPayment.Nofitication;
                    tfatAlt.FMPCEXE = mModel.advBalPaymentNotification.trnDoubleExpAdvBalPayment.Email;
                    tfatAlt.FMPCEXP = mModel.advBalPaymentNotification.trnDoubleExpAdvBalPayment.Priority;
                    tfatAlt.FMPCEXSelf = mModel.advBalPaymentNotification.trnDoubleExpAdvBalPayment.EnteredBy;
                    tfatAlt.FMPCEXU = mModel.advBalPaymentNotification.trnDoubleExpAdvBalPayment.User ?? "";
                    tfatAlt.FMPCEXBranch = mModel.advBalPaymentNotification.trnDoubleExpAdvBalPayment.Branch ?? "";
                    tfatAlt.FMPCDEXN = mModel.advBalPaymentNotification.trnParticularDoubleExpAdvBalPayment.Nofitication;
                    tfatAlt.FMPCDEXE = mModel.advBalPaymentNotification.trnParticularDoubleExpAdvBalPayment.Email;
                    tfatAlt.FMPCDEXP = mModel.advBalPaymentNotification.trnParticularDoubleExpAdvBalPayment.Priority;
                    tfatAlt.FMPCDEXSelf = mModel.advBalPaymentNotification.trnParticularDoubleExpAdvBalPayment.EnteredBy;
                    tfatAlt.FMPCDEXU = mModel.advBalPaymentNotification.trnParticularDoubleExpAdvBalPayment.User ?? "";
                    tfatAlt.FMPCDEXBranch = mModel.advBalPaymentNotification.trnParticularDoubleExpAdvBalPayment.Branch ?? "";
                    tfatAlt.FMPCDEXExpenses = mModel.advBalPaymentNotification.trnParticularDoubleExpAdvBalPayment.Expense ?? "";
                    
                    //Invoice
                    tfatAlt.INVBDN = mModel.billNotification.trnBackDaysBill.Nofitication;
                    tfatAlt.INVBDE = mModel.billNotification.trnBackDaysBill.Email;
                    tfatAlt.INVBDP = mModel.billNotification.trnBackDaysBill.Priority;
                    tfatAlt.INVBDSelf = mModel.billNotification.trnBackDaysBill.EnteredBy;
                    tfatAlt.INVBDU = mModel.billNotification.trnBackDaysBill.User ?? "";
                    tfatAlt.INVBDBranch = mModel.billNotification.trnBackDaysBill.Branch ?? "";
                    tfatAlt.INVBDDays = mModel.billNotification.trnBackDaysBill.BackDays;
                    tfatAlt.INVFDN = mModel.billNotification.trnForwardDaysBill.Nofitication;
                    tfatAlt.INVFDE = mModel.billNotification.trnForwardDaysBill.Email;
                    tfatAlt.INVFDP = mModel.billNotification.trnForwardDaysBill.Priority;
                    tfatAlt.INVFDSelf = mModel.billNotification.trnForwardDaysBill.EnteredBy;
                    tfatAlt.INVFDU = mModel.billNotification.trnForwardDaysBill.User ?? "";
                    tfatAlt.INVFDBranch = mModel.billNotification.trnForwardDaysBill.Branch ?? "";
                    tfatAlt.INVFDDays = mModel.billNotification.trnForwardDaysBill.ForwardDays;
                    tfatAlt.INVDAN = mModel.billNotification.trnDocAmountBill.Nofitication;
                    tfatAlt.INVDAE = mModel.billNotification.trnDocAmountBill.Email;
                    tfatAlt.INVDAP = mModel.billNotification.trnDocAmountBill.Priority;
                    tfatAlt.INVDASelf = mModel.billNotification.trnDocAmountBill.EnteredBy;
                    tfatAlt.INVDAU = mModel.billNotification.trnDocAmountBill.User ?? "";
                    tfatAlt.INVDABranch = mModel.billNotification.trnDocAmountBill.Branch ?? "";
                    tfatAlt.INVDADocAmt = mModel.billNotification.trnDocAmountBill.DocAmount;
                    tfatAlt.INVZAN = mModel.billNotification.trnZeroAmountBill.Nofitication;
                    tfatAlt.INVZAE = mModel.billNotification.trnZeroAmountBill.Email;
                    tfatAlt.INVZAP = mModel.billNotification.trnZeroAmountBill.Priority;
                    tfatAlt.INVZASelf = mModel.billNotification.trnZeroAmountBill.EnteredBy;
                    tfatAlt.INVZAU = mModel.billNotification.trnZeroAmountBill.User ?? "";
                    tfatAlt.INVZABranch = mModel.billNotification.trnZeroAmountBill.Branch ?? "";
                    tfatAlt.INVOPN = mModel.billNotification.trnOtherPartyBill.Nofitication;
                    tfatAlt.INVOPE = mModel.billNotification.trnOtherPartyBill.Email;
                    tfatAlt.INVOPP = mModel.billNotification.trnOtherPartyBill.Priority;
                    tfatAlt.INVOPSelf = mModel.billNotification.trnOtherPartyBill.EnteredBy;
                    tfatAlt.INVOPU = mModel.billNotification.trnOtherPartyBill.User ?? "";
                    tfatAlt.INVOPBranch = mModel.billNotification.trnOtherPartyBill.Branch ?? "";
                    tfatAlt.INVCDN = mModel.billNotification.trnConsignmentBill.Nofitication;
                    tfatAlt.INVCDE = mModel.billNotification.trnConsignmentBill.Email;
                    tfatAlt.INVCDP = mModel.billNotification.trnConsignmentBill.Priority;
                    tfatAlt.INVCDSelf = mModel.billNotification.trnConsignmentBill.EnteredBy;
                    tfatAlt.INVCDU = mModel.billNotification.trnConsignmentBill.User ?? "";
                    tfatAlt.INVCDBranch = mModel.billNotification.trnConsignmentBill.Branch ?? "";
                    tfatAlt.INVCDDays = mModel.billNotification.trnConsignmentBill.Days;
                    tfatAlt.INVPTN = mModel.billNotification.trnPartyBill.Nofitication;
                    tfatAlt.INVPTE = mModel.billNotification.trnPartyBill.Email;
                    tfatAlt.INVPTP = mModel.billNotification.trnPartyBill.Priority;
                    tfatAlt.INVPTSelf = mModel.billNotification.trnPartyBill.EnteredBy;
                    tfatAlt.INVPTU = mModel.billNotification.trnPartyBill.User ?? "";
                    tfatAlt.INVPTBranch = mModel.billNotification.trnPartyBill.Branch ?? "";
                    tfatAlt.INVPTParty = mModel.billNotification.trnPartyBill.Party ?? "";
                    tfatAlt.INVCEXN = mModel.billNotification.trnDoubleExpBill.Nofitication;
                    tfatAlt.INVCEXE = mModel.billNotification.trnDoubleExpBill.Email;
                    tfatAlt.INVCEXP = mModel.billNotification.trnDoubleExpBill.Priority;
                    tfatAlt.INVCEXSelf = mModel.billNotification.trnDoubleExpBill.EnteredBy;
                    tfatAlt.INVCEXU = mModel.billNotification.trnDoubleExpBill.User ?? "";
                    tfatAlt.INVCEXBranch = mModel.billNotification.trnDoubleExpBill.Branch ?? "";
                    tfatAlt.INVCDEXN = mModel.billNotification.trnParticularDoubleExpBill.Nofitication;
                    tfatAlt.INVCDEXE = mModel.billNotification.trnParticularDoubleExpBill.Email;
                    tfatAlt.INVCDEXP = mModel.billNotification.trnParticularDoubleExpBill.Priority;
                    tfatAlt.INVCDEXSelf = mModel.billNotification.trnParticularDoubleExpBill.EnteredBy;
                    tfatAlt.INVCDEXU = mModel.billNotification.trnParticularDoubleExpBill.User ?? "";
                    tfatAlt.INVCDEXBranch = mModel.billNotification.trnParticularDoubleExpBill.Branch ?? "";
                    tfatAlt.INVCDEXExpenses = mModel.billNotification.trnParticularDoubleExpBill.Expense ?? "";
                    
                    //Cash Bank Transaction
                    tfatAlt.CPOBDN = mModel.cashBankPaymentNotification.trnBackDaysCashBank.Nofitication;
                    tfatAlt.CPOBDE = mModel.cashBankPaymentNotification.trnBackDaysCashBank.Email;
                    tfatAlt.CPOBDP = mModel.cashBankPaymentNotification.trnBackDaysCashBank.Priority;
                    tfatAlt.CPOBDSelf = mModel.cashBankPaymentNotification.trnBackDaysCashBank.EnteredBy;
                    tfatAlt.CPOBDU = mModel.cashBankPaymentNotification.trnBackDaysCashBank.User ?? "";
                    tfatAlt.CPOBDBranch = mModel.cashBankPaymentNotification.trnBackDaysCashBank.Branch ?? "";
                    tfatAlt.CPOBDDays = mModel.cashBankPaymentNotification.trnBackDaysCashBank.BackDays;
                    tfatAlt.CPOFDN = mModel.cashBankPaymentNotification.trnForwardDaysCashBank.Nofitication;
                    tfatAlt.CPOFDE = mModel.cashBankPaymentNotification.trnForwardDaysCashBank.Email;
                    tfatAlt.CPOFDP = mModel.cashBankPaymentNotification.trnForwardDaysCashBank.Priority;
                    tfatAlt.CPOFDSelf = mModel.cashBankPaymentNotification.trnForwardDaysCashBank.EnteredBy;
                    tfatAlt.CPOFDU = mModel.cashBankPaymentNotification.trnForwardDaysCashBank.User ?? "";
                    tfatAlt.CPOFDBranch = mModel.cashBankPaymentNotification.trnForwardDaysCashBank.Branch ?? "";
                    tfatAlt.CPOFDDays = mModel.cashBankPaymentNotification.trnForwardDaysCashBank.ForwardDays;
                    tfatAlt.CPODAN = mModel.cashBankPaymentNotification.trnDocAmountCashBank.Nofitication;
                    tfatAlt.CPODAE = mModel.cashBankPaymentNotification.trnDocAmountCashBank.Email;
                    tfatAlt.CPODAP = mModel.cashBankPaymentNotification.trnDocAmountCashBank.Priority;
                    tfatAlt.CPODASelf = mModel.cashBankPaymentNotification.trnDocAmountCashBank.EnteredBy;
                    tfatAlt.CPODAU = mModel.cashBankPaymentNotification.trnDocAmountCashBank.User ?? "";
                    tfatAlt.CPODABranch = mModel.cashBankPaymentNotification.trnDocAmountCashBank.Branch ?? "";
                    tfatAlt.CPODADocAmt = mModel.cashBankPaymentNotification.trnDocAmountCashBank.DocAmount;
                    tfatAlt.CPOCEXN = mModel.cashBankPaymentNotification.trnDoubleExpCashBank.Nofitication;
                    tfatAlt.CPOCEXE = mModel.cashBankPaymentNotification.trnDoubleExpCashBank.Email;
                    tfatAlt.CPOCEXP = mModel.cashBankPaymentNotification.trnDoubleExpCashBank.Priority;
                    tfatAlt.CPOCEXSelf = mModel.cashBankPaymentNotification.trnDoubleExpCashBank.EnteredBy;
                    tfatAlt.CPOCEXU = mModel.cashBankPaymentNotification.trnDoubleExpCashBank.User ?? "";
                    tfatAlt.CPOCEXBranch = mModel.cashBankPaymentNotification.trnDoubleExpCashBank.Branch ?? "";
                    tfatAlt.CPOCDEXN = mModel.cashBankPaymentNotification.trnParticularDoubleExpCashBank.Nofitication;
                    tfatAlt.CPOCDEXE = mModel.cashBankPaymentNotification.trnParticularDoubleExpCashBank.Email;
                    tfatAlt.CPOCDEXP = mModel.cashBankPaymentNotification.trnParticularDoubleExpCashBank.Priority;
                    tfatAlt.CPOCDEXSelf = mModel.cashBankPaymentNotification.trnParticularDoubleExpCashBank.EnteredBy;
                    tfatAlt.CPOCDEXU = mModel.cashBankPaymentNotification.trnParticularDoubleExpCashBank.User ?? "";
                    tfatAlt.CPOCDEXBranch = mModel.cashBankPaymentNotification.trnParticularDoubleExpCashBank.Branch ?? "";
                    tfatAlt.CPOCDEXExpenses = mModel.cashBankPaymentNotification.trnParticularDoubleExpCashBank.Expense ?? "";

                    //Cash Bank Transaction(JV)
                    tfatAlt.COTBDN = mModel.cashBankJVPaymentNotification.trnBackDaysCashBankJV.Nofitication;
                    tfatAlt.COTBDE = mModel.cashBankJVPaymentNotification.trnBackDaysCashBankJV.Email;
                    tfatAlt.COTBDP = mModel.cashBankJVPaymentNotification.trnBackDaysCashBankJV.Priority;
                    tfatAlt.COTBDU = mModel.cashBankJVPaymentNotification.trnBackDaysCashBankJV.User ?? "";
                    tfatAlt.COTBDDays = mModel.cashBankJVPaymentNotification.trnBackDaysCashBankJV.BackDays;
                    tfatAlt.COTFDN = mModel.cashBankJVPaymentNotification.trnForwardDaysCashBankJV.Nofitication;
                    tfatAlt.COTFDE = mModel.cashBankJVPaymentNotification.trnForwardDaysCashBankJV.Email;
                    tfatAlt.COTFDP = mModel.cashBankJVPaymentNotification.trnForwardDaysCashBankJV.Priority;
                    tfatAlt.COTFDU = mModel.cashBankJVPaymentNotification.trnForwardDaysCashBankJV.User ?? "";
                    tfatAlt.COTFDDays = mModel.cashBankJVPaymentNotification.trnForwardDaysCashBankJV.ForwardDays;
                    tfatAlt.COTDAN = mModel.cashBankJVPaymentNotification.trnDocAmountCashBankJV.Nofitication;
                    tfatAlt.COTDAE = mModel.cashBankJVPaymentNotification.trnDocAmountCashBankJV.Email;
                    tfatAlt.COTDAP = mModel.cashBankJVPaymentNotification.trnDocAmountCashBankJV.Priority;
                    tfatAlt.COTDAU = mModel.cashBankJVPaymentNotification.trnDocAmountCashBankJV.User ?? "";
                    tfatAlt.COTDADocAmt = mModel.cashBankJVPaymentNotification.trnDocAmountCashBankJV.DocAmount;
                    tfatAlt.COTCEXN = mModel.cashBankJVPaymentNotification.trnDoubleExpCashBankJV.Nofitication;
                    tfatAlt.COTCEXE = mModel.cashBankJVPaymentNotification.trnDoubleExpCashBankJV.Email;
                    tfatAlt.COTCEXP = mModel.cashBankJVPaymentNotification.trnDoubleExpCashBankJV.Priority;
                    tfatAlt.COTCEXSelf = mModel.cashBankJVPaymentNotification.trnDoubleExpCashBankJV.EnteredBy;
                    tfatAlt.COTCEXU = mModel.cashBankJVPaymentNotification.trnDoubleExpCashBankJV.User ?? "";
                    tfatAlt.COTCEXBranch = mModel.cashBankJVPaymentNotification.trnDoubleExpCashBankJV.Branch ?? "";
                    tfatAlt.COTCDEXN = mModel.cashBankJVPaymentNotification.trnParticularDoubleExpCashBankJV.Nofitication;
                    tfatAlt.COTCDEXE = mModel.cashBankJVPaymentNotification.trnParticularDoubleExpCashBankJV.Email;
                    tfatAlt.COTCDEXP = mModel.cashBankJVPaymentNotification.trnParticularDoubleExpCashBankJV.Priority;
                    tfatAlt.COTCDEXSelf = mModel.cashBankJVPaymentNotification.trnParticularDoubleExpCashBankJV.EnteredBy;
                    tfatAlt.COTCDEXU = mModel.cashBankJVPaymentNotification.trnParticularDoubleExpCashBankJV.User ?? "";
                    tfatAlt.COTCDEXBranch = mModel.cashBankJVPaymentNotification.trnParticularDoubleExpCashBankJV.Branch ?? "";
                    tfatAlt.COTCDEXExpenses = mModel.cashBankJVPaymentNotification.trnParticularDoubleExpCashBankJV.Expense ?? "";
                    
                    //Credit Purchase
                    tfatAlt.PURBDN = mModel.creditPurchaseNotification.trnBackDaysCreditPurchase.Nofitication;
                    tfatAlt.PURBDE = mModel.creditPurchaseNotification.trnBackDaysCreditPurchase.Email;
                    tfatAlt.PURBDP = mModel.creditPurchaseNotification.trnBackDaysCreditPurchase.Priority;
                    tfatAlt.PURBDSelf = mModel.creditPurchaseNotification.trnBackDaysCreditPurchase.EnteredBy;
                    tfatAlt.PURBDU = mModel.creditPurchaseNotification.trnBackDaysCreditPurchase.User ?? "";
                    tfatAlt.PURBDBranch = mModel.creditPurchaseNotification.trnBackDaysCreditPurchase.Branch ?? "";
                    tfatAlt.PURBDDays = mModel.creditPurchaseNotification.trnBackDaysCreditPurchase.BackDays;
                    tfatAlt.PURFDN = mModel.creditPurchaseNotification.trnForwardDaysCreditPurchase.Nofitication;
                    tfatAlt.PURFDE = mModel.creditPurchaseNotification.trnForwardDaysCreditPurchase.Email;
                    tfatAlt.PURFDP = mModel.creditPurchaseNotification.trnForwardDaysCreditPurchase.Priority;
                    tfatAlt.PURFDSelf = mModel.creditPurchaseNotification.trnForwardDaysCreditPurchase.EnteredBy;
                    tfatAlt.PURFDU = mModel.creditPurchaseNotification.trnForwardDaysCreditPurchase.User ?? "";
                    tfatAlt.PURFDBranch = mModel.creditPurchaseNotification.trnForwardDaysCreditPurchase.Branch ?? "";
                    tfatAlt.PURFDDays = mModel.creditPurchaseNotification.trnForwardDaysCreditPurchase.ForwardDays;
                    tfatAlt.PURDAN = mModel.creditPurchaseNotification.trnDocAmountCreditPurchase.Nofitication;
                    tfatAlt.PURDAE = mModel.creditPurchaseNotification.trnDocAmountCreditPurchase.Email;
                    tfatAlt.PURDAP = mModel.creditPurchaseNotification.trnDocAmountCreditPurchase.Priority;
                    tfatAlt.PURDASelf = mModel.creditPurchaseNotification.trnDocAmountCreditPurchase.EnteredBy;
                    tfatAlt.PURDAU = mModel.creditPurchaseNotification.trnDocAmountCreditPurchase.User ?? "";
                    tfatAlt.PURDABranch = mModel.creditPurchaseNotification.trnDocAmountCreditPurchase.Branch ?? "";
                    tfatAlt.PURDADocAmt = mModel.creditPurchaseNotification.trnDocAmountCreditPurchase.DocAmount;
                    tfatAlt.PURVRN = mModel.creditPurchaseNotification.trnVendorCreditPurchase.Nofitication;
                    tfatAlt.PURVRE = mModel.creditPurchaseNotification.trnVendorCreditPurchase.Email;
                    tfatAlt.PURVRP = mModel.creditPurchaseNotification.trnVendorCreditPurchase.Priority;
                    tfatAlt.PURVRSelf = mModel.creditPurchaseNotification.trnVendorCreditPurchase.EnteredBy;
                    tfatAlt.PURVRU = mModel.creditPurchaseNotification.trnVendorCreditPurchase.User ?? "";
                    tfatAlt.PURVRBranch = mModel.creditPurchaseNotification.trnVendorCreditPurchase.Branch ?? "";
                    tfatAlt.PURVRVendor = mModel.creditPurchaseNotification.trnVendorCreditPurchase.Vendor ?? "";
                    tfatAlt.PURCEXN = mModel.creditPurchaseNotification.trnDoubleExpPurchase.Nofitication;
                    tfatAlt.PURCEXE = mModel.creditPurchaseNotification.trnDoubleExpPurchase.Email;
                    tfatAlt.PURCEXP = mModel.creditPurchaseNotification.trnDoubleExpPurchase.Priority;
                    tfatAlt.PURCEXSelf = mModel.creditPurchaseNotification.trnDoubleExpPurchase.EnteredBy;
                    tfatAlt.PURCEXU = mModel.creditPurchaseNotification.trnDoubleExpPurchase.User ?? "";
                    tfatAlt.PURCEXBranch = mModel.creditPurchaseNotification.trnDoubleExpPurchase.Branch ?? "";
                    tfatAlt.PURCDEXN = mModel.creditPurchaseNotification.trnParticularDoubleExpPurchase.Nofitication;
                    tfatAlt.PURCDEXE = mModel.creditPurchaseNotification.trnParticularDoubleExpPurchase.Email;
                    tfatAlt.PURCDEXP = mModel.creditPurchaseNotification.trnParticularDoubleExpPurchase.Priority;
                    tfatAlt.PURCDEXSelf = mModel.creditPurchaseNotification.trnParticularDoubleExpPurchase.EnteredBy;
                    tfatAlt.PURCDEXU = mModel.creditPurchaseNotification.trnParticularDoubleExpPurchase.User ?? "";
                    tfatAlt.PURCDEXBranch = mModel.creditPurchaseNotification.trnParticularDoubleExpPurchase.Branch ?? "";
                    tfatAlt.PURCDEXExpenses = mModel.creditPurchaseNotification.trnParticularDoubleExpPurchase.Expense ?? "";
                    
                    //Creditor Payment
                    tfatAlt.BPMBDN = mModel.creditPaymentNotification.trnBackDaysCreditPayment.Nofitication;
                    tfatAlt.BPMBDE = mModel.creditPaymentNotification.trnBackDaysCreditPayment.Email;
                    tfatAlt.BPMBDP = mModel.creditPaymentNotification.trnBackDaysCreditPayment.Priority;
                    tfatAlt.BPMBDSelf = mModel.creditPaymentNotification.trnBackDaysCreditPayment.EnteredBy;
                    tfatAlt.BPMBDU = mModel.creditPaymentNotification.trnBackDaysCreditPayment.User ?? "";
                    tfatAlt.BPMBDBranch = mModel.creditPaymentNotification.trnBackDaysCreditPayment.Branch ?? "";
                    tfatAlt.BPMBDDays = mModel.creditPaymentNotification.trnBackDaysCreditPayment.BackDays;
                    tfatAlt.BPMFDN = mModel.creditPaymentNotification.trnForwardDaysCreditPayment.Nofitication;
                    tfatAlt.BPMFDE = mModel.creditPaymentNotification.trnForwardDaysCreditPayment.Email;
                    tfatAlt.BPMFDP = mModel.creditPaymentNotification.trnForwardDaysCreditPayment.Priority;
                    tfatAlt.BPMFDSelf = mModel.creditPaymentNotification.trnForwardDaysCreditPayment.EnteredBy;
                    tfatAlt.BPMFDU = mModel.creditPaymentNotification.trnForwardDaysCreditPayment.User ?? "";
                    tfatAlt.BPMFDBranch = mModel.creditPaymentNotification.trnForwardDaysCreditPayment.Branch ?? "";
                    tfatAlt.BPMFDDays = mModel.creditPaymentNotification.trnForwardDaysCreditPayment.ForwardDays;
                    tfatAlt.BPMDAN = mModel.creditPaymentNotification.trnDocAmountCreditPayment.Nofitication;
                    tfatAlt.BPMDAE = mModel.creditPaymentNotification.trnDocAmountCreditPayment.Email;
                    tfatAlt.BPMDAP = mModel.creditPaymentNotification.trnDocAmountCreditPayment.Priority;
                    tfatAlt.BPMDASelf = mModel.creditPaymentNotification.trnDocAmountCreditPayment.EnteredBy;
                    tfatAlt.BPMDAU = mModel.creditPaymentNotification.trnDocAmountCreditPayment.User ?? "";
                    tfatAlt.BPMDABranch = mModel.creditPaymentNotification.trnDocAmountCreditPayment.Branch ?? "";
                    tfatAlt.BPMDADocAmt = mModel.creditPaymentNotification.trnDocAmountCreditPayment.DocAmount;
                    tfatAlt.BPMVRN = mModel.creditPaymentNotification.trnVendorCreditPayment.Nofitication;
                    tfatAlt.BPMVRE = mModel.creditPaymentNotification.trnVendorCreditPayment.Email;
                    tfatAlt.BPMVRP = mModel.creditPaymentNotification.trnVendorCreditPayment.Priority;
                    tfatAlt.BPMVRSelf = mModel.creditPaymentNotification.trnVendorCreditPayment.EnteredBy;
                    tfatAlt.BPMVRU = mModel.creditPaymentNotification.trnVendorCreditPayment.User ?? "";
                    tfatAlt.BPMVRBranch = mModel.creditPaymentNotification.trnVendorCreditPayment.Branch ?? "";
                    tfatAlt.BPMVRVendor = mModel.creditPaymentNotification.trnVendorCreditPayment.Vendor ?? "";
                    
                    //Bank Receipt
                    tfatAlt.BRCBDN = mModel.bankReceiptNotification.trnBackDaysBankReceipt.Nofitication;
                    tfatAlt.BRCBDE = mModel.bankReceiptNotification.trnBackDaysBankReceipt.Email;
                    tfatAlt.BRCBDP = mModel.bankReceiptNotification.trnBackDaysBankReceipt.Priority;
                    tfatAlt.BRCBDSelf = mModel.bankReceiptNotification.trnBackDaysBankReceipt.EnteredBy;
                    tfatAlt.BRCBDU = mModel.bankReceiptNotification.trnBackDaysBankReceipt.User ?? "";
                    tfatAlt.BRCBDBranch = mModel.bankReceiptNotification.trnBackDaysBankReceipt.Branch ?? "";
                    tfatAlt.BRCBDDays = mModel.bankReceiptNotification.trnBackDaysBankReceipt.BackDays;
                    tfatAlt.BRCFDN = mModel.bankReceiptNotification.trnForwardDaysBankReceipt.Nofitication;
                    tfatAlt.BRCFDE = mModel.bankReceiptNotification.trnForwardDaysBankReceipt.Email;
                    tfatAlt.BRCFDP = mModel.bankReceiptNotification.trnForwardDaysBankReceipt.Priority;
                    tfatAlt.BRCFDSelf = mModel.bankReceiptNotification.trnForwardDaysBankReceipt.EnteredBy;
                    tfatAlt.BRCFDU = mModel.bankReceiptNotification.trnForwardDaysBankReceipt.User ?? "";
                    tfatAlt.BRCFDBranch = mModel.bankReceiptNotification.trnForwardDaysBankReceipt.Branch ?? "";
                    tfatAlt.BRCFDDays = mModel.bankReceiptNotification.trnForwardDaysBankReceipt.ForwardDays;
                    tfatAlt.BRCDAN = mModel.bankReceiptNotification.trnDocAmountBankReceipt.Nofitication;
                    tfatAlt.BRCDAE = mModel.bankReceiptNotification.trnDocAmountBankReceipt.Email;
                    tfatAlt.BRCDAP = mModel.bankReceiptNotification.trnDocAmountBankReceipt.Priority;
                    tfatAlt.BRCDASelf = mModel.bankReceiptNotification.trnDocAmountBankReceipt.EnteredBy;
                    tfatAlt.BRCDAU = mModel.bankReceiptNotification.trnDocAmountBankReceipt.User ?? "";
                    tfatAlt.BRCDABranch = mModel.bankReceiptNotification.trnDocAmountBankReceipt.Branch ?? "";
                    tfatAlt.BRCDADocAmt = mModel.bankReceiptNotification.trnDocAmountBankReceipt.DocAmount;
                    tfatAlt.BRCUAN = mModel.bankReceiptNotification.trnPendingAdjustBankReceipt.Nofitication;
                    tfatAlt.BRCUAE = mModel.bankReceiptNotification.trnPendingAdjustBankReceipt.Email;
                    tfatAlt.BRCUAP = mModel.bankReceiptNotification.trnPendingAdjustBankReceipt.Priority;
                    tfatAlt.BRCUASelf = mModel.bankReceiptNotification.trnPendingAdjustBankReceipt.EnteredBy;
                    tfatAlt.BRCUAU = mModel.bankReceiptNotification.trnPendingAdjustBankReceipt.User ?? "";
                    tfatAlt.BRCUABranch = mModel.bankReceiptNotification.trnPendingAdjustBankReceipt.Branch ?? "";
                    tfatAlt.BRCFRN = mModel.bankReceiptNotification.trnFreightRebateAmountBankReceipt.Nofitication;
                    tfatAlt.BRCFRE = mModel.bankReceiptNotification.trnFreightRebateAmountBankReceipt.Email;
                    tfatAlt.BRCFRP = mModel.bankReceiptNotification.trnFreightRebateAmountBankReceipt.Priority;
                    tfatAlt.BRCFRSelf = mModel.bankReceiptNotification.trnFreightRebateAmountBankReceipt.EnteredBy;
                    tfatAlt.BRCFRU = mModel.bankReceiptNotification.trnFreightRebateAmountBankReceipt.User ?? "";
                    tfatAlt.BRCFRBranch = mModel.bankReceiptNotification.trnFreightRebateAmountBankReceipt.Branch ?? "";
                    tfatAlt.BRCFRFreightRebate = mModel.bankReceiptNotification.trnFreightRebateAmountBankReceipt.FreightRebateAmount;
                    
                    //Trop Sheet
                    tfatAlt.TRIPBDN = mModel.tripSheetNotification.trnBackDaysTripSheet.Nofitication;
                    tfatAlt.TRIPBDE = mModel.tripSheetNotification.trnBackDaysTripSheet.Email;
                    tfatAlt.TRIPBDP = mModel.tripSheetNotification.trnBackDaysTripSheet.Priority;
                    tfatAlt.TRIPBDSelf = mModel.tripSheetNotification.trnBackDaysTripSheet.EnteredBy;
                    tfatAlt.TRIPBDU = mModel.tripSheetNotification.trnBackDaysTripSheet.User ?? "";
                    tfatAlt.TRIPBDBranch = mModel.tripSheetNotification.trnBackDaysTripSheet.Branch ?? "";
                    tfatAlt.TRIPBDDays = mModel.tripSheetNotification.trnBackDaysTripSheet.BackDays;
                    tfatAlt.TRIPFDN = mModel.tripSheetNotification.trnForwardDaysTripSheet.Nofitication;
                    tfatAlt.TRIPFDE = mModel.tripSheetNotification.trnForwardDaysTripSheet.Email;
                    tfatAlt.TRIPFDP = mModel.tripSheetNotification.trnForwardDaysTripSheet.Priority;
                    tfatAlt.TRIPFDSelf = mModel.tripSheetNotification.trnForwardDaysTripSheet.EnteredBy;
                    tfatAlt.TRIPFDU = mModel.tripSheetNotification.trnForwardDaysTripSheet.User ?? "";
                    tfatAlt.TRIPFDBranch = mModel.tripSheetNotification.trnForwardDaysTripSheet.Branch ?? "";
                    tfatAlt.TRIPFDDays = mModel.tripSheetNotification.trnForwardDaysTripSheet.ForwardDays;
                    tfatAlt.TRIPDAN = mModel.tripSheetNotification.trnDocAmountTripSheet.Nofitication;
                    tfatAlt.TRIPDAE = mModel.tripSheetNotification.trnDocAmountTripSheet.Email;
                    tfatAlt.TRIPDAP = mModel.tripSheetNotification.trnDocAmountTripSheet.Priority;
                    tfatAlt.TRIPDASelf = mModel.tripSheetNotification.trnDocAmountTripSheet.EnteredBy;
                    tfatAlt.TRIPDAU = mModel.tripSheetNotification.trnDocAmountTripSheet.User ?? "";
                    tfatAlt.TRIPDABranch = mModel.tripSheetNotification.trnDocAmountTripSheet.Branch ?? "";
                    tfatAlt.TRIPDADocAmt = mModel.tripSheetNotification.trnDocAmountTripSheet.DocAmount;
                    tfatAlt.TRIPEEN = mModel.tripSheetNotification.trnEtraExpTripSheet.Nofitication;
                    tfatAlt.TRIPEEE = mModel.tripSheetNotification.trnEtraExpTripSheet.Email;
                    tfatAlt.TRIPEEP = mModel.tripSheetNotification.trnEtraExpTripSheet.Priority;
                    tfatAlt.TRIPEESelf = mModel.tripSheetNotification.trnEtraExpTripSheet.EnteredBy;
                    tfatAlt.TRIPEEU = mModel.tripSheetNotification.trnEtraExpTripSheet.User ?? "";
                    tfatAlt.TRIPEEBranch = mModel.tripSheetNotification.trnEtraExpTripSheet.Branch ?? "";
                    tfatAlt.TRIPEEExtraExpAmt = mModel.tripSheetNotification.trnEtraExpTripSheet.ExtraExp;
                    tfatAlt.TRIPFMN = mModel.tripSheetNotification.trnFmDateRangeTripSheet.Nofitication;
                    tfatAlt.TRIPFME = mModel.tripSheetNotification.trnFmDateRangeTripSheet.Email;
                    tfatAlt.TRIPFMP = mModel.tripSheetNotification.trnFmDateRangeTripSheet.Priority;
                    tfatAlt.TRIPFMSelf = mModel.tripSheetNotification.trnFmDateRangeTripSheet.EnteredBy;
                    tfatAlt.TRIPFMU = mModel.tripSheetNotification.trnFmDateRangeTripSheet.User ?? "";
                    tfatAlt.TRIPFMBranch = mModel.tripSheetNotification.trnFmDateRangeTripSheet.Branch ?? "";
                    tfatAlt.TRIPADVN = mModel.tripSheetNotification.trnADVDateRangeTripSheet.Nofitication;
                    tfatAlt.TRIPADVE = mModel.tripSheetNotification.trnADVDateRangeTripSheet.Email;
                    tfatAlt.TRIPADVP = mModel.tripSheetNotification.trnADVDateRangeTripSheet.Priority;
                    tfatAlt.TRIPADVSelf = mModel.tripSheetNotification.trnADVDateRangeTripSheet.EnteredBy;
                    tfatAlt.TRIPADVU = mModel.tripSheetNotification.trnADVDateRangeTripSheet.User ?? "";
                    tfatAlt.TRIPADVBranch = mModel.tripSheetNotification.trnADVDateRangeTripSheet.Branch ?? "";
                    tfatAlt.TRIPCCN = mModel.tripSheetNotification.trnCCDateRangeTripSheet.Nofitication;
                    tfatAlt.TRIPCCE = mModel.tripSheetNotification.trnCCDateRangeTripSheet.Email;
                    tfatAlt.TRIPCCP = mModel.tripSheetNotification.trnCCDateRangeTripSheet.Priority;
                    tfatAlt.TRIPCCSelf = mModel.tripSheetNotification.trnCCDateRangeTripSheet.EnteredBy;
                    tfatAlt.TRIPCCU = mModel.tripSheetNotification.trnCCDateRangeTripSheet.User ?? "";
                    tfatAlt.TRIPCCBranch = mModel.tripSheetNotification.trnCCDateRangeTripSheet.Branch ?? "";
                    tfatAlt.TRIPCEXN = mModel.tripSheetNotification.trnDoubleExpTripSheet.Nofitication;
                    tfatAlt.TRIPCEXE = mModel.tripSheetNotification.trnDoubleExpTripSheet.Email;
                    tfatAlt.TRIPCEXP = mModel.tripSheetNotification.trnDoubleExpTripSheet.Priority;
                    tfatAlt.TRIPCEXSelf = mModel.tripSheetNotification.trnDoubleExpTripSheet.EnteredBy;
                    tfatAlt.TRIPCEXU = mModel.tripSheetNotification.trnDoubleExpTripSheet.User ?? "";
                    tfatAlt.TRIPCEXBranch = mModel.tripSheetNotification.trnDoubleExpTripSheet.Branch ?? "";
                    tfatAlt.TRIPCDEXN = mModel.tripSheetNotification.trnParticularDoubleExpTripSheet.Nofitication;
                    tfatAlt.TRIPCDEXE = mModel.tripSheetNotification.trnParticularDoubleExpTripSheet.Email;
                    tfatAlt.TRIPCDEXP = mModel.tripSheetNotification.trnParticularDoubleExpTripSheet.Priority;
                    tfatAlt.TRIPCDEXSelf = mModel.tripSheetNotification.trnParticularDoubleExpTripSheet.EnteredBy;
                    tfatAlt.TRIPCDEXU = mModel.tripSheetNotification.trnParticularDoubleExpTripSheet.User ?? "";
                    tfatAlt.TRIPCDEXBranch = mModel.tripSheetNotification.trnParticularDoubleExpTripSheet.Branch ?? "";
                    tfatAlt.TRIPCDEXExpenses = mModel.tripSheetNotification.trnParticularDoubleExpTripSheet.Expense ?? "";

                    tfatAlt.AUTHIDS = muserid;
                    tfatAlt.AUTHORISE = mauthorise;
                    tfatAlt.ENTEREDBY = muserid;
                    tfatAlt.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                    if (mAdd == false)
                    {
                        ctxTFAT.Entry(tfatAlt).State = EntityState.Modified;
                    }
                    else
                    {
                        ctxTFAT.tfatAltNotification.Add(tfatAlt);
                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mAdd == true ? "Add" : "Edit", mModel.Header, "", DateTime.Now, 0, "", "ALT-Trasaction", "NA");

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