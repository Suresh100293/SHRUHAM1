using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ALT_ERP3.Models
{
    public class GraphDesignVM
    {
        //Pie Chart
        public GraphDesignVM()
        {
            labels = new List<string>();
            datasets = new List<GraphChildElementVM>();
        }

        //Explicitly setting the name to be used while serializing to JSON.
        //[DataMember(Name = "label")]
        public List<string> labels { get; set; }

        public List<GraphChildElementVM> datasets { get; set; }


    }
    public class GraphChildElementVM
    {
        public GraphChildElementVM()
        {
            backgroundColor = new List<string>();
            data = new List<int>();
        }
        public List<string> backgroundColor { get; set; }
        public List<int> data { get; set; }
    }
    
}