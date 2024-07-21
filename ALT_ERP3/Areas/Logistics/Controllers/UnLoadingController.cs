using ALT_ERP3.Areas.Accounts.Models;
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
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class UnLoadingController : BaseController
    {
        string attachmentPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentPath"].ToString()) == true ? "" : ConfigurationManager.AppSettings["AttachmentPath"].ToString();
        private static string mdocument = "";
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        private static string mauthorise = "A00";
        //ITransactionGridOperation mIlst = new TransactionGridOperation();
        private static string connstring;


        public ActionResult SaveData(LoadingDispachVM mModel)
        {
            string Message = "", Parentkey = "", UNLR = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var Child = GetChildGrp(mbranchcode);
                    FMROUTETable fM_ROUTE = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
                    if (fM_ROUTE != null)
                    {
                        FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == fM_ROUTE.Parentkey.ToString()).FirstOrDefault();
                        if (fMMaster != null)
                        {
                            Parentkey = fMMaster.ParentKey;
                            if (mModel.LRLIst != null)
                            {
                                if (mModel.LRLIst.Count() > 0)
                                {
                                    var RecordkeyOfLrForUnload = mModel.LRLIst.Select(x => x.recordkey).ToList();
                                    var LR_Wise_VehicleMaterial = ctxTFAT.LRStock.Where(x => RecordkeyOfLrForUnload.Contains(x.TableKey)).ToList();
                                    int Qty = 0;
                                    double Weight = 0;
                                    for (int i = 0; i < LR_Wise_VehicleMaterial.Count; i++)
                                    {
                                        LRStock VehicleMaterial = LR_Wise_VehicleMaterial[i];
                                        LRStock OldlRStock;
                                        LRMaster lRMaster;
                                        var CurrentDataForUnload = mModel.LRLIst.Where(x => x.recordkey == VehicleMaterial.TableKey).FirstOrDefault();
                                        int unlG = 0, unlD = 0;
                                        if (VehicleMaterial.TableKey.Contains("TRN00"))
                                        {
                                            VehicleMaterial = ctxTFAT.LRStock.Where(x => x.TableKey == VehicleMaterial.TableKey).FirstOrDefault();
                                            OldlRStock = ctxTFAT.LRStock.Where(x => Child.Contains(x.Branch) && x.ParentKey == VehicleMaterial.TableKey && x.Type == "LR").FirstOrDefault();
                                            lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey == VehicleMaterial.LRRefTablekey).FirstOrDefault();
                                        }
                                        else
                                        {
                                            VehicleMaterial = ctxTFAT.LRStock.Where(x => x.TableKey == VehicleMaterial.ParentKey).FirstOrDefault();
                                            OldlRStock = ctxTFAT.LRStock.Where(x => Child.Contains(x.Branch) && x.ParentKey == VehicleMaterial.TableKey && x.Type == "LR").FirstOrDefault();
                                            lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey == OldlRStock.LRRefTablekey).FirstOrDefault();
                                        }
                                        if (ctxTFAT.UnLoadDetails.Where(x => Child.Contains(x.Branch) && x.LRRefTablekey == VehicleMaterial.LRRefTablekey && x.LCRefTablekey == VehicleMaterial.LCRefTablekey && x.FMRefTablekey == fMMaster.TableKey && x.VehicleNO == fMMaster.TruckNo).FirstOrDefault() != null)
                                        {
                                            UnLoadDetails unLoad = ctxTFAT.UnLoadDetails.Where(x => Child.Contains(x.Branch) && x.LRRefTablekey == VehicleMaterial.LRRefTablekey && x.LCRefTablekey == VehicleMaterial.LCRefTablekey && x.FMRefTablekey == fMMaster.TableKey && x.VehicleNO == fMMaster.TruckNo).FirstOrDefault();
                                            unlG = unLoad.GQty;
                                            unlD = unLoad.DQty;
                                        }


                                        #region Godown Qty
                                        if (unlG > 0)//Unload It Already Some Material
                                        {
                                            if (unlG != CurrentDataForUnload.unloadGQty)
                                            {
                                                if (unlG >= CurrentDataForUnload.unloadGQty)//Reverse Qty
                                                {
                                                    Qty = Convert.ToInt32(unlG) - CurrentDataForUnload.unloadGQty;
                                                    Weight = Convert.ToDouble(((Convert.ToDecimal(Qty)) / ((decimal)VehicleMaterial.TotalQty) * ((decimal)VehicleMaterial.ActWeight)));
                                                    string Msg = UnloadReverse(Qty, Weight, VehicleMaterial, OldlRStock, "Godown", fM_ROUTE.LCNO);
                                                    if (!string.IsNullOrEmpty(Msg))
                                                    {
                                                        if (Msg == "Error")
                                                        {
                                                            Message = "Not Allowed To Unload " + VehicleMaterial.LrNo + " Consignment Due To Negative Stock...!";
                                                            return Json(new { Message = Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                                                        }
                                                    }
                                                    Message += Msg;
                                                }
                                                else//Unload Qty 
                                                {
                                                    Qty = CurrentDataForUnload.unloadGQty - Convert.ToInt32(unlG);
                                                    Weight = Convert.ToDouble(((Convert.ToDecimal(Qty)) / ((decimal)VehicleMaterial.TotalQty) * ((decimal)VehicleMaterial.ActWeight)));
                                                    string Msg = Unload(Qty, Weight, mbranchcode, VehicleMaterial, OldlRStock, "Godown", fM_ROUTE.LCNO, fM_ROUTE);
                                                    if (!string.IsNullOrEmpty(Msg))
                                                    {
                                                        if (Msg== "Error")
                                                        {
                                                            Message = "Not Allowed To Unload "+ VehicleMaterial.LrNo + " Consignment...!\n Please Contact To Admin...!";
                                                            return Json(new { Message = Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                                                        }
                                                    }
                                                    Message += Msg;
                                                }
                                            }
                                        }
                                        else//First Time Unload
                                        {
                                            if (CurrentDataForUnload.unloadGQty > 0)
                                            {
                                                Qty = CurrentDataForUnload.unloadGQty - Convert.ToInt32(unlG);
                                                Weight = Convert.ToDouble(((Convert.ToDecimal(Qty)) / ((decimal)VehicleMaterial.TotalQty) * ((decimal)VehicleMaterial.ActWeight)));
                                                string Msg = Unload(Qty, Weight, mbranchcode, VehicleMaterial, OldlRStock, "Godown", fM_ROUTE.LCNO, fM_ROUTE);
                                                if (!string.IsNullOrEmpty(Msg))
                                                {
                                                    if (Msg == "Error")
                                                    {
                                                        Message = "Not Allowed To Unload " + VehicleMaterial.LrNo + " Consignment Due To Negative Stock...!";
                                                        return Json(new { Message = Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                                                    }
                                                }
                                                Message += Msg;
                                            }
                                        }
                                        #endregion
                                    }
                                }
                            }

                            #region Update UnloadLc In Fmroute
                            List<LRModal> AlreadyUnloadLrList = TempData.Peek("AlreadyUnloadLrList") as List<LRModal>;
                            if (AlreadyUnloadLrList == null)
                            {
                                AlreadyUnloadLrList = new List<LRModal>();
                            }
                            string UnloadLC = "", UnloadLCKey = "";
                            foreach (var item in AlreadyUnloadLrList.Distinct().ToList())
                            {
                                UnloadLC += item.Lcno + ",";
                                LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey == item.recordkey).FirstOrDefault();
                                UnloadLCKey += lRStock.LCRefTablekey + ",";
                            }
                            if (String.IsNullOrEmpty(UnloadLC))
                            {
                                fM_ROUTE.UnLoadLCNO = "";
                                fM_ROUTE.UNLODRefTablekey = "";
                            }
                            else
                            {
                                fM_ROUTE.UnLoadLCNO = UnloadLC.Substring(0, UnloadLC.Length - 1);
                                fM_ROUTE.UNLODRefTablekey = UnloadLCKey.Substring(0, UnloadLCKey.Length - 1);
                            }
                            ctxTFAT.Entry(fM_ROUTE).State = EntityState.Modified;

                            #endregion
                        }
                        else
                        {
                            return Json(new { Message = "Error,FM Not Fount..\n DD", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { Message = "Error,Route Not Fount..\n", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, "UnLoading Vehicle", Parentkey, DateTime.Now, 0, "", Message, "NA");

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

        public string Unload(int Qty, double Weight, string BranchCode, LRStock LrVehicleLoadEntry, LRStock OldlRStock, string Type, string CurrRouteLoadLcNO, FMROUTETable fMROUTE)
        {
            bool StockUpdateOrNot = true;
            string ErroMsg = "";
            var Child = GetChildGrp(mbranchcode);
            if (Type == "Godown")
            {
                if (OldlRStock == null)
                {
                    LRStock newlRStock = new LRStock();
                    newlRStock.LoginBranch = mbranchcode;
                    newlRStock.Branch = BranchCode;
                    newlRStock.LrNo = LrVehicleLoadEntry.LrNo;
                    newlRStock.LCNO = LrVehicleLoadEntry.LCNO;
                    newlRStock.Fmno = LrVehicleLoadEntry.Fmno;
                    newlRStock.Date = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString().Trim());
                    newlRStock.Time = DateTime.Now.ToString("HH:mm");
                    newlRStock.TotalQty = Convert.ToInt32(Qty);
                    newlRStock.AllocatBalQty = Convert.ToInt32(Qty);
                    newlRStock.AllocatBalWght = Weight;
                    newlRStock.BalQty = Convert.ToInt32(Qty);
                    newlRStock.ChrgWeight = LrVehicleLoadEntry.ChrgWeight;
                    newlRStock.ActWeight = Weight;
                    newlRStock.BalWeight = Weight;
                    newlRStock.ChrgType = LrVehicleLoadEntry.ChrgType;
                    newlRStock.Description = LrVehicleLoadEntry.Description == null ? "" : LrVehicleLoadEntry.Description;
                    newlRStock.Unit = LrVehicleLoadEntry.Unit;
                    newlRStock.FromBranch = LrVehicleLoadEntry.FromBranch;
                    newlRStock.ToBranch = LrVehicleLoadEntry.ToBranch;
                    newlRStock.Consigner = LrVehicleLoadEntry.Consigner;
                    newlRStock.Consignee = LrVehicleLoadEntry.Consignee;
                    newlRStock.LrType = LrVehicleLoadEntry.LrType;
                    newlRStock.Coln = LrVehicleLoadEntry.Coln;
                    newlRStock.Delivery = LrVehicleLoadEntry.Delivery;
                    newlRStock.LRMode = LrVehicleLoadEntry.LRMode;
                    newlRStock.Remark = null;
                    newlRStock.Type = "LR";
                    newlRStock.ENTEREDBY = muserid;
                    newlRStock.AUTHIDS = muserid;
                    newlRStock.AUTHORISE = mauthorise;
                    newlRStock.Prefix = mperiod;
                    newlRStock.LRRefTablekey = LrVehicleLoadEntry.LRRefTablekey;
                    newlRStock.LCRefTablekey = LrVehicleLoadEntry.LCRefTablekey;
                    newlRStock.FMRefTablekey = LrVehicleLoadEntry.FMRefTablekey;
                    newlRStock.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    if (LrVehicleLoadEntry.Coln == "G")
                    {
                        newlRStock.StockAt = "Godown";
                        newlRStock.StockStatus = "G";
                    }
                    else
                    {
                        newlRStock.StockAt = "Pick";
                        newlRStock.StockStatus = "P";
                    }
                    var PreviousLrTableKey = ctxTFAT.LRStock.Where(x => x.LRRefTablekey == LrVehicleLoadEntry.LRRefTablekey && x.Type == "LR").OrderByDescending(x => x.RECORDKEY).Select(x => x.TableKey).FirstOrDefault();
                    int Sno = 1;
                    if (!String.IsNullOrEmpty(PreviousLrTableKey))
                    {
                        var CheckBranchCode = PreviousLrTableKey.Substring(0, 6);
                        if (ctxTFAT.TfatBranch.Where(x => x.Code == CheckBranchCode).FirstOrDefault() == null)
                        {
                            Sno = Convert.ToInt32(PreviousLrTableKey.Substring(7, 3)) + 1;
                        }
                        else
                        {
                            Sno = Convert.ToInt32(PreviousLrTableKey.Substring(13, 3)) + 1;
                        }
                    }

                    newlRStock.TableKey = mbranchcode + "STK00" + mperiod.Substring(0, 2) + Sno.ToString("D3") + LrVehicleLoadEntry.LrNo;
                    newlRStock.ParentKey = LrVehicleLoadEntry.TableKey;
                    var RStock = ctxTFAT.LRStock.Where(x => Child.Contains(x.Branch) && x.ParentKey == LrVehicleLoadEntry.TableKey && x.Type == "LR").FirstOrDefault();
                    if (RStock == null)
                    {
                        LrVehicleLoadEntry.BalQty -= Qty;
                        LrVehicleLoadEntry.BalWeight -= Weight;
                        ctxTFAT.Entry(LrVehicleLoadEntry).State = EntityState.Modified;
                        if (LrVehicleLoadEntry.BalQty < 0)
                        {
                            return "Error";
                        }


                        if (LrVehicleLoadEntry.Branch != mbranchcode)
                        {
                            UnloadOtherBranchMaterialNotification(fMROUTE, LrVehicleLoadEntry.LRRefTablekey);
                        }
                        ctxTFAT.LRStock.Add(newlRStock);
                        ErroMsg += "\n" + Qty + "  Qty Of  " + newlRStock.LrNo + " Consignment Is Sucessfully Unloded.\n";
                    }
                    else
                    {
                        ErroMsg += LrVehicleLoadEntry.LrNo + "This Consignment Of Some Material Already Unloded So PLease Refresh Your Page" + "\n,";
                        StockUpdateOrNot = false;
                    }
                }
                else
                {
                    var RStock = ctxTFAT.LRStock.Where(x => x.TableKey == LrVehicleLoadEntry.TableKey).FirstOrDefault();
                    if (RStock.BalQty >= Qty)
                    {
                        OldlRStock.TotalQty += Convert.ToInt32(Qty);
                        OldlRStock.ActWeight += Weight;
                        OldlRStock.AllocatBalQty += Convert.ToInt32(Qty);
                        OldlRStock.AllocatBalWght += Weight;
                        OldlRStock.BalQty += Convert.ToInt32(Qty);
                        OldlRStock.BalWeight += Weight;

                        LrVehicleLoadEntry.BalQty -= Qty;
                        LrVehicleLoadEntry.BalWeight -= Weight;
                        ctxTFAT.Entry(LrVehicleLoadEntry).State = EntityState.Modified;
                        if (LrVehicleLoadEntry.BalQty < 0)
                        {
                            return "Error";
                        }
                        if (LrVehicleLoadEntry.Branch != mbranchcode)
                        {
                            UnloadOtherBranchMaterialNotification(fMROUTE, LrVehicleLoadEntry.LRRefTablekey);
                        }
                        ErroMsg += "\n" + Qty + "  Qty Of  " + OldlRStock.LrNo + " LR Is Sucessfully Unloded.\n";
                        ctxTFAT.Entry(OldlRStock).State = EntityState.Modified;
                    }
                    else
                    {
                        ErroMsg += "   Available Material Of This " + LrVehicleLoadEntry.LrNo + " Lr Is " + RStock.BalQty + "" + "\n,";
                        StockUpdateOrNot = false;
                    }
                }
            }

            if (StockUpdateOrNot)
            {
                //LrVehicleLoadEntry.AllocatBalQty -= Convert.ToInt32(Qty);
                //LrVehicleLoadEntry.BalQty -= Convert.ToInt32(Qty);
                //LrVehicleLoadEntry.AllocatBalWght -= Convert.ToInt32(Weight);
                //LrVehicleLoadEntry.BalWeight -= Convert.ToInt32(Weight);
                if (Type == "Godown")
                {
                    LrVehicleLoadEntry.UnloadGodwonQty += Convert.ToInt32(Qty);
                }
                else
                {
                    LrVehicleLoadEntry.UnloadDirectQty += Convert.ToInt32(Qty);
                }
                ctxTFAT.Entry(LrVehicleLoadEntry).State = EntityState.Modified;

                UnLoadDetails unLoadDetails = new UnLoadDetails();
                bool AddEntry = true;
                if (ctxTFAT.UnLoadDetails.Where(x => Child.Contains(x.Branch) && x.LRRefTablekey == LrVehicleLoadEntry.LRRefTablekey && x.LCRefTablekey == LrVehicleLoadEntry.LCRefTablekey && x.FMRefTablekey == LrVehicleLoadEntry.FMRefTablekey && x.VehicleNO == LrVehicleLoadEntry.StockAt).FirstOrDefault() != null)
                {
                    unLoadDetails = ctxTFAT.UnLoadDetails.Where(x => Child.Contains(x.Branch) && x.LRRefTablekey == LrVehicleLoadEntry.LRRefTablekey && x.LCRefTablekey == LrVehicleLoadEntry.LCRefTablekey && x.FMRefTablekey == LrVehicleLoadEntry.FMRefTablekey && x.VehicleNO == LrVehicleLoadEntry.StockAt).FirstOrDefault();
                    AddEntry = false;
                }
                unLoadDetails.Branch = mbranchcode;
                unLoadDetails.LrNo = LrVehicleLoadEntry.LrNo.Value;
                unLoadDetails.LRRefTablekey = LrVehicleLoadEntry.LRRefTablekey;
                unLoadDetails.LCNO = LrVehicleLoadEntry.LCNO.Value;
                unLoadDetails.LCRefTablekey = LrVehicleLoadEntry.LCRefTablekey;
                unLoadDetails.FMNO = LrVehicleLoadEntry.Fmno.Value;
                unLoadDetails.FMRefTablekey = LrVehicleLoadEntry.FMRefTablekey;
                unLoadDetails.VehicleNO = LrVehicleLoadEntry.StockAt.ToString();
                unLoadDetails.Date = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                unLoadDetails.Time = (DateTime.Now.ToString("HH:mm"));
                unLoadDetails.GQty += Qty;
                unLoadDetails.Weight += Weight;
                unLoadDetails.ChrgWeight = LrVehicleLoadEntry.ChrgWeight;
                unLoadDetails.ChrgType = LrVehicleLoadEntry.ChrgType;
                unLoadDetails.Description = LrVehicleLoadEntry.Description == null ? "" : LrVehicleLoadEntry.Description;
                unLoadDetails.Unit = LrVehicleLoadEntry.Unit;
                unLoadDetails.FromBranch = LrVehicleLoadEntry.FromBranch;
                unLoadDetails.ToBranch = LrVehicleLoadEntry.ToBranch;
                unLoadDetails.Consigner = LrVehicleLoadEntry.Consigner;
                unLoadDetails.Consignee = LrVehicleLoadEntry.Consignee;
                unLoadDetails.LrType = LrVehicleLoadEntry.LrType;
                unLoadDetails.Coln = LrVehicleLoadEntry.Coln;
                unLoadDetails.Delivery = (LrVehicleLoadEntry.Delivery);
                unLoadDetails.Remark = LrVehicleLoadEntry.Remark;
                unLoadDetails.ENTEREDBY = muserid;
                unLoadDetails.AUTHIDS = muserid;
                unLoadDetails.AUTHORISE = mauthorise;
                unLoadDetails.Prefix = mperiod;
                unLoadDetails.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());

                if (AddEntry)
                {
                    ctxTFAT.UnLoadDetails.Add(unLoadDetails);
                }
                else
                {
                    ctxTFAT.Entry(unLoadDetails).State = EntityState.Modified;
                }
            }

            return ErroMsg;

        }

        public string UnloadReverse(int Qty, double Weight, LRStock LrVehicleLoadEntry, LRStock OldlRStock, string Type, string CurrRouteLoadLcNO)
        {
            bool StockUpdateOrNot = true;
            string ErroMsg = "";
            var Child = GetChildGrp(mbranchcode);
            if (Type == "Godown")
            {
                if (OldlRStock != null)
                {
                    if (OldlRStock.AllocatBalQty >= Convert.ToInt32(Qty))
                    {
                        //LrVehicleLoadEntry.AllocatBalQty += Convert.ToInt32(Qty);
                        //LrVehicleLoadEntry.BalQty += Convert.ToInt32(Qty);
                        //LrVehicleLoadEntry.AllocatBalWght += Convert.ToInt32(Weight);
                        //LrVehicleLoadEntry.BalWeight += Convert.ToInt32(Weight);
                        if (Type == "Godown")
                        {
                            LrVehicleLoadEntry.BalQty += Convert.ToInt32(Qty);
                            LrVehicleLoadEntry.BalWeight += Weight;
                            LrVehicleLoadEntry.UnloadGodwonQty -= Convert.ToInt32(Qty);
                        }
                        else
                        {
                            LrVehicleLoadEntry.UnloadDirectQty -= Convert.ToInt32(Qty);
                        }
                        var RStock = ctxTFAT.LRStock.Where(x => x.TableKey == OldlRStock.TableKey).FirstOrDefault();
                        if (RStock.BalQty >= Qty)
                        {
                            OldlRStock.TotalQty -= Convert.ToInt32(Qty);
                            OldlRStock.ActWeight -= Convert.ToInt32(Weight);
                            OldlRStock.AllocatBalQty -= Convert.ToInt32(Qty);
                            OldlRStock.BalQty -= Convert.ToInt32(Qty);
                            OldlRStock.AllocatBalWght -= Convert.ToInt32(Weight);
                            OldlRStock.BalWeight -= Convert.ToInt32(Weight);
                            if (OldlRStock.BalQty < 0)
                            {
                                return "Error";
                            }
                            ErroMsg += "\n" + Qty + "  Qty Of  " + OldlRStock.LrNo + " LR Is Sucessfully Reverse...\n";
                            ctxTFAT.Entry(OldlRStock).State = EntityState.Modified;
                            ctxTFAT.Entry(LrVehicleLoadEntry).State = EntityState.Modified;

                            UnLoadDetails unLoadDetails = new UnLoadDetails();
                            bool AddEntry = true;
                            if (ctxTFAT.UnLoadDetails.Where(x => Child.Contains(x.Branch) && x.LRRefTablekey == LrVehicleLoadEntry.LRRefTablekey && x.LCRefTablekey == LrVehicleLoadEntry.LCRefTablekey && x.FMRefTablekey == LrVehicleLoadEntry.FMRefTablekey && x.VehicleNO == LrVehicleLoadEntry.StockAt).FirstOrDefault() != null)
                            {
                                unLoadDetails = ctxTFAT.UnLoadDetails.Where(x => Child.Contains(x.Branch) && x.LRRefTablekey == LrVehicleLoadEntry.LRRefTablekey && x.LCRefTablekey == LrVehicleLoadEntry.LCRefTablekey && x.FMRefTablekey == LrVehicleLoadEntry.FMRefTablekey && x.VehicleNO == LrVehicleLoadEntry.StockAt).FirstOrDefault();
                                AddEntry = false;
                            }
                            unLoadDetails.Branch = mbranchcode;
                            unLoadDetails.LrNo = LrVehicleLoadEntry.LrNo.Value;
                            unLoadDetails.LRRefTablekey = LrVehicleLoadEntry.LRRefTablekey;
                            unLoadDetails.LCNO = LrVehicleLoadEntry.LCNO.Value;
                            unLoadDetails.LCRefTablekey = LrVehicleLoadEntry.LCRefTablekey;
                            unLoadDetails.FMNO = LrVehicleLoadEntry.Fmno.Value;
                            unLoadDetails.FMRefTablekey = LrVehicleLoadEntry.FMRefTablekey;
                            unLoadDetails.VehicleNO = LrVehicleLoadEntry.StockAt.ToString();
                            unLoadDetails.Date = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                            unLoadDetails.Time = (DateTime.Now.ToString("HH:mm"));
                            unLoadDetails.GQty -= Qty;
                            unLoadDetails.Weight -= Weight;
                            unLoadDetails.ChrgWeight = LrVehicleLoadEntry.ChrgWeight;
                            unLoadDetails.ChrgType = LrVehicleLoadEntry.ChrgType;
                            unLoadDetails.Description = LrVehicleLoadEntry.Description == null ? "" : LrVehicleLoadEntry.Description;
                            unLoadDetails.Unit = LrVehicleLoadEntry.Unit;
                            unLoadDetails.FromBranch = LrVehicleLoadEntry.FromBranch;
                            unLoadDetails.ToBranch = LrVehicleLoadEntry.ToBranch;
                            unLoadDetails.Consigner = LrVehicleLoadEntry.Consigner;
                            unLoadDetails.Consignee = LrVehicleLoadEntry.Consignee;
                            unLoadDetails.LrType = LrVehicleLoadEntry.LrType;
                            unLoadDetails.Coln = LrVehicleLoadEntry.Coln;
                            unLoadDetails.Delivery = (LrVehicleLoadEntry.Delivery);
                            unLoadDetails.Remark = LrVehicleLoadEntry.Remark;
                            unLoadDetails.Prefix = mperiod;
                            unLoadDetails.ENTEREDBY = muserid;
                            unLoadDetails.AUTHIDS = muserid;
                            unLoadDetails.AUTHORISE = mauthorise;
                            unLoadDetails.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                            if (AddEntry)
                            {
                                ctxTFAT.UnLoadDetails.Add(unLoadDetails);
                            }
                            else
                            {
                                ctxTFAT.Entry(unLoadDetails).State = EntityState.Modified;
                            }
                        }
                        else
                        {
                            ErroMsg += "   Available Material Of This " + LrVehicleLoadEntry.LrNo + " Lr Is " + RStock.BalQty + " So U Cannot Reverse " + Qty + " Qty." + "\n,";
                            StockUpdateOrNot = false;
                        }
                    }
                    else
                    {
                        ErroMsg = "This " + OldlRStock.LrNo + " No Qty Has Consumed So We Cant Unload IT(Reverse). \n";
                    }
                }
                else
                {
                    ErroMsg = "Stock Not Found To Reverse Qty....!";
                }
            }
            else
            {

            }
            return ErroMsg;
        }

        public ActionResult DeleteUnloadLR(LoadingDispachVM mModel)
        {
            string Message = "", Status = "Success";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var Areas = GetChildGrp(mbranchcode);
                    string Parentkey = "";
                    List<LRModal> AlreadyUnloadLrList = TempData.Peek("AlreadyUnloadLrList") as List<LRModal>;
                    var lr = AlreadyUnloadLrList.Where(x => x.recordkey.ToString() == mModel.DeleteUnloadLR).FirstOrDefault();
                    if (lr != null)
                    {

                        FMROUTETable FMROUTETable = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
                        FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == FMROUTETable.Parentkey.ToString()).FirstOrDefault();
                        Parentkey = fMMaster.ParentKey;
                        LRStock OldlRStock;
                        LRStock VehicleEntryOfLr=new LRStock();
                        bool TemporaryData = false;
                        if (mModel.DeleteUnloadLR.Contains("TRN00"))
                        {
                            OldlRStock = ctxTFAT.LRStock.Where(x => Areas.Contains(x.Branch) && x.ParentKey == mModel.DeleteUnloadLR && x.Type == "LR").FirstOrDefault();
                            if (OldlRStock==null)
                            {
                                TemporaryData = true;
                            }
                            else
                            {
                                VehicleEntryOfLr = ctxTFAT.LRStock.Where(x => x.TableKey.ToString() == OldlRStock.ParentKey).FirstOrDefault();

                            }

                        }
                        else
                        {
                            OldlRStock = ctxTFAT.LRStock.Where(x => Areas.Contains(x.Branch) && x.TableKey == mModel.DeleteUnloadLR && x.Type == "LR").FirstOrDefault();
                            if (OldlRStock == null)
                            {
                                TemporaryData = true;
                            }
                            else
                            {
                                VehicleEntryOfLr = ctxTFAT.LRStock.Where(x => x.TableKey.ToString() == OldlRStock.ParentKey).FirstOrDefault();

                            }
                        }

                        if (TemporaryData == false)
                        {
                            UnLoadDetails unLoadDetails = ctxTFAT.UnLoadDetails.Where(x => Areas.Contains(x.Branch) && x.FMRefTablekey == fMMaster.TableKey && x.LRRefTablekey == VehicleEntryOfLr.LRRefTablekey && x.LCRefTablekey == VehicleEntryOfLr.LCRefTablekey).FirstOrDefault();

                            #region Set Exist Unload LR

                            lr.unloadDQty = 0;
                            lr.unloadGQty = 0;
                            lr.UnWeight = 0;
                            AlreadyUnloadLrList.Remove(lr);

                            TempData["AlreadyUnloadLrList"] = AlreadyUnloadLrList;

                            Message += lr.Lrno + "This Lr Delete Sucessfully...\n";

                            #endregion

                            #region Stock Reduce First From The Branch
                            if (VehicleEntryOfLr.UnloadGodwonQty > 0)
                            {
                                if (OldlRStock.AllocatBalQty >= Convert.ToInt32(VehicleEntryOfLr.UnloadGodwonQty))
                                {
                                    double Weight = Convert.ToDouble(((Convert.ToDecimal(VehicleEntryOfLr.UnloadGodwonQty)) / ((decimal)VehicleEntryOfLr.TotalQty) * ((decimal)VehicleEntryOfLr.ActWeight)));
                                    OldlRStock.TotalQty -= Convert.ToInt32(VehicleEntryOfLr.UnloadGodwonQty);
                                    OldlRStock.ActWeight -= Convert.ToInt32(Weight);
                                    OldlRStock.AllocatBalQty -= Convert.ToInt32(VehicleEntryOfLr.UnloadGodwonQty);
                                    OldlRStock.BalQty -= Convert.ToInt32(VehicleEntryOfLr.UnloadGodwonQty);
                                    OldlRStock.AllocatBalWght -= Convert.ToInt32(Weight);
                                    OldlRStock.BalWeight -= Convert.ToInt32(Weight);
                                    ctxTFAT.Entry(OldlRStock).State = EntityState.Modified;
                                    if (OldlRStock.TotalQty == 0)
                                    {
                                        ctxTFAT.LRStock.Remove(OldlRStock);
                                    }
                                    #region Stock Increase In Vehicle


                                    //VehicleEntryOfLr.AllocatBalQty += Convert.ToInt32(VehicleEntryOfLr.UnloadGodwonQty);
                                    VehicleEntryOfLr.BalQty += Convert.ToInt32(VehicleEntryOfLr.UnloadGodwonQty);
                                    //VehicleEntryOfLr.AllocatBalWght += Convert.ToInt32(Weight);
                                    VehicleEntryOfLr.BalWeight += Convert.ToInt32(Weight);
                                    VehicleEntryOfLr.UnloadGodwonQty = 0;
                                    VehicleEntryOfLr.UnloadDirectQty = 0;
                                    ctxTFAT.Entry(VehicleEntryOfLr).State = EntityState.Modified;

                                    #endregion

                                    #region Delete Unload Details Entry

                                    ctxTFAT.UnLoadDetails.Remove(unLoadDetails);

                                    #endregion

                                    #region Update UnloadLc 
                                    var UnloadLc = AlreadyUnloadLrList.Distinct().ToList();
                                    String UnLC = "", UnLCKey = "";
                                    foreach (var item in UnloadLc)
                                    {
                                        UnLC += item.Lcno + ",";
                                        LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey == item.recordkey).FirstOrDefault();
                                        UnLCKey += lRStock.LCRefTablekey + ",";
                                    }
                                    if (UnLC == "")
                                    {
                                        FMROUTETable.UnLoadLCNO = "";
                                        FMROUTETable.UNLODRefTablekey = "";
                                    }
                                    else
                                    {
                                        FMROUTETable.UnLoadLCNO = UnLC.Substring(0, UnLC.Length - 1);
                                        FMROUTETable.UNLODRefTablekey = UnLCKey.Substring(0, UnLCKey.Length - 1);
                                    }
                                    ctxTFAT.Entry(FMROUTETable).State = EntityState.Modified;

                                    #endregion
                                }
                                else
                                {
                                    Status = "Error";
                                    Message += VehicleEntryOfLr.LrNo + "This Lr Have Only " + OldlRStock.AllocatBalQty + " So Cant Delete....\n";
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            AlreadyUnloadLrList.Remove(lr);

                            TempData["AlreadyUnloadLrList"] = AlreadyUnloadLrList;
                            Message += lr.Lrno + "This Lr Delete Sucessfully...\n";
                        }
                    }
                    
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, "UnLoading Vehicle", Parentkey, DateTime.Now, 0, "", Message, "NA");

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
            return Json(new { Status = Status, id = "StateMaster", Message = Message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EmergencyUnloading(LoadingDispachVM mModel)
        {
            LoadingDispachVM loadingDispachVM = new LoadingDispachVM();
            List<LRModal> lRModals = new List<LRModal>();
            List<LCModal> lCModals = new List<LCModal>();

            #region Emergency Unloading

            var AllLcNoAreLoadedThisFm = ctxTFAT.LCMaster.Where(x => x.DispachFM.ToString() == mModel.Document.ToString()).Select(x => x.LCno).ToList();
            var CheckQtyIsThereOrNotForUnloading = ctxTFAT.LCDetail.Where(x => AllLcNoAreLoadedThisFm.Contains(x.LCno) && x.BalQty > 0).ToList();

            #region Now We Get LcDetails

            var GetLcnoWhosWasNotUnload = lRModals.Select(x => x.Lcno).ToList().Distinct();
            var GetPrperDetailsOfLC = ctxTFAT.LCMaster.Where(x => GetLcnoWhosWasNotUnload.Contains(x.LCno.ToString())).ToList();
            foreach (var lCMaster in GetPrperDetailsOfLC)
            {
                LCModal lCModal = new LCModal
                {
                    Date = lCMaster.Date.ToShortDateString(),
                    Time = lCMaster.Time,
                    lcno = lCMaster.LCno.ToString(),
                    TotalQty = lRModals.Where(x => x.Lcno == lCMaster.LCno.ToString()).Sum(x => x.Qty),
                    From = lCMaster.FromBranch,
                    From_Name = ctxTFAT.TfatBranch.Where(x => x.Code == lCMaster.FromBranch).Select(x => x.Name).FirstOrDefault(),
                    To = lCMaster.ToBranch,
                    To_Name = ctxTFAT.TfatBranch.Where(x => x.Code == lCMaster.ToBranch).Select(x => x.Name).FirstOrDefault(),
                    Weight = lRModals.Where(x => x.Lcno == lCMaster.LCno.ToString()).Sum(x => x.Weight),
                    LrListOfLC = lRModals.Where(x => x.Lcno == lCMaster.LCno.ToString()).ToList()
                };
                lCModals.Add(lCModal);
            }

            #endregion

            TempData["ExistLC"] = lCModals;
            loadingDispachVM.LClist = lCModals;
            loadingDispachVM.LRLIst = lRModals;
            TempData["ExistAllLR"] = lRModals;

            #endregion

            var html = ViewHelper.RenderPartialView(this, "UnloadingLCLRPartialView", loadingDispachVM);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Reverse(LoadingDispachVM mModel)
        {
            var Areas = GetChildGrp(mbranchcode);

            FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.FmNo.ToString() == mModel.Document.ToString()).FirstOrDefault();
            FMROUTETable FMROUTETable = ctxTFAT.FMROUTETable.Where(x => Areas.Contains(x.RouteVia) && x.FmNo == fMMaster.FmNo).FirstOrDefault();
            if (mModel.UpdateFmStatus)
            {
                List<LRModal> AlllRModals = TempData.Peek("ExistAllLR") as List<LRModal>;
                if (AlllRModals == null)
                {
                    AlllRModals = new List<LRModal>();
                }
                var UnloadLrCount = AlllRModals.Where(x => x.unloadDQty + x.unloadGQty > 0).ToList().Count;
                if (UnloadLrCount > 0)
                {
                    return Json(new { Message = "Not Allow To Reverse.\n", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (Areas.Contains(fMMaster.Branch))
                    {
                        return Json(new { Message = "Not Allow To Reverse.\n Because Of This Fm Created By Our Branch", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        fMMaster.FmStatus = "A";
                        FMROUTETable.ArrivalDate = null;
                        FMROUTETable.ArrivalKM = null;
                        FMROUTETable.ArrivalRemark = null;
                        FMROUTETable.ArrivalTime = null;
                        ctxTFAT.Entry(fMMaster).State = EntityState.Modified;
                        ctxTFAT.SaveChanges();
                    }
                }
            }

            return Json(new { Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetInCurrentBranch()
        {
            string BranchCode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
            var Areas = GetChildGrp(mbranchcode);
            List<ArrivalDispatchVM> List = TempData.Peek("UnLoadingList") as List<ArrivalDispatchVM>;
            List<LCModal> lCModals = new List<LCModal>();
            List<LRModal> AllLRlist = new List<LRModal>();
            if (List == null)
            {
                List = new List<ArrivalDispatchVM>();
                var GetLoadingList = ctxTFAT.FMROUTETable.Where(x => Areas.Contains(x.RouteVia)).ToList();
                foreach (var item in GetLoadingList)
                {
                    bool AddFlg = false;

                    FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.FmNo == item.FmNo).FirstOrDefault();
                    ArrivalDispatchVM arrivalDispatchVM = new ArrivalDispatchVM();
                    arrivalDispatchVM.Fmno = item.FmNo.ToString();
                    arrivalDispatchVM.VehicleNo = fMMaster.TruckNo.ToString();
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
                    if (!String.IsNullOrEmpty(fMMaster.LCno))
                    {
                        var TransitListOfCurrentBranch = ctxTFAT.LRStock.Where(x => Areas.Contains(x.Branch) && x.StockStatus == "T" && x.TotalQty != x.BalQty).Select(x => x.LCNO).ToList().Distinct();
                        var GetLCNo = fMMaster.LCno.Split(',');
                        foreach (var lcmaster in TransitListOfCurrentBranch)
                        {
                            AddFlg = true;
                            List<LRModal> lRModals = new List<LRModal>();
                            var GetLCDetail = ctxTFAT.LCMaster.Where(x => x.LCno.ToString() == lcmaster.ToString()).FirstOrDefault();
                            LCModal lCModal = new LCModal
                            {
                                Date = GetLCDetail.Date.ToShortDateString(),
                                Time = GetLCDetail.Time,
                                lcno = lcmaster.ToString(),
                                TotalQty = 0,
                                From = GetLCDetail.FromBranch,
                                From_Name = ctxTFAT.TfatBranch.Where(x => x.Code == GetLCDetail.FromBranch).Select(x => x.Name).FirstOrDefault(),
                                To = GetLCDetail.ToBranch,
                                To_Name = ctxTFAT.TfatBranch.Where(x => x.Code == GetLCDetail.ToBranch).Select(x => x.Name).FirstOrDefault(),
                            };
                            var GetCurrentLrListOfLC = ctxTFAT.LRStock.Where(x => Areas.Contains(x.Branch) && x.LCNO.ToString() == lcmaster.ToString() && x.StockStatus == "T").ToList();
                            foreach (var item1 in GetCurrentLrListOfLC)
                            {
                                LRModal lRModal = new LRModal
                                {
                                    Lcno = lcmaster.ToString(),
                                    Date = item1.Date.ToShortDateString(),
                                    Time = item1.Time,
                                    Lrno = item1.LrNo.Value,
                                    Qty = item1.AllocatBalQty + Convert.ToInt32(item1.UnloadDirectQty) + Convert.ToInt32(item1.UnloadGodwonQty),
                                    From = item1.FromBranch,
                                    From_Name = ctxTFAT.TfatBranch.Where(x => x.Code == item1.FromBranch).Select(x => x.Name).FirstOrDefault(),
                                    To = item1.ToBranch,
                                    To_Name = ctxTFAT.TfatBranch.Where(x => x.Code == item1.ToBranch).Select(x => x.Name).FirstOrDefault(),
                                    Weight = item1.ChrgWeight,
                                    ChargeType = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == item1.ChrgType).Select(x => x.ChargeType).FirstOrDefault(),
                                    PorductType = ctxTFAT.UnitMaster.Where(x => x.Code == item1.Unit).Select(x => x.Name).FirstOrDefault(),
                                    unloadGQty = Convert.ToInt32(item1.UnloadGodwonQty),
                                    unloadDQty = Convert.ToInt32(item1.UnloadDirectQty),
                                    AllowQty = item1.AllocatBalQty
                                };
                                lRModals.Add(lRModal);
                            }

                            if (GetCurrentLrListOfLC.Count() != 0)
                            {

                                lCModal.Weight = lRModals.Select(x => x.Weight).ToList().Sum();
                                lCModal.TotalQty = lRModals.Select(x => x.Qty).ToList().Sum();
                                lCModal.LrListOfLC = lRModals;
                                arrivalDispatchVM.NoofLr = lRModals.Count.ToString();
                                arrivalDispatchVM.TotalWeight = lRModals.Sum(x => x.Weight).ToString();
                                arrivalDispatchVM.TotalQty = lRModals.Sum(x => x.Qty).ToString();
                                AllLRlist.AddRange(lRModals);
                                arrivalDispatchVM.LCno = fMMaster.LCno;
                                lCModals.Add(lCModal);
                            }

                        }
                    }
                    //arrivalDispatchVM.lCModals = lCModals;
                    if (AddFlg)
                    {
                        List.Add(arrivalDispatchVM);
                    }

                }
            }


            TempData["UnLoadingList"] = List;
            TempData["PendingLC"] = lCModals;
            TempData["AllLrList"] = AllLRlist;

            var html = ViewHelper.RenderPartialView(this, "_UnLoadingList", List);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUnLoadingDetails(string Lcno)
        {
            ArrivalDispatchVM mModel = new ArrivalDispatchVM();
            List<LCModal> lCModalsList = TempData.Peek("PendingLC") as List<LCModal>;
            List<LRModal> Lrlist = TempData.Peek("AllLrList") as List<LRModal>;
            var lcSplitByComma = Lcno.Split(',');
            List<LCModal> newLclist = new List<LCModal>();
            List<LRModal> newLrlist = new List<LRModal>();
            foreach (var item in lcSplitByComma)
            {
                if (lCModalsList.Where(x => x.lcno == item).FirstOrDefault() != null)
                {
                    newLclist.Add(lCModalsList.Where(x => x.lcno == item).FirstOrDefault());
                    //var lrlist = lCModalsList.Where(x => x.lcno == item).FirstOrDefault();
                    newLrlist.AddRange(lCModalsList.Where(x => x.lcno == item).Select(x => x.LrListOfLC).FirstOrDefault());
                }
            }

            mModel.lCModals = newLclist;
            mModel.lRMOdals = newLrlist;
            var html = ViewHelper.RenderPartialView(this, "_GetUnloadingDetailsOfLC", mModel);
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

        public ActionResult OTHSaveData(LoadingDispachVM mModel)
        {
            string Message = "", Parentkey = "", UNLR = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    FMROUTETable fM_ROUTE = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
                    if (fM_ROUTE != null)
                    {
                        FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == fM_ROUTE.Parentkey.ToString()).FirstOrDefault();

                        if (fMMaster != null)
                        {
                            Parentkey = fMMaster.ParentKey;
                            if (mModel.LRLIst != null)
                            {
                                if (mModel.LRLIst.Count() > 0)
                                {

                                    var RecordkeyOfLrForUnload = mModel.LRLIst.Select(x => x.recordkey).ToList();
                                    var LR_Wise_VehicleMaterial = ctxTFAT.LRStock.Where(x => RecordkeyOfLrForUnload.Contains(x.TableKey)).ToList();
                                    int Qty = 0;
                                    double Weight = 0;
                                    for (int i = 0; i < LR_Wise_VehicleMaterial.Count; i++)
                                    {

                                        LRStock VehicleMaterial = LR_Wise_VehicleMaterial[i];
                                        LRStock OldlRStock;
                                        LRMaster lRMaster;
                                        var CurrentDataForUnload = mModel.LRLIst.Where(x => x.recordkey == VehicleMaterial.TableKey).FirstOrDefault();
                                        var Child = mModel.LRLIst.Where(x => x.recordkey == VehicleMaterial.TableKey).Select(x => x.OTHBranch).FirstOrDefault();
                                        int unlG = 0, unlD = 0;
                                        if (VehicleMaterial.TableKey.Contains("TRN00"))
                                        {
                                            VehicleMaterial = ctxTFAT.LRStock.Where(x => x.TableKey == VehicleMaterial.TableKey).FirstOrDefault();
                                            OldlRStock = ctxTFAT.LRStock.Where(x => Child.Contains(x.Branch) && x.ParentKey == VehicleMaterial.TableKey && x.Type == "LR").FirstOrDefault();
                                            lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey == VehicleMaterial.LRRefTablekey).FirstOrDefault();
                                        }
                                        else
                                        {
                                            VehicleMaterial = ctxTFAT.LRStock.Where(x => x.TableKey == VehicleMaterial.ParentKey).FirstOrDefault();
                                            OldlRStock = ctxTFAT.LRStock.Where(x => Child.Contains(x.Branch) && x.ParentKey == VehicleMaterial.TableKey && x.Type == "LR").FirstOrDefault();
                                            lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey == OldlRStock.LRRefTablekey).FirstOrDefault();
                                        }
                                        if (ctxTFAT.UnLoadDetails.Where(x => Child.Contains(x.Branch) && x.LRRefTablekey == VehicleMaterial.LRRefTablekey && x.LCRefTablekey == VehicleMaterial.LCRefTablekey && x.FMRefTablekey == fMMaster.TableKey && x.VehicleNO == fMMaster.TruckNo).FirstOrDefault() != null)
                                        {
                                            UnLoadDetails unLoad = ctxTFAT.UnLoadDetails.Where(x => Child.Contains(x.Branch) && x.LRRefTablekey == VehicleMaterial.LRRefTablekey && x.LCRefTablekey == VehicleMaterial.LCRefTablekey && x.FMRefTablekey == fMMaster.TableKey && x.VehicleNO == fMMaster.TruckNo).FirstOrDefault();
                                            unlG = unLoad.GQty;
                                            unlD = unLoad.DQty;
                                        }


                                        #region Godown Qty
                                        if (unlG > 0)//Unload It Already Some Material
                                        {
                                            if (unlG != CurrentDataForUnload.unloadGQty)
                                            {
                                                if (unlG >= CurrentDataForUnload.unloadGQty)//Reverse Qty
                                                {
                                                    Qty = Convert.ToInt32(unlG) - CurrentDataForUnload.unloadGQty;
                                                    Weight = Convert.ToDouble(((Convert.ToDecimal(Qty)) / ((decimal)VehicleMaterial.TotalQty) * ((decimal)VehicleMaterial.ActWeight)));
                                                    string Msg = OTHUnloadReverse(Qty, Weight, VehicleMaterial, OldlRStock, "Godown", fM_ROUTE.LCNO);
                                                    if (!string.IsNullOrEmpty(Msg))
                                                    {
                                                        if (Msg == "Error")
                                                        {
                                                            Message = "Not Allowed To Unload " + VehicleMaterial.LrNo + " Consignment Due To Negative Stock...!";
                                                            return Json(new { Message = Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                                                        }
                                                    }
                                                    Message += Msg;
                                                }
                                                else//Unload Qty 
                                                {
                                                    Qty = CurrentDataForUnload.unloadGQty - Convert.ToInt32(unlG);
                                                    Weight = Convert.ToDouble(((Convert.ToDecimal(Qty)) / ((decimal)VehicleMaterial.TotalQty) * ((decimal)VehicleMaterial.ActWeight)));
                                                    string Msg = OTHUnload(Qty, Weight, mbranchcode, VehicleMaterial, OldlRStock, "Godown", fM_ROUTE.LCNO, Child, fM_ROUTE.RECORDKEY.ToString(), fM_ROUTE);
                                                    if (!string.IsNullOrEmpty(Msg))
                                                    {
                                                        if (Msg == "Error")
                                                        {
                                                            Message = "Not Allowed To Unload " + VehicleMaterial.LrNo + " Consignment Due To Negative Stock...!";
                                                            return Json(new { Message = Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                                                        }
                                                    }
                                                    Message += Msg;
                                                }
                                            }
                                        }
                                        else//First Time Unload
                                        {
                                            if (CurrentDataForUnload.unloadGQty > 0)
                                            {
                                                Qty = CurrentDataForUnload.unloadGQty - Convert.ToInt32(unlG);
                                                Weight = Convert.ToDouble(((Convert.ToDecimal(Qty)) / ((decimal)VehicleMaterial.TotalQty) * ((decimal)VehicleMaterial.ActWeight)));
                                                string Msg = OTHUnload(Qty, Weight, mbranchcode, VehicleMaterial, OldlRStock, "Godown", fM_ROUTE.LCNO, Child, fM_ROUTE.RECORDKEY.ToString(), fM_ROUTE);
                                                if (!string.IsNullOrEmpty(Msg))
                                                {
                                                    if (Msg == "Error")
                                                    {
                                                        Message = "Not Allowed To Unload " + VehicleMaterial.LrNo + " Consignment Due To Negative Stock...!";
                                                        return Json(new { Message = Message, Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                                                    }
                                                }
                                                Message += Msg;
                                            }
                                        }
                                        #endregion
                                    }
                                }
                            }

                            #region Update UnloadLc In Fmroute
                            List<LRModal> AlreadyUnloadLrList = TempData.Peek("AlreadyUnloadLrList") as List<LRModal>;
                            if (AlreadyUnloadLrList == null)
                            {
                                AlreadyUnloadLrList = new List<LRModal>();
                            }

                            List<LRModal> OTHAlreadyUnloadLrList = TempData.Peek("OTHAlreadyUnloadLrList") as List<LRModal>;
                            if (OTHAlreadyUnloadLrList == null)
                            {
                                OTHAlreadyUnloadLrList = new List<LRModal>();
                            }
                            string UnloadLC = "", UnloadLCKey = "";
                            foreach (var item in AlreadyUnloadLrList.Distinct().ToList())
                            {
                                UnloadLC += item.Lcno + ",";
                                //LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey == item.recordkey).FirstOrDefault();
                                //UnloadLCKey += lRStock.LCRefTablekey + ",";
                            }
                            foreach (var item in OTHAlreadyUnloadLrList.Distinct().ToList())
                            {
                                UnloadLC += item.Lcno + ",";
                                //LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey == item.recordkey).FirstOrDefault();
                                //UnloadLCKey += lRStock.LCRefTablekey + ",";
                            }

                            if (String.IsNullOrEmpty(UnloadLC))
                            {
                                fM_ROUTE.UnLoadLCNO = "";
                                //fM_ROUTE.UNLODRefTablekey = "";
                            }
                            else
                            {
                                fM_ROUTE.UnLoadLCNO = UnloadLC.Substring(0, UnloadLC.Length - 1);
                                //fM_ROUTE.UNLODRefTablekey = UnloadLCKey.Substring(0, UnloadLCKey.Length - 1);
                            }
                            ctxTFAT.Entry(fM_ROUTE).State = EntityState.Modified;

                            #endregion
                        }
                        else
                        {
                            return Json(new { Message = "Error,FM Not Fount..\n DD", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { Message = "Error,Route Not Fount..\n", Status = "Error", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
                    }
                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, "Other Warehouse UnLoading Vehicle", Parentkey, DateTime.Now, 0, "", Message, "NA");

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

        public string OTHUnload(int Qty, double Weight, string BranchCode, LRStock LrVehicleLoadEntry, LRStock OldlRStock, string Type, string CurrRouteLoadLcNO, string Child, string fM_ROUTEKey,FMROUTETable fMROUTE)
        {
            bool StockUpdateOrNot = true;
            string ErroMsg = "";
            if (Type == "Godown")
            {
                if (OldlRStock == null)
                {
                    LRStock newlRStock = new LRStock();
                    newlRStock.LoginBranch = mbranchcode;
                    newlRStock.Branch = Child;
                    newlRStock.LrNo = LrVehicleLoadEntry.LrNo;
                    newlRStock.LCNO = LrVehicleLoadEntry.LCNO;
                    newlRStock.Fmno = LrVehicleLoadEntry.Fmno;
                    newlRStock.Date = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString().Trim());
                    newlRStock.Time = DateTime.Now.ToString("HH:mm");
                    newlRStock.TotalQty = Convert.ToInt32(Qty);
                    newlRStock.AllocatBalQty = Convert.ToInt32(Qty);
                    newlRStock.AllocatBalWght = Weight;
                    newlRStock.BalQty = Convert.ToInt32(Qty);
                    newlRStock.ChrgWeight = LrVehicleLoadEntry.ChrgWeight;
                    newlRStock.ActWeight = Weight;
                    newlRStock.BalWeight = Weight;
                    newlRStock.ChrgType = LrVehicleLoadEntry.ChrgType;
                    newlRStock.Description = LrVehicleLoadEntry.Description == null ? "" : LrVehicleLoadEntry.Description;
                    newlRStock.Unit = LrVehicleLoadEntry.Unit;
                    newlRStock.FromBranch = LrVehicleLoadEntry.FromBranch;
                    newlRStock.ToBranch = LrVehicleLoadEntry.ToBranch;
                    newlRStock.Consigner = LrVehicleLoadEntry.Consigner;
                    newlRStock.Consignee = LrVehicleLoadEntry.Consignee;
                    newlRStock.LrType = LrVehicleLoadEntry.LrType;
                    newlRStock.Coln = LrVehicleLoadEntry.Coln;
                    newlRStock.Delivery = LrVehicleLoadEntry.Delivery;
                    newlRStock.LRMode = LrVehicleLoadEntry.LRMode;
                    newlRStock.Remark = null;
                    newlRStock.Type = "LR";
                    newlRStock.ENTEREDBY = muserid;
                    newlRStock.AUTHIDS = muserid;
                    newlRStock.AUTHORISE = mauthorise;
                    newlRStock.Prefix = mperiod;
                    newlRStock.LRRefTablekey = LrVehicleLoadEntry.LRRefTablekey;
                    newlRStock.LCRefTablekey = LrVehicleLoadEntry.LCRefTablekey;
                    newlRStock.FMRefTablekey = LrVehicleLoadEntry.FMRefTablekey;
                    newlRStock.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                    if (LrVehicleLoadEntry.Coln == "G")
                    {
                        newlRStock.StockAt = "Godown";
                        newlRStock.StockStatus = "G";
                    }
                    else
                    {
                        newlRStock.StockAt = "Pick";
                        newlRStock.StockStatus = "P";
                    }
                    var PreviousLrTableKey = ctxTFAT.LRStock.Where(x => x.LRRefTablekey == LrVehicleLoadEntry.LRRefTablekey && x.Type == "LR").OrderByDescending(x => x.RECORDKEY).Select(x => x.TableKey).FirstOrDefault();
                    int Sno = 1;
                    if (!String.IsNullOrEmpty(PreviousLrTableKey))
                    {
                        var CheckBranchCode = PreviousLrTableKey.Substring(0, 6);
                        if (ctxTFAT.TfatBranch.Where(x => x.Code == CheckBranchCode).FirstOrDefault() == null)
                        {
                            Sno = Convert.ToInt32(PreviousLrTableKey.Substring(7, 3)) + 1;
                        }
                        else
                        {
                            Sno = Convert.ToInt32(PreviousLrTableKey.Substring(13, 3)) + 1;
                        }
                    }

                    newlRStock.TableKey = mbranchcode + "STK00" + mperiod.Substring(0, 2) + Sno.ToString("D3") + LrVehicleLoadEntry.LrNo;
                    newlRStock.ParentKey = LrVehicleLoadEntry.TableKey;
                    var RStock = ctxTFAT.LRStock.Where(x => Child.Contains(x.Branch) && x.ParentKey == LrVehicleLoadEntry.TableKey && x.Type == "LR").FirstOrDefault();
                    if (RStock == null)
                    {
                        LrVehicleLoadEntry.BalQty -= Qty;
                        LrVehicleLoadEntry.BalWeight -= Weight;
                        ctxTFAT.Entry(LrVehicleLoadEntry).State = EntityState.Modified;
                        if (LrVehicleLoadEntry.BalQty < 0)
                        {
                            return "Error";
                        }
                        ctxTFAT.LRStock.Add(newlRStock);
                        if (LrVehicleLoadEntry.Branch != mbranchcode)
                        {
                            UnloadOtherBranchMaterialNotification(fMROUTE, LrVehicleLoadEntry.LRRefTablekey);
                        }
                        ErroMsg += "\n" + Qty + "  Qty Of  " + newlRStock.LrNo + " Consignment Is Sucessfully Unloded.\n";
                    }
                    else
                    {
                        ErroMsg += LrVehicleLoadEntry.LrNo + "This Consignment Of Some Material Already Unloded So PLease Refresh Your Page" + "\n,";
                        StockUpdateOrNot = false;
                    }
                }
                else
                {


                    var RStock = ctxTFAT.LRStock.Where(x => x.TableKey == LrVehicleLoadEntry.TableKey).FirstOrDefault();
                    if (RStock.BalQty >= Qty)
                    {
                        OldlRStock.TotalQty += Convert.ToInt32(Qty);
                        OldlRStock.ActWeight += Weight;
                        OldlRStock.AllocatBalQty += Convert.ToInt32(Qty);
                        OldlRStock.AllocatBalWght += Weight;
                        OldlRStock.BalQty += Convert.ToInt32(Qty);
                        OldlRStock.BalWeight += Weight;

                        LrVehicleLoadEntry.BalQty -= Qty;
                        LrVehicleLoadEntry.BalWeight -= Weight;
                        ctxTFAT.Entry(LrVehicleLoadEntry).State = EntityState.Modified;
                        if (LrVehicleLoadEntry.BalQty < 0)
                        {
                            return "Error";
                        }
                        if (LrVehicleLoadEntry.Branch != mbranchcode)
                        {
                            UnloadOtherBranchMaterialNotification(fMROUTE, LrVehicleLoadEntry.LRRefTablekey);
                        }
                        ErroMsg += "\n" + Qty + "  Qty Of  " + OldlRStock.LrNo + " LR Is Sucessfully Unloded.\n";
                        ctxTFAT.Entry(OldlRStock).State = EntityState.Modified;
                    }
                    else
                    {
                        ErroMsg += "   Available Material Of This " + LrVehicleLoadEntry.LrNo + " Lr Is " + RStock.BalQty + "" + "\n,";
                        StockUpdateOrNot = false;
                    }
                }
            }

            if (StockUpdateOrNot)
            {
                //LrVehicleLoadEntry.AllocatBalQty -= Convert.ToInt32(Qty);
                //LrVehicleLoadEntry.BalQty -= Convert.ToInt32(Qty);
                //LrVehicleLoadEntry.AllocatBalWght -= Convert.ToInt32(Weight);
                //LrVehicleLoadEntry.BalWeight -= Convert.ToInt32(Weight);
                if (Type == "Godown")
                {
                    LrVehicleLoadEntry.UnloadGodwonQty += Convert.ToInt32(Qty);
                }
                else
                {
                    LrVehicleLoadEntry.UnloadDirectQty += Convert.ToInt32(Qty);
                }
                ctxTFAT.Entry(LrVehicleLoadEntry).State = EntityState.Modified;

                UnLoadDetails unLoadDetails = new UnLoadDetails();
                bool AddEntry = true;
                if (ctxTFAT.UnLoadDetails.Where(x => Child.Contains(x.Branch) && x.LRRefTablekey == LrVehicleLoadEntry.LRRefTablekey && x.LCRefTablekey == LrVehicleLoadEntry.LCRefTablekey && x.FMRefTablekey == LrVehicleLoadEntry.FMRefTablekey && x.VehicleNO == LrVehicleLoadEntry.StockAt).FirstOrDefault() != null)
                {
                    unLoadDetails = ctxTFAT.UnLoadDetails.Where(x => Child.Contains(x.Branch) && x.LRRefTablekey == LrVehicleLoadEntry.LRRefTablekey && x.LCRefTablekey == LrVehicleLoadEntry.LCRefTablekey && x.FMRefTablekey == LrVehicleLoadEntry.FMRefTablekey && x.VehicleNO == LrVehicleLoadEntry.StockAt).FirstOrDefault();
                    AddEntry = false;
                }
                unLoadDetails.Branch = mbranchcode;
                unLoadDetails.LrNo = LrVehicleLoadEntry.LrNo.Value;
                unLoadDetails.LRRefTablekey = LrVehicleLoadEntry.LRRefTablekey;
                unLoadDetails.LCNO = LrVehicleLoadEntry.LCNO.Value;
                unLoadDetails.LCRefTablekey = LrVehicleLoadEntry.LCRefTablekey;
                unLoadDetails.FMNO = LrVehicleLoadEntry.Fmno.Value;
                unLoadDetails.FMRefTablekey = LrVehicleLoadEntry.FMRefTablekey;
                unLoadDetails.VehicleNO = LrVehicleLoadEntry.StockAt.ToString();
                unLoadDetails.Date = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                unLoadDetails.Time = (DateTime.Now.ToString("HH:mm"));
                unLoadDetails.GQty += Qty;
                unLoadDetails.Weight += Weight;
                unLoadDetails.ChrgWeight = LrVehicleLoadEntry.ChrgWeight;
                unLoadDetails.ChrgType = LrVehicleLoadEntry.ChrgType;
                unLoadDetails.Description = LrVehicleLoadEntry.Description == null ? "" : LrVehicleLoadEntry.Description;
                unLoadDetails.Unit = LrVehicleLoadEntry.Unit;
                unLoadDetails.FromBranch = LrVehicleLoadEntry.FromBranch;
                unLoadDetails.ToBranch = LrVehicleLoadEntry.ToBranch;
                unLoadDetails.Consigner = LrVehicleLoadEntry.Consigner;
                unLoadDetails.Consignee = LrVehicleLoadEntry.Consignee;
                unLoadDetails.LrType = LrVehicleLoadEntry.LrType;
                unLoadDetails.Coln = LrVehicleLoadEntry.Coln;
                unLoadDetails.Delivery = (LrVehicleLoadEntry.Delivery);
                unLoadDetails.Remark = LrVehicleLoadEntry.Remark;
                unLoadDetails.ENTEREDBY = muserid;
                unLoadDetails.AUTHIDS = muserid;
                unLoadDetails.AUTHORISE = mauthorise;
                unLoadDetails.Prefix = mperiod;
                unLoadDetails.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());

                if (AddEntry)
                {
                    ctxTFAT.UnLoadDetails.Add(unLoadDetails);
                }
                else
                {
                    ctxTFAT.Entry(unLoadDetails).State = EntityState.Modified;
                }
            }

            return ErroMsg;

        }

        public ActionResult OTHDeleteUnloadLR(LoadingDispachVM mModel)
        {
            string Message = "", Status = "Success";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    var Areas = GetChildGrp(mbranchcode);
                    string Parentkey = "";
                    List<LRModal> AlreadyUnloadLrList = TempData.Peek("OTHAlreadyUnloadLrList") as List<LRModal>;
                    var lr = AlreadyUnloadLrList.Where(x => x.recordkey.ToString() == mModel.DeleteUnloadLR).FirstOrDefault();
                    if (lr != null)
                    {

                        FMROUTETable FMROUTETable = ctxTFAT.FMROUTETable.Where(x => x.RECORDKEY.ToString() == mModel.Document).FirstOrDefault();
                        FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == FMROUTETable.Parentkey.ToString()).FirstOrDefault();
                        Parentkey = fMMaster.ParentKey;
                        LRStock OldlRStock;
                        LRStock VehicleEntryOfLr;
                        if (mModel.DeleteUnloadLR.Contains("TRN00"))
                        {
                            OldlRStock = ctxTFAT.LRStock.Where(x => x.ParentKey == mModel.DeleteUnloadLR && x.Type == "LR").FirstOrDefault();
                            VehicleEntryOfLr = ctxTFAT.LRStock.Where(x => x.TableKey.ToString() == OldlRStock.ParentKey).FirstOrDefault();
                        }
                        else
                        {
                            OldlRStock = ctxTFAT.LRStock.Where(x => x.TableKey == mModel.DeleteUnloadLR && x.Type == "LR").FirstOrDefault();
                            VehicleEntryOfLr = ctxTFAT.LRStock.Where(x => x.TableKey.ToString() == OldlRStock.ParentKey).FirstOrDefault();
                        }
                        UnLoadDetails unLoadDetails = ctxTFAT.UnLoadDetails.Where(x => Areas.Contains(x.Branch) && x.FMRefTablekey == fMMaster.TableKey && x.LRRefTablekey == VehicleEntryOfLr.LRRefTablekey && x.LCRefTablekey == VehicleEntryOfLr.LCRefTablekey).FirstOrDefault();

                        #region Set Exist Unload LR

                        lr.unloadDQty = 0;
                        lr.unloadGQty = 0;
                        lr.UnWeight = 0;
                        AlreadyUnloadLrList.Remove(lr);

                        TempData["OTHAlreadyUnloadLrList"] = AlreadyUnloadLrList;

                        Message += VehicleEntryOfLr.LrNo + "This Lr Delete Sucessfully...\n";

                        #endregion

                        #region Stock Reduce First From The Branch
                        if (VehicleEntryOfLr.UnloadGodwonQty > 0)
                        {
                            if (OldlRStock.AllocatBalQty >= Convert.ToInt32(VehicleEntryOfLr.UnloadGodwonQty))
                            {
                                double Weight = Convert.ToDouble(((Convert.ToDecimal(VehicleEntryOfLr.UnloadGodwonQty)) / ((decimal)VehicleEntryOfLr.TotalQty) * ((decimal)VehicleEntryOfLr.ActWeight)));
                                OldlRStock.TotalQty -= Convert.ToInt32(VehicleEntryOfLr.UnloadGodwonQty);
                                OldlRStock.ActWeight -= Convert.ToInt32(Weight);
                                OldlRStock.AllocatBalQty -= Convert.ToInt32(VehicleEntryOfLr.UnloadGodwonQty);
                                OldlRStock.BalQty -= Convert.ToInt32(VehicleEntryOfLr.UnloadGodwonQty);
                                OldlRStock.AllocatBalWght -= Convert.ToInt32(Weight);
                                OldlRStock.BalWeight -= Convert.ToInt32(Weight);
                                ctxTFAT.Entry(OldlRStock).State = EntityState.Modified;
                                if (OldlRStock.TotalQty == 0)
                                {
                                    ctxTFAT.LRStock.Remove(OldlRStock);
                                }
                                #region Stock Increase In Vehicle


                                //VehicleEntryOfLr.AllocatBalQty += Convert.ToInt32(VehicleEntryOfLr.UnloadGodwonQty);
                                VehicleEntryOfLr.BalQty += Convert.ToInt32(VehicleEntryOfLr.UnloadGodwonQty);
                                //VehicleEntryOfLr.AllocatBalWght += Convert.ToInt32(Weight);
                                VehicleEntryOfLr.BalWeight += Convert.ToInt32(Weight);
                                VehicleEntryOfLr.UnloadGodwonQty = 0;
                                VehicleEntryOfLr.UnloadDirectQty = 0;
                                ctxTFAT.Entry(VehicleEntryOfLr).State = EntityState.Modified;

                                #endregion

                                #region Delete Unload Details Entry

                                ctxTFAT.UnLoadDetails.Remove(unLoadDetails);

                                #endregion

                                #region Update UnloadLc 
                                var UnloadLc = AlreadyUnloadLrList.Distinct().ToList();
                                String UnLC = "", UnLCKey = "";
                                foreach (var item in UnloadLc)
                                {
                                    UnLC += item.Lcno + ",";
                                    LRStock lRStock = ctxTFAT.LRStock.Where(x => x.TableKey == item.recordkey).FirstOrDefault();
                                    UnLCKey += lRStock.LCRefTablekey + ",";
                                }
                                if (UnLC == "")
                                {
                                    FMROUTETable.UnLoadLCNO = "";
                                    //FMROUTETable.UNLODRefTablekey = "";
                                }
                                else
                                {
                                    FMROUTETable.UnLoadLCNO = UnLC.Substring(0, UnLC.Length - 1);
                                    //FMROUTETable.UNLODRefTablekey = UnLCKey.Substring(0, UnLCKey.Length - 1);
                                }
                                ctxTFAT.Entry(FMROUTETable).State = EntityState.Modified;

                                #endregion
                            }
                            else
                            {
                                Status = "Error";
                                Message += VehicleEntryOfLr.LrNo + "This Lr Have Only " + OldlRStock.AllocatBalQty + " So Cant Delete....\n";
                            }
                        }
                        #endregion


                    }

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, "UnLoading Vehicle", Parentkey, DateTime.Now, 0, "", Message, "NA");

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
            return Json(new { Status = Status, id = "StateMaster", Message = Message }, JsonRequestBehavior.AllowGet);
        }
        public string OTHUnloadReverse(int Qty, double Weight, LRStock LrVehicleLoadEntry, LRStock OldlRStock, string Type, string CurrRouteLoadLcNO)
        {
            bool StockUpdateOrNot = true;
            string ErroMsg = "";
            var Child = GetChildGrp(mbranchcode);
            if (Type == "Godown")
            {
                if (OldlRStock != null)
                {
                    if (OldlRStock.AllocatBalQty >= Convert.ToInt32(Qty))
                    {
                        //LrVehicleLoadEntry.AllocatBalQty += Convert.ToInt32(Qty);
                        //LrVehicleLoadEntry.BalQty += Convert.ToInt32(Qty);
                        //LrVehicleLoadEntry.AllocatBalWght += Convert.ToInt32(Weight);
                        //LrVehicleLoadEntry.BalWeight += Convert.ToInt32(Weight);
                        if (Type == "Godown")
                        {
                            LrVehicleLoadEntry.BalQty += Convert.ToInt32(Qty);
                            LrVehicleLoadEntry.BalWeight += Weight;
                            LrVehicleLoadEntry.UnloadGodwonQty -= Convert.ToInt32(Qty);
                        }
                        else
                        {
                            LrVehicleLoadEntry.UnloadDirectQty -= Convert.ToInt32(Qty);
                        }
                        var RStock = ctxTFAT.LRStock.Where(x => x.TableKey == OldlRStock.TableKey).FirstOrDefault();
                        if (RStock.BalQty >= Qty)
                        {
                            OldlRStock.TotalQty -= Convert.ToInt32(Qty);
                            OldlRStock.ActWeight -= Convert.ToInt32(Weight);
                            OldlRStock.AllocatBalQty -= Convert.ToInt32(Qty);
                            OldlRStock.BalQty -= Convert.ToInt32(Qty);
                            OldlRStock.AllocatBalWght -= Convert.ToInt32(Weight);
                            OldlRStock.BalWeight -= Convert.ToInt32(Weight);
                            if (OldlRStock.BalQty < 0)
                            {
                                return "Error";
                            }
                            ErroMsg += "\n" + Qty + "  Qty Of  " + OldlRStock.LrNo + " LR Is Sucessfully Reverse...\n";
                            ctxTFAT.Entry(OldlRStock).State = EntityState.Modified;
                            ctxTFAT.Entry(LrVehicleLoadEntry).State = EntityState.Modified;

                            UnLoadDetails unLoadDetails = new UnLoadDetails();
                            bool AddEntry = true;
                            if (ctxTFAT.UnLoadDetails.Where(x => Child.Contains(x.Branch) && x.LRRefTablekey == LrVehicleLoadEntry.LRRefTablekey && x.LCRefTablekey == LrVehicleLoadEntry.LCRefTablekey && x.FMRefTablekey == LrVehicleLoadEntry.FMRefTablekey && x.VehicleNO == LrVehicleLoadEntry.StockAt).FirstOrDefault() != null)
                            {
                                unLoadDetails = ctxTFAT.UnLoadDetails.Where(x => Child.Contains(x.Branch) && x.LRRefTablekey == LrVehicleLoadEntry.LRRefTablekey && x.LCRefTablekey == LrVehicleLoadEntry.LCRefTablekey && x.FMRefTablekey == LrVehicleLoadEntry.FMRefTablekey && x.VehicleNO == LrVehicleLoadEntry.StockAt).FirstOrDefault();
                                AddEntry = false;
                            }
                            unLoadDetails.Branch = mbranchcode;
                            unLoadDetails.LrNo = LrVehicleLoadEntry.LrNo.Value;
                            unLoadDetails.LRRefTablekey = LrVehicleLoadEntry.LRRefTablekey;
                            unLoadDetails.LCNO = LrVehicleLoadEntry.LCNO.Value;
                            unLoadDetails.LCRefTablekey = LrVehicleLoadEntry.LCRefTablekey;
                            unLoadDetails.FMNO = LrVehicleLoadEntry.Fmno.Value;
                            unLoadDetails.FMRefTablekey = LrVehicleLoadEntry.FMRefTablekey;
                            unLoadDetails.VehicleNO = LrVehicleLoadEntry.StockAt.ToString();
                            unLoadDetails.Date = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());
                            unLoadDetails.Time = (DateTime.Now.ToString("HH:mm"));
                            unLoadDetails.GQty -= Qty;
                            unLoadDetails.Weight -= Weight;
                            unLoadDetails.ChrgWeight = LrVehicleLoadEntry.ChrgWeight;
                            unLoadDetails.ChrgType = LrVehicleLoadEntry.ChrgType;
                            unLoadDetails.Description = LrVehicleLoadEntry.Description == null ? "" : LrVehicleLoadEntry.Description;
                            unLoadDetails.Unit = LrVehicleLoadEntry.Unit;
                            unLoadDetails.FromBranch = LrVehicleLoadEntry.FromBranch;
                            unLoadDetails.ToBranch = LrVehicleLoadEntry.ToBranch;
                            unLoadDetails.Consigner = LrVehicleLoadEntry.Consigner;
                            unLoadDetails.Consignee = LrVehicleLoadEntry.Consignee;
                            unLoadDetails.LrType = LrVehicleLoadEntry.LrType;
                            unLoadDetails.Coln = LrVehicleLoadEntry.Coln;
                            unLoadDetails.Delivery = (LrVehicleLoadEntry.Delivery);
                            unLoadDetails.Remark = LrVehicleLoadEntry.Remark;
                            unLoadDetails.Prefix = mperiod;
                            unLoadDetails.ENTEREDBY = muserid;
                            unLoadDetails.AUTHIDS = muserid;
                            unLoadDetails.AUTHORISE = mauthorise;
                            unLoadDetails.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToString());
                            if (AddEntry)
                            {
                                ctxTFAT.UnLoadDetails.Add(unLoadDetails);
                            }
                            else
                            {
                                ctxTFAT.Entry(unLoadDetails).State = EntityState.Modified;
                            }
                        }
                        else
                        {
                            ErroMsg += "   Available Material Of This " + LrVehicleLoadEntry.LrNo + " Lr Is " + RStock.BalQty + " So U Cannot Reverse " + Qty + " Qty." + "\n,";
                            StockUpdateOrNot = false;
                        }
                    }
                    else
                    {
                        ErroMsg = "This " + OldlRStock.LrNo + " No Qty Has Consumed So We Cant Unload IT(Reverse). \n";
                    }
                }
                else
                {
                    ErroMsg = "Stock Not Found To Reverse Qty....!";
                }
            }
            else
            {

            }
            return ErroMsg;
        }

    }
}