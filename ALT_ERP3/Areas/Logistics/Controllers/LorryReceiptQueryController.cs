using ALT_ERP3.Areas.Logistics.Models;
using ALT_ERP3.Controllers;
using Common;
using CrystalDecisions.CrystalReports.Engine;
using EntitiModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Logistics.Controllers
{
    public class LorryReceiptQueryController : BaseController
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

        public JsonResult GetLRNo(string term)
        {
            var list = ctxTFAT.LRMaster.ToList().Take(10);

            if (!(String.IsNullOrEmpty(term)))
            {
                list = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString().ToLower().Contains(term.ToLower())).ToList().Take(10);
            }

            var Modified = list.Select(x => new
            {
                Code = x.LrNo,
                Name = x.LrNo
            });
            return Json(Modified, JsonRequestBehavior.AllowGet);
        }


        // GET: Logistics/LorryReceiptQuery
        public ActionResult Index(LorryReceiptQueryVM Model)
        {
            UpdateAuditTrail(mbranchcode, "Report", Model.Header, "", DateTime.Now, 0, "", "", "NA");
            if (Model.Document == "" || Model.Document == null)
            {
                GetAllMenu(Session["ModuleName"].ToString());
            }

            if (Model.Shortcut)
            {
                Model.Lrno = ctxTFAT.LRMaster.Where(x => x.TableKey == Model.ConsignmentKey.Trim()).Select(x => x.LrNo.ToString()).FirstOrDefault();
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
        public ActionResult FetchDocumentList(LorryReceiptQueryVM Model)
        {
            List<LorryReceiptQueryVM> ValueList = new List<LorryReceiptQueryVM>();

            var mlrmaster = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == Model.Lrno).Select(x => x).ToList();
            if (mlrmaster == null)
            {
                Model.Status = "ValidError";
                Model.Message = "Consignment Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            if (mlrmaster.Count() == 0)
            {
                Model.Status = "ValidError";
                Model.Message = "Consignment Not Found..";
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            else if (mlrmaster.Count() == 1)
            {
                Model.Status = "Processed";
                Model.Message = mlrmaster.Select(x => x.TableKey).FirstOrDefault();
                return Json(Model, JsonRequestBehavior.AllowGet);
            }
            foreach (var item in mlrmaster)
            {
                LorryReceiptQueryVM otherTransact = new LorryReceiptQueryVM();
                otherTransact.ConsignmentKey = item.TableKey.ToString();
                otherTransact.Lrno = item.LrNo.ToString();
                otherTransact.LrDate = item.BookDate;
                otherTransact.LrQty = item.TotQty.ToString();
                otherTransact.LrActWt = item.ActWt.ToString("0.00");
                otherTransact.LrChrgWt = item.ChgWt.ToString("0.00");
                otherTransact.LRBranch = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault();
                otherTransact.LrFrom = ctxTFAT.TfatBranch.Where(x => x.Code == item.Source).Select(x => x.Name).FirstOrDefault();
                otherTransact.LrTo = ctxTFAT.TfatBranch.Where(x => x.Code == item.Dest).Select(x => x.Name).FirstOrDefault();
                otherTransact.LrConsignor = ctxTFAT.Consigner.Where(x => x.Code == item.RecCode).Select(x => x.Name).FirstOrDefault();
                otherTransact.LrConsignee = ctxTFAT.Consigner.Where(x => x.Code == item.SendCode).Select(x => x.Name).FirstOrDefault();
                ValueList.Add(otherTransact);
            }
            Model.ValueList = ValueList;
            var html = ViewHelper.RenderPartialView(this, "ConsignmentList", Model);
            return Json(new { Status = "Success", Html = html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetReport(LorryReceiptQueryVM Model)
        {
            int docdetail = 0, docdetail1 = 0;
            bool CurrentBranch = false;
            if (muserid == "Super")
            {
                Model.CD = true;
                Model.CS = true;
                Model.DS = true;
                Model.DD = true;
                Model.PD = true;
                Model.BD = true;
                Model.CSD = true;
                Model.ED = true;
            }
            else
            {
                var FmQueryAloow = ctxTFAT.TfatPass.Where(x => x.Code == muserid).Select(x => new { x.ConsignQryBranch, x.ConsignQryPanel }).FirstOrDefault();
                if (String.IsNullOrEmpty(FmQueryAloow.ConsignQryBranch))
                {
                    CurrentBranch = true;
                }
                else
                {
                    if (FmQueryAloow.ConsignQryBranch == "TR")
                    {
                        CurrentBranch = true;
                    }
                    else
                    {
                        CurrentBranch = false;
                    }
                }

                if (String.IsNullOrEmpty(FmQueryAloow.ConsignQryPanel))
                {
                    Model.CD = false;
                    Model.CS = false;
                    Model.DS = false;
                    Model.DD = false;
                    Model.PD = false;
                    Model.BD = false;
                    Model.CSD = false;
                    Model.ED = false;
                }
                else
                {
                    var List = FmQueryAloow.ConsignQryPanel.Split('^').ToList();
                    if (List.Contains("CD"))
                    {
                        Model.CD = true;
                    }
                    if (List.Contains("CS"))
                    {
                        Model.CS = true;
                    }
                    if (List.Contains("DS"))
                    {
                        Model.DS = true;
                    }
                    if (List.Contains("DD"))
                    {
                        Model.DD = true;
                    }
                    if (List.Contains("PD"))
                    {
                        Model.PD = true;
                    }
                    if (List.Contains("BD"))
                    {
                        Model.BD = true;
                    }
                    if (List.Contains("CSD"))
                    {
                        Model.CSD = true;
                    }
                    if (List.Contains("ED"))
                    {
                        Model.ED = true;
                    }

                }
            }

            Session["TempAttach"] = null;
            Session["CommnNarrlist"] = null;
            string Status = "Success";

            string Query = "select Lrno,(select T.Name From TfatBranch T where T.code= Branch) as LRBranch,BookDate as LrDate,(CONVERT(VARCHAR(15),CAST(Time AS TIME),100) ) as LrTime,CreateDate as LrCreateDate,ENTEREDBY as LrEnterdby,LASTUPDATEDATE as LRLastUpdateDate,(select T.Name From TfatBranch T where T.code=Source) as LrFrom,"
                           + " (select T.Name From TfatBranch T where T.code=dest) as LrTo,colln as LrCollection,Delivery as LrDelivery,LRMode as LrMode,(select S.ServiceType from ServiceTypeMaster S where S.code= L.ServiceType) as LrServiceType,(select C.Name from Consigner C where C.code= RecCode) as LrConsignor,(select C.Name from Consigner C where C.code=SendCode) as LrConsignee, (Select LL.LRType from LRTypeMaster LL where LL.Code=   L.LrType) as LrType,"
                           + " (Select H.Name From CustomerMaster H where H.Code= BillParty) as LrBillParty,(select T.Name From TfatBranch T where T.code=BillBran) as LrBillBranch,totQty as LrQty,(select U.Name from unitmaster U where U.Code= UnitCode) as LrUnit,ActWt as LrActWt,ChgWT as LrChrgWt, (select J.ChargeType From ChargeTypeMaster J where J.code= ChgType) as LrChargeType,(Select U.Description From DescriptionMaster U where U.Code= DescrType) as LrDescription,PartyRef as LrPartyChallan,PartyInvoice as LrPartyInvoice,PONumber as LrPONumber,BENumber as LrBENumber,"
                           + "DecVal as LrDeclareValue,GSTNO as LrGST,EwayBill as LrEwayBill,Narr as LrRemark,isnull( (select V.TruckNo from Vehiclemaster V where V.code=L.VehicleNo),(select H.TruckNo from HireVehiclemaster H where H.code=L.VehicleNo)) as VehicleNo"
                           + " from lrmaster L where Tablekey = '" + Model.ConsignmentKey + "'";
            List<DataRow> ordersstk = GetDataTable(Query).AsEnumerable().ToList();
            if (ordersstk.Count() > 0)
            {
                if (CurrentBranch)
                {
                    LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == Model.ConsignmentKey).FirstOrDefault();
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
                    if (items.Contains(lRMaster.Source) || items.Contains(lRMaster.Dest))
                    {
                        Model = LorryReceiptDetails(ordersstk, Model);
                        //Model = TrackingReq(Model);
                    }
                    else
                    {
                        Model.NotShowDetails = true;
                    }
                }
                else
                {
                    Model = LorryReceiptDetails(ordersstk, Model);
                    //Model = TrackingReq(Model);
                }


                if (Model.NotShowDetails == false)
                {

                    Query = "select LRSTK.FMRefTablekey, isnull( (select V.TruckNo from vehiclemaster V where V.code=LRSTK.StockAt),(select H.TruckNo from Hirevehiclemaster H where H.code=LRSTK.StockAt)) as VehicleNO, ( select Name From TfatBranch where Code=LRSTK.Branch) As Branch, LRSTK.Fmno , LRSTK.type As StkType,"
                        + " case when LRSTK.TotalQty = 0 then 0 else (LRSTK.TotalQty - (((select  ISNULL(SUM(LRT.TotalQty), 0) from LRStock LRT where LRT.ParentKey = LRSTK.TableKey)) +(select ISNULL(SUM(lcd.lrqty), 0) from lcdetail lcd where lcd.Parentkey=LRSTK.Tablekey and lcd.LCRefTablekey in (select LCM.Tablekey from lcmaster LCM where LCm.DispachFM = 0))  ) ) end as Qty"
                        + " from LRStock LRSTK where LRSTK.LRRefTablekey = '" + Model.ConsignmentKey + "' and LRSTK.type <> 'DEL' and(case when LRSTK.TotalQty = 0 then 0 else (LRSTK.TotalQty - (((select  ISNULL(SUM(LRT.TotalQty), 0) from LRStock LRT where LRT.ParentKey = LRSTK.TableKey)) +(select ISNULL(SUM(lcd.lrqty), 0) from lcdetail lcd where lcd.Parentkey=LRSTK.Tablekey and lcd.LCRefTablekey in (select LCM.Tablekey from lcmaster LCM where LCm.DispachFM = 0))  ) ) end ) <> 0 ";
                    ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                    Model.StockDetails = LorryReceiptStockDetails(ordersstk);

                    Query = "select LC.lcno as LCno,(select T.Name From TfatBranch T where T.code=LC.Branch )as LCBranch,(select T.Name From TfatBranch T where T.code=LC.FromBranch) as LCFrom,(select T.Name From TfatBranch T where T.code=LC.ToBranch) as LCTo,LCD.LrQty as LCQty,LC.Date as LCDate,(CONVERT(VARCHAR(15),CAST(LC.Time AS TIME),100) )  as LCTime,LC.CreateDate as LCCreateDate,LC.ENTEREDBY as LCEnteredBy,LC.LASTUPDATEDATE as LCLastUpdateDate,LC.Remark as LCRemark,LC.DispachFM as FMno,"
                            + " FM.Date as FMDate,(select T.Name From Drivermaster T where T.code=FM.Driver) as FMDriver,(CONVERT(VARCHAR(15),CAST(FM.Time AS TIME),100) ) as FMTime,(select T.Name From TfatBranch T where T.code=FM.Branch) as FMBranch,(select T.Name From TfatBranch T where T.code=FM.FromBranch) as FMFrom,(select T.Name From TfatBranch T where T.code=FM.ToBranch) as FMTo,FM.RouteViaName as FMVia,FM.CreateDate as FMCreateDate,FM.ENTEREDBY as FMEnterdBy,FM.LASTUPDATEDATE as FMLastUpdateDate,(select V.VehicleGroupStatus from VehicleGrpStatusMas V where V.code = FM.VehicleStatus ) as FMType, isnull( (select V.TruckNo from vehiclemaster V where V.code=FM.TruckNo),(select H.TruckNo from Hirevehiclemaster H where H.code=FM.TruckNo)) as FMVehicleNO,(select M.Name From Master M where M.code= FM.BroCode) as FMBroker,Cast(FM.Freight as Decimal(14,2)) as FMFreight,Cast(FM.Adv as Decimal(14,2)) as FMAdvance,Cast(FM.Balance as Decimal(14,2)) as FMBalance,(select T.Name From TfatBranch T where T.code=FM.PayAt) as FMPaybleAt,FM.Remark as FMRemark"
                            + " from LCDetail LCD left join LCMaster LC on LC.Tablekey = LCD.LCRefTablekey left join FMMaster FM on FM.Tablekey = LC.FMRefTablekey where LCD.LRRefTablekey = '" + Model.ConsignmentKey + "' order by LC.Date";
                    ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                    Model.DispatchDetails = DispatchDetails(ordersstk);

                    Query = " with temp as (select (ROW_NUMBER()over(partition by LC.Recordkey order by LC.Recordkey) )as Rn,LC.CreateOn as LodeDate,(select T.Name From TfatBranch T where T.code=LC.Branch) as LoadingIn,(select T.Name From TfatBranch T where T.code=S.Branch) as DirectLoadFor,LC.Qty as LoadQty,LC.EnteredBy as LodedBy, " +
                            "FM.FmNo as FMno, FM.Date as FMDate,(select T.Name From Drivermaster T where T.code = FM.Driver) as FMDriver, " +
                            "(CONVERT(VARCHAR(15), CAST(FM.Time AS TIME), 100)) as FMTime,(select T.Name From TfatBranch T where T.code = FM.Branch) as FMBranch," +
                            "(select T.Name From TfatBranch T where T.code = FM.FromBranch) as FMFrom,(select T.Name From TfatBranch T where T.code = FM.ToBranch) as FMTo," +
                            "FM.RouteViaName as FMVia,FM.CreateDate as FMCreateDate,FM.ENTEREDBY as FMEnterdBy,FM.LASTUPDATEDATE as FMLastUpdateDate," +
                            "(select V.VehicleGroupStatus from VehicleGrpStatusMas V where V.code = FM.VehicleStatus ) as FMType, " +
                            "isnull((select V.TruckNo from vehiclemaster V where V.code = FM.TruckNo),(select H.TruckNo from Hirevehiclemaster H where H.code = FM.TruckNo)) as FMVehicleNO," +
                            "(select M.Name From Master M where M.code = FM.BroCode) as FMBroker,Cast(FM.Freight as Decimal(14, 2)) as FMFreight," +
                            "Cast(FM.Adv as Decimal(14, 2)) as FMAdvance,Cast(FM.Balance as Decimal(14, 2)) as FMBalance," +
                            "(select T.Name From TfatBranch T where T.code = FM.PayAt) as FMPaybleAt,FM.Remark as FMRemark " +
                            "from LRStock S join FMMaster FM on S.FMRefTablekey = FM.Tablekey " +
                            "join LoadingConsignment LC on S.ParentKey = LC.StockKey where S.lcno = 0 and S.type = 'TRN' and S.LRRefTablekey = '" + Model.ConsignmentKey + "'" +
                            " ) select* from temp where rn = 1  ";
                    ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                    Model.DirectLoadingDetails = DirectLoadingDetails(ordersstk);



                    Query = "select DEL.ShortQty, DEL.DeliveryNo as Delno,(select T.Name From TfatBranch T where T.code=DEL.Branch) as DelBranch,DEL.DeliveryDate as DelDate,(CONVERT(VARCHAR(15),CAST(DEL.DeliveryTime AS TIME),100) ) as DelTime,DEL.CreateDate as DelCreateDate,DEL.ENTEREDBY as DelEnteredBy,DEL.LastUpdateDate as DelLastUpdateDate,DR.DelQty as DelQty,DEL.DeliveryGoodStatus as DelStatus,DEL.DeliveryRemark as DelRemark"
                            + " from DeliveryMaster DEL left join DelRelation DR on DR.DeliveryNo = DEL.DeliveryNo and DR.Prefix = DEL.Prefix where DEL.Parentkey = '" + Model.ConsignmentKey + "' order by DEL.DeliveryDate";
                    ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                    Model.DeliveryDetails = DeliveryDetails(ordersstk);

                    Query = "select POD.PODNO as PODno,POD.PODDate as PODDate,(CONVERT(VARCHAR(15),CAST(POD.PODTime AS TIME),100) ) as PODTime,POD.CreateDate as PODCreateDate,POD.ModuleName as PODMOduleName,POD.Task as PODTask,(select T.Name From TfatBranch T where T.code=POD.CurrentBranch) as PODBranch,POD.ENTEREDBY as PODEnterdeby,POD.LASTUPDATEDATE as PODLastUpdateDate,isnull(PODR.RecePODRemark, POD.PODRemark) as PODRemark,(select T.Name From TfatBranch T where T.code=PODR.FromBranch) as PODReceiverdFromBranch,(select T.Name From TfatBranch T where T.code=PODR.ToBranch) as PODSendToBranch,PODR.SendReceive as PODSendParticular"
                            + " from PODMaster POD left join PODRel PODR on PODR.Parentkey = POD.Tablekey    where PODR.LRRefTablekey = '" + Model.ConsignmentKey + "'"
                            + " order by POD.PODDate";
                    ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                    Model.PODDetails = PODDetails(ordersstk);

                    Query = "select S.Srl as BillNo,(select M.Name From CustomerMaster M where M.code=S.code) as BillParty,S.BillDate as BillNoDate,S.DocDate as BillNoCreateDate,(select T.Name From TfatBranch T where T.code=S.Branch) as BillNoBranch,S.ENTEREDBY as BillNoEnteredBy,S.LASTUPDATEDATE as BillNoLastUpdateDate,S.Narr as BillNoRemark,Cast(LRB.Amt as Decimal(14,2)) as BillTotal,S.TableKey as BillTablekey"
                            + " from Sales S left join LRBill LRB on LRB.ParentKey + LRB.Branch = S.TableKey + S.Branch where LRB.LRRefTablekey = '" + Model.ConsignmentKey + "' and S.Type = 'SLR00'"
                            + " order by S.BillDate";
                    ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                    Model.BillDetails = BillDetails(ordersstk, Model.Lrno);

                    Query = "select S.Srl as BillNo,(select M.Name From Master M where M.code=S.CashBankCode) as BillParty,S.BillDate as BillNoDate,S.DocDate as BillNoCreateDate,(select T.Name From TfatBranch T where T.code=S.Branch) as BillNoBranch,S.ENTEREDBY as BillNoEnteredBy,S.LASTUPDATEDATE as BillNoLastUpdateDate,S.Narr as BillNoRemark,Cast(LRB.Amt as Decimal(14,2)) as BillTotal,S.TableKey as BillTablekey"
                            + " from Sales S left join LRBill LRB on LRB.ParentKey + LRB.Branch = S.TableKey + S.Branch where LRB.LRRefTablekey = '" + Model.ConsignmentKey + "' and S.Type = 'CMM00'"
                            + " order by S.BillDate";
                    ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                    Model.MemoDetails = BillDetails(ordersstk, Model.Lrno);

                    Query = "select distinct M.Name as VouNOTrnChargeName,R.srl as VouNO,R.DocDate as VouNODate,R.DocDate as VouNOCreateDate,T.Name as VouNOBranch,R.EnteredBy as VouNOEnteredBy,R.LASTUPDATEDATE as VouNOLastUpdateDate,R.Narr as VouNORemark,DOC.Name as VouNOTrnType,R.Type as VouNOTrnTypeCode,case when R.amttype='true' then Cast(LR.LrAmt as Decimal(14,2)) else 0 end as VouNOTrnExpAmt,case when R.amttype='false' then Cast(LR.LrAmt as Decimal(14,2)) else 0 end as VouNOTrnIncAmt"
                            + " from RelateData R left"
                            + " join RelLr LR on LR.TableKey + LR.Branch = R.TableKey + R.Branch left"
                            + " join DocTypes DOC on DOC.Code = R.Type left"
                            + " join Master M on M.code = R.code  left"
                            + " join TfatBranch T on T.Code = R.Branch where LR.LRRefTablekey = '" + Model.ConsignmentKey + "'";
                    ordersstk = GetDataTable(Query).AsEnumerable().ToList();
                    Model.ExpensesDetails = ExpensesDetails(ordersstk);

                    #region Attachment
                    var Parentkey = ctxTFAT.LRMaster.Where(x => x.TableKey == Model.ConsignmentKey).Select(x => x.ParentKey).FirstOrDefault();
                    docdetail = ctxTFAT.Attachment.Where(x => x.ParentKey == Model.ConsignmentKey && (x.Type == "LR000") && x.Type != "Alert").ToList().Count();
                    docdetail1 = ctxTFAT.AlertNoteMaster.Where(x => x.ParentKey == Model.ConsignmentKey && x.Type == "LR000").ToList().Count();

                    #endregion
                }
            }
            else
            {
                Status = "Error";
            }

            var html = ViewHelper.RenderPartialView(this, "_ReportDetails", Model);
            var jsonResult = Json(new
            {
                Status = Status,
                Html = html,
                AttachC = docdetail,
                AlertC = docdetail1,
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public LorryReceiptQueryVM LorryReceiptDetails(List<DataRow> ordersstk, LorryReceiptQueryVM objitemlist)
        {
            foreach (var item in ordersstk)
            {
                objitemlist.Lrno = item["Lrno"].ToString();
                objitemlist.LRBranch = item["LRBranch"].ToString().Trim();
                objitemlist.LrDate = ConvertDDMMYYTOYYMMDD(item["LrDate"].ToString());
                objitemlist.LrTime = item["LrTime"].ToString();
                objitemlist.LrCreateDate = ConvertDDMMYYTOYYMMDD(item["LrCreateDate"].ToString());
                objitemlist.LrEnterdby = item["LrEnterdby"].ToString();
                objitemlist.LRLastUpdateDate = ConvertDDMMYYTOYYMMDD(item["LRLastUpdateDate"].ToString());
                objitemlist.LrFrom = item["LrFrom"].ToString().Trim();
                objitemlist.LrTo = item["LrTo"].ToString().Trim();
                objitemlist.LrCollection = item["LrCollection"].ToString() == "D" ? "Direct" : "Godown";
                objitemlist.LrDelivery = item["LrDelivery"].ToString() == "D" ? "Door" : "Godown";
                objitemlist.LrMode = item["LrMode"].ToString().Trim() == "G" ? "Regular" : item["LrMode"].ToString().Trim() == "R" ? "Express" : "Normal";
                objitemlist.LrServiceType = item["LrServiceType"].ToString().Trim();
                objitemlist.LrConsignor = item["LrConsignor"].ToString().Trim();
                objitemlist.LrConsignee = item["LrConsignee"].ToString().Trim();
                objitemlist.LrType = item["LrType"].ToString().Trim();
                objitemlist.LrBillParty = item["LrBillParty"].ToString().Trim();
                objitemlist.LrBillBranch = item["LrBillBranch"].ToString().Trim();
                objitemlist.LrQty = item["LrQty"].ToString();
                objitemlist.LrUnit = item["LrUnit"].ToString();
                objitemlist.LrActWt = item["LrActWt"].ToString();
                objitemlist.LrChrgWt = item["LrChrgWt"].ToString();
                objitemlist.LrChargeType = item["LrChargeType"].ToString().Trim();
                objitemlist.LrDescription = item["LrDescription"].ToString().Trim();
                objitemlist.LrPartyChallan = item["LrPartyChallan"].ToString();
                objitemlist.LrPartyInvoice = item["LrPartyInvoice"].ToString();
                objitemlist.LrPONumber = item["LrPONumber"].ToString();
                objitemlist.LrBENumber = item["LrBENumber"].ToString();
                objitemlist.LrDeclareValue = item["LrDeclareValue"].ToString();
                objitemlist.LrGST = item["LrGST"].ToString();
                objitemlist.LrEwayBill = item["LrEwayBill"].ToString();
                objitemlist.LrRemark = item["LrRemark"].ToString();
                objitemlist.VehicleNo = item["VehicleNo"].ToString();
            }

            return objitemlist;
        }
        public List<LRRelatedDispatchDetailsVM> DispatchDetails(List<DataRow> ordersstk)
        {
            List<LRRelatedDispatchDetailsVM> objitemlist = new List<LRRelatedDispatchDetailsVM>();
            foreach (var item in ordersstk)
            {
                objitemlist.Add(new LRRelatedDispatchDetailsVM()
                {
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
                    FMno = item["FMno"].ToString(),
                    FMDate = item["FMDate"].ToString() == "" ? DateTime.Now : ConvertDDMMYYTOYYMMDD(item["FMDate"].ToString()),
                    FMTime = item["FMTime"].ToString(),
                    FMBranch = item["FMBranch"].ToString().Trim(),
                    FMFrom = item["FMFrom"].ToString().Trim(),
                    FMTo = item["FMTo"].ToString().Trim(),
                    FMVia = item["FMVia"].ToString(),
                    FMCreateDate = item["FMCreateDate"].ToString() == "" ? DateTime.Now : ConvertDDMMYYTOYYMMDD(item["FMCreateDate"].ToString()),
                    FMEnterdBy = item["FMEnterdBy"].ToString(),
                    FMLastUpdateDate = item["FMLastUpdateDate"].ToString() == "" ? DateTime.Now : ConvertDDMMYYTOYYMMDD(item["FMLastUpdateDate"].ToString()),
                    FMType = item["FMType"].ToString(),
                    FMVehicleNO = item["FMVehicleNO"].ToString(),
                    FMBroker = item["FMBroker"].ToString().Trim(),
                    FMFreight = item["FMFreight"].ToString(),
                    FMAdvance = item["FMAdvance"].ToString(),
                    FMBalance = item["FMBalance"].ToString(),
                    FMPaybleAt = item["FMPaybleAt"].ToString().Trim(),
                    FMRemark = item["FMRemark"].ToString(),
                    FMDriver = item["FMDriver"].ToString(),
                });
            }

            return objitemlist;
        }
        public List<LRRelatedDispatchDetailsVM> DirectLoadingDetails(List<DataRow> ordersstk)
        {
            List<LRRelatedDispatchDetailsVM> objitemlist = new List<LRRelatedDispatchDetailsVM>();
            foreach (var item in ordersstk)
            {
                objitemlist.Add(new LRRelatedDispatchDetailsVM()
                {
                    LCCreateDate = Convert.ToDateTime(item["LodeDate"].ToString()),
                    LCBranch = item["LoadingIn"].ToString().Trim(),
                    LCTo = item["DirectLoadFor"].ToString().Trim(),
                    LCQty = item["LoadQty"].ToString(),
                    LCEnteredBy = item["LodedBy"].ToString(),

                    FMno = item["FMno"].ToString(),
                    FMDate = item["FMDate"].ToString() == "" ? DateTime.Now : ConvertDDMMYYTOYYMMDD(item["FMDate"].ToString()),
                    FMTime = item["FMTime"].ToString(),
                    FMBranch = item["FMBranch"].ToString().Trim(),
                    FMFrom = item["FMFrom"].ToString().Trim(),
                    FMTo = item["FMTo"].ToString().Trim(),
                    FMVia = item["FMVia"].ToString(),
                    FMCreateDate = item["FMCreateDate"].ToString() == "" ? DateTime.Now : ConvertDDMMYYTOYYMMDD(item["FMCreateDate"].ToString()),
                    FMEnterdBy = item["FMEnterdBy"].ToString(),
                    FMLastUpdateDate = item["FMLastUpdateDate"].ToString() == "" ? DateTime.Now : ConvertDDMMYYTOYYMMDD(item["FMLastUpdateDate"].ToString()),
                    FMType = item["FMType"].ToString(),
                    FMVehicleNO = item["FMVehicleNO"].ToString(),
                    FMBroker = item["FMBroker"].ToString().Trim(),
                    FMFreight = item["FMFreight"].ToString(),
                    FMAdvance = item["FMAdvance"].ToString(),
                    FMBalance = item["FMBalance"].ToString(),
                    FMPaybleAt = item["FMPaybleAt"].ToString().Trim(),
                    FMRemark = item["FMRemark"].ToString(),
                    FMDriver = item["FMDriver"].ToString(),
                });
            }

            return objitemlist;
        }
        public List<LRRelatedDeliveryDetailsVM> DeliveryDetails(List<DataRow> ordersstk)
        {
            List<LRRelatedDeliveryDetailsVM> objitemlist = new List<LRRelatedDeliveryDetailsVM>();
            foreach (var item in ordersstk)
            {
                objitemlist.Add(new LRRelatedDeliveryDetailsVM()
                {
                    Delno = item["Delno"].ToString(),
                    ShortQty = item["ShortQty"].ToString(),
                    DelBranch = item["DelBranch"].ToString().Trim(),
                    DelDate = ConvertDDMMYYTOYYMMDD(item["DelDate"].ToString()),
                    DelTime = item["DelTime"].ToString(),
                    DelCreateDate = ConvertDDMMYYTOYYMMDD(item["DelCreateDate"].ToString()),
                    DelEnteredBy = item["DelEnteredBy"].ToString(),
                    DelLastUpdateDate = ConvertDDMMYYTOYYMMDD(item["DelLastUpdateDate"].ToString()),
                    DelQty = item["DelQty"].ToString(),
                    DelStatus = item["DelStatus"].ToString(),
                    DelRemark = item["DelRemark"].ToString(),
                });
            }

            return objitemlist;
        }
        public List<LRRelatedPODDetailsVM> PODDetails(List<DataRow> ordersstk)
        {
            List<LRRelatedPODDetailsVM> objitemlist = new List<LRRelatedPODDetailsVM>();
            foreach (var item in ordersstk)
            {
                objitemlist.Add(new LRRelatedPODDetailsVM()
                {
                    PODno = item["PODno"].ToString(),
                    PODDate = ConvertDDMMYYTOYYMMDD(item["PODDate"].ToString()),
                    PODTime = item["PODTime"].ToString(),
                    PODCreateDate = ConvertDDMMYYTOYYMMDD(item["PODCreateDate"].ToString()),
                    PODMOduleName = item["PODMOduleName"].ToString(),
                    PODTask = item["PODTask"].ToString(),
                    PODBranch = item["PODBranch"].ToString().Trim(),
                    PODSendToBranch = item["PODSendToBranch"].ToString().Trim(),
                    PODReceiverdFromBranch = item["PODReceiverdFromBranch"].ToString().Trim(),
                    PODEnterdeby = item["PODEnterdeby"].ToString(),
                    PODSendParticular = item["PODSendParticular"].ToString(),
                    PODLastUpdateDate = ConvertDDMMYYTOYYMMDD(item["PODLastUpdateDate"].ToString()),
                    PODRemark = item["PODRemark"].ToString(),

                });
            }

            return objitemlist;
        }

        public List<LRRelatedInvoiceDetailsVM> BillDetails(List<DataRow> ordersstk, string LRno)
        {
            List<LRRelatedInvoiceDetailsVM> objitemlist = new List<LRRelatedInvoiceDetailsVM>();
            foreach (var item in ordersstk)
            {
                objitemlist.Add(new LRRelatedInvoiceDetailsVM()
                {
                    BillNo = item["BillNo"].ToString(),
                    BillNoDate = ConvertDDMMYYTOYYMMDD(item["BillNoDate"].ToString()),
                    BillNoCreateDate = ConvertDDMMYYTOYYMMDD(item["BillNoCreateDate"].ToString()),
                    BillNoBranch = item["BillNoBranch"].ToString().Trim(),
                    BillNoEnteredBy = item["BillNoEnteredBy"].ToString(),
                    BillNoLastUpdateDate = ConvertDDMMYYTOYYMMDD(item["BillNoLastUpdateDate"].ToString()),
                    BillNoRemark = item["BillNoRemark"].ToString(),
                    BillTotal = item["BillTotal"].ToString(),
                    BillTablekey = item["BillTablekey"].ToString(),
                    BillParty = item["BillParty"].ToString().Trim(),
                    Charges = GetLRCharges(item["BillTablekey"].ToString(), LRno),
                });
            }

            return objitemlist;
        }
        public List<LRInvoiceVM> GetLRCharges(string TableKey, string LRNO)
        {
            List<LRInvoiceVM> objledgerdetail = new List<LRInvoiceVM>();
            var trncharges = ctxTFAT.Charges.Where(x => x.Type == "LR000").Select(x => new { x.Fld, x.Head, x.EqAmt, x.Equation, x.Code }).ToList();
            foreach (var i in trncharges)
            {
                LRInvoiceVM c = new LRInvoiceVM();
                c.Fld = i.Fld;
                c.Header = i.Head;
                c.AddLess = i.EqAmt;
                c.ChgPostCode = i.Code;
                c.tempId = Convert.ToInt16(i.Fld.Substring(1, 3));
                c.Amt = GetLrWiseChargeValValue(c.tempId, TableKey, LRNO);

                if (c.Amt > 0)
                {
                    objledgerdetail.Add(c);
                }
            }
            return objledgerdetail;
        }
        public decimal GetLrWiseChargeValValue(int i, string TableKey, string LRno)
        {
            string connstring = GetConnectionString();
            decimal abc;

            var loginQuery3 = @"select Val" + i + " from LRBill where LRno=" + LRno + " and  Parentkey = '" + TableKey + "'";
            DataTable mDt2 = GetDataTable(loginQuery3, connstring);
            if (mDt2.Rows.Count > 0)
            {
                abc = (mDt2.Rows[0][0].ToString() == "" || mDt2.Rows[0][0].ToString() == null) ? 0 : Convert.ToDecimal(mDt2.Rows[0][0].ToString());
            }
            else
            {
                abc = 0;
            }

            return decimal.Round(abc, 2, MidpointRounding.AwayFromZero);
        }

        public List<LRRelatedExpensesDetailsVM> ExpensesDetails(List<DataRow> ordersstk)
        {
            List<LRRelatedExpensesDetailsVM> objitemlist = new List<LRRelatedExpensesDetailsVM>();
            foreach (var item in ordersstk)
            {
                objitemlist.Add(new LRRelatedExpensesDetailsVM()
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
        public List<LRRelatedStockDetailsVM> LorryReceiptStockDetails(List<DataRow> ordersstk)
        {
            List<LRRelatedStockDetailsVM> objitemlist = new List<LRRelatedStockDetailsVM>();
            foreach (var item in ordersstk)
            {
                objitemlist.Add(new LRRelatedStockDetailsVM()
                {
                    StkBranch = item["Branch"].ToString(),
                    Tablekey = item["FMRefTablekey"].ToString(),
                    FMNO = (item["Fmno"].ToString()),
                    Type = (item["StkType"].ToString()),
                    Qty = (item["Qty"].ToString()),
                    VehicleNo = (item["VehicleNO"].ToString()),
                });
            }

            return objitemlist;
        }


        #region Vehicle Tracking Details

        public ActionResult TrackID(LorryReceiptQueryVM Model)
        {
            var SetUrl = "";
            string Status = "Sucess", VehicleNO = "";
            string Latitude = "", Longitude = "", Vehicle = "";
            bool TrackReq = false;
            string Msg = "Tracking Not Avalable...!";
            var CurrDate = ConvertDDMMYYTOYYMMDD(DateTime.Now.ToShortDateString());

            FMMaster fM_ROUTE_ = ctxTFAT.FMMaster.Where(x => x.TableKey.ToString() == Model.Document).FirstOrDefault();
            if (fM_ROUTE_ != null)
            {
                TfatVehicleTrackingSetup vehicleTrackingSetup = ctxTFAT.TfatVehicleTrackingSetup.FirstOrDefault();
                if (vehicleTrackingSetup != null)
                {
                    if (vehicleTrackingSetup.CT_AllTime)
                    {
                        TrackReq = true;
                    }
                    else if (vehicleTrackingSetup.CT_UptoDaysReq)
                    {
                        var Docdate = ctxTFAT.LRMaster.Where(x => x.TableKey.ToString() == Model.ConsignmentKey.ToString().Trim()).Select(x => x.BookDate).FirstOrDefault();
                        Docdate = Docdate.AddDays(vehicleTrackingSetup.CT_UptoDays);
                        if (CurrDate <= Docdate)
                        {
                            TrackReq = true;
                        }
                        else
                        {
                            Msg = "This Vehicle Tacking Allow Upto " + Docdate.ToShortDateString() + " . We Can Not Processed To Tracking Of This Vehicle...!";
                        }
                    }
                    else if (vehicleTrackingSetup.CT_DeliveryReq)
                    {
                        var FMmaster = ctxTFAT.DeliveryMaster.Where(x => x.ParentKey.ToString() == Model.ConsignmentKey.ToString().Trim()).FirstOrDefault();
                        if (FMmaster != null)
                        {
                            Msg = "This Consignment Delivery Done. We Can Not Processed To Tracking Of This Vehicle...!";
                        }
                        else
                        {
                            TrackReq = true;
                        }

                    }
                }

                var GetVehicleTrackId = ctxTFAT.TfatVehicleTrackApiList.Where(x => x.VehicleList.Contains(fM_ROUTE_.TruckNo)).FirstOrDefault();
                if (GetVehicleTrackId == null)
                {
                    TrackReq = false;
                    Msg = "This Vehicle Not Fount To Any VehicleTracking List.\nPlease Check Tracking Details In Company Profile...!";
                }
                else
                {
                    if (!String.IsNullOrEmpty(GetVehicleTrackId.TrackApi))
                    {
                        SetUrl += GetVehicleTrackId.TrackApi;
                    }
                    if (!String.IsNullOrEmpty(GetVehicleTrackId.Username))
                    {
                        SetUrl += GetVehicleTrackId.Username;
                    }
                    if (!String.IsNullOrEmpty(GetVehicleTrackId.Password))
                    {
                        SetUrl += GetVehicleTrackId.Password;
                    }
                    if (!String.IsNullOrEmpty(GetVehicleTrackId.Para1))
                    {
                        SetUrl += GetVehicleTrackId.Para1;
                    }
                    if (!String.IsNullOrEmpty(GetVehicleTrackId.Para2))
                    {
                        SetUrl += GetVehicleTrackId.Para2;
                    }
                    if (!String.IsNullOrEmpty(GetVehicleTrackId.Para3))
                    {
                        SetUrl += GetVehicleTrackId.Para3;
                    }
                    if (!String.IsNullOrEmpty(GetVehicleTrackId.Para4))
                    {
                        SetUrl += GetVehicleTrackId.Para4;
                    }
                    if (!String.IsNullOrEmpty(GetVehicleTrackId.Para5))
                    {
                        SetUrl += GetVehicleTrackId.Para5;
                    }
                    if (fM_ROUTE_.TruckNo.Contains("H"))
                    {
                        SetUrl = SetUrl.Replace("@vehiclePara", ctxTFAT.HireVehicleMaster.Where(x => x.Code == fM_ROUTE_.TruckNo).Select(x => x.TruckNo).FirstOrDefault().Replace(" ", ""));
                    }
                    else
                    {
                        SetUrl = SetUrl.Replace("@vehiclePara", ctxTFAT.VehicleMaster.Where(x => x.Code == fM_ROUTE_.TruckNo).Select(x => x.TruckNo).FirstOrDefault().Replace(" ", ""));
                    }
                }

                //if (fM_ROUTE_.VehicleNo.Contains("H"))
                //{
                //    VehicleNO = ctxTFAT.HireVehicleMaster.Where(x => x.Code == fM_ROUTE_.VehicleNo).Select(x => x.TruckNo).FirstOrDefault().Replace(" ", "");
                //}
                //else
                //{
                //    VehicleNO = ctxTFAT.VehicleMaster.Where(x => x.Code == fM_ROUTE_.VehicleNo).Select(x => x.TruckNo).FirstOrDefault().Replace(" ", "");
                //}
            }

            if (TrackReq)
            {
                var GenerateUrl = "http://api.ilogistek.com:6049/tracking?AuthKey=PV7q60SurjHlacX398hbQ4SiWfYktoL&vehicle_no=" + VehicleNO;
                WebClient client = new WebClient();
                string jsonstring = client.DownloadString(SetUrl);
                dynamic dynObj = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonstring);
                var Check = "";
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


                //Latitude = Status == "Error" ? "" : dynObj.Latitude.Value;
                //Longitude = Status == "Error" ? "" : dynObj.Longitude.Value;

            }

            var jsonResult = Json(new
            {
                TrackReq = TrackReq,
                Msg = Msg,
                Status = Status,
                Latitude = Latitude,
                Longitude = Longitude,
                Vehicle = Status == "Error" ? "" : Vehicle,
            }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        #endregion

        #region Not Required Functions

        public ActionResult GetAllRemarkOfDocument(string LRNO)
        {
            LRMaster lRMaster = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == LRNO).FirstOrDefault();

            List<LoadingToDispatchVM> loadingTos = new List<LoadingToDispatchVM>();

            var FMList = (from FreightMemo in ctxTFAT.LRMaster
                          where FreightMemo.LrNo.ToString().Trim() == LRNO.ToString().Trim() && String.IsNullOrEmpty(FreightMemo.Narr) == false
                          orderby FreightMemo.LrNo
                          select new LoadingToDispatchVM()
                          {
                              FMNO = FreightMemo.LrNo.ToString(),
                              AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Branch).Select(x => x.Name).FirstOrDefault(),
                              NarrStr = FreightMemo.Narr,
                              ENTEREDBY = FreightMemo.ENTEREDBY,
                              AUTHIDS = "N",
                              NarrSno = 0,
                              PayLoadL = "Lorry Receipts",
                          }).ToList();
            loadingTos.AddRange(FMList);
            var GetAlertNote = (from FreightMemo in ctxTFAT.AlertNoteMaster
                                where FreightMemo.TypeCode.ToString().Trim() == LRNO.ToString().Trim() && FreightMemo.Type == "LR000"
                                orderby FreightMemo.DocNo
                                select new LoadingToDispatchVM()
                                {
                                    FMNO = FreightMemo.TypeCode.ToString(),
                                    AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Branch).Select(x => x.Name).FirstOrDefault(),
                                    NarrStr = FreightMemo.Note,
                                    ENTEREDBY = FreightMemo.CreateBy,
                                    AUTHIDS = "N",
                                    NarrSno = 0,
                                    PayLoadL = "Alert Note",
                                }).ToList();
            loadingTos.AddRange(GetAlertNote);
            var GetRemarkDocList = ctxTFAT.Narration.Where(x => x.Srl == LRNO && x.Type == "LR000").ToList();
            foreach (var item in GetRemarkDocList)
            {
                loadingTos.Add(new LoadingToDispatchVM
                {
                    FMNO = item.Srl,
                    AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault(),
                    NarrStr = item.Narr,
                    ENTEREDBY = item.ENTEREDBY,
                    AUTHIDS = item.ENTEREDBY.Trim().ToLower() == muserid.Trim().ToLower() ? "Y" : "N",
                    NarrSno = item.Sno,
                    PayLoadL = item.NarrRich
                });
            }
            var html = ViewHelper.RenderPartialView(this, "_DocumentNarrList", loadingTos);
            return Json(new { Html = html }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveNarration(LoadingToDispatchVM Model)
        {
            string html = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    if (Model.NarrStr != null)
                    {
                        LRMaster fM = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == Model.FMNO).FirstOrDefault();

                        var LastSno = ctxTFAT.Narration.Where(x => x.ParentKey == fM.ParentKey).ToList().Count();
                        ++LastSno;
                        Narration narr = new Narration();
                        narr.Branch = mbranchcode;
                        narr.Narr = Model.NarrStr;
                        narr.NarrRich = Model.Header;
                        narr.Prefix = mperiod;
                        narr.Sno = LastSno;
                        narr.Srl = fM.LrNo.ToString();
                        narr.Type = "LR000";
                        narr.ENTEREDBY = muserid;
                        narr.LASTUPDATEDATE = DateTime.Now;
                        narr.AUTHORISE = mauthorise;
                        narr.AUTHIDS = muserid;
                        narr.LocationCode = 0;
                        narr.TableKey = "LR000" + mperiod.Substring(0, 2) + LastSno.ToString("D3") + fM.LrNo.ToString();
                        narr.CompCode = mcompcode;
                        narr.ParentKey = fM.ParentKey;
                        ctxTFAT.Narration.Add(narr);
                        ctxTFAT.SaveChanges();
                        transaction.Commit();
                        transaction.Dispose();

                        List<LoadingToDispatchVM> loadingTos = new List<LoadingToDispatchVM>();


                        var FMList = (from FreightMemo in ctxTFAT.LRMaster
                                      where FreightMemo.LrNo.ToString().Trim() == fM.LrNo.ToString().Trim() && String.IsNullOrEmpty(FreightMemo.Narr) == false
                                      orderby FreightMemo.LrNo
                                      select new LoadingToDispatchVM()
                                      {
                                          FMNO = FreightMemo.LrNo.ToString(),
                                          AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Branch).Select(x => x.Name).FirstOrDefault(),
                                          NarrStr = FreightMemo.Narr,
                                          ENTEREDBY = FreightMemo.ENTEREDBY,
                                          AUTHIDS = "N",
                                          NarrSno = 0,
                                          PayLoadL = "Lorry Receipts",
                                      }).ToList();
                        loadingTos.AddRange(FMList);
                        var GetAlertNote = (from FreightMemo in ctxTFAT.AlertNoteMaster
                                            where FreightMemo.TypeCode.ToString().Trim() == fM.LrNo.ToString().Trim() && FreightMemo.Type == "LR000"
                                            orderby FreightMemo.DocNo
                                            select new LoadingToDispatchVM()
                                            {
                                                FMNO = FreightMemo.TypeCode.ToString(),
                                                AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Branch).Select(x => x.Name).FirstOrDefault(),
                                                NarrStr = FreightMemo.Note,
                                                ENTEREDBY = FreightMemo.CreateBy,
                                                AUTHIDS = "N",
                                                NarrSno = 0,
                                                PayLoadL = "Alert Note",
                                            }).ToList();
                        loadingTos.AddRange(GetAlertNote);
                        var GetRemarkDocList = ctxTFAT.Narration.Where(x => x.Srl == fM.LrNo.ToString() && x.Type == "LR000").ToList();
                        foreach (var item in GetRemarkDocList)
                        {
                            loadingTos.Add(new LoadingToDispatchVM
                            {
                                FMNO = item.Srl,
                                AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault(),
                                NarrStr = item.Narr,
                                ENTEREDBY = item.ENTEREDBY,
                                NarrSno = item.Sno,
                                PayLoadL = item.NarrRich,
                                AUTHIDS = item.ENTEREDBY.Trim().ToLower() == muserid.Trim().ToLower() ? "Y" : "N",
                            });
                        }




                        html = ViewHelper.RenderPartialView(this, "_DocumentNarrList", loadingTos);
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
            return Json(new { Html = html, Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteNarr(LoadingToDispatchVM mModel)
        {
            string html = "";
            using (var transaction = ctxTFAT.Database.BeginTransaction())
            {
                try
                {
                    Narration narration = ctxTFAT.Narration.Where(x => x.Sno.ToString() == mModel.NarrSno.ToString() && x.Type == "LR000").FirstOrDefault();
                    if (narration != null)
                    {
                        ctxTFAT.Narration.Remove(narration);
                        ctxTFAT.SaveChanges();
                        transaction.Commit();
                        transaction.Dispose();
                    }
                    List<LoadingToDispatchVM> loadingTos = new List<LoadingToDispatchVM>();

                    LRMaster fM_ROUTE_ = ctxTFAT.LRMaster.Where(x => x.LrNo.ToString() == mModel.FMNO).FirstOrDefault();
                    if (fM_ROUTE_ != null)
                    {

                        var FMList = (from FreightMemo in ctxTFAT.LRMaster
                                      where FreightMemo.LrNo.ToString().Trim() == fM_ROUTE_.LrNo.ToString().Trim() && String.IsNullOrEmpty(FreightMemo.Narr) == false
                                      orderby FreightMemo.LrNo
                                      select new LoadingToDispatchVM()
                                      {
                                          FMNO = FreightMemo.LrNo.ToString(),
                                          AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Branch).Select(x => x.Name).FirstOrDefault(),
                                          NarrStr = FreightMemo.Narr,
                                          ENTEREDBY = FreightMemo.ENTEREDBY,
                                          AUTHIDS = "N",
                                          NarrSno = 0,
                                          PayLoadL = "Lorry Receipt",
                                      }).ToList();
                        loadingTos.AddRange(FMList);
                        var GetAlertNote = (from FreightMemo in ctxTFAT.AlertNoteMaster
                                            where FreightMemo.TypeCode.ToString().Trim() == fM_ROUTE_.LrNo.ToString().Trim() && FreightMemo.Type == "LR000"
                                            orderby FreightMemo.DocNo
                                            select new LoadingToDispatchVM()
                                            {
                                                FMNO = FreightMemo.TypeCode.ToString(),
                                                AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == FreightMemo.Branch).Select(x => x.Name).FirstOrDefault(),
                                                NarrStr = FreightMemo.Note,
                                                ENTEREDBY = FreightMemo.CreateBy,
                                                AUTHIDS = "N",
                                                NarrSno = 0,
                                                PayLoadL = "Alert Note",
                                            }).ToList();
                        loadingTos.AddRange(GetAlertNote);
                        var GetRemarkDocList = ctxTFAT.Narration.Where(x => x.Srl == fM_ROUTE_.LrNo.ToString() && x.Type == "LR000").ToList();
                        foreach (var item in GetRemarkDocList)
                        {
                            loadingTos.Add(new LoadingToDispatchVM
                            {
                                FMNO = item.Srl,
                                AppBranch = ctxTFAT.TfatBranch.Where(x => x.Code == item.Branch).Select(x => x.Name).FirstOrDefault(),
                                NarrStr = item.Narr,
                                ENTEREDBY = item.ENTEREDBY,
                                NarrSno = item.Sno,
                                PayLoadL = item.NarrRich,
                                AUTHIDS = item.ENTEREDBY.Trim().ToLower() == muserid.Trim().ToLower() ? "Y" : "N",
                            });
                        }
                    }

                    html = ViewHelper.RenderPartialView(this, "_DocumentNarrList", loadingTos);
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
            return Json(new { Html = html, Status = "Success", id = "StateMaster" }, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}