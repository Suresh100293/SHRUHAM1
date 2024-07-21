using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using System.IO;
using System.Configuration;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class AssignTasksController : BaseController
    {
        string attachmentPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentPath"].ToString()) == true ? "" : ConfigurationManager.AppSettings["AttachmentPath"].ToString();

        ////private IBusinessCommon mBuss = new BusinessCommon();
        private int mlocation = 100001;
        private int mnewrecordkey = 0;
        private static string mauthorise = "A00";
        private static int mdocument = 0;

        #region GetLists
        public List<SelectListItem> GetPriorityList()
        {
            List<SelectListItem> CallPriorityList = new List<SelectListItem>();
            CallPriorityList.Add(new SelectListItem { Value = "Low", Text = "Low" });
            CallPriorityList.Add(new SelectListItem { Value = "Medium", Text = "Medium" });
            CallPriorityList.Add(new SelectListItem { Value = "Medium High", Text = "Medium High" });
            CallPriorityList.Add(new SelectListItem { Value = "High", Text = "High" });
            CallPriorityList.Add(new SelectListItem { Value = "Very High", Text = "Very High" });
            return CallPriorityList;
        }
        public List<SelectListItem> GetStatusList()
        {
            List<SelectListItem> CallStatusList = new List<SelectListItem>();
            CallStatusList.Add(new SelectListItem { Value = "Pending", Text = "Pending" });
            CallStatusList.Add(new SelectListItem { Value = "Complete", Text = "Complete" });
            CallStatusList.Add(new SelectListItem { Value = "On Hold", Text = "On Hold" });
            CallStatusList.Add(new SelectListItem { Value = "Cancelled", Text = "Cancelled" });
            CallStatusList.Add(new SelectListItem { Value = "In-Progress", Text = "In-Progress" });
            CallStatusList.Add(new SelectListItem { Value = "Re-Scheduled", Text = "Re-Scheduled" });
            return CallStatusList;
        }

        public List<SelectListItem> GetReferenceList()
        {
            List<SelectListItem> CallReferenceList = new List<SelectListItem>();
            CallReferenceList.Add(new SelectListItem { Value = "LR", Text = "Lorry Receipt" });
            CallReferenceList.Add(new SelectListItem { Value = "LC", Text = "Lorry Challan" });
            CallReferenceList.Add(new SelectListItem { Value = "FM", Text = "Freight Memo" });
            CallReferenceList.Add(new SelectListItem { Value = "Bill", Text = "Bill" });
            CallReferenceList.Add(new SelectListItem { Value = "Vehicle", Text = "Vehicle" });
            CallReferenceList.Add(new SelectListItem { Value = "Driver", Text = "Driver" });
            CallReferenceList.Add(new SelectListItem { Value = "Customer", Text = "Customer" });
            CallReferenceList.Add(new SelectListItem { Value = "Consigner", Text = "Consigner" });
            CallReferenceList.Add(new SelectListItem { Value = "Consignee", Text = "Consignee" });
            CallReferenceList.Add(new SelectListItem { Value = "PickUpOrder", Text = "Pick Up Order" });
            //CallReferenceList.Add(new SelectListItem { Value = "Campaigns", Text = "Campaigns" });
            //CallReferenceList.Add(new SelectListItem { Value = "Waves", Text = "Waves" });
            //CallReferenceList.Add(new SelectListItem { Value = "Leads", Text = "Leads" });
            //CallReferenceList.Add(new SelectListItem { Value = "Contacts", Text = "Contacts" });
            //CallReferenceList.Add(new SelectListItem { Value = "Item", Text = "Item" });
            //CallReferenceList.Add(new SelectListItem { Value = "Opportunity", Text = "Opportunity" });
            //CallReferenceList.Add(new SelectListItem { Value = "Quotes", Text = "Quotes" });
            //CallReferenceList.Add(new SelectListItem { Value = "Orders", Text = "Orders" });
            //CallReferenceList.Add(new SelectListItem { Value = "Sales", Text = "Sales" });
            //CallReferenceList.Add(new SelectListItem { Value = "Task", Text = "Task" });
            //CallReferenceList.Add(new SelectListItem { Value = "Bills", Text = "Bills" });
            //CallReferenceList.Add(new SelectListItem { Value = "Projects", Text = "Projects" });
            return CallReferenceList;
        }
        public JsonResult ReferenceDocList(string term, string Type)
        {
            if (Type == "Vehicle")
            {
                var List = ctxTFAT.VehicleMaster.Where(x => x.Acitve == true).Select(x => new { x.TruckNo,x.Code }).ToList();
                List.AddRange(ctxTFAT.HireVehicleMaster.Where(x => x.Acitve == true).Select(x => new { x.TruckNo, x.Code }).ToList());
                if (!String.IsNullOrEmpty(term))
                {
                    List = List.Where(x => x.TruckNo.Contains(term)).ToList();
                }
                
                return Json((from m in List
                             select new { Name = m.TruckNo, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
            }
            else if (Type == "Driver")
            {
                var List = ctxTFAT.DriverMaster.Where(x => x.Status == true).ToList();
                if (!String.IsNullOrEmpty(term))
                {
                    List = List.Where(x => x.Name.Contains(term)).ToList();
                }
                return Json((from m in List
                             select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
            }
            else if (Type == "Customer")
            {
                var List = ctxTFAT.CustomerMaster.Where(x => x.NonActive == false).ToList();
                if (!String.IsNullOrEmpty(term))
                {
                    List = List.Where(x => x.Name.Contains(term)).ToList();
                }
                return Json((from m in List
                             select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
            }
            else if (Type == "Consigner" || Type == "Consignee")
            {
                var List = ctxTFAT.Consigner.Where(x => x.Acitve == true).ToList();
                if (!String.IsNullOrEmpty(term))
                {
                    List = List.Where(x => x.Name.Contains(term)).ToList();
                }
                return Json((from m in List
                             select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
            }
            return null;
        }
        public JsonResult AutoCompleteTaskCode(string term)
        {
            if (String.IsNullOrEmpty(term))
            {
                return Json((from m in ctxTFAT.TaskMaster
                             select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json((from m in ctxTFAT.TaskMaster
                             where m.Name.ToLower().Contains(term.ToLower())
                             select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult AutoCompleteTask_EscalateTo(string term)
        {
            return Json((from m in ctxTFAT.TfatPass
                         where m.Name.ToLower().Contains(term.ToLower())
                         select new { Name = m.Name, Code = m.Code }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        #endregion GetLists

        // GET: Logistics/AssignTasks
        public ActionResult Index(AssignTasksVM mModel)
        {
            Session["TempAttach"] = null;
            GetAllMenu(Session["ModuleName"].ToString());
            //AccountingGetAllMenu(Session["ModuleName"].ToString());
            //VehicleGetAllMenu(Session["ModuleName"].ToString());
            //GeneralGetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, "Assign Tasks", "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;
            mModel.Task_DocDate = DateTime.Now;
            mModel.Task_StartDate = DateTime.Now;
            mModel.Task_StartTime = DateTime.Now.ToString("HH:mm");
            mModel.Task_EndDate = DateTime.Now;
            mModel.Task_EndTime = DateTime.Now.ToString("HH:mm");
            mModel.Task_aStartDate = DateTime.Now;
            mModel.Task_aStartTime = DateTime.Now;
            mModel.Task_aEndDate = DateTime.Now;
            mModel.Task_aEndTime = DateTime.Now;
            mModel.PriorityList = GetPriorityList();
            mModel.StatusList = GetStatusList();
            mModel.ReferenceList = GetReferenceList();

            List<SelectListItem> AssignedToList = new List<SelectListItem>();
            var AssignedToResultX = ctxTFAT.TfatPass.Select(x => new { Code = x.Code, Name = x.Name }).ToList().Distinct();
            foreach (var AssignedToitem in AssignedToResultX)
            {
                AssignedToList.Add(new SelectListItem { Text = AssignedToitem.Name, Value = AssignedToitem.Code.ToString() });
            }
            mModel.AssignedToMultiX = AssignedToList;

            mModel.Task_Code = mModel.Document;
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.Task.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                if (mList != null)
                {
                    AttachmentVM Att = new AttachmentVM();
                    Att.Type = "Task";
                    Att.Srl = mList.Code.ToString();

                    AttachmentController attachmentC = new AttachmentController();
                    List<AttachmentVM> attachments = attachmentC.GetAttachmentListInEdit(Att);
                    Session["TempAttach"] = attachments;


                    var mTaskCode = ctxTFAT.TaskMaster.Where(x => x.Code == mList.TaskCode).Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                    mModel.Task_TaskCode = mTaskCode != null ? mTaskCode.Code : 0;
                    mModel.TaskCodeName = mTaskCode != null ? mTaskCode.Name : "";
                    mModel.Task_Code = mList.Code;
                    mModel.Task_Descr = mList.Descr;
                    mModel.Task_DocDate = mList.DocDate != null ? mList.DocDate : DateTime.Now;
                    mModel.Task_Priority = mList.Priority;
                    mModel.Task_Status = mList.Status;
                    mModel.Task_StartDate = mList.StartDate != null ? mList.StartDate : DateTime.Now;
                    mModel.Task_StartTime = mList.StartTime != null ? mList.StartTime.ToString("HH:mm") : DateTime.Now.ToString("HH:mm");
                    mModel.Task_EndDate = mList.EndDate != null ? mList.EndDate.Value : DateTime.Now;
                    mModel.Task_EndTime = mList.EndTime != null ? mList.EndTime.Value.ToString("HH:mm") : DateTime.Now.ToString("HH:mm");
                    mModel.Task_EmailReminder = mList.EmailReminder;
                    mModel.Task_SMSReminder = mList.SMSReminder;
                    mModel.Task_ScreenReminder = mList.ScreenReminder;
                    mModel.Task_aStartDate = mList.aStartDate != null ? mList.aStartDate.Value : DateTime.Now;
                    mModel.Task_aStartTime = mList.aStartTime != null ? mList.aStartTime.Value : DateTime.Now;
                    mModel.Task_aEndDate = mList.aEndDate != null ? mList.aEndDate.Value : DateTime.Now;
                    mModel.Task_aEndTime = mList.aEndTime != null ? mList.aEndTime.Value : DateTime.Now;
                    mModel.Task_AssignedTo = mList.AssignedTo;
                    mModel.Task_Reference = mList.Reference;
                    mModel.Task_Read = mList.Read;
                   
                    if (mList.Reference == "Vehicle" || mList.Reference == "Driver" || mList.Reference == "Customer" || mList.Reference == "Consigner" || mList.Reference == "Consignee")
                    {
                        if (mList.Reference == "Vehicle")
                        {
                            if (String.IsNullOrEmpty(mList.RefDoc)==false && mList.RefDoc.Contains("H"))
                            {
                                mModel.RefDocNo = mList.RefDoc;
                                mModel.RefDocNoName = ctxTFAT.HireVehicleMaster.Where(x => x.Code == mList.RefDoc).Select(x => x.TruckNo).FirstOrDefault();
                            }
                            else
                            {
                                mModel.RefDocNo = mList.RefDoc;
                                mModel.RefDocNoName = ctxTFAT.VehicleMaster.Where(x => x.Code == mList.RefDoc).Select(x => x.TruckNo).FirstOrDefault();
                            }
                            
                        }
                        else if (mList.Reference == "Driver")
                        {
                            mModel.RefDocNo = mList.RefDoc;
                            mModel.RefDocNoName = ctxTFAT.DriverMaster.Where(x => x.Code == mList.RefDoc).Select(x => x.Name).FirstOrDefault();
                        }
                        else if (mList.Reference == "Customer")
                        {
                            mModel.RefDocNo = mList.RefDoc;
                            mModel.RefDocNoName = ctxTFAT.CustomerMaster.Where(x => x.Code == mList.RefDoc).Select(x => x.Name).FirstOrDefault();
                        }
                        else if (mList.Reference == "Consigner" || mList.Reference == "Consignee")
                        {
                            mModel.RefDocNo = mList.RefDoc;
                            mModel.RefDocNoName = ctxTFAT.Consigner.Where(x => x.Code == mList.RefDoc).Select(x => x.Name).FirstOrDefault();
                        }
                    }
                    else
                    {
                        mModel.Task_RefDoc = mList.RefDoc;
                    }
                    mModel.Task_Narr = mList.Narr;
                    mModel.Task_EscalateTo = mList.AssignedBy;
                    mModel.Task_EscalateToN = ctxTFAT.TfatPass.Where(x => x.Code == mList.AssignedBy).Select(x => x.Name).FirstOrDefault();

                }
            }
            else
            {
                var mTaskCode = ctxTFAT.TaskMaster.Where(x => x.Code.ToString() == "100008").Select(x => new { Name = x.Name, Code = x.Code }).FirstOrDefault();
                mModel.Task_TaskCode = mTaskCode != null ? mTaskCode.Code : 0;
                mModel.TaskCodeName = mTaskCode != null ? mTaskCode.Name : "";
                mModel.Task_Priority = "Low";
                mModel.Task_Status = "Pending";
                mModel.Task_EscalateTo = ctxTFAT.TfatPass.Where(x => x.Code == muserid).Select(x => x.Code).FirstOrDefault();
                mModel.Task_EscalateToN = ctxTFAT.TfatPass.Where(x => x.Code == muserid).Select(x => x.Name).FirstOrDefault();
                mModel.Task_StartTime = DateTime.Now.ToString("HH:mm");
                mModel.Task_EndTime = DateTime.Now.ToString("HH:mm");
                mModel.Task_Read = false;
            }
            // No ADD mode applicable

            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(AssignTasksVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteAssignTasks(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    Task mobj = new Task();
                    bool mAdd = true;
                    if (ctxTFAT.Task.Where(x => (x.Code == mModel.Task_Code)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.Task.Where(x => (x.Code == mModel.Task_Code)).FirstOrDefault();
                        mAdd = false;
                    }


                    var Date = mModel.Task_StartDateVM.Split('/');
                    string[] Time = mModel.Task_StartTime.Split(':');
                    DateTime StartTime = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Time[0]), Convert.ToInt32(Time[1]), 00);

                    var Date1 = mModel.Task_EndDateVM.Split('/');
                    string[] Time1 = mModel.Task_EndTime.Split(':');
                    DateTime EndTime = new DateTime(Convert.ToInt32(Date1[2]), Convert.ToInt32(Date1[1]), Convert.ToInt32(Date1[0]), Convert.ToInt32(Time1[0]), Convert.ToInt32(Time1[1]), 00);



                    mobj.Code = mModel.Task_Code;
                    mobj.Descr = mModel.Task_Descr;
                    mobj.TaskCode = mModel.Task_TaskCode;
                    mobj.DocDate = ConvertDDMMYYTOYYMMDD(mModel.Task_DocDateVM);
                    mobj.Priority = mModel.Task_Priority;
                    mobj.Status = mModel.Task_Status;
                    mobj.StartDate = ConvertDDMMYYTOYYMMDD(mModel.Task_StartDateVM);
                    mobj.EndDate = ConvertDDMMYYTOYYMMDD(mModel.Task_EndDateVM);

                    mobj.StartTime = StartTime;
                    mobj.EndTime = EndTime;
                    mobj.Read = mModel.Task_Read;

                    mobj.EmailReminder = mModel.Task_EmailReminder;
                    mobj.SMSReminder = mModel.Task_SMSReminder;
                    mobj.ScreenReminder = mModel.Task_ScreenReminder;
                    if (mModel.Task_aStartDateVM != null)
                    {
                        mobj.aStartDate = ConvertDDMMYYTOYYMMDD(mModel.Task_aStartDateVM);
                        mobj.aStartTime = ConvertDDMMYYTOYYMMDD(mModel.Task_aStartTimeVM);
                    }
                    if (mModel.Task_aEndDateVM != null)
                    {
                        mobj.aEndDate = ConvertDDMMYYTOYYMMDD(mModel.Task_aEndDateVM);
                        mobj.aEndTime = ConvertDDMMYYTOYYMMDD(mModel.Task_aEndTimeVM);
                    }

                    mobj.AssignedTo = mModel.Task_AssignedTo;
                    mobj.AssignedBy = mModel.Task_AssignedBy;
                    mobj.Reference = mModel.Task_Reference;
                    mobj.RefDoc = mModel.Task_RefDoc;
                    // iX9: default values for the fields not used @Form
                    mobj.Accepted = false;
                    mobj.aPeriod = 0;
                    mobj.aPeriodString = "";

                    mobj.BillAmount = 0;
                    mobj.Cost = 0;
                    mobj.DaysOfWeek = 0;
                    mobj.IsRecurring = false;
                    mobj.LastSent = System.DateTime.Now;
                    mobj.Narr = mModel.Task_Narr;
                    mobj.nChoice = false;
                    mobj.nDays = 0;
                    mobj.nListM1 = 0;
                    mobj.nListM2 = 0;
                    mobj.nListM3 = 0;
                    mobj.Occurs = "";
                    mobj.ReadFlag = false;
                    mobj.ReAssigned = false;
                    mobj.ReAssignedID = 0;
                    mobj.ReminderDone = false;
                    // iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = System.DateTime.Now;
                    if (mAdd == true)
                    {
                        mobj.Code = GetNextCode();
                    }
                    if (mAdd == true)
                    {
                        ctxTFAT.Task.Add(mobj);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }

                    AttachmentVM vM = new AttachmentVM();
                    vM.ParentKey = "TSK00" + mperiod.Substring(0, 2) + 1.ToString("D3") + mobj.Code;
                    vM.Srl = mobj.Code.ToString();
                    SaveAttachment(vM);


                    ctxTFAT.SaveChanges();
                    mnewrecordkey = mobj.RECORDKEY;
                    int mNewCode = 0;
                    mNewCode = mobj.Code;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, "Assign Tasks", "     " + mperiod.Substring(0, 2) + mNewCode.ToString(), DateTime.Now, 0, "", "", "NA");
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "AssignTasks" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "AssignTasks" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "AssignTasks" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "AssignTasks" }, JsonRequestBehavior.AllowGet);
        }

        public int GetNextCode()
        {
            var nextcode = (from x in ctxTFAT.Task select (int?)x.Code).Max() ?? 0;
            return (++nextcode);
        }

        public ActionResult DeleteAssignTasks(AssignTasksVM mModel)
        {
            // iX9: Check for Active Master Task
            string mactivestring = "";
            var mactive1 = ctxTFAT.Task.Where(x => (x.ReAssignedID == mModel.Task_Code)).Select(x => x.ReAssignedID).FirstOrDefault();
            if (mactive1 != null) { mactivestring = mactivestring + "\nTask: " + mactive1; }
            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "Active Item, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            var mList = ctxTFAT.Task.Where(x => (x.Code == mModel.Task_Code)).FirstOrDefault();
            ctxTFAT.Task.Remove(mList);
            ctxTFAT.SaveChanges();
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetDetails(AssignTasksVM mModel)
        {
            string Narr = "", Status = "Error";
            string Type = mModel.Task_Reference;
            if (Type == "LR")
            {
                LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == mModel.Task_RefDoc.Trim()).FirstOrDefault();
                if (lRMaster != null)
                {
                    Status = "Success";
                    Narr += "LRNO: " + lRMaster.LrNo + ",";
                    Narr += " Consignor: " + ctxTFAT.Consigner.Where(x => x.Code == lRMaster.RecCode).Select(x => x.Name).FirstOrDefault() + ", ";
                    Narr += " Consignee: " + ctxTFAT.Consigner.Where(x => x.Code == lRMaster.SendCode).Select(x => x.Name).FirstOrDefault() + ", ";
                    Narr += " From: " + ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Source).Select(x => x.Name).FirstOrDefault() + ", ";
                    Narr += " To: " + ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name).FirstOrDefault() + ", ";
                    Narr += " Qty: " + lRMaster.TotQty + ", ";
                    Narr += " Act Weight: " + lRMaster.ActWt + ", ";
                    Narr += " Chrg Weight: " + lRMaster.ChgWt;
                }
            }
            else if (Type == "LC")
            {
                LCMaster lCMaster = ctxTFAT.LCMaster.Where(x => x.LCno.ToString() == mModel.Task_RefDoc.Trim()).FirstOrDefault();
                if (lCMaster != null)
                {
                    Status = "Success";
                    Narr += "LCNO: " + lCMaster.LCno + ",";
                    Narr += " From: " + ctxTFAT.TfatBranch.Where(x => x.Code == lCMaster.FromBranch).Select(x => x.Name).FirstOrDefault() + ", ";
                    Narr += " To: " + ctxTFAT.TfatBranch.Where(x => x.Code == lCMaster.ToBranch).Select(x => x.Name).FirstOrDefault() + ", ";
                    Narr += " Qty: " + lCMaster.TotalQty + ", ";
                    Narr += " Dispatch FMNO: " + (lCMaster.DispachFM == 0 ? " Not Dispatch Yet." : lCMaster.DispachFM.ToString());
                }
            }
            else if (Type == "FM")
            {
                FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.FmNo.ToString() == mModel.Task_RefDoc.Trim()).FirstOrDefault();
                if (fMMaster != null)
                {
                    Status = "Success";
                    Narr += "FMNO: " + fMMaster.FmNo + ",";
                    Narr += " From: " + ctxTFAT.TfatBranch.Where(x => x.Code == fMMaster.FromBranch).Select(x => x.Name).FirstOrDefault() + ", ";
                    Narr += " To: " + ctxTFAT.TfatBranch.Where(x => x.Code == fMMaster.ToBranch).Select(x => x.Name).FirstOrDefault() + ", ";
                    if (!String.IsNullOrEmpty(fMMaster.RouteViaName))
                    {
                        Narr += " Via: " + fMMaster.RouteViaName + ", ";
                    }
                    Narr += "Vehicle No: " + fMMaster.TruckNo + ",";
                    Narr += " Driver: " + ctxTFAT.DriverMaster.Where(x => x.Code == fMMaster.Driver).Select(x => x.Name).FirstOrDefault() + ",";
                    Narr += " Freight: " + string.Format("{0:0.##}", fMMaster.Freight) + ",";
                    Narr += " Advance: " + string.Format("{0:0.##}", fMMaster.Adv);
                }
            }
            else if (Type == "Bill")
            {
                Sales sales = ctxTFAT.Sales.Where(x => x.BillNumber.ToString() == mModel.Task_RefDoc.Trim()).FirstOrDefault();
                if (sales != null)
                {
                    Status = "Success";
                    Narr += "BILL NO: " + sales.BillNumber + ",";
                    Narr += " Customer Name: " + ctxTFAT.CustomerMaster.Where(x => x.Code == sales.Code).Select(x => x.Name).FirstOrDefault() + ",";
                    Narr += " Amount: " + string.Format("{0:0.##}", sales.Amt.Value);
                }
            }
            else if (Type == "PickUpOrder")
            {
                LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == mModel.Task_RefDoc.Trim()).FirstOrDefault();
                if (lRMaster != null)
                {
                    Status = "Success";
                    Narr += "LRNO: " + lRMaster.LrNo + ",";
                    Narr += " Consignor: " + ctxTFAT.Consigner.Where(x => x.Code == lRMaster.RecCode).Select(x => x.Name).FirstOrDefault() + ", ";
                    Narr += " Consignee: " + ctxTFAT.Consigner.Where(x => x.Code == lRMaster.SendCode).Select(x => x.Name).FirstOrDefault() + ", ";
                    Narr += " From: " + ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Source).Select(x => x.Name).FirstOrDefault() + ", ";
                    Narr += " To: " + ctxTFAT.TfatBranch.Where(x => x.Code == lRMaster.Dest).Select(x => x.Name).FirstOrDefault() + ", ";
                    Narr += " Qty: " + lRMaster.TotQty + ", ";
                    Narr += " Act Weight: " + lRMaster.ActWt + ", ";
                    Narr += " Chrg Weight: " + lRMaster.ChgWt;
                }
            }
            else if (Type == "Vehicle")
            {
                VehicleMaster vehicleMaster = ctxTFAT.VehicleMaster.Where(x => x.Code == mModel.RefDocNo).FirstOrDefault();
                if (vehicleMaster != null)
                {
                    Status = "Success";
                    Narr += "Vehicle No: " + vehicleMaster.TruckNo + ",";
                    Narr += " Category: " + ctxTFAT.VehicleCategory.Where(x => x.Code == vehicleMaster.VCategory).Select(x => x.VehicleCategory1).FirstOrDefault() + ", ";
                    Narr += " Driver: " + ctxTFAT.DriverMaster.Where(x => x.Code == vehicleMaster.Driver).Select(x => x.Name).FirstOrDefault();
                }
                else
                {
                    HireVehicleMaster hireVehicleMaster = ctxTFAT.HireVehicleMaster.Where(x => x.Code == mModel.RefDocNo).FirstOrDefault();
                    if (hireVehicleMaster != null)
                    {
                        Status = "Success";
                        Narr += "Vehicle No: " + hireVehicleMaster.TruckNo + ",";
                        Narr += " Category: " + ctxTFAT.VehicleCategory.Where(x => x.Code == hireVehicleMaster.VCategory).Select(x => x.VehicleCategory1).FirstOrDefault() + ", ";
                        Narr += " Driver: " + hireVehicleMaster.Driver;
                    }
                }
            }
            else if (Type == "Driver")
            {
                DriverMaster driverMaster = ctxTFAT.DriverMaster.Where(x => x.Code == mModel.RefDocNo).FirstOrDefault();
                if (driverMaster != null)
                {
                    Status = "Success";
                    Narr += "Name: " + driverMaster.Name + ",";
                    Narr += " Contact No: " + driverMaster.MobileNo1 + "/" + driverMaster.MobileNo2 + ", ";
                    Narr += driverMaster.Name + " Active On " + driverMaster.VehicleNo + " This Vehicle.";
                }
            }
            else if (Type == "Customer")
            {
                CustomerMaster customerMaster = ctxTFAT.CustomerMaster.Where(x => x.Code == mModel.RefDocNo).FirstOrDefault();
                if (customerMaster != null)
                {
                    Status = "Success";
                    Narr += "Name: " + customerMaster.Name + ",";
                    Narr += " Contact Person Name: " + ctxTFAT.Caddress.Where(x => x.Code == customerMaster.Code).Select(x => x.Person).FirstOrDefault() + ",";
                    Narr += " Contact Person No: " + ctxTFAT.Caddress.Where(x => x.Code == customerMaster.Code).Select(x => x.ContMobile).FirstOrDefault() + ",";
                    Narr += " Address: " + ctxTFAT.Caddress.Where(x => x.Code == customerMaster.Code).Select(x => x.Adrl1).FirstOrDefault() + "," + ctxTFAT.Caddress.Where(x => x.Code == customerMaster.Code).Select(x => x.Adrl2).FirstOrDefault() + "," + ctxTFAT.Caddress.Where(x => x.Code == customerMaster.Code).Select(x => x.Adrl3).FirstOrDefault() + "," + ctxTFAT.Caddress.Where(x => x.Code == customerMaster.Code).Select(x => x.Adrl4).FirstOrDefault();
                }
            }
            else if (Type == "Consigner" || Type == "Consignee")
            {
                Consigner consigner = ctxTFAT.Consigner.Where(x => x.Code == mModel.RefDocNo).FirstOrDefault();
                if (consigner != null)
                {
                    Status = "Success";
                    Narr += "Name: " + consigner.Name + ",";
                    Narr += " Contact Person Name: " + consigner.ContactName + " / " + consigner.ContactName2 + ",";
                    Narr += " Contact Person No: " + consigner.ContactNO + " / " + consigner.ContactNO2 + ",";
                    Narr += " Address: " + consigner.Addr1+" "+ consigner.Addr2;
                }
            }


            return Json(new { Status = Status, Narr = Narr }, JsonRequestBehavior.AllowGet);
        }



        #endregion SaveData
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

        public void SaveAttachment(AttachmentVM Model)
        {
            var RemoveAttach = ctxTFAT.Attachment.Where(x => x.Type == "Task" && x.ParentKey == Model.ParentKey).ToList();
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
                //List<string> FileString = Model.AllFileStr.Split(',').ToList();
                //List<string> NameString = Model.FileNameStr.Split(',').ToList();

                var AttachCode = GetNewAttachCode();
                int c = 0;
                int an = 1;
                foreach (var item in DocList.Where(x => x.Type == "Task").ToList())
                {
                    Directory.CreateDirectory(attachmentPath + Model.ParentKey);
                    string directoryPath = attachmentPath + Model.ParentKey + "/" + item.FileName;
                    byte[] IData = Convert.FromBase64String(item.ImageStr);
                    Attachment att = new Attachment();
                    att.AUTHIDS = muserid;
                    att.AUTHORISE = mauthorise;
                    att.Branch = mbranchcode;
                    att.Code = AttachCode.ToString();
                    att.ENTEREDBY = muserid;
                    att.FilePath = directoryPath;
                    att.LASTUPDATEDATE = DateTime.Now;
                    att.LocationCode = Model.LocationCode;
                    att.Prefix = mperiod;
                    att.Sno = an;
                    att.Srl = Model.Srl;
                    att.SrNo = an;
                    //att.TableKey = Model.Type + Model.Prefix.Substring(0, 2) + an.ToString("D3") + Model.Srl;
                    att.TableKey = Model.ParentKey;
                    att.ParentKey = Model.ParentKey;
                    att.Type = "Task";
                    att.CompCode = mcompcode;
                    att.ExternalAttach = item.ExternalAttach;
                    att.RefDocNo = item.RefCode;
                    att.RefType = item.RefType;
                    ctxTFAT.Attachment.Add(att);
                    ctxTFAT.SaveChanges();
                    System.IO.File.WriteAllBytes(directoryPath, IData);
                    ++an;
                    ++c;
                    ++AttachCode;
                }
            }

        }
    }
}