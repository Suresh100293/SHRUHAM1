using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class MultiDeliveryController : BaseController
    {
        string attachmentPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentPath"].ToString()) == true ? "" : ConfigurationManager.AppSettings["AttachmentPath"].ToString();
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private string mnewrecordkey = "";
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        public static string connstring = "";
        private string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        public new string StartDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
        public new string EndDate = System.Web.HttpContext.Current.Session["LastDate"].ToString();

        public string GetNewCode()
        {
            var mPrevSrl = GetLastSerial("DeliveryMaster", mbranchcode, "DELV0", mperiod, "RP", DateTime.Now.Date);
            return mPrevSrl.ToString();
        }


        // GET: Logistics/MultiDelivery
        public ActionResult Index(MultiDeliveryVM mModel)
        {
            TempData["MultiConsignments"] = null;
            GetAllMenu(Session["ModuleName"].ToString());
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");
            mdocument = mModel.Document;
            mModel.StartDate = StartDate;
            mModel.EndDate = EndDate;
            mModel.DeliveryDays = "1";
            mModel.GridDetails = new List<MultiDeliveryVM>();
            return View(mModel);
        }

        #region LCMergeData JqGrid Combo

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
            return GetGridDataColumns(id, "L", "XXXX");
        }
        //All Stock
        public ActionResult GetGridData(GridOption Model)
        {
            ExecuteStoredProc("Drop Table tempAllGodownMultiStock");
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("SP_AllGodownMultiStock", tfat_conx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@mFromDate", SqlDbType.VarChar).Value = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            cmd.Parameters.Add("@mToDate", SqlDbType.VarChar).Value = (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            cmd.Parameters.Add("@mStockTableKeyList", SqlDbType.VarChar).Value = Model.mVar4 == null ? "" : Model.mVar4;
            cmd.Parameters.Add("@mAddBookDate", SqlDbType.VarChar).Value = Model.mVar5 == null ? "0" : Model.mVar5;
            cmd.Parameters.Add("@mStockType", SqlDbType.VarChar).Value = "'LR','TRN'";
            cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;

            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();

            string mSelectQuery = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
            tfat_conx.Close();

            return GetGridReport(Model, "M", "", false, 0);
        }
        //Godown Stock
        public ActionResult GetGridData1(GridOption Model)
        {
            ExecuteStoredProc("Drop Table tempAllGodownMultiStock");
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("SP_AllGodownMultiStock", tfat_conx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@mFromDate", SqlDbType.VarChar).Value = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            cmd.Parameters.Add("@mToDate", SqlDbType.VarChar).Value = (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            cmd.Parameters.Add("@mStockTableKeyList", SqlDbType.VarChar).Value = Model.mVar4 == null ? "" : Model.mVar4;
            cmd.Parameters.Add("@mAddBookDate", SqlDbType.VarChar).Value = Model.mVar5 == null ? "0" : Model.mVar5;
            cmd.Parameters.Add("@mStockType", SqlDbType.VarChar).Value = "'LR'";
            cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;

            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();

            string mSelectQuery = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
            tfat_conx.Close();

            return GetGridReport(Model, "M", "", false, 0);
        }
        //Transit Stock
        public ActionResult GetGridData2(GridOption Model)
        {
            ExecuteStoredProc("Drop Table tempAllGodownMultiStock");
            SqlConnection tfat_conx = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand("SP_AllGodownMultiStock", tfat_conx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            cmd.Parameters.Add("@mFromDate", SqlDbType.VarChar).Value = (Convert.ToDateTime(Model.FromDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            cmd.Parameters.Add("@mToDate", SqlDbType.VarChar).Value = (Convert.ToDateTime(Model.ToDate)).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            cmd.Parameters.Add("@mStockTableKeyList", SqlDbType.VarChar).Value = Model.mVar4 == null ? "" : Model.mVar4;
            cmd.Parameters.Add("@mAddBookDate", SqlDbType.VarChar).Value = Model.mVar5 == null ? "0" : Model.mVar5;
            cmd.Parameters.Add("@mStockType", SqlDbType.VarChar).Value = "'TRN'";
            cmd.Parameters.Add("@mReturnQuery", SqlDbType.VarChar, 5000).Value = "";
            cmd.Parameters["@mReturnQuery"].Direction = ParameterDirection.Output;

            tfat_conx.Open();
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();

            string mSelectQuery = (string)(cmd.Parameters["@mReturnQuery"].Value ?? "");
            tfat_conx.Close();

            return GetGridReport(Model, "M", "", false, 0);
        }

        //Save Consignment In Grid
        public ActionResult GridView(MultiDeliveryVM mModel)
        {
            //mModel.lCDetails.ToList().ForEach(w => w.EditLDSNo = true);
            List<MultiDeliveryVM> InserList = TempData.Peek("MultiConsignments") as List<MultiDeliveryVM>;
            if (InserList == null)
            {
                InserList = new List<MultiDeliveryVM>();
            }

            InserList.AddRange(mModel.GridDetails);
            TempData["MultiConsignments"] = InserList;
            var html = ViewHelper.RenderPartialView(this, "GridView", InserList);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteConsignment(MultiDeliveryVM mModel)
        {
            //mModel.lCDetails.ToList().ForEach(w => w.EditLDSNo = true);
            List<MultiDeliveryVM> InserList = TempData.Peek("MultiConsignments") as List<MultiDeliveryVM>;
            if (InserList == null)
            {
                InserList = new List<MultiDeliveryVM>();
            }

            InserList = InserList.Where(x => x.StockTablekey != mModel.StockTablekey).ToList();
            TempData["MultiConsignments"] = InserList;
            return Json(new { Status = "Sucess" }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public ActionResult SaveData(MultiDeliveryVM mModel)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    bool OtherBranchDel = false;
                    var DeliveryNo = Convert.ToInt32(GetNewCode());
                    List<MultiDeliveryVM> InserList = TempData.Peek("MultiConsignments") as List<MultiDeliveryVM>;
                    if (InserList == null)
                    {
                        InserList = new List<MultiDeliveryVM>();
                    }
                    foreach (var item in InserList.ToList())
                    {
                        DeliveryMaster deliveryMaster = new DeliveryMaster();
                        deliveryMaster.DeliveryNo = DeliveryNo;
                        deliveryMaster.GenerateType = "A";
                        deliveryMaster.CreateDate = DateTime.Now;
                        mnewrecordkey = deliveryMaster.DeliveryNo.ToString();
                        deliveryMaster.TableKey = mbranchcode + "DEL00" + mperiod.Substring(0, 2) + "001" + Convert.ToInt32(deliveryMaster.DeliveryNo).ToString("D6");
                        LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == item.Tablekey).FirstOrDefault();
                        deliveryMaster.LoginBranch = mbranchcode;
                        deliveryMaster.Branch = mbranchcode;
                        deliveryMaster.LrNO = Convert.ToInt32(item.Lrno);
                        deliveryMaster.DeliveryTime = "00:00";
                        deliveryMaster.DeliveryDate = ConvertDDMMYYTOYYMMDD(item.DeliveryDate);
                        deliveryMaster.Consigner = lRMaster.RecCode;
                        deliveryMaster.Consignee = lRMaster.SendCode;
                        deliveryMaster.FromBranch = lRMaster.Source;
                        deliveryMaster.ToBranch = lRMaster.Dest;
                        deliveryMaster.Qty = item.StockQty;
                        var DelWeight = lRMaster.ActWt == 0 ? 0 : ((lRMaster.ActWt / lRMaster.TotQty) * (item.StockQty));
                        deliveryMaster.Weight = DelWeight;
                        deliveryMaster.DeliveryGoodStatus = "OK";
                        deliveryMaster.ShortQty = 0;
                        deliveryMaster.DeliveryRemark = "Multi Auto Delivery...";
                        deliveryMaster.ParentKey = lRMaster.TableKey;
                        //deliveryMaster.VehicleNO = mModel.VehicleNo;
                        deliveryMaster.BillQty = 0;
                        //deliveryMaster.PersonName = mModel.PersonName;
                        //deliveryMaster.MobileNO = (mModel.MobileNO);
                        deliveryMaster.Prefix = mperiod;

                        deliveryMaster.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                        deliveryMaster.ENTEREDBY = muserid;
                        deliveryMaster.AUTHORISE = mauthorise;
                        deliveryMaster.AUTHIDS = muserid;

                        string Athorise1 = "A00";
                        #region Authorisation
                        TfatUserAuditHeader authorisation1 = ctxTFAT.TfatUserAuditHeader.Where(x => x.Type == "DELV0").FirstOrDefault();
                        if (authorisation1 != null)
                        {
                            Athorise1 = SetAuthorisationLogistics(authorisation1, deliveryMaster.TableKey, deliveryMaster.DeliveryNo.ToString(), 0, deliveryMaster.DeliveryDate.ToShortDateString(), 0, "", mbranchcode);
                            deliveryMaster.AUTHORISE = Athorise1;
                        }
                        #endregion

                        #region DelRelation
                        LRStock Lrstock = ctxTFAT.LRStock.Where(x => x.TableKey == item.StockTablekey).FirstOrDefault();
                        if (Lrstock.Branch != mbranchcode && OtherBranchDel == false)
                        {
                            OtherBranchDel = true;
                        }
                        var BalQty = ctxTFAT.LRStock.Where(x => x.ParentKey == Lrstock.TableKey && x.TableKey != deliveryMaster.TableKey).Sum(x => (int?)x.TotalQty) ?? 0;
                        var UnDispatchLC = ctxTFAT.LCMaster.Where(x => x.DispachFM == 0).Select(x => x.TableKey).ToList();
                        var LCLoadQty = ctxTFAT.LCDetail.Where(x => x.ParentKey == Lrstock.TableKey && UnDispatchLC.Contains(x.LCRefTablekey)).Sum(x => (int?)x.LrQty) ?? 0;
                        int Qty = 0; double Weight = 0;
                        Qty = item.StockQty;
                        Weight = DelWeight;
                        DelRelation delRelation = new DelRelation();
                        delRelation.DeliveryNo = deliveryMaster.DeliveryNo;
                        delRelation.Branch = Lrstock.Branch;
                        delRelation.Type = item.StockType.Trim() == "GODOWN" ? "LR" : "TRN";
                        delRelation.ParentKey = item.StockTablekey;
                        delRelation.DelQty = Qty;
                        delRelation.DelWeight = Weight;
                        delRelation.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                        delRelation.ENTEREDBY = muserid;
                        delRelation.AUTHORISE = Athorise1;
                        delRelation.AUTHIDS = muserid;
                        delRelation.Prefix = mperiod;
                        Lrstock.BalQty -= Qty;
                        Lrstock.BalWeight -= Weight;
                        ctxTFAT.Entry(Lrstock).State = EntityState.Modified;
                        if (Lrstock.BalQty < 0)
                        {
                            return Json(new
                            {
                                Status = "Error",
                                Message = "LR No: " + Lrstock.LrNo + "  Not Allowed To Delivery Due To Negative Stock!"
                            }, JsonRequestBehavior.AllowGet);
                        }
                        ctxTFAT.DelRelation.Add(delRelation);
                        #endregion

                        #region Delivery Entry In Stock
                        var LRData = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == item.Tablekey).FirstOrDefault();
                        LRStock LoadlRStock = new LRStock();
                        LoadlRStock.LoginBranch = mbranchcode;
                        LoadlRStock.Branch = mbranchcode;
                        LoadlRStock.LrNo = Convert.ToInt32(item.Lrno);
                        LoadlRStock.LoadForGodown = 0;
                        LoadlRStock.LoadForDirect = 0;
                        LoadlRStock.Date = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                        LoadlRStock.Time = (DateTime.Now.ToString("HH:mm"));
                        LoadlRStock.TotalQty = Qty;
                        LoadlRStock.AllocatBalQty = Qty;
                        LoadlRStock.BalQty = Qty;
                        LoadlRStock.ActWeight = Weight;
                        LoadlRStock.AllocatBalWght = Weight;
                        LoadlRStock.BalWeight = Weight;
                        LoadlRStock.ChrgWeight = LRData.ChgWt;
                        LoadlRStock.ChrgType = LRData.ChgType;
                        LoadlRStock.Description = LRData.DescrType;
                        LoadlRStock.Unit = LRData.UnitCode;
                        LoadlRStock.FromBranch = LRData.Source;
                        LoadlRStock.ToBranch = LRData.Dest;
                        LoadlRStock.Consigner = LRData.RecCode;
                        LoadlRStock.Consignee = LRData.SendCode;
                        LoadlRStock.LrType = LRData.LRtype;
                        LoadlRStock.Coln = LRData.Colln;
                        LoadlRStock.Delivery = LRData.Delivery;
                        LoadlRStock.Remark = "";
                        LoadlRStock.StockAt = "Delivery";
                        LoadlRStock.StockStatus = "D";
                        LoadlRStock.LCNO = Lrstock.LCNO;
                        LoadlRStock.AUTHIDS = muserid;
                        LoadlRStock.AUTHORISE = mauthorise;
                        LoadlRStock.ENTEREDBY = muserid;
                        LoadlRStock.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());
                        LoadlRStock.UnloadDirectQty = 0;
                        LoadlRStock.UnloadGodwonQty = 0;
                        LoadlRStock.Fmno = Lrstock.Fmno;
                        LoadlRStock.TableKey = mbranchcode + "DEL00" + mperiod.Substring(0, 2) + 1.ToString("D3") + deliveryMaster.DeliveryNo;
                        LoadlRStock.ParentKey = item.StockTablekey;
                        LoadlRStock.LRRefTablekey = Lrstock.LRRefTablekey;
                        LoadlRStock.LCRefTablekey = Lrstock.LCRefTablekey;
                        LoadlRStock.FMRefTablekey = Lrstock.FMRefTablekey;
                        LoadlRStock.Type = "DEL";
                        LoadlRStock.LRMode = LRData.LRMode;
                        LoadlRStock.Prefix = mperiod;
                        ctxTFAT.LRStock.Add(LoadlRStock);
                        #endregion

                        deliveryMaster.MultiDel = true;
                        ctxTFAT.DeliveryMaster.Add(deliveryMaster);
                        DeliveryNotification(deliveryMaster, OtherBranchDel,"MultiDelivery");
                        DeliveryNo++;
                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", " Save Multi Deliveries", "NA");

                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.InnerException.Message, Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { SerialNo = mdocument, Status = "Success", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteMultiDelivery(MultiDeliveryVM mModel)
        {
            string Message = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var ChildList = GetChildGrp(mbranchcode);
                    if (mModel.GridDetails == null)
                    {
                        return Json(new { Message = "Documents Not Found For Delevery...!", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        foreach (var item in mModel.GridDetails)
                        {
                            var deliveryMaster = ctxTFAT.DeliveryMaster.Where(x => x.RECORDKEY.ToString() == item.Document).FirstOrDefault();
                            if (ctxTFAT.PeriodLock.Where(x => x.Branch == mbranchcode && x.Type == "DELV0" && x.LockDate == deliveryMaster.DeliveryDate).FirstOrDefault() != null)
                            {
                                Message += "Delivery Date Locked Of :- " + deliveryMaster.DeliveryNo + " By Period Locking System..\n";
                            }
                            else
                            {
                                var DeliveryRelList = ctxTFAT.DelRelation.Where(x => x.DeliveryNo == deliveryMaster.DeliveryNo && x.Prefix == deliveryMaster.Prefix).ToList();
                                bool deleteDeliveryOrNot = true;
                                LRStock lRStock=new LRStock();
                                foreach (var delRelation in DeliveryRelList)
                                {
                                    if (deleteDeliveryOrNot)
                                    {
                                        lRStock = ctxTFAT.LRStock.Where(x => x.TableKey.ToString().Trim() == delRelation.ParentKey.Trim()).FirstOrDefault();
                                        if (lRStock.Type == "TRN")
                                        {
                                            FMMaster fM = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == lRStock.FMRefTablekey.ToString()).FirstOrDefault();
                                            if (fM != null)
                                            {
                                                if (fM.FmStatus == "CC")
                                                {
                                                    deleteDeliveryOrNot = false;
                                                    Message += "Not Allow To Delete " + deliveryMaster.DeliveryNo + " Delivery....!\n Because OF Fm Completed.";
                                                }
                                                if (fM.ActivityFollowup == true)
                                                {
                                                    if (!ChildList.Contains(fM.CurrBranch))
                                                    {
                                                        deleteDeliveryOrNot = false;
                                                        Message += "Not Allow To Delete "+ deliveryMaster.DeliveryNo + " Delivery....! \n Bcoz Fm Not In Our Branch...!";
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    
                                }
                                if (deleteDeliveryOrNot)
                                {
                                    #region Delete Old DeliveryStk Entry
                                    var GetParentKeyOfDeliveryLRNO = ctxTFAT.DelRelation.Where(x => x.DeliveryNo == deliveryMaster.DeliveryNo).ToList().Count();
                                    for (int i = 1; i <= GetParentKeyOfDeliveryLRNO; i++)
                                    {
                                        var tablekey = mbranchcode + "DEL00" + mperiod.Substring(0, 2) + i.ToString("D3") + deliveryMaster.DeliveryNo;
                                        var OldDelStkEntry = ctxTFAT.LRStock.Where(x => x.TableKey == tablekey && x.Type == "DEL").FirstOrDefault();
                                        if (OldDelStkEntry != null)
                                        {
                                            ctxTFAT.LRStock.Remove(OldDelStkEntry);
                                        }
                                    }
                                    #endregion
                                    foreach (var del in DeliveryRelList)
                                    {
                                        lRStock.BalQty += del.DelQty;
                                        lRStock.BalWeight += del.DelWeight.Value;
                                        ctxTFAT.Entry(lRStock).State = EntityState.Modified;

                                        ctxTFAT.DelRelation.Remove(del);
                                    }
                                    var AuthorisationEntry = ctxTFAT.Authorisation.Where(x => x.Type == "DELV0" && x.Srl == deliveryMaster.DeliveryNo.ToString()).FirstOrDefault();
                                    if (AuthorisationEntry != null)
                                    {
                                        ctxTFAT.Authorisation.Remove(AuthorisationEntry);
                                    }
                                    ctxTFAT.DeliveryMaster.Remove(deliveryMaster);
                                }
                            }
                        }
                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", " Delete Multi Deliveries", "NA");

                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex1.InnerException.Message, Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.InnerException.Message, Status = "Error", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Unable to Save Changes. Try again, and if the problem persists, Contact your System Administrator.");
                    ViewBag.Message = "Error, While updating the Data..\n" + e.Message;
                    return Json(new { Message = "Error, While updating the Data..\n" + e.Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Message = Message, Status = "Success", id = "LRMaster" }, JsonRequestBehavior.AllowGet);
        }
    }
}