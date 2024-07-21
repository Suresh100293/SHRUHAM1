//using EntitiModel;
using ALT_ERP3.Controllers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class GSTeWayController : BaseController
    {
        private string mdocument = "";

        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, "Report", Model.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = Model.Document;
            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            ViewBag.ViewDataId = Model.ViewDataId;
            ViewBag.Header = Model.Header;
            ViewBag.Controller = Model.Controller;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;
            mpara = (string.IsNullOrEmpty(Model.Document) == true) ? "" : "para01^" + Model.Document.Substring(6, Model.Document.Length - 6);
            return View(Model);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            return GetGridReport(Model, "R", mpara, false, 0);
        }

        public ActionResult GetExcel(GridOption Model)
        {
            return GetGridData(Model);
        }

        public ActionResult GetJSON(GridOption Model)
        {
            Model.mWhat = "JSN";
            string connstring = GetConnectionString();
            //string mFixedPara = "";
            //if (Model.Para != null)
            //{
            //    mFixedPara = Model.Para.ToString();
            //}
            //if (mFixedPara != "")
            //{
            //    mFixedPara += "~";
            //}

            string mWhat = Model.mWhat ?? "";
            int startIndex = mWhat == "" ? ((Model.page - 1) * Model.rows) + 1 : -1;
            int endIndex = mWhat == "" ? (Model.page * Model.rows) : -1;

            SqlDataAdapter da = new SqlDataAdapter();
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand();
            SqlConnection con = new SqlConnection(connstring);
            cmd = new SqlCommand("dbo.ExecuteReport", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            var date = Model.Date.Replace("-", "/").Split(':');
            Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            cmd.Parameters.Add("@mFormatCode", SqlDbType.VarChar).Value = Model.ViewDataId;
            cmd.Parameters.Add("@mAlias", SqlDbType.VarChar).Value = "";
            cmd.Parameters.Add("@mCurrDec", SqlDbType.TinyInt).Value = 2;
            cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
            cmd.Parameters.Add("@mUserID", SqlDbType.VarChar).Value = "";
            cmd.Parameters.Add("@mReportStartDate", SqlDbType.VarChar).Value = Model.FromDate;
            cmd.Parameters.Add("@mReportEndDate", SqlDbType.VarChar).Value = Model.ToDate;
            cmd.Parameters.Add("@mIsRunBalance", SqlDbType.Bit).Value = false;
            cmd.Parameters.Add("@mOrderBy", SqlDbType.VarChar).Value = Model.sidx != null ? Model.sidx.Replace(",", "") + ' ' + (Model.sidx.Contains("asc") || Model.sidx.Contains("desc") ? "" : Model.sord) : "";
            cmd.Parameters.Add("@mStartIndex", SqlDbType.Int).Value = startIndex;
            cmd.Parameters.Add("@mEndIndex", SqlDbType.Int).Value = endIndex;
            cmd.Parameters.Add("@mRunBalance", SqlDbType.Float).Value = 0;
            cmd.Parameters.Add("@mInsertIntoTable", SqlDbType.VarChar).Value = "";
            cmd.Parameters.Add("@mPara", SqlDbType.VarChar).Value = mpara;
            cmd.Parameters.Add("@mFilter", SqlDbType.VarChar).Value = "";
            cmd.Parameters.Add("@mDocString", SqlDbType.VarChar).Value = mDocString;
            // for output
            cmd.Parameters.Add("@mSumString", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters["@mSumString"].Direction = ParameterDirection.Output;
            cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;
            con.Open();
            da.SelectCommand = cmd;
            da.Fill(dt);
            //string mSumString = (string)cmd.Parameters["@mSumString"].Value;
            con.Close();

            string mGSTNo = Fieldoftable("TfatBranch", "GSTNo", "Code='" + mbranchcode + "'");
            //ctxTFAT.TfatBranch.Where(z => z.Code == mbranchcode).Select(x => x.GSTNo).FirstOrDefault() ?? "";
            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("{");
            jsonBuilder.Append("\"version\":\"1.0.1118\",\"billLists\":[");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                jsonBuilder.Append("{");
                jsonBuilder.Append("\"userGstin\":\"" + mGSTNo + "\",");
                jsonBuilder.Append("\"supplyType\":\"" + dt.Rows[i][0] + "\",");
                jsonBuilder.Append("\"subSupplyType\":1,");
                switch (dt.Rows[i][2].ToString())
                {
                    case "Challan":
                        jsonBuilder.Append("\"docType\":\"CHL\",");
                        break;
                    case "Credit Note":
                        jsonBuilder.Append("\"docType\":\"CNT\",");
                        break;
                    case "Bill of Supply":
                        jsonBuilder.Append("\"docType\":\"BIL\",");
                        break;
                    case "Bill of Entry":
                        jsonBuilder.Append("\"docType\":\"BOE\",");
                        break;
                    case "Others":
                        jsonBuilder.Append("\"docType\":\"OTH\",");
                        break;
                    default:
                        jsonBuilder.Append("\"docType\":\"INV\",");
                        break;
                }
                jsonBuilder.Append("\"docNo\":\"" + dt.Rows[i][3] + "\",");
                jsonBuilder.Append("\"docDate\":\"" + dt.Rows[i][4] + "\",");
                //m = Val(.TextMatrix(n, 44))
                //If m = 0 Then m = 1
                jsonBuilder.Append("\"transtype\":\"" + dt.Rows[i][44].ToString() + "\",");
                jsonBuilder.Append("\"fromGstin\":\"" + dt.Rows[i][6] + "\",");
                jsonBuilder.Append("\"fromTrdName\":\"" + dt.Rows[i][5] + "\",");
                jsonBuilder.Append("\"fromAddr1\":\"" + dt.Rows[i][7] + "\",");
                jsonBuilder.Append("\"fromAddr2\":\"" + dt.Rows[i][8] + "\",");
                jsonBuilder.Append("\"fromPlace\":\"" + dt.Rows[i][9] + "\",");
                jsonBuilder.Append("\"fromPincode\":" + (dt.Rows[i][10].ToString().Trim() == "" || dt.Rows[i][10].ToString() == null ? "0" : dt.Rows[i][10].ToString().Replace(" ", "")) + ",");
                jsonBuilder.Append("\"fromStateCode\":\"" + (dt.Rows[i][11].ToString() == "" ? "1" : dt.Rows[i][11]) + "\",");
                jsonBuilder.Append("\"actualFromStateCode\":\"" + (dt.Rows[i][11].ToString() == "" ? "1" : dt.Rows[i][11]) + "\",");
                if (dt.Rows[i][12].ToString().Trim() == "")
                {
                    jsonBuilder.Append("\"toGstin\":\"" + "URP" + "\",");
                }
                else
                {
                    jsonBuilder.Append("\"toGstin\":\"" + dt.Rows[i][13] + "\",");
                }
                jsonBuilder.Append("\"toTrdName\":\"" + dt.Rows[i][12] + "\",");
                jsonBuilder.Append("\"toAddr1\":\"" + dt.Rows[i][14] + "\",");
                jsonBuilder.Append("\"toAddr2\":\"" + dt.Rows[i][15] + "\",");
                jsonBuilder.Append("\"toPlace\":\"" + dt.Rows[i][16] + "\",");
                jsonBuilder.Append("\"toPincode\":" + (dt.Rows[i][17].ToString().Trim() == "" || dt.Rows[i][17].ToString() == null ? "0" : dt.Rows[i][17].ToString().Replace(" ", "")) + ",");
                jsonBuilder.Append("\"toStateCode\":\"" + (dt.Rows[i][18].ToString() == "" || dt.Rows[i][18].ToString() == null ? "1" : dt.Rows[i][18]) + "\",");
                jsonBuilder.Append("\"actualToStateCode\":\"" + (dt.Rows[i][18].ToString() == "" || dt.Rows[i][18].ToString() == null ? "1" : dt.Rows[i][18]) + "\",");
                //
                decimal mAmt = Convert.ToDecimal(Fieldoftable("StockTax", "Sum(-Taxable)", "ParentKey='" + dt.Rows[i][41] + "'", "N"));
                jsonBuilder.Append("\"totalValue\":" + mAmt + ",");
                mAmt = Convert.ToDecimal(Fieldoftable("StockTax", "Sum(-cGSTAmt)", "ParentKey='" + dt.Rows[i][41] + "'", "N"));
                jsonBuilder.Append("\"cgstValue\":" + mAmt + ",");
                mAmt = Convert.ToDecimal(Fieldoftable("StockTax", "Sum(-sGSTAmt)", "ParentKey='" + dt.Rows[i][41] + "'", "N"));
                jsonBuilder.Append("\"sgstValue\":" + mAmt + ",");
                mAmt = Convert.ToDecimal(Fieldoftable("StockTax", "Sum(-iGSTAmt)", "ParentKey='" + dt.Rows[i][41] + "'", "N"));
                jsonBuilder.Append("\"igstValue\":" + mAmt + ",");
                mAmt = 0;
                //Convert.ToDecimal(fieldoftable("StockTax", "Sum(-cess)", "ParentKey='" + dt.Rows[i][41] + "'", "N"));
                jsonBuilder.Append("\"cessValue\":" + mAmt + ",");
                //
                mAmt = Convert.ToDecimal(Fieldoftable((dt.Rows[i][43].ToString() == "SL" ? "Sales" : "Purchase"), "Sum(Amt)", "TableKey='" + dt.Rows[i][41] + "'", "N"));
                jsonBuilder.Append("\"TotNonAdvolVal\":" + 1 + ",");
                jsonBuilder.Append("\"OthValue\":" + 0 + ",");
                jsonBuilder.Append("\"totInvValue\":" + mAmt + ",");
                //
                switch (dt.Rows[i][34].ToString())
                {
                    case "Rail":
                        jsonBuilder.Append("\"transMode\":" + 2 + ",");
                        break;
                    case "Air":
                        jsonBuilder.Append("\"transMode\":" + 3 + ",");
                        break;
                    case "Ship":
                        jsonBuilder.Append("\"transMode\":" + 4 + ",");
                        break;
                    default:
                        jsonBuilder.Append("\"transMode\":" + 1 + ",");
                        break;
                }
                jsonBuilder.Append("\"transDistance\":" + dt.Rows[i][35] + ",");
                jsonBuilder.Append("\"transporterName\":\"" + dt.Rows[i][36] + "\",");
                jsonBuilder.Append("\"transporterId\":\"" + dt.Rows[i][37] + "\",");
                jsonBuilder.Append("\"transDocNo\":\"" + dt.Rows[i][38] + "\",");
                jsonBuilder.Append("\"transDocDate\":\"" + dt.Rows[i][39] + "\",");
                jsonBuilder.Append("\"vehicleNo\":\"" + dt.Rows[i][40].ToString().Replace(" ", "").Replace("-", "") + "\",");
                jsonBuilder.Append("\"vehicleType\":\"" + "R" + "\","); // regular or oversize
                jsonBuilder.Append("\"mainHsnCode\":" + dt.Rows[i][21] + ",");
                jsonBuilder.Append("\"itemList\":[");
                string mstr = dt.Rows[i][3].ToString(); //srl
                int mcnt = 1;
                for (int r = i; r < dt.Rows.Count; r++)
                {
                    if (dt.Rows[r][3].ToString() != mstr)
                    {
                        //i = r - 1;
                        break;
                    }
                    jsonBuilder.Append("{");
                    jsonBuilder.Append("\"itemNo\":" + mcnt++ + ",");
                    jsonBuilder.Append("\"productName\":\"" + dt.Rows[r][19].ToString().Replace("\"", "") + "\",");
                    jsonBuilder.Append("\"productDesc\":\"" + dt.Rows[r][20].ToString().Replace("\"", "") + "\",");
                    jsonBuilder.Append("\"hsnCode\":\"" + dt.Rows[r][21].ToString().Replace(" ", "") + "\",");
                    jsonBuilder.Append("\"quantity\":" + dt.Rows[r][23] + ",");
                    jsonBuilder.Append("\"qtyUnit\":\"" + dt.Rows[r][22] + "\",");
                    jsonBuilder.Append("\"taxableAmount\":" + dt.Rows[r][24] + ",");
                    jsonBuilder.Append("\"sgstRate\":" + dt.Rows[r][26] + ",");
                    jsonBuilder.Append("\"cgstRate\":" + dt.Rows[r][27] + ",");
                    jsonBuilder.Append("\"igstRate\":" + dt.Rows[r][28] + ",");
                    jsonBuilder.Append("\"cessRate\":" + dt.Rows[r][29] + ",");
                    jsonBuilder.Append("\"cessNonAdvol\":" + 0);
                    jsonBuilder.Append("},");
                }
                i += (mcnt - 1);
                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                jsonBuilder.Append("]},");
            }
            jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            jsonBuilder.Append("]}");
            string mfile = mdocument == "" ? "eWayBill-" + mGSTNo : "eWayBill-" + mGSTNo + "-" + mdocument;
            string attachment = "attachment; filename=" + mfile + @".json";
            Response.ClearContent();
            Response.AddHeader("content-disposition", attachment);
            Response.ContentType = "application/json";
            Response.Write(jsonBuilder);
            Response.End();
            return null;
        }
    }
}