using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class BillQueryVM
    {
        public List<BillQueryVM> ValueList { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }

        public bool FindOpening { get; set; }

        public string BillType { get; set; }
        public string BillBranch { get; set; }
        public string BillBranchN { get; set; }
        public string BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public string BillENTEREDBY { get; set; }
        public DateTime BillLASTUPDATEDATE { get; set; }
        public string Customer { get; set; }
        public string BMaster { get; set; }
        public string TotAmt { get; set; }
        public string Tablekey { get; set; }
        public string Parentkey { get; set; }
        public string Remark { get; set; }
        public string CustomerCode { get; set; }

        public decimal BillBalanceAmt { get; set; }

        public List<BillRelatedLorryReceiptDetailsVM> LorryReceiptList { get; set; }
        public List<BillsubmissinDetailsVM> billsubmissinDetails { get; set; }
        public List<SendBillsubmissinDetailsVM> sendbillsubmissinDetails { get; set; }
        public List<PaymentReceivedDetailsVM> PaymentReceivedDetails { get; set; }
        
        // iX9: Common default Fields
        public string Document { get; set; }
        public string Prefix { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Mode { get; set; }

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
        public string Code { get; set; }


        public bool Shortcut { get; set; }
    }

    public class BillRelatedLorryReceiptDetailsVM
    {
       

        public int Serial { get; set; }
        public string Lrno { get; set; }
        public DateTime LrDate { get; set; }
        public string LrTime { get; set; }
        public string Consignor { get; set; }
        public string Consignee { get; set; }
        public string LRFrom { get; set; }
        public string LRTo { get; set; }
        public string LRQty { get; set; }
        public string Unit { get; set; }
        public string ActWt { get; set; }
        public string ChrgWt { get; set; }
        public string ChrgType { get; set; }
        public decimal LrAmt { get; set; }
        public string PartyChallan { get; set; }
        public string PartyInvoice { get; set; }
        public string PONumber { get; set; }
        public string BENumber { get; set; }
        public string EawayBill { get; set; }
        public List<LRInvoiceVM> Charges { get; set; }

    }

    public class BillsubmissinDetailsVM
    {
        public string SBBranch { get; set; }
        public DateTime SBDate { get; set; }
        public DateTime SBSubDt { get; set; }
        public string SBDocNo { get; set; }
        public string SBThrough { get; set; }
        public string SBRemark { get; set; }
        public string SBENTEREDBY { get; set; }
        public DateTime SBLASTUPDATEDATE { get; set; }
        public string SBType { get; set; }
        public string SBPartyName { get; set; }
    }
    public class SendBillsubmissinDetailsVM
    {
        public string SBBranch { get; set; }
        public DateTime SBDate { get; set; }
        public string SBDocNo { get; set; }
        public string SBRemark { get; set; }
        public string SBENTEREDBY { get; set; }
        public DateTime SBLASTUPDATEDATE { get; set; }
        public string ToBranch { get; set; }
        public string FromBranch { get; set; }
        public string SBType { get; set; }
    }
    public class PaymentReceivedDetailsVM
    {
        public string Branch { get; set; }
        public DateTime Date { get; set; }
        public string DocNo { get; set; }
        public string Remark { get; set; }
        public string ENTEREDBY { get; set; }
        public DateTime LASTUPDATEDATE { get; set; }
        public string Type { get; set; }
        public string TypeName { get; set; }
        public string Bank { get; set; }
        public string Code { get; set; }
        public string TablekeyBranch { get; set; }
        public List<DebtorReceiptVM> GetAmountDetails { get; set; }
    }

}