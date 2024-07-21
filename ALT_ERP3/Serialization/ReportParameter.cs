using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RDL
{
    /// <summary>
    /// child of Report
    /// </summary>
    [Serializable()]
    public class ReportParameter
    {
        #region Serialization Interface
        [System.Xml.Serialization.XmlAttribute]
        public string Name;
        public string DataType;
        #endregion
    }
}