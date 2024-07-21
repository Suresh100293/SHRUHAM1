using System;
using System.Linq;
using System.Data.Entity;
using EntitiModel;
using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.DynamicBusinessLayer
{
    public class BusinessCommon : IBusinessCommon
    {
        //TFATERPDatabaseEntities1 mCtx = new TFATERPDatabaseEntities1();
        //nEntities mCtx = new nEntities();
        //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        private string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        private string mperiod = System.Web.HttpContext.Current.Session["FPerd"].ToString();

        //public FormCollection GetDBFormCollection(TFATDesignHeader DHead, FormCollection fc)
        //{
        //    FormCollection fcoll = new FormCollection();
        //    foreach (string Name in fc.AllKeys)
        //    {
        //        string n = ctxTFAT.TFATDesignForms.Where(f => f.LabelCaption == Name).Select(f => f.Fld).ToList()[0].ToString();
        //        string v = fc[Name].ToString();
        //        fc.Add(n, v);
        //    }
        //    return fcoll;
        //}

        //public IQueryable<TFATDesignHeader> SelectSingleTfatDeignHeader(string OptionCode)
        //{
        //    return from t in ctxTFAT.TFATDesignHeaders
        //           where t.OptionCode == OptionCode
        //           select t;
        //}

        public IQueryable<ReportHeader> SelectSingleReportHeader(string Code)
        {
            return from t in ctxTFAT.ReportHeader
                   where t.Code == Code
                   select t;
        }
        
        //public IQueryable<TFATDbf> SelectTableName(string fle)
        //{
        //    return from t in ctxTFAT.TFATDbfs
        //           where t.fle == fle
        //           select t;
        //}

        public string FieldOfTable(string mTableName, string mReturnField, string mFindWhat, string mFindVal)
        {
            var tblset = Core.CoreCommon.GetTableData(mTableName);
            var output = tblset.AsQueryable().ToListAsync().Result.ToList();
            var mqry = (from m in output
                        where m.GetType().GetProperty(mFindWhat).GetValue(m).ToString().Equals(mFindVal)
                        select m.GetType().GetProperty(mReturnField).GetValue(m).ToString());
            return (mqry.FirstOrDefault()).ToString();
        }

        public string GetSrl(string SubType)
        {

            var BR = (from TS in ctxTFAT.Ledger
                      where TS.SubType == SubType
                      select TS.Srl).Max();

            string str = BR;
            string digits = new string(str.Where(char.IsDigit).ToArray());
            string letters = new string(str.Where(char.IsLetter).ToArray());

            int number;
            if (!int.TryParse(digits, out number)) //int.Parse would do the job since only digits are selected
            {
                Console.WriteLine("Something weired happened");
            }
            string newStr = letters + (++number).ToString("D6");

            return newStr;
        }

        public string GetSrlNew(string BR)
        {
            string str = BR;
            if (str != null)
            {
                string digits = new string(str.Where(char.IsDigit).ToArray());
                string letters = new string(str.Where(char.IsLetter).ToArray());
                int number;
                if (!int.TryParse(digits, out number)) //int.Parse would do the job since only digits are selected
                {
                    Console.WriteLine("Something weired happened");
                }
                string newStr = letters + (++number).ToString("D6");
                return newStr;
            }
            else
            {
                str = "000001";
                return str;
            }
        }

        public string GetPrefix()
        {
            var perd = ctxTFAT.TfatPerd.Where(x => x.Code == mcompcode && x.PerdCode == mperiod).Select(b => new
            {
                b.StartDate,
                b.LastDate
            }).FirstOrDefault();
            var FDate = mperiod;
            var d1 = perd.StartDate.ToShortDateString();
            var d2 = perd.LastDate.ToShortDateString();
            var m1 = d1.Substring(3, 2);
            var y1 = d1.Substring(8, 2);
            var m2 = d2.Substring(3, 2);
            var y2 = d2.Substring(8, 2);
            var prefix = y1 + m1 + y2 + m2;
            return prefix;
        }

        //public string GetCode(bool mAutoAccCode, byte mAutoAccStyle, byte mAutoAccLength)
        //{
        //    string mCode = "";
        //    if (mAutoAccCode == true)
        //    {
        //        if (mAutoAccStyle == 0)   // continuous number
        //        {
        //            mCode = (from TS in ctxTFAT.Master select TS.Code).Max();
        //            string digits = new string(mCode.Where(char.IsDigit).ToArray());
        //            int number;
        //            if (!int.TryParse(digits, out number)) //int.Parse would do the job since only digits are selected
        //            {
        //                Console.WriteLine("Something Went Wrong");
        //            }
        //            mCode = (++number).ToString("D9");
        //        }
        //    }
        //    return mCode;
        //}

        public string GetAccountGroup()
        {
            var Code = (from TS in ctxTFAT.MasterGroups select TS.Code).Max();
            string str = Code;
            string digits = new string(str.Where(char.IsDigit).ToArray());
            //string letters = new string(str.Where(char.IsLetter).ToArray());
            int number;
            if (!int.TryParse(digits, out number)) //int.Parse would do the job since only digits are selected
            {
                Console.WriteLine("Something weired happened");
            }
            string newStr = (++number).ToString("D9");
            return newStr;
        }

        public string GetProductCode()
        {

            var Code = (from TS in ctxTFAT.ItemMaster
                        select TS.Code).Max();

            string str = Code;
            string digits = new string(str.Where(char.IsDigit).ToArray());
            string letters = new string(str.Where(char.IsLetter).ToArray());

            int number;
            if (!int.TryParse(digits, out number)) //int.Parse would do the job since only digits are selected
            {
                Console.WriteLine("Something weired happened");
            }
            //string newStr = (++number).ToString("D9");
            string newStr = letters + (++number).ToString("D6");

            return newStr;
        }
    }
}