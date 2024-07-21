using System;
using System.Data.Entity;
using System.Text;
using System.Web.Compilation;

namespace Core
{
    public class CoreCommon
    {
        public static object GetTableObject(string TableName)
        {
            Type mType = BuildManager.GetType(string.Format("EntitiModel.{0}", TableName), true);
            return System.Activator.CreateInstance(mType);
        }

        public static Type GetTableType(string TableName)
        {
            return BuildManager.GetType(string.Format("EntitiModel.{0}", TableName), true);
        }

        //public static DbSet GetTableData(string tablename)
        //{
        //    var mType = BuildManager.GetType(string.Format("EntitiModel.{0}", tablename), true);
        //    //TFATERPDatabaseEntities1 ctx = new TFATERPDatabaseEntities1();
        //    //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();
        //    return ctxTFAT.Set(mType);
        //}

        public static string GetString(string[] col)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in col)
            {
                sb.Append(s);
                sb.Append(",");
            }
            if (sb.Length > 0)
            {
                return sb.ToString().Substring(0, sb.Length - 1);
            }
            return null;
        }
    }
}