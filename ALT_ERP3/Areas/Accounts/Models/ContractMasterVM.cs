using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class ContractMasterVM
    {
        public string DocDate { get; set; }
        public string CustomerType { get; set; }
        public string Customer { get; set; }
        public string CustomerN { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string FromBranch { get; set; }
        public string FromBranchN { get; set; }
        public string ToBranch { get; set; }
        public string ToBranchN { get; set; }
        public string PaymentTerms { get; set; }
        public string Remark { get; set; }

        public string copyDocument { get; set; }

        public List<ContractList> ContractList { get; set; }
        public ContractList contract { get; set; }
        public List<PurchaseVM> ChargesHead { get; set; }

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

    public class ContractList
    {
        public string ContractType { get; set; }

        public string Service { get; set; }
        public string ServiceN { get; set; }
        public List<SelectListItem> ServiceList { get; set; }
        public List<SelectListItem> ItemListList { get; set; }
        public int Sno { get; set; }
        public string TypeOfChrg { get; set; }
        public int FromWT { get; set; }
        public int ToWT { get; set; }
        public float Rate { get; set; }
        public bool ChargeOfChrgWT { get; set; }
        public bool ChargeOfActWT { get; set; }
        public string ChargeONN { get; set; }
        public List<PurchaseVM> Charges { get; set; }
        public string Mode { get; set; }
        public string UniqueKey { get; set; }

        public string ConDetilsCode { get; set; }
    }
    public class CopyContractVM
    {
        public string DocumentNo { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime TODate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerType { get; set; }
    }
}