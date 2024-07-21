using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ALT_ERP3
{
    public class TreeVM
    {
    }
    public class FlatObject
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string data { get; set; }
        public bool isSelected { get; set; }
        //public FlatObject(string name, int id, int parentId)
        //{
        //    data = name;
        //    Id = id;
        //    ParentId = parentId;
        //}
    }

    public class RecursiveObject
    {
        public string data { get; set; }
        public int id { get; set; }
        public FlatTreeAttribute attr { get; set; }
        public List<RecursiveObject> children { get; set; }
    }

    public class FlatTreeAttribute
    {
        public string id;
        public bool selected;
    }
}