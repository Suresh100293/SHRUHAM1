using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class PeriodLockVM
    {



        public string Rules_Type { get; set; }
        public string BranchName { get; set; }
        public string CurrBranch { get; set; }
        public List<DateTime> PeriodList { get; set; }
        public DateTime Date { get; set; }
        public string MonthName { get; set; }
        public bool Lock { get; set; }
        public int Day { get; set; }





        // iX9: Common default Fields
        public int Document { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Mode { get; set; }
        public string Header { get; set; }
        public string ViewDataId { get; set; }
        public string OptionCode { get; set; }

        // iX9: special lists used for Grid2Grid relation type of interface
        public string DocTypes_Code { get; set; }
        public string DocTypes_Name { get; set; }
        public string DocTypes_MainType { get; set; }
        public string DocTypes_SubType { get; set; }

        public List<PeriodLockVM> mLeftList { get; set; }
        public List<PeriodLockVM> mRightList { get; set; }
    }
}