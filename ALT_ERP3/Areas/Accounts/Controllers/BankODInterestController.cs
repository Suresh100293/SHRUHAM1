/* -----------------------------------------
   Copyright 2019, Suchan Software Pvt. Ltd.
   ----------------------------------------- */
using ALT_ERP3.Controllers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class BankODInterestController : ReportController
    {
        // GET: Accounts/ReportCentre
        public ActionResult Index(GridOption Model)
        {
            if (muserid == null || muserid == "")
            {
                Response.Redirect("/Account/Login");
            }
            GetAllMenu(Session["ModuleName"].ToString());
            string murl = Request.Url.ToString();
            UpdateAuditTrail(mbranchcode, "Report", Model.Header, "", DateTime.Now, 0, "", murl, "NA");
            Model.FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
            Model.ToDate = GetEffectiveDate().ToShortDateString();
            moptioncode = Model.OptionCode;
            msubcodeof = Model.ViewDataId;
            mmaintype = Model.MainType;
            var reportheader = ctxTFAT.ReportHeader.Where(z => z.SubCodeOf == msubcodeof && z.DefaultReport == true).Select(x => new { x.Code, x.InputPara }).FirstOrDefault();
            if (reportheader == null)
            {
                reportheader = ctxTFAT.ReportHeader.Where(z => z.SubCodeOf == msubcodeof).Select(x => new { x.Code, x.InputPara }).FirstOrDefault();
            }
            if (reportheader != null)
            {
                Model.ViewCode = reportheader.Code == null ? "" : reportheader.Code;
                Model.IsFormatSelected = (Model.ViewCode == null || Model.ViewCode == "") ? false : true;
                Model.AddOnParaList = GetReportInputPara(reportheader.InputPara);
            }
            return View("ReportStandard/Index", Model);
        }


        #region executereport
        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            Model.MainType = mmaintype;
            ExecuteBankODCalc(Model.Date);
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        private void ExecuteBankODCalc(string mDate)
        {
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("SPTFAT_LedgerDailySummary", tfat_conx)
            {
                CommandType = CommandType.StoredProcedure
            };
            var date = mDate.Replace("-", "/").Split(':');
            cmd.Parameters.Add("@mCode", SqlDbType.VarChar).Value = ppara01;
            cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = mbranchcode;
            cmd.Parameters.Add("@mStartDate", SqlDbType.Date).Value = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            cmd.Parameters.Add("@mEndDate", SqlDbType.Date).Value = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            cmd.Parameters.Add("@mDocString", SqlDbType.VarChar).Value = mDocString;
            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            tfat_conx.Close();

            decimal mbelowodint = Convert.ToDecimal(ppara02);
            decimal maboveodint = Convert.ToDecimal(ppara03);
            decimal modlimit = FieldoftableNumber("Masterinfo", "ODLimit", "Code='" + ppara01 + "'");

            DataTable mdt = GetDataTable("Select StartDate from ztmp_TempDaily order by StartDate");
            foreach (DataRow mrow in mdt.Rows)
            {
                string mdate = MMDDYY(Convert.ToDateTime((mrow["StartDate"])));
                decimal mbalance = FieldoftableNumber("Ledger", "Sum(Debit-Credit)", "MainType<>'MV' and MainType<>'PV' and Code='" + ppara01 + "' and Year(ClearDate)<>1950 and DocDate<='" + mdate + "' and Branch='" + mbranchcode + "' and left(AUTHORISE,1)='A'" + (string.IsNullOrEmpty(mDocString) ? "" : " and Charindex(Type,'" + mDocString + "')=0"));
                decimal mint = 0;
                if (mbalance < 0)
                {
                    mbalance *= -1;
                    if (mbalance >= modlimit)
                    {
                        mint = (mbalance * mbelowodint / 100) / 365;
                    }
                    else if (mbalance < modlimit)
                    {
                        mint = (mbalance * maboveodint / 100) / 365;
                    }
                }
                ExecuteStoredProc("Update ztmp_TempDaily Set BankBalance=" + mbalance + ",Interest=" + mint + " where StartDate='" + mdate + "'");
            }
        }
        #endregion executereport
    }
}