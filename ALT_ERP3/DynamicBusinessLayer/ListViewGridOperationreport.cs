using EntitiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.DynamicBusinessLayer
{
    public class ListViewGridOperationreport : IReportGridOperation
    {
        //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        private IBusinessCommon mIBusi = new BusinessCommon();
        //public DateTime SDate;
        public JsonResult getGridDataColumns(string id, string open = "0", string close = "0", string TCredit = "0", string TDebit = "0", string mFlag = "L", string mVar1 = "_", string mVar2 = "_", string mVar3 = "_", string mVar4 = "_")
        {
            var mTfatSearch = (from TS in ctxTFAT.TfatSearch where TS.Code == id orderby TS.Sno
                               select new { TS.ColHead, TS.ColWidth, TS.ColField, TS.ColType, TS.YesTotal, TS.AllowEdit, TS.Decs, TS.IsHidden }).ToList();
            List<string> colname = new List<string>();
            List<GridColumn> colModal = new List<GridColumn>();
            List<object> result = new List<object>();
            string mHead = "";
            DateTime mDate = Convert.ToDateTime(System.Web.HttpContext.Current.Session["StartDate"].ToString());
            foreach (var Fld in mTfatSearch)
            {
                mHead = Fld.ColHead.Trim().Replace("##", "");
                // ## is used to make the column editable
                //if (mHead.ToLower().Contains("%pivotcolumns"))
                //{
                //    try
                //    {
                //        string mStr = ctxTFAT.ReportHeader.Where(z => z.Code == id).Select(x => x.PostLogic.Trim()).FirstOrDefault() ?? "";
                //        var mpara = mStr.Split(';');
                //        DataTable mcolsdt = new DataTable();
                //        mcolsdt = GetDataTable(mpara[0].ToString());
                //        mStr = "";
                //        for (int x = 0; x <= mcolsdt.Rows.Count - 1; x++)
                //        {
                //            mHead = mcolsdt.Rows[x][0].ToString();
                //            colname.Add(mHead);
                //            GridColumn gc = new GridColumn();
                //            gc.name = mHead;
                //            gc.index = "[" + mHead + "]";
                //            gc.editable = false;
                //            if (Fld.ColType == "Num" || Fld.ColType == "Qty" || Fld.ColType == "Rte")
                //            {
                //                gc.align = "right";
                //                gc.formatter = "number : { decimalSeparator: \".\", thousandsSeparator: \"\", decimalPlaces: " + Fld.Decs + "}";
                //                if (Fld.YesTotal == true)
                //                {
                //                    gc.summaryTpl = "<b>{0}</b>";
                //                    gc.summaryType = "sum";
                //                }
                //            }
                //            if (Fld.ColType == "Dte" || Fld.ColType == "Dtm")
                //            {
                //                gc.align = "center";
                //                if (gc.editable == true)
                //                    gc.editoptions = "dataInit:function(el){$(el).datepicker({ dateFormat: 'yy-mm-dd'});}";
                //            }
                //            gc.sortable = true;
                //            gc.width = Fld.IsHidden == true ? "0" : (Fld.ColWidth).ToString();   // removed /15
                //            colModal.Add(gc);
                //        }
                //    }
                //    catch { }
                //}
                //else
                //{
                if (mHead.Contains("%MonthYear"))
                {
                    mHead = mHead.Replace("%MonthYear1", mDate.ToString("MMM") + "-" + mDate.Year);
                    mHead = mHead.Replace("%MonthYear2", mDate.AddMonths(1).ToString("MMM") + "-" + mDate.AddMonths(1).Year);
                    mHead = mHead.Replace("%MonthYear3", mDate.AddMonths(2).ToString("MMM") + "-" + mDate.AddMonths(2).Year);
                    mHead = mHead.Replace("%MonthYear4", mDate.AddMonths(3).ToString("MMM") + "-" + mDate.AddMonths(3).Year);
                    mHead = mHead.Replace("%MonthYear5", mDate.AddMonths(4).ToString("MMM") + "-" + mDate.AddMonths(4).Year);
                    mHead = mHead.Replace("%MonthYear6", mDate.AddMonths(5).ToString("MMM") + "-" + mDate.AddMonths(5).Year);
                    mHead = mHead.Replace("%MonthYear7", mDate.AddMonths(6).ToString("MMM") + "-" + mDate.AddMonths(6).Year);
                    mHead = mHead.Replace("%MonthYear8", mDate.AddMonths(7).ToString("MMM") + "-" + mDate.AddMonths(7).Year);
                    mHead = mHead.Replace("%MonthYear9", mDate.AddMonths(8).ToString("MMM") + "-" + mDate.AddMonths(8).Year);
                    mHead = mHead.Replace("%MonthYearA", mDate.AddMonths(9).ToString("MMM") + "-" + mDate.AddMonths(9).Year);
                    mHead = mHead.Replace("%MonthYearB", mDate.AddMonths(10).ToString("MMM") + "-" + mDate.AddMonths(10).Year);
                    mHead = mHead.Replace("%MonthYearC", mDate.AddMonths(11).ToString("MMM") + "-" + mDate.AddMonths(11).Year);
                }
                colname.Add(mHead);
                GridColumn gc = new GridColumn();
                gc.name = mHead;
                gc.index = Fld.ColField.Trim();
                gc.editable = Fld.AllowEdit;
                if (Fld.ColType == "Num" || Fld.ColType == "Qty" || Fld.ColType == "Rte")
                {
                    gc.align = "right";
                    gc.formatter = "number : { decimalSeparator: \".\", thousandsSeparator: \"\", decimalPlaces: " + Fld.Decs + ", defaultValue: '0.00'}";
                    if (Fld.YesTotal == true)
                    {
                        gc.summaryTpl = "<b>{0}</b>";
                        gc.summaryType = "sum";
                    }
                }
                if (Fld.ColType == "Dte" || Fld.ColType == "Dtm")
                {
                    gc.align = "center";
                    if (gc.editable == true)
                        gc.editoptions = "dataInit:function(el){$(el).datepicker({ dateFormat: 'yy-mm-dd'});}";
                }
                gc.sortable = true;
                gc.width = Fld.IsHidden == true ? "0" : (Fld.ColWidth).ToString();   // removed /15
                gc.hidden = Fld.IsHidden;
                colModal.Add(gc);
                //}
            }
            result.Add(Core.CoreCommon.GetString(colname.ToArray()));
            result.Add(colModal);
            result.Add(open);
            result.Add(close);
            result.Add(TCredit);
            result.Add(TDebit);
            if (mVar1 != "_")
            {
                result.Add(mVar1);
            }
            if (mVar2 != "_")
            {
                result.Add(mVar2);
            }
            if (mVar3 != "_")
            {
                result.Add(mVar3);
            }
            if (mVar4 != "_")
            {
                result.Add(mVar4);
            }
            JsonResult JR = new JsonResult();
            JR.Data = Newtonsoft.Json.JsonConvert.SerializeObject(result);
            return JR;
        }

        //public object Get(string sidx, string sord, int page, int rows, bool _search, string searchField, string searchOper, string searchString, string mFlag = "L")
        //{
        //    var mData = ctxTFAT.Set(Core.CoreCommon.GetTableType("")) as IEnumerable<object>;
        //    if (_search)
        //    {
        //        switch (searchOper)
        //        {
        //            case "eq":
        //                mData = from r in mData
        //                        where r.GetType().GetProperty(searchField).GetValue(r).ToString().Equals(searchString)
        //                        select r;
        //                break;
        //            case "ne":
        //                mData = from r in mData
        //                        where r.GetType().GetProperty(searchField).GetValue(r).ToString().Equals(searchString) == false
        //                        select r;
        //                break;
        //            case "bw":
        //                mData = from r in mData
        //                        where r.GetType().GetProperty(searchField).GetValue(r).ToString().StartsWith(searchString)
        //                        select r;
        //                break;
        //            case "bn":
        //                mData = from r in mData
        //                        where r.GetType().GetProperty(searchField).GetValue(r).ToString().StartsWith(searchString) == false
        //                        select r;
        //                break;
        //            case "ew":
        //                mData = from r in mData
        //                        where r.GetType().GetProperty(searchField).GetValue(r).ToString().EndsWith(searchString)
        //                        select r;
        //                break;
        //            case "en":
        //                mData = from r in mData
        //                        where r.GetType().GetProperty(searchField).GetValue(r).ToString().EndsWith(searchString) == false
        //                        select r;
        //                break;
        //            case "cn":
        //            case "in":
        //                mData = from r in mData
        //                        where r.GetType().GetProperty(searchField).GetValue(r).ToString().Contains(searchString)
        //                        select r;
        //                break;
        //            case "nc":
        //            case "ni":
        //                mData = from r in mData
        //                        where r.GetType().GetProperty(searchField).GetValue(r).ToString().Contains(searchString) == false
        //                        select r;
        //                break;
        //        }
        //    }

        //    if (sord == "desc")
        //    {
        //        mData = from r in mData
        //                orderby r.GetType().GetProperty(sidx).GetValue(r) descending
        //                select r;
        //    }
        //    else
        //    {
        //        mData = from r in mData
        //                orderby r.GetType().GetProperty(sidx).GetValue(r)
        //                select r;
        //    }
        //    int count = mData.Count();
        //    try
        //    {
        //        count = mData.Count();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    //int count = mData.Count();
        //    int pageIndex = Convert.ToInt32(page) - 1;
        //    int pageSize = rows;
        //    int totalRecords = count;
        //    int totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);

        //    var Data = mData.AsEnumerable().Skip(pageIndex * pageSize).Take(pageSize);

        //    var result = new
        //    {
        //        total = totalPages,
        //        page = page,
        //        records = totalRecords,
        //        //rows = (from d in data
        //        //        select new
        //        //        {
        //        //            i = d.gettype().getproperty("code").getvalue(d).tostring(),
        //        //            cell = getcellfields(d.gettype().getproperty("code").getvalue(d).tostring(), dheader, data, mflag)
        //        //        }).toarray()
        //    };
        //    return result;
        //}

        //private string[] GetCellFields(string Code, IEnumerable<object> DHeaderData, IEnumerable<object> Data, string mFlag)
        //{
        //    dynamic TableName;
        //    string OptionCode;
        //    if (mFlag == "L")
        //    {
        //        TableName = DHeaderData.FirstOrDefault().GetType().GetProperty("TableName").GetValue(DHeaderData.FirstOrDefault());
        //        OptionCode = DHeaderData.FirstOrDefault().GetType().GetProperty("OptionCode").GetValue(DHeaderData.FirstOrDefault()).ToString();
        //    }
        //    else
        //    {
        //        TableName = DHeaderData.FirstOrDefault().GetType().GetProperty("Tables").GetValue(DHeaderData.FirstOrDefault());
        //        OptionCode = DHeaderData.FirstOrDefault().GetType().GetProperty("Code").GetValue(DHeaderData.FirstOrDefault()).ToString();
        //    }

        //    var mFlds = ctxTFAT.TfatSearch.Where(m => m.Code == OptionCode);
        //    var resultDB = from d in Data
        //                   where d.GetType().GetProperty("Code").GetValue(d).ToString().Equals(Code.ToString())
        //                   select d;
        //    return null;
        //}
    }
}