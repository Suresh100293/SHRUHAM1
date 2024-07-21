using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using System.Configuration;
//using ALT_ERP3.DynamicBusinessLayer;
//using ALT_ERP3.DynamicBusinessLayer.Repository;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class Loading_DispatchController : BaseController
    {
        string attachmentPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentPath"].ToString()) == true ? "" : ConfigurationManager.AppSettings["AttachmentPath"].ToString();
        private static string mdocument = "";
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        private static string mauthorise = "A00";

        public ActionResult LoadLC(LoadingDispachVM mModel)
        {
            string LoadLCnoList = "";
            bool RoutFlag = false;
            string Message = "", LCBranch = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    List<LCModal> LoadLCList = TempData.Peek("ExistLC") as List<LCModal>;
                    if (LoadLCList == null)
                    {
                        LoadLCList = new List<LCModal>();
                    }
                    List<LCModal> PendingList = TempData.Peek("PendingLC") as List<LCModal>;
                    if (PendingList == null)
                    {
                        PendingList = new List<LCModal>();
                    }
                    FMROUTETable FMROUTETable = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
                    if (FMROUTETable != null)
                    {
                        FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == FMROUTETable.Parentkey.ToString()).FirstOrDefault();

                        #region Loading Effects Route
                        if (!String.IsNullOrEmpty(mModel.LcNo))
                        {
                            List<string> LoadindLcList = new List<string>();
                            LoadindLcList = mModel.LcNo.Split(',').ToList();

                            #region Add New LC
                            var LastTrnasitEntryOfFM = ctxTFAT.LRStock.Where(x => x.Fmno == fMMaster.FmNo && x.Type == "TRN").OrderByDescending(x => x.RECORDKEY).Select(x => x.TableKey).FirstOrDefault();
                            var CheckBranchCode = LastTrnasitEntryOfFM == null ? "" : LastTrnasitEntryOfFM.Substring(0, 6);
                            int I = 1;
                            if (ctxTFAT.TfatBranch.Where(x => x.Code == CheckBranchCode).FirstOrDefault() == null)
                            {
                                I = LastTrnasitEntryOfFM == null ? 0 : Convert.ToInt32(LastTrnasitEntryOfFM.Substring(7, 3));
                            }
                            else
                            {
                                I = LastTrnasitEntryOfFM == null ? 0 : Convert.ToInt32(LastTrnasitEntryOfFM.Substring(13, 3));
                            }

                            I = I == 0 ? 1 : (I + 1);
                            List<LCMaster> lCMaster = ctxTFAT.LCMaster.Where(x => LoadindLcList.Contains(x.TableKey.ToString())).ToList();
                            foreach (var lC in lCMaster)
                            {
                                if (lC.DispachFM == 0)
                                {
                                    var LCF = ctxTFAT.LCMaster.Where(x => x.TableKey == lC.TableKey && x.DispachFM != fMMaster.FmNo).FirstOrDefault();
                                    if (LCF.DispachFM == 0)
                                    {
                                        LoadLCnoList += LCF.LCno + ",";
                                        if (RoutFlag == false)
                                        {
                                            RoutFlag = CheckNewRouteReqierdOrNot(fMMaster.TableKey, LCF.ToBranch);
                                        }
                                        LCBranch += LCF.ToBranch + ",";
                                        //AddNewRouteExistingRouteOfFreightMemo(fMMaster.FmNo, LCF.ToBranch);
                                        lC.DispachFM = fMMaster.FmNo;
                                        lC.FMRefTablekey = fMMaster.TableKey;
                                        lC.LoadDate = DateTime.Now;
                                        lC.LoadTime = DateTime.Now.ToString("HH:mm");
                                        LoadLCList.Add(PendingList.Where(x => x.Tablekey == lC.TableKey.ToString()).FirstOrDefault());
                                        ctxTFAT.Entry(lC).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        return Json(new
                                        {
                                            Status = "failure",
                                            Message = "LC No: " + LCF.LCno + " This LC is Already Loaded So We Cant Load IT Again.\n"
                                        }, JsonRequestBehavior.AllowGet);
                                    }
                                    List<LCDetail> lCDetails = ctxTFAT.LCDetail.Where(x => x.LCRefTablekey == lC.TableKey).ToList();
                                    foreach (var item in lCDetails)
                                    {
                                        #region Decrese Stock
                                        LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey.ToString() == item.ParentKey).FirstOrDefault();
                                        lRStock.BalQty -= item.Qty;
                                        lRStock.BalWeight -= item.LRActWeight;
                                        ctxTFAT.Entry(lRStock).State = EntityState.Modified;
                                        if (lRStock.BalQty < 0)
                                        {
                                            return Json(new
                                            {
                                                Status = "failure",
                                                Message = "Not Allowed To Load LC No: " + LCF.LCno + " Due To Negative Stock...!"
                                            }, JsonRequestBehavior.AllowGet);
                                        }

                                        #endregion

                                        #region Transit Entry IN LR Stock
                                        LRStock LoadlRStock = new LRStock();
                                        LoadlRStock.LoginBranch = mbranchcode;
                                        LoadlRStock.Branch = lC.ToBranch;
                                        LoadlRStock.LrNo = lRStock.LrNo;
                                        LoadlRStock.LoadForGodown = item.UnloadGodwonQty;
                                        LoadlRStock.LoadForDirect = item.UnloadDirectQty;
                                        LoadlRStock.Date = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                                        LoadlRStock.Time = (DateTime.Now.ToString("HH:mm"));
                                        LoadlRStock.TotalQty = item.Qty;
                                        LoadlRStock.AllocatBalQty = item.Qty;
                                        LoadlRStock.BalQty = item.Qty;
                                        LoadlRStock.ActWeight = item.LRActWeight;
                                        LoadlRStock.AllocatBalWght = item.LRActWeight;
                                        LoadlRStock.BalWeight = item.LRActWeight;
                                        LoadlRStock.ChrgWeight = lRStock.ChrgWeight;
                                        LoadlRStock.ChrgType = lRStock.ChrgType;
                                        LoadlRStock.Description = lRStock.Description;
                                        LoadlRStock.Unit = lRStock.Unit;
                                        LoadlRStock.FromBranch = lRStock.FromBranch;
                                        LoadlRStock.ToBranch = lRStock.ToBranch;
                                        LoadlRStock.Consigner = lRStock.Consigner;
                                        LoadlRStock.Consignee = lRStock.Consignee;
                                        LoadlRStock.LrType = lRStock.LrType;
                                        LoadlRStock.Coln = lRStock.Coln;
                                        LoadlRStock.Delivery = lRStock.Delivery;
                                        LoadlRStock.Remark = lRStock.Remark;
                                        LoadlRStock.StockAt = fMMaster.TruckNo;
                                        LoadlRStock.StockStatus = "T";
                                        LoadlRStock.LCNO = item.LCno;
                                        LoadlRStock.AUTHIDS = muserid;
                                        LoadlRStock.AUTHORISE = mauthorise;
                                        LoadlRStock.ENTEREDBY = muserid;
                                        LoadlRStock.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                                        LoadlRStock.UnloadDirectQty = 0;
                                        LoadlRStock.UnloadGodwonQty = 0;
                                        LoadlRStock.Fmno = fMMaster.FmNo;
                                        LoadlRStock.TableKey = mbranchcode + "TRN00" + mperiod.Substring(0, 2) + I.ToString("D3") + fMMaster.FmNo;
                                        LoadlRStock.ParentKey = item.ParentKey;
                                        LoadlRStock.Type = "TRN";
                                        LoadlRStock.LRMode = lRStock.LRMode;
                                        LoadlRStock.Prefix = mperiod;
                                        LoadlRStock.FMRefTablekey = fMMaster.TableKey;
                                        LoadlRStock.LCRefTablekey = item.LCRefTablekey;
                                        LoadlRStock.LRRefTablekey = item.LRRefTablekey;
                                        ctxTFAT.LRStock.Add(LoadlRStock);
                                        #endregion

                                        ++I;
                                    }
                                    Message += lC.LCno + " Loaded Sucessfully\n";
                                }
                                else
                                {
                                    Message += lC.LCno + " This LC is Already Loaded So We Cant Load IT Again.\n";
                                }
                            }
                            #endregion

                            #region Update Route
                            string LoadLC = "", LoadLCKey = "";
                            foreach (var item in LoadLCList.ToList())
                            {
                                LoadLC += item.lcno + ",";
                                LoadLCKey += item.Tablekey + ",";
                            }
                            if (!string.IsNullOrEmpty(LoadLC))
                            {
                                FMROUTETable.LCNO = LoadLC.Substring(0, LoadLC.Length - 1);
                                FMROUTETable.LODRefTablekey = LoadLCKey.Substring(0, LoadLCKey.Length - 1);
                            }
                            foreach (var SetLoadLCDATE in LoadLCList)
                            {
                                SetLoadLCDATE.LoadDate = DateTime.Now.ToShortDateString();
                                SetLoadLCDATE.LoadTime = DateTime.Now.ToString("HH:mm");
                            }
                            TempData["ExistLC"] = LoadLCList;
                            #endregion
                        }
                        #endregion

                        #region Update Fmdetails
                        string CurrentFmLodedLCList = "";
                        List<FMROUTETable> FMROUTETables = ctxTFAT.FMROUTETable.Where(x => x.Parentkey == fMMaster.TableKey && x.RECORDKEY != FMROUTETable.RECORDKEY).ToList();
                        foreach (var item in FMROUTETables)
                        {
                            if (!String.IsNullOrEmpty(item.LCNO))
                            {
                                CurrentFmLodedLCList += item.LCNO + ",";
                            }
                        }
                        if (!String.IsNullOrEmpty(FMROUTETable.LCNO))
                        {
                            CurrentFmLodedLCList += FMROUTETable.LCNO + ",";
                        }
                        if (CurrentFmLodedLCList != "")
                        {
                            fMMaster.LCno = CurrentFmLodedLCList.Substring(0, CurrentFmLodedLCList.Length - 1);
                        }
                        else
                        {
                            fMMaster.LCno = null;
                        }
                        ctxTFAT.Entry(fMMaster).State = EntityState.Modified;
                        #endregion

                        if (!String.IsNullOrEmpty(LoadLCnoList))
                        {
                            LoadLCnoList = LoadLCnoList.Substring(0, LoadLCnoList.Length - 1);
                        }

                        ctxTFAT.SaveChanges();
                        transaction.Commit();
                        transaction.Dispose();
                        if ((fMMaster.PayLoad ?? 0) > 0)
                        {
                            var OverLoadKGs = GetOverLoadKGs(FMROUTETable);
                            if (OverLoadKGs[2] != 0)
                            {
                                VehicleOverloadNotification(FMROUTETable, OverLoadKGs[0], OverLoadKGs[1], OverLoadKGs[2]);
                            }
                        }
                        UpdateAuditTrail(mbranchcode, "Veh-ACT", "Loading Master", fMMaster.ParentKey, DateTime.Now, 0, "", "Load LC :" + LoadLCnoList, "NA");
                    }
                    else
                    {
                        return Json(new { Message = "Error,Route Not Fount..\n", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "StateMaster", Message = Message, LCBranch = LCBranch, RoutFlag = RoutFlag }, JsonRequestBehavior.AllowGet);
        }

        public List<int> GetOverLoadKGs(FMROUTETable fMROUTETable)
        {
            int OverLoadKGs = 0;

            TempData.Remove("UnloadLcList");

            FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == fMROUTETable.Parentkey).FirstOrDefault();
            List<LRModal> UnloadList = new List<LRModal>();

            LoadingToDispatchController loading = new LoadingToDispatchController();
            UnloadList = loading.GetUnLoadLClistOfAllMaterial(fMMaster, "");
            TempData["UnloadLcList"] = UnloadList;

            List<LRModal> AlreadyUnloadLrList = TempData.Peek("AlreadyUnloadLrList") as List<LRModal>;
            if (AlreadyUnloadLrList == null)
            {
                AlreadyUnloadLrList = new List<LRModal>();
            }
            if (AlreadyUnloadLrList.Count() > 0)
            {
                UnloadList = UnloadList.Where(p => !AlreadyUnloadLrList.Any(p2 => p2.recordkey == p.recordkey)).ToList();
            }
            AlreadyUnloadLrList = TempData.Peek("OTHAlreadyUnloadLrList") as List<LRModal>;
            if (AlreadyUnloadLrList == null)
            {
                AlreadyUnloadLrList = new List<LRModal>();
            }
            if (AlreadyUnloadLrList.Count() > 0)
            {
                UnloadList = UnloadList.Where(p => !AlreadyUnloadLrList.Any(p2 => p2.recordkey == p.recordkey)).ToList();
            }

            double Payload = fMMaster.PayLoad ?? 0;
            double LoadedWeight = UnloadList.Sum(x => x.Weight);
            double Available = Payload - LoadedWeight;
            Available = Available < 0 ? Convert.ToInt32(Available * (-1)) : 0;

            List<int> MaterialDetails = new List<int>();
            MaterialDetails.Add(Convert.ToInt32(Payload));
            MaterialDetails.Add(Convert.ToInt32(LoadedWeight));
            MaterialDetails.Add(Convert.ToInt32(Available));
            return MaterialDetails;
        }

        public ActionResult DeleteLC(LoadingDispachVM mModel)
        {

            //bool Status = false;
            string Message = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    FMROUTETable FMROUTETable = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
                    if (FMROUTETable != null)
                    {
                        FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == FMROUTETable.Parentkey.ToString()).FirstOrDefault();
                        List<LCModal> LoadLCList = TempData.Peek("ExistLC") as List<LCModal>;

                        var Lc = LoadLCList.Where(x => x.Tablekey == mModel.LcNo).FirstOrDefault();
                        if (Lc != null)
                        {
                            #region Delete Transit Entry From LrStock

                            var LoadingLC = ctxTFAT.LRStock.Where(x => x.LCRefTablekey == mModel.LcNo && x.FMRefTablekey == fMMaster.TableKey && x.Type == "TRN").ToList();
                            foreach (var item in LoadingLC)
                            {
                                var UseStock = ctxTFAT.LRStock.Where(x => x.ParentKey == item.TableKey).FirstOrDefault();
                                if (UseStock != null)
                                {
                                    return Json(new { Message = "This Stock Used.\n Cant Remove " + item.LCNO + " Lorry Challan.", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            foreach (var IncreaseStock in LoadingLC)
                            {
                                LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey.ToString() == IncreaseStock.ParentKey).FirstOrDefault();
                                lRStock.BalQty += IncreaseStock.BalQty;
                                lRStock.BalWeight += IncreaseStock.BalWeight;
                                ctxTFAT.Entry(lRStock).State = EntityState.Modified;
                            }


                            ctxTFAT.LRStock.RemoveRange(LoadingLC);

                            #endregion



                            #region Update Lcmaster

                            LCMaster lC = ctxTFAT.LCMaster.Where(x => x.TableKey.ToString() == mModel.LcNo).FirstOrDefault();
                            lC.DispachFM = 0;
                            lC.FMRefTablekey = "";
                            lC.LoadDate = null;
                            lC.LoadTime = null;
                            ctxTFAT.Entry(lC).State = EntityState.Modified;

                            #endregion

                            LoadLCList.Remove(Lc);
                            TempData["ExistLC"] = LoadLCList;

                            #region Update FmRoute

                            string LoadLC = "", LoadLCKey = "";
                            foreach (var item in LoadLCList)
                            {
                                LoadLC += item.lcno + ",";
                                LoadLCKey += item.Tablekey + ",";
                            }
                            if (LoadLC == "")
                            {
                                FMROUTETable.LCNO = null;
                                FMROUTETable.LODRefTablekey = null;
                            }
                            else
                            {
                                FMROUTETable.LCNO = LoadLC.Substring(0, LoadLC.Length - 1);
                                FMROUTETable.LODRefTablekey = LoadLCKey.Substring(0, LoadLCKey.Length - 1);
                            }


                            #endregion

                            #region Update Fmdetails
                            string CurrentFmLodedLCList = "";
                            List<FMROUTETable> FMROUTETables = ctxTFAT.FMROUTETable.Where(x => x.Parentkey == fMMaster.TableKey && x.RECORDKEY != FMROUTETable.RECORDKEY).ToList();
                            foreach (var item in FMROUTETables)
                            {
                                if (!String.IsNullOrEmpty(item.LCNO))
                                {
                                    CurrentFmLodedLCList += item.LCNO + ",";
                                }
                            }
                            if (!String.IsNullOrEmpty(FMROUTETable.LCNO))
                            {
                                CurrentFmLodedLCList += FMROUTETable.LCNO + ",";
                            }
                            if (CurrentFmLodedLCList != "")
                            {
                                fMMaster.LCno = CurrentFmLodedLCList.Substring(0, CurrentFmLodedLCList.Length - 1);
                            }
                            else
                            {
                                fMMaster.LCno = null;
                            }
                            ctxTFAT.Entry(fMMaster).State = EntityState.Modified;
                            #endregion

                            Message += "" + Lc.lcno + " Delete Sucessfully\n";
                        }



                        ctxTFAT.SaveChanges();
                        transaction.Commit();
                        transaction.Dispose();
                        UpdateAuditTrail(mbranchcode, "Veh-ACT", "Loading Master", fMMaster.ParentKey, DateTime.Now, 0, "", Message, "NA");
                    }
                    else
                    {
                        return Json(new { Message = "Error,Route Not Fount..\n", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", id = "StateMaster", Message = Message }, JsonRequestBehavior.AllowGet);
        }

        private bool CheckNewRouteReqierdOrNot(string Tablekey, string toBranch)
        {
            bool RouteRequire = false;
            string Parent = mbranchcode;
            TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == toBranch).FirstOrDefault();
            if (tfatBranch.Category == "Area")
            {
                if (tfatBranch.Grp != "G00000")
                {
                    RouteRequire = true;
                    Parent = tfatBranch.Grp;
                }
            }
            else
            {
                RouteRequire = true;
                Parent = tfatBranch.Code;

            }
            if (RouteRequire)
            {
                var ExistingRout = ctxTFAT.FMROUTETable.Where(x => x.Parentkey == Tablekey && x.RouteType == "R").Select(x => x.Parent).ToList();
                if (!(ExistingRout.Contains(Parent)))
                {
                    RouteRequire = true;
                }
                else
                {
                    RouteRequire = false;
                }
            }

            return RouteRequire;
        }

        public ActionResult AddNewRouteExistingRouteOfFreightMemo(string fmNo, string toBranch)
        {
            bool AddRoute = false;
            FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.FmNo.ToString() == fmNo).FirstOrDefault();
            var Parent = mbranchcode;
            var BranchList = toBranch.Split(',').ToList();
            var LastRouteOfFM = ctxTFAT.FMROUTETable.Where(x => x.FmNo.ToString() == fmNo && x.RouteType == "R").OrderByDescending(x => x.SequenceRoute).FirstOrDefault();
            int LastRoute = LastRouteOfFM.SequenceRoute.Value;
            var LastRouteOfFMList = ctxTFAT.FMROUTETable.Where(x => x.FmNo.ToString() == fmNo && x.SubRoute == LastRoute).ToList();
            var ExistingRout = ctxTFAT.FMROUTETable.Where(x => x.FmNo.ToString() == fmNo && x.RouteType == "R").Select(x => x.Parent).ToList();
            var Via = "";
            var ViaName = "";
            foreach (var item in BranchList)
            {
                if (!String.IsNullOrEmpty(item))
                {
                    TfatBranch tfatBranch = ctxTFAT.TfatBranch.Where(x => x.Code == item).FirstOrDefault();
                    if (tfatBranch.Category == "Area")
                    {
                        if (tfatBranch.Grp != "G00000")
                        {
                            AddRoute = true;
                            Parent = tfatBranch.Grp;
                        }
                    }
                    else
                    {
                        AddRoute = true;
                        Parent = tfatBranch.Code;
                    }
                    if (AddRoute)
                    {

                        if (!(ExistingRout.Contains(Parent)))
                        {
                            Via += item + ",";
                            ViaName += tfatBranch.Name + ",";
                            FMROUTETable fMROUTE = new FMROUTETable();
                            fMROUTE.ENTEREDBY = muserid;
                            fMROUTE.FmNo = Convert.ToInt32(fmNo);
                            fMROUTE.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                            fMROUTE.Parent = Parent;
                            fMROUTE.RouteClear = false;
                            fMROUTE.RouteType = "R";
                            fMROUTE.RouteVia = item;
                            fMROUTE.SubRoute = LastRoute;
                            fMROUTE.SequenceRoute = LastRoute;
                            fMROUTE.VehicleActivity = "00:00";
                            fMROUTE.AUTHORISE = mauthorise;
                            fMROUTE.AUTHIDS = muserid;
                            fMROUTE.Prefix = mperiod;
                            fMROUTE.Parentkey = fMMaster.TableKey;
                            ctxTFAT.FMROUTETable.Add(fMROUTE);
                            ExistingRout.Add(Parent);
                            ++LastRoute;

                        }
                    }
                }

            }


            if (!(String.IsNullOrEmpty(Via)))
            {
                var MainRoute = LastRouteOfFMList.Where(x => x.RouteType == "R").FirstOrDefault();
                MainRoute.SequenceRoute = (LastRoute);
                MainRoute.SubRoute = (LastRoute);
                ctxTFAT.Entry(MainRoute).State = EntityState.Modified;
                var SubRouteList = LastRouteOfFMList.Where(x => x.RouteType != "R").ToList();
                SubRouteList.ForEach(x => x.SubRoute = (LastRoute));

                //Via = Via.Substring(0, Via.Length - 1);
                //ViaName = ViaName.Substring(0, ViaName.Length - 1);

                fMMaster.RouteVia += Via;
                //fMMaster.RouteViaName += ViaName;

                FMROUTETable LastRouteN = ctxTFAT.FMROUTETable.Where(x => x.FmNo == fMMaster.FmNo && x.RouteType == "R").OrderByDescending(x => x.SequenceRoute).FirstOrDefault();
                var LastBranchN = ctxTFAT.TfatBranch.Where(x => x.Code == LastRouteN.Parent).Select(x => x.Name).FirstOrDefault() + ",";
                string resdsult = fMMaster.RouteViaName.Replace(LastBranchN, "");
                fMMaster.RouteViaName = resdsult + ViaName + LastBranchN;

                fMMaster.SelectedRoute += ViaName;
                ctxTFAT.Entry(fMMaster).State = EntityState.Modified;
            }

            ctxTFAT.SaveChanges();

            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);
        }



        public ActionResult GetInCurrentBranch()
        {
            string BranchCode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
            var Areas = GetChildGrp(mbranchcode);
            List<ArrivalDispatchVM> List = TempData.Peek("LoadingList") as List<ArrivalDispatchVM>;
            List<LCModal> lCModals = new List<LCModal>();
            if (List == null)
            {
                List = new List<ArrivalDispatchVM>();
                var GetLoadingList = ctxTFAT.FMROUTETable.Where(x => Areas.Contains(x.RouteVia)).ToList();
                foreach (var item in GetLoadingList)
                {
                    bool loadF = false;
                    FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.FmNo == item.FmNo).FirstOrDefault();
                    ArrivalDispatchVM arrivalDispatchVM = new ArrivalDispatchVM();
                    arrivalDispatchVM.Fmno = item.FmNo.ToString();
                    arrivalDispatchVM.VehicleNo = fMMaster.TruckNo.ToString();
                    arrivalDispatchVM.FMDate = fMMaster.Date.ToShortDateString();
                    #region Route Details
                    var RouteDetails = fMMaster.RouteVia.Split(',');
                    int last = RouteDetails.Length - 1;
                    string Route = "";
                    for (int i = 0; i < RouteDetails.Length; i++)
                    {
                        var index = RouteDetails[i].ToString();
                        if (i == 0)
                        {
                            arrivalDispatchVM.From = ctxTFAT.TfatBranch.Where(x => x.Code == index).Select(x => x.Name).FirstOrDefault();
                        }
                        else if (last == i)
                        {
                            arrivalDispatchVM.To = ctxTFAT.TfatBranch.Where(x => x.Code == index).Select(x => x.Name).FirstOrDefault();
                        }
                        else
                        {
                            Route += ctxTFAT.TfatBranch.Where(x => x.Code == index).Select(x => x.Name).FirstOrDefault() + ",";
                        }
                    }
                    if (!String.IsNullOrEmpty(Route))
                    {
                        arrivalDispatchVM.Route = Route.Substring(0, Route.Length - 1);
                    }
                    #endregion
                    if (!String.IsNullOrEmpty(item.LCNO))
                    {
                        loadF = true;
                        arrivalDispatchVM.LCno = item.LCNO;
                        var GetLCNo = item.LCNO.Split(',');
                        foreach (var lcmaster in GetLCNo)
                        {
                            List<LRModal> lRModals = new List<LRModal>();
                            var GetLCDetail = ctxTFAT.LCMaster.Where(x => x.LCno.ToString() == lcmaster.ToString()).FirstOrDefault();
                            LCModal lCModal = new LCModal
                            {
                                Date = GetLCDetail.Date.ToShortDateString(),
                                Time = GetLCDetail.Time,
                                lcno = lcmaster.ToString(),
                                TotalQty = GetLCDetail.TotalQty,
                                From = GetLCDetail.FromBranch,
                                From_Name = ctxTFAT.TfatBranch.Where(x => x.Code == GetLCDetail.FromBranch).Select(x => x.Name).FirstOrDefault(),
                                To = GetLCDetail.ToBranch,
                                To_Name = ctxTFAT.TfatBranch.Where(x => x.Code == GetLCDetail.ToBranch).Select(x => x.Name).FirstOrDefault(),
                                Weight = 0
                            };
                            var GetCurrentLrListOfLC = ctxTFAT.LCDetail.Where(x => x.LCno.ToString() == lcmaster.ToString()).Select(x => x.LRno).ToList().Distinct().ToArray();
                            foreach (var item1 in GetCurrentLrListOfLC)
                            {
                                var GetLRDetail = ctxTFAT.LCDetail.Where(x => x.LRno == item1 && x.LCno.ToString() == lcmaster.ToString()).FirstOrDefault();
                                LRModal lRModal = new LRModal
                                {
                                    Lcno = lcmaster.ToString(),
                                    Date = GetLRDetail.Date.ToShortDateString(),
                                    Time = GetLRDetail.Time,
                                    Lrno = item1,
                                    Qty = GetLRDetail.Qty,
                                    From = GetLRDetail.FromBranch,
                                    From_Name = ctxTFAT.TfatBranch.Where(x => x.Code == GetLRDetail.FromBranch).Select(x => x.Name).FirstOrDefault(),
                                    To = GetLRDetail.ToBranch,
                                    To_Name = ctxTFAT.TfatBranch.Where(x => x.Code == GetLRDetail.ToBranch).Select(x => x.Name).FirstOrDefault(),
                                    Weight = GetLRDetail.ChrWeight,
                                    ChargeType = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == GetLRDetail.ChrgeType).Select(x => x.ChargeType).FirstOrDefault(),
                                    PorductType = ctxTFAT.UnitMaster.Where(x => x.Code == GetLRDetail.Unit).Select(x => x.Name).FirstOrDefault()
                                };
                                lRModals.Add(lRModal);
                            }
                            lCModal.Weight = lRModals.Select(x => x.Weight).ToList().Sum();
                            lCModal.LrListOfLC = lRModals;
                            arrivalDispatchVM.NoofLr = lRModals.Count.ToString();
                            arrivalDispatchVM.TotalWeight = lRModals.Sum(x => x.Weight).ToString();
                            arrivalDispatchVM.TotalQty = lRModals.Sum(x => x.Qty).ToString();
                            lCModals.Add(lCModal);
                        }
                    }
                    //arrivalDispatchVM.lCModals = lCModals;
                    if (loadF)
                    {
                        List.Add(arrivalDispatchVM);
                    }
                }
            }


            TempData["LoadingList"] = List;
            TempData["PendingLC"] = lCModals;

            var html = ViewHelper.RenderPartialView(this, "_LoadingList", List);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFmWiseLoadingDetails(string Lcno)
        {
            LoadingDispachVM mModel = new LoadingDispachVM();
            List<LCModal> lCModalsList = TempData.Peek("PendingLC") as List<LCModal>;
            var lcSplitByComma = Lcno.Split(',');
            List<LCModal> newLclist = new List<LCModal>();
            foreach (var item in lcSplitByComma)
            {
                if (lCModalsList.Where(x => x.lcno == item).FirstOrDefault() != null)
                {
                    newLclist.Add(lCModalsList.Where(x => x.lcno == item).FirstOrDefault());
                }
            }
            mModel.LClist = newLclist;
            var html = ViewHelper.RenderPartialView(this, "_GetPendingLCList", mModel);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public int GetNewAttachCode()
        {
            string Code = ctxTFAT.Attachment.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).Take(1).FirstOrDefault();
            if (String.IsNullOrEmpty(Code))
            {
                return 100000;
            }
            else
            {
                return Convert.ToInt32(Code) + 1;
            }
        }




    }
}