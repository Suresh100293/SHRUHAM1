using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class BillSubmissionVM
    {
        public List<GridOption> PrintGridList { get; set; }

        public bool GlobalSearch { get; set; }
        public string GlobalSearchBill { get; set; }
        public string Branch { get; set; }
        public string BranchN { get; set; }
        public string Methods { get; set; }
        public string SearchBillNo { get; set; }
        public string DocumentNo { get; set; }//Auto
        public string BillType { get; set; }//Direct or Partwise
        public string Party { get; set; }//Fetching Customer Master
        public string PartyParent { get; set; }//Fetching Customer Master
        public string PartyName { get; set; }
        public string PartyParentName { get; set; }
        public string FromBranch { get; set; }
        public string FromBranchName { get; set; }
        public string SubmitDate { get; set; }
        public string DocDate { get; set; }
        public string Through { get; set; }//By Hand or By Courier
        public string Remark { get; set; }
        public string BillTableKey { get; set; }
        public string BillParentKey { get; set; }
        public List<BillDetails> BillDetails { get; set; }
        public BillDetails BillDetail { get; set; }
        public bool SendBillController { get; set; }

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
        public string Code { get; set; }
    }

    public class BillDetails
    {
        public int Sno { get; set; }
        public string BillNo { get; set; }
        public string BillType { get; set; }
        public string BillTypeName { get; set; }
        public DateTime Date { get; set; }
        public string Amount { get; set; }
        public string PartyParent { get; set; }
        public string PartyParentName { get; set; }
        public bool AllowToChange { get; set; }
        public string BillParty { get; set; }
        public string BillPartyName { get; set; }
        public string BillBranch { get; set; }
        public string BillBranchN { get; set; }
        public string BillTableKey { get; set; }
        public string BillParentKey { get; set; }

        //public bool BillNoEnable { get; set; }
    }
}