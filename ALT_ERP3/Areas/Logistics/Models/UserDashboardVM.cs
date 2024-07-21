using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class UserDashboardVM
    {
        public List<UserDashboardVM> mLeftList { get; set; }
        public List<UserDashboardVM> mRightList { get; set; }




        public string FromDate { get; set; }
        public string ToDate { get; set; }



        public List<SelectListItem> Users { get; set; }
        public string User { get; set; }
        public string UserL { get; set; }
        public string SelectedUserName { get; set; }

        public List<SelectListItem> BranchsOnly { get; set; }
        public string Branch { get; set; }
        public string BranchL { get; set; }
        public bool EmptyBranchEwayBill { get; set; }

        public List<SelectListItem> Customers { get; set; }
        public List<SelectListItem> Masters { get; set; }
        public string Master { get; set; }
        public string MasterL { get; set; }

        public List<SelectListItem> Vehicles { get; set; }
        public string Vehicle { get; set; }
        public string VehicleL { get; set; }

        public List<SelectListItem> Drivers { get; set; }
        public string Driver { get; set; }
        public string DriverL { get; set; }

        public List<SelectListItem> VehicleExpDues { get; set; }
        public string VehicleExpDue { get; set; }
        public string VehicleExpDueL { get; set; }

        public List<SelectListItem> VehicleMasters { get; set; }
        public string VehicleMaster { get; set; }
        public string VehicleMasterL { get; set; }




        public List<SelectListItem> Codes { get; set; }
        public List<SelectListItem> ObjectTypes { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string ObjectType { get; set; }
        public string Query { get; set; }
        public string Para1 { get; set; }
        public string Para2 { get; set; }
        public string Para3 { get; set; }
        public string Para4 { get; set; }
        public string Para5 { get; set; }
        public string Para6 { get; set; }
        public string Para7 { get; set; }
        public string Para8 { get; set; }
        public string Para9 { get; set; }
        public string Para10 { get; set; }
        public string Para11 { get; set; }
        public string Para12 { get; set; }
        public string Para13 { get; set; }
        public string Para14 { get; set; }
        public string Para15 { get; set; }
        public string Para16 { get; set; }
        public string Para17 { get; set; }
        public string Para18 { get; set; }
        public string Para19 { get; set; }
        public string Para20 { get; set; }
        public bool Status { get; set; }
        public bool ZoomURL { get; set; }
        public int Srno { get; set; }
        public int DisplayOrder { get; set; }



        // iX9: Common default Fields
        public string ID { get; set; }
        public string Document { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Mode { get; set; }
        public string GridMode { get; set; }

        // iX9: used while passing parameters from MasterGrid to Option and Back 
        public string MainType { get; set; }
        public string Controller2 { get; set; }
        public string Controller { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
        
    }
}