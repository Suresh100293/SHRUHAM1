using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Models
{
    public class PartyCreditStatusManagementVM
    { 
        // iX9: Field Structure of HoldTransactions
        public int HoldTransactions_RECORDKEY { get; set; }
        public bool HoldTransactions_CheckCRDays { get; set; }
        public bool HoldTransactions_CheckCRLimit { get; set; }
        public bool HoldTransactions_ChkTempCRDays { get; set; }
        public bool HoldTransactions_ChkTempCRLimit { get; set; }
        public string HoldTransactions_Code { get; set; }
        public bool HoldTransactions_CRDaysWarn { get; set; }
        public decimal HoldTransactions_CrLimit { get; set; }
        public decimal HoldTransactions_CRLimitTole { get; set; }
        public bool HoldTransactions_CRLimitWarn { get; set; }
        public bool HoldTransactions_CRLimitWithPO { get; set; }
        public bool HoldTransactions_CRLimitWithTrx { get; set; }
        public int HoldTransactions_CrPeriod { get; set; }
        public System.DateTime HoldTransactions_DocDate { get; set; }
        public string HoldTransactions_DocDateVM { get; set; }
        public bool HoldTransactions_HoldDespatch { get; set; }
        public System.DateTime HoldTransactions_HoldDespatchDt1 { get; set; }
        public string HoldTransactions_HoldDespatchDt1VM { get; set; }
        public System.DateTime HoldTransactions_HoldDespatchDt2 { get; set; }
        public string HoldTransactions_HoldDespatchDt2VM { get; set; }
        public bool HoldTransactions_HoldEnquiry { get; set; }
        public System.DateTime HoldTransactions_HoldEnquiryDt1 { get; set; }
        public string HoldTransactions_HoldEnquiryDt1VM { get; set; }
        public System.DateTime HoldTransactions_HoldEnquiryDt2 { get; set; }
        public string HoldTransactions_HoldEnquiryDt2VM { get; set; }
        public bool HoldTransactions_HoldInvoice { get; set; }
        public System.DateTime HoldTransactions_HoldInvoiceDt1 { get; set; }
        public string HoldTransactions_HoldInvoiceDt1VM { get; set; }
        public System.DateTime HoldTransactions_HoldInvoiceDt2 { get; set; }
        public string HoldTransactions_HoldInvoiceDt2VM { get; set; }
        public string HoldTransactions_HoldNarr { get; set; }
        public bool HoldTransactions_HoldOrder { get; set; }
        public System.DateTime HoldTransactions_HoldOrderDt1 { get; set; }
        public string HoldTransactions_HoldOrderDt1VM { get; set; }
        public System.DateTime HoldTransactions_HoldOrderDt2 { get; set; }
        public string HoldTransactions_HoldOrderDt2VM { get; set; }
        public bool HoldTransactions_HoldPayment { get; set; }
        public System.DateTime HoldTransactions_HoldPaymentDt1 { get; set; }
        public string HoldTransactions_HoldPaymentDt1VM { get; set; }
        public System.DateTime HoldTransactions_HoldPaymentDt2 { get; set; }
        public string HoldTransactions_HoldPaymentDt2VM { get; set; }
        public bool HoldTransactions_HoldQuote { get; set; }
        public System.DateTime HoldTransactions_HoldQuoteDt1 { get; set; }
        public string HoldTransactions_HoldQuoteDt1VM { get; set; }
        public System.DateTime HoldTransactions_HoldQuoteDt2 { get; set; }
        public string HoldTransactions_HoldQuoteDt2VM { get; set; }
        public System.DateTime HoldTransactions_TempCrDayDt1 { get; set; }
        public string HoldTransactions_TempCrDayDt1VM { get; set; }
        public System.DateTime HoldTransactions_TempCrDayDt2 { get; set; }
        public string HoldTransactions_TempCrDayDt2VM { get; set; }
        public decimal HoldTransactions_TempCrLimit { get; set; }
        public System.DateTime HoldTransactions_TempCrLimitDt1 { get; set; }
        public string HoldTransactions_TempCrLimitDt1VM { get; set; }
        public System.DateTime HoldTransactions_TempCrLimitDt2 { get; set; }
        public string HoldTransactions_TempCrLimitDt2VM { get; set; }
        public int HoldTransactions_TempCrPeriod { get; set; }
        public string HoldTransactions_TempRemark { get; set; }
        public string HoldTransactions_Ticklers { get; set; }
        public string HoldTransactions_UserID { get; set; }
        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public List<SelectListItem> CRLimitWarnList { get; set; }
        public List<SelectListItem> CRDaysWarnList { get; set; }
        public string CodeName { get; set; }
        public string UserIDName { get; set; }

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
        public bool AutoClose { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
    }
}