using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;

namespace ReportViewer2013
{
    public partial class View : System.Web.UI.Page
    {
        /// <summary>
        /// Usage: http://localhost/View.aspx?Path=Report1&Param1=216&OtherParam=etc
        /// example: http://localhost:1337/View.aspx?Path=SQLTables.rdl
        /// </summary>
        #region Private Properties
        #region wrappers for web.config
        /// <summary>
        /// Report Path setting that was stored in the web.config
        /// </summary>
        private string ReportPath
        {
            get
            {   
                //If you get a compile error on the next line, you can fix it by setting - Project, Properties, Settings: add a setting called ReportPath, with type "String", with a value "/Reports" (matching the Reports folder in this project);
                //It is also stored in the web.config file, which is nice when you go to deploy your project.
                string reportPath = "/Reports/";
                if (reportPath.StartsWith("/"))
                    return Server.MapPath(reportPath);
                else if (Directory.Exists(reportPath))
                    return reportPath;
                else
                    return Server.MapPath(reportPath);
            }
            //set { }
        }

        /// <summary>
        /// Database connection string that was stored in the web.config
        /// </summary>
        private string DBConnectionString
        {
            get
            {
                //If you get a compile error on the next line, you can fix it 
                //by setting Project, Properties, Settings: 
                //add a setting called DnsReport, with type "ConnectionString", pointed at your DB;
                return "data source=151.106.32.145\\SQL2019,2521;initial catalog=ALT_ERP21;user id=sa;password=suc357";

                //return ""; 
            }
            //set { }
        }
        #endregion

        #region Wrapper for QueryString parameters
        /// <summary>
        /// all report params that were passed-in via the QueryString
        /// </summary>
        private System.Collections.Hashtable ReportParameters
        {
            get
            {
                System.Collections.Hashtable re = new System.Collections.Hashtable();
                //gather any params so they can be passed to the report
                foreach (string key in Request.QueryString.AllKeys)
                {
                    if (key.ToLower() != "path")//ignore the “path” param. It describes the report's file path
                    {
                        re.Add(key, Request.QueryString[key]);
                    }
                }
                return re;
            }
            //set { }
        }

        /// <summary>
        /// the report file info (filename, ext, path, etc)
        /// </summary>
        private FileInfo ReportFile
        {
            get
            {
                FileInfo re = null;
                try
                {
                    string reportFullPath = "", reportName = "";

                    if (Request.QueryString["path"] != null)
                    {
                        reportName = Request.QueryString["path"];
                    }

                    //check to make sure the file ACTUALLY exists, before we start working on it
                    if (File.Exists(Path.Combine(this.ReportPath, reportName)))
                    {
                        reportFullPath = Path.Combine(this.ReportPath, reportName);
                        reportName = reportName.Substring(0, reportName.LastIndexOf(".") - 1);
                    }
                    else if (File.Exists(Path.Combine(this.ReportPath, reportName + ".rdl")))
                        reportFullPath = Path.Combine(this.ReportPath, reportName + ".rdl");
                    else if (File.Exists(Path.Combine(this.ReportPath, reportName + ".rdlc")))
                        reportFullPath = Path.Combine(this.ReportPath, reportName + ".rdlc");

                    if (reportFullPath != "")
                        re = new FileInfo(reportFullPath);
                }
                finally { }

                return re;
            }
            //set { }
        }

        /// <summary>
        /// the Report file (.rdl/.rdlc) de-serialized into an object
        /// </summary>
        private RDL.Report ReportDefinition
        {
            get
            {
                FileInfo reportFile = this.ReportFile;
                if (reportFile != null)
                    return RDL.Report.GetReportFromFile(reportFile.FullName);
                else
                    return new RDL.Report();
            }
        }
        #endregion
        #endregion

        #region Event Handlers
        protected void Page_Load(object sender, EventArgs e)
        {
            lblError.Text = "";
            if (!Page.IsPostBack)
            {
                rvReportViewer.ReportError += rvReportViewer_ReportError;
                //call the report
                if (Request.QueryString["path"] != null)
                    ShowReport(); //with data
                else
                    ShowBlankReport(); //blank (default)
            }
        }

        protected void btnDownload_Click(object sender, EventArgs e)
        {
            DownloadReportFile();
        }

        void rvReportViewer_ReportError(object sender, ReportErrorEventArgs e)
        {
            lblError.Text = e.Exception.Message;
            //throw new NotImplementedException();
        }

        #endregion

        #region Private Helper Methods
        #region  Show Report
        /// <summary>
        /// Simple example to show the basics and to test that your initial config and prerequisites are correct
        /// </summary>
        private void ShowBlankReport()
        {
            //look up the report file
            string reportPath = ReportPath + "Example.rdlc";

            //In the example report, there are no parameters to load
            //Otherwise, I would do that first

            //attach an empty dataset, because all reports need a dataset
            System.Data.DataSet ds = new System.Data.DataSet() { Tables = { new System.Data.DataTable() } };
            ReportDataSource rds = new ReportDataSource("dsTableList", ds.Tables[0]);
            rvReportViewer.LocalReport.DataSources.Clear();
            rvReportViewer.LocalReport.DataSources.Add(rds);

            //apply the data to the report
            rvReportViewer.LocalReport.Refresh();

            //the viewer will load the report definition from the file
            rvReportViewer.LocalReport.ReportPath = reportPath;
        }

        /// <summary>
        /// Run one of your reports
        /// </summary>
        protected void ShowReport()
        {
            //adapted from http://www.codeproject.com/Articles/37845/Using-RDLC-and-DataSets-to-develop-ASP-NET-Reporti

            FileInfo reportFullPath = this.ReportFile;
            //check to make sure the file ACTUALLY exists, before we start working on it
            if (reportFullPath != null)
            {
                //map the reporting engine to the .rdl/.rdlc file
                LoadReportDefinitionFile(rvReportViewer.LocalReport, reportFullPath);

                //  1. Clear Report Data
                rvReportViewer.LocalReport.DataSources.Clear();

                //  2. Load new data
                // Look-up the DB query in the "DataSets" element of the report file (.rdl/.rdlc which contains XML)
                RDL.Report reportDef = this.ReportDefinition;

                // Run each query (usually, there is only one) and attach each to the report
                foreach (RDL.DataSet ds in reportDef.DataSets)
                {
                    //copy the parameters from the QueryString into the ReportParameters definitions (objects)
                    ds.AssignParameters(this.ReportParameters);
                    
                    //run the query to get real data for the report
                    System.Data.DataTable tbl = ds.GetDataTable(DBConnectionString);

                    //attach the data/table to the Report's dataset(s), by name
                    ReportDataSource rds = new ReportDataSource();
                    rds.Name = ds.Name; //This refers to the dataset name in the RDLC file
                    rds.Value = tbl;
                    rvReportViewer.LocalReport.DataSources.Add(rds);
                }

                //Load any other report parameters (which are not part of the DB query).  
                //If any of the parameters are required, make sure they were provided, or show an error message.  Note: SSRS cannot render the report if required parameters are missing
                CheckReportParameters(rvReportViewer.LocalReport);

                rvReportViewer.LocalReport.Refresh();
            }
        }

        /// <summary>
        /// Generate a PDF (as a download)
        /// adapted from http://www.codeproject.com/Articles/492739/Exporting-to-Word-PDF-using-Microsoft-Report-RDLC
        /// This fcn also gives much more useful errors, if something is wrong. 
        /// I often use it for troubleshooting.
        /// </summary>
        private void DownloadReportFile()
        {
            //this SSRS object allows us to run the report in-memory without a ReportViewer control
            LocalReport report = new LocalReport();

            FileInfo reportFullPath = this.ReportFile;
            //check to make sure the file ACTUALLY exists, before we start working on it
            if (reportFullPath != null)
            {
                //report name, minus the file extension (so the PDF will have a similar file name)
                string reportName = reportFullPath.Name.Substring(0, reportFullPath.Name.Length - reportFullPath.Extension.Length - 1);

                //map the reporting engine to the .rdl/.rdlc file
                LoadReportDefinitionFile(report, reportFullPath);

                //  1. Clear Report Data
                report.DataSources.Clear();

                //  2. Load new data
                // Look-up the DB query in the "DataSets" element of the report file (.rdl/.rdlc which contains XML)
                RDL.Report reportDef = this.ReportDefinition;

                foreach (RDL.DataSet ds in reportDef.DataSets)
                {
                    //copy the parameters from the QueryString into the ReportParameters definitions (objects)
                    ds.AssignParameters(this.ReportParameters);

                    //run the query to get real data for the report
                    System.Data.DataTable tbl = ds.GetDataTable(DBConnectionString);

                    //attach the data/table to the Report's dataset(s), by name
                    ReportDataSource rds = new ReportDataSource();
                    rds.Name = ds.Name; //This refers to the dataset name in the RDLC file
                    rds.Value = tbl;// { TableName="TableList", Columns = { new System.Data.DataColumn("Name"), new System.Data.DataColumn("object_id"), new System.Data.DataColumn("type") } };
                    report.DataSources.Add(rds);
                }

                //Load any other report parameters (which are not part of the DB query).  
                //If any of the parameters are required, make sure they were provided, or show an error message.  Note: SSRS cannot render the report if required parameters are missing
                CheckReportParameters(report);

                //Run the report
                Byte[] mybytes = report.Render("PDF");

                //output the PDF via the binary response stream
                Response.Clear();
                Response.AddHeader("content–disposition", "attachment; filename=" + reportName + ".pdf");
                Response.ContentType = "application/pdf";
                Response.BinaryWrite(mybytes);
            }
            else
            {
                //punt
                Response.Clear();
                Response.ContentType = "text/plain";
                Response.Write("Error: cannot find the report file [" + Request.QueryString["Path"] + "] in the configure report path.");
            }

        }
        #endregion

        #region  Helpers for loading files, parameters, settings
        /// <summary>
        /// Load the .rdl/.rdlc file into the reporting engine.  Also, fix the path for any embedded graphics.
        /// </summary>
        /// <param name="report">Instance of the ReportViewer control or equiv object</param>
        /// <param name="reportFullPath">(file) path to the Reports folder (on the HDD)</param>
        private void LoadReportDefinitionFile(LocalReport report, FileInfo reportFullPath)
        {
            string xml = File.ReadAllText(reportFullPath.FullName);
            if (xml.Contains("<Image"))
            {
                string rawUrlPath = Request.Url.OriginalString.ToLower();
                string newImagePath = rawUrlPath.Substring(0, rawUrlPath.IndexOf("/view")) + "/Reports/";
                xml = xml.Replace("<Value>/", "<Value>" + newImagePath);

                //if the report contains any images or report parts, then the report viewer
                //needs to read the string from a stream.  Create a string reader to convert 
                //the xml (string) into a stream
                using (StringReader sr = new StringReader(xml))
                {
                    //this actually changes the xml into the object
                    report.LoadReportDefinition(sr);
                    sr.Close();
                }
                report.EnableExternalImages = true;
            }
            else
            {
                report.ReportPath = reportFullPath.FullName;
            }
        }



        /// <summary>
        /// Note: SSRS cannot render the report if required parameters are missing.
        /// This will load any report parameters.
        /// If any of the parameters were required, but they were not provided, show an error message.
        /// </summary>
        /// <param name="report">Instance of the ReportViewer control or equiv object</param>
        private void CheckReportParameters(LocalReport report)
        {
            //copy-in any report parameters which were not part of the DB query
            ReportParameterInfoCollection rdlParams = report.GetParameters();
            foreach (ReportParameterInfo rdlParam in rdlParams)
            {
                if (this.ReportParameters.ContainsKey(rdlParam.Name))
                {
                    string val = this.ReportParameters[rdlParam.Name].ToString();
                    ReportParameter newParam = new ReportParameter(rdlParam.Name, val);
                    report.SetParameters(newParam);
                }
                else if (!rdlParam.AllowBlank)
                {
                    lblError.Text += "Report Parameter \"" + rdlParam.Name + "\" is required, but was not provided.<br/>";
                }
            }
        }

        //Further reading:
        //sub-reports: http://www.codeproject.com/Articles/473844/Using-Custom-Data-Source-to-create-RDLC-Reports

        #endregion
        #endregion

    }
}
