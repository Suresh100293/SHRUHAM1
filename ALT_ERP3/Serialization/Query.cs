using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RDL
{
    /// <summary>
    /// child of DataSet
    /// </summary>
    [Serializable()]
    public class Query
    {
        #region Serialization Interface
        public string DataSourceName;
        public List<QueryParameter> QueryParameters = new List<QueryParameter>();
        public string CommandText;
        #endregion
    }
}