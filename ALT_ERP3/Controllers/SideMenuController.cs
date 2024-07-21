using ALT_ERP3.Models;
using Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Controllers
{
    public class SideMenuController : BaseController
    {

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            return GetGridReport(Model, "R", "", false, 0);
        }

        // GET: SideMenu
        public ActionResult GetSideMenu1(SideMenuVM Model)
        {
            string HireSpclRemark = "";
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand();// ("SPTFAT_ReceivableAnalysis", tfat_conx);
            cmd.Connection = tfat_conx;
            Model = new SideMenuVM();
            string mStr = "";
            List<string> Menus = new List<string>();
            Menus.Add("Pie Chart");
            Menus.Add("Outstanding Chart");
            Menus.Add("New Outstanding Report");

            Model.Menus = Menus;
            List<string> Code = new List<string>();
            Code.Add("Pie Chart");
            Code.Add("Outstanding Chart");
            Code.Add("New Outstanding Report");
            Model.codes = Code;

            foreach (var item in Model.codes)
            {
                mStr += item + "|";
            }
            if (!String.IsNullOrEmpty(mStr))
            {
                mStr = mStr.Substring(0, mStr.Length - 1);
            }
            ViewBag.Codes = mStr;

            //fill Pie chart
            var Chart = GetChartData();
            Model.PieChart = Chart;

            GridOption mModel = new GridOption();
            mModel.ViewDataId = "New Outstanding Report";
            mModel.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            mModel.ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
            mModel.FromDate = (Convert.ToDateTime(mModel.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            mModel.ToDate = (Convert.ToDateTime(mModel.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            //fill Bar chart
            var BarChartData = OutstandingReports(mModel);
            Model.BarChart = BarChartData;


            #region Outstanding ChartData



            #endregion



            var html = ViewHelper.RenderPartialView(this, "_SideMenuDesign", Model);
            return Json(new
            {
                Html = html,
                JsonRequestBehavior.AllowGet
            });
        }

        public ActionResult GetSideMenu(ActiveObjectsVM Model)
        {
            


            var html = ViewHelper.RenderPartialView(this, "_SideMenuDesign", Model);
            return Json(new
            {
                Html = html,
                JsonRequestBehavior.AllowGet
            });
        }
        //Column Both
        private BarChartVM GetBarChartData()
        {
            var BarChartData = new BarChartVM();

            var labels = new List<string>();

            labels.Add("Jan");
            labels.Add("Feb");
            labels.Add("Mar");
            BarChartData.labels = labels;


            var datasets = new List<BarChartChildVM>();
            var childModel = new BarChartChildVM();

            childModel.label = "Earning";
            //childModel.backgroundColor = @"rgba(255,99,132,0.2)";
            childModel.borderColor = @"rgba(255,99,132,1)";
            childModel.borderWidth = 2;
            childModel.hoverbackgroundColor = @"rgba(255,99,132,0.4)";
            childModel.hoverborderColor = @"rgba(255,99,132,1)";

            var datalist = new List<decimal>();

            foreach (var label in labels)
            {
                if (label == "Jan")
                {
                    datalist.Add(65);
                }
                if (label == "Feb")
                {
                    datalist.Add(56);
                }
                if (label == "Mar")
                {
                    datalist.Add(45);
                }

            }

            childModel.data = datalist;
            datasets.Add(childModel);
            BarChartData.datasets = datasets;

            return BarChartData;
        }

        //Pie Chart
        private GraphDesignVM GetChartData()
        {
            var Model = new GraphDesignVM();

            var labels = new List<string>();
            labels.Add("Green");
            labels.Add("Blue");
            labels.Add("Gray");
            labels.Add("Purple");

            Model.labels = labels;

            var Datasets = new List<GraphChildElementVM>();
            var ChildModel = new GraphChildElementVM();

            var Backgroundcolorlist = new List<string>();
            var datalist = new List<int>();

            foreach (var item in labels)
            {
                if (item == "Green")
                {
                    Backgroundcolorlist.Add("#2ecc71");
                    datalist.Add(12);
                }
                if (item == "Blue")
                {
                    Backgroundcolorlist.Add("#3498db");
                    datalist.Add(20);
                }
                if (item == "Gray")
                {
                    Backgroundcolorlist.Add("#95a5a6");
                    datalist.Add(18);
                }
                if (item == "Purple")
                {
                    Backgroundcolorlist.Add("#9b59b6");
                    datalist.Add(50);
                }

            }

            ChildModel.backgroundColor = Backgroundcolorlist;
            ChildModel.data = datalist;

            Datasets.Add(ChildModel);
            Model.datasets = Datasets;

            return Model;
        }

        public BarChartVM OutstandingReports(GridOption Model)
        {


            #region Set Parameters

            ppara01 = "";
            ppara02 = "";
            ppara03 = "";
            ppara04 = "";
            ppara05 = "";
            ppara06 = "";
            ppara07 = "";
            ppara08 = "";
            ppara09 = "";
            ppara10 = "";
            ppara11 = "";
            ppara12 = "";
            ppara13 = "";
            ppara14 = "";
            ppara15 = "";
            ppara16 = "";
            ppara17 = "";
            ppara18 = "";
            ppara19 = "";
            ppara20 = "";
            ppara21 = "";
            ppara22 = "";
            ppara23 = "";
            ppara24 = "";
            mpara = "";

            var Branch = "";
            foreach (var item in ctxTFAT.TfatBranch.Where(x => x.Category != "Area" && x.Code != "G00000").Select(x => x.Code).ToList())
            {
                Branch += item + ",";
            }
            if (!String.IsNullOrEmpty(Branch))
            {
                Branch = Branch.Substring(0, Branch.Length - 1);
            }

            #endregion

            //var date = Model.Date.Replace("-", "/").Split(':');
            //Model.FromDate = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            //Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand();// ("SPTFAT_ReceivableAnalysis", tfat_conx);
            cmd.Connection = tfat_conx;
            bool AddPrameter = false;

            ExecuteStoredProc("Drop Table ztmp_zOS");
            AddPrameter = true;
            cmd.CommandText = "SPTFAT_ReceivableAnalysis";

            var GsetBaseGr = Model.Customer == true ? "D" : ctxTFAT.Master.Where(x => x.Code == Model.Code).Select(x => x.BaseGr).FirstOrDefault();
            ppara07 = "Yes";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@mBaseGr", SqlDbType.VarChar).Value = "D";
            cmd.Parameters.Add("@mDate", SqlDbType.Date).Value = Model.ToDate;
            cmd.Parameters.Add("@mDate2", SqlDbType.Date).Value = Model.FromDate;
            cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = Branch == null || Branch == "" ? mbranchcode : Branch;
            cmd.Parameters.Add("@mLocations", SqlDbType.VarChar).Value = Fieldoftable("Warehouse", "Code", "Name='" + ppara01 + "'");
            cmd.Parameters.Add("@mConsiderCRPR", SqlDbType.Bit).Value = ppara06 == "Yes" ? true : false;
            cmd.Parameters.Add("@mSuppressZero", SqlDbType.Bit).Value = ppara08 == "Yes" ? true : false;
            cmd.Parameters.Add("@mArea", SqlDbType.VarChar).Value = "";// ppara04;
            cmd.Parameters.Add("@mParties", SqlDbType.VarChar).Value = Model.Code == null ? "" : Model.Code;
            cmd.Parameters.Add("@mGrps", SqlDbType.VarChar).Value = Fieldoftable("MasterGroups", "Code", "Name='" + ppara16 + "'");
            cmd.Parameters.Add("@mSalesman", SqlDbType.VarChar).Value = ppara02;//FieldoftableNumber("Salesman", "Code", "Name='" + 
            cmd.Parameters.Add("@mBroker", SqlDbType.VarChar).Value = ppara03;//FieldoftableNumber("Broker", "Code", "Name='" + ppara03 + "'");
            cmd.Parameters.Add("@mCategory", SqlDbType.VarChar).Value = ppara05;
            cmd.Parameters.Add("@mRefTillDate", SqlDbType.Bit).Value = ppara07 == "Yes" ? true : false;
            cmd.Parameters.Add("@mDocString", SqlDbType.VarChar).Value = mDocString;
            cmd.Parameters.Add("@mCustomer", SqlDbType.VarChar).Value = "0";
            cmd.Parameters.Add("@mBillSubmission", SqlDbType.VarChar).Value = false;


            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            tfat_conx.Close();
            tfat_conx.Dispose();

            DataTable dt = new DataTable();
            string Query = "select  Cast(sum((Case when ODueDays between 0 and 30 then Pending else 0 end)) as Decimal(14,2)) as [0-30], Cast(sum((Case when ODueDays between 30+1  and 60 then Pending else 0 end)) as Decimal(14,2)) as [30-60], Cast(sum((Case when ODueDays between 60+1  and 90 then Pending else 0 end)) as Decimal(14,2)) as [60-90], Cast(sum((Case when ODueDays between 90+1  and 120 then Pending else 0 end)) as Decimal(14,2)) as [90-120], Cast(sum((Case when ODueDays between 120+1  and 150 then Pending else 0 end)) as Decimal(14,2)) as [120-150], Cast(Sum((Case when ODueDays between 150+1  and 180 then Pending else 0 end)) as Decimal(14,2)) as [150-180], Cast(sum((Case when ODueDays>180 then Pending else 0 end)) as Decimal(14,2)) as [>180],  Cast(sum(Pending-OnAccount ) as Decimal(14,2)) as [Outstanding] from ztmp_zOS";
            tfat_conx = new SqlConnection(GetConnectionString());
            cmd = new SqlCommand(Query, tfat_conx);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.Fill(dt);
            tfat_conx.Close();
            tfat_conx.Dispose();

            #region FillData
            var BarChartData = new BarChartVM();

            var labels = new List<string>();
            var LastRow = dt.Columns.Count - 1;
            for (int i = 0; i < dt.Columns.Count - 1; i++)
            {
                labels.Add(dt.Columns[i].ToString());
            }
            BarChartData.labels = labels;


            var datasets = new List<BarChartChildVM>();
            var childModel = new BarChartChildVM();


            //childModel.backgroundColor = @"rgba(255,99,132,0.2)";
            childModel.borderColor = @"rgba(255,99,132,1)";
            childModel.borderWidth = 2;
            childModel.hoverbackgroundColor = @"rgba(255,99,132,0.4)";
            childModel.hoverborderColor = @"rgba(255,99,132,1)";

            var datalist = new List<decimal>();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (i != LastRow)
                {
                    var Amt = Convert.ToDecimal(dt.Rows[0][i]);
                    datalist.Add(Amt);
                }
                else
                {
                    childModel.label = "Total :- " + dt.Rows[0][i].ToString();
                }

            }
            childModel.data = datalist;
            datasets.Add(childModel);
            BarChartData.datasets = datasets;
            #endregion
            return BarChartData;
        }
    }
}