using System;
using System.Data;
using System.Text;

namespace Common
{
    public class JQGridHelper
    {
        //private static //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();

        public static string JsonForJqgrid(DataTable dt, int pageSize, int totalRecords, int page, String msumstring = "")
        {
            int totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);
            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("{");
            jsonBuilder.Append("\"total\":" + totalPages + ",\"page\":" + page + ",\"records\":" + (totalRecords) + ",\"rows\"");
            jsonBuilder.Append(":[");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                jsonBuilder.Append("{\"i\":" + (i) + ",\"cell\":[");
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    jsonBuilder.Append("\"");
                    //var mvar = dt.Rows[i][j].ToString();
                    //if (mvar==null)
                    //{
                    //    mvar = "";
                    //}
                    ////dt.Rows[i][j].ToString()
                    jsonBuilder.Append(dt.Rows[i][j].ToString().Replace("\"", "").Replace(@"\", "").Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace("\b", ""));
                    jsonBuilder.Append("\",");
                }
                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                jsonBuilder.Append("]},");
            }
            if (dt.Rows.Count > 0)
                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);

            jsonBuilder.Append("]");

            if (msumstring != "")
            {
                jsonBuilder.Append(msumstring);
            }
            jsonBuilder.Append("}");
            return jsonBuilder.ToString();
        }

        //public static DataTable GetDataTable(GridOption Model)
        //{
        //    DataTable dt = new DataTable();
        //    SqlConnection conn = null;
        //    try
        //    {
        //        string sql = Model.query;
        //        conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ALT_ERP21EntitiesConnectionString"].ConnectionString);
        //        SqlDataAdapter adap = new SqlDataAdapter(sql, conn);
        //        conn.Open();
        //        var rows = adap.Fill(dt);
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //    finally
        //    {
        //        try
        //        {
        //            if (ConnectionState.Closed != conn.State)
        //            {
        //                conn.Close();
        //            }
        //        }
        //        catch
        //        {
        //        }
        //    }
        //    return dt;
        //}

        //public static int GetTotalCount(GridOption Model)
        //{
        //    SqlConnection conn = null;
        //    try
        //    {
        //        conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ALT_ERP21EntitiesConnectionString"].ConnectionString);
        //        SqlCommand comm = new SqlCommand(Model.queryforcount, conn);
        //        conn.Open();
        //        return (int)comm.ExecuteScalar();
        //    }
        //    catch
        //    {
        //    }
        //    finally
        //    {
        //        try
        //        {
        //            if (ConnectionState.Closed != conn.State)
        //            {
        //                conn.Close();
        //            }
        //        }
        //        catch
        //        {
        //        }
        //    }
        //    return -1;
        //}

        public static string ConvertDate(string Date)
        {
            var cDate = Date.Split('/');
            return cDate[0].Trim() + "/" + cDate[1].Trim() + "/" + cDate[2].Trim();
        }

        //public static string GetGridContent(GridOption Model)
        //{

        //    List<string> header = new List<string>();

        //    var HeaderList = (from TS in ctxTFAT.TfatSearch
        //                      where TS.Code == Model.Code && TS.IsHidden != true && TS.CalculatedCol != true
        //                      && TS.ColField != "0" && TS.ColField != " "
        //                      orderby TS.Sno
        //                      select TS.ColHead).ToList();

        //    var HFieldList = (from TS in ctxTFAT.TfatSearch
        //                      where TS.Code == Model.Code && TS.IsHidden != true && TS.CalculatedCol != true
        //                      && TS.ColField != "0" && TS.ColField != " " && TS.ColField != "*GetStock(Stock.Code,Stock.Store,Stock.Branch,%ACCENDDATE)"
        //                      orderby TS.Sno
        //                      select TS.ColField).ToList();


        //    for (int i = 0; i < HFieldList.Count; i++)
        //    {
        //        for (int j = 0; j < HeaderList.Count; j++)
        //        {
        //            if (i == j)
        //            {
        //                if (HFieldList[i] == "Ledger.Narr")
        //                {
        //                    HFieldList[i] = "(SELECT REPLACE(REPLACE(Ledger.Narr, CHAR(13), ' '), CHAR(10), ' '))";
        //                }
        //                header.Add(HFieldList[i] + " as " + HeaderList[j].Replace(".", " ").Replace(" ", "").Replace("-", "").Replace("%", "").Replace("(", "").Replace(")", ""));
        //            }
        //        }
        //    }
        //    var value = string.Join(",", header).ToString();
        //    var headerval = string.Join(",", HeaderList).ToString().Replace(".", " ").Replace(" ", "").Replace("-", "").Replace("%", "").Replace("(", "").Replace(")", "");
        //    var GridContent = value + "&" + headerval;
        //    return GridContent;
        //}
    }
}