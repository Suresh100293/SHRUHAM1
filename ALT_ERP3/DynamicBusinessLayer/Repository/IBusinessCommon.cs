using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using EntitiModel;

namespace ALT_ERP3.DynamicBusinessLayer.Repository
{
    interface IBusinessCommon
    {
        //FormCollection GetDBFormCollection(TFATDesignHeader DHead, FormCollection fc);

        //IQueryable<TFATDesignHeader> SelectSingleTfatDeignHeader(string OptionCode);

        IQueryable<ReportHeader> SelectSingleReportHeader(string Code);

        //IQueryable<TFATDbf> SelectTableName(string fle);

        string GetSrl(string SubType);
        string FieldOfTable(string mTableName, string mReturnField, string mFindWhat, string mFindVal);
        string GetSrlNew(string BR);
        string GetPrefix();
        //string GetCode(bool mAutoAccCode, byte mAutoAccStyle, byte mAutoAccLength);
        string GetProductCode();
        string GetAccountGroup();

    }
}