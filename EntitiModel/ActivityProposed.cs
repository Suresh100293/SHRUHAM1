//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EntitiModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class ActivityProposed
    {
        public int RECORDKEY { get; set; }
        public int Code { get; set; }
        public bool ConvertActivity { get; set; }
        public Nullable<decimal> Cost { get; set; }
        public Nullable<int> Duration { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string Participants { get; set; }
        public string Place { get; set; }
        public string ProposedBy { get; set; }
        public string ReminderMode { get; set; }
        public Nullable<int> ReminderTick { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public string StartTime { get; set; }
        public int Type { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
    }
}
