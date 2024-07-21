using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.Mvc;
using System.Xml;
using EntitiModel;
using ALT_ERP3;
using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.DynamicBusinessLayer
{
    public class ListViewGridOperation : IListViewGridOperation
    {
        //TFATERPDatabaseEntities1 mCtx = new TFATERPDatabaseEntities1();
        //nEntities mCtx = new nEntities();
        //ALT_ERP21Entities ctxTFAT = new ALT_ERP21Entities();

        private IBusinessCommon mIBusi = new BusinessCommon();

        public JsonResult getGridDataColumns(string id,string mFlag = "L")
        {
            var mTfatSearch = ctxTFAT.TfatSearch.Where(m => m.Code == id).Select(m => new { m.ColField, m.ColHead });
            List<string> colname = new List<string>();
            List<GridColumn> colModal = new List<GridColumn>();
            List<object> result = new List<object>();

            //mFlag   L -List   R- Report

            if (mFlag == "L")
            {
                colname.Add("");

                GridColumn gc1 = new GridColumn();
                gc1.name = "Edit";
                gc1.index = "Edit";
                gc1.width = "40px";
                gc1.frozen = true;
                colModal.Add(gc1);

                colname.Add("");

                gc1 = new GridColumn();
                gc1.name = "Delete";
                gc1.index = "Delete";
                gc1.width = "40px";
                gc1.frozen = true;
                colModal.Add(gc1);

                //Added by Snehalata 21/12/2015 to display view button in gridview
                colname.Add("");
                gc1 = new GridColumn();
                gc1.name = "View";
                gc1.index = "View";
                gc1.width = "40px";
                colModal.Add(gc1);
                 
                
                //var pkColumnName = ctxTFAT.TFATDbfs.Where(x => x.fle== id).Select(x => x.Indexes).FirstOrDefault();
                //if (pkColumnName.Contains("~"))
                //{
                //    string pkcolName = pkColumnName.Substring(0, pkColumnName.LastIndexOf('~')).Replace("(P)", "");
                //}
                
                //string pkcol = pkcolName.Replace("(P)", "");
                colname.Add("KeyCol");
                gc1 = new GridColumn();
                gc1.name = "KeyCol";
                gc1.index = "KeyCol";
                gc1.hidden = true;
                gc1.width = "0px";
                gc1.frozen = true;
                colModal.Add(gc1);
                //Code Ended
            }
            foreach (var Fld in mTfatSearch)
            {
                colname.Add(Fld.ColHead);

                GridColumn gc = new GridColumn();
                gc.name = Fld.ColHead;
                gc.index = Fld.ColHead;
                gc.editable = false;
                colModal.Add(gc);
            }
            result.Add(Core.CoreCommon.GetString(colname.ToArray()));
            result.Add(colModal);

            JsonResult JR = new JsonResult();
            JR.Data = Newtonsoft.Json.JsonConvert.SerializeObject(result);
            return JR;
        }

        public object Get(string sidx, string sord, int page, int rows, bool _search, string searchField, string searchOper, string searchString, string mFlag = "L")
        {
            string optionCode = HttpContext.Current.Request.UrlReferrer.LocalPath.Split('/').Last();
            //string tableName = HttpContext.Current.Request.UrlReferrer.LocalPath.Split('/').Last();
            string tableName = "SalesMan";
            IEnumerable <object> DHeader;
            if (mFlag == "L")
            {
                //DHeader = mIBusi.SelectSingleTfatDeignHeader(optionCode).ToList();
                //tableName = DHeader.FirstOrDefault().GetType().GetProperty("TableName").GetValue(DHeader.FirstOrDefault()).ToString().Trim();
            }
            else
            {
                DHeader = mIBusi.SelectSingleReportHeader(optionCode).ToList();
                tableName = DHeader.FirstOrDefault().GetType().GetProperty("Tables").GetValue(DHeader.FirstOrDefault()).ToString().Trim();
            }

            var mData = ctxTFAT.Set(Core.CoreCommon.GetTableType(tableName)) as IEnumerable<object>;

            if (_search)
            {
                switch (searchOper)
                {
                    case "eq":
                        mData = from r in mData
                                where r.GetType().GetProperty(searchField).GetValue(r).ToString().Equals(searchString)
                                select r;
                        break;
                    case "ne":
                        mData = from r in mData
                                where r.GetType().GetProperty(searchField).GetValue(r).ToString().Equals(searchString) == false
                                select r;
                        break;
                    case "bw":
                        mData = from r in mData
                                where r.GetType().GetProperty(searchField).GetValue(r).ToString().StartsWith(searchString)
                                select r;
                        break;
                    case "bn":
                        mData = from r in mData
                                where r.GetType().GetProperty(searchField).GetValue(r).ToString().StartsWith(searchString) == false
                                select r;
                        break;
                    case "ew":
                        mData = from r in mData
                                where r.GetType().GetProperty(searchField).GetValue(r).ToString().EndsWith(searchString)
                                select r;
                        break;
                    case "en":
                        mData = from r in mData
                                where r.GetType().GetProperty(searchField).GetValue(r).ToString().EndsWith(searchString) == false
                                select r;
                        break;
                    case "cn":
                    case "in":
                        mData = from r in mData
                                where r.GetType().GetProperty(searchField).GetValue(r).ToString().Contains(searchString)
                                select r;
                        break;
                    case "nc":
                    case "ni":
                        mData = from r in mData
                                where r.GetType().GetProperty(searchField).GetValue(r).ToString().Contains(searchString) == false
                                select r;
                        break;
                }
            }

            if (sord == "desc")
            {
                mData = from r in mData
                        orderby r.GetType().GetProperty(sidx).GetValue(r) descending
                        select r;
            }
            else
            {
                mData = from r in mData
                        orderby r.GetType().GetProperty(sidx).GetValue(r)
                        select r;
            }
            int count = mData.Count();
            //try
            //{
            //    count = mData.Count();
            //}
            //catch(Exception ex)
            //{

            //}
            // int count = mData.Count();
            int pageIndex = Convert.ToInt32(page) - 1;
            int pageSize = rows;
            int totalRecords = count;
            int totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);
           
            var Data = mData.AsEnumerable().Skip(pageIndex * pageSize).Take(pageSize);

            var result = new
            {
                total = totalPages,
                page = page,
                records = totalRecords,
                rows = (from d in Data
                        select new
                        {
                            i = d.GetType().GetProperty("Code").GetValue(d).ToString(),
                            //cell = GetCellFields(d.GetType().GetProperty("Code").GetValue(d).ToString(), DHeader, Data, mFlag)
                        }).ToArray()
            };
            return result;
        }

        private string[] GetCellFields(string Code, IEnumerable<object> DHeaderData, IEnumerable<object> Data, string mFlag)
        {
            dynamic TableName;
            string OptionCode;
            if (mFlag == "L")
            {
                TableName = DHeaderData.FirstOrDefault().GetType().GetProperty("TableName").GetValue(DHeaderData.FirstOrDefault());
                OptionCode = DHeaderData.FirstOrDefault().GetType().GetProperty("OptionCode").GetValue(DHeaderData.FirstOrDefault()).ToString();
            }
            else
            {
                TableName = DHeaderData.FirstOrDefault().GetType().GetProperty("Tables").GetValue(DHeaderData.FirstOrDefault());
                OptionCode = DHeaderData.FirstOrDefault().GetType().GetProperty("Code").GetValue(DHeaderData.FirstOrDefault()).ToString();
            }

            var mFlds = ctxTFAT.TfatSearch.Where(m => m.Code == OptionCode);
            var resultDB = from d in Data
                           where d.GetType().GetProperty("Code").GetValue(d).ToString().Equals(Code.ToString())
                           select d;
            //Search Foreign Key Table
            var fkeys = ctxTFAT.Database.SqlQuery(typeof(ForeignKey), "sp_fkeys @fktable_name=@fknm", new SqlParameter("@fknm", TableName)).ToListAsync().Result;

            //Changed by Snehalata 15/12/2015 to add additional column in grid
            //string[] mArr = new string[Convert.ToInt32(mFlds.Count() + (mFlag == "L" ? 3 : 0))];
            string[] mArr = new string[Convert.ToInt32(mFlds.Count() + (mFlag == "L" ? 4 : 0))];

            foreach (var CurrentDt in resultDB)
            {
                int i = 0;
                foreach (var mF in mFlds)
                {
                    string[] fldArr = mF.ColField.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    if (fldArr.Count() > 1)
                    {
                        //Search Foreign Key Table
                        var fkeyTableData = ctxTFAT.Database.SqlQuery(typeof(ForeignKey), "sp_fkeys @pktable_name=@pktblnm,@fktable_name =@fktblnm", new SqlParameter("@pktblnm", fldArr[0]), new SqlParameter("@fktblnm", TableName)).ToListAsync().Result;
                        if (fkeyTableData.Count > 0)
                        {
                            foreach (var fkey in fkeyTableData)
                            {
                                if (fldArr.Last().ToString().Equals(fkey.GetType().GetProperty("PKCOLUMN_NAME").GetValue(fkey)))
                                {
                                    string fkeyTable = fkey.GetType().GetProperty("PKTABLE_NAME").GetValue(fkey).ToString();
                                    var FKeyData = ctxTFAT.Set(Core.CoreCommon.GetTableType(fkeyTable)) as IEnumerable<object>;
                                    var FkeyResultDB = from f in FKeyData
                                                       where f.GetType().GetProperty(fkey.GetType().GetProperty("PKCOLUMN_NAME").GetValue(fkey).ToString()).GetValue(f).ToString().Equals(CurrentDt.GetType().GetProperty(fkey.GetType().GetProperty("FKCOLUMN_NAME").GetValue(fkey).ToString()).GetValue(CurrentDt).ToString())
                                                       select f;
                                    if (FkeyResultDB.Count() > 0)
                                    {
                                        foreach (var FKeyResult in FkeyResultDB)
                                        {
                                            mArr[i] = Convert.ToString(FKeyResult.GetType().GetProperty("Name").GetValue(FKeyResult));
                                        }
                                    }
                                }
                                else
                                {
                                    AddDefault(mArr, ref i, string.IsNullOrWhiteSpace(mF.ColField.ToString()) ? mF.ColHead.ToString() : mF.ColField.ToString(), CurrentDt, DHeaderData, mFlag);
                                }
                            }
                        }
                        else
                        {
                            //Search Primay Key Table
                            var pkeyTableData = ctxTFAT.Database.SqlQuery(typeof(PrimaryKey), "sp_pkeys @table_name=@tblnm", new SqlParameter("@tblnm", fldArr[0])).ToListAsync().Result;
                            if (pkeyTableData.Count > 0)
                            {
                                foreach (var pkey in pkeyTableData)
                                {
                                    if (fldArr.Last().ToString().Equals(pkey.GetType().GetProperty("COLUMN_NAME").GetValue(pkey)))
                                    {
                                        string pkeyTable = pkey.GetType().GetProperty("TABLE_NAME").GetValue(pkey).ToString();
                                        var PKeyData = ctxTFAT.Set(Core.CoreCommon.GetTableType(pkeyTable)) as IEnumerable<object>;
                                        var PkeyResultDB = from f in PKeyData
                                                           where f.GetType().GetProperty(pkey.GetType().GetProperty("COLUMN_NAME").GetValue(pkey).ToString()).GetValue(f).ToString().Equals(CurrentDt.GetType().GetProperty(pkey.GetType().GetProperty("COLUMN_NAME").GetValue(pkey).ToString()).GetValue(CurrentDt).ToString())
                                                           select f;
                                        if (PkeyResultDB.Count() > 0)
                                        {
                                            foreach (var FKeyResult in PkeyResultDB)
                                            {
                                                mArr[i] = Convert.ToString(FKeyResult.GetType().GetProperty("Name").GetValue(FKeyResult));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        AddDefault(mArr, ref i, string.IsNullOrWhiteSpace(mF.ColField.ToString()) ? mF.ColHead.ToString() : mF.ColField.ToString(), CurrentDt, DHeaderData, mFlag);
                                    }
                                }
                            }
                            else
                            {
                                AddDefault(mArr, ref i, string.IsNullOrWhiteSpace(mF.ColField.ToString()) ? mF.ColHead.ToString() : mF.ColField.ToString(), CurrentDt, DHeaderData, mFlag);
                            }
                        }
                    }
                    else
                    {
                        if (fkeys.Count > 0)
                        {
                            foreach (var fkey in fkeys)
                            {
                                if (mF.ColField.ToString().Equals(fkey.GetType().GetProperty("FKCOLUMN_NAME").GetValue(fkey)))
                                {
                                    string fkeyTable = fkey.GetType().GetProperty("PKTABLE_NAME").GetValue(fkey).ToString();
                                    var FKeyData = ctxTFAT.Set(Core.CoreCommon.GetTableType(fkeyTable)) as IEnumerable<object>;
                                    var FkeyResultDB = from f in FKeyData
                                                       where f.GetType().GetProperty(fkey.GetType().GetProperty("PKCOLUMN_NAME").GetValue(fkey).ToString()).GetValue(f).ToString().Equals(CurrentDt.GetType().GetProperty(mF.ColField.ToString()).GetValue(CurrentDt))
                                                       select f;
                                    if (FkeyResultDB.Count() > 0)
                                    {
                                        foreach (var FKeyResult in FkeyResultDB)
                                        {
                                            mArr[i] = Convert.ToString(FKeyResult.GetType().GetProperty("Name").GetValue(FKeyResult));
                                        }
                                    }
                                }
                                else
                                {
                                    AddDefault(mArr, ref i, string.IsNullOrWhiteSpace(mF.ColField.ToString()) ? mF.ColHead.ToString() : mF.ColField.ToString(), CurrentDt, DHeaderData, mFlag);
                                }
                            }
                        }
                        else
                        {
                            AddDefault(mArr, ref i, string.IsNullOrWhiteSpace(mF.ColField.Trim().ToString()) ? mF.ColHead.Trim().ToString() : mF.ColField.Trim().ToString(), CurrentDt, DHeaderData, mFlag);
                        }
                    }
                    i++;
                }
            }
            return mArr;
        }

        private void AddDefault(string[] mArr, ref int i, string ColName, object CurrentDt, IEnumerable<object> DHeaderData, string mflag)
        {
                              
            string tablename = "";
            tablename = DHeaderData.FirstOrDefault().GetType().GetProperty("OptionCode").GetValue(DHeaderData.FirstOrDefault()).ToString();
            //var colnm = ctxTFAT.TFATDbfs.Where(x => x.fle == tablename).Select(x => x.Indexes).FirstOrDefault();
            //if (colnm.Contains("~"))
            //{
            //    string col_name = colnm.Substring(0, colnm.LastIndexOf("~")).Replace("(P)", "");
            //    string mval = Convert.ToString(CurrentDt.GetType().GetProperty(col_name).GetValue(CurrentDt));

            //    //string col_name = colnm.Substring(0, colnm.LastIndexOf("~")).Replace("(P)", "");
            //    //string mval = Convert.ToString(CurrentDt.GetType().GetProperty(col_name).GetValue(CurrentDt));
            //    string mValue = Convert.ToString(CurrentDt.GetType().GetProperty(ColName).GetValue(CurrentDt));
            //    if (mflag == "L")
            //    {
            //        if (i == 0)
            //        {
            //            // //mArr[i] = "<a style='text-decoration:underline !important;' href='" + DHeaderData.FirstOrDefault().GetType().GetProperty("EditURL").GetValue(DHeaderData.FirstOrDefault()) + mValue + "'><img src='/Images/icon/edit.png'/></a>";
            //            //// mArr[i] = "<a style='text-decoration:underline !important;' href=''><img src='/Images/icon/edit.png'/></a>";
            //            // mArr[i] = "<input id='btnEdit" + mval + "' type='button' value='Edit' />";
            //            //mArr[i] = "<button id='btnEdit' type='button' value='Edit' />";
            //            mArr[i] = "<button id='btnEdit" + mval + "' type='button' value='Edit' Class='edit' tooltip = 'Edit', title = 'Edit' />";
            //            i++;
            //            // //mArr[i] = "<a style='text-decoration:underline !important;' href='" + DHeaderData.FirstOrDefault().GetType().GetProperty("DeleteURL").GetValue(DHeaderData.FirstOrDefault()) + mValue + "'><img src='/Images/icon/trash_(delete)_16x16.gif'/></a>";
            //            //// mArr[i] = "<a style='text-decoration:underline !important;' href='ViewList/Index/CityMaster'><img src='/Images/icon/trash_(delete)_16x16.gif'/></a>";
            //            // mArr[i] = "<input id='btnDelete" + mval + "' type='button' value='Delete' />";
            //            //mArr[i] = "<button id='btnDelete' type='button' value='Delete' />";
            //            mArr[i] = "<button id='btnDelete" + mval + "' type='button' value='Delete' Class='delete' tooltip = 'Delete', title = 'Delete' />";
            //            i++;
            //            mArr[i] = "<button id='btnView" + mval + "' type='button' value='View' Class='view' tooltip = 'View', title = 'View' />";
            //            i++;
            //            mArr[i] = mval;
            //            i++;
            //        }
            //    }
            //    mArr[i] = mValue;
            //}
        }
    }
}