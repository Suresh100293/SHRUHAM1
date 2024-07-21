using ALT_ERP3.Areas.DashBoard.Models;
using ALT_ERP3.Areas.Logistics.Controllers;
using ALT_ERP3.Models;
using Common;
using EntitiModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Controllers
{
    public class FirstPageController : BaseController
    {
        // GET: FirstPage

        //public ActionResult Index()
        //{
        //    // active modules
        //    List<TFATMenuVM> lmod = new List<TFATMenuVM>();

        //    if (Session["Modules"].ToString().Substring(0, 1) == "S")
        //        lmod.Add(new TFATMenuVM() { Menu = "SetUP", Controller = "SetUP" });
        //    if (Session["Modules"].ToString().Substring(1, 1) == "M")
        //        lmod.Add(new TFATMenuVM() { Menu = "Masters", Controller = "MasterDashBoard" });
        //    if (Session["Modules"].ToString().Substring(2, 1) == "T")
        //        lmod.Add(new TFATMenuVM() { Menu = "Transactions", Controller = "TransactionsDashBoard" });
        //    if (Session["Modules"].ToString().Substring(3, 1) == "R")
        //        lmod.Add(new TFATMenuVM() { Menu = "Reports", Controller = "ReportsDashBoard" });
        //    if (Session["Modules"].ToString().Substring(4, 1) == "C")
        //        lmod.Add(new TFATMenuVM() { Menu = "ControlPanel", Controller = "ControlPanel" });


        //    @Session["ModuleName"] = "ALT.air.3";
        //    ViewBag.Modules = lmod;
        //    // create list for favorites
        //    int mversion = Convert.ToInt32(Session["Version"]);
        //    string mquery;
        //    if (muserid.ToUpper() == "SUPER")
        //    {
        //        mquery = "Select AllowClick=isnull(AllowClick,0),Menu," +
        //                "ParentMenu=isnull(ParentMenu,''),ID=isnull(ID,''),SubType=isnull(SubType,''),MainType=isnull(MainType,''),DisplayOrder=isnull(DisplayOrder,1),FormatCode=isnull(FormatCode,''),Level=isnull(Level,1),TableName=isnull(TableName,'')," +
        //                "Hide=isnull(Hide,0),ModuleName=isnull(ModuleName,''),OptionType=isnull(OptionType,''),OptionCode=isnull(OptionCode,''),Controller=isnull(Controller,''),QuickMenu=isnull(QuickMenu,0),QuickMaster=isnull(QuickMaster,0),Version=isnull(Version,0) " +
        //                "from TfatMenu Where QuickMenu<>0 and AllowClick<>0 and Hide=0 Order by DisplayOrder";
        //    }
        //    else
        //    {
        //        mquery = "Select AllowClick=isnull(m.AllowClick,0),Menu," +
        //                "ParentMenu=isnull(m.ParentMenu,''),ID=isnull(m.ID,''),SubType=isnull(m.SubType,''),MainType=isnull(m.MainType,''),DisplayOrder=isnull(m.DisplayOrder,1),FormatCode=isnull(m.FormatCode,''),Level=isnull(m.Level,1),TableName=isnull(m.TableName,'')," +
        //                "Hide=isnull(m.Hide,0),ModuleName=isnull(m.ModuleName,''),OptionType=isnull(m.OptionType,''),OptionCode=isnull(m.OptionCode,''),Controller=isnull(m.Controller,''),QuickMenu=isnull(m.QuickMenu,0),QuickMaster=isnull(m.QuickMaster,0),Version=isnull(m.Version,0) " +
        //                "from TfatMenu m,UserRights u where m.ID=u.MenuID and u.Code='" + muserid + "' and u.xCess<>0 and m.QuickMenu<>0 and m.AllowClick<>0 and m.Hide=0 Order by m.DisplayOrder";
        //    }

        //    List<TFATMenuVM> list = new List<TFATMenuVM>();
        //    DataTable mlistx = GetDataTable(mquery);
        //    foreach (DataRow item in mlistx.Rows)
        //    {
        //        if (Convert.ToInt32(item["Version"]) > mversion)
        //        {
        //            goto mnext;
        //        }
        //        list.Add(new TFATMenuVM()
        //        {
        //            AllowClick = Convert.ToBoolean(item["AllowClick"]),
        //            Menu = item["Menu"].ToString() ?? "",
        //            ParentMenu = item["ParentMenu"].ToString() ?? "",
        //            DisplayOrder = (int)item["DisplayOrder"],
        //            FormatCode = item["FormatCode"].ToString() ?? "",
        //            SubType = item["SubType"].ToString() ?? "",
        //            MainType = item["MainType"].ToString() ?? "",
        //            Level = (byte)item["Level"],
        //            Controller = (item["OptionType"].ToString() == "R" || item["OptionType"].ToString() == "X" || item["OptionType"].ToString() == "") ? item["Controller"].ToString() : GetControllerName(item["OptionType"].ToString()),
        //            TableName = item["TableName"].ToString() ?? "",
        //            Hide = Convert.ToBoolean(item["Hide"]),
        //            ModuleName = item["ModuleName"].ToString() ?? "",
        //            OptionType = item["OptionType"].ToString() ?? "",
        //            OptionCode = item["OptionCode"].ToString() ?? "",
        //            Controller2 = item["Controller"].ToString() ?? "",
        //        });
        //    mnext:;
        //    }

        //    ViewBag.favlist = list;
        //    Session["FavList"] = ViewBag.favlist;
        //    return View();
        //}

        private static string mbasegr = "";
        JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

        public ActionResult Index(ActiveObjectsVM Model)
        {

            EwayBillController ewayBill = new EwayBillController();
            //ewayBill.GenerateEwayBill();

            Session["ModuleName"] = "";
            GetAllMenu(Session["ModuleName"].ToString());
            Model.FromDate = Convert.ToDateTime(System.Web.HttpContext.Current.Session["StartDate"].ToString());
            Model.ToDate = Convert.ToDateTime(System.Web.HttpContext.Current.Session["LastDate"].ToString());
            if (Model.ToDate > DateTime.Today) Model.ToDate = DateTime.Today;

            //var Branch = ctxTFAT.TfatBranch.Where(x => x.Code == mbranchcode).Select(x => x.Name).FirstOrDefault();
            //Model.Branch = Branch;
            //Model.User = muserid;

            //ViewBag.Code = "";
            //string mStr = "";
            //List<ActiveObjectsVM> lmod = new List<ActiveObjectsVM>();
            //var result = ctxTFAT.ActiveSideBarObjects.Where(x => x.Users.Contains(muserid) && x.Status == true).ToList();
            //if (result.Count() > 0)
            //{
            //    foreach (var item in result)
            //    {
            //        ActiveObjectsVM objectsVM = new ActiveObjectsVM();
            //        objectsVM.Name = item.Name;
            //        objectsVM.Code = item.Code;
            //        objectsVM.ID = item.ID;
            //        mStr += item.Code + "|";
            //        if (item.Code == "Outstanding")
            //        {
            //            OutstandingReports(item.Para1 == "Customer" ? true : false, item.Para11, item.Para8, item.Para9, item.Para10);
            //        }
            //        lmod.Add(objectsVM);
            //    }
            //    ViewBag.Code = mStr;
            //}
            //Model.codes = lmod;

            //Model.DashboardActive = ctxTFAT.TfatPass.Where(z => z.Code == muserid).Select(x => x.DashboardActive).FirstOrDefault();
            //ViewBag.Code = "";
            //string mStr = "";
            //if (Model.DashboardActive == true)
            //{
            //    var groups = ctxTFAT.ActiveObjects.Where(z => z.Status == true && z.SizeType != "0" && z.Users.Contains(muserid)).Select(x => new { x.Code, x.Name, x.SizeType, x.ObjectType, x.ReportCode }).ToList();
            //    foreach (var item in groups)
            //    {
            //        lmod.Add(new ActiveObjectsVM() { Code = item.Code, Name = item.Name, SizeType = item.SizeType, ObjectType = item.ObjectType, ReportCode = item.ReportCode });
            //        mStr += item.ReportCode + "|";
            //    }
            //    decimal? mAmt = 0;
            //    int? mCnt = 0;
            //    // no.of users defined
            //    Model.EnqCount = ctxTFAT.TfatPass.Count();
            //    // msgs pending
            //    Model.QtnCount = ctxTFAT.MessageLog.Where(z => z.Code == muserid && z.MessageRead == false && z.MessageDelete == false).Count();
            //    // auth pending
            //    Model.OrdCount = ctxTFAT.Authorisation.Where(z => z.AUTHORISE.StartsWith("N") && z.AUTHIDS == muserid && z.Branch == mbranchcode).Count();
            //    // pending tasks
            //    Model.InvCount = ctxTFAT.Task.Where(z => z.AssignedTo == muserid && z.Status == "Pending").Count();
            //    ViewBag.Code = mStr;
            //}
            //Model.codes = lmod;

            //DataTable Record = new DataTable();
            //using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            //{
            //    string sql = string.Format(@"Select ID from ActiveSideBarObjects where Status='true' and  '" + muserid + "'  in (SELECT value FROM STRING_SPLIT(ActiveSideBarObjects.Users, ','))  ");
            //    SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            //    da.Fill(Record);
            //}
            //var Recordlst = Record.AsEnumerable()
            //             .Select(r => r.Table.Columns.Cast<DataColumn>()
            //             .Select(c => new KeyValuePair<string, object>(c.ColumnName, r[c.Ordinal])
            //          ).ToDictionary(z => z.Key, z => z.Value)
            //       ).ToList();
            //if (Recordlst.Count > 0)
            //{
            //    Model.DashboardActive = true;
            //}
            return View(Model);
        }

        public ActionResult GetSideMenu(ActiveObjectsVM Model, bool Reload)
        {
            bool GetRefresh = false;

            Model = Session["SideMenuModel"] as ActiveObjectsVM;
            ViewBag.Code = "";
            string mStr = "", mStr1 = "";
            if (Reload)
            {
                GetRefresh = true;
            }
            else
            {
                if (Model == null)
                {
                    GetRefresh = false;
                }
                else
                {
                    if (Model.codes != null)
                    {
                        if (Model.codes.Count() == 0)
                        {
                            GetRefresh = false;
                        }
                        else
                        {
                            GetRefresh = false;
                        }
                    }

                }
            }


            if (GetRefresh)
            {
                Model = new ActiveObjectsVM();
                Model.DefaultMap = true;
                ViewBag.DiaGram = "";

                List<ActiveObjectsVM> lmod = new List<ActiveObjectsVM>();
                #region GetOnly Particular SideMenu List
                DataTable Record = new DataTable();
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string sql = string.Format(@"Select ID from ActiveSideBarObjects where '" + muserid + "'  in (SELECT value FROM STRING_SPLIT(ActiveSideBarObjects.Users, ','))  ");
                    SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                    da.Fill(Record);
                }
                var Recordlst = Record.AsEnumerable()
                             .Select(r => r.Table.Columns.Cast<DataColumn>()
                             .Select(c => new KeyValuePair<string, object>(c.ColumnName, r[c.Ordinal])
                          ).ToDictionary(z => z.Key, z => z.Value)
                       ).ToList();

                List<string> IDList = new List<string>();
                foreach (var item in Recordlst)
                {
                    IDList.Add(item["ID"].ToString());
                }
                #endregion
                var result = ctxTFAT.ActiveSideBarObjects.Where(x => IDList.Contains(x.ID) && x.Status == true).OrderBy(x => x.DisplayOrder).ToList();
                if (result.Count() > 0)
                {
                    foreach (var item in result)
                    {
                        DataTable dt = new DataTable();
                        string Query = "";
                        ActiveObjectsVM objectsVM = new ActiveObjectsVM();
                        objectsVM.Name = item.Name;
                        objectsVM.Code = item.Code;
                        objectsVM.ID = item.ID;

                        //mStr1 += (item.ObjectType == "BGH" ? "horizontalBar" : item.ObjectType == "PC" ? "pie" : "bar") + "|";
                        //mStr += item.Code + "|";
                        if (item.Code == "Outstanding")
                        {
                            OutstandingReports(item.Para1 == "Customer" ? true : false, item.Para11, "01-Jan-1950", EndDate, item.Para10);
                            Query = "select  Cast(sum((Case when ODueDays between 0 and 30 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [0-30], Cast(sum((Case when ODueDays between 30+1  and 60 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [30-60], Cast(sum((Case when ODueDays between 60+1  and 90 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [60-90], Cast(sum((Case when ODueDays between 90+1  and 120 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [90-120], Cast(sum((Case when ODueDays between 120+1  and 150 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [120-150], Cast(Sum((Case when ODueDays between 150+1  and 180 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [150-180], Cast(sum((Case when ODueDays>180 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [>180],sum(Amt),sum(OnAccount ) As UNAdj,sum(Pending-OnAccount ) as TotalBal from Dztmp_zOS";
                            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                            SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                            SqlDataAdapter sda = new SqlDataAdapter(cmd);
                            //if (dt.Rows.Count!=0)
                            {
                                sda.Fill(dt);
                            }
                            tfat_conx.Close();
                            tfat_conx.Dispose();
                            if (dt.Rows.Count > 0)
                            {
                                Model.OS30 = Convert.ToDecimal(dt.Rows[0]["0-30"].ToString() == "" ? "0" : dt.Rows[0]["0-30"].ToString());
                                Model.OS60 = Convert.ToDecimal(dt.Rows[0]["30-60"].ToString() == "" ? "0" : dt.Rows[0]["30-60"].ToString());
                                Model.OS90 = Convert.ToDecimal(dt.Rows[0]["60-90"].ToString() == "" ? "0" : dt.Rows[0]["60-90"].ToString());
                                Model.OS120 = Convert.ToDecimal(dt.Rows[0]["90-120"].ToString() == "" ? "0" : dt.Rows[0]["90-120"].ToString());
                                Model.OS150 = Convert.ToDecimal(dt.Rows[0]["120-150"].ToString() == "" ? "0" : dt.Rows[0]["120-150"].ToString());
                                Model.OS180 = Convert.ToDecimal(dt.Rows[0]["150-180"].ToString() == "" ? "0" : dt.Rows[0]["150-180"].ToString());
                                Model.OS180M = Convert.ToDecimal(dt.Rows[0][">180"].ToString() == "" ? "0" : dt.Rows[0][">180"].ToString());
                                Model.OSTotal = Convert.ToDecimal(dt.Rows[0]["TotalBal"].ToString() == "" ? "0" : dt.Rows[0]["TotalBal"].ToString());
                                Model.OSUnAdj = Convert.ToDecimal(dt.Rows[0]["UNAdj"].ToString() == "" ? "0" : dt.Rows[0]["UNAdj"].ToString());
                            }
                        }
                        else if (item.Code == "Payable")
                        {
                            #region OLDCode
                            //List<string> list = new List<string>();
                            //if (item.Para1 == "Customer")
                            //{
                            //    list = ctxTFAT.CustomerMaster.Where(x => x.Hide == false && x.ARAP == true).Select(x => x.Code).ToList();
                            //}
                            //else
                            //{
                            //    list = ctxTFAT.Master.Where(x => x.Hide == false && x.ARAP == true).Select(x => x.Code).ToList();
                            //}
                            //string Acco = String.Join(",", list);
                            //OutstandingPayableReports(item.Para1 == "Customer" ? true : false, Acco, "01-Jan-1950", EndDate, item.Para10);
                            #endregion

                            #region TemporarySolution
                            GridOption gridOption = new GridOption();
                            gridOption.ViewDataId = "Payment Reminder Letter";
                            gridOption.Code = "Payable";
                            gridOption.MainType = "S";
                            gridOption.ARAPReqOnly = true;
                            gridOption.FromDate = (Convert.ToDateTime("01-Jan-1950")).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            gridOption.ToDate = (Convert.ToDateTime(EndDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            if (item.Para1 == "Customer")
                            {
                                gridOption.Customer = true;
                            }
                            ppara04 = item.Para10;
                            gridOption.Supress = true;
                            Createztmp_zOS(gridOption);
                            ExecuteStoredProc("select * into PayDztmp_zOS from ztmp_zOS");
                            #endregion



                            Query = "select  Cast(sum((Case when ODueDays between 0 and 30 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [0-30], Cast(sum((Case when ODueDays between 30+1  and 60 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [30-60], Cast(sum((Case when ODueDays between 60+1  and 90 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [60-90], Cast(sum((Case when ODueDays between 90+1  and 120 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [90-120], Cast(sum((Case when ODueDays between 120+1  and 150 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [120-150], Cast(Sum((Case when ODueDays between 150+1  and 180 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [150-180], Cast(sum((Case when ODueDays>180 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [>180],sum(Amt),sum(OnAccount ) As UNAdj,sum(Pending-OnAccount ) as TotalBal from PayDztmp_zOS";
                            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                            SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                            SqlDataAdapter sda = new SqlDataAdapter(cmd);
                            //if (dt.Rows.Count!=0)
                            {
                                sda.Fill(dt);
                            }
                            tfat_conx.Close();
                            tfat_conx.Dispose();
                            if (dt.Rows.Count > 0)
                            {
                                Model.PayOS30 = Convert.ToDecimal(dt.Rows[0]["0-30"].ToString() == "" ? "0" : dt.Rows[0]["0-30"].ToString());
                                Model.PayOS60 = Convert.ToDecimal(dt.Rows[0]["30-60"].ToString() == "" ? "0" : dt.Rows[0]["30-60"].ToString());
                                Model.PayOS90 = Convert.ToDecimal(dt.Rows[0]["60-90"].ToString() == "" ? "0" : dt.Rows[0]["60-90"].ToString());
                                Model.PayOS120 = Convert.ToDecimal(dt.Rows[0]["90-120"].ToString() == "" ? "0" : dt.Rows[0]["90-120"].ToString());
                                Model.PayOS150 = Convert.ToDecimal(dt.Rows[0]["120-150"].ToString() == "" ? "0" : dt.Rows[0]["120-150"].ToString());
                                Model.PayOS180 = Convert.ToDecimal(dt.Rows[0]["150-180"].ToString() == "" ? "0" : dt.Rows[0]["150-180"].ToString());
                                Model.PayOS180M = Convert.ToDecimal(dt.Rows[0][">180"].ToString() == "" ? "0" : dt.Rows[0][">180"].ToString());
                                Model.PayOSTotal = Convert.ToDecimal(dt.Rows[0]["TotalBal"].ToString() == "" ? "0" : dt.Rows[0]["TotalBal"].ToString());
                                Model.PayOSUnAdj = Convert.ToDecimal(dt.Rows[0]["UNAdj"].ToString() == "" ? "0" : dt.Rows[0]["UNAdj"].ToString());
                            }
                        }
                        else if (item.Code == "UnBillConsignmet")
                        {
                            UnbillConsignment(item.Para10);
                            Query = "select top 1" +
                            " (SELECT COUNT(*) FROM Dztmp_zUnBillLR ct    WHERE ct.oDueDays Between 0 and 30) as LR30," +
                            " (SELECT COUNT(*) FROM Dztmp_zUnBillLR ct WHERE ct.oDueDays Between 31 and 60) as LR60," +
                            " (SELECT COUNT(*) FROM Dztmp_zUnBillLR ct WHERE ct.oDueDays Between 61 and 90) as LR90," +
                            " (SELECT COUNT(*) FROM Dztmp_zUnBillLR ct WHERE ct.oDueDays Between 91 and 120) as LR120," +
                            " (SELECT COUNT(*) FROM Dztmp_zUnBillLR ct WHERE ct.oDueDays > 120) as LR120M," +
                            " (SELECT ISNULL(sum(amt),0) FROM Dztmp_zUnBillLR ct WHERE ct.oDueDays Between 0 and 30) as LR30Amt," +
                            " (SELECT ISNULL(sum(amt),0) FROM Dztmp_zUnBillLR ct WHERE ct.oDueDays Between 31 and 60) as LR60Amt," +
                            " (SELECT ISNULL(sum(amt),0) FROM Dztmp_zUnBillLR ct WHERE ct.oDueDays Between 61 and 90) as LR90Amt," +
                            " (SELECT ISNULL(sum(amt),0) FROM Dztmp_zUnBillLR ct WHERE ct.oDueDays Between 91 and 120) as LR120Amt," +
                            " (SELECT ISNULL(sum(amt),0) FROM Dztmp_zUnBillLR ct WHERE ct.oDueDays > 120) as LR120MAmt" +
                            " from Dztmp_zUnBillLR ";
                            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                            SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                            SqlDataAdapter sda = new SqlDataAdapter(cmd);
                            sda.Fill(dt);
                            tfat_conx.Close();
                            tfat_conx.Dispose();
                            if (dt.Rows.Count > 0)
                            {
                                Model.UnBillLr30 = Convert.ToInt32(dt.Rows[0]["LR30"].ToString() == "" ? "0" : dt.Rows[0]["LR30"].ToString());
                                Model.UnBillLr60 = Convert.ToInt32(dt.Rows[0]["LR60"].ToString() == "" ? "0" : dt.Rows[0]["LR60"].ToString());
                                Model.UnBillLr90 = Convert.ToInt32(dt.Rows[0]["LR90"].ToString() == "" ? "0" : dt.Rows[0]["LR90"].ToString());
                                Model.UnBillLr120 = Convert.ToInt32(dt.Rows[0]["LR120"].ToString() == "" ? "0" : dt.Rows[0]["LR120"].ToString());
                                Model.UnBillLr120M = Convert.ToInt32(dt.Rows[0]["LR120M"].ToString() == "" ? "0" : dt.Rows[0]["LR120M"].ToString());

                                Model.UnBillLrAmt30 = Convert.ToDecimal(dt.Rows[0]["LR30Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR30Amt"].ToString());
                                Model.UnBillLrAmt60 = Convert.ToDecimal(dt.Rows[0]["LR60Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR60Amt"].ToString());
                                Model.UnBillLrAmt90 = Convert.ToDecimal(dt.Rows[0]["LR90Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR90Amt"].ToString());
                                Model.UnBillLrAmt120 = Convert.ToDecimal(dt.Rows[0]["LR120Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR120Amt"].ToString());
                                Model.UnBillLrAmt120M = Convert.ToDecimal(dt.Rows[0]["LR120MAmt"].ToString() == "" ? "0" : dt.Rows[0]["LR120MAmt"].ToString());

                                Model.UnBillTotalConsignment = Model.UnBillLr30 + Model.UnBillLr60 + Model.UnBillLr90 + Model.UnBillLr120 + Model.UnBillLr120M;
                                Model.UnBillTotalAmtConsignment = Model.UnBillLrAmt30 + Model.UnBillLrAmt60 + Model.UnBillLrAmt90 + Model.UnBillLrAmt120 + Model.UnBillLrAmt120M;
                            }
                        }
                        else if (item.Code == "ConsignmetStock")
                        {
                            ConsignmentStock(item.Para10);
                            Query = "select top 1" +
                            " (SELECT COUNT(*) FROM Dztmp_zStockLR ct    WHERE ct.oDueDays Between 0 and 30) as LR30," +
                            " (SELECT COUNT(*) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 31 and 60) as LR60," +
                            " (SELECT COUNT(*) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 61 and 90) as LR90," +
                            " (SELECT COUNT(*) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 91 and 120) as LR120," +
                            " (SELECT COUNT(*) FROM Dztmp_zStockLR ct WHERE ct.oDueDays > 120) as LR120M," +
                            " (SELECT ISNULL(sum(BalQty),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 0 and 30) as LR30Amt," +
                            " (SELECT ISNULL(sum(BalQty),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 31 and 60) as LR60Amt," +
                            " (SELECT ISNULL(sum(BalQty),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 61 and 90) as LR90Amt," +
                            " (SELECT ISNULL(sum(BalQty),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 91 and 120) as LR120Amt," +
                            " (SELECT ISNULL(sum(BalQty),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays > 120) as LR120MAmt," +
                            " (SELECT ISNULL(sum(ChgWt),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 0 and 30) as LR30CH," +
                            " (SELECT ISNULL(sum(ChgWt),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 31 and 60) as LR60CH," +
                            " (SELECT ISNULL(sum(ChgWt),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 61 and 90) as LR90CH," +
                            " (SELECT ISNULL(sum(ChgWt),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 91 and 120) as LR120CH," +
                            " (SELECT ISNULL(sum(ChgWt),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays > 120) as LR120MCH," +
                            " (SELECT ISNULL(sum(DecVal),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 0 and 30) as LR30Declare," +
                            " (SELECT ISNULL(sum(DecVal),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 31 and 60) as LR60Declare," +
                            " (SELECT ISNULL(sum(DecVal),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 61 and 90) as LR90Declare," +
                            " (SELECT ISNULL(sum(DecVal),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 91 and 120) as LR120Declare," +
                            " (SELECT ISNULL(sum(DecVal),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays > 120) as LR120MDeclare" +
                            " from Dztmp_zStockLR ";
                            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                            SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                            SqlDataAdapter sda = new SqlDataAdapter(cmd);
                            sda.Fill(dt);
                            tfat_conx.Close();
                            tfat_conx.Dispose();
                            if (dt.Rows.Count > 0)
                            {
                                Model.StockLr30 = Convert.ToInt32(dt.Rows[0]["LR30"].ToString() == "" ? "0" : dt.Rows[0]["LR30"].ToString());
                                Model.StockLr60 = Convert.ToInt32(dt.Rows[0]["LR60"].ToString() == "" ? "0" : dt.Rows[0]["LR60"].ToString());
                                Model.StockLr90 = Convert.ToInt32(dt.Rows[0]["LR90"].ToString() == "" ? "0" : dt.Rows[0]["LR90"].ToString());
                                Model.StockLr120 = Convert.ToInt32(dt.Rows[0]["LR120"].ToString() == "" ? "0" : dt.Rows[0]["LR120"].ToString());
                                Model.StockLr120M = Convert.ToInt32(dt.Rows[0]["LR120M"].ToString() == "" ? "0" : dt.Rows[0]["LR120M"].ToString());

                                Model.StockLrQty30 = Convert.ToInt32(dt.Rows[0]["LR30Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR30Amt"].ToString());
                                Model.StockLrQty60 = Convert.ToInt32(dt.Rows[0]["LR60Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR60Amt"].ToString());
                                Model.StockLrQty90 = Convert.ToInt32(dt.Rows[0]["LR90Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR90Amt"].ToString());
                                Model.StockLrQty120 = Convert.ToInt32(dt.Rows[0]["LR120Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR120Amt"].ToString());
                                Model.StockLrQty120M = Convert.ToInt32(dt.Rows[0]["LR120MAmt"].ToString() == "" ? "0" : dt.Rows[0]["LR120MAmt"].ToString());

                                Model.StockLrWT30 = Convert.ToDecimal(dt.Rows[0]["LR30CH"].ToString() == "" ? "0" : dt.Rows[0]["LR30CH"].ToString());
                                Model.StockLrWT60 = Convert.ToDecimal(dt.Rows[0]["LR60CH"].ToString() == "" ? "0" : dt.Rows[0]["LR60CH"].ToString());
                                Model.StockLrWT90 = Convert.ToDecimal(dt.Rows[0]["LR90CH"].ToString() == "" ? "0" : dt.Rows[0]["LR90CH"].ToString());
                                Model.StockLrWT120 = Convert.ToDecimal(dt.Rows[0]["LR120CH"].ToString() == "" ? "0" : dt.Rows[0]["LR120CH"].ToString());
                                Model.StockLrWT120M = Convert.ToDecimal(dt.Rows[0]["LR120MCH"].ToString() == "" ? "0" : dt.Rows[0]["LR120MCH"].ToString());

                                Model.StockLrDeclar30 = Convert.ToDecimal(dt.Rows[0]["LR30Declare"].ToString() == "" ? "0" : dt.Rows[0]["LR30Declare"].ToString());
                                Model.StockLrDeclar60 = Convert.ToDecimal(dt.Rows[0]["LR60Declare"].ToString() == "" ? "0" : dt.Rows[0]["LR60Declare"].ToString());
                                Model.StockLrDeclar90 = Convert.ToDecimal(dt.Rows[0]["LR90Declare"].ToString() == "" ? "0" : dt.Rows[0]["LR90Declare"].ToString());
                                Model.StockLrDeclar120 = Convert.ToDecimal(dt.Rows[0]["LR120Declare"].ToString() == "" ? "0" : dt.Rows[0]["LR120Declare"].ToString());
                                Model.StockLrDeclar120M = Convert.ToDecimal(dt.Rows[0]["LR120MDeclare"].ToString() == "" ? "0" : dt.Rows[0]["LR120MDeclare"].ToString());

                                Model.StockTotalConsignment = Model.StockLr30 + Model.StockLr60 + Model.StockLr90 + Model.StockLr120 + Model.StockLr120M;
                                Model.StockTotalQtyConsignment = Model.StockLrQty30 + Model.StockLrQty60 + Model.StockLrQty90 + Model.StockLrQty120 + Model.StockLrQty120M;
                                Model.StockTotalWeightConsignment = Model.StockLrWT30 + Model.StockLrWT60 + Model.StockLrWT90 + Model.StockLrWT120 + Model.StockLrWT120M;
                                Model.StockTotalValueConsignment = Model.StockLrDeclar30 + Model.StockLrDeclar60 + Model.StockLrDeclar90 + Model.StockLrDeclar120 + Model.StockLrDeclar120M;
                            }
                        }
                        else if (item.Code == "ConsignmeTRNStock")
                        {
                            ConsignmentStockTRN(item.Para10);
                            Query = "select top 1" +
                            " (SELECT COUNT(*) FROM Dztmp_zStockLRTRN ct    WHERE ct.oDueDays Between 0 and 30) as LR30," +
                            " (SELECT COUNT(*) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 31 and 60) as LR60," +
                            " (SELECT COUNT(*) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 61 and 90) as LR90," +
                            " (SELECT COUNT(*) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 91 and 120) as LR120," +
                            " (SELECT COUNT(*) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays > 120) as LR120M," +
                            " (SELECT ISNULL(sum(BalQty),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 0 and 30) as LR30Amt," +
                            " (SELECT ISNULL(sum(BalQty),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 31 and 60) as LR60Amt," +
                            " (SELECT ISNULL(sum(BalQty),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 61 and 90) as LR90Amt," +
                            " (SELECT ISNULL(sum(BalQty),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 91 and 120) as LR120Amt," +
                            " (SELECT ISNULL(sum(BalQty),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays > 120) as LR120MAmt," +
                            " (SELECT ISNULL(sum(ChgWt),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 0 and 30) as LR30CH," +
                            " (SELECT ISNULL(sum(ChgWt),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 31 and 60) as LR60CH," +
                            " (SELECT ISNULL(sum(ChgWt),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 61 and 90) as LR90CH," +
                            " (SELECT ISNULL(sum(ChgWt),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 91 and 120) as LR120CH," +
                            " (SELECT ISNULL(sum(ChgWt),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays > 120) as LR120MCH," +
                            " (SELECT ISNULL(sum(DecVal),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 0 and 30) as LR30Declare," +
                            " (SELECT ISNULL(sum(DecVal),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 31 and 60) as LR60Declare," +
                            " (SELECT ISNULL(sum(DecVal),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 61 and 90) as LR90Declare," +
                            " (SELECT ISNULL(sum(DecVal),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 91 and 120) as LR120Declare," +
                            " (SELECT ISNULL(sum(DecVal),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays > 120) as LR120MDeclare" +
                            " from Dztmp_zStockLRTRN ";
                            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                            SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                            SqlDataAdapter sda = new SqlDataAdapter(cmd);
                            sda.Fill(dt);
                            tfat_conx.Close();
                            tfat_conx.Dispose();

                            if (dt.Rows.Count > 0)
                            {
                                Model.StockLr30TRN = Convert.ToInt32(dt.Rows[0]["LR30"].ToString() == "" ? "0" : dt.Rows[0]["LR30"].ToString());
                                Model.StockLr60TRN = Convert.ToInt32(dt.Rows[0]["LR60"].ToString() == "" ? "0" : dt.Rows[0]["LR60"].ToString());
                                Model.StockLr90TRN = Convert.ToInt32(dt.Rows[0]["LR90"].ToString() == "" ? "0" : dt.Rows[0]["LR90"].ToString());
                                Model.StockLr120TRN = Convert.ToInt32(dt.Rows[0]["LR120"].ToString() == "" ? "0" : dt.Rows[0]["LR120"].ToString());
                                Model.StockLr120MTRN = Convert.ToInt32(dt.Rows[0]["LR120M"].ToString() == "" ? "0" : dt.Rows[0]["LR120M"].ToString());

                                Model.StockLrQty30TRN = Convert.ToInt32(dt.Rows[0]["LR30Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR30Amt"].ToString());
                                Model.StockLrQty60TRN = Convert.ToInt32(dt.Rows[0]["LR60Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR60Amt"].ToString());
                                Model.StockLrQty90TRN = Convert.ToInt32(dt.Rows[0]["LR90Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR90Amt"].ToString());
                                Model.StockLrQty120TRN = Convert.ToInt32(dt.Rows[0]["LR120Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR120Amt"].ToString());
                                Model.StockLrQty120MTRN = Convert.ToInt32(dt.Rows[0]["LR120MAmt"].ToString() == "" ? "0" : dt.Rows[0]["LR120MAmt"].ToString());

                                Model.StockLrWT30TRN = Convert.ToDecimal(dt.Rows[0]["LR30CH"].ToString() == "" ? "0" : dt.Rows[0]["LR30CH"].ToString());
                                Model.StockLrWT60TRN = Convert.ToDecimal(dt.Rows[0]["LR60CH"].ToString() == "" ? "0" : dt.Rows[0]["LR60CH"].ToString());
                                Model.StockLrWT90TRN = Convert.ToDecimal(dt.Rows[0]["LR90CH"].ToString() == "" ? "0" : dt.Rows[0]["LR90CH"].ToString());
                                Model.StockLrWT120TRN = Convert.ToDecimal(dt.Rows[0]["LR120CH"].ToString() == "" ? "0" : dt.Rows[0]["LR120CH"].ToString());
                                Model.StockLrWT120MTRN = Convert.ToDecimal(dt.Rows[0]["LR120MCH"].ToString() == "" ? "0" : dt.Rows[0]["LR120MCH"].ToString());

                                Model.StockLrDeclar30TRN = Convert.ToDecimal(dt.Rows[0]["LR30Declare"].ToString() == "" ? "0" : dt.Rows[0]["LR30Declare"].ToString());
                                Model.StockLrDeclar60TRN = Convert.ToDecimal(dt.Rows[0]["LR60Declare"].ToString() == "" ? "0" : dt.Rows[0]["LR60Declare"].ToString());
                                Model.StockLrDeclar90TRN = Convert.ToDecimal(dt.Rows[0]["LR90Declare"].ToString() == "" ? "0" : dt.Rows[0]["LR90Declare"].ToString());
                                Model.StockLrDeclar120TRN = Convert.ToDecimal(dt.Rows[0]["LR120Declare"].ToString() == "" ? "0" : dt.Rows[0]["LR120Declare"].ToString());
                                Model.StockLrDeclar120MTRN = Convert.ToDecimal(dt.Rows[0]["LR120MDeclare"].ToString() == "" ? "0" : dt.Rows[0]["LR120MDeclare"].ToString());

                                Model.StockTotalConsignmentTRN = Model.StockLr30TRN + Model.StockLr60TRN + Model.StockLr90TRN + Model.StockLr120TRN + Model.StockLr120MTRN;
                                Model.StockTotalQtyConsignmentTRN = Model.StockLrQty30TRN + Model.StockLrQty60TRN + Model.StockLrQty90TRN + Model.StockLrQty120TRN + Model.StockLrQty120MTRN;
                                Model.StockTotalWeightConsignmentTRN = Model.StockLrWT30TRN + Model.StockLrWT60TRN + Model.StockLrWT90TRN + Model.StockLrWT120TRN + Model.StockLrWT120MTRN;
                                Model.StockTotalValueConsignmentTRN = Model.StockLrDeclar30TRN + Model.StockLrDeclar60TRN + Model.StockLrDeclar90TRN + Model.StockLrDeclar120TRN + Model.StockLrDeclar120MTRN;

                            }
                        }
                        else if (item.Code == "ConsignmetBook")
                        {
                            ConsignmentBooking(item.Para10, item.Para1, StartDate, EndDate);
                            Query = "select top 1" +
                                    " SUM(Aprl) as Aprl,SUM(May) as May,SUM(Jun) as Jun,SUM(Jul) as Jul,SUM(Aug) as Aug," +
                                    " SUM(Sept) as Sept,SUM(Oct) as Oct,SUM(Nov) as Nov,SUM(Dec) as Dec,SUM(Jan) as Jan," +
                                    " SUM(Feb) as Feb,SUM(Mar) as Mar" +
                                    " from Dztmp_zBookLR ";
                            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                            SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                            SqlDataAdapter sda = new SqlDataAdapter(cmd);
                            sda.Fill(dt);
                            tfat_conx.Close();
                            tfat_conx.Dispose();
                            if (dt.Rows.Count > 0)
                            {
                                Model.BKApr = Convert.ToDecimal(dt.Rows[0]["Aprl"].ToString() == "" ? "0" : dt.Rows[0]["Aprl"].ToString());
                                Model.BKMay = Convert.ToDecimal(dt.Rows[0]["May"].ToString() == "" ? "0" : dt.Rows[0]["May"].ToString());
                                Model.BKJun = Convert.ToDecimal(dt.Rows[0]["Jun"].ToString() == "" ? "0" : dt.Rows[0]["Jun"].ToString());
                                Model.BKJuy = Convert.ToDecimal(dt.Rows[0]["Jul"].ToString() == "" ? "0" : dt.Rows[0]["Jul"].ToString());
                                Model.BKAug = Convert.ToDecimal(dt.Rows[0]["Aug"].ToString() == "" ? "0" : dt.Rows[0]["Aug"].ToString());
                                Model.BKSept = Convert.ToDecimal(dt.Rows[0]["Sept"].ToString() == "" ? "0" : dt.Rows[0]["Sept"].ToString());
                                Model.BKOct = Convert.ToDecimal(dt.Rows[0]["Oct"].ToString() == "" ? "0" : dt.Rows[0]["Oct"].ToString());
                                Model.BKNov = Convert.ToDecimal(dt.Rows[0]["Nov"].ToString() == "" ? "0" : dt.Rows[0]["Nov"].ToString());
                                Model.BKDec = Convert.ToDecimal(dt.Rows[0]["Dec"].ToString() == "" ? "0" : dt.Rows[0]["Dec"].ToString());
                                Model.BKJan = Convert.ToDecimal(dt.Rows[0]["Jan"].ToString() == "" ? "0" : dt.Rows[0]["Jan"].ToString());
                                Model.BKFeb = Convert.ToDecimal(dt.Rows[0]["Feb"].ToString() == "" ? "0" : dt.Rows[0]["Feb"].ToString());
                                Model.BKMar = Convert.ToDecimal(dt.Rows[0]["Mar"].ToString() == "" ? "0" : dt.Rows[0]["Mar"].ToString());

                                Model.BKTotal = Model.BKApr + Model.BKMay + Model.BKJun + Model.BKJuy + Model.BKAug + Model.BKSept + Model.BKOct + Model.BKNov + Model.BKDec + Model.BKJan + Model.BKFeb + Model.BKMar;
                            }
                        }
                        else if (item.Code == "TopCustomers")
                        {
                            string FromDate = (Convert.ToDateTime(StartDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            string ToDate = (Convert.ToDateTime(EndDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

                            ExecuteStoredProc("Drop Table Top10Customer");
                            Query = "Select Top 10 CustomerMaster.Name,sum(Amt) as Amt into Top10Customer from Sales,CustomerMaster " +
                                    " where Charindex(Sales.branch, '" + item.Para10 + "') <> 0  And sales.DocDate >= '" + FromDate + "' and " +
                                    "Sales.DocDate <= '" + ToDate + "' And CustomerMaster.Code = Sales.Code and " +
                                    "Sales.Code is not null group by CustomerMaster.Name Order by sum(amt) desc";
                            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                            SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                            SqlDataAdapter sda = new SqlDataAdapter(cmd);
                            sda.Fill(dt);
                            tfat_conx.Close();
                            tfat_conx.Dispose();

                            dt = new DataTable();
                            Query = "Select Name,Amt from Top10Customer ";
                            tfat_conx = new SqlConnection(GetConnectionString());
                            cmd = new SqlCommand(Query, tfat_conx);
                            sda = new SqlDataAdapter(cmd);
                            sda.Fill(dt);
                            tfat_conx.Close();
                            tfat_conx.Dispose();

                            List<string> CustomerName = new List<string>();
                            List<decimal> CustomerAmount = new List<decimal>();

                            foreach (DataRow row in dt.Rows)
                            {
                                CustomerName.Add(row["Name"].ToString().ToUpper());
                                CustomerAmount.Add(Convert.ToDecimal(row["Amt"].ToString()));
                            }

                            Model.TopCustomerName = CustomerName;
                            Model.TopCustomerAmt = CustomerAmount;
                        }
                        else if (item.Code == "TopGroupCustomers")
                        {
                            string FromDate = (Convert.ToDateTime(StartDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            string ToDate = (Convert.ToDateTime(EndDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

                            ExecuteStoredProc("Drop Table Top10GroupCustomer");
                            Query = "Select Top 10 Master.Name,sum(Amt) as Amt into Top10GroupCustomer from Sales,Master " +
                                    " where Charindex(Sales.branch, '" + item.Para10 + "') <> 0  And sales.DocDate >= '" + FromDate + "' and " +
                                    "Sales.DocDate <= '" + ToDate + "' And Master.Code = Sales.CustGroup and " +
                                    "Sales.CustGroup is not null group by Master.Name Order by sum(amt) desc";
                            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                            SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                            SqlDataAdapter sda = new SqlDataAdapter(cmd);
                            sda.Fill(dt);
                            tfat_conx.Close();
                            tfat_conx.Dispose();

                            dt = new DataTable();
                            Query = "Select Name,Amt from Top10GroupCustomer ";
                            tfat_conx = new SqlConnection(GetConnectionString());
                            cmd = new SqlCommand(Query, tfat_conx);
                            sda = new SqlDataAdapter(cmd);
                            sda.Fill(dt);
                            tfat_conx.Close();
                            tfat_conx.Dispose();

                            List<string> CustomerName = new List<string>();
                            List<decimal> CustomerAmount = new List<decimal>();

                            foreach (DataRow row in dt.Rows)
                            {
                                CustomerName.Add(row["Name"].ToString().ToUpper());
                                CustomerAmount.Add(Convert.ToDecimal(row["Amt"].ToString()));
                            }

                            Model.TopGroupCustomerName = CustomerName;
                            Model.TopGroupCustomerAmt = CustomerAmount;
                        }
                        else if (item.Code == "TopVendors")
                        {
                            string FromDate = (Convert.ToDateTime(StartDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            string ToDate = (Convert.ToDateTime(EndDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

                            ExecuteStoredProc("Drop Table Top10Vendor");
                            Query = "Select Top 10 Master.Name,sum(Amt) as Amt into Top10Vendor from Purchase,Master " +
                                    " where Charindex(Purchase.branch, '" + item.Para10 + "') <> 0  And Purchase.BillDate >= '" + FromDate + "' and " +
                                    "Purchase.BillDate <= '" + ToDate + "' And Master.Code = Purchase.Code and " +
                                    "Purchase.Code is not null group by Master.Name Order by sum(amt) desc";
                            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                            SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                            SqlDataAdapter sda = new SqlDataAdapter(cmd);
                            sda.Fill(dt);
                            tfat_conx.Close();
                            tfat_conx.Dispose();

                            dt = new DataTable();
                            Query = "Select Name,Amt from Top10Vendor ";
                            tfat_conx = new SqlConnection(GetConnectionString());
                            cmd = new SqlCommand(Query, tfat_conx);
                            sda = new SqlDataAdapter(cmd);
                            sda.Fill(dt);
                            tfat_conx.Close();
                            tfat_conx.Dispose();

                            List<string> CustomerName = new List<string>();
                            List<decimal> CustomerAmount = new List<decimal>();

                            foreach (DataRow row in dt.Rows)
                            {
                                CustomerName.Add(row["Name"].ToString().ToUpper());
                                CustomerAmount.Add(Convert.ToDecimal(row["Amt"].ToString()));
                            }

                            Model.TopVendorsName = CustomerName;
                            Model.TopVendorsAmt = CustomerAmount;
                        }
                        else if (item.Code == "TopExpenses")
                        {

                            ExecuteStoredProc("Drop Table Top10Expenses");
                            Query = "select top 10 (select M.Name From Master M where M.Code=L.Code) as Name,sum(L.debit) as Amt " +
                                "into Top10Expenses from Ledger L where L.Code in ( select m.code from master m where m.grp in " +
                                "(select g.code from mastergroups g where name like '%Exp%')) and Charindex(L.Branch, '" + item.Para10 + "') <> 0 group by L.code " +
                                "order by sum(L.debit) desc ";
                            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                            SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                            SqlDataAdapter sda = new SqlDataAdapter(cmd);
                            sda.Fill(dt);
                            tfat_conx.Close();
                            tfat_conx.Dispose();

                            dt = new DataTable();
                            Query = "Select Name,Amt from Top10Expenses ";
                            tfat_conx = new SqlConnection(GetConnectionString());
                            cmd = new SqlCommand(Query, tfat_conx);
                            sda = new SqlDataAdapter(cmd);
                            sda.Fill(dt);
                            tfat_conx.Close();
                            tfat_conx.Dispose();

                            List<string> CustomerName = new List<string>();
                            List<decimal> CustomerAmount = new List<decimal>();

                            foreach (DataRow row in dt.Rows)
                            {
                                CustomerName.Add(row["Name"].ToString().ToUpper());
                                CustomerAmount.Add(Convert.ToDecimal(row["Amt"].ToString()));
                            }

                            Model.TopExpenseName = CustomerName;
                            Model.TopExpenseAmt = CustomerAmount;
                        }
                        else if (item.Code == "VehicleStatus")
                        {
                            var VehicleGroupStatus = "";
                            if (item.Para1 == "T")
                            {
                                VehicleGroupStatus += "100002,";
                            }
                            if (item.Para2 == "T")
                            {
                                VehicleGroupStatus += "100000,";
                            }
                            VehicleGroupStatus = VehicleGroupStatus.Substring(0, VehicleGroupStatus.Length - 1);
                            ExecuteStoredProc("Drop Table Ztmp_VehicleStatus1");
                            ExecuteStoredProc("select (select top 1 G.Status from tfatVehicleStatusHistory G where G.TruckNo= VehicleMaster.Code order by G.FromPeriod desc,G.FromTime desc) as Status into Ztmp_VehicleStatus1 from vehiclemaster where VehicleMaster.Acitve='true' and VehicleMaster.Code not in ('99999','99998') and Charindex(TruckStatus, '" + VehicleGroupStatus + "') <> 0 ");
                            ExecuteStoredProc("Drop Table Ztmp_VehicleStatus");

                            Query = "select Status,count(status) as Total " +
                                "into Ztmp_VehicleStatus from Ztmp_VehicleStatus1 " +
                                "group by Status";
                            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                            SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                            SqlDataAdapter sda = new SqlDataAdapter(cmd);
                            sda.Fill(dt);
                            tfat_conx.Close();
                            tfat_conx.Dispose();

                            dt = new DataTable();
                            Query = "Select Status,Total from Ztmp_VehicleStatus order by status";
                            tfat_conx = new SqlConnection(GetConnectionString());
                            cmd = new SqlCommand(Query, tfat_conx);
                            sda = new SqlDataAdapter(cmd);
                            sda.Fill(dt);
                            tfat_conx.Close();
                            tfat_conx.Dispose();

                            int ActiverVehicle = 0;
                            int CountVehicle = 0;
                            int Maintain = 0;
                            int NODRiver = 0;
                            int Ready = 0;
                            int Transit = 0;
                            int Accident = 0;
                            int Sale = 0;

                            foreach (DataRow row in dt.Rows)
                            {

                                var Value = row["Status"].ToString();
                                ActiverVehicle += ctxTFAT.VehicleMaster.Where(x => x.Status == Value && x.Code != "99999" && x.Code != "99998" && x.Acitve == true).ToList().Count();
                                if (row["Status"].ToString().Trim() == "Maintaince")
                                {
                                    Maintain = Convert.ToInt32(row["Total"].ToString());
                                }
                                else if (row["Status"].ToString().Trim() == "NODriver" || row["Status"].ToString().Trim() == "NULL" || row["Status"].ToString().Trim() == "")
                                {
                                    NODRiver += Convert.ToInt32(row["Total"].ToString());
                                }
                                else if (row["Status"].ToString().Trim() == "Ready")
                                {
                                    Ready = Convert.ToInt32(row["Total"].ToString());
                                }
                                else if (row["Status"].ToString().Trim() == "Transit")
                                {
                                    Transit = Convert.ToInt32(row["Total"].ToString());
                                }
                                else if (row["Status"].ToString().Trim() == "Accident")
                                {
                                    Accident = Convert.ToInt32(row["Total"].ToString());
                                }
                                else if (row["Status"].ToString().Trim() == "Sale")
                                {
                                    Sale = Convert.ToInt32(row["Total"].ToString());
                                }
                            }

                            Model.VActiveVehicle = ActiverVehicle;
                            Model.VCountVehicle = ctxTFAT.VehicleMaster.Where(x => x.Code != "99999" && x.Code != "99998").ToList().Count();
                            Model.VMaintain = Maintain;
                            Model.VNoDriver = NODRiver;
                            Model.VReady = Ready;
                            Model.VTransit = Transit;
                            Model.VAccident = Accident;
                            Model.VSale = Sale;
                        }
                        else if (item.Code == "DriverStatus")
                        {
                            ExecuteStoredProc("Drop Table Ztmp_DriverStatus");
                            ExecuteStoredProc("select (select top 1  G.Vehicle  from TfatDriverStatus G where G.Driver= DriverMaster.Code order by G.DocNo desc) as Status into Ztmp_DriverStatus from DriverMaster where DriverMaster.Status='true' and DriverMaster.Code not in ('99999')");
                            ExecuteStoredProc("Drop Table Ztmp_DriverStatus1");
                            Query = "select (select V.Truckno from vehiclemaster V where V.code=D.Status) as Status,count(*) as Total into Ztmp_DriverStatus1 from Ztmp_DriverStatus D group by D.Status";
                            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                            SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                            SqlDataAdapter sda = new SqlDataAdapter(cmd);
                            sda.Fill(dt);
                            tfat_conx.Close();
                            tfat_conx.Dispose();

                            int ActiverDrivers = ctxTFAT.DriverMaster.Where(x => x.Code != "99999" && x.Status == true).ToList().Count();

                            dt = new DataTable();
                            Query = "Select Status,Total from Ztmp_DriverStatus1 ";
                            tfat_conx = new SqlConnection(GetConnectionString());
                            cmd = new SqlCommand(Query, tfat_conx);
                            sda = new SqlDataAdapter(cmd);
                            sda.Fill(dt);
                            tfat_conx.Close();
                            tfat_conx.Dispose();

                            int OnVehicleDriver = 0;
                            int bhatta = 0;
                            int novehicle = 0;

                            foreach (DataRow row in dt.Rows)
                            {
                                if (row["Status"].ToString().Trim() == "On Bhatta")
                                {
                                    bhatta += Convert.ToInt32(row["Total"].ToString());
                                }
                                else if (row["Status"].ToString().Trim() == "No Vehicle" || row["Status"].ToString().Trim() == "NULL" || row["Status"].ToString().Trim() == "")
                                {
                                    novehicle += Convert.ToInt32(row["Total"].ToString());
                                }
                                else
                                {
                                    OnVehicleDriver += Convert.ToInt32(row["Total"].ToString());
                                }
                            }
                            Model.ActiverDrivers = ActiverDrivers;
                            Model.DriverCount = ctxTFAT.DriverMaster.Where(x => x.Code != "99999").ToList().Count();
                            Model.Bhatta = bhatta;
                            Model.NOVehicle = novehicle;
                            Model.ONVehicle = OnVehicleDriver;
                        }
                        else if (item.Code == "VehicleLocation")
                        {
                            Model.DefaultMap = false;
                            var DbVehicelCode = item.Para1.Split(',').ToList();
                            var TrackVehicleList = ctxTFAT.VehicleMaster.Where(x => DbVehicelCode.Contains(x.Code)).Select(x => x.TruckNo.Replace(" ", "").Replace(" ", "").Replace(" ", "").ToUpper()).ToList();

                            //TrackVehicleList.ToList().ForEach(s => s = s.ToUpper());
                            List<VehicleTrackinModel> trackinModels = new List<VehicleTrackinModel>();
                            var SetUrl = "http://speed.elixiatech.com/modules/api/vts/api.php?action=getVehicleDataV1&jsonreq={'userkey':'c8b8ec3f3e0dca3c823fbda9ce93ed3a74684bc9','searchstring':'GJ','pageindex':1,'pagesize':10,'isLocationEnabled':1}";
                            SetUrl = SetUrl.Replace("'", "\"");
                            WebClient client = new WebClient();
                            string jsonstring = client.DownloadString(SetUrl);
                            dynamic dynObj = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonstring);
                            if (dynObj != null)
                            {
                                var status = dynObj.Status;
                                if (status.Value == "1")
                                {
                                    foreach (var member in dynObj.Result.data)
                                    {
                                        string VehicleNo = member["vehicleno"];
                                        VehicleNo = Regex.Replace(VehicleNo, @"\s+", "");
                                        VehicleNo = Regex.Replace(VehicleNo, @"\s+", "");
                                        string VehicleNo1 = Regex.Replace(VehicleNo, @"\s+", "");
                                        if (TrackVehicleList.Contains(VehicleNo1.ToUpper()))
                                        {
                                            VehicleTrackinModel vehicleTrackin = new VehicleTrackinModel();
                                            vehicleTrackin.title = member["vehicleno"];
                                            vehicleTrackin.description = member["vehicleno"];
                                            vehicleTrackin.lat = member["lat"];
                                            vehicleTrackin.lng = member["lng"];
                                            trackinModels.Add(vehicleTrackin);
                                        }
                                    }
                                }
                            }


                            SetUrl = "http://api.ilogistek.com:6049/tracking?AuthKey=PV7q60SurjHlacX398hbQ4SiWfYktoL&vehicle_no=all";
                            client = new WebClient();
                            jsonstring = client.DownloadString(SetUrl);
                            dynamic dynObj1 = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonstring);
                            foreach (var member in dynObj1)
                            {
                                string VehicleNo = member["VehicleNo"];
                                VehicleNo = Regex.Replace(VehicleNo, @"\s+", "");
                                VehicleNo = Regex.Replace(VehicleNo, @"\s+", "");
                                string VehicleNo1 = Regex.Replace(VehicleNo, @"\s+", "");
                                if (TrackVehicleList.Contains(VehicleNo1.ToUpper()))
                                {
                                    VehicleTrackinModel vehicleTrackin = new VehicleTrackinModel();
                                    vehicleTrackin.title = member["VehicleNo"];
                                    vehicleTrackin.description = member["VehicleNo"];
                                    vehicleTrackin.lat = member["Latitude"];
                                    vehicleTrackin.lng = member["Longitude"];
                                    trackinModels.Add(vehicleTrackin);
                                }
                            }
                            trackinModels.Add(new VehicleTrackinModel
                            {
                                title = "All Vehicles"
                            });
                            Model.VehicleTrackList = trackinModels;
                            Model.CountVehicle = (trackinModels.Count() - 1).ToString();
                            trackinModels = new List<VehicleTrackinModel>();
                            var list = ctxTFAT.TfatBranchLocation.Where(x => String.IsNullOrEmpty(x.Location) == false && String.IsNullOrEmpty(x.Title) == false).ToList();
                            Model.CountBranch = (list.Count()).ToString();
                            Model.BranchReq = true;
                            foreach (var branch in list)
                            {
                                var Location = branch.Location.Split(',');
                                VehicleTrackinModel vehicleTrackin = new VehicleTrackinModel();
                                vehicleTrackin.title = branch.Title;
                                vehicleTrackin.description = "Branch";
                                //vehicleTrackin.description = branch.Title;
                                vehicleTrackin.lat = Convert.ToDouble(Location[0]);
                                vehicleTrackin.lng = Convert.ToDouble(Location[1]);
                                trackinModels.Add(vehicleTrackin);
                            }
                            Model.BranchTrackList = trackinModels;
                        }
                        else if (item.Code == "DriverTripBalance")
                        {
                            var DriverGrp = ctxTFAT.Master.Where(x => x.OthPostType.Contains("D")).Select(x => x.Grp).FirstOrDefault();
                            GenerateGrpWithBalance(StartDate + ":" + EndDate, "", "TBL", Convert.ToInt32("0"), "", false, 0, true);
                            SqlConnection conTFAT = new SqlConnection(GetConnectionString());
                            SqlCommand cmd = new SqlCommand("dbo.SPTFAT_GetAccountSchedule", conTFAT);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandTimeout = 120;
                            cmd.Parameters.Add("@mGroup", SqlDbType.VarChar).Value = DriverGrp;
                            conTFAT.Open();
                            cmd.CommandTimeout = 0;
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                            conTFAT.Dispose();

                            //Driver Bal Total
                            dt = new DataTable();
                            Query = "with Dr as ( select sum(bal) as Bal from ztmp_temp ) select case when Bal>0 then cast(Bal as varchar)+' DR' else cast(Bal as varchar)+' CR' end as Bal from Dr";
                            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                            cmd = new SqlCommand(Query, tfat_conx);
                            SqlDataAdapter sda = new SqlDataAdapter(cmd);
                            sda.Fill(dt);
                            tfat_conx.Close();
                            tfat_conx.Dispose();
                            if (dt.Rows.Count > 0)
                            {
                                Model.DriverBalTotal = dt.Rows[0]["Bal"].ToString();
                            }

                            dt = new DataTable();
                            ExecuteStoredProc("Drop Table Ztmp_DriverTripBalance");
                            Query = "WITH ranked_messages AS ( select D.Name, ROW_NUMBER() OVER(PARTITION BY T.Driver ORDER BY T.TODT DESC) AS rn, Convert(char(10), T.TODT, 103) as LastDate,case when  Z.Bal > 0 then cast(Z.Bal as varchar) + ' DR' else cast(((Z.Bal) * (-1)) as varchar) + ' CR' end as BAL  from drivermaster D left join TripSheetMaster T on T.Driver = D.Code join ztmp_temp Z on Z.Code = D.Posting where Charindex(D.Code, '" + item.Para1 + "') <> 0 ) " +
                                "SELECT* into Ztmp_DriverTripBalance FROM ranked_messages WHERE rn = 1 order by Name";
                            tfat_conx = new SqlConnection(GetConnectionString());
                            cmd = new SqlCommand(Query, tfat_conx);
                            sda = new SqlDataAdapter(cmd);
                            sda.Fill(dt);
                            tfat_conx.Close();
                            tfat_conx.Dispose();

                            dt = new DataTable();
                            Query = "Select Name,LastDate,BAL from Ztmp_DriverTripBalance order by Name";
                            tfat_conx = new SqlConnection(GetConnectionString());
                            cmd = new SqlCommand(Query, tfat_conx);
                            sda = new SqlDataAdapter(cmd);
                            sda.Fill(dt);
                            tfat_conx.Close();
                            tfat_conx.Dispose();



                            List<string> DriverName = new List<string>();
                            List<string> DriverLastTrip = new List<string>();
                            List<string> DriverBal = new List<string>();

                            foreach (DataRow row in dt.Rows)
                            {
                                DriverName.Add(row["Name"].ToString().ToUpper());
                                DriverLastTrip.Add(row["LastDate"].ToString());
                                DriverBal.Add(row["BAL"].ToString());
                            }

                            Model.DriverName = DriverName;
                            Model.DriverLastTripDate = DriverLastTrip;
                            Model.DriverBal = DriverBal;
                        }
                        else if (item.Code == "VehicleTripDetails")
                        {
                            Query = "select D.TruckNo as Name," +
                                " (select Top 1 isnull( V.KM,0) from VehicleKmMaintainMa V where V.VehicleNo=D.Code order by V.Date desc) as LastKM," +
                                "(select top 1 Convert(char(10), T.TODT, 103) from tripsheetmaster T where T.Docno in (select TF.DocNo from TripFmList TF where TF.VehicleNo=D.Code) order by T.TODT desc ) as LastDate" +
                                " from vehiclemaster D where Charindex(D.Code, '" + item.Para1 + "') <> 0  order by D.TruckNo";

                            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                            SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                            SqlDataAdapter sda = new SqlDataAdapter(cmd);
                            sda.Fill(dt);
                            tfat_conx.Close();
                            tfat_conx.Dispose();


                            List<string> VehicleName = new List<string>();
                            List<string> VehicleLastTrip = new List<string>();
                            List<string> VehicleKM = new List<string>();

                            foreach (DataRow row in dt.Rows)
                            {
                                VehicleName.Add(row["Name"].ToString().ToUpper());
                                VehicleLastTrip.Add(row["LastDate"].ToString());
                                VehicleKM.Add(row["LastKM"].ToString());
                            }

                            Model.VehicleName = VehicleName;
                            Model.VehicleLastTripDate = VehicleLastTrip;
                            Model.VehicleKM = VehicleKM;
                        }
                        else if (item.Code == "VehicleExpDue")
                        {
                            ExecuteStoredProc("Drop Table Ztmp_VehicleExpDue2");
                            Query = "select R.Value8 as Vehicle,R.Code as ExpensesAc,R.date2 as TODT,R.Status as Status,R.Clear as Clear " +
                                    "into Ztmp_VehicleExpDue2 from RelateData R " +
                                    " where Charindex(R.value8, '" + item.Para1 + "') <> 0 " +
                                    " and  Charindex(R.Code, '" + item.Para2 + "') <> 0 ";
                            ExecuteStoredProc(Query);
                            Query = " insert into Ztmp_VehicleExpDue2" +
                                    " select R.code as Vehicle,R.combo1 as ExpensesAc,R.date2 as TODT,R.Status as Status,R.Clear as Clear " +
                                    " from RelateData R " +
                                    " where Charindex(R.Code,  '" + item.Para1 + "')  <> 0 " +
                                    " and  Charindex(R.combo1, '" + item.Para2 + "') <> 0 ";
                            ExecuteStoredProc(Query);

                            ExecuteStoredProc("Drop Table Ztmp_VehicleExpDue1");
                            Query = " WITH ranked_messages AS ( " +
                                        " SELECT m.*, ROW_NUMBER() OVER(PARTITION BY Vehicle, ExpensesAc ORDER BY Todt DESC ) AS rn  " +
                                        " FROM Ztmp_VehicleExpDue2 AS m )	" +
                                    " SELECT* into Ztmp_VehicleExpDue1 FROM ranked_messages WHERE rn = 1; ";
                            ExecuteStoredProc(Query);

                            var Todays = (Convert.ToDateTime(DateTime.Now.ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            ExecuteStoredProc("Drop Table Ztmp_VehicleExpDue");
                            Query = " select (select M.Name From Master M Where M.Code=ExpensesAc) as ExpensesAc," +
                                " Sum(Case when TODT<DateAdd(D,0,'" + Todays + "') then 1 Else 0 End) as Todays, " +
                                " Sum(Case when TODT<DateAdd(D,5,'" + Todays + "') then 1 Else 0 End) as FiveDay, " +
                                " Sum(Case when TODT<DateAdd(D,15,'" + Todays + "') then 1 Else 0 End) as FifteenDay, " +
                                " Sum(Case when TODT<DateAdd(D,30,'" + Todays + "') then 1 Else 0 End) as ThirtyDay " +
                                " into Ztmp_VehicleExpDue from Ztmp_VehicleExpDue1 where Clear='false' and Charindex(ExpensesAc, '" + item.Para2 + "') <> 0 " +
                                " group by ExpensesAc " +
                                " order by ExpensesAc ";
                            ExecuteStoredProc(Query);

                            dt = new DataTable();
                            Query = "Select ExpensesAc,Todays,FiveDay,FifteenDay,ThirtyDay from Ztmp_VehicleExpDue   ";
                            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                            SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                            SqlDataAdapter sda = new SqlDataAdapter(cmd);
                            sda.Fill(dt);
                            tfat_conx.Close();
                            tfat_conx.Dispose();


                            List<string> VehicleExpName = new List<string>();
                            List<string> VehicleExpName0 = new List<string>();
                            List<string> VehicleExpName5 = new List<string>();
                            List<string> VehicleExpName15 = new List<string>();
                            List<string> VehicleExpName30 = new List<string>();

                            foreach (DataRow row in dt.Rows)
                            {
                                VehicleExpName.Add(row["ExpensesAc"].ToString().ToUpper());
                                VehicleExpName0.Add(row["Todays"].ToString());
                                VehicleExpName5.Add(row["FiveDay"].ToString());
                                VehicleExpName15.Add(row["FifteenDay"].ToString());
                                VehicleExpName30.Add(row["ThirtyDay"].ToString());
                            }

                            Model.VehicleExpName = VehicleExpName;
                            Model.VehicleExpName0 = VehicleExpName0;
                            Model.VehicleExpName5 = VehicleExpName5;
                            Model.VehicleExpName15 = VehicleExpName15;
                            Model.VehicleExpName30 = VehicleExpName30;
                        }
                        else if (item.Code == "EwayBillDetails")
                        {
                            var Currdate = (Convert.ToDateTime(DateTime.Now.ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            var Tomorrowdate = (Convert.ToDateTime(DateTime.Now.AddDays(1).ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            var AfterTomorrowdate = (Convert.ToDateTime(DateTime.Now.AddDays(2).ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            ExecuteStoredProc("Drop Table Ztmp_EwayBillDetails");
                            ExecuteStoredProc("Drop Table Ztmp_EwayBillDetails2");
                            ExecuteStoredProc("Drop Table Ztmp_EwayBillDetails3");

                            Query = " select s.EWBNO as EwayBilNo, s.EWBValid as ValidUpto,  " +
                                    "(case when s.LrTablekey is null then '' else  ( case when(select T.Category From TfatBranch T where T.code = LRSTK.Branch) = 'Area' then(select T.Grp  From TfatBranch T where T.code = LRSTK.Branch) else (LRSTK.Branch)end )    end )as StockBranchCode,  " +
                                    "LRSTK.recordkey as Stockkey, case when s.LrTablekey is null then '' else LRSTK.Type end as StockType,  " +
                                    " case when LRSTK.TotalQty = 0 then 0 else (LRSTK.TotalQty - (((select  ISNULL(SUM(LRT.TotalQty), 0) from LRStock LRT where LRT.ParentKey = LRSTK.TableKey)) +(select ISNULL(SUM(lcd.lrqty), 0) from lcdetail lcd where lcd.Parentkey = LRSTK.Tablekey and lcd.LCRefTablekey in (select LCM.Tablekey from lcmaster LCM where LCm.DispachFM = 0))  ) ) end as StockQty  " +
                                    " into Ztmp_EwayBillDetails3 from tfatEWB s left join lrmaster R on s.LrTablekey = R.Tablekey left join LRStock LRSTK on LRSTK.LRRefTablekey = R.TableKey " +
                                    "  where (s.Doctype = 'LR000' and s.Clear = 'false' and(s.EWBValid is null or s.EWBValid >= '" + Currdate + "')  ) and(s.lrtablekey is null or((case when LRSTK.TotalQty = 0 then 0 else (LRSTK.TotalQty - (((select  ISNULL(SUM(LRT.TotalQty), 0) from LRStock LRT where LRT.ParentKey = LRSTK.TableKey)) + (select ISNULL(SUM(lcd.lrqty), 0) from lcdetail lcd where lcd.Parentkey = LRSTK.Tablekey and lcd.LCRefTablekey in (select LCM.Tablekey from lcmaster LCM where LCm.DispachFM = 0))  ) ) end ) <> 0 )) " +
                                    " Order by s.DocDate  ";
                            ExecuteStoredProc(Query);
                            Query = " WITH ranked_messages AS ( " +
                                    "SELECT m.*, ROW_NUMBER() OVER(PARTITION BY EwayBilNo ORDER BY Stockkey desc) AS rn  " +
                                    "FROM Ztmp_EwayBillDetails3 AS m ) SELECT* into Ztmp_EwayBillDetails2 FROM ranked_messages Z where rn = 1 ";
                            ExecuteStoredProc(Query);

                            ExecuteStoredProc("delete from Ztmp_EwayBillDetails2 where StockType='DEL'");
                            if (item.Para12 == "T")
                            {
                                ExecuteStoredProc("delete from Ztmp_EwayBillDetails2 where Stockkey is not null and  charindex(StockBranchCode,'" + item.Para10 + "')=0");
                            }
                            else
                            {
                                ExecuteStoredProc("delete from Ztmp_EwayBillDetails2 where  charindex(StockBranchCode,'" + item.Para10 + "')=0");
                            }
                            Query = "select count(*) as Active, " +
                                    "(select Count(*) from Ztmp_EwayBillDetails2 R where R.ValidUpto = '" + Currdate + "') as TodayExp, " +
                                    "(select Count(*) from Ztmp_EwayBillDetails2 R where R.ValidUpto = '" + Tomorrowdate + "') as TomorrowExp, " +
                                    "(select Count(*) from Ztmp_EwayBillDetails2 R where R.ValidUpto >= '" + AfterTomorrowdate + "') as Expird " +
                                    "  into Ztmp_EwayBillDetails from Ztmp_EwayBillDetails2";
                            ExecuteStoredProc(Query);

                            dt = new DataTable();
                            Query = "Select * from Ztmp_EwayBillDetails   ";
                            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                            SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                            SqlDataAdapter sda = new SqlDataAdapter(cmd);
                            sda.Fill(dt);
                            tfat_conx.Close();
                            tfat_conx.Dispose();


                            List<string> EWBActive = new List<string>();
                            List<string> EWBActiveToday = new List<string>();
                            List<string> EWBActiveTomorrow = new List<string>();
                            List<string> EWBActiveExpired = new List<string>();

                            foreach (DataRow row in dt.Rows)
                            {
                                EWBActive.Add(row["Active"].ToString());
                                EWBActiveToday.Add(row["TodayExp"].ToString());
                                EWBActiveTomorrow.Add(row["TomorrowExp"].ToString());
                                EWBActiveExpired.Add(row["Expird"].ToString());
                            }

                            Model.EWBActive = EWBActive;
                            Model.EWBActiveToday = EWBActiveToday;
                            Model.EWBActiveTomorrow = EWBActiveTomorrow;
                            Model.EWBActiveExpired = EWBActiveExpired;
                        }
                        lmod.Add(objectsVM);
                    }

                    ViewBag.DiaGram = mStr1;
                }
                Model.codes = lmod;
            }

            if (Model != null && Model.DefaultMap)
            {
                List<VehicleTrackinModel> trackinModels = new List<VehicleTrackinModel>();
                var list = ctxTFAT.TfatBranchLocation.Where(x => String.IsNullOrEmpty(x.Location) == false && String.IsNullOrEmpty(x.Title) == false).ToList();
                Model.CountBranch = (list.Count()).ToString();
                Model.BranchReq = true;
                foreach (var branch in list)
                {
                    var Location = branch.Location.Split(',');
                    VehicleTrackinModel vehicleTrackin = new VehicleTrackinModel();
                    vehicleTrackin.title = branch.Title;
                    vehicleTrackin.description = "Branch";
                    //vehicleTrackin.description = branch.Title;
                    vehicleTrackin.lat = Convert.ToDouble(Location[0]);
                    vehicleTrackin.lng = Convert.ToDouble(Location[1]);
                    trackinModels.Add(vehicleTrackin);
                }
                Model.BranchTrackList = trackinModels;
            }
            if (Model != null && Model.codes != null)
            {
                foreach (var item in Model.codes)
                {
                    mStr += item.Code + "|";
                }
            }



            if (Model == null)
            {
                Model = new ActiveObjectsVM();
            }
            Session["SideMenuModel"] = Model;


            ViewBag.Code = mStr;

            var html = ViewHelper.RenderPartialView(this, "_SideMenuDesign", Model);
            return Json(new
            {
                Html = html,
                JsonRequestBehavior.AllowGet
            });
        }
        public ActionResult ReloadSideMenu(string Code)
        {
            ViewBag.Code = "";
            string mStr = "";
            DataTable dt = new DataTable();
            string Query = "";
            ActiveObjectsVM Model = Session["SideMenuModel"] as ActiveObjectsVM;
            #region GetOnly Particular SideMenu List
            DataTable Record = new DataTable();
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string sql = string.Format(@"Select ID from ActiveSideBarObjects where '" + muserid + "' in (SELECT value FROM STRING_SPLIT(ActiveSideBarObjects.Users, ','))");
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                da.Fill(Record);
            }
            var Recordlst = Record.AsEnumerable()
                         .Select(r => r.Table.Columns.Cast<DataColumn>()
                         .Select(c => new KeyValuePair<string, object>(c.ColumnName, r[c.Ordinal])
                      ).ToDictionary(z => z.Key, z => z.Value)
                   ).ToList();

            List<string> IDList = new List<string>();
            foreach (var item in Recordlst)
            {
                IDList.Add(item["ID"].ToString());
            }
            #endregion

            var result = ctxTFAT.ActiveSideBarObjects.Where(x => IDList.Contains(x.ID) && x.Status == true && x.Code.Trim().ToLower() == Code.Trim().ToLower()).FirstOrDefault();

            if (result != null)
            {
                if (Code == "Outstanding")
                {
                    OutstandingReports(result.Para1 == "Customer" ? true : false, result.Para11, "01-Jan-1950", EndDate, result.Para10);
                    Query = "select  Cast(sum((Case when ODueDays between 0 and 30 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [0-30], Cast(sum((Case when ODueDays between 30+1  and 60 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [30-60], Cast(sum((Case when ODueDays between 60+1  and 90 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [60-90], Cast(sum((Case when ODueDays between 90+1  and 120 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [90-120], Cast(sum((Case when ODueDays between 120+1  and 150 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [120-150], Cast(Sum((Case when ODueDays between 150+1  and 180 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [150-180], Cast(sum((Case when ODueDays>180 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [>180],sum(Amt),sum(OnAccount ) As UNAdj,sum(Pending-OnAccount ) as TotalBal from Dztmp_zOS";
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();

                    if (dt.Rows.Count > 0)
                    {
                        Model.OS30 = Convert.ToDecimal(dt.Rows[0]["0-30"].ToString() == "" ? "0" : dt.Rows[0]["0-30"].ToString());
                        Model.OS60 = Convert.ToDecimal(dt.Rows[0]["30-60"].ToString() == "" ? "0" : dt.Rows[0]["30-60"].ToString());
                        Model.OS90 = Convert.ToDecimal(dt.Rows[0]["60-90"].ToString() == "" ? "0" : dt.Rows[0]["60-90"].ToString());
                        Model.OS120 = Convert.ToDecimal(dt.Rows[0]["90-120"].ToString() == "" ? "0" : dt.Rows[0]["90-120"].ToString());
                        Model.OS150 = Convert.ToDecimal(dt.Rows[0]["120-150"].ToString() == "" ? "0" : dt.Rows[0]["120-150"].ToString());
                        Model.OS180 = Convert.ToDecimal(dt.Rows[0]["150-180"].ToString() == "" ? "0" : dt.Rows[0]["150-180"].ToString());
                        Model.OS180M = Convert.ToDecimal(dt.Rows[0][">180"].ToString() == "" ? "0" : dt.Rows[0][">180"].ToString());
                        Model.OSTotal = Convert.ToDecimal(dt.Rows[0]["TotalBal"].ToString() == "" ? "0" : dt.Rows[0]["TotalBal"].ToString());
                        Model.OSUnAdj = Convert.ToDecimal(dt.Rows[0]["UNAdj"].ToString() == "" ? "0" : dt.Rows[0]["UNAdj"].ToString());
                    }
                }
                else if (Code == "Payable")
                {
                    List<string> list = new List<string>();
                    if (result.Para1 == "Customer")
                    {
                        list = ctxTFAT.CustomerMaster.Where(x => x.Hide == false && x.ARAP == true).Select(x => x.Code).ToList();
                    }
                    else
                    {
                        list = ctxTFAT.Master.Where(x => x.Hide == false && x.ARAP == true).Select(x => x.Code).ToList();
                    }
                    string Acco = String.Join(",", list);
                    OutstandingPayableReports(result.Para1 == "Customer" ? true : false, Acco, "01-Jan-1950", EndDate, result.Para10);
                    Query = "select  Cast(sum((Case when ODueDays between 0 and 30 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [0-30], Cast(sum((Case when ODueDays between 30+1  and 60 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [30-60], Cast(sum((Case when ODueDays between 60+1  and 90 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [60-90], Cast(sum((Case when ODueDays between 90+1  and 120 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [90-120], Cast(sum((Case when ODueDays between 120+1  and 150 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [120-150], Cast(Sum((Case when ODueDays between 150+1  and 180 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [150-180], Cast(sum((Case when ODueDays>180 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [>180],sum(Amt),sum(OnAccount ) As UNAdj,sum(Pending-OnAccount ) as TotalBal from PayDztmp_zOS";
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();

                    if (dt.Rows.Count > 0)
                    {
                        Model.PayOS30 = Convert.ToDecimal(dt.Rows[0]["0-30"].ToString() == "" ? "0" : dt.Rows[0]["0-30"].ToString());
                        Model.PayOS60 = Convert.ToDecimal(dt.Rows[0]["30-60"].ToString() == "" ? "0" : dt.Rows[0]["30-60"].ToString());
                        Model.PayOS90 = Convert.ToDecimal(dt.Rows[0]["60-90"].ToString() == "" ? "0" : dt.Rows[0]["60-90"].ToString());
                        Model.PayOS120 = Convert.ToDecimal(dt.Rows[0]["90-120"].ToString() == "" ? "0" : dt.Rows[0]["90-120"].ToString());
                        Model.PayOS150 = Convert.ToDecimal(dt.Rows[0]["120-150"].ToString() == "" ? "0" : dt.Rows[0]["120-150"].ToString());
                        Model.PayOS180 = Convert.ToDecimal(dt.Rows[0]["150-180"].ToString() == "" ? "0" : dt.Rows[0]["150-180"].ToString());
                        Model.PayOS180M = Convert.ToDecimal(dt.Rows[0][">180"].ToString() == "" ? "0" : dt.Rows[0][">180"].ToString());
                        Model.PayOSTotal = Convert.ToDecimal(dt.Rows[0]["TotalBal"].ToString() == "" ? "0" : dt.Rows[0]["TotalBal"].ToString());
                        Model.PayOSUnAdj = Convert.ToDecimal(dt.Rows[0]["UNAdj"].ToString() == "" ? "0" : dt.Rows[0]["UNAdj"].ToString());
                    }
                }
                else if (Code == "UnBillConsignmet")
                {
                    UnbillConsignment(result.Para10);
                    Query = "select top 1" +
                    " (SELECT COUNT(*) FROM Dztmp_zUnBillLR ct    WHERE ct.oDueDays Between 0 and 30) as LR30," +
                    " (SELECT COUNT(*) FROM Dztmp_zUnBillLR ct WHERE ct.oDueDays Between 31 and 60) as LR60," +
                    " (SELECT COUNT(*) FROM Dztmp_zUnBillLR ct WHERE ct.oDueDays Between 61 and 90) as LR90," +
                    " (SELECT COUNT(*) FROM Dztmp_zUnBillLR ct WHERE ct.oDueDays Between 91 and 120) as LR120," +
                    " (SELECT COUNT(*) FROM Dztmp_zUnBillLR ct WHERE ct.oDueDays > 120) as LR120M," +
                    " (SELECT ISNULL(sum(amt),0) FROM Dztmp_zUnBillLR ct WHERE ct.oDueDays Between 0 and 30) as LR30Amt," +
                    " (SELECT ISNULL(sum(amt),0) FROM Dztmp_zUnBillLR ct WHERE ct.oDueDays Between 31 and 60) as LR60Amt," +
                    " (SELECT ISNULL(sum(amt),0) FROM Dztmp_zUnBillLR ct WHERE ct.oDueDays Between 61 and 90) as LR90Amt," +
                    " (SELECT ISNULL(sum(amt),0) FROM Dztmp_zUnBillLR ct WHERE ct.oDueDays Between 91 and 120) as LR120Amt," +
                    " (SELECT ISNULL(sum(amt),0) FROM Dztmp_zUnBillLR ct WHERE ct.oDueDays > 120) as LR120MAmt" +
                    " from Dztmp_zUnBillLR ";
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();

                    if (dt.Rows.Count > 0)
                    {
                        Model.UnBillLr30 = Convert.ToInt32(dt.Rows[0]["LR30"].ToString() == "" ? "0" : dt.Rows[0]["LR30"].ToString());
                        Model.UnBillLr60 = Convert.ToInt32(dt.Rows[0]["LR60"].ToString() == "" ? "0" : dt.Rows[0]["LR60"].ToString());
                        Model.UnBillLr90 = Convert.ToInt32(dt.Rows[0]["LR90"].ToString() == "" ? "0" : dt.Rows[0]["LR90"].ToString());
                        Model.UnBillLr120 = Convert.ToInt32(dt.Rows[0]["LR120"].ToString() == "" ? "0" : dt.Rows[0]["LR120"].ToString());
                        Model.UnBillLr120M = Convert.ToInt32(dt.Rows[0]["LR120M"].ToString() == "" ? "0" : dt.Rows[0]["LR120M"].ToString());

                        Model.UnBillLrAmt30 = Convert.ToDecimal(dt.Rows[0]["LR30Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR30Amt"].ToString());
                        Model.UnBillLrAmt60 = Convert.ToDecimal(dt.Rows[0]["LR60Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR60Amt"].ToString());
                        Model.UnBillLrAmt90 = Convert.ToDecimal(dt.Rows[0]["LR90Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR90Amt"].ToString());
                        Model.UnBillLrAmt120 = Convert.ToDecimal(dt.Rows[0]["LR120Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR120Amt"].ToString());
                        Model.UnBillLrAmt120M = Convert.ToDecimal(dt.Rows[0]["LR120MAmt"].ToString() == "" ? "0" : dt.Rows[0]["LR120MAmt"].ToString());

                        Model.UnBillTotalConsignment = Model.UnBillLr30 + Model.UnBillLr60 + Model.UnBillLr90 + Model.UnBillLr120 + Model.UnBillLr120M;
                        Model.UnBillTotalAmtConsignment = Model.UnBillLrAmt30 + Model.UnBillLrAmt60 + Model.UnBillLrAmt90 + Model.UnBillLrAmt120 + Model.UnBillLrAmt120M;
                    }
                }
                else if (Code == "ConsignmetStock")
                {
                    ConsignmentStock(result.Para10);
                    Query = "select top 1" +
                            " (SELECT COUNT(*) FROM Dztmp_zStockLR ct    WHERE ct.oDueDays Between 0 and 30) as LR30," +
                            " (SELECT COUNT(*) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 31 and 60) as LR60," +
                            " (SELECT COUNT(*) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 61 and 90) as LR90," +
                            " (SELECT COUNT(*) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 91 and 120) as LR120," +
                            " (SELECT COUNT(*) FROM Dztmp_zStockLR ct WHERE ct.oDueDays > 120) as LR120M," +
                            " (SELECT ISNULL(sum(BalQty),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 0 and 30) as LR30Amt," +
                            " (SELECT ISNULL(sum(BalQty),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 31 and 60) as LR60Amt," +
                            " (SELECT ISNULL(sum(BalQty),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 61 and 90) as LR90Amt," +
                            " (SELECT ISNULL(sum(BalQty),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 91 and 120) as LR120Amt," +
                            " (SELECT ISNULL(sum(BalQty),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays > 120) as LR120MAmt," +
                            " (SELECT ISNULL(sum(ChgWt),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 0 and 30) as LR30CH," +
                            " (SELECT ISNULL(sum(ChgWt),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 31 and 60) as LR60CH," +
                            " (SELECT ISNULL(sum(ChgWt),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 61 and 90) as LR90CH," +
                            " (SELECT ISNULL(sum(ChgWt),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 91 and 120) as LR120CH," +
                            " (SELECT ISNULL(sum(ChgWt),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays > 120) as LR120MCH," +
                            " (SELECT ISNULL(sum(DecVal),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 0 and 30) as LR30Declare," +
                            " (SELECT ISNULL(sum(DecVal),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 31 and 60) as LR60Declare," +
                            " (SELECT ISNULL(sum(DecVal),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 61 and 90) as LR90Declare," +
                            " (SELECT ISNULL(sum(DecVal),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 91 and 120) as LR120Declare," +
                            " (SELECT ISNULL(sum(DecVal),0) FROM Dztmp_zStockLR ct WHERE ct.oDueDays > 120) as LR120MDeclare" +
                            " from Dztmp_zStockLR ";
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();

                    if (dt.Rows.Count > 0)
                    {
                        Model.StockLr30 = Convert.ToInt32(dt.Rows[0]["LR30"].ToString() == "" ? "0" : dt.Rows[0]["LR30"].ToString());
                        Model.StockLr60 = Convert.ToInt32(dt.Rows[0]["LR60"].ToString() == "" ? "0" : dt.Rows[0]["LR60"].ToString());
                        Model.StockLr90 = Convert.ToInt32(dt.Rows[0]["LR90"].ToString() == "" ? "0" : dt.Rows[0]["LR90"].ToString());
                        Model.StockLr120 = Convert.ToInt32(dt.Rows[0]["LR120"].ToString() == "" ? "0" : dt.Rows[0]["LR120"].ToString());
                        Model.StockLr120M = Convert.ToInt32(dt.Rows[0]["LR120M"].ToString() == "" ? "0" : dt.Rows[0]["LR120M"].ToString());

                        Model.StockLrQty30 = Convert.ToInt32(dt.Rows[0]["LR30Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR30Amt"].ToString());
                        Model.StockLrQty60 = Convert.ToInt32(dt.Rows[0]["LR60Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR60Amt"].ToString());
                        Model.StockLrQty90 = Convert.ToInt32(dt.Rows[0]["LR90Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR90Amt"].ToString());
                        Model.StockLrQty120 = Convert.ToInt32(dt.Rows[0]["LR120Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR120Amt"].ToString());
                        Model.StockLrQty120M = Convert.ToInt32(dt.Rows[0]["LR120MAmt"].ToString() == "" ? "0" : dt.Rows[0]["LR120MAmt"].ToString());

                        Model.StockLrWT30 = Convert.ToDecimal(dt.Rows[0]["LR30CH"].ToString() == "" ? "0" : dt.Rows[0]["LR30CH"].ToString());
                        Model.StockLrWT60 = Convert.ToDecimal(dt.Rows[0]["LR60CH"].ToString() == "" ? "0" : dt.Rows[0]["LR60CH"].ToString());
                        Model.StockLrWT90 = Convert.ToDecimal(dt.Rows[0]["LR90CH"].ToString() == "" ? "0" : dt.Rows[0]["LR90CH"].ToString());
                        Model.StockLrWT120 = Convert.ToDecimal(dt.Rows[0]["LR120CH"].ToString() == "" ? "0" : dt.Rows[0]["LR120CH"].ToString());
                        Model.StockLrWT120M = Convert.ToDecimal(dt.Rows[0]["LR120MCH"].ToString() == "" ? "0" : dt.Rows[0]["LR120MCH"].ToString());

                        Model.StockLrDeclar30 = Convert.ToDecimal(dt.Rows[0]["LR30Declare"].ToString() == "" ? "0" : dt.Rows[0]["LR30Declare"].ToString());
                        Model.StockLrDeclar60 = Convert.ToDecimal(dt.Rows[0]["LR60Declare"].ToString() == "" ? "0" : dt.Rows[0]["LR60Declare"].ToString());
                        Model.StockLrDeclar90 = Convert.ToDecimal(dt.Rows[0]["LR90Declare"].ToString() == "" ? "0" : dt.Rows[0]["LR90Declare"].ToString());
                        Model.StockLrDeclar120 = Convert.ToDecimal(dt.Rows[0]["LR120Declare"].ToString() == "" ? "0" : dt.Rows[0]["LR120Declare"].ToString());
                        Model.StockLrDeclar120M = Convert.ToDecimal(dt.Rows[0]["LR120MDeclare"].ToString() == "" ? "0" : dt.Rows[0]["LR120MDeclare"].ToString());

                        Model.StockTotalConsignment = Model.StockLr30 + Model.StockLr60 + Model.StockLr90 + Model.StockLr120 + Model.StockLr120M;
                        Model.StockTotalQtyConsignment = Model.StockLrQty30 + Model.StockLrQty60 + Model.StockLrQty90 + Model.StockLrQty120 + Model.StockLrQty120M;
                        Model.StockTotalWeightConsignment = Model.StockLrWT30 + Model.StockLrWT60 + Model.StockLrWT90 + Model.StockLrWT120 + Model.StockLrWT120M;
                        Model.StockTotalValueConsignment = Model.StockLrDeclar30 + Model.StockLrDeclar60 + Model.StockLrDeclar90 + Model.StockLrDeclar120 + Model.StockLrDeclar120M;
                    }
                }
                else if (Code == "ConsignmeTRNStock")
                {
                    ConsignmentStockTRN(result.Para10);
                    Query = "select top 1" +
                            " (SELECT COUNT(*) FROM Dztmp_zStockLRTRN ct    WHERE ct.oDueDays Between 0 and 30) as LR30," +
                            " (SELECT COUNT(*) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 31 and 60) as LR60," +
                            " (SELECT COUNT(*) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 61 and 90) as LR90," +
                            " (SELECT COUNT(*) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 91 and 120) as LR120," +
                            " (SELECT COUNT(*) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays > 120) as LR120M," +
                            " (SELECT ISNULL(sum(BalQty),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 0 and 30) as LR30Amt," +
                            " (SELECT ISNULL(sum(BalQty),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 31 and 60) as LR60Amt," +
                            " (SELECT ISNULL(sum(BalQty),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 61 and 90) as LR90Amt," +
                            " (SELECT ISNULL(sum(BalQty),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 91 and 120) as LR120Amt," +
                            " (SELECT ISNULL(sum(BalQty),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays > 120) as LR120MAmt," +
                            " (SELECT ISNULL(sum(ChgWt),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 0 and 30) as LR30CH," +
                            " (SELECT ISNULL(sum(ChgWt),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 31 and 60) as LR60CH," +
                            " (SELECT ISNULL(sum(ChgWt),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 61 and 90) as LR90CH," +
                            " (SELECT ISNULL(sum(ChgWt),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 91 and 120) as LR120CH," +
                            " (SELECT ISNULL(sum(ChgWt),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays > 120) as LR120MCH," +
                            " (SELECT ISNULL(sum(DecVal),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 0 and 30) as LR30Declare," +
                            " (SELECT ISNULL(sum(DecVal),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 31 and 60) as LR60Declare," +
                            " (SELECT ISNULL(sum(DecVal),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 61 and 90) as LR90Declare," +
                            " (SELECT ISNULL(sum(DecVal),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 91 and 120) as LR120Declare," +
                            " (SELECT ISNULL(sum(DecVal),0) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays > 120) as LR120MDeclare" +
                            " from Dztmp_zStockLRTRN ";
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();

                    if (dt.Rows.Count > 0)
                    {
                        Model.StockLr30TRN = Convert.ToInt32(dt.Rows[0]["LR30"].ToString() == "" ? "0" : dt.Rows[0]["LR30"].ToString());
                        Model.StockLr60TRN = Convert.ToInt32(dt.Rows[0]["LR60"].ToString() == "" ? "0" : dt.Rows[0]["LR60"].ToString());
                        Model.StockLr90TRN = Convert.ToInt32(dt.Rows[0]["LR90"].ToString() == "" ? "0" : dt.Rows[0]["LR90"].ToString());
                        Model.StockLr120TRN = Convert.ToInt32(dt.Rows[0]["LR120"].ToString() == "" ? "0" : dt.Rows[0]["LR120"].ToString());
                        Model.StockLr120MTRN = Convert.ToInt32(dt.Rows[0]["LR120M"].ToString() == "" ? "0" : dt.Rows[0]["LR120M"].ToString());

                        Model.StockLrQty30TRN = Convert.ToInt32(dt.Rows[0]["LR30Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR30Amt"].ToString());
                        Model.StockLrQty60TRN = Convert.ToInt32(dt.Rows[0]["LR60Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR60Amt"].ToString());
                        Model.StockLrQty90TRN = Convert.ToInt32(dt.Rows[0]["LR90Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR90Amt"].ToString());
                        Model.StockLrQty120TRN = Convert.ToInt32(dt.Rows[0]["LR120Amt"].ToString() == "" ? "0" : dt.Rows[0]["LR120Amt"].ToString());
                        Model.StockLrQty120MTRN = Convert.ToInt32(dt.Rows[0]["LR120MAmt"].ToString() == "" ? "0" : dt.Rows[0]["LR120MAmt"].ToString());

                        Model.StockLrWT30TRN = Convert.ToDecimal(dt.Rows[0]["LR30CH"].ToString() == "" ? "0" : dt.Rows[0]["LR30CH"].ToString());
                        Model.StockLrWT60TRN = Convert.ToDecimal(dt.Rows[0]["LR60CH"].ToString() == "" ? "0" : dt.Rows[0]["LR60CH"].ToString());
                        Model.StockLrWT90TRN = Convert.ToDecimal(dt.Rows[0]["LR90CH"].ToString() == "" ? "0" : dt.Rows[0]["LR90CH"].ToString());
                        Model.StockLrWT120TRN = Convert.ToDecimal(dt.Rows[0]["LR120CH"].ToString() == "" ? "0" : dt.Rows[0]["LR120CH"].ToString());
                        Model.StockLrWT120MTRN = Convert.ToDecimal(dt.Rows[0]["LR120MCH"].ToString() == "" ? "0" : dt.Rows[0]["LR120MCH"].ToString());

                        Model.StockLrDeclar30TRN = Convert.ToDecimal(dt.Rows[0]["LR30Declare"].ToString() == "" ? "0" : dt.Rows[0]["LR30Declare"].ToString());
                        Model.StockLrDeclar60TRN = Convert.ToDecimal(dt.Rows[0]["LR60Declare"].ToString() == "" ? "0" : dt.Rows[0]["LR60Declare"].ToString());
                        Model.StockLrDeclar90TRN = Convert.ToDecimal(dt.Rows[0]["LR90Declare"].ToString() == "" ? "0" : dt.Rows[0]["LR90Declare"].ToString());
                        Model.StockLrDeclar120TRN = Convert.ToDecimal(dt.Rows[0]["LR120Declare"].ToString() == "" ? "0" : dt.Rows[0]["LR120Declare"].ToString());
                        Model.StockLrDeclar120MTRN = Convert.ToDecimal(dt.Rows[0]["LR120MDeclare"].ToString() == "" ? "0" : dt.Rows[0]["LR120MDeclare"].ToString());

                        Model.StockTotalConsignmentTRN = Model.StockLr30TRN + Model.StockLr60TRN + Model.StockLr90TRN + Model.StockLr120TRN + Model.StockLr120MTRN;
                        Model.StockTotalQtyConsignmentTRN = Model.StockLrQty30TRN + Model.StockLrQty60TRN + Model.StockLrQty90TRN + Model.StockLrQty120TRN + Model.StockLrQty120MTRN;
                        Model.StockTotalWeightConsignmentTRN = Model.StockLrWT30TRN + Model.StockLrWT60TRN + Model.StockLrWT90TRN + Model.StockLrWT120TRN + Model.StockLrWT120MTRN;
                        Model.StockTotalValueConsignmentTRN = Model.StockLrDeclar30TRN + Model.StockLrDeclar60TRN + Model.StockLrDeclar90TRN + Model.StockLrDeclar120TRN + Model.StockLrDeclar120MTRN;

                    }
                }
                else if (Code == "ConsignmetBook")
                {
                    ConsignmentBooking(result.Para10, result.Para1, StartDate, EndDate);
                    Query = "select top 1" +
                            " SUM(Aprl) as Aprl,SUM(May) as May,SUM(Jun) as Jun,SUM(Jul) as Jul,SUM(Aug) as Aug," +
                            " SUM(Sept) as Sept,SUM(Oct) as Oct,SUM(Nov) as Nov,SUM(Dec) as Dec,SUM(Jan) as Jan," +
                            " SUM(Feb) as Feb,SUM(Mar) as Mar" +
                            " from Dztmp_zBookLR ";
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();

                    if (dt.Rows.Count > 0)
                    {
                        Model.BKApr = Convert.ToDecimal(dt.Rows[0]["Aprl"].ToString() == "" ? "0" : dt.Rows[0]["Aprl"].ToString());
                        Model.BKMay = Convert.ToDecimal(dt.Rows[0]["May"].ToString() == "" ? "0" : dt.Rows[0]["May"].ToString());
                        Model.BKJun = Convert.ToDecimal(dt.Rows[0]["Jun"].ToString() == "" ? "0" : dt.Rows[0]["Jun"].ToString());
                        Model.BKJuy = Convert.ToDecimal(dt.Rows[0]["Jul"].ToString() == "" ? "0" : dt.Rows[0]["Jul"].ToString());
                        Model.BKAug = Convert.ToDecimal(dt.Rows[0]["Aug"].ToString() == "" ? "0" : dt.Rows[0]["Aug"].ToString());
                        Model.BKSept = Convert.ToDecimal(dt.Rows[0]["Sept"].ToString() == "" ? "0" : dt.Rows[0]["Sept"].ToString());
                        Model.BKOct = Convert.ToDecimal(dt.Rows[0]["Oct"].ToString() == "" ? "0" : dt.Rows[0]["Oct"].ToString());
                        Model.BKNov = Convert.ToDecimal(dt.Rows[0]["Nov"].ToString() == "" ? "0" : dt.Rows[0]["Nov"].ToString());
                        Model.BKDec = Convert.ToDecimal(dt.Rows[0]["Dec"].ToString() == "" ? "0" : dt.Rows[0]["Dec"].ToString());
                        Model.BKJan = Convert.ToDecimal(dt.Rows[0]["Jan"].ToString() == "" ? "0" : dt.Rows[0]["Jan"].ToString());
                        Model.BKFeb = Convert.ToDecimal(dt.Rows[0]["Feb"].ToString() == "" ? "0" : dt.Rows[0]["Feb"].ToString());
                        Model.BKMar = Convert.ToDecimal(dt.Rows[0]["Mar"].ToString() == "" ? "0" : dt.Rows[0]["Mar"].ToString());

                        Model.BKTotal = Model.BKApr + Model.BKMay + Model.BKJun + Model.BKJuy + Model.BKAug + Model.BKSept + Model.BKOct + Model.BKNov + Model.BKDec + Model.BKJan + Model.BKFeb + Model.BKMar;
                    }
                }
                else if (Code == "TopCustomers")
                {
                    string FromDate = (Convert.ToDateTime(StartDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    string ToDate = (Convert.ToDateTime(EndDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

                    ExecuteStoredProc("Drop Table Top10Customer");
                    Query = "Select Top 10 CustomerMaster.Name,sum(Amt) as Amt into Top10Customer from Sales,CustomerMaster " +
                                    " where Charindex(Sales.branch, '" + result.Para10 + "') <> 0  And sales.DocDate >= '" + FromDate + "' and " +
                                    "Sales.DocDate <= '" + ToDate + "' And CustomerMaster.Code = Sales.Code and " +
                                    "Sales.Code is not null group by CustomerMaster.Name Order by sum(amt) desc";
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();

                    dt = new DataTable();
                    Query = "Select Name,Amt from Top10Customer ";
                    tfat_conx = new SqlConnection(GetConnectionString());
                    cmd = new SqlCommand(Query, tfat_conx);
                    sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();

                    List<string> CustomerName = new List<string>();
                    List<decimal> CustomerAmount = new List<decimal>();

                    foreach (DataRow row in dt.Rows)
                    {
                        CustomerName.Add(row["Name"].ToString().ToUpper());
                        CustomerAmount.Add(Convert.ToDecimal(row["Amt"].ToString()));
                    }

                    Model.TopCustomerName = CustomerName;
                    Model.TopCustomerAmt = CustomerAmount;
                }
                else if (Code == "TopGroupCustomers")
                {
                    string FromDate = (Convert.ToDateTime(StartDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    string ToDate = (Convert.ToDateTime(EndDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

                    ExecuteStoredProc("Drop Table Top10GroupCustomer");
                    Query = "Select Top 10 Master.Name,sum(Amt) as Amt into Top10GroupCustomer from Sales,Master " +
                            " where Charindex(Sales.branch, '" + result.Para10 + "') <> 0  And sales.DocDate >= '" + FromDate + "' and " +
                            "Sales.DocDate <= '" + ToDate + "' And Master.Code = Sales.CustGroup and " +
                            "Sales.CustGroup is not null group by Master.Name Order by sum(amt) desc";
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();

                    dt = new DataTable();
                    Query = "Select Name,Amt from Top10GroupCustomer ";
                    tfat_conx = new SqlConnection(GetConnectionString());
                    cmd = new SqlCommand(Query, tfat_conx);
                    sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();

                    List<string> CustomerName = new List<string>();
                    List<decimal> CustomerAmount = new List<decimal>();

                    foreach (DataRow row in dt.Rows)
                    {
                        CustomerName.Add(row["Name"].ToString().ToUpper());
                        CustomerAmount.Add(Convert.ToDecimal(row["Amt"].ToString()));
                    }

                    Model.TopGroupCustomerName = CustomerName;
                    Model.TopGroupCustomerAmt = CustomerAmount;
                }
                else if (Code == "TopVendors")
                {
                    string FromDate = (Convert.ToDateTime(StartDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    string ToDate = (Convert.ToDateTime(EndDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

                    ExecuteStoredProc("Drop Table Top10Vendor");
                    Query = "Select Top 10 Master.Name,sum(Amt) as Amt into Top10Vendor from Purchase,Master " +
                            " where Charindex(Purchase.branch, '" + result.Para10 + "') <> 0  And Purchase.BillDate >= '" + FromDate + "' and " +
                            "Purchase.BillDate <= '" + ToDate + "' And Master.Code = Purchase.Code and " +
                            "Purchase.Code is not null group by Master.Name Order by sum(amt) desc";
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();

                    dt = new DataTable();
                    Query = "Select Name,Amt from Top10Vendor ";
                    tfat_conx = new SqlConnection(GetConnectionString());
                    cmd = new SqlCommand(Query, tfat_conx);
                    sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();

                    List<string> CustomerName = new List<string>();
                    List<decimal> CustomerAmount = new List<decimal>();

                    foreach (DataRow row in dt.Rows)
                    {
                        CustomerName.Add(row["Name"].ToString().ToUpper());
                        CustomerAmount.Add(Convert.ToDecimal(row["Amt"].ToString()));
                    }

                    Model.TopVendorsName = CustomerName;
                    Model.TopVendorsAmt = CustomerAmount;
                }
                else if (Code == "TopExpenses")
                {

                    ExecuteStoredProc("Drop Table Top10Expenses");
                    Query = "select top 10 (select M.Name From Master M where M.Code=L.Code) as Name,sum(L.debit) as Amt " +
                        "into Top10Expenses from Ledger L where L.Code in ( select m.code from master m where m.grp in " +
                        "(select g.code from mastergroups g where name like '%Exp%')) and Charindex(L.Branch, '" + result.Para10 + "') <> 0 group by L.code " +
                        "order by sum(L.debit) desc ";
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();

                    dt = new DataTable();
                    Query = "Select Name,Amt from Top10Expenses ";
                    tfat_conx = new SqlConnection(GetConnectionString());
                    cmd = new SqlCommand(Query, tfat_conx);
                    sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();

                    List<string> CustomerName = new List<string>();
                    List<decimal> CustomerAmount = new List<decimal>();

                    foreach (DataRow row in dt.Rows)
                    {
                        CustomerName.Add(row["Name"].ToString().ToUpper());
                        CustomerAmount.Add(Convert.ToDecimal(row["Amt"].ToString()));
                    }

                    Model.TopExpenseName = CustomerName;
                    Model.TopExpenseAmt = CustomerAmount;
                }
                else if (Code == "VehicleStatus")
                {
                    var VehicleGroupStatus = "";
                    if (result.Para1 == "T")
                    {
                        VehicleGroupStatus += "100002,";
                    }
                    if (result.Para2 == "T")
                    {
                        VehicleGroupStatus += "100000,";
                    }
                    VehicleGroupStatus = VehicleGroupStatus.Substring(0, VehicleGroupStatus.Length - 1);
                    ExecuteStoredProc("Drop Table Ztmp_VehicleStatus1");
                    ExecuteStoredProc("select (select top 1 G.Status from tfatVehicleStatusHistory G where G.TruckNo= VehicleMaster.Code order by G.FromPeriod desc,G.FromTime desc) as Status into Ztmp_VehicleStatus1 from vehiclemaster where VehicleMaster.Acitve='true' and VehicleMaster.Code not in ('99999','99998') and Charindex(TruckStatus, '" + VehicleGroupStatus + "') <> 0 ");
                    ExecuteStoredProc("Drop Table Ztmp_VehicleStatus");

                    Query = "select Status,count(status) as Total " +
                        "into Ztmp_VehicleStatus from Ztmp_VehicleStatus1 " +
                        " group by Status";
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();

                    dt = new DataTable();
                    Query = "Select Status,Total from Ztmp_VehicleStatus order by status";
                    tfat_conx = new SqlConnection(GetConnectionString());
                    cmd = new SqlCommand(Query, tfat_conx);
                    sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();

                    int ActiverVehicle = 0;
                    int CountVehicle = 0;
                    int Maintain = 0;
                    int NODRiver = 0;
                    int Ready = 0;
                    int Transit = 0;
                    int Accident = 0;
                    int Sale = 0;

                    foreach (DataRow row in dt.Rows)
                    {

                        var Value = row["Status"].ToString();
                        ActiverVehicle += ctxTFAT.VehicleMaster.Where(x => x.Status == Value && x.Code != "99999" && x.Code != "99998" && x.Acitve == true).ToList().Count();
                        if (row["Status"].ToString().Trim() == "Maintaince")
                        {
                            Maintain = Convert.ToInt32(row["Total"].ToString());
                        }
                        else if (row["Status"].ToString().Trim() == "NODriver" || row["Status"].ToString().Trim() == "NULL" || row["Status"].ToString().Trim() == "")
                        {
                            NODRiver += Convert.ToInt32(row["Total"].ToString());
                        }
                        else if (row["Status"].ToString().Trim() == "Ready")
                        {
                            Ready = Convert.ToInt32(row["Total"].ToString());
                        }
                        else if (row["Status"].ToString().Trim() == "Transit")
                        {
                            Transit = Convert.ToInt32(row["Total"].ToString());
                        }
                        else if (row["Status"].ToString().Trim() == "Accident")
                        {
                            Accident = Convert.ToInt32(row["Total"].ToString());
                        }
                        else if (row["Status"].ToString().Trim() == "Sale")
                        {
                            Sale = Convert.ToInt32(row["Total"].ToString());
                        }
                    }

                    Model.VActiveVehicle = ActiverVehicle;
                    Model.VCountVehicle = ctxTFAT.VehicleMaster.Where(x => x.Code != "99999" && x.Code != "99998").ToList().Count();
                    Model.VMaintain = Maintain;
                    Model.VNoDriver = NODRiver;
                    Model.VReady = Ready;
                    Model.VTransit = Transit;
                    Model.VAccident = Accident;
                    Model.VSale = Sale;
                }
                else if (Code == "DriverStatus")
                {
                    ExecuteStoredProc("Drop Table Ztmp_DriverStatus");
                    ExecuteStoredProc("select (select top 1  G.Vehicle  from TfatDriverStatus G where G.Driver= DriverMaster.Code order by G.DocNo desc) as Status into Ztmp_DriverStatus from DriverMaster where DriverMaster.Status='true' and DriverMaster.Code not in ('99999')");
                    ExecuteStoredProc("Drop Table Ztmp_DriverStatus1");
                    Query = "select (select V.Truckno from vehiclemaster V where V.code=D.Status) as Status,count(*) as Total into Ztmp_DriverStatus1 from Ztmp_DriverStatus D group by D.Status";

                    //ExecuteStoredProc("Drop Table Ztmp_DriverStatus");
                    //Query = "select (select V.Truckno from vehiclemaster V where V.code=D.VehicleNo) as Status,count(*) as Total into Ztmp_DriverStatus from DriverMaster D group by D.VehicleNo";
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();

                    int ActiverDrivers = ctxTFAT.DriverMaster.Where(x => x.Code != "99999" && x.Status == true).ToList().Count();

                    dt = new DataTable();
                    Query = "Select Status,Total from Ztmp_DriverStatus1 ";
                    tfat_conx = new SqlConnection(GetConnectionString());
                    cmd = new SqlCommand(Query, tfat_conx);
                    sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();

                    int OnVehicleDriver = 0;
                    int bhatta = 0;
                    int novehicle = 0;

                    foreach (DataRow row in dt.Rows)
                    {
                        if (row["Status"].ToString().Trim() == "On Bhatta")
                        {
                            bhatta = Convert.ToInt32(row["Total"].ToString());
                        }
                        else if (row["Status"].ToString().Trim() == "No Vehicle" || row["Status"].ToString().Trim() == "NULL")
                        {
                            novehicle = Convert.ToInt32(row["Total"].ToString());
                        }
                        else
                        {
                            OnVehicleDriver += Convert.ToInt32(row["Total"].ToString());
                        }
                    }
                    Model.ActiverDrivers = ActiverDrivers;
                    Model.DriverCount = ctxTFAT.DriverMaster.Where(x => x.Code != "99999").ToList().Count();
                    Model.Bhatta = bhatta;
                    Model.NOVehicle = novehicle;
                    Model.ONVehicle = OnVehicleDriver;
                }
                else if (Code == "VehicleLocation")
                {
                    var DbVehicelCode = result.Para1.Split(',').ToList();
                    var TrackVehicleList = ctxTFAT.VehicleMaster.Where(x => DbVehicelCode.Contains(x.Code)).Select(x => x.TruckNo.Replace(" ", "").Replace(" ", "").Replace(" ", "").ToUpper()).ToList();

                    //TrackVehicleList.ToList().ForEach(s => s = s.ToUpper());
                    List<VehicleTrackinModel> trackinModels = new List<VehicleTrackinModel>();
                    var SetUrl = "http://speed.elixiatech.com/modules/api/vts/api.php?action=getVehicleDataV1&jsonreq={'userkey':'c8b8ec3f3e0dca3c823fbda9ce93ed3a74684bc9','searchstring':'GJ','pageindex':1,'pagesize':10,'isLocationEnabled':1}";
                    SetUrl = SetUrl.Replace("'", "\"");
                    WebClient client = new WebClient();
                    string jsonstring = client.DownloadString(SetUrl);
                    dynamic dynObj = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonstring);
                    var status = dynObj.Status;
                    if (status.Value == "1")
                    {
                        foreach (var member in dynObj.Result.data)
                        {
                            string VehicleNo = member["vehicleno"];
                            VehicleNo = Regex.Replace(VehicleNo, @"\s+", "");
                            VehicleNo = Regex.Replace(VehicleNo, @"\s+", "");
                            string VehicleNo1 = Regex.Replace(VehicleNo, @"\s+", "");
                            if (TrackVehicleList.Contains(VehicleNo1.ToUpper()))
                            {
                                VehicleTrackinModel vehicleTrackin = new VehicleTrackinModel();
                                vehicleTrackin.title = member["vehicleno"];
                                vehicleTrackin.description = member["vehicleno"];
                                vehicleTrackin.lat = member["lat"];
                                vehicleTrackin.lng = member["lng"];
                                trackinModels.Add(vehicleTrackin);
                            }
                        }
                    }

                    SetUrl = "http://api.ilogistek.com:6049/tracking?AuthKey=PV7q60SurjHlacX398hbQ4SiWfYktoL&vehicle_no=all";
                    client = new WebClient();
                    jsonstring = client.DownloadString(SetUrl);
                    dynamic dynObj1 = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonstring);
                    foreach (var member in dynObj1)
                    {
                        string VehicleNo = member["VehicleNo"];
                        VehicleNo = Regex.Replace(VehicleNo, @"\s+", "");
                        VehicleNo = Regex.Replace(VehicleNo, @"\s+", "");
                        string VehicleNo1 = Regex.Replace(VehicleNo, @"\s+", "");
                        if (TrackVehicleList.Contains(VehicleNo1.ToUpper()))
                        {
                            VehicleTrackinModel vehicleTrackin = new VehicleTrackinModel();
                            vehicleTrackin.title = member["VehicleNo"];
                            vehicleTrackin.description = member["VehicleNo"];
                            vehicleTrackin.lat = member["Latitude"];
                            vehicleTrackin.lng = member["Longitude"];
                            trackinModels.Add(vehicleTrackin);
                        }
                    }
                    trackinModels.Add(new VehicleTrackinModel
                    {
                        title = "All Vehicles"
                    });
                    Model.VehicleTrackList = trackinModels;
                    Model.CountVehicle = (trackinModels.Count() - 1).ToString();
                    trackinModels = new List<VehicleTrackinModel>();
                    var list = ctxTFAT.TfatBranchLocation.Where(x => String.IsNullOrEmpty(x.Location) == false && String.IsNullOrEmpty(x.Title) == false).ToList();
                    Model.CountBranch = (list.Count()).ToString();
                    Model.BranchReq = true;
                    foreach (var branch in list)
                    {
                        var Location = branch.Location.Split(',');
                        VehicleTrackinModel vehicleTrackin = new VehicleTrackinModel();
                        vehicleTrackin.title = branch.Title;
                        vehicleTrackin.description = "Branch";
                        vehicleTrackin.lat = Convert.ToDouble(Location[0]);
                        vehicleTrackin.lng = Convert.ToDouble(Location[1]);
                        trackinModels.Add(vehicleTrackin);
                    }
                    Model.BranchTrackList = trackinModels;
                }
                else if (Code == "DriverTripBalance")
                {
                    var DriverGrp = ctxTFAT.Master.Where(x => x.OthPostType.Contains("D")).Select(x => x.Grp).FirstOrDefault();
                    GenerateGrpWithBalance(StartDate + ":" + EndDate, "", "TBL", Convert.ToInt32("0"), "", false, 0, true);
                    SqlConnection conTFAT = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand("dbo.SPTFAT_GetAccountSchedule", conTFAT);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.Add("@mGroup", SqlDbType.VarChar).Value = DriverGrp;
                    conTFAT.Open();
                    cmd.CommandTimeout = 0;
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    conTFAT.Dispose();

                    //Driver Bal Total
                    dt = new DataTable();
                    Query = "with Dr as ( select sum(bal) as Bal from ztmp_temp ) select case when Bal>0 then cast(Bal as varchar)+' DR' else cast(Bal as varchar)+' CR' end as Bal from Dr";
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    cmd = new SqlCommand(Query, tfat_conx);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();
                    if (dt.Rows.Count > 0)
                    {
                        Model.DriverBalTotal = dt.Rows[0]["Bal"].ToString();
                    }

                    ExecuteStoredProc("Drop Table Ztmp_DriverTripBalance");
                    Query = "WITH ranked_messages AS ( select D.Name, ROW_NUMBER() OVER(PARTITION BY T.Driver ORDER BY T.TODT DESC) AS rn, Convert(char(10), T.TODT, 103) as LastDate,case when  Z.Bal > 0 then cast(Z.Bal as varchar) + ' DR' else cast(((Z.Bal) * (-1)) as varchar) + ' CR' end as BAL  from drivermaster D left join TripSheetMaster T on T.Driver = D.Code join ztmp_temp Z on Z.Code = D.Posting where Charindex(D.Code, '" + result.Para1 + "') <> 0 ) " +
                        "SELECT* into Ztmp_DriverTripBalance FROM ranked_messages WHERE rn = 1 order by Name";
                    tfat_conx = new SqlConnection(GetConnectionString());
                    cmd = new SqlCommand(Query, tfat_conx);
                    sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();

                    dt = new DataTable();
                    Query = "Select Name,LastDate,BAL from Ztmp_DriverTripBalance order by Name ";
                    tfat_conx = new SqlConnection(GetConnectionString());
                    cmd = new SqlCommand(Query, tfat_conx);
                    sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();

                    List<string> DriverName = new List<string>();
                    List<string> DriverLastTrip = new List<string>();
                    List<string> DriverBal = new List<string>();

                    foreach (DataRow row in dt.Rows)
                    {
                        DriverName.Add(row["Name"].ToString().ToUpper());
                        DriverLastTrip.Add(row["LastDate"].ToString());
                        DriverBal.Add(row["BAL"].ToString());
                    }

                    Model.DriverName = DriverName;
                    Model.DriverLastTripDate = DriverLastTrip;
                    Model.DriverBal = DriverBal;
                }
                else if (Code == "VehicleTripDetails")
                {
                    Query = "select D.TruckNo as Name," +
                        " (select Top 1 isnull( V.KM,0) from VehicleKmMaintainMa V where V.VehicleNo=D.Code order by V.Date desc) as LastKM," +
                        "(select top 1 Convert(char(10), T.TODT, 103) from tripsheetmaster T where T.Docno in (select TF.DocNo from TripFmList TF where TF.VehicleNo=D.Code) order by T.TODT desc ) as LastDate" +
                        " from vehiclemaster D where Charindex(D.Code, '" + result.Para1 + "') <> 0  order by D.TruckNo";

                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();


                    List<string> VehicleName = new List<string>();
                    List<string> VehicleLastTrip = new List<string>();
                    List<string> VehicleKM = new List<string>();

                    foreach (DataRow row in dt.Rows)
                    {
                        VehicleName.Add(row["Name"].ToString().ToUpper());
                        VehicleLastTrip.Add(row["LastDate"].ToString());
                        VehicleKM.Add(row["LastKM"].ToString());
                    }

                    Model.VehicleName = VehicleName;
                    Model.VehicleLastTripDate = VehicleLastTrip;
                    Model.VehicleKM = VehicleKM;
                }
                else if (Code == "VehicleExpDue")
                {
                    ExecuteStoredProc("Drop Table Ztmp_VehicleExpDue2");
                    Query = "select R.Value8 as Vehicle,R.Code as ExpensesAc,R.date2 as TODT,R.Clear as Clear " +
                            "into Ztmp_VehicleExpDue2 from RelateData R " +
                            " where Charindex(R.value8,'" + result.Para1 + "')  <> 0 " +
                            " and   Charindex(R.Code, '" + result.Para2 + "') <> 0 ";
                    ExecuteStoredProc(Query);

                    Query = " insert into Ztmp_VehicleExpDue2" +
                            " select R.code as Vehicle,R.combo1 as ExpensesAc,R.date2 as TODT,R.Clear as Clear " +
                            " from RelateData R " +
                            " where Charindex(R.Code, '" + result.Para1 + "') <> 0 " +
                            " and   Charindex(R.combo1, '" + result.Para2 + "') <> 0 ";
                    ExecuteStoredProc(Query);

                    ExecuteStoredProc("Drop Table Ztmp_VehicleExpDue1");
                    Query = " WITH ranked_messages AS ( " +
                                " SELECT m.*, ROW_NUMBER() OVER(PARTITION BY Vehicle, ExpensesAc ORDER BY Todt DESC ) AS rn  " +
                                " FROM Ztmp_VehicleExpDue2 AS m )	" +
                            " SELECT* into Ztmp_VehicleExpDue1 FROM ranked_messages WHERE rn = 1; ";
                    ExecuteStoredProc(Query);

                    var Todays = (Convert.ToDateTime(DateTime.Now.ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    ExecuteStoredProc("Drop Table Ztmp_VehicleExpDue");
                    Query = " select (select M.Name From Master M Where M.Code=ExpensesAc) as ExpensesAc," +
                        " Sum(Case when TODT<DateAdd(D,0,'" + Todays + "') then 1 Else 0 End) as Todays, " +
                        " Sum(Case when TODT<DateAdd(D,5,'" + Todays + "') then 1 Else 0 End) as FiveDay, " +
                        " Sum(Case when TODT<DateAdd(D,15,'" + Todays + "') then 1 Else 0 End) as FifteenDay, " +
                        " Sum(Case when TODT<DateAdd(D,30,'" + Todays + "') then 1 Else 0 End) as ThirtyDay " +
                        " into Ztmp_VehicleExpDue from Ztmp_VehicleExpDue1  where Clear='false' and Charindex(ExpensesAc, '" + result.Para2 + "') <> 0 " +
                        " group by ExpensesAc " +
                        " order by ExpensesAc ";
                    ExecuteStoredProc(Query);

                    dt = new DataTable();
                    Query = "Select ExpensesAc,Todays,FiveDay,FifteenDay,ThirtyDay from Ztmp_VehicleExpDue  ";
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();


                    List<string> VehicleExpName = new List<string>();
                    List<string> VehicleExpName0 = new List<string>();
                    List<string> VehicleExpName5 = new List<string>();
                    List<string> VehicleExpName15 = new List<string>();
                    List<string> VehicleExpName30 = new List<string>();

                    foreach (DataRow row in dt.Rows)
                    {
                        VehicleExpName.Add(row["ExpensesAc"].ToString().ToUpper());
                        VehicleExpName0.Add(row["Todays"].ToString());
                        VehicleExpName5.Add(row["FiveDay"].ToString());
                        VehicleExpName15.Add(row["FifteenDay"].ToString());
                        VehicleExpName30.Add(row["ThirtyDay"].ToString());
                    }

                    Model.VehicleExpName = VehicleExpName;
                    Model.VehicleExpName0 = VehicleExpName0;
                    Model.VehicleExpName5 = VehicleExpName5;
                    Model.VehicleExpName15 = VehicleExpName15;
                    Model.VehicleExpName30 = VehicleExpName30;
                }
                else if (Code == "EwayBillDetails")
                {
                    //var FromDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                    //var ToDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();
                    //FromDate = (Convert.ToDateTime(FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    //ToDate = (Convert.ToDateTime(ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    var Currdate = (Convert.ToDateTime(DateTime.Now.ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    var Tomorrowdate = (Convert.ToDateTime(DateTime.Now.AddDays(1).ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    var AfterTomorrowdate = (Convert.ToDateTime(DateTime.Now.AddDays(2).ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    //var EmptyEwayBill = "";
                    //if (result.Para12 == "T")
                    //{
                    //    EmptyEwayBill = " LrTablekey is null or ";
                    //}
                    //var SetFirstStockQueryToEwayBill = " and (charindex((case when LrTablekey is null then '' else  (select top 1 ( case when ( select T.Category From TfatBranch T where T.code= LRSTK.Branch)='Area' then ( select T.Grp From TfatBranch T where T.code= LRSTK.Branch) else LRSTK.Branch end ) from LRStock LRSTK where LRSTK.LRRefTablekey = LrTablekey and LRSTK.type <> 'DEL' and(case when LRSTK.TotalQty = 0 then 0 else (LRSTK.TotalQty - (((select  ISNULL(SUM(LRT.TotalQty), 0) from LRStock LRT where LRT.ParentKey = LRSTK.TableKey)) +(select ISNULL(SUM(lcd.lrqty), 0) from lcdetail lcd where lcd.Parentkey = LRSTK.Tablekey and lcd.LCRefTablekey in (select LCM.Tablekey from lcmaster LCM where LCm.DispachFM = 0))  ) ) end ) <> 0 )end),'" + result.Para10 + "')<> 0  " + EmptyEwayBill + ")  ";

                    ExecuteStoredProc("Drop Table Ztmp_EwayBillDetails");
                    ExecuteStoredProc("Drop Table Ztmp_EwayBillDetails2");
                    ExecuteStoredProc("Drop Table Ztmp_EwayBillDetails3");

                    Query = " select s.EWBNO as EwayBilNo, s.EWBValid as ValidUpto,  " +
                            "(case when s.LrTablekey is null then '' else  ( case when(select T.Category From TfatBranch T where T.code = LRSTK.Branch) = 'Area' then(select T.Grp  From TfatBranch T where T.code = LRSTK.Branch) else (LRSTK.Branch)end )    end )as StockBranchCode,  " +
                            "LRSTK.recordkey as Stockkey, case when s.LrTablekey is null then '' else LRSTK.Type end as StockType,  " +
                            " case when LRSTK.TotalQty = 0 then 0 else (LRSTK.TotalQty - (((select  ISNULL(SUM(LRT.TotalQty), 0) from LRStock LRT where LRT.ParentKey = LRSTK.TableKey)) +(select ISNULL(SUM(lcd.lrqty), 0) from lcdetail lcd where lcd.Parentkey = LRSTK.Tablekey and lcd.LCRefTablekey in (select LCM.Tablekey from lcmaster LCM where LCm.DispachFM = 0))  ) ) end as StockQty  " +
                            " into Ztmp_EwayBillDetails3 from tfatEWB s left join lrmaster R on s.LrTablekey = R.Tablekey left join LRStock LRSTK on LRSTK.LRRefTablekey = R.TableKey " +
                            "  where (s.Doctype = 'LR000' and s.Clear = 'false' and(s.EWBValid is null or s.EWBValid >= '" + Currdate + "')  ) and(s.lrtablekey is null or((case when LRSTK.TotalQty = 0 then 0 else (LRSTK.TotalQty - (((select  ISNULL(SUM(LRT.TotalQty), 0) from LRStock LRT where LRT.ParentKey = LRSTK.TableKey)) + (select ISNULL(SUM(lcd.lrqty), 0) from lcdetail lcd where lcd.Parentkey = LRSTK.Tablekey and lcd.LCRefTablekey in (select LCM.Tablekey from lcmaster LCM where LCm.DispachFM = 0))  ) ) end ) <> 0 )) " +
                            " Order by s.DocDate  ";
                    ExecuteStoredProc(Query);
                    Query = " WITH ranked_messages AS ( " +
                            "SELECT m.*, ROW_NUMBER() OVER(PARTITION BY EwayBilNo ORDER BY Stockkey desc) AS rn  " +
                            "FROM Ztmp_EwayBillDetails3 AS m ) SELECT* into Ztmp_EwayBillDetails2 FROM ranked_messages Z where rn = 1 ";
                    ExecuteStoredProc(Query);

                    ExecuteStoredProc("delete from Ztmp_EwayBillDetails2 where StockType='DEL'");
                    if (result.Para12 == "T")
                    {
                        ExecuteStoredProc("delete from Ztmp_EwayBillDetails2 where Stockkey is not null and  charindex(StockBranchCode,'" + result.Para10 + "')=0");
                    }
                    else
                    {
                        ExecuteStoredProc("delete from Ztmp_EwayBillDetails2 where  charindex(StockBranchCode,'" + result.Para10 + "')=0");
                    }
                    Query = "select count(*) as Active, " +
                            "(select Count(*) from Ztmp_EwayBillDetails2 R where R.ValidUpto = '" + Currdate + "') as TodayExp, " +
                            "(select Count(*) from Ztmp_EwayBillDetails2 R where R.ValidUpto = '" + Tomorrowdate + "') as TomorrowExp, " +
                            "(select Count(*) from Ztmp_EwayBillDetails2 R where R.ValidUpto >= '" + AfterTomorrowdate + "') as Expird " +
                            "  into Ztmp_EwayBillDetails from Ztmp_EwayBillDetails2";
                    ExecuteStoredProc(Query);


                    ////Query = "WITH ranked_messages AS( SELECT top 1 " +
                    //            "(select count(*) from tfatewb where EWBType <> 'Authe' and Clear = 'false'                                                         and( " + EmptyEwayBill + " (charindex((case when LrTablekey is null then '' else  (select top 1( case when(select T.Category From TfatBranch T where T.code = LRSTK.Branch) = 'Area' then(select T.Grp From TfatBranch T where T.code = LRSTK.Branch) else LRSTK.Branch end) from LRStock LRSTK where LRSTK.LRRefTablekey = LrTablekey and LRSTK.type <> 'DEL' and(case when LRSTK.TotalQty = 0 then 0 else (LRSTK.TotalQty - (((select  ISNULL(SUM(LRT.TotalQty), 0) from LRStock LRT where LRT.ParentKey = LRSTK.TableKey)) + (select ISNULL(SUM(lcd.lrqty), 0) from lcdetail lcd where lcd.Parentkey = LRSTK.Tablekey and lcd.LCRefTablekey in (select LCM.Tablekey from lcmaster LCM where LCm.DispachFM = 0)))) end ) <> 0 )end),'" + result.Para10 + "')<> 0  )))     AS Active, " +
                    //            "(select count(*) from tfatewb where (ewbvalid = '" + Currdate + "' or ewbvalid is null )   and EWBType<>'Authe' and Clear = 'false'    and( " + EmptyEwayBill + " (charindex((case when LrTablekey is null then '' else  (select top 1( case when(select T.Category From TfatBranch T where T.code = LRSTK.Branch) = 'Area' then(select T.Grp From TfatBranch T where T.code = LRSTK.Branch) else LRSTK.Branch end) from LRStock LRSTK where LRSTK.LRRefTablekey = LrTablekey and LRSTK.type <> 'DEL' and(case when LRSTK.TotalQty = 0 then 0 else (LRSTK.TotalQty - (((select  ISNULL(SUM(LRT.TotalQty), 0) from LRStock LRT where LRT.ParentKey = LRSTK.TableKey)) + (select ISNULL(SUM(lcd.lrqty), 0) from lcdetail lcd where lcd.Parentkey = LRSTK.Tablekey and lcd.LCRefTablekey in (select LCM.Tablekey from lcmaster LCM where LCm.DispachFM = 0))  ) ) end ) <> 0 )end),'" + result.Para10 + "')<> 0  )))     as TodayExp, " +
                    //            "(select count(*) from tfatewb where (ewbvalid = '" + Tomorrowdate + "' or ewbvalid is null )   and EWBType<>'Authe' and Clear = 'false'    and( " + EmptyEwayBill + " (charindex((case when LrTablekey is null then '' else  (select top 1( case when(select T.Category From TfatBranch T where T.code = LRSTK.Branch) = 'Area' then(select T.Grp From TfatBranch T where T.code = LRSTK.Branch) else LRSTK.Branch end) from LRStock LRSTK where LRSTK.LRRefTablekey = LrTablekey and LRSTK.type <> 'DEL' and(case when LRSTK.TotalQty = 0 then 0 else (LRSTK.TotalQty - (((select  ISNULL(SUM(LRT.TotalQty), 0) from LRStock LRT where LRT.ParentKey = LRSTK.TableKey)) + (select ISNULL(SUM(lcd.lrqty), 0) from lcdetail lcd where lcd.Parentkey = LRSTK.Tablekey and lcd.LCRefTablekey in (select LCM.Tablekey from lcmaster LCM where LCm.DispachFM = 0))  ) ) end ) <> 0 )end),'" + result.Para10 + "')<> 0  )))    as TomorrowExp, " +
                    //            "(select count(*) from tfatewb where (ewbvalid >= '" + AfterTomorrowdate + "' or ewbvalid is null )  and EWBType<>'Authe' and Clear = 'false'    and( " + EmptyEwayBill + " (charindex((case when LrTablekey is null then '' else  (select top 1( case when(select T.Category From TfatBranch T where T.code = LRSTK.Branch) = 'Area' then(select T.Grp From TfatBranch T where T.code = LRSTK.Branch) else LRSTK.Branch end) from LRStock LRSTK where LRSTK.LRRefTablekey = LrTablekey and LRSTK.type <> 'DEL' and(case when LRSTK.TotalQty = 0 then 0 else (LRSTK.TotalQty - (((select  ISNULL(SUM(LRT.TotalQty), 0) from LRStock LRT where LRT.ParentKey = LRSTK.TableKey)) + (select ISNULL(SUM(lcd.lrqty), 0) from lcdetail lcd where lcd.Parentkey = LRSTK.Tablekey and lcd.LCRefTablekey in (select LCM.Tablekey from lcmaster LCM where LCm.DispachFM = 0))  ) ) end ) <> 0 )end),'" + result.Para10 + "')<> 0  )))      as Expird  " +
                    //            " FROM vehiclemaster AS m )SELECT* into Ztmp_EwayBillDetails FROM ranked_messages  ";
                    //ExecuteStoredProc(Query);

                    dt = new DataTable();
                    Query = "Select * from Ztmp_EwayBillDetails   ";
                    SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
                    SqlCommand cmd = new SqlCommand(Query, tfat_conx);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                    tfat_conx.Close();
                    tfat_conx.Dispose();


                    List<string> EWBActive = new List<string>();
                    List<string> EWBActiveToday = new List<string>();
                    List<string> EWBActiveTomorrow = new List<string>();
                    List<string> EWBActiveExpired = new List<string>();

                    foreach (DataRow row in dt.Rows)
                    {
                        EWBActive.Add(row["Active"].ToString());
                        EWBActiveToday.Add(row["TodayExp"].ToString());
                        EWBActiveTomorrow.Add(row["TomorrowExp"].ToString());
                        EWBActiveExpired.Add(row["Expird"].ToString());
                    }

                    Model.EWBActive = EWBActive;
                    Model.EWBActiveToday = EWBActiveToday;
                    Model.EWBActiveTomorrow = EWBActiveTomorrow;
                    Model.EWBActiveExpired = EWBActiveExpired;
                }

            }
            foreach (var item in Model.codes)
            {
                mStr += item.Code + "|";
            }

            ViewBag.Code = mStr;

            var html = ViewHelper.RenderPartialView(this, "_SideMenuDesign", Model);
            return Json(new
            {
                Html = html,
                JsonRequestBehavior.AllowGet
            });
        }
        public JsonResult GetAllLocation()
        {
            ActiveObjectsVM Model = Session["SideMenuModel"] as ActiveObjectsVM;
            if (Model.VehicleTrackList == null)
            {
                Model.VehicleTrackList = new List<VehicleTrackinModel>();
            }
            if (Model.BranchTrackList == null)
            {
                Model.BranchTrackList = new List<VehicleTrackinModel>();
            }

            var data = Model.VehicleTrackList.ToList();
            data.AddRange(Model.BranchTrackList.ToList());
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetVehicleNo(string term)
        {
            ActiveObjectsVM Model = Session["SideMenuModel"] as ActiveObjectsVM;
            if (term == "" || term == null)
            {
                var result = Model.VehicleTrackList.Select(c => new { Code = c.title, Name = c.title }).Distinct().ToList();
                //result.AddRange(Model.BranchTrackList.Select(c => new { Code = c.title, Name = c.title }).Distinct().ToList());
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = Model.VehicleTrackList.Select(c => new { Code = c.title, Name = c.title }).Distinct().ToList();
                //result.AddRange(Model.BranchTrackList.Select(c => new { Code = c.title, Name = c.title }).Distinct().ToList());

                result = result.Where(x => x.Name.ToLower().Trim().Contains(term.ToLower().Trim())).Select(m => new { Code = m.Code, Name = m.Name }).Distinct().ToList();

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetAllLocationById(string Vehicleno)
        {
            ActiveObjectsVM Model = Session["SideMenuModel"] as ActiveObjectsVM;
            List<VehicleTrackinModel> data = new List<VehicleTrackinModel>();
            data.AddRange(Model.BranchTrackList.ToList());
            data.AddRange(Model.VehicleTrackList.Where(x => x.title.Trim().ToLower() == Vehicleno.Trim().ToLower()).ToList());
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        //Create Temp Table To Get Data
        public void OutstandingReports(bool Customer, string AccountList, string FromDate, string ToDate, string Branch)
        {
            #region Set Parameters
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
            mpara = "";

            #endregion

            FromDate = (Convert.ToDateTime(FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            ToDate = (Convert.ToDateTime(ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand();// ("SPTFAT_ReceivableAnalysis", tfat_conx);
            cmd.Connection = tfat_conx;

            ExecuteStoredProc("Drop Table Dztmp_zOS");
            ExecuteStoredProc("Drop Table ztmp_zOS");
            cmd.CommandText = "SPTFAT_ReceivableAnalysis";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@mBaseGr", SqlDbType.VarChar).Value = "D";
            cmd.Parameters.Add("@mDate", SqlDbType.Date).Value = ToDate;
            cmd.Parameters.Add("@mDate2", SqlDbType.Date).Value = FromDate;
            cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = Branch;
            cmd.Parameters.Add("@mLocations", SqlDbType.VarChar).Value = Fieldoftable("Warehouse", "Code", "Name='" + ppara01 + "'");
            cmd.Parameters.Add("@mConsiderCRPR", SqlDbType.Bit).Value = ppara06 == "Yes" ? true : false;
            cmd.Parameters.Add("@mSuppressZero", SqlDbType.Bit).Value = ppara08 == "Yes" ? true : false;
            cmd.Parameters.Add("@mArea", SqlDbType.VarChar).Value = "";// ppara04;
            cmd.Parameters.Add("@mParties", SqlDbType.VarChar).Value = AccountList == null ? "" : AccountList;
            cmd.Parameters.Add("@mGrps", SqlDbType.VarChar).Value = Fieldoftable("MasterGroups", "Code", "Name='" + ppara16 + "'");
            cmd.Parameters.Add("@mSalesman", SqlDbType.VarChar).Value = ppara02;//FieldoftableNumber("Salesman", "Code", "Name='" + 
            cmd.Parameters.Add("@mBroker", SqlDbType.VarChar).Value = ppara03;//FieldoftableNumber("Broker", "Code", "Name='" + ppara03 + "'");
            cmd.Parameters.Add("@mCategory", SqlDbType.VarChar).Value = ppara05;
            cmd.Parameters.Add("@mRefTillDate", SqlDbType.Bit).Value = true;
            cmd.Parameters.Add("@mDocString", SqlDbType.VarChar).Value = mDocString;
            cmd.Parameters.Add("@mCustomer", SqlDbType.VarChar).Value = Customer == true ? "1" : "0";

            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            tfat_conx.Close();
            tfat_conx.Dispose();

            ExecuteStoredProc("select * into Dztmp_zOS from ztmp_zOS");

        }
        public void OutstandingPayableReports(bool Customer, string AccountList, string FromDate, string ToDate, string Branch)
        {
            #region Set Parameters
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
            mpara = "";

            #endregion

            FromDate = (Convert.ToDateTime(FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            ToDate = (Convert.ToDateTime(ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand();// ("SPTFAT_ReceivableAnalysis", tfat_conx);
            cmd.Connection = tfat_conx;

            ExecuteStoredProc("Drop Table PayDztmp_zOS");
            ExecuteStoredProc("Drop Table ztmp_zOS");
            cmd.CommandText = "SPTFAT_ReceivableAnalysis";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@mBaseGr", SqlDbType.VarChar).Value = "S";
            cmd.Parameters.Add("@mDate", SqlDbType.Date).Value = ToDate;
            cmd.Parameters.Add("@mDate2", SqlDbType.Date).Value = FromDate;
            cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = Branch;
            cmd.Parameters.Add("@mLocations", SqlDbType.VarChar).Value = Fieldoftable("Warehouse", "Code", "Name='" + ppara01 + "'");
            cmd.Parameters.Add("@mConsiderCRPR", SqlDbType.Bit).Value = ppara06 == "Yes" ? true : false;
            cmd.Parameters.Add("@mSuppressZero", SqlDbType.Bit).Value = ppara08 == "Yes" ? true : false;
            cmd.Parameters.Add("@mArea", SqlDbType.VarChar).Value = "";// ppara04;
            cmd.Parameters.Add("@mParties", SqlDbType.VarChar).Value = AccountList == null ? "" : AccountList;
            cmd.Parameters.Add("@mGrps", SqlDbType.VarChar).Value = Fieldoftable("MasterGroups", "Code", "Name='" + ppara16 + "'");
            cmd.Parameters.Add("@mSalesman", SqlDbType.VarChar).Value = ppara02;//FieldoftableNumber("Salesman", "Code", "Name='" + 
            cmd.Parameters.Add("@mBroker", SqlDbType.VarChar).Value = ppara03;//FieldoftableNumber("Broker", "Code", "Name='" + ppara03 + "'");
            cmd.Parameters.Add("@mCategory", SqlDbType.VarChar).Value = ppara05;
            cmd.Parameters.Add("@mRefTillDate", SqlDbType.Bit).Value = true;
            cmd.Parameters.Add("@mDocString", SqlDbType.VarChar).Value = mDocString;
            cmd.Parameters.Add("@mCustomer", SqlDbType.VarChar).Value = Customer == true ? "1" : "0";

            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            tfat_conx.Close();
            tfat_conx.Dispose();

            ExecuteStoredProc("select * into PayDztmp_zOS from ztmp_zOS");

        }

        public string Createztmp_zOS(GridOption Model)
        {
            string Query = "";

            string FilterOnCode = "";
            if (Model.Code == "Payable")//Payable
            {
                if (!string.IsNullOrEmpty(Model.SundryCreditorsFilterGroups))
                {
                    FilterOnCode = " l.code in (select M.Code From Master M where  ";
                    var SplitOtherPostType = Model.SundryCreditorsFilterGroups.Split('^');
                    foreach (var item in SplitOtherPostType)
                    {
                        if (item == "B")
                        {
                            FilterOnCode += " M.OthPostType like '%B%' or";
                        }
                        else if (item == "H")
                        {
                            FilterOnCode += " M.OthPostType like '%V%' or";
                        }
                        else if (item == "D")
                        {
                            FilterOnCode += " M.OthPostType like '%D%' or";
                        }
                        else if (item == "V")
                        {
                            FilterOnCode = " l.code in (select M.Code From Master M where (M.BaseGr ='U' or  M.BaseGr ='S' ) and len(M.OthPostType)=0  or";
                        }

                    }
                    FilterOnCode = FilterOnCode.Substring(0, FilterOnCode.Length - 2) + ") ";
                }
                else if (Model.ARAPReqOnly)
                {
                    FilterOnCode = " l.code in (select M.Code From Master M where M.ARAP ='true') ";
                }
            }
            else//Receivable
            {
                if (!Model.Customer)
                {
                    FilterOnCode = (string.IsNullOrEmpty(Model.Code) == false ? " Charindex(l.code,'" + Model.Code + "')<>0 " : "1=1");
                }
                else
                {
                    FilterOnCode = (string.IsNullOrEmpty(Model.Code) == false ? " Charindex(l.party,'" + Model.Code + "')<>0 " : "1=1");
                }
            }

            if (Model.Customer == false)
            {
                if (Model.BillDetails)
                {
                    Query = "Select distinct l.RefDoc as RefDoc,l.Branch,l.ParentKey,l.TableKey,l.Type,l.Prefix,l.Srl,l.Sno,l.DocDate,l.Code,m.Name,l.BillNumber,l.BillDate,l.Narr as Narr, "
                            + " x.intrate as InterestRate,l.CrPeriod,x.CrPeriod as MasterCrPeriod, l.DocDate + l.CrPeriod as DueDate, Datediff(d, l.DocDate + l.CrPeriod, Getdate()) as oDueDays,isnull(BillS.Remark, '') as BillRemark,isnull(BillS.Through, '') as BillThrough,Bills.SubDt as BillSubDate, "
                            + "	(Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' then(Select s.BillNumber from Sales s where s.TableKey = l.ParentKey and s.Branch = l.Branch) Else(Select s.BillNumber from Purchase s where s.TableKey = l.ParentKey and s.Branch = l.Branch) End) as OrdNumber, "
                            + "	(Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' then(Select s.BillDate from Sales s where s.TableKey = l.ParentKey and s.Branch = l.Branch) Else(Select s.BillDate from Purchase s where s.TableKey = l.ParentKey and s.Branch = l.Branch) End) as OrdDate, "
                            + "	Pending = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + Model.ToDate + "', l.Branch) end) else 0 End), "
                            + "	OnAccount = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U1') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey,'" + Model.ToDate + "', l.Branch) end) else 0 end), "
                            + "	Amt = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 End), "
                            + "	UnAdj = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 end), "
                            + "	Flag = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then 0 else 1 End), "
                            + "	a.Adrl1,a.Adrl2,a.Adrl3,a.City,a.State,a.Pin,a.Country,a.Person,m.Category,(Select top 1 Name from PartyCategory where PartyCategory.Code = m.Category) as CategoryName, "
                            + "	m.SalesMan,isnull((Select top 1 Name from Salesman where Salesman.Code = m.Salesman),'~') as SalesManName, "
                            + "	a.Area,(Select top 1 Name from AreaMaster where AreaMaster.Code = a.Area) as AreaName, "
                            + "	m.Grp,(Select top 1 Name from MasterGroups where MasterGroups.Code = m.Grp) as GrpName,l.LocationCode, "
                            + "	m.Broker,isnull((Select top 1 Name from Broker where Broker.Code = m.Broker),'~') as BrokerName,l.party as Party, "
                            + "	(Select SUBSTRING((Select ',[' + Stock.Code + '] ' + Itemmaster.name AS 'data()' FROM Stock, Itemmaster where Stock.code = Itemmaster.code and Stock.Parentkey = l.Parentkey FOR XML PATH('')), 2, 9999)) as Itemdetails "
                            + "   into ztmp_zOS from Ledger l "
                            + "   left join Master m on m.Code = l.Code  "
                            + "   left join MasterInfo x on l.code = x.code  "
                            + "   left join Address a on a.Code = l.Code and a.Sno = 0 "
                            + "   left join BillSubRef BillRef on BillRef.BillBranch + BillRef.BillTableKey = l.Branch + l.TableKey left join BillSubmission BillS on BillS.DocNo = BillRef.DocNo "
                            + "   where  " + (string.IsNullOrEmpty(Model.Code) == false ? " Charindex(l.code,'" + Model.Code + "')<>0 " : "1=1") + " and  (m.BaseGr = 'U' or m.BaseGr = " + (Model.MainType == "SL" ? "'D'" : "'S'") + ") and Charindex(l.Branch, '" + ppara04 + "')<> 0 and l.MainType <> 'MV' and l.MainType <> 'PV' and "
                            + "   (case when 0 <> 0 then l.DocDate + (Case when l.crperiod = null then 0 else l.crperiod end) else l.DocDate end) between '" + Model.FromDate + "' and '" + Model.ToDate + "'";
                }
                else
                {
                    Query = "Select distinct l.RefDoc as RefDoc,l.Branch,l.ParentKey,l.TableKey,l.Type,l.Prefix,l.Srl,l.Sno,l.DocDate,l.Code,m.Name,l.BillNumber,l.BillDate,l.Narr as Narr, "
                            + " x.intrate as InterestRate,l.CrPeriod,x.CrPeriod as MasterCrPeriod, l.DocDate + l.CrPeriod as DueDate, Datediff(d, l.DocDate + l.CrPeriod, Getdate()) as oDueDays,'' as BillRemark,'' as BillThrough,GETDATE() as BillSubDate, "
                            + " (Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' then(Select s.BillNumber from Sales s where s.TableKey = l.ParentKey and s.Branch = l.Branch) Else(Select s.BillNumber from Purchase s where s.TableKey = l.ParentKey and s.Branch = l.Branch) End) as OrdNumber, "
                            + " (Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' then(Select s.BillDate from Sales s where s.TableKey = l.ParentKey and s.Branch = l.Branch) Else(Select s.BillDate from Purchase s where s.TableKey = l.ParentKey and s.Branch = l.Branch) End)  as OrdDate, "
                            + " Pending = (Case when(Debit <> 0 and (" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + Model.ToDate + "', l.Branch) end) else 0 End), "
                            + " OnAccount = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U1') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + Model.ToDate + "', l.Branch) end) else 0 end), "
                            + " Amt = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 End), "
                            + " UnAdj = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 end), "
                            + " Flag = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then 0 else 1 End),a.Adrl1,a.Adrl2,a.Adrl3,a.City,a.State,a.Pin,a.Country,a.Person,m.Category,(Select top 1 Name from PartyCategory where PartyCategory.Code = m.Category) as CategoryName, "
                            + " m.SalesMan,isnull((Select top 1 Name from Salesman where Salesman.Code = m.Salesman),'~')  as SalesManName, "
                            + " a.Area,(Select top 1 Name from AreaMaster where AreaMaster.Code = a.Area) as AreaName, "
                            + " m.Grp,(Select top 1 Name from MasterGroups where MasterGroups.Code = m.Grp) as GrpName,l.LocationCode, "
                            + " m.Broker,isnull((Select top 1 Name from Broker where Broker.Code = m.Broker),'~') as BrokerName,l.party as Party,(Select SUBSTRING((Select ',[' + Stock.Code + '] ' + Itemmaster.name AS 'data()' FROM Stock, Itemmaster where Stock.code = Itemmaster.code and Stock.Parentkey = l.Parentkey FOR XML PATH('')), 2, 9999)) as Itemdetails "
                            + "   into ztmp_zOS from Ledger l  "
                            + "   left join Master m on m.Code = l.Code  "
                            + "   left join MasterInfo x on l.code = x.code  "
                            + "   left join Address a on a.Code = l.Code and a.Sno = 0 "
                            + "   where " + FilterOnCode + " and (m.BaseGr = 'U' or m.BaseGr = " + (Model.MainType == "SL" ? "'D'" : "'S'") + ") and Charindex(l.Branch, '" + ppara04 + "')<> 0 and l.MainType <> 'MV' and l.MainType <> 'PV' and "
                            + "   (case when 0 <> 0 then l.DocDate + (Case when l.crperiod = null then 0 else l.crperiod end) else l.DocDate end) between '" + Model.FromDate + "' and '" + Model.ToDate + "'";
                }
            }
            else
            {
                if (Model.BillDetails)
                {
                    Query = "Select distinct l.RefDoc as RefDoc,l.Branch,l.ParentKey,l.TableKey,l.Type,l.Prefix,l.Srl,l.Sno,l.DocDate,c.Code,c.Name,l.BillNumber,l.BillDate,l.Narr as Narr,isnull(BillS.Remark,'') as BillRemark,isnull(BillS.Through,'') as BillThrough,Bills.SubDt as BillSubDate, "
                            + " x.intrate as InterestRate,l.CrPeriod,x.CrPeriod as MasterCrPeriod, l.DocDate + l.CrPeriod as DueDate, Datediff(d, l.DocDate + l.CrPeriod, Getdate()) as oDueDays, "
                            + " (Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' then(Select s.BillNumber from Sales s where s.TableKey = l.ParentKey and s.Branch = l.Branch) Else(Select s.BillNumber from Purchase s where s.TableKey = l.ParentKey and s.Branch = l.Branch) End) as OrdNumber, "
                            + " (Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' then(Select s.BillDate from Sales s where s.TableKey = l.ParentKey and s.Branch = l.Branch) Else(Select s.BillDate from Purchase s where s.TableKey = l.ParentKey and s.Branch = l.Branch) End) as OrdDate, "
                            + " Pending = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + Model.ToDate + "', l.Branch) end) else 0 End), "
                            + " OnAccount = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U1') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + Model.ToDate + "', l.Branch) end) else 0 end), "
                            + " Amt = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 End), "
                            + " UnAdj = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 end), "
                            + " Flag = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then 0 else 1 End), "
                            + " a.Adrl1,a.Adrl2,a.Adrl3,a.City,a.State,a.Pin,a.Country,a.Person,m.Category,(Select top 1 Name from PartyCategory where PartyCategory.Code = m.Category) as CategoryName, "
                            + " m.SalesMan,isnull((Select top 1 Name from Salesman where Salesman.Code = m.Salesman),'~') as SalesManName, "
                            + " a.Area,(Select top 1 Name from AreaMaster where AreaMaster.Code = a.Area) as AreaName, "
                            + " m.Grp,(Select top 1 Name from MasterGroups where MasterGroups.Code = m.Grp) as GrpName,l.LocationCode, "
                            + " m.Broker,isnull((Select top 1 Name from Broker where Broker.Code = m.Broker),'~') as BrokerName,l.party as Party, "
                            + " (Select SUBSTRING((Select ',[' + Stock.Code + '] ' + Itemmaster.name AS 'data()' FROM Stock, Itemmaster where Stock.code = Itemmaster.code and Stock.Parentkey = l.Parentkey FOR XML PATH('')), 2, 9999)) as Itemdetails "
                            + "   into ztmp_zOS from Ledger l "
                            + "   left join Master m on m.Code = l.Code  "
                            + "   left join customerMaster c on c.Code = l.Party  "
                            + "   left join CMasterInfo x on l.code = x.code  "
                            + "   left join Address a on a.Code = l.Code and a.Sno = 0 left join BillSubRef BillRef on BillRef.BillBranch + BillRef.BillTableKey = l.Branch + l.TableKey left join BillSubmission BillS on BillS.DocNo = BillRef.DocNo "
                            + "   where " + (string.IsNullOrEmpty(Model.Code) == false ? " Charindex(l.party,'" + Model.Code + "')<>0 " : "1=1") + " and (m.BaseGr = 'U' or m.BaseGr = " + (Model.MainType == "SL" ? "'D'" : "'S'") + ") and Charindex(l.Branch, '" + ppara04 + "')<> 0 and l.MainType <> 'MV' and l.MainType <> 'PV' and "
                            + "   (case when 0 <> 0 then l.DocDate + (Case when l.crperiod = null then 0 else l.crperiod end) else l.DocDate end) between '" + Model.FromDate + "' and '" + Model.ToDate + "'";
                }
                else
                {
                    Query = "Select distinct l.RefDoc as RefDoc,l.Branch,l.ParentKey,l.TableKey,l.Type,l.Prefix,l.Srl,l.Sno,l.DocDate,c.Code,c.Name,l.BillNumber,l.BillDate,l.Narr as Narr,'' as BillRemark,'' as BillThrough,GETDATE() as BillSubDate, "
                            + " x.intrate as InterestRate,l.CrPeriod,x.CrPeriod as MasterCrPeriod, l.DocDate + l.CrPeriod as DueDate, Datediff(d, l.DocDate + l.CrPeriod, Getdate()) as oDueDays, "
                            + " (Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' then(Select s.BillNumber from Sales s where s.TableKey = l.ParentKey and s.Branch = l.Branch) Else(Select s.BillNumber from Purchase s where s.TableKey = l.ParentKey and s.Branch = l.Branch) End) as OrdNumber, "
                            + " (Case When " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' then(Select s.BillDate from Sales s where s.TableKey = l.ParentKey and s.Branch = l.Branch) Else(Select s.BillDate from Purchase s where s.TableKey = l.ParentKey and s.Branch = l.Branch) End) as OrdDate, "
                            + " Pending = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + Model.ToDate + "', l.Branch) end) else 0 End), "
                            + " OnAccount = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U1') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(case when 1 <> 0 then dbo.GetOutstanding(l.TableKey, l.Branch) else dbo.GetOutstandingDate(l.TableKey, '" + Model.ToDate + "', l.Branch) end) else 0 end), "
                            + " Amt = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 End), "
                            + " UnAdj = (Case when(Debit <> 0 and " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U') or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then(l.Debit + l.Credit) else 0 end), "
                            + " Flag = (Case when(Debit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'D' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) or(Credit <> 0 and(" + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'S' or " + (Model.MainType == "SL" ? "'D'" : "'S'") + " = 'U')) then 0 else 1 End), "
                            + " a.Adrl1,a.Adrl2,a.Adrl3,a.City,a.State,a.Pin,a.Country,a.Person,m.Category,(Select top 1 Name from PartyCategory where PartyCategory.Code = m.Category) as CategoryName, "
                            + " m.SalesMan,isnull((Select top 1 Name from Salesman where Salesman.Code = m.Salesman),'~') as SalesManName, "
                            + " a.Area,(Select top 1 Name from AreaMaster where AreaMaster.Code = a.Area) as AreaName, "
                            + " m.Grp,(Select top 1 Name from MasterGroups where MasterGroups.Code = m.Grp) as GrpName,l.LocationCode, "
                            + " m.Broker,isnull((Select top 1 Name from Broker where Broker.Code = m.Broker),'~') as BrokerName,l.party as Party, "
                            + " (Select SUBSTRING((Select ',[' + Stock.Code + '] ' + Itemmaster.name AS 'data()' FROM Stock, Itemmaster where Stock.code = Itemmaster.code and Stock.Parentkey = l.Parentkey FOR XML PATH('')), 2, 9999)) as Itemdetails "
                            + "  into ztmp_zOS from Ledger l "
                            + "  left join Master m on m.Code = l.Code  "
                            + "  left join customerMaster c on c.Code = l.Party  "
                            + "  left join CMasterInfo x on l.code = x.code  "
                            + "  left join Address a on a.Code = l.Code and a.Sno = 0 "
                            + "  where " + FilterOnCode + " and (m.BaseGr = 'U' or m.BaseGr = " + (Model.MainType == "SL" ? "'D'" : "'S'") + ") and Charindex(l.Branch, '" + ppara04 + "')<> 0 and l.MainType <> 'MV' and l.MainType <> 'PV' and "
                            + "  (case when 0 <> 0 then l.DocDate + (Case when l.crperiod = null then 0 else l.crperiod end) else l.DocDate end) between '" + Model.FromDate + "' and '" + Model.ToDate + "'";
                }
            }
            ExecuteStoredProc("DROP TABLE IF EXISTS ztmp_zOS");
            ExecuteStoredProc(Query);
            if (Model.Supress)
            {
                ExecuteStoredProc("Delete from ztmp_zOS where Pending=0 and OnAccount=0;");
            }

            return "";
        }



        public void UnbillConsignment(string Branch)
        {
            DataTable dt = new DataTable();
            ExecuteStoredProc("Drop Table Dztmp_zUnBillLR");
            string Query = "";
            Query = "select LR.LrNo as LrNo,Datediff(d,LR.BookDate,Getdate()) as oDueDays,LR.Amt  into Dztmp_zUnBillLR from LRMaster LR where  (abs(LR.TotQty) - abs(isnull((Select sum(LRBill.TotQty) from LRBill Where LRBill.LRRefTablekey = LR.Tablekey),0)))>0 and Charindex(LR.Branch,'" + Branch + "')<>0";
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand(Query, tfat_conx);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.Fill(dt);
            tfat_conx.Close();
            tfat_conx.Dispose();
        }
        public void ConsignmentStock(string Branch)
        {
            Branch = Branch + ",G00000";
            DataTable dt = new DataTable();
            ExecuteStoredProc("Drop Table Dztmp_zStockLR");
            string Query = "";
            Query = "select ISNULL(l.ChgWt,0) as ChgWt,ISNULL(l.DecVal,0) as DecVal,l.LrNo,Datediff(d,l.BookDate,Getdate()) as oDueDays,case when ( select T.Category from TfatBranch T where T.Code=  LRSTK.branch)='Area' then ( select T.Grp from TfatBranch T where T.Code=  LRSTK.branch) else LRSTK.branch end  as Branch," +
            " case when LRSTK.TotalQty = 0 then 0 else (LRSTK.TotalQty - (((select  ISNULL(SUM(LRT.TotalQty), 0) from LRStock LRT where LRT.ParentKey = LRSTK.TableKey))   ) ) end as BalQty" +
            " into Dztmp_zStockLR from lrmaster l full outer join LRStock LRSTK on LRSTK.LrNo = l.LrNo" +
            " where LRSTK.type <> 'DEL' and LRSTK.type <> 'TRN' and(case when LRSTK.TotalQty = 0 then 0 else (LRSTK.TotalQty - (((select  ISNULL(SUM(LRT.TotalQty), 0) from LRStock LRT where LRT.ParentKey = LRSTK.TableKey))   ) ) end ) <> 0 ";
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand(Query, tfat_conx);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.Fill(dt);
            tfat_conx.Close();
            tfat_conx.Dispose();
            if (!String.IsNullOrEmpty(Branch))
            {
                ExecuteStoredProc("delete from Dztmp_zStockLR where Dztmp_zStockLR.LrNo not in (select X.LrNo from Dztmp_zStockLR X where Charindex(X.Branch,'" + Branch + "')<>0)");
            }
        }
        public void ConsignmentStockTRN(string Branch)
        {
            Branch = Branch + ",G00000";
            DataTable dt = new DataTable();
            ExecuteStoredProc("Drop Table Dztmp_zStockLRTRN");
            string Query = "";
            Query = "select ISNULL(l.ChgWt,0) as ChgWt,ISNULL(l.DecVal,0) as DecVal,l.LrNo,Datediff(d,l.BookDate,Getdate()) as oDueDays,case when ( select T.Category from TfatBranch T where T.Code=  LRSTK.branch)='Area' then ( select T.Grp from TfatBranch T where T.Code=  LRSTK.branch) else LRSTK.branch end  as Branch," +
            " case when LRSTK.TotalQty = 0 then 0 else (LRSTK.TotalQty - (((select  ISNULL(SUM(LRT.TotalQty), 0) from LRStock LRT where LRT.ParentKey = LRSTK.TableKey))   ) ) end as BalQty" +
            " into Dztmp_zStockLRTRN from lrmaster l full outer join LRStock LRSTK on LRSTK.LrNo = l.LrNo" +
            " where LRSTK.type <> 'DEL' and LRSTK.type = 'TRN' and(case when LRSTK.TotalQty = 0 then 0 else (LRSTK.TotalQty - (((select  ISNULL(SUM(LRT.TotalQty), 0) from LRStock LRT where LRT.ParentKey = LRSTK.TableKey))   ) ) end ) <> 0 ";
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand(Query, tfat_conx);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.Fill(dt);
            tfat_conx.Close();
            tfat_conx.Dispose();
            if (!String.IsNullOrEmpty(Branch))
            {
                ExecuteStoredProc("delete from Dztmp_zStockLRTRN where Dztmp_zStockLRTRN.LrNo not in (select X.LrNo from Dztmp_zStockLRTRN X where Charindex(X.Branch,'" + Branch + "')<>0)");
            }
        }
        public void ConsignmentBooking(string Branch, string BillAmt, string FromDate, string ToDate)
        {
            FromDate = (Convert.ToDateTime(FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            ToDate = (Convert.ToDateTime(ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

            DataTable dt = new DataTable();
            ExecuteStoredProc("Drop Table Dztmp_zBookLR");
            string Query = "";
            if (BillAmt == "T")
            {
                Query = " select l.branch as Branch, (select Y.Name From TfatBranch Y where Y.code=l.branch ) as [BranchName],'' as Code, '' as [Name],      Cast(Sum(Case when BookDate>='" + FromDate + "' and BookDate<=EOMonth('" + FromDate + "') then lr.Amt Else 0 End) as Decimal(14,2))  as [Aprl],   Cast(Sum(Case when BookDate>=DateAdd(m,1,'" + FromDate + "') and BookDate<=EOMonth(DateAdd(m,1,'" + FromDate + "')) then lr.Amt else 0 End) as Decimal(14,2))  as [May],   Cast(Sum(Case when BookDate>=DateAdd(m,2,'" + FromDate + "') and BookDate<=EOMonth(DateAdd(m,2,'" + FromDate + "')) then lr.Amt else 0 End) as Decimal(14,2))  as [Jun],   Cast(Sum(Case when BookDate>=DateAdd(m,3,'" + FromDate + "') and BookDate<=EOMonth(DateAdd(m,3,'" + FromDate + "')) then lr.Amt else 0 End) as Decimal(14,2))  as [Jul],    Cast(Sum(Case when BookDate>=DateAdd(m,4,'" + FromDate + "') and BookDate<=EOMonth(DateAdd(m,4,'" + FromDate + "')) then lr.Amt else 0 End) as Decimal(14,2))  as [Aug],    Cast(Sum(Case when BookDate>=DateAdd(m,5,'" + FromDate + "') and BookDate<=EOMonth(DateAdd(m,5,'" + FromDate + "')) then lr.Amt else 0 End) as Decimal(14,2))  as [Sept],    Cast(Sum(Case when BookDate>=DateAdd(m,6,'" + FromDate + "') and BookDate<=EOMonth(DateAdd(m,6,'" + FromDate + "')) then lr.Amt else 0 End) as Decimal(14,2))  as [Oct],    Cast(Sum(Case when BookDate>=DateAdd(m,7,'" + FromDate + "') and BookDate<=EOMonth(DateAdd(m,7,'" + FromDate + "')) then lr.Amt else 0 End) as Decimal(14,2))  as [Nov],    Cast(Sum(Case when BookDate>=DateAdd(m,8,'" + FromDate + "') and BookDate<=EOMonth(DateAdd(m,8,'" + FromDate + "')) then lr.Amt else 0 End) as Decimal(14,2))  as [Dec],    Cast(Sum(Case when BookDate>=DateAdd(m,9,'" + FromDate + "') and BookDate<=EOMonth(DateAdd(m,9,'" + FromDate + "')) then lr.Amt else 0 End) as Decimal(14,2))  as [Jan],   Cast(Sum(Case when BookDate>=DateAdd(m,10,'" + FromDate + "') and BookDate<=EOMonth(DateAdd(m,10,'" + FromDate + "')) then lr.Amt else 0 End) as Decimal(14,2))  as [Feb],   Cast(Sum(Case when BookDate>=DateAdd(m,11,'" + FromDate + "') and BookDate<=EOMonth(DateAdd(m,11,'" + FromDate + "')) then lr.Amt else 0 End) as Decimal(14,2))  as [Mar]" +
                        "  into Dztmp_zBookLR from lrmaster l left" +
                        "  join LRBill lr  on l.lrno = lr.lrno" +
                        " where Charindex(l.Branch, '" + Branch + "') <> 0 and l.BookDate <= '" + ToDate + "' Group by l.branch ";
            }
            else
            {
                Query = " select l.branch as Branch, (select Y.Name From TfatBranch Y where Y.code=l.branch ) as [BranchName],'' as Code, '' as [Name],      Cast(Sum(Case when BookDate>='" + FromDate + "' and BookDate<=EOMonth('" + FromDate + "') then l.Amt Else 0 End) as Decimal(14,2))  as [Aprl],   Cast(Sum(Case when BookDate>=DateAdd(m,1,'" + FromDate + "') and BookDate<=EOMonth(DateAdd(m,1,'" + FromDate + "')) then l.Amt else 0 End) as Decimal(14,2))  as [May],   Cast(Sum(Case when BookDate>=DateAdd(m,2,'" + FromDate + "') and BookDate<=EOMonth(DateAdd(m,2,'" + FromDate + "')) then l.Amt else 0 End) as Decimal(14,2))  as [Jun],   Cast(Sum(Case when BookDate>=DateAdd(m,3,'" + FromDate + "') and BookDate<=EOMonth(DateAdd(m,3,'" + FromDate + "')) then l.Amt else 0 End) as Decimal(14,2))  as [Jul],    Cast(Sum(Case when BookDate>=DateAdd(m,4,'" + FromDate + "') and BookDate<=EOMonth(DateAdd(m,4,'" + FromDate + "')) then l.Amt else 0 End) as Decimal(14,2))  as [Aug],    Cast(Sum(Case when BookDate>=DateAdd(m,5,'" + FromDate + "') and BookDate<=EOMonth(DateAdd(m,5,'" + FromDate + "')) then l.Amt else 0 End) as Decimal(14,2))  as [Sept],    Cast(Sum(Case when BookDate>=DateAdd(m,6,'" + FromDate + "') and BookDate<=EOMonth(DateAdd(m,6,'" + FromDate + "')) then l.Amt else 0 End) as Decimal(14,2))  as [Oct],    Cast(Sum(Case when BookDate>=DateAdd(m,7,'" + FromDate + "') and BookDate<=EOMonth(DateAdd(m,7,'" + FromDate + "')) then l.Amt else 0 End) as Decimal(14,2))  as [Nov],    Cast(Sum(Case when BookDate>=DateAdd(m,8,'" + FromDate + "') and BookDate<=EOMonth(DateAdd(m,8,'" + FromDate + "')) then l.Amt else 0 End) as Decimal(14,2))  as [Dec],    Cast(Sum(Case when BookDate>=DateAdd(m,9,'" + FromDate + "') and BookDate<=EOMonth(DateAdd(m,9,'" + FromDate + "')) then l.Amt else 0 End) as Decimal(14,2))  as [Jan],   Cast(Sum(Case when BookDate>=DateAdd(m,10,'" + FromDate + "') and BookDate<=EOMonth(DateAdd(m,10,'" + FromDate + "')) then l.Amt else 0 End) as Decimal(14,2))  as [Feb],   Cast(Sum(Case when BookDate>=DateAdd(m,11,'" + FromDate + "') and BookDate<=EOMonth(DateAdd(m,11,'" + FromDate + "')) then l.Amt else 0 End) as Decimal(14,2))  as [Mar]" +
                        "  into Dztmp_zBookLR from lrmaster l left" +
                        "  join LRBill lr  on l.lrno = lr.lrno" +
                        " where Charindex(l.Branch, '" + Branch + "') <> 0 and l.BookDate <= '" + ToDate + "' Group by l.branch ";

            }

            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand(Query, tfat_conx);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.Fill(dt);
            tfat_conx.Close();
            tfat_conx.Dispose();

        }
        //Create Temp Table To Get Data



        public JsonResult MultiBarChartDataEF(string CODE)
        {
            DataTable dt = new DataTable();
            string Query = "";
            if (CODE == "Outstanding")
            {
                Query = "select  Cast(sum((Case when ODueDays between 0 and 30 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [0-30], Cast(sum((Case when ODueDays between 30+1  and 60 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [30-60], Cast(sum((Case when ODueDays between 60+1  and 90 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [60-90], Cast(sum((Case when ODueDays between 90+1  and 120 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [90-120], Cast(sum((Case when ODueDays between 120+1  and 150 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [120-150], Cast(Sum((Case when ODueDays between 150+1  and 180 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [150-180], Cast(sum((Case when ODueDays>180 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [>180] from Dztmp_zOS";
            }
            else if (CODE == "Payable")
            {
                Query = "select  Cast(sum((Case when ODueDays between 0 and 30 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [0-30], Cast(sum((Case when ODueDays between 30+1  and 60 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [30-60], Cast(sum((Case when ODueDays between 60+1  and 90 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [60-90], Cast(sum((Case when ODueDays between 90+1  and 120 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [90-120], Cast(sum((Case when ODueDays between 120+1  and 150 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [120-150], Cast(Sum((Case when ODueDays between 150+1  and 180 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [150-180], Cast(sum((Case when ODueDays>180 then (Pending-OnAccount ) else 0 end)) as Decimal(14,2)) as [>180] from PayDztmp_zOS";
            }
            else if (CODE == "UnBillConsignmet")
            {
                Query = "select top 1" +
                        " (SELECT COUNT(*) FROM Dztmp_zUnBillLR ct    WHERE ct.oDueDays Between 0 and 30) as [0-30]," +
                        " (SELECT COUNT(*) FROM Dztmp_zUnBillLR ct WHERE ct.oDueDays Between 31 and 60) as [30-60]," +
                        " (SELECT COUNT(*) FROM Dztmp_zUnBillLR ct WHERE ct.oDueDays Between 61 and 90) as [60-90]," +
                        " (SELECT COUNT(*) FROM Dztmp_zUnBillLR ct WHERE ct.oDueDays Between 91 and 120) as [90-120]," +
                        " (SELECT COUNT(*) FROM Dztmp_zUnBillLR ct WHERE ct.oDueDays > 120) as [>120]" +
                        " from Dztmp_zUnBillLR ";
            }
            else if (CODE == "ConsignmetStock")
            {
                Query = "select top 1" +
                        " (SELECT COUNT(*) FROM Dztmp_zStockLR ct    WHERE ct.oDueDays Between 0 and 30) as [0-30]," +
                        " (SELECT COUNT(*) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 31 and 60) as [30-60]," +
                        " (SELECT COUNT(*) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 61 and 90) as [60-90]," +
                        " (SELECT COUNT(*) FROM Dztmp_zStockLR ct WHERE ct.oDueDays Between 91 and 120) as [90-120]," +
                        " (SELECT COUNT(*) FROM Dztmp_zStockLR ct WHERE ct.oDueDays > 120) as [>120]" +
                        " from Dztmp_zStockLR ";
            }
            else if (CODE == "ConsignmeTRNStock")
            {
                Query = "select top 1" +
                        " (SELECT COUNT(*) FROM Dztmp_zStockLRTRN ct    WHERE ct.oDueDays Between 0 and 30) as [0-30]," +
                        " (SELECT COUNT(*) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 31 and 60) as [30-60]," +
                        " (SELECT COUNT(*) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 61 and 90) as [60-90]," +
                        " (SELECT COUNT(*) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays Between 91 and 120) as [90-120]," +
                        " (SELECT COUNT(*) FROM Dztmp_zStockLRTRN ct WHERE ct.oDueDays > 120) as [>120]" +
                        " from Dztmp_zStockLRTRN ";
            }
            else if (CODE == "ConsignmetBook")
            {
                Query = "select top 1" +
                        " SUM(Aprl) as Aprl,SUM(May) as May,SUM(Jun) as Jun,SUM(Jul) as Jul,SUM(Aug) as Aug," +
                        " SUM(Sept) as Sept,SUM(Oct) as Oct,SUM(Nov) as Nov,SUM(Dec) as Dec,SUM(Jan) as Jan," +
                        " SUM(Feb) as Feb,SUM(Mar) as Mar" +
                        " from Dztmp_zBookLR ";
            }
            else if (CODE == "TopCustomers")
            {
                Query = " Select Name,Amt from Top10Customer ";
            }
            else if (CODE == "TopGroupCustomers")
            {
                Query = " Select Name,Amt from Top10GroupCustomer ";
            }
            else if (CODE == "TopVendors")
            {
                Query = " Select Name,Amt from Top10Vendor ";
            }
            else if (CODE == "TopExpenses")
            {
                Query = " Select Name,Amt from Top10Expenses ";
            }
            else if (CODE == "VehicleStatus")
            {
                Query = " Select Status,Total from Ztmp_VehicleStatus ";
            }
            else if (CODE == "DriverStatus")
            {
                Query = " Select Status,Total from Ztmp_DriverStatus1 ";
            }
            else if (CODE == "EwayBillDetails")
            {
                Query = " Select Active,TodayExp,TomorrowExp,Expird from Ztmp_EwayBillDetails ";
            }


            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand(Query, tfat_conx);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.Fill(dt);
            tfat_conx.Close();
            tfat_conx.Dispose();
            if (CODE == "VehicleStatus" || CODE == "DriverStatus")
            {
                #region FillData
                var BarChartData = new BarChartVM();
                var labels = new List<string>();
                var LastRow = dt.Columns.Count - 1;
                var datasets = new List<BarChartChildVM>();
                var childModel = new BarChartChildVM();

                childModel.backgroundColor = @"rgb(54 141 38 / 59%)";
                childModel.borderColor = @"rgba(255,99,132,1)";
                childModel.borderWidth = 2;
                childModel.hoverbackgroundColor = @"rgb(126 161 71)";
                childModel.hoverborderColor = @"rgba(255,99,132,1)";
                decimal TotalAmt = 0;
                var datalist = new List<decimal>();

                foreach (DataRow row in dt.Rows)
                {
                    labels.Add(row["Status"].ToString());
                    datalist.Add(Convert.ToInt32(row["Total"].ToString()));
                }
                childModel.label = "Total :- " + datalist.Sum().ToString();
                BarChartData.labels = labels;
                childModel.data = datalist;
                datasets.Add(childModel);

                BarChartData.datasets = datasets;
                #endregion

                return Json(BarChartData, JsonRequestBehavior.AllowGet);
            }

            else if (CODE == "TopCustomers" || CODE == "TopGroupCustomers" || CODE == "TopVendors" || CODE == "TopExpenses")
            {
                #region FillData
                var BarChartData = new BarChartVM();
                var labels = new List<string>();
                var LastRow = dt.Columns.Count - 1;
                var datasets = new List<BarChartChildVM>();
                var childModel = new BarChartChildVM();

                childModel.backgroundColor = @"rgb(54 141 38 / 59%)";
                childModel.borderColor = @"rgba(255,99,132,1)";
                childModel.borderWidth = 2;
                childModel.hoverbackgroundColor = @"rgb(126 161 71)";
                childModel.hoverborderColor = @"rgba(255,99,132,1)";
                decimal TotalAmt = 0;
                var datalist = new List<decimal>();

                foreach (DataRow row in dt.Rows)
                {
                    if (row["Name"].ToString().Length >= 10)
                    {
                        labels.Add(row["Name"].ToString().Substring(0, 10));
                    }
                    else
                    {
                        labels.Add(row["Name"].ToString());
                    }

                    datalist.Add(Convert.ToDecimal(row["Amt"].ToString()));
                }
                childModel.label = "Total :- " + datalist.Sum().ToString();
                BarChartData.labels = labels;
                childModel.data = datalist;
                datasets.Add(childModel);

                BarChartData.datasets = datasets;
                #endregion

                return Json(BarChartData, JsonRequestBehavior.AllowGet);
            }
            else
            {
                #region FillData
                var BarChartData = new BarChartVM();
                var labels = new List<string>();
                var LastRow = dt.Columns.Count - 1;
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    labels.Add(dt.Columns[i].ToString());
                }
                BarChartData.labels = labels;

                var datasets = new List<BarChartChildVM>();
                var childModel = new BarChartChildVM();

                childModel.backgroundColor = @"rgb(54 141 38 / 59%)";
                childModel.borderColor = @"rgba(255,99,132,1)";
                childModel.borderWidth = 2;
                childModel.hoverbackgroundColor = @"rgb(126 161 71)";
                childModel.hoverborderColor = @"rgba(255,99,132,1)";
                decimal TotalAmt = 0;
                var datalist1 = new List<decimal>();
                var datalist2 = new List<decimal>();
                var datalist3 = new List<decimal>();
                var datalist4 = new List<decimal>();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (dt.Rows.Count > 0)
                    {
                        var Amt = Convert.ToDecimal(dt.Rows[0][i].ToString() == "" ? "0" : dt.Rows[0][i]);
                        TotalAmt += Amt;
                        datalist1.Add(Amt);
                        childModel.label = "Total :- " + TotalAmt.ToString();
                    }

                }
                childModel.data = datalist1;
                datasets.Add(childModel);

                BarChartData.datasets = datasets;
                #endregion

                return Json(BarChartData, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetURLDocSideBar(string mdocument)
        {
            string murl = "";
            int MenuID = 0;
            ActiveSideBarObjects SideBar = ctxTFAT.ActiveSideBarObjects.Where(x => x.ID.ToString().Trim() == mdocument.Trim()).FirstOrDefault();
            if (SideBar != null)
            {
                if (SideBar.ZoomURL)
                {
                    if (SideBar.Name.ToUpper().Trim() == "BOOKING")
                    {
                        MenuID = 2377;
                        var mrights = ctxTFAT.UserRights.Where(z => z.Code == muserid && z.MenuID == MenuID).FirstOrDefault();
                        if (mrights != null || muserid.ToLower() == "super")
                        {
                            var FlagOfBillAmount = SideBar.Para1 == "T" ? true : false;
                            var mstr = ctxTFAT.TfatMenu.Select(x => new { x.ID, x.OptionCode, x.TableName, x.ModuleName, x.Menu, x.Controller, x.FormatCode, x.SubType, x.ParentMenu }).Where(z => z.ID == MenuID).FirstOrDefault();
                            murl = "/" + mstr.ParentMenu + "/" + mstr.Controller + "/Index?ViewDataId=" + HttpUtility.UrlEncode(mstr.FormatCode.Trim()) + "&Header=" + HttpUtility.UrlEncode(mstr.Menu.Trim()) + "&Module=" + HttpUtility.UrlEncode(mstr.ModuleName.Trim()) + "&TableName=" + mstr.TableName.Trim() + "&OptionType=T&OptionCode=" + HttpUtility.UrlEncode(mstr.OptionCode.Trim()) + "&Controller2=" + mstr.Controller.Trim() + "&Branch=" + SideBar.Para10 + "&BranchF=true&LRBillAmtF=" + FlagOfBillAmount;
                        }
                    }
                    else if (SideBar.Name.ToUpper().Trim() == "GODOWN STOCK")
                    {
                        MenuID = 2082;//Main Module
                        var BranchList = ctxTFAT.TfatBranch.Where(x => x.Category != "Area").Select(x => x.Code).ToList();
                        string AllBranch = "";
                        foreach (var item in BranchList)
                        {
                            AllBranch += item + ",";
                        }
                        AllBranch = AllBranch.Substring(0, AllBranch.Length - 1);
                        var mrights = ctxTFAT.UserRights.Where(z => z.Code == muserid && z.MenuID == MenuID).FirstOrDefault();
                        if (mrights != null || muserid.ToLower() == "super")
                        {
                            var FlagOfBillAmount = SideBar.Para1 == "T" ? true : false;
                            var mstr = ctxTFAT.TfatMenu.Select(x => new { x.ID, x.OptionCode, x.TableName, x.ModuleName, x.Menu, x.Controller, x.FormatCode, x.SubType, x.ParentMenu }).Where(z => z.ID == MenuID).FirstOrDefault();
                            murl = "/" + mstr.ParentMenu + "/" + mstr.Controller + "/Index?ViewDataId=" + HttpUtility.UrlEncode(mstr.FormatCode.Trim()) + "&Header=" + HttpUtility.UrlEncode(mstr.Menu.Trim()) + "&Module=" + HttpUtility.UrlEncode(mstr.ModuleName.Trim()) + "&TableName=" + mstr.TableName.Trim() + "&OptionType=T&OptionCode=" + HttpUtility.UrlEncode(mstr.OptionCode.Trim()) + "&Controller2=" + mstr.Controller.Trim() + "&ReportTypeL=LorryReceiptStock&StockType=LR&StockBranch=" + SideBar.Para10 + "&FromBranch=" + AllBranch;
                        }
                        else
                        {
                            MenuID = 2408;//User
                            mrights = ctxTFAT.UserRights.Where(z => z.Code == muserid && z.MenuID == MenuID).FirstOrDefault();
                            if (mrights != null || muserid.ToLower() == "super")
                            {
                                var FlagOfBillAmount = SideBar.Para1 == "T" ? true : false;
                                var mstr = ctxTFAT.TfatMenu.Select(x => new { x.ID, x.OptionCode, x.TableName, x.ModuleName, x.Menu, x.Controller, x.FormatCode, x.SubType, x.ParentMenu }).Where(z => z.ID == MenuID).FirstOrDefault();
                                murl = "/" + mstr.ParentMenu + "/" + mstr.Controller + "/Index?ViewDataId=" + HttpUtility.UrlEncode(mstr.FormatCode.Trim()) + "&Header=" + HttpUtility.UrlEncode(mstr.Menu.Trim()) + "&Module=" + HttpUtility.UrlEncode(mstr.ModuleName.Trim()) + "&TableName=" + mstr.TableName.Trim() + "&OptionType=T&OptionCode=" + HttpUtility.UrlEncode(mstr.OptionCode.Trim()) + "&Controller2=" + mstr.Controller.Trim() + "&ReportTypeL=LorryReceiptStock&StockType=LR&StockBranch=" + SideBar.Para10 + "&FromBranch=" + AllBranch;
                            }
                        }
                    }
                    else if (SideBar.Name.ToUpper().Trim() == "TRANSIT STOCK")
                    {
                        MenuID = 2082;//Main Module
                        var BranchList = ctxTFAT.TfatBranch.Where(x => x.Category != "Area").Select(x => x.Code).ToList();
                        string AllBranch = "";
                        foreach (var item in BranchList)
                        {
                            AllBranch += item + ",";
                        }
                        AllBranch = AllBranch.Substring(0, AllBranch.Length - 1);
                        var mrights = ctxTFAT.UserRights.Where(z => z.Code == muserid && z.MenuID == MenuID).FirstOrDefault();
                        if (mrights != null || muserid.ToLower() == "super")
                        {
                            var FlagOfBillAmount = SideBar.Para1 == "T" ? true : false;
                            var mstr = ctxTFAT.TfatMenu.Select(x => new { x.ID, x.OptionCode, x.TableName, x.ModuleName, x.Menu, x.Controller, x.FormatCode, x.SubType, x.ParentMenu }).Where(z => z.ID == MenuID).FirstOrDefault();
                            murl = "/" + mstr.ParentMenu + "/" + mstr.Controller + "/Index?ViewDataId=" + HttpUtility.UrlEncode(mstr.FormatCode.Trim()) + "&Header=" + HttpUtility.UrlEncode(mstr.Menu.Trim()) + "&Module=" + HttpUtility.UrlEncode(mstr.ModuleName.Trim()) + "&TableName=" + mstr.TableName.Trim() + "&OptionType=T&OptionCode=" + HttpUtility.UrlEncode(mstr.OptionCode.Trim()) + "&Controller2=" + mstr.Controller.Trim() + "&ReportTypeL=LorryReceiptStock&StockType=TRN&StockBranch=" + SideBar.Para10 + "&FromBranch=" + AllBranch;
                        }
                        else
                        {
                            MenuID = 2408;//User
                            mrights = ctxTFAT.UserRights.Where(z => z.Code == muserid && z.MenuID == MenuID).FirstOrDefault();
                            if (mrights != null || muserid.ToLower() == "super")
                            {
                                var FlagOfBillAmount = SideBar.Para1 == "T" ? true : false;
                                var mstr = ctxTFAT.TfatMenu.Select(x => new { x.ID, x.OptionCode, x.TableName, x.ModuleName, x.Menu, x.Controller, x.FormatCode, x.SubType, x.ParentMenu }).Where(z => z.ID == MenuID).FirstOrDefault();
                                murl = "/" + mstr.ParentMenu + "/" + mstr.Controller + "/Index?ViewDataId=" + HttpUtility.UrlEncode(mstr.FormatCode.Trim()) + "&Header=" + HttpUtility.UrlEncode(mstr.Menu.Trim()) + "&Module=" + HttpUtility.UrlEncode(mstr.ModuleName.Trim()) + "&TableName=" + mstr.TableName.Trim() + "&OptionType=T&OptionCode=" + HttpUtility.UrlEncode(mstr.OptionCode.Trim()) + "&Controller2=" + mstr.Controller.Trim() + "&ReportTypeL=LorryReceiptStock&StockType=TRN&StockBranch=" + SideBar.Para10 + "&FromBranch=" + AllBranch;
                            }
                        }
                    }
                    else if (SideBar.Name.ToUpper().Trim() == "UNBILLED CONSIGNMENT")
                    {
                        MenuID = 2082;//Main Module
                        var mrights = ctxTFAT.UserRights.Where(z => z.Code == muserid && z.MenuID == MenuID).FirstOrDefault();
                        if (mrights != null || muserid.ToLower() == "super")
                        {
                            var mstr = ctxTFAT.TfatMenu.Select(x => new { x.ID, x.OptionCode, x.TableName, x.ModuleName, x.Menu, x.Controller, x.FormatCode, x.SubType, x.ParentMenu }).Where(z => z.ID == MenuID).FirstOrDefault();
                            murl = "/" + mstr.ParentMenu + "/" + mstr.Controller + "/Index?ViewDataId=" + HttpUtility.UrlEncode(mstr.FormatCode.Trim()) + "&Header=" + HttpUtility.UrlEncode(mstr.Menu.Trim()) + "&Module=" + HttpUtility.UrlEncode(mstr.ModuleName.Trim()) + "&TableName=" + mstr.TableName.Trim() + "&OptionType=T&OptionCode=" + HttpUtility.UrlEncode(mstr.OptionCode.Trim()) + "&Controller2=" + mstr.Controller.Trim() + "&ReportTypeL=UNBillLorryReceipt&FromBranch=" + SideBar.Para10;
                        }
                        else
                        {
                            MenuID = 2449;//User
                            mrights = ctxTFAT.UserRights.Where(z => z.Code == muserid && z.MenuID == MenuID).FirstOrDefault();
                            if (mrights != null || muserid.ToLower() == "super")
                            {
                                var mstr = ctxTFAT.TfatMenu.Select(x => new { x.ID, x.OptionCode, x.TableName, x.ModuleName, x.Menu, x.Controller, x.FormatCode, x.SubType, x.ParentMenu }).Where(z => z.ID == MenuID).FirstOrDefault();
                                murl = "/" + mstr.ParentMenu + "/" + mstr.Controller + "/Index?ViewDataId=" + HttpUtility.UrlEncode(mstr.FormatCode.Trim()) + "&Header=" + HttpUtility.UrlEncode(mstr.Menu.Trim()) + "&Module=" + HttpUtility.UrlEncode(mstr.ModuleName.Trim()) + "&TableName=" + mstr.TableName.Trim() + "&OptionType=T&OptionCode=" + HttpUtility.UrlEncode(mstr.OptionCode.Trim()) + "&Controller2=" + mstr.Controller.Trim() + "&ReportTypeL=UNBillLorryReceipt&FromBranch=" + SideBar.Para10;
                            }
                        }

                    }
                    else if (SideBar.Name.ToUpper().Trim() == "OUTSTANDING")
                    {
                        MenuID = 2137;
                        var mrights = ctxTFAT.UserRights.Where(z => z.Code == muserid && z.MenuID == MenuID).FirstOrDefault();
                        if (mrights != null || muserid.ToLower() == "super")
                        {
                            var FlagOfBillAmount = SideBar.Para1 == "T" ? true : false;
                            var mstr = ctxTFAT.TfatMenu.Select(x => new { x.ID, x.OptionCode, x.TableName, x.ModuleName, x.Menu, x.Controller, x.FormatCode, x.SubType, x.ParentMenu }).Where(z => z.ID == MenuID).FirstOrDefault();
                            murl = "/" + mstr.ParentMenu + "/" + mstr.Controller + "/Index?ViewDataId=" + HttpUtility.UrlEncode(mstr.FormatCode.Trim()) + "&Header=" + HttpUtility.UrlEncode(mstr.Menu.Trim()) + "&Module=" + HttpUtility.UrlEncode(mstr.ModuleName.Trim()) + "&TableName=" + mstr.TableName.Trim() + "&OptionType=T&OptionCode=" + HttpUtility.UrlEncode(mstr.OptionCode.Trim()) + "&Controller2=" + mstr.Controller.Trim() + "&ReportTypeL=New Outstanding Report&MainType=SL&Branch=" + SideBar.Para10;
                        }

                    }
                    else if (SideBar.Name.ToUpper().Trim() == "EWAY BILL DETAILS")
                    {
                        MenuID = 2400;
                        var mrights = ctxTFAT.UserRights.Where(z => z.Code == muserid && z.MenuID == MenuID).FirstOrDefault();
                        //if (mrights != null || muserid.ToLower() == "super")
                        //{
                        var mstr = ctxTFAT.TfatMenu.Select(x => new { x.ID, x.OptionCode, x.TableName, x.ModuleName, x.Menu, x.Controller, x.FormatCode, x.SubType, x.ParentMenu }).Where(z => z.ID == MenuID).FirstOrDefault();
                        murl = "/" + mstr.ParentMenu + "/" + mstr.Controller + "/Index?ViewDataId=" + HttpUtility.UrlEncode(mstr.FormatCode.Trim()) + "&Header=" + HttpUtility.UrlEncode(mstr.Menu.Trim()) + "&Module=" + HttpUtility.UrlEncode(mstr.ModuleName.Trim()) + "&TableName=" + mstr.TableName.Trim() + "&OptionType=T&OptionCode=" + HttpUtility.UrlEncode(mstr.OptionCode.Trim()) + "&Controller2=" + mstr.Controller.Trim() + "&ZoomSideBar=true";
                        //}

                    }
                    else if (SideBar.Name.ToUpper().Trim() == "PAYABLE")
                    {
                        MenuID = 2130;
                        var mrights = ctxTFAT.UserRights.Where(z => z.Code == muserid && z.MenuID == MenuID).FirstOrDefault();
                        if (mrights != null || muserid.ToLower() == "super")
                        {
                            var FlagOfBillAmount = SideBar.Para1 == "T" ? true : false;
                            var mstr = ctxTFAT.TfatMenu.Select(x => new { x.ID, x.OptionCode, x.TableName, x.ModuleName, x.Menu, x.Controller, x.FormatCode, x.SubType, x.ParentMenu }).Where(z => z.ID == MenuID).FirstOrDefault();
                            murl = "/" + mstr.ParentMenu + "/" + mstr.Controller + "/Index?ViewDataId=" + HttpUtility.UrlEncode(mstr.FormatCode.Trim()) + "&Header=" + HttpUtility.UrlEncode(mstr.Menu.Trim()) + "&Module=" + HttpUtility.UrlEncode(mstr.ModuleName.Trim()) + "&TableName=" + mstr.TableName.Trim() + "&OptionType=T&OptionCode=" + HttpUtility.UrlEncode(mstr.OptionCode.Trim()) + "&Controller2=" + mstr.Controller.Trim() + "&ReportTypeL=New Outstanding Report&MainType=PR&Branch=" + SideBar.Para10;
                        }
                    }
                    else if (SideBar.Name.ToUpper().Trim() == "DRIVER STATUS")
                    {
                        MenuID = 2042;
                        var mrights = ctxTFAT.UserRights.Where(z => z.Code == muserid && z.MenuID == MenuID).FirstOrDefault();
                        if (mrights != null || muserid.ToLower() == "super")
                        {
                            var mstr = ctxTFAT.TfatMenu.Select(x => new { x.ID, x.OptionCode, x.TableName, x.ModuleName, x.Menu, x.Controller, x.FormatCode, x.SubType, x.ParentMenu }).Where(z => z.ID == MenuID).FirstOrDefault();
                            murl = "/" + mstr.ParentMenu + "/MasterGrid/Index?ViewDataId=" + HttpUtility.UrlEncode(mstr.FormatCode.Trim()) + "&Header=" + HttpUtility.UrlEncode(mstr.Menu.Trim()) + "&Module=" + HttpUtility.UrlEncode(mstr.ModuleName.Trim()) + "&TableName=" + mstr.TableName.Trim() + "&OptionType=T&OptionCode=" + HttpUtility.UrlEncode(mstr.OptionCode.Trim()) + "&Controller2=" + mstr.Controller.Trim() + "&LedgerThrough=true";
                        }
                    }
                    else if (SideBar.Name.ToUpper().Trim() == "VEHICLE STATUS")
                    {
                        MenuID = 2050;
                        var mrights = ctxTFAT.UserRights.Where(z => z.Code == muserid && z.MenuID == MenuID).FirstOrDefault();
                        if (mrights != null || muserid.ToLower() == "super")
                        {
                            var mstr = ctxTFAT.TfatMenu.Select(x => new { x.ID, x.OptionCode, x.TableName, x.ModuleName, x.Menu, x.Controller, x.FormatCode, x.SubType, x.ParentMenu }).Where(z => z.ID == MenuID).FirstOrDefault();
                            murl = "/" + mstr.ParentMenu + "/MasterGrid/Index?ViewDataId=" + HttpUtility.UrlEncode(mstr.FormatCode.Trim()) + "&Header=" + HttpUtility.UrlEncode(mstr.Menu.Trim()) + "&Module=" + HttpUtility.UrlEncode(mstr.ModuleName.Trim()) + "&TableName=" + mstr.TableName.Trim() + "&OptionType=T&OptionCode=" + HttpUtility.UrlEncode(mstr.OptionCode.Trim()) + "&Controller2=" + mstr.Controller.Trim() + "&LedgerThrough=true";
                        }
                    }
                    else if (SideBar.Name.ToUpper().Trim() == "TOP 10 CUSTOMERS")
                    {
                        MenuID = 0;
                    }
                    else if (SideBar.Name.ToUpper().Trim() == "TOP 10 EXPENSES")
                    {
                        MenuID = 0;
                    }
                    else if (SideBar.Name.ToUpper().Trim() == "TOP 10 GROUP CUSTOMERS")
                    {
                        MenuID = 0;
                    }
                    else if (SideBar.Name.ToUpper().Trim() == "TOP 10 VENDORS")
                    {
                        MenuID = 0;
                    }
                    else if (SideBar.Name.ToUpper().Trim() == "VEHICLE LOCATIONS")
                    {
                        MenuID = 0;
                    }
                    else if (SideBar.Name.ToUpper().Trim() == "Driver Trip & Balance".ToUpper().Trim())
                    {
                        MenuID = 2391;
                        var mrights = ctxTFAT.UserRights.Where(z => z.Code == muserid && z.MenuID == MenuID).FirstOrDefault();
                        if (mrights != null || muserid.ToLower() == "super")
                        {
                            var FlagOfBillAmount = SideBar.Para1 == "T" ? true : false;
                            var mstr = ctxTFAT.TfatMenu.Select(x => new { x.ID, x.OptionCode, x.TableName, x.ModuleName, x.Menu, x.Controller, x.FormatCode, x.SubType, x.ParentMenu }).Where(z => z.ID == MenuID).FirstOrDefault();
                            murl = "/" + mstr.ParentMenu + "/" + mstr.Controller + "/Index?ViewDataId=" + HttpUtility.UrlEncode(mstr.FormatCode.Trim()) + "&Header=" + HttpUtility.UrlEncode(mstr.Menu.Trim()) + "&Module=" + HttpUtility.UrlEncode(mstr.ModuleName.Trim()) + "&TableName=" + mstr.TableName.Trim() + "&OptionType=T&OptionCode=" + HttpUtility.UrlEncode(mstr.OptionCode.Trim()) + "&Controller2=" + mstr.Controller.Trim() + "&Driver=" + SideBar.Para1;
                        }
                    }
                    else if (SideBar.Name.ToUpper().Trim() == "Vehicle Trip Details".ToUpper().Trim())
                    {
                        MenuID = 2410;
                        var mrights = ctxTFAT.UserRights.Where(z => z.Code == muserid && z.MenuID == MenuID).FirstOrDefault();
                        if (mrights != null || muserid.ToLower() == "super")
                        {
                            var FlagOfBillAmount = SideBar.Para1 == "T" ? true : false;
                            var mstr = ctxTFAT.TfatMenu.Select(x => new { x.ID, x.OptionCode, x.TableName, x.ModuleName, x.Menu, x.Controller, x.FormatCode, x.SubType, x.ParentMenu }).Where(z => z.ID == MenuID).FirstOrDefault();
                            murl = "/" + mstr.ParentMenu + "/" + mstr.Controller + "/Index?ViewDataId=" + HttpUtility.UrlEncode(mstr.FormatCode.Trim()) + "&Header=" + HttpUtility.UrlEncode(mstr.Menu.Trim()) + "&Module=" + HttpUtility.UrlEncode(mstr.ModuleName.Trim()) + "&TableName=" + mstr.TableName.Trim() + "&OptionType=T&OptionCode=" + HttpUtility.UrlEncode(mstr.OptionCode.Trim()) + "&Controller2=" + mstr.Controller.Trim() + "&Vehicle=" + SideBar.Para1;
                        }
                    }
                    else if (SideBar.Name.ToUpper().Trim() == "Vehicle Exp Due".ToUpper().Trim())
                    {
                        MenuID = 2143;
                        var mrights = ctxTFAT.UserRights.Where(z => z.Code == muserid && z.MenuID == MenuID).FirstOrDefault();
                        if (mrights != null || muserid.ToLower() == "super")
                        {
                            var FlagOfBillAmount = SideBar.Para1 == "T" ? true : false;
                            var mstr = ctxTFAT.TfatMenu.Select(x => new { x.ID, x.OptionCode, x.TableName, x.ModuleName, x.Menu, x.Controller, x.FormatCode, x.SubType, x.ParentMenu }).Where(z => z.ID == MenuID).FirstOrDefault();
                            murl = "/" + mstr.ParentMenu + "/" + mstr.Controller + "/Index?ViewDataId=" + HttpUtility.UrlEncode(mstr.FormatCode.Trim()) + "&Header=" + HttpUtility.UrlEncode(mstr.Menu.Trim()) + "&Module=" + HttpUtility.UrlEncode(mstr.ModuleName.Trim()) + "&TableName=" + mstr.TableName.Trim() + "&OptionType=T&OptionCode=" + HttpUtility.UrlEncode(mstr.OptionCode.Trim()) + "&Controller2=" + mstr.Controller.Trim() + "&Days=30&Vehicle=" + SideBar.Para1 + "&Expenses=" + SideBar.Para2 + "&CostcenterReq=" + true;
                        }
                    }
                }
            }
            return Json(new { url = murl, Message = "In-sufficient Rights to Execute this Action." }, JsonRequestBehavior.AllowGet);




            //if (mdocument.StartsWith("%"))
            //{
            //    mdocument = mdocument.Substring(1);
            //}
            //try
            //{
            //    string mtype = mdocument.Substring(0, 5);
            //    string mBranch = ctxTFAT.Authorisation.Where(x => x.ParentKey == mdocument).Select(x => x.Branch).FirstOrDefault();

            //    if (mtype == "Trip0")
            //    {
            //        mdocument = mdocument.Substring(7, (mdocument.Length - 7));
            //    }
            //    else if (mtype == "FM000" || mtype == "LR000" || mtype == "LC000")
            //    {
            //        mdocument = mdocument.Substring(10, (mdocument.Length - 10));
            //    }
            //    else
            //    {
            //        mdocument = mBranch + mdocument;
            //    }

            //    int ID = GetId(mtype.Trim());




            //    if (mrights != null || muserid.ToLower() == "super")
            //    {
            //        string msubtype = GetSubType(mtype);
            //        var mstr = ctxTFAT.TfatMenu.Select(x => new { x.ID, x.OptionCode, x.TableName, x.ModuleName, x.Menu, x.Controller, x.FormatCode, x.SubType, x.ParentMenu }).Where(z => z.ID == ID && z.ModuleName == "Transactions" && (z.ParentMenu == "Accounts" || z.ParentMenu == "Logistics" || z.ParentMenu == "Vehicles")).FirstOrDefault();
            //        murl = "/" + mstr.ParentMenu + "/" + mstr.Controller + "/Index?Document=" + mdocument + "&ViewDataId=" + HttpUtility.UrlEncode(mstr.FormatCode.Trim()) + "&Header=" + HttpUtility.UrlEncode(mstr.Menu.Trim()) + "&Module=" + HttpUtility.UrlEncode(mstr.ModuleName.Trim()) + "&TableName=" + mstr.TableName.Trim() + "&OptionType=T&OptionCode=" + HttpUtility.UrlEncode(mstr.OptionCode.Trim()) + "&Controller2=" + mstr.Controller.Trim() + "&LedgerThrough=true";
            //    }
            //    return Json(new { url = murl, Message = "In-sufficient Rights to Execute this Action." }, JsonRequestBehavior.AllowGet);
            //}
            //catch (Exception ex)
            //{
            //    return Json(new { url = "", Message = "Error." }, JsonRequestBehavior.AllowGet);
            //}
        }









        public ActionResult GetBarDetail(string code)
        {
            GenerateChart(code);
            return null;
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords(GridOption Model)
        {
            mpara = "";
            ActiveSideBarObjects activeSideBar = ctxTFAT.ActiveSideBarObjects.Where(x => x.Code == Model.ViewDataId && x.Users.Contains(muserid)).FirstOrDefault();
            if (activeSideBar != null)
            {
                mpara = "para09^" + activeSideBar.Para2 + "~para10^" + activeSideBar.Para3 + "~para11^" + activeSideBar.Para4 + "~para12^" + activeSideBar.Para5 + "~para13^" + activeSideBar.Para6 + "~para14^" + activeSideBar.Para7 + "~";
            }

            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        [HttpPost]
        public ActionResult GetGridData(GridOption Model)
        {
            return GetGridReport(Model, "R", mpara, false, 0);
        }

        [HttpPost]
        public ActionResult GetGridStructureRecords1(GridOption Model)
        {

            #region Set Parameters

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
            mpara = "";
            var Branch = "";

            foreach (var item in ctxTFAT.TfatBranch.Where(x => x.Category != "Area").ToList())
            {
                Branch += "'" + item.Code + "',";
            }
            Model.Branch = Branch.Substring(0, Branch.Length - 1);
            Model.SelectContent = Model.Branch += "|" + "30" + "|" + "60" + "|" + "90" + "|" + "120" + "|" + "150" + "|" + "180";
            if (!String.IsNullOrEmpty(Model.SelectContent))
            {
                var GetPara = Model.SelectContent.Split('|');
                for (int i = 0; i < GetPara.Count(); i++)
                {
                    if (!String.IsNullOrEmpty(GetPara[i]))
                    {
                        switch (i + 1)
                        {
                            case 1:
                                ppara04 = GetPara[i];

                                break;
                            case 2:
                                ppara09 = GetPara[i];
                                mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                break;
                            case 3:
                                ppara10 = GetPara[i];
                                mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                break;
                            case 4:
                                ppara11 = GetPara[i];
                                mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                break;
                            case 5:
                                ppara12 = GetPara[i];
                                mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                break;
                            case 6:
                                ppara13 = GetPara[i];
                                mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                break;
                            case 7:
                                ppara14 = GetPara[i];
                                mpara = mpara + "para" + (i + 8).ToString().PadLeft(2, '0') + "^" + GetPara[i] + "~";
                                break;
                        }
                    }
                }
            }

            #endregion

            var date = Model.Date.Replace("-", "/").Split(':');
            Model.FromDate = (Convert.ToDateTime(date[0])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            Model.ToDate = (Convert.ToDateTime(date[1])).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand();// ("SPTFAT_ReceivableAnalysis", tfat_conx);
            cmd.Connection = tfat_conx;

            if (Model.ViewDataId == "Outstanding Report With LR")
            {
                cmd.CommandText = "SPTFAT_ReceivableAnalysisWithLR";
            }
            else if (Model.ViewDataId == "Outstanding Report" || Model.ViewDataId == "Outstanding Report Register" || Model.ViewDataId == "UnAdjust Report" || Model.ViewDataId == "Invoice wise Outstanding" || Model.ViewDataId == "OS Ageing" || Model.ViewDataId == "Party Ageing Summary")
            {
                cmd.CommandText = "SPTFAT_ReceivableAnalysis";
            }
            else
            {
                cmd.CommandText = "SPTFAT_ReceivableWithRefDoc";
            }

            var GsetBaseGr = Model.Customer == true ? "D" : ctxTFAT.Master.Where(x => x.Code == Model.Code).Select(x => x.BaseGr).FirstOrDefault();
            ppara07 = "Yes";
            mbasegr = GsetBaseGr;
            var hfgdfhd = ppara04 == null || ppara04 == "" ? mbranchcode : ppara04;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@mBaseGr", SqlDbType.VarChar).Value = mbasegr == null ? Model.MainType == "SL" ? "D" : "S" : mbasegr;
            cmd.Parameters.Add("@mDate", SqlDbType.Date).Value = Model.ToDate;
            cmd.Parameters.Add("@mBranch", SqlDbType.VarChar).Value = ppara04 == null || ppara04 == "" ? mbranchcode : ppara04;
            cmd.Parameters.Add("@mLocations", SqlDbType.VarChar).Value = Fieldoftable("Warehouse", "Code", "Name='" + ppara01 + "'");
            cmd.Parameters.Add("@mConsiderCRPR", SqlDbType.Bit).Value = ppara06 == "Yes" ? true : false;
            cmd.Parameters.Add("@mSuppressZero", SqlDbType.Bit).Value = ppara08 == "Yes" ? true : false;
            cmd.Parameters.Add("@mArea", SqlDbType.VarChar).Value = "";// ppara04;
            cmd.Parameters.Add("@mParties", SqlDbType.VarChar).Value = Model.Code == null ? "" : Model.Code;
            cmd.Parameters.Add("@mGrps", SqlDbType.VarChar).Value = Fieldoftable("MasterGroups", "Code", "Name='" + ppara15 + "'");
            cmd.Parameters.Add("@mSalesman", SqlDbType.VarChar).Value = ppara02;//FieldoftableNumber("Salesman", "Code", "Name='" + 
            cmd.Parameters.Add("@mBroker", SqlDbType.VarChar).Value = ppara03;//FieldoftableNumber("Broker", "Code", "Name='" + ppara03 + "'");
            cmd.Parameters.Add("@mCategory", SqlDbType.VarChar).Value = ppara05;
            cmd.Parameters.Add("@mRefTillDate", SqlDbType.Bit).Value = ppara07 == "Yes" ? true : false;
            cmd.Parameters.Add("@mDocString", SqlDbType.VarChar).Value = mDocString;
            cmd.Parameters.Add("@mCustomer", SqlDbType.VarChar).Value = Model.Customer == true ? "1" : "0";
            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            tfat_conx.Close();
            tfat_conx.Dispose();
            return GetGridDataColumns(Model.ViewDataId, "X", "");
        }

        [HttpPost]
        public ActionResult GetGridData1(GridOption Model)
        {
            return GetGridReport(Model, "R", mpara, true, 0);
        }

        public ActionResult GetMenuURLF(string moptioncode, string moptionname)
        {
            string murl = "";
            bool mrights = true;
            int mid = (int)FieldoftableNumber("TfatMenu", "ID", "OptionCode='" + moptioncode + "'");
            if (muserid.ToLower() != "super")
            {
                mrights = Convert.ToBoolean(Fieldoftable("UserRights", "xAdd", "MenuID=" + mid, "L"));
            }
            if (mrights == true)
            {
                string mmodule = "";
                if (moptionname.Contains("[") && moptionname.Contains("]"))
                {
                    mmodule = moptionname.Substring(moptionname.IndexOf('[') + 1, moptionname.IndexOf(']') - moptionname.IndexOf('[') - 1);
                }
                //Session["ModuleName"].ToString();
                if (mmodule == "")
                    mmodule = Session["ModuleName"].ToString();

                Session["ModuleName"] = mmodule;

                TfatMenu mmenu = ctxTFAT.TfatMenu.Where(z => z.OptionCode == moptioncode && z.ModuleName == mmodule).Select(x => x).FirstOrDefault();
                if (mmenu != null)
                {
                    //murl = "/" + mmenu.ParentMenu + "/" + GetControllerName(mmenu.OptionType, mmenu.Controller) + "/Index?Document=&Mode=Add&ChangeLog=Add&ViewDataId=" + HttpUtility.UrlEncode(mmenu.FormatCode.Trim()) + "&Header=" + HttpUtility.UrlEncode(mmenu.Menu.Trim()) + "&Module=" + HttpUtility.UrlEncode(mmenu.ModuleName.Trim()) + "&TableName=" + mmenu.TableName.Trim() + "&OptionType=T&OptionCode=" + HttpUtility.UrlEncode(mmenu.OptionCode.Trim()) + "&Controller2=" + mmenu.Controller.Trim() + "&AutoClose=false";
                    murl = "/" + mmenu.ParentMenu + "/" + GetControllerName(mmenu.OptionType, mmenu.Controller) + "/Index?ViewDataId=" + HttpUtility.UrlEncode(mmenu.FormatCode.Trim()) + "&Header=" + HttpUtility.UrlEncode(mmenu.Menu.Trim()) + "&Module=" + HttpUtility.UrlEncode(mmenu.ModuleName.Trim()) + "&TableName=" + mmenu.TableName.Trim() + "&MainType=" + mmenu.MainType + "&SubType=" + mmenu.SubType + "&OptionType=T&OptionCode=" + HttpUtility.UrlEncode(mmenu.OptionCode.Trim()) + "&Controller2=" + mmenu.Controller.Trim() + "&AutoClose=false";
                }
            }
            return Json(new { url = murl, Message = "Ohhh! You're not Authorised to Execute this Action." }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SearchMenuF(string term)
        {
            //string mmodule = Session["ModuleName"].ToString();
            if (muserid.ToLower() == "super")
            {
                return Json((from m in ctxTFAT.TfatMenu
                             where m.Hide == false && m.AllowClick == true && m.Menu.ToLower().Contains(term.ToLower()) //m.ModuleName == mmodule && 
                             select new { Name = m.Menu + " [" + m.ModuleName + "]", Code = m.OptionCode }).OrderBy(x => x.Name).Distinct().ToArray(), JsonRequestBehavior.AllowGet); ;
            }
            else
            {
                return Json((from m in ctxTFAT.TfatMenu
                             where m.Hide == false && m.AllowClick == true && m.Menu.ToLower().Contains(term.ToLower())//m.ModuleName == mmodule && 
                             join xUserRights in ctxTFAT.UserRights.Where(z => z.Code == muserid && z.xCess == true) on m.ID equals xUserRights.MenuID
                             select new { Name = m.Menu + " [" + m.ModuleName + "]", Code = m.OptionCode }).OrderBy(x => x.Name).Distinct().ToArray(), JsonRequestBehavior.AllowGet);
            }
        }
    }
}