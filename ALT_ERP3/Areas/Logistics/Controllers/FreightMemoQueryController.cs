using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using Common;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class FreightMemoQueryController : BaseController
    {
        private static string mauthorise = "A00";
        private static string mdocument = "";
        private int mnewrecordkey = 0;
        private new string mbranchcode = System.Web.HttpContext.Current.Session["BranchCode"].ToString();
        private new string mcompcode = System.Web.HttpContext.Current.Session["CompCode"].ToString();
        public new string muserid = System.Web.HttpContext.Current.Session["UserId"].ToString();
        private static string msubcodeof = "";
        private static string moptioncode = "";
        private static string mmodule = "";
        //ITransactionGridOperation mIlst = new TransactionGridOperation();
        private static string mbasegr = "";

        // GET: Logistics/FreightMemoQuery
        public ActionResult Index(FreightMemoQueryVM Model)
        {
            UpdateAuditTrail(mbranchcode, "Report", Model.Header, "", DateTime.Now, 0, "", "", "NA");
            if (Model.Document == "" || Model.Document == null)
            {
                GetAllMenu(Session["ModuleName"].ToString());

            }
            if (Model.Shortcut)
            {
                Model.FMno = ctxTFAT.FMMaster.Where(x => x.TableKey == Model.FreightMemoKey.Trim()).Select(x => x.FmNo.ToString()).FirstOrDefault();
            }
            ViewBag.ViewDataId = Model.ViewDataId;
            msubcodeof = Model.ViewDataId;
            mbasegr = Model.MainType;
            ViewBag.Header = Model.Header;
            ViewBag.Controller = Model.Controller;
            ViewBag.Controller2 = Model.Controller2;
            ViewBag.OptionType = Model.OptionType;
            ViewBag.OptionCode = Model.OptionCode;
            ViewBag.Module = Model.Module;

            return View(Model);
        }

        [HttpPost]
        public ActionResult FetchFreightMemoDocumentList(FreightMemoQueryVM Model)
        {
            List<FreightMemoQueryVM> ValueList = new List<FreightMemoQueryVM>();
            var ledgerlist = ctxTFAT.Ledger.Where(x => x.Srl.Trim() == Model.FMno.Trim() && x.Type == "FM000" && x.Sno == 1).ToList();
            var mlrmaster = ctxTFAT.FMMaster.Where(x => x.FmNo.ToString() == Model.FMno).ToList();

            List<string> TotalDocumentFount = new List<string>();
            TotalDocumentFount.AddRange(mlrmaster.Select(x => x.TableKey));
            foreach (var item in ledgerlist)
            {
                if (TotalDocumentFount.Where(x => x == item.TableKey).FirstOrDefault() == null)
                {
                    TotalDocumentFount.Add(item.TableKey);
                }
            }

            if (TotalDocumentFount == null)
            {
                Model.Status = "ValidError";
                Model.Message = "Freight Memo Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            if (TotalDocumentFount.Count() == 0)
            {
                Model.Status = "ValidError";
                Model.Message = "Freight Memo Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            else if (TotalDocumentFount.Count() == 1)
            {
                Model.Status = "Processed";
                if (mlrmaster.Count() == 0)
                {
                    Model.Message = ledgerlist.Select(x => x.TableKey).FirstOrDefault();
                    Model.FindOpening = true;
                }
                else
                {
                    Model.Message = mlrmaster.Select(x => x.TableKey).FirstOrDefault();
                    Model.FindOpening = false;
                }

                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            foreach (var item in mlrmaster)
            {
                FreightMemoQueryVM otherTransact = new FreightMemoQueryVM();
                otherTransact.FMBranch = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault();
                otherTransact.Broker = ctxTFAT.Master.Where(x => x.Code == item.BroCode).Select(x => x.Name).FirstOrDefault();
                otherTransact.Freight = item.Freight.ToString("0.00");
                otherTransact.Advance = item.Adv.ToString("0.00");
                otherTransact.Balance = item.Balance.ToString("0.00");
                otherTransact.FMno = item.FmNo.ToString();
                otherTransact.FreightMemoKey = item.TableKey.ToString();
                otherTransact.FMDate = item.Date;
                otherTransact.VehicleType = ctxTFAT.VehicleCategory.Where(x => x.Code == item.VehicleCategory).Select(x => x.VehicleCategory1).FirstOrDefault();
                otherTransact.FFrom = ctxTFAT.TfatBranch.Where(x => x.Code == item.FromBranch).Select(x => x.Name).FirstOrDefault();
                otherTransact.FTo = ctxTFAT.TfatBranch.Where(x => x.Code == item.ToBranch).Select(x => x.Name).FirstOrDefault();
                otherTransact.Driver = ctxTFAT.DriverMaster.Where(x => x.Code == item.Driver).Select(x => x.Name).FirstOrDefault();
                otherTransact.VehicleNo = item.VehicleStatus == "100001" ? ctxTFAT.HireVehicleMaster.Where(x => x.Code == item.TruckNo).Select(x => x.TruckNo).FirstOrDefault() : ctxTFAT.VehicleMaster.Where(x => x.Code == item.TruckNo).Select(x => x.TruckNo).FirstOrDefault();
                otherTransact.FindOpening = false;
                ValueList.Add(otherTransact);
            }
            foreach (var item in ledgerlist)
            {
                if (ValueList.Where(x => x.FreightMemoKey == item.TableKey).FirstOrDefault() == null)
                {
                    FreightMemoQueryVM otherTransact = new FreightMemoQueryVM();
                    otherTransact.FMBranch = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault();
                    otherTransact.Broker = ctxTFAT.Master.Where(x => x.Code == item.Code).Select(x => x.Name).FirstOrDefault();
                    otherTransact.Freight = item.Credit.Value.ToString("0.00");
                    otherTransact.Advance = item.Credit.Value.ToString("0.00");
                    otherTransact.Balance = 0.ToString("0.00");
                    otherTransact.FMno = item.Srl.ToString();
                    otherTransact.FreightMemoKey = item.TableKey.ToString();
                    otherTransact.FMDate = item.DocDate;
                    otherTransact.FindOpening = true;
                    ValueList.Add(otherTransact);
                }
            }

            Model.ValueList = ValueList;
            var html = ViewHelper.RenderPartialView(this, "FreightMemoList", Model);
            return Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetReport(FreightMemoQueryVM Model)
        {
            int docdetail = 0, docdetail1 = 0;
            string TrackID = "";
            bool CurrentBranch = false;

            #region USer Rights

            if (muserid == "Super")
            {
                Model.FMD = true;
                Model.DS = true;
                Model.PD = true;
                Model.ED = true;
            }
            else
            {
                var FmQueryAloow = ctxTFAT.TfatPass.Where(x => x.Code == muserid).Select(x => new { x.FreightMemoQryBranch, x.FreightMemoQryPanel }).FirstOrDefault();
                if (String.IsNullOrEmpty(FmQueryAloow.FreightMemoQryBranch))
                {
                    CurrentBranch = true;
                }
                else
                {
                    if (FmQueryAloow.FreightMemoQryBranch == "TR")
                    {
                        CurrentBranch = true;
                    }
                    else
                    {
                        CurrentBranch = false;
                    }
                }

                if (String.IsNullOrEmpty(FmQueryAloow.FreightMemoQryPanel))
                {
                    Model.FMD = false;
                    Model.DS = false;
                    Model.PD = false;
                    Model.ED = false;
                }
                else
                {
                    var List = FmQueryAloow.FreightMemoQryPanel.Split('^').ToList();
                    if (List.Contains("FMD"))
                    {
                        Model.FMD = true;
                    }
                    if (List.Contains("DS"))
                    {
                        Model.DS = true;
                    }
                    if (List.Contains("PD"))
                    {
                        Model.PD = true;
                    }
                    if (List.Contains("ED"))
                    {
                        Model.ED = true;
                    }
                }
            }

            #endregion

            Session["TempAttach"] = null;
            Session["CommnNarrlist"] = null;
            string Status = "Success";
            List<FMROUTETable> FMROUTETables = ctxTFAT.FMROUTETable.Where(x => x.Parentkey == Model.FreightMemoKey).OrderBy(x => x.SubRoute).ToList();
            List<RouteDetails> routeDetails = new List<RouteDetails>();
            foreach (var item in FMROUTETables)
            {
                RouteDetails routeDetail = new RouteDetails();
                routeDetail.Branch = ctxTFAT.TfatBranch.Where(x => x.Code == item.RouteVia).Select(x => x.Name).FirstOrDefault();
                routeDetail.ArrivalSchDate = item.ArrivalSchDate == null ? "" : item.ArrivalSchDate.Value.ToShortDateString();
                routeDetail.ArrivalSchTime = item.ArrivalSchTime == null ? "" : item.ArrivalSchTime;
                routeDetail.ArrivalSchKm = item.ArrivalSchKm == null ? "" : item.ArrivalSchKm.Value.ToString();
                routeDetail.ArrivalReSchDate = item.ArrivalReSchDate == null ? "" : item.ArrivalReSchDate.Value.ToShortDateString();
                routeDetail.ArrivalReSchTime = item.ArrivalReSchTime == null ? "" : item.ArrivalReSchTime;
                routeDetail.ArrivalReSchKm = item.ArrivalReSchKm == null ? "" : item.ArrivalReSchKm.Value.ToString();
                routeDetail.ArrivalDate = item.ArrivalDate == null ? "" : item.ArrivalDate.Value.ToShortDateString();
                routeDetail.ArrivalTime = item.ArrivalTime == null ? "" : item.ArrivalTime;
                routeDetail.ArrivalKM = item.ArrivalKM == null ? "" : item.ArrivalKM.Value.ToString();
                routeDetail.ArrivalLateTime = "0";
                routeDetail.DispatchLateTime = "0";

                if ((!String.IsNullOrEmpty(routeDetail.ArrivalSchDate)) && (!String.IsNullOrEmpty(routeDetail.ArrivalDate)))
                {
                    var Date = routeDetail.ArrivalSchDate.Split('/');
                    var Date1 = routeDetail.ArrivalSchTime.Split(':');
                    DateTime arrivalScheduleDate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Date1[0]), Convert.ToInt32(Date1[1]), 00);

                    Date = routeDetail.ArrivalDate.Split('/');
                    Date1 = routeDetail.ArrivalTime.Split(':');
                    DateTime arrivalDate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Date1[0]), Convert.ToInt32(Date1[1]), 00);

                    if (arrivalScheduleDate >= arrivalDate)
                    {
                        TimeSpan ts = arrivalScheduleDate - arrivalDate;
                        var TotalMinutes = ts.TotalMinutes;

                        if (TotalMinutes == 0)
                        {
                            //routeDetail.ArrivalLateTime = "Vehicle On Time. ";
                            routeDetail.ArrivalLateTime = "0 ";
                        }
                        else
                        {
                            //routeDetail.ArrivalLateTime = "Vehicle Reach Early " + Math.Round((TotalMinutes / 60)) + " Hours and " + (TotalMinutes % 60) + " Minutes.";
                            routeDetail.ArrivalLateTime = "  " + Math.Round((TotalMinutes / 60)) + " : " + (TotalMinutes % 60);
                        }

                    }
                    else
                    {
                        TimeSpan ts = arrivalDate - arrivalScheduleDate;
                        var TotalMinutes = ts.TotalMinutes;
                        //routeDetail.ArrivalLateTime = "Vehicle Reachd Late " + Math.Round((TotalMinutes / 60)) + " Hours and " + (TotalMinutes % 60) + " Minutes.";
                        routeDetail.ArrivalLateTime = " - " + Math.Round((TotalMinutes / 60)) + " : " + (TotalMinutes % 60);
                    }
                }

                routeDetail.DispatchSchDate = item.DispatchSchDate == null ? "" : item.DispatchSchDate.Value.ToShortDateString();
                routeDetail.DispatchSchTime = item.DispatchSchTime == null ? "" : item.DispatchSchTime;
                routeDetail.DispatchReSchDate = item.DispatchReSchDate == null ? "" : item.DispatchReSchDate.Value.ToShortDateString();
                routeDetail.DispatchReSchTime = item.DispatchReSchTime == null ? "" : item.DispatchReSchTime;
                routeDetail.DispatchDate = item.DispatchDate == null ? "" : item.DispatchDate.Value.ToShortDateString();
                routeDetail.DispatchTime = item.DispatchTime == null ? "" : item.DispatchTime;
                routeDetail.DispatchKM = item.DispatchKM == null ? "" : item.DispatchKM.Value.ToString();




                if ((!String.IsNullOrEmpty(routeDetail.DispatchSchDate)) && (!String.IsNullOrEmpty(routeDetail.DispatchDate)))
                {
                    var Date = routeDetail.DispatchSchDate.Split('/');
                    var Date1 = routeDetail.DispatchSchTime.Split(':');
                    DateTime dispatchScheduleDate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Date1[0]), Convert.ToInt32(Date1[1]), 00);

                    Date = routeDetail.DispatchDate.Split('/');
                    Date1 = routeDetail.DispatchTime.Split(':');
                    DateTime dispachDate = new DateTime(Convert.ToInt32(Date[2]), Convert.ToInt32(Date[1]), Convert.ToInt32(Date[0]), Convert.ToInt32(Date1[0]), Convert.ToInt32(Date1[1]), 00);

                    if (dispatchScheduleDate >= dispachDate)
                    {
                        TimeSpan ts = dispatchScheduleDate - dispachDate;
                        var TotalMinutes = ts.TotalMinutes;

                        if (TotalMinutes == 0)
                        {
                            //routeDetail.DispatchLateTime = "Vehicle Move On Time. ";
                            routeDetail.DispatchLateTime = " 0 ";
                        }
                        else
                        {
                            //routeDetail.DispatchLateTime = "Vehicle Move Early " + Math.Round((TotalMinutes / 60)) + " Hours and " + (TotalMinutes % 60) + " Minutes.";
                            routeDetail.DispatchLateTime = "  " + Math.Round((TotalMinutes / 60)) + " : " + (TotalMinutes % 60);
                        }

                    }
                    else
                    {
                        TimeSpan ts = dispachDate - dispatchScheduleDate;
                        var TotalMinutes = ts.TotalMinutes;
                        var ff = ts.TotalHours;
                        //routeDetail.DispatchLateTime = "Vehicle Move Late " + Math.Round((TotalMinutes / 60)) + " Hours and " + (TotalMinutes % 60) + " Minutes.";
                        routeDetail.DispatchLateTime = " - " + Math.Round((TotalMinutes / 60)) + " : " + (TotalMinutes % 60);
                    }
                }



                routeDetails.Add(routeDetail);
            }
            Model.ViewSchedule = routeDetails;


            if (Model.FindOpening)
            {
                string Query = " select 0 as Payload,FM.Parentkey,'' as TrackId,FM.Srl as FMno,FM.DocDate as FMDate,'' as FMTime," +
                            "(select T.Name From TfatBranch T where T.code=FM.Branch) as FMBranch,'' as FFrom," +
                            "'' as FTo,'' as Via,''as Route,FM.DocDate as FMCreateDate,FM.ENTEREDBY as FMEnterdby,FM.LASTUPDATEDATE as FMLastUpdateDate," +
                            "'' as VehicleType,'' as VehicleNo,(select M.Name From Master M where M.code = FM.Code) as Broker," +
                            "Cast(FM.Credit as Decimal(14, 2)) as Freight,Cast(FM.Credit as Decimal(14, 2)) as Advance," +
                            "Cast(FM.Debit as Decimal(14, 2)) as Balance,0 as KM,'' as Driver  ,'' as PayableAt," +
                            "FM.billnumber as Remark,0 as TotalQtyLoad " +
                            "from Ledger FM where FM.Tablekey = '" + Model.FreightMemoKey + "' and type = 'FM000' ";
                List<DataRow> ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                if (ordersstk.Count() > 0)
                {
                    if (CurrentBranch)
                    {
                        var Item = ordersstk.FirstOrDefault();
                        List<string> items = new List<string>();
                        using (SqlConnection con = new SqlConnection(GetConnectionString()))
                        {
                            string query = " select distinct Code from TfatBranch where (Charindex(Grp,'" + ctxTFAT.TfatPass.Where(x => x.Code == muserid).Select(x => x.AppBranch).FirstOrDefault() + "' )<>0) or (Charindex(Code,'" + ctxTFAT.TfatPass.Where(x => x.Code == muserid).Select(x => x.AppBranch).FirstOrDefault() + "' )<>0)";
                            using (SqlCommand cmd = new SqlCommand(query))
                            {
                                cmd.Connection = con;
                                con.Open();
                                using (SqlDataReader sdr = cmd.ExecuteReader())
                                {
                                    while (sdr.Read())
                                    {
                                        items.Add(sdr["Code"].ToString());
                                    }
                                }
                                con.Close();
                            }
                        }

                        if (items.Contains(Item["FMBranch"].ToString().Trim()))
                        {
                            Model = FreightMemoDetails(ordersstk, Model);
                            Model = TrackingReq(Model);
                        }
                        else
                        {
                            Model.NotShowDetails = true;
                        }
                    }
                    else
                    {
                        Model = FreightMemoDetails(ordersstk, Model);
                        Model = TrackingReq(Model);
                    }

                    if (Model.NotShowDetails == false)
                    {
                        var LedgerParentkey = ctxTFAT.Ledger.Where(x => x.TableKey == Model.FreightMemoKey).Select(x => x.ParentKey).FirstOrDefault();
                        Query = "select  case when VDA.InsClr='A' then 'Advance' else 'Balance' end as AB,  VDA.FMTableKey as FMTableKey,VDA.VouNo AS VouNO,VMA.VouDate AS VouNODate,VMA.VouDate AS VouNOCreateDate,(select T.Name From TfatBranch T where T.code =VMA.Branch) AS VouNOBranch ,(select M.Name From Master M where M.code =VMA.Account) AS VouNOBoker,(select M.Name From Master M where M.code =VMA.Bank) AS VouNOBank,Cast(VDA.NetAmt as Decimal(14, 2)) AS Amount,VMA.ENTEREDBY AS VouNOEnteredBy,VMA.LASTUPDATEDATE AS VouNOLastUpdateDate,VMA.Remark AS VouNORemark "
                                + " from VoucherDetail VDA  FULL OUTER JOIN VoucherMaster VMA On VMA.VouNo = VDA.VouNo and VMA.Branch = VDA.Branch  where VDA.FmNo = " + Model.FMno + " and VDA.FMtablekey in (select L.Tablekey from ledger L where L.type = 'FM000' and L.parentkey = '" + LedgerParentkey + "')  ";
                        ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                        Model.PaymentDetails = PaymentDetails(ordersstk, Model.FMno);


                        Query = "select distinct M.Name as VouNOTrnChargeName,R.srl as VouNO,R.DocDate as VouNODate,R.DocDate as VouNOCreateDate,T.Name as VouNOBranch,R.EnteredBy as VouNOEnteredBy,R.LASTUPDATEDATE as VouNOLastUpdateDate,R.Narr as VouNORemark,DOC.Name as VouNOTrnType,R.Type as VouNOTrnTypeCode,case when R.amttype='true' then Cast(LR.FmAmt as Decimal(14, 2)) else 0 end as VouNOTrnExpAmt,case when R.amttype='false' then Cast(LR.FmAmt as Decimal(14, 2))  else 0 end as VouNOTrnIncAmt"
                                + " from RelateData R left"
                                + " join RelFm LR on LR.TableKey + LR.Branch = R.TableKey + R.Branch left"
                                + " join DocTypes DOC on DOC.Code = R.Type left"
                                + " join Master M on M.code = R.code  left"
                                + " join TfatBranch T on T.Code = R.Branch where LR.FMRefTablekey = '" + Model.FreightMemoKey + "'";
                        ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                        Model.ExpensesDetails = ExpensesDetails(ordersstk);



                        #region Attachment
                        docdetail = 0;
                        docdetail1 = 0;
                        TrackID = Model.TrackId;
                        #endregion
                    }
                }
                else
                {
                    Status = "Error";
                }
            }
            else
            {
                string Query = "select FM.PayLoad as Payload,FM.Parentkey,REPLACE(FM.TruckNo,' ','') as TrackId,FM.FMno as FMno,FM.Date as FMDate,(CONVERT(VARCHAR(15),CAST(FM.Time AS TIME),100) ) as FMTime,(select T.Name From TfatBranch T where T.code=FM.Branch) as FMBranch,"
                            + " (select T.Name From TfatBranch T where T.code = FM.FromBranch) as FFrom,"
                            + " (select T.Name From TfatBranch T where T.code = FM.ToBranch) as FTo,"
                            + " FM.RouteViaName as Via,FM.SelectedRoute as Route,FM.CreateDate as FMCreateDate,FM.ENTEREDBY as FMEnterdby,"
                            + " FM.LASTUPDATEDATE as FMLastUpdateDate,"
                            + " (select V.VehicleGroupStatus from VehicleGrpStatusMas V where V.code = FM.VehicleStatus ) as VehicleType,"
                            + " isnull( (select V.TruckNo from Vehiclemaster V where V.code=FM.TruckNo),(select H.TruckNo from HireVehiclemaster H where H.code=FM.TruckNo)) as VehicleNo,(select M.Name From Master M where M.code = FM.BroCode) as Broker,"
                            + " Cast(FM.Freight as Decimal(14, 2)) as Freight,Cast(FM.Adv as Decimal(14, 2)) as Advance,"
                            + " Cast(FM.Balance as Decimal(14, 2)) as Balance,"
                            + " FM.KM as KM,"
                            + " case when FM.VehicleStatus = '100001' then FM.Driver  else (select D.Name from DriverMaster D where D.code = FM.Driver) end as Driver  ,"
                            + " (select T.Name From TfatBranch T where T.code = FM.PayAt) as PayableAt,FM.Remark as Remark,(select sum(lcd.LrQty) from LCDetail lcd where lcd.LCno in  (select value from STRING_SPLIT(  FM.lcno ,',')) ) as TotalQtyLoad"
                            + " from fmmaster FM where FM.Tablekey = '" + Model.FreightMemoKey + "'";
                List<DataRow> ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                if (ordersstk.Count() > 0)
                {

                    if (CurrentBranch)
                    {
                        FMMaster fMMaster = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == Model.FreightMemoKey).FirstOrDefault();

                        List<string> items = new List<string>();
                        using (SqlConnection con = new SqlConnection(GetConnectionString()))
                        {
                            string query = " select distinct Code from TfatBranch where (Charindex(Grp,'" + ctxTFAT.TfatPass.Where(x => x.Code == muserid).Select(x => x.AppBranch).FirstOrDefault() + "' )<>0) or (Charindex(Code,'" + ctxTFAT.TfatPass.Where(x => x.Code == muserid).Select(x => x.AppBranch).FirstOrDefault() + "' )<>0)";
                            using (SqlCommand cmd = new SqlCommand(query))
                            {
                                cmd.Connection = con;
                                con.Open();
                                using (SqlDataReader sdr = cmd.ExecuteReader())
                                {
                                    while (sdr.Read())
                                    {
                                        items.Add(sdr["Code"].ToString());
                                    }
                                }
                                con.Close();
                            }
                        }

                        if (items.Contains(fMMaster.FromBranch) || items.Contains(fMMaster.RouteVia) || items.Contains(fMMaster.ToBranch))
                        {
                            Model = FreightMemoDetails(ordersstk, Model);
                            Model = TrackingReq(Model);
                        }
                        else
                        {
                            Model.NotShowDetails = true;
                        }
                    }
                    else
                    {
                        Model = FreightMemoDetails(ordersstk, Model);
                        Model = TrackingReq(Model);
                    }


                    if (Model.NotShowDetails == false)
                    {
                        Query = "select LC.Tablekey,LC.TotalQty as TotalQtyLoad, LC.lcno as LCno,(select T.Name From TfatBranch T where T.code=LC.Branch )as LCBranch,(select T.Name From TfatBranch T where T.code=LC.FromBranch) as LCFrom,(select T.Name From TfatBranch T where T.code=LC.ToBranch) as LCTo,LC.TotalQty as LCQty,LC.Date as LCDate,(CONVERT(VARCHAR(15),CAST(LC.Time AS TIME),100) )  as LCTime,LC.CreateDate as LCCreateDate,LC.ENTEREDBY as LCEnteredBy,LC.LASTUPDATEDATE as LCLastUpdateDate,LC.Remark as LCRemark"
                            + " from LCMaster LC  where LC.FMRefTablekey = '" + Model.FreightMemoKey + "' ";
                        ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                        Model.LorryChallans = DispatchDetails(ordersstk);

                        Query = "select row_number() over(order by S.lrno) as Srno ,LC.CreateOn as LodeDate,(select T.Name From TfatBranch T where T.code=LC.Branch) as LoadingIn,(select T.Name From TfatBranch T where T.code=S.Branch) as DirectLoadFor,LC.Qty as LoadQty,LC.EnteredBy as LodedBy ,LR.LrNo,LR.BookDate,(select T.Name From TfatBranch T where T.code = LR.Branch) as Branch,(select C.LRType from LRTypeMaster C where C.Code = LR.LrType) as LrType,(select C.Name from Consigner C where C.Code = LR.RecCode) as Consignor,(select C.Name from Consigner C where C.Code = LR.SendCode) as Consignee,(select C.Name from TfatBranch C where C.Code = LR.Source) as Source,(select C.Name from TfatBranch C where C.Code = LR.Dest) as Dest,LR.TotQty,(select C.Name from UnitMaster C where C.Code = LR.UnitCode) as Unit,LR.ActWt,LR.ChgWt,(select C.ChargeType from ChargeTypeMaster C where C.Code = LR.ChgType)as ChgType " +
                                "from LRStock S join LRMaster LR on S.LRRefTablekey = LR.Tablekey join LoadingConsignment LC on S.ParentKey = LC.StockKey  where S.lcno = 0 and S.type = 'TRN' and S.FMRefTablekey = '" + Model.FreightMemoKey + "' ";
                        ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                        Model.DirectLoadingDetails = DirectLoadingDetails(ordersstk);



                        if (Model.VehicleType.Trim().ToLower() == "hire")
                        {
                            var LedgerParentkey = ctxTFAT.Ledger.Where(x => x.TableKey == Model.FreightMemoKey).Select(x => x.ParentKey).FirstOrDefault();
                            //Query = "select  case when VDA.InsClr='A' then 'Advance' else 'Balance' end as AB,  VDA.FMTableKey as FMTableKey,VDA.VouNo AS VouNO,VMA.VouDate AS VouNODate,VMA.VouDate AS VouNOCreateDate,(select T.Name From TfatBranch T where T.code =VMA.Branch) AS VouNOBranch ,(select M.Name From Master M where M.code =VMA.Account) AS VouNOBoker,(select M.Name From Master M where M.code =VMA.Bank) AS VouNOBank,Cast(VDA.NetAmt as Decimal(14, 2)) AS Amount,VMA.ENTEREDBY AS VouNOEnteredBy,VMA.LASTUPDATEDATE AS VouNOLastUpdateDate,VMA.Remark AS VouNORemark "
                            //        + " from VoucherDetail VDA  FULL OUTER JOIN VoucherMaster VMA On VMA.VouNo = VDA.VouNo and VMA.Branch = VDA.Branch  where VDA.FmNo = " + Model.FMno + " and VDA.FMtablekey in (select L.Tablekey from ledger L where L.type = 'FM000' and L.parentkey = '" + LedgerParentkey + "')  ";
                            Query = "select  case when VDA.InsClr='A' then 'Advance' else 'Balance' end as AB,  VDA.FMTableKey as FMTableKey,VDA.VouNo AS VouNO,VMA.VouDate AS VouNODate,VMA.VouDate AS VouNOCreateDate,(select T.Name From TfatBranch T where T.code =VMA.Branch) AS VouNOBranch ,(select M.Name From Master M where M.code =VMA.Account) AS VouNOBoker,(select M.Name From Master M where M.code =VMA.Bank) AS VouNOBank,Cast(VDA.NetAmt as Decimal(14, 2)) AS Amount,VMA.ENTEREDBY AS VouNOEnteredBy,VMA.LASTUPDATEDATE AS VouNOLastUpdateDate,VMA.Remark AS VouNORemark "
                                    + " from VoucherDetail VDA  FULL OUTER JOIN VoucherMaster VMA On VMA.TableKey = VDA.ParentKey and VMA.Branch = VDA.Branch  where VDA.FMRefParentkey = '" + Model.FreightMemoParentkey + "'  ";

                            ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                            Model.PaymentDetails = PaymentDetails(ordersstk, Model.FMno);
                        }
                        else
                        {
                            var FMDate = (Convert.ToDateTime(Model.FMDate.ToShortDateString())).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            //Query = "select  case when VDA.InsClr='A' then 'Advance' else 'Balance' end as AB,  VDA.FMTableKey as FMTableKey,VDA.VouNo AS VouNO,VMA.VouDate AS VouNODate,VMA.VouDate AS VouNOCreateDate,(select T.Name From TfatBranch T where T.code =VMA.Branch) AS VouNOBranch ,(select M.Name From Master M where M.code =VMA.Account) AS VouNOBoker,(select M.Name From Master M where M.code =VMA.Bank) AS VouNOBank,Cast(VDA.NetAmt as Decimal(14, 2)) AS Amount,VMA.ENTEREDBY AS VouNOEnteredBy,VMA.LASTUPDATEDATE AS VouNOLastUpdateDate,VMA.Remark AS VouNORemark "
                            //        + " from VoucherDetail VDA  FULL OUTER JOIN VoucherMaster VMA On VMA.VouNo = VDA.VouNo and VMA.Branch = VDA.Branch  where VDA.FmNo = " + Model.FMno + " and VDA.FMtablekey ='"+Model.FreightMemoKey+"'  ";
                            Query = "select  case when VDA.InsClr='A' then 'Advance' else 'Balance' end as AB,  VDA.FMTableKey as FMTableKey,VDA.VouNo AS VouNO,VMA.VouDate AS VouNODate,VMA.VouDate AS VouNOCreateDate,(select T.Name From TfatBranch T where T.code =VMA.Branch) AS VouNOBranch ,(select M.Name From Master M where M.code =VMA.Account) AS VouNOBoker,(select M.Name From Master M where M.code =VMA.Bank) AS VouNOBank,Cast(VDA.NetAmt as Decimal(14, 2)) AS Amount,VMA.ENTEREDBY AS VouNOEnteredBy,VMA.LASTUPDATEDATE AS VouNOLastUpdateDate,VMA.Remark AS VouNORemark "
                                    + " from VoucherDetail VDA  FULL OUTER JOIN VoucherMaster VMA On VMA.TableKey = VDA.ParentKey and VMA.Branch = VDA.Branch  where VDA.FMRefParentkey ='" + Model.FreightMemoParentkey + "'  ";
                            ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                            Model.PaymentDetails = PaymentDetails(ordersstk, Model.FMno);
                        }

                        Query = "select distinct M.Name as VouNOTrnChargeName,R.srl as VouNO,R.DocDate as VouNODate,R.DocDate as VouNOCreateDate,T.Name as VouNOBranch,R.EnteredBy as VouNOEnteredBy,R.LASTUPDATEDATE as VouNOLastUpdateDate,R.Narr as VouNORemark,DOC.Name as VouNOTrnType,R.Type as VouNOTrnTypeCode,case when R.amttype='true' then Cast(LR.FmAmt as Decimal(14, 2)) else 0 end as VouNOTrnExpAmt,case when R.amttype='false' then Cast(LR.FmAmt as Decimal(14, 2))  else 0 end as VouNOTrnIncAmt"
                                + " from RelateData R left"
                                + " join RelFm LR on LR.TableKey + LR.Branch = R.TableKey + R.Branch left"
                                + " join DocTypes DOC on DOC.Code = R.Type left"
                                + " join Master M on M.code = R.code  left"
                                + " join TfatBranch T on T.Code = R.Branch where LR.FMRefTablekey = '" + Model.FreightMemoKey + "'";
                        ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                        Model.ExpensesDetails = ExpensesDetails(ordersstk);

                        #region Attachment
                        docdetail = ctxTFAT.Attachment.Where(x => x.ParentKey == Model.FreightMemoKey && x.Type != "Alert").ToList().Count();
                        docdetail1 = ctxTFAT.AlertNoteMaster.Where(x => x.ParentKey == Model.FreightMemoKey).ToList().Count();
                        TrackID = Model.TrackId;
                        #endregion
                    }
                }
                else
                {
                    Status = "Error";
                }
            }

            var html = ViewHelper.RenderPartialView(this, "_ReportDetails", Model);
            var jsonResult = Json(new
            {
                Status = Status,
                Html = html,
                AttachC = docdetail,
                AlertC = docdetail1,
                TrackButton = Model.TrackButtonReq,
                TrackButtonMsg = Model.TrackErrorMsg,
                TrackID = TrackID,

            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public FreightMemoQueryVM FreightMemoDetails(List<DataRow> ordersstk, FreightMemoQueryVM objitemlist)
        {
            foreach (var item in ordersstk)
            {
                objitemlist.FreightMemoParentkey = item["Parentkey"].ToString();
                objitemlist.FMno = item["FMno"].ToString();
                objitemlist.FMBranch = item["FMBranch"].ToString().Trim();
                objitemlist.FMDate = ConvertDDMMYYTOYYMMDD(item["FMDate"].ToString());
                objitemlist.FMTime = item["FMTime"].ToString();
                objitemlist.FMCreateDate = ConvertDDMMYYTOYYMMDD(item["FMCreateDate"].ToString());
                objitemlist.FMEnterdby = item["FMEnterdby"].ToString();
                objitemlist.FMLastUpdateDate = ConvertDDMMYYTOYYMMDD(item["FMLastUpdateDate"].ToString());
                objitemlist.VehicleType = item["VehicleType"].ToString().Trim();
                objitemlist.VehicleNo = item["VehicleNo"].ToString().Trim();
                objitemlist.Broker = item["Broker"].ToString();
                objitemlist.KM = item["KM"].ToString();
                objitemlist.FFrom = item["FFrom"].ToString().Trim();
                objitemlist.Via = item["Via"].ToString().Trim();
                objitemlist.Route = item["Route"].ToString().Trim();
                objitemlist.FTo = item["FTo"].ToString().Trim();
                objitemlist.Driver = item["Driver"].ToString().Trim();
                objitemlist.Freight = item["Freight"].ToString().Trim();
                objitemlist.Advance = item["Advance"].ToString().Trim();
                objitemlist.Balance = item["Balance"].ToString().Trim();
                objitemlist.PayableAt = item["PayableAt"].ToString();
                objitemlist.Remark = item["Remark"].ToString();
                objitemlist.TotalQtyLoad = item["TotalQtyLoad"].ToString();
                objitemlist.TrackId = item["TrackId"].ToString();
                objitemlist.Payload = item["Payload"].ToString();
                //objitemlist.TotalQtyWeight = item["TotalQtyWeight"].ToString();
            }

            return objitemlist;
        }

        public List<FMRelatedDispatchDetailsVM> DispatchDetails(List<DataRow> ordersstk)
        {
            List<FMRelatedDispatchDetailsVM> objitemlist = new List<FMRelatedDispatchDetailsVM>();
            foreach (var item in ordersstk)
            {
                int i = 1;
                List<FMRelatedLorryReceiptDetailsVM> lorryReceipts = new List<FMRelatedLorryReceiptDetailsVM>();
                var LCTablekey = item["Tablekey"].ToString();
                var Lrlist = ctxTFAT.LCDetail.Where(x => x.LCRefTablekey.ToString() == LCTablekey).ToList();
                foreach (var lCDetail in Lrlist)
                {
                    FMRelatedLorryReceiptDetailsVM fM = new FMRelatedLorryReceiptDetailsVM();
                    fM.Serial = i;
                    fM.Lrno = lCDetail.LRno.ToString();
                    fM.BookDate = ctxTFAT.LRMaster.Where(x => x.TableKey == lCDetail.LRRefTablekey).Select(x => x.BookDate).FirstOrDefault();
                    fM.LRBranch = ctxTFAT.TfatBranch.Where(x => x.Code == lCDetail.FromBranch).Select(x => x.Name).FirstOrDefault();
                    fM.LRType = ctxTFAT.LRTypeMaster.Where(x => x.Code == lCDetail.LrType).Select(x => x.LRType).FirstOrDefault();
                    fM.Consignor = ctxTFAT.Consigner.Where(x => x.Code == lCDetail.Consignor).Select(x => x.Name).FirstOrDefault();
                    fM.Consignee = ctxTFAT.Consigner.Where(x => x.Code == lCDetail.Consignee).Select(x => x.Name).FirstOrDefault();
                    fM.LRFrom = ctxTFAT.TfatBranch.Where(x => x.Code == lCDetail.FromBranch).Select(x => x.Name).FirstOrDefault();
                    fM.LRTo = ctxTFAT.TfatBranch.Where(x => x.Code == lCDetail.ToBranch).Select(x => x.Name).FirstOrDefault();
                    fM.LRQty = lCDetail.LrQty.ToString();
                    fM.Unit = ctxTFAT.UnitMaster.Where(x => x.Code == lCDetail.Unit).Select(x => x.Name).FirstOrDefault();
                    fM.ActWt = lCDetail.LRActWeight.ToString();
                    fM.ChrgWt = lCDetail.ChrWeight.ToString();
                    fM.ChrgType = ctxTFAT.ChargeTypeMaster.Where(x => x.Code == lCDetail.ChrgeType).Select(x => x.ChargeType).FirstOrDefault();
                    lorryReceipts.Add(fM);
                    i = i + 1;
                }
                objitemlist.Add(new FMRelatedDispatchDetailsVM()
                {
                    TotalQtyLoad = item["TotalQtyLoad"].ToString(),
                    TotalQtyWeight = Lrlist.Sum(x => x.LRActWeight).ToString("F2"),
                    LCno = item["LCno"].ToString(),
                    LCBranch = item["LCBranch"].ToString().Trim(),
                    LCFrom = item["LCFrom"].ToString().Trim(),
                    LCTo = item["LCTo"].ToString().Trim(),
                    LCQty = item["LCQty"].ToString(),
                    LCDate = ConvertDDMMYYTOYYMMDD(item["LCDate"].ToString()),
                    LCTime = item["LCTime"].ToString(),
                    LCCreateDate = ConvertDDMMYYTOYYMMDD(item["LCCreateDate"].ToString()),
                    LCEnteredBy = item["LCEnteredBy"].ToString(),
                    LCRemark = item["LCRemark"].ToString(),
                    LCLastUpdateDate = ConvertDDMMYYTOYYMMDD(item["LCLastUpdateDate"].ToString()),
                    LorryReceipts = lorryReceipts
                });
            }

            return objitemlist;
        }

        public List<FMRelatedDirectLorryReceiptDetailsVM> DirectLoadingDetails(List<DataRow> ordersstk)
        {
            List<FMRelatedDirectLorryReceiptDetailsVM> objitemlist = new List<FMRelatedDirectLorryReceiptDetailsVM>();
            foreach (var item in ordersstk)
            {
                objitemlist.Add(new FMRelatedDirectLorryReceiptDetailsVM()
                {
                    Serial = Convert.ToInt32(item["Srno"].ToString()),
                    LodeDate = Convert.ToDateTime(item["LodeDate"].ToString()),
                    LoadingIn = item["LoadingIn"].ToString(),
                    LoadingFor = item["DirectLoadFor"].ToString(),
                    LoadQty = item["LoadQty"].ToString(),
                    LodedBy = item["LodedBy"].ToString(),
                    Lrno = item["LrNo"].ToString(),
                    BookDate = ConvertDDMMYYTOYYMMDD(item["BookDate"].ToString()),
                    LRBranch = item["Branch"].ToString().Trim(),
                    LRType = item["LrType"].ToString().Trim(),
                    Consignor = item["Consignor"].ToString().Trim(),
                    Consignee = item["Consignee"].ToString().Trim(),
                    LRFrom = item["Source"].ToString().Trim(),
                    LRTo = item["Dest"].ToString().Trim(),
                    LRQty = item["TotQty"].ToString().Trim(),
                    Unit = item["Unit"].ToString().Trim(),
                    ActWt = item["ActWt"].ToString().Trim(),
                    ChrgWt = item["ChgWt"].ToString().Trim(),
                    ChrgType = item["ChgType"].ToString().Trim()
                });
            }

            return objitemlist;
        }
        public List<FMRelatedPaymentDetailsVM> PaymentDetails(List<DataRow> ordersstk, string FMNO)
        {
            List<FMRelatedPaymentDetailsVM> objitemlist = new List<FMRelatedPaymentDetailsVM>();
            foreach (var item in ordersstk)
            {
                objitemlist.Add(new FMRelatedPaymentDetailsVM()
                {
                    VouNO = item["VouNO"].ToString(),
                    VouNODate = ConvertDDMMYYTOYYMMDD(item["VouNODate"].ToString()),
                    VouNOCreateDate = ConvertDDMMYYTOYYMMDD(item["VouNOCreateDate"].ToString()),

                    VouNOBranch = item["VouNOBranch"].ToString().Trim(),
                    VouNOBoker = item["VouNOBoker"].ToString().Trim(),
                    VouNOBank = item["VouNOBank"].ToString().Trim(),
                    Amount = item["Amount"].ToString().Trim(),
                    AB = item["AB"].ToString().Trim(),

                    VouNOEnteredBy = item["VouNOEnteredBy"].ToString(),
                    VouNORemark = item["VouNORemark"].ToString(),
                    VouNOLastUpdateDate = ConvertDDMMYYTOYYMMDD(item["VouNOLastUpdateDate"].ToString()),
                    Charges = GetFMWiseCharges(item["VouNO"].ToString(), FMNO, item["FMTableKey"].ToString())
                });
            }

            return objitemlist;
        }
        public List<AdvancePayModel> GetFMWiseCharges(string VouchNo, string FMNo, string FMTableKey)
        {
            List<AdvancePayModel> objledgerdetail = new List<AdvancePayModel>();

            AdvancePayModel c1 = new AdvancePayModel();
            c1.Fld = "Amt";
            c1.Header = "Amt";
            c1.AddLess = "";
            c1.tempid = 1;
            c1.PostCode = "";
            //c.Amt = (mStrArray[a] == null) ? (decimal)0 : Convert.ToDecimal(mStrArray[a]);
            c1.Amt = (decimal)ctxTFAT.VoucherDetail.Where(x => x.FMTableKey == FMTableKey && x.FMNo == FMNo && x.VouNo == VouchNo).Select(x => x.Amount).FirstOrDefault();
            if (c1.Amt > 0)
            {
                c1.Amt = Math.Round(c1.Amt, 2);
                objledgerdetail.Add(c1);
            }

            AdvancePayModel c2 = new AdvancePayModel();
            c2.Fld = "TDS";
            c2.Header = "TDS";
            c2.AddLess = "-";
            c2.PostCode = "";
            c1.tempid = 1;
            //c.Amt = (mStrArray[a] == null) ? (decimal)0 : Convert.ToDecimal(mStrArray[a]);
            c2.Amt = (decimal)ctxTFAT.VoucherDetail.Where(x => x.FMTableKey == FMTableKey && x.FMNo == FMNo && x.VouNo == VouchNo).Select(x => x.TdsAmout).FirstOrDefault();
            if (c2.Amt > 0)
            {
                c2.Amt = Math.Round(c2.Amt, 2);
                objledgerdetail.Add(c2);
            }


            var trncharges = ctxTFAT.Charges.Where(x => x.Type == "FMP00" && x.DontUse == false).Select(x => new { x.Fld, x.Head, x.EqAmt, x.Equation, x.Code }).ToList();
            foreach (var i in trncharges)
            {
                AdvancePayModel c = new AdvancePayModel();
                c.Fld = i.Fld;
                c.Header = i.Head;
                c.AddLess = i.EqAmt;
                c.PostCode = i.Code;
                c.tempid = Convert.ToInt16(i.Fld.Substring(1, 3));
                c.Amt = GetChargeValValue(c.tempid, VouchNo, FMNo, FMTableKey);

                if (c.Amt > 0)
                {
                    c.Amt = Math.Round(c.Amt, 2);
                    objledgerdetail.Add(c);
                }
            }
            return objledgerdetail;
        }
        public decimal GetChargeValValue(int i, string VouNo, string FMNo, string Fmtablekey)
        {
            string connstring = GetConnectionString();
            decimal abc;

            var loginQuery3 = @"select Val" + i + " from VoucherDetail where VouNo = '" + VouNo + "' and FMNO = '" + FMNo + "'and FMTableKey = '" + Fmtablekey + "'";
            DataTable mDt2 = GetDataTable(loginQuery3, connstring);
            if (mDt2.Rows.Count > 0)
            {
                abc = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? 0 : Convert.ToDecimal(mDt2.Rows[0][0].ToString());
            }
            else
            {
                abc = 0;
            }

            return abc;
        }

        public List<FMRelatedExpensesDetailsVM> ExpensesDetails(List<DataRow> ordersstk)
        {
            List<FMRelatedExpensesDetailsVM> objitemlist = new List<FMRelatedExpensesDetailsVM>();
            foreach (var item in ordersstk)
            {
                objitemlist.Add(new FMRelatedExpensesDetailsVM()
                {
                    VouNO = item["VouNO"].ToString(),
                    VouNODate = ConvertDDMMYYTOYYMMDD(item["VouNODate"].ToString()),
                    VouNOCreateDate = ConvertDDMMYYTOYYMMDD(item["VouNOCreateDate"].ToString()),
                    VouNOBranch = item["VouNOBranch"].ToString(),
                    VouNOEnteredBy = item["VouNOEnteredBy"].ToString(),
                    VouNOLastUpdateDate = ConvertDDMMYYTOYYMMDD(item["VouNOLastUpdateDate"].ToString()),
                    VouNORemark = item["VouNORemark"].ToString(),
                    VouNOTrnType = item["VouNOTrnType"].ToString(),
                    VouNOTrnTypeCode = item["VouNOTrnTypeCode"].ToString(),
                    VouNOTrnIncAmt = item["VouNOTrnIncAmt"].ToString(),
                    VouNOTrnExpAmt = item["VouNOTrnExpAmt"].ToString(),
                    VouNOTrnChargeName = item["VouNOTrnChargeName"].ToString(),
                });
            }

            return objitemlist;
        }

        public ActionResult TrackID(FreightMemoQueryVM Model)
        {
            string Status = "Sucess", Latitude = "", Longitude = "", Vehicle = "";

            TfatVehicleTrackApiList tfatVehicleTrackApi = ctxTFAT.TfatVehicleTrackApiList.Where(x => x.VehicleList.Contains(Model.TrackId)).FirstOrDefault();
            var SetUrl = "";
            if (!String.IsNullOrEmpty(tfatVehicleTrackApi.TrackApi))
            {
                SetUrl += tfatVehicleTrackApi.TrackApi;
            }
            if (!String.IsNullOrEmpty(tfatVehicleTrackApi.Username))
            {
                SetUrl += tfatVehicleTrackApi.Username;
            }
            if (!String.IsNullOrEmpty(tfatVehicleTrackApi.Password))
            {
                SetUrl += tfatVehicleTrackApi.Password;
            }
            if (!String.IsNullOrEmpty(tfatVehicleTrackApi.Para1))
            {
                SetUrl += tfatVehicleTrackApi.Para1;
            }
            if (!String.IsNullOrEmpty(tfatVehicleTrackApi.Para2))
            {
                SetUrl += tfatVehicleTrackApi.Para2;
            }
            if (!String.IsNullOrEmpty(tfatVehicleTrackApi.Para3))
            {
                SetUrl += tfatVehicleTrackApi.Para3;
            }
            if (!String.IsNullOrEmpty(tfatVehicleTrackApi.Para4))
            {
                SetUrl += tfatVehicleTrackApi.Para4;
            }
            if (!String.IsNullOrEmpty(tfatVehicleTrackApi.Para5))
            {
                SetUrl += tfatVehicleTrackApi.Para5;
            }
            if (Model.TrackId.Contains("H"))
            {
                SetUrl = SetUrl.Replace("@vehiclePara", ctxTFAT.HireVehicleMaster.Where(x => x.Code == Model.TrackId).Select(x => x.TruckNo).FirstOrDefault().Replace(" ", ""));
            }
            else
            {
                SetUrl = SetUrl.Replace("@vehiclePara", ctxTFAT.VehicleMaster.Where(x => x.Code == Model.TrackId).Select(x => x.TruckNo).FirstOrDefault().Replace(" ", ""));
            }





            var GenerateUrl = "http://api.ilogistek.com:6049/tracking?AuthKey=PV7q60SurjHlacX398hbQ4SiWfYktoL&vehicle_no=" + Model.TrackId;
            WebClient client = new WebClient();
            string jsonstring = client.DownloadString(SetUrl);
            dynamic dynObj = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonstring);
            var Check = dynObj.VehicleNo;
            if (SetUrl.Contains("ilogistek"))
            {
                foreach (var item in dynObj)
                {
                    Check = item["VehicleNo"];
                }
            }
            else if (SetUrl.Contains("elixiatech"))
            {
                var status = dynObj.Status;

                if (status.Value == "0")
                {
                    Check = null;
                }
                else
                {
                    Check = "Success";
                }
            }
            if (Check == null)
            {
                Status = "Error";
            }
            else
            {
                if (SetUrl.Contains("ilogistek"))
                {
                    foreach (var item in dynObj)
                    {
                        Latitude = item["Latitude"];
                        Longitude = item["Longitude"];
                        Vehicle = item["VehicleNo"];
                    }
                    //Latitude = dynObj.Latitude.Value;
                    //Longitude = dynObj.Longitude.Value;
                    //Vehicle = dynObj.VehicleNo.Value;
                }
                else if (SetUrl.Contains("elixiatech"))
                {

                    Latitude = dynObj.Result.data[0]["lat"];
                    Longitude = dynObj.Result.data[0]["lng"];
                    Vehicle = dynObj.Result.data[0]["vehicleno"];
                }
            }


            var jsonResult = Json(new
            {
                Status = Status,
                Latitude = Status == "Error" ? "" : Latitude,
                Longitude = Status == "Error" ? "" : Longitude,
                Vehicle = Status == "Error" ? "" : Vehicle,
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public FreightMemoQueryVM TrackingReq(FreightMemoQueryVM Model)
        {
            bool TrackReq = false;
            string Msg = "Tracking Not Avalable...!";
            var CurrDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());

            TfatVehicleTrackingSetup vehicleTrackingSetup = ctxTFAT.TfatVehicleTrackingSetup.FirstOrDefault();

            if (vehicleTrackingSetup != null)
            {
                if (vehicleTrackingSetup.AllTime)
                {
                    TrackReq = true;
                }
                else if (vehicleTrackingSetup.UptoDaysReq)
                {
                    var Docdate = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == Model.FreightMemoKey.Trim()).Select(x => x.Date).FirstOrDefault();
                    Docdate = Docdate.AddDays(vehicleTrackingSetup.UptoDays);
                    if (CurrDate <= Docdate)
                    {
                        TrackReq = true;
                    }
                    else
                    {
                        Msg = "This Vehicle Tacking Allow Upto " + Docdate.ToShortDateString() + " . We Can Not Processed To Tracking Of This Vehicle...!";
                    }
                }
                else if (vehicleTrackingSetup.OnlySchedule)
                {
                    var ScheduleDate = ctxTFAT.FMROUTETable.Where(x => x.FmNo.ToString() == Model.FMno.Trim()).OrderByDescending(x => x.RECORDKEY).Select(x => x.DispatchReSchDate).FirstOrDefault();
                    if (ScheduleDate != null)
                    {
                        if (CurrDate <= ScheduleDate.Value)
                        {
                            TrackReq = true;
                        }
                        else
                        {
                            Msg = "This Vehicle Tacking Allow Upto " + ScheduleDate.Value.ToShortDateString() + " . We Can Not Processed To Tracking Of This Vehicle...!";
                        }
                    }
                    else
                    {
                        Msg = "Vehicle Tracking Allow Only Schedule Based.\n This Vehicle Schedule Missing So We Cant Processed...!";
                    }

                }
                else if (vehicleTrackingSetup.ScheduleAndUptoDaysReq)
                {
                    var ScheduleDate = ctxTFAT.FMROUTETable.Where(x => x.FmNo.ToString() == Model.FMno.Trim()).OrderByDescending(x => x.RECORDKEY).Select(x => x.DispatchReSchDate).FirstOrDefault();
                    if (ScheduleDate != null)
                    {
                        ScheduleDate = ScheduleDate.Value.AddDays(vehicleTrackingSetup.ScheduleAndUptoDays);
                        if (CurrDate <= ScheduleDate.Value)
                        {
                            TrackReq = true;
                        }
                        else
                        {
                            Msg = "This Vehicle Tacking Allow Upto " + ScheduleDate.Value.ToShortDateString() + " . We Can Not Processed To Tracking Of This Vehicle...!";
                        }
                    }
                    else
                    {
                        Msg = "Vehicle Tracking Allow Only Schedule Based./n This Vehicle Schedule Missing So We Cant Processed...!";
                    }
                }
            }

            Model.TrackButtonReq = TrackReq;
            Model.TrackErrorMsg = Msg;

            var Vehicle = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == Model.FreightMemoKey).Select(x => x.TruckNo).FirstOrDefault();
            var GetVehicleTrackId = ctxTFAT.TfatVehicleTrackApiList.Where(x => x.VehicleList.Contains(Vehicle)).FirstOrDefault();

            //var GetVehicleTrackId1 = ctxTFAT.TfatVehicleTrackApiList.Where(x => Vehicle.Contains(x.VehicleList)).FirstOrDefault();

            if (GetVehicleTrackId == null)
            {
                Model.TrackButtonReq = false;
                Model.TrackErrorMsg = "This Vehicle Not Fount To Any VehicleTracking List.\nPlease Check Tracking Details In Company Profile...!";
            }
            return Model;
        }



    }
}