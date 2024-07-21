using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class ShareVM
    {

        public string Date { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string mWhat { get; set; }


        public string Type { get; set; }
        public string Parentkey { get; set; }
        public string Branch { get; set; }

        public bool Attachment { get; set; }
        public bool ExtraInfo { get; set; }

        public bool ExtraEamilAttachReq { get; set; }
        public string ExtraMobileNo { get; set; }
        public string ExtraEmailId { get; set; }
        public string ExtraFormat { get; set; }
        public string ExtraFormatL { get; set; }

        public List<SelectListItem> PrintFormats { get; set; }
        

        #region Lorry Receipt Parameter

        public bool ConsignorSmsReq { get; set; }
        public bool ConsignorEmailReq { get; set; }
        public bool ConsignorEmailAttachReq { get; set; }
        public string ConsignorName { get; set; }
        public string ConsignorMobileNo { get; set; }
        public string ConsignorEmailId { get; set; }
        public string ConsignorFormat { get; set; }
        public string ConsignorFormatL { get; set; }

        public bool ConsignoeeSmsReq { get; set; }
        public bool ConsignoeeEamilReq { get; set; }
        public bool ConsignoeeEamilAttachReq { get; set; }
        public string ConsignoeeName { get; set; }
        public string ConsignoeeMobileNo { get; set; }
        public string ConsignoeeEmailId { get; set; }
        public string ConsignoeeFormat { get; set; }
        public string ConsignoeeFormatL { get; set; }

        public bool BillPartySmsReq { get; set; }
        public bool BillPartyEamilReq { get; set; }
        public bool BillPartyEamilAttachReq { get; set; }
        public string BillPartyName { get; set; }
        public string BillPartyMobileNo { get; set; }
        public string BillPartyEmailId { get; set; }
        public string BillPartyFormat { get; set; }
        public string BillPartyFormatL { get; set; }

        #endregion

        #region Freight Memo Parameter

        public string BrokerName { get; set; }
        public string BrokerMobileNo { get; set; }
        public string BrokerEmailId { get; set; }

        #endregion

        #region Invoice (Bill) Parameter

        public bool CustomerSmsReq { get; set; }
        public bool CustomerEamilReq { get; set; }
        public bool CustomerEamilAttachReq { get; set; }
        public string CustomerName { get; set; }
        public string CustomerMobileNo { get; set; }
        public string CustomerEmailId { get; set; }
        public string CustomerFormat { get; set; }
        public string CustomerFormatL { get; set; }

        public bool CustomerGroupSmsReq { get; set; }
        public bool CustomerGroupEamilReq { get; set; }
        public bool CustomerGroupEamilAttachReq { get; set; }
        public string CustomerGroupName { get; set; }
        public string CustomerGroupMobileNo { get; set; }
        public string CustomerGroupEmailId { get; set; }
        public string CustomerGroupFormat { get; set; }
        public string CustomerGroupFormatL { get; set; }
        #endregion

        #region Payment (Creditor) Parameter

        public bool CreditorSmsReq { get; set; }
        public bool CreditorEamilReq { get; set; }
        public bool CreditorEamilAttachReq { get; set; }
        public string CreditorName { get; set; }
        public string CreditorMobileNo { get; set; }
        public string CreditorEmailId { get; set; }
        public string CreditorFormat { get; set; }
        public string CreditorFormatL { get; set; }
        #endregion

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
        public string Controller { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
        public string Code { get; set; }
    }
}