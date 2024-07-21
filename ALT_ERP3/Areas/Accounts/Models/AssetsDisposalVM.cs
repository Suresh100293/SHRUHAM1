namespace ALT_ERP3.Areas.Accounts.Models
{
    public class AssetsDisposalVM
    {
        // iX9: Field Structure of AssetDisposal
        public int AssetDisposal_RECORDKEY { get; set; }
        public string AssetDisposal_AssetID { get; set; }
        public decimal? AssetDisposal_AssetValue { get; set; }
        public string AssetDisposal_Branch { get; set; }
        public string AssetDisposal_CompCode { get; set; }
        public System.DateTime AssetDisposal_DocDate { get; set; }
        public string DocDateVM { get; set; }
        public int AssetDisposal_LocationCode { get; set; }
        public double AssetDisposal_Qty { get; set; }
        public double AssetDisposal_Qty2 { get; set; }
        public string AssetDisposal_Reason { get; set; }
        public int? AssetDisposal_Srl { get; set; }
        public int AssetDisposal_Store { get; set; }
        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public string AssetIDName { get; set; }
        public string BranchName { get; set; }
        public string StoreName { get; set; }

        // iX9: Common default Fields
        public int Document { get; set; }
        public string ENTEREDBY { get; set; }
        public System.DateTime LASTUPDATEDATE { get; set; }
        public string AUTHORISE { get; set; }
        public string AUTHIDS { get; set; }
        public string Mode { get; set; }

        // iX9: used while passing parameters from MasterGrid to Option and Back 
        public string Controller2 { get; set; }
        public string Header { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string TableName { get; set; }
        public string Module { get; set; }
        public string ViewDataId { get; set; }
    }
}