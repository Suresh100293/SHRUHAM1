using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class AssignTasksVM
    {
        // iX9: Field Structure of Task
        public int Task_RECORDKEY { get; set; }
        public bool Task_Accepted { get; set; }
        public System.DateTime Task_aEndDate { get; set; }
        public string Task_aEndDateVM { get; set; }
        public System.DateTime Task_aEndTime { get; set; }
        public string Task_aEndTimeVM { get; set; }
        public int Task_aPeriod { get; set; }
        public string Task_aPeriodString { get; set; }
        public string Task_AssignedBy { get; set; }
        public string Task_AssignedTo { get; set; }
        public System.DateTime Task_aStartDate { get; set; }
        public string Task_aStartDateVM { get; set; }
        public System.DateTime Task_aStartTime { get; set; }
        public string Task_aStartTimeVM { get; set; }
        public decimal Task_BillAmount { get; set; }
        public int Task_Code { get; set; }
        public decimal Task_Cost { get; set; }
        public string Task_DaysOfWeek { get; set; }
        public string Task_Descr { get; set; }
        public System.DateTime Task_DocDate { get; set; }
        public string Task_DocDateVM { get; set; }
        public bool Task_EmailReminder { get; set; }
        public System.DateTime Task_EndDate { get; set; }
        public string Task_EndDateVM { get; set; }
        public string Task_EndTime { get; set; }
        public string Task_EndTimeVM { get; set; }
        public bool Task_IsRecurring { get; set; }
        public System.DateTime Task_LastSent { get; set; }
        public string Task_LastSentVM { get; set; }
        public string Task_Narr { get; set; }
        public bool Task_nChoice { get; set; }
        public int Task_nDays { get; set; }
        public int Task_nListM1 { get; set; }
        public int Task_nListM2 { get; set; }
        public int Task_nListM3 { get; set; }
        public string Task_Occurs { get; set; }
        public string Task_Priority { get; set; }
        public bool Task_ReadFlag { get; set; }
        public bool Task_ReAssigned { get; set; }
        public int Task_ReAssignedID { get; set; }
        public string Task_RefDoc { get; set; }
        public string Task_Reference { get; set; }
        public bool Task_ReminderDone { get; set; }
        public bool Task_Read { get; set; }
        public bool Task_ScreenReminder { get; set; }
        public bool Task_SMSReminder { get; set; }
        public System.DateTime Task_StartDate { get; set; }
        public string Task_StartDateVM { get; set; }
        public string Task_StartTime { get; set; }
        public string Task_StartTimeVM { get; set; }
        public string Task_Status { get; set; }
        public int Task_TaskCode { get; set; }
        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public List<SelectListItem> PriorityList { get; set; }
        public List<SelectListItem> StatusList { get; set; }
        public List<SelectListItem> ReferenceList { get; set; }
        public string TaskCodeName { get; set; }
        public List<SelectListItem> AssignedToMultiX { get; set; }
        public string AssignedToItemX { get; set; }
        public string Task_EscalateTo { get; set; }
        public string Task_EscalateToN { get; set; }

        public string RefDocNo { get; set; }
        public string RefDocNoName { get; set; }


        // iX9: Common default Fields
        public int Document { get; set; }
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
}