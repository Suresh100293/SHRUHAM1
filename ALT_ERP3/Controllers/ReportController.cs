using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;

namespace ALT_ERP3.Controllers
{
    public class ReportController : BaseController
    {
        public static string mPara = "";
        public static string mmaintype = "";
        public static string msubcodeof = "";
        public static string moptioncode = "";

        public static string pitemgroups = "";
        public static string paccgroups = "";
        public static string pstores = "";
        public static string ptypes = "";
        public static string psalesman = "";

        public static string pitems = "";
        public static string paccounts = "";
        public static string pitemcat = "";
        public static string pprocess = "";
        public static string pareas = "";
        public static string pbroker = "";
        public static string pcity = "";
        public static string pstate = "";
        public static string pcategory = "";
        public static string paccheads = "";
        public static string pcostcent = "";
        public static string pprofitcent = "";
        public static string punits = "";
        public static string pmaintype = "";
        public static string psubtype = "";
        public static string pcountry = "";
        public static string pwarehouse = "";
        public static string pbin = "";
        public static string ptaxmas = "";
        public static string pgsttype = "";
        public static string phsn = "";
        public static string pgrade = "";
        public static string puser = "";
        public static string pempcat = "";
        public static string pdept = "";
        public static string ppost = "";
        public static string pemployee = "";
        public static string pmccat = "";
        public static string pmctype = "";
        public static string pmachines = "";
        public static string pparatype = "";
        public static string ptestcat = "";
        public static string ptests = "";

        public string GetGroupCode(string mName)
        {
            string mcode = ctxTFAT.ItemGroups.Where(x => x.Name == mName.Trim()).Select(x => x.Code).FirstOrDefault() ?? "";
            return mcode;
        }

        public string rep_GetGrpCode(string mName)
        {
            string bca = "";
            var abc = ctxTFAT.MasterGroups.Where(x => x.Name == mName).Select(x => new { x.Code }).FirstOrDefault();
            if (abc != null)
            {
                bca = abc.Code;
            }
            else
            {
                bca = "";
            }
            return bca;
        }

        public string rep_GetTypeCode(string mName)
        {
            string bca = "";
            var abc = ctxTFAT.DocTypes.Where(x => x.Name == mName && x.AppBranch.Contains(mbranchcode)).Select(x => new { x.Code }).FirstOrDefault();
            if (abc != null)
            {
                bca = abc.Code;
            }
            else
            {
                bca = "";
            }
            return bca;
        }

        public int rep_GetSalesmanCode(string mName)
        {
            int bca = 0;
            var abc = ctxTFAT.SalesMan.Where(x => x.Name == mName).Select(x => new
            {
                x.Code
            }).FirstOrDefault();
            if (abc != null)
            {
                bca = abc.Code;
            }
            else
            {
                bca = 0;
            }
            return bca;
        }

        public int GetStoreCode(string mName)
        {
            int bca = 0;
            var abc = ctxTFAT.Stores.Where(x => x.Name == mName).Select(x => new { x.Code }).FirstOrDefault();
            if (abc != null)
            {
                bca = abc.Code;
            }
            else
            {
                bca = 0;
            }
            return bca;
        }

        #region getlist
        [HttpPost]
        public ActionResult GetParameterValueReport(string mformat)
        {
            List<string> inputlist = new List<string>();
            List<string> inputlist2 = new List<string>();
            string mparastring = "";
            string maccgroups = "";
            string mitemgroups = "";
            string msubtypes = "";
            string minputpara = "";
            string mtabs = "";
            var result = ctxTFAT.ReportHeader.Select(x => new { x.ParaString, x.AccGroups, x.ItemGroups, x.SubTypes, x.Code, x.InputPara, x.Tabs }).Where(z => z.Code == mformat).FirstOrDefault();
            if (result != null)
            {
                mparastring = result.ParaString.Trim();
                maccgroups = result.AccGroups.Trim();
                mitemgroups = result.ItemGroups.Trim();
                msubtypes = result.SubTypes.Trim();
                minputpara = result.InputPara.Trim();
                mtabs = result.Tabs.Trim();
                inputlist = (result.InputPara == "" || result.InputPara == null) ? inputlist : result.InputPara.Trim().Split('~').ToList();
            }
            GridOption Model = new GridOption();
            foreach (var ai in inputlist)
            {
                string a1 = "";
                string a2 = "";
                if (ai != null && ai.Trim() != "")
                {
                    var a = ai.Split('^');
                    a1 = a[0];
                    if (a[1].Trim() != "")
                    {
                        a2 = GetQueryText(a[1]);
                    }
                    inputlist2.Add(a1 + "^" + a2);
                }
            }

            Model.AddOnParaList = inputlist2;
            string html = ViewHelper.RenderPartialView(this, "ReportAddOnGrid", Model);
            return Json(new
            {
                tabs = mtabs,
                parastring = mparastring,
                itemgroup = mitemgroups,
                accgroup = maccgroups,
                sub = msubtypes,
                inputpara = minputpara,
                inputlist = inputlist,
                Status = "Success",
                Html = html,
                Message = ""
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetURLReports(string mdocument, string mDate)
        {
            string murl = "";
            murl = ctxTFAT.TfatMenu.Where(z => z.OptionCode == moptioncode).Select(x => x.ZoomURL).FirstOrDefault() ?? "";
            //"/Reports/StockLedger?ViewDataId=StockLedgerScr&Module=Inventory&optiontype=R&optioncode=StockLedger&Header=Stock%20Ledger&Controller2=StockLedger&Document=~Document&FromDate=~FromDate&ToDate=~ToDate;
            try
            {
                if (murl != "")
                {
                    murl = murl.Replace("~Document", mdocument);
                    murl = murl.Replace("%Document", mdocument);
                    if (mDate != null)
                    {
                        var date = mDate.Replace("-", "/").Split(':');
                        murl = murl.Replace("~FromDate", date[0]);
                        murl = murl.Replace("~ToDate", date[1]);
                        murl = murl.Replace("%FromDate", date[0]);
                        murl = murl.Replace("%ToDate", date[1]);
                    }
                }
                else
                {
                    string mtype = mdocument.Substring(6, 5);
                    bool mrights = ctxTFAT.UserRightsTrx.Where(z => z.Code == muserid && z.Type == mtype).Select(x => x.xEdit).FirstOrDefault();
                    if (mrights == true || muserid.ToLower() == "super")
                    {
                        string msubtype = GetSubType(mtype);
                        var mstr = ctxTFAT.TfatMenu.Select(x => new { x.OptionCode, x.TableName, x.ModuleName, x.Menu, x.Controller, x.FormatCode, x.SubType, x.ParentMenu }).Where(z => z.SubType == msubtype && z.ParentMenu == "Transactions" && (z.Controller == "Purchase" || z.Controller == "CashBank")).FirstOrDefault();
                        murl = "/Transactions/" + mstr.Controller + "/Index?Document=" + mdocument + "&Mode=Edit&ChangeLog=Edit&ViewDataId=" + HttpUtility.UrlEncode(mstr.FormatCode.Trim()) + "&Header=" + HttpUtility.UrlEncode(mstr.Menu.Trim()) + "&Module=" + HttpUtility.UrlEncode(mstr.ModuleName.Trim()) + "&TableName=" + mstr.TableName.Trim() + "&OptionType=T&OptionCode=" + HttpUtility.UrlEncode(mstr.OptionCode.Trim()) + "&Controller2=" + mstr.Controller.Trim();
                    }
                }
            }
            catch
            {
            }
            return Json(new { url = murl, Message = "" }, JsonRequestBehavior.AllowGet);
        }

        #endregion getlist

        public ActionResult GetItemLists(List<string> ItemGroups)
        {
            //ItemGroups.Replace("&nbsp;", "");
            //var abc = ItemGroups.Split(',');
            List<string> CodeList = new List<string>();
            if (ItemGroups != null)
            {
                foreach (var i in ItemGroups)
                {
                    if (i.Trim() != "")
                    {
                        i.Replace("&nbsp;", "");
                        CodeList.Add(GetGroupCode(i.Trim()));
                    }
                }
            }
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            var jsonResult = Json("");
            if (CodeList.Count == 0)
            {
                var result = ctxTFAT.ItemMaster.Select(m => new
                {
                    Value = m.Code,
                    Text = m.Name
                }).OrderBy(n => n.Text).ToList();
                jsonResult = Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.ItemMaster.Where(x => CodeList.Contains(x.Code)).Select(m => new
                {
                    Value = m.Code,
                    Text = m.Name
                }).OrderBy(n => n.Text).ToList();
                jsonResult = Json(result, JsonRequestBehavior.AllowGet);
            }
            jsonResult.MaxJsonLength = int.MaxValue; // to enhance size of image attachment
            return jsonResult;
        }

        [HttpPost]
        public string GetItemGroupsTree(string ItemGroups)
        {
            var mTreeList = ctxTFAT.ItemGroups.Select(x => new
            {
                x.Name,
                x.GrpKey,
                x.AcType,
                x.RECORDKEY
            }).ToList();

            List<FlatObject> flatObjects2 = new List<FlatObject>();
            for (int n = 0; n < mTreeList.Count; n++)
            {
                FlatObject abc = new FlatObject();
                abc.data = mTreeList[n].Name;
                abc.Id = mTreeList[n].RECORDKEY;
                if (mTreeList[n].RECORDKEY == mTreeList[n].GrpKey)
                {
                    abc.ParentId = 0;
                }
                else
                {
                    abc.ParentId =(int) mTreeList[n].GrpKey;
                }
                abc.isSelected = ItemGroups.Contains(mTreeList[n].AcType + "^") ? true : false;
                flatObjects2.Add(abc);
            }
            var recursiveObjects = FillRecursive(flatObjects2, 0);
            string myjsonmodel = new JavaScriptSerializer().Serialize(recursiveObjects);
            return myjsonmodel;
        }

        [HttpPost]
        public string GetAccGroupsTree(string maccgroup)
        {
            var mTreeList = ctxTFAT.MasterGroups.Select(x => new
            {
                x.Name,
                x.GrpKey,
                x.BaseGr,
                x.RECORDKEY
            }).ToList();

            List<FlatObject> flatObjects2 = new List<FlatObject>();
            for (int n = 0; n < mTreeList.Count; n++)
            {
                FlatObject abc = new FlatObject();
                abc.data = mTreeList[n].Name;
                abc.Id = mTreeList[n].RECORDKEY;
                if (mTreeList[n].RECORDKEY == mTreeList[n].GrpKey)
                {
                    abc.ParentId = 0;
                }
                else
                {
                    abc.ParentId =(int) mTreeList[n].GrpKey;
                }
                abc.isSelected = maccgroup.Contains(mTreeList[n].BaseGr + "^") ? true : false;
                flatObjects2.Add(abc);
            }
            var recursiveObjects = FillRecursive(flatObjects2, 0);
            //string myjsonmodel = new JavaScriptSerializer().Serialize(recursiveObjects);
            return new JavaScriptSerializer().Serialize(recursiveObjects);
        }

        [HttpPost]
        public string GetStoresTree()
        {
            var mTreeList = ctxTFAT.Stores.Select(x => new
            {
                x.Name,
                x.GrpKey,
                x.RECORDKEY
            }).ToList();

            List<FlatObject> flatObjects2 = new List<FlatObject>();
            for (int n = 0; n < mTreeList.Count; n++)
            {
                FlatObject abc = new FlatObject();
                abc.data = mTreeList[n].Name;
                abc.Id = mTreeList[n].RECORDKEY;
                if (mTreeList[n].RECORDKEY == mTreeList[n].GrpKey)
                {
                    abc.ParentId = 0;
                }
                else
                {
                    abc.ParentId = mTreeList[n].GrpKey;
                }
                flatObjects2.Add(abc);
            }
            var recursiveObjects = FillRecursive(flatObjects2, 0);
            string myjsonmodel = new JavaScriptSerializer().Serialize(recursiveObjects);
            return myjsonmodel;
        }

        public ActionResult GetAccountLists(List<string> mGroups)
        {
            List<string> CodeList = new List<string>();
            if (mGroups != null)
            {
                //mGroups.Replace("&nbsp;", "");
                //var abc = mGroups;
                foreach (var i in mGroups)
                {
                    if (i.Trim() != "")
                    {
                        i.Replace("&nbsp;", "");
                        CodeList.Add(rep_GetGrpCode(i.Trim()));
                    }
                }
            }
            if (CodeList.Count == 0)
            {
                var result = ctxTFAT.Master.Where(x => x.Hide == false).Select(m => new
                {
                    Value = m.Code,
                    Text = m.Name + " [" + ctxTFAT.Address.Where(z => z.Code == m.Code && z.Sno == 0).Select(x => x.City).FirstOrDefault() + "]"
                }).OrderBy(n => n.Text).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = ctxTFAT.Master.Where(x => CodeList.Contains(x.Grp) && x.Hide == false).Select(m => new
                {
                    Value = m.Code,
                    Text = m.Name + " [" + ctxTFAT.Address.Where(z => z.Code == m.Code && z.Sno == 0).Select(x => x.City).FirstOrDefault() + "]"
                }).OrderBy(n => n.Text).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetSalesmanTree()
        {
            var mTreeList = ctxTFAT.SalesMan.Select(x => new
            {
                x.Name,
                x.GrpKey,
                x.RECORDKEY
            }).ToList();

            List<FlatObject> flatObjects2 = new List<FlatObject>();
            for (int n = 0; n < mTreeList.Count; n++)
            {
                FlatObject abc = new FlatObject();
                abc.data = mTreeList[n].Name;
                abc.Id = mTreeList[n].RECORDKEY;
                if (mTreeList[n].RECORDKEY == mTreeList[n].GrpKey)
                {
                    abc.ParentId = 0;
                }
                else
                {
                    abc.ParentId = mTreeList[n].GrpKey;
                }
                flatObjects2.Add(abc);
            }
            var recursiveObjects = FillRecursive(flatObjects2, 0);
            //string myjsonmodel = new JavaScriptSerializer().Serialize(recursiveObjects);
            return Json(recursiveObjects, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetAreaLists()
        {
            var result = ctxTFAT.AreaMaster.Select(m => new
            {
                Text = m.Name,
                Value = m.Code
            }).OrderBy(n => n.Text).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetItemCategoryLists()
        {
            var result = ctxTFAT.ItemCategory.Select(m => new
            {
                Text = m.Name,
                Value = m.Code
            }).OrderBy(n => n.Text).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetBrokerLists()
        {
            var result = ctxTFAT.Broker.Select(m => new
            {
                Text = m.Name,
                Value = m.Code
            }).OrderBy(n => n.Text).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetCityLists()
        {
            var result = ctxTFAT.TfatCity.Select(m => new
            {
                Text = m.Name,
                Value = m.Code.ToString()
            }).OrderBy(n => n.Text).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetStateLists()
        {
            var result = ctxTFAT.TfatState.Select(m => new
            {
                Text = m.Name,
                Value = m.Code.ToString()
            }).OrderBy(n => n.Text).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetAccCategoryLists()
        {
            var result = ctxTFAT.PartyCategory.Select(m => new
            {
                Text = m.Name,
                Value = m.Code
            }).OrderBy(n => n.Text).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetAccHeadLists()
        {
            var result = ctxTFAT.Master.Where(x => x.Hide == false).Select(m => new
            {
                Text = m.Name,
                Value = m.Code
            }).OrderBy(n => n.Text).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetCostCentLists()
        {
            var result = ctxTFAT.CostCentre.Select(m => new
            {
                Text = m.Name,
                Value = m.Code
            }).OrderBy(n => n.Text).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetUnitLists()
        {
            var result = ctxTFAT.UnitMaster.Select(m => new
            {
                Text = m.Name,
                Value = m.Code
            }).OrderBy(n => n.Text).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetMainTypeLists()
        {
            var result = ctxTFAT.MainTypes.Select(m => new
            {
                Text = m.Name,
                Value = m.Code
            }).OrderBy(n => n.Text).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetSubTypeLists()
        {
            var result = ctxTFAT.SubTypes.Where(z => z.MainType != z.Code).Select(m => new
            {
                Text = m.Name,
                Value = m.Code
            }).OrderBy(n => n.Text).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetWarehouseLists()
        {
            var result = ctxTFAT.Warehouse.Where(z => (z.Users + ",").Contains(muserid + ",")).Select(m => new
            {
                Text = m.Name,
                Value = m.Code
            }).OrderBy(n => n.Text).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetTaxLists()
        {
            var result = ctxTFAT.TaxMaster.Select(m => new
            {
                Text = m.Name,
                Value = m.Code
            }).OrderBy(n => n.Text).Distinct().ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetHSNLists()
        {
            var result = ctxTFAT.HSNMaster.Select(m => new
            {
                Text = m.Name,
                Value = m.Code
            }).OrderBy(n => n.Text).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetEmpGradeLists()
        {
            var result = ctxTFAT.Grade.Select(m => new
            {
                Text = m.Name,
                Value = m.Code
            }).OrderBy(n => n.Text).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetUserLists()
        {
            var result = ctxTFAT.TfatPass.Select(m => new
            {
                Text = m.Name,
                Value = m.Code
            }).OrderBy(n => n.Text).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetEmpCategoryLists()
        {
            var result = ctxTFAT.EmpCategory.Select(m => new
            {
                Text = m.Name,
                Value = m.Code
            }).OrderBy(n => n.Text).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetEmployeeLists()
        {
            var result = ctxTFAT.Employee.Select(m => new
            {
                Text = m.Name,
                Value = m.EmpID
            }).OrderBy(n => n.Text).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetDeptLists()
        {
            var result = ctxTFAT.Dept.Select(m => new
            {
                Text = m.Name,
                Value = m.Code
            }).OrderBy(n => n.Text).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetPostLists()
        {
            var result = ctxTFAT.Post.Select(m => new
            {
                Text = m.Name,
                Value = m.Code
            }).OrderBy(n => n.Text).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetCountryLists()
        {
            var result = ctxTFAT.TfatCountry.Select(m => new
            {
                Text = m.Name,
                Value = m.Code.ToString()
            }).OrderBy(n => n.Text).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public string GetTypesTree(string mmain, string msub)
        {
            var mainlist = ctxTFAT.SubTypes.Where(z => msub == "" ? true : msub.Contains(("'" + z.Code + "'"))).Select(x => x.MainType).Distinct().ToList();
            var maintypes = ctxTFAT.MainTypes.Where(z => mainlist.Contains(z.Code)).Select(x => new { x.Name, x.Code, x.RECORDKEY }).ToList();

            List<FlatObject> flatObjects2 = new List<FlatObject>();
            for (int n = 0; n < maintypes.Count; n++)
            {
                FlatObject abc = new FlatObject();
                abc.data = maintypes[n].Name;
                abc.Id = maintypes[n].RECORDKEY;
                abc.ParentId = 0;
                abc.isSelected = msub.Contains("'" + maintypes[n].Code + "'") ? true : false;
                flatObjects2.Add(abc);
            }

            DataTable subtypes = GetDataTable("Select Code,MainType,Name,RecordKey,mainrec=(Select top 1 MainTypes.Recordkey from MainTypes where MainTypes.Code=Subtypes.MainType) from SubTypes " + (msub == "" ? "" : " Where Charindex(SubType,'" + msub + "')<>0") + " Order by MainType,Code");
            //var subtypes = ctxTFAT.SubTypes.Where(z => msub == "" ? true : msub.Contains(("'" + z.Code + "'"))).Select(x => new { x.Name, x.Code, x.RECORDKEY, x.MainType }).ToList();
            //for (int n = 0; n < subtypes.Count; n++)
            //{
            //    FlatObject abc2 = new FlatObject();
            //    abc2.data = subtypes[n].Name;
            //    abc2.Id = subtypes[n].RECORDKEY + 100;
            //    abc2.ParentId = GetSubtypeParent(subtypes[n].MainType, flatObjects2);
            //    abc2.isSelected = msub.Contains("'" + subtypes[n].Code + "'") ? true : false;
            //    //abc2.isSelected = msub.Contains(subtypes[n].Code + "^") ? true : false;
            //    flatObjects2.Add(abc2);
            //}
            foreach (DataRow mrow in subtypes.Rows)
            {
                FlatObject abc2 = new FlatObject();
                abc2.data = mrow["Name"].ToString();
                abc2.Id = ((int)mrow["RECORDKEY"]) + 100;
                string main = mrow["MainType"].ToString();
                abc2.ParentId = (int)mrow["mainrec"];
                //ctxTFAT.MainTypes.Where(x => x.Code == main).Select(x => x.RECORDKEY).FirstOrDefault(); ;
                abc2.isSelected = msub.Contains(mrow["Code"].ToString() + "^") ? true : false;
                flatObjects2.Add(abc2);
            }

            if (muserid.ToLower() == "super")
            {
                //var types = ctxTFAT.DocTypes.Where(x => !x.Code.StartsWith("%") && x.AppBranch.Contains(mbranchcode) && x.SubType != x.Code && (msub == "" ? true : msub.Contains("'" + x.SubType + "'") && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(x.Code))))
                //.Select(x => new { x.Name, x.Code, x.RECORDKEY, x.SubType }).ToList();
                DataTable mdt = GetDataTable("Select Code,Name,RecordKey,SubType,subrec=(select top 1 subtypes.recordkey from subtypes where subtypes.code=doctypes.subtype) from DocTypes where Left(Code,1)<>'%' and CharIndex('" + mbranchcode + "',AppBranch)<>0 and SubType<>Code " + (msub == "" ? "" : " And Charindex(SubType,'" + msub + "')<>0"));
                for (int n = 0; n < mdt.Rows.Count; n++)
                {
                    FlatObject abc3 = new FlatObject();
                    abc3.data = mdt.Rows[n]["Name"].ToString(); //types[n].Name;
                    abc3.Id = 1000 + Convert.ToInt32(mdt.Rows[n]["RECORDKEY"]); //types[n].RECORDKEY + 1000;
                    //abc3.ParentId = GetTypeParent(types[n].SubType, flatObjects2);
                    abc3.ParentId = 100 + Convert.ToInt32(mdt.Rows[n]["subrec"]);
                    abc3.isSelected = msub.Contains("'" + mdt.Rows[n]["Code"].ToString() + "'") ? true : false;
                    flatObjects2.Add(abc3);
                }
            }
            else
            {
                //var types = (from x in ctxTFAT.DocTypes
                //             where x.Code != x.SubType && x.MainType != x.SubType && x.AppBranch.Contains(mbranchcode) && (string.IsNullOrEmpty(mDocString) ? true : !mDocString.Contains(x.Code))
                //             join ur in ctxTFAT.UserRightsTrx on x.Code equals ur.Type
                //             where ur.Code == muserid && ur.xCess == true
                //             select new { x.Name, x.Code, x.RECORDKEY, x.SubType }).ToList();

                //for (int n = 0; n < types.Count; n++)
                //{
                //    FlatObject abc3 = new FlatObject();
                //    abc3.data = types[n].Name;
                //    abc3.Id = types[n].RECORDKEY + 1000;
                //    abc3.ParentId = GetTypeParent(types[n].SubType, flatObjects2);
                //    abc3.isSelected = msub.Contains("'" + types[n].Code + "'") ? true : false;
                //    flatObjects2.Add(abc3);
                //}
                DataTable types = GetDataTable("Select d.Code,d.Name,d.RecordKey,subrec=(select top 1 subtypes.recordkey from subtypes where subtypes.code=d.subtype) from DocTypes d,UserRightsTrx x Where d.Code<>d.SubType and d.MainType<>d.Subtype and charindex('" + mbranchcode + "',AppBranch)<>0 and x.Type=d.Code and x.Code='" + muserid + "' And x.xcess<>0");
                foreach (DataRow mrow1 in types.Rows)
                {
                    FlatObject abc3 = new FlatObject();
                    abc3.data = mrow1["Name"].ToString();
                    abc3.Id = ((int)mrow1["RECORDKEY"]) + 1000;
                    abc3.ParentId = 100 + Convert.ToInt32(mrow1["subrec"]);
                    //abc3.ParentId = rep_GetTypeParent(mrow1["Name"].ToString(), flatObjects2);
                    abc3.isSelected = msub.Contains(mrow1["Code"].ToString() + "^") ? true : false;
                    flatObjects2.Add(abc3);
                }
            }

            var recursiveObjects = FillRecursive(flatObjects2, 0);
            string myjsonmodel = new JavaScriptSerializer().Serialize(recursiveObjects);
            return myjsonmodel;
        }

        public int GetSubtypeParent(string maintyp, List<FlatObject> flat)
        {
            //int i = 0;
            //string maintyp = ctxTFAT.SubTypes.Where(x => x.Name == subtype).Select(x => x.MainType).FirstOrDefault();
            //string mainname = ctxTFAT.MainTypes.Where(x => x.Code == maintyp).Select(x => x.Name).FirstOrDefault();
            //if (mainname != null)
            //{
            //    i = flat.Find(x => x.data == mainname).Id;
            //    //i = flat.Where(x => x.data == mainname).Select(x => x.Id).FirstOrDefault();
            //}
            //i = ctxTFAT.MainTypes.Where(x => x.Code == maintyp).Select(x => x.RECORDKEY).FirstOrDefault();
            //if (z != i)
            //{
            //    z = i;
            //}
            return ctxTFAT.MainTypes.Where(x => x.Code == maintyp).Select(x => x.RECORDKEY).FirstOrDefault();
        }

        public int GetTypeParent(string subtyp, List<FlatObject> flat)
        {
            //int i = 0;
            //string subtyp = ctxTFAT.DocTypes.Where(x => x.Name == type && !x.Code.StartsWith("%") && x.AppBranch.Contains(mbranchcode)).Select(x => x.SubType).FirstOrDefault();
            //string subname = ctxTFAT.SubTypes.Where(x => x.Code == subtyp).Select(x => x.Name).FirstOrDefault();
            //if (subname != null)
            //{
            //    i = flat.Find(x => x.data == subname).Id;
            //    //i = flat.Where(x => x.data == subname).Select(x => x.Id).FirstOrDefault();
            //}
            //i = 100 + ctxTFAT.SubTypes.Where(x => x.Code == subtyp).Select(x => x.RECORDKEY).FirstOrDefault();
            //if (z != i)
            //{
            //    z = i;
            //}
            return 100 + ctxTFAT.SubTypes.Where(x => x.Code == subtyp).Select(x => x.RECORDKEY).FirstOrDefault(); ;
        }

        public string rep_GetGroupby(string mFormat)
        {
            string mGrpby = "";
            var result = ctxTFAT.ReportHeader.Select(m => new { m.SubTotalOn, m.Code }).Where(z => z.Code == mFormat).FirstOrDefault();
            if (result != null)
            {
                if (result.SubTotalOn != null)
                {
                    mGrpby = result.SubTotalOn.Trim();
                }
            }
            return mGrpby;
        }

        public List<string> GetReportInputPara(string minputpara)
        {
            List<string> inputlist = new List<string>();
            List<string> inputlist2 = new List<string>();
            inputlist = (minputpara == "" || minputpara == null) ? inputlist : minputpara.Trim().Split('~').ToList();
            foreach (var ai in inputlist)
            {
                if (ai != "" && ai != null)
                {
                    var a = ai.Split('^');
                    string a1 = a[0];
                    string a2 = GetQueryText(a[1]);
                    inputlist2.Add(a1 + "^" + a2);
                }
            }
            return inputlist2;
        }

        public string rep_SaveParameters(string mParaString)
        {
            //itemgrouptreearray, accgrouptreearray, storestreearray, typestreearray, salesmantreearray, 
            string[] abc;
            var marr = mParaString.Split('~');
            List<string> mCodeList = new List<string>();
            string mitemgroups = "";
            if (marr.Length > 0)
            {
                mitemgroups = marr[0].Trim().Replace("&nbsp;", "");
                abc = mitemgroups.Split(',');
                foreach (var i in abc)
                {
                    if (i.Trim() != "") mCodeList.Add(GetGroupCode(i.Trim()));
                }
                mitemgroups = "";
                foreach (var i in mCodeList)
                {
                    mitemgroups += ("'" + i + "',");
                }
                mitemgroups = CutRightString(mitemgroups, 1, ",");
            }

            string maccgroups = "";
            if (marr.Length > 1)
            {
                maccgroups = marr[1].Trim().Replace("&nbsp;", "");
                abc = maccgroups.Split(',');
                mCodeList = new List<string>();
                foreach (var i in abc)
                {
                    if (i.Trim() != "") mCodeList.Add(rep_GetGrpCode(i.Trim()));
                }
                maccgroups = "";
                foreach (var i in mCodeList)
                {
                    maccgroups += ("'" + i + "',");
                }
                maccgroups = CutRightString(maccgroups, 1, ",");
            }

            string mstores = "";
            if (marr.Length > 2)
            {
                mstores = marr[2].Trim().Replace("&nbsp;", "");
                abc = mstores.Split(',');
                mCodeList = new List<string>();
                foreach (var i in abc)
                {
                    if (i.Trim() != "") mCodeList.Add(GetStoreCode(i.Trim()).ToString());
                }
                mstores = "";
                foreach (var i in mCodeList)
                {
                    mstores += (i + ",");
                }
                mstores = CutRightString(mstores, 1, ",");
            }

            string mtypes = "";
            if (marr.Length > 3)
            {
                mtypes = marr[3].Trim().Replace("&nbsp;", "");
                abc = mtypes.Split(',');
                mCodeList = new List<string>();
                foreach (var i in abc)
                {
                    if (i.Trim() != "") mCodeList.Add(rep_GetTypeCode(i.Trim()));
                }
                mtypes = "";
                foreach (var i in mCodeList)
                {
                    mtypes += ("'" + i + "',");
                }
                mtypes = CutRightString(mtypes, 1, ",");
            }

            string msalesman = "";
            if (marr.Length > 4)
            {
                msalesman = marr[4].Trim().Replace("&nbsp;", "");
                abc = msalesman.Split(',');
                mCodeList = new List<string>();
                foreach (var i in abc)
                {
                    if (i.Trim() != "") mCodeList.Add(rep_GetSalesmanCode(i.Trim()).ToString());
                }
                msalesman = "";
                foreach (var i in mCodeList)
                {
                    msalesman += (i + ",");
                }
                msalesman = CutRightString(msalesman, 1, ",");
            }
            //itemarray, accarray, itemcatarray, processarray, areaarray, brokerarray, cityarray, statearray, category,
            //accheads, costcent, profitcent, broker, units, maintype, subtype, country, warehouse, bin, taxmas gsttype, hsn, 
            //grade, emptype, empcat, dept, post, employee, mccat, mctype, machines, paratype, testcat, tests
            //FPSerial;

            pitemgroups = GetProperString(mitemgroups);
            mitemgroups = (pitemgroups != "" ? "itemgroup^" + pitemgroups + "~" : "");

            paccgroups = GetProperString(maccgroups);
            maccgroups = (paccgroups != "" ? "accountgroup^" + paccgroups + "~" : "");

            pstores = GetProperString(mstores);
            mstores = (pstores != "" ? "store^" + pstores + "~" : "");

            ptypes = GetProperString(mtypes);
            mtypes = (ptypes != "" ? "type^" + ptypes + "~" : "");

            psalesman = GetProperString(msalesman);
            msalesman = (psalesman != "" ? "salesman^" + psalesman + "~" : "");

            string mitems = "";
            if (marr.Length > 5)
            {
                pitems = GetProperString(marr[5].Trim());
                mitems = (pitems != "" ? "item^" + pitems + "~" : "");
            }

            string maccounts = "";
            if (marr.Length > 6)
            {
                paccounts = GetProperString(marr[6].Trim());
                maccounts = (paccounts != "" ? "account^" + paccounts + "~" : "");
            }

            string mitemcat = "";
            if (marr.Length > 7)
            {
                pitemcat = GetProperString(marr[7].Trim());
                mitemcat = (pitemcat != "" ? "itemcategory^" + pitemcat + "~" : "");
            }
            string mprocess = "";
            if (marr.Length > 8)
            {
                pprocess = GetProperString(marr[8].Trim());
                mprocess = (pprocess != "" ? "process^" + pprocess + "~" : "");
            }
            string mareas = "";
            if (marr.Length > 9)
            {
                pareas = GetProperString(marr[9].Trim());
                mareas = (pareas != "" ? "area^" + pareas + "~" : "");
            }
            string mbroker = "";
            if (marr.Length > 10)
            {
                pbroker = GetProperString(marr[10].Trim());
                mbroker = (pbroker != "" ? "broker^" + pbroker + "~" : "");
            }
            string mcity = "";
            if (marr.Length > 11)
            {
                pcity = GetProperString(marr[11].Trim());
                mcity = (pcity != "" ? "city^" + pcity + "~" : "");
            }
            string mstate = "";
            if (marr.Length > 12)
            {
                pstate = GetProperString(marr[12].Trim());
                mstate = (pstate != "" ? "state^" + pstate + "~" : "");
            }
            string mcategory = "";
            if (marr.Length > 13)
            {
                pcategory = GetProperString(marr[13].Trim());
                mcategory = (pcategory != "" ? "category^" + pcategory + "~" : "");
            }
            string maccheads = "";
            if (marr.Length > 14)
            {
                paccheads = GetProperString(marr[14].Trim());
                maccheads = (paccheads != "" ? "acchead^" + paccheads + "~" : "");
            }
            string mcostcent = "";
            if (marr.Length > 15)
            {
                pcostcent = GetProperString(marr[15].Trim());
                mcostcent = (pcostcent != "" ? "costcent^" + pcostcent + "~" : "");
            }
            string mprofitcent = "";
            if (marr.Length > 16)
            {
                pprofitcent = GetProperString(marr[16].Trim());
                mprofitcent = (pprofitcent != "" ? "profitcent^" + pprofitcent + "~" : "");
            }
            string munits = "";
            if (marr.Length > 17)
            {
                punits = GetProperString(marr[17].Trim());
                munits = (punits != "" ? "unit^" + punits + "~" : "");
            }
            string mmaintype = "";
            if (marr.Length > 18)
            {
                pmaintype = GetProperString(marr[18].Trim());
                mmaintype = (pmaintype != "" ? "maintype^" + pmaintype + "~" : "");
            }
            string msubtype = "";
            if (marr.Length > 19)
            {
                psubtype = GetProperString(marr[19].Trim());
                msubtype = (psubtype != "" ? "subtype^" + psubtype + "~" : "");
            }
            string mcountry = "";
            if (marr.Length > 20)
            {
                pcountry = GetProperString(marr[20].Trim());
                mcountry = (pcountry != "" ? "country^" + pcountry + "~" : "");
            }
            string mwarehouse = "";
            if (marr.Length > 21)
            {
                pwarehouse = GetProperString(marr[21].Trim());
                mwarehouse = (pwarehouse != "" ? "warehouse^" + pwarehouse + "~" : "");
            }
            string mbin = "";
            if (marr.Length > 22)
            {
                pbin = GetProperString(marr[22].Trim());
                mbin = (pbin != "" ? "bin^" + pbin + "~" : "");
            }
            string mtaxmas = "";
            if (marr.Length > 23)
            {
                ptaxmas = GetProperString(marr[23].Trim());
                mtaxmas = (ptaxmas != "" ? "tax^" + ptaxmas + "~" : "");
            }
            string mgsttype = "";
            if (marr.Length > 24)
            {
                pgsttype = GetProperString(marr[24].Trim());
                mgsttype = (pgsttype != "" ? "gsttype^" + pgsttype + "~" : "");
            }
            string mhsn = "";
            if (marr.Length > 25)
            {
                phsn = GetProperString(marr[25].Trim());
                mhsn = (phsn != "" ? "hsn^" + phsn + "~" : "");
            }
            string mgrade = "";
            if (marr.Length > 26)
            {
                pgrade = GetProperString(marr[26].Trim());
                mgrade = (pgrade != "" ? "empgrade^" + pgrade + "~" : "");
            }
            string muser = "";
            if (marr.Length > 27)
            {
                puser = GetProperString(marr[27].Trim());
                muser = (puser != "" ? "user^" + puser + "~" : "");
            }
            string mempcat = "";
            if (marr.Length > 28)
            {
                pempcat = GetProperString(marr[28].Trim());
                mempcat = (pempcat != "" ? "empcat^" + pempcat + "~" : "");
            }
            string mdept = "";
            if (marr.Length > 29)
            {
                pdept = GetProperString(marr[29].Trim());
                mdept = (pdept != "" ? "dept^" + pdept + "~" : "");
            }
            string mpost = "";
            if (marr.Length > 30)
            {
                ppost = GetProperString(marr[30].Trim());
                mpost = (ppost != "" ? "post^" + ppost + "~" : "");
            }
            string memployee = "";
            if (marr.Length > 31)
            {
                pemployee = GetProperString(marr[31].Trim());
                memployee = (pemployee != "" ? "employee^" + pemployee + "~" : "");
            }
            string mmccat = "";
            if (marr.Length > 32)
            {
                pmccat = GetProperString(marr[32].Trim());
                mmccat = (pmccat != "" ? "mccat^" + pmccat + "~" : "");
            }
            string mmctype = "";
            if (marr.Length > 33)
            {
                pmctype = GetProperString(marr[33].Trim());
                mmctype = (pmctype != "" ? "mctype^" + pmctype + "~" : "");
            }
            string mmachines = "";
            if (marr.Length > 34)
            {
                pmachines = GetProperString(marr[34].Trim());
                mmachines = (pmachines != "" ? "machine^" + pmachines + "~" : "");
            }
            string mparatype = "";
            if (marr.Length > 35)
            {
                pparatype = GetProperString(marr[35].Trim());
                mparatype = (pparatype != "" ? "qcparatype^" + pparatype + "~" : "");
            }
            string mtestcat = "";
            if (marr.Length > 36)
            {
                ptestcat = GetProperString(marr[36].Trim());
                mtestcat = (ptestcat != "" ? "qctestcat^" + ptestcat + "~" : "");
            }
            string mtests = "";
            if (marr.Length > 37)
            {
                ptests = GetProperString(marr[37].Trim());
                mtests = (ptests != "" ? "qctest^" + ptests + "~" : "");
            }
            //string mpara1 = marr[38].Trim();
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
            //ppara = marr[38].Trim();
            mpara = "";
            if (marr.Length > 38)
            {
                var mudf = marr[38].Replace("^", ",").Trim().Split('|');
                for (var x = 0; x < mudf.Length; x++)
                {
                    if (mudf[x].Trim() != "")
                    {
                        string mval = mudf[x];
                        if (mval.StartsWith("["))
                        {
                            mval = mval.Substring(1, mval.IndexOf("]") - 1);
                        }

                        mpara = mpara + "para" + (x + 1).ToString().PadLeft(2, '0') + "^" + mval + "~";
                        //mpara = mpara + "para" + (x + 1) + "^" + mudf[x] + "~";
                        switch (x + 1)
                        {
                            case 1:
                                ppara01 = mval;
                                break;
                            case 2:
                                ppara02 = mval;
                                break;
                            case 3:
                                ppara03 = mval;
                                break;
                            case 4:
                                ppara04 = mval;
                                break;
                            case 5:
                                ppara05 = mval;
                                break;
                            case 6:
                                ppara06 = mval;
                                break;
                            case 7:
                                ppara07 = mval;
                                break;
                            case 8:
                                ppara08 = mval;
                                break;
                            case 9:
                                ppara09 = mval;
                                break;
                            case 10:
                                ppara10 = mval;
                                break;
                            case 11:
                                ppara11 = mval;
                                break;
                            case 12:
                                ppara12 = mval;
                                break;
                            case 13:
                                ppara13 = mval;
                                break;
                            case 14:
                                ppara14 = mval;
                                break;
                            case 15:
                                ppara15 = mval;
                                break;
                            case 16:
                                ppara16 = mval;
                                break;
                            case 17:
                                ppara17 = mval;
                                break;
                            case 18:
                                ppara18 = mval;
                                break;
                            case 19:
                                ppara19 = mval;
                                break;
                            case 20:
                                ppara20 = mval;
                                break;
                            case 21:
                                ppara21 = mval;
                                break;
                            case 22:
                                ppara22 = mval;
                                break;
                            case 23:
                                ppara23 = mval;
                                break;
                            case 24:
                                ppara24 = mval;
                                break;
                        }
                    }
                }
            }
            mpara = mitemgroups + maccgroups + mstores + mtypes + msalesman + mitems + maccounts + mitemcat + mprocess + mareas +
                mbroker + mcity + mstate + mcategory + maccheads + mcostcent + mprofitcent + munits + mmaintype + msubtype +
                mcountry + mwarehouse + mbin + mtaxmas + mgsttype + mhsn + mgrade + muser + mempcat + mdept + mpost + memployee + mmccat + mmctype + mmachines + mparatype + mtestcat + mtests + mpara;
            return mpara;
        }

        private string GetProperString(string mStr)
        {
            mStr.Trim();
            return (mStr != "" && mStr != "'0'" && mStr != "0" && mStr != "''" ? mStr : "");
        }

        #region executereport
        [HttpPost]
        public ActionResult SaveParametersRep(GridOption Model)
        {
            mPara = rep_SaveParameters(Model.SelectContent.Trim());
            string mGrp = rep_GetGroupby(Model.ViewDataId);
            if (Model.IsFormatSelected == true)
            {
                ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=0 Where SubCodeOf='" + Model.OptionCode + "'");
                ctxTFAT.Database.ExecuteSqlCommand("Update ReportHeader Set DefaultReport=-1 Where Code='" + Model.ViewDataId + "'");
            }
            return Json(new { Group = mGrp, Status = "Success", Message = "" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            ReportQuery mobj = new ReportQuery();
            string mode = "A";
            var mrep = ctxTFAT.ReportQuery.Where(x => x.Code == Model.ViewDataId && x.UserID == muserid).Select(x => x.Code).FirstOrDefault();
            if (mrep != null)
            {
                mobj = ctxTFAT.ReportQuery.Where(x => x.Code == Model.ViewDataId && x.UserID == muserid).Select(x => x).FirstOrDefault();
                mode = "E";
            }
            var date = Model.Date.Replace("-", "/").Split(':');
            mobj.Code = Model.ViewDataId;
            mobj.Parameters = mPara;
            mobj.FromDate = Convert.ToDateTime(date[0]);
            //Model.FromDate == null ? DateTime.Now : Convert.ToDateTime(Model.FromDate);
            mobj.ToDate = Convert.ToDateTime(date[1]);
            //Model.ToDate == null ? DateTime.Now : Convert.ToDateTime(Model.ToDate);
            mobj.URL = "";
            mobj.UserID = muserid;
            mobj.AUTHIDS = muserid;
            mobj.AUTHORISE = "A00";
            //mobj.LASTUPDATEDATE = DateTime.Now;
            mobj.ENTEREDBY = muserid;
            if (mode == "A")
            {
                mobj.LASTUPDATEDATE = DateTime.Now;
                ctxTFAT.ReportQuery.Add(mobj);
            }
            else
            {
                ctxTFAT.Entry(mobj).State = EntityState.Modified;
            }
            ctxTFAT.SaveChanges();

            // execute stored procedure while on the first page (SPTFAT_StockStatement, DaybookItem etc)
            //            if (Model.page == 1)
            //                ExecutePreQuery(msubcodeof, Model.Date);

            //Session["ErrorMessage"] = "";
            //Model.query = (ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).Select(z => z.DrillQuery.Trim()).FirstOrDefault() ?? "").Trim();
            return GetGridReport(Model, "R", (mmaintype != null ? "MainType^" + mmaintype + "~" : "") + mPara, false, 0);
        }

        [HttpPost]
        public ActionResult GetSubGridStructureRep(GridOption Model)
        {
            string msubgrid = "";
            msubgrid = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).Select(z => z.DrillQuery.Trim()).FirstOrDefault() ?? "";
            if (msubgrid == "")
            {
                return Json(new { Status = "Error", Message = "Sub-Grid format not found.." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (msubgrid.Contains("~")) // name of the column to take else Document column e.g.SubGrid-OrdersExecuted~TableKey
                {
                    msubgrid = msubgrid.Substring(0, msubgrid.LastIndexOf("~"));
                }
                ////IReportGridOperation mIlst = new ListViewGridOperationreport();
                return GetGridDataColumns(msubgrid, "X", "");
            }
        }

        [HttpPost]
        public ActionResult GetSubGridDataRep(GridOption Model)
        {
            string msubgrid = "";
            msubgrid = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).Select(z => z.DrillQuery.Trim()).FirstOrDefault() ?? "";
            if (msubgrid == "")
            {
                return Json(new { Status = "Error", Message = "Sub-Grid format not found.." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (msubgrid.Contains("~")) // name of the column to take else Document column e.g.SubGrid-OrdersExecuted~TableKey
                {
                    msubgrid = msubgrid.Substring(0, msubgrid.LastIndexOf("~"));
                }
                var date = Model.Date.Replace("-", "/").Split(':');
                Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                Model.ViewDataId = msubgrid;
                //Model.Document = Model.Document.Substring(6);
                return GetGridReport(Model, "R", "Document^" + Model.Document + "~Code^" + Model.Document, false, 0);
            }
        }

        [HttpPost]
        public ActionResult GetDrillQuery(GridOption Model)
        {
            string msubgrid = ctxTFAT.ReportHeader.Where(x => x.Code == Model.ViewDataId).Select(z => z.DrillQuery.Trim()).FirstOrDefault() ?? "";
            if (msubgrid != "")
            {
                if (msubgrid.Contains("~")) // name of the column to take else Document column e.g.SubGrid-OrdersExecuted~TableKey
                {
                    msubgrid = msubgrid.Substring(msubgrid.LastIndexOf("~") + 1);
                }
                else
                {
                    msubgrid = "Document";
                }
            }
            return Json(new { query = msubgrid, Message = "" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPDFX(GridOption Model, string mwhat, string mpageorient, string mpapersize, string memaildata)
        {
            Model.mWhat = mwhat;
            string[] mArr = { "", "", "", "", "", "", "", "", "", "" };
            switch (mwhat)
            {
                case "RPDF":
                    return ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Portrait", (mmaintype != null ? "MainType^" + mmaintype + "~" : "") + mPara), Model.ViewDataId, "pdf", false, Model.Code);
                case "RPDL":
                    return ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", (mmaintype != null ? "MainType^" + mmaintype + "~" : "") + mPara), Model.ViewDataId, "pdf", false, Model.Code);
                case "RXLS":
                    return ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", (mmaintype != null ? "MainType^" + mmaintype + "~" : "") + mPara), Model.ViewDataId, "Excel", false, Model.Code);
                case "RWRD":
                    return ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", (mmaintype != null ? "MainType^" + mmaintype + "~" : "") + mPara), Model.ViewDataId, "Word", false, Model.Code);
                case "EPDF":
                    return ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", (mmaintype != null ? "MainType^" + mmaintype + "~" : "") + mPara), Model.ViewDataId, "pdf", true, Model.Code, memaildata);
                case "EXLS":
                    return ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", (mmaintype != null ? "MainType^" + mmaintype + "~" : "") + mPara), Model.ViewDataId, "Excel", true, Model.Code, memaildata);
                case "EWRD":
                    return ExportSSRSReport(CreateSSRSReport(Model, "L", mArr, "Landscape", (mmaintype != null ? "MainType^" + mmaintype + "~" : "") + mPara), Model.ViewDataId, "Word", true, Model.Code, memaildata);
                case "CRPDF":   // crystal report format
                    return PrintReportsCrystal(Model, "REP_" + msubcodeof, "SPREP_" + msubcodeof, mwhat, false, (mmaintype != null ? "MainType^" + mmaintype + "~" : "") + (mPara != "" ? "~" + mPara : ""), mpageorient, mpapersize);
                case "PDF":
                    break;
                case "PDL":
                    break;
            }
            return GetGridReport(Model, "R", (mmaintype != null ? "MainType^" + mmaintype + "~" : "") + mPara, false, 0, "", mpapersize);
        }

        public ActionResult PrintReport(GridOption Model)
        {
            Model.mWhat = "PDF";
            return PrintReportsCrystal(Model, "REP_" + msubcodeof, "SPREP_" + msubcodeof, "PDF", false, mPara, "Landscape", "A4");
        }
        #endregion executereport
    }
}