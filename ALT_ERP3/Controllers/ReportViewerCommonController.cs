using System.Web.Mvc;
using ALT_ERP3.Areas.Accounts.Models;

namespace ALT_ERP3.Controllers
{
    public class ReportViewerCommonController : ReportController
    {
        public ActionResult Index(GridOption Model, string mReportType, string mParaString = "", bool mRunning = false, decimal mopening = -1, string mpageorient = "Landscape")
        {
            PreExecute(Model);
            if (mReportType == null || mReportType == "")
            {
                mReportType = "R";
            }

            if (Model.SelectContent == null)
            {
                Model.SelectContent = "";
            }

            mParaString = mParaString ?? "" + rep_SaveParameters(Model.SelectContent.Trim());
            if (mParaString == null || mParaString == "")
            {
                mParaString = "Code^" + Model.Code;
            }
            mParaString += (mpara != "" ? "~" + mpara : "");
            string mname = NameofAccount(Model.Code);
            if (mname == "") mname = NameofAccount(Model.Code, "I");
            string[] mArr = { "", Model.Code + "-" + mname, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", Model.Closing.ToString(), Model.Code + "-" + mname, Model.Opening.ToString() };
            ViewBag.ReportViewer = CreateSSRSReport(Model, mReportType, mArr, mpageorient, mParaString, Model.Opening);
            return View();
        }
    }
}

//ReportViewer rv = new Microsoft.Reporting.WebForms.ReportViewer();
//rv.ProcessingMode = ProcessingMode.Local;
//rv.SizeToReportContent = true;
//rv.ZoomMode = ZoomMode.PageWidth;
//rv.ShowToolBar = true;
//rv.AsyncRendering = true;
//rv.BorderStyle = System.Web.UI.WebControls.BorderStyle.Solid;
//rv.Reset();

//// get query from reportheader/tfatsearch
//string connstring = GetConnectionString();
//string mFixedPara = "";
//if (Model.Para != null)
//{
//    mFixedPara = Model.Para.ToString();
//}
//if (mFixedPara != "")
//{
//    mFixedPara = mFixedPara + "~";
//}
//mParaString = mFixedPara + mParaString;

//string mWhat = Model.mWhat == null ? "" : Model.mWhat;
//int startIndex = mWhat == "" ? (Model.page - 1) * Model.rows + 1 : -1;
//int endIndex = mWhat == "" ? (Model.page * Model.rows) : -1;

//string mFilter = "";
//if (Model.searchField != "" && Model.searchField != null)
//{
//    switch (Model.searchOper)
//    {
//        case "eq":
//            mFilter = Model.searchField + " = '" + Model.searchString + "'";
//            break;
//        case "ne":
//            mFilter = Model.searchField + " <> " + Model.searchString;
//            break;
//        case "bw":
//            mFilter = Model.searchField + " like '" + Model.searchString + "%'";
//            break;
//        case "bn":
//            mFilter = Model.searchField + " Not like '" + Model.searchString + "%'";
//            break;
//        case "ew":
//            mFilter = Model.searchField + " like '%" + Model.searchString + "'";
//            break;
//        case "en":
//            mFilter = Model.searchField + " Not like '%" + Model.searchString + "'";
//            break;
//        case "cn":
//            mFilter = Model.searchField + " like '%" + Model.searchString + "%'";
//            break;
//        case "in":
//            mFilter = Model.searchField + " like '%" + Model.searchString + "%'";
//            break;
//        case "nc":
//            mFilter = Model.searchField + " Not like '%" + Model.searchString + "%'";
//            break;
//        case "ni":
//            mFilter = Model.searchField + " Not like '%" + Model.searchString + "%'";
//            break;
//    }
//}

//SqlConnection con = new SqlConnection(connstring);
//SqlCommand cmd = new SqlCommand();
//cmd = new SqlCommand("dbo.SPTFAT_ExecuteSSRSReport", con);
//cmd.CommandType = CommandType.StoredProcedure;

//cmd.Parameters.Add("@mFormatCode", SqlDbType.VarChar).Value = Model.ViewDataId;
//cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
//cmd.Parameters.Add("@mUserID", SqlDbType.VarChar).Value = muserid;
//if (mReportType == "M")
//{
//    cmd.Parameters.Add("@mReportStartDate", SqlDbType.VarChar).Value = "2018-01-01";
//    cmd.Parameters.Add("@mReportEndDate", SqlDbType.VarChar).Value = "2019-01-01";
//}
//else
//{
//    if (Model.Date != null)
//    {
//        var date = Model.Date.Replace("-", "/").Split(':');
//        Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
//        Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
//    }
//    else
//    {
//        Model.FromDate = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
//        Model.ToDate = (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
//    }
//    cmd.Parameters.Add("@mReportStartDate", SqlDbType.VarChar).Value = Model.FromDate;
//    cmd.Parameters.Add("@mReportEndDate", SqlDbType.VarChar).Value = Model.ToDate;
//}
//cmd.Parameters.Add("@mPara", SqlDbType.VarChar).Value = mParaString;
//cmd.Parameters.Add("@mFilter", SqlDbType.VarChar).Value = mFilter;
//con.Open();
//SqlDataAdapter da = new SqlDataAdapter();
//da.SelectCommand = cmd;
////
//string mreportheading = "";
//Model.AccountDescription = ctxTFAT.ReportHeader.Where(z => z.Code == Model.ViewDataId).Select(x => x.FormatHead).FirstOrDefault() ?? "";
//if (Model.AccountDescription != "")
//{
//    Model.AccountDescription = Model.AccountDescription.Replace("%RepStartDate", Model.FromDate);
//    Model.AccountDescription = Model.AccountDescription.Replace("%RepEndDate", Model.ToDate);
//    mreportheading = Model.AccountDescription.Trim();
//}
////var mConn = GetConnectionString();
////SqlConnection conx = new SqlConnection(mConn);
////SqlDataAdapter adp = new SqlDataAdapter("Select Master.*,TfatBranch.*,(Select sum(x.Debit - x.Credit) from Ledger x where x.Code = Master.Code) as Balance from Master,TfatBranch,Address Where TfatBranch.Code='" + mbranchcode + "' And Address.Code=Master.Code;", conx);
////SqlConnection conx1 = new SqlConnection(mConn); SqlDataAdapter adp1 = new SqlDataAdapter("SELECT RECORDKEY, ComplaintId, PendingReasonId, Prefix,(select name from repair where code = RepairStatus)as RepairName,RepairStatus, SrNo, VisitComment, VisitDate, VisitEnggId, VisitInTime, VisitOutTime, TOUCHVALUE, ENTEREDBY, LASTUPDATEDATE, ServiceCenterId,(select name from scuserinfo where code = VisitEnggId) as Technician from complaintvisitdetails WHERE ComplaintId IS NOT NULL  and ComplaintId = " + "'" + Model.Srl + "'" + ";", conx1);
////SqlConnection conx2 = new SqlConnection(mConn); SqlDataAdapter adp2 = new SqlDataAdapter("SELECT  RecordKey, Code, FirstName, LastName, Adrl1, Adrl2, Adrl3, Pin, StateId, CityId, MobileNo, Tel1, Email, LandMark, Locality, DocDate  from sccustomermaster where sccustomermaster.Code = " + "'" + Model.CustomerId + "'" + ";", conx2);
////SqlConnection conx3 = new SqlConnection(mConn); SqlDataAdapter adp3 = new SqlDataAdapter("SELECT  RECORDKEY, CustomerId, PartCode, Prefix, ProductId, Qty, Rate, SerialNo, ServiceCenterId, TOUCHVALUE, ENTEREDBY, LASTUPDATEDATE, ComplaintId, VisitId, SrNo, (select name from itemdetail where code = PartCode and grp = '000009' and branch = 'HO0000') as PartName,Qty* Rate as Amount from ComplaintPart  where ComplaintPart.ComplaintId = " + "'" + Model.Srl + "'" + ";", conx3);

//DS_SalesInvoice ds = new DS_SalesInvoice();
//da.Fill(ds, ds.DataTable1.TableName); // DataTable1 is the name of the tableadapter in Datasource DS_xReport.xsd
//da.Dispose();
//con.Close();
//con.Dispose();
////adp1.Fill(ds, ds.ComplaintVisitDetails.TableName);
////adp2.Fill(ds, ds.SCCustomerMaster.TableName);
////adp3.Fill(ds, ds.ComplaintPart.TableName);
//rv.LocalReport.ReportPath = Server.MapPath("/Reports/REP_" + Model.ViewDataId + "_L.rdlc");
////rv.LocalReport.ReportPath = Server.MapPath("/Reports/REP_xReport.rdlc");
//ReportDataSource rds = new ReportDataSource("DataSet1", ds.Tables[0]); // DataSet1 is defined in .rdlc

////ReportDataSource rds1 = new ReportDataSource("ComplDataSet", ds.Tables[3]);
////ReportDataSource rds2 = new ReportDataSource("CVisitDataSet", ds.Tables[1]);
////ReportDataSource rds3 = new ReportDataSource("CustDataSet", ds.Tables[2]);
//List<ReportParameter> reportParams = new List<ReportParameter>();
//reportParams.Add(new ReportParameter("muserid", muserid));
//reportParams.Add(new ReportParameter("mreportheading", mreportheading));
////reportParams.Add(new ReportParameter("mperiod", FPerd));
////reportParams.Add(new ReportParameter("mbranch", mbranchcode));
//rv.LocalReport.SetParameters(reportParams);
////ReportParameter[] mpara = new ReportParameter[1];
////mpara.SetValue(new ReportParameter("mreportheading",mreportheading), 0);
////rv.LocalReport.SetParameters(mpara);
//rv.LocalReport.DataSources.Clear();
//rv.LocalReport.DataSources.Add(rds);
////reportViewer.LocalReport.DataSources.Add(rds1);
////reportViewer.LocalReport.DataSources.Add(rds2);
////reportViewer.LocalReport.DataSources.Add(rds3);
//rv.LocalReport.Refresh();

//ViewBag.ReportViewer = rv;
//ds.Dispose();