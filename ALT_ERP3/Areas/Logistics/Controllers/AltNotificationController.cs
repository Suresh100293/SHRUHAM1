using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Controllers;
using Common;
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
    public class AltNotificationController : BaseController
    {
        List<string> SecondParameterReq = new List<string> { "Broker", "Vehicle", "Particular Charges", "Double Exp", "On Particular Expense", "Other Party", "Customer", "Vendor", "UnAdjust" };
        List<string> NoParameterReq = new List<string> { "Driver Lic", "Clear(Unload)", "Unload(Branch)", "Branch(Delivery)", "NO-Delivery", "Double Exp", "Zero Amount", "Other Party", "UnAdjust", "Freight Memo", "Advance", "Cost Center", };

        public List<SelectListItem> GetTypes()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem
            {
                Text = "CONSIGNMENT",
                Value = "CONSIGNMENT"
            });
            items.Add(new SelectListItem
            {
                Text = "LORRY CHALLAN",
                Value = "LORRY CHALLAN"
            });
            items.Add(new SelectListItem
            {
                Text = "FREIGHT MEMO",
                Value = "FREIGHT MEMO"
            });
            items.Add(new SelectListItem
            {
                Text = "VEHICLE ACTIVITY",
                Value = "VEHICLE ACTIVITY"
            });
            items.Add(new SelectListItem
            {
                Text = "DELIVERY",
                Value = "DELIVERY"
            });
            items.Add(new SelectListItem
            {
                Text = "POD",
                Value = "POD"
            });
            items.Add(new SelectListItem
            {
                Text = "BILL SUBMISSION",
                Value = "BILL SUBMISSION"
            });
            items.Add(new SelectListItem
            {
                Text = "DOC AUTHENTICATION",
                Value = "DOC AUTHENTICATION"
            });
            items.Add(new SelectListItem
            {
                Text = "ADVANCE BALANCE PAYMENT",
                Value = "ADVANCE BALANCE PAYMENT"
            });
            items.Add(new SelectListItem
            {
                Text = "INVOICE",
                Value = "INVOICE"
            });
            items.Add(new SelectListItem
            {
                Text = "CASH BANK PAYMENT",
                Value = "CASH BANK PAYMENT"
            });
            items.Add(new SelectListItem
            {
                Text = "CASH BANK PAYMENT (JV)",
                Value = "CASH BANK PAYMENT (JV)"
            });
            items.Add(new SelectListItem
            {
                Text = "CREDIT PURCHASE",
                Value = "CREDIT PURCHASE"
            });
            items.Add(new SelectListItem
            {
                Text = "CREDITOR PAYMENT",
                Value = "CREDITOR PAYMENT"
            });
            items.Add(new SelectListItem
            {
                Text = "BANK RECEIPRT",
                Value = "BANK RECEIPRT"
            });
            items.Add(new SelectListItem
            {
                Text = "TRIP SHEET",
                Value = "TRIP SHEET"
            });
            return items;
        }
        public JsonResult PopulateTypes(string term)
        {
            List<SelectListItem> items = GetTypes();

            if (!(String.IsNullOrEmpty(term)))
            {
                items = items.Where(x => x.Text.ToLower().Contains(term.ToLower())).ToList();
            }
            var Modified = items.Select(x => new
            {
                Code = x.Value,
                Name = x.Text
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public List<SelectListItem> GetSubTypes(string Type)
        {
            List<SelectListItem> items = new List<SelectListItem>();

            if (Type == "CONSIGNMENT")
            {
                items.Add(new SelectListItem
                {
                    Text = "Back Days",
                    Value = "Send Notification if Consignment Book Date is less Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Forward Days",
                    Value = "Send Notification if Consignment Book Date is more Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Declare Value",
                    Value = "Send Notification if Consignment Declare Value More Than Enterd Declare Value."
                });
                items.Add(new SelectListItem
                {
                    Text = "Description",
                    Value = "Send Notification if Consignment inlude Particular selected Description."
                });
                items.Add(new SelectListItem
                {
                    Text = "Eway Bill",
                    Value = "Send Notification if Consignment Declare Value More Than Enterd Declare Value and Eway Bill No Not Entered."
                });
            }
            else if (Type == "LORRY CHALLAN")
            {
                items.Add(new SelectListItem
                {
                    Text = "Back Days",
                    Value = "Send Notification if Lorry Challan Date is less Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Forward Days",
                    Value = "Send Notification if Lorry Challan Date is more Than No Of Days."
                });
            }
            else if (Type == "FREIGHT MEMO")
            {
                items.Add(new SelectListItem
                {
                    Text = "Back Days",
                    Value = "Send Notification if Freight Memo Date is less Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Forward Days",
                    Value = "Send Notification if Freight Memo Date is more Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Driver Lic",
                    Value = "Send Notification if Drivers License Exired In Freight Memo."
                });
                items.Add(new SelectListItem
                {
                    Text = "Doc Amount",
                    Value = "Send Notification if Freight Memo Freight More Than Enterd Freight Value."
                });
                items.Add(new SelectListItem
                {
                    Text = "Broker",
                    Value = "Send Notification if Freight Memo inlude Particular selected Broker."
                });
                items.Add(new SelectListItem
                {
                    Text = "Vehicle",
                    Value = "Send Notification if Freight Memo inlude Particular selected Vehicle."
                });

            }
            else if (Type == "VEHICLE ACTIVITY")
            {
                items.Add(new SelectListItem
                {
                    Text = "Arrival",
                    Value = "Send Notification if Vehicle Reached In Branch Late Upto Entered HH:MM."
                });
                items.Add(new SelectListItem
                {
                    Text = "Dispatch",
                    Value = "Send Notification if Vehicle Out From Branch Late Upto Entered HH:MM."
                });
                items.Add(new SelectListItem
                {
                    Text = "Over-Load",
                    Value = "Send Notification if Loading Weight Is Greter Than Entered Overload Weight."
                });
                items.Add(new SelectListItem
                {
                    Text = "Clear(Unload)",
                    Value = "Send Notification if Own Branch Material Not Unloading."
                });
                items.Add(new SelectListItem
                {
                    Text = "Unload(Branch)",
                    Value = "Send Notification if Unload Other Branch Material."
                });
            }
            else if (Type == "DELIVERY")
            {
                items.Add(new SelectListItem
                {
                    Text = "Delivery Status",
                    Value = "Send Notification if Delivery inlude Particular selected Status."
                });
                items.Add(new SelectListItem
                {
                    Text = "Branch(Delivery)",
                    Value = "Send Notification if Delivered Another Branch Material."
                });
            }
            else if (Type == "POD")
            {
                items.Add(new SelectListItem
                {
                    Text = "Back Days",
                    Value = "Send Notification if POD Date is less Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Forward Days",
                    Value = "Send Notification if POD Date is more Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Received Days",
                    Value = "Send Notification if POD Received Late As Compared To Entered Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Send Days",
                    Value = "Send Notification if POD Send Late As Compared To Entered Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "NO-Delivery",
                    Value = "Send Notification if Without Delivery POD Received."
                });
            }
            else if (Type == "BILL SUBMISSION")
            {
                items.Add(new SelectListItem
                {
                    Text = "Back Days",
                    Value = "Send Notification if Bill Submission Date is less Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Forward Days",
                    Value = "Send Notification if Bill Submission Date is more Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Late Submission",
                    Value = "Send Notification if Bill Submission Late As Compared (Bill Docdate) To Entered Days."
                });
            }
            else if (Type == "DOC AUTHENTICATION")
            {
                items.Add(new SelectListItem
                {
                    Text = "Late Authorise",
                    Value = "Send Notification if Document Authenticate Late As Compared To Entered Days."
                });
            }
            else if (Type == "ADVANCE BALANCE PAYMENT")
            {
                items.Add(new SelectListItem
                {
                    Text = "Back Days",
                    Value = "Send Notification if Advance-Balance Payment Date is less Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Forward Days",
                    Value = "Send Notification if Advance-Balance Payment Date is more Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Particular Charges",
                    Value = "Send Notification if Advance-Balance Payment inlude Particular selected Charges."
                });
                items.Add(new SelectListItem
                {
                    Text = "Double Exp",
                    Value = "Send Notification if Double Expenses Found ON Consigmnet."
                });
                items.Add(new SelectListItem
                {
                    Text = "On Particular Expense",
                    Value = "Send Notification if Double Expenses Found On Particular selected Expenses."
                });

            }
            else if (Type == "INVOICE")
            {
                items.Add(new SelectListItem
                {
                    Text = "Back Days",
                    Value = "Send Notification if Invoice Date is less Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Forward Days",
                    Value = "Send Notification if Invoice Date is more Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Doc Amount",
                    Value = "Send Notification if Invoice Amount More Than Enterd Doc Amount."
                });
                items.Add(new SelectListItem
                {
                    Text = "Zero Amount",
                    Value = "Send Notification if Invoice Amount Is Zero."
                });
                items.Add(new SelectListItem
                {
                    Text = "Other Party",
                    Value = "Send Notification if Invoice Generate On Another Party."
                });
                items.Add(new SelectListItem
                {
                    Text = "Consignment",
                    Value = "Send Notification if Difference Between Invoice Date And Consignment Date Is Greater Than Entered Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Customer",
                    Value = "Send Notification if Invoice inlude Particular selected Party."
                });
                items.Add(new SelectListItem
                {
                    Text = "Double Exp",
                    Value = "Send Notification if Double Expenses Found ON Consigmnet."
                });
                items.Add(new SelectListItem
                {
                    Text = "On Particular Expense",
                    Value = "Send Notification if Double Expenses Found On Particular selected Expenses."
                });
            }
            else if (Type == "CASH BANK PAYMENT")
            {
                items.Add(new SelectListItem
                {
                    Text = "Back Days",
                    Value = "Send Notification if CashBank Transaction Date is less Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Forward Days",
                    Value = "Send Notification if CashBank Transaction Date is more Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Doc Amount",
                    Value = "Send Notification if CashBank Transaction More Than Enterd Doc Amount."
                });
                items.Add(new SelectListItem
                {
                    Text = "Double Exp",
                    Value = "Send Notification if Double Expenses Found ON Consigmnet."
                });
                items.Add(new SelectListItem
                {
                    Text = "On Particular Expense",
                    Value = "Send Notification if Double Expenses Found On Particular selected Expenses."
                });
            }
            else if (Type == "CASH BANK PAYMENT (JV)")
            {
                items.Add(new SelectListItem
                {
                    Text = "Back Days",
                    Value = "Send Notification if CashBank JV Transaction Date is less Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Forward Days",
                    Value = "Send Notification if CashBank JV Transaction Date is more Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Doc Amount",
                    Value = "Send Notification if CashBank JV Transaction More Than Enterd Doc Amount."
                });
                items.Add(new SelectListItem
                {
                    Text = "Double Exp",
                    Value = "Send Notification if Double Expenses Found ON Consigmnet."
                });
                items.Add(new SelectListItem
                {
                    Text = "On Particular Expense",
                    Value = "Send Notification if Double Expenses Found On Particular selected Expenses."
                });
            }
            else if (Type == "CREDIT PURCHASE")
            {
                items.Add(new SelectListItem
                {
                    Text = "Back Days",
                    Value = "Send Notification if Credit Purchase Date is less Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Forward Days",
                    Value = "Send Notification if Credit Purchase Date is more Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Doc Amount",
                    Value = "Send Notification if Credit Purchase Amount More Than Enterd Doc Amount."
                });
                items.Add(new SelectListItem
                {
                    Text = "Vendor",
                    Value = "Send Notification if Credit Purchase inlude Particular selected Vendors."
                });
                items.Add(new SelectListItem
                {
                    Text = "Double Exp",
                    Value = "Send Notification if Double Expenses Found ON Consigmnet."
                });
                items.Add(new SelectListItem
                {
                    Text = "On Particular Expense",
                    Value = "Send Notification if Double Expenses Found On Particular selected Expenses."
                });
            }
            else if (Type == "CREDITOR PAYMENT")
            {
                items.Add(new SelectListItem
                {
                    Text = "Back Days",
                    Value = "Send Notification if Creditor Payment Date is less Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Forward Days",
                    Value = "Send Notification if Creditor Payment Date is more Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Doc Amount",
                    Value = "Send Notification if Creditor Payment Amount More Than Enterd Doc Amount."
                });
                items.Add(new SelectListItem
                {
                    Text = "Vendor",
                    Value = "Send Notification if Creditor Payment inlude Particular selected Vendors."
                });
            }
            else if (Type == "BANK RECEIPRT")
            {
                items.Add(new SelectListItem
                {
                    Text = "Back Days",
                    Value = "Send Notification if Bank Receipt Date is less Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Forward Days",
                    Value = "Send Notification if Bank Receipt Date is more Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Doc Amount",
                    Value = "Send Notification if Bank Receipt Amount More Than Enterd Doc Amount."
                });
                items.Add(new SelectListItem
                {
                    Text = "UnAdjust",
                    Value = "Send Notification if Bank Receipt Amount Not Adjust Properly."
                });
                items.Add(new SelectListItem
                {
                    Text = "Freight Rebate",
                    Value = "Send Notification if Bank Receipt FreightRebate Amount More Than Enterd Freight Rebate Amount."
                });

            }
            else if (Type == "TRIP SHEET")
            {
                items.Add(new SelectListItem
                {
                    Text = "Back Days",
                    Value = "Send Notification if Trip Sheet Date is less Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Forward Days",
                    Value = "Send Notification if Trip Sheet Date is more Than No Of Days."
                });
                items.Add(new SelectListItem
                {
                    Text = "Doc Amount",
                    Value = "Send Notification if Trip Sheet Amount More Than Enterd Doc Amount."
                });
                items.Add(new SelectListItem
                {
                    Text = "Extra Exp",
                    Value = "Send Notification if Trip Sheet Other Expenses Amount More Than Enterd ExtraExp Amount."
                });
                items.Add(new SelectListItem
                {
                    Text = "Freight Memo",
                    Value = "Send Notification if Trip Sheet Docdate Is Less Than Freight Memo Filter Date Range."
                });
                items.Add(new SelectListItem
                {
                    Text = "Advance",
                    Value = "Send Notification if Trip Sheet Docdate Is Less Than Advance Adjust Date Range."
                });
                items.Add(new SelectListItem
                {
                    Text = "Cost Center",
                    Value = "Send Notification if Trip Sheet Docdate Is Less Than Cost Center Adjust Date Range."
                });
                items.Add(new SelectListItem
                {
                    Text = "Double Exp",
                    Value = "Send Notification if Double Expenses Found ON Consigmnet."
                });
                items.Add(new SelectListItem
                {
                    Text = "On Particular Expense",
                    Value = "Send Notification if Double Expenses Found On Particular selected Expenses."
                });
            }

            return items;
        }
        public JsonResult PopulateSubType(string term, string Type)
        {
            List<SelectListItem> items = GetSubTypes(Type);

            if (!(String.IsNullOrEmpty(term)))
            {
                items = items.Where(x => x.Text.ToLower().Contains(term.ToLower())).ToList();
            }
            var Modified = items.Select(x => new
            {
                Code = x.Value,
                Name = x.Text
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }
        public string GetSubtypeValue(string DocType, string SubType)
        {
            return GetSubTypes(DocType).Where(y => y.Text == SubType).Select(y => y.Value).FirstOrDefault();

        }

        public string GetParameterType(string subType)
        {
            switch (subType)
            {
                case "Back Days":
                case "Forward Days":
                case "Received Days":
                case "Send Days":
                case "Late Submission":
                case "Late Authorise":
                case "Consignment":
                    return "Day";


                case "Declare Value":
                case "Eway Bill":
                case "Doc Amount":
                case "Freight Rebate":
                case "Extra Exp":
                case "Double Exp":
                    return "Money";


                case "Description":
                case "Broker":
                case "Vehicle":
                case "Delivery Status":
                case "Particular Charges":
                case "On Particular Expense":
                case "Customer":
                case "Vendor":
                    return "List";


                case "Arrival":
                case "Dispatch":
                    return "Time";

                case "Over-Load":
                    return "KG";

                case "Driver Lic":
                case "Clear(Unload)":
                case "Unload(Branch)":
                case "Branch(Delivery)":
                case "NO-Delivery":
                case "Zero Amount":
                case "Other Party":
                case "UnAdjust":
                case "Freight Memo":
                case "Advance":
                case "Cost Center":
                    return "NA";
                default:
                    return "NA";
            }
        }


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

        public List<SelectListItem> BindDynamicList(string SubType)
        {
            List<SelectListItem> items = new List<SelectListItem>();

            if (SubType == "Description")
            {
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
            }
            else if (SubType == "Broker")
            {
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
            }
            else if (SubType == "Vehicle")
            {
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
            }
            else if (SubType == "Delivery Status")
            {
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
            }
            else if (SubType == "Particular Charges")
            {
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
            }
            else if (SubType == "On Particular Expense")
            {
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
            }
            else if (SubType == "Customer")
            {
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
            }
            else if (SubType == "Vendor")
            {
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
            }
            return items;
        }

        // GET: Logistics/AltNotification
        public ActionResult Index(AltNotificationVM mModel)
        {
            Session["Notification"] = null;
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, mModel.Document, "", "AN");

            if (mModel.Mode == "Edit" || mModel.Mode == "View" || mModel.Mode == "Delete")
            {
                mModel.Type = mModel.Document;
                var list = ctxTFAT.tfatNotificationSetup.Where(x => x.Type == mModel.Type).Select(x => new AltNotificationVM
                {
                    RECORDKEY = x.RECORDKEY,
                    Type = x.Type,
                    SubTypeText = x.SubType,
                    Notification = x.Notification,
                    Email = x.Email,
                    Priority = x.Priority,
                    Self = x.Self,
                    User = x.User,
                    Branch = x.Branch,
                    Param1List = x.Paramlistreq,
                    Param1 = x.Param1,
                    Visible_Param2 = x.Param2Req,
                    Param2 = x.Param2
                }).ToList();
                list.ForEach(x => { x.Param1Type = GetParameterType(x.SubTypeText); x.SubType = GetSubtypeValue(x.Type, x.SubTypeText); x.Dynamiclist = BindDynamicList(x.SubTypeText); x.NoParameter = NoParameterReq.IndexOf(x.SubTypeText) == -1 ? false : true; });
                int SrNo = 1;
                foreach (var item in list)
                {
                    item.SrNo = SrNo++;
                }
                mModel.list = list;
                Session["Notification"] = list;
            }

            mModel.Branches = PopulateBranch();
            mModel.Users = PopulateUsers();

            return View(mModel);
        }

        #region GridOperations

        [HttpPost]
        public ActionResult AddNew(AltNotificationVM Model)
        {
            string Status = "Success", Message = "";
            Model.Branches = PopulateBranch();
            Model.Users = PopulateUsers();

            List<AltNotificationVM> objgriddetail = new List<AltNotificationVM>();
            if (Session["Notification"] != null)
            {
                objgriddetail = (List<AltNotificationVM>)Session["Notification"];
            }

            objgriddetail.Add(new AltNotificationVM()
            {
                SrNo = objgriddetail.Count + 1,
                CreateOn = DateTime.Now.ToString(),
                Type = Model.Type,
                SubType = Model.SubType,
                SubTypeText = Model.SubTypeText,
                Dynamiclist = BindDynamicList(Model.SubTypeText),
                Notification = Model.Notification,
                Email = Model.Email,
                Priority = Model.Priority,
                Self = Model.Self,
                Param1Type = GetParameterType(Model.SubTypeText),
                User = Model.User,
                Branch = Model.Branch,

                Param1List = Model.Param1List,
                Param1 = Model.Param1,
                Visible_Param2 = Model.Visible_Param2,
                Param2 = Model.Param2,
                NoParameter = Model.NoParameter

            });
            //Model = new AltNotificationVM();
            Model.list = objgriddetail;
            Model.Dynamiclist = BindDynamicList("");
            Model.Param1 = null;
            Model.User = null;
            Model.Branch = null;
            Model.Param2 = 0;
            Session.Add("Notification", objgriddetail);
            var html = ViewHelper.RenderPartialView(this, "_GridData", Model);

            return Json(new
            {
                Html = html,
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(AltNotificationVM Model)
        {
            Model.Branches = PopulateBranch();
            Model.Users = PopulateUsers();

            var result = (List<AltNotificationVM>)Session["Notification"];
            var result1 = result.Where(x => x.SrNo == Model.SrNo);
            foreach (var item in result1)
            {
                Model.SrNo = item.SrNo;
                Model.Type = item.Type;
                Model.SubType = item.SubType;
                Model.SubTypeText = item.SubTypeText;
                Model.Param1Type = GetParameterType(item.SubTypeText);
                Model.Dynamiclist = BindDynamicList(item.SubTypeText);
                Model.Param1List = item.Param1List;
                Model.Param1 = item.Param1;
                Model.Visible_Param2 = item.Visible_Param2;
                Model.Param2 = item.Param2;
                Model.Notification = item.Notification;
                Model.Email = item.Email;
                Model.Priority = item.Priority;
                Model.Self = item.Self;
                Model.User = item.User;
                Model.Branch = item.Branch;
                Model.NoParameter = item.NoParameter;
            }
            Model.list = result;


            return Json(new
            {
                Html = this.RenderPartialView("_GridData", Model),
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Update(AltNotificationVM Model)
        {
            Model.Branches = PopulateBranch();
            Model.Users = PopulateUsers();

            var result = (List<AltNotificationVM>)Session["Notification"];
            foreach (var item in result.Where(x => x.SrNo == Model.SrNo))
            {
                item.Type = Model.Type;
                item.SubType = Model.SubType;
                item.SubTypeText = Model.SubTypeText;
                item.Dynamiclist = BindDynamicList(Model.SubTypeText);
                item.Param1Type = GetParameterType(Model.SubTypeText);
                item.Param1List = Model.Param1List;
                item.Param1 = Model.Param1;
                item.Visible_Param2 = Model.Visible_Param2;
                item.Param2 = Model.Param2;
                item.Notification = Model.Notification;
                item.Email = Model.Email;
                item.Priority = Model.Priority;
                item.Self = Model.Self;
                item.User = Model.User;
                item.Branch = Model.Branch;
                item.NoParameter = Model.NoParameter;
            }

            Model.list = result;
            Model.Dynamiclist = BindDynamicList("");
            Model.Param1 = null;
            Model.User = null;
            Model.Branch = null;
            Session.Add("Notification", result);
            var html = ViewHelper.RenderPartialView(this, "_GridData", Model);

            return Json(new
            {
                Html = html,
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteGrid(int SrNo)
        {
            AltNotificationVM Model = new AltNotificationVM();
            Model.Branches = PopulateBranch();
            Model.Users = PopulateUsers();

            var result = (List<AltNotificationVM>)Session["Notification"];
            result = result.Where(x => x.SrNo != SrNo).ToList();
            int i = 1;
            foreach (var item in result)
            {
                item.SrNo = i++;
            }

            Model.list = result;
            Model.Dynamiclist = BindDynamicList("");
            Model.Param1 = null;
            Model.User = null;
            Model.Branch = null;
            Session.Add("Notification", result);
            var html = ViewHelper.RenderPartialView(this, "_GridData", Model);

            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Partial Methods

        public ActionResult DynamicParameter(AltNotificationVM Model)
        {
            Model.Dynamiclist = BindDynamicList(Model.SubType);
            if (Model.Dynamiclist.Count > 0)
            {
                Model.Param1List = true;
            }
            else
            {
                Model.Param1 = "0";
            }
            int index = SecondParameterReq.IndexOf(Model.SubType);
            if (index >= 0)
            {
                Model.Visible_Param2 = true;
            }

            index = NoParameterReq.IndexOf(Model.SubType);
            if (index >= 0)
            {
                Model.NoParameter = true;
            }
            Model.Param1Type = GetParameterType(Model.SubType);
            var html = ViewHelper.RenderPartialView(this, "_DynamicParameter", Model);
            return Json(new { Html = html, NoParameter = Model.NoParameter, SecondParameterReq = Model.Visible_Param2, Param1List = Model.Param1List }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ResetGrid(AltNotificationVM Model)
        {
            bool AllowToAdd = true;

            if (ctxTFAT.tfatNotificationSetup.Where(x => x.Type == Model.Type).FirstOrDefault() != null)
            {
                AllowToAdd = false;
            }
            Session["Notification"] = null;
            var html = ViewHelper.RenderPartialView(this, "_GridData", new AltNotificationVM() { Dynamiclist = BindDynamicList(""), Branches = PopulateBranch(), Users = PopulateUsers(), list = new List<AltNotificationVM>() });
            return Json(new
            {
                Html = html,
                AllowToAdd = AllowToAdd
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Save / Delete

        public ActionResult SaveData(AltNotificationVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    mModel.list = (List<AltNotificationVM>)Session["Notification"];
                    if (mModel.list != null)
                    {
                        foreach (var item in mModel.list)
                        {
                            bool mAdd = true;
                            tfatNotificationSetup tfatAlt = new tfatNotificationSetup();
                            if (ctxTFAT.tfatNotificationSetup.Where(x => x.RECORDKEY == item.RECORDKEY).FirstOrDefault() != null)
                            {
                                tfatAlt = ctxTFAT.tfatNotificationSetup.Where(x => x.RECORDKEY == item.RECORDKEY).FirstOrDefault();
                                mAdd = false;
                            }
                            tfatAlt.Type = item.Type;
                            tfatAlt.SubType = item.SubTypeText;
                            tfatAlt.Notification = item.Notification;
                            tfatAlt.Email = item.Email;
                            tfatAlt.Priority = item.Priority;
                            tfatAlt.Self = item.Self;
                            tfatAlt.User = item.User;
                            tfatAlt.Branch = item.Branch;

                            tfatAlt.Paramlistreq = item.Param1List;
                            tfatAlt.Param1 = item.Param1;

                            tfatAlt.Param2Req = item.Visible_Param2;
                            tfatAlt.Param2 = item.Param2;


                            tfatAlt.AUTHIDS = muserid;
                            tfatAlt.AUTHORISE = "A00";
                            tfatAlt.ENTEREDBY = muserid;
                            tfatAlt.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                            if (mAdd == false)
                            {
                                ctxTFAT.Entry(tfatAlt).State = EntityState.Modified;
                            }
                            else
                            {
                                tfatAlt.CreateDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                                ctxTFAT.tfatNotificationSetup.Add(tfatAlt);
                            }
                        }

                        var list = mModel.list.Where(x => x.RECORDKEY != 0).ToList();
                        var DBlist = ctxTFAT.tfatNotificationSetup.Where(x => x.Type == mModel.Type).ToList();
                        var result = (from dbItem in DBlist
                                      join listItem in list on dbItem.RECORDKEY equals listItem.RECORDKEY into gj
                                      from subListItem in gj.DefaultIfEmpty()
                                      where subListItem == null
                                      select dbItem
                                     ).ToList();
                        ctxTFAT.tfatNotificationSetup.RemoveRange(result);
                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "ALT-Trasaction", "NA");

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
    }
}