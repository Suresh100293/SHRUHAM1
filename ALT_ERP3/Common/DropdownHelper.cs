using Common.Enums;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Common
{

    public static class DropdownHelper
    {

        public static IList<SelectListItem> YesNoList(int selectedId = 0, string defaultString = "-- Select --")
        {
            return new List<SelectListItem>
            {
                new SelectListItem{ Text = defaultString, Value = "0",Selected = (0==selectedId) },
                new SelectListItem{ Text = "Yes", Value = "1", Selected = (1 == selectedId) },
                new SelectListItem{ Text = "No", Value = "2", Selected = (2 == selectedId) }
            };
        }

        public static List<SelectListItem> EmptyList(int selectedId = 0, string defaultString = "-- Select --")
        {
            List<SelectListItem> list = new List<SelectListItem>();

            list.Add(new SelectListItem { Text = defaultString, Value = "0", Selected = (0 == selectedId) });

            return list;
        }

        public static IEnumerable<SelectListItem> SourceDocument(int selectedId = 0, string defaultString = "-- Select --")
        {
            List<SelectListItem> list = new List<SelectListItem>();

            //list.Add(new SelectListItem { Text = defaultString, Value = "0", Selected = (0 == selectedId) });

            Array values = Enum.GetValues(typeof(SourceDocument));

            foreach (SourceDocument val in values)
            {
                list.Add(new SelectListItem { Text = val.ToString(), Value = val.ToString(), Selected = ((int)val == selectedId) });
            }

            return list;
        }

        //public static IEnumerable<SelectListItem> GetTransporter(string selectedId = "0", string defaultString = "-- Select --")
        //{
        //    //nEntities context = new nEntities();
        //    //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();

        //    List<SelectListItem> list = new List<SelectListItem>();

        //    List<Master> list1 = new List<Master>();

        //    var result = ctxTFAT.Master.Where(x => x.BaseGr == "S" && x.Category == "100002").Select(c => new { c.Code, c.Name }).Distinct().ToList();
        //    foreach (var item in result)
        //    {
        //        list1.Add(new Master
        //        {
        //            Code = item.Code,
        //            Name = item.Name

        //        });
        //    }
        //    foreach (var val in list1)
        //    {
        //        list.Add(new SelectListItem { Text = val.Name.ToString(), Value = val.Code.ToString()});
        //    }

        //    return list;
        //}

        public static IEnumerable<SelectListItem> GetMfgType(int selectedId = 0, string defaultString = "-- Select --")
        {
            List<SelectListItem> list = new List<SelectListItem>();

            //list.Add(new SelectListItem { Text = defaultString, Value = "0", Selected = (0 == selectedId) });

            Array values = Enum.GetValues(typeof(MfgType));

            foreach (MfgType val in values)
            {
                list.Add(new SelectListItem { Text = val.ToString(), Value = val.ToString(), Selected = ((int)val == selectedId) });
            }

            return list;
        }

        public static IEnumerable<SelectListItem> WorkPriority(int selectedId = 0, string defaultString = "-- Select --")
        {
            List<SelectListItem> list = new List<SelectListItem>();

            //list.Add(new SelectListItem { Text = defaultString, Value = "0", Selected = (0 == selectedId) });

            Array values = Enum.GetValues(typeof(WorkPriority));

            foreach (WorkPriority val in values)
            {
                list.Add(new SelectListItem { Text = val.ToString(), Value = val.ToString(), Selected = ((int)val == selectedId) });
            }

            return list;
        }

        public static IEnumerable<SelectListItem> PaymentMode(int selectedId = 0, string defaultString = "-- Select --")
        {
            List<SelectListItem> list = new List<SelectListItem>();

            Array values = Enum.GetValues(typeof(PaymentMode));

            foreach (int val in values)
            {
                list.Add(new SelectListItem { Text = Enum.GetName(typeof(PaymentMode), val), Value = val.ToString(), Selected = ((int)val == selectedId) });
            }
            return list;
        }
    }
}