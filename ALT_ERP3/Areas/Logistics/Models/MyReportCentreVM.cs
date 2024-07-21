using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Models
{
    public class MyReportCentreVM
    {
        // iX9: Field Structure of ReportHeader
        public int ReportHeader_RECORDKEY { get; set; }
        public string ReportHeader_AccGroups { get; set; }
        public bool ReportHeader_AllowEmail { get; set; }
        public bool ReportHeader_AllowSMS { get; set; }
        public string ReportHeader_BackColor { get; set; }
        public string ReportHeader_BarGraph { get; set; }
        public string ReportHeader_Code { get; set; }
        public string ReportHeader_Comp { get; set; }
        public bool ReportHeader_DefaultReport { get; set; }
        public bool ReportHeader_DisplayGrid { get; set; }
        public decimal ReportHeader_Divisor { get; set; }
        public bool ReportHeader_DontPrintTotal { get; set; }
        public bool ReportHeader_DontSort { get; set; }
        public string ReportHeader_DrillCaption { get; set; }
        public string ReportHeader_DrillQuery { get; set; }
        public string ReportHeader_DrillQuerySub { get; set; }
        public string ReportHeader_EmailTemplate { get; set; }
        public bool ReportHeader_FontBold { get; set; }
        public bool ReportHeader_FontItalics { get; set; }
        public string ReportHeader_FontName { get; set; }
        public int ReportHeader_FontSize { get; set; }
        public bool ReportHeader_FontStrike { get; set; }
        public bool ReportHeader_FontUnderLine { get; set; }
        public string ReportHeader_ForeColor { get; set; }
        public string ReportHeader_FormatGroup { get; set; }
        public string ReportHeader_FormatHead { get; set; }
        public int ReportHeader_GridColor { get; set; }
        public string ReportHeader_GroupHead { get; set; }
        public int ReportHeader_HeaderHeight { get; set; }
        public string ReportHeader_InputPara { get; set; }
        public bool ReportHeader_IsDrillDown { get; set; }
        public string ReportHeader_ItemGroups { get; set; }
        public decimal ReportHeader_LabelHORNos { get; set; }
        public decimal ReportHeader_LabelHORSpace { get; set; }
        public bool ReportHeader_LabelOption { get; set; }
        public int ReportHeader_LabelPrintTitle { get; set; }
        public decimal ReportHeader_LabelTitleWidth { get; set; }
        public decimal ReportHeader_LabelVERNos { get; set; }
        public decimal ReportHeader_LabelVERSpace { get; set; }
        public bool ReportHeader_Locked { get; set; }
        public string ReportHeader_MenuPlace { get; set; }
        public int ReportHeader_minHeight { get; set; }
        public string ReportHeader_Modules { get; set; }
        public bool ReportHeader_NoDetails { get; set; }
        public bool ReportHeader_NoSubHeading { get; set; }
        public string ReportHeader_OrderBy { get; set; }
        public int ReportHeader_OwnReport { get; set; }
        public bool ReportHeader_PageOrient { get; set; }
        public string ReportHeader_ParaString { get; set; }
        public bool ReportHeader_pBlank { get; set; }
        public string ReportHeader_pMerge { get; set; }
        public string ReportHeader_PostLogic { get; set; }
        public string ReportHeader_pToMerge { get; set; }
        public int ReportHeader_RecLines { get; set; }
        public string ReportHeader_RecordFilter { get; set; }
        public bool ReportHeader_RemoveUnused { get; set; }
        public bool ReportHeader_RunAtStart { get; set; }
        public string ReportHeader_SelectQuery { get; set; }
        public int ReportHeader_SepLines { get; set; }
        public bool ReportHeader_ShowFilter { get; set; }
        public string ReportHeader_SMSTemplate { get; set; }
        public string ReportHeader_SortedOn { get; set; }
        public string ReportHeader_SubCodeOf { get; set; }
        public bool ReportHeader_SubTotal { get; set; }
        public string ReportHeader_SubTotalOn { get; set; }
        public string ReportHeader_SubTypes { get; set; }
        public bool ReportHeader_Summarized { get; set; }
        public string ReportHeader_Tables { get; set; }
        public string ReportHeader_Tabs { get; set; }
        public int ReportHeader_TimesUsed { get; set; }
        public string ReportHeader_UserQuery { get; set; }
        public string ReportHeader_Users { get; set; }
        // iX9: Field Structure of TfatSearch
        public int TfatSearch_RECORDKEY { get; set; }
        public bool TfatSearch_AllowEdit { get; set; }
        public string TfatSearch_BackColor { get; set; }
        public bool TfatSearch_CalculatedCol { get; set; }
        public string TfatSearch_ChildOfCol { get; set; }
        public string TfatSearch_Code { get; set; }
        public int TfatSearch_ColChars { get; set; }
        public string TfatSearch_ColCondition { get; set; }
        public string TfatSearch_ColField { get; set; }
        public string TfatSearch_ColHead { get; set; }
        public int TfatSearch_ColPosition { get; set; }
        public string TfatSearch_ColType { get; set; }
        public int TfatSearch_ColWidth { get; set; }
        public bool TfatSearch_Comma { get; set; }
        public string TfatSearch_CondBCLR { get; set; }
        public int TfatSearch_Decs { get; set; }
        public bool TfatSearch_DisplayGrid { get; set; }
        public bool TfatSearch_FitToHeight { get; set; }
        public bool TfatSearch_FitToWidth { get; set; }
        public string TfatSearch_FldAs { get; set; }
        public bool TfatSearch_FontBold { get; set; }
        public bool TfatSearch_FontItalics { get; set; }
        public string TfatSearch_FontName { get; set; }
        public int TfatSearch_FontSize { get; set; }
        public bool TfatSearch_FontStrike { get; set; }
        public bool TfatSearch_FontUnderLine { get; set; }
        public string TfatSearch_ForeColor { get; set; }
        public string TfatSearch_FormatString { get; set; }
        public bool TfatSearch_GraphCol { get; set; }
        public string TfatSearch_GroupHead { get; set; }
        public bool TfatSearch_Idle { get; set; }
        public bool TfatSearch_IsHidden { get; set; }
        public bool TfatSearch_Locked { get; set; }
        public string TfatSearch_MergeCond { get; set; }
        public bool TfatSearch_MergeData { get; set; }
        public string TfatSearch_Modules { get; set; }
        public int TfatSearch_Sno { get; set; }
        public string TfatSearch_SplitColumn { get; set; }
        public string TfatSearch_SystemColumn { get; set; }
        public bool TfatSearch_xCurrency { get; set; }
        public bool TfatSearch_YesTotal { get; set; }
        // iX9: Structure from hidden fields (e.g. CountryName etc, for combo,autocomplete) 
        public List<SelectListItem> ColTypeList { get; set; }

        // iX9: Fields for GridView
        public bool tempIsDeleted { get; set; }
        public int tempid { get; set; }
        public IList<MyReportCentreVM> GridDataVM { get; set; }

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
}