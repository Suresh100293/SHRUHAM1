using ALT_ERP3.Areas.Accounts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class ConsignerMasterVM
    {
        public List<ConsignerMasterVM> AddressList { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string DuplicateName { get; set; }

        public int? SrNo { get; set; }
        public List<ConsignerList> consignerLists { get; set; }
        public bool ShortCutKey { get; set; }
        
        
        public string Fax { get; set; }
        public bool Acitve { get; set; }
        public string AcitveorNot { get; set; }
     
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        

        public bool TickLrConsignor { get; set; }
        public bool TickLrConsignee { get; set; }
        public bool HoldTickLrConsignor { get; set; }
        public bool HoldTickLrConsignee { get; set; }

        public bool RemarkReq { get; set; }
        public string Remark { get; set; }
        public bool HoldReq { get; set; }
        public string HoldRemark { get; set; }

        #region MailINfo
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public string Tel1 { get; set; }
        public string Tel2 { get; set; }
        public string Tel3 { get; set; }
        public string Country { get; set; }
        public string CountryName { get; set; }
        public string State { get; set; }
        public string StateName { get; set; }
        public string City { get; set; }
        public string CityName { get; set; }
        public string Pin { get; set; }
        public string GSTNo { get; set; }
        public string PanNo { get; set; }
        public string ContactPersonName { get; set; }
        public bool AllSendEmail { get; set; }
        public bool AllSendSMS { get; set; }

        #endregion
       

        // iX9: Common default Fields
        public string Document { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Mode { get; set; }

        //20.8.20 Suresh
        public string Branch { get; set; }
        //public string BranchL { get; set; }
        //public List<SelectListItem> Branches { get; set; }

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

    public class ConsignerList
    {
        public bool BrnachWiseConsigner { get; set; }
        public string CCode { get; set; }
        public string CName { get; set; }
        public string CContactPerson { get; set; }
        public string CAddress { get; set; }
        public string CCity { get; set; }
        public string CPincode { get; set; }
        public string CDistirict { get; set; }
        public string CPhoneNO { get; set; }
        public string CFax { get; set; }
        public bool Consigner { get; set; }
    }
}