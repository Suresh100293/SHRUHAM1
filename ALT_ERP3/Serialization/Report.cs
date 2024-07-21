using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RDL
{
    [Serializable(), System.Xml.Serialization.XmlRoot("Report")]
    public class Report : SerializableBase
    {
        #region Serialization Interface
        public List<DataSet> DataSets = new List<DataSet>();
        public List<ReportParameter> ReportParameters = new List<ReportParameter>();
        #endregion

        #region Factory Methods
        //override the constructor, so you don't have to cast it after deserializing it
        public static Report Deserialize(string xml, Type type)
        {
            Report re;
            re = (Report)SerializableBase.Deserialize(xml, type);

            //copy the type-names from the ReportParameters to the QueryParameters
            re.ResolveParameterTypes();

            return re;
        }

        ///<summary>
        ///Gets a Report object based on the XML within the specified file
        ///</summary>
        public static Report GetReportFromFile(string reportFileName)
        {
            RDL.Report re = new RDL.Report();
            string xml;

            try
            {
                xml = System.IO.File.ReadAllText(reportFileName);
                re = RDL.Report.Deserialize(xml, typeof(RDL.Report));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
                //ErrorHandling.ErrorLogger.LogException(ex, "ReportFileName=" & ReportFileName)
                throw ex;
            }

            return re;
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// map the report parameters to the query parameters
        /// </summary>
        private void ResolveParameterTypes()
        {
            foreach (ReportParameter rParam in this.ReportParameters)
            {
                foreach (DataSet ds in this.DataSets)
                    foreach (QueryParameter qParam in ds.Query.QueryParameters)
                    {
                        if (qParam.Value == "=Parameters!" + rParam.Name + ".Value")
                        {
                            qParam.DataType = rParam.DataType;
                        }
                    }
            }
        }
        #endregion
    }
}