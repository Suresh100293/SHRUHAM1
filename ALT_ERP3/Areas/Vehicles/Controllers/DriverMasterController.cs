using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ALT_ERP3.Areas.Vehicles.Models;
using ALT_ERP3.Controllers;
using ALT_ERP3.Areas.Accounts.Models;
using ALT_ERP3.Areas.Logistics.Models;
using System.Configuration;
using ALT_ERP3.Areas.Logistics.Controllers;

namespace ALT_ERP3.Areas.Vehicles.Controllers
{
    public class DriverMasterController : BaseController
    {
        string attachmentPath = string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentPath"].ToString()) == true ? "" : ConfigurationManager.AppSettings["AttachmentPath"].ToString();
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;

        #region GetList
        private List<SelectListItem> PopulatePriceLists()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            items.Add(new SelectListItem
            {
                Text = "-- Select Document Type --",
                Value = "0"
            });
            items.Add(new SelectListItem
            {
                Text = "PanCard",
                Value = "PanCard"
            });
            items.Add(new SelectListItem
            {
                Text = "AdharCard",
                Value = "AdharCard"
            });

            return items;
        }

        public JsonResult GetZone(string term)
        {
            var list = ctxTFAT.TfatBranch.Where(x => x.Category == "ZoneView").ToList().Distinct();
            if (!(String.IsNullOrEmpty(term)))
            {
                list = ctxTFAT.TfatBranch.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }

            var Modified = list.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
            //return Json((from m in ctxTFAT.VehicleMaster
            //             where m.Vehicle_No.ToLower().Contains(term.ToLower())
            //             select new { Name = m.Vehicle_No, Code = m.Vehicle_No }).ToArray(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBranch(string term)
        {
            List<TfatBranch> list = ctxTFAT.TfatBranch.Where(x => x.Category != "Zone" && x.Category != "0").ToList();

            if (!(String.IsNullOrEmpty(term)))
            {
                list = list.Where(x => x.Name.ToLower().Contains(term.ToLower())).ToList();
            }

            List<TfatBranch> treeTables = new List<TfatBranch>();

            foreach (var item in list)
            {
                if (item.Category == "Zone")
                {
                    item.Name += " - Z";
                    treeTables.Add(item);
                }
                else if (item.Category == "Branch")
                {
                    item.Name += " - B";
                    treeTables.Add(item);
                }
                else if (item.Category == "SubBranch")
                {
                    item.Name += " - SB";
                    treeTables.Add(item);
                }
                else
                {
                    item.Name = item.Name + " - A";
                    treeTables.Add(item);
                }
            }
            var Modified = treeTables.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCreditors(string term)
        {

            var result = ctxTFAT.Master.Where(x => x.Hide == false /*&& x.IsLast== true*/ /*&& x.AcType == "S"*/ && x.OthPostType.Contains("D")).Select(m => new { m.Code, m.Name }).ToList();

            if (!(String.IsNullOrEmpty(term)))
            {
                result = ctxTFAT.Master.Where(x => x.Hide == false /*&& x.IsLast== true*/ /*&& x.AcType == "S"*/ && x.OthPostType.Contains("D") && x.Name.Contains(term)).Select(m => new { m.Code, m.Name }).ToList();
            }


            var Modified = result.Select(x => new
            {
                Code = x.Code,
                Name = x.Name
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);




        }

        public JsonResult GetVehicleNos(string term)
        {
            var result = ctxTFAT.VehicleMaster.Where(x => x.Acitve == true).Select(m => new { m.Code, m.TruckNo }).ToList();
            if (!(String.IsNullOrEmpty(term)))
            {
                result = ctxTFAT.VehicleMaster.Where(x => x.Acitve == true && x.TruckNo.Contains(term)).Select(m => new { m.Code, m.TruckNo }).ToList();
            }
            var Modified = result.Select(x => new
            {
                Code = x.Code,
                Name = x.TruckNo
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }

        #endregion

        [HttpPost]
        public ActionResult GetGridStructureRecords(string id)
        {
            mpara = "";
            //return GetGridDataColumns(id, "L", mopt);
            return GetGridDataColumns(id, "L", "XXXX");
        }

        //[HttpPost]
        public ActionResult GetMasterGridData(GridOption Model)
        {
            ExecuteStoredProc("Drop Table ztmp_TfatDriverHistory");
            string Query = "with CTE_RN as ( select t.*, ROW_NUMBER() OVER(ORDER BY fromperiod, FromTime) as RN from VehicleDri_Hist as t where Driver = '" + Model.Code + "') select   LASTUPDATEDATE, FromPeriod, FromTime, TruckNo,DATEDIFF(Day, FromPeriod, (select FromPeriod from CTE_RN G where G.RN = C.RN + 1)) as [Days],ENTEREDBY,Narr into ztmp_TfatDriverHistory from CTE_RN as c";
            ExecuteStoredProc(Query);
            mpara = "para09^" + Model.Code;
            return GetGridReport(Model, "M", mpara, false, 0);
        }


        // GET: Vehicles/DriverMaster
        public ActionResult Index(DriverMasterVM mModel)
        {
            GetAllMenu(Session["ModuleName"].ToString());
            Session["TempAttach"] = null;
            UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "", DateTime.Now, 0, "", "", "NA");

            mdocument = mModel.Document;
            mModel.ValidProoflist = PopulatePriceLists();
            if ((mModel.Mode == "Edit") || (mModel.Mode == "View") || (mModel.Mode == "Delete"))
            {
                var mList = ctxTFAT.DriverMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                //var PartyCode = Convert.ToInt32(mList.Code);

                if (mList != null)
                {

                    //Get Attachment
                    AttachmentVM Att = new AttachmentVM();
                    Att.Type = "Drive";
                    Att.Srl = mList.Code.ToString();

                    AttachmentController attachmentC = new AttachmentController();
                    List<AttachmentVM> attachments = attachmentC.GetAttachmentListInEdit(Att);
                    Session["TempAttach"] = attachments;


                    mModel.Code = mList.Code;
                    mModel.Name = mList.Name;
                    mModel.Nick_Name = mList.NickName;
                    mModel.MobileNo1 = mList.MobileNo1;
                    mModel.MobileNo2 = mList.MobileNo2;
                    mModel.Guaranter = mList.Guaranter;
                    mModel.Reference = mList.Reference;
                    mModel.LicenceNo = mList.LicenceNo;
                    if (mList.LicenceExpDate != null)
                    {
                        mModel.LicenceExpDate = mList.LicenceExpDate.Value.ToShortDateString();
                    }
                    mModel.ValidProof = mList.ValidProof;
                    mModel.ProofNo = mList.ProofNo;
                    mModel.VehicleNo = ctxTFAT.VehicleDri_Hist.Where(x => x.Driver == mList.Code).OrderByDescending(x => new { x.FromPeriod, x.FromTime }).Select(x => x.TruckNo).FirstOrDefault();
                    //mModel.VehicleNo = mList.VehicleNo;
                    mModel.VehicleNoN = ctxTFAT.VehicleMaster.Where(x => x.Code == mModel.VehicleNo).Select(x => x.TruckNo).FirstOrDefault();
                    mModel.ZoneCode = mList.Branch;
                    mModel.ZoneName = ctxTFAT.TfatBranch.Where(x => x.Code == mList.Branch).Select(x => x.Name).FirstOrDefault();
                    mModel.BranchCode = mList.Branch;
                    mModel.BranchName = ctxTFAT.TfatBranch.Where(x => x.Code == mList.Branch).Select(x => x.Name).FirstOrDefault();
                    mModel.Active = mList.Status;
                    mModel.Posting = mList.Posting;

                    mModel.PostingName = ctxTFAT.Master.Where(x => x.Code == mList.Posting).Select(x => x.Name).FirstOrDefault();

                    mModel.Ticklers = mList.Ticklers;
                    mModel.HoldTicklers = mList.HoldTicklers;

                    //mModel.Active
                    //var AttachmentList = ctxTFAT.Attachment.Where(x => (x.ParentCode == mList.AttachmentCode)).ToList();
                    //if (AttachmentList != null)
                    //{
                    //    List<AttachmentDocumentVM> attachmentDocumentVMlist = new List<AttachmentDocumentVM>();

                    //    foreach (var item in AttachmentList)
                    //    {
                    //        AttachmentDocumentVM attachmentDocumentVM = new AttachmentDocumentVM
                    //        {
                    //            AttachmentCode = item.Code,
                    //            TypeOfAttachment = item.Type_Of_Attachement,
                    //            FileName = item.FileName,
                    //            DocumentString = item.DocumentString,
                    //            ContentType = item.ContentType,
                    //            Image= Convert.FromBase64String(item.DocumentString)
                    //    };
                    //        attachmentDocumentVMlist.Add(attachmentDocumentVM);
                    //    }
                    //    Session["AttachmentList"] = attachmentDocumentVMlist;
                    //    mModel.AttachmentList = attachmentDocumentVMlist;
                    //}
                }
            }
            else
            {
                mModel.Code = "";
                mModel.Name = "";
                mModel.Nick_Name = "";
                mModel.MobileNo1 = "";
                mModel.MobileNo2 = "";
                mModel.Guaranter = "";
                mModel.LicenceNo = "";
                mModel.LicenceExpDate = "";
                mModel.ValidProof = "";
                mModel.ProofNo = "";
                mModel.VehicleNo = "";
                mModel.AttachmentList = null;
                Session["AttachmentList"] = mModel.AttachmentList;
                mModel.Active = true;
            }
            return View(mModel);
        }

        #region SaveData
        public ActionResult SaveData(DriverMasterVM mModel)
        {
            //string OldVehicleNo = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    string FinalCode = "";
                    // iX9: Remove Existing Data for Delete Mode
                    if (mModel.Mode == "Delete")
                    {
                        DeleteStateMaster(mModel);
                        transaction.Commit();
                        transaction.Dispose();
                        return Json(new { Status = "Success", Message = "Data is Deleted." }, JsonRequestBehavior.AllowGet);
                    }
                    DriverMaster mobj = new DriverMaster();
                    List<AttachmentDocumentVM> SessionAttachList = Session["AttachmentList"] as List<AttachmentDocumentVM>;
                    bool mAdd = true;

                    if (ctxTFAT.DriverMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault() != null)
                    {
                        mobj = ctxTFAT.DriverMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault();
                        //OldVehicleNo = mobj.VehicleNo == null ? "" : mobj.VehicleNo;
                        mAdd = false;
                    }

                    if (mAdd)
                    {
                        int NewCode1;
                        var NewCode = ctxTFAT.DriverMaster.Where(x => x.Code != "99999").OrderByDescending(x => x.Code).Select(x => x.Code).Take(1).FirstOrDefault();
                        if (NewCode == null || NewCode == "")
                        {
                            NewCode1 = 100000;
                        }
                        else
                        {
                            NewCode1 = Convert.ToInt32(NewCode) + 1;
                        }
                        FinalCode = NewCode1.ToString("D6");
                        mobj.Code = FinalCode;
                    }



                    mobj.Name = mModel.Name;
                    mobj.NickName = mModel.Nick_Name;
                    mobj.MobileNo1 = (mModel.MobileNo1);
                    mobj.MobileNo2 = (mModel.MobileNo2);
                    mobj.LicenceNo = mModel.LicenceNo;
                    if (String.IsNullOrEmpty(mModel.LicenceExpDate))
                    {
                        mobj.LicenceExpDate = null;
                    }
                    else
                    {
                        mobj.LicenceExpDate = ConvertDDMMYYTOYYMMDD(mModel.LicenceExpDate);
                    }


                    //mModel.VehicleNo = ctxTFAT.VehicleDri_Hist.OrderByDescending(x => new { x.FromPeriod, x.FromTime }).Select(x => x.TruckNo).FirstOrDefault();
                    mobj.Guaranter = mModel.Guaranter;
                    mobj.Reference = mModel.Reference;
                    //mobj.VehicleNo = mModel.VehicleNo;
                    mobj.ValidProof = mModel.ValidProof;
                    mobj.ProofNo = mModel.ProofNo;
                    mobj.Status = mModel.Active;
                    mobj.Branch = mModel.ZoneCode;
                    mobj.Branch = mModel.BranchCode;
                    //mobj.AttachmentCode = Guid.NewGuid().ToString();
                    mobj.Posting = mModel.Posting;
                    mobj.Ticklers = mModel.Ticklers;
                    mobj.HoldTicklers = mModel.HoldTicklers;

                    //iX9: Save default values to Std fields
                    mobj.AUTHIDS = muserid;
                    mobj.AUTHORISE = mauthorise;
                    mobj.ENTEREDBY = muserid;
                    mobj.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());


                    #region Maintain History

                    //if (mobj.Code!= "99999")
                    //{
                    //    if (mobj.VehicleNo.Trim() != OldVehicleNo.Trim())
                    //    {
                    //        var HistoryCode= GetNewCode_Vehi_Driver();

                    //        #region OLD VEHICLE

                    //        if (OldVehicleNo.Trim() != "99998" && OldVehicleNo.Trim() != "99999")
                    //        {
                    //            VehicleMaster OldvehicleMaster = ctxTFAT.VehicleMaster.Where(x => x.Code == OldVehicleNo.Trim()).FirstOrDefault();
                    //            if (OldvehicleMaster != null)
                    //            {
                    //                OldvehicleMaster.Driver = "99999";
                    //                ctxTFAT.Entry(OldvehicleMaster).State = EntityState.Modified;
                    //            }
                    //        }

                    //        VehicleDri_Hist OldvehicleHistory = ctxTFAT.VehicleDri_Hist.Where(x => x.TruckNo == OldVehicleNo.Trim() && x.Driver == mobj.Code.Trim() && x.ToPeriod == null).FirstOrDefault();
                    //        if (OldvehicleHistory != null)
                    //        {
                    //            OldvehicleHistory.ToPeriod = DateTime.Now;
                    //            ctxTFAT.Entry(OldvehicleHistory).State = EntityState.Modified;
                    //        }
                    //        #endregion

                    //        #region CURRENT VEHICLE

                    //        if (mobj.VehicleNo == "99999" || mobj.VehicleNo == "99998")
                    //        {
                    //            VehicleDri_Hist vehicleDriver = new VehicleDri_Hist();
                    //            vehicleDriver.Code = HistoryCode;
                    //            vehicleDriver.TruckNo = mobj.VehicleNo.ToUpper().Trim();
                    //            vehicleDriver.Driver = mobj.Code;
                    //            vehicleDriver.FromPeriod = DateTime.Now;
                    //            vehicleDriver.Narr = mModel.DriverStatusChangeNarr;
                    //            vehicleDriver.ToPeriod = null;
                    //            vehicleDriver.AUTHIDS = muserid;
                    //            vehicleDriver.AUTHORISE = mauthorise;
                    //            vehicleDriver.ENTEREDBY = muserid;
                    //            vehicleDriver.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                    //            ctxTFAT.VehicleDri_Hist.Add(vehicleDriver);
                    //            HistoryCode = (Convert.ToInt32(HistoryCode) + 1).ToString();
                    //        }
                    //        else
                    //        {
                    //            var CurrnetVehicle = ctxTFAT.VehicleMaster.Where(x => x.Code == mobj.VehicleNo).FirstOrDefault();
                    //            if (CurrnetVehicle != null)
                    //            {
                    //                OldvehicleHistory = ctxTFAT.VehicleDri_Hist.Where(x => x.TruckNo == mobj.VehicleNo.Trim() && x.Driver == CurrnetVehicle.Driver.Trim() && x.ToPeriod == null).FirstOrDefault();
                    //                if (OldvehicleHistory != null)
                    //                {
                    //                    OldvehicleHistory.ToPeriod = DateTime.Now;
                    //                    ctxTFAT.Entry(OldvehicleHistory).State = EntityState.Modified;
                    //                }
                    //                var OldDriver = ctxTFAT.DriverMaster.Where(x => x.Code == CurrnetVehicle.Driver && x.Code != "99999").FirstOrDefault();
                    //                if (OldDriver != null)
                    //                {
                    //                    VehicleDri_Hist vehicleDriver1 = new VehicleDri_Hist();
                    //                    vehicleDriver1.Code = HistoryCode;
                    //                    vehicleDriver1.TruckNo = "99999";
                    //                    vehicleDriver1.Driver = OldDriver.Code;
                    //                    vehicleDriver1.FromPeriod = DateTime.Now;
                    //                    vehicleDriver1.Narr = mModel.DriverStatusChangeNarr;
                    //                    vehicleDriver1.ToPeriod = null;
                    //                    vehicleDriver1.AUTHIDS = muserid;
                    //                    vehicleDriver1.AUTHORISE = mauthorise;
                    //                    vehicleDriver1.ENTEREDBY = muserid;
                    //                    vehicleDriver1.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                    //                    ctxTFAT.VehicleDri_Hist.Add(vehicleDriver1);
                    //                    HistoryCode = (Convert.ToInt32(HistoryCode) + 1).ToString();

                    //                    OldDriver.VehicleNo = "99999";
                    //                    ctxTFAT.Entry(OldDriver).State = EntityState.Modified;
                    //                }

                    //                CurrnetVehicle.Driver = mobj.Code;
                    //                ctxTFAT.Entry(CurrnetVehicle).State = EntityState.Modified;

                    //            }
                    //            VehicleDri_Hist vehicleDriver = new VehicleDri_Hist();
                    //            vehicleDriver.Code = HistoryCode;
                    //            vehicleDriver.TruckNo = mobj.VehicleNo.ToUpper().Trim();
                    //            vehicleDriver.Driver = mobj.Code;
                    //            vehicleDriver.FromPeriod = DateTime.Now;
                    //            vehicleDriver.Narr = mModel.DriverStatusChangeNarr;
                    //            vehicleDriver.ToPeriod = null;
                    //            vehicleDriver.AUTHIDS = muserid;
                    //            vehicleDriver.AUTHORISE = mauthorise;
                    //            vehicleDriver.ENTEREDBY = muserid;
                    //            vehicleDriver.LASTUPDATEDATE = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                    //            ctxTFAT.VehicleDri_Hist.Add(vehicleDriver);
                    //            HistoryCode = (Convert.ToInt32(HistoryCode) + 1).ToString();
                    //        }
                    //        #endregion
                    //    }
                    //}

                    #endregion

                    if (mAdd == true)
                    {
                        ctxTFAT.DriverMaster.Add(mobj);
                        AttachmentVM vM = new AttachmentVM();
                        vM.Srl = mobj.Code.ToString();
                        vM.ParentKey = mobj.Code.ToString();
                        vM.Type = "Drive";
                        SaveAttachment(vM);
                    }
                    else
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }






                    ctxTFAT.SaveChanges();
                    mnewrecordkey = Convert.ToInt32(mobj.RECORDKEY);
                    //string mNewCode = "";
                    //mNewCode = mobj.Code;
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mobj.Code, DateTime.Now, 0, mobj.Code, "Save DRIVER MASTER", "DM");

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
            return Json(new { Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
        }
        public string GetNewCode_Vehi_Driver()
        {
            string Code = ctxTFAT.VehicleDri_Hist.OrderByDescending(x => x.RECORDKEY).Select(x => x.Code).FirstOrDefault();
            if (String.IsNullOrEmpty(Code))
            {

                return "100000";
            }
            else
            {
                return (Convert.ToInt32(Code) + 1).ToString();
            }
        }
        public ActionResult DeleteStateMaster(DriverMasterVM mModel)
        {
            if (mModel.Document == null || mModel.Document == "")
            {
                return Json(new
                {
                    Message = "Code not Entered..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            if (mModel.Document == "99999")
            {
                return Json(new
                {
                    Message = "Not Allowed To Delete..",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }




            var mList = ctxTFAT.DriverMaster.Where(x => (x.Code == mModel.Document)).FirstOrDefault();

            string mactivestring = "";
            var mactive1 = ctxTFAT.VehicleMaster.Where(x => (x.Driver.ToUpper().Trim() == mList.Code.ToUpper().Trim())).Select(x => x.TruckNo).FirstOrDefault();
            if (mactive1 != null)
            {
                mactivestring = mactivestring + "\nThis Driver Connected To " + mactive1 + " This Vehicle ";
            }

            var mactive2 = ctxTFAT.FMMaster.Where(x => (x.Driver.ToUpper().Trim() == mList.Code.ToUpper().Trim())).Select(x => x.FmNo).FirstOrDefault();
            if (mactive2 != 0)
            {
                mactivestring = mactivestring + "\nThis Driver Connected To " + mactive2 + " This FM ";
            }




            if (mactivestring != "")
            {
                return Json(new
                {
                    Message = "Active Account, Can't Delete..\nUsed with:\n" + mactivestring,
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    ctxTFAT.DriverMaster.Remove(mList);
                    var DriverAttachmentList = ctxTFAT.Attachment.Where(x => x.Type == "Drive" && x.Srl == mList.Code).ToList();
                    foreach (var item in DriverAttachmentList)
                    {
                        if (System.IO.File.Exists(item.FilePath))
                        {
                            System.IO.File.Delete(item.FilePath);
                        }
                    }
                    ctxTFAT.Attachment.RemoveRange(DriverAttachmentList);

                    ctxTFAT.SaveChanges();
                    transaction.Commit();
                    transaction.Dispose();
                    UpdateAuditTrail(mbranchcode, mModel.Mode, mModel.Header, "     " + mperiod.Substring(0, 2) + mList.Code, DateTime.Now, 0, mList.Code, "Delete DRIVER MASTER", "DM");

                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();
                    return Json(new
                    {
                        Message = ex1.InnerException.Message,
                        Status = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new
                    {
                        Message = ex.InnerException.Message,
                        Status = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new
            {
                Status = "Success"
            }, JsonRequestBehavior.AllowGet);

        }
        #endregion SaveData

        #region Attachment (Download,View,Delete,Save)
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

        public void SaveAttachment(AttachmentVM Model)
        {
            List<AttachmentVM> DocList = new List<AttachmentVM>();
            if (Session["TempAttach"] != null)
            {
                DocList = (List<AttachmentVM>)Session["TempAttach"];
            }

            if (DocList != null && DocList.Count != 0)
            {
                var AttachCode = GetNewAttachCode();
                int J = 1;
                foreach (var item in DocList.ToList())
                {
                    Directory.CreateDirectory(attachmentPath + Model.ParentKey);
                    string directoryPath = attachmentPath + Model.ParentKey + "/" + item.FileName;
                    byte[] IData = Convert.FromBase64String(item.ImageStr);
                    Attachment att = new Attachment();
                    att.AUTHIDS = String.IsNullOrEmpty(item.ENTEREDBY) == true ? muserid : item.ENTEREDBY;
                    att.DocDate = ConvertDDMMYYTOYYMMDD(item.DocDate);
                    att.AUTHORISE = mauthorise;
                    att.Branch = mbranchcode;
                    att.Code = AttachCode.ToString();
                    att.ENTEREDBY = String.IsNullOrEmpty(item.ENTEREDBY) == true ? muserid : item.ENTEREDBY;
                    att.FilePath = directoryPath;
                    att.LASTUPDATEDATE = DateTime.Now;
                    att.LocationCode = Model.LocationCode;
                    att.Prefix = mperiod;
                    att.Sno = J;
                    att.Srl = Model.Srl;
                    att.SrNo = J;
                    att.TableKey = Model.Type + mperiod.Substring(0, 2) + J.ToString("D3") + Model.Srl;
                    att.Type = String.IsNullOrEmpty(item.Type) == true ? "Drive" : item.Type;
                    att.ParentKey = Model.ParentKey;

                    att.CompCode = mcompcode;
                    att.ExternalAttach = item.ExternalAttach;
                    att.RefDocNo = item.RefCode;
                    att.RefType = item.RefType;
                    att.DocType = item.DocType;
                    ctxTFAT.Attachment.Add(att);
                    
                    ctxTFAT.SaveChanges();
                    System.IO.File.WriteAllBytes(directoryPath, IData);

                    ++AttachCode;
                    J++;
                }
            }

        }

        #endregion

        public ActionResult GenerareCreditors(string DriverName)
        {
            Session["MailInfo"] = null;
            MasterVM Model = new MasterVM();
            List<MasterVM> AddressList1 = new List<MasterVM>();

            var Company = ctxTFAT.TfatComp.Where(x => x.Code.ToString() == mcompcode).FirstOrDefault();
            var countryname = ctxTFAT.TfatCountry.Where(x => x.Code.ToString() == Company.Country).FirstOrDefault();
            var statename = ctxTFAT.TfatState.Where(x => x.Code.ToString() == Company.State).FirstOrDefault();
            var cityname = ctxTFAT.TfatCity.Where(x => x.Code.ToString() == Company.City).FirstOrDefault();

            AddressList1.Add(new MasterVM
            {
                Ifexist = true,
                SrNo = 0,
                AName = "",
                CorpID = mcompcode,
                Person = "",
                Adrl1 = "",
                Adrl2 = "",
                Adrl3 = "",
                Country = countryname.Code.ToString(),
                CountryName = countryname.Name,
                State = statename.Code.ToString(),
                StateName = statename.Name,
                City = cityname.Code.ToString(),
                CityName = cityname.Name,
                Pin = "",
                Area = 0,
                AreaName = "",
                Tel1 = "",
                Fax = "",
                Mobile = "",
                www = "",
                Email = "",
                MailingCategory = 0,
                UserID = muserid,
                CorrespondenceType = 0,
                Password = "",
                Source = 0,
                Segment = "",
                STaxCode = "",
                PTaxCode = "",
                Licence1 = "",
                Licence2 = "",
                PanNo = "",
                TINNo = "",
                Designation = 0,
                Language = 0,
                Dept = 0,
                Religion = 0,
                Division = 0,
                Bdate = DateTime.Now,
                Anndate = DateTime.Now,
                SpouseName = "",
                SpouseBdate = DateTime.Now,
                ChildName = "",
                ChildBdate = DateTime.Now,
                Code = "",
                ContactType = "1",
                AssistEmail = "",
                AssistMobile = "",
                AssistTel = "",
                AssistName = "",
                DefaultIGst = 0,
                DefaultSGst = 0,
                DefaultCGst = 0,
                VATReg = "",
                AadharNo = "",
                GSTNo = "",
                GSTType = "0",
                PoisonLicense = "",
                ReraNo = "",
                DealerType = "0"
            });
            Model.AddressList = AddressList1;

            if (AddressList1 != null)
            {
                Model.Ifexist = true;
                Model.SrNo = 0;
                Model.AName = "";
                Model.CorpID = mcompcode;
                Model.Person = "";
                Model.Adrl1 = "";
                Model.Adrl2 = "";
                Model.Adrl3 = "";
                Model.Country = countryname.Code.ToString();
                Model.CountryName = countryname.Name;
                Model.State = statename.Code.ToString();
                Model.StateName = statename.Name;
                Model.City = cityname.Code.ToString();
                Model.CityName = cityname.Name;
                Model.Pin = "";
                Model.Area = 0;
                Model.AreaName = "";
                Model.Tel1 = "";
                Model.Fax = "";
                Model.Mobile = "";
                Model.www = "";
                Model.Email = "";
                Model.MailingCategory = 0;
                Model.UserID = muserid;
                Model.CorrespondenceType = 0;
                Model.Password = "";
                Model.Source = 0;
                Model.Segment = "";
                Model.STaxCode = "";
                Model.PTaxCode = "";
                Model.Licence1 = "";
                Model.Licence2 = "";
                Model.PanNo = "";
                Model.TINNo = "";
                Model.Designation = 0;
                Model.Language = 0;
                Model.Dept = 0;
                Model.Religion = 0;
                Model.Division = 0;
                Model.Bdate = DateTime.Now;
                Model.Anndate = DateTime.Now;
                Model.SpouseName = "";
                Model.SpouseBdate = DateTime.Now;
                Model.ChildName = "";
                Model.ChildBdate = DateTime.Now;
                Model.Code = "";
                Model.ContactType = "1";
                Model.AssistEmail = "";
                Model.AssistMobile = "";
                Model.AssistTel = "";
                Model.AssistName = "";
                Model.DefaultIGst = 0;
                Model.DefaultSGst = 0;
                Model.DefaultCGst = 0;
                Model.VATReg = "";
                Model.AadharNo = "";
                Model.GSTNo = "";
                Model.GSTType = "0";
                Model.PoisonLicense = "";
                Model.ReraNo = "";
                Model.DealerType = "0";
                Model.Tel2 = "";
                Model.Tel3 = "";
            }

            Model.MailList = AddressList1;
            Session.Add("MailInfo", AddressList1);

            Model.AName = DriverName;
            Model.AcType = "S";

            Model.Collection = "D";
            Model.Delivery = "D";

            Model.AppBranch = mbranchcode;
            Model.City = "411111";
            Model.ContactType = "1";
            Model.CorpID = "100";
            Model.Country = "1";
            Model.DealerType = "0";
            Model.EmailTemplate = "False";
            Model.GSTType = "0";
            Model.Grp = "000000044";
            Model.Header = "Suppliers (Vendors/Creditors)";
            Model.Name = DriverName;
            Model.OptionGstType = "0";
            Model.SMSTemplate = "False";
            Model.ShortName = null;
            Model.State = "19";


            var Result = DriverPostIngAc(Model);
            return Result;
        }

        public ActionResult DriverPostIngAc(MasterVM Model)
        {
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {

                    Model.Code = GetCode(0, 9, Model.Grp);

                    var delmasterinfo = ctxTFAT.MasterInfo.Where(x => x.Code == Model.Document).FirstOrDefault();
                    var deladdress = ctxTFAT.Address.Where(x => x.Code == Model.Document).ToList();
                    var delholdtrx = ctxTFAT.HoldTransactions.Where(x => x.Code == Model.Document).FirstOrDefault();
                    var deltaxdet = ctxTFAT.TaxDetails.Where(x => x.Code == Model.Document).FirstOrDefault();
                    var deltaddons = ctxTFAT.AddonMas.Where(x => x.TableKey == Model.Document).ToList();
                    //SAttach var deltdoc = ctxTFAT.Attachment.Where(x => x.ParentKey == "Master_" + Model.Document).ToList();
                    if (delmasterinfo != null)
                    {
                        ctxTFAT.MasterInfo.Remove(delmasterinfo);
                    }
                    if (deladdress != null)
                    {
                        ctxTFAT.Address.RemoveRange(deladdress);
                    }
                    if (delholdtrx != null)
                    {
                        ctxTFAT.HoldTransactions.Remove(delholdtrx);
                    }
                    if (deltaxdet != null)
                    {
                        ctxTFAT.TaxDetails.Remove(deltaxdet);
                    }
                    if (deltaddons.Count > 0)
                    {
                        ctxTFAT.AddonMas.RemoveRange(deltaddons);
                    }

                    // SAttach if (deltdoc.Count > 0)
                    //{
                    //    ctxTFAT.Attachment.RemoveRange(deltdoc);
                    //}
                    ctxTFAT.SaveChanges();

                    Master mobj = new Master();
                    if (Model.Mode == "Edit")
                    {
                        mobj = ctxTFAT.Master.Where(x => x.Code == Model.Code).Select(x => x).FirstOrDefault();
                    }


                    //Add Driver
                    mobj.OthPostType = "D";
                    mobj.Code = Model.Code;
                    mobj.Grp = Model.Grp;
                    mobj.Name = Model.Name.ToUpper().Trim();
                    mobj.ForceCC = Model.CCReqd;
                    mobj.AcHeadCode = (Model.AcHeadCode == null) ? "" : Model.AcHeadCode;
                    mobj.AcType = (Model.AcType == null) ? "" : Model.AcType;
                    mobj.ARAP = Model.ARAP;
                    mobj.AUTHIDS = muserid;
                    mobj.AppBranch = Model.AppBranch;
                    mobj.AUTHORISE = "A00";
                    mobj.BaseGr = (Model.AcType == null) ? "" : Model.AcType;
                    mobj.Category = (Model.Category == null) ? 0 : Model.Category;
                    mobj.CCBudget = Model.CCBudget;
                    mobj.Hide = Model.Hide;
                    mobj.IsPublic = Model.IsPublic;
                    mobj.NonActive = Model.NonActive;
                    mobj.ShortName = (Model.ShortName == null) ? "" : Model.ShortName;
                    mobj.SalesMan = Model.SalesMan;
                    mobj.Broker = Model.Broker;
                    mobj.ENTEREDBY = muserid;
                    mobj.GroupTree = GetGroupTree(Model.Grp);
                    mobj.IsSubLedger = Model.IsSubLedger;
                    mobj.LASTUPDATEDATE = DateTime.Now;
                    mobj.UserID = (Model.AdminUser == null) ? "" : Model.AdminUser;
                    mobj.CreateDate = ConvertDDMMYYTOYYMMDD(System.DateTime.Now.ToString());

                    if (Model.Mode == "Edit")
                    {
                        ctxTFAT.Entry(mobj).State = EntityState.Modified;
                    }
                    else
                    {
                        ctxTFAT.Master.Add(mobj);
                    }

                    MasterInfo mobj2 = new MasterInfo();
                    mobj2.AppProduct = "";
                    mobj2.Area = null;
                    mobj2.AUTHIDS = muserid;
                    mobj2.AUTHORISE = "A00";
                    mobj2.BaseGr = (Model.AcType == null) ? "" : Model.AcType;
                    mobj2.Brokerage = 0;
                    mobj2.CashDisc = Model.CashDisc;
                    mobj2.CheckCRDays = Model.CheckCRDays;
                    mobj2.CheckCRLimit = Model.CheckCRLimit;
                    mobj2.Code = Model.Code;
                    mobj2.CompanyType = "";
                    mobj2.CostCentre = null;
                    mobj2.CRDaysWarn = (Model.CRDaysWarn == 0) ? false : true;
                    mobj2.CreatedOn = DateTime.Now;
                    mobj2.CrLimit = Model.CrLimit;
                    mobj2.CRLimitTole = Model.CRLimitTole;
                    mobj2.CRLimitWarn = (Model.CRLimitWarn == 0) ? false : true;
                    mobj2.CRLimitWithPO = Model.CRLimitWithPO;
                    mobj2.CRLimitWithTrx = Model.CRLimitWithTrx;
                    mobj2.CrPeriod = Convert.ToInt32(Model.CRPeriod);
                    mobj2.CurrName = Convert.ToInt32(Model.CurrCode);
                    mobj2.CutTDS = Model.CutTDS;
                    mobj2.DepricAC = "";
                    mobj2.DiscDays = Model.DiscDays;
                    mobj2.DiscPerc = Model.DiscPerc;
                    mobj2.EmailParty = Model.EmailParty;
                    mobj2.EmailPartyAlert = Model.EmailPartyAlert;
                    mobj2.EmailSalesman = Model.EmailSalesman;
                    mobj2.EmailTemplate = Model.EmailTemplate;
                    mobj2.EmailUsers = "";
                    mobj2.PriceList = Model.PriceList;
                    mobj2.FreqOS = (Model.FreqOS == null) ? 0 : Convert.ToInt32(Model.FreqOS);
                    mobj2.SGSTRate = Model.SGST;
                    mobj2.IGSTRate = Model.IGST;
                    mobj2.CGSTRate = Model.CGST;
                    mobj2.FreqForm = (Model.FreqForm == null) ? 0 : Convert.ToInt32(Model.FreqForm);
                    mobj2.Grp = Model.Grp;
                    mobj2.IntAmt = Convert.ToDecimal(0.00);
                    mobj2.IntRate = Model.IntRate;
                    mobj2.LastUpdateBy = muserid;

                    mobj2.LeadCode = "";
                    mobj2.LeadConvertDt = DateTime.Now;
                    mobj2.Name = Model.Name;
                    mobj2.Narr = (Model.Narr == null) ? "" : Model.Narr;
                    mobj2.PaymentTerms = (Model.PaymentTerms == null) ? "" : Model.PaymentTerms;
                    mobj2.Rank = (Model.Rank == null) ? 0 : Convert.ToInt32(Model.Rank);
                    mobj2.ReminderFormat = (Model.ReminderFormat == null) ? "" : Model.ReminderFormat;
                    mobj2.SMSTemplate = (Model.SMSTemplate == null) ? "" : Model.SMSTemplate;
                    mobj2.SMSUsers = "";
                    mobj2.SMSParty = Model.SMSParty;
                    mobj2.SMSSalesman = Model.SMSSalesman;
                    mobj2.IncoPlace = (Model.IncoPlace == null) ? "" : Model.IncoPlace;
                    mobj2.IncoTerms = (Model.IncoTerms == null) ? 0 : Model.IncoTerms;
                    mobj2.CurrName = Model.CurrCode;
                    mobj2.HSNCode = (Model.HSN == null) ? "" : Model.HSN;
                    mobj2.ReminderFormat = (Model.ReminderFormat == null) ? "" : Model.ReminderFormat;
                    mobj2.RTGS = (Model.RTGS == null) ? "" : Model.RTGS;
                    mobj2.TDSCode = (Model.TDSCode == null) ? 0 : Model.TDSCode;
                    mobj2.Transporter = (Model.Transporter == null) ? "" : Model.Transporter;
                    mobj2.xBranch = mbranchcode;
                    mobj2.ENTEREDBY = muserid;
                    mobj2.LastSent = DateTime.Now;
                    mobj2.ItemType = (Model.ItemType == null) ? "" : Model.ItemType;
                    mobj2.LocationCode = 100001;
                    mobj2.LASTUPDATEDATE = DateTime.Now;
                    mobj2.GSTType = (Model.OptionGstType == null) ? 0 : Convert.ToInt32(Model.OptionGstType);
                    mobj2.GSTFlag = Model.GstApplicable;
                    mobj2.ODLImit = Model.ODLimit;
                    mobj2.DrAcNo = (Model.AcCode == null) ? "" : Model.AcCode;
                    mobj2.SMSPartyAlert = Model.SMSPartyAlert;
                    mobj2.PriceDiscList = Model.PDiscList;
                    mobj2.SchemeList = Model.SchemeList;
                    ctxTFAT.MasterInfo.Add(mobj2);

                    HoldTransactions mobj4 = new HoldTransactions();
                    mobj4.AUTHIDS = muserid;
                    mobj4.AUTHORISE = "A00";
                    mobj4.CheckCRDays = Model.CheckCRDays;
                    mobj4.CheckCRLimit = Model.CheckCRLimit;
                    mobj4.ChkTempCRDays = false;
                    mobj4.ChkTempCRLimit = false;
                    mobj4.Code = Model.Code;
                    mobj4.CRDaysWarn = (Model.CRDaysWarn == 0) ? false : true;
                    mobj4.CRLimitWarn = (Model.CRLimitWarn == 0) ? false : true;
                    mobj4.CRLimitWithTrx = Model.CRLimitWithTrx;
                    mobj4.CRLimitWithPO = Model.CRLimitWithPO;
                    mobj4.CrPeriod = Convert.ToInt32(Model.CRPeriod);
                    mobj4.ENTEREDBY = muserid;
                    mobj4.HoldDespatch = Model.HoldDespatch;
                    mobj4.HoldDespatchDt1 = (Model.StrHoldDespatchDt == null || Model.StrHoldDespatchDt == "01-01-0001" || Model.StrHoldDespatchDt == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrHoldDespatchDt);
                    mobj4.HoldDespatchDt2 = DateTime.Now;
                    mobj4.HoldEnquiry = Model.HoldEnquiry;
                    mobj4.HoldEnquiryDt1 = (Model.StrHoldEnquiryDt == null || Model.StrHoldEnquiryDt == "01-01-0001" || Model.StrHoldEnquiryDt == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrHoldEnquiryDt);
                    mobj4.HoldEnquiryDt2 = DateTime.Now;
                    mobj4.HoldInvoice = Model.HoldInvoice;
                    mobj4.HoldInvoiceDt1 = (Model.StrHoldInvoiceDt == null || Model.StrHoldInvoiceDt == "01-01-0001" || Model.StrHoldInvoiceDt == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrHoldInvoiceDt);
                    mobj4.DocDate = DateTime.Now;
                    mobj4.HoldInvoiceDt2 = DateTime.Now;
                    mobj4.HoldNarr = Model.HoldNarr;
                    mobj4.HoldOrder = false;
                    mobj4.HoldOrderDt1 = DateTime.Now;
                    mobj4.HoldOrderDt2 = DateTime.Now;
                    mobj4.HoldPayment = false;
                    mobj4.HoldQuote = false;
                    mobj4.HoldQuoteDt1 = DateTime.Now;
                    mobj4.HoldQuoteDt2 = DateTime.Now; ;
                    mobj4.LASTUPDATEDATE = DateTime.Now;
                    mobj4.TempCrDayDt1 = DateTime.Now;
                    mobj4.TempCrDayDt2 = DateTime.Now;
                    mobj4.TempCrLimit = 0;
                    mobj4.TempCrLimitDt1 = DateTime.Now;
                    mobj4.TempCrLimitDt2 = DateTime.Now;
                    mobj4.TempCrPeriod = 0;
                    mobj4.TempRemark = "";
                    mobj4.Ticklers = Model.Ticklers;
                    ctxTFAT.HoldTransactions.Add(mobj4);

                    TaxDetails mobj3 = new TaxDetails();
                    mobj3.AUTHIDS = muserid;
                    mobj3.AUTHORISE = "A00";
                    mobj3.Code = Model.Code;
                    mobj3.CutTCS = Model.CutTCS;
                    mobj3.CutTDS = Model.CutTDS;
                    mobj3.Deductee = "";
                    mobj3.DifferRate = Model.DifferRate;
                    mobj3.DifferRateCertNo = (Model.DifferRateCertNo == null) ? "" : Model.DifferRateCertNo;
                    mobj3.ENTEREDBY = muserid;
                    mobj3.Form15HCITDate = (Model.StrForm15HCITDate == null || Model.StrForm15HCITDate == "01-01-0001" || Model.StrForm15HCITDate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrForm15HCITDate);
                    mobj3.Form15HDate = (Model.StrForm15HDate == null || Model.StrForm15HDate == "01-01-0001" || Model.StrForm15HDate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrForm15HDate);
                    mobj3.IsDifferRate = Model.IsDifferRate;
                    mobj3.IsForm15H = Model.IsForm15H;
                    mobj3.LASTUPDATEDATE = DateTime.Now;
                    mobj3.LocationCode = 100001;

                    mobj3.TDSCode = (Model.TDSCode == null) ? 0 : Convert.ToInt32(Model.TDSCode);
                    ctxTFAT.TaxDetails.Add(mobj3);

                    if (Session["MailInfo"] != null)
                    {
                        var mailinformation = (List<MasterVM>)Session["MailInfo"];
                        if (mailinformation.Count == 1)
                        {
                            Address mobj1 = new Address();
                            mobj1.AddOrContact = (Model.ContactType == null || Model.ContactType.Trim() == "") ? 0 : Convert.ToInt32(Model.ContactType);
                            mobj1.Adrl1 = (Model.Adrl1 == null) ? "" : Model.Adrl1;
                            mobj1.Adrl2 = (Model.Adrl2 == null) ? "" : Model.Adrl2;
                            mobj1.Adrl3 = (Model.Adrl3 == null) ? "" : Model.Adrl3;
                            mobj1.Adrl4 = "";
                            mobj1.AnnDate = (Model.StrAnndate == null || Model.StrAnndate == "01-01-0001" || Model.StrAnndate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrAnndate);
                            mobj1.Area = Model.Area;
                            mobj1.AssistEmail = (Model.AssistEmail == null) ? "" : Model.AssistEmail;
                            mobj1.AssistMobile = (Model.AssistMobile == null) ? "" : Model.AssistMobile;
                            mobj1.AssistName = (Model.AssistName == null) ? "" : Model.AssistName;
                            mobj1.AssistTel = (Model.AssistTel == null) ? "" : Model.AssistTel;
                            mobj1.AUTHIDS = muserid;
                            mobj1.AUTHORISE = "A00";
                            mobj1.BaseGr = (Model.AcType == null) ? "" : Model.AcType;
                            mobj1.BDate = (Model.Budate == null || Model.Budate == "01-01-0001" || Model.Budate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.Budate);
                            mobj1.ChildBdate = (Model.StrChildBdate == null || Model.StrChildBdate == "01-01-0001" || Model.StrChildBdate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.StrChildBdate);
                            mobj1.ChildName = (Model.ChildName == null) ? "" : Model.ChildName;
                            mobj1.City = (Model.City == null) ? "" : Model.City;
                            mobj1.Code = Model.Code;
                            mobj1.CorpID = (Model.CorpID == null) ? "" : Model.CorpID;
                            mobj1.CorrespondenceType = Model.CorrespondenceType;
                            mobj1.Country = (Model.Country == null) ? "" : Model.Country;
                            mobj1.Dept = Model.Dept;
                            mobj1.Designation = Model.Designation;
                            mobj1.Division = Model.Division;
                            mobj1.DraweeBank = Model.DraweeBank;
                            mobj1.Email = (Model.Email == null) ? "" : Model.Email;
                            mobj1.ENTEREDBY = muserid;
                            mobj1.Fax = (Model.Fax == null) ? "" : Model.Fax;
                            mobj1.Language = Model.Language;
                            mobj1.LASTUPDATEDATE = DateTime.Now;
                            mobj1.Licence1 = (Model.Licence1 == null) ? "" : Model.Licence1;
                            mobj1.Licence2 = (Model.Licence2 == null) ? "" : Model.Licence2;
                            mobj1.LocationCode = mlocationcode;
                            mobj1.MailingCategory = Model.MailingCategory;
                            mobj1.Mobile = (Model.Mobile == null) ? "" : Model.Mobile;
                            mobj1.Name = (Model.AName == null) ? "" : Model.AName;
                            mobj1.PanNo = (Model.PanNo == null) ? "" : Model.PanNo;
                            mobj1.Password = (Model.Password == null) ? "" : Model.Password;
                            mobj1.Person = (Model.Person == null) ? "" : Model.Person;
                            mobj1.PhotoPath = "";
                            mobj1.Pin = (Model.Pin == null) ? "" : Model.Pin;
                            mobj1.PTaxCode = (Model.PTaxCode == null) ? "" : Model.PTaxCode;
                            mobj1.STaxCode = (Model.STaxCode == null) ? "" : Model.STaxCode;
                            mobj1.Religion = Model.Religion;
                            mobj1.Segment = (Model.Segment == null) ? "" : Model.Segment;
                            mobj1.Sno = Convert.ToInt32(Model.SrNo);
                            mobj1.Source = Model.Source;
                            mobj1.SpouseBdate = (Model.SpouseBudate == null || Model.SpouseBudate == "01-01-0001" || Model.SpouseBudate == "") ? Convert.ToDateTime("1900-01-01") : ConvertDDMMYYTOYYMMDD(Model.SpouseBudate);
                            mobj1.SpouseName = (Model.SpouseName == null) ? "" : Model.SpouseName;
                            mobj1.State = (Model.State == null) ? "" : Model.State;
                            mobj1.Tel1 = (Model.Tel1 == null) ? "" : Model.Tel1;
                            mobj1.Tel2 = (Model.Tel2 == null) ? "" : Model.Tel2;
                            mobj1.Tel3 = (Model.Tel3 == null) ? "" : Model.Tel3;
                            mobj1.Tel4 = "";
                            mobj1.TINNo = (Model.TINNo == null) ? "" : Model.TINNo;
                            mobj1.UserID = (Model.UserID == null) ? "" : Model.UserID;
                            mobj1.www = (Model.www == null) ? "" : Model.www;
                            mobj1.AadharNo = (Model.AadharNo == null) ? "" : Model.AadharNo;
                            mobj1.GSTNo = (Model.GSTNo == null) ? "" : Model.GSTNo;
                            mobj1.IGSTRate = Model.DefaultIGst;
                            mobj1.CGSTRate = Model.DefaultCGst;
                            mobj1.SGSTRate = Model.DefaultSGst;
                            mobj1.GSTType = (Model.GSTType == null || Model.GSTType.Trim() == "") ? 0 : Convert.ToInt32(Model.GSTType);
                            mobj1.DealerType = (Model.DealerType == null || Model.DealerType.Trim() == "") ? (byte)0 : Convert.ToByte(Model.DealerType);
                            mobj1.VATReg = (Model.VATReg == null) ? "" : Model.VATReg;
                            mobj1.ReraRegNo = (Model.ReraNo == null) ? "" : Model.ReraNo;
                            ctxTFAT.Address.Add(mobj1);
                        }
                        else
                        {
                            foreach (var item in mailinformation)
                            {
                                Address mobj1 = new Address();
                                mobj1.AddOrContact = (item.ContactType == null || item.ContactType.Trim() == "") ? 0 : Convert.ToInt32(item.ContactType);
                                mobj1.Adrl1 = (item.Adrl1 == null) ? "" : item.Adrl1;
                                mobj1.Adrl2 = (item.Adrl2 == null) ? "" : item.Adrl2;
                                mobj1.Adrl3 = (item.Adrl3 == null) ? "" : item.Adrl3;
                                mobj1.Adrl4 = "";
                                mobj1.AnnDate = (item.Anndate == null) ? DateTime.Now : item.Anndate;
                                mobj1.Area = item.Area;
                                mobj1.AssistEmail = (item.AssistEmail == null) ? "" : item.AssistEmail;
                                mobj1.AssistMobile = (item.AssistMobile == null) ? "" : item.AssistMobile;
                                mobj1.AssistName = (item.AssistName == null) ? "" : item.AssistName;
                                mobj1.AssistTel = (item.AssistTel == null) ? "" : item.AssistTel;
                                mobj1.AUTHIDS = muserid;
                                mobj1.AUTHORISE = "A00";
                                mobj1.BaseGr = (Model.AcType == null) ? "" : Model.AcType;
                                mobj1.BDate = (item.Bdate == null) ? DateTime.Now : item.Bdate;
                                mobj1.ChildBdate = (item.ChildBdate == null) ? DateTime.Now : item.ChildBdate;
                                mobj1.ChildName = (item.ChildName == null) ? "" : item.ChildName;
                                mobj1.City = (item.City == null) ? "" : item.City;
                                mobj1.Code = Model.Code;
                                mobj1.CorpID = (item.CorpID == null) ? "" : item.CorpID;
                                mobj1.CorrespondenceType = item.CorrespondenceType;
                                mobj1.Country = (item.Country == null) ? "" : item.Country;
                                mobj1.Dept = item.Dept;
                                mobj1.Designation = item.Designation;
                                mobj1.Division = item.Division;
                                mobj1.DraweeBank = Model.DraweeBank;
                                mobj1.Email = (item.Email == null) ? "" : item.Email;
                                mobj1.ENTEREDBY = muserid;
                                mobj1.Fax = (item.Fax == null) ? "" : item.Fax;
                                mobj1.Language = item.Language;
                                mobj1.LASTUPDATEDATE = DateTime.Now;
                                mobj1.Licence1 = (item.Licence1 == null) ? "" : item.Licence1;
                                mobj1.Licence2 = (item.Licence2 == null) ? "" : item.Licence2;
                                mobj1.LocationCode = mlocationcode;
                                mobj1.MailingCategory = item.MailingCategory;
                                mobj1.Mobile = (item.Mobile == null) ? "" : item.Mobile;
                                mobj1.Name = (item.AName == null) ? "" : item.AName;
                                mobj1.PanNo = (item.PanNo == null) ? "" : item.PanNo;
                                mobj1.Password = (item.Password == null) ? "" : item.Password;
                                mobj1.Person = (item.Person == null) ? "" : item.Person;
                                mobj1.PhotoPath = "";
                                mobj1.Pin = (item.Pin == null) ? "" : item.Pin;
                                mobj1.PTaxCode = (item.PTaxCode == null) ? "" : item.PTaxCode;
                                mobj1.STaxCode = (item.STaxCode == null) ? "" : item.STaxCode;
                                mobj1.Religion = item.Religion;
                                mobj1.Segment = (item.Segment == null) ? "" : item.Segment;
                                mobj1.Sno = Convert.ToInt32(item.SrNo);
                                mobj1.Source = item.Source;
                                mobj1.SpouseBdate = (item.SpouseBdate == null) ? DateTime.Now : item.SpouseBdate;
                                mobj1.SpouseName = (item.SpouseName == null) ? "" : item.SpouseName;
                                mobj1.State = (item.State == null) ? "" : item.State;
                                mobj1.Tel1 = (item.Tel1 == null) ? "" : item.Tel1;
                                mobj1.Tel2 = (item.Tel2 == null) ? "" : item.Tel2;
                                mobj1.Tel3 = (item.Tel3 == null) ? "" : item.Tel3;
                                mobj1.Tel4 = "";
                                mobj1.TINNo = (item.TINNo == null) ? "" : item.TINNo;
                                mobj1.UserID = (item.UserID == null) ? "" : item.UserID;
                                mobj1.www = (item.www == null) ? "" : item.www;
                                mobj1.AadharNo = (item.AadharNo == null) ? "" : item.AadharNo;
                                mobj1.GSTNo = (item.GSTNo == null) ? "" : item.GSTNo;
                                mobj1.IGSTRate = item.DefaultIGst;
                                mobj1.CGSTRate = item.DefaultCGst;
                                mobj1.SGSTRate = item.DefaultSGst;
                                mobj1.GSTType = (item.GSTType == null || item.GSTType.Trim() == "") ? 0 : Convert.ToInt32(item.GSTType);
                                mobj1.DealerType = (item.DealerType == null || item.DealerType.Trim() == "") ? (byte)0 : Convert.ToByte(item.DealerType);
                                mobj1.VATReg = (item.VATReg == null) ? "" : item.VATReg;
                                mobj1.ReraRegNo = (item.ReraNo == null) ? "" : item.ReraNo;
                                ctxTFAT.Address.Add(mobj1);
                            }
                        }
                    }

                    if (Session["FixedAssets"] != null)
                    {
                        var FixedAssets = (List<MasterVM>)Session["FixedAssets"];
                        if (FixedAssets.Count != 0)
                        {
                            foreach (var item in FixedAssets)
                            {
                                Assets mobj1 = new Assets();
                                mobj1.AUTHORISE = "A00";
                                mobj1.Code = Model.Code;
                                mobj1.Branch = mbranchcode;
                                mobj1.AUTHIDS = muserid;
                                mobj1.Store = 100001;
                                mobj1.LocationCode = mlocationcode;
                                mobj1.AcDep = "";
                                mobj1.Method = item.Method;
                                mobj1.Rate = 1;
                                mobj1.AcCode = item.AcCode;
                                mobj1.BookValue = item.BookValue;
                                mobj1.CostPrice = item.CostPrice;
                                mobj1.PurchDate = item.PurchDate;
                                mobj1.UseDate = item.UseDate;
                                mobj1.LASTUPDATEDATE = DateTime.Now;
                                mobj1.ENTEREDBY = muserid;
                                ctxTFAT.Assets.Add(mobj1);
                            }
                        }
                    }

                    ctxTFAT.SaveChanges();
                    //UpdateAuditTrail(mbranchcode, Model.Mode, Model.Header, "", DateTime.Now, 0, mobj.Code, "", "DM");

                    //int n = ctxTFAT.Database.ExecuteSqlCommand("Update Master Set GroupTree = dbo.fn_GetGroupTree(Grp)");
                    transaction.Commit();
                    transaction.Dispose();
                    Session["TempAccMasterAttach"] = null;
                    Session["MailInfo"] = null;
                }
                catch (DbEntityValidationException ex1)
                {
                    transaction.Rollback();

                    return Json(new { Message = ex1.EntityValidationErrors.Select(x => x.ValidationErrors.Select(xa => xa.ErrorMessage).FirstOrDefault()), Status = "Error" }, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    transaction.Rollback();
                    return Json(new { Message = ex.InnerException.InnerException.Message, Status = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = "Success", Code = Model.Code, Name = Model.Name }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DriverNameCheck(string DriverName, string Document)
        {
            var DriverMaster = ctxTFAT.DriverMaster.Where(x => x.Name.ToString().Trim().ToLower() == DriverName.Trim().ToString().ToLower() && x.Code != Document).FirstOrDefault();
            if (DriverMaster != null)
            {
                return Json(new
                {
                    Message = "Driver Name Exist Please Change The Name.",
                    Status = "Error"
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Status = "Success" }, JsonRequestBehavior.AllowGet);


        }


    }
}