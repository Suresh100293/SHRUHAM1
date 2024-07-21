using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class TransactionUserRightsVM
    {
        // iX9: Field Structure of UserRightsTrx
        public int UserRightsTrx_RECORDKEY { get; set; }
        public string UserRightsTrx_Branch { get; set; }
        public string UserRightsTrx_Code { get; set; }
        public string UserRightsTrx_CompCode { get; set; }
        public int UserRightsTrx_LocationCode { get; set; }
        public int UserRightsTrx_Sno { get; set; }
        public string UserRightsTrx_Type { get; set; }
        public bool UserRightsTrx_xAdd { get; set; }
        public bool UserRightsTrx_xBackDated { get; set; }
        public bool UserRightsTrx_xCess { get; set; }
        public bool UserRightsTrx_xDelete { get; set; }
        public bool UserRightsTrx_xEdit { get; set; }
        public decimal UserRightsTrx_xLimit { get; set; }
        public decimal UserRightsTrx_xMinLimit { get; set; }
        public bool UserRightsTrx_xPrint { get; set; }

        // iX9: Common default Fields
        public int Document { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Mode { get; set; }
        public string Header { get; set; }

        // iX9: special lists used for Grid2Grid relation type of interface
        public string DocTypes_Code { get; set; }
        public string DocTypes_Name { get; set; }
        public string DocTypes_MainType { get; set; }
        public string DocTypes_SubType { get; set; }

        public List<TransactionUserRightsVM> mLeftList { get; set; }
        public List<TransactionUserRightsVM> mRightList { get; set; }
        public string SearchBy { get; set; }
        public string SearchContent { get; set; }
    }
}